/* ============================================================================
   08 — Eğitim Merkezi: özel kategori (grup) ve alt kategori (sekme)
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   Ornek: CategoryGroup = "Designer Eğitimi", SubCategory = "PDF Tasarımı"
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.TrainingMaterials', 'CategoryGroup') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD CategoryGroup NVARCHAR(120) NULL;
GO

IF COL_LENGTH('dbo.TrainingMaterials', 'SubCategory') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD SubCategory NVARCHAR(120) NULL;
GO

PRINT N'TrainingMaterials: CategoryGroup / SubCategory hazir.';
GO
