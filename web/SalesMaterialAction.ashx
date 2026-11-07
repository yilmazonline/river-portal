<%@ WebHandler Language="C#" Class="SalesMaterialActionHandler" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Satis Materyalleri islemleri.
// Herkes (giris yapmis, materyal sayfasini gorebilen): view (goruntuleme kaydi), send (belgeyi kendi e-postasina; BCC: Materials.SendBcc)
// Vendor: list, save, move, delete, catadd, catrename, catmove, catdel, intro. Degistirici istekler yalniz POST + ayni site.
public class SalesMaterialActionHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    private static readonly JavaScriptSerializer JS = new JavaScriptSerializer();

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json"; ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        HttpRequest q = ctx.Request;
        if (!Auth.IsLoggedIn || Auth.IsPartnerUserLimited) { Out(ctx, "Bu işlem için yetkiniz yok.", null); return; }
        if (!SalesMats.Ready()) { Out(ctx, "Satış materyalleri tablosu güncel değil: database\\59_sales_materials_plus.sql dosyasını çalıştırın.", null); return; }
        string op = q["op"] ?? "";
        if (op != "list")
        {
            Uri refr = q.UrlReferrer;
            if (!string.Equals(q.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, q.Url.Host, StringComparison.OrdinalIgnoreCase))
            { Out(ctx, "İstek kaynağı doğrulanamadı.", null); return; }
        }
        int id; int.TryParse(q.Form["id"] ?? q["id"], out id);
        try
        {
            if (op == "view") { if (id > 0) SalesMats.Log(id, Auth.UserId, Auth.CompanyId, "view"); Out(ctx, null, null); return; }
            if (op == "send") { string msg; string err = Send(ctx, id, out msg); Out(ctx, err, err == null ? (object)new { message = msg } : null); return; }

            if (!Auth.IsVendor) { Out(ctx, "Bu işlem yalnız yöneticilere açık.", null); return; }
            string err2 = null;
            if (op == "save")
            {
                int tint; if (!int.TryParse(q.Form["tint"], out tint)) tint = -1;
                err2 = SalesMats.Save(id, q.Unvalidated.Form["title"], q.Unvalidated.Form["titleEn"], q.Unvalidated.Form["group"], q.Unvalidated.Form["summary"], tint, q.Form["icon"]);
            }
            else if (op == "move") SalesMats.MoveItem(id, q.Form["dir"] == "up" ? -1 : 1);
            else if (op == "delete")
            {
                // Repo.DeleteSalesMaterial silinen dosyanin adini dondurur (hata degil): diskteki dosya da silinir
                string stored = Repo.DeleteSalesMaterial(id);
                if (!string.IsNullOrEmpty(stored))
                {
                    string p = Path.Combine(ctx.Server.MapPath("~/materials"), Path.GetFileName(stored));
                    try { if (File.Exists(p)) File.Delete(p); } catch { }
                }
            }
            else if (op == "catadd") err2 = SalesMats.AddCat(q.Unvalidated.Form["name"]);
            else if (op == "catrename") err2 = SalesMats.RenameCat(id, q.Unvalidated.Form["name"]);
            else if (op == "catmove") SalesMats.MoveCat(id, q.Form["dir"] == "up" ? -1 : 1);
            else if (op == "catdel") err2 = SalesMats.DeleteCat(id);
            else if (op == "intro")
            {
                string v = (q.Unvalidated.Form["text"] ?? "").Trim().Replace("\r", " ").Replace("\n", " ");
                if (v.Length > 300) err2 = "Sayfa metni en fazla 300 karakter olabilir."; else SalesMats.SetIntro(v);
            }
            else if (op != "list") err2 = "Geçersiz işlem.";
            Out(ctx, err2, State());
        }
        catch (Exception ex) { Out(ctx, "İşlem tamamlanamadı: " + ex.Message, null); }
    }

    private static object State()
    {
        List<object> items = new List<object>(), cats = new List<object>();
        foreach (SMat m in SalesMats.All())
            items.Add(new { id = m.Id, title = m.TitleTr, titleEn = m.TitleEn, group = m.Group, summary = m.Summary, tint = m.Tint, icon = m.Icon, autoIcon = SalesMats.AutoIcon(m),
                            ext = m.Ext(), href = m.Href(), external = m.External, views = m.Views, sends = m.Sends });
        foreach (SMatCat c in SalesMats.Categories()) cats.Add(new { id = c.Id, name = c.Name });
        return new { items = items, cats = cats, intro = SalesMats.Intro() };
    }

    /// <summary>Belgeyi giris yapan kisinin e-postasina gonderir; BCC: Materials.SendBcc (varsayilan yilmaz.guney@river.com.tr)</summary>
    private static string Send(HttpContext ctx, int id, out string message)
    {
        message = null;
        SMat m = id > 0 ? SalesMats.Get(id) : null;
        if (m == null) return "Materyal bulunamadı.";
        UserRow u = Repo.GetUserById(Auth.UserId);
        if (u == null || string.IsNullOrEmpty(u.Email)) return "Hesabınızda e-posta adresi kayıtlı değil.";
        if (SalesMats.SendsLastHour(Auth.UserId) >= 10) return "Bir saat içinde en fazla 10 doküman gönderebilirsiniz. Biraz sonra tekrar deneyin.";
        string bcc = (WebConfigurationManager.AppSettings["Materials.SendBcc"] ?? "yilmaz.guney@river.com.tr").Trim();
        string baseUrl = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim().TrimEnd('/');
        string pageUrl = (baseUrl.Length > 0 ? baseUrl : ctx.Request.Url.GetLeftPart(UriPartial.Authority)) + "/Materials.aspx";
        string title = m.Title("tr");
        List<string> att = null; bool asLink = m.External;
        if (!m.External)
        {
            string p = ctx.Server.MapPath("~/materials/" + Path.GetFileName(m.Stored));
            if (!File.Exists(p)) return "Doküman dosyası sunucuda bulunamadı.";
            if (new FileInfo(p).Length > 20L * 1024 * 1024) asLink = true;   // buyuk ekler e-posta sinirlarina takilmasin: baglanti gonderilir
            else att = new List<string> { p + "|" + Path.GetFileName(m.Original ?? m.Stored) };
        }
        string link = m.External ? m.Stored : pageUrl;
        string inner =
            "<p style=\"margin:0 0 14px;font-size:15px;color:#1B2440\">Merhaba " + HttpUtility.HtmlEncode(u.FullName ?? "") + ",</p>" +
            "<p style=\"margin:0 0 14px;font-size:15px;color:#1B2440\">İstediğiniz satış materyali: <b>" + HttpUtility.HtmlEncode(title) + "</b></p>" +
            (string.IsNullOrEmpty(m.Summary) ? "" : "<p style=\"margin:0 0 14px;font-size:14px;line-height:1.6;color:#4A5672\">" + HttpUtility.HtmlEncode(m.Summary) + "</p>") +
            (asLink ? "<p style=\"margin:0 0 14px;font-size:14px;color:#1B2440\">Dokümana bu bağlantıdan ulaşabilirsiniz: <a href=\"" + HttpUtility.HtmlAttributeEncode(link) + "\" style=\"color:#4F46E5\">" + HttpUtility.HtmlEncode(m.External ? link : "Satış Materyalleri") + "</a></p>"
                    : "<p style=\"margin:0 0 14px;font-size:14px;color:#1B2440\">Doküman bu e-postanın ekindedir.</p>") +
            "<p style=\"margin:18px 0 0;font-size:12.5px;color:#8A97AE\">Bu e-posta, River AI Asistan Portal'daki Satış Materyalleri sayfasından sizin isteğinizle gönderildi.</p>";
        string html = Emails.Shell("Satış Materyali", inner);
        string err;
        bool ok = Mailer.SendToBccWithFiles(u.Email, bcc, "River AI · " + title, html, att, out err);
        if (!ok) return "E-posta gönderilemedi: " + err;
        SalesMats.Log(m.Id, Auth.UserId, Auth.CompanyId, "send");
        message = "Doküman " + u.Email + " adresine gönderildi.";
        return null;
    }

    private static void Out(HttpContext ctx, string err, object data)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["ok"] = err == null; d["error"] = err;
        if (data != null) d["data"] = data;
        ctx.Response.Write(JS.Serialize(d));
    }
}
