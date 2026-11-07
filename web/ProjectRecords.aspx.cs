using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Proje Kayitlari: firma bazli hizmet/proje ekstresi (yalniz uretici tarafi).
public partial class ProjectRecordsPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyOptions = "";
    public string Rows = "";
    public int FilterCid = 0;
    public int Count = 0;

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorSide) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;

        int.TryParse(Request.QueryString["cid"], out FilterCid);
        CompanyOptions = BuildCompanyOptions(FilterCid);

        List<ProjectRecordRow> list = Repo.GetProjectRecords(FilterCid);
        Count = list.Count;
        Rows = Build(list);
    }

    private string Build(List<ProjectRecordRow> list)
    {
        if (list.Count == 0) return "<tr><td colspan=\"6\" style=\"color:var(--muted);padding:14px\">Kayıt bulunamadı.</td></tr>";
        StringBuilder sb = new StringBuilder();
        string td = " style=\"padding:10px 12px;border-bottom:1px solid var(--stroke-2);vertical-align:top;font-size:13px\"";
        for (int i = 0; i < list.Count; i++)
        {
            ProjectRecordRow r = list[i];
            string sure = r.Pool == "project"
                ? (r.Hours.HasValue ? r.Hours.Value.ToString("0.##", TR) + " saat" : "-")
                : (r.Minutes.HasValue ? r.Minutes.Value + " dk" : "-");
            string poolBadge = r.Pool == "project"
                ? "<span class=\"st invoiced\">Proje</span>"
                : "<span class=\"st approved\">Destek</span>";
            sb.Append("<tr class=\"pr-row\" style=\"cursor:pointer\" onclick=\"location.href='ProjectRecordDetail.aspx?id=" + r.RecordId + "'\">");
            sb.Append("<td" + td + ">" + r.CreatedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) + "</td>");
            sb.Append("<td" + td + "><b>" + Fmt.Enc(r.CompanyName) + "</b></td>");
            sb.Append("<td" + td + ">" + poolBadge + "</td>");
            sb.Append("<td" + td + " style=\"padding:10px 12px;border-bottom:1px solid var(--stroke-2);vertical-align:top;font-weight:600\">" + Fmt.Enc(sure) + "</td>");
            sb.Append("<td" + td + "><div class=\"pr-content\">" + Fmt.Enc(r.Content) + "</div></td>");
            sb.Append("<td" + td + " style=\"padding:10px 12px;border-bottom:1px solid var(--stroke-2);vertical-align:top;font-size:12px;color:var(--muted)\">" + Fmt.Enc(r.Recipients) + "<br /><span style=\"color:var(--dim)\">" + Fmt.Enc(r.CreatedByName) + "</span></td>");
            sb.Append("</tr>");
        }
        return sb.ToString();
    }

    private string BuildCompanyOptions(int sel)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<option value=\"0\">Tüm firmalar</option>");
        List<CompanyLite> partners = Repo.GetCompaniesByType("Partner");
        List<CompanyLite> customers = Repo.GetCompaniesByType("Customer");
        Action<string, List<CompanyLite>> grp = delegate (string label, List<CompanyLite> l)
        {
            if (l.Count == 0) return;
            sb.Append("<optgroup label=\"" + label + "\">");
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Name == "Harici Ürün Destekleri") continue;
                string selAttr = l[i].CompanyId == sel ? " selected=\"selected\"" : "";
                sb.Append("<option value=\"" + l[i].CompanyId + "\"" + selAttr + ">" + Fmt.Enc(l[i].Name) + "</option>");
            }
            sb.Append("</optgroup>");
        };
        grp("Bayiler", partners); grp("Müşteriler", customers);
        return sb.ToString();
    }
}
