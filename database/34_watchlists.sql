/* 34 - Akademi izleme listeleri (paylasilabilir) + kaldigi yerden devam. Idempotent: yeniden calistirmak guvenlidir.
   Kisit/indeks adlari WLST_ onekli (diger tablolarla cakismasin). */
USE PARTNER;
GO

/* ---- Video izleme ilerlemesi ----
   11_training_plus.sql bu tabloyu olusturamamisti: "Percent" SQL Server'da ayrilmis kelime ve koseli
   parantez olmadan tanimlanmisti. Tablo yoksa dogru tanimla (LastPosition dahil) burada olusturulur. */
IF OBJECT_ID('dbo.TrainingProgress', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingProgress
    (
        ProgressId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaterialId      INT           NOT NULL,
        UserId          INT           NOT NULL,
        WatchedSeconds  INT           NOT NULL CONSTRAINT DF_TP_Watched DEFAULT(0),     -- en ileri konum (tamamlanma)
        LastPosition    INT           NULL,                                             -- son kalinan konum (kaldigi yerden devam)
        DurationSeconds INT           NOT NULL CONSTRAINT DF_TP_Duration DEFAULT(0),
        [Percent]       INT           NOT NULL CONSTRAINT DF_TP_Percent DEFAULT(0),     -- 0..100
        Completed       BIT           NOT NULL CONSTRAINT DF_TP_Completed DEFAULT(0),
        LastWatchedAt   DATETIME2(0)  NOT NULL CONSTRAINT DF_TP_Last DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_TP_Material FOREIGN KEY (MaterialId) REFERENCES dbo.TrainingMaterials(MaterialId),
        CONSTRAINT UQ_TP UNIQUE (MaterialId, UserId)
    );
    PRINT N'TrainingProgress olusturuldu.';
END
GO
/* Tablo onceden varsa (farkli kurulum): yalniz son konum sutunu eklenir */
IF COL_LENGTH('dbo.TrainingProgress','LastPosition') IS NULL
    ALTER TABLE dbo.TrainingProgress ADD LastPosition INT NULL;
GO

/* ---- Izleme listeleri ---- */
IF OBJECT_ID('dbo.Watchlists','U') IS NULL
BEGIN
    CREATE TABLE dbo.Watchlists (
        ListId          INT IDENTITY(1,1) PRIMARY KEY,
        OwnerUserId     INT            NOT NULL,
        OwnerCompanyId  INT            NOT NULL,
        Name            NVARCHAR(120)  NOT NULL,
        IsDefault       BIT            NOT NULL CONSTRAINT DF_WLST_Default DEFAULT(0),     -- "Daha Sonra Izle"
        ShareScope      NVARCHAR(10)   NOT NULL CONSTRAINT DF_WLST_Scope DEFAULT(N'private'), -- private | company | users
        IsRecommended   BIT            NOT NULL CONSTRAINT DF_WLST_Rec DEFAULT(0),         -- egitim editoru: tum kullanicilara onerilen
        CreatedAt       DATETIME2(0)   NOT NULL CONSTRAINT DF_WLST_Created DEFAULT(SYSUTCDATETIME()),
        UpdatedAt       DATETIME2(0)   NOT NULL CONSTRAINT DF_WLST_Updated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT CK_WLST_Scope CHECK (ShareScope IN (N'private', N'company', N'users'))
    );
    CREATE INDEX IX_WLST_Owner ON dbo.Watchlists(OwnerUserId);
    CREATE INDEX IX_WLST_Company ON dbo.Watchlists(OwnerCompanyId, ShareScope);
END
GO

/* ---- Listedeki videolar ---- */
IF OBJECT_ID('dbo.WatchlistItems','U') IS NULL
BEGIN
    CREATE TABLE dbo.WatchlistItems (
        ItemId      INT IDENTITY(1,1) PRIMARY KEY,
        ListId      INT           NOT NULL,
        MaterialId  INT           NOT NULL,
        SortOrder   INT           NOT NULL CONSTRAINT DF_WLSTI_Sort DEFAULT(0),
        AddedAt     DATETIME2(0)  NOT NULL CONSTRAINT DF_WLSTI_Added DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_WLSTI_List FOREIGN KEY (ListId) REFERENCES dbo.Watchlists(ListId) ON DELETE CASCADE,
        CONSTRAINT UQ_WLSTI_ListMat UNIQUE (ListId, MaterialId)
    );
END
GO

/* ---- Belirli kisilerle paylasim (ShareScope = users) ---- */
IF OBJECT_ID('dbo.WatchlistShares','U') IS NULL
BEGIN
    CREATE TABLE dbo.WatchlistShares (
        ShareId   INT IDENTITY(1,1) PRIMARY KEY,
        ListId    INT           NOT NULL,
        UserId    INT           NOT NULL,
        SharedAt  DATETIME2(0)  NOT NULL CONSTRAINT DF_WLSTS_At DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_WLSTS_List FOREIGN KEY (ListId) REFERENCES dbo.Watchlists(ListId) ON DELETE CASCADE,
        CONSTRAINT UQ_WLSTS_ListUser UNIQUE (ListId, UserId)
    );
    CREATE INDEX IX_WLSTS_User ON dbo.WatchlistShares(UserId);
END
GO

PRINT N'Izleme listeleri + LastPosition hazir.';
GO
