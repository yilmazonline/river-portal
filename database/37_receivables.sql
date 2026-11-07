/* 37 - Alacak takibi (River Teklif siparisleri -> tahsilat). Idempotent. Kisit adlari RCV_ onekli.
   Kaynak: [RiverTeklif].[dbo].[Siparisler] (ayni SQL Server). Portalin SQL kullanicisinin bu tabloda SELECT yetkisi olmalidir. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.Receivables', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Receivables
    (
        ReceivableId   INT IDENTITY(1,1) NOT NULL CONSTRAINT RCV_PK PRIMARY KEY,
        SourceOrderId  INT            NOT NULL,                 -- RiverTeklif.dbo.Siparisler.Id
        OrderDate      DATETIME2(0)   NULL,                     -- Turkiye saati
        InvoiceDate    DATETIME2(0)   NULL,                     -- FaturaGonderimTarih
        CompanyId      INT            NULL,                     -- alacakli oldugumuz cari (portal firmasi); NULL = cari secilmedi
        PartnerName    NVARCHAR(300)  NULL,
        CustomerName   NVARCHAR(300)  NULL,
        SalesRep       NVARCHAR(150)  NULL,
        Tags           NVARCHAR(300)  NULL,
        ProductSummary NVARCHAR(1000) NULL,
        NetAmount      DECIMAL(18,2)  NOT NULL CONSTRAINT RCV_DF_Net DEFAULT(0),   -- KDV haric, iskonto sonrasi
        VatRate        DECIMAL(5,2)   NOT NULL CONSTRAINT RCV_DF_Vat DEFAULT(20),
        GrossAmount    DECIMAL(18,2)  NOT NULL CONSTRAINT RCV_DF_Gross DEFAULT(0), -- KDV dahil alacak tutari
        Currency       NVARCHAR(10)   NOT NULL CONSTRAINT RCV_DF_Cur DEFAULT(N'TL'),
        PaymentNote    NVARCHAR(MAX)  NULL,                     -- CariBilgileri + Detaylar
        OrderEmails    NVARCHAR(MAX)  NULL,                     -- MailToJson
        Warning        NVARCHAR(400)  NULL,                     -- tutar/doviz kontrol uyarisi
        Status         NVARCHAR(20)   NOT NULL CONSTRAINT RCV_DF_Status DEFAULT(N'Unassigned'), -- Unassigned / Open / Partial / Paid / Closed
        CloseReason    NVARCHAR(500)  NULL,
        IsLegacy       BIT            NOT NULL CONSTRAINT RCV_DF_Legacy DEFAULT(0),
        AssignedAt     DATETIME2(0)   NULL,
        CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT RCV_DF_Created DEFAULT(SYSUTCDATETIME()),
        UpdatedAt      DATETIME2(0)   NOT NULL CONSTRAINT RCV_DF_Updated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT RCV_UQ_Source UNIQUE (SourceOrderId),
        CONSTRAINT RCV_FK_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
        CONSTRAINT RCV_CK_Status CHECK (Status IN (N'Unassigned', N'Open', N'Partial', N'Paid', N'Closed'))
    );
    CREATE INDEX RCV_IX_Status ON dbo.Receivables(Status) INCLUDE (CompanyId, GrossAmount);
END
GO
IF OBJECT_ID('dbo.ReceivableInstallments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReceivableInstallments
    (
        InstallmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT RCV_PK_Inst PRIMARY KEY,
        ReceivableId  INT           NOT NULL,
        SeqNo         INT           NOT NULL,
        DueDate       DATE          NOT NULL,
        Amount        DECIMAL(18,2) NOT NULL,
        PaidAmount    DECIMAL(18,2) NOT NULL CONSTRAINT RCV_DF_InstPaid DEFAULT(0),   -- tahsilatlarin vade sirasiyla dagitimi
        CONSTRAINT RCV_FK_Inst FOREIGN KEY (ReceivableId) REFERENCES dbo.Receivables(ReceivableId)
    );
    CREATE INDEX RCV_IX_InstDue ON dbo.ReceivableInstallments(DueDate) INCLUDE (ReceivableId, Amount, PaidAmount);
END
GO
IF OBJECT_ID('dbo.ReceivablePayments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReceivablePayments
    (
        PaymentId       INT IDENTITY(1,1) NOT NULL CONSTRAINT RCV_PK_Pay PRIMARY KEY,
        ReceivableId    INT           NOT NULL,
        PayDate         DATE          NOT NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        Method          NVARCHAR(30)  NULL,       -- Havale/EFT, Kredi Karti, Cek, Senet, Nakit
        Bank            NVARCHAR(100) NULL,
        Note            NVARCHAR(500) NULL,
        CreatedByUserId INT           NULL,
        CreatedAt       DATETIME2(0)  NOT NULL CONSTRAINT RCV_DF_PayCreated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT RCV_FK_Pay FOREIGN KEY (ReceivableId) REFERENCES dbo.Receivables(ReceivableId),
        CONSTRAINT RCV_CK_PayAmt CHECK (Amount > 0)
    );
END
GO
IF OBJECT_ID('dbo.ReceivableHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReceivableHistory
    (
        HistoryId    INT IDENTITY(1,1) NOT NULL CONSTRAINT RCV_PK_Hist PRIMARY KEY,
        ReceivableId INT            NOT NULL,
        Action       NVARCHAR(40)   NOT NULL,
        Detail       NVARCHAR(MAX)  NULL,
        UserId       INT            NULL,
        CreatedAt    DATETIME2(0)   NOT NULL CONSTRAINT RCV_DF_HistCreated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT RCV_FK_Hist FOREIGN KEY (ReceivableId) REFERENCES dbo.Receivables(ReceivableId)
    );
    CREATE INDEX RCV_IX_Hist ON dbo.ReceivableHistory(ReceivableId, CreatedAt);
END
GO
IF OBJECT_ID('dbo.ReceivableReminders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReceivableReminders
    (
        ReminderId   INT IDENTITY(1,1) NOT NULL CONSTRAINT RCV_PK_Rem PRIMARY KEY,
        ReceivableId INT            NOT NULL,
        Scenario     NVARCHAR(20)   NOT NULL,     -- before / today / overdue
        DaysValue    INT            NOT NULL,
        Recipients   NVARCHAR(MAX)  NOT NULL,
        Subject      NVARCHAR(300)  NOT NULL,
        Success      BIT            NOT NULL,
        Error        NVARCHAR(500)  NULL,
        SentByUserId INT            NULL,
        SentAt       DATETIME2(0)   NOT NULL CONSTRAINT RCV_DF_RemSent DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT RCV_FK_Rem FOREIGN KEY (ReceivableId) REFERENCES dbo.Receivables(ReceivableId)
    );
END
GO
IF OBJECT_ID('dbo.ReceivableSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReceivableSettings
    (
        SettingKey   NVARCHAR(50)  NOT NULL CONSTRAINT RCV_PK_Set PRIMARY KEY,
        SettingValue NVARCHAR(200) NULL
    );
END
GO
/* Kurulum ani: fatura tarihi olmayan eski duzendeki siparisler YALNIZ bu andan oncekiler icin bir kez ice alinir */
IF NOT EXISTS (SELECT 1 FROM dbo.ReceivableSettings WHERE SettingKey = N'InstalledAtUtc')
    INSERT INTO dbo.ReceivableSettings (SettingKey, SettingValue) VALUES (N'InstalledAtUtc', CONVERT(NVARCHAR(30), SYSUTCDATETIME(), 126));
GO
PRINT N'Alacak takibi tabloları hazır. Sonraki adım: portalın SQL kullanıcısına RiverTeklif.dbo.Siparisler üzerinde SELECT yetkisi verin (aşağıdaki örnek).';
GO
/* Ornek yetki (portal uygulama havuzunun kullandigi SQL girisi ile degistirin):
   USE RiverTeklif; GRANT SELECT ON dbo.Siparisler TO [portal_kullanici]; */
