<%@ WebHandler Language="C#" Class="LeadActionHandler" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Lead Merkezi ucu. YALNIZ super admin. state / contacts (GET); digerleri POST + ayni site.
public class LeadActionHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    static readonly JavaScriptSerializer JS = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };

    public void ProcessRequest(HttpContext ctx)
    {
        HttpRequest q = ctx.Request; ctx.Response.ContentType = "application/json"; ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        if (!Auth.IsLoggedIn || !Auth.IsSuperAdmin) { ctx.Response.StatusCode = 403; Out(ctx, "Lead Merkezi yalnız süper admin yetkisine açıktır.", null); return; }
        if (!Leads.Ready()) { Out(ctx, "Lead tabloları bulunamadı: database\\62_leads.sql dosyasını çalıştırın.", null); return; }
        string op = q["op"] ?? "state";
        if (op != "state" && op != "contacts")
        {
            Uri refr = q.UrlReferrer;
            if (!string.Equals(q.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, q.Url.Host, StringComparison.OrdinalIgnoreCase)) { Out(ctx, "İstek kaynağı doğrulanamadı.", null); return; }
        }
        int id; int.TryParse(q["id"], out id);
        string baseUrl = (System.Web.Configuration.WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (baseUrl.Length == 0) baseUrl = q.Url.GetLeftPart(UriPartial.Authority) + (q.ApplicationPath ?? "").TrimEnd('/');
        string cat = q["category"] == "danisman" ? "danisman" : "musteri";
        try
        {
            string err = null; Dictionary<string, object> extra = new Dictionary<string, object>();
            switch (op)
            {
                case "state": try { LeadEngine.Tick(baseUrl, false); } catch { } break;   // acilista yanit okuma ve takip (en sik 5 dk)
                case "graphtest": extra["graphTest"] = LeadEngine.TestGraph(); break;
                case "tick": ctx.Server.ScriptTimeout = 300; extra["tick"] = LeadEngine.Tick(baseUrl, true); break;
                case "retry": Leads.Retry(id); break;
                case "reply": ctx.Server.ScriptTimeout = 120; err = Leads.ReplyOp(id, q.Form["do"] ?? "", q.Form["cls"], q.Unvalidated.Form["body"], Auth.UserId); break;
                case "contacts": { int pg; int.TryParse(q["page"], out pg); Out(ctx, null, Pool(q["category"], q["status"], q["q"], pg)); return; }
                case "import":
                    {
                        HttpPostedFile f = q.Files["file"];
                        if (f == null || f.ContentLength == 0) { err = "CSV dosyası seçin."; break; }
                        if (f.ContentLength > 10 * 1024 * 1024) { err = "Dosya en fazla 10 MB olabilir."; break; }
                        extra["import"] = Res(Leads.ImportCsv(f.InputStream, cat, q.Form["source"])); break;
                    }
                case "apollo":
                    {
                        int want; if (!int.TryParse(q.Form["want"], out want) || want < 1 || want > 30) { err = "Tek seferde 1 ile 30 arası kişi getirilebilir."; break; }
                        ctx.Server.ScriptTimeout = 300;
                        extra["import"] = Res(Leads.FromApollo(cat, want)); break;
                    }
                case "test":
                    {
                        List<object> sample; int total; err = Leads.Test(cat, out sample, out total);
                        if (err == null) { extra["test"] = new { total = total, sample = sample }; }
                        break;
                    }
                case "drafts":
                    {
                        int max; if (!int.TryParse(q.Form["max"], out max) || max < 1 || max > 100) max = Leads.I(Leads.Settings(), "dailyNew", 25);
                        ctx.Server.ScriptTimeout = 300;   // Claude ile kisisellestirme her aday icin bir cagri
                        extra["made"] = Leads.CreateDrafts(cat, max); break;
                    }
                case "aitpl":
                    {
                        // sablonu AI ile duzenle: kaydedilmemis metin ve istege bagli talimat; durum yeniden yuklenmez
                        string tCat = null, tPer = null; int stp = 0;
                        using (System.Data.SqlClient.SqlConnection c = Db.Open())
                        using (System.Data.SqlClient.SqlCommand x = new System.Data.SqlClient.SqlCommand("SELECT Category, Persona, StepNo FROM dbo.LeadTemplates WHERE TemplateId = @i", c))
                        { x.Parameters.AddWithValue("@i", id); using (System.Data.SqlClient.SqlDataReader r = x.ExecuteReader()) if (r.Read()) { tCat = r.GetString(0); tPer = r.IsDBNull(1) ? "genel" : r.GetString(1); stp = r.GetInt32(2); } }
                        if (tCat == null) { Out(ctx, "Şablon bulunamadı.", null); return; }
                        if (!LeadClaude.Configured) { Out(ctx, "Claude anahtarı tanımlı değil.", null); return; }
                        string tb = q.Unvalidated.Form["body"] ?? "", ts = q.Unvalidated.Form["subject"] ?? "", ins = q.Unvalidated.Form["instruction"] ?? "";
                        if (tb.Trim().Length < 10 && ins.Trim().Length < 10) { Out(ctx, "Şablon metni ya da talimat yazın.", null); return; }
                        if (ins.Length > 3000) ins = ins.Substring(0, 3000);
                        ctx.Server.ScriptTimeout = 120; string terr;
                        Dictionary<string, object> res = LeadAi.WriteTemplate(tCat, tPer, stp, ts, tb, ins, out terr);
                        if (res == null) { Out(ctx, terr ?? "Claude cevap vermedi.", null); return; }
                        Out(ctx, null, new Dictionary<string, object> { { "ai", res } }); return;
                    }
                case "aicheck":
                case "aiwrite":
                    {
                        // taslaga ozel: yalniz bu taslak ve alicisi; durum yeniden yuklenmez (duzenleyicideki kaydedilmemis metin korunur)
                        int step; string st; LdContact x = LeadAi.Context(id, out step, out st);
                        if (x == null) { Out(ctx, "Taslak bulunamadı.", null); return; }
                        if (st != "draft" && st != "approved") { Out(ctx, "Bu e-posta artık düzenlenemez.", null); return; }
                        string sj = q.Unvalidated.Form["subject"] ?? "", bd = q.Unvalidated.Form["body"] ?? "";
                        if (bd.Trim().Length < 10) { Out(ctx, "Metin boş.", null); return; }
                        Dictionary<string, object> res = new Dictionary<string, object>();
                        if (op == "aicheck") res["auto"] = LeadAi.PreCheck(x, sj, bd);
                        if (!LeadClaude.Configured) { if (op == "aicheck") { res["ai"] = null; res["aiError"] = "Claude anahtarı tanımlı değil; yalnız otomatik kontrol yapıldı."; Out(ctx, null, res); } else Out(ctx, "Claude anahtarı tanımlı değil.", null); return; }
                        ctx.Server.ScriptTimeout = 120; string aerr;
                        Dictionary<string, object> ai = op == "aicheck" ? LeadAi.Check(x, step, sj, bd, out aerr) : LeadAi.Write(x, step, sj, bd, out aerr);
                        if (ai == null && op == "aiwrite") { Out(ctx, aerr ?? "Claude cevap vermedi.", null); return; }
                        res["ai"] = ai; if (ai == null) res["aiError"] = aerr;
                        Out(ctx, null, res); return;
                    }
                case "msg": err = Leads.MessageOp(id, q.Form["do"] ?? "", q.Unvalidated.Form["subject"], q.Unvalidated.Form["body"], Auth.UserId); break;
                case "approveAll":
                    foreach (LdMessage m in Leads.Queue("draft", 500)) Leads.MessageOp(m.Id, "approve", m.Subject, m.Body, Auth.UserId);
                    break;
                case "exclude":
                    {
                        Leads.Exclude(id, q.Unvalidated.Form["reason"]);
                        string em = q.Form["block"] == "1" ? Leads.ContactEmail(id) : null;
                        if (em != null) Leads.AddSuppression("email", em, "Havuzdan engellendi");
                        break;
                    }
                case "sup": err = Leads.AddSuppression(q.Form["kind"], q.Unvalidated.Form["value"], q.Unvalidated.Form["reason"]); break;
                case "supdel": Leads.RemoveSuppression(id); break;
                case "gender": { int cid; int.TryParse(q.Form["contactId"], out cid); err = Leads.SetGender(cid, q.Form["g"]); break; }
                case "fixhitap": { int[] fx = Leads.FixHitaps(); extra["msg"] = fx[0] + " e-postada hitap düzeltildi" + (fx[1] > 0 ? "; " + fx[1] + " kişinin cinsiyeti tahmin, kartlardan kontrol edin." : "."); break; }
                case "template":
                    err = Leads.SaveTemplate(id, q.Unvalidated.Form["subject"], q.Unvalidated.Form["body"], q.Form["active"] == "1"); break;
                case "settings": err = SaveSettings(q); break;
                default: err = "Geçersiz işlem."; break;
            }
            if (err != null) { Out(ctx, err, null); return; }
            Dictionary<string, object> d = State(); foreach (KeyValuePair<string, object> kv in extra) d[kv.Key] = kv.Value;
            Out(ctx, null, d);
        }
        catch (Exception ex) { Out(ctx, "İşlem tamamlanamadı: " + ex.Message, null); }
    }

    static object Res(LdImport r)
    {
        return new { added = r.Added, duplicate = r.Duplicate, suppressed = r.Suppressed, noEmail = r.NoEmail, unverified = r.Unverified, companyCap = r.CompanyCap, freeMail = r.FreeMail, invalid = r.Invalid, credits = r.Credits, creditsWasted = r.CreditsWasted, notes = r.Notes };
    }
    public const int PageSize = 50;
    /// <summary>Havuzun bir sayfasi (50 kayit) ve suzgece uyan toplam</summary>
    static Dictionary<string, object> Pool(string cat, string status, string q, int page)
    {
        int total; if (page < 1) page = 1;
        List<LdContact> list = Leads.ContactsPage(cat, status, q, (page - 1) * PageSize, PageSize, true, out total);
        int pages = Math.Max(1, (total + PageSize - 1) / PageSize);
        if (page > pages && total > 0) { page = pages; list = Leads.ContactsPage(cat, status, q, (page - 1) * PageSize, PageSize, true, out total); }   // son sayfadaki kayitlar cikarilinca
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["contacts"] = Shape(list); d["total"] = total; d["page"] = page; d["size"] = PageSize;
        return d;
    }
    static List<object> Shape(List<LdContact> src)
    {
        List<object> l = new List<object>();
        foreach (LdContact x in src)
            l.Add(new { id = x.Id, category = x.Category, name = x.FullName, title = x.Title, persona = x.Persona, email = x.Email, emailStatus = x.EmailStatus, company = x.CompanyName, industry = x.Industry, city = x.City, employees = x.Employees,
                        linkedin = x.LinkedIn, source = x.Source, status = x.Status, note = x.Note, at = x.CreatedAt.ToTrLocal().ToString("d MMM", Leads.TR) });
        return l;
    }
    static string SaveSettings(HttpRequest q)
    {
        int daily, cap;
        if (!int.TryParse(q.Form["dailyNew"], out daily) || daily < 1 || daily > 60) return "Günlük yeni aday sayısı 1 ile 60 arasında olmalı.";
        if (!int.TryParse(q.Form["maxPerCompany"], out cap) || cap < 1 || cap > 5) return "Firma başına kişi sınırı 1 ile 5 arasında olmalı.";
        foreach (string k in new[] { "apolloMusteri", "apolloDanisman" })
        {
            string j = (q.Unvalidated.Form[k] ?? "").Trim();
            try { if (!(JS.DeserializeObject(j) is Dictionary<string, object>)) return (k == "apolloMusteri" ? "Müşteri" : "Danışman") + " filtresi bir JSON nesnesi olmalı."; }
            catch (Exception ex) { return (k == "apolloMusteri" ? "Müşteri" : "Danışman") + " filtresi geçerli JSON değil: " + ex.Message; }
        }
        foreach (string k in new[] { "mailboxMusteri", "mailboxDanisman" })
        {
            string v = (q.Form[k] ?? "").Trim().ToLowerInvariant();
            if (!v.EndsWith("@fol.solutions")) return "Gönderim kutuları fol.solutions domaininde olmalı (ana domaini korumak için).";
        }
        // gizli kopya (BCC): gonderilen her e-postaya eklenir; virgulle birden fazla adres
        string bcc = (q.Form["bcc"] ?? "").Trim();
        foreach (string a in bcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) if (!System.Text.RegularExpressions.Regex.IsMatch(a.Trim(), @"^[^@\s]+@[^@\s]+\.[a-z]{2,}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return "Gizli kopya adresi geçerli değil: " + a.Trim();
        Leads.SaveSetting("bcc", bcc);
        int perBox; if (!int.TryParse(q.Form["dailyPerMailbox"], out perBox) || perBox < 1 || perBox > 50) return "Kutu başına günlük gönderim 1 ile 50 arasında olmalı.";
        string notify = (q.Form["notify"] ?? "").Trim(); if (notify.Length > 0 && !notify.Contains("@")) return "Bildirim adresi geçerli değil.";
        string from = (q.Form["sendFrom"] ?? "09:30").Trim(), to = (q.Form["sendTo"] ?? "17:30").Trim(); TimeSpan tf, tt;
        if (!TimeSpan.TryParse(from, out tf) || !TimeSpan.TryParse(to, out tt) || tt <= tf) return "Gönderim saatleri geçerli değil (başlangıç bitişten önce olmalı).";
        Leads.SaveSetting("sendEnabled", q.Form["sendEnabled"] == "1" ? "1" : "0"); Leads.SaveSetting("dailyPerMailbox", perBox.ToString()); Leads.SaveSetting("followupAuto", q.Form["followupAuto"] == "1" ? "1" : "0");
        Leads.SaveSetting("personalize", q.Form["personalize"] == "1" ? "1" : "0"); Leads.SaveSetting("notify", notify); Leads.SaveSetting("sendFrom", tf.ToString(@"hh\:mm")); Leads.SaveSetting("sendTo", tt.ToString(@"hh\:mm"));
        Leads.SaveSetting("dailyNew", daily.ToString()); Leads.SaveSetting("maxPerCompany", cap.ToString()); Leads.SaveSetting("onlyVerified", q.Form["onlyVerified"] == "1" ? "1" : "0");
        Leads.SaveSetting("apolloMusteri", q.Unvalidated.Form["apolloMusteri"].Trim()); Leads.SaveSetting("apolloDanisman", q.Unvalidated.Form["apolloDanisman"].Trim());
        Leads.SaveSetting("mailboxMusteri", q.Form["mailboxMusteri"].Trim().ToLowerInvariant()); Leads.SaveSetting("mailboxDanisman", q.Form["mailboxDanisman"].Trim().ToLowerInvariant());
        Leads.SaveSetting("signature", (q.Unvalidated.Form["signature"] ?? "").Replace("\r\n", "\n").Trim()); Leads.SaveSetting("optOut", (q.Unvalidated.Form["optOut"] ?? "").Trim());
        return null;
    }
    static Dictionary<string, object> State()
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        Dictionary<string, string> st = Leads.Settings();
        d["settings"] = new { dailyNew = Leads.I(st, "dailyNew", 25), maxPerCompany = Leads.I(st, "maxPerCompany", 2), onlyVerified = Leads.S(st, "onlyVerified", "1") == "1",
                              mailboxMusteri = Leads.S(st, "mailboxMusteri", ""), mailboxDanisman = Leads.S(st, "mailboxDanisman", ""), signature = Leads.S(st, "signature", ""), optOut = Leads.S(st, "optOut", ""),
                              apolloMusteri = Leads.S(st, "apolloMusteri", "{}"), apolloDanisman = Leads.S(st, "apolloDanisman", "{}"), bcc = Leads.S(st, "bcc", "yilmaz.guney@river.com.tr"),
                              sendEnabled = Leads.S(st, "sendEnabled", "0") == "1", dailyPerMailbox = Leads.I(st, "dailyPerMailbox", 10), followupAuto = Leads.S(st, "followupAuto", "0") == "1",
                              personalize = Leads.S(st, "personalize", "1") == "1", notify = Leads.S(st, "notify", ""), sendFrom = Leads.S(st, "sendFrom", "09:30"), sendTo = Leads.S(st, "sendTo", "17:30") };
        d["engine"] = new { graph = LeadGraph.Configured, claude = LeadClaude.Configured, inWindow = LeadEngine.InWindow(st),
                            sentMusteri = LeadEngine.SentToday(Leads.S(st, "mailboxMusteri", "")), sentDanisman = LeadEngine.SentToday(Leads.S(st, "mailboxDanisman", "")) };
        d["replies"] = Leads.Replies(200);
        d["hasApollo"] = Leads.HasApollo;
        d["counts"] = Leads.Counts();
        d["dash"] = Leads.Dashboard();
        Dictionary<string, object> pool = Pool(null, null, null, 1); d["contacts"] = pool["contacts"]; d["poolTotal"] = pool["total"];
        List<object> qd = new List<object>();
        foreach (string s in new[] { "draft", "approved", "failed", "sent" })
            foreach (LdMessage m in Leads.Queue(s, s == "sent" ? 30 : 200)) qd.Add(new { id = m.Id, contactId = m.ContactId, step = m.StepNo, error = m.Error, sentAt = m.SentAt.HasValue ? m.SentAt.Value.ToTrLocal().ToString("d MMM HH:mm", Leads.TR) : "", mailbox = m.Mailbox, subject = m.Subject, body = m.Body, status = m.Status, name = m.ContactName, title = m.ContactTitle, company = m.CompanyName, email = m.Email, category = m.Category, persona = m.Persona,
                hitap = Leads.HitapOf(Leads.FirstOf(new LdContact { FirstName = m.FirstName, FullName = m.ContactName }), m.Gender ?? (TrNames.Gender(Leads.FirstOf(new LdContact { FirstName = m.FirstName, FullName = m.ContactName })) ?? "E")),
                hitapSure = m.Gender != null ? m.GenderSure : TrNames.Gender(Leads.FirstOf(new LdContact { FirstName = m.FirstName, FullName = m.ContactName })) != null });
        d["queue"] = qd;
        d["templates"] = Leads.Templates();
        d["suppression"] = Leads.SuppressionList();
        Dictionary<string, string> why; HashSet<string> bd = Leads.BlockedDomains(out why); d["autoBlocked"] = why.Count;
        return d;
    }
    static void Out(HttpContext ctx, string err, object data)
    {
        Dictionary<string, object> d = new Dictionary<string, object>(); d["ok"] = err == null; d["error"] = err; if (data != null) d["data"] = data;
        ctx.Response.Write(JS.Serialize(d));
    }
}
