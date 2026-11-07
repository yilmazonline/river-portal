using System;
using System.Globalization;
using System.Web.UI;

// Proje/Hizmet kaydi detayi (yalniz uretici tarafi).
public partial class ProjectRecordDetailPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public ProjectRecordRow R = null;
    public string SureStr = "";
    public string PoolStr = "";

    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorSide) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;

        int id;
        if (!int.TryParse(Request.QueryString["id"], out id)) { Response.Redirect("ProjectRecords.aspx"); return; }
        R = Repo.GetProjectRecord(id);
        if (R == null) { Response.Redirect("ProjectRecords.aspx"); return; }

        SureStr = R.Pool == "project"
            ? (R.Hours.HasValue ? R.Hours.Value.ToString("0.##", TR) + " saat" : "-")
            : (R.Minutes.HasValue ? R.Minutes.Value + " dakika" : "-");
        PoolStr = R.Pool == "project" ? "Proje süresinden düşüldü" : "Destek süresinden düşüldü";
    }

    public string DtStr { get { return R.CreatedAt.ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR); } }
}
