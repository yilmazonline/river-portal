using System;
using System.Web.Script.Serialization;
using System.Web.UI;

// Forum: giris yapmis tum kullanicilara acik. Ilk giriste topluluk kurallari onayi istenir.
public partial class ForumPage : Page
{
    public string Culture = "tr", UserName = "", CompanyName = "", FlashMsg = "", MeJson = "{}";
    public bool IsMod, NeedsConsent;
    public int FocusPost;
    protected void Page_Load(object sender, EventArgs e)
    {
        // Sayfa kisiye ozel (yetki, ad): Cloudflare ve tarayici onbellege almasin
        Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.AppendHeader("CDN-Cache-Control", "no-store");
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!ForumRepo.CanAccess) { Response.Redirect(Auth.HomeUrl); return; }
        Culture = Auth.Culture; UserName = Auth.FullName; CompanyName = Auth.CompanyName;
        IsMod = ForumRepo.IsModerator;
        ForumRepo.MarkSeen(Auth.UserId);
        NeedsConsent = !ForumRepo.HasAccepted(Auth.UserId);   // ilk giris: kurallar onayi   // menudeki yeni gonderi sayisi sifirlanir
        int.TryParse(Request.QueryString["post"], out FocusPost);
        string ava = ""; try { UserRow u = Repo.GetUserById(Auth.UserId); if (u != null) ava = u.AvatarImage ?? ""; } catch { }
        MeJson = new JavaScriptSerializer().Serialize(new { name = UserName, ini = Fmt.Initials(UserName), ava = ava });
    }
}
