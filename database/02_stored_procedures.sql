/* ============================================================================
   RIVER PARTNER PORTAL — İŞ MANTIĞI (Stored Procedures / Fonksiyonlar)
   ----------------------------------------------------------------------------
   İÇERİK:
     - fn_GetTierForRevenue      : cirodan kademe bulma
     - fn_GetMegaDealBonus       : tek anlaşma mega deal bonusu
     - usp_BookDeal              : anlaşma kaydı + hakediş hesabı + kademe geçişi
     - usp_RecalcCompanyTier     : firma kademesini/yıllık hakedişi güncelle
     - usp_CheckInactivity       : 12 ay hareketsizlik kuralı (zamanlanmış iş)
     - usp_ApproveDemoRequest    : demo onay/reschedule + bildirim
   ============================================================================ */
USE PARTNER;
GO

/* --------------------------------------------------------------------------
   KADEME BULMA — verilen yıllık ciroya karşılık gelen taban kademe
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.fn_GetTierForRevenue') IS NOT NULL DROP FUNCTION dbo.fn_GetTierForRevenue;
GO
CREATE FUNCTION dbo.fn_GetTierForRevenue(@Revenue DECIMAL(18,2))
RETURNS INT
AS
BEGIN
    DECLARE @TierId INT;
    SELECT TOP (1) @TierId = TierId
    FROM dbo.PartnerTiers
    WHERE @Revenue >= MinRevenue
      AND (@Revenue < MaxRevenue OR MaxRevenue IS NULL)
    ORDER BY SortOrder DESC;
    RETURN @TierId;
END;
GO

/* --------------------------------------------------------------------------
   MEGA DEAL BONUSU — tek anlaşma tutarına göre ek komisyon (%)
   Eşikler kümülatif değil; en yüksek uygulanabilir kademe alınır.
     1.5M+ -> +3   | 2.5M+ -> +5   | 4M+ -> +7   | 6M+ -> +10
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.fn_GetMegaDealBonus') IS NOT NULL DROP FUNCTION dbo.fn_GetMegaDealBonus;
GO
CREATE FUNCTION dbo.fn_GetMegaDealBonus(@DealAmount DECIMAL(18,2))
RETURNS DECIMAL(5,2)
AS
BEGIN
    RETURN CASE
        WHEN @DealAmount >= 6000000 THEN 10.00
        WHEN @DealAmount >= 4000000 THEN  7.00
        WHEN @DealAmount >= 2500000 THEN  5.00
        WHEN @DealAmount >= 1500000 THEN  3.00
        ELSE 0.00
    END;
END;
GO

/* --------------------------------------------------------------------------
   ANLAŞMA KAYDI (usp_BookDeal)
   KADEME GEÇİŞ KURALI (Prospective Threshold Progression):
     - Bu deal'in taban oranı, firmanın BU DEAL'DEN ÖNCEKİ kümülatif yıllık
       cirosuna göre belirlenir. Yani eşiği aşan deal ESKİ oranı alır;
       geriye dönük fark ödenmez. Sonraki deal'ler yeni oranı alır.
   HAKEDİŞ = Amount * (BaseRate + MegaBonus) / 100
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.usp_BookDeal') IS NOT NULL DROP PROCEDURE dbo.usp_BookDeal;
GO
CREATE PROCEDURE dbo.usp_BookDeal
    @CompanyId       INT,
    @Title           NVARCHAR(200),
    @CustomerName    NVARCHAR(200),
    @Amount          DECIMAL(18,2),
    @CreatedByUserId INT = NULL,
    @CreatedByVendor BIT = 0,
    @NewDealId       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    -- Yıl başından bu deal'den ÖNCEYE kadar olan kümülatif ciro
    DECLARE @YearStart DATETIME2(0) = DATEFROMPARTS(YEAR(SYSUTCDATETIME()), 1, 1);
    DECLARE @PriorRevenue DECIMAL(18,2) =
        ISNULL((SELECT SUM(Amount) FROM dbo.Deals
                WHERE CompanyId = @CompanyId
                  AND Status <> N'Rejected'
                  AND BookedAt >= @YearStart), 0);

    DECLARE @TierId INT   = dbo.fn_GetTierForRevenue(@PriorRevenue);
    DECLARE @BaseRate DECIMAL(5,2) = (SELECT BaseCommission FROM dbo.PartnerTiers WHERE TierId = @TierId);
    DECLARE @MegaRate DECIMAL(5,2) = dbo.fn_GetMegaDealBonus(@Amount);
    DECLARE @Commission DECIMAL(18,2) = ROUND(@Amount * (@BaseRate + @MegaRate) / 100.0, 2);

    INSERT INTO dbo.Deals
        (CompanyId, Title, CustomerName, Amount, AppliedTierId,
         BaseCommissionRate, MegaDealBonusRate, CommissionAmount,
         Status, CreatedByUserId, CreatedByVendor)
    VALUES
        (@CompanyId, @Title, @CustomerName, @Amount, @TierId,
         @BaseRate, @MegaRate, @Commission,
         N'Pending', @CreatedByUserId, @CreatedByVendor);

    SET @NewDealId = SCOPE_IDENTITY();

    -- Firma cirosu güncelle + gerekirse kademe yükselt (+ bildirim)
    EXEC dbo.usp_RecalcCompanyTier @CompanyId = @CompanyId;

    COMMIT TRAN;
END;
GO

/* --------------------------------------------------------------------------
   FİRMA KADEMESİNİ / YILLIK HAKEDİŞİ GÜNCELLE
   Kademe yükselmesi olduysa TierUpgrade bildirimi bırakır.
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.usp_RecalcCompanyTier') IS NOT NULL DROP PROCEDURE dbo.usp_RecalcCompanyTier;
GO
CREATE PROCEDURE dbo.usp_RecalcCompanyTier
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @YearStart DATETIME2(0) = DATEFROMPARTS(YEAR(SYSUTCDATETIME()), 1, 1);

    DECLARE @AnnualRevenue DECIMAL(18,2) =
        ISNULL((SELECT SUM(Amount) FROM dbo.Deals
                WHERE CompanyId = @CompanyId
                  AND Status <> N'Rejected'
                  AND BookedAt >= @YearStart), 0);

    DECLARE @OldTierId INT = (SELECT CurrentTierId FROM dbo.Companies WHERE CompanyId = @CompanyId);
    DECLARE @NewTierId INT = dbo.fn_GetTierForRevenue(@AnnualRevenue);

    UPDATE dbo.Companies
       SET AnnualNetRevenue = @AnnualRevenue,
           CurrentTierId     = @NewTierId,
           LastSaleDate      = SYSUTCDATETIME(),
           IsPassive         = 0
     WHERE CompanyId = @CompanyId;

    -- Kademe yükseldiyse bildirim (iki dilli)
    IF (@OldTierId IS NULL OR @NewTierId <> @OldTierId)
       AND (SELECT SortOrder FROM dbo.PartnerTiers WHERE TierId = @NewTierId)
         > ISNULL((SELECT SortOrder FROM dbo.PartnerTiers WHERE TierId = @OldTierId), 0)
    BEGIN
        DECLARE @TrName NVARCHAR(60) = (SELECT NameTr FROM dbo.PartnerTiers WHERE TierId = @NewTierId);
        DECLARE @EnName NVARCHAR(60) = (SELECT NameEn FROM dbo.PartnerTiers WHERE TierId = @NewTierId);

        INSERT INTO dbo.Notifications (CompanyId, TitleTr, TitleEn, BodyTr, BodyEn, Type)
        VALUES (@CompanyId,
                N'Tebrikler! Kademe yükseldiniz',
                N'Congratulations! Tier upgraded',
                N'Artık ' + @TrName + N' seviyesindesiniz. Yeni taban komisyon oranınız sonraki anlaşmalarda geçerli olacak.',
                N'You are now at ' + @EnName + N' level. Your new base commission applies to subsequent deals.',
                N'TierUpgrade');
    END
END;
GO

/* --------------------------------------------------------------------------
   12 AY HAREKETSİZLİK KURALI (zamanlanmış iş — SQL Agent günlük çalıştırır)
   Son 12 ayda hiç satışı olmayan aktif partnerleri Passive işaretler.
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.usp_CheckInactivity') IS NOT NULL DROP PROCEDURE dbo.usp_CheckInactivity;
GO
CREATE PROCEDURE dbo.usp_CheckInactivity
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Cutoff DATETIME2(0) = DATEADD(MONTH, -12, SYSUTCDATETIME());

    UPDATE c
       SET c.IsPassive = 1
    FROM dbo.Companies c
    WHERE c.IsVendor = 0
      AND c.IsActive = 1
      AND c.IsPassive = 0
      AND NOT EXISTS (
          SELECT 1 FROM dbo.Deals d
          WHERE d.CompanyId = c.CompanyId
            AND d.Status <> N'Rejected'
            AND d.BookedAt >= @Cutoff);

    -- Passive olan firmalara sistem bildirimi
    INSERT INTO dbo.Notifications (CompanyId, TitleTr, TitleEn, BodyTr, BodyEn, Type)
    SELECT c.CompanyId,
           N'Hesabınız pasif duruma alındı',
           N'Your account is now passive',
           N'Son 12 ayda satış kaydınız bulunmadığı için hesabınız pasife alındı. Yeniden aktifleşmek için bir anlaşma kaydedin.',
           N'No sales in the last 12 months, so your account was set to passive. Book a deal to reactivate.',
           N'System'
    FROM dbo.Companies c
    WHERE c.IsPassive = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.Notifications n
                      WHERE n.CompanyId = c.CompanyId AND n.Type = N'System'
                        AND n.CreatedAt >= DATEADD(DAY, -1, SYSUTCDATETIME()));
END;
GO

/* --------------------------------------------------------------------------
   DEMO TALEBİ ONAY / RESCHEDULE + BİLDİRİM
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.usp_ApproveDemoRequest') IS NOT NULL DROP PROCEDURE dbo.usp_ApproveDemoRequest;
GO
CREATE PROCEDURE dbo.usp_ApproveDemoRequest
    @DemoRequestId   INT,
    @VendorUserId    INT,
    @ScheduledDate   DATETIME2(0),
    @EnvironmentInfo NVARCHAR(500) = NULL,
    @IsReschedule    BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    DECLARE @CompanyId INT, @ClientName NVARCHAR(200);
    SELECT @CompanyId = CompanyId, @ClientName = ClientName
    FROM dbo.DemoRequests WHERE DemoRequestId = @DemoRequestId;

    UPDATE dbo.DemoRequests
       SET Status           = CASE WHEN @IsReschedule = 1 THEN N'Rescheduled' ELSE N'Approved' END,
           ScheduledDate    = @ScheduledDate,
           EnvironmentInfo  = @EnvironmentInfo,
           ApprovedByUserId = @VendorUserId,
           UpdatedAt        = SYSUTCDATETIME()
     WHERE DemoRequestId = @DemoRequestId;

    -- Partnere anlık bildirim (iki dilli)
    INSERT INTO dbo.Notifications (CompanyId, TitleTr, TitleEn, BodyTr, BodyEn, Type, LinkUrl)
    VALUES (@CompanyId,
            N'Demo talebiniz onaylandı',
            N'Your demo request is approved',
            N'"' + @ClientName + N'" için demo ' + FORMAT(@ScheduledDate, N'dd.MM.yyyy HH:mm') + N' tarihine planlandı.',
            N'Demo for "' + @ClientName + N'" scheduled on ' + FORMAT(@ScheduledDate, N'yyyy-MM-dd HH:mm') + N'.',
            N'DemoApproved',
            N'/partner/demos/' + CAST(@DemoRequestId AS NVARCHAR(10)));

    COMMIT TRAN;
END;
GO

PRINT N'İş mantığı yüklendi: 2 fonksiyon + 4 prosedür.';
GO
