# KENDI BILGISAYARINDAN canli sunucuda islem (Cloudflare Tunnel + Access uzerinden SSH, "river-deploy").
# Sunucudaki C:\River\portal once GitHub'dan guncellenir (git pull), sonra istenen is calisir.
#   .\tools\remote.ps1 status          sunucudaki surum, son dagitimlar
#   .\tools\remote.ps1 dryrun          ne kopyalanacagini goster (degisiklik yok)
#   .\tools\remote.ps1 deploy          kontrol + yedek + canliya dagitim
#   .\tools\remote.ps1 migrate-list    bekleyen veritabani migration'lari
#   .\tools\remote.ps1 migrate         bekleyenleri uygula (burada onay istenir)
#   .\tools\remote.ps1 backups         son yedekler
#   .\tools\remote.ps1 rollback 20261107-153000   canli siteyi o yedege dondur (onay istenir)
param([Parameter(Mandatory = $true)][ValidateSet('status', 'dryrun', 'deploy', 'migrate-list', 'migrate', 'backups', 'rollback')][string]$Cmd, [string]$Arg = '')
$ErrorActionPreference = 'Stop'
$repo = 'C:\River\portal'
$live = "(Get-ChildItem 'C:\River Platform*\Partner Portal\web' -Directory | Select-Object -First 1).FullName"
# yerelde gonderilmemis commit varsa uyar: sunucu GitHub'dan ceker
$ahead = git -C (Split-Path $PSScriptRoot -Parent) rev-list --count '@{u}..HEAD' 2>$null
if ($Cmd -in 'deploy', 'dryrun', 'migrate', 'migrate-list' -and $ahead -and [int]$ahead -gt 0) { throw "$ahead commit GitHub'a gonderilmemis. Once: git push" }
$pull = "Set-Location $repo; git pull --ff-only; if (`$LASTEXITCODE -ne 0) { throw 'git pull basarisiz' };"
switch ($Cmd) {
    'status'       { $r = "Set-Location $repo; git log -1 --format='%h %s (%cr)'; Get-Content C:\River\portal-deploys.log -Tail 5 -ErrorAction SilentlyContinue" }
    'dryrun'       { $r = "$pull .\tools\deploy.ps1 -DryRun" }
    'deploy'       { $r = "$pull .\tools\deploy.ps1" }
    'migrate-list' { $r = "$pull .\tools\db-migrate.ps1 -List" }
    'migrate'      { ssh river-deploy "$pull .\tools\db-migrate.ps1 -List"; if ((Read-Host 'Bu migration''lar CANLI veritabaninda uygulansin mi? (evet/hayir)') -ne 'evet') { Write-Host 'Iptal.'; return }; $r = "Set-Location $repo; .\tools\db-migrate.ps1 -Yes" }
    'backups'      { $r = "Get-ChildItem C:\River\portal-backups -Directory | Sort-Object Name -Descending | Select-Object -First 10 -ExpandProperty Name" }
    'rollback'     {
        if ($Arg -notmatch '^\d{8}-\d{6}$') { throw 'Yedek adi gerekli (ornek: 20261107-153000). Liste: .\tools\remote.ps1 backups' }
        if ((Read-Host "CANLI site $Arg yedegine dondurulsun mu? (evet/hayir)") -ne 'evet') { Write-Host 'Iptal.'; return }
        $r = "robocopy C:\River\portal-backups\$Arg ($live) /E /R:1 /W:1 /NFL /NDL /NJH; Add-Content C:\River\portal-deploys.log ((Get-Date -Format 'yyyyMMdd-HHmmss') + '  GERI ALMA -> $Arg')"
    }
}
ssh river-deploy $r
exit $LASTEXITCODE
