using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Configuration;

// Kurumsal HTML e-posta sablonlari (inline stil; e-posta istemcileri icin table-tabanli).
public static class Emails
{
    private static string E(string s) { return Fmt.Enc(s); }

    // Mail icindeki gorseller icin mutlak taban adres (web.config: Site.BaseUrl)
    private static string BaseUrl()
    {
        string b = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (b.Length == 0) b = "https://partner.folyazilim.com";
        return b.TrimEnd('/');
    }

    // Ortak cerceve
    // Alacak takibi ozeti gibi diger moduller de kurumsal sablonu kullanir
    public static string Shell(string title, string innerHtml)
    {
        return
"<div style=\"margin:0;padding:0;background:#EDF1F7;\">" +
"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#EDF1F7;padding:32px 0;\"><tr><td align=\"center\">" +
"<table role=\"presentation\" width=\"560\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:560px;max-width:92%;background:#FFFFFF;border:1px solid #E3E8F0;border-radius:18px;overflow:hidden;font-family:Segoe UI,Arial,sans-serif;box-shadow:0 10px 30px rgba(20,30,60,.08);\">" +
// header
"<tr><td style=\"padding:26px 32px 20px;background:#FFFFFF;border-bottom:1px solid #EEF1F6;\">" +
"<img src=\"cid:riverlogo\" alt=\"River AI\" height=\"30\" style=\"display:block;border:0;height:30px;\" />" +
"<span style=\"display:block;font-size:11px;letter-spacing:.22em;text-transform:uppercase;color:#8A97AE;margin-top:10px;\">Asistan Portal</span>" +
"</td></tr>" +
// accent line
"<tr><td style=\"height:3px;background:#4F46E5;line-height:3px;font-size:0;\">&nbsp;</td></tr>" +
// body
"<tr><td style=\"padding:30px 32px;color:#3A4A66;font-size:14px;line-height:1.7;\">" +
"<h2 style=\"margin:0 0 16px;font-size:20px;color:#1B2440;font-weight:700;\">" + E(title) + "</h2>" +
innerHtml +
"</td></tr>" +
// footer
"<tr><td style=\"padding:22px 32px;background:#F5F7FB;border-top:1px solid #EEF1F6;text-align:center;\">" +
"<img src=\"cid:follogo\" alt=\"Fol Yazılım\" height=\"20\" style=\"height:20px;border:0;\" />" +
"<div style=\"font-size:12.5px;font-weight:600;color:#41506E;margin-top:8px;\">Fol Yazılım A.Ş.</div>" +
"<div style=\"font-size:11px;color:#9AA6BC;margin-top:4px;\">Bu e-posta River Asistan Portal tarafından gönderilmiştir.</div>" +
"</td></tr>" +
"</table></td></tr></table></div>";
    }

    private static string Button(string url, string text)
    {
        return
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:22px 0 6px;\"><tr>" +
"<td style=\"background:#4F46E5;border-radius:10px;\">" +
"<a href=\"" + E(url) + "\" style=\"display:inline-block;padding:12px 26px;color:#FFFFFF;text-decoration:none;font-weight:700;font-size:14px;\">" + E(text) + "</a>" +
"</td></tr></table>";
    }

    private static string Row(string k, string v)
    {
        return
"<tr>" +
"<td style=\"padding:9px 0;color:#8A97AE;font-size:13px;width:150px;\">" + E(k) + "</td>" +
"<td style=\"padding:9px 0;color:#1B2440;font-size:14px;font-weight:600;\">" + E(v) + "</td>" +
"</tr>";
    }

    // İçerik önerisi (destek ekibine)
    public static string ContentSuggestion(string fullName, string company, string title, string body)
    {
        string inner =
"<p style=\"margin:0 0 14px;\">Bir kullanıcı yeni eğitim içeriği önerdi.</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" +
Row("Gönderen", fullName) + Row("Firma", company) + (string.IsNullOrEmpty(title) ? "" : Row("Başlık", title)) +
"</table>" +
"<p style=\"margin:16px 0 4px;color:#8A97AE;font-size:12px;\">Öneri</p>" +
"<div style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:14px 16px;white-space:pre-wrap;\">" + E(body) + "</div>";
        return Shell("Eğitim içerik önerisi", inner);
    }

    // Talep guncelleme (yanit / durum / yeniden acma)
    public static string TicketUpdate(string headline, string intro, string ticketNo, string subject, string detailUrl, string btn)
    {
        string inner =
"<p style=\"margin:0 0 18px;\">" + E(intro).Replace("\r\n", "<br>").Replace("\n", "<br>") + "</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" +
Row("Talep No", ticketNo) + Row("Konu", subject) +
"</table>" +
Button(detailUrl, string.IsNullOrEmpty(btn) ? "Talebi Görüntüle" : btn);
        return Shell(headline, inner);
    }

    // ===================== Ayrintili talep e-postasi =====================
    // Tum talep guncelleme e-postalarinda: talep bilgileri + islemi yapan kisi; istenirse yazismalar.
    // showInternal yalniz destek adresine giden e-postalarda true olur (ic notlar musteriye ASLA gitmez).
    private static string TrStatus(string s)
    {
        switch (s ?? "") { case "New": return "Yeni"; case "Open": return "Açık"; case "InProgress": return "İşlemde"; case "WaitingCustomer": return "Talep sahibinin yanıtı bekleniyor";
            case "Resolved": return "Çözüldü, onay bekleniyor"; case "Closed": return "Kapandı"; default: return s ?? ""; }
    }
    private static string TrPriority(string p)
    {
        switch ((p ?? "").ToLowerInvariant()) { case "high": return "Yüksek"; case "low": return "Düşük"; case "normal": return "Normal"; default: return p ?? ""; }
    }
    private static string Tr(DateTime? d) { return d.HasValue ? d.Value.ToTrLocal().ToString("dd.MM.yyyy HH:mm") : "·"; }

