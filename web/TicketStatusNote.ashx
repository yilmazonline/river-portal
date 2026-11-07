<%@ WebHandler Language="C#" Class="TicketStatusNoteHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Destek talebi gorunen durumu: yalniz Fol personeli (destek ya da portal yetkisi). POST, ayni site.
// Sistem durumu degismez; talebe sistem satiri eklenir, musteriye uygulama ici bildirim gider.
public class TicketStatusNoteHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!Auth.IsLoggedIn || !Auth.IsVendorFirm || !(Auth.IsSupportAgent || Auth.IsVendor)) { Fail(ctx, js, "Bu işlem yalnız destek ekibine açık.", 403); return; }
        Uri refr = ctx.Request.UrlReferrer;
        if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, ctx.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
        { Fail(ctx, js, "İstek kaynağı doğrulanamadı.", 403); return; }
        if (!Repo.HasStatusNoteCol()) { Fail(ctx, js, "Önce database\\44_ticket_status_note.sql dosyasını çalıştırın.", 500); return; }
        int id; int.TryParse(ctx.Request.Form["id"], out id);
        Ticket t = id > 0 ? Repo.GetTicket(id) : null;
        if (t == null) { Fail(ctx, js, "Talep bulunamadı.", 404); return; }
        if (t.Status != "New" && t.Status != "InProgress") { Fail(ctx, js, "Görünen durum yalnız yeni ve incelenen taleplerde değiştirilebilir.", 400); return; }
        string note = (ctx.Request.Unvalidated.Form["note"] ?? "").Trim();
        note = System.Text.RegularExpressions.Regex.Replace(note, @"\s+", " ");
        if (note.Length > 40) { Fail(ctx, js, "Durum en fazla 40 karakter olabilir.", 400); return; }
        string before = TicketFmt.Display(t, false);
        if (note == (t.StatusNote ?? "")) { ctx.Response.Write(js.Serialize(new { ok = true, label = TicketFmt.Display(t, true), note = note })); return; }
        try
        {
            Repo.SetTicketStatusNote(id, note, Auth.UserId);
            t.StatusNote = note.Length > 0 ? note : null;
            string after = TicketFmt.Display(t, false);
            Repo.AddTicketMessage(id, Auth.UserId, "System", "Durum güncellendi: " + after + (note.Length == 0 ? " (varsayılan durum)" : ""), false, "Status");
            Notify(t, after);
            ctx.Response.Write(js.Serialize(new { ok = true, label = TicketFmt.Display(t, true), note = note }));
        }
        catch (Exception ex) { Fail(ctx, js, "Durum güncellenemedi: " + ex.Message, 500); }
    }

    /// <summary>Musteriye uygulama ici bildirim: talebi acan musteri kullanicisi; personel acmissa firmanin tum kullanicilari</summary>
    private static void Notify(Ticket t, string label)
    {
        try
        {
            string title = t.TicketNo + ": talebinizin durumu güncellendi", titleEn = t.TicketNo + ": your request status was updated";
            string body = "Yeni durum: " + label + " · " + t.Subject, bodyEn = "New status: " + label + " · " + t.Subject;
            string link = "TicketDetail.aspx?id=" + t.TicketId;
            UserRow opener = Repo.GetUserById(t.OpenedByUserId);
            if (opener != null && opener.CompanyId == t.CompanyId)
                Repo.AddUserNotification(t.CompanyId, t.OpenedByUserId, title, titleEn, body, bodyEn, "Ticket", link);
            else
                Repo.AddNotification(t.CompanyId, title, titleEn, body, bodyEn, "Ticket", link);   // firma geneli
        }
        catch { }   // bildirim hatasi durum guncellemesini etkilemez
    }
    private static void Fail(HttpContext ctx, JavaScriptSerializer js, string msg, int code) { ctx.Response.StatusCode = code; ctx.Response.Write(js.Serialize(new { ok = false, error = msg })); }
}
