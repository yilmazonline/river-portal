using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;

// Siparis duzenle (vendor). Tutar degisirse hakedis ve firma ciro/kademe yeniden hesaplanir.
public partial class OrderEditPage : Page
{
    public string Culture = "tr";
    public int DealId = 0;
    public string Customer = "";
    public string Title = "";
    public string AmountStr = "";
    public string CommissionStr = "";
    public string RateStr = "";
    public string Status = "Pending";
    public string StatusOptionsHtml = "";
    public string CompanyName = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Orders.aspx"); return; }

        Culture = Auth.Culture;
        CompanyName = Auth.CompanyName;

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            Save();
            return;
        }

        int id;
        if (!int.TryParse(Request.QueryString["id"], out id)) { Response.Redirect("Orders.aspx"); return; }
        DealRow deal = Repo.GetDealById(id);
        if (deal == null) { Response.Redirect("Orders.aspx"); return; }

        DealId = deal.DealId;
        Customer = deal.CustomerName;
        Title = deal.Title;
        AmountStr = deal.Amount.ToString("0", CultureInfo.InvariantCulture);
        CommissionStr = deal.CommissionAmount.ToString("0", CultureInfo.InvariantCulture);
        RateStr = deal.EffectiveRate.ToString("0.##", CultureInfo.InvariantCulture);
        Status = deal.Status;
        StatusOptionsHtml = Fmt.StatusOptions(Status, Culture);
    }

    private void Save()
    {
        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Orders.aspx"); return; }
        DealRow deal = Repo.GetDealById(id);
        if (deal == null) { Response.Redirect("Orders.aspx"); return; }
        // Partner panelindeki yerinde duzenleme formundan geldiyse sonuc oraya doner
        bool toPartner = Request.Form["back"] == "partner";

        string customer = Request.Form["customer"];
        string title = Request.Form["title"];
        string status = Request.Form["status"];
        if (status != "Pending" && status != "Approved" && status != "Invoiced" && status != "Rejected")
            status = deal.Status;

        decimal amount;
        string amtRaw = Request.Form["amount"];
        if (amtRaw != null) amtRaw = amtRaw.Replace(".", "").Replace(",", ".");
        if (!decimal.TryParse(amtRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out amount) || amount < 0)
        { Response.Redirect(toPartner ? "Partner.aspx?err=amount#satislar" : "OrderEdit.aspx?id=" + id + "&err=amount"); return; }

        CompanyInfo company = Repo.GetCompany(deal.CompanyId);
        List<Tier> tiers = Repo.GetTiers();
        if (company == null || tiers.Count == 0) { Response.Redirect("Orders.aspx"); return; }

        // Hakediş manuel girilir (otomatik hesaplama yok)
        decimal commission;
        string commRaw = Request.Form["commission"];
        if (commRaw != null) commRaw = commRaw.Replace(".", "").Replace(",", ".");
        if (!decimal.TryParse(commRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out commission) || commission < 0)
        { Response.Redirect(toPartner ? "Partner.aspx?err=commission#satislar" : "OrderEdit.aspx?id=" + id + "&err=amount"); return; }

        TierEngine engine = new TierEngine(tiers);
        // Bu deal'den ONCEKI ciro = mevcut toplam - eski tutar
        decimal prior = Math.Max(0, company.AnnualNetRevenue - deal.Amount);
        int tierId = engine.ResolveTier(prior).TierId;

        // Hakediş yüzdesi manuel; boşsa tutardan türetilir
        decimal effRate;
        string rateRaw = Request.Form["rate"];
        if (rateRaw != null) rateRaw = rateRaw.Replace("%", "").Replace(",", ".").Trim();
        if (!decimal.TryParse(rateRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out effRate) || effRate < 0)
            effRate = amount > 0 ? Math.Round(commission / amount * 100m, 2) : 0m;

        DealCalc calc = new DealCalc(tierId, effRate, 0m, commission);
        decimal newRevenue = prior + amount;
        int newTierId = engine.ResolveTier(newRevenue).TierId;

        if (string.IsNullOrEmpty(title)) title = customer;
        Repo.UpdateDeal(id, deal.CompanyId, customer, title, amount, calc, status, newRevenue, newTierId);

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
                Repo.AddNotification(deal.CompanyId,
                    "Yeni partner seviyeniz",
                    "Your new partner tier",
                    "Tebrikler! Yeni seviyeniz " + newT.Name("tr") + ". Portal ayrıcalıklarınız güncellendi.",
                    "Congratulations! Your new tier is " + newT.Name("en") + ". Your portal benefits have been updated.",
                    "TierUp");
            }
        }

        Response.Redirect(toPartner ? "Partner.aspx?ok=dealedit#satislar" : "Orders.aspx?ok=edit");
    }
}
