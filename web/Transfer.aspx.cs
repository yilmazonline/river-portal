using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Herkese acik indirme sayfasi: /t/{token}. Giris gerektirmez; kisiye ozel link (+ opsiyonel sifre).
public partial class TransferPublicPage : Page
{
    public string State = "";        // ok | password | notfound | expired | limit
    public TransferInfo T = null;
    public TransferRecipient R = null;
    public string FilesHtml = "";
    public string Token = "";
    public string ExpiresStr = "";
    public string PwdError = "";
    public int FileCount = 0;
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.AppendHeader("X-Robots-Tag", "noindex");
        Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
        object rt = Page.RouteData != null ? Page.RouteData.Values["token"] : null;
        Token = (rt != null ? rt.ToString() : (Request.QueryString["k"] ?? "")).Trim();

        R = TransferRepo.GetRecipientByToken(Token);
        T = R != null ? TransferRepo.GetTransfer(R.TransferId) : null;
        if (R == null || T == null || T.Status == "Uploading") { State = "notfound"; return; }
        if (T.Status == "Cancelled") { State = "expired"; return; }
        if (!T.IsLive) { State = "expired"; return; }
        ExpiresStr = T.ExpiresAt.ToTrLocal().ToString("dd MMMM yyyy HH:mm", TR);

        if (T.HasPassword && !(Session["tp_" + Token] is bool))
        {
            State = "password";
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                string p = Request.Form["pwd"] ?? "";
                if (p.Length > 0 && PasswordHasher.Verify(p, T.PasswordHash)) { Session["tp_" + Token] = true; Response.Redirect(ResolveUrl("~/t/" + Token), false); return; }
                PwdError = "Şifre hatalı. Lütfen size iletilen şifreyi kontrol edin.";
            }
            return;
        }
        if (T.MaxDownloads.HasValue && R.DownloadCount >= T.MaxDownloads.Value) { State = "limit"; return; }

        State = "ok";
        List<TransferFile> files = TransferRepo.GetFiles(T.TransferId);
        FileCount = files.Count;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < files.Count; i++)
        {
            string href = ResolveUrl("~/TransferDownload.ashx") + "?k=" + Server.UrlEncode(Token) + "&f=" + files[i].FileId;
            sb.Append("<div class=\"fl\"><span class=\"fi\">" + Ext(files[i].OriginalName) + "</span><div class=\"fm\"><b>" + Server.HtmlEncode(files[i].OriginalName) + "</b>" +
                "<span>" + TransferRepo.FormatSize(files[i].SizeBytes) + "</span></div><a class=\"dl\" href=\"" + href + "\" data-dl=\"1\">İndir</a></div>");
        }
        FilesHtml = sb.ToString();
    }

    private static string Ext(string name)
    {
        int i = name.LastIndexOf('.');
        string e = i >= 0 && i < name.Length - 1 ? name.Substring(i + 1).ToUpperInvariant() : "DOSYA";
        return e.Length > 4 ? e.Substring(0, 4) : e;
    }

    public string V(string s) { return Server.HtmlEncode(s ?? ""); }
    public string U(string p) { return ResolveUrl(p); }
}
