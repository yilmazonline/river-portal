using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;

// Paylasim detayi: dosyalar, alicilarin indirme durumu, sure uzatma / iptal / yeniden gonderme.
public partial class TransferDetailPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public TransferInfo T = null;
    public string FilesHtml = "", RecipientsHtml = "";
    public string FlashMsg = "";
    public string ExpiresStr = "", SentStr = "";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorFirm) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;
        int id; int.TryParse(Request.QueryString["id"], out id);
        T = id > 0 ? TransferRepo.GetTransfer(id) : null;
        if (T == null || T.Status == "Uploading" || (T.CreatedBy != Auth.UserId && !Auth.IsVendor)) { Response.Redirect("Transfers.aspx"); return; }

        string ok = Request.QueryString["ok"];
        if (ok == "ext") FlashMsg = "Geçerlilik süresi 7 gün uzatıldı.";
        else if (ok == "cancel") FlashMsg = "Paylaşım iptal edildi; linkler kapandı ve dosyalar silindi.";
        else if (ok == "resent") FlashMsg = "E-posta yeniden gönderildi.";
        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Act(); if (Response.IsRequestBeingRedirected) return; }

        ExpiresStr = T.ExpiresAt.ToTrLocal().ToString("dd MMMM yyyy HH:mm", TR);
        SentStr = (T.SentAt ?? T.CreatedAt).ToTrLocal().ToString("dd MMMM yyyy HH:mm", TR);

        List<TransferFile> files = TransferRepo.GetFiles(T.TransferId);
        StringBuilder fb = new StringBuilder();
        for (int i = 0; i < files.Count; i++)
            fb.Append("<div class=\"tf-file\"><span class=\"tf-fn\">" + Fmt.Enc(files[i].OriginalName) + "</span><span class=\"tf-fs\">" + TransferRepo.FormatSize(files[i].SizeBytes) + "</span></div>");
        FilesHtml = fb.ToString();

        string b = BaseUrl();
        List<TransferRecipient> rs = TransferRepo.GetRecipients(T.TransferId);
        StringBuilder rb = new StringBuilder();
        for (int i = 0; i < rs.Count; i++)
        {
            TransferRecipient r = rs[i];
            string state = r.DownloadCount > 0
                ? "<span class=\"st approved\">İndirdi</span><small>" + r.DownloadCount + " kez · son " + r.LastDownloadAt.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) + "</small>"
                : "<span class=\"st pending\">İndirmedi</span>";
            rb.Append("<tr><td><b>" + Fmt.Enc(string.IsNullOrEmpty(r.DisplayName) ? r.Email : r.DisplayName) + "</b>" +
                (r.UserId.HasValue ? " <span class=\"tf-tag\">Portal</span>" : " <span class=\"tf-tag ext\">Harici</span>") +
                "<br/><small>" + Fmt.Enc(r.Email) + (string.IsNullOrEmpty(r.CompanyName) ? "" : " · " + Fmt.Enc(r.CompanyName)) + "</small></td>" +
                "<td class=\"tf-st\">" + state + "</td><td class=\"tf-act\">");
            if (T.IsLive)
                rb.Append("<button type=\"button\" class=\"btn-ghost tf-copy\" data-url=\"" + Fmt.Enc(b + "/t/" + r.Token) + "\">Linki kopyala</button>" +
                    "<form method=\"post\" data-noload=\"1\" style=\"display:inline\" onsubmit=\"return confirm('E-posta bu alıcıya yeniden gönderilsin mi?')\">" +
                    "<input type=\"hidden\" name=\"action\" value=\"resend\" /><input type=\"hidden\" name=\"rid\" value=\"" + r.RecipientId + "\" />" +
                    "<button class=\"btn-ghost\" type=\"submit\">Yeniden gönder</button></form>");
            rb.Append("</td></tr>");
        }
        RecipientsHtml = rb.ToString();
    }

    private string BaseUrl()
    {
        string b = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (b.Length == 0) b = Request.Url.GetLeftPart(UriPartial.Authority);
        return b.TrimEnd('/');
    }

    private void Act()
    {
        string a = Request.Form["action"] ?? "";
        string self = "TransferDetail.aspx?id=" + T.TransferId;
        if (a == "extend" && T.Status == "Sent") { TransferRepo.ExtendExpiry(T.TransferId, 7); Response.Redirect(self + "&ok=ext"); return; }
        if (a == "cancel" && T.Status == "Sent") { TransferRepo.SetStatus(T.TransferId, "Cancelled"); TransferRepo.DeleteStorage(T.TransferId); Response.Redirect(self + "&ok=cancel"); return; }
        if (a == "resend" && T.IsLive)
        {
            int rid; int.TryParse(Request.Form["rid"], out rid);
            TransferRecipient r = rid > 0 ? TransferRepo.GetRecipient(rid) : null;
            if (r != null && r.TransferId == T.TransferId && Mailer.HasSmtp)
            {
                List<TransferFile> files = TransferRepo.GetFiles(T.TransferId);
                List<string> fn = new List<string>(), fs = new List<string>();
                for (int i = 0; i < files.Count; i++) { fn.Add(files[i].OriginalName); fs.Add(TransferRepo.FormatSize(files[i].SizeBytes)); }
                string html = Emails.TransferMail(T.CreatedByName ?? Auth.FullName, T.Title, T.Message, fn, fs, TransferRepo.FormatSize(T.TotalBytes),
                    T.ExpiresAt.ToTrLocal().ToString("dd MMMM yyyy HH:mm", TR), T.HasPassword, BaseUrl() + "/t/" + r.Token);
                string err;
                if (Mailer.SendTo(r.Email, "River · " + (T.CreatedByName ?? Auth.FullName) + " sizinle dosya paylaştı: " + T.Title, html, out err)) TransferRepo.TouchResent(rid);
            }
            Response.Redirect(self + "&ok=resent"); return;
        }
        Response.Redirect(self);
    }

    public string V(string s) { return Server.HtmlEncode(s ?? ""); }
}
