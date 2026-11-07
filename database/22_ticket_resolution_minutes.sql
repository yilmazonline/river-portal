/* 22 - Talep cozum suresi (dakika). Destek personeli cozdugunde girilir; performans raporunda kullanilir. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Tickets','ResolutionMinutes') IS NULL
    ALTER TABLE dbo.Tickets ADD ResolutionMinutes INT NULL;
GO
PRINT N'Tickets.ResolutionMinutes hazir.';
GO
