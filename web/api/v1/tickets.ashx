<%@ WebHandler Language="C#" Class="ApiTicketsHandler" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

// POST /api/v1/tickets  — harici urunlerden destek talebi alma (Faz 1, tek yon)
// Auth: Authorization: Bearer <api_key>
public class ApiTicketsHandler : IHttpHandler
{
    public bool IsReusable { get { return false; } }

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json; charset=utf-8";
        string ip = ctx.Request.UserHostAddress;
        string endpoint = "POST /api/v1/tickets";
        string extRef = null, client = null;

        try
        {
            if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            { Fail(ctx, 405, "method_not_allowed", "Yalnizca POST desteklenir."); Repo.LogApiRequest(null, ip, endpoint, null, 405, "method"); return; }

            // --- Kimlik dogrulama (X-Api-Key veya Authorization: Bearer) ---
            string key = (ctx.Request.Headers["X-Api-Key"] ?? "").Trim();
            if (key.Length == 0)
            {
                string auth = ctx.Request.Headers["Authorization"] ?? "";
                key = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth.Substring(7).Trim() : "";
            }
            ApiClientInfo ci = Repo.ValidateApiClient(key);
            if (ci == null || string.IsNullOrEmpty(ci.Name))
            { Fail(ctx, 401, "unauthorized", "Gecersiz veya pasif API anahtari."); Repo.LogApiRequest(null, ip, endpoint, null, 401, "auth"); return; }
            client = ci.Name;

            // --- Govde ---
            string raw;
            using (StreamReader sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding ?? System.Text.Encoding.UTF8))
                raw = sr.ReadToEnd();
            if (string.IsNullOrEmpty(raw)) { Fail(ctx, 400, "empty_body", "Istek govdesi bos."); Repo.LogApiRequest(client, ip, endpoint, null, 400, "empty"); return; }

            JavaScriptSerializer js = new JavaScriptSerializer();
            js.MaxJsonLength = 64 * 1024 * 1024;
            Dictionary<string, object> d;
            try { d = js.Deserialize<Dictionary<string, object>>(raw); }
            catch { Fail(ctx, 400, "invalid_json", "JSON cozumlenemedi."); Repo.LogApiRequest(client, ip, endpoint, null, 400, "json"); return; }
            if (d == null) { Fail(ctx, 400, "invalid_json", "JSON cozumlenemedi."); return; }

            string product = client; // urun adi anahtardan gelir (guvenli); govdedeki 'product' bilgi amacli
            if (S(d, "product").Length > 0) product = S(d, "product");
            extRef = NullIfEmpty(S(d, "externalRef"));
            string subject = S(d, "subject");
            string description = S(d, "description");
            string priority = MapPriority(S(d, "priority"));

            // requester alt objesi veya duz alanlar
            Dictionary<string, object> rq = d.ContainsKey("requester") ? d["requester"] as Dictionary<string, object> : null;
            string reqCompany = Pick(d, rq, "company");
            string reqName = Pick(d, rq, "name");
            string reqEmail = Pick(d, rq, "email");
            string reqPhone = Pick(d, rq, "phone");

            // --- Zorunlu alan dogrulama ---
            List<string> miss = new List<string>();
            if (subject.Length == 0) miss.Add("subject");
            if (description.Length == 0) miss.Add("description");
            if (reqCompany.Length == 0) miss.Add("requester.company");
            if (reqName.Length == 0) miss.Add("requester.name");
            if (reqEmail.Length == 0 || reqEmail.IndexOf('@') < 1) miss.Add("requester.email");
            if (miss.Count > 0)
            { Fail(ctx, 400, "missing_fields", "Eksik/gecersiz alan: " + string.Join(", ", miss.ToArray())); Repo.LogApiRequest(client, ip, endpoint, extRef, 400, "fields"); return; }

            // --- Idempotency ---
            if (!string.IsNullOrEmpty(extRef))
            {
                string exNo; int exId = Repo.FindApiTicket(product, extRef, out exNo);
                if (exId > 0)
                {
                    Repo.LogApiRequest(client, ip, endpoint, extRef, 200, "duplicate");
                    Ok(ctx, 200, exId, exNo, "New", true);
                    return;
                }
            }

            // --- Talep olustur ---
            // --- Firma cozumu + destek plani kontrolu ---
            int[] owner = Repo.EnsureApiOwner();
            int systemUserId = owner[1];
            int companyId = ci.CompanyId > 0 ? ci.CompanyId : owner[0];
            SupportPlan plan = null;
            if (ci.CompanyId > 0)
            {
                plan = Repo.GetSupportPlan(ci.CompanyId);
                if (plan.Minutes <= 0)
                { Fail(ctx, 403, "support_exhausted", "Firmanin destek suresi dolmustur."); Repo.LogApiRequest(client, ip, endpoint, extRef, 403, "exhausted"); return; }
                if (plan.Until.HasValue && plan.Until.Value.Date < DateTime.Today)
                { Fail(ctx, 403, "support_expired", "Firmanin destek anlasmasinin suresi dolmustur."); Repo.LogApiRequest(client, ip, endpoint, extRef, 403, "expired"); return; }
            }

            string ticketNo;
            int ticketId = Repo.CreateApiTicket(companyId, systemUserId, product, subject, description, priority,
                reqCompany, reqName, reqEmail, reqPhone, extRef, out ticketNo);
            if (ticketId <= 0) { Fail(ctx, 500, "create_failed", "Talep olusturulamadi."); Repo.LogApiRequest(client, ip, endpoint, extRef, 500, "create"); return; }

