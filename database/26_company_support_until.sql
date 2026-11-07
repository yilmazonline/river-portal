/* 26 - Firma destek geçerlilik tarihi (SupportUntil). NULL ise tarih kisiti yok; dolu ve gecmisse destek kapali. BIR KEZ. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Companies','SupportUntil') IS NULL
    ALTER TABLE dbo.Companies ADD SupportUntil DATE NULL;
GO
PRINT N'Companies.SupportUntil hazir.';
GO
