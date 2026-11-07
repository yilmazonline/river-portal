<%@ WebHandler Language="C#" Class="ReceivableExportHandler" %>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.SessionState;

// Alacak listesi -> Excel (CSV, UTF-8 BOM, noktali virgul). Yalniz Finans yetkisi.
public class ReceivableExportHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        if (!Auth.IsLoggedIn || !Auth.IsVendorFirm || !Auth.CanFinance) { ctx.Response.StatusCode = 403; return; }
        string tab = ctx.Request.QueryString["tab"] ?? "all";
        CultureInfo TR = new CultureInfo("tr-TR");
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Sipariş No;Sorumlu;Cari;Cari Türü;Bayi;Müşteri;Satış Temsilcisi;Ürün;Sipariş Tarihi;Fatura Tarihi;Tutar (KDV hariç);KDV %;Alacak (KDV dahil);Tahsil Edilen;Son Tahsilat;Tahsilatlar;Kalan;Sıradaki Vade;Gün;Durum;Para Birimi");
        Dictionary<int, List<RcvPayment>> pays = ReceivableRepo.PaymentsByReceivable();
        foreach (Receivable x in ReceivableRepo.GetAll())
        {
            List<RcvPayment> pl; pays.TryGetValue(x.ReceivableId, out pl);
            string t = x.Status == "Paid" ? "paid" : x.Status == "Closed" ? "closed" : x.Status == "Unassigned" ? "unassigned" : "open";
            bool ok = tab == "all" || (tab == "active" && (t == "open" || t == "unassigned")) || (tab == "overdue" && x.IsOverdue) || t == tab ||
                      (tab == "week" && x.IsOpen && x.DaysToDue.HasValue && x.DaysToDue.Value >= 0 && x.DaysToDue.Value < 7);
            string chq = ctx.Request.QueryString["ch"] ?? "";
            if (chq == "kanal" && x.SalesChannel != ReceivableRepo.CH_CHANNEL) ok = false;
            if (chq == "direkt" && x.SalesChannel != ReceivableRepo.CH_DIRECT) ok = false;
            if (chq == "none" && x.SalesChannel != null) ok = false;
            if (!ok) continue;
            string st = x.Status == "Paid" ? "Tahsil edildi" : x.Status == "Closed" ? "Kapatıldı" : x.Status == "Unassigned" ? "Cari seçilmedi" : x.Status == "Partial" ? "Kısmi tahsil" : "Açık";
            sb.AppendLine(string.Join(";", new[] {
                x.SourceOrderId.ToString(), C(x.SalesChannel), C(x.CompanyName), x.CompanyType == "Partner" ? "Bayi" : (x.CompanyType == "Customer" ? "Müşteri" : ""), C(x.PartnerName), C(x.CustomerName), C(x.SalesRep), C(x.ProductSummary),
                x.OrderDate.HasValue ? x.OrderDate.Value.ToString("dd.MM.yyyy") : "", x.InvoiceDate.HasValue ? x.InvoiceDate.Value.ToString("dd.MM.yyyy") : "",
                x.NetAmount.ToString("0.00", TR), x.VatRate.ToString("0.##", TR), x.GrossAmount.ToString("0.00", TR), x.Paid.ToString("0.00", TR), x.LastPay.HasValue ? x.LastPay.Value.ToString("dd.MM.yyyy") : "", C(pl == null ? "" : string.Join("; ", pl.ConvertAll(p => p.PayDate.ToString("dd.MM.yyyy") + " " + p.Amount.ToString("0.00", TR)).ToArray())), x.Remaining.ToString("0.00", TR),
                x.IsOpen && x.NextDue.HasValue ? x.NextDue.Value.ToString("dd.MM.yyyy") : "", x.DaysToDue.HasValue ? x.DaysToDue.Value.ToString() : "", st, x.Currency }));
        }
        ctx.Response.Clear();
        ctx.Response.ContentType = "text/csv";
        ctx.Response.ContentEncoding = new UTF8Encoding(true);
        ctx.Response.AddHeader("Content-Disposition", "attachment; filename=\"alacaklar_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv\"");
        ctx.Response.BinaryWrite(new byte[] { 0xEF, 0xBB, 0xBF });
        ctx.Response.Write(sb.ToString());
    }
    // Excel'de formul enjeksiyonunu onle; ayraci ve tirnagi kacir
    private static string C(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Replace("\r", " ").Replace("\n", " ");
        if ("=+-@".IndexOf(s[0]) >= 0) s = "'" + s;
        return s.IndexOfAny(new[] { ';', '"' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }
}
