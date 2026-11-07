using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

// Musteri ve bayi personeli (PartnerUser) ana sayfasi: karsilama + "Yeni neler var?", gostergeler, Devam et / surum / sertifika, talepler, ekip.
// Her bolum sabit boyutta; kayit yoksa bolume uygun "henuz yok" metni gosterilir.
public partial class CustomerHomePage : Page
{
    public string Culture = "tr", UserName = "", CompanyName = "", FlashMsg = "";
    public string Greeting = "", TodayText = "", BalanceText = "", SummaryHtml = "", TodoHtml = "", NewsHtml = "", KpiHtml = "", MidHtml = "", TicketsHtml = "", TeamHtml = "";
    public int NewCount = 0;
    public string AvatarHtml = "";
    private bool en;
    private bool staff;
    private int prevWeekSec;   // bayi personeli: talep satirinda son musteri adi da gosterilir
    private static readonly CultureInfo TRC = new CultureInfo("tr-TR");
    private const string ARROW = "<svg width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2.2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M5 12h14M13 6l6 6-6 6\"/></svg>";
    private static string H(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsCustomerSide && !Auth.IsPartnerUserLimited) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; en = Culture == "en"; staff = Auth.IsPartnerUserLimited;
        UserName = Auth.FullName; CompanyName = Auth.CompanyName;
        int cid = Auth.CompanyId, uid = Auth.UserId;
        DateTime nowTr = DateTime.UtcNow.AddHours(3);
        Greeting = PartnerHome.GreetingFor(nowTr.Hour, en) + " " + (UserName ?? "").Split(' ')[0];
        AvatarHtml = HomeKit.HeroAvatar(uid, UserName, en);
        TodayText = nowTr.ToString(en ? "dddd, d MMMM yyyy" : "d MMMM yyyy, dddd", en ? new CultureInfo("en-US") : TRC);

        // Her bolum kendi hata korumasinda: biri basarisiz olursa digerleri calisir
        try { BalanceText = PartnerHome.Dur(Repo.GetSupportBalance(cid), en); } catch { BalanceText = ""; }
        int tOpen = 0, tWait = 0, resolved = 0; double avgDays = 0; List<Ticket> tickets = new List<Ticket>();
        try
        {
            tickets = Repo.GetTicketsForCompany(cid);
            PartnerHome.TicketCounts(cid, out tOpen, out tWait);
            double sum = 0;
            foreach (Ticket t in tickets)
                if (t.ResolvedAt.HasValue) { DateTime r = t.ResolvedAt.Value.AddHours(3); if (r.Year == nowTr.Year && r.Month == nowTr.Month) { resolved++; sum += (t.ResolvedAt.Value - t.CreatedAt).TotalDays; } }
            avgDays = resolved > 0 ? sum / resolved : 0;
        }
        catch { }
        int weekSec = 0, weekPeople = 0, monthSec = 0, monthPeople = 0; prevWeekSec = 0;
        try
        {
            int back = ((int)nowTr.DayOfWeek + 6) % 7;
            PartnerHome.WatchStats(cid, nowTr.Date.AddDays(-back).AddHours(-3), out weekSec, out weekPeople);
            int sincePrev, pp; PartnerHome.WatchStats(cid, nowTr.Date.AddDays(-back - 7).AddHours(-3), out sincePrev, out pp);
            prevWeekSec = Math.Max(0, sincePrev - weekSec);
            PartnerHome.WatchStats(cid, new DateTime(nowTr.Year, nowTr.Month, 1).AddHours(-3), out monthSec, out monthPeople);
        }
        catch { }
        List<PathStep> steps = new List<PathStep>();
        try { steps = LearningPath.GetSteps(uid, cid); } catch { }

