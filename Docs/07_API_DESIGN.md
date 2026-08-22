# Shopfolio — API Design

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `04`, `05`, `06`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 6 — API Tasarımı · **Rol:** API Designer
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (endpoint envanteri) geçilmez.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · Aktörler:** Üç aktör; yetkilendirme tasarımı **tek yönetici rolü** varsayar (çoklu kullanıcı, aynı yetki).
> - **K-17 · Terim sözlüğü:** `02 §1` sözlüğü **birebir devralınır**; endpoint, alan ve enum adları sözlüğün İngilizce karşılıklarından türer, eş anlamlı ad kullanılmaz.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `04` ekran aksiyonları + `03` akış adımları → endpoint eşlemesi.
> Ekrandaki her aksiyon ve her veri ihtiyacı bir endpoint'e eşlenmelidir. Eşlenmeyen aksiyon = **eksik endpoint**.

### 1.1 İleri izlenebilirlik (ekran aksiyonu / akış adımı → endpoint)

| Kaynak ID | Kaynak özeti | Endpoint | Durum |
|---|---|---|---|

### 1.2 Geri izlenebilirlik (endpoint → kaynak)

| Endpoint | Beslendiği kaynak | Durum |
|---|---|---|

### 1.3 Boşluklar (GAP) ve kararlar

| # | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|

---

## 2. Konvansiyonlar (ENDPOINT'LERDEN ÖNCE)

> **Ne yazılır:** URL yapısı · yanıt zarfı veya standart hata formatı · kimlik doğrulama · yetkilendirme · sayfalama · filtreleme · sıralama · sürümleme · idempotency · hız sınırlama · tarih/sayı biçimi.
>
> **Neden önce:** Konvansiyonlar baştan sabitlenip onaylandığında endpoint detayları hem hızlı hem tutarlı yazılır; her endpoint'te aynı tartışma tekrarlanmaz.

| # | Konvansiyon | Karar | Gerekçe |
|---|---|---|---|

## 3. Endpoint envanteri

| # | Method + Path | Amaç | Aktör / yetki | Kaynak ekran/akış |
|---|---|---|---|---|

## 4. Endpoint tanımları

### <METHOD> <path>

- **Amaç:** · **Yetki:** · **Hız sınırı:**
- **Request:** (parametreler, gövde, doğrulama kuralları)
- **Response (başarı):**
- **Response (hata):** (hata kodları — `06` ve `02` ile birebir)
- **Yan etkiler:** (durum geçişi, olay yayını, bildirim, denetim kaydı)
- **Idempotency:**

## 5. DTO envanteri

> **Kural:** Alan adları ve enum değerleri `06_DATA_MODEL.md` ile **birebir** eşleşir.

## 6. Hata kodları

| Kod | HTTP | Anlamı | Nerede döner |
|---|---|---|---|

## 7. Gerçek zamanlı olaylar (varsa)

## 8. İstemci karmaşıklığını azaltan kalıplar

> **Ne yazılır:** Durum × rol iş mantığının sunucuda tutulduğu yerler. Örneğin yanıta "şu an yapılabilecek aksiyonlar" listesi koymak, istemcinin bu mantığı kopyalamasını önler.

## 9. Yönetim (admin) endpoint'leri

## 10. Değişiklik kaydı

> **Ne yazılır:** `08` yazılırken bu dokümana **geriye dönük** eklenen endpoint'ler. Entegrasyon dokümanı doğası gereği yeni endpoint ihtiyacı doğurur; bu beklenen bir durumdur.

---

*Shopfolio — API Design v0.1*
