/* 64 - Lead Merkezi: firma tanimi (Instantly ve benzeri listelerden; Claude kisisellestirmesinde kullanilir). Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.LeadCompanies', 'Description') IS NULL ALTER TABLE dbo.LeadCompanies ADD Description NVARCHAR(1000) NULL;
GO
PRINT N'Firma tanımı sütunu hazır.';
GO
