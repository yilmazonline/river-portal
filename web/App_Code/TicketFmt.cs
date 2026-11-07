// Talep durum/oncelik etiketleri - paylasimli (App_Code). Sayfa sinifina baglilik olmadan her yerden erisilir.
public static class TicketFmt
{
    public static string StatusLabel(string s) { return StatusLabel(s, true); }
    /// <summary>Gosterilecek durum: personelin yazdigi gorunen durum varsa o, yoksa sistem durumu</summary>
    public static string Display(Ticket t, bool isSupport) { return t != null && !string.IsNullOrEmpty(t.StatusNote) ? t.StatusNote : StatusLabel(t != null ? t.Status : "", isSupport); }
    /// <summary>Hazir gorunen durumlar (personel listeden secer ya da ozel yazar)</summary>
    public static readonly string[] Presets = { "Analiz ediliyor", "Geliştirme aşamasında", "Güncelleme planlandı", "Test ediliyor", "Çözüm hazırlanıyor", "Üçüncü taraf bekleniyor" };

    public static string StatusLabel(string s, bool isSupport)
    {
        if (s == "InProgress") return "İnceleniyor";
        if (s == "WaitingCustomer") return isSupport ? "Müşteride Bekliyor" : "Sizden Yanıt Bekleniyor";
        if (s == "Resolved") return "Çözüldü";
        if (s == "Closed") return "Kapandı";
        return "Yeni";
    }

    public static string StatusClass(string s)
    {
        if (s == "InProgress") return "approved";
        if (s == "WaitingCustomer") return "pending";
        if (s == "Resolved") return "invoiced";
        if (s == "Closed") return "cancelled";
        return "pending";
    }

    public static string PrioLabel(string p)
    {
        if (p == "Low") return "Düşük";
        if (p == "High") return "Yüksek";
        if (p == "Critical") return "Kritik";
        return "Normal";
    }
}
