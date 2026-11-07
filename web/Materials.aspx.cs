using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Satış Materyal ve İçerikleri: vendor yayınlar (PDF/sunum/doküman), tüm iş ortakları görür/indirir.
public partial class MaterialsPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public bool IsVendor = false;
    public string GroupsHtml = "";
    public string GroupSuggestions = "";
    public string FlashMsg = "";
    public int Count = 0;
    // yeni duzen (59. migration): ince dokulu kartlar, arama, yonetim paneli
    public bool NewLayout = false;
    public string Intro = SalesMats.DefaultIntro;
    public string CatOptions = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (Auth.IsPartnerUserLimited) { Response.Redirect(Auth.HomeUrl); return; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        IsVendor = Auth.IsVendor;
        bool en = Culture == "en";

        string ok = Request.QueryString["ok"];
        if (ok == "add") FlashMsg = en ? "Material published." : "Materyal yayınlandı.";
        else if (ok == "del") FlashMsg = en ? "Material removed." : "Materyal kaldırıldı.";

        if (SalesMats.Ready())
        {
            NewLayout = true;
            List<SMat> all = SalesMats.All();
            Count = all.Count;
            Intro = SalesMats.Intro();
            GroupsHtml = BuildNew(all);
            StringBuilder co = new StringBuilder();
            foreach (SMatCat c in SalesMats.Categories()) co.Append("<option value=\"" + Fmt.Enc(c.Name) + "\">" + Fmt.Enc(c.Name) + "</option>");
            CatOptions = co.ToString();
            return;
        }
        List<SalesMaterial> mats = Repo.GetSalesMaterials();
        Count = mats.Count;
        GroupsHtml = Build(mats);

        List<string> gg = new List<string>();
        for (int i = 0; i < mats.Count; i++)
        {
            string g = mats[i].Group();
            if (g != "Genel" && !gg.Contains(g)) gg.Add(g);
        }
        StringBuilder gb = new StringBuilder();
        for (int i = 0; i < gg.Count; i++) gb.Append("<option value=\"" + Fmt.Enc(gg[i]) + "\"></option>");
        GroupSuggestions = gb.ToString();
    }

    private string SizeStr(long bytes)
    {
        if (bytes <= 0) return "";
        double mb = bytes / 1048576.0;
        if (mb >= 1) return mb.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " MB";
        double kb = bytes / 1024.0;
        return kb.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " KB";
    }

    /// <summary>Yeni duzen: kategori kategori ince kartlar. Kart: dokulu tema, sag alttan tasan egik ikon;
    /// uzerine gelince tek cumlelik ozet + Goruntule / Bana gonder. data-q: arama metni (Turkce sadelestirilmis).</summary>
    private string BuildNew(List<SMat> all)
    {
        bool en = Culture == "en";
        if (all.Count == 0) return "<div class=\"vid-empty\">" + (en ? "No materials yet." : "Henüz materyal eklenmemiş.") + "</div>";
        StringBuilder sb = new StringBuilder();
        string cur = null; int idx = 0, inGroup = 0;
        Func<string, int> groupCount = delegate (string g) { int n = 0; foreach (SMat x in all) if (x.Group == g) n++; return n; };
        foreach (SMat m in all)
        {
            if (m.Group != cur)
            {
                if (cur != null) sb.Append("</div></section>");
                cur = m.Group; inGroup = 0;
                sb.Append("<section class=\"sm-group\"><h2 class=\"sm-gt\">" + Fmt.Enc(cur) + "<span>" + groupCount(cur) + "</span></h2><div class=\"sm-grid\">");
            }
            int t = m.Tint >= 0 ? m.Tint : idx % 15; idx++; inGroup++;
            string icon = m.Icon ?? SalesMats.AutoIcon(m), ext = m.Ext(), size = SizeStr(m.Size);
            string type = m.External ? (en ? "Link" : "Bağlantı") : ext.ToUpperInvariant();
            string title = m.Title(Culture);
            string q = SalesMats.Fold(title + " " + (m.TitleEn ?? "") + " " + (m.Summary ?? "") + " " + m.Group + " " + (m.Original ?? ""));
            sb.Append("<article class=\"sm-card acg acg-" + t + "\" data-q=\"" + Fmt.Enc(q) + "\">");
            sb.Append("<span class=\"sm-ico\" aria-hidden=\"true\"><svg><use href=\"assets/caticons.svg#i-" + icon + "\"></use></svg></span>");
            // ust satir: tur ve boyut (kategori zaten bolum basliginda; aramada kullanilir)
            sb.Append("<div class=\"sm-k\">" + Fmt.Enc(type) + (size.Length > 0 ? "<span>" + size + "</span>" : "") + "</div>");
            sb.Append("<h3 class=\"sm-t\">" + Fmt.Enc(title) + "</h3>");
            sb.Append("<div class=\"sm-hv\">");
            if (!string.IsNullOrEmpty(m.Summary)) sb.Append("<p class=\"sm-sum\">" + Fmt.Enc(m.Summary) + "</p>");
            sb.Append("<div class=\"sm-act\">");
            string kind = ext == "pdf" ? "pdf" : (ext == "png" || ext == "jpg" || ext == "jpeg" ? "image" : "");
            if (m.External)
                sb.Append("<a class=\"sm-b sm-log\" data-id=\"" + m.Id + "\" href=\"" + Fmt.Enc(m.Href()) + "\" target=\"_blank\" rel=\"noopener\">" + (en ? "Open" : "Görüntüle") + "</a>");
            else if (kind.Length > 0)
                sb.Append("<button type=\"button\" class=\"sm-b mat-view sm-log\" data-id=\"" + m.Id + "\" data-src=\"" + Fmt.Enc(m.Href()) + "\" data-kind=\"" + kind + "\" data-name=\"" + Fmt.Enc(title) +
                          "\" data-file=\"" + Fmt.Enc(m.Original) + "\" data-size=\"" + Fmt.Enc(size) + "\">" + (en ? "View" : "Görüntüle") + "</button>");
            else
                sb.Append("<a class=\"sm-b sm-log\" data-id=\"" + m.Id + "\" href=\"" + Fmt.Enc(m.Href()) + "\" download=\"" + Fmt.Enc(m.Original) + "\">" + (en ? "Download" : "İndir") + "</a>");
            sb.Append("<button type=\"button\" class=\"sm-b sm-send\" data-id=\"" + m.Id + "\">" + (en ? "Send to me" : "Bana gönder") + "</button>");
            sb.Append("</div></div></article>");
        }
        sb.Append("</div></section>");
        return sb.ToString();
    }

    private string Build(List<SalesMaterial> mats)
    {
        bool en = Culture == "en";
        if (mats.Count == 0)
            return "<div class=\"vid-empty\">" + (en ? "No materials yet." : "Henüz materyal eklenmemiş.") + "</div>";

        List<string> groups = new List<string>();
        for (int i = 0; i < mats.Count; i++)
        {
            string g = mats[i].Group();
            if (!groups.Contains(g)) groups.Add(g);
        }

        StringBuilder sb = new StringBuilder();
        for (int gi = 0; gi < groups.Count; gi++)
        {
            string g = groups[gi];
            sb.Append("<div class=\"mat-group\">");
            sb.Append("<h2 class=\"tr-group-title\">" + Fmt.Enc(g) + "</h2>");
            sb.Append("<div class=\"mat-grid\">");
            for (int i = 0; i < mats.Count; i++)
            {
                SalesMaterial m = mats[i];
                if (m.Group() != g) continue;
                string size = SizeStr(m.FileSize);
                sb.Append("<div class=\"mat-card\">");
                sb.Append("<div class=\"mat-ic\"><svg width=\"22\" height=\"22\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z\"></path><polyline points=\"14 2 14 8 20 8\"></polyline></svg></div>");
                sb.Append("<div class=\"mat-body\">");
                sb.Append("<div class=\"mat-title\">" + Fmt.Enc(m.Title(Culture)) + "</div>");
                sb.Append("<div class=\"mat-meta\" title=\"" + Fmt.Enc(m.OriginalName) + "\"><span class=\"mat-fn\">" + Fmt.Enc(m.OriginalName) + "</span>" + (size.Length > 0 ? "<span class=\"mat-sz\"> · " + size + "</span>" : "") + "</div>");
                sb.Append("</div>");
                sb.Append("<div class=\"mat-act\">");
                if (m.IsExternal)
                    sb.Append("<a class=\"btn-cyan\" href=\"" + Fmt.Enc(m.Href()) + "\" target=\"_blank\" rel=\"noopener\">" + (en ? "Open" : "Aç") + "</a>");
                else
                {
                    // PDF ve resimler uygulama icinde goruntulenir (js/materials.js)
                    string ext = (System.IO.Path.GetExtension(m.OriginalName ?? m.StoredName ?? "") ?? "").ToLowerInvariant();
                    string kind = ext == ".pdf" ? "pdf" : (ext == ".png" || ext == ".jpg" || ext == ".jpeg" ? "image" : "");
                    if (kind.Length > 0)
                        sb.Append("<button type=\"button\" class=\"btn-ghost mat-view\" data-src=\"" + Fmt.Enc(m.Href()) + "\" data-kind=\"" + kind + "\" data-name=\"" + Fmt.Enc(m.Title(Culture)) +
                                  "\" data-file=\"" + Fmt.Enc(m.OriginalName) + "\" data-size=\"" + Fmt.Enc(size) + "\">" + (en ? "View" : "Görüntüle") + "</button>");
                    sb.Append("<a class=\"btn-cyan\" href=\"" + Fmt.Enc(m.Href()) + "\" download=\"" + Fmt.Enc(m.OriginalName) + "\">" + (en ? "Download" : "İndir") + "</a>");
                }
                if (IsVendor)
                {
                    sb.Append("<form method=\"post\" action=\"DeleteSalesMaterial.aspx\" style=\"margin:0\">");
                    sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + m.MaterialId + "\" />");
                    sb.Append("<button type=\"submit\" class=\"btn-ghost mat-del\">" + (en ? "Remove" : "Kaldır") + "</button>");
                    sb.Append("</form>");
                }
                sb.Append("</div>");
                sb.Append("</div>");
            }
            sb.Append("</div></div>");
        }
        return sb.ToString();
    }
}
