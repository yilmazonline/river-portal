/* 56 - Akademi kategori gorunumu: renk temasi sayisi 9 (0..8; 8 = Gunes, sari). Idempotent. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.TrainingCategoryStyles', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'TCST_CK_Tint' AND parent_object_id = OBJECT_ID('dbo.TrainingCategoryStyles'))
        ALTER TABLE dbo.TrainingCategoryStyles DROP CONSTRAINT TCST_CK_Tint;
    ALTER TABLE dbo.TrainingCategoryStyles ADD CONSTRAINT TCST_CK_Tint CHECK (Tint IS NULL OR Tint BETWEEN 0 AND 8);
END
GO
PRINT N'Kategori renk temaları: 0..8';
GO
