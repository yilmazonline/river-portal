/* 49 - Forum: kisinin son gordugu gonderi (menudeki okunmamis sayisi). Idempotent. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.ForumSeen', 'U') IS NULL
    CREATE TABLE dbo.ForumSeen (UserId INT NOT NULL CONSTRAINT FRM_PK_Seen PRIMARY KEY, LastPostId INT NOT NULL CONSTRAINT FRM_DF_SeenPost DEFAULT(0), SeenAt DATETIME2(0) NOT NULL CONSTRAINT FRM_DF_SeenAt DEFAULT(SYSUTCDATETIME()));
GO
PRINT N'Forum okunmamış sayacı hazır.';
GO
