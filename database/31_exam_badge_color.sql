/* 31 - Sinav sertifika rozet rengi. BIR KEZ calistirin. Idempotent. (30_exams.sql sonrasi) */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Exams','BadgeColor') IS NULL
    ALTER TABLE dbo.Exams ADD BadgeColor NVARCHAR(16) NOT NULL CONSTRAINT DF_Ex_Badge DEFAULT(N'gold');
GO
/* Verilis anindaki renk sertifikada saklanir (sinavin rengi sonradan degisse de sertifika degismez) */
IF COL_LENGTH('dbo.Certificates','BadgeColor') IS NULL
    ALTER TABLE dbo.Certificates ADD BadgeColor NVARCHAR(16) NULL;
GO
PRINT N'Rozet rengi kolonlari hazir.';
GO
