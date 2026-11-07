<%@ WebHandler Language="C#" Class="SalesSuggestActionHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Satis onerileri: book (is ortagina satis kaydi) / dismiss (kanal satisi degil) / restore. Yalniz Finans yetkisi, POST, ayni site.
public class SalesSuggestActionHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!Auth.IsLoggedIn || !Auth.CanFinance) { ctx.Response.StatusCode = 403; ctx.Response.Write("{\"ok\":false}"); return; }
        Uri refr = ctx.Request.UrlReferrer;
        if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, ctx.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
        { ctx.Response.StatusCode = 403; ctx.Response.Write("{\"ok\":false}"); return; }
        int id, cid; int.TryParse(ctx.Request.Form["id"], out id); int.TryParse(ctx.Request.Form["company"], out cid);
        string op = ctx.Request.Form["op"] ?? "", err;
        try
        {
            if (op == "book") err = cid > 0 ? SalesSuggestions.Book(id, cid, Auth.UserId) : "İş ortağı seçin.";
            else if (op == "dismiss") err = SalesSuggestions.Dismiss(id, Auth.UserId);
            else if (op == "restore") err = SalesSuggestions.Restore(id);
            else err = "Geçersiz işlem.";
            ctx.Response.Write(js.Serialize(new { ok = err == null, error = err, pending = SalesSuggestions.PendingCount() }));
        }
        catch (Exception ex) { ctx.Response.StatusCode = 500; ctx.Response.Write(js.Serialize(new { ok = false, error = "İşlem tamamlanamadı: " + ex.Message })); }
    }
}
