/* 27 - Talep inceleme (okundu) + hatirlatma zamani. BIR KEZ. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Tickets','SupportSeenAt') IS NULL ALTER TABLE dbo.Tickets ADD SupportSeenAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.Tickets','SupportSeenBy') IS NULL ALTER TABLE dbo.Tickets ADD SupportSeenBy NVARCHAR(160) NULL;
GO
IF COL_LENGTH('dbo.Tickets','LastReminderAt') IS NULL ALTER TABLE dbo.Tickets ADD LastReminderAt DATETIME2 NULL;
GO
PRINT N'Tickets seen/reminder kolonlari hazir.';
GO
