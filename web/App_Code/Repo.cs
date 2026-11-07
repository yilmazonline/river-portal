using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

// Tum veri erisimi burada (ADO.NET). Tenant izolasyonu sorgulardaki CompanyId ile saglanir.
public static class Repo
{
    // ---- Kullanicilar ----
    /// <summary>Kalici oturum geri yuklemesi icin kullaniciyi kimligiyle okur</summary>
    public static UserAccount GetAccountById(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT UserId, CompanyId, FullName, Email, PasswordHash, Role, Permissions, PreferredCulture, IsActive FROM dbo.Users WHERE UserId = @i", con))
        {
            cmd.Parameters.AddWithValue("@i", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                UserAccount u = new UserAccount();
                u.UserId = (int)r["UserId"]; u.CompanyId = (int)r["CompanyId"]; u.FullName = r["FullName"] as string; u.Email = r["Email"] as string;
                u.PasswordHash = r["PasswordHash"] as string; u.Role = r["Role"] as string; u.Permissions = r["Permissions"] == DBNull.Value ? "" : (string)r["Permissions"];
                u.PreferredCulture = r["PreferredCulture"] == DBNull.Value ? null : (string)r["PreferredCulture"]; u.IsActive = Convert.ToBoolean(r["IsActive"]);
                return u;
            }
        }
    }

