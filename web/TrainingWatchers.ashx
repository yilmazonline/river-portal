<%@ WebHandler Language="C#" Class="TrainingWatchersHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Akademi editor paneli: en cok izleyenler listesi (JSON). Yalniz egitim editoru / yonetici.
// period: all (varsayilan), 30 (son 30 gun), month (bu takvim ayi, Turkiye saatine gore)
public class TrainingWatchersHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer(); js.MaxJsonLength = 8 * 1024 * 1024;
        if (!Auth.IsLoggedIn || !Auth.CanEditTraining) { ctx.Response.StatusCode = 403; ctx.Response.Write(js.Serialize(new { ok = false, error = "Bu liste yalnız eğitim editörlerine açık." })); return; }
        string p = ctx.Request.QueryString["period"] ?? "all";
        DateTime? since = null;
        DateTime nowTr = DateTime.UtcNow.AddHours(3);
        if (p == "30") since = DateTime.UtcNow.AddDays(-30);
        else if (p == "month") since = new DateTime(nowTr.Year, nowTr.Month, 1).AddHours(-3);   // ayin basi, Turkiye saati -> UTC
        try { ctx.Response.Write(js.Serialize(new { ok = true, period = p, items = TrainingDetailRepo.TopWatchers(since) })); }
        catch (Exception ex) { ctx.Response.StatusCode = 500; ctx.Response.Write(js.Serialize(new { ok = false, error = "Liste okunamadı: " + ex.Message })); }
    }
}
