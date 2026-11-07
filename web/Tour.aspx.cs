using System;
using System.Web.UI;

// Uygulama turu durumu: state (JSON: show + role) / seen (POST: görüldü işaretle)
public partial class TourPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json; charset=utf-8";
        if (!Auth.IsLoggedIn) { Response.Write("{\"show\":false,\"role\":\"\"}"); return; }

        string action = (Request.QueryString["action"] ?? "state").Trim();

        if (action == "seen" && string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            try { Repo.MarkTourSeen(Auth.UserId); } catch { }
            Response.Write("{\"ok\":true}");
            return;
        }

        if (action == "introseen" && string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            try { Repo.MarkIntroSeen(Auth.UserId); } catch { }
            Response.Write("{\"ok\":true}");
            return;
        }

        // Portal turu otomatik acilmaz (rol videolari karsilamayi ustlendi)
        bool show = false;
        string role = RoleKey();
        bool hasIntro = IntroVideo.HasVideo(role);
        bool intro = false;
        try { intro = hasIntro && !Repo.IsIntroSeen(Auth.UserId); } catch { intro = false; }
        string av = "";
        try { av = Repo.GetUserAvatar(Auth.UserId) ?? ""; } catch { av = ""; }
        av = av.Replace("\\", "").Replace("\"", "");
        Response.Write("{\"show\":" + (show ? "true" : "false") + ",\"intro\":" + (intro ? "true" : "false") + ",\"hasIntro\":" + (hasIntro ? "true" : "false") + ",\"role\":\"" + RoleKey() + "\",\"avatar\":\"" + av + "\"}");
    }

    private string RoleKey() { return IntroVideo.RoleKey(); }
}
