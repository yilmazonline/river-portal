/* 29 - Proje saati havuzu (firma) + Proje/Hizmet kayitlari. BIR KEZ. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Companies','ProjectBalanceHours') IS NULL
    ALTER TABLE dbo.Companies ADD ProjectBalanceHours DECIMAL(9,2) NOT NULL CONSTRAINT DF_Comp_ProjBal DEFAULT(0);
GO
IF OBJECT_ID('dbo.ProjectRecords') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectRecords (
        RecordId       INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId      INT           NOT NULL,
        CreatedByUserId INT          NULL,
        CreatedByName  NVARCHAR(160) NULL,
        Content        NVARCHAR(MAX) NOT NULL,
        Pool           NVARCHAR(12)  NOT NULL,        -- 'project' | 'support'
        Hours          DECIMAL(9,2)  NULL,            -- Pool='project' ise
        Minutes        INT           NULL,            -- Pool='support' ise
        Recipients     NVARCHAR(1000) NULL,           -- gonderilen kisiler (ad/e-posta)
        CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_PR_Created DEFAULT(SYSUTCDATETIME())
    );
    CREATE INDEX IX_PR_Company ON dbo.ProjectRecords(CompanyId, CreatedAt DESC);
END
GO
PRINT N'ProjectBalanceHours + ProjectRecords hazir.';
GO
