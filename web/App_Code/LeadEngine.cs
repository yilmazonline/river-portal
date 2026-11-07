using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

// Lead Merkezi gonderim ve yanit motoru (63. migration).
// Graph: uygulama izniyle (Mail.Send, Mail.ReadWrite) YALNIZ fol.solutions kutulari (Exchange erisim ilkesiyle sinirli).
// Claude: yanit siniflandirma ve kisisellestirme (anahtar yoksa kural tabanli siniflandirma, kisisellestirme yok).
public static class LeadGraph
{
    static readonly JavaScriptSerializer JS = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };
    static string Cfg(string k) { return (WebConfigurationManager.AppSettings[k] ?? "").Trim(); }
    public static bool Configured { get { return Cfg("Graph.TenantId").Length > 0 && Cfg("Graph.ClientId").Length > 0 && Cfg("Graph.ClientSecret").Length > 0; } }
    static readonly object Lk = new object(); static string tok; static DateTime tokExp;

    public static string Token(out string err)
    {
        err = null;
        lock (Lk)
        {
            if (tok != null && DateTime.UtcNow < tokExp) return tok;
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                HttpWebRequest rq = (HttpWebRequest)WebRequest.Create("https://login.microsoftonline.com/" + Uri.EscapeDataString(Cfg("Graph.TenantId")) + "/oauth2/v2.0/token");
                rq.Method = "POST"; rq.ContentType = "application/x-www-form-urlencoded"; rq.Timeout = 30000;
                byte[] b = Encoding.UTF8.GetBytes("client_id=" + Uri.EscapeDataString(Cfg("Graph.ClientId")) + "&client_secret=" + Uri.EscapeDataString(Cfg("Graph.ClientSecret")) + "&scope=" + Uri.EscapeDataString("https://graph.microsoft.com/.default") + "&grant_type=client_credentials");
                rq.ContentLength = b.Length; using (Stream s = rq.GetRequestStream()) s.Write(b, 0, b.Length);
                using (HttpWebResponse rs = (HttpWebResponse)rq.GetResponse()) using (StreamReader rd = new StreamReader(rs.GetResponseStream()))
                {
                    Dictionary<string, object> d = JS.Deserialize<Dictionary<string, object>>(rd.ReadToEnd());
                    tok = Convert.ToString(d["access_token"]); int ex; int.TryParse(Convert.ToString(d["expires_in"]), out ex);
                    tokExp = DateTime.UtcNow.AddSeconds(Math.Max(60, ex - 300)); return tok;
                }
            }
            catch (WebException we) { err = "Microsoft oturum açma hatası: " + Detail(we); return null; }
            catch (Exception ex) { err = "Microsoft oturum açma hatası: " + ex.Message; return null; }
        }
    }
    static string Detail(WebException we)
    {
        try { if (we.Response != null) using (StreamReader rd = new StreamReader(we.Response.GetResponseStream())) { string t = rd.ReadToEnd(); return t.Length > 400 ? t.Substring(0, 400) : t; } } catch { }
        return we.Message;
    }
    /// <summary>Graph cagrisi. path: /users/... ; 202/204 bos sozluk doner. Hata: null + err.</summary>
    public static Dictionary<string, object> Call(string method, string path, object body, out string err, string prefer)
    {
        err = null; string t = Token(out err); if (t == null) return null;
        try
        {
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create("https://graph.microsoft.com/v1.0" + path);
            rq.Method = method; rq.Timeout = 60000; rq.Headers["Authorization"] = "Bearer " + t; rq.Accept = "application/json";
            if (!string.IsNullOrEmpty(prefer)) rq.Headers["Prefer"] = prefer;
            if (body != null)
            {
                byte[] b = Encoding.UTF8.GetBytes(JS.Serialize(body)); rq.ContentType = "application/json"; rq.ContentLength = b.Length;
                using (Stream s = rq.GetRequestStream()) s.Write(b, 0, b.Length);
            }
            else if (method == "POST") rq.ContentLength = 0;
            using (HttpWebResponse rs = (HttpWebResponse)rq.GetResponse()) using (StreamReader rd = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
            {
                string txt = rd.ReadToEnd();
                return txt.Trim().Length == 0 ? new Dictionary<string, object>() : JS.Deserialize<Dictionary<string, object>>(txt);
            }
        }
        catch (WebException we)
        {
            int code = we.Response is HttpWebResponse ? (int)((HttpWebResponse)we.Response).StatusCode : 0;
            err = "Graph " + (code > 0 ? code + " " : "") + "hatası: " + Detail(we);
            if (code == 403) err += " (izin ya da Exchange erişim ilkesi bu kutuya izin vermiyor)";
            return null;
        }
        catch (Exception ex) { err = "Graph bağlantı hatası: " + ex.Message; return null; }
    }
    public static string S(Dictionary<string, object> d, string k) { object o; return d != null && d.TryGetValue(k, out o) && o != null ? Convert.ToString(o, CultureInfo.InvariantCulture) : ""; }
    public static Dictionary<string, object> O(Dictionary<string, object> d, string k) { object o; return d != null && d.TryGetValue(k, out o) ? o as Dictionary<string, object> : null; }
    public static System.Collections.ArrayList A(Dictionary<string, object> d, string k) { object o; return d != null && d.TryGetValue(k, out o) ? o as System.Collections.ArrayList : null; }
    public static string Box(string mb) { return "/users/" + Uri.EscapeDataString(mb); }
}

