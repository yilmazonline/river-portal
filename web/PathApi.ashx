<%@ WebHandler Language="C#" Class="PathApiHandler" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Sifirdan Uzmanliga: durum, kutlama, paylasim tercihi, editor siralamasi
public class PathApiHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!Auth.IsLoggedIn) { ctx.Response.StatusCode = 401; ctx.Response.Write("{\"ok\":false}"); return; }
        string op = ctx.Request["op"] ?? "state";
        bool post = string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase);
        try
        {
            if (op == "state")
            {
                LearningPath.EnsureRetro(Auth.UserId, Auth.CompanyId);
                ctx.Response.Write(js.Serialize(LearningPath.State(Auth.UserId, Auth.CompanyId))); return;
            }
            if (op == "celebrate")
            {
                List<object> l = new List<object>();
                List<PathStep> steps = LearningPath.GetSteps(Auth.UserId, Auth.CompanyId);
                foreach (EarnedBadge b in LearningPath.TakeUncelebrated(Auth.UserId, Auth.CompanyId))
                {
                    int no = 0; for (int i = 0; i < steps.Count; i++) if ("video:" + steps[i].MaterialId == b.Key) no = i + 1;
                    l.Add(new { key = b.Key, title = b.Title, svg = LearningPath.BadgeSvg(b.Key, no, true, 120) });
                }
                ctx.Response.Write(js.Serialize(new { ok = true, items = l })); return;
            }
            if (op == "team")
            {
                ctx.Response.Write(js.Serialize(new { ok = true, items = LearningPath.Team(Auth.UserId, Auth.CompanyId) })); return;
            }
            if (op == "share")
            {
                if (post) LearningPath.SetShare(Auth.UserId, ctx.Request.Form["on"] == "1");
                ctx.Response.Write(js.Serialize(new { ok = true, on = LearningPath.GetShare(Auth.UserId) })); return;
            }
            if (op == "save" && post)
            {
                if (!Auth.CanEditTraining) { ctx.Response.StatusCode = 403; ctx.Response.Write("{\"ok\":false}"); return; }
                Uri refr = ctx.Request.UrlReferrer;
                if (refr == null || !string.Equals(refr.Host, ctx.Request.Url.Host, StringComparison.OrdinalIgnoreCase)) { ctx.Response.StatusCode = 403; ctx.Response.Write("{\"ok\":false}"); return; }
                List<int> ids = new List<int>();
                foreach (string s in (ctx.Request.Form["mids"] ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) { int m; if (int.TryParse(s, out m) && m > 0 && !ids.Contains(m)) ids.Add(m); }
                LearningPath.SaveOrder(ids);
                ctx.Response.Write(js.Serialize(new { ok = true, count = ids.Count })); return;
            }
            ctx.Response.StatusCode = 400; ctx.Response.Write("{\"ok\":false}");
        }
        catch (Exception ex) { ctx.Response.StatusCode = 500; ctx.Response.Write(js.Serialize(new { ok = false, error = ex.Message })); }
    }
}
