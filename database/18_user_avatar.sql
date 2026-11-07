/* ============================================================================
   18 — Kullanıcı profil resmi (avatar)
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.Users', 'AvatarImage') IS NULL
    ALTER TABLE dbo.Users ADD AvatarImage NVARCHAR(200) NULL;
GO

PRINT N'Users.AvatarImage hazir.';
GO
