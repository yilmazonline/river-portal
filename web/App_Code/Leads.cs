using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;

// Lead Merkezi (62. migration): aday havuzu, engelleme listesi, CSV ve Apollo ice aktarma, sablondan taslak, onay kuyrugu.
// Gonderim (Microsoft Graph) ve Claude ile kisisellestirme sonraki asamada bu kuyruga baglanir.
public class LdContact
{
    public int Id; public int? CompanyId; public string Category, FullName, FirstName, Title, Persona, Email, EmailStatus, Domain, LinkedIn, Source, ApolloId, Status, Note, CompanyName, Industry, City, Website, Description; public bool Suppressed;
    public int Step; public int? Employees; public DateTime CreatedAt;
    public string Gender; public bool GenderSure;   // hitap: E/K; kesin mi (67. migration)
}
public class LdMessage
{
    public int Id, ContactId, StepNo; public string Mailbox, Subject, Body, Status, Error, ContactName, ContactTitle, CompanyName, Email, Category, Persona, FirstName, Gender; public bool GenderSure;
    public DateTime CreatedAt; public DateTime? ScheduledAt, SentAt;
}
public class LdImport { public int Added, Duplicate, Suppressed, NoEmail, Unverified, CompanyCap, FreeMail, Invalid, Credits, CreditsWasted; public List<string> Notes = new List<string>(); }

