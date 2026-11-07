using System;
using System.Web;
using System.Web.UI;

public partial class LogoutPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Impersonation cookie'sini de temizle
        HttpCookie imp = new HttpCookie("imp_company", "");
        imp.Expires = DateTime.UtcNow.AddDays(-1);
        imp.Path = "/";
        Response.Cookies.Add(imp);

        Auth.SignOut();
        Response.Redirect("Login.aspx");
    }
}
