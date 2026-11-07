# River Asistan Portal

Fol Yazılım A.Ş. · https://portal.folyazilim.com · ASP.NET WebForms (.NET Framework 4.8, C# 5) + SQL Server, IIS üzerinde (derleme adımı yok).

- **Claude Code ile çalışma kuralları:** [`CLAUDE.md`](CLAUDE.md)
- **Bilgisayardan Cloudflare üzerinden canlıya dağıtım:** [`docs/UZAKTAN-DAGITIM.md`](docs/UZAKTAN-DAGITIM.md)
- **Kurulum / taşıma:** [`docs/TASIMA.md`](docs/TASIMA.md)
- **Mimari ve envanter:** [`docs/MIMARI.md`](docs/MIMARI.md)
- **Geçmiş ve kararlar:** [`docs/GECMIS.md`](docs/GECMIS.md)
- **Açık işler:** [`docs/ACIK-ISLER.md`](docs/ACIK-ISLER.md)
- **Destek API'si:** [`docs/SUPPORT_API.md`](docs/SUPPORT_API.md)

Komutlar (depo kökünde, PowerShell):
```powershell
.\tools\check.ps1               # derleme + kurallar (yerel)
.\tools\remote.ps1 deploy       # git pull + kontrol + yedek + canlıya dağıtım (sunucuda, Cloudflare üzerinden)
.\tools\remote.ps1 migrate      # bekleyen migration'lar (onaylı)
.\tools\remote.ps1 status       # sunucudaki sürüm ve son dağıtımlar
```
Gizli ayarlar (`web/secrets.config`, `web/connectionStrings.config`) depoda değildir; örnekleri `*.example` dosyalarındadır.
