<%@ WebHandler Language="C#" Class="LeaveActionHandler" %>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.SessionState;

// Izin talepleri ucu. Yalniz vendor personeli. state (GET), file (GET, yetkili kisiler); digerleri POST + ayni site.
// Yonetim (Kurallar: profil, onaylayicilar, genel kurallar, tatil, takvim): YALNIZ super admin (butun modul yetkileri).
// IK sorumlusu kurallari degistiremez; akistaki gorevi (bilgi, onaylayicisi olmayanlarin onayi, belge erisimi) surer.
public class LeaveActionHandler : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable { get { return false; } }
    private static readonly JavaScriptSerializer JS = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 };
    private static readonly CultureInfo IC = CultureInfo.InvariantCulture;

    public void ProcessRequest(HttpContext ctx)
    {
        HttpRequest q = ctx.Request; ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        string op = q["op"] ?? "state";
        if (!Auth.IsLoggedIn) { Deny(ctx, 401); return; }
        if (!Leaves.Ready()) { Out(ctx, "İzin tabloları bulunamadı: database\\61_leave_requests.sql dosyasını çalıştırın.", null); return; }
        int me = Auth.UserId;
        List<LvStaff> staff = Leaves.Staff(); Dictionary<int, LvStaff> map = Leaves.StaffMap(staff);
        if (!map.ContainsKey(me)) { Deny(ctx, 403); return; }
        LvSettings st = Leaves.Settings();
        bool admin = Auth.IsSuperAdmin;
        string baseUrl = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (baseUrl.Length == 0) baseUrl = q.Url.GetLeftPart(UriPartial.Authority) + (q.ApplicationPath ?? "").TrimEnd('/');
        int id; int.TryParse(q["id"], out id);

        if (op == "file") { File(ctx, id, me, admin, st); return; }
        if (op != "state")
        {
            Uri refr = q.UrlReferrer;
            if (!string.Equals(q.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || refr == null || !string.Equals(refr.Host, q.Url.Host, StringComparison.OrdinalIgnoreCase)) { Out(ctx, "İstek kaynağı doğrulanamadı.", null); return; }
        }
        try
        {
            string err = null, msg = null; object extra = null;
            if (op == "state") { Leaves.Tick(baseUrl, false); }
            else if (op == "submit")
            {
                LvReq f = Read(q, me); int editId; int.TryParse(q.Form["editId"], out editId);
                string ferr = SaveFile(ctx, q, f); if (ferr != null) { Out(ctx, ferr, null); return; }
                int nid; err = Leaves.Submit(f, editId, baseUrl, out nid);
                if (err != null && f.FileStored != null) { try { System.IO.File.Delete(Path.Combine(Dir(ctx), f.FileStored)); } catch { } }
                if (err == null) { LvReq r = Leaves.Get(nid); extra = new { id = nid }; msg = Message(r, map); }
            }
            else if (op == "decide") err = Leaves.Decide(id, me, q.Form["act"] ?? "", q.Unvalidated.Form["note"] ?? "", baseUrl, out msg);
            else if (op == "cancel") { err = Leaves.Cancel(id, me, baseUrl); if (err == null) msg = "Talep geri çekildi."; }
            else if (admin && op == "profile") err = SaveProfile(q, map);
            else if (admin && op == "settings") err = SaveSettings(q, map);
            else if (admin && op == "holiday") err = Holiday(q);
            else if (admin && op == "event") err = EventOp(q);
            else if (op != "state") err = "Bu işlem için yetkiniz yok.";
            if (err != null) { Out(ctx, err, null); return; }
            Dictionary<string, object> data = State(me, admin, Leaves.Staff(), Leaves.Settings());
            if (msg != null) data["message"] = msg; if (extra != null) data["result"] = extra;
            Out(ctx, null, data);
        }
        catch (Exception ex) { Out(ctx, "İşlem tamamlanamadı: " + ex.Message, null); }
    }

    static string Message(LvReq r, Dictionary<int, LvStaff> map)
    {
        if (r.Status == "recorded") return "Rapor bildiriminiz kaydedildi. Yöneticiniz ve İK bilgilendirildi.";
        if (r.Status == "approved") return "Talep kaydedildi ve onaylandı.";
        LvStep s = r.Current; return "Talep " + (s != null && map.ContainsKey(s.Approver) ? map[s.Approver].Name : "onaylayıcı") + " onayına gitti.";
    }

    // ---------- okuma ----------
    static LvReq Read(HttpRequest q, int me)
    {
        LvReq f = new LvReq(); f.UserId = me; f.Type = q.Form["type"] ?? ""; f.Sub = q.Form["sub"];
        f.Start = D(q.Form["start"]); f.End = D(q.Form["end"]); if (f.End == DateTime.MinValue) f.End = f.Start;
        f.StartHalf = q.Form["startHalf"] == "1"; f.EndHalf = q.Form["endHalf"] == "1"; f.HourFrom = q.Form["hFrom"]; f.HourTo = q.Form["hTo"];
        f.Delegate = I(q.Form["vekil"]); f.OnCallTo = I(q.Form["oncall"]); f.Handover = q.Unvalidated.Form["hand"]; f.Reach = q.Form["reach"] == "urgent" || q.Form["reach"] == "mail" ? q.Form["reach"] : "none";
        f.Phone = q.Unvalidated.Form["phone"]; f.Address = q.Unvalidated.Form["addr"]; f.Note = q.Unvalidated.Form["note"]; f.Ooo = q.Form["ooo"] == "1";
        return f;
    }
    static DateTime D(string s) { DateTime d; return DateTime.TryParseExact(s ?? "", "yyyy-MM-dd", IC, DateTimeStyles.None, out d) ? d : DateTime.MinValue; }
    static int? I(string s) { int n; return int.TryParse(s, out n) && n > 0 ? n : (int?)null; }
    static string Dir(HttpContext ctx) { string d = ctx.Server.MapPath("~/App_Data/leavefiles"); if (!Directory.Exists(d)) Directory.CreateDirectory(d); return d; }

    /// <summary>Belge: PDF / JPG / PNG, en fazla 10 MB, icerik imzasiyla dogrulanir; App_Data altinda (web'den dogrudan erisilemez)</summary>
    static string SaveFile(HttpContext ctx, HttpRequest q, LvReq f)
    {
        HttpPostedFile up = q.Files["file"]; if (up == null || up.ContentLength == 0) return null;
        if (up.ContentLength > 10 * 1024 * 1024) return "Belge en fazla 10 MB olabilir.";
        byte[] h = new byte[8]; int n = up.InputStream.Read(h, 0, 8); up.InputStream.Position = 0;
        string ext = n >= 4 && h[0] == 0x25 && h[1] == 0x50 && h[2] == 0x44 && h[3] == 0x46 ? ".pdf" : n >= 4 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47 ? ".png" : n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF ? ".jpg" : null;
        if (ext == null) return "Belge PDF, JPG ya da PNG olmalı.";
        string name = Guid.NewGuid().ToString("N") + ext; up.SaveAs(Path.Combine(Dir(ctx), name));
        f.FileStored = name; string orig = Path.GetFileName(up.FileName ?? "belge" + ext); f.FileOriginal = orig.Length > 200 ? orig.Substring(orig.Length - 200) : orig;
        return null;
    }
    static void File(HttpContext ctx, int id, int me, bool admin, LvSettings st)
    {
        LvReq r = Leaves.Get(id);
        bool ok = r != null && r.FileStored != null && (r.UserId == me || admin || (st.Hr.HasValue && st.Hr.Value == me) || r.EscalatedTo == me || r.Steps.Exists(delegate (LvStep s) { return s.Approver == me; }));
        if (r != null && r.Type == "hastalik" && !ok) { LvStaff p; Dictionary<int, LvStaff> m = Leaves.StaffMap(Leaves.Staff()); ok = r.FileStored != null && m.TryGetValue(r.UserId, out p) && p.Approver == me; }
        if (!ok) { Deny(ctx, 404); return; }
        string path = Path.Combine(Dir(ctx), Path.GetFileName(r.FileStored));
        if (!System.IO.File.Exists(path)) { Deny(ctx, 404); return; }
        string ext = Path.GetExtension(path).ToLowerInvariant();
        ctx.Response.ContentType = ext == ".pdf" ? "application/pdf" : ext == ".png" ? "image/png" : "image/jpeg";
        ctx.Response.AddHeader("Content-Disposition", "inline; filename=\"" + (r.FileOriginal ?? "belge" + ext).Replace("\"", "") + "\"");
        ctx.Response.AddHeader("X-Content-Type-Options", "nosniff");
        ctx.Response.TransmitFile(path);
    }

    // ---------- yonetim ----------
    static string SaveProfile(HttpRequest q, Dictionary<int, LvStaff> map)
    {
        int uid; if (!int.TryParse(q.Form["userId"], out uid) || !map.ContainsKey(uid)) return "Personel bulunamadı.";
        DateTime hire = D(q.Form["hire"]); decimal used, extra;
        if (!decimal.TryParse((q.Form["used"] ?? "0").Replace(',', '.'), NumberStyles.Number, IC, out used) || used < 0 || used > 60) return "Kullanılan gün 0 ile 60 arasında olmalı.";
        if (!decimal.TryParse((q.Form["extra"] ?? "0").Replace(',', '.'), NumberStyles.Number, IC, out extra) || extra < 0 || extra > 60) return "Devreden gün 0 ile 60 arasında olmalı.";
        int? ap = I(q.Form["approver"]), dl = I(q.Form["delegate"]);
        if (ap == uid || dl == uid) return "Kişi kendi onaylayıcısı olamaz.";
        if ((ap.HasValue && !map.ContainsKey(ap.Value)) || (dl.HasValue && !map.ContainsKey(dl.Value))) return "Onaylayıcı vendor personelinden seçilmeli.";
        if (hire != DateTime.MinValue && hire > Leaves.Today) return "İşe giriş tarihi gelecekte olamaz.";
        using (var c = Db.Open())
        using (var cmd = new System.Data.SqlClient.SqlCommand(
            "IF NOT EXISTS (SELECT 1 FROM dbo.LeaveProfiles WHERE UserId = @u) INSERT INTO dbo.LeaveProfiles (UserId) VALUES (@u); " +
            "UPDATE dbo.LeaveProfiles SET Title = @t, Department = @d, HireDate = @h, UsedDays = @us, ExtraDays = @ex, OnCall = @oc, ApproverUserId = @ap, NoApproval = @na, DelegateApproverUserId = @dl WHERE UserId = @u", c))
        {
            cmd.Parameters.AddWithValue("@u", uid); cmd.Parameters.AddWithValue("@t", Cut(q.Unvalidated.Form["title"], 120)); cmd.Parameters.AddWithValue("@d", Cut(q.Unvalidated.Form["dept"], 120));
            cmd.Parameters.AddWithValue("@h", hire == DateTime.MinValue ? (object)DBNull.Value : hire); cmd.Parameters.AddWithValue("@us", used); cmd.Parameters.AddWithValue("@ex", extra);
            cmd.Parameters.AddWithValue("@oc", q.Form["onCall"] == "1"); cmd.Parameters.AddWithValue("@ap", ap.HasValue ? (object)ap.Value : DBNull.Value); cmd.Parameters.AddWithValue("@na", q.Form["noApproval"] == "1");
            cmd.Parameters.AddWithValue("@dl", dl.HasValue ? (object)dl.Value : DBNull.Value); cmd.ExecuteNonQuery();
        }
        return null;
    }
    static object Cut(string s, int n) { s = (s ?? "").Trim(); if (s.Length == 0) return DBNull.Value; return s.Length > n ? s.Substring(0, n) : s; }
    static string SaveSettings(HttpRequest q, Dictionary<int, LvStaff> map)
    {
        int ld, rm, es;
        if (!int.TryParse(q.Form["longDays"], out ld) || ld < 1 || ld > 30) return "İkinci onay sınırı 1 ile 30 iş günü arasında olmalı.";
        if (!int.TryParse(q.Form["remind"], out rm) || rm < 1 || rm > 168) return "Hatırlatma süresi 1 ile 168 saat arasında olmalı.";
        if (!int.TryParse(q.Form["escalate"], out es) || es <= rm || es > 336) return "Yükseltme süresi hatırlatmadan uzun ve en fazla 336 saat olmalı.";
        int? sc = I(q.Form["second"]), hr = I(q.Form["hr"]);
        if ((sc.HasValue && !map.ContainsKey(sc.Value)) || (hr.HasValue && !map.ContainsKey(hr.Value))) return "Kişi vendor personelinden seçilmeli.";
        Leaves.SaveSetting("longDays", ld.ToString(IC)); Leaves.SaveSetting("remindHours", rm.ToString(IC)); Leaves.SaveSetting("escalateHours", es.ToString(IC));
        Leaves.SaveSetting("unpaidSecond", q.Form["unpaidSecond"] == "1" ? "1" : "0"); Leaves.SaveSetting("sickNoApproval", q.Form["sickNoApproval"] == "1" ? "1" : "0");
        Leaves.SaveSetting("secondApprover", sc.HasValue ? sc.Value.ToString(IC) : "0"); Leaves.SaveSetting("hrUser", hr.HasValue ? hr.Value.ToString(IC) : "0");
        return null;
    }
    static string Holiday(HttpRequest q)
    {
        DateTime d = D(q.Form["day"]); if (d == DateTime.MinValue) return "Tarih seçin.";
        using (var c = Db.Open())
        {
            if (q.Form["del"] == "1") { using (var cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM dbo.LeaveHolidays WHERE Day = @d", c)) { cmd.Parameters.AddWithValue("@d", d); cmd.ExecuteNonQuery(); } return null; }
            string n = (q.Unvalidated.Form["name"] ?? "").Trim(); if (n.Length == 0 || n.Length > 120) return "Tatil adını yazın (en fazla 120 karakter).";
            using (var cmd = new System.Data.SqlClient.SqlCommand("IF EXISTS (SELECT 1 FROM dbo.LeaveHolidays WHERE Day = @d) UPDATE dbo.LeaveHolidays SET Name = @n, IsHalf = @h WHERE Day = @d ELSE INSERT INTO dbo.LeaveHolidays (Day, Name, IsHalf) VALUES (@d, @n, @h)", c))
            { cmd.Parameters.AddWithValue("@d", d); cmd.Parameters.AddWithValue("@n", n); cmd.Parameters.AddWithValue("@h", q.Form["half"] == "1"); cmd.ExecuteNonQuery(); }
        }
        return null;
    }
    static string EventOp(HttpRequest q)
    {
        using (var c = Db.Open())
        {
            int eid; if (q.Form["del"] == "1" && int.TryParse(q.Form["eventId"], out eid)) { using (var cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM dbo.LeaveEvents WHERE EventId = @i", c)) { cmd.Parameters.AddWithValue("@i", eid); cmd.ExecuteNonQuery(); } return null; }
            DateTime d = D(q.Form["day"]); string t = (q.Unvalidated.Form["title"] ?? "").Trim();
            if (d == DateTime.MinValue) return "Tarih seçin."; if (t.Length == 0 || t.Length > 160) return "Açıklama yazın (en fazla 160 karakter).";
            using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO dbo.LeaveEvents (Day, Title) VALUES (@d, @t)", c)) { cmd.Parameters.AddWithValue("@d", d); cmd.Parameters.AddWithValue("@t", t); cmd.ExecuteNonQuery(); }
        }
        return null;
    }

    // ---------- durum ----------
    static string K(DateTime d) { return d.ToString("yyyy-MM-dd", IC); }
    static string T(DateTime? utc) { return utc.HasValue ? utc.Value.ToTrLocal().ToString("d MMMM ddd HH:mm", Leaves.TR) : ""; }
    static string N(Dictionary<int, LvStaff> m, int? i) { LvStaff s; return i.HasValue && m.TryGetValue(i.Value, out s) ? s.Name : ""; }
    static Dictionary<string, object> Req(LvReq r, Dictionary<int, LvStaff> m, Dictionary<DateTime, KeyValuePair<string, bool>> hol, bool detail)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["id"] = r.Id; d["no"] = r.No; d["who"] = r.UserId; d["whoName"] = N(m, r.UserId); d["type"] = r.Type; d["typeText"] = Leaves.TypeText(r); d["sub"] = r.Sub;
        d["start"] = K(r.Start); d["end"] = K(r.End); d["startHalf"] = r.StartHalf; d["endHalf"] = r.EndHalf; d["hFrom"] = r.HourFrom; d["hTo"] = r.HourTo;
        d["days"] = r.Days; d["dur"] = Leaves.DurText(r); d["range"] = Leaves.RangeText(r); d["ret"] = Leaves.ReturnText(r, hol);
        d["status"] = r.Status; d["step"] = r.Step; d["reason"] = r.Reason; d["escalatedTo"] = r.EscalatedTo; d["created"] = T(r.CreatedAt);
        List<object> steps = new List<object>();
        foreach (LvStep s in r.Steps) steps.Add(new { no = s.StepNo, label = s.Label, who = s.Approver, whoName = N(m, s.Approver), orig = s.Original, origName = N(m, s.Original), why = s.Why, decision = s.Decision, byName = N(m, s.DecidedBy), at = T(s.DecidedAt), note = s.Note });
        d["steps"] = steps;
        if (detail)
        {
            d["delegate"] = r.Delegate; d["delegateName"] = N(m, r.Delegate); d["onCallTo"] = r.OnCallTo; d["onCallName"] = N(m, r.OnCallTo); d["handover"] = r.Handover; d["reach"] = r.Reach; d["reachText"] = Leaves.ReachText(r);
            d["phone"] = r.Phone; d["addr"] = r.Address; d["note"] = r.Note; d["file"] = r.FileStored != null; d["fileName"] = r.FileOriginal; d["ooo"] = r.Ooo;
            List<object> w = new List<object>(); foreach (string x in (r.Warnings ?? "").Split('\n')) { int i = x.IndexOf('|'); if (i > 0) w.Add(new[] { x.Substring(0, i), x.Substring(i + 1) }); }
            d["warnings"] = w; List<object> log = new List<object>(); foreach (KeyValuePair<DateTime, string> l in r.Log) log.Add(new { t = T(l.Key), x = l.Value }); d["log"] = log;
            LvStaff p; if (r.Type == "yillik" && m.TryGetValue(r.UserId, out p)) { decimal b = Leaves.Balance(p); d["balance"] = b; d["balanceAfter"] = r.Status == "approved" ? b : b - r.Days; }
        }
        return d;
    }
    static Dictionary<string, object> State(int me, bool admin, List<LvStaff> staff, LvSettings st)
    {
        Dictionary<int, LvStaff> m = Leaves.StaffMap(staff); var hol = Leaves.Holidays(); LvStaff p = m[me];
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["me"] = new { id = me, name = p.Name, title = p.Title, dept = p.Dept, onCall = p.OnCall, entitle = Leaves.Entitle(p), years = Math.Floor(Leaves.Years(p)), hasHire = p.Hire.HasValue, balance = Leaves.Balance(p), pending = Leaves.PendingDays(p, 0), approver = p.Approver, approverName = N(m, p.Approver), noApproval = p.NoApproval };
        List<object> sl = new List<object>();
        foreach (LvStaff s in staff) sl.Add(new { id = s.Id, name = s.Name, title = s.Title, dept = s.Dept, onCall = s.OnCall, approver = s.Approver, noApproval = s.NoApproval, delegateApprover = s.DelegateApprover });
        d["staff"] = sl;
        d["settings"] = new { longDays = st.LongDays, unpaidSecond = st.UnpaidSecond, sickNoApproval = st.SickNoApproval, remind = st.Remind, escalate = st.Escalate, second = st.Second, hr = st.Hr };
        Dictionary<string, object> h = new Dictionary<string, object>(); foreach (var x in hol) h[K(x.Key)] = new object[] { x.Value.Key, x.Value.Value }; d["holidays"] = h;
        List<object> ev = new List<object>(); foreach (var x in Leaves.Events()) ev.Add(new { id = x.Key, d = K(x.Value.Key), t = x.Value.Value }); d["events"] = ev;
        List<object> cal = new List<object>(); foreach (LvReq r in Leaves.Calendar()) cal.Add(new { id = r.Id, who = r.UserId, s = K(r.Start), e = K(r.End), status = r.Status, type = r.Type }); d["cal"] = cal;
        Dictionary<string, object> ps = new Dictionary<string, object>(); ps["@u"] = me;
        List<object> mine = new List<object>(); foreach (LvReq r in Leaves.Requests("UserId = @u", ps)) mine.Add(Req(r, m, hol, true)); d["mine"] = mine;
        List<object> inbox = new List<object>();
        foreach (LvReq r in Leaves.Requests("Status = N'pending' AND (EscalatedTo = @u OR EXISTS (SELECT 1 FROM dbo.LeaveSteps s WHERE s.RequestId = LeaveRequests.RequestId AND s.StepNo = LeaveRequests.Step AND s.ApproverUserId = @u))", ps)) inbox.Add(Req(r, m, hol, true));
        d["inbox"] = inbox; d["isAdmin"] = admin;
        // son kararlarim: onaylayicinin karar verdigi talepler (60 gun) kendi kutusundan cikinca da izlenebilsin
        List<object> handled = new List<object>();
        foreach (LvReq r in Leaves.Requests("UserId <> @u AND CreatedAt >= DATEADD(DAY, -60, SYSUTCDATETIME()) AND EXISTS (SELECT 1 FROM dbo.LeaveSteps s WHERE s.RequestId = LeaveRequests.RequestId AND s.DecidedBy = @u)", ps)) handled.Add(Req(r, m, hol, true));
        d["handled"] = handled;
        if (admin)
        {
            List<object> prof = new List<object>();
            foreach (LvStaff s in staff) prof.Add(new { id = s.Id, name = s.Name, email = s.Email, title = s.Title, dept = s.Dept, hire = s.Hire.HasValue ? K(s.Hire.Value) : "", used = s.Used, extra = s.Extra, onCall = s.OnCall, approver = s.Approver, noApproval = s.NoApproval, delegateApprover = s.DelegateApprover, entitle = Leaves.Entitle(s), balance = Leaves.Balance(s) });
            d["profiles"] = prof;
            List<object> hl = new List<object>(); foreach (var x in hol) if (x.Key >= Leaves.Today.AddDays(-31)) hl.Add(new { d = K(x.Key), n = x.Value.Key, half = x.Value.Value }); d["holidayList"] = hl;
        }
        return d;
    }

    static void Deny(HttpContext ctx, int code) { ctx.Response.StatusCode = code; ctx.Response.ContentType = "application/json"; ctx.Response.Write("{\"ok\":false,\"error\":\"Bu sayfa yalnız vendor personeline açıktır.\"}"); }
    static void Out(HttpContext ctx, string err, object data)
    {
        ctx.Response.ContentType = "application/json";
        Dictionary<string, object> d = new Dictionary<string, object>(); d["ok"] = err == null; d["error"] = err; if (data != null) d["data"] = data;
        ctx.Response.Write(JS.Serialize(d));
    }
}
