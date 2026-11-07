using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Configuration;

// Izin talepleri (vendor personeli). 61. migration. Kurallar, hesap ve onay yolu burada; tarayicidaki hesap yalniz anlik gorsel geri bildirim.
public class LvStaff
{
    public int Id; public string Name, Email, Title, Dept; public DateTime? Hire; public decimal Used, Extra;
    public bool OnCall, NoApproval; public int? Approver, DelegateApprover;
}
public class LvStep
{
    public int StepNo, Approver; public int? Original, DecidedBy; public string Label, Why, Decision, Note; public DateTime? DecidedAt;
}
public class LvReq
{
    public int Id, UserId, Step; public string No, Type, Sub, HourFrom, HourTo, Handover, Reach, Phone, Address, Note, FileStored, FileOriginal, Status, Reason, Warnings;
    public DateTime Start, End, CreatedAt; public DateTime? StepStartedAt; public bool StartHalf, EndHalf, Ooo, Reminded, BeforeSent; public decimal Days;
    public int? Delegate, OnCallTo, EscalatedTo;
    public List<LvStep> Steps = new List<LvStep>(); public List<KeyValuePair<DateTime, string>> Log = new List<KeyValuePair<DateTime, string>>();
    public LvStep Current { get { foreach (LvStep s in Steps) if (s.StepNo == Step) return s; return null; } }
}
public class LvSettings
{
    public int LongDays = 5, Remind = 24, Escalate = 48; public bool UnpaidSecond = true, SickNoApproval = true; public int? Second, Hr;
}
public class LvCalc { public decimal Days; public List<string> Holidays = new List<string>(); public DateTime? Return; public bool ReturnAfternoon; public DateTime End; }

public static class Leaves
{
    public static readonly CultureInfo TR = new CultureInfo("tr-TR");
    // tur: ad, birim (workday / calendar / weeks / hour), is devri gerekir, belge zorunlu, gerekce zorunlu
    public static readonly string[] TypeKeys = { "yillik", "mazeret", "hastalik", "saatlik", "ucretsiz", "egitim", "dogumgunu", "dogum" };
    public static string TypeName(string t)
    {
        switch (t)
        {
            case "yillik": return "Yıllık izin"; case "mazeret": return "Mazeret izni"; case "hastalik": return "Hastalık ya da rapor"; case "saatlik": return "Saatlik izin";
            case "ucretsiz": return "Ücretsiz izin"; case "egitim": return "Eğitim ya da etkinlik"; case "dogumgunu": return "Doğum günü izni"; case "dogum": return "Doğum izni"; default: return t;
        }
    }
    public static string Unit(string t) { return t == "hastalik" ? "calendar" : t == "dogum" ? "weeks" : t == "saatlik" ? "hour" : "workday"; }
    public static bool Cover(string t) { return t == "yillik" || t == "mazeret" || t == "ucretsiz" || t == "egitim" || t == "dogum"; }
    public static bool CoverShown(string t) { return Cover(t) || t == "dogumgunu"; }
    public static bool DocRequired(string t) { return t == "hastalik" || t == "dogum"; }
    // mazeret: ad, gun siniri
    public static readonly Dictionary<string, KeyValuePair<string, int>> Subs = new Dictionary<string, KeyValuePair<string, int>>
    {
        { "evlilik", new KeyValuePair<string, int>("Evlilik", 3) }, { "babalik", new KeyValuePair<string, int>("Eşinin doğum yapması", 5) },
        { "vefat", new KeyValuePair<string, int>("Birinci derece yakının vefatı", 3) }, { "tasinma", new KeyValuePair<string, int>("Taşınma (şirket politikası)", 1) }
    };
    public static string TypeText(LvReq r) { return TypeName(r.Type) + (r.Type == "mazeret" && r.Sub != null && Subs.ContainsKey(r.Sub) ? " · " + Subs[r.Sub].Key : ""); }

