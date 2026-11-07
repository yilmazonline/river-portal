using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

// ===================== MODELLER =====================
public class ExamInfo
{
    public int ExamId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CategoryGroup { get; set; }
    public string Status { get; set; }          // Draft | Published | Active | Archived
    public int PassScore { get; set; }
    public int MaxAttempts { get; set; }
    public int CooldownHours { get; set; }
    public int? TimeLimitMin { get; set; }
    public int? ValidMonths { get; set; }
    public string BadgeColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int TotalPoints { get; set; }
}

public class ExamOption
{
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public string OText { get; set; }
    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}

public class ExamQuestion
{
    public int QuestionId { get; set; }
    public int ExamId { get; set; }
    public string QText { get; set; }
    public string QType { get; set; }           // single | multi | tf
    public int Points { get; set; }
    public int SortOrder { get; set; }
    public List<ExamOption> Options = new List<ExamOption>();
}

public class ExamAttempt
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? Score { get; set; }
    public bool? Passed { get; set; }
    public string UserName { get; set; }
    public string CompanyName { get; set; }
}

public class CertificateInfo
{
    public int CertificateId { get; set; }
    public string CertNo { get; set; }
    public string VerifyCode { get; set; }
    public int UserId { get; set; }
    public int ExamId { get; set; }
    public string HolderName { get; set; }
    public string ExamTitle { get; set; }
    public string CompanyName { get; set; }
    public int Score { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsRevoked { get; set; }
    public string BadgeColor { get; set; }
    public bool IsExpired { get { return ValidUntil.HasValue && ValidUntil.Value < DateTime.UtcNow; } }
}

public class ExamVideo
{
    public int MaterialId { get; set; }
    public string Title { get; set; }
    public int Percent { get; set; }
    public bool Done { get; set; }
}

public class GroupVideo
{
    public string Group;
    public int MaterialId, Percent, WatchedSeconds;
    public string Title, YoutubeId;
    public bool Done, Started;
}

public class RetakeRequest
{
    public int RequestId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string CompanyName { get; set; }
    public string Note { get; set; }
    public string Status { get; set; }          // Pending | Approved | Rejected | Used
    public DateTime RequestedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string DecisionNote { get; set; }
    public int? LastScore { get; set; }
    public int Tries { get; set; }
}

// Sinava giris sartlarinin tek noktadan hesaplanmis durumu
public class ExamGate
{
    public bool CanStart, Passed, IsActive, IsReady, VideosOk, QuotaOk, NeedsRequest, Pending, HasOpenAttempt;
    public int VideosDone, VideosTotal, Submitted, MaxAttempts, FreeLeft, CooldownHoursLeft, ApprovedRequestId;
    public int? LastScore;
    public RetakeRequest Latest;
    public string Reason = "";
}

// ===================== ROZET PALETI =====================
public class BadgePalette
{
    public string Key, Name, C1, C2, C3, Accent;
    public BadgePalette(string key, string name, string c1, string c2, string c3, string accent)
    { Key = key; Name = name; C1 = c1; C2 = c2; C3 = c3; Accent = accent; }

    // Sira: sicak metaller -> soguk metaller -> degerli taslar
    public static readonly BadgePalette[] All = new BadgePalette[] {
        new BadgePalette("gold",      "Altın",     "#FBE7AE", "#C9A04E", "#7A5A22", "#B08D4C"),
        new BadgePalette("champagne", "Bej",       "#FFF4DA", "#D6BC84", "#8F7440", "#B89C62"),
        new BadgePalette("rosegold",  "Gül Altın", "#FCDCCD", "#D98C77", "#8E4E3F", "#C07A67"),
        new BadgePalette("bronze",    "Bronz",     "#F4CC9C", "#B4703A", "#5E3413", "#A5652E"),
        new BadgePalette("platinum",  "Platin",    "#F4F6FA", "#9BA4B4", "#525B6B", "#7E8898"),
        new BadgePalette("onyx",      "Oniks",     "#7B808E", "#2A2E38", "#0B0D12", "#B08D4C"),
        new BadgePalette("ruby",      "Yakut",     "#FFA3B5", "#C8173D", "#5E0819", "#B3183A"),
        new BadgePalette("emerald",   "Zümrüt",    "#9AF5CE", "#1CA273", "#08503A", "#168F64"),
        new BadgePalette("sapphire",  "Safir",     "#B0D0FF", "#2E6DE0", "#122F78", "#2A63C8"),
        new BadgePalette("amethyst",  "Ametist",   "#E3C8FF", "#8A3FD1", "#3E1370", "#7C3AC2")
    };
    public static BadgePalette Get(string key)
    {
        for (int i = 0; i < All.Length; i++) if (All[i].Key == key) return All[i];
        return All[0];
    }
    public static string Norm(string key) { return Get(key).Key; }
}

// ===================== VERI ERISIMI =====================
public static class ExamRepo
{
    public const int VideoCompletePercent = 90;   // video "tamamlandi" esigi (Repo.SaveProgress ile ayni: %90)

