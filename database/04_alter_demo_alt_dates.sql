/* ============================================================================
   04 — DemoRequests: alternatif tarih/saat kolonlari (AltDate1, AltDate2)
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent'tir.
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.DemoRequests', 'AltDate1') IS NULL
    ALTER TABLE dbo.DemoRequests ADD AltDate1 DATETIME2(0) NULL;
GO

IF COL_LENGTH('dbo.DemoRequests', 'AltDate2') IS NULL
    ALTER TABLE dbo.DemoRequests ADD AltDate2 DATETIME2(0) NULL;
GO

PRINT N'DemoRequests: AltDate1 / AltDate2 kolonlari hazir.';
GO
