using System;

// Basit POCO modeller (C# 5 uyumlu; string interpolation / ?. / expression-body YOK).

public class Tier
{
    public int TierId { get; set; }
    public string Code { get; set; }
    public string NameTr { get; set; }
    public string NameEn { get; set; }
    public decimal MinRevenue { get; set; }
    public decimal? MaxRevenue { get; set; }
    public decimal BaseCommission { get; set; }
    public string SlaTr { get; set; }
    public string SlaEn { get; set; }
    public int SortOrder { get; set; }

    public string Name(string culture) { return culture == "en" ? NameEn : NameTr; }
    public string Sla(string culture) { return culture == "en" ? SlaEn : SlaTr; }
}

public class ApiClientInfo
{
    public int ApiClientId { get; set; }
    public string Name { get; set; }
    public int CompanyId { get; set; }
    public string ClientId { get; set; }
}
public class SupportPlan
{
    public int Minutes { get; set; }
    public DateTime? Until { get; set; }
    public decimal ProjectHours { get; set; }
}
public class ProjectRecordRow
{
    public int RecordId { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string CreatedByName { get; set; }
    public string Content { get; set; }
    public string Pool { get; set; }
    public decimal? Hours { get; set; }
    public int? Minutes { get; set; }
    public string Recipients { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class CompanyLite
{
    public int CompanyId { get; set; }
    public string Name { get; set; }
}

public class Ticket
{
    public int TicketId { get; set; }
    public string TicketNo { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string CompanyType { get; set; }
    public int OpenedByUserId { get; set; }
    public string OpenedByName { get; set; }
    public string Product { get; set; }
    public string Module { get; set; }
    public string RequestType { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public string CustomerName { get; set; }
    public string Version { get; set; }
    public string Environment { get; set; }
    public string Subject { get; set; }
    public string Phone { get; set; }
    public int? AssignedToUserId { get; set; }
    public string AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ResolutionMinutes { get; set; }
    public string Source { get; set; }
    public string ExternalProduct { get; set; }
    public string ExternalRef { get; set; }
    public string RequesterCompany { get; set; }
    public string RequesterName { get; set; }
    public string RequesterEmail { get; set; }
    public string RequesterPhone { get; set; }
    public string StatusNote { get; set; }   // personelin yazdigi gorunen durum; bos ise sistem durumu gosterilir
}

public class TicketMessage
{
    public int MessageId { get; set; }
    public int TicketId { get; set; }
    public int? AuthorUserId { get; set; }
    public string AuthorName { get; set; }
    public string AuthorSide { get; set; }
    public string Body { get; set; }
    public bool IsInternal { get; set; }
    public string MsgKind { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketAttachment
{
    public int AttachmentId { get; set; }
    public int TicketId { get; set; }
    public int? MessageId { get; set; }
    public string StoredName { get; set; }
    public string OriginalName { get; set; }
    public long FileSize { get; set; }
    public string Href() { return "ticketfiles/" + StoredName; }
}

public class CompanyInfo
{
    public int CompanyId { get; set; }
    public string Name { get; set; }
    public bool IsVendor { get; set; }
    public int? CurrentTierId { get; set; }
    public decimal AnnualNetRevenue { get; set; }
    public bool IsPassive { get; set; }
}

public class UserAccount
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }             // VendorAdmin / PartnerAdmin / PartnerUser
    public string Permissions { get; set; }      // vendor coklu izin: Portal,Editor,Support
    public string PreferredCulture { get; set; } // null olabilir
    public bool IsActive { get; set; }
}

public class DealRow
{
    public int DealId { get; set; }
    public int CompanyId { get; set; }
    public string Title { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; }
    public DateTime BookedAt { get; set; }
}

public class DemoRow
{
    public int DemoRequestId { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string ClientName { get; set; }
    public string ProductModule { get; set; }
    public string Status { get; set; }
    public DateTime? PreferredDate { get; set; }
    public DateTime? AltDate1 { get; set; }
    public DateTime? AltDate2 { get; set; }
    public DateTime? ScheduledDate { get; set; }
}

public class SalesMaterial
{
    public int MaterialId { get; set; }
    public string TitleTr { get; set; }
    public string TitleEn { get; set; }
    public string CategoryGroup { get; set; }
    public string StoredName { get; set; }
    public string OriginalName { get; set; }
    public string ContentType { get; set; }
    public bool IsExternal { get; set; }
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Title(string culture) { return culture == "en" && !string.IsNullOrEmpty(TitleEn) ? TitleEn : TitleTr; }
    public string Group() { return string.IsNullOrEmpty(CategoryGroup) ? "Genel" : CategoryGroup; }
    public string Href() { return IsExternal ? StoredName : "materials/" + StoredName; }
}

public class UserRow
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string CompanyName { get; set; }
    public string CompanyType { get; set; }
    public string AvatarImage { get; set; }
    public string Permissions { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class NotifRow
{
    public int NotificationId { get; set; }
    public string TitleTr { get; set; }
    public string TitleEn { get; set; }
    public string BodyTr { get; set; }
    public string BodyEn { get; set; }
    public bool IsRead { get; set; }
    public string Link { get; set; }
    public DateTime? CreatedAt { get; set; }   // UTC
    public string Type { get; set; }

    public string Title(string culture) { return culture == "en" ? TitleEn : TitleTr; }
    public string Body(string culture) { return culture == "en" ? BodyEn : BodyTr; }
}

public class PartnerRow
{
    public int CompanyId { get; set; }
    public string Name { get; set; }
    public string TierCode { get; set; }
    public string TierName { get; set; }
    public decimal Revenue { get; set; }
    public bool IsPassive { get; set; }
}

public class VersionAnnouncement
{
    public int VersionId { get; set; }
    public string ProductName { get; set; }
    public string VersionNo { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? AccessDate { get; set; }
    public string Highlights { get; set; }
    public string BodyText { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class TrainingItem
{
    public int MaterialId { get; set; }
    public string MaterialType { get; set; }   // Video / Document
    public string TitleTr { get; set; }
    public string TitleEn { get; set; }
    public string DescriptionTr { get; set; }
    public string CategoryGroup { get; set; }
    public string SubCategory { get; set; }
    public string YoutubeId { get; set; }
    public string DocOriginalName { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string FeatureText { get; set; }
    public string CoverImage { get; set; }   // dokuman kapagi (assets/traincovers altindaki dosya adi)
    public string TitleAny() { return string.IsNullOrEmpty(TitleTr) ? TitleEn : TitleTr; }
}

public class TrainingHist
{
    public int MaterialId { get; set; }
    public string YoutubeId { get; set; }
    public string TitleTr { get; set; }
    public string TitleEn { get; set; }
    public int Seconds { get; set; }
    public int Percent { get; set; }
    public bool Completed { get; set; }
    public DateTime LastAt { get; set; }
    public string Title(string c) { return c == "en" && !string.IsNullOrEmpty(TitleEn) ? TitleEn : TitleTr; }
}

public class TrainingDoc
{
    public int MaterialId { get; set; }
    public string TitleTr { get; set; }
    public string TitleEn { get; set; }
    public string DescriptionTr { get; set; }
    public string DescriptionEn { get; set; }
    public string CategoryGroup { get; set; }
    public string SubCategory { get; set; }
    public string StoredName { get; set; }
    public string OriginalName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string CoverImage { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Title(string c) { return c == "en" && !string.IsNullOrEmpty(TitleEn) ? TitleEn : TitleTr; }
    public string Desc(string c) { return c == "en" && !string.IsNullOrEmpty(DescriptionEn) ? DescriptionEn : (DescriptionTr ?? ""); }
    public string Group() { return string.IsNullOrEmpty(CategoryGroup) ? "Genel" : CategoryGroup; }
    public string Sub() { return string.IsNullOrEmpty(SubCategory) ? "Genel" : SubCategory; }
    public string Href() { return "TrainingFile.ashx?id=" + MaterialId; }          // giris kontrollu dosya ucu
    public string DownloadHref() { return "TrainingFile.ashx?id=" + MaterialId + "&dl=1"; }
    public string Kind() { return TrainingFiles.Kind("." + Ext()); }                  // tarayici goruntuleyici turu
    public string Ext()
    {
        string n = OriginalName ?? "";
        int i = n.LastIndexOf('.');
        return i >= 0 ? n.Substring(i + 1).ToUpperInvariant() : "DOSYA";
    }
    public bool IsPdf() { return Ext() == "PDF"; }
    public string SizeStr()
    {
        if (FileSize <= 0) return "";
        double mb = FileSize / 1048576.0;
        if (mb >= 1) return mb.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " MB";
        return (FileSize / 1024.0).ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " KB";
    }
}

public class TrainingVideo
{
    public int MaterialId { get; set; }
    public string TitleTr { get; set; }
    public string TitleEn { get; set; }
    public string DescriptionTr { get; set; }
    public string DescriptionEn { get; set; }
    public string Category { get; set; }
    public string CategoryGroup { get; set; }
    public string SubCategory { get; set; }
    public string YoutubeId { get; set; }
    public int DurationSeconds { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    // Calisma aninda doldurulur
    public double AvgRating { get; set; }
    public int RatingCount { get; set; }
    public int ProgressPercent { get; set; }
    public bool Completed { get; set; }
    public bool IsFeatured { get; set; }
    public string FeatureText { get; set; }
    public string Title(string culture) { return culture == "en" && !string.IsNullOrEmpty(TitleEn) ? TitleEn : TitleTr; }
    public string Desc(string culture) { return culture == "en" && !string.IsNullOrEmpty(DescriptionEn) ? DescriptionEn : (DescriptionTr ?? ""); }
    public string Group() { return string.IsNullOrEmpty(CategoryGroup) ? "Genel" : CategoryGroup; }
    public string Sub() { return string.IsNullOrEmpty(SubCategory) ? "Genel" : SubCategory; }
}

public class DemoContact
{
    public string ClientName { get; set; }
    public string ProductModule { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string ToName { get; set; }
    public DateTime? ScheduledDate { get; set; }
}

public class CustomerReg
{
    public int RegistrationId { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string CustomerName { get; set; }
    public string ContactPerson { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string ServiceType { get; set; }
    public int SalesTemperature { get; set; }
    public string Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RenewalRequested { get; set; }

    // Kalan gun (Approved ve suresi dolmamissa). Aksi halde 0.
    public int RemainingDays
    {
        get
        {
            if (ExpiresAt == null) return 0;
            TimeSpan diff = ExpiresAt.Value - DateTime.UtcNow;
            int d = (int)Math.Ceiling(diff.TotalDays);
            return d < 0 ? 0 : d;
        }
    }

    // Suresi doldu mu (Approved ama ExpiresAt gecmis)
    public bool IsExpired
    {
        get { return ExpiresAt != null && ExpiresAt.Value <= DateTime.UtcNow; }
    }
}
