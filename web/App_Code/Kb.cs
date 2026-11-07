using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;

// Bilgi tabani (River Akademi'den cikarilan konu kayitlari) ve destek talebi AI on yaniti.
// Kurallar: AI yalniz bulunan kayitlara dayanir, bulamazsa "bulunamadi" der; kullaniciyi asla engellemez.
public class KbEntry
{
    public int Id; public string Code, Product, Area, Title, Summary, Keywords, Phrases, YoutubeId, VideoTitle; public int? MaterialId; public int StartSec, HitCount; public bool IsActive;
    public List<KbStep> Steps = new List<KbStep>(); public List<string> Warnings = new List<string>();
    public Dictionary<string, double> Terms;   // arama dizini (kok -> agirlik)
    public string Link { get { return KbLink(StartSec); } }
    public string KbLink(int t) { return MaterialId.HasValue && !string.IsNullOrEmpty(YoutubeId) ? "Training.aspx?play=" + MaterialId.Value + "&v=" + Uri.EscapeDataString(YoutubeId) + "&t=" + t : null; }
}
public class KbStep { public int t; public string text; }

public static class Kb
{
    static readonly CultureInfo TR = new CultureInfo("tr-TR");
    static readonly JavaScriptSerializer JS = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };
    static readonly object Lck = new object();
    static List<KbEntry> cache; static Dictionary<string, double> idf;

    /// <summary>web.config Kb.PreAnswer (varsayilan acik)</summary>
    public static bool Enabled { get { string v = (WebConfigurationManager.AppSettings["Kb.PreAnswer"] ?? "1").Trim(); return v != "0" && !v.Equals("false", StringComparison.OrdinalIgnoreCase); } }
    public static string Ts(int s) { return (s / 60) + ":" + (s % 60).ToString("00"); }

    // ---------- arama ----------
    static readonly HashSet<string> Stop = new HashSet<string>(("ve veya ile icin gibi bir bu su o da de mi mu mi ne nasil neden niye ama fakat cok daha en hem ise ki olarak olan oldu olur olmuyor olmadi var yok degil " +
        "ben biz siz onlar bana bize sana ona benim bizim sizin icinde uzerinde sonra once kadar gore her hep ayni diger baska yapmak yapiyorum yapamiyorum istiyorum lazim gerek gerekiyor " +
        "merhaba selam tesekkurler rica ederim lutfen sayin iyi gunler calismalar acaba sekilde seklinde zaman simdi hala artik bazen tum hic").Split(' '));
    public static string Fold(string s)
    {
        s = (s ?? "").ToLower(TR);
        StringBuilder b = new StringBuilder(s.Length);
        foreach (char c in s)
        {
            char d = c == 'ç' ? 'c' : c == 'ğ' ? 'g' : c == 'ı' ? 'i' : c == 'ö' ? 'o' : c == 'ş' ? 's' : c == 'ü' ? 'u' : c == 'â' ? 'a' : c == 'î' ? 'i' : c == 'û' ? 'u' : c;
            b.Append(char.IsLetterOrDigit(d) ? d : ' ');
        }
        return b.ToString();
    }
    static string Stem(string w) { return w.Length > 5 ? w.Substring(0, 5) : w; }   // Turkce ekler icin kaba kok
    public static List<string> Terms(string s)
    {
        List<string> l = new List<string>();
        foreach (string w in Fold(s).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            if (w.Length >= 2 && !Stop.Contains(w)) l.Add(Stem(w));
        return l;
    }
    static void AddTerms(Dictionary<string, double> d, string text, double w)
    { foreach (string t in Terms(text)) { double o; d.TryGetValue(t, out o); d[t] = Math.Max(o, 0) + w; } }

    static List<KbEntry> All()
    {
        lock (Lck)
        {
            if (cache != null) return cache;
            List<KbEntry> l = new List<KbEntry>();
            using (SqlConnection c = Db.Open())
            using (SqlCommand q = new SqlCommand("SELECT EntryId, Code, Product, Area, Title, Summary, StepsJson, WarningsJson, Keywords, Phrases, MaterialId, YoutubeId, VideoTitle, StartSec, IsActive, HitCount FROM dbo.KbEntries", c))
            using (SqlDataReader r = q.ExecuteReader())
                while (r.Read())
                {
                    KbEntry e = new KbEntry
                    {
                        Id = r.GetInt32(0), Code = r.GetString(1), Product = r.GetString(2), Area = r.GetString(3), Title = r.GetString(4), Summary = r.GetString(5),
                        Keywords = r.IsDBNull(8) ? "" : r.GetString(8), Phrases = r.IsDBNull(9) ? "" : r.GetString(9), MaterialId = r.IsDBNull(10) ? (int?)null : r.GetInt32(10),
                        YoutubeId = r.IsDBNull(11) ? null : r.GetString(11), VideoTitle = r.IsDBNull(12) ? "" : r.GetString(12), StartSec = r.GetInt32(13), IsActive = r.GetBoolean(14), HitCount = r.GetInt32(15)
                    };
                    try { e.Steps = JS.Deserialize<List<KbStep>>(r.GetString(6)) ?? new List<KbStep>(); } catch { }
                    try { if (!r.IsDBNull(7)) e.Warnings = JS.Deserialize<List<string>>(r.GetString(7)) ?? new List<string>(); } catch { }
                    l.Add(e);
                }
            Index(l); return cache;
        }
    }
    /// <summary>Arama dizinini kurar (veritabanindan bagimsiz; test icin de kullanilir)</summary>
    public static void Index(List<KbEntry> l)
    {
        lock (Lck)
        {
            foreach (KbEntry e in l)
            {
                    Dictionary<string, double> d = new Dictionary<string, double>();
                    AddTerms(d, e.Title, 3); AddTerms(d, e.Keywords.Replace('|', ' '), 3); AddTerms(d, e.Phrases.Replace('|', ' '), 2.5); AddTerms(d, e.Area, 1);
                    AddTerms(d, e.Summary, 1.5); foreach (string w in e.Warnings) AddTerms(d, w, 1.5); foreach (KbStep s in e.Steps) AddTerms(d, s.text, 1);
                    foreach (string k in d.Keys.ToList()) d[k] = Math.Min(d[k], 8);   // tek kelime tekrariyla sisme olmasin
                    e.Terms = d;
            }
            Dictionary<string, int> df = new Dictionary<string, int>();
            foreach (KbEntry e in l) if (e.IsActive) foreach (string k in e.Terms.Keys) { int n; df.TryGetValue(k, out n); df[k] = n + 1; }
            int N = Math.Max(1, l.Count(x => x.IsActive)); idf = new Dictionary<string, double>();
            foreach (var kv in df) idf[kv.Key] = Math.Log(1.0 + N / (double)kv.Value);
            cache = l;
        }
    }
    public static void Invalidate() { lock (Lck) { cache = null; idf = null; } }

    public class Hit { public KbEntry E; public double Score; }
    const double MatchMin = 35;   // AI olmadan konu gosterme esigi (sinamada gercek eslesmeler 37+, alakasizlar 30-)
    /// <summary>Puanlama: sorgu koklerinin kayittaki agirligi x nadirlik; ifade (phrase) birebir geciyorsa bonus</summary>
    public static List<Hit> Search(string product, string text, int top)
    {
        List<KbEntry> all = All();
        List<string> q = Terms(text).Distinct().ToList(); string fq = " " + Fold(text) + " ";
        List<Hit> hits = new List<Hit>();
        if (q.Count == 0) return hits;
        foreach (KbEntry e in all)
        {
            if (!e.IsActive || !string.Equals(e.Product, product, StringComparison.OrdinalIgnoreCase)) continue;
            double s = 0; int matched = 0;
            foreach (string t in q) { double w; if (e.Terms.TryGetValue(t, out w)) { double i; idf.TryGetValue(t, out i); s += w * i; matched++; } }
            if (matched == 0) continue;
            foreach (string p in (e.Phrases + "|" + e.Keywords).Split('|')) { string fp = Fold(p).Trim(); if (fp.Length >= 8 && fq.Contains(" " + fp)) s += 6; }
            s *= 0.5 + 0.5 * matched / (double)q.Count;   // sorgunun ne kadarini karsiliyor
            hits.Add(new Hit { E = e, Score = s });
        }
        return hits.OrderByDescending(h => h.Score).Take(top).ToList();
    }

    // ---------- kisisel bilgi maskeleme (Claude'a ve kayda gitmeden once) ----------
    public static string Mask(string s)
    {
        s = s ?? "";
        s = Regex.Replace(s, @"[\w.+-]+@[\w-]+(\.[\w-]+)+", "[e-posta]");
        s = Regex.Replace(s, @"\bTR\s?\d{2}(\s?\d{4}){5}\s?\d{2}\b", "[IBAN]", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\b[1-9]\d{10}\b", "[kimlik no]");
        s = Regex.Replace(s, @"(\+?90[\s-]?)?\(?0?5\d{2}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}\b", "[telefon]");
        s = Regex.Replace(s, @"\b0?\(?[2-4]\d{2}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}\b", "[telefon]");
        s = Regex.Replace(s, @"(?i)(şifre|sifre|parola|password|pwd)\s*[:=]\s*\S+", "$1: [gizli]");
        return s;
    }

    // ---------- on yanit ----------
    /// <summary>Atlama kurallari: yalniz River AI; kritik oncelik ve ozellik talebi dogrudan ekibe</summary>
    public static string SkipReason(string product, string requestType, string priority)
    {
        if (!Enabled) return "off";
        if (product != "River AI") return "product";
        if (priority == "Critical") return "critical";
        if (requestType == "Özellik Talebi") return "feature";
        return null;
    }

    public static Dictionary<string, object> Suggest(int? companyId, int? userId, string product, string requestType, string subject, string description)
    {
        string q = Mask((subject ?? "").Trim() + "\n" + (description ?? "").Trim()).Trim();
        if (q.Length > 4000) q = q.Substring(0, 4000);
        Dictionary<string, object> res = new Dictionary<string, object>();
        List<Hit> hits = Search(product, q, 8);
        double best = hits.Count > 0 ? hits[0].Score : 0;
        // AI icin genis aday listesi (alakasizligi AI ayiklar); AI yoksa yalniz guclu eslesmeler gosterilir
        List<Hit> strong = hits.Where(h => h.Score >= Math.Max(12, best * 0.35)).ToList();
        List<KbEntry> sure = hits.Where(h => h.Score >= MatchMin).Take(3).Select(h => h.E).ToList();
        string mode = "none"; Dictionary<string, object> ai = null; List<KbEntry> used = new List<KbEntry>();
        if (strong.Count > 0)
        {
            bool canAi = LeadClaude.Configured && RecentCount(userId) < 20;   // kullanici basina saatlik sinir (maliyet)
            if (canAi)
            {
                string err; ai = AskClaude(q, strong.Take(6).Select(h => h.E).ToList(), out err);
                if (ai != null && ai.ContainsKey("found") && Convert.ToBoolean(ai["found"]))
                {
                    mode = "ai";
                    foreach (object r in AsList(ai.ContainsKey("refs") ? ai["refs"] : null)) { KbEntry e = strong.Select(h => h.E).FirstOrDefault(x => x.Code == Convert.ToString(r)); if (e != null && !used.Contains(e)) used.Add(e); }
                    if (used.Count == 0) used.Add(strong[0].E);
                }
                else if (ai != null) mode = "none";                       // AI kaynaklarda yanit bulamadi
                else if (sure.Count > 0) { mode = "match"; used = sure; }   // AI hatasi: yalniz guclu eslesmeler
            }
            else if (sure.Count > 0) { mode = "match"; used = sure; }
        }
        // istemciye: mod, yanit, kaynak kartlari
        res["mode"] = mode;
        if (mode == "ai")
        {
            res["intro"] = Convert.ToString(ai.ContainsKey("intro") ? ai["intro"] : "");
            List<object> steps = new List<object>();
            foreach (object so in AsList(ai.ContainsKey("steps") ? ai["steps"] : null))
            {
                Dictionary<string, object> sd = so as Dictionary<string, object>; if (sd == null) continue;
                string rc = Convert.ToString(sd.ContainsKey("ref") ? sd["ref"] : ""); KbEntry e = used.FirstOrDefault(x => x.Code == rc);
                int t = e != null ? e.StartSec : 0; object tv; if (sd.TryGetValue("t", out tv)) { int ti; if (int.TryParse(Convert.ToString(tv), out ti) && e != null && e.Steps.Any(z => z.t == ti)) t = ti; }
                steps.Add(new { text = Convert.ToString(sd.ContainsKey("text") ? sd["text"] : ""), link = e != null ? e.KbLink(t) : null, ts = e != null ? Ts(t) : null });
            }
            res["steps"] = steps;
            res["followup"] = Convert.ToString(ai.ContainsKey("followup") ? ai["followup"] : "");
        }
        res["refs"] = used.Select(e => new { code = e.Code, title = e.Title, area = e.Area, video = e.VideoTitle, link = e.Link, ts = Ts(e.StartSec), summary = mode == "match" ? e.Summary : null,
            steps = mode == "match" ? e.Steps.Select(s => new { text = s.text, link = e.KbLink(s.t), ts = Ts(s.t) }).ToList() : null, warnings = mode == "match" ? e.Warnings : null }).ToList();
        int id = SaveAnswer(companyId, userId, product, requestType, q, mode, mode == "ai" ? JS.Serialize(ai) : null, string.Join(",", used.Select(e => e.Code)));
        if (used.Count > 0) Touch(used.Select(e => e.Id));
        res["answerId"] = id;
        return res;
    }

    static List<object> AsList(object o) { System.Collections.ArrayList a = o as System.Collections.ArrayList; return a != null ? a.Cast<object>().ToList() : (o as object[] ?? new object[0]).ToList(); }

    static Dictionary<string, object> AskClaude(string question, List<KbEntry> src, out string err)
    {
        StringBuilder s = new StringBuilder();
        foreach (KbEntry e in src)
        {
            s.Append("[" + e.Code + "] " + e.Title + " (" + e.Area + ")\n" + e.Summary + "\nAdımlar:\n");
            foreach (KbStep st in e.Steps) s.Append("- (t=" + st.t + ") " + st.text + "\n");
            foreach (string w in e.Warnings) s.Append("Uyarı: " + w + "\n");
            s.Append("\n");
        }
        string sys = "River AI platformunun destek asistanısın. Bir iş ortağı destek talebi açmadan önce sorununu yazdı. Sana River Akademi eğitimlerinden çıkarılmış bilgi kayıtları (KAYNAKLAR) veriliyor. Kurallar: " +
            "1) YALNIZ kaynaklardaki bilgilere dayan; kaynakta olmayan menü, ayar, adım ya da özellik uydurma, genel bilgiyle tamamlama. " +
            "2) Kaynaklar sorunu açıkça karşılamıyorsa found=false döndür; tahmin yürütme. " +
            "3) Yanıt Türkçe, kısa ve uygulanabilir olsun: tek cümlelik giriş (intro) ve en fazla 6 adım. Her adımda dayandığın kaydın kodunu ref, ilgili adımın t değerini t olarak ver. " +
            "4) Kullanıcının anlattığı durum ya da hata bir kaynaktaki Uyarı ile eşleşiyorsa bunu açıkça söyle. " +
            "5) Doğru yanıt için kullanıcıdan bir bilgi gerekiyorsa followup alanına tek bir soru yaz; gerekmiyorsa boş bırak. " +
            "Sadece şu JSON'u döndür: {\"found\":true,\"intro\":\"...\",\"steps\":[{\"text\":\"...\",\"ref\":\"v19-4\",\"t\":512}],\"refs\":[\"v19-4\"],\"followup\":\"\"}";
        return LeadClaude.Json(sys, "SORUN:\n" + question + "\n\nKAYNAKLAR:\n" + s, 1200, out err);
    }

    static int RecentCount(int? userId)
    {
        if (!userId.HasValue) return 0;
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT COUNT(*) FROM dbo.KbAnswers WHERE UserId = @u AND Mode = N'ai' AND CreatedAt > DATEADD(HOUR, -1, SYSUTCDATETIME())", c))
        { q.Parameters.AddWithValue("@u", userId.Value); return (int)q.ExecuteScalar(); }
    }
    static int SaveAnswer(int? co, int? u, string product, string rt, string q, string mode, string json, string codes)
    {
        using (SqlConnection c = Db.Open())
        using (SqlCommand x = new SqlCommand("INSERT INTO dbo.KbAnswers (CompanyId, UserId, Product, RequestType, Question, Mode, AnswerJson, EntryCodes) OUTPUT INSERTED.AnswerId VALUES (@c, @u, @p, @r, @q, @m, @j, @e)", c))
        {
            x.Parameters.AddWithValue("@c", (object)co ?? DBNull.Value); x.Parameters.AddWithValue("@u", (object)u ?? DBNull.Value); x.Parameters.AddWithValue("@p", product);
            x.Parameters.AddWithValue("@r", (object)rt ?? DBNull.Value); x.Parameters.AddWithValue("@q", q); x.Parameters.AddWithValue("@m", mode);
            x.Parameters.AddWithValue("@j", (object)json ?? DBNull.Value); x.Parameters.AddWithValue("@e", (object)codes ?? DBNull.Value);
            return (int)x.ExecuteScalar();
        }
    }
    static void Touch(IEnumerable<int> ids)
    {
        string l = string.Join(",", ids.Select(i => i.ToString()));
        using (SqlConnection c = Db.Open()) using (SqlCommand x = new SqlCommand("UPDATE dbo.KbEntries SET HitCount = HitCount + 1 WHERE EntryId IN (" + l + ")", c)) x.ExecuteNonQuery();
        lock (Lck) { if (cache != null) foreach (KbEntry e in cache) if (ids.Contains(e.Id)) e.HitCount++; }
    }

    /// <summary>Kullanici "cozuldu" dedi (talep acilmadi)</summary>
    public static void MarkSolved(int answerId, int? userId)
    {
        using (SqlConnection c = Db.Open())
        using (SqlCommand x = new SqlCommand("UPDATE dbo.KbAnswers SET Outcome = N'solved', ClosedAt = SYSUTCDATETIME() WHERE AnswerId = @a AND (UserId = @u OR (@u IS NULL AND UserId IS NULL)) AND Outcome = N'shown'", c))
        { x.Parameters.AddWithValue("@a", answerId); x.Parameters.AddWithValue("@u", (object)userId ?? DBNull.Value); x.ExecuteNonQuery(); }
    }

    /// <summary>Talep acildi: on yaniti talebe bagla ve ekip icin dahili not metnini dondur (yoksa null)</summary>
    public static string AttachToTicket(int answerId, int? userId, int ticketId)
    {
        string mode = null, json = null, codes = null;
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT Mode, AnswerJson, EntryCodes FROM dbo.KbAnswers WHERE AnswerId = @a AND (UserId = @u OR (@u IS NULL AND UserId IS NULL)) AND TicketId IS NULL", c))
            {
                q.Parameters.AddWithValue("@a", answerId); q.Parameters.AddWithValue("@u", (object)userId ?? DBNull.Value);
                using (SqlDataReader r = q.ExecuteReader()) { if (!r.Read()) return null; mode = r.GetString(0); json = r.IsDBNull(1) ? null : r.GetString(1); codes = r.IsDBNull(2) ? "" : r.GetString(2); }
            }
            using (SqlCommand u = new SqlCommand("UPDATE dbo.KbAnswers SET Outcome = N'escalated', TicketId = @t, ClosedAt = SYSUTCDATETIME() WHERE AnswerId = @a", c))
            { u.Parameters.AddWithValue("@t", ticketId); u.Parameters.AddWithValue("@a", answerId); u.ExecuteNonQuery(); }
        }
        List<KbEntry> all = All(); StringBuilder b = new StringBuilder();
        b.Append("🤖 AI ön yanıtı — talep açılmadan önce kullanıcıya gösterildi; kullanıcı \"çözülmedi\" diyerek talebi gönderdi.\n\n");
        if (mode == "ai" && json != null)
        {
            try
            {
                Dictionary<string, object> d = JS.Deserialize<Dictionary<string, object>>(json);
                b.Append("Öneri: " + Convert.ToString(d.ContainsKey("intro") ? d["intro"] : "") + "\n");
                int i = 1; foreach (object so in AsList(d.ContainsKey("steps") ? d["steps"] : null)) { Dictionary<string, object> sd = so as Dictionary<string, object>; if (sd != null) b.Append((i++) + ". " + Convert.ToString(sd["text"]) + "\n"); }
            }
            catch { }
        }
        else if (mode == "match") b.Append("Yapay zekâ yanıtı üretilemedi; kullanıcıya aşağıdaki ilgili eğitim konuları gösterildi.\n");
        else b.Append("Bilgi tabanında bu soruya uygun bir kayıt bulunamadı. (Eksik içerik adayı)\n");
        if (!string.IsNullOrEmpty(codes))
        {
            b.Append("\nKaynaklar:\n");
            foreach (string code in codes.Split(',')) { KbEntry e = all.FirstOrDefault(x => x.Code == code); if (e != null) b.Append("• [" + e.Code + "] " + e.Title + " — " + e.VideoTitle + ", " + Ts(e.StartSec) + "\n"); }
        }
        return b.ToString().TrimEnd();
    }

    // ---------- yonetim ----------
    public class ImportResult { public int Added, Updated, Skipped; }
    public static ImportResult Import(string json)
    {
        ImportResult res = new ImportResult();
        Dictionary<string, object> root = JS.Deserialize<Dictionary<string, object>>(json);
        List<object> entries = AsList(root.ContainsKey("entries") ? root["entries"] : null);
        if (entries.Count == 0) throw new Exception("Dosyada kayıt bulunamadı (entries).");
        using (SqlConnection c = Db.Open())
            foreach (object o in entries)
            {
                Dictionary<string, object> e = o as Dictionary<string, object>;
                if (e == null || !e.ContainsKey("id") || !e.ContainsKey("title")) { res.Skipped++; continue; }
                Dictionary<string, object> src = (e.ContainsKey("source") ? e["source"] : null) as Dictionary<string, object> ?? new Dictionary<string, object>();
                Func<string, string> S = k => e.ContainsKey(k) && e[k] != null ? Convert.ToString(e[k]) : "";
                Func<string, string> J = k => string.Join("|", AsList(e.ContainsKey(k) ? e[k] : null).Select(x => Convert.ToString(x)));
                using (SqlCommand x = new SqlCommand(
                    "MERGE dbo.KbEntries AS t USING (SELECT @code AS Code) AS s ON t.Code = s.Code " +
                    "WHEN MATCHED THEN UPDATE SET Product=@p, Area=@a, Title=@ti, Summary=@su, StepsJson=@st, WarningsJson=@w, Keywords=@k, Phrases=@ph, MaterialId=@m, YoutubeId=@y, VideoTitle=@v, StartSec=@ss, UpdatedAt=SYSUTCDATETIME() " +
                    "WHEN NOT MATCHED THEN INSERT (Code, Product, Area, Title, Summary, StepsJson, WarningsJson, Keywords, Phrases, MaterialId, YoutubeId, VideoTitle, StartSec) VALUES (@code, @p, @a, @ti, @su, @st, @w, @k, @ph, @m, @y, @v, @ss) " +
                    "OUTPUT $action;", c))
                {
                    x.Parameters.AddWithValue("@code", S("id")); x.Parameters.AddWithValue("@p", S("product").Length > 0 ? S("product") : "River AI");
                    x.Parameters.AddWithValue("@a", S("area")); x.Parameters.AddWithValue("@ti", S("title")); x.Parameters.AddWithValue("@su", S("summary"));
                    x.Parameters.AddWithValue("@st", JS.Serialize(e.ContainsKey("steps") ? e["steps"] : new object[0])); x.Parameters.AddWithValue("@w", JS.Serialize(e.ContainsKey("warnings") ? e["warnings"] : new object[0]));
                    x.Parameters.AddWithValue("@k", J("keywords")); x.Parameters.AddWithValue("@ph", J("phrases"));
                    int mid; x.Parameters.AddWithValue("@m", src.ContainsKey("materialId") && int.TryParse(Convert.ToString(src["materialId"]), out mid) ? (object)mid : DBNull.Value);
                    x.Parameters.AddWithValue("@y", src.ContainsKey("youtubeId") ? (object)Convert.ToString(src["youtubeId"]) : DBNull.Value);
                    x.Parameters.AddWithValue("@v", src.ContainsKey("video") ? (object)Convert.ToString(src["video"]) : DBNull.Value);
                    int ss; x.Parameters.AddWithValue("@ss", src.ContainsKey("start") && int.TryParse(Convert.ToString(src["start"]), out ss) ? ss : 0);
                    string act = Convert.ToString(x.ExecuteScalar()); if (act == "INSERT") res.Added++; else res.Updated++;
                }
            }
        Invalidate(); return res;
    }
    public static void SetActive(int id, bool on)
    {
        using (SqlConnection c = Db.Open()) using (SqlCommand x = new SqlCommand("UPDATE dbo.KbEntries SET IsActive = @o WHERE EntryId = @i", c))
        { x.Parameters.AddWithValue("@o", on); x.Parameters.AddWithValue("@i", id); x.ExecuteNonQuery(); }
        Invalidate();
    }
    public static List<KbEntry> List() { return All(); }

    /// <summary>Pano: son N gun on yanit sayilari ve yanit bulunamayan / talebe donusen sorular</summary>
    public static Dictionary<string, object> Stats(int days)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT COUNT(*), SUM(CASE WHEN Outcome=N'solved' THEN 1 ELSE 0 END), SUM(CASE WHEN Outcome=N'escalated' THEN 1 ELSE 0 END), SUM(CASE WHEN Mode=N'none' THEN 1 ELSE 0 END), SUM(CASE WHEN Mode=N'ai' THEN 1 ELSE 0 END) FROM dbo.KbAnswers WHERE CreatedAt > DATEADD(DAY, -@d, SYSUTCDATETIME())", c))
            {
                q.Parameters.AddWithValue("@d", days);
                using (SqlDataReader r = q.ExecuteReader()) if (r.Read()) { d["total"] = r.GetInt32(0); d["solved"] = r.IsDBNull(1) ? 0 : r.GetInt32(1); d["escalated"] = r.IsDBNull(2) ? 0 : r.GetInt32(2); d["none"] = r.IsDBNull(3) ? 0 : r.GetInt32(3); d["ai"] = r.IsDBNull(4) ? 0 : r.GetInt32(4); }
            }
            List<object> l = new List<object>();
            using (SqlCommand q = new SqlCommand("SELECT TOP 40 a.AnswerId, a.CreatedAt, a.Mode, a.Outcome, a.Question, a.TicketId, t.TicketNo, co.Name FROM dbo.KbAnswers a LEFT JOIN dbo.Tickets t ON t.TicketId = a.TicketId LEFT JOIN dbo.Companies co ON co.CompanyId = a.CompanyId WHERE a.CreatedAt > DATEADD(DAY, -@d, SYSUTCDATETIME()) AND (a.Mode = N'none' OR a.Outcome = N'escalated') ORDER BY a.CreatedAt DESC", c))
            {
                q.Parameters.AddWithValue("@d", days);
                using (SqlDataReader r = q.ExecuteReader())
                    while (r.Read())
                    {
                        string qs = r.GetString(4); if (qs.Length > 400) qs = qs.Substring(0, 400) + "…";
                        l.Add(new { id = r.GetInt32(0), at = r.GetDateTime(1).ToString("dd.MM.yyyy HH:mm"), mode = r.GetString(2), outcome = r.GetString(3), question = qs, ticketId = r.IsDBNull(5) ? (int?)null : r.GetInt32(5), ticketNo = r.IsDBNull(6) ? null : r.GetString(6), company = r.IsDBNull(7) ? "" : r.GetString(7) });
                    }
            }
            d["gaps"] = l;
        }
        return d;
    }
}
