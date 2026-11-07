using System.Collections.Generic;
using System;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;

// Ticket aksiyonlari: reply / note / assign / resolve / reopen
public partial class TicketActionPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Tickets.aspx"); return; }

        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("Tickets.aspx"); return; }
        Ticket t = Repo.GetTicket(id);
        if (t == null) { Response.Redirect("Tickets.aspx"); return; }

        bool isSupport = Auth.IsVendorSide;
        if (!isSupport && t.CompanyId != Auth.CompanyId) { Response.Redirect("Tickets.aspx"); return; }

        string action = (Request.Form["action"] ?? "").Trim();
        string body = (Request.Unvalidated.Form["body"] ?? "").Trim();
        int uid = Auth.UserId;
        string detail = "TicketDetail.aspx?id=" + id;
        string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
        string detailUrl = baseUrl + "/" + detail;

        if (action == "note" && isSupport)
        {
            if (body.Length > 0) { int midN = Repo.AddTicketMessage(id, uid, "Support", body, true, "Reply"); SaveReplyFile(id, midN); }
            if (Request.Form["ajax"] == "1") { Response.ContentType = "text/plain; charset=utf-8"; Response.Write("OK"); return; }
            Response.Redirect(detail + "?ok=note"); return;
        }

        if (action == "assignto" && isSupport)
        {
            int target;
            if (int.TryParse(Request.Form["target"], out target) && target > 0)
            {
                Repo.AssignTicket(id, target);
                UserRow tu = Repo.GetUserById(target);
                string tname = tu != null ? tu.FullName : "";
                Repo.AddTicketMessage(id, uid, "System", Auth.FullName + " talebi " + tname + " kişisine atadı.", false, "Assign");
                try
                {
                    if (Mailer.HasSmtp && tu != null && !string.IsNullOrEmpty(tu.Email))
                    {
                        string cc = (WebConfigurationManager.AppSettings["Support.NotifyEmail"] ?? "").Trim();
                        Ticket cur = Repo.GetTicket(id) ?? t;
                        // konu ve baslikta atanan kisinin adi (kopya alan ortak destek kutusu da kime atandigini gorur)
                        string who = string.IsNullOrEmpty(tname) ? "" : tname.Trim();
                        string subject = "Size bir destek talebi atandı" + (who.Length > 0 ? " · " + who : "") + " · " + t.TicketNo;
                        string html = Emails.TicketDetailed((who.Length > 0 ? who + ", size" : "Size") + " bir destek talebi atandı", Auth.FullName + " bu talebi size atadı. Talep ayrıntıları ve yazışmalar aşağıdadır.",
                            cur, Balance(cur), Actor(), Repo.GetTicketMessages(id, true), true, detailUrl, "Talebi Aç");
                        string err;
                        if (cc.Length > 0) Mailer.SendToCc(tu.Email, cc, subject, html, out err);
                        else Mailer.SendTo(tu.Email, subject, html, out err);
                    }
                }
                catch { }
            }
            Response.Redirect(detail + "?ok=assigned"); return;
        }

        if (action == "assign" && isSupport)
        {
            Repo.AssignTicket(id, uid);
            Repo.AddTicketMessage(id, uid, "System", Auth.FullName + " talebi üzerine aldı.", false, "Assign");
            Response.Redirect(detail + "?ok=assign"); return;
        }

        if (action == "resolve" && isSupport)
        {
            int mins; if (!int.TryParse(Request.Form["minutes"], out mins)) mins = 0; mins = Fmt.NormMinutes(mins);
            Repo.SetTicketStatus(id, "Resolved");
            int bal = Repo.SetResolutionMinutes(id, mins);
            Repo.AddTicketMessage(id, uid, "System", "Talep çözüldü olarak işaretlendi. Harcanan süre: " + mins + " dakika. Lütfen çözümü onaylayın veya sorun devam ediyorsa yeniden açın. 5 gün içinde işlem yapılmazsa otomatik kapanacaktır.", false, "StatusChange");
            Repo.AddNotification(t.CompanyId, "Talebiniz çözüldü, onayınızı bekliyor", "Your ticket is resolved",
                "\"" + t.Subject + "\" talebiniz çözüldü olarak işaretlendi (harcanan süre: " + mins + " dk). 5 gün içinde onaylamazsanız talep otomatik kapanacaktır.",
                "Your ticket \"" + t.Subject + "\" was marked resolved (time spent: " + mins + " min). It will auto-close in 5 days unless you confirm.", "Ticket", "TicketDetail.aspx?id=" + id);
            MailOpener(t, "Talebiniz çözüldü, onayınızı bekliyoruz",
                "Destek talebiniz çözüldü olarak işaretlendi. Bu talep için harcanan süre: " + mins + " dakika. Kalan destek süreniz: " + bal + " dakika. Sorun giderildiyse 'Çözümü Onayla ve Kapat', devam ediyorsa 'Sorun Çözülmedi' diyebilirsiniz. 5 gün içinde işlem yapılmazsa talep otomatik kapanır.", detailUrl);
            MailSupport(t, "Çözüm iletildi",
                Actor() + " talebe çözüm iletti ve talebi çözüldü olarak işaretledi. Harcanan süre: " + mins + " dakika. Kalan destek süresi: " + bal + " dakika. Talep sahibinin onayı bekleniyor.",
                detailUrl, true);
            Response.Redirect(detail + "?ok=resolved"); return;
        }

        // Destek personeli: kapali/cozulmus talebe sonradan sure gir/guncelle (bakiyeden duser)
        if (action == "logminutes" && isSupport)
        {
            int mins; if (!int.TryParse(Request.Form["minutes"], out mins)) mins = 0; mins = Fmt.NormMinutes(mins);
            int bal = Repo.SetResolutionMinutes(id, mins);
            Repo.AddTicketMessage(id, uid, "System", Auth.FullName + " bu talep için çözüm süresini " + mins + " dakika olarak kaydetti. (Kalan destek süresi: " + bal + " dk)", false, "StatusChange");
            Response.Redirect(detail + "?ok=minutes"); return;
        }

        // Talep sahibi: talebi çözüldü olarak işaretle
        if (action == "resolve" && !isSupport)
        {
            Repo.SetTicketStatus(id, "Resolved");
            Repo.AddTicketMessage(id, uid, "System", Auth.FullName + " talebi çözüldü olarak işaretledi.", false, "StatusChange");
            MailSupport(t, "Talep çözüldü olarak işaretlendi", "Talep sahibi talebi çözüldü olarak işaretledi: " + t.Subject, detailUrl);
            Response.Redirect(detail + "?ok=resolved"); return;
        }

        // Talep sahibi: hatırlatma gönder (24 saatte bir)
        if (action == "remind" && !isSupport)
        {
            if (t.Status == "Resolved" || t.Status == "Closed") { Response.Redirect(detail); return; }
            if ((DateTime.UtcNow - t.CreatedAt).TotalHours < 24) { Response.Redirect(detail); return; }
            DayOfWeek dow = DateTime.UtcNow.ToTrLocal().DayOfWeek;
            if (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday) { Response.Redirect(detail); return; }
            DateTime? last = Repo.GetTicketReminderAt(id);
            if (last.HasValue && (DateTime.UtcNow - last.Value).TotalHours < 24) { Response.Redirect(detail + "?ok=remindwait"); return; }
            Repo.SetReminderNow(id);
            Repo.AddNotification(Repo.GetVendorCompanyId2(), "Talep hatırlatması", "Ticket reminder",
                "\"" + t.Subject + "\" talebi için " + Auth.CompanyName + " hatırlatma gönderdi.",
                "Reminder for \"" + t.Subject + "\" from " + Auth.CompanyName + ".", "Ticket", "TicketDetail.aspx?id=" + id);
            MailSupport(t, "Talep hatırlatması", Auth.CompanyName + " bu talep için hatırlatma gönderdi: " + t.Subject, detailUrl);
            try
            {
                if (t.AssignedToUserId.HasValue && Mailer.HasSmtp)
                {
                    UserRow au = Repo.GetUserById(t.AssignedToUserId.Value);
                    if (au != null && !string.IsNullOrEmpty(au.Email))
                    {
                        Ticket cur = Repo.GetTicket(id) ?? t;
                        string html = Emails.TicketDetailed("Talep hatırlatması", Actor() + " bu talep için hatırlatma gönderdi.", cur, Balance(cur), Actor(), null, true, detailUrl, "Talebi Aç");
                        string err; Mailer.SendTo(au.Email, "Talep hatırlatması · " + t.TicketNo, html, out err);
                    }
                }
            }
            catch { }
            Repo.AddTicketMessage(id, uid, "System", Auth.FullName + " hatırlatma gönderdi.", false, "Reminder");
            Response.Redirect(detail + "?ok=reminded"); return;
        }

        // Talep sahibi: çözümü onayla ve kapat
        if (action == "close" && !isSupport)
        {
            Repo.SetTicketStatus(id, "Closed");
            Repo.AddTicketMessage(id, uid, "System", Auth.FullName + " çözümü onayladı ve talebi kapattı.", false, "StatusChange");
            MailSupport(t, "Talep kapatıldı · çözüm onaylandı", Actor() + " çözümü onayladı ve talebi kapattı.", detailUrl, true);
            Response.Redirect(detail + "?ok=closed"); return;
        }

        if (action == "reopen" && !isSupport)
        {
            if (t.Status == "Closed") { Response.Redirect(detail); return; }
            if (body.Length == 0) { Response.Redirect(detail); return; }
            int midO = Repo.AddTicketMessage(id, uid, "Requester", body, false, "Reopen");
            SaveReplyFile(id, midO);
            Repo.SetTicketStatus(id, "InProgress");
            MailSupport(t, "Talep yeniden açıldı", "Bir talep yeniden açıldı: " + t.Subject + "\n\nÇözülemeyen/devam eden kısımlar (talep sahibinin notu):\n" + body, detailUrl);
            Response.Redirect(detail + "?ok=reopen"); return;
        }

        // reply (varsayilan)
        if (body.Length == 0) { Response.Redirect(detail); return; }
        if (isSupport)
        {
            int midS = Repo.AddTicketMessage(id, uid, "Support", body, false, "Reply");
            SaveReplyFile(id, midS);
            Repo.AssignTicket(id, uid); // en son yanıtlayan destek personeli atanan olur
            Repo.SetTicketStatus(id, "WaitingCustomer");
            Repo.AddNotification(t.CompanyId, "Talebinize yanıt geldi", "You have a reply",
                "Destek ekibi \"" + t.Subject + "\" talebinize yanıt yazdı; sizden yanıt bekleniyor.",
                "Support replied to your ticket \"" + t.Subject + "\"; awaiting your response.", "Ticket", "TicketDetail.aspx?id=" + id);
            MailOpener(t, "Talebinize yanıt geldi", "Destek ekibi talebinize yanıt yazdı.", detailUrl);
        }
        else
        {
            int midR = Repo.AddTicketMessage(id, uid, "Requester", body, false, "Reply");
            SaveReplyFile(id, midR);
            Repo.SetTicketStatus(id, "InProgress");
            MailSupport(t, "Talebe yeni yanıt", "Bir talebe bayi/müşteri yanıtı eklendi: " + t.Subject, detailUrl);
        }
        Response.Redirect(detail + "?ok=reply");
    }

    // Islemi yapan: "Ad Soyad (Firma)"
    private static string Actor() { return Auth.FullName + (string.IsNullOrEmpty(Auth.CompanyName) ? "" : " (" + Auth.CompanyName + ")"); }
    private static int? Balance(Ticket t) { try { return Repo.GetSupportPlan(t.CompanyId).Minutes; } catch { return null; } }

    // Talep sahibine: guncel talep bilgileriyle (ic notlar ASLA eklenmez)
    private void MailOpener(Ticket t, string headline, string intro, string detailUrl)
    {
        try
        {
            if (!Mailer.HasSmtp) return;
            UserRow opener = Repo.GetUserById(t.OpenedByUserId);
            if (opener == null || string.IsNullOrEmpty(opener.Email)) return;
            Ticket cur = Repo.GetTicket(t.TicketId) ?? t;      // durum/sure guncellemelerinden SONRAKI hali
            string html = Emails.TicketDetailed(headline, intro, cur, Balance(cur), Actor(), null, false, detailUrl, "Talebi Görüntüle");
            string err;
            Mailer.SendTo(opener.Email, headline + " · " + t.TicketNo, html, out err);
        }
        catch { }
    }
    // Destek adresine: guncel talep bilgileri; withConvo ise tum yazismalar (ic notlar etiketli)
    private void MailSupport(Ticket t, string headline, string intro, string detailUrl, bool withConvo)
    {
        try
        {
            if (!Mailer.HasSmtp) return;
            string addr = (WebConfigurationManager.AppSettings["Support.NotifyEmail"] ?? "").Trim();
            if (addr.Length == 0) return;
            Ticket cur = Repo.GetTicket(t.TicketId) ?? t;
            List<TicketMessage> convo = withConvo ? Repo.GetTicketMessages(t.TicketId, true) : null;
            string html = Emails.TicketDetailed(headline, intro, cur, Balance(cur), Actor(), convo, true, detailUrl, "Talebi Aç");
            string err;
            Mailer.SendTo(addr, headline + " · " + t.TicketNo, html, out err);
        }
        catch { }
    }
    private void MailSupport(Ticket t, string headline, string intro, string detailUrl) { MailSupport(t, headline, intro, detailUrl, false); }

    // Yanıt/mesaj ekini kaydet (maks. 5 MB), mesaja bağla.
    private void SaveReplyFile(int ticketId, int messageId)
    {
        try
        {
            HttpPostedFile file = Request.Files["attach"];
            if (file == null || file.ContentLength <= 0) return;
            if (file.ContentLength > 5 * 1024 * 1024) return; // 5 MB
            string dir = Server.MapPath("~/ticketfiles");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!TicketFiles.IsAllowed(file.FileName)) return;   // .aspx/.html gibi turler reddedilir (onceden kontrol yoktu)
            string ext = TicketFiles.Ext(file.FileName);
            string stored = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(dir, stored));
            Repo.AddTicketAttachment(ticketId, messageId, stored, Path.GetFileName(file.FileName), file.ContentType, file.ContentLength);
        }
        catch { }
    }
}
