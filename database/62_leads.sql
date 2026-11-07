/* 62 - Lead Merkezi: firmalar, kisiler, sablonlar, mesaj kuyrugu, yanitlar, engelleme listesi, ayarlar. Idempotent. Kisitlar LD_ onekli. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.LeadCompanies', 'U') IS NULL
CREATE TABLE dbo.LeadCompanies
(
    LeadCompanyId INT IDENTITY(1,1) NOT NULL CONSTRAINT LD_PK_Co PRIMARY KEY,
    Name          NVARCHAR(200) NOT NULL,
    Domain        NVARCHAR(120) NULL,
    Website       NVARCHAR(300) NULL,
    Industry      NVARCHAR(150) NULL,
    Employees     INT           NULL,
    City          NVARCHAR(100) NULL,
    ApolloOrgId   NVARCHAR(60)  NULL,
    CreatedAt     DATETIME2(0)  NOT NULL CONSTRAINT LD_DF_CoAt DEFAULT(SYSUTCDATETIME())
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_CoDomain') CREATE INDEX LD_IX_CoDomain ON dbo.LeadCompanies(Domain);
GO
IF OBJECT_ID('dbo.LeadContacts', 'U') IS NULL
CREATE TABLE dbo.LeadContacts
(
    ContactId     INT IDENTITY(1,1) NOT NULL CONSTRAINT LD_PK_Ct PRIMARY KEY,
    LeadCompanyId INT           NULL,
    Category      NVARCHAR(10)  NOT NULL,                 -- musteri / danisman
    FullName      NVARCHAR(150) NOT NULL,
    FirstName     NVARCHAR(80)  NULL,
    Title         NVARCHAR(200) NULL,
    Persona       NVARCHAR(12)  NULL,                     -- yonetim / bt / surec / genel / ortak
    Email         NVARCHAR(256) NOT NULL,
    EmailStatus   NVARCHAR(30)  NULL,
    Domain        NVARCHAR(120) NULL,
    LinkedIn      NVARCHAR(300) NULL,
    Source        NVARCHAR(10)  NOT NULL,                 -- apollo / csv / elle
    ApolloId      NVARCHAR(60)  NULL,
    Status        NVARCHAR(14)  NOT NULL CONSTRAINT LD_DF_CtSt DEFAULT(N'new'),
    Step          INT           NOT NULL CONSTRAINT LD_DF_CtStep DEFAULT(0),
    NextAt        DATETIME2(0)  NULL,
    Note          NVARCHAR(400) NULL,
    CreatedAt     DATETIME2(0)  NOT NULL CONSTRAINT LD_DF_CtAt DEFAULT(SYSUTCDATETIME()),
    UpdatedAt     DATETIME2(0)  NULL,
    CONSTRAINT LD_CK_CtCat CHECK (Category IN (N'musteri', N'danisman')),
    CONSTRAINT LD_CK_CtSt CHECK (Status IN (N'new', N'queued', N'sequence', N'replied', N'interested', N'later', N'notinterested', N'bounced', N'unsubscribed', N'excluded', N'done'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_UX_CtEmail') CREATE UNIQUE INDEX LD_UX_CtEmail ON dbo.LeadContacts(Email);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_CtDomain') CREATE INDEX LD_IX_CtDomain ON dbo.LeadContacts(Domain);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_CtStatus') CREATE INDEX LD_IX_CtStatus ON dbo.LeadContacts(Status, Category);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_CtApollo') CREATE INDEX LD_IX_CtApollo ON dbo.LeadContacts(ApolloId);
GO
IF OBJECT_ID('dbo.LeadTemplates', 'U') IS NULL
CREATE TABLE dbo.LeadTemplates
(
    TemplateId INT IDENTITY(1,1) NOT NULL CONSTRAINT LD_PK_Tpl PRIMARY KEY,
    Category   NVARCHAR(10)  NOT NULL,
    Persona    NVARCHAR(12)  NOT NULL,
    StepNo     INT           NOT NULL,                    -- 0 ilk e-posta, 1 ve 2 takip
    Subject    NVARCHAR(200) NOT NULL,
    Body       NVARCHAR(MAX) NOT NULL,
    IsActive   BIT           NOT NULL CONSTRAINT LD_DF_TplAct DEFAULT(1),
    UpdatedAt  DATETIME2(0)  NOT NULL CONSTRAINT LD_DF_TplAt DEFAULT(SYSUTCDATETIME())
);
GO
IF OBJECT_ID('dbo.LeadMessages', 'U') IS NULL
CREATE TABLE dbo.LeadMessages
(
    MessageId         INT IDENTITY(1,1) NOT NULL CONSTRAINT LD_PK_Msg PRIMARY KEY,
    ContactId         INT            NOT NULL,
    StepNo            INT            NOT NULL,
    Mailbox           NVARCHAR(256)  NOT NULL,
    Subject           NVARCHAR(200)  NOT NULL,
    Body              NVARCHAR(MAX)  NOT NULL,
    Status            NVARCHAR(10)   NOT NULL,           -- draft / approved / sent / failed / cancelled
    ScheduledAt       DATETIME2(0)   NULL,
    SentAt            DATETIME2(0)   NULL,
    GraphMessageId    NVARCHAR(400)  NULL,
    ConversationId    NVARCHAR(400)  NULL,
    InternetMessageId NVARCHAR(400)  NULL,
    Error             NVARCHAR(500)  NULL,
    ApprovedBy        INT            NULL,
    ApprovedAt        DATETIME2(0)   NULL,
    CreatedAt         DATETIME2(0)   NOT NULL CONSTRAINT LD_DF_MsgAt DEFAULT(SYSUTCDATETIME()),
    CONSTRAINT LD_CK_MsgSt CHECK (Status IN (N'draft', N'approved', N'sent', N'failed', N'cancelled'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_MsgSt') CREATE INDEX LD_IX_MsgSt ON dbo.LeadMessages(Status, ScheduledAt);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_IX_MsgCt') CREATE INDEX LD_IX_MsgCt ON dbo.LeadMessages(ContactId);
GO
IF OBJECT_ID('dbo.LeadReplies', 'U') IS NULL
CREATE TABLE dbo.LeadReplies
(
    ReplyId        INT IDENTITY(1,1) NOT NULL CONSTRAINT LD_PK_Rep PRIMARY KEY,
    ContactId      INT            NULL,
    MessageId      INT            NULL,
    Mailbox        NVARCHAR(256)  NOT NULL,
    GraphMessageId NVARCHAR(400)  NOT NULL,
    ReceivedAt     DATETIME2(0)   NOT NULL,
    FromEmail      NVARCHAR(256)  NULL,
    Subject        NVARCHAR(300)  NULL,
    Preview        NVARCHAR(1000) NULL,
    Class          NVARCHAR(14)   NULL,                  -- interested / later / notinterested / wrongperson / ooo / bounce / unsubscribe / other
    Handled        BIT            NOT NULL CONSTRAINT LD_DF_RepH DEFAULT(0),
    CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT LD_DF_RepAt DEFAULT(SYSUTCDATETIME())
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_UX_RepGraph') CREATE UNIQUE INDEX LD_UX_RepGraph ON dbo.LeadReplies(GraphMessageId);
GO
IF OBJECT_ID('dbo.LeadSuppression', 'U') IS NULL
CREATE TABLE dbo.LeadSuppression
(
    SuppressionId INT IDENTITY(1,1) NOT NULL CONSTRAINT LD_PK_Sup PRIMARY KEY,
    Kind          NVARCHAR(6)   NOT NULL,                -- email / domain
    Value         NVARCHAR(256) NOT NULL,
    Reason        NVARCHAR(200) NULL,
    CreatedAt     DATETIME2(0)  NOT NULL CONSTRAINT LD_DF_SupAt DEFAULT(SYSUTCDATETIME()),
    CONSTRAINT LD_CK_SupKind CHECK (Kind IN (N'email', N'domain'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LD_UX_Sup') CREATE UNIQUE INDEX LD_UX_Sup ON dbo.LeadSuppression(Kind, Value);
GO
IF OBJECT_ID('dbo.LeadSettings', 'U') IS NULL
CREATE TABLE dbo.LeadSettings (SettingKey NVARCHAR(40) NOT NULL CONSTRAINT LD_PK_Set PRIMARY KEY, SettingValue NVARCHAR(MAX) NOT NULL);
GO
MERGE dbo.LeadSettings AS t
USING (VALUES
  (N'dailyNew', N'25'), (N'maxPerCompany', N'2'), (N'onlyVerified', N'1'),
  (N'mailboxMusteri', N'satis@fol.solutions'), (N'mailboxDanisman', N'sales@fol.solutions'),
  (N'signature', N'River AI · Fol Yazılım A.Ş.' + NCHAR(10) + N'fol.solutions'),
  (N'optOut', N'Bu tür e-postalar almak istemiyorsanız bu e-postayı “çıkar” yazarak yanıtlamanız yeterli.'),
  (N'apolloMusteri', N'{"person_locations":["Turkey"],"person_titles":["genel müdür","general manager","ceo","bilgi işlem müdürü","it manager","cio","süreç geliştirme müdürü","kalite müdürü"],"organization_num_employees_ranges":["51,200","201,500","501,1000","1001,5000"],"contact_email_status":["verified"]}'),
  (N'apolloDanisman', N'{"person_locations":["Turkey"],"person_titles":["kurucu","founder","ortak","partner","genel müdür","iş geliştirme müdürü","business development manager"],"q_organization_keyword_tags":["erp","dijital dönüşüm","digital transformation","sap","microsoft dynamics","consulting"],"contact_email_status":["verified"]}')
) AS s(k, v)
ON t.SettingKey = s.k WHEN NOT MATCHED THEN INSERT (SettingKey, SettingValue) VALUES (s.k, s.v);
GO
/* baslangic sablonlari: {Hitap} {Ad} {Firma} {Unvan} {Giris} yer tutucular (Hitap simdilik ad soyad); imza ve ret cumlesi gonderimde eklenir. Sonraki asamada Claude kisisel giris paragrafi ekleyecek ({Giris}). */
IF NOT EXISTS (SELECT 1 FROM dbo.LeadTemplates)
BEGIN
INSERT INTO dbo.LeadTemplates (Category, Persona, StepNo, Subject, Body) VALUES
(N'musteri', N'yonetim', 0, N'{Firma} için süreçlerde hız ve görünürlük',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'{Giris}' + NCHAR(10) + NCHAR(10) +
 N'River AI ile şirketler onay, satın alma, talep ve raporlama süreçlerini kod yazmadan birkaç günde dijitale taşıyor; yönetim de tüm süreçleri tek ekrandan izliyor.' + NCHAR(10) + NCHAR(10) +
 N'{Firma} için 20 dakikalık kısa bir tanıtım görüşmesi yapmaya ne dersiniz? Size uygun bir zamanı yazmanız yeterli.'),
