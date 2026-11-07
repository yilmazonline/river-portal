# Uzaktan dağıtım: bilgisayardan Cloudflare üzerinden canlıya

Claude Code kendi bilgisayarında (`D:\river\portal`) çalışır; canlı sunucuya **Cloudflare Tunnel + Cloudflare Access** üzerinden SSH ile bağlanıp dağıtım yapar.

```
Bilgisayar (Claude Code)  ── git push ──►  GitHub (özel depo)
        │                                        ▲ git pull (salt okunur anahtar)
        └─ ssh river-deploy ─► Cloudflare Access ─► Tunnel ─► Sunucu: C:\River\portal → tools\deploy.ps1 → IIS
```

**Güvenlik modeli:**
- **Sunucu:** Hiçbir port internete açılmaz. Sunucudaki `cloudflared` servisi Cloudflare'e dışarı doğru bağlanır. SSH servisi yalnız `127.0.0.1`'i dinler ve şifreyle giriş kapalıdır.
- **Cloudflare Access:** Tünele yalnız senin **hizmet jetonun** (service token) ile girilir.
- **SSH girişi:** Yalnız bilgisayarındaki anahtar dosyasıyla yapılır.
- **Gizli ayarlar:** `secrets.config` ve `connectionStrings.config` **yalnız sunucuda** durur; GitHub'da ve bilgisayarında gerçek değerler bulunmaz.

> Cloudflare panelindeki menü adları zamanla değişebilir; aşağıdaki yollar Kasım 2026 itibarıyla. Bulamazsan Cloudflare'in "Tunnel" ve "Access" belgelerindeki güncel adları izle.

## A. GitHub (bir kez)

1. GitHub'da **özel (private)** bir depo aç: `river-portal`.
2. Bilgisayarında depoyu yerleştir ve gönder:
   ```powershell
   $zip = Get-ChildItem "$env:USERPROFILE\Downloads\river-portal-claude-code*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
   New-Item -ItemType Directory -Force D:\river | Out-Null
   Expand-Archive $zip.FullName -DestinationPath D:\river -Force          # D:\river\portal
   Rename-Item D:\river\portal\web\secrets.config secrets.config.GERCEK          # sunucuya taşınacak
   Rename-Item D:\river\portal\web\connectionStrings.config connectionStrings.config.GERCEK
   cd D:\river\portal
   git init; git add -A; git status --short | Select-String "\.GERCEK|secrets.config$|connectionStrings.config$"   # BOŞ olmalı
   git commit -m "v2.60.0 · Claude Code'a taşındı"; git branch -M main
   git remote add origin https://github.com/<KULLANICI>/river-portal.git; git push -u origin main
   ```
   `.GERCEK` uzantılı iki dosya `.gitignore` sayesinde depoya **girmez**; `git status` satırı bunu doğrular (boş çıkmalı). Bir sonraki adımda sunucuya taşıyıp bilgisayardan sileceğiz.

## B. Cloudflare (bir kez, https://one.dash.cloudflare.com)

1. **Tünel:** Networks › **Tunnels** › Create a tunnel › **Cloudflared** › ad: `river-portal`. "Install and run a connector" ekranındaki komutta `service install` sonrasındaki uzun **jetonu** (`eyJ…`) kopyala; sunucu kurulumunda kullanacağız.
2. **Genel adres:** Aynı tünelde **Public Hostname** ekle:
   - Subdomain: `deploy`, Domain: `folyazilim.com`
   - Service: **SSH**, URL: `localhost:22`

   Cloudflare `deploy.folyazilim.com` DNS kaydını kendisi oluşturur.
3. **Hizmet jetonu:** Access › Service Auth › **Service Tokens** › Create: ad `river-claude-code`. **Client ID** ve **Client Secret**'ı kopyala; Secret yalnız bir kez gösterilir.
4. **Uygulama:** Access › **Applications** › Add › **Self-hosted**:
   - Ad: `River Portal Deploy`, Domain: `deploy.folyazilim.com`.
   - Policy: Action **Service Auth**, Include › **Service Token** = `river-claude-code`.
   - İstersen ikinci bir policy: Action **Allow**, Include › Emails = `yilmaz.guney@river.com.tr` (tarayıcıdan e-posta koduyla giriş için yedek yol).

## C. Sunucu (bir kez, RDP ile, PowerShell YÖNETİCİ)

1. Bilgisayarında genel anahtarı üret: `cd D:\river\portal; .\tools\pc-setup.ps1 -ServerUser <sunucudaki-yönetici-kullanıcı>`. Sonunda gösterilen `ssh-ed25519 AAAA…` satırını kopyala; Cloudflare jetonunun Client ID/Secret'ı da bu sırada sorulur.
2. `tools\server-setup.ps1` dosyasını sunucuya kopyala (ör. `C:\River\server-setup.ps1`) ve çalıştır:
   ```powershell
   Set-ExecutionPolicy -Scope Process Bypass -Force
   C:\River\server-setup.ps1 -PublicKey "ssh-ed25519 AAAA... " -TunnelToken "eyJ..."
   ```
