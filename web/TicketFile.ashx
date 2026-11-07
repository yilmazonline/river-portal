<%@ WebHandler Language="C#" Class="TicketFileHandler" %>

using System;
using System.IO;
using System.Web;
using System.Web.SessionState;

// Destek talebi eki: yetki kontrollu sunum. ?id=EkId  [&inline=1 → onizlenebilir turler sayfa icinde]
// ticketfiles/ klasorune dogrudan erisim web.config'te kapali; ekler yalniz bu servisten acilir.
public class TicketFileHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        HttpResponse res = ctx.Response;
        res.Headers["X-Content-Type-Options"] = "nosniff";
        if (!Auth.IsLoggedIn) { res.StatusCode = 401; res.Write("Oturum gerekli."); return; }
        int id; int.TryParse(ctx.Request.QueryString["id"], out id);
        TicketFiles.Info a = id > 0 ? TicketFiles.Get(id) : null;
        if (a == null || !TicketFiles.CanSee(a)) { res.StatusCode = 404; res.Write("Dosya bulunamadı."); return; }   // yetkisizlige de 404: varligi belli olmasin

        string path = Path.Combine(ctx.Server.MapPath("~/ticketfiles"), Path.GetFileName(a.StoredName));   // klasor disina cikamaz
        if (!File.Exists(path)) { res.StatusCode = 404; res.Write("Dosya bulunamadı."); return; }

        string kind = TicketFiles.PreviewKind(a.OriginalName);
        bool inline = ctx.Request.QueryString["inline"] == "1" && kind.Length > 0;
        // icerik turu uzantidan: kayittaki ContentType'a guvenilmez; izin listesi disindaki eski ekler yalniz indirilebilir
        res.ContentType = inline || TicketFiles.IsAllowed(a.OriginalName) ? TicketFiles.MimeOf(a.OriginalName) : "application/octet-stream";
        if (inline && kind == "text") res.Headers["Content-Security-Policy"] = "default-src 'none'; sandbox";
        if (inline && kind == "image") res.Headers["Content-Security-Policy"] = "default-src 'none'; img-src 'self'; style-src 'unsafe-inline'; sandbox";
        string name = Path.GetFileName(a.OriginalName ?? "dosya");
        string ascii = System.Text.RegularExpressions.Regex.Replace(name, @"[^\x20-\x7E]|[""\\]", "_");
        res.AddHeader("Content-Disposition", (inline ? "inline" : "attachment") + "; filename=\"" + ascii + "\"; filename*=UTF-8''" + Uri.EscapeDataString(name));
        res.Cache.SetCacheability(HttpCacheability.Private);
        res.Cache.SetMaxAge(TimeSpan.FromHours(1));
        res.TransmitFile(path);
    }
}
