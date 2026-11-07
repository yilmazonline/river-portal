using System;
using System.Collections.Generic;
using System.Web.UI;

// Toplu bildirim mail onizlemesi (HTML). Yalniz portal yoneticileri. Ozel HTML temizlenmis haliyle gosterilir;
// cikarilan ogeler / uyarilar X-Broadcast-Notes basliginda (URL kodlu, | ayrimli) doner.
public partial class BroadcastPreviewPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Duz metin olarak doner: Cloudflare'in "e-posta adresi gizleme" ozelligi yalniz text/html yanitlara uygulanir;
        // HTML donseydi onizlemedeki adresler "[email protected]" olurdu (cerceve betik calistirmadigi icin geri acilamaz).
        // Betik icerigi metin olarak okuyup srcdoc ile cerceveye koyar. nosniff: tarayici bu yaniti sayfa gibi calistirmaz.
        Response.ContentType = "text/plain; charset=utf-8";
        Response.AddHeader("X-Content-Type-Options", "nosniff");
        if (!Auth.IsLoggedIn || !Auth.IsVendorAdmin) { Response.Write(""); return; }
        string err; List<string> notes;
        string html = BroadcastHtml.Build(Request.Unvalidated.Form, true, out err, out notes);
        if (notes.Count > 0) Response.AddHeader("X-Broadcast-Notes", Uri.EscapeDataString(string.Join("|", notes.ToArray())));
        if (html == null) { Response.AddHeader("X-Broadcast-Error", Uri.EscapeDataString(err ?? "")); Response.Write(""); return; }
        Response.Write(html);
    }
}
