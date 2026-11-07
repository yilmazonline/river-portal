using System.Collections.Generic;
using System.Text;

// Tum sayfalarda AYNI ust menuyu uretir. Sayfa yalnizca aktif modul anahtarini verir.
// Boylece rol basina menu her gecin ayni kalir (drift olmaz).
public static class Nav
{
    /// <summary>Logo baglantisi: giris yapmissa kisinin ana sayfasi (Auth.HomeUrl), degilse Default.aspx</summary>
    public static string HomeHref { get { return Auth.IsLoggedIn ? Auth.HomeUrl : "Default.aspx"; } }

    private class Item
    {
        public string Key, Label, Href, I18n;
        public bool Soon;   // "Yakinda": gorunur ama tiklanamaz
        public Item(string k, string l, string h, string i) { Key = k; Label = l; Href = h; I18n = i; }
    }

    private static bool En { get { return Auth.Culture == "en"; } }
    private static string L(string tr, string en) { return En ? en : tr; }

    private static List<Item> ForRole()
    {
        List<Item> x = new List<Item>();
        if (Auth.IsVendorFirm) // uretici personeli (coklu izin: Portal / Editor / Support)
        {
            bool portal = Auth.IsVendor, sup = Auth.IsSupportAgent, edit = Auth.CanEditTraining;
            if (!portal && !Auth.CanFinance)
                x.Add(new Item("home", L("Ana Sayfa", "Home"), "Staff.aspx", "nav_home"));   // Fol personeli paneli
            if (portal)
            {
                x.Add(new Item("overview", L("Genel Bakış", "Overview"), "Vendor.aspx", "nav_overview"));
                x.Add(new Item("users", L("Kullanıcılar", "Users"), "Users.aspx", "nav_users"));
                x.Add(new Item("materials", L("Satış Materyalleri", "Sales Materials"), "Materials.aspx", "nav_materials"));
                // API ve Dosyalar ust menude degil: Genel Bakis'taki dugme satirinda (menu sadelestirildi)
            }
            if (portal || sup)
                x.Add(new Item("support", portal ? L("Destek", "Support") : L("Destek Kuyruğu", "Support Queue"), "Tickets.aspx", "nav_support"));
            if (Auth.CanFinance)
                x.Add(new Item("receivables", L("Alacaklar", "Receivables"), "Receivables.aspx", "nav_receivables"));
            if (portal || sup)
                x.Add(new Item("projects", L("Proje Kayıtları", "Project Records"), "ProjectRecords.aspx", "nav_projects"));
            x.Add(new Item("edu", L("River Akademi", "River Academy"), "Training.aspx", edit ? "nav_lms" : "nav_training"));
            x.Add(new Item("versions", L("Versiyon Duyuruları", "Version Announcements"), "Versions.aspx", "nav_versions"));
            if (!portal) x.Add(new Item("files", L("Dosyalar", "Files"), "Transfers.aspx", "nav_files"));   // Genel Bakis'i gormeyenler icin menude kalir
            x.Add(new Item("leaves", L("İzinlerim", "My Leave"), "Leaves.aspx", "nav_leaves"));   // tum vendor personeli
            if (Auth.IsSupportAgent || Auth.IsSuperAdmin) x.Add(new Item("kb", L("Bilgi Tabanı", "Knowledge Base"), "KnowledgeBase.aspx", "nav_kb"));   // destek ekibi
            if (Auth.IsSuperAdmin) x.Add(new Item("leads", L("Lead Merkezi", "Lead Center"), "Leads.aspx", "nav_leads"));   // yalniz super admin
        }
        else if (Auth.IsCustomerSide)
        {
            x.Add(new Item("home", L("Ana Sayfa", "Home"), "Customer.aspx", "nav_home"));
            x.Add(new Item("support", L("Taleplerim", "My Requests"), "Tickets.aspx", "nav_tickets"));
            x.Add(new Item("edu", L("River Akademi", "River Academy"), "Training.aspx", "nav_training"));
            x.Add(new Item("versions", L("Versiyon Duyuruları", "Version Announcements"), "Versions.aspx", "nav_versions"));
        }
        else // Partner
        {
            if (Auth.IsPartnerAdmin)
            {
                x.Add(new Item("home", L("Panel", "Dashboard"), "Partner.aspx", "nav_dash"));
                x.Add(new Item("orders", L("Siparişler", "Orders"), "Orders.aspx", "nav_orders"));
                x.Add(new Item("demos", L("Demo Talepleri", "Demo Requests"), "Demos.aspx", "nav_demos"));
                x.Add(new Item("tiers", L("Partner Seviyeleri", "Partner Tiers"), "Tiers.aspx", "nav_tiers"));
                x.Add(new Item("materials", L("Satış Materyalleri", "Sales Materials"), "Materials.aspx", "nav_materials"));
                x.Add(new Item("support", L("Destek", "Support"), "Tickets.aspx", "nav_support"));
                x.Add(new Item("edu", L("River Akademi", "River Academy"), "Training.aspx", "nav_training"));
                x.Add(new Item("versions", L("Versiyon Duyuruları", "Version Announcements"), "Versions.aspx", "nav_versions"));
            }
            else // PartnerUser (kısıtlı): Ana Sayfa + Destek + Akademi + Versiyon
            {
                x.Add(new Item("home", L("Ana Sayfa", "Home"), "Customer.aspx", "nav_home"));
                x.Add(new Item("support", L("Destek", "Support"), "Tickets.aspx", "nav_support"));
                x.Add(new Item("edu", L("River Akademi", "River Academy"), "Training.aspx", "nav_training"));
                x.Add(new Item("versions", L("Versiyon Duyuruları", "Version Announcements"), "Versions.aspx", "nav_versions"));
            }
        }
        return x;
    }

