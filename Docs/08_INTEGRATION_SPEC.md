# Shopfolio — Integration Specifications

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `05`, `06`, `07` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 7 — Entegrasyon Spesifikasyonları · **Rol:** Integration Engineer
> **Traceability zorunlu:** Hayır
> **Beklenti:** Bu doküman doğası gereği `07`'ye **geriye dönük endpoint** ekletir (geri çağrım/webhook uçları). Bu bir hata değil, beklenen sonuçtur.

---

## 0. Yazım kuralları

- **Her entegrasyon ayrı bölüm.** API limitleri, hata senaryoları, retry stratejisi, geri düşme planı.
- **Bağımlılık riski:** Her entegrasyon için *"bu servis çökerse ne olur?"* cevaplanır.
- **Alan eşlemesi yazarken `06` açık tutulur** ve her alan adı kontrol edilir. Bu aşama, veri modeliyle uyumsuz alan adlarının en sık yakalandığı yerdir.
- **"Ücretsiz seçenek yeter mi?" sorusu ÖNCE sorulur.** Ücretli bir sağlayıcı seçilip sonra ücretsizin yettiğini keşfetmek sık görülen bir israftır.
- **Dış çağrıyı yapıp yapmamayı belirleyen kural burada da yazılır.** Bir iş kuralı (ör. minimum eşik) dış servis çağrısının yapılıp yapılmayacağını belirliyorsa, kaynağı `02` olsa bile burada tekrar edilir.

---

## 1. Entegrasyon envanteri

| # | Servis | Ne için | Kritiklik | Ücretli mi | Kota / limit |
|---|---|---|---|---|---|

## 2. Entegrasyon tanımları

### 2.x <Servis adı>

- **Amaç:**
- **Kimlik doğrulama:** (key türü, nerede saklanır, nasıl döndürülür)
- **Kullanılan uçlar:**
- **Alan eşlemesi:** (dış alan → iç entity.alan — `06` ile kontrol edilmiş)

| Dış alan | İç alan | Dönüşüm | `06` doğrulandı |
|---|---|---|---|

- **Hız limitleri ve kota:**
- **Hata senaryoları ve kodları:**
- **Retry stratejisi:** (kaç deneme, hangi gecikme, hangi hatalarda)
- **Idempotency / tekilleştirme anahtarı:**
- **Geri çağrım / webhook:** (imza doğrulama **zorunlu**, tekrar gönderim davranışı)
- **Servis çökerse:** (geri düşme, kullanıcıya ne gösterilir, süreler donar mı)
- **Test / sandbox imkânı:**
- **Runbook:** [`INTEGRATION_RUNBOOKS/<SERVIS>.md`](INTEGRATION_RUNBOOKS/)

## 3. Ortak hata yönetimi

## 4. Dış varsayımlar

> **Ne yazılır:** Bu dokümanın dayandığı her dış varsayım ve **doğrulama kanıtı**. Task'ların ön-uçuş kontrolü (00 §F.3) bu tabloyu başlangıç noktası alır.

| # | Varsayım | Kanıt | Doğrulama tarihi |
|---|---|---|---|

## 5. `07`'ye geri yansıtılanlar

| # | Yeni ihtiyaç | `07`'de eklenen |
|---|---|---|

---

*Shopfolio — Integration Specifications v0.1*
