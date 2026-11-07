using System;
using System.Web.UI;

// Admin: bir kullanıcının şifresini yeniler; opsiyonel olarak kullanıcıya mail gönderir.
public partial class ResetPasswordPage : Page
{
    public string Culture = "tr";
    public int UserId = 0;
    public string TargetName = "";
    public string TargetEmail = "";
    public string TargetCompany = "";
    public string GenPassword = "";
    public string FlashErr = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.RequireVendor(this);
        if (Response.IsRequestBeingRedirected) return;
        Culture = Auth.Culture;

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            Save();
            return;
        }

        int id;
        if (!int.TryParse(Request.QueryString["id"], out id)) { Response.Redirect("Users.aspx"); return; }
        UserRow u = Repo.GetUserById(id);
        if (u == null) { Response.Redirect("Users.aspx"); return; }

        UserId = u.UserId;
        TargetName = u.FullName;
        TargetEmail = u.Email;
        TargetCompany = u.CompanyName;
        GenPassword = Fmt.NewPassword();
    }

    private void Save()
    {
        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Users.aspx"); return; }
        UserRow u = Repo.GetUserById(id);
        if (u == null) { Response.Redirect("Users.aspx"); return; }

        string pwd = (Request.Form["password"] ?? "").Trim();
        bool sendMail = Request.Form["sendmail"] == "on" || Request.Form["sendmail"] == "1";

        if (pwd.Length < 4)
        {
            FlashErr = "Parola en az 4 karakter olmalı.";
            UserId = u.UserId; TargetName = u.FullName; TargetEmail = u.Email; TargetCompany = u.CompanyName;
            GenPassword = pwd.Length > 0 ? pwd : Fmt.NewPassword();
            return;
        }

        Repo.UpdateUserPassword(id, PasswordHasher.Hash(pwd));

        if (sendMail)
        {
            try
            {
                if (Mailer.HasSmtp && !string.IsNullOrEmpty(u.Email))
                {
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    string html = Emails.PasswordReset(u.FullName, u.Email, pwd, baseUrl + "/Login.aspx");
                    string err;
                    Mailer.SendTo(u.Email, "Şifreniz yenilendi · River Portal", html, out err);
                }
            }
            catch { }
        }

        Response.Redirect("Users.aspx?ok=reset");
    }
}
