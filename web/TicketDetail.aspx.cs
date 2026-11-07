using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Ticket detay: konuşma akışı + yanıt/durum aksiyonları.
public partial class TicketDetailPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public bool IsSupport = false;
    public bool IsCustomer = false;
    public Ticket T = null;
    public string ThreadHtml = "";
    public string ActivitiesHtml = "";
    public string MetaHtml = "";
    public string AttachHtml = "";
    public string FlashMsg = "";
    public bool CanRemind = false;
    public int RemindWaitHours = 0;
    public string RemindBtnText = "";
    public string RemindInfo = "";
    public List<UserRow> SupportUsers = new List<UserRow>();

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        IsSupport = Auth.IsVendorSide;
        IsCustomer = Auth.IsCustomerSide;

        int id;
        if (!int.TryParse(Request.QueryString["id"], out id)) { Response.Redirect("Tickets.aspx"); return; }
        T = Repo.GetTicket(id);
        if (T == null) { Response.Redirect("Tickets.aspx?gone=1"); return; }
        // Yetki: uretici tarafi her talebi gorur; diger taraf sadece kendi firmasini
        if (!IsSupport && T.CompanyId != Auth.CompanyId) { Response.Redirect("Tickets.aspx"); return; }

        string ok = Request.QueryString["ok"];
        if (ok == "reply") FlashMsg = "Yanıtınız eklendi.";
        else if (ok == "note") FlashMsg = "Dahili not eklendi.";
        else if (ok == "assign") FlashMsg = "Talep üzerinize alındı.";
        else if (ok == "assigned") FlashMsg = "Talep seçtiğiniz kişiye atandı.";
        else if (ok == "resolved") FlashMsg = "Talep çözüldü olarak işaretlendi.";
        else if (ok == "minutes") FlashMsg = "Çözüm süresi kaydedildi.";
        else if (ok == "reopen") FlashMsg = "Talep yeniden açıldı.";
        else if (ok == "closed") FlashMsg = "Talep kapatıldı.";
        else if (ok == "reminded") FlashMsg = "Hatırlatma gönderildi. Destek ekibi bilgilendirildi.";
        else if (ok == "remindwait") FlashMsg = "Bu talep için son 24 saat içinde zaten hatırlatma gönderdiniz.";

        // Destek tarafi ilk kez actiginda: incele/okundu + dahili log
        if (IsSupport && Repo.MarkTicketSeen(id, Auth.FullName))
            Repo.AddTicketMessage(id, Auth.UserId, "System", Auth.FullName + " talebi inceledi.", true, "Seen");

        // Hatirlatma (talep sahibi, acik talep): talep acildiktan 24 saat sonra, hafta ici, 24 saatte bir
        if (!IsSupport && T.Status != "Closed" && T.Status != "Resolved")
        {
            double sinceCreate = (DateTime.UtcNow - T.CreatedAt).TotalHours;
            if (sinceCreate < 24)
            {
                RemindWaitHours = (int)Math.Ceiling(24 - sinceCreate);
                RemindBtnText = "Hatırlatma · yaklaşık " + RemindWaitHours + " saat sonra";
                RemindInfo = "Talep açıldıktan 24 saat sonra hatırlatma gönderebilirsiniz.";
            }
            else
            {
                DateTime? last = Repo.GetTicketReminderAt(id);
                double sinceLast = last.HasValue ? (DateTime.UtcNow - last.Value).TotalHours : 1000;
                DayOfWeek dow = DateTime.UtcNow.ToTrLocal().DayOfWeek;
                bool weekend = (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday);
                if (sinceLast < 24)
                {
                    RemindWaitHours = (int)Math.Ceiling(24 - sinceLast);
                    RemindBtnText = "Hatırlatma · yaklaşık " + RemindWaitHours + " saat sonra";
                    RemindInfo = "Hatırlatma gönderildi. Yaklaşık " + RemindWaitHours + " saat sonra tekrar gönderebilirsiniz.";
                }
                else if (weekend)
                {
                    RemindBtnText = "Hatırlatma · hafta içi";
                    RemindInfo = "Hatırlatma yalnız hafta içi gönderilebilir.";
                }
                else CanRemind = true;
            }
        }

        if (IsSupport) SupportUsers = Repo.GetSupportUsers();

        List<TicketMessage> msgs = Repo.GetTicketMessages(id, IsSupport);
        List<TicketAttachment> atts = Repo.GetTicketAttachments(id);
        foreach (TicketMessage mm in msgs) { _visibleMsg.Add(mm.MessageId); if (mm.IsInternal) _internalMsg.Add(mm.MessageId); }
        ThreadHtml = BuildThread(msgs, atts);
        ActivitiesHtml = BuildActivities(msgs);
        MetaHtml = BuildMeta();
        AttachHtml = BuildAttachTop(atts);
    }

    private string Dt(DateTime d) { return d.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR); }

    private string BuildThread(List<TicketMessage> msgs, List<TicketAttachment> atts)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"tk-thread\">");
        int shown = 0;
        for (int i = 0; i < msgs.Count; i++)
        {
            TicketMessage m = msgs[i];
            if (m.AuthorSide == "System") continue; // sistem/log mesajlari Form Hareketleri'nde
            bool support = m.AuthorSide == "Support";
            string cls = "tk-msg" + (m.IsInternal ? " internal" : (support ? " support" : ""));
            string who = string.IsNullOrEmpty(m.AuthorName) ? "" : m.AuthorName;
            string tag = m.IsInternal ? " · Dahili Not" : (support ? " · Destek" : "");
            sb.Append("<div class=\"" + cls + "\">");
            sb.Append("<div class=\"who\">" + Fmt.Enc(who) + "<span style=\"font-weight:400;color:var(--muted)\">" + tag + "</span></div>");
            sb.Append("<div class=\"when\">" + Dt(m.CreatedAt) + "</div>");
            sb.Append("<div class=\"body\" data-no-i18n=\"1\">" + Fmt.Enc(m.Body) + "</div>");
            for (int k = 0; k < atts.Count; k++)
            {
                TicketAttachment a = atts[k];
                if (a.MessageId.HasValue && a.MessageId.Value == m.MessageId)
                    sb.Append(AttChip(a, true, m.IsInternal));
            }
            sb.Append("</div>");
            shown++;
        }
        if (shown == 0) sb.Append("<div style=\"color:var(--muted);font-size:13px\">Henüz mesaj yok.</div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private string BuildActivities(List<TicketMessage> msgs)
    {
        StringBuilder sb = new StringBuilder();
        int shown = 0;
        for (int i = 0; i < msgs.Count; i++)
        {
            TicketMessage m = msgs[i];
            if (m.AuthorSide != "System") continue;
            sb.Append("<div class=\"tk-act\"><div class=\"tk-act-when\">" + Dt(m.CreatedAt) + "</div><div class=\"tk-act-body\" data-no-i18n=\"1\">" + Fmt.Enc(m.Body) + "</div></div>");
            shown++;
        }
        if (shown == 0) return "<div style=\"color:var(--muted);font-size:12.5px;padding:4px 0\">Henüz hareket yok.</div>";
        return sb.ToString();
    }

    // Gorunur mesajlar (musteri/bayi icin ic notlar zaten haric) ve ic not mesajlari
    private HashSet<int> _visibleMsg = new HashSet<int>(), _internalMsg = new HashSet<int>();

    private string BuildAttachTop(List<TicketAttachment> atts)
    {
        List<string> chips = new List<string>();
        for (int i = 0; i < atts.Count; i++)
        {
            TicketAttachment a = atts[i];
            bool internalAtt = a.MessageId.HasValue && _internalMsg.Contains(a.MessageId.Value);
            // musteri/bayi: yalniz talep eki ya da gordugu mesajlarin ekleri (ic not ekleri gizli)
            if (!IsSupport && a.MessageId.HasValue && !_visibleMsg.Contains(a.MessageId.Value)) continue;
            chips.Add(AttChip(a, false, internalAtt));
        }
        if (chips.Count == 0) return "";
        return "<div class=\"section-h\"><span>Ekler</span></div><div class=\"tk-atts\">" + string.Join("", chips.ToArray()) + "</div>";
    }

    /// <summary>Ek karti: yetki kontrollu baglanti + onizleme bilgisi (js/ticketview.js)</summary>
    private string AttChip(TicketAttachment a, bool small, bool internalAtt)
    {
        string kind = TicketFiles.PreviewKind(a.OriginalName);
        string ext = TicketFiles.Ext(a.OriginalName).TrimStart('.').ToUpperInvariant();
        string url = "TicketFile.ashx?id=" + a.AttachmentId;
        string ic = kind == "image" ? "img" : kind == "pdf" ? "pdf" : kind == "text" ? "txt" : (ext == "XLS" || ext == "XLSX" || ext == "CSV" ? "xls" : (ext == "DOC" || ext == "DOCX" ? "doc" : (ext == "ZIP" ? "zip" : "file")));
        return "<a class=\"tk-att" + (small ? " sm" : "") + "\" href=\"" + url + "\" data-att=\"" + a.AttachmentId + "\" data-kind=\"" + kind + "\" data-name=\"" + Fmt.Enc(a.OriginalName) +
               "\" data-size=\"" + TicketFiles.Size(a.FileSize) + "\" title=\"" + (kind.Length > 0 ? "Önizle" : "İndir") + "\">" +
               "<span class=\"tk-att-ic " + ic + "\">" + (ext.Length > 0 && ext.Length <= 4 ? ext : "DOSYA") + "</span>" +
               "<span class=\"tk-att-tx\"><b>" + Fmt.Enc(a.OriginalName) + "</b><small>" + TicketFiles.Size(a.FileSize) + " · " + (kind.Length > 0 ? "Önizle" : "İndir") +
               (internalAtt && IsSupport ? " · <em>İç not</em>" : "") + "</small></span></a>";
    }

    private string Row(string k, string v)
    {
        if (string.IsNullOrEmpty(v)) v = "-";
        return "<div class=\"k\">" + Fmt.Enc(k) + "</div><div>" + Fmt.Enc(v) + "</div>";
    }

    private string BuildMeta()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"tk-meta\">");
        if (T.Source == "Api")
        {
            sb.Append(Row("Kaynak", "Harici ürün: " + (string.IsNullOrEmpty(T.ExternalProduct) ? "-" : T.ExternalProduct)));
            if (!string.IsNullOrEmpty(T.ExternalRef)) sb.Append(Row("Dış Referans", T.ExternalRef));
            if (!string.IsNullOrEmpty(T.RequesterCompany)) sb.Append(Row("Talep Eden Firma", T.RequesterCompany));
            if (!string.IsNullOrEmpty(T.RequesterName)) sb.Append(Row("Talep Eden Kişi", T.RequesterName));
            if (!string.IsNullOrEmpty(T.RequesterEmail)) sb.Append(Row("E-posta", T.RequesterEmail));
            if (!string.IsNullOrEmpty(T.RequesterPhone)) sb.Append(Row("Telefon", T.RequesterPhone));
        }
        else if (T.Source == "Hizmet")
        {
            sb.Append(Row("Kaynak", "Hizmet Formu — üretici tarafından oluşturuldu"));
            sb.Append(Row("Firma", T.CompanyName));
        }
        else
        {
            sb.Append(Row("Firma", T.CompanyName));
            sb.Append(Row("Talep Sahibi", T.OpenedByName));
            if (!string.IsNullOrEmpty(T.Phone)) sb.Append(Row("Telefon", T.Phone));
            if (!string.IsNullOrEmpty(T.CustomerName)) sb.Append(Row("Müşteri", T.CustomerName));
            sb.Append(Row(Fmt.HasModule(T.Module) ? "Ürün / Modül" : "Ürün", Fmt.ProdMod(T.Product, T.Module, " / ")));
            sb.Append(Row("Talep Tipi", T.RequestType));
            sb.Append(Row("Versiyon", T.Version));
            if (!string.IsNullOrEmpty(T.Environment)) sb.Append(Row("Ortam", T.Environment));
        }
        sb.Append(Row("Öncelik", TicketFmt.PrioLabel(T.Priority)));
        sb.Append(Row("Atanan", string.IsNullOrEmpty(T.AssignedToName) ? "Atanmamış" : T.AssignedToName));
        if (IsSupport && T.ResolutionMinutes.HasValue) sb.Append(Row("Çözüm Süresi", T.ResolutionMinutes.Value + " dk"));
        sb.Append(Row("Oluşturma", Dt(T.CreatedAt)));
        sb.Append("</div>");
        return sb.ToString();
    }

    // Görünüm yardımcıları
    public string StatusLabel { get { return TicketFmt.Display(T, IsSupport); } }
    public string StatusDefault { get { return TicketFmt.StatusLabel(T.Status, IsSupport); } }
    public bool CanEditStatus { get { return IsSupport && Auth.IsVendorFirm && (T.Status == "New" || T.Status == "InProgress"); } }
    public string StatusClass { get { return TicketFmt.StatusClass(T.Status); } }
    public bool CanReopen { get { return !IsSupport && T.Status == "Resolved"; } }
    public bool CanConfirm { get { return !IsSupport && T.Status == "Resolved"; } }
    public bool CanClose { get { return !IsSupport && T.Status != "Closed"; } }
    public bool CanTake { get { return IsSupport && T.Status != "Closed" && !(T.AssignedToUserId.HasValue && T.AssignedToUserId.Value == Auth.UserId); } }
    public bool CanAssign { get { return IsSupport && T.Status != "Closed"; } }
    public bool CanResolveReq { get { return !IsSupport && (T.Status == "New" || T.Status == "InProgress" || T.Status == "WaitingCustomer"); } }

    public string SupportUserOptions
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < SupportUsers.Count; i++)
            {
                UserRow u = SupportUsers[i];
                if (u.UserId == Auth.UserId) continue; // kendisi haric (Uzerime Al zaten var)
                sb.Append("<option value=\"" + u.UserId + "\">" + Fmt.Enc(u.FullName) + "</option>");
            }
            return sb.ToString();
        }
    }
    public bool CanReply { get { return T.Status != "Closed" || IsSupport; } }
}
