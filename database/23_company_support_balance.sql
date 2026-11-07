/* 23 - Firma destek suresi bakiyesi (dakika).
   Firma acilirken tanimlanir; talep cozuldugunde girilen dakika bu bakiyeden dusulur.
   Bakiye 0 (veya alti) ise firma yeni talep gonderemez. BIR KEZ calistirilir. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Companies','SupportBalanceMinutes') IS NULL
    ALTER TABLE dbo.Companies ADD SupportBalanceMinutes INT NOT NULL CONSTRAINT DF_Comp_SupBal DEFAULT(0);
GO
PRINT N'Companies.SupportBalanceMinutes hazir.';
GO