    public static string TicketDetailed(string headline, string intro, Ticket t, int? balanceMinutes, string actor,
                                        List<TicketMessage> convo, bool showInternal, string detailUrl, string btn)
    {
        bool api = !string.IsNullOrEmpty(t.RequesterEmail);
        string company = api ? (t.RequesterCompany ?? "") + (string.IsNullOrEmpty(t.ExternalProduct) ? "" : " · " + t.ExternalProduct + " (API)") : (t.CompanyName ?? "");
        string requester;
        if (api) requester = (t.RequesterName ?? "") + " · " + t.RequesterEmail;
        else
        {
            string em = ""; try { UserRow u = Repo.GetUserById(t.OpenedByUserId); if (u != null) em = u.Email ?? ""; } catch { }
            requester = (t.OpenedByName ?? "") + (em.Length > 0 ? " · " + em : "");
        }
        string prod = Fmt.ProdMod(t.Product, t.Module, " / ");
        StringBuilder rows = new StringBuilder();
        rows.Append(Row("Talep No", t.TicketNo)).Append(Row("Konu", t.Subject)).Append(Row("Firma", company)).Append(Row("Talep sahibi", requester));
        if (prod.Length > 0) rows.Append(Row(Fmt.HasModule(t.Module) ? "Ürün / Modül" : "Ürün", prod));
        if (!string.IsNullOrEmpty(t.RequestType)) rows.Append(Row("Talep türü", t.RequestType));
        rows.Append(Row("Öncelik", TrPriority(t.Priority))).Append(Row("Durum", TrStatus(t.Status)));
        rows.Append(Row("Açılış", Tr(t.CreatedAt))).Append(Row("Atanan uzman", string.IsNullOrEmpty(t.AssignedToName) ? "Atanmadı" : t.AssignedToName));
        if (t.ResolvedAt.HasValue) rows.Append(Row("Çözüm tarihi", Tr(t.ResolvedAt)));
        rows.Append(Row("Harcanan süre", t.ResolutionMinutes.HasValue ? t.ResolutionMinutes.Value + " dakika" : "Girilmedi"));
        if (balanceMinutes.HasValue) rows.Append(Row("Kalan destek süresi", balanceMinutes.Value + " dakika"));
        if (!string.IsNullOrEmpty(actor)) rows.Append(Row("İşlemi yapan", actor + " · " + DateTime.UtcNow.ToTrLocal().ToString("dd.MM.yyyy HH:mm")));

        StringBuilder sb = new StringBuilder();
        sb.Append("<p style=\"margin:0 0 18px;\">" + E(intro).Replace("\r\n", "<br>").Replace("\n", "<br>") + "</p>");
        sb.Append("<p style=\"margin:0 0 6px;color:#8A97AE;font-size:12px;font-weight:600;\">TALEP BİLGİLERİ</p>");
        sb.Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" + rows + "</table>");

        if (convo != null && convo.Count > 0)
        {
            int visible = 0; foreach (TicketMessage x in convo) if (!x.IsInternal || showInternal) visible++;
            sb.Append("<p style=\"margin:22px 0 8px;color:#8A97AE;font-size:12px;font-weight:600;\">YAZIŞMALAR (" + visible + ")</p>");
            foreach (TicketMessage m in convo)
            {
                if (m.IsInternal && !showInternal) continue;
                string side = m.AuthorSide == "Support" ? "Destek" : m.AuthorSide == "Requester" ? "Talep sahibi" : "Sistem";
                string when = m.CreatedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm");
                if (m.AuthorSide == "System")
                {
                    sb.Append("<p style=\"margin:8px 0;color:#8A97AE;font-size:12px;line-height:1.5;\">" + when + " · " + E(m.Body) + "</p>");
                    continue;
                }
                string bg = m.IsInternal ? "#FFF7E6" : (m.AuthorSide == "Support" ? "#F2F0FF" : "#F6F8FC");
                string bd = m.IsInternal ? "#F3D9A4" : (m.AuthorSide == "Support" ? "#DDD6FF" : "#E6EBF3");
                sb.Append("<div style=\"margin:8px 0;background:" + bg + ";border:1px solid " + bd + ";border-radius:12px;padding:12px 14px;\">" +
                          "<div style=\"font-size:12px;color:#5E6A86;margin-bottom:6px;\"><b style=\"color:#1B2150;\">" + E(m.AuthorName ?? side) + "</b> · " + side +
                          (m.IsInternal ? " · <b style=\"color:#A86A0C;\">İÇ NOT</b>" : "") + " · " + when + "</div>" +
                          "<div style=\"color:#3A4A66;white-space:pre-wrap;font-size:13.5px;line-height:1.6;\">" + E(m.Body) + "</div></div>");
            }
        }
        sb.Append(Button(detailUrl, string.IsNullOrEmpty(btn) ? "Talebi Görüntüle" : btn));
        return Shell(headline, sb.ToString());
    }

    // Talep sahibine: talebiniz alindi
    private static string MsgBox(string label, string body)
    {
        if (string.IsNullOrEmpty(body)) return "";
        return "<p style=\"margin:18px 0 6px;color:#8A97AE;font-size:12px;font-weight:600;\">" + E(label) + "</p>" +
               "<div style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:14px 16px;color:#3A4A66;white-space:pre-wrap;font-size:13.5px;line-height:1.6;\">" + E(body) + "</div>";
    }
    private static string AttachNote(string names)
    {
        if (string.IsNullOrEmpty(names)) return "";
        return "<p style=\"margin:12px 0 0;font-size:12px;color:#41506E;\">&#128206; Ekli dosya(lar): <b>" + E(names) + "</b> · bu e-postaya eklenmiştir.</p>";
    }

    public static string TicketConfirm(string fullName, string ticketNo, string subject, string product, string module, string detailUrl, string body, string attachNames)
    {
        return TicketConfirm(fullName, ticketNo, subject, product, module, detailUrl, body, attachNames, "tr");
    }

    public static string TicketConfirm(string fullName, string ticketNo, string subject, string product, string module, string detailUrl, string body, string attachNames, string culture)
    {
        bool en = culture == "en";
        string inner =
"<p style=\"margin:0 0 6px;\">" + (en ? "Hello" : "Merhaba") + " <b style=\"color:#1B2440;\">" + E(fullName) + "</b>,</p>" +
"<p style=\"margin:0 0 18px;\">" + (en ? "Your support request has been received. Our team will get back to you shortly." : "Destek talebiniz alınmıştır. Ekibimiz en kısa sürede ilgilenecektir.") + "</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" +
Row(en ? "Ticket No" : "Talep No", ticketNo) + Row(en ? "Subject" : "Konu", subject) + Row(en ? "Product" : "Ürün", Fmt.Product(product)) + (Fmt.HasModule(module) ? Row(en ? "Module" : "Modül", module) : "") +
"</table>" +
MsgBox(en ? "Request Details" : "Talep İçeriği", body) + AttachNote(attachNames) +
Button(detailUrl, en ? "View Ticket" : "Talebi Görüntüle") +
"<p style=\"margin:14px 0 0;font-size:12px;color:#8A97AE;\">" + (en ? "You can reply from the portal to add to your request." : "Talebinize ekleme yapmak için portal üzerinden yanıt yazabilirsiniz.") + "</p>";
        return Shell(en ? "Your support request was received" : "Destek talebiniz alındı", inner);
    }

