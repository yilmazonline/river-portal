<%@ WebHandler Language="C#" Class="ApiBalanceHandler" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;

// GET /api/v1/balance  — anahtarin bagli oldugu firmanin kalan destek suresi + tarihi + gonderebilir mi
// Auth: X-Api-Key: <anahtar>  (veya Authorization: Bearer <anahtar>)
public class ApiBalanceHandler : IHttpHandler
{
    public bool IsReusable { get { return false; } }

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json; charset=utf-8";
        string key = (ctx.Request.Headers["X-Api-Key"] ?? "").Trim();
        if (key.Length == 0)
        {
            string auth = ctx.Request.Headers["Authorization"] ?? "";
            key = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth.Substring(7).Trim() : "";
        }
        ApiClientInfo ci = Repo.ValidateApiClient(key);
        if (ci == null || string.IsNullOrEmpty(ci.Name))
        { ctx.Response.StatusCode = 401; ctx.Response.Write("{\"error\":\"unauthorized\"}"); return; }

        var o = new Dictionary<string, object>();
        o["product"] = ci.Name;
        o["clientId"] = ci.ClientId ?? "";

        if (ci.CompanyId <= 0)
        {
            o["companyBound"] = false;
            o["canSubmit"] = true; // firmaya bagli degil (eski davranis)
        }
        else
        {
            SupportPlan p = Repo.GetSupportPlan(ci.CompanyId);
            bool expired = p.Until.HasValue && p.Until.Value.Date < DateTime.Today;
            bool canSubmit = p.Minutes > 0 && !expired;
            o["companyBound"] = true;
            o["minutesLeft"] = p.Minutes;
            o["until"] = p.Until.HasValue ? p.Until.Value.ToString("yyyy-MM-dd") : null;
            o["canSubmit"] = canSubmit;
            if (!canSubmit) o["reason"] = (p.Minutes <= 0) ? "support_exhausted" : "support_expired";
        }
        ctx.Response.Write(new JavaScriptSerializer().Serialize(o));
    }
}
