using System;
using System.Globalization;
using System.Web.UI;

// Vendor: demo talebini onaylar ve partnere onay e-postasi gonderir.
public partial class ApproveDemoPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Vendor.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Vendor.aspx"); return; }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Vendor.aspx"); return; }

        Repo.ApproveDemo(id);

        try
        {
            DemoContact dc = Repo.GetDemoContact(id);
            if (dc != null && !string.IsNullOrEmpty(dc.Email) && Mailer.HasSmtp)
            {
                string sched = dc.ScheduledDate != null
                    ? dc.ScheduledDate.Value.ToString("dd.MM.yyyy HH:mm", new CultureInfo("tr-TR"))
                    : "-";
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                string html = Emails.DemoApproved(dc.ToName, dc.CompanyName, dc.ClientName, dc.ProductModule,
                                                  sched, baseUrl + "/Login.aspx");
                string err;
                Mailer.SendTo(dc.Email, "Demo talebiniz onaylandı · Portal", html, out err);
            }
        }
        catch { /* e-posta hatasi onayi bozmasin */ }

        Response.Redirect("Vendor.aspx?ok=demoapp");
    }
}