    // Yeni eğitim içeriği (e-posta) — Notice tabanlı, iki dilli
    public static string TrainingNew(string title, string category, string url, string culture)
    {
        bool en = culture == "en";
        string[] kv = new string[] { en ? "Content" : "İçerik", title, en ? "Category" : "Kategori", category };
        return Notice(
            en ? "New training content added" : "Yeni eğitim içeriği eklendi",
            en ? "New training content has been added to River Academy. Sign in to explore it." : "River Akademi'ye yeni bir eğitim içeriği eklendi. İncelemek için portala giriş yapın.",
            kv, url, en ? "Open River Academy" : "River Akademi'yi Aç");
    }

    // Destek ekibine: uzerine alinmamis talepler listesi (hafta ici gunluk)
    public static string UnassignedTickets(List<Ticket> list)
    {
        CultureInfo tr = new CultureInfo("tr-TR");
        StringBuilder rows = new StringBuilder();
        rows.Append("<tr>" +
            "<th style=\"text-align:left;padding:8px 10px;font-size:12px;color:#8A97AE;border-bottom:1px solid #E6EBF3;\">Talep No</th>" +
            "<th style=\"text-align:left;padding:8px 10px;font-size:12px;color:#8A97AE;border-bottom:1px solid #E6EBF3;\">Firma</th>" +
            "<th style=\"text-align:left;padding:8px 10px;font-size:12px;color:#8A97AE;border-bottom:1px solid #E6EBF3;\">Konu</th>" +
            "<th style=\"text-align:left;padding:8px 10px;font-size:12px;color:#8A97AE;border-bottom:1px solid #E6EBF3;\">Öncelik</th>" +
            "<th style=\"text-align:left;padding:8px 10px;font-size:12px;color:#8A97AE;border-bottom:1px solid #E6EBF3;\">Açılış</th></tr>");
        for (int i = 0; i < list.Count; i++)
        {
            Ticket t = list[i];
            rows.Append("<tr>" +
                "<td style=\"padding:9px 10px;font-size:13px;font-weight:600;color:#1B2440;border-bottom:1px solid #F0F3F8;\">" + E(t.TicketNo) + "</td>" +
                "<td style=\"padding:9px 10px;font-size:13px;color:#3A4A66;border-bottom:1px solid #F0F3F8;\">" + E(t.CompanyName) + "</td>" +
                "<td style=\"padding:9px 10px;font-size:13px;color:#3A4A66;border-bottom:1px solid #F0F3F8;\">" + E(t.Subject) + "</td>" +
                "<td style=\"padding:9px 10px;font-size:13px;color:#3A4A66;border-bottom:1px solid #F0F3F8;\">" + E(t.Priority) + "</td>" +
                "<td style=\"padding:9px 10px;font-size:12.5px;color:#8A97AE;border-bottom:1px solid #F0F3F8;\">" + t.CreatedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", tr) + "</td></tr>");
        }
        string inner =
"<p style=\"margin:0 0 6px;\">Günaydın,</p>" +
"<p style=\"margin:0 0 18px;\">Aşağıdaki <b style=\"color:#1B2440;\">" + list.Count + "</b> destek talebi henüz üstlenilmedi (\u201cÜzerime Al\u201d yapılmadı). Lütfen bugün en kısa sürede üstlenerek çözüm sürecini başlatın.</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border:1px solid #E6EBF3;border-radius:12px;border-collapse:separate;overflow:hidden;\">" +
rows.ToString() + "</table>" +
"<p style=\"margin:16px 0 0;font-size:12px;color:#8A97AE;\">Bu liste hafta içi her sabah otomatik olarak gönderilir.</p>";
        return Shell("Üstlenilmesi Beklenen Destek Talepleri", inner);
    }

    // Her gun 12:00: bekleyen destek talepleri ozeti (Cron.aspx?job=pending). Aciliyete gore gruplu kartlar:
    // 14+ gun, 7-13 gun, son bir hafta; ustte ozet kutulari. Turkiye saatine gore takvim gunu.
    public static string PendingTickets(List<Ticket> list, string baseUrl)
    {
        CultureInfo tr = new CultureInfo("tr-TR");
        DateTime now = DateTime.UtcNow.AddHours(3), today = now.Date;
        string dateTxt = today.ToString("d MMMM yyyy dddd", tr);
        if (list.Count == 0)
            return Shell("Bekleyen Destek Talepleri · " + today.ToString("d MMMM yyyy", tr),
                "<p style=\"margin:0 0 6px;\">Merhaba,</p><p style=\"margin:0;\">" + E(dateTxt) + ", saat 12:00 itibarıyla <b style=\"color:#1B2440;\">bekleyen destek talebi yok.</b> Tüm talepler çözüldü ya da kapatıldı.</p>" + PendingFoot(baseUrl));
        // en eski en ustte
        List<Ticket> s = new List<Ticket>(list);
        s.Sort(delegate (Ticket a, Ticket b) { return a.CreatedAt.CompareTo(b.CreatedAt); });
        int unassigned = 0, old = 0, high = 0;
        List<Ticket> g14 = new List<Ticket>(), g7 = new List<Ticket>(), g0 = new List<Ticket>();
        foreach (Ticket t in s)
        {
            int d = AgeDays(t, today);
            if (string.IsNullOrEmpty(t.AssignedToName)) unassigned++;
            if (d >= 7) old++;
            if (t.Priority == "High" || t.Priority == "Critical") high++;
            if (d >= 14) g14.Add(t); else if (d >= 7) g7.Add(t); else g0.Add(t);
        }
        string inner =
"<p style=\"margin:0 0 6px;\">Merhaba,</p>" +
"<p style=\"margin:0 0 16px;\">" + E(dateTxt) + ", saat 12:00 itibarıyla <b style=\"color:#1B2440;\">" + list.Count + "</b> destek talebi çözüm bekliyor.</p>" +
"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:0 0 6px;\"><tr>" +
PendStat(list.Count, "Bekleyen talep", "#1B2440") + PendStat(unassigned, "Atanmamış", "#B4600C") + PendStat(old, "7 gün ve üzeri", "#C42056") + PendStat(high, "Kritik / Yüksek", "#1B2440") +
"</tr></table>" +
PendGroup("Uzun süredir bekleyenler", "14 gün ve üzeri", "#C42056", g14, today, baseUrl) +
PendGroup("Dikkat", "7 ile 13 gün arası", "#B4600C", g7, today, baseUrl) +
PendGroup("Son bir hafta", "0 ile 6 gün", "#1B2440", g0, today, baseUrl) +
PendingFoot(baseUrl);
        return Shell("Bekleyen Destek Talepleri · " + today.ToString("d MMMM yyyy", tr), inner);
    }

    private static int AgeDays(Ticket t, DateTime todayTr)
    { return Math.Max(0, (todayTr - DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc).AddHours(3).Date).Days); }

