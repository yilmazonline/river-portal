<%@ WebHandler Language="C#" Class="WatchlistHandler" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Izleme listeleri API (giris yapmis tum kullanicilar). Tum degisiklikler sahiplik denetimli.
//   POST action=create(name) | rename(l,name) | delete(l) | toggle(l,m,on) | reorder(l,mids) | share(l,scope,uids,rec) | copy(l)
//   GET  action=colleagues  -> ayni firmadaki aktif kullanicilar (paylasim icin)
public class WatchlistHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        if (!Auth.IsLoggedIn) { Out(ctx, 403, false, "forbidden", null); return; }
        int uid = Auth.UserId, cid = Auth.CompanyId;
        string a = ctx.Request.QueryString["action"] ?? "";
        var f = ctx.Request.Unvalidated.Form;
        int l; int.TryParse(f["l"], out l);
        try
        {
            if (a == "colleagues")
            {
                List<object> us = new List<object>();
                List<UserRow> rows = Repo.GetActiveUsersForCompany(cid);
                for (int i = 0; i < rows.Count; i++) if (rows[i].UserId != uid) us.Add(new Dictionary<string, object> { { "id", rows[i].UserId }, { "name", rows[i].FullName } });
                Out(ctx, 200, true, null, new Dictionary<string, object> { { "users", us } }); return;
            }
            if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Out(ctx, 405, false, "method", null); return; }

            if (a == "create")
            {
                string name = Clean(f["name"]);
                if (name.Length == 0) { Out(ctx, 400, false, "Liste adı boş olamaz.", null); return; }
                int id = WatchlistRepo.Create(uid, cid, name);
                int m; if (int.TryParse(f["m"], out m) && m > 0) WatchlistRepo.SetItem(id, uid, m, true);   // olustur + ekle
                Out(ctx, 200, true, null, new Dictionary<string, object> { { "id", id }, { "name", name } }); return;
            }
            if (a == "rename") { string name = Clean(f["name"]); if (name.Length > 0) WatchlistRepo.Rename(l, uid, name); Out(ctx, 200, true, null, null); return; }
            if (a == "delete") { WatchlistRepo.Delete(l, uid); Out(ctx, 200, true, null, null); return; }
            if (a == "toggle")
            {
                int m; int.TryParse(f["m"], out m);
                if (m > 0) WatchlistRepo.SetItem(l, uid, m, f["on"] == "1");
                Out(ctx, 200, true, null, null); return;
            }
            if (a == "reorder") { WatchlistRepo.Reorder(l, uid, IntList(f["mids"])); Out(ctx, 200, true, null, null); return; }
            if (a == "copy")
            {
                int id = WatchlistRepo.Copy(l, uid, cid);
                if (id == 0) { Out(ctx, 404, false, "Liste bulunamadı.", null); return; }
                Out(ctx, 200, true, null, new Dictionary<string, object> { { "id", id } }); return;
            }
            if (a == "share")
            {
                if (!WatchlistRepo.IsOwner(l, uid)) { Out(ctx, 403, false, "Bu listeyi yalnız sahibi paylaşabilir.", null); return; }
                string scope = f["scope"] ?? "private";
                // yalniz ayni firmadaki kullanicilar kabul edilir
                HashSet<int> same = new HashSet<int>();
                List<UserRow> rows = Repo.GetActiveUsersForCompany(cid);
                for (int i = 0; i < rows.Count; i++) same.Add(rows[i].UserId);
                List<int> ids = new List<int>();
                foreach (int x in IntList(f["uids"])) if (same.Contains(x)) ids.Add(x);
                bool? rec = Auth.CanEditTraining ? (bool?)(f["rec"] == "1") : null;   // oneri yalniz egitim editoru
                List<int> added = WatchlistRepo.SetShare(l, uid, scope, ids, rec);
                string lname = "";
                List<Watchlist> vis = WatchlistRepo.GetVisible(uid, cid);
                for (int i = 0; i < vis.Count; i++) if (vis[i].ListId == l) lname = vis[i].Name;
                for (int i = 0; i < added.Count; i++)
                    try
                    {
                        Repo.AddUserNotification(cid, added[i], "İzleme listesi paylaşıldı: " + lname, "Watchlist shared: " + lname,
                            Auth.FullName + " sizinle bir izleme listesi paylaştı.", Auth.FullName + " shared a watchlist with you.",
                            "Training", VirtualPathUtility.ToAbsolute("~/Training.aspx") + "#wl-" + l);
                    }
                    catch { }
                Out(ctx, 200, true, null, new Dictionary<string, object> { { "notified", added.Count } }); return;
            }
            Out(ctx, 400, false, "action", null);
        }
        catch (Exception ex) { Out(ctx, 500, false, "server: " + ex.Message, null); }
    }

    private static string Clean(string s)
    {
        s = (s ?? "").Trim();
        return s.Length > 120 ? s.Substring(0, 120) : s;
    }

    private static List<int> IntList(string s)
    {
        List<int> r = new List<int>();
        foreach (string p in (s ?? "").Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)) { int v; if (int.TryParse(p, out v) && v > 0 && !r.Contains(v)) r.Add(v); }
        return r;
    }

    private static void Out(HttpContext ctx, int code, bool ok, string err, Dictionary<string, object> d)
    {
        if (d == null) d = new Dictionary<string, object>();
        d["ok"] = ok; if (err != null) d["error"] = err;
        ctx.Response.StatusCode = code;
        ctx.Response.Write(new JavaScriptSerializer().Serialize(d));
    }
}