public static class LeadClaude
{
    static readonly CultureInfo TR = new CultureInfo("tr-TR");
    static readonly JavaScriptSerializer JS = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
    static string Key { get { return (WebConfigurationManager.AppSettings["Anthropic.ApiKey"] ?? "").Trim(); } }
    static string Model { get { string m = (WebConfigurationManager.AppSettings["Anthropic.Model"] ?? "").Trim(); return m.Length > 0 ? m : "claude-haiku-5-5"; } }
    public static bool Configured { get { return Key.Length > 0; } }
    /// <summary>Tek mesaj; modelin metin cevabi icindeki ilk JSON nesnesi doner.</summary>
    static string ApiUrl = "https://api.anthropic.com/v1/messages";
    static bool useBearer;   // anahtar x-api-key ile reddedildiyse Authorization: Bearer ile calisiyor
    public static Dictionary<string, object> Json(string system, string user, int maxTokens, out string err)
    {
        Dictionary<string, object> r = JsonOnce(system, user, maxTokens, useBearer, out err);
        if (r == null && err != null && (err.Contains("401") || err.Contains("authentication_error")))
        {
            string err2; Dictionary<string, object> r2 = JsonOnce(system, user, maxTokens, !useBearer, out err2);
            if (r2 != null) { useBearer = !useBearer; err = null; return r2; }
        }
        return r;
    }
    static Dictionary<string, object> JsonOnce(string system, string user, int maxTokens, bool bearer, out string err)
    {
        err = null;
        try
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(ApiUrl);
            rq.Method = "POST"; rq.ContentType = "application/json"; rq.Timeout = 60000; rq.Headers["anthropic-version"] = "2023-06-01";
            if (bearer) rq.Headers["Authorization"] = "Bearer " + Key; else rq.Headers["x-api-key"] = Key;
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["model"] = Model; body["max_tokens"] = maxTokens; body["system"] = system;
            body["messages"] = new object[] { new Dictionary<string, object> { { "role", "user" }, { "content", user } } };
            byte[] b = Encoding.UTF8.GetBytes(JS.Serialize(body)); rq.ContentLength = b.Length;
            using (Stream s = rq.GetRequestStream()) s.Write(b, 0, b.Length);
            using (HttpWebResponse rs = (HttpWebResponse)rq.GetResponse()) using (StreamReader rd = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
            {
                Dictionary<string, object> d = JS.Deserialize<Dictionary<string, object>>(rd.ReadToEnd());
                StringBuilder txt = new StringBuilder();
                System.Collections.ArrayList c = LeadGraph.A(d, "content");
                if (c != null) foreach (object o in c) { Dictionary<string, object> p = o as Dictionary<string, object>; if (p != null && LeadGraph.S(p, "type") == "text") txt.Append(LeadGraph.S(p, "text")); }
                Match m = Regex.Match(txt.ToString(), @"\{[\s\S]*\}");
                if (!m.Success) { err = "Claude cevabında JSON yok."; return null; }
                return JS.Deserialize<Dictionary<string, object>>(m.Value);
            }
        }
        catch (WebException we)
        {
            string t = ""; try { if (we.Response != null) using (StreamReader rd = new StreamReader(we.Response.GetResponseStream())) t = rd.ReadToEnd(); } catch { }
            int code = we.Response is HttpWebResponse ? (int)((HttpWebResponse)we.Response).StatusCode : 0;
            err = "Claude " + (code > 0 ? code + " " : "") + "hatası: " + (t.Length > 0 ? (t.Length > 300 ? t.Substring(0, 300) : t) : we.Message); return null;
        }
        catch (Exception ex) { err = "Claude hatası: " + ex.Message; return null; }
    }

    /// <summary>Kisisellestirme: hitap ("Ayse Hanim") ve tek cumlelik giris. Yalniz verilen bilgiler; uydurma yok.</summary>
    public static bool Personalize(LdContact x, out string gender, out bool sure, out string giris)
    {
        gender = null; sure = false; giris = null; if (!Configured) return false;
        string sys = "Türkçe B2B tanıtım e-postaları için kısa kişiselleştirme yazıyorsun. Yalnız sana verilen bilgileri kullan; firma hakkında başarı, ödül, proje ya da haber UYDURMA. " +
                     "Firmanın kendi tanımı verildiyse giriş cümlesini ona dayandır (firmanın ne yaptığına somut ve sade bir atıf). Abartılı iltifat ve kalıp cümle kullanma (\"çalışmalarınızı takdirle izliyoruz\" gibi). Sadece şu JSON'u döndür: {\"cinsiyet\":\"E\",\"emin\":true,\"giris\":\"...\"}. " +
                     "cinsiyet: kişinin adına göre E (erkek, hitap Bey) ya da K (kadın, hitap Hanım); her zaman birini seç. emin: ad Türkçede yalnız o cinsiyette yaygınsa true, iki cinsiyette de kullanılıyorsa (Deniz, Umut, Evren gibi) false. giris: en fazla 28 kelimelik tek cümle; kişinin rolü ile firmanın sektörü ya da ölçeği arasında doğal bir bağ kurar, River AI'dan bahsetmez.";
        string usr = "Kategori: " + (x.Category == "danisman" ? "danışmanlık firması (iş ortağı adayı)" : "potansiyel müşteri") + "\nAd soyad: " + x.FullName + "\nUnvan: " + (x.Title ?? "") +
                     "\nFirma: " + (x.CompanyName ?? "") + "\nSektör: " + (string.IsNullOrEmpty(x.Industry) ? "bilinmiyor" : x.Industry) + "\nŞehir: " + (string.IsNullOrEmpty(x.City) ? "bilinmiyor" : x.City) +
                     "\nÇalışan sayısı (üst sınır): " + (x.Employees.HasValue ? x.Employees.Value.ToString() : "bilinmiyor") +
                     (string.IsNullOrEmpty(x.Description) ? "" : "\nFirmanın kendi tanımı: " + (x.Description.Length > 600 ? x.Description.Substring(0, 600) : x.Description).Replace("\r", " ").Replace("\n", " "));
        string err; Dictionary<string, object> d = Json(sys, usr, 300, out err);
        if (d == null) return false;
        string cg = LeadGraph.S(d, "cinsiyet").Trim().ToUpperInvariant(); gender = cg == "K" ? "K" : cg == "E" ? "E" : null;
        sure = gender != null && LeadGraph.S(d, "emin").Trim().ToLowerInvariant() == "true"; giris = LeadGraph.S(d, "giris").Trim();
        if (giris.Length > 260 || giris.Contains("River")) giris = null;
        return gender != null || giris != null;
    }

