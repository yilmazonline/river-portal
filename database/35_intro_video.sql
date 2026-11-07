/* 35 - Karsilama (tanitim) videosu: her kullaniciya yalniz bir kez. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Users','IntroSeenAt') IS NULL
    ALTER TABLE dbo.Users ADD IntroSeenAt DATETIME2(0) NULL;
GO
PRINT N'Users.IntroSeenAt hazir.';
GO
