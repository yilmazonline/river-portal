using System;
using System.Globalization;
using System.Text;
using System.Web;

// Bicimlendirme + guvenli HTML uretimi (C# 5).
public static class Fmt
{
    /// <summary>Destek talebi urun adi: eski "River" kayitlari "River PRO" gosterilir</summary>
    public static string Product(string p) { p = (p ?? "").Trim(); return p == "River" ? "River PRO" : p; }
    /// <summary>Urun ve modul: modul bos ya da "Genel" ise (yeni talepler) yalniz urun</summary>
    public static string ProdMod(string p, string m, string sep) { string a = Product(p); m = (m ?? "").Trim(); return m.Length == 0 || m == "Genel" ? a : a + sep + m; }
    public static bool HasModule(string m) { m = (m ?? "").Trim(); return m.Length > 0 && m != "Genel"; }
    // Sure kurali: 0 serbest; 0 disinda minimum 15; sonrasi 5'in katlari (0,15,20,25,...)
    public static int NormMinutes(int m)
    {
        if (m <= 0) return 0;
        if (m < 15) return 15;
        return ((m + 2) / 5) * 5;
    }

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    public static string N0(decimal v) { return v.ToString("N0", TR); }

    // Milyon gosterimi: 2.7M gibi (tr ondalik ',' -> gorsel tutarlilik icin '.' istiyoruz)
    public static string Millions(decimal v)
    {
        decimal m = v / 1000000m;
        // 0.## ama tr'de virgul cikar; gorsel olarak nokta istiyoruz -> replace
        string s = m.ToString("0.##", CultureInfo.InvariantCulture);
        return s;
    }

    public static string Enc(string s) { return HttpUtility.HtmlEncode(s == null ? "" : s); }

    public static string TierClass(string code)
    {
        if (code == "Silver") return "silver";
        if (code == "Gold") return "gold";
        if (code == "Platinum") return "plat";
        return "reg";
    }

    public static string ChipClass(string code)
    {
        if (code == "Silver") return "silver";
        if (code == "Gold") return "gold";
        if (code == "Platinum") return "platinum";
        return "registered";
    }

    public static string DealStClass(string status)
    {
        if (status == "Invoiced") return "invoiced";
        if (status == "Approved") return "approved";
        if (status == "Rejected") return "cancelled";
        return "pending";
    }

    public static string DealStLabel(string status, string culture)
    {
        bool en = culture == "en";
        if (status == "Invoiced") return en ? "Invoiced" : "Faturalandı";
        if (status == "Approved") return en ? "Approved" : "Onaylandı";
        if (status == "Pending") return en ? "Pending" : "Beklemede";
        if (status == "Rejected") return en ? "Cancelled" : "İptal edildi";
        return status;
    }

    // Sipariş durum <select> secenekleri (Rejected = "İptal edildi")
    public static string StatusOptions(string current, string culture)
    {
        string[] vals = new string[] { "Pending", "Approved", "Invoiced", "Rejected" };
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < vals.Length; i++)
        {
            string sel = vals[i] == current ? " selected=\"selected\"" : "";
            sb.Append("<option value=\"" + vals[i] + "\"" + sel + ">" + Enc(DealStLabel(vals[i], culture)) + "</option>");
        }
        return sb.ToString();
    }

    public static string DemoStClass(string status)
    {
        return status == "Pending" ? "pending" : "approved";
    }

    public static string DemoStLabel(string status, string culture)
    {
        bool en = culture == "en";
        if (status == "Pending") return en ? "Pending" : "Beklemede";
        return en ? "Approved" : "Onaylandı";
    }

    public static string Initials(string name)
    {
        if (string.IsNullOrEmpty(name)) return "?";
        string[] parts = name.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }

    // YouTube URL / embed iframe / kisa link icinden video ID cikarir
    /// <summary>Dogal siralama (Turkce): basliktaki sayilar sayi olarak karsilastirilir ("2. Giris" &lt; "10. Raporlar"),
    /// harfler Turkce alfabe kuraliyla ve buyuk/kucuk harf duyarsiz.</summary>
    public static int NaturalCompare(string a, string b)
    {
        a = (a ?? "").Trim(); b = (b ?? "").Trim();
        System.Globalization.CompareInfo ci = new System.Globalization.CultureInfo("tr-TR").CompareInfo;
        int i = 0, j = 0;
        while (i < a.Length && j < b.Length)
        {
            bool da = char.IsDigit(a[i]), db = char.IsDigit(b[j]);
            if (da && db)
            {
                int si = i, sj = j;
                while (i < a.Length && char.IsDigit(a[i])) i++;
                while (j < b.Length && char.IsDigit(b[j])) j++;
                string na = a.Substring(si, i - si).TrimStart('0'), nb = b.Substring(sj, j - sj).TrimStart('0');
                if (na.Length != nb.Length) return na.Length.CompareTo(nb.Length);   // uzun sayi buyuktur (tasmasiz)
                int c = string.CompareOrdinal(na, nb); if (c != 0) return c;
            }
            else
            {
                int si = i, sj = j;
                while (i < a.Length && !char.IsDigit(a[i])) i++;
                while (j < b.Length && !char.IsDigit(b[j])) j++;
                int c = ci.Compare(a.Substring(si, i - si), b.Substring(sj, j - sj), System.Globalization.CompareOptions.IgnoreCase);
                if (c != 0) return c;
            }
        }
        return (a.Length - i).CompareTo(b.Length - j);
    }

    public static string YouTubeId(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        string s = input.Trim();
        string[] markers = new string[] { "embed/", "watch?v=", "youtu.be/", "shorts/", "v=" };
        int start = -1;
        for (int i = 0; i < markers.Length; i++)
        {
            int p = s.IndexOf(markers[i], StringComparison.OrdinalIgnoreCase);
            if (p >= 0) { start = p + markers[i].Length; break; }
        }
        string rest = (start >= 0) ? s.Substring(start) : s;
        // Gecerli ID karakterlerini oku, ilk gecersiz karakterde dur
        StringBuilder sb = new StringBuilder();
        for (int k = 0; k < rest.Length; k++)
        {
            char c = rest[k];
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_') sb.Append(c);
            else break;
        }
        string id = sb.ToString();
        if (id.Length > 11) id = id.Substring(0, 11); // YouTube ID = 11 karakter
        return id;
    }

    // Yeni kullanici icin okunakli gecici parola
    public static string NewPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        Random rnd = new Random();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 10; i++) sb.Append(chars[rnd.Next(chars.Length)]);
        sb.Append('!');
        return sb.ToString();
    }
}
