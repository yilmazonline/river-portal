using System;
using System.Globalization;
using System.Web.Configuration;
using System.Web.UI;

// Sertifika belgesi (A4 yatay, yazdir / PDF). Sahibi veya uretici tarafi gorebilir.
public partial class CertificatePage : Page
{
    public CertificateInfo C = null;
    public string VerifyUrl = "";
    public string VerifyHost = "";
    public string IssuedStr = "", ValidStr = "";
    public bool IsNew = false;
    public bool IsOwner = false;
    public bool IsPreview = false;
    public BadgePalette P = BadgePalette.All[0];
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (Request.QueryString["preview"] == "1")
        {
            // Editor onizlemesi: kayit olusturmadan, girilen sinav adi + renk ile ornek belge
            if (!Auth.CanEditTraining) { Response.Redirect("Exams.aspx"); return; }
            IsPreview = true;
            string ttl = (Request.QueryString["title"] ?? "").Trim();
            if (ttl.Length == 0) ttl = "Sınav Adı";
            if (ttl.Length > 200) ttl = ttl.Substring(0, 200);
            int vm; int.TryParse(Request.QueryString["valid"], out vm);
            C = new CertificateInfo();
            C.CertNo = "RVR-" + DateTime.UtcNow.Year + "-000000";
            C.VerifyCode = "ORNEK00000";
            C.HolderName = "Ad Soyad";
            C.ExamTitle = ttl;
            C.IssuedAt = DateTime.UtcNow;
            C.ValidUntil = (vm > 0 && vm <= 120) ? (DateTime?)DateTime.UtcNow.AddMonths(vm) : null;
            C.BadgeColor = BadgePalette.Norm(Request.QueryString["color"]);
        }
        else
        {
            int id; int.TryParse(Request.QueryString["id"], out id);
            C = id > 0 ? ExamRepo.GetCertificate(id) : null;
            if (C == null) { Response.Redirect("Certificates.aspx"); return; }
            IsOwner = C.UserId == Auth.UserId;
            if (!IsOwner && !Auth.IsVendorSide) { Response.Redirect("Certificates.aspx"); return; }
            IsNew = Request.QueryString["new"] == "1";
        }
        P = BadgePalette.Get(C.BadgeColor);

        string b = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (b.Length == 0) b = Request.Url.GetLeftPart(UriPartial.Authority);
        b = b.TrimEnd('/');
        VerifyUrl = b + "/verify/" + C.VerifyCode;
        VerifyHost = b.Replace("https://", "").Replace("http://", "") + "/verify";
        IssuedStr = C.IssuedAt.ToTrLocal().ToString("dd MMMM yyyy", TR);
        ValidStr = C.ValidUntil.HasValue ? C.ValidUntil.Value.ToTrLocal().ToString("dd MMMM yyyy", TR) : "Süresiz";
    }

    public string V(string s) { return Server.HtmlEncode(s ?? ""); }

    // Muhur icin tirtikli rozet kenari (36 dis / 36 ic kose)
    public string SealPoints
    {
        get
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int n = 72;
            for (int i = 0; i < n; i++)
            {
                double r = (i % 2 == 0) ? 68 : 61;
                double a = Math.PI * 2 * i / n - Math.PI / 2;
                if (i > 0) sb.Append(" ");
                sb.Append((70 + r * Math.Cos(a)).ToString("0.0", CultureInfo.InvariantCulture) + "," + (70 + r * Math.Sin(a)).ToString("0.0", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
    }
    public bool Invalid { get { return C.IsRevoked || C.IsExpired; } }
}
