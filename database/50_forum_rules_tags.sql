/* 50 - Forum: topluluk kurallari onayi (surumlu) ve etiketler (gundemdekiler, etikete gore suzme). Idempotent. Kisitlar FRM_ onekli. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.ForumAgreements', 'U') IS NULL
    CREATE TABLE dbo.ForumAgreements
    (
        UserId       INT           NOT NULL,
        RulesVersion NVARCHAR(20)  NOT NULL,
        AcceptedAt   DATETIME2(0)  NOT NULL CONSTRAINT FRM_DF_AgrAt DEFAULT(SYSUTCDATETIME()),
        Ip           NVARCHAR(64)  NULL,
        CONSTRAINT FRM_PK_Agreements PRIMARY KEY (UserId, RulesVersion)
    );
GO
IF OBJECT_ID('dbo.ForumPostTags', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumPostTags
    (
        PostId  INT          NOT NULL,
        Tag     NVARCHAR(40) NOT NULL,     -- kucuk harf (Turkce kurallariyla)
        Display NVARCHAR(40) NOT NULL,     -- gonderide yazildigi hali
        CONSTRAINT FRM_PK_PostTags PRIMARY KEY (PostId, Tag)
    );
    CREATE INDEX FRM_IX_Tags_Tag ON dbo.ForumPostTags(Tag, PostId);
END
GO
PRINT N'Forum kural onayı ve etiket tabloları hazır. Mevcut gönderilerin etiketleri ilk açılışta otomatik işlenir.';
GO
