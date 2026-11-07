# Ürün ve doküman kapağı üreticisi

17 River AI ürününün "Ürün Özellikleri" kapaklarını üretir: A4 PDF (dokümanın başına eklenir) ve 16:9 WebP/PNG (Akademi doküman kapağı).
Tasarım: koyu üst bölüm, çapraz kesimli beyaz bant, ürün başına renk geçişi ve Lucide ikonu. Yazı tipleri Sora + Inter (Türkçe karakter destekli, PDF'e gömülür).

## Kurulum (bir kez, bu klasörde)
```powershell
npm init -y; npm i lucide-static@0.460.0 @fontsource/sora@5.1.0 @fontsource/inter@5.1.0
pip install playwright pillow; python -m playwright install chromium
```

## Ürün listesi
`products.json`: `[dosya adı, başlık, açılım, ikon, renk1, renk2]`. İkon adları: https://lucide.dev/icons (ör. `wrench`, `factory`).
Yeni ürün eklemek ya da açılım düzeltmek için bu dosyayı düzenleyin; 16:9 listesi (`urun16.json`) bundan türetilir.

## Üretim
```powershell
python gen.py; python render.py                      # A4: pdf\ ve png\ klasörleri
$env:OUT="urun16"; python gen16u.py urun16.json; python render16u.py   # 16:9: urun16\png\ (WebP + PNG)
```
Tek ürün: `python gen.py Bakım` (başlığa göre).

## Bilinen tuzaklar
- PDF çıktısında harflerin içine renk geçişi (`background-clip:text`) çizgi/çerçeve artefaktı üretir; düz renk kullanın.
- `filter: blur()` ve `backdrop-filter` PDF'te köşeli lekeler bırakır; yumuşak geçişler `radial-gradient` ile yapılır.
- Açıklamalar teyitsiz: **Banka → "Banka Yönetimi"** ve **Sandık → "Sandık Yönetimi"** henüz teyit edilmedi.
