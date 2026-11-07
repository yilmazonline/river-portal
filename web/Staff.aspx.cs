using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Fol personeli paneli (Portal ve Finans yetkisi olmayan uretici personeli). Yetkiye gore sekillenir:
// destek yetkisi: talep gostergeleri, 14 gunluk akis, ekip yuku, bana atanan talepler; editor: icerik onerileri; herkes: Akademi, dosyalar.
public partial class StaffHomePage : Page
{
    public string Culture = "tr", UserName = "", CompanyName = "", FlashMsg = "";
    public string Greeting = "", TodayText = "", RolesHtml = "", TeamText = "", AvatarHtml = "", TodoHtml = "", CtaHtml = "", NewsHtml = "", KpiHtml = "", ChartHtml = "", LoadHtml = "", MineHtml = "", MineTitle = "", SideHtml = "";
    public int NewCount = 0;
    public bool Sup, Edit;
    private bool en;
    private static string H(string s) { return HomeKit.H(s); }

    private const string I_CHAT = "M4 5h16v11H9l-5 4z", I_INBOX = "M3 13h5l2 3h4l2-3h5M5 5h14l2 8v6H3v-6z", I_BULB = "M12 3a6 6 0 0 0-3.5 10.9V17h7v-3.1A6 6 0 0 0 12 3zM9.5 21h5",
                         I_PLAY = "M8 5.5v13l10.5-6.5z", I_DOWN = "M12 4v11M7 10l5 5 5-5M5 20h14", I_WARN = "M12 8v4M12 16h.01M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18z", I_DONE = "M5 12l5 5 9-10",
                         I_MEDAL = "M12 14a5 5 0 1 0 0-10 5 5 0 0 0 0 10zM9 13.5L8 21l4-2 4 2-1-7.5", I_STAR = "M12 3l2 5 5 2-5 2-2 5-2-5-5-2 5-2z";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorFirm || Auth.IsVendor || Auth.CanFinance) { Response.Redirect(Auth.HomeUrl); return; }
        Culture = Auth.Culture; en = Culture == "en";
        UserName = Auth.FullName; CompanyName = Auth.CompanyName;
        Sup = Auth.IsSupportAgent; Edit = Auth.CanEditTraining;
        int uid = Auth.UserId, cid = Auth.CompanyId;
        DateTime nowTr = DateTime.UtcNow.AddHours(3);
        Greeting = PartnerHome.GreetingFor(nowTr.Hour, en) + " " + (UserName ?? "").Split(' ')[0];
        TodayText = nowTr.ToString(en ? "dddd, d MMMM yyyy" : "d MMMM yyyy, dddd", en ? new CultureInfo("en-US") : HomeKit.TRC);
        AvatarHtml = HomeKit.HeroAvatar(uid, UserName, en);
        if (Sup) RolesHtml += "<span class=\"vs-role\">" + (en ? "Support Specialist" : "Destek Uzmanı") + "</span>";
        if (Edit) RolesHtml += "<span class=\"vs-role\">" + (en ? "Academy Editor" : "Akademi Editörü") + "</span>";
        TeamText = "";   // rol rozetleri zaten ustte

        // --- veriler (her biri kendi korumasinda)
        List<Ticket> all = new List<Ticket>();
        if (Sup) { try { all = Repo.GetAllTickets(); } catch { } }
        List<Ticket> mine = new List<Ticket>(); int myAwait = 0, unassigned = 0, highMine = 0, todayIn = 0, myResolved = 0; double myHours = 0; Ticket oldest = null, unassignedHigh = null;
        foreach (Ticket t in all)
        {
            bool open = t.Status == "New" || t.Status == "InProgress" || t.Status == "WaitingCustomer";
            if (t.CreatedAt.AddHours(3).Date == nowTr.Date) todayIn++;
            if (open && !t.AssignedToUserId.HasValue) { unassigned++; if (unassignedHigh == null && (t.Priority == "High" || t.Priority == "Critical")) unassignedHigh = t; }
            if (t.AssignedToUserId == uid)
            {
                if (open) { mine.Add(t); if (t.Priority == "High" || t.Priority == "Critical") highMine++; }
                if (t.Status == "New" || t.Status == "InProgress") { myAwait++; if (oldest == null || t.UpdatedAt < oldest.UpdatedAt) oldest = t; }
                if (t.ResolvedAt.HasValue) { DateTime r = t.ResolvedAt.Value.AddHours(3); if (r.Year == nowTr.Year && r.Month == nowTr.Month) { myResolved++; myHours += (t.ResolvedAt.Value - t.CreatedAt).TotalHours; } }
            }
        }
        List<PathStep> steps = new List<PathStep>(); try { steps = LearningPath.GetSteps(uid, cid); } catch { }
        PathStep next = null; int no = 0, done = 0;
        for (int i = 0; i < steps.Count; i++) { if (steps[i].State == "done") done++; else if (next == null && steps[i].State == "next") { next = steps[i]; no = i + 1; } }
        int sugg = 0; DateTime? suggAt = null; if (Edit) { try { sugg = StaffHome.RecentSuggestions(7, out suggAt); } catch { } }
        int dl7 = 0, expiring = 0; string expTitle = null; try { StaffHome.MyTransfers(uid, out dl7, out expiring, out expTitle); } catch { }

        // --- BUGUN ICIN: yetkiye gore adaylar, ilk ucu
        List<string> todo = new List<string>();
        if (Sup)
        {
            if (myAwait > 0) todo.Add(HomeKit.TodoRow("orange", I_CHAT, myAwait + (en ? " requests await your reply" : " talep sizden yanıt bekliyor"),
                (en ? "Oldest: " : "En eskisi: ") + oldest.TicketNo + " · " + Waiting(oldest.UpdatedAt) + (en ? "" : " yanıt bekliyor"), "TicketDetail.aspx?id=" + oldest.TicketId));
            else todo.Add(HomeKit.TodoRow("green", I_DONE, en ? "No request is waiting for your reply" : "Sizden yanıt bekleyen talep yok", en ? "Well done; check the queue for new requests" : "Harika; yeni talepler için kuyruğa göz atabilirsiniz", "Tickets.aspx"));
            if (unassigned > 0) todo.Add(HomeKit.TodoRow("blue", I_INBOX, (en ? unassigned + " unassigned requests in the queue" : "Kuyrukta " + unassigned + " atanmamış talep var"),
                unassignedHigh != null ? (en ? "High priority: " : "Yüksek öncelikli: ") + (unassignedHigh.CompanyName ?? unassignedHigh.TicketNo) : (en ? "Pick one from the queue" : "Kuyruktan bir talep üstlenebilirsiniz"), "Tickets.aspx"));
        }
        if (Edit && sugg > 0) todo.Add(HomeKit.TodoRow("violet", I_BULB, (en ? sugg + " content suggestions in the last 7 days" : "Son 7 günde " + sugg + " içerik önerisi geldi"),
            (en ? "Latest " : "En yenisi ") + (suggAt.HasValue ? HomeKit.When(suggAt.Value.AddHours(3), nowTr, en).ToLower(HomeKit.TRC) : "") + " · " + (en ? "Editor Panel" : "Editör Paneli"), "Training.aspx"));
        if (expiring > 0) todo.Add(HomeKit.TodoRow("orange", I_WARN, (en ? expiring + " share(s) expire within 24 hours" : expiring + " paylaşımın süresi 24 saat içinde doluyor"), "“" + (expTitle ?? "") + "”", "Transfers.aspx"));
        if (next != null) todo.Add(HomeKit.TodoRow("violet", I_PLAY, (en ? "Your next training: " : "Sıradaki eğitiminiz: ") + next.Title, LearningPath.PATH_NAME + " · " + no + (en ? ". step" : ". adım") + (next.Percent > 0 ? " · %" + next.Percent + (en ? " watched" : " izlendi") : ""), "Training.aspx?path=1"));
        try
        {
            List<VersionAnnouncement> vs = Repo.GetPublishedVersions();
            if (vs.Count > 0) todo.Add(HomeKit.TodoRow("cyan", I_STAR, (en ? "Latest release: " : "Son sürüm: ") + vs[0].ProductName + " " + vs[0].VersionNo, en ? "Share the release notes with customers" : "Sürüm notlarını müşterilerle paylaşabilirsiniz", "Versions.aspx"));
        }
        catch { }
        if (todo.Count < 3) todo.Add(HomeKit.TodoRow("violet", I_PLAY, en ? "Explore River Academy" : "River Akademi'yi keşfedin", en ? "Short, hands-on trainings" : "Kısa ve uygulamalı eğitimler", "Training.aspx"));
        StringBuilder tb = new StringBuilder(); for (int i = 0; i < todo.Count && i < 3; i++) tb.Append(todo[i]); TodoHtml = tb.ToString();

        CtaHtml = Sup ? "<a class=\"btn-cyan\" href=\"Tickets.aspx\">" + (en ? "Open the support queue" : "Destek kuyruğunu aç") + "</a>" : "<a class=\"btn-cyan\" href=\"Training.aspx\">" + (en ? "Go to River Academy" : "River Akademi'ye git") + "</a>";
        CtaHtml += Edit ? "<a class=\"btn-ghost\" href=\"Training.aspx\">" + (en ? "Editor Panel" : "Editör Paneli") + "</a>" : "<a class=\"btn-ghost\" href=\"Transfers.aspx\">" + (en ? "My files" : "Dosyalarım") + "</a>";

        try { NewsHtml = HomeKit.News(cid, uid, en, nowTr, out NewCount); } catch { NewsHtml = HomeKit.Empty(en ? "No updates yet" : "Henüz yeni bir gelişme yok", en ? "Updates will appear here." : "Gelişmeler burada görünecek.", null, null, I_STAR); }

        // --- gostergeler
        StringBuilder k = new StringBuilder();
        if (Sup)
        {
            int replies = 0; try { replies = StaffHome.RepliesToday(); } catch { }
            k.Append(HomeKit.Kpi(en ? "My open requests" : "Bana atanan açık talepler", mine.Count.ToString(), mine.Count == 0 ? (en ? "Nothing assigned" : "Atanmış açık talep yok") : (highMine > 0 ? highMine + (en ? " high priority · " : " yüksek öncelikli · ") : "") + myAwait + (en ? " awaiting reply" : " yanıt bekliyor"), myAwait > 0 ? "warn" : "", "#F0A35E", en ? "My requests" : "Taleplerim", "Tickets.aspx"));
            k.Append(HomeKit.Kpi(en ? "Unassigned in queue" : "Kuyrukta atanmamış", unassigned.ToString(), unassigned == 0 ? (en ? "Queue is clear" : "Kuyruk temiz") : (unassignedHigh != null ? (en ? "Includes high priority" : "Yüksek öncelikli var") : (en ? "Ready to pick up" : "Üstlenilmeyi bekliyor")), unassigned > 0 ? "violet" : "ok", "#B14AF0", en ? "Support queue" : "Destek kuyruğu", "Tickets.aspx"));
            k.Append(HomeKit.Kpi(en ? "Today in / out" : "Bugün gelen / giden", todayIn + " <span class=\"vs-sep\">/</span> " + replies, todayIn + (en ? " new requests · " : " yeni talep · ") + replies + (en ? " replies sent" : " yanıt gönderildi"), "cyan", "#22B8D8", en ? "Daily flow" : "Günlük akış", "Tickets.aspx"));
            k.Append(HomeKit.Kpi(en ? "Resolved by me this month" : "Bu ay çözdüğüm", myResolved.ToString(), myResolved > 0 ? (en ? "Avg. resolution " : "Ortalama çözüm ") + Hours(myHours / myResolved) : (en ? "None resolved this month yet" : "Bu ay henüz çözülen yok"), myResolved > 0 ? "ok" : "", "#2FD08A", en ? "Resolved requests" : "Çözülen talepler", "Tickets.aspx?view=resolved"));
        }
        else
        {
            int ms = 0, mp = 0; try { PartnerHome.WatchStats(cid, new DateTime(nowTr.Year, nowTr.Month, 1).AddHours(-3), out ms, out mp); } catch { }
            int valid = 0; string myCert = null; try { PartnerHome.CertStats(cid, uid, out valid, out myCert); } catch { }
            if (steps.Count > 0) k.Append(HomeKit.Kpi(LearningPath.PATH_NAME, done + "/" + steps.Count, next != null ? (en ? "Next: " : "Sıradaki: ") + next.Title : (en ? "Path completed" : "Yol tamamlandı"), next != null ? "violet" : "ok", "#B14AF0", "River " + (en ? "Academy" : "Akademi"), "Training.aspx?path=1"));
            k.Append(HomeKit.Kpi(en ? "Team watch time" : "Ekibin izleme süresi", H(PartnerHome.Dur(ms / 60, en)), ms >= 60 ? (en ? "This month · " + mp + " people" : "Bu ay · " + mp + " kişi") : (en ? "No viewing this month yet" : "Bu ay henüz izleme yok"), ms >= 60 ? "cyan" : "", "#22B8D8", en ? "River Academy" : "River Akademi", "Training.aspx"));
            k.Append(HomeKit.Kpi(en ? "Certificates" : "Sertifikalar", valid.ToString(), myCert != null ? (en ? "Yours: " : "Sizinki: ") + myCert : (en ? "Valid in Fol Yazılım" : "Fol Yazılım'da geçerli"), "", "#F2C14E", en ? "Certificates" : "Sertifikalar", "Certificates.aspx"));
            k.Append(HomeKit.Kpi(en ? "My file downloads" : "Dosya indirmeleri", dl7.ToString(), en ? "Last 7 days" : "Son 7 gün", dl7 > 0 ? "cyan" : "", "#4F7BFF", en ? "My files" : "Dosyalarım", "Transfers.aspx"));
        }
        KpiHtml = k.ToString();

        // --- destek: 14 gunluk akis + ekip yuku + bana atananlar
        if (Sup)
        {
            try { ChartHtml = BuildChart(); } catch { ChartHtml = ""; }
            try { LoadHtml = BuildLoad(uid); } catch { LoadHtml = ""; }
            MineTitle = en ? "Requests assigned to me" : "Bana atanan talepler";
            MineHtml = BuildMine(mine, nowTr);
        }
        else
        {
            MineTitle = en ? "Latest releases" : "Son sürümler";
            try { MineHtml = BuildVersions(); } catch { MineHtml = ""; }
        }
        SideHtml = BuildSide(steps, next, no, done, sugg, suggAt, dl7, expiring, expTitle, nowTr);
    }

    // ---------------------------------------------------------------- bolumler
    private string BuildChart()
    {
        DateTime[] ds; int[] inc, res; StaffHome.Flow(14, out ds, out inc, out res);
        int fr = -1; try { fr = StaffHome.AvgFirstResponseMinutes(14); } catch { }
        return ChartSvg(ds, inc, res, fr);
    }
    /// <summary>Cizim: veriyi parametre olarak alir (veritabanindan bagimsiz)</summary>
    private string ChartSvg(DateTime[] ds, int[] inc, int[] res, int fr)
    {
        int mx = 4; for (int i = 0; i < ds.Length; i++) { mx = Math.Max(mx, Math.Max(inc[i], res[i])); }
        mx = (int)Math.Ceiling(mx / 5.0) * 5;
        const int W = 620, Hh = 190, pad = 26; double bw = (W - pad * 2) / (double)ds.Length;
        CultureInfo ic = CultureInfo.InvariantCulture; StringBuilder s = new StringBuilder();
        foreach (int v in new[] { 0, mx / 2, mx })
        {
            double y = Hh - 20 - v / (double)mx * (Hh - 40);
            s.Append("<line x1=\"" + pad + "\" x2=\"" + (W - pad) + "\" y1=\"" + y.ToString("0.0", ic) + "\" y2=\"" + y.ToString("0.0", ic) + "\" class=\"vs-grid\"/><text x=\"4\" y=\"" + (y + 4).ToString("0.0", ic) + "\" class=\"vs-axis\">" + v + "</text>");
        }
        int tin = 0, tres = 0;
        for (int i = 0; i < ds.Length; i++)
        {
            tin += inc[i]; tres += res[i];
            double x = pad + i * bw, ha = inc[i] / (double)mx * (Hh - 40), hr = res[i] / (double)mx * (Hh - 40);
            string tip = ds[i].ToString("d MMMM", HomeKit.TRC) + ": " + inc[i] + (en ? " in, " : " gelen, ") + res[i] + (en ? " resolved" : " çözülen");
            s.Append("<g><title>" + H(tip) + "</title><rect x=\"" + (x + bw * .16).ToString("0.0", ic) + "\" y=\"" + (Hh - 20 - ha).ToString("0.0", ic) + "\" width=\"" + (bw * .3).ToString("0.0", ic) + "\" height=\"" + ha.ToString("0.0", ic) + "\" rx=\"3\" fill=\"url(#vsA)\"/>" +
                     "<rect x=\"" + (x + bw * .52).ToString("0.0", ic) + "\" y=\"" + (Hh - 20 - hr).ToString("0.0", ic) + "\" width=\"" + (bw * .3).ToString("0.0", ic) + "\" height=\"" + hr.ToString("0.0", ic) + "\" rx=\"3\" fill=\"#2FD08A\" fill-opacity=\".85\"/></g>" +
                     "<text x=\"" + (x + bw * .5).ToString("0.0", ic) + "\" y=\"" + (Hh - 4) + "\" text-anchor=\"middle\" class=\"vs-axis\">" + ds[i].Day + "</text>");
        }
        string rate = tin > 0 ? "%" + Math.Min(999, (int)Math.Round(tres * 100.0 / tin)) : "·";
        return "<svg viewBox=\"0 0 " + W + " " + Hh + "\" width=\"100%\" height=\"" + Hh + "\" role=\"img\" aria-label=\"" + (en ? "Requests in and resolved over the last 14 days" : "Son 14 günde gelen ve çözülen talepler") + "\">" +
               "<defs><linearGradient id=\"vsA\" x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\"><stop offset=\"0\" stop-color=\"#B98CFF\"/><stop offset=\"1\" stop-color=\"#6C5BFF\"/></linearGradient></defs>" + s + "</svg>" +
               "<div class=\"vs-chart-foot\"><span><b>" + tin + "</b>" + (en ? "in" : "gelen") + "</span><span><b>" + tres + "</b>" + (en ? "resolved" : "çözülen") + "</span><span><b>" + rate + "</b>" + (en ? "resolution rate" : "çözüm oranı") + "</span>" +
               "<span><b>" + (fr >= 0 ? H(PartnerHome.Dur(fr, en)) : "·") + "</b>" + (en ? "avg. first reply" : "ort. ilk yanıt") + "</span></div>";
    }

    private string BuildLoad(int uid)
    {
        int un; List<StaffHome.Load> l = StaffHome.TeamLoad(out un);
        return LoadHtmlFor(l, un, uid);
    }
    private string LoadHtmlFor(List<StaffHome.Load> l, int un, int uid)
    {
        int mx = Math.Max(un, 1); foreach (StaffHome.Load x in l) mx = Math.Max(mx, x.Open);
        string[] c = { "#B98CFF", "#5FCDE6", "#F0A35E", "#2FD08A", "#8FB0FF" };
        StringBuilder sb = new StringBuilder("<div class=\"vs-load\">"); int n = 0;
        foreach (StaffHome.Load x in l)
        {
            if (n >= 5) break;
            sb.Append("<div class=\"vs-load-it\"><span>" + H(x.Name) + (x.UserId == uid ? (en ? " (you)" : " (siz)") : "") + "</span><span class=\"vs-load-bar\"><i style=\"width:" + (x.Open * 100 / mx) + "%;background:" + c[n % 5] + "\"></i></span><b>" + x.Open + "</b></div>");
            n++;
        }
        sb.Append("<div class=\"vs-load-it\"><span>" + (en ? "Queue (unassigned)" : "Kuyruk (atanmamış)") + "</span><span class=\"vs-load-bar\"><i style=\"width:" + (un * 100 / mx) + "%;background:#8E9AC0\"></i></span><b>" + un + "</b></div>");
        if (n == 0 && un == 0) return HomeKit.Empty(en ? "No open requests" : "Açık talep yok", en ? "The whole team is clear." : "Ekibin üzerinde açık talep bulunmuyor.", null, null, I_DONE);
        return sb.Append("</div>").ToString();
    }

    private string BuildMine(List<Ticket> mine, DateTime nowTr)
    {
        mine.Sort(delegate (Ticket a, Ticket b)
        {
            int ra = a.Status == "WaitingCustomer" ? 1 : 0, rb = b.Status == "WaitingCustomer" ? 1 : 0; if (ra != rb) return ra.CompareTo(rb);
            int pa = Pri(a.Priority), pb = Pri(b.Priority); if (pa != pb) return pb.CompareTo(pa);
            return a.UpdatedAt.CompareTo(b.UpdatedAt);
        });
        StringBuilder sb = new StringBuilder(); int c = 0;
        foreach (Ticket t in mine)
        {
            if (c++ >= 4) break;
            double hrs = (DateTime.UtcNow - t.UpdatedAt).TotalHours;
            string sla, cls;
            if (t.Status == "WaitingCustomer") { sla = en ? "Waiting for the customer" : "Müşterinin yanıtı bekleniyor"; cls = ""; }
            else { sla = (en ? "Awaiting your reply " : "") + Waiting(t.UpdatedAt) + (en ? "" : " yanıtınızı bekliyor"); cls = hrs >= 24 ? "vs-late" : hrs >= 4 ? "vs-soon" : ""; }
            string pl, pc; switch (t.Priority) { case "Critical": pl = en ? "Critical" : "Kritik"; pc = "warn"; break; case "High": pl = en ? "High" : "Yüksek"; pc = "warn"; break; case "Low": pl = en ? "Low" : "Düşük"; pc = "mute"; break; default: pl = "Normal"; pc = "info"; break; }
            sb.Append("<a class=\"cu-tk lk\" href=\"TicketDetail.aspx?id=" + t.TicketId + "\"><span class=\"cu-tk-tx\"><small>" + H(t.TicketNo) + " · " + H(t.CompanyName) + "</small><b>" + H(t.Subject) + "</b><em class=\"" + cls + "\">" + H(sla) + "</em></span><span class=\"ph-chip " + pc + "\">" + H(pl) + "</span></a>");
        }
        return sb.Length > 0 ? sb.ToString() : HomeKit.Empty(en ? "No open requests assigned to you" : "Size atanmış açık talep yok", en ? "Pick one from the queue when you're ready." : "Hazır olduğunuzda kuyruktan bir talep üstlenebilirsiniz.", en ? "Open the queue" : "Kuyruğu aç", "Tickets.aspx", I_INBOX);
    }

    private string BuildVersions()
    {
        List<VersionAnnouncement> vs = Repo.GetPublishedVersions(); StringBuilder sb = new StringBuilder(); int c = 0;
        foreach (VersionAnnouncement v in vs)
        {
            if (c++ >= 4) break;
            string label = HomeKit.VersionLabel(v);
            sb.Append("<a class=\"cu-tk lk\" href=\"Versions.aspx\"><span class=\"cu-tk-tx\"><b>" + H(v.ProductName) + " v" + H(v.VersionNo) + "</b><em>" + (en ? "Released " : "Yayın: ") + H(HomeKit.VersionDate(v, en)) + "</em></span>" +
                      (label != null ? "<span class=\"ph-chip info\">" + H(label) + "</span>" : "") + "</a>");
        }
        return sb.Length > 0 ? sb.ToString() : HomeKit.Empty(en ? "No releases yet" : "Henüz sürüm duyurusu yok", en ? "Release notes will appear here." : "Sürüm notları burada görünecek.", null, null, I_STAR);
    }

    private string BuildSide(List<PathStep> steps, PathStep next, int no, int done, int sugg, DateTime? suggAt, int dl7, int expiring, string expTitle, DateTime nowTr)
    {
        StringBuilder sb = new StringBuilder();
        if (steps.Count > 0)
        {
            int pct = done * 100 / steps.Count;
            sb.Append("<a class=\"vs-mini lk\" href=\"Training.aspx?path=1\"><span class=\"cu-ic violet\"><svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + I_PLAY + "\"/></svg></span>" +
                      "<span><b>" + LearningPath.PATH_NAME + " · " + done + "/" + steps.Count + "</b><em>" + H(next != null ? (en ? "Next: " : "Sıradaki: ") + next.Title : (en ? "Path completed" : "Yol tamamlandı")) + "</em></span><span class=\"cu-bar vs-mbar\"><i style=\"width:" + pct + "%\"></i></span></a>");
        }
        if (Edit) sb.Append(Mini("violet", I_BULB, sugg > 0 ? (en ? sugg + " content suggestions this week" : "Bu hafta " + sugg + " içerik önerisi") : (en ? "No new content suggestions" : "Yeni içerik önerisi yok"), en ? "Editor Panel · River Academy" : "Editör Paneli · River Akademi", "Training.aspx"));
        sb.Append(Mini("cyan", I_DOWN, dl7 > 0 ? (en ? "Your shares were downloaded " + dl7 + " times" : "Paylaşımlarınız " + dl7 + " kez indirildi") : (en ? "No downloads this week" : "Bu hafta indirme yok"), en ? "File sharing · last 7 days" : "Dosya paylaşımı · son 7 gün", "Transfers.aspx"));
        if (expiring > 0) sb.Append(Mini("orange", I_WARN, en ? expiring + " share(s) expire within 24 hours" : expiring + " paylaşımın süresi 24 saat içinde doluyor", "“" + (expTitle ?? "") + "”", "Transfers.aspx"));
        else
        {
            int valid = 0; string myCert = null; try { PartnerHome.CertStats(Auth.CompanyId, Auth.UserId, out valid, out myCert); } catch { }
            sb.Append(Mini("gold", I_MEDAL, myCert != null ? (en ? "Your certificate: " : "Sertifikanız: ") + myCert : (en ? "No certificate yet" : "Henüz sertifikanız yok"), en ? "Certificates" : "Sertifikalar", "Certificates.aspx"));
        }
        return sb.ToString();
    }

    private static string Mini(string tone, string icon, string title, string sub, string href)
    {
        return "<a class=\"vs-mini lk\" href=\"" + href + "\"><span class=\"cu-ic " + tone + "\"><svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + icon + "\"/></svg></span>" +
               "<span><b>" + H(title) + "</b><em>" + H(sub) + "</em></span></a>";
    }
    private static int Pri(string p) { return p == "Critical" ? 3 : p == "High" ? 2 : p == "Normal" ? 1 : 0; }
    /// <summary>"45 dakikadır" / "26 saattir" / "3 gündür" (Turkce ek birime gore); EN: "for 26 hours"</summary>
    private string Waiting(DateTime utc)
    {
        TimeSpan a = DateTime.UtcNow - utc;
        if (a.TotalHours < 1) { int m = Math.Max(1, (int)a.TotalMinutes); return en ? "for " + m + " min" : m + " dakikadır"; }
        if (a.TotalHours < 48) { int h = (int)a.TotalHours; return en ? "for " + h + " hours" : h + " saattir"; }
        int d = (int)a.TotalDays; return en ? "for " + d + " days" : d + " gündür";
    }
    private string Hours(double h) { return h < 24 ? h.ToString("0.#", en ? CultureInfo.InvariantCulture : HomeKit.TRC) + (en ? " hours" : " saat") : (h / 24).ToString("0.#", en ? CultureInfo.InvariantCulture : HomeKit.TRC) + (en ? " days" : " gün"); }
}
