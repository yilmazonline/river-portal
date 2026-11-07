/* 48 - Forum: yorum begenisi ve yoruma yanit (tek seviye). Idempotent. Kisitlar FRM_ onekli. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.ForumComments', 'ParentCommentId') IS NULL
    ALTER TABLE dbo.ForumComments ADD ParentCommentId INT NULL;   -- NULL = ust duzey yorum; dolu = o yoruma yanit
GO
IF COL_LENGTH('dbo.ForumComments', 'LikeCount') IS NULL
    ALTER TABLE dbo.ForumComments ADD LikeCount INT NOT NULL CONSTRAINT FRM_DF_CLikes DEFAULT(0);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'FRM_IX_Com_Parent' AND object_id = OBJECT_ID('dbo.ForumComments'))
    CREATE INDEX FRM_IX_Com_Parent ON dbo.ForumComments(ParentCommentId) WHERE ParentCommentId IS NOT NULL;
GO
IF OBJECT_ID('dbo.ForumCommentLikes', 'U') IS NULL
    CREATE TABLE dbo.ForumCommentLikes (CommentId INT NOT NULL, UserId INT NOT NULL, CreatedAt DATETIME2(0) NOT NULL CONSTRAINT FRM_DF_CLCreated DEFAULT(SYSUTCDATETIME()), CONSTRAINT FRM_PK_CLikes PRIMARY KEY (CommentId, UserId));
GO
PRINT N'Yorum beğenisi ve yanıtları hazır.';
GO
