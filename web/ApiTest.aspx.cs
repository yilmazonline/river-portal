using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

// Yonetici icin API test araci: formu alir, sunucudan gercek endpoint'e POST atar, yaniti gosterir.
public partial class ApiTestPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string DefaultRef = "";
    public string DefaultTarget = "";
    public string ResultHtml = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendor) { Response.Redirect("Default.aspx"); return; }

        Culture = Auth.Culture;
        UserName = Auth.FullName;
        DefaultRef = "TEST-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        DefaultTarget = "https://portal.folyazilim.com/api/v1/tickets.ashx";

        if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            DoSend();
    }

    private void DoSend()
    {
        string key = (Request.Form["key"] ?? "").Trim();
        string target = (Request.Form["target"] ?? "").Trim();
        if (target.Length == 0) target = DefaultTarget;

        Dictionary<string, object> payload = new Dictionary<string, object>();
        payload["product"] = (Request.Form["product"] ?? "").Trim();
        payload["externalRef"] = (Request.Form["externalRef"] ?? "").Trim();
        payload["subject"] = (Request.Unvalidated.Form["subject"] ?? "").Trim();
        payload["description"] = (Request.Unvalidated.Form["description"] ?? "").Trim();
        payload["priority"] = (Request.Form["priority"] ?? "normal").Trim();
        Dictionary<string, object> rq = new Dictionary<string, object>();
        rq["company"] = (Request.Form["rc"] ?? "").Trim();
        rq["name"] = (Request.Form["rn"] ?? "").Trim();
        rq["email"] = (Request.Form["re"] ?? "").Trim();
        rq["phone"] = (Request.Form["rp"] ?? "").Trim();
        payload["requester"] = rq;

        HttpPostedFile f = Request.Files["file"];
        if (f != null && f.ContentLength > 0)
        {
            byte[] b;
            using (MemoryStream ms = new MemoryStream()) { f.InputStream.CopyTo(ms); b = ms.ToArray(); }
            Dictionary<string, object> att = new Dictionary<string, object>();
            att["filename"] = Path.GetFileName(f.FileName);
            att["contentType"] = f.ContentType;
            att["contentBase64"] = Convert.ToBase64String(b);
            payload["attachments"] = new object[] { att };
        }

        JavaScriptSerializer js = new JavaScriptSerializer();
        js.MaxJsonLength = 64 * 1024 * 1024;
        string body = js.Serialize(payload);

        int status = 0; string respText = ""; string errNote = "";
        try
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(target);
            req.Method = "POST";
            req.ContentType = "application/json; charset=utf-8";
            req.Headers["X-Api-Key"] = key;
            req.Timeout = 30000;
            byte[] data = Encoding.UTF8.GetBytes(body);
            req.ContentLength = data.Length;
            using (Stream s = req.GetRequestStream()) s.Write(data, 0, data.Length);
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                status = (int)resp.StatusCode;
                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) respText = sr.ReadToEnd();
            }
        }
        catch (WebException wex)
        {
            HttpWebResponse r = wex.Response as HttpWebResponse;
            if (r != null)
            {
                status = (int)r.StatusCode;
                using (StreamReader sr = new StreamReader(r.GetResponseStream(), Encoding.UTF8)) respText = sr.ReadToEnd();
            }
            else errNote = wex.Message;
        }
        catch (Exception ex) { errNote = ex.Message; }

        string keyMask = key.Length > 8 ? key.Substring(0, 4) + "..." + key.Substring(key.Length - 4) : "****";
        bool okStatus = status >= 200 && status < 300;
        string badge = okStatus ? "#12A150" : "#E4467A";

        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"card\" style=\"margin-top:18px\"><div class=\"section-h\"><span>Sonuç</span></div><div style=\"padding:8px 20px 20px\">");
        if (errNote.Length > 0)
            sb.Append("<p style=\"color:#E4467A\">İstek gönderilemedi: " + Fmt.Enc(errNote) + "</p>");
        else
        {
            sb.Append("<p style=\"margin:0 0 10px\"><b>HTTP durumu:</b> <span style=\"color:" + badge + ";font-weight:700\">" + status + (okStatus ? " · başarılı" : " · hata") + "</span></p>");
            sb.Append("<div style=\"font-size:12px;color:var(--muted);margin-bottom:4px\">Yanıt gövdesi:</div>");
            sb.Append("<pre style=\"background:var(--glass-2);border:1px solid var(--stroke-2);border-radius:10px;padding:12px;overflow:auto;white-space:pre-wrap;word-break:break-word\">" + Fmt.Enc(respText) + "</pre>");
            if (okStatus) sb.Append("<p style=\"font-size:12.5px;margin-top:10px\">Talebi <a href=\"Tickets.aspx\">Destek kuyruğunda</a> görebilirsiniz.</p>");
        }
        sb.Append("<div style=\"font-size:12px;color:var(--muted);margin:12px 0 4px\">Gönderilen istek (anahtar gizlendi: " + Fmt.Enc(keyMask) + "):</div>");
        sb.Append("<pre style=\"background:var(--glass-2);border:1px solid var(--stroke-2);border-radius:10px;padding:12px;overflow:auto;white-space:pre-wrap;word-break:break-word;max-height:260px\">" + Fmt.Enc(body) + "</pre>");
        sb.Append("</div></div>");
        ResultHtml = sb.ToString();
    }
}
