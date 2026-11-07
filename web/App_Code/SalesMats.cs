using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

// Satis Materyalleri (genisletilmis): ozet, sira, renk temasi, ikon; kategori sirasi; goruntuleme / gonderim kayitlari; sayfa metni.
// 59. migration calistirilmamissa Ready() false doner ve sayfa eski duzende calisir.
public class SMat
{
    public int Id, Tint = -1, Sort, Views, Sends; public string TitleTr, TitleEn, Group, Stored, Original, ContentType, Summary, Icon; public bool External; public long Size; public DateTime CreatedAt;
    public string Title(string c) { return c == "en" && !string.IsNullOrEmpty(TitleEn) ? TitleEn : TitleTr; }
    public string Href() { return External ? Stored : "materials/" + Stored; }
    public string Ext() { return External ? "" : (System.IO.Path.GetExtension(Original ?? Stored ?? "") ?? "").ToLowerInvariant().TrimStart('.'); }
}
public class SMatCat { public int Id, Sort; public string Name; }

public static class SalesMats
{
    public const string IntroKey = "materials.intro";
    public const string DefaultIntro = "River AI sunumları, tanıtım dokümanları ve satış içerikleri. Tüm iş ortakları için.";
    public static readonly CultureInfo TR = new CultureInfo("tr-TR");
    public static string Fold(string s) { return (s ?? "").Trim().ToLower(TR).Replace('ı', 'i'); }

    public static bool Ready()
    {
        try { using (SqlConnection con = Db.Open()) using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH('dbo.SalesMaterials','Summary') IS NOT NULL AND OBJECT_ID('dbo.SalesMaterialCategories','U') IS NOT NULL AND OBJECT_ID('dbo.SalesMaterialEvents','U') IS NOT NULL AND OBJECT_ID('dbo.SiteTexts','U') IS NOT NULL THEN 1 ELSE 0 END", con)) return (int)cmd.ExecuteScalar() == 1; }
        catch { return false; }
    }

    public static List<SMatCat> Categories()
    {
        List<SMatCat> l = new List<SMatCat>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT CategoryId, Name, SortOrder FROM dbo.SalesMaterialCategories ORDER BY SortOrder, CategoryId", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read()) { SMatCat c = new SMatCat(); c.Id = r.GetInt32(0); c.Name = r.GetString(1); c.Sort = r.GetInt32(2); l.Add(c); }
        return l;
    }

    /// <summary>Etkin materyaller: kategori sirasi, sonra materyal sirasi; goruntuleme ve gonderim sayilariyla</summary>
    public static List<SMat> All()
    {
        EnsureCategories();
        List<SMat> l = new List<SMat>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT m.MaterialId, m.TitleTr, m.TitleEn, ISNULL(NULLIF(m.CategoryGroup, N''), N'Genel'), m.StoredName, m.OriginalName, m.ContentType, m.IsExternal, m.FileSize, m.CreatedAt, " +
            "m.Summary, m.SortOrder, m.Tint, m.Icon, " +
            "(SELECT COUNT(*) FROM dbo.SalesMaterialEvents e WHERE e.MaterialId = m.MaterialId AND e.Kind = N'view'), " +
            "(SELECT COUNT(*) FROM dbo.SalesMaterialEvents e WHERE e.MaterialId = m.MaterialId AND e.Kind = N'send') " +
            "FROM dbo.SalesMaterials m LEFT JOIN dbo.SalesMaterialCategories c ON c.Name = ISNULL(NULLIF(m.CategoryGroup, N''), N'Genel') " +
            "WHERE m.IsActive = 1 ORDER BY ISNULL(c.SortOrder, 100000), m.SortOrder, m.CreatedAt DESC, m.MaterialId DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                SMat m = new SMat(); m.Id = r.GetInt32(0); m.TitleTr = r.GetString(1); m.TitleEn = r.IsDBNull(2) ? null : r.GetString(2); m.Group = r.GetString(3);
                m.Stored = r.GetString(4); m.Original = r.GetString(5); m.ContentType = r.IsDBNull(6) ? null : r.GetString(6); m.External = r.GetBoolean(7); m.Size = r.GetInt64(8); m.CreatedAt = r.GetDateTime(9);
                m.Summary = r.IsDBNull(10) ? null : r.GetString(10); m.Sort = r.GetInt32(11); m.Tint = r.IsDBNull(12) ? -1 : Convert.ToInt32(r[12]);
                m.Icon = r.IsDBNull(13) ? null : r.GetString(13); if (m.Icon != null && Array.IndexOf(TrainingCats.Icons, m.Icon) < 0) m.Icon = null;
                m.Views = r.GetInt32(14); m.Sends = r.GetInt32(15);
                l.Add(m);
            }
        return l;
    }

