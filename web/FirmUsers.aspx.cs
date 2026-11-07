using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Firma kullanicilari (JSON) — Hizmet Formu kisi seciminde kullanilir. Yalniz uretici tarafi.
public partial class FirmUsersPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json; charset=utf-8";
        if (!Auth.IsLoggedIn || !Auth.IsVendorSide) { Response.Write("{\"users\":[],\"plan\":{}}"); return; }
        int cid; int.TryParse(Request.QueryString["cid"], out cid);
        if (cid <= 0) { Response.Write("{\"users\":[],\"plan\":{}}"); return; }

        List<UserRow> us = Repo.GetActiveUsersForCompany(cid);
        SupportPlan p = Repo.GetSupportPlan(cid);
        StringBuilder items = new StringBuilder();
        for (int i = 0; i < us.Count; i++)
        {
            if (items.Length > 0) items.Append(",");
            items.Append("{\"id\":" + us[i].UserId + ",\"name\":\"" + J(us[i].FullName) + "\",\"email\":\"" + J(us[i].Email) + "\"}");
        }
        bool expired = p.Until.HasValue && p.Until.Value.Date < DateTime.Today;
        string plan = "{\"minutes\":" + p.Minutes + ",\"hours\":" + p.ProjectHours.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            ",\"until\":\"" + (p.Until.HasValue ? p.Until.Value.ToString("dd.MM.yyyy") : "") + "\",\"expired\":" + (expired ? "true" : "false") + "}";
        Response.Write("{\"users\":[" + items.ToString() + "],\"plan\":" + plan + "}");
    }

    private static string J(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
    }
}
