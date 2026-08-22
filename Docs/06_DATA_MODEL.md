# Shopfolio — Data Model

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `05`, `09`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 5 — Veri Modeli · **Rol:** Data / Domain Modeler
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (entity envanteri) geçilmez.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · Aktörler ve yetki derinliği:** Üç aktör; yönetim tarafı **tek rol, çoklu kullanıcı** — yönetici hesapları arasında yetki farkı yoktur, rol/izin modeli buna göre kurulur.
> - **K-17 · Terim sözlüğü:** Sözlük `02 §1`'de yaşar (Türkçe terim · İngilizce kod karşılığı · tanım). Bu doküman sözlüğü **birebir devralır, isim icat etmez**; bir kavramın tek adı vardır.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `02` gereksinimleri + `03` akış adımları → entity/alan eşlemesi.
> Gereksinimlerdeki her *"X bilgisi saklanır"* ifadesi bir alana dönüşmelidir. Eşlenmeyen gereksinim = **eksik entity veya alan**.

### 1.1 İleri izlenebilirlik (gereksinim/akış → entity.alan)

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|

### 1.2 Geri izlenebilirlik (entity/alan → kaynak)

| Entity.alan | Beslendiği kaynak | Durum |
|---|---|---|

### 1.3 Boşluklar (GAP) ve kararlar

| # | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|

---

## 2. Modelleme kuralları

> **Ne yazılır (bu bölüm dolu başlar — hepsi geç öğrenilen dersler):**

- **Enum'lar kaynak dokümandan birebir alınır**, şablondan kopyalanmaz. Genel bir şablondan gelen değerler bu projede uygulanamaz çıkar; kaynak `02 §5` ve `03 §1`'dir.
- **Silme stratejisi erken tanımlanır.** Her entity üç kategoriden birine girer: kalıcı silinir · yumuşak silinir · asla silinmez (denetim/uyumluluk). Kategori entity başına **baştan** kararlaştırılır.
- **Eşzamanlılık kontrolü baştan eklenir.** Durum makinesi + eşzamanlı geri çağrım ihtimali varsa iyimser eşzamanlılık (satır sürümü) MVP'de bile zorunludur; sonradan eklemek risklidir.
- **Denormalizasyon bilinçli bir karardır.** Her denormalize alan için *"nerede güncellenir, tutarsızlık riski nedir?"* cevaplanır.
- **Yumuşak silme + bileşik birincil anahtar birlikte kullanılmaz.** Silinen kaydın aynı kombinasyonla yeniden oluşturulması anahtar ihlali verir. Çözüm: vekil anahtar + filtreli tekil indeks.
- **Sayısal alanların ölçeği tek yerde tanımlanır** (oran mı yüzde mi, kaç ondalık). İki doküman iki farklı ölçek kullanırsa hata sessizdir.

---

## 3. Entity envanteri

| # | Entity | Sorumluluk | Silme kategorisi |
|---|---|---|---|

## 4. Entity tanımları

### 4.x <EntityAdı>

| Alan | Tip | Zorunlu | Varsayılan | Açıklama | Kaynak |
|---|---|---|---|---|---|

- **İlişkiler:**
- **Kısıtlar:**
- **İndeksler:**
- **Silme davranışı:**

## 5. Enum tanımları

> **Kaynak:** `02 §5` ve `03 §1`. Değerler **birebir** aynı olmalıdır.

| Enum | Değerler | Kaynak referansı |
|---|---|---|

## 6. İlişki diyagramı

## 7. İndeks ve kısıt özeti

## 8. Seed / başlangıç verisi

> **Ne yazılır:** Sistem açılışında gerekli kayıtlar. **Hangilerinin yokluğunda sistem açılmamalı** (fail-fast) açıkça belirtilir — bu liste `DEPLOY_RUNBOOK.md §A`'yı besler.

**Üç sınıf ayrılır:**

| Sınıf | Tanım | Nerede yaşar |
|---|---|---|
| **Seed** | Her kurulumda aynı olan referans verisi (durum kodları, varsayılan parametreler) | Migration / seed dosyası |
| **Fail-fast zorunlu** | Kodda varsayılanı **olmaması gereken** iş-kritik değerler — yoksa sistem açılmaz | `DEPLOY_RUNBOOK §A` |
| **Kuruluma özgü (seed edilemez)** | İlk yönetici, operatör/servis hesabı — kimin olacağı kuruluma bağlıdır, kodda varsayılanı olamaz | `DEPLOY_RUNBOOK §H` + izlenen bootstrap script'i |

> Üçüncü sınıf en sık atlanandır ve ilk deploy'da elle veritabanı komutu yazılmasına yol açar — izlenmez, tekrarlanamaz, ikinci ortamda baştan keşfedilir.

## 9. Saklama ve arşivleme

---

*Shopfolio — Data Model v0.1*
