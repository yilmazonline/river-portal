using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;

// ============================================================================
// Satis onerileri: RiverTeklif siparislerinden is ortagi satis kaydi (Deals) onerisi.
// - Kapsam: is ortagi alani dolu YA DA kanal satisi olan siparisler, SalesSuggest.From tarihinden itibaren
// - Finans yetkilisi is ortagini secip "Satis olarak kaydet" der ya da yoksayar
// - Tutar KDV haric (net) kaydedilir, "+ KDV" olarak gosterilir; tutar/tarih cozumlemesi Alacak Takibi ile ayni
// - Bildirim: yalniz o firmanin bayi yoneticilerine (PartnerAdmin) uygulama ici
// ============================================================================
public class OrderSuggestion
{
    public int SuggestionId, SourceOrderId;
    public int? SuggestedCompanyId, DealId, DecidedByUserId;
    public string PartnerNameRaw, SalesChannel, CustomerName, ProductSummary, Currency, Warning, Status, SuggestedCompanyName, DealCompanyName, DecidedByName;
    public decimal NetAmount;
    public DateTime? OrderDate, DecidedAt;
}

public static class SalesSuggestions
{
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    public static DateTime From
    {
        get
        {
            DateTime d; string v = WebConfigurationManager.AppSettings["SalesSuggest.From"];
            return !string.IsNullOrEmpty(v) && DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : new DateTime(DateTime.Now.Year, 1, 1);
        }
    }

    private static string Clean(object o)
    {
        string s = o == null || o == DBNull.Value ? null : Convert.ToString(o);
        if (s == null) return null; s = s.Trim(); return s.Length == 0 ? null : s;
    }

