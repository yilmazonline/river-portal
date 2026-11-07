using System;
using System.IO;
using System.Web;
using System.Web.UI;

// Vendor: eğitim dokümanı yükler (her tür; program/betik dosyaları hariç) + opsiyonel kapak.
// Dosya App_Data altına kaydedilir (dogrudan sunulmaz, calistirilamaz); TrainingFile.ashx ile giris kontrollu sunulur.
public partial class AddTrainingDocumentPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Training.aspx"); return; }

        string titleTr = (Request.Unvalidated.Form["titletr"] ?? "").Trim();
        string titleEn = (Request.Unvalidated.Form["titleen"] ?? "").Trim();
        string group = (Request.Unvalidated.Form["categorygroup"] ?? "").Trim();
        string sub = (Request.Unvalidated.Form["subcategory"] ?? "").Trim();
        string desc = (Request.Unvalidated.Form["description"] ?? "").Trim();

        if (titleTr.Length == 0) { Response.Redirect("Training.aspx?err=1"); return; }
        if (titleEn.Length == 0) titleEn = titleTr;
        if (group.Length == 0) group = "Genel";
        if (sub.Length == 0) sub = "Genel";

        HttpPostedFile file = Request.Files["file"];
        if (file == null || file.ContentLength <= 0) { Response.Redirect("Training.aspx?err=nofile"); return; }

        string ext = (Path.GetExtension(file.FileName) ?? "").ToLowerInvariant();
        if (ext.Length == 0 || ext.Length > 12 || TrainingFiles.IsBlocked(ext)) { Response.Redirect("Training.aspx?err=type"); return; }

        string dir = TrainingFiles.NewDir();
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string stored = Guid.NewGuid().ToString("N") + ext;
        file.SaveAs(Path.Combine(dir, stored));

        // Opsiyonel kapak
        string cover = null;
        HttpPostedFile cf = Request.Files["cover"];
        if (cf != null && cf.ContentLength > 0)
        {
            string cext = (Path.GetExtension(cf.FileName) ?? "").ToLowerInvariant();
            if (cext == ".png" || cext == ".jpg" || cext == ".jpeg" || cext == ".webp")
            {
                string cdir = Server.MapPath("~/assets/traincovers");
                if (!Directory.Exists(cdir)) Directory.CreateDirectory(cdir);
                cover = Guid.NewGuid().ToString("N") + cext;
                cf.SaveAs(Path.Combine(cdir, cover));
            }
        }

        Repo.AddTrainingDocument(titleTr, titleEn, group, sub, desc, stored, Path.GetFileName(file.FileName), file.ContentType, file.ContentLength, cover);
        TrainingCats.Ensure("Document", group, sub);
        if (Request.Form["notify"] == "1")
        {
            string cat = (group.Length > 0 ? group : "Genel") + (sub.Length > 0 ? " / " + sub : "");
            Notifier.TrainingContentAdded(titleTr, cat);
        }
        Response.Redirect("Training.aspx?ok=add");
    }
}