(N'musteri', N'bt', 0, N'{Firma} BT ekibi için kodsuz süreç platformu',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'{Giris}' + NCHAR(10) + NCHAR(10) +
 N'River AI, iş birimlerinden gelen uygulama taleplerini BT ekibinin kontrolünde, kod yazmadan karşılamayı sağlıyor. Active Directory, ERP ve mevcut veritabanlarınızla entegre çalışıyor; kurulum yerinde ya da bulutta yapılabiliyor.' + NCHAR(10) + NCHAR(10) +
 N'Kısa bir teknik tanıtım için uygun olduğunuz bir zaman var mı?'),
(N'musteri', N'surec', 0, N'{Firma} süreçlerini birkaç günde modellemek',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'{Giris}' + NCHAR(10) + NCHAR(10) +
 N'River AI ile süreç ve kalite ekipleri akışları görsel olarak modelliyor, formları ve onay adımlarını kendileri kuruyor; yapay zekâ da süreç dokümanından ilk taslağı çıkarıyor.' + NCHAR(10) + NCHAR(10) +
 N'Kendi süreçlerinizden bir örnek üzerinden 20 dakikalık bir gösterim yapmamı ister misiniz?'),
(N'musteri', N'genel', 0, N'{Firma} için kodsuz süreç yönetimi',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'{Giris}' + NCHAR(10) + NCHAR(10) +
 N'River AI, şirketlerin iş süreçlerini kod yazmadan dijitale taşıdığı yerli bir yapay zekâ destekli platform. Hazır modüllerle CRM, satın alma, sözleşme ve talep yönetimi gibi süreçler kısa sürede devreye alınıyor.' + NCHAR(10) + NCHAR(10) +
 N'{Firma} için kısa bir tanıtım görüşmesi yapmaya ne dersiniz?'),