3. Sonunda gösterilen **GitHub deploy key**'i GitHub › `river-portal` › Settings › **Deploy keys** › Add key'e ekle (**Allow write access kapalı**).
4. Depoyu sunucuya klonla:
   ```powershell
   git -c core.sshCommand="ssh -i C:/River/.ssh/github_deploy -o StrictHostKeyChecking=accept-new" clone git@github.com:<KULLANICI>/river-portal.git C:\River\portal
   git -C C:\River\portal config core.sshCommand "ssh -i C:/River/.ssh/github_deploy"
   ```
5. **Gizli dosyaları yerleştir:** Bilgisayarındaki `D:\river\portal\web\*.GERCEK` dosyalarını RDP ile sunucuya kopyala ve adlarını düzelt:
   - `C:\River\portal\web\secrets.config`
   - `C:\River\portal\web\connectionStrings.config`

   Sonra **bilgisayarındaki `.GERCEK` dosyalarını sil** ve yerel derleme kontrolü için örnek dosyaları kopyala:
   ```powershell
   cd D:\river\portal\web
   Remove-Item *.GERCEK
   Copy-Item secrets.config.example secrets.config; Copy-Item connectionStrings.config.example connectionStrings.config
   ```

## D. Deneme

Bilgisayarında **yeni** bir PowerShell penceresinde (ortam değişkenleri yüklensin):
```powershell
ssh river-deploy hostname                       # sunucunun adı (RIVERNEXT) gelmeli; ilk seferde "yes"
cd D:\river\portal
.\tools\remote.ps1 status
.\tools\remote.ps1 dryrun                       # ne kopyalanacak (değişiklik yok)
.\tools\remote.ps1 deploy                       # ilk dağıtım: web.config yeni yapıya geçer
ssh river-deploy "Set-Location C:\River\portal; .\tools\db-migrate.ps1 -Baseline 65"   # bir kez
.\tools\remote.ps1 migrate                      # 66, 67 (onay ister)
```
İlk dağıtımdan sonra portalı aç (Ctrl+F5), giriş yap, Lead Merkezi › Ayarlar › "Graph bağlantısını test et".

## Günlük kullanım

Claude Code'a isteğini yaz. Claude Code şu sırayla ilerler:
1. Değişikliği yapar ve kontrol eder (`check.ps1`).
2. Commit edip GitHub'a gönderir (`git push`).
3. **Senin onayınla** `.\tools\remote.ps1 deploy` çalıştırır.

Veritabanı değişikliği varsa `.\tools\remote.ps1 migrate` ayrıca onay ister.

| Komut | Ne yapar |
|---|---|
| `.\tools\remote.ps1 status` | Sunucudaki sürüm ve son dağıtımlar |
| `.\tools\remote.ps1 dryrun` | Ne kopyalanacağını gösterir |
| `.\tools\remote.ps1 deploy` | git pull + kontrol + yedek + canlıya kopyalama |
| `.\tools\remote.ps1 migrate-list` / `migrate` | Bekleyen migration'lar / uygula (onaylı) |
| `.\tools\remote.ps1 backups` / `rollback <ad>` | Yedekler / canlıyı bir yedeğe döndür (onaylı) |

## Sorun giderme

- **`ssh river-deploy` tarayıcı açıp giriş istiyor ya da reddediliyor:**
  - **Jeton okunmuyor olabilir:** Hizmet jetonu ortam değişkenleri yeni pencerede yüklenir; PowerShell'i kapatıp yeniden aç. Hâlâ olmuyorsa `%USERPROFILE%\.ssh\config` içindeki ProxyCommand satırına jetonu doğrudan ekle: `--service-token-id <ID> --service-token-secret <SECRET>`. Bu dosya deponun dışında.
  - **Access policy'yi kontrol et:** Action **Service Auth** olmalı ve jeton eklenmiş olmalı.
- **"Permission denied (publickey)":** Sunucuda `C:\ProgramData\ssh\administrators_authorized_keys` dosyasında genel anahtarın olduğunu ve SSH kullanıcısının **yönetici** olduğunu kontrol et; `Restart-Service sshd`.
- **`git pull` başarısız:** Sunucuda `git -C C:\River\portal pull`'u elle dene; deploy key GitHub'da ekli mi?
- **Mac'ten:** `brew install cloudflared`; aynı `~/.ssh/config` girdisi ProxyCommand'de `cloudflared` yoluyla çalışır. `remote.ps1` yerine eşdeğer `ssh river-deploy "..."` komutları kullanılabilir (derleme kontrolü sunucuda yapılır).

## Cloudflare'siz alternatif (bilgi için)

GitHub Actions **self-hosted runner** sunucuya kurulursa sunucu GitHub'a kendisi bağlanır, gelen bağlantıya hiç gerek kalmaz: `main`'e her gönderimde sunucuda `deploy.ps1` çalışır. Daha az parça var ama dağıtım her gönderimde otomatik olur (onay adımı GitHub ortam korumasıyla kurulmalı). Cloudflare kurgusu, onayın senin elinde ve isteğe bağlı olduğu için tercih edildi.
