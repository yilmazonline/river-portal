/* 36 - River AI surum notlari (v1.0.1 - v1.1.44) ice aktarimi. Idempotent: ayni urun + surum varsa atlanir.
   Kaynak: Guncel_Surum_Notlari.rar (53 dosya). Tekrar eden, ic/gelistirici ve kurulum talimati notlari ayiklandi.
   Kayitlar YAYINDA olarak eklenir; dogrudan veritabanina yazildigi icin bildirim / e-posta GONDERILMEZ. */
USE PARTNER;
GO
SET NOCOUNT ON;
DECLARE @n INT = 0;
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.1')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.1', '2026-06-18', '2026-06-18', N'## Yenilikler
Yeni Sürüm Notları alanı eklendi. Her yeni sürümde buradan haberdar olun.
Proje içe aktarma sonrası form ve süreç listesi otomatik yenilenmesi eklendi.
Proje Yönetimi ekranına üye listesi eklendi.
Proje Yönetimi ekranında birden fazla üye aynı anda eklenebilmesi projeye dahil edildi.
Proje sahibi (Owner) rolü değiştirilememesi ve listeden çıkarılamaması eklendi.
## Hata Düzeltmeleri
Proje içe aktarmada süreç adımlarındaki form isimleri GUID olarak görünmesi düzeltildi.
"Bekleyen İşlerim" sayacı yanlış gösterilmesi düzeltildi.
Kataloglar sayfasının sekme başlığında proje adının yanlış görünmesi düzeltildi.
Dashboardlar sayfasının sekme başlığında proje adının yanlış görünmesi düzeltildi.', N'Ara Sürüm', 1, '2026-06-18 09:00:00', '2026-06-18 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.2')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.2', '2026-06-19', '2026-06-19', N'## Yenilikler
Bir süreç adım geçtiğinde ya da tamamlandığında pano sayfayı yenilemeden otomatik güncelleniyor, kart doğru kolona anlık olarak geçiyor
Süreç ilerleyince kart otomatik olarak ilgili kolona taşınıyor; süreç tamamlandığında "Tamamlandı" kolonuna geçiyor, böyle bir kolon yoksa kart arşivleniyor
Sürece bağlı bir panoda kartı elle başka kolona sürüklemeye çalışınca "Kartlar süreç ilerledikçe taşınır, elle kolon değiştirilemez" uyarısı gösteriliyor
Kart detayında kartın süreci hangi adımda olduğu ve toplam kaç adım kaldığı ilerleme göstergesiyle görünüyor
"Onaya Gönder" ve "Tamamlamaya Gönder" butonları artık yalnızca onay akışı aktif olan panolarda çıkıyor; kart zaten onay sürecindeyse bu butonlar yerine "⏳ Onay sürecinde" etiketi gösteriliyor
Kart detay ekranına "Arşivle" butonu eklendi, artık tek bir kartı detayından direkt arşivleyebilirsin
Atama alanlarında seçili kullanıcıların sistem kimliği (GUID) yerine gerçek adları gösteriliyor
Sürece bağlı kartlarda kart oluşturulurken ilgili kullanıcı veya rol otomatik olarak atanan olarak işaretleniyor
İş listesinde görevlerin yanında kalan süreyi gösteren dairesel bir zamanlayıcı eklendi; geciken görevlerde kırmızı uyarı çıkıyor
Tamamlanmış ya da devam eden bir görevi iş listesinden açtığında daha önce doldurulan form verileri otomatik olarak yükleniyor
Süreç geçmişi ekranında sürecin hangi daldan geçtiği ve hangi adımda bittiği görsel olarak vurgulanıyor
## Güvenlik
Panolar listesinde "Yeni Pano" butonu eklendi ve yönetici yetkisi olan kullanıcılara panoyu silme seçeneği getirildi', N'Ara Sürüm', 1, '2026-06-19 09:00:00', '2026-06-19 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.3')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.3', '2026-06-23', '2026-06-23', N'## Yenilikler
Belgelerinizi tarayıp yükleyebileceğiniz Doküman Yönetimi (DMS) modülü eklendi
Taranan belgelerde yapay zeka belge türünü tanıyıp tarih, tür ve ilgili alanları otomatik dolduruyor
Word, Excel ve PDF dosyaları tarayıcıdan açılıp düzenlenebiliyor
Belgeler e-imza ile imzalanabiliyor (mobil imza desteği dahil)
Belgelere renkli etiket ve açıklama eklenebiliyor; arşivlenip arşivden geri alınabiliyor
Dosya adları seçilen şablona göre (belge türü / klasör / tarih) otomatik oluşuyor
Belge tarama ekranına kırpma-netleştirme, aynı belgeyi tekrar yükleme uyarısı ve yükleme öncesi yapay zeka analizi eklendi
Yüklenen belgeler havale, onay akışı ve dağıtıma gönderilebiliyor
Yardımlaşma Sandığı modülü eklendi: Üye kayıtları, borç/avans talepleri, aidat takibi ve nema dağıtımı
Sandıkta toplu aidat tahakkuku, ödeme planı ve ayrılma işlemleri yapılabiliyor
Borç talepleri için ıslak imza formu PDF çıktısı alınabiliyor
Sandık raporları ve CSV dışa aktarımı eklendi; üyeler yalnızca kendi işlemlerini görüyor
Form ve süreç oluştururken adım adım soru sorarak yol gösteren yapay zeka sihirbazı eklendi
Farklı kurumlar arasında güvenli belge ve mesaj iletimi eklendi; gönderilen ve gelen iletiler yönetim ekranından takip ediliyor
Kullanıcılar dış sistemlerden otomatik içe aktarılıp düzenli aralıklarla senkronize edilebiliyor
Evrak havale/dağıtım akışı (EBYS) ve gelen e-fatura kutusu eklendi
Gelen talepleri sınıflandırıp ilgili kişiye yönlendiren Talep/Olay Yönetimi modülü eklendi
## Hata Düzeltmeleri
Süreçten panoya aktarılan kartlarda Türkçe karakterlerin (İ, ş, ğ gibi) bozuk görünmesi düzeltildi', N'Ara Sürüm', 1, '2026-06-23 09:00:00', '2026-06-23 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.4')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.4', '2026-06-24', '2026-06-24', N'## Yenilikler
Süreç tamamlandığında kart panonun doğru sütununa gelmiyordu; sütun tespit algoritması yeniden yazıldı
Formlara "Kaydet" butonu desteği eklendi: Bu butona basıldığında süreç ilerlemez, form taslak olarak kaydedilir
Kaydedilen taslak "Bekleyen İşlerim" listesinde görünür; kullanıcı formu bölüm bölüm doldurabilir
Taslak formu tekrar açtığınızda doldurduğunuz veriler sıfırlanmadan gelir
Hazır olduğunuzda "Gönder" butonuyla süreci bir sonraki aşamaya iletebilirsiniz
Pano kartı açıklamasına tablo alanı (datagrid) eşlendiğinde okunaksız metin yerine düzgün tablo görünüyor
## Hata Düzeltmeleri
Bir süreci ilk kez panoya bağlarken çıkan "403 Erişim Engellendi" hatası düzeltildi
Tablodaki kolon sırası artık formda tanımladığınız sırayı koruyor (veritabanı anahtar sırası sorunu çözüldü)', N'Ara Sürüm', 1, '2026-06-24 09:00:00', '2026-06-24 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.5')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.5', '2026-06-25', '2026-06-25', N'## Yenilikler
Görev panosuna özel bildirim paneli eklendi: Atama, onaya sunma, onay, ret ve yönlendirme bildirimleri tek yerde toplanıyor; panel açılıp kapatılabiliyor ve kapatıp tekrar açınca geçmiş bildirimler yine listede kalıyor
Görev panosunda önemli/kritik bildirimler, panoya girişte ekranın ortasında açılır pencere olarak gösteriliyor
Görev panosunda bir bildirime tıklayınca ilgili kartın detayı doğrudan açılıyor
Görev panosuna bildirim ayarları eklendi: Hangi tür bildirimlerin ekranda öne çıkacağı seçilebiliyor (örneğin "reddedilenleri göster")
Görev panosunda bildirim ayarlarını yalnızca pano yöneticisi düzenleyebiliyor ve ayar tüm pano için geçerli oluyor
Görev panosunda, bitiş tarihi yaklaşan görevler için hatırlatma bildirimi ve hatırlatma e-postası seçenekleri eklendi
Görev panosundaki "Detay Grafiği" görünümüne yeni grafikler eklendi; bir grafik dilimine tıklayınca o dilime ait görevlerin listesi açılır pencerede gösteriliyor ve listeden kartın detayına geçilebiliyor
Görev panosunda kart üzerindeki dosya ekleri için önizleme eklendi: Ek simgesine tıklayınca karta girmeden görsel/PDF önizlemesi açılıyor, birden çok ek arasında gezinilebiliyor ve indirilebiliyor
Görev panosunda kartlara pano içi iş numarası verildi (örneğin #12); kartın üzerinde, kart başlığında ve ilişkili kart listelerinde görünüyor
Görev panosuna "Detaylı Kart" ekleme ekranı eklendi: Tek formda başlık, kolon, son tarih, atanan, onaylayan, öncelik ve sprint girilebiliyor; mevcut hızlı kart ekleme de korundu
Görev panosunda yönlendirme (devretme) onayında, talebi yapanın yazdığı açıklama onaylayan kişiye gösteriliyor
Sandık modülü için ayrı (izole) kiracı yapısı eklendi: Kendi kullanıcıları ve akış ayarlarıyla
Sandık üyelik başvurusunda e-posta ile tek kullanımlık kod (OTP) doğrulaması eklendi (6 hane, 60 saniye)
Sandık için Office365 e-posta entegrasyonu Entegrasyonlar ekranından yapılabiliyor
Sandık başvuru formu, kişinin mevcut bilgilerinden otomatik dolduruluyor
Sandık çıkış (ayrılma) süreci İK onayına bağlandı; onay ekranında birikim ve borç bilgisini içeren çıkış formu PDF olarak görüntüleniyor
Sandık yönetim ekranına test/demo verisini sıfırlama butonu eklendi (tanımları koruyup yalnızca işlemsel veriyi temizliyor)
Doküman Yönetimi aramasına "AI ile ara" eklendi: Belgelerde anlam/kavram bazlı arama yapılabiliyor (isteğe bağlı)
Doküman Yönetimi aramasında sonuçlar yapay zekâ ile yeniden sıralanarak en alakalı belgeler öne çıkarılıyor; alakasız eşleşmeler ve gereksiz kelime gürültüsü azaltıldı
Doküman Yönetiminde "İndeksle" butonu her zaman erişilebilir; daha önce indekslenmemiş belgeler arama için hazırlanabiliyor
Yönetim demo verilerine organizasyon şeması (departmanlar ve çalışanlar) eklendi
Süreç tasarımcısında bağlantı (ok) etiketlerine daha okunaklı bir görünüm verildi
## İyileştirmeler
Görev panosunda kart ilişkilendirme geliştirildi: Bağlı kartlar artık numara, başlık, durum ve öncelik bilgisiyle listeleniyor ve başlığa tıklayınca ilgili kart açılıyor
Doküman Yönetimi ekranında logo, başlık ve arama kutusu düzeni iyileştirildi
Görselden süreç üretme özelliğinde otomatik yerleşim iyileştirildi; adımlar daha ferah ve birbirine girmeden diziliyor
Gösterge panosunda iş yükü dönem hesabı iyileştirildi
Fotoğraf düzenleme aracında görüntü ayarları iyileştirildi
## Hata Düzeltmeleri
Görev panosunda ek dosyaların küçük resimlerinin görünmemesi ve indirilememesi sorunu giderildi
Yapay zekâ yardımcıları daha çok ekranda devreye giriyor; geçici hatalarda otomatik yeniden deneme ve genel kararlılık iyileştirmeleri yapıldı
## Güvenlik
KAAF görev panosuna Excel dosyasından kullanıcı içe aktarımı ve yetkilere göre örnek görevler/atamalar eklendi
Yönetim bölümüne yeni "Güvenlik" sayfası eklendi: İki adımlı doğrulama (2FA), IP kısıtlama, şifre politikası ve oturum güvenliği ayarları', N'Ara Sürüm', 1, '2026-06-25 09:00:00', '2026-06-25 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.6')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.6', '2026-06-26', '2026-06-26', N'## Yenilikler
Görev panosunda kartlarla ilgili önemli olaylar artık e-posta ile bildiriliyor: Bir görev sana atandığında, onayına sunulduğunda, onaylandığında veya reddedildiğinde ilgili kişiye otomatik e-posta gidiyor
Bu e-postalar her olay türü için ayrı ve görsel olarak tasarlandı: Renkli başlık bandı, görev detayları (pano, durum, öncelik, bitiş tarihi) ve doğrudan göreve gitmek için buton içeriyor
E-postadan tek tıkla onay/ret eklendi: Onayına sunulan bir görevi, e-postadaki "Onayla" / "Reddet" butonlarıyla uygulamaya girmeden sonuçlandırabiliyorsun
E-postadan reddederken açıklama (gerekçe) zorunlu; yazılan gerekçe işi yapan kişiye iletiliyor
Görev panosuna "Onay & ret e-postaları" ayarı eklendi: Onaya sunma / onay / ret e-postalarının gönderilip gönderilmeyeceği pano bazında açılıp kapatılabiliyor (yalnızca pano yöneticisi düzenleyebiliyor)
Zaten giriş yapmışsan, e-postadaki bağlantı seni hiç giriş sormadan doğrudan kartın detayına götürüyor
Yeni bildirim tasarımı hem bildirim panelinde hem de panoya girişte açılan "Önemli bildirimler" penceresinde kullanılıyor; daha okunaklı ve profesyonel bir görünüm sağlandı
## İyileştirmeler
Görev panosu bildirimleri görsel olarak yenilendi: Her bildirim türü (atama, onay bekleyen, onaylandı, reddedildi, tamamlandı, gecikme) kendi simgesi, rengi ve etiketiyle gösteriliyor; bildirimin türü bir bakışta anlaşılıyor
Bildirim başlıklarındaki tekrar eden önekler ("Sana atandı:" gibi) sadeleştirildi; tür etiketi zaten görünür olduğu için başlıkta yalnızca görev adı kalıyor
## Hata Düzeltmeleri
Aynı görev için tek işlem güvencesi: Biri onayladıktan veya reddettikten sonra, aynı e-postayı almış başka biri ikinci kez işlem yapamıyor; "bu görev için zaten karar verilmiş" uyarısı gösteriliyor
E-postadaki onay/ret, butona kimin e-postasından tıklandıysa o kişi adına yapılıyor; grup (çoklu) atamalarda onaylayan ve işi yapan kişiler birbirine karışmıyor
Görev panosu bildirim ayarları panelinin alt kısmının kesilmesi giderildi; panel artık kaydırılabiliyor ve en alttaki "Onay & ret e-postaları" ayarı tam görünüyor
Panoya girişte açılan "Önemli bildirimler" penceresinin bazen hiç açılmaması sorunu giderildi (bildirimler yüklenmeden pencere "gösterildi" sayılıyordu)
E-postadan veya bildirimden bir karta gidildiğinde, ilgili pano sekmesi zaten açıkken kartın açılmaması sorunu giderildi
## Güvenlik
E-postadaki "Görevi aç / İncele" bağlantısına tıklayınca giriş ekranı senin e-postan önceden dolu olarak açılıyor; şifreni girince ilgili görev kartı doğrudan açılıyor
E-postadaki bu bağlantı tek başına hiçbir erişim vermiyor; gerçek giriş her zaman şifreyle yapılıyor, böylece bağlantı başkasının eline geçse bile şifre olmadan işe yaramıyor', N'Ara Sürüm', 1, '2026-06-26 09:00:00', '2026-06-26 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.7')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.7', '2026-06-27', '2026-06-27', N'## Yenilikler
Detaylı kart oluşturma ekranına "Atanan kullanıcılara e-posta da gönder" seçeneği eklendi (önceden yalnızca kart detayında vardı); kart oluşturulurken atananlara bildirim e-postası gönderilebiliyor ve özel bir atama mesajı yazılabiliyor
Kart atamaları artık tek tek kullanıcının yanı sıra rol ve grup bazlı da yapılabiliyor; bir role veya gruba atanan iş, o roldeki/gruptaki tüm kullanıcılara bildiriliyor
Onaylayan alanı çoklu seçilebiliyor ve rol/grup bazlı atanabiliyor; bir role atanan onayda o roldeki kullanıcılara onay e-postaları gidiyor
Proje Yönetimi > "Üye Ekle" alanı artık rol ve grup eklemeyi de destekliyor; bir rol veya grup projeye eklendiğinde o role/gruba sahip herkes projeye (seçilen rolle) erişiyor
Panolar ekranındaki her panoya ⚙ "Pano/Kolon Ayarları" butonu eklendi (yalnızca yöneticiler erişebiliyor)
"Kolon = Proje" modeli: Bir panonun her kolonu bir projeyi temsil edebiliyor, kartlar o projenin işleri olabiliyor
Her kolona proje kodu (örn. KP001) atanabiliyor; yeni kolon eklerken kod, Excel''deki gibi otomatik artarak öneriliyor (istenirse elle değiştirilebiliyor)
Kart numaralarına proje kodu öneki eklenebiliyor: Kartlar KP001-1, KP001-2 … şeklinde, kolona göre otomatik artan numara alıyor
Proje kodunu kolon başlığında gösterme seçeneği eklendi (varsayılan açık): Kolon başlığı "KP001 · GENEL" gibi görünüyor
Kolon başlığını o kolondaki kartların üzerinde gösterme seçeneği eklendi
Kolon (proje) durumu eklendi: Aktif / Pasif / Tamamlandı / İptal; aktif olmayan kolonlar panoda gizleniyor ve "Pasif projeleri göster" düğmesiyle açılabiliyor; kolon başlığında durum rozeti gösteriliyor
Kolona özel kart kuralları eklendi: Kart oluştururken "Tahmini Saat / Atanan / Son Tarih zorunlu"; kartı tamamlarken "Harcanan Süre / Kontrol Listesi / Açıklama zorunlu"; ayrıca proje kodunu yeni kartlara otomatik etiket olarak ekleme
"Kolon yapısını kilitle" ayarı eklendi: Açıkken panoda elle kolon ekleme, kolon adını değiştirme ve silme kapanıyor; kolonlar yalnız ayarlar ekranından yönetiliyor
Kart oluştururken kolon seçiminde yalnızca aktif kolonlar listeleniyor
"Atanan kullanıcıya otomatik erişim ver" ayarı eklendi: Bir kart, erişimi olmayan bir kullanıcıya atandığında otomatik olarak önce projeye (görüntüleyen), ardından ilgili panoya (katkıcı) erişim veriliyor; kullanıcının erişimi zaten varsa hiçbir değişiklik yapılmıyor
Kart arama genişletildi: Başlık ve açıklamanın yanı sıra kart kodu (örn. KP001-5), iş numarası ve atanan kişi (ad veya kimlik) ile de arama yapılabiliyor
Panolara "📊 Analitik" ekranı eklendi: Kimin hangi işe ne kadar zaman harcadığı ve proje (kolon) bazlı ilerleme görüntülenebiliyor
Harcanan süre girişlerine "kim" bilgisi eklendi: Süre artık girişi yapan kullanıcıya atfediliyor; "aynı karta birden çok kişi süre girebilsin" ayarıyla farklı kişiler ayrı ayrı süre girebiliyor
Analitik ekranı sade ve sekmeli tasarlandı: "Kişiler" sekmesinde kişi bazlı süre grafiği (bir kişiye tıklayınca hangi işlere ne kadar harcadığı açılıyor) ve "Projeler" sekmesinde aranabilir proje (kolon) tablosu
Tek bir "Odak proje" seçici ile tüm analitik belirli bir projeye daraltılabiliyor; çok sayıda kolon olduğunda karışmaması için tasarlandı
Panolar ekranına "📊 Tüm Panolar Analitiği" eklendi: Bir projedeki tüm panoların toplu (portföy) analitiği — pano bazlı özet ve genel kişi dağılımı; bir panoya tıklayınca o panonun detaylı analitiği açılıyor
Analitik ekranlarından Excel''e (CSV) dışa aktarma eklendi: "Görünen" (ekrandaki, filtreli veri) ve "Tümü" (filtresiz, tüm veri) seçenekleri
"Panoya Kopyala" seçeneği eklendi: Tablo doğrudan Excel veya Google Sheets''e yapıştırılabiliyor
Dışa aktarılan dosyalar Türkçe Excel ile tam uyumlu: Sütunlar doğru bölünüyor, Türkçe karakterler düzgün görünüyor ve saat gibi ondalıklı değerler tarih olarak değil sayı olarak açılıyor
"RiverAI Akademi" modülü eklendi: Uygulamayı kategorilere ayrılmış canlı turlarla tanıtıyor; ekranda ilgili alanları sırayla vurgulayıp (highlight) sesli anlatımla adım adım gezdiriyor
Formlara "AI Tur" özelliği eklendi: Form alanlarını sırayla vurgulayıp Türkçe sesli anlatımla tanıtıyor; anlatım metni, formun ve bağlı sürecin yapısına göre yapay zekâ tarafından üretiliyor
Anlatımda teknik alan adı yerine alanın görünen etiketi kullanılıyor; tur panelinde ses seçimi ile Adım / Duraklat / Durdur kontrolleri bulunuyor
Form doldurma ekranına sabit (üst köşe) "Düzenle" ve "AI Tur" butonları eklendi; AI Tur yalnızca ilgili süreçte "AI ile destekle" açıkken görünüyor
Süreçlere SAP entegrasyon adımları (RFC / HANA / OData / IDoc) eklendi: Tasarımcıda hazır adım kartları, bağlantı seçici, popüler RFC/IDoc/OData listeleri ve JSON/gövde alanları için ayrı düzenleme penceresi
Zamanlama (Timer) adımlarına "AI ile cron öner" butonu eklendi: Doğal dille tarif edilen tekrarı uygun cron ifadesine çeviriyor
Kod yazmadan görsel pivot/rapor kurucu eklendi: Pivot tablo hazırlanıp "rapor olarak kaydet" ile saklanabiliyor
Gösterge paneli filtreleri kolaylaştı: Bağlantı seçimi metin yerine açılır liste, parametreler için anahtar/değer satır editörü ve filtreleri grafiklerle eşleyen "parametre çubuğu" eklendi
Tablolara kolon renklendirme (her ay/grup farklı renk), çok serili ve yıldan-yıla (YoY) çubuk grafikler, panel/sekme/bölücü (splitter) yerleşimi ve bölmeleri yeniden sıralama eklendi
Zamanlanmış rapor önbelleği eklendi: Ağır raporlar arka planda hazırlanıp hızlı açılıyor
Profil ekranına "Ana ekran tercihleri" eklendi: Kanban panel görünürlüğü ve tema seçimi anında uygulanıyor
Proje dışa/içe aktarma (.rpp) genişletildi: Artık gösterge panelleri, senkronlar ve raporlar da pakete dahil ediliyor
Sandık modülüne Excel benzeri veri tablosu (sıralama + çoklu seçimli kolon filtresi) eklendi: Üyeler, Aidat, Raporlar, Ayrılma ve Nema ekranlarında; Üye Detayında katkı ve borç geçmişi alt tabloları
"Sandık Giriş Formu" ilk girişte zorunlu hâle getirildi; doldurulunca PDF üretilip "Belgelerim"e ekleniyor
Ayrılma başvurusunda iki resmi form PDF olarak üretilip "Belgelerim"e ekleniyor; ayrılma onayı sandık görevlisinde yapılıyor
"Taksit Öteleme" ekranı eklendi: Sandık görevlisi bir borcu gerekçeyle erteleyebiliyor
Nema (getiri) dağıtımı yıllıktan aylığa çevrildi ve katkı oranına göre hesaplanıyor; ay adları Türkçe gösteriliyor
Aidat ayarları (giriş/aylık/yıllık) ile Dernek için ayrı aidatlar eklendi
KVKK aydınlatma metni zengin metin olarak ayarlardan girilip başvuru ve giriş formlarında gösteriliyor
Şirket bazlı veri görünürlüğü eklendi: Personel yalnızca kendi şirketinin verisini görüyor
SAP FI muhasebe fişleri üretimi eklendi: Şablondan ve gerçek veriden, her sayfa ayrı Excel olarak
Çıkış (oturum kapatma) yapıldığında açık sekmeler de kapatılıyor; bir sonraki girişte ekran temiz başlıyor
## İyileştirmeler
Süreç tasarımcısına "AI ile destekle" seçeneği eklendi: Sürece özel destekleyici bir not (yönerge) yazılabiliyor, yapay zekâ önerileri bunu dikkate alıyor; süreç özellikleri paneli sadeleştirildi
Ana ekran yeniden düzenlendi: Solda bekleyen işler ve pano kısayolları (akordeon), sağda gömülü Kanban; bölmeler arası sürüklenebilir ayraç (splitter) ile genişlik ayarlanabiliyor
Bağlantılar ekranında satır işlem butonları sadeleştirildi (yalnız ikon, açıklama balonda)
Borç akışı tek onaya indirildi (yalnız İK müdürü); borç uygunluk ve limit ayarları yenilendi (katkı-bazlı limit, ay/yıl gider payı, alt/üst sınır)
E-Fatura ekosistem menüsü ikonu güncellendi
## Hata Düzeltmeleri
Kart etiketleri artık okunaklı: Üzerinde etiket adı yazan renkli kapsüller olarak gösteriliyor (önceden yalnız renkli bir çubuktu, adı görünmüyordu); arka plan rengine göre okunur yazı rengi otomatik seçiliyor
Pasif / tamamlanmış / iptal edilmiş bir kolona kart ekleme yalnızca yöneticilere açık (işleri önceden hazırlamak için); diğer kullanıcılar bu alanı görmüyor
Kart sıralaması düzeltildi: Bir kartı kolon içinde istediğin konuma sürükleyebiliyorsun (örneğin en alttaki kartı en üste taşımak); önceden bırakılan kart her zaman en sona düşüyordu
Filtre uygulandığında "Tamamlanan görevler" bölümü otomatik açılıyor; böylece filtreyle eşleşen tamamlanmış kartlar da görünür kalıyor, kolon boş sanılmıyor
SQL/rapor çalıştırma iyileştirildi: MSSQL bağlantıları doğru sürücüyle çalışıyor, opsiyonel filtreler boş bırakılınca sorgu hata vermiyor ve rapor sonucunda kolon sırası korunuyor
Genel arayüz cilası: Tema ile uyumlu ince kaydırma çubukları ve çeşitli görsel düzeltmeler
## Güvenlik
Gösterge panellerine "Yetki" (erişim matrisi) eklendi: Panel erişimi paylaşım yerine kullanıcı/rol/grup bazlı yetkilerle yönetiliyor
Üyeler için "Profilim" (şifre değiştirme, katkı payı, belgeler) ve "Parametrelerim" (versiyonlanan aylık katılım payı) ekranları eklendi
E-posta gönderiminde (Microsoft 365) hata mesajları iyileştirildi: Yetki ve ağ kaynaklı sorunlar daha anlaşılır gösteriliyor
## Kaldırılanlar
Analitik grafikleri lisanssız bir grafik altyapısına taşındı; analitik ekranında görünen deneme sürümü uyarı bandı kaldırıldı', N'Ara Sürüm', 1, '2026-06-27 09:00:00', '2026-06-27 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.0.8')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.0.8', '2026-07-02', '2026-07-02', N'## Yenilikler
Ana ekran Kanban artık kişisel: Yalnızca sana atanan, bir rol veya grup üzerinden atanan ya da onayını beklediğin kartları gösteriyor; işin olmadığı pano ve kolonlar gizleniyor, pano sekmelerinde ilgili kart sayısı görünüyor
Kart detayında sekmeler daha okunaklı: İçeriği olan sekmeler (Alt Görevler, Yorumlar, Süreler, Ekler …) renkli rozet ve ikonla belirginleştirildi
Kart detayına "Süreler" sekmesi eklendi: Harcanan süreyi artık kartın içinden de girebiliyorsun (önceden yalnızca kart menüsünden)
Yorumlar sekmesi sohbet düzenine geçti: Yorumlar üstte kronolojik ve kaydırılabilir, yazma alanı altta; renkli baş-harf avatarları ve okunaklı balonlarla daha kullanıcı dostu
Kart önceliği sayı yerine etiketli seçime dönüştü: Çok düşük / Düşük / Normal / Yüksek / Acil
Kart detay ekranı genişletildi (daha ferah çalışma alanı)
Bir kolonda kart oluşturma kuralı varsa (tahmini saat / atanan / son tarih zorunlu), kolon altından hızlı kart eklerken otomatik olarak detaylı ekleme ekranına yönlendiriliyor; zorunlu alanlar (*) ile işaretleniyor ve eksikse net uyarı veriliyor
Yeni eklenen kartın başlangıç tarihi varsayılan olarak bugünün tarihi geliyor (istenirse sonradan değiştirilebiliyor)
Kanban''da kolonların üstüne, ana kaydırma ile eşzamanlı bir yatay kaydırma çubuğu eklendi: Çok kolonlu panoda en alta inmeden yatay kaydırma yapılabiliyor
Pano ayarları ekranına "Pano Bilgileri" ve "Kapak Sayfası" sekmeleri eklendi: Pano oluşturulduktan sonra da adı, açıklaması, teması ve kapak alanları düzenlenebiliyor (önceden yalnız oluşturma sırasında ayarlanabiliyordu)
Kullanıcı yönetiminde bir kullanıcıya birden çok rol tek seferde seçilip eklenebiliyor
Kullanıcı düzenleme eklendi: Kullanıcı adı, e-posta, ad ve soyad değiştirilebiliyor
Katalog kayıt düzenleme ekranında formun kendi butonları (Kaydet / Onaya Gönder) gizlendi; kataloğun kendi Kaydet / İptal düğmeleri kullanılıyor
"Seed Datayı Oluştur" ekranına KAAF örnek proje panoları (Kolon = Proje) için tek tıkla seed butonu eklendi
Sekmelerin kapatma butonu modernleştirildi: Apple tarzı, üzerine gelince kırmızı yuvarlak × (net SVG ikon)
E-imza: TÜRKKEP mobil imza sağlayıcısı eklendi; KEP entegrasyonu açıksa mobil imza ayarları otomatik görünüyor, imza türüne (e-imza / mobil) göre imzalama ekranı uyarlanıyor; belge içeriği sağlayıcıya güvenli biçimde iletiliyor
DMS (Doküman Yönetimi) onay akışı güçlendirildi: Çok onaycılı aşamalar (paralel + kurallı), üstveriye göre koşullu aşama atlama, serbest (ad-hoc) onay zinciri ve "İşlem Bekleyenler" kokpiti
Veri tablosu (DataGrid) hücrelerinde tarayıcı-içi düzenleme ve kolon olayları / no-code aksiyonlar eklendi; kayıtlı SQL snippet''leriyle çalışabiliyor
Docker-compose servislerinin otomatik keşfi + konteyner yönetimi için arayüz eklendi
Çeşitli modüller (BudgetAi, Sandık, Intranet, EBYS / KEP, entegrasyonlar) ortak hatta senkronlandı
## İyileştirmeler
Performans modülü eklendi: İşlem / rapor performans kaydı, ısı haritası, en çok kullanılanlar ve "what-if" senaryo simülatörü (kaydırıcı + sürükle-bırak ile projeksiyon)
## Hata Düzeltmeleri
Harcanan süre, tahmini saatle ilişkilendirildi: Tahmini süre girildiyse toplam harcamayla karşılaştırılıp geri bildirim veriliyor — tahmini aştıysa uyarı (kırmızı), tam uyumluysa yeşil, altındaysa "tahminin önünde" (mavi) + ilerleme çubuğu
Görev tamamlanırken zorunlu bir alan boşsa (harcanan süre / kontrol listesi / açıklama), sadece hata mesajı yerine ilgili sekmeye otomatik geçiliyor ve eksik alan belirtiliyor
Atama ve onay kullanıcı seçicilerinde Türkçe isimle arama düzeltildi: "İsmet" yazınca "İsmet Can Tuncay" gibi kayıtlar artık listeleniyor (Türkçe büyük/küçük harf uyumsuzluğu giderildi)
Form tasarımcısına "onInstanceCreate" olayı eklendi: Form kaydı (instance) ilk oluşturulduğunda bir kez çalışan script; sürecin sonraki ilerletmelerinde tekrar çalışmıyor
Genel arayüz cilası ve çeşitli hata düzeltmeleri', N'Ara Sürüm', 1, '2026-07-02 09:00:00', '2026-07-02 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.0')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.0', '2026-07-02', '2026-07-02', N'## Yenilikler
Yeni Özellikler: Katalog Excel İçe Aktarma — Ürün ve katalog verilerinizi Excel dosyasından master-detail yapıda (ana kayıt + alt satırlar birlikte) toplu olarak sisteme aktarabilirsiniz. Katalog Yönetimi ekranından "Excel''den İçe Aktar" butonuyla kullanılır.
Yeni Özellikler: Tablo Hücresi Veritabanı Arama Editörü (BrowserEdit) — Formlardaki tablolarda bir hücreyi düzenlerken, o alan için tanımlanmış veritabanı sorgusuna göre açılan bir arama/seçim kutusu çıkar. Kayıt seçince ilgili alanlar otomatik dolar.
Yeni Özellikler: Tablo Master-Detail Otomatik Doldurma — Grid (tablo) kolonundan tetiklenen olaylarla, seçilen satırdaki birden fazla alanı (row.*) bağlı formlara toplu olarak aktarabilirsiniz. Ayrı detay tablosu ile birlikte çalışır.
Yeni Özellikler: No-Code Kolon Aksiyonları — Form tasarımında tablo kolonlarına — kod yazmadan — olay bazlı aksiyonlar tanımlayabilirsiniz: Hücre değişince başka alana yaz, API çağır, vb. "Sütun Özellikleri → Olaylar → Aksiyonlar" sekmesinden yönetilir.
Yeni Özellikler: Süreç Başlangıç Hook Noktası — Bir süreç (workflow) başlatıldığında devreye giren özelleştirilebilir hook noktası eklendi. Entegrasyon ve özel iş kuralları buraya bağlanabilir.
Yeni Özellikler: DataGrid Kolon Olayları ve Satır Bazlı Atama — Tablolarda kolon değeri değişince aynı satırdaki başka alanlara (row.alan_adı) otomatik değer atayabilirsiniz.
E-İmza — TÜRKKEP Mobil İmza Desteği — İmzalama modalında TÜRKKEP mobil imza sağlayıcısı seçeneği eklendi. Operatör bilgileri ve bilgilendirme metni override edilebilir. KEP açıksa config otomatik görünür; kimlik bilgileri Entegrasyonlar ekranındaki KEP ayarından okunur.
BrowserEdit Görüntüleme/Değer Alanı Seçimi — Arama kutularında hangi alanın değer (valueField), hangisinin ekranda görünür (displayField) olarak kullanılacağını isteğe bağlı geçersiz kılabilirsiniz.
## İyileştirmeler
Yeni Özellikler: Performans Modülü — Sistem yöneticileri için kaynak kullanımı (CPU, bellek, sorgu süreleri) izleme ekranı. Isı haritası görünümü, hangi süreç/raporun en çok yük oluşturduğunu gösterir. "What-if" simülatörüyle parametre değişikliklerinin etkisini önceden görebilirsiniz. Yönetici menüsü → Performans.
Kişisel Ana Ekran — Kanban Görünümü — Ana ekranda kişisel görevlerinizi Kanban kartları olarak takip edebilirsiniz. Kart detayı, kullanıcı düzenleme ve form onInstanceCreate akışları iyileştirildi.
BudgetAI, Sandık, İntranet, EBYS/KEP Genel İyileştirmeler Bütçe modülünde sürücü yönetimi, yeni kalem modalları ve özet raporlar geliştirildi. Sandık, İntranet ve EBYS modüllerinde arayüz ve işlev iyileştirmeleri yapıldı.
DMS Belge Şablonları — Belge yönetiminde şablon üretme sihirbazı iyileştirildi.
Performans "Uygula" butonu, önizleme boş döndüğünde kaydetmeden devam ediyordu; önce kaydet zorunlu hale getirildi.
## Hata Düzeltmeleri
E-imza belge içeriği sağlayıcıya doğrudan iletiliyordu, 401 hatasına yol açan sorun giderildi.
Grid BrowserEdit, kayıtlı SQL Snippet kullanmak yerine inline SQL çalıştırıyordu; düzeltildi.
Çeşitli TypeScript derleme hataları giderildi.', N'Ara Sürüm', 1, '2026-07-02 09:01:00', '2026-07-02 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.1')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.1', '2026-07-03', '2026-07-03', N'## Yenilikler
Akıllı SQL sorgularına artık form alanlarının ve tablo (grid) satır değerlerinin değeri parametre olarak verilebiliyor. Örneğin bir alanın "Değişim (onChange)" olayında yazılan SQL içinde {{alan_adi}} veya {{row.kolon}} kullanıldığında, o anki değer sorguya otomatik parametre olarak geçiyor. Bu artık tüm bileşen tiplerinde (metin, açılır liste, lookup, tarih vb.) çalışıyor.
Sorgu sonucunun nereye yazılacağı artık elle metin yazmak yerine seçimle belirleniyor: Aynı satırdaki başka bir kolon, tüm satır (toplu doldurma), ya da başka bir alan / başka bir tablo seçilebiliyor; ayrıca sonucun alanın hangi özelliğine yazılacağı (değer / açılır liste / tablo satırları) seçilebiliyor.
Kaskad (bağımlı) açılır listeler: Bir seçime göre başka bir alanın veya hücrenin listesi otomatik doldurulabiliyor. Örneğin bir satırda ilk kolondan yapılan seçime göre ikinci kolonun açılır listesi ilgili kayıtlarla dolduruluyor.
Tablo (grid) kolonlarında SQL kaynaklı seçenek listeleri artık hücrelerin içinde de görünüyor. Önceden liste yalnızca tasarım ekranındaki önizlemede geliyordu; artık son kullanıcı hücresinde de açılıyor.
Formlarda açılır liste / seçenek alanları için "varsayılan sıra" (selectedIndex) ile başlangıç seçimi belirleme; tarih ve saat alanları için belirli gün/saatleri kapatan özel takvim/saat seçici (kara liste) eklendi.
Satın alma (PO) tedarikçi akışı: Personel (Sicil No) seçimi ile ilişkili organizasyon detay tablosu ve pozisyona göre sipariş notu üretimi.
## İyileştirmeler
Yeni ayar: "İlk seçeneği otomatik seç." Açık olduğunda liste dolduğunda (veya bağımlı liste yenilendiğinde) ilk seçenek otomatik seçili gelir; kullanıcının geçerli seçimi korunur. Otomatik seçim yapıldığında ilgili "Değişim" olayı da tetiklenir; böylece zincirleme (kaskad) doldurma kesintisiz ilerler.
Performans modülü: Sekmeli yeni yapı ve yapay zekâ destekli planlayıcı (öneriler + sürükle-bırak). Seçilen performans profili artık uygulamanın çalışma zamanını gerçekten etkiliyor.
Büyük veri testleri için demo ortamına performans senaryosu verisi (yaklaşık 1,1 milyon iş) eklendi.
## Hata Düzeltmeleri
Kararlılık: Bir alana ya da hücreye yanlışlıkla tekil değer yerine bir kayıt/liste (nesne/dizi) yazıldığında formun tamamen açılamaz hale gelmesi (beyaz ekran) engellendi; hatalı eşlemeler artık formu çökertmiyor, kullanıcıya sezgisel şekilde davranıyor.
İş Listesi (Worklist) hız iyileştirmeleri: KPI / kapsam değiştirilirken bekleme göstergesi (kum saati ve örtü animasyonu), Keycloak kaynaklı bir gecikme hatasının giderilmesi ve gereksiz sayım (COUNT) adımlarının atlanmasıyla belirgin hızlanma; görünen ad artık profil bilgisinden öncelikli getiriliyor.
Doküman Yönetimi (DMS) demo verisi düzeltmesi: Örnek kayıtlar için gerçek dosyalar üretilip yükleniyor (boş önizleme ve "Düzenle" hatası giderildi).', N'Ara Sürüm', 1, '2026-07-03 09:00:00', '2026-07-03 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.2')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.2', '2026-07-06', '2026-07-06', N'## Yenilikler
Yeni "Üretim" modülü eklendi: Modül karşılama/kokpit ekranı, koyu tema uyumlu okunaklı tablolar ve demo veri kurulumu.
Malzeme İhtiyaç Planlama (MRP) motoru: Talep + planlama kokpiti; net-change (fark bazlı) yeniden planlama.
Yapay zekâ destekli planlama: Doğal dil ile soru sorulabilen "konuşan kokpit", MRP istisnaları için AI çözüm önerileri (Claude), ve onay kuyruğu + guardrail (koruma kuralları) ile denetimli-otonom AI kararları; tek tıkla net-change MRP çalıştırma.
BOM / Reçete: React-flow tabanlı görsel editör ve görüntüleyici (kesikli üretim + proses üretim, yan ürün ve çıktı destekli); satır ekle/düzenle/sil.
Üretim yürütme: İş Emri + backflush (geri düşüm), stok ekranı ve stok-yeterlilik kontrolü, iş emri bileşenleri için stok rezervasyonu.
İzlenebilirlik ve maliyet: Lot / seri takibi + soy ağacı; iş emri fiili maliyet ile standart maliyet karşılaştırması ve sapma analizi.
Kalite Kontrol: Muayene planları, kalite-hold (karantina), disposition (karar) akışı, KPI''lar ve AI destekli değerlendirme.
Tanım ekranları: Rota, Vardiya/Takvim, Depolar, İş Merkezleri; CRP (iş merkezi yük/kapasite görünümü).
Raporlar: Özet göstergeler, stok değerleme ve AI yönetici özeti; AI kararlarının panelde (drawer) sunulması, rapor e-postası gönderme ve dosyaya kaydetme.
SSH kendi ekosistem ürünü olarak eklendi: Bağımsız /ssh arayüzü, kendi kokpiti ve 9 ekranlık tam iskelet; ana ekosistem kartından erişim.
Servis Talepleri: Zengin ve etkileşimli ekran, canlı SLA takibi ve gösterişli talep formu.
Atama (Dispatch): Sürükle-bırak atama kokpiti, teknisyen doluluk/durum göstergesi ve alarmlar.
Saha Servisi (mobil), SLA/KPI panosu ve Teknisyenler ekranı.
Sözleşmeler, Yedek Parça ve Ayarlar ekranları.
SQL Veri Konsolu yenilendi: İkon-tabanlı araç çubuğu, tam ekran modu, otomatik tamamlama, sürüklenebilir bölme (splitter), anahtar kelime renklendirme ve inline bağlantı seçimi.
Excel çıktısı: SQL sonucundan çalışma anında .xlsx dosyası üretme (dosya adı belirtilebilir).
DKYS modülü eklendi; Logo ERP eşleme ile katalog/organizasyon/entegrasyon arayüzü dalgası.
Logo ERP (Tiger) bağlantı yapılandırması + sipariş seed şemaları ve canlı "Test" butonu ile bağlantı doğrulama.
Teknik dokümanlar: OCR yerel kütüphaneleri (Tesseract/Leptonica) sudo''suz kurulum notu; Üretim ve SSH için test ekibi yönergeleri (uçtan uca senaryolar); sunucu kurulum/devir kılavuzu (systemd servisleri, NetBird, seed durumu, doğrulama betikleri).
## İyileştirmeler
Demo verisi: Organizasyon şeması verisi, form varsayılan/zorunlu alan örnekleri, katalog modalı ve performans ekranı koyu tema iyileştirmeleri.
## Hata Düzeltmeleri
Şema-farkında AI-SQL: Veritabanı şemasını tanıyıp sorgu üretir/düzeltir (self-repair), kullanılan motora göre tanımlayıcıları doğru tırnaklar, kolon-tablo eşleşmesini zorlar, liste isteğinde uygun SELECT üretir ve ne yaptığını açıklar.
SQL çalıştırmada veritabanı hataları artık 500 yerine anlaşılır bir 400 + mesaj olarak dönüyor.
Süreç Tasarımcısı (Process Designer) node zenginleştirmeleri: · SQL node içinde gömülü mini veri konsolu + "AI ile SQL düzelt" + Excel çıktı dosyası alanı. · scriptTask: JS editörü modalı, parametre eşleme ve AI (self-repair). · mongoTask: Mongo Veri Konsolu (modal editör + çalıştır + AI self-repair). · serviceTask: Mini-Postman "Yapılandır" modalı (gerçek test + çıktı eşleme) ve "✨ AI ile doldur" (REST bilmeyen kullanıcı için bileşeni otomatik doldurur). · Logo ERP node''u: Belge eşleme (buton + modal). · Bağlantı deneyimi: Node üstüne bırakınca otomatik bağlama, hover-bekleme ile otomatik bağlantı ve bağlı ucu sürükleyince mevcut bağlantıyı taşıma (reconnect) düzeltmesi.
## Güvenlik
Kod tabanında kapsamlı bir güvenlik denetimi gerçekleştirildi ve bulgular önceliklendirilmiş ayrı bir iç raporda toplandı (düzeltmeler planlanıyor).', N'Ara Sürüm', 1, '2026-07-06 09:00:00', '2026-07-06 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.3')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.3', '2026-07-07', '2026-07-07', N'## Yenilikler
Form bileşenlerinin olaylar (onChange) kısmındaki SQL sorgularına, formdaki diğer alanların değerleri {{alanAdi}} şeklinde parametre olarak verilebiliyor (ör. WHERE EMAIL = {{eposta}}). Bu; selectbox, lookup, metin, sayı, tarih... Fark etmeksizin tüm bileşenlerde aynı mantıkla çalışır.
Veri tablosu kolon aksiyonlarında, aynı satırın diğer kolonları {{row.kolonAdi}} ile yakalanabiliyor (ör. Select ID, fullname from etr_userlıst where emaıl = {{row.col1}}).
"{{" yazıldığında açılan otomatik-tamamlama listesi form alanlarını, satır kolonlarını (row.*) ve sistem değişkenlerini gruplu şekilde önerir; klavye ok tuşları / Tab / Enter ile hızlı seçim yapılabilir.
SQL sonucunun yazılacağı hedef artık serbest metin yerine açılır listeden seçiliyor: Aynı satırın bir kolonu, formdaki başka bir alan ya da başka bir bileşen seçilebilir.
Sadece değer değil; hedefin özelliği de seçilebiliyor — bir alanın "değeri", "seçenek listesi" (options) veya "veri kaynağı" (dataSource) hedeflenebilir. Böylece SQL sonucundan bir açılır listeyi doldurmak ("liste basmak") mümkün.
Bu davranış yalnızca lookup değil, liste tutabilen tüm bileşenlerde (selectbox, çoklu seçim vb.) geçerli.
Bir satırdaki seçime göre, aynı satırdaki başka bir kolonun açılır liste içeriği doldurulabiliyor (row.kolonAdi.options). Örneğin ilk kolonda seçilen değere göre ikinci kolonun seçenekleri anında değişir.
Yeni "İlk seçeneği otomatik seç" ayarı: Liste dolabilen bileşenlerde (selectbox, lookup, çoklu seçim vb.) liste yüklendiğinde ilk değer otomatik seçilir.
Otomatik seçim, alanın onChange olayını da tetikler; böylece kademeli listeler zincirleme olarak birbirini doldurabilir.
Veri tablosu içine çoklu açılır liste (multiselectbox / çoklu onay listesi / listbox) kolonu eklenebiliyor ve düzenlenebiliyor; daha önce bu kolonlar "salt-okunur" görünüp kullanılamıyordu, artık tam çalışıyor.
Çoklu seçimden gelen değerler SQL''e dizi olarak taşınıyor ve CASE WHEN / IN benzeri sorgularda parametre olarak kullanılabiliyor (ör. Birden çok firmayı tek kolondan seçip sorguyu ona göre kurmak). Bu davranış tüm bileşenlerde tutarlı.
{{__formJson}} — SQL/Mongo çalışırken formun TÜM verisini JSON olarak yazar (tıpkı {{selectbox}} bir alanın değerini yazdığı gibi). Örn: Formu tek kolona kaydetme ınsert ınto kayıtlar (verı) values ({{__formJson}}), ya da OPENJSON({{__formJson}}) ile alanları satır satır gezme.
{{__rowJson}} — veri tablosu aksiyonlarında tetikleyen satırın tüm verisini JSON olarak yazar.
Her ikisi de "{{" otomatik-tamamlamasında "Sistem" grubunda görünür; güvenli parametre olarak bağlanır.
Çok satırlı / akış içeren SQL blokları da sonuç döndürebiliyor: IF ... BEGIN ... Select, case when, wıth (CTE) ve saklı yordam çağrıları (EXEC) artık form ve tablo aksiyonlarında satır sonuçlarını geri getirir.
Saklı yordamlara birden çok parametre gönderilerek (ör. EXEC dbo.MyProc @p1 = {{...}}, @p2 = {{...}}) çağrı yapılabiliyor.
SQL / metin giriş alanlarında "büyüt" (⤢) düğmesi eklendi: Uzun sorgular tam ekrana yakın, dikey olarak yeniden boyutlandırılabilen bir alanda rahatça yazılabiliyor.
Otomatik-tamamlama menüsünde klavyeyle gezinme (ok tuşları, Tab, Enter, Esc) ve seçili öğeye otomatik kaydırma.
Kolon Düzenleyici''de "Aksiyonlar" (kod yazmadan) bölümü en üste alındı; JS kancaları (hooks) altına taşındı — sık kullanılan işleve daha hızlı erişim.
No-Code Form & Veri Tablosu: Aynı olayda (onChange) birden fazla SQL / veri kaynağı bloğu tanımlanabiliyor. Her blok kendi bağlantısını (farklı veritabanı olabilir), sorgusunu ve sonuç değişkenini (rs, rs2, …) taşır; bloklar sırayla çalışır ve sonraki blok bir öncekinin sonucunu kullanabilir. Sonuç değişkeni adları benzersiz olacak şekilde güvenceye alındı.
No-Code Form & Veri Tablosu: Satır-bazlı (per-row) parametreli lookup: "..." ile açılan veri-kaynağı seçici (browserEdit) açıldığı anda, o satırın hücreleri ve form alanlarıyla ({{row.kolon}} / {{alan}}) sorgu çalıştırıp sonucu listeliyor. Böylece bir hücrenin değerine göre dolan aç-getir listeler mümkün.
No-Code Form & Veri Tablosu: Form açılışında otomatik onarım: Geçmişte çoğaltmadan kalan çakışan kimlikler yükleme sırasında otomatik benzersizleştiriliyor — eski formlar açılıp kaydedilince kendiliğinden düzeliyor.
SOAP/XML web servis aracı (soapTask): Mini-Postman deneyimi, "AI ile doldur" ve XPath ile çıktı eşleme — REST aracıyla aynı olgunlukta; gönder butonu ve kilitli POST göstergesi.
E-Fatura Faz-2: UBL-TR faturaların yapısal olarak ayrıştırılması, tedarikçi ana verisi ekranı ve demo veri kurulumu.
DMS Dağıtım: Klasör-referanslı dağıtım + kişiye tebliğ (okundu-onayı) akışı.
## Hata Düzeltmeleri
Bir aksiyon SQL sonucundaki dizi/nesne değerini bir hücreye yazmaya çalıştığında oluşan "Objects are not valid as a React child" hatası giderildi; artık uygun olmayan değerler güvenle metne çevrilir/atlanır, form çökmez.
Veritabanına bağlanamayan tiplerde (dizi vb.) parametre bağlama düzeltildi: Diziler SQL Server tarafında JSON''a çevrilip OPENJSON ile, PostgreSQL tarafında ise doğal dizi olarak işlenir.
No-Code Form & Veri Tablosu: Kolon veri kaynağı (Seçenek editörü) SQL önizlemesi güçlendirildi: Sorguda {{...}} parametreleri varsa örnek değer girilerek test edilebiliyor; veritabanı hatası artık ham "500" yerine anlaşılır bir mesaj olarak gösteriliyor ve önizleme kaydetmeden de çalışıyor.
No-Code Form & Veri Tablosu: "Kopyala" (alan/bölüm çoğaltma) düzeltmesi: Bir veri tablosunu veya kolonlu bir bölümü çoğaltınca kopya artık orijinalle hiçbir iç bağ (referans) paylaşmıyor; tüm kimlikler yeniden üretiliyor. Bu sayede "bir kolonu düzenleyince başka kolonlar / başka tablolar etkileniyor" ve kopyalarda veri kaybı sorunları giderildi.
No-Code Form & Veri Tablosu: Sütun editörü: Farklı bir veri tablosunun "Sütunlar → Düzenle" ekranı açıldığında önceki tablonun sütunlarının görünmesi sorunu giderildi (her tablonun editörü kendi sütunlarıyla açılıyor).
No-Code Form & Veri Tablosu: Veritabanı hataları artık form/tablo veri-kaynağı ve tarama (browse) uçlarında da anlaşılır 400 mesajı olarak dönüyor (ham 500 yerine).', N'Ara Sürüm', 1, '2026-07-07 09:00:00', '2026-07-07 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.4')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.4', '2026-07-08', '2026-07-08', N'## Yenilikler
SOAP''a özel opsiyonel varsayılanlar: SOAP sürümü (1.1 / 1.2), varsayılan SOAPAction, hedef namespace, WSDL adresi ve varsayılan SOAP zarfı (envelope) şablonu.
Bu ayarlar bağlantıda saklanır; süreçteki SOAP Servisi düğümü bu bağlantıyı seçtiğinde sürüm / SOAPAction / zarfı buradan otomatik ön-doldurur.
Süreç tasarımcısındaki SOAP Servisi (mini-Postman) düğümü artık SOAP bağlantılarını da listeliyor; bir bağlantı seçilince boş alanlar bağlantının varsayılanlarıyla dolar.
"AI ile doldur" yardımcısı da SOAP bağlantılarını görebiliyor.
Senkron ekranına "Kaynak tipi: REST (JSON) / SOAP (XML)" seçimi eklendi.
SOAP kaynağında: Bağlantı, SOAP sürümü, endpoint, SOAPAction, SOAP Envelope (değişken/token destekli) ve Satır XPath girilir. Envelope POST edilir, XML yanıtı ayrıştırılır ve Satır XPath ile seçilen tekrarlayan düğümler satırlara dönüştürülür (boş bırakılırsa tekrarlayan düğümler otomatik tespit edilir; alt elemanlar otomatik olarak satır alanı olur — namespace bağımsız).
Benzersizlik anahtarı, tarih alanı, dilim ve cron ayarları REST ile aynı şekilde çalışır; dashboard bu satırları kind:"sync" kaynağından milisaniyede okur.
Bağlantı değişkenleri (ör. ParaBirimi) SOAP zarfında @paraBirimi olarak kullanılır ve runtime''da bağlantıdan çözülür — tek yerden değiştir, her yer takip etsin.
Yeni çalışma-anı token''ları: @Now (çalıştığı tam tarih-saat), @RunDate (tarih), @RunTime (saat). Mevcut senkron token''ları (@StartDate / @EndDate ve noktalı @..._dotted varyantları) SOAP zarfında da geçerli. Bu token''lar servisten gelmez — RiverAI runtime''da otomatik doldurur.
Denemeye hazır bağlantı örnekleri eklendi: SOAP Örnek — Sayıyı Yazıya Çevir, SOAP Örnek — Para Birimini Kullanan Ülkeler (değişkenli) ve REST Örnek — JSONPlaceholder. Hepsi ücretsiz/kimliksiz gerçek servislerdir, canlı denenebilir.
Senkron ekranında HTTP/REST bağlantısı tanımlı değilse açıklayıcı bir uyarı gösteriliyor (senkron yalnızca REST/SOAP kaynak destekler; SQL/Mongo bağlantıları listelenmez).
## Hata Düzeltmeleri
SOAP çağrılarında SOAP Fault ve DB/HTTP hataları artık ham 500 yerine anlaşılır bir mesaj olarak dönüyor (hem tarama/browse hem senkron yolunda).
Metin alanı otomatik-numara (auto-number) düzeltmesi: Sayaç artık gerçekten artıyor.
## Güvenlik
"Bağlantı tipi" listesine SOAP / XML servis eklendi. Endpoint URL + kimlik (auth) + varsayılan başlıklar + bağlantı değişkenleri, HTTP bağlantısıyla aynı alanlardan girilir.
Bağlantı tipi değiştirildiğinde tipe özel alanlar (ör. Veritabanı bağlantı dizesi) yeni tipin alanına taşınmıyor — düz-metin parolanın yanlış alana sızması önlendi.', N'Ara Sürüm', 1, '2026-07-08 09:00:00', '2026-07-08 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.5')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.5', '2026-07-08', '2026-07-08', N'## Yenilikler
Tabloda "Çoklu Seçimi Satır Olarak Ekle": Çoklu seçim yapılan kolonlara yeni bir ayar geldi: "Seçimleri satır olarak ekle". Çoklu seçim kutusu, çoklu onay listesi, liste kutusu, çoklu lookup ve gözat (browser) ile çoklu seçim kolonlarının hepsinde kullanılabilir.
Tabloda "Çoklu Seçimi Satır Olarak Ekle": Ayar açıkken listeden seçtiğiniz her kayıt, o kolona değer olarak girilmiş AYRI bir satır olur — örneğin 5 kayıt seçtiğinizde 5 satır otomatik eklenir.
Tabloda "Çoklu Seçimi Satır Olarak Ekle": Aynı değer kolonda zaten varsa tekrar eklenmez (mükerrer kayıt önlenir).
Tabloda "Çoklu Seçimi Satır Olarak Ekle": Listeyi hangi satırdan açtıysanız, o satır ilgili kolonda boşsa ilk seçim o satıra yazılır (arkada boş satır bırakmaz); kalan seçimler yeni satır olur.
Tabloda "Çoklu Seçimi Satır Olarak Ekle": Çoklu seçim penceresinde daha önce eklenmiş değerler işaretli görünür ve "seçili" sayacı doğru çalışır — ne eklediğinizi tek bakışta görürsünüz.
RPA: Robotik süreç otomasyonu için yeni bir modül eklendi: Robot akışları, çalıştırmalar ve işleri tek yerden yönetilir (akış / çalıştırma / iş orkestrasyonu + çalıştırıcı altyapısı).
RPA: "Kontrol Merkezi" ekranı: Kayıtlı robotlar, son etkinlikler ve sağlık trendi bir arada; çalıştırmalar ve çalıştırma detayları ayrı ekranlardan izlenir.
EBYS / DKYS geliştirmeleri: Entegrasyonlar: KEP/UETS için TurkKep istemcileri eklendi.
Kataloglar: Katalog kayıtları için "Kayıt Geçmişi" penceresi eklendi — bir kaydın zaman içindeki değişiklikleri görüntülenebilir.
Kataloglar: Katalog alanlarına benzersizlik (unique) kısıtı eklenebiliyor.
Süreç düğümleri için uygulama-içi yardım: Süreç tasarımcısındaki düğümler için Türkçe yardım içerikleri eklendi (Başlat, Kullanıcı Görevi, E-posta Gönder, Durum Güncelle, SAP OData/RFC/HANA/IDoc vb.) — düğüm seçilince ilgili açıklama panelde görünür.
## İyileştirmeler
EBYS / DKYS geliştirmeleri: DKYS: Belge versiyonlama ve imza akışı geliştirildi.
Kataloglar: Katalog kaydı, form örneği (form-instance) tabanlı bir saklama ile de beslenebiliyor; Kullanıcı Görevi (UserTask) ile katalog↔form entegrasyonu iyileştirildi.
## Hata Düzeltmeleri
Tabloda "Çoklu Seçimi Satır Olarak Ekle": Eklenen satırların diğer kolonları normal "satır ekle" gibi davranır: Varsayılan değerler dolar; "ilk değeri otomatik seç" kolonları artık toplu eklemede de her satırda güvenilir biçimde dolar (bazı satırların boş kalması sorunu giderildi).
Tasarımcılarda oturum (token) süresi TAM düzenleme sırasında dolduğunda, henüz otomatik-kaydedilmemiş taslak sessizce kaybolabiliyordu; bu yarış durumu kapatıldı — değişiklikleriniz artık girişe yönlendirmeden önce güvenle saklanıyor.
## Güvenlik
Süreç görevlerinden gönderilen e-postalardaki Onayla/Reddet bağlantıları artık giriş gerektirmeyen bir onay sayfasına gidiyor (/task-decision). Yetki, süre-sınırlı imzalı token''da taşınır; karar tek butonla verilir (mail tarayıcılarının bağlantı ön-yüklemesi yanlışlıkla onay veremez — Boards onay deseniyle aynı).', N'Ara Sürüm', 1, '2026-07-08 09:01:00', '2026-07-08 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.6')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.6', '2026-07-10', '2026-07-10', N'## Yenilikler
Form ve süreç tasarımcılarında "Kaydet"e basınca artık bir seçim çıkıyor: "Normal kaydet" (eskisi gibi) ya da "Yeni versiyon olarak kaydet" (isteğe bağlı not ile). Böylece önemli bir aşamayı işaretleyip sonra ona geri dönebilirsiniz.
Yeni "Versiyonlar" düğmesi: Kayıtlı tüm versiyonlar (numara, not, tarih, kişi) listelenir; istediğiniz versiyona tek tıkla geri dönebilirsiniz.
Geri dönüş sonrası tasarımcı, geri dönülen tanımı otomatik yükler.
Bir lookup alanının seçenekleri bir sorgudan (SQL/katalog) geldiğinde, artık yalnız etiket ve değer değil, sorgunun DÖNDÜRDÜĞÜ diğer kolonlar da görünüyor.
Seçim penceresi bu durumda tek satırlık listeden kolon başlıklı bir TABLOYA dönüşüyor (örn. Ünvan | Şehir | Bakiye | Telefon). Arama bu kolonları da kapsar ("İstanbul" yazınca şehir kolonundan da bulunur).
Seçim yapıldığında, seçilen kaydın tüm kolonlarına form olaylarından erişebilir (onChange script''inde "record" / "selectedOption"); ayrıca otomatik-doldurma (autoFill) artık yalnız kataloglarda değil SQL kaynaklı lookup''larda da çalışır.
Not: Ekstra kolonların gelmesi için sorguda o kolonları da seçmeniz yeterli (örn. SELECT ... , CITY AS Sehir, BALANCE AS Bakiye).
Form olay script''lerine iki yeni yardımcı eklendi: · ctx.sql(''bağlantı'', ''SELECT ...'') — herhangi bir bağlantıda (PostgreSQL veya MSSQL) sorgu çalıştırır; {{alan}} yazımıyla form değerleri güvenli parametreye bağlanır (SQL enjeksiyonu yok). · ctx.catalogQuery(''katalog'', { alan: Değer }, limit) — katalog kayıtlarını alan=değer filtresiyle sorgular.
Tipik kullanım: "Kaydet" (beforeSave) ya da "Kapat" (onBeforeClose) anında bir kontrol yapıp koşula göre işlemi durdurmak. Örneğin açık siparişi olan müşteride kaydı engellemek, ya da tabloda belirli satır sayısına ulaşıldığında uyarmak.
Engelleme mesajları artık köşedeki küçük bildirim yerine EKRANIN ORTASINDA belirgin bir pencerede gösteriliyor — gözden kaçmıyor.
Bir alanın "Değişim (onChange)" olayında artık aynı anda birden çok veri kaynağı tanımlanabiliyor (örn. Hem Katalog hem SQL). Kaynak seçimine basmak mevcut tanımı silmiyor; yeni bir sorgu bloğu ekliyor. Her blok kendi türünü seçebiliyor.
Katalog formlarındaki otomatik-numara artık ayrı bir sayaçtan değil, katalogdaki MEVCUT kayıtlardan hesaplanıyor: 0 kayıt varsa KAT-00001, 3 kayıt varsa KAT-00004.
Formu açıp kapatmak sayacı ARTIK İLERLETMİYOR — numara yalnızca kayıtla ilişkili olarak üretiliyor (eskiden her açılış numarayı yakıyordu).
Otomatik-numara alanı artık salt-okunur — dışarıdan elle değiştirilemez.
Mevcut bir kataloğa Excel aktarırken artık form TASARIMINA dokunulmuyor: Otomatik-numara, açılır liste kolonları, olaylar vb. Korunuyor (önceden import bunları eziyordu).
Anahtar kolon adı esnek eşleşiyor: "Kategori Kodu" da "kategori_kodu" da kabul.
İçe aktarılan tablo satırlarının bağımlı (kaskad) açılır listeleri, kayıt açıldığında otomatik olarak dolduruluyor.
Sayı kolonları: Yeni "Tam sayı" kolon tipi eklendi ve sayı gösterimi akıllandı — ID gibi tam sayılar "1,00" yerine "1" olarak görünüyor.
OnChange ile çekilen katalog verisindeki gereksiz şişkinlik (satır başına gömülü seçenek listeleri) temizlendi; tablo verisi artık düzgün bir dizi olarak geliyor ve belirli bir satır/kolon değerine erişilebiliyor (ör. {{rs.data.datagrid1.1.gider_merkezi}}).
OnChange''in ek filtrelerinde tablo kolonuna göre satır süzme desteği: Örneğin "datagrid1.gider_merkezi = PAZ" yazıldığında yalnız o kolonu eşleşen satırlar döner.
Katalog/hızlı-arama artık kelime değil "içeren" (substring) mantığıyla çalışıyor — "joh" yazınca "John" bulunuyor.
Seçenek editöründe SQL''i "Temizle" ile kaldırınca artık gerçekten temizleniyor (eski sorgu geri gelmiyor).
## Hata Düzeltmeleri
Geri dönmeden önce mevcut hâl otomatik olarak yedeklenir — yanlışlıkla geri dönseniz bile hiçbir şey kaybolmaz, yedekten geri gelirsiniz.
Excel kolonları formdaki alan adlarıyla eşleştiriliyor; tabloya yalnızca form şemasında tanımlı kolonlar yazılıyor, anahtar kolon satıra sızmıyor.
Kayıtlar anahtar kolona göre ekleniyor/güncelleniyor (upsert). Excel''de boş bırakılan bir hücre mevcut değeri silmiyor; bir kayıt için tablo satırı yoksa o kaydın mevcut tablosu korunuyor.
SQL kaynaklı Gözat''ta arama ve sayfalama düzeltildi — düz bir sorguda arama artık gerçekten filtreliyor, sayfa okları gerçekten farklı kayıtları getiriyor, toplam sayı doğru gösteriliyor.
Süreç başlatırken oluşan "API 500" hatası giderildi (görev tablosunda eksik bir veritabanı kolonu tamamlandı).
Keycloak giriş adresi ortama göre dinamik çözülüyor; hem localhost hem sunucu IP''si üzerinden giriş sorunsuz çalışıyor.', N'Ara Sürüm', 1, '2026-07-10 09:00:00', '2026-07-10 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.7')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.7', '2026-07-11', '2026-07-11', N'## Yenilikler
Kullanıcı Yönetimi: Kullanıcılar sayfasına kendi verdiğiniz isimle kolon ekleyebilirsiniz (örn. "Sicil No", "Departman", "Dahili"): Üstteki "⚙ Özel Kolonlar" ile tanımlanır, silinebilir.
Kullanıcı Yönetimi: Tanımladığınız kolonlar kullanıcı tablosunda gerçek sütun olarak görünür.
Kullanıcı Yönetimi: Her kullanıcının satırındaki yeni "Alanlar" düğmesiyle o kullanıcının özel kolon değerleri doldurulur/güncellenir; tabloda anında yansır.
Kullanıcı Yönetimi: Değerler kullanıcının kimlik kaydında (Keycloak) tutulur — ayrı bir eşitleme gerektirmez; yalnız tanımlı kolonlar yazılabilir (güvenli).
Lookup / Seçim Alanlarında Ekstra Kolonlar (yeni): Bir lookup/gözat alanının seçenekleri bir sorgudan geldiğinde, artık yalnız etiket-değer değil, sorgunun döndürdüğü diğer kolonlar da taşınır.
Lookup / Seçim Alanlarında Ekstra Kolonlar (yeni): Seçim penceresi bu durumda kolon başlıklı bir tabloya dönüşür ve arama bu kolonları da kapsar.
Lookup / Seçim Alanlarında Ekstra Kolonlar (yeni): Seçim yapıldığında, seçilen kaydın tüm kolonlarına onChange olaylarından erişilebilir (record / selectedOption); otomatik-doldurma (autoFill) artık SQL kaynaklı lookup''larda da çalışır.
MongoDB Veri Kaynağı + Kod Yazmadan Sorgu Kurucusu (yeni): Gözat (browser) bileşeni artık MongoDB''yi de veri kaynağı olarak kullanabilir.
MongoDB Veri Kaynağı + Kod Yazmadan Sorgu Kurucusu (yeni): Ham UUID yapıştırmak yerine yeni "🍃 Mongo yaz" satır-içi editörü: Sorguyu yazıp kaydedersiniz, bağlantı otomatik kurulur.
MongoDB Veri Kaynağı + Kod Yazmadan Sorgu Kurucusu (yeni): Kod yazmak istemeyenler için "Kolay kurucu": Form ya da Katalog seçin, tabloyu (datagrid) seçip satırlara açın, göstermek istediğiniz kolonları işaretleyin ve "Sorguyu üret" deyin — sorgu otomatik oluşur.
MongoDB Veri Kaynağı + Kod Yazmadan Sorgu Kurucusu (yeni): Filtre satırı: Kaynak kolon = değer; değere form alanı bağlanabilir ({{alan}}). Datagrid hücresinde satır bağlamı için {{row.kolon}} desteklenir.
MongoDB Veri Kaynağı + Kod Yazmadan Sorgu Kurucusu (yeni): Mongo sorgularında arama, sıralama ve sayfalama artık düz sorgularda da otomatik çalışır (SQL Gözat''ta olduğu gibi).
Tablodan Tabloya Filtre (yeni): Bir tablodaki satıra tıklayınca başka bir tabloyu süzebilirsiniz. Kaynak tablonun özelliklerinde "🔗 Tablodan tabloya filtre" ile kural tanımlanır: Kaynak kolon → hedef tablo + hedef kolon.
Tablodan Tabloya Filtre (yeni): Çalışma anında: Kaynak tabloda bir satıra tıklarsınız; hedef tablo yalnız ilgili kolonu bu satırın değerine eşit satırları gösterir. Aynı satıra tekrar tıklamak filtreyi kaldırır.
Tablodan Tabloya Filtre (yeni): Yön tek taraflıdır (kaynak → hedef); ters filtre oluşmaz.
Süreç e-postalarına yapılandırılabilir "karar linkleri" bileşeni eklendi — alıcı, giriş yapmadan (süre-sınırlı imzalı bağlantıyla) tek tıkla karar verebilir; kararın ardından açılan sayfada ilgili form gösterilir.
Uçtan uca örnek olarak "Masraf Yönetimi" demo formu + süreci eklendi.
DMS: Turkcell mobil imza sağlayıcısı resmi MSSP SDK V2.3''e göre gerçeklendi.
DMS: Mobil imza butonu "SMS gönder" yerine "Doğrulama talebi gönder" olarak netleştirildi.
DMS: E-imza sağlayıcı ayarları: Düzenlemede kayıtlı değerler geri gelir ve gizli anahtar (secret) korunur.
Tablo (datagrid) kolonlarının olay editöründe "Temizle" artık gerçekten temizliyor — eskiden temizleyip kaydedince sorgular geri geliyordu.
## İyileştirmeler
Süreç tasarımcısında karar-linki editörü + süreç değişken paneli geliştirildi; form alanı yer-tutucuları (placeholder) mail/karar akışına bağlanabilir.
## Hata Düzeltmeleri
MongoDB sorgularında parametreler düzeltildi: "{{alan}}" (tırnaklı) yazımı doğru çalışır ve iç içe değerler ({{row.kolon}}) çözülür.
Gözat Mongo editöründe filtre değer kutusunun daralma sorunu giderildi; değere form alanı bağlamak için seçim listesi eklendi.
## Kaldırılanlar
DMS: İmza servisinde gereksiz yeniden-deneme (retry) kaldırıldı; erişilemeyen uçta hızlı ve anlaşılır hata verilir. SSH tünel/test senaryoları için opsiyonel TLS-bypass eklendi.', N'Ara Sürüm', 1, '2026-07-11 09:00:00', '2026-07-11 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.8')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.8', '2026-07-13', '2026-07-13', N'## Yenilikler
Tabloda "Çoklu Seçimi Satır Olarak Ekle": Çoklu-seçim kolonlarında (çoklu lookup, gözat/browser, çoklu seçim/onay kutusu, liste) seçilen her kayıt AYRI bir satır olur; kolonda zaten olan değer tekrar eklenmez, açtığınız boş satır ilk seçimle dolar, diğer kolonlar varsayılan/ilk-değerle gelir.
Lookup/Gözat olay editöründe Katalog Anahtar Alanı: Katalog adımı "Tek kayıt (key)" modunda "Key field" ve ek filtreler artık kataloğun formundaki bileşenlerden (açılır liste / öneri) seçilir; elle yazmak gerekmez, eski özel değerler korunur.
"Süreç" Veri Kaynağı (kod yazmadan Mongo kurucusu): Form ve Katalog''un yanına "Süreç" eklendi. Formu seçince o formun tüm süreçlerden gelen verileri, süreci seçince yalnız o sürecin verileri gelir; süreç birden çok form kullanıyorsa siz seçersiniz, tek form varsa otomatik gelir.
Süreç içindeki form gönderimleri artık sorgulanabilir kayıt olarak da tutulur; bu kayıtlar yalnız "Süreç" veri kaynağında görünür ve formun Yanıtlar sekmesini, admin listesini veya katalog tarayıcısını kirletmez. Yalnız-okuma inceleme adımları ve boş (yalnızca buton) gönderimler için kayıt oluşturulmaz.
P2P Satın Alma Zinciri (birleştirilen): Teklif Kıyaslama / TCO, Fiyat Araştırma (referans katalog + band otomatik doldurma), Sipariş (PO) + çerçeve anlaşmalar, Kalite Kontrollü Mal Kabul (GRN), Fatura 3''lü eşleştirme + ödeme.
Gerçek-Zamanlı e-İhale modülü (birleştirilen): Canlı teklif panosu, ihale listesi/oluşturma ekranı, formlarda "auction" (ihale) alan tipi, e-İhale kuralları katalogu + formu.
Süreç Zamanlama (ProcessSchedule) altyapısı + tetikleyiciler; pano grafik iyileştirmeleri ve buton genişliği düzenlemeleri (birleştirilen).
Masraf seed''i mevcut formun tasarımını artık geri yazmıyor (kullanıcı düzenlemeleri korunur).
## İyileştirmeler
Katalog ↔ Form canlı şema: Formu düzenleyip kaydettiğinizde bağlı katalog(lar) yeni şemayı ANINDA gösterir (hem yeni kayıt ekleme formunda hem mevcut kayıtları düzenlerken); artık formu katalog için yeniden seçmek gerekmez.
Sürüm Notları dosyadan içe aktarma: Üst başlık ve "sürüm özeti" (başlık satırı ile ilk madde arasındaki metin) artık okunur; içe aktarılan sürüm kartında özet olarak görünür.', N'Ara Sürüm', 1, '2026-07-13 09:00:00', '2026-07-13 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.9')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.9', '2026-07-14', '2026-07-14', N'## Yenilikler
— Form / Süreç Tasarımcısı —: Tarih alanına "Manuel yasaklı tarih listesi": SQL/AI''nin yanına, engellenecek tarihleri elle tek tek seçebileceğiniz bir seçenek eklendi (bu günler takvimde üzeri çizili ve seçilemez olur).
— Form / Süreç Tasarımcısı —: Tarih/Saat alanında "Varsayılan: Bugün / şu an": Açık olduğunda form açılır açılmaz alan otomatik olarak günün tarihiyle (tarih alanı) veya o anki saatle (saat alanı) dolar.
— Form / Süreç Tasarımcısı —: Tarih Min/Max aralığı artık gerçekten engelliyor: Aralık dışındaki günler takvimde seçilemez (yasaklı liste ile birlikte kullanıldığında da geçerli).
— Form / Süreç Tasarımcısı —: Label (başlık) bileşeni artık bağlantı olabilir: Dış bir web adresi (yeni sekmede / aynı sekmede açma seçeneğiyle) veya uygulama içi bir hedef (Süreç / Form / Dashboard / Katalog) verebilirsiniz; uygulama-içi hedef RiverAI''nin kendi sekme düzeninde açılır.
— Kurallar / Olaylar (İlk Açılış Davranışı) —: Lookup (gözat) alanı kayıtlı seçimi etiketiyle gösterir: Süreç ilerleyip form yeniden açıldığında alan artık ham kimlik (örn. "118") yerine seçilen kaydın adını (örn. "Malzeme Yönetimi") gösterir.
Sözleşme Yönetimi: Sözleşme kataloğu + formu + onay akışı.
Hizmet Kabul / Hakediş: SLA, kesinti ve net tutar hesabıyla.
Bütçe ve maliyet kontrolü: Bütçeler kataloğu + Bütçe Kontrol formu.
Satın Alma Yönetim Panosu: Raporlama, KPI ve dashboard.
Satınalma Siparişi (PO) belge şablonu + otomatik PDF üretimi (documentTask).
AI teklif değerlendirme (insan onaylı) ve AI yaygınlaştırma: Talep sınıflandırma, fiyat anomali tespiti, sözleşme risk analizi.
Zengin demo verisi: Gerçek akış tetiklemeli satın alma süreçleriyle dolu örnek veri.
## İyileştirmeler
— Arama & Genel —: Türkçe büyük/küçük harf duyarsız arama HER veri kaynağında: Katalog, harici Mongo, SQL, Mongo ve Kullanıcı (Keycloak) aramalarında "İSM" yazınca "ismet", "GÜNEŞ" yazınca "güneş" bulunur. İ/İ/ı/I aynı harf sayılır; ş/Ş, ğ/Ğ, ü/Ü, ö/Ö, ç/Ç eşleşir (ş ile s birbirine karışmaz).
— Form / Süreç Tasarımcısı —: Süreç kaydında klasör artık zorunlu: Klasör seçmeden kaydedilemez (klasörsüz kayıtlar sol menüde görünmüyordu). Uygun klasör yoksa, kayıt penceresini terk etmeden "+ Yeni klasör" ile oracıkta oluşturup seçebilirsiniz.
— Form / Süreç Tasarımcısı —: Yasaklı tarih/saat editöründe "Test et": Sorguyu çalıştırıp kaç kayıt döndüğünü gösterir; hata varsa artık sessiz kalmıyor, hatayı hem editörde hem formda görürsünüz.
OCR "AI ile çözümle": Çözümlenen alanlar artık katalog/SQL/sabit seçicilerin canlı değerleriyle sınırlandırılabiliyor.
Onay zinciri alanı gerçek süreç görevleriyle canlı hale geldi.
Satın alma/işlem formlarında tutarlı görsel düzen (3D paneller ve eşit genişlikte 3D butonlar).
## Hata Düzeltmeleri
— Kurallar / Olaylar (İlk Açılış Davranışı) —: Koşullar form ilk açıldığında da uygulanır: Bir alanın varsayılan/kayıtlı değeri kuralı zaten sağlıyorsa, göster/gizle kuralları ve no-code olay/kural grupları (ve bunlara bağlı SQL/bağımlı liste doldurma) kullanıcı bir şeye dokunmadan, form açılır açılmaz çalışır. Önceden yalnız kullanıcı elle değiştirince tetikleniyordu (özellikle Seçim Kutusu/Onay Kutusu''na bağlı kurallarda).', N'Ara Sürüm', 1, '2026-07-14 09:00:00', '2026-07-14 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.10')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.10', '2026-07-20', '2026-07-20', N'## Yenilikler
"Özel (theme builder)" pano teması: Tema seçici "Özel" olunca altında bir özelleştirme paneli açılır. Renkler (pano / kolon / kart / metin / vurgu / kolon başlığı), yazı tipi (varsayılan / el yazısı / serif / daktilo) ve "yapışkan not" görünümü ayarlanır; canlı önizleme ile sonucu anında görürsün. Tema panoyla birlikte kaydedilir.
Hazır başlangıç şablonları: Kurumsal / Dengeli / Reklam Ajansı + pastel seri (Pembe, Lavanta, Nane, Gökyüzü, Şeftali, Limon) + koyu varyantlar (Gece Moru, Orman Yeşili, Okyanus). Şablona tıkla, üzerinde düzenle. Her şablonun yanında mini renk önizlemesi var.
Kolon bazlı VARSAYILAN ATANAN ve ONAYCI: Her kolon için varsayılan atanan(lar) ve onaycı(lar) seçilir — kişi, rol veya grup olabilir. O kolonda kart oluşturulunca otomatik uygulanır.
"Detaylı kart ekle" ekranında kolon seçilince (ya da o kolonda kart açarken) varsayılan atanan + onaycı alanlara otomatik gelir; elle değiştirebilirsin.
Yeni kart kuralları: "Oluştururken açıklama zorunlu (işin detayı)" ve "Tamamlarken süre ekranında iş detayı (yapılan iş) zorunlu". Kurallar hem arayüzde hem sunucuda doğrulanır.
Genel pano ayarı: "Kartı oluşturan otomatik onaycı olsun" — açıkken kart açıkça onaycı seçilmediyse onaycı, kartı oluşturan kişi atanır.
Süre ekranına GENEL AÇIKLAMA alanı: Tüm süre kayıtlarının özetini/notunu tek yerde yazabilirsin.
Kart detayında "Atayan" (kartı oluşturan kişi) görünür.
Min/Max tarih aralığı artık gerçekten engelliyor: Aralık dışındaki günler takvimde seçilemez (yasaklı gün listesiyle birlikte de).
Tarih/Saat alanında "Varsayılan: Bugün / şu an" — form açılınca otomatik doldurur.
Tarih alanına manuel yasaklı gün listesi (SQL/AI''nin yanına) + yasaklı-liste sorgusu için "Test et" (hata artık sessiz kalmıyor).
Data Console (SQL çalışma ekranı) yenilendi: Şema / Kaydedilenler panelleri kenar drawer''ına alındı, salt-okunur için hacimli yeşil/kırmızı toggle, Sonuçlar/Audit sekmeleri dengelendi, editör ↔ sonuç arasına sürüklenebilir bölücü (splitter).
Data Console AI SQL Asistanı: "✨ AI" ile doğal dilden sorgu üretme veya çalışma hatasını otomatik düzeltme (şema-farkında; PostgreSQL rezerve-kelime tırnaklama kuralıyla).
Teklif AI: Gelen teklif/talep maillerinden AI ile aday malzeme eşleştirme + Teklif Analiz sayfası.
Posta kutusu entegrasyonu (Office365 / Graph): Gelen kutusunu bağlama (kullanıcı-bazlı OAuth); arka planda yeni teklif maillerinin otomatik taslağa dönüşmesi.
Tenant taşınabilirliği: Tenant dışa/içe aktarma (export/import) — sihirbaz modalları + arka plan işi + canlı ilerleme.
Form Hesaplama (Formül Oluşturucu): Form alanları için formül/hesap motoru.
Dosya alanı geliştirmeleri + OnlyOffice ile belge düzenleme; belge dönüştürme görevi (documentConvertTask).
AI ile süreç oluşturma (görsel/BPMN''den) + çözümlenemeyen düğüm eşleme sihirbazı.
Organizasyon şeması ekranı; Entegrasyonlar ekranı; Yardım (Help) sistemi; Fabrika ayarlarına sıfırlama.
## İyileştirmeler
Kolon ayarları ekranı sadeleştirildi: Kalabalık düz liste yerine bölümlü kart düzeni (Görünüm / Varsayılan atama / Kart oluşturma / Kart tamamlama / Diğer). "Tümünü seç" ve toplu düzenleme paneli de aynı düzene getirildi.
"Harcanan Süreler" ekranı yeniden tasarlandı: Her kayıt ayrı kart-satırı; tarih, kişi (baş harf avatarı), saat/dakika, tam-genişlik "Yapılan iş". Süre kaydını EKLEYEN kişi otomatik "işi yapan" olur (kişi seçimi/temizleme yok).
Kart detay kaydı: Sağ üstte ikon "Kaydet" (pencere kapanmadan kaydeder) + altta "Kaydet & Kapat". Her değişiklikte kaydedip pencereyi kaybetme derdi bitti.
Formlar: Listede "Projeye Taşı", designer''da "kaydederken hataları gözardı et", "Listeye dön" proje kapsamı düzeltmesi.
İş akışları: Worklist sayfalama toplam sayısı düzeltmesi; form-proje atama; login teması + CORS iyileştirmeleri.
## Hata Düzeltmeleri
Özel tema artık bildirim/onay/kilometre-taşı gibi diğer kayıtlarda sessizce silinmiyor (kaydı gönderilmeyen alanlar korunuyor).', N'Ara Sürüm', 1, '2026-07-20 09:00:00', '2026-07-20 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.11')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.11', '2026-07-20', '2026-07-20', N'## Yenilikler
Kaydedilmemiş değişiklik hatırlatması: Kart penceresini (×, ESC veya "Vazgeç") kaydetmeden kapatmaya çalışırsan, hangi alanların kaydedilmediğini listeleyen küçük bir uyarı çıkar (ör. "Süre kayıtları, Açıklama kaydedilmedi"). Yalnızca düzenleme yetkisi olan kullanıcıda görünür.
Yeni pano ayarı: "Onaya gönderilince / tamamlanınca kartı kilitle". Açıkken, bir kart onaya gönderildikten (onay bekliyor) veya tamamlandıktan sonra içeriği düzenlenemez (salt-okur) — böylece onaydaki/bitmiş kartlar sonradan değiştirilemez. Onaylayan yine onaylayıp reddedebilir. Ayar kapalıyken bu durumlarda bile (eskisi gibi) düzenlenebilir. Kural hem arayüzde hem sunucuda uygulanır.
Kart detay penceresinin üst başlığı artık kart numarasının yanında kartın başlığını da gösterir (ör. "Kart detayı · RA003-5 — Delegasyon (vekalet) yönetimi"). Başlığı düzenledikçe canlı güncellenir.
Süreç akışı oynatıcısı (Process Replay): Akış üzerindeki bir düğümün üzerine gelince bilgi balonu (tooltip) açılır — düğümün durumu, neden beklediği (atanan kişi + süre), varsa hata mesajı ve süreç değişkenleri (ilk 6 + "hepsini gör"). Düğüme tıklayınca AI teşhisi (eskisi gibi) korunur; balon, pencere kenarına göre otomatik konumlanır.
## İyileştirmeler
Kart detayında "Süreler" (Harcanan Süreler) ekranının kendine ait ayrı "Kaydet" butonu kaldırıldı. Girdiğin süre satırları ve genel açıklama artık kartın ana Kaydet''iyle (sağ üstteki kaydet ikonu veya alttaki "Kaydet & Kapat") birlikte kaydediliyor. Her ekran için ayrı ayrı kaydetme derdi bitti.
Kart penceresinin sağ üstündeki "Kaydet" ikonu, yanındaki kapat (×) butonuyla aynı sade görünüme getirildi.
KYS (Kalite Yönetim Sistemi) modülü yeni bir görünüme kavuştu: Marka rehberi teması (cream/turuncu renkler, Poppins başlık + Plus Jakarta gövde yazı tipleri). Tema yalnız KYS ekranlarına özel (scoped) — RiverAI''ın geri kalanı etkilenmez.
KYS doküman hazırlama sihirbazı yenilendi: Tamamlanma yüzdesi halkası, sol menü yerine tepede yatay adım çubuğu (tıklayarak gezinme + tamamlananlarda ✓), çalışan kaydırıcılar (bağlantı süresi / gözden geçirme periyodu) ve paneller arası geçiş animasyonu.
## Hata Düzeltmeleri
"Onaya gönder" / "Tamamlamaya gönder" düğmelerine basmadan önce, o an girilmiş ama henüz kaydedilmemiş süre/değişiklikler otomatik kaydedilir. Böylece girilen süre kaybolmuyor ve "süre girilmedi" gibi kurallar yanlış tetiklenmiyor. Kayıt başarısız olursa gönderim durur (veri kaybı önlenir).', N'Ara Sürüm', 1, '2026-07-20 09:01:00', '2026-07-20 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.12')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.12', '2026-07-21', '2026-07-21', N'## Yenilikler
KYS geneli / kabuk: Üst bardaki kullanıcı adı ve baş harfleri artık gerçek oturum açan kullanıcıdan geliyor (sabit örnek isim kaldırıldı).
Kalite Kokpiti: Kişiye özel karşılama panosu: Oturum açan kullanıcıya ait "Onay Bekleyen Belgeler", "Bana Gelen Tebliğler" (okundu onayı bekleyenler), "Doküman Değişiklik Talepleri", "Açık / Dış Müşteri Şikayetleri" ve "Doküman İşlemleri" tek ekranda.
Kalite Kokpiti: Şikayet kayıtları için aşama takibi (Triyaj → Çözüm ekibi → Kök neden → Aksiyon → İzleme → Değerlendirme → Sonuç) ve SLA / geri-dönüş göstergeleri.
Dış Müşteri Şikayet: Uçtan uca şikayet yönetimi: Şikayet Talebi, Şikayet İşlemleri, Grafikler ve Onay Bekleyen Şikayetler sekmeleri.
Dış Müşteri Şikayet: Ekip lideri / üye atama, termin tarihli Gelişme Raporu, Aksiyon planlama-gerçekleştirme, İzleme ve Değerlendirme (izlemeye alma, süre uzatma, uygun bulup kapatma).
Dış Müşteri Şikayet: Kök Neden Analizi şablonları: 5N1K, Balık Kılçığı (Ishikawa) ve Liste görünümü.
Dış Müşteri Şikayet: Onay işlemi (onaycı / onay tipi / onayla-reddet) ve şikayeti tek tuşla DİF kaydına dönüştürme.
Dış Müşteri Şikayet: Karar Destek ve Analiz Merkezi: X-Y Analiz Matrisi ile şikayet kategorileri ve yapay zekâ destekli öneriler.
Dış Müşteri Şikayet: Not: Aksiyona/şikayete dosya ekleme arayüzde görünüyor ancak dosya servisi sonraki faza bırakıldı (henüz aktif değil).
DİF Yönetimi: Detaylı filtre/arama: DİF no, tanım, iş yeri veya açan kişiye göre.
DİF Yönetimi: Aşama bazlı iş akışı (Uygunsuzluk, Ekip, Gelişme, Kök-neden, Aksiyon, Kapatma, Sonuç), Düzeltici / İyileştirici ayrımı ve yaygınlaştırma.
DİF Yönetimi: Yeni DİF açıldığında "Açan" alanı otomatik olarak oturum açan kullanıcıya atanıyor.
Denetim Faaliyetleri: Denetim Planları, Bekleyen Denetimler ve Plansız Denetimler görünümleri; plan durum dağılımı grafiği ve plana göre filtreleme.
Denetim Faaliyetleri: Denetim çizelgesi, baş denetçi / ekip üyesi atama, denetim türü, öncelik, plan dosyaları.
Denetim Faaliyetleri: Soru-cevap yürütmesi (yeni soru ekleme) ve bulgu değerlendirme (Uygun / Uygun değil / Geliştirilmeli).
Denetim Faaliyetleri: Denetim raporu, dışa aktarma, erteleme/iptal ve denetimi DİF''e dönüştürme.
OCR Tarama Merkezi: Ağ tarayıcısı (eSCL) ile "Akıllı Bağlantı": Cihaz listeleme/seçme, tek/çift taraflı (Duplex/Platen), renkli/gri tonlama tarama.
OCR Tarama Merkezi: Görüntü iyileştirme: Otomatik döndürme, eğiklik düzeltme, boş sayfa silme, kenar kırpma, kontrast/parlaklık/keskinlik.
OCR Tarama Merkezi: OCR ile metin tanıma + durum takibi ve tarama kuyruğu yönetimi (kuyruktan kaldırma, iptal).
OCR Tarama Merkezi: Otomatik klasörleme ve kod önerisi: Güven seviyesine göre evrakları ilgili klasöre yönlendirme, "Kodları Uygula ve Aktar", farklı klasör seçme.
OCR Tarama Merkezi: Gelen evrak görüntüleme ve Doküman DNA Kartı bağlantısı.
Arşiv Yönetimi (fiziksel arşiv): Dashboard, Arşiv Haritası, Layout Planı ve Raf/Kutu görünümü ile arşivin fiziksel yerleşimini (dolap/raf/kutu) inceleme.
Arşiv Yönetimi (fiziksel arşiv): Kutu işlemleri: Yeni kutu ekleme, taşıma, silme; evrak detayı ve fiziksel konum zinciri.
Arşiv Yönetimi (fiziksel arşiv): Dışarı verme/rezervasyon takibi ve saklama süresi / imha takibi (süre yaklaşıyor, imha zamanı, imhaya ayır).
Operasyon Merkezi: Doküman yaşam döngüsü aksiyonları tek ekranda: Dokümanı Aç, Gözden Geçirme Başlat, Onay Sürecini Başlat, Revizyon Talebi, Askıya Al, Yürürlüğe Al, İptal Et, Arşivle, Yönlendir, talebi reddet.
Operasyon Merkezi: Statüye göre filtreleme (Yürürlükte, Askıda, Süresi Geçmiş, Süresi Yaklaşan, Tümü) ve statü akışı görünümü.
Operasyon Merkezi: Zenginleştirilmiş kolonlar: Doküman kodu/adı/özeti, klasör kodu/adı, hazırlayan + tarihi, revize eden, revizyon no/tarihi, talebi yapan + departmanı.
Klasör Tanımlama ve Yetkiler: Klasöre kod verme (elle ya da şablondan otomatik üretim, ör. "PR-KAL-{seq}"; boş bırakılınca otomatik).
Klasör Tanımlama ve Yetkiler: Klasör ayarları tek panelde: Gizlilik, doküman tipi, kontrol/gözden geçirme tipi, zorunlu alan bayrakları, saklama/imha, baskı-kopya, devralma/kilit.
Klasör Tanımlama ve Yetkiler: Klasörü taşıma (üst klasör değiştirme), kopyalama, kod değiştirme, silme.
Klasör Tanımlama ve Yetkiler: Yetki matrisine yeni kolonlar: Yazdırma, Eski revizyonları görme, Alt klasör tanımlama ve "Zorunlu" (kaldırılamaz) satır; kullanıcı/grup/role göre yetki tanımlama ve arama.
Kontrollü Kopya: Birden fazla listeleme görünümü: Ana liste + föy yazdırma, ayrıntılı filtreli listeleme, klasör bazlı, saha bazlı ve organizasyon/gizlilik/ürün-süreç bazlı.
Kontrollü Kopya: Yazdırılabilir dağıtım föyleri (genel, dağıtım yeri bazında, grup bazında, doküman föyü).
Kontrollü Kopya: Yeni kontrollü kopya oluşturma, nüsha iade etme ve nüsha silme.
Değişiklik Yönetimi: Doküman değişiklik talebi akışı: Mevcut ↔ talep edilen madde/uygulama karşılaştırması, değişiklik gerekçesi.
Değişiklik Yönetimi: Etki işaretleri: Eğitim gerekliliği, müşteri/tedarikçi bilgilendirme, organizasyon şeması etkisi, prosedür/talimat/form/risk analizi güncelleme.
Değişiklik Yönetimi: Revizyon geçmişi, işlem izi (audit trail), yorumlar; red/iptalde zorunlu gerekçe.
Kalite Kayıtları: Saklama ve imha takibi; durum filtreleri (Taslak / Onayda / Onaylı); klasör içinde ve kod/ad ile arama.
Kalite Kayıtları: Silme talebi oluşturma, onaylama ve imha etme akışı (gerekçe zorunlu).
Doküman DNA Merkezi: Doküman soyağacı/izlenebilirlik kartı: Revizyon zinciri, referans alınan/verilen dokümanlar, ilişkili süreç/ürün/riskler, sahibi, departman, gizlilik sınıfı, yayınlanma ve sonraki gözden geçirme tarihi, bağlı formlar; ilişki ekleme/silme ve doküman arama.
Arama / Askıya Alma / Revizyon / Hazırlama / Raporlar: Arama: Klasör içi/filtreli doküman arama, versiyon geçmişi ve buradan doküman hazırlama talebi oluşturma.
Arama / Askıya Alma / Revizyon / Hazırlama / Raporlar: Askıya Alma: Dokümanı gerekçe zorunlu olacak şekilde askıya alma.
Arama / Askıya Alma / Revizyon / Hazırlama / Raporlar: Raporlar: Doküman durum/tip dağılımı, departman bazlı yük, değişiklik talebi durumları gibi grafikler + yapay zekâ destekli öneri kutusu (ilgili ekrana yönlendirme).
DMS / Klasör altyapısı (arka plan): Klasörlere "Kod" ve esnek "Ayarlar (jsonb)" alanı eklendi (klasör tanımlama ekranını besler).
DMS / Klasör altyapısı (arka plan): Klasör taşıma (yeniden üst klasör atama) desteği; klasörün kendi altına taşınması engellenir.
DMS / Klasör altyapısı (arka plan): Yetki matrisi kolonları (Yazdırma / Eski revizyonlar / Alt klasör / Zorunlu) için veritabanı desteği (yeni migration).
Sürüm sabiti 1.1.12''ye yükseltildi.
Mobil uygulama altyapısı: Capacitor ile Android ve iOS native projeleri + mobil yapı ortamı (.env.mobile). RiverAI''ın mobil paketlenmesi için temel atıldı.
## İyileştirmeler
KYS geneli / kabuk: Her modül kendi renk temasıyla açılıyor (DİF, Denetim, Şikayet, Arşiv ayrı paletler) — görsel yenileme.
KYS geneli / kabuk: "DİF / Düzeltici Faaliyet" başlığı "DİF Yönetimi" olarak sadeleştirildi.
DİF Yönetimi: DİF listesinde artık Öncelik ve Açan (kaydı oluşturan) kolonları görünüyor.
Arama / Askıya Alma / Revizyon / Hazırlama / Raporlar: Revizyon: Doküman revize/iptal ekranı yeniden tasarlandı.
Arama / Askıya Alma / Revizyon / Hazırlama / Raporlar: Hazırlama: Sihirbaz üstteki yatay adım çubuğundan sol dikey adım listesine (stepper) taşındı; zorunlu alan tamamlanma oranı adımların altında.
DMS / Klasör altyapısı (arka plan): Aynı konumda aynı adlı klasör oluşturma/taşımada teknik hata yerine anlaşılır uyarı ("Bu konumda aynı adlı bir klasör zaten var").
Kimlik doğrulama yenilemesi (OIDC / AppAuth): Web ve mobil için ortak modern oturum akışı (giriş başlatma, korumalı rota, tenant seçici).
[Yeni/İyileştirme] Formlar: Form Görüntüleyici (FormRenderer) ve zengin bileşenler geliştirildi, Form Tasarımcısı iyileştirildi, herkese açık (public) form doldurma akışları ve tarih kara-liste seçici eklendi.
Süreç Tasarımcısı (ProcessDesigner) geliştirmeleri.
Arka plan: Tenant dışa/içe aktarma (portability) genişletildi, SOAP aksiyonları, iş akışı e-posta gönderimi ve süreç yürütme (ProcessExecution) iyileştirmeleri.
## Hata Düzeltmeleri
Klasör Tanımlama ve Yetkiler: Klasör seçilince açıklama alanı forma yükleniyor (önceki veri-kaybı giderildi).
DMS / Klasör altyapısı (arka plan): Klasör adı değişince veya taşınınca alt klasörlerin yolları (path) otomatik güncellenir.
Birleştirme sonrası Tarih (kara-liste) seçici bileşeninde derleme (tsc) hatası giderildi.
## Güvenlik
Arşiv Yönetimi (fiziksel arşiv): Not: Güvenlik kamerası/sensör/yangın ekipmanı gibi "Digital Twin" katmanı arayüzde yer tutucu; verileri sonraki faza bırakıldı.', N'Ara Sürüm', 1, '2026-07-21 09:00:00', '2026-07-21 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.13')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.13', '2026-07-21', '2026-07-21', N'## Yenilikler
KYS geneli: Ortak "Ek Dosyalar" bileşeni: PDF/Word/Excel/görsel yükleme, indirme ve silme — KYS''nin ilgili tüm ekranlarında aynı deneyim.
KYS geneli: "Dosya ekleme sonraki fazda" tarzı yer tutucu metinler, ilgili özellik artık çalıştığı için kaldırıldı; gerçekten aktif olmayan (bir sonraki faza kalan) alanlarda ise dürüst bilgilendirme bırakıldı.
Doküman Hazırlama: Doküman/şablon dosyası yükleme gerçek dosya servisine bağlandı (taslak kaydedildikten sonra).
Doküman DNA Merkezi & Doküman Kartı (master): Yayınlanan/yürürlükteki her doküman için kalıcı "doküman kartı" (master) ve revizyon geçmişi altyapısı; benzersiz doküman kodu üretimi.
DİF Yönetimi: DİF kayıtlarına gerçek ek dosya ekleme.
KYS Ana Sayfa (Hub): Modül kartlarındaki bekleyen/görev/onay rozet sayıları artık sabit örnek değerler değil, gerçek kayıt sayılarından hesaplanıyor (kayıt yoksa rozet gizleniyor).
Arama: Salt-okunur doküman görüntüleyicisine "İndir" butonu eklendi.
Altyapı (arka plan): KYS geneli dosya (ek belge) servisi: İçerik nesne deposunda (MinIO) tutulur, ilgili kayda türü + kimliğiyle bağlanır; boyut ve dosya türü güvenlik denetimli.
Altyapı (arka plan): KYS doküman kartı (master) ve revizyon geçmişi tabloları; hazırlama taslağı onaya gönderildiğinde otomatik olarak doküman kartına dönüştürülüyor.
## İyileştirmeler
KYS geneli: Tüm ekranlarda kişi/sorumlu/atanan/talep-eden alanları artık kimlik numarası (id) yerine gerçek ad-soyad olarak gösteriliyor; seçim kullanıcı, grup veya rol bazında yapılabiliyor.
Doküman Hazırlama: Doküman sahibi, hazırlayan, kontrol kullanıcısı/grubu, onaylayıcılar, revize eden ve görüş/yetki/dağıtım/bilgilendirme satırları artık örnek isim listesinden değil, gerçek kullanıcı-grup-rol seçiminden geliyor.
Doküman Hazırlama: Yeni doküman açılışında "Hazırlayan" otomatik olarak oturum açan kullanıcıya, "Hazırlama tarihi" bugüne ayarlanıyor.
Doküman Hazırlama: Kontrollü kopya / süreç / ürün satırlarında otomatik doldurulan sahte değerler kaldırıldı; alanlar kullanıcı seçimiyle doldruluyor.
Doküman DNA Merkezi & Doküman Kartı (master): DNA Merkezi, Kontrollü Kopya ve Arama artık bu gerçek doküman kartını kullanıyor; kimlik bilgileri (sahip, gizlilik, klasör, güncel revizyon) tek kaynaktan geliyor.
Operasyon Merkezi: "Askıya Al", "Yürürlüğe Al" ve "Arşivle" işlemleri artık gerçekten dokümanın durumunu değiştiriyor / arşiv kaydı oluşturuyor (önceden yalnızca bilgilendirme veriyordu).
Operasyon Merkezi: "Talebi yapan" alanı gerçek ad-soyad olarak gösteriliyor.
Operasyon Merkezi: Devam eden ve bekleyen taleplerde ek dosyalar gerçek dosya servisiyle görüntülenip yönetiliyor.
Kalite Kayıtları: "İmha et" ve "Arşivle" artık gerçek işlem yapıyor (kayıt siliniyor / arşiv kutusuna taşınıyor).
Kalite Kayıtları: "Dosya indir" ve "Yeni versiyon yükle" işlemleri, kaydın görüntüleme ekranındaki gerçek dosya servisine bağlandı.
Değişiklik Yönetimi: Talep detayında ek dosyalar gerçek dosya servisiyle listeleniyor/yükleniyor.
Değişiklik Yönetimi: "Talep eden" alanında kullanıcı, grup ve rol adları doğru şekilde çözümleniyor (grup adları da görünüyor).
Dış Müşteri Şikayet: Aksiyon sorumlusu / yapacak kişi, izleme sorumlusu ve ekip lideri artık gerçek ad-soyad olarak gösteriliyor.
Dış Müşteri Şikayet: Aksiyon "İlgili Doküman" seçimi gerçek doküman listesinden dolduruluyor.
Dış Müşteri Şikayet: Çözüm ekibi, gelişme, sonuç ve geri-dönüş sekmelerinde ek dosyalar gerçek dosya servisine bağlandı (her sekme kendi dosya setini tutar).
DİF Yönetimi: "Açan" kolonu gerçek ad-soyad olarak gösteriliyor.
DİF Yönetimi: "Sorumlu" alanı sabit örnek isim listesi yerine gerçek kullanıcı-grup-rol seçiminden geliyor.
Denetim Faaliyetleri: Plan/sertifika dosyaları ve soru bazlı ek dosyalar gerçek dosya servisine bağlandı.
Denetim Faaliyetleri: Denetçi ve ekip üyeleri gerçek ad-soyad olarak gösteriliyor.
Arşiv Yönetimi: Kutu içindeki evraklar/belgeler gerçek dosya servisiyle yükleniyor/listeleniyor.
Arşiv Yönetimi: Dışarı verilen kutunun teslim-alan kişisi gerçek ad-soyad olarak gösteriliyor.
Raporlar & Kalite Kokpiti: Rapor ekranındaki şikayet "Sorumlu" ve kokpitteki "Talep eden / açan" alanları gerçek ad-soyad olarak gösteriliyor.
Kontrollü Kopya: Föy listelerinde "Revizyon no" gerçek doküman verisinden gösteriliyor.
Arama: "Hazırlayan" alanı ve versiyon geçmişi gerçek ad-soyad olarak gösteriliyor.
Arama: Doküman hazırlama talebine ek dosya ekleme gerçek dosya servisine bağlandı (talep kaydedildikten sonra).
Altyapı (arka plan): Operasyon Merkezi''nin durum işlemleri için taslak listesine doküman-kartı bağlantısı eklendi.', N'Ara Sürüm', 1, '2026-07-21 09:01:00', '2026-07-21 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.14')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.14', '2026-07-22', '2026-07-22', N'## Yenilikler
Formlar: Tarih / Saat Seçici: Takvim ve saat açılır pencereleri panel/kart içindeki "taşmayı gizle" (overflow) ayarı yüzünden kırpılıyordu; artık ekran katmanında (portal) açıldığı için kırpılmıyor.
Formlar: Tarih Alanını Döneme Bağlama: Form tasarımcısında bir tarih alanı, başka bir "dönem" alanına (ay+yıl, YYYY-AA) bağlanabiliyor: Seçilen tarih o aya kısıtlanıyor ve gg.aa.yyyy takvim seçici kullanılıyor.
Diğer (arka plan / iyileştirme): Not: Yukarıdaki form/AI/dağıtım-klasörü kalemlerinin bir kısmı çok-oturumlu bir çalışmanın ara paketidir (WIP); temel davranışlar çalışır durumda olup ince ayarları sürebilir.
## İyileştirmeler
Formlar: Tarih / Saat Seçici: Açılır pencere, ekranda yer yoksa otomatik olarak yukarı açılıyor, sağ kenardan taşarsa içeri çekiliyor; sayfa kaydırınca/boyut değişince konumu güncelleniyor.
Formlar: Veri Kaynağı (SQL/HTTP) Test Paneli: İç içe alan yolu (ör. Satır.kolon) desteklendi.
Dağıtım Klasörleri: Dolu bir dağıtım klasörü artık "zorla sil" ile kaldırılabiliyor: İçindeki öğeler (form / süreç / katalog / dashboard) SİLİNMEZ, yalnızca klasör ataması kaldırılır ve öğeler klasörsüz olarak ana listede kalır. "İçe Aktarılanlar" gibi geçici klasörleri temizlemek için. (Alt klasörü olan klasörler yine önce boşaltılmalıdır.)
Diğer (arka plan / iyileştirme): İş akışı süreç yürütmesinde (onay/red e-posta bağlantıları) küçük iyileştirmeler.
## Hata Düzeltmeleri
Giriş Ekranı (Kurum / Tenant Seçici): Giriş ekranındaki "Kurum" seçim kutusu prod (nginx arkası) ortamda tüm kurumları listelemiyordu; sabit-kodlanmış geliştirme adresleri (localhost:5050/5173) uzaktan erişimde CORS''a takılıp yalnızca mevcut kurumu gösteriyordu. Artık isteğin geldiği adres (location.origin) kullanılıyor ve seçim doğru realm''e yönlendiriliyor; tüm kurumlar listeleniyor.
Formlar: Veri Kaynağı (SQL/HTTP) Test Paneli: Tasarım aşamasında "Test çalıştır", SQL/HTTP sorgusundaki {{alan}} yer tutucularını otomatik algılayıp değer girdiriyor; önceden yer tutuculu sorgular tasarımda boş dönüyordu ("yer tutucu çalışmıyor" sorununun kök nedeni giderildi).
Formlar: Veri Kaynağı (SQL/HTTP) Test Paneli: Test değerleri tipe göre (sayı / doğru-yanlış) dönüştürülüyor; böylece sayısal/mantıksal kolonlarda oluşan veritabanı tip hatası ("operator does not exist") önlendi.
Formlar: Süreç Başlatan Kişi: Formdan (ve herkese açık form doldurmadan) bir süreç başlatıldığında başlatan kişi artık doğru kaydediliyor; önceden "sistem" olarak kaydedildiği için başlatan kişinin "Başlattıklarım" listesinde ve kanban görünümünde görünmüyordu.
Form Tasarımcısı: Sürükle-Bırak: İç içe kapsayıcılarda (panel / sekme / bölme) sürükle-bırak hedeflemesi düzeltildi: Bir alan artık başladığı panelin dışına taşınabiliyor (önceden panele "yapışıp kalıyordu") ve geniş bir alan küçük bir iç panele bırakılabiliyor. Hedef artık işaretçinin üzerinde olduğu en içteki (en küçük) kapsayıcıdır.
AI Fiş / Belge Çıkarımı: Tablo (grid) alanlarında AI çıkarımı sırasında, SQL''e bağlı bir seçici kolonun listesi şirket/proje gibi form değerlerine göre filtreliyse, bu değerler artık AI''a aktarılıyor; böylece doğru seçenek listesi üretiliyor ve AI eşleştirme yapabiliyor (önceden bağlam aktarılmadığı için liste boş kalıyor, eşleşme bulunamıyordu).', N'Ara Sürüm', 1, '2026-07-22 09:00:00', '2026-07-22 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.15')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.15', '2026-07-23', '2026-07-23', N'## Yenilikler
Genel: Kişi Alanları: KYS''nin tüm ekranlarında ve PDF çıktılarında kişi/grup alanları artık ham kullanıcı kimliği yerine gerçek ad-soyad gösteriyor (hazırlayan, sorumlu, denetçi, talep eden vb.).
Doküman Hazırlama: Uygulama-İçi İçerik: Doküman gövdesi artık tarayıcı içinde, biçimli metin editörüyle (başlık, kalın/italik/altı çizili, madde ve numaralı liste, bağlantı) hazırlanabiliyor — harici Word/belge sunucusu gerekmeden.
Doküman Hazırlama: Uygulama-İçi İçerik: Hazırlanan içerik "İçeriği PDF indir" ile antetli, sayfa numaralı A4 PDF olarak alınabiliyor.
Katalog & Sözlük: Müşteri, süreç, ürün, standart, gizlilik gibi listeler artık serbest metin yerine merkezi katalogdan seçiliyor; tutarlı ve tekilleştirilmiş veri.
Arşiv: Fiziksel Konum: Fiziksel arşiv artık Bina → Kat → Oda → Dolap → Raf → Göz hiyerarşisiyle tanımlanıyor; kutular gerçek konuma bağlanıyor ve göz kapasitesi görülebiliyor.
Arşiv: Fiziksel Konum: Arşiv kutusu için üzerinde QR bulunan etiket PDF''i üretilebiliyor.
Raporlar & Çıktılar (PDF + QR): Arşiv etiketi, kontrollü kopya föyü, denetim raporu, değişiklik talebi (ECN) dışa aktarımı, kalite kayıtları listesi ve KYS dağılım raporları (tip/departman/durum/talep) PDF olarak üretiliyor.
Raporlar & Çıktılar (PDF + QR): Çıktılardaki QR kodu okunduğunda ilgili kaydı doğrudan uygulamada açıyor (arşiv kutusu, kontrollü kopya, denetim, değişiklik talebi).
Dış Paylaşım Bağlantısı: Bir doküman için dışarıya, isteğe bağlı şifreli ve süreli (TTL) paylaşım bağlantısı oluşturulabiliyor; bağlantı açıldığında güvenli bir bilgi kartı gösteriliyor.
Değişiklik Yönetimi (ECN): Değişiklik taleplerine ekip yorumu eklenebiliyor; yorumda "@kişi" ile bahsedilen kullanıcıya bildirim gidiyor.
Değişiklik Yönetimi (ECN): Elektronik onay/red izi tutuluyor (kim, ne zaman, karar, not); red için gerekçe zorunlu.
Görev / Bildirim Yönlendirme: Talep ve revizyonlar ilgili kişilere/gruplara iş listesi (uygulama-içi bildirim) ve/veya e-posta ile yönlendirilebiliyor.
Doküman DNA / İçerik Haritası: Bir dokümanın dosyasından "konu → sayfa" içerik haritası çıkarılıp DNA Merkezi''nde gösterilebiliyor.
Yapay Zeka (Kurum AI Anahtarı Tanımlıysa): KYS Rapor Asistanı: Rapor verileri (doküman, DİF, denetim, şikayet sayı/dağılımları) hakkındaki soruları yanıtlıyor.
Yapay Zeka (Kurum AI Anahtarı Tanımlıysa): Doküman ISO Uygunluk Risk Analizi: Hazırlama sırasında dokümana yönelik risk/öneri listesi üretiyor.
Dış Müşteri Şikayeti: Talep Portalı: Müşterilerin/çağrı merkezinin şikayet talebi iletebileceği herkese açık bir portal formu; gelen talepler "Talepten Şikayet Oluştur" kuyruğuna düşüyor.
Dış Müşteri Şikayeti: Talep Portalı: Personel gelen talebi tek tıkla şikayet kaydına dönüştürebiliyor ya da gerekçeyle reddedebiliyor; portal bağlantısı panoya kopyalanabiliyor.
Katalog Veri Kaynağı: Katalogdan beslenen tablo (datagrid) ve kolon-dışı alanlar artık kaydın tamamını alıyor; liste temiz diziler halinde doğru doluyor (önceden yalnızca tanımlı kolonlar geliyordu).
Sorgu Editörleri & Form Araçları: Form/süreç veri kaynakları için SQL/Mongo sorgu editörü paneli.
Sorgu Editörleri & Form Araçları: Test: Görünürlük kuralı ve katalog satır testleri (ön yüz test paketi 86/86 geçti).
## İyileştirmeler
Değişiklik Yönetimi (ECN): Etki analizi, beklenen fayda ve revizyon geçmişi alanları kalıcı olarak saklanıyor.
Demo / Veri: KYS demo verisi ve örnek kayıtlar; ekranların gerçek akışla test edilebilmesi için hazır senaryo.
Sorgu Editörleri & Form Araçları: Tarih/saat alanı için kapalı-gün (kara liste) editörü iyileştirmeleri.
Sorgu Editörleri & Form Araçları: Kurum (tenant) seçici için yönetici yetkilendirmesi; Veri Konsolu (DataConsole) ve Denetim (Audit) ekranlarında iyileştirmeler; personel/insan kaynakları bileşenlerinde küçük düzeltmeler.
## Hata Düzeltmeleri
Yapay Zeka (Kurum AI Anahtarı Tanımlıysa): Not: Kurumun yapay zeka anahtarı tanımlı değilse ekran hata vermeden, nereden anahtar girileceğini belirten nazik bir uyarı gösteriyor.
Form Görünürlük Kuralları: "Göster" / "Etkinleştir" kuralı tanımlı alanlar, koşul sağlanana kadar varsayılan olarak gizli/pasif başlıyor; yalnızca "gizle/pasifleştir" kuralı olan alanlar varsayılan görünür kalıyor. Tasarımcı ve çalışma zamanı birebir aynı davranıyor.
Form Görünürlük Kuralları: Kaydetme öncesi zorunlu alan/doğrulama kontrolleri, bağlam değişkenine bağlı koşullarla aynı mantığı kullanıyor; böylece koşula bağlı zorunlu alanlar gönderimde sessizce atlanmıyor.', N'Ara Sürüm', 1, '2026-07-23 09:00:00', '2026-07-23 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.16')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.16', '2026-07-24', '2026-07-24', N'## Yenilikler
Doküman revizyon ve iptal işlemleri baştan sona çalışır hale getirildi.
Her revizyonda revizyon numarası otomatik artar; aynı doküman için mükerrer kayıt oluşmaz, önceki sürüm arşivlenir, doküman kodu korunur.
Doküman hazırlamada "Word''de aç / düzenle" ile belge OnlyOffice üzerinde düzenlenir; kimlik/başlık alanları kilitli, içerik alanı serbestçe düzenlenebilir ve kaydedilir.
Word''e eklenen içerik "İçeriği PDF indir" çıktısına da yansır.
Revizyonda Word belgesinin başlığı yeni revizyon numarasına göre güncellenir; gövde önceki onaylı içerikten gelir.
Askıya alma artık dokümanın gerçek durumunu değiştirir ("Askıya Alınmış"); askıya alınan doküman Revizyon, Arşiv, İlişkilendirme gibi tüm ekranlarda pasif görünür ve yeniden aktifleştirilebilir.
Askıya alma nedeni, süresi ve tarihi kayıt altına alınır.
Kullanıcı-dostu genel görünüm: Aktif / askıda / toplam özet kartları, durum sekmeleri, arama ve tek tıkla askıya al / aktifleştir.
Doküman hazırlama talepleri artık bu ekrandan onaylanıp reddedilebiliyor (onayla → yürürlüğe alır; reddet → hazırlayana revizyon için geri gönderir).
Talep listesine durum filtresi eklendi (onay bekleyenler öne alınır).
"Rev." (revizyon no) kolonu eklendi.
"Hazırlayan" alanı artık kimlik numarası yerine kişi adını gösterir.
Aynı dokümanın eski/mükerrer (revize edilmiş) satırları listeden gizlendi; her doküman tek satır görünür.
Ana modüllerin yanında "Ayarlar" kartı eklendi.
Doküman türleri/tipleri, form kullanım şekilleri, kontrol tipleri, şablonlar, diller; revizyon/iptal nedenleri; değişiklik/ret nedenleri, öncelikler, talep durumları, etki analizi kalemleri; klasör kod şablonları; dağıtım yerleri ve grupları, nüsha durumları; kayıt türleri, imha yöntemleri, silme nedenleri; yönlendirme hedefleri, bildirim türleri, yaşam döngüsü durumları; sistem parametreleri; e-posta bildirim şablonları.
Alan/sekme aç-kapat paneli (modüllerdeki bazı özellikleri açıp kapatma).
Doküman kısa yol tanımlama (sık kullanılan dokümanları gruplama).
Dokümanı iptalden geri getirme aracı.
Ana modüllerin yanında "Genel Tanımlar" kartı eklendi.
Personel, kullanıcı grupları, departman, unvan, pozisyon, iş yeri, süreçler, ürünler ve ürün tipleri, standartlar, gizlilik sınıfları; ülke/il; meslek ve tehlike sınıfları; roller; modüller; müşteri-tedarikçi; mesaj şablonları.
Grup kuralı: Personel ölçütlerine (departman/unvan/pozisyon/kategori) göre gruba otomatik üyelik.
QR Kod Al: Bir ekran seçip hızlı erişim QR kodu üretme ve indirme.
Ayarlar/Genel Tanımlar sol menüsü başlıklara göre açılır-kapanır (akordeon), sabit ve kendi içinde kaydırılabilir; alttan bir öğe seçince içerik otomatik görünür alana gelir.
Form alan görünürlük kuralları: "göster/gizle" ile "etkinleştir/pasifleştir" ayrımı netleştirildi.
SQL ve MongoDB verilerini birleştiren sorgu (cross-store join) motoru — form ve no-code sorgularında iki veri kaynağını birlikte kullanabilme.
İntranet: İlan panosu (Classifieds) bileşeni.
## İyileştirmeler
Kapsamlı "KYS Analiz ve Yol Haritası" dokümanı hazırlandı (mevcut durum, eQMS olmazsa-olmazları, öncelikli boşluklar ve fazlı geliştirme planı).
Form tasarımı iyileştirmeleri: SQL / MongoDB sorgu editörleri, tarih "kara liste" (kapalı günler) editörü, seçenek (options) editörü, form içi olay/aksiyon çalıştırıcı ve AI ile veri çıkarma paneli.
Uygulama açılış performansı: Ekran bazlı kod bölme (route code-splitting) — uygulama ilk açılışta daha hızlı yüklenir.
Süreç (workflow) tasarımcısı: Değişken paneli ve şema ağacı iyileştirmeleri, "join" görev tipi, süreci başlatan kullanıcının kaydı.
Sık kullanılan formlar ve çalışan listesi ekranlarında küçük iyileştirmeler.
## Hata Düzeltmeleri
Revize/iptal ekranında önizleme ve alan doldurma sorunları giderildi; iptal alanlarında klasör/kod/ad gibi bilgiler otomatik ve değiştirilemez gelir.
Bir doküman Word''de düzenlenirken, uygulama içi metin editörü salt-okunur olur (yanlışlıkla iki yerde farklı içerik oluşması engellenir).
Kurum (tenant) listesinin giriş ekranında eksik görünmesine yol açan sorun giderildi; tüm kurumlar tekrar seçilebilir.
Katalog veri ızgarası (datagrid) düzeltmesi; katalog kayıt ekleme/düzenleme penceresi ve yerleşik format doğrulama.
## Güvenlik
Toplu doküman/klasör yetkilendirme (kişi/grup/role toplu yetki ekle/çıkar).
Yetki grubu menü atama (bir yetki grubuna hangi ekranların açık olacağını çift listeli aktarım ile belirleme).', N'Ara Sürüm', 1, '2026-07-24 09:00:00', '2026-07-24 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.17')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.17', '2026-07-24', '2026-07-24', N'## Yenilikler
KYS: Çok seviyeli onay zinciri: Bir doküman birden fazla onay seviyesinden (kişi + seviye) geçebilir; tüm onaylar tamamlanınca doküman otomatik yürürlüğe girer, herhangi bir reddte hazırlayana revizyon için geri döner.
KYS: Görev ayrılığı: Bir dokümanı hazırlayan kişi kendi dokümanını onaylayamaz.
KYS: Doküman gönderiminde eksik alan kontrolü: En az bir onaylayıcı ve dağıtım listesi seçilmeden, ayrıca hedef klasör belirtilmeden talep gönderilemez.
KYS: Yeni "Görevlerim" ekranı: Kullanıcının onayını bekleyen dokümanlar ve kendisine tebliğ edilen dokümanlar tek ekranda toplanır.
KYS: Onayımı Bekleyenler: Satır üzerinden tek tıkla Onayla / Reddet.
KYS: Tebliğlerim: Yayınlanan dokümanı "Okudum" ile teslim-tebellüğ etme.
KYS: Ana menüye "Görevlerim" kartı eklendi.
KYS: Bir doküman yürürlüğe girdiğinde dağıtım listesindeki kişilere otomatik tebliğ edilir; kimin ne zaman okuduğu (teslim-tebellüğ) kayıt altına alınır.
KYS: Kişi ve grup dağıtımları, ilgili kullanıcılara otomatik açılır.
KYS: Doküman kodu artık kurum içinde benzersizdir; aynı kod ikinci kez oluşmaz.
KYS: Kod üretimi eşzamanlı taleplerde bile ardışık ve çakışmasız numara verir (aynı anda iki kişi doküman oluştursa dahi mükerrer kod çıkmaz).
KYS: Doküman hazırlamada kaynak seçimi (İç / Dış) eklendi.
KYS: Dış kaynaklı dokümanlar için ek bilgi alanları (dış referans/takip) girilebilir.
KYS: Doküman ilişkilendirme artık çift yönlü çalışır: Bir dokümana bağlanan ilişki, karşı dokümanın ekranında da görünür.
KYS: İlişki kurarken doküman "kod · ad" olarak serbest metinden seçilse bile doğru dokümana bağlanır.
KYS: DİF kapatma kuralı: Etkinlik kontrol tarihi girilmiş ancak etkinliği henüz doğrulanmamış bir DİF kapatılamaz (etkinlik doğrulanmadan kapanış engellenir).
KYS: Taslak/onaysız doküman PDF çıktısına "KONTROLSÜZ KOPYA" filigranı basılır.
KYS: Her onay/ret işleminde imzalayan kişi, imza anlamı (onay/ret beyanı) ve imzalanan içeriğin parmak izi (içerik özeti) kayıt altına alınır — böylece onaylanan içerik ile imza kaydı birbirine bağlanır.
KYS: Gözden geçirme hatırlatması: Yaklaşan gözden geçirme tarihleri için otomatik hatırlatma.
KYS: Saklama / imha: Saklama süresi dolan kayıtlar için otomatik takip.
KYS: DİF etkinlik hatırlatması: Etkinlik kontrol tarihi yaklaşan DİF''ler için otomatik hatırlatma.
KYS: Doküman aramada tam metin arama altyapısı eklendi (başlık ve içerik üzerinde daha isabetli ve hızlı sonuç).
KYS: Denetim izi (audit) genişletildi ve değiştirilemez (append-only) hale getirildi: DİF, denetim, şikayet ve arşiv kutusu üzerindeki oluştur/güncelle/ilerlet/sil olayları kalıcı olarak kaydedilir.
Kanban: Panolarda kolon sayısı arttıkça sağa sola kaydırmak zorunda kalıyordunuz. Artık araç çubuğunda tek bir "Görünüm" düğmesi var; tüm görünüm ayarlarınız burada toplandı: · "Sadece benim işlerim" — panoyu size atanmış, onaycısı olduğunuz, bilgi amaçlı eklendiğiniz ve sizin açtığınız kartlara indirir. Açıldığında sizi ilgilendirmeyen proje kolonları kendiliğinden gizlenir; 40 kolonlu bir pano birkaç kolona düşer. · "Boş kolonları gizle" — görünür kartı olmayan projeler çizilmez. · "Pasif projeleri göster" — tamamlanmış / duraklatılmış / iptal projeleri geçici olarak görünür kılar. · Düzen seçimi: · Kolonlar: Klasik yatay Kanban. · Satırlar: Her proje tam genişlik bir satır olur, yatay kaydırma tamamen ortadan kalkar (dar ekran ve mobil için). · Proje seçici — aramalı liste; her projeyi tek tıkla gizleyip geri açabilir, daraltabilir veya "→" ile doğrudan o kolona atlayabilirsiniz. Kolon başına kart sayıları listede görünür. · Hızlı seçimler: "Tümü", "Kartı olanlar", "Tümünü daralt", "Tümünü aç". · "Görünüm kaydet" — kapsam + kolon seçimi + filtreleriniz bir profil olarak kaydedilir ("Sabah panom" gibi) ve tek tıkla geri gelir. · "Sıfırla" — tüm görünüm tercihlerini varsayılana döndürür.
Kanban: Kolonları artık gizlemek zorunda kalmadan DARALTABİLİRSİNİZ: Kolon ince bir dikey şeride iner, adı ve kart sayısı görünmeye devam eder, tek tıkla geri açılır. Şeride kart sürükleyip bırakabilirsiniz — bırakınca kolon kendiliğinden açılır.
Kanban: Görünüm tercihleriniz kalıcı — sayfayı yenilediğinizde, görünüm değiştirdiğinizde ve panolar arasında geçiş yaptığınızda korunuyor.
Kanban: Yöneticiler kolon sırasını hem pano ayarlarından hem de Kanban içindeki kolon menüsünden ("Sola taşı / Sağa taşı") değiştirebilir.
Kanban: Panoda "katkıcı" rolündeki kişiler artık yalnızca şunları yapabilir: · Görevi tamamlama · Yorum ekleme · Alt görev ve kontrol listesi maddelerini işaretleme · Dosya eki EKLEME (ekleri silemez) · Görevi üstüne alarak kabul etme · Görevin bir başkasına yönlendirilmesini TALEP etme Bunların dışındaki alanlarda (atama, termin, öncelik, etiket, açıklama vb.) değişiklik yapamazlar.
Kanban: Bir görev size atandığında bilgilendirme gider; görevi "Üstüne al" ile kabul ettiğinizde kartı açan kişiye onay bildirimi gider.
Kanban: Kabul edilmemiş görevler tamamlanamaz — iş sahipliği netleşir.
Kanban: "Yönlendirme talebi" ekranında artık hem GEREKÇE hem de KİME yönlendirilmesi istendiği seçilebiliyor. Yönlendirme düğmesi "Üstüne al" alanının yanına da eklendi.
Kanban: Yeniden atama ve kabul durumlarında e-posta bildirimi gönderimi pano ayarından açılıp kapatılabiliyor.
Kanban: Bir iş onaya düştüğünde onaycı, işi yapan kişi veya kişileri 1–5 yıldız ile puanlayarak onaylıyor. Kısa bir gerekçe notu eklenebilir.
Kanban: Puanlama genel pano ayarlarından açılıp kapatılabiliyor.
Kanban: Pano yöneticileri "Analitik" ekranındaki yeni "Puanlar" sekmesinde kimin hangi işten kaç puan aldığını kişi ve görev bazında görebiliyor; kim puanladı ve tarih bilgisi de saklanıyor. Rapor CSV olarak dışa aktarılabiliyor.
Kanban: Pano ayarlarına "Katalog bağlantısı" eklendi: Proje adı ve proje kodu artık tanımlı bir katalogdan seçimli gelebiliyor.
Kanban: Katalog bağlantısı varsa yeni kolon eklerken proje listeden seçilir (elle yazım kapatılır); bağlantı yoksa o panoda elle giriş serbest kalır.
Kanban: Takvim görünümü yeniden düzenlendi: Kalabalık günler açılabiliyor, durum renkleri ve açıklama şeridi (legend) eklendi, "Bugün" düğmesi ile hızlı dönüş, hafta sonu ayrımı, bakılan ayın korunması.
Kanban: Profil > Ana Ekran Ayarları''na yeni bir kısayol türü eklendi: Sizi ilgilendiren tüm panolar tek ekranda, pano sekmeleri halinde gelir.
Kanban: Yalnızca sizi ilgilendiren kartlar ve o kartların bulunduğu kolonlar görünür; ilgisiz kolonlar tamamen gizlenir.
Kanban: "Tüm kartlar" düğmesiyle geçici olarak panonun tamamına bakılabilir.
EBYS: UETS gelen belge çekme: 4 saatlik otomatik tarama, sayfalama ve mükerrer kayıt engelleme; canlı ortamda doğrulandı.
EBYS: KEP gelen belge çekme ve KEP ile gönderim (düz gönderim ve iki aşamalı mobil imzalı gönderim).
EBYS: Teslim delili otomatik olarak evrağa işleniyor ve denetim izine kaydediliyor.
EBYS: E-Devlet barkodlu belge doğrulama deseni: Her evrak için değişmeyen, kriptografik olarak rastgele bir doğrulama kodu üretiliyor.
EBYS: Evrak detayında "Doğrulama" düğmesi QR kodu ve paylaşılabilir link veriyor.
EBYS: "Gereği" ve "bilgi" havaleleri ayrıştırıldı.
EBYS: Evraklar otomatik sınıflandırılıyor.
EBYS: Saklama planı yürütmesi: İmha durumu, imha tarihi ve açıklaması.
EBYS: Tutanaklı imha ve devir işlemleri (yalnız ayrıcalıklı kullanıcılar).
EBYS: Yeni "Saklama · Tasfiye" sayfası ve kokpitte saklama radarı.
EBYS: EYP paketi üretme ve içe alma (paket bütünlüğü özeti ile).
EBYS: Ayarlar: Numaratör basamak/önek düzeni, gizli roller, paraf şablonları; kokpit göstergeleri gizlilik filtresine uygun hâle getirildi.
EBYS: Gizlilik erişim kontrolü artık TÜM değiştirme ve işlem yollarında uygulanıyor.
EBYS: Tüm işlemler denetim izine kaydediliyor.
Yeni Modüller: Sözleşme imza, müzakere ve onay akışları.
Yeni Modüller: Saklama / imha politikaları, veri işleme kayıtları, sorgu günlüğü.
Yeni Modüller: Çalışan kayıtları, organizasyon yapısı.
Yeni Modüller: İşe alım: Talep, ilan, aday, mülakat, mülakat odası, değerlendirme kartları, teklif ve teklif onay sayfası.
Yeni Modüller: İşe başlatma (onboarding) ve çıkış (offboarding) süreçleri.
Yeni Modüller: Devir/turnover ve kadro (headcount) raporları, uyum takibi.
Yeni Modüller: İntranet ile köprü kurulumu.
Yeni Modüller: OCR kuyruğu: Yüklenen dokümanların metne çevrilmesi (sayfa bazlı).
Yeni Modüller: Belge üzerinde karartma (redaction) desteği.
Yeni Modüller: Kısıtlı rol tanımları.
Yeni Modüller: Varlık kayıt defteri (Asset Registry) eklendi.
Yapay zekâ ile üretilen süreçlerde adım tipleri artık doğru belirleniyor. Önceden "Tipi belirsiz" görünen bariz adımlar (karar, onay, e-posta gönder vb.) doğru tanınıyor.
Süreç Tasarımı ve Yapay Zekâ › Öğrenme Döngüsü (Flywheel): "Öğrenilen eşlemeler" ekranı eklendi: Öğrenilen kuralları görebilir ve gerekirse silebilirsiniz.
WPF tabanlı rakip sistemlerden süreç içe alma altyapısı.
Süreç değişkenlerindeki alan bazlı değişiklikler zaman çizelgesinde "önce → sonra" biçiminde okunabiliyor.
Formda değişiklik yapıp kaydetmeden sekmeyi kapatmaya veya sayfadan ayrılmaya çalıştığınızda uyarı alıyorsunuz — veri kaybı önleniyor.
İntranet: Widget Yönetimi sekmesi: Widget''ları aktif / pasif yapabilirsiniz.
İntranet: Hikâye (story) moderasyonu: Onaya gönderme → onaylandı / reddedildi akışı.
İntranet: Kitle (audience) hedefleme: Etkinlik, Haber ve Duyurular artık yalnızca seçilen kitleye gösterilebiliyor.
İntranet: Etkinlik kayıt listesi ekranı eklendi.
Kullanıcı, Rol ve Grup Yönetimi › Rol Hiyerarşisi: Roller artık birbirinin altına eklenebiliyor (bir rol başka rolleri içerebiliyor) ve bu ilişki ağaç şeması olarak görüntüleniyor.
Kullanıcı, Rol ve Grup Yönetimi › Rol Hiyerarşisi: Döngüsel bağ kurma (bir rolün kendi üstüne eklenmesi) engelleniyor.
Kullanıcı, Rol ve Grup Yönetimi › Statik Grup Hiyerarşisi: Var olan statik gruplar birbirinin altına taşınabiliyor ve tekrar çıkarılabiliyor.
Kullanıcı, Rol ve Grup Yönetimi › Gruba Üye Ekleme: Tek tek seçim yerine aramalı çoklu seçim geldi: Kullanıcı adı veya e-posta ile anlık filtreleme, onay kutulu liste, "Hepsini seç /bırak", canlı sayaç ve toplu ekleme özeti.
Kullanıcı, Rol ve Grup Yönetimi › Gruba Üye Ekleme: Aranabilir kullanıcı havuzu 100''den 500''e çıkarıldı.
Boş ekranlarda çıkan "Filtreyi temizle" düğmesi bazı durumlarda hiçbir şey yapmıyordu; artık duruma göre doğru eylem sunuluyor.
Rol ve grup eşleştirmesi büyük/küçük harf farkına duyarsız hâle getirildi.
## İyileştirmeler
İntranet: Takvim, Seri İlanlar, Etkinlikler, Sosyal Duvar ve Hikâyeler widget''ları ile tasarım sekmeleri güncellendi.
Kullanıcı, Rol ve Grup Yönetimi › Statik Grup Hiyerarşisi: Grup ve rol ağaçları görsel olarak yenilendi: Bağlantı çizgileri, ikonlar, üye sayısı etiketleri, aramada üst kırılımın korunması, varsayılan açık görünüm.
## Hata Düzeltmeleri
KYS: Denetim bulgusundan tek tıkla DİF (Düzeltici/Önleyici Faaliyet) oluşturma butonu eklendi.
Kanban: Revizyon/ret turları ayrı ayrı kaydediliyor, geçmiş puanlar silinmiyor.
Kanban: Liste görünümünde Başlık / Liste / Sprint / Bitiş / Tahmin (saat) /Öncelik başlıklarına tıklayarak sıralama yapılabiliyor. Eşit değerlerde sıralama artık kararlı (rastgele görünmüyor), boş değerler sona atılıyor, Türkçe alfabetik sıralama uygulanıyor.
EBYS: Sıralı paraf düzeni (sıra / tur / tur numarası) ve dikey zincir görselleştirmesi eklendi; iade sonrası tur kilidi düzeltildi.
EBYS: Anlamsal (semantik) arama: Sorunuzu tam kelime eşleşmesi olmadan da anlayan hibrit arama; evrak–evrak ilgi bağı kuruluyor.
EBYS: Gözden geçirmede bulunan 21 kusur düzeltildi.
~80 eş anlamlı terim tanınıyor; büyük/küçük harf ve Türkçe karakter farkları sorun oluşturmuyor; iki çıkışlı adımlar yapısal olarak karar adımı sayılıyor.
Süreç Tasarımı ve Yapay Zekâ › Öğrenme Döngüsü (Flywheel): Sistem, düzeltilen tip eşleştirmelerini öğreniyor ve bu bilgi TÜM kurumlar arasında paylaşılıyor: Bir kurumda çözülen bilinmeyen terim, diğerinde ilk seferde doğru çıkıyor.
Form alan tiplerinin içe alım sırasında öğrenilerek düzeltilmesi.
Kullanıcı, Rol ve Grup Yönetimi › Statik Grup Hiyerarşisi: Bir grubu başka grubun altına taşıdığınızda grubun listeden kaybolması sorunu giderildi.
Kart ekleme ve görev iptal işlemlerinde alınan kayıt hatası (veritabanı alan tipi uyuşmazlığı) giderildi.
Her dakika tekrarlayan "zamanlanmış tetikleyici tablosu bulunamadı" hatası giderildi.
ÖNEMLİ: "Atamayı devret" işlemi kartın açıklamasını, etiketlerini, kontrol listesini ve eklerini siliyordu — düzeltildi.
Grup veya role atanmış görevlerde onay puanlaması yapılamaması sorunu giderildi (puanlanacak kişiler artık grup/rol üyelerine açılıyor).
Katkıcı rolündeki kullanıcının işaretlediği kontrol listesi maddelerinin kaydedilememesi sorunu giderildi.
Boş bir kolona kart sürükleyip bıraktığınızda kartın taşınmaması sorunu giderildi.
Kanban''da kolon sırası değiştirme çift tıklamada aynı konuma düşüyordu — düzeltildi.
Kişisel görünümde iş bulunmadığında yanlış olarak "Henüz liste yok — ilk kolonu ekleyerek başla" mesajı gösteriliyordu; artık gerçek sebep ve doğru çıkış yolu gösteriliyor.
Grup veya rol üzerinden atanmış görevlerin kişisel listelerde görünmemesi sorunu giderildi.
Üyesi olmadığı hâlde onaycı olduğunuz panoların kişisel listede görünmemesi sorunu giderildi.
Aynı panoyu iki yerde açtığınızda görünüm tercihlerinin birbirini ezmesi sorunu giderildi.
Panolar arasında geçiş yaparken bir panonun tercihlerinin diğerine yazılması (kalıcı tercih kaybı) sorunu giderildi.
Geliştirme sunucusunun kendiliğinden kapanması sorunu giderildi.
## Güvenlik
KYS: Klasör bazlı yetki denetimi artık gerçekten uygulanıyor: Onaylama, iptal, yazma, yazdırma ve okuma işlemleri, ilgili klasöre tanımlı yetkilere göre kısıtlanır. Yetkisi olmayan kullanıcı bu işlemleri yapamaz.
KYS: Yetki tanımlanmamış klasörler açık kalır (mevcut çalışma düzeni bozulmaz); yalnızca yetki tanımlanmış klasörlerde kısıtlama devreye girer.
KYS: Doküman çıktısı almak (yazdırma/dışa aktarma) klasör yetkisine bağlandı; her çıktı alma işlemi denetim izine yazılır.
KYS servisleri için gerçek veritabanına karşı çalışan entegrasyon testleri eklendi (mükerrer kod reddi, atomik kod sayacı, klasör yetki modeli).
Kanban: Kolon gizleme artık kişisel bir görünüm tercihi: Yönetici yetkisi gerektirmiyor ve kolon yapısı kilitli panolarda da çalışıyor (önceden bu panolarda hiç kimse kolon gizleyemiyordu).
EBYS: Bu linki açan kişi SİSTEME GİRMEDEN belgenin künyesini ve aslını görebiliyor — kodun kendisi tek yetkidir.
EBYS: Gizli evrakta doğrulama kodu üretme yetkisi kısıtlı; tüm işlemler denetim izine yazılıyor.
Yönlendirme talebi oluştururken alınan yetki hatası (403) giderildi.
Sekme kapatma uyarısı, gizlilik erişimi, denetim izi ve rol yetkileriyle ilgili güvenlik sertleştirmeleri yapıldı; yönetici kimliğine bürünme (persona) özelliğinin yetki kararlarını etkilemesi engellendi ve süperadmin işlemleri denetim izine alındı.
Salt-okur "Yetki Matrisi" ekranı eklendi: Hangi rolün hangi işlemi yapabildiği tek ekranda görülebiliyor.
## Kaldırılanlar
Kanban: Gantt görünümü sadeleştirildi: Gereksiz başlık katmanları ve kazara sürükleme kaynaklı bağımlılık değişiklikleri kaldırıldı, tıklama ile sürükleme birbirinden ayrıldı.', N'Ara Sürüm', 1, '2026-07-24 09:01:00', '2026-07-24 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.18')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.18', '2026-07-28', '2026-07-28', N'## Yenilikler
BudgetAI: Sol tarafta konsolidasyon tanımları listesi (taslak / çalıştı durumu, üye şirket sayısı, eliminasyon sayısı ile birlikte); yeni konsolidasyon adı yazıp anında oluşturulabiliyor.
BudgetAI: Sağda seçilen konsolidasyonun detayı: Üye şirketler tablosu (para birimi, sahiplik yüzdesi, bütçesi olup olmadığı), şirketlerarası eliminasyonlar tablosu.
BudgetAI: "✨ Demo verisini kur" düğmesi: Hiç konsolidasyon tanımı yokken tek tıkla örnek holding + iştirakler + eliminasyonlarla dolu bir demo kuruyor (yabancı para birimli iştirakler dahil — USD ve EUR).
BudgetAI: Motoru çalıştırınca adım adım bir köprü (waterfall) görünümü beliriyor: Σ Şirketler (FX sonrası) → − Şirketlerarası eliminasyon → = Grup Net Kârı → − Azınlık payı → = Ana Ortaklık Payına Düşen Net.
BudgetAI: Şirket katkıları tablosu: Her iştirakin kendi para biriminden raporlama para birimine çevrilmiş net katkısı.
BudgetAI: Sahiplik oranı %100''ün altındaki şirketler için ayrı bir "Azınlık payı dökümü" tablosu (ana ortaklık dışı pay ve tutarı).
BudgetAI: "Konsolide bütçeyi aç →" düğmesiyle üretilen konsolide bütçeye doğrudan geçiliyor.
BudgetAI: Konsolidasyon tanımları silinebiliyor (onay istenir).
BudgetAI: Bütçelere şirket kodu (CompanyCode) alanı eklendi — bütçenin hangi şirkete ait olduğu artık açıkça tutuluyor.
BudgetAI: Konsolidasyon tanımı, üyeleri, eliminasyonları ve azınlık payı hesapları için yeni veritabanı tabloları eklendi.
BudgetAI: Müşteri beklentisi: Gelir Tablosu, Nakit Akış Tablosu ve Bilanço''nun BİRLİKTE çalışması. Seçilen bütçe (konsolide bütçeler dahil) için üç tablo birbirine bağlı olarak türetiliyor ve bilanço inşa gereği DENKTİR (Aktif = Pasif + Özkaynak).
BudgetAI: Bütçe seçici — konsolide bütçeler 🏛 simgesiyle ayırt ediliyor.
BudgetAI: Denklik rozeti: Bilanço tuttuğunda "✓ Bilanço denk" yeşil rozeti, tutmadığında farkı gösteren uyarı rozeti.
BudgetAI: Kullanılan varsayımlar tek satırda özetleniyor: Vergi oranı, DSO (alacak tahsil günü), DIO (stok günü), DPO (borç ödeme günü), açılış nakit/duran varlık/borç/özkaynak bakiyeleri.
BudgetAI: "🔗 Tablolar nasıl bağlı" paneli: Net kâr → özkaynak, dönem sonu nakit → bilanço gibi tablolar arası bağlantıları (artikülasyon) açık açık gösteriyor.
BudgetAI: Üç tablo da (Gelir Tablosu, Nakit Akış Tablosu — dolaylı yöntem, Bilanço) dönemsel sütunlarla ve toplam/bakiye sütunuyla yan yana okunabiliyor; ara toplam ve genel toplam satırları vurgulanıyor.
Riverpedia: "River''da bunu nasıl yaparım?" sorusuna, sisteme kaydedilmiş yardım içeriğinden ve modül dokümanlarından beslenen (grounded) bir cevap veren yeni bağımsız asistan ekranı.
Riverpedia: Tek bir soru kutusu + modül filtresi (Tüm River / BudgetAI /Platform — Süreç/Form); Ctrl+Enter ile hızlı gönderim.
Riverpedia: Cevaplar zengin metin (Markdown) olarak, kaynak referanslarıyla (📎 hangi yardım dokümanından geldiği) birlikte gösteriliyor.
Riverpedia: "Son sorular" ve "En çok sorulanlar" (son 90 gün, soru bazlı sayaçla) listeleri — bir soruya tıklamak onu tekrar sorar.
Riverpedia: Bilgi tabanı; platformun yardım içerikleri (süreç adımları ve form alanları için yazılmış yardım metinleri) ile BudgetAI sunum/nasıl-çalışır dokümanından besleniyor.
Riverpedia: Her soru ve cevap, hangi kullanıcı tarafından soruldu bilgisiyle kayıt altına alınıyor (popüler soru istatistikleri buradan gelir).
Datagrıd İyileştirmeleri › Sayfalama: Grid araç çubuğuna (≡) sayfalama açma/kapama düğmesi eklendi: Tek sayfa görünümü ile 50 kayıt/sayfa + alt sayfalama çubuğu (pager) arasında anında geçiş yapılabiliyor.
Datagrıd İyileştirmeleri › Sayısal Kolon: Sayı tipi kolonlara yeni bir kolon ayarı eklendi: Hücrelerdeki yukarı/aşağı (▲▼) artırma-azaltma oklarını gizleme seçeneği.
Form Tasarımcısı: Çoklu seçim kutusunun açılır listesine arama kutusu ve "Tümünü seç / Tümünü bırak" başlık satırı eklendi — uzun listelerde tek tek işaretlemeye gerek kalmadı.
Form Tasarımcısı''nda yeni bir "Seçilenlerin gösterimi" ayarı: · Kompakt: İlk 4 seçim + "+X" özeti (varsayılan). · Genişle: Seçilen tüm değerler alt alta tam olarak gösterilir.
## Hata Düzeltmeleri
BudgetAI: Bir şirketin ilgili dilimde (senaryo/versiyon) bütçesi yoksa açıkça uyarılıyor ve o şirket konsolidasyona dahil edilmiyor.
Datagrıd İyileştirmeleri › Görsel Düzeltmeler: Sabit (sticky) başlık ve dip toplam satırının arka planı artık OPAK: Çok satırlı gridlerde kaydırma sırasında alttan geçen satırların başlık/toplam ile karışması (saydamlık sorunu) giderildi.
Çoklu seçim yapıp "her seçim ayrı satır" olarak eklemede, çip gösterimi artık kendi satırının GERÇEK değerini gösteriyor (önceden yanlış/karışık değer gösterilebiliyordu).
Kaynak grid''de hiçbir satır seçili değilken hedef grid artık boş geliyor (filtrelenmemiş / yanlış veri göstermiyor).
Filtreli bir hedef grid''e yeni satır eklendiğinde, eklenen satır otomatik olarak geçerli filtre değerini alıyor — eklenir eklenmez filtre dışına düşüp kaybolmuyor.
Kaynak grid''de satırın filtre seçiciye tıklamak ("…" düğmesi) yanlışlıkla o satırın filtre durumunu değiştirmiyor (tıklamalar artık birbirine karışmıyor).
Bir kolonun "ilk kaydı otomatik seç" ayarı, veri kaynağı formdaki başka bir alana ({{alan}} gibi bir yer tutucuya) bağlıyken artık doğru çalışıyor: Form değeri değiştiğinde veri yeniden çekilip hücre otomatik dolduruluyor (önceden dışarıdan doldurulan formda bu otomatik seçim çalışmıyordu).
## Güvenlik
Riverpedia: "⚙ Bilgi tabanını güncelle" işlemi yalnızca admin yetkisiyle çalışıyor; çalıştırıldığında kaç kaynak ve kaç parçanın işlendiği bildiriliyor.', N'Ara Sürüm', 1, '2026-07-28 09:00:00', '2026-07-28 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.19')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.19', '2026-08-01', '2026-08-01', N'## Yenilikler
RPA artık tarayıcı dışındaki klasik Windows programlarını da (muhasebe, ERP, bayi/uygulama ekranları) kendisi sürebiliyor — uygulamayı açar, giriş yapar, menüden rapor ekranına gider, filtreleri doldurur ve sonucu getirir.
RPA: Masaüstü akışlarında kullanılabilen adımlar: Uygulamayı başlatma veya zaten açık uygulamaya bağlanma, pencere öne alma, alan doldurma, tıklama (çift/sağ tık dahil), menü yolunda gezinme, listeden seçim, işaret kutusu, tuş kısayolu gönderme, koşul bekleme, tek değer okuma, tablo okuma, dışa aktarma ve dışa aktarılan dosyayı okuma.
RPA: Ekran tablolarını satır satır okuyabiliyor: Rapor ızgarasındaki kolonları seçip verinin tamamını yapılandırılmış şekilde RiverAI''ya alabiliyorsunuz. Programın "Excel/CSV olarak kaydet" penceresini de robot sürüyor ve oluşan dosyayı okuyup içeriğini akışa taşıyor.
RPA: Ekran öğesi keşfi: Daha önce hiç otomasyon yazılmamış bir programda robot, ekrandaki alanların ve düğmelerin listesini kendisi çıkarabiliyor. Alan adlarını deneme-yanılma ile aramak yerine gerçek isimleriyle seçiyorsunuz — eski/bilinmeyen ERP ekranlarında kurulum süresi belirgin şekilde kısalıyor.
RPA: Robot eklerken yeni "Yetenek (çalıştırıcı türü)" seçimi: Web (tarayıcı), Masaüstü (Windows) veya Her ikisi. Masaüstü robotu tanımlamak için teknik destek gerekmiyor.
RPA: Robot listesinde her robotun yetenek etiketleri görünüyor; çevrimiçi robotlarda ayrıca işletim sistemi ve ajan sürümü de yazıyor — hangi makinenin ayakta olduğunu tek bakışta görüyorsunuz.
RPA: Akış tanımlama ekranına yeni "Hedef (çalışma ortamı)" seçimi eklendi: Web (tarayıcı), Masaüstü (Windows), Script. Önceden her akış otomatik olarak web sayılıyordu.
RPA: Hedef olarak "Masaüstü" seçtiğinizde ekranda uyarı çıkıyor: Adımları JSON modunda yazmanız gerektiği, görsel adım paletinin web akışları için olduğu hatırlatılıyor.
RPA: Her adımda ekran görüntüsü alınıyor ve koşu detayında adım adım izlenebiliyor, "film gibi oynat" ile baştan sona seyredilebiliyor. Görüntü artık robotun sürdüğü pencerenin kendi içeriğini yakalıyor: Pencere arkada, başka bir pencerenin altında veya küçültülmüş olsa bile görüntü boş/siyah çıkmıyor.
RPA: Kolay kurulum: Komut satırı kullanımı kısıtlı sunucularda robot ajanı, yanındaki ayar dosyasındaki bilgilerle çift tıklanarak çalıştırılabiliyor.
RPA: Çok büyük rapor çıktılarında koşu artık "Çalışıyor" durumunda asılı kalmıyor; büyük veri işaretlenip iş düzgün şekilde tamamlanıyor.
RPA: Ajanın oturum açılışında otomatik başlaması ve uzak masaüstü bağlantısı koptuğunda ekranın kilitlenmemesi için makinede tek seferlik bir ayar yapılması gerekiyor (kilitli oturumda ekran görüntüsü ve ekran öğesi okuma çalışmaz).
AI Yönetim Paneli: Yapay zekâ özelliklerinin ne kadar kullanıldığını artık tek ekrandan görebiliyorsunuz. Yeni AI Yönetim Paneli kurumunuzun toplam token tüketimini, tahmini maliyetini, hangi kullanıcının ve hangi AI özelliğinin ne kadar harcadığını gösteriyor. Aynı ekranda günlük veya aylık token tavanı tanımlayıp, tavan aşıldığında sistemin uyarmasını ya da yeni AI isteklerini durdurmasını seçebiliyorsunuz.
AI Yönetim Paneli: Panelin üstünde dönem seçimi var: "Bugün", "Bu ay" ve "Son 30 gün". Tüm tablolar ve grafik seçtiğiniz döneme göre yenileniyor.
AI Yönetim Paneli: Dört özet kart: "Toplam token", "Tahmini maliyet" (dolar), "AI çağrısı" sayısı ve "Giriş / Çıkış token" dağılımı.
AI Yönetim Paneli: "Günlük token kullanımı" grafiği: Dönemdeki her günün tüketimini sütun grafik olarak gösteriyor; sütunun üzerine gelince o günün tam token sayısı görünüyor.
AI Yönetim Paneli: "Kullanıcı bazında" tablosu: Hangi kullanıcının kaç token harcadığı, tahmini maliyeti ve kaç AI çağrısı yaptığı; en çok kullanan en üstte, yanında oransal çubuk ile.
AI Yönetim Paneli: "Özellik (AI aracı) bazında" tablosu: AI''ın hangi özellikte ne kadar kullanıldığını aynı kırılımla listeliyor.
AI Yönetim Paneli: Yeni "Kullanım limitleri" bölümü: Tanımlı tüm limitleri kapsam, hedef, dönem, token tavanı, aşılınca ne olacağı ve aktif/kapalı durumu ile tek tabloda gösteriyor.
AI Yönetim Paneli: "+ Yeni limit" ile üç kapsamda tavan tanımlanabiliyor: Tüm kurum geneli, kullanıcı bazında veya belirli bir AI özelliği bazında.
AI Yönetim Paneli: Dönem "Günlük" veya "Aylık" seçiliyor. Sayaç dönem başında (günlük limitte günün başında, aylık limitte ayın 1''inde) sıfırdan saymaya başlıyor.
AI Yönetim Paneli: "Limit aşılınca" davranışı her limit için ayrı: "Sert engelle — yeni AI çağrıları reddedilir" veya "Uyar — çağrı çalışır, sadece kaydedilir".
AI Yönetim Paneli: Limitler tek tek açılıp kapatılabiliyor: "Limit aktif" işaretini kaldırdığınızda kural silinmeden devre dışı kalıyor.
AI Yönetim Paneli: Veri İşleme menüsünden az kullanılan "Form Yanıtları", "Cihaz Geçmişi", "Dispatch (Atama)", "Saha Servisi (mobil)" ve izin formu demo girişleri kaldırıldı; Data Console, Audit ve seed/demo işlemleri yerinde kaldı.
PTT KEP / UETS: EBYS''nin resmi yazışma kanalı artık tek sağlayıcıya bağlı değil. Bugüne kadar KEP gönderimi ve e-Tebligat çekimi yalnızca TÜRKKEP üzerinden yapılabiliyordu; bu sürümle PTT de seçilebilir hâle geldi. Seçim tamamen Ayarlar''dan yapılır; gönderim ekranları, gelen kutusu ve mobil imza akışı ikisinde de aynı görünür. Hâlihazırda TÜRKKEP kullanıyorsanız hiçbir şey değişmez — varsayılan seçim TÜRKKEP kalır.
PTT KEP / UETS: Ayarlar → Entegrasyonlar → KEP sekmesine yeni bir "KEP / UETS Sağlayıcısı" bölümü eklendi.
PTT KEP / UETS: "KEP sağlayıcısı" listesinde iki seçenek: "TÜRKKEP (Entegrasyon Suit)" ve "PTT KEP (E-Yazışma)".
PTT KEP / UETS: "UETS sağlayıcısı" listesinde iki seçenek: "TÜRKKEP (Entegrasyon Suit)" ve "PTT UETS (REST)".
PTT KEP / UETS: İki kanal birbirinden bağımsız seçilebiliyor — örneğin giden KEP yazışmalarını PTT üzerinden, e-Tebligat çekimini TÜRKKEP üzerinden yürütebilirsiniz.
PTT KEP / UETS: PTT seçildiğinde ekranda "PTT — KEP / UETS Kimlikleri" bölümü açılır; TÜRKKEP seçiliyse bu bölüm hiç görünmez, ayar ekranı eskisi gibi kalır.
PTT KEP / UETS: PTT KEP için "Ortam" seçimi: "Test" ve "Canlı". Test ortamındaki gönderimler hukuki sonuç doğurmaz.
PTT KEP / UETS: PTT UETS için "UETS API anahtarı" ve "Ortam" ("Ön-üretim" / "Canlı") alanları eklendi. Anahtarın UETS Panel → API Ayarları''ndan üretildiği ve süreli olduğu ekranda hatırlatılıyor.
PTT KEP / UETS: "Servis URL" ve "UETS Base URL" alanları opsiyonel; boş bırakılırsa seçtiğiniz ortama göre otomatik belirlenir.
PTT KEP / UETS: PTT''ye özel uyarı: Web servis erişimi için sunucunuzun çıkış IP adresinin PTT''ye önceden bildirilmiş olması gerekir.
PTT KEP / UETS: "KEP ile Gönder — Mobil İmza" penceresi PTT''de de birebir aynı çalışır: İmzacı cep telefonunu ve operatörü girip "İmzayı Başlat" düğmesine basar, telefona gelen imza isteğini onaylar, evrak otomatik gönderilir.
PTT KEP / UETS: PTT gönderiminde de tek bir telefon onayı yeterlidir; kullanıcı tarafında ek adım veya ikinci PIN girişi yoktur.
PTT KEP / UETS: Gönderim tamamlandığında evrak durumu "Gönderildi" olur ve açıklamasına "PTT KEP ile gönderildi" notu ile KEP kimliği ve gönderim zamanı işlenir — hangi kanaldan gittiği evrak üzerinden görülebilir.
PTT KEP / UETS: "KEP / UETS Gelen Kutusu" ekranındaki "KEP''ten Çek" ve "UETS''ten Çek" düğmeleri seçtiğiniz sağlayıcıya göre çalışır; ekranın görünümü ve kullanımı değişmedi.
PTT KEP / UETS: "Teslim Delili" düğmesi PTT gönderimlerinde de çalışır. PTT delilleri bir süre gecikmeli üretir; delil henüz hazır değilse "Delil henüz oluşmadı" bilgisi verilir.
PTT KEP / UETS: PTT UETS kullanımında e-Tebligat listesini görüntülemek tebligatı okundu yapmaz; okundu saymayı yalnızca indirme tetikler. Bu nedenle indirme onaya bağlı tutuluyor.
PTT KEP / UETS: UETS tarafında istek limitine takılırsanız sistem üst üste denemek yerine kendi içinde bekler; limit cezasının uzamasına yol açan ısrarlı denemeler engellenir.
KYS: İmha talepleri artık kalıcı bir deftere yazılıyor. Daha önce açtığınız silme talebi sayfayı yenileyince kayboluyordu; artık kalıcı olarak saklanıyor ve geçmişe dönük görüntülenebiliyor.
KYS: İmha gerekçesi zorunlu. Red gerekçesi ve imha yöntemi (güvenli silme, kıyma makinesi vb.) kaydedilip defter tablosunda görünüyor.
KYS: İmha tutanağı PDF olarak üretiliyor: Tutanak numarası, hedef kaydın kod ve adı, konum/klasör, saklama süresi, imha gerekçesi, kararı veren ve icrayı yapan kişiler, tarihler, imha kapsamı, silinen ek dosya sayısı ve üç imza bloğu (talep eden / onaylayan / imha eden).
KYS: Fiziksel arşiv kutusunda kutu kaydı ve içindeki evrak listesi korunuyor, yalnızca durumu "İmha edildi" oluyor — çünkü o liste neyin imha edildiğinin kanıtı.
KYS: Hedef kayıt daha önce başka bir yolla silinmişse tutanakta bunu belirten bir uyarı basılıyor; hiçbir şey silmemiş bir icra, gerçek imhadan ayırt edilebiliyor.
KYS: Aynı kayıt için iki kez imha talebi açılamıyor; imha edilmiş bir kayıt için ikinci tutanak üretilemiyor. Reddedilen talepten sonra yeni talep açmak ise serbest.
KYS: Saklama süresi dolan kayıtlar için var olan otomatik takip artık ileriye taşınabiliyor: "İmha bekliyor" durumuna düşen kayıt bu defter üzerinden sonuçlandırılabiliyor.
KYS: Sekme 2''deki (Saklama ve imha takibi) "İmha planla" düğmesi çalışır hâle geldi — daha önce "sonraki fazda" uyarısı veriyordu. Açık talebi olan kayıtta düğme kapanıp "Talep açık" rozeti çıkıyor.
KYS: Arama ekranının "Klasör" ve "Filtreli" sekmeleri artık KYS kalite kayıt defterinde arıyor. Sonuç tablosundaki bilgiler gerçek kalite verisi.
KYS: "Doküman kodu" artık gerçek KYS kodu — eskiden çoğunlukla boş görünüyordu.
KYS: Yeni "Durum" rozeti: Yürürlükte / Onay Bekliyor / Revizyonda /Askıda / Arşivlendi / İptal.
KYS: "Yayın tarihi", "Gizlilik sınıfı" ve "Revize eden" kolonları gerçek verilerle dolduruldu — bunlar boş çizgi olarak duruyordu.
KYS: Revizyon geçmişi zenginleşti: Her revizyonun onaylayanı ve revizyon nedeni artık görünüyor. Bu bilgi sistemde vardı ama ekranda hiç gösterilmiyordu.
KYS: Filtreli aramada doküman türü, tipi ve durumu gerçek filtre olarak çalışıyor. Seçenekler kendi verinizden geliyor; sistemde karşılığı olmayan seçenekler listeden kalktı.
KYS: Ayarlar ve Genel Tanımlar ekranları çalışır hâle getirildi: 54 tanım kategorisi (doküman tipleri, türleri, gizlilik sınıfları, departmanlar, ürünler, standartlar, denetim soruları, şikayet kanalları, saklama süreleri vb.) tek bir düzenden yönetiliyor.
KYS: Her tanım listesine kayıt ekleyip düzenleyebiliyor, sıralayabiliyor ve pasife alabiliyorsunuz.
KYS: Tanımlar artık gerçekten tanımlıyor: Ekranlardaki seçim kutuları elle yazılmış sabit listeler yerine Genel Tanımlar''dan besleniyor. Örneğin Doküman Hazırlama''daki ürün seçimi, Ürün Tanımları''na eklediğiniz kayıtları anında gösteriyor.
KYS: Ayarlar ve Genel Tanımlar yalnızca yöneticilere görünür.
KYS: DİF, denetim, şikayet, arşiv kutusu, değişiklik talebi ve imha talebi numaraları artık kurum içinde benzersiz ve eşzamanlı işlemlerde bile çakışmıyor. Daha önce bir kayıt silindiğinde sonraki kayıt aynı numarayı alabiliyordu.
Kanban: Onayda verilen 1-5 puanlar ve onaycı notu ("⭐ Puanlayarak onayla") artık yalnızca pano yöneticisine görünüyor; panoyu görebilen diğer kullanıcılar bu puanlara erişemiyor — arşivlenmiş kartlar dahil.
Kanban: İptal kararı bekleyen ya da iptal edilmiş bir görev artık tamamlanamıyor ve tamamlanması geri alınamıyor. Tamamlama kutusu bu durumda kapalı; üzerine gelindiğinde "İptal kararı bekliyor" veya "İptal edildi" yazıyor. Alt görevler için de aynı kural geçerli.
Kanban: Görevi tamamlayan kişi artık kendi tamamladığı iş için "Tamamlandı" bildirimi almıyor.
Kanban: Ana ekrandaki pano kartında "Tüm kartlar" düğmesi artık çalışıyor: Pano ekranında açık bırakılmış "Sadece benim işlerim" tercihi ana ekranı etkilemiyor.
Kanban: Eski kayıtlı filtre profillerini uygulamak artık gizlediğiniz/daralttığınız kolonları ve kapsam seçiminizi sıfırlamıyor. Ayrıca aktif profil vurgusu doğru profili işaretliyor.
Bildirim zilinde artık üç sekme var: Süreç, Pano ve yeni eklenen İntranet.
Zilin üzerinde her kategori için ayrı renkli sayaç görünüyor (Süreç kırmızı, Pano mavi, İntranet yeşil) ve okunmamış bildirim oldukça hafifçe yanıp sönüyor.
İntranet bildirimine tıklayınca doğrudan ilgili gönderiye gidiyor ve gönderi kısa süre çerçeveyle vurgulanıyor.
Form Sosyal''de gönderinize yorum yapıldığında, tepki verildiğinde ya da bir gönderi/yorumda sizden @ ile bahsedildiğinde zilinize bildirim düşüyor.
@ ile kişi etiketlerken artık kullanıcı adı yerine kişinin adı soyadı yazılıyor ve metin içinde renkli görünüyor.
@ ile kişi ararken açılan öneri listesi artık widget kutusunun kenarında kesilmiyor, tamamı görünüyor.
Doğum günü / yeni katılan kartında emoji tepki verdiğinizde veya kutlama mesajı yazdığınızda, kutladığınız kişinin ziline kişisel bildirim düşüyor.
Widget Yönetimi''ne "Medya onaya gitsin" seçeneği eklendi: Açıkken sosyal duvarda yalnız foto/video içeren paylaşımlar yayınlanmadan onaya düşer, düz metin paylaşımlar serbest kalır.
Anket oluşturma/düzenleme ekranına "Kimler görebilir?" alanı eklendi — anketi belirli kullanıcı ve/veya gruplara özel yapabiliyorsunuz; boş bırakılırsa herkese açık kalıyor.
Etkinlik detayında "Takvimime ekle" düğmesi, Outlook hesabınız bağlıysa etkinliği doğrudan takviminize yazıyor; bağlı değilse eskisi gibi takvim dosyası indiriyor.
Yeni "Yük Testi" ekranı (Ayarlar menüsünden açılır, yalnızca yöneticiler görür): Seçtiğiniz bir süreci aynı anda çok sayıda sanal kullanıcıyla çalıştırıp sistemin nasıl davrandığını ölçüyor.
Senaryoda hedef süreç, eşzamanlı kullanıcı sayısı, test süresi, kullanıcıların devreye giriş biçimi (anında / doğrusal / kademeli), bekleme süreleri ve gönderilecek örnek kayıt şablonu tanımlanıyor.
Varsayılan açık gelen "Sandbox" seçeneği, test sırasında gerçek e-posta, bildirim, KEP ve dış servis çağrılarının gönderilmesini engelliyor; kapatırsanız kırmızı uyarı gösteriliyor.
İçe aktarma penceresine "Kaynak ürün" seçimi eklendi — hangi eski uygulamadan aktardığınızı seçiyorsunuz, eşleştirme sözlüğü ona göre çalışıyor.
Sabit dosya kutuları yerine "Materyaller" alanına istediğiniz kadar dosyayı topluca bırakabiliyorsunuz (kod, şartname belgesi, ekran görüntüsü, PDF, Visio, zip); her dosyanın ne olduğu otomatik anlaşılıyor. Analiz dökümanı olarak .docx ve .pdf de kabul ediliyor.
Güven yüzdesinin altında kırılım paneli açıldı: Puanı hangi etkenlerin hangi ağırlıkla oluşturduğu, kısa gerekçesiyle satır satır gösteriliyor.
Eşleştirme listesi ikiye ayrıldı: "Eşleşmeyen — tip karşılığı yok" ve "Gözden geçir — tipi eşlendi, emin değil". İkinci listede her satırın neden gözden geçirilmesi gerektiği yazıyor.
Tip seçme listelerinde teknik ad da görünüyor (ör. "sqlTask · SQL") ve arama hem teknik ad hem Türkçe karşılık üzerinden çalışıyor.
Eşleştirme yaptıkça "kaç tanesinin kalıcı olarak öğrenileceği" satır sayacında gösteriliyor.
Analiz raporu artık tamamen kaydırılabiliyor; iki eşleştirme tablosu birlikte açıkken alt kısım kırpılıp erişilemez hâle gelmiyor.
Gösterge panosu tablolarında bir kolonu tıklanabilir bağlantı olarak gösterebiliyorsunuz; bağlantı yeni sekmede açılıyor, adresi boş olan hücrede ölü bağlantı yerine "—" görünüyor.
Gösterge panosu tablolarına satır filtresi eklendi: Aynı veri kaynağını farklı widget''larda belirli bir alan değerine göre alt kümeye daraltabiliyorsunuz.
İç içe gelen alanlar (özel alan listeleri, envanter alanları) artık tabloda düz kolon olarak gösterilebiliyor.
REST veri aktarımı artık istenirse her satırı kaydın detay servisinden zenginleştirebiliyor (ör. Kaydın kendi ekranına giden bağlantıyı çekip tabloya eklemek); aktarım özetinde "... Zenginleştirildi" olarak raporlanıyor.
## İyileştirmeler
KYS: Arama belirgin şekilde hızlandı (arama indeksi artık gerçekten kullanılıyor).
KYS: Doküman kartı yenilendi: Durum, tür/tip, gizlilik, sahip, departman, hazırlayan, revize eden ve yayın tarihi tek bakışta; içeriği PDF olarak indirme.
Doküman yönetimi ekranlarındaki düğmeler yenilendi: Belirgin yükseklik, gölge ve üzerine gelince hafif kalkma efekti — eskiden düz birer etiket gibi görünüyorlardı.
## Hata Düzeltmeleri
RPA: Masaüstü robotu şirketin kendi Windows makinesinde çalışıp RiverAI''ya kendisi bağlanıyor; şirket ağına dışarıdan bağlantı açmaya veya rapor için uzak masaüstüne girmeye gerek kalmıyor.
RPA: Windows 11 uyumu: Mağaza/paketli olarak gelen uygulamalarda (ör. Not Defteri) robot uygulamayı açtığında artık doğru pencereye bağlanıyor — daha önce "ana pencere elde edilemedi" hatasıyla akış başlamıyordu.
RPA: Anlaşılır başlangıç hataları: Ajan bağlanamazsa pencere hemen kapanmıyor; sunucu adresine erişilemediği, kimlik/anahtar bilgisinin reddedildiği veya makine saatinin kaydığı sebebiyle birlikte ekranda kalıyor.
RPA: Aynı robot kimliğiyle ikinci bir ajan başlatılırsa uyarı verip kapanıyor; yanlışlıkla çift çalıştırma yüzünden işlerin karışması engelleniyor.
RPA: Robotun kendi açtığı uygulama iş bitince kapatılıyor, arkada süreç bırakmıyor; kullanıcının sürekli açık tuttuğu bir uygulamaya bağlanıldığında ise uygulamaya dokunulmuyor.
RPA: Masaüstü robot ajanı ayrı bir kurulum paketi olarak dağıtılıyor ve yalnızca Windows makinede çalışıyor; sunucu tarafında değişiklik gerekmiyor.
AI Yönetim Paneli: Ürün içindeki tüm yapay zekâ özellikleri otomatik olarak izleniyor; herhangi bir özellik için ayrıca kurulum veya açma işlemi yapmanız gerekmiyor.
AI Yönetim Paneli: Sert limitte tavan dolduğunda AI isteği hiç gönderilmiyor; kullanıcı hangi limitin aşıldığını, kaç token kullanıldığını ve tavanı belirten anlaşılır bir mesaj görüyor.
AI Yönetim Paneli: Kullanıcı bazlı limit tanımlarken listede yalnızca gerçekten AI kullanımı olan kişiler çıkıyor; uzun kullanıcı listesinde arama yapmanız gerekmiyor.
AI Yönetim Paneli: Kayıt veya kota sorgusunda bir sorun olursa AI çağrısı engellenmiyor — yalnızca sert limit aşımı çağrıyı durduruyor.
PTT KEP / UETS: Telefonda onaylanan imza PTT''ye iletilmeden önce sistem içinde doğrulanır; bozuk veya uyumsuz bir imza tespit edilirse gönderim yapılmaz ve hata olarak bildirilir.
PTT KEP / UETS: "Canlı" ortam seçiliyken gerçek gönderim ek bir emniyet kilidi arkasındadır — yanlış ortama bağlıyken kazara gerçek tebligat/KEP gönderilmesi engellenir.
KYS: Görev ayrılığı: Kendi açtığınız imha talebine siz karar veremiyorsunuz. Onay/Reddet düğmeleri kapalı geliyor ve üzerine gelince sebebi yazıyor.
KYS: Elektronik kayıt imhasında kaydın ek dosyaları ve Word dokümanı da imha kapsamına alınıyor — tutanak, gerçekte silinmemiş bir şeyi silinmiş göstermiyor.
KYS: Yayınlanmış bir dokümana bağlı kayıt bu defterden imha edilemiyor; sistem sizi Revizyon/İptal ekranına yönlendiriyor. Böylece yürürlükteki doküman ayakta kalırken "imha edildi" tutanağı üretilmesi engelleniyor.
KYS: Doğrudan silme yan kapısı kapatıldı: "İmha et" düğmesi artık kaydı tek tıkla silmek yerine imha talebi açıyor.
KYS: "Revizyon numarası" artık her satırda dolu — eskiden arama sonuçlarında hiç görünmüyordu.
KYS: Filtreler büyük/küçük harf ayırmıyor — daha önce bazı kayıtlar filtreye takılmadan sessizce atlanabiliyordu.
KYS: "Serbest metin" ve "Anlamsal" sekmeleri dosya içeriğinde aramaya devam ediyor (taranmış/OCR belgeler dahil) ve hangi arşivde aradıkları ekranda açıkça yazıyor. Böylece "kod/revizyon ile bul" ile "içinde şu yazan dosyayı bul" ihtiyaçları karışmıyor.
KYS: Doküman dosyalarına bağlantısını bilen herkesin erişebildiği açık yol kapatıldı. Doküman dosyaları artık yalnızca süreli ve o dosyaya özel bir bağlantıyla açılıyor; başka bir dosya için kullanılamıyor. Profil fotoğrafı ve pano ekleri gibi genel görseller eskisi gibi çalışıyor.
KYS: PDF çıktılarındaki Türkçe büyük harf hatası düzeltildi: Başlıklar "İMHA İCRASı" ve "ETKILENEN DOKÜMANLAR" gibi yanlış yazılıyordu, artık "İMHA İCRASI" ve "ETKİLENEN DOKÜMANLAR". Düzeltme tüm KYS raporlarını kapsıyor.
Kanban: Bekleyen iptal talebi artık kaybolmuyor. Önceden görevi tamamlamak veya tamamlamayı geri almak iptal talebini karar verilmeden siliyordu; iptal gerekçesi ve iptal öncesi atananlar kalıcı olarak kayboluyor, iptal kararı bir daha verilemiyordu.
Kanban: Aynı görev için ikinci bir iptal talebi açılamıyor: "Bu kart zaten iptal edildi." uyarısı veriliyor.
Kanban: Tamamlama onayı, yönlendirme onayı veya iptal kararı beklenirken yeni bir yönlendirme talebi açılamıyor; sistem hangi kararın beklendiğini açıkça söylüyor. Önceden bekleyen onay sessizce yok oluyordu.
Kanban: Kartı açan kullanıcı bulunamadığında iptal akışı tıkanmıyor: İptal kararı pano yöneticilerine, yönetici yoksa pano sahibine düşüyor ve bildirimde bunun nedeni yazıyor. Eskiden bu kartlar için iptal hiç kimse tarafından yürütülemiyordu.
Kanban: Sprint bitiş hatırlatması gruba/role atanmış kartlarda üyelere gönderiliyor. Grup henüz boşsa hatırlatma "gönderildi" sayılmıyor; grup doldurulduğunda sprint bitmeden uyarı yine gidiyor.
Kanban: Geciken görev uyarılarında "kaç gün geriye bakılacağı" ayarı artık ilgili panonun kendi ayarına uyuyor. Önceden en geniş ayar bütün panolara uygulandığı için pano bazlı ayar fiilen çalışmıyordu.
Kanban: Gecikme uyarısı gönderilirken hata oluştuysa bir sonraki turda yeniden deneniyor; uyarı sessizce ve kalıcı olarak kaybolmuyor.
Kanban: CSV dışa aktarma ve "panoya kopyala" güvenliği: Bir kart başlığına veya metin alanına = + - @ ile başlayan içerik yazılarak, dışa aktarılan dosyayı açan kişide Excel/Google Sheets üzerinden formül/komut çalıştırılması engellendi. Bu içerikler artık düz metin olarak açılıyor. Hem analitik dışa aktarımı, hem panoya kopyalama, hem pano CSV''si kapsamda.
Kanban: Kart kaydederken başlık boş kalırsa anlaşılır "Kart başlığı zorunludur." uyarısı görünüyor; teknik hata ayrıntısı gösterilmiyor.
İntranet, Bildirimler, Yük Testi ve İçe Aktarma: Bildirim zili İntranet bildirimlerini de kendi sekmesinde toplamaya başladı; sosyal duvarda yorum, tepki ve @bahsetme bildirimleri, doğum günü ve yeni katılan kutlamaları devreye girdi. Yöneticiler için bir süreci onlarca sanal kullanıcıyla test edip yanıt süresi, hata oranı ve CPU/RAM değerlerini canlı izleyebileceğiniz yeni bir Yük Testi ekranı eklendi. Eski uygulamalardan süreç/form aktarma penceresi de yenilendi.
Doğum günü olan ve son 30 günde işe başlayan arkadaşlar için herkese bildirim gidiyor ("Bugün ...''in doğum günü!", "... Aramıza katıldı!"); aynı kutlama tekrar gönderilmiyor.
Kullanıcı/grup hedeflemesinde seçili gruplar artık her zaman kaldırılabilir etiket olarak görünüyor; grup listesi geç yüklendiğinde veya grup silindiğinde seçim görünmez şekilde takılı kalmıyor.
Takviminize eklediğiniz etkinlikte düğme "Takvimden sil"e dönüşüyor, böylece aynı etkinliği ikinci kez eklemiyorsunuz.
Test çalışırken saniyedeki işlem sayısı, yanıt süresi, hata yüzdesi, aktif kullanıcı, CPU ve RAM canlı kartlar ve zaman grafikleriyle izleniyor; "Durdur" ile test yarıda kesilebiliyor.
Test bitince yanıt süresi dilimleri, en yavaş ve ortalama süre, toplam/başarılı/hatalı istek sayısı, tepe CPU-RAM ve hata dağılımı özet tablosu çıkıyor; tüm koşular "Koşu geçmişi" listesinde saklanıyor.
Pencere içindeki aramalı seçim kutuları artık açıldığında pencerenin arkasında kaybolmuyor — "combo açılmıyor" şikâyeti giderildi.
## Güvenlik
RPA: Akış içindeki şifre alanları çalışma kayıtlarında maskelenerek gösteriliyor.
AI Yönetim Paneli: Yeni ekran: "AI Yönetim Paneli". 4 nokta (Ayarlar) menüsündeki Yönetim bölümünden 🤖 "AI Yönetim Paneli" ile açılıyor. Yalnızca yönetici yetkisi olanlar görebiliyor.
PTT KEP / UETS: PTT KEP kimliği için dört alan: "KEP hesap adresi", "Yetkili TC Kimlik No", "Parola" ve "Şifre". Parola ve şifre PTT tarafında birbirinden farklı iki bilgidir; ekranda hangisinin birinci hangisinin ikinci sır olduğu ipucu olarak yazılıdır.
PTT KEP / UETS: TC Kimlik No, parola, şifre ve UETS anahtarı kayıtlıysa alanlar "(saklı)" görünür; bunları yeniden yazmadan diğer ayarları değiştirip kaydedebilirsiniz.
PTT KEP / UETS: Sistem günlüklerinde parola, şifre, TC Kimlik No ve imza verileri maskelenerek yazılır; bu bilgiler günlüklerde açık görünmez.
KYS tarafında bu sürümde üç ana iş var: Saklama süresi dolan kayıtların imhası artık talep-onay-icra akışıyla yürüyor ve imza bloklu bir imha tutanağı üretiyor; doküman arama ekranı kalite verisinin kendisine bağlandı (kod, revizyon, durum artık gerçek); Ayarlar ve Genel Tanımlar ekranları çalışır hâle getirilip diğer ekranların seçim listelerini beslemeye başladı. Ayrıca yetki ve gizlilik tarafında birkaç önemli boşluk kapatıldı.
KYS: Akış netleşti: Talep → yetkili onayı → imha icrası → imha tutanağı. Onay ile fiili imha ayrı adımlar; onaydan sonra imha ayrıca uygulanıyor, böylece bir aksaklıkta defter yanlış bilgi göstermiyor.
KYS: Sistem sıfırdan kurulabilen, tüm alanları dolu bir örnek veri seti ile geliyor: Klasör ağacı, dokümanlar, revizyonlar, DİF kayıtları, denetimler, şikayetler, arşiv kutuları ve yetki matrisleri. Sunumlarda boş ekran kalmıyor.
KYS: Yetki grubuna göre menü ve kart görünürlüğü eklendi: Bir kullanıcının yetkisi olmayan ekran artık ona hiç gösterilmiyor. Girip hata alacağı boş ekranlarla uğraşmıyor.
KYS: Yetkisiz erişimin son kapısı kapatıldı: Bir kullanıcı, listede görmediği bir dokümanı doğrudan bağlantıyla ya da kodunu bilerek de açamıyor. Daha önce liste süzülüyordu ama tek doküman açılışı süzülmüyordu.
Kanban panolarında yetkiler, görevin yaşam döngüsü (tamamlama / iptal /yönlendirme) ve bildirimler baştan sona gözden geçirildi. Artık salt-okunur eklenen kişi gerçekten sadece görüyor, onay puanları yalnızca pano yöneticisine açık, iptal kararı bekleyen bir görev karar verilmeden tamamlanıp ezilemiyor. Gruba veya role atanan görevlerin bildirimleri ilk kez gerçekten üyelere ulaşıyor.
Kanban: "👁 Salt-okunur kullanıcılar (görür ama düzenleyemez)" listesine eklenen kişi artık gerçekten sadece görüyor: Kart içeriğini düzenleyemiyor ve kartın atamalarını da değiştiremiyor. Önceden bu kişi kendini salt-okunur listesinden çıkarıp kendisine tam düzenleme yetkisi verebiliyordu.
Kanban: Kart "düzenlenebilir" görünüp kaydetmeye çalışınca "yetkiniz yok" hatası alma durumu giderildi: Salt-okunur kullanıcıya baştan doğru gerekçe gösteriliyor ve boşuna "atamayı devret" düğmesi açılmıyor.
Kanban: Tamamlanmış ve onaylanmış bir görevi iptale göndermek artık onay yetkisi istiyor. Onaylı iş, onaycıdan geçmeden iptale sürüklenip onayı geçersiz kılamıyor.
Gruba göre kısıtlanan intranet içerikleri (haber, duyuru, etkinlik, anket) ile pano/proje/doküman yetkileri artık ilk girişten itibaren doğru uygulanıyor.
## Kaldırılanlar
KYS: İşe yaramayan iki öğe kaldırıldı: Hiçbir etkisi olmayan "OCR belgeleri dahil et" seçeneği ve yanıltıcı alaka yüzdesi (anlamsal aramada her sonuç "%2 benzerlik" gösteriyordu) — yerine sonuç sırası yazıyor.
Kanban: Gruba veya role atanmış görevlerin bildirimleri artık grubun/rolün üyelerine ulaşıyor: "Tamamlandı", "Sana yönlendirildi" ve "Atamanız kaldırıldı" bildirimleri (ve e-postaları) daha önce hiç kimseye gitmiyordu.', N'Ara Sürüm', 1, '2026-08-01 09:00:00', '2026-08-01 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.20')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.20', '2026-08-05', '2026-08-05', N'## Yenilikler
Depo konum omurgası: Depo → koridor → raf → göz hiyerarşisi, kapasite bilgisi ve depo krokisi. Konumlar ağacı ve görsel Depo Haritası ekranları eklendi.
Stok artık konum bazlı: Stok listesinde hangi gözde durduğu görünüyor ve göze giriş yapılabiliyor.
Görev motoru + Operatör Modu: Toplama/yerleştirme görevleri kuyruğa düşüyor, depo personeli kendi ekranından sırayla işliyor.
Karekod etiket üretimi: Palet ve göz etiketleri QR kodlu olarak basılabiliyor.
Palet (LPN) ve paketleme: Toplama → paketleme → sevkiyat turu tamamlandı; her palet izlenebiliyor.
Sevkiyat, toplama dalgası ve FEFO tahsisi: Son kullanma tarihi en yakın olan parti önce çıkıyor.
Avlu yönetimi: Rampa (dock) ve park cebi panosu — hangi aracın hangi rampada olduğu GPS gerektirmeden takip ediliyor.
WMS raporları + kokpit göstergeleri ve rapor özetlerinin yapay zekâ ile yazıya dökülmesi.
Malzeme Kartı''na Satın Alma ve Satış fiyat sekmeleri eklendi; iki taraf çakıştığında uyarı veren kural tanımlandı.
Tedarikçi alt formları (Ön Yeterlilik, Durum, Self-Servis) ve Teklif Kıyaslama (TCO) formu yeni panel düzenine geçti; katalog formlarına kaydet düğmesi eklendi.
Tedarikçi seçimleri arama pencereli seçime çevrildi; tablolarda tarih biçimi gg.aa.yyyy oldu.
Yeni tarama ajanı: Ağdaki tarayıcılardan (eSCL destekli) doğrudan tarama alıp RiverAI''ya aktarabiliyor.
Masaüstü robotu raporlarını REST üzerinden gönderebiliyor.
Masraf yönetimi eğitim demosu ve kalabalık veriyle çalışan "yoğun demo" kurulumu; canlı sunum için durak durak senaryo kılavuzu.
KYS: Bir klasörün tüm alt klasör yapısı tek işlemle başka bir yere kopyalanabiliyor. Benzer yapıdaki yeni bir departman/şube için ağacı elle yeniden kurmaya gerek yok.
KYS: Doküman listesinde her satır için "Detay" hücresi geldi; açılan panelde künye, ön izleme, tarihçe ve revizyon bilgileri ayrı sekmelerde görünüyor.
KYS: Doküman içeriği indirmeye gerek kalmadan ekranda açılıyor.
KYS: Görüntüleme işlemi denetim kaydına yazılıyor — bir dokümanı kimin ne zaman görüntülediği izlenebiliyor.
KYS: Doküman hazırlarken "ilgili doküman" alanı artık serbest metin değil, gerçek doküman arayan bir seçici. Seçilen doküman kayıtla ilişkilendiriliyor ve yalnızca kullanıcının görme yetkisi olan dokümanlar listeleniyor.
KYS: Doküman hazırlama ekranındaki "Word''de aç", şablondan bir Word dosyası üretip tarayıcı içinde düzenlemeye açıyor. Bu özelliğin çalışmamasının tek sebebi gereken doküman sunucusunun kurulu olmamasıydı; artık kurulum paketinin içinde geliyor.
KYS: Word/Excel/PowerPoint dosyaları indirilmeden ekranda ön izlenebiliyor.
KYS: Doküman şablonu antetli olarak indirilebiliyor, masaüstündeki Word ile doldurulup aynı kayda geri yüklenebiliyor.
KYS: DİF süreci artık uçtan uca ekranda yürütülüyor: Uygunsuzluk → Çözüm Ekibi → Gelişme Raporu → Kök Neden Analizi → Aksiyonlar → Kapatma İncelemesi → Sonuç. Eskiden yedi aşamanın yalnızca üçünün alanı vardı; ekip, gelişme, kapatma ve sonuç aşamalarında doldurulacak hiçbir alan yoktu.
KYS: Sürecin iki çatalı da çalışıyor: "aksiyona ihtiyaç yok" denirse kök neden ve aksiyon adımları atlanıyor; kapatma incelemesinde karar olumsuzsa süreç kök neden analizine geri gönderiliyor.
KYS: Aşama atlamak artık mümkün değil. Eski "DİF''i kapat" düğmesi yeni açılmış bir DİF''i tek tıkla kapatabiliyor, kapatma incelemesini ve etkinlik doğrulama kilidini atlıyordu.
KYS: Kök neden analizi üç yöntemle yapılabiliyor (5 Neden, Balık Kılçığı, 5N1K) ve her aksiyon bir kök nedene bağlanabiliyor.
KYS: Her aşamanın zorunlu alanları var; eksik alan varsa hangilerinin eksik olduğu tek tek yazılıyor.
KYS: DİF numarası ilk kaydetmede kesinleşiyor; ekranı açıp vazgeçmek artık numara harcamıyor.
KYS: Detaylı filtre: Durum, aşama, faaliyet türü, öncelik, işlem kaynağı, sorumlu, açan, ekip lideri, bölüm, iş yeri, kategori ve üç ayrı tarih aralığı ile süzme. Etkin filtreler liste üstünde çip olarak görünüyor ve tek tek kaldırılabiliyor — "neden bu kadar az kayıt görüyorum" sorusu ortadan kalktı.
KYS: Çoklu satır seçimi ve toplu silme; tek onayla birden fazla kayıt silinebiliyor, silinemeyen kayıtlar ayrıca bildiriliyor.
KYS: Master liste penceresi: Tüm DİF''lerin özet dökümü ve istatistikleri.
KYS: Kaydedilen bazı alanlar tekrar açıldığında boş geliyordu; kaydet ↔ yükle turu tamamlandı.
KYS: Etkinlik kontrol tarihi ve doğrulama işareti her kayıt işleminde sıfırlanıyordu; artık yalnızca o alanlara dokunulduğunda yazılıyor.
KYS: Üç ayrı yerde farklı hesaplanan "açık DİF" sayısı tek hesaba indirildi.
KYS: Aşama ilerledikten sonra ekran eski adımda kalıyordu; artık kendiliğinden yeni adıma geçiyor.
KYS: Doküman, klasör, ek dosya ve kalite kayıtları artık kalıcı olarak silinmiyor; silinen her şey Çöp Kutusu''na düşüyor ve geri alınabiliyor.
KYS: Yeni Çöp Kutusu ekranı: Silinen kayıtlar, kim ne zaman sildi bilgisi ve "Geri al" işlemi.
KYS: Ek dosyalar silindiğinde dosyanın kendisi de depodan siliniyordu; artık korunuyor.
KYS: Onay görevleri artık Kalite Kokpiti''ne düşüyor; onay bekleyen işi görmek için ilgili ekranı tek tek gezmek gerekmiyor.
KYS: Görev satırında hangi modülden geldiği (doküman, DİF, değişiklik talebi, şikayet) görünüyor.
KYS: Görev detay paneli: 3 sekmede görev bilgileri, geçmiş kararlar ve tarihçe.
KYS: Aşama şeridi artık gerçek durumu gösteriyor; eskiden tamamlanmış gibi görünen adımlar aslında hiç gerçekleşmemiş olabiliyordu.
KYS: İmha süreci onay isteyen bir akış hâline geldi: Talep eden kişi kendi talebini onaylayamıyor ve imhayı kendisi yürütemiyor (görevler ayrılığı).
KYS: Doküman hazırlarken işaretlenen "indirilemez" ve "yazdırılamaz" kuralları artık gerçekten uygulanıyor; eskiden kaydediliyor ama hiçbir yerde denetlenmiyordu.
KYS: Ayarlar ve Genel Tanımlar: Kaynak, departman, iş yeri, kategori gibi kataloglar tek yerden yönetiliyor; DİF ve doküman ekranlarındaki seçim listeleri buradan besleniyor.
KYS: Askıya alma ekranı: Yürürlükteki bir dokümanı geçici olarak askıya alma / yeniden yürürlüğe koyma; aktif ve pasif dokümanların özeti, sekmeli görünüm ve gerekçe kaydı.
KYS: Revizyon ve iptal ekranı: Revizyon önizlemesi, alanların otomatik dolması ve gerçek iptal işlemi çalışıyor.
KYS: Müşteri madde listesi ekranı (15 kalem).
KYS: Kalite kayıtlarının numaraları artık her kiracıda kendi sayacından ve çakışmasız üretiliyor.
Arayüz: KYS''ye kalıcı bir yan menü geldi; alt modüller arasında geçerken menü yerinde kalıyor.
Arayüz: Sekmeler artık uygulama gibi davranıyor: Açık bir sekmeye tekrar tıklamak onu kapatıp ana ekrana dönüyor.
Arayüz: Sekme açılışında macOS tarzı bir açılış animasyonu var — ekran sekmenin kendi konumundan büyüyerek açılıyor, kapanırken aynı yere geri toplanıyor. Animasyon çerçevesinin çevresinde hafif bir gölge var ve pencere tamamen açıldığında kayboluyor.
Arayüz: Bölünmüş görünümde animasyon devre dışı — iki ekran arasında sık geçiş yapıldığında rahatsız etmemesi için.
Arayüz: KYS pencereleri modül rengini otomatik alıyor; 36 pencerenin tamamı aynı görünümde.
Kurulum ve Sürüm Paketi: Word/Office özelliklerinin çalışması için gereken doküman sunucusu kurulum paketine eklendi. Sunucu sürümü sabitlendi ki aynı paket aylar sonra farklı bir sürüm çekmesin.
Kurulum ve Sürüm Paketi: Giriş ekranı teması kurulum paketine dahil edildi; kurulumdan sonra "Kurum" listesi ilk açılışta çalışıyor.
Kurulum ve Sürüm Paketi: Yüklenen dokümanlar için kalıcı depolama alanı tanımlandı. Kurulumcunun ürettiği yapılandırmada bu tanım yoktu; sürüm güncellemesi yüklenen tüm dokümanları siliyordu.
## İyileştirmeler
Diğer Modüller: Doküman yönetiminde düzenleyici ekranı sadeleştirildi ve salt okunur modda gerçekten salt okunur çalışıyor.
Diğer Modüller: Doküman yönetimi ekranlarında arama ve gezinme iyileştirmeleri.
## Hata Düzeltmeleri
Kalite tutması (karantina/hurda): Karantinaya veya hurdaya alınan stok kullanılabilir sayılmıyor, sevkiyata ve üretime tahsis edilemiyor.
Sevkiyat ekranı, paketleme çalışmasıyla sunucuya eklenen alanları (palet sayısı, paketlenen miktar) okuyor ama arayüz tarafındaki tanım güncellenmemişti; bu yüzden uygulama paketi hiç üretilemiyordu. Tanım sunucunun gerçek şekline hizalandı, ekran koduna dokunulmadı. Aşağıdaki 1-12. Bölümler bu turda KYS tarafında yapılan işlerdir.
KYS: Bir alt klasörün kodu, üstündeki klasörlerin kodlarından oluşan tam zincir olarak üretiliyor (ör. "KYS.01.03"). Eskiden yalnızca kendi kısa kodu yazılıyor, aynı kodun ağacın iki ayrı yerinde tekrar etmesi ayırt edilemiyordu.
KYS: Kod önizlemesi klasörü kaydetmeden önce ekranda görünüyor; kaydettikten sonra sürpriz bir kodla karşılaşmıyorsunuz.
KYS: Doküman numarası sayacı düzeltildi: Aynı klasöre eklenen ikinci doküman artık doğru sırada numara alıyor, eski kayıtlar da geri dolduruldu.
KYS: Bir dokümanın metne çevrilmiş (aranabilir) olup olmadığı listede, aramada ve detay panelinde aynı rozetle görünüyor. Eskiden bu bilgi hiçbir yerde yoktu ve "neden bu doküman aramada çıkmıyor" sorusunun cevabı bulunamıyordu.
KYS: Bir dokümanın iki sürümü arasındaki farkı gösteren panel onarıldı.
KYS: Reklam engelleyici kullanan tarayıcılarda editörün boş açılmasına yol açan sorun için kurulum sırasında otomatik bir uyum yaması uygulanıyor.
KYS: Yürürlükteki bir doküman ise içerik değişimine karşı kilitli — yayınlanmış bir metin farkında olmadan değiştirilemiyor.
KYS: Kısa konu, sorumlu ve öncelik alanları da 1. Adımda düzenlenebiliyor. Bunlar eskiden yalnızca açılış formunda giriliyor ve sonradan hiçbir ekrandan değiştirilemiyordu.
KYS: İşlem kaynağı düzenlenebilir oldu; kaynağı boş kalmış bir kayıt artık süreçte tıkanmıyor.
KYS: Filtre penceresi "Uygula" demeden listeyi değiştirmiyor ve kaç kaydın eşleştiğini uygulamadan önce gösteriyor.
KYS: Arama artık Türkçe karakterlere duyarsız: "İzmir" yazarken büyük İ''nin ayrı bir harf sayılması yüzünden satır bulunamama sorunu giderildi. Sorumlu araması hem kişinin adıyla hem sistem kaydıyla çalışıyor.
KYS: "Aksiyona ihtiyaç yok" seçildiğinde süreç yanlış aşamaya gidiyordu (kök neden analizine), üstelik ekranda "kapatma aşamasına geçti" yazıyordu. Artık gerçekten kapatmaya gidiyor.
KYS: Kaydet düğmesine hızlı iki kez basıldığında ikinci bir DİF kaydı oluşabiliyordu; bu kapatıldı.
KYS: Klasörün "boş mu" kontrolü silinmiş kayıtları saymıyor — içi silinmiş kayıtlarla dolu bir klasör "boş" görünüp silinemez.
KYS: Kalite kayıtlarına (DİF, denetim, şikayet, arşiv) katılımcı kapısı: Kaydı açan, sorumlusu ve ekibinde adı geçenler dışındaki kullanıcılar kaydı göremiyor.
KYS: Doküman deposundaki dosyalara imzasız erişim kapatıldı.
Arayüz: "Hareketi azalt" tercihi açık olan cihazlarda animasyon çalışmıyor.
Arayüz: Ekranlar arası geçişte artık boş beyaz sayfa görünmüyor.
Kurulum ve Sürüm Paketi: Kurulum dokümanındaki 4 hatalı adım düzeltildi (kimlik sunucusu adresi, veritabanı ilk kurulum dosyası, olmayan bir içe aktarma adımı ve sağlık kontrolü adresi) ve üç kurulum yolunun hangisinin ne zaman kullanılacağı yazıldı.
Kurulum ve Sürüm Paketi: Uygulamanın açılışta çökmesine yol açan iki sorun giderildi.
## Güvenlik
KYS: Bir klasöre verilen yetki, altındaki klasörlere kendiliğinden geçiyor. Alt klasöre kendi yetkisi tanımlanana kadar en yakın üst klasörün yetkisi geçerli oluyor; yetkiyi her klasörde tek tek tanımlama zorunluluğu kalktı.
KYS: Alt klasöre kendi yetkisi tanımlandığında devralma o klasörde duruyor — istisna tanımlamak mümkün.
KYS: Yetki tanımlama ekranı yalnızca yetkisi olan kullanıcılara açık; eskiden yetki düzenleme yüzeyi herkese görünüyordu.
KYS: Kopyalama hem kaynak hem hedef klasörde yazma yetkisi istiyor.
KYS: Klasör açarken artık kimlik bilgileri, yetkiler, hazırlama şablonları, saklama/imha kuralları, onay zinciri ve görüntüleme ayarları ayrı panellerde toplanıyor.
KYS: "Düzenle" düğmesi salt görüntüleme yetkisi olan kullanıcıda artık "Görüntüle" olarak çıkıyor — yetkisi olmayan kullanıcı düzenleme ekranını açıp kaydedememe hatasıyla karşılaşmıyor.
KYS: Yetki matrisindeki kişi tanımları gerçek kullanıcı dizinine bağlandı. Eskiden çözülemeyen bir kişi tanımı sessizce "yetki var" sayılıyordu; artık erişim kapanıyor ve çözülemeyen tanımlar ekranda uyarı olarak gösteriliyor.
Kurulum ve Sürüm Paketi: Windows arayüzlü kurulumcu da bu servisi kuruyor. Gereken güvenlik anahtarı kurulum sırasında üretiliyor ve iki servise aynı değerle yazılıyor — tek yere yazılması editörün hiç açılmamasına yol açıyordu.
Diğer Modüller: Kanban panolarında yetki, iptal akışı ve bildirimler sertleştirildi.
## Kaldırılanlar
Üretim/Rota ekranındaki artık geçersiz olan "yakında" etiketi kaldırıldı.
KYS: Yeni DİF açarken doldurulan ayrı form kaldırıldı; artık doğrudan sürecin 1. Adımı açılıyor. Eski form, 1. Adımın eksik bir kopyasıydı ve üç zorunlu alanı (uygunsuzluk kategorisi, açıklama, ilişkili olduğu alan) hiç sormuyordu — bu yüzden kaydı açan kullanıcı, hemen ardından "şu alanlar zorunlu" uyarısına çarpıyor ve aynı bilgileri ikinci kez giriyordu.', N'Ara Sürüm', 1, '2026-08-05 09:00:00', '2026-08-05 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.21')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.21', '2026-08-07', '2026-08-07', N'## Yenilikler
Yürürlükteki belgenin Word içeriği artık gerçekten kilitli. Yayınlanmış bir dokümanın metni üç ayrı yoldan hâlâ değiştirilebiliyordu. En ciddisi şuydu: Onaya gönderilmiş belge kilitli değildi, yani 1. Onaylayıcı imzaladıktan sonra hazırlayan metni değiştirebiliyor ve imzalar farklı iki metne verilmiş sayılıyordu. Artık "Yürürlükte", "Arşivlendi", "İptal Edildi" ve "Onay Bekliyor" durumlarında içerik değiştirilemiyor. Kullanıcı çıkışsız bırakılmıyor: Mesaj ne yapılacağını söylüyor — onay sürecindeki belge için "onaycıdan geri gönderilmesini isteyin", yayınlanmış belge için "revizyon açın".
Şikayet kuyruğunun trafik ışığı artık gerçek durumu gösteriyor. Bütün şikayetler koşulsuz KIRMIZI yanıyordu: Dün açılan şikayet ile 60 gündür bekleyen şikayet ekranda birebir aynı görünüyordu ve "kritik sayısı" her zaman toplam açık şikayet sayısına eşit çıkıyordu. Artık ışık, söz verilen işlem tarihine göre yanıyor; gün sayısı ve bekleme filtresi de bu kuyrukta görünür hâle geldi. 2. HASSAS BELGELER — iki onaylayıcı kuralı gerçekten uygulanıyor
Ekranda kuralın nasıl işlediği yazılı, ve zincire ek adım eklendiğinde bu belgenin tarihçesine de düşüyor — "3 onaycı seçtim, 4 adım çıktı" sorusu cevapsız kalmıyor. 3. DİF (düzeltici faaliyet) — yetkiler ve yaygınlaştırma
Kalite yöneticisi artık adı geçmediği kayda da müdahale edebiliyor. Demo kurulumu "kalite-yoneticisi" rolünü oluşturup atıyordu ama hiçbir kapı bu rolü okumuyordu: Role sahip kullanıcı ilk denemesinde yetki hatası görüyordu. Rol adı Ayarlar''dan değiştirilebiliyor. Yol boyunca ikinci bir eksik de çıktı: "superadmin" rolü bütün ekranları görüyor ama hiçbir kayda dokunamıyordu — o da düzeltildi.
Bekleyen onaylar için hatırlatma: 3., 6. Ve 9. Gün (periyot Ayarlar''dan).
Hatırlatma sayacı her onay adımı için sıfırlanıyor, yani yeni onaycı önceki adımın gönderimlerini devralıp doğrudan eskalasyona gitmiyor.
Yeni seçenek: Hassas belge işaretinin altında "Belgedeki hassas bilgiler maskelensin". İsteğinize göre OTOMATİK değil, ayrı bir onay kutusu.
Kurumun kendi kuralları: Genel Tanımlar → Maskeleme kuralları. "Etiket" türü o etiketten sonraki DEĞERİ gizliyor ("Maaş: [GİZLENDİ]") ve etiketi bırakıyor, böylece belge okunabilir kalıyor. "Desen" türü ileri düzey kullanım için düzenli ifade kabul ediyor.
Hassas belgelerde ek dosyalar artık doğrudan depo adresinden değil uygulama üzerinden iniyor. Daha önce verilen imzalı adres uygulamayı tamamen atlıyor, iptal edilemiyor ve denetim izi bırakmıyordu. İndirme bir miktar yavaşlıyor — bilinçli bir takas.
"Doküman görme formatı" satırı artık gerçekten uygulanıyor: PDF kullan /PDF kullanma / Doküman viewer / Şirket parametresi.
Çakışma kuralı ekranda yazılı: Doküman ayarı klasörü ezer, boş bırakılırsa klasör geçerli. Doküman adımındaki boş seçenek "Klasör ayarı — PDF kullan" diye yazıyor, yani hangi değerin geçerli olacağı görünüyor.
Ayar uygulanamazsa görüntüleyici SEBEBİNİ yazıyor. Örnek: "PDF kullanma", Word''de hazırlanmış bir dokümanın gömülü görünümünde teknik olarak mümkün değil — sessiz kalmıyoruz.
Bedeli: Sunucu imajı küçük ("distroless") sürümden normal sürüme geçti. Temel imaj 180 MB → 349 MB. Bu, görüştüğümüz ve kabul ettiğiniz takas. 9. PANOLAR — kilometre taşı hatırlatmaları
Kilometre taşları tanımlanabiliyordu ama tarihi yaklaşan ya da geçen bir taş için kimseye haber gitmiyordu; taş yalnız ayar ekranında duran bir kayıttı.
Artık günde bir çalışan bir hatırlatma var: Taşın tarihi yaklaşınca ön uyarı, tarih geçince gecikme bildirimi gidiyor.
Pano ayar ekranında her kilometre taşı için yeni bir alan: "Hatırlat (gün önce)". Boş bırakırsanız ön uyarı gitmez ama gecikme bildirimi yine gider.
ONAY EKRANINDAN açılan belge kendi görüntüleme biçimini kullanıyor. Panel 9''daki "Doküman onay görme formatı" satırı bağlandı. Kalan dört satır (yazdırma · kontrollü kopya yazdırma · görüş görme · kalite kaydı görme) incelendi ve bağlanacak bir şey olmadığı görüldü: Görüş ekranı henüz yok, kalite kayıtları doküman göstermiyor, yazdırma ve kontrollü kopya çıktısı ise yalnız PDF olarak üretilebiliyor. O satırlar artık ekranda "(henüz uygulanmıyor)" yazıyor — boşa ayar girmenizi istemedik.
Veritabanı Güncellemesi Var (İki Adet): Uygulama açılışta kendini güncelliyor, ek işlem gerekmiyor. Mevcut verileriniz korunuyor: – Onay taleplerine hatırlatma sayacı eklendi (mevcut talepler sıfırdan başlıyor, yani devraldıkları bir sayaçla eskalasyona gitmiyorlar). – Klasör yetki matrisine "Maskesiz Görme" kutusu eklendi ve KAPALI geliyor. Bu güvenli: Maskeleme doküman başına açılan bir seçenek, yani bu sürüme geçen hiçbir dokümanda maskeleme açık değil. Panolar tarafında da bir kolon eklendi (kilometre taşı hatırlatma damgaları). Ayrıca: Mükerrer/benzer içerik karşılaştırması için iki kolon ve belge açma talepleri için yeni bir tablo. Hepsini uygulama açılışta kendisi kuruyor, ek işlem yok.
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): Bu satırlar YALNIZ parametre listesi boş olan kurumlarda otomatik oluşur; mevcut kurulumlarda kod içindeki varsayılan kullanılır, yani hiçbir şey bozulmaz. İsterseniz elle ekleyip değiştirebilirsiniz: – Gecikme eşiği (gün) — ışık kırmızıya döner – Kalite yöneticisi rolü – Eskalasyon rolü – Antet logo metni (boşsa antette "FİRMA LOGO" yazar) Yeni katalog listeleri (Genel Tanımlar): Antet şablonları · Maskeleme kuralları · Roller. Bunlar da yalnız ilgili liste boşsa otomatik oluşur. 13. Bu sürümde kapsam dışı bırakılanlar (şeffaflık) Sözünü ettiğimiz ama bu sürüme girmeyen kalemler:
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): SMS ile doğrulama (11.5) — sizin isteğinizle bu sürüme alınmadı. (SMS gönderim ve tek-kullanımlık kod parçaları depoda var ama birleştirilmedi.)
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): Görüntüleme biçimi ayarının kalan 4 satırı — bağlanacak bir yüzeyleri OLMADIĞI için. Görüş ekranı yazıldığında ya da kalite kayıtlarına doküman görüntüleme eklendiğinde bağlanabilirler.
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): Otomatik test sayısı: KYS (doküman) tarafı 88 → 487, arayüz tarafı 381 → 472. Panolar tarafında kilometre taşı hatırlatması için ayrıca testler eklendi (Boards toplam 34 test).
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): Her yeni test kümesi, önce ilgili kodu bilerek bozup testlerin GERÇEKTEN kırmızıya döndüğü görülerek doğrulandı — yani testler "her şeye evet diyen" testler değil.
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): Karartma/OCR altyapısı gerçek bir kapta koşularak doğrulandı. ═══════════════════════════════════════════════════════════════════════════════
## Hata Düzeltmeleri
Karar bekliyor sandığımız ama aslında HATA olan beş konu — bunlar "ayar ekranda var ama hiçbir şey yapmıyor" türünden sessiz kusurlardı.
Sizin verdiğiniz kararların gerçek uygulaması. Ek olarak, "sonraki faza" bırakılmış kalemler de kapatıldı — ikisinin altından hata çıktı. Bu sürümden sonra listede yalnız SMS doğrulaması kalıyor (sizin isteğinizle ertelendi). ⚠ Bu sürüm kurulum değişikliği içeriyor — en sonda "Kurulum notları" bölümünü mutlaka okuyun (sunucu imajı değişti, veritabanı güncellemesi var). 1. Ayarlarda görünüp çalışmayan beş konu düzeltildi Bunları "hangi seçeneği istersiniz" diye size soracaktık; kodu incelerken seçenek değil hata olduklarını gördük.
Ayarlar''daki "Onay hatırlatma periyodu" satırı artık okunuyor. Değer ekranda görünüyor ve düzenlenebiliyordu ama hiçbir kod okumuyordu. Şimdi Kokpit''in trafik ışığı eşiği ve onay hatırlatmaları bu değeri kullanıyor. "Gecikme eşiği (gün)" için de yeni bir satır eklendi. Varsayılanlar bugünkü davranışla aynı (3 / 8), yani ekran görünümü değişmiyor.
Kararınız gereği alanlar ekranda KALDI. Onaya gönderdiğinizde bu iki kişi onay zincirinin SON adımı olarak ekleniyor ve ikisi de imzalamadan doküman yürürlüğe girmiyor.
Onay adımında zaten seçtiyseniz tekrar eklenmiyor — aynı kişiye iki kez imza attırmıyoruz.
Sessiz bir kilitlenme riski de kapatıldı: Aynı kişi aynı onay seviyesine iki kez girdiğinde o seviye ASLA geçemiyordu (kişi bir kez imzalıyor, sistem ikinci imzayı reddediyor) ve belge "Onay Bekliyor"da sonsuza kalıyordu. Artık tekrarlı seçimler birleştiriliyor.
"Düzeltici faaliyeti yaygınlaştır" düğmesi artık doğru kayıt üretiyor. Düğme hatasız "başarılı" diyordu ama ürettiği kayıt yanlıştı: – kaynağın AYNI iş yerine kayıt açıyordu (yaygınlaştırmanın tanımı başka birimlere yaymaktır), – türü "Düzeltici" yazıyordu (başka birimde henüz sorun yok, yani ÖNLEYİCİ olmalı), – kaynak DİF bağı yoktu, gerekçe alınmıyordu, kimseye haber gitmiyordu, – kaynağın GEÇMİŞ termini kopyalanıyordu, yani yeni kayıt ilk saniyesinde gecikmiş görünüyordu. Artık hedef iş yerleri/bölümler çoklu seçiliyor, gerekçe ve ortak termin giriliyor, HER HEDEF için ayrı bir önleyici DİF açılıyor, kaynak bağlanıyor ve hedef birimin sorumlusuna bildirim gidiyor. Sorumlusu tanımlı olmayan birimler ekranda yazılıyor ("kayıt açıldı ama bildirim gitmedi"). "Kapalı ve düzeltici" şartı da artık sunucuda — daha önce yalnız tarayıcıda kontrol ediliyordu. 4. ONAY SÜRECİ — hatırlatma ve eskalasyon
Sıra ilerlediğinde artık yeni onaycıya haber gidiyor. Bildirim yalnız onaya GÖNDERME anında atılıyordu; zincir ilerlediğinde hiçbir bildirim yoktu, yani 2. Ve sonraki seviyedeki onaycı sıranın kendisine geldiğini HİÇ öğrenmiyordu. Gecikmelerin en sık sebebi buydu.
Sonuç alınmazsa 12. Günde ÜST AMİRE eskalasyon. Üst amir bulunamazsa eskalasyon rolüne gidiyor ve hazırlayana bilgi veriliyor — "yönetici tanımlı değil" durumu denetim izine de yazılıyor, sessiz kalmıyor.
Bildirim gönderilemezse eşik tüketilmiyor; bir sonraki turda tekrar deneniyor (uyarı sessizce kaybolmuyor). 5. KVKK — belgedeki hassas bilgilerin maskelenmesi
Açıldığında indirilen/yazdırılan çıktıda otomatik gizlenenler: TC kimlik no, IBAN, kredi kartı no, telefon, e-posta. Bunlar GERÇEK doğrulamayla yakalanıyor (TCKN resmî algoritması, IBAN mod-97, kart Luhn) — yani sipariş numarası, barkod ya da seri numarası yanlışlıkla gizlenip belge okunamaz hâle gelmiyor.
Maskelenen belgenin gövdesi ARAMADA da görünmüyor. Belge yine bulunabiliyor (adı, kodu, departmanı, türü, klasörü ve etiketleri aranıyor).
Bu ayar iki ayrı yerde, iki ayrı sözlükle tutuluyordu (klasör tanımlamada 6 satır, doküman sihirbazında 4 bambaşka seçenek) ve ikisi de kaydediliyor, hiçbiri okunmuyordu. Sözlük birleştirildi.
KAPSAM: Bu sürümde "Doküman görme" satırı bağlandı. Diğer 5 satır (yazdırma, kontrollü kopya yazdırma, görüş görme, onay görme, kalite kaydı görme) kaydediliyor ve okunmaya hazır ama henüz uygulanmıyor. 8. Karartma ve OCR motoru gerçekten çalışır hâle getirildi Taranmış kâğıt ve fotoğraflarda görsel karartma (siyah kutu) isteğinizin karşılığı. Bu tamamen sunucu tarafı kurulum işi; kullanıcı ekranlarında değişiklik yok ama etkisi büyük:
Karartma motoru ve OCR üretimde SESSİZCE devre dışıydı. İki sebebi vardı: Dil verisi sunucu paketine hiç girmiyordu ve gerekli sistem kütüphaneleri kurulu değildi.
İkisi de düzeltildi ve GERÇEKTEN ÇALIŞTIĞI kapta koşularak doğrulandı: Üretilen bir görüntüden metin tanındı ve karartma kutularının çizildiği kelime koordinatları alındı.
Karşılığında OCR de açıldı — daha önce aynı sebeple kapalıydı, yani taranan belgelerin içinde arama yapılamıyordu.
Sahibi seçilmemiş bir taş hatırlatma ALAMAZ — sistem bildirimi kime göndereceğini bilmiyor. Bu durum sessiz geçilmiyor: Hatırlatma tanımlıyken sahip seçilmemişse ekranda uyarı çıkıyor.
Aynı hatırlatmanın iki kez gitmesi ya da hiç gitmemesi engellendi. Taşın tarihini değiştirirseniz hatırlatma yeni tarihe göre yeniden kurulur.
ANTET ŞABLONU seçimi artık gerçekten iş yapıyor — altından hata çıktı. Seçtiğiniz şablonun ADI, belgenin sol üst LOGO kutusuna yazı olarak basılıyordu: "Kalite belgesi şablonu" seçince antette literal olarak o yazı çıkıyordu. Üstelik üç seçenek de AYNI anteti üretiyordu — "Boş / şablonsuz" dahil. Düzeltildi: Logo kutusuna artık kurumunuzun metni basılıyor (Ayarlar → "Antet logo metni"), "şablonsuz" tipi GERÇEKTEN antetsiz belge üretiyor, ve şablon listesi Genel Tanımlar → "Antet şablonları"na taşındı — kendi şablonunuzu ekleyebilirsiniz. Mevcut taslaklardaki eski seçimler çalışmaya devam ediyor.
"İZLEMEDE" durumundaki DİF artık yaygınlaştırılabiliyor. Önceki sürümde reddediliyordu. Gözden geçirince reddin gerekçesiz olduğu görüldü: Şartın amacı "etkinliği DOĞRULANMAMIŞ bir düzeltmeyi yayma", oysa izlemeye alınmış kayıt süreç olarak bitmiş ve etkinliği doğrulanmıştır. Reddetmek, doğrulanmış bir düzeltmenin yayılmasını geciktiriyordu. İptal edilmiş ve kapanmamış kayıtlar hâlâ reddediliyor.
Ekran Görünümü Değişmeyen Ayarlar: Hatırlatma periyodu ve gecikme eşiği varsayılanları bugünkü davranışla aynı (3 / 8 gün); bir şey ayarlamanız gerekmiyor.
## Güvenlik
Dış paylaşım bağlantısındaki şifre artık işe yarıyor. "Dokümana şifre yok" demenizin sebebi buydu: Bağlantı oluşturulurken hangi dosyanın paylaşılacağı sunucuya hiç gönderilmiyordu. Alıcı şifreyi DOĞRU giriyor, sonra "bu paylaşımda indirilebilir dosya bulunmuyor" yazısını görüyordu. Artık paylaşılacak dosya seçiliyor ve seçilmezse uyarı veriliyor. Ek olarak: Üretilmiş bağlantıların listesi ve İPTAL düğmesi ekrana geldi (daha önce dışa çıkmış bir bağlantı geri alınamıyordu), ve şifre deneme hızı dakikada 30''dan 5''e düşürüldü.
Hazırlama sihirbazının "Ek güvenlik" adımındaki "1. Onaylayıcı / 2. Onaylayıcı" alanları kaydediliyor ama bir daha hiç okunmuyordu. Ekranda "İki onaylayıcı olmadan değişiklik yapılamaz" başlığı duruyordu ama arkasında hiçbir kapı yoktu.
Asıl belgeye erişim korundu — istediğiniz gibi. Klasör yetki matrisine yeni bir kutu geldi: "Maskesiz Görme". Bu yetkisi olan kullanıcı belgenin aslını görüyor ve HER maskesiz erişim denetim izine yazılıyor. Kurumun kendini kilitlemesi mümkün değil: Yönetici her zaman aslını görüyor.
Gövdesi Word dosyasında olan dokümanlarda metin maskesi çalışmıyor; o dokümanlarda çıktı, maskesiz görme yetkisi olmayan kullanıcıya VERİLMİYOR (sessizce maskesiz vermek yerine). 6. Hassas belge indirmelerine parola
Hassas işaretli bir dokümanın PDF''i indirilirken parola isteniyor ve indirilen dosya o parolayla açılıyor.
Parola hiçbir yerde saklanmıyor. İndirme anında soruluyor, dosya şifreleniyor ve değer düşüyor. Ekran, parolayı belgeyi ilettiğiniz kişiye ayrı bir kanaldan (telefon / yüz yüze) bildirmenizi hatırlatıyor. Bunun sebebi güvenlik: Parolayı kalıcı saklamak, veritabanında okunabilir bir sır tutmak demekti.
En az 6 karakter isteniyor (PDF parolası çevrimdışı denenebildiği için kısa parola gerçek koruma sağlamıyor).
Şifreleme yapılamazsa dosya VERİLMİYOR — korunduğu sanılan bir belgeyi korumasız teslim etmemek için.
KAPSAM: Parola, uygulama içinde ÜRETİLEN PDF''ler için. Word/Excel ekleri parolalanamıyor (kütüphane desteği yok); onlar için yukarıdaki depo-adresi önlemi geçerli. 7. Görüntüleme biçimi ayarı (klasör sihirbazı 9. PANEL)
Kontrollü kopya föyü de hassas belgede parolalı iniyor. Föy, nüshanın kime/ne zaman verildiğini taşıyor; hassas bir belgenin dağıtım kaydı da hassastır. Parola NÜSHA BAŞINA: Kontrollü kopya zaten tek bir alıcı için üretilen numaralı bir nüsha. Tek ortak parola olsaydı bir alıcıya verilen parola bütün nüshaları açardı — oysa kontrollü kopyanın tüm amacı nüshaları tek tek izleyebilmek. Toplu yazdırmada nüshalar sırayla soruluyor.
Ayrıca doğrulandı: "ekran görüntüsü uyarısı" işareti ZATEN bağlıymış — hassas belge görüntülenirken uyarı şeridi çıkıyor. (Tarayıcıda ekran görüntüsünü ENGELLEMEK teknik olarak mümkün değil; dürüst karşılığı görünür uyarı + görüntülemenin kayda geçmesi.) Böylece hassas belge güvenlik işaretlerinin 8''inin 8''i de gerçekten çalışıyor. 11. Belge açma onayı ve BENZER İÇERİK — iki yeni yetenek
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): Doküman açarken şifre sorma (11.4-b). Word''de düzenlemede belgeyi editör sunucusu kendisi çekiyor ve o istekte kullanıcı oturumu yok — şifre kapısı orada uygulanamıyor. En pahalı seçenek aynı zamanda en kolay delinen olurdu.
YENİ AYAR SATIRLARI (Ayarlar → Parametreler): Word/Excel EKLERİNE parola: Bunu yapabilecek kütüphane yok. Onlar için alınan önlem farklı. 14. KALİTE GÜVENCESİ
## Kaldırılanlar
Temizlik: Artık kullanılmayan tek seferlik demo veri betiği (scripts/seed-kaaf-boards.ps1) kaldırıldı. 10. Kapsam dışı sanılan beş kalem de kapatıldı Bu beşi "sonraki faza" bırakılmıştı; incelenince ikisinin altından HATA çıktı.', N'Ara Sürüm', 1, '2026-08-07 09:00:00', '2026-08-07 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.22')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.22', '2026-08-08', '2026-08-08', N'## Yenilikler
Yeni "Veritabanı" (Veri Sözlüğü) penceresi — hangi tablo ne işe yarıyor, artık ekrandan görülebiliyor ve yapay zekâ sorgu üretirken buna bakıyor.
Senkronlar artık bir veritabanı sorgusundan veri çekebiliyor ve çektiğini bir kataloğa ya da bir veritabanı tablosuna yazabiliyor.
Süreçlerde "süreci başlatan" ve "işlemi yapan" kişi bilgileri her yerde hazır geliyor.
Formların olaylarına artık kod yazmadan işlem eklenebiliyor.
Senkronlar: Veritabanından okuma, veritabanına yazma. Senkronlar şimdiye kadar yalnız web servislerinden (REST/SOAP) veri çekip kendi deposuna yazabiliyordu. İki yeni yetenek geldi:
Veritabanı sorgusundan veri çekme: Kaynak tipi olarak artık "SQL" seçilip tanımlı bir PostgreSQL veya SQL Server bağlantısına sorgu yazılabiliyor. Sorguda tarih aralığı yer almıyorsa senkron, tarih penceresine bakmadan tek seferde çalışıyor.
Çekileni başka bir yere yazma: "Hedef" bölümünden satırların nereye yazılacağı seçiliyor: Eskisi gibi senkronun kendi deposuna, ya da bir kataloğa, ya da doğrudan bir veritabanı tablosuna. Aynı kayıt ikinci kez geldiğinde üzerine yazılıyor, yenisi ekleniyor — senkronu tekrar tekrar çalıştırmak mükerrer kayıt üretmiyor.
"Süreci başlatan" ve "işlemi yapan" her yerde hazır: Süreci başlatan kişi ile son işlemi yapan kişinin bilgileri artık e-posta metinlerinde, SQL sorgularında, bildirimlerde ve betik adımlarında doğrudan kullanılabiliyor. Ad-soyad, e-posta ve yöneticisi bilgisi hazır geliyor; "onay maili amirine gitsin" gibi kurgular için ayrıca sorgu yazmak gerekmiyor.
Değişkenler panelinde yeni gruplar: Panelde bu iki kişi için ayrı başlıklar açıldı; üzerine tıklayınca değişken adı panoya kopyalanıyor.
Mongo sorguları için yapay zekâ. Mongo sorgu penceresine, SQL tarafındakinin aynısı olan "AI ile Mongo sorgusu üret" bloğu eklendi.
Yeni "swıtch (metin karşılaştırma)" adımı. Bir değeri birden çok seçenekle karşılaştırıp farklı yollara ayıran adım. Kutunun alt kenarında 8 ayrı çıkış noktası var, böylece dallar birbirine karışmadan çiziliyor. Sağdaki panelde "karşılaştırılacak değer" alanı ve her ok için değer yazılan bir liste var. Adımın Türkçe kullanım rehberi de yazıldı — her adımda olduğu gibi kutunun köşesindeki "?" düğmesinden açılıyor.
Diyagram okları okunur hâle geldi. Ok çizgileri ve etiketleri, üst üste binen ve hangi adımdan çıktığı seçilemeyen görünümden kurtarıldı.
Aktarılan süreç artık ekrana sığıyor: Eski uygulamadan aktarılan bir süreç yaklaşık 25 ekran genişliğine yayılıyordu; kullanıcı diyagramı görebilmek için sürekli kaydırmak zorunda kalıyordu. Artık makul bir alana yerleşiyor.
Yapay zekânın ürettiği şemalar artık üst üste binmiyor. Adımlar merdiven düzeninde yerleştiriliyor. Aynı düzen, fotoğraf/ekran görüntüsünden üretilen süreçlerde de uygulanıyor.
Eşleştirme kararınız artık son söz: Bir satırın hangi adım tipine karşılık geldiğini onayladığınızda, aynı satır her yeni analizde tekrar karşınıza geliyordu. Artık kararınız öğreniliyor ve tekrar sorulmuyor. Kararları sıfırlamak için yeni bir "Eğitimi temizle" düğmesi eklendi.
Görselden üretimde hiç ok bulunamadıysa artık söyleniyor: Önceden sessizce bağlantısız bir şema üretiliyordu.
Birden fazla sorgu sekmesi: Artık aynı anda birkaç sorgu açık tutulabiliyor.
Sorgu alanı renklendirme ve öneri kazandı: Anahtar kelimeler renkleniyor, tablo ve kolon adları otomatik tamamlamada çıkıyor.
ARAÇ ÇUBUĞU tek satır, simge tabanlı ve gruplanmış hâle geldi.
Ekranı ikiye bölme: Sol proje ağacındaki satırlardan "yan panede aç" ile bir sekme ikiye bölünüp iki ekran yan yana kullanılabiliyor. Sınır: Bu düğme yalnız KULLANIM modundaki proje ağacında ve yalnız form, süreç, katalog satırlarında çıkıyor — pano/rapor satırlarında yok, tasarım modundaki listelerden veya sekmeye sağ tıklayarak bölme yapılamıyor. Bölme korunur ama sürüklediğiniz genişlik oranı sayfa yenilenince ortadan başlar. ⚠ Yazma modu artık gerçekten yazıyor — ve bu bir güvenlik kararıdır. Süper yönetici Veri Konsolu''nu "Yazma" moduna aldığında UPDATE / DELETE / DROP /TRUNCATE komutları canlı veritabanında GERÇEKTEN çalışır ve GERİ ALINAMAZ. Önceden sunucu tarafındaki bir kilit bu komutları sessizce yalnızca-okuma moduna düşürüyordu; bu sürümde o kilit ayar dosyasından kapatıldı. Ekrandaki kalıcı kırmızı uyarı şeridi de kaldırıldı; durumu artık yalnız düğmenin rengi belli ediyor. → Bu ayarın sizin kurulumunuzda açık mı kapalı mı kalacağı KARARA BAĞLANMALI. Ayrıntı bölüm 10''da.
YÜK TESTİ (Yönetim > Veri İşleme > Yük Testi): Sonuçlara otomatik "performans önerileri" bölümü: Koşu bitince sonuçlara bakıp ne yapılması gerektiğini söyleyen bir bölüm ekleniyor.
YÜK TESTİ (Yönetim > Veri İşleme > Yük Testi): Sayfa başlığının yanına "?" terim rehberi eklendi (eşzamanlı kullanıcı, gecikme, yüzdelik gibi terimler açıklanıyor).
Giriş ekranı sonsuza kadar "Yönlendiriliyor…" DİYORDU. Sunucuya ulaşılamadığında "Kurum" listesi isteği süresiz askıda kalıyor ve giriş ekranı hiç ilerlemiyordu. Artık 10 saniye sonunda vazgeçip "Sunucuya ulaşılamadı (zaman aşımı). Ağ bağlantını kontrol et." mesajı gösteriliyor. Özellikle uzaktan ve mobil bağlantıda hissedilir.
Veri sözlüğü müşteride açıklamasız başlıyor: Ürünle birlikte gelmesi planlanan hazır açıklama seti bu sürümde henüz üretilmedi (dosya boş bir taslak olarak duruyor). Sözlüğü yapay zekâyla toplu doldurma düğmesi de müşteri kurulumlarında bulunmuyor. Pratik sonuç: "↻ Yapıyı yenile" tablo/alan listesini getirir, insan diliyle açıklamalar elle yazılır. Hazır set üretildiğinde yeni sürümle birlikte otomatik yüklenecek şekilde hazırlandı.
Sözlük kendiliğinden tazelenmiyor: Yeni veri tablosu tanımlandığında sözlüğün elle yenilenmesi gerekiyor; otomatik tazeleme sonraki aşamaya bırakıldı.
Bölünmüş ekranda "kaydedilmemiş değişiklik" uyarısı doğrulanmadı: İkinci bölmede yapılan değişikliklerin kapatırken uyarı verip vermediği ekranda test edilmedi.
## Hata Düzeltmeleri
Kolon eşleştirme: "+ Eşleşme ekle" ile hangi kaynak alanın hedefte hangi kolona yazılacağı satır satır tanımlanıyor. Kaynak alan adları sorgunun kendisinden, hedef tablo ve kolonlar bağlantının şemasından, katalog kolonları ise katalog tanımından okunuyor — elle yazmak gerekmiyor. Eşleştirmede anahtar alanları unutsanız bile anahtarlar hedefe her zaman taşınıyor; bu, boş anahtarlı mükerrer kayıt üretilmesini engelliyor. NEREDE: Yönetim > Dış İşlemler > Veri Bağlantıları > Senkronlar Sınırlar: – "Veritabanı tablosuna yazma" bu ilk sürümde yalnız PostgreSQL hedeflerini destekliyor. SQL Server''a yazmak denenirse senkron hata verip durur. (SQL Server''ı KAYNAK olarak kullanmakta sorun yok.) – Depo dışında bir hedef seçildiğinde depoya özel işlemler (eski kayıtların temizlenmesi, zenginleştirme) atlanır. – "Sorgudan kaynak alanlarını getir" düğmesi yalnız kaynak tipi SQL olan senkronlarda çıkıyor. REST/SOAP kaynaklı bir senkronu kataloğa yazdırırken kaynak alan adlarını elle yazmanız gerekir. Ayrıca küçük bir düzeltme: Hiç REST bağlantısı tanımlı değilken çıkan uyarı kullanıcıyı yanlış yere yönlendiriyordu, düzeltildi. 4. SÜREÇLER — kişi bilgileri ve yazım kolaylıkları
Yazarken öneri çıkıyor: SQL kutularında ve bildirim adımının Başlık / Gövde /Gerekçe / Link alanlarında hazır değişkenler otomatik tamamlama listesinde görünüyor. Öneriler tek bir kaynaktan besleniyor, yani listeler birbirinden ayrışmıyor.
Kaydet / vazgeç: Süreç Tasarımcısı''ndaki SQL, Mongo, Betik, REST gövdesi ve SOAP zarfı kutularında ve form alanlarının SQL/Mongo pencerelerinde yazdığınız şey artık siz "💾 Kaydet" demeden uygulanmıyor; "Vazgeç" son kayıtlı hâle döndürüyor. Değişiklik yokken düğme "✓ Kaydedildi" yazıyor. (Veri Konsolu''nun kendi sorgu alanı bu kapsamda değil, orası eskisi gibi.) DÜZELTME — BETİK ADIMINDAKİ log() satırları hiç kaydedilmiyordu. Betik (Script) adımında log() ile yazdığınız satırlar hiçbir yere işlenmiyordu; süreç neden o yoldan gitti diye bakmak isteyen kullanıcı elinde hiçbir iz bulamıyordu. Düzeltildi. Kayıtlar süreç örneğinin değişkenler bölümünde _logs başlığı altında görünüyor — ayrı bir "günlük" ekranı yok. ⚠ Not: Betik adımının "?" yardım metni henüz güncellenmedi, orada hâlâ eski uyarı yazıyor.
SÜREÇ TASARIMCISI — yeni swıtch adımı ve çizim düzeltmeleri
Sessiz bir yanlış çalışma kapatıldı: Bir adımdan iki ok. Önceden Kullanıcı Görevi gibi sıradan bir adımdan iki ok çekilebiliyordu. Süreç çalışırken kullanıcının hangi düğmeye bastığına BAKILMADAN her zaman sessizce ilk ok izleniyordu — yani süreç yanlış yoldan gidiyor, kimse fark etmiyordu. Artık ikinci oku çizerken ekranda uyarı çıkıyor ve bağlantı kurulmuyor; aynı kural yayın öncesi Sağlık Kontrolü''nde de hata veriyor. Yapay zekâ ile süreç üretiminde böyle bir adım otomatik olarak Switch''e çevriliyor. ⚠ Bu desene sahip ESKİ süreçleriniz yeniden yayınlanmak istendiğinde artık hata alacaktır. Bu kasıtlıdır — o süreçler zaten yanlış çalışıyordu. (Süreç bazındaki "Kaydederken hataları gözardı et" seçeneği açıksa engel uygulanmaz.)
Sağlık kontrolü swıtch''i de denetliyor: Çıkış oku hiç yoksa hata, karşılaştırılacak değer boşsa hata, "diğer" dalı yoksa uyarı, etiketi boş oklar için uyarı veriyor.
Büyük süreçlerde "diyagram ekrana sığmıyor" düzeltildi.
Formun kendi olaylarına şimdiye kadar yalnızca JavaScript yazılabiliyordu. Artık alanlarda kullandığınız görsel işlem editörünün aynısı bu olaylarda da kullanılabiliyor — listeden işlem seçip parametrelerini dolduruyorsunuz. Kapsanan olaylar (ekranda İngilizce adlarıyla duruyor): Form açılınca ............ OnShow kaydetmeden önce ......... BeforeSave kaydettikten sonra ....... AfterSave kayıt ilk oluşunca ....... OnInstanceCreate kapanmadan önce .......... OnBeforeClose Beş ekranda da geçerli: Form tasarımı önizlemesi, form doldurma, herkese açık form bağlantısı, süreç görev formu ve süreç başlatma formu. Bir adım hata verirse kaydetme durmuyor: Köşede kırmızı uyarı çıkıp zincirin kalanı çalışmıyor, kayıt yine de tamamlanıyor. Adım ayarlarında "hata olsa da devam et" işaretliyse sarı uyarı verip sonraki adıma geçiyor. Sınırlar — bunları bilerek kullanın: – "Kaydetmeden önce" ve "kapanmadan önce" olaylarına konan GÖRSEL işlemler kaydı/kapanışı DURDURAMAZ. Durdurma hâlâ yalnız kod kutusundan yapılıyor; görsel işlem sadece yan işlem çalıştırır. – "Atama" adımıyla bir alanı gizleme/kilitleme/zorunlu yapma ayarları form olaylarında yok sayılıyor; yalnız değer ve seçenek yazma çalışıyor. Ayrıca: Süreç görev formu ve süreç başlatma formunda "kapanmadan önce" olayı önceden yalnız kod kutusu doluysa çalışıyordu; artık kod olmasa da görsel işlemler için tetikleniyor. NEREDE: Sol menü > Projeler > [Proje] > Formlar > form tasarımı > form özellikleri > Olaylar tasarımcı panelleri sadeleşti. Form Tasarımcısı ve Süreç Tasarımcısı''nın ayar panellerindeki uzun açıklama paragrafları küçük bir ⓘ simgesine taşındı; üzerine gelince balon olarak açılıyor. Panel gövdesi belirgin şekilde kısaldı, ayarlar daha az kaydırmayla görünüyor. Açıklamaların içeriği değişmedi, sadece yeri. 7. Eski uygulamadan aktarım — CİDDİ DÜZELTMELER
Güven puanı düzeltildi: Puan, yapay zekânın kendi kendine verdiği nottan etkileniyordu — yani model "eminim" derse puan yükseliyordu. Artık ölçülebilir kriterlere dayanıyor.
Aktarılan formu, aktarmadan önce çalışır hâlde görebiliyorsunuz. ⚠ SAP ve LOGO adım tipleri seçilebiliyor ama henüz çalışmıyor. Bir satırı SAP ya da Logo adımına eşleyebiliyorsunuz ve kararınız artık öğreniliyor. Ancak süreç motoru bu iki adım tipini henüz çalıştıramıyor: Aktarım tamamlanır, fakat süreç çalışırken o adım iş yapmaz. Bu tipleri daha önce işaretleyen "⛔ çalıştırılamaz" damgası da kaldırıldığı için ekranda uyaran bir şey yok. Bu tiplere eşleme yapmadan önce bunu bilin.
Kaydedilmiş sorguyu açmak artık üzerine yazmıyor: Önceden kayıtlı bir sorguyu açmak ekranda yazmakta olduğunuz sorgunun üzerine yazıyordu ve yazdığınız kayboluyordu. Artık kayıtlı sorgu kendi sekmesinde açılıyor.
YÜK TESTİ (Yönetim > Veri İşleme > Yük Testi): Süreç listesi eksik geliyor, başlatılan test yetki hatasına düşüyordu: Test kurulurken seçilecek süreçlerin bir kısmı listede görünmüyordu; görünenlerden biri seçilip test başlatıldığında da koşu yetki hatasıyla düşüyordu. İkisi de düzeltildi.
YÜK TESTİ (Yönetim > Veri İşleme > Yük Testi): Veri şablonu boş bırakılınca koşu baştan sona hata veriyordu: Artık şablon boşsa test yine de anlamlı biçimde çalışıyor.
Yeni sürüm çıktıktan sonra takılan "Yükleniyor…" EKRANI. Yeni bir sürüm yayınlandığında, tarayıcısı açık kalmış kullanıcıların ekranları "Yükleniyor…" yazısında sonsuza kadar takılıyordu — çünkü tarayıcı artık var olmayan eski dosya parçalarını istiyordu. Artık uygulama bunu fark edip kendini bir kez yeniliyor. Sekme içindeki ekranlar ve Akademi/Performans gibi ayrı ekranlar dahil, uygulama genelinde geçerli.
Mobil uygulama hâlâ iç test aşamasında. Bu sürümde telefon testleri geçici bir tünel adresi üzerinden yapıldı; kalıcı bir mobil ağ çözümü henüz belirlenmedi. Müşteriye dağıtılabilir bir mobil uygulama olarak duyurulmamalı. (Bu geçici adresler yalnız elle mobil derleme yapılırken okunuyor, normal web paketine sızmıyor.)
Mongo bağlantısında "aı ile sorgu üret" doğrulanmadı: Sözlük dolu iken Mongo bağlantısı seçiliyse yapay zekâya PostgreSQL sözlüğünün gönderiliyor olması ihtimali var; canlı denenmediği için bu sürümde vaat edilmiyor.', N'Ara Sürüm', 1, '2026-08-08 09:00:00', '2026-08-08 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.23')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.23', '2026-08-09', '2026-08-09', N'## Yenilikler
Mikrofon butonuna dokunup soru sorulur: Ses kaydedilir → yazıya çevrilir → doğal dilden SQL''e çevrilip salt-okur çalıştırılır → sonuç sözlü/konuşma diline uygun bir özet olarak hem yazılı hem sesli (metin-okuma) geri döner.
Tamamen ellerin işgal olduğu senaryolar (araçta, sahada) için tasarlandı — soru sorup cevabı dinlemek yeterli.
Sorgular her zaman salt-okur çalıştırılır; herhangi bir yazma işlemi tetiklenmez.
Ana ekranda mikrofon giriş butonu; ses kaydı ve oynatma cihaz üzerinde yönetilir.
Bu ilk sürüm temel altyapıyı kurar; ekran kapsamı ilerleyen sürümlerde genişleyecek.
Mobil uygulamayı yükleyen kullanıcılara görev atama/onay gibi olaylarda gerçek zamanlı bildirim gönderilmesi için altyapı kuruldu (Firebase üzerinden, dışarıya bağımlı üçüncü parti bir servis olmadan — kapalı ağ kurulumlarında da çalışır).
Bildirim gönderimi opsiyoneldir: Yapılandırılmamışsa sessizce devre dışı kalır, hiçbir yazma işlemini etkilemez.
Üretim: Yeni "Depo İşlemleri" ekranı: Rafa yerleştirme, raflar arası taşıma, mamul↔ret transferleri artık dokunmatik cihazlarda (tablet/telefon) da sahada yürütülebiliyor.
Üretim: Sekmeler: Rafa Yerleştirme, Raf Arası Taşıma, Mamul→Ret, Ret→Mamul.
Üretim: Sürükle-bırak ile göze taşıma: Doluluk durumuna göre ısı haritası renklendirmesi, bloklu/kilitli gözler açıkça işaretli.
Üretim: Taşıma sırasında miktar/lot/neden/not bilgisi bir modal ile istenir.
Üretim: Barkod okutma: Hem hızlı el terminali/okuyucu (klavye taklidi) hem elle giriş desteklenir.
Üretim: Sürükleme artık dokunmatik ekranlarda da doğal çalışıyor: Kısa basılı tutma sürüklemeyi başlatır (titreşimle onaylanır), erken hareket sayfa kaydırması olarak yorumlanır — parmak listeyi kaydırmakla kart sürüklemek birbirine karışmıyor.
Üretim: Kenara yaklaşınca liste otomatik kayar; doğru bir göze girildiğinde ve bırakıldığında titreşim + görsel geri bildirim (yeşil yanıp sönme) verilir; kapasite dolu bir göze bırakılmaya çalışılırsa uyarı titreşimi + toast mesajı gösterilir.
Üretim: Masaüstünde görüntülenen bir eşleştirme QR kodu telefonla okutularak, telefonun kamerası masaüstündeki Depo İşlemleri ekranı için uzaktan bir barkod okuyucu haline getirilebiliyor — telefonda ayrıca giriş yapmaya gerek yok, eşleştirme kodu 15 dakika geçerli.
Üretim: Bu özellik ek bir kurulum (sertifika) gerektirir; devreye alınmadıysa diğer tüm Depo İşlemleri özellikleri normal çalışmaya devam eder.
Üretim: 23 satırlık düz menü, 4 katlanır gruba ayrıldı: Planlama, Yürütme, Depo (WMS), Tanımlar. Son açık grup hatırlanıyor.
Üretim: Dar ekranlarda (≤900px, tablet/telefon) menü hamburger-çekmece haline geliyor, üst başlık sadeleşiyor.
Form Tasarımcısı araç çubuğuna "JS Yardım" düğmesi eklendi: Formlarda özel kod yazarken kullanılabilecek tüm hazır fonksiyonları (alan okuma/yazma, kullanıcı bilgisi, SQL/Mongo sorgusu çalıştırma, katalog sorgulama vb.) aranabilir bir referans penceresinde gösterir.
Form Tasarımcısı: Form event''lerine (buton tıklama, alan değişimi vb.) kod yazmadan aksiyon tanımlama desteği genişletildi; kolon editörü ve zengin bileşen ayarlarına yeni seçenekler eklendi.
KYS: Arşiv kutusu silme işlemine, önceden sadece ekranda olan imha defteri onayı sunucu tarafında da eklendi (uç doğrudan çağrılarak bu kontrol atlanabiliyordu).
KYS: Belge görüntüleyicide artık doküman durumu (yürürlükte / onay bekliyor vb.) her zaman görünür bir şerit olarak gösteriliyor — önceden bu bilgi bazı ekranlarda gizli kalabiliyordu.
KYS: Şikayet durum geçiş kuralları sunucu ve ekranda birbirine hizalandı — "Kapalı" bir şikayette "Onaya Gönder" düğmesinin aktif kalması ve sahipsiz DİF numarası üretilmesi önlendi.
KYS: Erişim talebi akışı, doküman ilişki grafiği görünümü, fiziksel arşiv konum ağacı editörü (Ayarlar''dan yönetilebilir), doküman↔ arşiv kutusu bağı, imha defteri talebi, gözden geçirme periyodu + etiket köprüsü, tebliğ özeti, onay zinciri görünümü ekranları eklendi — bu özelliklerin backend''i hazırdı, artık kullanıcı arayüzünden de erişilebiliyor.
KYS: Backend test sayısı 388''den 1344''e çıktı (çözüme hiç eklenmemiş 4 test projesi bağlandı, 794 gizli kalmış test ortaya çıktı). Ön yüz test sayısı 472''den 566''ya çıktı. Hepsi yeşil.
## Hata Düzeltmeleri
Üretim: Depo İşlemleri ekranındaki "Kamera" düğmesiyle telefonun/tabletin kendi kamerası barkod/karekod okuyucusu olarak kullanılabiliyor — ayrı bir el terminali gerekmiyor. Okutma sesli (bip) ve dokunsal (titreşim) olarak onaylanıyor, ardışık okutmalarda ekran açık kalıp bir sonraki barkodu beklemeye geçiyor.
Üretim: Kamera/konum izni verilmemişse veya güvenli bağlantı (HTTPS) yoksa kullanıcıya anlaşılır bir hata mesajı gösteriliyor.
Süreç Tasarımcısı: "Doğru/Yanlış" (ifElse) adında yeni, sabit 2 çıkışlı bir karar düğümü eklendi. Mevcut "Karar" düğümünün aynı mantığını kullanır; farkı, her zaman sadece iki sabit çıkışı (sağda yeşil ✓ Doğru, altta kırmızı ✗ Yanlış) olması ve bu çıkışlardan çekilen bağlantının otomatik olarak doğru şekilde etiketlenmesidir — elle etiketleme veya karıştırma riski ortadan kalkar.
Süreç Tasarımcısı: Bu düğümün çıkışları dışındaki genel bağlantı noktalarından (üst/sol/sağ/alt) yanlışlıkla bağlantı çekilirse sistem bunu reddedip uyarır — etiketsiz/hatalı dallanan bir bağlantının sessizce yanlış tarafa gitmesi artık mümkün değil.
Süreç Worklıst Yönlendirmesi: Bir formu "Gönder" ile tamamladığınızda, sıradaki görev SİZE atanmış olsa bile form artık otomatik olarak açılmıyor — İş Listesi''ne düşüyor. Böylece kullanıcı bilinçli olarak İş Listesi''nden görevi seçip açar; art arda otomatik form açılmaları ortadan kalktı.
KYS modülü uçtan uca tarandı; tespit edilen 19 eksiklik paketinden 18''i bu sürümde kapatıldı.
KYS: Postgres imajı: Dağıtım dosyalarında kullanılan veritabanı imajı, Doküman arama ve Veri Sözlüğü''nün ihtiyaç duyduğu bir bileşeni (pgvector) içermiyordu — bu durumda uygulama hiç açılmıyordu. Düzeltildi; kurulum belgeleri de buna göre güncellendi.
KYS: Kontrollü kopya durumu sunucuyla hizalandı — "Aktif" seçimi artık sessizce "Bekliyor"a düşmüyor, "Geri al" işlemi artık hata vermiyor. Onaylanmamış bir sürümün kontrollü kopyası artık basılamıyor.
KYS: Kayıtlı içerik haritası artık ekranda düzgün görünüyor (önceden veri formatı uyuşmazlığı yüzünden hiç görünmüyordu).
KYS: Reddedilen bir revizyon geri alındığında doküman artık ne eski onaylı metinde ne de "Yürürlükte" gibi yanlış bir durumda asılı kalmıyor; red durumu hazırlayan kişiye bildiriliyor ve tarihçede görünüyor.
## Güvenlik
KYS: Form açılır listeleri: Yönetici olmayan kullanıcılarda bazı form alanlarının açılır listeleri (SQL/Mongo kaynaklı) yetki hatası alıp sessizce boş kalıyordu. Düzeltildi.
KYS: Doküman paylaşım linki listesi erişim jetonunu (token) açıkça gösteriyordu — yetki kontrolü eklendi, jeton listeden kaldırıldı.
KYS: Dosya ekleme işlemi artık depoya yazmadan ÖNCE yetki kontrolünden geçiyor.
KYS: OnlyOffice üzerinden indirme/yazdırma işlemleri güvenlik ayarlarına bağlandı; erişim jetonunun geçerlilik süresi 2 saatten yapılandırılabilir 15 dakikaya düşürüldü.
KYS: Menü yetkilerinde "Aktif" işaretinin görmezden gelindiği iki nokta düzeltildi (pasif bir yetki satırı artık yetki vermiyor).
KYS: Reddedilen bir revizyonun geri alınması işlemi artık klasördeki onay yetkisini de soruyor.', N'Ara Sürüm', 1, '2026-08-09 09:00:00', '2026-08-09 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.24')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.24', '2026-08-11', '2026-08-11', N'## Yenilikler
Artık klasörün "hayır"ı bağlayıcı: Tarih hiç konmuyor, dolayısıyla hatırlatma da hiç doğmuyor. Dokümanda açıkça "gözden geçirilecek" seçilirse doküman klasörü ezmeye devam ediyor (eski kural).
Doküman Arama''nın detay kartı, dokümanın güncel durumuna değil "bir zamanlar askıya alınmış mı" bilgisine bakıyordu. Bir kez askıya alınmış doküman askıdan indirildikten sonra da sonsuza kadar "Askıya alınmış" görünüyordu.
Askıya Alma ekranının kendisi baştan beri doğru çalışıyordu; yani aynı doküman iki ekranda birbirine zıt görünüyordu. Artık ikisi de dokümanın güncel durumunu gösteriyor. Geçmişte askıya alınmış olması bilgisi kaybolmadı, ayrı bir ipucu olarak duruyor.
"Askıda" ve "İptal" kartları her zaman 0 yazıyordu, tıklanınca liste boş geliyordu; "Yürürlükte" ile "Tümü" aynı sayıyı veriyordu.
Askıya alınmış bir doküman bu listede hâlâ "Yürürlükte" satırı olarak duruyordu; yani Askıya Alma ekranında yapılan işin bu ekranda hiçbir izi yoktu.
"Bir sonraki gözden geçirme" ve "İmha tarihi" hücreleri ekranda hesaplanıyordu (yayın tarihi + sabit 12 ay). Gözden geçirme artık kullanıcının seçimine bağlı olduğu için bu tarihler sistemin kendi takvimiyle çelişiyordu — kullanıcı olmayan bir işi takvimde görüyordu.
Düzeltilen Hatalar › Doküman Arama''da tür ve tip listeleri: Sonuç tablosunda aynı kolonda "Dokuman", "dokuman" ve "Form" yan yana çıkıyordu; kullanıcı üç ayrı tür olduğunu sanıyordu. Artık tek ve düzgün yazımla gösteriliyor.
Düzeltilen Hatalar › Doküman Arama''da tür ve tip listeleri: "Doküman tipi" filtresi yalnızca mevcut dokümanlarda geçen tipleri listeliyordu. Ayarlar → Genel Tanımlar''da tanımlanmış ama henüz kullanılmamış tipler (Prosedür, Talimat…) listede yoktu; artık tanımlı tiplerin tamamı seçilebiliyor.
Artık seçenekler tam yol olarak yazılıyor: "Ana klasör > Alt klasör > Klasör". Doküman hazırlama, doküman yükleme, tarama merkezi ve yayınlama ekranlarının hepsinde geçerli.
NOT: Bu bir ara çözüm. Açılıp kapanan, aranabilir gerçek ağaç seçici ayrı bir iş olarak duruyor.
Yeni doküman sihirbazında klasör seçildiğinde klasörün tanımları forma geliyor: Doküman tipi, doküman sahibi, ilgili bölüm, gizlilik sınıfı, kontrol tipi, kontrol edecek ve gözden geçirecek kişi/gruplar, dağıtım · onay · görüş listeleri.
KİLİTLİ alanda değer dokümana KOPYALANMIYOR, klasör zincirinden canlı okunuyor: Üst klasörde bir tanım değişirse alttaki dokümanlar yeni değeri görür. Doküman o alanda kendi değerini taşımaz.
Klasör değiştirildiğinde eski klasörden gelmiş ve sizin dokunmadığınız satırlar yenileniyor; elle eklediğiniz satırlar korunuyor. Önceden liste "boş olmadığı" için yeni klasörün listeleri hiç uygulanmıyor, doküman eski klasörün dağıtım listesiyle onaya gidiyordu ve ekranda satırlar dolu göründüğü için kimse fark etmiyordu.
Klasör Tanımlama ekranına kilit ızgarası eklendi: Yukarıdaki 13 alanın her biri ayrı ayrı kilitlenebiliyor. Kilit klasör başına tek bir kutu değil — "hepsi serbest ya da hepsi kilitli" zorunluluğu kalktı.
Izgarada her alanın değerinin nereden geldiği (bu klasör mü, üst klasör mü) yazıyor. Kilitlediğiniz bir alanda klasörde değer yoksa kayıtta uyarı veriliyor: O kilit etkisiz kalır ve nerede değer girileceği söylenir.
Mevcut kurulumlarda hiçbir alan kilitli değildir; kilitler ancak yönetici işaretledikçe doğar. Klasör ekranının eski "yansıt / değiştirilemez" kutuları hiçbir yerde okunmuyordu; o kayıtlar silinmedi, ekranda artık kilit ızgarasına yönlendiren açıklama var.
Revizyon numarası hiçbir akışta elle girilemiyor: İlk yayında "00" olur, her revizyonda bir artar. Alan ekranda salt-okunur ve değerin nereden geldiğini yazıyor. Kural yalnız ekranda değil SUNUCUDA uygulanıyor: Uçtan gönderilen revizyon numarası hiç okunmuyor, değer dokümanın kendi geçmişinden türetiliyor — yani arayüzü atlayan bir istemci de numarayı değiştiremiyor.
Bu madde test turunda "yeni dokümanda elle girilebilir olsun" diye istenmişti; 11.08.2026''da alınan kararla istek geri alındı ve numara sistem denetimine bırakıldı.
Gerekçe: "1" gibi tek haneli bir değer yazıldığında sonraki revizyon "02" olur ve "01" hiç var olmaz — atlanmış revizyon numarası ISO denetiminde açıklama gerektirir. Ayrıca tebliğ/okundu kayıtları revizyon numarasına göre eşleştiği için serbest bir değer eşleşmeyi bozar.
Sihirbazda "Klasörden gelen" seçiliyken ekranda hiçbir sayı yoktu; kullanıcı 6 ay mı 12 ay mı geleceğini, hatta hiç gelmeyeceğini göremiyordu. Artık salt-okunur olarak yazıyor: "6 ay", "12 ay · varsayılan" ya da "gözden geçirilmeyecek".
Üç seçenekli yapı (Klasörden gelen · Gözden geçirilecek · Gözden geçirilmeyecek) zaten vardı ve korunuyor.
Artık hatırlatma (uygulama içi bildirim + e-posta) klasörde seçilen kişi ve gruplara gidiyor; gruplar üyelerine açılıyor, aynı kişi iki kez seçilmişse tek bildirim alıyor. Klasörde seçim yoksa eski davranış korunuyor ve hazırlayana gidiyor.
Aynı kişilere ayrıca kapatılabilir bir GÖREV açılıyor: Hatırlatma artık tek çıktı değil — bkz. 2.6.
Yeni ve Geliştirilenler › Ekran bölme ve sekmeler kolaylaştı: Açık bir sekmeye tıklamak onu sekmenin içine geri büzüyor ve ana ekranı açıyor; kapalı sekmeye tıklamak ekrana yayıyor.
Klasör kopyalamada nelerin kopyalanıp nelerin kopyalanmadığı, devralmanın canlı çalıştığı ve kilidin alan bazında olduğu ekran içi açıklamalara yazıldı.
Aynı Paketle Gelen Diğer Modül İşleri: Bu sürüm notunun 1-6. Bölümleri KYS''ye ayrılmıştı. Aşağıdakiler aynı paketin içinde, KYS dışında yapılan işlerdir.
Aynı Paketle Gelen Diğer Modül İşleri › Sesli asistan: Uygulamaya sesle soru sorulabiliyor: Söylediğiniz cümle yazıya çevriliyor, veriye dönüştürülüp sorgulanıyor ve sonuç özetlenerek geri okunuyor.
Aynı Paketle Gelen Diğer Modül İşleri › Sesli asistan: Araç modu için tasarlandı — eller meşgulken kısa bilgi almak (örneğin bir işin durumu) hedefleniyor.
Aynı Paketle Gelen Diğer Modül İşleri › Sesli asistan: Kurulum notu: Konuşma-yazı çevirisi dış bir servise bağlıdır; ilgili anahtar tanımlanmazsa özellik sessizce kapalı kalır.
Aynı Paketle Gelen Diğer Modül İşleri › Depo İşlemleri (WMS): Depo transferi çekirdeği eklendi: Dört farklı transfer biçimi destekleniyor ve işlem sunucu tarafında korumalı yürüyor.
Aynı Paketle Gelen Diğer Modül İşleri › Depo İşlemleri (WMS): Dört sekmeli Depo İşlemleri ekranı geldi. Dokunmatik sürükle-bırak ile çalışıyor; telefonda uzun basınca kaldırma, otomatik kaydırma ve titreşimli geri bildirim var.
Aynı Paketle Gelen Diğer Modül İşleri › Depo İşlemleri (WMS): Barkod okutma iki yoldan mümkün: Ekrandaki gömülü kamera ile ya da telefonu bilgisayara eşleyerek (telefon tarayıcı köprüsü). İkinci yol için sertifika/HTTPS kurulumu GEREKMİYOR.
Pivot tablodaki bir hücreye tıklayınca o hücreyi oluşturan kayıtlar açılıyor — "bu sayı nereden geliyor" sorusu ekrandan cevaplanıyor.
Aynı Paketle Gelen Diğer Modül İşleri › Süreç tasarımcısı: Bu düğüm, süreci yapay zekâya anlattırıp kurdurma özelliğinin sözlüğüne de eklendi; aksi hâlde model bu düğümü hiç kullanamazdı.
Aynı Paketle Gelen Diğer Modül İşleri › Formlar ve RPA: Form olaylarında kullanılabilecek yardımcı işlevler için ekran içi yardım/örnek desteği geldi.
Aynı Paketle Gelen Diğer Modül İşleri › Formlar ve RPA: Kod yazmadan kurulan form aksiyonlarında ve RPA tarafında tamamlanan işler bu pakete dahil.
Aynı Paketle Gelen Diğer Modül İşleri › Mobil uygulama: RiverAI mobil uygulaması kaynak koduyla birlikte projeye alındı.
Aynı Paketle Gelen Diğer Modül İşleri › Mobil uygulama: ÖNEMLİ: Mobil uygulama sunucu kurulum paketinin PARÇASI DEĞİLDİR. Kendi derleme/dağıtım süreci vardır (uygulama mağazası ya da kurum içi dağıtım) ve bu sürümle birlikte kendiliğinden kurulmaz/güncellenmez.
Çok kurumlu kurulumda bir kurumun yöneticisi, adres satırındaki kurum adını değiştirerek BAŞKA kurumun yönetim uçlarına erişebiliyordu. Artık yalnız süper yönetici kurum sınırını aşabiliyor; normal yönetici kendi kurumunun dışına çıkamaz.
Web''e telefonla giriş: Ekrandaki kareyi (QR) telefonla okutarak oturum açılabiliyor.
Telefon–bilgisayar eşleştirme köprüsü artık okutma çubuğu olan TÜM ekranlarda çalışıyor (yalnız Depo İşlemleri''nde değil). Aynı anda tek eşleştirme oturumu tutuluyor ve telefonda "karşıda dinleyen yok" uyarısı veriliyor.
Sürükle-bırak yerine "kart seç + göze dokun" kısayolu eklendi — telefonda sürüklemek zor olduğunda kullanılıyor.
Aynı Paketle Gelen Diğer Modül İşleri › Depo İşlemleri: Mal kabul, karma koli kabulü, sayım, çıkış ve son kullanma tarihi kokpiti eklendi.
Aynı Paketle Gelen Diğer Modül İşleri › Depo İşlemleri: Kalem listesinde saklama koşulu görünüyor ve kayıt listeden açılıp düzenlenebiliyor.
PDF çıktı şablonları için görsel tasarımcı yerleşimi eklendi.
Aynı Paketle Gelen Diğer Modül İşleri › Mobil uygulama: Ayarlar ekranları, biyometrik kilit (parmak izi / yüz), QR tarayıcı, bildirimler ve köşe menüsü eklendi.
Aynı Paketle Gelen Diğer Modül İşleri › Mobil uygulama: 7.6''daki uyarı geçerli: Mobil uygulama sunucu kurulum paketinin parçası değildir, kendi dağıtım süreci vardır.
## İyileştirmeler
Aynı Paketle Gelen Diğer Modül İşleri › Depo İşlemleri (WMS): Menü, çok sayıda ekranı barındırabilmek için gruplu ve katlanabilir hâle getirildi; mobil görünüm buna göre düzenlendi.
Büyük tablolarda gezinme ergonomisi iyileştirildi.
Aynı Paketle Gelen Diğer Modül İşleri › Formlar ve RPA: Süreç iş listesinde yönlendirme iyileştirildi.
Aynı Paketle Gelen Diğer Modül İşleri › Mobil uygulama: Depo İşlemleri ekranları mobil tarayıcıda kullanılabilecek şekilde düzenlendi; yani mobil uygulama olmadan da telefondan çalışılabiliyor.
## Hata Düzeltmeleri
Klasör ayarında "gözden geçirme yapılmayacak" seçilmiş olsa bile, o klasörde yayınlanan her dokümana 12 ay sonrası için gözden geçirme tarihi yazılıyor ve o gün hatırlatma e-postası gidiyordu. En sık kullanılan yol tam olarak buydu (klasörün varsayılanı "gözden geçirilmeyecek", dokümanın varsayılanı ise "klasörden gelen").
Askıyı kaldıran kişi farkında olmadan üç şeye sebep oluyordu: · O revizyonun "Onaylayan" alanı, askıyı kaldıran kişinin adıyla değiştiriliyordu — hiç onaylamadığı bir revizyonun imzası ona geçiyor, gerçek onaylayanın adı siliniyordu. · Dağıtım listesindeki herkese ikinci kez tebliğ/okundu görevi düşüyordu. · Gözden geçirme takvimi sıfırlanıyordu; yani süresi dolmak üzere olan bir doküman askıya alınıp hemen geri alınarak 12 ay daha "temiz" gösterilebiliyordu. Artık geri alma yeni bir yayın sayılmıyor: İmza, tebliğ ve takvim olduğu gibi korunuyor. (İptalden geri getirilen doküman ise revize edilebilir listesine yeniden giriyor — önceden orada hiç görünmüyordu.)
Artık kartlar, filtreler ve tarihler dokümanın gerçek kayıtlı değerlerinden geliyor. Tarih yoksa hücre boş kalıyor (uydurma tarih yerine sessizlik). İmha tarihi henüz belirlenmemiş dokümanlarda gösterilen değer "~" işaretiyle tahmin olarak işaretleniyor ve uyarı/renk üretmiyor.
Klasör seçme listeleri hiyerarşiyi boşlukla girintileyerek anlatmaya çalışıyordu, tarayıcı o boşlukları yok sayıyor ve ekranda hiç girinti çıkmıyordu. Aynı adı taşıyan klasörler (ör. İki ayrı ağaçtaki "Raporlar", "(Kopya)" varyantları) yan yana duruyor, hangisinin hangi ağaç altında olduğu anlaşılmıyordu — doğru klasörü seçmek neredeyse imkânsızdı.
KİLİTSİZ alanda ise klasörün değeri sihirbaza ÖN-DOLGU olarak YAZILIR ve kullanıcı değiştirebilir — amaç zaten "hazır gelsin, ama gerekirse düzeltebileyim". Ekranda "ℹ Klasörden dolduruldu — değiştirebilirsiniz." ipucu görünür. Kullanıcı o alana dokunduğu andan itibaren değer artık kullanıcınındır ve klasör bir daha üzerine yazmaz.
Önceden yalnızca üç liste kopyalanıyordu ve yalnızca klasör açılır listesi değiştirildiği anda; kayıtlı bir taslak yeniden açıldığında, "Word''de aç" akışında ve revizyonda hiç çalışmıyordu.
Kilitli alan doküman sihirbazında değiştirilemez; değeri klasörden gelir. 13 alanın 11''inde kural yalnız ekranda değil SUNUCUDA da uygulanıyor: Doküman onaya gönderilirken de denetleniyor ve arayüzü atlayan bir istemci kilidi aşamıyor.
SINIR — iki alan yalnız EKRANDA kilitleniyor: "Gözden geçirecek tipi" ve "Kontrol matrisi". Bu ikisinde kilit normal kullanımda çalışır (sihirbaz alanı kapatır) ama sunucu tarafında ayrıca denetlenmez. Izgarada bu iki satırın yanında "(sunucuda denetlenmiyor)" yazıyor ve altında nedeni açıklanıyor — tahmin etmeniz gerekmiyor.
Hatırlatma bugüne kadar her zaman dokümanı hazırlayan kişiye gidiyordu. Klasör ekranında "gözden geçirecek kişi/grup" seçimi vardı, kaydediliyordu, ama hiçbir yerde kullanılmıyordu.
Yeni ve Geliştirilenler › Ekran bölme ve sekmeler kolaylaştı: AKTİF OLMAYAN her sekmede, kapat düğmesinin yanında bir "böl" düğmesi var. Önceden bölme yalnız sağ tık menüsünden yapılabiliyordu ve bulunamıyordu. Aktif sekmede düğme bilerek gösterilmiyor: Bir sekme kendisiyle bölünemez, devre dışı bir düğme de en görünür yerde "bozuk" görünürdü. KYS ana ekranında (hiçbir sekme açık değilken) düğme de çıkmaz — bölmek için önce bir ekran açmak gerekiyor.
DNA Merkezi''nde "Referans verilen" / "Referans alınan" başlıkları kimin kime atıf yaptığını söylemiyordu. Başlıklar açık cümleye çevrildi: "Bu doküman şunlara atıf yapıyor" ve "Şu dokümanlar bu dokümana atıf yapıyor". Lejant, "İlişki ekle" listesi ve kutu başlıkları aynı metinden besleniyor.
Ters yön ayrıca girilmiyor, otomatik türetiliyor: Bir kez A → B ilişkisi kurulduğunda B''nin kartında "bu dokümana atıf yapan" olarak görünüyor.
Davranış Değişikliği: Klasör ayarlarının dokümana geçmesi mevcut kurulumlar için bir değişikliktir. Bir klasörde: · doküman tipi "Prosedür" dışında bir değere ayarlıysa, · gizlilik sınıfı "Genel" dışında bir değere ayarlıysa, · kontrol edecek / gözden geçirecek kişi-grup listeleri doluysa, · dağıtım · onay · görüş listeleri doluysa, bu değerler ARTIK yeni doküman sihirbazına otomatik yazılıyor. Bugüne kadar klasörden YALNIZCA üç liste (dağıtım · onay · görüş) kopyalanıyordu; doküman tipi, gizlilik, sahip, ilgili bölüm ve kontrol/gözden geçirme alanları gelmiyordu. Önerimiz: Sürüm geçildikten sonra kalite yöneticisinin klasör ayarlarını bir kez gözden geçirmesi. Özellikle geçmişte "nasılsa kullanılmıyor" diyerek doldurulmuş alanlar varsa, artık dokümanlara yansıyacaktır. Not: "Prosedür" ve "Genel" değerleri, klasör ekranının fabrika varsayılanları olduğu için bilinçli bir seçim sayılmıyor ve dokümana yazılmıyor — böylece hiç düşünülmemiş klasörlerde bütün dokümanların sessizce "Prosedür" olması engellendi.
Bu Turda Kapsam Dışı Kalanlar (Dürüst Liste): Test notlarında iletilen ve bu sürümde YAPILMAYAN maddeler: · Gözden geçirme tamamlanınca dokümanın ONAYINA DÜŞMESİ dalı. Görevin kendisi, kapatma, periyodun ilerlemesi ve sonuç bildirimi bu sürümde yapıldı; yapılmayan yalnız "bildirim yerine onaya düşsün" seçeneği ve gerekçesi 2.6''da yazılı. · Klasör seçiminde açılıp kapanan gerçek ağaç seçici — şimdilik tam yol yazılıyor. · Doküman yükleme ekranında "OCR taransın mı?" seçeneği. · Doküman görüntüleme içindeki OCR/metin arama ile ilgili bildirilen sorun.
Aynı Paketle Gelen Diğer Modül İşleri › Süreç tasarımcısı: Karar düğümünün, tam iki çıkışı olan sabit bir biçimi eklendi: Çıkışlar Doğru ve Yanlış olarak hazır geliyor ve bağlantılar otomatik etiketleniyor.
Aynı Paketle Gelen Diğer Modül İşleri › Süreç tasarımcısı: Amaç, iki dallı basit koşullarda etiketi elle yazma ve yanlış dala bağlama riskini ortadan kaldırmak. Çok dallı kararlar için mevcut düğümler duruyor.
Onay motorunda kurum ayrımı eksikti: Bir kurumun onayı, başka kurumdan karara bağlanabiliyordu. Onay okuma ve karar verme artık çağıranın kurumuna kilitli.
Okutma çekirdeği ortaklaştırıldı ve gerçek etiketlerle çalışacak şekilde sağlamlaştırıldı: Tek boyutlu barkodlarda çözünürlük, ayırıcısı olmayan GS1 kodları, son kullanma tarihi okuma. Bir kod okunduktan sonra tabanca kilidi devreye giriyor (aynı kodu üst üste okumuyor) ve okutma bir pencere açtıysa kamera kendini kapatıyor.
Aynı Paketle Gelen Diğer Modül İşleri › Depo İşlemleri: Odaklı görevde tanınmayan bir kod okutulursa "yanlış göz" uyarısı veriliyor.
## Kaldırılanlar
Ne olduğu her seferinde ekranda yazılı olarak bildiriliyor ("Klasör tanımından dolduruldu: …", "Klasör değişti, … kaldırıldı").
Çıktı Şablonları listesinden teknik "slug" kolonu kaldırıldı.', N'Ara Sürüm', 1, '2026-08-11 09:00:00', '2026-08-11 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.25')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.25', '2026-08-13', '2026-08-13', N'## Yenilikler
Artık hatırlatma gerçekten gidiyor. Personelin platform hesabı tanımlı değilse ama e-posta adresi varsa hatırlatma doğrudan o adrese ulaşıyor — hatırlatma bir yetki değil bilgidir.
Teslim edilemeyen hatırlatmada artık "⚠ Gönderilemedi" yazıyor ve sebebi söyleniyor ("hedef personelin sistem hesabı eşlenmemiş — Genel Tanımlar › Personel''den tamamlayın"). Sessiz kayıp bitti.
Talep her hâlükârda sistemde kayıtlı kalıyor; posta gitmese de numarası var ve takip edilebiliyor.
Destek ekibi artık talebi yanıtlayabiliyor, durumunu değiştirebiliyor ve kapatabiliyor. Yanıt yazıldığında talebi açan kişiye bildirim ve e-posta gidiyor.
NOT: Bu işlemler şu an yalnız sistem üzerinden yapılabiliyor; destek ekibinin kendi ekranı bir sonraki sürümde geliyor.
Aynı hatanın tekrarını engellemek için ekranın kullandığı durum adları artık sunucunun sözlüğüne karşı otomatik denetleniyor.
Artık okunabilen kayıtlar gösteriliyor ve üstünde kaç kuyruğun okunamadığı yazıyor. "Hiç işiniz yok" ile "bir liste okunamadı" artık aynı görünmüyor.
Genel Tanımlar''a "SSS & Kullanım Kitapçığı" tanımı eklendi; yönetici artık soru-cevap girebiliyor.
Örnek veriyle kurulan sistemlerde altı hazır soru-cevap geliyor (revizyon numarası, tebliğ okundu onayı, kilitli klasör alanı, arşivden geri alma, kontrollü kopya).
Listede metinle arama: Doküman kodu, adı, sorumlu — hepsi taranıyor.
Sütun başlığına tıklayarak sıralama. Tarih sütunları takvim sırasına göre sıralanıyor (metin sırasına göre değil), numaralar da sayı olarak: "DF-2026-10" artık "DF-2026-9"dan sonra geliyor.
Sütuna göre gruplama — örneğin sorumluya göre.
İş türü süzgeçleri (Onayımı Bekleyen · İncelemem Gereken · Gerçekleştireceğim) artık tıklanabilir. Önceden yalnız sayı gösteriyorlardı, süzmüyorlardı.
"Sadece Yüksek Önemli" süzgeci. Öncelik alanı yalnız DÖF ve değişiklik taleplerinde tutulduğu için, süzgeç açıkken diğer kuyrukların kaç kaydının gizlendiği ekranda YAZIYOR — kullanıcı "işim kalmamış" sanmasın.
Açık süzgeçler listenin üstünde görünüyor ve tek tıkla temizlenebiliyor; kaç kaydın kaç kayıttan süzüldüğü de yazıyor.
Şeritteki modül kartlarında o modülde sizi bekleyen iş sayısı rozet olarak duruyor. Kuyruk kavramı olmayan modüllerde rozet çizilmiyor — "0" göstermek "işiniz bitti" demek olurdu.
Karta tıklamak artık başka bir ekrana atmıyor; o modülün iş listeleri aşağıdaki ağaçta açılıyor.
Erişiminiz olmayan bir modülün kartına tıklamak önceden HİÇBİR ŞEY yapmıyordu. Artık erişim talebi formu ön-dolgulu açılıyor (konu, modül ve açıklama hazır).
"Neden yok, oku" düğmesi artık gerekçenin gerçekten yazılı olduğu sayfaya götürüyor; önceden genel bir "yakında" mesajına gidiyordu.
Yeni ve Geliştirilenler › Dört modülün altyapısı: Kişisel panodaki beş bileşen "bu modül kurulmadı" diye boş duruyordu. Bu modüllerin ALTYAPISI kuruldu: Risk kaydı, eğitim planı ve katılım, tedarikçi performans skoru, cihaz envanteri ve kalibrasyon takvimi.
Yeni ve Geliştirilenler › Dört modülün altyapısı: Panodaki bileşenler artık gerçek veriden besleniyor: Yüksek Risk Sayısı, Risk Seviyesi Dağılımı, Eğitim Tamamlama Oranı, Ortalama Tedarikçi Skoru, Kalibrasyonu Yaklaşan Ekipman.
Yeni ve Geliştirilenler › Dört modülün altyapısı: Modüllerin kendi ekranları henüz yok: Kart üzerinde bu açıkça yazıyor: Hangi tablonun kurulduğu, hangi panoyu beslediği ve neyin eksik olduğu. "Yakında" denmiyor.
Yeni ve Geliştirilenler › Dört modülün altyapısı: Tablolar boşken panolar "0" göstermiyor, "henüz risk kaydı girilmedi" gibi doğru cümleyi yazıyor. Sıfır bir cevaptır; veri yokluğu başka bir şeydir.
Yeni ve Geliştirilenler › Dört modülün altyapısı: Tedarikçide ikinci bir tablo açılmadı: Tedarikçi kartı Genel Tanımlar''da zaten vardı, yalnız dönemsel skoru eksikti.
Yeni ve Geliştirilenler › Dört modülün altyapısı: Örnek veriyle kurulan sistemlerde tutarlı bir veri kümesi geliyor (7 risk, 4 eğitim ve katılımları, 4 tedarikçi skoru, 6 ekipman — biri gecikmiş kalibrasyonlu).
DÖF''lerin kapanma tarihi artık kaydediliyor; panoda "DÖF Kapanma Süresi Trendi" aylık ortalama olarak çiziliyor. Ölçüm bu sürümle başladığı için trend boş başlayıp zamanla doluyor — geçmiş kayıtlar için süre uydurulmuyor.
"Zamanında Onay Oranı" bileşeni eklendi. ÖNEMLİ: "zamanında" her AŞAMANIN kendi bekleme süresine bakıyor, toplam süreye değil. Üç seviyeli bir zincirde ilk iki adım aynı gün onaylansa bile toplam süreye bakmak uzun zincirli dokümanları haksız yere "geciken" gösterirdi.
Gecikme eşiği yeni bir ayar DEĞİL: Ayarlar › Sistem parametrelerindeki mevcut "Gecikme eşiği (gün)" kullanılıyor. İki ayrı yerden iki farklı gecikme tanımı olmuyor.
Yeni ve Geliştirilenler › Dokümanlarım ve Profilim ekranları: Dokümanlarım''a "Gözden Geçirici Olduğum" sekmesi eklendi. Kapanmış gözden geçirme görevleri de sayılıyor: Geçen yıl gözden geçirdiğiniz doküman bugün de sizin gözden geçirdiğiniz dokümandır.
Yeni ve Geliştirilenler › Dokümanlarım ve Profilim ekranları: Durum çipleriyle süzme (Bende · Yürürlükte · Onayda …), "N kayıt aksiyonunuzu bekliyor" uyarısı ve tek tıkla o kayıtlara gitme.
Yeni ve Geliştirilenler › Dokümanlarım ve Profilim ekranları: Doküman satırında "son düzenleme" tarihi ve yıldızla sık kullanılanlara ekleme.
Yeni ve Geliştirilenler › Dokümanlarım ve Profilim ekranları: Profilim ekranı artık çıkmaz sokak değil: Dört sayaç (grup üyeliği, oluşturduğunuz doküman, bu ay tamamladıklarınız, aktif akışlarınız) ve üç doküman rolü kartı var; hepsi ilgili listeye götürüyor.
Yeni ve Geliştirilenler › Dokümanlarım ve Profilim ekranları: Sol paneldeki dört künye kutusu ikon yerine SAYI gösteriyor. Sayı okunamazsa "0" değil uyarı işareti çıkıyor — hata hâlinde sıfır göstermek "işiniz bitti" demek olurdu.
Yeni ve Geliştirilenler › Dokümanlarım ve Profilim ekranları: Ana grup rozeti eklendi; birden çok gruptaysanız "İlk Grup +2 grup" biçiminde yazıyor.
Not formuna "Kendime" düğmesi eklendi. Özelliğin en sık kullanımı buydu ve önceden MÜMKÜN DEĞİLDİ: Kullanıcının kendi personel kaydını bilmesi ve listeden bulması gerekiyordu.
Hatırlatma artık bir kişiye, gruba ya da role de kurulabiliyor. Sunucu bunu zaten destekliyordu, yalnız ekranda seçilemiyordu.
"Başkasına atanmış" sayacı artık gerçekten bunu sayıyor; önceden "kaç farklı hedef var" diye başka bir şey ölçüyordu.
## Hata Düzeltmeleri
Ana ekranın yapışkan notlarına hatırlatma kurduğunuzda, hatırlatma hedefe HİÇ ULAŞMIYORDU. Ne e-posta gidiyordu ne uygulama içi bildirim.
Daha kötüsü: Sistem hatırlatmayı "gönderildi" diye işaretliyor ve notun üzerinde "✓ gönderildi" yazıyordu. Yani kullanıcı hatırlatmasının kaybolduğunu ASLA öğrenemiyordu ve bir daha da denenmiyordu.
Sebep, kişi seçicisinden gelen personel kaydının arka planda platform hesabına çevrilmemesiydi. Bu, demo/örnek veriyle kurulan HER sistemde tetikleniyordu.
Destek talebi oluşturduğunuzda ekran her koşulda "Talep hem e-posta hem sistem kaydı olarak iletildi" yazıyordu. E-posta ayarı yapılmamış ya da posta sunucusu bozuk bir kurumda bile.
Artık gönderim sonucu gerçekten kontrol ediliyor. Posta gitmediyse ekran bunu söylüyor ve iki durumu AYIRIYOR: "kurumda e-posta ayarı yapılmamış — yönetici Ayarlar''dan tanımlamalı" (kalıcı, yönetici işi) ile "posta sunucusu hatası" (geçici arıza). Kullanıcının yapacağı şey farklı olduğu için ayrı yazılıyor.
Talep açılıyordu ama durumu sonsuza kadar "Açık" kalıyordu. Yanıtlama, inceleme ve kapatma yolu YOKTU; destek ekibinin talepleri göreceği bir ekran da yoktu.
Ekran ise aksini vaat ediyordu: "İncelemede", "Yanıtlandı" ve "Kapalı" durumları için hazır görünüm vardı ama bu durumlar hiç oluşamıyordu.
Ana ekranın kişisel panosundaki bu kutu, onayda bekleyen dokümanlar olduğu hâlde DAİMA "0" yazıyordu. Kutu herkesin varsayılan panosunda olduğu için her kullanıcı onu ölü hâlde görüyordu.
Sessiz kalmasının sebebi şuydu: "0 bekleyen iş" makul bir cevap gibi duruyor, kimse hata olduğunu düşünmüyor.
Bekleyen işler ağacında on bir kuyruk var ama yalnız dördünün hata durumu izleniyordu. Kalan yedisinden biri okunamazsa o kuyruk sessizce boş görünüyordu — kullanıcı o modülde işi olmadığını sanıyordu.
Ayrıca bir kuyruk hata verdiğinde TÜM liste gizleniyordu; diğer on kuyruğun verisi elde olduğu hâlde kullanıcı hiçbir şey göremiyordu.
Destek ekranındaki "SSS & Kullanım Kitapçığı" kartı hiçbir zaman içerik göstermiyordu ve "yönetici Ayarlar''dan ekleyebilir" diyordu — ama gösterdiği yer sistemde YOKTU, yönetici gidip ekleyemiyordu.
Düzeltilen Hatalar › Küçük ama görünür düzeltmeler: Birleşik iş listesinde sütun başlıkları içerikle uyuşmuyordu: "Durum" sütununda modül adı, "Tarih" sütununda tarih olmayan değerler görünüyordu.
Düzeltilen Hatalar › Küçük ama görünür düzeltmeler: Ekran "dört kuyruk" yazıyordu; kuyruk sayısı on bire çıkmıştı.
Düzeltilen Hatalar › Küçük ama görünür düzeltmeler: Pano düzenini değiştirdiğinizde kayıt başarısız olursa hiçbir şey söylenmiyordu; sayfayı yenileyince eski düzeni buluyordunuz.
Düzeltilen Hatalar › Küçük ama görünür düzeltmeler: Evrak çantasındaki tarih "son düzenleme" gibi duruyordu ama aslında kaydın çantaya eklenme tarihiydi. Artık iki tarih de ayrı ayrı ve etiketli.
Düzeltilen Hatalar › Küçük ama görünür düzeltmeler: Süreç önizlemesinde "sonraki adımın onaycısı sunucudan gelmiyor" yazıyordu; bilgi sunucuda vardı, yalnız ekrana taşınmıyordu. Artık sıradaki onaycılar ve kaç onay gerektiği görünüyor.', N'Ara Sürüm', 1, '2026-08-13 09:00:00', '2026-08-13 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.26')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.26', '2026-08-14', '2026-08-14', N'## Yenilikler
Onbir eksik alanın tamamı kapatıldı. Güncelleme yalnızca EKLEME yapıyor: Hiçbir veri silinmiyor, hiçbir alan değiştirilmiyor. Uygulama açılışta eksikleri kendisi tamamlar.
Zamanlama ayarlarının saklanma biçimi geçmişte değiştirildi ve eski kayıtların yeni biçime taşınması gereken adım hiçbir kurulumda çalışmamış.
Sistem artık açılışta bu durumu tespit edip uyarı veriyor: Kaç zamanlamanın tetikleyici kaydı olmadığını söylüyor.
Eski kayıtlar KASITLI olarak otomatik canlandırılmadı. Aylardır uykuda olan süreçlerin habersizce çalışmaya başlaması istenmeyen sonuçlar doğurabilir; karar kurumun kendisine bırakıldı. Uyarı görürseniz KURULUM UYARILARI bölümüne bakın.
Düzeltilen Hatalar › Şikâyet kaydında veri kaybı: Artık onay işlemi kaydın hiçbir alanına dokunmuyor.
Düzeltilen Hatalar › Arayüzde kırık ve dağınık duran alanlar: Ana ekran artık geniş ekranlarda sağa sola yayılıyor; boşa giden kenar boşlukları kullanılıyor.
Düzeltilen Hatalar › Arayüzde kırık ve dağınık duran alanlar: Dar ekranlarda iş listesi öne alındı: Önce yapılacak işler, sonra özet kutuları.
Yeni ve Geliştirilenler › Bekleyen işlerde tek tıkla karar: Bekleyen işler listesinden çıkmadan onaylama, reddetme, "okudum" ve "gözden geçirdim" işaretlemesi yapılabiliyor. Artık Görevlerim ekranına gitmek zorunlu değil.
Yeni ve Geliştirilenler › Bekleyen işlerde tek tıkla karar: Liste çok uzadığında sayfa aşağı doğru büyümüyor; liste kendi içinde kayıyor ve başlık satırı sabit kalıyor.
Yeni ve Geliştirilenler › Bekleyen işlerde tek tıkla karar: Bir satırda işlem yaparken yalnız O SATIR kilitleniyor. Önceden tüm tablo soluklaşıyor ve ekran donmuş gibi görünüyordu.
Yeni ve Geliştirilenler › Bekleyen işlerde tek tıkla karar: Karar verirken veri kaybı riski taşıyan beş iş türü bilinçli olarak liste dışı bırakıldı; onlar kendi ekranlarından yapılmaya devam ediyor.
"Dokümanlarım", "Bu Ay Tamamladıklarım" ve "Aktif Akışlarım" listelerinden bir dokümanı açtığınızda artık doküman yönetiminin genel ekranına gitmiyorsunuz. Dört sekmeli detay ekranı açılıyor: Künye bilgileri, önizleme, akış tarihçesi ve revizyonlar.
Akış tarihçesi artık ayrı bir ekrana götürmüyor, ilgili satırın altında açılıyor. Önceden "akış tarihçesi" ve "incele" düğmeleri aynı yere gidiyordu.
Ana ekrandaki kutuları düzenleme alanı geldi: Hazır şablonlar arasından seçebilir, kutu kitaplığından ekleme yapabilir ve değişikliği anında önizleyebilirsiniz.
Seçiminiz kişiye özel saklanıyor.
Yeni ve Geliştirilenler › Bildirimler ve destek kuyruğu: Bildirim çanı KYS ekranlarında artık yalnız KYS bildirimlerini gösteriyor. Önceden tüm platformun bildirimleri karışık geliyordu.
Yeni ve Geliştirilenler › Bildirimler ve destek kuyruğu: Destek ekibinin talepleri göreceği ekran geldi (1.1.25''te "bir sonraki sürümde" denmişti). Talep yanıtlama, durum değiştirme ve kapatma artık ekrandan yapılıyor.
Tasarımda olup sistemde karşılığı olmayan beş liste için altyapı kuruldu: Aksiyonlar, açma onayı, bilgilendirmeler, rapor onayı ve bulgular.
İlgili modüller eklendiğinde bu listeler ana ekrana KENDİLİĞİNDEN yansıyacak; ek geliştirme gerekmeyecek.
Bu listeler bugün "modül henüz yok" bilgisiyle geliyor — boş görünüp kullanıcıyı yanıltmıyorlar.
## Hata Düzeltmeleri
Mevcut kurulumlarda veritabanı güncellemelerinin bir bölümü hiç uygulanmamıştı. Sonuç: Süreç başlatma hata veriyor, WPF içe aktarma ekranı çalışmıyor ve ilgili kayıtlar açılmıyordu.
Hatanın en sinsi yanı, YALNIZCA müşteri kurulumlarında çıkmasıydı. Yeni ve boş bir veritabanında her şey doğru kurulduğu için geliştirme ve test ortamlarında hiçbir belirti vermiyordu.
Aynı kusurun ikinci bir örneği İntranet tarafında bulundu: Duyuru slaytları, etkinlikler ve anketlerin "kimler görecek" ayarı ile hikâyelerin onay durumu, mevcut kurulumlarda hiç oluşmamıştı.
Üçüncü bir örnek daha çıktı: Bağlantı tanımlarındaki "değişkenler" alanı yalnızca örnek veri yüklenen sistemlerde oluşuyordu. Yani gerçek müşteri kurulumlarında hiç yoktu.
Etkisi tamamen sessiz: Bu tarihten önce kurulmuş bir zamanlama, ekranda kurulu ve doğru görünür, hata da vermez — ama HİÇ TETİKLENMEZ.
Düzeltilen Hatalar › Şikâyet kaydında veri kaybı: Şikâyet onaylandığında kayıttaki ayrıntılı bilgilerin bir kısmı siliniyordu. Onay sonrası kaydı açtığınızda daha önce girilmiş alanlar boş geliyordu.
Düzeltilen Hatalar › Arayüzde kırık ve dağınık duran alanlar: Süzgeç ve durum kutularına tıkladığınızda seçili olduğu ANLAŞILMIYORDU — görünüm değişmiyordu. Beş ayrı yerde aynı sorun vardı.
Düzeltilen Hatalar › Arayüzde kırık ve dağınık duran alanlar: Ekranda yaklaşık yirmi beş düğme, tasarıma uymayan gri sistem düğmesi gibi görünüyordu.
Düzeltilen Hatalar › Arayüzde kırık ve dağınık duran alanlar: Renk noktaları renksiz çıkıyordu; ipucu metinleri okunamayacak kadar soluktu.
Düzeltilen Hatalar › Arayüzde kırık ve dağınık duran alanlar: Bazı açıklamalar yalnızca fareyle üzerine gelindiğinde görünüyordu — dokunmatik ekran ve klavye kullananlar bu bilgilere hiç ulaşamıyordu. Artık tıklanarak açılıyorlar.
Düzeltilen Hatalar › Arayüzde kırık ve dağınık duran alanlar: Uzun metin blokları duvar gibi akıyordu; okunabilir hâle getirildi.', N'Ara Sürüm', 1, '2026-08-14 09:00:00', '2026-08-14 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.27')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.27', '2026-08-17', '2026-08-17', N'## Yenilikler
Yeni ve Geliştirilenler › Süreç Modelleme: Süreçler artık kendi tanım havuzuna sahip: Nitelikler (girdi, çıktı, risk, kaynak vb.) merkezi olarak tanımlanıp süreç adımlarına bağlanıyor. Onsekiz hazır tanım kataloğu kurulumla birlikte geliyor.
Yeni ve Geliştirilenler › Süreç Modelleme: Süreç Adım Detayları ekranı: Her adımın niteliklerini sekmeli bir formda girip görüntüleyebiliyorsunuz.
Yeni ve Geliştirilenler › Süreç Modelleme: Tanımlar tasarım alanından ayrıldı ve kendi ekranını aldı; süreç çizmekle tanım yönetmek artık karışmıyor.
Yeni ve Geliştirilenler › Süreç Modelleme: Süreç Envanteri görünümü: Kurumdaki tüm süreçlerin tek listede envanteri.
Yeni ve Geliştirilenler › Süreç Modelleme: Süreç Kartı ve Süreç Kitabı: Yazdırılabilir, denetime sunulabilir çıktılar.
Yeni ve Geliştirilenler › Süreç yönetişimi: Her sürecin bir SAHİBİ, gözden geçirme takvimi ve revizyon talebi akışı var.
Yeni ve Geliştirilenler › Süreç yönetişimi: Yönetişim ekranı: Sahip, takvim, kilit durumu ve bekleyen talepler tek yerde.
Yeni ve Geliştirilenler › Süreç yönetişimi: Süreç "check-out" (kilit) desteği: Bir süreci düzenlemeye başladığınızda kilitleniyor, aynı anda iki kişinin birbirinin işini ezmesi engelleniyor.
Yeni ve Geliştirilenler › Süreç yönetişimi: Kilit tasarımcı ekranına da bağlandı; kilitli bir süreci tasarımcıda açtığınızda durumu görüyorsunuz.
Yeni ve Geliştirilenler › ISO uyumu: Süreç ekranında ISO uyum halkası: Sürecin hangi maddeleri karşıladığı ve hangilerinin EKSİK olduğu görünüyor.
Yeni ve Geliştirilenler › ISO uyumu: Belge ve kayıtları ISO maddelerine eşleyebiliyorsunuz; eşleme uyum hesabına dahil oluyor.
Yeni ve Geliştirilenler › ISO uyumu: Belge bağlarken kaynak seçilebiliyor: KYS, doküman yönetimi veya disk.
Yeni ve Geliştirilenler › ISO uyumu: Uyum skoru geçmişi ve trendi tutuluyor; panelde bir önceki ölçüme göre değişimi gösteren fark rozeti çıkıyor.
Yeni ve Geliştirilenler › ISO uyumu: Yapay zekâ ile süreç üretimi artık ISO-farkında: Üretilen süreç ilgili maddeleri gözetiyor.
Yeni ve Geliştirilenler › Gerçek çalışmadan ölçüm ve analiz: As-is madenciliği: Süreçlerin gerçekte nasıl işlediği çalışma kayıtlarından çıkarılıyor ve TASARIMLA farkı raporlanıyor.
Yeni ve Geliştirilenler › Gerçek çalışmadan ölçüm ve analiz: Yol analizi: Motor bir dallanmada hangi yolu seçtiğini artık kaydediyor, böylece hangi yolun ne sıklıkla kullanıldığı görülebiliyor.
Yeni ve Geliştirilenler › Gerçek çalışmadan ölçüm ve analiz: Performans simülasyonu gerçek geçmişle kalibre ediliyor; tahminler varsayıma değil kuruma ait veriye dayanıyor.
Yeni ve Geliştirilenler › Gerçek çalışmadan ölçüm ve analiz: Gösterge ambarı: Dönem, ölçüm yeri, gösterge ve ölçüm kavramlarıyla performans verisi tutuluyor.
Yeni ve Geliştirilenler › Gerçek çalışmadan ölçüm ve analiz: Gösterge × dönem ölçüm ızgarası ekranı: Ölçümleri tablo üzerinde toplu girip görebiliyorsunuz.
Yeni ve Geliştirilenler › Gerçek çalışmadan ölçüm ve analiz: Adım süreleri artık doğru ölçülüyor: Olay damgası işin kuyruğa girdiği anda atılıyor ve arka planda çalışan adımların işlem süresi de hesaba katılıyor.
Yeni ve Geliştirilenler › Kalite: Hazır DÖF süreç şablonu geldi.
Yeni ve Geliştirilenler › Kalite: Şablon gerçek kayıtla çalışıyor: Süreç üzerinden DÖF kaydı açılabiliyor ve adımları ilerletilebiliyor.
Yeni ve Geliştirilenler › Linux kurulum sihirbazı: Grafik arayüzlü kurulum sihirbazı geldi. Adımlar: Kaynak seçimi, yapılandırma, kurulum, uygulama ve bitiş.
Yeni ve Geliştirilenler › Linux kurulum sihirbazı: Tanı kutusu ön kontrol yapıyor; eksik bir koşul varsa kurulum başlamadan söylüyor.
Yeni ve Geliştirilenler › Linux kurulum sihirbazı: Yönetici (sudo) yetkisi gereken adımlar ayrı bir geçitle isteniyor.
Yeni ve Geliştirilenler › Linux kurulum sihirbazı: Kurulum sırasında canlı yapay zekâ desteği ve kendi kendini onarma (self-heal) var.
Yeni ve Geliştirilenler › Linux kurulum sihirbazı: Kurum, kullanıcı ve rol bilgileri sihirbazdan toplu yüklenebiliyor. Yükleme öncesi "deneme çalıştırması" ile ne olacağını görebilirsiniz.
Yeni ve Geliştirilenler › Linux kurulum sihirbazı: Sihirbaz tek dosya olarak paketleniyor; hedef makineye ek kurulum gerekmiyor.
Yeni ve Geliştirilenler › Projeyi başka kuruma taşıma: Bir projeyi hedef kuruma BİREBİR ÇALIŞIR hâlde aktarabiliyorsunuz — formlar, süreçler ve bağlı tanımlar birlikte gidiyor.
Yeni ve Geliştirilenler › Projeyi başka kuruma taşıma: Taze bir kuruma aktarılan proje artık otomatik olarak varsayılan proje yapılıyor; aktarım sonrası boş ekran karşılamıyor.
Yeni ve Geliştirilenler › WF dosyalarından süreç içe aktarma: Mevcut WF (XAML) süreç dosyalarınız içe aktarılabiliyor. Aktarma iki aşamalı: Önce dosyadan kesin olan iskelet çıkarılıyor, sonra yapay zekâ eksikleri zenginleştiriyor.
Yeni ve Geliştirilenler › İntranet: Dünya saati widget''ı.
Yeni ve Geliştirilenler › İntranet: Foto, video ve ilanlar için onay akışı: Kullanıcı yüklüyor, yönetici onaylıyor ya da gerekçesiyle reddediyor. Onaylanmamış içerik yalnız yükleyene ve yöneticiye görünüyor.
Yeni ve Geliştirilenler › İntranet: Videolar için ayrı sayfa.
Mevcut fotoğraf ve videolar "onaylı" sayılıyor. Yani güncellemeden sonra galeriniz olduğu gibi görünmeye devam ediyor; hiçbir içerik onay bekler duruma düşmüyor.
Yeni ve Geliştirilenler › Arayüz: Sık kullandığınız ürünleri alt çubuğa sabitleyebiliyorsunuz.
Yeni ve Geliştirilenler › Arayüz: Tablolarda donmuş bölme: Sola sabitlenen sütunlar yatay kaydırmada yerinde kalıyor.
Yeni ve Geliştirilenler › Arayüz: Onay zinciri görünümü ve tema düzeltmeleri.
## Hata Düzeltmeleri
Yeni ve Geliştirilenler › İntranet: Grup bazlı görünürlük (kimler görecek) artık anlık çözülüyor; grup üyeliği değiştiğinde içerik hemen doğru kişilere görünüyor.
Yukarıdaki onay akışının veritabanı alanları mevcut kurulumlar için hazırlanmamıştı. Güncelleme sonrası fotoğraf galerisi ve video listesi hata verip açılmayacaktı.
Sistem bunu kendisi yakaladı: 1.1.26''da eklenen açılış denetimi eksik alanları bildirdi ve sürüm çıkmadan kapatıldı.
Düzeltilen Hatalar › Diğer: Bildirim sekmesindeki sayı ile zil rozetindeki sayı farklı olabiliyordu; artık ikisi aynı şeyi anlatıyor.
Düzeltilen Hatalar › Diğer: Bir süreçte döngü bulunması yayınlamayı ENGELLİYORDU. Döngü her zaman hata değildir — artık uyarı olarak gösteriliyor ve yayınlamayı durdurmuyor.
Düzeltilen Hatalar › Diğer: Örnek veriyle kurulan bazı sistemlerde DÖF kayıtları proje ve klasör bağı olmadan oluşuyor, sonuçta ekranda hiç görünmüyordu.
Düzeltilen Hatalar › Diğer: Akışın nereden başlatıldığı bilgisi kayda işlenmiyordu; artık damgalanıyor ve analizlerde kullanılabiliyor.', N'Ara Sürüm', 1, '2026-08-17 09:00:00', '2026-08-17 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.28')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.28', '2026-08-17', '2026-08-17', N'## Yenilikler
Müşteri Maddeleri › Doküman kodu ve klasör kodu: Kod deseni doküman tipinin kendi kodundan okunuyor; "PR-###" gibi bir maske tanımlıysa hane sayısı ona göre ayarlanıyor.
Müşteri Maddeleri › Doküman kodu ve klasör kodu: Klasör kodu yeni klasör açarken kendiliğinden geliyor ve ağaçtaki yerinize göre kırılıyor: En üstte "0", altında "0-1", onun altında "0-1-1". "0-2" klasörünün altına yeni klasör açarsanız "0-2-1" öneriliyor.
Müşteri Maddeleri › Doküman kodu ve klasör kodu: Önerilen klasör kodunu değiştirebiliyorsunuz; alan kilitli değil, kendi kodlama düzeninizi yazabilirsiniz.
Müşteri Maddeleri › Klasör devralma ve kilitler: "Her şeyi klasörden dayat" seçeneği eklendi: Tek kutuyla devralınabilen bütün alanlar birden kilitlenir. Tek tek kilit seçmek de mümkün; ikisi birlikte de kullanılabilir.
Müşteri Maddeleri › Gözden geçirme ve onay davranışı: Klasör gözden geçirme ayarına "ZORUNLU" seçeneği eklendi. Varsayılan seçenekte hazırlayan gözden geçirmeyi kapatabiliyor; zorunlu seçildiğinde KAPATAMIYOR, yalnızca periyodu ve kişiyi değiştirebiliyor.
Müşteri Maddeleri › Gözden geçirme ve onay davranışı: "Kendi dokümanını onaylayabilir" izni ayara bağlandı. Üç seçenek var: Şirket ayarını kullan, yasak, serbest. Klasör bazında da belirlenebiliyor.
Müşteri Maddeleri › Gözden geçirme ve onay davranışı: Word içerik düzenlemesinde değişiklik takibi (track changes) desteği.
Müşteri Maddeleri › Yetkilendirme: Bir kullanıcıyı gruba eklemek artık TEK BAŞINA yetiyor: Grup kendi yetki grubunu taşıyabiliyor ve kullanıcı gruba eklendiği anda o grubun ekranlarını görüyor. Önceden araya ayrı bir eşleme satırı gerekiyordu; o satır unutulunca kullanıcı hiçbir yetkiye bağlanamıyordu.
Müşteri Maddeleri › Yetkilendirme: Yetki grubuna atanabilir ekran listesine Çöp Kutusu ve Destek Kuyruğu eklendi.
Müşteri Maddeleri › Kullanım kolaylığı: Klasör tanımlama ve doküman hazırlama ekranlarındaki yatay kaydırma çubukları kaldırıldı. Tablo hücreleri artık satır sarıyor ve düzen dar ekranda tek sütuna iniyor; içerik ekrana sığıyor.
Müşteri Maddeleri › Kullanım kolaylığı: Onay kutusu içeren bütün alanlara "tümünü seç" eklendi (doküman hazırlama, klasör tanımlama, ayarlar ve genel tanımlar dahil). Kutu üç durumlu: Hiçbiri, bazıları, hepsi. "Bazıları" durumundayken tıklamak seçimi silmiyor, tamamlıyor.
Ekranda Kimlik Gösterimi: PDF çıktılarına ve kilitli Word üstbilgisine ham personel/grup referansı basılabiliyordu; artık ad yazılıyor.
Doküman Hazırlama › Yalancı ekranlar ve sessiz veri kaybı: Tarama Merkezi''nde girilen metadata "Kaydet"e basılmadan başka bir evrağa geçilince ya da birime aktarılınca uyarısız siliniyordu. Alanlar artık yazdıkça kaydediliyor; "Kaydet" düğmesi yalnız zorunlu alanları denetliyor.
Güvenlik ve Yetki Sertleştirmesi: ⚠ "Kullanıcı yetki tanımlama" satırını pasife çekmek ya da silmek yetkiyi kaldırmıyor, tam erişim veriyordu. Yani yöneticinin kısıtlama hareketi yetkiyi genişletiyordu: O gruptaki herkes bir sonraki sayfa yüklemesinde Ayarlar ve Genel Tanımlar dahil bütün ekranları görüyordu. Artık kısıtlıyor.
Güvenlik ve Yetki Sertleştirmesi: Aynı hareket bir seviye yukarıda TAM KARARTMA yapıyordu. İki seviye artık aynı sonucu veriyor.
Güvenlik ve Yetki Sertleştirmesi: Klasör yetki matrisinde bir satır hiçbir sistem hesabına çözülemediğinde artık uyarı çıkıyor ve o satırların yetki VERMEDİĞİ söyleniyor; uyarıda kişilerin adları yazıyor.
Kalite ve Test: Otomatik test sayısı bu sürümde belirgin şekilde arttı: Doküman modülü 1230, arayüz 1183 test. Yeni eklenen her kapı, kaldırıldığında testin kırmızıya döndüğü doğrulanarak yazıldı.
Kalite ve Test: Kimlik gösterimi, çözücü beslemesi ve ana ekran bağlantıları için kaynak denetleyen kapılar eklendi: Aynı kusurlar sessizce geri gelemiyor.
Kalite ve Test: Doküman hazırlama ve ana ekran alanları dokuz ayrı mercekten taranıp bulunan her kusur canlı veriyle doğrulandı.
## Değişiklikler
Müşteri Maddeleri › Doküman kodu ve klasör kodu: Doküman kodu artık KLASÖRDEN değil DOKÜMAN TİPİNDEN üretiliyor. Doküman tipini seçtiğinizde kod alanı o tipin sıradaki numarasını gösteriyor (Prosedür için PR-001, PR-002; Talimat için TL-001 gibi).
Müşteri Maddeleri › Revizyon numarası ve hazırlayan bilgisi: Revizyon numarası YALNIZCA dokümanı ilk oluştururken elle yazılabiliyor. Gerekçesi dışarıdan gelen belgeler: Başka bir sistemde hazırlanmış ve örneğin 5. Revizyonunda olan bir dokümanı içeri alırken numarayı doğru girebiliyorsunuz. Doküman bir kez yayınlandıktan sonra alan kapanıyor.
Müşteri Maddeleri › Revizyon numarası ve hazırlayan bilgisi: "Hazırlayan" alanı artık sunucuda damgalanıyor ve değiştirilemiyor: Her zaman taslağı ilk açan kişidir. Başkasının adına doküman hazırlanamıyor.
Müşteri Maddeleri › Revizyon numarası ve hazırlayan bilgisi: "Hazırlama tarihi" de sunucuda damgalanıyor ama taslaktan KORUNUYOR: Taslağa günler sonra devam etseniz bile ilk açılış tarihi kaymıyor.
Müşteri Maddeleri › Klasör devralma ve kilitler: Klasör değiştirdiğinizde klasörden gelen ayarlar hazırlama ekranlarının HEPSİNDE yeniden yükleniyor. Önceden eski klasörün değerleri ekranda kalabiliyordu.
Müşteri Maddeleri › Klasör devralma ve kilitler: Klasör seçicisi artık ağaç görünümünde ve kırılımlı: Klasörü hiyerarşi içinde görerek seçiyorsunuz, düz bir listeden değil.
Müşteri Maddeleri › Yetkilendirme: "Ayarlar" ve "Genel Tanımlar" ekranları artık yetki grubuna atanabiliyor — admin dışındaki kişilere de verilebiliyor. Önceden atansa bile yok sayılıyordu.
## Hata Düzeltmeleri
Müşteri Maddeleri › Klasör devralma ve kilitler: Klasörde seçilen DOKÜMAN TİPİ artık gerçekten dokümana yansıyor ve kilitlenebiliyor. Önceden en yaygın tip olan "Prosedür" değeri "karar verilmedi" sayıldığı için devralma ve kilit tam da en sık kullanılan durumda sessizce çalışmıyordu.
Müşteri Maddeleri › Klasör devralma ve kilitler: Klasör ekranı seçili klasörün doküman tipini doğru yansıtıyor.
Müşteri Maddeleri › Gözden geçirme ve onay davranışı: Onay zincirindeki görev ayrılığı denetimi de bu ayara bağlandı. Önceden ayar açılsa bile onay adımında ikinci bir kapı kişiyi durduruyordu — yani ayarın hiçbir etkisi olmuyordu.
Müşteri Maddeleri › Yetkilendirme: Grup onaycısı hiç çözülmüyordu. Kişi seçici grubu kimliğiyle saklarken onay denetimi grup ADIYLA karşılaştırma yapıyordu; bu yüzden onaycısı GRUP olan aşamalarda kimse onaylayamıyordu. Artık grup üyeleri gerçek kullanıcılara açılıyor.
Müşteri Maddeleri › Yetkilendirme: Bir kişiye ya da gruba modül yetkisi verildiğinde ana ekranda o modüller görünmüyordu. Aynı kural üç ayrı yerde yazılmıştı ve biri kuralı hiç uygulamıyordu; tek kaynağa indirildi.
Ekranda Kimlik Gösterimi: KYS''nin hiçbir yerinde artık kullanıcı, grup veya rol kimliği (uzun kod) görünmüyor; her yerde ad yazıyor. Canlı veride doküman listesindeki her satır ve kontrollü kopya kayıtlarının tamamı kimlik gösteriyordu.
Ekranda Kimlik Gösterimi: Grup adları artık herkese çözülüyor. Önceden grup listesi yalnız yöneticiye açık bir yerden okunuyordu; yönetici olmayan kullanıcıda liste boş kalıyor ve her grup kısaltılmış bir kod olarak görünüyordu.
Ekranda Kimlik Gösterimi: Kontrollü Kopya ekranı kendi dar çözücüsünü tutuyordu ve üç ayrı biçimde kimlik sızdırıyordu; ortak çözücüye bağlandı.
Ekranda Kimlik Gösterimi: Ad çözülemediğinde artık kısaltılmış kod basılmıyor, insan dilinde bir metin yazıyor ("Tanımsız kullanıcı" gibi). Kırpılmış bir kod da kullanıcıya hiçbir şey anlatmıyor.
Ekranda Kimlik Gösterimi: İmha tutanağı ve kontrolsüz kopya filigranı, ad bilgisi okunamadığında veritabanına kimlik yazıyordu. Yazma anında kaybedilen ad sonradan geri getirilemediği için düzeltme kaydeden tarafta yapıldı.
Doküman Hazırlama › Düzeltilen hatalar: Kalıcı kilit: Onay adımına yalnızca kendinizi eklerseniz doküman ne onaylanabiliyor ne reddedilebiliyordu — kayıt sonsuza kadar onayda kalıyordu. Artık onaya gönderirken uyarı alıyorsunuz ve nasıl çözeceğiniz yazıyor. (Küçük ekipler için klasör ayarından kendi onayına izin verilebiliyor.)
Doküman Hazırlama › Düzeltilen hatalar: Revizyon çıkmazı: Bir revizyon taslağını listeden yeniden açıp kaydettiğinizde revizyon bağı sessizce siliniyordu. Sonrasında ya "bu doküman kodu zaten kullanımda" hatasıyla hiç gönderemiyordunuz ya da revizyon ayrı kodlu ikinci bir doküman olarak yayınlanıyor ve geçmiş ikiye bölünüyordu.
Doküman Hazırlama › Düzeltilen hatalar: Yanlış metin yayını: Reddedilen bir dokümanı düzeltip yeniden gönderdiğinizde düzeltmeleriniz yayınlanacak belgeye işlenmiyordu; doküman DÜZELTME ÖNCESİ metinle yürürlüğe giriyordu. Hazırlayan ekranda düzeltilmiş metni görmeye devam ettiği için kayıp fark edilmiyordu.
Doküman Hazırlama › Düzeltilen hatalar: Yürürlükteki bir dokümanın taslağı sihirbazdan yeniden "Onaya gönder"e verilebiliyordu: Doküman yürürlükte kalırken sahte bir onay zinciri açılıyor ve onaycıların görev kutusunda gerçek bir yayına karşılık gelmeyen görev üretiliyordu. Artık engelleniyor.
Doküman Hazırlama › Düzeltilen hatalar: Onay kararı ile dokümanın yayınlanması ayrı ayrı kaydediliyordu. Yayın adımı yetki denetimine takılırsa zincir kapanıyor ama doküman yayınlanmıyordu; kayıt bütün kuyruklardan düşüyor, kimseye bildirim gitmiyor ve tekrar denemek de mümkün olmuyordu. İkisi artık tek işlemde yapılıyor: Biri olmazsa diğeri de geri alınıyor ve gerçek hata mesajını görüyorsunuz.
Doküman Hazırlama › Düzeltilen hatalar: İki onaycı neredeyse aynı anda onayladığında aşama hiç geçmiyor ve bir daha da geçemiyordu. Kararlar artık sıraya alınıyor.
Doküman Hazırlama › Düzeltilen hatalar: Hassas belgelerin ekleri indirilemiyordu: İndirme isteği yetkisiz gidiyor, dosya inmiyor ve kullanıcı hiçbir hata mesajı görmüyordu.
Doküman Hazırlama › Düzeltilen hatalar: İmha icrası ek dosyaları gerçekte silmiyordu. Tutanak "N ek dosya imha edildi" beyan ederken dosyaların baytları depoda kalmaya devam ediyordu; artık gerçekten siliniyor ve sayaç yalnız silme başarılıysa artıyor.
Doküman Hazırlama › Yalancı ekranlar ve sessiz veri kaybı: Değişiklik Yönetimi yayınlama ekranındaki dört alan (yayınlayan, geçerlilik tarihi, dağıtım listesi, bildirim tercihi) hiçbir yere kaydedilmiyordu. Ekran ayrıca "yeni revizyon aktif oldu ve dağıtım listesine bildirim gönderildi" diyordu; ikisi de olmuyordu. Alanlar artık kaydediliyor ve ekran ne yaptığını doğru anlatıyor.
Doküman Hazırlama › Yalancı ekranlar ve sessiz veri kaybı: Görevlerim ekranındaki dört kuyruk (onay, tebliğ, gözden geçirme, erişim talebi) sunucu hatasını "kayıt yok" olarak gösteriyordu. Onaycı, bildirime tıklayıp "onayınızı bekleyen doküman yok" görüyor ve kuyruğun boş olduğuna kanaat getiriyordu. Artık hata ayrı gösteriliyor ve "tekrar dene" düğmesi çıkıyor.
Ana Ekran › Düzeltilen hatalar: Termin gün sayısı bir gün eksik hesaplanıyordu: Termini BUGÜN olan bir kayıt gün içinde "1 gün geçti" yazıyor, kırmızı yanıyor ve "Termini Geçmiş" grubuna düşüyordu. Sistemin gönderdiği hatırlatma e-postası ise doğru günü söylüyordu; iki taraf artık aynı dili konuşuyor.
Ana Ekran › Düzeltilen hatalar: Kırıntı çubuğundaki zaman etiketi beş gruptan üçünü yanlış yazıyordu: "Bu Hafta", "İleri Tarihli" ve "Termin Tarihi Olmayan" seçildiğinde ekran "Bugün Son Gün" diyordu.
Ana Ekran › Düzeltilen hatalar: Üstteki eylem rozetleri seçili kuyruğu hesaba katmıyordu: Rozet bir sayı gösteriyor, tıklayınca liste boş çıkıyordu.
Ana Ekran › Düzeltilen hatalar: Profilim ekranındaki "Modül Erişimleri" listesi ana ekrandan farklı bir kural kullanıyordu; "Doküman İşlemleri" ve "Görevlerim" en geniş yetkideki kullanıcıda bile kilitli görünüyordu. Kullanıcı yöneticiden yetki isteyince yönetici de veremiyordu.
Ana Ekran › Düzeltilen hatalar: Evrak Çantam''daki "Devam et" düğmesi taslağı açmıyor, boş bir sihirbaz açıyordu. Orada doldurup kaydeden kullanıcı aynı iş için ikinci bir taslak üretiyordu.
Ana Ekran › Düzeltilen hatalar: Katman içindeki bir formda açık bir seçici listesini kapatmak için ESC''e bastığınızda tüm katman kapanıyor ve doldurduğunuz form uyarısız siliniyordu. ESC artık önce açık listeyi kapatıyor.
Ana Ekran › Düzeltilen hatalar: Katman açıkken ana ekranın her yenilenmesinde imleç yazdığınız alandan kaçıyordu; odak artık yalnız katman ilk açıldığında alınıyor.
Güvenlik ve Yetki Sertleştirmesi: Yetki bilgisi okunamadığında (geçici sunucu hatası, oturum yenilenememesi) ekran bütün menüyü ve modül kartlarını AÇIK gösteriyordu; tıklayan kullanıcı yetki hatasına çarpıyordu. Hata artık kapıyı kapatıyor ve durumu söylüyor.
Güvenlik ve Yetki Sertleştirmesi: Yetki ekranı ile yetki kapısı farklı kaynaklardan okuyordu: Gruba doğrudan yetki verilmiş bir kullanıcıda kapı kısıt uygularken ekran "menü kısıtı uygulanmıyor" yazıyordu. İkisi aynı kaynağa bağlandı.
## Kaldırılanlar
Müşteri Maddeleri › Doküman kodu ve klasör kodu: Klasör ayarlarından "doküman kodu üretim" bölümü kaldırıldı — kod artık doküman tipinden geldiği için o ayarın karşılığı kalmamıştı.
Müşteri Maddeleri › Doküman kodu ve klasör kodu: Klasör ayarlarından "rev no değişim" bölümü kaldırıldı.
Müşteri Maddeleri › Revizyon numarası ve hazırlayan bilgisi: Doküman hazırlama ekranının en üstündeki "doküman / şablon dosyaları" alanı kaldırıldı.', N'Ara Sürüm', 1, '2026-08-17 09:01:00', '2026-08-17 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.29')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.29', '2026-08-20', '2026-08-20', N'## Yenilikler
KYS › Ekran düzeni ve mobil uyum: Mobil ve tablet uyumu: İki kolonlu form alanları dar ekranda tek kolona iniyor, başlık şeridi ve düğmeler alt alta diziliyor, sekmeler kaydırılabiliyor. Önceden sihirbazın form alanları telefonda iki sıkışık sütun olarak kalıyor ve etiketler okunmuyordu.
KYS › Ekran düzeni ve mobil uyum: Tablo hücrelerinde kelimeler harf harf bölünüyordu ("Personel" yerine alt alta "Pe / rs / on / el"). Artık yalnız gerçekten sığmayan kelime bölünüyor.
KYS › Doküman arama: Hiçbir bilgi kaldırılmadı — sütun azaltma bilgi gizlemeye dönüşmesin diye bu otomatik testle güvence altına alındı.
KYS › Word değişiklik izleme: Belgeyi salt okunur açan kişi artık değişiklik listesini GÖREBİLİYOR. Bu izin daha önce yalnız düzenleme modunda açıktı; yani onay verecek kişi "ne değişmiş" sorusunu hiç cevaplayamıyordu.
KYS › Word değişiklik izleme: Her Word kaydetmesi tarihçeye düşüyor: Kim kaydetti ve hangi revizyonda. Revizyon numarası kaydetme anında sabitleniyor — sonradan hesaplansaydı doküman o günden beri revize edildiğinde yanlış numara gösterirdi.
KYS › Word değişiklik izleme: Sürüm listesindeki açıklama artık "OnlyOffice — <kişi> · Rev <no>" biçiminde.
KYS › Word değişiklik izleme: Dışarıdan doküman yüklendiğinde editör hep aynı (eski/boş) belgeyi açıyordu. Editör ayarları içeriğe bağlı olduğu hâlde önbellekte süresiz tutuluyor ve yeni sürüm sonrası tazelenmiyordu.
KYS › Word değişiklik izleme: Dışarıdan doküman yüklendiğinde ekranda hiçbir şey değişmiyordu; yalnız bir bildirim çıkıyordu. Artık yüklenen belge kendiliğinden gösteriliyor.
KYS › Klasör tanımlama: Kaydedilmemiş değişiklik uyarısı. Klasör ayarları yalnız "Kaydet" ile sunucuya gidiyor; paneller arasında gezinmek kaydetmiyordu ve bunu söyleyen hiçbir işaret yoktu.
KYS › Klasör tanımlama: Her devralınan alan için artık ÜÇ seçenek var: Zorunlu (dokümanda gri gelir, değiştirilemez) · Varsayılan (hazır gelir ama hazırlayan üzerine yazabilir) · Boş (klasörden hiç gelmez, doküman özelinde karar verilir). Önceden yalnız tek bir "değiştirilemesin" kutusu vardı, yani iki durum görünüyordu; üçüncüsü ancak klasördeki değeri elle silerek elde edilebiliyordu.
Doküman hazırlama: Klasör seçim listesinde klasör/alt klasör işaretleri: İçinde alt klasör olan klasörler açık klasör simgesiyle, yaprak klasörler kapalı klasör simgesiyle gösteriliyor. Hangi klasörün altında devamı olduğu tek bakışta görünüyor.
Yeni ve Geliştirilenler › PDF şablon tasarımcısı: Şablonlara üst bilgi ve alt bilgi bantları eklenebiliyor; yükseklikleri milimetre olarak ayarlanıyor.
Yeni ve Geliştirilenler › PDF şablon tasarımcısı: Şablon içinde veri kaynağı tanımlanabiliyor — şablon kendi verisini çekip render edebiliyor.
Yeni ve Geliştirilenler › PDF şablon tasarımcısı: Görsel tasarımcı geliştirildi: Blok yerleşimi, tema ve panel düzeltmeleri.
Yeni ve Geliştirilenler › Proje taşıma ve içe aktarma: Proje taşıma paketine proje üyelikleri de giriyor ve hedef kurulumda eşleniyor.
Yeni ve Geliştirilenler › Proje taşıma ve içe aktarma: Rol üyesi hesaplar hedefte otomatik oluşturulabiliyor; katalogların arkasındaki formlar da pakete dahil.
Yeni ve Geliştirilenler › Proje taşıma ve içe aktarma: WPF içe aktarma raporuna bileşen eşleştirme tablosu eklendi (süreç ve form) — hangi kaynak bileşenin neye karşılık geldiği görünüyor.
Yeni ve Geliştirilenler › Proje taşıma ve içe aktarma: Form otomatik numaralandırma artık numarayı gönderim anında tahsis ediyor; numara sürekliliği korunuyor.
Yeni ve Geliştirilenler › Proje taşıma ve içe aktarma: Bütçe modülü örnek verisi zenginleştirildi (bütçeler, ana veriler, hareketler).
Arayüz iyileştirmeleri: Tablolarda sabit başlık, yatay kaydırma göstergesi ve kenar solma efekti.
Arayüz iyileştirmeleri: Mongo bağlantı kurucusunda çoklu seçim listesi.
Kalite ve Test: Otomatik test sayısı arttı: Doküman modülü 1253, arayüz 1230, süreç motoru 132.
Kalite ve Test: Ekran düzeni artık testle korunuyor: Sütun sayısı tavanı, başlık ile boş-satır genişliğinin eşitliği, birleştirilen alanların geri ayrılmaması, sayfa genişliğinin daralmaması ve mobil kuralların varlığı.
## Değişiklikler
KYS › Ekran düzeni ve mobil uyum: KYS iç ekranları artık ekranın tamamını kullanıyor. Önceden tüm ekranlar 1340 piksele sıkıştırılıyordu; bu ölçü düz metin okumak için doğru ama bu ekranlar tablo ve matris. Geniş bir monitörde sağda yüzlerce piksel boş dururken tablo yatay kaymaya zorlanıyordu.
KYS › Ekran düzeni ve mobil uyum: Kenar boşlukları ekran genişliğiyle birlikte daralıyor. Sabit boşluk telefonda içeriğin yarısını yiyordu.
KYS › Doküman arama: Sonuç tablosu on üç sütundan sekize indi. Sütun daraltılmadı; birlikte okunan bilgiler tek hücrede toplandı: Doküman kodu + adı + klasör yolu bir arada, revizyon + durum bir arada, hazırlayan + revize eden bir arada, gizlilik + fiziki arşiv bir arada.
KYS › Doküman arama: Uzun klasör yolları tek satırda kırpılıyor, tamamı üzerine gelince görünüyor. Sarmasına izin verilseydi satır üç katına çıkıyordu.
KYS › Word değişiklik izleme: Word içindeki değişikliklerin yanında görünen ad artık kişinin gerçek adı (kullanıcı adı değil). Bu ad belgenin içine kalıcı olarak yazıldığı için sonradan düzeltilemiyor; bu yüzden öncelikli olarak yapıldı.
KYS › Klasör tanımlama: Yetki matrisi okunur hâle geldi: Kullanıcı adı ve tipi tek hücrede birleşti, sütun başlıkları kısaldı (tam adı üzerine gelince görünüyor), her yetki sütununun başlığına "tümünü seç" eklendi ve kullanıcı sütunu yatay kaydırmada sabit kalıyor. Önceden sekizinci sütuna geldiğinizde kimin yetkisini işaretlediğiniz görünmüyordu.
KYS › Klasör tanımlama: Yetki matrisinin altındaki iki uzun açıklama tek nota indi. Henüz bir davranış üretmeyen üç yetki bitini artık başlıktaki "°" işareti gösteriyor.
KYS › Klasör tanımlama: "Boş" seçmek klasördeki değeri de temizler. Aksi hâlde kilit kalkar ama değer akmaya devam eder ve seçim yanıltıcı olurdu. "Zorunlu" ile "Varsayılan" arasında gidip gelmek değeri korur.
Doküman hazırlama: Referans doküman seçimi artık liste hâlinde açılıyor. Önceden en az iki karakter yazmak gerekiyordu, yani dokümanın kodunu ya da adını önceden bilmek zorundaydınız. Şimdi alana tıklamak yayınlanmış dokümanları listeliyor; yazmak listeyi daraltıyor.
Doküman hazırlama: Kullanıcı/grup seçicilerinde de aynı düzeltme: Liste tıklayınca açılıyor, isim hatırlamak gerekmiyor. Büyük kurumlarda liste sınırlı geliyor ve devamı için "yazarak daraltın" uyarısı çıkıyor.
Doküman hazırlama: İndirilen dosyaların adı artık doküman kodu, adı, revizyonu ve tarihi taşıyor: PR-001_Doküman_Kontrol_Prosedürü_Rev03_2026-08-19.pdf. Word ve PDF aynı biçimi kullanıyor. Önceden Word indirmesi ham dosya adıyla iniyordu ("KAL-000.docx"); aynı dokümanın iki revizyonu masaüstünde aynı adla inip birbirinin üstüne yazıyordu.
## Hata Düzeltmeleri
Doküman hazırlama: Doküman kodu, tip KLASÖRDEN devralındığında yanlış üretiliyordu. Klasörde doküman tipi tanımlı olsa bile kod "DOC-001" olarak çıkıyor, tipin kendi ön eki (PR, TL, FR…) kullanılmıyordu. Sebep: Devralınan tip taslağa bilinçli olarak boş yazılıyor ve kod üreticisi bu devralmayı çözmüyordu. Artık klasör zincirinden çözülüyor; dokümanın kendi tipi varsa o kazanmaya devam ediyor.
Yeni ve Geliştirilenler › Proje taşıma ve içe aktarma: Katalog kısa-ad eşlemesi artık form ve süreç tanımlarına da uygulanıyor; taşınan projede kırık katalog bağı kalmıyor.
Arayüz iyileştirmeleri: Süreç tasarımcısı, gösterge tasarımcısı ve genel şablon/tema düzeltmeleri.
Kalite ve Test: Word/PDF yolunu koruyan otomatik kontrol (self-test) çalışmıyordu; silinmiş bir klasöre bağlı olduğu için her çağrıda hata veriyor, hata da kurulum verisine ait göründüğü için fark edilmiyordu. Onarıldı ve "dışarıdan doküman alma" turu eklendi.
## Kaldırılanlar
KYS › Klasör tanımlama: "Kontrol matrisi" paneli kaldırıldı. Kayıtlı veriler korunuyor: Daha önce tanımlanmış kontrol matrisleri dokümanlara aktarılmaya devam ediyor.
KYS › Klasör tanımlama: Kilit listesindeki "Kontrol matrisi" satırı da kaldırıldı — artık var olmayan bir adıma yönlendiriyor ve kilitlense bile değeri hiçbir yerde girilemiyordu.
KYS › Klasör tanımlama: Onay matrisindeki "Onaycı rol", "Üst unvan sınırı", "Alt unvan sınırı" ve "Tersten çalışsın" alanları kaldırıldı. Bu alanlar hiçbir işe yaramıyordu: Onay zinciri aşamaları yalnız kişi/grup matrisinden kuruluyordu ve ekranın kendi uyarısı da "bu kısıt yalnız tanım olarak saklanıyor" diyordu. Artık yalnız onay sırası (3-2-1-0, sıfır son onaycı) var. Kayıtlı değerler silinmedi.', N'Ara Sürüm', 1, '2026-08-20 09:00:00', '2026-08-20 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.30')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.30', '2026-08-25', '2026-08-25', N'## Yenilikler
KYS › Word''de düzenleme: Belge açıldığı hâlde hiçbir şey yazılamıyordu. Düzenleme yetkiniz varsa belge artık gerçekten düzenlenebiliyor — yeni doküman oluştururken de.
KYS › Word''de düzenleme: Düzenleyicide oluşan hata ve uyarılar başlık çubuğunda görünüyor. Önceden ekranda hiçbir işaret çıkmıyor, sorun sessizce yutuluyordu.
KYS › Word''de düzenleme: Bir belge salt okunur açıldığında nedeni başlıkta yazıyor: Doküman kilitli mi, yoksa o klasörde hazırlama yetkiniz mi yok.
KYS › Word''de düzenleme: Başlıkta "değişiklik algılandı" bilgisi eklendi; yazdıklarınızın belgeye ulaştığını buradan görüyorsunuz.
KYS › Değişiklik geçmişi: Word''de hazırlanan dokümanlar için "Değişiklik geçmişi" paneli eklendi. Her Word kaydı ayrı bir sürüm olarak saklanıyor; bir satıra tıkladığınızda hangi paragrafta ne değiştiği kelime kelime, kim ve hangi revizyon bilgisiyle görünüyor.
KYS › Değişiklik geçmişi: Panel üç yerden açılabiliyor: Doküman Hazırlama''nın Word adımı, revizyon sihirbazı ve doküman detayının Akış/Tarihçe sekmesi.
KYS › Onay akışı: Onayda bekleyen dokümanın detayında "şu an kimde bekliyor" bilgisi görünüyor: Onaycı adları, kaçıncı adımda olunduğu ve kaç onay verildiği.
KYS › Onay akışı: Onay görev ekranında sizden sonra hangi adımın ve kimlerin geleceği yazıyor; son aşamadaysanız onayladığınızda dokümanın yürürlüğe gireceği ayrıca uyarılıyor.
KYS › Onay akışı: Doküman yayınlandığında ona bağlı değişiklik talebi kendiliğinden Yayın aşamasına geçiyor; talepler kuyrukta açık kalmıyor.
KYS › Onay akışı: Değişiklik talebini onayladığınızda sıra revizyona geldiyse doküman sihirbazı bilgileri dolu olarak açılıyor; gelmediyse sıradaki aşamanın adı bildiriliyor.
KYS › Doküman Hazırlama ekranı: Doldurulması zorunlu boş alanlar kırmızı çerçeve ve "Zorunlu" rozetiyle işaretleniyor.
KYS › Doküman Hazırlama ekranı: Klasör tanımından dolan alanlar mor şerit ve "Klasörden" rozetiyle, klasörde kilitlenmiş alanlar "Klasör ayarı" rozetiyle ayırt ediliyor.
KYS › Doküman Hazırlama ekranı: Her adımın başında kaç zorunlu alanın eksik olduğu ve bu alanların adları listeleniyor.
KYS › Klasör yetkileri: Klasör Tanımlama yetkisi olan kullanıcılar yetki matrisini değiştirebiliyor; bu iş artık yalnızca yöneticiye kilitli değil.
KYS › Listeler ve tarihçe: Arama listesinde onay bekleyen dokümanların tarih sütununda yayın tarihi "önceki: …" olarak gösteriliyor; yeni revizyonun henüz onaylanmadığı böylece anlaşılıyor.
KYS › Listeler ve tarihçe: Doküman detayındaki bölüm başlıkları düz metin gibi görünüyordu; artık başlık biçiminde çiziliyor.
## İyileştirmeler
KYS › Word''de düzenleme: Kabul et / reddet paneli kaldırıldı. Yazdıklarınız doğrudan belgeye işleniyor; neyin değiştiğini Değişiklik geçmişi panelinden görüyorsunuz.
KYS › Word''de düzenleme: Kapatırken kayıt tamamlanmadıysa "değişiklikler hâlâ kaydediliyor" uyarısı çıkıyor ve kapatma düğmesi kayıt bitene kadar bekliyor.
KYS › Değişiklik geçmişi: Geçmiş salt okunurdur; kimse bir kabul veya ret hareketiyle izi silemez.
KYS › Doküman kodu: Paylaşım bağlantısı için dokümanı onaya göndermek gerekmiyor; taslağı kaydettikten sonra bağlantı oluşturulabiliyor.
KYS › Doküman Hazırlama ekranı: Klasörden gelen alanların altındaki açıklamalar belirginleşti; "değiştirilemez" ile "isterseniz değiştirebilirsiniz" ayrı renklerde görünüyor.
KYS › Klasör yetkileri: Yetki matrisini değiştirme yetkiniz yoksa tablo baştan salt okunur geliyor ve nedeni açıklanıyor; kaydete basıp reddedilmeyi beklemek gerekmiyor.
KYS › Listeler ve tarihçe: KYS''ye her girişte sekmeler temiz açılıyor; önceki oturumdan kalan sekmeleri tek tek kapatmanız gerekmiyor.
KYS › Bilinen konular: 1.1.29''da duyurulan "belgeyi salt okunur açan kişi Word''ün değişiklik listesini görebiliyor" maddesi geçerliliğini yitirdi. Değişiklik izleme kapatıldı; onaycı "ne değişmiş" sorusunu artık Değişiklik geçmişi panelinden yanıtlıyor.
## Hata Düzeltmeleri
KYS › Word''de düzenleme: Word''de belge "Hakkınız olmayan bir eylem gerçekleştirmeye çalışıyorsunuz" hatası verip hiç açılmıyordu; giderildi.
KYS › Word''de düzenleme: Kendi hazırladığınız taslağın Word içeriğini, klasör yetkiniz sonradan değişmiş olsa bile düzenleyebiliyorsunuz.
KYS › Word''de düzenleme: Word''de düzenleme yapıp kapattığınızda değişiklikler kapanmadan kaydediliyor; önizlemede belgenin eski hâlinin görünmesi sorunu giderildi.
KYS › Doküman kodu: Doküman kodu taslağı kaydettiğiniz anda oluşuyor. Hazırlama listesinde, sihirbazda ve Word antetinde kod artık boş kalmıyor.
KYS › Doküman kodu: Ekranda gösterilen sıradaki kod ile kaydedince oluşan kod aynı; iki farklı taslağın aynı kodu görmesi sona erdi.
KYS › Doküman kodu: Revizyon açtığınızda doküman kodu değişmiyor; sihirbaz, arama ve Word anteti aynı kodu gösteriyor.
KYS › Doküman kodu: Klasör değiştirdiğinizde daha önce atanmış doküman kodunun ekrandan silinmesi giderildi.
KYS › Doküman kodu: Aynı doküman kodunun ikinci kez verilmesi engellendi; silinmiş bir taslağın kodu da yeniden dağıtılmıyor.
KYS › Onay akışı: Onay verirken karşılaşılan hata giderildi; onay ve ret kararları sorunsuz kaydediliyor.
KYS › Onay akışı: Klasörün onay matrisinde onaycı olarak tanımlanan kişi, yetki matrisinde ayrıca onay kutusu işaretli olmasa da dokümanı onaylayabiliyor.
KYS › Onay akışı: Onayınıza düşen bir dokümanı, o klasörde görüntüleme yetkiniz olmasa bile açıp okuyabiliyorsunuz.
KYS › Klasör yetkileri: Yetki matrisi kaydedilemediğinde yaptığınız işaretlemeler ekranda korunuyor; eskiden hepsi kayboluyordu.
KYS › Klasör yetkileri: Yetki matrisinde silinmiş kullanıcı veya grup satırları uyarı olarak bildiriliyor. Tek bir ölü satırın klasörü farkında olmadan herkese kapatması engellendi.
KYS › Revizyon ve iptal: Revizyon ekranında doküman önizleme ve indirme sırasında alınan hata giderildi; filigranlı ve künyeli PDF''ler düzgün üretiliyor.
KYS › Revizyon ve iptal: "Revizyona başla" düğmesi kapalıyken gerçek gerekçesi yazıyor; doküman onay sürecindeyse bu açıkça bildiriliyor.
KYS › Revizyon ve iptal: Revizyon talepleri listesindeki "Revizyona başla" düğmesi üstteki Revize/İptal seçiminden etkilenmiyor ve her zaman revizyon sihirbazını açıyor.
KYS › Listeler ve tarihçe: Akış tarihçesinde henüz onaylanmamış revizyonlar "yayınlandı" yerine "açıldı — onay bekliyor" olarak görünüyor.
KYS › Listeler ve tarihçe: Akış tarihçesindeki açıklamalarda ham teknik metin ve kimlik numaraları yerine okunur bilgi gösteriliyor: Dosya adı, revizyon ve neden.
KYS › Listeler ve tarihçe: Onaylanmamış dokümanlar Word görünümünde de uyarı şeridiyle işaretli kalıyor.
KYS › Bilinen konular: DKYS modülündeki Word düzenleyici bu sürümde açılmıyor. KYS tarafındaki Word düzenleme tam çalışıyor; DKYS''nin kendi düzenleyicisi bir sonraki sürümde ele alınacak.', N'Ara Sürüm', 1, '2026-08-25 09:00:00', '2026-08-25 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.31.2')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.31.2', '2026-08-28', '2026-08-28', N'## Yenilikler
KYS › Vukuat (Uygunsuzluk): Uygunsuzluk kayıtları için ayrı bir modül eklendi: Kendi ana ekranı, işlem panosu, talep formu, bloke kararı kuyruğu, grafikler ve tanımlar ekranı.
KYS › Vukuat (Uygunsuzluk): Kayıt altı aşamada ilerliyor: Bloke kararı → Talep → Aksiyon → Kontrol → Onay → Kapandı. Kartı sürükleyerek aşama ilerletebiliyorsunuz.
KYS › Vukuat (Uygunsuzluk): Her kaydın kritikliği etki, tekrar sayısı ve bloke edilen miktardan hesaplanıyor; kritikliğe göre 3/7/14/30 günlük termin bloke kararı anında damgalanıyor.
KYS › Vukuat (Uygunsuzluk): Aynı bölüm, tip ve hat birleşimi 90 gün içinde üçüncü kez tekrarlarsa kayıt vukuat olarak ilerletilemiyor; önce bir DÖF kararı vermeniz gerekiyor.
KYS › Vukuat (Uygunsuzluk): Blokeye alınan ürün için karantina etiketi PDF olarak üretiliyor. Etiketteki QR okutulduğunda doğrudan ilgili kayıt açılıyor.
KYS › Vukuat (Uygunsuzluk): F.10.04 uygunsuzluk kayıt özeti çıktısı eklendi: Künye, bloke kararı, sorumluluk, DÖF bağı, aksiyonlar, kontrol sonucu, kayıt geçmişi ve imza bloğu tek belgede. Kaydın her aşamasında alınabiliyor.
KYS › Vukuat (Uygunsuzluk): Grafikler ekranı: Uygunsuzluk tipi Pareto analizi, kritiklik dağılımı, kolon iş yükü ve kontrolden geri dönen kayıtlar.
KYS › Vukuat (Uygunsuzluk): Uygunsuzluk tipi tanımları ekranı eklendi; tipin bölümü, hattı ve kontrol listesi şablonu buradan yönetiliyor. Talep formundaki tip listesi, tekrar imzası ve Pareto ekseni bu tanımdan besleniyor.
KYS › Vukuat (Uygunsuzluk): Benzer kayıt önerisi: Aynı imzayı taşıyan açık kayıtlar ve ilgili DÖF''ler kaydın içinde listeleniyor; mükerrer kayıtları tek kayda birleştirebiliyorsunuz.
KYS › Vukuat (Uygunsuzluk): Kaydın ekleri, notları, aksiyon listesi ve kontrol listesi kayıt içinden yönetiliyor; kontrol eden yetersiz bulduğunda gerekçesiyle sorumluya iade edebiliyor.
KYS › Vukuat ve DÖF bağı: Vukuattan DÖF açılabiliyor: Yeni DÖF açma, mevcut bir DÖF ile ilişkilendirme ya da gerekçeli "DÖF gerekmiyor" kararı.
KYS › Vukuat ve DÖF bağı: Ters yön de eklendi: Kök neden çalışması sahada somut bir uygunsuzluk ortaya çıkardığında DÖF ekranından vukuat açabiliyorsunuz.
KYS › Vukuat ve DÖF bağı: Bağ her iki ekranda da görünüyor: Vukuat kaydında "bu kayıttan açılan DÖF", DÖF kaydında "kaynak vukuat" ve "bu DÖF''ten açılan vukuatlar".
KYS › Vukuat kapanış onayı ve hatırlatma: Kapanış onayında onaylayan kişi artık seçilmiyor, onayı veren kullanıcı olarak sistem tarafından damgalanıyor. Aksiyonu yürüten sorumlu kendi kaydının kapanışını onaylayamıyor.
KYS › Vukuat kapanış onayı ve hatırlatma: Termini geçen açık kayıtlar için otomatik hatırlatma eklendi: Sorumluya ve varsa vekiline bildirim gidiyor, hatırlatma sıklığı Ayarlar''daki hatırlatma periyodundan geliyor.
KYS › Vukuat kapanış onayı ve hatırlatma: Görevlerim ekranına dördüncü vukuat kuyruğu eklendi: "Kapanış Onayı Bekleyen Vukuatlar". Önceden onay bekleyen kayıt hiçbir kuyrukta görünmüyordu.
KYS › Vukuat kapanış onayı ve hatırlatma: Kayıt artık kaydın içinden silinebiliyor; silinen kayıt Pasif Kayıtlar ekranına düşüyor ve oradan geri alınabiliyor.
KYS › Ana ekran: Kişisel dashboard alanınızın kutu sayısını kendiniz belirliyorsunuz (1–6). Kutu sayısını azalttığınızda kalan kutular boşluğu dolduracak şekilde genişliyor.
KYS › Ana ekran: Şablonla dashboard kurmak kutu sayısı tercihinizi sıfırlıyordu; artık şablonun kutu sayısı uygulanıyor.
KYS › Ana ekran: Raporlar ekranındaki vukuat kritiklik ve aşama dağılımları da dashboard kutusu olarak eklenebiliyor.
KYS › Ana ekran: Ayarlar, Genel Tanımlar ve Arşiv kartları arka planı olmadan, okunması güç biçimde çiziliyordu. Ayarlar ve Genel Tanımlar artık gri tonda ve diğer iş modüllerinden ayırt ediliyor.
KYS › Doküman yönetimi: Raporlar bölümüne Doküman Ana Listesi (Masterlist) raporu eklendi: Kod, doküman adı, tip, revizyon, yayın tarihi, durum, departman ve sorumlu tek listede. PDF çıktısı yatay sayfa olarak alınıyor ve hassas belge sayısı listenin altında belirtiliyor.
KYS › DÖF (Düzeltici Faaliyetler): Arama ekranına "Kayıt arama" sekmesi eklendi: Vukuat, DÖF, şikayet ve denetim kayıtlarını numara, konu, tip, müşteri veya sorumluya göre arayabiliyorsunuz. Türkçe harflere duyarsız arama yapıyor.
Sözleşme Yönetimi (CLM): Süreçten sözleşme üretimi eklendi: "Sözleşme Üret" adımı bir sözleşme şablonundan CLM sözleşmesi oluşturuyor.
Sözleşme Yönetimi (CLM): Kayıt eşlemesi hedefleri artık şablondaki yer tutuculardan geliyor; eşlemeyi elle yazmak gerekmiyor.
Sözleşme Yönetimi (CLM): Koşullu ve biçimli madde motoru geldi — sözleşme maddeleri koşula göre metne giriyor ya da çıkıyor.
Sözleşme Yönetimi (CLM): Sözleşme hatırlatıcıları eklendi (yenileme/bitiş tarihleri için).
Eski River (WPF) geçişi: Eski River sürümünden içe aktarma üretim modülü eklendi.
Formlar, süreç ve gösterge panosu: Mongo "Kolay kurucu" filtresine tam operatör seti eklendi (IN / NOT IN dâhil).
Formlar, süreç ve gösterge panosu: SQL ve Mongo sorgu düzenleyicilerine yer tutucu test değerleri paneli eklendi.
Formlar, süreç ve gösterge panosu: Gösterge panosu yapay zekâ bileşenine web araması ve "Kaynakları Göster" açıklaması eklendi.
Formlar, süreç ve gösterge panosu: Kurulumda Keycloak platform kurumu ayarı şablona eklendi.
Formlar, süreç ve gösterge panosu: Belge/PDF ve e-posta tarafında çizim blokları, çoklu gönderen desteği ve sessizce yutulan hataların görünür kılınması.
Formlar, süreç ve gösterge panosu: Satın alma Form Grup turu için platform düzeltmeleri ve demo varlıkları.
Formlar, süreç ve gösterge panosu: Süreç düğüm tipleri sözlüğüne "Sözleşme Üret" adımı eklendi; ön yüze eklenmiş olduğu hâlde yapay zekâ içe aktarma bu tipi tanıyamıyordu.
## İyileştirmeler
KYS › Vukuat ve DÖF bağı: Aynı DÖF''ten aynı imzayla ikinci bir açık kayıt açılması engellendi; farklı hatlarda farklı kayıt açmak serbest.
KYS › Ana ekran: Bekleyen iş listesinde yalnız gerçekten işiniz olan modüller ve kuyruklar görünüyor. Sıfır kayıtlı satırlar kalktı; kuyruğu olduğu hâlde işinizin olmadığı modüller katlanmış bir nota taşındı.
KYS › Ana ekran: Kalite Kokpiti modül kartları arasından çıkarıldı. Ekran, sol menü ve ana ekrandaki kısayolu yerinde duruyor.
KYS › Doküman yönetimi: "Çöp kutusu" ekranının adı Pasif Kayıtlar oldu. Sistemde kalıcı silme olmadığı için eski ad yanlış anlaşılıyor, kullanıcılar silmekten kaçınıyordu. Ekranın adresi ve yetki ayarları değişmedi.
KYS › Doküman yönetimi: Silme onaylarındaki ve bildirimlerdeki "çöp kutusuna taşındı" ifadeleri de yeni ada göre güncellendi (arşiv kutusu, kontrollü kopya ve vukuat kayıtları dahil).
KYS › DÖF (Düzeltici Faaliyetler): Yapay zekâ asistanı artık vukuat sayılarını, termini geçen kayıt sayısını ve en sık görülen uygunsuzluk tipini de biliyor.
KYS › Diğer düzeltmeler: Sürüm notu başlıklarında dört parçalı sürüm numaraları (örneğin 1.1.31.2) artık doğru okunuyor; önceden son parça sessizce düşüyordu.
Eski River (WPF) geçişi: SQL bağlama, XAML''e gömülü sorguları da kurtarıyor; içe aktarma sonunda akıbet raporu üretiliyor.
## Hata Düzeltmeleri
KYS › Ana ekran: Kutu sayısını azaltmak seçtiğiniz bileşenleri silmiyor; sayıyı geri yükselttiğinizde bileşenler yerinde duruyor.
KYS › DÖF (Düzeltici Faaliyetler): Kök Neden Analizi''nde balık kılçığı hazırlarken neden metnini yazacağınız kutu görünmez denecek kadar küçülüyordu; hiçbir neden girilemiyordu. Giderildi.
KYS › DÖF (Düzeltici Faaliyetler): Balık kılçığında kolon başlığının yanındaki silme düğmesi alt satıra düşüyordu; artık başlıkla aynı satırda.
KYS › DÖF (Düzeltici Faaliyetler): Silme düğmeleri sistem düğmesi gibi gri ve iri görünüyordu; arayüzün geri kalanıyla uyumlu hâle getirildi. Aynı düzeltme ek dosya listesi, tarama merkezi kartları ve klasör onay matrisindeki silme düğmelerini de kapsıyor.
KYS › Diğer düzeltmeler: Uygunsuzluk tipi tanımlarında kullanımda olan bir tipin silinmesi engellendi; çöp kutusundaki kayıtlar da sayıma dâhil.
KYS › Diğer düzeltmeler: Vukuat kayıt özeti çıktısı, kayıt verisi beklenmeyen biçimde yazılmış olduğunda hata veriyordu; artık her durumda üretiliyor.
KYS › Diğer düzeltmeler: Aşama adları ekranda ve PDF çıktısında farklı yazılıyordu (örneğin "Onay & Kapanış" yerine "Onay"); ikisi eşitlendi.
KYS › Diğer düzeltmeler: Vukuat kayıtlarına dosya eklerken ve ek silerken kayıt bazlı yetki kontrolü yapılmıyordu; giderildi. Kapanmış kayıtların kanıt dosyaları korumaya alındı.
KYS › Diğer düzeltmeler: Birleştirilmiş bir vukuat kaydı hangi kayda bağlandığını göstermiyordu; kaydın ilişkileri artık tek panelde listeleniyor.
Eski River (WPF) geçişi: İçe aktarma sırasında SQL betiği uygulama ve bağlantı seçimi bilgileri eksik gönderiliyordu; giderildi.
Eski River (WPF) geçişi: Katalogların birbirini beslediği projelerde içe aktarma sırasında bağlar kopuyordu; katalog kimlik eşlemesi düzeltildi.
Kurumlar arası veri sızıntısı kapatıldı; superadmin rolü tek başına yetmiyor, platform kurumuna bağlı olmak gerekiyor.
Kurum kısaltması taşımayan yönetim yolları kapatıldı ve rota kısaltması çapraz kontrolden geçiyor.
Mongo tabanlı gösterge panosu bileşeninde kurum sınırının aşılabildiği durum giderildi.
Formlar, süreç ve gösterge panosu: Formdaki veri tablosu toplamları sonsuz yenileme döngüsü tetikliyordu; giderildi.
Formlar, süreç ve gösterge panosu: Süreçteki e-posta adımına birden çok alıcı eklenebiliyor.
Formlar, süreç ve gösterge panosu: Word editöründe izin uyuşmazlığı ve belge indirmede 401 hatası giderildi.
Formlar, süreç ve gösterge panosu: Girişte "Failed to fetch" hatası giderildi; kimlik sunucusu hazır olana kadar bekleniyor ve Kurum listesi doğru sırayla geliyor.', N'Ara Sürüm', 1, '2026-08-28 09:00:00', '2026-08-28 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.32')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.32', '2026-08-29', '2026-08-29', N'## Yenilikler
KYS › Eğitim ve Yetkinlik: Eğitim ve yetkinlik yönetimi için ayrı bir modül eklendi: Pozisyon yetkinlikleri, çalışan kartı, eğitim planı, polivalans matrisi, sertifika takvimi ve eğitim kayıtları.
KYS › Eğitim ve Yetkinlik: Pozisyon bazlı yetkinlik tanımı: Her pozisyon için gereken yetkinlikler ve seviyeleri tanımlanıyor, çalışanın mevcut durumu bu tanımla karşılaştırılıyor.
KYS › Eğitim ve Yetkinlik: Yetkinlik açığı (gap) analizi: Çalışanın hangi yetkinlikte hangi seviyede eksik olduğu ve bunun hangi eğitimle kapanacağı listeleniyor.
KYS › Eğitim ve Yetkinlik: Polivalans (çok yönlülük) matrisi: İstasyon × çalışan kesişiminde seviye gösteriliyor; bir istasyonu kaç kişinin yapabildiği (yedeklilik) veriden hesaplanıyor.
KYS › Eğitim ve Yetkinlik: Eğitim planı ekranı: Planlanan eğitimler, katılımcılar ve durum takibi (Planlandı / Devam Ediyor / Yapıldı / İptal).
KYS › Eğitim ve Yetkinlik: Sertifika takvimi: Geçerli, yaklaşan ve süresi geçmiş sertifikalar ayrı ayrı görünüyor; kalan gün sayısı her satırda yazıyor.
KYS › Eğitim ve Yetkinlik: Sertifika süresi dolmadan otomatik hatırlatma eklendi. Uyarı eşiği varsayılan 60 gün; Tanımlamalar''dan değiştirilebiliyor.
KYS › Eğitim ve Yetkinlik: Eğitim dokümanı sürüm takibi: Eğitim materyalinin sürümü yükseldiğinde eğitimi alanların yeniden eğitilmesi gerektiği görülebiliyor.
KYS › Eğitim ve Yetkinlik: Yedi yeni katalog eklendi: Yetkinlik, yetkinlik kategorisi, eğitim türü, polivalans seviyesi, istasyon, meslek grubu ve takvim istisnası.
KYS › Denetim: "Yıllık sonuçlar ve senaryo" ekranı eklendi: Yıl seçimi, tamamlanma ve gecikme kartları, uygunluk oranı, bulgu kırılımı, aylık dağılım grafiği, birim ve denetçi kırılımı ile yıllar arası karşılaştırma.
KYS › Denetim: Aynı ekranda yaklaşan denetimler için geçmişe dayalı senaryo tablosu var: Her satır kaç kayıttan hesaplandığını yazıyor ve yeterli geçmiş yoksa sayı yerine sebebini gösteriyor. Ekranın altında, veri olmadığı için üretilemeyen ölçütler gerekçesiyle listeleniyor.
KYS › Denetim: Denetim sorularını başka bir plandan kopyalama eklendi. Aynı numaraya sahip sorular atlanıyor, mevcut sorular hiçbir koşulda ezilmiyor.
KYS › Denetim: Denetim sonucu ilk kez kaydedildiğinde o anki soru listesi kayda saklanıyor; plan soruları sonradan değişse bile eski denetimin cevapları hangi soruya ait olduğunu koruyor.
KYS › DİF Tanımlamalar: DİF sürecinin bütün tanımları tek ekranda toplandı: KYS › DİF ekranının sağ üstündeki ⚙ Tanımlamalar düğmesinden ya da DİF açılış kartlarından ulaşılıyor.
KYS › DİF Tanımlamalar: Kök neden kütüphanesi: Kök neden analizinde öneri olarak kullanılacak tanımlar, balık kılçığı kategorisiyle birlikte. Aynı nedenin her DİF''te farklı cümleyle yazılması sorununu ortadan kaldırıyor.
KYS › DİF Tanımlamalar: Bölüm tanımlama: Uygunsuzluğun görüldüğü bölümler ve sorumlu departmanları.
KYS › DİF Tanımlamalar: DİF Yetki Matrisi: Bölüm bazında "DİF başlatan" ve "kapatma onaycısı" tanımlanabiliyor. Kural yazılan bölümde yetki sunucu tarafında zorlanıyor.
KYS › DİF Tanımlamalar: E-Posta Ayarları: DİF süreç olaylarının 15 bildirim tanımı, satır içi düzenlemeyle.
KYS › DİF Tanımlamalar: Müşteri / Tedarikçi rapor formatları kataloğu.
KYS › DİF Tanımlamalar: Aksiyon şablonları: Aksiyon adımında hazır olarak kullanılabilecek aksiyon metinleri, türü ve varsayılan süresi.
KYS › DİF Tanımlamalar: DF Parametreler ekranı: Müşterinin eski sistemindeki numaralarıyla birlikte 97 süreç parametresi, arama ve satır içi düzenlemeyle.
KYS › DİF Tanımlamalar: Anket soru listeleri: DİF etkinlik değerlendirmesi gibi süreçler için soru kümeleri, cevap tipi ve zorunluluk bilgisiyle.
KYS › DİF Tanımlamalar: Toplu DF iptali: Birden fazla DİF kaydı seçilip tek işlemde iptal edilebiliyor.
KYS › DİF Tanımlamalar: Tekrar eden kayıtlar raporu: Kategori, bölüm, iş yeri ya da kök nedene göre gruplanıp tekrar eşiğini aşanlar listeleniyor.
KYS › DİF Tanımlamalar: Tanım ekranlarının sol menüsüne arama kutusu eklendi. Genel Tanımlar ve Ayarlar ekranlarında da çalışıyor; kapalı gruplardaki tanımlar artık aranarak bulunabiliyor.
KYS › DİF Tanımlamalar: DF Parametreler ekranında her satır bağlı ya da katalog olarak işaretli. "Bağlı" satırın değerini değiştirmek davranışı değiştiriyor; "katalog" satır şu an yalnızca liste. Böylece hiçbir ayar, çalışıyormuş gibi görünüp arka planda hiçbir şey yapmıyor.
KYS › DİF Tanımlamalar: Bu sürümde yedi parametre gerçekten bağlandı: Termin tarihi kullanımı (117), uygunsuzluk kategorisi zorunluluğu (145), kök neden zorunluluğu (146), aksiyon zorunluluğu (147), etkinlik değerlendirmesi kullanımı (87), etkinlik kontrol süresi (81) ve DİF silme yetkisi (234).
KYS › DİF Tanımlamalar: Yetki matrisi ekranı, kural tanımlanmamış bölümlerde kapının açık olduğunu yazılı olarak söylüyor — matrisin boş olması "kimse yetkili değil" değil "henüz yapılandırılmadı" anlamına geliyor.
KYS › DİF Tanımlamalar: Henüz bir çıktı üretmeyen ekranlar (rapor formatları, e-posta tanımları, aksiyon şablonları, anket soruları) bu durumu ekranın üstünde açıkça yazıyor.
KYS › DİF: Detaylı filtredeki DİF Açan ölçütü hiçbir kaydı süzemiyordu; seçim yapıldığında her zaman sıfır sonuç dönüyor, kullanıcı bunu "o kişinin kaydı yok" diye okuyordu. Artık kullanıcı adının bir parçası yazılarak aranıyor.
KYS › DİF: Etkinlik doğrulaması, etkinlik kontrol tarihi boş bırakıldığında hiç devreye girmiyordu: Tarihi doldurmayan herkes doğrulamayı atlayıp DİF''i kapatabiliyordu. Artık zorunluluk parametreden geliyor ve varsayılan olarak açık.
KYS › DİF: "Tespit türü", "Yönetim sistemi" ve "Ürün tipi" listeleri yalnız demo kurulumunda doluydu; gerçek bir kurumda tespit türü alanı hiç doldurulamıyordu. Üçü de standart tanım setine eklendi ve yönetilebilir hale geldi.
## İyileştirmeler
KYS › Denetim: Denetim planına atama yaparken plan kapasitesi kontrol ediliyor; kapasite aşılırsa işlem yapılmıyor ve kaç kaydın atandığı bildiriliyor.
KYS › Denetim: Demo verisi üç yıla yayıldı; cevapları, bulguları ve plan bağı olan denetimlerle birlikte geliyor. Yeni yıllık ekran artık boş açılmıyor.
KYS › DİF: Etkinlik kontrol tarihi alanı boşken, kapanış tarihine göre bir tarih öneriyor (öneri süresi 81 numaralı parametreden).
Güvenlik ve yetkilendirme: Yetki reddi mesajları artık hangi yetkinin gerektiğini ve nereden verileceğini yazıyor.
## Değişiklikler
Bilinen sınırlar: DİF aşama ilerletmesi henüz e-posta bildirimi üretmiyor. E-Posta Ayarları ekranındaki tanımlar bu nedenle şu an katalog olarak duruyor ve ekranda böyle işaretli.
Bilinen sınırlar: DİF için müşteri/tedarikçi rapor çıktısı ve Excel şablonu yükleme henüz yok; rapor formatları ekranı katalog olarak çalışıyor.
Bilinen sınırlar: Aksiyon geciktirme talebi, onayı ve geciktirme sayacı henüz yok; ilgili parametreler katalog olarak duruyor.
Bilinen sınırlar: Anket soruları tanımlanabiliyor ancak cevaplama ve yanıt toplama yüzeyi henüz DİF tarafında yok; etkinlik değerlendirmesi bugün sihirbazın 6. Adımındaki sabit formla yapılıyor.
Bilinen sınırlar: Kök neden analizinde birden fazla kritik kategoriyle çalışılırken her kategori için ayrı bir çalışma alanı henüz yok; ikinci kategoriye geçildiğinde birincinin çalışması ekranda kalıyor.
## Hata Düzeltmeleri
KYS › Denetim: "Havuzdan denetim ekle" listesi hiçbir zaman dolmuyordu; düğme vardı, liste her zaman boştu. Artık planı olmayan bütün denetimler havuzda görünüyor ve her satır kaynağını yazıyor (Atanmamış / Plansız / Planı silinmiş).
KYS › Denetim: Denetim ertelendiğinde yeni tarih eskisinin üzerine yazılıyor ve erteleme geçmişi kayboluyordu. Artık her erteleme kayda ekleniyor.
KYS › Denetim: Denetim raporunda DÖF ve Aksiyon numaraları her zaman "—" basılıyordu; bulgudan gerçekten DÖF açılmış olsa bile izlenebilirlik kanıtı denetlenene giden belgeye hiç girmiyordu.
KYS › Denetim: Denetim listesindeki "tüm dönemler" ve "tüm zamanlar" başlıkları yanlışlıkla tek bir yıl yazıyordu.
KYS › DİF: DİF formundaki Müşteri ve Tedarikçi listeleri hiçbir zaman dolmuyordu. Genel Tanımlar''da tanımlanan firmalar bu alanlarda görünmüyor, kullanıcı her kayıtta firma adını elle yazıyordu — bu yüzden müşteriye göre süzme ve gruplama güvenilmezdi. Artık tanımlı firmalar türüne göre listeleniyor.
KYS › DİF: Kök neden analizinde "Pareto ile ilerlensin mi?" sorusuna önce Evet deyip kategori seçtikten sonra Hayır''a dönüldüğünde, seçilen kategori kayıtta kalıyor ve kök nedenlere yanlışlıkla damgalanıyordu; sonuç raporunda da o kategori basılıyordu.
Güvenlik ve yetkilendirme: DİF kayıt silme yetkisi 234 numaralı parametredeki sicil listesiyle sınırlandırılabiliyor. Liste boşsa davranış değişmiyor.
## Güvenlik
Güvenlik ve yetkilendirme: DİF açma ve kapatma onayı için bölüm bazlı yetki kapısı eklendi. Bir bölüme kural yazıldığı anda o bölümde kural bağlayıcı oluyor; kural yazılmamış bölümlerde davranış değişmiyor.
Güvenlik ve yetkilendirme: Katalog yönetim yetkisi ekran bazına ayrıldı. DİF Tanımlamalar yetkisi verilen kullanıcı yalnız DİF kataloglarını yönetebiliyor; personel, departman, roller gibi kurum geneli kataloglar için Genel Tanımlar yetkisi gerekiyor.', N'Ara Sürüm', 1, '2026-08-29 09:00:00', '2026-08-29 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.33')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.33', '2026-08-31', '2026-08-31', N'## Yenilikler
KYS › Aksiyon Yönetimi: Modüller arası aksiyon havuzu eklendi: Aksiyon planları, plan çalışma alanı, tüm aksiyonlar, onay kuyruğu ve iş listesi.
KYS › Aksiyon Yönetimi: Aksiyonlar liste, pano ve zaman çizelgesi olmak üzere üç görünümde izlenebiliyor.
KYS › Aksiyon Yönetimi: Plan ilerlemesi aksiyonların etki ağırlıklarından hesaplanıyor; bir aksiyonun plana katkısı yüzde olarak veriliyor.
KYS › Aksiyon Yönetimi: Aksiyon yaşam döngüsü: Açılış onayı, gerçekleştirme, kapatma onayı, geciktirme talebi, yönlendirme, iptal ve yeniden açma.
KYS › Aksiyon Yönetimi: Aksiyon geçmişi gerçek denetim izinden okunuyor: Kim, ne zaman, hangi alanı neyle değiştirdi.
KYS › Aksiyon Yönetimi: Yetki kapısı: Bir aksiyonun ilerlemesini yazma, onaya gönderme ve gerçekleştirme yalnız aksiyonun sorumlusunda, işi yapacak kişide, plan sorumlusunda ve modül yöneticisinde.
KYS › Aksiyon Yönetimi: Kısıtlı görünürlükteki planların aksiyonları yetkisiz kullanıcıya görünmüyor.
KYS › Anket ve Sınav: Anket ve sınav modülü eklendi: Tasarım sihirbazı, yayınlama, bağlantıdan doldurma, sonuç analizi ve sınav puanlama.
KYS › Anket ve Sınav: Hedef kitle tanımı, gönderim planı ve katılım hunisi ile gerçek katılım oranı takibi.
KYS › Anket ve Sınav: Soru bankası, görsel kütüphanesi ve şablondan anket oluşturma.
KYS › KKD ve Zimmet Takibi: Kişisel koruyucu donanım zimmeti: Zimmet listesi, gereksinim matrisi, talep kuyruğu, yenilenecekler, eksik KKD ve stok defteri.
KYS › KKD ve Zimmet Takibi: Zimmet tutanağı PDF olarak alınabiliyor; Code 128 barkod desteği var.
KYS › KKD ve Zimmet Takibi: Belge ve fotoğraf ekleme: KKD uygunluk beyanı ve sertifikası, zimmet teslim tutanağı, hasar ve kayıp talebinde fotoğraf.
KYS › KKD ve Zimmet Takibi: Talep akışına "Bilgi iste" durumu eklendi.
KYS › KKD ve Zimmet Takibi: Toplu işlemler ve dışa aktarım: Toplu yenileme, amirlere bildirim, toplu zimmet, rapor alma ve Excel''e aktarma.
KYS › Öneri Yönetimi: Kaizen öneri sistemi: Öneri havuzu, akış panosu, değerlendirme, puanlama, ödüllendirme ve liderlik tablosu.
KYS › Öneri Yönetimi: "Önerimi detaylandır" yardımcısı: Kısa bir fikir cümlesinden yapılandırılmış öneri taslağı üretiyor. Yalnız düğmeye basıldığında çalışıyor.
KYS › Öneri Yönetimi: On beş hazır rapor PDF olarak alınabiliyor. Rapor sayıları ekrandaki sayılarla aynı kaynaktan geliyor.
KYS › Risk Değerlendirme: Risk değerlendirme modülü: Risk alanları, analizler, tehlike kütüphanesi, metot kütüphanesi, aksiyon planı ve Excel''den aktarım.
KYS › Risk Değerlendirme: Farklı değerlendirme metotları destekleniyor; önlem öncesi ve önlem sonrası risk profili karşılaştırmalı gösteriliyor.
KYS › Kimyasal Güvenlik (GBF/SDS): Kimyasal envanteri, güvenlik bilgi formu görüntüleme, revizyon arşivi, tedarikçi ve lokasyon takibi, KKD eşleşmesi.
KYS › Kimyasal Güvenlik (GBF/SDS): Kontrol periyodu göstergesi her kayıtta aynı yüzdeyi gösteriyordu; artık kaydın kendi kontrol tarihinden hesaplanıyor.
KYS › Olay Bildirim: Olay bildirimi: Masaüstü ve mobil bildirim ekranı, olay detayı, risk değerlendirmesi, belgeler, panel ve raporlar.
KYS › Olay Bildirim: Yeni bir kuruma sistem kurulduğunda altı adet örnek risk kaydı kendiliğinden yazılıyordu; kurum bunları kendi kaydı sanabiliyordu. Not: Daha önce kurulmuş kurumlarda bu kayıtlar durmaya devam eder, üzerlerine olay bağlanmış olabileceği için kendiliğinden silinmiyor.
KYS › Olay Bildirim: Olay detayındaki risk sekmesi her olayda aynı altı önlemi ve dördünü işaretli gösteriyordu; önlemler artık tanımlardan geliyor ve hiçbiri ön işaretli değil.
KYS › Yönetimin Gözden Geçirmesi: YGG kokpiti, girdiler, yönetim değerlendirmesi, kararlar, aksiyon atama ve önceki kararların takibi.
KYS › Yönetimin Gözden Geçirmesi: Aksiyon satırı "Bildirildi" yazıyordu ama kimseye bildirim gitmiyordu; bildirim artık ilgili kişilere gerçekten gönderiliyor.
KYS › Yönetimin Gözden Geçirmesi: Yeni karar ve aksiyon formları sabit tarihlerle ve örnek metinlerle açılıyordu.
Tanımlamalar''a olay kontrol önlemleri kataloğu ve iş yeri departmanı alanı eklendi.
## İyileştirmeler
KYS › Aksiyon Yönetimi: Gecikme, kalan gün ve tüm liste süzgeçleri sunucuda hesaplanıyor. Tarayıcı saati değiştirilerek "geciken" listesi kaydırılamıyor.
KYS › Öneri Yönetimi: Liderlik puan ağırlıkları artık tanımlardan okunuyor; yönetici değiştirince ekran da değişiyor.
KYS › Kimyasal Güvenlik (GBF/SDS): Tedarikçi, departman ve tehlike sınıfı listeleri tanımlardan okunuyor.
KYS › Olay Bildirim: Departman ve lokasyon listeleri tanımlardan okunuyor.
Uzun listeler sayfalanıyor; sayfa sayıları ve özet sayaçlar sunucuda hesaplanıyor.
Ekranlarda karşılığı olmayan alanlar boş bırakılıyor.
## Hata Düzeltmeleri
KYS › Anket ve Sınav: Anket yayınlama ve yanıt gönderme ilk denemede hata veriyordu.
KYS › KKD ve Zimmet Takibi: Zimmet tutanağı düğmesi yanlış adrese istek attığı için hiçbir zaman belge indirmiyordu.
KYS › KKD ve Zimmet Takibi: "Yeni KKD tanımla" kendi ekranı olmaktan çıkıp listenin içine gizlenmişti ve menüde görünmüyordu; ayrı ekran ve menü kalemi geri geldi.
KYS › Risk Değerlendirme: Modül ekranları veri tarafına hiç bağlı değildi: Kayıt listesi her zaman boş geliyordu ve Tehlike Kütüphanesi ekranı açılırken hata verip kapanıyordu.
KYS › Risk Değerlendirme: Kayıt çekmecesinde girilen altı alan (kaynak, etkilenenler, tahmini maliyet, yasal dayanak, doğrulama yöntemi, kontrol hiyerarşisi) kaydedilmiyordu; yazılan değer sessizce kayboluyordu.
KYS › Risk Değerlendirme: Kontrol hiyerarşisi hiç seçilmemiş kayıtlarda "Mühendislik kontrolleri" seçilmiş görünüyordu.
KYS › Kimyasal Güvenlik (GBF/SDS): Taşıma bölümündeki UN numarası kaydın kendi verisinden değil tehlike işaretinden üretiliyordu; aynı sınıftaki her kimyasala aynı numara yazılıyordu. Resmî belgeye yanlış bilgi girmemek için bu alanlar artık veri yoksa boş kalıyor.
KYS › Kimyasal Güvenlik (GBF/SDS): Güvenlik bilgi formunun bölümlerinin çoğu her kimyasal için aynı örnek metni gösteriyordu.
KYS › Olay Bildirim: Aylık grafik sekiz ayla sınırlıydı ve panel başlıkları sabit bir yıl yazıyordu.
KYS › Yönetimin Gözden Geçirmesi: Karar kartı "CAPA modülünde açıldı" yazıyordu ama hiçbir kayıt açılmıyordu; artık gerçek bir DÖF kaydı açılıyor ve numarası karta yazılıyor. Kayıt açılamazsa bu da açıkça belirtiliyor.
KYS › Yönetimin Gözden Geçirmesi: Henüz toplantı açılmamış kurumlarda kokpit teknik bir hata mesajı gösteriyordu; artık ne yapılması gerektiğini yazıyor.', N'Ara Sürüm', 1, '2026-08-31 09:00:00', '2026-08-31 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.34')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.34', '2026-09-04', '2026-09-04', N'## Yenilikler
KYS › Kural Yapılandırma: Onay akışlarının kurulduğu yeni modül eklendi: 17 ekran, ana ekranda kendi kartı ve yan menüde kendi bölümü.
KYS › Kural Yapılandırma: Ezilebilir Ayarlar: Şirket geneli bir ayar, koşul eşleştiğinde farklılaşıyor. Örnek: 250.000 ₺ üzeri kayıtta zincire Genel Müdür onayı ekleniyor, Fabrika B''de numara serisi ayrışıyor.
KYS › Kural Yapılandırma: Akıllı Roller: Onay adımı kişiye değil role bağlanıyor. Rol, senaryolar öncelik sırasıyla denenerek organizasyon ağacından çözümleniyor — üst amir, belirli unvan, belirli kademe, aynı tesis gibi. Aday bulunamazsa ne yapılacağı da tanımlanabiliyor.
KYS › Kural Yapılandırma: Onay Matrisleri: Modül başına koşullu onay zinciri şablonu. Her modülde koşulsuz bir varsayılan matris zorunlu; hiçbir kayıt zincirsiz kalmıyor.
KYS › Kural Yapılandırma: İş Akışı Tasarımcısı: Modülün akış haritası kulvarlı diyagram olarak çiziliyor. Düğüme tıklanınca o adımın rolü, adayları, göstergeleri ve referans dokümanları görünüyor.
KYS › Kural Yapılandırma: Kural Simülasyonu: Bir kayıt için tüm zincir baştan sona çözümleniyor — hangi kural ezdi, hangi matris seçildi, her adımda kim aday oldu ve neden.
KYS › Kural Yapılandırma: Yetki Simülasyonu: Bir kullanıcının bir kaydı neden görebildiği (ya da göremediği) katman katman gösteriliyor.
KYS › Kural Yapılandırma: Alan Yönetimi: Modüllere özel alan eklenebiliyor, alan bazlı yetkilendirme yapılabiliyor. Koşullarda kullanılabilir işaretli alanlar diğer ekranların kataloglarına otomatik düşüyor.
KYS › Kural Yapılandırma: Veri Kapsamı Kuralları: Kullanıcının hangi kayıtları göreceği organizasyon ağacından çözümleniyor — kendi kayıtları, birimi, tesisi ya da şirket geneli.
KYS › Kural Yapılandırma: Gizlilik Seviyeleri ve Filigran: Seviye başına indirme, yazdırma, dışa aktarma ve e-posta izinleri; kopya tipine göre filigran şablonu ve A4 canlı önizleme.
KYS › Kural Yapılandırma: Gösterge Tanımları: Süreç, eğitim, doküman, İK ve denetim verisinden beslenen ölçütler. Engelleyici bir gösterge onay adımını durdurabiliyor.
KYS › Kural Yapılandırma: Modül Ayarları ve Numaralandırma: Modül başına ortak ayarlar, süreç davranışı, kayıt numarası biçimi ve sayaç davranışı; bir sonraki üretilecek numara ekranda görünüyor.
KYS › Kural Yapılandırma: Bildirim Şablonları: Olay bazlı bildirimler. Alıcı kişi olarak değil, akıllı rolün çözümlediği adaylar üzerinden belirleniyor.
KYS › Kural Yapılandırma: Denetim İzi ve Entegrasyon Günlüğü: Kural değişiklikleri ve uygulanan kurallar ayrı ayrı izlenebiliyor.
KYS › Kural Yapılandırma: Organizasyon Ağacı: İK''dan gelen koltuk ağacı ve sağlık taraması (kopuk bağ, döngü, eksik kademe/unvan/tesis).
KYS › Kural Yapılandırma: Demo verisine 22 koltuklu örnek organizasyon eklendi. Rol çözümleme, simülasyon ve yetki simülasyonu ekranları artık gerçek bir ağaç üzerinde çalışıyor; öncesinde ağaç boş olduğu için hiçbir aday çözülemiyordu.
KYS › Kural Yapılandırma: Bir kural, tanımlı olmayan bir ayara bağlanabiliyordu. Kural kaydediliyor, listede görünüyor ama hiçbir kayda uygulanmıyordu. Artık reddediliyor.
KYS › Vukuat: Kanban görünümüne Panolar seviyesinde süzme ve arama geldi: Önceliğe göre, sorumluya göre, tipe göre, termin durumuna göre; eskale ve tekrar eden kayıt işaretleri.
KYS › Vukuat: Kartlar beş ayrı eksende şeritlere ayrılabiliyor; süzülen ve toplam kayıt sayısı üstte görünüyor, tek düğmeyle temizlenebiliyor.
Kayan ekran görünümüne klasik alternatif geldi. Ayar altı açılış ekranını birlikte kapsıyor ve kullanıcıya özel olarak sunucuda saklanıyor.
## İyileştirmeler
KYS › Risk Değerlendirme: Modülün tüm ekranları KYS genel temasına ve menü yapısına uyarlandı.
KYS › Risk Değerlendirme: Çalışmayan takvim görünümü kaldırıldı; Excel içe aktarma düğmesi standart görünüme alındı.
Kayıt numarası önekleri kısaltıldı (ör. SIKAYET yerine SK). Halihazırda kaydı olan modüllerde mevcut önek korundu; numara serisi ortadan biçim değiştirmiyor.
## Hata Düzeltmeleri
KYS › Kural Yapılandırma: Silinen bir kaydın kodu bir daha kullanılamıyordu. Onay matrisinin adımlarını düzenlemek her seferinde hata veriyor, bir alandan kaldırılan seçeneği geri eklemek hata verip alanı seçeneksiz bırakıyordu (veri kaybı). Silinen bir gösterge, rol veya matris kodu yeniden açılamıyordu. 21 yerde düzeltildi.
KYS › Kural Yapılandırma: Adı boş bırakılan kural, rol ve matris kaydedilebiliyordu; listede adsız satır olarak duruyordu.
KYS › Kural Yapılandırma: Adım eklenmeden onay matrisi "Aktif" olarak kaydedilebiliyordu. O matris seçildiğinde kayıt "onaya gitti" görünüyor ama onaylayacak kimse olmuyordu.
KYS › Kural Yapılandırma: Denetim izi modülün 11 kayıt kaynağından yalnız 6''sını gösteriyordu. Akış haritası, gizlilik seviyesi, bildirim şablonu, modül ayarı ve numaralandırma değişiklikleri iz bırakmamış gibi görünüyordu.
KYS › Kural Yapılandırma: Bir rol silindiğinde senaryoları ve aday kaynakları geride kalıyordu.
KYS › Kural Yapılandırma: Koşullarda "Tesis" ve "Departman" alanlarına kurulan hiçbir kural eşleşmiyordu: Alanın sunduğu kodlar ile organizasyondan gelen kodlar farklı kümelerdi.
KYS › Kural Yapılandırma: Bazı koşul alanları "motora bağlı" görünüyordu ama motor o alanı okuyamıyordu; rozet yanlış bilgi veriyordu.
KYS › Kural Yapılandırma: Onay adımı sayısı yalnız ekranda sınırlanıyordu; doğrudan gönderilen aşırı değer organizasyon ağacında gereksiz tırmanma denemesine yol açıyordu.
KYS › Kural Yapılandırma: Numara biçiminde aylık sıfırlama seçilince {MM} ay kısaltması reddediliyordu, oysa {AY} ile aynı sonucu üretiyor.
KYS › Kural Yapılandırma: Yetki simülasyonundaki "eşleşen özneler" özeti, sunucunun açıklama metninden çıkarılıyordu; metin değişince özet sessizce bozulabilirdi.
KYS › Kural Yapılandırma: Bir alanın seçenek listesi kaydedilirken önceki seçenekler ayrı bir adımda siliniyordu; ikinci adım başarısız olursa alan seçeneksiz kalıyordu.
KYS › Kural Yapılandırma: Organizasyon ağacında koltuk sakinleri kimlik numarası olarak, kademe/tesis/departman/unvan ise kod olarak görünüyordu. Artık ad geliyor; ad çözülemezse kod ve açık bir uyarı gösteriliyor.
KYS › Risk Değerlendirme: Yalnız kolon ayarlarını kaydetmek çıktı biçimleri menüsünü boşaltıyordu.
KYS › Risk Değerlendirme: Kullanımdaki bir değerlendirme yöntemi kaldırıldığında kayıtlı puanlar sessizce başka bir ölçeğe göre yorumlanıyordu.
KYS › Risk Değerlendirme: Geciken kayıt sayısı gün dönümünde bir gün kayıyordu.
KYS › Risk Değerlendirme: Risk bandı devralması ekranda görünmediği için tasarımcı kaydedildiğinde farkında olmadan bozulabiliyordu.
KYS › Vukuat: Kapanmış kayıtlar "geciken" sayısına dahil ediliyordu.
Kural Yapılandırma menüde görünmüyordu ve yetki grubuna atanamıyordu.
Demo verisinin yeniden yüklenmesi ikinci çağrıda hata veriyordu.', N'Ara Sürüm', 1, '2026-09-04 09:00:00', '2026-09-04 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.35')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.35', '2026-09-04', '2026-09-04', N'## Yenilikler
Giriş ekranı referans tasarımdan sıfırdan yazıldı; Keycloak''ın hazır sayfası yerine uygulamanın kendi ekranı açılıyor.
Giriş ekranı: Kurum seçici, şifremi-unuttum ve iletişim pencereleri ekranın içinde; kullanıcı giriş için başka bir sayfaya gitmiyor.
Giriş ekranı: Karekodla giriş gerçek akışa bağlandı. Önceki hâli örnek bir görüntüydü ve "yakında" yazıyordu.
Giriş ekranı: Giriş sayfasında açılabilecek herkese açık form listesi eklendi.
Giriş ekranı: Yönetici panelinde "Login Sayfa Ayarları": Tanıtım slaytları yüklenip sıralanıyor, giriş ekranında slayt gösterisi olarak dönüyor.
Giriş ekranı: Alt bantta sürüm numarası görünüyor ("2026 ® v1.1.35"). Numara üç kaynakta birlikte tutuluyor ve bir kapı ayrışmayı engelliyor: Giriş ekranı ile uygulamanın sürümü artık ayrışamaz.
Giriş ekranı: Kurum seçicisi arşivlenmiş ya da hiç kurulmamış kurumları da listeliyordu; seçen kullanıcı giriş yapamıyor, çıkış da bulamıyordu. Artık yalnız aktif kurumlar geliyor.
Sözleşme Yönetimi (CLM) ve Belge Şablonları: Sözleşme türüne göre talep formu: Her tür kendi alanlarıyla açılıyor.
Sözleşme Yönetimi (CLM) ve Belge Şablonları: Onay akışı editörü: Sözleşme paketinin onay zinciri arayüzden kuruluyor.
Sözleşme Yönetimi (CLM) ve Belge Şablonları: Raporlar menüsü ve içe alım onay zinciri.
Sözleşme Yönetimi (CLM) ve Belge Şablonları: Şablon göçü ve parametrik paket altyapısı; sözleşme revizyon turu.
Sözleşme Yönetimi (CLM) ve Belge Şablonları: Belge Şablonları modülü: Sözleşmeye bağlı olmadan, şablondan maddeli Word belgesi üretiliyor.
Ana ekran ve gezinme: Proje kartı sekmeli bir çalışma alanına dönüştü; Süreçler sekmesi eklendi.
Ana ekran ve gezinme: Modül kartlı ana ekran ve yandan açılan İş Listesi.
Mobil ve bildirimler: İOS bildirimleri Apple''ın servisine doğrudan gönderiliyor; önceki boru iOS anahtarını kabul etmiyordu.
Mobil ve bildirimler: İOS''ta bildirim izni istenemiyordu; gerekli eklenti eklendi.
Mobil ve bildirimler: Mülakat bildirimine dokununca liste açılıyordu; artık ilgili mülakat kaydı seçili açılıyor.
Formlar ve süreçler: Form alanlarına koşullu zorunluluk: Bir alan, başka bir alanın değerine göre zorunlu olabiliyor. Tablo hücrelerinde de geçerli.
Formlar ve süreçler: Süreç tasarımcısına Nebim V3 ERP düğümü: Eşleme tablosundan fiş (sipariş/irsaliye) yazılıyor ve dönen fiş numarası süreç değişkenine geçiriliyor. Satır döngüsü ve sipariş→irsaliye zinciri destekli.
Kullanıcı yönetimi: Kullanıcı kartı sekmeli açılıyor (Genel · Ek Kolonlar). Kuruma özel kolonların değerleri kartın içinde düzenleniyor; tek Kaydet ile yazılıyor ve yalnız değişen değerler gönderiliyor. Satırdaki ayrı "Alanlar" düğmesi ve penceresi kaldırıldı.
Kullanıcı yönetimi: Özel kolonlar dış sistemle eşlenebiliyor: Eşleme tanımlandığında senkronizasyon bu kolonları da dolduruyor.
Kimlik doğrulama: Kurumsal dizin (LDAP) ile parola doğrulaması. Kullanıcı adı önce sistemde doğrulanıyor, sonra parola dizine sorulur.
Herkese açık panolarda anonim kullanıcı kendi SQL''ini yazabiliyordu; sorgu metni artık tek bir salt-okunur ifadeye kısıtlanıyor.
## İyileştirmeler
Giriş ekranı: Kart ve tanıtım görseli birebir aynı boyda; tema düğmesi kurum satırına alındı, panel kısaltıldı, RiverAI logosu görselin dışına taşındı (marka karışıklığı gideriliyor).
Sözleşme Yönetimi (CLM) ve Belge Şablonları: Belge çıktısı kurumsal tipografiyi taşıyor.
Ana ekran ve gezinme: Ana ekran sadeleştirildi; projeler üzerine gelince açılan hızlı gezgin eklendi.
Ana ekran ve gezinme: Organizasyon menüsünden kullanılmayan üç kalem kaldırıldı (Veri Modeli, E-posta Ayarları, Belgeler).
Ana ekran ve gezinme: Profil ekranından "Ana Ekran" ve "Kişisel Verilerim" sekmeleri kaldırıldı.
Mobil ve bildirimler: Mobil bağlantı ayarı üç ayrı adresten tek adrese indi.
Mobil ve bildirimler: Mobil paket arşivi 4,2 GB''dan 57 MB''a düştü.
Formlar ve süreçler: Başlatma ekranı adım kurallarını form ekranıyla aynı şekilde uyguluyor.
Kullanıcı yönetimi: Özel Kolonlar penceresi genişletildi; her kayıt satır içinde silinebiliyor.
KYS: Risk içe aktarma eşlemesinde alan kodu kırpılıyor; arayüzde boşlukla gelen bir kod eşlemenin sessizce tutmamasına yol açmıyor.
Kurulum ve dağıtım: Dağıtım paketinde bildirim servisi yapılandırması ve anahtar yeri hazırlandı.
## Hata Düzeltmeleri
Giriş ekranı: Kurum ve adım içerikleri görünmüyordu (sınıf adı çakışması).
Giriş ekranı: Çıkışta "Geçersiz yönlendirme url''i" hatası alınıyordu. Onarım tek bozuk kurum yüzünden durup diğerlerini yarım bırakmıyor.
Giriş ekranı: Giriş hataları İngilizce görünüyordu ("Invalid user credentials"). Bilinen durumlar Türkçeye çevrildi: Hatalı şifre, çok sayıda denemeden sonra geçici kilit, devre dışı hesap, eksik kurulum. Tanınmayan bir hata İngilizce sızdırılmıyor; genel bir mesaj gösteriliyor. Sunucuya ulaşılamadığında da açık bir uyarı çıkıyor.
Ana ekran ve gezinme: Koyu temada açılır menü okunmuyordu (açık zemin üzerine açık yazı).
Ana ekran ve gezinme: Çalışma alanı ve herkese açık form düğmeleri görünmüyordu.
Ana ekran ve gezinme: Tanımsız uyarı sınıfları ve koyu temada görünmeyen iki kontrol düzeltildi.
Mobil ve bildirimler: Belge bildirimleri liste yerine belgenin kendisine gidiyor.
Formlar ve süreçler: OnlyOffice ile PDF dönüşümü ilk günden beri çalışmıyordu (istek başlığı eksikti).
Dosya görüntüleme yolunda saklanan betik açığı kapatıldı.
Buton kimliği taklit edilebiliyordu.
Dış servis çağrısı yapan ekranın adres denetiminde üç açık kapatıldı; iç ağ adreslerine erişim engellendi.
Ters proxy baypası ve HTTPS şablonu sertleştirildi; kurulum rehberindeki çalışmayan sunucu örneği düzeltildi.
Herkese açık pano SQL denetimi Türkçe kurulu makinelerde baypas ediliyordu: Büyük harfli anahtar kelimeler ("ınto", "ınsert", "commıt") harf karşılaştırması yüzünden denetimden kaçıyordu ve yeni tablo yazan bir sorgu geçebiliyordu. Aynı sebeple geçerli bir raporlama sorgusu ("WITH ...") haksız yere reddediliyordu. İkisi de kapatıldı.
KYS: Excel''den risk içe aktarma tüm sunucuyu düşürüyordu: İçe aktarma penceresini açan herhangi bir kullanıcı, diğer tüm kurumlar dâhil uygulamayı cevapsız bırakıyordu.
KYS: Yönetimin Gözden Geçirmesi ekranındaki boş-durum metni var olmayan bir işleme yönlendiriyordu.
Kurulum ve dağıtım: .NET 10 ile gelen yapılandırma uyarısı giderildi.', N'Ara Sürüm', 1, '2026-09-04 09:01:00', '2026-09-04 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.36')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.36', '2026-09-08', '2026-09-08', N'## Yenilikler
Doküman arama: Arama ekranı sekmeli yapıdan tek gövdeye geçti. Seçili klasörün alt ağacında arıyor ve KYS dokümanı, arşiv dosyası ile kalite kaydını tek listede gösteriyor; hangi kaynaktan geldiği rozetle belli.
Doküman arama: Üst şeritteki arama kutusu gerçek bir arama alanı oldu; yanına kapsam seçici (Hepsi / Metin / Kavram / Kayıt) ve "Klasörde ara" geldi. Büyüteç her yerde arar, "Klasörde ara" klasör gezer.
Doküman arama: Üst şerit aramasının sonucu için ayrı bir "Bulunanlar" ekranı; klasörden bağımsız çalışıyor.
Doküman arama: Serbest metin aramasında eşleşen pasaj gösteriliyor: Hangi cümlede geçtiğini görmeden doğru dokümanı ayırt etmek zordu.
Doküman arama: Onay, kontrol, hazırlama ve taslak aşamasındaki dokümanlar aramada görünüyordu. Sebep: Durum süzgeci yalnız bir klasör seçiliyken gönderiliyordu, ekranın varsayılan hâli ise "tüm klasörler" olduğu için süzgeç hiç okunmuyordu. Arama artık yürürlükteki dokümanla açılıyor; isteyen taslakları da seçebiliyor.
Doküman arama: Arama terimi ekran hafızasında tutulduğu için ikinci aramada donuyor, paylaşılan bağlantı boş açılıyordu. Terim artık adresten okunuyor.
KYS: Kontrol ve görüş matrisleri hiçbir işe yaramıyordu. Matris kaydediliyor, ekran "kaydedildi" diyor, ama kimseye görev düşmüyordu — sunucuda kontrol/görüş aşaması diye bir şey yoktu. Artık onaya gönderme anında gerçek görev satırları açılıyor ve Görevlerim''de iki yeni kuyruk var.
KYS: Kontrolcü "Uygun" demeden onay akışı ilerlemiyor. "Düzeltme istiyorum" derse doküman taslağa geri çekiliyor, açık onay isteği kapanıyor ve gerekçe hazırlayana bildiriliyor.
KYS: Görüş için "İşe devam kuralı" gerçekten uygulanıyor: Belirlenen sayıda görüş gelince ya da gün dolunca akış ilerliyor. Gün değeri, görüş hiç gelmezse dokümanın süresiz beklemede kalmaması için güvenlik valfi olarak çalışıyor.
KYS: "Okudum onayı alınmasın" ve satır bazında "Okuma görevi düşmesin" ayarları çalışıyor. Tebliğ kaydı yine oluşuyor (kime dağıtıldığı kalite gerekliliği), kalkan yalnızca görev.
KYS: Hazırlayan ile onaylayanın aynı kişi olmak zorunda olduğu durumlar için istisna yetkisi: Yalnız sistem yöneticisinin atadığı bir yetkiyle aşılabiliyor ve her geçiş denetim kaydına yazılıyor. Varsayılan davranış değişmedi — hazırlayan kendi dokümanını onaylayamaz.
KYS: Kontrolsüz kopya yazısı kapatılsa bile basılıyordu ve Filigran Tasarımcısı''nda yapılan hiçbir ayar çıktıya yansımıyordu: Çıktıya sabit bir yazı basılıyor, şablon hiç okunmuyordu. Artık şablonun her alanı uygulanıyor — metin, yerleşim, açı, punto, renk, opaklık, ön/arka katman, döşeli tekrar, çerçeve, sayfa numarası.
KYS: Filigran gerçekten kapatılabiliyor: Gizlilik seviyesi filigran istemiyorsa damga basılmıyor. Taslak, onay bekleyen ve iptal/arşiv belgelerinde ise zorunlu basılıyor — o belgeler kontrolsüzdür.
KYS: İndirilen Word dosyasına da filigran basılıyor. Önceden damga yalnız PDF üretiminde vuruluyordu, dokümanı Word''de açan kişi damgasız bir kopya görüyordu.
KYS: Değişkenler çözülüyor: Kullanıcı, tarih, saat, kayıt no, kopya tipi, IP adresi ve birim. Çözülemeyen bir değişken uydurulmuyor, "—" basılıyor ve gerekçe denetim kaydına yazılıyor.
KYS: Doküman özeti, imha tutanağı, vukuat, denetim raporu, master liste ve kontrollü kopya föyü dâhil dokuz çıktı aynı filigran kurallarına bağlandı. Kontrollü kopya föyünde kopya sıra numarası da basılıyor.
KYS: "Yayımlanmayacak" seçimi hiçbir şey yapmıyordu. Artık o klasördeki doküman için dış paylaşım bağlantısı üretilemiyor; kapı sunucuda olduğu için düğmeyi gizlemekle yetinilmiyor.
KYS: Kontrollü kopya matrisi için klasör sihirbazına yeni bir panel eklendi.
KYS: "Şirket parametresi" seçeneğinin okunacağı bir yer yoktu. Dört parametre artık KYS Ayarları → Şirket parametreleri panelinden yönetiliyor ve klasör ekranında o andaki değeri yazılı.
KYS: "Kontrollü kopya baskı nedeni" alanı açıldı; klasör ayarı zorunlu kılıyorsa neden boş bırakılamıyor ve nedenin kendisi föy çıktısında görünüyor.
KYS: Ek dosyalar revizyonda gerçekten taşınıyor. "Taşınsın" varsayılanı yerine getirilmeyen bir vaatti.
KYS: Revizyonda "yapılanlar" alanı Word sürüm notlarından otomatik doluyor; aynı bilgiyi iki kez yazmak gerekmiyor. Kullanıcı sonradan değiştirebiliyor.
KYS: Word sürüm geçmişinde sürüm notu görünüyor; satırlar "v1 — (bilinmiyor)" olarak kalmıyor.
KYS: Sihirbaz adımları (Kontrollü kopya, Süreçler, Ürün, Ek güvenlik) KYS Ayarları''ndan açılıp kapatılabiliyor. Kapatılan adımın verisi silinmiyor. Zorunlu alan taşıyan adımlar kapatılamıyor — kapatılsa doküman hiç onaya gönderilemezdi.
KYS: Kullanıcı tanımlı alanlar: Sistemin içinden yeni alan tanımlanıp istenen sihirbaz adımında gösterilebiliyor (metin, sayı, tarih, uzun metin, onay kutusu, katalogdan seçim). Zorunlu bırakılan alan boşsa doküman onaya gönderilemiyor. Matris ve kişi seçici tipleri bu turda yok; editörde hangi tiplerin olduğu yazılı.
KYS: Ana ekran slotları: Ortadaki slot kaldırılabiliyor (önceden hep sondaki kalkıyordu), sürükle-bırak ile taşınabiliyor ve genişliği ayarlanabiliyor — üstte tek geniş, altında dört slot kurulabiliyor.
KYS: Modül kartına sağ tık o modülün ayarlar/tanımlar ekranına gidiyor. Birden çok ayar ekranı olan modülde küçük bir menü açılıyor; ayar ekranı olmayan kartta hiçbir şey açılmıyor — çalışmayan bir menü göstermek olmamasından kötü. Belirli bir tanım grubuna doğrudan bağlantı verilebiliyor.
KYS: Modül kartı özelleştirme: Kart başlığı, alt metin, arka plan görseli, renkler, animasyon ve boyut kullanıcıya özel. Ayarlar sunucuda tutulduğu için başka cihazdan girince aynı görünüm geliyor.
KYS: İş listesinde okundu göstergesi: Açılan görevin zarfı açılıyor ve kalın yazı kalkıyor.
Ekran akıcılığı ve okunurluk: Doküman ve revizyon ön izlemesinde tam ekran seçeneği.
KYS: Vukuat kanbanında önceliğe ve kişiye göre süzme, arama ve şerit seçenekleri.
KYS: Kayan ekran görünümü ile klasik görünüm arasında ayar.
Süreç tasarımı: Bileşen kopyala/yapıştır (Ctrl+C / Ctrl+V) geri geldi ve güvenli: Kendi panosunu kullanıyor, işletim sistemi panosuna dokunmuyor ve yalnız imleç tuvalin üzerindeyken çalışıyor. Seçili düğümler aralarındaki bağlantılarla kopyalanıyor, başka bir süreç sekmesine de yapıştırılabiliyor. Yapıştırma imlecin bulunduğu noktaya düşüyor. İzleme modunda kapalı.
Sözleşme Yönetimi (CLM): Sözleşme, taraf ve tür silme; aktif, yasal saklama altındaki veya zeyilnameli kayıtlar asla silinmiyor, yönetici olmayan yalnız onaya girmemiş taslağı silebiliyor. Bağlı satırlar birlikte temizleniyor ve denetim kaydı yazılıyor.
Sözleşme Yönetimi (CLM): Madde başlığı ve metninde {{ yazınca bağlı formun alanları öneriliyor; onay akışı ve hatırlatıcı editörlerinde rol adı önerisi çıkıyor.
Sözleşme Yönetimi (CLM): Yeni Taraf penceresi Düzenle ile aynı künye alanlarını ve Yetkili Kişi bölümünü taşıyor; kütüphaneden seçmede madde türü filtresi.
Katalog ve yönetim: Excel aktarım penceresine "Şablon indir": Kataloğun yapısına uygun, yalnız başlık satırlı boş dosya üretiliyor; her datagrid için ayrı sayfa. Etiketi alan adına çözülemeyen kolonlarda alan adı yazılıyor — aksi halde aktarım o kolonu sessizce atlıyordu.
Katalog ve yönetim: Grup ağacında kullanıcı sayısı rozeti.
Kurulum ve dağıtım: Stok kurulumda hiç yedekleme yoktu. Yedekleme ve geri yükleme betikleri, yönetim komutları ve gecelik otomatik yedek eklendi. Yarım yedek tuzağına karşı mühürleme var: Yedek dizini ancak tüm adımlar bitince asıl adına taşınıyor ve geri yükleme mühürsüz dizini reddediyor. Başarısız bir geri yükleme artık sessizce "tamam" demiyor.
## İyileştirmeler
Doküman arama: Filtreli arama doküman listesinin üzerine alındı; sık kullanılan süzgeçler "ekle-çıkar" çipleri olarak kaydedilebiliyor.
KYS: "Kendi dokümanını onaylama" seçimi klasör sihirbazında öne çıkan bir yerdeydi; ileri ayarlara alındı. Seçimin sonucu ve ISO uyarısı görünür kalıyor, ayrıca şirket parametresinin o andaki değeri yazılı.
KYS: Onayla/Reddet düğmeleri gerçek yetkiye göre kapanıyor; pasifse kararın kimde olduğu yazıyor.
KYS: Bekleyen işler ile dashboard yer değiştirdi; alttaki listeyle bağı kuruldu.
KYS: Slot yerine iki büyük dashboard kurulabiliyor.
KYS: "Kalite Kokpiti" kalemi ana ekran yüzeylerinin tamamından kaldırıldı. Ekran ve adresi duruyor.
Ekran akıcılığı ve okunurluk: Küçük yazı boyutları tarayıcı/işletim sistemi yazı ölçeğine uyacak şekilde yeniden yazıldı — yakınlaştırmadan okumak kolaylaştı. Varsayılan görünüm birebir aynı kaldı.
Süreç tasarımı: Sekme şeridi üst bara yapışık: Uzun sayfalarda kaydırınca kaybolmuyor.
Katalog ve yönetim: Aktarım penceresinden teknik "slug" alanı kaldırıldı; değer otomatik gidiyor.
## Hata Düzeltmeleri
Doküman arama: Klasör kapsamı yol adıyla kuruluyordu: "/Formlar" ile "/Formlar Eski" yanlış eşleşiyordu. Kapsam artık klasör kimlikleriyle kuruluyor.
KYS: Okuma görevi hiç düşmüyordu. Dağıtım matrisi klasörde kilitliyken doküman o alanı boş taşıyor, tebliğ üretimi ise doğrudan o boş alanı okuyordu — sonuç: Hiç tebliğ yazılmıyordu ("Okundu Onayı Bekleyen: 0"). Dağıtım artık klasör ağacından çözülüyor; ekranda görünen satırlarla tebliğ düşenler aynı küme.
KYS: Tebliğ kutusu büyük/küçük harf duyarlı eşliyordu; bazı kullanıcılar kendilerine düşen tebliği ne görüyor ne de sayabiliyordu.
KYS: Taslağı olmayan (dışarıdan alınan) bir doküman yürürlüğe girdiğinde hiç tebliğ üretilmiyordu.
KYS: Kendi hazırladığınız doküman "onayımı bekleyenler" listesine düşüyor, "Onayla"ya basınca reddediliyordu. Sistem yapamayacağınız bir iş dağıtıyordu. Bu satırlar artık kuyruktan çıkıyor ama kaybolmuyor: "Görev ayrılığı — onaylayamadıklarım" listesinde gerekçesiyle görünüyor, böylece dokümanın nerede takıldığı izlenebiliyor.
KYS: Filigran metninde satır sonu, tasarımcı önizlemesinde ters eğik çizgi olarak görünüyor ama kâğıda iki satır olarak basılıyordu; önizleme ile çıktı ayrışmıyor.
KYS: "Revizyonda matris atanma şekli" altındaki beş seçim de kaydedilip hiç okunmuyordu. Beşi de revizyonda uygulanıyor. Anlaşılmayan "Doküman Tipi" seçeneği "Varsayılan" oldu ve üçünün ne demek olduğu ekranda yazıyor: Doküman = klasöre bakmaksızın doküman özelinde, Klasör = klasörün matrisi revizyonda da bağlayıcı, Varsayılan = klasörden gelen ön-dolu yazılır ama değiştirilebilir. Daha önce kaydedilmiş değerler okunmaya devam ediyor.
KYS: "Gözden geçirilecek mi?" iki seçenek + ayrı bir "zorunlu olsun" kutusuydu; üç duruma indi: Geçirilmeyecek (değiştirilemez) · Geçirilecek (zorunlu) · Varsayılan (değiştirilebilir). Sunucu önceden yalnız "kapatamaz" yönünü zorluyordu, "açamaz" yönü hiç yoktu — klasör "gözden geçirilmeyecek" derken doküman kendine hatırlatma kurabiliyordu.
KYS: Revizyonda "Word olarak aç" boş bir belge açıyordu ve ekran "gövde önceki onaylı içerikten gelir" diye söz veriyordu. Kaynak zinciri revizyonda kopuyor, sonra sessizce boş bir şablon üretiliyordu — ve bu geri dönülemez oluyordu. Kaynak artık ikinci bir yoldan da bulunuyor; hiçbir yoldan bulunamazsa boş belge üretilmiyor, sebebi yazılı bir hata veriliyor.
KYS: Revizyon talebi doküman sahibine hiç bildirilmiyordu. Artık sahibe bildirim gidiyor; sahip çözülemezse sessiz kalınmıyor, denetim izine gerekçe yazılıyor.
KYS: Revizyon ekranı üç sekmeye dağılmıştı ve ölçüt girilmeden liste tamamen boş geliyordu. Tek düzene indi; onaylananlar ve işlem bekleyenler tek listede, durum kolonuyla.
KYS: İptal edilmiş bir revizyon talebi "bekleyen talepler" kutusunda canlı Onayla/Reddet düğmeleriyle duruyordu; henüz gönderilmemiş taslak talep ise "onay sürecinde" diye yanlış etiketleniyordu.
KYS: Hassas belge indirirken PDF parolası isteniyor ama alan görünmüyordu: Kenarlık çok soluktu ve kısıt metni alanı statik bir açıklama gibi gösteriyordu. Boş alanla "İndir"e basmak da indirmeyi iptal ediyordu. Alan artık maskeli ve belirgin, kısıt altta ipucu olarak duruyor, onay düğmesi parola yeterli uzunluğa gelene kadar pasif.
KYS: Sağ tık menüleri kaydırılmış sayfada imlecin çok altında, bazen ekranın tamamen dışında açılıyordu. Yedi çağrı yerinden altısı sayfa koordinatı veriyordu; hepsi düzeltildi.
Ekran akıcılığı ve okunurluk: Adım geçişinde sayfa başa dönmüyordu: Uzun bir adımın dibindeyken "İleri"ye basınca yeni adım ortasından açılıyor, üstteki zorunlu alanlar görülmüyordu. Altı ekranda düzeltildi (doküman hazırlama, DİF sihirbazı, değişiklik yönetimi, kalite kayıtları, klasör tanımlama, kontrollü kopya). Klavye odağı da yeni adıma taşınıyor.
Ekran akıcılığı ve okunurluk: Adım listesi sayfa kaydırılınca yukarı gidip kayboluyordu. Sebep ölçülerek bulundu: Dış kabuk hiç kaymayan bir kaydırma alanı açıyor ve içindeki bütün yapışkan başlıkları sessizce etkisiz kılıyordu. Liste artık ekranda kalıyor; dar ekranda ve yakınlaştırmada yatay şeride dönüyor.
Ekran akıcılığı ve okunurluk: Telefon genişliğinde üst panel sayfayı yatay kaydırıyordu.
Ekran akıcılığı ve okunurluk: Gömülü ön izleme tam ekranken başka bir sekmeye geçilince ekran boşalıyor, yalnız ESC ile kurtarılabiliyordu.
KYS: DİF''te aksiyon bildirimi geliyor ama içeriği görünmüyordu.
KYS: Serbest metin arama çalışmıyordu.
Süreç tasarımı: Tasarımcıda kaydedilmemiş değişiklikler kaybolabiliyordu.
Süreç tasarımı: Bir düğümü diğerinin üstüne bırakınca bağlantı kurulmuyordu.
Süreç tasarımı: E-posta düğümünde CC/BCC alanları Mail modülü (Graph/SMTP) yolunda taşınmıyordu.
Sözleşme Yönetimi (CLM): Editörde bırakılan boş satırlar Word çıktısında kayboluyordu.
Katalog ve yönetim: Üst Yönetici seçim listesi kırpılıyordu.
Kurulum ve dağıtım: Taze kurulum tamamen başarısız oluyordu: Ayar dosyasındaki tırnaksız bir satır yüzünden kurulum betiği hata verip ölüyordu. Yeniden üretilerek doğrulandı.
Kurulum ve dağıtım: HTML şablonlu PDF üretimi (çıktı şablonu önizlemesi, doküman görevi) müşteride ilk kullanımda çalışmıyordu: Gereken tarayıcı bileşeni imajda kurulmuyordu, kurulduktan sonra da yalnız yönetici hesabı çalıştırabiliyordu. İzin düzeltildi ve derleme sonunda sınanıyor — eksikse derleme başarısız oluyor.
Kurulum ve dağıtım: Alt bar ve bağlam menüsü açılışta ve üzerine gelindiğinde boyut değiştiriyor, tıklanmak istenen öğe kaçıyordu.', N'Ara Sürüm', 1, '2026-09-08 09:00:00', '2026-09-08 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.37')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.37', '2026-09-10', '2026-09-10', N'## Yenilikler
Karanlık mod artık uygulamanın tamamında geçerli: Görev ekranları, formlar, süreçler, panolar, yönetim ekranları, üretim, dms, clm, İK, EBYS, e-Fatura, ıtsm, CRM, dkys ve Sandık. Önceden yalnız üç ekran destekliyordu.
Karanlık mod: Sekiz modüle kendi renk katmanı kuruldu. Her modülün kimliği korundu — İK turuncu, EBYS turkuaz, e-Fatura lacivert, ITSM mor, DKYS yeşil kalmaya devam ediyor; karanlıkta aynı rengin açılmış tonu kullanılıyor, gri bir şeye dönüşmüyor.
Karanlık modda uygulamanın her birincil düğmesi okunması zor hâle geliyordu; üzerine gelindiğinde daha da kötüleşiyordu. Düğme dolgusu artık iki temada da aynı.
KYS: Klasör ve doküman yapısını Excel ile toplu kurmak için 5 adımlı sihirbaz (Doküman Yönetimi → Toplu İçe Aktarım): Hazırlık, klasör/yetki, dokümanlar, dosyalar ve sonuç raporu.
KYS: Ekrandan hazır şablon indirilebiliyor; hangi sütunun ne beklediği şablonda yazılı.
KYS: İçe aktarım iki aşamalı: Önce taslaklar oluşuyor, sonra Word dosyaları eşleşince yayına alınıyor. Aynı aktarım ikinci kez çalıştırılırsa var olan satırlar tekrar oluşturulmuyor.
CLM: Revize turu: Tur numarası şablonu, ilgililere bildirim, iş listesine düşen görev ve formdan sözleşme oluşturma.
CLM: Elle giriş gerektiren üç eksik alan tamamlandı: Yükümlülük, KVKK kaydı ve muhafaza.
CLM: Uyum ekranına elle DPA kaydı eklenebiliyor.
Performans panosu: Performans & Darboğaz panosu (Kullanıcılar → Performans): Ekip göstergeleri ve süreçlerde nerede beklendiğini gösteren darboğaz analizi.
Doküman ve arama: Yüklenen her dosya türünden metin çıkarılıyor; doküman içeriği Word açılmadan da aranabiliyor.
Doküman ve arama: Kurulumda "yalnız vektör tamamlama" modu: Mevcut veriye dokunmadan eksik arama vektörlerini parça parça tamamlıyor.
## İyileştirmeler
Karanlık mod düğmesi artık her ekranda görünüyor (açık kalan ekranlar hariç).
Tasarımcı Modu düğmesi: Düğme bir anahtara dönüştü: Mod değişince kalem simgesi sağa kayıyor ve yerini kullanıcı simgesine bırakıyor, geri dönüşte tersi. Düğmenin genişliği geçiş boyunca sabit kalıyor, yani yanındaki öğeler oynamıyor.
Tasarımcı Modu düğmesi: Hareket azaltma tercihi açık olan cihazlarda geçiş anında oluyor.
CLM: "Son sorgularım" listesinde satır tek tek kaldırılabiliyor, tümü birden temizlenebiliyor.
Küçük düzeltmeler: Yüzen İş Listesi kulakçığı daraltıldı; varsayılan olarak yalnız simge gösteriyor.
## Hata Düzeltmeleri
Karanlık mod: Rozet ve etiket yazıları pastel zeminlerinde soluk kalıyordu. Renk paletine ayrı bir "yazı" basamağı eklendi; onay, uyarı, hata, bağlantı ve vurgu yazılarının tamamı iki temada da okunur.
Karanlık modda "Word''de aç" denince ekranın tamamı beyazlıyordu.
Karanlık mod: KYS''nin Öneri ve Aksiyon ekranlarında liste satırlarının bir kısmı okunmuyordu (koyu satır zemini üzerine koyu yazı).
Karanlık mod: Bazı listeler ve açılır pencereler karanlıkta beyaz kalıyordu.
Karanlık mod: Klavyeyle gezinirken Tasarımcı Modu düğmesinin odak halkası görünmüyordu.
KYS: Gövdesi gerçekten yüklenmemiş bir doküman yanlışlıkla yayına alınamıyor; dosyası gelmeyen satırlar sonuç raporunda ayrıca sayılıyor.
KYS: Doküman hazırlama ekranında klasör yetkisi kontrol edilmeyen bir yol vardı; kapatıldı.
Doküman ve arama: Arama anahtarına yapışmış görünmez bir karakter aramayı sessizce çalışmaz hâle getiriyordu — hata da vermiyordu.
Güvenlik ve kararlılık: Kurulumda LDAP eklentisi yokken Keycloak açılmıyordu.
Küçük düzeltmeler: Form tasarımcısında "Zorunlu yap" kuralının ne yaptığı yazılı değildi; açıklama ve canlı önizleme rozeti eklendi.
Küçük düzeltmeler: KYS Öneri Yönetimi kartının renk şeridi boş kalıyordu.
## Güvenlik
Güvenlik ve kararlılık: DMS ve İK''nın yazma yollarında kiracılar arası sızıntıya açık noktalar kapatıldı.
Güvenlik ve kararlılık: KYS doküman gövdesine yetki kontrolünden geçmeden ulaşan üç yol ve bunlara komşu iki kanal kapatıldı; gövdeye erişim tek yetkili kapıya alındı.
Güvenlik ve kararlılık: Sınırsız büyüyen iki önbellek kapatıldı; ikisi de kimlik doğrulaması istemiyordu.', N'Ara Sürüm', 1, '2026-09-10 09:00:00', '2026-09-10 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.38')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.38', '2026-09-14', '2026-09-14', N'## Yenilikler
DÖF / DİF aksiyonları: Aksiyonun kendi yaşam döngüsü geldi: Atandı → Devam Ediyor → Tamamlandı (kontrole gönderildi) → Onaylandı ya da Revizyon İstendi. Sorumlunun "tamamlandı" demesi işin onaylandığı anlamına gelmiyor; yalnız "üzerime düşeni yaptım" demek.
DÖF / DİF aksiyonları: Gerçekleştirilen işlem / sonuç alanı: Aksiyonu yapan kişi ne yaptığını yazıyor ve kanıt dosyası ekleyebiliyor. Denetimde ilk istenen kayıt bu.
DÖF / DİF aksiyonları: Aksiyon Detayı görünümü: Yapılması gereken, gerçekleştirilen işlem, ekler, sorumlu, termin ve kontrole gönderilme tarihi bir arada. Kontrolü yapan kişi buradan onaylıyor ya da gerekçesiyle revizyon istiyor.
DÖF / DİF aksiyonları: Aksiyon kontrole gönderildiğinde DÖF sorumlusuna ve ekip liderine bildirim gidiyor; bildirim doğrudan o aksiyona açılıyor.
DÖF / DİF aksiyonları: Tüm aksiyonlar onaylandığında süreç kapatma onayına kendiliğinden geçiyor. Geçiş normal yetki kapılarından geçiyor; onaylayan kişinin yetkisi yoksa aşama yerinde kalıyor ve onay geri alınmıyor.
DÖF / DİF aksiyonları: Atanan aksiyonlar artık iş listesine düşüyor — ana ekranda, Görevlerim''de ve sol menü rozetinde. Liste satırından kaydın doğrudan aksiyon adımı açılıyor.
DÖF / DİF aksiyonları: Aksiyon numarası (AKS-01) artık kalıcı bir sayaçtan veriliyor; bir satır silinince numara ikinci kez dağıtılmıyor.
DİF süreci: Süreç ilerlemiyorken ilerlemiş gibi görünüyordu: Kullanıcı kapatma adımının alanlarını doldurup kaydedebiliyor, ekran sessiz kalıyordu. Kapatma onayının alanları artık yalnız süreç o aşamadayken yazılabiliyor, kaydet düğmesi ne olduğunu söylüyor ve ray hangi aşamada olunduğunu gösteriyor.
DİF süreci: Karar düğmeleri ("süreci kapat", "izlemeye al", "kök nedene geri gönder") mevcut aşama dışında gizleniyordu; kullanıcı onları hiç olmadığı sanıyordu. Artık görünüyor, kilitli ve neden kilitli olduğu yazılı. Başlık "Kapatmayı onayla" oldu.
DİF süreci: Kök nedene geri gönderilen kayıt etkinlik doğrulamasını taşımaya devam ediyordu; yeni aksiyonların etkinliği hiç ölçülmeden kayıt ikinci kez kapatılabiliyordu.
DİF süreci: Etkinlik doğrulaması zamanı geldiğinde, doğrulamanın henüz yapılamayacağı kayıtlar için de hatırlatma gidiyordu.
Yaygınlaştırma: İyileştirici faaliyetler de yaygınlaştırılabiliyor. Önceden yalnız düzeltici faaliyetlere izin veriliyordu.
Yaygınlaştırma: İki tür ayrı iş üretiyor: Düzeltici faaliyette "bu kök neden burada da var mı, kontrol edin", iyileştirici faaliyette "bu iyi uygulama burada da uygulanabilir mi". Ekranın sorduğu soru, üretilen kaydın konusu, tanımı ve giden bildirim dört yerde de buna göre değişiyor.
Yaygınlaştırma: Hedef olarak iş yeri ve bölümün yanında doküman seçilebiliyor: İyi uygulamanın ilgili prosedüre standart olarak eklenmesi.
Yaygınlaştırmadan doğan kayıtlar listede rozetle işaretleniyor; rozet kaynağı, istenen işi ve hedefi yazıyor.
Doküman yönetimi: Klasördeki "kontrollü / kontrolsüz yazısı basılmasın" ayarının sunucuda hiçbir okuyucusu yoktu; ayar işaretli olsa da filigran basılmaya devam ediyordu. Artık PDF ve Word çıktılarının ikisi de aynı kararı okuyor.
Doküman yönetimi: Gizliliği çözülemeyen bir dokümanın damgası bu ayarla susturulamıyor. Ayar bir kurumsal tercihi geçersiz kılar; sistemin "bu dokümanın gizliliğini bilmiyorum" dediği için bastığı damgayı değil.
Doküman yönetimi: Klasör ayarlarında kontrollü kopya zorunlu seçiliyse filigran düzenleme alanı açılıyor.
Doküman yönetimi: Revizyon numarasının elle mi sistem tarafından mı belirleneceği klasörden ayarlanıyor — yeni doküman ve revizyon için ayrı ayrı. Varsayılanlar bugünkü davranışı koruyor.
Fotoğraf ve kanıt: Sahadan gelen ilk vukuat talebine fotoğraf eklenebiliyor — kayıt henüz oluşmadan.
Fotoğraf ve kanıt: Aksiyonu yapan kişi de fotoğraf ekliyor; önce ve sonra ayrı ayrı tutuluyor ve karşılaştırılabiliyor.
Fotoğraf ve kanıt: DİF talebine fotoğraf eklenebiliyor.
Organizasyon şeması: Kişi ve departman elle eklenebiliyor — İK uygulaması olmayan kurumlar ağacı buradan kurabiliyor.
Organizasyon şeması: Excel ile toplu içe aktarım: Şablon indirme, önizleme, satır bazlı hata raporu. Var olan kişilerin üzerine yazılmıyor ve ağaçta döngü oluşturacak bağlar reddediliyor.
## İyileştirmeler
DİF süreci: Yaygınlaştırma reddedildiğinde gerekçe artık kaydın durumunu, aşamasını ve türünü yazıyor.
Organizasyon şeması: Çalışmayan dört içe aktarma düğmesi (Visio, REST, Active Directory) ve üç menü girdisi kaldırıldı.
Küçük düzeltmeler: Sandık ödeme planı yazdırma çıktısının stili karanlık mod bütçesine dahil edildi; kâğıt her temada beyaz kaldığı için renkleri temaya bağlanmadı.
## Hata Düzeltmeleri
DÖF / DİF aksiyonları: Aksiyonu yapan kişi kendi aksiyonunu onaylayamıyor. Onaylayabilecek başka kimse yoksa (kayıt sorumlusu aksiyonu kendine yazmış ve ekip lideri yoksa) kural uygulanmıyor — yoksa kayıt hiç kapatılamazdı.
DÖF / DİF aksiyonları: İşi tarif eden alanlar (yapılması gereken, sorumlu, termin, hedef) aksiyonu yapan kişi tarafından değiştirilemiyor. Kural sunucuda; ekrandaki kilit yalnız yol gösteriyor.
DÖF / DİF aksiyonları: İncelemeye girmiş bir aksiyon satırı silinemiyor — kanıt dosyaları o satıra bağlı.
DÖF / DİF aksiyonları: Aynı kaydı iki kişi aynı anda düzenlerse sonradan kaydeden öncekini sessizce ezmiyor; ekran "tazeleyip tekrar deneyin" diyor.
DİF süreci: Listede kaydın hangi aşamada olduğu görünmüyordu; durum rozetinin altına aşama sayacı eklendi.
Doküman yönetimi: Yürürlükteki bir dokümandan revizyon başlatıldığında numara alanı düzenlenebilir açılıyor ve girilen numara onaya gönderildiğinde sessizce değişiyordu; ekran kullanıcıya yanlış numarayı onaylatıyordu.
Doküman yönetimi: Revizyon numarası politikası taslağın ve dokümanın klasöründen ayrı ayrı okunuyordu; ikisi farklı olduğunda ekranın söylediği ile yayınlanan numara tutmuyordu.
Fotoğraf ve kanıt: Silinen bir aksiyonun kanıt dosyaları sonradan eklenen başka bir aksiyonun altında görünebiliyordu.
Kural Yapılandırma: Onay matrisinde koşul eklenemiyordu. Sebep göründüğü gibi değildi: Ekran her zaman DİF modülünü açmaya çalışıyordu ve o modülü tanımlamamış kurumlarda seçici boş kalıp alan kataloğu boş dönüyordu.
Kural Yapılandırma: Alt ekranlarda başlık hep "Kural Yapılandırma" yazıyordu; girilen modülün adı görünmüyordu.
Kural Yapılandırma: Bir gösterge, değerini üretecek kaynak bağlı olmadığı hâlde "hesaplanabilir" görünüyordu. En pahalı göründüğü yer tasarımcıydı: Göstergeyi bir durağa bağlayınca durak sağlıklı görünüyor, ama değer hiç üretilmiyordu.
Organizasyon şeması: İK senkronizasyonu, elle çizilmiş yönetici bağını her senkronda siliyordu.
Küçük düzeltmeler: Ek dosya listelerinde her görsel için dosyanın tam boyu indiriliyor ve okuma kaydına satır yazılıyordu; küçük görsel artık yalnız karşılaştırma yapılan ekranlarda ve kendi kaydıyla.
Küçük düzeltmeler: Aksiyon tamamlandığında giden bildirim, işlemi yapan kişinin kendisine de gidebiliyordu.
Küçük düzeltmeler: Onay damgası, daha önce istenmiş bir revizyonun gerekçesini taşıyordu; onaylanan aksiyon "gerekçe: Kanıt yetersiz" yazıyordu.
Küçük düzeltmeler: Sonuç raporunda aksiyon durumu kullanıcı sözlüğünde olmayan bir kelimeyle basılıyordu, ve kontrol bekleyen kayıt hiç iş yapılmamış gibi görünüyordu.
Küçük düzeltmeler: Sol menü rozeti her sayfada iki ağır liste sorgusu çalıştırıyordu; artık sayaç ucu kullanılıyor.', N'Ara Sürüm', 1, '2026-09-14 09:00:00', '2026-09-14 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.39')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.39', '2026-09-14', '2026-09-14', N'## Yenilikler
Doküman revizyon akışı: Revizyon İşlemleri ekranı: Revizyon talepleri, kontrol bekleyen revizyonlar ve revizyon yayınlama onayları tek yerde.
Doküman revizyon akışı: Revizyon talebini herkes açabiliyor; değerlendirmesi dokümanın sahibine gidiyor ve sahip onayladığı anda revizyon çalışması başlıyor. Önceden onaydan sonra iki adım daha vardı ve hiçbir ekran bunu söylemiyordu.
Doküman revizyon akışı: Revizyon talepleri artık iş listesine düşüyor ve bildirim gidiyor. Önceden bildirim gidiyor ama iş listesi boş kalıyordu: Kuyruk kendi ön koşulunu bekliyordu — kaydı ilerletecek kişi, tam da kuyrukta göremediği için ilerletemeyen kişiydi.
Doküman revizyon akışı: Onaylanmış revizyonlar sahibinin kendi listesinde görünüyor.
Doküman revizyon akışı: Revizyonu yapacak kişi görevlendirilebiliyor; görevlendirme artık kayda yazılıyor. Önceden yalnız bildirim gidiyor, sistemde o kişiyi revizyonun sahibi yapan tek bir kayıt bile oluşmuyordu.
Değişiklik yönetimi: Revizyon kuyruğundaki "Talebi aç" düğmesi kullanıcıyı değişiklik yönetimi formuna götürüyordu; artık aynı ekranda açılıyor.
Değişiklik yönetimi: Doküman yönlendirme penceresi "yönlendirildi" diyordu ama kayda hiçbir şey yazmıyordu; metin artık ne olduğunu doğru söylüyor.
Yetki ve güvenlik: Süper admin modu bağlandı. Rol her kurumda zaten tanımlıydı ama yalnız menüleri açıyordu; sekiz ayrı yerdeki elle yazılmış yönetici kontrolü tek tanıma bağlandı. Süper admin revizyon numarası politikasını aşabiliyor.
Yetki ve güvenlik: Revizyonda kontrolcü zorunluluğu artık kurum genelinde açılabiliyor. Kural yazılmıştı ama açmanın yolu yoktu.
Doküman arama ve kimlik: Kimlik bilgileri panelinde doküman tipi ve dört alan daha gösteriliyor.
İlişki merkezi: İlişki eklerken doküman klasör ağacından seçilebiliyor; kod ezberlemek gerekmiyor.
Klasör ve kontrollü kopya: Klasör yönetiminde kopyalama ve taşıma birden fazla klasör seçilerek yapılabiliyor.
Organizasyon şeması: Kişi ve departman elle eklenebiliyor; Excel ile toplu içe aktarım geldi.
Kural Yapılandırma: Alt ekranlarda başlık hep "Kural Yapılandırma" yazıyordu; artık girilen modülün adı görünüyor.
## İyileştirmeler
Doküman revizyon akışı: Revizyon kuyruğundaki satır artık dokümanın kendi kodunu, adını ve revizyon numarasını gösteriyor; talep numarası ayrı satırda.
Değişiklik yönetimi: Modül doküman yönetiminin altından çıkarıldı, kendi başına bir KYS modülü oldu.
Doküman arama ve kimlik: Revizyon sürecindeki dokümanlar arama sonucunda rozetle işaretleniyor. Yürürlükteki sürüm gizlenmedi — o hâlâ geçerli doküman ve insanların ona ulaşması gerekiyor.
İlişki merkezi: Yürürlüğe alma ve askıya alma komutları kaldırıldı; ekran yalnız dokümanları birbirine bağlıyor. Bu işlemler dört ayrı ekranda duruyor.
Klasör ve kontrollü kopya: Kontrollü kopya iki seçeneğe indirildi: Kullan (içinde kontrollü/kontrolsüz seçilir) ve kullanma (çıktıya hiçbir yazı basılmaz).
Organizasyon şeması: Çalışmayan yedi düğme ve menü girdisi kaldırıldı.
## Hata Düzeltmeleri
Doküman revizyon akışı: Reddedilen bir revizyon talebi dokümanı süresiz kilitliyordu.
Doküman revizyon akışı: Doküman sahibi kendi dokümanı için talep açtığında talebi kimse onaylayamıyor ve doküman kalıcı kilitleniyordu.
Doküman revizyon akışı: Talep onaylandıktan sonra kayıt her iş listesinden düşüyor ama revizyon yine açılmıyordu.
Doküman revizyon akışı: İş listesindeki satır doğru ekrana ama yanlış sekmeye götürüyordu; kullanıcı aradığı listeyi göremiyordu.
Yetki ve güvenlik: Kontrolcü zorunluluğu yalnız "kontrolcü yazılmış mı" diye bakıyordu; silinmiş bir gruba işaret eden tanım kapıdan geçiyor ve revizyon kontrolsüz yayına gidebiliyordu.
Klasör ve kontrollü kopya: Klasör ayarı bazı durumlarda kendi kendini tersine çeviriyordu.
Kural Yapılandırma: Onay matrisinde koşul eklenemiyordu. Ekran her zaman DİF modülünü açmaya çalışıyordu; o modülü tanımlamamış kurumlarda seçici boşalıp alan kataloğu boş dönüyordu.
Kural Yapılandırma: Bir gösterge, değerini üretecek kaynak bağlı olmadığı hâlde "hesaplanabilir" görünüyordu.
## Güvenlik
Yetki ve güvenlik: Klasörde revize yetkisi olmayan kullanıcı, yürürlükteki bir dokümanın revizyonunu başlatabiliyordu. Yetki yalnız onaya gönderme anında sorgulanıyordu; kapı artık başlangıçta ve dokümanın kendi klasöründen soruluyor.
Yetki ve güvenlik: Onaylanmamış bir revizyon talebi varken doğrudan revizyon başlatılamıyor.
Yetki ve güvenlik: Yönetici baypası gizlilik kapılarını kapsamıyor: Maskelenmiş belgenin aslını görme ve indirme hakları rolden otomatik gelmiyor, matristen alınıyor.', N'Ara Sürüm', 1, '2026-09-14 09:01:00', '2026-09-14 09:01:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.40')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.40', '2026-09-16', '2026-09-16', N'## Yenilikler
Doküman ve klasör yetkileri: Açık yasaklama geldi. Artık "bu klasörde herkese izin, ama şu alt klasörde şu gruba yasak" denebiliyor. Yasak, üst klasörden gelen izni geçersiz kılıyor ve yalnız hedeflediği kişi/grubu etkiliyor; klasörün geri kalan mirasını bozmuyor. Yasak alt klasörlere de iniyor, yani bir alt klasöre inerek aşılamıyor.
Doküman ve klasör yetkileri: Doküman kendi yetkisini taşıyabiliyor. Hazırlama sihirbazının yetki satırları bugüne kadar kaydediliyor ama hiçbir erişimi açıp kapatmıyordu; kullanıcı "yetki verdim" sanıyordu. Artık erişim kararına giriyorlar — ama yalnız klasörde "Doküman bazında yetki değişikliğine izin ver" açıksa. Ayar varsayılan olarak KAPALI: Mevcut dokümanların davranışı değişmiyor.
Doküman ve klasör yetkileri: Klasör ekranı artık üst klasörden devralınan yetki satırlarını gösteriyor. Önceden boş bir tablo görünüyordu ve yönetici "burada yetki yok" sanıp bir satır eklediğinde üst klasörün matrisi anında devre dışı kalıyor, o ana kadar erişebilen herkes sessizce düşüyordu. Ekran artık ilk satırı eklemenin devralmayı durduracağını önceden söylüyor.
Doküman ve klasör yetkileri: Klasörde varsayılandan sapan dokümanlar listeleniyor; ayar kapalıyken o satırların hiçbir erişimi değiştirmediği de yazıyor.
Doküman ve klasör yetkileri: Yetki artık pozisyona bağlanabiliyor (kişi adına değil). Kişi seçicisinde pozisyonlar ayrı bir kaynak olarak listeleniyor; pozisyon sahibi değişince o pozisyona bağlı tüm yetkiler kendiliğinden yeni kişiye işaret ediyor, hiçbir klasöre ya da dokümana dokunulmuyor. Onay zinciri de pozisyon tanıyor — "onaycı: Filanca kişi" yazılmış bir akış, o kişi ayrıldığında süresiz beklemiyor.
Doküman ve klasör yetkileri: Dokümanı başka klasöre taşıma ayrı bir işlem oldu: Hem kaynak hem hedef klasörde yetki istiyor, gerekçe zorunlu ve kendi denetim kaydını yazıyor. Denetimde "bu doküman neden bu klasörde" sorusunun bugüne kadar hiçbir cevabı yoktu.
Görev panosu (Kanban): Ek süre talebi. Görevi yapan kişi kaç gün ek süre istediğini ve gerekçesini yazıyor; kartı atayan kişi ya da pano yöneticisi onaylıyor. Onaylanınca termin gerçekten güncelleniyor, talebi açan kişi bildirim alıyor. Bugüne kadar termini yalnız atayan değiştirebildiği için görevi yapanın elinde sistem içinde hiçbir yol yoktu.
Görev panosu (Kanban): Onaya gönderirken açıklama yazılabiliyor. Açıklama onaylayanın bildirimine ve onay e-postasına giriyor — onaylayanın gördüğü tek şey artık kartın başlığı değil.
Görev panosu (Kanban): Yönlendirme ve atama gerekçeleri artık kart geçmişinde okunuyor. Gerekçe aslında kaydediliyordu ama etkinlik listesi o olay türünü tanımadığı için ham bir etiket olarak görünüyor, yazılan açıklamayı kimse göremiyordu.
Görev panosu (Kanban): Panodan giden e-postalar kartın açıklamasını da taşıyor — hem olay bildirimlerinde hem onay e-postasında. Uzun açıklamalar kırpılıp karta yönlendiriliyor. Gizlilik gerekçesiyle kapatmak isteyen pano tek ayarla kapatabiliyor.
Görev panosu (Kanban): İş listesi çekmecesindeki dört sayaç kartı tıklanabilir: Karta basınca liste o kapsama filtreleniyor. Görev kartı yeniden düzenlendi; Tarihçe ve Not panelleri artık tüm ekranın üstünde açılıyor.
Görev panosu (Kanban): Pano "Ana ekranda göster" ile ana ekranda canlı bir kart olarak gösterilebiliyor.
EBYS: TÜRKKEP ile KEP gönderimi doğrulanmış dört adımlı akışa geçti.
EBYS: Evrağa Turkcell mobil imza ile doğrudan imza atılabiliyor (KEP''siz).
EBYS: Kayıt, havale ve gönderim işlemleri için rol kapıları tanımlandı.
İntranet: "Yaşam" tanım ekranları: Servis, menü ve yan haklar tanımlanabiliyor; menüler Excel ile toplu aktarılabiliyor.
İntranet: Bilgi Tabanı materyallerine kitle yetkilendirmesi geldi — hangi materyali kimin göreceği belirlenebiliyor.
Sözleşme yönetimi (CLM): Son kullanıcı için basit form tasarımcısı.
Sözleşme yönetimi (CLM): Yetkilendirme yapısı, Excel ile toplu aktarım, şablon kopyalama ve takvime ekleme.
Sözleşme yönetimi (CLM): Bildirimler, iş listesi, tur numarası şablonu ve formdan sözleşme oluşturma.
Sözleşme yönetimi (CLM): Yükümlülük, KVKK kaydı ve muhafaza için elle giriş tamamlandı.
Doküman yönetimi (DMS): Ortak Çalışma Alanı: Klasör bağlantısıyla dosya paylaşımı ve ortak çizim.
Doküman yönetimi (DMS): Her dosya türünden metin çıkarımı.
Kalite yönetimi (KYS): Mükerrer içerik kontrolü: Doküman yüklenmeden önce benzerleri gösteriliyor.
Kalite yönetimi (KYS): Güncelleme Sihirbazı — dayanılan standardın yeni baskısı çıkmış mı takip ediyor.
Süreç tasarımcısı ve veri konsolu: Atama düğümüne "İfade" kipi; ifadeler her yerde kullanılabiliyor.
Süreç tasarımcısı ve veri konsolu: SQL ve Mongo düğümlerine test paneli + Mongo için "Kolay kurucu".
Süreç tasarımcısı ve veri konsolu: Katalog düğümüne alan eşleştirme ekranı.
Süreç tasarımcısı ve veri konsolu: Veri Konsolu''na Diagram sekmesi: Görsel bağlantı kurucu, sorgu üretimi ve kaydedilen sorguyla saklama.
Süreç tasarımcısı ve veri konsolu: Kaydetmeden kapatılan süreç, yeni tasarımda geri yükleniyordu.
## İyileştirmeler
EBYS: TÜRKKEP, UETS ve mobil imza entegrasyonları çalışan referansa göre sertleştirildi.
İntranet: Duyurularda tam kaplama görünüm ve metin tonu seçimi; haberlerde video oynatma.
Sözleşme yönetimi (CLM): "Son sorgularım" listesinden satır tek tek ya da toplu kaldırılabiliyor.
Kalite yönetimi (KYS): Arama KYS evrenine çekildi: Türkçe gövdeleme ve kendi anlamsal indeksi.
Süreç tasarımcısı ve veri konsolu: Sistem değişkeni erişimi genişledi: Form alanına bağlama, yönetici e-postası ve belge değişkeni seçici.
## Hata Düzeltmeleri
Doküman ve klasör yetkileri: Klasör kopyalarken yasak satırları izin satırına dönüşüyordu.
Görev panosu (Kanban): Kart açıklaması düzenlenebilir açılıyor. Önceden önizleme modunda açılıyordu ve daha kötüsü: Boş bir açıklamaya ilk harf yazıldığı anda yazma alanı kaybolup önizlemeye düşüyordu. Önizleme korundu (tablo ve listeler için) ama artık isteğe bağlı; salt-okur kullanıcıya zaten yalnız önizleme gösteriliyor.
EBYS: İlgi ekranındaki gizlilik kapısı atlanabiliyordu.
İntranet: Bağlantı yapıştırırken ekran donuyordu.
İntranet: Ham sorgudaki bir işaret yüzünden uygulama açılışta çöküyordu.
İntranet: İki yeni alan mevcut kurulumlarda açılmıyordu.
Doküman yönetimi (DMS): Ağ tarayıcısı bazı cihazları bulamıyordu (arama süresi uzatıldı).
Doküman yönetimi (DMS): Bir anahtardaki görünmez karakter aramayı sessizce durduruyordu.
Kalite yönetimi (KYS): Karanlık temada okunmayan durum kolonu.
Süreç tasarımcısı ve veri konsolu: İzleme modunda her düğüme geri sarılabiliyor; sağ tık menüsü ve simüle düğmesi düzeltildi.
Güvenlik ve kararlılık: Alıcı alanları boşken e-posta gönderimi hata veriyordu.
Güvenlik ve kararlılık: Bazı uçlar alan adı yerine sayı bekleyip hata döndürüyordu (kullanıcı senkronu, İletilerim, kurumlar arası aktarım).
Güvenlik ve kararlılık: Kurulumda dizin sağlayıcısı eksikken kimlik sunucusu açılmıyordu.
## Güvenlik
Doküman ve klasör yetkileri: Bir dokümanın klasörü, doküman oluşturulduktan sonra artık değiştirilemiyor. Ekrandaki kilit bir güvenlik kapısı değildi: Uca doğrudan istek gönderen bir istemci taslağın klasörünü değiştirebiliyor, taslak ile yayınlanmış kayıt sessizce ayrışıyor ve o dokümanın yetkisi, sahibi, kontrol edeni gibi klasörden gelen her şey yanlış klasörden hesaplanmaya başlıyordu.
Doküman ve klasör yetkileri: Belgeye yazan üç kanal klasör yetki matrisine bağlandı: Yeni belge yükleme, mevcut belgeye yeni sürüm yazma ve eski sürüme geri döndürme. Üçü de matrisi hiç uygulamıyordu; Tarama Merkezi ekranı "Evrak, seçilen klasörün yetkilerini miras alır" derken bu yazma tarafında karşılıksızdı.
Doküman ve klasör yetkileri: Arama da klasör yetkisini uyguluyor. Matrisle gizlenen bir belge, tekil açılışta reddedilirken aramada adı, kodu ve içerik parçasıyla görünmeye devam ediyordu.
Güvenlik ve kararlılık: Yazma yollarında kiracı sızıntısı, yutulan bir yetki kapısı ve bütünlük açıkları kapatıldı.
Güvenlik ve kararlılık: Kalite dokümanı gövdesine giden üç yetkisiz yol ve iki bitişik kanal kapatıldı.', N'Ara Sürüm', 1, '2026-09-16 09:00:00', '2026-09-16 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.41')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.41', '2026-09-18', '2026-09-18', N'## Yenilikler
Görev panoları: Pano görünürlüğü ayarı: Bir pano yalnız üyelerine görünür hâle getirilebiliyor. Varsayılan kapalı — mevcut panoların davranışı değişmiyor.
Görev panoları: Kart detayında bir karttan diğerine geçince önceki kartın düzenlemesi yeni karta yazılıyordu. Bağlı karta ya da alt göreve atlandığında pencere yeniden kurulmadığı için başlık, atanan, başlangıç ve son tarih önceki karttan taşınıyordu.
Gözünden Gör: Yönetici, doğrudan astının ekranını salt okunur olarak görebiliyor. Destek ve devir süreçlerinde "bende çıkmıyor" sorununu ekranı paylaşmadan çözmek için.
EBYS ve e-imza: USB / akıllı kart ile e-imza geldi. Artık belgeler mobil imzanın yanı sıra elinizdeki e-imza token''ı ile de imzalanabiliyor. Token''ın özel anahtarı hiçbir zaman bilgisayarınızdan çıkmıyor: İmza bilgisayarınızdaki yerel imza aracısında atılıyor, sunucuya yalnız imzanın kendisi geliyor. (Yerel aracının kurulumu ayrı bir adım.)
EBYS ve e-imza: İmzalar artık standart CAdES-BES (.p7s). Ham imza yerine uluslararası standarda uygun paket üretiliyor ve imzalı dosya indirilebiliyor — böylece imza, RiverAI dışındaki araçlarla da doğrulanabiliyor. Sunucu, gelen imzayı sertifikayla doğrulamadan kabul etmiyor.
EBYS ve e-imza: EBYS evrakları da e-imzalanabiliyor. Evrak detayındaki tek "E-imza" düğmesi önce yöntemi soruyor (USB/kart ya da mobil); imza tamamlanınca evrak "imzalandı" durumuna geçiyor. Eski "Mobil İmzala" düğmesinin yerini aldı.
Outlet Ürün Tanımlama için örnek kurulum: Kataloglar, form, süreç ve PDF ekli bilgilendirme e-postası.
## İyileştirmeler
İntranet: Duyuru slayt görselleri artık dosya deposuna yükleniyor; kayıt içine gömülmüyor.
## Hata Düzeltmeleri
Görev panoları: Bir kart, yanlışlıkla başka panonun kolonuna taşınabiliyordu. İşlem başarılı görünüyor, ama kart o andan sonra hiçbir panoda çizilmiyordu: Kaynak pano onu listeliyor, hiçbir kolonla eşleşmiyor, alt görevleri de peşinden gidiyordu. Taşıma artık hedef panonun da sahipliğini ve yetkisini soruyor; panolar arası taşımanın ayrı ve yetkili bir yolu var.
Görev panoları: Pano listesi ile ana ekran aynı erişim kuralını kullanıyor. Kural tek bir yerde toplandı ve üç yolu birden tanıyor: Üyelik (kullanıcı, rol ve grup), pano sahipliği ve onaycılık. Onaycı olmak üyelik yaratmadığı için, e-postadan gelen onaycı kendi kartının panosunu açamıyordu.
Görev panoları: Son tarih sessizce siliniyordu. Tarih ve saatten yalnız biri girildiğinde alan boş kabul edilip termin siliniyor, ekranda tarih durduğu için kayıp fark edilmiyordu.
Görev panoları: Alt görev, kuralı olan kolonlarda eklenemiyordu. Alt görev üst kartın kolonuna doğuyor ve o kolonun "oluştururken zorunlu" kurallarına tabi oluyor; oluşturma penceresi yalnız başlık topladığı için kayıt reddediliyordu. Üstelik red, kullanıcıya anlaşılmaz bir sistem hatası olarak görünüyordu — artık ne eksik olduğunu söylüyor.
EBYS ve e-imza: KEP ile gelen evraklar kaydedilemiyordu; evrak tarihi saat dilimi yüzünden reddediliyor ve hata hiçbir yere yazılmıyordu.
EBYS ve e-imza: E-imza sağlayıcı bilgileri (parola / anahtar) kaydedilmiyordu.
EBYS ve e-imza: KEP çağrıları uygulama geneli zaman aşımına takılıyordu; bu entegrasyonun kendi süresi ayrıldı.
EBYS ve e-imza: Mobil imza durum sorgusu yanlış adrese gidiyordu; kullanıcı telefonunda imzayı tamamlasa bile sonuç okunamıyor, işlem tamamlanmamış görünüyordu.
İntranet: Duyuru slaytına fotoğraf eklendiğinde görsel görünmüyor, alan gri kalıyordu.
Görünüm: Koyu temada sol menüde seçili öğe ve grup başlığı okunmuyordu.
Görünüm: Ana ekrandaki pano kartı koyu temada açık renkte kalıyordu.
E-posta: Microsoft 365 ile gönderim yapan kurulumlarda giden postanın tamamı sessizce düşmüş olabilir. "Gönderen adresi" alanı Microsoft tarafında uzun süre hiç kullanılmıyordu — yalnızca görünüm içindi. Çoklu gönderen özelliği geldikten sonra bu alan canlandı ve "Gönderen kullanıcı" ile farklı yazılmış her kurulumda Exchange gönderimi reddetmeye başladı. Hata hiçbir ekrana yansımadığı için (şifre sıfırlama, KYS bildirimi, pano maili ve süreç e-postası hatayı sessizce yutuyor) aylarca fark edilmeden sürebiliyordu. Kontrol edin: Entegrasyonlar → E-posta''da bu iki alan farklıysa "Gönderen adresi"ni boşaltın; mail o anda düzelir.
E-posta: Aynı hata artık ne yapılacağını söylüyor. Önceden ekrana Microsoft''un ham İngilizce metni çıkıyordu. Şimdi hangi posta kutusundan gönderildiği, üzerine hangi adresin yazılmak istendiği ve üç çıkış yolu Türkçe yazıyor — ve hata, gerçekten kullanılan kutuyu adlandırıyor.
Alıcı alanları boşken e-posta gönderimi yine hata veriyordu (bir önceki düzeltme tüm durumları kapsamıyormuş).', N'Ara Sürüm', 1, '2026-09-18 09:00:00', '2026-09-18 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.42')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.42', '2026-09-22', '2026-09-22', N'## Yenilikler
Form ve Süreç Tasarımcısı: Tasarımcı araç çubuğuna "Notlar" eklendi — formu/süreci tasarlayan kişiler serbest not bırakabiliyor (kim, ne zaman, ne yazdı). Sadece ekle + listele; düzenleme veya silme yok.
Form ve Süreç Tasarımcısı: Datagrid alanlarında sütun bazlı zorunlu hücre kontrolü eklendi — satır eklenip zorunlu bir hücre boş bırakılabiliyordu, fark edilmiyordu.
Form ve Süreç Tasarımcısı: "Onayla" artık aynı zorunlu-alan doğrulamasından geçiyor. Önceden onay bu kontrolü tamamen atlayıp süreci tamamlıyordu; datagrid satırlarındaki boş zorunlu hücreler onaylanabiliyordu.
PDF şablonlarına sayfa boyutu desteği eklendi (A3/A4/A5/A6 veya özel ölçü, dikey/yatay) — tasarımcıda seçiliyor, render alırken bu ayar kullanılıyor.
Kataloglar: Katalog listesinde "Silinenleri göster" — soft-delete edilmiş bir katalog tanımı artık geri yüklenebiliyor. Önceden silinen kataloglar listeden tamamen kayboluyor, geri getirme yolu yoktu.
Sandık: Katkı tercihine geçerlilik ayı eklendi — yeni tutarın hangi aydan itibaren geçerli olacağı artık belirtilebiliyor.
Sandık: Yıllık dernek aidatı artık her Ocak ayında aktif üyelere otomatik tahakkuk ediyor (telafi kontrollü — sunucu 1 Ocak''ta kapalıysa ertesi gün kendiliğinden yapılıyor).
İntranet: Ankette en az bir yanıt alındıysa sorular ve seçenekler artık değiştirilemiyor — mevcut yanıtların anlamını bozmasın diye. Başlık, durum, tarih ve kitle ayarları hâlâ serbestçe güncellenebiliyor.
## İyileştirmeler
Form ve Süreç Tasarımcısı: Formül derleme hatası artık uygulamayı çökertmeden ekranda gösteriliyor; önceki geçerli formül korunuyor.
Sandık: Direkt çıkışta nema ve kalan borç artık elle girilmiyor, sistemden okunuyor; sandık görevlisi yalnızca "işten de ayrıldı mı" seçiyor.
## Hata Düzeltmeleri
Form ve Süreç Tasarımcısı: Formül oluşturucuda DATEDIFF artık datagrid satırı içinde de çalışıyor (önceden "desteklenmiyor" hatası veriyordu).
Süreçler: "Yönetici (org şeması)" ataması, konu belirtilmediğinde (varsayılan: Süreci başlatanın yöneticisi) yanlış bir kimlikle arama yaptığı için her zaman sessizce başarısız oluyor, atama boş kalıyordu. Artık doğru kimlikle çözülüyor.
İntranet: Bilgi Tabanı''nda dosya/link/kategori yüklemesi sunucuda 500 hatası veriyordu (AudienceJson kolonu veritabanında jsonb, uygulama text gönderiyordu).', N'Ara Sürüm', 1, '2026-09-22 09:00:00', '2026-09-22 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.43')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.43', '2026-09-24', '2026-09-24', N'## Yenilikler
Süreç Tasarımcısı: Datagrid Arayüzü (datagridTask) node''u — süreç akışı bir datagrid değişkenine update/delete/getCellValue/getCheckedIds/rowCount/colCount işlemlerinden birini uygulayıp sonucu (veya hatayı) bir süreç değişkenine yazıyor, ardından otomatik ilerliyor.
Süreç Tasarımcısı: Her iki yeni node için yardım dokümanı eklendi (Datagrid Arayüzü, Form Override — güncellendi) ve yardım kayıt defterine işlendi.
Giriş Ekranı: "Giriş yapmadan erişebileceğiniz formlar" popup''ının başlığı ve açıklaması artık admin → Login sayfa ayarları''ndan düzenlenebiliyor.
Giriş Ekranı: Sandık üyelik başvuru linki bu listeye geri eklendi — eski Keycloak temasındaki public-forms.js normal akışta görünmediği için bu link erişilemez hâle gelmişti.
Entegrasyonlar: PTT KEP/UETS için tek tıkla bağlantı testi düğmesi eklendi (TürkKEP''teki testin PTT karşılığı) — KEP için bakiye sorgusu, UETS için mesaj sayımı ile doğrulama yapıyor.
Sandık: Üye ayrılma talebi artık gerçek onay akışına bağlandı (Akış Ayarları → Onay Kuyruğu) — önceden akış/onay olmadan doğrudan görevlinin düz listesine düşüyordu.
Sandık: Üye kendi ayrılma talebinde "işten de ayrılıyorum" tercihini beyan edebiliyor; görevli onay ekranında bunu ön-dolu görüyor ama isterse değiştirebiliyor.
Sandık: Akış adımlarına birden fazla rol atanabiliyor (VEYA mantığıyla — hangisi önce onaylarsa adım biter); önceden tek rol seçilebiliyordu.
Sandık: Bekleyen bir borç talebi artık düzenlenebiliyor — üye yanlış tutar/taksit girdiyse (onay süreci henüz başlamamışsa) düzeltebiliyor; admin ayrıca limit ve taksit kataloğu kontrollerini atlayarak (override) düzenleyebiliyor.
Sandık: Üye için "tekrar aktif et" (reaktivasyon) ekranı eklendi.
## İyileştirmeler
Süreç Tasarımcısı: Form Override editörü — hedef alan seçimi artık aranabilir bir açılır listeden yapılıyor (SearchableCombo), placeholder metni girilebiliyor; datagrid''lerin rows alanı artık satır içi JSON/değişken YA DA canlı bir SQL sorgusuyla doldurulabiliyor (kolon adları otomatik eşleniyor).
Sandık görevlisi ayrılma onayında nema ve kalan borç tutarlarını artık elle girmiyor — direkt çıkışla aynı prensip: Sistemden okunuyor, görevli yalnızca "işten de ayrıldı mı" seçiyor.
Sandık: Akış Ayarları''ndan Nema Dağılım ve Aidat Tahakkuk adımları listeden kaldırıldı — hiçbir servis bu ikisi için akışı başlatmıyor, düzenlenebilir göstermek yanıltıcıydı.
Sandık: Onay kuyruğu kayıtlarına şirket bilgisi eklendi — çok şirketli sandık kurulumlarında kayıtlar şirkete göre süzülebiliyor.
## Hata Düzeltmeleri
İntranet: Ankette az önce eklenen bir soru, aynı kaydetme işleminde silme geçişi tarafından sessizce süpürülebiliyordu (kök neden: Veritabanı tarafında üretilen ID''lerin geç atanması). ID''ler artık istemci tarafında üretiliyor, sorun ortadan kalktı.', N'Ara Sürüm', 1, '2026-09-24 09:00:00', '2026-09-24 09:00:00');
  SET @n = @n + 1;
END
IF NOT EXISTS (SELECT 1 FROM dbo.VersionAnnouncements WHERE ProductName = N'River AI' AND VersionNo = N'1.1.44')
BEGIN
  INSERT INTO dbo.VersionAnnouncements (ProductName, VersionNo, ReleaseDate, AccessDate, Highlights, BodyText, IsPublished, CreatedAt, PublishedAt)
  VALUES (N'River AI', N'1.1.44', '2026-09-25', '2026-09-25', N'## Yenilikler
Yöneticiler İçin: "Klasör oluşturma" yeni bir yetki kalemi. Yeni klasör açma, klasör ağacı taslağı, hazır şablonlar, kod biçimi, doküman tipi ve "koda dahil" ayarı bu kaleme bağlı. Yetki grubu olan kullanıcılarda bu kalem kapalı başlıyor. Bugün Klasör tanımlama ekranından klasör açan ya da doküman tipini değiştiren kişilerin yetki gruplarına Genel Tanımlar → Yetki grubu tanımlama ekranından "Klasör oluşturma" ekleyin.
KYS: Doküman kodu artık klasörden üretiliyor. Kod üç parçadan oluşuyor: Tür kısaltması, koda dahil edilen üst klasörlerin kodları ve sıra numarası (ör. PR-10-01-001). 1.1.28''de getirilen "kod yalnız doküman tipinden gelir" kuralı müşteri kararıyla geri alındı.
KYS: Her klasör kod bloğunun sıra numarası 001''den başlıyor. Kodu olmayan klasördeki dokümanlar eski tip serisinden (PR-001, PR-002…) kesintisiz devam ediyor.
KYS: Her klasöre "Kodu doküman koduna dahil et" ayarı eklendi. Kapalıysa klasör ağaçta durur ama kodu dokümana yazılmaz; böylece ağaç derin, kod kısa kalabiliyor. Varsayılan açık olduğu için mevcut klasörler bugünkü gibi davranıyor.
KYS: Aynı adlı iki doküman tipi varken aynı tipteki dokümanlar farklı ön ek alabiliyordu. Artık ön ek her seferinde aynı tipten geliyor.
KYS: Doküman Yönetimi altına Klasör oluşturma ekranı eklendi. Beş sekmesi var: Klasör yapısı, Hazır şablonlar, Kod önizleme, Kod biçimi ve Tanımlamalar. Klasör ağacı ve doküman kodlama kuralı burada kurulup yayınlanıyor.
KYS: Klasör ağacı önce taslak üzerinde düzenleniyor ve "Yayınla" denene kadar canlı ağaç değişmiyor. Taslak ekranında "Canlıdan yenile" ve "Taslağı sil" seçenekleri var.
KYS: Yayından önce taslak denetleniyor ve yayında yalnız taslakta değiştirdiğiniz alanlar canlıya yazılıyor. Aynı alan bu arada canlı ağaçta da farklı bir değere değiştirildiyse ekran çakışmayı gösteriyor ve yayın duruyor.
KYS: Aynı taslağı iki kişi aynı anda kaydederse ikinci kişi uyarılıyor. Böylece birinin değişikliği ötekinin üstüne sessizce yazılmıyor.
KYS: 30 hazır klasör şablonu eklendi, 3 grupta toplanıyor. Her şablonun detayında ağaç, her klasörde açılacak ilk dokümanın kodu, sağlık uyarıları ve eksik doküman türleri görünüyor. Şablonları yan yana karşılaştıran bir tablo da var.
KYS: Şablon "Taslağa yükle…" ile seçilen klasörün altına ekleniyor. Kurumda henüz doküman kodu verilmemişse şablonun kod ayarını da uygulama seçeneği çıkıyor.
KYS: Şablonun istediği doküman türleri kurumda yoksa "Eksik türleri Doküman tiplerine ekle" ile tek tıkla açılıyor. Aynı adlı bir tip zaten varsa yenisi açılmıyor ve nedeni ekranda yazıyor.
KYS: Kod önizleme sekmesi, taslaktaki her klasörde açılacak ilk dokümanın kodunu gösteriyor.
KYS: Kod biçimi ayarları eklendi: Genel ayraç, tür ayracı, sıra ayracı, sıra hane sayısı, bitişik sıra ve harf biçimi (aynen / büyük / küçük). Kurumda bir kez doküman kodu verildikten sonra kod biçimi kilitleniyor; böylece arşivde iki farklı kod yazımı yan yana kalmıyor.
KYS: Fiziki dosya adı ayarları eklendi: Düzen (kod-ad-rev, kod-rev-ad, ad-kod-rev, kod-ad, kod-rev), revizyon yazımı (Rev03, R03, v03, (03)…), ayraçlar, tarih eklensin mi (varsayılan açık) ve boşluklar. Seçim yaparken canlı örnek görünüyor. Ayar Word ve PDF indirmelerinde uygulanıyor. Varsayılan, bugün kullanılan biçim.
KYS: Kod sağlığı kartı canlı ağacı denetliyor: Kardeş klasörlerde aynı kod, aynı kod bloğunu paylaşan klasörler, zincirinde hiç kod olmayan klasörler, tür kodu ve uzunluk. 18 karakterin üstü uyarı, 64 karakterin üstü hata olarak gösteriliyor.
KYS: "Tanımdan ekle" taslağa klasörleri tanımlardan kuruyor. Süreçler alt süreçleriyle, departmanlar alt departmanlarıyla, iş yerleri bağlı hat ve makinalarıyla ekleniyor. Klasör adı tanımın adı, klasör kodu tanımın kodu oluyor. Üst bağı çözülemeyen ya da belirsiz olan kayıt köke ekleniyor ve bu durum pencerede söyleniyor.
KYS: Tanımlamalar sekmesi doküman tipi, süreç, alt süreç, departman, iş yeri, makina, yönetim sistemi ve standart ekranlarına kısayol veriyor.
KYS: Klasör seçilince sağ panelde kodun parçaları görünüyor: Tür, zincirdeki klasörler (koda dahil olmayanlar üstü çizili) ve sıra. Panelde sıradaki gerçek doküman kodu ve örnek dosya adı da yer alıyor.
KYS: "Güncelle" artık yalnız değiştirdiğiniz ayarları kaydediyor, dokunmadığınız ayarlar üst klasörden devralınmaya devam ediyor. Önceden kaydetmek bütün ayarları klasöre kopyalıyor ve devralma kopuyordu.
KYS: Taslak artık klasör seçilmeden kaydedilemiyor. Kayıtlı bir taslakta klasörü boşaltıp kaydetmek de reddediliyor.
KYS: Taslağın klasörü değişince doküman kodu yeni klasörün kod bloğundan yeniden veriliyor. Eski numara bir daha kullanılmıyor. Elle girilmiş ya da toplu içe aktarımla gelmiş kodlar korunuyor; aynı kod bloğunu paylaşan klasörler arasında taşımak kodu değiştirmiyor. Klasör değiştirildiğinde ekranda bununla ilgili ipucu çıkıyor.
KYS: Kod yeniden verildiğinde taslağın paylaşım bağlantıları yeni koda taşınıyor. Bağlı Word belgesinin başlığındaki kod da güncelleniyor; güncellenemezse kayıt yine yapılıyor ve uyarı gösteriliyor.
KYS: Klasörün yetki matrisini görme yetkiniz yoksa Hazırlama ekranı ve Tarama merkezi artık "tanımlı yetki yok" demiyor. Bunun yerine "görme yetkiniz yok" yazıyor.
KYS Ayarlar → Araçlar altına Rol ve yetki ekranı eklendi. Buradan KYS yetki rolleri oluşturulup kopyalanıyor ve siliniyor. Her rolün yetki ağacında KYS modüllerinin ekranları ve işlemleri için "yetki ver", "yetkiyi kaldır" ya da "yalnız okuma" seçiliyor. İşaretlenmeyen satır üst satırdan devralıyor. Ağaçta arama, "alt kırılımlara uygula" ve başka bir rolden (tamamını ya da tek bir dalını) yetki kopyalama da var.
KYS: Yönetim → Kullanıcılar → Roller penceresine KYS yetki rolleri bölümü eklendi. Kullanıcıya KYS yetki rolü buradan veriliyor ve kaldırılıyor.
KYS: Yetki grubundaki işlem yetkileri artık gerçekten uygulanıyor. Rolünde kapalı olan bir işlemi deneyen kullanıcı reddediliyor ve mesajda hangi yetkinin eksik olduğu yazıyor. Kullanıcının gerçek yetkisi, rol yetkisiyle klasör yetkisinin kesişimi: Ağaçta "Onayla" açık olsa bile o klasörde Onay yetkisi yoksa onaylayamaz.
KYS: Klasör yetki matrisine Ek sil yetkisi eklendi. Klasör yetkilendirme, Klasör yetkileri (toplu) ve toplu içe aktarım şablonunda görünüyor. Ek silme artık Yazma''dan ayrı. Mevcut satırlarda Yazma ile aynı değerle geldiği için bugünkü davranış değişmiyor.
KYS: Hazırlama sihirbazının 5. Adımındaki doküman yetki satırlarına açıklamalı Yazma, Onay ve Ek sil sütunları eklendi. Bu üç yetkide yönetici otomatik muaf değil; kilitlenmemek için revizyonla kendine satır ekleyebiliyor. Revizyonla verilen Yazma ve Ek sil yetkisi, revizyon onaylanınca geçerli oluyor.
KYS: Doküman düzeyindeki yetki satırları artık her yerde uygulanıyor: Taslak, PDF, ekler, listeler, raporlar, içerik haritası, kontrollü kopya, ilişki grafiği ve Word düzenleyici.
KYS: Gruba verilen okuma yetkisi ve gruba konan yasak artık doküman listelerinde, aramada ve panolarda da uygulanıyor; bu, hem klasör yetkisi hem dokümana özel yetki satırları için geçerli. Önceden gruba yasaklanan bir doküman listede görünebiliyordu.
KYS: Bir taslak artık yetkiniz olmayan bir klasöre ya da klasörden taşınamıyor.
KYS: Yetki ekranları bir reddi artık "Grup yok." yerine gerekçesiyle gösteriyor.
KYS: Klasörün doküman hazırlama şablonu artık çöp kutusuna atılabiliyor ve geri alınabiliyor. Önceden ekrandan hiçbir yolla kaldırılamıyordu; bu da şablonlu klasörü silinemez yapıyordu. Çöpe atmak "Ek sil" yetkisini ve o klasörde İptal yetkisini, geri almak da o klasörde İptal yetkisini istiyor. Klasörü silinmiş bir şablon geri alınamıyor.
KYS: Onay listesinde yalnız kişisi seçilmemiş satırlar bulunan bir doküman onaya gönderilebiliyor ve hiçbir onay aşamasından geçmeden, imza kaydı oluşmadan yürürlüğe girebiliyordu. Artık onaya göndermek için en az bir gerçek onaycı gerekiyor.
KYS: Dağıtım listesinde hedefi boş ya da farklı biçimde girilmiş satırlar olduğunda doküman tebliğ yazılmadan yayına çıkabiliyordu. Artık listedeki her geçerli satır tebliğ üretiyor.
KYS: İptal ya da arşiv edilen dokümanın açık onay talebi artık kapanıyor. Önceden son onaycı görevini onaylayınca doküman kendiliğinden yeniden yürürlüğe dönebiliyordu.
KYS: Personel ya da pozisyon olarak tanımlanmış revizyon kontrolcüsü artık tanınıyor. Önceden "kontrolcü yok" sanılıyor ve kontrol adımı atlanıyordu.
KYS: Başkasının düzenleme kilidini artık yalnız yönetici ya da rolünde bu yetki olan kullanıcı kırabiliyor; önceden herkes kırabiliyordu.
KYS: Dış paylaşım bağlantısı çöpteki dosyayı artık sunmuyor ve çöpteki dosya için indirme düğmesi çıkmıyor.
KYS: Talep sahibi kendi değişiklik talebini değerlendirme ve onay aşamalarından kendisi geçiremiyor. Görev ayrılığı artık aşama ilerletmede de uygulanıyor; buna Operasyon Merkezi''ndeki "✓ Onayla" da dahil.
KYS: Aksiyon listesinde satırdaki "Yönlendir" artık çalışıyor: Kaydın detayını açıp kişi seçimini getiriyor. Önceden yalnız bir bildirim gösteriyor, yönlendirme yapmıyordu.
KYS ana ekranındaki "Reddedilen Şikâyet Talepleri" listesi herkese kurumun tüm reddedilen taleplerini gösteriyordu. Artık talep kuyruğunu görme yetkisi olmayan kullanıcı yalnız kendi açtığı taleplerin reddini görüyor.
KYS: Olay bildiriminde kayıttaki "bildiren" artık her zaman oturum açan kişi. Formdaki alana başka bir ad yazılırsa kayıtta "(… adına)" olarak yanında görünüyor.
KYS: Karar yetkisi olmayan biri kendi önerisine "kabul puanı" yazdırabiliyordu; öneri sonradan reddedilse bile puan liderlik sıralamasında kalıyordu. Puan artık yalnız gerçek bir kabul kararında yazılıyor.
KYS: Alt süreçler tanımı eklendi. Kod süreç kodu ile alt koddan oluşuyor (ör. 03.02); klasöre yalnız alt kod yazılıyor.
KYS: Makina tanımlama ekranı eklendi. Her makina bağlı olduğu hatta ya da iş yerine bağlanıyor.
KYS: İş yeri tanımına "Bağlı olduğu fabrika / iş yeri" alanı eklendi; hat, fabrikasına bu alanla bağlanıyor.
KYS: Yönetim sistemleri ekranı açıldı. DÖF kayıtlarının bağlandığı liste (ISO 9001, IATF 16949…) artık yönetilebiliyor. Yeni kurulumlarda liste 4''ten 10 sisteme çıktı.
KYS: Doküman tiplerine "Tür klasörü adı" alanı eklendi. Hazır şablonlarda bu tipin klasörüne verilecek adı belirliyor.
KYS: Yeni kurulumlarda varsayılan doküman tipleri artık kısa kodlarla geliyor (PR, TL, FR…; 20 tip). Önceden ilk doküman "prosedur-001" gibi bir kod alıyordu.
KYS: Kişi tipi olarak pozisyon (koltuk) kullanılabiliyor. Boş koltuk hata değil uyarı veriyor.
KYS: Klasörler sayfasına "Doküman sahibi", "Kontrol edenler" ve "Gözden geçirenler" kolonları eklendi; önceden kontrol tipi yazılabiliyor ama kişi atanamıyordu. Çözülemeyen kişi hata veriyor; boş hücre üst klasörden devralmayı bozmuyor.
KYS: Ret (yasak) satırı olan bir klasöre aktarım bu ret kararlarını siliyordu ve yasaklanmış kişi erişim kazanıyordu. Yetkiler sayfasına "yasak" kolonu eklendi. Mevcut ret satırları değişecekse ayrıca uyarı veriliyor.
KYS: Doküman sahibi personel ya da pozisyon olarak tanımlıysa revizyon talebi bildirimi artık ona ulaşıyor.
KYS: Klasör onaycısı personel ya da pozisyon olarak tanımlıysa erişim talepleri artık onaycıya düşüyor.
Doküman Yönetimi (DMS): Ortak Çalışma Alanı''nda bir KYS klasörü için dış bağlantı açmak artık yetki istiyor; önceden klasörde hiç yetkisi olmayan biri açabiliyordu. Klasörde Okuma ve İndirme, alıcıya yazma hakkı veriliyorsa Hazırlama da gerekiyor. "Yayımlanmayacak" klasörde bağlantı açılamıyor. Bağlantı adresi yalnız paylaşımı açan kişiye gösteriliyor.
Doküman Yönetimi (DMS): Ortak Çalışma Alanı''nın dış paylaşımını kapatmak artık yönetici ya da rolünde bu yetki olan kullanıcıya açık. Önceden herkes başkasının paylaşımını da kapatabiliyordu.
Doküman Yönetimi (DMS): Belge şablonu kütüphanesini (Sözleşme modülü de kullanıyor) ve adlandırma şemasını değiştirmek ya da silmek artık yönetici ya da rolünde bu yetki olan kullanıcıya açık. Önceden her kullanıcı değiştirebiliyordu.
Sandık: Operasyon ekranından indirilen aylık kesinti dosyasına borç satırında borcun toplam tutarı yazılıyordu. Artık o ayın taksit tutarı (anapara + gider payı) yazılıyor.
Sandık: Aidat Paneli''ne "Şirket" kolonu eklendi. Aidatlar artık şirkete göre süzülebiliyor.
Form Tasarımcısı: Seçenekler penceresine "Kullanıcılar" sekmesi eklendi. Açılır liste, radyo, çoklu seçim, liste kutusu ve arama listesi alanları doğrudan kurumun kullanıcı listesinden doldurulabiliyor. Yeni açılan bir kullanıcı listeye kendiliğinden geliyor.
Form Tasarımcısı: Bu kullanıcı listesi gruba, role veya yalnız aktif hesaplara göre süzülebiliyor. Grup ve rol birlikte seçilirse ikisine birden uyan kullanıcılar listeleniyor. Liste kaydetmeden önizlenebiliyor.
Form Tasarımcısı: Alan paletine hazır ayarlı "Kullanıcı seç" alanı eklendi. Alanda ad soyad görünüyor, kayda kullanıcı kimliği yazılıyor. Böylece kullanıcı adı değişse de kayıt bozulmuyor.
Form Tasarımcısı: Kullanıcı seçme alanlarında arama, kurumdaki ilk 1000 kullanıcıdan sonrakileri hiç bulamıyordu. Artık bütün kullanıcılar içinde aranıyor ve Türkçe I/İ harfleri doğru eşleşiyor.
EBYS: TürkKEP üzerinden gelen evrakın içeriği ve ekleri artık indirilip görüntülenebiliyor. Önceden belge görüntüleyicide gerçek içerik yerine örnek şablon ve örnek ek görünüyordu. PTT KEP hattında ek indirme henüz yok.
EBYS: Belge görüntüleyicide gelen iletinin bütün ekleri listeleniyor ve tek tek indirilebiliyor. Önizlenebilir ana belge yoksa bu ekranda belirtiliyor.
EBYS: "Gelen Havaleler" sayfasının adı "Havaleler" oldu ve sayfaya "Havale Ettiklerim" sekmesi eklendi. Havale eden kişi, başlattığı havalelerin durumunu iade sonrası dahil görebiliyor (Bekliyor / Onaylandı / İade / İptal).
EBYS: Bilgi amacıyla havale edilen kullanıcılar evrakı havale listesinde göremiyordu. Artık evrak "Bilgi" etiketiyle, yalnız görüntülenebilir olarak listeleniyor.
Akanlar: Ekosistem menüsüne Akanlar (Tedarikçi Kıyas) uygulaması eklendi. Bu bir ön sürüm; ürün × tedarikçi teklif karşılaştırması yapıyor.
Akanlar: Kıyas ekranı eklendi: Kriter sekmeleri, "En İyi 1 / 3 / Tümü" süzgeci, en iyi teklifin yeşil vurgusu, hedef fiyattan sapma, kalite rozeti ve veriye dayalı skor. Skor elle düzeltilebiliyor.
Akanlar: Pivot & Grafik ekranı eklendi: Renklendirilmiş etkileşimli pivot tablo ve tedarikçi karşılaştırma grafiği.
Akanlar: Talep ve Teklif formlarıyla girilen teklifler, talep numarasına göre Kıyas ekranına canlı yansıyor. Henüz teklif yoksa veya Akanlar o kurumda kurulu değilse ekran hata vermeden örnek veriyle açılıyor.
Akanlar: Uygulama telefonda ve dar ekranda da düzgün çalışıyor: Üst sekmeler, pivot yerine sade tablo, kart görünümünde kıyas. Grafik sayfayı yana taşırmıyor ve pencere daraltılınca sayfa bozulmuyor.
Entegrasyonlar: Süreçlerdeki Logo ERP adımı artık gerçekten çalışıyor: Akış bu adıma gelince eşlenen alanlarla Logo''ya kayıt gönderiyor. Önceki sürümlerde bu adım seçilebiliyor ama hiçbir iş yapmıyordu.
Entegrasyonlar: Logo ERP adımına Logo nesne seçici (36 nesne) eklendi. Başlık alanları aranıp eklenerek, detay/satır alanları ayrıca eşlenebiliyor.
Entegrasyonlar: Logo ERP adımının sonucu ve hatası ayrı süreç değişkenlerine yazılıyor. Logo hata verse de akış durmuyor ve sonuca göre koşullu dallanma yapılabiliyor.
Entegrasyonlar: Entegrasyon Ayarları → Logo bölümüne "Şema dökümü" düğmesi eklendi. Düğme, Logo''daki nesnelerin alan yapısını salt okunur olarak çıkarıyor.
Kurulum / Dağıtım: Kurulu kurumlarda eski uzun doküman tipi kodlarını (ör. "prosedur") kısa koda (PR) çeviren bir onarım işlemi eklendi. İşlem önce deneme olarak çalışıyor ve yalnız hiç değiştirilmemiş varsayılan kodlara dokunuyor. Çakışan satır atlanıp raporlanıyor. Ekranda düğmesi yok; destek ekibi çalıştırıyor.
Müşteriye Özel: TREDA kurulumu için eski River sistemindeki formlar ve süreçler RiverAI''ye taşındı. Diğer kurumları etkilemez.
Müşteriye Özel: Taşınan formlar: Cari hesap, hizmet kartı, satış koşulları, fiyat, iade ve sözleşme formları ile stok/maliyet/fiyat raporları.
Müşteriye Özel: Satış Sözleşmesi, Ürün İade, Fiyat Değişikliği ve Hizmet Kartı onay süreçleri, süreç sonucunu bildiren e-postalarla birlikte taşındı. Fiyat artışı %10''u aşan talepler otomatik reddediliyor.
Müşteriye Özel: Sözleşme ve fiyat kartı bitişine 60 gün kala hatırlatma gidiyor; süresi dolan sözleşmenin durumu kendiliğinden "Süresi Doldu" oluyor.
## İyileştirmeler
Yöneticiler İçin: "Klasör yetkilendirme" (eski Klasör tanımlama) eski yetki kalemiyle devam ediyor. Güncelle, Klasör kodu değiştir, Kopyala, Taşı ve Link üret için artık bu yetki ve klasörde yazma yetkisi gerekiyor; yetkisi olmayan kişi düğmeleri kapalı ve gerekçesiyle görüyor. Yetki grubu atanmamış kullanıcılar da bu düğmeleri artık kapalı görüyor. Bu işleri yapacak kişilerin yetki grubuna "Klasör yetkilendirme" ekleyin.
Yöneticiler İçin: Klasör oluşturma''da taslak yayını bir klasörün devraldığı yetkiyi değiştiriyorsa (ör. Klasör taşınıyorsa) ayrıca "Klasör yetkilendirme" yetkisi gerekiyor. Taslağı şablonun kod ayarıyla yayınlamak da ayrıca sistem parametrelerini düzenleme yetkisi istiyor.
Yöneticiler İçin: KYS yetki rolleri kullanıcıya rol atandığında devreye giriyor; rolü olmayan kullanıcıların bugünkü erişimi değişmiyor. Hiç işaret taşımayan (boş) bir role bağlı kullanıcı ise KYS işlemlerinde reddediliyor; rolü kullanıcıya atamadan önce yetki ağacını doldurun.
Yöneticiler İçin: Klasör yetkilendirme''de zorunlu alanlarda daha önce yapılmış "Boş" seçimleri yeni kurala dönüştürülmedi. Üst klasörde değeri olan alanlarda "Boş" seçimini yeniden yapın.
Yöneticiler İçin: Toplu içe aktarımda "kilitliAlanlar" kolonu artık "kontrolMatrisi" değerini kabul etmiyor (kilitlenebilir alan sayısı 13''ten 12''ye indi). Eski Excel dosyalarından bu adı çıkarın ya da şablonu yeniden indirin.
Yöneticiler İçin: Klasör güncellemenin eski entegrasyon yolu kaldırıldı. Dış sistemden klasör güncelleyen kurumlar yeni klasör ayarı ve klasör yapısı işlemlerine geçmeli.
Yöneticiler İçin: Logo bağlantı ayarlarındaki "REST modu" seçeneği kaldırıldı. Bağlantı ve bağlantı testi artık her zaman REST üzerinden yapılıyor; eski DLL modu desteklenmiyor. Logo''ya DLL moduyla bağlanan kurumlarda güncellemeden sonra Entegrasyon Ayarları → Logo bölümünden bağlantı testini çalıştırın.
KYS: Daha önce verilmiş doküman kodları değişmiyor. Klasörün kodu ya da yeri sonradan değişse de dağıtılmış kodlar aynen kalıyor.
KYS: Doküman kodu 64 karakteri aşacaksa kayıt anlaşılır bir mesajla duruyor. Önceden anlaşılmaz bir hata çıkıyordu.
KYS: Taslak otomatik kaydedilmiyor, "Kaydet" ile kaydediliyor. Kaydedilmemiş değişiklik varken sekmeyi kapatmak, şablonlara geçmek ya da tarayıcıdan çıkmak onay istiyor.
KYS: Sistemin kullandığı kök klasörlerin (EBYS Evrakları, İK, Sözleşmeler, Mükerrer) adı ve yeri taslaktan değiştirilemiyor. Bu adlarla yeni kök klasör de açılamıyor.
KYS: "Klasör tanımlama" ekranının adı Klasör yetkilendirme oldu. Bu ekran artık yayınlanmış ağaçta yetki matrisini ve klasör ayarlarını yönetiyor.
KYS: Yeni klasör açmak, doküman tipini ve "koda dahil" ayarını değiştirmek Klasör oluşturma''ya taşındı. Bunun için "Klasör oluşturmada aç" ve "Yapıyı düzenle →" bağlantıları eklendi.
KYS: "Klasör kodu değiştir" kaydetmeden önce etkilenen doküman sayısını, kod bloğu değişecek klasörleri ve sıradaki doküman kodunu gösteriyor. Yapılamayan değişiklik engelleniyor. Kod yalnız harf, rakam ve alt çizgiden oluşabiliyor, en fazla 16 karakter.
KYS: "Klasörü taşı" önizlemesi yetkisi değişecek ve herkese açılacak klasörleri gösteriyor. Üst klasörden gelen bir yasağın kalkması kırmızı uyarıyla işaretleniyor. Taşımadan sonra artık bağlamayacak alan kilitleri de uyarı olarak çıkıyor; değeri tanımlanmamış (zaten etkisiz) kilitler için uyarı çıkmıyor.
KYS: Taşıma (ya da taslak yayını) bir klasörün erişimini genişletecekse ayrıca onay isteniyor. Onaydan sonra genişleyen klasörler değişirse onay kendiliğinden kalkıyor ve güncel listeyle yeniden isteniyor. Çoklu taşımada onay yalnız önizlenen klasör için geçerli.
KYS: Zorunlu alanlarda "Boş" artık gerçekten boş getiriyor, üst klasörden de değer gelmiyor. Üst klasörde "Zorunlu" olan alanda "Boş" seçilemiyor.
KYS: "Tümünü klasörden dayat" açılınca "Boş" seçimleri kalkıyor ve bu ekranda söyleniyor. Değeri olmayan alan dokümana dayatılmıyor. Gözden geçirme satırlarında ve doküman tipinde "Boş" kapatıldı.
KYS: Daha önce klasörsüz kaydedilmiş eski taslaklar klasör seçilince kaydedilebiliyor ve kodları değişmiyor.
KYS: Sıradaki kod önizlemesi seçilen klasöre göre hesaplanıyor; ekranda görünen kod kaydedilen kodla aynı.
KYS: Kaydetme hatalarında sunucunun gerekçesi gösteriliyor.
KYS: "Doküman hazırlama talebi" artık hedef klasör seçilmeden açılamıyor. Seçilen klasörde Hazırlama yetkisi gerekiyor; yalnız okuma yetkisi olan kişi talep açamıyor. Talep reddedilirse nedeni ekranda yazıyor.
KYS ekranlarında yetkinizin olmadığı işlerin düğmeleri artık görünmüyor; düğmeye basıp "yetkiniz yok" hatası alma durumu kalktı. Bu şu yerleri kapsıyor: Doküman Hazırlama, Operasyon Merkezi, Askıya Alma, Tarama Merkezi, doküman detayı, Değişiklik Yönetimi, Görevlerim ve ana ekrandaki onay kuyrukları, DİF, Denetim, Şikâyet, Öneri, Aksiyon, Risk, KKD, Arşiv, Kalite Kayıtları, Genel Tanımlar, Ayarlar ve tüm ekranlardaki ek dosya alanı.
KYS: Dosya yükleme alanı yetki yüzünden gizlendiğinde, yerinde hangi yetkinin tanımlanması gerektiği yazıyor.
KYS: "Yalnız okuma" verilen ekran ve listeler açılıyor, ama o ekrandaki işlemler yapılamıyor.
KYS: Yetki ağacında hiçbir yerde uygulanmayan, yani kapatınca hiçbir şey değiştirmeyen seçenekler kaldırıldı. Ağaçta görünen her yetki gerçekten bir işlemi kontrol ediyor.
KYS: Tarama ajanı ve bakım ayarlarını şimdiye kadar yalnız yönetici değiştirebiliyordu; bu yetki artık bir role de verilebiliyor.
KYS: Beş parametre artık ayrı ve kritik bir yetki istiyor. Bunlardan dördü görev ayrılığını ve okundu kaydını gevşeten şirket parametreleri: Kendi onayı serbest, okudum onayı alınmasın, kontrollü kopya yazdırma nedeni zorunlu, ek dosya revizyonda taşınmasın. Beşincisi "Kalite yöneticisi rolü" parametresi. "Kalite yöneticisi rolü" ve "Kendi dokümanını onaylama serbest" parametrelerini ayrıca yalnız sistem yöneticisi değiştirebiliyor.
KYS: Genel Tanımlar ve DİF Tanımlamaları''ndaki her katalog (cari, departman, pozisyon, unvan, ürün grubu, SSS vb.) rolde ayrı ayrı yetkilendirilebiliyor.
KYS: Kullanıcı dizinine o an ulaşılamazsa yasaklı klasör yönetici dışındakilere kapalı tutuluyor. Silinmiş bir grubu gösteren yasak satırı kimseyi kapsamıyor; bu satırları yetki matrisinden temizleyebilirsiniz.
KYS: Klasör ağacında okuma ya da hazırlama yetkiniz olmayan klasörlerin adları görünüyor ama onay/dağıtım listeleri, kilitler ve kurallar gösterilmiyor. Görüntüleme biçimi, saklama süresi, koda dahil ve doküman tipi görünmeye devam ediyor.
KYS: Bir klasörün yetki matrisini yalnız o klasörde okuma ya da hazırlama yetkisi olanlar, yöneticiler ve "Klasör yetkilendirme" yetkisi olanlar görüyor.
KYS: Paylaşım ayarlarını yalnız yöneticiler ve "Klasör yetkilendirme" yetkisi olanlar görüyor.
KYS: Yetki gruplarını, kullanıcı gruplarını ve kullanıcı-yetki eşlemelerini artık yalnız sistem yöneticisi görüntüleyip değiştirebiliyor. Diğer kullanıcılar ekranda gerekçesini görüyor.
KYS: Bir yetki grubuna ekran atamak, grubun adını ya da aktifliğini değiştirmek ayrıca kritik "Gruba ekran ata" yetkisini istiyor. Genel tanım düzenleme yetkisi tek başına buna yetmiyor.
KYS: Personelde aşağıdaki işlemler için sistem yöneticisi gerekiyor: Sistem hesabı bağlamak ya da değiştirmek, hesaplı personelin sicilini değiştirmek, hesaplı yeni personel eklemek ve hesaplı pasif personeli aktife almak. Diğer personel işlemleri Genel Tanımlar yetkisiyle sürüyor.
KYS: Klasör silme artık klasöre bağlı her şeyi denetliyor: Şablon, açık paylaşım bağlantısı ve başka birimin dağıttığı belge. Silme engellenirse mesaj neyin engellediğini sayıyla söylüyor.
KYS: Sistem kök klasörleri (İK, Sözleşmeler, EBYS Evrakları, Mükerrer) silinemiyor.
KYS: Toplu "İptalden geri getir" ekranı alınamayan dokümanları sessizce atlamıyor; "x doküman yürürlüğe alındı, y doküman alınamadı. Nedeni: …" yazıyor.
KYS: Onaylama ve reddetme, elle yayımlama ve yayından kaldırma, askıya alma ve askıdan indirme, iptalden geri getirme ve Word düzenleyicide yazdırma rolde ayrı yetkiler; hangi ekrandan yapılırsa yapılsın aynı yetki soruluyor.
KYS: Operasyon Merkezi''nde toplu işlem menüsü yalnız seçili dokümanların durumuna uyan işlemleri gösteriyor. Uyan işlem yoksa boş pencere yerine nedeni yazıyor.
KYS: "Görev Ayrılığı — Onaylayamadıklarım" bölümünün adı "Onaylayamadıklarım" oldu. Görev ayrılığı yüzünden bekleyenlerin yanında, iptal ya da arşiv edildiği için onayı işletilemeyen dokümanlar da burada listeleniyor. Bu dokümanların onay görevi onaycının kuyruğundan düşüyor. Her satırın "Neden onaylayamıyorum" kolonunda gerçek neden yazıyor.
KYS: Maskeli içeriği gören kullanıcı gövdeyi değiştiremiyor, maskeyi kaldıramıyor ve maskeli kaynaktan revizyon başlatamıyor; böylece gizlenen metin kaybolmuyor. Maskeli kaynaktan revizyonu yalnız kaynağın sahibi başlatabiliyor.
KYS: Taslağın sahibi ya da hazırlayanı kendi maskeli taslağını maskesiz görüp düzenleyebiliyor; buna Word''de açma, indirme, karşılaştırma ve taslak PDF''i dahil. Künye maskeli kalıyor. Maskeli gören biri taslağın sahibini değiştirerek bu muafiyeti kendine açamıyor.
KYS: Bağlantı üretmek klasörde hem Yazdırma hem İndirme yetkisi istiyor. İndirme kısıtlı ve "yalnız grup" belgeler paylaşılamıyor; Hazırlama ekranı bu durumlarda paylaşım oluşturma alanını göstermiyor.
KYS: Üretilmiş bağlantılar listesinde her bağlantının gerçek durumu görünüyor: Aktif, iptal, süresi doldu, kapalı.
KYS: Değişiklik talebinin her aşaması kendi yetkisini istiyor: Talebi göndermek "yeni talep", değerlendirme ve onay adımları "onayla", yayın adımı "yayımla" yetkisi istiyor. Talebi düzenleme ve etki-fayda da ayrı yetkiler.
KYS: Aksiyon kanıt dosyasını ekleme, indirme ve kaldırma rolde ayrı yetkiler. Yetkisi olmayana yükleme alanı yerine hangi yetkinin gerektiği yazılıyor.
KYS: Kısıtlı aksiyon planlarını görmek rolde ayrı bir yetki. Liste, plan detayı ve sayaç rozetleri aynı karara göre çalışıyor. Kısıtlı planın ekleri de planı göremeyene görünmüyor.
KYS: Aksiyon planına ek eklemek ve silmek ayrı yetkiler. Ekleyemeyen kullanıcıya ek seçici gösterilmiyor ama plan sorunsuz oluşturuluyor.
KYS: Plan durumu, elle ilerleme yüzdesi, kalem sorumlusu, kalem ilerlemesi ve geciktirme kendi yetkilerini istiyor.
KYS: DİF''te şu işlemler rolde ayrı yetkiler: Aksiyon tanımlama ve atama, görev adımlarının durumu, DİF iptali (listeden ve toplu iptalden), kapatma kararı ve DİF tanımları. Yetkisi olmayana "Yeni DİF" yerine nedeni anlatan bir not gösteriliyor.
KYS: Denetimde plan düzenleme, soru ekleme, kopyalama ve silme, sonuç kaydetme, denetim kopyalama ve denetim sorusu ekleri rolde ayrı yetkiler. Bulgudan "DİF/Aksiyon Oluştur", hem DİF kaydı hem sonuç kaydetme yetkisi istiyor.
KYS: Şikâyet kaydında her alan ve her aşama geçişi (kapatma, gelişme, gelişme onayı, ekip vb.) kendi yetkisini istiyor. Yalnız gerçekten değişen alanın yetkisi soruluyor; kaydı olduğu gibi kaydetmek engellenmiyor. Kapalı bir şikâyeti yeniden açmak kapatma yetkisi istiyor.
KYS: Olay detayındaki yaralı kişi bilgileri (ad, yaralanma türü, vücut bölgesi, hastane) rolde ayrı ve kritik bir yetkiye bağlandı.
KYS: Öneriyi kabul etme, reddetme ve ek bilgi isteme ayrı yetkiler.
KYS: Kutu taşıma, dışarı verme, iade alma ve imhaya ayırma rolde ayrı yetkiler.
KYS: Yeni kurulumlarda süreç listesi (Yönetim, İnsan Kaynakları, Satın Alma, Üretim…) hazır geliyor. Önceden bu listeyi kullanan ekranlar boş açılıyordu.
KYS: Toplu içe aktarım doküman kodlarını da yeni kurala ve kurumun kod biçimine göre üretiyor. Önizlemede görünen kod, aktarılan kodla aynı.
KYS: Şablonun Rehber sayfası artık RiverAI kullanıcılarını (kullanıcı adıyla, pasif hesaplar işaretli) ve pozisyonları da listeliyor. Önceden yalnız KYS personeli listelendiği için "kullanıcı bulunamadı" hatası alınıyordu.
KYS: Sihirbaz metinleri düzeltildi. Bulunamayan kişiler için hangi kaynaklara bakıldığı ve ne yazılması gerektiği anlatılıyor. Kişi kolonlarında açılır liste olmadığı, değerlerin Rehber''den kopyalanması gerektiği belirtiliyor.
Sandık: Üyeler listesinde ve Raporlar → Aktif Üyeler tablosunda şirket süzgeci artık şirket kodunu değil şirket adını gösteriyor.
Sandık: Ayrılma listesinde onay akışındaki taleplerde "Çıkış Yap" düğmesi yerine "Onay Kuyruğunda" bilgisi görünüyor. Böylece akış yarıda bırakılamıyor.
Sandık: Ayrılma onayında üretilen belgeler direkt çıkıştakilerle aynı oldu: Ayrılma beyanı ve dernek ayrılma formu. Gider payı iade formu artık bu adımda üretilmiyor.
Menü: Bir sürecin adımında kullanılan formlar artık kullanıcı menüsünde ayrıca listelenmiyor. Bu formlara süreci başlatarak ulaşılıyor; menüden açıldıklarında süreçten bağımsız boş kayıt oluşuyordu. Süreci başlatan formlar menüde kalıyor, tasarım modunda ise her şey görünüyor.
EBYS: İndirilen belgeler ve ekler doğru dosya adı ve uzantısıyla iniyor.
EBYS: İçeriği ilk denemede alınamayan KEP evrakı bir sonraki çekimde kendiliğinden yeniden deneniyor.
EBYS: İş listesindeki EBYS sayacı yalnız işlem bekleyen havaleleri sayıyor.
Proje Dışa / İçe Aktarma: Proje paketi artık kayıtlı SQL ve Mongo sorgularını da taşıyor. Bu sorguları kullanan süreç adımları ve form alanları hedefte çalışır hâlde geliyor.
Proje Dışa / İçe Aktarma: Süreçlerin bağlı olduğu pano ve kolon tanımları da pakete giriyor. Panodaki kartlar taşınmıyor.
Proje Dışa / İçe Aktarma: Formlardaki alt form bağlantıları paketle birlikte taşınıyor. Eski paketler de sorunsuz içe aktarılmaya devam ediyor.
Giriş Ekranı: Giriş sunucusu o adres için HTTPS istediğinde ekranda artık "Kullanıcı adı veya şifre hatalı" yazmıyor. Bunun yerine sorunun HTTPS olduğunu söyleyen açık bir uyarı görünüyor.
Müşteriye Özel: Menüde kayıt formu yerine katalog gösteriliyor. Logo ve DMS aktarım adımları Logo bağlandığında devreye girecek.
## Hata Düzeltmeleri
KYS: "Güncelle"de yetki matrisi klasör ayarlarından ayrı kaydediliyor. Biri kaydedilemezse öteki kaybolmuyor ve ekran hangisinin kaydedilemediğini söylüyor.
KYS: Grup yetkileri 100''den fazla üyesi olan gruplarda da doğru çalışıyor.
KYS: "Yalnız grup" kısıtlı dokümanlar artık grup dışındakilere şu yerlerde de görünmüyor: Rapor sayıları, kalite kaydı PDF''i, kontrollü kopya listesi, ilişki grafiği, ekler ve taslak listesi. Bu dokümanlar dış paylaşıma da kapalı.
KYS: "Klasör yetkileri düzenleme (toplu)" ekranında yapılan işlemler klasördeki yasak (ret) satırlarını sessizce izne çevirebiliyordu; "Çıkar" da yasak satırını silip erişimi açabiliyordu. Artık Ekle/Çıkar yalnız seçilen kişinin izin satırını değiştiriyor; diğer satırlar, "Ek sil" yetkileri dahil, olduğu gibi kalıyor.
KYS: Bir doküman başka klasöre taşındığında bağlı taslakları ve Word dosyası da birlikte taşınıyor.
KYS: Klasör kopyalanırken yetki, kopyanın oluşacağı üst klasörde kontrol ediliyor.
KYS: Kısayollar, okuma yetkiniz olmayan doküman ya da taslağın kodunu ve başlığını göstermiyor. Kalite kayıtlarına verilmiş kısayollar da modül yetkisine göre süzülüyor.
KYS: Reddedilen menü ataması ekranda atanmış görünmüyor, eski hâline dönüyor.
KYS: Doküman yetkisi kalıntı taraması, yetkiniz yoksa bunu açıkça söylüyor.
KYS: "Görev ayrılığı istisnası" kaleminin açıklaması düzeltildi. Eski "(henüz zorlanmıyor)" notu yanlıştı; kural onay kararında uygulanıyor, ancak yalnız sizin bulunduğunuz bir onay zincirini göndermeyi açmıyor.
KYS: Çöp kutusu, okuyamadığınız klasörlerin silinmiş taslaklarını ve eklerini listelemiyor. Geri yükleme, kaydın gerçek klasöründeki yetkiye ve o kayıt türünün silme yetkisine bakıyor.
KYS: Hiç onaylanmamış içerikle iptal edilmiş bir doküman "İptalden geri getir" ile yürürlüğe alınamıyor; onaylanmamış içerik ancak onay zincirinden geçerek yürürlüğe girebiliyor. Ret mesajı durumu doğru anlatıyor.
KYS: Yürürlükteki bir KYS dokümanının içeriği "eski sürümü geri yükle" ya da genel doküman yüklemesiyle revizyon açılmadan değiştirilebiliyordu. Artık engelleniyor.
KYS: Doküman içerik PDF''ini indirmek klasörde hem Yazdırma hem İndirme yetkisi istiyor.
KYS: Maskeli dokümanlar artık her görüntüleme yolunda maskeli; önceden maske yalnız PDF çıktısında uygulanıyordu. Künye ve taslak görünümü maskeli metni gösteriyor. Maskesiz görme yetkisi olmayan kullanıcıya Word''de açma, Word indirme ve Word geçmişi karşılaştırması açılmıyor; nedeni yazıyor.
KYS: Taslak listesindeki arama, maskeli görünen taslağın metninde arama yapmıyor; gizli içerik aramayla tahmin edilemiyor.
KYS: Oturum açmış herhangi bir kullanıcı, uydurma bir doküman koduyla kurumdaki herhangi bir dosyayı (ör. Bir aksiyon kanıtı) şifresiz paylaşım bağlantısıyla dışarı açabiliyordu. Bağlantı artık yalnız gerçek bir dokümanın ya da taslağın kendi dosyası için, o dokümanın klasör yetkisiyle üretiliyor. Daha önce üretilmiş bu tür ilişkisiz bağlantılar da artık açılmıyor.
KYS: İptal edilen, askıya alınan ya da klasörü sonradan "yayımlanmayacak" yapılan dokümanların eski bağlantıları artık açılmıyor.
KYS: Elle ilerleme yüzdesi yalnız "elle" modundaki planlarda girilebiliyor.
KYS: Vukuat kaydı açılırken (DİF''ten açma dahil) kapanış bilgileri önceden doldurulamıyor. Kayıt ancak kapanış onayından ve görev ayrılığı kontrolünden geçerek kapanıyor. Bloke kararı aşaması da düzenlemeyle atlanamıyor.
KYS: Kayıt açılırken de alan yetkileri uygulanıyor. Şikâyet "kapalı" olarak ya da DİF tamamlanmış aksiyonlarla veya iptal edilmiş olarak açılamıyor.
KYS: Bir arşiv kutusu düzenleme ekranından "İmha edildi" yapılamıyor; imha yalnız imha defterinin icra adımıyla gerçekleşiyor. İmha edilmiş bir kutu geri alınamıyor ve konumu değiştirilemiyor; ekranda Dışarı Ver ve İade Al düğmeleri çıkmıyor. Tanınmayan bir kutu durumu reddediliyor.
KYS: Dokümanlar sayfasındaki sahip kolonu artık çözümleniyor; önceden doküman hata vermeden sahipsiz oluşuyordu. Değerin başına personel, pozisyon, grup, rol ya da kullanıcı ön eki yazılabiliyor; ön ek yoksa değer kullanıcı adı sayılıyor. Çözülemeyen değer hata olarak gösteriliyor.
Doküman Yönetimi (DMS): Genel doküman listesi ve indirme, KYS klasör yetkilerini atlıyordu; KYS taslaklarının Word dosyası buradan indirilebiliyordu. KYS dokümanları artık bu listede görünmüyor ve buradan indirilemiyor; KYS ekranlarından açılıyor. Yetki matrisi olan klasörlerde liste için Okuma, indirme için İndirme yetkisi aranıyor; matrisi olmayan klasörler eskisi gibi çalışıyor.
Doküman Yönetimi (DMS): Doküman denetim izi ve denetim özeti, yönetici olmayan kullanıcıya yalnız kendi işlemlerini gösteriyor. Önceden kurumun tüm kayıtları kişi, IP ve tarayıcı bilgisiyle görünüyordu.
Sandık: Akış Ayarları''nda akışı tanımlanmamış bir işlem türü için talep oluşturmak hata veriyordu. Artık Sandık Yetkilisi / admin onayına giden varsayılan bir akış kendiliğinden oluşuyor. Bu akış Akış Ayarları''nda görünüyor ve istenirse değiştirilebiliyor.
Proje Dışa / İçe Aktarma: İçe aktarılan projelerde servis (REST/SOAP), Mongo adımları ve form override''daki SQL ile satır doldurma, bağlantılarını bulamadığı için sessizce çalışmıyordu. Bu düzeltildi.
PDF Şablonları: Önceki sürümden yükseltilen kurulumlarda Çıktı Şablonları ekranı, sayfa boyutu özelliği geldikten sonra her açılışta hata veriyordu. Bu düzeltildi.
Kurulum / Dağıtım: HTTPS''siz çalışan kurulumlarda (ör. Yerel ağ IP''siyle erişilen sunucular) yeni açılan kurumlara giriş yapılamıyordu. Ekranda ise yanıltıcı biçimde şifre hatası görünüyordu. Yeni kurumlar artık kurulumun adresine uygun güvenlik ayarıyla açılıyor; HTTPS kullanan kurulumlarda davranış değişmedi.', N'Ara Sürüm', 1, '2026-09-25 09:00:00', '2026-09-25 09:00:00');
  SET @n = @n + 1;
END
PRINT N'River AI sürüm notları: ' + CAST(@n AS NVARCHAR(10)) + N' sürüm eklendi (var olanlar atlandı).';
GO
