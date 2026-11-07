using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;

// Paylasimlarim: uretici personeli kendi paylasimlarini, portal yoneticisi tumunu gorur.
public partial class TransfersPage : Page
{
    public string Culture = "tr";
    public string UserName = "";
    public string RowsHtml = "";
    public bool ShowAll = false, CanSeeAll = false;
    public int Count = 0;
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    protected void Page_Load(object sender, EventArgs e)
    {
        Auth.Require(this);
        if (Response.IsRequestBeingRedirected) return;
        if (!Auth.IsVendorFirm) { Response.Redirect("Default.aspx"); return; }
        Culture = Auth.Culture; UserName = Auth.FullName;
        CanSeeAll = Auth.IsVendor;
        ShowAll = CanSeeAll && Request.QueryString["all"] == "1";

        List<TransferInfo> list;
        try { list = TransferRepo.ListTransfers(ShowAll ? 0 : Auth.UserId); }
        catch { RowsHtml = "<tr><td colspan=\"7\" class=\"tf-empty\">Dosya paylaşımı altyapısı henüz kurulmadı (database\\33_file_transfer.sql çalıştırılmalı).</td></tr>"; return; }
        Count = list.Count;
        if (list.Count == 0) { RowsHtml = "<tr><td colspan=\"7\" class=\"tf-empty\">Henüz paylaşım yok. “+ Yeni Paylaşım” ile başlayın.</td></tr>"; return; }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            TransferInfo t = list[i];
            string st = t.Status == "Cancelled" ? "<span class=\"st cancelled\">İptal</span>" : t.IsExpired ? "<span class=\"st passive\">Süresi doldu</span>" : "<span class=\"st approved\">Aktif</span>";
            sb.Append("<tr class=\"tf-row\" onclick=\"location.href='TransferDetail.aspx?id=" + t.TransferId + "'\">" +
                "<td><b>" + Fmt.Enc(t.Title) + "</b>" + (t.HasPassword ? " <span title=\"Şifreli\">&#128274;</span>" : "") + "</td>" +
                (ShowAll ? "<td>" + Fmt.Enc(t.CreatedByName) + "</td>" : "") +
                "<td>" + (t.SentAt ?? t.CreatedAt).ToTrLocal().ToString("dd.MM.yyyy HH:mm", TR) + "</td>" +
                "<td>" + t.FileCount + " dosya · " + TransferRepo.FormatSize(t.TotalBytes) + "</td>" +
                "<td><b>" + t.DownloadedCount + "</b> / " + t.RecipientCount + " indirdi</td>" +
                "<td>" + t.ExpiresAt.ToTrLocal().ToString("dd.MM.yyyy", TR) + "</td><td>" + st + "</td></tr>");
        }
        RowsHtml = sb.ToString();
    }
}