    public static bool Ready()
    {
        try { using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("SELECT CASE WHEN OBJECT_ID('dbo.LeaveRequests','U') IS NOT NULL AND OBJECT_ID('dbo.LeaveProfiles','U') IS NOT NULL AND OBJECT_ID('dbo.LeaveHolidays','U') IS NOT NULL THEN 1 ELSE 0 END", c)) return (int)q.ExecuteScalar() == 1; }
        catch { return false; }
    }
    public static DateTime Today { get { return DateTime.UtcNow.ToTrLocal().Date; } }
    public static DateTime NowTr { get { return DateTime.UtcNow.ToTrLocal(); } }

    // ================= veri =================
    /// <summary>Vendor firmasinin etkin kullanicilari + izin profilleri</summary>
    public static List<LvStaff> Staff()
    {
        List<LvStaff> l = new List<LvStaff>();
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand(
            "SELECT u.UserId, u.FullName, u.Email, p.Title, p.Department, p.HireDate, ISNULL(p.UsedDays,0), ISNULL(p.ExtraDays,0), ISNULL(p.OnCall,0), ISNULL(p.NoApproval,0), p.ApproverUserId, p.DelegateApproverUserId " +
            "FROM dbo.Users u JOIN dbo.Companies co ON co.CompanyId = u.CompanyId LEFT JOIN dbo.LeaveProfiles p ON p.UserId = u.UserId " +
            "WHERE u.IsActive = 1 AND co.CompanyType = N'Vendor' ORDER BY u.FullName", c))
        using (SqlDataReader r = q.ExecuteReader())
            while (r.Read())
            {
                LvStaff s = new LvStaff(); s.Id = r.GetInt32(0); s.Name = r.GetString(1); s.Email = r.GetString(2); s.Title = r.IsDBNull(3) ? "" : r.GetString(3); s.Dept = r.IsDBNull(4) ? "" : r.GetString(4);
                s.Hire = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5); s.Used = r.GetDecimal(6); s.Extra = r.GetDecimal(7); s.OnCall = Convert.ToBoolean(r[8]); s.NoApproval = Convert.ToBoolean(r[9]);
                s.Approver = r.IsDBNull(10) ? (int?)null : r.GetInt32(10); s.DelegateApprover = r.IsDBNull(11) ? (int?)null : r.GetInt32(11);
                l.Add(s);
            }
        return l;
    }
    public static Dictionary<int, LvStaff> StaffMap(List<LvStaff> l) { Dictionary<int, LvStaff> d = new Dictionary<int, LvStaff>(); foreach (LvStaff s in l) d[s.Id] = s; return d; }
    public static bool IsStaff(int userId) { foreach (LvStaff s in Staff()) if (s.Id == userId) return true; return false; }

    public static LvSettings Settings()
    {
        LvSettings s = new LvSettings();
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT SettingKey, SettingValue FROM dbo.LeaveSettings", c))
        using (SqlDataReader r = q.ExecuteReader())
            while (r.Read())
            {
                string k = r.GetString(0), v = r.GetString(1); int n; bool ok = int.TryParse(v, out n);
                if (k == "longDays" && ok) s.LongDays = n; else if (k == "remindHours" && ok) s.Remind = n; else if (k == "escalateHours" && ok) s.Escalate = n;
                else if (k == "unpaidSecond") s.UnpaidSecond = v == "1"; else if (k == "sickNoApproval") s.SickNoApproval = v == "1";
                else if (k == "secondApprover" && ok && n > 0) s.Second = n; else if (k == "hrUser" && ok && n > 0) s.Hr = n;
            }
        return s;
    }
    public static void SaveSetting(string k, string v)
    {
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("IF EXISTS (SELECT 1 FROM dbo.LeaveSettings WHERE SettingKey = @k) UPDATE dbo.LeaveSettings SET SettingValue = @v WHERE SettingKey = @k ELSE INSERT INTO dbo.LeaveSettings (SettingKey, SettingValue) VALUES (@k, @v)", c))
        { q.Parameters.AddWithValue("@k", k); q.Parameters.AddWithValue("@v", v); q.ExecuteNonQuery(); }
    }

    /// <summary>Resmi tatiller: gun -> (ad, yarim gun mu)</summary>
    public static Dictionary<DateTime, KeyValuePair<string, bool>> Holidays()
    {
        Dictionary<DateTime, KeyValuePair<string, bool>> d = new Dictionary<DateTime, KeyValuePair<string, bool>>();
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT Day, Name, IsHalf FROM dbo.LeaveHolidays", c))
        using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) d[r.GetDateTime(0).Date] = new KeyValuePair<string, bool>(r.GetString(1), Convert.ToBoolean(r[2]));
        return d;
    }
    public static List<KeyValuePair<int, KeyValuePair<DateTime, string>>> Events()
    {
        List<KeyValuePair<int, KeyValuePair<DateTime, string>>> l = new List<KeyValuePair<int, KeyValuePair<DateTime, string>>>();
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT EventId, Day, Title FROM dbo.LeaveEvents WHERE Day >= DATEADD(DAY, -30, CAST(GETDATE() AS DATE)) ORDER BY Day", c))
        using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) l.Add(new KeyValuePair<int, KeyValuePair<DateTime, string>>(r.GetInt32(0), new KeyValuePair<DateTime, string>(r.GetDateTime(1), r.GetString(2))));
        return l;
    }

    /// <summary>Talepler: kosul (WHERE parcasi) ve parametreleri; adimlar ve gecmisle</summary>
    public static List<LvReq> Requests(string where, Dictionary<string, object> ps)
    {
        List<LvReq> l = new List<LvReq>(); Dictionary<int, LvReq> byId = new Dictionary<int, LvReq>();
        using (SqlConnection c = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT RequestId, RequestNo, UserId, LeaveType, SubType, StartDate, EndDate, StartHalf, EndHalf, HourFrom, HourTo, Days, DelegateUserId, OnCallUserId, Handover, Reach, Phone, LeaveAddress, Note, " +
                "FileStored, FileOriginal, OooReminder, Status, Step, StepStartedAt, Reminded, EscalatedTo, BeforeSent, Reason, Warnings, CreatedAt FROM dbo.LeaveRequests WHERE " + where + " ORDER BY CreatedAt DESC, RequestId DESC", c))
            {
                if (ps != null) foreach (KeyValuePair<string, object> p in ps) q.Parameters.AddWithValue(p.Key, p.Value);
                using (SqlDataReader r = q.ExecuteReader())
                    while (r.Read())
                    {
                        LvReq x = new LvReq(); x.Id = r.GetInt32(0); x.No = r.IsDBNull(1) ? "" : r.GetString(1); x.UserId = r.GetInt32(2); x.Type = r.GetString(3); x.Sub = r.IsDBNull(4) ? null : r.GetString(4);
                        x.Start = r.GetDateTime(5); x.End = r.GetDateTime(6); x.StartHalf = r.GetBoolean(7); x.EndHalf = r.GetBoolean(8); x.HourFrom = r.IsDBNull(9) ? null : r.GetString(9); x.HourTo = r.IsDBNull(10) ? null : r.GetString(10);
                        x.Days = r.GetDecimal(11); x.Delegate = r.IsDBNull(12) ? (int?)null : r.GetInt32(12); x.OnCallTo = r.IsDBNull(13) ? (int?)null : r.GetInt32(13); x.Handover = r.IsDBNull(14) ? "" : r.GetString(14);
                        x.Reach = r.GetString(15); x.Phone = r.IsDBNull(16) ? "" : r.GetString(16); x.Address = r.IsDBNull(17) ? "" : r.GetString(17); x.Note = r.IsDBNull(18) ? "" : r.GetString(18);
                        x.FileStored = r.IsDBNull(19) ? null : r.GetString(19); x.FileOriginal = r.IsDBNull(20) ? null : r.GetString(20); x.Ooo = r.GetBoolean(21); x.Status = r.GetString(22); x.Step = r.GetInt32(23);
                        x.StepStartedAt = r.IsDBNull(24) ? (DateTime?)null : r.GetDateTime(24); x.Reminded = r.GetBoolean(25); x.EscalatedTo = r.IsDBNull(26) ? (int?)null : r.GetInt32(26); x.BeforeSent = r.GetBoolean(27);
                        x.Reason = r.IsDBNull(28) ? null : r.GetString(28); x.Warnings = r.IsDBNull(29) ? "" : r.GetString(29); x.CreatedAt = r.GetDateTime(30);
                        l.Add(x); byId[x.Id] = x;
                    }
            }
            if (l.Count == 0) return l;
            string ids = string.Join(",", new List<int>(byId.Keys).ConvertAll(delegate (int i) { return i.ToString(CultureInfo.InvariantCulture); }).ToArray());
            using (SqlCommand q = new SqlCommand("SELECT RequestId, StepNo, Label, ApproverUserId, OriginalUserId, Why, DecidedBy, DecidedAt, Decision, DecisionNote FROM dbo.LeaveSteps WHERE RequestId IN (" + ids + ") ORDER BY RequestId, StepNo", c))
            using (SqlDataReader r = q.ExecuteReader())
                while (r.Read())
                {
                    LvStep s = new LvStep(); s.StepNo = r.GetInt32(1); s.Label = r.GetString(2); s.Approver = r.GetInt32(3); s.Original = r.IsDBNull(4) ? (int?)null : r.GetInt32(4); s.Why = r.IsDBNull(5) ? null : r.GetString(5);
                    s.DecidedBy = r.IsDBNull(6) ? (int?)null : r.GetInt32(6); s.DecidedAt = r.IsDBNull(7) ? (DateTime?)null : r.GetDateTime(7); s.Decision = r.IsDBNull(8) ? null : r.GetString(8); s.Note = r.IsDBNull(9) ? null : r.GetString(9);
                    byId[r.GetInt32(0)].Steps.Add(s);
                }
            using (SqlCommand q = new SqlCommand("SELECT RequestId, At, Text FROM dbo.LeaveLog WHERE RequestId IN (" + ids + ") ORDER BY At DESC, LogId DESC", c))
            using (SqlDataReader r = q.ExecuteReader()) while (r.Read()) byId[r.GetInt32(0)].Log.Add(new KeyValuePair<DateTime, string>(r.GetDateTime(1), r.GetString(2)));
        }
        return l;
    }
    public static LvReq Get(int id) { Dictionary<string, object> p = new Dictionary<string, object>(); p["@i"] = id; List<LvReq> l = Requests("RequestId = @i", p); return l.Count > 0 ? l[0] : null; }
    /// <summary>Cakisma ve vekil kontrolu icin: onayli ya da onay bekleyen izinler (gecmis 60 gun ve sonrasi)</summary>
    public static List<LvReq> Calendar() { return Requests("Status IN (N'approved', N'pending', N'recorded') AND EndDate >= DATEADD(DAY, -60, CAST(GETDATE() AS DATE))", null); }

    // ================= hesap =================
    public static decimal DayValue(DateTime d, Dictionary<DateTime, KeyValuePair<string, bool>> hol)
    {
        if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) return 0;
        KeyValuePair<string, bool> h; if (hol.TryGetValue(d.Date, out h)) return h.Value ? 0.5m : 0;
        return 1;
    }
    public static LvCalc Calc(string type, DateTime start, DateTime end, bool sh, bool eh, string hFrom, string hTo, Dictionary<DateTime, KeyValuePair<string, bool>> hol)
    {
        LvCalc c = new LvCalc(); c.End = type == "dogum" ? start.AddDays(111) : type == "saatlik" ? start : end;
        if (type == "saatlik")
        {
            TimeSpan a, b; if (TimeSpan.TryParse(hFrom ?? "", out a) && TimeSpan.TryParse(hTo ?? "", out b)) c.Days = (decimal)Math.Round((b - a).TotalHours, 2);
            return c;
        }
        string u = Unit(type);
        if (u == "calendar" || u == "weeks") c.Days = (decimal)((c.End - start).TotalDays + 1);
        else
        {
            for (DateTime d = start; d <= c.End; d = d.AddDays(1))
            {
                c.Days += DayValue(d, hol);
                KeyValuePair<string, bool> h;
                if (hol.TryGetValue(d, out h) && d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) c.Holidays.Add(d.ToString("d MMMM", TR) + " " + h.Key + (h.Value ? " (yarım gün)" : ""));
            }
            if (sh && DayValue(start, hol) == 1) c.Days -= 0.5m;
            if (eh && DayValue(c.End, hol) == 1) c.Days -= 0.5m;
        }
        if (eh) { c.Return = c.End; c.ReturnAfternoon = true; }
        else { DateTime n = c.End.AddDays(1); int guard = 0; while (DayValue(n, hol) == 0 && guard++ < 30) n = n.AddDays(1); c.Return = n; }
        return c;
    }
    public static double Years(LvStaff s) { return s.Hire.HasValue ? (Today - s.Hire.Value).TotalDays / 365.25 : 0; }
    /// <summary>Is Kanunu 53: 1 yildan az 0; 1-5 yil 14; 5-15 yil 20; 15 yil ve ustu 26 gun</summary>
    public static int Entitle(LvStaff s) { if (!s.Hire.HasValue) return 0; double y = Years(s); return y < 1 ? 0 : y < 5 ? 14 : y < 15 ? 20 : 26; }
    static decimal YillikSum(int userId, string statuses, int exceptId)
    {
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT ISNULL(SUM(Days),0) FROM dbo.LeaveRequests WHERE UserId = @u AND LeaveType = N'yillik' AND YEAR(StartDate) = YEAR(GETDATE()) AND Status IN (" + statuses + ") AND RequestId <> @x", c))
        { q.Parameters.AddWithValue("@u", userId); q.Parameters.AddWithValue("@x", exceptId); return Convert.ToDecimal(q.ExecuteScalar()); }
    }
    /// <summary>Kalan: hak + devreden - sistem disi kullanilan - bu yil onaylanan</summary>
    public static decimal Balance(LvStaff s) { return Entitle(s) + s.Extra - s.Used - YillikSum(s.Id, "N'approved'", 0); }
    public static decimal PendingDays(LvStaff s, int exceptId) { return YillikSum(s.Id, "N'pending', N'revise'", exceptId); }

    // ================= onay yolu =================
    public static LvReq OnLeave(int userId, DateTime day, List<LvReq> cal)
    {
        foreach (LvReq r in cal) if (r.UserId == userId && r.Status == "approved" && r.Start <= day && day <= r.End && r.Type != "saatlik") return r;
        return null;
    }
    /// <summary>Onay adimlari ve bilgilendirilecekler. Hata: onaylayici tanimli degil.</summary>
    public static List<LvStep> Chain(LvReq r, Dictionary<int, LvStaff> map, LvSettings st, List<LvReq> cal, List<int> info, out string err)
    {
        err = null; List<LvStep> steps = new List<LvStep>(); LvStaff p = map[r.UserId];
        int? first = p.NoApproval ? (int?)null : (p.Approver.HasValue && map.ContainsKey(p.Approver.Value) ? p.Approver : st.Hr);
        if (r.Type == "hastalik" && st.SickNoApproval)
        {
            if (p.Approver.HasValue) info.Add(p.Approver.Value);
        }
        else
        {
            if (r.Type == "dogum") { if (st.Hr.HasValue) steps.Add(Step(st.Hr.Value, "İK onayı", null)); if (p.Approver.HasValue) info.Add(p.Approver.Value); }
            else if (first.HasValue) steps.Add(Step(first.Value, p.Approver.HasValue ? "Yönetici onayı" : "İK onayı", null));
            else if (!p.NoApproval) { err = "Onaylayıcınız tanımlı değil. Süper admin yetkisindeki yöneticinizden İzinlerim › Kurallar bölümünde onaylayıcınızı belirlemesini isteyin."; return steps; }
            bool lng = r.Type == "yillik" && r.Days > st.LongDays, unpaid = r.Type == "ucretsiz" && st.UnpaidSecond;
            if ((lng || unpaid) && st.Second.HasValue && st.Second.Value != p.Id && !steps.Exists(delegate (LvStep s) { return s.Approver == st.Second.Value; }))
                steps.Add(Step(st.Second.Value, "Genel Müdür onayı", lng ? st.LongDays + " iş gününü aşan yıllık izin" : "Ücretsiz izin"));
        }
        // onaylayici su an izindeyse: vekil onaylayici
        foreach (LvStep s in steps)
        {
            LvStaff a; if (!map.TryGetValue(s.Approver, out a)) continue;
            if (OnLeave(s.Approver, Today, cal) != null && a.DelegateApprover.HasValue && a.DelegateApprover.Value != p.Id) { s.Original = s.Approver; s.Approver = a.DelegateApprover.Value; }
        }
        steps.RemoveAll(delegate (LvStep s) { return s.Approver == p.Id; });
        for (int i = 0; i < steps.Count; i++) steps[i].StepNo = i;
        if (r.Delegate.HasValue) info.Add(r.Delegate.Value);
        if (r.OnCallTo.HasValue) info.Add(r.OnCallTo.Value);
        if (st.Hr.HasValue) info.Add(st.Hr.Value);
        List<int> seen = new List<int>();
        info.RemoveAll(delegate (int x) { bool drop = x == p.Id || seen.Contains(x) || (steps.Exists(delegate (LvStep s) { return s.Approver == x; }) && x != (st.Hr ?? -1)); if (!drop) seen.Add(x); return drop; });
        return steps;
    }
    static LvStep Step(int who, string label, string why) { LvStep s = new LvStep(); s.Approver = who; s.Label = label; s.Why = why; return s; }

    // ================= dogrulama =================
    public static List<string> Validate(LvReq f, LvCalc c, LvStaff p, Dictionary<DateTime, KeyValuePair<string, bool>> hol, int exceptId, List<string> warns, List<LvReq> cal, Dictionary<int, LvStaff> map, List<KeyValuePair<int, KeyValuePair<DateTime, string>>> events)
    {
        List<string> e = new List<string>(); DateTime td = Today;
        if (Array.IndexOf(TypeKeys, f.Type) < 0) { e.Add("Geçersiz izin türü."); return e; }
        if (f.Type == "saatlik")
        {
            if (f.Start < td) e.Add("Geçmiş bir tarih için saatlik izin istenemez.");
            if (DayValue(f.Start, hol) == 0) e.Add("Seçilen gün çalışma günü değil.");
            if (c.Days <= 0) e.Add("Bitiş saati başlangıçtan sonra olmalı."); else if (c.Days > 4) e.Add("Saatlik izin en fazla 4 saat olabilir. Daha uzun süre için yarım gün yıllık izin seçin.");
            if (string.CompareOrdinal(f.HourFrom ?? "", "08:30") < 0 || string.CompareOrdinal(f.HourTo ?? "", "18:30") > 0) e.Add("Saatlik izin mesai saatleri (08.30 ile 18.30) içinde olmalı.");
        }
        else
        {
            if (f.Type != "dogum" && f.End < f.Start) e.Add("Bitiş tarihi başlangıçtan önce olamaz.");
            if (f.Start < td && f.Type != "hastalik") e.Add("Geçmiş tarihli izin istenemez. Rapor için “Hastalık ya da rapor” türünü seçin.");
            if (f.Type == "hastalik" && (td - f.Start).TotalDays > 30) e.Add("30 günden eski raporlar portaldan girilemez; İK ile görüşün.");
            if (c.End > td.AddMonths(13)) e.Add("En fazla 13 ay sonrası için izin istenebilir.");
            if (Unit(f.Type) == "workday" && c.Days <= 0) e.Add("Seçilen aralıkta çalışma günü yok.");
            if (f.Start == c.End && f.StartHalf && f.EndHalf) e.Add("Aynı gün için hem öğleden sonra başlangıç hem öğlene kadar bitiş seçilemez.");
        }
        if (f.Type == "yillik")
        {
            if (Entitle(p) == 0) e.Add(p.Hire.HasValue ? "Bir yıllık kıdemi doldurmadan yıllık izin hakkı doğmaz (İş Kanunu 53. madde). Mazeret ya da ücretsiz izin seçebilirsiniz." : "İşe giriş tarihiniz tanımlı değil; yıllık izin hakkınız hesaplanamıyor. İK ile görüşün.");
            else { decimal left = Balance(p) - PendingDays(p, exceptId); if (c.Days > left) e.Add("Yıllık izin bakiyeniz yetersiz (kalan " + Num(left) + " gün). Fazlası için ücretsiz izin talebi oluşturun."); }
        }
        if (f.Type == "mazeret") { if (f.Sub == null || !Subs.ContainsKey(f.Sub)) e.Add("Mazeret nedenini seçin."); else if (c.Days > Subs[f.Sub].Value) e.Add(Subs[f.Sub].Key + " izni " + Subs[f.Sub].Value + " günü aşamaz. Fazlası için yıllık izin talebi oluşturun."); }
        if (f.Type == "dogumgunu")
        {
            if (c.Days > 1) e.Add("Doğum günü izni 1 iş günüdür.");
            foreach (LvReq x in cal) if (x.UserId == p.Id && x.Type == "dogumgunu" && x.Id != exceptId && x.Start.Year == f.Start.Year) { e.Add("Bu yıl doğum günü izninizi kullandınız."); break; }
        }
        bool needCover = Cover(f.Type) && c.Days >= 1;
        if (needCover && !f.Delegate.HasValue) e.Add("İşlerinizi takip edecek vekili seçin.");
        if (f.Delegate.HasValue && (f.Delegate.Value == p.Id || !map.ContainsKey(f.Delegate.Value))) e.Add("Geçerli bir vekil seçin.");
        if (p.OnCall && CoverShown(f.Type) && f.Type != "saatlik" && !f.OnCallTo.HasValue) e.Add("Nöbet listesindesiniz; nöbeti devredeceğiniz kişiyi seçin.");
        if (f.Type == "ucretsiz" && (f.Note ?? "").Trim().Length < 10) e.Add("Ücretsiz izin için gerekçe yazın (en az 10 karakter).");
        if (f.Reach == "urgent" && string.IsNullOrEmpty(f.Phone)) e.Add("Acil durumda ulaşılabilecekseniz telefon numaranızı yazın.");
        // uyarilar (engellemez)
        foreach (KeyValuePair<int, KeyValuePair<DateTime, string>> ev in events)
            if (ev.Value.Key >= f.Start && ev.Value.Key <= c.End) warns.Add("warn|Planlı takvim: " + ev.Value.Key.ToString("d MMMM", TR) + ", " + ev.Value.Value + ".");
        foreach (LvReq x in cal)
        {
            if (x.UserId == p.Id || x.Id == exceptId || x.End < f.Start || x.Start > c.End || !map.ContainsKey(x.UserId)) continue;
            LvStaff q = map[x.UserId]; string when = x.Start.ToString("d MMMM", TR) + " ile " + x.End.ToString("d MMMM", TR) + " arası";
            if (f.Delegate.HasValue && x.UserId == f.Delegate.Value) warns.Add("warn|Vekil izinde: " + q.Name + ", " + when + " izinli.");
            else if (!string.IsNullOrEmpty(p.Dept) && q.Dept == p.Dept) warns.Add("info|Ekipten izinli: " + q.Name + ", " + when + (x.Status == "pending" ? " (onay bekliyor)" : "") + ".");
        }
        return e;
    }
    public static string Num(decimal d) { return d == Math.Floor(d) ? ((int)d).ToString(TR) : d.ToString("0.#", TR); }

    // ================= yasam dongusu =================
    public static string NextNo(int id) { return "IZN-" + Today.Year + "-" + id.ToString("0000", CultureInfo.InvariantCulture); }
    public static void Log(SqlConnection c, SqlTransaction tx, int rid, string text)
    {
        using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeaveLog (RequestId, Text) VALUES (@r, @t)", c, tx)) { q.Parameters.AddWithValue("@r", rid); q.Parameters.AddWithValue("@t", text.Length > 500 ? text.Substring(0, 500) : text); q.ExecuteNonQuery(); }
    }
    static object N(int? v) { return v.HasValue ? (object)v.Value : DBNull.Value; }
    static object S(string v) { return string.IsNullOrEmpty(v) ? (object)DBNull.Value : v; }

    /// <summary>Yeni talep ya da duzeltme sonrasi yeniden gonderim. Hata metni ya da null.</summary>
    public static string Submit(LvReq f, int editId, string baseUrl, out int newId)
    {
        newId = 0;
        List<LvStaff> staff = Staff(); Dictionary<int, LvStaff> map = StaffMap(staff); LvStaff p;
        if (!map.TryGetValue(f.UserId, out p)) return "İzin talebi yalnız vendor personeline açıktır.";
        LvReq old = null;
        if (editId > 0) { old = Get(editId); if (old == null || old.UserId != f.UserId || old.Status != "revise") return "Bu talep güncellenemez."; }
        var hol = Holidays(); LvSettings st = Settings(); List<LvReq> cal = Calendar(); var events = Events();
        LvCalc c = Calc(f.Type, f.Start, f.End, f.StartHalf, f.EndHalf, f.HourFrom, f.HourTo, hol);
        f.End = c.End; f.Days = c.Days;
        if (!CoverShown(f.Type)) { f.Delegate = null; f.OnCallTo = null; }
        if (f.Type == "saatlik" || f.Type == "hastalik" || f.Type == "dogum") { f.StartHalf = false; f.EndHalf = false; }
        if (f.Type != "mazeret") f.Sub = null;
        List<string> warns = new List<string>();
        List<string> errs = Validate(f, c, p, hol, editId, warns, cal, map, events);
        if (DocRequired(f.Type) && string.IsNullOrEmpty(f.FileStored) && (old == null || string.IsNullOrEmpty(old.FileStored))) errs.Add(f.Type == "hastalik" ? "Sağlık raporunu yükleyin (PDF ya da fotoğraf)." : "Doğum izni için doktor raporunu yükleyin.");
        if (errs.Count > 0) return string.Join("\n", errs.ToArray());
        List<int> info = new List<int>(); string cerr;
        List<LvStep> steps = Chain(f, map, st, cal, info, out cerr);
        if (cerr != null) return cerr;
        bool sick = f.Type == "hastalik" && st.SickNoApproval;
        string status = sick ? "recorded" : steps.Count == 0 ? "approved" : "pending";
        f.Warnings = string.Join("\n", warns.ToArray()); f.Status = status;
        if (old != null && string.IsNullOrEmpty(f.FileStored)) { f.FileStored = old.FileStored; f.FileOriginal = old.FileOriginal; }
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            int id = editId;
            string cols = "UserId=@u, LeaveType=@t, SubType=@sb, StartDate=@s, EndDate=@e, StartHalf=@sh, EndHalf=@eh, HourFrom=@hf, HourTo=@ht, Days=@d, DelegateUserId=@dl, OnCallUserId=@oc, Handover=@ho, Reach=@rc, Phone=@ph, LeaveAddress=@ad, Note=@nt, " +
                          "FileStored=@fs, FileOriginal=@fo, OooReminder=@oo, Status=@st, Step=0, StepStartedAt=SYSUTCDATETIME(), Reminded=0, EscalatedTo=NULL, BeforeSent=0, Reason=NULL, Warnings=@w, UpdatedAt=SYSUTCDATETIME()";
            string sql = editId > 0 ? "UPDATE dbo.LeaveRequests SET " + cols + " WHERE RequestId = @id; SELECT @id;" :
                "INSERT INTO dbo.LeaveRequests (UserId, LeaveType, SubType, StartDate, EndDate, StartHalf, EndHalf, HourFrom, HourTo, Days, DelegateUserId, OnCallUserId, Handover, Reach, Phone, LeaveAddress, Note, FileStored, FileOriginal, OooReminder, Status, Step, StepStartedAt, Warnings) " +
                "VALUES (@u, @t, @sb, @s, @e, @sh, @eh, @hf, @ht, @d, @dl, @oc, @ho, @rc, @ph, @ad, @nt, @fs, @fo, @oo, @st, 0, SYSUTCDATETIME(), @w); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (SqlCommand q = new SqlCommand(sql, con, tx))
            {
                q.Parameters.AddWithValue("@u", f.UserId); q.Parameters.AddWithValue("@t", f.Type); q.Parameters.AddWithValue("@sb", S(f.Sub)); q.Parameters.AddWithValue("@s", f.Start); q.Parameters.AddWithValue("@e", f.End);
                q.Parameters.AddWithValue("@sh", f.StartHalf); q.Parameters.AddWithValue("@eh", f.EndHalf); q.Parameters.AddWithValue("@hf", f.Type == "saatlik" ? S(f.HourFrom) : DBNull.Value); q.Parameters.AddWithValue("@ht", f.Type == "saatlik" ? S(f.HourTo) : DBNull.Value);
                q.Parameters.AddWithValue("@d", f.Days); q.Parameters.AddWithValue("@dl", N(f.Delegate)); q.Parameters.AddWithValue("@oc", N(f.OnCallTo)); q.Parameters.AddWithValue("@ho", S(Cut(f.Handover, 800))); q.Parameters.AddWithValue("@rc", f.Reach);
                q.Parameters.AddWithValue("@ph", S(Cut(f.Phone, 40))); q.Parameters.AddWithValue("@ad", S(Cut(f.Address, 200))); q.Parameters.AddWithValue("@nt", S(Cut(f.Note, 600))); q.Parameters.AddWithValue("@fs", S(f.FileStored)); q.Parameters.AddWithValue("@fo", S(f.FileOriginal));
                q.Parameters.AddWithValue("@oo", f.Ooo); q.Parameters.AddWithValue("@st", status); q.Parameters.AddWithValue("@w", S(Cut(f.Warnings, 2000))); q.Parameters.AddWithValue("@id", editId);
                id = Convert.ToInt32(q.ExecuteScalar());
            }
            using (SqlCommand q = new SqlCommand("DELETE FROM dbo.LeaveSteps WHERE RequestId = @r; UPDATE dbo.LeaveRequests SET RequestNo = ISNULL(RequestNo, @no) WHERE RequestId = @r;", con, tx))
            { q.Parameters.AddWithValue("@r", id); q.Parameters.AddWithValue("@no", NextNo(id)); q.ExecuteNonQuery(); }
            foreach (LvStep s in steps)
                using (SqlCommand q = new SqlCommand("INSERT INTO dbo.LeaveSteps (RequestId, StepNo, Label, ApproverUserId, OriginalUserId, Why) VALUES (@r, @n, @l, @a, @o, @w)", con, tx))
                { q.Parameters.AddWithValue("@r", id); q.Parameters.AddWithValue("@n", s.StepNo); q.Parameters.AddWithValue("@l", s.Label); q.Parameters.AddWithValue("@a", s.Approver); q.Parameters.AddWithValue("@o", N(s.Original)); q.Parameters.AddWithValue("@w", S(s.Why)); q.ExecuteNonQuery(); }
            Log(con, tx, id, p.Name + " talebi " + (editId > 0 ? "güncelleyip yeniden gönderdi" : "gönderdi"));
            if (sick) Log(con, tx, id, "Rapor kaydedildi; yönetici ve İK bilgilendirildi");
            tx.Commit(); newId = id;
        }
        LvReq r = Get(newId);
        if (sick) { Mail("sickAck", r, map, st, baseUrl); Mail("sickInfo", r, map, st, baseUrl); }
        else if (status == "approved") { Mail("submitted", r, map, st, baseUrl); Finalize(r, map, st, baseUrl, null); }
        else { Mail("submitted", r, map, st, baseUrl); Mail("approve", r, map, st, baseUrl); }
        return null;
    }
    static string Cut(string s, int n) { s = (s ?? "").Trim(); return s.Length > n ? s.Substring(0, n) : s; }

    static void Finalize(LvReq r, Dictionary<int, LvStaff> map, LvSettings st, string baseUrl, SqlConnection con)
    {
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("UPDATE dbo.LeaveRequests SET Status = N'approved', UpdatedAt = SYSUTCDATETIME() WHERE RequestId = @r", c))
        { q.Parameters.AddWithValue("@r", r.Id); q.ExecuteNonQuery(); using (SqlTransaction tx = c.BeginTransaction()) { Log(c, tx, r.Id, "Talep onaylandı; vekil ve İK bilgilendirildi"); tx.Commit(); } }
        r = Get(r.Id);
        Mail("approved", r, map, st, baseUrl);
        if (r.Delegate.HasValue) Mail("cover", r, map, st, baseUrl);
        if (st.Hr.HasValue) Mail("hr", r, map, st, baseUrl);
    }

    /// <summary>Onay / ret / duzeltme. by: islemi yapan (onaylayici ya da yukseltilen kisi)</summary>
    public static string Decide(int id, int by, string act, string note, string baseUrl, out string msg)
    {
        msg = null; LvReq r = Get(id);
        if (r == null) return "Talep bulunamadı.";
        LvStep s = r.Current;
        if (r.Status != "pending" || s == null) return "Bu talep artık onay beklemiyor.";
        if (s.Approver != by && r.EscalatedTo != by) return "Bu talebin onaylayıcısı değilsiniz.";
        note = Cut(note, 400);
        if ((act == "reject" || act == "revise") && note.Length < 5) return (act == "reject" ? "Reddetme" : "Düzeltme isteği") + " için gerekçe yazın; talep sahibine iletilir.";
        if (act != "approve" && act != "reject" && act != "revise") return "Geçersiz işlem.";
        Dictionary<int, LvStaff> map = StaffMap(Staff()); LvSettings st = Settings(); string byName = map.ContainsKey(by) ? map[by].Name : "Onaylayıcı";
        using (SqlConnection c = Db.Open())
        using (SqlTransaction tx = c.BeginTransaction())
        {
            using (SqlCommand q = new SqlCommand("UPDATE dbo.LeaveSteps SET DecidedBy = @b, DecidedAt = SYSUTCDATETIME(), Decision = @a, DecisionNote = @n WHERE RequestId = @r AND StepNo = @s", c, tx))
            { q.Parameters.AddWithValue("@b", by); q.Parameters.AddWithValue("@a", act); q.Parameters.AddWithValue("@n", S(note)); q.Parameters.AddWithValue("@r", id); q.Parameters.AddWithValue("@s", s.StepNo); q.ExecuteNonQuery(); }
            bool last = s.StepNo >= r.Steps.Count - 1;
            string upd = act == "approve" ? (last ? "" : "Step = Step + 1, StepStartedAt = SYSUTCDATETIME(), Reminded = 0, EscalatedTo = NULL") : "Status = @st, Reason = @n";
            if (upd.Length > 0)
                using (SqlCommand q = new SqlCommand("UPDATE dbo.LeaveRequests SET " + upd + ", UpdatedAt = SYSUTCDATETIME() WHERE RequestId = @r", c, tx))
                { q.Parameters.AddWithValue("@st", act == "reject" ? "rejected" : "revise"); q.Parameters.AddWithValue("@n", S(note)); q.Parameters.AddWithValue("@r", id); q.ExecuteNonQuery(); }
            Log(c, tx, id, byName + (act == "approve" ? " onayladı" : act == "reject" ? " reddetti" : " düzeltme istedi") + (note.Length > 0 ? ": “" + note + "”" : ""));
            tx.Commit();
        }
        r = Get(id); string owner = map.ContainsKey(r.UserId) ? map[r.UserId].Name : "Talep sahibi";
        if (act == "approve")
        {
            if (r.Status == "pending" && r.Step > s.StepNo) { Mail("approve", r, map, st, baseUrl); LvStep n = r.Current; msg = "Onaylandı. Talep " + (map.ContainsKey(n.Approver) ? map[n.Approver].Name : "sonraki onaylayıcı") + " onayına geçti."; }
            else { Finalize(r, map, st, baseUrl, null); msg = "Talep onaylandı. " + owner + ", vekil ve İK bilgilendirildi."; }
        }
        else if (act == "reject") { Mail("rejected", r, map, st, baseUrl); msg = "Talep reddedildi. " + owner + " gerekçeyle bilgilendirildi."; }
        else { Mail("revise", r, map, st, baseUrl); msg = "Düzeltme isteği " + owner + " kişisine gönderildi."; }
        return null;
    }

    public static string Cancel(int id, int userId, string baseUrl)
    {
        LvReq r = Get(id);
        if (r == null || r.UserId != userId) return "Talep bulunamadı.";
        if (r.Status != "pending" && r.Status != "revise") return "Yalnız onay bekleyen ya da düzeltme istenen talepler geri çekilebilir.";
        using (SqlConnection c = Db.Open())
        using (SqlTransaction tx = c.BeginTransaction())
        {
            using (SqlCommand q = new SqlCommand("UPDATE dbo.LeaveRequests SET Status = N'cancelled', UpdatedAt = SYSUTCDATETIME() WHERE RequestId = @r", c, tx)) { q.Parameters.AddWithValue("@r", id); q.ExecuteNonQuery(); }
            Log(c, tx, id, "Talep sahibi talebi geri çekti"); tx.Commit();
        }
        if (r.Status == "pending") { Dictionary<int, LvStaff> map = StaffMap(Staff()); Mail("cancelled", r, map, Settings(), baseUrl); }
        return null;
    }

    // ================= zamanlanmis isler =================
    /// <summary>Hatirlatma, yukseltme ve izin oncesi hatirlatma. Cron (saatlik) ve sayfa acilisinda (en sik 10 dk) calisir.</summary>
    public static string Tick(string baseUrl, bool force)
    {
        HttpContext ctx = HttpContext.Current;
        if (!force && ctx != null)
        {
            object last = ctx.Application["lvTick"];
            if (last is DateTime && (DateTime.UtcNow - (DateTime)last).TotalMinutes < 10) return "skip";
            ctx.Application["lvTick"] = DateTime.UtcNow;
        }
        int rem = 0, esc = 0, bef = 0;
        Dictionary<int, LvStaff> map = StaffMap(Staff()); LvSettings st = Settings(); var hol = Holidays();
        foreach (LvReq r in Requests("Status = N'pending'", null))
        {
            LvStep s = r.Current; if (s == null || !r.StepStartedAt.HasValue) continue;
            double h = (DateTime.UtcNow - r.StepStartedAt.Value).TotalHours;
            if (h >= st.Remind && !r.Reminded) { Flag(r.Id, "Reminded = 1", (map.ContainsKey(s.Approver) ? map[s.Approver].Name : "Onaylayıcı") + " kişisine hatırlatma gönderildi"); Mail("remind", r, map, st, baseUrl); rem++; }
            if (h >= st.Escalate && !r.EscalatedTo.HasValue)
            {
                LvStaff a; int up = map.TryGetValue(s.Approver, out a) && a.Approver.HasValue && a.Approver.Value != r.UserId ? a.Approver.Value : (st.Hr ?? 0);
                if (up > 0 && up != s.Approver)
                {
                    using (SqlConnection c = Db.Open()) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeaveRequests SET EscalatedTo = @u WHERE RequestId = @r", c)) { q.Parameters.AddWithValue("@u", up); q.Parameters.AddWithValue("@r", r.Id); q.ExecuteNonQuery(); }
                    Flag(r.Id, null, st.Escalate + " saat yanıt gelmedi; " + (map.ContainsKey(up) ? map[up].Name : "üst yönetici") + " kişisine yükseltildi");
                    r.EscalatedTo = up; Mail("escalate", r, map, st, baseUrl); esc++;
                }
            }
        }
        DateTime now = NowTr;
        foreach (LvReq r in Requests("Status = N'approved' AND OooReminder = 1 AND BeforeSent = 0 AND StartDate > CAST(GETDATE() AS DATE) AND StartDate <= DATEADD(DAY, 6, CAST(GETDATE() AS DATE))", null))
        {
            DateTime d = r.Start.AddDays(-1); int g = 0; while (DayValue(d, hol) == 0 && g++ < 10) d = d.AddDays(-1);
            if (now >= d.AddHours(9)) { Flag(r.Id, "BeforeSent = 1", "İzin öncesi hatırlatma gönderildi"); Mail("before", r, map, st, baseUrl); bef++; }
        }
        return "hatirlatma " + rem + ", yukseltme " + esc + ", izin oncesi " + bef;
    }
    static void Flag(int id, string set, string log)
    {
        using (SqlConnection c = Db.Open())
        using (SqlTransaction tx = c.BeginTransaction())
        {
            if (set != null) using (SqlCommand q = new SqlCommand("UPDATE dbo.LeaveRequests SET " + set + " WHERE RequestId = @r", c, tx)) { q.Parameters.AddWithValue("@r", id); q.ExecuteNonQuery(); }
            Log(c, tx, id, log); tx.Commit();
        }
    }

    // ================= metinler =================
    public static string RangeText(LvReq r)
    {
        if (r.Type == "saatlik") return r.Start.ToString("d MMMM yyyy dddd", TR) + ", " + r.HourFrom + " ile " + r.HourTo + " arası";
        string a = r.Start.ToString("d MMMM yyyy dddd", TR) + (r.StartHalf ? " (öğleden sonra)" : ""), b = r.End.ToString("d MMMM yyyy dddd", TR) + (r.EndHalf ? " (öğlene kadar)" : "");
        return r.Start == r.End ? a : a + " ile " + b + " arası";
    }
    public static string RangeShort(LvReq r) { if (r.Type == "saatlik") return r.Start.ToString("d MMMM", TR) + ", " + r.HourFrom + " ile " + r.HourTo; return r.Start == r.End ? r.Start.ToString("d MMMM", TR) : r.Start.ToString("d MMMM", TR) + " ile " + r.End.ToString("d MMMM", TR); }
    public static string DurText(LvReq r)
    {
        if (r.Type == "saatlik") return Num(r.Days) + " saat"; if (r.Type == "hastalik") return Num(r.Days) + " takvim günü"; if (r.Type == "dogum") return "16 hafta (112 takvim günü)";
        return Num(r.Days) + " iş günü";
    }
    public static string ReturnText(LvReq r, Dictionary<DateTime, KeyValuePair<string, bool>> hol)
    {
        if (r.Type == "saatlik") return null; LvCalc c = Calc(r.Type, r.Start, r.End, r.StartHalf, r.EndHalf, null, null, hol);
        return c.Return.HasValue ? c.Return.Value.ToString("d MMMM yyyy dddd", TR) + (c.ReturnAfternoon ? ", 13.00" : "") : null;
    }
    public static string ReachText(LvReq r)
    {
        string t = r.Reach == "urgent" ? "Yalnız acil durumda telefonla ulaşılabilir" : r.Reach == "mail" ? "Günde bir kez e-postalarına bakar" : "Ulaşılamaz";
        return t + (r.Reach == "urgent" && !string.IsNullOrEmpty(r.Phone) ? " (" + r.Phone + ")" : "");
    }

    // ================= e-postalar =================
    static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }
    static string P(string t) { return "<p style=\"margin:0 0 14px;font-size:14.5px;line-height:1.65;color:#3A4A66\">" + t + "</p>"; }
    static string Rows(List<string[]> l)
    {
        StringBuilder b = new StringBuilder("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;margin:4px 0 18px;font-size:14px\">");
        foreach (string[] x in l) if (!string.IsNullOrEmpty(x[1])) b.Append("<tr><td style=\"padding:8px 12px 8px 0;color:#8A97AE;width:140px;vertical-align:top;border-bottom:1px solid #EEF1F6\">" + x[0] + "</td><td style=\"padding:8px 0;vertical-align:top;border-bottom:1px solid #EEF1F6;color:#1B2440\">" + x[1] + "</td></tr>");
        return b.Append("</table>").ToString();
    }
    static string Box(string t, string tone)
    {
        string col = tone == "bad" ? "#C42B55" : tone == "ok" ? "#16895B" : "#9A5B00", bg = tone == "bad" ? "#FBE6EC" : tone == "ok" ? "#E3F5EC" : "#FCF1DC";
        return "<div style=\"background:" + bg + ";color:" + col + ";border-radius:10px;padding:12px 14px;font-size:14px;line-height:1.55;margin:0 0 16px\">" + t + "</div>";
    }
    static string Btn(string label, string url, bool primary)
    {
        return "<a href=\"" + E(url) + "\" style=\"display:inline-block;background:" + (primary ? "#4F46E5" : "#FFFFFF") + ";color:" + (primary ? "#FFFFFF" : "#1B2440") + ";border:1px solid " + (primary ? "#4F46E5" : "#D5DCE8") +
               ";text-decoration:none;font-weight:700;font-size:14px;padding:11px 18px;border-radius:10px;margin:0 8px 8px 0\">" + label + "</a>";
    }
    static string First(LvStaff s) { return s == null ? "" : s.Name.Split(' ')[0]; }
    static string Nm(Dictionary<int, LvStaff> m, int? id) { LvStaff s; return id.HasValue && m.TryGetValue(id.Value, out s) ? s.Name : ""; }
    static string Summary(LvReq r, Dictionary<int, LvStaff> m, bool bal, bool note, bool hand, bool reach, bool file)
    {
        var hol = Holidays(); LvStaff p = m.ContainsKey(r.UserId) ? m[r.UserId] : null;
        List<string[]> l = new List<string[]>();
        l.Add(new[] { "Talep no", E(r.No) }); l.Add(new[] { "Talep eden", E(p != null ? p.Name + (string.IsNullOrEmpty(p.Title) ? "" : ", " + p.Title) : "") }); l.Add(new[] { "İzin türü", E(TypeText(r)) });
        l.Add(new[] { "Tarihler", E(RangeText(r)) }); l.Add(new[] { "Süre", "<b>" + E(DurText(r)) + "</b>" }); l.Add(new[] { "İşe dönüş", E(ReturnText(r, hol)) });
        l.Add(new[] { "Vekil", E(Nm(m, r.Delegate)) }); l.Add(new[] { "Nöbet devri", E(Nm(m, r.OnCallTo)) });
        if (reach) l.Add(new[] { "İzindeyken", E(ReachText(r)) });
        if (bal && r.Type == "yillik" && p != null) { decimal b = Balance(p); l.Add(new[] { "Yıllık izin", "Kalan " + Num(b) + " gün, onaylanırsa <b>" + Num(b - r.Days) + " gün</b>" }); }
        if (note) l.Add(new[] { "Açıklama", E(r.Note) });
        if (hand) l.Add(new[] { "Devir notu", E(r.Handover) });
        if (file && !string.IsNullOrEmpty(r.FileOriginal)) l.Add(new[] { "Belge", E(r.FileOriginal) + " (portalda)" });
        return Rows(l);
    }
    static string Warn(LvReq r)
    {
        if (string.IsNullOrEmpty(r.Warnings)) return "";
        List<string> l = new List<string>(); foreach (string x in r.Warnings.Split('\n')) { int i = x.IndexOf('|'); if (i > 0) l.Add(E(x.Substring(i + 1))); }
        return l.Count > 0 ? Box(string.Join("<br>", l.ToArray()), "warn") : "";
    }
    static string Url(string baseUrl, string q) { return (baseUrl ?? "").TrimEnd('/') + "/Leaves.aspx" + q; }

    /// <summary>Bildirim e-postasi. Alicinin e-postasi yoksa sessizce atlanir; gonderim hatasi akisi durdurmaz.</summary>
    public static void Mail(string type, LvReq r, Dictionary<int, LvStaff> m, LvSettings st, string baseUrl)
    {
        try
        {
            LvStaff p = m.ContainsKey(r.UserId) ? m[r.UserId] : null; if (p == null) return;
            LvStep s = r.Current; List<int> to = new List<int>(), cc = new List<int>(); string sj = "", title = "", body = "";
            string inbox = Url(baseUrl, "?tab=inbox&r=" + r.Id), approveUrl = Url(baseUrl, "?tab=inbox&r=" + r.Id + "&do=approve"), mine = Url(baseUrl, "?tab=mine&r=" + r.Id);
            switch (type)
            {
                case "submitted":
                    to.Add(r.UserId); sj = "İzin talebiniz alındı · " + r.No; title = "İzin talebiniz alındı";
                    body = P("Merhaba " + E(First(p)) + ",") + P(s != null ? "Talebiniz <b>" + E(Nm(m, s.Approver)) + "</b> onayına gönderildi. Her adımda e-postayla bilgilendirileceksiniz." : "Talebiniz kaydedildi.") + Summary(r, m, false, false, false, true, false) + Btn("Talebimi görüntüle", mine, false);
                    break;
                case "approve":
                    if (s == null) return; to.Add(s.Approver);
                    sj = "Onayınız bekleniyor: " + p.Name + ", " + TypeName(r.Type).ToLower(TR) + " (" + DurText(r) + ")"; title = "Onayınız bekleniyor";
                    body = P("Merhaba " + E(First(m.ContainsKey(s.Approver) ? m[s.Approver] : null)) + ",") +
                           P("<b>" + E(p.Name) + "</b> izin talebi gönderdi" + (s.StepNo > 0 ? " ve " + E(Nm(m, r.Steps[s.StepNo - 1].DecidedBy ?? r.Steps[s.StepNo - 1].Approver)) + " tarafından onaylandı" : "") + "." + (s.Why != null ? " İkinci onay gerekçesi: " + E(s.Why) + "." : "")) +
                           (s.Original.HasValue ? Box(E(Nm(m, s.Original)) + " izinde olduğu için bu talep vekil onaylayıcı olarak size iletildi.", "warn") : "") +
                           Summary(r, m, true, true, true, true, true) + Warn(r) + Btn("Onayla", approveUrl, true) + Btn("Portalda incele", inbox, false) +
                           "<p style=\"margin:10px 0 0;font-size:12.5px;color:#8A97AE;line-height:1.55\">Onay ve reddetme portalda yapılır; giriş yapmanız gerekir. Reddetme ve düzeltme isteme için gerekçe yazılır. " + st.Remind + " saat içinde yanıt verilmezse hatırlatma, " + st.Escalate + " saat sonra yöneticinize yükseltme yapılır.</p>";
                    break;
                case "remind":
                    if (s == null) return; to.Add(s.Approver); sj = "Hatırlatma: " + p.Name + " kişisinin izin talebi onayınızı bekliyor"; title = "İzin talebi onayınızı bekliyor";
                    body = P("<b>" + E(p.Name) + "</b> kişisinin " + E(r.Start.ToString("d MMMM", TR)) + " tarihinde başlayan izin talebi " + st.Remind + " saattir yanıt bekliyor. " + st.Escalate + " saat dolduğunda talep yöneticinize yükseltilecek.") + Summary(r, m, false, false, false, false, false) + Btn("Onayla", approveUrl, true) + Btn("Portalda incele", inbox, false);
                    break;
                case "escalate":
                    if (s == null || !r.EscalatedTo.HasValue) return; to.Add(r.EscalatedTo.Value); cc.Add(s.Approver);
                    sj = "Yükseltildi: " + p.Name + " kişisinin izin talebi " + st.Escalate + " saattir yanıtsız"; title = "Yanıtsız izin talebi size yükseltildi";
                    body = P("<b>" + E(p.Name) + "</b> kişisinin izin talebi " + st.Escalate + " saattir <b>" + E(Nm(m, s.Approver)) + "</b> onayını bekliyor. Talebi siz de onaylayabilir ya da reddedebilirsiniz; " + E(Nm(m, s.Approver)) + " bilgide.") + Summary(r, m, false, true, false, false, false) + Btn("Onayla", approveUrl, true) + Btn("Portalda incele", inbox, false);
                    break;
                case "approved":
                    to.Add(r.UserId); sj = "İzin talebiniz onaylandı · " + RangeShort(r); title = "İzin talebiniz onaylandı";
                    List<string> by = new List<string>(); foreach (LvStep x in r.Steps) by.Add(Nm(m, x.DecidedBy ?? x.Approver));
                    body = Box((by.Count > 0 ? E(string.Join(" ve ", by.ToArray())) + " onayladı. " : "") + "İyi dinlenmeler!", "ok") + Summary(r, m, false, false, false, false, false) +
                           (r.Type == "yillik" ? P("Kalan yıllık izniniz: <b>" + Num(Balance(p)) + " gün</b>.") : "") + (r.Ooo ? P("İzin başlamadan önceki iş günü size otomatik yanıt hatırlatması göndereceğiz.") : "");
                    break;
                case "cover":
                    if (!r.Delegate.HasValue) return; to.Add(r.Delegate.Value); cc.Add(r.UserId); sj = p.Name + " izinde, vekili sizsiniz · " + RangeShort(r); title = p.Name + " izinliyken vekili sizsiniz";
                    body = P("<b>" + E(p.Name) + "</b> aşağıdaki tarihlerde izinli olacak ve işlerinin takibi size devredildi.") + Summary(r, m, false, false, true, true, false) + (r.OnCallTo == r.Delegate ? Box("Bu tarihlerdeki nöbet de size devredildi.", "warn") : "");
                    if (r.OnCallTo.HasValue && r.OnCallTo != r.Delegate) cc.Add(r.OnCallTo.Value);
                    break;
                case "hr":
                    if (!st.Hr.HasValue) return; to.Add(st.Hr.Value); sj = "Onaylanan izin: " + p.Name + ", " + TypeName(r.Type).ToLower(TR) + ", " + DurText(r); title = "Onaylanan izin kaydı";
                    List<string> path = new List<string>(); foreach (LvStep x in r.Steps) path.Add(Nm(m, x.DecidedBy ?? x.Approver) + (x.DecidedAt.HasValue ? " (" + x.DecidedAt.Value.ToTrLocal().ToString("d MMMM HH:mm", TR) + ")" : ""));
                    body = P("Aşağıdaki izin onaylandı. İzin cetveline işleyip bordro döneminde dikkate alınız.") + Summary(r, m, false, false, false, false, true) + Rows(new List<string[]> { new[] { "Onay yolu", E(path.Count > 0 ? string.Join(", ", path.ToArray()) : "Onay gerekmedi") } });
                    break;
                case "rejected":
                case "revise":
                    to.Add(r.UserId); LvStep d = null; foreach (LvStep x in r.Steps) if (x.Decision == type.Replace("rejected", "reject")) d = x;
                    string who = d != null ? Nm(m, d.DecidedBy) : "Onaylayıcı";
                    if (type == "rejected") { sj = "İzin talebiniz onaylanmadı · " + r.No; title = "İzin talebiniz onaylanmadı"; body = Box("<b>" + E(who) + ":</b> “" + E(r.Reason) + "”", "bad") + Summary(r, m, false, false, false, false, false) + P("Farklı tarihler için yeni bir talep oluşturabilir ya da yöneticinizle görüşebilirsiniz."); }
                    else { sj = "İzin talebiniz için düzeltme istendi · " + r.No; title = "Talebiniz için düzeltme istendi"; body = Box("<b>" + E(who) + ":</b> “" + E(r.Reason) + "”", "warn") + Summary(r, m, false, false, false, false, false) + P("Talebinizi güncelleyip yeniden gönderdiğinizde onay yolu baştan başlar.") + Btn("Talebi güncelle", mine, true); }
                    break;
                case "cancelled":
                    if (s == null) return; to.Add(s.Approver); if (r.EscalatedTo.HasValue) cc.Add(r.EscalatedTo.Value);
                    sj = "Geri çekildi: " + p.Name + " kişisinin izin talebi"; title = "İzin talebi geri çekildi";
                    body = P("<b>" + E(p.Name) + "</b> onayınızı bekleyen izin talebini geri çekti. Sizden bir işlem beklenmiyor.") + Summary(r, m, false, false, false, false, false);
                    break;
                case "before":
                    to.Add(r.UserId); sj = "Yarın izindesiniz: başlamadan önce kontrol listesi"; title = "Yarın izindesiniz";
                    LvStaff v = r.Delegate.HasValue && m.ContainsKey(r.Delegate.Value) ? m[r.Delegate.Value] : null; string ret = ReturnText(r, Holidays());
                    string ooo = "Merhaba, " + r.Start.ToString("d MMMM", TR) + (r.Start == r.End ? "" : " ile " + r.End.ToString("d MMMM", TR)) + " tarihleri arasında izindeyim ve e-postalarıma erişimim kısıtlı olacak. " +
                                 (v != null ? "Acil konular için " + v.Name + " (" + v.Email + ") ile iletişime geçebilirsiniz. " : "") + (ret != null ? ret.Split(',')[0] + " tarihinde döndüğümde size yanıt vereceğim." : "");
                    body = P("Merhaba " + E(First(p)) + ", izniniz " + E(r.Start.ToString("d MMMM yyyy dddd", TR)) + " başlıyor. Çıkmadan önce:") +
                           "<ul style=\"margin:0 0 16px;padding-left:20px;font-size:14px;line-height:1.8;color:#3A4A66\"><li>Outlook otomatik yanıtını açın (önerilen metin aşağıda)</li><li>Teams durumunuzu “İzinde” olarak ayarlayın</li><li>Açık talep, PR ve görevleri " + (v != null ? E(v.Name) + " kişisine" : "vekilinize") + " devredin</li>" +
                           (r.OnCallTo.HasValue ? "<li>Nöbet devrini " + E(Nm(m, r.OnCallTo)) + " ile teyit edin</li>" : "") + "<li>Takviminizdeki toplantıları iptal edin ya da yönlendirin</li></ul>" +
                           "<div style=\"background:#F4F6FB;border:1px solid #E3E8F0;border-radius:10px;padding:12px 14px;font-size:14px;line-height:1.6;margin:0 0 16px;color:#1B2440\"><b style=\"display:block;margin-bottom:4px\">Önerilen otomatik yanıt</b>" + E(ooo) + "</div>";
                    break;
                case "sickAck":
                    to.Add(r.UserId); sj = "Rapor bildiriminiz alındı · " + r.No; title = "Rapor bildiriminiz alındı";
                    body = P("Geçmiş olsun " + E(First(p)) + ". Raporunuz kaydedildi; yöneticiniz ve İK bilgilendirildi. Sizden ek bir işlem beklenmiyor.") + Summary(r, m, false, false, false, false, true) + P("Rapor süresi uzarsa yeni raporu aynı sayfadan yükleyin.");
                    break;
                case "sickInfo":
                    if (p.Approver.HasValue) { to.Add(p.Approver.Value); if (st.Hr.HasValue) cc.Add(st.Hr.Value); } else if (st.Hr.HasValue) to.Add(st.Hr.Value); else return;
                    sj = "Bilgi: " + p.Name + " raporlu, " + RangeShort(r); title = p.Name + " raporlu";
                    body = P("Onay gerektirmeyen bilgilendirme. Rapor süresince ekip planlamasını buna göre yapınız.") + Summary(r, m, false, false, false, false, true) + P("Rapor belgesi yalnız İK ve yönetici tarafından portalda görüntülenebilir.");
                    break;
                default: return;
            }
            List<string> toE = new List<string>(), ccE = new List<string>();
            foreach (int i in to) if (m.ContainsKey(i) && !string.IsNullOrEmpty(m[i].Email) && !toE.Contains(m[i].Email)) toE.Add(m[i].Email);
            foreach (int i in cc) if (m.ContainsKey(i) && !string.IsNullOrEmpty(m[i].Email) && !toE.Contains(m[i].Email) && !ccE.Contains(m[i].Email)) ccE.Add(m[i].Email);
            if (toE.Count == 0) return;
            string err; string html = Emails.Shell(title, body);
            if (ccE.Count > 0) Mailer.SendToCc(string.Join(",", toE.ToArray()), string.Join(",", ccE.ToArray()), sj, html, out err);
            else Mailer.SendTo(string.Join(",", toE.ToArray()), sj, html, out err);
        }
        catch { }
    }
}
