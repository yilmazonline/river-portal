/* 65 - Lead Merkezi: rakip firma domainleri engelleme listesine (kendi calisanlari elenir; is ortaklari etkilenmez). Idempotent. */
USE PARTNER;
GO
MERGE dbo.LeadSuppression AS t
USING (VALUES
  (N'autommate.com', N'Olası rakip: Autom Mate'),
  (N'fileorbis.com', N'Olası rakip: FileOrbis'),
  (N'signumtte.com', N'Olası rakip: Signum TTE'),
  (N'secube.com.tr', N'Olası rakip: Secube Teknoloji Ar-ge A.ş'),
  (N'aksis.com.tr', N'Rakip: Aksis Dijital Çözümler'),
  (N'kod.com.tr', N'Olası rakip: Veboni ERP'),
  (N'bimser.com', N'Rakip: Bimser'),
  (N'fiz.com.tr', N'Olası rakip: Fiz Consulting'),
  (N'sozdijital.com', N'Olası rakip: Söz Dijital'),
  (N'epochtechnology.co', N'Olası rakip: Epoch Technology & Innovation'),
  (N'btarge.com', N'Rakip: Btarge'),
  (N'paperwork.com.tr', N'Rakip: PaperWork, Ölçemezseniz Yönetemezsiniz'),
  (N'flovo.app', N'Rakip: Flovo: Smart Business Solutions'),
  (N'codease.io', N'Rakip: Codease'),
  (N'harmona.ai', N'Olası rakip: Harmona AI'),
  (N'mrpyazilim.com', N'Olası rakip: Emar Proje Yazılım Dan. Ve Bil. Hiz. . Şti'),
  (N'geometa.ae', N'Rakip: Geometa'),
  (N'logo.com.tr', N'Olası rakip: eLogo'),
  (N'agentron.ai', N'Olası rakip: Emploidai'),
  (N'arvena.com.tr', N'Olası rakip: Arima Erp'),
  (N'kuika.com', N'Rakip: Kuika (low-code)'),
  (N'workcube.com', N'Rakip: Workcube')
) AS s(v, r)
ON t.Kind = N'domain' AND t.Value = s.v
WHEN NOT MATCHED THEN INSERT (Kind, Value, Reason) VALUES (N'domain', s.v, s.r);
GO
PRINT N'Rakip domainleri engelleme listesine eklendi.';
GO
