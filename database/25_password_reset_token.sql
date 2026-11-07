/* 25 - Self-servis sifre sifirlama jetonu (kullanici e-postasindaki baglantiyla). BIR KEZ. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Users','ResetToken') IS NULL
    ALTER TABLE dbo.Users ADD ResetToken NVARCHAR(80) NULL;
GO
IF COL_LENGTH('dbo.Users','ResetTokenExpires') IS NULL
    ALTER TABLE dbo.Users ADD ResetTokenExpires DATETIME2 NULL;
GO
PRINT N'Users reset token kolonlari hazir.';
GO