    // Aktif modul anahtari: home, orders, demos, tiers, materials, support, edu, versions,
    // overview, partners, vdemos, users
    public static string Links(string active)
    {
        List<Item> items = ForRole();
        // Forum: herkese acik (yeni gonderi sayisi kosede)
        Item forum = new Item("forum", "Forum", "Forum.aspx", null);
        forum.Soon = !ForumRepo.CanAccess;
        items.Add(forum);
        StringBuilder sb = new StringBuilder();
        // menuden Genel Bakis'a tasinan sayfalar (Dosyalar, API) acikken Genel Bakis vurgulu gorunur
        if ((active == "files" || active == "api") && !items.Exists(delegate (Item z) { return z.Key == active; })) active = "overview";
        for (int i = 0; i < items.Count; i++)
        {
            Item it = items[i];
            bool act = it.Key == active;
            if (it.Soon)
            {
                sb.Append("<span class=\"nav-soon\" aria-disabled=\"true\" title=\"" + L("Yakında kullanıma açılacak", "Coming soon") + "\">" + Fmt.Enc(it.Label) + "<i>" + L("Yakında", "Soon") + "</i></span>");
                continue;
            }
            // forum: son ziyaretten sonraki yeni ana gonderi sayisi (forum sayfasinda gosterilmez, orada sifirlanir)
            int unread = it.Key == "forum" && !act ? ForumRepo.UnreadCountCached() : 0;
            sb.Append("<a");
            if (act) sb.Append(" class=\"active\"");
            sb.Append(" href=\"" + it.Href + "\"");
            if (!string.IsNullOrEmpty(it.I18n)) sb.Append(" data-i18n=\"" + it.I18n + "\"");
            if (unread > 0) sb.Append(" aria-label=\"" + Fmt.Enc(it.Label) + ", " + unread + L(" yeni gönderi", " new posts") + "\"");
            sb.Append(">" + Fmt.Enc(it.Label) + (unread > 0 ? "<i class=\"nav-cnt\" aria-hidden=\"true\">" + (unread > 99 ? "99+" : unread.ToString()) + "</i>" : "") + "</a>");
        }
        return sb.ToString();
    }
}
