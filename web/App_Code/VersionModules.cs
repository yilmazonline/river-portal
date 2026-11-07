using System;
using System.Collections.Generic;
using System.Data.SqlClient;

// Versiyon duyurulari: modul listesi. Bolum basliginda "## Kategori | Modul" olarak saklanir.
public static class VersionModuleRepo
{
    public const int MAX_NAME = 80;

    /// <summary>Bolum basligini kategori ve module ayirir: "Iyilestirmeler | E-Fatura" -> (Iyilestirmeler, E-Fatura). Modulsuz baslikta modul bos.</summary>
    public static void SplitHeader(string header, out string cat, out string module)
    {
        header = (header ?? "").Trim(); module = "";
        int i = header.IndexOf('|');
        if (i < 0) { cat = header; return; }
        cat = header.Substring(0, i).Trim(); module = header.Substring(i + 1).Trim();
    }

    /// <summary>Modul listesi (ada gore). Tablo yoksa (migration 51 calismadiysa) bos liste.</summary>
    public static List<string> GetAll()
    {
        List<string> l = new List<string>();
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("IF OBJECT_ID('dbo.VersionModules', 'U') IS NOT NULL SELECT Name FROM dbo.VersionModules ORDER BY Name", con))
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read()) l.Add(r.GetString(0));
        }
        catch { }
        return l;
    }

    /// <summary>Modul ekler; ayni ad (buyuk-kucuk harf duyarsiz) varsa yenisini acmaz, kayitli adi doner.</summary>
    public static string Add(string name, int userId)
    {
        name = Clean(name);
        if (name.Length == 0) return null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "DECLARE @ex NVARCHAR(80) = (SELECT TOP 1 Name FROM dbo.VersionModules WHERE LOWER(Name) = LOWER(@n)); " +
            "IF @ex IS NULL BEGIN INSERT INTO dbo.VersionModules (Name, CreatedByUserId) VALUES (@n, @u); SELECT @n; END ELSE SELECT @ex;", con))
        {
            cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToString(cmd.ExecuteScalar());
        }
    }

    /// <summary>Ad temizligi: bosluklar teklesir, "|" ve satir sonu cikar (baslik bicimini bozmasin), en fazla 80 karakter</summary>
    public static string Clean(string s)
    {
        s = System.Text.RegularExpressions.Regex.Replace((s ?? "").Replace("|", " ").Replace("#", " "), @"\s+", " ").Trim();
        return s.Length > MAX_NAME ? s.Substring(0, MAX_NAME).Trim() : s;
    }
}
