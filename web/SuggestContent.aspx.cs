using System;
using System.Web.Configuration;
using System.Web.UI;

// Kullanıcı içerik önerisi: kaydeder + destek@ adresine mail atar.
public partial class SuggestContentPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Training.aspx"); return; }

        string title = (Request.Unvalidated.Form["title"] ?? "").Trim();
        string body = (Request.Unvalidated.Form["body"] ?? "").Trim();
        if (body.Length == 0) { Response.Redirect("Training.aspx"); return; }

        try { Repo.AddContentSuggestion(Auth.UserId, Auth.CompanyId, title, body); }
        catch { }

        try
        {
            string addr = (WebConfigurationManager.AppSettings["Support.NotifyEmail"] ?? "").Trim();
            if (Mailer.HasSmtp && addr.Length > 0)
            {
                string intro = "Bir kullanıcı yeni eğitim içeriği önerdi.";
                string html = Emails.ContentSuggestion(Auth.FullName, Auth.CompanyName, title, body);
                string err;
                Mailer.SendTo(addr, "Eğitim içerik önerisi · " + (title.Length > 0 ? title : "Öneri"), html, out err);
            }
        }
        catch { }

        Response.Redirect("Training.aspx?ok=suggest");
    }
}
