/* 38 - Alacak takibi: Sorumlu (satis kaynagi). Idempotent.
   Teklif'teki SatisKaynagi DOLU olan siparislerden doldurulur; bos olanlar bos kalir (cikarim yapilmaz).
   Alacak detay ekranindan elle de secilebilir: Kanal Satış / Direkt Satış. */
USE PARTNER;
GO
IF COL_LENGTH('dbo.Receivables', 'SalesChannel') IS NULL
    ALTER TABLE dbo.Receivables ADD SalesChannel NVARCHAR(30) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'RCV_CK_Channel')
    ALTER TABLE dbo.Receivables ADD CONSTRAINT RCV_CK_Channel CHECK (SalesChannel IS NULL OR SalesChannel IN (N'Kanal Satış', N'Direkt Satış'));
GO
/* Daha once aktarilmis alacaklar: yalniz kaynakta deger varsa ve alacakta henuz secilmemisse */
UPDATE r SET SalesChannel =
    CASE WHEN s.SatisKaynagi LIKE N'%direkt%' OR s.SatisKaynagi LIKE N'%direct%' THEN N'Direkt Satış'
         WHEN s.SatisKaynagi LIKE N'%kanal%' OR s.SatisKaynagi LIKE N'%bayi%' OR s.SatisKaynagi LIKE N'%partner%' THEN N'Kanal Satış' END
FROM dbo.Receivables r
JOIN [RiverTeklif].[dbo].[Siparisler] s ON s.Id = r.SourceOrderId
WHERE r.SalesChannel IS NULL AND s.SatisKaynagi IS NOT NULL AND LTRIM(RTRIM(s.SatisKaynagi)) <> N'';
PRINT N'Sorumlu (satış kaynağı) hazır: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' alacak dolduruldu.';
GO
