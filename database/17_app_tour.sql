/* ============================================================================
   17 — Uygulama Turu: kullanıcı bazında "görüldü" bayrağı
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

IF COL_LENGTH('dbo.Users', 'TourSeenAt') IS NULL
    ALTER TABLE dbo.Users ADD TourSeenAt DATETIME2 NULL;
GO

PRINT N'Users.TourSeenAt hazir.';
GO
