using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Taleplerim (bayi/müşteri) veya Destek Kuyruğu (üretici tarafı).
public partial class TicketsPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public bool IsSupport = false;
    public int SupportMinutes = 0;
    public string SupportUntilStr = "";
    public bool ShowBalance = false;
    private System.Collections.Generic.HashSet<int> UnseenIds = new System.Collections.Generic.HashSet<int>();
    public bool IsCustomer = false;
    public string Rows = "";
    public string FlashMsg = "";
    public int Count = 0;
    public int CNew = 0, CProgress = 0, CWaiting = 0, CResolved = 0;
    public string View = "active";
    public int ActiveCount = 0, ResolvedCount = 0;

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        IsSupport = Auth.IsVendorSide;
        IsCustomer = Auth.IsCustomerSide;
        if (!IsSupport) { SupportPlan sp = Repo.GetSupportPlan(Auth.CompanyId); SupportMinutes = sp.Minutes; SupportUntilStr = sp.Until.HasValue ? sp.Until.Value.ToString("dd.MM.yyyy") : "süresiz"; ShowBalance = true; }
        else UnseenIds = Repo.GetUnseenTicketIds();

        if (Request.QueryString["gone"] == "1") FlashMsg = "Aradığınız talep artık mevcut değil veya kaldırılmış.";
        string ok = Request.QueryString["ok"];
        if (ok == "created") FlashMsg = "Destek talebiniz oluşturuldu.";
        else if (ok == "reply") FlashMsg = "Yanıtınız eklendi.";
        else if (ok == "status") FlashMsg = "Talep durumu güncellendi.";

        List<Ticket> all = IsSupport ? Repo.GetAllTickets() : Repo.GetTicketsForCompany(Auth.CompanyId);
        View = (Request.QueryString["view"] == "resolved") ? "resolved" : "active";

        List<Ticket> active = new List<Ticket>(), resolved = new List<Ticket>();
        for (int i = 0; i < all.Count; i++)
        {
            string s = all[i].Status;
            // Vendor: Çözüldü+Kapandı çözülenlere. Müşteri/bayi: yalnız Kapandı çözülenlere;
            // "Çözüldü" (destek işaretledi) onaylanana kadar AKTİF listede kalır (gözden kaçmasın).
            bool isDone = IsSupport ? (s == "Resolved" || s == "Closed") : (s == "Closed");
            if (isDone) resolved.Add(all[i]);
            else active.Add(all[i]);
            if (s == "New") CNew++;
            else if (s == "InProgress") CProgress++;
            else if (s == "WaitingCustomer") CWaiting++;
            else if (s == "Resolved") CResolved++;
        }
        ActiveCount = active.Count;
        ResolvedCount = resolved.Count;

        List<Ticket> show = (View == "resolved") ? resolved : active;
        if (View == "active") show.Sort(CompareActive);
        else show.Sort(delegate (Ticket a, Ticket b) { return b.TicketId.CompareTo(a.TicketId); });

        Count = show.Count;
        Rows = Build(show);
    }

    // Aktif liste sirasi: yanitlanmamis/bostaki (Yeni + atanmamis) once
    private int CompareActive(Ticket a, Ticket b)
    {
        int ra = Rank(a.Status), rb = Rank(b.Status);
        if (ra != rb) return ra.CompareTo(rb);
        if (IsSupport)
        {
            int aa = a.AssignedToUserId.HasValue ? 1 : 0;
            int ab = b.AssignedToUserId.HasValue ? 1 : 0;
            if (aa != ab) return aa.CompareTo(ab);
        }
        return b.TicketId.CompareTo(a.TicketId);
    }
    private static int Rank(string s)
    {
        if (s == "Resolved") return -1; // müşteri/bayi aktifinde onay bekleyen çözümler en üstte
        if (s == "New") return 0;
        if (s == "InProgress") return 1;
        if (s == "WaitingCustomer") return 2;
        return 3;
    }

    public string StatusFilterOptions
    {
        get
        {
            string[] st;
            if (View == "resolved")
                st = IsSupport ? new string[] { "Resolved", "Closed" } : new string[] { "Closed" };
            else
                st = IsSupport ? new string[] { "New", "InProgress", "WaitingCustomer" } : new string[] { "New", "InProgress", "WaitingCustomer", "Resolved" };
            StringBuilder sb = new StringBuilder();
            sb.Append("<option value=\"\">Tüm durumlar</option>");
            for (int i = 0; i < st.Length; i++)
                sb.Append("<option value=\"" + st[i] + "\">" + StatusLabel(st[i], IsSupport) + "</option>");
            return sb.ToString();
        }
    }

    public static string StatusLabel(string s) { return TicketFmt.StatusLabel(s, true); }
    public static string StatusLabel(string s, bool isSupport) { return TicketFmt.StatusLabel(s, isSupport); }
    public static string StatusClass(string s) { return TicketFmt.StatusClass(s); }
    public static string PrioLabel(string p) { return TicketFmt.PrioLabel(p); }

    private string Build(List<Ticket> list)
    {
        if (list.Count == 0)
            return "<tr><td colspan=\"" + (IsSupport ? (View == "resolved" ? "9" : "8") : (View == "resolved" ? "8" : "7")) + "\" style=\"color:var(--muted)\">Henüz destek talebi yok.</td></tr>";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            Ticket t = list[i];
            string txt = (t.TicketNo + " " + t.Subject + " " + t.CompanyName + " " + Fmt.Product(t.Product) + " " + t.Product + " " + t.Module + " " + t.RequestType + " " + (t.AssignedToName ?? "")).ToLower(TR);
            sb.Append("<tr class=\"tk-row" + (IsSupport && UnseenIds.Contains(t.TicketId) ? " tk-unread" : "") + "\" data-status=\"" + t.Status + "\" data-text=\"" + Fmt.Enc(txt) + "\" style=\"cursor:pointer\" onclick=\"location.href='TicketDetail.aspx?id=" + t.TicketId + "'\">");
            sb.Append("<td style=\"font-family:var(--mono,monospace);font-size:12.5px\">" + Fmt.Enc(t.TicketNo) + "</td>");
            sb.Append("<td class=\"tk-wrap\">" + Fmt.Enc(t.Subject) + (t.Source == "Hizmet" ? " <span class=\"tk-src\">Hizmet Formu</span>" : "") + "</td>");
            if (IsSupport) sb.Append("<td>" + Fmt.Enc(t.CompanyName) + "</td>");
            sb.Append("<td class=\"tk-wrap tk-mod\" style=\"font-size:12.5px\">" + Fmt.Enc(Fmt.ProdMod(t.Product, t.Module, " · ")) + "</td>");
            sb.Append("<td style=\"font-size:12.5px\">" + Fmt.Enc(t.RequestType) + "</td>");
            sb.Append("<td><span class=\"tk-prio p-" + (t.Priority ?? "Normal").ToLowerInvariant() + "\">" + PrioLabel(t.Priority) + "</span></td>");
            // personel: yeni/incelenen talepte gorunen durum duzenlenebilir (js/ticketstatus.js)
            if (IsSupport && Auth.IsVendorFirm && (t.Status == "New" || t.Status == "InProgress"))
                sb.Append("<td><button type=\"button\" class=\"st st-edit " + StatusClass(t.Status) + "\" data-tid=\"" + t.TicketId + "\" data-note=\"" + Fmt.Enc(t.StatusNote ?? "") + "\" data-def=\"" + Fmt.Enc(StatusLabel(t.Status, IsSupport)) + "\" title=\"Görünen durumu değiştir\"><span>" + Fmt.Enc(TicketFmt.Display(t, IsSupport)) + "</span><svg width=\"12\" height=\"12\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2.2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M4 20h4L19 9l-4-4L4 16z\"/></svg></button></td>");
            else
                sb.Append("<td><span class=\"st " + StatusClass(t.Status) + "\">" + Fmt.Enc(TicketFmt.Display(t, IsSupport)) + "</span></td>");
            if (IsSupport) sb.Append(AgeCell(t));   // yalniz personel: kac gunluk talep / kac gunde cozuldu
            if (IsSupport) sb.Append("<td style=\"font-size:12.5px\">" + (string.IsNullOrEmpty(t.AssignedToName) ? "<span style=\"color:var(--dim)\">Atanmamış</span>" : Fmt.Enc(t.AssignedToName)) + "</td>");
            if (View == "resolved") sb.Append("<td style=\"font-weight:600;white-space:nowrap\">" + (t.ResolutionMinutes.HasValue ? t.ResolutionMinutes.Value + " dk" : "<span style=\"color:var(--dim)\">-</span>") + "</td>");
            sb.Append("</tr>");
        }
        return sb.ToString();
    }

    /// <summary>Personel listesi: aktif sekmede talebin yasi ("Bugun", "1 gun", "12 gun"; 7+ turuncu, 14+ kirmizi),
    /// cozulenlerde kac gunde cozuldugu ("Ayni gun", "3 gunde"). Turkiye saatine gore takvim gunu.</summary>
    private string AgeCell(Ticket t)
    {
        DateTime open = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc).AddHours(3);
        string tip = "Açılış: " + open.ToString("dd.MM.yyyy HH:mm");
        if (View == "resolved")
        {
            if (!t.ResolvedAt.HasValue) return "<td class=\"tk-age\" title=\"" + tip + "\"><span style=\"color:var(--dim)\">-</span></td>";
            DateTime done = DateTime.SpecifyKind(t.ResolvedAt.Value, DateTimeKind.Utc).AddHours(3);
            int dd = Math.Max(0, (done.Date - open.Date).Days);
            return "<td class=\"tk-age\" data-days=\"" + dd + "\" title=\"" + tip + " · Çözüm: " + done.ToString("dd.MM.yyyy HH:mm") + "\">" + (dd == 0 ? "Aynı gün" : dd + " günde") + "</td>";
        }
        int d = Math.Max(0, (DateTime.UtcNow.AddHours(3).Date - open.Date).Days);
        string cls = d >= 14 ? " old" : d >= 7 ? " warn" : "";
        return "<td class=\"tk-age" + cls + "\" data-days=\"" + d + "\" title=\"" + tip + "\">" + (d == 0 ? "Bugün" : d + " gün") + "</td>";
    }
}
