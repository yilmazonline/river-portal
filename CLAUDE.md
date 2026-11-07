# River Asistan Portal: Claude Code çalışma kuralları

Bu depo, Fol Yazılım A.Ş.'nin **River Asistan Portal** uygulamasıdır (canlı: https://portal.folyazilim.com). İş ortakları (bayiler), müşteriler ve Fol personeli için: destek talepleri, River Akademi (eğitim/sınav/sertifika), satış materyalleri, alacak takibi, izinler, forum, dosya paylaşımı, Lead Merkezi (soğuk e-posta), bilgi tabanı ve AI destek ön yanıtı.

**Kullanıcı:** Yılmaz Güney. Her zaman **Türkçe** yaz; sade, net, adım adım. Teknik ayrıntıyı gerektiği kadar ver; ne değiştiğini, nasıl test edeceğini ve canlıya alınıp alınmadığını her işin sonunda kısaca söyle.

Ayrıntılar: `docs/MIMARI.md` (bütün sayfa/modül/tablo envanteri) · `docs/GECMIS.md` (neden böyle yapıldı) · `docs/ACIK-ISLER.md` (bekleyenler) · `docs/UZAKTAN-DAGITIM.md` (bilgisayardan Cloudflare üzerinden canlıya) · `docs/TASIMA.md` (genel kurulum).

## Ortam

| | |
|---|---|
| Sunucu | Windows Server + IIS (RIVERNEXT), önünde Cloudflare (HTTPS) |
| Canlı site klasörü | Sunucuda `C:\River Platformları\Partner Portal\web` (**doğrudan düzenleme yok**, yalnız `tools\deploy.ps1`) |
| Bu depo | Kullanıcının bilgisayarı (`D:\river\portal`). GitHub özel depo `origin/main`; sunucudaki `C:\River\portal` yalnız buradan `git pull` yapar |
| Sunucuya erişim | `ssh river-deploy` (Cloudflare Tunnel + Access). Hazır komutlar: `.\tools\remote.ps1` (bkz. `docs/UZAKTAN-DAGITIM.md`) |
| Gizli ayarlar | Yalnız sunucuda. Bilgisayardaki `web/secrets.config` ve `web/connectionStrings.config` **örnek (boş) değerlerdir**; yalnız yerel derleme kontrolü için |
| Veritabanı | SQL Server `RIVERNEXT\SQLEXPRESS`, veritabanı `PARTNER`; River Teklif siparişleri aynı sunucuda (`Receivables.Source`) |
| Çalışma zamanı | .NET Framework 4.8, ASP.NET WebForms, **C# 5**, ADO.NET; derleme adımı yok (IIS `.aspx`/`.ashx`/`App_Code` dosyalarını kendisi derler) |

## Altın kurallar (ihlal etme)

1. **C# 5.** Kullanma: `$"..."`, `?.`, `??=`, `nameof`, ifade gövdeli üyeler (`=>`), `out var`, tuple, yerel fonksiyon, `using static`, desen eşleştirme. `StringBuilder` için `using System.Text;`. LINQ için `using System.Linq;`.
2. **UTF-8 BOM:** `.cs`, `.ashx`, `.asax` ve `js/*.js` dosyaları BOM'lu UTF-8 olmalı (Türkçe karakterler). Yeni dosya oluştururken de. `tools\check.ps1` denetler.
3. **Sayfa yapısı:** `X.aspx` → `CodeFile="X.aspx.cs" Inherits="XPage"`. JSON uçları `*.ashx` (`IRequiresSessionState`, yalnız POST + aynı site kontrolü). Mevcut bir ucun desenini kopyala (ör. `LeadAction.ashx`, `KbAction.ashx`).
4. **Güvenlik:**
   - SQL **her zaman parametreli** (`AddWithValue`); asla metin birleştirme.
   - Çıktı **her zaman kodlanır**: `Fmt.Enc(...)` (sunucu), `esc(...)` (JS).
   - HTML içerebilen form alanları `Request.Unvalidated.Form["x"]` ile okunur.
   - Her sayfa/uç başında yetki: `Auth.Require(this)` + `Auth.IsVendor`, `Auth.IsSupportAgent`, `Auth.CanEditTraining`, `Auth.CanFinance`, `Auth.IsSuperAdmin` gibi kontroller (`App_Code/Auth.cs`).
   - Dosya yükleme: uzantı beyaz listesi + boyut sınırı + `Guid` dosya adı; silerken `Path.GetFileName`.
