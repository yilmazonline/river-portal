using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;

public partial class PartnerPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public string RevM = "0";
    public string TierNameStr = "";
    public string BadgeFile = "kayitli";
    public string BadgeLabel = "Kayıtlı Partner";
    public string BaseCommStr = "0";
    public string RentalStr = "0";
    public string RenewalStr = "0";
    public string LicenseStr = "0";
    public string SlaStr = "";
    public double Gauge = 0;
    public double Band = 0;
    public int UnreadCount = 0;

    public bool HasNext = false;
    public string NextName = "";
    public string RemainingStr = "0";
    public string NextBaseStr = "0";
    public string CurMinM = "0";
    public string NextMinM = "0";

    public bool IsVendorActing = false;   // vendor partner adina islem yapabilir
    public bool IsImpersonating = false;
    public string FlashMsg = "";
    public string FlashErr = "";

    public string DealsHtml = "";
    public string DemosHtml = "";
    public string NotifHtml = "";
    public string CustomerRegsHtml = "";
    // Yeni ana sayfa (Komuta Merkezi)
    public string AvatarHtml = "";
    public string Greeting = "", TodayText = "", SummaryHtml = "", KpiHtml = "", PipelineHtml = "", FeedHtml = "", AcademyHtml = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (Auth.IsPartnerUserLimited) { Response.Redirect(Auth.HomeUrl); return; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        IsVendorActing = Auth.IsVendor;
        IsImpersonating = Auth.IsImpersonating;

        bool en = Culture == "en";
        string ok = Request.QueryString["ok"];
        if (ok == "deal") FlashMsg = en ? "Deal booked on behalf of the partner." : "Anlaşma partner adına kaydedildi.";
        else if (ok == "dealedit") FlashMsg = en ? "Deal updated." : "Anlaşma güncellendi.";
        else if (ok == "dealdel") FlashMsg = en ? "Deal deleted." : "Anlaşma silindi.";
        else if (ok == "demo") FlashMsg = en ? "Demo request submitted." : "Demo talebi gönderildi.";
        string err = Request.QueryString["err"];
        if (err == "demotime") FlashErr = en ? "Please pick a weekday between 09:00 and 18:00." : "Lütfen hafta içi ve 09:00 ile 18:00 arası bir saat seçin.";
        else if (err == "demo") FlashErr = en ? "Please fill in customer and module." : "Lütfen müşteri ve modül alanlarını doldurun.";
        else if (err == "amount") FlashErr = en ? "Please enter a valid amount." : "Lütfen geçerli bir tutar girin.";
        else if (err == "commission") FlashErr = en ? "Please enter a valid commission amount." : "Lütfen geçerli bir hakediş tutarı girin.";
        else if (err == "custname") FlashErr = en ? "Customer name is required." : "Müşteri adı zorunludur.";
        else if (err == "custprotected") FlashErr = en ? "This customer is already registered and protected by another partner." : "Bu müşteri başka bir iş ortağı tarafından kaydedilmiş ve korumada. Kayıt yapılamaz.";
        if (ok == "custreg") FlashMsg = en ? "Customer registered · sent to Fol Yazılım for approval." : "Müşteri kaydı oluşturuldu · Fol Yazılım'a gönderildi (onay bekleniyor).";
        else if (ok == "renewreq") FlashMsg = en ? "Renewal request sent to Fol Yazılım." : "Kayıt yenileme talebi Fol Yazılım'a gönderildi.";

        int companyId = Auth.CompanyId;
        CompanyInfo company = Repo.GetCompany(companyId);
        List<Tier> tiers = Repo.GetTiers();
        if (company == null || tiers.Count == 0) return;

        CompanyName = Auth.CompanyName;
        if (string.IsNullOrEmpty(CompanyName)) CompanyName = company.Name;

        TierEngine engine = new TierEngine(tiers);
        decimal revenue = company.AnnualNetRevenue;

        Tier current = null;
        if (company.CurrentTierId != null)
            current = tiers.FirstOrDefault(t => t.TierId == company.CurrentTierId.Value);
        if (current == null) current = engine.ResolveTier(revenue);

        Tier next = engine.NextTier(current);

        RevM = Fmt.Millions(revenue);
        TierNameStr = current.Name(Culture);
        if (current.Code == "Platinum") { BadgeFile = "platinum"; BadgeLabel = "Platinum Partner"; }
        else if (current.Code == "Gold") { BadgeFile = "gold"; BadgeLabel = "Gold Partner"; }
        else if (current.Code == "Silver") { BadgeFile = "silver"; BadgeLabel = "Silver Partner"; }
        else { BadgeFile = "kayitli"; BadgeLabel = "Kayıtlı Partner"; }
        BaseCommStr = current.BaseCommission.ToString("0", CultureInfo.InvariantCulture);
        RentalStr = engine.RentalRate(current).ToString("0", CultureInfo.InvariantCulture);
        RenewalStr = engine.RenewalRate(current).ToString("0", CultureInfo.InvariantCulture);
        LicenseStr = engine.LicenseUpgradeRate(current).ToString("0", CultureInfo.InvariantCulture);
        SlaStr = current.Sla(Culture);

        // Gosterge: Platinum esigine gore doluluk
        decimal platThreshold = tiers[tiers.Count - 1].MinRevenue;
        if (platThreshold <= 0) platThreshold = 4000000m;
        Gauge = ClampD((double)(revenue / platThreshold), 0, 1);

        // Band ilerlemesi (mevcut kademe bandi icinde)
        decimal lo = current.MinRevenue;
        decimal hi = current.MaxRevenue != null ? current.MaxRevenue.Value : (lo == 0 ? 1 : lo * 2);
        Band = hi > lo ? ClampD((double)((revenue - lo) / (hi - lo)), 0, 1) : 1;

        if (next != null)
        {
            HasNext = true;
            NextName = next.Name(Culture);
            decimal remaining = Math.Max(0, next.MinRevenue - revenue);
            RemainingStr = Fmt.N0(remaining);
            NextBaseStr = next.BaseCommission.ToString("0", CultureInfo.InvariantCulture);
            CurMinM = Fmt.Millions(current.MinRevenue);
            NextMinM = Fmt.Millions(next.MinRevenue);
        }

        List<DealRow> deals = Repo.GetDeals(companyId, 10);
        List<DemoRow> demos = Repo.GetDemos(companyId, 6);
        List<NotifRow> notifs = Repo.GetNotifications(companyId, 10);
        UnreadCount = notifs.Count(n => !n.IsRead);

        DealsHtml = BuildDeals(deals);
        DemosHtml = BuildDemos(demos);
        NotifHtml = BuildNotifs(notifs);

        List<CustomerReg> regs = Repo.GetCustomerRegs(companyId);
        CustomerRegsHtml = BuildCustomerRegs(regs);
        try { BuildHome(companyId, regs, notifs); } catch { }   // ana sayfa ozeti; hata eski bolumleri etkilemez
    }

    private string BuildCustomerRegs(List<CustomerReg> regs)
    {
        bool en = Culture == "en";
        if (regs.Count == 0)
            return "<div class=\"demo-item\"><div class=\"mod\">" +
                   (en ? "No registered customers yet." : "Henüz kayıtlı müşteri yok.") + "</div></div>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < regs.Count; i++)
        {
            CustomerReg c = regs[i];
            string stClass, stText;
            if (c.Status == "Pending") { stClass = "pending"; stText = en ? "Awaiting approval" : "Onay bekliyor"; }
            else if (c.Status == "Approved" && !c.IsExpired) { stClass = "invoiced"; stText = (en ? "Active · " : "Aktif · ") + c.RemainingDays + (en ? " days left" : " gün kaldı"); }
            else if (c.Status == "Rejected") { stClass = "cancelled"; stText = en ? "Rejected" : "Reddedildi"; }
            else { stClass = "cancelled"; stText = en ? "Expired" : "Süresi doldu"; }

            sb.Append("<div class=\"reg-item\" id=\"reg-" + c.RegistrationId + "\">");
            sb.Append("<div class=\"row\"><div><div class=\"cn\">" + Fmt.Enc(c.CustomerName) + "</div>");
            sb.Append("<div class=\"mod\">" + Fmt.Enc(string.IsNullOrEmpty(c.ServiceType) ? "-" : c.ServiceType));
            if (!string.IsNullOrEmpty(c.ContactPerson)) sb.Append(" · " + Fmt.Enc(c.ContactPerson));
            sb.Append("</div>");
            if (!string.IsNullOrEmpty(c.ContactEmail) || !string.IsNullOrEmpty(c.ContactPhone))
            {
                sb.Append("<div class=\"mod\">");
                if (!string.IsNullOrEmpty(c.ContactEmail)) sb.Append(Fmt.Enc(c.ContactEmail));
                if (!string.IsNullOrEmpty(c.ContactEmail) && !string.IsNullOrEmpty(c.ContactPhone)) sb.Append(" · ");
                if (!string.IsNullOrEmpty(c.ContactPhone)) sb.Append(Fmt.Enc(c.ContactPhone));
                sb.Append("</div>");
            }
            sb.Append("</div>");
            sb.Append("<span class=\"st " + stClass + "\">" + stText + "</span></div>");

            // Satis sicakligi mini bar
            sb.Append("<div class=\"temp-mini\"><i style=\"width:" + c.SalesTemperature + "%\"></i></div>");
            sb.Append("<div class=\"temp-lbl\">" + (en ? "Sales temp: " : "Satış sıcaklığı: ") + c.SalesTemperature + "/100</div>");

            // Yenileme
            if (c.Status == "Approved" && !c.IsExpired)
            {
                if (c.RenewalRequested)
                    sb.Append("<div class=\"reg-note\">" + (en ? "Renewal requested · sent to Fol Yazılım." : "Yenileme talebi Fol Yazılım'a gönderildi.") + "</div>");
                else
                {
                    sb.Append("<form method=\"post\" action=\"RenewCustomerReg.aspx\" style=\"margin:8px 0 0\">");
                    sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + c.RegistrationId + "\" />");
                    sb.Append("<button type=\"submit\" class=\"btn-ghost\" style=\"width:100%\">" +
                              (en ? "Request renewal" : "Kayıt Yenileme Talebi Gönder") + "</button>");
                    sb.Append("</form>");
                }
            }
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    // ===================== Ana sayfa: karsilama, gostergeler, akis, gelismeler =====================
    private static string H(string s) { return System.Web.HttpUtility.HtmlEncode(s ?? ""); }
    private static readonly System.Globalization.CultureInfo TRC = new System.Globalization.CultureInfo("tr-TR");
    private const string ARROW = "<svg width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2.2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M5 12h14M13 6l6 6-6 6\"/></svg>";

    private void BuildHome(int companyId, List<CustomerReg> regs, List<NotifRow> notifs)
    {
        bool en = Culture == "en";
        DateTime nowTr = DateTime.UtcNow.AddHours(3);
        string first = (UserName ?? "").Split(' ')[0];
        Greeting = PartnerHome.GreetingFor(nowTr.Hour, en) + " " + first;
        AvatarHtml = HomeKit.HeroAvatar(Auth.UserId, UserName, en);
        TodayText = nowTr.ToString(en ? "dddd, d MMMM yyyy" : "d MMMM yyyy, dddd", en ? new System.Globalization.CultureInfo("en-US") : TRC);

        // --- veriler
        List<CustomerReg> active = new List<CustomerReg>(); int pending = 0, newMonth = 0;
        foreach (CustomerReg c in regs)
        {
            bool live = c.Status == "Pending" || (c.Status == "Approved" && !c.IsExpired);
            if (!live) continue;
            active.Add(c); if (c.Status == "Pending") pending++;
            if (c.RegisteredAt.Year == nowTr.Year && c.RegisteredAt.Month == nowTr.Month) newMonth++;
        }
        List<DealRow> deals = Repo.GetAllDeals(companyId); int salesYear = 0, salesMonth = 0;
        foreach (DealRow d in deals)
        {
            if (d.Status == "Rejected") continue;
            DateTime b = d.BookedAt.AddHours(3);
            if (b.Year == nowTr.Year) { salesYear++; if (b.Month == nowTr.Month) salesMonth++; }
        }
        List<DemoRow> demos = Repo.GetDemos(companyId, 60); int planned = 0, demoPending = 0; DateTime? nextDemo = null; string nextClient = "";
        foreach (DemoRow d in demos)
        {
            if (d.Status == "Pending") demoPending++;
            if ((d.Status == "Approved" || d.Status == "Rescheduled") && d.ScheduledDate.HasValue && d.ScheduledDate.Value >= nowTr.AddHours(-1))
            {
                planned++;
                if (!nextDemo.HasValue || d.ScheduledDate.Value < nextDemo.Value) { nextDemo = d.ScheduledDate.Value; nextClient = d.ClientName; }
            }
        }
        int tOpen = 0, tWait = 0; try { PartnerHome.TicketCounts(companyId, out tOpen, out tWait); } catch { }

        // --- karsilama ozeti (gunun durumu)
        List<string> parts = new List<string>();
        if (pending > 0) parts.Add("<b>" + pending + (en ? (pending == 1 ? " customer registration" : " customer registrations") : " müşteri kaydınız") + "</b>" + (en ? " awaiting approval" : " onay bekliyor"));
        if (nextDemo.HasValue) parts.Add((en ? "your next demo is " : "sıradaki demonuz ") + "<b>" + H(DemoWhen(nextDemo.Value, nowTr, en)) + "</b>" + (en ? "" : " · " + H(nextClient)));
        if (tWait > 0) parts.Add((en ? "a support request is " : "bir destek talebinde ") + "<b>" + (en ? "waiting for your reply" : "yanıtınız bekleniyor") + "</b>");
        SummaryHtml = parts.Count > 0 ? string.Join(en ? ", " : ", ", parts.ToArray()) + "."
                                     : (en ? "Register a new customer to protect your next opportunity." : "Yeni bir müşteri kaydederek sıradaki fırsatınızı koruma altına alabilirsiniz.");
        if (SummaryHtml.Length > 0) SummaryHtml = char.ToUpper(SummaryHtml[0], TRC) + SummaryHtml.Substring(1);

        // --- gostergeler: her biri ilgili veriye goturur
        StringBuilder k = new StringBuilder();
        k.Append(Kpi(en ? "Registered customers" : "Kayıtlı müşteri", active.Count.ToString(), newMonth > 0 ? "+" + newMonth + (en ? " this month" : " bu ay") : (pending > 0 ? pending + (en ? " awaiting approval" : " onay bekliyor") : (en ? "No new registration this month" : "Bu ay yeni kayıt yok")), newMonth > 0 ? "ok" : "", "#4F7BFF", en ? "My customers" : "Müşterilerim", "#musteriler"));
        k.Append(Kpi(en ? "Sales" : "Satışlar", salesYear.ToString(), (en ? "This year" : "Bu yıl") + (salesMonth > 0 ? (en ? " · " + salesMonth + " this month" : " · " + salesMonth + "’si bu ay") : ""), salesMonth > 0 ? "ok" : "", "#2FD08A", en ? "My sales" : "Satışlarım", "#satislar"));
        k.Append(Kpi(en ? "Open support requests" : "Açık destek talebi", tOpen.ToString(), tWait > 0 ? (en ? "Waiting for your reply" : "Yanıtınız bekleniyor") : (tOpen > 0 ? (en ? "Our team is on it" : "Ekibimiz ilgileniyor") : (en ? "No open request" : "Açık talep yok")), tWait > 0 ? "warn" : "", "#E4467A", en ? "Support requests" : "Destek talepleri", "Tickets.aspx"));
        string acad = AcademyKpi(companyId, en);   // en sagda: Sifirdan Uzmanliga
        if (acad.Length > 0) k.Append(acad);
        KpiHtml = k.ToString();

        // --- musteri ve demo akisi: kayit -> onay -> demo -> satis
        StringBuilder p = new StringBuilder();
        active.Sort(delegate (CustomerReg a, CustomerReg b) { return b.RegisteredAt.CompareTo(a.RegisteredAt); });
        int shown = 0;
        foreach (CustomerReg c in active)
        {
            if (shown++ >= 5) break;
            string key = SalesSuggestions.Norm(c.CustomerName);
            bool hasSale = false, hasDemo = false; DateTime? dDate = null; string dStatus = "";
            foreach (DealRow d in deals) if (d.Status != "Rejected" && SalesSuggestions.Norm(d.CustomerName) == key && key.Length > 0) hasSale = true;
            foreach (DemoRow d in demos) if (SalesSuggestions.Norm(d.ClientName) == key && key.Length > 0 && d.Status != "Rejected") { hasDemo = true; dDate = d.ScheduledDate; dStatus = d.Status; }
            int stage = hasSale ? 4 : hasDemo ? 3 : c.Status == "Approved" ? 2 : 1;
            string label, cls;
            if (stage == 4) { label = en ? "Sale" : "Satış"; cls = "ok"; }
            else if (stage == 3) { label = dStatus == "Completed" ? (en ? "Demo done" : "Demo yapıldı") : (dDate.HasValue ? (en ? "Demo " : "Demo ") + DemoWhen(dDate.Value, nowTr, en) : (en ? "Demo requested" : "Demo talebi")); cls = "violet"; }
            else if (stage == 2) { label = en ? "Approved" : "Onaylandı"; cls = "info"; }
            else { label = en ? "Awaiting approval" : "Onay bekliyor"; cls = "warn"; }
            StringBuilder steps = new StringBuilder();
            for (int i = 1; i <= 4; i++) steps.Append("<i class=\"" + (i <= stage ? "on" : "") + "\"></i>");
            p.Append("<a class=\"ph-pipe lk\" href=\"#reg-" + c.RegistrationId + "\" aria-label=\"" + H(c.CustomerName) + ", " + H(label) + "\">" +
                     "<span class=\"ph-ini\">" + H(Fmt.Initials(c.CustomerName)) + "</span>" +
                     "<span class=\"ph-pipe-tx\"><b>" + H(c.CustomerName) + "</b><small>" + (en ? "Registered " : "Kayıt: ") + c.RegisteredAt.AddHours(3).ToString("d MMMM", TRC) + (string.IsNullOrEmpty(c.ServiceType) ? "" : " · " + H(c.ServiceType)) + "</small></span>" +
                     "<span class=\"ph-steps\" aria-hidden=\"true\">" + steps + "</span><span class=\"ph-chip " + cls + "\">" + H(label) + "</span><span class=\"ph-go\">" + ARROW + "</span></a>");
        }
        PipelineHtml = p.Length > 0 ? p.ToString() : "<div class=\"ph-empty\">" + (en ? "No active customer registration yet. Register a customer to see your pipeline here." : "Henüz aktif müşteri kaydı yok. Müşteri kaydettiğinizde akışınız burada görünecek.") + "</div>";

        // --- son gelismeler: bildirimler + forum gonderileri (tarih sirasiyla; forum yalniz bu kisiye aciksa)
        List<KeyValuePair<DateTime, string>> feed = new List<KeyValuePair<DateTime, string>>(); int fn = 0;
        foreach (NotifRow n in notifs)
        {
            if (fn++ >= 6) break;
            DateTime at = n.CreatedAt ?? DateTime.UtcNow;
            feed.Add(new KeyValuePair<DateTime, string>(at, FeedItem(!n.IsRead, n.Title(Culture), null, FeedWhen(at.AddHours(3), nowTr, en), n.Link)));
        }
        if (ForumRepo.CanAccess)
            try
            {
                foreach (ForumRepo.RecentPost fp in ForumRepo.Recent(Auth.UserId, 4))
                    feed.Add(new KeyValuePair<DateTime, string>(fp.AtUtc, FeedItem(fp.IsNew, fp.Author + (en ? " posted in the forum" : " forumda paylaştı"), ForumRepo.Excerpt(fp, 70), "FORUM · " + FeedWhen(fp.AtUtc.AddHours(3), nowTr, en), "Forum.aspx?post=" + fp.Id)));
            }
            catch { }
        feed.Sort(delegate (KeyValuePair<DateTime, string> a, KeyValuePair<DateTime, string> b) { return b.Key.CompareTo(a.Key); });
        StringBuilder fd = new StringBuilder();
        for (int i = 0; i < feed.Count && i < 4; i++) fd.Append(feed[i].Value);
        FeedHtml = fd.Length > 0 ? fd.ToString() : "<div class=\"ph-empty\">" + (en ? "No updates yet." : "Henüz bir gelişme yok.") + "</div>";

    }

    private static string FeedItem(bool isNew, string title, string excerpt, string when, string link)
    {
        string inner = "<span class=\"ph-dot" + (isNew ? " new" : "") + "\"></span><span class=\"ph-feed-tx\"><span>" + H(title) + "</span>" +
                       (string.IsNullOrEmpty(excerpt) ? "" : "<em>" + H(excerpt) + "</em>") + "<small>" + H(when) + "</small></span>";
        return string.IsNullOrEmpty(link) ? "<div class=\"ph-feed\">" + inner + "</div>" : "<a class=\"ph-feed lk\" href=\"" + H(link) + "\">" + inner + "<span class=\"ph-go\">" + ARROW + "</span></a>";
    }

    /// <summary>Sifirdan Uzmanliga ilerlemesi: diger gostergelerle ayni yapida (sayi + kucuk halka + siradaki adim)</summary>
    private string AcademyKpi(int companyId, bool en)
    {
        try
        {
            List<PathStep> steps = LearningPath.GetSteps(Auth.UserId, companyId);
            if (steps.Count == 0) return "";
            int done = 0; string next = null;
            foreach (PathStep s in steps) { if (s.State == "done") done++; else if (next == null && s.State == "next") next = s.Title; }
            double c = 2 * Math.PI * 15, off = c * (1 - (double)done / steps.Count);
            string ring = "<svg class=\"ph-kpi-ring\" width=\"34\" height=\"34\" viewBox=\"0 0 36 36\" aria-hidden=\"true\"><defs><linearGradient id=\"phKr\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\"><stop offset=\"0\" stop-color=\"#4F7BFF\"/><stop offset=\"1\" stop-color=\"#D650FA\"/></linearGradient></defs>" +
                          "<circle cx=\"18\" cy=\"18\" r=\"15\" fill=\"none\" stroke=\"currentColor\" stroke-opacity=\".14\" stroke-width=\"4.5\"/><circle cx=\"18\" cy=\"18\" r=\"15\" fill=\"none\" stroke=\"url(#phKr)\" stroke-width=\"4.5\" stroke-linecap=\"round\" stroke-dasharray=\"" +
                          c.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "\" stroke-dashoffset=\"" + off.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "\" transform=\"rotate(-90 18 18)\"/></svg>";
            string note = done == steps.Count ? (en ? "Path completed" : "Yol tamamlandı") : next != null ? (en ? "Next: " : "Sıradaki: ") + next : (en ? "Start the path" : "Yola başlayın");
            return "<a class=\"card ph-kpi ph-kpi-acad lk\" href=\"Training.aspx?path=1\" aria-label=\"" + LearningPath.PATH_NAME + ": " + steps.Count + (en ? " steps, " : " adımın ") + done + (en ? " done. River Academy" : "'i tamamlandı. River Akademi") + "\">" +
                   "<span class=\"ph-kpi-h\"><span>" + LearningPath.PATH_NAME + "</span><i style=\"background:#B14AF0\"></i></span>" +
                   "<b>" + done + "<span class=\"ph-kpi-of\">/" + steps.Count + "</span>" + ring + "</b>" +
                   "<small class=\"" + (done == steps.Count ? "ok" : "violet") + " ph-kpi-next\">" + H(note) + "</small>" +
                   "<span class=\"ph-kpi-go\">River " + (en ? "Academy" : "Akademi") + ARROW + "</span></a>";
        }
        catch { return ""; }
    }

    private static string Kpi(string label, string value, string note, string tone, string dot, string target, string href)
    {
        return "<a class=\"card ph-kpi lk\" href=\"" + href + "\" aria-label=\"" + H(label) + ": " + H(value) + ". " + H(target) + "\">" +
               "<span class=\"ph-kpi-h\"><span>" + H(label) + "</span><i style=\"background:" + dot + "\"></i></span>" +
               "<b>" + H(value) + "</b><small class=\"" + tone + "\">" + H(note) + "</small>" +
               "<span class=\"ph-kpi-go\">" + H(target) + ARROW + "</span></a>";
    }
    private static string DemoWhen(DateTime d, DateTime nowTr, bool en)
    {
        int days = (int)(d.Date - nowTr.Date).TotalDays;
        string hm = d.ToString("HH:mm");
        if (days == 0) return (en ? "today " : "bugün ") + hm;
        if (days == 1) return (en ? "tomorrow " : "yarın ") + hm;
        return d.ToString(en ? "MMM d, HH:mm" : "d MMMM HH:mm", en ? new System.Globalization.CultureInfo("en-US") : TRC);
    }
    private static string FeedWhen(DateTime d, DateTime nowTr, bool en)
    {
        int days = (int)(nowTr.Date - d.Date).TotalDays;
        if (days <= 0) return (en ? "Today " : "Bugün ") + d.ToString("HH:mm");
        if (days == 1) return en ? "Yesterday" : "Dün";
        return days < 7 ? days + (en ? " days ago" : " gün önce") : d.ToString("d MMMM", TRC);
    }

    private static double ClampD(double v, double lo, double hi)
    {
        if (v < lo) return lo; if (v > hi) return hi; return v;
    }

    private string BuildDeals(List<DealRow> deals)
    {
        bool en = Culture == "en";
        int cols = IsVendorActing ? 6 : 5;
        if (deals.Count == 0)
            return "<tr><td colspan=\"" + cols + "\" style=\"color:var(--muted)\">" +
                   (en ? "No deals yet." : "Henüz anlaşma yok.") + "</td></tr>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < deals.Count; i++)
        {
            DealRow d = deals[i];
            sb.Append("<tr>");
            sb.Append("<td>" + Fmt.Enc(d.CustomerName) + "</td>");
            sb.Append("<td class=\"num\">" + Fmt.N0(d.Amount) + " ₺</td>");
            sb.Append("<td class=\"num\">" + d.EffectiveRate.ToString("0", CultureInfo.InvariantCulture) + "%</td>");
            sb.Append("<td class=\"num\">" + Fmt.N0(d.CommissionAmount) + " ₺</td>");
            sb.Append("<td><span class=\"st " + Fmt.DealStClass(d.Status) + "\">" +
                      Fmt.DealStLabel(d.Status, Culture) + "</span></td>");
            if (IsVendorActing)
            {
                // Vendor partner adina: yerinde duzenle (alttaki satir acilir) ve sil
                sb.Append("<td class=\"dl-acts\"><button type=\"button\" class=\"btn-ghost dl-edit-btn\" data-dl=\"" + d.DealId + "\" aria-expanded=\"false\">" + (en ? "Edit" : "Düzenle") + "</button>");
                sb.Append("<form method=\"post\" action=\"DeleteOrder.aspx\"><input type=\"hidden\" name=\"id\" value=\"" + d.DealId + "\" /><input type=\"hidden\" name=\"back\" value=\"partner\" />");
                sb.Append("<button type=\"submit\" class=\"btn-ghost dl-del-btn\">" + (en ? "Delete" : "Sil") + "</button></form></td>");
            }
            sb.Append("</tr>");
            if (IsVendorActing) sb.Append(BuildDealEditRow(d, cols));
        }
        return sb.ToString();
    }

    // Yerinde duzenleme formu: OrderEdit.aspx'e gonderilir (ciro ve kademe orada yeniden hesaplanir), sonra Partner.aspx'e donulur
    private string BuildDealEditRow(DealRow d, int cols)
    {
        bool en = Culture == "en";
        CultureInfo inv = CultureInfo.InvariantCulture;
        StringBuilder sb = new StringBuilder();
        sb.Append("<tr class=\"dl-edit\" id=\"dl-edit-" + d.DealId + "\" hidden=\"hidden\"><td colspan=\"" + cols + "\">");
        sb.Append("<form method=\"post\" action=\"OrderEdit.aspx\" class=\"dl-form\">");
        sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + d.DealId + "\" /><input type=\"hidden\" name=\"back\" value=\"partner\" />");
        sb.Append("<label><span>" + (en ? "Customer" : "Müşteri") + "</span><input class=\"input\" name=\"customer\" required=\"required\" maxlength=\"200\" value=\"" + Fmt.Enc(d.CustomerName) + "\" /></label>");
        sb.Append("<label><span>" + (en ? "Title" : "Başlık") + "</span><input class=\"input\" name=\"title\" maxlength=\"200\" value=\"" + Fmt.Enc(d.Title) + "\" /></label>");
        sb.Append("<label><span>" + (en ? "Amount (₺)" : "Tutar (₺)") + "</span><input class=\"input\" name=\"amount\" inputmode=\"decimal\" required=\"required\" value=\"" + d.Amount.ToString("0", inv) + "\" /></label>");
        sb.Append("<label><span>" + (en ? "Commission %" : "Hakediş %") + "</span><input class=\"input\" name=\"rate\" inputmode=\"decimal\" value=\"" + d.EffectiveRate.ToString("0.##", inv) + "\" /></label>");
        sb.Append("<label><span>" + (en ? "Commission (₺)" : "Hakediş (₺)") + "</span><input class=\"input\" name=\"commission\" inputmode=\"decimal\" required=\"required\" value=\"" + d.CommissionAmount.ToString("0", inv) + "\" /></label>");
        sb.Append("<label><span>" + (en ? "Status" : "Durum") + "</span><select name=\"status\" class=\"input st-select\">" + Fmt.StatusOptions(d.Status, Culture) + "</select></label>");
        sb.Append("<div class=\"dl-form-acts\"><button type=\"submit\" class=\"btn-cyan\">" + (en ? "Save" : "Kaydet") + "</button>");
        sb.Append("<button type=\"button\" class=\"btn-ghost dl-cancel\" data-dl=\"" + d.DealId + "\">" + (en ? "Cancel" : "Vazgeç") + "</button></div>");
        sb.Append("</form></td></tr>");
        return sb.ToString();
    }

    private string BuildDemos(List<DemoRow> demos)
    {
        if (demos.Count == 0)
            return "<div class=\"demo-item\"><div class=\"mod\">" +
                   (Culture == "en" ? "No demo requests yet." : "Henüz demo talebi yok.") + "</div></div>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < demos.Count; i++)
        {
            DemoRow d = demos[i];
            sb.Append("<div class=\"demo-item\"><div class=\"row\">");
            sb.Append("<div><div class=\"cn\">" + Fmt.Enc(d.ClientName) + "</div><div class=\"mod\">" +
                      Fmt.Enc(d.ProductModule) + "</div></div>");
            sb.Append("<span class=\"st " + Fmt.DemoStClass(d.Status) + "\">" +
                      Fmt.DemoStLabel(d.Status, Culture) + "</span>");
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }

    private string BuildNotifs(List<NotifRow> notifs)
    {
        if (notifs.Count == 0)
            return "<div class=\"ntf\"><div class=\"dot read\"></div><div><div class=\"b\">" +
                   (Culture == "en" ? "No notifications." : "Bildirim yok.") + "</div></div></div>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < notifs.Count; i++)
        {
            NotifRow n = notifs[i];
            sb.Append("<div class=\"ntf" + (n.IsRead ? " read" : "") + "\"><div class=\"dot\"></div><div>");
            sb.Append("<div class=\"t\">" + Fmt.Enc(n.Title(Culture)) + "</div>");
            sb.Append("<div class=\"b\">" + Fmt.Enc(n.Body(Culture)) + "</div>");
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }
}
