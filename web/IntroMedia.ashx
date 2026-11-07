<%@ WebHandler Language="C#" Class="IntroMediaHandler" %>

using System;
using System.IO;
using System.Web;
using System.Web.SessionState;

// Karsilama videosu / kapak gorseli: yalniz giris yapmis kullanicilara.
// Dosyalar App_Data/intro altinda (IIS bu klasoru dogrudan sunmaz). Range destegi: ileri-geri sarma.
//   IntroMedia.ashx?f=video  -> intro-{rol}.mp4   IntroMedia.ashx?f=poster -> poster-{rol}.jpg (rol: IntroVideo.RoleKey)
public class IntroMediaHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }

    public void ProcessRequest(HttpContext ctx)
    {
        if (!Auth.IsLoggedIn) { ctx.Response.StatusCode = 403; return; }
        bool poster = ctx.Request.QueryString["f"] == "poster";
        string role = IntroVideo.RoleKey();                       // rol sunucuda belirlenir
        string path = poster ? IntroVideo.PosterPath(role) : IntroVideo.VideoPath(role);
        if (!File.Exists(path)) { ctx.Response.StatusCode = 404; return; }
        long size = new FileInfo(path).Length;

        long start = 0, end = size - 1; bool partial = false;
        string range = ctx.Request.Headers["Range"];
        if (!poster && !string.IsNullOrEmpty(range) && range.StartsWith("bytes="))
        {
            string[] p = range.Substring(6).Split('-');
            long a, z;
            if (p.Length == 2 && long.TryParse(p[0], out a)) { start = a; if (p[1].Length > 0 && long.TryParse(p[1], out z)) end = Math.Min(z, size - 1); partial = true; }
            else if (p.Length == 2 && p[0].Length == 0 && long.TryParse(p[1], out z)) { start = Math.Max(0, size - z); partial = true; }
            if (start > end || start >= size) { ctx.Response.StatusCode = 416; ctx.Response.AddHeader("Content-Range", "bytes */" + size); return; }
        }

        HttpResponse r = ctx.Response;
        r.Clear();
        r.BufferOutput = false;
        r.ContentType = poster ? "image/jpeg" : "video/mp4";
        r.AddHeader("Accept-Ranges", "bytes");
        r.AddHeader("Cache-Control", "private, max-age=86400");   // tarayicida onbellek; Cloudflare'de degil (yetkili icerik)
        r.AddHeader("X-Robots-Tag", "noindex");
        long len = end - start + 1;
        if (partial) { r.StatusCode = 206; r.AddHeader("Content-Range", "bytes " + start + "-" + end + "/" + size); }
        r.AddHeader("Content-Length", len.ToString());
        r.TransmitFile(path, start, len);
    }
}
