using System;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Demo talebi olusturur (tercih + 2 alternatif tarih/saat) ve bildirim e-postasi gonderir.
public partial class SubmitDemoPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Partner.aspx"); return; }

        string client = Request.Form["client"];
        string module = Request.Form["module"];
        DateTime? preferred = ParseDt(Request.Form["preferred"]);
        DateTime? alt1 = ParseDt(Request.Form["alt1"]);
        DateTime? alt2 = ParseDt(Request.Form["alt2"]);

        if (string.IsNullOrEmpty(client) || string.IsNullOrEmpty(module))
        { Response.Redirect("Partner.aspx?err=demo"); return; }

        // Tarih kurali: hafta ici + 09:00-18:00
        if (preferred == null || !IsBusiness(preferred.Value))
        { Response.Redirect("Partner.aspx?err=demotime"); return; }
        if (alt1 != null && !IsBusiness(alt1.Value)) alt1 = null;
        if (alt2 != null && !IsBusiness(alt2.Value)) alt2 = null;

        int companyId = Auth.CompanyId;
        int? uid = null;
        var s = Session["UserId"];
        if (s != null) uid = (int)s;

        Repo.SubmitDemo(companyId, uid, client, module, preferred, alt1, alt2);

        // Bildirim e-postasi (SMTP yapilandirilmissa)
        try
        {
            if (Mailer.IsConfigured)
            {
                string subject = "Yeni Demo Talebi · " + client;
                StringBuilder b = new StringBuilder();
                b.Append("<h3>Yeni Demo Talebi</h3>");
                b.Append("<p><b>Firma (Partner):</b> " + Enc(Auth.CompanyName) + "</p>");
                b.Append("<p><b>Talep eden:</b> " + Enc(Auth.FullName) + "</p>");
                b.Append("<p><b>Müşteri:</b> " + Enc(client) + "</p>");
                b.Append("<p><b>Ürün modülü:</b> " + Enc(module) + "</p>");
                b.Append("<p><b>Tercih edilen tarih/saat:</b> " + DtStr(preferred) + "</p>");
                b.Append("<p><b>Alternatif 1:</b> " + DtStr(alt1) + "</p>");
                b.Append("<p><b>Alternatif 2:</b> " + DtStr(alt2) + "</p>");
                string err;
                Mailer.Send(subject, b.ToString(), out err);
                // err loglanabilir; kullaniciyi bekletmiyoruz
            }
        }
        catch { /* e-posta hatasi demo kaydini bozmasin */ }

        Response.Redirect("Partner.aspx?ok=demo");
    }

    private static bool IsBusiness(DateTime d)
    {
        if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) return false;
        TimeSpan t = d.TimeOfDay;
        return t >= new TimeSpan(9, 0, 0) && t <= new TimeSpan(18, 0, 0);
    }

    private static DateTime? ParseDt(string v)
    {
        if (string.IsNullOrEmpty(v)) return null;
        DateTime d;
        // datetime-local formati: 2026-07-31T14:30
        if (DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
        return null;
    }

    private static string DtStr(DateTime? d)
    {
        if (d == null) return "-";
        return d.Value.ToString("dd.MM.yyyy HH:mm", new CultureInfo("tr-TR"));
    }

    private static string Enc(string s)
    {
        return System.Web.HttpUtility.HtmlEncode(s == null ? "" : s);
    }
}
