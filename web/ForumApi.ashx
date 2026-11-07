<%@ WebHandler Language="C#" Class="ForumApiHandler" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Forum islemleri (JSON). Okuma GET, yazma POST + ayni site. Gelistirme asamasinda yalniz Fol personeli (ForumRepo.CanAccess).
public class ForumApiHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    private static readonly string[] IMG_EXT = { ".jpg", ".jpeg", ".png", ".gif", ".webp" }, VID_EXT = { ".mp4", ".webm" };

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer(); js.MaxJsonLength = 8 * 1024 * 1024;
        if (!ForumRepo.CanAccess) { Fail(ctx, js, "Forum henüz kullanıma açılmadı.", 403); return; }
        HttpRequest q = ctx.Request;
        string op = q["op"] ?? "";
        int uid = Auth.UserId; bool mod = ForumRepo.IsModerator;
        bool post = string.Equals(q.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase);
        if (post)
        {
            Uri refr = q.UrlReferrer;
            if (refr == null || !string.Equals(refr.Host, q.Url.Host, StringComparison.OrdinalIgnoreCase)) { Fail(ctx, js, "İstek kaynağı doğrulanamadı.", 403); return; }
        }
        int id; int.TryParse(q["id"], out id);
        try
        {
            if (!post && op == "feed")
            {
                string tab = q["tab"] == "announcements" || q["tab"] == "saved" ? q["tab"] : "feed";
                int before, author; int.TryParse(q["before"], out before); int.TryParse(q["author"], out author); bool more;
                List<Dictionary<string, object>> items = ForumRepo.Feed(uid, mod, tab, before, author, q["tag"], out more);
                string note; bool closed = ForumRepo.IsClosed(out note);
                Ok(ctx, js, new { ok = true, items = items, more = more, mod = mod, vendor = Auth.IsVendorFirm, reports = mod ? ForumRepo.OpenReportCount() : 0,
                                  closed = closed, closedNote = note ?? "", restriction = mod ? null : ForumRepo.ActiveRestriction(uid) });
            }
            else if (!post && op == "trending") Ok(ctx, js, new { ok = true, items = ForumRepo.Trending(30, 8) });
            else if (post && op == "accept")
            {
                if (q.Form["agree"] != "1") { Fail(ctx, js, "Devam etmek için kuralları onaylayın.", 400); return; }
                ForumRepo.Accept(uid, q.UserHostAddress); Ok(ctx, js, new { ok = true });
            }
            else if (!post && op == "whoami")
            {
                // Teshis (yalniz Fol personeli): oturumdaki ve veritabanindaki yetki yan yana
                if (!Auth.IsVendorFirm) { Fail(ctx, js, "Yetkiniz yok.", 403); return; }
                UserRow me = Repo.GetUserById(uid);
                Ok(ctx, js, new { ok = true, userId = uid, session = new { role = Auth.Role, permissions = Auth.Permissions },
                                  database = me == null ? null : new { role = me.Role, permissions = me.Permissions, companyType = me.CompanyType },
                                  forumAdminInDatabase = me != null && ForumRepo.IsAdminUser(me.Role, me.Permissions, me.CompanyType), isModerator = mod,
                                  note = "isModerator 60 saniyelik önbellekten gelir; yetki yeni verildiyse en geç bir dakika içinde güncellenir." });
            }
            else if (!post && op == "post")
            {
                Dictionary<string, object> p = ForumRepo.GetPost(id, uid, mod);
                if (p == null) { Fail(ctx, js, "Gönderi bulunamadı ya da kaldırılmış.", 404); return; }
                Ok(ctx, js, new { ok = true, item = p, comments = ForumRepo.Comments(id, uid, mod) });
            }
            else if (!post && op == "profile")
            {
                Dictionary<string, object> pr = ForumRepo.Profile(id, uid);
                if (pr == null) { Fail(ctx, js, "Kullanıcı bulunamadı.", 404); return; }
                Ok(ctx, js, new { ok = true, profile = pr });
            }
            else if (post && op == "edit")
            {
                string body = Clean(q.Unvalidated.Form["body"], ForumRepo.MAX_BODY + 1);
                if (body.Length > ForumRepo.MAX_BODY) { Fail(ctx, js, "Gönderi en fazla " + ForumRepo.MAX_BODY + " karakter olabilir.", 400); return; }
                if (!ForumRepo.EditPost(id, uid, body)) { Fail(ctx, js, "Gönderi düzenlenemedi.", 400); return; }
                Ok(ctx, js, new { ok = true, item = ForumRepo.GetPost(id, uid, mod) });
            }
            else if (!post && op == "comments") Ok(ctx, js, new { ok = true, items = ForumRepo.Comments(id, uid, mod) });
            else if (!post && op == "reports")
            {
                if (!mod) { Fail(ctx, js, "Yetkiniz yok.", 403); return; }
                Ok(ctx, js, new { ok = true, items = ForumRepo.OpenReports() });
            }
            else if (post && op == "create")
            {
                string why = ForumRepo.CannotPost(uid, mod) ?? ForumRepo.RateLimited(uid, false, mod); if (why != null) { Fail(ctx, js, why, why.StartsWith("Bir saat") ? 429 : 403); return; }
                Create(ctx, js, q, uid, mod);
            }
            else if (post && op == "comment")
            {
                string body = Clean(q.Unvalidated.Form["body"], ForumRepo.MAX_COMMENT);
                if (body.Length == 0) { Fail(ctx, js, "Yorum boş olamaz.", 400); return; }
                string why = ForumRepo.CannotPost(uid, mod) ?? ForumRepo.RateLimited(uid, true, mod); if (why != null) { Fail(ctx, js, why, why.StartsWith("Bir saat") ? 429 : 403); return; }
                int par; int? parent = int.TryParse(q.Form["parent"], out par) && par > 0 ? (int?)par : null;
                int cid = ForumRepo.AddComment(id, uid, body, Auth.FullName, parent);
                if (cid == 0) { Fail(ctx, js, "Gönderi bulunamadı ya da kaldırılmış.", 404); return; }
                if (cid < 0) { Fail(ctx, js, "Yanıtlanan yorum bulunamadı.", 404); return; }
                Ok(ctx, js, new { ok = true, items = ForumRepo.Comments(id, uid, mod) });
            }
            else if (post && op == "delcomment") Ok(ctx, js, new { ok = ForumRepo.DeleteComment(id, uid, mod, mod ? Clean(q.Unvalidated.Form["reason"], 1000) : null, Auth.FullName) });
            else if (post && op == "delpost") Ok(ctx, js, new { ok = ForumRepo.DeletePost(id, uid, mod, mod ? Clean(q.Unvalidated.Form["reason"], 1000) : null, Auth.FullName) });
            else if (post && op == "like") { int c; bool on = ForumRepo.ToggleLike(id, uid, Auth.FullName, out c); Ok(ctx, js, new { ok = true, on = on, count = c }); }
            else if (post && op == "clike") { int c; bool on = ForumRepo.ToggleCommentLike(id, uid, Auth.FullName, out c); Ok(ctx, js, new { ok = true, on = on, count = c }); }
            else if (post && op == "save") Ok(ctx, js, new { ok = true, on = ForumRepo.ToggleSave(id, uid) });
            else if (post && op == "pin")
            {
                if (!mod) { Fail(ctx, js, "Yalnız moderatörler sabitleyebilir.", 403); return; }
                ForumRepo.SetPinned(id, q.Form["on"] == "1"); Ok(ctx, js, new { ok = true });
            }
            else if (post && op == "report")
            {
                int c; int? comment = int.TryParse(q.Form["comment"], out c) && c > 0 ? (int?)c : null;
                ForumRepo.Report(id, comment, uid, Clean(q.Unvalidated.Form["reason"], 500));
                Ok(ctx, js, new { ok = true });
            }
            else if (post && (op == "resolve" || op == "unhide"))
            {
                if (!mod) { Fail(ctx, js, "Yetkiniz yok.", 403); return; }
                int c; int? comment = int.TryParse(q.Form["comment"], out c) && c > 0 ? (int?)c : null;
                if (op == "unhide") ForumRepo.Unhide(id, comment, uid, Auth.FullName); else ForumRepo.Resolve(id, comment, q.Form["hide"] == "1", uid, Clean(q.Unvalidated.Form["reason"], 1000), Auth.FullName);
                Ok(ctx, js, new { ok = true, reports = ForumRepo.OpenReportCount() });
            }
            // ---- Forum Admini islemleri
            else if (op == "restrictions" || op == "log" || op == "restrict" || op == "unrestrict" || op == "settings" || op == "modview" || op == "restore" || op == "purge")
            {
                if (!mod) { Fail(ctx, js, "Bu işlem yalnız Forum Adminlerine açık.", 403); return; }
                int cm; int? mc = int.TryParse(q["comment"], out cm) && cm > 0 ? (int?)cm : null;
                if (!post && op == "modview")
                {
                    Dictionary<string, object> v = ForumRepo.ModView(id, mc, uid);
                    if (v == null) { Fail(ctx, js, "İçerik kalıcı olarak silinmiş; yalnız kayıttaki metni kaldı.", 404); return; }
                    Ok(ctx, js, new { ok = true, view = v }); return;
                }
                if (post && op == "restore") { bool okr = ForumRepo.Restore(id, mc, uid, Auth.FullName); if (!okr) { Fail(ctx, js, "İçerik zaten yayında ya da kalıcı olarak silinmiş.", 400); return; } Ok(ctx, js, new { ok = true }); return; }
                if (post && op == "purge") { bool okp = ForumRepo.Purge(id, mc, uid, Auth.FullName, Clean(q.Unvalidated.Form["reason"], 1000)); if (!okp) { Fail(ctx, js, "İçerik bulunamadı ya da zaten kalıcı olarak silinmiş.", 404); return; } Ok(ctx, js, new { ok = true }); return; }
                if (!post && op == "restrictions") Ok(ctx, js, new { ok = true, items = ForumRepo.Restrictions() });
                else if (!post && op == "log") { int before; int.TryParse(q["before"], out before); bool more; List<Dictionary<string, object>> l = ForumRepo.ModLog(before, out more); Ok(ctx, js, new { ok = true, items = l, more = more }); }
                else if (post && op == "restrict")
                {
                    int target, days; int.TryParse(q.Form["user"], out target); int.TryParse(q.Form["days"], out days);
                    if (target <= 0 || target == uid) { Fail(ctx, js, "Geçerli bir kullanıcı seçin.", 400); return; }
                    if (days != 0 && days != 1 && days != 7 && days != 30) { Fail(ctx, js, "Geçersiz süre.", 400); return; }
                    UserRow tu = Repo.GetUserById(target);
                    if (tu != null && ForumRepo.IsAdminUser(tu.Role, tu.Permissions, tu.CompanyType)) { Fail(ctx, js, "Forum adminleri kısıtlanamaz.", 400); return; }
                    ForumRepo.Restrict(target, days, Clean(q.Unvalidated.Form["reason"], 1000), uid, Auth.FullName);
                    Ok(ctx, js, new { ok = true });
                }
                else if (post && op == "unrestrict") { ForumRepo.Unrestrict(id, uid, Auth.FullName); Ok(ctx, js, new { ok = true }); }
                else if (post && op == "settings") { ForumRepo.SetClosed(q.Form["closed"] == "1", Clean(q.Unvalidated.Form["note"], 500), uid, Auth.FullName); Ok(ctx, js, new { ok = true }); }
                else Fail(ctx, js, "Geçersiz işlem.", 400);
            }
            else Fail(ctx, js, "Geçersiz işlem.", 400);
        }
        catch (Exception ex) { Fail(ctx, js, "İşlem tamamlanamadı: " + ex.Message, 500); }
    }

    /// <summary>Yeni gonderi: metin (en fazla 4000) + 4 gorsel (5 MB) ya da 1 video (25 MB). Sinirlar ve dosya icerigi sunucuda da denetlenir.</summary>
    private void Create(HttpContext ctx, JavaScriptSerializer js, HttpRequest q, int uid, bool mod)
    {
        string body = Clean(q.Unvalidated.Form["body"], ForumRepo.MAX_BODY + 1);
        if (body.Length > ForumRepo.MAX_BODY) { Fail(ctx, js, "Gönderi en fazla " + ForumRepo.MAX_BODY + " karakter olabilir.", 400); return; }
        int images = 0, videos = 0;
        for (int i = 0; i < q.Files.Count; i++)
        {
            HttpPostedFile f = q.Files[i]; if (f == null || f.ContentLength == 0) continue;
            string ext = Path.GetExtension(f.FileName ?? "").ToLowerInvariant();
            if (Array.IndexOf(IMG_EXT, ext) >= 0) { images++; if (f.ContentLength > ForumRepo.MAX_IMAGE_BYTES) { Fail(ctx, js, "Görseller en fazla 5 MB olabilir: " + f.FileName, 400); return; } }
            else if (Array.IndexOf(VID_EXT, ext) >= 0) { videos++; if (f.ContentLength > ForumRepo.MAX_VIDEO_BYTES) { Fail(ctx, js, "Video 25 MB sınırını aşıyor. YouTube ya da Vimeo'ya yükleyip bağlantısını ekleyebilirsiniz.", 400); return; } }
            else { Fail(ctx, js, "Desteklenmeyen dosya türü: " + f.FileName + ". Görsel (JPG, PNG, GIF, WEBP) ya da video (MP4, WEBM) ekleyin.", 400); return; }
        }
        if (images > ForumRepo.MAX_IMAGES) { Fail(ctx, js, "Bir gönderiye en fazla 4 görsel eklenebilir.", 400); return; }
        if (videos > 1 || (videos == 1 && images > 0)) { Fail(ctx, js, "Bir gönderiye ya görseller ya da tek bir video eklenebilir.", 400); return; }
        if (body.Length == 0 && images + videos == 0) { Fail(ctx, js, "Gönderi boş olamaz.", 400); return; }

        string dir = ForumRepo.FilesDir; Directory.CreateDirectory(dir);
        List<ForumRepo.MediaIn> media = new List<ForumRepo.MediaIn>(); List<string> written = new List<string>();
        try
        {
            for (int i = 0; i < q.Files.Count; i++)
            {
                HttpPostedFile f = q.Files[i]; if (f == null || f.ContentLength == 0) continue;
                byte[] head = new byte[16]; int n = f.InputStream.Read(head, 0, head.Length); f.InputStream.Position = 0;
                string ct = ForumRepo.Sniff(head, n);
                string ext = Path.GetExtension(f.FileName ?? "").ToLowerInvariant();
                bool wantVideo = Array.IndexOf(VID_EXT, ext) >= 0;
                if (ct == null || (wantVideo != ct.StartsWith("video/"))) { Cleanup(written); Fail(ctx, js, "Dosyanın içeriği türüyle uyuşmuyor: " + f.FileName, 400); return; }
                string stored = Guid.NewGuid().ToString("N") + (ct == "image/jpeg" ? ".jpg" : "." + ct.Split('/')[1]);
                f.SaveAs(Path.Combine(dir, stored)); written.Add(Path.Combine(dir, stored));
                ForumRepo.MediaIn m = new ForumRepo.MediaIn();
                m.Kind = wantVideo ? "video" : "image"; m.StoredName = stored; m.OriginalName = ForumRepo.Cut(Path.GetFileName(f.FileName), 260); m.ContentType = ct; m.Size = f.ContentLength;
                media.Add(m);
            }
            bool ann = mod && q.Form["ann"] == "1", pin = mod && q.Form["pin"] == "1";
            int id = ForumRepo.CreatePost(uid, Auth.CompanyId, body, ann, pin, media);
            Ok(ctx, js, new { ok = true, item = ForumRepo.GetPost(id, uid, mod) });
        }
        catch { Cleanup(written); throw; }
    }

    private static void Cleanup(List<string> files) { foreach (string f in files) try { File.Delete(f); } catch { } }
    private static string Clean(string s, int max)
    {
        s = (s ?? "").Replace("\r\n", "\n").Trim();
        return s.Length > max ? s.Substring(0, max) : s;
    }
    private static void Ok(HttpContext ctx, JavaScriptSerializer js, object o) { ctx.Response.Write(js.Serialize(o)); }
    private static void Fail(HttpContext ctx, JavaScriptSerializer js, string msg, int code) { ctx.Response.StatusCode = code; ctx.Response.Write(js.Serialize(new { ok = false, error = msg })); }
}
