using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

// ===================== MODEL =====================
public class Watchlist
{
    public int ListId, OwnerUserId, OwnerCompanyId;
    public string Name, OwnerName, ShareScope;   // private | company | users
    public bool IsDefault, IsRecommended;
    public string Kind;                          // own | shared | rec (goruntuleyene gore)
    public List<int> Items = new List<int>();
    public List<int> SharedUserIds = new List<int>();
}

// ===================== VERI =====================
// Gorunurluk: kendi listeleri (tam yetki) + ayni firmada paylasilanlar (firma geneli veya kisiye ozel) + editorun onerdikleri.
// Firmalar arasi paylasim yoktur. Paylasilan / onerilen listeler alici icin salt okunurdur (kopyalanabilir).
public static class WatchlistRepo
{
    public const string DefaultName = "Daha Sonra İzle";

    public static List<Watchlist> GetVisible(int userId, int companyId)
    {
        List<Watchlist> list = new List<Watchlist>();
        Dictionary<int, Watchlist> map = new Dictionary<int, Watchlist>();
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT w.ListId, w.OwnerUserId, w.OwnerCompanyId, w.Name, w.IsDefault, w.ShareScope, w.IsRecommended, u.FullName AS OwnerName " +
                "FROM dbo.Watchlists w LEFT JOIN dbo.Users u ON u.UserId = w.OwnerUserId " +
                "WHERE w.OwnerUserId = @u " +
                "   OR (w.OwnerCompanyId = @c AND w.ShareScope = N'company') " +
                "   OR (w.ShareScope = N'users' AND EXISTS (SELECT 1 FROM dbo.WatchlistShares s WHERE s.ListId = w.ListId AND s.UserId = @u)) " +
                "   OR w.IsRecommended = 1 " +
                "ORDER BY CASE WHEN w.OwnerUserId = @u THEN 0 ELSE 1 END, w.IsDefault DESC, w.UpdatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@c", companyId);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        Watchlist w = new Watchlist();
                        w.ListId = (int)r["ListId"]; w.OwnerUserId = (int)r["OwnerUserId"]; w.OwnerCompanyId = (int)r["OwnerCompanyId"];
                        w.Name = r["Name"] as string; w.IsDefault = (bool)r["IsDefault"]; w.ShareScope = r["ShareScope"] as string;
                        w.IsRecommended = (bool)r["IsRecommended"]; w.OwnerName = r["OwnerName"] as string;
                        w.Kind = w.OwnerUserId == userId ? "own" : (w.IsRecommended && !(w.OwnerCompanyId == companyId && w.ShareScope != "private") ? "rec" : "shared");
                        list.Add(w); map[w.ListId] = w;
                    }
            }
            if (list.Count == 0) return list;
            string ids = Ids(list);
            using (SqlCommand cmd = new SqlCommand("SELECT ListId, MaterialId FROM dbo.WatchlistItems WHERE ListId IN (" + ids + ") ORDER BY ListId, SortOrder, ItemId", con))
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read()) { Watchlist w; if (map.TryGetValue((int)r["ListId"], out w)) w.Items.Add((int)r["MaterialId"]); }
            using (SqlCommand cmd = new SqlCommand("SELECT ListId, UserId FROM dbo.WatchlistShares WHERE ListId IN (" + ids + ")", con))
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read()) { Watchlist w; if (map.TryGetValue((int)r["ListId"], out w) && w.Kind == "own") w.SharedUserIds.Add((int)r["UserId"]); }
        }
        return list;
    }

    private static string Ids(List<Watchlist> l)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < l.Count; i++) { if (i > 0) sb.Append(","); sb.Append(l[i].ListId); }   // yalniz int -> enjeksiyon riski yok
        return sb.ToString();
    }

    // "Daha Sonra Izle" listesi yoksa olustur
    public static void EnsureDefault(int userId, int companyId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "IF NOT EXISTS (SELECT 1 FROM dbo.Watchlists WHERE OwnerUserId=@u AND IsDefault=1) " +
            "INSERT INTO dbo.Watchlists (OwnerUserId, OwnerCompanyId, Name, IsDefault) VALUES (@u, @c, @n, 1)", con))
        { cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@n", DefaultName); cmd.ExecuteNonQuery(); }
    }

    public static int Create(int userId, int companyId, string name)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.Watchlists (OwnerUserId, OwnerCompanyId, Name) OUTPUT INSERTED.ListId VALUES (@u, @c, @n)", con))
        { cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@n", name); return (int)cmd.ExecuteScalar(); }
    }

    // Sahiplik denetimli islemler: yalniz liste sahibi degistirebilir
    public static bool IsOwner(int listId, int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Watchlists WHERE ListId=@l AND OwnerUserId=@u", con))
        { cmd.Parameters.AddWithValue("@l", listId); cmd.Parameters.AddWithValue("@u", userId); return (int)cmd.ExecuteScalar() > 0; }
    }

    public static void Rename(int listId, int userId, string name)
    {
        Exec("UPDATE dbo.Watchlists SET Name=@n, UpdatedAt=SYSUTCDATETIME() WHERE ListId=@l AND OwnerUserId=@u AND IsDefault=0", listId, userId, "@n", name);
    }

    public static void Delete(int listId, int userId)
    {
        Exec("DELETE FROM dbo.Watchlists WHERE ListId=@l AND OwnerUserId=@u AND IsDefault=0", listId, userId, null, null);
    }

    public static void SetItem(int listId, int userId, int materialId, bool on)
    {
        if (!IsOwner(listId, userId)) return;
        using (SqlConnection con = Db.Open())
        {
            string sql = on
                ? "IF NOT EXISTS (SELECT 1 FROM dbo.WatchlistItems WHERE ListId=@l AND MaterialId=@m) " +
                  "INSERT INTO dbo.WatchlistItems (ListId, MaterialId, SortOrder) VALUES (@l, @m, (SELECT ISNULL(MAX(SortOrder),0)+1 FROM dbo.WatchlistItems WHERE ListId=@l))"
                : "DELETE FROM dbo.WatchlistItems WHERE ListId=@l AND MaterialId=@m";
            using (SqlCommand cmd = new SqlCommand(sql + "; UPDATE dbo.Watchlists SET UpdatedAt=SYSUTCDATETIME() WHERE ListId=@l", con))
            { cmd.Parameters.AddWithValue("@l", listId); cmd.Parameters.AddWithValue("@m", materialId); cmd.ExecuteNonQuery(); }
        }
    }

    public static void Reorder(int listId, int userId, List<int> mids)
    {
        if (!IsOwner(listId, userId)) return;
        using (SqlConnection con = Db.Open())
            for (int i = 0; i < mids.Count; i++)
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.WatchlistItems SET SortOrder=@s WHERE ListId=@l AND MaterialId=@m", con))
                { cmd.Parameters.AddWithValue("@s", i + 1); cmd.Parameters.AddWithValue("@l", listId); cmd.Parameters.AddWithValue("@m", mids[i]); cmd.ExecuteNonQuery(); }
    }

    // Paylasim: scope private|company|users; userIds yalniz ayni firmadan (cagiran taraf dogrular). Donen: yeni eklenen kisiler.
    public static List<int> SetShare(int listId, int userId, string scope, List<int> userIds, bool? recommended)
    {
        List<int> added = new List<int>();
        if (!IsOwner(listId, userId)) return added;
        if (scope != "company" && scope != "users") scope = "private";
        using (SqlConnection con = Db.Open())
        {
            HashSet<int> before = new HashSet<int>();
            using (SqlCommand cmd = new SqlCommand("SELECT UserId FROM dbo.WatchlistShares WHERE ListId=@l", con))
            {
                cmd.Parameters.AddWithValue("@l", listId);
                using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) before.Add((int)r[0]);
            }
            using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.WatchlistShares WHERE ListId=@l", con))
            { cmd.Parameters.AddWithValue("@l", listId); cmd.ExecuteNonQuery(); }
            if (scope == "users")
                for (int i = 0; i < userIds.Count; i++)
                {
                    if (userIds[i] == userId) continue;
                    using (SqlCommand cmd = new SqlCommand("IF NOT EXISTS (SELECT 1 FROM dbo.WatchlistShares WHERE ListId=@l AND UserId=@u) INSERT INTO dbo.WatchlistShares (ListId, UserId) VALUES (@l, @u)", con))
                    { cmd.Parameters.AddWithValue("@l", listId); cmd.Parameters.AddWithValue("@u", userIds[i]); cmd.ExecuteNonQuery(); }
                    if (!before.Contains(userIds[i]) && !added.Contains(userIds[i])) added.Add(userIds[i]);
                }
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Watchlists SET ShareScope=@s" + (recommended.HasValue ? ", IsRecommended=@r" : "") + ", UpdatedAt=SYSUTCDATETIME() WHERE ListId=@l", con))
            {
                cmd.Parameters.AddWithValue("@s", scope); cmd.Parameters.AddWithValue("@l", listId);
                if (recommended.HasValue) cmd.Parameters.AddWithValue("@r", recommended.Value);
                cmd.ExecuteNonQuery();
            }
        }
        return added;
    }

    // Goruntulenebilen bir listeyi kendi listesine kopyala
    public static int Copy(int listId, int userId, int companyId)
    {
        Watchlist src = null;
        List<Watchlist> vis = GetVisible(userId, companyId);
        for (int i = 0; i < vis.Count; i++) if (vis[i].ListId == listId) src = vis[i];
        if (src == null) return 0;
        string nm = src.Name.Length > 110 ? src.Name.Substring(0, 110) : src.Name;
        int nid = Create(userId, companyId, nm + (src.Kind == "own" ? " (kopya)" : ""));
        for (int i = 0; i < src.Items.Count; i++) SetItem(nid, userId, src.Items[i], true);
        return nid;
    }

    private static void Exec(string sql, int listId, int userId, string p, object v)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@l", listId); cmd.Parameters.AddWithValue("@u", userId);
            if (p != null) cmd.Parameters.AddWithValue(p, v);
            cmd.ExecuteNonQuery();
        }
    }
}
