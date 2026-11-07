using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;

// Akademi kategorileri (editor yonetimi) ve dokuman dosya yardimcilari.
public class TCat
{
    public int Id; public int? ParentId; public string Scope, Name; public int Sort, Count;
    public List<TCat> Subs = new List<TCat>();
}

/// <summary>Kategori gorunumu (ada bagli): kart gorseli, kapak gorseli, kart rengi (0..7, null = otomatik)</summary>
public class TStyle { public int Id; public string Group, Sub, Card, Cover, Icon; public int? Tint; }   // Tint 0..14 (8 Gunes, 14 Buz: koyu yazili); Icon: assets/caticons.svg#i-<ad>

public static class TrainingCats
{
    public const string StyleDir = "~/assets/catcovers";
    /// <summary>Ad karsilastirmasi: Turkce kucuk harf, sonra "ı" = "i" (sayfadaki Fold ve acshelf.js ile ayni kural)</summary>
    public static string Fold(string s) { return (s ?? "").Trim().ToLower(new System.Globalization.CultureInfo("tr-TR")).Replace('ı', 'i'); }

    public static bool StylesExist()
    {
        try { using (SqlConnection con = Db.Open()) using (SqlCommand cmd = new SqlCommand("SELECT OBJECT_ID('dbo.TrainingCategoryStyles', 'U')", con)) { object o = cmd.ExecuteScalar(); return o != null && o != DBNull.Value; } }
        catch { return false; }
    }

    /// <summary>Hazir kategori ikonlari (assets/caticons.svg; "i-" onekli semboller). Yalniz bu adlar kaydedilebilir.</summary>
    public static readonly string[] Icons = { "form", "flow", "doc", "template", "pdf", "table", "catalog", "database", "chart", "dashboard", "api", "settings", "team", "lock", "shield", "bell", "calendar", "mail", "chat", "ai", "mobile", "globe", "cloud", "code", "upload", "search", "check", "sign", "qr", "pin", "image", "play", "news", "cap", "book", "idea", "target", "rocket", "cart", "invoice", "puzzle", "layers", "building", "clock" };
    public static bool IsIcon(string id) { return !string.IsNullOrEmpty(id) && (Array.IndexOf(Icons, id) >= 0 || IsCustomIcon(id)); }

    /// <summary>Editorun yukledigi ikon: "u:" + assets/catcovers altindaki islenmis PNG (32 hane + .png)</summary>
    public static bool IsCustomIcon(string id) { return id != null && System.Text.RegularExpressions.Regex.IsMatch(id, "^u:[0-9a-f]{32}\\.png$"); }
    public static string CustomIconUrl(string id) { return IsCustomIcon(id) ? StyleUrl(id.Substring(2)) : null; }

    /// <summary>Yuklenen ikonu hazir ikonlarla ayni olcuye getirir: saydam kenarlari kirpar, 512x512 kareye
    /// hazir ikonlarla ayni kenar payiyla (%10) ortalar, PNG kaydeder. Zemini saydam degilse reddeder.</summary>
    public static string PrepareIcon(Stream input, out string err)
    {
        byte[] png = ProcessIcon(input, out err);
        if (png == null) return null;
        string dir = HttpContext.Current.Server.MapPath(StyleDir);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string name = Guid.NewGuid().ToString("N") + ".png";
        File.WriteAllBytes(Path.Combine(dir, name), png);
        return name;
    }

