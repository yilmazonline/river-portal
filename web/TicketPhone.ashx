<%@ WebHandler Language="C#" Class="TicketPhoneHandler" %>

using System;
using System.Web;
using System.Web.SessionState;

// "Numarami sistemden sil": hatirlanan telefon numarasini aninda siler (yalniz POST, ayni site)
public class TicketPhoneHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        if (!Auth.IsLoggedIn) { ctx.Response.StatusCode = 401; ctx.Response.Write("{\"ok\":false}"); return; }
        Uri refr = ctx.Request.UrlReferrer;
        bool post = string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase);
        if (!post || refr == null || !string.Equals(refr.Host, ctx.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
        { ctx.Response.StatusCode = 403; ctx.Response.Write("{\"ok\":false}"); return; }
        try
        {
            if (ctx.Request.Form["op"] == "forget") { TicketPhone.Forget(Auth.UserId); ctx.Response.Write("{\"ok\":true}"); return; }
            ctx.Response.StatusCode = 400; ctx.Response.Write("{\"ok\":false}");
        }
        catch { ctx.Response.StatusCode = 500; ctx.Response.Write("{\"ok\":false}"); }
    }
}
