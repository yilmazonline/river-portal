using System;
using System.Data.SqlClient;

// Zamanlanmis isler: gunde bir calisma. Claim -> isi yap -> basarisizsa Release.
public static class JobRuns
{
    /// <summary>Bugun (Turkiye saati) icin isi ustlenir. true: bu cagri calistirmali. false: bugun zaten calisti.
    /// null: tablo yok (migration 53 calismamis); cagiran tekrar onlemesiz calisabilir.</summary>
    public static bool? Claim(string key)
    {
        try
        {
            DateTime today = DateTime.UtcNow.AddHours(3).Date;
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "IF OBJECT_ID('dbo.JobRuns', 'U') IS NULL BEGIN SELECT -1; RETURN; END; " +
                "IF NOT EXISTS (SELECT 1 FROM dbo.JobRuns WITH (UPDLOCK, HOLDLOCK) WHERE JobKey = @k) INSERT INTO dbo.JobRuns (JobKey) VALUES (@k); " +
                "UPDATE dbo.JobRuns SET LastRunDate = @d, LastRunAt = SYSUTCDATETIME(), LastResult = NULL WHERE JobKey = @k AND LastRunDate < @d; SELECT @@ROWCOUNT;", con))
            {
                cmd.Parameters.AddWithValue("@k", key); cmd.Parameters.AddWithValue("@d", today);
                int n = Convert.ToInt32(cmd.ExecuteScalar());
                if (n < 0) return null;
                return n > 0;
            }
        }
        catch { return null; }
    }

    /// <summary>Is basarisiz oldu: bugunku isareti geri al (bir sonraki cagri yeniden deneyebilsin)</summary>
    public static void Release(string key, string result)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.JobRuns SET LastRunDate = DATEADD(DAY, -1, LastRunDate), LastResult = @r WHERE JobKey = @k", con))
            { cmd.Parameters.AddWithValue("@k", key); cmd.Parameters.AddWithValue("@r", Cut(result)); cmd.ExecuteNonQuery(); }
        }
        catch { }
    }

    public static void Note(string key, string result)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.JobRuns SET LastResult = @r WHERE JobKey = @k", con))
            { cmd.Parameters.AddWithValue("@k", key); cmd.Parameters.AddWithValue("@r", Cut(result)); cmd.ExecuteNonQuery(); }
        }
        catch { }
    }
    private static string Cut(string s) { s = s ?? ""; return s.Length > 400 ? s.Substring(0, 400) : s; }
}
