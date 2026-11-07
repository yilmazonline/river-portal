using System;
using System.Web.UI;

// Puanlama: yıldız (1-5) + opsiyonel yorum
public partial class SaveRatingPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/plain; charset=utf-8";
        if (!Auth.IsLoggedIn) { Response.Write("no"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Response.Write("bad"); return; }

        int mid, stars;
        if (!int.TryParse(Request.Form["mid"], out mid) || mid <= 0) { Response.Write("bad"); return; }
        if (!int.TryParse(Request.Form["stars"], out stars) || stars < 1 || stars > 5) { Response.Write("bad"); return; }
        // "comment" alani hic yoksa (yildiza tiklama): yalniz puan guncellenir, mevcut yorum korunur
        bool withComment = Request.Unvalidated.Form["comment"] != null;
        string comment = (Request.Unvalidated.Form["comment"] ?? "").Trim();
        if (comment.Length > 1000) comment = comment.Substring(0, 1000);

        try { Repo.SaveRating(mid, Auth.UserId, stars, comment, withComment); Response.Write("ok"); }
        catch { Response.Write("err"); }
    }
}
