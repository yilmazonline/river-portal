/* 44 - Destek talebi: personelin yazdigi gorunen durum (or. "Guncelleme planlandi").
   Sistem durumu (New/InProgress/...) ve kurallari degismez; gorunen durum onun uzerinde gosterilir, sistem durumu degisince sifirlanir. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Tickets', 'StatusNote') IS NULL ALTER TABLE dbo.Tickets ADD StatusNote NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.Tickets', 'StatusNoteAt') IS NULL ALTER TABLE dbo.Tickets ADD StatusNoteAt DATETIME2(0) NULL;
GO
IF COL_LENGTH('dbo.Tickets', 'StatusNoteBy') IS NULL ALTER TABLE dbo.Tickets ADD StatusNoteBy INT NULL;
GO
PRINT N'Talep görünen durumu hazır.';
GO
