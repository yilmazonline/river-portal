/* ============================================================================
   RIVER PARTNER & VENDOR PORTAL — VERİTABANI ŞEMASI (MS SQL Server / T-SQL)
   ----------------------------------------------------------------------------
   Çok kiracılı (multi-tenant) yapı: her kayıt bir CompanyId'ye (Tenant) bağlıdır.
   Vendor (üretici) firması da bir Company kaydıdır ve IsVendor = 1 taşır.
   Para birimleri decimal(18,2) TL olarak tutulur.
   Uygulama katmanında (EF Core global query filter) tenant izolasyonu ayrıca
   zorlanır; buradaki şema veri bütünlüğünü garanti eder.
   ============================================================================ */

IF DB_ID('PARTNER') IS NULL
    CREATE DATABASE PARTNER;
GO
USE PARTNER;
GO

/* --------------------------------------------------------------------------
   1) COMPANIES  (Tenant / Firma)
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Companies') IS NOT NULL DROP TABLE dbo.Companies;
GO
CREATE TABLE dbo.Companies
(
    CompanyId          INT IDENTITY(1,1) NOT NULL,
    Name               NVARCHAR(200)     NOT NULL,
    TaxNumber          NVARCHAR(20)      NULL,
    IsVendor           BIT               NOT NULL CONSTRAINT DF_Companies_IsVendor DEFAULT(0),
    -- Kademe (tier) durumu: TierEngine tarafından yıllık hakedişe göre güncellenir
    CurrentTierId      INT               NULL,
    AnnualNetRevenue   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Companies_AnnualRev DEFAULT(0),
    -- Disiplin: 12 ay hareketsizlik kuralı
    IsPassive          BIT               NOT NULL CONSTRAINT DF_Companies_IsPassive DEFAULT(0),
    LastSaleDate       DATETIME2(0)      NULL,
    -- Kültür/dil tercihi: 'tr' (varsayılan) veya 'en'
    PreferredCulture   NVARCHAR(5)       NOT NULL CONSTRAINT DF_Companies_Culture DEFAULT(N'tr'),
    CreatedAt          DATETIME2(0)      NOT NULL CONSTRAINT DF_Companies_Created DEFAULT(SYSUTCDATETIME()),
    IsActive           BIT               NOT NULL CONSTRAINT DF_Companies_IsActive DEFAULT(1),
    CONSTRAINT PK_Companies PRIMARY KEY CLUSTERED (CompanyId)
);
GO
CREATE INDEX IX_Companies_IsVendor    ON dbo.Companies(IsVendor) INCLUDE(Name);
CREATE INDEX IX_Companies_Tier        ON dbo.Companies(CurrentTierId) INCLUDE(AnnualNetRevenue);
CREATE INDEX IX_Companies_Passive     ON dbo.Companies(IsPassive, LastSaleDate);
GO

/* --------------------------------------------------------------------------
   2) PARTNER TIERS  (Kademe tanımları — konfigürasyon tablosu)
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.PartnerTiers') IS NOT NULL DROP TABLE dbo.PartnerTiers;
GO
CREATE TABLE dbo.PartnerTiers
(
    TierId          INT IDENTITY(1,1) NOT NULL,
    Code            NVARCHAR(20)      NOT NULL,   -- Registered / Silver / Gold / Platinum
    NameTr          NVARCHAR(60)      NOT NULL,
    NameEn          NVARCHAR(60)      NOT NULL,
    MinRevenue      DECIMAL(18,2)     NOT NULL,   -- alt eşik (dahil)
    MaxRevenue      DECIMAL(18,2)     NULL,       -- üst eşik (hariç); NULL = sınırsız
    BaseCommission  DECIMAL(5,2)      NOT NULL,   -- taban komisyon % (ör. 30.00)
    SlaTr           NVARCHAR(80)      NOT NULL,
    SlaEn           NVARCHAR(80)      NOT NULL,
    SortOrder       INT               NOT NULL,
    CONSTRAINT PK_PartnerTiers PRIMARY KEY CLUSTERED (TierId),
    CONSTRAINT UQ_PartnerTiers_Code UNIQUE (Code)
);
GO

/* Companies -> PartnerTiers FK (tier tablosu oluştuğu için şimdi ekliyoruz) */
ALTER TABLE dbo.Companies
    ADD CONSTRAINT FK_Companies_Tier FOREIGN KEY (CurrentTierId)
        REFERENCES dbo.PartnerTiers(TierId);
GO

