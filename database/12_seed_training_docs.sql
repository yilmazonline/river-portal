/* ============================================================================
   12 — Eğitim Merkezi başlangıç dokümanları (seed)
   Not: Bu satirlar 'web/trainingdocs/' altindaki dosyalara referans verir.
   Once ilgili dosyalari sunucuya kopyalayin, sonra bu script'i BIR KEZ calistirin.
   ============================================================================ */
USE PARTNER;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TrainingMaterials WHERE MaterialType = N'Document' AND DocStoredName = N'river-ai-enterprise.pdf')
INSERT INTO dbo.TrainingMaterials
    (CompanyId, TitleTr, TitleEn, DescriptionTr, Category, CategoryGroup, SubCategory, MaterialType, DocStoredName, DocOriginalName, DocContentType, DocFileSize, IsActive)
VALUES
    (NULL, N'River AI Kurumsal Tanıtım', N'River AI Enterprise',
     N'İş süreçlerinde yapay zeka çağı: tasarımcıdan son kullanıcıya, uçtan uca kurumsal çözüm platformu tanıtımı.',
     N'Technical', N'River AI', N'Tanıtım', N'Document', N'river-ai-enterprise.pdf', N'River_AI_Enterprise.pdf', N'application/pdf', 6801178, 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TrainingMaterials WHERE MaterialType = N'Document' AND DocStoredName = N'riverai-ekosistem.pptx')
INSERT INTO dbo.TrainingMaterials
    (CompanyId, TitleTr, TitleEn, DescriptionTr, Category, CategoryGroup, SubCategory, MaterialType, DocStoredName, DocOriginalName, DocContentType, DocFileSize, IsActive)
VALUES
    (NULL, N'River AI Ekosistem Sunumu', N'River AI Ecosystem',
     N'DMS, QMS, EBYS, İntranet ve sözleşme yönetimi; sınırsız modüler ekosistem sunumu.',
     N'Technical', N'River AI', N'Sunumlar', N'Document', N'riverai-ekosistem.pptx', N'RiverAI-Ekosistem.pptx',
     N'application/vnd.openxmlformats-officedocument.presentationml.presentation', 5284926, 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TrainingMaterials WHERE MaterialType = N'Document' AND DocStoredName = N'dokuman-yonetimi.pdf')
INSERT INTO dbo.TrainingMaterials
    (CompanyId, TitleTr, TitleEn, DescriptionTr, Category, CategoryGroup, SubCategory, MaterialType, DocStoredName, DocOriginalName, DocContentType, DocFileSize, IsActive)
VALUES
    (NULL, N'Doküman Yönetimi Sistemi', N'Document Management System',
     N'River Docs Ready: doküman yaşam döngüsü, onay akışları, OCR arama, e-imza ve versiyon yönetimi.',
     N'Technical', N'Doküman Yönetimi (DMS)', N'Tanıtım', N'Document', N'dokuman-yonetimi.pdf', N'Dokuman_Yonetimi.pdf', N'application/pdf', 3891042, 1);
GO

PRINT N'Baslangic egitim dokumanlari eklendi.';
GO
