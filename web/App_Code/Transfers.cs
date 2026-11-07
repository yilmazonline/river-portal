using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;

// ===================== MODELLER =====================
public class TransferInfo
{
    public int TransferId, CreatedBy;
    public string Title, Message, CreatedByName, Status, PasswordHash;
    public DateTime CreatedAt, ExpiresAt;
    public DateTime? SentAt;
    public long TotalBytes;
    public int? MaxDownloads;
    public int FileCount, RecipientCount, DownloadedCount;
    public bool HasPassword { get { return !string.IsNullOrEmpty(PasswordHash); } }
    public bool IsExpired { get { return Status == "Expired" || (Status == "Sent" && ExpiresAt < DateTime.UtcNow); } }
    public bool IsLive { get { return Status == "Sent" && ExpiresAt >= DateTime.UtcNow; } }
}

public class TransferFile
{
    public int FileId, TransferId;
    public string OriginalName, ContentType;
    public long SizeBytes, UploadedBytes;
    public bool IsComplete;
}

public class TransferRecipient
{
    public int RecipientId, TransferId, DownloadCount;
    public string Token, Email, DisplayName, CompanyName;
    public int? UserId, CompanyId;
    public DateTime? SentAt, FirstDownloadAt, LastDownloadAt;
}

// ===================== VERI + DEPOLAMA =====================
public static class TransferRepo
{
    public const int ChunkBytes = 16 * 1024 * 1024;   // 16 MB parca (Cloudflare 100 MB, IIS 50 MB sinirinin altinda)

