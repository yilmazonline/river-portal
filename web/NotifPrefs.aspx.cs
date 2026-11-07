using System;
using System.Web.UI;

// Kullanıcı e-posta bildirim tercihleri (JSON). GET: {version,training} · POST action=save: kaydet
public partial class NotifPrefsPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json; charset=utf-8";
        if (!Auth.IsLoggedIn) { Response.Write("{\"version\":true,\"training\":true}"); return; }

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            bool v = Request.Form["version"] == "1";
            bool t = Request.Form["training"] == "1";
            try { Repo.UpdateNotifyPrefs(Auth.UserId, v, t); } catch { }
            Response.Write("{\"ok\":true}");
            return;
        }

        bool[] p = Repo.GetNotifyPrefs(Auth.UserId);
        Response.Write("{\"version\":" + (p[0] ? "true" : "false") + ",\"training\":" + (p[1] ? "true" : "false") + "}");
    }
}
