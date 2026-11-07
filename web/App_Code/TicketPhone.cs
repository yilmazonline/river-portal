using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

// Destek talebi formunda "Sonraki taleplerimde hatirla": kullanicinin telefon numarasi.
// Yalniz kisi isaretleyip talep gonderdiginde saklanir; "Numarami sistemden sil" ile aninda silinir.
public static class TicketPhone
{
    /// <summary>Telefon numarasina benzeyen giris: rakam, + ( ) - bosluk; 7-15 rakam</summary>
    public static string Clean(string s)
    {
        s = (s ?? "").Trim();
        if (s.Length == 0 || s.Length > 30 || !Regex.IsMatch(s, @"^\+?[0-9\s\-\(\)\.]+$")) return null;
        int digits = Regex.Replace(s, @"\D", "").Length;
        return digits >= 7 && digits <= 15 ? Regex.Replace(s, @"\s+", " ") : null;
    }
    public static string Get(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TicketPhone FROM dbo.Users WHERE UserId = @u", con))
        { cmd.Parameters.AddWithValue("@u", userId); return cmd.ExecuteScalar() as string ?? ""; }
    }
    public static void Save(int userId, string phone)
    {
        string p = Clean(phone); if (p == null) return;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET TicketPhone = @p WHERE UserId = @u", con))
        { cmd.Parameters.AddWithValue("@p", p); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
    }
    public static void Forget(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET TicketPhone = NULL WHERE UserId = @u", con))
        { cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
    }
}
