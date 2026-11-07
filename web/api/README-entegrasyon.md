# River Destek API — Entegrasyon Kılavuzu (Faz 1)

Bu servis, River ürünlerinden (Bütçe platformu vb.) gelen destek taleplerini
**River Asistan Portal** destek merkezine aktarır. Faz 1 **tek yönlüdür**:
ürün talebi portala gönderir; portal talebi kendi destek kuyruğuna düşürür,
destek ekibine e-posta ve uygulama içi bildirim gider. (Yanıtların ürüne geri
dönüşü Faz 2'de eklenecektir.)

---

>  **Bu dokümanda ürüne özel API anahtarı YER ALMAZ.** Anahtar her ürün için
>  ayrı üretilir ve size **ayrı/güvenli bir kanaldan** iletilir (bu MD herkese
>  aynı gider). Ayrıca şu an **ayrı bir test/sandbox ortamı yoktur**: denemeler
>  canlı ortama gerçek talep olarak düşer. Test ederken `externalRef` değerini
>  `TEST-...` ile başlatın ve konuya "TEST" yazın; destek ekibiyle önceden haberleşin.

## 1. Uç Nokta (Endpoint)

```
POST https://portal.folyazilim.com/api/v1/tickets.ashx
Content-Type: application/json; charset=utf-8
X-Api-Key: <API_ANAHTARINIZ>
```

- Yalnız **HTTPS** ve **POST**.
- Kimlik: **`X-Api-Key: <anahtar>`** başlığı (önerilen). `Authorization: Bearer <anahtar>` da kabul edilir; ancak bazı sunucularda `Authorization` başlığı kurumsal kimlik doğrulamaya takılabildiği için **`X-Api-Key` tercih edilmelidir.**
- Her ürünün **kendi API anahtarı** olur. Anahtar, portal yöneticisi tarafından
  **Portal > API** ekranından üretilir ve size bir kez gösterilir (portalda
  yalnız özeti/hash'i saklanır; kaybederseniz yenisi üretilir).

---

## 2. Destek Bakiyesi ve Firma Bağlama (ÖNCE BUNU OKUYUN)

> Bu yapı entegrasyonun temelidir. Talep göndermeden önce firmanın destek bakiyesini bilmeniz gerekir.

Her API anahtarı **bir firmaya** bağlanabilir (portal > API > anahtar üretirken firma seçilir). Bu durumda:
- Talep o firmanın altında açılır ve firmanın **destek bakiyesinden** (dakika) düşer.
- Bakiye bittiyse veya anlaşma tarihi geçtiyse istek **reddedilir**:
  - `403 {"error":"support_exhausted"}` — dakika kalmadı
  - `403 {"error":"support_expired"}` — anlaşma tarihi geçti
- Başarılı yanıt kalan bakiyeyi de içerir:
  `201 {..., "support":{"minutesLeft":420,"until":"2026-12-31"}}`

**Kalan bakiyeyi sorgulama:**
```
GET https://portal.folyazilim.com/api/v1/balance.ashx
X-Api-Key: <anahtar>
-> {"product":"Butce","clientId":"cid_...","companyBound":true,"minutesLeft":420,"until":"2026-12-31","canSubmit":true}
```
Firmaya bağlı olmayan (eski) anahtarlar bakiyesiz çalışır (`companyBound:false`).

---

## 3. İstek Gövdesi (JSON)

| Alan | Zorunlu | Açıklama |
|------|---------|----------|
| `subject` | Evet | Talep başlığı/konusu |
| `description` | Evet | Talep detayı (serbest metin) |
| `requester.company` | Evet | Talebi açan son kullanıcının **firma adı** |
| `requester.name` | Evet | Son kullanıcının **adı** |
| `requester.email` | Evet | Son kullanıcının **e-posta** adresi |
| `requester.phone` | Hayır | Telefon |
| `product` | Hayır | Ürün/modül adı (bilgi amaçlı; boşsa anahtardan alınır) |
| `priority` | Hayır | `low` / `normal` / `high` (varsayılan `normal`) |
| `externalRef` | Önerilir | Ürünün kendi talep numarası — **mükerrer engelleme** için |
| `attachments` | Hayır | Ek dosya listesi (bkz. bölüm 4) |

>  **Alan uzunlukları (aşan kısımlar kırpılabilir):** `subject` ≤ 200,
>  `product`/`priority` kısa kod, `requester.company/name/email` ≤ 200 karakter.
>  `description` uzun olabilir. Türkçe karakterler için gövde **UTF-8** olmalı.

>  **Sunucu-sunucu kullanım:** Bu API tarayıcıdan (JavaScript/fetch) çağrılmak
>  için tasarlanmamıştır (CORS açık değildir ve anahtar tarayıcıya konmamalıdır).
>  Çağrıyı ürünün **sunucu tarafından** yapın.

`requester` bilgilerini **düz alan** olarak da gönderebilirsiniz
(`requester_company`, `requester_name`, `requester_email`), ama iç içe obje önerilir.

### Örnek gövde
```json
{
  "product": "Butce",
  "externalRef": "BUTCE-2026-0142",
  "subject": "Rapor ekranında toplam hatası",
  "description": "Aylık rapor ekranında genel toplam yanlış hesaplanıyor.",
  "priority": "high",
  "requester": {
    "company": "Anadolu Yazılım A.Ş.",
    "name": "Ali Veli",
    "email": "ali.veli@anadolu.com",
    "phone": "+90 555 000 00 00"
  },
  "attachments": [
    { "filename": "ekran.png", "contentType": "image/png", "contentBase64": "iVBORw0KGgo..." }
  ]
}
```

---

## 4. Yanıtlar

**201 Created** — talep oluşturuldu:
```json
{ "ticketId": 812, "ticketNo": "DST-2026-000812", "status": "New", "duplicate": false }
```

**200 OK** — aynı `externalRef` daha önce gelmiş (mükerrer; yeni talep açılmaz):
```json
{ "ticketId": 812, "ticketNo": "DST-2026-000812", "status": "New", "duplicate": true }
```

**Hatalar** (`{ "error": "...", "message": "..." }`):

| HTTP | error | Anlamı |
|------|-------|--------|
| 400 | `empty_body` / `invalid_json` | Gövde boş veya bozuk JSON |
| 400 | `missing_fields` | Zorunlu alan eksik (`message` eksik alanları listeler) |
| 401 | `unauthorized` | Anahtar geçersiz veya pasif |
| 405 | `method_not_allowed` | POST dışı istek |
| 500 | `server_error` / `create_failed` | Sunucu hatası |

---

## 5. Ekler

- `attachments` bir dizidir; her öğe `{ filename, contentType, contentBase64 }`.
- `contentBase64`: dosyanın **base64** içeriği.
- Dosya başına **en fazla 10 MB**, talep başına **en fazla 10 dosya**.
- Sınırı aşan / çözülemeyen ekler sessizce atlanır (talep yine oluşur).
- **Önemli — toplam istek boyutu ≤ 50 MB.** Base64, dosyayı ~%33 büyütür; bu
  yüzden pratikte tek istekte **yaklaşık 3-4 adet 10 MB'lık dosya** sığar.
  Toplam 50 MB'ı aşan istekleri sunucu (IIS) daha handler'a ulaşmadan reddeder.
  Çok/büyük ek gerekiyorsa dosyaları ayrı taleplere bölün veya sıkıştırın.

---

## 6. Mükerrer Engelleme (Idempotency)

Ağ hatasında güvenle tekrar deneyebilmeniz için her talepte kendi benzersiz
`externalRef` değerinizi gönderin. Aynı ürün + aynı `externalRef` ile ikinci
kez gelen istekte **yeni talep açılmaz**; mevcut talep `duplicate: true` ile döner.

---

## 7. İstemci Örnekleri

### cURL
```bash
curl -X POST "https://portal.folyazilim.com/api/v1/tickets.ashx" \
  -H "X-Api-Key: $RIVER_API_KEY" \
  -H "Content-Type: application/json; charset=utf-8" \
  -d '{
    "product":"Butce","externalRef":"BUTCE-2026-0142",
    "subject":"Rapor hatası","description":"Toplam yanlış.",
    "priority":"high",
    "requester":{"company":"Anadolu Yazılım A.Ş.","name":"Ali Veli","email":"ali@anadolu.com"}
  }'
```

### C# (.NET — HttpClient; Framework 4.x ve .NET Core/5+ uyumlu)
```csharp
using System.Net.Http;
using System.Text;

var payload = @"{
  ""product"":""Butce"",""externalRef"":""BUTCE-2026-0142"",
  ""subject"":""Rapor hatası"",""description"":""Toplam yanlış."",
  ""priority"":""high"",
  ""requester"":{""company"":""Anadolu Yazılım A.Ş."",""name"":""Ali Veli"",""email"":""ali@anadolu.com""}
}";

using (var http = new HttpClient())
{
    http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    var content = new StringContent(payload, Encoding.UTF8, "application/json");
    var resp = http.PostAsync("https://portal.folyazilim.com/api/v1/tickets.ashx", content).Result;
    var body = resp.Content.ReadAsStringAsync().Result;
    // resp.StatusCode == Created (201) => body içinde ticketNo
}
```

Ek göndermek için (C#): dosyayı `Convert.ToBase64String(File.ReadAllBytes(path))`
ile kodlayıp `attachments` dizisine ekleyin.

### Python (requests)
```python
import requests, base64

payload = {
    "product": "Butce",
    "externalRef": "BUTCE-2026-0142",
    "subject": "Rapor hatası",
    "description": "Toplam yanlış.",
    "priority": "high",
    "requester": {"company": "Anadolu Yazılım A.Ş.", "name": "Ali Veli", "email": "ali@anadolu.com"},
    "attachments": [{
        "filename": "ekran.png",
        "contentType": "image/png",
        "contentBase64": base64.b64encode(open("ekran.png", "rb").read()).decode()
    }],
}
r = requests.post(
    "https://portal.folyazilim.com/api/v1/tickets.ashx",
    json=payload,
    headers={"X-Api-Key": API_KEY},
    timeout=30,
)
print(r.status_code, r.json())
```

### Node.js (fetch — Node 18+)
```javascript
const res = await fetch("https://portal.folyazilim.com/api/v1/tickets.ashx", {
  method: "POST",
  headers: {
    "X-Api-Key": process.env.RIVER_API_KEY,
    "Content-Type": "application/json; charset=utf-8",
  },
  body: JSON.stringify({
    product: "Butce",
    externalRef: "BUTCE-2026-0142",
    subject: "Rapor hatası",
    description: "Toplam yanlış.",
    priority: "high",
    requester: { company: "Anadolu Yazılım A.Ş.", name: "Ali Veli", email: "ali@anadolu.com" },
  }),
});
console.log(res.status, await res.json());
```

### PHP (cURL)
```php
$payload = json_encode([
  "product" => "Butce",
  "externalRef" => "BUTCE-2026-0142",
  "subject" => "Rapor hatası",
  "description" => "Toplam yanlış.",
  "priority" => "high",
  "requester" => ["company" => "Anadolu Yazılım A.Ş.", "name" => "Ali Veli", "email" => "ali@anadolu.com"],
], JSON_UNESCAPED_UNICODE);

$ch = curl_init("https://portal.folyazilim.com/api/v1/tickets.ashx");
curl_setopt_array($ch, [
  CURLOPT_POST => true,
  CURLOPT_POSTFIELDS => $payload,
  CURLOPT_RETURNTRANSFER => true,
  CURLOPT_HTTPHEADER => ["X-Api-Key: $apiKey", "Content-Type: application/json; charset=utf-8"],
]);
$res = curl_exec($ch);
$code = curl_getinfo($ch, CURLINFO_HTTP_CODE);
curl_close($ch);
```

### Java (HttpClient — Java 11+)
```java
var body = """
{"product":"Butce","externalRef":"BUTCE-2026-0142",
 "subject":"Rapor hatası","description":"Toplam yanlış.","priority":"high",
 "requester":{"company":"Anadolu Yazılım A.Ş.","name":"Ali Veli","email":"ali@anadolu.com"}}""";
var req = HttpRequest.newBuilder()
    .uri(URI.create("https://portal.folyazilim.com/api/v1/tickets.ashx"))
    .header("X-Api-Key", apiKey)
    .header("Content-Type", "application/json; charset=utf-8")
    .POST(HttpRequest.BodyPublishers.ofString(body, java.nio.charset.StandardCharsets.UTF_8))
    .build();
var resp = HttpClient.newHttpClient().send(req, HttpResponse.BodyHandlers.ofString());
```

---

## 8. Önerilen İstemci Davranışı

- **Timeout + retry:** 30 sn timeout; 5xx/ağ hatasında aynı `externalRef` ile 2–3 tekrar (idempotent).
- **Anahtar gizliliği:** API anahtarını sunucu tarafında saklayın; istemci/tarayıcıya koymayın.
- **UTF-8:** Türkçe karakterler için gövdeyi UTF-8 gönderin.
- **Ekleri şişirmeyin:** Büyük dosyaları sıkıştırın; 10 MB üstü atlanır.

---

## 9. Onboarding (ürün eklemek)

1. Portal yöneticisi **Portal > API** ekranına girer.
2. Ürün adını yazıp **anahtar üretir**; anahtar bir kez gösterilir, güvenli yere kaydedilir.
3. Anahtar ürünün sunucu ortamına (env değişkeni) tanımlanır.
4. Ürün yukarıdaki `POST` çağrısını yapar. İlk test için cURL örneği yeterlidir.

Talep portala düştüğünde destek ekibi **Destek** kuyruğunda görür; talebin
kaynağı "Harici ürün: <ürün>" ve talep eden firma/kişi/e-posta bilgileriyle listelenir.


---
