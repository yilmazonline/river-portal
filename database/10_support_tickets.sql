/* ============================================================================
   10 — Destek (Ticket) Sistemi + Firma Tipi + Roller
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

/* ---- 1) Firma tipi: Vendor / Partner / Customer ---- */
IF COL_LENGTH('dbo.Companies', 'CompanyType') IS NULL
BEGIN
    ALTER TABLE dbo.Companies ADD CompanyType NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Companies_Type DEFAULT(N'Partner');
END
GO
-- Mevcut kayitlari tiple: vendor -> Vendor (partnerler zaten DEFAULT ile 'Partner')
UPDATE dbo.Companies SET CompanyType = N'Vendor' WHERE IsVendor = 1 AND CompanyType <> N'Vendor';
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Companies_Type')
    ALTER TABLE dbo.Companies ADD CONSTRAINT CK_Companies_Type
        CHECK (CompanyType IN (N'Vendor', N'Partner', N'Customer'));
GO

/* ---- 2) Roller: SupportAgent, CustomerAdmin, CustomerUser eklendi ---- */
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Role')
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role;
GO
ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Role CHECK
    (Role IN (N'VendorAdmin', N'SupportAgent', N'PartnerAdmin', N'PartnerUser', N'CustomerAdmin', N'CustomerUser'));
GO

/* ---- 3) Tickets ---- */
IF OBJECT_ID('dbo.Tickets', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tickets
    (
        TicketId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TicketNo       NVARCHAR(20)   NULL,           -- TLP-0xxxxx (insert sonrasi doldurulur)
        CompanyId      INT            NOT NULL,        -- talebi acan firma (bayi veya musteri)
        OpenedByUserId INT            NOT NULL,        -- talebi acan kullanici
        Product        NVARCHAR(40)   NOT NULL,        -- River AI / River
        Module         NVARCHAR(60)   NOT NULL,        -- Form Designer, River Designer, ...
        RequestType    NVARCHAR(30)   NOT NULL,        -- Hata / Kurulum / ...
        Priority       NVARCHAR(20)   NOT NULL CONSTRAINT DF_Tickets_Prio DEFAULT(N'Normal'),
        Status         NVARCHAR(20)   NOT NULL CONSTRAINT DF_Tickets_Status DEFAULT(N'New'),
        CustomerRegId  INT            NULL,            -- bayi ise: kayitli musteri (opsiyonel)
        CustomerName   NVARCHAR(200)  NULL,            -- denormalize musteri adi (opsiyonel)
        Version        NVARCHAR(60)   NULL,
        Environment    NVARCHAR(20)   NULL,            -- Canli / Test
        Subject        NVARCHAR(200)  NOT NULL,
        Phone          NVARCHAR(40)   NULL,
        AssignedToUserId INT          NULL,            -- atanan destek personeli
        CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT DF_Tickets_Created DEFAULT(SYSUTCDATETIME()),
        UpdatedAt      DATETIME2(0)   NOT NULL CONSTRAINT DF_Tickets_Updated DEFAULT(SYSUTCDATETIME()),
        ResolvedAt     DATETIME2(0)   NULL,            -- Cozuldu isaretlendigi an (5 gun sayaci)
        ReminderSentAt DATETIME2(0)   NULL,            -- otomatik kapanis oncesi hatirlatma
        ClosedAt       DATETIME2(0)   NULL,
        CONSTRAINT FK_Tickets_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
        CONSTRAINT CK_Tickets_Status CHECK
            (Status IN (N'New', N'InProgress', N'WaitingCustomer', N'Resolved', N'Closed')),
        CONSTRAINT CK_Tickets_Prio CHECK
            (Priority IN (N'Low', N'Normal', N'High', N'Critical'))
    );
    CREATE INDEX IX_Tickets_Status ON dbo.Tickets(Status);
    CREATE INDEX IX_Tickets_Company ON dbo.Tickets(CompanyId);
    CREATE INDEX IX_Tickets_Assigned ON dbo.Tickets(AssignedToUserId);
    PRINT N'dbo.Tickets olusturuldu.';
END
GO

/* ---- 4) Ticket mesajlari (konusmali log) ---- */
IF OBJECT_ID('dbo.TicketMessages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TicketMessages
    (
        MessageId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TicketId     INT            NOT NULL,
        AuthorUserId INT            NULL,             -- yazan kullanici (sistem mesaji ise NULL)
        AuthorSide   NVARCHAR(20)   NOT NULL,         -- Requester / Support / System
        Body         NVARCHAR(MAX)  NOT NULL,
        IsInternal   BIT            NOT NULL CONSTRAINT DF_TMsg_Internal DEFAULT(0), -- dahili not
        MsgKind      NVARCHAR(20)   NULL,             -- Reply / Reopen / StatusChange / Assign
        CreatedAt    DATETIME2(0)   NOT NULL CONSTRAINT DF_TMsg_Created DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_TMsg_Ticket FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(TicketId)
    );
    CREATE INDEX IX_TMsg_Ticket ON dbo.TicketMessages(TicketId);
    PRINT N'dbo.TicketMessages olusturuldu.';
END
GO

/* ---- 5) Ticket ekleri ---- */
IF OBJECT_ID('dbo.TicketAttachments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TicketAttachments
    (
        AttachmentId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TicketId     INT            NOT NULL,
        MessageId    INT            NULL,             -- belirli bir mesaja bagli olabilir
        StoredName   NVARCHAR(260)  NOT NULL,
        OriginalName NVARCHAR(260)  NOT NULL,
        ContentType  NVARCHAR(120)  NULL,
        FileSize     BIGINT         NOT NULL CONSTRAINT DF_TAtt_Size DEFAULT(0),
        CreatedAt    DATETIME2(0)   NOT NULL CONSTRAINT DF_TAtt_Created DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_TAtt_Ticket FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(TicketId)
    );
    CREATE INDEX IX_TAtt_Ticket ON dbo.TicketAttachments(TicketId);
    PRINT N'dbo.TicketAttachments olusturuldu.';
END
GO

PRINT N'Migration 10 tamamlandi.';
GO
