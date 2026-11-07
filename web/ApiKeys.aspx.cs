using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;

public partial class ApiKeysPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string FlashMsg = "";
    public string NewKey = "";
    public string NewClientId = "";
    public string ClientsHtml = "";
    public string CompanyOptions = "";

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendor) { Response.Redirect("Default.aspx"); return; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            string op = (Request.Form["op"] ?? "").Trim();
            if (op == "create")
            {
                string name = (Request.Form["name"] ?? "").Trim();
                int cid; int.TryParse(Request.Form["cid"], out cid);
                if (name.Length > 0) { string clId; NewKey = Repo.CreateApiClient(name, cid, out clId); NewClientId = clId; FlashMsg = "Anahtar üretildi."; }
            }
            else if (op == "toggle")
            {
                int id; bool active;
                if (int.TryParse(Request.Form["id"], out id) && bool.TryParse(Request.Form["active"], out active))
                { Repo.SetApiClientActive(id, active); FlashMsg = active ? "Anahtar etkinleştirildi." : "Anahtar pasifleştirildi."; }
            }
            else if (op == "delete")
            {
                int id;
                if (int.TryParse(Request.Form["id"], out id)) { Repo.DeleteApiClient(id); FlashMsg = "Anahtar silindi."; }
            }
        }

        CompanyOptions = BuildCompanyOptions();
        ClientsHtml = BuildClients();
    }

    private string BuildCompanyOptions()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<option value=\"\">Firma seçin...</option>");
        System.Collections.Generic.List<CompanyLite> partners = Repo.GetCompaniesByType("Partner");
        System.Collections.Generic.List<CompanyLite> customers = Repo.GetCompaniesByType("Customer");
        if (partners.Count > 0)
        {
            sb.Append("<optgroup label=\"Bayiler\">");
            for (int i = 0; i < partners.Count; i++) sb.Append("<option value=\"" + partners[i].CompanyId + "\">" + Fmt.Enc(partners[i].Name) + "</option>");
            sb.Append("</optgroup>");
        }
        if (customers.Count > 0)
        {
            sb.Append("<optgroup label=\"Müşteriler\">");
            for (int i = 0; i < customers.Count; i++) { if (customers[i].Name == "Harici Ürün Destekleri") continue; sb.Append("<option value=\"" + customers[i].CompanyId + "\">" + Fmt.Enc(customers[i].Name) + "</option>"); }
            sb.Append("</optgroup>");
        }
        return sb.ToString();
    }

    private string BuildClients()
    {
        DataTable dt = Repo.GetApiClients();
        if (dt.Rows.Count == 0) return "<p style=\"color:var(--muted);font-size:13px\">Henüz anahtar tanımlı değil.</p>";
        StringBuilder sb = new StringBuilder();
        string th = " style=\"text-align:left;padding:8px 10px;border-bottom:2px solid var(--stroke-2);color:var(--muted);font-size:12px;font-weight:600\"";
        string td = " style=\"padding:9px 10px;border-bottom:1px solid var(--stroke-2)\"";
        sb.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px\"><thead><tr>");
        sb.Append("<th" + th + ">Ürün</th><th" + th + ">Firma</th><th" + th + ">Client ID</th><th" + th + ">Anahtar Ön Eki</th><th" + th + ">Durum</th><th" + th + ">Oluşturma</th><th" + th + ">Son Kullanım</th><th" + th + "></th>");
        sb.Append("</tr></thead><tbody>");
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            DataRow r = dt.Rows[i];
            int id = (int)r["ApiClientId"];
            bool active = (bool)r["IsActive"];
            string prefix = r["KeyPrefix"] == DBNull.Value ? "" : (string)r["KeyPrefix"];
            string created = r["CreatedAt"] == DBNull.Value ? "" : ((DateTime)r["CreatedAt"]).ToTrLocal().ToString("dd.MM.yyyy", TR);
            string last = r["LastUsedAt"] == DBNull.Value ? "-" : ((DateTime)r["LastUsedAt"]).ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR);
            string statusBadge = active
                ? "<span style=\"color:#12A150;font-weight:600\">Aktif</span>"
                : "<span style=\"color:var(--muted)\">Pasif</span>";
            sb.Append("<tr>");
            string coName = dt.Columns.Contains("CompanyName") && r["CompanyName"] != DBNull.Value ? (string)r["CompanyName"] : "-";
            string clientId = dt.Columns.Contains("ClientId") && r["ClientId"] != DBNull.Value ? (string)r["ClientId"] : "-";
            sb.Append("<td" + td + ">" + Fmt.Enc((string)r["Name"]) + "</td>");
            sb.Append("<td" + td + ">" + Fmt.Enc(coName) + "</td>");
            sb.Append("<td" + td + " style=\"padding:9px 10px;border-bottom:1px solid var(--stroke-2);font-family:monospace;font-size:11px\">" + Fmt.Enc(clientId) + "</td>");
            sb.Append("<td" + td + " style=\"padding:9px 10px;border-bottom:1px solid var(--stroke-2);font-family:monospace\">" + Fmt.Enc(prefix) + "…</td>");
            sb.Append("<td" + td + ">" + statusBadge + "</td>");
            sb.Append("<td" + td + ">" + created + "</td>");
            sb.Append("<td" + td + ">" + last + "</td>");
            sb.Append("<td" + td + " style=\"padding:9px 10px;border-bottom:1px solid var(--stroke-2);text-align:right\"><div style=\"display:inline-flex;gap:6px;justify-content:flex-end\">" +
                "<form method=\"post\" action=\"ApiKeys.aspx\" style=\"margin:0\"><input type=\"hidden\" name=\"op\" value=\"toggle\" /><input type=\"hidden\" name=\"id\" value=\"" + id + "\" />" +
                "<input type=\"hidden\" name=\"active\" value=\"" + (active ? "false" : "true") + "\" />" +
                "<button class=\"btn-ghost\" type=\"submit\" style=\"padding:6px 14px;font-size:12px\">" + (active ? "Pasifleştir" : "Etkinleştir") + "</button></form>" +
                "<form method=\"post\" action=\"ApiKeys.aspx\" style=\"margin:0\" onsubmit=\"return confirm('Bu API anahtarı kalıcı olarak silinsin mi? Bu anahtarı kullanan ürünler artık talep gönderemez.');\">" +
                "<input type=\"hidden\" name=\"op\" value=\"delete\" /><input type=\"hidden\" name=\"id\" value=\"" + id + "\" />" +
                "<button class=\"btn-ghost\" type=\"submit\" style=\"padding:6px 12px;font-size:12px;border-color:rgba(255,61,113,.45);color:#FF6B8A\">Sil</button></form>" +
                "</div></td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table>");
        return sb.ToString();
    }
}
