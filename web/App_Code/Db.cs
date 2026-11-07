using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;

// ADO.NET baglanti yardimcisi. EF/DLL yok; System.Data.SqlClient GAC'ta gelir.
public static class Db
{
    public static SqlConnection Open()
    {
        string cs = WebConfigurationManager.ConnectionStrings["Partner"].ConnectionString;
        SqlConnection con = new SqlConnection(cs);
        con.Open();
        return con;
    }
}

// SHA-256 parola dogrulamasi (demo). Uretimde BCrypt/PBKDF2 + salt kullanin.
// Seed'deki hash = SHA256(UTF8(parola)) -> kucuk harf hex. Demo parola: River2026!
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        if (password == null) password = "";
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }
    }

    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;
        string h = Hash(password);
        return string.Equals(h, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
