# River Asistan Portal: Mimari ve Envanter

> Bu belge koddan otomatik çıkarıldı (dosyaların kendi açıklama satırları). Kod değiştikçe güncel tutun: yeni sayfa/modül/migration eklediğinizde ilgili tabloya bir satır ekleyin.


## Genel yapı

- **Teknoloji:** ASP.NET WebForms (.NET Framework 4.8) + ADO.NET. **Derleme adımı yok:** `.aspx`, `.ashx` ve `App_Code/*.cs` dosyaları IIS tarafından çalışma zamanında derlenir. `bin/` klasörü ve NuGet yok.
- **Derleyici:** C# 5 (Framework'ün yerleşik `csc`). C# 6+ söz dizimi (`$"..."`, `?.`, `nameof`, ifade gövdeli üyeler, `out var`, tuple) **derlenmez.**
- **Sayfa yapısı:** Her sayfa `X.aspx` (görünüm, `CodeFile="X.aspx.cs"`) + `X.aspx.cs` (Page_Load). Sayfaların çoğu HTML'i sunucuda `StringBuilder` ile üretir; etkileşimli modüller ise `*.ashx` JSON uçları + `js/*.js` ile çalışır (sayfayı yenilemeden).
- **Veri erişimi:** `App_Code/Db.cs` (`Db.Open()`), sorguların çoğu `App_Code/Repo.cs` ve modül dosyalarında; **her zaman parametreli** (`AddWithValue`).
- **Oturum:** `Session` (InProc) + imzalı kalıcı oturum çerezi `rv_auth` (`Auth.TryRestore`, `Global.asax` → `Application_AcquireRequestState`). Sunucu oturumu düşse de kullanıcı çıkışa düşmez (30 gün, kullanıldıkça yenilenir; şifre değişince geçersiz).
- **Ortak istemci betiği:** `js/common.js` (menü, tema, bildirim zili, yükleniyor perdesi, arama kutusu otomatik doldurma koruması, sürüm etiketi `APP_VERSION`).
- **Stil:** Tek dosya `css/site.css` (koyu varsayılan tema, `html[data-theme="light"]` açık tema).

## Roller ve yetkiler (`App_Code/Auth.cs`)

| Taraf | Rol / izin | Kısa açıklama |
|---|---|---|
| Fol personeli (vendor) | `Permissions` kümesi: `Portal`, `Editor`, `Support`, `Finance`, `ForumAdmin` | Çoklu izin. `IsVendor` (Portal), `CanEditTraining` (Editor), `IsSupportAgent` (Support), `CanFinance` (Finance) |
| Süper admin | Beş iznin hepsi | `Auth.IsSuperAdmin`. Lead Merkezi, İzinlerim › Kurallar yalnız süper admine açık |
| İş ortağı (bayi) | `PartnerAdmin`, `PartnerUser` | Bayi paneli, talepler, akademi, satış materyalleri |
| Müşteri | `CustomerAdmin`, `CustomerUser` | Müşteri paneli, talepler, akademi |
| Eski roller | `VendorAdmin`, `SalesRep`, `SupportAgent`, `TrainingEditor` | Geriye uyumluluk için hâlâ tanınır |

Vekaleten görüntüleme (impersonation): vendor bir firmayı onun gözünden görür (`imp_company` çerezi).

## Zamanlanmış görevler (Windows Görev Zamanlayıcı → `Cron.aspx?key=…`)

| İş | Sıklık | Ne yapar |
|---|---|---|
| (varsayılan) | Günde bir, sabah | Çözülmüş taleplerde 4. gün hatırlatma, 5. gün otomatik kapanış; ayın 1'i bayi seviyeleri; süresi dolan dosya paylaşımlarının temizliği; alacak takibi (Teklif'ten siparişler, pazartesi haftalık özet); satış önerileri |
| `job=pending` | Hafta içi 12:00 | Bekleyen destek talepleri özeti e-postası |
| `job=leaves` | Saatlik | İzin talepleri hatırlatma / yükseltme / izin öncesi hatırlatma |
| `job=leads` | 10 dakikada bir | Lead Merkezi: yanıt okuma ve sınıflandırma, takip taslakları, gönderim (açık ve mesai içindeyse) |

Sunucuda Lead görevi `C:\RiverCron\lead-cron.ps1` ile çalışır ("River Lead Merkezi" görevi); anahtarı `web.config`/`secrets.config` içinden kendisi okur.

## Dış bağlantılar

| Servis | Kullanım | Ayarlar |
|---|---|---|
| Microsoft 365 SMTP | Bütün bildirim e-postaları (`App_Code/Mailer.cs`) | `Smtp.*` |
| Microsoft Graph (Entra uygulaması "River Portal Lead") | Lead Merkezi gönderim ve yanıt okuma; Exchange erişim ilkesiyle YALNIZ `satis@` ve `sales@fol.solutions` | `Graph.*` |
| Apollo API | Lead aday araması | `Apollo.*` |
| Anthropic Claude API | Lead kişiselleştirme ve yanıt sınıflandırma, AI ile şablon/taslak düzenleme ve kontrol, destek talebi AI ön yanıtı | `Anthropic.ApiKey`, `Anthropic.Model` |
| River Teklif veritabanı (aynı SQL Server) | Alacak takibi ve satış önerileri için siparişler | `Receivables.Source` |
| YouTube | Akademi videoları (yalnız 11 karakterlik kimlik saklanır) | |
| Cloudflare | DNS/CDN, portalın önünde (HTTPS) | |

## Ayarlar

- `web/web.config`: gizli olmayan ayarlar (depoda).
- `web/secrets.config`: gizli anahtarlar (`Smtp.Password`, `Apollo.ApiKey`, `Graph.*`, `Anthropic.ApiKey`, `Auth.CookieKey`, `Cron.Key`). **Depoda değil.** Örnek: `secrets.config.example`.
- `web/connectionStrings.config`: veritabanı bağlantısı. **Depoda değil.** Örnek: `connectionStrings.config.example`.

## Kullanıcı verisi klasörleri (canlıda yaşar; depoda ve dağıtımda yok)

`assets/traincovers` (doküman kapakları) · `assets/avatars` · `assets/catcovers` (kategori görselleri) · `ticketfiles` · `forumfiles` · `materials` · `trainingdocs` · `App_Data/trainingdocs` · `App_Data/leavefiles` · `App_Data/intro` (karşılama videoları) · `..\transfer_data` (dosya paylaşımı, web dışında). `tools/deploy.ps1` bu klasörlere asla dokunmaz.


## Sayfalar (`web/*.aspx` + `.aspx.cs`)

