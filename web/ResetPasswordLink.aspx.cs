using System;
using System.Web.UI;

// Self-servis şifre sıfırlama: e-postadaki jetonlu bağlantı. GET: form; POST: yeni parolayı kaydet.
public partial class ResetPasswordLinkPage : Page
{
    public bool Valid = false;
    public bool Done = false;
    public string Token = "";
    public string UserName = "";
    public string Msg = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Token = (Request.Form["token"] ?? Request.QueryString["token"] ?? "").Trim();
        UserRow u = Repo.FindUserByResetToken(Token);
        if (u == null)
        {
            Valid = false;
            Msg = "Bağlantı geçersiz veya süresi dolmuş. Lütfen giriş ekranından yeniden şifre sıfırlama talebi oluşturun.";
            return;
        }
        Valid = true;
        UserName = u.FullName;

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            string p1 = Request.Form["pw"] ?? "";
            string p2 = Request.Form["pw2"] ?? "";
            if (p1.Length < 6) { Msg = "Parola en az 6 karakter olmalıdır."; return; }
            if (p1 != p2) { Msg = "Parolalar eşleşmiyor."; return; }
            try
            {
                Repo.UpdateUserPassword(u.UserId, PasswordHasher.Hash(p1));
                Repo.ClearResetToken(u.UserId);
                Done = true;
            }
            catch { Msg = "Bir hata oluştu, lütfen tekrar deneyin."; }
        }
    }
}
