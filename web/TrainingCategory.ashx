<%@ WebHandler Language="C#" Class="TrainingCategoryHandler" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Akademi kategorileri: list (GET), addMain / addSub / rename / move / delete (POST + ayni site). Yalniz editorler.
public class TrainingCategoryHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!Auth.IsLoggedIn || !Auth.CanEditTraining) { Out(ctx, js, "Bu işlem yalnız Akademi editörlerine açık.", null); return; }
        if (!TrainingCats.TableExists()) { Out(ctx, js, "Kategori tablosu bulunamadı. database\\54_training_categories.sql dosyasını çalıştırın.", null); return; }
        HttpRequest q = ctx.Request;
        string scope = TrainingCats.NormScope(q["scope"]);
        if (scope == null) { Out(ctx, js, "Geçersiz kapsam.", null); return; }
        string op = q["op"] ?? "list", err = null;
        if (op != "list")
        {
            Uri refr = q.UrlReferrer;
            if (!string.Equals(q.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, q.Url.Host, StringComparison.OrdinalIgnoreCase))
            { Out(ctx, js, "İstek kaynağı doğrulanamadı.", null); return; }
            int id; int.TryParse(q.Form["id"], out id);
            string name = q.Unvalidated.Form["name"];
            try
            {
                if (op == "addMain") err = TrainingCats.AddMain(scope, name);
                else if (op == "addSub") err = TrainingCats.AddSub(scope, id, name);
                else if (op == "rename") err = TrainingCats.Rename(id, name);
                else if (op == "move") err = TrainingCats.Move(id, q.Form["dir"] == "up" ? -1 : 1);
                else if (op == "delete") err = TrainingCats.Delete(id);
                else if (op == "style") err = SaveStyle(ctx, id);
                else err = "Geçersiz işlem.";
            }
            catch (Exception ex) { err = "İşlem tamamlanamadı: " + ex.Message; }
        }
        List<object> cats = new List<object>();
        List<TStyle> st = TrainingCats.Styles();
        foreach (TCat m in TrainingCats.List(scope))
        {
            List<object> subs = new List<object>();
            foreach (TCat s in m.Subs) subs.Add(new { id = s.Id, name = s.Name, count = s.Count, style = StyleJson(TrainingCats.FindStyle(st, m.Name, s.Name)) });
            cats.Add(new { id = m.Id, name = m.Name, count = m.Count, subs = subs, style = StyleJson(TrainingCats.FindStyle(st, m.Name, "")) });
        }
        Out(ctx, js, err, cats);
    }
    private static object StyleJson(TStyle t)
    {
        if (t == null) return new { card = (string)null, cover = (string)null, tint = (int?)null, icon = (string)null, iconUrl = (string)null };
        return new { card = TrainingCats.StyleUrl(t.Card), cover = TrainingCats.StyleUrl(t.Cover), tint = t.Tint, icon = t.Icon, iconUrl = TrainingCats.CustomIconUrl(t.Icon) };
    }

    /// <summary>Gorunum: tint ("" = otomatik, 0..7), card / cover dosyalari (PNG, JPG, WebP; en fazla 5 MB), clearCard / clearCover</summary>
    private static string SaveStyle(HttpContext ctx, int id)
    {
        if (!TrainingCats.StylesExist()) return "Görünüm tablosu bulunamadı. database\\55_training_category_styles.sql dosyasını çalıştırın.";
        string g, sub;
        if (!TrainingCats.NamesOf(id, out g, out sub)) return "Kategori bulunamadı.";
        HttpRequest q = ctx.Request;
        string ts = (q.Form["tint"] ?? "").Trim(); int tv; int? tint = null;
        if (ts.Length > 0) { if (!int.TryParse(ts, out tv) || tv < 0 || tv > 14) return "Geçersiz renk."; tint = tv; }
        // kart gorseli kipi: auto (ilk video), icon (hazir ikon), myicon (yuklenen ikon), upload (yuklenen gorsel); yoksa eski davranis
        string mode = q.Form["mode"], icon = q.Form["icon"];
        if (mode == "icon" && (!TrainingCats.IsIcon(icon) || TrainingCats.IsCustomIcon(icon))) return "Bir ikon seçin.";
        if ((mode == "icon" || mode == "myicon") && !TrainingCats.IconColumn()) return "İkon seçimi için database\\57_training_category_icon.sql dosyasını çalıştırın.";
        if (mode == "myicon")
        {
            HttpPostedFile f = q.Files["iconfile"];
            if (f != null && f.ContentLength > 0)
            {
                if (f.ContentLength > 5 * 1024 * 1024) return "İkon en fazla 5 MB olabilir.";
                byte[] h = new byte[8]; int n = f.InputStream.Read(h, 0, 8); f.InputStream.Position = 0;
                if (n < 8 || h[0] != 0x89 || h[1] != 0x50 || h[2] != 0x4E || h[3] != 0x47) return "İkon için saydam zeminli PNG yükleyin.";
                string ierr; string iname;
                try { iname = TrainingCats.PrepareIcon(f.InputStream, out ierr); }
                catch { return "İkon okunamadı; geçerli bir PNG yükleyin."; }
                if (ierr != null) return ierr;
                icon = "u:" + iname;
            }
            else
            {
                TStyle cur0 = TrainingCats.FindStyle(TrainingCats.Styles(), g, sub);
                if (cur0 == null || !TrainingCats.IsCustomIcon(cur0.Icon)) return "Yüklenecek ikonu seçin (saydam zeminli PNG).";
                icon = cur0.Icon;
            }
        }
        string card = null, cover = null, err;
        card = SaveImage(q.Files["card"], out err); if (err != null) return err;
        cover = SaveImage(q.Files["cover"], out err); if (err != null) { TrainingCats.DeleteStyleFile(card); return err; }
        if (mode == "upload" && card == null)
        {
            TStyle cur = TrainingCats.FindStyle(TrainingCats.Styles(), g, sub);
            if (cur == null || cur.Card == null) { TrainingCats.DeleteStyleFile(cover); return "Yüklenecek kart görselini seçin."; }
        }
        bool clearCard = q.Form["clearCard"] == "1" || mode == "icon" || mode == "myicon" || mode == "auto";
        bool setIcon = mode == "icon" || mode == "myicon" || mode == "auto" || mode == "upload";
        TrainingCats.SaveStyle(g, sub, card, cover, clearCard, q.Form["clearCover"] == "1", tint, true, mode == "icon" || mode == "myicon" ? icon : null, setIcon);
        return null;
    }

    /// <summary>Gorsel kaydi: uzanti degil icerik imzasi dogrulanir (PNG, JPEG, WebP). Dosya yoksa null.</summary>
    private static string SaveImage(HttpPostedFile f, out string err)
    {
        err = null;
        if (f == null || f.ContentLength == 0) return null;
        if (f.ContentLength > 5 * 1024 * 1024) { err = "Görsel en fazla 5 MB olabilir."; return null; }
        byte[] h = new byte[12]; int n = f.InputStream.Read(h, 0, 12); f.InputStream.Position = 0;
        string ext = null;
        if (n >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) ext = ".png";
        else if (n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) ext = ".jpg";
        else if (n >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) ext = ".webp";
        if (ext == null) { err = "Yalnız PNG, JPG ya da WebP görsel yüklenebilir."; return null; }
        string dir = HttpContext.Current.Server.MapPath(TrainingCats.StyleDir);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string name = Guid.NewGuid().ToString("N") + ext;
        f.SaveAs(Path.Combine(dir, name));
        return name;
    }

    private static void Out(HttpContext ctx, JavaScriptSerializer js, string err, List<object> cats)
    { ctx.Response.Write(js.Serialize(new { ok = err == null, error = err, cats = cats ?? new List<object>() })); }
}
