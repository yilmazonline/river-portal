/* ============================================================================
   11 — Eğitim Merkezi (Netflix tarzı): doküman alanları, izleme ilerlemesi,
        puanlama/yorum, izleme günlüğü, içerik önerileri.
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

/* ---- 1) TrainingMaterials: aciklama, dokuman dosyasi, kapak, sure, sayaclar ---- */
IF COL_LENGTH('dbo.TrainingMaterials', 'DescriptionTr') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD DescriptionTr NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'DescriptionEn') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD DescriptionEn NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'DocStoredName') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD DocStoredName NVARCHAR(260) NULL;   -- diskteki dosya (dokuman)
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'DocOriginalName') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD DocOriginalName NVARCHAR(260) NULL;
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'DocContentType') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD DocContentType NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'DocFileSize') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD DocFileSize BIGINT NULL;
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'CoverImage') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD CoverImage NVARCHAR(260) NULL;      -- opsiyonel kapak gorseli
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'DurationSeconds') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD DurationSeconds INT NULL;           -- video suresi (sn)
GO
IF COL_LENGTH('dbo.TrainingMaterials', 'ViewCount') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD ViewCount INT NOT NULL CONSTRAINT DF_TM_ViewCount DEFAULT(0);
GO

/* ---- 2) Kullanici izleme ilerlemesi (video basina) ---- */
IF OBJECT_ID('dbo.TrainingProgress', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingProgress
    (
        ProgressId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaterialId    INT           NOT NULL,
        UserId        INT           NOT NULL,
        WatchedSeconds INT          NOT NULL CONSTRAINT DF_TP_Watched DEFAULT(0),
        DurationSeconds INT         NOT NULL CONSTRAINT DF_TP_Duration DEFAULT(0),
        [Percent]     INT           NOT NULL CONSTRAINT DF_TP_Percent DEFAULT(0),  -- 0..100 (PERCENT ayrilmis kelime: koseli parantez sart)
        Completed     BIT           NOT NULL CONSTRAINT DF_TP_Completed DEFAULT(0),
        LastWatchedAt DATETIME2(0)  NOT NULL CONSTRAINT DF_TP_Last DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_TP_Material FOREIGN KEY (MaterialId) REFERENCES dbo.TrainingMaterials(MaterialId),
        CONSTRAINT UQ_TP UNIQUE (MaterialId, UserId)
    );
    CREATE INDEX IX_TP_User ON dbo.TrainingProgress(UserId);
    PRINT N'dbo.TrainingProgress olusturuldu.';
END
GO

/* ---- 3) Puanlama + opsiyonel yorum (kullanici basina) ---- */
IF OBJECT_ID('dbo.TrainingRatings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingRatings
    (
        RatingId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaterialId INT           NOT NULL,
        UserId     INT           NOT NULL,
        Stars      INT           NOT NULL,
        Comment    NVARCHAR(1000) NULL,
        CreatedAt  DATETIME2(0)  NOT NULL CONSTRAINT DF_TR_Created DEFAULT(SYSUTCDATETIME()),
        UpdatedAt  DATETIME2(0)  NOT NULL CONSTRAINT DF_TR_Updated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_TR_Material FOREIGN KEY (MaterialId) REFERENCES dbo.TrainingMaterials(MaterialId),
        CONSTRAINT CK_TR_Stars CHECK (Stars BETWEEN 1 AND 5),
        CONSTRAINT UQ_TR UNIQUE (MaterialId, UserId)
    );
    CREATE INDEX IX_TR_Material ON dbo.TrainingRatings(MaterialId);
    PRINT N'dbo.TrainingRatings olusturuldu.';
END
GO

/* ---- 4) Izleme gunlugu (aylik istatistik icin) ---- */
IF OBJECT_ID('dbo.TrainingWatchLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingWatchLog
    (
        LogId      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId     INT          NOT NULL,
        MaterialId INT          NOT NULL,
        Seconds    INT          NOT NULL CONSTRAINT DF_TW_Seconds DEFAULT(0),
        LoggedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_TW_Logged DEFAULT(SYSUTCDATETIME())
    );
    CREATE INDEX IX_TW_UserDate ON dbo.TrainingWatchLog(UserId, LoggedAt);
    PRINT N'dbo.TrainingWatchLog olusturuldu.';
END
GO

/* ---- 5) İçerik önerileri (kullanici talebi -> destek@ mail; kayit da tutulur) ---- */
IF OBJECT_ID('dbo.ContentSuggestions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContentSuggestions
    (
        SuggestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId       INT           NULL,
        CompanyId    INT           NULL,
        Title        NVARCHAR(200) NULL,
        Body         NVARCHAR(2000) NOT NULL,
        CreatedAt    DATETIME2(0)  NOT NULL CONSTRAINT DF_CS_Created DEFAULT(SYSUTCDATETIME())
    );
    PRINT N'dbo.ContentSuggestions olusturuldu.';
END
GO

PRINT N'Migration 11 tamamlandi.';
GO
