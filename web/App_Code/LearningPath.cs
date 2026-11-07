using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

// ============================================================================
// "Sifirdan Uzmanliga" ogrenme yolu + rozetler
// - Adim bitti = %90 izlendi VE hizli gecilmedi (gercek izleme suresi video suresinin yarisindan az degil).
//   Kurulumdan ONCE tamamlanmis ve izleme kaydi olmayan videolar iyi niyetle sayilir.
// - Kilit yalniz bu yolda: kategorilerden her video serbest; tamamlanan video yolda da sayilir.
// - Rozetler: video:<MaterialId>, m:first (Ilk Adim), m:half (Yari Yol), path:done (Sifirdan Uzmanliga)
// - Bildirim: kisiye anlik; firmadaki diger kisilere (paylasim aciksa) gunluk birlesik; yol bitince portal yoneticilerine.
// ============================================================================
public class PathStep
{
    public int StepId, MaterialId, SortOrder, Percent, DurationSeconds, LastPosition, WatchLogSeconds;
    public string Title, YoutubeId, Group;
    public bool Started, Completed, Skimmed, Qualified, Earned;
    public DateTime? EarnedAt;
    public string State = "locked";   // done / next / locked
}
public class EarnedBadge { public string Key, Title; public DateTime EarnedAt; public bool IsRetro; }