5. **Sürüm ve önbellek:** Her teslimde `web/js/common.js` içindeki `APP_VERSION` artırılır (küçük: v2.60.0 → v2.60.1, özellik: v2.61.0). Değişen CSS/JS'nin `?v=` numarası **bütün `.aspx` dosyalarında** artırılır (ör. `css/site.css?v=170` → `171`, `js/leads.js?v=9` → `10`).
6. **Arayüz metinleri:** Türkçe; **uzun/orta tire (— –) yok**, yerine orta nokta (·) ya da yeniden kurulmuş cümle. Kullanıcıya görünen hata mesajları ne yapması gerektiğini söyler.
7. **Veritabanı değişikliği:** Yalnız yeni numaralı migration ile: `database/NN_kisa_ad.sql` (son numara 67; sıradaki 68). Tekrar çalıştırmaya dayanıklı yaz (`IF OBJECT_ID('dbo.X') IS NULL`, `IF COL_LENGTH('dbo.T','C') IS NULL`). Kod, migration uygulanmamışken de çökmemeli (bkz. `Leads.HasGender` deseni: sütun yoksa eski davranış).
8. **Kullanıcı verisine dokunma:** `assets/traincovers`, `assets/avatars`, `assets/catcovers`, `ticketfiles`, `forumfiles`, `materials`, `trainingdocs`, `App_Data` ve `..\transfer_data` canlıda yaşar; depoda yok, dağıtımda atlanır.
9. **Gizli bilgiler:** `web/secrets.config` ve `web/connectionStrings.config` Git'e girmez; içeriklerini sohbete, commit mesajına, belgeye ya da koda yazma. Yeni bir gizli ayar gerekirse `secrets.config.example`'a boş değerle ekle, kullanıcıdan sunucudaki `secrets.config`'e girmesini iste.

## Her değişiklikte akış

