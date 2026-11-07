using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

// Eğitim içeriği düzenleme: başlık, açıklama, kategori, öne çıkarma + metni; videolarda bağlantı (YouTube / embed kodu).
public partial class TrainingEditPage : Page
{
    public string Culture = "tr";
    public int MaterialId = 0;
    public string TypeLabel = "";
    public string TitleTr = "";
    public string TitleEn = "";
    public string DescriptionTr = "";
    public string CategoryGroup = "";
    public string SubCategory = "";
    public bool IsFeatured = false;
    public string FeatureText = "";
    public string FlashErr = "";
    // video baglantisi: mevcut YouTube kimligi, formdaki deger, bilgi tabaninda kaynak olarak kullanilma sayisi
    public bool IsVideo = false;
    public string YoutubeIdCur = "", VideoUrl = "";
    public int KbCount = 0;
    public string CoverCur = "";   // dokuman kapagi (assets/traincovers)
    // kategori secimi: Kategoriler sekmesinde tanimli ana / alt kategoriler (icerik turune gore); tablo yoksa eski metin kutulari
    public bool UseSelects = false;
    public string GroupOptions = "", SubOptions = "";
    private string _scope = "Video";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.CanEditTraining) { Response.Redirect("Training.aspx"); return; }
        Culture = Auth.Culture;

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Save(); return; }

        int id;
        if (!int.TryParse(Request.QueryString["id"], out id)) { Response.Redirect("TrainingManage.aspx"); return; }
        Load(id);
    }

    private void Load(int id)
    {
        TrainingItem t = Repo.GetTrainingItem(id);
        if (t == null) { Response.Redirect("TrainingManage.aspx"); return; }
        MaterialId = t.MaterialId;
        TypeLabel = t.MaterialType == "Document" ? "Doküman" : "Video";
        TitleTr = t.TitleTr; TitleEn = t.TitleEn; DescriptionTr = t.DescriptionTr;
        CategoryGroup = t.CategoryGroup; SubCategory = t.SubCategory;
        IsFeatured = t.IsFeatured; FeatureText = t.FeatureText;
        _scope = t.MaterialType == "Document" ? "Document" : "Video";
        SetVideo(t, null);
        BuildOptions(t.CategoryGroup, t.SubCategory);
    }

    private void SetVideo(TrainingItem t, string formUrl)
    {
        IsVideo = t.MaterialType != "Document";
        CoverCur = IsVideo ? "" : (t.CoverImage ?? "");
        if (!IsVideo) return;
        YoutubeIdCur = t.YoutubeId ?? "";
        VideoUrl = formUrl ?? (YoutubeIdCur.Length > 0 ? "https://www.youtube.com/watch?v=" + YoutubeIdCur : "");
        KbCount = Repo.KbEntryCountForMaterial(t.MaterialId);
    }

    /// <summary>YouTube kimligi: baglanti, kisa baglanti, shorts ya da embed kodu (iframe); tam 11 karakter olmali</summary>
    private static string ParseYid(string url)
    {
        string y = Fmt.YouTubeId(url);
        return y.Length == 11 ? y : "";
    }

    private static string Norm(string s) { s = (s ?? "").Trim(); return s.Length == 0 ? "Genel" : s; }

    /// <summary>Secenekler: tanimli kategoriler (editorun sirasiyla); icerigin mevcut kategorisi listede yoksa o da eklenir (Kaydet istemeden degistirmesin)</summary>
    private void BuildOptions(string curG, string curS)
    {
        if (!TrainingCats.TableExists()) return;
        UseSelects = true;
        List<TCat> mains = TrainingCats.List(_scope);
        string g0 = Norm(curG), s0 = Norm(curS);
        bool gFound = false, sFound = TrainingCats.Fold(s0) == "genel";
        StringBuilder go = new StringBuilder(), so = new StringBuilder();
        foreach (TCat m in mains)
        {
            bool gs = TrainingCats.Fold(m.Name) == TrainingCats.Fold(g0); if (gs) gFound = true;
            go.Append("<option value=\"" + Fmt.Enc(m.Name) + "\"" + (gs ? " selected=\"selected\"" : "") + ">" + Fmt.Enc(m.Name) + "</option>");
            foreach (TCat s in m.Subs)
            {
                bool ss = gs && TrainingCats.Fold(s.Name) == TrainingCats.Fold(s0); if (ss) sFound = true;
                so.Append("<option value=\"" + Fmt.Enc(s.Name) + "\" data-g=\"" + Fmt.Enc(m.Name) + "\"" + (ss ? " selected=\"selected\"" : "") + ">" + Fmt.Enc(s.Name) + "</option>");
            }
        }
        if (!gFound) go.Insert(0, "<option value=\"" + Fmt.Enc(g0) + "\" selected=\"selected\">" + Fmt.Enc(g0) + " (mevcut)</option>");
        if (!sFound) so.Insert(0, "<option value=\"" + Fmt.Enc(s0) + "\" data-g=\"" + Fmt.Enc(g0) + "\" selected=\"selected\">" + Fmt.Enc(s0) + " (mevcut)</option>");
        GroupOptions = go.ToString(); SubOptions = so.ToString();
    }

    /// <summary>Sunucu dogrulamasi: secim tanimli kategorilerden biri (ya da icerigin mevcut kategorisi) olmali</summary>
    private bool ValidChoice(string grp, string sub, TrainingItem t)
    {
        string g = TrainingCats.Fold(Norm(grp)), s = TrainingCats.Fold(Norm(sub));
        bool same = g == TrainingCats.Fold(Norm(t.CategoryGroup)) && (s == "genel" || s == TrainingCats.Fold(Norm(t.SubCategory)));
        if (same) return true;
        foreach (TCat m in TrainingCats.List(_scope))
            if (TrainingCats.Fold(m.Name) == g)
            {
                if (s == "genel") return true;
                foreach (TCat x in m.Subs) if (TrainingCats.Fold(x.Name) == s) return true;
                return false;
            }
        return false;
    }

    private void Save()
    {
        int id;
        if (!int.TryParse(Request.Form["id"], out id)) { Response.Redirect("TrainingManage.aspx"); return; }
        TrainingItem t = Repo.GetTrainingItem(id);
        if (t == null) { Response.Redirect("TrainingManage.aspx"); return; }

        string titleTr = (Request.Unvalidated.Form["titletr"] ?? "").Trim();
        string titleEn = (Request.Unvalidated.Form["titleen"] ?? "").Trim();
        string desc = (Request.Unvalidated.Form["description"] ?? "").Trim();
        string grp = (Request.Unvalidated.Form["categorygroup"] ?? "").Trim();
        string sub = (Request.Unvalidated.Form["subcategory"] ?? "").Trim();
        bool featured = Request.Form["featured"] == "on" || Request.Form["featured"] == "1";
        string featureText = (Request.Unvalidated.Form["featuretext"] ?? "").Trim();
        bool isVideo = t.MaterialType != "Document";
        string url = (Request.Unvalidated.Form["url"] ?? "").Trim();
        string yid = isVideo ? ParseYid(url) : "";

        _scope = t.MaterialType == "Document" ? "Document" : "Video";
        bool badCat = TrainingCats.TableExists() && !ValidChoice(grp, sub, t);
        bool badUrl = isVideo && yid.Length == 0;
        // dokuman kapagi: yeni gorsel (png/jpg/webp, en fazla 5 MB) ya da kaldirma
        HttpPostedFile cf = isVideo ? null : Request.Files["cover"];
        bool hasCover = cf != null && cf.ContentLength > 0, removeCover = !isVideo && Request.Form["coverremove"] == "1";
        string cext = hasCover ? Path.GetExtension(cf.FileName ?? "").ToLowerInvariant() : "";
        string badCover = !hasCover ? null : (cext != ".png" && cext != ".jpg" && cext != ".jpeg" && cext != ".webp") ? "Kapak görseli PNG, JPG ya da WebP olmalı."
                        : cf.ContentLength > 5 * 1024 * 1024 ? "Kapak görseli en fazla 5 MB olabilir." : null;
        if (titleTr.Length == 0 || badCat || badUrl || badCover != null)
        {
            FlashErr = badCat ? "Kategori listede yok. Yeni kategoriyi önce Editör Paneli › Kategoriler sekmesinde ekleyin." : titleTr.Length == 0 ? "Başlık zorunludur." :
                badUrl ? "Video bağlantısından YouTube videosu bulunamadı. YouTube bağlantısını ya da embed kodunun tamamını yapıştırın." : badCover;
            SetVideo(t, url);
            BuildOptions(badCat ? t.CategoryGroup : grp, badCat ? t.SubCategory : sub);
            MaterialId = t.MaterialId; TypeLabel = t.MaterialType == "Document" ? "Doküman" : "Video";
            TitleTr = titleTr; TitleEn = titleEn; DescriptionTr = desc; CategoryGroup = grp; SubCategory = sub;
            IsFeatured = featured; FeatureText = featureText;
            return;
        }
        if (grp.Length == 0) grp = "Genel";
        if (sub.Length == 0) sub = "Genel";

        Repo.UpdateTrainingBasics(id, titleTr, titleEn, desc, grp, sub);
        TrainingCats.Ensure(t.MaterialType == "Document" ? "Document" : "Video", grp, sub);
        Repo.SetTrainingFeatured(id, featured, featureText);
        if (hasCover || removeCover)
        {
            string dir = Server.MapPath("~/assets/traincovers"), newName = null;
            if (hasCover) { Directory.CreateDirectory(dir); newName = Guid.NewGuid().ToString("N") + cext; cf.SaveAs(Path.Combine(dir, newName)); }
            Repo.UpdateTrainingCover(id, newName);
            // eski kapak dosyasi: ayni dosyayi kullanan baska icerik yoksa sil (dosya adi guvenli: yalniz ad, klasor disina cikamaz)
            string old = Path.GetFileName(t.CoverImage ?? "");
            if (old.Length > 0) { try { string op = Path.Combine(dir, old); if (File.Exists(op)) File.Delete(op); } catch { } }
        }
        bool vidChanged = isVideo && !string.Equals(yid, t.YoutubeId ?? "", StringComparison.Ordinal);
        if (vidChanged) { Repo.UpdateTrainingVideoId(id, yid); try { Kb.Invalidate(); } catch { } }
        Response.Redirect("TrainingManage.aspx?ok=" + (vidChanged ? "editvid" : "edit"));
    }
}
