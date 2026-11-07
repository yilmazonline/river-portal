<%@ WebHandler Language="C#" Class="VersionModuleHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Versiyon duyurusu formu: modul listesine yeni modul ekler. Yalniz icerik editoru / yonetici; POST + ayni site.
public class VersionModuleHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!Auth.IsLoggedIn || !Auth.CanEditTraining) { Fail(ctx, js, "Bu işlem için yetkiniz yok.", 403); return; }
        Uri refr = ctx.Request.UrlReferrer;
        if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, ctx.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
        { Fail(ctx, js, "İstek kaynağı doğrulanamadı.", 403); return; }
        string name = VersionModuleRepo.Clean(ctx.Request.Unvalidated.Form["name"]);
        if (name.Length < 2) { Fail(ctx, js, "Modül adı en az 2 karakter olmalı.", 400); return; }
        try
        {
            string saved = VersionModuleRepo.Add(name, Auth.UserId);
            ctx.Response.Write(js.Serialize(new { ok = true, name = saved, existed = !string.Equals(saved, name, StringComparison.Ordinal) }));
        }
        catch (Exception ex) { Fail(ctx, js, "Modül eklenemedi. Veritabanında 51 numaralı güncellemenin çalıştırıldığından emin olun. (" + ex.Message + ")", 500); }
    }
    private static void Fail(HttpContext ctx, JavaScriptSerializer js, string msg, int code) { ctx.Response.StatusCode = code; ctx.Response.Write(js.Serialize(new { ok = false, error = msg })); }
}
