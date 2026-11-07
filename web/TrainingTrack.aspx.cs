using System;
using System.Web.UI;

// İzleme takibi: action=view (izlenme sayacı) / action=progress (ilerleme + izleme günlüğü)
public partial class TrainingTrackPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/plain; charset=utf-8";
        if (!Auth.IsLoggedIn) { Response.Write("no"); return; }

        int mid;
        if (!int.TryParse(Request.Form["mid"], out mid) || mid <= 0) { Response.Write("bad"); return; }
        string action = (Request.Form["action"] ?? "").Trim();
        int uid = Auth.UserId;

        try
        {
            if (action == "reset")
            {
                // Devam Et'ten kaldir: yalniz POST ve ayni site (yikici islem)
                System.Uri refr = Request.UrlReferrer;
                if (Request.HttpMethod != "POST" || refr == null || !string.Equals(refr.Host, Request.Url.Host, System.StringComparison.OrdinalIgnoreCase)) { Response.Write("bad"); return; }
                TrainingDetailRepo.ResetUserVideo(uid, mid);
                Response.Write("ok"); return;
            }
            if (action == "view")
            {
                Repo.IncrementViewCount(mid);
            }
            else
            {
                int pos, dur, delta;
                int.TryParse(Request.Form["pos"], out pos);
                int.TryParse(Request.Form["dur"], out dur);
                int.TryParse(Request.Form["delta"], out delta);
                Repo.SaveProgress(mid, uid, pos, dur);
                if (delta > 0) Repo.AddWatchLog(uid, mid, delta);
                if (dur > 0) { try { TrainingDetailRepo.FillDuration(mid, dur); } catch { } }   // kapak kosesindeki sure
                // Sifirdan Uzmanliga rozetleri: izleme kaydindan SONRA ve ayri korumada (hata kaydi bozmaz)
                try { LearningPath.OnProgress(uid, Auth.CompanyId, mid); } catch { }
            }
            Response.Write("ok");
        }
        catch { Response.Write("err"); }
    }
}
