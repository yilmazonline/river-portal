/* ============================================================================
   21 — Harici Destek API'si (Faz 1: tek yön, ürünlerden talep alma)
   - Tickets tablosuna harici kaynak alanları
   - ApiClients: her ürün için bir API anahtarı (hash'li saklanır)
   - ApiRequestLog: gelen çağrı denetim kaydı
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   ============================================================================ */
USE PARTNER;
GO

/* --- Tickets: harici kaynak alanları --- */
IF COL_LENGTH('dbo.Tickets','Source')           IS NULL ALTER TABLE dbo.Tickets ADD Source NVARCHAR(20) NOT NULL CONSTRAINT DF_T_Source DEFAULT(N'Portal');
GO
IF COL_LENGTH('dbo.Tickets','ExternalProduct')  IS NULL ALTER TABLE dbo.Tickets ADD ExternalProduct NVARCHAR(100) NULL;
GO
IF COL_LENGTH('dbo.Tickets','ExternalRef')      IS NULL ALTER TABLE dbo.Tickets ADD ExternalRef NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.Tickets','RequesterCompany') IS NULL ALTER TABLE dbo.Tickets ADD RequesterCompany NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.Tickets','RequesterName')    IS NULL ALTER TABLE dbo.Tickets ADD RequesterName NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.Tickets','RequesterEmail')   IS NULL ALTER TABLE dbo.Tickets ADD RequesterEmail NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.Tickets','RequesterPhone')   IS NULL ALTER TABLE dbo.Tickets ADD RequesterPhone NVARCHAR(60) NULL;
GO

/* (ExternalProduct, ExternalRef) tekilligi — idempotency (ayni talep iki kez dusmez) */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Tickets_ExternalRef' AND object_id = OBJECT_ID('dbo.Tickets'))
    CREATE UNIQUE INDEX UX_Tickets_ExternalRef ON dbo.Tickets(ExternalProduct, ExternalRef) WHERE ExternalRef IS NOT NULL;
GO

/* --- ApiClients: her urun icin bir anahtar (SHA-256 hash'li) --- */
IF OBJECT_ID('dbo.ApiClients') IS NULL
CREATE TABLE dbo.ApiClients (
    ApiClientId  INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100) NOT NULL,      -- urun adi (or. "butce")
    KeyHash      CHAR(64)      NOT NULL,      -- SHA-256(anahtar) hex
    KeyPrefix    NVARCHAR(16)  NULL,          -- gosterim icin ilk birkac karakter
    IsActive     BIT           NOT NULL CONSTRAINT DF_ApiClients_Active DEFAULT(1),
    CreatedAt    DATETIME2     NOT NULL CONSTRAINT DF_ApiClients_Created DEFAULT(SYSUTCDATETIME()),
    LastUsedAt   DATETIME2     NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ApiClients_KeyHash' AND object_id = OBJECT_ID('dbo.ApiClients'))
    CREATE UNIQUE INDEX UX_ApiClients_KeyHash ON dbo.ApiClients(KeyHash);
GO

/* --- ApiRequestLog: denetim / sorun giderme --- */
IF OBJECT_ID('dbo.ApiRequestLog') IS NULL
CREATE TABLE dbo.ApiRequestLog (
    LogId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    ClientName  NVARCHAR(100) NULL,
    RemoteIp    NVARCHAR(60)  NULL,
    Endpoint    NVARCHAR(120) NULL,
    ExternalRef NVARCHAR(120) NULL,
    StatusCode  INT           NULL,
    Note        NVARCHAR(400) NULL,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_ApiReqLog_Created DEFAULT(SYSUTCDATETIME())
);
GO

PRINT N'Destek API semasi hazir (Tickets alanlari + ApiClients + ApiRequestLog).';
GO