            // --- Ekler (base64) ---
            List<string> attachNames = new List<string>();
            object attObj = d.ContainsKey("attachments") ? d["attachments"] : null;
            object[] atts = attObj as object[];
            if (atts != null)
            {
                string dir = ctx.Server.MapPath("~/ticketfiles/");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                int saved = 0;
                for (int i = 0; i < atts.Length && saved < 10; i++)
                {
                    Dictionary<string, object> a = atts[i] as Dictionary<string, object>;
                    if (a == null) continue;
                    string fn = S(a, "filename"); string b64 = S(a, "contentBase64");
                    if (fn.Length == 0 || b64.Length == 0) continue;
                    byte[] bytes;
                    try { bytes = Convert.FromBase64String(b64); } catch { continue; }
                    if (bytes.Length <= 0 || bytes.Length > 10 * 1024 * 1024) continue; // <=10 MB
                    if (!TicketFiles.IsAllowed(fn)) continue;   // ortak izin listesi; onceden uzanti kontrolu yoktu
                    string ext = TicketFiles.Ext(fn);
                    string stored = Guid.NewGuid().ToString("N") + ext;
                    try { File.WriteAllBytes(Path.Combine(dir, stored), bytes); }
                    catch { continue; }
                    Repo.AddTicketAttachment(ticketId, null, stored, Path.GetFileName(fn), S(a, "contentType"), bytes.Length);
                    attachNames.Add(Path.GetFileName(fn));
                    saved++;
                }
            }

            // --- Bildirim + destek maili (portaldaki akisla ayni) ---
            try
            {
                Repo.AddNotification(Repo.GetVendorCompanyId2(), "Yeni destek talebi", "New support ticket",
                    "\"" + subject + "\" konulu yeni bir destek talebi geldi (" + product + " / " + reqCompany + ").",
                    "New support ticket: \"" + subject + "\" (" + product + " / " + reqCompany + ").", "Ticket", "TicketDetail.aspx?id=" + ticketId);
            }
            catch { }
            try
            {
                if (Mailer.HasSmtp)
                {
                    string baseUrl = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
                    if (baseUrl.Length == 0) baseUrl = ctx.Request.Url.GetLeftPart(UriPartial.Authority);
                    string detailUrl = baseUrl.TrimEnd('/') + "/TicketDetail.aspx?id=" + ticketId;
                    string supportAddr = (WebConfigurationManager.AppSettings["Support.NotifyEmail"] ?? "").Trim();
                    if (supportAddr.Length > 0)
                    {
                        string names = attachNames.Count > 0 ? string.Join(", ", attachNames.ToArray()) : "";
                        string reqLine = reqName + " · " + reqEmail + (reqPhone.Length > 0 ? " · " + reqPhone : "");
                        string html = Emails.TicketNotify(ticketNo, reqCompany + " (" + product + ")", subject, product, reqLine,
                            "Harici", priority, "", detailUrl, description, names);
                        string err;
                        Mailer.SendTo(supportAddr, "Yeni destek talebi (" + product + ") · " + ticketNo, html, out err);
                    }
                }
            }
            catch { }

            Repo.LogApiRequest(client, ip, endpoint, extRef, 201, "created id=" + ticketId);
            Ok(ctx, 201, ticketId, ticketNo, "New", false, plan);
        }
        catch (Exception ex)
        {
            try { Repo.LogApiRequest(client, ip, endpoint, extRef, 500, "ex:" + ex.Message); }
            catch { }
            Fail(ctx, 500, "server_error", "Sunucu hatasi.");
        }
    }

    // ---- yardimcilar ----
    private static string S(Dictionary<string, object> d, string k)
    {
        if (d == null || !d.ContainsKey(k) || d[k] == null) return "";
        return d[k].ToString().Trim();
    }
    private static string Pick(Dictionary<string, object> flat, Dictionary<string, object> nested, string k)
    {
        string v = nested != null ? S(nested, k) : "";
        if (v.Length == 0) v = S(flat, "requester_" + k);
        if (v.Length == 0) v = S(flat, k); // duz alan da kabul
        return v;
    }
    private static string NullIfEmpty(string s) { return string.IsNullOrEmpty(s) ? null : s; }
    private static string MapPriority(string p)
    {
        p = (p ?? "").ToLowerInvariant();
        if (p == "high" || p == "yuksek" || p == "urgent" || p == "acil") return "High";
        if (p == "low" || p == "dusuk") return "Low";
        return "Normal";
    }
    private static void Ok(HttpContext ctx, int code, int id, string no, string status, bool dup)
    {
        Ok(ctx, code, id, no, status, dup, null);
    }
    private static void Ok(HttpContext ctx, int code, int id, string no, string status, bool dup, SupportPlan plan)
    {
        ctx.Response.StatusCode = code;
        JavaScriptSerializer js = new JavaScriptSerializer();
        Dictionary<string, object> o = new Dictionary<string, object>();
        o["ticketId"] = id; o["ticketNo"] = no; o["status"] = status; o["duplicate"] = dup;
        if (plan != null)
        {
            Dictionary<string, object> s = new Dictionary<string, object>();
            s["minutesLeft"] = plan.Minutes;
            s["until"] = plan.Until.HasValue ? plan.Until.Value.ToString("yyyy-MM-dd") : null;
            o["support"] = s;
        }
        ctx.Response.Write(js.Serialize(o));
    }
    private static void Fail(HttpContext ctx, int code, string err, string msg)
    {
        ctx.Response.StatusCode = code;
        JavaScriptSerializer js = new JavaScriptSerializer();
        Dictionary<string, object> o = new Dictionary<string, object>();
        o["error"] = err; o["message"] = msg;
        ctx.Response.Write(js.Serialize(o));
    }
}
