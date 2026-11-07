/* 57 - Akademi kategori gorunumu: hazir ikon secimi (assets/caticons.svg icindeki "i-<ad>" sembolu). Idempotent. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.TrainingCategoryStyles', 'U') IS NOT NULL AND COL_LENGTH('dbo.TrainingCategoryStyles', 'Icon') IS NULL
    ALTER TABLE dbo.TrainingCategoryStyles ADD Icon NVARCHAR(40) NULL;
GO
PRINT N'Kategori ikonları hazır.';
GO
