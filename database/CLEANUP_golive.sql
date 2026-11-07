/* ============================================================================
   CLEANUP_golive.sql — CANLIYA ALMADAN ÖNCE test verisi temizligi
   ----------------------------------------------------------------------------
   SILER:   tum bildirimler, tum destek talepleri (mesaj + ek dahil)
   KORUR:   is ortaklari/firmalar, kullanicilar, kayitli siparisler (Deals)
   DIKKAT:  Geri alinamaz. Once yedek alin. BIR KEZ calistirin.
   ============================================================================ */
USE PARTNER;
GO

BEGIN TRAN;

/* 1) Destek talepleri (once bagli kayitlar) */
IF OBJECT_ID('dbo.TicketAttachments') IS NOT NULL DELETE FROM dbo.TicketAttachments;
IF OBJECT_ID('dbo.TicketMessages')    IS NOT NULL DELETE FROM dbo.TicketMessages;
IF OBJECT_ID('dbo.Tickets')           IS NOT NULL DELETE FROM dbo.Tickets;

/* 2) Bildirimler */
IF OBJECT_ID('dbo.Notifications') IS NOT NULL DELETE FROM dbo.Notifications;

/* 3) Kimlik sayaclarini sifirla (yeni kayitlar temiz numaradan baslasin) */
IF OBJECT_ID('dbo.Tickets') IS NOT NULL AND OBJECTPROPERTY(OBJECT_ID('dbo.Tickets'),'TableHasIdentity')=1 DBCC CHECKIDENT('dbo.Tickets', RESEED, 0);
IF OBJECT_ID('dbo.TicketMessages') IS NOT NULL AND OBJECTPROPERTY(OBJECT_ID('dbo.TicketMessages'),'TableHasIdentity')=1 DBCC CHECKIDENT('dbo.TicketMessages', RESEED, 0);
IF OBJECT_ID('dbo.TicketAttachments') IS NOT NULL AND OBJECTPROPERTY(OBJECT_ID('dbo.TicketAttachments'),'TableHasIdentity')=1 DBCC CHECKIDENT('dbo.TicketAttachments', RESEED, 0);
IF OBJECT_ID('dbo.Notifications') IS NOT NULL AND OBJECTPROPERTY(OBJECT_ID('dbo.Notifications'),'TableHasIdentity')=1 DBCC CHECKIDENT('dbo.Notifications', RESEED, 0);

COMMIT;
GO

/* ----------------------------------------------------------------------------
   ISTEGE BAGLI — sadece bunlari da temizlemek isterseniz yorumu kaldirin.
   (Is ortaklari ve kayitli siparisler yine korunur.)
   ----------------------------------------------------------------------------
-- Demo talepleri:
-- IF OBJECT_ID('dbo.Demos') IS NOT NULL DELETE FROM dbo.Demos;
-- Musteri kayitlari (30 gun koruma kayitlari):
-- IF OBJECT_ID('dbo.CustomerRegistrations') IS NOT NULL DELETE FROM dbo.CustomerRegistrations;
-- Egitim izleme/ilerleme/puan kayitlari:
-- IF OBJECT_ID('dbo.TrainingWatchLog') IS NOT NULL DELETE FROM dbo.TrainingWatchLog;
-- IF OBJECT_ID('dbo.TrainingProgress') IS NOT NULL DELETE FROM dbo.TrainingProgress;
-- IF OBJECT_ID('dbo.TrainingRatings')  IS NOT NULL DELETE FROM dbo.TrainingRatings;
   ---------------------------------------------------------------------------- */

PRINT N'Temizlik tamam: bildirimler ve destek talepleri silindi; is ortaklari ve siparisler korundu.';
GO
