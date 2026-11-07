using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Sinav sayfasi.
// Kullanici (bayi/musteri): sartlar listesi -> giris -> cozme -> sonuc (yalniz puan) -> isim teyidi -> sertifika.
// Sorular kullaniciya YALNIZ cozme sirasinda (ve sinav "Yayinda" iken, hic girilmemisse salt okunur) gosterilir; sinav sonrasi asla.
// Dogru/yanlis bilgisi kullaniciya hic gosterilmez. Uretici tarafi salt okunur gorur (editor dogru cevaplarla).
public partial class ExamTakePage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public ExamInfo X = null;
    public string Mode = "view";          // view | gate | take | result | name | done
    public bool CanEdit = false;
    public bool IsTaker = false;
    public ExamGate G = new ExamGate();
    public string QuestionsHtml = "";
    public string ChecklistHtml = "";
    public ExamAttempt A = null;
    public CertificateInfo Cert = null;
    public string CertName = "";
    public long DeadlineMs = 0;
    public string FlashMsg = "";
    public string WatermarkCss = "";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        Culture = Auth.Culture; UserName = Auth.FullName;
        CanEdit = Auth.CanEditTraining;
        IsTaker = Auth.IsPartnerSide || Auth.IsCustomerSide;

        int id; int.TryParse(Request.QueryString["id"], out id);
        X = id > 0 ? ExamRepo.GetExam(id) : null;
        if (X == null || (X.Status == "Draft" && !CanEdit)) { Response.Redirect(IsTaker ? "Training.aspx" : "Exams.aspx"); return; }

        if (Request.QueryString["ok"] == "req") FlashMsg = "Yeniden giriş talebiniz eğitim editörlerine iletildi. Onaylandığında e-posta ile bilgilendirileceksiniz.";

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) && IsTaker) { HandlePost(); if (Response.IsRequestBeingRedirected) return; }

        // Uretici tarafi: salt okunur (editor dogru cevaplari gorur)
        if (!IsTaker) { Mode = "view"; QuestionsHtml = ReadOnly(ExamRepo.GetQuestions(X.ExamId), CanEdit); return; }

        List<ExamAttempt> atts = ExamRepo.GetUserAttempts(X.ExamId, Auth.UserId);
        Cert = ExamRepo.GetUserExamCertificate(Auth.UserId, X.ExamId);

        // Suresi dolmus acik deneme -> kapat
        for (int i = 0; i < atts.Count; i++)
            if (!atts[i].SubmittedAt.HasValue && atts[i].DeadlineAt.HasValue && DateTime.UtcNow > atts[i].DeadlineAt.Value.AddSeconds(90))
            { ExamRepo.ExpireAttempt(atts[i].AttemptId); Response.Redirect("ExamTake.aspx?id=" + X.ExamId + "&r=" + atts[i].AttemptId); return; }

        G = ExamRepo.Gate(X, Auth.UserId, Auth.CompanyId, atts);

        // Sonuc ekrani (yalniz kendi teslim edilmis denemesi) - soru/cevap icermez
        int rid; int.TryParse(Request.QueryString["r"], out rid);
        if (rid > 0)
        {
            ExamAttempt r = ExamRepo.GetAttempt(rid);
            if (r != null && r.UserId == Auth.UserId && r.ExamId == X.ExamId && r.SubmittedAt.HasValue)
            {
                A = r;
                if (r.Passed == true && Cert == null) { Mode = "name"; CertName = ExamRepo.GetCertificateName(Auth.UserId) ?? Auth.FullName; }
                else Mode = "result";
                return;
            }
        }

        if (Cert != null) { Mode = "done"; return; }
        for (int i = 0; i < atts.Count; i++)
            if (atts[i].Passed == true) { A = atts[i]; Mode = "name"; CertName = ExamRepo.GetCertificateName(Auth.UserId) ?? Auth.FullName; return; }

        // Devam eden deneme -> cozme ekrani
        ExamAttempt open = null;
        for (int i = 0; i < atts.Count; i++) if (!atts[i].SubmittedAt.HasValue) open = atts[i];
        if (open != null && X.Status == "Active")
        {
            A = open; Mode = "take";
            if (open.DeadlineAt.HasValue) DeadlineMs = (long)(open.DeadlineAt.Value - new DateTime(1970, 1, 1)).TotalMilliseconds;
            QuestionsHtml = TakeForm(ExamRepo.GetQuestions(X.ExamId), open.AttemptId);
            WatermarkCss = Watermark(Auth.FullName + " · " + DateTime.UtcNow.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR));
            return;
        }

        // Yayinda (henuz aktif degil) ve hic girilmemis: salt okunur on inceleme
        if (X.Status == "Published" && G.Submitted == 0) { Mode = "view"; QuestionsHtml = ReadOnly(ExamRepo.GetQuestions(X.ExamId), false); ChecklistHtml = Checklist(); return; }

        Mode = "gate";
        ChecklistHtml = Checklist();
    }

    private void HandlePost()
    {
        var f = Request.Unvalidated.Form;
        string action = f["action"] ?? "";
        string self = "ExamTake.aspx?id=" + X.ExamId;
        List<ExamAttempt> atts = ExamRepo.GetUserAttempts(X.ExamId, Auth.UserId);
        ExamGate g = ExamRepo.Gate(X, Auth.UserId, Auth.CompanyId, atts);

        if (action == "start")
        {
            if (g.HasOpenAttempt || !g.CanStart) { Response.Redirect(self); return; }
            int aid = ExamRepo.StartAttempt(X.ExamId, Auth.UserId, Auth.CompanyId, X.TimeLimitMin);
            if (g.Submitted >= X.MaxAttempts && g.ApprovedRequestId > 0) ExamRepo.MarkRequestUsed(g.ApprovedRequestId, aid);
            Response.Redirect(self); return;
        }
        if (action == "submit")
        {
            int aid; int.TryParse(f["attempt"], out aid);
            ExamAttempt a = aid > 0 ? ExamRepo.GetAttempt(aid) : null;
            if (a == null || a.UserId != Auth.UserId || a.ExamId != X.ExamId || a.SubmittedAt.HasValue) { Response.Redirect(self); return; }
            if (a.DeadlineAt.HasValue && DateTime.UtcNow > a.DeadlineAt.Value.AddSeconds(90)) { ExamRepo.ExpireAttempt(aid); Response.Redirect(self + "&r=" + aid); return; }
            List<ExamQuestion> qs = ExamRepo.GetQuestions(X.ExamId);
            Dictionary<int, List<int>> sel = new Dictionary<int, List<int>>();
            for (int i = 0; i < qs.Count; i++)
            {
                string[] vals = f.GetValues("q_" + qs[i].QuestionId);
                List<int> picked = new List<int>();
                if (vals != null)
                    for (int k = 0; k < vals.Length; k++)
                    {
                        int oid; if (!int.TryParse(vals[k], out oid)) continue;
                        for (int j = 0; j < qs[i].Options.Count; j++) if (qs[i].Options[j].OptionId == oid && !picked.Contains(oid)) picked.Add(oid);
                    }
                if (qs[i].QType != "multi" && picked.Count > 1) picked = new List<int> { picked[0] };
                sel[qs[i].QuestionId] = picked;
            }
            bool passed;
            ExamRepo.GradeAndSubmit(aid, qs, sel, X.PassScore, out passed);
            Response.Redirect(self + "&r=" + aid); return;
        }
        if (action == "request")
        {
            if (!g.NeedsRequest || !g.VideosOk || !g.IsActive) { Response.Redirect(self); return; }
            string note = (f["note"] ?? "").Trim();
            if (note.Length > 1000) note = note.Substring(0, 1000);
            int rq = ExamRepo.CreateRequest(X.ExamId, Auth.UserId, Auth.CompanyId, note);
            if (rq > 0)
                try
                {
                    int vid = Repo.GetVendorCompanyId();
                    if (vid > 0) Repo.AddNotification(vid, "Yeniden sınav talebi: " + X.Title, "Exam retake request: " + X.Title,
                        Auth.FullName + " (" + Auth.CompanyName + ") yeniden giriş onayı bekliyor.", Auth.FullName + " is waiting for retake approval.",
                        "ExamRequest", "Exams.aspx#requests");
                }
                catch { }
            Response.Redirect(self + "&ok=req"); return;
        }
        if (action == "issue")
        {
            ExamAttempt passedAtt = null;
            for (int i = 0; i < atts.Count; i++) if (atts[i].Passed == true) { passedAtt = atts[i]; break; }
            if (passedAtt == null) { Response.Redirect(self); return; }
            string name = (f["certName"] ?? "").Trim();
            if (name.Length < 3 || f["confirmName"] == null) { Response.Redirect(self + "&r=" + passedAtt.AttemptId); return; }
            if (name.Length > 160) name = name.Substring(0, 160);
            try { ExamRepo.SetCertificateName(Auth.UserId, name); } catch { }
            string companyName = "";
            try { CompanyInfo co = Repo.GetCompany(Auth.CompanyId); if (co != null) companyName = co.Name; } catch { }
            CertificateInfo c = ExamRepo.IssueCertificate(Auth.UserId, Auth.CompanyId, X, passedAtt.AttemptId, passedAtt.Score ?? 0, name, companyName);
            Response.Redirect("Certificate.aspx?id=" + c.CertificateId + "&new=1"); return;
        }
        Response.Redirect(self);
    }

    // ---------- Giris sartlari listesi ----------
    private string Item(string state, string title, string sub, string extra)
    {
        string ic = state == "ok" ? "&#10003;" : state == "wait" ? "&#8987;" : "&#10005;";
        return "<div class=\"gc-item " + state + "\"><span class=\"gc-ic\">" + ic + "</span><div class=\"gc-tx\"><b>" + title + "</b>" +
            (sub.Length > 0 ? "<small>" + sub + "</small>" : "") + extra + "</div></div>";
    }

    private string Checklist()
    {
        StringBuilder sb = new StringBuilder();
        // 1) Sinav girise acik
        if (X.Status == "Published") sb.Append(Item("wait", "Sınav henüz girişe açılmadı", "Eğitim editörü sınavı aktive ettiğinde giriş yapabilirsiniz.", ""));
        else if (X.Status == "Archived") sb.Append(Item("no", "Sınav arşivlendi", "Bu sınava yeni giriş yapılamaz.", ""));
        else if (!G.IsReady) sb.Append(Item("wait", "Sınav hazırlanıyor", "", ""));
        else sb.Append(Item("ok", "Sınav girişe açık", X.QuestionCount + " soru · geçme puanı " + X.PassScore + (X.TimeLimitMin.HasValue ? " · " + X.TimeLimitMin.Value + " dk" : " · süre sınırı yok"), ""));

        // 2) Egitim videolari
        if (G.VideosTotal > 0)
        {
            int pct = (int)(G.VideosDone * 100.0 / G.VideosTotal);
            string bar = "<div class=\"ex-bar\" style=\"margin-top:8px\"><i style=\"width:" + pct + "%\"></i></div>";
            if (G.VideosOk) sb.Append(Item("ok", "\u201c" + Fmt.Enc(X.CategoryGroup) + "\u201d eğitim videolarının tamamı izlendi", G.VideosDone + "/" + G.VideosTotal + " video", bar));
            else
            {
                StringBuilder miss = new StringBuilder("<div class=\"gc-miss\"><div class=\"gc-miss-h\">İzlenmesi gereken videolar</div>");
                List<ExamVideo> vs; try { vs = ExamRepo.GetGroupVideos(X.CategoryGroup, Auth.UserId, Auth.CompanyId); } catch { vs = new List<ExamVideo>(); }
                for (int i = 0; i < vs.Count; i++)
                {
                    if (vs[i].Done) continue;
                    miss.Append("<a class=\"gc-vid\" href=\"Training.aspx#play-" + vs[i].MaterialId + "\"><span class=\"gc-vid-t\">" + Fmt.Enc(vs[i].Title) + "</span>" +
                        "<span class=\"gc-vid-p\">" + (vs[i].Percent > 0 ? "%" + vs[i].Percent + " izlendi" : "İzlenmedi") + "</span><span class=\"gc-vid-go\">İzle &#8594;</span></a>");
                }
                miss.Append("<div class=\"gc-miss-f\">Bir video en az %" + ExamRepo.VideoCompletePercent + " izlendiğinde tamamlanmış sayılır.</div></div>");
                sb.Append(Item("no", "\u201c" + Fmt.Enc(X.CategoryGroup) + "\u201d eğitim videolarının tamamını izleyin", G.VideosDone + "/" + G.VideosTotal + " video tamamlandı", bar + miss.ToString()));
            }
        }

        // 3) Deneme hakki / yeniden giris onayi
        if (G.Submitted < X.MaxAttempts)
        {
            string sub = G.Submitted == 0 ? X.MaxAttempts + " deneme hakkınız var" : "Kalan deneme hakkı: " + G.FreeLeft + "/" + X.MaxAttempts +
                (G.LastScore.HasValue ? " · son denemeniz " + G.LastScore.Value + " puan" : "");
            if (G.CooldownHoursLeft > 0) sb.Append(Item("wait", "Yeni deneme için yaklaşık " + G.CooldownHoursLeft + " saat bekleyin", sub + " · denemeler arası " + X.CooldownHours + " saat", ""));
            else sb.Append(Item("ok", "Deneme hakkınız var", sub, ""));
        }
        else if (G.ApprovedRequestId > 0) sb.Append(Item("ok", "Yeniden giriş onaylandı", "Eğitim editörü size bir deneme hakkı daha tanıdı.", ""));
        else if (G.Pending) sb.Append(Item("wait", "Yeniden giriş talebiniz onay bekliyor",
            "Talep tarihi: " + G.Latest.RequestedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) + ". Onaylandığında e-posta ile bilgilendirileceksiniz.", ""));
        else
        {
            string rej = (G.Latest != null && G.Latest.Status == "Rejected")
                ? "<div class=\"gc-rej\"><b>Önceki talebiniz reddedildi.</b>" + (string.IsNullOrEmpty(G.Latest.DecisionNote) ? "" : " Not: " + Fmt.Enc(G.Latest.DecisionNote)) + "</div>" : "";
            string form = (G.VideosOk && G.IsActive) ?
                "<form method=\"post\" class=\"gc-req\"><input type=\"hidden\" name=\"action\" value=\"request\" />" +
                "<textarea class=\"input\" name=\"note\" maxlength=\"1000\" placeholder=\"Editöre not (opsiyonel), örneğin hangi konuları tekrar çalıştığınız\"></textarea>" +
                "<button class=\"btn-cyan\" type=\"submit\">Yeniden Giriş Talebi Gönder</button></form>" : "";
            sb.Append(Item("no", "Deneme hakkınız doldu",
                X.MaxAttempts + " deneme kullanıldı" + (G.LastScore.HasValue ? " · son denemeniz " + G.LastScore.Value + " puan" : "") + ". Yeniden girmek için eğitim editörünün onayı gerekir.", rej + form));
        }
        return sb.ToString();
    }

    // ---------- Soru gorunumleri ----------
    private string ReadOnly(List<ExamQuestion> qs, bool showCorrect)
    {
        if (qs.Count == 0) return "<div class=\"ex-empty\">Bu sınavda henüz soru yok.</div>";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < qs.Count; i++)
        {
            ExamQuestion q = qs[i];
            sb.Append("<div class=\"xt-q ro\"><div class=\"xt-qh\"><span class=\"eq-no\">" + (i + 1) + "</span><div class=\"xt-qt\">" + Fmt.Enc(q.QText).Replace("\n", "<br/>") + "</div>" +
                "<span class=\"eq-pts\">" + q.Points + " puan</span></div>");
            sb.Append("<div class=\"xt-hint\">" + (q.QType == "multi" ? "Birden fazla doğru olabilir" : ExamRepo.TypeLabel(q.QType)) + "</div><div class=\"xt-opts\">");
            for (int j = 0; j < q.Options.Count; j++)
            {
                bool ok = showCorrect && q.Options[j].IsCorrect;
                sb.Append("<div class=\"xt-opt ro" + (ok ? " ok" : "") + "\"><span class=\"xt-box " + (q.QType == "multi" ? "sq" : "") + "\"></span>" + Fmt.Enc(q.Options[j].OText) + (ok ? " <b>&#10003;</b>" : "") + "</div>");
            }
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }

    private string TakeForm(List<ExamQuestion> qs, int attemptId)
    {
        Random rnd = new Random(attemptId * 7919);
        List<ExamQuestion> order = new List<ExamQuestion>(qs);
        Shuffle(order, rnd);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < order.Count; i++)
        {
            ExamQuestion q = order[i];
            List<ExamOption> opts = new List<ExamOption>(q.Options);
            if (q.QType != "tf") Shuffle(opts, rnd);
            string kind = q.QType == "multi" ? "checkbox" : "radio";
            sb.Append("<div class=\"xt-q\" data-q=\"" + q.QuestionId + "\"><div class=\"xt-qh\"><span class=\"eq-no\">" + (i + 1) + "</span><div class=\"xt-qt\">" + Fmt.Enc(q.QText).Replace("\n", "<br/>") + "</div>" +
                "<span class=\"eq-pts\">" + q.Points + " puan</span></div>");
            sb.Append("<div class=\"xt-hint\">" + (q.QType == "multi" ? "Birden fazla doğru olabilir; tüm doğruları seçin" : "Tek doğru cevap") + "</div><div class=\"xt-opts\">");
            for (int j = 0; j < opts.Count; j++)
                sb.Append("<label class=\"xt-opt\"><input type=\"" + kind + "\" name=\"q_" + q.QuestionId + "\" value=\"" + opts[j].OptionId + "\" />" +
                    "<span class=\"xt-box " + (kind == "checkbox" ? "sq" : "") + "\"></span><span>" + Fmt.Enc(opts[j].OText) + "</span></label>");
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }

    private static void Shuffle<T>(List<T> list, Random rnd)
    {
        for (int i = list.Count - 1; i > 0; i--) { int j = rnd.Next(i + 1); T tmp = list[i]; list[i] = list[j]; list[j] = tmp; }
    }

    // Cozme ekraninda kullanici adi + tarih filigrani (ekran goruntusu paylasimini caydirir)
    private static string Watermark(string text)
    {
        string t = System.Security.SecurityElement.Escape(text);
        // SVG ozniteliklerinde cift tirnak -> EscapeDataString %22 yapar; url('...') tek tirnakla sarilir (HTML style="" ile cakismaz)
        string svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"360\" height=\"200\"><text x=\"0\" y=\"120\" transform=\"rotate(-24 180 100)\" " +
            "font-family=\"Arial\" font-size=\"15\" fill=\"rgba(128,140,170,0.14)\">" + t + "</text></svg>";
        return "url('data:image/svg+xml;charset=utf-8," + Uri.EscapeDataString(svg).Replace("'", "%27") + "')";
    }

    public string V(string s) { return Server.HtmlEncode(s ?? ""); }
    public string BackUrl { get { return IsTaker ? "Training.aspx" : "Exams.aspx"; } }
    public string BackLabel { get { return IsTaker ? "Akademi" : "Sınavlar"; } }
}
