<%@ WebHandler Language="C#" Class="BroadcastEmailsHandler" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Toplu bildirim: ek e-posta adreslerini analiz eder (ayirir). Gonderimle ayni kod: EmailListParser.Analyze.
public class BroadcastEmailsHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer(); js.MaxJsonLength = 4 * 1024 * 1024;
        if (!Auth.IsLoggedIn || !Auth.IsVendorAdmin) { ctx.Response.StatusCode = 403; ctx.Response.Write("{\"ok\":false}"); return; }
        if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { ctx.Response.StatusCode = 405; ctx.Response.Write("{\"ok\":false}"); return; }
        var f = ctx.Request.Unvalidated.Form;
        List<int> cids = new List<int>();
        string[] sel = f.GetValues("cid");
        if (sel != null) foreach (string s in sel) { int id; if (int.TryParse(s, out id) && id > 0) cids.Add(id); }
        EmailListResult r = EmailListParser.Analyze(f["text"] ?? "", cids, Repo.GetAllUsers());
        ctx.Response.Write(js.Serialize(new { ok = true, valid = r.Valid, covered = r.Covered, typos = r.Typos, invalid = r.Invalid, blocked = r.Blocked,
            duplicates = r.Duplicates, domains = r.Domains, send = r.SendList, truncated = r.Truncated, max = EmailListParser.MaxAddresses }));
    }
}
