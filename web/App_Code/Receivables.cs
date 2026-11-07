using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

// ============================================================================
// Alacak takibi: River Teklif siparisleri -> tahsilat
// Kaynak: [RiverTeklif].[dbo].[Siparisler] (ayni SQL Server; web.config Receivables.Source)
// Kurallar: tutar = genel toplam (iskonto sonrasi, KDV haric) x (1 + KDV); tarih = OdemeTarihi
//           yoksa fatura tarihi + varsayilan vade (hafta sonu -> pazartesi).
//           Esik tarihinden onceki siparisler "tahsil edildi" sayilir (gecmis veri).
// ============================================================================
public class Receivable
{
    public int ReceivableId, SourceOrderId;
    public int? CompanyId;
    public string SalesChannel;   // Sorumlu: Kanal Satış / Direkt Satış / null
    public string CompanyName, CompanyType, PartnerName, CustomerName, SalesRep, Tags, ProductSummary, PaymentNote, OrderEmails, Warning, Status, CloseReason, Currency;
    public DateTime? OrderDate, InvoiceDate, NextDue, AssignedAt;
    public decimal NetAmount, VatRate, GrossAmount, Paid;
    public DateTime? LastPay; public int PayCount;   // son tahsilat tarihi ve odeme sayisi (ReceivablePayments)
    public bool IsLegacy;
    public bool IsManual;   // Teklif siparisi olmadan elle girilen alacak (SourceOrderId negatif)
    public string OrderLabel { get { return IsManual ? "Manuel kayıt" : "Teklif siparişi #" + SourceOrderId; } }
    public decimal Remaining { get { decimal r = GrossAmount - Paid; return r < 0 ? 0 : r; } }
    public bool IsOpen { get { return Status == "Unassigned" || Status == "Open" || Status == "Partial"; } }
    /// <summary>Vadeye kalan gun (+) / gecen gun (-); acik degilse null</summary>
    public int? DaysToDue { get { if (!IsOpen || !NextDue.HasValue) return null; return (int)(NextDue.Value.Date - DateTime.Today).TotalDays; } }
    public bool IsOverdue { get { int? d = DaysToDue; return d.HasValue && d.Value < 0; } }
}
public class RcvInstallment { public int InstallmentId, ReceivableId, SeqNo; public DateTime DueDate; public decimal Amount, PaidAmount; public decimal Left { get { decimal l = Amount - PaidAmount; return l < 0 ? 0 : l; } } }
public class RcvPayment { public int PaymentId; public DateTime PayDate; public decimal Amount; public string Method, Bank, Note, CreatedBy; public DateTime CreatedAt; }
public class RcvHistory { public string Action, Detail, UserName; public DateTime CreatedAt; }
public class RcvContact { public string Name, Email, Source; }

