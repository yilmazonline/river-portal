using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

// Alacak takibi ana ekrani: yalniz Finans yetkisi
public partial class ReceivablesPage : Page
{
    public string Culture = "tr", UserName = "", RowsHtml = "", KpiHtml = "", TotalHtml = "", FlowHtml = "", AgingHtml = "", SyncMsg = "", SetupMsg = "";
    public string CompanyOptionsHtml = "";   // manuel alacak penceresi: bayi + musteri cariler
    public int CntAll, CntUnassigned, CntOpen, CntOverdue, CntWeek, CntPaid, CntClosed;
    // Satis Onerileri (menuden tasindi): bekleyen sayisi rozet; ?ss=1|booked|dismissed ile pencere acik gelir
    public int SsPending = 0; public string SsOpen = "";
    public string RecentHtml = "";   // "Son tahsilatlar" yan tablosu
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");
    private static string M(decimal v) { return v.ToString("#,##0", TR) + " TL"; }
    private static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

    protected void Page_Load(object sender, EventArgs e)
    {
        // manuel alacak penceresi icin cari listesi (bayi + musteri, gruplu)
        try
        {
            System.Text.StringBuilder co = new System.Text.StringBuilder("<option value=\"\">Daha sonra seçeceğim</option>");
            foreach (string[] g in new[] { new[] { "Partner", "Bayiler" }, new[] { "Customer", "Müşteriler" } })
            {
                co.Append("<optgroup label=\"" + g[1] + "\">");
                foreach (CompanyLite c in Repo.GetCompaniesByType(g[0])) co.Append("<option value=\"" + c.CompanyId + "\">" + System.Web.HttpUtility.HtmlEncode(c.Name) + "</option>");
                co.Append("</optgroup>");
            }
            CompanyOptionsHtml = co.ToString();
        }
        catch { }
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorFirm || !Auth.CanFinance) { Response.Redirect("Default.aspx"); return; }
        try { SsPending = SalesSuggestions.PendingCount(); } catch { SsPending = 0; }
        string ss = Request.QueryString["ss"]; SsOpen = ss == "booked" || ss == "dismissed" ? ss : ss == "1" ? "pending" : "";
        Culture = Auth.Culture; UserName = Auth.FullName;

        string err; int added = ReceivableRepo.Sync(Request.QueryString["sync"] == "1", out err);
        if (err != null) SetupMsg = "Teklif siparişleri okunamadı: " + err + " (database\\37_receivables.sql çalıştırıldı mı ve portalın SQL kullanıcısının " + ReceivableRepo.SourceTable + " üzerinde SELECT yetkisi var mı?)";
        else if (added > 0) SyncMsg = added + " yeni sipariş alacak listesine eklendi.";

        List<Receivable> all; List<RcvInstallment> inst; decimal paidMonth; Dictionary<int, List<RcvPayment>> pays;
        try { all = ReceivableRepo.GetAll(); inst = ReceivableRepo.GetOpenInstallments(); paidMonth = ReceivableRepo.PaidBetween(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today.AddDays(1)); pays = ReceivableRepo.PaymentsByReceivable(); }
        catch (Exception ex)
        {
            // yalniz tablo gercekten yoksa kurulum mesaji; diger hatalar oldugu gibi gosterilir
            SetupMsg = ex.Message.IndexOf("Invalid object name", StringComparison.OrdinalIgnoreCase) >= 0
                ? "Alacak tabloları bulunamadı: database\\37_receivables.sql çalıştırılmalı. (" + ex.Message + ")"
                : "Alacaklar okunurken hata oluştu: " + ex.Message;
            return;
        }

