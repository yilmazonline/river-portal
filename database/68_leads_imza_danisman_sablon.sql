/* ==========================================================================
   68) LEAD MERKEZI: OTOMATIK IMZA KALDIRILDI + DANISMAN ILK E-POSTA SABLONU
   - Ayarlar > Imza bosaltilir: "River AI · Fol Yazılım A.Ş. / fol.solutions" artik
     hicbir e-postanin sonuna eklenmez (imza gonderimde eklendigi icin onay
     kuyrugundaki taslaklar da etkilenir). Ret cumlesi (optOut) eklenmeye devam eder.
   - Danisman (sales@) ilk e-posta sablonunun metni yenilenir; imza sablonun icindedir.
   - Mevcut taslaklarin (LeadMessages) metnine dokunulmaz.
   Tekrar calistirmaya dayanikli (ayni degerleri yeniden yazar).
   ========================================================================== */
IF OBJECT_ID('dbo.LeadSettings', 'U') IS NOT NULL
    UPDATE dbo.LeadSettings SET SettingValue = N'' WHERE SettingKey = N'signature' AND SettingValue <> N'';
GO
IF OBJECT_ID('dbo.LeadTemplates', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.LeadTemplates SET UpdatedAt = SYSUTCDATETIME(), Body =
        N'Merhaba {Hitap},' + NCHAR(10) + NCHAR(10) +
        N'Umarım iyisinizdir. Çalışmalarınızı ve etkinliklerinizi ilgiyle takip ediyoruz.' + NCHAR(10) + NCHAR(10) +
        N'Fol Yazılım olarak, kodsuz ve yapay zekâ destekli süreç yönetim platformu River AI’ın üreticisiyiz. River AI bugün 150''yi aşkın orta ve büyük ölçekli işletmede, 10.000''den fazla kullanıcı tarafından aktif olarak kullanılıyor.' + NCHAR(10) + NCHAR(10) +
        N'Danışmanlık firmalarıyla iş ortaklığı modeliyle çalışıyoruz. İş ortaklarımıza müşterilerine hızlı teslim edebilecekleri hazır modüller, satışlardan gelir payı ve ekipleri için eğitim ve sertifika programı sunuyoruz.' + NCHAR(10) + NCHAR(10) +
        N'Sizler için de uygun bir zamanda platformumuzu ve iş ortaklığı modelimizi kısa bir çevrimiçi toplantıda tanıtmak isteriz.' + NCHAR(10) + NCHAR(10) +
        N'İyi çalışmalar dilerim.' + NCHAR(10) +
        N'Saygılarımla,' + NCHAR(10) + NCHAR(10) +
        N'Yılmaz Güney' + NCHAR(10) +
        N'İş Ortakları Yöneticisi' + NCHAR(10) +
        N'0541 857 94 74' + NCHAR(10) +
        N'www.river.com.tr'
    WHERE Category = N'danisman' AND Persona = N'ortak' AND StepNo = 0;
    PRINT N'Danışman ilk e-posta şablonu: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' satır güncellendi.';
END
GO
PRINT N'Lead Merkezi imza ayarı boşaltıldı.';
GO
