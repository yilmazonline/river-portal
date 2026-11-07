using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

// Destek talebi ekleri: izin verilen turler, guvenli MIME, onizleme turu ve yetki kontrollu okuma.
// Talep acma (AddTicket), yanit (TicketAction) ve API (api/v1/tickets) AYNI listeyi kullanir.
// .aspx/.html/.svg/.exe gibi sunucuda calisabilecek ya da tarayicida betik calistirabilecek turler kabul edilmez.
public static class TicketFiles
{
    private static readonly Dictionary<string, string> Mime = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".pdf", "application/pdf" },
        { ".png", "image/png" }, { ".jpg", "image/jpeg" }, { ".jpeg", "image/jpeg" }, { ".gif", "image/gif" }, { ".webp", "image/webp" },
        { ".txt", "text/plain; charset=utf-8" }, { ".log", "text/plain; charset=utf-8" }, { ".csv", "text/plain; charset=utf-8" },
        { ".json", "text/plain; charset=utf-8" }, { ".xml", "text/plain; charset=utf-8" },
        { ".zip", "application/zip" },
        { ".doc", "application/msword" }, { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" }, { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt", "application/vnd.ms-powerpoint" }, { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
    };

    public static string Ext(string fileName) { return (Path.GetExtension(fileName ?? "") ?? "").ToLowerInvariant(); }
    public static bool IsAllowed(string fileName) { return Mime.ContainsKey(Ext(fileName)); }
    public static string MimeOf(string fileName) { string m; return Mime.TryGetValue(Ext(fileName), out m) ? m : "application/octet-stream"; }

    /// <summary>Uygulama icinde gosterilebilir tur: image / pdf / text; bos ise yalniz indirilir</summary>
    public static string PreviewKind(string fileName)
    {
        switch (Ext(fileName))
        {
            case ".png": case ".jpg": case ".jpeg": case ".gif": case ".webp": return "image";
            case ".pdf": return "pdf";
            case ".txt": case ".log": case ".csv": case ".json": case ".xml": return "text";
            default: return "";
        }
    }

    public static string Size(long b)
    {
        if (b < 1024) return b + " B";
        if (b < 1024 * 1024) return (b / 1024.0).ToString("0") + " KB";
        return (b / 1024.0 / 1024.0).ToString("0.0", new System.Globalization.CultureInfo("tr-TR")) + " MB";
    }

    public class Info { public int AttachmentId, TicketId, CompanyId; public string StoredName, OriginalName; public long FileSize; public bool Internal; }

    /// <summary>Ek + talebin firmasi + bagli mesaj ic not mu</summary>
    public static Info Get(int attachmentId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT a.AttachmentId, a.TicketId, t.CompanyId, a.StoredName, a.OriginalName, a.FileSize, ISNULL(m.IsInternal, 0) AS Internal " +
            "FROM dbo.TicketAttachments a JOIN dbo.Tickets t ON t.TicketId = a.TicketId " +
            "LEFT JOIN dbo.TicketMessages m ON m.MessageId = a.MessageId WHERE a.AttachmentId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", attachmentId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                Info x = new Info();
                x.AttachmentId = r.GetInt32(0); x.TicketId = r.GetInt32(1); x.CompanyId = r.GetInt32(2);
                x.StoredName = r[3] as string ?? ""; x.OriginalName = r[4] as string ?? ""; x.FileSize = Convert.ToInt64(r[5]); x.Internal = Convert.ToBoolean(r[6]);
                return x;
            }
        }
    }

    /// <summary>Gorme yetkisi: destek ekibi her eki; musteri/bayi yalniz kendi firmasinin talebinde ve ic not olmayan ekleri</summary>
    public static bool CanSee(Info a)
    {
        if (a == null || !Auth.IsLoggedIn) return false;
        if (Auth.IsVendorSide) return true;
        return a.CompanyId == Auth.CompanyId && !a.Internal;
    }
}
