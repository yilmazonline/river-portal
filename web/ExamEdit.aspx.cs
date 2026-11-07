using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Sinav editoru: bilgiler, soru ekle/duzenle, puan butcesi (toplam 100), durum gecisleri, sonuclar.
public partial class ExamEditPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public ExamInfo X = null;
    public bool IsNew = true;
    public bool IsDraft = true;
    public int Remaining = 100;
    public string FlashMsg = "";
    public string FlashErr = "";
    public string QuestionsHtml = "";
    public string ResultsHtml = "";
    public string GroupOptions = "";
    public string BadgeOptions = "";
    public ExamQuestion EditQ = null;       // duzenlenen soru (null = yeni)
    public int EditQMaxPoints = 100;
    public string EditOptsJson = "[]";
    private List<ExamQuestion> qs = new List<ExamQuestion>();
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.CanEditTraining) { Response.Redirect("Exams.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;

        int id; int.TryParse(Request.QueryString["id"], out id);
        if (id > 0) { X = ExamRepo.GetExam(id); if (X == null) { Response.Redirect("Exams.aspx"); return; } IsNew = false; }
        else X = new ExamInfo { Status = "Draft", PassScore = 70, MaxAttempts = 3, CooldownHours = 24, Title = "", BadgeColor = "gold" };
        IsDraft = X.Status == "Draft";

        string ok = Request.QueryString["ok"];
        if (ok == "saved") FlashMsg = "Sınav kaydedildi.";
        else if (ok == "published") FlashMsg = "Sınav yayına alındı. Aktive edene kadar herkes salt okunur görür.";
        else if (ok == "activated") FlashMsg = "Sınav aktive edildi; kullanıcılar artık giriş yapabilir.";
        else if (ok == "draft") FlashMsg = "Sınav taslağa alındı, düzenleyebilirsiniz.";
        else if (ok == "archived") FlashMsg = "Sınav arşive alındı.";
        else if (ok == "q") FlashMsg = "Soru kaydedildi.";
        else if (ok == "copied") FlashMsg = "Sınav kopyalandı. Bu yeni taslak üzerinde düzenleme yapabilirsiniz.";
        string er = Request.QueryString["err"];
        if (er == "points") FlashErr = "Yayına almak için soru puanlarının toplamı tam 100 olmalı.";
        else if (er == "correct") FlashErr = "Her sorunun en az bir doğru cevabı işaretli olmalı.";

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { HandlePost(); if (Response.IsRequestBeingRedirected) return; }

        if (!IsNew) qs = ExamRepo.GetQuestions(X.ExamId);
        int used = 0; for (int i = 0; i < qs.Count; i++) used += qs[i].Points;
        Remaining = 100 - used;

        int eq; int.TryParse(Request.QueryString["q"], out eq);
        if (eq > 0 && IsDraft) for (int i = 0; i < qs.Count; i++) if (qs[i].QuestionId == eq) EditQ = qs[i];
        EditQMaxPoints = Remaining + (EditQ != null ? EditQ.Points : 0);
        EditOptsJson = OptsJson(EditQ);

        GroupOptions = BuildGroups(X.CategoryGroup);
        BadgeOptions = BuildBadges(BadgePalette.Norm(X.BadgeColor));
        QuestionsHtml = BuildQuestions();
        if (!IsNew) ResultsHtml = BuildResults();
    }

    // ---------------- POST ----------------
    private void HandlePost()
    {
        var f = Request.Unvalidated.Form;
        string action = f["action"] ?? "";
        string self = "ExamEdit.aspx?id=" + X.ExamId;

        if (action == "saveMeta" || action == "saveAndPublish")
        {
            if (!IsDraft) { Response.Redirect(self); return; }
            X.Title = (f["title"] ?? "").Trim();
            if (X.Title.Length == 0) { FlashErr = "Sınav adı zorunludur."; return; }
            X.Description = (f["desc"] ?? "").Trim();
            X.CategoryGroup = (f["group"] ?? "").Trim();
            X.PassScore = Clamp(f["pass"], 1, 100, 70);
            X.MaxAttempts = Clamp(f["max"], 1, 20, 3);
            X.CooldownHours = Clamp(f["cool"], 0, 720, 24);
            int tl = Clamp(f["timelimit"], 0, 600, 0); X.TimeLimitMin = tl > 0 ? (int?)tl : null;
            int vm = Clamp(f["valid"], 0, 120, 0); X.ValidMonths = vm > 0 ? (int?)vm : null;
            X.BadgeColor = BadgePalette.Norm(f["badge"]);
            int newId = ExamRepo.SaveExam(X, Auth.UserId);
            if (action == "saveAndPublish") { Publish(newId); return; }
            Response.Redirect("ExamEdit.aspx?id=" + newId + "&ok=saved"); return;
        }
        if (IsNew) { Response.Redirect("ExamEdit.aspx"); return; }

        if (action == "saveQ")
        {
            if (!IsDraft) { Response.Redirect(self); return; }
            int qid; int.TryParse(f["qid"], out qid);
            string text = (f["qtext"] ?? "").Trim();
            string type = f["qtype"] ?? "single";
            if (type != "multi" && type != "tf") type = "single";
            int pts; int.TryParse(f["points"], out pts);

            List<ExamQuestion> cur = ExamRepo.GetQuestions(X.ExamId);
            int used = 0; int oldPts = 0;
            for (int i = 0; i < cur.Count; i++) { if (cur[i].QuestionId == qid) oldPts = cur[i].Points; else used += cur[i].Points; }
            int max = 100 - used;

            string[] opt = f.GetValues("opt") ?? new string[0];
            string[] corr = f.GetValues("corr") ?? new string[0];
            HashSet<int> cset = new HashSet<int>();
            for (int i = 0; i < corr.Length; i++) { int c; if (int.TryParse(corr[i], out c)) cset.Add(c); }

            List<KeyValuePair<string, bool>> opts = new List<KeyValuePair<string, bool>>();
            if (type == "tf")
            {
                opts.Add(new KeyValuePair<string, bool>("Doğru", cset.Contains(0)));
                opts.Add(new KeyValuePair<string, bool>("Yanlış", cset.Contains(1)));
            }
            else
                for (int i = 0; i < opt.Length; i++) { string o = (opt[i] ?? "").Trim(); if (o.Length > 0) opts.Add(new KeyValuePair<string, bool>(o, cset.Contains(i))); }

            int nCorrect = 0; for (int i = 0; i < opts.Count; i++) if (opts[i].Value) nCorrect++;
            string back = self + (qid > 0 ? "&q=" + qid : "") + "#qform";
            if (text.Length == 0) { FlashErr = "Soru metni zorunludur."; return; }
            if (pts < 1 || pts > max) { FlashErr = "Puan 1 ile " + max + " arasında olmalı (kalan puan bütçesi)."; return; }
            if (opts.Count < 2) { FlashErr = "En az iki şık girilmeli."; return; }
            if (nCorrect == 0) { FlashErr = "En az bir doğru cevap işaretlenmeli."; return; }
            if (type != "multi" && nCorrect > 1) { FlashErr = "Tek seçimli soruda yalnız bir doğru cevap olabilir."; return; }

            if (qid > 0 && ExamRepo.GetQuestionExamId(qid) == X.ExamId) ExamRepo.UpdateQuestion(qid, text, type, pts, opts);
            else ExamRepo.AddQuestion(X.ExamId, text, type, pts, opts);
            Response.Redirect(self + "&ok=q#qlist"); return;
        }
        if (action == "delQ" || action == "upQ" || action == "downQ")
        {
            if (!IsDraft) { Response.Redirect(self); return; }
            int qid; int.TryParse(f["qid"], out qid);
            if (qid > 0 && ExamRepo.GetQuestionExamId(qid) == X.ExamId)
            {
                if (action == "delQ") ExamRepo.DeleteQuestion(qid);
                else ExamRepo.MoveQuestion(X.ExamId, qid, action == "upQ" ? -1 : 1);
            }
            Response.Redirect(self + "#qlist"); return;
        }
        if (action == "equalize" && IsDraft) { ExamRepo.EqualizePoints(X.ExamId); Response.Redirect(self + "&ok=q#qlist"); return; }
        if (action == "publish" && IsDraft) { Publish(X.ExamId); return; }
        if (action == "toDraft" && X.Status == "Published") { ExamRepo.SetStatus(X.ExamId, "Draft"); Response.Redirect(self + "&ok=draft"); return; }
        if (action == "activate" && X.Status == "Published")
        {
            ExamRepo.SetStatus(X.ExamId, "Active");
            if (f["notify"] != null)
                try { Repo.NotifyAllCompanies("Yeni sınav: " + X.Title, "New exam: " + X.Title,
                    "Akademi'de yeni bir sınav girişe açıldı. Eğitimleri tamamlayıp sertifikanızı alabilirsiniz.",
                    "A new exam is open in the Academy.", "Training", "Exams.aspx"); } catch { }
            Response.Redirect(self + "&ok=activated"); return;
        }
        if (action == "archive" && X.Status == "Active") { ExamRepo.SetStatus(X.ExamId, "Archived"); Response.Redirect(self + "&ok=archived"); return; }
        if (action == "copy") { int nid = ExamRepo.CopyExam(X.ExamId, Auth.UserId); Response.Redirect("ExamEdit.aspx?id=" + nid + "&ok=copied"); return; }
        if (action == "delete" && IsDraft && !ExamRepo.HasAttempts(X.ExamId)) { ExamRepo.DeleteExam(X.ExamId); Response.Redirect("Exams.aspx?ok=deleted"); return; }
        if (action == "revoke" || action == "restore")
        {
            int cid; int.TryParse(f["certId"], out cid);
            CertificateInfo c = cid > 0 ? ExamRepo.GetCertificate(cid) : null;
            if (c != null && c.ExamId == X.ExamId) ExamRepo.SetRevoked(cid, action == "revoke");
            Response.Redirect(self + "#results"); return;
        }
        Response.Redirect(self);
    }

    private void Publish(int examId)
    {
        ExamInfo ex = ExamRepo.GetExam(examId);
        string self = "ExamEdit.aspx?id=" + examId;
        if (ex == null || ex.QuestionCount == 0 || ex.TotalPoints != 100) { Response.Redirect(self + "&err=points"); return; }
        List<ExamQuestion> list = ExamRepo.GetQuestions(examId);
        for (int i = 0; i < list.Count; i++)
        {
            bool any = false; for (int j = 0; j < list[i].Options.Count; j++) if (list[i].Options[j].IsCorrect) any = true;
            if (!any) { Response.Redirect(self + "&err=correct"); return; }
        }
        ExamRepo.SetStatus(examId, "Published");
        Response.Redirect(self + "&ok=published");
    }

    private static int Clamp(string s, int min, int max, int def)
    {
        int v; if (!int.TryParse(s, out v)) return def;
        if (v < min) return min; if (v > max) return max; return v;
    }

    // ---------------- RENDER ----------------
    private string BuildGroups(string sel)
    {
        StringBuilder sb = new StringBuilder("<option value=\"\">Koşul yok (tüm kullanıcılar girebilir)</option>");
        List<string> gs; try { gs = ExamRepo.GetVideoGroups(); } catch { gs = new List<string>(); }
        bool found = string.IsNullOrEmpty(sel);
        for (int i = 0; i < gs.Count; i++)
        {
            bool s = gs[i] == sel; if (s) found = true;
            sb.Append("<option value=\"" + Fmt.Enc(gs[i]) + "\"" + (s ? " selected=\"selected\"" : "") + ">" + Fmt.Enc(gs[i]) + "</option>");
        }
        if (!found) sb.Append("<option value=\"" + Fmt.Enc(sel) + "\" selected=\"selected\">" + Fmt.Enc(sel) + " (artık video yok)</option>");
        return sb.ToString();
    }

    private string BuildBadges(string sel)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < BadgePalette.All.Length; i++)
        {
            BadgePalette p = BadgePalette.All[i];
            sb.Append("<label class=\"bg-opt\" style=\"--bgc:" + p.Accent + "\"><input type=\"radio\" name=\"badge\" value=\"" + p.Key + "\"" + (p.Key == sel ? " checked=\"checked\"" : "") + " />" +
                ExamRepo.SealSvg(54, false, p.Key) + "<span>" + p.Name + "</span></label>");
        }
        return sb.ToString();
    }

    // Onizleme adresi (sayfa ilk acilisinda; sonrasi JS ile guncellenir)
    public string PreviewUrl
    {
        get
        {
            return "Certificate.aspx?preview=1&title=" + Server.UrlEncode(X.Title ?? "") + "&color=" + BadgePalette.Norm(X.BadgeColor) +
                "&valid=" + (X.ValidMonths.HasValue ? X.ValidMonths.Value : 0);
        }
    }

    private string BuildQuestions()
    {
        if (IsNew) return "";
        if (qs.Count == 0) return "<div class=\"ex-empty\">Henüz soru eklenmedi.</div>";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < qs.Count; i++)
        {
            ExamQuestion q = qs[i];
            bool editing = EditQ != null && EditQ.QuestionId == q.QuestionId;
            sb.Append("<div class=\"eq-item" + (editing ? " editing" : "") + "\">");
            sb.Append("<div class=\"eq-head\"><span class=\"eq-no\">" + (i + 1) + "</span><div class=\"eq-text\">" + Fmt.Enc(q.QText).Replace("\n", "<br/>") + "</div>");
            sb.Append("<span class=\"eq-pts\">" + q.Points + " puan</span></div>");
            sb.Append("<div class=\"eq-meta\">" + ExamRepo.TypeLabel(q.QType) + "</div><ul class=\"eq-opts\">");
            for (int j = 0; j < q.Options.Count; j++)
                sb.Append("<li class=\"" + (q.Options[j].IsCorrect ? "ok" : "") + "\">" + (q.Options[j].IsCorrect ? "&#10003; " : "") + Fmt.Enc(q.Options[j].OText) + "</li>");
            sb.Append("</ul>");
            if (IsDraft)
            {
                sb.Append("<div class=\"eq-act\"><a class=\"btn-ghost\" href=\"ExamEdit.aspx?id=" + X.ExamId + "&q=" + q.QuestionId + "#qform\">Düzenle</a>");
                if (i > 0) sb.Append(ActBtn("upQ", q.QuestionId, "&#8593;", "Yukarı taşı", ""));
                if (i < qs.Count - 1) sb.Append(ActBtn("downQ", q.QuestionId, "&#8595;", "Aşağı taşı", ""));
                sb.Append(ActBtn("delQ", q.QuestionId, "Sil", "Soruyu sil", "Bu soru silinsin mi?"));
                sb.Append("</div>");
            }
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private string ActBtn(string action, int qid, string label, string title, string confirm)
    {
        return "<form method=\"post\" action=\"ExamEdit.aspx?id=" + X.ExamId + "\" style=\"display:inline\"" +
            (confirm.Length > 0 ? " onsubmit=\"return confirm('" + confirm + "')\"" : "") + " data-noload=\"1\">" +
            "<input type=\"hidden\" name=\"action\" value=\"" + action + "\" /><input type=\"hidden\" name=\"qid\" value=\"" + qid + "\" />" +
            "<button type=\"submit\" class=\"btn-ghost" + (action == "delQ" ? " eq-del" : "") + "\" title=\"" + title + "\">" + label + "</button></form>";
    }

    private string OptsJson(ExamQuestion q)
    {
        if (q == null) return "[]";
        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < q.Options.Count; i++)
        {
            if (i > 0) sb.Append(",");
            sb.Append("{\"t\":\"" + J(q.Options[i].OText) + "\",\"c\":" + (q.Options[i].IsCorrect ? "true" : "false") + "}");
        }
        return sb.Append("]").ToString();
    }

    private static string J(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n").Replace("<", "\\u003c").Replace(">", "\\u003e");
    }

    private string BuildResults()
    {
        List<ExamAttempt> rs; try { rs = ExamRepo.GetExamResults(X.ExamId); } catch { return ""; }
        if (rs.Count == 0) return "";
        // kullanici basina deneme numarasi (eskiden yeniye)
        Dictionary<int, int> seq = new Dictionary<int, int>();
        Dictionary<int, int> attNo = new Dictionary<int, int>();
        for (int i = rs.Count - 1; i >= 0; i--) { int u = rs[i].UserId; seq[u] = (seq.ContainsKey(u) ? seq[u] : 0) + 1; attNo[rs[i].AttemptId] = seq[u]; }
        int passCnt = 0; Dictionary<int, bool> users = new Dictionary<int, bool>();
        for (int i = 0; i < rs.Count; i++) { users[rs[i].UserId] = true; if (rs[i].Passed == true) passCnt++; }

        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"ex-stats\"><span><b>" + users.Count + "</b> kullanıcı</span><span><b>" + rs.Count + "</b> deneme</span><span><b>" + passCnt + "</b> başarılı</span></div>");
        sb.Append("<div class=\"tbl-scroll\"><table><thead><tr><th>Kullanıcı</th><th>Firma</th><th>Deneme</th><th>Puan</th><th>Sonuç</th><th>Tarih</th><th>Sertifika</th><th></th></tr></thead><tbody>");
        for (int i = 0; i < rs.Count; i++)
        {
            ExamAttempt a = rs[i];
            string certCell = "<span style=\"color:var(--dim)\">—</span>";
            if (a.Passed == true)
            {
                CertificateInfo c = ExamRepo.GetUserExamCertificate(a.UserId, X.ExamId);
                if (c != null)
                    certCell = "<a href=\"Certificate.aspx?id=" + c.CertificateId + "\" target=\"_blank\">" + Fmt.Enc(c.CertNo) + "</a>" + (c.IsRevoked ? " <span class=\"st cancelled\">İptal</span>" : "") +
                        " <form method=\"post\" action=\"ExamEdit.aspx?id=" + X.ExamId + "\" style=\"display:inline\" data-noload=\"1\" onsubmit=\"return confirm('" + (c.IsRevoked ? "Sertifika yeniden geçerli yapılsın mı?" : "Sertifika iptal edilsin mi? Doğrulama sayfasında geçersiz görünür.") + "')\">" +
                        "<input type=\"hidden\" name=\"action\" value=\"" + (c.IsRevoked ? "restore" : "revoke") + "\" /><input type=\"hidden\" name=\"certId\" value=\"" + c.CertificateId + "\" />" +
                        "<button class=\"btn-ghost\" style=\"padding:3px 9px;font-size:11.5px\">" + (c.IsRevoked ? "Geri al" : "İptal et") + "</button></form>";
                else certCell = "<span style=\"color:var(--muted);font-size:12px\">Ad teyidi bekleniyor</span>";
            }
            sb.Append("<tr><td>" + Fmt.Enc(a.UserName) + "</td><td>" + Fmt.Enc(a.CompanyName) + "</td><td>" + (attNo.ContainsKey(a.AttemptId) ? attNo[a.AttemptId] : 1) + "</td>" +
                "<td><b>" + (a.Score.HasValue ? a.Score.Value : 0) + "</b></td>" +
                "<td>" + (a.Passed == true ? "<span class=\"st approved\">Geçti</span>" : "<span class=\"st cancelled\">Kaldı</span>") + "</td>" +
                "<td>" + (a.SubmittedAt.HasValue ? a.SubmittedAt.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) : "") + "</td><td>" + certCell + "</td>" +
                "<td><a class=\"btn-ghost\" style=\"padding:4px 11px;font-size:12px;white-space:nowrap\" href=\"ExamAttempt.aspx?a=" + a.AttemptId + "\">Cevaplar</a></td></tr>");
        }
        sb.Append("</tbody></table></div>");
        return sb.ToString();
    }

    public string V(string s) { return Server.HtmlEncode(s ?? ""); }
}
