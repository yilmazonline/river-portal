using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

// Egitim gecmisi gorunumu: kisinin kendi "Egitim Gecmisim" penceresi (self=true) ve yonetici sayfasi (self=false) AYNI hesabi kullanir.
public class HistoryStats { public int CoursesTotal, CoursesDone, VideosTotal, VideosDone, Rate, Skimmed; }

public static class TrainingHistoryView
{
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");
    private static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }
    public static string T(DateTime? d) { return d.HasValue ? d.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) : "·"; }
    private static string MS(int s) { return (s / 60) + ":" + (s % 60).ToString("00"); }
    public const string SKIM_TIP = "Videonun yarısından azı izlenmiş görünüyor (sona sarılmış olabilir).";

    public static string Build(int userId, int companyId, bool taker, List<CertificateInfo> certs, bool self, HistoryStats st, out List<VideoDetail> vids)
    {
        vids = TrainingDetailRepo.GetUserVideoDetail(userId, companyId);
        if (vids.Count == 0) return "";
        List<string> order = new List<string>(); Dictionary<string, List<VideoDetail>> groups = new Dictionary<string, List<VideoDetail>>();
        int pctSum = 0;
        foreach (VideoDetail v in vids)
        {
            if (!groups.ContainsKey(v.Group)) { groups[v.Group] = new List<VideoDetail>(); order.Add(v.Group); }
            groups[v.Group].Add(v); st.VideosTotal++; if (v.Done) st.VideosDone++; pctSum += v.Percent; if (v.Skimmed) st.Skimmed++;
        }
        st.Rate = (int)Math.Round(pctSum / (double)vids.Count); st.CoursesTotal = order.Count;

        Dictionary<string, ExamInfo> examByGroup = new Dictionary<string, ExamInfo>();
        Dictionary<int, CertificateInfo> certByExam = new Dictionary<int, CertificateInfo>();
        if (taker)
        {
            try
            {
                foreach (ExamInfo x in ExamRepo.GetExams(false)) if (!string.IsNullOrEmpty(x.CategoryGroup) && !examByGroup.ContainsKey(x.CategoryGroup)) examByGroup[x.CategoryGroup] = x;
                if (certs != null) foreach (CertificateInfo c in certs) certByExam[c.ExamId] = c;
            }
            catch { }
        }
        StringBuilder sb = new StringBuilder();
        foreach (string g in order)
        {
            List<VideoDetail> gv = groups[g];
            int done = 0, started = 0, psum = 0, logSec = 0, durSum = 0, skim = 0; DateTime? last = null;
            foreach (VideoDetail v in gv)
            {
                if (v.Done) done++; if (v.Started) started++; psum += v.Percent; logSec += v.WatchLogSeconds; durSum += v.DurationSeconds; if (v.Skimmed) skim++;
                DateTime? l = v.LastLogAt ?? v.LastWatchedAt; if (l.HasValue && (!last.HasValue || l.Value > last.Value)) last = l;
            }
            int rate = (int)Math.Round(psum / (double)gv.Count);
            string state = done == gv.Count ? "ok" : (started > 0 ? "wait" : "idle");
            if (state == "ok") st.CoursesDone++;
            string ic = state == "ok" ? "&#10003;" : state == "wait" ? "&#9654;" : "&#9675;";
            string label = state == "ok" ? "Tamamlandı" : state == "wait" ? "Devam ediyor" : "Başlanmadı";
            sb.Append("<div class=\"gc-item " + state + " hp-item\"><span class=\"gc-ic\">" + ic + "</span><div class=\"gc-tx\">");
            sb.Append("<div class=\"hp-top\"><b>" + E(g) + "</b><span class=\"hp-state " + state + "\">" + label + "</span></div>");
            sb.Append("<small>" + done + "/" + gv.Count + " video tamamlandı · izleme oranı %" + rate + "</small>");
            sb.Append("<div class=\"ex-bar\" style=\"margin-top:8px\"><i style=\"width:" + rate + "%\"></i></div>");
            sb.Append("<div class=\"ut-gmeta\"><span>" + (self ? "İzlediğiniz süre" : "Gerçek izleme") + " <b>" + TrainingDetailRepo.Dur(logSec) + "</b>" + (durSum > 0 ? " / toplam video süresi " + TrainingDetailRepo.Dur(durSum) : "") + "</span>" +
                      "<span>Son izleme <b>" + T(last) + "</b></span>" + (skim > 0 && !self ? "<span class=\"rc-st warn\" title=\"" + SKIM_TIP + "\">" + skim + " video hızlı geçilmiş olabilir</span>" : "") + "</div>");
            if (skim > 0 && self)
                sb.Append("<div class=\"ut-skim\"><b>" + skim + " video hızlı geçilmiş görünüyor.</b> Bu videoların yarısından azını izlemişsiniz; bu durum sertifika sınavı hakkınızı etkileyebilir. İşaretli videoları baştan sona izlemenizi öneririz.</div>");

            ExamInfo ex;
            if (taker && examByGroup.TryGetValue(g, out ex))
            {
                CertificateInfo c; string tx;
                if (certByExam.TryGetValue(ex.ExamId, out c))
                    sb.Append("<a class=\"hp-exam won\" href=\"Certificate.aspx?id=" + c.CertificateId + "\">" + ExamRepo.SealSvg(22, c.IsRevoked || c.IsExpired, c.BadgeColor) + "<span>Sertifika " + (self ? "kazandınız" : "kazandı") + " · " + E(ex.Title) + " · " + c.IssuedAt.ToString("dd.MM.yyyy") + "</span></a>");
                else
                {
                    if (ex.Status == "Published") tx = self ? "Sınav yakında girişe açılacak" : "Sınav henüz girişe açılmadı";
                    else
                    {
                        ExamGate eg = ExamRepo.Gate(ex, userId, companyId, ExamRepo.GetUserAttempts(ex.ExamId, userId));
                        tx = self
                            ? (eg.HasOpenAttempt ? "Devam eden sınavınız var" : eg.CanStart ? "Sınava girebilirsiniz" : !eg.VideosOk ? "Videoları tamamlayınca sınava girebilirsiniz" :
                               eg.Pending ? "Yeniden giriş talebiniz onay bekliyor" : eg.NeedsRequest ? "Yeniden giriş talebi gerekli" : eg.Reason)
                            : (eg.HasOpenAttempt ? "Devam eden sınavı var" : eg.CanStart ? "Sınava girebilir" : !eg.VideosOk ? "Sınav için videoları tamamlaması gerekiyor" :
                               eg.Pending ? "Yeniden giriş talebi onay bekliyor" : eg.NeedsRequest ? "Deneme hakkı bitti, yeniden giriş talebi gerekli" : eg.Reason);
                    }
                    if (self) sb.Append("<a class=\"hp-exam\" href=\"ExamTake.aspx?id=" + ex.ExamId + "\">" + ExamRepo.SealSvg(22, false, ex.BadgeColor) + "<span>" + E(ex.Title) + " · " + E(tx) + "</span></a>");
                    else sb.Append("<span class=\"hp-exam\">" + ExamRepo.SealSvg(22, true, ex.BadgeColor) + "<span>" + E(ex.Title) + " · " + E(tx) + "</span></span>");
                }
            }

            // video tablosu: tamamlananlar dahil
            bool open = self ? (state == "wait" || skim > 0) : state != "idle";
            // kisinin kendi penceresi dar: 4 sutun (kaldigi yer durum sutununda); yonetici sayfasi: 6 sutun
            sb.Append("<details class=\"hp-det\"" + (open ? " open" : "") + "><summary>Videolar (" + gv.Count + ")</summary><div class=\"tbl-scroll\"><table class=\"ut-vt" + (self ? " self" : "") + "\"><thead><tr>" +
                      (self ? "<th>Video</th><th>Durum</th><th>İzlediğiniz süre</th><th></th>"
                            : "<th>Video</th><th>Durum</th><th>Gerçek izleme</th><th>Son kalınan yer</th><th>Son izleme</th><th>Puanı</th>") + "</tr></thead><tbody>");
            foreach (VideoDetail v in gv)
            {
                string stt = v.Done ? "<span class=\"rc-st ok\">Tamamlandı</span>" : v.Started ? "<span class=\"rc-st info\">%" + v.Percent + " izlendi</span>" : "<span class=\"rc-st mute\">Başlanmadı</span>";
                if (v.Skimmed) stt += " <span class=\"rc-st warn\" title=\"" + SKIM_TIP + "\">Hızlı geçilmiş olabilir</span>";
                string watch = v.WatchLogSeconds > 0 ? TrainingDetailRepo.Dur(v.WatchLogSeconds) + (v.DurationSeconds > 0 ? " <small>/ " + TrainingDetailRepo.Dur(v.DurationSeconds) + "</small>" : "") + (v.ActiveDays > 1 ? "<small> · " + v.ActiveDays + " gün</small>" : "") : (v.Started ? "<small>Kayıt yok</small>" : "·");
                string pos = v.Started && !v.Done && v.LastPosition > 0 ? MS(v.LastPosition) + (v.DurationSeconds > 0 ? " <small>/ " + MS(v.DurationSeconds) + "</small>" : "") : "·";
                string lastCol;
                if (self) lastCol = v.YoutubeId.Length > 0 ? "<span class=\"ut-go\">" + (v.Done && !v.Skimmed ? "Tekrar izle" : (v.Skimmed ? "Baştan izle" : (v.Started ? "Devam et" : "İzle"))) + " &#8594;</span>" : "";
                else lastCol = v.Stars.HasValue ? "<span class=\"ut-star\"" + (string.IsNullOrEmpty(v.Comment) ? "" : " title=\"" + E(v.Comment) + "\"") + ">★ " + v.Stars.Value + "</span>" : "·";
                string trAttr = self && v.YoutubeId.Length > 0 ? " class=\"hist-open ut-click\" data-vid=\"" + E(v.YoutubeId) + "\" data-mid=\"" + v.MaterialId + "\" data-title=\"" + E(v.Title) + "\"" : "";
                if (self)
                {
                    if (v.Started && !v.Done && v.LastPosition > 0) stt += "<small class=\"ut-pos\">" + MS(v.LastPosition) + "'te kaldınız</small>";
                    sb.Append("<tr" + trAttr + "><td><b>" + E(v.Title) + "</b></td><td class=\"ut-st\">" + stt + "</td><td>" + watch + "</td><td>" + lastCol + "</td></tr>");
                }
                else
                    sb.Append("<tr" + trAttr + "><td><b>" + E(v.Title) + "</b>" + (!string.IsNullOrEmpty(v.Comment) ? "<small class=\"ut-cm\">“" + E(v.Comment) + "”</small>" : "") + "</td><td>" + stt + "</td><td>" + watch + "</td><td>" + pos + "</td><td>" + T(v.LastLogAt ?? v.LastWatchedAt) + "</td><td>" + lastCol + "</td></tr>");
            }
            sb.Append("</tbody></table></div></details></div></div>");
        }
        return sb.ToString();
    }
}