        // ---- donemler (hafta pazartesi baslar, ay takvim ayi)
        DateTime today = DateTime.Today;
        int dow = ((int)today.DayOfWeek + 6) % 7;                // pazartesi = 0
        DateTime wkStart = today.AddDays(-dow), wkEnd = wkStart.AddDays(7), nwEnd = wkEnd.AddDays(7);
        DateTime moStart = new DateTime(today.Year, today.Month, 1), moEnd = moStart.AddMonths(1), nmEnd = moEnd.AddMonths(1);
        decimal overdue = 0, week = 0, nweek = 0, month = 0, nmonth = 0; decimal[] age = new decimal[4]; decimal[] flow = new decimal[12];
        Dictionary<int, bool> weekIds = new Dictionary<int, bool>();
        foreach (RcvInstallment i in inst)
        {
            decimal l = i.Left;
            if (i.DueDate < today)
            {
                overdue += l; int d = (int)(today - i.DueDate).TotalDays;
                age[d <= 30 ? 0 : d <= 60 ? 1 : d <= 90 ? 2 : 3] += l;
            }
            else
            {
                if (i.DueDate < wkEnd) { week += l; weekIds[i.ReceivableId] = true; }
                else if (i.DueDate < nwEnd) nweek += l;
                if (i.DueDate < moEnd) month += l; else if (i.DueDate < nmEnd) nmonth += l;
                int w = (int)((i.DueDate - wkStart).TotalDays / 7); if (w >= 0 && w < 12) flow[w] += l;
            }
        }
        TotalHtml = BuildTotal(all, inst, today);
        KpiHtml = Kpi("Gecikmiş", overdue, "bad", "Vadesi geçmiş, tahsil edilmemiş") + Kpi("Bu hafta", week, "", wkStart.ToString("dd.MM") + " ile " + wkEnd.AddDays(-1).ToString("dd.MM") + " arası") +
                  Kpi("Gelecek hafta", nweek, "", wkEnd.ToString("dd.MM") + " ile " + nwEnd.AddDays(-1).ToString("dd.MM") + " arası") + Kpi("Bu ay", month, "", TR.DateTimeFormat.GetMonthName(today.Month) + " kalan") +
                  Kpi("Gelecek ay", nmonth, "", TR.DateTimeFormat.GetMonthName(moEnd.Month)) + Kpi("Bu ay tahsil edilen", paidMonth, "ok", TR.DateTimeFormat.GetMonthName(today.Month));

        // ---- nakit akis grafigi (gecikmis + 12 hafta)
        decimal mx = overdue; foreach (decimal f in flow) if (f > mx) mx = f; if (mx <= 0) mx = 1;
        StringBuilder fb = new StringBuilder();
        fb.Append(Bar(overdue, mx, "Gecikmiş", "bad"));
        for (int w = 0; w < 12; w++) fb.Append(Bar(flow[w], mx, wkStart.AddDays(7 * w).ToString("dd.MM"), w == 0 ? "now" : ""));
        FlowHtml = fb.ToString();
        string[] al = { "1-30 gün", "31-60 gün", "61-90 gün", "90+ gün" };
        StringBuilder ab = new StringBuilder();
        decimal at = age[0] + age[1] + age[2] + age[3];
        for (int k = 0; k < 4; k++)
            ab.Append("<div class=\"rc-age\"><span>" + al[k] + "</span><div class=\"rc-age-bar\"><i style=\"width:" + (at > 0 ? (age[k] / at * 100m).ToString("0.#", CultureInfo.InvariantCulture) : "0") + "%\"></i></div><b>" + M(age[k]) + "</b></div>");
        AgingHtml = ab.ToString();

