using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;

// Toplu Bildirim: secili bayi/musterilere uygulama ici bildirim ve/veya temali e-posta (To: info, alicilar BCC).
public partial class BroadcastPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string FlashMsg = "";
    public string FlashErr = "";
    public string PartnerRows = "";
    public string CustomerRows = "";
    public int PartnerCount = 0, CustomerCount = 0;
    public string ToAddress = "info@river.com.tr";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorAdmin) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;
        string cfgTo = (WebConfigurationManager.AppSettings["Broadcast.ToEmail"] ?? "").Trim();
        if (cfgTo.Length > 0) ToAddress = cfgTo;

        if (Request.QueryString["ok"] == "1")
        {
            int a, m, er; int.TryParse(Request.QueryString["a"], out a); int.TryParse(Request.QueryString["m"], out m); int.TryParse(Request.QueryString["e"], out er);
            var parts = new List<string>();
            if (a > 0) parts.Add(a + " firmaya uygulama bildirimi");
            if (m > 0) parts.Add(m + " alıcıya e-posta");
            FlashMsg = (parts.Count > 0 ? string.Join(" ve ", parts.ToArray()) + " gönderildi." : "Gönderim tamamlandı.");
            if (er > 0) FlashErr = er + " e-posta paketi gönderilemedi. SMTP ayarlarını kontrol edin.";
        }

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Send(); if (Response.IsRequestBeingRedirected) return; }
        BuildLists();
    }

    private void BuildLists()
    {
        List<UserRow> all = Repo.GetAllUsers();
        var cnt = new Dictionary<int, int>();
        for (int i = 0; i < all.Count; i++) { int c = all[i].CompanyId; if (!cnt.ContainsKey(c)) cnt[c] = 0; cnt[c]++; }
        List<CompanyLite> ps = Repo.GetCompaniesByType("Partner");
        List<CompanyLite> cs = Repo.GetCompaniesByType("Customer");
        PartnerRows = Rows(ps, cnt, "p", out PartnerCount);
        CustomerRows = Rows(cs, cnt, "c", out CustomerCount);
    }

    private string Rows(List<CompanyLite> list, Dictionary<int, int> cnt, string grp, out int n)
    {
        StringBuilder sb = new StringBuilder(); n = 0;
        for (int i = 0; i < list.Count; i++)
        {
            CompanyLite c = list[i];
            if (c.Name == "Harici Ürün Destekleri") continue;
            int u = cnt.ContainsKey(c.CompanyId) ? cnt[c.CompanyId] : 0;
            sb.Append("<label class=\"bc-firm\" data-name=\"" + Fmt.Enc((c.Name ?? "").ToLowerInvariant()) + "\">" +
                "<input type=\"checkbox\" name=\"cid\" value=\"" + c.CompanyId + "\" data-grp=\"" + grp + "\" data-users=\"" + u + "\" />" +
                "<span class=\"bc-fn\">" + Fmt.Enc(c.Name) + "</span><span class=\"bc-fu\">" + u + " kullanıcı</span></label>");
            n++;
        }
        if (n == 0) return "<div class=\"bc-empty\">Kayıt yok.</div>";
        return sb.ToString();
    }

    private void Send()
    {
        var f = Request.Unvalidated.Form;
        string[] sel = f.GetValues("cid");
        bool chApp = f["ch_app"] != null, chMail = f["ch_mail"] != null;
        string title = (f["title"] ?? "").Trim();
        string body = (f["body"] ?? "").Trim();
        string theme = f["theme"] ?? "duyuru";
        string subject = (f["subject"] ?? "").Trim();
        if (subject.Length == 0) subject = title;
        string btnText = (f["btnText"] ?? "").Trim(), btnUrl = (f["btnUrl"] ?? "").Trim();
        string f1 = (f["f1"] ?? "").Trim(), f2 = (f["f2"] ?? "").Trim(), f3 = (f["f3"] ?? "").Trim();

        if (!chApp && !chMail) { FlashErr = "Lütfen en az bir gönderim kanalı seçin."; return; }
        // Kime (To): formdan; bos ise varsayilan. Diger tum alicilar BCC.
        string toAddr = EmailListParser.Normalize(f["toAddr"] ?? "");
        if (toAddr.Length == 0) toAddr = ToAddress;
        if (chMail && EmailListParser.Problem(toAddr) != null) { FlashErr = "Kime (To) adresi geçerli değil: " + EmailListParser.Problem(toAddr); return; }
        var cids = new List<int>();
        if (sel != null) for (int i = 0; i < sel.Length; i++) { int id; if (int.TryParse(sel[i], out id) && id > 0 && !cids.Contains(id)) cids.Add(id); }
        // ek e-posta adresleri: ekrandaki analizle ayni ayristirici (gecersiz, yazim hatali, tekrar, noreply ve secili firmada zaten alici olanlar elenir)
        List<string> extra = new List<string>();
        if (chMail && (f["extraEmails"] ?? "").Trim().Length > 0)
        {
            EmailListResult er = EmailListParser.Analyze(f["extraEmails"], cids, Repo.GetAllUsers());
            if (er.Truncated) { FlashErr = "Ek adres listesi " + EmailListParser.MaxAddresses + " adres sınırını aşıyor."; return; }
            extra = er.SendList;
        }
        if (cids.Count == 0 && extra.Count == 0) { FlashErr = chMail ? "Lütfen en az bir bayi / müşteri seçin ya da geçerli bir ek e-posta adresi girin." : "Lütfen en az bir bayi veya müşteri seçin."; return; }
        bool custom = theme == "custom";
        if (title.Length == 0) { FlashErr = "Başlık zorunludur."; return; }
        if (body.Length == 0 && (chApp || !custom)) { FlashErr = custom ? "Uygulama içi bildirim için metin zorunludur." : "Başlık ve metin zorunludur."; return; }
        string mailHtml = null;
        if (chMail)
        {
            string buildErr; List<string> notes;
            mailHtml = BroadcastHtml.Build(f, false, out buildErr, out notes);
            if (mailHtml == null) { FlashErr = buildErr; return; }
        }


        int appCount = 0, mailCount = 0, errCount = 0;

        if (chApp)
        {
            string shortBody = body.Replace("\r", "").Replace("\n", " ");
            if (shortBody.Length > 280) shortBody = shortBody.Substring(0, 277) + "...";
            for (int i = 0; i < cids.Count; i++)
            {
                try { Repo.AddNotification(cids[i], title, title, shortBody, shortBody, "Announcement"); appCount++; } catch { }
            }
        }

        if (chMail && Mailer.HasSmtp)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var emails = new List<string>();
            for (int i = 0; i < cids.Count; i++)
            {
                List<UserRow> us = Repo.GetActiveUsersForCompany(cids[i]);
                for (int j = 0; j < us.Count; j++)
                {
                    string em = (us[j].Email ?? "").Trim();
                    if (em.Length == 0 || em.IndexOf('@') <= 0) continue;
                    if (seen.Add(em)) emails.Add(em);
                }
            }
            for (int i = 0; i < extra.Count; i++) if (seen.Add(extra[i])) emails.Add(extra[i]);   // ek adresler (BCC)
            emails.RemoveAll(delegate (string x) { return string.Equals(x, toAddr, StringComparison.OrdinalIgnoreCase); });   // Kime'deki adres BCC'de tekrar yer almaz
            if (emails.Count > 0)
            {
                string html = mailHtml;   // hazir tema ya da temizlenmis ozel HTML (BroadcastHtml.Build)
                const int chunk = 400; // Office 365 alici siniri altinda
                for (int s = 0; s < emails.Count; s += chunk)
                {
                    int len = Math.Min(chunk, emails.Count - s);
                    string bcc = string.Join(",", emails.GetRange(s, len).ToArray());
                    string err;
                    if (Mailer.SendToBcc(toAddr, subject, html, bcc, out err)) mailCount += len; else errCount++;
                }
            }
        }
        else if (chMail) errCount++;

        Response.Redirect("Broadcast.aspx?ok=1&a=" + appCount + "&m=" + mailCount + "&e=" + errCount);
    }
}
