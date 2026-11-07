using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

// Alacak detayi: cari secimi, tahsilat, vadelere bolme, tarih guncelleme, hatirlatma, gecmis
public partial class ReceivableDetailPage : Page
{
    public string Culture = "tr", UserName = "";
    public Receivable X;
    public List<RcvInstallment> Inst = new List<RcvInstallment>();
    public List<RcvPayment> Pays = new List<RcvPayment>();
    public List<RcvHistory> Hist = new List<RcvHistory>();
    public string CompaniesJson = "[]", SuggestHtml = "", ContactsJson = "[]", ReminderJson = "null", NextInstJson = "null";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");
    public string M(decimal v) { return ReceivableRepo.Money(v, X != null ? X.Currency : "TL"); }
    public static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }
    public string D(DateTime? d) { return d.HasValue ? d.Value.ToString("dd.MM.yyyy") : "·"; }

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorFirm || !Auth.CanFinance) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;
        int id; int.TryParse(Request.QueryString["id"], out id);
        X = id > 0 ? ReceivableRepo.Get(id) : null;
        if (X == null) { Response.Redirect("Receivables.aspx"); return; }
        Inst = ReceivableRepo.GetInstallments(id); Pays = ReceivableRepo.GetPayments(id); Hist = ReceivableRepo.GetHistory(id);
        JavaScriptSerializer js = new JavaScriptSerializer();

        // cari listesi (bayi + musteri) ve oneriler
        List<object> comps = new List<object>(); List<KeyValuePair<double, CompanyLite>> scored = new List<KeyValuePair<double, CompanyLite>>();
        string[] types = { "Partner", "Customer" };
        foreach (string t in types)
            foreach (CompanyLite c in Repo.GetCompaniesByType(t))
            {
                comps.Add(new { id = c.CompanyId, n = c.Name, t = t == "Partner" ? "Bayi" : "Müşteri" });
                double sc = Math.Max(Score(X.PartnerName, c.Name), Score(X.CustomerName, c.Name));
                if (sc > 0.34) scored.Add(new KeyValuePair<double, CompanyLite>(sc + (t == "Partner" && Score(X.PartnerName, c.Name) >= sc ? 0.001 : 0), c));
            }
        CompaniesJson = js.Serialize(comps);
        scored.Sort(delegate (KeyValuePair<double, CompanyLite> a, KeyValuePair<double, CompanyLite> b) { return b.Key.CompareTo(a.Key); });
        StringBuilder sg = new StringBuilder();
        for (int k = 0; k < scored.Count && k < 3; k++)
            sg.Append("<button type=\"button\" class=\"rc-sug\" data-id=\"" + scored[k].Value.CompanyId + "\" data-n=\"" + E(scored[k].Value.Name) + "\">" + E(scored[k].Value.Name) + "<em>%" + Math.Round(scored[k].Key * 100) + " eşleşme</em></button>");
        SuggestHtml = sg.ToString();

        // hatirlatma: siradaki odenmemis taksit + alici onerileri
        RcvInstallment next = null;
        foreach (RcvInstallment i in Inst) if (i.Left > 0.005m) { next = i; break; }
        if (next != null && X.IsOpen)
        {
            string subj, body; ReceivableRepo.ReminderText(X, next, out subj, out body);
            int days = (int)(next.DueDate.Date - DateTime.Today).TotalDays;
            ReminderJson = js.Serialize(new { scenario = ReceivableRepo.Scenario(days), days = days, subject = subj, body = body, inst = next.InstallmentId });
            List<object> cs2 = new List<object>();
            foreach (RcvContact c in ReceivableRepo.GetContacts(X)) cs2.Add(new { n = c.Name, e = c.Email, s = c.Source });
            ContactsJson = js.Serialize(cs2);
        }
        if (next != null) NextInstJson = js.Serialize(new { id = next.InstallmentId, due = next.DueDate.ToString("yyyy-MM-dd") });
    }

    /// <summary>Unvan benzerligi: anlamli kelimelerin ortusme orani (0..1)</summary>
    private static double Score(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
        HashSet<string> A = Words(a), B = Words(b); if (A.Count == 0 || B.Count == 0) return 0;
        int hit = 0; foreach (string w in A) if (B.Contains(w)) hit++;
        return (double)hit / Math.Min(A.Count, B.Count);
    }
    private static readonly HashSet<string> STOP = new HashSet<string>(new[] { "ltd", "sti", "tic", "san", "ve", "as", "a", "s", "limited", "sirketi", "anonim", "bilgisayar", "yazilim", "bilisim", "teknoloji", "ith", "ihr", "dis", "ins", "paz" });
    private static HashSet<string> Words(string s)
    {
        s = s.Replace("İ", "i").Replace("I", "ı").ToLower(TR).Replace("ı", "i").Replace("ş", "s").Replace("ğ", "g").Replace("ü", "u").Replace("ö", "o").Replace("ç", "c");
        HashSet<string> h = new HashSet<string>();
        foreach (string w in Regex.Split(s, "[^a-z0-9]+")) if (w.Length >= 2 && !STOP.Contains(w)) h.Add(w);
        return h;
    }
    public string StatusChip()
    {
        int? d = X.DaysToDue;
        if (X.Status == "Paid") return "<span class=\"rc-st ok\">Tahsil edildi</span>";
        if (X.Status == "Closed") return "<span class=\"rc-st mute\">Kapatıldı</span>";
        string c = X.Status == "Unassigned" ? "<span class=\"rc-st warn\">Cari seçilmedi</span> " : (X.Status == "Partial" ? "<span class=\"rc-st mute\">Kısmi tahsil</span> " : "");
        if (d.HasValue && d.Value < 0) return c + "<span class=\"rc-st bad\">" + (-d.Value) + " gün geçti</span>";
        if (d.HasValue && d.Value == 0) return c + "<span class=\"rc-st warn\">Vadesi bugün</span>";
        return c + "<span class=\"rc-st info\">" + (d.HasValue ? d.Value + " gün kaldı" : "Tarih yok") + "</span>";
    }
    public string HistIcon(string a)
    {
        switch (a) { case "payment": return "₺"; case "assign": return "✓"; case "split": return "÷"; case "redate": return "↻"; case "remind": return "✉"; case "close": return "×"; case "channel": return "⇄"; case "note": return "✎"; default: return "•"; }
    }
}
