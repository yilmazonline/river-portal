/* 41 - Satis onerileri: RiverTeklif siparislerinden is ortagi satis kaydi onerisi. Idempotent. Kisitlar OSG_ onekli. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.OrderSuggestions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderSuggestions
    (
        SuggestionId       INT IDENTITY(1,1) NOT NULL CONSTRAINT OSG_PK PRIMARY KEY,
        SourceOrderId      INT            NOT NULL,                 -- [RiverTeklif].[dbo].[Siparisler].Id
        OrderDate          DATETIME2(0)   NULL,
        PartnerNameRaw     NVARCHAR(300)  NULL,                     -- siparisteki is ortagi metni
        SalesChannel       NVARCHAR(40)   NULL,
        CustomerName       NVARCHAR(300)  NULL,
        ProductSummary     NVARCHAR(1000) NULL,
        NetAmount          DECIMAL(18,2)  NOT NULL CONSTRAINT OSG_DF_Net DEFAULT(0),   -- KDV haric
        Currency           NVARCHAR(10)   NOT NULL CONSTRAINT OSG_DF_Cur DEFAULT(N'TL'),
        Warning            NVARCHAR(500)  NULL,
        SuggestedCompanyId INT            NULL,                     -- ad eslesmesiyle onerilen is ortagi
        Status             NVARCHAR(12)   NOT NULL CONSTRAINT OSG_DF_Status DEFAULT(N'Pending'),
        DealId             INT            NULL,
        DecidedByUserId    INT            NULL,
        DecidedAt          DATETIME2(0)   NULL,
        CreatedAt          DATETIME2(0)   NOT NULL CONSTRAINT OSG_DF_Created DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT OSG_UQ_Order UNIQUE (SourceOrderId),
        CONSTRAINT OSG_CK_Status CHECK (Status IN (N'Pending', N'Booked', N'Dismissed'))
    );
    CREATE INDEX OSG_IX_Status ON dbo.OrderSuggestions(Status, OrderDate);
END
GO
/* Anlasma kaydinin kaynak siparisi: ayni siparis iki kez satis olarak kaydedilemez */
IF COL_LENGTH('dbo.Deals', 'SourceOrderId') IS NULL
    ALTER TABLE dbo.Deals ADD SourceOrderId INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'OSG_UX_DealsOrder' AND object_id = OBJECT_ID('dbo.Deals'))
    CREATE UNIQUE INDEX OSG_UX_DealsOrder ON dbo.Deals(SourceOrderId) WHERE SourceOrderId IS NOT NULL;
GO
PRINT N'Satış önerileri tabloları hazır. Öneriler Satış Önerileri ekranı açıldığında ve zamanlanmış görevde Teklif''ten alınır.';
GO
