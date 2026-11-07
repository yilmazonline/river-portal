using System;
using System.Text;
using System.Web.UI;

// Portal admin: kullanıcı ad + yetki düzenleme. Vendor personelinde çoklu yetki (checkbox).
public partial class UserEditPage : Page
{
    public string Culture = "tr";
    public int UserId = 0;
    public string Email = "";
    public string CompanyName = "";
    public string FullName = "";
    public string RoleOptions = "";
    public bool IsVendorUser = false;
    public bool PermPortal = false;
    public bool PermEditor = false;
    public bool PermSupport = false;
    public bool PermFinance = false;
    public bool PermForum = false;
    public string FlashErr = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.RequireVendor(this);
        if (Response.IsRequestBeingRedirected) return;
        Culture = Auth.Culture;

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Save(); return; }

        int id;
        if (!int.TryParse(Request.QueryString["id"], out id)) { Response.Redirect("Users.aspx"); return; }
        UserRow u = Repo.GetUserById(id);
        if (u == null) { Response.Redirect("Users.aspx"); return; }
        UserId = u.UserId; Email = u.Email; CompanyName = u.CompanyName; FullName = u.FullName;
        IsVendorUser = u.CompanyType == "Vendor";
        if (IsVendorUser) SetPerms(u);
        else RoleOptions = BuildRoleOptions(u.CompanyType, u.Role);
    }

    private void SetPerms(UserRow u)
    {
        string p = (u.Permissions ?? "").ToLowerInvariant();
        if (p.Length > 0)
        {
            PermPortal = p.IndexOf("portal") >= 0;
            PermEditor = p.IndexOf("editor") >= 0;
            PermSupport = p.IndexOf("support") >= 0;
            PermFinance = p.IndexOf("finance") >= 0;
            PermForum = p.IndexOf("forumadmin") >= 0;
        }
        else
        {
            string r = u.Role ?? "";
            if (r == "VendorAdmin") { PermPortal = PermEditor = PermSupport = true; }
            else if (r == "SalesRep") PermPortal = true;
            else if (r == "TrainingEditor") PermEditor = true;
            else if (r == "SupportAgent") PermSupport = true;
        }
    }

    private string BuildRoleOptions(string type, string current)
    {
        string[][] roles;
        if (type == "Customer")
            roles = new string[][] {
                new string[]{"CustomerAdmin","Müşteri Yöneticisi"},
                new string[]{"CustomerUser","Müşteri Kullanıcısı"} };
        else
            roles = new string[][] {
                new string[]{"PartnerAdmin","Bayi Yöneticisi"},
                new string[]{"PartnerUser","Bayi Kullanıcısı"} };
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < roles.Length; i++)
            sb.Append("<option value=\"" + roles[i][0] + "\"" + (roles[i][0] == current ? " selected=\"selected\"" : "") + ">" + Fmt.Enc(roles[i][1]) + "</option>");
        return sb.ToString();
    }

    private void Save()
    {
        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Users.aspx"); return; }
        UserRow u = Repo.GetUserById(id);
        if (u == null) { Response.Redirect("Users.aspx"); return; }

        string fullName = (Request.Form["fullname"] ?? "").Trim();
        UserId = u.UserId; Email = u.Email; CompanyName = u.CompanyName; FullName = fullName.Length > 0 ? fullName : u.FullName;
        IsVendorUser = u.CompanyType == "Vendor";

        if (IsVendorUser)
        {
            PermPortal = Request.Form["perm_portal"] == "1";
            PermEditor = Request.Form["perm_editor"] == "1";
            PermSupport = Request.Form["perm_support"] == "1";
            PermFinance = Request.Form["perm_finance"] == "1";
            PermForum = Request.Form["perm_forum"] == "1";
            if (fullName.Length == 0 || (!PermPortal && !PermEditor && !PermSupport && !PermFinance && !PermForum))
            {
                FlashErr = "Lütfen ad soyad girin ve en az bir yetki seçin.";
                return;
            }
            string perms = "";
            if (PermPortal) perms += (perms.Length > 0 ? "," : "") + "Portal";
            if (PermEditor) perms += (perms.Length > 0 ? "," : "") + "Editor";
            if (PermSupport) perms += (perms.Length > 0 ? "," : "") + "Support";
            if (PermFinance) perms += (perms.Length > 0 ? "," : "") + "Finance";
            if (PermForum) perms += (perms.Length > 0 ? "," : "") + "ForumAdmin";
            Repo.UpdateUser(id, fullName, "VendorStaff", perms, true);
            Response.Redirect("Users.aspx?ok=edit");
            return;
        }

        string role = (Request.Form["role"] ?? "").Trim();
        bool ok = (u.CompanyType == "Customer") ? (role == "CustomerAdmin" || role == "CustomerUser")
                                                 : (role == "PartnerAdmin" || role == "PartnerUser");
        if (fullName.Length == 0 || !ok)
        {
            FlashErr = "Lütfen ad soyad ve geçerli bir rol girin.";
            RoleOptions = BuildRoleOptions(u.CompanyType, role.Length > 0 ? role : u.Role);
            return;
        }
        Repo.UpdateUser(id, fullName, role);
        Response.Redirect("Users.aspx?ok=edit");
    }
}
