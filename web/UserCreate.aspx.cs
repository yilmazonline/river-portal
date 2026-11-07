using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Vendor: Bayi / Müşteri kullanıcısı veya Destek Personeli oluşturma.
public partial class UserCreatePage : Page
{
    public string Culture = "tr";
    public string PartnersJson = "[]";
    public string CustomersJson = "[]";
    public string GenPassword = "";
    public string FlashErr = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Partner.aspx"); return; }
        Culture = Auth.Culture;

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Save(); return; }

        GenPassword = Fmt.NewPassword();
        BuildLists();
    }

    private void BuildLists()
    {
        PartnersJson = ToJson(Repo.GetCompaniesByType("Partner"));
        CustomersJson = ToJson(Repo.GetCompaniesByType("Customer"));
    }

    private string ToJson(List<CompanyLite> items)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) sb.Append(",");
            string nm = (items[i].Name ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("<", "\\u003C");
            sb.Append("{\"id\":" + items[i].CompanyId + ",\"name\":\"" + nm + "\"}");
        }
        sb.Append("]");
        return sb.ToString();
    }

    private void Save()
    {
        string type = (Request.Form["utype"] ?? "bayi").Trim();
        string fullName = (Request.Form["fullname"] ?? "").Trim();
        string email = (Request.Form["email"] ?? "").Trim();
        string role = (Request.Form["role"] ?? "").Trim();
        string password = (Request.Form["password"] ?? "").Trim();
        int companyId;
        int.TryParse(Request.Form["company"], out companyId);

        string roleLabel;
        string permissions = null;
        if (type == "uretici" || type == "destek")
        {
            companyId = Repo.GetVendorCompanyId2();
            bool pPortal = Request.Form["perm_portal"] == "1";
            bool pEditor = Request.Form["perm_editor"] == "1";
            bool pSupport = Request.Form["perm_support"] == "1";
            bool pFinance = Request.Form["perm_finance"] == "1";
            bool pForum = Request.Form["perm_forum"] == "1";
            if (!pPortal && !pEditor && !pSupport && !pFinance && !pForum)
            {
                FlashErr = "Üretici personeli için en az bir yetki seçin.";
                GenPassword = password.Length > 0 ? password : Fmt.NewPassword();
                BuildLists();
                return;
            }
            permissions = "";
            string lbl = "";
            if (pPortal) { permissions += (permissions.Length > 0 ? "," : "") + "Portal"; lbl += (lbl.Length > 0 ? " + " : "") + "Portal Yönetimi"; }
            if (pEditor) { permissions += (permissions.Length > 0 ? "," : "") + "Editor"; lbl += (lbl.Length > 0 ? " + " : "") + "Eğitim Editörü"; }
            if (pSupport) { permissions += (permissions.Length > 0 ? "," : "") + "Support"; lbl += (lbl.Length > 0 ? " + " : "") + "Destek Personeli"; }
            if (pFinance) { permissions += (permissions.Length > 0 ? "," : "") + "Finance"; lbl += (lbl.Length > 0 ? " + " : "") + "Finans"; }
            if (pForum) { permissions += (permissions.Length > 0 ? "," : "") + "ForumAdmin"; lbl += (lbl.Length > 0 ? " + " : "") + "Forum Admini"; }
            role = "VendorStaff";
            roleLabel = lbl;
        }
        else if (type == "musteri")
        {
            if (role != "CustomerAdmin" && role != "CustomerUser") role = "CustomerUser";
            roleLabel = role == "CustomerAdmin" ? "Müşteri Yöneticisi" : "Müşteri Kullanıcısı";
        }
        else
        {
            if (role != "PartnerAdmin" && role != "PartnerUser") role = "PartnerUser";
            roleLabel = role == "PartnerAdmin" ? "Bayi Yöneticisi" : "Bayi Kullanıcısı";
        }

        if (companyId <= 0)
        {
            FlashErr = (type == "uretici" || type == "destek")
                ? "Üretici firması bulunamadı. Lütfen önce bir üretici (Vendor) firması olduğundan emin olun."
                : (type == "musteri"
                    ? "Lütfen bir müşteri firması seçin. Listede yoksa önce 'Müşteri Oluştur' ile firmayı ekleyin."
                    : "Lütfen bir bayi firması seçin.");
            GenPassword = password.Length > 0 ? password : Fmt.NewPassword();
            BuildLists();
            return;
        }
        if (fullName.Length == 0) { FlashErr = "Ad soyad zorunludur."; GenPassword = password.Length > 0 ? password : Fmt.NewPassword(); BuildLists(); return; }
        if (email.Length == 0) { FlashErr = "E-posta zorunludur."; GenPassword = password.Length > 0 ? password : Fmt.NewPassword(); BuildLists(); return; }
        if (password.Length < 4) { FlashErr = "Parola en az 4 karakter olmalıdır."; GenPassword = Fmt.NewPassword(); BuildLists(); return; }
        if (Repo.EmailExists(email))
        {
            FlashErr = "Bu e-posta zaten kayıtlı: " + email;
            GenPassword = password;
            BuildLists();
            return;
        }

        string hash = PasswordHasher.Hash(password);
        string culture = (Request.Form["culture"] == "en") ? "en" : "tr";
        Repo.CreateUser(companyId, fullName, email, hash, role, culture, permissions);

        // Hoş geldin maili için rol anahtarı (modül içeriği) + firma adı
        string roleKey;
        if (type == "musteri") roleKey = "customer";
        else if (type == "uretici" || type == "destek")
        {
            string p = (permissions ?? "").ToLowerInvariant();
            if (p.IndexOf("portal") >= 0) roleKey = "vendor";
            else if (p.IndexOf("support") >= 0) roleKey = "support";
            else roleKey = "editor";
        }
        else roleKey = "partner";
        string companyName = "";
        try { CompanyInfo co = Repo.GetCompany(companyId); if (co != null) companyName = co.Name; }
        catch { }

        try
        {
            if (Mailer.HasSmtp)
            {
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                string loginUrl = baseUrl + "/Login.aspx";
                string html = Emails.Welcome(fullName, email, password, companyName, loginUrl, roleKey, culture);
                string err;
                string subj = culture == "en" ? "River Portal · Your account is ready" : "River Portalı · Hesabınız oluşturuldu";
                string bcc = (System.Web.Configuration.WebConfigurationManager.AppSettings["UserCreate.BccEmail"] ?? "").Trim();
                if (bcc.Length > 0) Mailer.SendToBcc(email, subj, html, bcc, out err);
                else Mailer.SendTo(email, subj, html, out err);
            }
        }
        catch { }

        Response.Redirect("Vendor.aspx?ok=user");
    }
}
