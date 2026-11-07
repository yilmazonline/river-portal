/* ============================================================================
   14 — Eğitim: Öne Çıkar (featured) alanları
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.TrainingMaterials', 'IsFeatured') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD IsFeatured BIT NOT NULL CONSTRAINT DF_TM_Featured DEFAULT(0);
GO

IF COL_LENGTH('dbo.TrainingMaterials', 'FeatureText') IS NULL
    ALTER TABLE dbo.TrainingMaterials ADD FeatureText NVARCHAR(120) NULL;   -- banner/hero etiketi (or. "Öne Çıkan", "Yeni Sürüm")
GO

PRINT N'Egitim: IsFeatured / FeatureText hazir.';
GO
