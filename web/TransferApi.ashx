<%@ WebHandler Language="C#" Class="TransferApiHandler" %>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Dosya paylasimi API (yalniz uretici personeli)
//   POST ?action=create    form: title, message, days, maxdl, password, fname[], fsize[], ftype[]  -> {transferId, files:[{id,name,size}]}
//   POST ?action=chunk&t=&f=&o=   govde: ham parca (<= 16 MB)  -> {uploaded, complete}
//   POST ?action=finalize  form: t, uids (virgullu), emails (serbest metin) -> {sent, mailed, errors}
public class TransferApiHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        if (!Auth.IsLoggedIn || !Auth.IsVendorFirm) { Fail(ctx, 403, "forbidden"); return; }
        if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Fail(ctx, 405, "method"); return; }
        string action = ctx.Request.QueryString["action"] ?? "";
        try
        {
            if (action == "create") Create(ctx);
            else if (action == "chunk") Chunk(ctx);
            else if (action == "finalize") Finalize(ctx);
            else Fail(ctx, 400, "action");
        }
        catch (Exception ex) { Fail(ctx, 500, "server: " + ex.Message); }
    }

    private static void Fail(HttpContext ctx, int code, string msg)
    {
        ctx.Response.StatusCode = code;
        ctx.Response.Write(new JavaScriptSerializer().Serialize(new Dictionary<string, object> { { "ok", false }, { "error", msg } }));
    }
    private static void Ok(HttpContext ctx, Dictionary<string, object> d)
    {
        d["ok"] = true;
        ctx.Response.Write(new JavaScriptSerializer().Serialize(d));
    }

    // ---------- 1) Transfer olustur ----------
    private void Create(HttpContext ctx)
    {
        var f = ctx.Request.Unvalidated.Form;
        string title = (f["title"] ?? "").Trim();
        string message = (f["message"] ?? "").Trim();
        if (title.Length == 0) { Fail(ctx, 400, "Başlık zorunludur."); return; }
        if (title.Length > 200) title = title.Substring(0, 200);
        if (message.Length > 2000) message = message.Substring(0, 2000);

        int days; int.TryParse(f["days"], out days);
        if (days < 1 || days > 30) days = 7;
        int maxdl; int.TryParse(f["maxdl"], out maxdl);
        string pwd = (f["password"] ?? "").Trim();

        string[] names = f.GetValues("fname") ?? new string[0];
        string[] sizes = f.GetValues("fsize") ?? new string[0];
        string[] types = f.GetValues("ftype") ?? new string[0];
        if (names.Length == 0 || names.Length != sizes.Length) { Fail(ctx, 400, "Dosya seçilmedi."); return; }
        if (names.Length > 50) { Fail(ctx, 400, "Bir paylaşımda en fazla 50 dosya olabilir."); return; }

        List<string> n = new List<string>(); List<long> s = new List<long>(); List<string> ty = new List<string>();
        long total = 0;
        for (int i = 0; i < names.Length; i++)
        {
            long sz; if (!long.TryParse(sizes[i], out sz) || sz <= 0) { Fail(ctx, 400, "Boş dosya gönderilemez: " + names[i]); return; }
            string nm = Path.GetFileName((names[i] ?? "").Trim());
            if (nm.Length == 0) nm = "dosya";
            if (nm.Length > 250) nm = nm.Substring(nm.Length - 250);
            n.Add(nm); s.Add(sz); ty.Add(i < types.Length ? types[i] : null); total += sz;
        }
        if (total > TransferRepo.MaxBytes)
        { Fail(ctx, 400, "Toplam boyut " + TransferRepo.FormatSize(total) + "; en fazla " + TransferRepo.FormatSize(TransferRepo.MaxBytes) + " gönderilebilir."); return; }

        List<int> ids;
        int tid = TransferRepo.CreateTransfer(title, message.Length > 0 ? message : null, Auth.UserId, Auth.FullName, DateTime.UtcNow.AddDays(days),
            maxdl > 0 ? (int?)Math.Min(maxdl, 1000) : null, pwd.Length > 0 ? PasswordHasher.Hash(pwd) : null, n, s, ty, out ids);

        List<object> files = new List<object>();
        for (int i = 0; i < ids.Count; i++) files.Add(new Dictionary<string, object> { { "id", ids[i] }, { "name", n[i] }, { "size", s[i] } });
        Ok(ctx, new Dictionary<string, object> { { "transferId", tid }, { "chunk", TransferRepo.ChunkBytes }, { "files", files } });
    }

    // ---------- 2) Parca yukle ----------
    private void Chunk(HttpContext ctx)
    {
        int tid, fid; long off;
        int.TryParse(ctx.Request.QueryString["t"], out tid);
        int.TryParse(ctx.Request.QueryString["f"], out fid);
        long.TryParse(ctx.Request.QueryString["o"], out off);
        TransferInfo t = tid > 0 ? TransferRepo.GetTransfer(tid) : null;
        TransferFile file = fid > 0 ? TransferRepo.GetFile(fid) : null;
        if (t == null || file == null || file.TransferId != tid || t.CreatedBy != Auth.UserId) { Fail(ctx, 404, "notfound"); return; }
        if (t.Status != "Uploading") { Fail(ctx, 409, "Paylaşım artık yükleme kabul etmiyor."); return; }

        string path = TransferRepo.FilePath(tid, fid);
        long have = File.Exists(path) ? new FileInfo(path).Length : 0;
        if (off > have) { ctx.Response.StatusCode = 409; Ok(ctx, new Dictionary<string, object> { { "expected", have } }); return; }

        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1 << 16))
        {
            if (off < have) fs.SetLength(off);          // tekrar gonderilen parca: ustune yaz
            fs.Seek(off, SeekOrigin.Begin);
            Stream input = ctx.Request.GetBufferlessInputStream();
            byte[] buf = new byte[1 << 16];
            int r; long written = 0;
            while ((r = input.Read(buf, 0, buf.Length)) > 0)
            {
                written += r;
                if (written > TransferRepo.ChunkBytes + 1024 || off + written > file.SizeBytes) { fs.SetLength(off); Fail(ctx, 400, "Parça boyutu geçersiz."); return; }
                fs.Write(buf, 0, r);
            }
            have = off + written;
        }
        bool complete = have == file.SizeBytes;
        TransferRepo.SetUploaded(fid, have, complete);
        Ok(ctx, new Dictionary<string, object> { { "uploaded", have }, { "complete", complete } });
    }

    // ---------- 3) Gonder ----------
    private void Finalize(HttpContext ctx)
    {
        var f = ctx.Request.Unvalidated.Form;
        int tid; int.TryParse(f["t"], out tid);
        TransferInfo t = tid > 0 ? TransferRepo.GetTransfer(tid) : null;
        if (t == null || t.CreatedBy != Auth.UserId) { Fail(ctx, 404, "notfound"); return; }
        if (t.Status != "Uploading") { Fail(ctx, 409, "Bu paylaşım zaten gönderildi."); return; }
        List<TransferFile> files = TransferRepo.GetFiles(tid);
        for (int i = 0; i < files.Count; i++) if (!files[i].IsComplete) { Fail(ctx, 409, "Yüklemesi tamamlanmayan dosya var: " + files[i].OriginalName); return; }

        // Alicilar: portal kullanicilari + harici e-postalar (e-postaya gore tekillestirilir)
        Dictionary<string, bool> seen = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        List<object[]> rc = new List<object[]>();   // email, name, userId, companyId, companyName
        foreach (string p in (f["uids"] ?? "").Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            int uid; if (!int.TryParse(p, out uid)) continue;
            UserRow u = Repo.GetUserById(uid);
            if (u == null || string.IsNullOrEmpty(u.Email) || seen.ContainsKey(u.Email)) continue;
            seen[u.Email] = true;
            rc.Add(new object[] { u.Email.Trim(), u.FullName, (int?)u.UserId, (int?)u.CompanyId, u.CompanyName });
        }
        Regex rx = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        foreach (string p in (f["emails"] ?? "").Split(new char[] { ',', ';', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string e = p.Trim().Trim('<', '>');
            if (e.Length > 256 || !rx.IsMatch(e) || seen.ContainsKey(e)) continue;
            seen[e] = true;
            rc.Add(new object[] { e, null, null, null, null });
        }
        if (rc.Count == 0) { Fail(ctx, 400, "En az bir alıcı seçin veya e-posta adresi girin."); return; }
        if (rc.Count > 500) { Fail(ctx, 400, "Bir paylaşımda en fazla 500 alıcı olabilir."); return; }

        // Mail icerigi (dosya listesi herkeste ayni, link kisiye ozel)
        List<string> fn = new List<string>(), fs = new List<string>();
        for (int i = 0; i < files.Count; i++) { fn.Add(files[i].OriginalName); fs.Add(TransferRepo.FormatSize(files[i].SizeBytes)); }
        string total = TransferRepo.FormatSize(t.TotalBytes);
        string expires = t.ExpiresAt.ToTrLocal().ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR"));
        string b = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (b.Length == 0) b = ctx.Request.Url.GetLeftPart(UriPartial.Authority);
        b = b.TrimEnd('/');

        int mailed = 0, errors = 0;
        for (int i = 0; i < rc.Count; i++)
        {
            string token = TransferRepo.NewToken();
            string email = (string)rc[i][0], name = (string)rc[i][1], cname = (string)rc[i][4];
            int? uid = (int?)rc[i][2], cid = (int?)rc[i][3];
            TransferRepo.AddRecipient(tid, token, email, name, uid, cid, cname);
            string url = b + "/t/" + token;
            try
            {
                if (Mailer.HasSmtp)
                {
                    string html = Emails.TransferMail(Auth.FullName, t.Title, t.Message, fn, fs, total, expires, t.HasPassword, url);
                    string err;
                    if (Mailer.SendTo(email, "River · " + Auth.FullName + " sizinle dosya paylaştı: " + t.Title, html, out err)) mailed++; else errors++;
                }
            }
            catch { errors++; }
            // Portal kullanicisi: yalniz ona gorunen uygulama ici bildirim
            if (uid.HasValue && cid.HasValue)
                try
                {
                    Repo.AddUserNotification(cid.Value, uid.Value, "Dosya paylaşıldı: " + t.Title, "File shared: " + t.Title,
                        Auth.FullName + " sizinle " + files.Count + " dosya paylaştı (" + total + "). Son indirme: " + expires + ".",
                        Auth.FullName + " shared " + files.Count + " file(s) with you.", "Transfer", VirtualPathUtility.ToAbsolute("~/t/" + token));
                }
                catch { }
        }
        TransferRepo.SetStatus(tid, "Sent");
        Ok(ctx, new Dictionary<string, object> { { "sent", rc.Count }, { "mailed", mailed }, { "errors", errors }, { "transferId", tid } });
    }
}