    /// <summary>Gorsel isleme (kaydetmeden): kirpma + 512x512 kareye ortalama; PNG baytlari ya da hata</summary>
    public static byte[] ProcessIcon(Stream input, out string err)
    {
        err = null;
        using (System.Drawing.Bitmap src = new System.Drawing.Bitmap(input))
        {
            int w = src.Width, h = src.Height;
            if (w < 16 || h < 16) { err = "İkon en az 16×16 piksel olmalı."; return null; }
            if (w > 4096 || h > 4096) { err = "İkon en fazla 4096×4096 piksel olabilir."; return null; }
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp)) gr.DrawImage(src, 0, 0, w, h);
                System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                byte[] px = new byte[bd.Stride * h];
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, px, 0, px.Length);
                int stride = bd.Stride; bmp.UnlockBits(bd);
                int minX = w, minY = h, maxX = -1, maxY = -1; long clear = 0;
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        byte a = px[y * stride + x * 4 + 3];
                        if (a < 250) clear++;
                        if (a > 12) { if (x < minX) minX = x; if (x > maxX) maxX = x; if (y < minY) minY = y; if (y > maxY) maxY = y; }
                    }
                if (maxX < 0) { err = "İkon tamamen saydam görünüyor."; return null; }
                if (clear < (long)w * h / 20) { err = "İkonun zemini saydam olmalı (saydam zeminli PNG yükleyin). Saydam olmayan zemin kartta dikdörtgen olarak görünür."; return null; }
                int cw = maxX - minX + 1, ch = maxY - minY + 1;
                const int S = 512; const int pad = 51;   // %10 kenar payi (hazir ikonlar 64'luk izgarada ~8 birim pay birakir)
                double k = Math.Min((S - 2.0 * pad) / cw, (S - 2.0 * pad) / ch);
                int dw = Math.Max(1, (int)Math.Round(cw * k)), dh = Math.Max(1, (int)Math.Round(ch * k));
                using (System.Drawing.Bitmap outp = new System.Drawing.Bitmap(S, S, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(outp))
                    {
                        gr.Clear(System.Drawing.Color.Transparent);
                        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        gr.DrawImage(bmp, new System.Drawing.Rectangle((S - dw) / 2, (S - dh) / 2, dw, dh), new System.Drawing.Rectangle(minX, minY, cw, ch), System.Drawing.GraphicsUnit.Pixel);
                    }
                    using (MemoryStream ms = new MemoryStream()) { outp.Save(ms, System.Drawing.Imaging.ImageFormat.Png); return ms.ToArray(); }
                }
            }
        }
    }

    /// <summary>Icon sutunu (57. migration) var mi? Yoksa ikon ozelligi sessizce devre disi kalir.</summary>
    public static bool IconColumn()
    {
        try { using (SqlConnection con = Db.Open()) using (SqlCommand cmd = new SqlCommand("SELECT COL_LENGTH('dbo.TrainingCategoryStyles', 'Icon')", con)) { object o = cmd.ExecuteScalar(); return o != null && o != DBNull.Value; } }
        catch { return false; }
    }

    public static List<TStyle> Styles()
    {
        List<TStyle> l = new List<TStyle>();
        if (!StylesExist()) return l;
        bool ic = IconColumn();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT StyleId, GroupName, SubName, CardImage, CoverImage, Tint" + (ic ? ", Icon" : "") + " FROM dbo.TrainingCategoryStyles", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                TStyle t = new TStyle(); t.Id = r.GetInt32(0); t.Group = r.GetString(1); t.Sub = r.GetString(2);
                t.Card = r.IsDBNull(3) ? null : r.GetString(3); t.Cover = r.IsDBNull(4) ? null : r.GetString(4); t.Tint = r.IsDBNull(5) ? (int?)null : Convert.ToInt32(r[5]);
                if (ic && !r.IsDBNull(6)) { string i = r.GetString(6); if (IsIcon(i)) t.Icon = i; }
                l.Add(t);
            }
        return l;
    }

    /// <summary>Ada gore gorunum: sub bos / null / "Genel" = ana kategori</summary>
    public static TStyle FindStyle(List<TStyle> l, string group, string sub)
    {
        string g = Fold(group), s = Fold(sub); if (s == "genel") s = "";
        foreach (TStyle t in l) if (Fold(t.Group) == g && Fold(t.Sub) == s) return t;
        return null;
    }

    public static string StyleUrl(string file) { return string.IsNullOrEmpty(file) ? null : "assets/catcovers/" + file; }

    /// <summary>Kategori kimliginden ad cifti (ana kategori icin sub = "")</summary>
    public static bool NamesOf(int id, out string group, out string sub)
    {
        group = null; sub = "";
        using (SqlConnection con = Db.Open())
        {
            TCat c = Get(con, null, id); if (c == null) return false;
            if (c.ParentId.HasValue) { group = c.Subs.Count > 0 ? c.Subs[0].Name : ""; sub = c.Name; } else group = c.Name;
            return true;
        }
    }

    /// <summary>Gorunumu kaydeder. card / cover: yeni dosya adi (null = degismez); clear*: kaldir. Eski dosyalar diskten silinir.</summary>
    public static void SaveStyle(string group, string sub, string card, string cover, bool clearCard, bool clearCover, int? tint, bool setTint)
    { SaveStyle(group, sub, card, cover, clearCard, clearCover, tint, setTint, null, false); }

    /// <summary>icon / setIcon: hazir ikon (null = kaldir); Icon sutunu yoksa yok sayilir</summary>
    public static void SaveStyle(string group, string sub, string card, string cover, bool clearCard, bool clearCover, int? tint, bool setTint, string icon, bool setIcon)
    {
        TStyle old = FindStyle(Styles(), group, sub);
        string nCard = card ?? (clearCard ? null : old != null ? old.Card : null), nCover = cover ?? (clearCover ? null : old != null ? old.Cover : null);
        int? nTint = setTint ? tint : old != null ? old.Tint : null;
        bool ic = IconColumn();
        string nIcon = setIcon ? (IsIcon(icon) ? icon : null) : old != null ? old.Icon : null;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(old == null
            ? "INSERT INTO dbo.TrainingCategoryStyles (GroupName, SubName, CardImage, CoverImage, Tint" + (ic ? ", Icon" : "") + ") VALUES (@g, @s, @c, @v, @t" + (ic ? ", @ic" : "") + ")"
            : "UPDATE dbo.TrainingCategoryStyles SET CardImage = @c, CoverImage = @v, Tint = @t" + (ic ? ", Icon = @ic" : "") + ", UpdatedAt = SYSUTCDATETIME() WHERE StyleId = @i", con))
        {
            cmd.Parameters.AddWithValue("@ic", (object)nIcon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@g", group); cmd.Parameters.AddWithValue("@s", sub ?? "");
            cmd.Parameters.AddWithValue("@c", (object)nCard ?? DBNull.Value); cmd.Parameters.AddWithValue("@v", (object)nCover ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@t", nTint.HasValue ? (object)(byte)nTint.Value : DBNull.Value); cmd.Parameters.AddWithValue("@i", old != null ? old.Id : 0);
            cmd.ExecuteNonQuery();
        }
        if (old != null && old.Card != null && old.Card != nCard) DeleteStyleFile(old.Card);
        if (old != null && old.Cover != null && old.Cover != nCover) DeleteStyleFile(old.Cover);
        if (old != null && IsCustomIcon(old.Icon) && old.Icon != nIcon) DeleteStyleFile(old.Icon.Substring(2));   // eski yuklenmis ikon
    }

    public static void DeleteStyleFile(string file)
    {
        if (string.IsNullOrEmpty(file) || file.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return;
        try { string p = Path.Combine(HttpContext.Current.Server.MapPath(StyleDir), file); if (File.Exists(p)) File.Delete(p); } catch { }
    }

    /// <summary>Kategori silinince gorunumu ve gorselleri de silinir (ana kategori: tum alt kategorilerinin gorunumleriyle)</summary>
    private static void DropStyles(string group, string sub, bool wholeGroup)
    {
        if (!StylesExist()) return;
        foreach (TStyle t in Styles())
            if (Fold(t.Group) == Fold(group) && (wholeGroup || Fold(t.Sub) == Fold(sub)))
            {
                DeleteStyleFile(t.Card); DeleteStyleFile(t.Cover); if (IsCustomIcon(t.Icon)) DeleteStyleFile(t.Icon.Substring(2));
                using (SqlConnection con = Db.Open()) using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.TrainingCategoryStyles WHERE StyleId = @i", con))
                { cmd.Parameters.AddWithValue("@i", t.Id); cmd.ExecuteNonQuery(); }
            }
    }
    private static string Mt(string scope) { return scope == "Video" ? "Video" : "Document"; }
    public static string NormScope(string s) { return s == "Video" ? "Video" : s == "Document" ? "Document" : null; }

    public static bool TableExists()
    {
        try { using (SqlConnection con = Db.Open()) using (SqlCommand cmd = new SqlCommand("SELECT OBJECT_ID('dbo.TrainingCategories', 'U')", con)) { object o = cmd.ExecuteScalar(); return o != null && o != DBNull.Value; } }
        catch { return false; }
    }

    /// <summary>Kapsamin ana kategorileri (sirali), alt kategorileriyle ve icerik sayilariyla</summary>
    public static List<TCat> List(string scope)
    {
        List<TCat> mains = new List<TCat>(); Dictionary<int, TCat> byId = new Dictionary<int, TCat>(); List<TCat> subs = new List<TCat>();
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand("SELECT CategoryId, ParentId, Name, SortOrder FROM dbo.TrainingCategories WHERE Scope = @s ORDER BY SortOrder, CategoryId", con))
            {
                cmd.Parameters.AddWithValue("@s", scope);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        TCat c = new TCat(); c.Id = r.GetInt32(0); c.ParentId = r.IsDBNull(1) ? (int?)null : r.GetInt32(1); c.Name = r.GetString(2); c.Sort = r.GetInt32(3); c.Scope = scope;
                        byId[c.Id] = c; if (c.ParentId.HasValue) subs.Add(c); else mains.Add(c);
                    }
            }
            foreach (TCat s in subs) { TCat p; if (byId.TryGetValue(s.ParentId.Value, out p)) p.Subs.Add(s); }
            // icerik sayilari (pasifler dahil degil)
            using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(NULLIF(LTRIM(RTRIM(CategoryGroup)), N''), N'Genel'), ISNULL(NULLIF(LTRIM(RTRIM(SubCategory)), N''), N'Genel'), COUNT(*) FROM dbo.TrainingMaterials WHERE MaterialType = @t AND IsActive = 1 GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(CategoryGroup)), N''), N'Genel'), ISNULL(NULLIF(LTRIM(RTRIM(SubCategory)), N''), N'Genel')", con))
            {
                cmd.Parameters.AddWithValue("@t", Mt(scope));
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        string g = r.GetString(0), sb = r.GetString(1); int n = r.GetInt32(2);
                        foreach (TCat m in mains)
                            if (string.Equals(m.Name, g, StringComparison.CurrentCultureIgnoreCase))
                            {
                                m.Count += n;
                                foreach (TCat s in m.Subs) if (string.Equals(s.Name, sb, StringComparison.CurrentCultureIgnoreCase)) s.Count += n;
                            }
                    }
            }
        }
        return mains;
    }

    /// <summary>Sayfa siralamasi icin: ana kategori adi -> sira, "ana\u0001alt" -> sira</summary>
    public static Dictionary<string, int> Order(string scope)
    {
        Dictionary<string, int> d = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
        if (!TableExists()) return d;
        int i = 0;
        foreach (TCat m in List(scope)) { d[m.Name] = i++; int k = 0; foreach (TCat s in m.Subs) d[m.Name + "\u0001" + s.Name] = k++; }
        return d;
    }

    private static string Clean(string n, out string err)
    {
        err = null; n = (n ?? "").Trim();
        while (n.Contains("  ")) n = n.Replace("  ", " ");
        if (n.Length == 0) { err = "Kategori adı boş olamaz."; return null; }
        if (n.Length > 120) { err = "Kategori adı en fazla 120 karakter olabilir."; return null; }
        return n;
    }

    private static bool Exists(SqlConnection con, SqlTransaction tx, string scope, int? parent, string name, int exceptId)
    {
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.TrainingCategories WHERE Scope = @s AND " + (parent.HasValue ? "ParentId = @p" : "ParentId IS NULL") + " AND Name = @n AND CategoryId <> @x", con, tx))
        {
            cmd.Parameters.AddWithValue("@s", scope); cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@x", exceptId);
            if (parent.HasValue) cmd.Parameters.AddWithValue("@p", parent.Value);
            return (int)cmd.ExecuteScalar() > 0;
        }
    }

    private static int Insert(SqlConnection con, SqlTransaction tx, string scope, int? parent, string name)
    {
        using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.TrainingCategories (Scope, ParentId, Name, SortOrder) OUTPUT INSERTED.CategoryId " +
            "SELECT @s, @p, @n, ISNULL(MAX(SortOrder), 0) + 1 FROM dbo.TrainingCategories WHERE Scope = @s AND " + (parent.HasValue ? "ParentId = @p" : "ParentId IS NULL"), con, tx))
        {
            cmd.Parameters.AddWithValue("@s", scope); cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@p", parent.HasValue ? (object)parent.Value : DBNull.Value);
            return (int)cmd.ExecuteScalar();
        }
    }

    public static string AddMain(string scope, string name)
    {
        string err; name = Clean(name, out err); if (err != null) return err;
        using (SqlConnection con = Db.Open())
        {
            if (Exists(con, null, scope, null, name, 0)) return "\"" + name + "\" adında bir ana kategori zaten var.";
            Insert(con, null, scope, null, name);
        }
        return null;
    }

    public static string AddSub(string scope, int parentId, string name)
    {
        string err; name = Clean(name, out err); if (err != null) return err;
        if (string.Equals(name, "Genel", StringComparison.CurrentCultureIgnoreCase)) return "\"Genel\" alt kategori adı ayrılmıştır; alt kategorisiz içerikler için kullanılır.";
        using (SqlConnection con = Db.Open())
        {
            TCat p = Get(con, null, parentId);
            if (p == null || p.ParentId.HasValue || p.Scope != scope) return "Ana kategori bulunamadı.";
            if (Exists(con, null, scope, parentId, name, 0)) return "\"" + p.Name + "\" altında \"" + name + "\" zaten var.";
            Insert(con, null, scope, parentId, name);
        }
        return null;
    }

    private static TCat Get(SqlConnection con, SqlTransaction tx, int id)
    {
        using (SqlCommand cmd = new SqlCommand("SELECT c.CategoryId, c.ParentId, c.Name, c.SortOrder, c.Scope, p.Name FROM dbo.TrainingCategories c LEFT JOIN dbo.TrainingCategories p ON p.CategoryId = c.ParentId WHERE c.CategoryId = @i", con, tx))
        {
            cmd.Parameters.AddWithValue("@i", id);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                TCat c = new TCat(); c.Id = r.GetInt32(0); c.ParentId = r.IsDBNull(1) ? (int?)null : r.GetInt32(1); c.Name = r.GetString(2); c.Sort = r.GetInt32(3); c.Scope = r.GetString(4);
                if (!r.IsDBNull(5)) { TCat p = new TCat(); p.Name = r.GetString(5); c.Subs.Add(p); }   // Subs[0] = ust kategori adi (yalniz okuma)
                return c;
            }
        }
    }

    /// <summary>Ad degisikligi: kategori ve o kategorideki tum icerikler ayni islemde guncellenir</summary>
    public static string Rename(int id, string name)
    {
        string err; name = Clean(name, out err); if (err != null) return err;
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                TCat c = Get(con, tx, id); if (c == null) { tx.Rollback(); return "Kategori bulunamadı."; }
                if (c.ParentId.HasValue && string.Equals(name, "Genel", StringComparison.CurrentCultureIgnoreCase)) { tx.Rollback(); return "\"Genel\" alt kategori adı ayrılmıştır."; }
                if (Exists(con, tx, c.Scope, c.ParentId, name, id)) { tx.Rollback(); return "Bu adda bir kategori zaten var."; }
                string sql = c.ParentId.HasValue
                    ? "UPDATE dbo.TrainingMaterials SET SubCategory = @n WHERE MaterialType = @t AND ISNULL(NULLIF(LTRIM(RTRIM(CategoryGroup)), N''), N'Genel') = @g AND LTRIM(RTRIM(SubCategory)) = @o"
                    : "UPDATE dbo.TrainingMaterials SET CategoryGroup = @n WHERE MaterialType = @t AND ISNULL(NULLIF(LTRIM(RTRIM(CategoryGroup)), N''), N'Genel') = @o";
                using (SqlCommand cmd = new SqlCommand(sql, con, tx))
                {
                    cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@o", c.Name); cmd.Parameters.AddWithValue("@t", Mt(c.Scope));
                    cmd.Parameters.AddWithValue("@g", c.Subs.Count > 0 ? c.Subs[0].Name : "");
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TrainingCategories SET Name = @n WHERE CategoryId = @i", con, tx))
                { cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@i", id); cmd.ExecuteNonQuery(); }
                // gorunum ayarlari ada baglidir: yeni ada tasinir
                using (SqlCommand cmd = new SqlCommand("IF OBJECT_ID('dbo.TrainingCategoryStyles', 'U') IS NOT NULL " + (c.ParentId.HasValue
                    ? "UPDATE dbo.TrainingCategoryStyles SET SubName = @n WHERE GroupName = @g AND SubName = @o"
                    : "UPDATE dbo.TrainingCategoryStyles SET GroupName = @n WHERE GroupName = @o"), con, tx))
                {
                    cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@o", c.Name);
                    cmd.Parameters.AddWithValue("@g", c.Subs.Count > 0 ? c.Subs[0].Name : "");
                    cmd.ExecuteNonQuery();
                }
                tx.Commit(); return null;
            }
            catch { tx.Rollback(); throw; }
        }
    }

    /// <summary>Sira degisikligi: ayni seviyedeki komsuyla yer degistirir (dir: -1 yukari, +1 asagi)</summary>
    public static string Move(int id, int dir)
    {
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                TCat c = Get(con, tx, id); if (c == null) { tx.Rollback(); return "Kategori bulunamadı."; }
                List<int> ids = new List<int>();
                using (SqlCommand cmd = new SqlCommand("SELECT CategoryId FROM dbo.TrainingCategories WHERE Scope = @s AND " + (c.ParentId.HasValue ? "ParentId = @p" : "ParentId IS NULL") + " ORDER BY SortOrder, CategoryId", con, tx))
                {
                    cmd.Parameters.AddWithValue("@s", c.Scope); if (c.ParentId.HasValue) cmd.Parameters.AddWithValue("@p", c.ParentId.Value);
                    using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) ids.Add(r.GetInt32(0));
                }
                int i = ids.IndexOf(id), j = i + (dir < 0 ? -1 : 1);
                if (j < 0 || j >= ids.Count) { tx.Rollback(); return null; }
                int t = ids[i]; ids[i] = ids[j]; ids[j] = t;
                for (int k = 0; k < ids.Count; k++)
                    using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TrainingCategories SET SortOrder = @o WHERE CategoryId = @i", con, tx))
                    { cmd.Parameters.AddWithValue("@o", k + 1); cmd.Parameters.AddWithValue("@i", ids[k]); cmd.ExecuteNonQuery(); }
                tx.Commit(); return null;
            }
            catch { tx.Rollback(); throw; }
        }
    }

    /// <summary>Silme: icinde (alt kategorileri dahil) etkin icerik varsa reddedilir; ana kategori bos alt kategorileriyle silinir</summary>
    public static string Delete(int id)
    {
        TCat c;
        using (SqlConnection con = Db.Open()) c = Get(con, null, id);
        if (c == null) return "Kategori bulunamadı.";
        foreach (TCat m in List(c.Scope))
        {
            if (!c.ParentId.HasValue && m.Id == id && m.Count > 0) return "\"" + m.Name + "\" kategorisinde " + m.Count + " içerik var. Önce içerikleri başka bir kategoriye taşıyın.";
            foreach (TCat s in m.Subs) if (s.Id == id && s.Count > 0) return "\"" + s.Name + "\" alt kategorisinde " + s.Count + " içerik var. Önce içerikleri başka bir kategoriye taşıyın.";
        }
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.TrainingCategories WHERE ParentId = @i; DELETE FROM dbo.TrainingCategories WHERE CategoryId = @i;", con))
        { cmd.Parameters.AddWithValue("@i", id); cmd.ExecuteNonQuery(); }
        // ayni ad diger kapsamda (video / dokuman) hala varsa gorunum korunur
        string g = c.ParentId.HasValue ? (c.Subs.Count > 0 ? c.Subs[0].Name : "") : c.Name, sb = c.ParentId.HasValue ? c.Name : "";
        string other = c.Scope == "Video" ? "Document" : "Video"; bool still = false;
        if (TableExists()) foreach (TCat m in List(other)) if (Fold(m.Name) == Fold(g)) { if (!c.ParentId.HasValue) still = true; else foreach (TCat x in m.Subs) if (Fold(x.Name) == Fold(sb)) still = true; }
        if (!still) DropStyles(g, sb, !c.ParentId.HasValue);
        return null;
    }

    /// <summary>Icerik eklenirken / duzenlenirken yazilan kategori tabloda yoksa eklenir (formdaki "+ Yeni" secenegi)</summary>
    public static void Ensure(string scope, string group, string sub)
    {
        if (!TableExists()) return;
        string err; group = Clean(string.IsNullOrEmpty(group) ? "Genel" : group, out err); if (err != null) return;
        using (SqlConnection con = Db.Open())
        {
            int pid;
            using (SqlCommand cmd = new SqlCommand("SELECT CategoryId FROM dbo.TrainingCategories WHERE Scope = @s AND ParentId IS NULL AND Name = @n", con))
            { cmd.Parameters.AddWithValue("@s", scope); cmd.Parameters.AddWithValue("@n", group); object o = cmd.ExecuteScalar(); pid = o == null ? Insert(con, null, scope, null, group) : (int)o; }
            string s = (sub ?? "").Trim();
            if (s.Length == 0 || string.Equals(s, "Genel", StringComparison.CurrentCultureIgnoreCase) || s.Length > 120) return;
            if (!Exists(con, null, scope, pid, s, 0)) Insert(con, null, scope, pid, s);
        }
    }
}

