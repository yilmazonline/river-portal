<%@ WebHandler Language="C#" Class="KbActionHandler" %>

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Bilgi tabani ucu.
//  suggest / solved : talep acan taraf (is ortagi, musteri) - destek talebi oncesi AI on yaniti
//  list / import / toggle / stats : destek ekibi ve super admin
// Tum istekler POST + ayni site.
public class KbActionHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    static readonly JavaScriptSerializer JS = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };

    public void ProcessRequest(HttpContext ctx)
    {
        HttpRequest q = ctx.Request; ctx.Response.ContentType = "application/json"; ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        if (!Auth.IsLoggedIn) { ctx.Response.StatusCode = 401; Out(ctx, "Oturum açmanız gerekiyor.", null); return; }
        Uri refr = q.UrlReferrer;
        if (!string.Equals(q.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, q.Url.Host, StringComparison.OrdinalIgnoreCase))
        { ctx.Response.StatusCode = 400; Out(ctx, "Geçersiz istek.", null); return; }
        if (!Ready()) { Out(ctx, "Bilgi tabanı tabloları bulunamadı: database\\66_knowledge_base.sql dosyasını çalıştırın.", null); return; }
        string op = q["op"] ?? ""; var f = q.Unvalidated.Form;
        bool admin = Auth.IsSupportAgent || Auth.IsSuperAdmin;
        try
        {
            switch (op)
            {
                case "suggest":
                    {
                        if (Auth.IsVendorSide) { Out(ctx, null, new { mode = "skip", reason = "vendor" }); return; }
                        string product = (f["product"] ?? "").Trim(), rt = (f["requesttype"] ?? "").Trim(), pr = (f["priority"] ?? "").Trim();
                        string skip = Kb.SkipReason(product, rt, pr);
                        if (skip != null) { Out(ctx, null, new { mode = "skip", reason = skip }); return; }
                        string subject = (f["subject"] ?? "").Trim(), desc = (f["description"] ?? "").Trim();
                        if ((subject + desc).Length < 12) { Out(ctx, null, new { mode = "skip", reason = "short" }); return; }
                        ctx.Server.ScriptTimeout = 90;
                        Out(ctx, null, Kb.Suggest(Auth.CompanyId > 0 ? (int?)Auth.CompanyId : null, Auth.UserId, product, rt, subject, desc)); return;
                    }
                case "solved":
                    { int id; int.TryParse(f["id"], out id); if (id > 0) Kb.MarkSolved(id, Auth.UserId); Out(ctx, null, new { ok = true }); return; }
                case "list":
                    {
                        if (!admin) { Deny(ctx); return; }
                        Out(ctx, null, Kb.List().OrderBy(e => e.Area).ThenBy(e => e.Code, StringComparer.Ordinal).Select(e => new
                        { id = e.Id, code = e.Code, area = e.Area, title = e.Title, summary = e.Summary, video = e.VideoTitle, link = e.Link, ts = Kb.Ts(e.StartSec), active = e.IsActive, hits = e.HitCount,
                          steps = e.Steps.Select(s => new { text = s.text, ts = Kb.Ts(s.t), link = e.KbLink(s.t) }), warnings = e.Warnings, keywords = e.Keywords.Replace("|", ", ") }).ToList());
                        return;
                    }
                case "import":
                    {
                        if (!admin) { Deny(ctx); return; }
                        HttpPostedFile file = q.Files["file"]; string json = null;
                        if (file != null && file.ContentLength > 0) { if (file.ContentLength > 20 * 1024 * 1024) { Out(ctx, "Dosya 20 MB'tan büyük olamaz.", null); return; } using (var r = new System.IO.StreamReader(file.InputStream, System.Text.Encoding.UTF8)) json = r.ReadToEnd(); }
                        if (string.IsNullOrWhiteSpace(json)) { Out(ctx, "Bir JSON dosyası seçin.", null); return; }
                        Kb.ImportResult res; try { res = Kb.Import(json); } catch (Exception ex) { Out(ctx, "Dosya okunamadı: " + ex.Message, null); return; }
                        Out(ctx, null, new { added = res.Added, updated = res.Updated, skipped = res.Skipped }); return;
                    }
                case "toggle":
                    { if (!admin) { Deny(ctx); return; } int id; int.TryParse(f["id"], out id); Kb.SetActive(id, f["on"] == "1"); Out(ctx, null, new { ok = true }); return; }
                case "stats":
                    { if (!admin) { Deny(ctx); return; } int d; if (!int.TryParse(f["days"], out d) || d < 1 || d > 365) d = 30; Out(ctx, null, Kb.Stats(d)); return; }
                default: Out(ctx, "Bilinmeyen işlem.", null); return;
            }
        }
        catch (Exception ex)
        {
            // on yanit hatasi kullaniciyi asla engellemesin: istemci talebi normal gonderir
            if (op == "suggest") { Out(ctx, null, new { mode = "skip", reason = "error" }); return; }
            Out(ctx, "İşlem tamamlanamadı: " + ex.Message, null);
        }
    }
    static bool Ready()
    {
        try { using (SqlConnection c = Db.Open()) using (SqlCommand x = new SqlCommand("SELECT CASE WHEN OBJECT_ID('dbo.KbEntries') IS NOT NULL AND OBJECT_ID('dbo.KbAnswers') IS NOT NULL THEN 1 ELSE 0 END", c)) return (int)x.ExecuteScalar() == 1; }
        catch { return false; }
    }
    static void Deny(HttpContext ctx) { ctx.Response.StatusCode = 403; Out(ctx, "Bu işlem destek ekibine açıktır.", null); }
    static void Out(HttpContext ctx, string err, object data)
    {
        Dictionary<string, object> d = new Dictionary<string, object>(); d["ok"] = err == null; d["error"] = err; if (data != null) d["data"] = data;
        ctx.Response.Write(JS.Serialize(d));
    }
}
