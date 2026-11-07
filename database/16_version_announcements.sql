/* ============================================================================
   16 — Versiyon Duyuruları
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF OBJECT_ID('dbo.VersionAnnouncements') IS NULL
BEGIN
    CREATE TABLE dbo.VersionAnnouncements
    (
        VersionId    INT IDENTITY(1,1) PRIMARY KEY,
        ProductName  NVARCHAR(120)  NOT NULL,          -- ör. "River AI", "River Docs"
        VersionNo    NVARCHAR(40)   NOT NULL,          -- ör. "5.2", "2026.1"
        ReleaseDate  DATE           NULL,              -- yayın tarihi
        AccessDate   DATE           NULL,              -- erişime açılma tarihi
        Highlights   NVARCHAR(MAX)  NULL,              -- yenilik listesi (her satır bir madde)
        BodyText     NVARCHAR(MAX)  NULL,              -- opsiyonel açıklama
        IsPublished  BIT            NOT NULL CONSTRAINT DF_VA_Pub DEFAULT(0),
        CreatedByUserId INT         NULL,
        CreatedAt    DATETIME2      NOT NULL CONSTRAINT DF_VA_Created DEFAULT(SYSUTCDATETIME()),
        PublishedAt  DATETIME2      NULL
    );
    CREATE INDEX IX_VA_Pub ON dbo.VersionAnnouncements(IsPublished, PublishedAt DESC);
END
GO

PRINT N'VersionAnnouncements hazir.';
GO
