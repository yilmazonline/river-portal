using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;

// Partner adina anlasma kaydi. YALNIZCA vendor (impersonation) yapabilir.
public partial class BookDealPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Partner.aspx"); return; }  // partner giremez
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Partner.aspx"); return; }

        string customer = Request.Form["customer"];
        string title = Request.Form["title"];
        decimal amount;
        string amtRaw = Request.Form["amount"];
        if (amtRaw != null) amtRaw = amtRaw.Replace(".", "").Replace(",", ".");
        if (!decimal.TryParse(amtRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out amount) || amount <= 0)
        { Response.Redirect("Partner.aspx?err=amount"); return; }

        int companyId = Auth.CompanyId;
        CompanyInfo company = Repo.GetCompany(companyId);
        List<Tier> tiers = Repo.GetTiers();
        if (company == null || tiers.Count == 0) { Response.Redirect("Partner.aspx"); return; }

        // Hakediş manuel girilir (otomatik hesaplama yok)
        decimal commission;
        string commRaw = Request.Form["commission"];
        if (commRaw != null) commRaw = commRaw.Replace(".", "").Replace(",", ".");
        if (!decimal.TryParse(commRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out commission) || commission < 0)
        { Response.Redirect("Partner.aspx?err=commission"); return; }

        TierEngine engine = new TierEngine(tiers);
        decimal prior = company.AnnualNetRevenue;
        int tierId = engine.ResolveTier(prior).TierId;

        // Hakediş yüzdesi manuel girilir; boşsa tutardan türetilir
        decimal effRate;
        string rateRaw = Request.Form["rate"];
        if (rateRaw != null) rateRaw = rateRaw.Replace("%", "").Replace(",", ".").Trim();
        if (!decimal.TryParse(rateRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out effRate) || effRate < 0)
            effRate = amount > 0 ? Math.Round(commission / amount * 100m, 2) : 0m;

        DealCalc calc = new DealCalc(tierId, effRate, 0m, commission);
        decimal newRevenue = prior + amount;
        int newTierId = engine.ResolveTier(newRevenue).TierId;

        int? uid = null;
        var s = Session["UserId"];
        if (s != null) uid = (int)s;

        if (string.IsNullOrEmpty(title)) title = customer;
        Repo.BookDeal(companyId, title, customer, amount, calc, newRevenue, newTierId, uid);

        // Seviye yükseldiyse tebrik bildirimi
        if (newTierId != tierId)
        {
            Tier oldT = null, newT = null;
            for (int i = 0; i < tiers.Count; i++)
            {
                if (tiers[i].TierId == tierId) oldT = tiers[i];
                if (tiers[i].TierId == newTierId) newT = tiers[i];
            }
            if (oldT != null && newT != null && newT.SortOrder > oldT.SortOrder)
            {
                Repo.AddNotification(companyId,
                    "Yeni partner seviyeniz",
                    "Your new partner tier",
                    "Tebrikler! Yeni seviyeniz " + newT.Name("tr") + ". Portal ayrıcalıklarınız güncellendi.",
                    "Congratulations! Your new tier is " + newT.Name("en") + ". Your portal benefits have been updated.",
                    "TierUp");
            }
        }

        Response.Redirect("Partner.aspx?ok=deal");
    }
}
