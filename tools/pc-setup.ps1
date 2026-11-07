# KENDI BILGISAYARINDA bir kez calistirilir (normal kullanici). Uzaktan dagitim icin:
#   1) cloudflared kurar  2) dagitim anahtari uretir (river_deploy)  3) ~/.ssh/config'e "river-deploy" girdisini ekler
#   4) Cloudflare Access hizmet jetonunu kullanici ortam degiskenlerine yazar (depoya ve dosyaya YAZILMAZ)
# Ornek: .\tools\pc-setup.ps1 -ServerUser Administrator -Hostname deploy.folyazilim.com
param([Parameter(Mandatory = $true)][string]$ServerUser, [string]$Hostname = 'deploy.folyazilim.com')
$ErrorActionPreference = 'Stop'
Write-Host '1) cloudflared' -ForegroundColor Cyan
if (-not (Get-Command cloudflared -ErrorAction SilentlyContinue)) { winget install --id Cloudflare.cloudflared -e --accept-source-agreements --accept-package-agreements }
$cf = (Get-Command cloudflared -ErrorAction SilentlyContinue).Source
if (-not $cf) { $cf = Get-ChildItem 'C:\Program Files*\cloudflared\cloudflared.exe' | Select-Object -First 1 -ExpandProperty FullName }
if (-not $cf) { throw 'cloudflared bulunamadi; yeni bir PowerShell penceresinde tekrar deneyin.' }

Write-Host '2) Dagitim anahtari' -ForegroundColor Cyan
$sshDir = Join-Path $env:USERPROFILE '.ssh'; New-Item -ItemType Directory -Force $sshDir | Out-Null
$key = Join-Path $sshDir 'river_deploy'
if (-not (Test-Path $key)) { ssh-keygen -t ed25519 -f $key -N '""' -C "$env:USERNAME-river-deploy" | Out-Null }

Write-Host '3) SSH yapilandirmasi' -ForegroundColor Cyan
$conf = Join-Path $sshDir 'config'
if (-not (Test-Path $conf) -or -not (Select-String -Path $conf -Pattern '^Host river-deploy' -Quiet)) {
    Add-Content $conf @"

Host river-deploy
    HostName $Hostname
    User $ServerUser
    IdentityFile ~/.ssh/river_deploy
    IdentitiesOnly yes
    ProxyCommand "$cf" access ssh --hostname %h
    ServerAliveInterval 30
"@ -Encoding ascii
}

Write-Host '4) Cloudflare Access hizmet jetonu' -ForegroundColor Cyan
if (-not [Environment]::GetEnvironmentVariable('TUNNEL_SERVICE_TOKEN_ID', 'User')) {
    $id = Read-Host 'Service token Client ID'; $sec = Read-Host 'Service token Client Secret'
    [Environment]::SetEnvironmentVariable('TUNNEL_SERVICE_TOKEN_ID', $id, 'User')
    [Environment]::SetEnvironmentVariable('TUNNEL_SERVICE_TOKEN_SECRET', $sec, 'User')
}
Write-Host ''
Write-Host 'Sunucuya eklenecek GENEL anahtar (server-setup.ps1 -PublicKey "..."):' -ForegroundColor Yellow
Get-Content "$key.pub"
Write-Host ''
Write-Host 'Bitti. Yeni bir PowerShell penceresinde test: ssh river-deploy hostname' -ForegroundColor Green
