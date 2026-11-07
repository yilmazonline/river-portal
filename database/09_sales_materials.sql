/* ============================================================================
   09 — Satış Materyal ve İçerikleri (PDF, sunum, tanıtım dokümanları)
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF OBJECT_ID('dbo.SalesMaterials', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesMaterials
    (
        MaterialId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TitleTr       NVARCHAR(200)  NOT NULL,
        TitleEn       NVARCHAR(200)  NULL,
        CategoryGroup NVARCHAR(120)  NULL,           -- ozel kategori (or. Sunumlar, Brosurler)
        StoredName    NVARCHAR(260)  NOT NULL,       -- diskteki dosya adi (GUID.ext) veya harici URL
        OriginalName  NVARCHAR(260)  NOT NULL,       -- kullaniciya gosterilecek indirme adi
        ContentType   NVARCHAR(120)  NULL,           -- MIME; harici link icin 'url'
        IsExternal    BIT            NOT NULL DEFAULT 0,
        FileSize      BIGINT         NOT NULL DEFAULT 0,
        IsActive      BIT            NOT NULL DEFAULT 1,
        CreatedAt     DATETIME2(0)   NOT NULL DEFAULT SYSUTCDATETIME()
    );
    PRINT N'dbo.SalesMaterials olusturuldu.';
END
ELSE
    PRINT N'dbo.SalesMaterials zaten var.';
GO
