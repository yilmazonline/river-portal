using System;
using System.Web.UI;

// Vendor: River Akademi'ye YouTube videosu ekler (global, tüm bayilere açık).
public partial class AddTrainingVideoPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Training.aspx"); return; }

        var f = Request.Unvalidated.Form;
        string titleTr = (f["titletr"] ?? "").Trim();
        string titleEn = (f["titleen"] ?? "").Trim();
        string categoryGroup = (f["categorygroup"] ?? "").Trim();
        string subCategory = (f["subcategory"] ?? "").Trim();
        string url = f["url"];

        string yid = Fmt.YouTubeId(url);

        if (titleTr.Length == 0 || yid.Length == 0)
        { Response.Redirect("Training.aspx?err=1"); return; }
        if (titleEn.Length == 0) titleEn = titleTr;
        if (categoryGroup.Length == 0) categoryGroup = "Genel";
        if (subCategory.Length == 0) subCategory = "Genel";

        Repo.AddTrainingVideo(titleTr, titleEn, categoryGroup, subCategory, yid);
        TrainingCats.Ensure("Video", categoryGroup, subCategory);
        if (Request.Form["notify"] == "1")
        {
            string cat = (categoryGroup.Length > 0 ? categoryGroup : "Genel") + (subCategory.Length > 0 ? " / " + subCategory : "");
            Notifier.TrainingContentAdded(titleTr, cat);
        }
        Response.Redirect("Training.aspx?ok=add");
    }
}
