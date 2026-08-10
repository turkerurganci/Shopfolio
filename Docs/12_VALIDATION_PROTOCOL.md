# Shopfolio — Validation Protocol

**Versiyon: v0.1** | **Bağımlılıklar:** `02`–`11` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 10 — Doğrulama Protokolü · **Rol:** QA / Verification Lead
> **Bu bir test case deposu değildir.** Cevapladığı soru: **"Bu ürün bir sonraki aşamaya geçilebilecek olgunlukta mı?"**
> **Kapsam `10_MVP_SCOPE.md` ile birebir hizalıdır.**

---

## 1. Amaç ve kapsam

### 1.1 Amaç
### 1.2 Kapsam dahili

| Alan | Kapsam |
|---|---|

### 1.3 Kapsam dışı

| Alan | Neden |
|---|---|

---

## 2. Doğrulama prensipleri

> Bu bölüm **dolu başlar** — evrensel prensiplerdir, projeye göre değişmez.

| # | Prensip | Anlamı |
|---|---|---|
| **P1** | Gereksinim bazlı doğrulama | Her madde bir iş kuralına/akış adımına/spesifikasyona dayanır. "Çalışıyor gibi görünüyor" kabul edilmez; beklenen sonuç **önceden** tanımlıdır. |
| **P2** | Kanıt zorunluluğu | Kanıtsız PASS geçersizdir. Her sonuç en az bir kanıt türüyle desteklenir (§7). |
| **P3** | Çok katmanlı kanıt | Kritik akışlarda tek bir arayüz gözlemi yetmez; durum, veri, log ve olay düzeyinde tutarlılık aranır. |
| **P4** | Failure ve geri alma dahil | Yalnız happy path doğrulanmaz. "Başarısız olduğunda güvenli duruma dönüyor mu?" da kanıtlanır. |
| **P5** | Yapan ≠ denetleyen | Kodu yazan ajan kendi çıktısını doğrulayamaz. Doğrulama **ayrı context**te yapılır. |
| **P6** | Tekrar edilebilirlik | Manuel doğrulama geçici olarak kabul edilir; otomatize edilebilenler zamanla otomatize edilir. |
| **P7** | Kapsam hizalaması | MVP'de olmayan özellik kriter olamaz; MVP'de olan çıkarılamaz. |
| **P8** | Tıkanma yasağı | FAIL'de net düzeltme akışı vardır (§8). Kritik olmayan açık bulgular **kayıtlı** risk olarak kabul edilebilir. |

---

## 3. Doğrulama seviyeleri

> **Not:** Doğrulama **seviyeleri** (bu bölüm) ile kalite **kapıları** (00 §D.6) farklı eksenlerdir. Seviyeler *neyin* doğrulandığını, kapılar *ne zaman* doğrulandığını söyler.

| Seviye | Soru | Ne zaman tetiklenir | Kaynak |
|---|---|---|---|
| **A — Gereksinim** | İş kuralları gerçekten karşılanıyor mu? | İlgili task bitince | `02`, `10` |
| **B — Fonksiyonel** | Akışlar uçtan uca çalışıyor mu? | Task + faz | `02`, `03`, `10` |
| **C — Durum makinesi** | İzinli geçişler çalışıyor, yasak geçişler engelleniyor mu? | Durum makinesine dokunan her task | `03`, `06` |
| **D — Veri** | Veri doğru saklanıyor, dönüşüyor, siliniyor mu? | Veri katmanı task'ları | `06`, `09` |
| **E — Entegrasyon** | Dış servislerle sözleşme doğru mu? | Entegrasyon task'ları | `08` |
| **F — Operasyonel** | Sistem gerçek koşullarda ayakta kalıyor mu? | Faz sonu + final | `05`, `08`, `09` |

---

## 4. Doğrulama matrisi

> **Ne yazılır:** Her doğrulama maddesi bir satır.

