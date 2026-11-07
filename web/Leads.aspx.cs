using System;
using System.Web.UI;

// Lead Merkezi: aday havuzu, ice aktarma (CSV, Apollo), onay kuyrugu, sablonlar, engelleme, ayarlar. Yalniz super admin. Veriler LeadAction.ashx'ten.
public partial class LeadsPage : Page
{
    public string Culture = "tr", UserName = "", SetupMsg = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsSuperAdmin) { Response.Redirect(Auth.HomeUrl); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;
        if (!Leads.Ready()) SetupMsg = "Lead tabloları bulunamadı. database\\62_leads.sql dosyasını çalıştırın.";
    }
}
