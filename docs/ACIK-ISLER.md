# Açık işler ve bekleyen kararlar (7 Kasım 2026 itibarıyla)

## Güvenlik (öncelikli)
- [ ] **Anahtarları yenile:** Aşağıdakiler sohbet geçmişinde açık metin olarak geçti; Claude Code'a geçtikten sonra yenilenmeli ve yalnız `web/secrets.config` / `web/connectionStrings.config` içinde güncellenmeli:
  - SQL Server `sa` şifresi (eski README'de de yazıyordu). Ayrıca portal için `sa` yerine yalnız PARTNER (ve Teklif'te okuma) yetkili **ayrı bir SQL kullanıcısı** açılması önerilir.
  - Apollo API anahtarı · Graph (River Portal Lead) client secret (Entra'da yeni secret, eskisini sil) · Anthropic API anahtarı (kurum hesabı; eski kişisel değerlendirme hesabındaki anahtar da silinmeli).
- [ ] `Auth.CookieKey` değiştirilirse herkes bir kez yeniden giriş yapar (sorun değil).

## Kurulum teyitleri
- [ ] Migration **66** (bilgi tabanı) ve **67** (Lead hitap) canlıda uygulandı mı? Claude Code'a geçince `tools/db-migrate.ps1 -Baseline 65` sonra `-List` (66/67 tekrar çalıştırmaya dayanıklı).
- [ ] Bilgi tabanı içe aktarıldı mı? (Bilgi Tabanı › Kayıtlar'da 142 kayıt; yoksa İçe aktar › `database/data/river-ai-bilgi-tabani.json`)
- [ ] Görev Zamanlayıcı: "River Lead Merkezi" (10 dk) çalışıyor mu; `job=leaves` (saatlik) ve `job=pending` (12:00) görevleri kurulu mu?

## Lead Merkezi: gönderime başlamadan
- [ ] Ayarlar: kutu başına günlük sınır **5**; **İmza alanı boş** (imza şablonlarda); "Takip e-postaları onaysız gitsin" kapalı.
- [ ] Şablonlara imzayı yaz (AI ile düzenle › talimata imza).
- [ ] Onay kuyruğunda **"Hitapları düzelt"**; "tahmin, kontrol edin" kartlarını kontrol et. Mevcut taslaklarda imza yok: elle ekle (ya da toplu imza ekleme düğmesi yapılabilir; önerildi, yapılmadı).
- [ ] Kendine deneme gönderimi (kişisel adres aday olarak) → gelen kutusu, imza, BCC, yanıtın portala düşmesi.
- [ ] İlk gönderiler yalnız **doğrulanmış (verified)** adreslerden; ilk hafta günlük kontrol (geri dönen > 2/gün ya da sağlık < 90 ise durdur).
- [ ] Instantly deneme planının bitiş tarihi (ısınma kesilmemeli).
- [ ] Birkaç hafta sonra DMARC `p=none` → `p=quarantine`.
- [ ] Netoloji domaini engelleme listesine eklenmedi (domaininden emin olunamadı).

## Bilgi tabanı
- [ ] Teyit bekleyen çıkarımlar: `v14-3` (klasöre bağlanmayan süreç görünmez), `v15-4` (AI özellikleri API anahtarına bağlı), `v38-5` (koşullu madde numaralandırması), `v38-7` (taslak süreçler kaydı engelliyor; kullanıcının gördüğü hata mesajı eklenmeli).
- [ ] Menü adları altyazıdan alındı; arayüzdeki adlarla farklı olanlar düzeltilmeli.
- [ ] Öneri: talep kapatılırken **"Bu çözümü bilgi tabanına ekle"** (ekip çözümleri bilgi tabanını büyütür).
- [ ] Öneri: yeni akademi videoları için portaldan otomatik altyazı → kayıt çıkarımı (Claude API ile). Şimdilik `C:\RiverCron\akademi-altyazi.ps1` + elle çıkarım.

## Akademi / içerik
- [ ] 17 ürün kapağını (16:9 WebP) Akademi'deki dokümanlara yükle (İçerik Yönetimi › Düzenle › Kapak görseli).
- [ ] A4 kapakları ürün PDF'lerinin başına ekle (istenirse otomatik birleştirilebilir).
- [ ] **Banka** ve **Sandık** ürünlerinin açılımları teyit edilecek (kapaklarda "Banka Yönetimi", "Sandık Yönetimi").
- [ ] Videoların numara sırası için başlıkların başına numara (ör. "01 · …"); sıralama doğal ve başlığa göre.

## Ayrı projeler (bu depoda değil)
- River AI ürününe kurum içi talep/ticket modülü taşıma (analiz yapıldı).
