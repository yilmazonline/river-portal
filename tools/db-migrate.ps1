# River Asistan Portal: veritabani migration'larini sirayla uygular ve dbo.SchemaMigrations tablosunda kaydini tutar.
#   Ilk kullanim (canlida zaten uygulanmis olanlari isaretle):  .\tools\db-migrate.ps1 -Baseline 65
#   Bekleyenleri gor:                                            .\tools\db-migrate.ps1 -List
#   Bekleyenleri uygula (onay ister):                            .\tools\db-migrate.ps1
#   Tek dosya (tek seferlik / OPTIONAL betikler icin):           .\tools\db-migrate.ps1 -Only CLEANUP_golive.sql
# Yalniz numarali dosyalar (01_*.sql ...) otomatik calisir; CLEANUP_/RESET_ ve *_OPTIONAL betikler yalniz -Only ile.
# Baglanti bilgisi web\connectionStrings.config dosyasindan okunur (sifre baska yerde tutulmaz). sqlcmd gerekir.
param([int]$Baseline = -1, [switch]$List, [string]$Only = '', [switch]$Yes)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent; $dbDir = Join-Path $root 'database'
[xml]$cfg = Get-Content -LiteralPath (Join-Path $root 'web\connectionStrings.config') -Encoding UTF8
$cs = $cfg.connectionStrings.add.connectionString
$b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $cs
$auth = if ($b.IntegratedSecurity) { @('-E') } else { @('-U', $b.UserID, '-P', $b.Password) }
function Sql([string]$q) { & sqlcmd -S $b.DataSource -d $b.InitialCatalog @auth -b -h -1 -W -Q ("SET NOCOUNT ON; " + $q) }
Sql "IF OBJECT_ID('dbo.SchemaMigrations') IS NULL CREATE TABLE dbo.SchemaMigrations (FileName NVARCHAR(200) NOT NULL PRIMARY KEY, AppliedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME());" | Out-Null
$applied = @(Sql "SELECT FileName FROM dbo.SchemaMigrations;" | Where-Object { $_ -and $_.Trim() } | ForEach-Object { $_.Trim() })
$numbered = Get-ChildItem $dbDir -Filter *.sql | Where-Object { $_.Name -match '^(\d+)_' -and $_.Name -notmatch '_OPTIONAL' } |
    Sort-Object @{ e = { [int]($_.Name -split '_')[0] } }, Name

if ($Baseline -ge 0) {
    $n = 0
    foreach ($f in $numbered) { if ([int]($f.Name -split '_')[0] -le $Baseline -and $applied -notcontains $f.Name) { Sql ("INSERT dbo.SchemaMigrations (FileName) VALUES (N'" + $f.Name.Replace("'", "''") + "');") | Out-Null; $n++ } }
    Write-Host "$n dosya uygulanmis olarak isaretlendi (<= $Baseline)." -ForegroundColor Green; return
}
$pending = if ($Only) { @(Get-Item (Join-Path $dbDir $Only)) } else { @($numbered | Where-Object { $applied -notcontains $_.Name }) }
if (-not $pending -or $pending.Count -eq 0) { Write-Host 'Bekleyen migration yok.' -ForegroundColor Green; return }
Write-Host 'Uygulanacak:' -ForegroundColor Cyan; $pending | ForEach-Object { Write-Host "   $($_.Name)" }
if ($List) { return }
if (-not $Yes) { $a = Read-Host 'Uygulansin mi? (evet/hayir)'; if ($a -ne 'evet') { Write-Host 'Iptal edildi.'; return } }
foreach ($f in $pending) {
    Write-Host "-> $($f.Name)" -ForegroundColor Cyan
    & sqlcmd -S $b.DataSource -d $b.InitialCatalog @auth -b -I -f 65001 -i $f.FullName
    if ($LASTEXITCODE -ne 0) { throw "HATA: $($f.Name) uygulanamadi; sonrakiler calistirilmadi." }
    if (-not $Only -or $f.Name -match '^\d+_') { Sql ("IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE FileName = N'" + $f.Name.Replace("'", "''") + "') INSERT dbo.SchemaMigrations (FileName) VALUES (N'" + $f.Name.Replace("'", "''") + "');") | Out-Null }
}
Write-Host 'Tamamlandi.' -ForegroundColor Green
