using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Bayi / Müşteri: yeni destek talebi formu.
public partial class NewTicketPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public bool IsCustomer = false;
    public string CustomerOptionsHtml = "";
    public string FlashErr = "";
    public bool NoBalance = false;
    public string SavedPhone = ""; public bool PhoneFeature = false;   // "Sonraki taleplerimde hatirla"

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        // Üretici tarafı talep açmaz; kuyruğa gider
        if (Auth.IsVendorSide) { Response.Redirect("Tickets.aspx"); return; }
        // Hatirlanan telefon numarasi (migration 40 yoksa secenekler gizli kalir)
        try { SavedPhone = TicketPhone.Get(Auth.UserId); PhoneFeature = true; } catch { PhoneFeature = false; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        IsCustomer = Auth.IsCustomerSide;

        if (Request.QueryString["err"] == "1")
            FlashErr = Culture == "en" ? "Please fill in the required fields." : "Lütfen zorunlu alanları doldurun.";
        if (Request.QueryString["err"] == "product")
            FlashErr = Culture == "en" ? "Please select the product your request is about: River AI or River PRO." : "Lütfen talebinizin ilgili olduğu ürünü seçin: River AI ya da River PRO.";
        if (Request.QueryString["err"] == "nobalance" || !Repo.CanSubmitTicket(Auth.CompanyId))
            NoBalance = true;

        if (!IsCustomer) BuildCustomerOptions();
    }

    private void BuildCustomerOptions()
    {
        List<CompanyLite> custs = Repo.GetPartnerCustomerNames(Auth.CompanyId);
        StringBuilder sb = new StringBuilder();
        sb.Append("<option value=\"\">" + (Culture == "en" ? "Select customer" : "Müşteri seçin") + "</option>");
        for (int i = 0; i < custs.Count; i++)
            sb.Append("<option value=\"" + custs[i].CompanyId + "\">" + Fmt.Enc(custs[i].Name) + "</option>");
        CustomerOptionsHtml = sb.ToString();
    }
}
