/* 28 - API anahtarini firmaya bagla + public ClientId. BIR KEZ. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.ApiClients','CompanyId') IS NULL ALTER TABLE dbo.ApiClients ADD CompanyId INT NULL;
GO
IF COL_LENGTH('dbo.ApiClients','ClientId') IS NULL ALTER TABLE dbo.ApiClients ADD ClientId NVARCHAR(40) NULL;
GO
PRINT N'ApiClients CompanyId + ClientId hazir.';
GO
