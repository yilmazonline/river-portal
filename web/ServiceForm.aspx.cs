using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;

// Hizmet Formu: Destek (cozulmus talep) veya Proje (ProjectRecords) kaydi + secili kisilere mail.
public partial class ServiceFormPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string FlashMsg = "";
    public string FlashErr = "";
    public string CompanyOptions = "";
    public string CompanyJson = "[]";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorSide) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;

        if (Request.QueryString["ok"] == "sent") FlashMsg = "Hizmet bildirimi gönderildi.";

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) Save();

        CompanyOptions = BuildCompanyOptions();
        CompanyJson = BuildCompanyJson();
    }

    private void Save()
    {
        string type = (Request.Form["type"] == "proje") ? "proje" : "destek";
        int cid; int.TryParse(Request.Form["cid"], out cid);
        string content = (Request.Unvalidated.Form["content"] ?? "").Trim();
        if (cid <= 0) { FlashErr = "Lütfen firma seçin."; return; }
        if (content.Length == 0) { FlashErr = "Lütfen hizmet içeriğini yazın."; return; }

        string pool = "support";
        int minutes = 0; decimal hours = 0m;
        if (type == "proje")
        {
            pool = (Request.Form["poolsource"] == "project") ? "project" : "support";
        }
        if (pool == "project")
        {
            string hs = (Request.Form["hours"] ?? "").Trim().Replace(",", ".");
            decimal.TryParse(hs, NumberStyles.Any, CultureInfo.InvariantCulture, out hours);
            if (hours <= 0) { FlashErr = "Lütfen proje süresini (saat) girin."; return; }
        }
        else
        {
            int.TryParse(Request.Form["minutes"], out minutes);
            minutes = Fmt.NormMinutes(minutes);
        }

        // Alicilar
        List<UserRow> firmUsers = Repo.GetActiveUsersForCompany(cid);
        var byId = new Dictionary<int, UserRow>();
        for (int i = 0; i < firmUsers.Count; i++) byId[firmUsers[i].UserId] = firmUsers[i];
        var toEmails = new List<string>();
        var names = new List<string>();
        string[] tsel = Request.Form.GetValues("targets");
        if (tsel != null)
            for (int i = 0; i < tsel.Length; i++)
            {
                int uid; if (int.TryParse(tsel[i], out uid) && byId.ContainsKey(uid))
                { if (!string.IsNullOrEmpty(byId[uid].Email)) toEmails.Add(byId[uid].Email); names.Add(byId[uid].FullName); }
            }
        string extra = (Request.Form["extraEmail"] ?? "").Trim();
        if (extra.Length > 0 && extra.IndexOf('@') > 0) { toEmails.Add(extra); names.Add(extra); }

        string firmName = "";
        try { CompanyInfo co = Repo.GetCompany(cid); if (co != null) firmName = co.Name; } catch { }
        string recipientsStr = string.Join(", ", names.ToArray());

        // Kayit olustur
        if (type == "destek")
        {
            string subj = content.Length > 70 ? content.Substring(0, 70) + "…" : content;
            string no; Repo.CreateServiceTicket(cid, Auth.UserId, subj, content, minutes, firmName, Auth.FullName, out no);
        }
        else
        {
            Repo.AddProjectRecord(cid, Auth.UserId, Auth.FullName, content, pool,
                pool == "project" ? (decimal?)hours : null, pool == "support" ? (int?)minutes : null, recipientsStr);
        }

        // Mail
        try
        {
            if (Mailer.HasSmtp && toEmails.Count > 0)
            {
                SupportPlan p = Repo.GetSupportPlan(cid);
                string sureLabel = pool == "project" ? hours.ToString("0.##", CultureInfo.GetCultureInfo("tr-TR")) + " saat" : minutes + " dakika";
                string balLabel = pool == "project" ? p.ProjectHours.ToString("0.##", CultureInfo.GetCultureInfo("tr-TR")) + " saat" : p.Minutes + " dk";
                string baseUrl = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
                if (baseUrl.Length == 0) baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                string catTitle = (type == "proje") ? "Proje Hizmet Bildirimi" : "Destek Hizmet Bildirimi";
                string html = Emails.ServiceMail(firmName, content, sureLabel, balLabel, baseUrl.TrimEnd('/') + "/Login.aspx", catTitle);
                string cc = (WebConfigurationManager.AppSettings["Support.NotifyEmail"] ?? "").Trim();
                string toJoined = string.Join(",", toEmails.ToArray());
                string err;
                if (cc.Length > 0) Mailer.SendToCc(toJoined, cc, "River · " + catTitle, html, out err);
                else Mailer.SendTo(toJoined, "River · " + catTitle, html, out err);
            }
        }
        catch { }

        Response.Redirect("ServiceForm.aspx?ok=sent");
    }

    private string BuildCompanyJson()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("[");
        List<CompanyLite> partners = Repo.GetCompaniesByType("Partner");
        List<CompanyLite> customers = Repo.GetCompaniesByType("Customer");
        bool first = true;
        Action<string, List<CompanyLite>> add = delegate (string grp, List<CompanyLite> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Name == "Harici Ürün Destekleri") continue;
                if (!first) sb.Append(","); first = false;
                string nm = (l[i].Name ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                sb.Append("{\"id\":" + l[i].CompanyId + ",\"name\":\"" + nm + "\",\"g\":\"" + grp + "\"}");
            }
        };
        add("Bayi", partners); add("Müşteri", customers);
        sb.Append("]");
        return sb.ToString();
    }

    private string BuildCompanyOptions()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<option value=\"\">Firma seçin...</option>");
        List<CompanyLite> partners = Repo.GetCompaniesByType("Partner");
        List<CompanyLite> customers = Repo.GetCompaniesByType("Customer");
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
}