    private static string PendingFoot(string baseUrl)
    {
        return "<p style=\"margin:16px 0 0;font-size:12px;color:#8A97AE;\">Bu özet hafta içi her gün saat 12:00'de otomatik gönderilir. Tüm liste: <a href=\"" + E(baseUrl) + "/Tickets.aspx\" style=\"color:#4F46E5;\">Destek Kuyruğu</a></p>";
    }

    private static string PendStat(int v, string label, string color)
    {
        return "<td width=\"25%\" style=\"padding:0 4px 0 0;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:10px;\"><tr><td style=\"padding:10px 12px;\">" +
               "<div style=\"font-size:22px;font-weight:800;color:" + color + ";line-height:1.1;\">" + v + "</div><div style=\"font-size:11.5px;color:#6B7895;margin-top:2px;\">" + E(label) + "</div></td></tr></table></td>";
    }

    private static string PendPill(string text, string color, string bg)
    { return "<span style=\"display:inline-block;padding:2px 9px;border-radius:999px;font-size:11.5px;font-weight:700;color:" + color + ";background:" + bg + ";white-space:nowrap;\">" + E(text) + "</span>"; }

    private static string PendGroup(string title, string sub, string color, List<Ticket> items, DateTime today, string baseUrl)
    {
        if (items.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        sb.Append("<div style=\"margin:18px 0 8px;font-size:13px;font-weight:800;color:" + color + ";\">" + E(title) + " <span style=\"font-weight:600;color:#8A97AE;\">· " + E(sub) + " · " + items.Count + " talep</span></div>");
        foreach (Ticket t in items)
        {
            int d = AgeDays(t, today);
            string ac = d >= 14 ? "#C42056" : d >= 7 ? "#B4600C" : "#5B6788", abg = d >= 14 ? "#FDE8EF" : d >= 7 ? "#FDF1E4" : "#EEF1F7";
            string badge = d == 0
                ? "<div style=\"font-size:12.5px;font-weight:800;line-height:1.2;padding:5px 0;\">Bugün</div>"
                : "<div style=\"font-size:18px;font-weight:800;line-height:1;\">" + d + "</div><div style=\"font-size:10px;font-weight:700;margin-top:2px;\">gün</div>";
            string pr = t.Priority ?? "Normal";
            string prT = pr == "Critical" ? "Kritik" : pr == "High" ? "Yüksek" : pr == "Low" ? "Düşük" : "Normal";
            string prC = pr == "Critical" ? "#C42056" : pr == "High" ? "#B4600C" : pr == "Low" ? "#5B6788" : "#1E6FD9";
            string prB = pr == "Critical" ? "#FDE8EF" : pr == "High" ? "#FDF1E4" : pr == "Low" ? "#EEF1F7" : "#E8F1FD";
            bool blue = t.Status == "InProgress";
            string url = E(baseUrl) + "/TicketDetail.aspx?id=" + t.TicketId;
            bool un = string.IsNullOrEmpty(t.AssignedToName);
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:0 0 8px;border:1px solid #E6EBF3;border-radius:12px;border-collapse:separate;\"><tr>" +
                "<td width=\"62\" valign=\"top\" style=\"padding:12px 0 12px 12px;\"><div style=\"width:52px;padding:7px 0;border-radius:10px;text-align:center;background:" + abg + ";color:" + ac + ";\">" + badge + "</div></td>" +
                "<td valign=\"top\" style=\"padding:11px 12px;\"><div style=\"font-size:14px;font-weight:700;color:#1B2440;line-height:1.35;\">" + E(t.Subject) + "</div>" +
                "<div style=\"font-size:12px;color:#6B7895;margin-top:3px;\"><a href=\"" + url + "\" style=\"color:#4F46E5;font-weight:700;text-decoration:none;font-family:Consolas,monospace;\">" + E(t.TicketNo) + "</a> · " + E(t.CompanyName) + "</div>" +
                "<div style=\"margin-top:7px;\">" + PendPill(prT, prC, prB) + " " + PendPill(TicketFmt.Display(t, true), blue ? "#1E6FD9" : "#B4600C", blue ? "#E8F1FD" : "#FDF1E4") +
                " <span style=\"font-size:12px;color:" + (un ? "#B4600C" : "#6B7895") + ";white-space:nowrap;\">· " + E(un ? "Atanmamış" : t.AssignedToName) + "</span></div></td></tr></table>");
        }
        return sb.ToString();
    }

    // Destek ekibine: yeni talep
    public static string TicketNotify(string ticketNo, string company, string subject, string product, string module, string requestType, string priority, string version, string detailUrl, string body, string attachNames)
    {
        return TicketNotify(ticketNo, company, null, null, null, null, null, subject, product, module, requestType, priority, version, detailUrl, body, attachNames);
    }
    /// <summary>Yeni talep bildirimi (destek ekibine): firmanin yaninda talebi giren kisi, e-postasi, telefonu ve (is ortagi baglamissa) musterisi</summary>
    public static string TicketNotify(string ticketNo, string company, string requesterName, string requesterEmail, string requesterPhone, string customerName, string environment,
                                      string subject, string product, string module, string requestType, string priority, string version, string detailUrl, string body, string attachNames)
    {
        string who = "";
        if (!string.IsNullOrEmpty(requesterName)) who += Row("Talep Sahibi", requesterName);
        if (!string.IsNullOrEmpty(requesterEmail)) who += Row("E-posta", requesterEmail);
        if (!string.IsNullOrEmpty(requesterPhone)) who += Row("Telefon", requesterPhone);
        if (!string.IsNullOrEmpty(customerName)) who += Row("Müşteri", customerName);
        string ver = string.IsNullOrEmpty(version) ? version : version + (string.IsNullOrEmpty(environment) ? "" : " · " + environment);
        string inner =
"<p style=\"margin:0 0 18px;\">Yeni bir destek talebi oluşturuldu.</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" +
Row("Talep No", ticketNo) + Row("Firma", company) + who + Row("Konu", subject) +
Row(Fmt.HasModule(module) ? "Ürün / Modül" : "Ürün", Fmt.ProdMod(product, module, " / ")) + Row("Tip", requestType) + Row("Öncelik", priority) + Row("Versiyon", ver) +
"</table>" +
MsgBox("Talep İçeriği", body) + AttachNote(attachNames) +
Button(detailUrl, "Talebi Aç") +
"<p style=\"margin:14px 0 0;font-size:12px;color:#8A97AE;\">Talep havuzda bekliyor; üstlenmek için 'Üzerime al' diyebilirsiniz.</p>";
        return Shell("Yeni destek talebi", inner);
    }

