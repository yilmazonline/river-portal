using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;

// Versiyon Duyurusu işlemleri: save (taslak) / publish (yayınla + bildirim + mail) / delete
public partial class VersionActionPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Versions.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Versions.aspx"); return; }

        string action = (Request.Form["action"] ?? "").Trim();

        if (action == "save")
        {
            string product = (Request.Unvalidated.Form["product"] ?? "").Trim();
            string versionNo = (Request.Unvalidated.Form["versionno"] ?? "").Trim();
            string body = (Request.Unvalidated.Form["body"] ?? "").Trim();
            string highlights = (Request.Unvalidated.Form["highlights"] ?? "").Trim();
            if (product.Length == 0 || versionNo.Length == 0) { Response.Redirect("Versions.aspx"); return; }
            DateTime? rel = ParseDate(Request.Form["release"]);
            DateTime? acc = ParseDate(Request.Form["access"]);
            Repo.AddVersion(product, versionNo, rel, acc, highlights, body, Auth.UserId);
            Response.Redirect("Versions.aspx?ok=saved");
            return;
        }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Versions.aspx"); return; }

        if (action == "delete") { Repo.DeleteVersion(id); Response.Redirect("Versions.aspx?ok=del"); return; }

        if (action == "publish")
        {
            VersionAnnouncement v = Repo.GetVersion(id);
            if (v == null) { Response.Redirect("Versions.aspx"); return; }
            Repo.PublishVersion(id);

            bool chSel = Request.Form["chsel"] == "1";
            bool wantApp = chSel ? (Request.Form["ch_app"] != null) : true;
            bool wantMail = chSel ? (Request.Form["ch_mail"] != null) : true;

            string titleTr = "\"" + v.VersionNo + "\" numaralı " + v.ProductName + " Yeni Versiyonu Yayında";
            string titleEn = v.ProductName + " " + v.VersionNo + " — New Version Available";
            // Uygulama ici: kullanici zaten portalda -> sekmeye yonlendir
            string inAppTr = "Yeni sürümün tüm detayları ve yenilik listesi \"Versiyon Duyuruları\" sekmesinden incelenebilir.";
            string inAppEn = "All details and the changelog can be viewed in the \"Version Announcements\" tab.";
            // E-posta: dis ortam -> portala giris
            string mailTr = "Yeni sürümün tüm detayları ve yenilik listesi River Portal'da. Görüntülemek için portala giriş yapın.";

            // 1) Uygulama ici bildirim (secildiyse)
            if (wantApp)
                try { Repo.NotifyAllCompanies(titleTr, titleEn, inAppTr, inAppEn, "Version", "Versions.aspx"); }
                catch { }

            // 2) E-posta (secildiyse) - best effort
            if (wantMail)
            try
            {
                string url = SiteBase();
                string[] kv = new string[] {
                    "Ürün", v.ProductName,
                    "Versiyon", "v" + v.VersionNo,
                    "Yayın Tarihi", v.ReleaseDate.HasValue ? v.ReleaseDate.Value.ToString("dd.MM.yyyy", new CultureInfo("tr-TR")) : "-",
                    "Erişime Açılma", v.AccessDate.HasValue ? v.AccessDate.Value.ToString("dd.MM.yyyy", new CultureInfo("tr-TR")) : "-"
                };
                string html = Emails.Notice(titleTr, mailTr, kv, url, "River Portal'a Giriş Yap");
                string subject = titleTr;
                List<string> emails = Repo.GetActiveEmailsByPref("NotifyVersionEmail");
                for (int i = 0; i < emails.Count; i++)
                {
                    string err;
                    try { Mailer.SendTo(emails[i], subject, html, out err); }
                    catch { }
                }
            }
            catch { }

            Response.Redirect("Versions.aspx?ok=pub");
            return;
        }

        Response.Redirect("Versions.aspx");
    }

    private static DateTime? ParseDate(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        DateTime d;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
        return null;
    }

    private static string SiteBase()
    {
        string b = (System.Web.Configuration.WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (b.Length == 0) b = "https://partner.folyazilim.com";
        return b.TrimEnd('/');
    }
}
