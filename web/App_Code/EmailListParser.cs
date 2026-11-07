using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Toplu bildirim: serbest metinden e-posta adreslerini cikarir ve ayirir.
// Virgul / noktali virgul / satir, "Ad Soyad <ad@firma.com>", Excel sutunu ya da duz metin icindeki adresler.
// Ekrandaki analiz (BroadcastEmails.ashx) ve gonderim (Broadcast.aspx.cs) ayni kodu kullanir.
public class EmailListResult
{
    public List<Dictionary<string, object>> Valid = new List<Dictionary<string, object>>();     // gonderilecek
    public List<Dictionary<string, object>> Covered = new List<Dictionary<string, object>>();   // secili firmada zaten alici
    public List<Dictionary<string, object>> Typos = new List<Dictionary<string, object>>();     // yazim hatasi olabilir (gonderilmez)
    public List<Dictionary<string, object>> Invalid = new List<Dictionary<string, object>>();   // bicimi bozuk
    public List<Dictionary<string, object>> Blocked = new List<Dictionary<string, object>>();   // noreply vb.
    public List<Dictionary<string, object>> Duplicates = new List<Dictionary<string, object>>();
    public List<Dictionary<string, object>> Domains = new List<Dictionary<string, object>>();
    public List<string> SendList = new List<string>();
    public bool Truncated;
}

