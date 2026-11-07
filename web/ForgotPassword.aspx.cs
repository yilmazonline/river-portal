using System;
using System.Web.Configuration;
using System.Web.UI;

// Şifremi unuttum (self-servis): kullanıcıya e-posta ile jetonlu sıfırlama bağlantısı gönderilir.
// Güvenlik: e-postanın kayıtlı olup olmadığını sızdırmayız (her durumda genel mesaj).
public partial class ForgotPasswordPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Login.aspx"); return; }

        string email = (Request.Form["email"] ?? "").Trim();
        if (email.Length == 0) { Response.Redirect("Login.aspx?forgot=1"); return; }

        try
        {
            UserRow u = Repo.FindUserByEmail(email);
            if (u != null)
            {
                string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                Repo.SetResetToken(u.UserId, token, DateTime.UtcNow.AddHours(2));

                if (Mailer.HasSmtp)
                {
                    string baseUrl = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
                    if (baseUrl.Length == 0) baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    string resetUrl = baseUrl.TrimEnd('/') + "/ResetPasswordLink.aspx?token=" + token;
                    bool en = false; // UserRow kültür taşımıyor; sıfırlama maili TR
                    string html = Emails.Notice(
                        en ? "Password Reset" : "Şifre Sıfırlama",
                        en ? "Click the button below to reset your password. The link is valid for 2 hours. If you did not request this, you can ignore this email."
                           : "Şifrenizi sıfırlamak için aşağıdaki butona tıklayın. Bağlantı 2 saat geçerlidir. Bu talebi siz yapmadıysanız e-postayı yok sayabilirsiniz.",
                        new string[] { en ? "User" : "Kullanıcı", u.FullName, en ? "Email" : "E-posta", u.Email },
                        resetUrl, en ? "Reset Password" : "Şifremi Sıfırla");
                    string err;
                    Mailer.SendTo(u.Email, en ? "River Portal · Password Reset" : "River Portalı · Şifre Sıfırlama", html, out err);
                }
            }
        }
        catch { }

        Response.Redirect("Login.aspx?forgot=1");
    }
}
