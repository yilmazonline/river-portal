<%@ WebHandler Language="C#" Class="ReceivableActionHandler" %>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Alacak islemleri (yalniz Finans yetkisi). POST, JSON yanit.
public class ReceivableActionHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    private static readonly CultureInfo TR = new CultureInfo("tr-TR");

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        JavaScriptSerializer js = new JavaScriptSerializer();
        try
        {
            if (!Auth.IsLoggedIn || !Auth.IsVendorFirm || !Auth.CanFinance) { Fail(ctx, js, "Bu işlem için Finans yetkisi gerekiyor.", 403); return; }
            if (!string.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { Fail(ctx, js, "Geçersiz istek.", 405); return; }
            Uri refr = ctx.Request.UrlReferrer;
            if (refr == null || !string.Equals(refr.Host, ctx.Request.Url.Host, StringComparison.OrdinalIgnoreCase)) { Fail(ctx, js, "İstek kaynağı doğrulanamadı.", 403); return; }

            HttpRequest q = ctx.Request;
            string act = q.Form["action"] ?? "";
            int rid; int.TryParse(q.Form["id"], out rid);

            // Manuel alacak kaydi (var olan bir alacak gerektirmez)
            if (act == "create")
            {
                decimal net, vat; DateTime inv, due; int cid; int.TryParse(q.Form["companyId"], out cid);
                string product = Txt(q.Form["product"], 1000), customer = Txt(q.Form["customer"], 300), cur = (q.Form["currency"] ?? "TL").Trim().ToUpperInvariant();
                string channel = Txt(q.Form["channel"], 30); if (channel != null && channel != ReceivableRepo.CH_CHANNEL && channel != ReceivableRepo.CH_DIRECT) channel = null;
                if (!Amount(q.Form["net"], out net) || net <= 0) { Fail(ctx, js, "Geçerli bir tutar girin (KDV hariç).", 400); return; }
                if (!Amount(q.Form["vat"], out vat) || (vat != 0 && vat != 1 && vat != 10 && vat != 20)) { Fail(ctx, js, "KDV oranı %0, %1, %10 ya da %20 olmalı.", 400); return; }
                if (cur != "TL" && cur != "USD" && cur != "EUR") { Fail(ctx, js, "Döviz TL, USD ya da EUR olmalı.", 400); return; }
                if (product == null) { Fail(ctx, js, "Açıklama / ürün bilgisini girin.", 400); return; }
                if (!Date(q.Form["invoiceDate"], out inv)) { Fail(ctx, js, "Fatura tarihini girin.", 400); return; }
                if (!Date(q.Form["due"], out due)) { Fail(ctx, js, "Vade tarihini girin.", 400); return; }
                if (due.Date < inv.Date) { Fail(ctx, js, "Vade tarihi fatura tarihinden önce olamaz.", 400); return; }
                string cname = null;
                if (cid > 0) { CompanyInfo c = Repo.GetCompany(cid); if (c == null) { Fail(ctx, js, "Seçilen cari bulunamadı.", 400); return; } cname = c.Name; }
                if (customer == null && cname == null) { Fail(ctx, js, "Cari seçin ya da müşteri adını yazın.", 400); return; }
                int newId = ReceivableRepo.CreateManual(cid, cname, customer ?? cname, Txt(q.Form["partner"], 300), channel, product, Txt(q.Form["rep"], 150), Txt(q.Form["note"], 2000), inv, due, net, vat, cur, Auth.UserId);
                ctx.Response.Write(js.Serialize(new { ok = true, id = newId }));
                return;
            }

            Receivable x = rid > 0 ? ReceivableRepo.Get(rid) : null;
            if (x == null) { Fail(ctx, js, "Alacak bulunamadı.", 404); return; }
            int uid = Auth.UserId;

            if (act == "assign")
            {
                int cid; int.TryParse(q.Form["companyId"], out cid);
                string name = null;
                foreach (string t in new[] { "Partner", "Customer" })
                    foreach (CompanyLite c in Repo.GetCompaniesByType(t)) if (c.CompanyId == cid) name = c.Name;
                if (name == null) { Fail(ctx, js, "Geçerli bir bayi ya da müşteri seçin.", 400); return; }
                if (x.Status == "Paid" || x.Status == "Closed") { Fail(ctx, js, "Tahsil edilmiş ya da kapatılmış alacağın carisi değiştirilemez.", 400); return; }
                ReceivableRepo.Assign(rid, cid, name, uid);
            }
            else if (act == "pay")
            {
                if (!x.IsOpen) { Fail(ctx, js, "Bu alacak açık değil.", 400); return; }
                decimal amt; DateTime dt;
                if (!Amount(q.Form["amount"], out amt) || amt <= 0) { Fail(ctx, js, "Geçerli bir tutar girin.", 400); return; }
                if (amt > x.Remaining + 0.01m) { Fail(ctx, js, "Tutar kalan alacaktan (" + ReceivableRepo.Money(x.Remaining, x.Currency) + ") büyük olamaz.", 400); return; }
                if (!Date(q.Form["date"], out dt) || dt.Date > DateTime.Today) { Fail(ctx, js, "Tahsilat tarihi bugün ya da daha önce olmalı.", 400); return; }
                ReceivableRepo.AddPayment(rid, dt, amt, Txt(q.Form["method"], 30), Txt(q.Form["bank"], 100), Txt(q.Form["note"], 500), uid, x.Currency);
            }
            else if (act == "split")
            {
                if (!x.IsOpen) { Fail(ctx, js, "Bu alacak açık değil.", 400); return; }
                int n; DateTime first; int.TryParse(q.Form["count"], out n);
                if (n < 1 || n > 36) { Fail(ctx, js, "Vade sayısı 1 ile 36 arasında olmalı.", 400); return; }
                if (!Date(q.Form["first"], out first)) { Fail(ctx, js, "İlk vade tarihini girin.", 400); return; }
                string iv = q.Form["interval"] == "week" ? "week" : "month";
                List<decimal> amounts = null;
                string raw = (q.Form["amounts"] ?? "").Trim();
                if (raw.Length > 0)
                {
                    amounts = new List<decimal>();
                    foreach (string p in raw.Split(new[] { ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)) { decimal a; if (!Amount(p, out a)) { Fail(ctx, js, "Taksit tutarlarından biri okunamadı: " + p, 400); return; } amounts.Add(a); }
                    if (amounts.Count != n) { Fail(ctx, js, n + " vade için " + n + " tutar girilmeli (" + amounts.Count + " girildi).", 400); return; }
                }
                try { ReceivableRepo.Split(rid, n, first, iv, amounts, uid, x.Currency); }
                catch (InvalidOperationException ex) { Fail(ctx, js, ex.Message, 400); return; }
            }
            else if (act == "redate")
            {
                int iid; DateTime dt; int.TryParse(q.Form["inst"], out iid);
                string reason = Txt(q.Form["reason"], 400);
                if (!Date(q.Form["date"], out dt)) { Fail(ctx, js, "Yeni tarihi girin.", 400); return; }
                if (reason == null || reason.Length < 3) { Fail(ctx, js, "Tarih değişikliğinin gerekçesini yazın.", 400); return; }
                try { ReceivableRepo.Redate(rid, iid, dt, reason, uid); }
                catch (InvalidOperationException ex) { Fail(ctx, js, ex.Message, 400); return; }
            }
            else if (act == "close")
            {
                string reason = Txt(q.Form["reason"], 500);
                if (reason == null || reason.Length < 3) { Fail(ctx, js, "Kapatma gerekçesini yazın.", 400); return; }
                ReceivableRepo.Close(rid, reason, uid);
            }
            else if (act == "reopen") { ReceivableRepo.Reopen(rid, uid); }
            else if (act == "channel")
            {
                string ch = (q.Form["channel"] ?? "").Trim();
                if (ch.Length == 0) ch = null;
                else if (ch != ReceivableRepo.CH_CHANNEL && ch != ReceivableRepo.CH_DIRECT) { Fail(ctx, js, "Geçersiz sorumlu değeri.", 400); return; }
                ReceivableRepo.SetChannel(rid, ch, uid);
            }
            else if (act == "note")
            {
                string note = Txt(q.Form["note"], 2000);
                if (note == null || note.Length < 2) { Fail(ctx, js, "Not boş olamaz.", 400); return; }
                ReceivableRepo.AddNote(rid, note, uid);
            }
            else if (act == "remind")
            {
                if (!x.IsOpen) { Fail(ctx, js, "Kapanmış alacak için hatırlatma gönderilemez.", 400); return; }
                string subject = Txt(q.Form["subject"], 300), body = Txt(q.Form["body"], 8000);
                if (subject == null || body == null) { Fail(ctx, js, "Konu ve mesaj boş olamaz.", 400); return; }
                List<string> to = new List<string>(); HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string all = (q.Form["to"] ?? "") + "," + (q.Form["manual"] ?? "");
                foreach (string p in all.Split(new[] { ',', ';', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string em = p.Trim();
                    if (!Regex.IsMatch(em, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { Fail(ctx, js, "Geçersiz e-posta adresi: " + em, 400); return; }
                    if (seen.Add(em)) to.Add(em);
                }
                if (to.Count == 0) { Fail(ctx, js, "En az bir alıcı seçin ya da yazın.", 400); return; }
                if (to.Count > 20) { Fail(ctx, js, "En fazla 20 alıcıya gönderilebilir.", 400); return; }
                StringBuilder html = new StringBuilder();
                foreach (string para in body.Replace("\r", "").Split(new[] { "\n\n" }, StringSplitOptions.None))
                    html.Append("<p style=\"margin:0 0 14px;line-height:1.6\">" + HttpUtility.HtmlEncode(para).Replace("\n", "<br/>") + "</p>");
                string mail = Emails.Shell(subject, html.ToString());
                int iid; int.TryParse(q.Form["inst"], out iid);
                int days = 0;
                foreach (RcvInstallment i in ReceivableRepo.GetInstallments(rid)) if (i.InstallmentId == iid) days = (int)(i.DueDate.Date - DateTime.Today).TotalDays;
                List<string> failed = new List<string>(); string lastErr = null;
                foreach (string em in to) { string err; if (!Mailer.SendTo(em, subject, mail, out err)) { failed.Add(em); lastErr = err; } }
                bool ok = failed.Count == 0;
                ReceivableRepo.LogReminder(rid, ReceivableRepo.Scenario(days), days, string.Join(", ", to.ToArray()), subject, ok, ok ? null : (string.Join(", ", failed.ToArray()) + ": " + lastErr), uid);
                if (!ok) { Fail(ctx, js, "Gönderilemeyen adresler: " + string.Join(", ", failed.ToArray()) + (lastErr != null ? " (" + lastErr + ")" : ""), 502); return; }
                ctx.Response.Write(js.Serialize(new { ok = true, msg = to.Count + " alıcıya hatırlatma gönderildi." })); return;
            }
            else { Fail(ctx, js, "Bilinmeyen işlem.", 400); return; }
            ctx.Response.Write(js.Serialize(new { ok = true }));
        }
        catch (Exception ex) { Fail(ctx, js, "İşlem tamamlanamadı: " + ex.Message, 500); }
    }

    private static void Fail(HttpContext ctx, JavaScriptSerializer js, string msg, int code)
    {
        ctx.Response.StatusCode = code; ctx.Response.TrySkipIisCustomErrors = true;
        ctx.Response.Write(js.Serialize(new { ok = false, error = msg }));
    }
    private static string Txt(string s, int max) { if (s == null) return null; s = s.Trim(); if (s.Length == 0) return null; return s.Length > max ? s.Substring(0, max) : s; }
    /// <summary>"1.099.008,00" / "1099008,5" / "1099008.50" kabul edilir</summary>
    private static bool Amount(string s, out decimal v)
    {
        v = 0; if (string.IsNullOrEmpty(s)) return false;
        s = s.Trim().Replace(" ", "").Replace("TL", "").Replace("₺", "");
        if (s.IndexOf(',') >= 0) s = s.Replace(".", "").Replace(",", ".");
        else if (Regex.IsMatch(s, @"^\d{1,3}(\.\d{3})+$")) s = s.Replace(".", "");
        return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out v);
    }
    private static bool Date(string s, out DateTime d) { return DateTime.TryParseExact((s ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d); }
}
