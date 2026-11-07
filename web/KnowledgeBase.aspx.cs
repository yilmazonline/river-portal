using System;
using System.Data.SqlClient;
using System.Web.UI;

// Bilgi tabani yonetimi: pano (on yanit sonuclari, yanit bulunamayan sorular), kayitlar, ice aktarma. Destek ekibi ve super admin. Veriler KbAction.ashx'ten.
public partial class KnowledgeBasePage : Page
{
    public string Culture = "tr", UserName = "", SetupMsg = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!(Auth.IsSupportAgent || Auth.IsSuperAdmin)) { Response.Redirect(Auth.HomeUrl); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;
        try { using (SqlConnection c = Db.Open()) using (SqlCommand x = new SqlCommand("SELECT CASE WHEN OBJECT_ID('dbo.KbEntries') IS NOT NULL AND OBJECT_ID('dbo.KbAnswers') IS NOT NULL THEN 1 ELSE 0 END", c)) if ((int)x.ExecuteScalar() != 1) SetupMsg = "Bilgi tabanı tabloları bulunamadı. database\\66_knowledge_base.sql dosyasını çalıştırın."; }
        catch { SetupMsg = "Veritabanına bağlanılamadı."; }
    }
}
