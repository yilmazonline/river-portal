/* ============================================================================
   20 — Kullanıcı e-posta bildirim tercihleri
   NotifyVersionEmail / NotifyTrainingEmail: varsayilan ACIK (1)
   (Uygulama ici bildirim her zaman gonderilir; bu tercihler yalniz e-posta icin.)
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.Users', 'NotifyVersionEmail') IS NULL
    ALTER TABLE dbo.Users ADD NotifyVersionEmail BIT NOT NULL CONSTRAINT DF_U_NVE DEFAULT(1);
GO
IF COL_LENGTH('dbo.Users', 'NotifyTrainingEmail') IS NULL
    ALTER TABLE dbo.Users ADD NotifyTrainingEmail BIT NOT NULL CONSTRAINT DF_U_NTE DEFAULT(1);
GO

PRINT N'Kullanici bildirim tercihleri hazir.';
GO
