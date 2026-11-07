/* ============================================================================
   RESET_tour_all_users.sql - Portal turunu tum kullanicilar icin sifirlar.
   Tur, her kullanicinin portalda acacagi ilk sayfada bir kez otomatik gosterilir
   (yeni eklenen sinav / sertifika / bildirim adimlari dahil).
   Kullanici turu bitirince ya da kapatinca yeniden isaretlenir; tekrar cikmaz.
   ============================================================================ */
USE PARTNER;
GO

SELECT COUNT(*) AS [Turu daha once gormus kullanici] FROM dbo.Users WHERE TourSeenAt IS NOT NULL;

UPDATE dbo.Users SET TourSeenAt = NULL WHERE TourSeenAt IS NOT NULL;

SELECT @@ROWCOUNT AS [Sifirlanan kullanici];
GO

/* ----------------------------------------------------------------------------
   ISTEGE BAGLI - yalniz belirli rollerde sifirlamak icin yukaridaki UPDATE
   yerine asagidakilerden birini kullanin:

-- Yalniz bayi ve musteri kullanicilari:
-- UPDATE dbo.Users SET TourSeenAt = NULL
--  WHERE TourSeenAt IS NOT NULL AND Role IN (N'PartnerAdmin', N'PartnerUser', N'CustomerAdmin', N'CustomerUser');

-- Yalniz uretici tarafi (portal yoneticisi, egitim editoru, destek):
-- UPDATE dbo.Users SET TourSeenAt = NULL
--  WHERE TourSeenAt IS NOT NULL AND Role IN (N'VendorAdmin', N'VendorStaff', N'SalesRep', N'TrainingEditor', N'SupportAgent');
   ---------------------------------------------------------------------------- */
