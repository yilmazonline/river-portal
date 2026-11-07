using System;
using System.Web;
using System.Web.UI;

public partial class LoginPage : Page
{
    public bool ShowError = false;
    public bool ShowForgotOk = false;

    // Acilis sayfasi: Auth.HomeUrl (tek kural; Default.aspx ile ayni)
    private static string HomeFor(string role) { return Auth.HomeUrl; }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Auth.IsLoggedIn)
        {
            Response.Redirect(HomeFor(Auth.Role));
            return;
        }

        if (Request.QueryString["forgot"] == "1") ShowForgotOk = true;
        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            string email = (Request.Form["email"] ?? "").Trim();
            string pw = (Request.Form["password"] ?? "").Trim();
            string culture = Request.Form["culture"];

            UserAccount u = Repo.GetUserByEmail(email);
            if (u == null || !u.IsActive || !PasswordHasher.Verify(pw, u.PasswordHash))
            {
                ShowError = true;
                return;
            }

            string chosen = Normalize(culture);
            if (chosen == null) chosen = Normalize(u.PreferredCulture);
            if (chosen == null) chosen = "tr";

            CompanyInfo company = Repo.GetCompany(u.CompanyId);
            Repo.UpdateLastLogin(u.UserId);
            Auth.Set(u, company != null ? company.Name : "", chosen);
            Auth.IssuePersistent(u);   // sunucu oturumu dusse de kullanici cikisa dusmez
            SetCultureCookie(chosen);

            Response.Redirect(HomeFor(u.Role));
        }
    }

    private static string Normalize(string c)
    {
        if (string.IsNullOrEmpty(c)) return null;
        if (c.StartsWith("en")) return "en";
        if (c.StartsWith("tr")) return "tr";
        return null;
    }

    private void SetCultureCookie(string culture)
    {
        HttpCookie ck = new HttpCookie("ui_culture", culture);
        ck.Expires = DateTime.UtcNow.AddYears(1);
        ck.Path = "/";
        Response.Cookies.Add(ck);
    }
}
