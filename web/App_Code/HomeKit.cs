using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

// Ana sayfa panelleri (bayi yoneticisi / musteri + bayi personeli / Fol personeli) icin ortak parcalar:
// karsilama profil resmi, "Yeni neler var?" akisi, bos durum kutusu, "Bugun icin" satiri, gosterge karti, zaman ifadesi.
public static class HomeKit
{
    public static readonly CultureInfo TRC = new CultureInfo("tr-TR");
    public const string ARROW = "<svg width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2.2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M5 12h14M13 6l6 6-6 6\"/></svg>";
    public static string H(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

    /// <summary>Karsilama profil resmi: yuklu resim ya da bas harfler; tiklaninca profil resmi penceresi acilir (js: data-hero-ava)</summary>
    public static string HeroAvatar(int userId, string name, bool en)
    {
        string img = "";
        try { UserRow u = Repo.GetUserById(userId); if (u != null) img = u.AvatarImage ?? ""; } catch { }
        string inner = img.Length > 0 ? "<img src=\"" + H(img) + "\" alt=\"\" />" : H(Fmt.Initials(name));
        return "<button type=\"button\" class=\"pa-ava\" data-hero-ava=\"1\" title=\"" + (en ? "Change profile picture" : "Profil resmini değiştir") + "\" aria-label=\"" + (en ? "Change profile picture" : "Profil resmini değiştir") + "\">" +
               "<span class=\"" + (img.Length > 0 ? "img" : "ini") + "\">" + inner + "</span></button>";
    }

    public static string Empty(string title, string text, string cta, string href, string icon)
    {
        return "<div class=\"cu-empty\"><span class=\"cu-empty-ic\"><svg width=\"22\" height=\"22\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + icon + "\"/></svg></span>" +
               "<b>" + H(title) + "</b><span>" + H(text) + "</span>" + (cta != null ? "<a class=\"btn-ghost\" href=\"" + href + "\">" + H(cta) + "</a>" : "") + "</div>";
    }

    public static string TodoRow(string tone, string icon, string title, string sub, string href)
    {
        string inner = "<span class=\"cu-ic " + tone + "\"><svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + icon + "\"/></svg></span>" +
                       "<span class=\"cu-todo-tx\"><b>" + H(title) + "</b><em>" + H(sub) + "</em></span>";
        return href == null ? "<div class=\"cu-todo-it\">" + inner + "</div>" : "<a class=\"cu-todo-it lk\" href=\"" + href + "\">" + inner + "<span class=\"ph-go\">" + ARROW + "</span></a>";
    }

    public static string Kpi(string label, string value, string note, string tone, string dot, string target, string href)
    {
        return "<a class=\"card ph-kpi cu-kpi lk\" href=\"" + href + "\" aria-label=\"" + H(label) + ": " + H(value) + ". " + H(target) + "\">" +
               "<span class=\"ph-kpi-h\"><span>" + H(label) + "</span><i style=\"background:" + dot + "\"></i></span>" +
               "<b>" + value + "</b><small class=\"" + tone + " ph-kpi-next\">" + H(note) + "</small>" +
               "<span class=\"ph-kpi-go\">" + H(target) + ARROW + "</span></a>";
    }

    public static string When(DateTime d, DateTime nowTr, bool en)
    {
        TimeSpan ago = nowTr - d; int days = (int)(nowTr.Date - d.Date).TotalDays;
        if (ago.TotalMinutes < 60) return ago.TotalMinutes < 2 ? (en ? "Just now" : "Az önce") : (int)ago.TotalMinutes + (en ? " min ago" : " dk önce");
        if (days <= 0) return (int)ago.TotalHours + (en ? " hours ago" : " saat önce");
        if (days == 1) return en ? "Yesterday" : "Dün";
        return days < 7 ? days + (en ? " days ago" : " gün önce") : d.ToString(en ? "MMM d" : "d MMMM", en ? new CultureInfo("en-US") : TRC);
    }

    // ---------------------------------------------------------------- surum duyurusu
    /// <summary>Surum etiketi: Versiyon Duyurulari ile ayni kural (aciklama 24 karakterden kisaysa etikettir, or. "Ara Surum")</summary>
    public static string VersionLabel(VersionAnnouncement v)
    {
        return v != null && !string.IsNullOrEmpty(v.BodyText) && v.BodyText.Trim().Length <= 24 ? v.BodyText.Trim() : null;
    }
    /// <summary>Yayin tarihi (ReleaseDate; yoksa yayinlanma zamani)</summary>
    public static string VersionDate(VersionAnnouncement v, bool en)
    {
        DateTime d = v.ReleaseDate ?? (v.PublishedAt ?? v.CreatedAt).AddHours(3);
        return d.ToString("d MMMM yyyy", en ? new CultureInfo("en-US") : TRC);
    }

    // ---------------------------------------------------------------- Yeni neler var?
    /// <summary>Kisiye gelen son 4 bildirim: turune gore renk ve ikon; okunmamislar parlak. Bos ise uygun mesaj.</summary>
    public static string News(int cid, int uid, bool en, DateTime nowTr, out int newCount)
    {
        newCount = 0;
        List<KeyValuePair<DateTime, string>> items = new List<KeyValuePair<DateTime, string>>();
        List<NotifRow> ns = Repo.GetNotifications(cid, uid, 20, null);
        int k = 0;
        foreach (NotifRow n in ns)
        {
            if (!n.IsRead) newCount++;
            if (k++ >= 6) continue;
            string kind, color, icon; Classify(n, en, out kind, out color, out icon);
            DateTime at = n.CreatedAt ?? DateTime.UtcNow;
            items.Add(new KeyValuePair<DateTime, string>(at, Item(kind, color, icon, !n.IsRead, When(at.AddHours(3), nowTr, en), n.Title(en ? "en" : "tr"), n.Body(en ? "en" : "tr"), n.Link, en)));
        }
        // forum: son ana gonderiler (yalniz forum bu kisiye aciksa)
        if (ForumRepo.CanAccess)
            try
            {
                foreach (ForumRepo.RecentPost p in ForumRepo.Recent(uid, 4))
                {
                    if (p.IsNew) newCount++;
                    items.Add(new KeyValuePair<DateTime, string>(p.AtUtc, Item("FORUM", "violet", "M4 5h16v11H9l-5 4zM8 9h8M8 12.5h5", p.IsNew, When(p.AtUtc.AddHours(3), nowTr, en),
                        p.Author + (en ? " posted in the forum" : " forumda paylaştı"), ForumRepo.Excerpt(p, 90), "Forum.aspx?post=" + p.Id, en)));
                }
            }
            catch { }
        items.Sort(delegate (KeyValuePair<DateTime, string> a, KeyValuePair<DateTime, string> b) { return b.Key.CompareTo(a.Key); });
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < items.Count && i < 4; i++) sb.Append(items[i].Value);
        if (sb.Length == 0)
            return Empty(en ? "No updates yet" : "Henüz yeni bir gelişme yok", en ? "Request replies, release notes and achievements will appear here." : "Talep yanıtları, sürüm duyuruları ve başarılar burada görünecek.", null, null, "M12 3l2 5 5 2-5 2-2 5-2-5-5-2 5-2z");
        return "<ol class=\"cu-news\"><li class=\"cu-line\" aria-hidden=\"true\"></li>" + sb + "</ol>";
    }