    public static UserAccount GetUserByEmail(string email)
    {
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT UserId, CompanyId, FullName, Email, PasswordHash, Role, Permissions, PreferredCulture, IsActive " +
                "FROM dbo.Users WHERE Email = @e", con))
            {
                cmd.Parameters.AddWithValue("@e", email);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    UserAccount u = new UserAccount();
                    u.UserId = (int)r["UserId"];
                    u.CompanyId = (int)r["CompanyId"];
                    u.FullName = r["FullName"] as string;
                    u.Email = r["Email"] as string;
                    u.PasswordHash = r["PasswordHash"] as string;
                    u.Role = r["Role"] as string;
                    u.Permissions = r["Permissions"] == DBNull.Value ? "" : (string)r["Permissions"];
                    u.PreferredCulture = r["PreferredCulture"] == DBNull.Value ? null : (string)r["PreferredCulture"];
                    u.IsActive = (bool)r["IsActive"];
                    return u;
                }
            }
        }
    }

    public static void UpdateLastLogin(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.Users SET LastLoginAt = SYSUTCDATETIME() WHERE UserId = @u", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.ExecuteNonQuery();
        }
    }

    // ---- Firma ----
    public static CompanyInfo GetCompany(int companyId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT CompanyId, Name, IsVendor, CurrentTierId, AnnualNetRevenue, IsPassive " +
            "FROM dbo.Companies WHERE CompanyId = @c", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                CompanyInfo c = new CompanyInfo();
                c.CompanyId = (int)r["CompanyId"];
                c.Name = r["Name"] as string;
                c.IsVendor = (bool)r["IsVendor"];
                c.CurrentTierId = r["CurrentTierId"] == DBNull.Value ? (int?)null : (int)r["CurrentTierId"];
                c.AnnualNetRevenue = (decimal)r["AnnualNetRevenue"];
                c.IsPassive = (bool)r["IsPassive"];
                return c;
            }
        }
    }

    // ---- Kademeler ----
    public static List<Tier> GetTiers()
    {
        List<Tier> list = new List<Tier>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TierId, Code, NameTr, NameEn, MinRevenue, MaxRevenue, BaseCommission, SlaTr, SlaEn, SortOrder " +
            "FROM dbo.PartnerTiers ORDER BY SortOrder", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                Tier t = new Tier();
                t.TierId = (int)r["TierId"];
                t.Code = r["Code"] as string;
                t.NameTr = r["NameTr"] as string;
                t.NameEn = r["NameEn"] as string;
                t.MinRevenue = (decimal)r["MinRevenue"];
                t.MaxRevenue = r["MaxRevenue"] == DBNull.Value ? (decimal?)null : (decimal)r["MaxRevenue"];
                t.BaseCommission = (decimal)r["BaseCommission"];
                t.SlaTr = r["SlaTr"] as string;
                t.SlaEn = r["SlaEn"] as string;
                t.SortOrder = (int)r["SortOrder"];
                list.Add(t);
            }
        }
        return list;
    }

    // ---- Anlasmalar ----
    public static List<DealRow> GetDeals(int companyId, int top)
    {
        List<DealRow> list = new List<DealRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP (" + top + ") CustomerName, Amount, EffectiveRate, CommissionAmount, Status " +
            "FROM dbo.Deals WHERE CompanyId = @c ORDER BY BookedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    DealRow d = new DealRow();
                    d.CustomerName = r["CustomerName"] as string;
                    d.Amount = (decimal)r["Amount"];
                    d.EffectiveRate = (decimal)r["EffectiveRate"];
                    d.CommissionAmount = (decimal)r["CommissionAmount"];
                    d.Status = r["Status"] as string;
                    list.Add(d);
                }
            }
        }
        return list;
    }

    // ---- Demolar (partner) ----
    public static List<DemoRow> GetAllDemos(int companyId)
    {
        List<DemoRow> list = new List<DemoRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT DemoRequestId, CompanyId, ClientName, ProductModule, Status, " +
            "PreferredDate, AltDate1, AltDate2, ScheduledDate " +
            "FROM dbo.DemoRequests WHERE CompanyId = @c ORDER BY CreatedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    DemoRow d = new DemoRow();
                    d.DemoRequestId = (int)r["DemoRequestId"];
                    d.CompanyId = (int)r["CompanyId"];
                    d.ClientName = r["ClientName"] as string;
                    d.ProductModule = r["ProductModule"] as string;
                    d.Status = r["Status"] as string;
                    d.PreferredDate = r["PreferredDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["PreferredDate"];
                    d.AltDate1 = r["AltDate1"] == DBNull.Value ? (DateTime?)null : (DateTime)r["AltDate1"];
                    d.AltDate2 = r["AltDate2"] == DBNull.Value ? (DateTime?)null : (DateTime)r["AltDate2"];
                    d.ScheduledDate = r["ScheduledDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ScheduledDate"];
                    list.Add(d);
                }
            }
        }
        return list;
    }

    // ---- Demolar (partner) - dashboard ozeti ----
    public static List<DemoRow> GetDemos(int companyId, int top)
    {
        List<DemoRow> list = new List<DemoRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP (" + top + ") DemoRequestId, CompanyId, ClientName, ProductModule, Status " +
            "FROM dbo.DemoRequests WHERE CompanyId = @c ORDER BY CreatedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(ReadDemo(r));
            }
        }
        return list;
    }

    // ---- Bekleyen demolar (vendor kuyrugu) ----
    public static List<DemoRow> GetPendingDemos()
    {
        List<DemoRow> list = new List<DemoRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT DemoRequestId, CompanyId, ClientName, ProductModule, Status " +
            "FROM dbo.DemoRequests WHERE Status = N'Pending' ORDER BY CreatedAt ASC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read()) list.Add(ReadDemo(r));
        }
        return list;
    }

    private static DemoRow ReadDemo(SqlDataReader r)
    {
        DemoRow d = new DemoRow();
        d.DemoRequestId = (int)r["DemoRequestId"];
        d.CompanyId = (int)r["CompanyId"];
        d.ClientName = r["ClientName"] as string;
        d.ProductModule = r["ProductModule"] as string;
        d.Status = r["Status"] as string;
        return d;
    }

    // ---- Bildirimler ----
    public static List<NotifRow> GetNotifications(int companyId, int top)
    {
        return GetNotifications(companyId, top, null);
    }

    // allowedTypes null ise tüm tipler; doluysa yalnız o tipler (rol bazlı gizleme)
    public static List<NotifRow> GetNotifications(int companyId, int top, string[] allowedTypes)
    {
        return GetNotifications(companyId, 0, top, allowedTypes);
    }

    // userId > 0: firma geneli (UserId NULL) + yalniz bu kullaniciya ozel bildirimler.
    // UserId / Link kolonu yoksa (migration calismamis) eski davranisa geri duser.
    public static List<NotifRow> GetNotifications(int companyId, int userId, int top, string[] allowedTypes)
    {
        List<NotifRow> list = new List<NotifRow>();
        string typeClause = "";
        if (allowedTypes != null && allowedTypes.Length > 0)
        {
            System.Text.StringBuilder tb = new System.Text.StringBuilder();
            for (int i = 0; i < allowedTypes.Length; i++) { if (i > 0) tb.Append(","); tb.Append("@t" + i); }
            typeClause = " AND Type IN (" + tb.ToString() + ")";
        }
        // deneme sirasi: (kullanici filtresi + link) -> (link) -> (linksiz)
        for (int attempt = 0; attempt < 3; attempt++)
        {
            bool withUser = attempt == 0 && userId > 0;
            bool withLink = attempt < 2;
            if (attempt == 0 && userId <= 0) continue;
            list.Clear();
            string cols = withLink ? "NotificationId, TitleTr, TitleEn, BodyTr, BodyEn, IsRead, Link, CreatedAt, Type" : "NotificationId, TitleTr, TitleEn, BodyTr, BodyEn, IsRead, CreatedAt, Type";
            string userClause = withUser ? " AND (UserId IS NULL OR UserId = @u)" : "";
            try
            {
                using (SqlConnection con = Db.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP (" + top + ") " + cols +
                    " FROM dbo.Notifications WHERE CompanyId = @c" + userClause + typeClause + " ORDER BY CreatedAt DESC", con))
                {
                    cmd.Parameters.AddWithValue("@c", companyId);
                    if (withUser) cmd.Parameters.AddWithValue("@u", userId);
                    if (allowedTypes != null) for (int i = 0; i < allowedTypes.Length; i++) cmd.Parameters.AddWithValue("@t" + i, allowedTypes[i]);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            NotifRow n = new NotifRow();
                            n.NotificationId = (int)r["NotificationId"];
                            n.TitleTr = r["TitleTr"] as string;
                            n.TitleEn = r["TitleEn"] as string;
                            n.BodyTr = r["BodyTr"] == DBNull.Value ? null : (string)r["BodyTr"];
                            n.BodyEn = r["BodyEn"] == DBNull.Value ? null : (string)r["BodyEn"];
                            n.IsRead = (bool)r["IsRead"];
                            if (withLink) n.Link = r["Link"] == DBNull.Value ? "" : (string)r["Link"];
                            n.CreatedAt = r["CreatedAt"] as DateTime?;
                            n.Type = r["Type"] as string;
                            list.Add(n);
                        }
                    }
                }
                return list;
            }
            catch { continue; }
        }
        return list;
    }

    public static void MarkNotificationRead(int notificationId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Notifications SET IsRead = 1 WHERE NotificationId = @id", con))
        { cmd.Parameters.AddWithValue("@id", notificationId); cmd.ExecuteNonQuery(); }
    }

    public static void MarkAllRead(int companyId)
    {
        MarkAllRead(companyId, null);
    }

    public static void MarkAllRead(int companyId, string[] allowedTypes)
    {
        string typeClause = "";
        if (allowedTypes != null && allowedTypes.Length > 0)
        {
            System.Text.StringBuilder tb = new System.Text.StringBuilder();
            for (int i = 0; i < allowedTypes.Length; i++) { if (i > 0) tb.Append(","); tb.Append("@t" + i); }
            typeClause = " AND Type IN (" + tb.ToString() + ")";
        }
        int uid = CurrentUserIdSafe();
        if (uid > 0)
        {
            try
            {
                using (SqlConnection con = Db.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE dbo.Notifications SET IsRead = 1, ReadAt = SYSUTCDATETIME() " +
                    "WHERE CompanyId = @c AND IsRead = 0 AND (UserId IS NULL OR UserId = @u)" + typeClause, con))
                {
                    cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", uid);
                    if (allowedTypes != null) for (int i = 0; i < allowedTypes.Length; i++) cmd.Parameters.AddWithValue("@t" + i, allowedTypes[i]);
                    cmd.ExecuteNonQuery();
                }
                return;
            }
            catch { }
        }
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.Notifications SET IsRead = 1, ReadAt = SYSUTCDATETIME() " +
            "WHERE CompanyId = @c AND IsRead = 0" + typeClause, con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            if (allowedTypes != null) for (int i = 0; i < allowedTypes.Length; i++) cmd.Parameters.AddWithValue("@t" + i, allowedTypes[i]);
            cmd.ExecuteNonQuery();
        }
    }

    private static int CurrentUserIdSafe()
    {
        try { return Auth.IsLoggedIn ? Auth.UserId : 0; } catch { return 0; }
    }

    // Yalniz tek bir kullaniciya gorunen uygulama ici bildirim
    public static void AddUserNotification(int companyId, int userId, string titleTr, string titleEn, string bodyTr, string bodyEn, string type, string link)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.Notifications (CompanyId, UserId, TitleTr, TitleEn, BodyTr, BodyEn, Type, Link) VALUES (@c, @u, @tt, @te, @bt, @be, @ty, @l)", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId); cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@tt", titleTr ?? ""); cmd.Parameters.AddWithValue("@te", titleEn ?? "");
            cmd.Parameters.AddWithValue("@bt", (object)bodyTr ?? DBNull.Value); cmd.Parameters.AddWithValue("@be", (object)bodyEn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ty", type ?? "System"); cmd.Parameters.AddWithValue("@l", (object)link ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }

    // ---- Siparisler: tum anlasmalar + duzenle/sil/durum ----
    public static List<DealRow> GetAllDeals(int companyId)
    {
        List<DealRow> list = new List<DealRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT DealId, CompanyId, Title, CustomerName, Amount, EffectiveRate, CommissionAmount, Status, BookedAt " +
            "FROM dbo.Deals WHERE CompanyId = @c ORDER BY BookedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(ReadDeal(r));
            }
        }
        return list;
    }

    public static DealRow GetDealById(int dealId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT DealId, CompanyId, Title, CustomerName, Amount, EffectiveRate, CommissionAmount, Status, BookedAt " +
            "FROM dbo.Deals WHERE DealId = @d", con))
        {
            cmd.Parameters.AddWithValue("@d", dealId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                return ReadDeal(r);
            }
        }
    }

    private static DealRow ReadDeal(SqlDataReader r)
    {
        DealRow d = new DealRow();
        d.DealId = (int)r["DealId"];
        d.CompanyId = (int)r["CompanyId"];
        d.Title = r["Title"] as string;
        d.CustomerName = r["CustomerName"] as string;
        d.Amount = (decimal)r["Amount"];
        d.EffectiveRate = (decimal)r["EffectiveRate"];
        d.CommissionAmount = (decimal)r["CommissionAmount"];
        d.Status = r["Status"] as string;
        d.BookedAt = (DateTime)r["BookedAt"];
        return d;
    }

    public static void UpdateDealStatus(int dealId, string status)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Deals SET Status = @s WHERE DealId = @d", con))
        {
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@d", dealId);
            cmd.ExecuteNonQuery();
        }
    }

    public static void UpdateDeal(int dealId, int companyId, string customer, string title, decimal amount,
                                  DealCalc calc, string status, decimal newRevenue, int newTierId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE dbo.Deals SET CustomerName=@cust, Title=@t, Amount=@amt, AppliedTierId=@tier, " +
                    "BaseCommissionRate=@base, MegaDealBonusRate=@mega, CommissionAmount=@comm, Status=@st " +
                    "WHERE DealId=@d", con, tx))
                {
                    cmd.Parameters.AddWithValue("@cust", (object)customer ?? "");
                    cmd.Parameters.AddWithValue("@t", (object)title ?? "");
                    cmd.Parameters.AddWithValue("@amt", amount);
                    cmd.Parameters.AddWithValue("@tier", calc.TierId);
                    cmd.Parameters.AddWithValue("@base", calc.BaseRate);
                    cmd.Parameters.AddWithValue("@mega", calc.MegaRate);
                    cmd.Parameters.AddWithValue("@comm", calc.Commission);
                    cmd.Parameters.AddWithValue("@st", status);
                    cmd.Parameters.AddWithValue("@d", dealId);
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd2 = new SqlCommand(
                    "UPDATE dbo.Companies SET AnnualNetRevenue=@rev, CurrentTierId=@tier WHERE CompanyId=@c", con, tx))
                {
                    cmd2.Parameters.AddWithValue("@rev", newRevenue);
                    cmd2.Parameters.AddWithValue("@tier", newTierId);
                    cmd2.Parameters.AddWithValue("@c", companyId);
                    cmd2.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch { tx.Rollback(); throw; }
        }
    }

    public static void DeleteDeal(int dealId, int companyId, decimal newRevenue, int newTierId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.Deals WHERE DealId=@d", con, tx))
                {
                    cmd.Parameters.AddWithValue("@d", dealId);
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd2 = new SqlCommand(
                    "UPDATE dbo.Companies SET AnnualNetRevenue=@rev, CurrentTierId=@tier WHERE CompanyId=@c", con, tx))
                {
                    cmd2.Parameters.AddWithValue("@rev", newRevenue);
                    cmd2.Parameters.AddWithValue("@tier", newTierId);
                    cmd2.Parameters.AddWithValue("@c", companyId);
                    cmd2.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch { tx.Rollback(); throw; }
        }
    }

    // ---- Vendor: partner adina anlasma kaydi (hakedis TierEngine ile hesaplanir) ----
    public static void BookDeal(int companyId, string title, string customer, decimal amount,
                               DealCalc calc, decimal newRevenue, int newTierId, int? createdByUserId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlTransaction tx = con.BeginTransaction())
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO dbo.Deals (CompanyId, Title, CustomerName, Amount, AppliedTierId, " +
                    "BaseCommissionRate, MegaDealBonusRate, CommissionAmount, Status, CreatedByUserId, CreatedByVendor) " +
                    "VALUES (@c, @t, @cust, @amt, @tier, @base, @mega, @comm, N'Approved', @uid, 1)", con, tx))
                {
                    cmd.Parameters.AddWithValue("@c", companyId);
                    cmd.Parameters.AddWithValue("@t", (object)title ?? "");
                    cmd.Parameters.AddWithValue("@cust", (object)customer ?? "");
                    cmd.Parameters.AddWithValue("@amt", amount);
                    cmd.Parameters.AddWithValue("@tier", calc.TierId);
                    cmd.Parameters.AddWithValue("@base", calc.BaseRate);
                    cmd.Parameters.AddWithValue("@mega", calc.MegaRate);
                    cmd.Parameters.AddWithValue("@comm", calc.Commission);
                    cmd.Parameters.AddWithValue("@uid", (object)createdByUserId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd2 = new SqlCommand(
                    "UPDATE dbo.Companies SET AnnualNetRevenue = @rev, CurrentTierId = @tier, " +
                    "LastSaleDate = SYSUTCDATETIME(), IsPassive = 0 WHERE CompanyId = @c", con, tx))
                {
                    cmd2.Parameters.AddWithValue("@rev", newRevenue);
                    cmd2.Parameters.AddWithValue("@tier", newTierId);
                    cmd2.Parameters.AddWithValue("@c", companyId);
                    cmd2.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }

    // ---- Partner: demo talebi (tercih + 2 alternatif tarih/saat) ----
    public static int SubmitDemo(int companyId, int? userId, string client, string module,
                                 DateTime? preferred, DateTime? alt1, DateTime? alt2)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.DemoRequests (CompanyId, RequestedByUserId, ClientName, ProductModule, " +
            "PreferredDate, AltDate1, AltDate2, Status) " +
            "OUTPUT INSERTED.DemoRequestId " +
            "VALUES (@c, @u, @client, @mod, @p, @a1, @a2, N'Pending')", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            cmd.Parameters.AddWithValue("@u", (object)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@client", (object)client ?? "");
            cmd.Parameters.AddWithValue("@mod", (object)module ?? "");
            cmd.Parameters.AddWithValue("@p", (object)preferred ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@a1", (object)alt1 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@a2", (object)alt2 ?? DBNull.Value);
            object id = cmd.ExecuteScalar();
            return id != null ? (int)id : 0;
        }
    }

    // ---- Admin: tüm demolar (tüm firmalar, tüm durumlar, geçmiş dahil) ----
    public static List<DemoRow> GetAllDemosAllCompanies()
    {
        List<DemoRow> list = new List<DemoRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT d.DemoRequestId, d.CompanyId, d.ClientName, d.ProductModule, d.Status, " +
            "d.PreferredDate, d.AltDate1, d.AltDate2, d.ScheduledDate, c.Name AS CompanyName " +
            "FROM dbo.DemoRequests d JOIN dbo.Companies c ON c.CompanyId = d.CompanyId " +
            "ORDER BY d.CreatedAt DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                DemoRow d = new DemoRow();
                d.DemoRequestId = (int)r["DemoRequestId"];
                d.CompanyId = (int)r["CompanyId"];
                d.CompanyName = r["CompanyName"] as string;
                d.ClientName = r["ClientName"] as string;
                d.ProductModule = r["ProductModule"] as string;
                d.Status = r["Status"] as string;
                d.PreferredDate = r["PreferredDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["PreferredDate"];
                d.AltDate1 = r["AltDate1"] == DBNull.Value ? (DateTime?)null : (DateTime)r["AltDate1"];
                d.AltDate2 = r["AltDate2"] == DBNull.Value ? (DateTime?)null : (DateTime)r["AltDate2"];
                d.ScheduledDate = r["ScheduledDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ScheduledDate"];
                list.Add(d);
            }
        }
        return list;
    }

    // ---- Admin: kullanıcı yönetimi + şifre yenileme ----
    public static List<UserRow> GetAllUsers()
    {
        List<UserRow> list = new List<UserRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT u.UserId, u.CompanyId, u.FullName, u.Email, u.Role, u.Permissions, u.LastLoginAt, u.AvatarImage, c.Name AS CompanyName, c.CompanyType " +
            "FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId WHERE u.IsActive = 1 " +
            "ORDER BY c.IsVendor DESC, c.Name ASC, u.FullName ASC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read()) list.Add(ReadUserRow(r));
        }
        return list;
    }

    // Atama için: üretici firmasındaki destek/portal yetkili aktif kullanıcılar
    public static List<UserRow> GetSupportUsers()
    {
        List<UserRow> list = new List<UserRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT u.UserId, u.CompanyId, u.FullName, u.Email, u.Role, u.Permissions, u.LastLoginAt, u.AvatarImage, c.Name AS CompanyName, c.CompanyType " +
            "FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId " +
            "WHERE u.IsActive = 1 AND c.IsActive = 1 AND c.IsVendor = 1 AND (" +
            "u.Role IN ('VendorAdmin','SupportAgent') OR u.Permissions LIKE '%Support%' OR u.Permissions LIKE '%Portal%') " +
            "ORDER BY u.FullName ASC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read()) list.Add(ReadUserRow(r));
        }
        return list;
    }

    public static UserRow GetUserById(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT u.UserId, u.CompanyId, u.FullName, u.Email, u.Role, u.Permissions, u.LastLoginAt, u.AvatarImage, c.Name AS CompanyName, c.CompanyType " +
            "FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId WHERE u.UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                return ReadUserRow(r);
            }
        }
    }

    private static UserRow ReadUserRow(SqlDataReader r)
    {
        UserRow u = new UserRow();
        u.UserId = (int)r["UserId"];
        u.CompanyId = (int)r["CompanyId"];
        u.FullName = r["FullName"] as string;
        u.Email = r["Email"] as string;
        u.Role = r["Role"] as string;
        u.CompanyName = r["CompanyName"] as string;
        u.CompanyType = r["CompanyType"] as string;
        u.AvatarImage = r["AvatarImage"] as string;
        u.Permissions = r["Permissions"] == DBNull.Value ? "" : (string)r["Permissions"];
        u.LastLoginAt = r["LastLoginAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["LastLoginAt"];
        return u;
    }

    // Profil resmi (avatar)
    public static void SetUserAvatar(int userId, string path)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET AvatarImage = @p WHERE UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@p", string.IsNullOrEmpty(path) ? (object)DBNull.Value : path);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }
    }

    public static string GetUserAvatar(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT AvatarImage FROM dbo.Users WHERE UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", userId);
            object o = cmd.ExecuteScalar();
            return o == null || o == DBNull.Value ? "" : (string)o;
        }
    }

    // Uygulama turu: kullanici turu gordu mu / gordu isaretle
    public static bool IsTourSeen(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TourSeenAt FROM dbo.Users WHERE UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", userId);
            object o = cmd.ExecuteScalar();
            return o != null && o != DBNull.Value;
        }
    }

    public static void MarkTourSeen(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET TourSeenAt = SYSUTCDATETIME() WHERE UserId = @id AND TourSeenAt IS NULL", con))
        { cmd.Parameters.AddWithValue("@id", userId); cmd.ExecuteNonQuery(); }
    }

    // Karsilama videosu: her kullaniciya bir kez. Kolon yoksa (migration 35 calismamis) "gorulmus" sayilir -> her giriste acilmaz.
    public static bool IsIntroSeen(int userId)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT IntroSeenAt FROM dbo.Users WHERE UserId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                object o = cmd.ExecuteScalar();
                return o != null && o != DBNull.Value;
            }
        }
        catch { return true; }
    }

    public static void MarkIntroSeen(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET IntroSeenAt = SYSUTCDATETIME() WHERE UserId = @id AND IntroSeenAt IS NULL", con))
        { cmd.Parameters.AddWithValue("@id", userId); cmd.ExecuteNonQuery(); }
    }

    // Kullanicinin mevcut parola hash'i (self-servis sifre degistirme icin)
    public static string GetPasswordHash(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT PasswordHash FROM dbo.Users WHERE UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", userId);
            object o = cmd.ExecuteScalar();
            return o == null || o == DBNull.Value ? "" : (string)o;
        }
    }

    public static void SetResetToken(int userId, string token, DateTime expiresUtc)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET ResetToken = @t, ResetTokenExpires = @e WHERE UserId = @id", con))
        { cmd.Parameters.AddWithValue("@t", (object)token ?? DBNull.Value); cmd.Parameters.AddWithValue("@e", expiresUtc); cmd.Parameters.AddWithValue("@id", userId); cmd.ExecuteNonQuery(); }
    }

    public static void ClearResetToken(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET ResetToken = NULL, ResetTokenExpires = NULL WHERE UserId = @id", con))
        { cmd.Parameters.AddWithValue("@id", userId); cmd.ExecuteNonQuery(); }
    }

    // Gecerli (suresi dolmamis, aktif) jeton -> kullanici; yoksa null
    public static UserRow FindUserByResetToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        int uid = 0;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT UserId FROM dbo.Users WHERE ResetToken = @t AND ResetTokenExpires IS NOT NULL AND ResetTokenExpires > SYSUTCDATETIME() AND IsActive = 1", con))
        { cmd.Parameters.AddWithValue("@t", token); object o = cmd.ExecuteScalar(); if (o != null && o != DBNull.Value) uid = Convert.ToInt32(o); }
        return uid > 0 ? GetUserById(uid) : null;
    }

    public static void UpdateUserPassword(int userId, string passwordHash)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET PasswordHash = @h WHERE UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@h", (object)passwordHash ?? "");
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }
    }

    // Kullaniciyi pasiflestir (soft delete): giris engellenir, kayitlar korunur
    public static void DeactivateUser(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET IsActive = 0 WHERE UserId = @id", con))
        { cmd.Parameters.AddWithValue("@id", userId); cmd.ExecuteNonQuery(); }
    }

    // Kullanici ad + rol (yetki) guncelle
    public static void UpdateUser(int userId, string fullName, string role)
    {
        UpdateUser(userId, fullName, role, null, false);
    }

    public static void UpdateUser(int userId, string fullName, string role, string permissions, bool setPermissions)
    {
        using (SqlConnection con = Db.Open())
        {
            string sql = setPermissions
                ? "UPDATE dbo.Users SET FullName = @n, Role = @r, Permissions = @perm WHERE UserId = @id"
                : "UPDATE dbo.Users SET FullName = @n, Role = @r WHERE UserId = @id";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@n", (object)fullName ?? "");
                cmd.Parameters.AddWithValue("@r", (object)role ?? "");
                if (setPermissions) cmd.Parameters.AddWithValue("@perm", string.IsNullOrEmpty(permissions) ? (object)DBNull.Value : permissions);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Şifremi unuttum: e-postaya göre kullanıcı bul (varsa temel bilgiler)
    public static UserRow FindUserByEmail(string email)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 1 u.UserId, u.CompanyId, u.FullName, u.Email, u.Role, u.Permissions, u.LastLoginAt, u.AvatarImage, c.Name AS CompanyName, c.CompanyType " +
            "FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId WHERE u.Email = @e", con))
        {
            cmd.Parameters.AddWithValue("@e", (object)email ?? "");
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                return ReadUserRow(r);
            }
        }
    }

    // ---- Vendor: bayi (partner) ve kullanici olusturma, demo onayi ----
    public static int CreateCompany(string name, string taxNumber, int tierId, string culture)
    {
        return CreateCompany(name, taxNumber, tierId, culture, "Partner");
    }

    public static int CreateCompany(string name, string taxNumber, int tierId, string culture, string companyType)
    {
        return CreateCompany(name, taxNumber, tierId, culture, companyType, 0);
    }

    public static int CreateCompany(string name, string taxNumber, int tierId, string culture, string companyType, int supportMinutes)
    {
        if (companyType != "Partner" && companyType != "Customer") companyType = "Partner";
        bool isCustomer = companyType == "Customer";
        if (supportMinutes < 0) supportMinutes = 0;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.Companies (Name, TaxNumber, IsVendor, CompanyType, CurrentTierId, AnnualNetRevenue, PreferredCulture, SupportBalanceMinutes, IsActive) " +
            "OUTPUT INSERTED.CompanyId " +
            "VALUES (@n, @tax, 0, @type, @tier, 0, @cul, @sb, 1)", con))
        {
            cmd.Parameters.AddWithValue("@n", (object)name ?? "");
            cmd.Parameters.AddWithValue("@tax", string.IsNullOrEmpty(taxNumber) ? (object)DBNull.Value : taxNumber);
            cmd.Parameters.AddWithValue("@type", companyType);
            cmd.Parameters.AddWithValue("@tier", isCustomer ? (object)DBNull.Value : tierId);
            cmd.Parameters.AddWithValue("@cul", string.IsNullOrEmpty(culture) ? "tr" : culture);
            cmd.Parameters.AddWithValue("@sb", supportMinutes);
            object id = cmd.ExecuteScalar();
            return id != null ? (int)id : 0;
        }
    }

    // Firmanin kalan destek suresi (dakika)
    public static int GetSupportBalance(int companyId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(SupportBalanceMinutes,0) FROM dbo.Companies WHERE CompanyId = @c", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            object o = cmd.ExecuteScalar();
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }
    }

    // Tum firmalarin destek plani (dakika + gecerlilik tarihi). SupportUntil kolonu yoksa tarihsiz doner.
    public static System.Collections.Generic.Dictionary<int, SupportPlan> GetSupportPlanMap()
    {
        var map = new System.Collections.Generic.Dictionary<int, SupportPlan>();
        string[] colsets = new string[] {
            "CompanyId, ISNULL(SupportBalanceMinutes,0) AS M, SupportUntil AS U, ISNULL(ProjectBalanceHours,0) AS PH",
            "CompanyId, ISNULL(SupportBalanceMinutes,0) AS M, SupportUntil AS U",
            "CompanyId, ISNULL(SupportBalanceMinutes,0) AS M"
        };
        for (int ci = 0; ci < colsets.Length; ci++)
        {
            map.Clear();
            bool hasU = colsets[ci].Contains("AS U");
            bool hasPH = colsets[ci].Contains("AS PH");
            try
            {
                using (SqlConnection con = Db.Open())
                using (SqlCommand cmd = new SqlCommand("SELECT " + colsets[ci] + " FROM dbo.Companies", con))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        SupportPlan sp = new SupportPlan();
                        sp.Minutes = Convert.ToInt32(r["M"]);
                        if (hasU) sp.Until = r["U"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["U"]);
                        if (hasPH) sp.ProjectHours = Convert.ToDecimal(r["PH"]);
                        map[(int)r["CompanyId"]] = sp;
                    }
                }
                return map;
            }
            catch { continue; }
        }
        return map;
    }

    public static void UpdateSupportPlan(int companyId, int minutes, DateTime? until)
    {
        decimal ph = GetSupportPlan(companyId).ProjectHours;
        UpdateSupportPlan(companyId, minutes, until, ph);
    }

    public static void UpdateSupportPlan(int companyId, int minutes, DateTime? until, decimal projectHours)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Companies SET SupportBalanceMinutes = @m, SupportUntil = @u, ProjectBalanceHours = @ph WHERE CompanyId = @c", con))
            {
                cmd.Parameters.AddWithValue("@m", minutes);
                cmd.Parameters.AddWithValue("@u", until.HasValue ? (object)until.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ph", projectHours);
                cmd.Parameters.AddWithValue("@c", companyId);
                cmd.ExecuteNonQuery();
            }
            return;
        }
        catch { }
        UpdateSupportPlanLegacy(companyId, minutes, until);
    }

    private static void UpdateSupportPlanLegacy(int companyId, int minutes, DateTime? until)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Companies SET SupportBalanceMinutes = @m, SupportUntil = @u WHERE CompanyId = @c", con))
            {
                cmd.Parameters.AddWithValue("@m", minutes);
                cmd.Parameters.AddWithValue("@u", until.HasValue ? (object)until.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@c", companyId);
                cmd.ExecuteNonQuery();
            }
        }
        catch
        {
            // SupportUntil kolonu yoksa yalniz dakikayi guncelle
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Companies SET SupportBalanceMinutes = @m WHERE CompanyId = @c", con))
            { cmd.Parameters.AddWithValue("@m", minutes); cmd.Parameters.AddWithValue("@c", companyId); cmd.ExecuteNonQuery(); }
        }
    }

    // Talep gonderebilir mi: dakika > 0 VE (tarih yok ya da bugun/ilerisi)
    public static bool CanSubmitTicket(int companyId)
    {
        int mins = 0; DateTime? until = null; bool withUntil = true;
        for (int attempt = 0; attempt < 2; attempt++)
        {
            string cols = withUntil ? "ISNULL(SupportBalanceMinutes,0) AS M, SupportUntil AS U" : "ISNULL(SupportBalanceMinutes,0) AS M";
            try
            {
                using (SqlConnection con = Db.Open())
                using (SqlCommand cmd = new SqlCommand("SELECT " + cols + " FROM dbo.Companies WHERE CompanyId = @c", con))
                {
                    cmd.Parameters.AddWithValue("@c", companyId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                        if (r.Read()) { mins = Convert.ToInt32(r["M"]); if (withUntil) until = r["U"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["U"]); }
                }
                break;
            }
            catch { if (withUntil) { withUntil = false; continue; } return false; }
        }
        if (mins <= 0) return false;
        if (until.HasValue && until.Value.Date < DateTime.Today) return false;
        return true;
    }

    // Tip bazli firma listesi (Partner / Customer) - id + ad
    public static List<CompanyLite> GetCompaniesByType(string companyType)
    {
        List<CompanyLite> list = new List<CompanyLite>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT CompanyId, Name FROM dbo.Companies WHERE CompanyType = @t AND IsActive = 1 ORDER BY Name", con))
        {
            cmd.Parameters.AddWithValue("@t", companyType);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) { CompanyLite c = new CompanyLite(); c.CompanyId = (int)r["CompanyId"]; c.Name = r["Name"] as string; list.Add(c); }
            }
        }
        return list;
    }

    // Uretici firma id'si (destek personeli olusturmak icin)
    public static int GetVendorCompanyId2()
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 CompanyId FROM dbo.Companies WHERE CompanyType = N'Vendor' ORDER BY CompanyId", con))
        {
            object o = cmd.ExecuteScalar();
            return o != null && o != DBNull.Value ? (int)o : 0;
        }
    }

    public static bool EmailExists(string email)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Users WHERE Email = @e", con))
        {
            cmd.Parameters.AddWithValue("@e", (object)email ?? "");
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }

    public static int CreateUser(int companyId, string fullName, string email, string passwordHash, string role, string culture)
    {
        return CreateUser(companyId, fullName, email, passwordHash, role, culture, null);
    }

    public static int CreateUser(int companyId, string fullName, string email, string passwordHash, string role, string culture, string permissions)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.Users (CompanyId, FullName, Email, PasswordHash, Role, Permissions, PreferredCulture, IsActive) " +
            "OUTPUT INSERTED.UserId " +
            "VALUES (@c, @n, @e, @h, @r, @perm, @cul, 1)", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            cmd.Parameters.AddWithValue("@n", (object)fullName ?? "");
            cmd.Parameters.AddWithValue("@e", (object)email ?? "");
            cmd.Parameters.AddWithValue("@h", (object)passwordHash ?? "");
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@perm", string.IsNullOrEmpty(permissions) ? (object)DBNull.Value : permissions);
            cmd.Parameters.AddWithValue("@cul", string.IsNullOrEmpty(culture) ? (object)DBNull.Value : culture);
            object id = cmd.ExecuteScalar();
            return id != null ? (int)id : 0;
        }
    }

    public static void ApproveDemo(int demoRequestId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.DemoRequests SET Status = N'Approved', " +
            "ScheduledDate = COALESCE(ScheduledDate, PreferredDate) WHERE DemoRequestId = @d", con))
        {
            cmd.Parameters.AddWithValue("@d", demoRequestId);
            cmd.ExecuteNonQuery();
        }
    }

    public static DemoContact GetDemoContact(int demoRequestId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT d.ClientName, d.ProductModule, d.PreferredDate, d.ScheduledDate, c.Name AS CompanyName, " +
            "  COALESCE(u.Email, (SELECT TOP 1 Email FROM dbo.Users WHERE CompanyId = d.CompanyId AND IsActive = 1 ORDER BY Role)) AS Email, " +
            "  COALESCE(u.FullName, c.Name) AS ToName " +
            "FROM dbo.DemoRequests d " +
            "JOIN dbo.Companies c ON c.CompanyId = d.CompanyId " +
            "LEFT JOIN dbo.Users u ON u.UserId = d.RequestedByUserId " +
            "WHERE d.DemoRequestId = @d", con))
        {
            cmd.Parameters.AddWithValue("@d", demoRequestId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                DemoContact dc = new DemoContact();
                dc.ClientName = r["ClientName"] as string;
                dc.ProductModule = r["ProductModule"] as string;
                dc.CompanyName = r["CompanyName"] as string;
                dc.Email = r["Email"] == DBNull.Value ? null : (string)r["Email"];
                dc.ToName = r["ToName"] as string;
                dc.ScheduledDate = r["ScheduledDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ScheduledDate"];
                if (dc.ScheduledDate == null && r["PreferredDate"] != DBNull.Value) dc.ScheduledDate = (DateTime)r["PreferredDate"];
                return dc;
            }
        }
    }

    // ---- Müşteri Kayıt (lead koruma) ----
    public static void ExpireOldRegs()
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.CustomerRegistrations SET Status = N'Expired' " +
            "WHERE Status = N'Approved' AND ExpiresAt IS NOT NULL AND ExpiresAt <= SYSUTCDATETIME()", con))
        {
            cmd.ExecuteNonQuery();
        }
    }

    // Baska bir is ortagi bu musteriyi aktif korumada tutuyor mu?
    public static bool IsCustomerProtectedByOther(string customerName, int exceptCompanyId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT COUNT(1) FROM dbo.CustomerRegistrations " +
            "WHERE CustomerName = @n AND CompanyId <> @me " +
            "AND Status IN (N'Pending', N'Approved') " +
            "AND (ExpiresAt IS NULL OR ExpiresAt > SYSUTCDATETIME())", con))
        {
            cmd.Parameters.AddWithValue("@n", (object)customerName ?? "");
            cmd.Parameters.AddWithValue("@me", exceptCompanyId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }

    public static int CreateCustomerReg(int companyId, int? userId, string customerName, string contact,
                                        string contactEmail, string contactPhone, string serviceType, int temperature)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.CustomerRegistrations " +
            "(CompanyId, CustomerName, ContactPerson, ContactEmail, ContactPhone, ServiceType, SalesTemperature, Status, ExpiresAt, CreatedByUserId) " +
            "OUTPUT INSERTED.RegistrationId " +
            "VALUES (@c, @n, @ct, @ce, @cp, @st, @tmp, N'Pending', DATEADD(day, 30, SYSUTCDATETIME()), @u)", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            cmd.Parameters.AddWithValue("@n", (object)customerName ?? "");
            cmd.Parameters.AddWithValue("@ct", string.IsNullOrEmpty(contact) ? (object)DBNull.Value : contact);
            cmd.Parameters.AddWithValue("@ce", string.IsNullOrEmpty(contactEmail) ? (object)DBNull.Value : contactEmail);
            cmd.Parameters.AddWithValue("@cp", string.IsNullOrEmpty(contactPhone) ? (object)DBNull.Value : contactPhone);
            cmd.Parameters.AddWithValue("@st", string.IsNullOrEmpty(serviceType) ? (object)DBNull.Value : serviceType);
            cmd.Parameters.AddWithValue("@tmp", temperature);
            cmd.Parameters.AddWithValue("@u", (object)userId ?? DBNull.Value);
            object id = cmd.ExecuteScalar();
            return id != null ? (int)id : 0;
        }
    }

    public static List<CustomerReg> GetCustomerRegs(int companyId)
    {
        ExpireOldRegs();
        List<CustomerReg> list = new List<CustomerReg>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT RegistrationId, CompanyId, CustomerName, ContactPerson, ContactEmail, ContactPhone, ServiceType, SalesTemperature, " +
            "Status, RegisteredAt, ExpiresAt, RenewalRequested " +
            "FROM dbo.CustomerRegistrations WHERE CompanyId = @c ORDER BY RegisteredAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(ReadReg(r, null));
            }
        }
        return list;
    }

    public static CustomerReg GetRegById(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT r.RegistrationId, r.CompanyId, r.CustomerName, r.ContactPerson, r.ContactEmail, r.ContactPhone, r.ServiceType, r.SalesTemperature, " +
            "r.Status, r.RegisteredAt, r.ExpiresAt, r.RenewalRequested, c.Name AS CompanyName " +
            "FROM dbo.CustomerRegistrations r JOIN dbo.Companies c ON c.CompanyId = r.CompanyId " +
            "WHERE r.RegistrationId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                return ReadReg(r, "CompanyName");
            }
        }
    }

    private static CustomerReg ReadReg(SqlDataReader r, string companyNameCol)
    {
        CustomerReg c = new CustomerReg();
        c.RegistrationId = (int)r["RegistrationId"];
        c.CompanyId = (int)r["CompanyId"];
        c.CustomerName = r["CustomerName"] as string;
        c.ContactPerson = r["ContactPerson"] == DBNull.Value ? "" : (string)r["ContactPerson"];
        c.ContactEmail = r["ContactEmail"] == DBNull.Value ? "" : (string)r["ContactEmail"];
        c.ContactPhone = r["ContactPhone"] == DBNull.Value ? "" : (string)r["ContactPhone"];
        c.ServiceType = r["ServiceType"] == DBNull.Value ? "" : (string)r["ServiceType"];
        c.SalesTemperature = (int)r["SalesTemperature"];
        c.Status = r["Status"] as string;
        c.RegisteredAt = (DateTime)r["RegisteredAt"];
        c.ExpiresAt = r["ExpiresAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ExpiresAt"];
        c.RenewalRequested = (bool)r["RenewalRequested"];
        if (companyNameCol != null) c.CompanyName = r[companyNameCol] as string;
        return c;
    }

    public static void RequestRenewal(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.CustomerRegistrations SET RenewalRequested = 1, RenewalRequestedAt = SYSUTCDATETIME() " +
            "WHERE RegistrationId = @id AND Status = N'Approved'", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public static List<CustomerReg> GetVendorRegQueue(bool renewalsOnly)
    {
        ExpireOldRegs();
        List<CustomerReg> list = new List<CustomerReg>();
        string where = renewalsOnly ? "r.RenewalRequested = 1" : "r.Status = N'Pending'";
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT r.RegistrationId, r.CompanyId, r.CustomerName, r.ContactPerson, r.ContactEmail, r.ContactPhone, r.ServiceType, r.SalesTemperature, " +
            "r.Status, r.RegisteredAt, r.ExpiresAt, r.RenewalRequested, c.Name AS CompanyName " +
            "FROM dbo.CustomerRegistrations r JOIN dbo.Companies c ON c.CompanyId = r.CompanyId " +
            "WHERE " + where + " ORDER BY r.RegisteredAt ASC", con))
        {
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(ReadReg(r, "CompanyName"));
            }
        }
        return list;
    }

    public static void DecideReg(int id, bool approve)
    {
        string sql = approve
            ? "UPDATE dbo.CustomerRegistrations SET Status=N'Approved', ApprovedAt=SYSUTCDATETIME(), " +
              "ExpiresAt=DATEADD(day,30,SYSUTCDATETIME()) WHERE RegistrationId=@id"
            : "UPDATE dbo.CustomerRegistrations SET Status=N'Rejected' WHERE RegistrationId=@id";
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public static void DecideRenewal(int id, bool approve)
    {
        string sql = approve
            ? "UPDATE dbo.CustomerRegistrations SET Status=N'Approved', RenewalRequested=0, " +
              "ExpiresAt=DATEADD(day,30,SYSUTCDATETIME()) WHERE RegistrationId=@id"
            : "UPDATE dbo.CustomerRegistrations SET RenewalRequested=0 WHERE RegistrationId=@id";
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public static int GetVendorCompanyId()
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 1 CompanyId FROM dbo.Companies WHERE IsVendor = 1 ORDER BY CompanyId", con))
        {
            object o = cmd.ExecuteScalar();
            return o == null || o == DBNull.Value ? 0 : (int)o;
        }
    }

    public static string GetCompanyPrimaryEmail(int companyId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 1 Email FROM dbo.Users WHERE CompanyId = @c AND IsActive = 1 ORDER BY Role", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            object o = cmd.ExecuteScalar();
            return o == null || o == DBNull.Value ? null : (string)o;
        }
    }

    public static void AddNotification(int companyId, string titleTr, string titleEn, string bodyTr, string bodyEn, string type)
    {
        AddNotification(companyId, titleTr, titleEn, bodyTr, bodyEn, type, null);
    }

    public static void AddNotification(int companyId, string titleTr, string titleEn, string bodyTr, string bodyEn, string type, string link)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.Notifications (CompanyId, TitleTr, TitleEn, BodyTr, BodyEn, Type, Link) " +
            "VALUES (@c, @tt, @te, @bt, @be, @ty, @ln)", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            cmd.Parameters.AddWithValue("@tt", (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@te", (object)titleEn ?? "");
            cmd.Parameters.AddWithValue("@bt", (object)bodyTr ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@be", (object)bodyEn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ty", (object)type ?? "System");
            cmd.Parameters.AddWithValue("@ln", string.IsNullOrEmpty(link) ? (object)DBNull.Value : link);
            cmd.ExecuteNonQuery();
        }
    }

    // ================= DESTEK (TICKET) =================
    public static string FormatTicketNo(int ticketId)
    {
        int n = 13000 + ticketId;
        return "TLP-" + n.ToString("000000");
    }

    // Yeni ticket + ilk mesaj (aciklama). Olusan ticketId doner, ticketNo out.
    public static int CreateTicket(int companyId, int openedByUserId, string product, string module,
        string requestType, string priority, int? customerRegId, string customerName, string version,
        string environment, string subject, string phone, string description, out string ticketNo)
    {
        ticketNo = "";
        int ticketId = 0;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.Tickets (CompanyId, OpenedByUserId, Product, Module, RequestType, Priority, Status, " +
                "CustomerRegId, CustomerName, Version, Environment, Subject, Phone) " +
                "OUTPUT INSERTED.TicketId " +
                "VALUES (@co, @by, @prod, @mod, @rt, @prio, N'New', @crid, @cust, @ver, @env, @subj, @ph)", con))
            {
                cmd.Parameters.AddWithValue("@co", companyId);
                cmd.Parameters.AddWithValue("@by", openedByUserId);
                cmd.Parameters.AddWithValue("@prod", (object)product ?? "");
                cmd.Parameters.AddWithValue("@mod", (object)module ?? "");
                cmd.Parameters.AddWithValue("@rt", (object)requestType ?? "");
                cmd.Parameters.AddWithValue("@prio", (object)priority ?? "Normal");
                cmd.Parameters.AddWithValue("@crid", customerRegId.HasValue ? (object)customerRegId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@cust", string.IsNullOrEmpty(customerName) ? (object)DBNull.Value : customerName);
                cmd.Parameters.AddWithValue("@ver", string.IsNullOrEmpty(version) ? (object)DBNull.Value : version);
                cmd.Parameters.AddWithValue("@env", string.IsNullOrEmpty(environment) ? (object)DBNull.Value : environment);
                cmd.Parameters.AddWithValue("@subj", (object)subject ?? "");
                cmd.Parameters.AddWithValue("@ph", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                object o = cmd.ExecuteScalar();
                ticketId = o != null ? (int)o : 0;
            }
            if (ticketId > 0)
            {
                ticketNo = FormatTicketNo(ticketId);
                using (SqlCommand up = new SqlCommand("UPDATE dbo.Tickets SET TicketNo = @no WHERE TicketId = @id", con))
                {
                    up.Parameters.AddWithValue("@no", ticketNo);
                    up.Parameters.AddWithValue("@id", ticketId);
                    up.ExecuteNonQuery();
                }
                using (SqlCommand m = new SqlCommand(
                    "INSERT INTO dbo.TicketMessages (TicketId, AuthorUserId, AuthorSide, Body, IsInternal, MsgKind) " +
                    "VALUES (@t, @by, N'Requester', @b, 0, N'Reply')", con))
                {
                    m.Parameters.AddWithValue("@t", ticketId);
                    m.Parameters.AddWithValue("@by", openedByUserId);
                    m.Parameters.AddWithValue("@b", (object)description ?? "");
                    m.ExecuteNonQuery();
                }
            }
        }
        return ticketId;
    }

    public static int AddTicketMessage(int ticketId, int? authorUserId, string authorSide, string body, bool isInternal, string kind)
    {
        using (SqlConnection con = Db.Open())
        {
            int mid;
            using (SqlCommand m = new SqlCommand(
                "INSERT INTO dbo.TicketMessages (TicketId, AuthorUserId, AuthorSide, Body, IsInternal, MsgKind) " +
                "OUTPUT INSERTED.MessageId VALUES (@t, @by, @side, @b, @int, @k)", con))
            {
                m.Parameters.AddWithValue("@t", ticketId);
                m.Parameters.AddWithValue("@by", authorUserId.HasValue ? (object)authorUserId.Value : DBNull.Value);
                m.Parameters.AddWithValue("@side", (object)authorSide ?? "System");
                m.Parameters.AddWithValue("@b", (object)body ?? "");
                m.Parameters.AddWithValue("@int", isInternal);
                m.Parameters.AddWithValue("@k", (object)kind ?? "Reply");
                object o = m.ExecuteScalar();
                mid = o != null ? (int)o : 0;
            }
            using (SqlCommand up = new SqlCommand("UPDATE dbo.Tickets SET UpdatedAt = SYSUTCDATETIME() WHERE TicketId = @id", con))
            { up.Parameters.AddWithValue("@id", ticketId); up.ExecuteNonQuery(); }
            return mid;
        }
    }

    public static void AddTicketAttachment(int ticketId, int? messageId, string stored, string original, string contentType, long size)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.TicketAttachments (TicketId, MessageId, StoredName, OriginalName, ContentType, FileSize) " +
            "VALUES (@t, @m, @sn, @on, @ct, @sz)", con))
        {
            cmd.Parameters.AddWithValue("@t", ticketId);
            cmd.Parameters.AddWithValue("@m", messageId.HasValue ? (object)messageId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@sn", (object)stored ?? "");
            cmd.Parameters.AddWithValue("@on", (object)original ?? "");
            cmd.Parameters.AddWithValue("@ct", (object)contentType ?? "");
            cmd.Parameters.AddWithValue("@sz", size);
            cmd.ExecuteNonQuery();
        }
    }

    // ===================== Harici Destek API (Faz 1) =====================
    private static string Sha256Hex(string s)
    {
        using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
        {
            byte[] b = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s ?? ""));
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < b.Length; i++) sb.Append(b[i].ToString("x2"));
            return sb.ToString();
        }
    }

    // Harici taleplerin sahibi olacak sanal firma + (pasif) sistem kullanicisi
    public static int[] EnsureApiOwner()
    {
        const string apiCompanyName = "Harici Ürün Destekleri";
        int companyId = 0, userId = 0;
        using (SqlConnection con = Db.Open())
        using (SqlCommand c = new SqlCommand("SELECT TOP 1 CompanyId FROM dbo.Companies WHERE Name = @n ORDER BY CompanyId", con))
        { c.Parameters.AddWithValue("@n", apiCompanyName); object o = c.ExecuteScalar(); if (o != null) companyId = (int)o; }
        if (companyId == 0)
        {
            int tierId = 0;
            using (SqlConnection con2 = Db.Open())
            using (SqlCommand tc = new SqlCommand("SELECT TOP 1 TierId FROM dbo.PartnerTiers ORDER BY SortOrder", con2))
            { object o = tc.ExecuteScalar(); if (o != null) tierId = (int)o; }
            companyId = CreateCompany(apiCompanyName, "", tierId, "tr", "Customer");
        }
        using (SqlConnection con = Db.Open())
        using (SqlCommand c = new SqlCommand("SELECT TOP 1 UserId FROM dbo.Users WHERE CompanyId = @c ORDER BY UserId", con))
        { c.Parameters.AddWithValue("@c", companyId); object o = c.ExecuteScalar(); if (o != null) userId = (int)o; }
        if (userId == 0)
        {
            userId = CreateUser(companyId, "API Servisi", "api-system@local", "!disabled!", "CustomerUser", "tr", "");
            using (SqlConnection con = Db.Open())
            using (SqlCommand up = new SqlCommand("UPDATE dbo.Users SET IsActive = 0 WHERE UserId = @id", con))
            { up.Parameters.AddWithValue("@id", userId); up.ExecuteNonQuery(); }
        }
        return new int[] { companyId, userId };
    }

    // Anahtar dogrula: gecerli+aktif ise urun adini dondurur, degilse null
    public static string ValidateApiKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        string hash = Sha256Hex(key.Trim());
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ApiClientId, Name FROM dbo.ApiClients WHERE KeyHash = @h AND IsActive = 1", con))
        {
            cmd.Parameters.AddWithValue("@h", hash);
            int cid = 0; string name = null;
            using (SqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { cid = (int)r["ApiClientId"]; name = r["Name"] as string; } }
            if (cid > 0)
            {
                using (SqlCommand up = new SqlCommand("UPDATE dbo.ApiClients SET LastUsedAt = SYSUTCDATETIME() WHERE ApiClientId = @id", con))
                { up.Parameters.AddWithValue("@id", cid); up.ExecuteNonQuery(); }
            }
            return name;
        }
    }

    // Anahtardan tam istemci bilgisi (isim + firma + clientId). Kolonlar yoksa firmasiz doner.
    public static ApiClientInfo ValidateApiClient(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        string hash = Sha256Hex(key.Trim());
        bool withExtra = true;
        for (int attempt = 0; attempt < 2; attempt++)
        {
            string cols = withExtra ? "ApiClientId, Name, ISNULL(CompanyId,0) AS CompanyId, ClientId" : "ApiClientId, Name";
            try
            {
                ApiClientInfo info = null;
                using (SqlConnection con = Db.Open())
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT " + cols + " FROM dbo.ApiClients WHERE KeyHash = @h AND IsActive = 1", con))
                    {
                        cmd.Parameters.AddWithValue("@h", hash);
                        using (SqlDataReader r = cmd.ExecuteReader())
                            if (r.Read())
                            {
                                info = new ApiClientInfo();
                                info.ApiClientId = (int)r["ApiClientId"];
                                info.Name = r["Name"] as string;
                                if (withExtra) { info.CompanyId = Convert.ToInt32(r["CompanyId"]); info.ClientId = r["ClientId"] == DBNull.Value ? "" : (string)r["ClientId"]; }
                            }
                    }
                    if (info != null)
                        using (SqlCommand up = new SqlCommand("UPDATE dbo.ApiClients SET LastUsedAt = SYSUTCDATETIME() WHERE ApiClientId = @id", con))
                        { up.Parameters.AddWithValue("@id", info.ApiClientId); up.ExecuteNonQuery(); }
                }
                return info;
            }
            catch { if (withExtra) { withExtra = false; continue; } return null; }
        }
        return null;
    }

    // Yeni anahtar uret (duz metin bir kez dondurulur; sadece hash saklanir)
    public static string CreateApiClient(string name)
    {
        string cid;
        return CreateApiClient(name, 0, out cid);
    }

    public static string CreateApiClient(string name, int companyId, out string clientId)
    {
        byte[] rb = new byte[24];
        using (System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) rng.GetBytes(rb);
        System.Text.StringBuilder kb = new System.Text.StringBuilder("rvr_");
        for (int i = 0; i < rb.Length; i++) kb.Append(rb[i].ToString("x2"));
        string key = kb.ToString();
        string hash = Sha256Hex(key);
        string prefix = key.Substring(0, 12);
        clientId = "cid_" + Guid.NewGuid().ToString("N");
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ApiClients (Name, KeyHash, KeyPrefix, CompanyId, ClientId, IsActive) VALUES (@n, @h, @p, @co, @cl, 1)", con))
            {
                cmd.Parameters.AddWithValue("@n", (object)name ?? "");
                cmd.Parameters.AddWithValue("@h", hash);
                cmd.Parameters.AddWithValue("@p", prefix);
                cmd.Parameters.AddWithValue("@co", companyId > 0 ? (object)companyId : DBNull.Value);
                cmd.Parameters.AddWithValue("@cl", clientId);
                cmd.ExecuteNonQuery();
            }
        }
        catch
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ApiClients (Name, KeyHash, KeyPrefix, IsActive) VALUES (@n, @h, @p, 1)", con))
            {
                cmd.Parameters.AddWithValue("@n", (object)name ?? "");
                cmd.Parameters.AddWithValue("@h", hash);
                cmd.Parameters.AddWithValue("@p", prefix);
                cmd.ExecuteNonQuery();
            }
            clientId = "";
        }
        return key;
    }

    public static System.Data.DataTable GetApiClients()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT a.ApiClientId, a.Name, a.KeyPrefix, a.IsActive, a.CreatedAt, a.LastUsedAt, a.ClientId, c.Name AS CompanyName FROM dbo.ApiClients a LEFT JOIN dbo.Companies c ON c.CompanyId = a.CompanyId ORDER BY a.Name", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }
        catch
        {
            dt = new System.Data.DataTable();
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT ApiClientId, Name, KeyPrefix, IsActive, CreatedAt, LastUsedAt FROM dbo.ApiClients ORDER BY Name", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }
        return dt;
    }

    public static void SetApiClientActive(int id, bool active)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.ApiClients SET IsActive = @a WHERE ApiClientId = @id", con))
        { cmd.Parameters.AddWithValue("@a", active); cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }

    public static void DeleteApiClient(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.ApiClients WHERE ApiClientId = @id", con))
        { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }

    // Idempotency: ayni (urun, dis referans) daha once dustuyse mevcut talebi dondur
    public static int FindApiTicket(string product, string externalRef, out string ticketNo)
    {
        ticketNo = "";
        if (string.IsNullOrEmpty(externalRef)) return 0;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 TicketId, TicketNo FROM dbo.Tickets WHERE ExternalProduct = @p AND ExternalRef = @r", con))
        {
            cmd.Parameters.AddWithValue("@p", (object)product ?? "");
            cmd.Parameters.AddWithValue("@r", externalRef);
            using (SqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { ticketNo = r["TicketNo"] as string; return (int)r["TicketId"]; } }
        }
        return 0;
    }

    // Harici talep olustur (sanal firma/kullanici + harici kaynak alanlari)
    public static int CreateApiTicket(int companyId, int systemUserId, string product, string subject, string description,
        string priority, string reqCompany, string reqName, string reqEmail, string reqPhone, string externalRef, out string ticketNo)
    {
        ticketNo = ""; int ticketId = 0;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.Tickets (CompanyId, OpenedByUserId, Product, Module, RequestType, Priority, Status, Subject, Phone, " +
                "Source, ExternalProduct, ExternalRef, RequesterCompany, RequesterName, RequesterEmail, RequesterPhone) " +
                "OUTPUT INSERTED.TicketId " +
                "VALUES (@co, @by, @prod, N'', N'', @prio, N'New', @subj, @ph, N'Api', @prod, @ref, @rc, @rn, @re, @rp)", con))
            {
                cmd.Parameters.AddWithValue("@co", companyId);
                cmd.Parameters.AddWithValue("@by", systemUserId);
                cmd.Parameters.AddWithValue("@prod", (object)product ?? "");
                cmd.Parameters.AddWithValue("@prio", (object)priority ?? "Normal");
                cmd.Parameters.AddWithValue("@subj", (object)subject ?? "");
                cmd.Parameters.AddWithValue("@ph", string.IsNullOrEmpty(reqPhone) ? (object)DBNull.Value : reqPhone);
                cmd.Parameters.AddWithValue("@ref", string.IsNullOrEmpty(externalRef) ? (object)DBNull.Value : externalRef);
                cmd.Parameters.AddWithValue("@rc", (object)reqCompany ?? "");
                cmd.Parameters.AddWithValue("@rn", (object)reqName ?? "");
                cmd.Parameters.AddWithValue("@re", (object)reqEmail ?? "");
                cmd.Parameters.AddWithValue("@rp", (object)reqPhone ?? "");
                object o = cmd.ExecuteScalar();
                ticketId = o != null ? (int)o : 0;
            }
            if (ticketId > 0)
            {
                ticketNo = FormatTicketNo(ticketId);
                using (SqlCommand up = new SqlCommand("UPDATE dbo.Tickets SET TicketNo = @no WHERE TicketId = @id", con))
                { up.Parameters.AddWithValue("@no", ticketNo); up.Parameters.AddWithValue("@id", ticketId); up.ExecuteNonQuery(); }
                using (SqlCommand m = new SqlCommand(
                    "INSERT INTO dbo.TicketMessages (TicketId, AuthorUserId, AuthorSide, Body, IsInternal, MsgKind) " +
                    "VALUES (@t, @by, N'Requester', @b, 0, N'Reply')", con))
                {
                    m.Parameters.AddWithValue("@t", ticketId);
                    m.Parameters.AddWithValue("@by", systemUserId);
                    m.Parameters.AddWithValue("@b", (object)description ?? "");
                    m.ExecuteNonQuery();
                }
            }
        }
        return ticketId;
    }

    public static void LogApiRequest(string clientName, string ip, string endpoint, string externalRef, int status, string note)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.ApiRequestLog (ClientName, RemoteIp, Endpoint, ExternalRef, StatusCode, Note) VALUES (@c, @ip, @e, @r, @s, @n)", con))
            {
                cmd.Parameters.AddWithValue("@c", (object)clientName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ip", (object)ip ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@e", (object)endpoint ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@r", (object)externalRef ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@n", (object)(note ?? "") ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
        catch { }
    }

    // Gorunen durum sutunu (migration 44) varsa talep sorgularina eklenir; yoksa sayfalar eskisi gibi calisir
    private static bool? statusNoteCol;
    public static bool HasStatusNoteCol()
    {
        if (statusNoteCol.HasValue) return statusNoteCol.Value;
        try { using (SqlConnection con = Db.Open()) using (SqlCommand cmd = new SqlCommand("SELECT COL_LENGTH('dbo.Tickets', 'StatusNote')", con)) { object o = cmd.ExecuteScalar(); statusNoteCol = o != null && o != DBNull.Value; } }
        catch { statusNoteCol = false; }
        return statusNoteCol.Value;
    }
    private static string TicketSelect { get { return HasStatusNoteCol() ? TICKET_SELECT.Replace("t.RequesterPhone ", "t.RequesterPhone, t.StatusNote ") : TICKET_SELECT; } }
    private const string TICKET_SELECT =
        "SELECT t.TicketId, t.TicketNo, t.CompanyId, c.Name AS CompanyName, c.CompanyType, t.OpenedByUserId, " +
        "u.FullName AS OpenedByName, t.Product, t.Module, t.RequestType, t.Priority, t.Status, t.CustomerName, " +
        "t.Version, t.Environment, t.Subject, t.Phone, t.AssignedToUserId, au.FullName AS AssignedToName, " +
        "t.CreatedAt, t.UpdatedAt, t.ResolvedAt, t.ResolutionMinutes, t.Source, t.ExternalProduct, t.ExternalRef, " +
        "t.RequesterCompany, t.RequesterName, t.RequesterEmail, t.RequesterPhone " +
        "FROM dbo.Tickets t JOIN dbo.Companies c ON c.CompanyId = t.CompanyId " +
        "LEFT JOIN dbo.Users u ON u.UserId = t.OpenedByUserId " +
        "LEFT JOIN dbo.Users au ON au.UserId = t.AssignedToUserId ";

    // Uzerine alinmamis (havuzda bekleyen) acik talepler
    public static List<Ticket> GetUnassignedTickets()
    {
        List<Ticket> list = new List<Ticket>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TicketSelect +
            "WHERE t.AssignedToUserId IS NULL AND t.Status NOT IN (N'Closed', N'Resolved') ORDER BY t.CreatedAt ASC", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadTicket(r)); }
        return list;
    }

    private static Ticket ReadTicket(SqlDataReader r)
    {
        Ticket t = new Ticket();
        t.TicketId = (int)r["TicketId"];
        t.TicketNo = r["TicketNo"] as string;
        t.CompanyId = (int)r["CompanyId"];
        t.CompanyName = r["CompanyName"] as string;
        t.CompanyType = r["CompanyType"] as string;
        t.OpenedByUserId = (int)r["OpenedByUserId"];
        t.OpenedByName = r["OpenedByName"] as string;
        t.Product = r["Product"] as string;
        t.Module = r["Module"] as string;
        t.RequestType = r["RequestType"] as string;
        t.Priority = r["Priority"] as string;
        t.Status = r["Status"] as string;
        t.CustomerName = r["CustomerName"] == DBNull.Value ? "" : (string)r["CustomerName"];
        t.Version = r["Version"] == DBNull.Value ? "" : (string)r["Version"];
        t.Environment = r["Environment"] == DBNull.Value ? "" : (string)r["Environment"];
        t.Subject = r["Subject"] as string;
        t.Phone = r["Phone"] == DBNull.Value ? "" : (string)r["Phone"];
        t.AssignedToUserId = r["AssignedToUserId"] == DBNull.Value ? (int?)null : (int)r["AssignedToUserId"];
        t.AssignedToName = r["AssignedToName"] == DBNull.Value ? "" : (string)r["AssignedToName"];
        t.CreatedAt = (DateTime)r["CreatedAt"];
        t.UpdatedAt = (DateTime)r["UpdatedAt"];
        t.ResolvedAt = r["ResolvedAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ResolvedAt"];
        t.ResolutionMinutes = r["ResolutionMinutes"] == DBNull.Value ? (int?)null : (int)r["ResolutionMinutes"];
        t.Source = r["Source"] == DBNull.Value ? "Portal" : (string)r["Source"];
        t.ExternalProduct = r["ExternalProduct"] == DBNull.Value ? "" : (string)r["ExternalProduct"];
        t.ExternalRef = r["ExternalRef"] == DBNull.Value ? "" : (string)r["ExternalRef"];
        t.RequesterCompany = r["RequesterCompany"] == DBNull.Value ? "" : (string)r["RequesterCompany"];
        t.RequesterName = r["RequesterName"] == DBNull.Value ? "" : (string)r["RequesterName"];
        t.RequesterEmail = r["RequesterEmail"] == DBNull.Value ? "" : (string)r["RequesterEmail"];
        t.RequesterPhone = r["RequesterPhone"] == DBNull.Value ? "" : (string)r["RequesterPhone"];
        for (int i = 0; i < r.FieldCount; i++) if (r.GetName(i) == "StatusNote") { t.StatusNote = r.IsDBNull(i) ? null : (string)r[i]; break; }
        return t;
    }

    public static List<Ticket> GetTicketsForCompany(int companyId)
    {
        List<Ticket> list = new List<Ticket>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TicketSelect + "WHERE t.CompanyId = @c ORDER BY t.UpdatedAt DESC", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadTicket(r)); }
        }
        return list;
    }

    public static List<Ticket> GetAllTickets()
    {
        List<Ticket> list = new List<Ticket>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TicketSelect + "ORDER BY " +
            "CASE t.Status WHEN N'New' THEN 0 WHEN N'InProgress' THEN 1 WHEN N'WaitingCustomer' THEN 2 WHEN N'Resolved' THEN 3 ELSE 4 END, " +
            "CASE t.Priority WHEN N'Critical' THEN 0 WHEN N'High' THEN 1 WHEN N'Normal' THEN 2 ELSE 3 END, t.UpdatedAt DESC", con))
        {
            using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadTicket(r)); }
        }
        return list;
    }

    public static Ticket GetTicket(int ticketId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TicketSelect + "WHERE t.TicketId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", ticketId);
            using (SqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) return ReadTicket(r); }
        }
        return null;
    }

    public static List<TicketMessage> GetTicketMessages(int ticketId, bool includeInternal)
    {
        List<TicketMessage> list = new List<TicketMessage>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT m.MessageId, m.TicketId, m.AuthorUserId, u.FullName AS AuthorName, m.AuthorSide, m.Body, m.IsInternal, m.MsgKind, m.CreatedAt " +
            "FROM dbo.TicketMessages m LEFT JOIN dbo.Users u ON u.UserId = m.AuthorUserId " +
            "WHERE m.TicketId = @t " + (includeInternal ? "" : "AND m.IsInternal = 0 ") +
            "ORDER BY m.CreatedAt ASC, m.MessageId ASC", con))
        {
            cmd.Parameters.AddWithValue("@t", ticketId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    TicketMessage m = new TicketMessage();
                    m.MessageId = (int)r["MessageId"];
                    m.TicketId = (int)r["TicketId"];
                    m.AuthorUserId = r["AuthorUserId"] == DBNull.Value ? (int?)null : (int)r["AuthorUserId"];
                    m.AuthorName = r["AuthorName"] == DBNull.Value ? "" : (string)r["AuthorName"];
                    m.AuthorSide = r["AuthorSide"] as string;
                    m.Body = r["Body"] as string;
                    m.IsInternal = (bool)r["IsInternal"];
                    m.MsgKind = r["MsgKind"] as string;
                    m.CreatedAt = (DateTime)r["CreatedAt"];
                    list.Add(m);
                }
            }
        }
        return list;
    }

    public static List<TicketAttachment> GetTicketAttachments(int ticketId)
    {
        List<TicketAttachment> list = new List<TicketAttachment>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT AttachmentId, TicketId, MessageId, StoredName, OriginalName, FileSize FROM dbo.TicketAttachments WHERE TicketId = @t ORDER BY AttachmentId", con))
        {
            cmd.Parameters.AddWithValue("@t", ticketId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    TicketAttachment a = new TicketAttachment();
                    a.AttachmentId = (int)r["AttachmentId"];
                    a.TicketId = (int)r["TicketId"];
                    a.MessageId = r["MessageId"] == DBNull.Value ? (int?)null : (int)r["MessageId"];
                    a.StoredName = r["StoredName"] as string;
                    a.OriginalName = r["OriginalName"] as string;
                    a.FileSize = Convert.ToInt64(r["FileSize"]);
                    list.Add(a);
                }
            }
        }
        return list;
    }

    // Cozum suresini kaydeder ve farki (delta) firma destek bakiyesinden duser. Yeni bakiyeyi dondurur.
    public static int SetResolutionMinutes(int ticketId, int minutes)
    {
        if (minutes < 0) minutes = 0;
        int companyId = 0, old = 0;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand q = new SqlCommand("SELECT CompanyId, ISNULL(ResolutionMinutes,0) FROM dbo.Tickets WHERE TicketId = @id", con))
            {
                q.Parameters.AddWithValue("@id", ticketId);
                using (SqlDataReader r = q.ExecuteReader()) { if (r.Read()) { companyId = (int)r[0]; old = Convert.ToInt32(r[1]); } }
            }
            using (SqlCommand up = new SqlCommand("UPDATE dbo.Tickets SET ResolutionMinutes = @m WHERE TicketId = @id", con))
            { up.Parameters.AddWithValue("@m", minutes); up.Parameters.AddWithValue("@id", ticketId); up.ExecuteNonQuery(); }

            int delta = minutes - old;
            if (delta != 0 && companyId > 0)
                using (SqlCommand b = new SqlCommand("UPDATE dbo.Companies SET SupportBalanceMinutes = ISNULL(SupportBalanceMinutes,0) - @d WHERE CompanyId = @c", con))
                { b.Parameters.AddWithValue("@d", delta); b.Parameters.AddWithValue("@c", companyId); b.ExecuteNonQuery(); }

            int bal = 0;
            if (companyId > 0)
                using (SqlCommand g = new SqlCommand("SELECT ISNULL(SupportBalanceMinutes,0) FROM dbo.Companies WHERE CompanyId = @c", con))
                { g.Parameters.AddWithValue("@c", companyId); object o = g.ExecuteScalar(); bal = o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o); }
            return bal;
        }
    }

    // Destek tarafi talebi ilk kez actiginda: SupportSeenAt bos ise isaretle. Yeni isaretlendiyse true.
    public static bool MarkTicketSeen(int ticketId, string byName)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.Tickets SET SupportSeenAt = SYSUTCDATETIME(), SupportSeenBy = @b WHERE TicketId = @id AND SupportSeenAt IS NULL", con))
            {
                cmd.Parameters.AddWithValue("@b", (object)byName ?? "");
                cmd.Parameters.AddWithValue("@id", ticketId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        catch { return false; }
    }

    // Destek tarafinca HENUZ acilmamis (okunmamis) talep id'leri
    public static System.Collections.Generic.HashSet<int> GetUnseenTicketIds()
    {
        var set = new System.Collections.Generic.HashSet<int>();
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT TicketId FROM dbo.Tickets WHERE SupportSeenAt IS NULL", con))
            using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) set.Add((int)r[0]); }
        }
        catch { }
        return set;
    }

    // Son hatirlatma zamani: LastReminderAt kolonu varsa ondan, yoksa TicketMessages'taki son "Reminder" kaydindan (guvenilir).
    public static DateTime? GetTicketReminderAt(int ticketId)
    {
        DateTime? fromCol = null;
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT LastReminderAt FROM dbo.Tickets WHERE TicketId = @id", con))
            { cmd.Parameters.AddWithValue("@id", ticketId); object o = cmd.ExecuteScalar(); if (o != null && o != DBNull.Value) fromCol = Convert.ToDateTime(o); }
        }
        catch { }
        DateTime? fromMsg = null;
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT MAX(CreatedAt) FROM dbo.TicketMessages WHERE TicketId = @id AND MsgKind = N'Reminder'", con))
            { cmd.Parameters.AddWithValue("@id", ticketId); object o = cmd.ExecuteScalar(); if (o != null && o != DBNull.Value) fromMsg = Convert.ToDateTime(o); }
        }
        catch { }
        if (fromCol.HasValue && fromMsg.HasValue) return fromCol.Value > fromMsg.Value ? fromCol : fromMsg;
        return fromCol ?? fromMsg;
    }

    public static void SetReminderNow(int ticketId)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Tickets SET LastReminderAt = SYSUTCDATETIME() WHERE TicketId = @id", con))
            { cmd.Parameters.AddWithValue("@id", ticketId); cmd.ExecuteNonQuery(); }
        }
        catch { }
    }

    public static void DeductSupportMinutes(int companyId, int minutes)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Companies SET SupportBalanceMinutes = ISNULL(SupportBalanceMinutes,0) - @m WHERE CompanyId = @c", con))
        { cmd.Parameters.AddWithValue("@m", minutes); cmd.Parameters.AddWithValue("@c", companyId); cmd.ExecuteNonQuery(); }
    }

    public static void DeductProjectHours(int companyId, decimal hours)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Companies SET ProjectBalanceHours = ISNULL(ProjectBalanceHours,0) - @h WHERE CompanyId = @c", con))
        { cmd.Parameters.AddWithValue("@h", hours); cmd.Parameters.AddWithValue("@c", companyId); cmd.ExecuteNonQuery(); }
    }

    public static int AddProjectRecord(int companyId, int byUserId, string byName, string content, string pool, decimal? hours, int? minutes, string recipients)
    {
        int id = 0;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.ProjectRecords (CompanyId, CreatedByUserId, CreatedByName, Content, Pool, Hours, Minutes, Recipients) " +
            "OUTPUT INSERTED.RecordId VALUES (@c, @by, @bn, @ct, @p, @h, @m, @r)", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            cmd.Parameters.AddWithValue("@by", byUserId > 0 ? (object)byUserId : DBNull.Value);
            cmd.Parameters.AddWithValue("@bn", (object)byName ?? "");
            cmd.Parameters.AddWithValue("@ct", (object)content ?? "");
            cmd.Parameters.AddWithValue("@p", (object)pool ?? "project");
            cmd.Parameters.AddWithValue("@h", hours.HasValue ? (object)hours.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@m", minutes.HasValue ? (object)minutes.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@r", (object)recipients ?? "");
            object o = cmd.ExecuteScalar(); id = o != null ? (int)o : 0;
        }
        // havuzdan dus
        if (pool == "support" && minutes.HasValue) DeductSupportMinutes(companyId, minutes.Value);
        else if (hours.HasValue) DeductProjectHours(companyId, hours.Value);
        return id;
    }

    public static ProjectRecordRow GetProjectRecord(int recordId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT p.RecordId, p.CompanyId, c.Name AS CompanyName, p.CreatedByName, p.Content, p.Pool, p.Hours, p.Minutes, p.Recipients, p.CreatedAt " +
            "FROM dbo.ProjectRecords p JOIN dbo.Companies c ON c.CompanyId = p.CompanyId WHERE p.RecordId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", recordId);
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    ProjectRecordRow x = new ProjectRecordRow();
                    x.RecordId = (int)r["RecordId"]; x.CompanyId = (int)r["CompanyId"];
                    x.CompanyName = r["CompanyName"] as string; x.CreatedByName = r["CreatedByName"] as string;
                    x.Content = r["Content"] as string; x.Pool = r["Pool"] as string;
                    x.Hours = r["Hours"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Hours"]);
                    x.Minutes = r["Minutes"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["Minutes"]);
                    x.Recipients = r["Recipients"] as string; x.CreatedAt = (DateTime)r["CreatedAt"];
                    return x;
                }
        }
        return null;
    }

    public static System.Collections.Generic.List<ProjectRecordRow> GetProjectRecords(int companyIdOrZero)
    {
        var list = new System.Collections.Generic.List<ProjectRecordRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 500 p.RecordId, p.CompanyId, c.Name AS CompanyName, p.CreatedByName, p.Content, p.Pool, p.Hours, p.Minutes, p.Recipients, p.CreatedAt " +
            "FROM dbo.ProjectRecords p JOIN dbo.Companies c ON c.CompanyId = p.CompanyId " +
            (companyIdOrZero > 0 ? "WHERE p.CompanyId = @c " : "") + "ORDER BY p.CreatedAt DESC", con))
        {
            if (companyIdOrZero > 0) cmd.Parameters.AddWithValue("@c", companyIdOrZero);
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                {
                    ProjectRecordRow x = new ProjectRecordRow();
                    x.RecordId = (int)r["RecordId"]; x.CompanyId = (int)r["CompanyId"];
                    x.CompanyName = r["CompanyName"] as string; x.CreatedByName = r["CreatedByName"] as string;
                    x.Content = r["Content"] as string; x.Pool = r["Pool"] as string;
                    x.Hours = r["Hours"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Hours"]);
                    x.Minutes = r["Minutes"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["Minutes"]);
                    x.Recipients = r["Recipients"] as string; x.CreatedAt = (DateTime)r["CreatedAt"];
                    list.Add(x);
                }
        }
        return list;
    }

    public static System.Collections.Generic.List<UserRow> GetActiveUsersForCompany(int companyId)
    {
        var list = new System.Collections.Generic.List<UserRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT u.UserId, u.CompanyId, u.FullName, u.Email, u.Role, u.Permissions, u.LastLoginAt, u.AvatarImage, c.Name AS CompanyName, c.CompanyType " +
            "FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId WHERE u.CompanyId = @c AND u.IsActive = 1 ORDER BY u.FullName", con))
        { cmd.Parameters.AddWithValue("@c", companyId); using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) list.Add(ReadUserRow(r)); }
        return list;
    }

    // Hizmet Formu (Destek turu): cozulmus talep olustur + destek dakikasini dus
    public static int CreateServiceTicket(int companyId, int byUserId, string subject, string content, int minutes, string reqCompany, string byName, out string ticketNo)
    {
        ticketNo = ""; int ticketId = 0;
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.Tickets (CompanyId, OpenedByUserId, Product, Module, RequestType, Priority, Status, Subject, Source, RequesterCompany, ResolutionMinutes, SupportSeenAt, SupportSeenBy) " +
                "OUTPUT INSERTED.TicketId VALUES (@co, @by, N'Hizmet', N'', N'Hizmet Formu', N'Normal', N'Resolved', @subj, N'Hizmet', @rc, @min, SYSUTCDATETIME(), @bn)", con))
            {
                cmd.Parameters.AddWithValue("@co", companyId);
                cmd.Parameters.AddWithValue("@by", byUserId);
                cmd.Parameters.AddWithValue("@subj", (object)subject ?? "Hizmet kaydı");
                cmd.Parameters.AddWithValue("@rc", (object)reqCompany ?? "");
                cmd.Parameters.AddWithValue("@min", minutes);
                cmd.Parameters.AddWithValue("@bn", (object)byName ?? "");
                object o = cmd.ExecuteScalar(); ticketId = o != null ? (int)o : 0;
            }
            if (ticketId > 0)
            {
                ticketNo = FormatTicketNo(ticketId);
                using (SqlCommand up = new SqlCommand("UPDATE dbo.Tickets SET TicketNo = @no WHERE TicketId = @id", con))
                { up.Parameters.AddWithValue("@no", ticketNo); up.Parameters.AddWithValue("@id", ticketId); up.ExecuteNonQuery(); }
                using (SqlCommand m = new SqlCommand(
                    "INSERT INTO dbo.TicketMessages (TicketId, AuthorUserId, AuthorSide, Body, IsInternal, MsgKind) VALUES (@t, @by, N'Support', @b, 0, N'Reply')", con))
                { m.Parameters.AddWithValue("@t", ticketId); m.Parameters.AddWithValue("@by", byUserId); m.Parameters.AddWithValue("@b", (object)content ?? ""); m.ExecuteNonQuery(); }
            }
        }
        if (ticketId > 0) DeductSupportMinutes(companyId, minutes);
        return ticketId;
    }

    // Tek firma destek plani (dakika + tarih)
    public static SupportPlan GetSupportPlan(int companyId)
    {
        SupportPlan sp = new SupportPlan();
        var map = GetSupportPlanMap();
        if (map.ContainsKey(companyId)) return map[companyId];
        return sp;
    }

    public static void AssignTicket(int ticketId, int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.Tickets SET AssignedToUserId = @u, " +
            "Status = CASE WHEN Status = N'New' THEN N'InProgress' ELSE Status END, " +
            "UpdatedAt = SYSUTCDATETIME() WHERE TicketId = @id", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@id", ticketId);
            cmd.ExecuteNonQuery();
        }
    }

    // Durum degistir. Resolved -> ResolvedAt set + sayaci baslat. Reopen/diger -> ResolvedAt temizle.
    /// <summary>Personelin yazdigi gorunen durum (bos = sistem durumuna don)</summary>
    public static void SetTicketStatusNote(int ticketId, string note, int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Tickets SET StatusNote = @n, StatusNoteAt = SYSUTCDATETIME(), StatusNoteBy = @u, UpdatedAt = SYSUTCDATETIME() WHERE TicketId = @id", con))
        {
            cmd.Parameters.AddWithValue("@n", string.IsNullOrEmpty(note) ? (object)DBNull.Value : note);
            cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@id", ticketId);
            cmd.ExecuteNonQuery();
        }
    }

    public static void SetTicketStatus(int ticketId, string status)
    {
        string extra;
        if (status == "Resolved") extra = ", ResolvedAt = SYSUTCDATETIME(), ReminderSentAt = NULL";
        else if (status == "Closed") extra = ", ClosedAt = SYSUTCDATETIME()";
        else extra = ", ResolvedAt = NULL, ReminderSentAt = NULL";
        if (HasStatusNoteCol()) extra += ", StatusNote = CASE WHEN Status = @s THEN StatusNote ELSE NULL END";   // sistem durumu degisirse gorunen durum sifirlanir
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.Tickets SET Status = @s" + extra + ", UpdatedAt = SYSUTCDATETIME() WHERE TicketId = @id", con))
        {
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@id", ticketId);
            cmd.ExecuteNonQuery();
        }
    }

    // Otomatik kapanis: cozulmus + 4 gun gecmis + hatirlatma gonderilmemis
    public static List<Ticket> GetTicketsNeedingReminder()
    {
        List<Ticket> list = new List<Ticket>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TicketSelect +
            "WHERE t.Status = N'Resolved' AND t.ReminderSentAt IS NULL AND t.ResolvedAt <= DATEADD(day,-4,SYSUTCDATETIME())", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadTicket(r)); }
        return list;
    }

    // Cozulmus + 5 gun gecmis -> kapanacak
    public static List<Ticket> GetTicketsNeedingClose()
    {
        List<Ticket> list = new List<Ticket>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TicketSelect +
            // hatirlatma gittikten en az 1 gun sonra kapanir: gorev gunlerce calismasa bile musteri "1 gun icinde kapatilacak"
            // uyarisini alir ve o gun gercekten vardir (eskiden birikmis talepler ayni calistirmada hem uyarilip hem kapaniyordu)
            "WHERE t.Status = N'Resolved' AND t.ResolvedAt <= DATEADD(day,-5,SYSUTCDATETIME()) " +
            "AND t.ReminderSentAt IS NOT NULL AND t.ReminderSentAt <= DATEADD(day,-1,SYSUTCDATETIME())", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadTicket(r)); }
        return list;
    }

    public static void MarkReminderSent(int ticketId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Tickets SET ReminderSentAt = SYSUTCDATETIME() WHERE TicketId = @id", con))
        { cmd.Parameters.AddWithValue("@id", ticketId); cmd.ExecuteNonQuery(); }
    }

    // ================= /DESTEK =================

    // Bir bayinin kayitli musterileri (ticket formu acilir listesi)
    public static List<CompanyLite> GetPartnerCustomerNames(int companyId)
    {
        List<CompanyLite> list = new List<CompanyLite>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT RegistrationId, CustomerName FROM dbo.CustomerRegistrations " +
            "WHERE CompanyId = @c AND Status IN (N'Approved', N'Pending') ORDER BY CustomerName", con))
        {
            cmd.Parameters.AddWithValue("@c", companyId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) { CompanyLite x = new CompanyLite(); x.CompanyId = (int)r["RegistrationId"]; x.Name = r["CustomerName"] as string; list.Add(x); }
            }
        }
        return list;
    }

    // ---- Satış Materyalleri ----
    public static List<SalesMaterial> GetSalesMaterials()
    {
        List<SalesMaterial> list = new List<SalesMaterial>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT MaterialId, TitleTr, TitleEn, CategoryGroup, StoredName, OriginalName, ContentType, IsExternal, FileSize, CreatedAt " +
            "FROM dbo.SalesMaterials WHERE IsActive = 1 ORDER BY CategoryGroup ASC, CreatedAt DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                SalesMaterial m = new SalesMaterial();
                m.MaterialId = (int)r["MaterialId"];
                m.TitleTr = r["TitleTr"] as string;
                m.TitleEn = r["TitleEn"] as string;
                m.CategoryGroup = r["CategoryGroup"] == DBNull.Value ? "" : (string)r["CategoryGroup"];
                m.StoredName = r["StoredName"] as string;
                m.OriginalName = r["OriginalName"] as string;
                m.ContentType = r["ContentType"] as string;
                m.IsExternal = (bool)r["IsExternal"];
                m.FileSize = Convert.ToInt64(r["FileSize"]);
                m.CreatedAt = (DateTime)r["CreatedAt"];
                list.Add(m);
            }
        }
        return list;
    }

    public static void AddSalesMaterial(string titleTr, string titleEn, string group, string storedName, string originalName, string contentType, bool isExternal, long size)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.SalesMaterials (TitleTr, TitleEn, CategoryGroup, StoredName, OriginalName, ContentType, IsExternal, FileSize, IsActive) " +
            "VALUES (@tt, @te, @grp, @sn, @on, @ct, @ext, @sz, 1)", con))
        {
            cmd.Parameters.AddWithValue("@tt", (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@te", (object)titleEn ?? (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@grp", string.IsNullOrEmpty(group) ? (object)DBNull.Value : group);
            cmd.Parameters.AddWithValue("@sn", (object)storedName ?? "");
            cmd.Parameters.AddWithValue("@on", (object)originalName ?? "");
            cmd.Parameters.AddWithValue("@ct", (object)contentType ?? "");
            cmd.Parameters.AddWithValue("@ext", isExternal);
            cmd.Parameters.AddWithValue("@sz", size);
            cmd.ExecuteNonQuery();
        }
    }

    // Siler; diskten silinebilmesi icin StoredName doner (harici ise bos)
    public static string DeleteSalesMaterial(int id)
    {
        string stored = "";
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand g = new SqlCommand("SELECT StoredName, IsExternal FROM dbo.SalesMaterials WHERE MaterialId = @id", con))
            {
                g.Parameters.AddWithValue("@id", id);
                using (SqlDataReader r = g.ExecuteReader())
                {
                    if (r.Read() && !(bool)r["IsExternal"]) stored = r["StoredName"] as string;
                }
            }
            using (SqlCommand d = new SqlCommand("DELETE FROM dbo.SalesMaterials WHERE MaterialId = @id", con))
            {
                d.Parameters.AddWithValue("@id", id);
                d.ExecuteNonQuery();
            }
        }
        return stored;
    }

    public static void AddContentSuggestion(int? userId, int? companyId, string title, string body)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.ContentSuggestions (UserId, CompanyId, Title, Body) VALUES (@u, @c, @t, @b)", con))
        {
            cmd.Parameters.AddWithValue("@u", userId.HasValue ? (object)userId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@c", companyId.HasValue ? (object)companyId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@t", string.IsNullOrEmpty(title) ? (object)DBNull.Value : title);
            cmd.Parameters.AddWithValue("@b", (object)body ?? "");
            cmd.ExecuteNonQuery();
        }
    }

    // İzleme ilerlemesini kaydet (en uzak konum -> yüzde; %90+ tamamlandı)
    public static void SaveProgress(int materialId, int userId, int position, int duration)
    {
        try { SaveProgressCore(materialId, userId, position, duration, true); }
        catch (SqlException) { SaveProgressCore(materialId, userId, position, duration, false); } // LastPosition kolonu yoksa
    }

    // WatchedSeconds = en ileri konum (tamamlanma icin); LastPosition = son kalinan konum (kaldigi yerden devam icin)
    private static void SaveProgressCore(int materialId, int userId, int position, int duration, bool withLast)
    {
        if (position < 0) position = 0;
        int pct = duration > 0 ? (int)Math.Min(100, Math.Round(position * 100.0 / duration)) : 0;
        using (SqlConnection con = Db.Open())
        {
            int rows;
            using (SqlCommand up = new SqlCommand(
                "UPDATE dbo.TrainingProgress SET " +
                "WatchedSeconds = CASE WHEN @pos > WatchedSeconds THEN @pos ELSE WatchedSeconds END, " +
                (withLast ? "LastPosition = @pos, " : "") +
                "DurationSeconds = @dur, " +
                "[Percent] = CASE WHEN @pct > [Percent] THEN @pct ELSE [Percent] END, " +
                "Completed = CASE WHEN @pct >= 90 THEN 1 ELSE Completed END, " +
                "LastWatchedAt = SYSUTCDATETIME() WHERE MaterialId = @m AND UserId = @u", con))
            {
                up.Parameters.AddWithValue("@pos", position);
                up.Parameters.AddWithValue("@dur", duration);
                up.Parameters.AddWithValue("@pct", pct);
                up.Parameters.AddWithValue("@m", materialId);
                up.Parameters.AddWithValue("@u", userId);
                rows = up.ExecuteNonQuery();
            }
            if (rows == 0)
            {
                using (SqlCommand ins = new SqlCommand(
                    "INSERT INTO dbo.TrainingProgress (MaterialId, UserId, WatchedSeconds, " + (withLast ? "LastPosition, " : "") + "DurationSeconds, [Percent], Completed) " +
                    "VALUES (@m, @u, @pos, " + (withLast ? "@pos, " : "") + "@dur, @pct, @c)", con))
                {
                    ins.Parameters.AddWithValue("@m", materialId);
                    ins.Parameters.AddWithValue("@u", userId);
                    ins.Parameters.AddWithValue("@pos", position);
                    ins.Parameters.AddWithValue("@dur", duration);
                    ins.Parameters.AddWithValue("@pct", pct);
                    ins.Parameters.AddWithValue("@c", pct >= 90);
                    ins.ExecuteNonQuery();
                }
            }
        }
    }

    public static void AddWatchLog(int userId, int materialId, int seconds)
    {
        if (seconds <= 0) return;
        if (seconds > 3600) seconds = 3600;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.TrainingWatchLog (UserId, MaterialId, Seconds) VALUES (@u, @m, @s)", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", materialId);
            cmd.Parameters.AddWithValue("@s", seconds);
            cmd.ExecuteNonQuery();
        }
    }

    public static void IncrementViewCount(int materialId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.TrainingMaterials SET ViewCount = ISNULL(ViewCount,0) + 1 WHERE MaterialId = @m", con))
        { cmd.Parameters.AddWithValue("@m", materialId); cmd.ExecuteNonQuery(); }
    }

    public static void SaveRating(int materialId, int userId, int stars, string comment) { SaveRating(materialId, userId, stars, comment, true); }
    /// <summary>updateComment=false: yalniz yildiz guncellenir, mevcut yorum korunur (yildiza tiklayinca aninda kayit)</summary>
    public static void SaveRating(int materialId, int userId, int stars, string comment, bool updateComment)
    {
        if (stars < 1) stars = 1; if (stars > 5) stars = 5;
        using (SqlConnection con = Db.Open())
        {
            int rows;
            using (SqlCommand up = new SqlCommand(
                "UPDATE dbo.TrainingRatings SET Stars = @s" + (updateComment ? ", Comment = @c" : "") + ", UpdatedAt = SYSUTCDATETIME() " +
                "WHERE MaterialId = @m AND UserId = @u", con))
            {
                up.Parameters.AddWithValue("@s", stars);
                up.Parameters.AddWithValue("@c", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment);
                up.Parameters.AddWithValue("@m", materialId);
                up.Parameters.AddWithValue("@u", userId);
                rows = up.ExecuteNonQuery();
            }
            if (rows == 0)
            {
                using (SqlCommand ins = new SqlCommand(
                    "INSERT INTO dbo.TrainingRatings (MaterialId, UserId, Stars, Comment) VALUES (@m, @u, @s, @c)", con))
                {
                    ins.Parameters.AddWithValue("@m", materialId);
                    ins.Parameters.AddWithValue("@u", userId);
                    ins.Parameters.AddWithValue("@s", stars);
                    ins.Parameters.AddWithValue("@c", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment);
                    ins.ExecuteNonQuery();
                }
            }
        }
    }

    // Kullanicinin puanlari: MaterialId -> [stars, "comment"] (JS map icin)
    public static Dictionary<int, string[]> GetUserRatings(int userId)
    {
        Dictionary<int, string[]> map = new Dictionary<int, string[]>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT MaterialId, Stars, Comment FROM dbo.TrainingRatings WHERE UserId = @u", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                    map[(int)r["MaterialId"]] = new string[] { Convert.ToInt32(r["Stars"]).ToString(), r["Comment"] == DBNull.Value ? "" : (string)r["Comment"] };
            }
        }
        return map;
    }

    // Son 30 gunluk izleme: [dakika, farkli icerik sayisi]
    public static int[] GetMonthlyWatchStats(int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT ISNULL(SUM(Seconds),0) AS Secs, COUNT(DISTINCT MaterialId) AS Cnt " +
            "FROM dbo.TrainingWatchLog WHERE UserId = @u AND LoggedAt >= DATEADD(day,-30,SYSUTCDATETIME())", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                    return new int[] { (int)(Convert.ToInt64(r["Secs"]) / 60), Convert.ToInt32(r["Cnt"]) };
            }
        }
        return new int[] { 0, 0 };
    }

    // ---- Versiyon Duyuruları ----
    private static VersionAnnouncement ReadVersion(SqlDataReader r)
    {
        VersionAnnouncement v = new VersionAnnouncement();
        v.VersionId = (int)r["VersionId"];
        v.ProductName = r["ProductName"] as string;
        v.VersionNo = r["VersionNo"] as string;
        v.ReleaseDate = r["ReleaseDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ReleaseDate"];
        v.AccessDate = r["AccessDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["AccessDate"];
        v.Highlights = r["Highlights"] == DBNull.Value ? "" : (string)r["Highlights"];
        v.BodyText = r["BodyText"] == DBNull.Value ? "" : (string)r["BodyText"];
        v.IsPublished = Convert.ToInt32(r["IsPublished"]) == 1;
        v.CreatedAt = r["CreatedAt"] == DBNull.Value ? DateTime.UtcNow : (DateTime)r["CreatedAt"];
        v.PublishedAt = r["PublishedAt"] == DBNull.Value ? (DateTime?)null : (DateTime)r["PublishedAt"];
        return v;
    }

    private const string VA_SELECT =
        "SELECT VersionId, ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, " +
        "ISNULL(IsPublished,0) AS IsPublished, CreatedAt, PublishedAt FROM dbo.VersionAnnouncements ";

    public static List<VersionAnnouncement> GetPublishedVersions()
    {
        List<VersionAnnouncement> list = new List<VersionAnnouncement>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(VA_SELECT + "WHERE IsPublished = 1 ORDER BY ISNULL(PublishedAt, CreatedAt) DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadVersion(r)); }
        return list;
    }

    public static List<VersionAnnouncement> GetAllVersions()
    {
        List<VersionAnnouncement> list = new List<VersionAnnouncement>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(VA_SELECT + "ORDER BY CreatedAt DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadVersion(r)); }
        return list;
    }

    public static VersionAnnouncement GetVersion(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(VA_SELECT + "WHERE VersionId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) return ReadVersion(r); }
        }
        return null;
    }

    public static int AddVersion(string product, string versionNo, DateTime? release, DateTime? access, string highlights, string body, int userId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, CreatedByUserId) " +
            "OUTPUT INSERTED.VersionId VALUES (@p, @v, @r, @a, @h, @b, @u)", con))
        {
            cmd.Parameters.AddWithValue("@p", (object)product ?? "");
            cmd.Parameters.AddWithValue("@v", (object)versionNo ?? "");
            cmd.Parameters.AddWithValue("@r", (object)release ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@a", (object)access ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@h", string.IsNullOrEmpty(highlights) ? (object)DBNull.Value : highlights);
            cmd.Parameters.AddWithValue("@b", string.IsNullOrEmpty(body) ? (object)DBNull.Value : body);
            cmd.Parameters.AddWithValue("@u", userId);
            object id = cmd.ExecuteScalar();
            return id != null ? (int)id : 0;
        }
    }

    public static void PublishVersion(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.VersionAnnouncements SET IsPublished = 1, PublishedAt = SYSUTCDATETIME() WHERE VersionId = @id", con))
        { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }

    public static void DeleteVersion(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.VersionAnnouncements WHERE VersionId = @id", con))
        { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }

    // Tum aktif firmalara uygulama ici bildirim (tek sorgu)
    public static void NotifyAllCompanies(string titleTr, string titleEn, string bodyTr, string bodyEn, string type)
    {
        NotifyAllCompanies(titleTr, titleEn, bodyTr, bodyEn, type, null);
    }

    public static void NotifyAllCompanies(string titleTr, string titleEn, string bodyTr, string bodyEn, string type, string link)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.Notifications (CompanyId, TitleTr, TitleEn, BodyTr, BodyEn, Type, Link) " +
            "SELECT CompanyId, @tt, @te, @bt, @be, @ty, @ln FROM dbo.Companies WHERE IsActive = 1", con))
        {
            cmd.Parameters.AddWithValue("@tt", (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@te", (object)titleEn ?? "");
            cmd.Parameters.AddWithValue("@bt", (object)bodyTr ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@be", (object)bodyEn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ty", (object)type ?? "System");
            cmd.Parameters.AddWithValue("@ln", string.IsNullOrEmpty(link) ? (object)DBNull.Value : link);
            cmd.ExecuteNonQuery();
        }
    }

    // Belirli bir e-posta bildirim tercihi ACIK olan aktif kullanicilarin e-postalari
    // col: NotifyVersionEmail / NotifyTrainingEmail (kod kontrollu)
    public static List<string> GetActiveEmailsByPref(string col)
    {
        if (col != "NotifyVersionEmail" && col != "NotifyTrainingEmail") col = "NotifyVersionEmail";
        List<string> list = new List<string>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT DISTINCT u.Email FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId " +
            "WHERE u.IsActive = 1 AND c.IsActive = 1 AND u.Email IS NOT NULL AND u.Email <> '' AND ISNULL(u." + col + ",1) = 1", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) { string em = r[0] as string; if (!string.IsNullOrEmpty(em)) list.Add(em); } }
        return list;
    }

    public static bool[] GetNotifyPrefs(int userId)
    {
        bool v = true, t = true;
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(NotifyVersionEmail,1) AS V, ISNULL(NotifyTrainingEmail,1) AS T FROM dbo.Users WHERE UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", userId);
            using (SqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { v = Convert.ToBoolean(r["V"]); t = Convert.ToBoolean(r["T"]); } }
        }
        return new bool[] { v, t };
    }

    public static void UpdateNotifyPrefs(int userId, bool version, bool training)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET NotifyVersionEmail = @v, NotifyTrainingEmail = @t WHERE UserId = @id", con))
        {
            cmd.Parameters.AddWithValue("@v", version);
            cmd.Parameters.AddWithValue("@t", training);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }
    }

    // Tum aktif kullanicilarin e-postalari (versiyon duyuru maili icin)
    public static List<string> GetAllActiveEmails()
    {
        List<string> list = new List<string>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT DISTINCT u.Email FROM dbo.Users u JOIN dbo.Companies c ON c.CompanyId = u.CompanyId " +
            "WHERE u.IsActive = 1 AND c.IsActive = 1 AND u.Email IS NOT NULL AND u.Email <> ''", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) { string em = r[0] as string; if (!string.IsNullOrEmpty(em)) list.Add(em); } }
        return list;
    }

    // ---- Versiyon Duyuruları sonu ----

    // ---- Eğitim içerik yönetimi (admin / eğitim editörü) ----
    private static TrainingItem ReadTrainingItem(SqlDataReader r)
    {
        TrainingItem t = new TrainingItem();
        t.MaterialId = (int)r["MaterialId"];
        t.MaterialType = r["MaterialType"] as string;
        t.TitleTr = r["TitleTr"] as string;
        t.TitleEn = r["TitleEn"] as string;
        t.DescriptionTr = r["DescriptionTr"] == DBNull.Value ? "" : (string)r["DescriptionTr"];
        t.CategoryGroup = r["CategoryGroup"] == DBNull.Value ? "" : (string)r["CategoryGroup"];
        t.SubCategory = r["SubCategory"] == DBNull.Value ? "" : (string)r["SubCategory"];
        t.YoutubeId = r["YoutubeId"] == DBNull.Value ? "" : (string)r["YoutubeId"];
        t.DocOriginalName = r["DocOriginalName"] == DBNull.Value ? "" : (string)r["DocOriginalName"];
        t.IsActive = Convert.ToInt32(r["IsActive"]) == 1;
        t.IsFeatured = Convert.ToInt32(r["IsFeatured"]) == 1;
        t.FeatureText = r["FeatureText"] == DBNull.Value ? "" : (string)r["FeatureText"];
        t.CoverImage = r["CoverImage"] == DBNull.Value ? "" : (string)r["CoverImage"];
        return t;
    }

    private const string TITEM_SELECT =
        "SELECT MaterialId, MaterialType, TitleTr, TitleEn, DescriptionTr, CategoryGroup, SubCategory, YoutubeId, " +
        "DocOriginalName, ISNULL(IsActive,0) AS IsActive, ISNULL(IsFeatured,0) AS IsFeatured, FeatureText, CoverImage " +
        "FROM dbo.TrainingMaterials ";

    public static List<TrainingItem> GetTrainingManageList()
    {
        List<TrainingItem> list = new List<TrainingItem>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TITEM_SELECT +
            "WHERE MaterialType IN (N'Video', N'Document') ORDER BY MaterialType ASC, CategoryGroup ASC, SubCategory ASC, CreatedAt DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader()) { while (r.Read()) list.Add(ReadTrainingItem(r)); }
        return list;
    }

    public static TrainingItem GetTrainingItem(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(TITEM_SELECT + "WHERE MaterialId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) return ReadTrainingItem(r); }
        }
        return null;
    }

    /// <summary>Dokuman kapagini degistirir ya da kaldirir (null)</summary>
    public static void UpdateTrainingCover(int id, string cover)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TrainingMaterials SET CoverImage=@c WHERE MaterialId=@id AND MaterialType=N'Document'", con))
        { cmd.Parameters.AddWithValue("@c", (object)cover ?? DBNull.Value); cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }

    /// <summary>Videonun YouTube kimligini degistirir; kayitli sure sifirlanir (oynatici yeni videonun suresini ilk izlemede yazar).
    /// Bilgi tabani bu videoyu kaynak gosteriyorsa kayitlar yeni videoya baglanir.</summary>
    public static void UpdateTrainingVideoId(int id, string youtubeId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.TrainingMaterials SET YoutubeId=@y, DurationSeconds=0 WHERE MaterialId=@id AND MaterialType=N'Video'; " +
            "IF OBJECT_ID('dbo.KbEntries') IS NOT NULL UPDATE dbo.KbEntries SET YoutubeId=@y WHERE MaterialId=@id;", con))
        {
            cmd.Parameters.AddWithValue("@y", youtubeId); cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Bilgi tabaninda bu icerigi kaynak gosteren kayit sayisi (tablo yoksa 0)</summary>
    public static int KbEntryCountForMaterial(int id)
    {
        try
        {
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand("IF OBJECT_ID('dbo.KbEntries') IS NULL SELECT 0 ELSE SELECT COUNT(*) FROM dbo.KbEntries WHERE MaterialId=@id", con))
            { cmd.Parameters.AddWithValue("@id", id); return Convert.ToInt32(cmd.ExecuteScalar()); }
        }
        catch { return 0; }
    }

    public static void UpdateTrainingBasics(int id, string titleTr, string titleEn, string descTr, string group, string sub)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE dbo.TrainingMaterials SET TitleTr=@tt, TitleEn=@te, DescriptionTr=@d, CategoryGroup=@g, SubCategory=@s WHERE MaterialId=@id", con))
        {
            cmd.Parameters.AddWithValue("@tt", (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@te", string.IsNullOrEmpty(titleEn) ? (object)titleTr ?? "" : titleEn);
            cmd.Parameters.AddWithValue("@d", string.IsNullOrEmpty(descTr) ? (object)DBNull.Value : descTr);
            cmd.Parameters.AddWithValue("@g", string.IsNullOrEmpty(group) ? (object)DBNull.Value : group);
            cmd.Parameters.AddWithValue("@s", string.IsNullOrEmpty(sub) ? (object)DBNull.Value : sub);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public static void SetTrainingActive(int id, bool active)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TrainingMaterials SET IsActive=@a WHERE MaterialId=@id", con))
        { cmd.Parameters.AddWithValue("@a", active); cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }

    public static void SetTrainingFeatured(int id, bool featured, string featureText)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.TrainingMaterials SET IsFeatured=@f, FeatureText=@t WHERE MaterialId=@id", con))
        {
            cmd.Parameters.AddWithValue("@f", featured);
            cmd.Parameters.AddWithValue("@t", string.IsNullOrEmpty(featureText) ? (object)DBNull.Value : featureText);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    // ---- Eğitim: Dokümanlar ----
    public static List<TrainingDoc> GetTrainingDocuments()
    {
        List<TrainingDoc> list = new List<TrainingDoc>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT MaterialId, TitleTr, TitleEn, DescriptionTr, DescriptionEn, CategoryGroup, SubCategory, " +
            "DocStoredName, DocOriginalName, DocContentType, ISNULL(DocFileSize,0) AS DocFileSize, CoverImage, CreatedAt " +
            "FROM dbo.TrainingMaterials " +
            "WHERE MaterialType = N'Document' AND IsActive = 1 AND DocStoredName IS NOT NULL " +
            "ORDER BY CategoryGroup ASC, SubCategory ASC, SortOrder ASC, CreatedAt DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                TrainingDoc d = new TrainingDoc();
                d.MaterialId = (int)r["MaterialId"];
                d.TitleTr = r["TitleTr"] as string;
                d.TitleEn = r["TitleEn"] as string;
                d.DescriptionTr = r["DescriptionTr"] == DBNull.Value ? "" : (string)r["DescriptionTr"];
                d.DescriptionEn = r["DescriptionEn"] == DBNull.Value ? "" : (string)r["DescriptionEn"];
                d.CategoryGroup = r["CategoryGroup"] == DBNull.Value ? "" : (string)r["CategoryGroup"];
                d.SubCategory = r["SubCategory"] == DBNull.Value ? "" : (string)r["SubCategory"];
                d.StoredName = r["DocStoredName"] as string;
                d.OriginalName = r["DocOriginalName"] as string;
                d.ContentType = r["DocContentType"] as string;
                d.FileSize = Convert.ToInt64(r["DocFileSize"]);
                d.CoverImage = r["CoverImage"] == DBNull.Value ? "" : (string)r["CoverImage"];
                d.CreatedAt = r["CreatedAt"] == DBNull.Value ? DateTime.UtcNow : (DateTime)r["CreatedAt"];
                list.Add(d);
            }
        }
        return list;
    }

    public static int AddTrainingDocument(string titleTr, string titleEn, string group, string sub, string descTr,
        string stored, string original, string contentType, long size, string cover)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.TrainingMaterials (CompanyId, TitleTr, TitleEn, DescriptionTr, Category, CategoryGroup, SubCategory, " +
            "MaterialType, DocStoredName, DocOriginalName, DocContentType, DocFileSize, CoverImage, IsActive) " +
            "OUTPUT INSERTED.MaterialId " +
            "VALUES (NULL, @tt, @te, @d, N'Technical', @grp, @sub, N'Document', @sn, @on, @ct, @sz, @cov, 1)", con))
        {
            cmd.Parameters.AddWithValue("@tt", (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@te", (object)titleEn ?? (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@d", string.IsNullOrEmpty(descTr) ? (object)DBNull.Value : descTr);
            cmd.Parameters.AddWithValue("@grp", string.IsNullOrEmpty(group) ? (object)DBNull.Value : group);
            cmd.Parameters.AddWithValue("@sub", string.IsNullOrEmpty(sub) ? (object)DBNull.Value : sub);
            cmd.Parameters.AddWithValue("@sn", (object)stored ?? "");
            cmd.Parameters.AddWithValue("@on", (object)original ?? "");
            cmd.Parameters.AddWithValue("@ct", (object)contentType ?? "");
            cmd.Parameters.AddWithValue("@sz", size);
            cmd.Parameters.AddWithValue("@cov", string.IsNullOrEmpty(cover) ? (object)DBNull.Value : cover);
            object id = cmd.ExecuteScalar();
            return id != null ? (int)id : 0;
        }
    }

    // Dokuman sil; diskten silinebilmesi icin [DocStoredName, CoverImage] doner
    public static string[] DeleteTrainingDoc(int id)
    {
        string stored = "", cover = "";
        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand g = new SqlCommand("SELECT DocStoredName, CoverImage FROM dbo.TrainingMaterials WHERE MaterialId = @id", con))
            {
                g.Parameters.AddWithValue("@id", id);
                using (SqlDataReader r = g.ExecuteReader())
                {
                    if (r.Read()) { stored = r["DocStoredName"] as string; cover = r["CoverImage"] == DBNull.Value ? "" : (string)r["CoverImage"]; }
                }
            }
            using (SqlCommand d = new SqlCommand("DELETE FROM dbo.TrainingMaterials WHERE MaterialId = @id", con))
            { d.Parameters.AddWithValue("@id", id); d.ExecuteNonQuery(); }
        }
        return new string[] { stored ?? "", cover ?? "" };
    }

    // ---- Eğitim: Dokümanlar sonu ----

    // ---- River Akademi (video) ----
    public static List<TrainingVideo> GetTrainingVideos()
    {
        List<TrainingVideo> list = new List<TrainingVideo>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT m.MaterialId, m.TitleTr, m.TitleEn, m.DescriptionTr, m.DescriptionEn, m.Category, " +
            "m.CategoryGroup, m.SubCategory, m.YoutubeId, COALESCE(NULLIF(m.DurationSeconds,0), (SELECT MAX(p.DurationSeconds) FROM dbo.TrainingProgress p WHERE p.MaterialId = m.MaterialId), 0) AS DurationSeconds, " +
            "ISNULL(m.ViewCount,0) AS ViewCount, m.CreatedAt, ISNULL(m.IsFeatured,0) AS IsFeatured, m.FeatureText, " +
            "ISNULL(ra.Avg,0) AS AvgRating, ISNULL(ra.Cnt,0) AS RatingCount " +
            "FROM dbo.TrainingMaterials m " +
            "OUTER APPLY (SELECT AVG(CAST(Stars AS float)) AS Avg, COUNT(*) AS Cnt FROM dbo.TrainingRatings tr WHERE tr.MaterialId = m.MaterialId) ra " +
            "WHERE m.MaterialType = N'Video' AND m.IsActive = 1 AND m.YoutubeId IS NOT NULL " +
            "ORDER BY m.CategoryGroup ASC, m.SubCategory ASC, m.SortOrder ASC, m.CreatedAt DESC", con))
        {
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    TrainingVideo v = new TrainingVideo();
                    v.MaterialId = (int)r["MaterialId"];
                    v.TitleTr = r["TitleTr"] as string;
                    v.TitleEn = r["TitleEn"] as string;
                    v.DescriptionTr = r["DescriptionTr"] == DBNull.Value ? "" : (string)r["DescriptionTr"];
                    v.DescriptionEn = r["DescriptionEn"] == DBNull.Value ? "" : (string)r["DescriptionEn"];
                    v.Category = r["Category"] as string;
                    v.CategoryGroup = r["CategoryGroup"] == DBNull.Value ? "" : (string)r["CategoryGroup"];
                    v.SubCategory = r["SubCategory"] == DBNull.Value ? "" : (string)r["SubCategory"];
                    v.YoutubeId = r["YoutubeId"] as string;
                    v.DurationSeconds = Convert.ToInt32(r["DurationSeconds"]);
                    v.ViewCount = Convert.ToInt32(r["ViewCount"]);
                    v.CreatedAt = r["CreatedAt"] == DBNull.Value ? DateTime.UtcNow : (DateTime)r["CreatedAt"];
                    v.AvgRating = Convert.ToDouble(r["AvgRating"]);
                    v.RatingCount = Convert.ToInt32(r["RatingCount"]);
                    v.IsFeatured = Convert.ToInt32(r["IsFeatured"]) == 1;
                    v.FeatureText = r["FeatureText"] == DBNull.Value ? "" : (string)r["FeatureText"];
                    list.Add(v);
                }
            }
        }
        return list;
    }

    // Kullanicinin son 30 gunde izledigi icerikler (Egitim Gecmisim)
    public static List<TrainingHist> GetUserTrainingHistory(int userId)
    {
        List<TrainingHist> list = new List<TrainingHist>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT m.MaterialId, m.YoutubeId, m.TitleTr, m.TitleEn, ISNULL(SUM(w.Seconds),0) AS Secs, MAX(w.LoggedAt) AS LastAt, " +
            "ISNULL(MAX(tp.[Percent]),0) AS Pct, ISNULL(MAX(CAST(tp.Completed AS int)),0) AS Done " +
            "FROM dbo.TrainingWatchLog w " +
            "JOIN dbo.TrainingMaterials m ON m.MaterialId = w.MaterialId " +
            "LEFT JOIN dbo.TrainingProgress tp ON tp.MaterialId = w.MaterialId AND tp.UserId = w.UserId " +
            "WHERE w.UserId = @u AND w.LoggedAt >= DATEADD(day,-30,SYSUTCDATETIME()) " +
            "GROUP BY m.MaterialId, m.YoutubeId, m.TitleTr, m.TitleEn ORDER BY MAX(w.LoggedAt) DESC", con))
        {
            cmd.Parameters.AddWithValue("@u", userId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    TrainingHist h = new TrainingHist();
                    h.MaterialId = (int)r["MaterialId"];
                    h.YoutubeId = r["YoutubeId"] == DBNull.Value ? "" : (string)r["YoutubeId"];
                    h.TitleTr = r["TitleTr"] as string;
                    h.TitleEn = r["TitleEn"] as string;
                    h.Seconds = Convert.ToInt32(r["Secs"]);
                    h.Percent = Convert.ToInt32(r["Pct"]);
                    h.Completed = Convert.ToInt32(r["Done"]) == 1;
                    h.LastAt = r["LastAt"] == DBNull.Value ? DateTime.UtcNow : (DateTime)r["LastAt"];
                    list.Add(h);
                }
            }
        }
        return list;
    }

    // Kullanicinin izleme ilerlemesi: MaterialId -> (Percent, Completed)
    // MaterialId -> [yuzde, tamamlandi(0/1), son konum sn]
    public static Dictionary<int, int[]> GetUserProgress(int userId)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            bool withLast = attempt == 0;
            Dictionary<int, int[]> map = new Dictionary<int, int[]>();
            try
            {
                using (SqlConnection con = Db.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT MaterialId, [Percent], Completed" + (withLast ? ", ISNULL(LastPosition, 0) AS LastPos" : "") + " FROM dbo.TrainingProgress WHERE UserId = @u", con))
                {
                    cmd.Parameters.AddWithValue("@u", userId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            map[(int)r["MaterialId"]] = new int[] { Convert.ToInt32(r["Percent"]), (bool)r["Completed"] ? 1 : 0, withLast ? Convert.ToInt32(r["LastPos"]) : 0 };
                }
                return map;
            }
            catch (SqlException) { if (!withLast) throw; }
        }
        return new Dictionary<int, int[]>();
    }

    public static int AddTrainingVideo(string titleTr, string titleEn, string categoryGroup, string subCategory, string youtubeId)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO dbo.TrainingMaterials (CompanyId, TitleTr, TitleEn, Category, CategoryGroup, SubCategory, MaterialType, YoutubeId, IsActive) " +
            "OUTPUT INSERTED.MaterialId " +
            "VALUES (NULL, @tt, @te, N'Technical', @grp, @sub, N'Video', @yid, 1)", con))
        {
            cmd.Parameters.AddWithValue("@tt", (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@te", (object)titleEn ?? (object)titleTr ?? "");
            cmd.Parameters.AddWithValue("@grp", string.IsNullOrEmpty(categoryGroup) ? (object)DBNull.Value : categoryGroup);
            cmd.Parameters.AddWithValue("@sub", string.IsNullOrEmpty(subCategory) ? (object)DBNull.Value : subCategory);
            cmd.Parameters.AddWithValue("@yid", (object)youtubeId ?? "");
            object id = cmd.ExecuteScalar();
            return id != null ? (int)id : 0;
        }
    }

    public static void DeleteTrainingMaterial(int id)
    {
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.TrainingMaterials WHERE MaterialId = @id", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    // ---- Aylık seviye yeniden hesaplama (son 12 ay siparişleri) ----
    // Her partnerin son 365 gündeki (Reddedilmemiş) sipariş toplamına göre seviyesini günceller.
    // Seviye değişenlere bildirim gönderir. Değişen partner sayısını döner.
    public static int RecomputeTiers()
    {
        List<Tier> tiers = GetTiers();
        if (tiers.Count == 0) return 0;
        TierEngine engine = new TierEngine(tiers);

        // (companyId, currentTierId) listesi
        List<int[]> partners = new List<int[]>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT CompanyId, ISNULL(CurrentTierId,0) FROM dbo.Companies WHERE CompanyType = N'Partner' AND IsActive = 1", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read()) partners.Add(new int[] { (int)r[0], Convert.ToInt32(r[1]) });
        }

        int changed = 0;
        for (int i = 0; i < partners.Count; i++)
        {
            int companyId = partners[i][0];
            int oldTierId = partners[i][1];
            decimal rolling = 0m;
            using (SqlConnection con = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(SUM(Amount),0) FROM dbo.Deals " +
                "WHERE CompanyId = @c AND Status <> N'Rejected' AND BookedAt >= DATEADD(day,-365,SYSUTCDATETIME())", con))
            {
                cmd.Parameters.AddWithValue("@c", companyId);
                object o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value) rolling = Convert.ToDecimal(o);
            }

            Tier nt = engine.ResolveTier(rolling);
            int newTierId = nt.TierId;

            using (SqlConnection con = Db.Open())
            using (SqlCommand up = new SqlCommand(
                "UPDATE dbo.Companies SET AnnualNetRevenue = @rev, CurrentTierId = @t WHERE CompanyId = @c", con))
            {
                up.Parameters.AddWithValue("@rev", rolling);
                up.Parameters.AddWithValue("@t", newTierId);
                up.Parameters.AddWithValue("@c", companyId);
                up.ExecuteNonQuery();
            }

            if (newTierId != oldTierId && oldTierId != 0)
            {
                Tier ot = null;
                for (int k = 0; k < tiers.Count; k++) if (tiers[k].TierId == oldTierId) ot = tiers[k];
                bool up2 = ot == null || nt.SortOrder > ot.SortOrder;
                string tr = up2
                    ? "Tebrikler! Son 1 yıldaki satışlarınıza göre partner seviyeniz " + nt.Name("tr") + " oldu. Yeni komisyon oranlarınız uygulanmaktadır."
                    : "Son 1 yıldaki satışlarınıza göre partner seviyeniz " + nt.Name("tr") + " olarak güncellendi.";
                string en = up2
                    ? "Congratulations! Based on your last 12 months of sales, your partner tier is now " + nt.Name("en") + "."
                    : "Based on your last 12 months of sales, your partner tier is now " + nt.Name("en") + ".";
                AddNotification(companyId, "Partner seviyeniz güncellendi", "Your partner tier was updated", tr, en, "TierUpdate");
                changed++;
            }
        }
        return changed;
    }

    // ---- Aylık seviye sonu ----

    // ---- Vendor dashboard grafik verisi (JSON) ----
    private static string JEsc(string s)
    {
        if (s == null) return "";
        StringBuilder b = new StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '"') b.Append("\\\"");
            else if (c == '\\') b.Append("\\\\");
            else if (c == '<') b.Append("\\u003C");
            else if (c == '\n' || c == '\r' || c == '\t') b.Append(' ');
            else b.Append(c);
        }
        return b.ToString();
    }

    private static string Num(decimal d)
    {
        return d.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string TierTr(string code)
    {
        if (code == "Registered") return "Kayıtlı";
        if (code == "Silver") return "Silver";
        if (code == "Gold") return "Gold";
        if (code == "Platinum") return "Platinum";
        return code;
    }

    private static string DemoTr(string s)
    {
        if (s == "Approved") return "Onaylandı";
        if (s == "Rescheduled") return "Yeniden Planlandı";
        if (s == "Completed") return "Tamamlandı";
        if (s == "Rejected") return "Reddedildi";
        return "Beklemede";
    }

    public static string GetVendorChartsJson()
    {
        int partners = 0, deals = 0;
        decimal revenue = 0m, commission = 0m;

        List<string> tierLabels = new List<string>();
        List<int> tierData = new List<int>();
        List<string> topLabels = new List<string>();
        List<decimal> topData = new List<decimal>();
        List<string> dsLabels = new List<string>();
        List<int> dsData = new List<int>();
        List<string> demoLabels = new List<string>();
        List<int> demoData = new List<int>();

        using (SqlConnection con = Db.Open())
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT " +
                "(SELECT COUNT(*) FROM dbo.Companies WHERE CompanyType=N'Partner' AND IsActive=1), " +
                "(SELECT ISNULL(SUM(AnnualNetRevenue),0) FROM dbo.Companies WHERE CompanyType=N'Partner'), " +
                "(SELECT COUNT(*) FROM dbo.Deals), " +
                "(SELECT ISNULL(SUM(CommissionAmount),0) FROM dbo.Deals)", con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    partners = Convert.ToInt32(r[0]);
                    revenue = Convert.ToDecimal(r[1]);
                    deals = Convert.ToInt32(r[2]);
                    commission = Convert.ToDecimal(r[3]);
                }
            }

            using (SqlCommand cmd = new SqlCommand(
                "SELECT t.Code, COUNT(c.CompanyId) FROM dbo.PartnerTiers t " +
                "LEFT JOIN dbo.Companies c ON c.CurrentTierId=t.TierId AND c.CompanyType=N'Partner' AND c.IsActive=1 " +
                "GROUP BY t.TierId, t.Code, t.SortOrder ORDER BY t.SortOrder", con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) { tierLabels.Add(TierTr(r[0] as string)); tierData.Add(Convert.ToInt32(r[1])); }
            }

            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 7 Name, AnnualNetRevenue FROM dbo.Companies " +
                "WHERE CompanyType=N'Partner' AND IsActive=1 ORDER BY AnnualNetRevenue DESC", con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) { topLabels.Add(r[0] as string); topData.Add(Math.Round(Convert.ToDecimal(r[1]) / 1000000m, 2)); }
            }

            using (SqlCommand cmd = new SqlCommand(
                "SELECT Status, COUNT(*) FROM dbo.Deals GROUP BY Status", con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) { dsLabels.Add(Fmt.DealStLabel(r[0] as string, "tr")); dsData.Add(Convert.ToInt32(r[1])); }
            }

            using (SqlCommand cmd = new SqlCommand(
                "SELECT Status, COUNT(*) FROM dbo.DemoRequests GROUP BY Status", con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) { demoLabels.Add(DemoTr(r[0] as string)); demoData.Add(Convert.ToInt32(r[1])); }
            }
        }

        // Son 6 ay
        string[] monthsTr = new string[] { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };
        DateTime now = DateTime.Now;
        List<string> mLabels = new List<string>();
        List<int> mDeals = new List<int>();
        List<decimal> mRev = new List<decimal>();
        int[] yr = new int[6]; int[] mo = new int[6];
        for (int i = 0; i < 6; i++)
        {
            DateTime dt = now.AddMonths(-(5 - i));
            yr[i] = dt.Year; mo[i] = dt.Month;
            mLabels.Add(monthsTr[dt.Month - 1]);
            mDeals.Add(0); mRev.Add(0m);
        }
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT YEAR(BookedAt), MONTH(BookedAt), COUNT(*), ISNULL(SUM(Amount),0) FROM dbo.Deals " +
            "WHERE BookedAt >= DATEADD(month,-5,CAST(GETDATE() AS date)) " +
            "GROUP BY YEAR(BookedAt), MONTH(BookedAt)", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                int y = Convert.ToInt32(r[0]), m = Convert.ToInt32(r[1]);
                for (int i = 0; i < 6; i++)
                {
                    if (yr[i] == y && mo[i] == m)
                    {
                        mDeals[i] = Convert.ToInt32(r[2]);
                        mRev[i] = Math.Round(Convert.ToDecimal(r[3]) / 1000000m, 2);
                        break;
                    }
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append("\"totals\":{\"partners\":" + partners + ",\"revenue\":" + Num(Math.Round(revenue / 1000000m, 2)) +
                  ",\"deals\":" + deals + ",\"commission\":" + Num(Math.Round(commission / 1000000m, 2)) + "},");
        sb.Append("\"tiers\":" + Pair(tierLabels, tierData) + ",");
        sb.Append("\"top\":" + PairD(topLabels, topData) + ",");
        sb.Append("\"dealStatus\":" + Pair(dsLabels, dsData) + ",");
        sb.Append("\"demoStatus\":" + Pair(demoLabels, demoData) + ",");
        sb.Append("\"monthly\":{\"labels\":" + StrArr(mLabels) + ",\"deals\":" + IntArr(mDeals) + ",\"revenue\":" + DecArr(mRev) + "}");
        sb.Append("}");
        return sb.ToString();
    }

    private static string StrArr(List<string> a)
    {
        StringBuilder b = new StringBuilder(); b.Append("[");
        for (int i = 0; i < a.Count; i++) { if (i > 0) b.Append(","); b.Append("\"" + JEsc(a[i]) + "\""); }
        b.Append("]"); return b.ToString();
    }
    private static string IntArr(List<int> a)
    {
        StringBuilder b = new StringBuilder(); b.Append("[");
        for (int i = 0; i < a.Count; i++) { if (i > 0) b.Append(","); b.Append(a[i].ToString()); }
        b.Append("]"); return b.ToString();
    }
    private static string DecArr(List<decimal> a)
    {
        StringBuilder b = new StringBuilder(); b.Append("[");
        for (int i = 0; i < a.Count; i++) { if (i > 0) b.Append(","); b.Append(Num(a[i])); }
        b.Append("]"); return b.ToString();
    }
    private static string Pair(List<string> labels, List<int> data)
    {
        return "{\"labels\":" + StrArr(labels) + ",\"data\":" + IntArr(data) + "}";
    }
    private static string PairD(List<string> labels, List<decimal> data)
    {
        return "{\"labels\":" + StrArr(labels) + ",\"data\":" + DecArr(data) + "}";
    }

    // ---- Vendor: partner matrisi ----
    public static List<PartnerRow> GetPartners(string culture)
    {
        bool en = culture == "en";
        List<PartnerRow> list = new List<PartnerRow>();
        using (SqlConnection con = Db.Open())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT c.CompanyId, c.Name, c.AnnualNetRevenue, c.IsPassive, t.Code AS TierCode, " +
            "       t.NameTr, t.NameEn " +
            "FROM dbo.Companies c " +
            "LEFT JOIN dbo.PartnerTiers t ON t.TierId = c.CurrentTierId " +
            "WHERE c.CompanyType = N'Partner' AND c.IsActive = 1 " +
            "ORDER BY c.AnnualNetRevenue DESC", con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                PartnerRow p = new PartnerRow();
                p.CompanyId = (int)r["CompanyId"];
                p.Name = r["Name"] as string;
                p.Revenue = (decimal)r["AnnualNetRevenue"];
                p.IsPassive = (bool)r["IsPassive"];
                p.TierCode = r["TierCode"] == DBNull.Value ? "" : (string)r["TierCode"];
                object nm = en ? r["NameEn"] : r["NameTr"];
                p.TierName = nm == DBNull.Value ? "-" : (string)nm;
                list.Add(p);
            }
        }
        return list;
    }
}
