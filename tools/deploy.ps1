# River Asistan Portal: depodaki web\ klasorunu canli IIS klasorune dagitir.
#   - Once tools\check.ps1 calisir (-SkipCheck ile atlanir); hata varsa dagitim yapilmaz.
#   - Canli klasorun yedegi alinir (kullanici yuklemeleri haric), son 10 yedek tutulur.
#   - KULLANICI VERISI KLASORLERINE ve gizli ayar dosyalarina asla yazilmaz, hicbir sey silinmez.
#   - Canlida olup depoda olmayan dosyalar yalniz listelenir (silme karari kullanicinin).
param([string]$Target = '', [switch]$SkipCheck, [switch]$DryRun)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent; $src = Join-Path $root 'web'
if (-not $Target) { $Target = (Get-ChildItem 'C:\River Platform*\Partner Portal\web' -Directory | Select-Object -First 1).FullName }
if (-not $Target -or -not (Test-Path $Target)) { throw "Canli klasor bulunamadi: $Target" }
# kullanici yuklemeleri ve calisma zamani verisi (asla ustune yazilmaz / silinmez)
$runtime = @('assets\traincovers', 'assets\avatars', 'assets\catcovers', 'ticketfiles', 'forumfiles', 'materials', 'trainingdocs', 'App_Data')
$secretFiles = @('secrets.config', 'connectionStrings.config')

if (-not $SkipCheck) { & (Join-Path $PSScriptRoot 'check.ps1'); if ($LASTEXITCODE -ne 0) { throw 'Kontrol basarisiz; dagitim yapilmadi.' } }

# gizli dosyalar canlida yoksa (ilk gecis) depodaki yerel kopyadan bir kez yerlestir
foreach ($f in $secretFiles) {
    $t = Join-Path $Target $f
    if (-not (Test-Path $t)) {
        $s = Join-Path $src $f
        if (-not (Test-Path $s)) { throw "$f ne canlida ne depoda var. Ornek dosyadan olusturun." }
        if (-not $DryRun) { Copy-Item $s $t }; Write-Host "Ilk gecis: $f canliya yerlestirildi" -ForegroundColor Yellow
    }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'; $bk = "C:\River\portal-backups\$stamp"
$xd = @(); foreach ($r in $runtime) { $xd += (Join-Path $Target $r); $xd += (Join-Path $src $r) }
if (-not $DryRun) {
    Write-Host "Yedek: $bk" -ForegroundColor Cyan
    robocopy $Target $bk /E /XD $xd /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
    Get-ChildItem 'C:\River\portal-backups' -Directory | Sort-Object Name -Descending | Select-Object -Skip 10 | Remove-Item -Recurse -Force
}

Write-Host "Dagitim: $src -> $Target" -ForegroundColor Cyan
$flags = @('/E', '/R:1', '/W:1', '/NDL', '/NJH', '/NJS', '/NP', '/XD') + $xd + @('/XF') + $secretFiles + @('*.example', '*.GERCEK')
if ($DryRun) { $flags += '/L' }
$changed = robocopy $src $Target @flags | Where-Object { $_.Trim() -ne '' }
$changed | ForEach-Object { Write-Host ("   " + $_.Trim()) }
Write-Host ("Kopyalanan dosya: " + @($changed).Count) -ForegroundColor Green

# canlida olup depoda olmayan dosyalar (bilgi icin; silinmez)
$extra = Get-ChildItem $Target -Recurse -File | Where-Object {
    $rel = $_.FullName.Substring($Target.Length + 1)
    -not ($runtime | Where-Object { $rel.StartsWith($_ + '\') }) -and ($secretFiles -notcontains $rel) -and -not (Test-Path (Join-Path $src $rel))
} | ForEach-Object { $_.FullName.Substring($Target.Length + 1) }
if ($extra) { Write-Host 'Canlida olup depoda olmayan dosyalar (silinmedi):' -ForegroundColor Yellow; $extra | ForEach-Object { Write-Host "   $_" } }

$commit = ''; try { $commit = (git -C $root rev-parse --short HEAD) } catch { }
if (-not $DryRun) { Add-Content 'C:\River\portal-deploys.log' ("$stamp  commit=$commit  dosya=" + @($changed).Count) }
Write-Host 'Bitti. Tarayicida Ctrl+F5 ile kontrol edin.' -ForegroundColor Green
