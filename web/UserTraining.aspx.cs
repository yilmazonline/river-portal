using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

// Bir kullanicinin egitim gecmisi (yonetici gorunumu): "Egitim Gecmisim" ile ayni hesap + sertifikalar + sinav sonuclari.
// Portal Yonetimi yetkisi ya da egitim editoru (editor panelindeki "En cok izleyenler" listesinden, embed=1 ile pencerede acilir).
public partial class UserTrainingPage : Page
{
    public string Culture = "tr", UserName = "", CompanyName = "";
    public UserRow U;
    public bool Embed;   // editor panelinde pencere icinde: menu ve geri baglantisi gizli
    public string ProgressHtml = "", CertHtml = "", AttemptHtml = "", PathHtml = "";
    public int StatCoursesTotal, StatCoursesDone, StatVideosTotal, StatVideosDone, StatRate, StatCerts, StatActiveDays;
    public string StatWatch = "0 dk", StatFirst = "·", StatLast = "·", RecentHtml = "";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");
    public static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.RequireVendor(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorAdmin && !Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        Embed = Request.QueryString["embed"] == "1";
        Culture = Auth.Culture; UserName = Auth.FullName; CompanyName = Auth.CompanyName;
        int id; int.TryParse(Request.QueryString["id"], out id);
        U = id > 0 ? Repo.GetUserById(id) : null;
        if (U == null) { Response.Redirect(Auth.IsVendorAdmin ? "Users.aspx" : "Training.aspx"); return; }

        string ct = (U.CompanyType ?? "").ToLowerInvariant();
        bool taker = ct == "partner" || ct == "customer";      // sinavlar bayi ve musteri tarafi icin
        List<CertificateInfo> certs = new List<CertificateInfo>();
        try { certs = ExamRepo.GetUserCertificates(U.UserId); } catch { }
        StatCerts = 0; foreach (CertificateInfo c in certs) if (!c.IsRevoked && !c.IsExpired) StatCerts++;

        // Ayrintili gorunum; sorgu calismazsa (or. eski kurulum) sade gorunume duser
        try { ProgressHtml = BuildDetailed(taker, certs); }
        catch
        {
            try { ProgressHtml = BuildProgress(taker, certs); } catch (Exception ex) { ProgressHtml = "<div class=\"ut-empty\">Eğitim ilerlemesi okunamadı: " + E(ex.Message) + "</div>"; }
        }
        try
        {
            int sec, days; DateTime? fa, la; TrainingDetailRepo.GetUserTotals(U.UserId, out sec, out days, out fa, out la);
            StatWatch = TrainingDetailRepo.Dur(sec); StatActiveDays = days;
            if (fa.HasValue) StatFirst = fa.Value.ToTrLocal().ToString("dd.MM.yyyy", TR);
            if (la.HasValue) StatLast = la.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR);
        }
        catch { }
        CertHtml = BuildCerts(certs);
        try { PathHtml = BuildPath(); } catch { PathHtml = ""; }   // Sifirdan Uzmanliga tablolari yoksa bolum gizli
        try { AttemptHtml = BuildAttempts(); } catch { AttemptHtml = ""; }
    }

    private string BuildProgress(bool taker, List<CertificateInfo> certs)
    {
        List<GroupVideo> vids = ExamRepo.GetAllVideoProgress(U.UserId, U.CompanyId);
        if (vids.Count == 0) return "<div class=\"ut-empty\">Bu kişinin erişebildiği eğitim içeriği yok.</div>";
        List<string> order = new List<string>(); Dictionary<string, List<GroupVideo>> groups = new Dictionary<string, List<GroupVideo>>();
        int pctSum = 0;
        foreach (GroupVideo v in vids)
        {
            if (!groups.ContainsKey(v.Group)) { groups[v.Group] = new List<GroupVideo>(); order.Add(v.Group); }
            groups[v.Group].Add(v); StatVideosTotal++; if (v.Done) StatVideosDone++; pctSum += v.Percent;
        }
        StatRate = (int)Math.Round(pctSum / (double)vids.Count); StatCoursesTotal = order.Count;

        Dictionary<string, ExamInfo> examByGroup = new Dictionary<string, ExamInfo>();
        Dictionary<int, CertificateInfo> certByExam = new Dictionary<int, CertificateInfo>();
        if (taker)
        {
            try
            {
                foreach (ExamInfo x in ExamRepo.GetExams(false)) if (!string.IsNullOrEmpty(x.CategoryGroup) && !examByGroup.ContainsKey(x.CategoryGroup)) examByGroup[x.CategoryGroup] = x;
                foreach (CertificateInfo c in certs) certByExam[c.ExamId] = c;
            }
            catch { }
        }
        StringBuilder sb = new StringBuilder();
        foreach (string g in order)
        {
            List<GroupVideo> gv = groups[g];
            int done = 0, started = 0, psum = 0, secs = 0;
            foreach (GroupVideo v in gv) { if (v.Done) done++; if (v.Started) started++; psum += v.Percent; secs += v.WatchedSeconds; }
            int rate = (int)Math.Round(psum / (double)gv.Count);
            string state = done == gv.Count ? "ok" : (started > 0 ? "wait" : "idle");
            if (state == "ok") StatCoursesDone++;
            string ic = state == "ok" ? "&#10003;" : state == "wait" ? "&#9654;" : "&#9675;";
            string label = state == "ok" ? "Tamamlandı" : state == "wait" ? "Devam ediyor" : "Başlanmadı";
            int mins = secs / 60;
            sb.Append("<div class=\"gc-item " + state + " hp-item\"><span class=\"gc-ic\">" + ic + "</span><div class=\"gc-tx\">");
            sb.Append("<div class=\"hp-top\"><b>" + E(g) + "</b><span class=\"hp-state " + state + "\">" + label + "</span></div>");
            sb.Append("<small>" + done + "/" + gv.Count + " video tamamlandı · izleme oranı %" + rate + (mins > 0 ? " · " + mins + " dk izlendi" : "") + "</small>");
            sb.Append("<div class=\"ex-bar\" style=\"margin-top:8px\"><i style=\"width:" + rate + "%\"></i></div>");
            ExamInfo ex;
            if (taker && examByGroup.TryGetValue(g, out ex))
            {
                CertificateInfo c; string st;
                if (certByExam.TryGetValue(ex.ExamId, out c))
                    sb.Append("<a class=\"hp-exam won\" href=\"Certificate.aspx?id=" + c.CertificateId + "\">" + ExamRepo.SealSvg(22, c.IsRevoked || c.IsExpired, c.BadgeColor) + "<span>Sertifika kazandı · " + E(ex.Title) + "</span></a>");
                else
                {
                    if (ex.Status == "Published") st = "Sınav henüz girişe açılmadı";
                    else
                    {
                        ExamGate eg = ExamRepo.Gate(ex, U.UserId, U.CompanyId, ExamRepo.GetUserAttempts(ex.ExamId, U.UserId));
                        st = eg.HasOpenAttempt ? "Devam eden sınavı var" : eg.CanStart ? "Sınava girebilir" : !eg.VideosOk ? "Sınav için videoları tamamlaması gerekiyor" :
                             eg.Pending ? "Yeniden giriş talebi onay bekliyor" : eg.NeedsRequest ? "Deneme hakkı bitti, yeniden giriş talebi gerekli" : eg.Reason;
                    }
                    sb.Append("<span class=\"hp-exam\">" + ExamRepo.SealSvg(22, true, ex.BadgeColor) + "<span>" + E(ex.Title) + " · " + E(st) + "</span></span>");
                }
            }
            if (state != "ok")
            {
                sb.Append("<details class=\"hp-det\"><summary>Kalan videolar (" + (gv.Count - done) + ")</summary>");
                foreach (GroupVideo v in gv)
                {
                    if (v.Done) continue;
                    sb.Append("<div class=\"gc-vid\"><span class=\"gc-vid-t\">" + E(v.Title) + "</span><span class=\"gc-vid-p" + (v.Started ? " part" : "") + "\">" + (v.Started ? "%" + v.Percent + " izlendi" : "Başlanmadı") + "</span></div>");
                }
                sb.Append("</details>");
            }
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }

    private static string T(DateTime? d) { return d.HasValue ? d.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) : "·"; }
    private static string MS(int s) { return (s / 60) + ":" + (s % 60).ToString("00"); }

    private string BuildDetailed(bool taker, List<CertificateInfo> certs)
    {
        HistoryStats st = new HistoryStats(); List<VideoDetail> vids;
        string html = TrainingHistoryView.Build(U.UserId, U.CompanyId, taker, certs, false, st, out vids);
        StatCoursesTotal = st.CoursesTotal; StatCoursesDone = st.CoursesDone; StatVideosTotal = st.VideosTotal; StatVideosDone = st.VideosDone; StatRate = st.Rate;
        if (vids.Count == 0) return "<div class=\"ut-empty\">Bu kişinin erişebildiği eğitim içeriği yok.</div>";
        // son etkinlikler: en son izlenen 8 video
        List<VideoDetail> recent = new List<VideoDetail>();
        foreach (VideoDetail v in vids) if (v.LastLogAt.HasValue || v.LastWatchedAt.HasValue) recent.Add(v);
        recent.Sort(delegate (VideoDetail a, VideoDetail b) { return (b.LastLogAt ?? b.LastWatchedAt).Value.CompareTo((a.LastLogAt ?? a.LastWatchedAt).Value); });
        StringBuilder rs = new StringBuilder();
        for (int i = 0; i < recent.Count && i < 8; i++)
        {
            VideoDetail v = recent[i];
            rs.Append("<div class=\"ut-rec\"><span class=\"ut-rec-t\">" + TrainingHistoryView.T(v.LastLogAt ?? v.LastWatchedAt) + "</span><div><b>" + E(v.Title) + "</b><small>" + E(v.Group) + "</small></div>" +
                      (v.Done ? "<span class=\"rc-st ok\">Tamamlandı</span>" : "<span class=\"rc-st info\">%" + v.Percent + "</span>") + "</div>");
        }
        RecentHtml = rs.Length > 0 ? rs.ToString() : "<div class=\"ut-empty\">Henüz video izlemedi.</div>";
        return html;
    }

    /// <summary>Sifirdan Uzmanliga: yoldaki ilerleme + rozetler (yalniz okuma; rozet vermez)</summary>
    private string BuildPath()
    {
        List<PathStep> steps = LearningPath.GetSteps(U.UserId, U.CompanyId);
        if (steps.Count == 0) return "";
        Dictionary<string, DateTime> got = new Dictionary<string, DateTime>();
        foreach (EarnedBadge b in LearningPath.GetBadges(U.UserId, steps)) got[b.Key] = b.EarnedAt;
        int done = 0; foreach (PathStep s in steps) if (s.State == "done") done++;
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"ut-lp-top\"><div><b>" + done + " / " + steps.Count + " adım tamamlandı</b><small>" +
                  (done == steps.Count ? "Yolu tamamladı" : (done == 0 ? "Henüz başlamadı" : "Sıradaki adım: " + E(Next(steps)))) + "</small></div>" +
                  "<div class=\"ut-lp-bar\"><i style=\"width:" + (done * 100 / steps.Count) + "%\"></i></div></div>");
        sb.Append("<div class=\"ut-lp-grid\">");
        for (int i = 0; i < steps.Count; i++)
        {
            PathStep s = steps[i]; bool on = s.State == "done"; DateTime when;
            string sub = on ? (got.TryGetValue("video:" + s.MaterialId, out when) ? when.ToTrLocal().ToString("dd.MM.yyyy", TR) : "Tamamlandı")
                            : (s.State == "next" ? (s.Skimmed ? "Hızlı geçilmiş" : (s.Percent > 0 ? "%" + s.Percent + " izlendi" : "Sıradaki")) : "Kilitli");
            sb.Append("<div class=\"ut-lp-b" + (on ? " on" : "") + (s.State == "next" ? " next" : "") + "\" title=\"" + E((i + 1) + ". " + s.Title) + "\">" +
                      LearningPath.BadgeSvg("video:" + s.MaterialId, i + 1, on, 52) + "<b>" + E(s.Title) + "</b><small>" + sub + "</small></div>");
        }
        sb.Append("</div><div class=\"ut-lp-ms\">");
        foreach (string[] m in new[] { new[] { "m:first", "İlk Adım" }, new[] { "m:half", "Yarı Yol" }, new[] { "path:done", LearningPath.PATH_NAME } })
        {
            DateTime when; bool on = got.TryGetValue(m[0], out when);
            sb.Append("<div class=\"ut-lp-b ms" + (on ? " on" : "") + "\">" + LearningPath.BadgeSvg(m[0], 0, on, 44) + "<b>" + E(m[1]) + "</b><small>" + (on ? when.ToTrLocal().ToString("dd.MM.yyyy", TR) : "Kazanılmadı") + "</small></div>");
        }
        return sb.Append("</div>").ToString();
    }
    private static string Next(List<PathStep> steps) { foreach (PathStep s in steps) if (s.State == "next") return s.Title; return ""; }

    private string BuildCerts(List<CertificateInfo> certs)
    {
        if (certs.Count == 0) return "<div class=\"ut-empty\">Henüz sertifikası yok.</div>";
        StringBuilder sb = new StringBuilder("<div class=\"ut-certs\">");
        foreach (CertificateInfo c in certs)
        {
            string st = c.IsRevoked ? "<span class=\"rc-st bad\">İptal edildi</span>" : c.IsExpired ? "<span class=\"rc-st warn\">Süresi doldu</span>" : "<span class=\"rc-st ok\">Geçerli</span>";
            sb.Append("<a class=\"ut-cert\" href=\"Certificate.aspx?id=" + c.CertificateId + "\"><span class=\"ut-seal\">" + ExamRepo.SealSvg(54, c.IsRevoked || c.IsExpired, c.BadgeColor) + "</span><div>" +
                      "<b>" + E(c.ExamTitle) + "</b><small>No: " + E(c.CertNo) + " · Veriliş: " + c.IssuedAt.ToString("dd.MM.yyyy") +
                      (c.ValidUntil.HasValue ? " · Geçerlilik: " + c.ValidUntil.Value.ToString("dd.MM.yyyy") : " · Süresiz") + "</small>" + st + "</div></a>");
        }
        return sb.Append("</div>").ToString();
    }

    private string BuildAttempts()
    {
        List<KeyValuePair<ExamInfo, ExamAttempt>> rows = new List<KeyValuePair<ExamInfo, ExamAttempt>>();
        foreach (ExamInfo x in ExamRepo.GetExams(false))
            foreach (ExamAttempt a in ExamRepo.GetUserAttempts(x.ExamId, U.UserId)) rows.Add(new KeyValuePair<ExamInfo, ExamAttempt>(x, a));
        if (rows.Count == 0) return "<div class=\"ut-empty\">Henüz sınava girmedi.</div>";
        rows.Sort(delegate (KeyValuePair<ExamInfo, ExamAttempt> p, KeyValuePair<ExamInfo, ExamAttempt> q) { return q.Value.StartedAt.CompareTo(p.Value.StartedAt); });
        bool detail = Auth.CanEditTraining;
        StringBuilder sb = new StringBuilder("<div class=\"tbl-scroll\"><table class=\"tk-table\"><thead><tr><th>Sınav</th><th>Tarih</th><th style=\"text-align:right\">Puan</th><th>Sonuç</th>" + (detail ? "<th></th>" : "") + "</tr></thead><tbody>");
        foreach (KeyValuePair<ExamInfo, ExamAttempt> r in rows)
        {
            ExamAttempt a = r.Value;
            string res = !a.SubmittedAt.HasValue ? "<span class=\"rc-st info\">Devam ediyor</span>" : (a.Passed == true ? "<span class=\"rc-st ok\">Geçti</span>" : "<span class=\"rc-st bad\">Kaldı</span>");
            sb.Append("<tr><td>" + E(r.Key.Title) + "</td><td>" + a.StartedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) + "</td><td style=\"text-align:right\">" + (a.Score.HasValue ? a.Score.Value.ToString() : "·") + "</td><td>" + res + "</td>" +
                      (detail ? "<td style=\"text-align:right\">" + (a.SubmittedAt.HasValue ? "<a class=\"rc-link\" href=\"ExamAttempt.aspx?id=" + a.AttemptId + "\">Cevaplar</a>" : "") + "</td>" : "") + "</tr>");
        }
        return sb.Append("</tbody></table></div>").ToString();
    }
}
