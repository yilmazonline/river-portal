/* Karsilama videosunu tum kullanicilara yeniden (bir kez) goster. */
USE PARTNER;
GO
UPDATE dbo.Users SET IntroSeenAt = NULL WHERE IntroSeenAt IS NOT NULL;
SELECT @@ROWCOUNT AS [Sifirlanan kullanici];
GO