public static class EmailListParser
{
    public const int MaxAddresses = 2000;
    private static readonly Regex Token = new Regex(@"[^\s,;<>""'()\[\]{}|]*@[^\s,;<>""'()\[\]{}|]*", RegexOptions.Compiled);   // "@eksik.com" da yakalanir
    private static readonly Regex Strict = new Regex(@"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,24}$", RegexOptions.Compiled);
    private static readonly Dictionary<string, string> TypoDomains = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"gmial.com","gmail.com"},{"gmal.com","gmail.com"},{"gamil.com","gmail.com"},{"gmaill.com","gmail.com"},{"gnail.com","gmail.com"},{"gmail.co","gmail.com"},{"gmail.con","gmail.com"},{"gmail.cm","gmail.com"},{"gmail.om","gmail.com"},{"gmail.com.tr","gmail.com"},{"gmai.com","gmail.com"},
        {"hotmial.com","hotmail.com"},{"hotmai.com","hotmail.com"},{"hotmal.com","hotmail.com"},{"hotmail.co","hotmail.com"},{"hotmail.con","hotmail.com"},{"hotmil.com","hotmail.com"},
        {"outlok.com","outlook.com"},{"outloo.com","outlook.com"},{"outlook.co","outlook.com"},{"outlook.con","outlook.com"},
        {"yaho.com","yahoo.com"},{"yahoo.con","yahoo.com"},{"yahooo.com","yahoo.com"},{"icloud.co","icloud.com"},{"iclod.com","icloud.com"},{"yandex.con","yandex.com"}
    };
    private static readonly string[] BlockedLocals = { "noreply", "no-reply", "no_reply", "donotreply", "do-not-reply", "mailer-daemon", "postmaster", "bounce", "bounces" };

    /// <summary>selectedCids: secili firmalar (o firmalarin kullanicilari zaten alici). users: aktif portal kullanicilari.</summary>
    public static EmailListResult Analyze(string text, ICollection<int> selectedCids, List<UserRow> users)
    {
        EmailListResult r = new EmailListResult();
        if (string.IsNullOrEmpty(text)) return r;
        Dictionary<string, UserRow> byMail = new Dictionary<string, UserRow>(StringComparer.OrdinalIgnoreCase);
        if (users != null) foreach (UserRow u in users) { string e = (u.Email ?? "").Trim(); if (e.Length > 0 && !byMail.ContainsKey(e)) byMail[e] = u; }
        Dictionary<string, int> seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> dom = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int total = 0;
        foreach (Match m in Token.Matches(text))
        {
            if (++total > MaxAddresses) { r.Truncated = true; break; }
            string raw = m.Value, e = Normalize(raw);
            if (e == "@" || e.Length < 3) { total--; continue; }   // tek basina "@" (metindeki isaret) adres sayilmaz
            string why = Problem(e);
            if (why != null) { Add(r.Invalid, "raw", raw, "reason", why); continue; }
            if (seen.ContainsKey(e)) { seen[e]++; continue; }
            seen[e] = 1;
            string local = e.Substring(0, e.IndexOf('@')), domain = e.Substring(e.IndexOf('@') + 1);
            if (Array.IndexOf(BlockedLocals, local) >= 0) { Add(r.Blocked, "email", e, "reason", "Sistem adresi; yanıt almaz, gönderilmez."); continue; }
            string fix;
            if (TypoDomains.TryGetValue(domain, out fix)) { Add(r.Typos, "email", e, "suggest", local + "@" + fix); continue; }
            UserRow u;
            if (byMail.TryGetValue(e, out u) && selectedCids != null && selectedCids.Contains(u.CompanyId))
            { Add(r.Covered, "email", e, "company", u.CompanyName ?? ""); continue; }
            Dictionary<string, object> v = new Dictionary<string, object>();
            v["email"] = e;
            if (byMail.TryGetValue(e, out u)) { v["user"] = u.FullName ?? ""; v["company"] = u.CompanyName ?? ""; }
            if (Regex.IsMatch(local, @"^(info|bilgi|iletisim|contact|sales|satis|destek|support|admin|muhasebe|ik|hr|office)$")) v["role"] = true;
            r.Valid.Add(v); r.SendList.Add(e);
            int c; dom.TryGetValue(domain, out c); dom[domain] = c + 1;
        }
        foreach (KeyValuePair<string, int> kv in seen) if (kv.Value > 1) Add(r.Duplicates, "email", kv.Key, "count", kv.Value);
        List<KeyValuePair<string, int>> dl = new List<KeyValuePair<string, int>>(dom);
        dl.Sort(delegate (KeyValuePair<string, int> a, KeyValuePair<string, int> b) { int x = b.Value.CompareTo(a.Value); return x != 0 ? x : string.CompareOrdinal(a.Key, b.Key); });
        foreach (KeyValuePair<string, int> kv in dl) Add(r.Domains, "domain", kv.Key, "count", kv.Value);
        return r;
    }

    /// <summary>"mailto:", sondaki nokta / iki nokta, buyuk harf temizligi</summary>
    public static string Normalize(string s)
    {
        s = (s ?? "").Trim();
        if (s.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) s = s.Substring(7);
        int q = s.IndexOf('?'); if (q >= 0) s = s.Substring(0, q);
        return s.TrimEnd('.', ':', '!', '?').ToLowerInvariant();
    }

    /// <summary>Bicim sorunu varsa Turkce aciklama, yoksa null</summary>
    public static string Problem(string e)
    {
        int at = e.IndexOf('@');
        if (at <= 0) return "@ işaretinden önce ad yok.";
        if (e.IndexOf('@', at + 1) >= 0) return "Birden fazla @ işareti var.";
        string local = e.Substring(0, at), domain = e.Substring(at + 1);
        if (domain.Length == 0) return "@ işaretinden sonra alan adı yok.";
        if (domain.IndexOf('.') < 0) return "Alan adında uzantı yok (.com gibi).";
        if (e.Contains("..")) return "Art arda iki nokta var.";
        if (local.StartsWith(".") || local.EndsWith(".")) return "Ad nokta ile başlıyor ya da bitiyor.";
        if (Regex.IsMatch(e, @"[çğıöşüÇĞİÖŞÜ]")) return "Türkçe karakter içeriyor.";
        if (e.Length > 254) return "Adres çok uzun.";
        if (!Strict.IsMatch(e)) return "Geçersiz karakter ya da uzantı.";
        return null;
    }

    private static void Add(List<Dictionary<string, object>> l, string k1, object v1, string k2, object v2)
    { Dictionary<string, object> d = new Dictionary<string, object>(); d[k1] = v1; d[k2] = v2; l.Add(d); }
}
