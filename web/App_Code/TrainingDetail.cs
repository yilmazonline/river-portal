using System;
using System.Collections.Generic;
using System.Data.SqlClient;

// Bir kullanicinin video bazinda ayrintili izleme kaydi (yonetici gorunumu icin).
// Kaynaklar: TrainingProgress (oran, konum, sure, son izleme), TrainingWatchLog (gercek izleme suresi: oynarken her 15 sn bir kayit),
// TrainingRatings (verdigi puan ve yorum).
public class VideoDetail
{
    public string Group, Title, YoutubeId;
    public int MaterialId, Percent, WatchedSeconds, LastPosition, DurationSeconds, WatchLogSeconds, ActiveDays;
    public bool Started, Done;
    public DateTime? LastWatchedAt, FirstLogAt, LastLogAt;
    public int? Stars;
    public string Comment;
    /// <summary>Sona sarma suphesi: tamamlandi gorunuyor ama gercek izleme suresi videonun yarisindan az</summary>
    public bool Skimmed { get { return Done && DurationSeconds > 120 && WatchLogSeconds > 0 && WatchLogSeconds < DurationSeconds / 2; } }   // izleme kaydi olmayan eski videolarda uyarma
}

public static class TrainingDetailRepo
{
    public static List<VideoDetail> GetUserVideoDetail(int userId, int companyId)
    {
        List<VideoDetail> list = new List<VideoDetail>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT ISNULL(NULLIF(m.CategoryGroup,N''), N'Diğer Eğitimler') AS Grp, m.MaterialId, m.TitleTr, m.YoutubeId, " +
            "p.ProgressId, ISNULL(p.[Percent],0) AS Pct, ISNULL(p.WatchedSeconds,0) AS Sec, ISNULL(p.LastPosition,0) AS LastPos, ISNULL(p.DurationSeconds,0) AS Dur, " +
            "CASE WHEN p.Completed = 1 OR p.[Percent] >= @pct THEN 1 ELSE 0 END AS Done, p.LastWatchedAt, " +
            "w.LogSec, w.FirstAt, w.LastAt, w.Days, r.Stars, r.Comment " +
            "FROM dbo.TrainingMaterials m " +
            "LEFT JOIN dbo.TrainingProgress p ON p.MaterialId = m.MaterialId AND p.UserId = @u " +
            "OUTER APPLY (SELECT SUM(l.Seconds) AS LogSec, MIN(l.LoggedAt) AS FirstAt, MAX(l.LoggedAt) AS LastAt, COUNT(DISTINCT CAST(l.LoggedAt AS DATE)) AS Days " +
            "             FROM dbo.TrainingWatchLog l WHERE l.UserId = @u AND l.MaterialId = m.MaterialId) w " +
            "LEFT JOIN dbo.TrainingRatings r ON r.MaterialId = m.MaterialId AND r.UserId = @u " +
            "WHERE m.MaterialType = N'Video' AND m.IsActive = 1 AND (m.CompanyId IS NULL OR m.CompanyId = @c) " +
            "ORDER BY CASE WHEN NULLIF(m.CategoryGroup,N'') IS NULL THEN 1 ELSE 0 END, m.CategoryGroup, m.SortOrder, m.MaterialId", con))
        {
            cmd.Parameters.AddWithValue("@pct", ExamRepo.VideoCompletePercent);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    VideoDetail v = new VideoDetail();
                    v.Group = r["Grp"] as string; v.MaterialId = (int)r["MaterialId"]; v.Title = r["TitleTr"] as string; v.YoutubeId = (r["YoutubeId"] as string) ?? "";
                    v.Started = r["ProgressId"] != DBNull.Value;
                    v.Percent = Math.Min(100, Convert.ToInt32(r["Pct"])); v.WatchedSeconds = Convert.ToInt32(r["Sec"]);
                    v.LastPosition = Convert.ToInt32(r["LastPos"]); v.DurationSeconds = Convert.ToInt32(r["Dur"]);
                    v.Done = Convert.ToInt32(r["Done"]) == 1;
                    if (v.Done) v.Percent = Math.Max(v.Percent, ExamRepo.VideoCompletePercent);
                    v.LastWatchedAt = r["LastWatchedAt"] as DateTime?;
                    v.WatchLogSeconds = r["LogSec"] == DBNull.Value ? 0 : Convert.ToInt32(r["LogSec"]);
                    v.FirstLogAt = r["FirstAt"] as DateTime?; v.LastLogAt = r["LastAt"] as DateTime?;
                    v.ActiveDays = r["Days"] == DBNull.Value ? 0 : Convert.ToInt32(r["Days"]);
                    v.Stars = r["Stars"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["Stars"]);
                    v.Comment = r["Comment"] as string;
                    list.Add(v);
                }
        }
        return list;
    }

    /// <summary>Kullanicinin tum videolardaki genel izleme ozeti</summary>
    public static void GetUserTotals(int userId, out int totalSeconds, out int activeDays, out DateTime? firstAt, out DateTime? lastAt)
    {
        totalSeconds = 0; activeDays = 0; firstAt = null; lastAt = null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(SUM(Seconds),0), COUNT(DISTINCT CAST(LoggedAt AS DATE)), MIN(LoggedAt), MAX(LoggedAt) FROM dbo.TrainingWatchLog WHERE UserId = @u", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    totalSeconds = Convert.ToInt32(r[0]); activeDays = Convert.ToInt32(r[1]);
                    firstAt = r.IsDBNull(2) ? (DateTime?)null : r.GetDateTime(2); lastAt = r.IsDBNull(3) ? (DateTime?)null : r.GetDateTime(3);
                }
        }
    }

    /// <summary>En cok izleyenler (editor paneli): kisi basina toplam izleme suresi, izledigi ve tamamladigi video, aktif gun, son izleme;
    /// firma adi ve turuyle; en cok izleyenden en aza. sinceUtc bos = tum zamanlar.</summary>
    public static List<Dictionary<string, object>> TopWatchers(DateTime? sinceUtc)
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT u.UserId, u.FullName, u.Email, u.AvatarImage, c.Name AS CompanyName, c.CompanyType, w.Sec, w.Vids, w.Days, w.LastAt, ISNULL(p.Done, 0) AS Done " +
            "FROM (SELECT UserId, SUM(Seconds) AS Sec, COUNT(DISTINCT MaterialId) AS Vids, COUNT(DISTINCT CAST(LoggedAt AS DATE)) AS Days, MAX(LoggedAt) AS LastAt " +
            "      FROM dbo.TrainingWatchLog WHERE (@s IS NULL OR LoggedAt >= @s) GROUP BY UserId HAVING SUM(Seconds) > 0) w " +
            "JOIN dbo.Users u ON u.UserId = w.UserId LEFT JOIN dbo.Companies c ON c.CompanyId = u.CompanyId " +
            "OUTER APPLY (SELECT COUNT(*) AS Done FROM dbo.TrainingProgress tp WHERE tp.UserId = w.UserId AND tp.Completed = 1 AND (@s IS NULL OR tp.LastWatchedAt >= @s)) p " +
            "ORDER BY w.Sec DESC, w.LastAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@s", sinceUtc.HasValue ? (object)sinceUtc.Value : DBNull.Value);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["id"] = (int)r["UserId"]; d["name"] = r["FullName"] as string ?? ""; d["email"] = r["Email"] as string ?? ""; d["ava"] = r["AvatarImage"] as string ?? "";
                    d["company"] = r["CompanyName"] as string ?? ""; string ct = r["CompanyType"] as string ?? "";
                    d["type"] = ct == "Vendor" ? "fol" : ct == "Partner" ? "partner" : ct == "Customer" ? "customer" : "";
                    d["sec"] = Convert.ToInt32(r["Sec"]); d["videos"] = Convert.ToInt32(r["Vids"]); d["done"] = Convert.ToInt32(r["Done"]); d["days"] = Convert.ToInt32(r["Days"]);
                    d["last"] = r["LastAt"] == DBNull.Value ? null : DateTime.SpecifyKind((DateTime)r["LastAt"], DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssZ");
                    l.Add(d);
                }
        }
        return l;
    }

    /// <summary>"Devam Et'ten kaldir": kisinin bu videodaki ilerlemesi ve izleme suresi kaydi silinir (rozetler korunur)</summary>
    public static void ResetUserVideo(int userId, int materialId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "DELETE FROM dbo.TrainingProgress WHERE UserId = @u AND MaterialId = @m; DELETE FROM dbo.TrainingWatchLog WHERE UserId = @u AND MaterialId = @m;", con))
        { cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@m", materialId); cmd.ExecuteNonQuery(); }
    }

    /// <summary>Videonun suresi bos ise oynaticinin bildirdigi sureyi yaz (kapak kosesindeki sure icin)</summary>
    public static void FillDuration(int materialId, int seconds)
    {
        if (seconds <= 0 || seconds > 24 * 3600) return;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TrainingMaterials SET DurationSeconds = @d WHERE MaterialId = @m AND ISNULL(DurationSeconds, 0) = 0", con))
        { cmd.Parameters.AddWithValue("@d", seconds); cmd.Parameters.AddWithValue("@m", materialId); cmd.ExecuteNonQuery(); }
    }

    /// <summary>"1 sa 12 dk" / "12 dk" / "45 sn"</summary>
    public static string Dur(int s)
    {
        if (s <= 0) return "0 dk";
        if (s < 60) return s + " sn";
        int h = s / 3600, m = (s % 3600) / 60;
        return h > 0 ? h + " sa" + (m > 0 ? " " + m + " dk" : "") : m + " dk";
    }
}