    public static readonly string[] Classes = { "interested", "later", "notinterested", "wrongperson", "ooo", "bounce", "unsubscribe", "other" };
    /// <summary>Yanit siniflandirma: Claude (varsa) ya da kurallar.</summary>
    public static string Classify(string subject, string text, bool autoHeader, out string summary)
    {
        summary = null; string t = (subject + "\n" + text);
        if (Configured)
        {
            string sys = "Bir B2B tanıtım e-postasına gelen yanıtı sınıflandır. Sadece JSON döndür: {\"class\":\"...\",\"summary\":\"...\"}. class şunlardan biri: " +
                         "interested (görüşme, demo, bilgi istiyor ya da olumlu), later (şimdi değil, ileride), notinterested (ilgilenmiyor), wrongperson (doğru kişi değil ya da başkasına yönlendiriyor), " +
                         "ooo (otomatik yanıt, izin, ofis dışı), bounce (teslim edilemedi bildirimi), unsubscribe (listeden çıkarılmak istiyor), other. summary: Türkçe, en fazla 20 kelime.";
            string err; Dictionary<string, object> d = Json(sys, "Konu: " + subject + "\n\n" + (text.Length > 3000 ? text.Substring(0, 3000) : text), 200, out err);
            if (d != null)
            {
                string c = LeadGraph.S(d, "class"); summary = LeadGraph.S(d, "summary"); if (summary.Length > 300) summary = summary.Substring(0, 300);
                if (Array.IndexOf(Classes, c) >= 0) return c;
            }
        }
        // Turkce kucuk harf ("İ" -> "i", "I" -> "ı") + dilden bagimsiz kucuk harf (Ingilizce "INTERESTED" bozulmasin); "ı" sadelestirilmez, kurallar "ı" icerir
        string f = (t ?? "").ToLower(TR) + "\n" + (t ?? "").ToLowerInvariant();
        if (autoHeader || Regex.IsMatch(f, @"(^|\n)\s*(otomatik yanıt|otomatik cevap|automatic reply|auto(matic)?[- ]?reply|out of office)") || Regex.IsMatch(f, @"izindeyim|ofis dışında|yıllık izin(deyim)?|ulaşamayacağım|out of (the )?office|on leave|on vacation")) return "ooo";
        if (Regex.IsMatch(f, @"çıkar(ın|ır mısınız)?\b|listeden|abonelik|unsubscribe|remove me|e-?posta (göndermeyin|istemiyorum)|bir daha (yazmayın|göndermeyin)")) return "unsubscribe";
        if (Regex.IsMatch(f, @"ilgilenmiyoruz|ihtiyacımız yok|gerek yok|not interested|no need|uygun değil")) return "notinterested";
        if (Regex.IsMatch(f, @"doğru kişi değil|ilgili (kişi|arkadaş)|yönlendir|wrong person|not the right")) return "wrongperson";
        if (Regex.IsMatch(f, @"daha sonra|ileride|şu an (değil|için)|önümüzdeki (ay|dönem|yıl)|later|next (month|quarter|year)")) return "later";
        if (Regex.IsMatch(f, @"görüşelim|toplantı|demo|uygun(uz|um)|ilgileniyor|detay|bilgi (alabilir|almak|ister)|arayalım|telefon|meeting|interested|let's talk|call")) return "interested";
        return "other";
    }
}

