using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Firma (müşteri/bayi) destek analizi: talepler, harcanan süre, en aktif kullanıcılar, PDF indir.
public partial class FirmAnalyticsPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string FirmName = "";
    public int Cid = 0;
    public int TotalT = 0, ResolvedT = 0, OpenT = 0, TotalMin = 0;
    public string BalanceStr = "";
    public string StatusBars = "";
    public string TopUsers = "";
    public string UserTable = "";
    public string RecentRows = "";

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    private class UStat { public string Name; public int Count; public int Minutes; }

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorSide) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;

        int.TryParse(Request.QueryString["cid"], out Cid);
        if (Cid <= 0) { Response.Redirect("Vendor.aspx"); return; }
        try { CompanyInfo co = Repo.GetCompany(Cid); if (co != null) FirmName = co.Name; } catch { }

        List<Ticket> ts = Repo.GetTicketsForCompany(Cid);
        var byUser = new Dictionary<int, UStat>();
        var statusCount = new Dictionary<string, int>();
        List<Ticket> resolvedList = new List<Ticket>();

        for (int i = 0; i < ts.Count; i++)
        {
            Ticket t = ts[i];
            TotalT++;
            string s = t.Status ?? "New";
            if (!statusCount.ContainsKey(s)) statusCount[s] = 0;
            statusCount[s]++;
            bool done = (s == "Resolved" || s == "Closed");
            if (done) { ResolvedT++; resolvedList.Add(t); } else OpenT++;
            int mins = t.ResolutionMinutes.HasValue ? t.ResolutionMinutes.Value : 0;
            TotalMin += mins;

            int uid = t.OpenedByUserId;
            if (!byUser.ContainsKey(uid)) byUser[uid] = new UStat { Name = string.IsNullOrEmpty(t.OpenedByName) ? "—" : t.OpenedByName };
            byUser[uid].Count++;
            byUser[uid].Minutes += mins;
        }

        SupportPlan sp = Repo.GetSupportPlan(Cid);
        BalanceStr = sp.Minutes + " dk" + (sp.ProjectHours > 0 ? " · " + sp.ProjectHours.ToString("0.##", TR) + " sa proje" : "") +
            (sp.Until.HasValue ? " · " + sp.Until.Value.ToString("dd.MM.yyyy", TR) : "");

        StatusBars = BuildStatusBars(statusCount);
        TopUsers = BuildTopUsers(byUser);
        UserTable = BuildUserTable(byUser);
        RecentRows = BuildRecent(resolvedList);
    }

    private string BuildStatusBars(Dictionary<string, int> sc)
    {
        string[,] defs = { { "New", "Yeni", "#168CFF" }, { "InProgress", "İnceleniyor", "#4F46E5" }, { "WaitingCustomer", "Müşteride", "#C08A1E" }, { "Resolved", "Çözüldü", "#12A150" }, { "Closed", "Kapandı", "#6B7A99" } };
        int max = 1;
        for (int i = 0; i < defs.GetLength(0); i++) { int v = sc.ContainsKey(defs[i, 0]) ? sc[defs[i, 0]] : 0; if (v > max) max = v; }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < defs.GetLength(0); i++)
        {
            int v = sc.ContainsKey(defs[i, 0]) ? sc[defs[i, 0]] : 0;
            int pct = (int)(v * 100.0 / max);
            sb.Append("<div class=\"fa-bar\"><div class=\"fa-bar-l\">" + defs[i, 1] + "</div>" +
                "<div class=\"fa-bar-t\"><div class=\"fa-bar-f\" style=\"width:" + pct + "%;background:" + defs[i, 2] + "\"></div></div>" +
                "<div class=\"fa-bar-v\">" + v + "</div></div>");
        }
        return sb.ToString();
    }

    private string BuildTopUsers(Dictionary<int, UStat> bu)
    {
        UStat topC = null, topM = null;
        foreach (UStat u in bu.Values)
        {
            if (topC == null || u.Count > topC.Count) topC = u;
            if (topM == null || u.Minutes > topM.Minutes) topM = u;
        }
        string c1 = topC != null ? Fmt.Enc(topC.Name) + " <b>(" + topC.Count + " talep)</b>" : "—";
        string c2 = topM != null ? Fmt.Enc(topM.Name) + " <b>(" + topM.Minutes + " dk)</b>" : "—";
        return "<div class=\"fa-top\"><div class=\"fa-top-c\"><div class=\"fa-top-k\">En çok talep açan</div><div class=\"fa-top-v\">" + c1 + "</div></div>" +
               "<div class=\"fa-top-c\"><div class=\"fa-top-k\">En çok süre harcanan</div><div class=\"fa-top-v\">" + c2 + "</div></div></div>";
    }

    private string BuildUserTable(Dictionary<int, UStat> bu)
    {
        var list = new List<UStat>(bu.Values);
        list.Sort(delegate (UStat a, UStat b) { return b.Minutes.CompareTo(a.Minutes); });
        if (list.Count == 0) return "<tr><td colspan=\"3\" style=\"color:var(--muted);padding:12px\">Kayıt yok.</td></tr>";
        StringBuilder sb = new StringBuilder();
        string td = " style=\"padding:9px 12px;border-top:1px solid var(--stroke-2);font-size:13px\"";
        for (int i = 0; i < list.Count; i++)
            sb.Append("<tr><td" + td + ">" + Fmt.Enc(list[i].Name) + "</td><td" + td + ">" + list[i].Count + "</td><td" + td + " style=\"padding:9px 12px;border-top:1px solid var(--stroke-2);font-weight:600\">" + list[i].Minutes + " dk</td></tr>");
        return sb.ToString();
    }

    private string BuildRecent(List<Ticket> resolved)
    {
        resolved.Sort(delegate (Ticket a, Ticket b) { DateTime da = a.ResolvedAt ?? a.UpdatedAt, db = b.ResolvedAt ?? b.UpdatedAt; return db.CompareTo(da); });
        if (resolved.Count == 0) return "<tr><td colspan=\"5\" style=\"color:var(--muted);padding:12px\">Çözülmüş talep yok.</td></tr>";
        StringBuilder sb = new StringBuilder();
        string td = " style=\"padding:9px 12px;border-top:1px solid var(--stroke-2);font-size:13px\"";
        int n = Math.Min(resolved.Count, 25);
        for (int i = 0; i < n; i++)
        {
            Ticket t = resolved[i];
            DateTime d = (t.ResolvedAt ?? t.UpdatedAt).ToTrLocal();
            sb.Append("<tr><td" + td + "><b>" + Fmt.Enc(t.TicketNo) + "</b></td>" +
                "<td" + td + ">" + Fmt.Enc(t.Subject) + "</td>" +
                "<td" + td + ">" + d.ToString("dd.MM.yyyy", TR) + "</td>" +
                "<td" + td + " style=\"padding:9px 12px;border-top:1px solid var(--stroke-2);font-weight:600\">" + (t.ResolutionMinutes.HasValue ? t.ResolutionMinutes.Value + " dk" : "—") + "</td>" +
                "<td" + td + "><a class=\"btn-ghost\" style=\"padding:5px 12px\" href=\"TicketPrint.aspx?id=" + t.TicketId + "\" target=\"_blank\">PDF</a></td></tr>");
        }
        return sb.ToString();
    }
}
