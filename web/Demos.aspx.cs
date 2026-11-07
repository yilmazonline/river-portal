using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Demo Talepleri: bayinin tum demo taleplerini listeler.
public partial class DemosPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public bool IsVendor = false;
    public string DemosHtml = "";
    public int Count = 0;

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (Auth.IsPartnerUserLimited) { Response.Redirect(Auth.HomeUrl); return; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        IsVendor = Auth.IsVendor;

        List<DemoRow> demos = Repo.GetAllDemos(Auth.CompanyId);
        Count = demos.Count;
        DemosHtml = Build(demos);
    }

    private string Dt(DateTime? d)
    {
        return d == null ? "-" : d.Value.ToString("dd.MM.yyyy HH:mm", TR);
    }

    private string StatusLabel(string s)
    {
        bool en = Culture == "en";
        if (s == "Approved") return en ? "Approved" : "Onaylandı";
        if (s == "Rescheduled") return en ? "Rescheduled" : "Yeniden Planlandı";
        if (s == "Completed") return en ? "Completed" : "Tamamlandı";
        if (s == "Rejected") return en ? "Rejected" : "Reddedildi";
        return en ? "Pending" : "Beklemede";
    }

    private string StatusClass(string s)
    {
        if (s == "Approved" || s == "Completed") return "invoiced";
        if (s == "Rejected") return "cancelled";
        if (s == "Rescheduled") return "approved";
        return "pending";
    }

    private string Build(List<DemoRow> demos)
    {
        bool en = Culture == "en";
        if (demos.Count == 0)
            return "<tr><td colspan=\"5\" style=\"color:var(--muted)\">" +
                   (en ? "No demo requests yet." : "Henüz demo talebi yok.") + "</td></tr>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < demos.Count; i++)
        {
            DemoRow d = demos[i];
            string dates = Dt(d.PreferredDate);
            if (d.AltDate1 != null) dates += " / " + Dt(d.AltDate1);
            if (d.AltDate2 != null) dates += " / " + Dt(d.AltDate2);

            sb.Append("<tr>");
            sb.Append("<td>" + Fmt.Enc(d.ClientName) + "</td>");
            sb.Append("<td>" + Fmt.Enc(d.ProductModule) + "</td>");
            sb.Append("<td style=\"font-size:12.5px\">" + dates + "</td>");
            sb.Append("<td>" + (d.ScheduledDate != null ? Dt(d.ScheduledDate) : "-") + "</td>");
            sb.Append("<td><span class=\"st " + StatusClass(d.Status) + "\">" + StatusLabel(d.Status) + "</span></td>");
            sb.Append("</tr>");
        }
        return sb.ToString();
    }
}