| Sütun | Açıklama |
|---|---|
| **ID** | Benzersiz tanımlayıcı (`VAL-NNN`) |
| **Seviye** | A–F (§3) |
| **Kural / Gereksinim** | Doğrulanacak iş kuralı veya davranış |
| **Kaynak** | Kuralın tanımlandığı doküman ve bölüm |
| **Ön koşul** | Doğrulama öncesi sağlanması gereken durum |
| **Beklenen sonuç** | PASS için sağlanması gereken koşul |
| **Kanıt türü** | Kabul edilen kanıt(lar) — §7 |
| **Kanıt ortamı** | `mock` / `sandbox` / `gerçek` — kanıtın hangi ortamda üretildiği |
| **Severity** | KRİTİK / ORTA / DÜŞÜK — §8 |
| **Durum** | `BEKLEMEDE` / `PASS` / `FAIL` / `KABUL EDİLMİŞ RİSK` |

| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |
|---|---|---|---|---|---|---|---|---|---|
| VAL-A001 | A | | 02 §… | | | | mock/sandbox/gerçek | KRİTİK/ORTA/DÜŞÜK | BEKLEMEDE |

### 4.1 Durum tanımları

| Durum | Anlamı |
|---|---|
| `BEKLEMEDE` | Ön koşulu sağlanmadığı için **henüz doğrulanamadı**. PASS da FAIL da değildir. |
| `PASS` | Kanıtla doğrulandı |
| `FAIL` | Beklenen sonuç sağlanmadı — §8 akışı başlar |
| `KABUL EDİLMİŞ RİSK` | FAIL ama proje sahibi onayıyla kayda alındı (§8.4). KRİTİK olamaz. |

**Neden ayrı bir alan:** `§6.3` final çıkışı *"hiçbir KRİTİK madde FAIL veya BEKLEMEDE olamaz"* der. Durum bir alanda tutulmuyorsa bu soru **mekanik olarak cevaplanamaz** — "hangi maddeler hâlâ beklemede?" sorusu 200 madde arasında elle aranır ve pratikte hiç sorulmaz.

**Neden kanıt ortamı ayrı bir alan:** `§6.3` ve `§11` mock ile alınmış PASS'lere iki farklı sonuç bağlar (final gate'i geçemez · gerçek entegrasyona geçince geçersizleşir). Hangi maddenin mock ile PASS aldığı kaydedilmiyorsa **her iki kural da sessizce devre dışı kalır** — bu, playbook'un önlemek için var olduğu defekt sınıfının ta kendisidir.

---

## 5. Giriş kriterleri

> **Ne yazılır:** Bir doğrulamanın başlayabilmesi için sağlanması gerekenler (seviye bazlı).

**Değişmez kapılar (00 §F.1):** working tree temiz · ana dal CI son 3 run yeşil · repo hafızası güncel · hook'lar kurulu (`git config core.hooksPath`).

**Değişmez giriş kuralları:**

| # | Kural |
|---|---|
| 1 | **Ön koşulu sağlanmayan madde `BEKLEMEDE` kalır — PASS veya FAIL verilemez.** Doğrulanamayan bir maddeyi FAIL saymak yanlış sinyal, PASS saymak kanıtsız onaydır. |
| 2 | **Mock/gerçek ayrımı kayıt altındadır.** Seviye E (entegrasyon) doğrulaması, hangi dış servisin mock, hangisinin gerçek olduğu kaydedilmeden **başlatılamaz**. Kayıt yeri: matrisin "Kanıt ortamı" sütunu. |
| 3 | Task bazlı doğrulama, task'ın kabul kriterleri ve doğrulama kontrol listesi `11`'de tanımlı olmadan başlatılamaz. |

---

## 6. Çıkış kriterleri

### 6.1 Task bazlı çıkış

| Kriter | Açıklama |
|---|---|
| İlgili maddeler PASS | Task'a ait tüm doğrulama maddeleri PASS |
| KRİTİK açık bulgu yok | |
| ORTA bulgular kayıt altında | `DEFERRED_BACKLOG.md`'ye yazılmış |
| Kanıt üretilmiş | §7'ye uygun |

### 6.2 Faz bazlı çıkış

