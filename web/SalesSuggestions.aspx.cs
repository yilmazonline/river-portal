using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

// Satis Onerileri: Teklif siparislerinden is ortagi satis kaydi onerileri (yalniz Finans yetkisi)
public partial class SalesSuggestionsPage : Page
{
    public string Culture = "tr", UserName = "", CompanyName = "", Tab = "pending", ListHtml = "", SetupMsg = "", SyncMsg = "";
    public int CntPending, CntBooked, CntDismissed;
    public bool Embed = false;   // Alacaklar sayfasindaki pencerede (iframe) gosterim
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");
    public static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.CanFinance) { Response.Redirect("Default.aspx"); return; }
        Embed = Request.QueryString["embed"] == "1";
        // sayfa menuden kaldirildi: dogrudan gelen (eski baglanti, yer imi) Alacaklar'a yonlendirilir, pencere otomatik acilir
        if (!Embed) { string tt = Request.QueryString["tab"]; Response.Redirect("Receivables.aspx?ss=" + (tt == "booked" || tt == "dismissed" ? tt : "1")); return; }
        Culture = Auth.Culture; UserName = Auth.FullName; CompanyName = Auth.CompanyName;
        string t = Request.QueryString["tab"]; Tab = t == "booked" || t == "dismissed" ? t : "pending";
        try
        {
            string err; int added = SalesSuggestions.Sync(out err);
            if (err != null) SyncMsg = "Teklif'ten okunamadı: " + err;
            else if (added > 0) SyncMsg = added + " yeni sipariş önerisi eklendi.";
            List<OrderSuggestion> pend = SalesSuggestions.List("Pending"), book = SalesSuggestions.List("Booked"), dis = SalesSuggestions.List("Dismissed");
            CntPending = pend.Count; CntBooked = book.Count; CntDismissed = dis.Count;
            ListHtml = Tab == "pending" ? Pending(pend) : Decided(Tab == "booked" ? book : dis, Tab == "booked");
        }
        catch (Exception ex) { SetupMsg = "Satış önerileri tabloları bulunamadı. database\\41_sales_suggestions.sql dosyasını çalıştırın. (" + ex.Message + ")"; }
    }

    private static string D(DateTime? d) { return d.HasValue ? d.Value.ToString("dd.MM.yyyy", TR) : "·"; }

    private string Pending(List<OrderSuggestion> l)
    {
        if (l.Count == 0) return "<div class=\"ss-empty\"><b>Bekleyen öneri yok</b><span>Teklif'e girilen yeni kanal siparişleri burada görünecek.</span></div>";
        List<KeyValuePair<int, string>> partners = SalesSuggestions.PartnerOptions();
        StringBuilder sb = new StringBuilder("<div class=\"ss-list\">");
        foreach (OrderSuggestion s in l)
        {
            StringBuilder opt = new StringBuilder("<option value=\"\">İş ortağı seçin</option>");
            foreach (KeyValuePair<int, string> p in partners)
                opt.Append("<option value=\"" + p.Key + "\"" + (s.SuggestedCompanyId == p.Key ? " selected=\"selected\"" : "") + ">" + E(p.Value) + (s.SuggestedCompanyId == p.Key ? " (önerilen)" : "") + "</option>");
            sb.Append("<article class=\"card ss-card\" data-id=\"" + s.SuggestionId + "\">" +
                "<div class=\"ss-top\"><span class=\"ss-no\">Sipariş #" + s.SourceOrderId + "</span><span class=\"ss-date\">" + D(s.OrderDate) + "</span>" +
                (string.IsNullOrEmpty(s.SalesChannel) ? "" : "<span class=\"rc-st info\">" + E(s.SalesChannel) + "</span>") + "</div>" +
                "<div class=\"ss-main\"><div class=\"ss-info\"><b class=\"ss-cust\">" + E(s.CustomerName ?? "Müşteri adı yok") + "</b>" +
                (string.IsNullOrEmpty(s.ProductSummary) ? "" : "<div class=\"ss-prod\">" + E(s.ProductSummary) + "</div>") +
                "<div class=\"ss-raw\">" + (string.IsNullOrEmpty(s.PartnerNameRaw) ? "Siparişte iş ortağı adı yazmıyor" : "Siparişteki iş ortağı: <b>" + E(s.PartnerNameRaw) + "</b>") + "</div></div>" +
                "<div class=\"ss-amt\">" + E(SalesSuggestions.AmountText(s.NetAmount, s.Currency)) + "</div></div>" +
                (string.IsNullOrEmpty(s.Warning) ? "" : "<div class=\"ss-warn\">" + E(s.Warning) + "</div>") +
                "<div class=\"ss-act\"><label class=\"ss-sel\"><span>İş ortağı</span><select class=\"input\" aria-label=\"Sipariş #" + s.SourceOrderId + " için iş ortağı\">" + opt + "</select></label>" +
                "<button type=\"button\" class=\"btn-cyan ss-book\">Satış olarak kaydet</button>" +
                "<button type=\"button\" class=\"btn-ghost ss-dis\">Kanal satışı değil</button></div>" +
                "<div class=\"ss-msg\" role=\"status\" aria-live=\"polite\"></div></article>");
        }
        return sb.Append("</div>").ToString();
    }

    private string Decided(List<OrderSuggestion> l, bool booked)
    {
        if (l.Count == 0) return "<div class=\"ss-empty\"><b>" + (booked ? "Henüz satış kaydedilmedi" : "Yoksayılan öneri yok") + "</b></div>";
        StringBuilder sb = new StringBuilder("<div class=\"card\" style=\"padding:6px 0\"><div class=\"tbl-scroll\"><table class=\"tk-table\"><thead><tr><th>Sipariş</th><th>Müşteri</th><th>" + (booked ? "İş ortağı" : "Siparişteki iş ortağı") +
                                             "</th><th style=\"text-align:right\">Tutar</th><th>İşlemi yapan</th><th>Tarih</th>" + (booked ? "" : "<th></th>") + "</tr></thead><tbody>");
        foreach (OrderSuggestion s in l)
            sb.Append("<tr data-id=\"" + s.SuggestionId + "\"><td>#" + s.SourceOrderId + "<br><small style=\"color:var(--muted)\">" + D(s.OrderDate) + "</small></td><td>" + E(s.CustomerName) + "</td><td>" +
                      E(booked ? s.DealCompanyName : (s.PartnerNameRaw ?? "·")) + "</td><td style=\"text-align:right;white-space:nowrap\">" + E(SalesSuggestions.AmountText(s.NetAmount, s.Currency)) +
                      "</td><td>" + E(s.DecidedByName) + "</td><td style=\"white-space:nowrap\">" + (s.DecidedAt.HasValue ? s.DecidedAt.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) : "·") + "</td>" +
                      (booked ? "" : "<td style=\"text-align:right\"><button type=\"button\" class=\"btn-ghost ss-restore\">Geri al</button></td>") + "</tr>");
        return sb.Append("</tbody></table></div></div>").ToString();
    }
}
