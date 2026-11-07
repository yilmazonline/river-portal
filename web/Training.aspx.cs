using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// River Akademi (Netflix tarzı): tam genişlik, hero, kategori ağacı, slider satırları.
public partial class TrainingPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public bool IsVendor = false;
    public bool IsCustomer = false;
    public bool CanEdit = false;
    public string CertBadgesHtml = "";
    public string PathJson = "null", PathAdminJson = "[]";
    public string TeamJson = "null";   // sol menu "Takim Arkadaslarim" karti
    // sol menu kisisel kart: avatar, bu haftaki seri, toplam izleme
    public string MyAvatar = "", MyWatch = "0 dk"; public int MyWeekDays = 0;   // Sifirdan Uzmanliga: ilk durum + editor listesi
    public string WatchJson = "[]";
    public string VideosJson = "{}";
    public int CertCount = 0;
    public int PendingRequests = 0;
    public bool IsEditorOnly = false;
    public string HeroHtml = "";
    public string RowsHtml = "";
    public string DocsHtml = "";
    public string EditorStatsHtml = "";
    public string TreeHtml = "";
    public string DocTreeHtml = "";
    public string ShelfHtml = "";   // ana ekran: kaydirilabilir kategori rafi (video + dokuman kategorileri birlesik)
    public string CatGroupOptions = "";
    public string CatSubOptions = "";
    // kapsama gore (Video / Dokuman) kategori secenekleri; alt kategoriler data-g ile ana kategoriye bagli
    public string VidGroupOptions = "", VidSubOptions = "", DocGroupOptions = "", DocSubOptions = "";
    public string FlashErr = "";
    public string GroupSuggestions = "";
    public string SubSuggestions = "";
    public string MyRatingsJson = "{}";
    public int StatMinutes = 0;
    public int StatContent = 0;
    public string ProgressHtml = "";
    public int StatVideosDone = 0, StatVideosTotal = 0, StatRate = 0, StatCoursesDone = 0, StatCoursesTotal = 0;
    public string HistHtml = "";
    public string FlashMsg = "";
    public int Count = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        IsVendor = Auth.IsVendor;
        IsCustomer = Auth.IsCustomerSide;
        CanEdit = Auth.CanEditTraining;
        IsEditorOnly = Auth.IsTrainingEditor;
        bool en = Culture == "en";
        if (Auth.IsPartnerSide || Auth.IsCustomerSide) CertBadgesHtml = BuildCertBadges();
        else if (CanEdit) PendingRequests = ExamRepo.PendingRequestCount();

        string ok = Request.QueryString["ok"];
        if (ok == "add") FlashMsg = en ? "Video published." : "Video yayınlandı.";
        else if (ok == "del") FlashMsg = en ? "Video removed." : "Video kaldırıldı.";
        else if (ok == "suggest") FlashMsg = en ? "Thanks! Your suggestion was sent." : "Teşekkürler! Öneriniz iletildi.";
        string er = Request.QueryString["err"];
        if (er == "type") FlashErr = "Bu dosya türü yüklenemez: program ve betik dosyaları (.exe, .bat, .js, .ps1 gibi) güvenlik nedeniyle engellenir.";
        else if (er == "nofile") FlashErr = "Dosya seçilmedi ya da dosya boş.";
        else if (er == "1") FlashErr = "Başlık zorunludur.";

        List<TrainingVideo> videos = Repo.GetTrainingVideos();
        Dictionary<int, int> lastPos = new Dictionary<int, int>();
        Count = videos.Count;

        // Kullanici ilerlemesini birlestir
        try
        {
            Dictionary<int, int[]> prog = Repo.GetUserProgress(Auth.UserId);
            for (int i = 0; i < videos.Count; i++)
            {
                int[] p;
                if (prog.TryGetValue(videos[i].MaterialId, out p)) { videos[i].ProgressPercent = p[0]; videos[i].Completed = p[1] == 1; lastPos[videos[i].MaterialId] = p.Length > 2 ? p[2] : 0; }
            }
        }
        catch { }

        VideosJson = BuildVideosJson(videos, lastPos);
        WatchJson = BuildWatchJson();

        RowsHtml = BuildRows(videos);
        List<TrainingDoc> docs = Repo.GetTrainingDocuments();
        DocsHtml = BuildDocs(docs);
        if (Auth.CanEditTraining) EditorStatsHtml = BuildEditorStats(videos, docs);
        // Sol menu ve ana ekran rafi ayni birlesik kategori listesini kullanir (video + dokuman kategorileri tek listede)
        List<UCat> ucats = BuildUnified(videos, docs);
        List<HCard> hcards = BuildCards(ucats, videos, docs);   // rafta alt kategoriler ayri kart
        TreeHtml = BuildCatTree(ucats, hcards);
        DocTreeHtml = "";
        ShelfHtml = BuildShelf(hcards);
        HeroHtml = "";   // ust alan kaldirildi: sayfa dogrudan sol menu ve kategorilerle baslar
        try { UserRow me = Repo.GetUserById(Auth.UserId); if (me != null) MyAvatar = me.AvatarImage ?? ""; } catch { }
        try { int sec, days; DateTime? fa, la; TrainingDetailRepo.GetUserTotals(Auth.UserId, out sec, out days, out fa, out la); MyWatch = sec < 3600 ? (sec / 60) + " dk" : (sec / 3600.0).ToString("0.#", new System.Globalization.CultureInfo("tr-TR")) + " sa"; } catch { }
        // Sifirdan Uzmanliga: gecmise donuk rozetler (bir kez, bildirimsiz) + ilk durum; tablo yoksa ozellik gizli kalir
        try
        {
            LearningPath.EnsureRetro(Auth.UserId, Auth.CompanyId);
            System.Web.Script.Serialization.JavaScriptSerializer pjs = new System.Web.Script.Serialization.JavaScriptSerializer();
            PathJson = pjs.Serialize(LearningPath.State(Auth.UserId, Auth.CompanyId));
            try { TeamJson = pjs.Serialize(LearningPath.TeamSummary(Auth.UserId, Auth.CompanyId)); } catch { TeamJson = "null"; }
            try { MyWeekDays = LearningPath.WeekDays(Auth.UserId); } catch { }
            if (CanEdit)
            {
                List<object> al = new List<object>();
                foreach (LearningPath.AdminStep a in LearningPath.GetAdminList()) al.Add(new { mid = a.MaterialId, title = a.Title, grp = a.Group, inPath = a.InPath });
                PathAdminJson = pjs.Serialize(al);
            }
        }
        catch { PathJson = "null"; }
        BuildSuggestions(videos);
        BuildCatOptions(videos, docs);

        try
        {
            int[] st = Repo.GetMonthlyWatchStats(Auth.UserId);
            StatMinutes = st[0]; StatContent = st[1];
        }
        catch { }
        try { MyRatingsJson = BuildRatingsJson(Repo.GetUserRatings(Auth.UserId)); }
        catch { }
        try { HistHtml = BuildHistory(Repo.GetUserTrainingHistory(Auth.UserId)); }
        catch { }
        // Ayrintili gecmis (yonetici sayfasiyla ayni hesap); sorgu calismazsa sade gorunume duser
        try { ProgressHtml = BuildProgressDetailed(); }
        catch { try { ProgressHtml = BuildProgress(); } catch { } }
    }

    private string HistItem(TrainingHist h)
    {
        System.Globalization.CultureInfo tr = new System.Globalization.CultureInfo("tr-TR");
        int mins = h.Seconds / 60;
        string open = h.YoutubeId.Length > 0
            ? " data-vid=\"" + Fmt.Enc(h.YoutubeId) + "\" data-mid=\"" + h.MaterialId + "\" data-title=\"" + Fmt.Enc(h.Title(Culture)) + "\" style=\"cursor:pointer\""
            : "";
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"hist-item hist-open\"" + open + ">");
        sb.Append("<div class=\"hist-main\"><div class=\"hist-title\">" + Fmt.Enc(h.Title(Culture)) + "</div>");
        sb.Append("<div class=\"hist-sub\">" + h.LastAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", tr) + " · " + mins + " dk" +
                  (h.Completed ? " · <span class=\"hist-done\">Tamamlandı</span>" : "") + "</div></div>");
        sb.Append("<div class=\"hist-prog\"><div class=\"hist-bar\"><i style=\"width:" + h.Percent + "%\"></i></div><span>%" + h.Percent + "</span></div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private string BuildHistory(List<TrainingHist> hist)
    {
        if (hist.Count == 0)
            return "<div class=\"ec-empty\" style=\"padding:24px\">Bu ay henüz bir eğitim izlemediniz.</div>";

        List<TrainingHist> ongoing = new List<TrainingHist>();
        List<TrainingHist> done = new List<TrainingHist>();
        for (int i = 0; i < hist.Count; i++)
        {
            if (hist[i].Completed) done.Add(hist[i]); else ongoing.Add(hist[i]);
        }

        StringBuilder sb = new StringBuilder();
        if (ongoing.Count > 0)
        {
            sb.Append("<div class=\"hist-sec\">Devam Edenler (" + ongoing.Count + ")</div><div class=\"hist-list\">");
            for (int i = 0; i < ongoing.Count; i++) sb.Append(HistItem(ongoing[i]));
            sb.Append("</div>");
        }
        if (done.Count > 0)
        {
            sb.Append("<div class=\"hist-sec\">Tamamladıklarım (" + done.Count + ")</div><div class=\"hist-list\">");
            for (int i = 0; i < done.Count; i++) sb.Append(HistItem(done[i]));
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private string BuildRatingsJson(Dictionary<int, string[]> map)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        bool first = true;
        foreach (KeyValuePair<int, string[]> kv in map)
        {
            if (!first) sb.Append(",");
            first = false;
            string c = (kv.Value[1] ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", " ").Replace("<", "\\u003C");
            sb.Append("\"" + kv.Key + "\":{\"s\":" + kv.Value[0] + ",\"c\":\"" + c + "\"}");
        }
        sb.Append("}");
        return sb.ToString();
    }

    private string BuildDocTree(List<TrainingDoc> docs)
    {
        if (docs.Count == 0) return "";
        List<string> groups = new List<string>();
        Dictionary<string, List<string>> subs = new Dictionary<string, List<string>>();
        for (int i = 0; i < docs.Count; i++)
        {
            string g = docs[i].Group(), s = docs[i].Sub();
            if (!groups.Contains(g)) { groups.Add(g); subs[g] = new List<string>(); }
            if (!subs[g].Contains(s)) subs[g].Add(s);
        }
        SortCats(groups, subs, TrainingCats.Order("Document"));
        StringBuilder sb = new StringBuilder();
        for (int gi = 0; gi < groups.Count; gi++)
        {
            string g = groups[gi];
            sb.Append("<div class=\"ec-tnode\"><a class=\"ec-tgroup\" href=\"#doc-grp-" + gi + "\">" + Fmt.Enc(g) + "</a>");
            if (!(subs[g].Count == 1 && subs[g][0] == "Genel"))
            {
                sb.Append("<div class=\"ec-tsubs\">");
                for (int si = 0; si < subs[g].Count; si++)
                    sb.Append("<a href=\"#doc-" + gi + "-" + si + "\">" + Fmt.Enc(subs[g][si]) + "</a>");
                sb.Append("</div>");
            }
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private void BuildCatOptions(List<TrainingVideo> videos, List<TrainingDoc> docs)
    {
        List<string[]> vp = new List<string[]>(), dp = new List<string[]>();
        for (int i = 0; i < videos.Count; i++) vp.Add(new[] { videos[i].Group(), videos[i].Sub() });
        for (int i = 0; i < docs.Count; i++) dp.Add(new[] { docs[i].Group(), docs[i].Sub() });
        string go, so;
        ScopeOptions("Video", vp, out go, out so); VidGroupOptions = go; VidSubOptions = so;
        ScopeOptions("Document", dp, out go, out so); DocGroupOptions = go; DocSubOptions = so;
        BuildCatOptionsLegacy(videos, docs);
    }

    private static void ScopeOptions(string scope, List<string[]> pairs, out string groupOpts, out string subOpts)
    {
        List<string> gs = new List<string>(); List<string[]> ss = new List<string[]>();
        bool tbl = TrainingCats.TableExists();
        if (tbl) foreach (TCat m in TrainingCats.List(scope)) { gs.Add(m.Name); foreach (TCat s in m.Subs) ss.Add(new[] { m.Name, s.Name }); }
        foreach (string[] p in pairs)
        {
            if (!gs.Exists(x => string.Equals(x, p[0], StringComparison.CurrentCultureIgnoreCase))) gs.Add(p[0]);
            if (p[1] != "Genel" && !ss.Exists(x => string.Equals(x[0], p[0], StringComparison.CurrentCultureIgnoreCase) && string.Equals(x[1], p[1], StringComparison.CurrentCultureIgnoreCase))) ss.Add(new[] { p[0], p[1] });
        }
        if (!tbl) gs.Sort();
        StringBuilder gb = new StringBuilder(), sb = new StringBuilder();
        foreach (string g in gs) gb.Append("<option value=\"" + Fmt.Enc(g) + "\">" + Fmt.Enc(g) + "</option>");
        foreach (string[] s in ss) sb.Append("<option value=\"" + Fmt.Enc(s[1]) + "\" data-g=\"" + Fmt.Enc(s[0]) + "\">" + Fmt.Enc(s[1]) + "</option>");
        groupOpts = gb.ToString(); subOpts = sb.ToString();
    }

    private void BuildCatOptionsLegacy(List<TrainingVideo> videos, List<TrainingDoc> docs)
    {
        List<string> gg = new List<string>();
        List<string> ss = new List<string>();
        for (int i = 0; i < videos.Count; i++)
        {
            string g = videos[i].Group(), s = videos[i].Sub();
            if (!gg.Contains(g)) gg.Add(g);
            if (!ss.Contains(s)) ss.Add(s);
        }
        for (int i = 0; i < docs.Count; i++)
        {
            string g = docs[i].Group(), s = docs[i].Sub();
            if (!gg.Contains(g)) gg.Add(g);
            if (!ss.Contains(s)) ss.Add(s);
        }
        gg.Sort(); ss.Sort();
        StringBuilder gb = new StringBuilder();
        for (int i = 0; i < gg.Count; i++) gb.Append("<option value=\"" + Fmt.Enc(gg[i]) + "\">" + Fmt.Enc(gg[i]) + "</option>");
        CatGroupOptions = gb.ToString();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < ss.Count; i++) sb.Append("<option value=\"" + Fmt.Enc(ss[i]) + "\">" + Fmt.Enc(ss[i]) + "</option>");
        CatSubOptions = sb.ToString();
    }

    /// <summary>Satira kategori / alt kategori etiketi ekler (kategori sayfasinda suzme icin)</summary>
    private static string Tag(string row, string g, string sub)
    {
        if (string.IsNullOrEmpty(row)) return row;
        int i = row.IndexOf("<section class=\"ec-row\"");
        if (i < 0) return row;
        return row.Substring(0, i) + "<section data-g=\"" + Fmt.Enc(g) + "\" data-s=\"" + Fmt.Enc(sub) + "\" class=\"ec-row\"" + row.Substring(i + "<section class=\"ec-row\"".Length);
    }

    private class UCat { public string Name, Slug, Cover; public List<string> Subs = new List<string>(); public int Videos, Docs; }



    /// <summary>Adres icin kisa ad: "Form Tasarimi" -> "form-tasarimi" (Turkce harfler sadelesir)</summary>
    private static string Slug(string n)
    {
        string t = (n ?? "").ToLower(new CultureInfo("tr-TR"));
        t = t.Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i").Replace("ö", "o").Replace("ş", "s").Replace("ü", "u").Replace("â", "a").Replace("î", "i").Replace("û", "u");
        StringBuilder b = new StringBuilder(); bool dash = false;
        foreach (char ch in t) { if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9')) { b.Append(ch); dash = false; } else if (!dash && b.Length > 0) { b.Append('-'); dash = true; } }
        string r = b.ToString().TrimEnd('-');
        return r.Length == 0 ? "kategori" : r;
    }

    /// <summary>Video ve dokuman kategorilerini addan eslestirip birlestirir; sira: video sirasi, sonra yalniz dokumani olanlar</summary>
    private static readonly CultureInfo TRc = new CultureInfo("tr-TR");
    /// <summary>Kategori adi esitligi: Turkce kurallarla kucuk harf, sonra "ı" = "i".
    /// "FORM TASARIMI" = "Form Tasarımı", "Intranet" = "intranet" = "İntranet". Tarayicidaki acshelf.js fold() ile ayni kural.</summary>
    private static string Fold(string s) { return (s ?? "").Trim().ToLower(TRc).Replace('ı', 'i'); }
    private static bool Same(string a, string b) { return Fold(a) == Fold(b); }

    private List<UCat> BuildUnified(List<TrainingVideo> videos, List<TrainingDoc> docs)
    {
        List<UCat> l = new List<UCat>();
        Func<string, UCat> get = delegate (string name)
        {
            foreach (UCat u in l) if (Same(u.Name, name)) return u;
            UCat n = new UCat(); n.Name = name; l.Add(n); return n;
        };
        Prepare(videos);
        foreach (string g in _groups)
        {
            UCat u = get(g);
            foreach (string sub in _subs[g]) if (sub != "Genel" && !u.Subs.Exists(x => Same(x, sub))) u.Subs.Add(sub);
            foreach (TrainingVideo v in videos) if (v.Group() == g) u.Videos++;
            // kapak: alt kategori sirasindaki ilk alt kategorinin alfabetik ilk videosu (listedeki "1." ile uyumlu)
            foreach (string sub in _subs[g])
            {
                foreach (TrainingVideo v in InSub(videos, g, sub)) if (!string.IsNullOrEmpty(v.YoutubeId)) { u.Cover = "https://img.youtube.com/vi/" + v.YoutubeId + "/hqdefault.jpg"; break; }
                if (u.Cover != null) break;
            }
        }
        List<string> dg = new List<string>(); Dictionary<string, List<string>> ds = new Dictionary<string, List<string>>();
        foreach (TrainingDoc d in docs) { string g = d.Group(), sub = d.Sub(); if (!dg.Contains(g)) { dg.Add(g); ds[g] = new List<string>(); } if (!ds[g].Contains(sub)) ds[g].Add(sub); }
        SortCats(dg, ds, TrainingCats.Order("Document"));
        foreach (string g in dg)
        {
            UCat u = get(g);
            foreach (string sub in ds[g]) if (sub != "Genel" && !u.Subs.Exists(x => Same(x, sub))) u.Subs.Add(sub);
            foreach (TrainingDoc d in docs) if (Same(d.Group(), g)) { u.Docs++; if (u.Cover == null && !string.IsNullOrEmpty(d.CoverImage)) u.Cover = "assets/traincovers/" + d.CoverImage; }
        }
        Dictionary<string, int> used = new Dictionary<string, int>();
        foreach (UCat u in l) { string sl = Slug(u.Name); int n; if (used.TryGetValue(sl, out n)) { used[sl] = n + 1; sl = sl + "-" + (n + 1); } else used[sl] = 1; u.Slug = sl; }
        return l;
    }


    /// <summary>Raf karti: bir alt kategori (Sub dolu) ya da ana kategorinin alt kategorisiz icerikleri (Sub = null)</summary>
    private class HCard { public UCat Main; public string Sub, Slug, Cover, Band, Icon; public int? Tint; public int Videos, Docs; public List<string> Titles = new List<string>(); }

    /// <summary>Editorun kategori gorunum ayarlari (kart / kapak gorseli, renk); tablo yoksa bos</summary>
    private static List<TStyle> SafeStyles() { try { return TrainingCats.Styles(); } catch { return new List<TStyle>(); } }

    /// <summary>Raf kartlari: her ana kategorinin alt kategorileri ayri kart; alt kategorisiz icerik varsa ana kategori adiyla bir kart</summary>
    private List<HCard> BuildCards(List<UCat> mains, List<TrainingVideo> videos, List<TrainingDoc> docs)
    {
        List<HCard> l = new List<HCard>();
        List<TStyle> st = SafeStyles();
        foreach (UCat m in mains)
        {
            List<string> subs = new List<string>(); subs.Add(null); subs.AddRange(m.Subs);   // null = "Genel" (alt kategorisiz)
            foreach (string sub in subs)
            {
                HCard c = new HCard(); c.Main = m; c.Sub = sub;
                string want = sub ?? "Genel";
                foreach (TrainingVideo v in videos)
                    if (Same(v.Group(), m.Name) && Same(v.Sub(), want))
                    {
                        c.Videos++; if (c.Titles.Count < 3) c.Titles.Add(v.Title(Culture));
                        if (c.Cover == null && !string.IsNullOrEmpty(v.YoutubeId)) c.Cover = "https://img.youtube.com/vi/" + v.YoutubeId + "/hqdefault.jpg";
                    }
                foreach (TrainingDoc d in docs)
                    if (Same(d.Group(), m.Name) && Same(d.Sub(), want))
                    {
                        c.Docs++; if (c.Titles.Count < 3) c.Titles.Add(d.Title(Culture));
                        if (c.Cover == null && !string.IsNullOrEmpty(d.CoverImage)) c.Cover = "assets/traincovers/" + d.CoverImage;
                    }
                if (c.Videos + c.Docs == 0) continue;
                // editorun gorunum ayarlari: kart gorseli otomatik kapagin, kapak gorseli bant resminin yerine gecer
                TStyle ts = TrainingCats.FindStyle(st, m.Name, sub ?? "");
                // oncelik: yuklenen kart gorseli > hazir ikon > otomatik (ilk video)
                if (ts != null) { if (ts.Card != null) c.Cover = TrainingCats.StyleUrl(ts.Card); else if (ts.Icon != null) c.Icon = ts.Icon; c.Band = TrainingCats.StyleUrl(ts.Cover); c.Tint = ts.Tint; }
                // alt kategorisiz kart "--genel" alir: "#kat-<ana>" sol menudeki ana kategori sayfasina (tum icerikler) ayrilmistir
                c.Slug = m.Slug + "--" + (sub == null ? "genel" : Slug(sub));
                l.Add(c);
            }
        }
        return l;
    }

    /// <summary>Kapak ikonu: hazir ikon (SVG sprite) ya da yuklenen ikon (sekli maske olarak kullanilir, ayni renk ve efekt)</summary>
    private static string IconHtml(string icon)
    {
        string u = TrainingCats.CustomIconUrl(icon);
        // mutlak yol: degiskendeki url() stil dosyasinin (css/) konumuna gore cozulur, goreli yol yanlis klasore gider
        if (u != null) { try { u = System.Web.VirtualPathUtility.ToAbsolute("~/" + u); } catch { u = "/" + u; } }
        if (u != null) return "<i class=\"ico-u\" style=\"--iu:url('" + u + "')\" aria-hidden=\"true\"></i>";
        return "<svg aria-hidden=\"true\"><use href=\"assets/caticons.svg#i-" + icon + "\"></use></svg>";
    }

    private static string Meta(int v, int d)
    {
        List<string> p = new List<string>();
        if (v > 0) p.Add(v + " video");
        if (d > 0) p.Add(d + " doküman");
        return string.Join(" · ", p.ToArray());
    }

    /// <summary>Kardes kategoriler: ayni ana kategorinin raf kartlari ("slug|ad" ciftleri; bantta gecis etiketi olur)</summary>
    private static string Siblings(List<HCard> cards, UCat m, string exceptSlug)
    {
        List<string> p = new List<string>();
        foreach (HCard c in cards) if (c.Main == m && c.Slug != exceptSlug) p.Add(c.Slug + "|" + (c.Sub ?? m.Name).Replace("|", "/"));
        return string.Join("||", p.ToArray());
    }

    /// <summary>Kategori sayfasinin veri ozellikleri: raf kartinda ve sol menu baglantisinda ayni adlarla tasinir</summary>
    private const int Themes = 15;  // dokulu temalar: css .acg-0 .. .acg-14 (editor paletiyle ayni sira; 8 Gunes ve 14 Buz koyu yazili)
    private static string DataAttrs(string slug, string title, string kicker, string g, string s, int v, int d, string img, int theme, string sibs)
    { return DataAttrs(slug, title, kicker, g, s, v, d, img, theme, sibs, null); }

    /// <summary>ico: kapak gorseli yoksa bantta gosterilecek hazir ikon</summary>
    private static string DataAttrs(string slug, string title, string kicker, string g, string s, int v, int d, string img, int theme, string sibs, string ico)
    {
        if (ico != null) return DataAttrs(slug, title, kicker, g, s, v, d, img, theme, sibs, null) + " data-ico=\"" + ico + "\"";
        return " data-cat=\"" + slug + "\" data-name=\"" + Fmt.Enc(title) + "\" data-kicker=\"" + Fmt.Enc(kicker) + "\" data-g=\"" + Fmt.Enc(g) + "\"" +
               (s != null ? " data-s=\"" + Fmt.Enc(s) + "\"" : "") + " data-meta=\"" + Fmt.Enc(Meta(v, d)) + "\" data-vc=\"" + v + "\" data-dc=\"" + d +
               "\" data-img=\"" + Fmt.Enc(img ?? "") + "\" data-tint=\"" + theme + "\" data-sibs=\"" + Fmt.Enc(sibs) + "\"";
    }

    /// <summary>Ana ekran rafi: her alt kategori ayri, renk tonlu uzun kart; sigmayinca saga kayar (mevcut ray yapisi)</summary>
    private string BuildShelf(List<HCard> l)
    {
        if (l.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        sb.Append("<section class=\"ac-shelf\" id=\"acShelf\" aria-labelledby=\"acShelfH\"><div class=\"ac-shelf-h\"><h2 class=\"ec-section-title\" id=\"acShelfH\">Kategoriler</h2><span>" + l.Count + " kategori</span></div>");
        sb.Append("<div class=\"ac-grid\">");   // kartlar satir satir dizilir (kaydirma ve ok yok)
        for (int i = 0; i < l.Count; i++)
        {
            HCard c = l[i]; int t = c.Tint.HasValue ? c.Tint.Value : i % Themes; string num = (i < 9 ? "0" : "") + (i + 1);
            string title = c.Sub ?? c.Main.Name, kicker = c.Sub != null ? c.Main.Name : "";
            sb.Append("<button type=\"button\" class=\"ac-cat acg acg-" + t + "\"" + DataAttrs(c.Slug, title, kicker, c.Main.Name, c.Sub ?? "Genel", c.Videos, c.Docs, c.Band ?? (c.Icon != null ? null : c.Cover), t, Siblings(l, c.Main, c.Slug), c.Band == null ? c.Icon : null) +
                      " data-num=\"" + num + "\">");
            if (c.Icon != null)   // hazir ikon: temanin dokusu uzerinde, tek basina
                sb.Append("<span class=\"ac-cat-cv ico\"><span class=\"ico-g\">" + IconHtml(c.Icon) + "</span></span>");
            else
                sb.Append("<span class=\"ac-cat-cv\">" + (c.Cover != null ? "<img src=\"" + Fmt.Enc(c.Cover) + "\" alt=\"\" loading=\"lazy\" />" : "<span class=\"ac-cat-ph\" aria-hidden=\"true\">" + Fmt.Enc(title.Substring(0, 1).ToUpper(TRc)) + "</span>") + "</span>");
            sb.Append("<span class=\"ac-cat-b\"><span class=\"ac-cat-n\">" + num + (kicker.Length > 0 ? " · " + Fmt.Enc(kicker) : "") + "</span><span class=\"ac-cat-t\">" + Fmt.Enc(title) + "</span><span class=\"ac-cat-s\">");
            foreach (string tt in c.Titles) sb.Append("<span>" + Fmt.Enc(tt) + "</span>");
            int more = c.Videos + c.Docs - c.Titles.Count;
            if (more > 0) sb.Append("<span class=\"ac-cat-more\">+" + more + " içerik daha</span>");
            sb.Append("</span><span class=\"ac-cat-f\"><span>" + Fmt.Enc(Meta(c.Videos, c.Docs)) + "</span><span class=\"ac-cat-go\" aria-hidden=\"true\"><svg width=\"16\" height=\"16\" viewBox=\"0 0 16 16\"><path d=\"M4 8 H12 M8.5 4.5 L12 8 L8.5 11.5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg></span></span></span></button>");
        }
        sb.Append("</div></section>");
        return sb.ToString();
    }

    /// <summary>Sol menu: ayni yapi (ana kategori, altinda alt kategoriler). Ana kategori: tum icerikleri; alt kategori: rafla ayni sayfa.</summary>
    private string BuildCatTree(List<UCat> l, List<HCard> cards)
    {
        if (l.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        sb.Append("<a class=\"ec-tree-top\" href=\"#ec-home\" data-home=\"1\">Ana Sayfa</a>");
        List<TStyle> st = SafeStyles();
        for (int i = 0; i < l.Count; i++)
        {
            UCat u = l[i]; TStyle ts = TrainingCats.FindStyle(st, u.Name, "");
            int t = ts != null && ts.Tint.HasValue ? ts.Tint.Value : i % Themes;
            string img = ts != null ? (TrainingCats.StyleUrl(ts.Cover) ?? TrainingCats.StyleUrl(ts.Card) ?? (ts.Icon != null ? null : u.Cover)) : u.Cover;
            string ico = ts != null && ts.Cover == null && ts.Card == null ? ts.Icon : null;
            string sibs = Siblings(cards, u, null);
            sb.Append("<div class=\"ec-tnode\"><a class=\"ec-tgroup\" href=\"#kat-" + u.Slug + "\"" + DataAttrs(u.Slug, u.Name, "Tüm içerikler", u.Name, null, u.Videos, u.Docs, img, t, sibs, ico) + ">" + Fmt.Enc(u.Name) + "</a>");
            List<HCard> mine = cards.FindAll(c => c.Main == u && c.Sub != null);
            if (mine.Count > 0)
            {
                sb.Append("<div class=\"ec-tsubs\">");
                foreach (HCard c in mine) sb.Append("<a href=\"#kat-" + c.Slug + "\" data-cat=\"" + c.Slug + "\" data-sub=\"1\">" + Fmt.Enc(c.Sub) + "</a>");
                sb.Append("</div>");
            }
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private string DocCard(TrainingDoc d)
    {
        StringBuilder sb = new StringBuilder();
        string ext = d.Ext();
        string href = Fmt.Enc(d.Href());
        sb.Append("<div class=\"ec-doc ec-card-doc\" data-href=\"" + href + "\" data-type=\"" + Fmt.Enc(ext) +
                  "\" data-kind=\"" + d.Kind() + "\" data-dl=\"" + Fmt.Enc(d.DownloadHref()) + "\" data-size=\"" + d.FileSize +
                  "\" data-pdf=\"" + (d.IsPdf() ? "1" : "0") + "\" data-title=\"" + Fmt.Enc(d.Title(Culture)) +
                  "\" data-desc=\"" + Fmt.Enc(d.Desc(Culture)) + "\" data-download=\"" + Fmt.Enc(d.OriginalName) + "\">");
        sb.Append("<div class=\"ec-doc-cover\">");
        if (!string.IsNullOrEmpty(d.CoverImage))
            sb.Append("<img src=\"assets/traincovers/" + Fmt.Enc(d.CoverImage) + "\" alt=\"\" loading=\"lazy\" />");
        else
            sb.Append("<div class=\"ec-doc-ph ext-" + ext.ToLowerInvariant() + "\"><span class=\"ec-doc-ext\">" + Fmt.Enc(ext) + "</span></div>");
        sb.Append("<span class=\"ec-doc-badge\">" + Fmt.Enc(ext) + (d.SizeStr().Length > 0 ? " · " + d.SizeStr() : "") + "</span>");
        sb.Append("<span class=\"ec-play ec-doc-open\"><svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#fff\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z\"/><polyline points=\"14 2 14 8 20 8\"/></svg></span>");
        sb.Append("</div>");
        sb.Append("<div class=\"ec-info\"><div class=\"ec-title\">" + Fmt.Enc(d.Title(Culture)) + "</div>");
        sb.Append("<div class=\"ec-rate\">" + Fmt.Enc(ext) + (d.SizeStr().Length > 0 ? " · " + d.SizeStr() : "") + "</div></div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private string DocRow(string id, string title, List<TrainingDoc> items)
    {
        if (items.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        sb.Append("<section class=\"ec-row\" id=\"" + id + "\">");
        sb.Append("<h2 class=\"ec-row-title\">" + Fmt.Enc(title) + "</h2>");
        sb.Append("<div class=\"ec-rail-wrap\"><button type=\"button\" class=\"ec-arrow left\" aria-label=\"Geri\">\u2039</button><div class=\"ec-rail\">");
        for (int i = 0; i < items.Count; i++) sb.Append(DocCard(items[i]));
        sb.Append("</div><button type=\"button\" class=\"ec-arrow right\" aria-label=\"İleri\">\u203A</button></div></section>");
        return sb.ToString();
    }

    private string BuildEditorStats(List<TrainingVideo> videos, List<TrainingDoc> docs)
    {
        int totalViews = 0;
        for (int i = 0; i < videos.Count; i++) totalViews += videos[i].ViewCount;
        List<TrainingVideo> top = new List<TrainingVideo>(videos);
        top.Sort(delegate (TrainingVideo a, TrainingVideo b) { return b.ViewCount.CompareTo(a.ViewCount); });

        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"ec-stats\" style=\"margin:2px 0 18px\">");
        sb.Append("<div class=\"ec-stat\"><div class=\"v\">" + videos.Count + "</div><div class=\"k\">Yayındaki video</div></div>");
        sb.Append("<div class=\"ec-stat\"><div class=\"v\">" + docs.Count + "</div><div class=\"k\">Yayındaki doküman</div></div>");
        sb.Append("<div class=\"ec-stat\"><div class=\"v\">" + totalViews + "</div><div class=\"k\">Toplam izlenme</div></div>");
        sb.Append("</div>");

        sb.Append("<div class=\"ed-stats-h\">En Çok İzlenen Videolar</div>");
        if (top.Count == 0) sb.Append("<div class=\"ec-empty\" style=\"padding:14px\">Henüz video yok.</div>");
        else
        {
            sb.Append("<div class=\"ed-toplist\">");
            int n = Math.Min(5, top.Count);
            for (int i = 0; i < n; i++)
            {
                sb.Append("<div class=\"ed-toprow\"><span class=\"ed-toprank\">" + (i + 1) + "</span>" +
                          "<span class=\"ed-topttl\">" + Fmt.Enc(top[i].Title(Culture)) + "</span>" +
                          "<span class=\"ed-topval\">" + top[i].ViewCount + " izlenme</span></div>");
            }
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private string BuildDocs(List<TrainingDoc> docs)
    {
        if (docs.Count == 0)
            return "<div class=\"ec-empty\">Henüz doküman eklenmemiş.</div>";

        List<string> groups = new List<string>();
        Dictionary<string, List<string>> subs = new Dictionary<string, List<string>>();
        for (int i = 0; i < docs.Count; i++)
        {
            string g = docs[i].Group(), s = docs[i].Sub();
            if (!groups.Contains(g)) { groups.Add(g); subs[g] = new List<string>(); }
            if (!subs[g].Contains(s)) subs[g].Add(s);
        }

        SortCats(groups, subs, TrainingCats.Order("Document"));
        StringBuilder sb = new StringBuilder();
        for (int gi = 0; gi < groups.Count; gi++)
        {
            string g = groups[gi];
            sb.Append("<div class=\"ec-group\" id=\"doc-grp-" + gi + "\" data-g=\"" + Fmt.Enc(g) + "\"><h3 class=\"ec-group-title\">" + Fmt.Enc(g) + "</h3></div>");
            for (int si = 0; si < subs[g].Count; si++)
            {
                string s = subs[g][si];
                List<TrainingDoc> items = new List<TrainingDoc>();
                for (int i = 0; i < docs.Count; i++) if (docs[i].Group() == g && docs[i].Sub() == s) items.Add(docs[i]);
                string title = (subs[g].Count == 1 && s == "Genel") ? g : s;
                sb.Append(Tag(DocRow("doc-" + gi + "-" + si, title, items), g, s));
            }
        }
        return sb.ToString();
    }

    // Bayi ve musteri rollerinde yalniz puan gorunur: kac kisinin puan verdigi ve "Puan yok" gosterilmez
    // (az izlenen icerige karsi onyargi olusmasin). Portal personeli sayiyi gormeye devam eder.
    private string Stars(double avg, int cnt)
    {
        bool staff = Auth.IsVendorSide;
        if (cnt <= 0) return staff ? "<span class=\"ec-nr\">Puan yok</span>" : "";
        return "<span class=\"ec-star\">\u2605</span> " + avg.ToString("0.0", CultureInfo.InvariantCulture) +
               (staff ? " <span class=\"ec-rc\">(" + cnt + ")</span>" : "");
    }

    private string Card(TrainingVideo v) { return Card(v, false); }
    /// <summary>Yuklenme tarihi (Turkiye saati): "12 Eki 2026" / "12 Oct 2026"</summary>
    private string UpDate(TrainingVideo v)
    {
        DateTime d = DateTime.SpecifyKind(v.CreatedAt, DateTimeKind.Utc).AddHours(3);
        return d.ToString("d MMM yyyy", Culture == "en" ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("tr-TR"));
    }
    private static string DurTxt(int s) { return s >= 3600 ? (s / 3600) + ":" + ((s % 3600) / 60).ToString("00") + ":" + (s % 60).ToString("00") : (s / 60) + ":" + (s % 60).ToString("00"); }
    // cont: "Devam Et" satiri karti (ucu nokta menusu: Devam Et'ten kaldir)
    private string Card(TrainingVideo v, bool cont)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"ec-card\" data-vid=\"" + Fmt.Enc(v.YoutubeId) + "\" data-mid=\"" + v.MaterialId +
                  "\" data-title=\"" + Fmt.Enc(v.Title(Culture)) + "\" data-desc=\"" + Fmt.Enc(v.Desc(Culture)) + "\" data-date=\"" + Fmt.Enc(UpDate(v)) + "\">");
        sb.Append("<div class=\"ec-thumb\">");
        sb.Append("<img src=\"https://img.youtube.com/vi/" + Fmt.Enc(v.YoutubeId) + "/hqdefault.jpg\" alt=\"\" loading=\"lazy\" />");
        sb.Append("<button type=\"button\" class=\"ec-bm\" data-mid=\"" + v.MaterialId + "\" title=\"İzleme listesine ekle\" aria-label=\"İzleme listesine ekle\"><svg width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z\"/></svg></button>");
        if (v.Completed) sb.Append("<span class=\"ec-done\">Tamamlandı</span>");
        if (v.DurationSeconds > 0) sb.Append("<span class=\"ec-dur" + (v.ProgressPercent > 0 && v.ProgressPercent < 100 ? " up" : "") + "\" aria-label=\"Süre\">" + DurTxt(v.DurationSeconds) + "</span>");
        if (v.ProgressPercent > 0 && v.ProgressPercent < 100) sb.Append("<div class=\"ec-prog\"><i style=\"width:" + v.ProgressPercent + "%\"></i></div>");
        sb.Append("<div class=\"ec-hoverinfo\"><div class=\"ec-h-title\">" + Fmt.Enc(v.Title(Culture)) + "</div>");
        string d = v.Desc(Culture);
        if (d.Length > 0) sb.Append("<div class=\"ec-h-desc\">" + Fmt.Enc(d.Length > 120 ? d.Substring(0, 120) + "…" : d) + "</div>");
        sb.Append("<div class=\"ec-h-rate\">" + Stars(v.AvgRating, v.RatingCount) + "</div></div>");
        sb.Append("</div>");
        sb.Append("<div class=\"ec-info\"><div class=\"ec-title-row\"><div class=\"ec-title\">" + Fmt.Enc(v.Title(Culture)) + "</div>" +
                  (cont ? "<button type=\"button\" class=\"ec-more\" data-mid=\"" + v.MaterialId + "\" aria-label=\"Seçenekler\" aria-haspopup=\"menu\" title=\"Seçenekler\">" +
                          "<svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><circle cx=\"5\" cy=\"12\" r=\"1.9\"/><circle cx=\"12\" cy=\"12\" r=\"1.9\"/><circle cx=\"19\" cy=\"12\" r=\"1.9\"/></svg></button>" : "") + "</div>");
        // puan + yuklenme tarihi ("★ 4,6 · 12 Eki 2026"); puan yoksa yalniz tarih
        sb.Append("<div class=\"ec-meta\"><span class=\"ec-rate\">" + Stars(v.AvgRating, v.RatingCount) + "</span>" +
                  "<time class=\"ec-date\" datetime=\"" + DateTime.SpecifyKind(v.CreatedAt, DateTimeKind.Utc).AddHours(3).ToString("yyyy-MM-dd") + "\" title=\"" + (Culture == "en" ? "Upload date" : "Yüklenme tarihi") + "\">" + Fmt.Enc(UpDate(v)) + "</time></div></div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private string Row(string id, string title, List<TrainingVideo> items) { return RowHtml(id, Fmt.Enc(title), items); }
    // titleHtml onceden kodlanmis olmali (kategori satiri: ana kategori + soluk alt kategori)
    private string RowHtml(string id, string titleHtml, List<TrainingVideo> items)
    {
        if (items.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        sb.Append("<section class=\"ec-row\" id=\"" + id + "\">");
        sb.Append("<h2 class=\"ec-row-title\">" + titleHtml + "</h2>");
        sb.Append("<div class=\"ec-rail-wrap\"><button type=\"button\" class=\"ec-arrow left\" aria-label=\"Geri\">\u2039</button>");
        sb.Append("<div class=\"ec-rail\">");
        bool cont = id == "row-continue";
        for (int i = 0; i < items.Count; i++) sb.Append(Card(items[i], cont));
        sb.Append("</div><button type=\"button\" class=\"ec-arrow right\" aria-label=\"İleri\">\u203A</button></div>");
        sb.Append("</section>");
        return sb.ToString();
    }

    /// <summary>Ust alan: "Ogren. Uygula. Uzmanlas." buyuk tipografi; sagda kisa aciklama ve gercek sayilar</summary>
    private static string BuildIntro(int cats, int vids, int docs)
    {
        StringBuilder n = new StringBuilder();
        Action<int, string> num = delegate (int v, string l) { if (v > 0) n.Append("<div><dt>" + l + "</dt><dd>" + v + "</dd></div>"); };
        num(cats, "kategori"); num(vids, "video"); num(docs, "doküman");
        string p = cats > 0 ? "River AI eğitimleri konularına göre " + cats + " kategoride. Bir kategori açın; videoları izleyin, dokümanlarını aynı yerde bulun."
                            : "Bir kategori açın; videoları izleyin, dokümanlarını aynı yerde bulun.";
        return "<section class=\"ac-intro\" id=\"ecHero\" aria-label=\"River Akademi\">" +
               "<h1 class=\"ac-intro-h\"><span>Öğren.</span><span>Uygula.</span><span class=\"hl\">Uzmanlaş.</span></h1>" +
               "<div class=\"ac-intro-r\"><p>" + Fmt.Enc(p) + "</p>" + (n.Length > 0 ? "<dl class=\"ac-intro-n\">" + n + "</dl>" : "") + "</div></section>";
    }

    // Ust serit sloganlari: sade, kisa. { vurgusuz kisim, vurgulu kelime }
    private static readonly string[][] Mottos = {
        new[] { "Öğren. Uygula. ", "Uzmanlaş." },
        new[] { "Kendi hızınızda, ", "kaldığınız yerden." },
        new[] { "Her eğitim bir sonraki adımın ", "kapısı." },
        new[] { "River AI'ı ekibinizle birlikte ", "keşfedin." },
        new[] { "Bilgi paylaştıkça ", "çoğalır." } };

    /// <summary>Kayan slogan seridi: iki kopya yan yana (kesintisiz dongu); ekran okuyucu yalniz duz metni bir kez okur</summary>
    private static string BuildMotto()
    {
        StringBuilder one = new StringBuilder(), plain = new StringBuilder();
        foreach (string[] m in Mottos)
        {
            one.Append("<span class=\"ac-m\">" + Fmt.Enc(m[0]) + "<em>" + Fmt.Enc(m[1]) + "</em></span><i class=\"ac-m-sep\"></i>");
            plain.Append(m[0] + m[1] + " ");
        }
        return "<section class=\"ac-motto\" id=\"ecHero\" aria-label=\"River Akademi\"><p class=\"sr-only\">" + Fmt.Enc(plain.ToString().Trim()) + "</p>" +
               "<div class=\"ac-motto-row\" aria-hidden=\"true\"><div class=\"ac-motto-track\">" + one + one + "</div></div></section>";
    }

    private string HeroSlide(TrainingVideo h)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"ec-hslide\" style=\"background-image:linear-gradient(90deg,rgba(9,10,16,.96),rgba(9,10,16,.7) 45%,rgba(9,10,16,.15)),url('https://img.youtube.com/vi/" + Fmt.Enc(h.YoutubeId) + "/maxresdefault.jpg')\">");
        sb.Append("<div class=\"ec-hero-in\">");
        string tag = string.IsNullOrEmpty(h.FeatureText) ? "ÖNE ÇIKAN EĞİTİM" : h.FeatureText;
        sb.Append("<div class=\"ec-hero-tag\">" + Fmt.Enc(tag) + "</div>");
        sb.Append("<h1 class=\"ec-hero-title\">" + Fmt.Enc(h.Title(Culture)) + "</h1>");
        string d = h.Desc(Culture);
        if (d.Length > 0) sb.Append("<p class=\"ec-hero-desc\">" + Fmt.Enc(d.Length > 200 ? d.Substring(0, 200) + "…" : d) + "</p>");
        sb.Append("<div class=\"ec-hero-meta\">" + Stars(h.AvgRating, h.RatingCount) + "</div>");
        sb.Append("<button type=\"button\" class=\"ec-hero-btn ec-card\" data-vid=\"" + Fmt.Enc(h.YoutubeId) + "\" data-mid=\"" + h.MaterialId +
                  "\" data-title=\"" + Fmt.Enc(h.Title(Culture)) + "\" data-desc=\"" + Fmt.Enc(h.Desc(Culture)) +
                  "\"><svg width=\"22\" height=\"22\" viewBox=\"0 0 24 24\" fill=\"currentColor\"><path d=\"M8 5v14l11-7z\"/></svg> Hemen İzle</button>");
        sb.Append("</div></div>");
        return sb.ToString();
    }

    private string BuildHero(List<TrainingVideo> videos)
    {
        if (videos.Count == 0) return "";
        // Öne çıkarılanlar varsa onları göster; yoksa en yeniler (izlenme sayısına göre sıralama yok)
        List<TrainingVideo> feat = new List<TrainingVideo>();
        for (int i = 0; i < videos.Count; i++) if (videos[i].IsFeatured) feat.Add(videos[i]);
        if (feat.Count == 0)
        {
            feat = new List<TrainingVideo>(videos);
            feat.Sort(delegate (TrainingVideo a, TrainingVideo b) { return b.CreatedAt.CompareTo(a.CreatedAt); });
        }
        int n = Math.Min(6, feat.Count);

        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"ec-hero-slider\" id=\"ecHero\"><div class=\"ec-hero-track\" id=\"ecHeroTrack\">");
        for (int i = 0; i < n; i++) sb.Append(HeroSlide(feat[i]));
        sb.Append("</div>");
        if (n > 1)
        {
            sb.Append("<button type=\"button\" class=\"ec-hero-nav prev\" id=\"ecHeroPrev\" aria-label=\"Önceki\">\u2039</button>");
            sb.Append("<button type=\"button\" class=\"ec-hero-nav next\" id=\"ecHeroNext\" aria-label=\"Sonraki\">\u203A</button>");
            sb.Append("<div class=\"ec-hero-dots\" id=\"ecHeroDots\">");
            for (int i = 0; i < n; i++) sb.Append("<button type=\"button\" class=\"ec-hdot" + (i == 0 ? " on" : "") + "\" data-i=\"" + i + "\" aria-label=\"" + (i + 1) + "\"></button>");
            sb.Append("</div>");
        }
        sb.Append("</div>");
        return sb.ToString();
    }

    private List<string> _groups = new List<string>();
    private Dictionary<string, List<string>> _subs = new Dictionary<string, List<string>>();

    private void Prepare(List<TrainingVideo> videos)
    {
        if (_groups.Count > 0) return;
        for (int i = 0; i < videos.Count; i++)
        {
            string g = videos[i].Group(), s = videos[i].Sub();
            if (!_groups.Contains(g)) { _groups.Add(g); _subs[g] = new List<string>(); }
            if (!_subs[g].Contains(s)) _subs[g].Add(s);
        }
        SortCats(_groups, _subs, TrainingCats.Order("Video"));
    }

    /// <summary>Kategorileri editorun belirledigi siraya dizer (tabloda olmayanlar mevcut sirayla sona); "Genel" alt kategori basta kalir</summary>
    private static void SortCats(List<string> groups, Dictionary<string, List<string>> subs, Dictionary<string, int> ord)
    {
        if (ord.Count == 0) return;
        List<string> g0 = new List<string>(groups);
        groups.Sort(delegate (string a, string b) { int x, y; int ia = ord.TryGetValue(a, out x) ? x : 100000 + g0.IndexOf(a), ib = ord.TryGetValue(b, out y) ? y : 100000 + g0.IndexOf(b); return ia.CompareTo(ib); });
        foreach (string g in groups)
        {
            List<string> l = subs[g], s0 = new List<string>(l); string gg = g;
            l.Sort(delegate (string a, string b)
            {
                int x, y;
                int ia = a == "Genel" ? -1 : ord.TryGetValue(gg + "\u0001" + a, out x) ? x : 100000 + s0.IndexOf(a);
                int ib = b == "Genel" ? -1 : ord.TryGetValue(gg + "\u0001" + b, out y) ? y : 100000 + s0.IndexOf(b);
                return ia.CompareTo(ib);
            });
        }
    }

    /// <summary>Bir alt kategorinin videolari, gorunen basliga gore dogal (sayi duyarli) alfabetik sirada</summary>
    private List<TrainingVideo> InSub(List<TrainingVideo> videos, string g, string s)
    {
        List<TrainingVideo> items = new List<TrainingVideo>();
        for (int i = 0; i < videos.Count; i++) if (videos[i].Group() == g && videos[i].Sub() == s) items.Add(videos[i]);
        string cul = Culture;
        items.Sort(delegate (TrainingVideo a, TrainingVideo b) { int c = Fmt.NaturalCompare(a.Title(cul), b.Title(cul)); return c != 0 ? c : a.MaterialId.CompareTo(b.MaterialId); });
        return items;
    }

    private string BuildRows(List<TrainingVideo> videos)
    {
        if (videos.Count == 0) return "<div class=\"ec-empty\">Henüz eğitim içeriği yok.</div>";
        Prepare(videos);
        StringBuilder sb = new StringBuilder();

        // Devam Et
        List<TrainingVideo> cont = new List<TrainingVideo>();
        for (int i = 0; i < videos.Count; i++) if (videos[i].ProgressPercent > 0 && videos[i].ProgressPercent < 100) cont.Add(videos[i]);
        // Öne çıkarılan videolar yalnız üstteki kapak alanında gösterilir (listede ayrı satır yok)

        sb.Append(Row("row-continue", "Devam Et", cont));

        // "En Cok Izlenenler" ve "Yeni Eklenenler" satirlari kaldirildi; liste: Devam Et + kategori satirlari

        // Kategori bazli satirlar
        for (int gi = 0; gi < _groups.Count; gi++)
        {
            string g = _groups[gi];
            // Ana kategori ayrica baslik olarak yazilmaz (satir basliginda var); menu baglantilari icin capa kalir
            sb.Append("<div class=\"ec-group ec-group-anchor\" id=\"grp-" + gi + "\" data-g=\"" + Fmt.Enc(g) + "\"></div>");
            List<string> subs = _subs[g];
            for (int si = 0; si < subs.Count; si++)
            {
                string s = subs[si];
                List<TrainingVideo> items = InSub(videos, g, s);
                // Alt kategori ana kategoriyle ayniysa ya da varsayilan "Genel" ise yalniz ana kategori;
                // degilse "Ana Kategori – Alt Kategori" (alt kategori daha soluk)
                bool same = string.Equals(s.Trim(), g.Trim(), StringComparison.CurrentCultureIgnoreCase) || s.Trim() == "Genel" || s.Trim().Length == 0;
                string titleHtml = Fmt.Enc(g) + (same ? "" : "<span class=\"ec-row-sub\"> – " + Fmt.Enc(s) + "</span>");
                sb.Append(Tag(RowHtml("row-" + gi + "-" + si, titleHtml, items), g, s));
            }
        }
        return sb.ToString();
    }

    private string BuildTree(List<TrainingVideo> videos)
    {
        if (videos.Count == 0) return "";
        Prepare(videos);
        StringBuilder sb = new StringBuilder();
        sb.Append("<a class=\"ec-tree-top\" href=\"#ec-home\">Ana Sayfa</a>");
        for (int gi = 0; gi < _groups.Count; gi++)
        {
            string g = _groups[gi];
            sb.Append("<div class=\"ec-tnode\">");
            sb.Append("<a class=\"ec-tgroup\" href=\"#grp-" + gi + "\">" + Fmt.Enc(g) + "</a>");
            List<string> subs = _subs[g];
            if (!(subs.Count == 1 && subs[0] == "Genel"))
            {
                sb.Append("<div class=\"ec-tsubs\">");
                for (int si = 0; si < subs.Count; si++)
                    sb.Append("<a href=\"#row-" + gi + "-" + si + "\">" + Fmt.Enc(subs[si]) + "</a>");
                sb.Append("</div>");
            }
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private void BuildSuggestions(List<TrainingVideo> videos)
    {
        Prepare(videos);
        StringBuilder gb = new StringBuilder(), sbs = new StringBuilder();
        List<string> seen = new List<string>();
        for (int gi = 0; gi < _groups.Count; gi++)
        {
            if (_groups[gi] != "Genel") gb.Append("<option value=\"" + Fmt.Enc(_groups[gi]) + "\"></option>");
            List<string> subs = _subs[_groups[gi]];
            for (int si = 0; si < subs.Count; si++)
                if (subs[si] != "Genel" && !seen.Contains(subs[si])) { seen.Add(subs[si]); sbs.Append("<option value=\"" + Fmt.Enc(subs[si]) + "\"></option>"); }
        }
        GroupSuggestions = gb.ToString();
        SubSuggestions = sbs.ToString();
    }

    // Rozetlerim: kazanilan sertifikalar + kazanilabilecek sinavlar (giris noktasi)
    private string BuildCertBadges()
    {
        List<CertificateInfo> certs; List<ExamInfo> exams;
        try { certs = ExamRepo.GetUserCertificates(Auth.UserId); exams = ExamRepo.GetExams(false); } catch { return ""; }
        CertCount = 0;
        Dictionary<int, bool> earned = new Dictionary<int, bool>();
        StringBuilder won = new StringBuilder(), todo = new StringBuilder();
        for (int i = 0; i < certs.Count; i++)
        {
            CertificateInfo c = certs[i];
            earned[c.ExamId] = true;
            bool dim = c.IsRevoked || c.IsExpired;
            if (!dim) CertCount++;
            won.Append("<a class=\"ec-badge" + (dim ? " dim" : "") + "\" href=\"Certificate.aspx?id=" + c.CertificateId + "\" title=\"" + Fmt.Enc(c.ExamTitle) + "\">" +
                ExamRepo.SealSvg(38, dim, c.BadgeColor) +
                "<span class=\"ec-badge-tx\"><b>" + Fmt.Enc(c.ExamTitle) + "</b><small>" +
                (c.IsRevoked ? "İptal edildi" : c.IsExpired ? "Süresi doldu · yenileyin" : c.IssuedAt.ToTrLocal().ToString("MMMM yyyy", new CultureInfo("tr-TR"))) +
                "</small></span></a>");
        }
        for (int i = 0; i < exams.Count; i++)
        {
            ExamInfo x = exams[i];
            if (earned.ContainsKey(x.ExamId)) continue;
            string st, cls;
            if (x.Status == "Published") { st = "Yakında girişe açılacak"; cls = "todo"; }
            else
            {
                List<ExamAttempt> atts = ExamRepo.GetUserAttempts(x.ExamId, Auth.UserId);
                ExamGate g = ExamRepo.Gate(x, Auth.UserId, Auth.CompanyId, atts);
                if (g.HasOpenAttempt) { st = "Devam eden sınavınız var"; cls = "ready"; }
                else if (g.Passed) { st = "Sertifikanızı oluşturun"; cls = "ready"; }
                else if (g.CanStart) { st = g.Submitted == 0 ? "Sınava girebilirsin" : "Tekrar girebilirsin"; cls = "ready"; }
                else if (!g.VideosOk) { st = "Videolar: " + g.VideosDone + "/" + g.VideosTotal; cls = "todo"; }
                else if (g.Pending) { st = "Onay bekleniyor"; cls = "todo wait"; }
                else if (g.NeedsRequest) { st = "Yeniden giriş talebi gerekli"; cls = "todo"; }
                else if (g.CooldownHoursLeft > 0) { st = "~" + g.CooldownHoursLeft + " saat sonra"; cls = "todo"; }
                else { st = g.Reason; cls = "todo"; }
            }
            todo.Append("<a class=\"ec-badge " + cls + "\" href=\"ExamTake.aspx?id=" + x.ExamId + "\" title=\"" + Fmt.Enc(x.Title) + "\">" +
                ExamRepo.SealSvg(38, false, x.BadgeColor) +
                "<span class=\"ec-badge-tx\"><b>" + Fmt.Enc(x.Title) + "</b><small>" + Fmt.Enc(st) + "</small></span></a>");
        }
        StringBuilder sb = new StringBuilder();
        if (won.Length > 0) sb.Append(won.ToString());
        if (todo.Length > 0) sb.Append((won.Length > 0 ? "<div class=\"ec-badges-sub\">Kazanılabilecek rozetler</div>" : "") + todo.ToString());
        if (sb.Length == 0)
            return "<div class=\"ec-badge empty\">" + ExamRepo.SealSvg(38, true) +
                "<span class=\"ec-badge-tx\"><b>Rozetler yakında</b><small>Sınavlar yayınlandığında burada görünecek</small></span></div>";
        return sb.ToString();
    }

    // Egitim gecmisi: egitim (grup) bazinda ilerleme kartlari + genel istatistikler
    private string BuildProgressDetailed()
    {
        bool taker = Auth.IsPartnerSide || Auth.IsCustomerSide;
        List<CertificateInfo> certs = null;
        if (taker) { try { certs = ExamRepo.GetUserCertificates(Auth.UserId); } catch { } }
        HistoryStats st = new HistoryStats(); List<VideoDetail> vids;
        string html = TrainingHistoryView.Build(Auth.UserId, Auth.CompanyId, taker, certs, true, st, out vids);
        StatCoursesTotal = st.CoursesTotal; StatCoursesDone = st.CoursesDone; StatVideosTotal = st.VideosTotal; StatVideosDone = st.VideosDone; StatRate = st.Rate;
        return html;
    }

    private string BuildProgress()
    {
        List<GroupVideo> vids = ExamRepo.GetAllVideoProgress(Auth.UserId, Auth.CompanyId);
        if (vids.Count == 0) return "";
        bool taker = Auth.IsPartnerSide || Auth.IsCustomerSide;

        // gruplara ayir (sorgu sirasi korunur)
        List<string> order = new List<string>();
        Dictionary<string, List<GroupVideo>> groups = new Dictionary<string, List<GroupVideo>>();
        int pctSum = 0;
        for (int i = 0; i < vids.Count; i++)
        {
            if (!groups.ContainsKey(vids[i].Group)) { groups[vids[i].Group] = new List<GroupVideo>(); order.Add(vids[i].Group); }
            groups[vids[i].Group].Add(vids[i]);
            StatVideosTotal++; if (vids[i].Done) StatVideosDone++;
            pctSum += vids[i].Percent;
        }
        StatRate = (int)Math.Round(pctSum / (double)vids.Count);
        StatCoursesTotal = order.Count;

        // gruba bagli sinavlar (bayi/musteri icin)
        Dictionary<string, ExamInfo> examByGroup = new Dictionary<string, ExamInfo>();
        Dictionary<int, CertificateInfo> certByExam = new Dictionary<int, CertificateInfo>();
        if (taker)
        {
            try
            {
                List<ExamInfo> ex = ExamRepo.GetExams(false);
                for (int i = 0; i < ex.Count; i++) if (!string.IsNullOrEmpty(ex[i].CategoryGroup) && !examByGroup.ContainsKey(ex[i].CategoryGroup)) examByGroup[ex[i].CategoryGroup] = ex[i];
                List<CertificateInfo> cs = ExamRepo.GetUserCertificates(Auth.UserId);
                for (int i = 0; i < cs.Count; i++) certByExam[cs[i].ExamId] = cs[i];
            }
            catch { }
        }

        StringBuilder sb = new StringBuilder();
        for (int g = 0; g < order.Count; g++)
        {
            List<GroupVideo> gv = groups[order[g]];
            int done = 0, started = 0, psum = 0, secs = 0;
            for (int i = 0; i < gv.Count; i++) { if (gv[i].Done) done++; if (gv[i].Started) started++; psum += gv[i].Percent; secs += gv[i].WatchedSeconds; }
            int rate = (int)Math.Round(psum / (double)gv.Count);
            string state = done == gv.Count ? "ok" : (started > 0 ? "wait" : "idle");
            if (state == "ok") StatCoursesDone++;
            string ic = state == "ok" ? "&#10003;" : state == "wait" ? "&#9654;" : "&#9675;";
            string label = state == "ok" ? "Tamamlandı" : state == "wait" ? "Devam ediyor" : "Başlanmadı";
            int mins = secs / 60;

            sb.Append("<div class=\"gc-item " + state + " hp-item\"><span class=\"gc-ic\">" + ic + "</span><div class=\"gc-tx\">");
            sb.Append("<div class=\"hp-top\"><b>" + Fmt.Enc(order[g]) + "</b><span class=\"hp-state " + state + "\">" + label + "</span></div>");
            sb.Append("<small>" + done + "/" + gv.Count + " video tamamlandı · izleme oranı %" + rate + (mins > 0 ? " · " + mins + " dk izlendi" : "") + "</small>");
            sb.Append("<div class=\"ex-bar\" style=\"margin-top:8px\"><i style=\"width:" + rate + "%\"></i></div>");

            // bagli sinav / sertifika durumu
            ExamInfo x;
            if (taker && examByGroup.TryGetValue(order[g], out x))
            {
                CertificateInfo c;
                string chip;
                if (certByExam.TryGetValue(x.ExamId, out c))
                    chip = "<a class=\"hp-exam won\" href=\"Certificate.aspx?id=" + c.CertificateId + "\">" + ExamRepo.SealSvg(22, false, c.BadgeColor) + "<span>Sertifika kazanıldı · " + Fmt.Enc(x.Title) + "</span></a>";
                else
                {
                    string st;
                    if (x.Status == "Published") st = "Sınav yakında girişe açılacak";
                    else
                    {
                        ExamGate eg = ExamRepo.Gate(x, Auth.UserId, Auth.CompanyId, ExamRepo.GetUserAttempts(x.ExamId, Auth.UserId));
                        st = eg.HasOpenAttempt ? "Devam eden sınavınız var" : eg.CanStart ? "Sınava girebilirsiniz" : !eg.VideosOk ? "Videoları tamamlayınca sınava girebilirsiniz" :
                             eg.Pending ? "Yeniden giriş talebiniz onay bekliyor" : eg.NeedsRequest ? "Yeniden giriş talebi gerekli" : eg.Reason;
                    }
                    chip = "<a class=\"hp-exam\" href=\"ExamTake.aspx?id=" + x.ExamId + "\">" + ExamRepo.SealSvg(22, false, x.BadgeColor) + "<span>" + Fmt.Enc(x.Title) + " · " + Fmt.Enc(st) + "</span></a>";
                }
                sb.Append(chip);
            }

            // kalan videolar
            if (state != "ok")
            {
                sb.Append("<details class=\"hp-det\"" + (state == "wait" ? " open" : "") + "><summary>Kalan videolar (" + (gv.Count - done) + ")</summary>");
                for (int i = 0; i < gv.Count; i++)
                {
                    if (gv[i].Done) continue;
                    string open = gv[i].YoutubeId.Length > 0 ? " hist-open\" data-vid=\"" + Fmt.Enc(gv[i].YoutubeId) + "\" data-mid=\"" + gv[i].MaterialId + "\" data-title=\"" + Fmt.Enc(gv[i].Title) : "";
                    sb.Append("<div class=\"gc-vid" + open + "\" role=\"button\"><span class=\"gc-vid-t\">" + Fmt.Enc(gv[i].Title) + "</span>" +
                        "<span class=\"gc-vid-p" + (gv[i].Started ? " part" : "") + "\">" + (gv[i].Started ? "%" + gv[i].Percent + " izlendi" : "Başlanmadı") + "</span>" +
                        "<span class=\"gc-vid-go\">İzle &#8594;</span></div>");
                }
                sb.Append("</details>");
            }
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }

    // Izleme listeleri icin video ozeti: mid -> youtube id, baslik, grup, ilerleme, son konum
    private string BuildVideosJson(List<TrainingVideo> videos, Dictionary<int, int> lastPos)
    {
        StringBuilder sb = new StringBuilder("{");
        for (int i = 0; i < videos.Count; i++)
        {
            TrainingVideo v = videos[i];
            int lp; lastPos.TryGetValue(v.MaterialId, out lp);
            if (i > 0) sb.Append(",");
            sb.Append("\"" + v.MaterialId + "\":{\"v\":\"" + J(v.YoutubeId) + "\",\"t\":\"" + J(v.Title(Culture)) + "\",\"g\":\"" + J(v.Group()) + "\"," +
                "\"p\":" + v.ProgressPercent + ",\"d\":" + (v.Completed ? 1 : 0) + ",\"s\":" + lp + ",\"dur\":" + v.DurationSeconds + "}");
        }
        return sb.Append("}").ToString();
    }

    private string BuildWatchJson()
    {
        List<Watchlist> ls;
        try { WatchlistRepo.EnsureDefault(Auth.UserId, Auth.CompanyId); ls = WatchlistRepo.GetVisible(Auth.UserId, Auth.CompanyId); }
        catch { return "null"; }   // tablo yoksa (migration 34 calismamis) ozellik gizlenir
        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < ls.Count; i++)
        {
            Watchlist w = ls[i];
            if (i > 0) sb.Append(",");
            sb.Append("{\"id\":" + w.ListId + ",\"n\":\"" + J(w.Name) + "\",\"def\":" + (w.IsDefault ? 1 : 0) + ",\"scope\":\"" + J(w.ShareScope) + "\"," +
                "\"rec\":" + (w.IsRecommended ? 1 : 0) + ",\"kind\":\"" + w.Kind + "\",\"owner\":\"" + J(w.OwnerName) + "\",\"items\":[" + string.Join(",", w.Items) + "]," +
                "\"shared\":[" + string.Join(",", w.SharedUserIds) + "]}");
        }
        return sb.Append("]").ToString();
    }

    private static string J(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ").Replace("<", "\\u003c").Replace(">", "\\u003e");
    }
}