| Kriter | Açıklama |
|---|---|
| Faz içi tüm task'lar §6.1'i karşılamış | |
| Entegrasyon kontrolü yapılmış | |
| Regresyon kontrolü yapılmış | Önceki fazlar dahil |
| Açık risk envanteri güncel | Proje sahibi tarafından kabul edilmiş |
| Öğrenim terfisi yapılmış | 00 §K |

### 6.3 Final çıkış (MVP release gate)

| Kriter | Açıklama |
|---|---|
| Tüm KRİTİK maddeler PASS | Hiçbiri FAIL veya BEKLEMEDE olamaz |
| ORTA maddeler PASS veya kabul edilmiş risk | |
| DÜŞÜK maddeler bloklamaz | Ama kayıtlıdır |
| Operasyonel doğrulama tamamlanmış | Seviye F |
| Happy path uçtan uca geçmiş | |
| Çekirdek entegrasyonlar **mock olmayan** ortamda doğrulanmış | Mock PASS bu maddeler için final gate'i geçemez — BEKLEMEDE kalır |
| Kanıt arşivi erişilebilir | |
| Proje sahibi onayı | |

---

## 7. Kanıt standardı

### 7.1 Kabul edilen kanıt türleri

| Tür | Örnek |
|---|---|
| API/arayüz yanıtı | İstek + tam yanıt (durum kodu + gövde) |
| Veri kaydı | İlgili tablo satırının snapshot'ı |
| Yapılandırılmış log | Korelasyon kimliğiyle filtrelenmiş çıktı |
| Olay kaydı | Yayınlanmış/işlenmiş olay satırı |
| Test raporu | Otomatik test çıktısı (**tekrar edilebilirlik açısından tercih edilir**) |
| Dış sistem kaydı | Üçüncü parti tarafındaki işlem kaydı |
| İzleme/uyarı | Tetiklenen uyarı veya pano görüntüsü |

### 7.2 Severity'ye göre minimum kanıt

| Severity | Minimum |
|---|---|
| KRİTİK | **En az 2 farklı türde** kanıt (çok katmanlı — P3) |
| ORTA | 1 kanıt türü |
| DÜŞÜK | 1 kanıt türü |

### 7.3 Kanıt kuralları

- Kanıt **doğrulama anında** üretilmiş olmalıdır — eski veya başka ortamdan alınan kanıt geçersizdir.
- Kanıtlar madde ID'siyle eşleştirilerek saklanır.
- Tek bir arayüz gözlemi KRİTİK akışlarda tek başına yeterli değildir.
- *"Lokal makinemde geçiyor"* CI kanıtını ikame etmez.

---

## 8. Severity ve hata yönetimi

### 8.1 Sınıflandırma

| Severity | Tanım |
|---|---|
| **KRİTİK** | Veri/varlık kaybı, yanlış sahiplik, yasak durum geçişi, mükerrer tamamlanma, güvenlik ihlali. Release'i mutlak bloklar. |
| **ORTA** | Yanlış retry davranışı, eksik log, kurtarma zayıflığı, kayıp yaratmayan iş kuralı defekti. Kayıtlı risk olarak kabul edilebilir. |
| **DÜŞÜK** | Arayüz kusuru, metin hatası, operasyonu bloklamayan görünürlük sorunu. |

### 8.2 FAIL akışı

```
FAIL tespit edilir
  → Defect kaydı (madde ID, severity, beklenen vs gerçekleşen, etkilenen komşu maddeler)
  → Düzeltme (etki alanı belirlenir)
  → Yeniden doğrulama (+ regresyon — §8.3)
  → PASS → kapatılır, kanıt eklenir | Tekrar FAIL → döngüye döner
```

### 8.3 Regresyon kuralları

- KRİTİK durum-akışı veya veri-akışı düzeltmelerinde **aynı akışa ait tüm maddeler** yeniden doğrulanır. Dar kapsam kabul edilmez.
- ORTA/DÜŞÜK düzeltmelerde yalnız doğrudan etkilenen maddeler.
- **Şüphe durumunda yeniden doğrulanır.** Şüpheli maddeyi atlamak kabul edilmez.

### 8.4 Kabul edilmiş risk

