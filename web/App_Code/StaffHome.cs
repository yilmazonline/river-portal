using System;
using System.Collections.Generic;
using System.Data.SqlClient;

// Fol personeli paneli: destek ekibi akisi, ilk yanit suresi, ekip yuku, icerik onerileri, dosya paylasimlari
public static class StaffHome
{
    public class Load { public int? UserId; public string Name; public int Open; }

    /// <summary>Son N gun (bugun dahil, TR saati): gunluk gelen ve cozulen talep sayilari</summary>
    public static void Flow(int days, out DateTime[] dates, out int[] incoming, out int[] resolved)
    {
        DateTime todayTr = DateTime.UtcNow.AddHours(3).Date, fromTr = todayTr.AddDays(-(days - 1));
        dates = new DateTime[days]; incoming = new int[days]; resolved = new int[days];
        for (int i = 0; i < days; i++) dates[i] = fromTr.AddDays(i);
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("SELECT CAST(DATEADD(HOUR, 3, CreatedAt) AS DATE), COUNT(*) FROM dbo.Tickets WHERE CreatedAt >= @f GROUP BY CAST(DATEADD(HOUR, 3, CreatedAt) AS DATE)", con))
            {
                cmd.Parameters.AddWithValue("@f", fromTr.AddHours(-3));
                using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) { int i = (int)(r.GetDateTime(0) - fromTr).TotalDays; if (i >= 0 && i < days) incoming[i] = r.GetInt32(1); }
            }
            using (SqlCommand cmd = new SqlCommand("SELECT CAST(DATEADD(HOUR, 3, ResolvedAt) AS DATE), COUNT(*) FROM dbo.Tickets WHERE ResolvedAt >= @f GROUP BY CAST(DATEADD(HOUR, 3, ResolvedAt) AS DATE)", con))
            {
                cmd.Parameters.AddWithValue("@f", fromTr.AddHours(-3));
                using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) { int i = (int)(r.GetDateTime(0) - fromTr).TotalDays; if (i >= 0 && i < days) resolved[i] = r.GetInt32(1); }
            }
        }
    }

    /// <summary>Son N gunde acilan taleplerde destek ekibinin ilk (ic not olmayan) yanitina kadar ortalama dakika; veri yoksa -1</summary>
    public static int AvgFirstResponseMinutes(int days)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT AVG(CAST(DATEDIFF(MINUTE, t.CreatedAt, f.FirstAt) AS FLOAT)) FROM dbo.Tickets t " +
            "CROSS APPLY (SELECT MIN(m.CreatedAt) AS FirstAt FROM dbo.TicketMessages m WHERE m.TicketId = t.TicketId AND m.AuthorSide = N'Support' AND m.IsInternal = 0) f " +
            "WHERE t.CreatedAt >= DATEADD(DAY, -@d, SYSUTCDATETIME()) AND f.FirstAt IS NOT NULL", con))
        {
            cmd.Parameters.AddWithValue("@d", days);
            object o = cmd.ExecuteScalar();
            return o == null || o == DBNull.Value ? -1 : (int)Math.Round(Convert.ToDouble(o));
        }
    }

    /// <summary>Bugun (TR) destek ekibinin gonderdigi yanit sayisi (ic notlar haric)</summary>
    public static int RepliesToday()
    {
        DateTime fromUtc = DateTime.UtcNow.AddHours(3).Date.AddHours(-3);
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.TicketMessages WHERE AuthorSide = N'Support' AND IsInternal = 0 AND CreatedAt >= @f", con))
        { cmd.Parameters.AddWithValue("@f", fromUtc); return Convert.ToInt32(cmd.ExecuteScalar()); }
    }

    /// <summary>Ekip yuku: uzman basina acik talep (en fazla 5 kisi, en yuklu once) + atanmamislar ayri</summary>
    public static List<Load> TeamLoad(out int unassigned)
    {
        unassigned = 0; List<Load> l = new List<Load>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT t.AssignedToUserId, u.FullName, COUNT(*) FROM dbo.Tickets t LEFT JOIN dbo.Users u ON u.UserId = t.AssignedToUserId " +
            "WHERE t.Status IN (N'New', N'InProgress', N'WaitingCustomer') GROUP BY t.AssignedToUserId, u.FullName ORDER BY COUNT(*) DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                if (r.IsDBNull(0)) { unassigned = r.GetInt32(2); continue; }
                Load x = new Load(); x.UserId = r.GetInt32(0); x.Name = r[1] as string ?? ""; x.Open = r.GetInt32(2); l.Add(x);
            }
        return l;
    }

    /// <summary>Son N gunde gelen Akademi icerik onerisi sayisi ve en yenisinin zamani</summary>
    public static int RecentSuggestions(int days, out DateTime? latestUtc)
    {
        latestUtc = null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*), MAX(CreatedAt) FROM dbo.ContentSuggestions WHERE CreatedAt >= DATEADD(DAY, -@d, SYSUTCDATETIME())", con))
        {
            cmd.Parameters.AddWithValue("@d", days);
            using (SqlDataReader r = cmd.ExecuteReader()) { if (!r.Read()) return 0; latestUtc = r[1] as DateTime?; return r.GetInt32(0); }
        }
    }

    /// <summary>Kisinin dosya paylasimlari: son 7 gundeki indirme sayisi ve 24 saat icinde suresi dolacak paylasim sayisi (en yakini)</summary>
    public static void MyTransfers(int userId, out int downloads7, out int expiringSoon, out string expiringTitle)
    {
        downloads7 = 0; expiringSoon = 0; expiringTitle = null;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.TransferDownloads d JOIN dbo.TransferRecipients r ON r.RecipientId = d.RecipientId JOIN dbo.Transfers t ON t.TransferId = r.TransferId " +
                "WHERE t.CreatedBy = @u AND d.DownloadedAt >= DATEADD(DAY, -7, SYSUTCDATETIME())", con))
            { cmd.Parameters.AddWithValue("@u", userId); downloads7 = Convert.ToInt32(cmd.ExecuteScalar()); }
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) OVER (), Title FROM dbo.Transfers WHERE CreatedBy = @u AND ExpiresAt > SYSUTCDATETIME() AND ExpiresAt <= DATEADD(HOUR, 24, SYSUTCDATETIME()) ORDER BY ExpiresAt", con))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) { expiringSoon = r.GetInt32(0); expiringTitle = r[1] as string; }
            }
        }
    }
}
