/* 60 - Lisans modulu kaldirildi: tablolarini siler. ISTEGE BAGLI ve GERI ALINAMAZ.
   Calistirmadan once veritabani yedegi alin. Tablolarda tutulan lisans kayitlari (firma, kod, domain, yenileme tarihi,
   kapanis gecmisi) kalici olarak silinir. Kayitlari saklamak istiyorsaniz bu dosyayi CALISTIRMAYIN: portal tablolari kullanmaz. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.LicenseEvents', 'U') IS NOT NULL DROP TABLE dbo.LicenseEvents;
IF OBJECT_ID('dbo.Licenses', 'U') IS NOT NULL DROP TABLE dbo.Licenses;
GO
PRINT N'Lisans tabloları kaldırıldı.';
GO