// Akademi dokuman dosyalari: guvenli saklama (App_Data), tur siniflandirmasi, tarayici goruntuleyici turu.
public static class TrainingFiles
{
    // Program / betik dosyalari: dokuman degildir, portal uzerinden dagitilmalari engellenir
    private static readonly string[] Blocked = { ".exe", ".msi", ".bat", ".cmd", ".com", ".scr", ".pif", ".ps1", ".psm1", ".vbs", ".vbe", ".js", ".jse", ".wsf", ".wsh", ".hta", ".dll", ".sys", ".cpl", ".jar", ".lnk", ".reg", ".aspx", ".asp", ".ashx", ".asmx", ".config", ".cshtml", ".php", ".sh" };
    public static bool IsBlocked(string ext) { return Array.IndexOf(Blocked, (ext ?? "").ToLowerInvariant()) >= 0; }

    /// <summary>Tarayicidaki goruntuleyici: pdf, image, video, audio, text, csv, docx, xlsx; desteklenmeyen: none (indirilir)</summary>
    public static string Kind(string ext)
    {
        switch ((ext ?? "").ToLowerInvariant().TrimStart('.'))
        {
            case "pdf": return "pdf";
            case "png": case "jpg": case "jpeg": case "gif": case "webp": case "bmp": case "svg": return "image";
            case "mp4": case "webm": case "ogv": case "mov": case "m4v": return "video";
            case "mp3": case "wav": case "ogg": case "oga": case "m4a": case "aac": case "flac": return "audio";
            case "txt": case "log": case "md": case "json": case "xml": case "ini": case "yml": case "yaml": case "sql": return "text";
            case "csv": case "tsv": return "csv";
            case "docx": return "docx";
            case "xlsx": case "xls": case "ods": return "xlsx";
            default: return "none";
        }
    }

