using System;
using System.Web.UI;

// Bildirimleri okundu isaretler. partner.js buraya POST atar.
public partial class MarkReadPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.StatusCode = 401; Response.End(); return; }
        Repo.MarkAllRead(Auth.CompanyId);
        Response.ContentType = "text/plain";
        Response.Write("ok");
        Response.End();
    }
}
