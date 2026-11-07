using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Editor: bir denemenin detayi - kisinin verdigi cevaplar, dogru cevaplar, soru bazinda puan. Kullaniciya ASLA gosterilmez.
public partial class ExamAttemptPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public ExamInfo X = null;
    public ExamAttempt A = null;
    public string QuestionsHtml = "";
    public int CorrectCount = 0, WrongCount = 0, EmptyCount = 0;
    public string DateStr = "", DurationStr = "";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;

        int aid; int.TryParse(Request.QueryString["a"], out aid);
        A = aid > 0 ? ExamRepo.GetAttempt(aid) : null;
        if (A == null || !A.SubmittedAt.HasValue) { Response.Redirect("Exams.aspx"); return; }
        X = ExamRepo.GetExam(A.ExamId);
        if (X == null) { Response.Redirect("Exams.aspx"); return; }

        DateStr = A.SubmittedAt.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR);
        TimeSpan d = A.SubmittedAt.Value - A.StartedAt;
        DurationStr = d.TotalMinutes < 1 ? "1 dk'dan kısa" : ((int)d.TotalMinutes) + " dk " + d.Seconds + " sn";

        Dictionary<int, List<int>> picked; Dictionary<int, int> pts;
        ExamRepo.GetAttemptAnswers(aid, out picked, out pts);
        List<ExamQuestion> qs = ExamRepo.GetQuestions(X.ExamId);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < qs.Count; i++)
        {
            ExamQuestion q = qs[i];
            List<int> sel = picked.ContainsKey(q.QuestionId) ? picked[q.QuestionId] : new List<int>();
            int earned = pts.ContainsKey(q.QuestionId) ? pts[q.QuestionId] : 0;
            bool empty = sel.Count == 0, ok = earned > 0;
            if (empty) EmptyCount++; else if (ok) CorrectCount++; else WrongCount++;
            string state = empty ? "empty" : ok ? "ok" : "no";
            sb.Append("<div class=\"xa-q " + state + "\"><div class=\"xt-qh\"><span class=\"eq-no\">" + (i + 1) + "</span><div class=\"xt-qt\">" + Fmt.Enc(q.QText).Replace("\n", "<br/>") + "</div>" +
                "<span class=\"xa-pts " + state + "\">" + earned + " / " + q.Points + "</span></div>");
            sb.Append("<div class=\"xt-hint\">" + ExamRepo.TypeLabel(q.QType) + " · " + (empty ? "Cevaplanmadı" : ok ? "Doğru" : "Yanlış") + "</div><div class=\"xt-opts\">");
            for (int j = 0; j < q.Options.Count; j++)
            {
                ExamOption o = q.Options[j];
                bool mine = sel.Contains(o.OptionId);
                string cls = o.IsCorrect ? (mine ? " hit" : " correct") : (mine ? " miss" : "");
                string tag = "";
                if (mine) tag += "<span class=\"xa-tag you\">Kullanıcının cevabı</span>";
                if (o.IsCorrect) tag += "<span class=\"xa-tag right\">Doğru cevap</span>";
                sb.Append("<div class=\"xt-opt ro xa-opt" + cls + "\"><span class=\"xt-box " + (q.QType == "multi" ? "sq" : "") + (mine ? " on" : "") + "\"></span>" +
                    "<span class=\"xa-ot\">" + Fmt.Enc(o.OText) + "</span>" + tag + "</div>");
            }
            sb.Append("</div></div>");
        }
        QuestionsHtml = sb.ToString();
    }

    public string V(string s) { return Server.HtmlEncode(s ?? ""); }
}
