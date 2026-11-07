using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;

public partial class VendorPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public int TotalPartners = 0;
    public int ActivePartners = 0;
    public string TotalRevenueM = "0";
    public int PendingDemos = 0;
    public string MatrixHtml = "";
    public string QueueHtml = "";
    public string RegQueueHtml = "";
    public string AllDemosHtml = "";
    public string CustomersHtml = "";
    public string PartnersHtml = "";
    public string PartnerTiles = "";
    public string PartnerDetails = "";
    public string CustomerTiles = "";
    public string CustomerDetails = "";
    public string ChartsJson = "{}";
    public string FlashMsg = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.RequireVendor(this);
        if (Response.IsRequestBeingRedirected) return;

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;

        bool en = Culture == "en";
        string ok = Request.QueryString["ok"];
        if (ok == "partner") FlashMsg = en ? "Partner created." : "Bayi oluşturuldu.";
        else if (ok == "customer") FlashMsg = en ? "Customer created." : "Müşteri oluşturuldu.";
        else if (ok == "support") FlashMsg = "Destek planı güncellendi.";
        else if (ok == "user") FlashMsg = en ? "User created and notified by email." : "Kullanıcı oluşturuldu ve e-posta ile bilgilendirildi.";
        else if (ok == "demoapp") FlashMsg = en ? "Demo approved and partner notified." : "Demo onaylandı ve partner bilgilendirildi.";
        else if (ok == "regapp") FlashMsg = en ? "Customer registration approved." : "Müşteri kaydı onaylandı.";
        else if (ok == "regrej") FlashMsg = en ? "Customer registration rejected." : "Müşteri kaydı reddedildi.";
        else if (ok == "renapp") FlashMsg = en ? "Renewal approved." : "Kayıt yenileme onaylandı.";
        else if (ok == "renrej") FlashMsg = en ? "Renewal rejected." : "Kayıt yenileme reddedildi.";

        List<PartnerRow> partners = Repo.GetPartners(Culture);
        List<DemoRow> queue = Repo.GetPendingDemos();

        TotalPartners = partners.Count;
        ActivePartners = partners.Count(p => !p.IsPassive);
        decimal totalRev = 0m;
        for (int i = 0; i < partners.Count; i++) totalRev += partners[i].Revenue;
        TotalRevenueM = Fmt.Millions(totalRev);
        PendingDemos = queue.Count;

        MatrixHtml = BuildMatrix(partners);
        QueueHtml = BuildQueue(queue);
        RegQueueHtml = BuildRegQueue();
        AllDemosHtml = BuildAllDemos();
        List<UserRow> allUsers = Repo.GetAllUsers();
        System.Collections.Generic.Dictionary<int, SupportPlan> plans = Repo.GetSupportPlanMap();
        BuildPartnersGlass(partners, allUsers, plans, out PartnerTiles, out PartnerDetails);
        BuildCustomers(allUsers, plans, out CustomerTiles, out CustomerDetails);

        try { ChartsJson = Repo.GetVendorChartsJson(); }
        catch { ChartsJson = "{}"; }
    }

    private string DemoStLabel(string s)
    {
        if (s == "Approved") return "Onaylandı";
        if (s == "Rescheduled") return "Yeniden Planlandı";
        if (s == "Completed") return "Tamamlandı";
        if (s == "Rejected") return "Reddedildi";
        return "Beklemede";
    }
    private string DemoStClass(string s)
    {
        if (s == "Approved" || s == "Completed") return "invoiced";
        if (s == "Rejected") return "cancelled";
        if (s == "Rescheduled") return "approved";
        return "pending";
    }

    private string RoleLabelTr(string role, string perms)
    {
        if (role == "CustomerAdmin") return "Müşteri Yöneticisi";
        if (role == "CustomerUser") return "Müşteri Kullanıcısı";
        if (role == "PartnerAdmin") return "Bayi Yöneticisi";
        if (role == "PartnerUser") return "Bayi Kullanıcısı";
        return string.IsNullOrEmpty(role) ? "Kullanıcı" : role;
    }

    private void BuildCustomers(List<UserRow> users, System.Collections.Generic.Dictionary<int, SupportPlan> plans, out string tiles, out string details)
    {
        List<CompanyLite> customers = Repo.GetCompaniesByType("Customer");
        StringBuilder t = new StringBuilder(), d = new StringBuilder();
        int shown = 0;
        for (int i = 0; i < customers.Count; i++)
        {
            CompanyLite c = customers[i];
            if (c.Name == "Harici Ürün Destekleri") continue;
            List<UserRow> cu = new List<UserRow>();
            for (int j = 0; j < users.Count; j++) if (users[j].CompanyId == c.CompanyId) cu.Add(users[j]);
            string did = "det-cust-" + c.CompanyId;
            t.Append("<div class=\"fc-tile\" data-det=\"" + did + "\" data-name=\"" + Fmt.Enc(c.Name) + "\">");
            t.Append("<div class=\"fc-nm\">" + Fmt.Enc(c.Name) + "</div>");
            t.Append("<div class=\"fc-meta\"><span class=\"fc-usr\">" + cu.Count + " kullanıcı</span></div></div>");

            d.Append("<div class=\"fc-detail\" id=\"" + did + "\" style=\"display:none\">");
            d.Append("<div class=\"fc-det-top\"><span class=\"fc-det-nm\">" + Fmt.Enc(c.Name) + "</span>");
            d.Append("<span class=\"fc-det-act\"><a class=\"btn-ghost\" href=\"FirmAnalytics.aspx?cid=" + c.CompanyId + "\">Analiz</a> <button type=\"button\" class=\"fc-close\" title=\"Kapat\">&times;</button></span></div>");
            d.Append(PlanFormHtml(c.CompanyId, plans));
            d.Append(UserRowsHtml(cu));
            d.Append("</div>");
            shown++;
        }
        tiles = shown == 0 ? "<div class=\"ig-empty\">Henüz müşteri firma yok.</div>" : t.ToString();
        details = d.ToString();
    }

    private void BuildPartnersGlass(List<PartnerRow> partners, List<UserRow> users, System.Collections.Generic.Dictionary<int, SupportPlan> plans, out string tiles, out string details)
    {
        StringBuilder t = new StringBuilder(), d = new StringBuilder();
        for (int i = 0; i < partners.Count; i++)
        {
            PartnerRow p = partners[i];
            List<UserRow> pu = new List<UserRow>();
            for (int j = 0; j < users.Count; j++) if (users[j].CompanyId == p.CompanyId) pu.Add(users[j]);
            string did = "det-ptr-" + p.CompanyId;
            string chip = "<span class=\"tier-chip " + Fmt.ChipClass(p.TierCode) + "\">" + Fmt.Enc(p.TierName) + "</span>";
            t.Append("<div class=\"fc-tile\" data-det=\"" + did + "\" data-name=\"" + Fmt.Enc(p.Name) + "\">");
            t.Append("<div class=\"fc-nm\">" + Fmt.Enc(p.Name) + "</div>" + chip);
            t.Append("<div class=\"fc-meta\"><span class=\"fc-dot " + (p.IsPassive ? "off" : "") + "\"></span>" + (p.IsPassive ? "Pasif" : "Aktif") + " · <span class=\"fc-rev\">" + Fmt.Millions(p.Revenue) + "M ₺</span><span class=\"fc-usr\">" + pu.Count + " kullanıcı</span></div></div>");

            d.Append("<div class=\"fc-detail\" id=\"" + did + "\" style=\"display:none\">");
            d.Append("<div class=\"fc-det-top\"><span class=\"fc-det-nm\">" + Fmt.Enc(p.Name) + " " + chip + "</span>");
            d.Append("<span class=\"fc-det-act\"><button class=\"btn-ghost\" data-imp=\"" + p.CompanyId + "\">Adına Görüntüle</button> <a class=\"btn-ghost\" href=\"FirmAnalytics.aspx?cid=" + p.CompanyId + "\">Analiz</a> <button type=\"button\" class=\"fc-close\" title=\"Kapat\">&times;</button></span></div>");
            d.Append(PlanFormHtml(p.CompanyId, plans));
            d.Append(UserRowsHtml(pu));
            d.Append("</div>");
        }
        tiles = partners.Count == 0 ? "<div class=\"ig-empty\">Henüz iş ortağı yok.</div>" : t.ToString();
        details = d.ToString();
    }

    private string PlanFormHtml(int companyId, System.Collections.Generic.Dictionary<int, SupportPlan> plans)
    {
        SupportPlan sp = null; if (plans != null && plans.ContainsKey(companyId)) sp = plans[companyId];
        int mins = sp != null ? sp.Minutes : 0;
        string untilVal = (sp != null && sp.Until.HasValue) ? sp.Until.Value.ToString("yyyy-MM-dd") : "";
        string untilLbl = (sp != null && sp.Until.HasValue) ? sp.Until.Value.ToString("dd.MM.yyyy") : "süresiz";
        string phVal = sp != null ? sp.ProjectHours.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) : "0";
        string phLbl = sp != null ? sp.ProjectHours.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) : "0";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("<form method=\"post\" action=\"SupportPlanSave.aspx\" class=\"ig-plan\">");
        sb.Append("<input type=\"hidden\" name=\"cid\" value=\"" + companyId + "\" />");
        sb.Append("<div class=\"ig-plan-cur\">Kalan destek: <b>" + mins + " dk</b> &middot; Geçerlilik: <b>" + untilLbl + "</b> &middot; Proje: <b>" + phLbl + " saat</b></div>");
        sb.Append("<div class=\"ig-plan-row\">");
        sb.Append("<label>Süre (dk)<input class=\"input\" type=\"number\" name=\"minutes\" step=\"15\" value=\"" + mins + "\" /></label>");
        sb.Append("<label>Geçerlilik Tarihi<input class=\"input\" type=\"date\" name=\"until\" value=\"" + untilVal + "\" /></label>");
        sb.Append("<label>Proje Süresi (saat)<input class=\"input\" type=\"number\" name=\"phours\" step=\"0.25\" value=\"" + phVal + "\" /></label>");
        sb.Append("<button class=\"btn-cyan\" type=\"submit\">Kaydet</button>");
        sb.Append("</div></form>");
        return sb.ToString();
    }

    private string UserRowsHtml(List<UserRow> list)
    {
        if (list.Count == 0) return "<div class=\"ig-user\"><span class=\"nm\" style=\"color:var(--muted);font-weight:400\">Tanımlı kullanıcı yok.</span></div>";
        StringBuilder sb = new StringBuilder();
        for (int j = 0; j < list.Count; j++)
        {
            UserRow u = list[j];
            sb.Append("<div class=\"ig-user\"><span class=\"nm\">" + Fmt.Enc(u.FullName) + "</span>" +
                "<span class=\"em\">" + Fmt.Enc(u.Email) + "</span>" +
                "<span class=\"rl\">" + Fmt.Enc(RoleLabelTr(u.Role, u.Permissions)) + "</span></div>");
        }
        return sb.ToString();
    }

    private string BuildAllDemos()
    {
        System.Collections.Generic.List<DemoRow> demos = Repo.GetAllDemosAllCompanies();
        if (demos.Count == 0)
            return "<tr><td colspan=\"5\" style=\"color:var(--muted)\">Henüz demo talebi yok.</td></tr>";

        System.Globalization.CultureInfo tr = new System.Globalization.CultureInfo("tr-TR");
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < demos.Count; i++)
        {
            DemoRow d = demos[i];
            string dates = d.PreferredDate != null ? d.PreferredDate.Value.ToString("dd.MM.yyyy HH:mm", tr) : "-";
            if (d.AltDate1 != null) dates += " / " + d.AltDate1.Value.ToString("dd.MM.yyyy HH:mm", tr);
            if (d.AltDate2 != null) dates += " / " + d.AltDate2.Value.ToString("dd.MM.yyyy HH:mm", tr);
            string sched = d.ScheduledDate != null ? d.ScheduledDate.Value.ToString("dd.MM.yyyy HH:mm", tr) : "-";

            sb.Append("<tr>");
            sb.Append("<td>" + Fmt.Enc(d.CompanyName) + "</td>");
            sb.Append("<td>" + Fmt.Enc(d.ClientName) + "</td>");
            sb.Append("<td>" + Fmt.Enc(d.ProductModule) + "</td>");
            sb.Append("<td style=\"font-size:12px\">" + dates + "<br/><span style=\"color:var(--muted)\">Planlanan: " + sched + "</span></td>");
            sb.Append("<td><span class=\"st " + DemoStClass(d.Status) + "\">" + DemoStLabel(d.Status) + "</span></td>");
            sb.Append("</tr>");
        }
        return sb.ToString();
    }

    private string BuildRegQueue()
    {
        bool en = Culture == "en";
        List<CustomerReg> pending = Repo.GetVendorRegQueue(false);
        List<CustomerReg> renewals = Repo.GetVendorRegQueue(true);

        if (pending.Count == 0 && renewals.Count == 0)
            return "<div class=\"reg-queue-item\"><div class=\"mod\">" +
                   (en ? "No pending registrations." : "Bekleyen kayıt yok.") + "</div></div>";

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < pending.Count; i++)
        {
            CustomerReg c = pending[i];
            sb.Append("<div class=\"reg-queue-item\">");
            sb.Append("<div class=\"row\"><div><div class=\"cn\">" + Fmt.Enc(c.CustomerName) +
                      " <span style=\"color:var(--muted);font-weight:400\">· " + Fmt.Enc(c.CompanyName) + "</span></div>");
            sb.Append("<div class=\"mod\">" + Fmt.Enc(string.IsNullOrEmpty(c.ServiceType) ? "-" : c.ServiceType) +
                      " · " + (en ? "temp " : "sıcaklık ") + c.SalesTemperature + "/100</div></div>");
            sb.Append("<span class=\"st pending\">" + (en ? "New" : "Yeni") + "</span></div>");
            sb.Append(DecisionForms(c.RegistrationId, "approveReg", "rejectReg", en));
            sb.Append("</div>");
        }

        for (int i = 0; i < renewals.Count; i++)
        {
            CustomerReg c = renewals[i];
            sb.Append("<div class=\"reg-queue-item\">");
            sb.Append("<div class=\"row\"><div><div class=\"cn\">" + Fmt.Enc(c.CustomerName) +
                      " <span style=\"color:var(--muted);font-weight:400\">· " + Fmt.Enc(c.CompanyName) + "</span></div>");
            sb.Append("<div class=\"mod\">" + (en ? "Renewal request" : "Yenileme talebi") + "</div></div>");
            sb.Append("<span class=\"st approved\">" + (en ? "Renewal" : "Yenileme") + "</span></div>");
            sb.Append(DecisionForms(c.RegistrationId, "approveRenew", "rejectRenew", en));
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private string DecisionForms(int id, string approveAction, string rejectAction, bool en)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div style=\"display:flex;gap:8px;margin-top:10px\">");
        sb.Append("<form method=\"post\" action=\"CustomerRegDecision.aspx\" style=\"flex:1;margin:0\">");
        sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + id + "\" />");
        sb.Append("<input type=\"hidden\" name=\"action\" value=\"" + approveAction + "\" />");
        sb.Append("<button type=\"submit\" class=\"btn-cyan\" style=\"width:100%\">" + (en ? "Approve" : "Onayla") + "</button>");
        sb.Append("</form>");
        sb.Append("<form method=\"post\" action=\"CustomerRegDecision.aspx\" style=\"flex:1;margin:0\">");
        sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + id + "\" />");
        sb.Append("<input type=\"hidden\" name=\"action\" value=\"" + rejectAction + "\" />");
        sb.Append("<button type=\"submit\" class=\"btn-ghost\" style=\"width:100%\">" + (en ? "Reject" : "Reddet") + "</button>");
        sb.Append("</form>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private string BuildMatrix(List<PartnerRow> partners)
    {
        bool en = Culture == "en";
        if (partners.Count == 0)
            return "<tr><td colspan=\"5\" style=\"color:var(--muted)\">" +
                   (en ? "No partners." : "Partner yok.") + "</td></tr>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < partners.Count; i++)
        {
            PartnerRow p = partners[i];
            sb.Append("<tr>");
            sb.Append("<td>" + Fmt.Enc(p.Name) + "</td>");
            sb.Append("<td><span class=\"tier-chip " + Fmt.ChipClass(p.TierCode) + "\">" + Fmt.Enc(p.TierName) + "</span></td>");
            sb.Append("<td class=\"num\">" + Fmt.Millions(p.Revenue) + "M ₺</td>");
            if (p.IsPassive)
                sb.Append("<td><span class=\"st passive\">" + (en ? "Passive" : "Pasif") + "</span></td>");
            else
                sb.Append("<td><span class=\"st invoiced\">" + (en ? "Active" : "Aktif") + "</span></td>");
            sb.Append("<td style=\"text-align:right\"><button class=\"btn-ghost\" data-imp=\"" + p.CompanyId + "\">" +
                      (en ? "View As" : "Adına Görüntüle") + "</button></td>");
            sb.Append("</tr>");
        }
        return sb.ToString();
    }

    private string BuildQueue(List<DemoRow> queue)
    {
        bool en = Culture == "en";
        if (queue.Count == 0)
            return "<div class=\"demo-item\"><div class=\"mod\">" +
                   (en ? "No pending demo requests." : "Bekleyen demo talebi yok.") + "</div></div>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < queue.Count; i++)
        {
            DemoRow d = queue[i];
            sb.Append("<div class=\"demo-item\"><div class=\"row\">");
            sb.Append("<div><div class=\"cn\">" + Fmt.Enc(d.ClientName) + "</div><div class=\"mod\">" +
                      Fmt.Enc(d.ProductModule) + "</div></div>");
            sb.Append("<span class=\"st pending\">" + (en ? "Pending" : "Beklemede") + "</span>");
            sb.Append("</div>");
            sb.Append("<div style=\"display:flex;gap:8px;margin-top:10px\">");
            sb.Append("<form method=\"post\" action=\"ApproveDemo.aspx\" style=\"flex:1;margin:0\">");
            sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + d.DemoRequestId + "\" />");
            sb.Append("<button type=\"submit\" class=\"btn-cyan\" style=\"width:100%\">" + (en ? "Approve" : "Onayla") + "</button>");
            sb.Append("</form>");
            sb.Append("<button class=\"btn-ghost\" style=\"flex:1\">" + (en ? "Reschedule" : "Yeniden Planla") + "</button>");
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }
}
