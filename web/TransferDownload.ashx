<%@ WebHandler Language="C#" Class="TransferDownloadHandler" %>

using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.SessionState;

// Herkese acik indirme: ?k={token}&f={fileId}. Giris gerektirmez; kisiye ozel token + (varsa) sifre dogrulamasi.
// Range destegi: yarida kalan indirme kaldigi yerden devam eder. Dosya bellege alinmadan diskten akitilir.
public class TransferDownloadHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }

    public void ProcessRequest(HttpContext ctx)
    {
        string token = ctx.Request.QueryString["k"] ?? "";
        int fid; int.TryParse(ctx.Request.QueryString["f"], out fid);
        TransferRecipient rc = TransferRepo.GetRecipientByToken(token);
        TransferInfo t = rc != null ? TransferRepo.GetTransfer(rc.TransferId) : null;
        TransferFile file = fid > 0 ? TransferRepo.GetFile(fid) : null;
        if (rc == null || t == null || file == null || file.TransferId != t.TransferId || !file.IsComplete) { Deny(ctx, 404, "Dosya bulunamadı."); return; }
        if (!t.IsLive) { Deny(ctx, 410, "Bu paylaşımın süresi doldu veya iptal edildi."); return; }
        if (t.HasPassword && !(ctx.Session["tp_" + token] is bool)) { ctx.Response.Redirect(VirtualPathUtility.ToAbsolute("~/t/" + token), false); return; }

        string path = TransferRepo.FilePath(t.TransferId, file.FileId);
        if (!File.Exists(path)) { Deny(ctx, 404, "Dosya bulunamadı."); return; }
        long size = new FileInfo(path).Length;

        // Range: bytes=start-end
        long start = 0, end = size - 1;
        string range = ctx.Request.Headers["Range"];
        bool partial = false;
        if (!string.IsNullOrEmpty(range) && range.StartsWith("bytes="))
        {
            string[] p = range.Substring(6).Split('-');
            long a, z;
            if (p.Length == 2 && long.TryParse(p[0], out a)) { start = a; if (p[1].Length > 0 && long.TryParse(p[1], out z)) end = Math.Min(z, size - 1); partial = true; }
            else if (p.Length == 2 && p[0].Length == 0 && long.TryParse(p[1], out z)) { start = Math.Max(0, size - z); partial = true; }
            if (start > end || start >= size) { ctx.Response.StatusCode = 416; ctx.Response.AddHeader("Content-Range", "bytes */" + size); return; }
        }

        // Indirme siniri + kayit: yalniz dosyanin basindan baslayan isteklerde sayilir (devam istekleri sayilmaz)
        if (start == 0)
        {
            if (t.MaxDownloads.HasValue && rc.DownloadCount >= t.MaxDownloads.Value) { Deny(ctx, 403, "İndirme sınırına ulaşıldı."); return; }
            try { TransferRepo.LogDownload(rc.RecipientId, file.FileId, ctx.Request.UserHostAddress); } catch { }
        }

        HttpResponse r = ctx.Response;
        r.Clear();
        r.BufferOutput = false;
        r.ContentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;
        r.AddHeader("Accept-Ranges", "bytes");
        r.AddHeader("Cache-Control", "private, no-store");
        r.AddHeader("X-Robots-Tag", "noindex");
        r.AddHeader("Content-Disposition", Disposition(file.OriginalName));
        long len = end - start + 1;
        if (partial) { r.StatusCode = 206; r.AddHeader("Content-Range", "bytes " + start + "-" + end + "/" + size); }
        r.AddHeader("Content-Length", len.ToString());
        r.TransmitFile(path, start, len);
    }

    // Turkce karakterli dosya adlari icin RFC 5987 (filename*)
    private static string Disposition(string name)
    {
        StringBuilder ascii = new StringBuilder();
        foreach (char ch in name) ascii.Append(ch < 128 && ch != '"' && ch != '\\' ? ch : '_');
        return "attachment; filename=\"" + ascii + "\"; filename*=UTF-8''" + Uri.EscapeDataString(name);
    }

    private static void Deny(HttpContext ctx, int code, string msg)
    {
        ctx.Response.StatusCode = code;
        ctx.Response.ContentType = "text/plain; charset=utf-8";
        ctx.Response.Write(msg);
    }
}