    // Şifre yenileme e-postası
    public static string PasswordReset(string fullName, string email, string newPassword, string loginUrl)
    {
        string inner =
"<p style=\"margin:0 0 6px;\">Merhaba <b style=\"color:#1B2440;\">" + E(fullName) + "</b>,</p>" +
"<p style=\"margin:0 0 18px;\">Hesabınızın şifresi yenilenmiştir. Yeni giriş bilgileriniz:</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" +
Row("E-posta", email) +
Row("Yeni parola", newPassword) +
"</table>" +
Button(loginUrl, "Portala Giriş Yap") +
"<p style=\"margin:14px 0 0;font-size:12px;color:#8A97AE;\">Bu talebi siz yapmadıysanız lütfen yöneticinizle iletişime geçin.</p>";
        return Shell("Şifreniz yenilendi", inner);
    }

    // Esnek bildirim sablonu: baslik + giris + anahtar/deger satirlari + opsiyonel buton
    // ===================== TOPLU BILDIRIM TEMALARI =====================
    // theme: duyuru | yenilik | bakim | etkinlik | kritik
    // f1/f2/f3: temaya ozel alanlar (yenilik: alt baslik | bakim: tarih,saat,etkilenen | etkinlik: tarih(yyyy-MM-dd),saat,yer | kritik: uyari bandi)
    public static string Broadcast(string theme, string title, string body, string btnText, string btnUrl, string f1, string f2, string f3, bool preview)
    {
        string accent = "#4F46E5", inner = "";
        string ttl = E(title ?? "");
        string bodyHtml = BBody(body);
        string btn = (!string.IsNullOrEmpty(btnText) && !string.IsNullOrEmpty(btnUrl)) ? "{BTN}" : "";

        if (theme == "yenilik")
        {
            accent = "#7A3BEA";
            inner =
"<tr><td bgcolor=\"#6D5DF6\" style=\"padding:34px 32px;background:#6D5DF6;background:linear-gradient(135deg,#4F46E5 0%,#7A3BEA 55%,#22B8D8 100%);\">" +
BBadge("Yeni", "#FFFFFF", "rgba(255,255,255,.18)", "rgba(255,255,255,.4)") +
"<h2 style=\"margin:14px 0 8px;font-size:23px;line-height:1.3;color:#FFFFFF;font-weight:800;\">" + ttl + "</h2>" +
(string.IsNullOrEmpty(f1) ? "" : "<p style=\"margin:0;font-size:14px;line-height:1.7;color:#E8ECFF;\">" + E(f1) + "</p>") +
"</td></tr><tr><td style=\"padding:26px 32px;\">" + bodyHtml + btn + "</td></tr>";
        }
        else if (theme == "bakim")
        {
            accent = "#D99A1E";
            string kv = "";
            if (!string.IsNullOrEmpty(f1)) kv += BKv("Tarih", f1);
            if (!string.IsNullOrEmpty(f2)) kv += BKv("Saat", f2);
            if (!string.IsNullOrEmpty(f3)) kv += BKv("Etkilenen", f3);
            inner = "<tr><td style=\"padding:30px 32px;\">" + BBadge("Bilgilendirme", "#9A6A0E", "#FBF3E3", "#EAD9AE") + BH(ttl) + bodyHtml +
                (kv.Length > 0 ? "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#FFF9EE;border:1px solid #F1E2BF;border-radius:12px;padding:6px 18px;margin:6px 0 4px;\">" + kv + "</table>" : "") +
                btn + "</td></tr>";
        }
        else if (theme == "etkinlik")
        {
            accent = "#12A150";
            string day = "", mon = "", wd = "";
            DateTime dt;
            if (!string.IsNullOrEmpty(f1) && DateTime.TryParse(f1, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            {
                System.Globalization.CultureInfo tr = new System.Globalization.CultureInfo("tr-TR");
                day = dt.Day.ToString(); mon = dt.ToString("MMMM", tr); wd = dt.ToString("dddd", tr);
            }
            string when = (wd.Length > 0 ? wd : "") + (string.IsNullOrEmpty(f2) ? "" : (wd.Length > 0 ? ", " : "") + E(f2));
            string cal = day.Length == 0 ? "" :
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:4px 0 16px;\"><tr>" +
"<td bgcolor=\"#12A150\" style=\"background:#12A150;border-radius:12px;padding:10px 16px;text-align:center;color:#FFFFFF;\"><div style=\"font-size:22px;font-weight:800;line-height:1;\">" + day + "</div><div style=\"font-size:11px;letter-spacing:.1em;text-transform:uppercase;margin-top:4px;\">" + E(mon) + "</div></td>" +
"<td style=\"padding-left:16px;font-size:13.5px;color:#3A4A66;line-height:1.6;\"><b style=\"color:#1B2440\">" + when + "</b>" + (string.IsNullOrEmpty(f3) ? "" : "<br/>" + E(f3)) + "</td></tr></table>";
            inner = "<tr><td style=\"padding:30px 32px;\">" + BBadge("Davet", "#0E7A3E", "#E7F6EE", "#CBEBD8") + BH(ttl) + cal + bodyHtml + btn + "</td></tr>";
        }
        else if (theme == "kritik")
        {
            accent = "#E4467A";
            string warn = string.IsNullOrEmpty(f1) ? "Bu bildirim acil işlem gerektirmektedir." : f1;
            inner = "<tr><td style=\"padding:30px 32px;\">" +
"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#FDECF2;border:1px solid #F6C4D5;border-radius:12px;\"><tr><td style=\"padding:14px 18px;font-size:13.5px;color:#9B1F4A;line-height:1.6;\"><b>Önemli:</b> " + E(warn) + "</td></tr></table>" +
                BH(ttl) + bodyHtml + btn + "</td></tr>";
        }
        else
        {
            accent = "#4F46E5";
            inner = "<tr><td style=\"padding:30px 32px;\">" + BBadge("Duyuru", "#4F46E5", "#EEF0FE", "#D5D8FB") + BH(ttl) + bodyHtml + btn + "</td></tr>";
        }

        if (btn.Length > 0)
            inner = inner.Replace("{BTN}",
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:22px 0 4px;\"><tr><td bgcolor=\"" + accent + "\" style=\"background:" + accent + ";border-radius:10px;\">" +
"<a href=\"" + E(btnUrl) + "\" style=\"display:inline-block;padding:12px 26px;color:#FFFFFF;text-decoration:none;font-weight:700;font-size:14px;\">" + E(btnText) + "</a></td></tr></table>");

        string rl = preview ? "assets/RIVER_AI_black.png" : "cid:riverlogo";
        string fl = preview ? "assets/fol_dark.png" : "cid:follogo";
        return
"<div style=\"margin:0;padding:0;background:#EDF1F7;\">" +
"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#EDF1F7;padding:32px 0;\"><tr><td align=\"center\">" +
"<table role=\"presentation\" width=\"560\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:560px;max-width:94%;background:#FFFFFF;border:1px solid #E3E8F0;border-radius:18px;overflow:hidden;font-family:Segoe UI,Arial,sans-serif;box-shadow:0 10px 30px rgba(20,30,60,.08);\">" +
"<tr><td style=\"padding:26px 32px 20px;background:#FFFFFF;border-bottom:1px solid #EEF1F6;\">" +
"<img src=\"" + rl + "\" alt=\"River AI\" height=\"30\" style=\"display:block;border:0;height:30px;\" />" +
"<span style=\"display:block;font-size:11px;letter-spacing:.22em;text-transform:uppercase;color:#8A97AE;margin-top:10px;\">Asistan Portal</span></td></tr>" +
"<tr><td bgcolor=\"" + accent + "\" style=\"height:3px;background:" + accent + ";line-height:3px;font-size:0;\">&nbsp;</td></tr>" +
inner +
"<tr><td style=\"padding:22px 32px;background:#F5F7FB;border-top:1px solid #EEF1F6;text-align:center;\">" +
"<img src=\"" + fl + "\" alt=\"Fol Yazılım\" height=\"20\" style=\"height:20px;border:0;\" />" +
"<div style=\"font-size:12.5px;font-weight:600;color:#41506E;margin-top:8px;\">Fol Yazılım A.Ş.</div>" +
"<div style=\"font-size:11px;color:#9AA6BC;margin-top:4px;\">Bu e-posta River Asistan Portal tarafından gönderilmiştir.</div></td></tr>" +
"</table></td></tr></table></div>";
    }

    private static string BBadge(string t, string c, string bg, string br)
    {
        return "<span style=\"display:inline-block;font-size:11px;font-weight:700;letter-spacing:.12em;text-transform:uppercase;color:" + c + ";background:" + bg + ";border:1px solid " + br + ";border-radius:999px;padding:5px 12px;\">" + t + "</span>";
    }

    private static string BH(string encTitle)
    {
        return "<h2 style=\"margin:14px 0 14px;font-size:21px;line-height:1.3;color:#1B2440;font-weight:800;\">" + encTitle + "</h2>";
    }

    private static string BKv(string k, string v)
    {
        return "<tr><td style=\"padding:8px 0;font-size:13px;color:#6B7A99;\">" + k + "</td><td style=\"padding:8px 0;font-size:13px;color:#1B2440;font-weight:600;text-align:right;\">" + E(v) + "</td></tr>";
    }

    // Govde: bos satir = yeni paragraf, "- " / "• " ile baslayan satirlar = madde listesi
    private static string BBody(string body)
    {
        if (string.IsNullOrEmpty(body)) return "";
        string[] lines = body.Replace("\r", "").Split('\n');
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        System.Text.StringBuilder para = new System.Text.StringBuilder();
        bool inList = false;
        string pOpen = "<p style=\"margin:0 0 12px;font-size:14px;line-height:1.75;color:#3A4A66;\">";
        for (int i = 0; i <= lines.Length; i++)
        {
            string ln = i < lines.Length ? lines[i].TrimEnd() : "";
            bool end = i == lines.Length;
            string tl = ln.TrimStart();
            bool isItem = !end && (tl.StartsWith("- ") || tl.StartsWith("• "));
            if (isItem)
            {
                if (para.Length > 0) { sb.Append(pOpen + para.ToString() + "</p>"); para.Length = 0; }
                if (!inList) { sb.Append("<ul style=\"margin:0 0 12px;padding-left:18px;color:#3A4A66;font-size:14px;line-height:1.9;\">"); inList = true; }
                sb.Append("<li>" + E(tl.Substring(2).Trim()) + "</li>");
                continue;
            }
            if (inList) { sb.Append("</ul>"); inList = false; }
            if (end || tl.Length == 0)
            {
                if (para.Length > 0) { sb.Append(pOpen + para.ToString() + "</p>"); para.Length = 0; }
                continue;
            }
            if (para.Length > 0) para.Append("<br/>");
            para.Append(E(ln));
        }
        return sb.ToString();
    }

    // Dosya paylasimi: aliciya kisiye ozel indirme linki
    public static string TransferMail(string senderName, string title, string message, List<string> fileNames, List<string> fileSizes,
                                      string totalSize, string expiresStr, bool hasPassword, string url)
    {
        StringBuilder files = new StringBuilder();
        int show = Math.Min(fileNames.Count, 8);
        for (int i = 0; i < show; i++)
            files.Append("<tr><td style=\"padding:9px 0;border-top:1px solid #EEF1F6;font-size:13px;color:#1B2440;\">&#128196;&nbsp; " + E(fileNames[i]) + "</td>" +
                "<td style=\"padding:9px 0;border-top:1px solid #EEF1F6;font-size:12.5px;color:#6B7A99;text-align:right;white-space:nowrap;\">" + E(fileSizes[i]) + "</td></tr>");
        if (fileNames.Count > show)
            files.Append("<tr><td colspan=\"2\" style=\"padding:9px 0;border-top:1px solid #EEF1F6;font-size:12.5px;color:#6B7A99;\">+ " + (fileNames.Count - show) + " dosya daha</td></tr>");

        string msg = string.IsNullOrEmpty(message) ? "" :
            "<div style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-left:3px solid #4F46E5;border-radius:10px;padding:13px 16px;margin:0 0 18px;font-size:13.5px;line-height:1.65;color:#2A3550;\">" +
            E(message).Replace("\r\n", "<br>").Replace("\n", "<br>") + "</div>";

        string inner =
"<p style=\"margin:0 0 6px;font-size:13px;color:#6B7A99;\"><b style=\"color:#1B2440;\">" + E(senderName) + "</b> sizinle dosya paylaştı</p>" +
"<h2 style=\"margin:0 0 16px;font-size:21px;line-height:1.3;color:#1B2440;font-weight:800;\">" + E(title) + "</h2>" +
msg +
"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:0 0 6px;\">" + files.ToString() + "</table>" +
"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:linear-gradient(135deg,#EEF0FE,#FFFFFF 80%);border:1px solid #E4E8F5;border-radius:12px;padding:6px 18px;margin-top:12px;\">" +
Row("Toplam", fileNames.Count + " dosya · " + totalSize) + Row("Son indirme tarihi", expiresStr) + (hasPassword ? Row("Koruma", "Şifreli · şifre size ayrıca iletilecektir") : "") +
"</table>" +
Button(url, "Dosyaları İndir") +
"<p style=\"margin:14px 0 0;font-size:11.5px;color:#9AA6BC;line-height:1.6;\">Bu bağlantı size özeldir; lütfen başkalarıyla paylaşmayın. Bağlantı son indirme tarihinden sonra geçersiz olur ve dosyalar silinir.</p>";
        return Shell("Dosya Paylaşımı", inner);
    }

    // Hizmet bildirimi (Hizmet Formu -> musteri kisilerine)
    public static string ServiceMail(string companyName, string content, string sureLabel, string balanceLabel, string url, string headline)
    {
        string body = E(content).Replace("\r\n", "<br>").Replace("\n", "<br>");
        string inner =
"<p style=\"margin:0 0 14px;\">Aşağıda tarafınıza sunulan hizmetin ayrıntıları yer almaktadır.</p>" +
"<div style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:14px 16px;font-size:13.5px;line-height:1.65;color:#2A3550;\">" + body + "</div>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:linear-gradient(135deg,#EEF0FE,#FFFFFF 80%);border:1px solid #E4E8F5;border-radius:12px;padding:6px 18px;margin-top:12px;\">" +
Row("Firma", companyName) + Row("Harcanan Süre", sureLabel) + (string.IsNullOrEmpty(balanceLabel) ? "" : Row("Kalan Bakiye", balanceLabel)) +
"</table>" +
Button(url, "Portala Giriş Yap");
        return Shell(string.IsNullOrEmpty(headline) ? "Hizmet Bildirimi" : headline, inner);
    }

    public static string Notice(string title, string intro, string[] kv, string url, string btn)
    {
        StringBuilder rows = new StringBuilder();
        if (kv != null)
            for (int i = 0; i + 1 < kv.Length; i += 2) rows.Append(Row(kv[i], kv[i + 1]));

        StringBuilder inner = new StringBuilder();
        inner.Append("<p style=\"margin:0 0 16px;\">" + E(intro) + "</p>");
        if (rows.Length > 0)
            inner.Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" +
                         rows.ToString() + "</table>");
        if (!string.IsNullOrEmpty(url)) inner.Append(Button(url, btn));
        return Shell(title, inner.ToString());
    }

    // Yeni kullanici hos geldin e-postasi
    public static string Welcome(string fullName, string email, string password, string companyName, string loginUrl)
    {
        return Welcome(fullName, email, password, companyName, loginUrl, "partner", "tr");
    }

    private static string TintBg(string hex)
    {
        switch (hex)
        {
            case "#4F46E5": return "#EEF0FE";
            case "#168CFF": return "#EAF4FF";
            case "#22B8D8": return "#E7F7FB";
            case "#C08A1E": return "#FBF3E3";
            case "#7A3BEA": return "#F1EBFE";
            case "#12A150": return "#E7F6EE";
            case "#C019E6": return "#FBEAFE";
            default: return "#E5F5F3"; // #0E9488 teal
        }
    }

    private static string ModCard(string color, string title, string desc)
    {
        return "<tr><td style=\"padding:7px 0;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:linear-gradient(135deg," + TintBg(color) + " 0%,#FFFFFF 78%);border:1px solid #E7ECF5;border-radius:14px;\">" +
            "<tr><td width=\"46\" valign=\"top\" style=\"padding:14px 0 14px 16px;\"><div style=\"width:30px;height:30px;border-radius:9px;background:" + color + ";\"></div></td>" +
            "<td style=\"padding:13px 16px 13px 12px;\"><div style=\"font-size:14.5px;font-weight:700;color:#16213C;\">" + E(title) + "</div>" +
            "<div style=\"font-size:12.7px;line-height:1.55;color:#51607C;margin-top:3px;\">" + E(desc) + "</div></td></tr></table></td></tr>";
    }

    // roleKey: partner / customer / vendor / editor / support
    private static string WelcomeModules(string roleKey, bool en)
    {
        string[][] m;
        if (roleKey == "customer")
            m = new string[][] {
                new string[]{"#12A150","Taleplerim","My Requests","Destek taleplerinizi açın ve takip edin; ürün/modül seçip dosya ekleyebilir, ekiple mesajlaşabilirsiniz.","Open and track support requests; choose product/module, attach files, and message the team."},
                new string[]{"#C019E6","River Akademi","River Academy","Ürün eğitim videolarını izleyin, dokümanları inceleyin; arama ve kategorilerle hızlıca erişin.","Watch product training videos and browse documents; find content fast with search and categories."},
                new string[]{"#0E9488","Versiyon Duyuruları","Version Announcements","Kullandığınız ürünlerin yeni sürümlerini ve yeniliklerini buradan öğrenin.","See new versions and highlights of the products you use."} };
        else if (roleKey == "vendor")
            m = new string[][] {
                new string[]{"#4F46E5","Genel Bakış","Overview","Partner performansını, anlaşmaları ve özet grafikleri tek ekranda görün.","See partner performance, deals and summary charts in one place."},
                new string[]{"#168CFF","Partnerler","Partners","Yeni bayi/iş ortağı oluşturun ve mevcut partnerleri yönetin.","Create new partners and manage existing ones."},
                new string[]{"#22B8D8","Demo Onayları","Demo Approvals","İş ortaklarından gelen demo taleplerini inceleyip onaylayın.","Review and approve demo requests from partners."},
                new string[]{"#C08A1E","Kullanıcılar","Users","Kullanıcı ekleyin; rol ve yetkilerini (çoklu yetki) düzenleyin.","Add users; edit their roles and permissions (multi-permission)."},
                new string[]{"#7A3BEA","Satış Materyalleri","Sales Materials","Partnerlerle paylaşılacak sunum, broşür ve dokümanları yükleyin ve yönetin.","Upload and manage presentations, brochures and documents for partners."},
                new string[]{"#12A150","Destek Kuyruğu","Support Queue","Talepleri üstlenin, yanıtlayın, dahili not düşün ve çözüme kavuşturun.","Take, reply, add internal notes and resolve tickets."},
                new string[]{"#C019E6","River Akademi","River Academy","Eğitim içeriklerini yönetin: video/doküman ekleyin, kategori oluşturun, öne çıkarın.","Manage training content: add videos/documents, create categories, feature items."},
                new string[]{"#0E9488","Versiyon Duyuruları","Version Announcements","Yeni sürüm duyurusu yayınlayın; herkese bildirim ve e-posta gider.","Publish version announcements; everyone gets an in-app notice and email."} };
        else if (roleKey == "editor")
            m = new string[][] {
                new string[]{"#C019E6","River Akademi","River Academy","Eğitim içeriğini yönetin: video ve doküman ekleyin, kategori oluşturun, başlık/açıklama düzenleyin, öne çıkarın.","Manage training: add videos/documents, create categories, edit titles/descriptions, feature items."},
                new string[]{"#0E9488","Versiyon Duyuruları","Version Announcements","Yeni sürüm duyurularını hazırlayıp yayınlayın.","Prepare and publish version announcements."} };
        else if (roleKey == "support")
            m = new string[][] {
                new string[]{"#12A150","Destek Kuyruğu","Support Queue","Havuzdaki talepleri üstlenin ve müşteriyle yazışarak çözüme kavuşturun. Dahili notlar yalnız üretici ekibine görünür.","Take pooled tickets and resolve them with the customer. Internal notes stay within your team."},
                new string[]{"#C019E6","River Akademi","River Academy","Eğitim içeriklerini izleyerek ürün bilginizi güncel tutun.","Keep your product knowledge current by watching training content."},
                new string[]{"#0E9488","Versiyon Duyuruları","Version Announcements","Yeni sürümleri ve yenilikleri takip edin.","Follow new versions and highlights."} };
        else // partner
            m = new string[][] {
                new string[]{"#4F46E5","Panel","Dashboard","Yıllık cironuzu, güncel partner seviyenizi ve bir üst seviyeye kalan tutarı tek bakışta görün.","See your annual revenue, current tier and how much is left to the next tier at a glance."},
                new string[]{"#168CFF","Siparişler","Orders","Anlaşmalarınızı ve hak ediş tutarlarınızı takip edin; komisyon ve primler otomatik hesaplanır.","Track your deals and commissions; commission and bonuses are calculated automatically."},
                new string[]{"#22B8D8","Demo Talepleri","Demo Requests","Müşterileriniz için demo talebi oluşturun; onay ve planlama sürecini izleyin. Kayıt 30 gün korunur.","Create demo requests for your customers and follow approval/planning. Registration is protected for 30 days."},
                new string[]{"#C08A1E","Partner Seviyeleri","Partner Tiers","Seviye komisyon oranlarını, avantajları ve şartları görün; ciro arttıkça seviyeniz otomatik yükselir.","See tier commission rates, benefits and terms; your tier rises automatically as revenue grows."},
                new string[]{"#7A3BEA","Satış Materyalleri","Sales Materials","Sunum, broşür ve teknik dokümanları tek tıkla indirin; her zaman en güncel sürüme erişin.","Download presentations, brochures and technical docs in one click; always the latest version."},
                new string[]{"#12A150","Destek","Support","Talep açın, konuşma geçmişini görün, dosya ekleyin ve çözümü onaylayın.","Open tickets, view the conversation, attach files and confirm the resolution."},
                new string[]{"#C019E6","River Akademi","River Academy","Video ve dokümanlarla ekibinizi geliştirin; kaldığınız yerden devam edin, ilerlemenizi takip edin.","Grow your team with videos and documents; resume where you left off and track progress."},
                new string[]{"#0E9488","Versiyon Duyuruları","Version Announcements","Ürünlerdeki yeni sürümleri, yenilikleri ve erişim tarihlerini takip edin.","Follow new versions, highlights and access dates."} };

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < m.Length; i++) sb.Append(ModCard(m[i][0], en ? m[i][2] : m[i][1], en ? m[i][4] : m[i][3]));
        return sb.ToString();
    }