    // Transfer basina azami boyut (web.config: Transfer.MaxGB, varsayilan 5)
    public static long MaxBytes
    {
        get
        {
            double gb; string s = (WebConfigurationManager.AppSettings["Transfer.MaxGB"] ?? "").Trim();
            if (!double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out gb) || gb <= 0) gb = 5;
            return (long)(gb * 1024 * 1024 * 1024);
        }
    }

    // Depolama koku: web klasorunun DISINDA (dogrudan URL ile erisilemez). web.config: Transfer.StoragePath
    public static string StorageRoot
    {
        get
        {
            string p = (WebConfigurationManager.AppSettings["Transfer.StoragePath"] ?? "").Trim();
            if (p.Length == 0) p = Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, "..", "transfer_data"));
            if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            return p;
        }
    }

    public static string FilePath(int transferId, int fileId)
    {
        string dir = Path.Combine(StorageRoot, transferId.ToString());
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileId + ".bin");
    }

    public static void DeleteStorage(int transferId)
    {
        try { string dir = Path.Combine(StorageRoot, transferId.ToString()); if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { }
    }

    public static string FormatSize(long b)
    {
        System.Globalization.CultureInfo tr = new System.Globalization.CultureInfo("tr-TR");
        if (b >= 1073741824L) return (b / 1073741824.0).ToString("0.##", tr) + " GB";
        if (b >= 1048576L) return (b / 1048576.0).ToString("0.#", tr) + " MB";
        if (b >= 1024L) return (b / 1024.0).ToString("0", tr) + " KB";
        return b + " B";
    }

    // Tahmin edilemez 24 karakterlik link anahtari
    public static string NewToken()
    {
        const string a = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        byte[] buf = new byte[24];
        using (System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create()) rng.GetBytes(buf);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < buf.Length; i++) sb.Append(a[buf[i] % a.Length]);
        return sb.ToString();
    }

    // ---------- Transfer ----------
    private const string TR_SELECT =
        "SELECT t.TransferId, t.Title, t.Message, t.CreatedBy, t.CreatedByName, t.CreatedAt, t.SentAt, t.ExpiresAt, t.Status, t.TotalBytes, t.MaxDownloads, t.PasswordHash, " +
        "(SELECT COUNT(*) FROM dbo.TransferFiles f WHERE f.TransferId = t.TransferId) AS FCnt, " +
        "(SELECT COUNT(*) FROM dbo.TransferRecipients r WHERE r.TransferId = t.TransferId) AS RCnt, " +
        "(SELECT COUNT(*) FROM dbo.TransferRecipients r WHERE r.TransferId = t.TransferId AND r.DownloadCount > 0) AS DCnt " +
        "FROM dbo.Transfers t ";

    private static TransferInfo ReadTr(SqlDataReader r)
    {
        TransferInfo t = new TransferInfo();
        t.TransferId = (int)r["TransferId"]; t.Title = r["Title"] as string; t.Message = r["Message"] as string;
        t.CreatedBy = (int)r["CreatedBy"]; t.CreatedByName = r["CreatedByName"] as string;
        t.CreatedAt = (DateTime)r["CreatedAt"]; t.SentAt = r["SentAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["SentAt"];
        t.ExpiresAt = (DateTime)r["ExpiresAt"]; t.Status = r["Status"] as string; t.TotalBytes = Convert.ToInt64(r["TotalBytes"]);
        t.MaxDownloads = r["MaxDownloads"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["MaxDownloads"]);
        t.PasswordHash = r["PasswordHash"] as string;
        t.FileCount = Convert.ToInt32(r["FCnt"]); t.RecipientCount = Convert.ToInt32(r["RCnt"]); t.DownloadedCount = Convert.ToInt32(r["DCnt"]);
        return t;
    }

    public static TransferInfo GetTransfer(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TR_SELECT + "WHERE t.TransferId=@id", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadTr(r);
        }
        return null;
    }

    // createdByOrZero = 0 -> tum transferler (portal yoneticisi)
    public static List<TransferInfo> ListTransfers(int createdByOrZero)
    {
        List<TransferInfo> list = new List<TransferInfo>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TR_SELECT + "WHERE t.Status <> N'Uploading'" + (createdByOrZero > 0 ? " AND t.CreatedBy=@u" : "") + " ORDER BY t.CreatedAt DESC", con))
        {
            if (createdByOrZero > 0) cmd.Parameters.AddWithValue("@u", createdByOrZero);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(ReadTr(r));
        }
        return list;
    }

    public static int CreateTransfer(string title, string message, int userId, string userName, DateTime expiresAt, int? maxDownloads, string passwordHash,
                                     List<string> names, List<long> sizes, List<string> types, out List<int> fileIds)
    {
        fileIds = new List<int>();
        long total = 0; for (int i = 0; i < sizes.Count; i++) total += sizes[i];
        int tid;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.Transfers (Title, Message, CreatedBy, CreatedByName, ExpiresAt, TotalBytes, MaxDownloads, PasswordHash) OUTPUT INSERTED.TransferId " +
                "VALUES (@t, @m, @u, @un, @e, @b, @md, @p)", con))
            {
                cmd.Parameters.AddWithValue("@t", title); cmd.Parameters.AddWithValue("@m", (object)message ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@un", (object)userName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@e", expiresAt); cmd.Parameters.AddWithValue("@b", total);
                cmd.Parameters.AddWithValue("@md", maxDownloads.HasValue ? (object)maxDownloads.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@p", (object)passwordHash ?? DBNull.Value);
                tid = (int)cmd.ExecuteScalar();
            }
            for (int i = 0; i < names.Count; i++)
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.TransferFiles (TransferId, OriginalName, ContentType, SizeBytes) OUTPUT INSERTED.FileId VALUES (@t, @n, @c, @s)", con))
                {
                    cmd.Parameters.AddWithValue("@t", tid); cmd.Parameters.AddWithValue("@n", names[i]);
                    cmd.Parameters.AddWithValue("@c", (object)types[i] ?? DBNull.Value); cmd.Parameters.AddWithValue("@s", sizes[i]);
                    fileIds.Add((int)cmd.ExecuteScalar());
                }
        }
        return tid;
    }

    public static void SetStatus(int transferId, string status)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Transfers SET Status=@s" + (status == "Sent" ? ", SentAt=SYSUTCDATETIME()" : "") + " WHERE TransferId=@id", con))
        { cmd.Parameters.AddWithValue("@s", status); cmd.Parameters.AddWithValue("@id", transferId); cmd.ExecuteNonQuery(); }
    }

    public static void ExtendExpiry(int transferId, int days)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.Transfers SET ExpiresAt = DATEADD(DAY, @d, CASE WHEN ExpiresAt < SYSUTCDATETIME() THEN SYSUTCDATETIME() ELSE ExpiresAt END) WHERE TransferId=@id AND Status=N'Sent'", con))
        { cmd.Parameters.AddWithValue("@d", days); cmd.Parameters.AddWithValue("@id", transferId); cmd.ExecuteNonQuery(); }
    }

    // ---------- Dosyalar ----------
    public static List<TransferFile> GetFiles(int transferId)
    {
        List<TransferFile> list = new List<TransferFile>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT FileId, TransferId, OriginalName, ContentType, SizeBytes, UploadedBytes, IsComplete FROM dbo.TransferFiles WHERE TransferId=@t ORDER BY FileId", con))
        {
            cmd.Parameters.AddWithValue("@t", transferId);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    TransferFile f = new TransferFile();
                    f.FileId = (int)r["FileId"]; f.TransferId = (int)r["TransferId"]; f.OriginalName = r["OriginalName"] as string;
                    f.ContentType = r["ContentType"] as string; f.SizeBytes = Convert.ToInt64(r["SizeBytes"]);
                    f.UploadedBytes = Convert.ToInt64(r["UploadedBytes"]); f.IsComplete = (bool)r["IsComplete"];
                    list.Add(f);
                }
        }
        return list;
    }

    public static TransferFile GetFile(int fileId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT FileId, TransferId, OriginalName, ContentType, SizeBytes, UploadedBytes, IsComplete FROM dbo.TransferFiles WHERE FileId=@f", con))
        {
            cmd.Parameters.AddWithValue("@f", fileId);
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    TransferFile f = new TransferFile();
                    f.FileId = (int)r["FileId"]; f.TransferId = (int)r["TransferId"]; f.OriginalName = r["OriginalName"] as string;
                    f.ContentType = r["ContentType"] as string; f.SizeBytes = Convert.ToInt64(r["SizeBytes"]);
                    f.UploadedBytes = Convert.ToInt64(r["UploadedBytes"]); f.IsComplete = (bool)r["IsComplete"];
                    return f;
                }
        }
        return null;
    }

    public static void SetUploaded(int fileId, long uploaded, bool complete)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TransferFiles SET UploadedBytes=@u, IsComplete=@c WHERE FileId=@f", con))
        { cmd.Parameters.AddWithValue("@u", uploaded); cmd.Parameters.AddWithValue("@c", complete); cmd.Parameters.AddWithValue("@f", fileId); cmd.ExecuteNonQuery(); }
    }

    // ---------- Alicilar ----------
    private static TransferRecipient ReadRc(SqlDataReader r)
    {
        TransferRecipient x = new TransferRecipient();
        x.RecipientId = (int)r["RecipientId"]; x.TransferId = (int)r["TransferId"]; x.Token = r["Token"] as string;
        x.Email = r["Email"] as string; x.DisplayName = r["DisplayName"] as string; x.CompanyName = r["CompanyName"] as string;
        x.UserId = r["UserId"] == DBNull.Value ? (int?)null : (int)r["UserId"];
        x.CompanyId = r["CompanyId"] == DBNull.Value ? (int?)null : (int)r["CompanyId"];
        x.SentAt = r["SentAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["SentAt"];
        x.FirstDownloadAt = r["FirstDownloadAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["FirstDownloadAt"];
        x.LastDownloadAt = r["LastDownloadAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["LastDownloadAt"];
        x.DownloadCount = (int)r["DownloadCount"];
        return x;
    }
    private const string RC_SELECT = "SELECT RecipientId, TransferId, Token, Email, DisplayName, UserId, CompanyId, CompanyName, SentAt, FirstDownloadAt, LastDownloadAt, DownloadCount FROM dbo.TransferRecipients ";

    public static List<TransferRecipient> GetRecipients(int transferId)
    {
        List<TransferRecipient> list = new List<TransferRecipient>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(RC_SELECT + "WHERE TransferId=@t ORDER BY CompanyName, DisplayName, Email", con))
        {
            cmd.Parameters.AddWithValue("@t", transferId);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(ReadRc(r));
        }
        return list;
    }

    public static TransferRecipient GetRecipientByToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length > 40) return null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(RC_SELECT + "WHERE Token=@k", con))
        {
            cmd.Parameters.AddWithValue("@k", token);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadRc(r);
        }
        return null;
    }

    public static TransferRecipient GetRecipient(int recipientId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(RC_SELECT + "WHERE RecipientId=@id", con))
        {
            cmd.Parameters.AddWithValue("@id", recipientId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadRc(r);
        }
        return null;
    }

    public static int AddRecipient(int transferId, string token, string email, string name, int? userId, int? companyId, string companyName)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.TransferRecipients (TransferId, Token, Email, DisplayName, UserId, CompanyId, CompanyName, SentAt) OUTPUT INSERTED.RecipientId " +
            "VALUES (@t, @k, @e, @n, @u, @c, @cn, SYSUTCDATETIME())", con))
        {
            cmd.Parameters.AddWithValue("@t", transferId); cmd.Parameters.AddWithValue("@k", token); cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@n", (object)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@u", userId.HasValue ? (object)userId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@c", companyId.HasValue ? (object)companyId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@cn", (object)companyName ?? DBNull.Value);
            return (int)cmd.ExecuteScalar();
        }
    }

    public static void TouchResent(int recipientId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TransferRecipients SET SentAt=SYSUTCDATETIME() WHERE RecipientId=@id", con))
        { cmd.Parameters.AddWithValue("@id", recipientId); cmd.ExecuteNonQuery(); }
    }

    public static void LogDownload(int recipientId, int fileId, string ip)
    {
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.TransferDownloads (RecipientId, FileId, Ip) VALUES (@r, @f, @ip)", con))
            { cmd.Parameters.AddWithValue("@r", recipientId); cmd.Parameters.AddWithValue("@f", fileId); cmd.Parameters.AddWithValue("@ip", (object)ip ?? DBNull.Value); cmd.ExecuteNonQuery(); }
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.TransferRecipients SET DownloadCount = DownloadCount + 1, LastDownloadAt = SYSUTCDATETIME(), FirstDownloadAt = ISNULL(FirstDownloadAt, SYSUTCDATETIME()) WHERE RecipientId=@r", con))
            { cmd.Parameters.AddWithValue("@r", recipientId); cmd.ExecuteNonQuery(); }
        }
    }

    // ---------- Temizlik (cron) ----------
    // Suresi dolan / iptal edilen transferlerin dosyalarini siler; 24 saatten eski yarim kalmis yuklemeleri temizler.
    public static int Cleanup()
    {
        List<int> ids = new List<int>();
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.Transfers SET Status=N'Expired' OUTPUT INSERTED.TransferId WHERE Status=N'Sent' AND ExpiresAt < SYSUTCDATETIME()", con))
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) ids.Add((int)r[0]);
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.Transfers SET Status=N'Cancelled' OUTPUT INSERTED.TransferId WHERE Status=N'Uploading' AND CreatedAt < DATEADD(HOUR, -24, SYSUTCDATETIME())", con))
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) ids.Add((int)r[0]);
        }
        // Onceden iptal/suresi dolmus olup klasoru kalanlar
        try
        {
            foreach (string d in Directory.GetDirectories(StorageRoot))
            {
                int id; if (!int.TryParse(Path.GetFileName(d), out id)) continue;
                TransferInfo t = GetTransfer(id);
                if (t == null || t.Status == "Cancelled" || t.Status == "Expired") { if (!ids.Contains(id)) ids.Add(id); }
            }
        }
        catch { }
        for (int i = 0; i < ids.Count; i++) DeleteStorage(ids[i]);
        return ids.Count;
    }
}
