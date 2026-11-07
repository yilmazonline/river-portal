/* ============================================================================
   07 — Müşteri Kayıt: kontak e-posta ve telefon kolonlari
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.CustomerRegistrations', 'ContactEmail') IS NULL
    ALTER TABLE dbo.CustomerRegistrations ADD ContactEmail NVARCHAR(256) NULL;
GO

IF COL_LENGTH('dbo.CustomerRegistrations', 'ContactPhone') IS NULL
    ALTER TABLE dbo.CustomerRegistrations ADD ContactPhone NVARCHAR(40) NULL;
GO

PRINT N'CustomerRegistrations: ContactEmail / ContactPhone hazir.';
GO
