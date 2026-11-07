using System;
using System.Collections.Generic;
using System.Linq;

// KADEME & MEGA DEAL MOTORU (spec):
//   0-1.25M Registered 30 | 1.25-2.5M Silver 36 | 2.5-4M Gold 42 | 4M+ Platinum 50
//   Mega Deal (tek anlasma): 1.5M+ +3 | 2.5M+ +5 | 4M+ +7 | 6M+ +10
//   Kademe gecisi: bir deal'in taban orani, o deal'den ONCEKI kumulatif ciroya gore.
public class TierEngine
{
    private readonly List<Tier> _tiers;

    public TierEngine(IEnumerable<Tier> tiers)
    {
        _tiers = tiers.OrderBy(t => t.SortOrder).ToList();
    }

    public Tier ResolveTier(decimal annualRevenue)
    {
        Tier found = _tiers[0];
        for (int i = 0; i < _tiers.Count; i++)
        {
            Tier t = _tiers[i];
            bool aboveMin = annualRevenue >= t.MinRevenue;
            bool belowMax = (t.MaxRevenue == null) || (annualRevenue < t.MaxRevenue.Value);
            if (aboveMin && belowMax) found = t;
        }
        return found;
    }

    public Tier NextTier(Tier current)
    {
        return _tiers.FirstOrDefault(t => t.SortOrder > current.SortOrder);
    }

    // Kiralama hakedisi: Gold ve Platinum %30, Silver %25, Kayitli %20.
    public decimal RentalRate(Tier tier)
    {
        if (tier.Code == "Gold" || tier.Code == "Platinum") return 30m;
        if (tier.Code == "Silver") return 25m;
        return 20m;
    }

    // Versiyon yenileme hakedisi: Platinum %30, Gold %25, alti %20.
    public decimal RenewalRate(Tier tier)
    {
        if (tier.Code == "Platinum") return 30m;
        if (tier.Code == "Gold") return 25m;
        return 20m;
    }

    // Lisans yukseltme ve hazir modul: Platinum %30, Gold %25, Silver %25, Kayitli %20.
    public decimal LicenseUpgradeRate(Tier tier)
    {
        if (tier.Code == "Platinum") return 30m;
        if (tier.Code == "Gold" || tier.Code == "Silver") return 25m;
        return 20m;
    }

    public decimal MegaDealBonus(decimal dealAmount)
    {
        if (dealAmount >= 6000000m) return 10m;
        if (dealAmount >= 4000000m) return 7m;
        if (dealAmount >= 2500000m) return 5m;
        if (dealAmount >= 1500000m) return 3m;
        return 0m;
    }

    // Anlasma hakedisini hesaplar. priorRevenue = bu anlasmadan ONCEKI kumulatif ciro.
    public DealCalc CalculateDeal(decimal priorRevenue, decimal amount)
    {
        Tier t = ResolveTier(priorRevenue);
        decimal baseRate = t.BaseCommission;
        decimal mega = MegaDealBonus(amount);
        decimal commission = Math.Round(amount * (baseRate + mega) / 100m, 2);
        return new DealCalc(t.TierId, baseRate, mega, commission);
    }
}

public class DealCalc
{
    public int TierId;
    public decimal BaseRate;
    public decimal MegaRate;
    public decimal Commission;
    public DealCalc(int tierId, decimal baseRate, decimal megaRate, decimal commission)
    {
        TierId = tierId; BaseRate = baseRate; MegaRate = megaRate; Commission = commission;
    }
}
