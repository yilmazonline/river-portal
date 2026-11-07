<%@ WebHandler Language="C#" Class="TrainingFileHandler" %>

using System;
using System.IO;
using System.Web;
using System.Web.SessionState;

// Akademi dokumani: yalniz giris yapmis kullanicilara. Goruntulenebilen turler sayfa icinde (inline), digerleri ve dl=1 indirme.
// Video / ses icin Range (kismi icerik) destegi: ileri geri sarma calisir.
public class TrainingFileHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        HttpResponse res = ctx.Response;
        if (!Auth.IsLoggedIn) { res.StatusCode = 403; res.Write("Bu dosya için giriş yapmanız gerekiyor."); return; }
        int id; int.TryParse(ctx.Request.QueryString["id"], out id);
        TrainingDoc d = id > 0 ? TrainingFiles.Get(id) : null;
        string path = d == null ? null : TrainingFiles.PathOf(d.StoredName);
        if (path == null) { res.StatusCode = 404; res.Write("Dosya bulunamadı."); return; }
        string ext = Path.GetExtension(d.OriginalName ?? d.StoredName) ?? "";
        string kind = TrainingFiles.Kind(ext);
        bool download = ctx.Request.QueryString["dl"] == "1" || kind == "none";
        string name = Path.GetFileName(d.OriginalName ?? d.StoredName);
        string ascii = System.Text.RegularExpressions.Regex.Replace(name, @"[^\x20-\x7E]|""", "_");
        res.ContentType = TrainingFiles.Mime(ext);
        res.AddHeader("Content-Disposition", (download ? "attachment" : "inline") + "; filename=\"" + ascii + "\"; filename*=UTF-8''" + Uri.EscapeDataString(name));
        res.AddHeader("X-Content-Type-Options", "nosniff");
        res.AddHeader("Accept-Ranges", "bytes");
        res.Cache.SetCacheability(HttpCacheability.Private);
        res.Cache.SetMaxAge(TimeSpan.FromHours(1));
        // SVG ve metin: dogrudan acilsa bile icindeki betik calismaz
        string e = ext.ToLowerInvariant();
        if (e == ".svg" || kind == "text" || kind == "csv")
            res.AddHeader("Content-Security-Policy", "default-src 'none'; style-src 'unsafe-inline'; img-src data:; sandbox");
        long len = new FileInfo(path).Length, start = 0, end = len - 1;
        string range = ctx.Request.Headers["Range"];
        if (!download && !string.IsNullOrEmpty(range) && range.StartsWith("bytes=") && len > 0)
        {
            string[] p = range.Substring(6).Split(',')[0].Split('-');
            long a, b;
            bool ha = long.TryParse(p[0].Trim(), out a);
            if (!(p.Length > 1 && long.TryParse(p[1].Trim(), out b))) b = -1;
            if (ha) { start = a; end = b >= 0 ? Math.Min(b, len - 1) : len - 1; }
            else if (b > 0) { start = Math.Max(0, len - b); end = len - 1; }   // son N bayt
            if (start > end || start >= len) { res.StatusCode = 416; res.AddHeader("Content-Range", "bytes */" + len); return; }
            res.StatusCode = 206;
            res.AddHeader("Content-Range", "bytes " + start + "-" + end + "/" + len);
        }
        res.AddHeader("Content-Length", (end - start + 1).ToString());
        if (string.Equals(ctx.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase)) return;
        res.TransmitFile(path, start, end - start + 1);
    }
}
