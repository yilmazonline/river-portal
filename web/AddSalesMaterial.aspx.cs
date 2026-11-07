using System;
using System.IO;
using System.Web;
using System.Web.UI;

// Vendor: satış materyali yayınlar (dosya yükleme veya harici URL).
public partial class AddSalesMaterialPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Materials.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Materials.aspx"); return; }

        string titleTr = (Request.Unvalidated.Form["titletr"] ?? "").Trim();
        string titleEn = (Request.Unvalidated.Form["titleen"] ?? "").Trim();
        string group = (Request.Unvalidated.Form["categorygroup"] ?? "").Trim();
        string url = (Request.Unvalidated.Form["url"] ?? "").Trim();

        if (titleTr.Length == 0) { Response.Redirect("Materials.aspx?err=1"); return; }
        if (titleEn.Length == 0) titleEn = titleTr;
        if (group.Length == 0) group = "Genel";

        HttpPostedFile file = null;
        if (Request.Files.Count > 0) file = Request.Files["file"];

        if (file != null && file.ContentLength > 0)
        {
            string ext = Path.GetExtension(file.FileName);
            ext = (ext ?? "").ToLowerInvariant();
            string[] allow = new string[] { ".pdf", ".ppt", ".pptx", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".png", ".jpg", ".jpeg" };
            bool okExt = false;
            for (int i = 0; i < allow.Length; i++) { if (allow[i] == ext) { okExt = true; break; } }
            if (!okExt) { Response.Redirect("Materials.aspx?err=type"); return; }

            string dir = Server.MapPath("~/materials");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string stored = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(dir, stored));

            string original = Path.GetFileName(file.FileName);
            Repo.AddSalesMaterial(titleTr, titleEn, group, stored, original, file.ContentType, false, file.ContentLength);
            Response.Redirect("Materials.aspx?ok=add");
            return;
        }

        if (url.Length > 0 && (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            Repo.AddSalesMaterial(titleTr, titleEn, group, url, titleTr, "url", true, 0);
            Response.Redirect("Materials.aspx?ok=add");
            return;
        }

        Response.Redirect("Materials.aspx?err=1");
    }
}
