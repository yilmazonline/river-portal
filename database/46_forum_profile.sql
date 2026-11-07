/* 46 - Forum: profil sayfasi (kisinin gonderileri) icin dizin. Idempotent. */
USE PARTNER;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'FRM_IX_Posts_Author' AND object_id = OBJECT_ID('dbo.ForumPosts'))
    CREATE INDEX FRM_IX_Posts_Author ON dbo.ForumPosts(AuthorUserId, PostId DESC) WHERE DeletedAt IS NULL;
GO
PRINT N'Forum profil dizini hazır.';
GO