public static class ReceivableRepo
{
    // ------------------------------------------------------------------ ayarlar
    private static string Cfg(string k, string d) { string v = WebConfigurationManager.AppSettings[k]; return string.IsNullOrEmpty(v) ? d : v.Trim(); }
    public static string SourceTable
    {
        get
        {
            string s = Cfg("Receivables.Source", "[RiverTeklif].[dbo].[Siparisler]");
            // SQL enjeksiyonuna karsi yalniz [db].[sema].[tablo] bicimi kabul edilir
            return Regex.IsMatch(s, @"^\[?[A-Za-z0-9_]+\]?\.\[?[A-Za-z0-9_]+\]?\.\[?[A-Za-z0-9_]+\]?$") ? s : "[RiverTeklif].[dbo].[Siparisler]";
        }
    }
    public static decimal VatRate { get { decimal v; return decimal.TryParse(Cfg("Receivables.VatRate", "20"), NumberStyles.Number, CultureInfo.InvariantCulture, out v) ? v : 20m; } }
    public static int DefaultTermDays { get { int v; return int.TryParse(Cfg("Receivables.DefaultTermDays", "15"), out v) ? v : 15; } }
    public static DateTime LegacyCutoff { get { DateTime d; return DateTime.TryParseExact(Cfg("Receivables.LegacyCutoff", "2026-09-01"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : new DateTime(2026, 9, 1); } }
    public static string DigestTo { get { return Cfg("Receivables.DigestTo", "yilmaz.guney@river.com.tr,deniz@river.com.tr"); } }
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");
    public static string Money(decimal v, string cur) { return v.ToString("#,##0.00", TR) + " " + (string.IsNullOrEmpty(cur) ? "TL" : cur); }

    // ------------------------------------------------------------------ ayristirma yardimcilari
    /// <summary>"915.840,00 TL" -> 915840.00 ; "915840.00" de kabul edilir</summary>
    public static bool ParseMoney(string s, out decimal amount, out string cur)
    {
        amount = 0; cur = "TL";
        if (string.IsNullOrEmpty(s)) return false;
        s = s.Trim();
        Match m = Regex.Match(s, @"^(-?[\d.]+,\d{1,2})\s*([^\d\s]*)$");
        if (m.Success)
        {
            if (m.Groups[2].Value.Length > 0) cur = m.Groups[2].Value.Replace("₺", "TL");
            return decimal.TryParse(m.Groups[1].Value.Replace(".", "").Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }
        m = Regex.Match(s, @"^(-?\d+(\.\d+)?)\s*([^\d\s]*)$");
        if (m.Success)
        {
            if (m.Groups[3].Value.Length > 0) cur = m.Groups[3].Value;
            return decimal.TryParse(m.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }
        return false;
    }
    /// <summary>Toplu ice aktarmada gece yarisi UTC olarak yazilmis kayitlar (saat tam 21:00:00) Turkiye saatine cevrilir</summary>
    public static DateTime FixLegacyUtc(DateTime d) { return (d.Hour == 21 && d.Minute == 0 && d.Second == 0 && d.Millisecond == 0) ? d.AddHours(3) : d; }
    public static DateTime NextBusinessDay(DateTime d)
    {
        if (d.DayOfWeek == DayOfWeek.Saturday) return d.AddDays(2);
        if (d.DayOfWeek == DayOfWeek.Sunday) return d.AddDays(1);
        return d;
    }
    public const string CH_CHANNEL = "Kanal Satış", CH_DIRECT = "Direkt Satış";
    private static bool? channelCol = null;
    /// <summary>SalesChannel sutunu var mi (migration 38)? Yoksa aktarim sorumlu bilgisi olmadan surer.</summary>
    public static bool HasChannelColumn(SqlConnection con, SqlTransaction tx)
    {
        if (channelCol.HasValue) return channelCol.Value;
        using (SqlCommand cmd = new SqlCommand("SELECT COL_LENGTH('dbo.Receivables', 'SalesChannel')", con, tx))
        { object o = cmd.ExecuteScalar(); channelCol = o != null && o != DBNull.Value; }
        return channelCol.Value;
    }
    /// <summary>Teklif SatisKaynagi -> Sorumlu. Bos ya da taninmayan deger icin null (cikarim yapilmaz).</summary>
    public static string MapChannel(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        string f = raw.Replace("İ", "i").Replace("I", "ı").ToLower(new CultureInfo("tr-TR"));
        if (f.Contains("direkt") || f.Contains("direct")) return CH_DIRECT;
        if (f.Contains("kanal") || f.Contains("bayi") || f.Contains("partner")) return CH_CHANNEL;
        return null;
    }
    private static string Clean(object o) { string s = o == null || o == DBNull.Value ? null : Convert.ToString(o); if (s == null) return null; s = s.Trim(); return s.Length == 0 || s == "NULL" || s == "[]" ? null : s; }

    // ------------------------------------------------------------------ eslestirme (Teklif -> portal)
    private static DateTime lastSync = DateTime.MinValue;
    private static readonly object syncLock = new object();
    /// <summary>Yeni siparisleri alacak olarak ice alir. force=false ise en fazla 2 dakikada bir calisir.</summary>
    public static int Sync(bool force, out string error)
    {
        error = null;
        lock (syncLock)
        {
            if (!force && (DateTime.Now - lastSync).TotalSeconds < 120) return 0;
            lastSync = DateTime.Now;
        }
        int added = 0;
        try
        {
            DateTime installed = InstalledAtTr();
            using (SqlConnection con = Db.Open())
            {
                List<Dictionary<string, object>> src = new List<Dictionary<string, object>>();
                string sql = "SELECT s.Id, s.IsOrtagi, s.MusteriUnvani, s.CariBilgileri, s.SatirlarJson, s.MailToJson, s.ToplamJson, s.Baslatan, s.Etiket, s.SatisKaynagi, " +
                             "s.OlusturmaTarih, s.OdemeTarihi, s.FaturaGonderimTarih, s.Detaylar FROM " + SourceTable + " s " +
                             "WHERE NOT EXISTS (SELECT 1 FROM dbo.Receivables r WHERE r.SourceOrderId = s.Id) " +
                             "AND (s.FaturaGonderimTarih IS NOT NULL OR s.OlusturmaTarih < @inst) ORDER BY s.Id";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    // eski duzendeki (fatura tarihsiz) siparisler yalniz kurulum anindan oncekiler icin bir kez
                    cmd.Parameters.AddWithValue("@inst", installed.AddHours(-3));
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < r.FieldCount; i++) row[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                            src.Add(row);
                        }
                }
                foreach (Dictionary<string, object> s in src) { if (ImportOne(con, s)) added++; }
            }
        }
        catch (Exception ex) { error = ex.Message; }
        return added;
    }

    private static DateTime InstalledAtTr()
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT SettingValue FROM dbo.ReceivableSettings WHERE SettingKey = N'InstalledAtUtc'", con))
            {
                object o = cmd.ExecuteScalar(); DateTime d;
                if (o != null && DateTime.TryParse(Convert.ToString(o), CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d.AddHours(3);
            }
        }
        catch { }
        return DateTime.Now;
    }

    private static bool ImportOne(SqlConnection con, Dictionary<string, object> s)
    {
        int id = Convert.ToInt32(s["Id"]);
        decimal net; string cur;
        string warn = null;
        if (!ParseMoney(Clean(s["ToplamJson"]), out net, out cur)) { net = 0; warn = "Genel toplam okunamadı (" + (Clean(s["ToplamJson"]) ?? "boş") + "); tutarı kontrol edin."; }
        // satirlar: urun ozeti + tutar saglamasi + doviz kontrolu
        string products = null;
        string lines = Clean(s["SatirlarJson"]);
        if (lines != null)
        {
            try
            {
                object[] arr = new JavaScriptSerializer().DeserializeObject(lines) as object[];
                if (arr != null)
                {
                    decimal sum = 0; List<string> names = new List<string>(); bool foreign = false;
                    foreach (object o in arr)
                    {
                        Dictionary<string, object> d = o as Dictionary<string, object>; if (d == null) continue;
                        if (d.ContainsKey("tutar") && d["tutar"] != null) sum += Convert.ToDecimal(d["tutar"], CultureInfo.InvariantCulture);
                        if (d.ContainsKey("urun") && d["urun"] != null) names.Add(Convert.ToString(d["urun"]));
                        if (d.ContainsKey("paraBirimi") && d["paraBirimi"] != null && Convert.ToString(d["paraBirimi"]) != "TL") foreign = true;
                    }
                    products = string.Join(" · ", names.ToArray());
                    if (net > 0 && Math.Abs(sum - net) > 1m) warn = "Satır toplamı (" + Money(sum, cur) + ") genel toplamla (" + Money(net, cur) + ") uyuşmuyor; kontrol edin.";
                    if (foreign) warn = (warn == null ? "" : warn + " ") + "Dövizli satır var; döviz cinsini kontrol edin.";
                }
            }
            catch { }
        }
        decimal vat = VatRate;
        decimal gross = Math.Round(net * (1m + vat / 100m), 2);
        DateTime? orderDate = s["OlusturmaTarih"] == null ? (DateTime?)null : FixLegacyUtc(Convert.ToDateTime(s["OlusturmaTarih"]));
        DateTime? invoiceDate = s["FaturaGonderimTarih"] == null ? (DateTime?)null : Convert.ToDateTime(s["FaturaGonderimTarih"]);
        DateTime due;
        if (s["OdemeTarihi"] != null) due = Convert.ToDateTime(s["OdemeTarihi"]).Date;
        else due = NextBusinessDay((invoiceDate ?? orderDate ?? DateTime.Today).Date.AddDays(DefaultTermDays));
        bool legacy = orderDate.HasValue && orderDate.Value < LegacyCutoff;
        string note = string.Join("\n", new[] { Clean(s["CariBilgileri"]), Clean(s["Detaylar"]) }.Where2());

        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                int rid;
                bool hasCh = HasChannelColumn(con, tx);
                using (SqlCommand cmd = new SqlCommand((
                    "INSERT INTO dbo.Receivables (SourceOrderId, SalesChannel, OrderDate, InvoiceDate, PartnerName, CustomerName, SalesRep, Tags, ProductSummary, NetAmount, VatRate, GrossAmount, Currency, PaymentNote, OrderEmails, Warning, Status, IsLegacy) " +
                    "VALUES (@src, @ch, @od, @inv, @pn, @cn, @rep, @tags, @prod, @net, @vat, @gross, @cur, @note, @mails, @warn, @st, @leg); SELECT CAST(SCOPE_IDENTITY() AS INT);")
                    .Replace(hasCh ? "\u0000" : "SalesChannel, ", "").Replace(hasCh ? "\u0000" : "@ch, ", ""), con, tx))
                {
                    cmd.Parameters.AddWithValue("@src", id);
                    cmd.Parameters.AddWithValue("@ch", (object)MapChannel(Clean(s["SatisKaynagi"])) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@od", (object)orderDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@inv", (object)invoiceDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pn", (object)Clean(s["IsOrtagi"]) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cn", (object)Clean(s["MusteriUnvani"]) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@rep", (object)Clean(s["Baslatan"]) ?? DBNull.Value);
                    string tags = Clean(s["Etiket"]) ?? "";
                    cmd.Parameters.AddWithValue("@tags", tags.Length > 0 ? (object)tags : DBNull.Value);
                    cmd.Parameters.AddWithValue("@prod", (object)products ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@net", net);
                    cmd.Parameters.AddWithValue("@vat", vat);
                    cmd.Parameters.AddWithValue("@gross", gross);
                    cmd.Parameters.AddWithValue("@cur", cur);
                    cmd.Parameters.AddWithValue("@note", note.Length > 0 ? (object)note : DBNull.Value);
                    cmd.Parameters.AddWithValue("@mails", (object)Clean(s["MailToJson"]) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@warn", (object)warn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@st", legacy ? "Paid" : "Unassigned");
                    cmd.Parameters.AddWithValue("@leg", legacy);
                    rid = (int)cmd.ExecuteScalar();
                }
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ReceivableInstallments (ReceivableId, SeqNo, DueDate, Amount, PaidAmount) VALUES (@r, 1, @d, @a, @p)", con, tx))
                {
                    cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@d", due);
                    cmd.Parameters.AddWithValue("@a", gross); cmd.Parameters.AddWithValue("@p", legacy ? gross : 0m);
                    cmd.ExecuteNonQuery();
                }
                Hist(con, tx, rid, "import", legacy
                    ? "Geçmiş veri: " + LegacyCutoff.AddDays(-1).ToString("dd.MM.yyyy") + " ve öncesindeki sipariş, tahsil edildi sayıldı."
                    : "Teklif siparişi #" + id + " alacak olarak eklendi. Tutar " + Money(gross, cur) + " (KDV %" + vat.ToString("0.##", TR) + " dahil), tahmini tarih " + due.ToString("dd.MM.yyyy") + ".", null);
                tx.Commit();
                return true;
            }
            catch { tx.Rollback(); return false; }
        }
    }

    // ------------------------------------------------------------------ okuma
    private const string SELECT_LIST =
        "SELECT r.*, c.Name AS CompanyName, c.CompanyType, ISNULL(p.Paid, 0) AS Paid, p.LastPay, p.PayCount, n.NextDue FROM dbo.Receivables r " +
        "LEFT JOIN dbo.Companies c ON c.CompanyId = r.CompanyId " +
        "OUTER APPLY (SELECT SUM(Amount) AS Paid, MAX(PayDate) AS LastPay, COUNT(*) AS PayCount FROM dbo.ReceivablePayments x WHERE x.ReceivableId = r.ReceivableId) p " +
        "OUTER APPLY (SELECT MIN(DueDate) AS NextDue FROM dbo.ReceivableInstallments i WHERE i.ReceivableId = r.ReceivableId AND i.Amount - i.PaidAmount > 0.005) n ";
    private static bool HasCol(SqlDataReader r, string name) { for (int i = 0; i < r.FieldCount; i++) if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase)) return true; return false; }
    private static Receivable Map(SqlDataReader r)
    {
        Receivable x = new Receivable();
        x.ReceivableId = (int)r["ReceivableId"]; x.SourceOrderId = (int)r["SourceOrderId"];
        x.CompanyId = r["CompanyId"] as int?; x.CompanyName = r["CompanyName"] as string; x.CompanyType = r["CompanyType"] as string;
        x.SalesChannel = HasCol(r, "SalesChannel") ? r["SalesChannel"] as string : null;
        x.PartnerName = r["PartnerName"] as string; x.CustomerName = r["CustomerName"] as string; x.SalesRep = r["SalesRep"] as string;
        x.Tags = r["Tags"] as string; x.ProductSummary = r["ProductSummary"] as string; x.PaymentNote = r["PaymentNote"] as string;
        x.OrderEmails = r["OrderEmails"] as string; x.Warning = r["Warning"] as string; x.Status = (string)r["Status"]; x.CloseReason = r["CloseReason"] as string;
        x.Currency = (string)r["Currency"]; x.OrderDate = r["OrderDate"] as DateTime?; x.InvoiceDate = r["InvoiceDate"] as DateTime?;
        x.AssignedAt = r["AssignedAt"] as DateTime?; x.NextDue = r["NextDue"] as DateTime?;
        x.NetAmount = (decimal)r["NetAmount"]; x.VatRate = (decimal)r["VatRate"]; x.GrossAmount = (decimal)r["GrossAmount"];
        x.Paid = Convert.ToDecimal(r["Paid"]); x.IsLegacy = (bool)r["IsLegacy"];
        x.LastPay = r["LastPay"] as DateTime?; x.PayCount = r["PayCount"] == DBNull.Value ? 0 : Convert.ToInt32(r["PayCount"]);
        x.IsManual = HasCol(r, "IsManual") ? (bool)r["IsManual"] : x.SourceOrderId < 0;
        // gecmis veri: odeme kaydi olmadan tahsil edildi sayilir
        if (x.IsLegacy && x.Status == "Paid" && x.Paid == 0) x.Paid = x.GrossAmount;
        return x;
    }
    public static List<Receivable> GetAll()
    {
        List<Receivable> list = new List<Receivable>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(SELECT_LIST + "ORDER BY CASE WHEN r.Status IN (N'Unassigned', N'Open', N'Partial') THEN 0 ELSE 1 END, n.NextDue, r.SourceOrderId DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(Map(r));
        return list;
    }
    public static Receivable Get(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(SELECT_LIST + "WHERE r.ReceivableId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader()) return r.Read() ? Map(r) : null;
        }
    }
    public static List<RcvInstallment> GetInstallments(int id)
    {
        List<RcvInstallment> list = new List<RcvInstallment>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.ReceivableInstallments WHERE ReceivableId = @id ORDER BY DueDate, SeqNo", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    RcvInstallment i = new RcvInstallment();
                    i.InstallmentId = (int)r["InstallmentId"]; i.ReceivableId = id; i.SeqNo = (int)r["SeqNo"];
                    i.DueDate = (DateTime)r["DueDate"]; i.Amount = (decimal)r["Amount"]; i.PaidAmount = (decimal)r["PaidAmount"];
                    list.Add(i);
                }
        }
        return list;
    }
    /// <summary>Tum acik alacaklarin odenmemis taksitleri (panel hesaplari icin)</summary>
    public static List<RcvInstallment> GetOpenInstallments()
    {
        List<RcvInstallment> list = new List<RcvInstallment>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT i.* FROM dbo.ReceivableInstallments i JOIN dbo.Receivables r ON r.ReceivableId = i.ReceivableId " +
                                               "WHERE r.Status IN (N'Unassigned', N'Open', N'Partial') AND i.Amount - i.PaidAmount > 0.005", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                RcvInstallment i = new RcvInstallment();
                i.InstallmentId = (int)r["InstallmentId"]; i.ReceivableId = (int)r["ReceivableId"]; i.SeqNo = (int)r["SeqNo"];
                i.DueDate = (DateTime)r["DueDate"]; i.Amount = (decimal)r["Amount"]; i.PaidAmount = (decimal)r["PaidAmount"];
                list.Add(i);
            }
        return list;
    }
    public static decimal PaidBetween(DateTime from, DateTime to)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM dbo.ReceivablePayments WHERE PayDate >= @a AND PayDate < @b", con))
        {
            cmd.Parameters.AddWithValue("@a", from.Date); cmd.Parameters.AddWithValue("@b", to.Date);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }
    }
    /// <summary>Tum tahsilatlar, alacak bazinda, eskiden yeniye (liste ve disa aktarim icin tek sorgu)</summary>
    public static Dictionary<int, List<RcvPayment>> PaymentsByReceivable()
    {
        Dictionary<int, List<RcvPayment>> d = new Dictionary<int, List<RcvPayment>>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ReceivableId, PaymentId, PayDate, Amount, Method FROM dbo.ReceivablePayments ORDER BY ReceivableId, PayDate, PaymentId", con))
        using (SqlDataReader r = cmd.ExecuteReader())
            while (r.Read())
            {
                int id = r.GetInt32(0); List<RcvPayment> l;
                if (!d.TryGetValue(id, out l)) { l = new List<RcvPayment>(); d[id] = l; }
                RcvPayment p = new RcvPayment(); p.PaymentId = r.GetInt32(1); p.PayDate = r.GetDateTime(2); p.Amount = r.GetDecimal(3); p.Method = r.IsDBNull(4) ? null : r.GetString(4);
                l.Add(p);
            }
        return d;
    }

    public static List<RcvPayment> GetPayments(int id)
    {
        List<RcvPayment> list = new List<RcvPayment>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT p.*, u.FullName FROM dbo.ReceivablePayments p LEFT JOIN dbo.Users u ON u.UserId = p.CreatedByUserId WHERE p.ReceivableId = @id ORDER BY p.PayDate DESC, p.PaymentId DESC", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    RcvPayment p = new RcvPayment();
                    p.PaymentId = (int)r["PaymentId"]; p.PayDate = (DateTime)r["PayDate"]; p.Amount = (decimal)r["Amount"];
                    p.Method = r["Method"] as string; p.Bank = r["Bank"] as string; p.Note = r["Note"] as string;
                    p.CreatedBy = r["FullName"] as string; p.CreatedAt = (DateTime)r["CreatedAt"];
                    list.Add(p);
                }
        }
        return list;
    }
    public static List<RcvHistory> GetHistory(int id)
    {
        List<RcvHistory> list = new List<RcvHistory>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT h.*, u.FullName FROM dbo.ReceivableHistory h LEFT JOIN dbo.Users u ON u.UserId = h.UserId WHERE h.ReceivableId = @id ORDER BY h.CreatedAt DESC, h.HistoryId DESC", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    RcvHistory h = new RcvHistory();
                    h.Action = (string)r["Action"]; h.Detail = r["Detail"] as string; h.UserName = r["FullName"] as string; h.CreatedAt = (DateTime)r["CreatedAt"];
                    list.Add(h);
                }
        }
        return list;
    }
    /// <summary>Hatirlatma alici onerileri: carinin portal kullanicilari + siparisin gonderildigi e-postalar</summary>
    public static List<RcvContact> GetContacts(Receivable x)
    {
        List<RcvContact> list = new List<RcvContact>(); HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (x.CompanyId.HasValue)
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT FullName, Email FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 ORDER BY FullName", con))
            {
                cmd.Parameters.AddWithValue("@c", x.CompanyId.Value);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) { string e = r["Email"] as string; if (!string.IsNullOrEmpty(e) && seen.Add(e)) { RcvContact c = new RcvContact(); c.Name = r["FullName"] as string; c.Email = e; c.Source = "portal"; list.Add(c); } }
            }
        }
        if (!string.IsNullOrEmpty(x.OrderEmails))
        {
            try
            {
                object[] arr = new JavaScriptSerializer().DeserializeObject(x.OrderEmails) as object[];
                if (arr != null) foreach (object o in arr) { string e = Convert.ToString(o).Trim(); if (e.Length > 3 && seen.Add(e)) { RcvContact c = new RcvContact(); c.Name = ""; c.Email = e; c.Source = "order"; list.Add(c); } }
            }
            catch { }
        }
        return list;
    }

    // ------------------------------------------------------------------ islemler (hepsi tek islem icinde + gecmis)
    private static void Hist(SqlConnection con, SqlTransaction tx, int rid, string action, string detail, int? userId)
    {
        using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ReceivableHistory (ReceivableId, Action, Detail, UserId) VALUES (@r, @a, @d, @u)", con, tx))
        {
            cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@a", action);
            cmd.Parameters.AddWithValue("@d", (object)detail ?? DBNull.Value); cmd.Parameters.AddWithValue("@u", (object)userId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
    /// <summary>Tahsilatlari taksitlere vade sirasiyla dagitir, durumu gunceller</summary>
    private static void Recompute(SqlConnection con, SqlTransaction tx, int rid)
    {
        decimal paid;
        using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM dbo.ReceivablePayments WHERE ReceivableId = @r", con, tx))
        { cmd.Parameters.AddWithValue("@r", rid); paid = Convert.ToDecimal(cmd.ExecuteScalar()); }
        List<KeyValuePair<int, decimal>> inst = new List<KeyValuePair<int, decimal>>();
        using (SqlCommand cmd = new SqlCommand("SELECT InstallmentId, Amount FROM dbo.ReceivableInstallments WHERE ReceivableId = @r ORDER BY DueDate, SeqNo", con, tx))
        {
            cmd.Parameters.AddWithValue("@r", rid);
            using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) inst.Add(new KeyValuePair<int, decimal>((int)r[0], (decimal)r[1]));
        }
        decimal left = paid;
        foreach (KeyValuePair<int, decimal> i in inst)
        {
            decimal p = Math.Min(i.Value, Math.Max(0, left)); left -= p;
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ReceivableInstallments SET PaidAmount = @p WHERE InstallmentId = @i", con, tx))
            { cmd.Parameters.AddWithValue("@p", p); cmd.Parameters.AddWithValue("@i", i.Key); cmd.ExecuteNonQuery(); }
        }
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.Receivables SET UpdatedAt = SYSUTCDATETIME(), Status = CASE WHEN Status = N'Closed' THEN Status " +
            "WHEN @paid >= GrossAmount - 0.005 AND GrossAmount > 0 THEN N'Paid' WHEN CompanyId IS NULL THEN N'Unassigned' WHEN @paid > 0 THEN N'Partial' ELSE N'Open' END " +
            "WHERE ReceivableId = @r", con, tx))
        { cmd.Parameters.AddWithValue("@paid", paid); cmd.Parameters.AddWithValue("@r", rid); cmd.ExecuteNonQuery(); }
    }
    private delegate void TxWork(SqlConnection con, SqlTransaction tx);
    private static void InTx(TxWork w)
    {
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try { w(con, tx); tx.Commit(); }
            catch { tx.Rollback(); throw; }
        }
    }

    /// <summary>Manuel alacak: kayit + tek vade satiri + gecmis; cari secildiyse ayni islemde atanir ve takibe girer. Doner: yeni ReceivableId</summary>
    public static int CreateManual(int companyId, string companyName, string customer, string partner, string channel, string product, string rep, string note,
                                   DateTime invoiceDate, DateTime due, decimal net, decimal vat, string cur, int userId)
    {
        decimal gross = Math.Round(net * (1 + vat / 100m), 2);
        int rid = 0;
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            bool hasCh = HasChannelColumn(con, tx);
            int src;
            // cakismayan negatif numara (es zamanli girislerde ayni numara verilmesin)
            using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MIN(SourceOrderId), 0) - 1 FROM dbo.Receivables WITH (UPDLOCK, HOLDLOCK) WHERE SourceOrderId < 0", con, tx))
                src = Math.Min(-1, Convert.ToInt32(cmd.ExecuteScalar()));
            using (SqlCommand cmd = new SqlCommand((
                "INSERT INTO dbo.Receivables (SourceOrderId, SalesChannel, OrderDate, InvoiceDate, PartnerName, CustomerName, SalesRep, ProductSummary, NetAmount, VatRate, GrossAmount, Currency, PaymentNote, Status, IsLegacy, IsManual, CreatedByUserId) " +
                "VALUES (@src, @ch, @inv, @inv, @pn, @cn, @rep, @prod, @net, @vat, @gross, @cur, @note, N'Unassigned', 0, 1, @u); SELECT CAST(SCOPE_IDENTITY() AS INT);")
                .Replace(hasCh ? "\u0000" : "SalesChannel, ", "").Replace(hasCh ? "\u0000" : "@ch, ", ""), con, tx))
            {
                cmd.Parameters.AddWithValue("@src", src);
                cmd.Parameters.AddWithValue("@ch", (object)channel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inv", invoiceDate.Date);
                cmd.Parameters.AddWithValue("@pn", (object)partner ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cn", (object)customer ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@rep", (object)rep ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@prod", (object)product ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@net", net); cmd.Parameters.AddWithValue("@vat", vat); cmd.Parameters.AddWithValue("@gross", gross);
                cmd.Parameters.AddWithValue("@cur", cur);
                cmd.Parameters.AddWithValue("@note", (object)note ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@u", userId);
                rid = (int)cmd.ExecuteScalar();
            }
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ReceivableInstallments (ReceivableId, SeqNo, DueDate, Amount, PaidAmount) VALUES (@r, 1, @d, @a, 0)", con, tx))
            { cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@d", due.Date); cmd.Parameters.AddWithValue("@a", gross); cmd.ExecuteNonQuery(); }
            Hist(con, tx, rid, "manual", "Manuel alacak kaydı oluşturuldu. Tutar " + Money(gross, cur) + " (KDV %" + vat.ToString("0.##", TR) + " dahil), vade " + due.ToString("dd.MM.yyyy") + ".", userId);
            if (companyId > 0)
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Receivables SET CompanyId = @c, AssignedAt = SYSUTCDATETIME() WHERE ReceivableId = @r", con, tx))
                { cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@r", rid); cmd.ExecuteNonQuery(); }
                Recompute(con, tx, rid);
                Hist(con, tx, rid, "assign", "Alacaklı olunan cari: " + companyName + ". Alacak takibe girdi.", userId);
            }
        });
        return rid;
    }

    public static void Assign(int rid, int companyId, string companyName, int userId)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Receivables SET CompanyId = @c, AssignedAt = SYSUTCDATETIME() WHERE ReceivableId = @r", con, tx))
            { cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@r", rid); cmd.ExecuteNonQuery(); }
            Recompute(con, tx, rid);
            Hist(con, tx, rid, "assign", "Alacaklı olunan cari: " + companyName + ". Alacak takibe girdi.", userId);
        });
    }
    public static void AddPayment(int rid, DateTime date, decimal amount, string method, string bank, string note, int userId, string cur)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ReceivablePayments (ReceivableId, PayDate, Amount, Method, Bank, Note, CreatedByUserId) VALUES (@r, @d, @a, @m, @b, @n, @u)", con, tx))
            {
                cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@d", date.Date); cmd.Parameters.AddWithValue("@a", amount);
                cmd.Parameters.AddWithValue("@m", (object)method ?? DBNull.Value); cmd.Parameters.AddWithValue("@b", (object)bank ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@n", (object)note ?? DBNull.Value); cmd.Parameters.AddWithValue("@u", userId);
                cmd.ExecuteNonQuery();
            }
            Recompute(con, tx, rid);
            Hist(con, tx, rid, "payment", "Tahsilat: " + Money(amount, cur) + ", " + date.ToString("dd.MM.yyyy") + (string.IsNullOrEmpty(method) ? "" : ", " + method) + (string.IsNullOrEmpty(note) ? "" : ". " + note), userId);
        });
    }
    /// <summary>Kalan tutari vadelere boler. amounts null ise esit bolunur (kurus farki son taksite).</summary>
    public static void Split(int rid, int count, DateTime first, string interval, List<decimal> amounts, int userId, string cur)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            decimal gross, paid; DateTime? lastPay = null;
            using (SqlCommand cmd = new SqlCommand("SELECT GrossAmount, (SELECT ISNULL(SUM(Amount),0) FROM dbo.ReceivablePayments WHERE ReceivableId = @r), (SELECT MAX(PayDate) FROM dbo.ReceivablePayments WHERE ReceivableId = @r) FROM dbo.Receivables WHERE ReceivableId = @r", con, tx))
            {
                cmd.Parameters.AddWithValue("@r", rid);
                using (SqlDataReader r = cmd.ExecuteReader()) { r.Read(); gross = (decimal)r[0]; paid = Convert.ToDecimal(r[1]); if (!r.IsDBNull(2)) lastPay = (DateTime)r[2]; }
            }
            decimal remaining = gross - paid;
            if (remaining <= 0.005m) throw new InvalidOperationException("Kalan tutar yok; bölünecek bir şey bulunmuyor.");
            List<decimal> parts = new List<decimal>();
            if (amounts != null && amounts.Count == count)
            {
                decimal s = 0; foreach (decimal a in amounts) { if (a <= 0) throw new InvalidOperationException("Taksit tutarları sıfırdan büyük olmalı."); s += a; }
                if (Math.Abs(s - remaining) > 0.01m) throw new InvalidOperationException("Taksitlerin toplamı (" + Money(s, cur) + ") kalan tutara (" + Money(remaining, cur) + ") eşit olmalı.");
                parts = amounts;
            }
            else
            {
                decimal each = Math.Floor(remaining / count * 100m) / 100m;
                for (int i = 0; i < count; i++) parts.Add(i < count - 1 ? each : remaining - each * (count - 1));
            }
            using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.ReceivableInstallments WHERE ReceivableId = @r", con, tx)) { cmd.Parameters.AddWithValue("@r", rid); cmd.ExecuteNonQuery(); }
            int seq = 1; StringBuilder sb = new StringBuilder();
            if (paid > 0.005m)
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ReceivableInstallments (ReceivableId, SeqNo, DueDate, Amount) VALUES (@r, @s, @d, @a)", con, tx))
                { cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@s", seq++); cmd.Parameters.AddWithValue("@d", (lastPay ?? DateTime.Today).Date); cmd.Parameters.AddWithValue("@a", paid); cmd.ExecuteNonQuery(); }
            }
            for (int i = 0; i < parts.Count; i++)
            {
                DateTime d = interval == "week" ? first.AddDays(7 * i) : first.AddMonths(i);
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ReceivableInstallments (ReceivableId, SeqNo, DueDate, Amount) VALUES (@r, @s, @d, @a)", con, tx))
                { cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@s", seq++); cmd.Parameters.AddWithValue("@d", d.Date); cmd.Parameters.AddWithValue("@a", parts[i]); cmd.ExecuteNonQuery(); }
                sb.Append((i > 0 ? ", " : "") + d.ToString("dd.MM.yyyy") + " " + Money(parts[i], cur));
            }
            Recompute(con, tx, rid);
            Hist(con, tx, rid, "split", "Kalan " + Money(remaining, cur) + " " + count + " vadeye bölündü: " + sb, userId);
        });
    }
    public static void Redate(int rid, int installmentId, DateTime newDate, string reason, int userId)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            DateTime old;
            using (SqlCommand cmd = new SqlCommand("SELECT DueDate FROM dbo.ReceivableInstallments WHERE InstallmentId = @i AND ReceivableId = @r", con, tx))
            { cmd.Parameters.AddWithValue("@i", installmentId); cmd.Parameters.AddWithValue("@r", rid); object o = cmd.ExecuteScalar(); if (o == null) throw new InvalidOperationException("Taksit bulunamadı."); old = (DateTime)o; }
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ReceivableInstallments SET DueDate = @d WHERE InstallmentId = @i", con, tx))
            { cmd.Parameters.AddWithValue("@d", newDate.Date); cmd.Parameters.AddWithValue("@i", installmentId); cmd.ExecuteNonQuery(); }
            Recompute(con, tx, rid);
            Hist(con, tx, rid, "redate", "Tahsilat tarihi " + old.ToString("dd.MM.yyyy") + " → " + newDate.ToString("dd.MM.yyyy") + ". Gerekçe: " + reason, userId);
        });
    }
    public static void Close(int rid, string reason, int userId)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Receivables SET Status = N'Closed', CloseReason = @c, UpdatedAt = SYSUTCDATETIME() WHERE ReceivableId = @r", con, tx))
            { cmd.Parameters.AddWithValue("@c", reason); cmd.Parameters.AddWithValue("@r", rid); cmd.ExecuteNonQuery(); }
            Hist(con, tx, rid, "close", "Alacak kapatıldı. Gerekçe: " + reason, userId);
        });
    }
    public static void Reopen(int rid, int userId)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Receivables SET Status = N'Open', CloseReason = NULL WHERE ReceivableId = @r AND Status = N'Closed'", con, tx))
            { cmd.Parameters.AddWithValue("@r", rid); cmd.ExecuteNonQuery(); }
            Recompute(con, tx, rid);
            Hist(con, tx, rid, "reopen", "Alacak yeniden açıldı.", userId);
        });
    }
    public static void SetChannel(int rid, string channel, int userId)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            string old;
            using (SqlCommand cmd = new SqlCommand("SELECT SalesChannel FROM dbo.Receivables WHERE ReceivableId = @r", con, tx))
            { cmd.Parameters.AddWithValue("@r", rid); old = cmd.ExecuteScalar() as string; }
            if (old == channel) return;
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Receivables SET SalesChannel = @c, UpdatedAt = SYSUTCDATETIME() WHERE ReceivableId = @r", con, tx))
            { cmd.Parameters.AddWithValue("@c", (object)channel ?? DBNull.Value); cmd.Parameters.AddWithValue("@r", rid); cmd.ExecuteNonQuery(); }
            Hist(con, tx, rid, "channel", "Sorumlu: " + (old ?? "belirtilmemiş") + " → " + (channel ?? "belirtilmemiş"), userId);
        });
    }
    public static void AddNote(int rid, string note, int userId)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx) { Hist(con, tx, rid, "note", note, userId); });
    }
    public static void LogReminder(int rid, string scenario, int days, string recipients, string subject, bool ok, string err, int userId)
    {
        InTx(delegate (SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ReceivableReminders (ReceivableId, Scenario, DaysValue, Recipients, Subject, Success, Error, SentByUserId) VALUES (@r, @s, @d, @to, @sub, @ok, @e, @u)", con, tx))
            {
                cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@s", scenario); cmd.Parameters.AddWithValue("@d", days);
                cmd.Parameters.AddWithValue("@to", recipients); cmd.Parameters.AddWithValue("@sub", subject); cmd.Parameters.AddWithValue("@ok", ok);
                cmd.Parameters.AddWithValue("@e", (object)err ?? DBNull.Value); cmd.Parameters.AddWithValue("@u", userId);
                cmd.ExecuteNonQuery();
            }
            Hist(con, tx, rid, "remind", (ok ? "Hatırlatma gönderildi: " : "Hatırlatma gönderilemedi: ") + recipients + (ok ? "" : " (" + err + ")"), userId);
        });
    }

    // ------------------------------------------------------------------ hatirlatma metni (3 senaryo)
    public static string Scenario(int days) { return days > 0 ? "before" : (days == 0 ? "today" : "overdue"); }
    public static void ReminderText(Receivable x, RcvInstallment inst, out string subject, out string body)
    {
        int days = (int)(inst.DueDate.Date - DateTime.Today).TotalDays;
        string amt = Money(inst.Left, x.Currency), due = inst.DueDate.ToString("dd.MM.yyyy");
        string who = x.CompanyName ?? x.CustomerName ?? "";
        string order = (x.IsManual ? "Manuel kayıt" : "Sipariş no: " + x.SourceOrderId) + (string.IsNullOrEmpty(x.ProductSummary) ? "" : " · " + x.ProductSummary);
        if (days > 0)
        {
            subject = "Ödeme hatırlatması · vadesine " + days + " gün kaldı";
            body = "Sayın " + who + " yetkilisi,\n\n" + due + " tarihinde vadesi dolacak " + amt + " tutarındaki ödemenizi hatırlatmak isteriz. Vadesine " + days + " gün kaldı.\n\n" + order + "\n\nÖdemenizi gerçekleştirdiyseniz bu mesajı dikkate almayınız. Sorularınız için bizimle iletişime geçebilirsiniz.\n\nSaygılarımızla,\nFol Yazılım A.Ş.";
        }
        else if (days == 0)
        {
            subject = "Ödeme hatırlatması · vadesi bugün";
            body = "Sayın " + who + " yetkilisi,\n\n" + amt + " tutarındaki ödemenizin vadesi bugün (" + due + ") dolmaktadır.\n\n" + order + "\n\nÖdemenizi gerçekleştirdiyseniz bu mesajı dikkate almayınız. Sorularınız için bizimle iletişime geçebilirsiniz.\n\nSaygılarımızla,\nFol Yazılım A.Ş.";
        }
        else
        {
            subject = "Ödeme hatırlatması · vadesi " + (-days) + " gün önce doldu";
            body = "Sayın " + who + " yetkilisi,\n\nVadesi " + due + " tarihinde, yani " + (-days) + " gün önce dolan " + amt + " tutarındaki ödemeniz henüz tarafımıza ulaşmamıştır. Ödemenizin en kısa sürede yapılmasını rica ederiz.\n\n" + order + "\n\nÖdemenizi gerçekleştirdiyseniz dekontunu iletmenizi rica ederiz. Sorularınız için bizimle iletişime geçebilirsiniz.\n\nSaygılarımızla,\nFol Yazılım A.Ş.";
        }
    }

    // ------------------------------------------------------------------ haftalik ozet (pazartesi, yalniz DigestTo adreslerine)
    public static int SendWeeklyDigestIfDue(out string error)
    {
        error = null;
        if (DateTime.Today.DayOfWeek != DayOfWeek.Monday) return 0;
        string week = DateTime.Today.ToString("yyyy-MM-dd");
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT SettingValue FROM dbo.ReceivableSettings WHERE SettingKey = N'LastDigest'", con))
            { object o = cmd.ExecuteScalar(); if (o != null && Convert.ToString(o) == week) return 0; }
            string ignore; Sync(true, out ignore);
            List<Receivable> all = GetAll(); Dictionary<int, Receivable> byId = new Dictionary<int, Receivable>();
            foreach (Receivable r in all) byId[r.ReceivableId] = r;
            List<RcvInstallment> inst = GetOpenInstallments();
            DateTime today = DateTime.Today, weekEnd = today.AddDays(7), monthEnd = new DateTime(today.Year, today.Month, 1).AddMonths(1);
            decimal overdue = 0, thisWeek = 0, thisMonth = 0; int unassigned = 0;
            List<RcvInstallment> od = new List<RcvInstallment>(), wk = new List<RcvInstallment>();
            foreach (RcvInstallment i in inst)
            {
                if (i.DueDate < today) { overdue += i.Left; od.Add(i); }
                else if (i.DueDate < weekEnd) { thisWeek += i.Left; wk.Add(i); }
                if (i.DueDate >= today && i.DueDate < monthEnd) thisMonth += i.Left;
            }
            foreach (Receivable r in all) if (r.Status == "Unassigned") unassigned++;
            od.Sort(delegate (RcvInstallment a, RcvInstallment b) { return a.DueDate.CompareTo(b.DueDate); });
            wk.Sort(delegate (RcvInstallment a, RcvInstallment b) { return a.DueDate.CompareTo(b.DueDate); });
            string baseUrl = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "https://portal.folyazilim.com").TrimEnd('/');
            StringBuilder h = new StringBuilder();
            h.Append("<p style=\"margin:0 0 16px\">Haftalık alacak özeti · " + today.ToString("dd.MM.yyyy") + "</p>");
            h.Append("<table style=\"width:100%;border-collapse:collapse;margin-bottom:18px\"><tr>");
            h.Append(Kpi("Gecikmiş", Money(overdue, "TL"), "#C8174F") + Kpi("Bu hafta beklenen", Money(thisWeek, "TL"), "#2E6DE0") + Kpi("Bu ay beklenen", Money(thisMonth, "TL"), "#7A3BEA"));
            h.Append("</tr></table>");
            if (unassigned > 0) h.Append("<p style=\"margin:0 0 16px;color:#8E4E3F\"><b>" + unassigned + "</b> alacakta cari seçimi bekleniyor.</p>");
            h.Append(Rows("Vadesi geçenler", od, byId, true)); h.Append(Rows("Bu hafta vadesi dolacaklar", wk, byId, false));
            h.Append("<p style=\"margin:18px 0 0\"><a href=\"" + baseUrl + "/Receivables.aspx\" style=\"color:#7A3BEA;font-weight:600\">Alacak takibini aç</a></p>");
            string html = Emails.Shell("Haftalık alacak özeti", h.ToString());
            string err = null; int sent = 0;
            foreach (string to in DigestTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string e1; if (Mailer.SendTo(to.Trim(), "Haftalık alacak özeti · " + today.ToString("dd.MM.yyyy"), html, out e1)) sent++; else err = e1;
            }
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT 1 FROM dbo.ReceivableSettings WHERE SettingKey = N'LastDigest') UPDATE dbo.ReceivableSettings SET SettingValue = @w WHERE SettingKey = N'LastDigest' ELSE INSERT INTO dbo.ReceivableSettings VALUES (N'LastDigest', @w)", con))
            { cmd.Parameters.AddWithValue("@w", week); cmd.ExecuteNonQuery(); }
            error = err; return sent;
        }
        catch (Exception ex) { error = ex.Message; return 0; }
    }
    private static string Kpi(string label, string value, string color)
    {
        return "<td style=\"padding:12px;border:1px solid #E6E9F4;border-radius:10px\"><div style=\"font-size:12px;color:#66729A\">" + label + "</div><div style=\"font-size:18px;font-weight:700;color:" + color + "\">" + value + "</div></td>";
    }
    private static string Rows(string title, List<RcvInstallment> items, Dictionary<int, Receivable> byId, bool overdue)
    {
        if (items.Count == 0) return "";
        StringBuilder s = new StringBuilder("<h3 style=\"font-size:14px;margin:14px 0 8px\">" + title + " (" + items.Count + ")</h3><table style=\"width:100%;border-collapse:collapse;font-size:13px\">");
        foreach (RcvInstallment i in items)
        {
            Receivable r; if (!byId.TryGetValue(i.ReceivableId, out r)) continue;
            int d = (int)(i.DueDate.Date - DateTime.Today).TotalDays;
            s.Append("<tr><td style=\"padding:7px 0;border-bottom:1px solid #EEF0F6\">" + HttpUtility.HtmlEncode(r.CompanyName ?? r.CustomerName ?? "Cari seçilmedi") + " <span style=\"color:#8A95B8\">· #" + r.SourceOrderId + "</span></td>" +
                     "<td style=\"padding:7px 0;border-bottom:1px solid #EEF0F6;text-align:right;white-space:nowrap\">" + Money(i.Left, r.Currency) + "</td>" +
                     "<td style=\"padding:7px 0 7px 12px;border-bottom:1px solid #EEF0F6;text-align:right;white-space:nowrap;color:" + (overdue ? "#C8174F" : "#2E6DE0") + "\">" + i.DueDate.ToString("dd.MM.yyyy") + (overdue ? " · " + (-d) + " gün geçti" : (d == 0 ? " · bugün" : " · " + d + " gün kaldı")) + "</td></tr>");
        }
        return s.Append("</table>").ToString();
    }
}

// C# 5 yardimcisi: bos/null olmayanlari al (LINQ kullanmadan)
public static class RcvExt
{
    public static string[] Where2(this string[] a)
    {
        List<string> l = new List<string>(); foreach (string s in a) if (!string.IsNullOrEmpty(s)) l.Add(s); return l.ToArray();
    }
}