public static class Leads
{
    public static readonly CultureInfo TR = new CultureInfo("tr-TR");
    static readonly JavaScriptSerializer JS = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };
    public static readonly string[] OwnDomains = { "river.com.tr", "folyazilim.com", "fol.solutions" };
    public static readonly string[] FreeMail = { "gmail.com", "googlemail.com", "hotmail.com", "hotmail.com.tr", "outlook.com", "outlook.com.tr", "live.com", "msn.com", "yahoo.com", "yahoo.com.tr", "yandex.com", "yandex.com.tr", "icloud.com", "me.com", "mynet.com", "windowslive.com", "proton.me", "protonmail.com", "aol.com", "gmx.com", "mail.ru" };
    static readonly Regex EmailRx = new Regex(@"^[^@\s]+@[^@\s]+\.[a-z]{2,}$", RegexOptions.IgnoreCase);

    public static bool Ready()
    {
        try { using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT CASE WHEN OBJECT_ID('dbo.LeadContacts','U') IS NOT NULL AND OBJECT_ID('dbo.LeadMessages','U') IS NOT NULL AND OBJECT_ID('dbo.LeadSuppression','U') IS NOT NULL THEN 1 ELSE 0 END", c)) return (int)q.ExecuteScalar() == 1; }
        catch { return false; }
    }
    public static string Fold(string s) { return (s ?? "").Trim().ToLower(TR).Replace('ı', 'i'); }
    static bool? hasGender;
    /// <summary>67. migration (hitap: Gender, GenderSure) calistirildi mi</summary>
    public static bool HasGender
    {
        get
        {
            if (hasGender.HasValue) return hasGender.Value;
            try { using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT CASE WHEN COL_LENGTH('dbo.LeadContacts','Gender') IS NULL THEN 0 ELSE 1 END", c)) hasGender = (int)q.ExecuteScalar() == 1; }
            catch { return false; }
            return hasGender.Value;
        }
    }

    // ---------- hitap: her zaman "Ad Bey" / "Ad Hanim" (ad soyad yazilmaz) ----------
    /// <summary>Kisinin adi (Turkce buyuk harf kuraliyla); ad alani bossa ad soyadin ilk kelimesi</summary>
    public static string FirstOf(LdContact x)
    {
        string[] p = (x.FullName ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string f = !string.IsNullOrEmpty(x.FirstName) ? x.FirstName : (p.Length > 0 ? p[0] : "");
        return TrNames.Proper(f);
    }
    public static string HitapOf(string first, string g) { return first + (g == "K" ? " Hanım" : " Bey"); }
    /// <summary>Cinsiyet: kayitli (elle ya da onceden) -> isim listesi -> null (belirsiz)</summary>
    public static string ResolveGender(LdContact x, out bool sure)
    {
        if (!string.IsNullOrEmpty(x.Gender)) { sure = x.GenderSure; return x.Gender; }
        string g = TrNames.Gender(FirstOf(x)); sure = g != null; return g;
    }
    /// <summary>Hitap metni; cinsiyet belirsizse "Bey" (onay kuyrugunda "kontrol edin" olarak isaretlenir)</summary>
    public static string Hitap(LdContact x) { bool s; string g = ResolveGender(x, out s); return HitapOf(FirstOf(x), g ?? "E"); }
    public static void SaveGender(int contactId, string g, bool sure)
    {
        if (!HasGender) return;
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadContacts SET Gender = @g, GenderSure = @s WHERE ContactId = @c", c))
        { q.Parameters.AddWithValue("@g", (object)g ?? DBNull.Value); q.Parameters.AddWithValue("@s", sure); q.Parameters.AddWithValue("@c", contactId); q.ExecuteNonQuery(); }
    }
    /// <summary>Bekleyen (taslak/onayli) e-postalarda eski hitaplari ve ad soyadi yeni hitapla degistirir; degisen e-posta sayisini doner</summary>
    static int ReplaceHitap(SqlConnection c, int contactId, string fullName, string first, string newHitap)
    {
        List<string> olds = new List<string>();
        foreach (string o in new[] { fullName, TrNames.Proper(fullName), first + " Bey", first + " Hanım" }) if (!string.IsNullOrEmpty(o) && o != newHitap && !olds.Contains(o)) olds.Add(o);
        List<object[]> rows = new List<object[]>();
        using (SqlCommand q = new SqlCommand("SELECT MessageId, Subject, Body FROM dbo.LeadMessages WHERE ContactId = @c AND Status IN (N'draft', N'approved')", c))
        { q.Parameters.AddWithValue("@c", contactId); using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) rows.Add(new object[] { r.GetInt32(0), r.GetString(1), r.GetString(2) }); }
        int n = 0;
        foreach (object[] row in rows)
        {
            string sj = (string)row[1], bd = (string)row[2], sj0 = sj, bd0 = bd;
            foreach (string o in olds) { string rx = "(?<![\\p{L}])" + Regex.Escape(o) + "(?![\\p{L}])"; sj = Regex.Replace(sj, rx, newHitap, RegexOptions.IgnoreCase); bd = Regex.Replace(bd, rx, newHitap, RegexOptions.IgnoreCase); }
            sj = FixSuffix(sj, newHitap); bd = FixSuffix(bd, newHitap);
            if (sj == sj0 && bd == bd0) continue;
            using (SqlCommand u = new SqlCommand("UPDATE dbo.LeadMessages SET Subject = @s, Body = @b WHERE MessageId = @i", c))
            { u.Parameters.AddWithValue("@s", sj); u.Parameters.AddWithValue("@b", bd); u.Parameters.AddWithValue("@i", (int)row[0]); u.ExecuteNonQuery(); }
            n++;
        }
        return n;
    }
    /// <summary>Hitap degisince kesme isaretinden sonraki eki unlu uyumuna gore duzeltir: "Can Hanım'e" -> "Can Hanım'a", "Ali Bey'ın" -> "Ali Bey'in"</summary>
    static string FixSuffix(string s, string hitap)
    {
        bool hanim = hitap.EndsWith(" Hanım");
        string[][] map = hanim
            ? new[] { new[] { "e", "a" }, new[] { "ye", "ya" }, new[] { "i", "ı" }, new[] { "yi", "yı" }, new[] { "in", "ın" }, new[] { "nin", "nın" }, new[] { "den", "dan" }, new[] { "le", "la" }, new[] { "yle", "yla" } }
            : new[] { new[] { "a", "e" }, new[] { "ya", "ye" }, new[] { "ı", "i" }, new[] { "yı", "yi" }, new[] { "ın", "in" }, new[] { "nın", "nin" }, new[] { "dan", "den" }, new[] { "la", "le" }, new[] { "yla", "yle" } };
        foreach (string[] m in map)
            s = Regex.Replace(s, Regex.Escape(hitap) + "(['’])" + m[0] + "(?![\\p{L}])", hitap + "$1" + m[1]);
        return s;
    }

    /// <summary>Onay kuyrugundan elle: kisinin cinsiyetini kesinlestir ve bekleyen e-postalarindaki hitabi duzelt</summary>
    public static string SetGender(int contactId, string g)
    {
        if (!HasGender) return "Önce database\\67_leads_hitap.sql dosyasını çalıştırın.";
        if (g != "E" && g != "K") return "Geçersiz seçim.";
        string full = null, first = null;
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT FullName, FirstName FROM dbo.LeadContacts WHERE ContactId = @c", c))
            { q.Parameters.AddWithValue("@c", contactId); using (SqlDataReader r = q.ExecuteReader()) if (r.Read()) { full = r.GetString(0); first = r.IsDBNull(1) ? "" : r.GetString(1); } }
            if (full == null) return "Aday bulunamadı.";
            LdContact x = new LdContact { FullName = full, FirstName = first };
            SaveGender(contactId, g, true);
            ReplaceHitap(c, contactId, full, FirstOf(x), HitapOf(FirstOf(x), g));
        }
        return null;
    }
    /// <summary>Bekleyen butun e-postalarda ad soyadli hitaplari "Ad Bey / Ad Hanim"a cevirir. Cinsiyet listeden; belirsizse "Bey" ve tahmin olarak isaretlenir.</summary>
    public static int[] FixHitaps()
    {
        int changed = 0, unsure = 0;
        List<LdContact> l = new List<LdContact>();
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT DISTINCT ct.ContactId, ct.FullName, ct.FirstName, " + (HasGender ? "ct.Gender, ct.GenderSure" : "NULL, CAST(0 AS BIT)") + " FROM dbo.LeadContacts ct JOIN dbo.LeadMessages m ON m.ContactId = ct.ContactId WHERE m.Status IN (N'draft', N'approved')", c))
            using (SqlDataReader r = q.ExecuteReader())
                while (r.Read()) l.Add(new LdContact { Id = r.GetInt32(0), FullName = r.GetString(1), FirstName = r.IsDBNull(2) ? "" : r.GetString(2), Gender = r.IsDBNull(3) ? null : r.GetString(3), GenderSure = !r.IsDBNull(4) && r.GetBoolean(4) });
            foreach (LdContact x in l)
            {
                bool sure; string g = ResolveGender(x, out sure);
                if (g == null) { g = "E"; sure = false; }
                if (string.IsNullOrEmpty(x.Gender)) SaveGender(x.Id, g, sure);
                if (!sure) unsure++;
                changed += ReplaceHitap(c, x.Id, x.FullName, FirstOf(x), HitapOf(FirstOf(x), g));
            }
        }
        return new[] { changed, unsure };
    }

    static bool? hasDesc;
    /// <summary>64. migration (firma tanimi) calistirildi mi</summary>
    public static bool HasDescription
    {
        get
        {
            if (hasDesc.HasValue) return hasDesc.Value;
            try { using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT CASE WHEN COL_LENGTH('dbo.LeadCompanies','Description') IS NULL THEN 0 ELSE 1 END", c)) hasDesc = (int)q.ExecuteScalar() == 1; }
            catch { return false; }
            return hasDesc.Value;
        }
    }

    // ================= ayarlar =================
    public static Dictionary<string, string> Settings()
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT SettingKey, SettingValue FROM dbo.LeadSettings", c)) using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) d[r.GetString(0)] = r.GetString(1);
        return d;
    }
    public static string S(Dictionary<string, string> d, string k, string def) { string v; return d.TryGetValue(k, out v) && v != null ? v : def; }
    public static int I(Dictionary<string, string> d, string k, int def) { int n; return int.TryParse(S(d, k, ""), out n) ? n : def; }
    public static void SaveSetting(string k, string v)
    {
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("IF EXISTS (SELECT 1 FROM dbo.LeadSettings WHERE SettingKey = @k) UPDATE dbo.LeadSettings SET SettingValue = @v WHERE SettingKey = @k ELSE INSERT INTO dbo.LeadSettings (SettingKey, SettingValue) VALUES (@k, @v)", c))
        { q.Parameters.AddWithValue("@k", k); q.Parameters.AddWithValue("@v", v ?? ""); q.ExecuteNonQuery(); }
    }

    // ================= engelleme =================
    public static string DomainOf(string emailOrUrl)
    {
        string s = (emailOrUrl ?? "").Trim().ToLowerInvariant(); if (s.Length == 0) return null;
        int at = s.LastIndexOf('@'); if (at >= 0) return s.Substring(at + 1);
        s = Regex.Replace(s, "^[a-z]+://", ""); s = s.Split('/', '?', '#')[0]; if (s.StartsWith("www.")) s = s.Substring(4);
        return s.Contains(".") ? s : null;
    }
    /// <summary>Engelli domainler: kendi domainlerimiz + portaldaki is ortagi ve musteri firmalarinin kullanici domainleri + elle eklenenler</summary>
    public static HashSet<string> BlockedDomains(out Dictionary<string, string> why)
    {
        why = new Dictionary<string, string>(); HashSet<string> h = new HashSet<string>();
        foreach (string d in OwnDomains) { h.Add(d); why[d] = "Kendi domainimiz"; }
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT DISTINCT LOWER(SUBSTRING(u.Email, CHARINDEX('@', u.Email) + 1, 200)), co.CompanyType FROM dbo.Users u JOIN dbo.Companies co ON co.CompanyId = u.CompanyId WHERE co.CompanyType IN (N'Partner', N'Customer') AND CHARINDEX('@', u.Email) > 0", c))
            using (SqlDataReader r = q.ExecuteReader())
                while (r.Read()) { string d = r.GetString(0).Trim(); if (Array.IndexOf(FreeMail, d) >= 0 || d.Length == 0) continue; if (h.Add(d)) why[d] = r.GetString(1) == "Partner" ? "Portalda iş ortağı" : "Portalda müşteri"; }
            using (SqlCommand q = new SqlCommand("SELECT Value, ISNULL(Reason, N'') FROM dbo.LeadSuppression WHERE Kind = N'domain'", c))
            using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) { string d = r.GetString(0); if (h.Add(d)) why[d] = r.GetString(1).Length > 0 ? r.GetString(1) : "Elle engellendi"; }
        }
        return h;
    }
    public static HashSet<string> BlockedEmails()
    {
        HashSet<string> h = new HashSet<string>();
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT Value FROM dbo.LeadSuppression WHERE Kind = N'email'", c)) using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) h.Add(r.GetString(0));
        return h;
    }
    public static string AddSuppression(string kind, string value, string reason)
    {
        value = (value ?? "").Trim().ToLowerInvariant();
        if (kind == "domain") { value = DomainOf(value) ?? ""; if (value.Length == 0) return "Geçerli bir domain yazın (örnek: firma.com.tr)."; }
        else if (kind == "email") { if (!EmailRx.IsMatch(value)) return "Geçerli bir e-posta adresi yazın."; }
        else return "Geçersiz tür.";
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("IF NOT EXISTS (SELECT 1 FROM dbo.LeadSuppression WHERE Kind = @k AND Value = @v) INSERT INTO dbo.LeadSuppression (Kind, Value, Reason) VALUES (@k, @v, @r)", c))
        { q.Parameters.AddWithValue("@k", kind); q.Parameters.AddWithValue("@v", value); q.Parameters.AddWithValue("@r", string.IsNullOrEmpty(reason) ? (object)DBNull.Value : Cut(reason, 200)); q.ExecuteNonQuery(); }
        // havuzdaki eslesen adaylar da disarida kalir; kuyruktaki taslaklari iptal edilir
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadContacts SET Status = N'excluded', Note = ISNULL(Note, N'Engelleme listesi'), UpdatedAt = SYSUTCDATETIME() WHERE Status IN (N'new', N'queued') AND " + (kind == "domain" ? "Domain = @v" : "Email = @v") +
            "; UPDATE m SET Status = N'cancelled' FROM dbo.LeadMessages m JOIN dbo.LeadContacts ct ON ct.ContactId = m.ContactId WHERE m.Status IN (N'draft', N'approved') AND ct.Status = N'excluded'", c))
        { q.Parameters.AddWithValue("@v", value); q.ExecuteNonQuery(); }
        return null;
    }
    public static void RemoveSuppression(int id) { using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("DELETE FROM dbo.LeadSuppression WHERE SuppressionId = @i", c)) { q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); } }
    public static List<object> SuppressionList()
    {
        List<object> l = new List<object>();
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT SuppressionId, Kind, Value, ISNULL(Reason, N''), CreatedAt FROM dbo.LeadSuppression ORDER BY CreatedAt DESC", c)) using (SqlDataReader r = q.ExecuteReader())
            while (r.Read()) l.Add(new { id = r.GetInt32(0), kind = r.GetString(1), value = r.GetString(2), reason = r.GetString(3), at = r.GetDateTime(4).ToTrLocal().ToString("d MMM yyyy", TR) });
        return l;
    }

    /// <summary>Dogrulama durumu: metin (Verified, valid, deliverable) ya da sayisal / mantiksal kod (Instantly: 1, true)</summary>
    public static bool IsVerified(string v) { string f = Fold(v); return f == "verified" || f == "valid" || f == "deliverable" || f == "1" || f == "true" || f == "yes"; }
    /// <summary>Bos sayilan degerler: "No data found", "None", "null", "-"</summary>
    static string Clean(string v) { v = (v ?? "").Trim(); string f = Fold(v); return f == "no data found" || f == "none" || f == "null" || f == "n/a" || f == "-" ? "" : v; }
    /// <summary>Calisan sayisi: tek sayi ya da aralik ("51 - 200" -> 200, "10001+" -> 10001)</summary>
    public static int? Employees(string v)
    {
        MatchCollection m = Regex.Matches((v ?? "").Replace(".", "").Replace(",", ""), @"\d+"); if (m.Count == 0) return null;
        int n; return int.TryParse(m[m.Count - 1].Value, out n) && n > 0 ? n : (int?)null;
    }
    /// <summary>Sehir: konumun ilk parcasi; yalniz ulke yaziyorsa bos ("Istanbul, Istanbul, Turkey" -> Istanbul)</summary>
    public static string CityOf(string v)
    {
        string c = (v ?? "").Split(',')[0].Trim(); string f = Fold(c);
        return f == "turkey" || f == "türkiye" || f == "turkiye" ? "" : c;
    }
    static string Url(string v) { v = (v ?? "").Trim(); return v.Length == 0 || Regex.IsMatch(v, "^https?://", RegexOptions.IgnoreCase) ? v : "https://" + v; }

    // ================= persona =================
    /// <summary>Unvandan persona: yonetim / bt / surec / genel. Danisman kategorisinde hep "ortak".</summary>
    public static string Persona(string category, string title)
    {
        if (category == "danisman") return "ortak";
        string t = " " + Fold(title) + " ";
        string[] bt = { "bilgi işlem", "bilgi teknoloji", " it ", "it manager", "it director", "cio", "cto", "yazılım", "software", "dijital", "digital", "technology", "teknoloji", "sistem" };
        string[] sr = { "süreç", "process", "kalite", "quality", "operasyon", "operation", "iyileştirme", "improvement" };
        string[] yn = { "genel müdür", "general manager", "ceo", "kurucu", "founder", "yönetim kurulu", "owner", "sahibi", "managing director", "coo", "başkan", "president" };
        foreach (string k in bt) if (t.Contains(k)) return "bt";
        foreach (string k in sr) if (t.Contains(k)) return "surec";
        foreach (string k in yn) if (t.Contains(k)) return "yonetim";
        return "genel";
    }

    // ================= aday ekleme =================
    static string Cut(string s, int n) { s = (s ?? "").Trim(); return s.Length > n ? s.Substring(0, n) : s; }
    static object N(string s) { return string.IsNullOrEmpty(s) ? (object)DBNull.Value : s; }
    /// <summary>Tek aday: tekrar, engel, ucretsiz e-posta, dogrulanmamis ve firma basi sinir kontrolleriyle. Donus: eklendiyse null, degilse neden kodu.</summary>
    public static string Add(SqlConnection c, LdContact x, HashSet<string> blockedDomains, HashSet<string> blockedEmails, Dictionary<string, string> st, Dictionary<string, int> perDomain)
    {
        x.Email = (x.Email ?? "").Trim().ToLowerInvariant();
        if (!EmailRx.IsMatch(x.Email)) return "invalid";
        x.Domain = DomainOf(x.Email);
        if (Array.IndexOf(FreeMail, x.Domain) >= 0) return "freemail";
        if (blockedEmails.Contains(x.Email) || blockedDomains.Contains(x.Domain)) return "suppressed";
        if (x.Suppressed) return "suppressed";
        if (S(st, "onlyVerified", "1") == "1" && !string.IsNullOrEmpty(x.EmailStatus) && !IsVerified(x.EmailStatus)) return "unverified";
        using (SqlCommand q = new SqlCommand("SELECT COUNT(*) FROM dbo.LeadContacts WHERE Email = @e", c)) { q.Parameters.AddWithValue("@e", x.Email); if ((int)q.ExecuteScalar() > 0) return "duplicate"; }
        int have; if (!perDomain.TryGetValue(x.Domain, out have))
            using (SqlCommand q = new SqlCommand("SELECT COUNT(*) FROM dbo.LeadContacts WHERE Domain = @d", c)) { q.Parameters.AddWithValue("@d", x.Domain); have = (int)q.ExecuteScalar(); }
        if (have >= I(st, "maxPerCompany", 2)) return "companycap";
        // firma
        int coId;
        using (SqlCommand q = new SqlCommand("SELECT TOP 1 LeadCompanyId FROM dbo.LeadCompanies WHERE Domain = @d", c))
        { q.Parameters.AddWithValue("@d", x.Domain); object o = q.ExecuteScalar(); coId = o == null ? 0 : (int)o; }
        if (coId == 0)
            using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeadCompanies (Name, Domain, Website, Industry, Employees, City" + (HasDescription ? ", Description" : "") + ") VALUES (@n, @d, @w, @i, @e, @c" + (HasDescription ? ", @ds" : "") + "); SELECT CAST(SCOPE_IDENTITY() AS INT);", c))
            {
                q.Parameters.AddWithValue("@ds", N(Cut(x.Description, 1000)));
                q.Parameters.AddWithValue("@n", Cut(string.IsNullOrEmpty(x.CompanyName) ? x.Domain : x.CompanyName, 200)); q.Parameters.AddWithValue("@d", x.Domain); q.Parameters.AddWithValue("@w", N(Cut(x.Website, 300)));
                q.Parameters.AddWithValue("@i", N(Cut(x.Industry, 150))); q.Parameters.AddWithValue("@e", x.Employees.HasValue ? (object)x.Employees.Value : DBNull.Value); q.Parameters.AddWithValue("@c", N(Cut(x.City, 100)));
                coId = (int)q.ExecuteScalar();
            }
        if (string.IsNullOrEmpty(x.FirstName)) x.FirstName = (x.FullName ?? "").Split(' ')[0];
        x.Persona = Persona(x.Category, x.Title);
        using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeadContacts (LeadCompanyId, Category, FullName, FirstName, Title, Persona, Email, EmailStatus, Domain, LinkedIn, Source, ApolloId) VALUES (@co, @cat, @fn, @f, @t, @p, @e, @es, @d, @li, @src, @ap)", c))
        {
            q.Parameters.AddWithValue("@co", coId); q.Parameters.AddWithValue("@cat", x.Category); q.Parameters.AddWithValue("@fn", Cut(string.IsNullOrEmpty(x.FullName) ? x.Email : x.FullName, 150)); q.Parameters.AddWithValue("@f", N(Cut(x.FirstName, 80)));
            q.Parameters.AddWithValue("@t", N(Cut(x.Title, 200))); q.Parameters.AddWithValue("@p", x.Persona); q.Parameters.AddWithValue("@e", x.Email); q.Parameters.AddWithValue("@es", N(Cut(x.EmailStatus, 30))); q.Parameters.AddWithValue("@d", x.Domain);
            q.Parameters.AddWithValue("@li", N(Cut(x.LinkedIn, 300))); q.Parameters.AddWithValue("@src", x.Source); q.Parameters.AddWithValue("@ap", N(Cut(x.ApolloId, 60))); q.ExecuteNonQuery();
        }
        perDomain[x.Domain] = have + 1;
        return null;
    }
    static void Count(LdImport res, string why)
    {
        if (why == null) res.Added++; else if (why == "duplicate") res.Duplicate++; else if (why == "suppressed") res.Suppressed++; else if (why == "freemail") res.FreeMail++;
        else if (why == "unverified") res.Unverified++; else if (why == "companycap") res.CompanyCap++; else res.Invalid++;
    }

    // ================= CSV =================
    /// <summary>CSV / Excel'den kaydedilmis CSV. Apollo disa aktarma basliklari ve Turkce basliklar taninir; UTF-8 ya da Windows-1254.</summary>
    public static LdImport ImportCsv(Stream input, string category) { return ImportCsv(input, category, "csv"); }
    public static LdImport ImportCsv(Stream input, string category, string source)
    {
        LdImport res = new LdImport();
        List<LdContact> list = ReadCsv(input, category, source, res);
        if (list == null) return res;
        Dictionary<string, string> why; HashSet<string> bd = BlockedDomains(out why), be = BlockedEmails(); Dictionary<string, string> st = Settings(); Dictionary<string, int> per = new Dictionary<string, int>();
        using (SqlConnection c = Db.Open()) foreach (LdContact x in list) Count(res, Add(c, x, bd, be, st, per));
        return res;
    }
    /// <summary>CSV'yi aday listesine cevirir (veritabanina dokunmaz). Apollo, Instantly ve Turkce Excel basliklari.</summary>
    public static List<LdContact> ReadCsv(Stream input, string category, string source, LdImport res)
    {
        byte[] raw; using (MemoryStream ms = new MemoryStream()) { input.CopyTo(ms); raw = ms.ToArray(); }
        string text;
        try { text = new UTF8Encoding(false, true).GetString(raw); } catch (DecoderFallbackException) { text = Encoding.GetEncoding(1254).GetString(raw); }
        if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1);
        List<List<string>> rows = ParseCsv(text);
        if (rows.Count < 2) { res.Notes.Add("Dosyada başlık satırı ve en az bir kayıt olmalı."); return null; }
        // basliklar sadelestirilerek eslesir: "First Name", "firstName", "first_name" ayni sayilir (Apollo, Instantly, Excel)
        Func<string, string> norm = delegate (string h) { return Regex.Replace(Fold(h).Replace("\"", ""), @"[\s_\-]+", ""); };
        List<string> head = rows[0].ConvertAll(delegate (string h) { return norm(h); });
        Func<string[], int> col = delegate (string[] names) { foreach (string n in names) { int i = head.IndexOf(norm(n)); if (i >= 0) return i; } return -1; };
        int cFirst = col(new[] { "first name", "ad", "isim" }), cLast = col(new[] { "last name", "soyad", "soyadı" }), cName = col(new[] { "name", "full name", "ad soyad", "adı soyadı", "contact name" });
        int cTitle = col(new[] { "title", "job title", "position", "unvan", "ünvan", "görev", "pozisyon", "headline" }), cCo = col(new[] { "company", "company name", "organization", "organization name", "account name", "firma", "şirket", "firma adı" });
        int cMail = col(new[] { "email", "email address", "work email", "business email", "e-mail", "e-posta", "eposta", "mail" }), cStat = col(new[] { "email status", "email verification", "verification status", "verification", "e-posta durumu" });
        // ayni ada uyan birden fazla sutun (Instantly: "verification_status" kod, "Verification Status" metin): sondaki metin olani tercih et
        for (int k = head.Count - 1; cStat >= 0 && k > cStat; k--) if (head[k] == head[cStat]) { cStat = k; break; }
        int cSup = col(new[] { "is suppressed" }), cSub = col(new[] { "sub industry" }), cDesc = col(new[] { "company description", "description", "firma tanımı" });
        int cWeb = col(new[] { "website", "company website", "company domain", "domain", "web sitesi", "web" });
        int cInd = col(new[] { "industry", "company industry", "sektör" }), cEmp = col(new[] { "# employees", "employees", "employee count", "company size", "company headcount", "headcount", "çalışan sayısı", "çalışan" });
        int cCity = col(new[] { "city", "company city", "location", "person location", "şehir", "il" });
        int cLi = col(new[] { "person linkedin url", "linkedin url", "linkedin", "linkedin profile", "profile url" }), cAp = col(new[] { "apollo contact id", "apollo id" });
        if (cMail < 0) { res.Notes.Add("E-posta sütunu bulunamadı. Başlıklardan biri “Email” ya da “E-posta” olmalı."); return null; }
        Func<List<string>, int, string> v = delegate (List<string> r, int i) { return i >= 0 && i < r.Count ? Clean(r[i]) : ""; };
        List<LdContact> list = new List<LdContact>();
        for (int i = 1; i < rows.Count && i <= 5000; i++)
        {
            List<string> r = rows[i]; if (r.Count == 1 && r[0].Trim().Length == 0) continue;
            LdContact x = new LdContact(); x.Category = category; x.Source = source == "apollo" || source == "instantly" ? source : "csv";
            x.FirstName = v(r, cFirst); string last = v(r, cLast); x.FullName = v(r, cName); if (x.FullName.Length == 0) x.FullName = (x.FirstName + " " + last).Trim();
            x.Title = v(r, cTitle); x.CompanyName = v(r, cCo); x.Email = v(r, cMail); x.EmailStatus = v(r, cStat);
            string sub = v(r, cSub); x.Industry = sub.Length > 0 ? sub : v(r, cInd);   // alt sektor daha bilgilendirici
            x.City = CityOf(v(r, cCity)); x.Website = Url(v(r, cWeb)); x.LinkedIn = Url(v(r, cLi)); x.ApolloId = v(r, cAp); x.Employees = Employees(v(r, cEmp));
            x.Description = v(r, cDesc); x.Suppressed = Fold(v(r, cSup)) == "true";
            if (x.Email.Length == 0) { res.NoEmail++; continue; }
            list.Add(x);
        }
        if (rows.Count > 5001) res.Notes.Add("Tek seferde en fazla 5.000 satır işlenir; kalan satırlar alınmadı.");
        return list;
    }
    public static List<List<string>> ParseCsv(string text)
    {
        // ayirici: ilk satirda noktali virgul virgulden coksa ";" (Turkce Excel)
        int nl = text.IndexOf('\n'); string first = nl > 0 ? text.Substring(0, nl) : text;
        char sep = first.Split(';').Length > first.Split(',').Length ? ';' : ',';
        List<List<string>> rows = new List<List<string>>(); List<string> row = new List<string>(); StringBuilder f = new StringBuilder(); bool q = false;
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            if (q) { if (ch == '"') { if (i + 1 < text.Length && text[i + 1] == '"') { f.Append('"'); i++; } else q = false; } else f.Append(ch); }
            else if (ch == '"') q = true;
            else if (ch == sep) { row.Add(f.ToString()); f.Length = 0; }
            else if (ch == '\n' || ch == '\r') { if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++; row.Add(f.ToString()); f.Length = 0; rows.Add(row); row = new List<string>(); }
            else f.Append(ch);
        }
        if (f.Length > 0 || row.Count > 0) { row.Add(f.ToString()); rows.Add(row); }
        return rows;
    }

    // ================= Apollo =================
    static string ApiKey { get { return (WebConfigurationManager.AppSettings["Apollo.ApiKey"] ?? "").Trim(); } }
    static string SearchUrl { get { string u = (WebConfigurationManager.AppSettings["Apollo.SearchUrl"] ?? "").Trim(); return u.Length > 0 && !u.EndsWith("/mixed_people/search") ? u : "https://api.apollo.io/api/v1/mixed_people/api_search"; } }   // eski uc API icin kapatildi
    static string MatchUrl { get { string u = (WebConfigurationManager.AppSettings["Apollo.MatchUrl"] ?? "").Trim(); return u.Length > 0 ? u : "https://api.apollo.io/api/v1/people/match"; } }
    public static bool HasApollo { get { return ApiKey.Length > 0; } }
    static Dictionary<string, object> Post(string url, Dictionary<string, object> body, out string err)
    {
        err = null;
        try
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
            rq.Method = "POST"; rq.ContentType = "application/json"; rq.Accept = "application/json"; rq.Timeout = 30000; rq.Headers["X-Api-Key"] = ApiKey; rq.Headers["Cache-Control"] = "no-cache";
            byte[] b = Encoding.UTF8.GetBytes(JS.Serialize(body)); rq.ContentLength = b.Length;
            using (Stream s = rq.GetRequestStream()) s.Write(b, 0, b.Length);
            using (HttpWebResponse rs = (HttpWebResponse)rq.GetResponse()) using (StreamReader rd = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
                return JS.Deserialize<Dictionary<string, object>>(rd.ReadToEnd());
        }
        catch (WebException we)
        {
            string detail = "";
            try { if (we.Response != null) using (StreamReader rd = new StreamReader(we.Response.GetResponseStream())) detail = rd.ReadToEnd(); } catch { }
            int code = we.Response is HttpWebResponse ? (int)((HttpWebResponse)we.Response).StatusCode : 0;
            err = "Apollo " + (code > 0 ? code + " " : "") + "hatası: " + (detail.Length > 0 ? (detail.Length > 300 ? detail.Substring(0, 300) : detail) : we.Message);
            if (code == 401 || code == 403) err += " (API anahtarı geçersiz ya da planınız bu uca erişim vermiyor)";
            if (code == 429) err += " (istek sınırı aşıldı; biraz sonra tekrar deneyin)";
            return null;
        }
        catch (Exception ex) { err = "Apollo bağlantı hatası: " + ex.Message; return null; }
    }
    static Dictionary<string, object> Filters(string category, out string err)
    {
        err = null; Dictionary<string, string> st = Settings(); string json = S(st, category == "danisman" ? "apolloDanisman" : "apolloMusteri", "{}");
        try { return JS.Deserialize<Dictionary<string, object>>(json); } catch (Exception ex) { err = "Apollo filtre ayarı geçerli JSON değil: " + ex.Message; return null; }
    }
    static string Str(Dictionary<string, object> d, string k) { object o; return d != null && d.TryGetValue(k, out o) && o != null ? Convert.ToString(o, CultureInfo.InvariantCulture) : ""; }
    static Dictionary<string, object> Obj(Dictionary<string, object> d, string k) { object o; return d != null && d.TryGetValue(k, out o) ? o as Dictionary<string, object> : null; }
    /// <summary>Ad: "name" yoksa ad + soyad (yeni arama ucu soyadi gizli "last_name_obfuscated" verebilir)</summary>
    static string NameOf(Dictionary<string, object> p)
    {
        string n = Str(p, "name"); if (n.Length > 0) return n;
        string l = Str(p, "last_name"); if (l.Length == 0) l = Str(p, "last_name_obfuscated");
        return (Str(p, "first_name") + " " + l).Trim();
    }
    static int Total(Dictionary<string, object> r) { int t; if (int.TryParse(Str(Obj(r, "pagination"), "total_entries"), out t)) return t; return int.TryParse(Str(r, "total_entries"), out t) ? t : 0; }

    /// <summary>Baglanti testi: kategorinin filtreleriyle 3 kisilik arama. E-posta acmaz, kredi harcamaz.</summary>
    public static string Test(string category, out List<object> sample, out int total)
    {
        sample = new List<object>(); total = 0;
        if (!HasApollo) return "web.config içinde Apollo.ApiKey tanımlı değil.";
        string err; Dictionary<string, object> f = Filters(category, out err); if (f == null) return err;
        f["page"] = 1; f["per_page"] = 3;
        Dictionary<string, object> r = Post(SearchUrl, f, out err); if (r == null) return err;
        total = Total(r);
        object ps; if (r.TryGetValue("people", out ps) && ps is System.Collections.ArrayList)
            foreach (object o in (System.Collections.ArrayList)ps) { Dictionary<string, object> p = o as Dictionary<string, object>; if (p == null) continue; Dictionary<string, object> org = Obj(p, "organization"); sample.Add(new { name = NameOf(p), title = Str(p, "title"), company = Str(org, "name"), emailStatus = Str(p, "email_status") }); }
        return null;
    }

    /// <summary>Apollo'dan aday getir: once ucretsiz arama, sonra elemeden gecenlerin e-postasi acilir (kredi). want: eklenecek kisi sayisi.</summary>
    public static LdImport FromApollo(string category, int want)
    {
        LdImport res = new LdImport();
        if (!HasApollo) { res.Notes.Add("web.config içinde Apollo.ApiKey tanımlı değil."); return res; }
        string err; Dictionary<string, object> f = Filters(category, out err); if (f == null) { res.Notes.Add(err); return res; }
        Dictionary<string, string> why; HashSet<string> bd = BlockedDomains(out why), be = BlockedEmails(); Dictionary<string, string> st = Settings(); Dictionary<string, int> per = new Dictionary<string, int>();
        int startPage = I(st, "apolloPage" + category, 1);
        using (SqlConnection c = Db.Open())
            for (int page = startPage; page < startPage + 8 && res.Added < want && res.Credits < want * 2; page++)
            {
                f["page"] = page; f["per_page"] = 25;
                Dictionary<string, object> r = Post(SearchUrl, f, out err); if (r == null) { res.Notes.Add(err); break; }
                object ps; if (!r.TryGetValue("people", out ps) || !(ps is System.Collections.ArrayList) || ((System.Collections.ArrayList)ps).Count == 0) { res.Notes.Add("Filtrelere uyan yeni kişi kalmadı; sayfa başa alındı."); SaveSetting("apolloPage" + category, "1"); break; }
                foreach (object o in (System.Collections.ArrayList)ps)
                {
                    if (res.Added >= want || res.Credits >= want * 2) break;
                    Dictionary<string, object> p = o as Dictionary<string, object>; if (p == null) continue;
                    string apId = Str(p, "id"); Dictionary<string, object> org = Obj(p, "organization");
                    string dom = DomainOf(Str(org, "primary_domain")) ?? DomainOf(Str(org, "website_url"));
                    using (SqlCommand q = new SqlCommand("SELECT COUNT(*) FROM dbo.LeadContacts WHERE ApolloId = @a", c)) { q.Parameters.AddWithValue("@a", apId); if ((int)q.ExecuteScalar() > 0) { res.Duplicate++; continue; } }
                    // kredi harcamadan once ucretsiz elemeler (arama sonucunda domain varsa)
                    if (dom != null)
                    {
                        if (bd.Contains(dom)) { res.Suppressed++; continue; }
                        int have; if (!per.TryGetValue(dom, out have)) using (SqlCommand q = new SqlCommand("SELECT COUNT(*) FROM dbo.LeadContacts WHERE Domain = @d", c)) { q.Parameters.AddWithValue("@d", dom); have = (int)q.ExecuteScalar(); per[dom] = have; }
                        if (have >= I(st, "maxPerCompany", 2)) { res.CompanyCap++; continue; }
                    }
                    string email = Str(p, "email"), status = Str(p, "email_status");
                    if (email.Length == 0 || email.Contains("not_unlocked") || email.Contains("email_not")) // e-posta kilitli: ac (kredi)
                    {
                        Dictionary<string, object> body = new Dictionary<string, object>(); body["id"] = apId; body["reveal_personal_emails"] = false;
                        Dictionary<string, object> m = Post(MatchUrl, body, out err); res.Credits++;
                        if (m == null) { res.Notes.Add(err); break; }
                        Dictionary<string, object> mp = Obj(m, "person");
                        if (mp != null)
                        {
                            email = Str(mp, "email"); status = Str(mp, "email_status");
                            // yeni arama ucu bazi alanlari vermiyor: acilan kayit asil kaynak
                            foreach (KeyValuePair<string, object> kv in mp) if (kv.Value != null && (!p.ContainsKey(kv.Key) || Str(p, kv.Key).Length == 0 || kv.Key == "last_name" || kv.Key == "name")) p[kv.Key] = kv.Value;
                            Dictionary<string, object> morg = Obj(mp, "organization"); if (morg != null) org = morg;
                            if (dom == null) { dom = DomainOf(Str(org, "primary_domain")) ?? DomainOf(Str(org, "website_url")); if (dom != null && bd.Contains(dom)) { res.Suppressed++; res.CreditsWasted++; continue; } }
                        }
                    }
                    if (email.Length == 0 || email.Contains("not_unlocked")) { res.NoEmail++; continue; }
                    LdContact x = new LdContact(); x.Category = category; x.Source = "apollo"; x.ApolloId = apId; x.FullName = NameOf(p); x.FirstName = Str(p, "first_name"); x.Title = Str(p, "title");
                    x.Email = email; x.EmailStatus = status; x.LinkedIn = Str(p, "linkedin_url"); x.CompanyName = Str(org, "name"); x.Industry = Str(org, "industry"); x.City = Str(org, "city"); x.Website = Str(org, "website_url");
                    int emp; if (int.TryParse(Str(org, "estimated_num_employees"), out emp) && emp > 0) x.Employees = emp;
                    Count(res, Add(c, x, bd, be, st, per));
                }
                SaveSetting("apolloPage" + category, (page + 1).ToString(CultureInfo.InvariantCulture));   // sonraki cagri kaldigi sayfadan devam eder
            }
        return res;
    }

    // ================= havuz =================
    public static List<LdContact> Contacts(string category, string status, string q, int max) { int total; return ContactsPage(category, status, q, 0, max, false, out total); }
    /// <summary>Sayfali havuz: skip kadar atlayip size kadar kayit; count=true ise suzgece uyan toplam da doner.</summary>
    public static List<LdContact> ContactsPage(string category, string status, string q, int skip, int size, bool count, out int total)
    {
        List<LdContact> l = new List<LdContact>(); total = 0;
        string w = "1=1"; if (category == "musteri" || category == "danisman") w += " AND ct.Category = @cat"; if (!string.IsNullOrEmpty(status)) w += " AND ct.Status = @st";
        if (!string.IsNullOrEmpty(q)) w += " AND (ct.FullName LIKE @q OR ct.Email LIKE @q OR ct.Title LIKE @q OR co.Name LIKE @q)";
        using (SqlConnection c = Db.Open())
        {
        if (count)
            using (SqlCommand cq = new SqlCommand("SELECT COUNT(*) FROM dbo.LeadContacts ct LEFT JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE " + w, c))
            {
                cq.Parameters.AddWithValue("@cat", category ?? ""); cq.Parameters.AddWithValue("@st", status ?? ""); cq.Parameters.AddWithValue("@q", "%" + (q ?? "") + "%");
                total = (int)cq.ExecuteScalar();
            }
        using (SqlCommand cmd = new SqlCommand("SELECT ct.ContactId, ct.Category, ct.FullName, ct.FirstName, ct.Title, ct.Persona, ct.Email, ct.EmailStatus, ct.Domain, ct.LinkedIn, ct.Source, ct.Status, ct.Note, ct.Step, ct.CreatedAt, co.Name, co.Industry, co.City, co.Employees, " + (HasDescription ? "co.Description" : "NULL") + ", " + (HasGender ? "ct.Gender, ct.GenderSure" : "NULL, CAST(0 AS BIT)") + " " +
            "FROM dbo.LeadContacts ct LEFT JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE " + w + " ORDER BY ct.CreatedAt DESC, ct.ContactId DESC OFFSET @k ROWS FETCH NEXT @m ROWS ONLY", c))
        {
            cmd.Parameters.AddWithValue("@k", Math.Max(0, skip)); cmd.Parameters.AddWithValue("@m", Math.Max(1, size)); cmd.Parameters.AddWithValue("@cat", category ?? ""); cmd.Parameters.AddWithValue("@st", status ?? ""); cmd.Parameters.AddWithValue("@q", "%" + (q ?? "") + "%");
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    LdContact x = new LdContact(); x.Id = r.GetInt32(0); x.Category = r.GetString(1); x.FullName = r.GetString(2); x.FirstName = r.IsDBNull(3) ? "" : r.GetString(3); x.Title = r.IsDBNull(4) ? "" : r.GetString(4);
                    x.Persona = r.IsDBNull(5) ? "" : r.GetString(5); x.Email = r.GetString(6); x.EmailStatus = r.IsDBNull(7) ? "" : r.GetString(7); x.Domain = r.IsDBNull(8) ? "" : r.GetString(8); x.LinkedIn = r.IsDBNull(9) ? "" : r.GetString(9);
                    x.Source = r.GetString(10); x.Status = r.GetString(11); x.Note = r.IsDBNull(12) ? "" : r.GetString(12); x.Step = r.GetInt32(13); x.CreatedAt = r.GetDateTime(14);
                    x.CompanyName = r.IsDBNull(15) ? "" : r.GetString(15); x.Industry = r.IsDBNull(16) ? "" : r.GetString(16); x.City = r.IsDBNull(17) ? "" : r.GetString(17); x.Employees = r.IsDBNull(18) ? (int?)null : r.GetInt32(18);
                    x.Description = r.IsDBNull(19) ? "" : r.GetString(19);
                    x.Gender = r.IsDBNull(20) ? null : r.GetString(20); x.GenderSure = !r.IsDBNull(21) && r.GetBoolean(21);
                    l.Add(x);
                }
        }
        }
        return l;
    }
    public static Dictionary<string, int> Counts()
    {
        Dictionary<string, int> d = new Dictionary<string, int>();
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT Category + N'.' + Status, COUNT(*) FROM dbo.LeadContacts GROUP BY Category, Status", c)) using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) d[r.GetString(0)] = r.GetInt32(1);
            using (SqlCommand q = new SqlCommand("SELECT N'msg.' + Status, COUNT(*) FROM dbo.LeadMessages GROUP BY Status", c)) using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) d[r.GetString(0)] = r.GetInt32(1);
        }
        return d;
    }
    public static string ContactEmail(int id)
    {
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT Email FROM dbo.LeadContacts WHERE ContactId = @i", c)) { q.Parameters.AddWithValue("@i", id); object o = q.ExecuteScalar(); return o as string; }
    }
    /// <summary>Genel bakis panosu: gostergeler, 30 gunluk akis, huni ve dagilimlar (tek baglantida birkac toplu sorgu)</summary>
    public static Dictionary<string, object> Dashboard()
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        Func<SqlConnection, string, List<object>> pairs = delegate (SqlConnection c, string sql)
        {
            List<object> l = new List<object>();
            using (SqlCommand q = new SqlCommand(sql, c)) using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) l.Add(new object[] { r.IsDBNull(0) ? "" : Convert.ToString(r[0]), r.GetInt32(1) });
            return l;
        };
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand(
                "SELECT (SELECT COUNT(*) FROM dbo.LeadContacts), (SELECT COUNT(*) FROM dbo.LeadContacts WHERE Status <> N'excluded'), " +
                "(SELECT COUNT(*) FROM dbo.LeadContacts WHERE CreatedAt >= DATEADD(DAY, -7, SYSUTCDATETIME())), " +
                "(SELECT COUNT(DISTINCT ContactId) FROM dbo.LeadMessages), (SELECT COUNT(DISTINCT ContactId) FROM dbo.LeadMessages WHERE Status IN (N'approved', N'sent')), " +
                "(SELECT COUNT(DISTINCT ContactId) FROM dbo.LeadMessages WHERE Status = N'sent'), (SELECT COUNT(*) FROM dbo.LeadMessages WHERE Status = N'sent'), " +
                "(SELECT COUNT(*) FROM dbo.LeadContacts WHERE Status IN (N'replied', N'interested', N'later', N'notinterested')), (SELECT COUNT(*) FROM dbo.LeadContacts WHERE Status = N'interested'), " +
                "(SELECT COUNT(*) FROM dbo.LeadMessages WHERE Status = N'draft'), (SELECT COUNT(*) FROM dbo.LeadContacts WHERE Status = N'bounced'), " +
                "(SELECT COUNT(*) FROM dbo.LeadContacts WHERE Source = N'apollo' AND CreatedAt >= DATEADD(DAY, 1 - DAY(GETDATE()), CAST(GETDATE() AS DATE)))", c))
            using (SqlDataReader r = q.ExecuteReader())
                if (r.Read())
                {
                    d["total"] = r.GetInt32(0); d["active"] = r.GetInt32(1); d["week"] = r.GetInt32(2); d["drafted"] = r.GetInt32(3); d["approved"] = r.GetInt32(4);
                    d["sentContacts"] = r.GetInt32(5); d["sentMails"] = r.GetInt32(6); d["replied"] = r.GetInt32(7); d["interested"] = r.GetInt32(8); d["pending"] = r.GetInt32(9); d["bounced"] = r.GetInt32(10); d["apolloMonth"] = r.GetInt32(11);
                }
            // son 30 gun: gun basina eklenen (kategoriye gore), gonderilen, yanit
            List<object> days = new List<object>();
            Dictionary<string, int[]> map = new Dictionary<string, int[]>();
            DateTime t0 = DateTime.UtcNow.ToTrLocal().Date.AddDays(-29);
            for (int i = 0; i < 30; i++) map[t0.AddDays(i).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)] = new int[4];
            string[] sqls = {
                "SELECT CONVERT(char(10), CAST(SWITCHOFFSET(CAST(CreatedAt AS DATETIMEOFFSET), '+03:00') AS DATE), 23), Category, COUNT(*) FROM dbo.LeadContacts WHERE CreatedAt >= DATEADD(DAY, -31, SYSUTCDATETIME()) GROUP BY CONVERT(char(10), CAST(SWITCHOFFSET(CAST(CreatedAt AS DATETIMEOFFSET), '+03:00') AS DATE), 23), Category",
                "SELECT CONVERT(char(10), CAST(SWITCHOFFSET(CAST(SentAt AS DATETIMEOFFSET), '+03:00') AS DATE), 23), N'sent', COUNT(*) FROM dbo.LeadMessages WHERE Status = N'sent' AND SentAt >= DATEADD(DAY, -31, SYSUTCDATETIME()) GROUP BY CONVERT(char(10), CAST(SWITCHOFFSET(CAST(SentAt AS DATETIMEOFFSET), '+03:00') AS DATE), 23)",
                "SELECT CONVERT(char(10), CAST(SWITCHOFFSET(CAST(ReceivedAt AS DATETIMEOFFSET), '+03:00') AS DATE), 23), N'reply', COUNT(*) FROM dbo.LeadReplies WHERE ReceivedAt >= DATEADD(DAY, -31, SYSUTCDATETIME()) GROUP BY CONVERT(char(10), CAST(SWITCHOFFSET(CAST(ReceivedAt AS DATETIMEOFFSET), '+03:00') AS DATE), 23)" };
            foreach (string sql in sqls)
                using (SqlCommand q = new SqlCommand(sql, c)) using (SqlDataReader r = q.ExecuteReader())
                    while (r.Read())
                    {
                        int[] a; if (!map.TryGetValue(r.GetString(0), out a)) continue; string k = r.GetString(1);
                        a[k == "musteri" ? 0 : k == "danisman" ? 1 : k == "sent" ? 2 : 3] += r.GetInt32(2);
                    }
            foreach (KeyValuePair<string, int[]> kv in map) days.Add(new object[] { kv.Key, kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3] });
            d["days"] = days;
            const string act = " WHERE Status <> N'excluded'";
            d["persona"] = pairs(c, "SELECT Category + N'.' + ISNULL(Persona, N'genel'), COUNT(*) FROM dbo.LeadContacts" + act + " GROUP BY Category, Persona ORDER BY COUNT(*) DESC");
            d["industry"] = pairs(c, "SELECT TOP 8 co.Industry, COUNT(*) FROM dbo.LeadContacts ct JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE ct.Status <> N'excluded' AND ISNULL(co.Industry, N'') <> N'' GROUP BY co.Industry ORDER BY COUNT(*) DESC");
            d["city"] = pairs(c, "SELECT TOP 8 co.City, COUNT(*) FROM dbo.LeadContacts ct JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE ct.Status <> N'excluded' AND ISNULL(co.City, N'') <> N'' GROUP BY co.City ORDER BY COUNT(*) DESC");
            d["size"] = pairs(c, "SELECT b, COUNT(*) FROM (SELECT CASE WHEN co.Employees IS NULL THEN N'Bilinmiyor' WHEN co.Employees <= 50 THEN N'1-50' WHEN co.Employees <= 200 THEN N'51-200' WHEN co.Employees <= 500 THEN N'201-500' WHEN co.Employees <= 1000 THEN N'501-1.000' ELSE N'1.000+' END AS b " +
                                "FROM dbo.LeadContacts ct LEFT JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE ct.Status <> N'excluded') x GROUP BY b");
            d["source"] = pairs(c, "SELECT Source, COUNT(*) FROM dbo.LeadContacts" + act + " GROUP BY Source ORDER BY COUNT(*) DESC");
            d["status"] = pairs(c, "SELECT Status, COUNT(*) FROM dbo.LeadContacts GROUP BY Status ORDER BY COUNT(*) DESC");
        }
        return d;
    }

    public static void Exclude(int id, string reason)
    {
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadContacts SET Status = N'excluded', Note = @n, UpdatedAt = SYSUTCDATETIME() WHERE ContactId = @i; UPDATE dbo.LeadMessages SET Status = N'cancelled' WHERE ContactId = @i AND Status IN (N'draft', N'approved')", c))
        { q.Parameters.AddWithValue("@i", id); q.Parameters.AddWithValue("@n", Cut(string.IsNullOrEmpty(reason) ? "Elle çıkarıldı" : reason, 400)); q.ExecuteNonQuery(); }
    }

    // ================= sablon ve taslak =================
    public static List<object> Templates()
    {
        List<object> l = new List<object>();
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT TemplateId, Category, Persona, StepNo, Subject, Body, IsActive FROM dbo.LeadTemplates ORDER BY Category DESC, StepNo, Persona", c)) using (SqlDataReader r = q.ExecuteReader())
            while (r.Read()) l.Add(new { id = r.GetInt32(0), category = r.GetString(1), persona = r.GetString(2), step = r.GetInt32(3), subject = r.GetString(4), body = r.GetString(5), active = r.GetBoolean(6) });
        return l;
    }
    public static string SaveTemplate(int id, string subject, string body, bool active)
    {
        subject = Cut(subject, 200); body = (body ?? "").Replace("\r\n", "\n").Trim();
        if (subject.Length == 0 || body.Length < 20) return "Konu ve en az 20 karakterlik metin gerekli.";
        if (Regex.IsMatch(body, @"<\s*(script|iframe|object|embed|form)\b", RegexOptions.IgnoreCase)) return "E-posta metninde script, iframe, form gibi etiketler kullanılamaz.";
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadTemplates SET Subject = @s, Body = @b, IsActive = @a, UpdatedAt = SYSUTCDATETIME() WHERE TemplateId = @i", c))
        { q.Parameters.AddWithValue("@s", subject); q.Parameters.AddWithValue("@b", body); q.Parameters.AddWithValue("@a", active); q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); }
        return null;
    }
    static readonly Regex HtmlRx = new Regex(@"<\s*/?\s*[a-z][a-z0-9]*(\s[^>]*)?>", RegexOptions.IgnoreCase);
    /// <summary>Metin HTML mi? Etiket iceriyorsa HTML, yoksa duz metin.</summary>
    public static bool IsHtml(string s) { return !string.IsNullOrEmpty(s) && HtmlRx.IsMatch(s); }
    /// <summary>Gonderim govdesi: duz metin paragraflara cevrilir; HTML oldugu gibi. Imza ve ret cumlesi sonuna eklenir.</summary>
    public static string ToHtml(string body, string signature, string optOut)
    {
        Func<string, string> txt = delegate (string t)
        {
            string e = System.Web.HttpUtility.HtmlEncode((t ?? "").Replace("\r\n", "\n").Trim());
            return e.Length == 0 ? "" : "<p style=\"margin:0 0 14px\">" + Regex.Replace(e, "\n{2,}", "</p><p style=\"margin:0 0 14px\">").Replace("\n", "<br>") + "</p>";
        };
        string b = IsHtml(body) ? body : txt(body);
        return "<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14.5px;line-height:1.6;color:#1B2440\">" + b + txt(signature) +
               (string.IsNullOrEmpty(optOut) ? "" : "<p style=\"margin:18px 0 0;font-size:12px;color:#8A90AE\">" + System.Web.HttpUtility.HtmlEncode(optOut) + "</p>") + "</div>";
    }
    /// <summary>Yer tutucular: {Hitap} ("Ad Bey" / "Ad Hanim") {Ad} {Firma} {Unvan} {Giris}. HTML sablonda yerine konan degerler kodlanir. Giris Claude ile yazilir (bos ise satir kaldirilir).</summary>
    public static string Fill(string t, LdContact x, string giris) { return Fill(t, x, giris, null); }
    public static string Fill(string t, LdContact x, string giris, string hitap)
    {
        bool html = IsHtml(t);
        Func<string, string> v = delegate (string s0) { return html ? System.Web.HttpUtility.HtmlEncode(s0 ?? "") : (s0 ?? ""); };
        string s = (t ?? "").Replace("{Hitap}", v(string.IsNullOrEmpty(hitap) ? Hitap(x) : hitap)).Replace("{Ad}", v(string.IsNullOrEmpty(x.FirstName) ? x.FullName : x.FirstName)).Replace("{Firma}", v(string.IsNullOrEmpty(x.CompanyName) ? x.Domain : x.CompanyName))
                            .Replace("{Unvan}", v(x.Title)).Replace("{Giris}", html && !string.IsNullOrEmpty(giris) ? "<p>" + v(giris) + "</p>" : (giris ?? ""));
        if (html) s = Regex.Replace(s, @"<p>\s*</p>", "");   // bos giris paragrafi
        else s = Regex.Replace(s.Replace("\r\n", "\n"), "\n{3,}", "\n\n");
        return s.Trim();
    }
    /// <summary>Yeni adaylardan ilk e-posta taslaklari (gunluk sinir: dailyNew). Kategoriye gore posta kutusu; persona sablonu yoksa "genel".</summary>
    public static int CreateDrafts(string category, int max)
    {
        Dictionary<string, string> st = Settings(); int made = 0;
        Dictionary<string, KeyValuePair<string, string>> tpl = new Dictionary<string, KeyValuePair<string, string>>();
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT Category, Persona, Subject, Body FROM dbo.LeadTemplates WHERE StepNo = 0 AND IsActive = 1", c)) using (SqlDataReader r = q.ExecuteReader())
            while (r.Read()) tpl[r.GetString(0) + "." + r.GetString(1)] = new KeyValuePair<string, string>(r.GetString(2), r.GetString(3));
        bool pers = S(st, "personalize", "1") == "1" && LeadClaude.Configured;   // Claude: hitap ve kisisel giris cumlesi
        foreach (LdContact x in Contacts(category, "new", null, max))
        {
            KeyValuePair<string, string> t;
            if (!tpl.TryGetValue(x.Category + "." + x.Persona, out t) && !tpl.TryGetValue(x.Category + ".genel", out t) && !tpl.TryGetValue(x.Category + ".ortak", out t)) continue;
            string box = S(st, x.Category == "danisman" ? "mailboxDanisman" : "mailboxMusteri", "");
            using (SqlConnection c = Db.Open())
            using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeadMessages (ContactId, StepNo, Mailbox, Subject, Body, Status) VALUES (@c, 0, @m, @s, @b, N'draft'); UPDATE dbo.LeadContacts SET Status = N'queued', UpdatedAt = SYSUTCDATETIME() WHERE ContactId = @c", c))
            {
                string giris = null, aiG = null; bool aiSure = false, sure; if (pers) LeadClaude.Personalize(x, out aiG, out aiSure, out giris);
                string g = ResolveGender(x, out sure);
                if (g == null) { g = aiG ?? "E"; sure = false; }   // listede yok ya da iki cinsiyette de kullaniliyor: tahmin, kuyrukta "kontrol edin"
                SaveGender(x.Id, g, sure); x.Gender = g; x.GenderSure = sure;
                string hitap = HitapOf(FirstOf(x), g);
                q.Parameters.AddWithValue("@c", x.Id); q.Parameters.AddWithValue("@m", box); q.Parameters.AddWithValue("@s", Cut(Fill(t.Key, x, giris, hitap), 200)); q.Parameters.AddWithValue("@b", Fill(t.Value, x, giris, hitap)); q.ExecuteNonQuery();
            }
            made++;
        }
        return made;
    }
    public static List<LdMessage> Queue(string status, int max)
    {
        List<LdMessage> l = new List<LdMessage>();
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT TOP (@m) m.MessageId, m.ContactId, m.StepNo, m.Mailbox, m.Subject, m.Body, m.Status, m.Error, m.CreatedAt, m.ScheduledAt, m.SentAt, ct.FullName, ct.Title, co.Name, ct.Email, ct.Category, ct.Persona, ct.FirstName, " + (HasGender ? "ct.Gender, ct.GenderSure" : "NULL, CAST(0 AS BIT)") + " " +
            "FROM dbo.LeadMessages m JOIN dbo.LeadContacts ct ON ct.ContactId = m.ContactId LEFT JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE m.Status = @s ORDER BY " + (status == "sent" ? "m.SentAt DESC" : "m.CreatedAt, m.MessageId"), c))
        {
            q.Parameters.AddWithValue("@m", max); q.Parameters.AddWithValue("@s", status);
            using (SqlDataReader r = q.ExecuteReader())
                while (r.Read())
                {
                    LdMessage x = new LdMessage(); x.Id = r.GetInt32(0); x.ContactId = r.GetInt32(1); x.StepNo = r.GetInt32(2); x.Mailbox = r.GetString(3); x.Subject = r.GetString(4); x.Body = r.GetString(5); x.Status = r.GetString(6);
                    x.Error = r.IsDBNull(7) ? null : r.GetString(7); x.CreatedAt = r.GetDateTime(8); x.ScheduledAt = r.IsDBNull(9) ? (DateTime?)null : r.GetDateTime(9); x.SentAt = r.IsDBNull(10) ? (DateTime?)null : r.GetDateTime(10);
                    x.ContactName = r.GetString(11); x.ContactTitle = r.IsDBNull(12) ? "" : r.GetString(12); x.CompanyName = r.IsDBNull(13) ? "" : r.GetString(13); x.Email = r.GetString(14); x.Category = r.GetString(15); x.Persona = r.IsDBNull(16) ? "" : r.GetString(16);
                    x.FirstName = r.IsDBNull(17) ? "" : r.GetString(17); x.Gender = r.IsDBNull(18) ? null : r.GetString(18); x.GenderSure = !r.IsDBNull(19) && r.GetBoolean(19);
                    l.Add(x);
                }
        }
        return l;
    }
    /// <summary>Taslak islemleri: kaydet (konu/metin), onayla, iptal (aday da havuzdan cikar)</summary>
    public static string MessageOp(int id, string op, string subject, string body, int by)
    {
        using (SqlConnection c = Db.Open())
        {
            string st; int cid;
            using (SqlCommand q = new SqlCommand("SELECT Status, ContactId FROM dbo.LeadMessages WHERE MessageId = @i", c)) { q.Parameters.AddWithValue("@i", id); using (SqlDataReader r = q.ExecuteReader()) { if (!r.Read()) return "Taslak bulunamadı."; st = r.GetString(0); cid = r.GetInt32(1); } }
            if (st != "draft" && st != "approved") return "Bu e-posta artık düzenlenemez.";
            if (op == "save" || op == "approve")
            {
                subject = Cut(subject, 200); body = (body ?? "").Replace("\r\n", "\n").Trim();
                if (subject.Length == 0 || body.Length < 20) return "Konu ve metin boş olamaz.";
                if (Regex.IsMatch(body, @"<\s*(script|iframe|object|embed|form)\b", RegexOptions.IgnoreCase)) return "E-posta metninde script, iframe, form gibi etiketler kullanılamaz.";
                if (body.Contains("{") && Regex.IsMatch(body, @"\{(Hitap|Ad|Firma|Unvan|Giris|Konu)\}")) return "Metinde doldurulmamış yer tutucu var: " + Regex.Match(body, @"\{(Hitap|Ad|Firma|Unvan|Giris|Konu)\}").Value;
                using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadMessages SET Subject = @s, Body = @b" + (op == "approve" ? ", Status = N'approved', ApprovedBy = @by, ApprovedAt = SYSUTCDATETIME()" : "") + " WHERE MessageId = @i", c))
                { q.Parameters.AddWithValue("@s", subject); q.Parameters.AddWithValue("@b", body); q.Parameters.AddWithValue("@by", by); q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); }
                return null;
            }
            if (op == "unapprove") { using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadMessages SET Status = N'draft', ApprovedBy = NULL, ApprovedAt = NULL WHERE MessageId = @i AND Status = N'approved'", c)) { q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); } return null; }
            if (op == "cancel") { Exclude(cid, "Taslak iptal edildi"); return null; }
            return "Geçersiz işlem.";
        }
    }
    /// <summary>Hatali gonderimi yeniden kuyruga al</summary>
    public static void Retry(int id) { using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadMessages SET Status = N'approved', Error = NULL WHERE MessageId = @i AND Status = N'failed'", c)) { q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); } }

    // ================= yanitlar =================
    public static List<object> Replies(int max)
    {
        List<object> l = new List<object>();
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT TOP (@m) r.ReplyId, r.ContactId, r.Mailbox, r.ReceivedAt, r.FromEmail, r.Subject, ISNULL(r.Body, r.Preview), r.Class, r.Summary, r.Handled, ct.FullName, ct.Title, ISNULL(co.Name, N''), ct.Category, ct.Status, " +
            "(SELECT TOP 1 Subject FROM dbo.LeadMessages m WHERE m.ContactId = r.ContactId AND m.Status = N'sent' ORDER BY m.SentAt DESC), (SELECT COUNT(*) FROM dbo.LeadMessages m WHERE m.ReplyToId = r.ReplyId AND m.Status = N'sent') " +
            "FROM dbo.LeadReplies r JOIN dbo.LeadContacts ct ON ct.ContactId = r.ContactId LEFT JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId ORDER BY r.Handled, r.ReceivedAt DESC", c))
        {
            q.Parameters.AddWithValue("@m", max);
            using (SqlDataReader r = q.ExecuteReader())
                while (r.Read())
                    l.Add(new { id = r.GetInt32(0), contactId = r.GetInt32(1), mailbox = r.GetString(2), at = r.GetDateTime(3).ToTrLocal().ToString("d MMM ddd HH:mm", TR), from = r.IsDBNull(4) ? "" : r.GetString(4),
                                subject = r.IsDBNull(5) ? "" : r.GetString(5), body = r.IsDBNull(6) ? "" : r.GetString(6), cls = r.IsDBNull(7) ? "other" : r.GetString(7), summary = r.IsDBNull(8) ? "" : r.GetString(8), handled = r.GetBoolean(9),
                                name = r.GetString(10), title = r.IsDBNull(11) ? "" : r.GetString(11), company = r.GetString(12), category = r.GetString(13), status = r.GetString(14), sentSubject = r.IsDBNull(15) ? "" : r.GetString(15), answered = r.GetInt32(16) });
        }
        return l;
    }
    public static string ReplyOp(int id, string op, string cls, string body, int by)
    {
        int cid;
        using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT ContactId FROM dbo.LeadReplies WHERE ReplyId = @i", c)) { q.Parameters.AddWithValue("@i", id); object o = q.ExecuteScalar(); if (o == null) return "Yanıt bulunamadı."; cid = (int)o; }
        if (op == "class")
        {
            if (Array.IndexOf(LeadClaude.Classes, cls) < 0) return "Geçersiz sınıf.";
            using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadReplies SET Class = @c WHERE ReplyId = @i", c)) { q.Parameters.AddWithValue("@c", cls); q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); }
            LeadEngine.Apply(cid, cls); return null;
        }
        if (op == "handled" || op == "unhandled")
        {
            using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadReplies SET Handled = @h WHERE ReplyId = @i", c)) { q.Parameters.AddWithValue("@h", op == "handled"); q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); }
            return null;
        }
        if (op == "send")
        {
            body = (body ?? "").Replace("\r\n", "\n").Trim(); if (body.Length < 5) return "Yanıt metni boş olamaz.";
            if (Regex.IsMatch(body, @"<\s*(script|iframe|object|embed|form)\b", RegexOptions.IgnoreCase)) return "E-posta metninde script, iframe, form gibi etiketler kullanılamaz.";
            if (!LeadGraph.Configured) return "Gönderim için web.config içinde Graph ayarları tanımlı olmalı.";
            string mb, subj; using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT Mailbox, ISNULL(Subject, N'') FROM dbo.LeadReplies WHERE ReplyId = @i", c)) { q.Parameters.AddWithValue("@i", id); using (SqlDataReader r = q.ExecuteReader()) { r.Read(); mb = r.GetString(0); subj = r.GetString(1); } }
            int mid;
            using (SqlConnection c = Db.Open())
            using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeadMessages (ContactId, StepNo, Mailbox, Subject, Body, Status, ApprovedBy, ApprovedAt, ReplyToId) VALUES (@c, 9, @m, @s, @b, N'approved', @by, SYSUTCDATETIME(), @r); SELECT CAST(SCOPE_IDENTITY() AS INT);", c))
            { q.Parameters.AddWithValue("@c", cid); q.Parameters.AddWithValue("@m", mb); q.Parameters.AddWithValue("@s", Cut(subj.StartsWith("Re:") ? subj : "Re: " + subj, 200)); q.Parameters.AddWithValue("@b", body); q.Parameters.AddWithValue("@by", by); q.Parameters.AddWithValue("@r", id); mid = (int)q.ExecuteScalar(); }
            string err = LeadEngine.Send(mid);   // insan yaniti: mesai penceresi ve gunluk sinir beklenmez
            if (err != null) return "Yanıt gönderilemedi: " + err;
            using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeadReplies SET Handled = 1 WHERE ReplyId = @i", c)) { q.Parameters.AddWithValue("@i", id); q.ExecuteNonQuery(); }
            return null;
        }
        return "Geçersiz işlem.";
    }
}
