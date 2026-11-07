# Claude Code'a taşıma rehberi

> **Önerilen kurgu (Kasım 2026): Claude Code kendi bilgisayarında, canlıya Cloudflare üzerinden dağıtım** → adım adım: [`UZAKTAN-DAGITIM.md`](UZAKTAN-DAGITIM.md).
> Bu belgenin geri kalanı ortak kavramları (depo/canlı ayrımı, betikler, geri alma) ve Claude Code'u doğrudan sunucuda çalıştırma alternatifini anlatır.

Amaç: Netliyo'da olduğu gibi **dosya indirme, ZIP açma, kopyalama ve SSMS'te betik çalıştırma adımlarını kaldırmak.** Claude Code doğrudan proje klasöründe çalışır; değişikliği yapar, kontrol eder, veritabanını günceller ve canlıya dağıtır. Sen yalnız ne istediğini yazar ve kritik adımları onaylarsın.

## Hedef yapı (sunucu: RIVERNEXT)

```
C:\River\portal\                      ← Git deposu. Claude Code burada çalışır.
    CLAUDE.md                         ← Claude Code'un her oturumda okuduğu kurallar
    docs\  database\  tools\  web\
    web\secrets.config                ← gizli (Git'e girmez)
    web\connectionStrings.config      ← gizli (Git'e girmez)
C:\River Platformları\Partner Portal\web\   ← CANLI site (IIS). Elle ya da Claude Code tarafından DOĞRUDAN düzenlenmez;
                                              yalnız tools\deploy.ps1 ile güncellenir.
C:\River\portal-backups\              ← her dağıtımdan önce canlı sitenin yedeği (son 10)
C:\River\portal-deploys.log           ← dağıtım günlüğü (tarih, commit, dosya sayısı)
C:\RiverCron\                         ← zamanlanmış görev betikleri (lead-cron.ps1, akademi-altyazi.ps1)
```

Akış: **sen yazarsın → Claude Code `C:\River\portal` içinde değiştirir → `tools\check.ps1` (derleme + kurallar) → gerekiyorsa `tools\db-migrate.ps1` (onayınla) → `tools\deploy.ps1` (yedek alır, canlıya kopyalar) → Git commit.**

Neden depo ile canlı klasör ayrı: canlı klasörde kullanıcıların yüklediği dosyalar (kapaklar, avatarlar, destek ekleri, eğitim dokümanları) ve gizli ayarlar var. Claude Code canlıda doğrudan çalışırsa yanlış bir toplu işlem bunları bozabilir; deploy betiği bu klasörlere hiç dokunmaz ve her seferinde yedek alır.

## Kurulum (bir kez, sunucuda, yönetici olarak)

### 1. Araçlar
1. **Git for Windows:** https://git-scm.com/download/win (varsayılan seçenekler).
2. **Node.js LTS:** https://nodejs.org (JS söz dizimi kontrolü için; Claude Code'un terminal sürümü için de gerekli).
3. **Claude Code:** Netliyo'da kullandığın masaüstü uygulaması sunucuda çalışıyorsa onu kur; çalışmıyorsa terminal sürümünü kur. Güncel kurulum adımları: https://docs.claude.com (Claude Code › Kurulum).
4. `sqlcmd` SQL Server ile birlikte gelir; PowerShell'de `sqlcmd -?` ile kontrol et.

### 2. Depoyu yerleştir
İndirdiğin `river-portal-claude-code.zip` dosyasını aç:
```powershell
$zip = Get-ChildItem "$env:USERPROFILE\Downloads\river-portal-claude-code*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
New-Item -ItemType Directory -Force C:\River | Out-Null
Expand-Archive $zip.FullName -DestinationPath C:\River -Force      # C:\River\portal oluşur
cd C:\River\portal
git init; git add -A
git status --short | Select-String "secrets.config|connectionStrings.config"   # BOŞ olmalı (gizli dosyalar depoya girmiyor)
git commit -m "v2.60.0 · Claude Code'a taşındı"
```
İstersen GitHub'da **özel (private)** bir depo açıp `git remote add origin …` + `git push -u origin main` ile yedekle. Gizli dosyalar `.gitignore` sayesinde gitmez.