public static class LeadEngine
{
    public static readonly CultureInfo TR = Leads.TR;
    static DateTime NowTr { get { return DateTime.UtcNow.ToTrLocal(); } }
    static HashSet<DateTime> holCache; static DateTime holAt;
    static bool Holiday(DateTime d)
    {
        if (holCache == null || (DateTime.UtcNow - holAt).TotalHours > 6)
        {
            HashSet<DateTime> h = new HashSet<DateTime>();
            try { if (Leaves.Ready()) foreach (KeyValuePair<DateTime, KeyValuePair<string, bool>> kv in Leaves.Holidays()) if (!kv.Value.Value) h.Add(kv.Key.Date); } catch { }
            holCache = h; holAt = DateTime.UtcNow;
        }
        return holCache.Contains(d.Date);
    }
    static bool Workday(DateTime d) { return d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !Holiday(d); }
    public static DateTime AddWorkdays(DateTime fromTr, int n) { DateTime d = fromTr.Date; int k = 0; while (k < n) { d = d.AddDays(1); if (Workday(d)) k++; } return d.AddHours(10); }
    static TimeSpan Hm(string s, TimeSpan def) { TimeSpan t; return TimeSpan.TryParseExact(s ?? "", "hh\\:mm", CultureInfo.InvariantCulture, out t) ? t : def; }
    public static bool InWindow(Dictionary<string, string> st)
    {
        DateTime n = NowTr; if (!Workday(n)) return false;
        return n.TimeOfDay >= Hm(Leads.S(st, "sendFrom", "09:30"), new TimeSpan(9, 30, 0)) && n.TimeOfDay <= Hm(Leads.S(st, "sendTo", "17:30"), new TimeSpan(17, 30, 0));
    }
    static DateTime TodayStartUtc { get { return NowTr.Date.AddHours(-3); } }   // TR = UTC+3
    public static int SentToday(string mb)
    {
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT COUNT(*) FROM dbo.LeadMessages WHERE Status = N'sent' AND Mailbox = @m AND SentAt >= @t", c))
        { q.Parameters.AddWithValue("@m", mb); q.Parameters.AddWithValue("@t", TodayStartUtc); return (int)q.ExecuteScalar(); }
    }
    static List<string> Bcc(Dictionary<string, string> st)
    {
        List<string> l = new List<string>(); foreach (string a in Leads.S(st, "bcc", "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) if (a.Trim().Contains("@")) l.Add(a.Trim()); return l;
    }
    static object Rcpt(string email, string name) { return new Dictionary<string, object> { { "emailAddress", new Dictionary<string, object> { { "address", email }, { "name", name ?? "" } } } }; }

    /// <summary>Tek e-posta gonderimi. Ilk e-posta yeni ileti; takip ve portal yaniti ayni zincirde (createReply). Hata metni ya da null.</summary>
    public static string Send(int messageId)
    {
        Dictionary<string, string> st = Leads.Settings();
        int contactId, step; string mb, subject, body, email, name, replyGraphId = null; int? replyToId;
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT m.ContactId, m.StepNo, m.Mailbox, m.Subject, m.Body, ct.Email, ct.FullName, m.ReplyToId FROM dbo.LeadMessages m JOIN dbo.LeadContacts ct ON ct.ContactId = m.ContactId WHERE m.MessageId = @i AND m.Status = N'approved'", c))
        {
            q.Parameters.AddWithValue("@i", messageId);
            using (SqlDataReader r = q.ExecuteReader()) { if (!r.Read()) return "Gönderilecek onaylı e-posta bulunamadı."; contactId = r.GetInt32(0); step = r.GetInt32(1); mb = r.GetString(2); subject = r.GetString(3); body = r.GetString(4); email = r.GetString(5); name = r.GetString(6); replyToId = r.IsDBNull(7) ? (int?)null : r.GetInt32(7); }
        }
        if (!mb.EndsWith("@fol.solutions", StringComparison.OrdinalIgnoreCase)) return Fail(messageId, "Gönderim kutusu fol.solutions dışında olamaz.");
        // son kontrol: engelleme listesi (onaydan sonra eklenmis olabilir)
        Dictionary<string, string> why; HashSet<string> bd = Leads.BlockedDomains(out why);
        if (Leads.BlockedEmails().Contains(email) || bd.Contains(Leads.DomainOf(email) ?? "")) { Leads.Exclude(contactId, "Gönderimden önce engelleme listesinde"); return "Alıcı engelleme listesinde; gönderilmedi."; }
        string html = Leads.ToHtml(body, Leads.S(st, "signature", ""), Leads.S(st, "optOut", ""));
        List<object> bcc = new List<object>(); foreach (string a in Bcc(st)) bcc.Add(Rcpt(a, null));
        string err; Dictionary<string, object> draft = null;
        // zincir: portal yaniti -> gelen yanitin uzerine; takip -> ilk e-postanin uzerine
        if (replyToId.HasValue)
            using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT GraphMessageId FROM dbo.LeadReplies WHERE ReplyId = @r", c)) { q.Parameters.AddWithValue("@r", replyToId.Value); replyGraphId = q.ExecuteScalar() as string; }
        else if (step > 0)
        {
            string imid = null;
            using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT TOP 1 InternetMessageId FROM dbo.LeadMessages WHERE ContactId = @c AND Status = N'sent' AND InternetMessageId IS NOT NULL ORDER BY SentAt DESC", c)) { q.Parameters.AddWithValue("@c", contactId); imid = q.ExecuteScalar() as string; }
            if (imid != null)
            {
                Dictionary<string, object> f = LeadGraph.Call("GET", LeadGraph.Box(mb) + "/messages?$top=1&$select=id&$filter=" + Uri.EscapeDataString("internetMessageId eq '" + imid.Replace("'", "''") + "'"), null, out err, null);
                System.Collections.ArrayList v = LeadGraph.A(f, "value"); if (v != null && v.Count > 0) replyGraphId = LeadGraph.S(v[0] as Dictionary<string, object>, "id");
            }
        }
        if (replyGraphId != null)
        {
            draft = LeadGraph.Call("POST", LeadGraph.Box(mb) + "/messages/" + Uri.EscapeDataString(replyGraphId) + "/createReply", null, out err, null);
            if (draft == null) return Fail(messageId, err);
            Dictionary<string, object> patch = new Dictionary<string, object>();
            patch["body"] = new Dictionary<string, object> { { "contentType", "HTML" }, { "content", html } };
            patch["toRecipients"] = new object[] { Rcpt(email, name) }; patch["bccRecipients"] = bcc.ToArray();
            draft = LeadGraph.Call("PATCH", LeadGraph.Box(mb) + "/messages/" + Uri.EscapeDataString(LeadGraph.S(draft, "id")), patch, out err, null);
            if (draft == null) return Fail(messageId, err);
        }
        else
        {
            Dictionary<string, object> msg = new Dictionary<string, object>();
            msg["subject"] = step > 0 && !subject.StartsWith("Re:") ? "Re: " + subject : subject;
            msg["body"] = new Dictionary<string, object> { { "contentType", "HTML" }, { "content", html } };
            msg["toRecipients"] = new object[] { Rcpt(email, name) }; msg["bccRecipients"] = bcc.ToArray();
            draft = LeadGraph.Call("POST", LeadGraph.Box(mb) + "/messages", msg, out err, null);
            if (draft == null) return Fail(messageId, err);
        }
        string id = LeadGraph.S(draft, "id"), conv = LeadGraph.S(draft, "conversationId"), inet = LeadGraph.S(draft, "internetMessageId");
        Dictionary<string, object> sent = LeadGraph.Call("POST", LeadGraph.Box(mb) + "/messages/" + Uri.EscapeDataString(id) + "/send", null, out err, null);
        if (sent == null) return Fail(messageId, err);
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadMessages SET Status = N'sent', SentAt = SYSUTCDATETIME(), GraphMessageId = @g, ConversationId = @cv, InternetMessageId = @im, Error = NULL WHERE MessageId = @i; " +
            (replyToId.HasValue ? "" : "UPDATE dbo.LeadContacts SET Status = N'sequence', Step = @st, NextAt = @nx, UpdatedAt = SYSUTCDATETIME() WHERE ContactId = @c AND Status IN (N'queued', N'sequence', N'new');"), c))
        {
            q.Parameters.AddWithValue("@g", id); q.Parameters.AddWithValue("@cv", conv); q.Parameters.AddWithValue("@im", inet.Length > 0 ? (object)inet : DBNull.Value); q.Parameters.AddWithValue("@i", messageId);
            q.Parameters.AddWithValue("@st", step); q.Parameters.AddWithValue("@c", contactId);
            // takip: ilk e-postadan 3 is gunu, ikinciden 5 is gunu sonra (8. gun); son adimdan sonra 5 is gunu bekleyip dizi kapanir
            q.Parameters.AddWithValue("@nx", AddWorkdays(NowTr, step == 0 ? 3 : 5).AddHours(-3));
            q.ExecuteNonQuery();
        }
        return null;
    }
    static string Fail(int id, string err)
    {
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadMessages SET Status = N'failed', Error = @e WHERE MessageId = @i", c))
        { q.Parameters.AddWithValue("@e", (err ?? "").Length > 500 ? err.Substring(0, 500) : (object)(err ?? "")); q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); }
        return err;
    }

    /// <summary>Takip taslaklari: dizideki, sirasi gelmis ve yanit vermemis adaylar. Son adimdan sonra dizi kapanir.</summary>
    public static int Followups()
    {
        Dictionary<string, string> st = Leads.Settings(); bool auto = Leads.S(st, "followupAuto", "0") == "1"; int made = 0;
        List<int[]> due = new List<int[]>();
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadContacts SET Status = N'done', NextAt = NULL, UpdatedAt = SYSUTCDATETIME() WHERE Status = N'sequence' AND Step >= 2 AND NextAt <= SYSUTCDATETIME()", c)) q.ExecuteNonQuery();
            using (SqlCommand q = new SqlCommand("SELECT ContactId, Step FROM dbo.LeadContacts WHERE Status = N'sequence' AND Step < 2 AND NextAt <= SYSUTCDATETIME() AND NOT EXISTS (SELECT 1 FROM dbo.LeadMessages m WHERE m.ContactId = LeadContacts.ContactId AND m.Status IN (N'draft', N'approved'))", c))
            using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) due.Add(new[] { r.GetInt32(0), r.GetInt32(1) });
        }
        Dictionary<int, LdContact> seq = new Dictionary<int, LdContact>(); if (due.Count > 0) foreach (LdContact y in Leads.Contacts(null, "sequence", null, 100000)) seq[y.Id] = y;
        foreach (int[] d in due)
        {
            LdContact x; if (!seq.TryGetValue(d[0], out x)) continue;
            if (x == null) continue;
            int next = d[1] + 1; string subj = null, body = null, firstSubj = null, box = Leads.S(st, x.Category == "danisman" ? "mailboxDanisman" : "mailboxMusteri", "");
            using (SqlConnection c = Db.Open())
            {
                using (SqlCommand q = new SqlCommand("SELECT TOP 1 Subject, Body FROM dbo.LeadTemplates WHERE Category = @c AND StepNo = @s AND IsActive = 1 ORDER BY CASE WHEN Persona = @p THEN 0 WHEN Persona IN (N'genel', N'ortak') THEN 1 ELSE 2 END", c))
                { q.Parameters.AddWithValue("@c", x.Category); q.Parameters.AddWithValue("@s", next); q.Parameters.AddWithValue("@p", x.Persona ?? "genel"); using (SqlDataReader r = q.ExecuteReader()) if (r.Read()) { subj = r.GetString(0); body = r.GetString(1); } }
                using (SqlCommand q = new SqlCommand("SELECT TOP 1 Subject FROM dbo.LeadMessages WHERE ContactId = @c AND StepNo = 0 AND Status = N'sent'", c)) { q.Parameters.AddWithValue("@c", x.Id); firstSubj = q.ExecuteScalar() as string; }
            }
            if (body == null) { using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadContacts SET Status = N'done', NextAt = NULL WHERE ContactId = @c", c)) { q.Parameters.AddWithValue("@c", x.Id); q.ExecuteNonQuery(); } continue; }
            string s2 = (subj ?? "").Replace("{Konu}", firstSubj ?? ""); s2 = Leads.Fill(s2, x, null, null);
            using (SqlConnection c = Db.Open())
            using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeadMessages (ContactId, StepNo, Mailbox, Subject, Body, Status, ApprovedAt) VALUES (@c, @s, @m, @sj, @b, @st, " + (auto ? "SYSUTCDATETIME()" : "NULL") + "); UPDATE dbo.LeadContacts SET NextAt = NULL WHERE ContactId = @c", c))
            {
                q.Parameters.AddWithValue("@c", x.Id); q.Parameters.AddWithValue("@s", next); q.Parameters.AddWithValue("@m", box); q.Parameters.AddWithValue("@sj", s2.Length > 200 ? s2.Substring(0, 200) : s2);
                q.Parameters.AddWithValue("@b", Leads.Fill(body.Replace("{Konu}", firstSubj ?? ""), x, null, null)); q.Parameters.AddWithValue("@st", auto ? "approved" : "draft"); q.ExecuteNonQuery();
            }
            made++;
        }
        return made;
    }

    static readonly Regex BounceFrom = new Regex(@"postmaster|mailer-daemon|microsoftexchange|mail delivery", RegexOptions.IgnoreCase);
    static readonly Regex BounceSubj = new Regex(@"undeliverable|teslim edilemedi|delivery status notification|could not be delivered|returned mail|iletilemedi", RegexOptions.IgnoreCase);
    static readonly Regex EmailIn = new Regex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}");

    /// <summary>Gelen kutularini okur. YALNIZ adaylardan gelenler ve geri donen adres bildirimleri islenir (isinma e-postalari yok sayilir).</summary>
    public static string ReadReplies(string baseUrl)
    {
        Dictionary<string, string> st = Leads.Settings(); int got = 0; List<string> errs = new List<string>();
        foreach (string mb in new[] { Leads.S(st, "mailboxMusteri", ""), Leads.S(st, "mailboxDanisman", "") })
        {
            if (mb.Length == 0) continue;
            string last = Leads.S(st, "lastRead." + mb, DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
            string err; Dictionary<string, object> r = LeadGraph.Call("GET", LeadGraph.Box(mb) + "/mailFolders/inbox/messages?$top=50&$orderby=receivedDateTime asc&$select=id,conversationId,from,subject,receivedDateTime,internetMessageHeaders,uniqueBody,bodyPreview&$filter=" +
                Uri.EscapeDataString("receivedDateTime gt " + last), null, out err, "outlook.body-content-type=\"text\"");
            if (r == null) { errs.Add(mb + ": " + err); continue; }
            System.Collections.ArrayList v = LeadGraph.A(r, "value"); if (v == null) continue;
            string maxAt = last;
            foreach (object o in v)
            {
                Dictionary<string, object> m = o as Dictionary<string, object>; if (m == null) continue;
                string gid = LeadGraph.S(m, "id"), conv = LeadGraph.S(m, "conversationId"), subj = LeadGraph.S(m, "subject"), at = LeadGraph.S(m, "receivedDateTime");
                if (string.CompareOrdinal(at, maxAt) > 0) maxAt = at;
                string from = LeadGraph.S(LeadGraph.O(LeadGraph.O(m, "from"), "emailAddress"), "address").ToLowerInvariant();
                string text = LeadGraph.S(LeadGraph.O(m, "uniqueBody"), "content"); if (text.Length == 0) text = LeadGraph.S(m, "bodyPreview");
                bool auto = false; System.Collections.ArrayList hs = LeadGraph.A(m, "internetMessageHeaders");
                if (hs != null) foreach (object h in hs) { Dictionary<string, object> hd = h as Dictionary<string, object>; string hn = LeadGraph.S(hd, "name").ToLowerInvariant(), hv = LeadGraph.S(hd, "value").ToLowerInvariant(); if ((hn == "auto-submitted" && hv != "no") || hn == "x-autoreply" || hn == "x-autorespond" || (hn == "precedence" && hv == "auto_reply")) auto = true; }
                bool bounce = BounceFrom.IsMatch(from) || BounceSubj.IsMatch(subj);
                int? contactId = null, messageId = null;
                using (SqlConnection c = Db.Open())
                {
                    using (SqlCommand q = new SqlCommand("SELECT COUNT(*) FROM dbo.LeadReplies WHERE GraphMessageId = @g", c)) { q.Parameters.AddWithValue("@g", gid); if ((int)q.ExecuteScalar() > 0) continue; }
                    if (conv.Length > 0) using (SqlCommand q = new SqlCommand("SELECT TOP 1 ContactId, MessageId FROM dbo.LeadMessages WHERE ConversationId = @cv ORDER BY SentAt DESC", c))
                        { q.Parameters.AddWithValue("@cv", conv); using (SqlDataReader rd = q.ExecuteReader()) if (rd.Read()) { contactId = rd.GetInt32(0); messageId = rd.GetInt32(1); } }
                    if (!contactId.HasValue && !bounce) using (SqlCommand q = new SqlCommand("SELECT TOP 1 ContactId FROM dbo.LeadContacts WHERE Email = @e", c)) { q.Parameters.AddWithValue("@e", from); object x = q.ExecuteScalar(); if (x != null) contactId = (int)x; }
                    if (!contactId.HasValue && bounce)
                        foreach (Match em in EmailIn.Matches(text + " " + subj))
                            using (SqlCommand q = new SqlCommand("SELECT TOP 1 ContactId FROM dbo.LeadContacts WHERE Email = @e AND Status IN (N'sequence', N'queued')", c)) { q.Parameters.AddWithValue("@e", em.Value.ToLowerInvariant()); object x = q.ExecuteScalar(); if (x != null) { contactId = (int)x; break; } }
                }
                if (!contactId.HasValue) continue;   // adaylarimizla ilgisiz (isinma e-postalari vb.)
                string summary = null; string cls = bounce ? "bounce" : LeadClaude.Classify(subj, text, auto, out summary);
                if (bounce) summary = "Teslim edilemedi bildirimi";
                int replyId;
                using (SqlConnection c = Db.Open())
                using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeadReplies (ContactId, MessageId, Mailbox, GraphMessageId, ReceivedAt, FromEmail, Subject, Preview, Body, Class, Summary, Handled) VALUES (@c, @m, @mb, @g, @at, @f, @s, @p, @b, @cl, @sm, @h); SELECT CAST(SCOPE_IDENTITY() AS INT);", c))
                {
                    DateTime ra; if (!DateTime.TryParse(at, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out ra)) ra = DateTime.UtcNow;
                    q.Parameters.AddWithValue("@c", contactId.Value); q.Parameters.AddWithValue("@m", messageId.HasValue ? (object)messageId.Value : DBNull.Value); q.Parameters.AddWithValue("@mb", mb); q.Parameters.AddWithValue("@g", gid);
                    q.Parameters.AddWithValue("@at", ra); q.Parameters.AddWithValue("@f", from); q.Parameters.AddWithValue("@s", subj.Length > 300 ? subj.Substring(0, 300) : subj);
                    string pv = Regex.Replace(text, @"\s+", " ").Trim(); q.Parameters.AddWithValue("@p", pv.Length > 1000 ? pv.Substring(0, 1000) : pv); q.Parameters.AddWithValue("@b", text.Length > 4000 ? text.Substring(0, 4000) : text);
                    q.Parameters.AddWithValue("@cl", cls); q.Parameters.AddWithValue("@sm", summary == null ? (object)DBNull.Value : summary);
                    q.Parameters.AddWithValue("@h", cls == "ooo" || cls == "bounce" || cls == "unsubscribe" || cls == "notinterested");   // otomatik islenenler ele alinmis sayilir
                    replyId = (int)q.ExecuteScalar();
                }
                Apply(contactId.Value, cls);
                if (cls == "interested" || cls == "later" || cls == "wrongperson" || cls == "other") Notify(replyId, baseUrl);
                got++;
            }
            Leads.SaveSetting("lastRead." + mb, maxAt);
        }
        return "yanıt " + got + (errs.Count > 0 ? " · hata: " + string.Join(" | ", errs.ToArray()) : "");
    }
    /// <summary>Siniflandirmaya gore aday: dizi durur (ooo haric), engelleme listesi, durum.</summary>
    public static void Apply(int contactId, string cls)
    {
        string st = cls == "interested" ? "interested" : cls == "later" ? "later" : cls == "notinterested" ? "notinterested" : cls == "bounce" ? "bounced" : cls == "unsubscribe" ? "unsubscribed" : cls == "ooo" ? null : "replied";
        using (SqlConnection c = Db.Open())
        {
            if (st == null)   // otomatik yanit: dizi bir hafta ertelenir
                using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadContacts SET NextAt = DATEADD(DAY, 7, ISNULL(NextAt, SYSUTCDATETIME())) WHERE ContactId = @c AND Status = N'sequence'", c)) { q.Parameters.AddWithValue("@c", contactId); q.ExecuteNonQuery(); }
            else
                using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadContacts SET Status = @s, NextAt = NULL, UpdatedAt = SYSUTCDATETIME() WHERE ContactId = @c; UPDATE dbo.LeadMessages SET Status = N'cancelled' WHERE ContactId = @c AND Status IN (N'draft', N'approved') AND ReplyToId IS NULL", c))
                { q.Parameters.AddWithValue("@s", st); q.Parameters.AddWithValue("@c", contactId); q.ExecuteNonQuery(); }
        }
        if (cls == "bounce" || cls == "unsubscribe") { string em = Leads.ContactEmail(contactId); if (em != null) Leads.AddSuppression("email", em, cls == "bounce" ? "Geri dönen adres" : "Listeden çıkmak istedi"); }
    }
    static void Notify(int replyId, string baseUrl)
    {
        try
        {
            Dictionary<string, string> st = Leads.Settings(); string to = Leads.S(st, "notify", ""); if (to.Length == 0) return;
            string name = "", company = "", cls = "", prev = "", sum = "", subj = "";
            using (SqlConnection c = Db.Open())
            using (SqlCommand q = new SqlCommand("SELECT ct.FullName, ISNULL(co.Name, N''), r.Class, ISNULL(r.Preview, N''), ISNULL(r.Summary, N''), ISNULL(r.Subject, N'') FROM dbo.LeadReplies r JOIN dbo.LeadContacts ct ON ct.ContactId = r.ContactId LEFT JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE r.ReplyId = @r", c))
            { q.Parameters.AddWithValue("@r", replyId); using (SqlDataReader r = q.ExecuteReader()) if (r.Read()) { name = r.GetString(0); company = r.GetString(1); cls = r.GetString(2); prev = r.GetString(3); sum = r.GetString(4); subj = r.GetString(5); } }
            string lbl = cls == "interested" ? "İlgileniyor" : cls == "later" ? "Daha sonra" : cls == "wrongperson" ? "Doğru kişi değil" : "Yanıt";
            string url = (baseUrl ?? "").TrimEnd('/') + "/Leads.aspx?tab=replies";
            string inner = "<p style=\"margin:0 0 14px;font-size:14.5px;color:#3A4A66\"><b>" + HttpUtility.HtmlEncode(name) + "</b>" + (company.Length > 0 ? " (" + HttpUtility.HtmlEncode(company) + ")" : "") + " tanıtım e-postasına yanıt verdi.</p>" +
                (sum.Length > 0 ? "<p style=\"margin:0 0 14px;font-size:14px;color:#1B2440\"><b>Özet:</b> " + HttpUtility.HtmlEncode(sum) + "</p>" : "") +
                "<div style=\"background:#F4F6FB;border:1px solid #E3E8F0;border-radius:10px;padding:12px 14px;font-size:14px;line-height:1.6;margin:0 0 16px;color:#1B2440\">" + HttpUtility.HtmlEncode(prev.Length > 600 ? prev.Substring(0, 600) + "…" : prev) + "</div>" +
                "<a href=\"" + HttpUtility.HtmlAttributeEncode(url) + "\" style=\"display:inline-block;background:#4F46E5;color:#fff;text-decoration:none;font-weight:700;font-size:14px;padding:11px 18px;border-radius:10px\">Lead Merkezi’nde aç</a>";
            string err; Mailer.SendTo(to, "Lead yanıtı · " + lbl + ": " + name + (company.Length > 0 ? ", " + company : ""), Emails.Shell("Yeni lead yanıtı: " + lbl, inner), out err);
        }
        catch { }
    }

    /// <summary>Zamanlanmis is: yanit okuma, takip taslaklari, (acik ve mesai penceresindeyse) gonderim. Cron (10 dk) ve Lead Merkezi acilisi (en sik 5 dk).</summary>
    public static string Tick(string baseUrl, bool force)
    {
        HttpContext ctx = HttpContext.Current;
        if (!force && ctx != null) { object l = ctx.Application["ldTick"]; if (l is DateTime && (DateTime.UtcNow - (DateTime)l).TotalMinutes < 5) return "skip"; ctx.Application["ldTick"] = DateTime.UtcNow; }
        if (!Leads.Ready()) return "lead tabloları yok";
        List<string> o = new List<string>();
        if (LeadGraph.Configured) o.Add(ReadReplies(baseUrl)); else o.Add("Graph tanımlı değil");
        o.Add("takip taslağı " + Followups());
        Dictionary<string, string> st = Leads.Settings();
        if (Leads.S(st, "sendEnabled", "0") != "1") { o.Add("gönderim kapalı"); return string.Join(" · ", o.ToArray()); }
        if (!LeadGraph.Configured) { o.Add("gönderim için Graph tanımlı değil"); return string.Join(" · ", o.ToArray()); }
        if (!InWindow(st)) { o.Add("mesai penceresi dışında"); return string.Join(" · ", o.ToArray()); }
        int limit = Leads.I(st, "dailyPerMailbox", 10); Random rnd = new Random();
        foreach (string mb in new[] { Leads.S(st, "mailboxMusteri", ""), Leads.S(st, "mailboxDanisman", "") })
        {
            if (mb.Length == 0) continue;
            DateTime next; if (DateTime.TryParse(Leads.S(st, "nextSend." + mb, ""), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out next) && DateTime.UtcNow < next) { o.Add(mb + " bekliyor"); continue; }
            if (SentToday(mb) >= limit) { o.Add(mb + " günlük sınırda"); continue; }
            int id = 0;
            using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT TOP 1 MessageId FROM dbo.LeadMessages WHERE Status = N'approved' AND Mailbox = @m ORDER BY ApprovedAt, MessageId", c)) { q.Parameters.AddWithValue("@m", mb); object x = q.ExecuteScalar(); if (x != null) id = (int)x; }
            if (id == 0) continue;
            string err = Send(id);
            o.Add(mb + (err == null ? " gönderdi" : " hata: " + err));
            Leads.SaveSetting("nextSend." + mb, DateTime.UtcNow.AddMinutes(rnd.Next(3, 9)).ToString("o", CultureInfo.InvariantCulture));   // e-postalar arasi 3-8 dk
        }
        return string.Join(" · ", o.ToArray());
    }

    /// <summary>Baglanti testi: belirtec + iki kutunun okunabilmesi (gonderim yapmaz)</summary>
    public static List<object> TestGraph()
    {
        List<object> l = new List<object>(); Dictionary<string, string> st = Leads.Settings();
        if (!LeadGraph.Configured) { l.Add(new { ok = false, text = "web.config içinde Graph.TenantId, Graph.ClientId ve Graph.ClientSecret tanımlı değil." }); return l; }
        string err; if (LeadGraph.Token(out err) == null) { l.Add(new { ok = false, text = err }); return l; }
        l.Add(new { ok = true, text = "Microsoft oturumu açıldı." });
        foreach (string mb in new[] { Leads.S(st, "mailboxMusteri", ""), Leads.S(st, "mailboxDanisman", "") })
        {
            Dictionary<string, object> r = LeadGraph.Call("GET", LeadGraph.Box(mb) + "/mailFolders/inbox?$select=totalItemCount,unreadItemCount", null, out err, null);
            l.Add(r == null ? (object)new { ok = false, text = mb + ": " + err } : new { ok = true, text = mb + ": erişim var (gelen kutusunda " + LeadGraph.S(r, "totalItemCount") + " e-posta)" });
        }
        if (!LeadClaude.Configured) l.Add(new { ok = false, text = "Claude anahtarı tanımlı değil: sınıflandırma kurallarla yapılır, kişiselleştirme kapalı." });
        else
        {
            string cerr; Dictionary<string, object> pong = LeadClaude.Json("Sadece JSON döndür.", "Şunu aynen döndür: {\"ok\":true}", 20, out cerr);
            l.Add(pong != null ? (object)new { ok = true, text = "Claude çalışıyor: kişiselleştirme ve sınıflandırma açık." }
                               : new { ok = false, text = "Claude anahtarı tanımlı ama çağrı başarısız (" + cerr + "). Sınıflandırma kurallarla sürer." });
        }
        return l;
    }
}
