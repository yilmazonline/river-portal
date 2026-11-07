using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;

// Bayi / Müşteri: destek talebini kaydeder (dosya + mail).
public partial class AddTicketPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (Auth.IsVendorSide) { Response.Redirect("Tickets.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("NewTicket.aspx"); return; }

        var f = Request.Unvalidated.Form;
        string product = (f["product"] ?? "").Trim();
        string module = (f["module"] ?? "").Trim();
        string requestType = (f["requesttype"] ?? "").Trim();
        string priority = (f["priority"] ?? "Normal").Trim();
        string version = (f["version"] ?? "").Trim();
        string environment = (f["environment"] ?? "").Trim();
        string phone = (f["phone"] ?? "").Trim();
        string subject = (f["subject"] ?? "").Trim();
        string description = (f["description"] ?? "").Trim();

        if (priority != "Low" && priority != "Normal" && priority != "High" && priority != "Critical") priority = "Normal";
        // Urun: yalniz River AI ya da River PRO; secilmeden gonderilemez (yanlis urune dusmesin). Eski "River" degeri River PRO sayilir.
        if (product == "River") product = "River PRO";
        if (product != "River AI" && product != "River PRO") { Response.Redirect("NewTicket.aspx?err=product"); return; }
        if (module.Length == 0) module = "Genel";   // alt modul secimi kaldirildi

        if (subject.Length == 0 || version.Length == 0 || description.Length == 0)
        { Response.Redirect("NewTicket.aspx?err=1"); return; }

        // Destek bakiyesi kontrolu: bakiye yoksa talep gonderilemez
        if (!Repo.CanSubmitTicket(Auth.CompanyId))
        { Response.Redirect("NewTicket.aspx?err=nobalance"); return; }

        int companyId = Auth.CompanyId;
        int uid = Auth.UserId;

        // Bayi ise: kayitli musteri secimi (opsiyonel)
        int? customerRegId = null;
        string customerName = null;
        if (!Auth.IsCustomerSide)
        {
            int rid;
            if (int.TryParse(f["customer"], out rid) && rid > 0)
            {
                customerRegId = rid;
                List<CompanyLite> cs = Repo.GetPartnerCustomerNames(companyId);
                for (int i = 0; i < cs.Count; i++) { if (cs[i].CompanyId == rid) { customerName = cs[i].Name; break; } }
            }
        }

        string ticketNo;
        int ticketId = Repo.CreateTicket(companyId, uid, product, module, requestType, priority,
            customerRegId, customerName, version, environment, subject, phone, description, out ticketNo);

        // AI on yaniti gosterildiyse talebe bagla ve ekip icin dahili not ekle (hata talebi asla engellemez)
        int aiAnswerId;
        if (ticketId > 0 && int.TryParse(f["aiAnswerId"], out aiAnswerId) && aiAnswerId > 0)
        {
            try { string note = Kb.AttachToTicket(aiAnswerId, uid, ticketId); if (!string.IsNullOrEmpty(note)) Repo.AddTicketMessage(ticketId, null, "Support", note, true, "Reply"); } catch { }
        }

        if (ticketId <= 0) { Response.Redirect("NewTicket.aspx?err=1"); return; }

        // "Sonraki taleplerimde hatirla": yalniz isaretliyse kaydet (acik onay); hata talebi etkilemez
        if (f["rememberPhone"] == "1" && phone.Length > 0) { try { TicketPhone.Save(uid, phone); } catch { } }

        // Ekler
        System.Collections.Generic.List<string> attachPaths = new System.Collections.Generic.List<string>();
        string attachNames = "";
        try
        {
            if (Request.Files.Count > 0)
            {
                string dir = Server.MapPath("~/ticketfiles");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFile file = Request.Files[i];
                    if (file == null || file.ContentLength <= 0) continue;
                    if (file.ContentLength > 10 * 1024 * 1024) continue; // en fazla 10 MB
                    string ext = TicketFiles.Ext(file.FileName);
                    if (!TicketFiles.IsAllowed(file.FileName)) continue;   // ortak izin listesi (App_Code/TicketFiles.cs)
                    string stored = Guid.NewGuid().ToString("N") + ext;
                    file.SaveAs(Path.Combine(dir, stored));
                    Repo.AddTicketAttachment(ticketId, null, stored, Path.GetFileName(file.FileName), file.ContentType, file.ContentLength);
                    attachPaths.Add(Path.Combine(dir, stored));
                    attachNames += (attachNames.Length > 0 ? ", " : "") + Path.GetFileName(file.FileName);
                }
            }
        }
        catch { }

        // Destek ekibine uygulama ici bildirim (uretici firma -> bell)
        try
        {
            Repo.AddNotification(Repo.GetVendorCompanyId2(), "Yeni destek talebi", "New support ticket",
                "\"" + subject + "\" konulu yeni bir destek talebi açıldı (" + Auth.CompanyName + " · " + Auth.FullName + ").",
                "A new support ticket was opened: \"" + subject + "\" (" + Auth.CompanyName + " · " + Auth.FullName + ").", "Ticket", "TicketDetail.aspx?id=" + ticketId);
        }
        catch { }

        // Mailler
        try
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            string detailUrl = baseUrl + "/TicketDetail.aspx?id=" + ticketId;

            // Talep sahibine onay
            UserRow opener = Repo.GetUserById(uid);
            if (Mailer.HasSmtp && opener != null && !string.IsNullOrEmpty(opener.Email))
            {
                string html = Emails.TicketConfirm(opener.FullName, ticketNo, subject, product, module, detailUrl, description, attachNames, Auth.Culture);
                string err;
                string subjC = Auth.Culture == "en" ? "Your support request was received · " + ticketNo : "Destek talebiniz alındı · " + ticketNo;
                Mailer.SendTo(opener.Email, subjC, html, attachPaths, out err);
            }
            // Destek ekibine bildirim (sabit adres)
            string supportAddr = (WebConfigurationManager.AppSettings["Support.NotifyEmail"] ?? "").Trim();
            if (Mailer.HasSmtp && supportAddr.Length > 0)
            {
                SupportPlan sp = Repo.GetSupportPlan(Auth.CompanyId);
                string compLine = Auth.CompanyName + " · Kalan destek: " + sp.Minutes + " dk" + (sp.Until.HasValue ? " · Anlaşma son tarih: " + sp.Until.Value.ToString("dd.MM.yyyy") : " · süresiz");
                string html2 = Emails.TicketNotify(ticketNo, compLine, opener != null ? opener.FullName : Auth.FullName, opener != null ? opener.Email : null, phone, customerName, environment,
                                                   subject, product, module, requestType, priority, version, detailUrl, description, attachNames);
                string err2;
                Mailer.SendTo(supportAddr, "Yeni destek talebi · " + ticketNo, html2, attachPaths, out err2);
            }
        }
        catch { }

        Response.Redirect("Tickets.aspx?ok=created");
    }
}
