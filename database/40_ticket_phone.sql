/* 40 - Destek talebi: kullanicinin hatirlanan telefon numarasi (yalniz acik onayla saklanir). Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Users', 'TicketPhone') IS NULL
    ALTER TABLE dbo.Users ADD TicketPhone NVARCHAR(30) NULL;
GO
PRINT N'Users.TicketPhone hazır.';
GO
