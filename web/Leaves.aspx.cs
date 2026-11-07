using System;
using System.Web;
using System.Web.UI;

// Izinlerim: vendor personeli izin talepleri (yeni talep, taleplerim, onay kutusu, kurallar). Veriler LeaveAction.ashx'ten.
public partial class LeavesPage : Page
{
    public string Culture = "tr", UserName = "", SetupMsg = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        Culture = Auth.Culture; UserName = Auth.FullName;
        if (!Leaves.Ready()) { SetupMsg = "İzin tabloları bulunamadı. database\\61_leave_requests.sql dosyasını çalıştırın."; return; }
        if (!Leaves.IsStaff(Auth.UserId)) { Response.Redirect("Default.aspx"); return; }
    }
}
