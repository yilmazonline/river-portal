using System;
using System.Web.UI;

// Firma destek plani guncelle (yalniz portal yoneticisi): dakika + gecerlilik tarihi.
public partial class SupportPlanSavePage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Default.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Vendor.aspx"); return; }

        int cid; int.TryParse(Request.Form["cid"], out cid);
        int minutes; if (!int.TryParse(Request.Form["minutes"], out minutes)) minutes = 0;
        decimal phours = 0m;
        string ph = (Request.Form["phours"] ?? "").Trim().Replace(",", ".");
        decimal.TryParse(ph, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out phours);

        DateTime? until = null;
        DateTime d;
        string us = (Request.Form["until"] ?? "").Trim();
        if (us.Length > 0 && DateTime.TryParse(us, out d)) until = d.Date;

        if (cid > 0)
        {
            try { Repo.UpdateSupportPlan(cid, minutes, until, phours); }
            catch { }
        }
        Response.Redirect("Vendor.aspx?ok=support#sec-customers");
    }
}
