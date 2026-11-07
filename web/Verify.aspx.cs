using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;

// Herkese acik sertifika dogrulama (giris gerektirmez). Yalniz tahmin edilemez dogrulama kodu ile sorgular.
public partial class VerifyPage : Page
{
    public string Code = "";
    public bool Searched = false;
    public CertificateInfo C = null;
    public string State = "";      // valid | revoked | expired | notfound
    public string IssuedStr = "", ValidStr = "";
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.AppendHeader("X-Robots-Tag", "noindex");

        // Kod: /verify/{kod} rotasindan ya da ?c= parametresinden
        object rc = Page.RouteData != null ? Page.RouteData.Values["code"] : null;
        string raw = rc != null ? rc.ToString() : (Request.QueryString["c"] ?? "");
        Code = Regex.Replace(raw.Trim().ToUpperInvariant(), "[^A-Z0-9]", "");

        // Kanonik adres: eski /Verify.aspx?c=KOD (basilmis QR'lar) ve /verify?c=KOD -> /verify/KOD
        bool viaAspx = Request.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase);
        if (viaAspx || (rc == null && Code.Length > 0))
        {
            string target = ResolveUrl("~/verify") + (Code.Length > 0 && Code.Length <= 20 ? "/" + Code : "");
            if (viaAspx) Response.RedirectPermanent(target, true); else Response.Redirect(target, true);
            return;
        }
        if (Code.Length == 0) return;
        Searched = true;
        if (Code.Length > 20) { State = "notfound"; return; }
        try { C = ExamRepo.GetCertificateByCode(Code); } catch { C = null; }
        if (C == null) { State = "notfound"; return; }
        State = C.IsRevoked ? "revoked" : C.IsExpired ? "expired" : "valid";
        IssuedStr = C.IssuedAt.ToTrLocal().ToString("dd MMMM yyyy", TR);
        ValidStr = C.ValidUntil.HasValue ? C.ValidUntil.Value.ToTrLocal().ToString("dd MMMM yyyy", TR) : "Süresiz";
    }

    public string V(string s) { return Server.HtmlEncode(s ?? ""); }
    public string U(string p) { return ResolveUrl(p); }
}
