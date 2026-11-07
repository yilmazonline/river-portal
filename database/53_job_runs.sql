/* 53 - Zamanlanmis isler: gunluk calisma kaydi (ayni gun ikinci kez calismasin). Idempotent. Kisitlar JOB_ onekli.
   Ilk kullanim: "PendingTicketsDigest" (her gun 12:00 bekleyen destek talepleri ozeti). */
USE PARTNER;
GO
IF OBJECT_ID('dbo.JobRuns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JobRuns
    (
        JobKey       NVARCHAR(60)  NOT NULL CONSTRAINT JOB_PK PRIMARY KEY,
        LastRunDate  DATE          NOT NULL CONSTRAINT JOB_DF_Date DEFAULT('2000-01-01'),   -- Turkiye saatine gore gun
        LastRunAt    DATETIME2(0)  NULL,
        LastResult   NVARCHAR(400) NULL
    );
END
GO
PRINT N'Zamanlanmış iş kaydı hazır.';
GO