| Dosya | Açıklama | Satır |
|---|---|---|
| `AddSalesMaterial.aspx.cs` | Vendor: satış materyali yayınlar (dosya yükleme veya harici URL). | 57 |
| `AddTicket.aspx.cs` | Bayi / Müşteri: destek talebini kaydeder (dosya + mail). | 141 |
| `AddTrainingDocument.aspx.cs` | Vendor: eğitim dokümanı yükler (her tür; program/betik dosyaları hariç) + opsiyonel kapak. Dosya App_Data altına kaydedilir (dogrudan sunulmaz, calistirilamaz); TrainingFile.ashx ile giris kontrollu sunulur. | 63 |
| `AddTrainingVideo.aspx.cs` | Vendor: River Akademi'ye YouTube videosu ekler (global, tüm bayilere açık). | 38 |
| `ApiKeys.aspx.cs` | Destek API anahtarlari yonetimi (firma bazli anahtar uretme/iptal; yalniz portal yoneticisi) | 119 |
| `ApiTest.aspx.cs` | Yonetici icin API test araci: formu alir, sunucudan gercek endpoint'e POST atar, yaniti gosterir. | 118 |
| `ApproveDemo.aspx.cs` | Vendor: demo talebini onaylar ve partnere onay e-postasi gonderir. | 39 |
| `BookDeal.aspx.cs` | Partner adina anlasma kaydi. YALNIZCA vendor (impersonation) yapabilir. | 81 |
| `Broadcast.aspx.cs` | Toplu Bildirim: secili bayi/musterilere uygulama ici bildirim ve/veya temali e-posta (To: info, alicilar BCC). | 156 |
| `BroadcastPreview.aspx.cs` | Toplu bildirim mail onizlemesi (HTML). Yalniz portal yoneticileri. Ozel HTML temizlenmis haliyle gosterilir; cikarilan ogeler / uyarilar X-Broadcast-Notes basliginda (URL kodlu, / ayrimli) doner. | 23 |
| `Certificate.aspx.cs` | Sertifika belgesi (A4 yatay, yazdir / PDF). Sahibi veya uretici tarafi gorebilir. | 81 |
| `Certificates.aspx.cs` | Sertifikalarim: kullanicinin sertifikalari (yoksa salt okunur bilgi mesaji). | 37 |
| `ChangePassword.aspx.cs` | Self-servis şifre değiştirme (tüm kullanıcılar). AJAX: ok/badold/mismatch/short/no | 27 |
| `Cron.aspx.cs` | Günlük tetiklenir (Windows Görev Zamanlayıcı). Gizli anahtar ile korunur. Çözülmüş taleplerde: 4. günde hatırlatma, 5. günde otomatik kapanış. | 156 |
| `Customer.aspx.cs` | Musteri ve bayi personeli (PartnerUser) ana sayfasi: karsilama + "Yeni neler var?", gostergeler, Devam et / surum / sertifika, talepler, ekip. Her bolum sabit boyutta; kayit yoksa bolume uygun "henuz yok" metni gosterili | 304 |
| `CustomerRegDecision.aspx.cs` | Vendor: musteri kaydi veya yenileme talebini kabul/reddeder. Her durumda partner + vendor bilgilendirilir. | 68 |
| `Default.aspx.cs` | Kok sayfa: oturuma ve role gore ilgili ana sayfaya yonlendirme | 16 |
| `DeleteOrder.aspx.cs` | Siparis sil. YALNIZCA vendor. Silince firma cirosu/kademesi yeniden hesaplanir. | 34 |
| `DeleteSalesMaterial.aspx.cs` | Vendor: satış materyalini kaldırır (kayıt + varsa disk dosyası). | 31 |
| `DeleteTrainingDoc.aspx.cs` | Vendor: eğitim dokümanını kaldırır (kayıt + dosya + kapak). | 32 |
| `DeleteTrainingVideo.aspx.cs` | Vendor: River Akademi videosunu kaldırır. | 18 |
| `DeleteUser.aspx.cs` | Portal admin: kullanıcıyı siler (pasifleştirir). | 22 |
| `Demos.aspx.cs` | Demo Talepleri: bayinin tum demo taleplerini listeler. | 83 |
| `ExamAttempt.aspx.cs` | Editor: bir denemenin detayi - kisinin verdigi cevaplar, dogru cevaplar, soru bazinda puan. Kullaniciya ASLA gosterilmez. | 68 |
| `ExamEdit.aspx.cs` | Sinav editoru: bilgiler, soru ekle/duzenle, puan butcesi (toplam 100), durum gecisleri, sonuclar. | 323 |
| `ExamTake.aspx.cs` | Sinav sayfasi. Kullanici (bayi/musteri): sartlar listesi -> giris -> cozme -> sonuc (yalniz puan) -> isim teyidi -> sertifika. Sorular kullaniciya YALNIZ cozme sirasinda (ve sinav "Yayinda" iken, hic girilmemisse salt ok | 295 |
| `Exams.aspx.cs` | Akademi > Sinavlar: editor tum sinavlari yonetir; bayi/musteri yayindaki ve aktif sinavlari gorur. | 149 |
| `FirmAnalytics.aspx.cs` | Firma (müşteri/bayi) destek analizi: talepler, harcanan süre, en aktif kullanıcılar, PDF indir. | 131 |
| `FirmUsers.aspx.cs` | Firma kullanicilari (JSON) — Hizmet Formu kisi seciminde kullanilir. Yalniz uretici tarafi. | 35 |
| `ForgotPassword.aspx.cs` | Şifremi unuttum (self-servis): kullanıcıya e-posta ile jetonlu sıfırlama bağlantısı gönderilir. Güvenlik: e-postanın kayıtlı olup olmadığını sızdırmayız (her durumda genel mesaj). | 46 |
| `Forum.aspx.cs` | Forum: giris yapmis tum kullanicilara acik. Ilk giriste topluluk kurallari onayi istenir. | 28 |
| `KnowledgeBase.aspx.cs` | Bilgi tabani yonetimi: pano (on yanit sonuclari, yanit bulunamayan sorular), kayitlar, ice aktarma. Destek ekibi ve super admin. Veriler KbAction.ashx'ten. | 18 |
| `Leads.aspx.cs` | Lead Merkezi: aday havuzu, ice aktarma (CSV, Apollo), onay kuyrugu, sablonlar, engelleme, ayarlar. Yalniz super admin. Veriler LeadAction.ashx'ten. | 16 |
| `Leaves.aspx.cs` | Izinlerim: vendor personeli izin talepleri (yeni talep, taleplerim, onay kutusu, kurallar). Veriler LeaveAction.ashx'ten. | 17 |
| `Login.aspx.cs` | Giris (e-posta + sifre, dil secimi); basarili giriste Auth.Set + kalici oturum cerezi | 64 |
| `Logout.aspx.cs` | Cikis: oturum, kalici oturum cerezi ve vekaleten goruntuleme (imp_company) cerezi temizlenir | 18 |
| `MarkRead.aspx.cs` | Bildirimleri okundu isaretler. partner.js buraya POST atar. | 15 |
| `Materials.aspx.cs` | Satış Materyal ve İçerikleri: vendor yayınlar (PDF/sunum/doküman), tüm iş ortakları görür/indirir. | 177 |
| `NewTicket.aspx.cs` | Bayi / Müşteri: yeni destek talebi formu. | 51 |
| `Notif.aspx.cs` | Bildirim listesi (JSON) — bell her sayfada common.js ile bunu ceker. | 62 |
| `NotifPrefs.aspx.cs` | Kullanıcı e-posta bildirim tercihleri (JSON). GET: {version,training} · POST action=save: kaydet | 24 |
| `OrderEdit.aspx.cs` | Siparis duzenle (vendor). Tutar degisirse hakedis ve firma ciro/kademe yeniden hesaplanir. | 119 |
| `Orders.aspx.cs` | Siparisler: bayinin TUM anlasmalari. Vendor icin duzenle/sil/durum guncelle. | 91 |
| `Partner.aspx.cs` | Sifirdan Uzmanliga ilerlemesi: diger gostergelerle ayni yapida (sayi + kucuk halka + siradaki adim) | 417 |
| `PartnerCreate.aspx.cs` | Vendor: yeni bayi (partner firma) olusturma. | 63 |
| `ProjectRecordDetail.aspx.cs` | Proje/Hizmet kaydi detayi (yalniz uretici tarafi). | 35 |
| `ProjectRecords.aspx.cs` | Proje Kayitlari: firma bazli hizmet/proje ekstresi (yalniz uretici tarafi). | 81 |
| `ReceivableDetail.aspx.cs` | Alacak detayi: cari secimi, tahsilat, vadelere bolme, tarih guncelleme, hatirlatma, gecmis | 98 |
| `Receivables.aspx.cs` | Alacak takibi ana ekrani: yalniz Finans yetkisi | 219 |
| `RegisterCustomer.aspx.cs` | Partner: musteri kaydi olusturur (Pending). 60 gun koruma. Vendor onayina gider. | 67 |
| `RenewCustomerReg.aspx.cs` | Partner: kayit yenileme talebi gonderir. Vendor onayina gider. | 45 |
| `ResetPassword.aspx.cs` | Admin: bir kullanıcının şifresini yeniler; opsiyonel olarak kullanıcıya mail gönderir. | 76 |
| `ResetPasswordLink.aspx.cs` | Self-servis şifre sıfırlama: e-postadaki jetonlu bağlantı. GET: form; POST: yeni parolayı kaydet. | 41 |
| `SalesSuggestions.aspx.cs` | Satis Onerileri: Teklif siparislerinden is ortagi satis kaydi onerileri (yalniz Finans yetkisi) | 79 |
| `SaveRating.aspx.cs` | Puanlama: yıldız (1-5) + opsiyonel yorum | 24 |
| `ServiceForm.aspx.cs` | Hizmet Formu: Destek (cozulmus talep) veya Proje (ProjectRecords) kaydi + secili kisilere mail. | 156 |
| `SetLang.aspx.cs` | Dil (kültür) değiştir: oturum + çerez; güvenli şekilde geri yönlendir. | 27 |
| `Staff.aspx.cs` | Fol personeli paneli (Portal ve Finans yetkisi olmayan uretici personeli). Yetkiye gore sekillenir: destek yetkisi: talep gostergeleri, 14 gunluk akis, ekip yuku, bana atanan talepler; editor: icerik onerileri; herkes: A | 253 |
| `SubmitDemo.aspx.cs` | Demo talebi olusturur (tercih + 2 alternatif tarih/saat) ve bildirim e-postasi gonderir. | 88 |
| `SuggestContent.aspx.cs` | Kullanıcı içerik önerisi: kaydeder + destek@ adresine mail atar. | 36 |
| `SupportPlanSave.aspx.cs` | Firma destek plani guncelle (yalniz portal yoneticisi): dakika + gecerlilik tarihi. | 32 |
| `TicketAction.aspx.cs` | Ticket aksiyonlari: reply / note / assign / resolve / reopen | 241 |
| `TicketDetail.aspx.cs` | Ticket detay: konuşma akışı + yanıt/durum aksiyonları. | 253 |
| `TicketPrint.aspx.cs` | Talep yazdırma / PDF: temiz belge düzeni, otomatik yazdırma penceresi (tarayıcıdan "PDF olarak kaydet"). | 44 |
| `Tickets.aspx.cs` | Taleplerim (bayi/müşteri) veya Destek Kuyruğu (üretici tarafı). | 166 |
| `Tiers.aspx.cs` | Partner Seviyeleri sayfasi: tum kademeler, getiri (avantaj) ve gotursu (sart/yukumluluk). | 131 |
| `Tour.aspx.cs` | Uygulama turu durumu: state (JSON: show + role) / seen (POST: görüldü işaretle) | 41 |
| `Training.aspx.cs` | River Akademi (Netflix tarzı): tam genişlik, hero, kategori ağacı, slider satırları. | 1044 |
| `TrainingEdit.aspx.cs` | Eğitim içeriği düzenleme: başlık, açıklama, kategori, öne çıkarma + metni; videolarda bağlantı (YouTube / embed kodu). | 175 |
| `TrainingItemAction.aspx.cs` | Eğitim içerik işlemleri: toggle (yayın) / feature (öne çıkar) / delete | 40 |
| `TrainingManage.aspx.cs` | Eğitim içerik yönetimi (admin / eğitim editörü): düzenle, yayınla/kaldır, öne çıkar, sil. | 79 |
| `TrainingTrack.aspx.cs` | İzleme takibi: action=view (izlenme sayacı) / action=progress (ilerleme + izleme günlüğü) | 47 |
| `Transfer.aspx.cs` | Herkese acik indirme sayfasi: /t/{token}. Giris gerektirmez; kisiye ozel link (+ opsiyonel sifre). | 69 |
| `TransferDetail.aspx.cs` | Paylasim detayi: dosyalar, alicilarin indirme durumu, sure uzatma / iptal / yeniden gonderme. | 100 |
| `TransferNew.aspx.cs` | Yeni dosya paylasimi (yalniz uretici personeli). Yukleme + gonderim istemci tarafinda TransferApi.ashx ile yapilir. | 44 |
| `Transfers.aspx.cs` | Paylasimlarim: uretici personeli kendi paylasimlarini, portal yoneticisi tumunu gorur. | 46 |
| `UpdateOrderStatus.aspx.cs` | Siparis durumu guncelle. YALNIZCA vendor. | 23 |
| `UploadAvatar.aspx.cs` | Profil resmi (avatar): dosya yukleme VEYA hazir avatar (preset) secimi. AJAX: yeni yolu (assets/avatars/...) plain text doner; hata -> "err:..." | 66 |
| `UserCreate.aspx.cs` | Vendor: Bayi / Müşteri kullanıcısı veya Destek Personeli oluşturma. | 154 |
| `UserEdit.aspx.cs` | Portal admin: kullanıcı ad + yetki düzenleme. Vendor personelinde çoklu yetki (checkbox). | 124 |
| `UserTraining.aspx.cs` | Bir kullanicinin egitim gecmisi (yonetici gorunumu): "Egitim Gecmisim" ile ayni hesap + sertifikalar + sinav sonuclari. Portal Yonetimi yetkisi ya da egitim editoru (editor panelindeki "En cok izleyenler" listesinden, em | 217 |
| `Users.aspx.cs` | Admin: tüm kullanıcılar + şifre yenileme. | 131 |
| `Vendor.aspx.cs` | Fol personeli (vendor) genel bakis: bayi matrisi, demo kuyrugu, vekaleten goruntuleme, Dosyalar/API dugmeleri | 323 |
| `Verify.aspx.cs` | Herkese acik sertifika dogrulama (giris gerektirmez). Yalniz tahmin edilemez dogrulama kodu ile sorgular. | 45 |
| `VersionAction.aspx.cs` | Versiyon Duyurusu işlemleri: save (taslak) / publish (yayınla + bildirim + mail) / delete | 104 |
| `Versions.aspx.cs` | Versiyon Duyuruları — herkes (müşteri + iş ortağı + vendor) görür; içerik girişi eğitim editörü/admin. | 186 |

## JSON / dosya uçları (`web/*.ashx`, `web/api/v1`)

| Dosya | Açıklama | Satır |
|---|---|---|
| `BroadcastEmails.ashx` | Toplu bildirim: ek e-posta adreslerini analiz eder (ayirir). Gonderimle ayni kod: EmailListParser.Analyze. | 28 |
| `BroadcastTest.ashx` | Toplu bildirim: e-postayi yalniz oturumdaki yoneticinin adresine test olarak gonderir (POST + ayni site). | 37 |
| `ForumApi.ashx` | Forum islemleri (JSON). Okuma GET, yazma POST + ayni site. Gelistirme asamasinda yalniz Fol personeli (ForumRepo.CanAccess). | 208 |
| `ForumFile.ashx` | Forum gorsel ve videolari: yalniz foruma erisebilenler; gizlenmis gonderinin dosyasi yalniz sahibine ve moderatorlere. Video icin HTTP Range (206) destegi: ileri-geri sarma ve parca parca yukleme. | 51 |
| `IntroMedia.ashx` | Karsilama videosu / kapak gorseli: yalniz giris yapmis kullanicilara. Dosyalar App_Data/intro altinda (IIS bu klasoru dogrudan sunmaz). Range destegi: ileri-geri sarma. IntroMedia.ashx?f=video  -> intro-{rol}.mp4   Intro | 47 |
| `KbAction.ashx` | Bilgi tabani ucu. suggest / solved : talep acan taraf (is ortagi, musteri) - destek talebi oncesi AI on yaniti list / import / toggle / stats : destek ekibi ve super admin Tum istekler POST + ayni site. | 89 |
| `LeadAction.ashx` | Lead Merkezi ucu. YALNIZ super admin. state / contacts (GET); digerleri POST + ayni site. | 216 |
| `LeaveAction.ashx` | Izin talepleri ucu. Yalniz vendor personeli. state (GET), file (GET, yetkili kisiler); digerleri POST + ayni site. Yonetim (Kurallar: profil, onaylayicilar, genel kurallar, tatil, takvim): YALNIZ super admin (butun modul | 240 |
| `PathApi.ashx` | Sifirdan Uzmanliga: durum, kutlama, paylasim tercihi, editor siralamasi | 62 |
| `ReceivableAction.ashx` | Alacak islemleri (yalniz Finans yetkisi). POST, JSON yanit. | 175 |
| `ReceivableExport.ashx` | Alacak listesi -> Excel (CSV, UTF-8 BOM, noktali virgul). Yalniz Finans yetkisi. | 55 |
| `SalesMaterialAction.ashx` | Satis Materyalleri islemleri. Herkes (giris yapmis, materyal sayfasini gorebilen): view (goruntuleme kaydi), send (belgeyi kendi e-postasina; BCC: Materials.SendBcc) Vendor: list, save, move, delete, catadd, catrename, c | 126 |
| `SalesSuggestAction.ashx` | Satis onerileri: book (is ortagina satis kaydi) / dismiss (kanal satisi degil) / restore. Yalniz Finans yetkisi, POST, ayni site. | 33 |
| `TicketFile.ashx` | Destek talebi eki: yetki kontrollu sunum. ?id=EkId  [&inline=1 → onizlenebilir turler sayfa icinde] ticketfiles/ klasorune dogrudan erisim web.config'te kapali; ekler yalniz bu servisten acilir. | 38 |
| `TicketPhone.ashx` | "Numarami sistemden sil": hatirlanan telefon numarasini aninda siler (yalniz POST, ayni site) | 27 |
| `TicketStatusNote.ashx` | Destek talebi gorunen durumu: yalniz Fol personeli (destek ya da portal yetkisi). POST, ayni site. Sistem durumu degismez; talebe sistem satiri eklenir, musteriye uygulama ici bildirim gider. | 61 |
| `TrainingCategory.ashx` | Akademi kategorileri: list (GET), addMain / addSub / rename / move / delete (POST + ayni site). Yalniz editorler. | 129 |
| `TrainingFile.ashx` | Akademi dokumani: yalniz giris yapmis kullanicilara. Goruntulenebilen turler sayfa icinde (inline), digerleri ve dl=1 indirme. Video / ses icin Range (kismi icerik) destegi: ileri geri sarma calisir. | 54 |
| `TrainingWatchers.ashx` | Akademi editor paneli: en cok izleyenler listesi (JSON). Yalniz egitim editoru / yonetici. period: all (varsayilan), 30 (son 30 gun), month (bu takvim ayi, Turkiye saatine gore) | 27 |
| `TransferApi.ashx` | Dosya paylasimi API (yalniz uretici personeli) POST ?action=create    form: title, message, days, maxdl, password, fname[], fsize[], ftype[]  -> {transferId, files:[{id,name,size}]} POST ?action=chunk&t=&f=&o=   govde: h | 201 |
| `TransferDownload.ashx` | Herkese acik indirme: ?k={token}&f={fileId}. Giris gerektirmez; kisiye ozel token + (varsa) sifre dogrulamasi. Range destegi: yarida kalan indirme kaldigi yerden devam eder. Dosya bellege alinmadan diskten akitilir. | 78 |
| `VersionModule.ashx` | Versiyon duyurusu formu: modul listesine yeni modul ekler. Yalniz icerik editoru / yonetici; POST + ayni site. | 31 |
| `Watchlist.ashx` | Izleme listeleri API (giris yapmis tum kullanicilar). Tum degisiklikler sahiplik denetimli. POST action=create(name) / rename(l,name) / delete(l) / toggle(l,m,on) / reorder(l,mids) / share(l,scope,uids,rec) / copy(l) GET | 109 |
| `api/v1/balance.ashx` | GET /api/v1/balance  — anahtarin bagli oldugu firmanin kalan destek suresi + tarihi + gonderebilir mi Auth: X-Api-Key: <anahtar>  (veya Authorization: Bearer <anahtar>) | 49 |
| `api/v1/tickets.ashx` | POST /api/v1/tickets  — harici urunlerden destek talebi alma (Faz 1, tek yon) Auth: Authorization: Bearer <api_key> | 226 |

## Modüller (`web/App_Code/*.cs`)

| Dosya | Açıklama | Satır |
|---|---|---|
| `App_Code/Auth.cs` | Oturum tabanli kimlik. Login basarili olunca Set(...) cagrilir. Korumali sayfalar Page_Load'da Auth.Require(this) veya Auth.RequireVendor(this) cagirir. | 262 |
| `App_Code/BroadcastHtml.cs` | Toplu bildirim e-postasi: hazir tema ya da ozel HTML. Onizleme, test gonderimi ve asil gonderim ayni yoldan gecer. | 70 |
| `App_Code/DateExt.cs` | Depoda UTC (SYSUTCDATETIME) tutulan zamanlari Turkiye saatine cevirir. Sunucunun saat diliminden BAGIMSIZ calisir: her zaman UTC -> Istanbul (UTC+3). | 29 |
| `App_Code/Db.cs` | ADO.NET baglanti yardimcisi. EF/DLL yok; System.Data.SqlClient GAC'ta gelir. | 41 |
| `App_Code/EmailListParser.cs` | Toplu bildirim: serbest metinden e-posta adreslerini cikarir ve ayirir. Virgul / noktali virgul / satir, "Ad Soyad <ad@firma.com>", Excel sutunu ya da duz metin icindeki adresler. Ekrandaki analiz (BroadcastEmails.ashx)  | 103 |
| `App_Code/Emails.cs` | Kurumsal HTML e-posta sablonlari (inline stil; e-posta istemcileri icin table-tabanli). | 659 |
| `App_Code/Exams.cs` | ===================== MODELLER ===================== | 993 |
| `App_Code/Fmt.cs` | Bicimlendirme + guvenli HTML uretimi (C# 5). | 171 |
| `App_Code/Forum.cs` | ============================================================================ Forum (Faz 1): akis, gonderi (metin + 4 gorsel / 1 video), yorum, begeni, kaydetme, Fol duyurusu ve sabitleme, sikayet ve moderasyon, bildiriml | 1033 |
| `App_Code/HomeKit.cs` | Ana sayfa panelleri (bayi yoneticisi / musteri + bayi personeli / Fol personeli) icin ortak parcalar: karsilama profil resmi, "Yeni neler var?" akisi, bos durum kutusu, "Bugun icin" satiri, gosterge karti, zaman ifadesi. | 136 |
| `App_Code/IntroVideo.cs` | Rol bazli karsilama videolari. Dosyalar: App_Data/intro/intro-{rol}.mp4 + poster-{rol}.jpg Rol SUNUCUDA belirlenir (kullanici baska rolun videosunu isteyemez). Dosyasi olmayan rol icin karsilama yok. | 36 |
| `App_Code/JobRuns.cs` | Zamanlanmis isler: gunde bir calisma. Claim -> isi yap -> basarisizsa Release. | 52 |
| `App_Code/Kb.cs` | Bilgi tabani (River Akademi'den cikarilan konu kayitlari) ve destek talebi AI on yaniti. Kurallar: AI yalniz bulunan kayitlara dayanir, bulamazsa "bulunamadi" der; kullaniciyi asla engellemez. | 364 |
| `App_Code/LeadAi.cs` | Onay kuyrugu: taslaga ozel "AI ile kontrol et" ve "AI ile duzenle". Her cagri tek bir taslak ve onun alicisi icin: alici profili (unvan, kategori, persona, firma, sektor, buyukluk, sehir, firma tanimi) ile. Uydurma yok:  | 126 |
| `App_Code/LeadEngine.cs` | Lead Merkezi gonderim ve yanit motoru (63. migration). Graph: uygulama izniyle (Mail.Send, Mail.ReadWrite) YALNIZ fol.solutions kutulari (Exchange erisim ilkesiyle sinirli). Claude: yanit siniflandirma ve kisisellestirme | 478 |
| `App_Code/Leads.cs` | Lead Merkezi (62. migration): aday havuzu, engelleme listesi, CSV ve Apollo ice aktarma, sablondan taslak, onay kuyrugu. Gonderim (Microsoft Graph) ve Claude ile kisisellestirme sonraki asamada bu kuyruga baglanir. | 777 |
| `App_Code/LearningPath.cs` | ============================================================================ "Sifirdan Uzmanliga" ogrenme yolu + rozetler - Adim bitti = %90 izlendi VE hizli gecilmedi (gercek izleme suresi video suresinin yarisindan az  | 478 |
| `App_Code/Leaves.cs` | Izin talepleri (vendor personeli). 61. migration. Kurallar, hesap ve onay yolu burada; tarayicidaki hesap yalniz anlik gorsel geri bildirim. | 645 |
| `App_Code/Mailer.cs` | SMTP ile e-posta gonderir. Ayarlar web.config -> <appSettings> altindadir. Bilgiler girilmemisse gonderim atlanir; islem (demo/kullanici kaydi) yine tamamlanir. | 158 |
| `App_Code/Models.cs` | Basit POCO modeller (C# 5 uyumlu; string interpolation / ?. / expression-body YOK). | 373 |
| `App_Code/Nav.cs` | Tum sayfalarda AYNI ust menuyu uretir. Sayfa yalnizca aktif modul anahtarini verir. Boylece rol basina menu her gecin ayni kalir (drift olmaz). | 112 |
| `App_Code/Notifier.cs` | Yeni eğitim içeriği eklendiğinde bildirim: uygulama içi (her zaman) + e-posta (tercihi açık olanlar). | 39 |
| `App_Code/PartnerHome.cs` | Ana sayfalar (partner + musteri): ortak yardimcilar ve firma geneli hesaplamalar | 133 |
| `App_Code/Receivables.cs` | ============================================================================ Alacak takibi: River Teklif siparisleri -> tahsilat Kaynak: [RiverTeklif].[dbo].[Siparisler] (ayni SQL Server; web.config Receivables.Source) K | 747 |
| `App_Code/Repo.cs` | Tum veri erisimi burada (ADO.NET). Tenant izolasyonu sorgulardaki CompanyId ile saglanir. | 3125 |
| `App_Code/SalesMats.cs` | Satis Materyalleri (genisletilmis): ozet, sira, renk temasi, ikon; kategori sirasi; goruntuleme / gonderim kayitlari; sayfa metni. 59. migration calistirilmamissa Ready() false doner ve sayfa eski duzende calisir. | 240 |
| `App_Code/SalesSuggestions.cs` | ============================================================================ Satis onerileri: RiverTeklif siparislerinden is ortagi satis kaydi (Deals) onerisi. - Kapsam: is ortagi alani dolu YA DA kanal satisi olan sipa | 303 |
| `App_Code/StaffHome.cs` | Fol personeli paneli: destek ekibi akisi, ilk yanit suresi, ekip yuku, icerik onerileri, dosya paylasimlari | 102 |
| `App_Code/TicketFiles.cs` | Destek talebi ekleri: izin verilen turler, guvenli MIME, onizleme turu ve yetki kontrollu okuma. Talep acma (AddTicket), yanit (TicketAction) ve API (api/v1/tickets) AYNI listeyi kullanir. .aspx/.html/.svg/.exe gibi sunu | 76 |
| `App_Code/TicketFmt.cs` | Talep durum/oncelik etiketleri - paylasimli (App_Code). Sayfa sinifina baglilik olmadan her yerden erisilir. | 35 |
| `App_Code/TicketPhone.cs` | Destek talebi formunda "Sonraki taleplerimde hatirla": kullanicinin telefon numarasi. Yalniz kisi isaretleyip talep gonderdiginde saklanir; "Numarami sistemden sil" ile aninda silinir. | 36 |
| `App_Code/TierEngine.cs` | KADEME & MEGA DEAL MOTORU (spec): 0-1.25M Registered 30 / 1.25-2.5M Silver 36 / 2.5-4M Gold 42 / 4M+ Platinum 50 Mega Deal (tek anlasma): 1.5M+ +3 / 2.5M+ +5 / 4M+ +7 / 6M+ +10 Kademe gecisi: bir deal'in taban orani, o d | 90 |
| `App_Code/TrNames.cs` | Yaygin Turkce adlarin cinsiyeti (hitap icin: "Ad Bey" / "Ad Hanim"). Iki cinsiyette de kullanilan adlar belirsiz sayilir. Liste disindaki ya da belirsiz adlarda karar Claude'un tahminine (ve onay kuyrugundaki kontrole) b | 56 |
| `App_Code/TrainingCats.cs` | Akademi kategorileri (editor yonetimi) ve dokuman dosya yardimcilari. | 485 |
| `App_Code/TrainingDetail.cs` | Bir kullanicinin video bazinda ayrintili izleme kaydi (yonetici gorunumu icin). Kaynaklar: TrainingProgress (oran, konum, sure, son izleme), TrainingWatchLog (gercek izleme suresi: oynarken her 15 sn bir kayit), Training | 137 |
| `App_Code/TrainingHistoryView.cs` | Egitim gecmisi gorunumu: kisinin kendi "Egitim Gecmisim" penceresi (self=true) ve yonetici sayfasi (self=false) AYNI hesabi kullanir. | 117 |
| `App_Code/Transfers.cs` | ===================== MODELLER ===================== | 351 |
| `App_Code/VersionModules.cs` | Versiyon duyurulari: modul listesi. Bolum basliginda "## Kategori / Modul" olarak saklanir. | 55 |
| `App_Code/Watchlists.cs` | ===================== MODEL ===================== | 185 |

## İstemci betikleri (`web/js/*.js`)

| Dosya | Açıklama | Satır |
|---|---|---|
| `js/acshelf.js` | Akademi ana ekrani: kaydirilabilir kategori rafi (her alt kategori ayri kart) + kategori sayfasi (Videolar / Dokumanlar). | 208 |
| `js/common.js` |  | 728 |
| `js/demos.js` |  | 42 |
| `js/forum.js` | River Forum (Faz 1): akis, gonderi yazma (gorsel / video / bağlanti), yorum, begeni, kaydetme, sikayet, moderasyon. | 674 |
| `js/i18n.js` |  | 158 |
| `js/kb-admin.js` | Bilgi tabani yonetimi: pano, kayitlar, ice aktarma (KbAction.ashx) | 71 |
| `js/kb-suggest.js` | Destek talebi formu: River AI icin talep gonderilmeden once bilgi tabanindan AI on yaniti. | 94 |
| `js/leads.js` | Lead Merkezi: aday havuzu, aday ekleme (CSV, Apollo), onay kuyrugu, sablonlar, engelleme, ayarlar. Yalniz super admin. | 458 |
| `js/leaves.js` | Izinlerim: yeni talep (anlik sure / donus / onay yolu), taleplerim, onay kutusu, kurallar (yonetici ve IK). | 345 |
| `js/materials.js` |  | 199 |
| `js/orders.js` |  | 58 |
| `js/partner.js` |  | 190 |
| `js/path.js` | "Sifirdan Uzmanliga" ogrenme yolu: menu butonu, rozetler, yol ekrani, kutlama, editor siralamasi. | 302 |
| `js/qrcode.js` | --------------------------------------------------------------------- | 2297 |
| `js/salesmats.js` | Satis Materyalleri: arama, goruntuleme kaydi, "Bana gonder" (belge giris yapan kisinin e-postasina; BCC sunucuda). | 54 |
| `js/smadmin.js` | Satis Materyalleri: Icerik Yonetimi paneli (vendor). Materyaller (duzenle, sirala, kaldir, ozet onerisi), Kategoriler, Sayfa metni. | 218 |
| `js/ticketstatus.js` | Destek talebi gorunen durumu: personel listede ve detayda durum etiketine tiklayip hazir ya da ozel bir durum yazar. | 43 |
| `js/ticketview.js` | Destek talebi ekleri: uygulama icinde onizleme (resim / PDF / metin). Onizlenemeyenler dogrudan indirilir. | 97 |
| `js/tiers.js` |  | 42 |
| `js/training-manage.js` | Egitim Icerik Yonetimi: arama, suzme ve siralama (tarayicida; sayfa yenilenmez). Durum oturum boyunca korunur. | 50 |
| `js/training.js` |  | 941 |
| `js/trcats.js` | Akademi: editor kategori yonetimi (Kategoriler sekmesi) ve formlarda alt kategoriyi ana kategoriye gore suzme. | 242 |
| `js/users.js` |  | 40 |
| `js/vendor.js` |  | 113 |
| `js/watchers.js` | Akademi editor paneli: En Cok Izleyenler. Liste sekme ilk acildiginda yuklenir; satira tiklaninca egitim gecmisi pencerede acilir. | 67 |

## Veritabanı migration'ları (`database/`)

Numaralı dosyalar sırayla ve **bir kez** uygulanır; hepsi tekrar çalıştırılmaya dayanıklı yazılır (`IF OBJECT_ID… IS NULL`, `IF COL_LENGTH… IS NULL`). Uygulananlar `dbo.SchemaMigrations` tablosunda tutulur (`tools/db-migrate.ps1`). `CLEANUP_*`, `RESET_*` ve `*_OPTIONAL` betikleri tek seferlik/veri silen işlerdir; otomatik çalışmaz.

| Dosya | Açıklama | Oluşturduğu tablolar |
|---|---|---|
| `01_schema.sql` | RIVER PARTNER & VENDOR PORTAL — VERİTABANI ŞEMASI (MS SQL Server / T-SQL) | Companies, Deals, DemoRequests, ImpersonationLog, Notifications, Orders, PartnerTiers, QuoteRequests, TrainingMaterials, Users |
| `02_stored_procedures.sql` | RIVER PARTNER PORTAL — İŞ MANTIĞI (Stored Procedures / Fonksiyonlar) |  |
| `03_seed.sql` | RIVER PARTNER PORTAL — BAŞLANGIÇ VERİSİ (Seed) |  |
| `04_alter_demo_alt_dates.sql` | 04 — DemoRequests: alternatif tarih/saat kolonlari (AltDate1, AltDate2) |  |
| `06_customer_registrations.sql` | 06 — Müşteri Kayıt (lead koruma) sistemi | CustomerRegistrations |
| `07_customer_contact.sql` | 07 — Müşteri Kayıt: kontak e-posta ve telefon kolonlari |  |
| `08_training_categories.sql` | 08 — Eğitim Merkezi: özel kategori (grup) ve alt kategori (sekme) |  |
| `09_sales_materials.sql` | 09 — Satış Materyal ve İçerikleri (PDF, sunum, tanıtım dokümanları) | SalesMaterials |
| `10_support_tickets.sql` | 10 — Destek (Ticket) Sistemi + Firma Tipi + Roller | TicketAttachments, TicketMessages, Tickets |
| `11_training_plus.sql` | 11 — Eğitim Merkezi (Netflix tarzı): doküman alanları, izleme ilerlemesi, | ContentSuggestions, TrainingProgress, TrainingRatings, TrainingWatchLog |
| `12_seed_training_docs.sql` | 12 — Eğitim Merkezi başlangıç dokümanları (seed) |  |
| `13_vendor_roles.sql` | 13 — Üretici personeli rolleri: SalesRep (Satış Temsilcisi) + TrainingEditor (Eğitim Editörü) |  |
| `14_training_featured.sql` | 14 — Eğitim: Öne Çıkar (featured) alanları |  |
| `15_tier_rates.sql` | 15 — Partner seviyeleri: baz komisyon güncellemesi |  |
| `16_version_announcements.sql` | 16 — Versiyon Duyuruları | VersionAnnouncements |
| `17_app_tour.sql` | 17 — Uygulama Turu: kullanıcı bazında "görüldü" bayrağı |  |
| `18_user_avatar.sql` | 18 — Kullanıcı profil resmi (avatar) |  |
| `19_multi_permissions.sql` | 19 — Vendor kullanıcılarında çoklu yetki |  |
| `20_notify_prefs.sql` | 20 — Kullanıcı e-posta bildirim tercihleri |  |
| `21_support_api.sql` | 21 — Harici Destek API'si (Faz 1: tek yön, ürünlerden talep alma) | ApiClients, ApiRequestLog |
| `22_ticket_resolution_minutes.sql` | 22 - Talep cozum suresi (dakika). Destek personeli cozdugunde girilir; performans raporunda kullanilir. */ |  |
| `23_company_support_balance.sql` | 23 - Firma destek suresi bakiyesi (dakika). |  |
| `24_notification_link.sql` | 24 - Bildirime tiklaninca gidilecek hedef (ilgili islem sayfasi). BIR KEZ. Idempotent. */ |  |
| `25_password_reset_token.sql` | 25 - Self-servis sifre sifirlama jetonu (kullanici e-postasindaki baglantiyla). BIR KEZ. Idempotent. */ |  |
| `26_company_support_until.sql` | 26 - Firma destek geçerlilik tarihi (SupportUntil). NULL ise tarih kisiti yok; dolu ve gecmisse destek kapali. BIR KEZ. Idempotent. */ |  |
| `27_ticket_seen_reminder.sql` | 27 - Talep inceleme (okundu) + hatirlatma zamani. BIR KEZ. Idempotent. */ |  |
| `28_apiclient_company.sql` | 28 - API anahtarini firmaya bagla + public ClientId. BIR KEZ. Idempotent. */ |  |
| `29_project_service.sql` | 29 - Proje saati havuzu (firma) + Proje/Hizmet kayitlari. BIR KEZ. Idempotent. */ | ProjectRecords |
| `30_exams.sql` | 30 - Akademi sinav + sertifika sistemi. BIR KEZ calistirin. Idempotent. */ | Certificates, ExamAnswers, ExamAttempts, ExamOptions, ExamQuestions, Exams |
| `31_exam_badge_color.sql` | 31 - Sinav sertifika rozet rengi. BIR KEZ calistirin. Idempotent. (30_exams.sql sonrasi) */ |  |
| `32_exam_retake_requests.sql` | 32 - Yeniden sinav talepleri (deneme hakki bitince editor onayi). BIR KEZ calistirin. Idempotent. */ | ExamRetakeRequests |
| `33_file_transfer.sql` | 33 - Dosya Paylasimi (WeTransfer benzeri) + kisiye ozel bildirim. Idempotent: yeniden calistirmak guvenlidir. | TransferDownloads, TransferFiles, TransferRecipients, Transfers |
| `34_watchlists.sql` | 34 - Akademi izleme listeleri (paylasilabilir) + kaldigi yerden devam. Idempotent: yeniden calistirmak guvenlidir. | TrainingProgress, WatchlistItems, WatchlistShares, Watchlists |
| `35_intro_video.sql` | 35 - Karsilama (tanitim) videosu: her kullaniciya yalniz bir kez. Idempotent. */ |  |
| `36_riverai_version_notes.sql` | 36 - River AI surum notlari (v1.0.1 - v1.1.44) ice aktarimi. Idempotent: ayni urun + surum varsa atlanir. |  |
| `37_receivables.sql` | 37 - Alacak takibi (River Teklif siparisleri -> tahsilat). Idempotent. Kisit adlari RCV_ onekli. | ReceivableHistory, ReceivableInstallments, ReceivablePayments, ReceivableReminders, ReceivableSettings, Receivables |
| `38_receivables_channel.sql` | 38 - Alacak takibi: Sorumlu (satis kaynagi). Idempotent. |  |
| `39_learning_path.sql` | 39 - "Sifirdan Uzmanliga" ogrenme yolu + rozetler + basari paylasimi. Idempotent. Kisit adlari LPATH_ onekli. */ | AcademySettings, LearningPathSteps, UserBadges |
| `40_ticket_phone.sql` | 40 - Destek talebi: kullanicinin hatirlanan telefon numarasi (yalniz acik onayla saklanir). Idempotent. */ |  |
| `41_sales_suggestions.sql` | 41 - Satis onerileri: RiverTeklif siparislerinden is ortagi satis kaydi onerisi. Idempotent. Kisitlar OSG_ onekli. */ | OrderSuggestions |
| `42_fix_achievement_notifications.sql` | 42 - Hatali basari bildirimlerinin temizligi. |  |
| `43_receivables_manual.sql` | 43 - Alacak takibi: manuel alacak kaydi. Idempotent. Kisitlar RCV_ onekli. |  |
| `44_ticket_status_note.sql` | 44 - Destek talebi: personelin yazdigi gorunen durum (or. "Guncelleme planlandi"). |  |
| `45_forum.sql` | 45 - Forum (Faz 1): gonderiler, medya, yorumlar, begeniler, kaydedilenler, sikayetler. Idempotent. Kisitlar FRM_ onekli. */ | ForumComments, ForumLikes, ForumMedia, ForumPosts, ForumReports, ForumSaves |
| `46_forum_profile.sql` | 46 - Forum: profil sayfasi (kisinin gonderileri) icin dizin. Idempotent. */ |  |
| `47_forum_moderation.sql` | 47 - Forum moderasyonu: kalici moderasyon kaydi, kullanici kisitlamalari, forum ayarlari (sessize alma). Idempotent. Kisitlar FRM_ onekli. */ | ForumModLog, ForumRestrictions, ForumSettings |
| `48_forum_comment_replies.sql` | 48 - Forum: yorum begenisi ve yoruma yanit (tek seviye). Idempotent. Kisitlar FRM_ onekli. */ | ForumCommentLikes |
| `49_forum_seen.sql` | 49 - Forum: kisinin son gordugu gonderi (menudeki okunmamis sayisi). Idempotent. */ | ForumSeen |
| `50_forum_rules_tags.sql` | 50 - Forum: topluluk kurallari onayi (surumlu) ve etiketler (gundemdekiler, etikete gore suzme). Idempotent. Kisitlar FRM_ onekli. */ | ForumAgreements, ForumPostTags |
| `51_version_modules.sql` | 51 - Versiyon duyurulari: modul listesi (or. River AI icindeki E-Fatura). Idempotent. Kisitlar VMOD_ onekli. | VersionModules |
| `53_job_runs.sql` | 53 - Zamanlanmis isler: gunluk calisma kaydi (ayni gun ikinci kez calismasin). Idempotent. Kisitlar JOB_ onekli. | JobRuns |
| `54_training_categories.sql` | 54 - Akademi kategorileri: editorlerin yonettigi ana ve alt kategoriler (Video ve Dokuman ayri). Idempotent. Kisitlar TCAT_ onekli. | TrainingCategories |
| `55_training_category_styles.sql` | 55 - Akademi kategori gorunumu: kart gorseli, kategori sayfasi kapak gorseli, kart rengi. Idempotent. Kisitlar TCST_ onekli. | TrainingCategoryStyles |
| `56_training_category_tint9.sql` | 56 - Akademi kategori gorunumu: renk temasi sayisi 9 (0..8; 8 = Gunes, sari). Idempotent. */ |  |
| `57_training_category_icon.sql` | 57 - Akademi kategori gorunumu: hazir ikon secimi (assets/caticons.svg icindeki "i-<ad>" sembolu). Idempotent. */ |  |
| `58_training_category_tint15.sql` | 58 - Akademi kategori gorunumu: renk temasi sayisi 15 (0..14; 9 Lagun, 10 Ametist, 11 Mercan, 12 Orman, 13 Sarap, 14 Buz). Idempotent. */ |  |
| `59_sales_materials_plus.sql` | 59 - Satis Materyalleri: ozet, sira, renk temasi ve ikon; kategori sirasi; goruntuleme / gonderim kayitlari; sayfa metni. Idempotent. Kisitlar SMAT_ o | SalesMaterialCategories, SalesMaterialEvents, SiteTexts |
| `60_drop_licenses_OPTIONAL.sql` | 60 - Lisans modulu kaldirildi: tablolarini siler. ISTEGE BAGLI ve GERI ALINAMAZ. |  |
| `61_leave_requests.sql` | 61 - Izin talepleri: profil (unvan, departman, ise giris, onaylayici), talepler, onay adimlari, islem gecmisi, | LeaveEvents, LeaveHolidays, LeaveLog, LeaveProfiles, LeaveRequests, LeaveSettings, LeaveSteps |
| `62_leads.sql` | 62 - Lead Merkezi: firmalar, kisiler, sablonlar, mesaj kuyrugu, yanitlar, engelleme listesi, ayarlar. Idempotent. Kisitlar LD_ onekli. */ | LeadCompanies, LeadContacts, LeadMessages, LeadReplies, LeadSettings, LeadSuppression, LeadTemplates |
| `63_leads_sending.sql` | 63 - Lead Merkezi gonderim ve yanit: yanit ozeti ve metni, portaldan verilen yanitin baglantisi, gonderim ayarlari. Idempotent. */ |  |
| `64_leads_company_description.sql` | 64 - Lead Merkezi: firma tanimi (Instantly ve benzeri listelerden; Claude kisisellestirmesinde kullanilir). Idempotent. */ |  |
| `65_leads_competitor_block.sql` | 65 - Lead Merkezi: rakip firma domainleri engelleme listesine (kendi calisanlari elenir; is ortaklari etkilenmez). Idempotent. */ |  |
| `66_knowledge_base.sql` | 66) BILGI TABANI + DESTEK TALEBI AI ON YANITI | KbAnswers, KbEntries |
| `67_leads_hitap.sql` | 67) LEAD MERKEZI: HITAP (Ad Bey / Ad Hanim) |  |
| `CLEANUP_golive.sql` | CLEANUP_golive.sql — CANLIYA ALMADAN ÖNCE test verisi temizligi |  |
| `CLEANUP_support_tickets.sql` | CLEANUP_support_tickets.sql — Su ana kadar girilmis TUM destek taleplerini siler |  |
| `RESET_intro_all_users.sql` | Karsilama videosunu tum kullanicilara yeniden (bir kez) goster. */ |  |
| `RESET_tour_all_users.sql` | RESET_tour_all_users.sql - Portal turunu tum kullanicilar icin sifirlar. |  |
