/* 33 - Dosya Paylasimi (WeTransfer benzeri) + kisiye ozel bildirim. Idempotent: yeniden calistirmak guvenlidir.
   Not: kisit/indeks adlari diger tablolarla (orn. TrainingProgress DF_TR_*) cakismasin diye XFR_ onekli. */
USE PARTNER;
GO

/* ---- Transferler ---- */
IF OBJECT_ID('dbo.Transfers','U') IS NULL
BEGIN
    CREATE TABLE dbo.Transfers (
        TransferId     INT IDENTITY(1,1) PRIMARY KEY,
        Title          NVARCHAR(200)  NOT NULL,
        Message        NVARCHAR(2000) NULL,
        CreatedBy      INT            NOT NULL,
        CreatedByName  NVARCHAR(160)  NULL,
        CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT DF_XFR_Created DEFAULT(SYSUTCDATETIME()),
        SentAt         DATETIME2(0)   NULL,
        ExpiresAt      DATETIME2(0)   NOT NULL,
        Status         NVARCHAR(12)   NOT NULL CONSTRAINT DF_XFR_Status DEFAULT(N'Uploading'),  -- Uploading|Sent|Cancelled|Expired
        TotalBytes     BIGINT         NOT NULL CONSTRAINT DF_XFR_Bytes DEFAULT(0),
        MaxDownloads   INT            NULL,            -- alici basina indirme siniri (NULL = sinirsiz)
        PasswordHash   NVARCHAR(200)  NULL,            -- opsiyonel sifre
        CONSTRAINT CK_XFR_Status CHECK (Status IN (N'Uploading', N'Sent', N'Cancelled', N'Expired'))
    );
    CREATE INDEX IX_XFR_Status ON dbo.Transfers(Status, ExpiresAt);
END
GO

/* ---- Dosyalar ---- */
IF OBJECT_ID('dbo.TransferFiles','U') IS NULL
BEGIN
    CREATE TABLE dbo.TransferFiles (
        FileId         INT IDENTITY(1,1) PRIMARY KEY,
        TransferId     INT            NOT NULL,
        OriginalName   NVARCHAR(260)  NOT NULL,
        ContentType    NVARCHAR(120)  NULL,
        SizeBytes      BIGINT         NOT NULL,
        UploadedBytes  BIGINT         NOT NULL CONSTRAINT DF_XFRF_Up DEFAULT(0),
        IsComplete     BIT            NOT NULL CONSTRAINT DF_XFRF_Done DEFAULT(0),
        CONSTRAINT FK_XFRF_Transfer FOREIGN KEY (TransferId) REFERENCES dbo.Transfers(TransferId) ON DELETE CASCADE
    );
    CREATE INDEX IX_XFRF_Transfer ON dbo.TransferFiles(TransferId);
END
GO

/* ---- Alicilar (her aliciya kisiye ozel link) ---- */
IF OBJECT_ID('dbo.TransferRecipients','U') IS NULL
BEGIN
    CREATE TABLE dbo.TransferRecipients (
        RecipientId     INT IDENTITY(1,1) PRIMARY KEY,
        TransferId      INT            NOT NULL,
        Token           NVARCHAR(40)   NOT NULL,
        Email           NVARCHAR(256)  NOT NULL,
        DisplayName     NVARCHAR(160)  NULL,
        UserId          INT            NULL,           -- portal kullanicisi ise
        CompanyId       INT            NULL,
        CompanyName     NVARCHAR(200)  NULL,
        SentAt          DATETIME2(0)   NULL,
        FirstDownloadAt DATETIME2(0)   NULL,
        LastDownloadAt  DATETIME2(0)   NULL,
        DownloadCount   INT            NOT NULL CONSTRAINT DF_XFRR_Count DEFAULT(0),
        CONSTRAINT FK_XFRR_Transfer FOREIGN KEY (TransferId) REFERENCES dbo.Transfers(TransferId) ON DELETE CASCADE,
        CONSTRAINT UQ_XFRR_Token UNIQUE (Token)
    );
    CREATE INDEX IX_XFRR_Transfer ON dbo.TransferRecipients(TransferId);
END
GO

/* ---- Indirme kayitlari ---- */
IF OBJECT_ID('dbo.TransferDownloads','U') IS NULL
BEGIN
    CREATE TABLE dbo.TransferDownloads (
        DownloadId   INT IDENTITY(1,1) PRIMARY KEY,
        RecipientId  INT           NOT NULL,
        FileId       INT           NOT NULL,
        DownloadedAt DATETIME2(0)  NOT NULL CONSTRAINT DF_XFRD_At DEFAULT(SYSUTCDATETIME()),
        Ip           NVARCHAR(64)  NULL,
        CONSTRAINT FK_XFRD_Recipient FOREIGN KEY (RecipientId) REFERENCES dbo.TransferRecipients(RecipientId) ON DELETE CASCADE
    );
    CREATE INDEX IX_XFRD_Recipient ON dbo.TransferDownloads(RecipientId);
END
GO

/* ---- Kisiye ozel uygulama ici bildirim (NULL = firmanin tum kullanicilari, eski davranis) ---- */
IF COL_LENGTH('dbo.Notifications','UserId') IS NULL
BEGIN
    ALTER TABLE dbo.Notifications ADD UserId INT NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notif_User' AND object_id = OBJECT_ID('dbo.Notifications'))
    CREATE INDEX IX_Notif_User ON dbo.Notifications(CompanyId, UserId, CreatedAt DESC);
GO

PRINT N'Dosya paylasimi tablolari + kisiye ozel bildirim hazir.';
GO