    public static string Mime(string ext)
    {
        switch ((ext ?? "").ToLowerInvariant().TrimStart('.'))
        {
            case "pdf": return "application/pdf";
            case "png": return "image/png"; case "jpg": case "jpeg": return "image/jpeg"; case "gif": return "image/gif"; case "webp": return "image/webp"; case "bmp": return "image/bmp"; case "svg": return "image/svg+xml";
            case "mp4": case "m4v": return "video/mp4"; case "webm": return "video/webm"; case "ogv": return "video/ogg"; case "mov": return "video/quicktime";
            case "mp3": return "audio/mpeg"; case "wav": return "audio/wav"; case "ogg": case "oga": return "audio/ogg"; case "m4a": return "audio/mp4"; case "aac": return "audio/aac"; case "flac": return "audio/flac";
            case "txt": case "log": case "md": case "json": case "xml": case "ini": case "yml": case "yaml": case "sql": case "csv": case "tsv": return "text/plain; charset=utf-8";   // metin olarak: tarayici calistirmaz
            case "docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            case "xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            case "xls": return "application/vnd.ms-excel";
            default: return "application/octet-stream";
        }
    }

    public static string NewDir() { return HttpContext.Current.Server.MapPath("~/App_Data/trainingdocs"); }

    /// <summary>Dosyanin diskteki yolu: once App_Data (yeni), yoksa eski ~/trainingdocs</summary>
    public static string PathOf(string stored)
    {
        if (string.IsNullOrEmpty(stored) || stored.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || stored.Contains("..")) return null;
        string a = Path.Combine(NewDir(), stored);
        if (File.Exists(a)) return a;
        string b = Path.Combine(HttpContext.Current.Server.MapPath("~/trainingdocs"), stored);
        return File.Exists(b) ? b : null;
    }

    public static void DeleteFile(string stored) { string p = PathOf(stored); if (p != null) try { File.Delete(p); } catch { } }

    public static TrainingDoc Get(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT MaterialId, TitleTr, DocStoredName, DocOriginalName, ISNULL(DocFileSize, 0) FROM dbo.TrainingMaterials WHERE MaterialId = @i AND MaterialType = N'Document' AND IsActive = 1 AND DocStoredName IS NOT NULL", con))
        {
            cmd.Parameters.AddWithValue("@i", id);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                TrainingDoc d = new TrainingDoc(); d.MaterialId = r.GetInt32(0); d.TitleTr = r.GetString(1); d.StoredName = r.GetString(2); d.OriginalName = r.IsDBNull(3) ? r.GetString(2) : r.GetString(3); d.FileSize = Convert.ToInt64(r[4]);
                return d;
            }
        }
    }
}
