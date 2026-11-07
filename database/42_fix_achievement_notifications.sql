/* 42 - Hatali basari bildirimlerinin temizligi.
   v2.3.0-v2.5.0 arasi "is arkadasi gunluk basari bildirimi" eski sutunlara (TargetUserId/LinkUrl) yazildi;
   uygulama UserId/Link kullandigi icin bu satirlar firma geneline ve mukerrer gorundu. Idempotent. */
USE PARTNER;
GO
DECLARE @n INT;
DELETE FROM dbo.Notifications WHERE Type = N'Achievement' AND TargetUserId IS NOT NULL AND UserId IS NULL;
SET @n = @@ROWCOUNT;
PRINT N'Temizlenen hatalı başarı bildirimi: ' + CAST(@n AS NVARCHAR(12));
GO
