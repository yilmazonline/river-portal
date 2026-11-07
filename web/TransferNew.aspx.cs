using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// Yeni dosya paylasimi (yalniz uretici personeli). Yukleme + gonderim istemci tarafinda TransferApi.ashx ile yapilir.
public partial class TransferNewPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string FirmsJson = "[]";
    public long MaxBytes = 0;
    public string MaxLabel = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorFirm) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;
        MaxBytes = TransferRepo.MaxBytes; MaxLabel = TransferRepo.FormatSize(MaxBytes);

        StringBuilder sb = new StringBuilder("[");
        bool first = true;
        string[] types = { "Partner", "Customer" };
        for (int k = 0; k < types.Length; k++)
        {
            List<CompanyLite> l = Repo.GetCompaniesByType(types[k]);
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Name == "Harici Ürün Destekleri") continue;
                if (!first) sb.Append(","); first = false;
                sb.Append("{\"id\":" + l[i].CompanyId + ",\"n\":\"" + J(l[i].Name) + "\",\"g\":\"" + (k == 0 ? "Bayi" : "Müşteri") + "\"}");
            }
        }
        FirmsJson = sb.Append("]").ToString();
    }

    private static string J(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("<", "\\u003c").Replace(">", "\\u003e");
    }
}