    public static SMat Get(int id)
    {
        foreach (SMat m in All()) if (m.Id == id) return m;
        return null;
    }

    /// <summary>Materyallerde olup kategori tablosunda olmayan adlari sona ekler (yeni yuklemeler)</summary>
    public static void EnsureCategories()
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.SalesMaterialCategories (Name, SortOrder) SELECT g.Name, ISNULL((SELECT MAX(SortOrder) FROM dbo.SalesMaterialCategories), 0) + ROW_NUMBER() OVER (ORDER BY g.Name) " +
            "FROM (SELECT DISTINCT ISNULL(NULLIF(CategoryGroup, N''), N'Genel') AS Name FROM dbo.SalesMaterials WHERE IsActive = 1) g " +
            "WHERE NOT EXISTS (SELECT 1 FROM dbo.SalesMaterialCategories c WHERE c.Name = g.Name)", con)) cmd.ExecuteNonQuery();
    }

    public static string Intro()
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT TextValue FROM dbo.SiteTexts WHERE TextKey = @k", con))
            { cmd.Parameters.AddWithValue("@k", IntroKey); object o = cmd.ExecuteScalar(); return o == null || o == DBNull.Value || ((string)o).Trim().Length == 0 ? DefaultIntro : (string)o; }
        }
        catch { return DefaultIntro; }
    }

    public static void SetIntro(string v)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT 1 FROM dbo.SiteTexts WHERE TextKey = @k) UPDATE dbo.SiteTexts SET TextValue = @v, UpdatedAt = SYSUTCDATETIME() WHERE TextKey = @k ELSE INSERT INTO dbo.SiteTexts (TextKey, TextValue) VALUES (@k, @v)", con))
        { cmd.Parameters.AddWithValue("@k", IntroKey); cmd.Parameters.AddWithValue("@v", v); cmd.ExecuteNonQuery(); }
    }

    /// <summary>Duzenleme: baslik, ingilizce baslik, kategori (var olan), ozet, tema (-1 otomatik), ikon (null otomatik)</summary>
    public static string Save(int id, string titleTr, string titleEn, string group, string summary, int tint, string icon)
    {
        titleTr = (titleTr ?? "").Trim(); titleEn = (titleEn ?? "").Trim(); summary = (summary ?? "").Trim().Replace("\r", " ").Replace("\n", " ");
        while (summary.Contains("  ")) summary = summary.Replace("  ", " ");
        if (titleTr.Length == 0) return "Başlık boş olamaz.";
        if (titleTr.Length > 200 || titleEn.Length > 200) return "Başlık en fazla 200 karakter olabilir.";
        if (summary.Length > 400) return "Özet en fazla 400 karakter olabilir.";
        if (tint < -1 || tint > 14) return "Geçersiz renk.";
        if (icon != null && icon.Length > 0 && Array.IndexOf(TrainingCats.Icons, icon) < 0) return "Geçersiz ikon.";
        string cat = null;
        foreach (SMatCat c in Categories()) if (Fold(c.Name) == Fold(group)) cat = c.Name;
        if (cat == null) return "Kategori listede yok. Önce Kategoriler sekmesinden ekleyin.";
        int newSort = 0;
        using (SqlConnection con = Db.Open())
        {
            string oldCat = null;
            using (SqlCommand q = new SqlCommand("SELECT ISNULL(NULLIF(CategoryGroup, N''), N'Genel') FROM dbo.SalesMaterials WHERE MaterialId = @i AND IsActive = 1", con))
            { q.Parameters.AddWithValue("@i", id); object o = q.ExecuteScalar(); if (o == null) return "Materyal bulunamadı."; oldCat = (string)o; }
            if (oldCat != cat)   // kategori degisti: yeni kategorinin sonuna
                using (SqlCommand q = new SqlCommand("SELECT ISNULL(MAX(SortOrder), 0) + 1 FROM dbo.SalesMaterials WHERE ISNULL(NULLIF(CategoryGroup, N''), N'Genel') = @g AND IsActive = 1", con))
                { q.Parameters.AddWithValue("@g", cat); newSort = (int)q.ExecuteScalar(); }
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.SalesMaterials SET TitleTr = @t, TitleEn = @te, CategoryGroup = @g, Summary = @s, Tint = @n, Icon = @ic" + (newSort > 0 ? ", SortOrder = @so" : "") + " WHERE MaterialId = @i", con))
            {
                cmd.Parameters.AddWithValue("@t", titleTr); cmd.Parameters.AddWithValue("@te", titleEn.Length == 0 ? titleTr : titleEn); cmd.Parameters.AddWithValue("@g", cat);
                cmd.Parameters.AddWithValue("@s", summary.Length == 0 ? (object)DBNull.Value : summary); cmd.Parameters.AddWithValue("@n", tint < 0 ? (object)DBNull.Value : (byte)tint);
                cmd.Parameters.AddWithValue("@ic", string.IsNullOrEmpty(icon) ? (object)DBNull.Value : icon); cmd.Parameters.AddWithValue("@so", newSort); cmd.Parameters.AddWithValue("@i", id);
                cmd.ExecuteNonQuery();
            }
        }
        return null;
    }

    /// <summary>Materyal sirasi: ayni kategorideki komsuyla yer degistirir</summary>
    public static void MoveItem(int id, int dir)
    {
        List<SMat> all = All(); SMat me = null; foreach (SMat m in all) if (m.Id == id) me = m;
        if (me == null) return;
        List<int> ids = new List<int>(); foreach (SMat m in all) if (m.Group == me.Group) ids.Add(m.Id);
        Reorder("dbo.SalesMaterials", "MaterialId", ids, id, dir);
    }

    public static void MoveCat(int id, int dir)
    {
        List<int> ids = new List<int>(); foreach (SMatCat c in Categories()) ids.Add(c.Id);
        Reorder("dbo.SalesMaterialCategories", "CategoryId", ids, id, dir);
    }

    private static void Reorder(string table, string key, List<int> ids, int id, int dir)
    {
        int i = ids.IndexOf(id), j = i + (dir < 0 ? -1 : 1);
        if (i < 0 || j < 0 || j >= ids.Count) return;
        int t = ids[i]; ids[i] = ids[j]; ids[j] = t;
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            for (int k = 0; k < ids.Count; k++)
                using (SqlCommand cmd = new SqlCommand("UPDATE " + table + " SET SortOrder = @o WHERE " + key + " = @i", con, tx))
                { cmd.Parameters.AddWithValue("@o", k + 1); cmd.Parameters.AddWithValue("@i", ids[k]); cmd.ExecuteNonQuery(); }
            tx.Commit();
        }
    }

    public static string AddCat(string name)
    {
        name = (name ?? "").Trim(); if (name.Length == 0) return "Kategori adı boş olamaz."; if (name.Length > 120) return "Kategori adı en fazla 120 karakter olabilir.";
        foreach (SMatCat c in Categories()) if (Fold(c.Name) == Fold(name)) return "Bu adda bir kategori zaten var.";
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.SalesMaterialCategories (Name, SortOrder) SELECT @n, ISNULL(MAX(SortOrder), 0) + 1 FROM dbo.SalesMaterialCategories", con))
        { cmd.Parameters.AddWithValue("@n", name); cmd.ExecuteNonQuery(); }
        return null;
    }

    /// <summary>Kategori adi degisikligi: o kategorideki materyallere de ayni islemde yansir</summary>
    public static string RenameCat(int id, string name)
    {
        name = (name ?? "").Trim(); if (name.Length == 0) return "Kategori adı boş olamaz."; if (name.Length > 120) return "Kategori adı en fazla 120 karakter olabilir.";
        string old = null;
        foreach (SMatCat c in Categories()) { if (c.Id == id) old = c.Name; else if (Fold(c.Name) == Fold(name)) return "Bu adda bir kategori zaten var."; }
        if (old == null) return "Kategori bulunamadı.";
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.SalesMaterials SET CategoryGroup = @n WHERE ISNULL(NULLIF(CategoryGroup, N''), N'Genel') = @o", con, tx))
            { cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@o", old); cmd.ExecuteNonQuery(); }
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.SalesMaterialCategories SET Name = @n WHERE CategoryId = @i", con, tx))
            { cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@i", id); cmd.ExecuteNonQuery(); }
            tx.Commit();
        }
        return null;
    }

    /// <summary>Bos kategori silinebilir; icinde etkin materyal varsa reddedilir</summary>
    public static string DeleteCat(int id)
    {
        string name = null; foreach (SMatCat c in Categories()) if (c.Id == id) name = c.Name;
        if (name == null) return "Kategori bulunamadı.";
        int n = 0; foreach (SMat m in All()) if (m.Group == name) n++;
        if (n > 0) return "\"" + name + "\" kategorisinde " + n + " materyal var. Önce materyalleri başka kategoriye taşıyın.";
        using (SqlConnection con = Db.Open()) using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.SalesMaterialCategories WHERE CategoryId = @i", con)) { cmd.Parameters.AddWithValue("@i", id); cmd.ExecuteNonQuery(); }
        return null;
    }

    public static void Log(int materialId, int userId, int companyId, string kind)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.SalesMaterialEvents (MaterialId, UserId, CompanyId, Kind) VALUES (@m, @u, @c, @k)", con))
            { cmd.Parameters.AddWithValue("@m", materialId); cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@c", companyId > 0 ? (object)companyId : DBNull.Value); cmd.Parameters.AddWithValue("@k", kind); cmd.ExecuteNonQuery(); }
        }
        catch { }
    }

    /// <summary>Kullanicinin son bir saatteki gonderim sayisi (kotuye kullanima karsi sinir)</summary>
    public static int SendsLastHour(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.SalesMaterialEvents WHERE UserId = @u AND Kind = N'send' AND CreatedAt > DATEADD(HOUR, -1, SYSUTCDATETIME())", con))
        { cmd.Parameters.AddWithValue("@u", userId); return (int)cmd.ExecuteScalar(); }
    }

    /// <summary>Otomatik ikon: basliktaki anahtar sozcuklere, yoksa dosya turune gore (hazir ikon listesinden)</summary>
    public static string AutoIcon(SMat m)
    {
        string t = Fold(m.TitleTr + " " + m.Group);
        // anahtar sozcukler Fold() ciktisiyla ayni bicimde: kucuk harf, "ı" -> "i"; diger Turkce harfler korunur
        string[][] map = {
            new[]{"fiyat","invoice"}, new[]{"fatura","invoice"}, new[]{"crm","team"}, new[]{"banka","building"}, new[]{"bakim","settings"}, new[]{"budget","chart"}, new[]{"bütçe","chart"},
            new[]{"ebys","doc"}, new[]{"belge","doc"}, new[]{"kalite","check"}, new[]{"qms","check"}, new[]{"itsm","chat"}, new[]{"helpdesk","chat"}, new[]{"ticket","chat"}, new[]{"destek","chat"},
            new[]{"rpa","ai"}, new[]{"robot","ai"}, new[]{"yapay","ai"}, new[]{"sandik","database"}, new[]{"satin alma","cart"}, new[]{"satiş sonrasi","settings"}, new[]{"ssh","settings"},
            new[]{"sözleşme","sign"}, new[]{"süreç","flow"}, new[]{"üretim","layers"}, new[]{"sunum","dashboard"}, new[]{"broşür","book"}, new[]{"katalog","catalog"}, new[]{"liste","table"},
            new[]{"tanitim","rocket"}, new[]{"form","form"}, new[]{"intranet","building"}, new[]{"eğitim","cap"}, new[]{"güvenlik","shield"}, new[]{"mobil","mobile"} };
        foreach (string[] p in map) if (t.Contains(p[0])) return p[1];
        switch (m.Ext())
        {
            case "pdf": return "pdf"; case "ppt": case "pptx": return "dashboard"; case "xls": case "xlsx": case "csv": return "table";
            case "zip": return "layers"; case "png": case "jpg": case "jpeg": return "image"; case "": return "globe"; default: return "doc";
        }
    }
}
