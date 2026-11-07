using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Bildirim listesi (JSON) — bell her sayfada common.js ile bunu ceker.
public partial class NotifPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json; charset=utf-8";
        if (!Auth.IsLoggedIn) { Response.Write("{\"count\":0,\"items\":[]}"); return; }

        string action = (Request.QueryString["action"] ?? "list").Trim();
        int cid = Auth.CompanyId;

        // Rol bazlı gizleme: üretici personeli yalnız yetki alanındaki bildirimleri görür
        string[] allowed = null;
        if (Auth.IsVendorFirm && !Auth.IsVendor)
        {
            var al = new List<string>();
            al.Add("Version"); al.Add("System");
            if (Auth.IsSupportAgent) al.Add("Ticket");
            if (Auth.CanEditTraining) al.Add("ExamRequest");
            allowed = al.ToArray();
        }

        if (action == "markall" && string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            try { Repo.MarkAllRead(cid, allowed); } catch { }
            Response.Write("{\"ok\":true}");
            return;
        }

        if (action == "markread")
        {
            int nid; int.TryParse(Request.QueryString["id"], out nid);
            if (nid > 0) { try { Repo.MarkNotificationRead(nid); } catch { } }
            Response.Write("{\"ok\":true}");
            return;
        }

        string c = Auth.Culture;
        List<NotifRow> list = Repo.GetNotifications(cid, Auth.UserId, 15, allowed);
        int cnt = 0;
        StringBuilder items = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            NotifRow n = list[i];
            if (!n.IsRead) cnt++;
            if (items.Length > 0) items.Append(",");
            items.Append("{\"id\":" + n.NotificationId + ",\"t\":\"" + J(n.Title(c)) + "\",\"b\":\"" + J(n.Body(c)) + "\",\"u\":" + (n.IsRead ? "false" : "true") + ",\"l\":\"" + J(n.Link ?? "") + "\"}");
        }
        Response.Write("{\"count\":" + cnt + ",\"items\":[" + items.ToString() + "]}");
    }

    private static string J(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
    }
}
