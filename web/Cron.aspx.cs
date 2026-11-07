using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.UI;

// Günlük tetiklenir (Windows Görev Zamanlayıcı). Gizli anahtar ile korunur.
// Çözülmüş taleplerde: 4. günde hatırlatma, 5. günde otomatik kapanış.
public partial class CronPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/plain; charset=utf-8";

        string key = (Request.QueryString["key"] ?? "").Trim();
        string expected = (WebConfigurationManager.AppSettings["Cron.Key"] ?? "").Trim();
        if (expected.Length == 0 || key != expected)
        {
            Response.StatusCode = 403;
            Response.Write("forbidden");
            Response.End();
            return;
        }

        string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

        // Ayri gorev: her gun 12:00 bekleyen destek talepleri ozeti (Cron.aspx?key=...&job=pending).
        // Yalniz bu isi yapar; sabahki isler (hatirlatma, kapanis, ozetler) tekrar calismaz.
        // Lead Merkezi: 10 dakikada bir (Cron.aspx?key=...&job=leads). Yanit okuma, takip taslaklari, gonderim (acik ve mesai penceresindeyse).
        if (string.Equals(Request.QueryString["job"], "leads", StringComparison.OrdinalIgnoreCase))
        {
            try { Server.ScriptTimeout = 300; Response.Write("lead: " + LeadEngine.Tick(baseUrl, true)); } catch (Exception ex) { Response.Write("lead hatasi: " + ex.Message); }
            return;
        }
        // Izin talepleri: saatlik (Cron.aspx?key=...&job=leaves). Hatirlatma, yukseltme, izin oncesi hatirlatma.
        if (string.Equals(Request.QueryString["job"], "leaves", StringComparison.OrdinalIgnoreCase))
        {
            try { Response.Write(Leaves.Ready() ? "izin: " + Leaves.Tick(baseUrl, true) : "izin tablolari yok"); } catch (Exception ex) { Response.Write("izin hatasi: " + ex.Message); }
            return;
        }
        if (string.Equals(Request.QueryString["job"], "pending", StringComparison.OrdinalIgnoreCase))
        {
            Response.Write(SendPendingDigest(baseUrl));
            return;
        }
        int reminded = 0, closed = 0;

        // 1) Hatirlatmalar (4. gun)
        try
        {
            List<Ticket> rem = Repo.GetTicketsNeedingReminder();
            for (int i = 0; i < rem.Count; i++)
            {
                Ticket t = rem[i];
                MailOpener(t, "Talebiniz yakında kapatılacak",
                    "Talebiniz çözüldü olarak işaretlendi ve yanıtlanmadığı için 1 gün içinde otomatik olarak kapatılacaktır. Sorun devam ediyorsa portal üzerinden 'Sorun Çözülmedi' diyerek yeniden açabilirsiniz.",
                    baseUrl + "/TicketDetail.aspx?id=" + t.TicketId);
                Repo.MarkReminderSent(t.TicketId);
                reminded++;
            }
        }
        catch (Exception ex) { Response.Write("reminder-error: " + ex.Message + "\n"); }

        // 2) Otomatik kapanis (5. gun)
        try
        {
            List<Ticket> cl = Repo.GetTicketsNeedingClose();
            for (int i = 0; i < cl.Count; i++)
            {
                Ticket t = cl[i];
                Repo.SetTicketStatus(t.TicketId, "Closed");
                Repo.AddTicketMessage(t.TicketId, null, "System",
                    "Talep, çözüldükten sonra 5 gün içinde yanıt gelmediği için otomatik olarak kapatıldı.", false, "StatusChange");
                MailOpener(t, "Talebiniz kapatıldı",
                    "Talebiniz çözüldükten sonra yanıt gelmediği için otomatik olarak kapatılmıştır. Yeni bir talep açabilir veya bu talebe yanıt yazarak yeniden açabilirsiniz.",
                    baseUrl + "/TicketDetail.aspx?id=" + t.TicketId);
                closed++;
            }
        }
        catch (Exception ex) { Response.Write("close-error: " + ex.Message + "\n"); }

        // 3) Partner seviyelerini son 12 ay siparişlerine göre güncelle (ayda bir: ayın 1'i)
        int tiersChanged = -1;
        if (DateTime.Now.Day == 1)
        {
            try { tiersChanged = Repo.RecomputeTiers(); }
            catch (Exception ex) { Response.Write("tier-error: " + ex.Message + "\n"); }
        }

        // 4) (kaldirildi) Sabah "uzerine alinmamis talepler" e-postasi artik gonderilmiyor; yerine hafta ici 12:00
        //    bekleyen talepler ozeti gidiyor (job=pending), atanmamis talepleri de iceriyor.

        // 5) Dosya paylasimi temizligi: suresi dolan / iptal edilen paylasimlarin dosyalari diskten silinir
        //    (linkler sure dolunca zaten kapanir; bu adim disk alanini bosaltir)
        int transfersCleaned = -1;
        try { transfersCleaned = TransferRepo.Cleanup(); }
        catch (Exception ex) { Response.Write("transfer-cleanup-error: " + ex.Message + "\n"); }
        // Alacak takibi: Teklif'ten yeni siparisleri al; pazartesi sabahi haftalik ozet (yalniz Receivables.DigestTo adresleri)
        int rcvAdded = 0, rcvDigest = 0;
        try { string rerr; rcvAdded = ReceivableRepo.Sync(true, out rerr); string derr; rcvDigest = ReceivableRepo.SendWeeklyDigestIfDue(out derr); }
        catch { }
        // Satis onerileri: Teklif'ten yeni kanal siparisleri (Finans ekraninda eslestirilir)
        try { string serr; SalesSuggestions.Sync(out serr); } catch { }

        Response.Write("ok reminded=" + reminded + " closed=" + closed + " tiersChanged=" + tiersChanged + " transfersCleaned=" + transfersCleaned + " receivablesAdded=" + rcvAdded + " receivableDigest=" + rcvDigest);
    }

    /// <summary>Bekleyen talepler ozeti: To = Support.PendingDigestTo, CC = Support.PendingDigestCc (web.config).
    /// Gunde bir kez (JobRuns); gonderim basarisizsa isaret geri alinir, sonraki cagri yeniden dener.</summary>
    private string SendPendingDigest(string baseUrl)
    {
        const string KEY = "PendingTicketsDigest";
        string to = (WebConfigurationManager.AppSettings["Support.PendingDigestTo"] ?? "").Trim();
        string cc = (WebConfigurationManager.AppSettings["Support.PendingDigestCc"] ?? "").Trim();
        if (to.Length == 0) return "pending-digest: Support.PendingDigestTo ayarı boş, gönderilmedi";
        if (!Mailer.HasSmtp) return "pending-digest: SMTP ayarı yok, gönderilmedi";
        bool force = Request.QueryString["force"] == "1";   // elle deneme: hafta sonu ve ayni gun sinirini atlar
        DayOfWeek dw = DateTime.UtcNow.AddHours(3).DayOfWeek;   // Turkiye saatine gore
        if (!force && (dw == DayOfWeek.Saturday || dw == DayOfWeek.Sunday)) return "pending-digest: hafta sonu, gönderilmedi";
        bool? claim = force ? null : JobRuns.Claim(KEY);   // elle denemede gunluk kayda hic dokunulmaz
        if (claim == false) return "pending-digest: bugün zaten gönderildi";
        try
        {
            List<Ticket> open = new List<Ticket>();
            foreach (Ticket t in Repo.GetAllTickets())
                if (t.Status == "New" || t.Status == "InProgress" || t.Status == "WaitingCustomer") open.Add(t);
            string html = Emails.PendingTickets(open, baseUrl);
            string subject = "Bekleyen destek talepleri · " + DateTime.UtcNow.AddHours(3).ToString("d MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")) + " (" + open.Count + ")";
            string err;
            bool ok = cc.Length > 0 ? Mailer.SendToCc(to, cc, subject, html, out err) : Mailer.SendTo(to, subject, html, out err);
            string res = ok ? "pending-digest: gönderildi " + open.Count + " talep · to=" + to + (cc.Length > 0 ? " cc=" + cc : "") : "pending-digest: gönderilemedi: " + err;
            if (claim == true) { if (ok) JobRuns.Note(KEY, res); else JobRuns.Release(KEY, res); }
            return res + (force ? " (elle deneme)" : claim == null ? " (uyarı: 53_job_runs.sql çalıştırılmamış; aynı gün tekrar gönderim engellenmiyor)" : "");
        }
        catch (Exception ex)
        {
            if (claim == true) JobRuns.Release(KEY, ex.Message);
            return "pending-digest-error: " + ex.Message;
        }
    }

    private void MailOpener(Ticket t, string headline, string intro, string detailUrl)
    {
        try
        {
            if (!Mailer.HasSmtp) return;
            UserRow opener = Repo.GetUserById(t.OpenedByUserId);
            if (opener == null || string.IsNullOrEmpty(opener.Email)) return;
            Ticket cur = Repo.GetTicket(t.TicketId) ?? t;
            int? bal = null; try { bal = Repo.GetSupportPlan(cur.CompanyId).Minutes; } catch { }
            string html = Emails.TicketDetailed(headline, intro, cur, bal, "Sistem (otomatik)", null, false, detailUrl, "Talebi Görüntüle");
            string err;
            Mailer.SendTo(opener.Email, headline + " · " + t.TicketNo, html, out err);
        }
        catch { }
    }
}
