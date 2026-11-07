/* 43 - Alacak takibi: manuel alacak kaydi. Idempotent. Kisitlar RCV_ onekli.
   Manuel kayitlarin Teklif siparisi yoktur; SourceOrderId olarak cakismayan NEGATIF numara alirlar (Teklif senkronu yalniz pozitif, yeni siparisleri ekler). */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Receivables', 'IsManual') IS NULL
    ALTER TABLE dbo.Receivables ADD IsManual BIT NOT NULL CONSTRAINT RCV_DF_Manual DEFAULT(0);
GO
IF COL_LENGTH('dbo.Receivables', 'CreatedByUserId') IS NULL
    ALTER TABLE dbo.Receivables ADD CreatedByUserId INT NULL;
GO
PRINT N'Manuel alacak kaydı hazır.';
GO
