using System;
using System.Web.UI;

// Siparis durumu guncelle. YALNIZCA vendor.
public partial class UpdateOrderStatusPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Orders.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Orders.aspx"); return; }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Orders.aspx"); return; }
        string status = Request.Form["status"];
        if (status != "Pending" && status != "Approved" && status != "Invoiced" && status != "Rejected")
        { Response.Redirect("Orders.aspx"); return; }

        Repo.UpdateDealStatus(id, status);
        Response.Redirect("Orders.aspx?ok=status");
    }
}