public static class LearningPath
{
    public const string PATH_NAME = "Sıfırdan Uzmanlığa";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    private static DateTime InstalledUtc(SqlConnection con)
    {
        using (SqlCommand cmd = new SqlCommand("SELECT SettingValue FROM dbo.AcademySettings WHERE SettingKey = N'PathInstalledAtUtc'", con))
        {
            object o = cmd.ExecuteScalar(); DateTime d;
            if (o != null && DateTime.TryParse(Convert.ToString(o), CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
        }
        return DateTime.UtcNow;
    }

    /// <summary>Kullanicinin yoldaki adimlari (sirali) ve durumlari</summary>
    public static List<PathStep> GetSteps(int userId, int companyId)
    {
        List<PathStep> list = new List<PathStep>();
        using (SqlConnection con = Db.Open())
        {
            DateTime inst = InstalledUtc(con);
            using (SqlCommand cmd = new SqlCommand(
                "SELECT s.StepId, s.MaterialId, s.SortOrder, m.TitleTr, m.YoutubeId, ISNULL(NULLIF(m.CategoryGroup,N''), N'Diğer Eğitimler') AS Grp, " +
                "p.ProgressId, ISNULL(p.[Percent],0) AS Pct, ISNULL(p.Completed,0) AS Comp, ISNULL(p.DurationSeconds,0) AS Dur, ISNULL(p.LastPosition,0) AS Pos, p.LastWatchedAt, " +
                "ISNULL(w.LogSec,0) AS LogSec, b.EarnedAt " +
                "FROM dbo.LearningPathSteps s JOIN dbo.TrainingMaterials m ON m.MaterialId = s.MaterialId " +
                "LEFT JOIN dbo.TrainingProgress p ON p.MaterialId = s.MaterialId AND p.UserId = @u " +
                "OUTER APPLY (SELECT SUM(l.Seconds) AS LogSec FROM dbo.TrainingWatchLog l WHERE l.UserId = @u AND l.MaterialId = s.MaterialId) w " +
                "LEFT JOIN dbo.UserBadges b ON b.UserId = @u AND b.BadgeKey = N'video:' + CAST(s.MaterialId AS NVARCHAR(12)) " +
                "WHERE s.IsActive = 1 AND m.IsActive = 1 AND m.MaterialType = N'Video' AND (m.CompanyId IS NULL OR m.CompanyId = @c) " +
                "ORDER BY s.SortOrder, s.StepId", con))
            {
                cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@c", companyId);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        PathStep x = new PathStep();
                        x.StepId = (int)r["StepId"]; x.MaterialId = (int)r["MaterialId"]; x.SortOrder = (int)r["SortOrder"];
                        x.Title = r["TitleTr"] as string; x.YoutubeId = (r["YoutubeId"] as string) ?? ""; x.Group = r["Grp"] as string;
                        x.Started = r["ProgressId"] != DBNull.Value;
                        x.Percent = Math.Min(100, Convert.ToInt32(r["Pct"]));
                        x.Completed = Convert.ToBoolean(r["Comp"]) || x.Percent >= ExamRepo.VideoCompletePercent;
                        x.DurationSeconds = Convert.ToInt32(r["Dur"]); x.LastPosition = Convert.ToInt32(r["Pos"]);
                        x.WatchLogSeconds = Convert.ToInt32(r["LogSec"]);
                        DateTime? lw = r["LastWatchedAt"] as DateTime?;
                        bool legacyNoLog = x.WatchLogSeconds == 0 && lw.HasValue && lw.Value < inst;   // kurulum oncesi, kayit yok: iyi niyet
                        x.Skimmed = x.Completed && x.DurationSeconds > 120 && x.WatchLogSeconds < x.DurationSeconds / 2 && !legacyNoLog;
                        x.Qualified = x.Completed && !x.Skimmed;
                        x.EarnedAt = r["EarnedAt"] as DateTime?; x.Earned = x.EarnedAt.HasValue;
                        list.Add(x);
                    }
            }
        }
        bool nextSet = false;
        foreach (PathStep x in list)
        {
            if (x.Qualified || x.Earned) x.State = "done";
            else if (!nextSet) { x.State = "next"; nextSet = true; }
            else x.State = "locked";
        }
        return list;
    }

    public static string BadgeTitle(string key, List<PathStep> steps)
    {
        if (key == "m:first") return "İlk Adım";
        if (key == "m:half") return "Yarı Yol";
        if (key == "path:done") return PATH_NAME;
        if (key.StartsWith("video:") && steps != null) foreach (PathStep s in steps) if ("video:" + s.MaterialId == key) return s.Title;
        return key;
    }

    /// <summary>Hak edilen rozetleri verir. notify=false: gecmise donuk, sessiz (kutlama/bildirim yok).</summary>
    public static List<EarnedBadge> Evaluate(int userId, int companyId, bool notify)
    {
        List<EarnedBadge> fresh = new List<EarnedBadge>();
        List<PathStep> steps = GetSteps(userId, companyId);
        if (steps.Count == 0) return fresh;
        List<string> keys = new List<string>(); Dictionary<string, int?> mids = new Dictionary<string, int?>();
        int done = 0;
        foreach (PathStep s in steps)
        {
            if (s.Qualified || s.Earned) done++;
            if (s.Qualified && !s.Earned) { keys.Add("video:" + s.MaterialId); mids["video:" + s.MaterialId] = s.MaterialId; }
        }
        if (done >= 1) { keys.Add("m:first"); mids["m:first"] = null; }
        if (steps.Count >= 2 && done >= (steps.Count + 1) / 2) { keys.Add("m:half"); mids["m:half"] = null; }
        if (done == steps.Count) { keys.Add("path:done"); mids["path:done"] = null; }

        using (SqlConnection con = Db.Open())
            foreach (string k in keys)
            {
                using (SqlCommand cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT 1 FROM dbo.UserBadges WHERE UserId = @u AND BadgeKey = @k) " +
                    "BEGIN INSERT INTO dbo.UserBadges (UserId, BadgeKey, MaterialId, Celebrated, IsRetro) VALUES (@u, @k, @m, @cel, @retro); SELECT 1; END ELSE SELECT 0;", con))
                {
                    cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@k", k);
                    cmd.Parameters.AddWithValue("@m", (object)mids[k] ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cel", !notify); cmd.Parameters.AddWithValue("@retro", !notify);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                    {
                        EarnedBadge b = new EarnedBadge(); b.Key = k; b.Title = BadgeTitle(k, steps); b.EarnedAt = DateTime.UtcNow; b.IsRetro = !notify;
                        fresh.Add(b);
                    }
                }
            }
        if (notify && fresh.Count > 0) { try { Notify(userId, companyId, fresh, steps); } catch { } }
        return fresh;
    }

    /// <summary>Izleme kaydindan sonra: video yoldaysa rozetleri degerlendir (bildirimli)</summary>
    public static void OnProgress(int userId, int companyId, int materialId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.LearningPathSteps WHERE MaterialId = @m AND IsActive = 1", con))
        { cmd.Parameters.AddWithValue("@m", materialId); if (Convert.ToInt32(cmd.ExecuteScalar()) == 0) return; }
        EnsureRetro(userId, companyId);     // ilk kez: eski tamamlamalar sessizce
        Evaluate(userId, companyId, true);
    }

    /// <summary>Gecmise donuk rozetler: kisi Akademi'yi ilk actiginda BIR KEZ, bildirimsiz</summary>
    public static void EnsureRetro(int userId, int companyId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.UserBadges WHERE UserId = @u AND BadgeKey = N'sys:init'", con))
        { cmd.Parameters.AddWithValue("@u", userId); if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return; }
        Evaluate(userId, companyId, false);
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("IF NOT EXISTS (SELECT 1 FROM dbo.UserBadges WHERE UserId = @u AND BadgeKey = N'sys:init') INSERT INTO dbo.UserBadges (UserId, BadgeKey, Celebrated, IsRetro) VALUES (@u, N'sys:init', 1, 1)", con))
        { cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
    }

    /// <summary>Kazanilan rozetler (sys:* haric)</summary>
    public static List<EarnedBadge> GetBadges(int userId, List<PathStep> steps)
    {
        List<EarnedBadge> list = new List<EarnedBadge>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT BadgeKey, EarnedAt, IsRetro FROM dbo.UserBadges WHERE UserId = @u AND BadgeKey NOT LIKE N'sys:%' ORDER BY EarnedAt", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read()) { EarnedBadge b = new EarnedBadge(); b.Key = (string)r[0]; b.EarnedAt = (DateTime)r[1]; b.IsRetro = (bool)r[2]; b.Title = BadgeTitle(b.Key, steps); list.Add(b); }
        }
        return list;
    }

    /// <summary>Kutlamasi henuz gosterilmemis rozetler; okununca kutlandi isaretlenir</summary>
    public static List<EarnedBadge> TakeUncelebrated(int userId, int companyId)
    {
        List<PathStep> steps = GetSteps(userId, companyId);
        List<EarnedBadge> list = new List<EarnedBadge>();
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("SELECT BadgeKey, EarnedAt FROM dbo.UserBadges WHERE UserId = @u AND Celebrated = 0 AND BadgeKey NOT LIKE N'sys:%' ORDER BY EarnedAt", con))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) { EarnedBadge b = new EarnedBadge(); b.Key = (string)r[0]; b.EarnedAt = (DateTime)r[1]; b.Title = BadgeTitle(b.Key, steps); list.Add(b); }
            }
            if (list.Count > 0)
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.UserBadges SET Celebrated = 1 WHERE UserId = @u AND Celebrated = 0", con))
                { cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
        }
        return list;
    }

    // ------------------------------------------------------------------ bildirimler
    private static void Notify(int userId, int companyId, List<EarnedBadge> fresh, List<PathStep> steps)
    {
        string name = "", email = "", comp = ""; bool share = true;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT u.FullName, u.Email, ISNULL(u.ShareAchievements, 1), c.Name FROM dbo.Users u LEFT JOIN dbo.Companies c ON c.CompanyId = u.CompanyId WHERE u.UserId = @u", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) { name = r[0] as string ?? ""; email = r[1] as string ?? ""; share = Convert.ToBoolean(r[2]); comp = r[3] as string ?? ""; }
        }
        // 1) kisiye: her yeni rozet icin kutlama
        foreach (EarnedBadge b in fresh)
        {
            string t, te;
            if (b.Key == "path:done") { t = "🏆 Tebrikler! " + PATH_NAME + " yolunu tamamladınız"; te = "🏆 Congratulations! You completed the " + PATH_NAME + " path"; }
            else if (b.Key.StartsWith("m:")) { t = "🎖️ Yeni rozet: " + b.Title; te = "🎖️ New badge: " + b.Title; }
            else { t = "🎉 Tebrikler! \"" + b.Title + "\" rozetini kazandınız"; te = "🎉 Congratulations! You earned the \"" + b.Title + "\" badge"; }
            Repo.AddUserNotification(companyId, userId, Cut(t, 200), Cut(te, 200), PATH_NAME + " yolunda bir adım daha ilerlediniz.", "One more step on the " + PATH_NAME + " path.", "Achievement", "Training.aspx?path=1");
        }
        // 2) firmadaki diger kisilere: gunluk birlesik (paylasim aciksa)
        bool anyVideo = false; foreach (EarnedBadge b in fresh) if (b.Key.StartsWith("video:") || b.Key == "path:done") anyVideo = true;
        if (share && anyVideo) CompanyDigest(userId, companyId, name, steps);
        // 3) yol bitti: portal yoneticilerine
        foreach (EarnedBadge b in fresh)
            if (b.Key == "path:done")
            {
                foreach (int[] a in VendorAdmins())
                    Repo.AddUserNotification(a[1], a[0], Cut("🏆 " + name + " · " + PATH_NAME + " yolunu tamamladı", 200), Cut("🏆 " + name + " completed the " + PATH_NAME + " path", 200),
                        Cut((comp.Length > 0 ? comp + " · " : "") + email, 1000), Cut((comp.Length > 0 ? comp + " · " : "") + email, 1000), "Achievement", "UserTraining.aspx?id=" + userId);
            }
    }

    /// <summary>Ayni gun icindeki tamamlamalar tek bildirimde: her is arkadasi icin bugunku bildirim varsa guncellenir</summary>
    private static void CompanyDigest(int userId, int companyId, string name, List<PathStep> steps)
    {
        List<string> today = new List<string>(); bool pathDone = false;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT BadgeKey FROM dbo.UserBadges WHERE UserId = @u AND IsRetro = 0 AND (BadgeKey LIKE N'video:%' OR BadgeKey = N'path:done') " +
                "AND CAST(DATEADD(HOUR, 3, EarnedAt) AS DATE) = CAST(DATEADD(HOUR, 3, SYSUTCDATETIME()) AS DATE) ORDER BY EarnedAt", con))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) { string k = (string)r[0]; if (k == "path:done") pathDone = true; else today.Add(BadgeTitle(k, steps)); }
            }
            if (today.Count == 0 && !pathDone) return;
            string title = pathDone ? "🏆 " + name + " " + PATH_NAME + " yolunu tamamladı!"
                         : "🎉 " + name + (today.Count == 1 ? " bugün bir eğitim videosunu tamamladı" : " bugün " + today.Count + " eğitim videosunu tamamladı");
            string titleEn = pathDone ? "🏆 " + name + " completed the " + PATH_NAME + " path!"
                         : "🎉 " + name + (today.Count == 1 ? " completed a training video today" : " completed " + today.Count + " training videos today");
            string body = string.Join(" · ", today.ToArray());
            string link = "Training.aspx?ach=" + userId;
            List<int> mates = new List<int>();
            using (SqlCommand cmd = new SqlCommand("SELECT UserId FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 AND UserId <> @u", con))
            {
                cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId);
                using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) mates.Add(r.GetInt32(0));
            }
            foreach (int m in mates)
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE dbo.Notifications SET TitleTr = @t, TitleEn = @te, BodyTr = @b, BodyEn = @b, IsRead = 0, ReadAt = NULL, CreatedAt = SYSUTCDATETIME() " +
                    "WHERE UserId = @m AND Type = N'Achievement' AND Link = @l AND CAST(DATEADD(HOUR, 3, CreatedAt) AS DATE) = CAST(DATEADD(HOUR, 3, SYSUTCDATETIME()) AS DATE); " +
                    "IF @@ROWCOUNT = 0 INSERT INTO dbo.Notifications (CompanyId, UserId, TitleTr, TitleEn, BodyTr, BodyEn, Type, Link) VALUES (@c, @m, @t, @te, @b, @b, N'Achievement', @l);", con))
                {
                    cmd.Parameters.AddWithValue("@t", Cut(title, 200)); cmd.Parameters.AddWithValue("@te", Cut(titleEn, 200)); cmd.Parameters.AddWithValue("@b", Cut(body, 1000));
                    cmd.Parameters.AddWithValue("@m", m); cmd.Parameters.AddWithValue("@l", link); cmd.Parameters.AddWithValue("@c", companyId);
                    cmd.ExecuteNonQuery();
                }
        }
    }

    /// <summary>Portal yoneticileri: [UserId, CompanyId]</summary>
    private static List<int[]> VendorAdmins()
    {
        List<int[]> list = new List<int[]>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT u.UserId, u.CompanyId FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId " +
                                               "WHERE u.IsActive = 1 AND c.CompanyType = N'Vendor' AND (u.Role = N'VendorAdmin' OR u.Permissions LIKE N'%Portal%')", con))
        using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(new[] { r.GetInt32(0), r.GetInt32(1) });
        return list;
    }


    // ------------------------------------------------------------------ rozet gorselleri (SVG)
    // Video rozeti: altigen + adim numarasi, sertifika paleti renkleri. Kazanilmamis: seffaf, kesik cizgi.
    private static readonly string[][] PAL = {
        new[] { "#FBE7AE", "#C9A04E", "#7A5A22" }, new[] { "#B0D0FF", "#2E6DE0", "#122F78" }, new[] { "#9AF5CE", "#1CA273", "#08503A" },
        new[] { "#E3C8FF", "#8A3FD1", "#3E1370" }, new[] { "#FFA3B5", "#C8173D", "#5E0819" }, new[] { "#FCDCCD", "#D98C77", "#8E4E3F" },
        new[] { "#F4CC9C", "#B4703A", "#5E3413" }, new[] { "#F4F6FA", "#9BA4B4", "#525B6B" } };
    private static int seq = 0;
    public static string BadgeSvg(string key, int stepNo, bool earned, int size)
    {
        string id = "lb" + (++seq) + "_" + stepNo;
        string[] p = PAL[Math.Max(0, stepNo - 1) % PAL.Length];
        bool milestone = !key.StartsWith("video:");
        if (milestone) p = key == "path:done" ? new[] { "#FFF1B8", "#E0A526", "#8A5A00" } : (key == "m:half" ? new[] { "#C9F1FF", "#22B8D8", "#0B5A72" } : new[] { "#D9FFD6", "#2FBF5A", "#0E6A2A" });
        StringBuilder s = new StringBuilder("<svg class=\"lb-svg\" viewBox=\"0 0 64 64\" width=\"" + size + "\" height=\"" + size + "\" aria-hidden=\"true\">");
        string shape = milestone ? "<circle cx=\"32\" cy=\"32\" r=\"27\"" : "<path d=\"M32 4l24 14v28L32 60 8 46V18z\"";
        if (earned)
        {
            s.Append("<defs><linearGradient id=\"" + id + "\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\"><stop offset=\"0\" stop-color=\"" + p[0] + "\"/><stop offset=\".55\" stop-color=\"" + p[1] + "\"/><stop offset=\"1\" stop-color=\"" + p[2] + "\"/></linearGradient></defs>");
            s.Append(shape + " fill=\"url(#" + id + ")\" stroke=\"" + p[0] + "\" stroke-width=\"2\"/>");
            s.Append(milestone ? "<circle cx=\"32\" cy=\"32\" r=\"21\" fill=\"none\" stroke=\"#fff\" stroke-opacity=\".45\" stroke-width=\"1.5\"/>" : "<path d=\"M32 10l19 11v22L32 54 13 43V21z\" fill=\"none\" stroke=\"#fff\" stroke-opacity=\".45\" stroke-width=\"1.5\"/>");
        }
        else s.Append(shape + " fill=\"none\" stroke=\"currentColor\" stroke-opacity=\".45\" stroke-width=\"2\" stroke-dasharray=\"4 3\"/>");
        string ink = earned ? "#fff" : "currentColor"; string op = earned ? "1" : ".45";
        if (key == "path:done")
            s.Append("<path d=\"M23 19h18v7a9 9 0 0 1-18 0z\" fill=\"" + ink + "\" fill-opacity=\"" + op + "\"/><path d=\"M23 21h-5a5 5 0 0 0 5 6M41 21h5a5 5 0 0 1-5 6\" fill=\"none\" stroke=\"" + ink + "\" stroke-opacity=\"" + op + "\" stroke-width=\"2\"/><path d=\"M29 35h6v5h4v4H25v-4h4z\" fill=\"" + ink + "\" fill-opacity=\"" + op + "\"/>");
        else if (key == "m:half")
            s.Append("<circle cx=\"32\" cy=\"32\" r=\"11\" fill=\"none\" stroke=\"" + ink + "\" stroke-opacity=\"" + op + "\" stroke-width=\"2.5\"/><path d=\"M32 21a11 11 0 0 1 0 22z\" fill=\"" + ink + "\" fill-opacity=\"" + op + "\"/>");
        else if (key == "m:first")
            s.Append("<path d=\"M26 20v24\" stroke=\"" + ink + "\" stroke-opacity=\"" + op + "\" stroke-width=\"2.6\" stroke-linecap=\"round\"/><path d=\"M27 21h14l-3 5 3 5H27z\" fill=\"" + ink + "\" fill-opacity=\"" + op + "\"/>");
        else
            s.Append("<text x=\"32\" y=\"38\" text-anchor=\"middle\" font-family=\"Sora,Inter,sans-serif\" font-size=\"18\" font-weight=\"800\" fill=\"" + ink + "\" fill-opacity=\"" + op + "\">" + stepNo + "</text>" +
                     "<path d=\"M29 43.5l6 3.5-6 3.5z\" fill=\"" + ink + "\" fill-opacity=\"" + (earned ? ".85" : ".35") + "\"/>");
        return s.Append("</svg>").ToString();
    }

    // ------------------------------------------------------------------ paylasim tercihi
    public static bool GetShare(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(ShareAchievements, 1) FROM dbo.Users WHERE UserId = @u", con))
        { cmd.Parameters.AddWithValue("@u", userId); object o = cmd.ExecuteScalar(); return o == null || Convert.ToBoolean(o); }
    }
    public static void SetShare(int userId, bool on)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET ShareAchievements = @s WHERE UserId = @u", con))
        { cmd.Parameters.AddWithValue("@s", on); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
    }

    // ------------------------------------------------------------------ istemciye durum (sayfa ilk yukleme + PathApi)
    public static object State(int userId, int companyId)
    {
        List<PathStep> steps = GetSteps(userId, companyId);
        List<object> st = new List<object>(); int done = 0;
        for (int i = 0; i < steps.Count; i++)
        {
            PathStep s = steps[i]; if (s.State == "done") done++;
            st.Add(new { no = i + 1, mid = s.MaterialId, title = s.Title, vid = s.YoutubeId, grp = s.Group, state = s.State, pct = s.Percent, skim = s.Skimmed, pos = s.LastPosition,
                         dur = s.DurationSeconds, earned = s.Earned, svg = BadgeSvg("video:" + s.MaterialId, i + 1, s.Earned || s.Qualified, 44) });
        }
        HashSet<string> have = new HashSet<string>();
        foreach (EarnedBadge b in GetBadges(userId, steps)) have.Add(b.Key);
        List<object> ms = new List<object>();
        foreach (string[] m in new[] { new[] { "m:first", "İlk Adım", "İlk videoyu tamamla" }, new[] { "m:half", "Yarı Yol", "Yolun yarısını tamamla" }, new[] { "path:done", PATH_NAME, "Tüm adımları tamamla" } })
            ms.Add(new { key = m[0], title = m[1], hint = m[2], earned = have.Contains(m[0]), svg = BadgeSvg(m[0], 0, have.Contains(m[0]), 44) });
        return new { ok = true, name = PATH_NAME, total = steps.Count, done = done, steps = st, milestones = ms, share = GetShare(userId) };
    }

    // ------------------------------------------------------------------ takim arkadaslari (ayni firma)
    /// <summary>Ayni firmadaki diger aktif kullanicilar: yol ilerlemesi, rozetler, gecerli sertifikalar.
    /// Basari paylasimini kapatanlarin rozet/sertifikasi gosterilmez. Rozetler ilerlemeden hesaplanir
    /// (Akademi'yi henuz acmamis kisilerin eski tamamlamalari da gorunur).</summary>
    public static List<object> Team(int userId, int companyId)
    {
        List<object> list = new List<object>();
        List<object[]> mates = new List<object[]>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TOP 100 UserId, FullName, AvatarImage, ISNULL(ShareAchievements, 1) FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 AND UserId <> @u ORDER BY FullName", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) mates.Add(new object[] { r.GetInt32(0), r[1] as string ?? "", r[2] as string ?? "", Convert.ToBoolean(r[3]) });
        }
        foreach (object[] m in mates)
        {
            int uid = (int)m[0]; bool share = (bool)m[3];
            if (!share) { list.Add(new { name = m[1], ava = m[2], share = false, done = -1, total = 0, badges = new object[0], certs = new object[0] }); continue; }
            List<PathStep> steps = GetSteps(uid, companyId);
            int done = 0; List<object> bs = new List<object>();
            for (int i = 0; i < steps.Count; i++)
                if (steps[i].State == "done") { done++; bs.Add(new { t = (i + 1) + ". " + steps[i].Title, svg = BadgeSvg("video:" + steps[i].MaterialId, i + 1, true, 30) }); }
            if (steps.Count > 0)
            {
                if (done >= 1) bs.Add(new { t = "İlk Adım", svg = BadgeSvg("m:first", 0, true, 30) });
                if (steps.Count >= 2 && done >= (steps.Count + 1) / 2) bs.Add(new { t = "Yarı Yol", svg = BadgeSvg("m:half", 0, true, 30) });
                if (done == steps.Count) bs.Add(new { t = PATH_NAME, svg = BadgeSvg("path:done", 0, true, 30) });
            }
            List<object> cs = new List<object>();
            try
            {
                foreach (CertificateInfo c in ExamRepo.GetUserCertificates(uid))
                    if (!c.IsRevoked)
                        cs.Add(new { t = c.ExamTitle, d = c.IssuedAt.ToString("dd.MM.yyyy"), exp = c.IsExpired, url = "verify/" + c.VerifyCode, svg = ExamRepo.SealSvg(26, c.IsExpired, c.BadgeColor) });
            }
            catch { }
            list.Add(new { name = m[1], ava = m[2], share = true, done = done, total = steps.Count, badges = bs, certs = cs });
        }
        return list;
    }

    /// <summary>Sol menu karti icin hafif ozet: kisi sayisi, yolu bitiren sayisi, ilk 4 yuz, ekipteki en son rozet (paylasimi aciklardan)</summary>
    public static object TeamSummary(int userId, int companyId)
    {
        int count = 0, finished = 0; List<object> faces = new List<object>(); object recent = null;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 AND UserId <> @u", con))
            { cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId); count = Convert.ToInt32(cmd.ExecuteScalar()); }
            if (count == 0) return new { count = 0 };
            // once avatari olanlar, sonra ada gore
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 4 FullName, AvatarImage FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 AND UserId <> @u ORDER BY CASE WHEN ISNULL(AvatarImage,N'') = N'' THEN 1 ELSE 0 END, FullName", con))
            {
                cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId);
                using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) faces.Add(new { n = r[0] as string ?? "", a = r[1] as string ?? "" });
            }
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT b.UserId) FROM dbo.UserBadges b JOIN dbo.Users u ON u.UserId = b.UserId " +
                                                   "WHERE b.BadgeKey = N'path:done' AND u.CompanyId = @c AND u.IsActive = 1 AND u.UserId <> @u AND ISNULL(u.ShareAchievements, 1) = 1", con))
            { cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId); finished = Convert.ToInt32(cmd.ExecuteScalar()); }
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 u.FullName, b.BadgeKey, b.EarnedAt, m.TitleTr FROM dbo.UserBadges b JOIN dbo.Users u ON u.UserId = b.UserId " +
                "LEFT JOIN dbo.TrainingMaterials m ON m.MaterialId = b.MaterialId " +
                "WHERE u.CompanyId = @c AND u.IsActive = 1 AND u.UserId <> @u AND ISNULL(u.ShareAchievements, 1) = 1 AND b.IsRetro = 0 AND b.BadgeKey NOT LIKE N'sys:%' " +
                "AND b.EarnedAt >= DATEADD(DAY, -14, SYSUTCDATETIME()) ORDER BY b.EarnedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId);
                using (SqlDataReader r = cmd.ExecuteReader())
                    if (r.Read())
                    {
                        string key = (string)r[1], title = key == "m:first" ? "İlk Adım" : key == "m:half" ? "Yarı Yol" : key == "path:done" ? PATH_NAME : (r[3] as string ?? "");
                        string first = ((r[0] as string) ?? "").Split(' ')[0];
                        recent = new { who = first, title = title, path = key == "path:done", ago = Ago((DateTime)r[2]) };
                    }
            }
        }
        return new { count = count, finished = finished, faces = faces, recent = recent };
    }
    private static string Ago(DateTime utc)
    {
        DateTime tr = utc.ToTrLocal(), now = DateTime.UtcNow.ToTrLocal();
        int days = (int)(now.Date - tr.Date).TotalDays;
        if (days <= 0) { int h = (int)(now - tr).TotalHours; return h < 1 ? "az önce" : h + " saat önce"; }
        return days == 1 ? "dün" : days + " gün önce";
    }

    /// <summary>Bu hafta (Pazartesi'den beri, TR saati) video izlenen farkli gun sayisi</summary>
    public static int WeekDays(int userId)
    {
        DateTime nowTr = DateTime.UtcNow.AddHours(3);
        int back = ((int)nowTr.DayOfWeek + 6) % 7;                // Pazartesi = 0
        DateTime mondayUtc = nowTr.Date.AddDays(-back).AddHours(-3);
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT CAST(DATEADD(HOUR, 3, LoggedAt) AS DATE)) FROM dbo.TrainingWatchLog WHERE UserId = @u AND LoggedAt >= @m", con))
        { cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@m", mondayUtc); return Convert.ToInt32(cmd.ExecuteScalar()); }
    }

    // ------------------------------------------------------------------ yonetim (Editor Paneli)
    public class AdminStep { public int MaterialId; public string Title, Group; public bool InPath; public int SortOrder; }
    public static List<AdminStep> GetAdminList()
    {
        List<AdminStep> list = new List<AdminStep>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT m.MaterialId, m.TitleTr, ISNULL(NULLIF(m.CategoryGroup,N''), N'Diğer Eğitimler'), CASE WHEN s.StepId IS NULL OR s.IsActive = 0 THEN 0 ELSE 1 END, ISNULL(s.SortOrder, 0) " +
            "FROM dbo.TrainingMaterials m LEFT JOIN dbo.LearningPathSteps s ON s.MaterialId = m.MaterialId " +
            "WHERE m.MaterialType = N'Video' AND m.IsActive = 1 ORDER BY CASE WHEN s.StepId IS NULL OR s.IsActive = 0 THEN 1 ELSE 0 END, s.SortOrder, m.TitleTr", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read()) { AdminStep a = new AdminStep(); a.MaterialId = r.GetInt32(0); a.Title = r[1] as string; a.Group = r[2] as string; a.InPath = r.GetInt32(3) == 1; a.SortOrder = r.GetInt32(4); list.Add(a); }
        return list;
    }
    /// <summary>Yolu verilen sirayla kaydeder; listede olmayan adimlar pasife alinir (kazanilan rozetler silinmez)</summary>
    public static void SaveOrder(List<int> materialIds)
    {
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.LearningPathSteps SET IsActive = 0", con, tx)) cmd.ExecuteNonQuery();
                int ord = 10;
                foreach (int mid in materialIds)
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "IF EXISTS (SELECT 1 FROM dbo.LearningPathSteps WHERE MaterialId = @m) UPDATE dbo.LearningPathSteps SET SortOrder = @o, IsActive = 1 WHERE MaterialId = @m " +
                        "ELSE INSERT INTO dbo.LearningPathSteps (MaterialId, SortOrder) SELECT @m, @o WHERE EXISTS (SELECT 1 FROM dbo.TrainingMaterials WHERE MaterialId = @m AND MaterialType = N'Video')", con, tx))
                    { cmd.Parameters.AddWithValue("@m", mid); cmd.Parameters.AddWithValue("@o", ord); cmd.ExecuteNonQuery(); }
                    ord += 10;
                }
                tx.Commit();
            }
            catch { tx.Rollback(); throw; }
        }
    }

    private static string Cut(string s, int n) { if (s == null) return ""; return s.Length > n ? s.Substring(0, n) : s; }
}
