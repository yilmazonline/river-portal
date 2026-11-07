/* 24 - Bildirime tiklaninca gidilecek hedef (ilgili islem sayfasi). BIR KEZ. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Notifications','Link') IS NULL
    ALTER TABLE dbo.Notifications ADD Link NVARCHAR(300) NULL;
GO
PRINT N'Notifications.Link hazir.';
GO
