using System;
using System.Web.UI;

// Partner: kayit yenileme talebi gonderir. Vendor onayina gider.
public partial class RenewCustomerRegPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Partner.aspx"); return; }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Partner.aspx"); return; }

        CustomerReg reg = Repo.GetRegById(id);
        if (reg == null || reg.CompanyId != Auth.CompanyId) { Response.Redirect("Partner.aspx"); return; }

        Repo.RequestRenewal(id);

        try
        {
            int vendorCo = Repo.GetVendorCompanyId();
            if (vendorCo > 0)
                Repo.AddNotification(vendorCo,
                    "Kayıt yenileme talebi", "Registration renewal request",
                    Auth.CompanyName + " · \"" + reg.CustomerName + "\" için süre yenileme talep etti.",
                    Auth.CompanyName + " · renewal requested for \"" + reg.CustomerName + "\".", "RenewalReq");

            if (Mailer.IsConfigured)
            {
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                string[] kv = new string[] { "İş ortağı", Auth.CompanyName, "Müşteri", reg.CustomerName };
                string html = Emails.Notice("Kayıt Yenileme Talebi",
                    "Bir iş ortağı müşteri kaydının süresinin yenilenmesini (sıfırlanmasını) talep etti.",
                    kv, baseUrl + "/Vendor.aspx", "Onay Kuyruğuna Git");
                string err;
                Mailer.Send("Kayıt Yenileme Talebi · " + reg.CustomerName, html, out err);
            }
        }
        catch { }

        Response.Redirect("Partner.aspx?ok=renewreq");
    }
}
