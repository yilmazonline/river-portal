using System;
using System.Web.UI;

// Vendor: River Akademi videosunu kaldırır.
public partial class DeleteTrainingVideoPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Training.aspx"); return; }

        int id;
        if (int.TryParse(Request.Form["id"], out id)) Repo.DeleteTrainingMaterial(id);
        Response.Redirect("Training.aspx?ok=del");
    }
}
