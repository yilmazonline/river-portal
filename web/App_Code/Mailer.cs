using System;
using System.Net;
using System.Net.Mail;
using System.Web.Configuration;

// SMTP ile e-posta gonderir. Ayarlar web.config -> <appSettings> altindadir.
// Bilgiler girilmemisse gonderim atlanir; islem (demo/kullanici kaydi) yine tamamlanir.
public static class Mailer
{
    private static string Cfg(string key)
    {
        string v = WebConfigurationManager.AppSettings[key];
        return v == null ? "" : v.Trim();
    }

    // Genel SMTP hazir mi (host var mi)
    public static bool HasSmtp
    {
        get { return Cfg("Smtp.Host").Length > 0; }
    }

    // Demo bildirimi icin (host + sabit alici)
    public static bool IsConfigured
    {
        get { return HasSmtp && Cfg("Demo.NotifyEmail").Length > 0; }
    }

    // Demo talebi bildirimi -> sabit Demo.NotifyEmail adresine
    public static bool Send(string subject, string body, out string error)
    {
        error = "";
        if (!IsConfigured) { error = "SMTP/alici yapilandirilmamis."; return false; }
        return SendCore(Cfg("Demo.NotifyEmail"), subject, body, null, null, null, out error);
    }

    // Belirli bir aliciya gonderim (hos geldin / demo onay vb.)
    public static bool SendTo(string to, string subject, string body, out string error)
    {
        return SendTo(to, subject, body, null, out error);
    }

    public static bool SendTo(string to, string subject, string body, System.Collections.Generic.List<string> attachments, out string error)
    {
        error = "";
        if (!HasSmtp) { error = "SMTP yapilandirilmamis."; return false; }
        if (string.IsNullOrEmpty(to)) { error = "Alici adresi yok."; return false; }
        return SendCore(to, subject, body, attachments, null, null, out error);
    }

    // Bilgi (CC) ile gonderim
    public static bool SendToCc(string to, string cc, string subject, string body, out string error)
    {
        error = "";
        if (!HasSmtp) { error = "SMTP yapilandirilmamis."; return false; }
        if (string.IsNullOrEmpty(to)) { error = "Alici adresi yok."; return false; }
        return SendCore(to, subject, body, null, null, cc, out error);
    }

    // Ekli + gizli kopyali gonderim (attachments: "yol|gorunen ad")
    public static bool SendToBccWithFiles(string to, string bcc, string subject, string body, System.Collections.Generic.List<string> attachments, out string error)
    {
        error = "";
        if (!HasSmtp) { error = "SMTP yapilandirilmamis."; return false; }
        if (string.IsNullOrEmpty(to)) { error = "Alici adresi yok."; return false; }
        return SendCore(to, subject, body, attachments, bcc, null, out error);
    }

    // Gizli kopya (BCC) ile gonderim
    public static bool SendToBcc(string to, string subject, string body, string bcc, out string error)
    {
        error = "";
        if (!HasSmtp) { error = "SMTP yapilandirilmamis."; return false; }
        if (string.IsNullOrEmpty(to)) { error = "Alici adresi yok."; return false; }
        return SendCore(to, subject, body, null, bcc, null, out error);
    }

    private static void AddLogo(AlternateView view, string cid, string vpath)
    {
        try
        {
            string p = System.Web.Hosting.HostingEnvironment.MapPath(vpath);
            if (p != null && System.IO.File.Exists(p))
            {
                LinkedResource lr = new LinkedResource(p, new System.Net.Mime.ContentType("image/png"));
                lr.ContentId = cid;
                lr.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                view.LinkedResources.Add(lr);
            }
        }
        catch { }
    }

    private static bool SendCore(string to, string subject, string body, System.Collections.Generic.List<string> attachments, string bcc, string cc, out string error)
    {
        error = "";
        try
        {
            string host = Cfg("Smtp.Host");
            int port = 587; int.TryParse(Cfg("Smtp.Port"), out port);
            string user = Cfg("Smtp.User");
            string pass = Cfg("Smtp.Password");
            string from = Cfg("Smtp.From");
            if (from.Length == 0) from = user;
            bool ssl = true; bool.TryParse(Cfg("Smtp.EnableSsl"), out ssl);

            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(from);
                string[] rcpts = to.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < rcpts.Length; i++) msg.To.Add(rcpts[i].Trim());
                if (!string.IsNullOrEmpty(bcc))
                {
                    string[] bs = bcc.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < bs.Length; i++) msg.Bcc.Add(bs[i].Trim());
                }
                if (!string.IsNullOrEmpty(cc))
                {
                    string[] cs = cc.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < cs.Length; i++) msg.CC.Add(cs[i].Trim());
                }
                msg.Subject = subject;
                msg.IsBodyHtml = true;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.SubjectEncoding = System.Text.Encoding.UTF8;

                // HTML govde + gomulu logolar (cid) - dis adrese bagimli degil
                AlternateView view = AlternateView.CreateAlternateViewFromString(body, System.Text.Encoding.UTF8, "text/html");
                AddLogo(view, "riverlogo", "~/assets/RIVER_AI_black.png");
                AddLogo(view, "follogo", "~/assets/fol_dark.png");
                msg.AlternateViews.Add(view);

                if (attachments != null)
                {
                    for (int i = 0; i < attachments.Count; i++)
                    {
                        // "yol|gorunen ad" bicimi: ek alicida gercek dosya adiyla gorunur (eski cagrilar etkilenmez)
                        string ap = attachments[i] ?? "", an = null; int bar = ap.IndexOf('|'); if (bar > 0) { an = ap.Substring(bar + 1); ap = ap.Substring(0, bar); }
                        try { if (ap.Length > 0 && System.IO.File.Exists(ap)) { Attachment at = new Attachment(ap); if (!string.IsNullOrEmpty(an)) at.Name = an; msg.Attachments.Add(at); } }
                        catch { }
                    }
                }

                using (SmtpClient client = new SmtpClient(host, port))
                {
                    client.EnableSsl = ssl;
                    if (user.Length > 0) client.Credentials = new NetworkCredential(user, pass);
                    client.Send(msg);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
