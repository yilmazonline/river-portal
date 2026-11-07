using System;
using System.Web.UI;

// Partner: musteri kaydi olusturur (Pending). 60 gun koruma. Vendor onayina gider.
public partial class RegisterCustomerPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Partner.aspx"); return; }

        string name = (Request.Form["customer"] ?? "").Trim();
        string contact = (Request.Form["contact"] ?? "").Trim();
        string contactEmail = (Request.Form["contactemail"] ?? "").Trim();
        string contactPhone = (Request.Form["contactphone"] ?? "").Trim();
        string service = (Request.Form["service"] ?? "").Trim();
        int temp = 50;
        int.TryParse(Request.Form["temperature"], out temp);
        if (temp < 1) temp = 1; if (temp > 100) temp = 100;

        if (name.Length == 0) { Response.Redirect("Partner.aspx?err=custname"); return; }

        int companyId = Auth.CompanyId;
        Repo.ExpireOldRegs();
        if (Repo.IsCustomerProtectedByOther(name, companyId))
        { Response.Redirect("Partner.aspx?err=custprotected"); return; }

        int? uid = null;
        object s = Session["UserId"];
        if (s != null) uid = (int)s;

        Repo.CreateCustomerReg(companyId, uid, name, contact, contactEmail, contactPhone, service, temp);

        // Vendor'i bilgilendir (in-app + e-posta)
        try
        {
            int vendorCo = Repo.GetVendorCompanyId();
            if (vendorCo > 0)
                Repo.AddNotification(vendorCo,
                    "Yeni müşteri kaydı onayı", "New customer registration",
                    Auth.CompanyName + " · \"" + name + "\" kaydı onayınızı bekliyor.",
                    Auth.CompanyName + " · \"" + name + "\" awaits your approval.", "CustomerReg");

            if (Mailer.IsConfigured)
            {
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                string[] kv = new string[] {
                    "İş ortağı", Auth.CompanyName, "Müşteri", name,
                    "Kontak", contact.Length > 0 ? contact : "-",
                    "Kontak e-posta", contactEmail.Length > 0 ? contactEmail : "-",
                    "Kontak telefon", contactPhone.Length > 0 ? contactPhone : "-",
                    "Hizmet türü", service.Length > 0 ? service : "-",
                    "Satış sıcaklığı", temp.ToString() + " / 100"
                };
                string html = Emails.Notice("Yeni Müşteri Kaydı · Onay Bekliyor",
                    "Bir iş ortağı yeni bir müşteri kaydı oluşturdu. Onay kuyruğunuzda bekliyor.",
                    kv, baseUrl + "/Vendor.aspx", "Onay Kuyruğuna Git");
                string err;
                Mailer.Send("Yeni Müşteri Kaydı · " + name, html, out err);
            }
        }
        catch { }

        Response.Redirect("Partner.aspx?ok=custreg");
    }
}
