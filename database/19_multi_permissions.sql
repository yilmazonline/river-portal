/* ============================================================================
   19 — Vendor kullanıcılarında çoklu yetki
   - Users.Permissions: virgullu izin seti (Portal, Editor, Support)
   - Role CHECK'e 'VendorStaff' eklenir (coklu-yetkili uretici personeli icin marker;
     yetkiler Permissions'tan turetilir)
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.Users', 'Permissions') IS NULL
    ALTER TABLE dbo.Users ADD Permissions NVARCHAR(100) NULL;
GO

DECLARE @cn sysname;
SELECT @cn = cc.name FROM sys.check_constraints cc
 WHERE cc.parent_object_id = OBJECT_ID('dbo.Users') AND cc.definition LIKE '%Role%';
IF @cn IS NOT NULL EXEC('ALTER TABLE dbo.Users DROP CONSTRAINT ' + @cn);
GO

ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Role CHECK
 (Role IN (N'VendorAdmin', N'SupportAgent', N'SalesRep', N'TrainingEditor', N'VendorStaff',
           N'PartnerAdmin', N'PartnerUser', N'CustomerAdmin', N'CustomerUser'));
GO

PRINT N'Users.Permissions + VendorStaff hazir.';
GO
