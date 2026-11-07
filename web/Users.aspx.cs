using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Admin: tüm kullanıcılar + şifre yenileme.
public partial class UsersPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public string UsersHtml = "";
    public string FlashMsg = "";
    public int Count = 0;

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.RequireVendor(this);
        if (Response.IsRequestBeingRedirected) return;

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        bool en = Culture == "en";

        string ok = Request.QueryString["ok"];
        if (ok == "reset") FlashMsg = en ? "Password updated." : "Şifre yenilendi.";
        else if (ok == "edit") FlashMsg = en ? "User updated." : "Kullanıcı güncellendi.";
        else if (ok == "del") FlashMsg = en ? "User removed." : "Kullanıcı silindi.";
        else if (Request.QueryString["err"] == "self") FlashMsg = en ? "You cannot delete your own account." : "Kendi hesabınızı silemezsiniz.";

        List<UserRow> users = Repo.GetAllUsers();
        Count = users.Count;
        UsersHtml = Build(users);
    }

    private string RoleLabelFor(UserRow u)
    {
        string perms = u.Permissions ?? "";
        if (u.Role == "VendorStaff" || perms.Length > 0)
        {
            string p = perms.ToLowerInvariant();
            // super admin: butun modul yetkileri (Auth.IsSuperAdmin ile ayni tanim)
            bool all = true; foreach (string k in new[] { "portal", "editor", "support", "finance", "forumadmin" }) { bool has = false; foreach (string x in p.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)) if (x == k) has = true; if (!has) all = false; }
            if (all) return "Süper Admin (tüm modüller)";
            string lbl = "";
            if (p.IndexOf("portal") >= 0) lbl += (lbl.Length > 0 ? " + " : "") + "Portal Yönetimi";
            if (p.IndexOf("editor") >= 0) lbl += (lbl.Length > 0 ? " + " : "") + "Eğitim Editörü";
            if (p.IndexOf("support") >= 0) lbl += (lbl.Length > 0 ? " + " : "") + "Destek";
            if (p.IndexOf("finance") >= 0) lbl += (lbl.Length > 0 ? " + " : "") + "Finans";
            if (p.IndexOf("forumadmin") >= 0) lbl += (lbl.Length > 0 ? " + " : "") + "Forum Admini";
            return lbl.Length > 0 ? lbl : "Üretici Personeli";
        }
        string r = u.Role ?? "";
        if (r == "VendorAdmin") return "Vendor Yöneticisi (Tümü)";
        if (r == "SalesRep") return "Satış Temsilcisi";
        if (r == "TrainingEditor") return "Eğitim Editörü";
        if (r == "SupportAgent") return "Destek Personeli";
        if (r == "CustomerAdmin") return "Müşteri Yöneticisi";
        if (r == "CustomerUser") return "Müşteri Kullanıcısı";
        if (r == "PartnerAdmin") return "Bayi Yöneticisi";
        return "Bayi Kullanıcısı";
    }

    private string Badge(string txt, string cls) { return "<span class=\"rl-badge " + cls + "\">" + Fmt.Enc(txt) + "</span>"; }

    private string RoleBadgesHtml(UserRow u)
    {
        string perms = (u.Permissions ?? "").ToLowerInvariant();
        string r = u.Role ?? "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder("<div class=\"rl-badges\">");
        if (u.Role == "VendorStaff" || perms.Length > 0)
        {
            bool any = false;
            if (perms.IndexOf("portal") >= 0) { sb.Append(Badge("Portal", "b-portal")); any = true; }
            if (perms.IndexOf("editor") >= 0) { sb.Append(Badge("Eğitim", "b-edu")); any = true; }
            if (perms.IndexOf("support") >= 0) { sb.Append(Badge("Destek", "b-sup")); any = true; }
            if (perms.IndexOf("finance") >= 0) { sb.Append(Badge("Finans", "b-portal")); any = true; }
            if (perms.IndexOf("forumadmin") >= 0) { sb.Append(Badge("Forum Admini", "b-admin")); any = true; }
            if (!any) sb.Append(Badge("Üretici", "b-portal"));
        }
        else if (r == "VendorAdmin") sb.Append(Badge("Vendor Yön. (Tümü)", "b-admin"));
        else if (r == "SalesRep") sb.Append(Badge("Satış Temsilcisi", "b-portal"));
        else if (r == "TrainingEditor") sb.Append(Badge("Eğitim Editörü", "b-edu"));
        else if (r == "SupportAgent") sb.Append(Badge("Destek Personeli", "b-sup"));
        else if (r == "CustomerAdmin") sb.Append(Badge("Müşteri Yön.", "b-cust"));
        else if (r == "CustomerUser") sb.Append(Badge("Müşteri", "b-cust"));
        else if (r == "PartnerAdmin") sb.Append(Badge("Bayi Yön.", "b-part"));
        else sb.Append(Badge("Bayi", "b-part"));
        sb.Append("</div>");
        return sb.ToString();
    }

    private string Build(List<UserRow> users)
    {
        bool en = Culture == "en";
        if (users.Count == 0)
            return "<tr><td colspan=\"7\" style=\"color:var(--muted)\">" + (en ? "No users." : "Kullanıcı yok.") + "</td></tr>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < users.Count; i++)
        {
            UserRow u = users[i];
            string ct = (u.CompanyType ?? "").ToLowerInvariant();
            string ctKey = ct == "vendor" ? "vendor" : (ct == "partner" ? "partner" : (ct == "customer" ? "customer" : "other"));
            sb.Append("<tr data-ctype=\"" + ctKey + "\">");
            if (!string.IsNullOrEmpty(u.AvatarImage))
                sb.Append("<td><span class=\"uava\"><img src=\"" + Fmt.Enc(u.AvatarImage) + "\" alt=\"\" /></span></td>");
            else
                sb.Append("<td><span class=\"uava\">" + Fmt.Enc(Fmt.Initials(u.FullName)) + "</span></td>");
            sb.Append("<td>" + Fmt.Enc(u.FullName) + "</td>");
            sb.Append("<td>" + Fmt.Enc(u.Email) + "</td>");
            sb.Append("<td>" + Fmt.Enc(u.CompanyName) + "</td>");
            sb.Append("<td>" + RoleBadgesHtml(u) + "</td>");
            sb.Append("<td>" + (u.LastLoginAt != null ? u.LastLoginAt.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) : "-") + "</td>");
            sb.Append("<td style=\"text-align:right;white-space:nowrap\">" +
                      (Auth.IsVendorAdmin ? "<a class=\"btn-ghost\" href=\"UserTraining.aspx?id=" + u.UserId + "\" style=\"padding:6px 12px\">" + (en ? "Training" : "Eğitim") + "</a> " : "") +
                      "<a class=\"btn-ghost\" href=\"UserEdit.aspx?id=" + u.UserId + "\" style=\"padding:6px 12px\">" + (en ? "Edit" : "Düzenle") + "</a> " +
                      "<a class=\"btn-ghost\" href=\"ResetPassword.aspx?id=" + u.UserId + "\" style=\"padding:6px 12px\">" + (en ? "Password" : "Şifre") + "</a> " +
                      "<form method=\"post\" action=\"DeleteUser.aspx\" style=\"display:inline\" onsubmit=\"return confirm('Bu kullanıcıyı silmek istediğinize emin misiniz?');\">" +
                      "<input type=\"hidden\" name=\"id\" value=\"" + u.UserId + "\" />" +
                      "<button type=\"submit\" class=\"btn-ghost\" style=\"padding:6px 12px;border-color:rgba(255,61,113,.4);color:#FF9DB5\">" + (en ? "Delete" : "Sil") + "</button></form>" +
                      "</td>");
            sb.Append("</tr>");
        }
        return sb.ToString();
    }
}
