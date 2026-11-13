using System;
using System.Collections.Generic;
using System.Web.UI;

// Siparis sil. YALNIZCA vendor. Silince firma cirosu/kademesi yeniden hesaplanir.
public partial class DeleteOrderPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Orders.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Orders.aspx"); return; }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Orders.aspx"); return; }

        DealRow deal = Repo.GetDealById(id);
        if (deal == null) { Response.Redirect("Orders.aspx"); return; }

        CompanyInfo company = Repo.GetCompany(deal.CompanyId);
        List<Tier> tiers = Repo.GetTiers();
        decimal newRevenue = company != null ? Math.Max(0, company.AnnualNetRevenue - deal.Amount) : 0;
        int newTierId = deal.CompanyId;
        if (tiers.Count > 0)
        {
            TierEngine engine = new TierEngine(tiers);
            newTierId = engine.ResolveTier(newRevenue).TierId;
        }

        Repo.DeleteDeal(id, deal.CompanyId, newRevenue, newTierId);
        // Partner panelindeki defterden silindiyse oraya don
        Response.Redirect(Request.Form["back"] == "partner" ? "Partner.aspx?ok=dealdel#satislar" : "Orders.aspx?ok=del");
    }
}
