# River Asistan Portal: yayindan once kontrol. Hata varsa cikis kodu 1.
#   1) .cs/.ashx/.asax/.js dosyalarinda UTF-8 BOM
#   2) C# 6+ soz dizimi on taramasi ($"...", ?. , nameof) - sunucu C# 5 derler
#   3) aspnet_compiler ile butun sitenin derlenmesi (IIS'in calisma zamaninda yapacagi derlemenin aynisi)
#   4) node varsa JS soz dizimi
param([switch]$NoCompile)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent; $web = Join-Path $root 'web'; $err = 0
function Rel($p) { $p.Substring($root.Length + 1) }

Write-Host '1) UTF-8 BOM kontrolu...' -ForegroundColor Cyan
Get-ChildItem $web -Recurse -File -Include *.cs, *.ashx, *.asax, *.js | Where-Object { $_.FullName -notmatch '\\(node_modules|assets)\\' } | ForEach-Object {
    $fs = [IO.File]::OpenRead($_.FullName); $b = New-Object byte[] 3; $n = $fs.Read($b, 0, 3); $fs.Close()
    if ($n -lt 3 -or $b[0] -ne 0xEF -or $b[1] -ne 0xBB -or $b[2] -ne 0xBF) { Write-Host ("   BOM YOK: " + (Rel $_.FullName)) -ForegroundColor Red; $script:err++ }
}

Write-Host '2) C# 5 uyumluluk on taramasi...' -ForegroundColor Cyan
Get-ChildItem $web -Recurse -File -Include *.cs, *.ashx, *.asax, *.aspx | Where-Object { $_.FullName -notmatch '\\assets\\' } | ForEach-Object {
    $i = 0
    foreach ($line in [IO.File]::ReadAllLines($_.FullName)) {
        $i++; $code = ($line -split '//', 2)[0]
        if ($code -cmatch '(^|[\s(=,+:?])\$"' -or $code -cmatch '\w\?\.\w' -or $code -cmatch '\bnameof\(') { Write-Host ("   C#6+? " + (Rel $_.FullName) + ":" + $i + "  " + $line.Trim()) -ForegroundColor Yellow }
    }
}

if (-not $NoCompile) {
    Write-Host '3) aspnet_compiler (tum site)...' -ForegroundColor Cyan
    $asp = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe'
    $out = Join-Path $env:TEMP 'river-precompile'
    if (Test-Path $out) { Remove-Item $out -Recurse -Force }
    & $asp -v /river-check -p $web $out -f 2>&1 | ForEach-Object { if ($_ -match 'error') { Write-Host "   $_" -ForegroundColor Red } }
    if ($LASTEXITCODE -ne 0) { Write-Host '   Derleme HATALI' -ForegroundColor Red; $err++ } else { Write-Host '   Derleme temiz' -ForegroundColor Green }
    if (Test-Path $out) { Remove-Item $out -Recurse -Force }
}

if (Get-Command node -ErrorAction SilentlyContinue) {
    Write-Host '4) JS soz dizimi...' -ForegroundColor Cyan
    Get-ChildItem (Join-Path $web 'js') -File -Filter *.js | ForEach-Object { node --check $_.FullName 2>&1 | Out-Null; if ($LASTEXITCODE -ne 0) { Write-Host ("   JS HATA: " + (Rel $_.FullName)) -ForegroundColor Red; $script:err++ } }
}
if ($err -gt 0) { Write-Host "KONTROL BASARISIZ ($err sorun)" -ForegroundColor Red; exit 1 }
Write-Host 'KONTROL TEMIZ' -ForegroundColor Green; exit 0
