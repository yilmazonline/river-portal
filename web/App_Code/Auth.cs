using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;

// Oturum tabanli kimlik. Login basarili olunca Set(...) cagrilir.
// Korumali sayfalar Page_Load'da Auth.Require(this) veya Auth.RequireVendor(this) cagirir.
public static class Auth
{
    public static void Set(UserAccount u, string companyName, string culture)
    {
        var s = HttpContext.Current.Session;
        s["UserId"] = u.UserId;
        s["CompanyId"] = u.CompanyId;
        s["FullName"] = u.FullName;
        s["Role"] = u.Role;
        s["Permissions"] = u.Permissions ?? "";
        s["CompanyName"] = companyName;
        s["Culture"] = culture;
    }

    // ---------- kalici oturum (sunucu oturumu dussse de kullanici cikisa dusmez) ----------
    // Cerez: kullaniciId.bitis.damga.imza  (HMAC-SHA256, anahtar web.config Auth.CookieKey). Damga sifre ozetinden turetilir:
    // sifre degisince eski cerezler gecersizdir. Kullanici pasiflestirilirse geri yukleme yapilmaz. Sure 30 gun, kullanildikca yenilenir.
    const string PCK = "rv_auth";
    const int PDAYS = 30;
    static byte[] PKey
    {
        get { string k = (WebConfigurationManager.AppSettings["Auth.CookieKey"] ?? "").Trim(); return k.Length >= 32 ? Encoding.UTF8.GetBytes(k) : null; }
    }
    static string B64(byte[] b) { return Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_'); }
    static string Stamp(UserAccount u)
    {
        using (SHA256 h = SHA256.Create()) return B64(h.ComputeHash(Encoding.UTF8.GetBytes(u.UserId + "|" + (u.PasswordHash ?? "")))).Substring(0, 22);
    }
    static string Sig(string payload, byte[] key) { using (HMACSHA256 h = new HMACSHA256(key)) return B64(h.ComputeHash(Encoding.UTF8.GetBytes(payload))); }
    static bool SameStr(string a, string b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        int d = 0; for (int i = 0; i < a.Length; i++) d |= a[i] ^ b[i]; return d == 0;   // sabit sureli karsilastirma
    }
    static bool IsHttps(HttpRequest q)
    {
        if (q.IsSecureConnection) return true;
        string p = q.Headers["X-Forwarded-Proto"] ?? "", v = q.Headers["CF-Visitor"] ?? "";   // Cloudflare arkasinda
        return p.Equals("https", StringComparison.OrdinalIgnoreCase) || v.IndexOf("https", StringComparison.OrdinalIgnoreCase) >= 0;
    }
    static void PutCookie(HttpContext ctx, string value, DateTime expires)
    {
        HttpCookie c = new HttpCookie(PCK, value); c.Path = "/"; c.HttpOnly = true; c.Expires = expires; c.Secure = IsHttps(ctx.Request);
        try { var p = typeof(HttpCookie).GetProperty("SameSite"); if (p != null) p.SetValue(c, Enum.Parse(p.PropertyType, "Lax"), null); } catch { }
        ctx.Response.Cookies.Remove(PCK); ctx.Response.Cookies.Add(c);
    }
    /// <summary>Giristen sonra (ve kullanici kendi sifresini degistirince) cagrilir</summary>
    public static void IssuePersistent(UserAccount u)
    {
        HttpContext ctx = HttpContext.Current; byte[] key = PKey; if (ctx == null || key == null || u == null) return;
        DateTime exp = DateTime.UtcNow.AddDays(PDAYS);
        string payload = u.UserId + "." + (long)(exp - new DateTime(1970, 1, 1)).TotalSeconds + "." + Stamp(u);
        PutCookie(ctx, payload + "." + Sig(payload, key), exp);
    }
    public static void ClearPersistent()
    {
        HttpContext ctx = HttpContext.Current; if (ctx == null) return;
        PutCookie(ctx, "", DateTime.UtcNow.AddDays(-1));
    }
    /// <summary>Her istekte (Global.asax): sunucu oturumu bossa ve gecerli cerez varsa kullaniciyi sessizce geri yukler</summary>
    public static void TryRestore(HttpContext ctx)
    {
        try
        {
            if (ctx == null || ctx.Session == null || ctx.Session.IsReadOnly || ctx.Session["UserId"] != null) return;
            HttpCookie ck = ctx.Request.Cookies[PCK]; byte[] key = PKey;
            if (ck == null || string.IsNullOrEmpty(ck.Value) || key == null) return;
            string[] p = ck.Value.Split('.');
            if (p.Length != 4) { ClearPersistent(); return; }
            string payload = p[0] + "." + p[1] + "." + p[2];
            int uid; long exp;
            if (!SameStr(Sig(payload, key), p[3]) || !int.TryParse(p[0], out uid) || !long.TryParse(p[1], out exp)) { ClearPersistent(); return; }
            DateTime expires = new DateTime(1970, 1, 1).AddSeconds(exp);
            if (expires < DateTime.UtcNow) { ClearPersistent(); return; }
            UserAccount u = Repo.GetAccountById(uid);
            if (u == null || !u.IsActive || !SameStr(Stamp(u), p[2])) { ClearPersistent(); return; }   // pasif kullanici ya da sifre degismis
            HttpCookie cc = ctx.Request.Cookies["ui_culture"]; string cul = cc != null ? cc.Value : null;
            if (cul != "tr" && cul != "en") cul = u.PreferredCulture == "en" ? "en" : "tr";
            CompanyInfo co = Repo.GetCompany(u.CompanyId);
            Set(u, co != null ? co.Name : "", cul);
            if ((expires - DateTime.UtcNow).TotalDays < PDAYS / 2) IssuePersistent(u);   // kullanildikca yenile
        }
        catch { }   // geri yukleme hatasi oturumu bozmaz: kullanici giris sayfasina yonlenir
    }

    public static bool IsLoggedIn
    {
        get { return HttpContext.Current.Session["UserId"] != null; }
    }

    public static int CompanyId
    {
        get
        {
            var s = HttpContext.Current.Session;
            // Vendor impersonation: imp_company cookie'si varsa hedef firma
            if (IsVendor)
            {
                HttpCookie c = HttpContext.Current.Request.Cookies["imp_company"];
                int target;
                if (c != null && int.TryParse(c.Value, out target)) return target;
            }
            return s["CompanyId"] != null ? (int)s["CompanyId"] : 0;
        }
    }

    public static int HomeCompanyId
    {
        get
        {
            var s = HttpContext.Current.Session;
            return s["CompanyId"] != null ? (int)s["CompanyId"] : 0;
        }
    }

    public static string Role
    {
        get
        {
            var s = HttpContext.Current.Session;
            return s["Role"] != null ? (string)s["Role"] : "";
        }
    }

    public static int UserId
    {
        get { var s = HttpContext.Current.Session; return s["UserId"] != null ? (int)s["UserId"] : 0; }
    }

    public static string Permissions
    {
        get { var s = HttpContext.Current.Session; return s["Permissions"] != null ? (string)s["Permissions"] : ""; }
    }

    // Coklu izin kontrolu: Permissions setinde anahtar var mi? (Portal / Editor / Support)
    public static bool HasPerm(string key)
    {
        string p = Permissions;
        if (string.IsNullOrEmpty(p)) return false;
        string[] parts = p.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++) if (string.Equals(parts[i], key, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    // Portal yonetimi (bayi/kullanici/anlasma/demo/materyal): legacy rol veya "Portal" izni
    public static bool IsVendor { get { return Role == "VendorAdmin" || Role == "SalesRep" || HasPerm("Portal"); } }
    public static bool IsVendorAdmin { get { return Role == "VendorAdmin" || HasPerm("Portal"); } }
    public static bool IsSalesRep { get { return Role == "SalesRep"; } }
    // Egitim duzenleme: legacy rol veya "Editor" izni
    public static bool CanEditTraining { get { return Role == "VendorAdmin" || Role == "TrainingEditor" || HasPerm("Editor"); } }
    // Destek kuyrugu: legacy rol veya "Support" izni
    public static bool IsSupportAgent { get { return Role == "SupportAgent" || HasPerm("Support"); } }
    // Yalnizca egitim editoru (portal/destek yetkisi yok) - sade nav/logo icin
    public static bool IsTrainingEditor { get { return CanEditTraining && !IsVendor && !IsSupportAgent; } }

    // Destek kuyruguna erisebilen uretici personeli (portal admin veya destek)
    // Finans: alacak takibi. YALNIZ acikca secilen yetkiyle (VendorAdmin dahil hicbir role kendiliginden verilmez)
    public static bool CanFinance { get { return HasPerm("Finance"); } }
    public static bool IsVendorSide { get { return IsVendor || IsSupportAgent || CanFinance; } }
    // Super admin: butun modul yetkilerine sahip kisi (Portal, Editor, Destek, Finans, Forum Admini). Hicbir role kendiliginden verilmez.
    public static bool IsSuperAdmin { get { return HasPerm("Portal") && HasPerm("Editor") && HasPerm("Support") && HasPerm("Finance") && HasPerm("ForumAdmin"); } }
    // Üretici firmasının herhangi bir personeli (portal/editor/support ya da legacy vendor rol)
    public static bool IsVendorFirm
    {
        get
        {
            return Role == "VendorAdmin" || Role == "SupportAgent" || Role == "SalesRep" ||
                   Role == "TrainingEditor" || Role == "VendorStaff" ||
                   IsVendor || IsSupportAgent || CanEditTraining;
        }
    }
    public static bool IsPartnerSide { get { return Role == "PartnerAdmin" || Role == "PartnerUser"; } }
    public static bool IsPartnerAdmin { get { return Role == "PartnerAdmin"; } }
    // Kisitli bayi kullanicisi (yonetici degil): yalniz Destek + Akademi + Versiyon
    public static bool IsPartnerUserLimited { get { return Role == "PartnerUser"; } }

    /// <summary>Rolun acilis (ana) sayfasi. Giris, Default.aspx ve yetkisiz sayfa yonlendirmeleri HEP bunu kullanir.</summary>
    public static string HomeUrl
    {
        get
        {
            if (IsVendor) return "Vendor.aspx";                              // Portal yetkisi: Genel Bakis
            if (CanFinance && IsVendorFirm) return "Receivables.aspx";       // Finans: Alacaklar
            if (IsVendorFirm) return "Staff.aspx";                           // diger Fol personeli (destek, editor, yetkisiz): personel paneli
            if (IsCustomerSide || IsPartnerUserLimited) return "Customer.aspx";   // musteri + bayi personeli ana sayfasi
            return "Partner.aspx";                                           // bayi yoneticisi
        }
    }
    public static bool IsCustomerSide { get { return Role == "CustomerAdmin" || Role == "CustomerUser"; } }
    // Oturumdaki kullanicinin firma tipi. Vendor / Partner / Customer
    public static string CompanyKind
    {
        get
        {
            if (IsVendorFirm) return "Vendor";
            if (IsCustomerSide) return "Customer";
            return "Partner";
        }
    }

    // Destek kuyruguna erisim: portal admin veya destek personeli
    public static void RequireSupport(Page p)
    {
        if (!IsLoggedIn) { p.Response.Redirect("Login.aspx"); return; }
        if (!(IsVendor || IsSupportAgent)) p.Response.Redirect("Partner.aspx");
    }

    // Vendor su an bir partner adina mi islem yapiyor?
    public static bool IsImpersonating
    {
        get
        {
            if (!IsVendor) return false;
            HttpCookie c = HttpContext.Current.Request.Cookies["imp_company"];
            int t;
            return c != null && int.TryParse(c.Value, out t);
        }
    }

    public static string FullName
    {
        get { var s = HttpContext.Current.Session; return s["FullName"] != null ? (string)s["FullName"] : ""; }
    }

    public static string CompanyName
    {
        get { var s = HttpContext.Current.Session; return s["CompanyName"] != null ? (string)s["CompanyName"] : ""; }
    }

    public static string Culture
    {
        get { var s = HttpContext.Current.Session; return s["Culture"] != null ? (string)s["Culture"] : "tr"; }
        set { HttpContext.Current.Session["Culture"] = value; }
    }

    public static void SignOut()
    {
        HttpContext.Current.Session.Clear();
        HttpContext.Current.Session.Abandon();
        ClearPersistent();
    }

    public static void Require(Page p)
    {
        if (!IsLoggedIn) p.Response.Redirect("Login.aspx");
    }

    public static void RequireVendor(Page p)
    {
        if (!IsLoggedIn) { p.Response.Redirect("Login.aspx"); return; }
        if (!IsVendor) p.Response.Redirect("Partner.aspx");
    }
}
