/* 39 - "Sifirdan Uzmanliga" ogrenme yolu + rozetler + basari paylasimi. Idempotent. Kisit adlari LPATH_ onekli. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.LearningPathSteps', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LearningPathSteps
    (
        StepId     INT IDENTITY(1,1) NOT NULL CONSTRAINT LPATH_PK_Step PRIMARY KEY,
        MaterialId INT          NOT NULL,
        SortOrder  INT          NOT NULL CONSTRAINT LPATH_DF_Sort DEFAULT(0),
        IsActive   BIT          NOT NULL CONSTRAINT LPATH_DF_Active DEFAULT(1),
        CreatedAt  DATETIME2(0) NOT NULL CONSTRAINT LPATH_DF_StepCreated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT LPATH_UQ_Material UNIQUE (MaterialId),
        CONSTRAINT LPATH_FK_Material FOREIGN KEY (MaterialId) REFERENCES dbo.TrainingMaterials(MaterialId)
    );
END
GO
IF OBJECT_ID('dbo.UserBadges', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserBadges
    (
        UserBadgeId  INT IDENTITY(1,1) NOT NULL CONSTRAINT LPATH_PK_Badge PRIMARY KEY,
        UserId       INT           NOT NULL,
        BadgeKey     NVARCHAR(40)  NOT NULL,      -- video:<MaterialId> / m:first / m:half / path:done
        MaterialId   INT           NULL,
        EarnedAt     DATETIME2(0)  NOT NULL CONSTRAINT LPATH_DF_Earned DEFAULT(SYSUTCDATETIME()),
        Celebrated   BIT           NOT NULL CONSTRAINT LPATH_DF_Celebrated DEFAULT(0),   -- kutlama ekrani gosterildi mi
        IsRetro      BIT           NOT NULL CONSTRAINT LPATH_DF_Retro DEFAULT(0),        -- gecmise donuk, bildirimsiz verildi
        CONSTRAINT LPATH_UQ_UserBadge UNIQUE (UserId, BadgeKey)
    );
    CREATE INDEX LPATH_IX_Badge_User ON dbo.UserBadges(UserId, EarnedAt);
END
GO
IF COL_LENGTH('dbo.Users', 'ShareAchievements') IS NULL
    ALTER TABLE dbo.Users ADD ShareAchievements BIT NOT NULL CONSTRAINT LPATH_DF_Share DEFAULT(1);
GO
IF OBJECT_ID('dbo.AcademySettings', 'U') IS NULL
    CREATE TABLE dbo.AcademySettings (SettingKey NVARCHAR(50) NOT NULL CONSTRAINT LPATH_PK_Set PRIMARY KEY, SettingValue NVARCHAR(200) NULL);
GO
/* Kurulum ani: bu andan ONCE tamamlanmis ve izleme kaydi olmayan videolar iyi niyetle sayilir; SONRASI icin sona sarma rozet kazandirmaz */
IF NOT EXISTS (SELECT 1 FROM dbo.AcademySettings WHERE SettingKey = N'PathInstalledAtUtc')
    INSERT INTO dbo.AcademySettings VALUES (N'PathInstalledAtUtc', CONVERT(NVARCHAR(30), SYSUTCDATETIME(), 126));
GO
/* Baslangic sirasi (Egitim Videolari Listesi). 3. adim (Tenant/kullanici/rol ayarlari) video yuklenince Icerik Yonetimi'nden eklenir. */
IF NOT EXISTS (SELECT 1 FROM dbo.LearningPathSteps)
BEGIN
    DECLARE @map TABLE (Ord INT, Pat1 NVARCHAR(100), Pat2 NVARCHAR(100), Label NVARCHAR(100));
    INSERT INTO @map VALUES
        (10, N'%Giriş Ekran%',        N'%Login%',              N'1 · Login Ekran Eğitimi'),
        (20, N'%Ana Sayfa%',          N'%Ana Ekran%',          N'2 · Ana Ekran Tanıtımı'),
        (40, N'%Genel%Yapılandırma%', N'%Genel Proje%',        N'4 · Genel Proje Yapılandırması'),
        (50, N'%Zek%Form Tasarım%',   N'%AI%Form Tasarım%',    N'5 · AI ile Form Tasarımı'),
        (60, N'%Şablon%',             N'%Sablon%',             N'6 · Hazır Form Şablonlarının Kullanımı'),
        (70, N'%Form Tasarımcı%',     N'%Tasarımcısı%',        N'7 · Form Tasarımcısı Detayları'),
        (80, N'%Akış Tasarım%',       N'%Süreç Oluştur%',      N'8 · AI ile Süreç Oluşturma'),
        (90, N'%Katalog%',            N'%Kart Yapı%',          N'9 · Katalog Yapısı Oluşturma');
    DECLARE @ord INT, @p1 NVARCHAR(100), @p2 NVARCHAR(100), @lbl NVARCHAR(100), @mid INT, @title NVARCHAR(300);
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT Ord, Pat1, Pat2, Label FROM @map ORDER BY Ord;
    OPEN c; FETCH NEXT FROM c INTO @ord, @p1, @p2, @lbl;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @mid = NULL; SET @title = NULL;
        SELECT TOP 1 @mid = m.MaterialId, @title = m.TitleTr FROM dbo.TrainingMaterials m
        WHERE m.MaterialType = N'Video' AND m.IsActive = 1 AND (m.TitleTr LIKE @p1 OR m.TitleTr LIKE @p2)
          AND NOT EXISTS (SELECT 1 FROM dbo.LearningPathSteps s WHERE s.MaterialId = m.MaterialId)
        ORDER BY m.MaterialId;
        IF @mid IS NOT NULL
        BEGIN
            INSERT INTO dbo.LearningPathSteps (MaterialId, SortOrder) VALUES (@mid, @ord);
            PRINT N'✓ ' + @lbl + N'  →  ' + @title;
        END
        ELSE PRINT N'✗ ' + @lbl + N'  →  EŞLEŞEN VİDEO BULUNAMADI (Editör Paneli › Sıfırdan Uzmanlığa sekmesinden ekleyin)';
        FETCH NEXT FROM c INTO @ord, @p1, @p2, @lbl;
    END
    CLOSE c; DEALLOCATE c;
END
GO
PRINT N'Sıfırdan Uzmanlığa: tablolar hazır. Geçmişe dönük rozetler kullanıcılar Akademi''yi ilk açtığında bildirimsiz verilir.';
GO
