/* 54 - Akademi kategorileri: editorlerin yonettigi ana ve alt kategoriler (Video ve Dokuman ayri). Idempotent. Kisitlar TCAT_ onekli.
   Icerikler kategori adini TrainingMaterials.CategoryGroup / SubCategory alanlarinda tutmaya devam eder;
   ad degisikligi bu alanlara da yansitilir (TrainingCats.Rename). Mevcut kategoriler asagida aktarilir. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.TrainingCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingCategories
    (
        CategoryId  INT IDENTITY(1,1) NOT NULL CONSTRAINT TCAT_PK PRIMARY KEY,
        Scope       NVARCHAR(10)  NOT NULL,          -- Video / Document
        ParentId    INT           NULL CONSTRAINT TCAT_FK_Parent REFERENCES dbo.TrainingCategories(CategoryId),
        Name        NVARCHAR(120) NOT NULL,
        SortOrder   INT           NOT NULL CONSTRAINT TCAT_DF_Sort DEFAULT(0),
        CreatedAt   DATETIME2(0)  NOT NULL CONSTRAINT TCAT_DF_Created DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT TCAT_CK_Scope CHECK (Scope IN (N'Video', N'Document'))
    );
    CREATE UNIQUE INDEX TCAT_UX_Name ON dbo.TrainingCategories(Scope, ParentId, Name);
END
GO
/* mevcut ana kategoriler: sayfadaki sirayla (ilk iceriklerin sirasi) */
;WITH g AS (
    SELECT CASE WHEN m.MaterialType = N'Video' THEN N'Video' ELSE N'Document' END AS Scope,
           ISNULL(NULLIF(LTRIM(RTRIM(m.CategoryGroup)), N''), N'Genel') AS Name,
           MIN(m.SortOrder) AS s1, MIN(m.MaterialId) AS s2
    FROM dbo.TrainingMaterials m GROUP BY CASE WHEN m.MaterialType = N'Video' THEN N'Video' ELSE N'Document' END, ISNULL(NULLIF(LTRIM(RTRIM(m.CategoryGroup)), N''), N'Genel'))
INSERT INTO dbo.TrainingCategories (Scope, ParentId, Name, SortOrder)
SELECT g.Scope, NULL, g.Name, ROW_NUMBER() OVER (PARTITION BY g.Scope ORDER BY g.s1, g.s2)
FROM g WHERE NOT EXISTS (SELECT 1 FROM dbo.TrainingCategories c WHERE c.Scope = g.Scope AND c.ParentId IS NULL AND c.Name = g.Name);
GO
/* mevcut alt kategoriler ("Genel" alt kategori yok sayilir: alt kategorisiz demektir) */
;WITH s AS (
    SELECT CASE WHEN m.MaterialType = N'Video' THEN N'Video' ELSE N'Document' END AS Scope,
           ISNULL(NULLIF(LTRIM(RTRIM(m.CategoryGroup)), N''), N'Genel') AS GroupName,
           LTRIM(RTRIM(m.SubCategory)) AS Name, MIN(m.SortOrder) AS s1, MIN(m.MaterialId) AS s2
    FROM dbo.TrainingMaterials m
    WHERE NULLIF(LTRIM(RTRIM(m.SubCategory)), N'') IS NOT NULL AND LTRIM(RTRIM(m.SubCategory)) <> N'Genel'
    GROUP BY CASE WHEN m.MaterialType = N'Video' THEN N'Video' ELSE N'Document' END, ISNULL(NULLIF(LTRIM(RTRIM(m.CategoryGroup)), N''), N'Genel'), LTRIM(RTRIM(m.SubCategory)))
INSERT INTO dbo.TrainingCategories (Scope, ParentId, Name, SortOrder)
SELECT s.Scope, p.CategoryId, s.Name, ROW_NUMBER() OVER (PARTITION BY s.Scope, p.CategoryId ORDER BY s.s1, s.s2)
FROM s JOIN dbo.TrainingCategories p ON p.Scope = s.Scope AND p.ParentId IS NULL AND p.Name = s.GroupName
WHERE NOT EXISTS (SELECT 1 FROM dbo.TrainingCategories c WHERE c.Scope = s.Scope AND c.ParentId = p.CategoryId AND c.Name = s.Name);
GO
PRINT N'Akademi kategorileri hazır.';
GO
