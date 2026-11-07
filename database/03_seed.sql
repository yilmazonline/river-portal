/* ============================================================================
   RIVER PARTNER PORTAL — BAŞLANGIÇ VERİSİ (Seed)
   ============================================================================ */
USE PARTNER;
GO

/* ---- Kademeler (spec'teki eşikler ve komisyonlar) ---- */
SET IDENTITY_INSERT dbo.PartnerTiers ON;
INSERT INTO dbo.PartnerTiers
    (TierId, Code, NameTr, NameEn, MinRevenue, MaxRevenue, BaseCommission, SlaTr, SlaEn, SortOrder)
VALUES
    (1, N'Registered', N'Kayıtlı Partner', N'Registered Partner',       0,       1250000, 30.00, N'Self-Servis Destek',     N'Self-Service Support', 1),
    (2, N'Silver',     N'Silver Partner',  N'Silver Partner',      1250000,       2500000, 36.00, N'Standart SLA',           N'Standard SLA',         2),
    (3, N'Gold',       N'Gold Partner',    N'Gold Partner',        2500000,       4000000, 42.00, N'Hızlı SLA',              N'Fast SLA',             3),
    (4, N'Platinum',   N'Platinum Partner',N'Platinum Partner',    4000000,          NULL, 50.00, N'Stratejik & Öncelikli',  N'Strategic & Priority', 4);
SET IDENTITY_INSERT dbo.PartnerTiers OFF;
GO

/* ---- Vendor firması + örnek partnerler ---- */
INSERT INTO dbo.Companies (Name, IsVendor, PreferredCulture) VALUES (N'River (Vendor)', 1, N'tr');
INSERT INTO dbo.Companies (Name, IsVendor, PreferredCulture) VALUES (N'Anadolu Yazılım A.Ş.', 0, N'tr');
INSERT INTO dbo.Companies (Name, IsVendor, PreferredCulture) VALUES (N'Bosphorus Tech Ltd.', 0, N'en');
INSERT INTO dbo.Companies (Name, IsVendor, PreferredCulture) VALUES (N'Ege Dijital Çözümler', 0, N'tr');
GO

/* ---- Kullanıcılar (PasswordHash: gerçek uygulamada BCrypt/PBKDF2) ---- */
INSERT INTO dbo.Users (CompanyId, FullName, Email, PasswordHash, Role, PreferredCulture) VALUES
    (1, N'River Admin',  N'admin@river.com.tr',      N'4b955e77d2357c10ebd5575c4b5adc4e345b729875c4a4dd66f821d4732b581d', N'VendorAdmin',  N'tr'),
    (2, N'Yılmaz Demir', N'yilmaz@anadoluyazilim.com',N'4b955e77d2357c10ebd5575c4b5adc4e345b729875c4a4dd66f821d4732b581d', N'PartnerAdmin', N'tr'),
    (2, N'Ayşe Kaya',    N'ayse@anadoluyazilim.com', N'4b955e77d2357c10ebd5575c4b5adc4e345b729875c4a4dd66f821d4732b581d', N'PartnerUser',  N'tr'),
    (3, N'John Baker',   N'john@bosphorustech.com',  N'4b955e77d2357c10ebd5575c4b5adc4e345b729875c4a4dd66f821d4732b581d', N'PartnerAdmin', N'en');
GO

/* ---- Örnek anlaşmalar (usp_BookDeal üzerinden — kademe geçişini tetikler) ---- */
DECLARE @d INT;
EXEC dbo.usp_BookDeal @CompanyId=2, @Title=N'ERP Modülü Lisansı', @CustomerName=N'Migros', @Amount=800000,  @NewDealId=@d OUTPUT;
EXEC dbo.usp_BookDeal @CompanyId=2, @Title=N'No-Code Platform',    @CustomerName=N'Türk Telekom', @Amount=1800000, @NewDealId=@d OUTPUT; -- mega +3
EXEC dbo.usp_BookDeal @CompanyId=2, @Title=N'Kurumsal Paket',      @CustomerName=N'Sabancı', @Amount=2700000, @NewDealId=@d OUTPUT;      -- mega +5, kademe atlar
EXEC dbo.usp_BookDeal @CompanyId=3, @Title=N'Enterprise License',  @CustomerName=N'Vodafone', @Amount=600000, @NewDealId=@d OUTPUT;
GO

/* ---- Bekleyen demo talebi ---- */
INSERT INTO dbo.DemoRequests (CompanyId, RequestedByUserId, ClientName, ClientContact, ProductModule, PreferredDate, Status)
VALUES (2, 2, N'Koç Holding', N'it@koc.com.tr', N'No-Code Workflow', DATEADD(DAY, 5, SYSUTCDATETIME()), N'Pending');
GO

/* ---- LMS içerikleri (global — CompanyId NULL) ---- */
INSERT INTO dbo.TrainingMaterials (CompanyId, TitleTr, TitleEn, Category, MaterialType, YoutubeId, SortOrder) VALUES
    (NULL, N'River No-Code 101',        N'River No-Code 101',        N'Technical',     N'Video', N'dQw4w9WgXcQ', 1),
    (NULL, N'Satış Argümanları',        N'Sales Playbook',           N'Sales',         N'Video', N'dQw4w9WgXcQ', 2),
    (NULL, N'Sürüm Notları 2026 Q3',    N'Release Notes 2026 Q3',    N'ProductUpdate', N'Video', N'dQw4w9WgXcQ', 3);
INSERT INTO dbo.TrainingMaterials (CompanyId, TitleTr, TitleEn, Category, MaterialType, FileUrl, SortOrder) VALUES
    (NULL, N'Mimari Teknik Doküman',    N'Architecture Whitepaper',  N'Technical', N'Document', N'/media/whitepaper.pdf', 4),
    (NULL, N'SLA Rehberi',              N'SLA Guidelines',           N'Sales',     N'Document', N'/media/sla.pdf',        5);
GO

PRINT N'Seed tamamlandı.';
SELECT c.Name, t.NameTr AS Kademe, c.AnnualNetRevenue AS YillikCiro
FROM dbo.Companies c LEFT JOIN dbo.PartnerTiers t ON t.TierId = c.CurrentTierId
WHERE c.IsVendor = 0;
GO
