using System;
using System.Collections.Generic;
using System.Data.SqlClient;

// Ana sayfalar (partner + musteri): ortak yardimcilar ve firma geneli hesaplamalar
public static class PartnerHome
{
    public class Mate { public int UserId; public string Name; public int Done, Total; public bool Me; }

    /// <summary>05-11 Gunaydin, 12-17 Iyi gunler, 18-21 Iyi aksamlar, 22-04 Iyi geceler (TR saati)</summary>
    public static string GreetingFor(int hour, bool en)
    {
        if (hour >= 5 && hour < 12) return en ? "Good morning" : "Günaydın";
        if (hour >= 12 && hour < 18) return en ? "Good afternoon" : "İyi günler";
        if (hour >= 18 && hour < 22) return en ? "Good evening" : "İyi akşamlar";
        return en ? "Good night" : "İyi geceler";
    }

    /// <summary>Destek talebi sayilari (acik / yanit beklenen)</summary>
    public static void TicketCounts(int companyId, out int open, out int waiting)
    {
        open = 0; waiting = 0;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT SUM(CASE WHEN Status IN (N'New', N'InProgress', N'WaitingCustomer') THEN 1 ELSE 0 END), SUM(CASE WHEN Status = N'WaitingCustomer' THEN 1 ELSE 0 END) " +
            "FROM dbo.Tickets WHERE CompanyId = @c", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read()) { open = r.IsDBNull(0) ? 0 : Convert.ToInt32(r[0]); waiting = r.IsDBNull(1) ? 0 : Convert.ToInt32(r[1]); }
        }
    }

    /// <summary>Talebe gorunur son yaniti veren kisi (ic notlar haric) ve zamani</summary>
    public static bool LastReply(int ticketId, out string name, out DateTime whenUtc)
    {
        name = null; whenUtc = DateTime.MinValue;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 1 u.FullName, m.CreatedAt FROM dbo.TicketMessages m LEFT JOIN dbo.Users u ON u.UserId = m.AuthorUserId " +
            "WHERE m.TicketId = @t AND m.IsInternal = 0 ORDER BY m.CreatedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@t", ticketId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return false;
                name = r[0] as string; whenUtc = (DateTime)r[1]; return true;
            }
        }
    }

    /// <summary>"12 sa 30 dk" / "45 dk"</summary>
    public static string Dur(int minutes, bool en)
    {
        if (minutes < 0) minutes = 0;
        int h = minutes / 60, m = minutes % 60;
        if (h == 0) return m + (en ? " min" : " dk");
        return h + (en ? " h" : " sa") + (m > 0 ? " " + m + (en ? " min" : " dk") : "");
    }

    /// <summary>Firmanin izleme suresi (saniye) ve izleyen kisi sayisi, verilen UTC tarihten bu yana</summary>
    public static void WatchStats(int companyId, DateTime fromUtc, out int seconds, out int people)
    {
        seconds = 0; people = 0;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT ISNULL(SUM(w.Seconds), 0), COUNT(DISTINCT w.UserId) FROM dbo.TrainingWatchLog w JOIN dbo.Users u ON u.UserId = w.UserId " +
            "WHERE u.CompanyId = @c AND w.LoggedAt >= @f", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@f", fromUtc);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) { seconds = Convert.ToInt32(r[0]); people = Convert.ToInt32(r[1]); }
        }
    }

    /// <summary>Firmadaki gecerli sertifika sayisi ve kisinin en son gecerli sertifikasi</summary>
    public static void CertStats(int companyId, int userId, out int valid, out string mine)
    {
        valid = 0; mine = null;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Certificates c JOIN dbo.Users u ON u.UserId = c.UserId WHERE u.CompanyId = @c AND c.IsRevoked = 0 AND (c.ValidUntil IS NULL OR c.ValidUntil > SYSUTCDATETIME())", con))
            { cmd.Parameters.AddWithValue("@c", companyId); valid = Convert.ToInt32(cmd.ExecuteScalar()); }
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 ExamTitle FROM dbo.Certificates WHERE UserId = @u AND IsRevoked = 0 AND (ValidUntil IS NULL OR ValidUntil > SYSUTCDATETIME()) ORDER BY IssuedAt DESC", con))
            { cmd.Parameters.AddWithValue("@u", userId); mine = cmd.ExecuteScalar() as string; }
        }
    }

    /// <summary>Takim: kisinin kendisi + basari paylasimi acik takim arkadaslari, Sifirdan Uzmanliga ilerlemesine gore (en fazla 30 kisi hesaplanir)</summary>
    public static List<Mate> Team(int userId, int companyId)
    {
        List<Mate> l = new List<Mate>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 30 UserId, FullName FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 AND (UserId = @u OR ISNULL(ShareAchievements, 1) = 1) ORDER BY CASE WHEN UserId = @u THEN 0 ELSE 1 END, FullName", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) { Mate m = new Mate(); m.UserId = r.GetInt32(0); m.Name = r[1] as string ?? ""; m.Me = m.UserId == userId; l.Add(m); }
        }
        foreach (Mate m in l)
        {
            List<PathStep> steps = LearningPath.GetSteps(m.UserId, companyId);
            m.Total = steps.Count; foreach (PathStep s in steps) if (s.State == "done") m.Done++;
        }
        l.Sort(delegate (Mate a, Mate b) { int c = b.Done.CompareTo(a.Done); return c != 0 ? c : string.Compare(a.Name, b.Name, StringComparison.CurrentCulture); });
        return l;
    }

    /// <summary>Takimda son 14 gunde kazanilan en son rozet (paylasimi acik kisilerden, kisinin kendisi haric)</summary>
    public static bool TeamRecent(int userId, int companyId, out string who, out string title, out DateTime whenUtc)
    {
        who = null; title = null; whenUtc = DateTime.MinValue;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 1 u.FullName, b.BadgeKey, b.EarnedAt, m.TitleTr FROM dbo.UserBadges b JOIN dbo.Users u ON u.UserId = b.UserId " +
            "LEFT JOIN dbo.TrainingMaterials m ON m.MaterialId = b.MaterialId " +
            "WHERE u.CompanyId = @c AND u.IsActive = 1 AND u.UserId <> @u AND ISNULL(u.ShareAchievements, 1) = 1 AND b.IsRetro = 0 AND b.BadgeKey NOT LIKE N'sys:%' " +
            "AND b.EarnedAt >= DATEADD(DAY, -14, SYSUTCDATETIME()) ORDER BY b.EarnedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return false;
                string key = (string)r[1];
                who = ((r[0] as string) ?? "").Split(' ')[0];
                title = key == "m:first" ? "İlk Adım" : key == "m:half" ? "Yarı Yol" : key == "path:done" ? LearningPath.PATH_NAME : (r[3] as string ?? "");
                whenUtc = (DateTime)r[2];
                return true;
            }
        }
    }
}