1. Gerekli dosyaları oku, değişikliği yap (en küçük değişiklik; mevcut desenleri izle).
2. `.\tools\check.ps1` → temiz olmalı (Windows'ta aspnet_compiler; derleme kontrolü dağıtımda sunucuda da tekrar çalışır). Hata varsa düzelt, tekrar çalıştır.
3. `git add -A; git commit -m "v2.60.1 · kısa açıklama"; git push` (sunucu GitHub'dan çeker; gönderilmemiş commit varsa `remote.ps1` dağıtımı başlatmaz).
4. Migration varsa: ne yapacağını söyle, **onay al**, `.\tools\remote.ps1 migrate`.
5. Canlıya alma: kullanıcı değişikliği istediyse ve kontrol temizse **"Canlıya alayım mı?"** diye sor; onayla `.\tools\remote.ps1 deploy`. `web.config`, `Global.asax`, `App_Code/Auth.cs` ya da migration içeren değişikliklerde neyin etkileneceğini önce açıkla. Sorun çıkarsa: `.\tools\remote.ps1 backups` ve `rollback <ad>` (onaylı).
6. Kullanıcıya özet: ne değişti, nasıl test edilir (hangi sayfa, ne görmeli), canlıda mı.

## Onay almadan YAPMA

- Veritabanında `DELETE`, `UPDATE` (toplu), `DROP`, `TRUNCATE`; `CLEANUP_*`, `RESET_*`, `*_OPTIONAL` betikleri.
- `tools\remote.ps1 deploy | migrate | rollback` (canlıya alma, veritabanı, geri alma); `ssh river-deploy` ile sunucuda serbest komut çalıştırmak (yalnız okuma amaçlı olanlar hariç: `status`, `dryrun`, `migrate-list`, `backups`, log okuma).
- Canlı site klasörüne, IIS ayarlarına, Görev Zamanlayıcı'ya, `C:\RiverCron` betiklerine dokunmak.
- Lead Merkezi'nde **gönderimi açmak**, toplu onaylamak ya da e-posta göndermek; herhangi bir kullanıcıya e-posta tetikleyen işlem.
- `secrets.config` / `connectionStrings.config` değiştirmek; anahtar üretmek ya da yenilemek.

## Bilinen tuzaklar (yaşandı, tekrar etme)

- **Ortak yükleniyor perdesi** (`common.js`) form gönderiminde `capture` aşamasında açılır; sayfa yenilemeden çalışan formlara `data-noload="1"` ver. Kendi doğrulaman `preventDefault` yaparsa perde kendiliğinden kapanır (düzeltildi), yine de test et.
- **Arama kutuları** `type="search"` ve hiçbir `<form>` içinde değil; `common.js` tıklanana kadar salt okunur tutar (e-posta/şifre önerisi çıkmasın). Testte önce tıkla, sonra yaz.
- **Türkçe:** `ToLower(new CultureInfo("tr-TR"))` (İ→i, I→ı). Arama/eşleştirme için katlama: ç→c, ğ→g, ı→i, ö→o, ş→s, ü→u (`Kb.Fold`, `TrNames.Fold`). Sıralama: `Fmt.NaturalCompare` (sayılar sayı olarak: 2 < 10; Türkçe alfabe). JS'te `Intl.Collator('tr', {numeric:true})`.
- **CSS:** `.lv-pill` sınıfında `margin-left:auto` var, flex satırlarında öğeleri sağa iter; gerekirse `margin:0`. `display:flex` verilen bir elemanda `hidden` özniteliği çalışmaz; `.x[hidden]{display:none}` ekle. Global form stili onay kutularını tam genişliğe yayar; onay kutusuna `width:auto` ver.
- **Koyu tema varsayılan:** `:root` koyu değişkenler; `html[data-theme="light"]` açık. Yeni bileşenlerde zemin ve yazı rengini birlikte değişkenden al (`--text`, `--muted`, `--stroke`); tanımsız değişkene dayanma (`var(--card,#fff)` koyu temada beyaz zemin + açık yazı üretti).
- **YouTube:** yalnız 11 karakterlik kimlik saklanır (`Fmt.YouTubeId` + 11 karakter kontrolü); bağlantı, youtu.be, Shorts ve embed kodu kabul edilir.
- **Claude API** (`LeadClaude.Json`, `App_Code/LeadEngine.cs`): anahtar `Anthropic.ApiKey`, model `Anthropic.Model`; `x-api-key` reddedilirse `Authorization: Bearer` ile bir kez yeniden dener. AI çıktısı her zaman "yalnız verilen kaynaklara dayan, yoksa bulunamadı de" kuralıyla istenir; kullanıcıya gösterilmeden önce kodlanır.
- **Kalıcı oturum:** `Auth.TryRestore` her istekte (Global.asax). Oturuma yeni bir alan eklersen `Auth.Set` içine ekle ki geri yüklemede de dolsun.
- **Mail/Graph:** Lead e-postaları yalnız `satis@` / `sales@fol.solutions` kutularından (Exchange erişim ilkesi diğerlerini engeller). Her Lead e-postası BCC `yilmaz.guney@river.com.tr`.
- **PDF üretimi (kapaklar):** harf içi renk geçişi ve `filter: blur` PDF'te artefakt üretir (`tools/kapaklar/README.md`).

## Modül haritası (kısa)

| Modül | Ana dosyalar |
|---|---|
| Kimlik, yetki, menü | `App_Code/Auth.cs`, `Nav.cs`, `Login.aspx`, `Global.asax` |
| Destek talepleri + AI ön yanıt | `NewTicket.aspx`, `AddTicket.aspx.cs`, `Tickets.aspx`, `TicketDetail.aspx`, `TicketAction.aspx`, `App_Code/Kb.cs`, `KbAction.ashx`, `KnowledgeBase.aspx`, `js/kb-suggest.js`, `js/kb-admin.js` |
| River Akademi | `Training.aspx`, `TrainingManage.aspx`, `TrainingEdit.aspx`, `AddTraining*.aspx`, `Exams*.aspx`, `Certificate*.aspx`, `App_Code/TrainingCats.cs`, `LearningPath.cs`, `js/training.js`, `js/training-manage.js` |
| Lead Merkezi | `Leads.aspx`, `LeadAction.ashx`, `App_Code/Leads.cs`, `LeadEngine.cs` (Graph, gönderim, yanıtlar, Claude), `LeadAi.cs`, `TrNames.cs`, `js/leads.js` |
| Satış materyalleri | `Materials.aspx`, `SalesMaterialAction.ashx`, `App_Code/SalesMats.cs`, `js/salesmats.js`, `js/smadmin.js` |
| Alacaklar / satış önerileri | `App_Code/Receivables.cs`, `SalesSuggestions.cs` |
| İzinlerim | `Leaves.aspx`, `LeaveAction.ashx`, `App_Code/Leaves.cs`, `js/leaves.js` |
| Forum | `Forum.aspx`, `ForumApi.ashx`, `App_Code/Forum.cs`, `js/forum.js` |
| Dosya paylaşımı | `App_Code/Transfers.cs`, `/t/{link}` → `Transfer.aspx` |
| Zamanlanmış işler | `Cron.aspx` (`job=` leads / leaves / pending / varsayılan) |
| E-posta | `App_Code/Mailer.cs`, `Emails.cs`, `BroadcastHtml.cs` |

Bütün liste ve migration'lar: `docs/MIMARI.md`.

## Test

- **Derleme:** `.\tools\check.ps1` (aspnet_compiler, IIS'in yapacağı derlemenin aynısı).
- **Tarayıcı:** Değişen sayfayı canlıda (ya da kullanıcıdan) Ctrl+F5 ile kontrol ettir; ne görmesi gerektiğini somut yaz. Kullanıcı istemedikçe gerçek müşterilere e-posta gidecek bir akışı test etme.
- **Hesaplama/eşleştirme mantığı** (arama puanı, hitap, sıralama gibi) için küçük bir PowerShell ya da C# denemesiyle önce örnek girdilerle doğrula.
