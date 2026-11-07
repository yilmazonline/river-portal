using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Vendor: yeni bayi (partner firma) olusturma.
public partial class PartnerCreatePage : Page
{
    public string Culture = "tr";
    public string TierOptionsHtml = "";
    public string FlashErr = "";
    public bool IsCustomerType = false;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Partner.aspx"); return; }
        Culture = Auth.Culture;

        string ctype = (Request.QueryString["type"] == "customer" || Request.Form["ctype"] == "customer") ? "Customer" : "Partner";
        IsCustomerType = ctype == "Customer";

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            string name = (Request.Form["name"] ?? "").Trim();
            string tax = (Request.Form["tax"] ?? "").Trim();
            int tierId;
            int.TryParse(Request.Form["tier"], out tierId);

            if (name.Length == 0) { FlashErr = "Firma adı zorunludur."; BuildTierOptions(tierId); return; }
            if (!IsCustomerType && tierId <= 0)
            {
                List<Tier> ts = Repo.GetTiers();
                if (ts.Count > 0) tierId = ts[0].TierId;
            }
            int supportMin; int.TryParse(Request.Form["supportmin"], out supportMin); if (supportMin < 0) supportMin = 0;
            decimal projHours = 0m;
            string phraw = (Request.Form["projhours"] ?? "").Trim().Replace(",", ".");
            decimal.TryParse(phraw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out projHours);
            if (projHours < 0) projHours = 0m;
            int newCoId = Repo.CreateCompany(name, tax, tierId, "tr", ctype, supportMin);
            if (newCoId > 0 && projHours > 0) { try { Repo.UpdateSupportPlan(newCoId, supportMin, null, projHours); } catch { } }
            Response.Redirect(IsCustomerType ? "Vendor.aspx?ok=customer" : "Vendor.aspx?ok=partner");
            return;
        }

        BuildTierOptions(0);
    }

    private void BuildTierOptions(int selected)
    {
        List<Tier> tiers = Repo.GetTiers();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < tiers.Count; i++)
        {
            Tier t = tiers[i];
            bool sel = (selected == 0 && i == 0) || (t.TierId == selected);
            sb.Append("<option value=\"" + t.TierId + "\"" + (sel ? " selected=\"selected\"" : "") + ">" +
                      Fmt.Enc(t.Name(Culture)) + "</option>");
        }
        TierOptionsHtml = sb.ToString();
    }
}