    public static string Welcome(string fullName, string email, string password, string companyName, string loginUrl, string roleKey, string culture)
    {
        bool en = culture == "en";
        string inner =
"<p style=\"margin:0 0 6px;\">" + (en ? "Hello" : "Merhaba") + " <b style=\"color:#1B2440;\">" + E(fullName) + "</b>,</p>" +
"<p style=\"margin:0 0 18px;\">" + (en ? "An account has been created for you on the portal. You can sign in with the details below and start using the sections we prepared for you." : "Portalda sizin için bir hesap oluşturuldu. Aşağıdaki bilgilerle giriş yapabilir, sizin için hazırladığımız bölümleri kullanmaya hemen başlayabilirsiniz.") + "</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:linear-gradient(135deg,#EEF0FE,#FFFFFF 80%);border:1px solid #E4E8F5;border-radius:14px;padding:6px 18px;\">" +
Row(en ? "Email" : "E-posta", email) +
Row(en ? "Company" : "Firma Adı", companyName) +
"</table>" +
"<div style=\"margin:16px 0 5px;font-size:12px;color:#8A97AE;\">" + (en ? "Temporary password" : "Geçici Parola") + "</div>" +
"<div style=\"font-family:Consolas,Menlo,monospace;font-size:17px;font-weight:700;color:#16213C;background:#F3F5FB;border:1px dashed #C6CEE4;border-radius:10px;padding:13px 16px;letter-spacing:.5px;-webkit-user-select:all;user-select:all;\">" + E(password) + "</div>" +
"<div style=\"font-size:11px;color:#8A97AE;margin:5px 0 0;\">" + (en ? "Tap/select the password to copy it." : "Parolayı seçip kopyalayabilirsiniz.") + "</div>" +
Button(loginUrl, en ? "Sign in to the Portal" : "Portala Giriş Yap") +
"<div style=\"margin:22px 0 10px;font-size:13px;font-weight:800;letter-spacing:.02em;color:#14203A;text-transform:uppercase;\">" + (en ? "What you can do in the portal" : "Portalda neler yapabilirsiniz?") + "</div>" +
"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">" + WelcomeModules(roleKey, en) + "</table>";
        return Shell(en ? "Welcome to River Assistant Portal" : "River Asistan Portal\u2019a hoş geldiniz", inner);
    }

    // Demo onay e-postasi
    public static string DemoApproved(string toName, string companyName, string client, string module, string scheduled, string loginUrl)
    {
        string inner =
"<p style=\"margin:0 0 6px;\">Merhaba <b style=\"color:#1B2440;\">" + E(toName) + "</b>,</p>" +
"<p style=\"margin:0 0 18px;\">Aşağıdaki demo talebiniz <b style=\"color:#34E1B5;\">onaylandı</b>.</p>" +
"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#F6F8FC;border:1px solid #E6EBF3;border-radius:12px;padding:6px 18px;\">" +
Row("Firma", companyName) +
Row("Müşteri", client) +
Row("Ürün modülü", module) +
Row("Planlanan tarih", scheduled) +
"</table>" +
Button(loginUrl, "Portalda Görüntüle");
        return Shell("Demo talebiniz onaylandı", inner);
    }
}