/* --------------------------------------------------------------------------
   3) USERS  (Kullanıcılar + RBAC)
   Roller: VendorAdmin / PartnerAdmin / PartnerUser
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Users') IS NOT NULL DROP TABLE dbo.Users;
GO
CREATE TABLE dbo.Users
(
    UserId           INT IDENTITY(1,1) NOT NULL,
    CompanyId        INT               NOT NULL,
    FullName         NVARCHAR(150)     NOT NULL,
    Email            NVARCHAR(256)     NOT NULL,
    PasswordHash     NVARCHAR(256)     NOT NULL,
    Role             NVARCHAR(20)      NOT NULL,   -- VendorAdmin / PartnerAdmin / PartnerUser
    PreferredCulture NVARCHAR(5)       NULL,       -- boşsa firmanın / sistemin varsayılanı ('tr')
    IsActive         BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
    LastLoginAt      DATETIME2(0)      NULL,
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Users_Created DEFAULT(SYSUTCDATETIME()),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'VendorAdmin', N'PartnerAdmin', N'PartnerUser'))
);
GO
CREATE INDEX IX_Users_Company ON dbo.Users(CompanyId) INCLUDE(Role, IsActive);
GO

/* --------------------------------------------------------------------------
   4) DEALS  (Anlaşmalar — tek işlemler; hakediş & mega deal hesabı burada)
   Kademe geçiş kuralı: her deal'in taban oranı, o deal BOOKED edilirken
   firmanın o ana kadarki kümülatif yıllık cirosuna göre belirlenir.
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Deals') IS NOT NULL DROP TABLE dbo.Deals;
GO
CREATE TABLE dbo.Deals
(
    DealId             INT IDENTITY(1,1) NOT NULL,
    CompanyId          INT               NOT NULL,   -- partner firması (tenant)
    Title              NVARCHAR(200)     NOT NULL,
    CustomerName       NVARCHAR(200)     NOT NULL,
    Amount             DECIMAL(18,2)     NOT NULL,   -- anlaşma net tutarı (TL)
    -- Hesaplanan alanlar (TierEngine dolduruyor):
    AppliedTierId      INT               NULL,       -- deal anında geçerli olan kademe
    BaseCommissionRate DECIMAL(5,2)      NOT NULL CONSTRAINT DF_Deals_Base DEFAULT(0),  -- %
    MegaDealBonusRate  DECIMAL(5,2)      NOT NULL CONSTRAINT DF_Deals_Mega DEFAULT(0),  -- %
    EffectiveRate      AS (BaseCommissionRate + MegaDealBonusRate) PERSISTED,           -- toplam %
    CommissionAmount   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Deals_Comm DEFAULT(0),  -- hakediş (TL)
    Status             NVARCHAR(20)      NOT NULL CONSTRAINT DF_Deals_Status DEFAULT(N'Pending'),
    BookedAt           DATETIME2(0)      NOT NULL CONSTRAINT DF_Deals_Booked DEFAULT(SYSUTCDATETIME()),
    CreatedByUserId    INT               NULL,       -- partner veya (on-behalf) vendor kullanıcısı
    CreatedByVendor    BIT               NOT NULL CONSTRAINT DF_Deals_ByVendor DEFAULT(0),
    CONSTRAINT PK_Deals PRIMARY KEY CLUSTERED (DealId),
    CONSTRAINT FK_Deals_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
    CONSTRAINT FK_Deals_Tier    FOREIGN KEY (AppliedTierId) REFERENCES dbo.PartnerTiers(TierId),
    CONSTRAINT FK_Deals_User    FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Deals_Status  CHECK (Status IN (N'Pending', N'Approved', N'Invoiced', N'Rejected')),
    CONSTRAINT CK_Deals_Amount  CHECK (Amount >= 0)
);
GO
-- Yıllık kümülatif ciro / kademe hesabı bu index üzerinden hızlı yürür:
CREATE INDEX IX_Deals_Company_Booked ON dbo.Deals(CompanyId, BookedAt)
    INCLUDE(Amount, CommissionAmount, Status);
CREATE INDEX IX_Deals_Status ON dbo.Deals(Status) INCLUDE(CompanyId, Amount);
GO

/* --------------------------------------------------------------------------
   5) ORDERS  (Sipariş defteri — lisans/servis siparişleri)
   Deal onaylandıktan sonra veya vendor tarafından on-behalf girilebilir.
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Orders') IS NOT NULL DROP TABLE dbo.Orders;
GO
CREATE TABLE dbo.Orders
(
    OrderId          INT IDENTITY(1,1) NOT NULL,
    CompanyId        INT               NOT NULL,
    DealId           INT               NULL,
    OrderNumber      NVARCHAR(40)      NOT NULL,
    ProductModule    NVARCHAR(120)     NOT NULL,   -- No-Code ürün modülü
    Amount           DECIMAL(18,2)     NOT NULL,
    Status           NVARCHAR(20)      NOT NULL CONSTRAINT DF_Orders_Status DEFAULT(N'Pending'),
    IsServiceSale    BIT               NOT NULL CONSTRAINT DF_Orders_Service DEFAULT(0),
    EnteredByVendor  BIT               NOT NULL CONSTRAINT DF_Orders_ByVendor DEFAULT(0),
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Orders_Created DEFAULT(SYSUTCDATETIME()),
    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (OrderId),
    CONSTRAINT UQ_Orders_Number UNIQUE (OrderNumber),
    CONSTRAINT FK_Orders_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
    CONSTRAINT FK_Orders_Deal    FOREIGN KEY (DealId)    REFERENCES dbo.Deals(DealId),
    CONSTRAINT CK_Orders_Status  CHECK (Status IN (N'Pending', N'Approved', N'Invoiced', N'Cancelled'))
);
GO
CREATE INDEX IX_Orders_Company ON dbo.Orders(CompanyId, CreatedAt) INCLUDE(Status, Amount);
GO

/* --------------------------------------------------------------------------
   6) DEMO REQUESTS  (Demo talebi yaşam döngüsü)
   Pending -> Approved/Rescheduled -> Completed
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.DemoRequests') IS NOT NULL DROP TABLE dbo.DemoRequests;
GO
CREATE TABLE dbo.DemoRequests
(
    DemoRequestId    INT IDENTITY(1,1) NOT NULL,
    CompanyId        INT               NOT NULL,   -- talebi açan partner
    RequestedByUserId INT              NULL,
    ClientName       NVARCHAR(200)     NOT NULL,
    ClientContact    NVARCHAR(200)     NULL,
    ProductModule    NVARCHAR(120)     NOT NULL,
    PreferredDate    DATETIME2(0)      NULL,
    AltDate1         DATETIME2(0)      NULL,       -- alternatif tarih/saat 1
    AltDate2         DATETIME2(0)      NULL,       -- alternatif tarih/saat 2
    ScheduledDate    DATETIME2(0)      NULL,       -- vendor onaylayınca / reschedule edince
    EnvironmentInfo  NVARCHAR(500)     NULL,       -- vendor demo ortam bilgisi
    Status           NVARCHAR(20)      NOT NULL CONSTRAINT DF_Demo_Status DEFAULT(N'Pending'),
    ApprovedByUserId INT               NULL,       -- VendorAdmin
    Notes            NVARCHAR(1000)    NULL,
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Demo_Created DEFAULT(SYSUTCDATETIME()),
    UpdatedAt        DATETIME2(0)      NULL,
    CONSTRAINT PK_DemoRequests PRIMARY KEY CLUSTERED (DemoRequestId),
    CONSTRAINT FK_Demo_Company  FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
    CONSTRAINT FK_Demo_ReqUser  FOREIGN KEY (RequestedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Demo_AppUser  FOREIGN KEY (ApprovedByUserId)  REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Demo_Status   CHECK (Status IN (N'Pending', N'Approved', N'Rescheduled', N'Completed', N'Rejected'))
);
GO
CREATE INDEX IX_Demo_Company ON dbo.DemoRequests(CompanyId, Status) INCLUDE(CreatedAt);
CREATE INDEX IX_Demo_Pending ON dbo.DemoRequests(Status) INCLUDE(CompanyId, CreatedAt);
GO

/* --------------------------------------------------------------------------
   7) QUOTE REQUESTS  (Fiyat teklifi talepleri — demo ile benzer döngü)
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.QuoteRequests') IS NOT NULL DROP TABLE dbo.QuoteRequests;
GO
CREATE TABLE dbo.QuoteRequests
(
    QuoteRequestId   INT IDENTITY(1,1) NOT NULL,
    CompanyId        INT               NOT NULL,
    RequestedByUserId INT              NULL,
    ClientName       NVARCHAR(200)     NOT NULL,
    ProductModule    NVARCHAR(120)     NOT NULL,
    EstimatedAmount  DECIMAL(18,2)     NULL,
    Status           NVARCHAR(20)      NOT NULL CONSTRAINT DF_Quote_Status DEFAULT(N'Pending'),
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Quote_Created DEFAULT(SYSUTCDATETIME()),
    CONSTRAINT PK_QuoteRequests PRIMARY KEY CLUSTERED (QuoteRequestId),
    CONSTRAINT FK_Quote_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
    CONSTRAINT CK_Quote_Status  CHECK (Status IN (N'Pending', N'Approved', N'Completed', N'Rejected'))
);
GO
CREATE INDEX IX_Quote_Company ON dbo.QuoteRequests(CompanyId, Status);
GO

/* --------------------------------------------------------------------------
   8) NOTIFICATIONS  (Bildirim merkezi — zil ikonu / okunmamış sayacı)
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Notifications') IS NOT NULL DROP TABLE dbo.Notifications;
GO
CREATE TABLE dbo.Notifications
(
    NotificationId   INT IDENTITY(1,1) NOT NULL,
    CompanyId        INT               NOT NULL,
    TargetUserId     INT               NULL,       -- NULL = firmadaki tüm kullanıcılar
    -- Metni iki dilde de tutuyoruz; UI aktif kültüre göre gösterir
    TitleTr          NVARCHAR(200)     NOT NULL,
    TitleEn          NVARCHAR(200)     NOT NULL,
    BodyTr           NVARCHAR(1000)    NULL,
    BodyEn           NVARCHAR(1000)    NULL,
    Type             NVARCHAR(30)      NOT NULL,    -- DemoApproved / OrderStatus / TierUpgrade / System
    LinkUrl          NVARCHAR(300)     NULL,
    IsRead           BIT               NOT NULL CONSTRAINT DF_Notif_Read DEFAULT(0),
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Notif_Created DEFAULT(SYSUTCDATETIME()),
    ReadAt           DATETIME2(0)      NULL,
    CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (NotificationId),
    CONSTRAINT FK_Notif_Company FOREIGN KEY (CompanyId)   REFERENCES dbo.Companies(CompanyId),
    CONSTRAINT FK_Notif_User    FOREIGN KEY (TargetUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Notif_Unread ON dbo.Notifications(CompanyId, TargetUserId, IsRead) INCLUDE(CreatedAt);
GO

/* --------------------------------------------------------------------------
   9) TRAINING MATERIALS  (LMS — video playlisti + doküman merkezi)
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.TrainingMaterials') IS NOT NULL DROP TABLE dbo.TrainingMaterials;
GO
CREATE TABLE dbo.TrainingMaterials
(
    MaterialId       INT IDENTITY(1,1) NOT NULL,
    -- LMS içerikleri tüm partnerlere açıktır; CompanyId NULL = global (vendor yayını)
    CompanyId        INT               NULL,
    TitleTr          NVARCHAR(200)     NOT NULL,
    TitleEn          NVARCHAR(200)     NOT NULL,
    Category         NVARCHAR(30)      NOT NULL,    -- Technical / Sales / ProductUpdate
    CategoryGroup    NVARCHAR(120)     NULL,        -- ozel ust kategori (or. Designer Eğitimi)
    SubCategory      NVARCHAR(120)     NULL,        -- ozel alt sekme (or. PDF Tasarımı)
    MaterialType     NVARCHAR(20)      NOT NULL,    -- Video / Document
    YoutubeId        NVARCHAR(40)      NULL,        -- embed için video id
    FileUrl          NVARCHAR(400)     NULL,        -- PDF/Doc indirme linki
    SortOrder        INT               NOT NULL CONSTRAINT DF_Mat_Sort DEFAULT(0),
    IsActive         BIT               NOT NULL CONSTRAINT DF_Mat_Active DEFAULT(1),
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Mat_Created DEFAULT(SYSUTCDATETIME()),
    CONSTRAINT PK_TrainingMaterials PRIMARY KEY CLUSTERED (MaterialId),
    CONSTRAINT FK_Mat_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
    CONSTRAINT CK_Mat_Type    CHECK (MaterialType IN (N'Video', N'Document')),
    CONSTRAINT CK_Mat_Cat     CHECK (Category IN (N'Technical', N'Sales', N'ProductUpdate'))
);
GO
CREATE INDEX IX_Mat_Category ON dbo.TrainingMaterials(Category, MaterialType, IsActive);
GO

/* --------------------------------------------------------------------------
   10) IMPERSONATION LOG  (Vendor "on-behalf" işlemleri denetim izi)
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ImpersonationLog') IS NOT NULL DROP TABLE dbo.ImpersonationLog;
GO
CREATE TABLE dbo.ImpersonationLog
(
    LogId            INT IDENTITY(1,1) NOT NULL,
    VendorUserId     INT               NOT NULL,   -- işlemi yapan VendorAdmin
    TargetCompanyId  INT               NOT NULL,   -- adına işlem yapılan partner
    Action           NVARCHAR(60)      NOT NULL,   -- ViewDashboard / EnterOrder / SubmitDemo ...
    Detail           NVARCHAR(500)     NULL,
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Imp_Created DEFAULT(SYSUTCDATETIME()),
    CONSTRAINT PK_ImpersonationLog PRIMARY KEY CLUSTERED (LogId),
    CONSTRAINT FK_Imp_User    FOREIGN KEY (VendorUserId)    REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Imp_Company FOREIGN KEY (TargetCompanyId) REFERENCES dbo.Companies(CompanyId)
);
GO
CREATE INDEX IX_Imp_Company ON dbo.ImpersonationLog(TargetCompanyId, CreatedAt);
GO

PRINT N'Şema oluşturuldu: 10 tablo + FK + index.';
GO
