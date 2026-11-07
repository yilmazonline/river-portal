using System;
using System.Web.UI;

// Vendor: musteri kaydi veya yenileme talebini kabul/reddeder. Her durumda partner + vendor bilgilendirilir.
public partial class CustomerRegDecisionPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Vendor.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Vendor.aspx"); return; }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Vendor.aspx"); return; }
        string action = Request.Form["action"];

        CustomerReg reg = Repo.GetRegById(id);
        if (reg == null) { Response.Redirect("Vendor.aspx"); return; }

        string titleTr, titleEn, okKey;
        bool approved;
        bool isRenewal = false;

        if (action == "approveReg") { Repo.DecideReg(id, true); approved = true; titleTr = "Müşteri kaydınız onaylandı"; titleEn = "Your customer registration is approved"; okKey = "regapp"; }
        else if (action == "rejectReg") { Repo.DecideReg(id, false); approved = false; titleTr = "Müşteri kaydınız reddedildi"; titleEn = "Your customer registration is rejected"; okKey = "regrej"; }
        else if (action == "approveRenew") { Repo.DecideRenewal(id, true); approved = true; isRenewal = true; titleTr = "Kayıt yenileme talebiniz onaylandı"; titleEn = "Your renewal request is approved"; okKey = "renapp"; }
        else if (action == "rejectRenew") { Repo.DecideRenewal(id, false); approved = false; isRenewal = true; titleTr = "Kayıt yenileme talebiniz reddedildi"; titleEn = "Your renewal request is rejected"; okKey = "renrej"; }
        else { Response.Redirect("Vendor.aspx"); return; }

        string bodyTr = "\"" + reg.CustomerName + "\" " + (isRenewal ? "kaydınızın süresi" : "müşteri kaydı") +
                        (approved ? " onaylandı." : " reddedildi.");
        string bodyEn = "\"" + reg.CustomerName + "\" " + (approved ? "approved." : "rejected.");

        // Partner'i bilgilendir (in-app + e-posta)
        try
        {
            Repo.AddNotification(reg.CompanyId, titleTr, titleEn, bodyTr, bodyEn, "CustomerReg");

            // Vendor tarafi kaydi (bilgilendirme izi)
            int vendorCo = Repo.GetVendorCompanyId();
            if (vendorCo > 0)
                Repo.AddNotification(vendorCo,
                    "Karar kaydedildi", "Decision recorded",
                    reg.CompanyName + " · \"" + reg.CustomerName + "\": " + (approved ? "onay" : "ret"),
                    reg.CompanyName + " · \"" + reg.CustomerName + "\": " + (approved ? "approved" : "rejected"),
                    "CustomerReg");

            if (Mailer.HasSmtp)
            {
                string email = Repo.GetCompanyPrimaryEmail(reg.CompanyId);
                if (!string.IsNullOrEmpty(email))
                {
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    string[] kv = new string[] { "Müşteri", reg.CustomerName, "Durum", approved ? "Onaylandı" : "Reddedildi" };
                    string html = Emails.Notice(titleTr,
                        approved ? "İyi haber! Talebiniz onaylandı." : "Talebiniz bu kez onaylanmadı.",
                        kv, baseUrl + "/Login.aspx", "Portala Git");
                    string err;
                    Mailer.SendTo(email, titleTr + " · Portal", html, out err);
                }
            }
        }
        catch { }

        Response.Redirect("Vendor.aspx?ok=" + okKey);
    }
}
