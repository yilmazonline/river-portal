using System;
using System.IO;
using System.Web.UI;

// Vendor: eğitim dokümanını kaldırır (kayıt + dosya + kapak).
public partial class DeleteTrainingDocPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Training.aspx"); return; }

        int id;
        if (int.TryParse(Request.Form["id"], out id))
        {
            string[] files = Repo.DeleteTrainingDoc(id);
            try
            {
                if (!string.IsNullOrEmpty(files[0])) TrainingFiles.DeleteFile(files[0]);   // App_Data ya da eski ~/trainingdocs
                if (!string.IsNullOrEmpty(files[1]))
                {
                    string cp = Path.Combine(Server.MapPath("~/assets/traincovers"), files[1]);
                    if (File.Exists(cp)) File.Delete(cp);
                }
            }
            catch { }
        }
        Response.Redirect("Training.aspx?ok=del");
    }
}