    /// <summary>Firma adi karsilastirma: Turkce harf, sirket ekleri, noktalama farklari yok sayilir</summary>
    public static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Replace("İ", "i").Replace("I", "ı").ToLower(TR);
        s = s.Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
        s = Regex.Replace(s, @"[^a-z0-9 ]", " ");
        s = Regex.Replace(s, @"\b(a s|as|anonim|sirketi|sti|ltd|limited|tic|ticaret|san|sanayi|ve|bilgi|teknolojileri|yazilim)\b", " ");
        return Regex.Replace(s, @"\s+", " ").Trim();
    }

    private class Partner { public int Id; public string Name, Key; }
    private static List<Partner> Partners(SqlConnection con)
    {
        List<Partner> l = new List<Partner>();
        using (SqlCommand cmd = new SqlCommand("SELECT CompanyId, Name FROM dbo.Companies WHERE CompanyType = N'Partner' ORDER BY Name", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read()) { Partner p = new Partner(); p.Id = r.GetInt32(0); p.Name = r[1] as string ?? ""; p.Key = Norm(p.Name); l.Add(p); }
        return l;
    }
    /// <summary>Siparisteki is ortagi metninden TEK bir firmaya kesin eslesme; belirsizse null (admin secer)</summary>
    private static int? Match(List<Partner> ps, string raw)
    {
        string k = Norm(raw); if (k.Length < 3) return null;
        List<int> exact = new List<int>(), part = new List<int>();
        foreach (Partner p in ps)
        {
            if (p.Key.Length < 3) continue;
            if (p.Key == k) exact.Add(p.Id);
            else if (k.Length >= 4 && p.Key.Length >= 4 && (p.Key.Contains(k) || k.Contains(p.Key))) part.Add(p.Id);
        }
        if (exact.Count == 1) return exact[0];
        if (exact.Count == 0 && part.Count == 1) return part[0];
        return null;
    }

    /// <summary>Teklif'teki yeni siparisleri oneri olarak al. Doner: eklenen sayi</summary>
    public static int Sync(out string error)
    {
        error = null; int added = 0;
        try
        {
            using (SqlConnection con = Db.Open())
            {
                List<Partner> ps = Partners(con);
                List<Dictionary<string, object>> src = new List<Dictionary<string, object>>();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT s.Id, s.IsOrtagi, s.MusteriUnvani, s.SatirlarJson, s.ToplamJson, s.SatisKaynagi, s.OlusturmaTarih FROM " + ReceivableRepo.SourceTable + " s " +
                    "WHERE s.OlusturmaTarih >= @from AND NOT EXISTS (SELECT 1 FROM dbo.OrderSuggestions o WHERE o.SourceOrderId = s.Id) " +
                    "AND NOT EXISTS (SELECT 1 FROM dbo.Deals d WHERE d.SourceOrderId = s.Id) ORDER BY s.Id", con))
                {
                    cmd.Parameters.AddWithValue("@from", From.AddHours(-3));
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < r.FieldCount; i++) row[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                            src.Add(row);
                        }
                }
                RefreshUnreadable(con);
                foreach (Dictionary<string, object> s in src)
                {
                    string partnerRaw = Clean(s["IsOrtagi"]), channel = ReceivableRepo.MapChannel(Clean(s["SatisKaynagi"]));
                    if (partnerRaw == null && channel != ReceivableRepo.CH_CHANNEL) continue;   // kapsam: is ortagi dolu ya da kanal satisi
                    decimal net; string cur, warn = null;
                    if (!ReceivableRepo.ParseMoney(Clean(s["ToplamJson"]), out net, out cur)) { net = 0; cur = "TL"; warn = "Genel toplam okunamadı; tutarı Teklif'te kontrol edin."; }
                    string products = null;
                    try
                    {
                        object[] arr = new JavaScriptSerializer().DeserializeObject(Clean(s["SatirlarJson"]) ?? "[]") as object[];
                        if (arr != null)
                        {
                            List<string> names = new List<string>();
                            foreach (object o in arr) { Dictionary<string, object> d = o as Dictionary<string, object>; if (d != null && d.ContainsKey("urun") && d["urun"] != null) names.Add(Convert.ToString(d["urun"])); }
                            products = string.Join(" · ", names.ToArray());
                        }
                    }
                    catch { }
                    DateTime? od = s["OlusturmaTarih"] == null ? (DateTime?)null : ReceivableRepo.FixLegacyUtc(Convert.ToDateTime(s["OlusturmaTarih"]));
                    using (SqlCommand ins = new SqlCommand(
                        "IF NOT EXISTS (SELECT 1 FROM dbo.OrderSuggestions WHERE SourceOrderId = @o) " +
                        "INSERT INTO dbo.OrderSuggestions (SourceOrderId, OrderDate, PartnerNameRaw, SalesChannel, CustomerName, ProductSummary, NetAmount, Currency, Warning, SuggestedCompanyId) " +
                        "VALUES (@o, @d, @p, @ch, @c, @pr, @n, @cur, @w, @sc)", con))
                    {
                        ins.Parameters.AddWithValue("@o", Convert.ToInt32(s["Id"]));
                        ins.Parameters.AddWithValue("@d", (object)od ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@p", (object)Cut(partnerRaw, 300) ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@ch", (object)channel ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@c", (object)Cut(Clean(s["MusteriUnvani"]), 300) ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@pr", (object)Cut(products, 1000) ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@n", net);
                        ins.Parameters.AddWithValue("@cur", string.IsNullOrEmpty(cur) ? "TL" : cur);
                        ins.Parameters.AddWithValue("@w", (object)warn ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@sc", (object)Match(ps, partnerRaw) ?? DBNull.Value);
                        if (ins.ExecuteNonQuery() > 0) added++;
                    }
                }
            }
        }
        catch (Exception ex) { error = ex.Message; }
        return added;
    }

    /// <summary>Tutari okunamamis BEKLEYEN oneriler: Teklif'te duzeltildiyse tutari guncelle</summary>
    private static void RefreshUnreadable(SqlConnection con)
    {
        List<int[]> ids = new List<int[]>();
        using (SqlCommand cmd = new SqlCommand("SELECT SuggestionId, SourceOrderId FROM dbo.OrderSuggestions WHERE Status = N'Pending' AND NetAmount <= 0", con))
        using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) ids.Add(new[] { r.GetInt32(0), r.GetInt32(1) });
        foreach (int[] x in ids)
        {
            string tj = null;
            using (SqlCommand cmd = new SqlCommand("SELECT ToplamJson FROM " + ReceivableRepo.SourceTable + " WHERE Id = @o", con))
            { cmd.Parameters.AddWithValue("@o", x[1]); tj = Clean(cmd.ExecuteScalar()); }
            decimal net; string cur;
            if (tj == null || !ReceivableRepo.ParseMoney(tj, out net, out cur) || net <= 0) continue;
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.OrderSuggestions SET NetAmount = @n, Currency = @c, Warning = NULL WHERE SuggestionId = @s AND Status = N'Pending'", con))
            { cmd.Parameters.AddWithValue("@n", net); cmd.Parameters.AddWithValue("@c", string.IsNullOrEmpty(cur) ? "TL" : cur); cmd.Parameters.AddWithValue("@s", x[0]); cmd.ExecuteNonQuery(); }
        }
    }

    public static List<OrderSuggestion> List(string status)
    {
        List<OrderSuggestion> l = new List<OrderSuggestion>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT o.*, sc.Name AS SuggestedName, dc.Name AS DealCompanyName, u.FullName AS DecidedByName FROM dbo.OrderSuggestions o " +
            "LEFT JOIN dbo.Companies sc ON sc.CompanyId = o.SuggestedCompanyId LEFT JOIN dbo.Deals d ON d.DealId = o.DealId " +
            "LEFT JOIN dbo.Companies dc ON dc.CompanyId = d.CompanyId LEFT JOIN dbo.Users u ON u.UserId = o.DecidedByUserId " +
            "WHERE o.Status = @s ORDER BY " + (status == "Pending" ? "o.OrderDate DESC, o.SourceOrderId DESC" : "o.DecidedAt DESC"), con))
        {
            cmd.Parameters.AddWithValue("@s", status);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    OrderSuggestion x = new OrderSuggestion();
                    x.SuggestionId = (int)r["SuggestionId"]; x.SourceOrderId = (int)r["SourceOrderId"]; x.OrderDate = r["OrderDate"] as DateTime?;
                    x.PartnerNameRaw = r["PartnerNameRaw"] as string; x.SalesChannel = r["SalesChannel"] as string; x.CustomerName = r["CustomerName"] as string;
                    x.ProductSummary = r["ProductSummary"] as string; x.NetAmount = (decimal)r["NetAmount"]; x.Currency = r["Currency"] as string ?? "TL";
                    x.Warning = r["Warning"] as string; x.SuggestedCompanyId = r["SuggestedCompanyId"] as int?; x.SuggestedCompanyName = r["SuggestedName"] as string;
                    x.Status = (string)r["Status"]; x.DealId = r["DealId"] as int?; x.DealCompanyName = r["DealCompanyName"] as string;
                    x.DecidedByUserId = r["DecidedByUserId"] as int?; x.DecidedByName = r["DecidedByName"] as string; x.DecidedAt = r["DecidedAt"] as DateTime?;
                    l.Add(x);
                }
        }
        return l;
    }

    public static int PendingCount()
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.OrderSuggestions WHERE Status = N'Pending'", con))
            return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static List<KeyValuePair<int, string>> PartnerOptions()
    {
        List<KeyValuePair<int, string>> l = new List<KeyValuePair<int, string>>();
        using (SqlConnection con = Db.Open()) foreach (Partner p in Partners(con)) l.Add(new KeyValuePair<int, string>(p.Id, p.Name));
        return l;
    }

    /// <summary>Oneriyi is ortaginin satis kaydi (Deals) olarak kaydet. Tek islem: anlasma + ciro + oneri durumu.</summary>
    public static string Book(int suggestionId, int companyId, int userId)
    {
        OrderSuggestion sg = null;
        foreach (OrderSuggestion x in List("Pending")) if (x.SuggestionId == suggestionId) sg = x;
        if (sg == null) return "Öneri bulunamadı ya da zaten işlenmiş.";
        if (sg.NetAmount <= 0) return "Siparişin tutarı okunamadı. Önce Teklif'te genel toplamı düzeltin; öneri bir sonraki güncellemede yenilenir.";
        bool isPartner = false;
        foreach (KeyValuePair<int, string> p in PartnerOptions()) if (p.Key == companyId) isPartner = true;   // yalniz Partner turundeki firmalar
        CompanyInfo company = isPartner ? Repo.GetCompany(companyId) : null;
        if (company == null) return "Geçerli bir iş ortağı seçin.";
        List<Tier> tiers = Repo.GetTiers();
        int tierId = 0, newTierId = 0; decimal prior = company.AnnualNetRevenue, newRevenue = prior + sg.NetAmount;
        if (tiers.Count > 0) { TierEngine engine = new TierEngine(tiers); tierId = engine.ResolveTier(prior).TierId; newTierId = engine.ResolveTier(newRevenue).TierId; }
        string title = Cut("Sipariş #" + sg.SourceOrderId + (string.IsNullOrEmpty(sg.ProductSummary) ? "" : " · " + sg.ProductSummary), 200);
        int dealId;
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO dbo.Deals (CompanyId, Title, CustomerName, Amount, AppliedTierId, BaseCommissionRate, MegaDealBonusRate, CommissionAmount, Status, CreatedByUserId, CreatedByVendor, SourceOrderId) " +
                    "OUTPUT INSERTED.DealId VALUES (@c, @t, @cust, @amt, @tier, 0, 0, 0, N'Approved', @uid, 1, @o)", con, tx))
                {
                    cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@t", title);
                    cmd.Parameters.AddWithValue("@cust", Cut(sg.CustomerName ?? "", 200)); cmd.Parameters.AddWithValue("@amt", sg.NetAmount);
                    cmd.Parameters.AddWithValue("@tier", tierId > 0 ? (object)tierId : DBNull.Value); cmd.Parameters.AddWithValue("@uid", userId); cmd.Parameters.AddWithValue("@o", sg.SourceOrderId);
                    dealId = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Companies SET AnnualNetRevenue = @rev" + (newTierId > 0 ? ", CurrentTierId = @tier" : "") +
                                                       ", LastSaleDate = SYSUTCDATETIME(), IsPassive = 0 WHERE CompanyId = @c", con, tx))
                {
                    cmd.Parameters.AddWithValue("@rev", newRevenue); cmd.Parameters.AddWithValue("@c", companyId);
                    if (newTierId > 0) cmd.Parameters.AddWithValue("@tier", newTierId);
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.OrderSuggestions SET Status = N'Booked', DealId = @d, DecidedByUserId = @u, DecidedAt = SYSUTCDATETIME() WHERE SuggestionId = @s AND Status = N'Pending'", con, tx))
                {
                    cmd.Parameters.AddWithValue("@d", dealId); cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@s", suggestionId);
                    if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("Öneri bu arada başka biri tarafından işlendi.");
                }
                tx.Commit();
            }
            catch (SqlException ex)
            {
                tx.Rollback();
                if (ex.Number == 2601 || ex.Number == 2627) return "Bu sipariş zaten bir satış olarak kaydedilmiş.";   // benzersiz sipariş kısıtı
                throw;
            }
            catch (InvalidOperationException ex) { tx.Rollback(); return ex.Message; }
            catch { tx.Rollback(); throw; }
        }
        try { NotifyPartnerAdmins(companyId, sg); } catch { }
        return null;
    }

    public static string Dismiss(int suggestionId, int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.OrderSuggestions SET Status = N'Dismissed', DecidedByUserId = @u, DecidedAt = SYSUTCDATETIME() WHERE SuggestionId = @s AND Status = N'Pending'", con))
        { cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@s", suggestionId); return cmd.ExecuteNonQuery() > 0 ? null : "Öneri bulunamadı ya da zaten işlenmiş."; }
    }
    public static string Restore(int suggestionId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.OrderSuggestions SET Status = N'Pending', DecidedByUserId = NULL, DecidedAt = NULL WHERE SuggestionId = @s AND Status = N'Dismissed'", con))
        { cmd.Parameters.AddWithValue("@s", suggestionId); return cmd.ExecuteNonQuery() > 0 ? null : "Öneri geri alınamadı."; }
    }

    /// <summary>Yalniz bayi yoneticilerine (PartnerAdmin) uygulama ici bildirim</summary>
    private static void NotifyPartnerAdmins(int companyId, OrderSuggestion sg)
    {
        List<int> admins = new List<int>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT UserId FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 AND Role = N'PartnerAdmin'", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) admins.Add(r.GetInt32(0));
        }
        string amt = AmountText(sg.NetAmount, sg.Currency), cust = sg.CustomerName ?? "";
        foreach (int u in admins)
            Repo.AddUserNotification(companyId, u, Cut("Yeni satış kaydı: " + cust, 200), Cut("New sale recorded: " + cust, 200),
                Cut(amt + (string.IsNullOrEmpty(sg.ProductSummary) ? "" : " · " + sg.ProductSummary), 1000), Cut(amt.Replace("KDV", "VAT") + (string.IsNullOrEmpty(sg.ProductSummary) ? "" : " · " + sg.ProductSummary), 1000),
                "Sale", "Partner.aspx#satislar");
    }

    /// <summary>"120.000,00 ₺ + KDV"</summary>
    public static string AmountText(decimal net, string cur)
    {
        string sym = string.IsNullOrEmpty(cur) || cur == "TL" || cur == "TRY" ? "₺" : cur;
        return net.ToString("#,##0.00", TR) + " " + sym + " + KDV";
    }

    private static string Cut(string s, int n) { if (s == null) return null; return s.Length > n ? s.Substring(0, n) : s; }
}
