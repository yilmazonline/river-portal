/* ============================================================================
   CLEANUP_support_tickets.sql — Su ana kadar girilmis TUM destek taleplerini siler
   ----------------------------------------------------------------------------
   SILER:  Tickets + TicketMessages + TicketAttachments (ek KAYITLARI).
           (Hizmet Formu'ndan olusan "Destek" tipi cozulmus talepler de Tickets
            icinde oldugundan bunlar da silinir.)
   SILMEZ: ProjectRecords (proje kayitlari), firmalar, kullanicilar, siparisler.
           Fiziksel ek DOSYALARI (ticketfiles klasoru) — SQL silemez; asagida
           PowerShell komutu var.
   DIKKAT: Geri alinamaz. Once YEDEK alin. BIR KEZ calistirin.
   ============================================================================ */
USE PARTNER;
GO

BEGIN TRAN;

/* FK sirasi: once bagli kayitlar, sonra Tickets */
IF OBJECT_ID('dbo.TicketAttachments') IS NOT NULL DELETE FROM dbo.TicketAttachments;
IF OBJECT_ID('dbo.TicketMessages')    IS NOT NULL DELETE FROM dbo.TicketMessages;
IF OBJECT_ID('dbo.Tickets')           IS NOT NULL DELETE FROM dbo.Tickets;

/* Kimlik sayaclarini sifirla (yeni talepler temiz numaradan baslasin) */
IF OBJECT_ID('dbo.TicketAttachments') IS NOT NULL AND OBJECTPROPERTY(OBJECT_ID('dbo.TicketAttachments'),'TableHasIdentity')=1 DBCC CHECKIDENT('dbo.TicketAttachments', RESEED, 0);
IF OBJECT_ID('dbo.TicketMessages')    IS NOT NULL AND OBJECTPROPERTY(OBJECT_ID('dbo.TicketMessages'),'TableHasIdentity')=1 DBCC CHECKIDENT('dbo.TicketMessages', RESEED, 0);
IF OBJECT_ID('dbo.Tickets')           IS NOT NULL AND OBJECTPROPERTY(OBJECT_ID('dbo.Tickets'),'TableHasIdentity')=1 DBCC CHECKIDENT('dbo.Tickets', RESEED, 0);

COMMIT;
GO

/* ----------------------------------------------------------------------------
   ISTEGE BAGLI — yorumu kaldirirsan sunlar da yapilir:
   ----------------------------------------------------------------------------
-- Talep (bell) bildirimlerini de temizle:
-- IF OBJECT_ID('dbo.Notifications') IS NOT NULL DELETE FROM dbo.Notifications WHERE Type = N'Ticket';

-- Proje/Hizmet (proje tipi) kayitlarini da sil:
-- IF OBJECT_ID('dbo.ProjectRecords') IS NOT NULL DELETE FROM dbo.ProjectRecords;
   ---------------------------------------------------------------------------- */

PRINT N'Destek talepleri (mesaj + ek kayitlari) silindi.';
GO