    private static object N(object v) { return v ?? DBNull.Value; }
    private static int? NInt(object o) { return (o == null || o == DBNull.Value) ? (int?)null : Convert.ToInt32(o); }
    private static DateTime? NDt(object o) { return (o == null || o == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(o); }

    public static string StatusLabel(string s)
    {
        if (s == "Published") return "Yayında (salt okunur)";
        if (s == "Active") return "Aktif";
        if (s == "Archived") return "Arşiv";
        return "Taslak";
    }
    public static string StatusClass(string s)
    {
        if (s == "Published") return "pending";
        if (s == "Active") return "approved";
        if (s == "Archived") return "cancelled";
        return "passive";
    }
    public static string TypeLabel(string t)
    {
        if (t == "multi") return "Çoklu seçim";
        if (t == "tf") return "Doğru / Yanlış";
        return "Tek seçim";
    }

    // ---------- Sinavlar ----------
    private const string EXAM_SELECT =
        "SELECT e.ExamId, e.Title, e.Description, e.CategoryGroup, e.Status, e.PassScore, e.MaxAttempts, e.CooldownHours, e.TimeLimitMin, e.ValidMonths, e.BadgeColor, e.CreatedAt, e.UpdatedAt, " +
        "(SELECT COUNT(*) FROM dbo.ExamQuestions q WHERE q.ExamId = e.ExamId) AS QCount, " +
        "(SELECT ISNULL(SUM(q.Points),0) FROM dbo.ExamQuestions q WHERE q.ExamId = e.ExamId) AS QPoints FROM dbo.Exams e ";

    private static ExamInfo ReadExam(SqlDataReader r)
    {
        ExamInfo e = new ExamInfo();
        e.ExamId = (int)r["ExamId"];
        e.Title = r["Title"] as string;
        e.Description = r["Description"] as string;
        e.CategoryGroup = r["CategoryGroup"] as string;
        e.Status = r["Status"] as string;
        e.PassScore = (int)r["PassScore"];
        e.MaxAttempts = (int)r["MaxAttempts"];
        e.CooldownHours = (int)r["CooldownHours"];
        e.TimeLimitMin = NInt(r["TimeLimitMin"]);
        e.ValidMonths = NInt(r["ValidMonths"]);
        e.BadgeColor = BadgePalette.Norm(r["BadgeColor"] as string);
        e.CreatedAt = (DateTime)r["CreatedAt"];
        e.UpdatedAt = (DateTime)r["UpdatedAt"];
        e.QuestionCount = Convert.ToInt32(r["QCount"]);
        e.TotalPoints = Convert.ToInt32(r["QPoints"]);
        return e;
    }

    public static List<ExamInfo> GetExams(bool includeDraft)
    {
        List<ExamInfo> list = new List<ExamInfo>();
        string where = includeDraft ? "" : "WHERE e.Status IN (N'Published', N'Active') ";
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(EXAM_SELECT + where +
            "ORDER BY CASE e.Status WHEN N'Active' THEN 0 WHEN N'Published' THEN 1 WHEN N'Draft' THEN 2 ELSE 3 END, e.UpdatedAt DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read()) list.Add(ReadExam(r));
        return list;
    }

    public static ExamInfo GetExam(int examId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(EXAM_SELECT + "WHERE e.ExamId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", examId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadExam(r);
        }
        return null;
    }

    public static int SaveExam(ExamInfo e, int userId)
    {
        using (SqlConnection con = Db.Open())
        {
            if (e.ExamId <= 0)
            {
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO dbo.Exams (Title, Description, CategoryGroup, PassScore, MaxAttempts, CooldownHours, TimeLimitMin, ValidMonths, BadgeColor, CreatedBy) " +
                    "OUTPUT INSERTED.ExamId VALUES (@t, @d, @g, @p, @m, @c, @tl, @vm, @bc, @by)", con))
                {
                    AddExamParams(cmd, e);
                    cmd.Parameters.AddWithValue("@by", userId);
                    return (int)cmd.ExecuteScalar();
                }
            }
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.Exams SET Title=@t, Description=@d, CategoryGroup=@g, PassScore=@p, MaxAttempts=@m, CooldownHours=@c, " +
                "TimeLimitMin=@tl, ValidMonths=@vm, BadgeColor=@bc, UpdatedAt=SYSUTCDATETIME() WHERE ExamId=@id", con))
            {
                AddExamParams(cmd, e);
                cmd.Parameters.AddWithValue("@id", e.ExamId);
                cmd.ExecuteNonQuery();
            }
            return e.ExamId;
        }
    }

    private static void AddExamParams(SqlCommand cmd, ExamInfo e)
    {
        cmd.Parameters.AddWithValue("@t", e.Title ?? "");
        cmd.Parameters.AddWithValue("@d", N(string.IsNullOrEmpty(e.Description) ? null : e.Description));
        cmd.Parameters.AddWithValue("@g", N(string.IsNullOrEmpty(e.CategoryGroup) ? null : e.CategoryGroup));
        cmd.Parameters.AddWithValue("@p", e.PassScore);
        cmd.Parameters.AddWithValue("@m", e.MaxAttempts);
        cmd.Parameters.AddWithValue("@c", e.CooldownHours);
        cmd.Parameters.AddWithValue("@tl", e.TimeLimitMin.HasValue ? (object)e.TimeLimitMin.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@vm", e.ValidMonths.HasValue ? (object)e.ValidMonths.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@bc", BadgePalette.Norm(e.BadgeColor));
    }

    public static void SetStatus(int examId, string status)
    {
        string extra = status == "Published" ? ", PublishedAt = ISNULL(PublishedAt, SYSUTCDATETIME())" :
                       status == "Active" ? ", ActivatedAt = ISNULL(ActivatedAt, SYSUTCDATETIME())" : "";
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Exams SET Status=@s, UpdatedAt=SYSUTCDATETIME()" + extra + " WHERE ExamId=@id", con))
        {
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@id", examId);
            cmd.ExecuteNonQuery();
        }
    }

    public static bool HasAttempts(int examId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ExamAttempts WHERE ExamId=@id", con))
        { cmd.Parameters.AddWithValue("@id", examId); return (int)cmd.ExecuteScalar() > 0; }
    }

    public static void DeleteExam(int examId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.Exams WHERE ExamId=@id AND Status=N'Draft' AND NOT EXISTS (SELECT 1 FROM dbo.ExamAttempts a WHERE a.ExamId=@id)", con))
        { cmd.Parameters.AddWithValue("@id", examId); cmd.ExecuteNonQuery(); }
    }

    // Sinavi sorulari ve siklariyla kopyala (yeni Taslak)
    public static int CopyExam(int examId, int userId)
    {
        ExamInfo src = GetExam(examId);
        if (src == null) return 0;
        src.ExamId = 0;
        src.Title = src.Title + " (Kopya)";
        int newId = SaveExam(src, userId);
        List<ExamQuestion> qs = GetQuestions(examId);
        for (int i = 0; i < qs.Count; i++)
        {
            List<KeyValuePair<string, bool>> opts = new List<KeyValuePair<string, bool>>();
            for (int j = 0; j < qs[i].Options.Count; j++) opts.Add(new KeyValuePair<string, bool>(qs[i].Options[j].OText, qs[i].Options[j].IsCorrect));
            AddQuestion(newId, qs[i].QText, qs[i].QType, qs[i].Points, opts);
        }
        return newId;
    }

    // ---------- Sorular ----------
    public static List<ExamQuestion> GetQuestions(int examId)
    {
        List<ExamQuestion> list = new List<ExamQuestion>();
        Dictionary<int, ExamQuestion> map = new Dictionary<int, ExamQuestion>();
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("SELECT QuestionId, ExamId, QText, QType, Points, SortOrder FROM dbo.ExamQuestions WHERE ExamId=@id ORDER BY SortOrder, QuestionId", con))
            {
                cmd.Parameters.AddWithValue("@id", examId);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        ExamQuestion q = new ExamQuestion();
                        q.QuestionId = (int)r["QuestionId"]; q.ExamId = (int)r["ExamId"];
                        q.QText = r["QText"] as string; q.QType = r["QType"] as string;
                        q.Points = (int)r["Points"]; q.SortOrder = (int)r["SortOrder"];
                        list.Add(q); map[q.QuestionId] = q;
                    }
            }
            using (SqlCommand cmd = new SqlCommand(
                "SELECT o.OptionId, o.QuestionId, o.OText, o.IsCorrect, o.SortOrder FROM dbo.ExamOptions o " +
                "JOIN dbo.ExamQuestions q ON q.QuestionId = o.QuestionId WHERE q.ExamId=@id ORDER BY o.SortOrder, o.OptionId", con))
            {
                cmd.Parameters.AddWithValue("@id", examId);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        ExamOption o = new ExamOption();
                        o.OptionId = (int)r["OptionId"]; o.QuestionId = (int)r["QuestionId"];
                        o.OText = r["OText"] as string; o.IsCorrect = (bool)r["IsCorrect"]; o.SortOrder = (int)r["SortOrder"];
                        ExamQuestion q; if (map.TryGetValue(o.QuestionId, out q)) q.Options.Add(o);
                    }
            }
        }
        return list;
    }

    public static int AddQuestion(int examId, string text, string type, int points, List<KeyValuePair<string, bool>> opts)
    {
        int qid;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.ExamQuestions (ExamId, QText, QType, Points, SortOrder) OUTPUT INSERTED.QuestionId " +
                "VALUES (@e, @t, @ty, @p, (SELECT ISNULL(MAX(SortOrder),0)+1 FROM dbo.ExamQuestions WHERE ExamId=@e))", con))
            {
                cmd.Parameters.AddWithValue("@e", examId); cmd.Parameters.AddWithValue("@t", text);
                cmd.Parameters.AddWithValue("@ty", type); cmd.Parameters.AddWithValue("@p", points);
                qid = (int)cmd.ExecuteScalar();
            }
            InsertOptions(con, qid, opts);
            Touch(con, examId);
        }
        return qid;
    }

    public static void UpdateQuestion(int questionId, string text, string type, int points, List<KeyValuePair<string, bool>> opts)
    {
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ExamQuestions SET QText=@t, QType=@ty, Points=@p WHERE QuestionId=@id", con))
            {
                cmd.Parameters.AddWithValue("@t", text); cmd.Parameters.AddWithValue("@ty", type);
                cmd.Parameters.AddWithValue("@p", points); cmd.Parameters.AddWithValue("@id", questionId);
                cmd.ExecuteNonQuery();
            }
            using (SqlCommand del = new SqlCommand("DELETE FROM dbo.ExamOptions WHERE QuestionId=@id", con))
            { del.Parameters.AddWithValue("@id", questionId); del.ExecuteNonQuery(); }
            InsertOptions(con, questionId, opts);
            using (SqlCommand t = new SqlCommand("UPDATE dbo.Exams SET UpdatedAt=SYSUTCDATETIME() WHERE ExamId=(SELECT ExamId FROM dbo.ExamQuestions WHERE QuestionId=@id)", con))
            { t.Parameters.AddWithValue("@id", questionId); t.ExecuteNonQuery(); }
        }
    }

    private static void InsertOptions(SqlConnection con, int qid, List<KeyValuePair<string, bool>> opts)
    {
        for (int i = 0; i < opts.Count; i++)
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ExamOptions (QuestionId, OText, IsCorrect, SortOrder) VALUES (@q, @t, @c, @s)", con))
            {
                cmd.Parameters.AddWithValue("@q", qid); cmd.Parameters.AddWithValue("@t", opts[i].Key);
                cmd.Parameters.AddWithValue("@c", opts[i].Value); cmd.Parameters.AddWithValue("@s", i + 1);
                cmd.ExecuteNonQuery();
            }
    }

    private static void Touch(SqlConnection con, int examId)
    {
        using (SqlCommand t = new SqlCommand("UPDATE dbo.Exams SET UpdatedAt=SYSUTCDATETIME() WHERE ExamId=@e", con))
        { t.Parameters.AddWithValue("@e", examId); t.ExecuteNonQuery(); }
    }

    public static int GetQuestionExamId(int questionId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ExamId FROM dbo.ExamQuestions WHERE QuestionId=@id", con))
        { cmd.Parameters.AddWithValue("@id", questionId); object o = cmd.ExecuteScalar(); return o == null ? 0 : (int)o; }
    }

    public static void DeleteQuestion(int questionId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.ExamQuestions WHERE QuestionId=@id", con))
        { cmd.Parameters.AddWithValue("@id", questionId); cmd.ExecuteNonQuery(); }
    }

    // Puanlari 100'e esit dagit (kalan artik ilk sorulara +1)
    public static void EqualizePoints(int examId)
    {
        List<ExamQuestion> qs = GetQuestions(examId);
        if (qs.Count == 0) return;
        int n = qs.Count, baseP = 100 / n, rem = 100 - baseP * n;
        using (SqlConnection con = Db.Open())
            for (int i = 0; i < n; i++)
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ExamQuestions SET Points=@p WHERE QuestionId=@id", con))
                { cmd.Parameters.AddWithValue("@p", Math.Max(1, baseP + (i < rem ? 1 : 0))); cmd.Parameters.AddWithValue("@id", qs[i].QuestionId); cmd.ExecuteNonQuery(); }
    }

    // Soruyu bir yukari / asagi tasi
    public static void MoveQuestion(int examId, int questionId, int dir)
    {
        List<ExamQuestion> qs = GetQuestions(examId);
        int idx = -1;
        for (int i = 0; i < qs.Count; i++) if (qs[i].QuestionId == questionId) { idx = i; break; }
        int j = idx + (dir < 0 ? -1 : 1);
        if (idx < 0 || j < 0 || j >= qs.Count) return;
        ExamQuestion tmp = qs[idx]; qs[idx] = qs[j]; qs[j] = tmp;
        using (SqlConnection con = Db.Open())
            for (int i = 0; i < qs.Count; i++)
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ExamQuestions SET SortOrder=@s WHERE QuestionId=@id", con))
                { cmd.Parameters.AddWithValue("@s", i + 1); cmd.Parameters.AddWithValue("@id", qs[i].QuestionId); cmd.ExecuteNonQuery(); }
    }

    // ---------- Video kosulu ----------
    public static List<string> GetVideoGroups()
    {
        List<string> list = new List<string>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT DISTINCT CategoryGroup FROM dbo.TrainingMaterials WHERE MaterialType=N'Video' AND IsActive=1 AND CategoryGroup IS NOT NULL AND CategoryGroup<>N'' ORDER BY CategoryGroup", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read()) list.Add(r["CategoryGroup"] as string);
        return list;
    }

    // Kullanicinin bir gruptaki video ilerlemesi (grup bos ise kosul yok -> 0/0)
    public static void VideoProgress(string group, int userId, int companyId, out int done, out int total)
    {
        done = 0; total = 0;
        if (string.IsNullOrEmpty(group)) return;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT COUNT(*) AS Total, " +
            "SUM(CASE WHEN p.Completed = 1 OR p.[Percent] >= @pct THEN 1 ELSE 0 END) AS Done " +
            "FROM dbo.TrainingMaterials m LEFT JOIN dbo.TrainingProgress p ON p.MaterialId = m.MaterialId AND p.UserId = @u " +
            "WHERE m.MaterialType=N'Video' AND m.IsActive=1 AND m.CategoryGroup=@g AND (m.CompanyId IS NULL OR m.CompanyId=@c)", con))
        {
            cmd.Parameters.AddWithValue("@pct", VideoCompletePercent);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@g", group);
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    total = r["Total"] == DBNull.Value ? 0 : Convert.ToInt32(r["Total"]);
                    done = r["Done"] == DBNull.Value ? 0 : Convert.ToInt32(r["Done"]);
                }
        }
    }

    // ---------- Denemeler ----------
    private const string ATT_SELECT =
        "SELECT a.AttemptId, a.ExamId, a.UserId, a.CompanyId, a.StartedAt, a.DeadlineAt, a.SubmittedAt, a.Score, a.Passed, " +
        "u.FullName AS UserName, c.Name AS CompanyName FROM dbo.ExamAttempts a " +
        "LEFT JOIN dbo.Users u ON u.UserId = a.UserId LEFT JOIN dbo.Companies c ON c.CompanyId = a.CompanyId ";

    private static ExamAttempt ReadAttempt(SqlDataReader r)
    {
        ExamAttempt a = new ExamAttempt();
        a.AttemptId = (int)r["AttemptId"]; a.ExamId = (int)r["ExamId"];
        a.UserId = (int)r["UserId"]; a.CompanyId = (int)r["CompanyId"];
        a.StartedAt = (DateTime)r["StartedAt"];
        a.DeadlineAt = NDt(r["DeadlineAt"]); a.SubmittedAt = NDt(r["SubmittedAt"]);
        a.Score = NInt(r["Score"]);
        a.Passed = r["Passed"] == DBNull.Value ? (bool?)null : (bool)r["Passed"];
        a.UserName = r["UserName"] as string; a.CompanyName = r["CompanyName"] as string;
        return a;
    }

    public static List<ExamAttempt> GetUserAttempts(int examId, int userId)
    {
        List<ExamAttempt> list = new List<ExamAttempt>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(ATT_SELECT + "WHERE a.ExamId=@e AND a.UserId=@u ORDER BY a.StartedAt", con))
        {
            cmd.Parameters.AddWithValue("@e", examId); cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(ReadAttempt(r));
        }
        return list;
    }

    public static List<ExamAttempt> GetExamResults(int examId)
    {
        List<ExamAttempt> list = new List<ExamAttempt>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(ATT_SELECT + "WHERE a.ExamId=@e AND a.SubmittedAt IS NOT NULL ORDER BY a.SubmittedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@e", examId);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(ReadAttempt(r));
        }
        return list;
    }

    public static ExamAttempt GetAttempt(int attemptId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(ATT_SELECT + "WHERE a.AttemptId=@id", con))
        {
            cmd.Parameters.AddWithValue("@id", attemptId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadAttempt(r);
        }
        return null;
    }

    public static int StartAttempt(int examId, int userId, int companyId, int? timeLimitMin)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.ExamAttempts (ExamId, UserId, CompanyId, DeadlineAt) OUTPUT INSERTED.AttemptId " +
            "VALUES (@e, @u, @c, " + (timeLimitMin.HasValue ? "DATEADD(MINUTE, @tl, SYSUTCDATETIME())" : "NULL") + ")", con))
        {
            cmd.Parameters.AddWithValue("@e", examId); cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@c", companyId);
            if (timeLimitMin.HasValue) cmd.Parameters.AddWithValue("@tl", timeLimitMin.Value);
            return (int)cmd.ExecuteScalar();
        }
    }

    // Degerlendir + kaydet. selections: QuestionId -> secilen OptionId'ler. Donen: puan.
    // Puanlama: soru tam dogruysa puaninin tamami, aksi halde 0 (coklu secimde eksik/fazla = 0).
    public static int GradeAndSubmit(int attemptId, List<ExamQuestion> qs, Dictionary<int, List<int>> selections, int passScore, out bool passed)
    {
        int score = 0;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand clr = new SqlCommand("DELETE FROM dbo.ExamAnswers WHERE AttemptId=@a", con))
            { clr.Parameters.AddWithValue("@a", attemptId); clr.ExecuteNonQuery(); }
            for (int i = 0; i < qs.Count; i++)
            {
                ExamQuestion q = qs[i];
                List<int> sel;
                if (!selections.TryGetValue(q.QuestionId, out sel)) sel = new List<int>();
                List<int> correct = new List<int>();
                for (int j = 0; j < q.Options.Count; j++) if (q.Options[j].IsCorrect) correct.Add(q.Options[j].OptionId);
                bool ok = correct.Count > 0 && sel.Count == correct.Count;
                if (ok) for (int k = 0; k < sel.Count; k++) if (!correct.Contains(sel[k])) { ok = false; break; }
                int pts = ok ? q.Points : 0;
                score += pts;
                string[] ids = new string[sel.Count];
                for (int k = 0; k < sel.Count; k++) ids[k] = sel[k].ToString();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ExamAnswers (AttemptId, QuestionId, OptionIds, IsCorrect, PointsEarned) VALUES (@a, @q, @o, @c, @p)", con))
                {
                    cmd.Parameters.AddWithValue("@a", attemptId); cmd.Parameters.AddWithValue("@q", q.QuestionId);
                    cmd.Parameters.AddWithValue("@o", string.Join(",", ids)); cmd.Parameters.AddWithValue("@c", ok);
                    cmd.Parameters.AddWithValue("@p", pts);
                    cmd.ExecuteNonQuery();
                }
            }
            if (score > 100) score = 100;
            passed = score >= passScore;
            using (SqlCommand up = new SqlCommand("UPDATE dbo.ExamAttempts SET SubmittedAt=SYSUTCDATETIME(), Score=@s, Passed=@p WHERE AttemptId=@a AND SubmittedAt IS NULL", con))
            {
                up.Parameters.AddWithValue("@s", score); up.Parameters.AddWithValue("@p", passed); up.Parameters.AddWithValue("@a", attemptId);
                up.ExecuteNonQuery();
            }
        }
        return score;
    }

    // Suresi dolmus ve teslim edilmemis denemeyi 0 puanla kapat
    public static void ExpireAttempt(int attemptId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ExamAttempts SET SubmittedAt=SYSUTCDATETIME(), Score=0, Passed=0 WHERE AttemptId=@a AND SubmittedAt IS NULL", con))
        { cmd.Parameters.AddWithValue("@a", attemptId); cmd.ExecuteNonQuery(); }
    }

    // ---------- Sertifikalar ----------
    private const string CERT_SELECT =
        "SELECT CertificateId, CertNo, VerifyCode, UserId, ExamId, HolderName, ExamTitle, CompanyName, Score, IssuedAt, ValidUntil, IsRevoked, BadgeColor FROM dbo.Certificates ";

    private static CertificateInfo ReadCert(SqlDataReader r)
    {
        CertificateInfo c = new CertificateInfo();
        c.CertificateId = (int)r["CertificateId"]; c.CertNo = r["CertNo"] as string; c.VerifyCode = r["VerifyCode"] as string;
        c.UserId = (int)r["UserId"]; c.ExamId = (int)r["ExamId"];
        c.HolderName = r["HolderName"] as string; c.ExamTitle = r["ExamTitle"] as string; c.CompanyName = r["CompanyName"] as string;
        c.Score = (int)r["Score"]; c.IssuedAt = (DateTime)r["IssuedAt"]; c.ValidUntil = NDt(r["ValidUntil"]);
        c.IsRevoked = (bool)r["IsRevoked"];
        c.BadgeColor = BadgePalette.Norm(r["BadgeColor"] as string);
        return c;
    }

    public static CertificateInfo GetUserExamCertificate(int userId, int examId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(CERT_SELECT + "WHERE UserId=@u AND ExamId=@e", con))
        {
            cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@e", examId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadCert(r);
        }
        return null;
    }

    public static List<CertificateInfo> GetUserCertificates(int userId)
    {
        List<CertificateInfo> list = new List<CertificateInfo>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(CERT_SELECT + "WHERE UserId=@u ORDER BY IssuedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(ReadCert(r));
        }
        return list;
    }

    public static CertificateInfo GetCertificateByCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(CERT_SELECT + "WHERE VerifyCode=@c", con))   // yalniz tahmin edilemez kod (sirali CertNo ile sorgu yok)
        {
            cmd.Parameters.AddWithValue("@c", code.Trim().ToUpperInvariant());
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadCert(r);
        }
        return null;
    }

    public static CertificateInfo GetCertificate(int certificateId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(CERT_SELECT + "WHERE CertificateId=@id", con))
        {
            cmd.Parameters.AddWithValue("@id", certificateId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadCert(r);
        }
        return null;
    }

    // Tahmin edilemez, karistirilmayan karakterlerle (0/O, 1/I yok) 10 haneli dogrulama kodu
    private static string NewVerifyCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        byte[] buf = new byte[10];
        using (System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create()) rng.GetBytes(buf);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < buf.Length; i++) sb.Append(alphabet[buf[i] % alphabet.Length]);
        return sb.ToString();
    }

    public static CertificateInfo IssueCertificate(int userId, int companyId, ExamInfo exam, int attemptId, int score, string holderName, string companyName)
    {
        CertificateInfo existing = GetUserExamCertificate(userId, exam.ExamId);
        if (existing != null) return existing;
        int id;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.Certificates (VerifyCode, UserId, CompanyId, ExamId, AttemptId, HolderName, ExamTitle, CompanyName, Score, BadgeColor, ValidUntil) " +
                "OUTPUT INSERTED.CertificateId VALUES (@v, @u, @c, @e, @a, @h, @t, @cn, @s, @bc, " +
                (exam.ValidMonths.HasValue ? "DATEADD(MONTH, @vm, SYSUTCDATETIME())" : "NULL") + ")", con))
            {
                cmd.Parameters.AddWithValue("@v", NewVerifyCode());
                cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@c", companyId);
                cmd.Parameters.AddWithValue("@e", exam.ExamId); cmd.Parameters.AddWithValue("@a", attemptId);
                cmd.Parameters.AddWithValue("@h", holderName); cmd.Parameters.AddWithValue("@t", exam.Title ?? "");
                cmd.Parameters.AddWithValue("@cn", N(companyName)); cmd.Parameters.AddWithValue("@s", score);
                cmd.Parameters.AddWithValue("@bc", BadgePalette.Norm(exam.BadgeColor));
                if (exam.ValidMonths.HasValue) cmd.Parameters.AddWithValue("@vm", exam.ValidMonths.Value);
                id = (int)cmd.ExecuteScalar();
            }
            using (SqlCommand no = new SqlCommand("UPDATE dbo.Certificates SET CertNo = N'RVR-' + CAST(YEAR(IssuedAt) AS NVARCHAR(4)) + N'-' + RIGHT(N'000000' + CAST(CertificateId AS NVARCHAR(10)), 6) WHERE CertificateId=@id", con))
            { no.Parameters.AddWithValue("@id", id); no.ExecuteNonQuery(); }
        }
        return GetCertificate(id);
    }

    public static void SetRevoked(int certificateId, bool revoked)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Certificates SET IsRevoked=@r WHERE CertificateId=@id", con))
        { cmd.Parameters.AddWithValue("@r", revoked); cmd.Parameters.AddWithValue("@id", certificateId); cmd.ExecuteNonQuery(); }
    }

    // Mini altin muhur (rozet) - egitim sayfasi / listeler icin. dim = suresi dolmus/iptal
    private static int _sealSeq = 0;
    public static string SealSvg(int px, bool dim) { return SealSvg(px, dim, "gold"); }

    public static string SealSvg(int px, bool dim, string color)
    {
        int id = System.Threading.Interlocked.Increment(ref _sealSeq);
        StringBuilder pts = new StringBuilder();
        for (int i = 0; i < 48; i++)
        {
            double r = (i % 2 == 0) ? 48 : 43;
            double a = Math.PI * 2 * i / 48 - Math.PI / 2;
            if (i > 0) pts.Append(" ");
            pts.Append((50 + r * Math.Cos(a)).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "," +
                       (50 + r * Math.Sin(a)).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
        }
        string g = "sg" + id;
        BadgePalette p = BadgePalette.Get(color);
        string c1 = dim ? "#C9CED8" : p.C1, c2 = dim ? "#9AA2B2" : p.C2, c3 = dim ? "#7B8394" : p.C3;
        return "<svg width=\"" + px + "\" height=\"" + px + "\" viewBox=\"0 0 100 100\" aria-hidden=\"true\">" +
            "<defs><linearGradient id=\"" + g + "\" x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\">" +
            "<stop offset=\"0\" stop-color=\"" + c1 + "\"/><stop offset=\".42\" stop-color=\"" + c2 + "\"/><stop offset=\".78\" stop-color=\"" + c2 + "\"/><stop offset=\"1\" stop-color=\"" + c3 + "\"/></linearGradient>" +
            "<radialGradient id=\"" + g + "h\" cx=\".5\" cy=\".1\" r=\".85\"><stop offset=\"0\" stop-color=\"#fff\" stop-opacity=\"" + (dim ? ".06" : ".16") + "\"/><stop offset=\"1\" stop-color=\"#fff\" stop-opacity=\"0\"/></radialGradient>" +
            "<filter id=\"" + g + "f\" x=\"-20%\" y=\"-20%\" width=\"140%\" height=\"140%\"><feDropShadow dx=\"0\" dy=\"1.2\" stdDeviation=\"1.4\" flood-color=\"#1a1206\" flood-opacity=\".45\"/></filter></defs>" +
            "<polygon fill=\"url(#" + g + ")\" points=\"" + pts.ToString() + "\"/>" +
            "<polygon fill=\"url(#" + g + "h)\" points=\"" + pts.ToString() + "\"/>" +
            "<circle cx=\"50\" cy=\"50\" r=\"36\" fill=\"none\" stroke=\"#FDFBF6\" stroke-width=\"1.6\" opacity=\".85\"/>" +
            "<image href=\"assets/fol_mark_light.png\" x=\"31\" y=\"30.5\" width=\"38\" height=\"37.2\" filter=\"url(#" + g + "f)\"/></svg>";
    }

    // Sertifikada gorunecek ad (bir kez teyit edilir)
    public static string GetCertificateName(int userId)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT CertificateName FROM dbo.Users WHERE UserId=@u", con))
            { cmd.Parameters.AddWithValue("@u", userId); object o = cmd.ExecuteScalar(); return (o == null || o == DBNull.Value) ? null : (string)o; }
        }
        catch { return null; }
    }

    public static void SetCertificateName(int userId, string name)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET CertificateName=@n WHERE UserId=@u", con))
        { cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
    }

    // ===================== EGITIM VIDEOLARI (giris sarti detayi) =====================
    public static List<ExamVideo> GetGroupVideos(string group, int userId, int companyId)
    {
        List<ExamVideo> list = new List<ExamVideo>();
        if (string.IsNullOrEmpty(group)) return list;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT m.MaterialId, m.TitleTr, ISNULL(p.[Percent],0) AS Pct, CASE WHEN p.Completed = 1 OR p.[Percent] >= @pct THEN 1 ELSE 0 END AS Done " +
            "FROM dbo.TrainingMaterials m LEFT JOIN dbo.TrainingProgress p ON p.MaterialId = m.MaterialId AND p.UserId = @u " +
            "WHERE m.MaterialType=N'Video' AND m.IsActive=1 AND m.CategoryGroup=@g AND (m.CompanyId IS NULL OR m.CompanyId=@c) " +
            "ORDER BY m.SortOrder, m.MaterialId", con))
        {
            cmd.Parameters.AddWithValue("@pct", VideoCompletePercent);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@g", group);
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    ExamVideo v = new ExamVideo();
                    v.MaterialId = (int)r["MaterialId"]; v.Title = r["TitleTr"] as string;
                    v.Percent = Convert.ToInt32(r["Pct"]); v.Done = Convert.ToInt32(r["Done"]) == 1;
                    list.Add(v);
                }
        }
        return list;
    }

    // Egitim gecmisi: tum aktif videolarin kullanici ilerlemesi (grup = CategoryGroup, bos ise "Diger Egitimler")
    public static List<GroupVideo> GetAllVideoProgress(int userId, int companyId)
    {
        List<GroupVideo> list = new List<GroupVideo>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT ISNULL(NULLIF(m.CategoryGroup,N''), N'Diğer Eğitimler') AS Grp, m.MaterialId, m.TitleTr, m.YoutubeId, " +
            "ISNULL(p.[Percent],0) AS Pct, ISNULL(p.WatchedSeconds,0) AS Sec, CASE WHEN p.ProgressId IS NULL THEN 0 ELSE 1 END AS Started, " +
            "CASE WHEN p.Completed = 1 OR p.[Percent] >= @pct THEN 1 ELSE 0 END AS Done " +
            "FROM dbo.TrainingMaterials m LEFT JOIN dbo.TrainingProgress p ON p.MaterialId = m.MaterialId AND p.UserId = @u " +
            "WHERE m.MaterialType=N'Video' AND m.IsActive=1 AND (m.CompanyId IS NULL OR m.CompanyId=@c) " +
            "ORDER BY CASE WHEN NULLIF(m.CategoryGroup,N'') IS NULL THEN 1 ELSE 0 END, m.CategoryGroup, m.SortOrder, m.MaterialId", con))
        {
            cmd.Parameters.AddWithValue("@pct", VideoCompletePercent);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    GroupVideo v = new GroupVideo();
                    v.Group = r["Grp"] as string; v.MaterialId = (int)r["MaterialId"]; v.Title = r["TitleTr"] as string; v.YoutubeId = (r["YoutubeId"] as string) ?? "";
                    v.Percent = Math.Min(100, Convert.ToInt32(r["Pct"])); v.WatchedSeconds = Convert.ToInt32(r["Sec"]);
                    v.Started = Convert.ToInt32(r["Started"]) == 1; v.Done = Convert.ToInt32(r["Done"]) == 1;
                    if (v.Done) v.Percent = Math.Max(v.Percent, VideoCompletePercent);
                    list.Add(v);
                }
        }
        return list;
    }

    // ===================== YENIDEN GIRIS TALEPLERI =====================
    private const string REQ_SELECT =
        "SELECT q.RequestId, q.ExamId, e.Title AS ExamTitle, q.UserId, u.FullName AS UserName, u.Email AS UserEmail, c.Name AS CompanyName, " +
        "q.Note, q.Status, q.RequestedAt, q.DecidedAt, q.DecisionNote, " +
        "(SELECT TOP 1 a.Score FROM dbo.ExamAttempts a WHERE a.ExamId=q.ExamId AND a.UserId=q.UserId AND a.SubmittedAt IS NOT NULL ORDER BY a.SubmittedAt DESC) AS LastScore, " +
        "(SELECT COUNT(*) FROM dbo.ExamAttempts a WHERE a.ExamId=q.ExamId AND a.UserId=q.UserId AND a.SubmittedAt IS NOT NULL) AS Tries " +
        "FROM dbo.ExamRetakeRequests q JOIN dbo.Exams e ON e.ExamId=q.ExamId " +
        "LEFT JOIN dbo.Users u ON u.UserId=q.UserId LEFT JOIN dbo.Companies c ON c.CompanyId=q.CompanyId ";

    private static RetakeRequest ReadReq(SqlDataReader r)
    {
        RetakeRequest q = new RetakeRequest();
        q.RequestId = (int)r["RequestId"]; q.ExamId = (int)r["ExamId"]; q.ExamTitle = r["ExamTitle"] as string;
        q.UserId = (int)r["UserId"]; q.UserName = r["UserName"] as string; q.UserEmail = r["UserEmail"] as string;
        q.CompanyName = r["CompanyName"] as string; q.Note = r["Note"] as string; q.Status = r["Status"] as string;
        q.RequestedAt = (DateTime)r["RequestedAt"]; q.DecidedAt = NDt(r["DecidedAt"]); q.DecisionNote = r["DecisionNote"] as string;
        q.LastScore = NInt(r["LastScore"]); q.Tries = Convert.ToInt32(r["Tries"]);
        return q;
    }

    public static RetakeRequest GetLatestRequest(int examId, int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM (" + REQ_SELECT + "WHERE q.ExamId=@e AND q.UserId=@u) x ORDER BY x.RequestedAt DESC, x.RequestId DESC", con))
        {
            cmd.Parameters.AddWithValue("@e", examId); cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadReq(r);
        }
        return null;
    }

    public static RetakeRequest GetRequest(int requestId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(REQ_SELECT + "WHERE q.RequestId=@id", con))
        {
            cmd.Parameters.AddWithValue("@id", requestId);
            using (SqlDataReader r = cmd.ExecuteReader()) if (r.Read()) return ReadReq(r);
        }
        return null;
    }

    public static List<RetakeRequest> GetPendingRequests()
    {
        List<RetakeRequest> list = new List<RetakeRequest>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(REQ_SELECT + "WHERE q.Status=N'Pending' ORDER BY q.RequestedAt", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read()) list.Add(ReadReq(r));
        return list;
    }

    public static int PendingRequestCount()
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ExamRetakeRequests WHERE Status=N'Pending'", con))
                return (int)cmd.ExecuteScalar();
        }
        catch { return 0; }
    }

    public static int CreateRequest(int examId, int userId, int companyId, string note)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.ExamRetakeRequests (ExamId, UserId, CompanyId, Note) OUTPUT INSERTED.RequestId " +
            "SELECT @e, @u, @c, @n WHERE NOT EXISTS (SELECT 1 FROM dbo.ExamRetakeRequests WHERE ExamId=@e AND UserId=@u AND Status IN (N'Pending', N'Approved'))", con))
        {
            cmd.Parameters.AddWithValue("@e", examId); cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@n", N(string.IsNullOrEmpty(note) ? null : note));
            object o = cmd.ExecuteScalar(); return o == null ? 0 : (int)o;
        }
    }

    public static bool DecideRequest(int requestId, bool approve, int byUserId, string note)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.ExamRetakeRequests SET Status=@s, DecidedAt=SYSUTCDATETIME(), DecidedBy=@by, DecisionNote=@n WHERE RequestId=@id AND Status=N'Pending'", con))
        {
            cmd.Parameters.AddWithValue("@s", approve ? "Approved" : "Rejected"); cmd.Parameters.AddWithValue("@by", byUserId);
            cmd.Parameters.AddWithValue("@n", N(string.IsNullOrEmpty(note) ? null : note)); cmd.Parameters.AddWithValue("@id", requestId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public static void MarkRequestUsed(int requestId, int attemptId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ExamRetakeRequests SET Status=N'Used', UsedAttemptId=@a WHERE RequestId=@id AND Status=N'Approved'", con))
        { cmd.Parameters.AddWithValue("@a", attemptId); cmd.Parameters.AddWithValue("@id", requestId); cmd.ExecuteNonQuery(); }
    }

    // ===================== GIRIS SARTLARI (tek kaynak) =====================
    // Kural: tanimli deneme hakki kadar serbest giris (aradaki bekleme gecerli).
    // Haklar bitti ve gecilmediyse: yeniden giris talebi -> editor onayi -> +1 deneme.
    public static ExamGate Gate(ExamInfo e, int userId, int companyId, List<ExamAttempt> atts)
    {
        ExamGate g = new ExamGate();
        g.MaxAttempts = e.MaxAttempts;
        int done, total;
        VideoProgress(e.CategoryGroup, userId, companyId, out done, out total);
        g.VideosDone = done; g.VideosTotal = total; g.VideosOk = total == 0 || done >= total;
        DateTime? last = null;
        for (int i = 0; i < atts.Count; i++)
        {
            if (!atts[i].SubmittedAt.HasValue) { g.HasOpenAttempt = true; continue; }
            g.Submitted++;
            if (atts[i].Passed == true) g.Passed = true;
            if (!last.HasValue || atts[i].SubmittedAt.Value > last.Value) { last = atts[i].SubmittedAt.Value; g.LastScore = atts[i].Score; }
        }
        g.FreeLeft = Math.Max(0, e.MaxAttempts - g.Submitted);
        g.IsActive = e.Status == "Active";
        g.IsReady = e.QuestionCount > 0 && e.TotalPoints == 100;

        // deneme hakki durumu
        if (!g.Passed && g.Submitted >= e.MaxAttempts)
        {
            try { g.Latest = GetLatestRequest(e.ExamId, userId); } catch { g.Latest = null; }
            if (g.Latest != null && g.Latest.Status == "Approved") { g.QuotaOk = true; g.ApprovedRequestId = g.Latest.RequestId; }
            else if (g.Latest != null && g.Latest.Status == "Pending") g.Pending = true;
            else g.NeedsRequest = true;
        }
        else if (!g.Passed)
        {
            g.QuotaOk = true;
            if (last.HasValue && g.Submitted > 0 && e.CooldownHours > 0)
            {
                double h = (DateTime.UtcNow - last.Value).TotalHours;
                if (h < e.CooldownHours) { g.QuotaOk = false; g.CooldownHoursLeft = (int)Math.Ceiling(e.CooldownHours - h); }
            }
        }

        if (g.Passed) g.Reason = "Bu sınavı başarıyla tamamladınız.";
        else if (e.Status == "Archived") g.Reason = "Bu sınav arşivlendi; yeni giriş yapılamaz.";
        else if (!g.IsActive) g.Reason = "Sınav henüz girişe açılmadı.";
        else if (!g.IsReady) g.Reason = "Sınav hazırlığı tamamlanmadı.";
        else if (!g.VideosOk) g.Reason = "Eğitim videolarının tamamını izlemelisiniz (" + done + "/" + total + ").";
        else if (g.Pending) g.Reason = "Yeniden giriş talebiniz eğitim editörünün onayını bekliyor.";
        else if (g.NeedsRequest) g.Reason = "Deneme hakkınız doldu. Yeniden girmek için talep oluşturun.";
        else if (g.CooldownHoursLeft > 0) g.Reason = "Yeni deneme için yaklaşık " + g.CooldownHoursLeft + " saat beklemelisiniz.";
        g.CanStart = g.Reason.Length == 0;
        return g;
    }

    // Editor: bir denemede verilen cevaplar (QuestionId -> secilen sik id'leri) ve kazanilan puanlar
    public static void GetAttemptAnswers(int attemptId, out Dictionary<int, List<int>> picked, out Dictionary<int, int> points)
    {
        picked = new Dictionary<int, List<int>>(); points = new Dictionary<int, int>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT QuestionId, OptionIds, PointsEarned FROM dbo.ExamAnswers WHERE AttemptId=@a", con))
        {
            cmd.Parameters.AddWithValue("@a", attemptId);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    int q = (int)r["QuestionId"];
                    List<int> ids = new List<int>();
                    string s = r["OptionIds"] as string;
                    if (!string.IsNullOrEmpty(s))
                        foreach (string p in s.Split(',')) { int v; if (int.TryParse(p, out v)) ids.Add(v); }
                    picked[q] = ids; points[q] = (int)r["PointsEarned"];
                }
        }
    }

    // Geriye donuk uyumluluk: Gate uzerinden neden metni
    public static string Eligibility(ExamInfo e, int userId, int companyId, List<ExamAttempt> attempts, out int videosDone, out int videosTotal)
    {
        ExamGate g = Gate(e, userId, companyId, attempts);
        videosDone = g.VideosDone; videosTotal = g.VideosTotal;
        return g.Reason;
    }
}
