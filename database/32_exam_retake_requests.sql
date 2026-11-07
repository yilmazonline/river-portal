/* 32 - Yeniden sinav talepleri (deneme hakki bitince editor onayi). BIR KEZ calistirin. Idempotent. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.ExamRetakeRequests','U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamRetakeRequests (
        RequestId     INT IDENTITY(1,1) PRIMARY KEY,
        ExamId        INT            NOT NULL,
        UserId        INT            NOT NULL,
        CompanyId     INT            NOT NULL,
        Note          NVARCHAR(1000) NULL,          -- kullanicinin notu
        Status        NVARCHAR(12)   NOT NULL CONSTRAINT DF_Err_Status DEFAULT(N'Pending'),  -- Pending|Approved|Rejected|Used
        RequestedAt   DATETIME2(0)   NOT NULL CONSTRAINT DF_Err_Req DEFAULT(SYSUTCDATETIME()),
        DecidedAt     DATETIME2(0)   NULL,
        DecidedBy     INT            NULL,
        DecisionNote  NVARCHAR(1000) NULL,
        UsedAttemptId INT            NULL,
        CONSTRAINT FK_Err_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exams(ExamId) ON DELETE CASCADE,
        CONSTRAINT CK_Err_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected', N'Used'))
    );
    CREATE INDEX IX_Err_UserExam ON dbo.ExamRetakeRequests(UserId, ExamId, RequestedAt DESC);
    CREATE INDEX IX_Err_Status ON dbo.ExamRetakeRequests(Status);
END
GO
PRINT N'ExamRetakeRequests hazir.';
GO
