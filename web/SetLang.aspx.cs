using System;
using System.Web;
using System.Web.UI;

// Dil (kültür) değiştir: oturum + çerez; güvenli şekilde geri yönlendir.
public partial class SetLangPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        string lang = (Request.QueryString["lang"] == "en") ? "en" : "tr";
        Auth.Culture = lang;
        try
        {
            HttpCookie c = new HttpCookie("ui_culture", lang);
            c.Expires = DateTime.Now.AddYears(1); c.Path = "/";
            Response.Cookies.Add(c);
        }
        catch { }

        // Guvenli geri yonlendirme: yalniz ayni site, goreli yol
        string r = Request.QueryString["r"] ?? "";
        if (r.Length == 0 || r.IndexOf("//") >= 0 || r.IndexOf(":") >= 0 || !r.StartsWith("/"))
            r = "Default.aspx";
        Response.Redirect(r);
    }
}
