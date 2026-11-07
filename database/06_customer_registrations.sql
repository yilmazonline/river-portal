/* ============================================================================
   06 — Müşteri Kayıt (lead koruma) sistemi
   Canli PARTNER veritabaninda BIR KEZ calistirilir. Idempotent.
   Kural: kaydedilen musteri 60 gun boyunca baska is ortagi tarafindan
   kaydedilemez/satis yapilamaz; sonuclanmazsa kayit duser (Expired).
   ============================================================================ */
USE PARTNER;
GO

IF OBJECT_ID('dbo.CustomerRegistrations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerRegistrations
    (
        RegistrationId     INT IDENTITY(1,1) NOT NULL,
        CompanyId          INT               NOT NULL,   -- kaydi yapan is ortagi (bayi)
        CustomerName       NVARCHAR(200)     NOT NULL,
        ContactPerson      NVARCHAR(200)     NULL,
        ContactEmail       NVARCHAR(256)     NULL,
        ContactPhone       NVARCHAR(40)      NULL,
        ServiceType        NVARCHAR(100)     NULL,
        SalesTemperature   INT               NOT NULL CONSTRAINT DF_CustReg_Temp DEFAULT(50),
        Status             NVARCHAR(20)      NOT NULL CONSTRAINT DF_CustReg_Status DEFAULT(N'Pending'),
        RegisteredAt       DATETIME2(0)      NOT NULL CONSTRAINT DF_CustReg_Reg DEFAULT(SYSUTCDATETIME()),
        ApprovedAt         DATETIME2(0)      NULL,
        ExpiresAt          DATETIME2(0)      NULL,       -- koruma bitis (60 gun)
        RenewalRequested   BIT               NOT NULL CONSTRAINT DF_CustReg_Renew DEFAULT(0),
        RenewalRequestedAt DATETIME2(0)      NULL,
        CreatedByUserId    INT               NULL,
        CONSTRAINT PK_CustomerRegistrations PRIMARY KEY CLUSTERED (RegistrationId),
        CONSTRAINT FK_CustReg_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(CompanyId),
        CONSTRAINT CK_CustReg_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected', N'Expired', N'Closed')),
        CONSTRAINT CK_CustReg_Temp CHECK (SalesTemperature BETWEEN 1 AND 100)
    );
    CREATE INDEX IX_CustReg_Company ON dbo.CustomerRegistrations(CompanyId);
    CREATE INDEX IX_CustReg_Customer ON dbo.CustomerRegistrations(CustomerName);
    PRINT N'CustomerRegistrations tablosu olusturuldu.';
END
ELSE
    PRINT N'CustomerRegistrations zaten mevcut.';
GO
