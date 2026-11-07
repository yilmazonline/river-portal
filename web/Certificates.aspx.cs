using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Sertifikalarim: kullanicinin sertifikalari (yoksa salt okunur bilgi mesaji).
public partial class CertificatesPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string CardsHtml = "";
    public int Count = 0;
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        Culture = Auth.Culture; UserName = Auth.FullName;
        List<CertificateInfo> list;
        try { list = ExamRepo.GetUserCertificates(Auth.UserId); } catch { list = new List<CertificateInfo>(); }
        Count = list.Count;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            CertificateInfo c = list[i];
            string state = c.IsRevoked ? "<span class=\"st cancelled\">İptal edildi</span>" : c.IsExpired ? "<span class=\"st pending\">Süresi doldu</span>" : "<span class=\"st approved\">Geçerli</span>";
            sb.Append("<a class=\"ce-card\" href=\"Certificate.aspx?id=" + c.CertificateId + "\">" +
                "<div class=\"ce-seal2\">" + ExamRepo.SealSvg(58, c.IsRevoked || c.IsExpired, c.BadgeColor) + "</div>" +
                "<div class=\"ce-body\"><div class=\"ce-t\">" + Fmt.Enc(c.ExamTitle) + "</div>" +
                "<div class=\"ce-m\">" + Fmt.Enc(c.CertNo) + " · " + c.IssuedAt.ToTrLocal().ToString("dd MMMM yyyy", TR) + " · Puan " + c.Score + "</div>" +
                "<div style=\"margin-top:8px\">" + state + "</div></div><span class=\"tym-arw\">→</span></a>");
        }
        CardsHtml = sb.ToString();
    }
}