### 3. Canlıyla karşılaştır (hiçbir şey değiştirmeden)
```powershell
cd C:\River\portal
.\tools\deploy.ps1 -DryRun
```
Kopyalanacak dosyaların listesi ve "canlıda olup depoda olmayan dosyalar" görünür. Canlıda v2.60.0 kuruluysa liste yalnız yeni yapıyla değişen birkaç dosyadan (web.config ve iki gizli dosya) oluşmalı.

### 4. İlk gerçek dağıtım (web.config yeni yapıya geçer)
```powershell
.\tools\deploy.ps1
```
- Kontrol (derleme) çalışır, canlı sitenin yedeği alınır.
- `secrets.config` ve `connectionStrings.config` canlıya **ilk kez** yerleştirilir; `web.config` artık gizli değer içermez, bu dosyalara referans verir.
- Tarayıcıda portalı aç (Ctrl+F5), giriş yap, Lead Merkezi › Ayarlar › "Graph bağlantısını test et" ile ayarların okunduğunu doğrula.

### 5. Veritabanı migration takibi
```powershell
.\tools\db-migrate.ps1 -Baseline 65      # 01–65 uygulanmış olarak işaretlenir
.\tools\db-migrate.ps1 -List             # bekleyenler: 66_knowledge_base.sql, 67_leads_hitap.sql
.\tools\db-migrate.ps1                   # "evet" ile uygula (ikisi de tekrar çalıştırmaya dayanıklı)
```

### 6. Claude Code'u başlat
`C:\River\portal` klasöründe Claude Code'u aç. İlk mesaj olarak şunu yazabilirsin:
> CLAUDE.md, docs/GECMIS.md ve docs/ACIK-ISLER.md dosyalarını oku. Projenin durumunu ve açık işleri bana kısaca özetle.

Bundan sonra istekleri doğrudan yaz: "Talep listesine firma filtresi ekle", "Lead Merkezi'nde günlük sınırı 8 yap" gibi. Claude Code değişikliği yapar, kontrol eder, ne yaptığını anlatır ve canlıya almak için onay ister.

## Günlük çalışma

| Ne | Kim / nasıl |
|---|---|
| İstek | Sen, Türkçe, Claude Code'a |
| Kod değişikliği | Claude Code, `C:\River\portal\web` içinde |
| Kontrol | `.\tools\check.ps1` (derleme, UTF-8 BOM, C# 5 uyumu, JS) |
| Veritabanı | Yeni migration dosyası + `.\tools\db-migrate.ps1` (**her zaman senin onayınla**) |
| Canlıya alma | `.\tools\deploy.ps1` (yedek + kopyalama; senin onayınla) |
| Geçmiş | `git commit` (mesaj: sürüm + kısa açıklama) |

**Geri alma** (bir dağıtım sorun çıkarırsa):
```powershell
Get-ChildItem C:\River\portal-backups | Sort-Object Name -Descending | Select-Object -First 3   # son yedekler
robocopy "C:\River\portal-backups\<YEDEK>" ((Get-ChildItem 'C:\River Platform*\Partner Portal\web').FullName) /E
```
Kod tarafında: `git log --oneline` + `git revert <commit>` ardından yeniden deploy.

## Kendi bilgisayarından çalışma (önerilen)

Claude Code `D:\river\portal` içinde çalışır; GitHub'a gönderir ve `.\tools\remote.ps1 deploy` ile Cloudflare Tunnel üzerinden sunucuda dağıtımı başlatır. Sunucuda yalnız salt okunur bir depo kopyası (`C:\River\portal`, `git pull`) ve gizli ayarlar bulunur. Kurulum: [`UZAKTAN-DAGITIM.md`](UZAKTAN-DAGITIM.md).

## Güvenlik notları

- Claude Code canlı sunucuda çalışacağı için ilk haftalarda **komutları tek tek onaylayarak** kullan; güven oluştukça dosya düzenleme izinlerini genişletebilirsin.
- `CLAUDE.md`'deki kurallar Claude Code'a veritabanında silme, toplu güncelleme, gizli dosyaları değiştirme ve canlı klasörü doğrudan düzenleme için **her zaman onay almasını** söyler.
- Gizli dosyaları yalnız sunucuda tut; sohbetlere ya da e-postalara yapıştırma. Sohbet geçmişinde kalmış anahtarları yenile (bkz. `docs/ACIK-ISLER.md`).
