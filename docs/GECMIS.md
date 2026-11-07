# River Asistan Portal: Proje Geçmişi ve Kararlar

> Temmuz–Kasım 2026 arasında Claude.ai sohbetlerinde yapılan bütün geliştirmelerin özeti. Her bölüm bir sohbet dönemidir. Kararların **neden** alındığı da yazıyor; bir şeyi değiştirmeden önce buraya bakın.

## Proje sahibi ve çalışma biçimi

- **Sahip:** Yılmaz Güney (Fol Yazılım A.Ş., River AI Platform). Ürün, satış/iş geliştirme ve teknik geliştirmeyi birlikte yürütüyor.
- **Dil:** Bütün arayüz ve iletişim **Türkçe**. İngilizce arayüz katmanı var (`js/i18n.js`, ~1.500 girdilik sözlük) ama ana dil Türkçe.
- **Önceki dağıtım biçimi:** Her değişiklik bir ZIP olarak teslim edildi; ZIP sunucuda PowerShell ile açılıp `web\` klasörü IIS klasörünün üzerine kopyalandı, migration'lar SSMS'te elle çalıştırıldı. Claude Code'a geçişle bunun yerini `tools/` betikleri alıyor (bkz. `docs/TASIMA.md`).
- **Sürümleme:** Her teslimde `APP_VERSION` (js/common.js) artırıldı; değişen CSS/JS dosyalarının `?v=` numarası bütün `.aspx` dosyalarında güncellendi (tarayıcı önbelleği).

## 1) 24 Temmuz: İlk taslak (ASP.NET Core) → vazgeçildi

İlk taslak ASP.NET Core 8 + EF Core olarak kuruldu; sunucuda derleme/yayın adımı gerektirdiği için **klasik WebForms + ADO.NET'e geçildi** (kopyala-çalıştır, derleme yok, `bin/` yok). Bütün sonraki iş bu yapı üzerinde.

## 2) Temmuz sonu–Ağustos: Partner & Vendor Portalı temeli

Partner (bayi) paneli (hakediş, kademe, teşvik), vendor kontrol merkezi (bayi matrisi, demo kuyruğu, vekaleten görüntüleme), anlaşma/sipariş kayıtları, koyu/açık tema, Eğitim Merkezi (YouTube, iki seviyeli kategori, lightbox oynatıcı), Satış Materyalleri, kademe rozetleri, kullanıcı yönetimi, şifre sıfırlama, Office 365 SMTP bildirimleri, Chart.js'li vendor panosu, kademe komisyon oranları, mobil hamburger menü.
**Kural doğdu:** C# 5 (string interpolation ve `?.` yok), `using System.Text;` unutulmasın, HTML alanlarında `Request.Unvalidated.Form`, **arayüzde uzun/orta tire (— –) kullanılmaz** (orta nokta · kullanılır).

## 3) Eylül ortası: River Akademi, destek talepleri, çoklu izin

Netflix tarzı eğitim merkezi (sonra adı **River Akademi**), destek talebi sistemi (talep, mesaj, ek, görüldü/hatırlatma, otomatik kapanış), sürüm duyuruları, çoklu izinli vendor kullanıcıları (`Permissions`), uygulama turu, avatar sistemi, e-posta tasarımı. Yükleniyor perdeleri, bildirim zili, TR/EN, firma bazlı destek bakiyesi, buzlu cam arayüz panelleri, firma bağlı API anahtarları ve Destek API'si (`docs/SUPPORT_API.md`).

## 4) Eylül sonu: Akademi sınav, sertifika, dosya paylaşımı, izleme listeleri

Hizmet formu / proje kayıtları, toplu bildirim (5 e-posta teması), firma analiz panosu + talep PDF'i, mobil/Safari düzeltmeleri, bayi/müşteri kart listeleri. Akademi sınav sistemi ve **QR doğrulamalı sertifika** (`/verify`, 10 renk rozet paleti, Cormorant Garamond tasarım), WeTransfer benzeri **dosya paylaşımı** (`/t/{link}`, parçalı yükleme, web dışında `transfer_data`), izleme listeleri (kaldığı yerden devam, hız, otomatik sonraki). Migration 29–34.

## 5) Ekim başı: v2 geliştirme

İngilizce arayüz katmanı, karşılama videosu sistemi (role göre, ilk girişte bir kez), sürüm notları toplu aktarımı (53 sürüm), sürümlerde arama, **Alacak Takibi** (River Teklif siparişlerinden; yalnız Finans izni; haftalık özet Yılmaz ve Deniz'e), satış kanalı alanı, YouTube eğitim kapakları (Lavanta / Gece / Degrade temaları), Destek API belgeleri, akademi kategorileri ve öğrenme yolu.

## 6) Ekim ortası: Ana sayfalar, forum, talep modülü analizi

Rozet bildirimleri, Takım Arkadaşlarım, destek eki önizleme (`TicketFile.ashx`), Satış Materyalleri görüntüleyici. Partner ana sayfası "Komuta Merkezi", Satış Önerileri, müşteri ana sayfası, bayi personeli ve Fol personeli panelleri (`Staff.aspx`). **Forum** (gönderi/yorum/beğeni, profil, moderasyon, kural onayı, etiketler, okunmamış sayacı). Manuel alacak kaydı. Kurum içi talep/ticket modülünü River AI ürününe taşıma analizi (ayrı proje). Lisans modülü (sonra kaldırıldı), bekleyen talepler günlük özeti (12:00), talep yaşı sütunu.

## 7) Ekim sonu: Akademi yeniden tasarım, satış materyalleri, İzinlerim

Alacaklarda ara tahsilatlar; Akademi ana ekranı yeniden tasarımı (kategori rafı, ızgara, 9 dokulu degrade tema, 44 geometrik kategori ikonu `assets/caticons.svg`, kendi ikonunu yükleme), WebP MIME tanımı. Satış Materyalleri yeniden tasarımı (kategori kartları, "Bana gönder" + BCC takibi, içerik yönetimi), PDF salt okunur görüntüleyici (pdf.js), Lisanslar modülü kaldırıldı. **İzinlerim** (izin talepleri; kişi bazlı onaycılar; Kurallar sekmesi yalnız süper admin; 13 e-posta şablonu; saatlik iş).

## 8) 28 Ekim–7 Kasım: Lead Merkezi, Bilgi Tabanı ve son düzenlemeler (v2.48 → v2.60)

### Lead Merkezi (otomatik aday bulma ve soğuk e-posta)
- **Amaç:** Türkiye'deki danışmanlık firmalarına (iş ortağı adayı) ve potansiyel müşterilere ayrı ayrı tanıtım e-postası; yanıtların otomatik yakalanması. Hedef günde 20–30 kişi; başlangıç kutu başına günde 5.
- **E-posta altyapısı:** Ayrı gönderim domaini **fol.solutions** (GoDaddy DNS, Microsoft 365). Kutular `satis@fol.solutions` (müşteri) ve `sales@fol.solutions` (danışman). DKIM açık, DMARC `p=none` (birkaç hafta sonra `quarantine`), SPF/MX tamam; fol.solutions → river.com.tr yönlendirmesi. mail-tester 9.6/10. Isınma Instantly ile (28 Ekim'de başladı, sağlık %100).
- **Graph:** Entra uygulaması "River Portal Lead" (Mail.Send, Mail.ReadWrite). Exchange **Application Access Policy** ile uygulama YALNIZ iki fol.solutions kutusuna erişebilir (diğer kutular Denied; test edildi).
- **Modül:** Aday havuzu (CSV/Excel: Apollo, Instantly, Türkçe Excel; Apollo API), otomatik eleme (portal müşteri/bayi domainleri, kendi domainlerimiz, ücretsiz e-posta, 22 rakip domain; rakiplerin bayileri hedef olabilir), şablonlar (düz metin/HTML, yer tutucular), taslak → **onay kuyruğu** (yalnız onaylı e-postalar gider), gönderim motoru (hafta içi 09:30–17:30, resmî tatil hariç, kutu başına günlük sınır, e-postalar arası 3–8 dk, 3 ve 5 iş günü sonra takipler, yanıt gelince dizi durur), yanıt okuma ve sınıflandırma (Claude ya da kural tabanlı), Yanıtlar sekmesi, pano, BCC (her e-posta `yilmaz.guney@river.com.tr`).
- **AI:** Taslağa özel "AI ile kontrol et" (ücretsiz ön kontrol + Claude) ve "AI ile düzenle" (firma profiline göre; River AI hakkında yalnız taslaktaki bilgiler). Şablonlar için talimatlı "AI ile düzenle" (v2.58.0).
- **Kararlar (v2.58.0):** E-postalar **düz metin**. Hitap **her zaman "Ad Bey / Ad Hanım"**, asla ad soyad (otomatik e-posta izlenimi verir). Cinsiyet önce Türkçe isim listesinden (`App_Code/TrNames.cs`), belirsizse Claude tahmini + kuyrukta "kontrol edin". **İmza otomatik eklenmez;** şablonun içine yazılır (Ayarlar'daki imza alanı boş). Ret cümlesi otomatik eklenir (yasal). Migration 67.
- **Zamanlanmış görev:** `C:\RiverCron\lead-cron.ps1`, 10 dakikada bir (Görev Zamanlayıcı "River Lead Merkezi").

### Destek: ürün seçimi ve AI ön yanıtı
- **v2.55.0:** Talep formunda modül seçimi kaldırıldı; yalnız **River AI** / **River PRO** (eski "River" değeri River PRO gösterilir). Ürün önceden seçili gelmez. River AI seçilince "yanıtlar yalnız River AI içindir" uyarısı. Ortak yükleniyor perdesinin doğrulama sonrası açık kalma hatası düzeltildi.
- **Bilgi tabanı (v2.56.0):** River Akademi'deki 48 videonun YouTube altyazıları (`C:\RiverCron\akademi-altyazi.ps1` ile indirildi) temizlenip **142 konu kaydı** çıkarıldı (adımlar videodaki saniyesiyle, uyarılar, anahtar kelimeler, kullanıcı ifadeleri). Dosya: `database/data/river-ai-bilgi-tabani.json`. Tablolar: migration 66 (`KbEntries`, `KbAnswers`). Yönetim: `KnowledgeBase.aspx` (pano, kayıtlar, içe aktar).
- **AI ön yanıt akışı:** River AI seçili talep gönderilmeden önce portal kendi aramasıyla en uygun kayıtları bulur, Claude yalnız bu kaynaklara dayanarak adım adım yanıt yazar (kaynakta yoksa "bulunamadı"). Panel: adımlar + "▶ dakika" bağlantıları (Akademi oynatıcısını o saniyeden açar: `Training.aspx?play=…&t=…`), "Sorunum çözüldü" / "Çözülmedi, talebi gönder" / "Talebi düzenle". Çözülmezse ön yanıt talebe **dahili not** olarak düşer. River PRO, Kritik öncelik ve Özellik Talebi'nde atlanır. Kişisel bilgiler maskelenir; kullanıcı başına saatte 20 AI yanıtı. `Kb.PreAnswer=0` ile kapanır. Bekleme ekranı: River AI rozeti animasyonu, "River AI'ya bağlanılıyor", "Sıcak" mesaj akışı (v2.56.1–3).
- **Sınama:** 16 gerçekçi talep cümlesinin 16'sında doğru kayıt ilk sırada.

### Akademi ve içerik yönetimi
- **v2.57.0:** Video düzenlemede bağlantı değiştirme (bağlantı, youtu.be, Shorts ya da embed kodu; yalnız 11 karakterlik kimlik saklanır; video değişince süre sıfırlanır, bilgi tabanı kayıtları yeni videoya bağlanır).
- **v2.57.1:** Alt kategorideki videolar başlığa göre **doğal Türkçe sırada** (1, 2, … 10); kategori kapağı listedeki ilk video.
- **v2.58.1:** Doküman kapağını sonradan değiştirme/kaldırma (İçerik Yönetimi › Düzenle).
- **v2.59.0:** İçerik Yönetimi'nde arama, filtre (tür, kategori, durum, öne çıkan) ve sütun sıralaması; durum oturum boyunca korunur. Arama kutusu form dışında, otomatik doldurma kapalı (e-posta önerisi çıkmaz).

### Oturum
- **v2.60.0:** Kalıcı oturum: imzalı `rv_auth` çerezi (HMAC, `Auth.CookieKey`), 30 gün ve kullanıldıkça yenilenir. Sunucu oturumu (60 dk boşta kalma, uygulama yeniden başlaması, güncelleme) düşse de kullanıcı çıkışa düşmez. Şifre değişince eski çerezler geçersiz, pasif kullanıcı dışarıda kalır, Çıkış çerezi siler.

### Görseller (kodda değil, `tools/kapaklar/` üreticisiyle)
- 17 River AI ürününün **"Ürün Özellikleri" kapakları:** A4 PDF (doküman başına eklenecek) + 16:9 WebP (Akademi'de doküman kapağı). Satış teması: koyu üst bölüm, çapraz kesimli beyaz bant, ürün başına renk geçişi ve Lucide ikonu.
- Akademi doküman kapağı 16:9 şablonu (aynı tasarım dili).

## Kalıcı tasarım ve içerik kuralları (kullanıcının tercihleri)

- Arayüzde **uzun/orta tire yok**; orta nokta (·) ya da cümleyi yeniden kur.
- Türkçe karakterlerde dikkat: `ToLower(new CultureInfo("tr-TR"))`, İ/ı dönüşümleri; aramalar Türkçe karaktere duyarsız (ç→c, ğ→g, ı→i, ö→o, ş→s, ü→u).
- Kullanıcıya görünen metinler sade, resmî ama sıcak Türkçe; teknik jargon yok.
- Sayfayı yenilemeden çalışan formlar `data-noload="1"` alır (ortak yükleniyor perdesi açılmaz).
- Arama kutuları `type="search"`, hiçbir formun içinde değil (`common.js` otomatik doldurma korumasını uygular).
- Yeni özellikler mümkünse tarayıcıda, sayfa yenilemeden çalışır; uzun listelerde sayfalama ya da istemci tarafı süzme.
- E-postalarda Fol/River markası; dahili bildirimler `yilmaz.guney@river.com.tr`.
