using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;

// Partner Seviyeleri sayfasi: tum kademeler, getiri (avantaj) ve gotursu (sart/yukumluluk).
public partial class TiersPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string TiersHtml = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (Auth.IsPartnerUserLimited) { Response.Redirect(Auth.HomeUrl); return; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;

        List<Tier> tiers = Repo.GetTiers();
        CompanyInfo company = Repo.GetCompany(Auth.CompanyId);
        int currentTierId = 0;
        if (company != null)
        {
            if (company.CurrentTierId != null) currentTierId = company.CurrentTierId.Value;
            else if (tiers.Count > 0)
            {
                TierEngine eng0 = new TierEngine(tiers);
                currentTierId = eng0.ResolveTier(company.AnnualNetRevenue).TierId;
            }
        }

        TierEngine engine = new TierEngine(tiers);
        TiersHtml = BuildTiers(tiers, engine, currentTierId);
    }

    private int InactiveMonths(string code)
    {
        if (code == "Platinum") return 18;
        if (code == "Gold") return 15;
        return 12;
    }

    private string[] Perks(string code)
    {
        bool en = Culture == "en";
        switch (code)
        {
            case "Registered": return en ? new[] { "Self-service support", "Basic training access" }
                                          : new[] { "Self-servis destek", "Temel eğitim erişimi" };
            case "Silver": return en ? new[] { "Standard SLA", "Email support", "Product catalog" }
                                     : new[] { "Standart SLA", "E-posta destek", "Ürün kataloğu" };
            case "Gold": return en ? new[] { "Fast SLA", "Priority demos", "Marketing materials" }
                                   : new[] { "Hızlı SLA", "Öncelikli demo", "Pazarlama materyalleri" };
            case "Platinum": return en ? new[] { "Strategic SLA", "Dedicated account manager", "Market development fund", "Beta access" }
                                       : new[] { "Stratejik SLA", "Özel hesap yöneticisi", "Ortak pazarlama fonu", "Beta erişim" };
        }
        return new string[0];
    }

    // Kademeye ulasma / koruma sarti (gotursu)
    private string Requirement(Tier t)
    {
        bool en = Culture == "en";
        if (t.MinRevenue <= 0)
            return en ? "No revenue requirement · registration is enough."
                      : "Ciro şartı yok · kayıt yeterli.";
        string min = Fmt.Millions(t.MinRevenue) + "M ₺";
        return en ? ("Annual cumulative revenue of " + min + " and above.")
                  : ("Yıllık kümülatif ciro " + min + " ve üzeri.");
    }

    private string BuildTiers(List<Tier> tiers, TierEngine engine, int currentTierId)
    {
        bool en = Culture == "en";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < tiers.Count; i++)
        {
            Tier t = tiers[i];
            string cls = Fmt.TierClass(t.Code);
            bool isCurrent = t.TierId == currentTierId;

            sb.Append("<div class=\"tier-detail rung " + cls + (isCurrent ? " current" : "") + "\">");
            if (isCurrent) sb.Append("<div class=\"tag\">" + (en ? "Your tier" : "Mevcut seviyeniz") + "</div>");

            sb.Append("<div class=\"lvl\">" + (en ? "Level " : "Seviye ") + t.SortOrder + "</div>");
            sb.Append("<div class=\"nm\"><span class=\"dotc\"></span><span>" + Fmt.Enc(t.Name(Culture)) + "</span></div>");

            // Oran satiri: taban / kiralama / yenileme / lisans-modul
            decimal rental = engine.RentalRate(t);
            decimal renewal = engine.RenewalRate(t);
            decimal license = engine.LicenseUpgradeRate(t);
            sb.Append("<div class=\"rates3\">");
            sb.Append(RateBox(en ? "License Sales" : "Lisans Satışı", t.BaseCommission));
            sb.Append(RateBox(en ? "Rental" : "Kiralama", rental));
            sb.Append(RateBox(en ? "Renewal" : "Yenileme", renewal));
            sb.Append(RateBox(en ? "License/Module Upgrade" : "Lisans/Modül Yükseltme", license));
            sb.Append("</div>");

            // Getiri (avantajlar)
            sb.Append("<div class=\"td-h\">" + (en ? "Benefits" : "Getirileri") + "</div>");
            sb.Append("<ul>");
            string[] perks = Perks(t.Code);
            for (int p = 0; p < perks.Length; p++) sb.Append("<li>" + Fmt.Enc(perks[p]) + "</li>");
            sb.Append("<li>" + (en ? "SLA: " : "SLA: ") + Fmt.Enc(t.Sla(Culture)) + "</li>");
            sb.Append("</ul>");

            // Sartlar (yukumluluk)
            sb.Append("<div class=\"td-h\">" + (en ? "Requirements" : "Şartlar") + "</div>");
            sb.Append("<ul class=\"muted-list\">");
            sb.Append("<li>" + Fmt.Enc(Requirement(t)) + "</li>");
            int im = InactiveMonths(t.Code);
            sb.Append("<li>" + (en ? im + " months of inactivity moves the account to passive status."
                                   : im + " ay hareketsizlikte hesap pasif duruma düşer.") + "</li>");
            sb.Append("</ul>");

            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private string RateBox(string label, decimal val)
    {
        return "<div class=\"rate-box\"><div class=\"rb-k\">" + Fmt.Enc(label) + "</div>" +
               "<div class=\"rb-v\">" + val.ToString("0", CultureInfo.InvariantCulture) + "%</div></div>";
    }
}
