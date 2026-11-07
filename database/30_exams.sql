/* 30 - Akademi sinav + sertifika sistemi. BIR KEZ calistirin. Idempotent. */
USE PARTNER;
GO

/* ---- Sinavlar ---- */
IF OBJECT_ID('dbo.Exams','U') IS NULL
BEGIN
    CREATE TABLE dbo.Exams (
        ExamId         INT IDENTITY(1,1) PRIMARY KEY,
        Title          NVARCHAR(200)  NOT NULL,
        Description    NVARCHAR(2000) NULL,
        CategoryGroup  NVARCHAR(120)  NULL,           -- giris kosulu: bu gruptaki tum videolar izlenmeli (NULL = kosul yok)
        Status         NVARCHAR(12)   NOT NULL CONSTRAINT DF_Ex_Status DEFAULT(N'Draft'),  -- Draft|Published|Active|Archived
        PassScore      INT            NOT NULL CONSTRAINT DF_Ex_Pass DEFAULT(70),
        MaxAttempts    INT            NOT NULL CONSTRAINT DF_Ex_Max DEFAULT(3),
        CooldownHours  INT            NOT NULL CONSTRAINT DF_Ex_Cool DEFAULT(24),
        TimeLimitMin   INT            NULL,           -- NULL = sure siniri yok
        ValidMonths    INT            NULL,           -- sertifika gecerliligi; NULL = suresiz
        CreatedBy      INT            NULL,
        CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT DF_Ex_Created DEFAULT(SYSUTCDATETIME()),
        UpdatedAt      DATETIME2(0)   NOT NULL CONSTRAINT DF_Ex_Updated DEFAULT(SYSUTCDATETIME()),
        PublishedAt    DATETIME2(0)   NULL,
        ActivatedAt    DATETIME2(0)   NULL,
        CONSTRAINT CK_Ex_Status CHECK (Status IN (N'Draft', N'Published', N'Active', N'Archived'))
    );
END
GO

/* ---- Sorular ---- */
IF OBJECT_ID('dbo.ExamQuestions','U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamQuestions (
        QuestionId  INT IDENTITY(1,1) PRIMARY KEY,
        ExamId      INT            NOT NULL,
        QText       NVARCHAR(2000) NOT NULL,
        QType       NVARCHAR(10)   NOT NULL CONSTRAINT DF_Eq_Type DEFAULT(N'single'),  -- single|multi|tf
        Points      INT            NOT NULL,
        SortOrder   INT            NOT NULL CONSTRAINT DF_Eq_Sort DEFAULT(0),
        CONSTRAINT FK_Eq_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exams(ExamId) ON DELETE CASCADE,
        CONSTRAINT CK_Eq_Type CHECK (QType IN (N'single', N'multi', N'tf')),
        CONSTRAINT CK_Eq_Pts  CHECK (Points BETWEEN 1 AND 100)
    );
    CREATE INDEX IX_Eq_Exam ON dbo.ExamQuestions(ExamId, SortOrder);
END
GO

/* ---- Siklar ---- */
IF OBJECT_ID('dbo.ExamOptions','U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamOptions (
        OptionId    INT IDENTITY(1,1) PRIMARY KEY,
        QuestionId  INT            NOT NULL,
        OText       NVARCHAR(1000) NOT NULL,
        IsCorrect   BIT            NOT NULL CONSTRAINT DF_Eo_Correct DEFAULT(0),
        SortOrder   INT            NOT NULL CONSTRAINT DF_Eo_Sort DEFAULT(0),
        CONSTRAINT FK_Eo_Q FOREIGN KEY (QuestionId) REFERENCES dbo.ExamQuestions(QuestionId) ON DELETE CASCADE
    );
    CREATE INDEX IX_Eo_Q ON dbo.ExamOptions(QuestionId, SortOrder);
END
GO

/* ---- Denemeler ---- */
IF OBJECT_ID('dbo.ExamAttempts','U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamAttempts (
        AttemptId    INT IDENTITY(1,1) PRIMARY KEY,
        ExamId       INT          NOT NULL,
        UserId       INT          NOT NULL,
        CompanyId    INT          NOT NULL,
        StartedAt    DATETIME2(0) NOT NULL CONSTRAINT DF_Ea_Start DEFAULT(SYSUTCDATETIME()),
        DeadlineAt   DATETIME2(0) NULL,
        SubmittedAt  DATETIME2(0) NULL,
        Score        INT          NULL,
        Passed       BIT          NULL,
        CONSTRAINT FK_Ea_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exams(ExamId)
    );
    CREATE INDEX IX_Ea_User ON dbo.ExamAttempts(UserId, ExamId);
    CREATE INDEX IX_Ea_Exam ON dbo.ExamAttempts(ExamId);
END
GO

/* ---- Verilen cevaplar ---- */
IF OBJECT_ID('dbo.ExamAnswers','U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamAnswers (
        AnswerId      INT IDENTITY(1,1) PRIMARY KEY,
        AttemptId     INT           NOT NULL,
        QuestionId    INT           NOT NULL,
        OptionIds     NVARCHAR(400) NULL,   -- secilen sik id'leri (virgullu)
        IsCorrect     BIT           NOT NULL CONSTRAINT DF_Ean_Ok DEFAULT(0),
        PointsEarned  INT           NOT NULL CONSTRAINT DF_Ean_Pts DEFAULT(0),
        CONSTRAINT FK_Ean_At FOREIGN KEY (AttemptId) REFERENCES dbo.ExamAttempts(AttemptId) ON DELETE CASCADE
    );
    CREATE INDEX IX_Ean_At ON dbo.ExamAnswers(AttemptId);
END
GO

/* ---- Sertifikalar ---- */
IF OBJECT_ID('dbo.Certificates','U') IS NULL
BEGIN
    CREATE TABLE dbo.Certificates (
        CertificateId  INT IDENTITY(1,1) PRIMARY KEY,
        CertNo         NVARCHAR(30)  NULL,     -- RVR-2026-000123 (kayittan sonra atanir)
        VerifyCode     NVARCHAR(20)  NOT NULL, -- QR dogrulama kodu
        UserId         INT           NOT NULL,
        CompanyId      INT           NOT NULL,
        ExamId         INT           NOT NULL,
        AttemptId      INT           NOT NULL,
        HolderName     NVARCHAR(160) NOT NULL,
        ExamTitle      NVARCHAR(200) NOT NULL,
        CompanyName    NVARCHAR(200) NULL,
        Score          INT           NOT NULL,
        IssuedAt       DATETIME2(0)  NOT NULL CONSTRAINT DF_Ce_Issued DEFAULT(SYSUTCDATETIME()),
        ValidUntil     DATETIME2(0)  NULL,
        IsRevoked      BIT           NOT NULL CONSTRAINT DF_Ce_Rev DEFAULT(0),
        CONSTRAINT UQ_Ce_Code UNIQUE (VerifyCode),
        CONSTRAINT UQ_Ce_UserExam UNIQUE (UserId, ExamId)
    );
    CREATE INDEX IX_Ce_User ON dbo.Certificates(UserId);
END
GO

/* ---- Sertifikada gorunecek ad (kullanici bir kez teyit eder) ---- */
IF COL_LENGTH('dbo.Users','CertificateName') IS NULL
    ALTER TABLE dbo.Users ADD CertificateName NVARCHAR(160) NULL;
GO

PRINT N'Sinav + sertifika tablolari hazir.';
GO
