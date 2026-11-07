using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Eğitim içerik yönetimi (admin / eğitim editörü): düzenle, yayınla/kaldır, öne çıkar, sil.
public partial class TrainingManagePage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string Rows = "";
    public string GroupOptions = "";   // kategori suzgeci secenekleri
    public string FlashMsg = "";
    public int Count = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        Culture = Auth.Culture;
        UserName = Auth.FullName;

        string ok = Request.QueryString["ok"];
        if (ok == "edit") FlashMsg = "İçerik güncellendi.";
        else if (ok == "editvid") FlashMsg = "İçerik güncellendi; video bağlantısı değiştirildi.";
        else if (ok == "toggle") FlashMsg = "Yayın durumu değiştirildi.";
        else if (ok == "feature") FlashMsg = "Öne çıkarma durumu değiştirildi.";
        else if (ok == "del") FlashMsg = "İçerik silindi.";

        List<TrainingItem> items = Repo.GetTrainingManageList();
        Count = items.Count;
        Rows = Build(items);
        // kategori suzgeci: benzersiz ana kategoriler, dogal Turkce sirada
        List<string> groups = new List<string>();
        foreach (TrainingItem t in items) { string g = string.IsNullOrEmpty(t.CategoryGroup) ? "Genel" : t.CategoryGroup; if (!groups.Contains(g)) groups.Add(g); }
        groups.Sort(Fmt.NaturalCompare);
        StringBuilder go = new StringBuilder(); foreach (string g in groups) go.Append("<option value=\"" + Fmt.Enc(g) + "\">" + Fmt.Enc(g) + "</option>");
        GroupOptions = go.ToString();
    }

    private string Build(List<TrainingItem> items)
    {
        if (items.Count == 0)
            return "<tr><td colspan=\"6\" style=\"color:var(--muted)\">Henüz içerik yok.</td></tr>";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < items.Count; i++)
        {
            TrainingItem t = items[i];
            string typeLabel = t.MaterialType == "Document" ? "Doküman" : "Video";
            string cat = Fmt.Enc((string.IsNullOrEmpty(t.CategoryGroup) ? "Genel" : t.CategoryGroup) + " · " + (string.IsNullOrEmpty(t.SubCategory) ? "Genel" : t.SubCategory));
            string grp = string.IsNullOrEmpty(t.CategoryGroup) ? "Genel" : t.CategoryGroup, sb2 = string.IsNullOrEmpty(t.SubCategory) ? "Genel" : t.SubCategory;
            // istemci tarafi arama / suzme / siralama icin veri
            string hay = string.Join(" ", new[] { t.TitleTr, t.TitleEn, grp, sb2, t.DescriptionTr, t.DocOriginalName, typeLabel });
            sb.Append("<tr data-title=\"" + Fmt.Enc(t.TitleAny()) + "\" data-type=\"" + (t.MaterialType == "Document" ? "doc" : "video") + "\" data-group=\"" + Fmt.Enc(grp) + "\" data-cat=\"" + Fmt.Enc(grp + " · " + sb2) +
                      "\" data-active=\"" + (t.IsActive ? "1" : "0") + "\" data-feat=\"" + (t.IsFeatured ? "1" : "0") + "\" data-i=\"" + i + "\" data-hay=\"" + Fmt.Enc(hay) + "\">");
            sb.Append("<td>" + Fmt.Enc(t.TitleAny()) + "</td>");
            sb.Append("<td>" + typeLabel + "</td>");
            sb.Append("<td style=\"font-size:12.5px\">" + cat + "</td>");
            sb.Append("<td><span class=\"st " + (t.IsActive ? "invoiced" : "cancelled") + "\">" + (t.IsActive ? "Yayında" : "Kaldırıldı") + "</span></td>");
            sb.Append("<td>" + (t.IsFeatured ? "<span class=\"tk-prio p-high\">Öne Çıkan</span>" : "<span style=\"color:var(--dim)\">-</span>") + "</td>");
            sb.Append("<td style=\"text-align:right;white-space:nowrap\">");
            sb.Append("<a class=\"btn-ghost\" href=\"TrainingEdit.aspx?id=" + t.MaterialId + "\" style=\"padding:6px 12px\">Düzenle</a> ");
            sb.Append(Form(t.MaterialId, "feature", t.IsFeatured ? "Öne Çıkarma" : "Öne Çıkar", false));
            sb.Append(Form(t.MaterialId, "toggle", t.IsActive ? "Yayından Kaldır" : "Yayınla", false));
            sb.Append(Form(t.MaterialId, "delete", "Sil", true));
            sb.Append("</td></tr>");
        }
        return sb.ToString();
    }

    private string Form(int id, string action, string label, bool danger)
    {
        string style = "padding:6px 12px" + (danger ? ";border-color:rgba(255,61,113,.4);color:#FF9DB5" : "");
        string confirm = danger ? " onsubmit=\"return confirm('Bu içeriği silmek istediğinize emin misiniz?');\"" : "";
        return "<form method=\"post\" action=\"TrainingItemAction.aspx\" style=\"display:inline\"" + confirm + ">" +
               "<input type=\"hidden\" name=\"id\" value=\"" + id + "\" /><input type=\"hidden\" name=\"action\" value=\"" + action + "\" />" +
               "<button type=\"submit\" class=\"btn-ghost\" style=\"" + style + "\">" + label + "</button></form> ";
    }
}
