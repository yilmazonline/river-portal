using System;
using System.IO;
using System.Web.UI;

// Eğitim içerik işlemleri: toggle (yayın) / feature (öne çıkar) / delete
public partial class TrainingItemActionPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("TrainingManage.aspx"); return; }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("TrainingManage.aspx"); return; }
        TrainingItem t = Repo.GetTrainingItem(id);
        if (t == null) { Response.Redirect("TrainingManage.aspx"); return; }
        string action = (Request.Form["action"] ?? "").Trim();

        if (action == "toggle") { Repo.SetTrainingActive(id, !t.IsActive); Response.Redirect("TrainingManage.aspx?ok=toggle"); return; }
        if (action == "feature") { Repo.SetTrainingFeatured(id, !t.IsFeatured, t.FeatureText); Response.Redirect("TrainingManage.aspx?ok=feature"); return; }
        if (action == "delete")
        {
            if (t.MaterialType == "Document")
            {
                string[] files = Repo.DeleteTrainingDoc(id);
                try
                {
                    if (!string.IsNullOrEmpty(files[0])) TrainingFiles.DeleteFile(files[0]);   // App_Data ya da eski ~/trainingdocs
                    if (!string.IsNullOrEmpty(files[1])) { string cp = Path.Combine(Server.MapPath("~/assets/traincovers"), files[1]); if (File.Exists(cp)) File.Delete(cp); }
                }
                catch { }
            }
            else Repo.DeleteTrainingMaterial(id);
            Response.Redirect("TrainingManage.aspx?ok=del"); return;
        }
        Response.Redirect("TrainingManage.aspx");
    }
}
