/* 51 - Versiyon duyurulari: modul listesi (or. River AI icindeki E-Fatura). Idempotent. Kisitlar VMOD_ onekli.
   Bolum basliginda modul: "## Kategori | Modul" (modulsuz ve eski duyurular "## Kategori" olarak okunmaya devam eder). */
USE PARTNER;
GO
IF OBJECT_ID('dbo.VersionModules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VersionModules
    (
        ModuleId        INT IDENTITY(1,1) NOT NULL CONSTRAINT VMOD_PK PRIMARY KEY,
        Name            NVARCHAR(80)  NOT NULL,
        CreatedByUserId INT           NULL,
        CreatedAt       DATETIME2(0)  NOT NULL CONSTRAINT VMOD_DF_Created DEFAULT(SYSUTCDATETIME())
    );
    CREATE UNIQUE INDEX VMOD_UX_Name ON dbo.VersionModules(Name);
    INSERT INTO dbo.VersionModules (Name) VALUES (N'E-Fatura'), (N'Form Designer'), (N'Akış Tasarımcısı'), (N'Raporlama'), (N'Mobil Uygulama'), (N'Doküman Sistemi');
END
GO
PRINT N'Versiyon modül listesi hazır.';
GO
