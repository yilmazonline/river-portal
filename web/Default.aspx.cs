using System;
using System.Web.UI;

public partial class DefaultPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Yonlendirme kisiye ozel: Cloudflare ve tarayici onbellege almasin (eski surumun yaniti tekrar kullanilmasin)
        Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1));
        Response.AppendHeader("CDN-Cache-Control", "no-store");
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        Response.Redirect(Auth.HomeUrl);   // tek kural: App_Code/Auth.cs
    }
}
