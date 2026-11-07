using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;

// ============================================================================
// Forum (Faz 1): akis, gonderi (metin + 4 gorsel / 1 video), yorum, begeni, kaydetme,
// Fol duyurusu ve sabitleme, sikayet ve moderasyon, bildirimler.
// Giris yapmis tum kullanicilara acik (CanAccess). Moderasyon yalniz Forum Adminlerinde.
// ============================================================================
public static class ForumRepo
{
    public const int PAGE = 15, MAX_BODY = 4000, MAX_COMMENT = 2000, MAX_IMAGES = 4;
    public const long MAX_IMAGE_BYTES = 5L * 1024 * 1024, MAX_VIDEO_BYTES = 25L * 1024 * 1024;

    /// <summary>Forum yayinda: giris yapmis tum kullanicilar (musteri, bayi, Fol personeli)</summary>
    public static bool CanAccess { get { return Auth.IsLoggedIn; } }

    /// <summary>Gonderi sikligi siniri (forumu doldurmaya karsi): saatte 10 gonderi, 60 yorum/yanit. Forum Adminleri muaf.</summary>
    public const int MAX_POSTS_PER_HOUR = 10, MAX_COMMENTS_PER_HOUR = 60;
    public static string RateLimited(int uid, bool comment, bool mod)
    {
        if (mod) return null;
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(comment
                ? "SELECT COUNT(*) FROM dbo.ForumComments WHERE AuthorUserId = @u AND CreatedAt >= DATEADD(HOUR, -1, SYSUTCDATETIME())"
                : "SELECT COUNT(*) FROM dbo.ForumPosts WHERE AuthorUserId = @u AND CreatedAt >= DATEADD(HOUR, -1, SYSUTCDATETIME())", con))
            {
                cmd.Parameters.AddWithValue("@u", uid);
                int n = Convert.ToInt32(cmd.ExecuteScalar());
                if (comment && n >= MAX_COMMENTS_PER_HOUR) return "Bir saat içinde en fazla " + MAX_COMMENTS_PER_HOUR + " yorum yazılabilir. Lütfen biraz sonra tekrar deneyin.";
                if (!comment && n >= MAX_POSTS_PER_HOUR) return "Bir saat içinde en fazla " + MAX_POSTS_PER_HOUR + " gönderi paylaşılabilir. Lütfen biraz sonra tekrar deneyin.";
            }
        }
        catch { }
        return null;
    }
    /// <summary>Moderator (Forum Admini): Fol personelinden "ForumAdmin" yetkisi olanlar ya da Vendor yoneticisi</summary>
    /// Yetki oturumdaki kopyadan degil veritabanindan okunur (60 sn onbellek): verilen ya da alinan yetki yeniden giris gerekmeden devreye girer.
    public static bool IsModerator { get { return Auth.IsLoggedIn && Auth.IsVendorFirm && LiveAdmin(Auth.UserId); } }
    private static readonly Dictionary<int, KeyValuePair<DateTime, bool>> adminCache = new Dictionary<int, KeyValuePair<DateTime, bool>>();
    private static bool LiveAdmin(int userId)
    {
        lock (adminCache)
        {
            KeyValuePair<DateTime, bool> c;
            if (adminCache.TryGetValue(userId, out c) && (DateTime.UtcNow - c.Key).TotalSeconds < 60) return c.Value;
        }
        bool v;
        try { UserRow u = Repo.GetUserById(userId); v = u != null && IsAdminUser(u.Role, u.Permissions, u.CompanyType); }
        catch { v = Auth.Role == "VendorAdmin" || Auth.HasPerm("ForumAdmin"); }   // veritabanina ulasilamazsa oturumdaki yetki
        lock (adminCache) adminCache[userId] = new KeyValuePair<DateTime, bool>(DateTime.UtcNow, v);
        return v;
    }
    public static bool IsAdminUser(string role, string perms, string companyType)
    {
        if (companyType != "Vendor") return false;
        if (role == "VendorAdmin") return true;
        foreach (string p in (perms ?? "").Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)) if (string.Equals(p, "ForumAdmin", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public static string FilesDir { get { return HttpContext.Current.Server.MapPath("~/forumfiles/"); } }

    // ------------------------------------------------------------------ akis
    private const string POST_SELECT =
        "SELECT {TOP} p.PostId, p.AuthorUserId, p.Body, p.IsAnnouncement, p.IsPinned, p.IsHidden, p.LikeCount, p.CommentCount, p.CreatedAt, p.EditedAt, " +
        "u.FullName, u.AvatarImage, u.Role, u.Permissions, c.Name AS CompanyName, c.CompanyType, " +
        "CASE WHEN l.UserId IS NULL THEN 0 ELSE 1 END AS Liked, CASE WHEN s.UserId IS NULL THEN 0 ELSE 1 END AS Saved, " +
        "lc.CommentId AS LcId, lc.Body AS LcBody, lc.CreatedAt AS LcAt, lc.AuthorUserId AS LcUid, lc.LikeCount AS LcLikes, lc.CLiked AS LcLiked, lc.Replies AS LcReplies, lcu.FullName AS LcName, lcu.AvatarImage AS LcAva, lcu.Role AS LcRole, lcu.Permissions AS LcPerms, lcc.CompanyType AS LcType " +
        "FROM dbo.ForumPosts p JOIN dbo.Users u ON u.UserId = p.AuthorUserId JOIN dbo.Companies c ON c.CompanyId = p.CompanyId " +
        "LEFT JOIN dbo.ForumLikes l ON l.PostId = p.PostId AND l.UserId = @v LEFT JOIN dbo.ForumSaves s ON s.PostId = p.PostId AND s.UserId = @v " +
        "OUTER APPLY (SELECT TOP 1 x.CommentId, x.Body, x.CreatedAt, x.AuthorUserId, x.LikeCount, (SELECT COUNT(*) FROM dbo.ForumComments y WHERE y.ParentCommentId = x.CommentId AND y.DeletedAt IS NULL AND y.IsHidden = 0) AS Replies, " +
        "CASE WHEN EXISTS (SELECT 1 FROM dbo.ForumCommentLikes cl WHERE cl.CommentId = x.CommentId AND cl.UserId = @v) THEN 1 ELSE 0 END AS CLiked " +
        "FROM dbo.ForumComments x WHERE x.PostId = p.PostId AND x.ParentCommentId IS NULL AND x.DeletedAt IS NULL AND (x.IsHidden = 0 OR @mod = 1) ORDER BY x.CommentId DESC) lc " +
        "LEFT JOIN dbo.Users lcu ON lcu.UserId = lc.AuthorUserId LEFT JOIN dbo.Companies lcc ON lcc.CompanyId = lcu.CompanyId " +
        "WHERE p.DeletedAt IS NULL AND (p.IsHidden = 0 OR @mod = 1 OR p.AuthorUserId = @v) ";

    /// <summary>tab: feed / announcements / saved. before: sayfalama (o PostId'den kucukler). Ilk sayfada sabitlenmisler ustte.</summary>
    public static List<Dictionary<string, object>> Feed(int viewer, bool mod, string tab, int before, out bool more) { return Feed(viewer, mod, tab, before, 0, out more); }
    /// <summary>author > 0: yalniz o kisinin gonderileri (profil). Sayfalama PostId uzerinden (anahtar kumesi): her sayfa ilk sayfa kadar hizli.</summary>
    public static List<Dictionary<string, object>> Feed(int viewer, bool mod, string tab, int before, int author, out bool more) { return Feed(viewer, mod, tab, before, author, null, out more); }
    /// <summary>tag: yalniz o etiketi tasiyan gonderiler (Turkce kucuk harfe cevrilmis)</summary>
    public static List<Dictionary<string, object>> Feed(int viewer, bool mod, string tab, int before, int author, string tag, out bool more)
    {
        tag = NormTag(tag);
        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        using (SqlConnection con = Db.Open())
        {
            if (tab == "feed" && before <= 0 && author <= 0 && tag == null)
                list.AddRange(Query(con, POST_SELECT.Replace("{TOP}", "") + "AND p.IsPinned = 1 ORDER BY p.PostId DESC", viewer, mod, 0));
            string where = author > 0 ? "AND p.AuthorUserId = @a " : tag != null ? "AND EXISTS (SELECT 1 FROM dbo.ForumPostTags tg WHERE tg.PostId = p.PostId AND tg.Tag = @tag) "
                         : tab == "announcements" ? "AND p.IsAnnouncement = 1 " : tab == "saved" ? "AND s.UserId IS NOT NULL " : "AND p.IsPinned = 0 ";
            List<Dictionary<string, object>> page = Query(con, POST_SELECT.Replace("{TOP}", "TOP (" + (PAGE + 1) + ")") + where + (before > 0 ? "AND p.PostId < @b " : "") + "ORDER BY p.PostId DESC", viewer, mod, before, author, tag);
            more = page.Count > PAGE; if (more) page.RemoveAt(PAGE);
            list.AddRange(page);
            AttachMedia(con, list);
        }
        return list;
    }

    public static Dictionary<string, object> GetPost(int id, int viewer, bool mod)
    {
        using (SqlConnection con = Db.Open())
        {
            List<Dictionary<string, object>> l = Query(con, POST_SELECT.Replace("{TOP}", "") + "AND p.PostId = @b", viewer, mod, id);
            if (l.Count == 0) return null;
            AttachMedia(con, l);
            return l[0];
        }
    }

    private static List<Dictionary<string, object>> Query(SqlConnection con, string sql, int viewer, bool mod, int b) { return Query(con, sql, viewer, mod, b, 0, null); }
    private static List<Dictionary<string, object>> Query(SqlConnection con, string sql, int viewer, bool mod, int b, int author) { return Query(con, sql, viewer, mod, b, author, null); }
    private static List<Dictionary<string, object>> Query(SqlConnection con, string sql, int viewer, bool mod, int b, int author, string tag)
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@v", viewer); cmd.Parameters.AddWithValue("@mod", mod ? 1 : 0); cmd.Parameters.AddWithValue("@b", b); cmd.Parameters.AddWithValue("@a", author); cmd.Parameters.AddWithValue("@tag", (object)tag ?? DBNull.Value);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    Dictionary<string, object> p = new Dictionary<string, object>();
                    int au = (int)r["AuthorUserId"];
                    p["id"] = (int)r["PostId"];
                    p["author"] = Person(au, r["FullName"] as string, r["AvatarImage"] as string, r["CompanyType"] as string, r["Role"] as string, r["Permissions"] as string);
                    p["body"] = r["Body"] as string ?? "";
                    p["ann"] = (bool)r["IsAnnouncement"]; p["pin"] = (bool)r["IsPinned"]; p["hidden"] = (bool)r["IsHidden"];
                    p["likes"] = (int)r["LikeCount"]; p["coms"] = (int)r["CommentCount"];
                    p["liked"] = Convert.ToInt32(r["Liked"]) == 1; p["saved"] = Convert.ToInt32(r["Saved"]) == 1;
                    p["mine"] = au == viewer; p["at"] = Iso((DateTime)r["CreatedAt"]); p["edited"] = r["EditedAt"] != DBNull.Value;
                    if (r["LcId"] != DBNull.Value)
                    {
                        Dictionary<string, object> c = new Dictionary<string, object>();
                        int cu = (int)r["LcUid"];
                        c["id"] = (int)r["LcId"]; c["body"] = r["LcBody"] as string ?? ""; c["at"] = Iso((DateTime)r["LcAt"]); c["mine"] = cu == viewer;
                        c["author"] = Person(cu, r["LcName"] as string, r["LcAva"] as string, r["LcType"] as string, r["LcRole"] as string, r["LcPerms"] as string);
                        c["parent"] = null; c["likes"] = Convert.ToInt32(r["LcLikes"]); c["liked"] = Convert.ToInt32(r["LcLiked"]) == 1; c["replies"] = Convert.ToInt32(r["LcReplies"]);
                        p["last"] = c;
                    }
                    p["media"] = new List<object>();
                    l.Add(p);
                }
        }
        return l;
    }

    private static void AttachMedia(SqlConnection con, List<Dictionary<string, object>> posts)
    {
        if (posts.Count == 0) return;
        Dictionary<int, Dictionary<string, object>> byId = new Dictionary<int, Dictionary<string, object>>();
        List<string> ids = new List<string>();
        foreach (Dictionary<string, object> p in posts) { int id = (int)p["id"]; if (!byId.ContainsKey(id)) { byId[id] = p; ids.Add(id.ToString()); } }
        using (SqlCommand cmd = new SqlCommand("SELECT MediaId, PostId, Kind, ContentType FROM dbo.ForumMedia WHERE PostId IN (" + string.Join(",", ids.ToArray()) + ") ORDER BY PostId, SortOrder, MediaId", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                Dictionary<string, object> m = new Dictionary<string, object>();
                m["id"] = (int)r["MediaId"]; m["kind"] = (string)r["Kind"]; m["ct"] = (string)r["ContentType"];
                ((List<object>)byId[(int)r["PostId"]]["media"]).Add(m);
            }
    }

    /// <summary>Yazar karti: ad, profil resmi; Fol personeli ise onay isareti, Forum Admini ise Admin etiketi (diger firmalarda kurum bilgisi gosterilmez)</summary>
    private static Dictionary<string, object> Person(int id, string name, string ava, string companyType, string role, string perms)
    {
        Dictionary<string, object> a = Person(id, name, ava, null, null);
        a["fol"] = companyType == "Vendor"; a["admin"] = IsAdminUser(role, perms, companyType);
        return a;
    }
    private static Dictionary<string, object> Person(int id, string name, string ava, string company, string type)
    {
        Dictionary<string, object> a = new Dictionary<string, object>();
        a["id"] = id; a["name"] = name ?? ""; a["ini"] = Fmt.Initials(name ?? ""); a["ava"] = ava ?? "";   // yalniz ad ve profil resmi
        return a;
    }
    private static string Iso(DateTime utc) { return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssZ"); }

    /// <summary>Gonderinin yorumlari (duz liste, CommentId sirali; istemci yanitlari ust yorumun altina dizer).
    /// Yanitlari olan silinmis ya da gizlenmis ust yorum, metni bos bir yer tutucu olarak gelir (yanitlar kaybolmasin).</summary>
    public static List<Dictionary<string, object>> Comments(int postId, int viewer, bool mod)
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT x.CommentId, x.ParentCommentId, x.Body, x.CreatedAt, x.IsHidden, x.DeletedAt, x.LikeCount, x.AuthorUserId, u.FullName, u.AvatarImage, u.Role, u.Permissions, c.CompanyType, " +
            "CASE WHEN cl.UserId IS NULL THEN 0 ELSE 1 END AS Liked FROM dbo.ForumComments x " +
            "JOIN dbo.Users u ON u.UserId = x.AuthorUserId JOIN dbo.Companies c ON c.CompanyId = u.CompanyId " +
            "LEFT JOIN dbo.ForumCommentLikes cl ON cl.CommentId = x.CommentId AND cl.UserId = @v " +
            "WHERE x.PostId = @p AND ( (x.DeletedAt IS NULL AND (x.IsHidden = 0 OR @mod = 1 OR x.AuthorUserId = @v)) " +
            "   OR (x.ParentCommentId IS NULL AND EXISTS (SELECT 1 FROM dbo.ForumComments y WHERE y.ParentCommentId = x.CommentId AND y.DeletedAt IS NULL AND (y.IsHidden = 0 OR @mod = 1 OR y.AuthorUserId = @v))) ) " +
            "ORDER BY x.CommentId", con))
        {
            cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@v", viewer); cmd.Parameters.AddWithValue("@mod", mod ? 1 : 0);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    Dictionary<string, object> c = new Dictionary<string, object>();
                    int au = (int)r["AuthorUserId"];
                    bool deleted = r["DeletedAt"] != DBNull.Value, hidden = (bool)r["IsHidden"];
                    bool placeholder = deleted || (hidden && !mod && au != viewer);
                    c["id"] = (int)r["CommentId"]; c["parent"] = r["ParentCommentId"] as int?;
                    c["body"] = placeholder ? "" : (r["Body"] as string ?? ""); c["at"] = Iso((DateTime)r["CreatedAt"]);
                    c["hidden"] = hidden; c["gone"] = placeholder ? (deleted ? "deleted" : "hidden") : null; c["mine"] = au == viewer && !placeholder;
                    c["likes"] = (int)r["LikeCount"]; c["liked"] = Convert.ToInt32(r["Liked"]) == 1;
                    c["author"] = placeholder ? Person(0, "", "", null, null) : Person(au, r["FullName"] as string, r["AvatarImage"] as string, r["CompanyType"] as string, r["Role"] as string, r["Permissions"] as string);
                    l.Add(c);
                }
        }
        return l;
    }

    // ------------------------------------------------------------------ yazma
    public class MediaIn { public string Kind, StoredName, OriginalName, ContentType; public long Size; }

    public static int CreatePost(int uid, int cid, string body, bool ann, bool pin, List<MediaIn> media)
    {
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                int id;
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ForumPosts (AuthorUserId, CompanyId, Body, IsAnnouncement, IsPinned) OUTPUT INSERTED.PostId VALUES (@u, @c, @b, @a, @p)", con, tx))
                {
                    cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@c", cid); cmd.Parameters.AddWithValue("@b", body);
                    cmd.Parameters.AddWithValue("@a", ann); cmd.Parameters.AddWithValue("@p", pin);
                    id = (int)cmd.ExecuteScalar();
                }
                SaveTags(con, tx, id, body);
                int i = 0;
                foreach (MediaIn m in media)
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ForumMedia (PostId, Kind, StoredName, OriginalName, ContentType, FileSize, SortOrder) VALUES (@p, @k, @s, @o, @t, @z, @i)", con, tx))
                    {
                        cmd.Parameters.AddWithValue("@p", id); cmd.Parameters.AddWithValue("@k", m.Kind); cmd.Parameters.AddWithValue("@s", m.StoredName);
                        cmd.Parameters.AddWithValue("@o", (object)m.OriginalName ?? DBNull.Value); cmd.Parameters.AddWithValue("@t", m.ContentType);
                        cmd.Parameters.AddWithValue("@z", m.Size); cmd.Parameters.AddWithValue("@i", i++);
                        cmd.ExecuteNonQuery();
                    }
                tx.Commit();
                return id;
            }
            catch { tx.Rollback(); throw; }
        }
    }

    /// <summary>Kendi gonderisinin metnini duzenler (medya degismez)</summary>
    public static bool EditPost(int postId, int uid, string body)
    {
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumPosts SET Body = @b, EditedAt = SYSUTCDATETIME() WHERE PostId = @p AND AuthorUserId = @u AND DeletedAt IS NULL AND (Body <> N'' OR EXISTS (SELECT 1 FROM dbo.ForumMedia m WHERE m.PostId = @p) OR @b <> N'')", con))
            { cmd.Parameters.AddWithValue("@b", body); cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@u", uid); if (cmd.ExecuteNonQuery() == 0) return false; }
            try { SaveTags(con, null, postId, body); } catch { }
            return true;
        }
    }

    /// <summary>Profil: ad, profil resmi, uyelik, gonderi ve alinan begeni sayisi; paylasim aciksa (ya da kendi profiliyse) rozetler ve sertifikalar</summary>
    public static Dictionary<string, object> Profile(int userId, int viewer)
    {
        UserRow u = Repo.GetUserById(userId);
        if (u == null) return null;
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["user"] = Person(userId, u.FullName, u.AvatarImage, u.CompanyType, u.Role, u.Permissions);
        d["me"] = userId == viewer;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*), ISNULL(SUM(LikeCount), 0), MIN(CreatedAt) FROM dbo.ForumPosts WHERE AuthorUserId = @u AND DeletedAt IS NULL AND IsHidden = 0", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read()) { d["posts"] = r.GetInt32(0); d["likes"] = Convert.ToInt32(r[1]); d["since"] = r.IsDBNull(2) ? null : Iso(r.GetDateTime(2)); }
        }
        bool share = userId == viewer; try { share = share || LearningPath.GetShare(userId); } catch { }
        d["shared"] = share;
        List<object> badges = new List<object>(), certs = new List<object>();
        if (share)
        {
            try
            {
                List<PathStep> steps = LearningPath.GetSteps(userId, u.CompanyId);
                foreach (EarnedBadge b in LearningPath.GetBadges(userId, steps))
                {
                    int no = 1; for (int i = 0; i < steps.Count; i++) if ("video:" + steps[i].MaterialId == b.Key) no = i + 1;
                    badges.Add(new { key = b.Key, title = b.Title, at = Iso(b.EarnedAt), svg = LearningPath.BadgeSvg(b.Key, no, true, 52) });
                }
            }
            catch { }
            try
            {
                foreach (CertificateInfo c in ExamRepo.GetUserCertificates(userId))
                    if (!c.IsRevoked && (!c.ValidUntil.HasValue || c.ValidUntil.Value > DateTime.UtcNow))
                        certs.Add(new { title = c.ExamTitle, no = c.CertNo, at = Iso(c.IssuedAt), color = c.BadgeColor ?? "", verify = c.VerifyCode ?? "" });
            }
            catch { }
        }
        d["badges"] = badges; d["certs"] = certs;
        return d;
    }

    /// <summary>Gonderi sahibi ve durumu (silinmemis); yoksa null</summary>
    private static int? PostOwner(SqlConnection con, int postId)
    {
        using (SqlCommand cmd = new SqlCommand("SELECT AuthorUserId FROM dbo.ForumPosts WHERE PostId = @p AND DeletedAt IS NULL", con))
        { cmd.Parameters.AddWithValue("@p", postId); return cmd.ExecuteScalar() as int?; }
    }

    public static int AddComment(int postId, int uid, string body, string authorName) { return AddComment(postId, uid, body, authorName, null); }
    /// <summary>Yorum ya da yanit. Yanitin yanitina verilen yanit ayni diziye (ust yoruma) baglanir: tek seviye.
    /// Bildirim: gonderi sahibine "yorum yapti", yanitlanan yorumun sahibine "yanit verdi" (ayni kisiye iki kez gitmez).</summary>
    public static int AddComment(int postId, int uid, string body, string authorName, int? parentId)
    {
        int id; int? owner, parentOwner = null;
        using (SqlConnection con = Db.Open())
        {
            owner = PostOwner(con, postId);
            if (!owner.HasValue) return 0;
            if (parentId.HasValue)
            {
                int? top = null;
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(ParentCommentId, CommentId), AuthorUserId FROM dbo.ForumComments WHERE CommentId = @c AND PostId = @p", con))
                {
                    cmd.Parameters.AddWithValue("@c", parentId.Value); cmd.Parameters.AddWithValue("@p", postId);
                    using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) { top = r.GetInt32(0); parentOwner = r.GetInt32(1); }
                }
                if (!top.HasValue) return -1;   // yanitlanan yorum bu gonderide yok
                parentId = top;
            }
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ForumComments (PostId, AuthorUserId, Body, ParentCommentId) OUTPUT INSERTED.CommentId VALUES (@p, @u, @b, @pr)", con))
            {
                cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@b", body);
                cmd.Parameters.AddWithValue("@pr", (object)parentId ?? DBNull.Value); id = (int)cmd.ExecuteScalar();
            }
            RecountComments(con, postId);
        }
        if (parentOwner.HasValue && parentOwner.Value != uid)
            Notify(parentOwner.Value, postId, Short(authorName) + " yorumunuza yanıt verdi", Short(authorName) + " replied to your comment", Cut(body, 140), "ForumReply");
        if (owner.Value != uid && owner.Value != parentOwner)
            Notify(owner.Value, postId, Short(authorName) + " gönderinize yorum yaptı", Short(authorName) + " commented on your post", Cut(body, 140), "ForumComment");
        return id;
    }

    /// <summary>Yorumu begen / begeniyi geri al. Sahibine toplu bildirim ("N kisi yorumunuzu begendi").</summary>
    public static bool ToggleCommentLike(int commentId, int uid, string name, out int count)
    {
        bool liked; int owner, postId;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("SELECT AuthorUserId, PostId FROM dbo.ForumComments WHERE CommentId = @c AND DeletedAt IS NULL", con))
            {
                cmd.Parameters.AddWithValue("@c", commentId);
                using (SqlDataReader r = cmd.ExecuteReader()) { if (!r.Read()) { count = 0; return false; } owner = r.GetInt32(0); postId = r.GetInt32(1); }
            }
            using (SqlCommand cmd = new SqlCommand(
                "IF EXISTS (SELECT 1 FROM dbo.ForumCommentLikes WHERE CommentId = @c AND UserId = @u) BEGIN DELETE FROM dbo.ForumCommentLikes WHERE CommentId = @c AND UserId = @u; SELECT 0; END " +
                "ELSE BEGIN INSERT INTO dbo.ForumCommentLikes (CommentId, UserId) VALUES (@c, @u); SELECT 1; END", con))
            { cmd.Parameters.AddWithValue("@c", commentId); cmd.Parameters.AddWithValue("@u", uid); liked = Convert.ToInt32(cmd.ExecuteScalar()) == 1; }
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumComments SET LikeCount = (SELECT COUNT(*) FROM dbo.ForumCommentLikes WHERE CommentId = @c) OUTPUT INSERTED.LikeCount WHERE CommentId = @c", con))
            { cmd.Parameters.AddWithValue("@c", commentId); count = (int)cmd.ExecuteScalar(); }
        }
        if (liked && owner != uid) NotifyLikes(owner, postId, name, count, "ForumCommentLike", "yorumunuzu", "your comment", "&c=" + commentId);
        return liked;
    }

    private static void RecountComments(SqlConnection con, int postId)
    {
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumPosts SET CommentCount = (SELECT COUNT(*) FROM dbo.ForumComments WHERE PostId = @p AND DeletedAt IS NULL AND IsHidden = 0) WHERE PostId = @p", con))
        { cmd.Parameters.AddWithValue("@p", postId); cmd.ExecuteNonQuery(); }
    }

    public static bool DeleteComment(int commentId, int uid, bool mod) { return DeleteComment(commentId, uid, mod, null, null); }
    /// <summary>Yorum silme. Moderator baskasinin yorumunu silerse aciklama sahibine bildirim olarak gider. Her silme kayda girer.</summary>
    public static bool DeleteComment(int commentId, int uid, bool mod, string reason, string modName)
    {
        using (SqlConnection con = Db.Open())
        {
            int postId; int? owner; string ownerName, body; int media;
            int pidLookup = 0;
            using (SqlCommand c0 = new SqlCommand("SELECT PostId FROM dbo.ForumComments WHERE CommentId = @c", con)) { c0.Parameters.AddWithValue("@c", commentId); object o0 = c0.ExecuteScalar(); if (o0 != null) pidLookup = (int)o0; }
            Snapshot(con, pidLookup, commentId, out owner, out ownerName, out body, out media);
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumComments SET DeletedAt = SYSUTCDATETIME() OUTPUT INSERTED.PostId WHERE CommentId = @c AND DeletedAt IS NULL AND (AuthorUserId = @u OR @mod = 1)", con))
            {
                cmd.Parameters.AddWithValue("@c", commentId); cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@mod", mod ? 1 : 0);
                object o = cmd.ExecuteScalar(); if (o == null) return false; postId = (int)o;
            }
            RecountComments(con, postId);
            Log("delete_comment", postId, commentId, owner, ownerName, body, 0, uid, modName ?? Auth.FullName, reason);
            if (owner.HasValue && owner.Value != uid)
                Notify(owner.Value, postId, "Yorumunuz bir forum admini tarafından kaldırıldı", "Your comment was removed by a forum admin",
                       (string.IsNullOrEmpty(reason) ? "" : "Açıklama: " + reason + " · ") + "Yorum: " + Cut(body, 120), "ForumModeration");
            return true;
        }
    }

    public static bool DeletePost(int postId, int uid, bool mod) { return DeletePost(postId, uid, mod, null, null); }
    /// <summary>Gonderi silme. Moderator baskasinin gonderisini silerse aciklama sahibine bildirim olarak gider. Her silme kayda girer.</summary>
    public static bool DeletePost(int postId, int uid, bool mod, string reason, string modName)
    {
        int? owner; string ownerName, body; int media;
        using (SqlConnection con = Db.Open())
        {
            Snapshot(con, postId, null, out owner, out ownerName, out body, out media);
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumPosts SET DeletedAt = SYSUTCDATETIME(), IsPinned = 0 WHERE PostId = @p AND DeletedAt IS NULL AND (AuthorUserId = @u OR @mod = 1)", con))
            { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@mod", mod ? 1 : 0); if (cmd.ExecuteNonQuery() == 0) return false; }
        }
        Log("delete_post", postId, null, owner, ownerName, body, media, uid, modName ?? Auth.FullName, reason);
        if (owner.HasValue && owner.Value != uid)
            Notify(owner.Value, 0, "Gönderiniz bir forum admini tarafından kaldırıldı", "Your post was removed by a forum admin",
                   (string.IsNullOrEmpty(reason) ? "" : "Açıklama: " + reason + " · ") + "Gönderi: " + Cut(string.IsNullOrEmpty(body) ? (media > 0 ? "(" + media + " görsel/video)" : "") : body, 120), "ForumModeration");
        return true;
    }

    /// <summary>Begen / begeniyi geri al. Doner: yeni durum ve sayi. Sahibine toplu bildirim ("N kisi begendi").</summary>
    public static bool ToggleLike(int postId, int uid, string name, out int count)
    {
        bool liked; int? owner;
        using (SqlConnection con = Db.Open())
        {
            owner = PostOwner(con, postId);
            if (!owner.HasValue) { count = 0; return false; }
            using (SqlCommand cmd = new SqlCommand(
                "IF EXISTS (SELECT 1 FROM dbo.ForumLikes WHERE PostId = @p AND UserId = @u) BEGIN DELETE FROM dbo.ForumLikes WHERE PostId = @p AND UserId = @u; SELECT 0; END " +
                "ELSE BEGIN INSERT INTO dbo.ForumLikes (PostId, UserId) VALUES (@p, @u); SELECT 1; END", con))
            { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@u", uid); liked = Convert.ToInt32(cmd.ExecuteScalar()) == 1; }
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumPosts SET LikeCount = (SELECT COUNT(*) FROM dbo.ForumLikes WHERE PostId = @p) OUTPUT INSERTED.LikeCount WHERE PostId = @p", con))
            { cmd.Parameters.AddWithValue("@p", postId); count = (int)cmd.ExecuteScalar(); }
        }
        if (liked && owner.Value != uid)
            NotifyLikes(owner.Value, postId, name, count);
        return liked;
    }

    public static bool ToggleSave(int postId, int uid)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "IF EXISTS (SELECT 1 FROM dbo.ForumSaves WHERE PostId = @p AND UserId = @u) BEGIN DELETE FROM dbo.ForumSaves WHERE PostId = @p AND UserId = @u; SELECT 0; END " +
            "ELSE BEGIN INSERT INTO dbo.ForumSaves (PostId, UserId) VALUES (@p, @u); SELECT 1; END", con))
        { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@u", uid); return Convert.ToInt32(cmd.ExecuteScalar()) == 1; }
    }

    public static void SetPinned(int postId, bool pin)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumPosts SET IsPinned = @v WHERE PostId = @p AND DeletedAt IS NULL", con))
        { cmd.Parameters.AddWithValue("@v", pin); cmd.Parameters.AddWithValue("@p", postId); cmd.ExecuteNonQuery(); }
    }

    // ------------------------------------------------------------------ sikayet ve moderasyon
    /// <summary>Sikayet: ayni kisi ayni icerigi bir kez bildirir (tekrar bildirmek hata degil)</summary>
    public static void Report(int postId, int? commentId, int uid, string reason)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "IF NOT EXISTS (SELECT 1 FROM dbo.ForumReports WHERE ReporterUserId = @u AND PostId = @p AND ((CommentId IS NULL AND @c IS NULL) OR CommentId = @c)) " +
            "INSERT INTO dbo.ForumReports (PostId, CommentId, ReporterUserId, Reason) VALUES (@p, @c, @u, @r)", con))
        {
            cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@r", (object)reason ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }

    public static int OpenReportCount()
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT CAST(PostId AS NVARCHAR(12)) + ':' + ISNULL(CAST(CommentId AS NVARCHAR(12)), '')) FROM dbo.ForumReports WHERE Status = N'Open'", con))
            return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>Acik sikayetler: icerik basina tek satir (kac kisi bildirdi, son neden)</summary>
    public static List<Dictionary<string, object>> OpenReports()
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT r.PostId, r.CommentId, COUNT(*) AS Cnt, MAX(r.CreatedAt) AS LastAt, MAX(r.Reason) AS Reason, " +
            "MAX(CASE WHEN r.CommentId IS NULL THEN p.Body ELSE x.Body END) AS Body, MAX(au.FullName) AS AuthorName " +
            "FROM dbo.ForumReports r JOIN dbo.ForumPosts p ON p.PostId = r.PostId LEFT JOIN dbo.ForumComments x ON x.CommentId = r.CommentId " +
            "LEFT JOIN dbo.Users au ON au.UserId = CASE WHEN r.CommentId IS NULL THEN p.AuthorUserId ELSE x.AuthorUserId END " +
            "WHERE r.Status = N'Open' GROUP BY r.PostId, r.CommentId ORDER BY MAX(r.CreatedAt) DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["post"] = (int)r["PostId"]; d["comment"] = r["CommentId"] as int?; d["count"] = (int)r["Cnt"];
                d["at"] = Iso((DateTime)r["LastAt"]); d["reason"] = r["Reason"] as string ?? ""; d["body"] = Cut(r["Body"] as string ?? "", 280); d["author"] = r["AuthorName"] as string ?? "";
                l.Add(d);
            }
        return l;
    }

    /// <summary>Moderasyon karari: hide = icerigi gizle (sahibine bildirim), dismiss = sikayeti yoksay. Ayni icerigin tum acik sikayetleri kapanir.</summary>
    public static void Resolve(int postId, int? commentId, bool hide, int modId) { Resolve(postId, commentId, hide, modId, null, null); }
    public static void Resolve(int postId, int? commentId, bool hide, int modId, string reason, string modName)
    {
        int? owner = null; int? snapOwner; string snapName, snapBody; int snapMedia;
        using (SqlConnection con = Db.Open())
        {
            Snapshot(con, postId, commentId, out snapOwner, out snapName, out snapBody, out snapMedia);
            if (hide)
            {
                string sql = commentId.HasValue ? "UPDATE dbo.ForumComments SET IsHidden = 1 OUTPUT INSERTED.AuthorUserId WHERE CommentId = @c"
                                                : "UPDATE dbo.ForumPosts SET IsHidden = 1, IsPinned = 0 OUTPUT INSERTED.AuthorUserId WHERE PostId = @p";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value); owner = cmd.ExecuteScalar() as int?; }
                if (commentId.HasValue) RecountComments(con, postId);
            }
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.ForumReports SET Status = @s, ResolvedByUserId = @m, ResolvedAt = SYSUTCDATETIME() WHERE Status = N'Open' AND PostId = @p AND ((CommentId IS NULL AND @c IS NULL) OR CommentId = @c)", con))
            {
                cmd.Parameters.AddWithValue("@s", hide ? "Hidden" : "Dismissed"); cmd.Parameters.AddWithValue("@m", modId);
                cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
        Log(hide ? (commentId.HasValue ? "hide_comment" : "hide_post") : "dismiss", postId, commentId, snapOwner, snapName, snapBody, snapMedia, modId, modName ?? Auth.FullName, reason);
        if (hide && owner.HasValue)
            Notify(owner.Value, postId, (commentId.HasValue ? "Yorumunuz" : "Gönderiniz") + " topluluk kuralları gereği gizlendi", (commentId.HasValue ? "Your comment" : "Your post") + " was hidden under the community guidelines",
                   string.IsNullOrEmpty(reason) ? "Sorunuz varsa Fol Yazılım destek ekibine yazabilirsiniz." : "Açıklama: " + reason, "ForumModeration");
    }

    /// <summary>Moderator: gizlemeyi kaldir</summary>
    public static void Unhide(int postId, int? commentId) { Unhide(postId, commentId, 0, null); }
    public static void Unhide(int postId, int? commentId, int modId, string modName)
    {
        using (SqlConnection con = Db.Open())
        {
            int? o; string on, b; int m; Snapshot(con, postId, commentId, out o, out on, out b, out m);
            if (modId > 0) Log(commentId.HasValue ? "unhide_comment" : "unhide_post", postId, commentId, o, on, b, m, modId, modName, null);
            string sql = commentId.HasValue ? "UPDATE dbo.ForumComments SET IsHidden = 0 WHERE CommentId = @c" : "UPDATE dbo.ForumPosts SET IsHidden = 0 WHERE PostId = @p";
            using (SqlCommand cmd = new SqlCommand(sql, con)) { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value); cmd.ExecuteNonQuery(); }
            if (commentId.HasValue) RecountComments(con, postId);
        }
    }

    // ------------------------------------------------------------------ forum ayarlari (sessize alma)
    private static DateTime settingsAt = DateTime.MinValue; private static bool closed; private static string closedNote;
    private static readonly object settingsLock = new object();
    /// <summary>Forum yeni paylasimlara kapali mi (30 sn onbellek)</summary>
    public static bool IsClosed(out string note)
    {
        lock (settingsLock)
        {
            if ((DateTime.UtcNow - settingsAt).TotalSeconds > 30)
            {
                closed = false; closedNote = null;
                try
                {
                    using (SqlConnection con = Db.Open())
                    using (SqlCommand cmd = new SqlCommand("SELECT SettingKey, Value FROM dbo.ForumSettings WHERE SettingKey IN (N'Closed', N'ClosedNote')", con))
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read()) { if ((string)r[0] == "Closed") closed = (r[1] as string) == "1"; else closedNote = r[1] as string; }
                }
                catch { }
                settingsAt = DateTime.UtcNow;
            }
            note = closedNote; return closed;
        }
    }
    public static void SetClosed(bool on, string note, int modId, string modName)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "MERGE dbo.ForumSettings AS t USING (VALUES (N'Closed', @v), (N'ClosedNote', @n)) AS s(k, v) ON t.SettingKey = s.k " +
            "WHEN MATCHED THEN UPDATE SET Value = s.v, UpdatedByUserId = @u, UpdatedAt = SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT (SettingKey, Value, UpdatedByUserId) VALUES (s.k, s.v, @u);", con))
        { cmd.Parameters.AddWithValue("@v", on ? "1" : "0"); cmd.Parameters.AddWithValue("@n", (object)note ?? DBNull.Value); cmd.Parameters.AddWithValue("@u", modId); cmd.ExecuteNonQuery(); }
        lock (settingsLock) settingsAt = DateTime.MinValue;
        Log(on ? "close" : "open", null, null, null, null, null, 0, modId, modName, note);
    }

    // ------------------------------------------------------------------ kisitlamalar
    /// <summary>Kisinin etkin kisitlamasi (yoksa null): bitis (null = suresiz) ve gerekce</summary>
    public static Dictionary<string, object> ActiveRestriction(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 RestrictionId, Until, Reason FROM dbo.ForumRestrictions WHERE UserId = @u AND LiftedAt IS NULL AND (Until IS NULL OR Until > SYSUTCDATETIME()) ORDER BY RestrictionId DESC", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["id"] = r.GetInt32(0); d["until"] = r.IsDBNull(1) ? null : Iso(r.GetDateTime(1)); d["reason"] = r[2] as string ?? "";
                return d;
            }
        }
    }
    /// <summary>Paylasim yapabilir mi: forum kapaliysa ya da kisi kisitliysa nedenini doner (moderatorler her zaman yazabilir)</summary>
    public static string CannotPost(int userId, bool mod)
    {
        if (!HasAccepted(userId)) return "Paylaşım yapmadan önce topluluk kurallarını okuyup onaylayın.";
        if (mod) return null;
        string note; if (IsClosed(out note)) return "Forum şu an yeni paylaşımlara kapalı." + (string.IsNullOrEmpty(note) ? "" : " " + note);
        Dictionary<string, object> r = ActiveRestriction(userId);
        if (r != null) return "Forumda paylaşım hakkınız " + (r["until"] == null ? "süresiz olarak" : DateTime.Parse((string)r["until"]).ToLocalTime().ToString("dd.MM.yyyy HH:mm") + " tarihine kadar") + " kısıtlandı.";
        return null;
    }
    public static void Restrict(int userId, int days, string reason, int modId, string modName)
    {
        UserRow u = Repo.GetUserById(userId); if (u == null) throw new InvalidOperationException("Kullanıcı bulunamadı.");
        DateTime? until = days > 0 ? (DateTime?)DateTime.UtcNow.AddDays(days) : null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.ForumRestrictions SET LiftedAt = SYSUTCDATETIME(), LiftedByUserId = @m WHERE UserId = @u AND LiftedAt IS NULL; " +
            "INSERT INTO dbo.ForumRestrictions (UserId, Until, Reason, CreatedByUserId) VALUES (@u, @t, @r, @m);", con))
        {
            cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@t", (object)until ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@r", (object)reason ?? DBNull.Value); cmd.Parameters.AddWithValue("@m", modId);
            cmd.ExecuteNonQuery();
        }
        string span = days > 0 ? days + " gün" : "süresiz";
        Log("restrict", null, null, userId, u.FullName, null, 0, modId, modName, (days > 0 ? days + " gün" : "Süresiz") + (string.IsNullOrEmpty(reason) ? "" : " · " + reason));
        Notify(userId, 0, "Forumda paylaşım hakkınız kısıtlandı (" + span + ")", "Your forum posting was restricted (" + (days > 0 ? days + " days" : "indefinitely") + ")",
               string.IsNullOrEmpty(reason) ? "Forumu okumaya devam edebilirsiniz; gönderi ve yorum yazamazsınız." : "Açıklama: " + reason, "ForumModeration");
    }
    public static void Unrestrict(int restrictionId, int modId, string modName)
    {
        int? uid = null; string name = null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ForumRestrictions SET LiftedAt = SYSUTCDATETIME(), LiftedByUserId = @m OUTPUT INSERTED.UserId WHERE RestrictionId = @r AND LiftedAt IS NULL", con))
        { cmd.Parameters.AddWithValue("@r", restrictionId); cmd.Parameters.AddWithValue("@m", modId); uid = cmd.ExecuteScalar() as int?; }
        if (!uid.HasValue) return;
        UserRow u = Repo.GetUserById(uid.Value); if (u != null) name = u.FullName;
        Log("unrestrict", null, null, uid, name, null, 0, modId, modName, null);
        Notify(uid.Value, 0, "Forumda paylaşım kısıtlamanız kaldırıldı", "Your forum restriction was lifted", "Yeniden gönderi ve yorum yazabilirsiniz.", "ForumModeration");
    }
    public static List<Dictionary<string, object>> Restrictions()
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT r.RestrictionId, r.UserId, u.FullName, r.Until, r.Reason, r.CreatedAt, m.FullName AS ModName FROM dbo.ForumRestrictions r JOIN dbo.Users u ON u.UserId = r.UserId LEFT JOIN dbo.Users m ON m.UserId = r.CreatedByUserId " +
            "WHERE r.LiftedAt IS NULL AND (r.Until IS NULL OR r.Until > SYSUTCDATETIME()) ORDER BY r.RestrictionId DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["id"] = r.GetInt32(0); d["uid"] = r.GetInt32(1); d["name"] = r[2] as string ?? ""; d["until"] = r.IsDBNull(3) ? null : Iso(r.GetDateTime(3));
                d["reason"] = r[4] as string ?? ""; d["at"] = Iso(r.GetDateTime(5)); d["by"] = r[6] as string ?? "";
                l.Add(d);
            }
        return l;
    }

    // ------------------------------------------------------------------ moderasyon kaydi
    /// <summary>Kalici kayit: icerik ve kisi adlari kayit aninda saklanir (icerik silinse de okunur)</summary>
    private static void Log(string action, int? postId, int? commentId, int? ownerId, string ownerName, string snapshot, int media, int modId, string modName, string reason)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.ForumModLog (Action, PostId, CommentId, OwnerUserId, OwnerName, ContentSnapshot, MediaCount, ModeratorUserId, ModeratorName, Reason) VALUES (@a, @p, @c, @o, @on, @s, @mc, @m, @mn, @r)", con))
            {
                cmd.Parameters.AddWithValue("@a", action); cmd.Parameters.AddWithValue("@p", (object)postId ?? DBNull.Value); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@o", (object)ownerId ?? DBNull.Value); cmd.Parameters.AddWithValue("@on", (object)Cut(ownerName, 200) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@s", (object)Cut(snapshot, 4000) ?? DBNull.Value); cmd.Parameters.AddWithValue("@mc", media);
                cmd.Parameters.AddWithValue("@m", modId); cmd.Parameters.AddWithValue("@mn", (object)Cut(modName, 200) ?? DBNull.Value); cmd.Parameters.AddWithValue("@r", (object)Cut(reason, 1000) ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
        catch { }
    }
    /// <summary>Kayit (en yeni once), anahtar kumesi sayfalama</summary>
    public static List<Dictionary<string, object>> ModLog(int before, out bool more)
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TOP (31) l.*, " +
            "CASE WHEN l.PostId IS NULL THEN NULL " +
            "     WHEN l.CommentId IS NOT NULL THEN (CASE WHEN c.CommentId IS NULL THEN 'purged' WHEN c.DeletedAt IS NOT NULL THEN 'deleted' WHEN c.IsHidden = 1 THEN 'hidden' ELSE 'live' END) " +
            "     ELSE (CASE WHEN p.PostId IS NULL THEN 'purged' WHEN p.DeletedAt IS NOT NULL THEN 'deleted' WHEN p.IsHidden = 1 THEN 'hidden' ELSE 'live' END) END AS State " +
            "FROM dbo.ForumModLog l LEFT JOIN dbo.ForumPosts p ON p.PostId = l.PostId LEFT JOIN dbo.ForumComments c ON c.CommentId = l.CommentId " +
            (before > 0 ? "WHERE l.LogId < @b " : "") + "ORDER BY l.LogId DESC", con))
        {
            cmd.Parameters.AddWithValue("@b", before);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["id"] = (int)r["LogId"]; d["action"] = (string)r["Action"]; d["post"] = r["PostId"] as int?; d["comment"] = r["CommentId"] as int?;
                    d["owner"] = r["OwnerName"] as string ?? ""; d["content"] = r["ContentSnapshot"] as string ?? ""; d["media"] = (int)r["MediaCount"];
                    d["mod"] = r["ModeratorName"] as string ?? ""; d["reason"] = r["Reason"] as string ?? ""; d["at"] = Iso((DateTime)r["CreatedAt"]);
                    d["self"] = r["OwnerUserId"] != DBNull.Value && (int)r["OwnerUserId"] == (int)r["ModeratorUserId"];
                    d["state"] = r["State"] as string;
                    l.Add(d);
                }
        }
        more = l.Count > 30; if (more) l.RemoveAt(30);
        return l;
    }
    // ------------------------------------------------------------------ moderator: onizleme, geri yukleme, kalici silme
    /// <summary>Moderator onizlemesi: silinmis ya da gizlenmis olsa da gonderi (medyasiyla) ve istenirse tek yorum</summary>
    public static Dictionary<string, object> ModView(int postId, int? commentId, int viewer)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        using (SqlConnection con = Db.Open())
        {
            List<Dictionary<string, object>> l = Query(con, POST_SELECT.Replace("{TOP}", "").Replace("WHERE p.DeletedAt IS NULL AND (p.IsHidden = 0 OR @mod = 1 OR p.AuthorUserId = @v) ", "WHERE 1 = 1 ") + "AND p.PostId = @b", viewer, true, postId);
            if (l.Count == 0) return null;
            AttachMedia(con, l);
            using (SqlCommand cmd = new SqlCommand("SELECT DeletedAt FROM dbo.ForumPosts WHERE PostId = @p", con))
            { cmd.Parameters.AddWithValue("@p", postId); object o = cmd.ExecuteScalar(); l[0]["deleted"] = o != null && o != DBNull.Value; }
            d["post"] = l[0];
            if (commentId.HasValue)
                using (SqlCommand cmd = new SqlCommand("SELECT x.Body, x.CreatedAt, x.DeletedAt, x.IsHidden, u.FullName FROM dbo.ForumComments x LEFT JOIN dbo.Users u ON u.UserId = x.AuthorUserId WHERE x.CommentId = @c AND x.PostId = @p", con))
                {
                    cmd.Parameters.AddWithValue("@c", commentId.Value); cmd.Parameters.AddWithValue("@p", postId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                        if (r.Read()) d["comment"] = new { body = r[0] as string ?? "", at = Iso(r.GetDateTime(1)), deleted = !r.IsDBNull(2), hidden = r.GetBoolean(3), author = r[4] as string ?? "" };
                }
        }
        return d;
    }

    /// <summary>Silinmis ya da gizlenmis icerigi yeniden yayina alir; sahibine bildirim, kayda girer</summary>
    public static bool Restore(int postId, int? commentId, int modId, string modName)
    {
        int? owner; string ownerName, body; int media;
        using (SqlConnection con = Db.Open())
        {
            string sql = commentId.HasValue
                ? "UPDATE dbo.ForumComments SET DeletedAt = NULL, IsHidden = 0 OUTPUT INSERTED.AuthorUserId WHERE CommentId = @c AND PostId = @p AND (DeletedAt IS NOT NULL OR IsHidden = 1)"
                : "UPDATE dbo.ForumPosts SET DeletedAt = NULL, IsHidden = 0 OUTPUT INSERTED.AuthorUserId WHERE PostId = @p AND (DeletedAt IS NOT NULL OR IsHidden = 1)";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value); if (cmd.ExecuteScalar() == null) return false; }
            if (commentId.HasValue) RecountComments(con, postId);
            Snapshot(con, postId, commentId, out owner, out ownerName, out body, out media);
        }
        Log(commentId.HasValue ? "restore_comment" : "restore_post", postId, commentId, owner, ownerName, body, media, modId, modName, null);
        if (owner.HasValue && owner.Value != modId)
            Notify(owner.Value, postId, (commentId.HasValue ? "Yorumunuz" : "Gönderiniz") + " yeniden yayına alındı", (commentId.HasValue ? "Your comment" : "Your post") + " was restored", "Bir forum admini içeriğinizi yeniden yayına aldı.", "ForumModeration");
        return true;
    }

    /// <summary>Kalici silme: icerik, medya dosyalari, yorumlar, begeniler, kaydedilmeler, sikayetler tamamen yok edilir.
    /// Geriye yalniz kayitta metin olarak bir satir kalir (icerigin metni, silinen dosya sayisi, kim, aciklama).</summary>
    public static bool Purge(int postId, int? commentId, int modId, string modName, string reason)
    {
        int? owner = null; string ownerName = null, body = null; int media = 0; List<string> files = new List<string>();
        using (SqlConnection con = Db.Open())
        {
            // kalici silmeden once metin (silinmis icerik dahil)
            string snap = commentId.HasValue
                ? "SELECT x.AuthorUserId, u.FullName, x.Body FROM dbo.ForumComments x LEFT JOIN dbo.Users u ON u.UserId = x.AuthorUserId WHERE x.CommentId = @c AND x.PostId = @p"
                : "SELECT p.AuthorUserId, u.FullName, p.Body FROM dbo.ForumPosts p LEFT JOIN dbo.Users u ON u.UserId = p.AuthorUserId WHERE p.PostId = @p";
            using (SqlCommand cmd = new SqlCommand(snap, con))
            {
                cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value);
                using (SqlDataReader r = cmd.ExecuteReader()) { if (!r.Read()) return false; owner = r.GetInt32(0); ownerName = r[1] as string; body = r[2] as string; }
            }
            if (!commentId.HasValue)
                using (SqlCommand cmd = new SqlCommand("SELECT StoredName FROM dbo.ForumMedia WHERE PostId = @p", con))
                { cmd.Parameters.AddWithValue("@p", postId); using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) files.Add(r.GetString(0)); }
            media = files.Count;
            using (SqlTransaction tx = con.BeginTransaction())
            {
                try
                {
                    string sql = commentId.HasValue
                        // yorum ve (ust yorumsa) yanitlari
                        ? "DECLARE @ids TABLE (Id INT); INSERT INTO @ids SELECT CommentId FROM dbo.ForumComments WHERE CommentId = @c OR ParentCommentId = @c; " +
                          "DELETE FROM dbo.ForumCommentLikes WHERE CommentId IN (SELECT Id FROM @ids); DELETE FROM dbo.ForumReports WHERE CommentId IN (SELECT Id FROM @ids); " +
                          "DELETE FROM dbo.ForumComments WHERE CommentId IN (SELECT Id FROM @ids);"
                        : "DELETE FROM dbo.ForumCommentLikes WHERE CommentId IN (SELECT CommentId FROM dbo.ForumComments WHERE PostId = @p); " +
                          "DELETE FROM dbo.ForumReports WHERE PostId = @p; DELETE FROM dbo.ForumComments WHERE PostId = @p; DELETE FROM dbo.ForumLikes WHERE PostId = @p; " +
                          "DELETE FROM dbo.ForumSaves WHERE PostId = @p; DELETE FROM dbo.ForumMedia WHERE PostId = @p; IF OBJECT_ID('dbo.ForumPostTags', 'U') IS NOT NULL DELETE FROM dbo.ForumPostTags WHERE PostId = @p; DELETE FROM dbo.ForumPosts WHERE PostId = @p;";
                    using (SqlCommand cmd = new SqlCommand(sql, con, tx))
                    { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value); cmd.ExecuteNonQuery(); }
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
            if (commentId.HasValue) RecountComments(con, postId);
        }
        foreach (string fn in files) try { File.Delete(Path.Combine(FilesDir, fn)); } catch { }   // dosyalar diskten de silinir
        Log(commentId.HasValue ? "purge_comment" : "purge_post", postId, commentId, owner, ownerName, body, media, modId, modName,
            (media > 0 ? media + " medya dosyası silindi" + (string.IsNullOrEmpty(reason) ? "" : " · ") : "") + (reason ?? ""));
        return true;
    }

    /// <summary>Icerigin o anki metni, sahibi ve gorsel sayisi (kayit icin)</summary>
    private static void Snapshot(SqlConnection con, int postId, int? commentId, out int? owner, out string ownerName, out string body, out int media)
    {
        owner = null; ownerName = null; body = null; media = 0;
        string sql = commentId.HasValue
            ? "SELECT x.AuthorUserId, u.FullName, x.Body, 0 FROM dbo.ForumComments x LEFT JOIN dbo.Users u ON u.UserId = x.AuthorUserId WHERE x.CommentId = @c AND x.DeletedAt IS NULL"
            : "SELECT p.AuthorUserId, u.FullName, p.Body, (SELECT COUNT(*) FROM dbo.ForumMedia m WHERE m.PostId = p.PostId) FROM dbo.ForumPosts p LEFT JOIN dbo.Users u ON u.UserId = p.AuthorUserId WHERE p.PostId = @p AND p.DeletedAt IS NULL";
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@c", (object)commentId ?? DBNull.Value);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) { owner = r.GetInt32(0); ownerName = r[1] as string; body = r[2] as string; media = Convert.ToInt32(r[3]); }
        }
    }

    // ------------------------------------------------------------------ topluluk kurallari onayi
    /// <summary>Kurallar degistiginde artirin: herkesten yeniden onay istenir</summary>
    public const string RULES_VERSION = "2026-10";
    /// <summary>Kisi gecerli surumdeki kurallari onayladi mi (oturumda saklanir; onaylayana kadar her istekte bakilir)</summary>
    public static bool HasAccepted(int uid)
    {
        HttpContext c = HttpContext.Current;
        if (c != null && c.Session != null && (c.Session["frmRules"] as string) == RULES_VERSION) return true;
        bool ok = false;
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ForumAgreements WHERE UserId = @u AND RulesVersion = @v", con))
            { cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@v", RULES_VERSION); ok = Convert.ToInt32(cmd.ExecuteScalar()) > 0; }
        }
        catch { return true; }   // tablo yoksa (migration 50 calismadiysa) kimse engellenmez
        if (ok && c != null && c.Session != null) c.Session["frmRules"] = RULES_VERSION;
        return ok;
    }
    public static void Accept(int uid, string ip)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("IF NOT EXISTS (SELECT 1 FROM dbo.ForumAgreements WHERE UserId = @u AND RulesVersion = @v) INSERT INTO dbo.ForumAgreements (UserId, RulesVersion, Ip) VALUES (@u, @v, @ip)", con))
        { cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@v", RULES_VERSION); cmd.Parameters.AddWithValue("@ip", (object)Cut(ip, 64) ?? DBNull.Value); cmd.ExecuteNonQuery(); }
        HttpContext c = HttpContext.Current; if (c != null && c.Session != null) c.Session["frmRules"] = RULES_VERSION;
    }

    // ------------------------------------------------------------------ etiketler
    private static readonly System.Globalization.CultureInfo TRC = new System.Globalization.CultureInfo("tr-TR");
    private static readonly System.Text.RegularExpressions.Regex TAG_RX = new System.Text.RegularExpressions.Regex(@"(?<=^|[\s(>])#([0-9A-Za-zÇĞİÖŞÜçğıöşü_]{2,30})");
    /// <summary>Etiket anahtari: Turkce kurallariyla kucuk harf ("#AKIŞ" = "#akış")</summary>
    public static string NormTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        tag = tag.Trim().TrimStart('#'); if (tag.Length < 2 || tag.Length > 30) return null;
        return tag.ToLower(TRC);
    }
    public static List<KeyValuePair<string, string>> ParseTags(string body)
    {
        List<KeyValuePair<string, string>> l = new List<KeyValuePair<string, string>>(); HashSet<string> seen = new HashSet<string>();
        foreach (System.Text.RegularExpressions.Match m in TAG_RX.Matches(body ?? ""))
        {
            string disp = m.Groups[1].Value, key = disp.ToLower(TRC);
            if (seen.Add(key) && l.Count < 10) l.Add(new KeyValuePair<string, string>(key, disp));
        }
        return l;
    }
    private static void SaveTags(SqlConnection con, SqlTransaction tx, int postId, string body)
    {
        using (SqlCommand cmd = new SqlCommand("IF OBJECT_ID('dbo.ForumPostTags', 'U') IS NOT NULL DELETE FROM dbo.ForumPostTags WHERE PostId = @p", con, tx))
        { cmd.Parameters.AddWithValue("@p", postId); cmd.ExecuteNonQuery(); }
        foreach (KeyValuePair<string, string> t in ParseTags(body))
            using (SqlCommand cmd = new SqlCommand("IF OBJECT_ID('dbo.ForumPostTags', 'U') IS NOT NULL INSERT INTO dbo.ForumPostTags (PostId, Tag, Display) VALUES (@p, @t, @d)", con, tx))
            { cmd.Parameters.AddWithValue("@p", postId); cmd.Parameters.AddWithValue("@t", t.Key); cmd.Parameters.AddWithValue("@d", t.Value); cmd.ExecuteNonQuery(); }
    }
    private static bool tagsBackfilled;
    /// <summary>Etiket tablosundan once yazilmis gonderiler: ilk cagrida bir kez islenir</summary>
    private static void EnsureTagBackfill(SqlConnection con)
    {
        if (tagsBackfilled) return;
        List<KeyValuePair<int, string>> todo = new List<KeyValuePair<int, string>>();
        using (SqlCommand cmd = new SqlCommand("SELECT p.PostId, p.Body FROM dbo.ForumPosts p WHERE p.Body LIKE N'%#%' AND NOT EXISTS (SELECT 1 FROM dbo.ForumPostTags t WHERE t.PostId = p.PostId)", con))
        using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) todo.Add(new KeyValuePair<int, string>(r.GetInt32(0), r[1] as string ?? ""));
        foreach (KeyValuePair<int, string> p in todo) SaveTags(con, null, p.Key, p.Value);
        tagsBackfilled = true;
    }
    /// <summary>Gundemdekiler: son N gunde en cok kullanilan etiketler (silinmis ve gizli gonderiler haric)</summary>
    public static List<Dictionary<string, object>> Trending(int days, int take)
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        using (SqlConnection con = Db.Open())
        {
            EnsureTagBackfill(con);
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP (@n) t.Tag, (SELECT TOP 1 x.Display FROM dbo.ForumPostTags x WHERE x.Tag = t.Tag ORDER BY x.PostId DESC) AS Display, COUNT(*) AS Cnt " +   // en son yazildigi hali
                "FROM dbo.ForumPostTags t JOIN dbo.ForumPosts p ON p.PostId = t.PostId " +
                "WHERE p.DeletedAt IS NULL AND p.IsHidden = 0 AND p.CreatedAt >= DATEADD(DAY, -@d, SYSUTCDATETIME()) GROUP BY t.Tag ORDER BY COUNT(*) DESC, MAX(t.PostId) DESC", con))
            {
                cmd.Parameters.AddWithValue("@n", take); cmd.Parameters.AddWithValue("@d", days);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) { Dictionary<string, object> d = new Dictionary<string, object>(); d["tag"] = r.GetString(0); d["display"] = r.GetString(1); d["count"] = r.GetInt32(2); l.Add(d); }
            }
        }
        return l;
    }

    // ------------------------------------------------------------------ okunmamis gonderiler (menu sayaci) ve ana sayfa akislari
    /// <summary>Kisinin son gordugu gonderi numarasi. Ilk kez: mevcut en son gonderi (yeni kullanici eski gonderileri "yeni" saymaz).</summary>
    private static int SeenId(SqlConnection con, int uid)
    {
        using (SqlCommand cmd = new SqlCommand(
            "IF NOT EXISTS (SELECT 1 FROM dbo.ForumSeen WHERE UserId = @u) INSERT INTO dbo.ForumSeen (UserId, LastPostId) SELECT @u, ISNULL(MAX(PostId), 0) FROM dbo.ForumPosts; " +
            "SELECT LastPostId FROM dbo.ForumSeen WHERE UserId = @u", con))
        { cmd.Parameters.AddWithValue("@u", uid); return Convert.ToInt32(cmd.ExecuteScalar()); }
    }
    /// <summary>Son ziyaretten sonra paylasilan ana gonderi sayisi (kendi, silinmis ve gizli gonderiler haric). Birincil anahtar uzerinden sayilir.</summary>
    public static int UnreadCount(int uid)
    {
        using (SqlConnection con = Db.Open())
        {
            int seen = SeenId(con, uid);
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ForumPosts WHERE PostId > @s AND DeletedAt IS NULL AND IsHidden = 0 AND AuthorUserId <> @u", con))
            { cmd.Parameters.AddWithValue("@s", seen); cmd.Parameters.AddWithValue("@u", uid); return Convert.ToInt32(cmd.ExecuteScalar()); }
        }
    }
    /// <summary>Menu sayaci: oturumda 30 sn onbellek (her sayfa acilisinda veritabanina gidilmez)</summary>
    public static int UnreadCountCached()
    {
        if (!CanAccess) return 0;
        HttpContext c = HttpContext.Current; if (c == null || c.Session == null) return 0;
        object at = c.Session["frmUnreadAt"], v = c.Session["frmUnread"];
        if (at is DateTime && v is int && (DateTime.UtcNow - (DateTime)at).TotalSeconds < 30) return (int)v;
        int n = 0; try { n = UnreadCount(Auth.UserId); } catch { }
        c.Session["frmUnread"] = n; c.Session["frmUnreadAt"] = DateTime.UtcNow;
        return n;
    }
    /// <summary>Foruma girildi: sayac sifirlanir</summary>
    public static void MarkSeen(int uid)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "DECLARE @m INT = (SELECT ISNULL(MAX(PostId), 0) FROM dbo.ForumPosts); " +
                "UPDATE dbo.ForumSeen SET LastPostId = @m, SeenAt = SYSUTCDATETIME() WHERE UserId = @u; IF @@ROWCOUNT = 0 INSERT INTO dbo.ForumSeen (UserId, LastPostId) VALUES (@u, @m);", con))
            { cmd.Parameters.AddWithValue("@u", uid); cmd.ExecuteNonQuery(); }
        }
        catch { }
        HttpContext c = HttpContext.Current; if (c != null && c.Session != null) { c.Session["frmUnread"] = 0; c.Session["frmUnreadAt"] = DateTime.UtcNow; }
    }
    public class RecentPost { public int Id; public string Author, Body; public DateTime AtUtc; public bool IsNew, HasMedia; }
    /// <summary>Ana sayfa akislari icin son ana gonderiler (son 14 gun, kendi/silinmis/gizli haric). Son ziyaretten sonrakiler "yeni".</summary>
    public static List<RecentPost> Recent(int uid, int take)
    {
        List<RecentPost> l = new List<RecentPost>();
        using (SqlConnection con = Db.Open())
        {
            int seen = SeenId(con, uid);
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP (@n) p.PostId, u.FullName, p.Body, p.CreatedAt, CASE WHEN EXISTS (SELECT 1 FROM dbo.ForumMedia m WHERE m.PostId = p.PostId) THEN 1 ELSE 0 END " +
                "FROM dbo.ForumPosts p JOIN dbo.Users u ON u.UserId = p.AuthorUserId " +
                "WHERE p.DeletedAt IS NULL AND p.IsHidden = 0 AND p.AuthorUserId <> @u AND p.CreatedAt >= DATEADD(DAY, -14, SYSUTCDATETIME()) ORDER BY p.PostId DESC", con))
            {
                cmd.Parameters.AddWithValue("@n", take); cmd.Parameters.AddWithValue("@u", uid);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        RecentPost x = new RecentPost(); x.Id = r.GetInt32(0); x.Author = r[1] as string ?? ""; x.Body = r[2] as string ?? "";
                        x.AtUtc = r.GetDateTime(3); x.HasMedia = Convert.ToInt32(r[4]) == 1; x.IsNew = x.Id > seen; l.Add(x);
                    }
            }
        }
        return l;
    }
    /// <summary>Akista gosterilecek kisa metin: bosluklar sadelesir, baglantilar kisalir</summary>
    public static string Excerpt(RecentPost p, int n)
    {
        string s = System.Text.RegularExpressions.Regex.Replace(p.Body ?? "", @"https?://\S+", "🔗");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim();
        if (s.Length == 0) s = p.HasMedia ? "Görsel / video paylaştı" : "";
        return s.Length > n ? s.Substring(0, n - 1) + "…" : s;
    }

    // ------------------------------------------------------------------ dosyalar
    /// <summary>Medya kaydi (sunum icin): yol, tur, gorunurluk</summary>
    public static bool MediaFor(int mediaId, int viewer, bool mod, out string path, out string contentType, out string kind)
    {
        path = null; contentType = null; kind = null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT m.StoredName, m.ContentType, m.Kind FROM dbo.ForumMedia m JOIN dbo.ForumPosts p ON p.PostId = m.PostId " +
            "WHERE m.MediaId = @m AND ((p.DeletedAt IS NULL AND (p.IsHidden = 0 OR p.AuthorUserId = @v)) OR @mod = 1)", con))
        {
            cmd.Parameters.AddWithValue("@m", mediaId); cmd.Parameters.AddWithValue("@v", viewer); cmd.Parameters.AddWithValue("@mod", mod ? 1 : 0);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return false;
                string name = (string)r[0];
                if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains("..")) return false;
                path = Path.Combine(FilesDir, name); contentType = (string)r[1]; kind = (string)r[2];
                return File.Exists(path);
            }
        }
    }

    /// <summary>Dosya iceriginin turuyle uyumu (uzanti degil, ilk baytlar): jpg / png / gif / webp / mp4 / webm</summary>
    public static string Sniff(byte[] h, int n)
    {
        if (n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return "image/jpeg";
        if (n >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return "image/png";
        if (n >= 6 && h[0] == 0x47 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x38) return "image/gif";
        if (n >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) return "image/webp";
        if (n >= 12 && h[4] == 0x66 && h[5] == 0x74 && h[6] == 0x79 && h[7] == 0x70) return "video/mp4";
        if (n >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3) return "video/webm";
        return null;
    }

    // ------------------------------------------------------------------ bildirimler
    private static void Notify(int userId, int postId, string tr, string en, string body, string type)
    {
        try
        {
            UserRow u = Repo.GetUserById(userId); if (u == null) return;
            Repo.AddUserNotification(u.CompanyId, userId, Cut(tr, 200), Cut(en, 200), Cut(body ?? "", 1000), Cut(body ?? "", 1000), type, postId > 0 ? "Forum.aspx?post=" + postId : "Forum.aspx");
        }
        catch { }
    }
    /// <summary>Begeni bildirimi tek satirda toplanir: okunmamis bildirim varsa guncellenir ("Ayse ve 3 kisi daha begendi")</summary>
    private static void NotifyLikes(int owner, int postId, string name, int count) { NotifyLikes(owner, postId, name, count, "ForumLike", "gönderinizi", "your post", ""); }
    private static void NotifyLikes(int owner, int postId, string name, int count, string type, string whatTr, string whatEn, string linkSuffix)
    {
        try
        {
            UserRow u = Repo.GetUserById(owner); if (u == null) return;
            string tr = count > 1 ? Short(name) + " ve " + (count - 1) + " kişi daha " + whatTr + " beğendi" : Short(name) + " " + whatTr + " beğendi";
            string en = count > 1 ? Short(name) + " and " + (count - 1) + " others liked " + whatEn : Short(name) + " liked " + whatEn;
            string link = "Forum.aspx?post=" + postId + linkSuffix;
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.Notifications SET TitleTr = @t, TitleEn = @e, CreatedAt = SYSUTCDATETIME() WHERE UserId = @u AND Type = @ty AND Link = @l AND IsRead = 0; " +
                "IF @@ROWCOUNT = 0 INSERT INTO dbo.Notifications (CompanyId, UserId, TitleTr, TitleEn, BodyTr, BodyEn, Type, Link) VALUES (@c, @u, @t, @e, N'', N'', @ty, @l);", con))
            {
                cmd.Parameters.AddWithValue("@t", Cut(tr, 200)); cmd.Parameters.AddWithValue("@e", Cut(en, 200)); cmd.Parameters.AddWithValue("@u", owner);
                cmd.Parameters.AddWithValue("@l", link); cmd.Parameters.AddWithValue("@c", u.CompanyId); cmd.Parameters.AddWithValue("@ty", type);
                cmd.ExecuteNonQuery();
            }
        }
        catch { }
    }

    private static string Short(string name) { return string.IsNullOrEmpty(name) ? "Bir kullanıcı" : name.Split(' ')[0]; }
    public static string Cut(string s, int n) { if (s == null) return null; return s.Length > n ? s.Substring(0, n) : s; }
}