        // --- karsilama ozeti
        List<string> parts = new List<string>();
        if (tWait > 0) parts.Add("<b>" + tWait + (en ? (tWait == 1 ? " request" : " requests") : " talebinizde") + "</b>" + (en ? " awaiting your reply." : " yanıtınız bekleniyor."));
        if (weekSec >= 60) parts.Add((en ? "Your team watched " : "Ekibiniz bu hafta ") + "<b>" + H(PartnerHome.Dur(weekSec / 60, en)) + "</b>" + (en ? " of training this week." : " eğitim izledi."));
        SummaryHtml = parts.Count > 0 ? string.Join(" ", parts.ToArray())
            : (en ? "Welcome. Our support team is one request away whenever you need help, and short, hands-on trainings are waiting for you in River Academy."
                  : "Hoş geldiniz. Bir sorunla karşılaştığınızda destek ekibimiz bir talep uzağınızda; River Akademi'de ise sizi kısa ve uygulamalı eğitimler bekliyor.");

        try { TodoHtml = BuildTodo(tickets, tOpen, steps, weekSec, weekPeople, nowTr); } catch { TodoHtml = ""; }
        // Yeni neler var?: bildirimler + forum gonderileri (ortak akis)
        try { NewsHtml = HomeKit.News(cid, uid, en, nowTr, out NewCount); } catch { NewsHtml = Empty(en ? "No updates yet" : "Henüz yeni bir gelişme yok", en ? "Request replies, release notes and your team's achievements will appear here." : "Talep yanıtları, sürüm duyuruları ve ekibinizin başarıları burada görünecek.", null, null, "M12 3l2 5 5 2-5 2-2 5-2-5-5-2 5-2z"); }

        // --- gostergeler
        StringBuilder k = new StringBuilder();
        k.Append(Kpi(en ? "Open requests" : "Açık talepler", tOpen.ToString(), tWait > 0 ? tWait + (en ? " awaiting your reply" : "'i yanıtınızı bekliyor") : (tOpen > 0 ? (en ? "Our team is on it" : "Ekibimiz ilgileniyor") : (en ? "No open request" : "Açık talebiniz yok")), tWait > 0 ? "warn" : "", "#E4467A", en ? "My requests" : "Taleplerim", "Tickets.aspx"));
        k.Append(Kpi(en ? "Resolved this month" : "Bu ay çözülen", resolved.ToString(), resolved > 0 ? (en ? "Avg. resolution " : "Ortalama çözüm ") + avgDays.ToString(avgDays < 10 ? "0.#" : "0", en ? CultureInfo.InvariantCulture : TRC) + (en ? " days" : " gün") : (en ? "No request resolved this month" : "Bu ay çözülen talep yok"), resolved > 0 ? "ok" : "", "#2FD08A", en ? "Resolved requests" : "Çözülen talepler", "Tickets.aspx?view=resolved"));
        if (steps.Count > 0)
        {
            int done = 0; string next = null;
            foreach (PathStep s in steps) { if (s.State == "done") done++; else if (next == null && s.State == "next") next = s.Title; }
            k.Append(Kpi(LearningPath.PATH_NAME, done + "/" + steps.Count, done == steps.Count ? (en ? "Path completed" : "Yol tamamlandı") : (next != null ? (en ? "Next: " : "Sıradaki: ") + next : (en ? "Start the path" : "Yola başlayın")), done == steps.Count ? "ok" : "violet", "#B14AF0", "River " + (en ? "Academy" : "Akademi"), "Training.aspx?path=1"));
        }
        k.Append(Kpi(en ? "Team watch time" : "Ekibin izleme süresi", PartnerHome.Dur(monthSec / 60, en), monthSec >= 60 ? (en ? "This month · " + monthPeople + " people" : "Bu ay · " + monthPeople + " kişi") : (en ? "No viewing this month yet" : "Bu ay henüz izleme yok"), monthSec >= 60 ? "cyan" : "", "#22B8D8", en ? "My teammates" : "Takım arkadaşlarım", "Training.aspx"));
        KpiHtml = k.ToString();

