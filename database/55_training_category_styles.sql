/* 55 - Akademi kategori gorunumu: kart gorseli, kategori sayfasi kapak gorseli, kart rengi. Idempotent. Kisitlar TCST_ onekli.
   Kategori ADINA baglidir (GroupName + SubName; ana kategori icin SubName = ''): ayni ad hem videolarda hem dokumanlarda
   varsa tek gorunum gecerlidir. Ad degisikligi ve silme TrainingCats.Rename / Delete tarafindan buraya yansitilir. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.TrainingCategoryStyles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingCategoryStyles
    (
        StyleId     INT IDENTITY(1,1) NOT NULL CONSTRAINT TCST_PK PRIMARY KEY,
        GroupName   NVARCHAR(120) NOT NULL,
        SubName     NVARCHAR(120) NOT NULL CONSTRAINT TCST_DF_Sub DEFAULT(N''),
        CardImage   NVARCHAR(200) NULL,      -- assets/catcovers altindaki dosya adi
        CoverImage  NVARCHAR(200) NULL,
        Tint        TINYINT       NULL,      -- 0..7 palet tonu; NULL = otomatik
        UpdatedAt   DATETIME2(0)  NOT NULL CONSTRAINT TCST_DF_Upd DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT TCST_CK_Tint CHECK (Tint IS NULL OR Tint BETWEEN 0 AND 7)
    );
    CREATE UNIQUE INDEX TCST_UX_Name ON dbo.TrainingCategoryStyles(GroupName, SubName);
END
GO
PRINT N'Kategori görünüm ayarları hazır.';
GO
