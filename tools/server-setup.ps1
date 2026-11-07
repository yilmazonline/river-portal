# SUNUCUDA, YONETICI olarak bir kez calistirilir. Uzaktan dagitim icin:
#   1) OpenSSH Server: yalniz 127.0.0.1'den (Cloudflare Tunnel uzerinden) ve yalniz anahtarla giris; varsayilan kabuk PowerShell
#   2) Bilgisayarinin genel anahtarini yetkili anahtarlara ekler
#   3) cloudflared'i Cloudflare panelinden alinan tunel jetonuyla servis olarak kurar
#   4) GitHub icin salt okunur "deploy key" uretir (genel anahtari GitHub > Repo > Settings > Deploy keys'e eklenir)
# Ornek: .\server-setup.ps1 -PublicKey "ssh-ed25519 AAAA... yilmaz-pc" -TunnelToken "eyJh..."
param([string]$PublicKey = '', [string]$TunnelToken = '')
$ErrorActionPreference = 'Stop'
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'PowerShell''i YONETICI olarak acin.' }

Write-Host '1) OpenSSH Server' -ForegroundColor Cyan
$cap = Get-WindowsCapability -Online | Where-Object Name -like 'OpenSSH.Server*'
if ($cap.State -ne 'Installed') { Add-WindowsCapability -Online -Name $cap.Name | Out-Null }
Set-Service sshd -StartupType Automatic; Start-Service sshd
$cfg = Join-Path $env:ProgramData 'ssh\sshd_config'
$lines = Get-Content $cfg
function Set-Opt([string]$k, [string]$v) {
    $script:lines = $script:lines | Where-Object { $_ -notmatch "^\s*#?\s*$k\s" }
    # Match bloklarindan ONCE olmali
    $i = [Array]::FindIndex([string[]]$script:lines, [Predicate[string]] { param($l) $l -match '^\s*Match\s' })
    if ($i -lt 0) { $script:lines += "$k $v" } else { $script:lines = $script:lines[0..($i - 1)] + "$k $v" + $script:lines[$i..($script:lines.Count - 1)] }
}
Set-Opt 'ListenAddress' '127.0.0.1'          # yalniz yerel: disaridan dogrudan erisim yok, yalniz tunel
Set-Opt 'PasswordAuthentication' 'no'        # sifreyle giris kapali
Set-Opt 'PubkeyAuthentication' 'yes'
Set-Content $cfg $lines -Encoding ascii
# Varsayilan OpenSSH guvenlik duvari kuralini kapat (port disariya acilmaz)
Get-NetFirewallRule -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*OpenSSH*' } | Disable-NetFirewallRule
# Uzak komutlar PowerShell'de calissin
New-Item -Path 'HKLM:\SOFTWARE\OpenSSH' -Force | Out-Null
New-ItemProperty -Path 'HKLM:\SOFTWARE\OpenSSH' -Name DefaultShell -Value "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" -PropertyType String -Force | Out-Null

if ($PublicKey) {
    Write-Host '2) Yetkili anahtar' -ForegroundColor Cyan
    $ak = Join-Path $env:ProgramData 'ssh\administrators_authorized_keys'   # yonetici hesaplari icin Windows OpenSSH bu dosyayi kullanir
    if (-not (Test-Path $ak) -or -not (Select-String -Path $ak -SimpleMatch $PublicKey -Quiet)) { Add-Content $ak $PublicKey -Encoding ascii }
    icacls $ak /inheritance:r /grant 'Administrators:F' /grant 'SYSTEM:F' | Out-Null
}
Restart-Service sshd

if ($TunnelToken) {
    Write-Host '3) cloudflared (tunel servisi)' -ForegroundColor Cyan
    if (-not (Get-Command cloudflared -ErrorAction SilentlyContinue)) { winget install --id Cloudflare.cloudflared -e --accept-source-agreements --accept-package-agreements }
    $cf = (Get-Command cloudflared -ErrorAction SilentlyContinue).Source
    if (-not $cf) { $cf = Get-ChildItem 'C:\Program Files*\cloudflared\cloudflared.exe' | Select-Object -First 1 -ExpandProperty FullName }
    & $cf service install $TunnelToken
}

Write-Host '4) GitHub deploy key (salt okunur)' -ForegroundColor Cyan
New-Item -ItemType Directory -Force 'C:\River\.ssh' | Out-Null
$gk = 'C:\River\.ssh\github_deploy'
if (-not (Test-Path $gk)) { ssh-keygen -t ed25519 -f $gk -N '""' -C 'river-portal-server' | Out-Null }
Write-Host 'Bu anahtari GitHub > depo > Settings > Deploy keys > Add (Allow write access KAPALI):' -ForegroundColor Yellow
Get-Content "$gk.pub"
Write-Host ''
Write-Host 'Bitti. Sonraki adim: docs\UZAKTAN-DAGITIM.md > Sunucu > 5' -ForegroundColor Green