        // ---- liste
        StringBuilder rb = new StringBuilder();
        foreach (Receivable x in all)
        {
            string tab; string chip;
            int? dd = x.DaysToDue;
            if (x.Status == "Paid") { tab = "paid"; chip = "<span class=\"rc-st ok\">" + (x.IsLegacy ? "Tahsil edildi (geçmiş)" : "Tahsil edildi") + "</span>"; CntPaid++; }
            else if (x.Status == "Closed") { tab = "closed"; chip = "<span class=\"rc-st mute\">Kapatıldı</span>"; CntClosed++; }
            else
            {
                tab = x.Status == "Unassigned" ? "unassigned" : "open";
                if (x.Status == "Unassigned") CntUnassigned++; else CntOpen++;
                if (dd.HasValue && dd.Value < 0) { chip = "<span class=\"rc-st bad\">" + (-dd.Value) + " gün geçti</span>"; CntOverdue++; }
                else if (dd.HasValue && dd.Value == 0) chip = "<span class=\"rc-st warn\">Bugün</span>";
                else chip = "<span class=\"rc-st info\">" + (dd.HasValue ? dd.Value + " gün kaldı" : "Tarih yok") + "</span>";
                if (x.Status == "Unassigned") chip = "<span class=\"rc-st warn\">Cari seçilmedi</span> " + chip;
                if (x.Status == "Partial") chip += " <span class=\"rc-st mute\">Kısmi</span>";
            }
            bool inWeek = weekIds.ContainsKey(x.ReceivableId);
            if (inWeek) CntWeek++;
            CntAll++;
            string who = x.CompanyName != null ? E(x.CompanyName) : "<em class=\"rc-dim\">Cari seçilmedi</em>";
            string sub = E(string.Join(" · ", new[] { x.PartnerName, x.CustomerName }.Where2()));
            string chKey = x.SalesChannel == ReceivableRepo.CH_CHANNEL ? "kanal" : (x.SalesChannel == ReceivableRepo.CH_DIRECT ? "direkt" : "none");
            string search = ((x.IsManual ? "manuel " : "") + x.SourceOrderId + " " + x.SalesChannel + " " + x.CompanyName + " " + x.PartnerName + " " + x.CustomerName + " " + x.SalesRep + " " + x.ProductSummary + " " + x.Tags).ToLowerInvariant();
            rb.Append("<tr data-tab=\"" + tab + "\" data-od=\"" + (x.IsOverdue ? "1" : "0") + "\" data-wk=\"" + (inWeek ? "1" : "0") + "\" data-ch=\"" + chKey + "\" data-s=\"" + E(search) + "\" onclick=\"location.href='ReceivableDetail.aspx?id=" + x.ReceivableId + "'\">");
            rb.Append("<td class=\"rc-no\">" + (x.IsManual ? "<span class=\"rc-man\" title=\"Elle girilen alacak\">Manuel</span>" : "#" + x.SourceOrderId) + "</td>");
            rb.Append("<td><div class=\"rc-who\">" + who + (x.Warning != null ? " <span class=\"rc-warn\" title=\"" + E(x.Warning) + "\">!</span>" : "") + "</div><div class=\"rc-sub\">" + sub + "</div></td>");
            rb.Append("<td>" + (x.SalesChannel != null ? "<span class=\"rc-ch " + chKey + "\">" + E(x.SalesChannel) + "</span>" : "<span class=\"rc-dim\">Belirtilmemiş</span>") + "</td>");
            rb.Append("<td class=\"rc-num\">" + ReceivableRepo.Money(x.GrossAmount, x.Currency) + "</td>");
            rb.Append("<td class=\"rc-num\"><b>" + ReceivableRepo.Money(x.Remaining, x.Currency) + "</b></td>");
            // tahsil edilenlerde son tahsilat tarihi (birden fazla odemede sayisi); acik alacaklarda siradaki vade
            string dateCell;
            if (x.Status == "Paid")
                dateCell = x.LastPay.HasValue
                    ? x.LastPay.Value.ToString("dd.MM.yyyy") + (x.PayCount > 1 ? " <span class=\"rc-dim\">(" + x.PayCount + " ödeme)</span>" : "")
                    : "<span class=\"rc-dim\" title=\"Bu sipariş için sisteme ödeme kaydı girilmemiş (geçmiş kayıt)\">·</span>";
            else
                dateCell = x.IsOpen && x.NextDue.HasValue ? x.NextDue.Value.ToString("dd.MM.yyyy") : "<span class=\"rc-dim\">·</span>";
            rb.Append("<td class=\"rc-date\">" + dateCell + "</td>");
            rb.Append("<td>" + chip + "</td></tr>");
        }
        RecentHtml = BuildRecent(all, pays);
        RowsHtml = rb.Length > 0 ? rb.ToString() : "<tr><td colspan=\"7\" class=\"rc-empty\">Henüz alacak yok. Teklif'te faturası gönderilen siparişler burada listelenir.</td></tr>";
    }

    /// <summary>Son tahsilatlar: en yeni 25 odeme. Nitelik: alacagi kapatan son odeme "Kapanis", digerleri "Ara tahsilat"; odeme sekli altta.</summary>
    private static string BuildRecent(List<Receivable> all, Dictionary<int, List<RcvPayment>> pays)
    {
        Dictionary<int, Receivable> byId = new Dictionary<int, Receivable>();
        foreach (Receivable r in all) byId[r.ReceivableId] = r;
        List<KeyValuePair<int, RcvPayment>> l = new List<KeyValuePair<int, RcvPayment>>();
        foreach (KeyValuePair<int, List<RcvPayment>> kv in pays) foreach (RcvPayment p in kv.Value) l.Add(new KeyValuePair<int, RcvPayment>(kv.Key, p));
        l.Sort(delegate (KeyValuePair<int, RcvPayment> a, KeyValuePair<int, RcvPayment> b) { int c = b.Value.PayDate.CompareTo(a.Value.PayDate); return c != 0 ? c : b.Value.PaymentId.CompareTo(a.Value.PaymentId); });
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < l.Count && i < 25; i++)
        {
            Receivable r; if (!byId.TryGetValue(l[i].Key, out r)) continue;
            RcvPayment p = l[i].Value; List<RcvPayment> pl = pays[l[i].Key];
            bool closing = r.Status == "Paid" && pl[pl.Count - 1].PaymentId == p.PaymentId;
            string firm = r.CompanyName ?? r.CustomerName ?? r.PartnerName ?? ("#" + r.SourceOrderId);
            sb.Append("<tr onclick=\"location.href='ReceivableDetail.aspx?id=" + r.ReceivableId + "'\">" +
                "<td><span class=\"rc-st " + (closing ? "ok" : "info") + "\">" + (closing ? "Kapanış" : "Ara tahsilat") + "</span>" +
                (string.IsNullOrEmpty(p.Method) ? "" : "<div class=\"rc-sub\">" + E(p.Method) + "</div>") + "</td>" +
                "<td class=\"rc-date\">" + p.PayDate.ToString("dd.MM.yyyy") + "</td>" +
                "<td><div class=\"rc-who\">" + E(firm) + "</div></td></tr>");
        }
        return sb.Length > 0 ? sb.ToString() : "<tr><td colspan=\"3\" class=\"rc-dim\">Henüz tahsilat kaydı yok.</td></tr>";
    }

    /// <summary>Toplam bekleyen alacak: tahsil edilmemis tum vade tutarlari (cari secimi bekleyenler dahil), doviz bazinda.
    /// Dovizler TL'ye cevrilmez, ayri gosterilir. Dagilim cubugu TL tutarin gecikmis / 30 gun icinde / daha sonra kirilimi.</summary>
    private static string BuildTotal(List<Receivable> all, List<RcvInstallment> inst, DateTime today)
    {
        Dictionary<int, Receivable> byId = new Dictionary<int, Receivable>();
        foreach (Receivable r in all) byId[r.ReceivableId] = r;
        Dictionary<string, decimal> byCur = new Dictionary<string, decimal>();
        Dictionary<int, bool> ids = new Dictionary<int, bool>(), unassigned = new Dictionary<int, bool>();
        decimal tlLate = 0, tl30 = 0, tlLater = 0;
        foreach (RcvInstallment i in inst)
        {
            Receivable r; if (!byId.TryGetValue(i.ReceivableId, out r)) continue;
            string cur = string.IsNullOrEmpty(r.Currency) ? "TL" : r.Currency.ToUpperInvariant();
            decimal l = i.Left; if (l <= 0) continue;
            decimal c; byCur.TryGetValue(cur, out c); byCur[cur] = c + l;
            ids[i.ReceivableId] = true; if (r.Status == "Unassigned") unassigned[i.ReceivableId] = true;
            if (cur == "TL") { if (i.DueDate < today) tlLate += l; else if (i.DueDate < today.AddDays(30)) tl30 += l; else tlLater += l; }
        }
        decimal tl; byCur.TryGetValue("TL", out tl);
        StringBuilder others = new StringBuilder();
        foreach (string cur in new[] { "USD", "EUR" })
        {
            decimal v; if (!byCur.TryGetValue(cur, out v) || v <= 0) continue;
            others.Append((others.Length == 0 ? "+ " : " · ") + v.ToString("#,##0", TR) + " " + cur);
        }
        foreach (KeyValuePair<string, decimal> kv in byCur)
            if (kv.Key != "TL" && kv.Key != "USD" && kv.Key != "EUR" && kv.Value > 0) others.Append((others.Length == 0 ? "+ " : " · ") + kv.Value.ToString("#,##0", TR) + " " + E(kv.Key));
        decimal sum = tlLate + tl30 + tlLater;
        Func<decimal, string> pct = delegate (decimal v) { return sum > 0 ? (v / sum * 100m).ToString("0.##", CultureInfo.InvariantCulture) : "0"; };
        string bar = sum > 0
            ? "<div class=\"rc-tbar\" role=\"img\" aria-label=\"Gecikmiş " + M(tlLate) + ", 30 gün içinde " + M(tl30) + ", daha sonra " + M(tlLater) + "\">" +
              "<i class=\"late\" style=\"width:" + pct(tlLate) + "%\"></i><i class=\"soon\" style=\"width:" + pct(tl30) + "%\"></i><i class=\"later\" style=\"width:" + pct(tlLater) + "%\"></i></div>" +
              "<div class=\"rc-tleg\"><span><i class=\"late\"></i>Gecikmiş <b>" + M(tlLate) + "</b></span><span><i class=\"soon\"></i>30 gün içinde <b>" + M(tl30) + "</b></span><span><i class=\"later\"></i>Daha sonra <b>" + M(tlLater) + "</b></span></div>"
            : "";
        return "<div class=\"rc-total\"><div class=\"rc-tmain\"><div class=\"rc-k\">Toplam bekleyen alacak</div>" +
               "<div class=\"rc-tv\">" + M(tl) + "</div>" +
               (others.Length > 0 ? "<div class=\"rc-tcur\">" + others + "</div>" : "") +
               "<div class=\"rc-h\">" + ids.Count + " alacak, henüz tahsil edilmemiş" + (unassigned.Count > 0 ? " · " + unassigned.Count + " tanesi cari seçimi bekliyor" : "") + "</div></div>" +
               "<div class=\"rc-tside\">" + bar + "</div></div>";
    }

    private static string Kpi(string label, decimal v, string cls, string hint)
    {
        return "<div class=\"rc-kpi " + cls + "\"><div class=\"rc-k\">" + label + "</div><div class=\"rc-v\">" + M(v) + "</div><div class=\"rc-h\">" + hint + "</div></div>";
    }
    private static string Bar(decimal v, decimal mx, string label, string cls)
    {
        int h = v > 0 ? Math.Max(4, (int)(v / mx * 150m)) : 2;
        return "<div class=\"rc-fb " + cls + "\" title=\"" + label + " · " + M(v) + "\"><div class=\"rc-fv\">" + (v > 0 ? (v >= 1000000m ? (v / 1000000m).ToString("0.#", TR) + "M" : (v / 1000m).ToString("0", TR) + "B") : "") + "</div><i style=\"height:" + h + "px\"></i><span>" + label + "</span></div>";
    }
}
