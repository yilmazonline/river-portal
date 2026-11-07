<%@ WebHandler Language="C#" Class="BroadcastTestHandler" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Toplu bildirim: e-postayi yalniz oturumdaki yoneticinin adresine test olarak gonderir (POST + ayni site).
public class BroadcastTestHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!Auth.IsLoggedIn || !Auth.IsVendorAdmin) { Out(ctx, js, false, "Bu işlem yalnız Portal Yönetimi yetkisine açık.", null); return; }
        Uri refr = ctx.Request.UrlReferrer;
        if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, ctx.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
        { Out(ctx, js, false, "İstek kaynağı doğrulanamadı.", null); return; }
        if (!Mailer.HasSmtp) { Out(ctx, js, false, "SMTP ayarı yok; e-posta gönderilemiyor.", null); return; }
        UserRow me = Repo.GetUserById(Auth.UserId);
        string to = me != null ? (me.Email ?? "").Trim() : "";
        if (to.IndexOf('@') <= 0) { Out(ctx, js, false, "Profilinizde e-posta adresi yok.", null); return; }
        var f = ctx.Request.Unvalidated.Form;
        string err; List<string> notes;
        string html = BroadcastHtml.Build(f, false, out err, out notes);
        if (html == null) { Out(ctx, js, false, err, notes); return; }
        string subject = (f["subject"] ?? "").Trim(); if (subject.Length == 0) subject = (f["title"] ?? "").Trim(); if (subject.Length == 0) subject = "Toplu bildirim";
        string sendErr;
        if (Mailer.SendTo(to, "[TEST] " + subject, html, out sendErr)) Out(ctx, js, true, "Test e-postası " + to + " adresine gönderildi.", notes);
        else Out(ctx, js, false, "Gönderilemedi: " + sendErr, notes);
    }
    private static void Out(HttpContext ctx, JavaScriptSerializer js, bool ok, string msg, List<string> notes)
    { ctx.Response.Write(js.Serialize(new { ok = ok, message = msg, notes = notes ?? new List<string>() })); }
}
