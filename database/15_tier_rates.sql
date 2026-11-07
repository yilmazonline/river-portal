/* ============================================================================
   15 — Partner seviyeleri: baz komisyon güncellemesi
   Platinum %60, Gold %50, Silver %40. (Kayıtlı %30 aynı kalır.)
   Canli PARTNER veritabaninda BIR KEZ calistirilir.
   ============================================================================ */
USE PARTNER;
GO

UPDATE dbo.PartnerTiers SET BaseCommission = 60.00 WHERE Code = N'Platinum';
UPDATE dbo.PartnerTiers SET BaseCommission = 50.00 WHERE Code = N'Gold';
UPDATE dbo.PartnerTiers SET BaseCommission = 40.00 WHERE Code = N'Silver';
GO

PRINT N'Baz komisyonlar guncellendi (Platinum 60, Gold 50, Silver 40).';
GO
