/* 59 - Satis Materyalleri: ozet, sira, renk temasi ve ikon; kategori sirasi; goruntuleme / gonderim kayitlari; sayfa metni. Idempotent. Kisitlar SMAT_ onekli. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.SalesMaterials', 'Summary') IS NULL   ALTER TABLE dbo.SalesMaterials ADD Summary NVARCHAR(400) NULL;
IF COL_LENGTH('dbo.SalesMaterials', 'SortOrder') IS NULL ALTER TABLE dbo.SalesMaterials ADD SortOrder INT NOT NULL CONSTRAINT SMAT_DF_Sort DEFAULT(0);
IF COL_LENGTH('dbo.SalesMaterials', 'Tint') IS NULL      ALTER TABLE dbo.SalesMaterials ADD Tint TINYINT NULL;
IF COL_LENGTH('dbo.SalesMaterials', 'Icon') IS NULL      ALTER TABLE dbo.SalesMaterials ADD Icon NVARCHAR(40) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'SMAT_CK_Tint')
    ALTER TABLE dbo.SalesMaterials ADD CONSTRAINT SMAT_CK_Tint CHECK (Tint IS NULL OR Tint BETWEEN 0 AND 14);
GO
/* mevcut materyallerin sirasi: kategori icinde en yeni ustte (bugunku gorunum korunur) */
;WITH s AS (SELECT MaterialId, ROW_NUMBER() OVER (PARTITION BY ISNULL(NULLIF(CategoryGroup, N''), N'Genel') ORDER BY CreatedAt DESC, MaterialId DESC) AS rn
            FROM dbo.SalesMaterials WHERE SortOrder = 0)
UPDATE m SET SortOrder = s.rn FROM dbo.SalesMaterials m JOIN s ON s.MaterialId = m.MaterialId;
GO
IF OBJECT_ID('dbo.SalesMaterialCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesMaterialCategories
    (
        CategoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT SMAT_PK_Cat PRIMARY KEY,
        Name       NVARCHAR(120) NOT NULL,
        SortOrder  INT NOT NULL CONSTRAINT SMAT_DF_CatSort DEFAULT(0)
    );
    CREATE UNIQUE INDEX SMAT_UX_CatName ON dbo.SalesMaterialCategories(Name);
END
GO
/* mevcut kategoriler: bugunku alfabetik sirayla */
INSERT INTO dbo.SalesMaterialCategories (Name, SortOrder)
SELECT g.Name, ROW_NUMBER() OVER (ORDER BY g.Name)
FROM (SELECT DISTINCT ISNULL(NULLIF(CategoryGroup, N''), N'Genel') AS Name FROM dbo.SalesMaterials) g
WHERE NOT EXISTS (SELECT 1 FROM dbo.SalesMaterialCategories c WHERE c.Name = g.Name);
GO
IF OBJECT_ID('dbo.SalesMaterialEvents', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesMaterialEvents
    (
        EventId    INT IDENTITY(1,1) NOT NULL CONSTRAINT SMAT_PK_Ev PRIMARY KEY,
        MaterialId INT NOT NULL,
        UserId     INT NOT NULL,
        CompanyId  INT NULL,
        Kind       NVARCHAR(10) NOT NULL,          -- view / send
        CreatedAt  DATETIME2(0) NOT NULL CONSTRAINT SMAT_DF_EvAt DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT SMAT_CK_EvKind CHECK (Kind IN (N'view', N'send'))
    );
    CREATE INDEX SMAT_IX_EvMat ON dbo.SalesMaterialEvents(MaterialId, Kind);
    CREATE INDEX SMAT_IX_EvUser ON dbo.SalesMaterialEvents(UserId, Kind, CreatedAt);
END
GO
IF OBJECT_ID('dbo.SiteTexts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SiteTexts
    (
        TextKey   NVARCHAR(60)  NOT NULL CONSTRAINT SMAT_PK_Text PRIMARY KEY,
        TextValue NVARCHAR(600) NOT NULL,
        UpdatedAt DATETIME2(0)  NOT NULL CONSTRAINT SMAT_DF_TextAt DEFAULT(SYSUTCDATETIME())
    );
END
GO
PRINT N'Satış materyalleri genişletmesi hazır.';
GO
