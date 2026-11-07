/* 45 - Forum (Faz 1): gonderiler, medya, yorumlar, begeniler, kaydedilenler, sikayetler. Idempotent. Kisitlar FRM_ onekli. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.ForumPosts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumPosts
    (
        PostId          INT IDENTITY(1,1) NOT NULL CONSTRAINT FRM_PK_Posts PRIMARY KEY,
        AuthorUserId    INT            NOT NULL,
        CompanyId       INT            NOT NULL,
        Body            NVARCHAR(4000) NOT NULL CONSTRAINT FRM_DF_Body DEFAULT(N''),
        IsAnnouncement  BIT            NOT NULL CONSTRAINT FRM_DF_Ann DEFAULT(0),
        IsPinned        BIT            NOT NULL CONSTRAINT FRM_DF_Pin DEFAULT(0),
        IsHidden        BIT            NOT NULL CONSTRAINT FRM_DF_Hid DEFAULT(0),   -- moderasyonla gizlendi
        LikeCount       INT            NOT NULL CONSTRAINT FRM_DF_Likes DEFAULT(0),
        CommentCount    INT            NOT NULL CONSTRAINT FRM_DF_Coms DEFAULT(0),
        CreatedAt       DATETIME2(0)   NOT NULL CONSTRAINT FRM_DF_PCreated DEFAULT(SYSUTCDATETIME()),
        EditedAt        DATETIME2(0)   NULL,
        DeletedAt       DATETIME2(0)   NULL
    );
    CREATE INDEX FRM_IX_Posts_Feed ON dbo.ForumPosts(IsPinned, PostId DESC) WHERE DeletedAt IS NULL;
END
GO
IF OBJECT_ID('dbo.ForumMedia', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumMedia
    (
        MediaId      INT IDENTITY(1,1) NOT NULL CONSTRAINT FRM_PK_Media PRIMARY KEY,
        PostId       INT           NOT NULL CONSTRAINT FRM_FK_Media_Post REFERENCES dbo.ForumPosts(PostId),
        Kind         NVARCHAR(10)  NOT NULL,          -- image / video
        StoredName   NVARCHAR(80)  NOT NULL,          -- forumfiles/ altinda rastgele ad
        OriginalName NVARCHAR(260) NULL,
        ContentType  NVARCHAR(60)  NOT NULL,
        FileSize     BIGINT        NOT NULL,
        SortOrder    INT           NOT NULL CONSTRAINT FRM_DF_MSort DEFAULT(0),
        CONSTRAINT FRM_CK_MediaKind CHECK (Kind IN (N'image', N'video'))
    );
    CREATE INDEX FRM_IX_Media_Post ON dbo.ForumMedia(PostId, SortOrder);
END
GO
IF OBJECT_ID('dbo.ForumComments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumComments
    (
        CommentId    INT IDENTITY(1,1) NOT NULL CONSTRAINT FRM_PK_Comments PRIMARY KEY,
        PostId       INT            NOT NULL CONSTRAINT FRM_FK_Com_Post REFERENCES dbo.ForumPosts(PostId),
        AuthorUserId INT            NOT NULL,
        Body         NVARCHAR(2000) NOT NULL,
        IsHidden     BIT            NOT NULL CONSTRAINT FRM_DF_CHid DEFAULT(0),
        CreatedAt    DATETIME2(0)   NOT NULL CONSTRAINT FRM_DF_CCreated DEFAULT(SYSUTCDATETIME()),
        DeletedAt    DATETIME2(0)   NULL
    );
    CREATE INDEX FRM_IX_Com_Post ON dbo.ForumComments(PostId, CommentId);
END
GO
IF OBJECT_ID('dbo.ForumLikes', 'U') IS NULL
    CREATE TABLE dbo.ForumLikes (PostId INT NOT NULL, UserId INT NOT NULL, CreatedAt DATETIME2(0) NOT NULL CONSTRAINT FRM_DF_LCreated DEFAULT(SYSUTCDATETIME()), CONSTRAINT FRM_PK_Likes PRIMARY KEY (PostId, UserId));
GO
IF OBJECT_ID('dbo.ForumSaves', 'U') IS NULL
    CREATE TABLE dbo.ForumSaves (PostId INT NOT NULL, UserId INT NOT NULL, CreatedAt DATETIME2(0) NOT NULL CONSTRAINT FRM_DF_SCreated DEFAULT(SYSUTCDATETIME()), CONSTRAINT FRM_PK_Saves PRIMARY KEY (UserId, PostId));
GO
IF OBJECT_ID('dbo.ForumReports', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumReports
    (
        ReportId       INT IDENTITY(1,1) NOT NULL CONSTRAINT FRM_PK_Reports PRIMARY KEY,
        PostId         INT           NOT NULL,
        CommentId      INT           NULL,             -- NULL = gonderinin kendisi
        ReporterUserId INT           NOT NULL,
        Reason         NVARCHAR(500) NULL,
        Status         NVARCHAR(12)  NOT NULL CONSTRAINT FRM_DF_RStatus DEFAULT(N'Open'),
        CreatedAt      DATETIME2(0)  NOT NULL CONSTRAINT FRM_DF_RCreated DEFAULT(SYSUTCDATETIME()),
        ResolvedByUserId INT         NULL,
        ResolvedAt     DATETIME2(0)  NULL,
        CONSTRAINT FRM_CK_RStatus CHECK (Status IN (N'Open', N'Hidden', N'Dismissed'))
    );
    CREATE UNIQUE INDEX FRM_UX_Report_Once ON dbo.ForumReports(ReporterUserId, PostId, CommentId);   -- ayni kisi ayni icerigi bir kez bildirir
    CREATE INDEX FRM_IX_Report_Open ON dbo.ForumReports(Status, CreatedAt);
END
GO
PRINT N'Forum tabloları hazır. forumfiles klasörüne IIS uygulama havuzu kimliğine yazma izni verin.';
GO
