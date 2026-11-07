using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

// Profil resmi (avatar): dosya yukleme VEYA hazir avatar (preset) secimi.
// AJAX: yeni yolu (assets/avatars/...) plain text doner; hata -> "err:..."
public partial class UploadAvatarPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/plain; charset=utf-8";
        if (!Auth.IsLoggedIn) { Response.Write("no"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Response.Write("no"); return; }

        // Hazir avatar (preset) secimi
        string preset = (Request.Form["preset"] ?? "").Trim();
        if (preset.Length > 0)
        {
            // Yalniz izin listesindeki hazir avatarlar (marka: 4 FOL rengi + River R/AI, is hayati karakterleri, klasik av1-av12)
            if (Regex.IsMatch(preset, @"^assets/avatars/presets/(av([1-9]|1[0-2])|fol-(gold|platinum|onyx|amethyst)|river-(r|ai)|is-(kahve|deadline|akis|form|onay|bug|robot|toplanti|excel|roket|bende|destek))\.png$"))
            {
                DeleteOldUploaded();
                Repo.SetUserAvatar(Auth.UserId, preset);
                Response.Write(preset);
            }
            else Response.Write("err:preset");
            return;
        }

        // Dosya yukleme
        HttpPostedFile file = Request.Files["avatar"];
        if (file == null || file.ContentLength <= 0) { Response.Write("err:nofile"); return; }
        if (file.ContentLength > 4 * 1024 * 1024) { Response.Write("err:size"); return; }

        string ext = (Path.GetExtension(file.FileName) ?? "").ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".webp" && ext != ".gif")
        { Response.Write("err:type"); return; }

        string dir = Server.MapPath("~/assets/avatars");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        DeleteOldUploaded();

        string stored = "u" + Auth.UserId + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext;
        file.SaveAs(Path.Combine(dir, stored));
        string rel = "assets/avatars/" + stored;
        Repo.SetUserAvatar(Auth.UserId, rel);
        Response.Write(rel);
    }

    // Kullanicinin ONCEKI yuklenmis avatarini diskten siler; preset ise dokunmaz (paylasilan dosya)
    private void DeleteOldUploaded()
    {
        try
        {
            string old = Repo.GetUserAvatar(Auth.UserId);
            if (!string.IsNullOrEmpty(old) && old.StartsWith("assets/avatars/") && old.IndexOf("/presets/") < 0)
            {
                string op = Server.MapPath("~/" + old);
                if (File.Exists(op)) File.Delete(op);
            }
        }
        catch { }
    }
}
