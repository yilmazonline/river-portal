/* ============================================================================
   13 — Üretici personeli rolleri: SalesRep (Satış Temsilcisi) + TrainingEditor (Eğitim Editörü)
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Role')
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role;
GO
ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Role CHECK
    (Role IN (N'VendorAdmin', N'SupportAgent', N'SalesRep', N'TrainingEditor',
              N'PartnerAdmin', N'PartnerUser', N'CustomerAdmin', N'CustomerUser'));
GO

PRINT N'Roller guncellendi: SalesRep, TrainingEditor eklendi.';
GO
