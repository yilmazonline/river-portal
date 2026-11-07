using System;
using System.Web.UI;

// Portal admin: kullanıcıyı siler (pasifleştirir).
public partial class DeleteUserPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Partner.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Users.aspx"); return; }

        int id;
        if (int.TryParse(Request.Form["id"], out id))
        {
            if (id == Auth.UserId) { Response.Redirect("Users.aspx?err=self"); return; }
            Repo.DeactivateUser(id);
        }
        Response.Redirect("Users.aspx?ok=del");
    }
}