        // --- Devam et / yeni surum / sertifikalar
        StringBuilder m = new StringBuilder();
        try { m.Append(ContinueCard(steps)); } catch { }
        try
        {
            List<VersionAnnouncement> vs = Repo.GetPublishedVersions();
            if (vs.Count > 0)
            {
                VersionAnnouncement v = vs[0];
                string label = HomeKit.VersionLabel(v);
                // ustte surum numarasi, altinda etiket + yayin tarihi, en altta yonlendirme
                m.Append("<a class=\"cu-mid cu-ver lk\" href=\"Versions.aspx\"><span class=\"cu-mid-tx\"><small>" + (en ? "NEW RELEASE" : "YENİ SÜRÜM") + "</small>" +
                         "<b class=\"cu-ver-no\">" + H(v.ProductName) + " v" + H(v.VersionNo) + "</b>" +
                         "<span class=\"cu-ver-meta\">" + (label != null ? "<span class=\"cu-ver-tag\">" + H(label) + "</span>" : "") + "<span>" + (en ? "Released " : "Yayın: ") + H(HomeKit.VersionDate(v, en)) + "</span></span>" +
                         "<em>" + (en ? "Read the release notes →" : "Sürüm notlarını inceleyin →") + "</em></span></a>");
            }
        }
        catch { }
        try
        {
            int valid; string mine; PartnerHome.CertStats(cid, uid, out valid, out mine);
            m.Append("<a class=\"card cu-mid lk\" href=\"Certificates.aspx\"><span class=\"cu-mid-ic cert\"><svg width=\"26\" height=\"26\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><circle cx=\"12\" cy=\"9\" r=\"5\"/><path d=\"M9 13.5L8 21l4-2 4 2-1-7.5\"/></svg></span>" +
                     "<span class=\"cu-mid-tx\"><small class=\"gold\">" + (en ? "CERTIFICATES" : "SERTİFİKALAR") + "</small><b>" + (valid > 0 ? (en ? valid + " valid certificates in your team" : "Ekibinizde " + valid + " geçerli sertifika") : (en ? "No certificates yet" : "Henüz sertifika yok")) + "</b>" +
                     "<em class=\"dim\">" + (mine != null ? (en ? "Yours: " : "Sizin sertifikanız: ") + H(mine) : (en ? "Complete trainings and take the exams" : "Eğitimleri tamamlayıp sınavlara girebilirsiniz")) + "</em></span></a>");
        }
        catch { }
        MidHtml = m.ToString();

        // --- destek talepleri (en fazla 4)
        try { TicketsHtml = BuildTickets(tickets, nowTr); } catch { TicketsHtml = ""; }
        if (TicketsHtml.Length == 0)
            TicketsHtml = Empty(en ? "No support requests yet" : "Henüz destek talebiniz yok", en ? "Whenever you run into an issue, our experts are one request away." : "Bir sorunla karşılaştığınızda uzmanlarımız bir talep uzağınızda.", en ? "+ New Support Request" : "+ Destek Talebi Oluştur", "NewTicket.aspx", "M4 13a8 8 0 0 1 16 0v4a2 2 0 0 1-2 2h-1v-6h3M4 13v4a2 2 0 0 0 2 2h1v-6H4");

