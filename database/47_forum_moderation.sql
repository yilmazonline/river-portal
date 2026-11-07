/* 47 - Forum moderasyonu: kalici moderasyon kaydi, kullanici kisitlamalari, forum ayarlari (sessize alma). Idempotent. Kisitlar FRM_ onekli. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.ForumModLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumModLog
    (
        LogId            INT IDENTITY(1,1) NOT NULL CONSTRAINT FRM_PK_ModLog PRIMARY KEY,
        Action           NVARCHAR(30)   NOT NULL,      -- delete_post, delete_comment, hide_post, hide_comment, unhide_post, unhide_comment, restrict, unrestrict, close, open
        PostId           INT            NULL,
        CommentId        INT            NULL,
        OwnerUserId      INT            NULL,
        OwnerName        NVARCHAR(200)  NULL,          -- kayit aninda (kisi sonradan silinse de okunur)
        ContentSnapshot  NVARCHAR(4000) NULL,          -- icerigin o anki metni
        MediaCount       INT            NOT NULL CONSTRAINT FRM_DF_LogMedia DEFAULT(0),
        ModeratorUserId  INT            NOT NULL,
        ModeratorName    NVARCHAR(200)  NULL,
        Reason           NVARCHAR(1000) NULL,
        CreatedAt        DATETIME2(0)   NOT NULL CONSTRAINT FRM_DF_LogCreated DEFAULT(SYSUTCDATETIME())
    );
    CREATE INDEX FRM_IX_ModLog_Date ON dbo.ForumModLog(LogId DESC);
END
GO
IF OBJECT_ID('dbo.ForumRestrictions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumRestrictions
    (
        RestrictionId  INT IDENTITY(1,1) NOT NULL CONSTRAINT FRM_PK_Restr PRIMARY KEY,
        UserId         INT            NOT NULL,
        Until          DATETIME2(0)   NULL,            -- NULL = suresiz
        Reason         NVARCHAR(1000) NULL,
        CreatedByUserId INT           NOT NULL,
        CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT FRM_DF_RestrCreated DEFAULT(SYSUTCDATETIME()),
        LiftedAt       DATETIME2(0)   NULL,
        LiftedByUserId INT            NULL
    );
    CREATE INDEX FRM_IX_Restr_User ON dbo.ForumRestrictions(UserId, LiftedAt);
END
GO
IF OBJECT_ID('dbo.ForumSettings', 'U') IS NULL
    CREATE TABLE dbo.ForumSettings (SettingKey NVARCHAR(50) NOT NULL CONSTRAINT FRM_PK_Settings PRIMARY KEY, Value NVARCHAR(1000) NULL, UpdatedByUserId INT NULL, UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT FRM_DF_SetUpd DEFAULT(SYSUTCDATETIME()));
GO
PRINT N'Forum moderasyon tabloları hazır. Kullanıcı düzenleme ekranından Fol personeline "Forum Admini" yetkisi verin.';
GO
