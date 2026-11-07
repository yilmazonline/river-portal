using System;
using System.Web.UI;

// Self-servis şifre değiştirme (tüm kullanıcılar). AJAX: ok/badold/mismatch/short/no
public partial class ChangePasswordPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/plain; charset=utf-8";
        if (!Auth.IsLoggedIn) { Response.Write("no"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Response.Write("no"); return; }

        string oldPw = Request.Form["old"] ?? "";
        string nw = Request.Form["new"] ?? "";
        string nw2 = Request.Form["new2"] ?? "";

        if (nw != nw2) { Response.Write("mismatch"); return; }
        if (nw.Length < 6) { Response.Write("short"); return; }

        string hash = Repo.GetPasswordHash(Auth.UserId);
        if (string.IsNullOrEmpty(hash) || !PasswordHasher.Verify(oldPw, hash)) { Response.Write("badold"); return; }

        Repo.UpdateUserPassword(Auth.UserId, PasswordHasher.Hash(nw));
        Auth.IssuePersistent(Repo.GetAccountById(Auth.UserId));   // eski cerezler gecersiz; bu tarayiciya yenisi
        Response.Write("ok");
    }
}
