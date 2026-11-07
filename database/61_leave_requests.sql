/* 61 - Izin talepleri: profil (unvan, departman, ise giris, onaylayici), talepler, onay adimlari, islem gecmisi,
   ayarlar, resmi tatiller, planli surum / sprint tarihleri. Idempotent. Kisitlar LV_ onekli. */
USE PARTNER;
GO
IF OBJECT_ID('dbo.LeaveProfiles', 'U') IS NULL
CREATE TABLE dbo.LeaveProfiles
(
    UserId                 INT            NOT NULL CONSTRAINT LV_PK_Prof PRIMARY KEY,
    Title                  NVARCHAR(120)  NULL,
    Department             NVARCHAR(120)  NULL,
    HireDate               DATE           NULL,
    UsedDays               DECIMAL(5,1)   NOT NULL CONSTRAINT LV_DF_Used DEFAULT(0),   -- sistem disinda bu yil kullanilan
    ExtraDays              DECIMAL(5,1)   NOT NULL CONSTRAINT LV_DF_Extra DEFAULT(0),  -- devreden / ek hak
    OnCall                 BIT            NOT NULL CONSTRAINT LV_DF_OnCall DEFAULT(0),
    ApproverUserId         INT            NULL,
    NoApproval             BIT            NOT NULL CONSTRAINT LV_DF_NoAppr DEFAULT(0),
    DelegateApproverUserId INT            NULL                                          -- bu kisi izindeyken onayini kim verir
);
GO
IF OBJECT_ID('dbo.LeaveRequests', 'U') IS NULL
CREATE TABLE dbo.LeaveRequests
(
    RequestId      INT IDENTITY(1,1) NOT NULL CONSTRAINT LV_PK_Req PRIMARY KEY,
    RequestNo      NVARCHAR(20)   NULL,
    UserId         INT            NOT NULL,
    LeaveType      NVARCHAR(12)   NOT NULL,
    SubType        NVARCHAR(12)   NULL,
    StartDate      DATE           NOT NULL,
    EndDate        DATE           NOT NULL,
    StartHalf      BIT            NOT NULL CONSTRAINT LV_DF_SH DEFAULT(0),
    EndHalf        BIT            NOT NULL CONSTRAINT LV_DF_EH DEFAULT(0),
    HourFrom       NVARCHAR(5)    NULL,
    HourTo         NVARCHAR(5)    NULL,
    Days           DECIMAL(6,1)   NOT NULL,
    DelegateUserId INT            NULL,
    OnCallUserId   INT            NULL,
    Handover       NVARCHAR(800)  NULL,
    Reach          NVARCHAR(8)    NOT NULL CONSTRAINT LV_DF_Reach DEFAULT(N'none'),
    Phone          NVARCHAR(40)   NULL,
    LeaveAddress   NVARCHAR(200)  NULL,
    Note           NVARCHAR(600)  NULL,
    FileStored     NVARCHAR(60)   NULL,
    FileOriginal   NVARCHAR(200)  NULL,
    OooReminder    BIT            NOT NULL CONSTRAINT LV_DF_Ooo DEFAULT(1),
    Status         NVARCHAR(10)   NOT NULL,
    Step           INT            NOT NULL CONSTRAINT LV_DF_Step DEFAULT(0),
    StepStartedAt  DATETIME2(0)   NULL,
    Reminded       BIT            NOT NULL CONSTRAINT LV_DF_Rem DEFAULT(0),
    EscalatedTo    INT            NULL,
    BeforeSent     BIT            NOT NULL CONSTRAINT LV_DF_Bef DEFAULT(0),
    Reason         NVARCHAR(400)  NULL,
    Warnings       NVARCHAR(2000) NULL,
    CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT LV_DF_ReqAt DEFAULT(SYSUTCDATETIME()),
    UpdatedAt      DATETIME2(0)   NULL,
    CONSTRAINT LV_CK_Type CHECK (LeaveType IN (N'yillik', N'mazeret', N'hastalik', N'saatlik', N'ucretsiz', N'egitim', N'dogumgunu', N'dogum')),
    CONSTRAINT LV_CK_Status CHECK (Status IN (N'pending', N'approved', N'rejected', N'revise', N'cancelled', N'recorded'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LV_IX_ReqUser') CREATE INDEX LV_IX_ReqUser ON dbo.LeaveRequests(UserId, Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LV_IX_ReqDates') CREATE INDEX LV_IX_ReqDates ON dbo.LeaveRequests(StartDate, EndDate) INCLUDE (Status, UserId);
GO
IF OBJECT_ID('dbo.LeaveSteps', 'U') IS NULL
CREATE TABLE dbo.LeaveSteps
(
    StepId          INT IDENTITY(1,1) NOT NULL CONSTRAINT LV_PK_Step PRIMARY KEY,
    RequestId       INT           NOT NULL,
    StepNo          INT           NOT NULL,
    Label           NVARCHAR(60)  NOT NULL,
    ApproverUserId  INT           NOT NULL,
    OriginalUserId  INT           NULL,     -- asil onaylayici izindeyse
    Why             NVARCHAR(120) NULL,
    DecidedBy       INT           NULL,
    DecidedAt       DATETIME2(0)  NULL,
    Decision        NVARCHAR(10)  NULL,
    DecisionNote    NVARCHAR(400) NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LV_IX_StepReq') CREATE INDEX LV_IX_StepReq ON dbo.LeaveSteps(RequestId, StepNo);
GO
IF OBJECT_ID('dbo.LeaveLog', 'U') IS NULL
CREATE TABLE dbo.LeaveLog
(
    LogId     INT IDENTITY(1,1) NOT NULL CONSTRAINT LV_PK_Log PRIMARY KEY,
    RequestId INT            NOT NULL,
    At        DATETIME2(0)   NOT NULL CONSTRAINT LV_DF_LogAt DEFAULT(SYSUTCDATETIME()),
    Text      NVARCHAR(500)  NOT NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'LV_IX_LogReq') CREATE INDEX LV_IX_LogReq ON dbo.LeaveLog(RequestId);
GO
IF OBJECT_ID('dbo.LeaveSettings', 'U') IS NULL
CREATE TABLE dbo.LeaveSettings (SettingKey NVARCHAR(40) NOT NULL CONSTRAINT LV_PK_Set PRIMARY KEY, SettingValue NVARCHAR(200) NOT NULL);
GO
IF OBJECT_ID('dbo.LeaveHolidays', 'U') IS NULL
CREATE TABLE dbo.LeaveHolidays (Day DATE NOT NULL CONSTRAINT LV_PK_Hol PRIMARY KEY, Name NVARCHAR(120) NOT NULL, IsHalf BIT NOT NULL CONSTRAINT LV_DF_Half DEFAULT(0));
GO
IF OBJECT_ID('dbo.LeaveEvents', 'U') IS NULL
CREATE TABLE dbo.LeaveEvents (EventId INT IDENTITY(1,1) NOT NULL CONSTRAINT LV_PK_Ev PRIMARY KEY, Day DATE NOT NULL, Title NVARCHAR(160) NOT NULL);
GO
/* varsayilan ayarlar */
MERGE dbo.LeaveSettings AS t
USING (VALUES (N'longDays', N'5'), (N'unpaidSecond', N'1'), (N'sickNoApproval', N'1'), (N'remindHours', N'24'), (N'escalateHours', N'48')) AS s(k, v)
ON t.SettingKey = s.k WHEN NOT MATCHED THEN INSERT (SettingKey, SettingValue) VALUES (s.k, s.v);
GO
/* resmi tatiller: 2026 tamami, 2027 sabit tarihli olanlar (2027 bayramlari aciklaninca Kurallar sekmesinden eklenir). Yarim gun: arife */
MERGE dbo.LeaveHolidays AS t
USING (VALUES
 ('2026-01-01', N'Yılbaşı', 0), ('2026-03-19', N'Ramazan Bayramı arifesi', 1), ('2026-03-20', N'Ramazan Bayramı', 0), ('2026-03-21', N'Ramazan Bayramı', 0), ('2026-03-22', N'Ramazan Bayramı', 0),
 ('2026-04-23', N'Ulusal Egemenlik ve Çocuk Bayramı', 0), ('2026-05-01', N'Emek ve Dayanışma Günü', 0), ('2026-05-19', N'Atatürk''ü Anma, Gençlik ve Spor Bayramı', 0),
 ('2026-05-26', N'Kurban Bayramı arifesi', 1), ('2026-05-27', N'Kurban Bayramı', 0), ('2026-05-28', N'Kurban Bayramı', 0), ('2026-05-29', N'Kurban Bayramı', 0), ('2026-05-30', N'Kurban Bayramı', 0),
 ('2026-07-15', N'Demokrasi ve Millî Birlik Günü', 0), ('2026-08-30', N'Zafer Bayramı', 0), ('2026-10-28', N'Cumhuriyet Bayramı arifesi', 1), ('2026-10-29', N'Cumhuriyet Bayramı', 0),
 ('2027-01-01', N'Yılbaşı', 0), ('2027-04-23', N'Ulusal Egemenlik ve Çocuk Bayramı', 0), ('2027-05-01', N'Emek ve Dayanışma Günü', 0), ('2027-05-19', N'Atatürk''ü Anma, Gençlik ve Spor Bayramı', 0),
 ('2027-07-15', N'Demokrasi ve Millî Birlik Günü', 0), ('2027-08-30', N'Zafer Bayramı', 0), ('2027-10-28', N'Cumhuriyet Bayramı arifesi', 1), ('2027-10-29', N'Cumhuriyet Bayramı', 0)
) AS s(d, n, h)
ON t.Day = CAST(s.d AS DATE) WHEN NOT MATCHED THEN INSERT (Day, Name, IsHalf) VALUES (CAST(s.d AS DATE), s.n, s.h);
GO
PRINT N'İzin talepleri tabloları hazır.';
GO
