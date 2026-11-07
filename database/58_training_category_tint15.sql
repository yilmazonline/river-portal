/* 58 - Akademi kategori gorunumu: renk temasi sayisi 15 (0..14; 9 Lagun, 10 Ametist, 11 Mercan, 12 Orman, 13 Sarap, 14 Buz). Idempotent. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.TrainingCategoryStyles', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'TCST_CK_Tint' AND parent_object_id = OBJECT_ID('dbo.TrainingCategoryStyles'))
        ALTER TABLE dbo.TrainingCategoryStyles DROP CONSTRAINT TCST_CK_Tint;
    ALTER TABLE dbo.TrainingCategoryStyles ADD CONSTRAINT TCST_CK_Tint CHECK (Tint IS NULL OR Tint BETWEEN 0 AND 14);
END
GO
PRINT N'Kategori renk temaları: 0..14';
GO
