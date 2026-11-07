using System;
using System.IO;
using System.Web.UI;

// Vendor: satış materyalini kaldırır (kayıt + varsa disk dosyası).
public partial class DeleteSalesMaterialPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Auth.IsLoggedIn) { Response.Redirect("Login.aspx"); return; }
        if (!Auth.IsVendor) { Response.Redirect("Materials.aspx"); return; }
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        { Response.Redirect("Materials.aspx"); return; }

        int id;
        if (int.TryParse(Request.Form["id"], out id))
        {
            string stored = Repo.DeleteSalesMaterial(id);
            if (!string.IsNullOrEmpty(stored))
            {
                try
                {
                    string path = Path.Combine(Server.MapPath("~/materials"), stored);
                    if (File.Exists(path)) File.Delete(path);
                }
                catch { }
            }
        }
        Response.Redirect("Materials.aspx?ok=del");
    }
}
