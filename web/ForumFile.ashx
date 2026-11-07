<%@ WebHandler Language="C#" Class="ForumFileHandler" %>

using System;
using System.IO;
using System.Web;
using System.Web.SessionState;

// Forum gorsel ve videolari: yalniz foruma erisebilenler; gizlenmis gonderinin dosyasi yalniz sahibine ve moderatorlere.
// Video icin HTTP Range (206) destegi: ileri-geri sarma ve parca parca yukleme.
public class ForumFileHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        HttpResponse res = ctx.Response;
        if (!ForumRepo.CanAccess) { res.StatusCode = 403; return; }
        int id; int.TryParse(ctx.Request["id"], out id);
        string path, ct, kind;
        if (id <= 0 || !ForumRepo.MediaFor(id, Auth.UserId, ForumRepo.IsModerator, out path, out ct, out kind)) { res.StatusCode = 404; return; }
        res.Cache.SetCacheability(HttpCacheability.Private);
        res.Cache.SetMaxAge(TimeSpan.FromHours(12));
        res.AppendHeader("X-Content-Type-Options", "nosniff");
        res.AppendHeader("Content-Security-Policy", "default-src 'none'; sandbox");
        res.ContentType = ct;
        long size = new FileInfo(path).Length, start = 0, end = size - 1;
        res.AppendHeader("Accept-Ranges", "bytes");
        string range = ctx.Request.Headers["Range"];
        if (!string.IsNullOrEmpty(range) && range.StartsWith("bytes="))
        {
            string[] p = range.Substring(6).Split(',')[0].Split('-');
            long a, b;
            if (p[0].Length == 0 && long.TryParse(p[1], out b)) { start = Math.Max(0, size - b); }              // son N bayt
            else if (long.TryParse(p[0], out a)) { start = a; if (p.Length > 1 && long.TryParse(p[1], out b)) end = Math.Min(b, size - 1); }
            if (start > end || start >= size) { res.StatusCode = 416; res.AppendHeader("Content-Range", "bytes */" + size); return; }
            res.StatusCode = 206;
            res.AppendHeader("Content-Range", "bytes " + start + "-" + end + "/" + size);
        }
        long len = end - start + 1;
        res.AppendHeader("Content-Length", len.ToString());
        res.BufferOutput = false;
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            fs.Position = start; byte[] buf = new byte[64 * 1024];
            while (len > 0 && res.IsClientConnected)
            {
                int n = fs.Read(buf, 0, (int)Math.Min(buf.Length, len)); if (n <= 0) break;
                res.OutputStream.Write(buf, 0, n); len -= n;
            }
        }
    }
}
