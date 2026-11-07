using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Versiyon Duyuruları — herkes (müşteri + iş ortağı + vendor) görür; içerik girişi eğitim editörü/admin.
public partial class VersionsPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CompanyName = "";
    public bool IsVendor = false;
    public bool IsCustomer = false;
    public bool IsSupport = false;
    public bool IsEditorOnly = false;
    public bool CanEdit = false;
    public string FlashMsg = "";
    public string PublicHtml = "";
    public string ManageHtml = "";
    public string ModulesJson = "[]";   // form: modul listesi
    public bool OpenList = false;       // taslak kaydedilince Versiyon Listesi penceresi acik gelir

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        Culture = Auth.Culture;
        UserName = Auth.FullName;
        CompanyName = Auth.CompanyName;
        IsVendor = Auth.IsVendor;
        IsCustomer = Auth.IsCustomerSide;
        IsSupport = Auth.IsSupportAgent;
        IsEditorOnly = Auth.IsTrainingEditor;
        CanEdit = Auth.CanEditTraining;

        string ok = Request.QueryString["ok"];
        if (ok == "saved") { FlashMsg = "Duyuru taslağı kaydedildi. Yayınlamak için Versiyon Listesi'ndeki 'Yayınla ve Duyur' düğmesini kullanın."; OpenList = true; }
        else if (ok == "pub") FlashMsg = "Duyuru yayınlandı ve tüm kullanıcılara bildirim gönderildi.";
        else if (ok == "del") FlashMsg = "Duyuru silindi.";

        PublicHtml = BuildPublic(Repo.GetPublishedVersions());
        if (CanEdit)
        {
            ManageHtml = BuildManage(Repo.GetAllVersions());
            ModulesJson = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(VersionModuleRepo.GetAll());
        }
    }

    private static string D(DateTime? d)
    {
        return d.HasValue ? d.Value.ToString("dd.MM.yyyy", new CultureInfo("tr-TR")) : "-";
    }

    private string HighlightsList(string h)
    {
        // Kategori isaretli mi? (satir "## " ile baslar)
        if (string.IsNullOrEmpty(h)) return "";
        string[] lines = h.Replace("\r", "").Split('\n');
        bool hasCats = false;
        for (int i = 0; i < lines.Length; i++) if (lines[i].TrimStart().StartsWith("## ")) { hasCats = true; break; }

        StringBuilder sb = new StringBuilder();
        if (!hasCats)
        {
            sb.Append("<h4 class=\"va-hl-h\">" + (Culture == "en" ? "What\u2019s New" : "Yenilikler") + "</h4><ul class=\"va-hl\">");
            for (int i = 0; i < lines.Length; i++) { string ln = lines[i].Trim(); if (ln.Length == 0) continue; sb.Append("<li>" + Fmt.Enc(ln) + "</li>"); }
            sb.Append("</ul>");
            return sb.ToString();
        }

        // Arama icin: her kategori bir .va-sec, her madde data-cat tasir; ilk 6 madde disindakiler katlanir (.va-x)
        bool inSec = false; string cat = ""; int n = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string ln = lines[i].Trim();
            if (ln.Length == 0) continue;
            if (ln.StartsWith("## "))
            {
                if (inSec) sb.Append("</ul></div>");
                string mod; VersionModuleRepo.SplitHeader(ln.Substring(3), out cat, out mod);   // "## Kategori | Modul"
                sb.Append("<div class=\"va-sec\" data-cat=\"" + Fmt.Enc(cat) + "\" data-mod=\"" + Fmt.Enc(mod) + "\"><h4 class=\"va-hl-h\">" + Fmt.Enc(cat) +
                          (mod.Length > 0 ? "<span class=\"va-mod\">" + Fmt.Enc(mod) + "</span>" : "") + "</h4><ul class=\"va-hl\">");
                inSec = true;
            }
            else
            {
                if (!inSec) { sb.Append("<div class=\"va-sec\" data-cat=\"\"><ul class=\"va-hl\">"); inSec = true; }
                sb.Append("<li" + (n >= 6 ? " class=\"va-x\"" : "") + ">" + Fmt.Enc(ln) + "</li>");
                n++;
            }
        }
        if (inSec) sb.Append("</ul></div>");
        LastCount = n;
        return sb.ToString();
    }
    private int LastCount = 0;

    private string BuildPublic(List<VersionAnnouncement> list)
    {
        if (list.Count == 0)
            return "<div class=\"va-empty\">" + (Culture=="en" ? "No published version announcements yet." : "Henüz yayınlanmış bir versiyon duyurusu yok.") + "</div>";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            VersionAnnouncement v = list[i];
            bool shortType = !string.IsNullOrEmpty(v.BodyText) && v.BodyText.Trim().Length <= 24;   // or. "Ara Surum" -> etiket
            sb.Append("<article class=\"va-card\" data-ver=\"" + Fmt.Enc(v.VersionNo) + "\" data-prod=\"" + Fmt.Enc(v.ProductName) + "\">");
            sb.Append("<div class=\"va-top\"><div class=\"va-title\"><span class=\"va-prod\">" + Fmt.Enc(v.ProductName) +
                      "</span><span class=\"va-ver\">v" + Fmt.Enc(v.VersionNo) + "</span>" +
                      (shortType ? "<span class=\"va-type\">" + Fmt.Enc(v.BodyText.Trim()) + "</span>" : "") + "</div></div>");
            sb.Append("<div class=\"va-meta\">");
            sb.Append("<div class=\"va-meta-item\"><span class=\"va-k\">" + (Culture=="en" ? "Release Date" : "Yayın Tarihi") + "</span><span class=\"va-v\">" + D(v.ReleaseDate) + "</span></div>");
            sb.Append("<div class=\"va-meta-item\"><span class=\"va-k\">" + (Culture=="en" ? "Access Date" : "Erişime Açılma") + "</span><span class=\"va-v\">" + D(v.AccessDate) + "</span></div>");
            sb.Append("</div>");
            if (!string.IsNullOrEmpty(v.BodyText) && !shortType)
                sb.Append("<p class=\"va-body\">" + Fmt.Enc(v.BodyText) + "</p>");
            if (!string.IsNullOrEmpty(v.Highlights))
            {
                LastCount = 0;
                sb.Append("<div class=\"va-hl-wrap\">" + HighlightsList(v.Highlights) + "</div>");
                if (LastCount > 6) sb.Append("<button type=\"button\" class=\"va-more\" data-n=\"" + LastCount + "\">Tüm notları göster (" + LastCount + ")</button>");
            }
            sb.Append("</article>");
        }
        return sb.ToString();
    }

    private string BuildManage(List<VersionAnnouncement> list)
    {
        if (list.Count == 0) return "<tr><td colspan=\"6\" class=\"va-lempty\">Henüz versiyon duyurusu yok. \"Yeni Versiyon\" ile ilk duyuruyu ekleyin.</td></tr>";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            VersionAnnouncement v = list[i];
            sb.Append("<tr>");
            sb.Append("<td data-l=\"Ürün\"><span class=\"va-ptag\">" + Fmt.Enc(v.ProductName) + "</span></td>");
            sb.Append("<td data-l=\"Versiyon\"><b>v" + Fmt.Enc(v.VersionNo) + "</b></td>");
            List<string> mods = ModulesOf(v.Highlights);
            sb.Append("<td data-l=\"Modüller\">");
            if (mods.Count == 0) sb.Append("<span class=\"va-dim\">-</span>");
            for (int k = 0; k < mods.Count; k++) sb.Append("<span class=\"va-mod\">" + Fmt.Enc(mods[k]) + "</span>");
            sb.Append("</td>");
            sb.Append("<td data-l=\"Yayın Tarihi\">" + D(v.ReleaseDate) + "</td>");
            sb.Append("<td data-l=\"Durum\"><span class=\"st " + (v.IsPublished ? "invoiced" : "cancelled") + "\">" + (v.IsPublished ? "Yayında" : "Taslak") + "</span></td>");
            sb.Append("<td class=\"va-lacts\">");
            if (!v.IsPublished)
                sb.Append(PublishForm(v.VersionId, "Bu duyuru seçtiğiniz kanallardan gönderilecek. Onaylıyor musunuz?"));
            sb.Append(ActForm(v.VersionId, "delete", "Sil", true, "Bu duyuruyu silmek istediğinize emin misiniz?"));
            sb.Append("</td></tr>");
        }
        return sb.ToString();
    }

    /// <summary>Duyurunun bolum basliklarindaki moduller (tekil, sirasiyla)</summary>
    private static List<string> ModulesOf(string h)
    {
        List<string> l = new List<string>();
        if (string.IsNullOrEmpty(h)) return l;
        foreach (string raw in h.Replace("\r", "").Split('\n'))
        {
            string ln = raw.Trim(); if (!ln.StartsWith("## ")) continue;
            string cat, mod; VersionModuleRepo.SplitHeader(ln.Substring(3), out cat, out mod);
            if (mod.Length > 0 && !l.Exists(delegate (string x) { return string.Equals(x, mod, StringComparison.OrdinalIgnoreCase); })) l.Add(mod);
        }
        return l;
    }

    private string ActForm(int id, string action, string label, bool danger, string confirm)
    {
        string style = "padding:6px 12px" + (danger ? ";border-color:rgba(255,61,113,.4);color:#FF9DB5" : "");
        return "<form method=\"post\" action=\"VersionAction.aspx\" style=\"display:inline\" onsubmit=\"return confirm('" + confirm + "');\">" +
               "<input type=\"hidden\" name=\"id\" value=\"" + id + "\" /><input type=\"hidden\" name=\"action\" value=\"" + action + "\" />" +
               "<button type=\"submit\" class=\"btn-ghost\" style=\"" + style + "\">" + label + "</button></form> ";
    }

    private string PublishForm(int id, string confirm)
    {
        return "<form method=\"post\" action=\"VersionAction.aspx\" class=\"pub-form\" onsubmit=\"return confirm('" + confirm + "');\">" +
               "<input type=\"hidden\" name=\"id\" value=\"" + id + "\" /><input type=\"hidden\" name=\"action\" value=\"publish\" /><input type=\"hidden\" name=\"chsel\" value=\"1\" />" +
               "<div class=\"pub-ch\">" +
               "<label class=\"ch-mini\"><input type=\"checkbox\" name=\"ch_mail\" checked=\"checked\" /> Mail Bildirimi</label>" +
               "<label class=\"ch-mini\"><input type=\"checkbox\" name=\"ch_app\" checked=\"checked\" /> Uygulama içi bildirim</label>" +
               "</div>" +
               "<button type=\"submit\" class=\"btn-ghost\" style=\"padding:6px 12px\">Yayınla ve Duyur</button></form> ";
    }
}
