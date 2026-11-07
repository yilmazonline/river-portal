/* ==========================================================================
   66) BILGI TABANI + DESTEK TALEBI AI ON YANITI
   KbEntries : River Akademi videolarindan cikarilan konu kayitlari (JSON ile ice aktarilir)
   KbAnswers : talep formunda verilen on yanitlar ve sonuclari (cozuldu / talebe donustu / bulunamadi)
   ========================================================================== */
IF OBJECT_ID('dbo.KbEntries') IS NULL
BEGIN
    CREATE TABLE dbo.KbEntries
    (
        EntryId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KbEntries PRIMARY KEY,
        Code         NVARCHAR(40)   NOT NULL,              -- ornek: v19-4 (ice aktarmada eslestirme anahtari)
        Product      NVARCHAR(40)   NOT NULL CONSTRAINT DF_Kb_Product DEFAULT(N'River AI'),
        Area         NVARCHAR(80)   NOT NULL,
        Title        NVARCHAR(300)  NOT NULL,
        Summary      NVARCHAR(MAX)  NOT NULL,
        StepsJson    NVARCHAR(MAX)  NOT NULL,              -- [{t, text}]
        WarningsJson NVARCHAR(MAX)  NULL,                  -- ["..."]
        Keywords     NVARCHAR(MAX)  NULL,                  -- | ile ayrilmis
        Phrases      NVARCHAR(MAX)  NULL,                  -- | ile ayrilmis (kullanicinin gunluk ifadeleri)
        MaterialId   INT            NULL,                  -- TrainingMaterials.MaterialId
        YoutubeId    NVARCHAR(40)   NULL,
        VideoTitle   NVARCHAR(300)  NULL,
        StartSec     INT            NOT NULL CONSTRAINT DF_Kb_Start DEFAULT(0),
        IsActive     BIT            NOT NULL CONSTRAINT DF_Kb_Active DEFAULT(1),
        HitCount     INT            NOT NULL CONSTRAINT DF_Kb_Hits DEFAULT(0),   -- on yanitta kullanilma sayisi
        UpdatedAt    DATETIME2(0)   NOT NULL CONSTRAINT DF_Kb_Upd DEFAULT(SYSUTCDATETIME())
    );
    CREATE UNIQUE INDEX UX_KbEntries_Code ON dbo.KbEntries(Code);
END
GO
IF OBJECT_ID('dbo.KbAnswers') IS NULL
BEGIN
    CREATE TABLE dbo.KbAnswers
    (
        AnswerId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KbAnswers PRIMARY KEY,
        CompanyId    INT            NULL,
        UserId       INT            NULL,
        Product      NVARCHAR(40)   NOT NULL,
        RequestType  NVARCHAR(40)   NULL,
        Question     NVARCHAR(MAX)  NOT NULL,              -- kisisel bilgiler maskelenmis
        Mode         NVARCHAR(10)   NOT NULL,              -- ai / match / none
        AnswerJson   NVARCHAR(MAX)  NULL,
        EntryCodes   NVARCHAR(400)  NULL,                  -- kullanilan kayitlar
        Outcome      NVARCHAR(12)   NOT NULL CONSTRAINT DF_KbA_Out DEFAULT(N'shown'),  -- shown / solved / escalated
        TicketId     INT            NULL,
        CreatedAt    DATETIME2(0)   NOT NULL CONSTRAINT DF_KbA_Created DEFAULT(SYSUTCDATETIME()),
        ClosedAt     DATETIME2(0)   NULL
    );
    CREATE INDEX IX_KbAnswers_Created ON dbo.KbAnswers(CreatedAt DESC);
END
GO