Koşullar: severity **KRİTİK olamaz** · proje sahibi onayı zorunlu · risk kaydı oluşturulur (madde ID, açıklama, olası etki, mitigasyon, kabul tarihi) · `DEFERRED_BACKLOG.md`'ye yazılır · final çıkışta yeniden değerlendirilir.

### 8.5 Tıkanma yasağı

- Bir FAIL düzeltilirken bağımsız task'ların doğrulaması devam eder.
- Düzeltme **2 iterasyonda** çözülemezse proje sahibine eskale edilir: kapsam daraltma, alternatif yaklaşım veya kabul edilmiş risk.

---

## 9. Ajan çalışma modeli

| Rol | Sorumluluk | Context |
|---|---|---|
| **Coding agent** | Task'ı implement eder, testleri yazar | Kendi context'i |
| **Reviewer agent** | Doğrulama maddelerini çalıştırır, kanıt üretir, PASS/FAIL verir | **Ayrı context** — coding context'ini görmez |

Aynı ajan aynı task için iki rolü üstlenemez.

**Reviewer'a verilecek dokümanlar:** 00 §C.6 katman tablosu + task tanımı + değişen kod.
**Kaynak-güdümlü ek:** Bir maddenin kaynak sütununda referans verilen doküman katman setinde yoksa **yine de eklenir**.

---

## 10. Roller ve onay

| Karar | Onay veren |
|---|---|
| Madde PASS / FAIL | Reviewer agent (kanıtla) |
| Kabul edilmiş risk | **Proje sahibi** |
| Faz bazlı çıkış | Proje sahibi |
| Final çıkış | Proje sahibi |
| Eskalasyon | Proje sahibi |
| Regresyon kapsamını genişletme | Teknik karar verici |

**Sorumluluk sınırları:** Reviewer PASS/FAIL verir ama kabul edilmiş risk kararı **alamaz**. Coding agent kendi kodunu review edemez. Hiçbir rol tek başına final çıkış kararı veremez.

---

## 11. Yeniden doğrulama

| Tetikleyici | Kapsam |
|---|---|
| Defect düzeltmesi | FAIL alan madde + etkilenen komşu maddeler |
| Mock → gerçek entegrasyon geçişi | O entegrasyonun **tüm** maddeleri + bağımlı fonksiyonel maddeler. **Mock ile alınmış PASS'ler geçersiz sayılır.** |
| Faz geçişi | Yeni kodun etkileyebileceği önceki faz maddeleri |
| Konfigürasyon değişikliği | Değişen parametreye bağımlı maddeler |
| Altyapı değişikliği | Şema değişimi, ortam değişimi, bağımlılık güncellemesi → etkilenen seviyedeki tüm maddeler |

### 11.1 Yeniden doğrulama sonuçları

| # | Kural |
|---|---|
| 1 | Re-validation sonucu orijinal maddenin **Durum** alanını günceller. |
| 2 | **Re-validation'da FAIL çıkarsa bu bir regresyondur** — normal bir FAIL gibi işlenmez; §8 akışı **S2 Kırılma** olarak başlar. Daha önce PASS almış bir maddenin geri düşmesi, yeni bir maddenin FAIL almasından farklı bir sinyaldir. |
| 3 | **En güncel kanıt geçerlidir.** Yeni kanıt eskisinin yerine geçer; eski kanıt arşivde kalır ama geçerliliğini yitirir. Aynı madde için iki çelişen kanıt yan yana bırakılmaz. |
| 4 | Mock ile alınmış PASS'ler gerçek entegrasyona geçildiğinde **geçersiz sayılır** ve madde `BEKLEMEDE`'ye döner (§6.3). |

---

## 12. Proje-spesifik ekler

> **Ne yazılır:** Bu projenin domain'ine özel doğrulama tabloları — durum geçiş matrisi, kritik hesaplama doğrulamaları, sahiplik/yetki kontrolleri, idempotency kontrolleri, denetim izi kontrolleri.
> Bu bölüm **projeye göre tamamen değişir**; yukarıdaki §1–§11 değişmez.

---

*Shopfolio — Validation Protocol v0.1*