(N'musteri', N'genel', 1, N'Re: {Konu}',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'Önceki e-postamın yoğunluk içinde gözden kaçmış olabileceğini düşündüm. Uygunsa bu hafta 20 dakikalık bir görüşme ayarlayabiliriz; doğru kişi siz değilseniz kime yazmamın daha uygun olacağını iletirseniz çok sevinirim.'),
(N'musteri', N'genel', 2, N'Re: {Konu}',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'Sizi daha fazla meşgul etmek istemem; bu konudaki son e-postam. İleride süreçlerinizi dijitalleştirmeyi düşünürseniz bu e-postayı yanıtlamanız yeterli, memnuniyetle yardımcı olurum.'),
(N'danisman', N'ortak', 0, N'{Firma} ile River AI iş ortaklığı',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'{Giris}' + NCHAR(10) + NCHAR(10) +
 N'River AI, kodsuz ve yapay zekâ destekli yerli bir süreç yönetimi platformu. Danışmanlık firmalarıyla iş ortaklığı modeliyle çalışıyoruz: müşterilerinize hızlı teslim edebileceğiniz hazır modüller, satıştan gelir payı ve ekibinize eğitim ve sertifika sunuyoruz.' + NCHAR(10) + NCHAR(10) +
 N'İş ortaklığı modelimizi 20 dakikalık bir görüşmede anlatmak isterim. Size uygun bir zaman var mı?'),
(N'danisman', N'ortak', 1, N'Re: {Konu}',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'Önceki e-postamı hatırlatmak istedim. Müşterilerinize süreç dijitalleştirme projelerinde ek bir gelir kalemi oluşturabileceğini düşünüyoruz; kısa bir görüşme için uygun olduğunuz bir zaman var mı?'),
(N'danisman', N'ortak', 2, N'Re: {Konu}',
 N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) + N'Bu konudaki son e-postam. İş ortaklığı ileride gündeminize gelirse bu e-postayı yanıtlamanız yeterli; detayları memnuniyetle paylaşırım.');
END
GO
PRINT N'Lead Merkezi tabloları hazır.';
GO
