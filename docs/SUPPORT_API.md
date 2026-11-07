# River Destek API — Faz 1 (Tek Yön: Ürün → Portal)

Farklı ürünlerin (Bütçe vb.) kendi içindeki destek taleplerini River Asistan Portal'ın
destek merkezine düşürmesi için basit bir REST endpoint'i.

- **Yön:** Tek yön (ürün → portal). Yanıt/durum geri dönüşü Faz 2'de.
- **Kimlik:** Her ürün için bir API anahtarı (Bearer). Anahtarlar portalda **hash'li** saklanır.
- **Format:** JSON (UTF-8). Yalnız **HTTPS**.

---

## 1. Anahtar üretme (yönetici)

Portal → `https://portal.folyazilim.com/ApiKeys.aspx` (yalnız portal yöneticisi).
Ürün adını girip **Anahtar Üret** → anahtar **bir kez** gösterilir (`rvr_...`). Kopyalayıp
ürünün yapılandırmasına koyun. Sonradan yalnız ön eki görünür; kaybolursa yeni anahtar üretin.

---

## 2. Endpoint

```
POST https://portal.folyazilim.com/api/v1/tickets.ashx
Authorization: Bearer <api_key>
Content-Type: application/json
```

### İstek gövdesi

| Alan                  | Zorunlu | Açıklama                                             |
|-----------------------|---------|------------------------------------------------------|
| `subject`             | evet    | Talep başlığı                                        |
| `description`         | evet    | Talep detayı                                         |
| `requester.company`   | evet    | Talebi açan son kullanıcının firması                 |
| `requester.name`      | evet    | Kişi adı                                             |
| `requester.email`     | evet    | E-posta                                              |
| `requester.phone`     | hayır   | Telefon                                              |
| `product`             | hayır   | Ürün adı (verilmezse anahtarın ürün adı kullanılır)  |
| `priority`            | hayır   | `low` / `normal` / `high` (varsayılan `normal`)      |
| `externalRef`         | hayır   | Ürünün kendi talep no'su — **idempotency** için önerilir |
| `attachments[]`       | hayır   | `{ filename, contentType, contentBase64 }` (dosya başına ≤ 10 MB, en çok 10 dosya) |

> Not: `requester.company/name/email/phone` yerine düz `company/name/email/phone`
> alanları da kabul edilir; `requester` alt objesi tercih edilir.

### Örnek istek

```json
{
  "product": "butce",
  "externalRef": "BUTCE-2026-0142",
  "subject": "Rapor ekranında toplam hatası",
  "description": "Aylık rapor toplam satırı yanlış hesaplıyor.",
  "priority": "high",
  "requester": {
    "company": "Anadolu Gıda A.Ş.",
    "name": "Ali Veli",
    "email": "ali.veli@anadolugida.com",
    "phone": "+90 212 000 00 00"
  },
  "attachments": [
    { "filename": "ekran.png", "contentType": "image/png", "contentBase64": "iVBORw0KGgo..." }
  ]
}
```

### Başarılı yanıt — `201 Created`

```json
{ "ticketId": 812, "ticketNo": "DST-2026-000812", "status": "New", "duplicate": false }
```

Aynı `product`+`externalRef` ile tekrar gönderilirse **yeni talep açılmaz**; mevcut talep
`200 OK` + `"duplicate": true` ile döner.

### Hatalar

| Kod | error              | Anlamı                                  |
|-----|--------------------|-----------------------------------------|
| 400 | `invalid_json`     | Gövde JSON değil                        |
| 400 | `missing_fields`   | Zorunlu alan eksik/geçersiz (mesajda listelenir) |
| 401 | `unauthorized`     | Anahtar geçersiz veya pasif             |
| 405 | `method_not_allowed` | POST dışı istek                       |
| 500 | `server_error`     | Sunucu hatası                           |

Her istek portalda `ApiRequestLog` tablosuna kaydedilir (denetim/sorun giderme).

---

## 3. Örnekler

### curl

```bash
curl -X POST https://portal.folyazilim.com/api/v1/tickets.ashx \
  -H "Authorization: Bearer rvr_xxx" \
  -H "Content-Type: application/json" \
  -d '{"product":"butce","externalRef":"BUTCE-1","subject":"Test","description":"Deneme",
       "requester":{"company":"X A.Ş.","name":"Ali","email":"ali@x.com","phone":"555"}}'
```

### C# (.NET — HttpClient)

```csharp
var payload = new {
    product = "butce",
    externalRef = "BUTCE-2026-0142",
    subject = "Rapor hatası",
    description = "Detay...",
    priority = "high",
    requester = new { company = "Anadolu Gıda A.Ş.", name = "Ali Veli",
                      email = "ali@anadolugida.com", phone = "+90..." }
    // attachments = new[]{ new { filename="a.png", contentType="image/png", contentBase64=b64 } }
};
using (var http = new HttpClient())
{
    http.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "rvr_xxx");
    var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(payload);
    var resp = await http.PostAsync("https://portal.folyazilim.com/api/v1/tickets.ashx",
        new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
    var body = await resp.Content.ReadAsStringAsync();
}
```

### Diğer diller

Standart HTTPS + JSON olduğu için her dil kullanılabilir (Java/OkHttp, Node/fetch,
Python/requests, PHP/cURL…). Tek gereken: `Authorization: Bearer <key>` başlığı ve yukarıdaki
gövde. Ek dosyalar base64 olarak `attachments` dizisinde gönderilir.

---

## 4. Portal tarafında ne olur?

1. Talep, sanal **"Harici Ürün Destekleri"** firması altına düşer; talep eden firma/kişi/
   e-posta/telefon talebin üstünde görünür (Kaynak: ürün adı).
2. Destek kuyruğuna girer; destek ekibine e-posta + üretici firmaya uygulama içi bildirim gider.
3. Destek ekibi talebi üstlenir/atar/yanıtlar/çözer (mevcut akış).

## 5. Kurulum notları

- `database/21_support_api.sql` bir kez çalıştırılmalı (Tickets alanları + ApiClients + ApiRequestLog).
- `ticketfiles/` klasörüne IIS App Pool **yazma izni** (ekler buraya kaydedilir) — zaten portal ekleri için gerekli.
- Büyük eklerde `web.config` `maxRequestLength` (şu an 50 MB) yeterli olmalı; çok büyük dosyalar için artırın.

## 6. Faz 2 (sonra konuşulacak)

Ürün tarafında yanıt/durum görüntüleme: webhook (push) veya `GET /api/v1/tickets/{ref}` (pull),
ürün tarafından mesaj ekleme, HMAC imza, tenant eşleme.
