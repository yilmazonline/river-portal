using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Akademi > Sinavlar: editor tum sinavlari yonetir; bayi/musteri yayindaki ve aktif sinavlari gorur.
public partial class ExamsPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public bool CanEdit = false;
    public bool IsTaker = false;
    public string CardsHtml = "";
    public string FlashMsg = "";
    public string RequestsHtml = "";
    public int PendingCount = 0;
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        Culture = Auth.Culture; UserName = Auth.FullName;
        CanEdit = Auth.CanEditTraining;
        IsTaker = Auth.IsPartnerSide || Auth.IsCustomerSide;
        // Bayi/musteri icin sinavlar Akademi'deki "Rozetlerim" alanindan yonetilir
        if (IsTaker) { Response.Redirect("Training.aspx"); return; }
        string okq = Request.QueryString["ok"];
        if (okq == "deleted") FlashMsg = "Sınav silindi.";
        else if (okq == "approved") FlashMsg = "Talep onaylandı; kullanıcıya e-posta ile bildirildi.";
        else if (okq == "rejected") FlashMsg = "Talep reddedildi; kullanıcıya e-posta ile bildirildi.";
        if (CanEdit && string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Decide(); if (Response.IsRequestBeingRedirected) return; }
        if (CanEdit) RequestsHtml = BuildRequests();

        List<ExamInfo> list;
        try { list = ExamRepo.GetExams(CanEdit); }
        catch { CardsHtml = "<div class=\"ex-empty\">Sınav altyapısı henüz kurulmadı (database\\30_exams.sql çalıştırılmalı).</div>"; return; }
        CardsHtml = Build(list);
    }

    private string Build(List<ExamInfo> list)
    {
        if (list.Count == 0)
            return "<div class=\"ex-empty\">" + (CanEdit ? "Henüz sınav oluşturulmadı. \u201c+ Sınav Oluştur\u201d ile başlayın." : "Şu an yayında bir sınav bulunmuyor.") + "</div>";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            ExamInfo x = list[i];
            sb.Append("<div class=\"ex-card\">");
            sb.Append("<div class=\"ex-card-top\"><span class=\"st " + ExamRepo.StatusClass(x.Status) + "\">" + ExamRepo.StatusLabel(x.Status) + "</span>");
            sb.Append("<span class=\"ex-q\">" + x.QuestionCount + " soru" + (CanEdit ? " · " + x.TotalPoints + "/100 puan" : "") + "</span></div>");
            sb.Append("<div class=\"ex-title-row\">" + ExamRepo.SealSvg(40, false, x.BadgeColor) + "<div class=\"ex-title\">" + Fmt.Enc(x.Title) + "</div></div>");
            if (!string.IsNullOrEmpty(x.Description)) sb.Append("<div class=\"ex-desc\">" + Fmt.Enc(x.Description) + "</div>");
            sb.Append("<div class=\"ex-rules\">Geçme puanı " + x.PassScore + " · " + x.MaxAttempts + " deneme" +
                (x.TimeLimitMin.HasValue ? " · " + x.TimeLimitMin.Value + " dk" : "") + "</div>");

            if (CanEdit)
            {
                sb.Append("<div class=\"ex-act\"><a class=\"btn-cyan\" href=\"ExamEdit.aspx?id=" + x.ExamId + "\">Düzenle</a>" +
                    "<a class=\"btn-ghost\" href=\"ExamTake.aspx?id=" + x.ExamId + "\">Önizle</a></div>");
            }
            else if (IsTaker) sb.Append(TakerBlock(x));
            else sb.Append("<div class=\"ex-act\"><a class=\"btn-ghost\" href=\"ExamTake.aspx?id=" + x.ExamId + "\">Görüntüle</a></div>");
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    // ---------- Yeniden giris talepleri (editor) ----------
    private void Decide()
    {
        var f = Request.Unvalidated.Form;
        int rid; int.TryParse(f["rid"], out rid);
        bool approve = f["decision"] == "approve";
        string note = (f["note"] ?? "").Trim();
        if (note.Length > 1000) note = note.Substring(0, 1000);
        RetakeRequest q = rid > 0 ? ExamRepo.GetRequest(rid) : null;
        if (q == null || !ExamRepo.DecideRequest(rid, approve, Auth.UserId, note)) { Response.Redirect("Exams.aspx#requests"); return; }
        // Karar kullaniciya kisisel e-posta ile iletilir (firma geneli bildirim: basarisizlik bilgisi paylasilmasin)
        try
        {
            if (Mailer.HasSmtp && !string.IsNullOrEmpty(q.UserEmail))
            {
                string b = (System.Web.Configuration.WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
                if (b.Length == 0) b = Request.Url.GetLeftPart(UriPartial.Authority);
                string url = b.TrimEnd('/') + "/ExamTake.aspx?id=" + q.ExamId;
                string title = approve ? "Yeniden sınav talebiniz onaylandı" : "Yeniden sınav talebiniz hakkında";
                string intro = approve
                    ? "“" + q.ExamTitle + "” sınavı için yeniden giriş talebiniz onaylandı. Size bir deneme hakkı daha tanımlandı; dilediğiniz zaman sınava girebilirsiniz."
                    : "“" + q.ExamTitle + "” sınavı için yeniden giriş talebiniz şu an için onaylanmadı.";
                if (note.Length > 0) intro += "\n\nEditör notu: " + note;
                string html = Emails.Notice(title, intro, new string[] { "Sınav", q.ExamTitle }, url, approve ? "Sınava Git" : "Akademi'ye Git");
                string err; Mailer.SendTo(q.UserEmail, "River Akademi · " + title, html, out err);
            }
        }
        catch { }
        Response.Redirect("Exams.aspx?ok=" + (approve ? "approved" : "rejected") + "#requests");
    }

    private string BuildRequests()
    {
        List<RetakeRequest> list;
        try { list = ExamRepo.GetPendingRequests(); } catch { return ""; }
        PendingCount = list.Count;
        if (list.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            RetakeRequest q = list[i];
            sb.Append("<div class=\"rq-item\"><div class=\"rq-main\"><div class=\"rq-who\"><b>" + Fmt.Enc(q.UserName) + "</b><span>" + Fmt.Enc(q.CompanyName) + "</span></div>" +
                "<div class=\"rq-exam\">" + Fmt.Enc(q.ExamTitle) + "</div>" +
                "<div class=\"rq-meta\">" + q.Tries + " deneme · son puan " + (q.LastScore.HasValue ? q.LastScore.Value.ToString() : "-") +
                " · " + q.RequestedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) + "</div>" +
                (string.IsNullOrEmpty(q.Note) ? "" : "<div class=\"rq-note\">\u201c" + Fmt.Enc(q.Note) + "\u201d</div>") + "</div>" +
                "<form method=\"post\" class=\"rq-form\" data-noload=\"1\"><input type=\"hidden\" name=\"rid\" value=\"" + q.RequestId + "\" />" +
                "<input class=\"input\" name=\"note\" maxlength=\"1000\" placeholder=\"Kullanıcıya not (opsiyonel)\" />" +
                "<div class=\"rq-btns\"><button class=\"btn-cyan\" type=\"submit\" name=\"decision\" value=\"approve\">Onayla</button>" +
                "<button class=\"btn-ghost eq-del\" type=\"submit\" name=\"decision\" value=\"reject\" onclick=\"return confirm('Talep reddedilsin mi?')\">Reddet</button></div></form></div>");
        }
        return sb.ToString();
    }

    private string TakerBlock(ExamInfo x)
    {
        StringBuilder sb = new StringBuilder();
        CertificateInfo cert = ExamRepo.GetUserExamCertificate(Auth.UserId, x.ExamId);
        List<ExamAttempt> atts = ExamRepo.GetUserAttempts(x.ExamId, Auth.UserId);
        int done, total;
        string why = ExamRepo.Eligibility(x, Auth.UserId, Auth.CompanyId, atts, out done, out total);
        if (total > 0)
        {
            int pct = (int)(done * 100.0 / total);
            sb.Append("<div class=\"ex-prog\"><div class=\"ex-prog-h\"><span>" + Fmt.Enc(x.CategoryGroup) + " videoları</span><b>" + done + "/" + total + "</b></div>" +
                "<div class=\"ex-bar\"><i style=\"width:" + pct + "%\"></i></div></div>");
        }
        int used = 0; for (int i = 0; i < atts.Count; i++) if (atts[i].SubmittedAt.HasValue) used++;
        if (used > 0) sb.Append("<div class=\"ex-rules\">Kullanılan deneme: " + used + "/" + x.MaxAttempts + "</div>");

        sb.Append("<div class=\"ex-act\">");
        if (cert != null) sb.Append("<a class=\"btn-cyan\" href=\"Certificate.aspx?id=" + cert.CertificateId + "\">Sertifikayı Gör</a>");
        else if (x.Status == "Published") sb.Append("<a class=\"btn-ghost\" href=\"ExamTake.aspx?id=" + x.ExamId + "\">Sınavı İncele</a>");
        else if (why.Length == 0) sb.Append("<a class=\"btn-cyan\" href=\"ExamTake.aspx?id=" + x.ExamId + "\">Sınava Başla</a>");
        else sb.Append("<span class=\"btn-ghost ex-lock\">Kilitli</span>");
        sb.Append("</div>");
        if (cert == null && why.Length > 0 && x.Status == "Active") sb.Append("<div class=\"ex-why\">" + Fmt.Enc(why) + "</div>");
        return sb.ToString();
    }
}
