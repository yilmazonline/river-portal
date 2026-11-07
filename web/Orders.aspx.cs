using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Siparisler: bayinin TUM anlasmalari. Vendor icin duzenle/sil/durum guncelle.
public partial class OrdersPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public bool IsVendor = false;
    public bool IsImpersonating = false;
    public string OrdersHtml = "";
    public string FlashMsg = "";
    public string FlashErr = "";
    public int Count = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (Auth.IsPartnerUserLimited) { Response.Redirect(Auth.HomeUrl); return; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        IsVendor = Auth.IsVendor;
        IsImpersonating = Auth.IsImpersonating;
        bool en = Culture == "en";

        string ok = Request.QueryString["ok"];
        if (ok == "status") FlashMsg = en ? "Order status updated." : "Sipariş durumu güncellendi.";
        else if (ok == "edit") FlashMsg = en ? "Order updated." : "Sipariş güncellendi.";
        else if (ok == "del") FlashMsg = en ? "Order deleted." : "Sipariş silindi.";

        int companyId = Auth.CompanyId;
        List<DealRow> deals = Repo.GetAllDeals(companyId);
        Count = deals.Count;
        OrdersHtml = BuildRows(deals);
    }

    private string BuildRows(List<DealRow> deals)
    {
        bool en = Culture == "en";
        int cols = IsVendor ? 8 : 7;
        if (deals.Count == 0)
            return "<tr><td colspan=\"" + cols + "\" style=\"color:var(--muted)\">" +
                   (en ? "No orders yet." : "Henüz sipariş yok.") + "</td></tr>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < deals.Count; i++)
        {
            DealRow d = deals[i];
            sb.Append("<tr>");
            sb.Append("<td>" + Fmt.Enc(d.CustomerName) + "</td>");
            sb.Append("<td>" + Fmt.Enc(d.Title) + "</td>");
            sb.Append("<td class=\"num\">" + Fmt.N0(d.Amount) + " ₺</td>");
            sb.Append("<td class=\"num\">" + d.EffectiveRate.ToString("0", CultureInfo.InvariantCulture) + "%</td>");
            sb.Append("<td class=\"num\">" + Fmt.N0(d.CommissionAmount) + " ₺</td>");
            sb.Append("<td>" + d.BookedAt.ToString("dd.MM.yyyy", new CultureInfo("tr-TR")) + "</td>");

            if (IsVendor)
            {
                // Durum: duzenlenebilir select (otomatik submit)
                sb.Append("<td><form method=\"post\" action=\"UpdateOrderStatus.aspx\" style=\"margin:0\">");
                sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + d.DealId + "\" />");
                sb.Append("<select name=\"status\" class=\"st-select\" data-auto=\"1\">" +
                          Fmt.StatusOptions(d.Status, Culture) + "</select>");
                sb.Append("</form></td>");
                // Islemler
                sb.Append("<td style=\"text-align:right;white-space:nowrap\">");
                sb.Append("<a class=\"btn-ghost\" href=\"OrderEdit.aspx?id=" + d.DealId + "\">" +
                          (en ? "Edit" : "Düzenle") + "</a> ");
                sb.Append("<form method=\"post\" action=\"DeleteOrder.aspx\" style=\"display:inline;margin:0\">");
                sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + d.DealId + "\" />");
                sb.Append("<button type=\"submit\" class=\"btn-ghost del-btn\">" + (en ? "Delete" : "Sil") + "</button>");
                sb.Append("</form>");
                sb.Append("</td>");
            }
            else
            {
                sb.Append("<td><span class=\"st " + Fmt.DealStClass(d.Status) + "\">" +
                          Fmt.DealStLabel(d.Status, Culture) + "</span></td>");
            }
            sb.Append("</tr>");
        }
        return sb.ToString();
    }
}