        // --- ekip
        try { TeamHtml = BuildTeam(uid, cid, nowTr); } catch { TeamHtml = ""; }
        if (TeamHtml.Length == 0)
            TeamHtml = Empty(en ? "Nobody in your team has started yet" : "Ekibinizden henüz kimse yola başlamadı", en ? "Take the first step with Zero to Expert; your team's progress will appear here." : "Sıfırdan Uzmanlığa ile ilk adımı siz atın; ekibiniz ilerledikçe burada görünecek.", en ? "Start the path" : "Yola başla", "Training.aspx?path=1", "M4 19h4v-5h4v-5h4V4h4");
    }

    // ---------------------------------------------------------------- bolumler
    private const string I_CHAT = "M4 5h16v11H9l-5 4z", I_PLAY = "M8 5.5v13l10.5-6.5z", I_DONE = "M5 12l5 5 9-10", I_PLUS = "M12 5v14M5 12h14",
                         I_TEAM = "M9 11.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7zM3 20c.7-3.6 3.2-5.6 6-5.6s5.3 2 6 5.6M17 8.5a2.6 2.6 0 1 0 0-5M18.5 14.6c2.3.2 4 1.8 4.5 4.6";
    private static string TodoRow(string tone, string icon, string title, string sub, string href)
    {
        string inner = "<span class=\"cu-ic " + tone + "\"><svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + icon + "\"/></svg></span>" +
                       "<span class=\"cu-todo-tx\"><b>" + H(title) + "</b><em>" + H(sub) + "</em></span>";
        return href == null ? "<div class=\"cu-todo-it\">" + inner + "</div>" : "<a class=\"cu-todo-it lk\" href=\"" + href + "\">" + inner + "<span class=\"ph-go\">" + ARROW + "</span></a>";
    }
    /// <summary>Her zaman uc satir: talep, egitim, ekip. Verisi olmayan satir konuya uygun yonlendirme olur.</summary>
    private string BuildTodo(List<Ticket> tickets, int tOpen, List<PathStep> steps, int weekSec, int weekPeople, DateTime nowTr)
    {
        StringBuilder sb = new StringBuilder();
        // 1) talep
        Ticket wait = null; int waiting = 0;
        foreach (Ticket t in tickets) if (t.Status == "WaitingCustomer") { waiting++; if (wait == null || t.UpdatedAt > wait.UpdatedAt) wait = t; }
        if (wait != null)
        {
            string who; DateTime w; string sub = H2(wait.TicketNo);
            try { if (PartnerHome.LastReply(wait.TicketId, out who, out w) && !string.IsNullOrEmpty(who)) sub += " · " + who.Split(' ')[0] + (en ? " replied, " : " yanıtladı, ") + When(w.AddHours(3), nowTr).ToLower(TRC); }
            catch { }   // yanit bilgisi alinamazsa satir yine gosterilir
            sb.Append(TodoRow("orange", I_CHAT, waiting + (en ? (waiting == 1 ? " request awaits your reply" : " requests await your reply") : " talebinizde yanıtınız bekleniyor"), sub, "TicketDetail.aspx?id=" + wait.TicketId));
        }
        else if (tOpen > 0) sb.Append(TodoRow("blue", I_CHAT, tOpen + (en ? " open request in progress" : " açık talebiniz inceleniyor"), en ? "Our team is on it; you'll be notified on every reply" : "Ekibimiz ilgileniyor; her yanıtta bildirim alacaksınız", "Tickets.aspx"));
        else sb.Append(TodoRow("blue", I_PLUS, en ? "No open requests" : "Açık talebiniz yok", en ? "Run into an issue? Open a support request" : "Bir sorun mu var? Destek talebi oluşturun", "NewTicket.aspx"));
        // 2) egitim
        PathStep next = null; int no = 0, done = 0;
        for (int i = 0; i < steps.Count; i++) { if (steps[i].State == "done") done++; else if (next == null && steps[i].State == "next") { next = steps[i]; no = i + 1; } }
        if (next != null)
            sb.Append(TodoRow("violet", I_PLAY, (en ? "Your next training: " : "Sıradaki eğitiminiz: ") + next.Title,
                LearningPath.PATH_NAME + " · " + no + (en ? ". step" : ". adım") + (next.Percent > 0 ? " · %" + next.Percent + (en ? " watched" : " izlendi") : ""), "Training.aspx?path=1"));
        else if (steps.Count > 0) sb.Append(TodoRow("green", I_DONE, en ? "You completed Zero to Expert" : "Sıfırdan Uzmanlığa'yı tamamladınız", done + "/" + steps.Count + (en ? " steps · watch again anytime" : " adım · dilediğiniz zaman tekrar izleyebilirsiniz"), "Training.aspx?path=1"));
        else sb.Append(TodoRow("violet", I_PLAY, en ? "Explore River Academy" : "River Akademi'yi keşfedin", en ? "Short, hands-on trainings" : "Kısa ve uygulamalı eğitimler", "Training.aspx"));
        // 3) ekip
        if (weekSec >= 60)
        {
            string cmp = "";
            if (prevWeekSec >= 60) { int d = (int)Math.Round((weekSec - prevWeekSec) * 100.0 / prevWeekSec); cmp = d > 0 ? (en ? " · " + d + "% more than last week" : " · geçen haftadan %" + d + " fazla") : d < 0 ? (en ? " · " + (-d) + "% less than last week" : " · geçen haftadan %" + (-d) + " az") : (en ? " · same as last week" : " · geçen haftayla aynı"); }
            sb.Append(TodoRow("cyan", I_TEAM, (en ? "Your team watched " : "Ekibiniz bu hafta ") + PartnerHome.Dur(weekSec / 60, en) + (en ? " of training this week" : " eğitim izledi"), (en ? "This week " + weekPeople + " people" : "Bu hafta " + weekPeople + " kişi") + cmp, null));
        }
        else sb.Append(TodoRow("cyan", I_TEAM, en ? "No training watched this week yet" : "Bu hafta henüz eğitim izlenmedi", en ? "A short video is a good start" : "Kısa bir videoyla başlamaya ne dersiniz?", "Training.aspx"));
        return sb.ToString();
    }
    private static string H2(string s) { return s ?? ""; }
    private string BuildNews(int cid, int uid, DateTime nowTr)
    {
        List<NotifRow> ns = Repo.GetNotifications(cid, uid, 20, null);
        foreach (NotifRow n in ns) if (!n.IsRead) NewCount++;
        StringBuilder sb = new StringBuilder(); int c = 0;
        foreach (NotifRow n in ns)
        {
            if (c++ >= 4) break;
            string kind, color, icon; Classify(n, out kind, out color, out icon);
            string when = n.CreatedAt.HasValue ? When(n.CreatedAt.Value.AddHours(3), nowTr) : "";
            string body = n.Body(Culture);
            string inner = "<span class=\"cu-node " + color + (n.IsRead ? "" : " new") + "\"><svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + icon + "\"/></svg></span>" +
                           "<span class=\"cu-news-tx\"><span class=\"cu-news-meta\"><span class=\"cu-kind " + color + "\">" + H(kind) + "</span><span>" + H(when) + "</span>" + (n.IsRead ? "" : "<i class=\"cu-unread\" aria-label=\"" + (en ? "unread" : "okunmadı") + "\"></i>") + "</span>" +
                           "<b" + (n.IsRead ? "" : " class=\"new\"") + ">" + H(n.Title(Culture)) + "</b>" + (string.IsNullOrEmpty(body) ? "" : "<small>" + H(body) + "</small>") + "</span>";
            sb.Append(string.IsNullOrEmpty(n.Link) ? "<li><div class=\"cu-news-it\">" + inner + "</div></li>"
                                                   : "<li><a class=\"cu-news-it lk\" href=\"" + H(n.Link) + "\">" + inner + "<span class=\"ph-go\">" + ARROW + "</span></a></li>");
        }
        if (sb.Length == 0)
            return Empty(en ? "No updates yet" : "Henüz yeni bir gelişme yok", en ? "Request replies, release notes and your team's achievements will appear here." : "Talep yanıtları, sürüm duyuruları ve ekibinizin başarıları burada görünecek.", null, null, "M12 3l2 5 5 2-5 2-2 5-2-5-5-2 5-2z");
        return "<ol class=\"cu-news\"><li class=\"cu-line\" aria-hidden=\"true\"></li>" + sb + "</ol>";
    }

    /// <summary>Bildirimin turu: once Type alani, sonra baglantisi (talep / surum / akademi / sertifika)</summary>
    private void Classify(NotifRow n, out string kind, out string color, out string icon)
    {
        string t = (n.Type ?? "").ToLowerInvariant(), l = (n.Link ?? "").ToLowerInvariant(), title = (n.Title("tr") ?? "").ToLower(TRC);
        if (t.Contains("ticket") || l.Contains("ticket"))
        {
            if (title.Contains("çözül") || title.Contains("kapat")) { kind = en ? "RESOLVED" : "ÇÖZÜLDÜ"; color = "blue"; icon = "M5 12l5 5 9-10"; }
            else { kind = en ? "REQUEST" : "TALEP"; color = "orange"; icon = "M4 5h16v11H9l-5 4z"; }
        }
        else if (t.Contains("version") || l.Contains("version")) { kind = en ? "NEW RELEASE" : "YENİ SÜRÜM"; color = "violet"; icon = "M12 3l2 5 5 2-5 2-2 5-2-5-5-2 5-2zM19 15l.8 2.2L22 18l-2.2.8L19 21l-.8-2.2L16 18l2.2-.8z"; }
        else if (t.Contains("cert") || t.Contains("exam") || l.Contains("certif") || l.Contains("exam")) { kind = en ? "CERTIFICATE" : "SERTİFİKA"; color = "gold"; icon = "M12 14a5 5 0 1 0 0-10 5 5 0 0 0 0 10zM9 13.5L8 21l4-2 4 2-1-7.5"; }
        else if (t.Contains("achievement")) { kind = en ? "YOUR TEAM" : "EKİBİNİZ"; color = "green"; icon = "M12 2.5l8 4.6v9.8l-8 4.6-8-4.6V7.1z"; }
        else if (t.Contains("training") || l.Contains("training")) { kind = en ? "ACADEMY" : "AKADEMİ"; color = "violet"; icon = "M2 9l10-5 10 5-10 5zM6 11v5c2 2 10 2 12 0v-5"; }
        else { kind = en ? "NOTICE" : "BİLDİRİM"; color = "gray"; icon = "M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9M10.3 21a1.9 1.9 0 0 0 3.4 0"; }
    }

    private string ContinueCard(List<PathStep> steps)
    {
        if (steps.Count == 0) return "";
        PathStep next = null; int done = 0;
        foreach (PathStep s in steps) { if (s.State == "done") done++; else if (next == null && s.State == "next") next = s; }
        int no = next != null ? steps.IndexOf(next) + 1 : steps.Count;
        int pct = next != null ? Math.Max(0, Math.Min(100, next.Percent)) : 100;
        double c = 113.1, off = c * (1 - pct / 100.0);
        string kicker = next == null ? (en ? "PATH COMPLETED" : "YOL TAMAMLANDI") : pct > 0 ? (en ? "CONTINUE · " : "DEVAM ET · %") + pct + (en ? "%" : "") : (done == 0 ? (en ? "FIRST STEP" : "İLK ADIM") : (en ? "UP NEXT · STEP " : "SIRADAKİ · ADIM ") + no);
        string title = next != null ? next.Title : (en ? "Congratulations, all steps done" : "Tebrikler, tüm adımlar bitti");
        return "<a class=\"card cu-mid lk\" href=\"Training.aspx?path=1\"><span class=\"cu-mid-ring\"><svg width=\"52\" height=\"52\" viewBox=\"0 0 44 44\" aria-hidden=\"true\"><circle cx=\"22\" cy=\"22\" r=\"18\" fill=\"rgba(177,74,240,.15)\" stroke=\"currentColor\" stroke-opacity=\".16\" stroke-width=\"3.5\"/>" +
               "<circle cx=\"22\" cy=\"22\" r=\"18\" fill=\"none\" stroke=\"#C9A6FF\" stroke-width=\"3.5\" stroke-linecap=\"round\" stroke-dasharray=\"" + c.ToString(CultureInfo.InvariantCulture) + "\" stroke-dashoffset=\"" + off.ToString("0.0", CultureInfo.InvariantCulture) + "\" transform=\"rotate(-90 22 22)\"/>" +
               "<text x=\"22\" y=\"27.5\" text-anchor=\"middle\" font-size=\"15\" font-weight=\"800\" fill=\"currentColor\">" + (next != null ? no.ToString() : "✓") + "</text></svg></span>" +
               "<span class=\"cu-mid-tx\"><small>" + kicker + "</small><b>" + H(title) + "</b><em class=\"dim\">" + LearningPath.PATH_NAME + " · " + done + "/" + steps.Count + "</em></span></a>";
    }

    private string BuildTickets(List<Ticket> all, DateTime nowTr)
    {
        List<Ticket> l = new List<Ticket>(all);
        // once acik olanlar (yanit beklenen en ustte), sonra son guncellenen
        l.Sort(delegate (Ticket a, Ticket b)
        {
            int ra = Rank(a.Status), rb = Rank(b.Status); if (ra != rb) return ra.CompareTo(rb);
            return b.UpdatedAt.CompareTo(a.UpdatedAt);
        });
        StringBuilder sb = new StringBuilder(); int c = 0;
        foreach (Ticket t in l)
        {
            if (c++ >= 4) break;
            string label, cls; StatusOf(t.Status, out label, out cls);
            if (!string.IsNullOrEmpty(t.StatusNote) && (t.Status == "New" || t.Status == "InProgress")) label = t.StatusNote;   // personelin yazdigi gorunen durum
            string meta = t.Status == "Resolved" || t.Status == "Closed"
                ? label + " · " + (t.ResolvedAt ?? t.UpdatedAt).AddHours(3).ToString("d MMMM", TRC)
                : (string.IsNullOrEmpty(t.AssignedToName) ? (en ? "Awaiting assignment" : "Uzman atanıyor") : (en ? "Assigned: " : "Atanan: ") + t.AssignedToName.Split(' ')[0]) + " · " + When(t.UpdatedAt.AddHours(3), nowTr) + (en ? " updated" : " güncellendi");
            string cust = staff && !string.IsNullOrEmpty(t.CustomerName) ? " · " + (en ? "Customer: " : "Müşteri: ") + H(t.CustomerName) : "";
            sb.Append("<a class=\"cu-tk lk\" href=\"TicketDetail.aspx?id=" + t.TicketId + "\"><span class=\"cu-tk-tx\"><small>" + H(t.TicketNo) + (string.IsNullOrEmpty(t.Product) ? "" : " · " + H(t.Product)) + cust + "</small>" +
                      "<b>" + H(t.Subject) + "</b><em>" + H(meta) + "</em></span><span class=\"ph-chip " + cls + "\">" + H(label) + "</span></a>");
        }
        return sb.ToString();
    }
    private static int Rank(string s) { return s == "WaitingCustomer" ? 0 : (s == "New" || s == "InProgress") ? 1 : 2; }
    private void StatusOf(string s, out string label, out string cls)
    {
        switch (s)
        {
            case "WaitingCustomer": label = en ? "Awaiting your reply" : "Yanıtınız bekleniyor"; cls = "warn"; break;
            case "New": label = en ? "Received" : "Alındı"; cls = "info"; break;
            case "InProgress": label = en ? "In progress" : "İnceleniyor"; cls = "info"; break;
            case "Resolved": label = en ? "Resolved" : "Çözüldü"; cls = "ok"; break;
            default: label = en ? "Closed" : "Kapatıldı"; cls = "mute"; break;
        }
    }

    private string BuildTeam(int uid, int cid, DateTime nowTr)
    {
        List<PartnerHome.Mate> team = PartnerHome.Team(uid, cid);
        bool anyProgress = false; foreach (PartnerHome.Mate m in team) if (m.Done > 0) anyProgress = true;
        if (team.Count == 0 || team[0].Total == 0 || !anyProgress) return "";
        StringBuilder sb = new StringBuilder();
        string who, title; DateTime w;
        if (PartnerHome.TeamRecent(uid, cid, out who, out title, out w))
            sb.Append("<div class=\"cu-live\"><i aria-hidden=\"true\"></i><span><b>" + H(who) + "</b> " + H(When(w.AddHours(3), nowTr).ToLower(TRC)) + " “" + H(title) + "” " + (en ? "badge earned" : "rozetini kazandı") + "</span></div>");
        sb.Append("<div class=\"cu-team\">");
        int c = 0; string[] g = { "linear-gradient(135deg,#4F7BFF,#C23BF0)", "linear-gradient(135deg,#22B8D8,#4F7BFF)", "linear-gradient(135deg,#F07AD8,#A06BFF)", "linear-gradient(135deg,#FFB547,#F07AD8)" };
        foreach (PartnerHome.Mate m in team)
        {
            if (c >= 4) break;
            int pct = m.Total > 0 ? m.Done * 100 / m.Total : 0;
            sb.Append("<div class=\"cu-mate\"><span class=\"cu-ava\" style=\"background:" + g[c % 4] + "\">" + H(Fmt.Initials(m.Name)) + "</span><span class=\"cu-mate-tx\"><span><b>" + H(m.Name) + (m.Me ? (en ? " (you)" : " (siz)") : "") + "</b><small>" + m.Done + "/" + m.Total + "</small></span>" +
                      "<span class=\"cu-bar\"><i style=\"width:" + pct + "%\"></i></span></span></div>");
            c++;
        }
        return sb.Append("</div>").ToString();
    }

    // ---------------------------------------------------------------- yardimcilar
    private static string Kpi(string label, string value, string note, string tone, string dot, string target, string href)
    {
        return "<a class=\"card ph-kpi cu-kpi lk\" href=\"" + href + "\" aria-label=\"" + H(label) + ": " + H(value) + ". " + H(target) + "\">" +
               "<span class=\"ph-kpi-h\"><span>" + H(label) + "</span><i style=\"background:" + dot + "\"></i></span>" +
               "<b>" + H(value) + "</b><small class=\"" + tone + " ph-kpi-next\">" + H(note) + "</small>" +
               "<span class=\"ph-kpi-go\">" + H(target) + ARROW + "</span></a>";
    }
    private static string Empty(string title, string text, string cta, string href, string icon)
    {
        return "<div class=\"cu-empty\"><span class=\"cu-empty-ic\"><svg width=\"22\" height=\"22\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + icon + "\"/></svg></span>" +
               "<b>" + H(title) + "</b><span>" + H(text) + "</span>" + (cta != null ? "<a class=\"btn-ghost\" href=\"" + href + "\">" + H(cta) + "</a>" : "") + "</div>";
    }
    private string When(DateTime d, DateTime nowTr)
    {
        TimeSpan ago = nowTr - d; int days = (int)(nowTr.Date - d.Date).TotalDays;
        if (ago.TotalMinutes < 60) return ago.TotalMinutes < 2 ? (en ? "Just now" : "Az önce") : (int)ago.TotalMinutes + (en ? " min ago" : " dk önce");
        if (days <= 0) return (int)ago.TotalHours + (en ? " hours ago" : " saat önce");
        if (days == 1) return en ? "Yesterday" : "Dün";
        return days < 7 ? days + (en ? " days ago" : " gün önce") : d.ToString(en ? "MMM d" : "d MMMM", en ? new CultureInfo("en-US") : TRC);
    }
}
