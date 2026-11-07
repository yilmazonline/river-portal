/* 63 - Lead Merkezi gonderim ve yanit: yanit ozeti ve metni, portaldan verilen yanitin baglantisi, gonderim ayarlari. Idempotent. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.LeadReplies', 'Summary') IS NULL ALTER TABLE dbo.LeadReplies ADD Summary NVARCHAR(300) NULL;
IF COL_LENGTH('dbo.LeadReplies', 'Body') IS NULL    ALTER TABLE dbo.LeadReplies ADD Body NVARCHAR(4000) NULL;
IF COL_LENGTH('dbo.LeadMessages', 'ReplyToId') IS NULL ALTER TABLE dbo.LeadMessages ADD ReplyToId INT NULL;   -- portaldan verilen yanit: hangi gelen yanita
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_MsgConv') CREATE INDEX LD_IX_MsgConv ON dbo.LeadMessages(ConversationId) WHERE ConversationId IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_RepCt') CREATE INDEX LD_IX_RepCt ON dbo.LeadReplies(ContactId, Handled);
GO
/* gonderim varsayilan KAPALI: isinma bitince Ayarlar'dan acilir */
MERGE dbo.LeadSettings AS t
USING (VALUES (N'sendEnabled', N'0'), (N'dailyPerMailbox', N'10'), (N'followupAuto', N'0'), (N'notify', N'yilmaz.guney@river.com.tr'),
              (N'bcc', N'yilmaz.guney@river.com.tr'), (N'personalize', N'1'), (N'sendFrom', N'09:30'), (N'sendTo', N'17:30')) AS s(k, v)
ON t.SettingKey = s.k WHEN NOT MATCHED THEN INSERT (SettingKey, SettingValue) VALUES (s.k, s.v);
GO
PRINT N'Lead Merkezi gönderim ve yanıt alanları hazır (gönderim kapalı).';
GO
