using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Talep yazdırma / PDF: temiz belge düzeni, otomatik yazdırma penceresi (tarayıcıdan "PDF olarak kaydet").
public partial class TicketPrintPage : Page
{
    public Ticket T = null;
    public string Body = "";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        int id;
        if (!int.TryParse(Request.QueryString["id"], out id)) { Response.Redirect("Tickets.aspx"); return; }
        T = Repo.GetTicket(id);
        if (T == null) { Response.Redirect("Tickets.aspx"); return; }
        // Yetki: üretici tarafı ya da talebin firmasındaki kullanıcı
        if (!Auth.IsVendorSide && T.CompanyId != Auth.CompanyId) { Response.Redirect("Tickets.aspx"); return; }

        bool inc = Auth.IsVendorSide; // dahili notlar yalnız üreticide
        List<TicketMessage> msgs = Repo.GetTicketMessages(id, inc);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < msgs.Count; i++)
        {
            TicketMessage m = msgs[i];
            if (m.IsInternal) continue; // belgede yalnız gerçek yazışma
            string kind = (m.MsgKind ?? "").ToLowerInvariant();
            if (kind != "reply" && kind != "") continue; // sistem/log satırlarını atla
            string side = (m.AuthorSide == "Support") ? "Destek" : "Talep Sahibi";
            string body = Fmt.Enc(m.Body ?? "").Replace("\r\n", "<br>").Replace("\n", "<br>");
            sb.Append("<div class=\"msg\"><div class=\"mh\"><b>" + Fmt.Enc(m.AuthorName ?? side) + "</b> · " + side +
                " · " + m.CreatedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) + "</div><div class=\"mb\">" + body + "</div></div>");
        }
        Body = sb.ToString();
    }

    public string Fld(string v) { return string.IsNullOrEmpty(v) ? "—" : Server.HtmlEncode(v); }
    public string Dt(DateTime d) { return d.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR); }
}