    private static string Item(string kind, string color, string icon, bool isNew, string when, string title, string body, string link, bool en)
    {
        string inner = "<span class=\"cu-node " + color + (isNew ? " new" : "") + "\"><svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"" + icon + "\"/></svg></span>" +
                       "<span class=\"cu-news-tx\"><span class=\"cu-news-meta\"><span class=\"cu-kind " + color + "\">" + H(kind) + "</span><span>" + H(when) + "</span>" + (isNew ? "<i class=\"cu-unread\" aria-label=\"" + (en ? "unread" : "okunmadı") + "\"></i>" : "") + "</span>" +
                       "<b" + (isNew ? " class=\"new\"" : "") + ">" + H(title) + "</b>" + (string.IsNullOrEmpty(body) ? "" : "<small>" + H(body) + "</small>") + "</span>";
        return string.IsNullOrEmpty(link) ? "<li><div class=\"cu-news-it\">" + inner + "</div></li>"
                                          : "<li><a class=\"cu-news-it lk\" href=\"" + H(link) + "\">" + inner + "<span class=\"ph-go\">" + ARROW + "</span></a></li>";
    }

    /// <summary>Bildirimin turu: once Type alani, sonra baglanti ve baslik (talep / atama / cozuldu / surum / sertifika / basari / akademi)</summary>
    public static void Classify(NotifRow n, bool en, out string kind, out string color, out string icon)
    {
        string t = (n.Type ?? "").ToLowerInvariant(), l = (n.Link ?? "").ToLowerInvariant(), title = (n.Title("tr") ?? "").ToLower(TRC);
        if (t.Contains("ticket") || l.Contains("ticket"))
        {
            if (title.Contains("çözül") || title.Contains("kapat")) { kind = en ? "RESOLVED" : "ÇÖZÜLDÜ"; color = "blue"; icon = "M5 12l5 5 9-10"; }
            else if (title.Contains("atand") || title.Contains("atama")) { kind = en ? "ASSIGNED" : "ATAMA"; color = "blue"; icon = "M3 13h5l2 3h4l2-3h5M5 5h14l2 8v6H3v-6z"; }
            else { kind = en ? "REQUEST" : "TALEP"; color = "orange"; icon = "M4 5h16v11H9l-5 4z"; }
        }
        else if (t.StartsWith("forum") || l.Contains("forum"))
        {
            if (t == "forummoderation") { kind = "FORUM"; color = "gray"; icon = "M5 21V4h11l-2 4 2 4H5"; }
            else if (t.EndsWith("like")) { kind = "FORUM"; color = "violet"; icon = "M12 20s-7-4.4-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.6-7 10-7 10z"; }
            else { kind = "FORUM"; color = "violet"; icon = "M4 5h16v11H9l-5 4zM8 9h8M8 12.5h5"; }
        }
        else if (t.Contains("version") || l.Contains("version")) { kind = en ? "NEW RELEASE" : "YENİ SÜRÜM"; color = "violet"; icon = "M12 3l2 5 5 2-5 2-2 5-2-5-5-2 5-2zM19 15l.8 2.2L22 18l-2.2.8L19 21l-.8-2.2L16 18l2.2-.8z"; }
        else if (t.Contains("cert") || t.Contains("exam") || l.Contains("certif") || l.Contains("exam")) { kind = en ? "CERTIFICATE" : "SERTİFİKA"; color = "gold"; icon = "M12 14a5 5 0 1 0 0-10 5 5 0 0 0 0 10zM9 13.5L8 21l4-2 4 2-1-7.5"; }
        else if (t.Contains("achievement")) { kind = en ? "YOUR TEAM" : "EKİBİNİZ"; color = "green"; icon = "M12 2.5l8 4.6v9.8l-8 4.6-8-4.6V7.1z"; }
        else if (t.Contains("transfer") || l.Contains("transfer")) { kind = en ? "FILES" : "DOSYA"; color = "blue"; icon = "M12 4v11M7 10l5 5 5-5M5 20h14"; }
        else if (t.Contains("training") || l.Contains("training")) { kind = en ? "ACADEMY" : "AKADEMİ"; color = "violet"; icon = "M2 9l10-5 10 5-10 5zM6 11v5c2 2 10 2 12 0v-5"; }
        else { kind = en ? "NOTICE" : "BİLDİRİM"; color = "gray"; icon = "M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9M10.3 21a1.9 1.9 0 0 0 3.4 0"; }
    }
}
