# T203 — İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (doğrulama tur 1 FAIL) | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Ortak ardışık düzen** `Infrastructure/SignedEndpoints/SignedEndpointHandler`: (1) gövde tavanı — bildirilen uzunluk tavanı aşıyorsa gövde okunmadan 413; uzunluk yoksa en çok tavan+1 bayt okunur, sunucunun gövde sınırı da tavana çekilir; (2) form okunur (okunamayan, iki kez verilen alan, dosya → imza tutmuyor sayılır) ve ucun `Verify`'ı imzayı denetler — tutmazsa 400 gövdesiz, seyreltilmiş güvenlik satırı, kayıt ve iş satırı yok; (3) kayıt ile iş satırı `ITransactionRunner` işleminde, "alındı" (200 + ucun kendi gövdesi) işlem kesinleştikten sonra; tekrar `AlreadyRecorded` → aynı yanıt; (4) yazımda ya da kesinleşmede veritabanı hatası (`DbUpdateException`, `DbException`) → `UnexpectedError` satırı, gövdesiz 500.
- **Uç arayüzü** `ISignedEndpoint<TNotice>`: `Acknowledgement`, `Verify(formFields)`, `RecordAsync` (açık işleme yazar, kaydetmez; tekrar için `INSERT ... ON CONFLICT DO NOTHING`). Sonuç `InboundRecordOutcome` (`Recorded`, `AlreadyRecorded`).
- **Sabit zamanlı karşılaştırma** `Infrastructure/Integrations/SignatureComparison.Matches` (`CryptographicOperations.FixedTimeEquals`, UTF-8 bayt); boş/eksik imza asla tutmaz.
- **Seyreltilmiş satır** `SignatureFailureLog`: adres ekseni genel sınırınki (IPv4, IPv6 /64); ilk istek bir `SignatureFailureSampled` satırı (`reason`, `suppressedCount`), 60 sn içindekiler yalnız sayılır, sayı pencere sonrasındaki ilk satırdadır; IP satıra yazılmaz. Süpürme her bitmiş pencereyi bellekten düşürür; sayısı olan bitmiş pencere o anda kendi satırını yazar (bellek gördüğü adreslerle büyümez, sayı kaybolmaz).
- **Sabitler** `SignedEndpointLimits`: `BodyMaxBytes = 64 × 1024`, `FailureLogWindow = 60 sn`; `09 §3.5` tablosuna satır.
- **Bağlama** `AddSignedEndpoints()` (`Program.cs`) ve `MapSignedEndpoint<TEndpoint,TNotice>(path)`: yalnız POST, `DisableAntiforgery()`. Gerçek bildirim ucu T311'de, PayTR imza hesabı T402'de bağlanır.
- **Kaynak taraması** `UnitTests/Architecture/SignatureComparisonSourceScan` + kasıtlı ihlal örneği `ViolationSamples/SignatureComparison/EarlyExitComparisons.cs` (11 ihlal biçimi; sabit zamanlı çağrı ve yorumlar bulgu değil).
- **Belgeler:** `09 §3.5` satırı, `§11.1` m.5 (ortak düzen, karşılaştırma, tarama), `§15.2` m.2 (makine ucunda `DisableAntiforgery()`), `§18.2` m.3 (alanlar, süpürme); `CONTEXT.md` klasör haritası.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/SignedEndpoints/` (6 dosya), `Infrastructure/Integrations/SignatureComparison.cs`, `Program.cs`
- `tests/Shopfolio.UnitTests/SignedEndpoints/` (4), `Integrations/SignatureComparisonTests.cs`, `Architecture/SignatureComparisonSourceScan*.cs` + ihlal örneği, `Consistency/SignedEndpointLimitsConsistencyTests.cs`
- `tests/Shopfolio.IntegrationTests/SignedEndpoints/SignedEndpointDatabaseTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `.claude/CONTEXT.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İmza işleyiciden ve kuyruktan önce | ✓ | `SignedEndpointHandler.HandleAsync` sırası; `ValidSignature_IsVerifiedFirst_...` (`Verify, Begin, RecordAsync, Commit`); Integration `BrokenOrEmptySignature_...`/`MissingSignature_...` iş satırı 0 |
| 2 | Sabit zamanlı karşılaştırma | ✓ | `SignatureComparison.Matches` (`FixedTimeEquals`); `SignatureComparisonTests`; `SignatureComparisonSourceScanTests` (gerçek ağaç temiz + ihlal örneği 11 satır) |
| 3 | Tutmayan/imzasız → kayıt yok, işlenmez, 400 gövdesiz | ✓ | `BrokenOrEmptySignature_Gets400...`, `MissingSignature_Gets400...` (Unit + Integration: satır yok, iş satırı yok) |
| 4 | Seyreltilmiş güvenlik olayı | ✓ | `SignatureFailureLogTests` (sahte saat, pencere `FailureLogWindow`'dan); `RejectedRequest_WritesOneThinnedSecurityLine_...` (gövde/imza/IP yok) |
| 5 | Önce kayıt, kesinleşince "alındı" | ✓ | Unit sıra testi; Integration `ValidSignature_IsRecordedWithItsJobRow_AndReceivedOnlyAfterTheCommit` (kesinleşme anında dışarıdan 0 satır, yanıttan sonra 1 kayıt + 1 `InboundNotificationProcessing` iş satırı) |
| 6 | Tekrar kaydedilmez, işlenmez, "alındı" | ✓ | `SameNoticeAgain_...` (Unit + Integration: aynı durum ve gövde, tek kayıt, tek iş satırı) |
| 7 | Kaydedilemeyen → 500 | ✓ | Unit `DatabaseErrorOnTheWrite_/OnTheCommit_Gets500...`; Integration `DatabaseErrorOnTheCommit_Gets500...` (kesinleşmede `NpgsqlException`), `RecordThatBreaksARule_Gets500...` (`amount_not_negative`) — satır yok |
| 8 | Gelen kayıt kişisel alan taşımaz | ✓ | `InboundRecordFieldsTests` (alan → tür eşlemesi `05 §4.5.2`; kayıt ve `PaymentNotice` alan adları biçimleyicinin yasak listesinde değil; ihlal örneği `PayerCard`) |
| 9 | 64 KB tavan okumadan ve imzadan önce, 413; sahtecilik dışı | ✓ | `DeclaredLengthAboveTheLimit_Gets413WithoutReadingTheBody_...` (okunamayan akış), `UndeclaredBodyAboveTheLimit_...`, `BodyExactlyAtTheLimit_...`; Integration `BodyAboveTheLimit_Gets413...`; `MappedEndpoint_IsPostOnly_AndOutsideTheForgeryProtection`; Integration `TokenlessPost_IsHandledBySignedEndpoint_WhileTheProtectedFormOfTheSameAppIsRefused`; 429 T215 |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 imza işleyiciden önce; bozuk/eksik imzada iş satırı yok | ✓ | `SignedEndpointHandler.cs` `HandleAsync` (Verify → RunAsync); Integration `AssertEmptyAndNothingRecordedAsync` (`jobs` 0) |
| KK2 sabit zamanlı işlev | ✓ | `SignatureComparison.cs`; tarama yalnız bu dosyayı dışlar (`Applies_ToTheTwoFolders_ExceptTheComparisonFileItself`), ihlal örneği ayrı |
| KK3 bozuk imza 400, gövde boş, satır yok | ✓ | `BrokenOrEmptySignature_*` (Unit, Integration) |
| KK4 güvenlik satırı seyreltilmiş | ✓ | `SignatureFailureLogTests.*`; satırda yalnız `event`, `reason`, `suppressedCount` |
| KK5 kesinleşmeden sonra yanıt | ✓ | `ValidSignature_IsRecordedWithItsJobRow_AndReceivedOnlyAfterTheCommit` (`CommitStep` kesinleşme anında ayrı bağlantıdan sayar) |
| KK6 tekrar: ikinci satır yok, yanıt aynı | ✓ | `SameNoticeAgain_IsReceivedWithTheSameAnswer_AndWritesNoSecondRecordOrJobRow` |
| KK7 veritabanı hatasında 500 | ✓ | `DatabaseErrorOnTheCommit_Gets500...`, `RecordThatBreaksARule_Gets500...` |
| KK8 kayıtta kişisel alan yok | ✓ | `InboundRecordFieldsTests` (3 test) |
| KK9 64 KB üstü imzadan önce 413; sahtecilik denetimi yok; 429 T215 | ✓ | 413 testleri (`SignedEndpointLimits.BodyMaxBytes`'tan türetilir), `MappedEndpoint_*`, `TokenlessPost_*` |
| `09 §25` ilgili satırlar | ✓ | PR açıklaması |
| Güvenlik taraması | ✓ | Kayıt satırında gövde/imza/anahtar/IP yok (testler `DoesNotContain`); 500 satırında yalnız istisna türü (biçimleyici ileti yazmaz); tavan okumadan önce; tarama istisnası tek dosya |
| Sık FAIL listesi | ✓ | Sabitler `09 §3.5`'te; testler sabitten türetir; sahte saat (`ManualTimeProvider`); Integration kendi koleksiyonu (`SignedEndpoint`), yalnız kendi veritabanında `TRUNCATE`; ihlal örneği ayrı dosya; 09/CONTEXT/STATUS/MEMORY güncel |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... -c Release --filter-not-trait "Consistency=true"` | 1025/1025 |
| Unit (Consistency) | ✓ | `... --filter-trait "Consistency=true"` | 150/150 |
| Integration | ✓ | `--filter-namespace Shopfolio.IntegrationTests.SignedEndpoints` / `.Web` | 10/10, 75/75 (geri kalan ad alanları CI'da) |
| Contract | — | — | yok |

Derleme: `dotnet build Shopfolio.slnx -c Release -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 0; `restore --locked-mode` 0.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-05, head `d5c004a`) |
| Bulgu sayısı | 2 (B1, B2 — ikisi de ORTA, S1) |
| Düzeltme gerekli mi | Evet — B1 ve B2 bu PR'da |
| Bloke-etmeyen not (K) sayısı | 2 (K1, K2 — düzeltme turunda) |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (iki kod sabiti, `09 §3.5`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T203-imzali-makine-ucu`
- **Commit:** PR head
- **PR:** #397
- **CI:** *(izlenmedi — yönetici izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `DisableAntiforgery()` uca `IAntiforgeryMetadata.RequiresValidation = false` yazar; `IgnoreAntiforgeryTokenAttribute` bu üst veriyi taşımaz | `MappedEndpoint_IsPostOnly_AndOutsideTheForgeryProtection` (öznitelikle denendi: üst veri bulunamadı) | ✓ `09 §15.2` m.2'ye yazıldı |
| 2 | PostgreSQL `ON CONFLICT DO NOTHING` (hedefsiz) iki kısmi benzersiz dizinin ikisini de karşılar | Integration `SameNoticeAgain_*` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gerçek bildirim ucu (`/sistem/odeme-bildirimi`), kayıt + iş satırı yazan uygulama | — (planlı: T311) | task kapsamı ortak altyapı |
| 2 | PayTR imza hesabı | — (planlı: T402) | sağlayıcı uygulaması |
| 3 | Koruma dışı uçlar listesi `AntiforgeryExemptions` ve mimari testi | — (planlı: T209 m.6, KK6) | sahtecilik korumasının task'ı |
| 4 | Bozuk imzanın form gönderimi sınırına sayılması, 429 | — (planlı: T215 m.4) | hız sınırının task'ı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** CI 42042657330 success, Publish 42042657292 success (T202 #396)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** iki yapım işçisiyle yapıldı (devir notu 1); 500 ayrımı T212'nin yakalama katmanı gelene kadar işleyicide.

---

## Doğrulama Sonucu — T203 İmzalı makine ucu altyapısı — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T203-imzali-makine-ucu   **Commit:** d5c004a (doğrulanan head)

### Verdict: ✗ FAIL

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (switch öncesi ve sonrası, test koşularından sonra) |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42042657292 success · 42042657330 success · 42031439167 success |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22 T203 |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İmza bağlantı katmanında, işleyiciden ve kuyruktan önce | ✓ | `SignedEndpointHandler.HandleAsync` sırası: tavan → form → `Verify` → `RunAsync`; Unit `ValidSignature_IsVerifiedFirst_...` (`Verify, Begin, RecordAsync, Commit`); Integration `BrokenOrEmptySignature_*`, `MissingSignature_*`: `jobs` 0 satır |
| 2 | Sabit zamanlı karşılaştırma | ✓ | `Integrations/SignatureComparison.Matches` → `CryptographicOperations.FixedTimeEquals`; `src/` içinde başka `FixedTimeEquals`/imza karşılaştırması yok (grep); `SignatureComparisonSourceScanTests` (gerçek ağaç temiz, ihlal örneği 11 satır kırmızı) |
| 3 | İmzasız/tutmayan istek kaydedilmez, işlenmez, 400 gövdesiz | ✓ | Integration (gerçek PostgreSQL 18) `BrokenOrEmptySignature_Gets400...` ×2, `MissingSignature_Gets400...`: 400, boş gövde, kayıt 0, iş satırı 0 |
| 4 | Reddedilen istek seyreltilmiş güvenlik olayı olarak yazılır (`08 §3.2` ilke 2; `09 §18.2`) | ✗ | Tek adreste kural tutuyor (`SignatureFailureLogTests`). Süpürme ise iki sapma doğuruyor — bulgu B1: (a) bitmiş pencerenin sayısı adres geri dönmeden başka bir adresin isteğinde ayrı satır olarak yazılır, adres sonra döndüğünde ilk satırı `suppressedCount: 0` taşır (`SignatureFailureLog.cs:77`, `:90-93`, `:107-129`; dalın kendi testi `AnEndedWindowWithACount_IsWrittenAtTheNextSweep_...` satır 134-135 bunu sabitler) — `05 §4.5.2` "sayı ... o IP'nin yazılan ilk satırının alanı olur", `07 §4.30.1`, `VAL-E045` beklenen sonucu; (b) o ayrı satır, yazıldığı anki isteğin `requestId`/`method`/`path`'ini alır (`RequestLogEnricher.Enrich` satırı yazım anında `IHttpContextAccessor`'dan okur), yani başka adresin sayısı erişim kaydında yanlış IP'ye bağlanır — sınıfın kendi yorumu "the line's request id joins it to the access log" ile de çelişir |
| 5 | Önce kayıt, diske kesinleşince "alındı" | ✓ | Integration `ValidSignature_IsRecordedWithItsJobRow_AndReceivedOnlyAfterTheCommit`: kesinleşme anında dışarıdan 0 satır, yanıttan sonra 1 kayıt + 1 `InboundNotificationProcessing` iş satırı; `TransactionRunner` commit'ten sonra döner, yanıt ondan sonra yazılır |
| 6 | Tekrar kaydedilmez, işlenmez, yine "alındı" | ✓ | Integration `SameNoticeAgain_...`: aynı durum + gövde, tek kayıt, tek iş satırı (`ON CONFLICT DO NOTHING`) |
| 7 | Kaydedilemeyen bildirim "alındı" almaz, 500 | ✓ | Integration `DatabaseErrorOnTheCommit_Gets500...` (kesinleşmede `NpgsqlException`), `RecordThatBreaksARule_Gets500...` (`amount_not_negative`): 500, boş gövde, satır yok. Davranış tutuyor; 500'ün üretildiği yer `09 §8.1` ile çelişiyor — bulgu B2 |
| 8 | Gelen kayıt kişisel alan taşımaz | ✓ | `InboundRecordFieldsTests` (12 alanın her biri `05 §4.5.2` türüne eşli; kayıt ve `PaymentNotice` adları biçimleyicinin yasak listesinde değil; ihlal örneği `PayerCard`); ret satırında gövde/imza/IP yok (`RejectedRequest_WritesOneThinnedSecurityLine_...`), 500 satırında yalnız istisna türü (`ExceptionMasker`) |
| 9 | 64 KB tavan okumadan ve imzadan önce, 413; sahtecilik dışı; 429 T215 | ✓ | `ReadBodyWithinLimitAsync`: bildirilen uzunluk > tavan → okumadan null; Unit `DeclaredLengthAboveTheLimit_...` (okunamayan akış), `UndeclaredBodyAboveTheLimit_...`, `BodyExactlyAtTheLimit_...` ×2; Integration `BodyAboveTheLimit_Gets413...`; `MapPost` + `DisableAntiforgery()`: Unit `MappedEndpoint_IsPostOnly_...`, Integration `TokenlessPost_...` (aynı uygulamanın korumalı formu 400, imzalı uç 200), `OtherMethodThanPost_...`; 429 T215 m.4'te |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-E045 | KRİTİK | mock | BEKLEMEDE | FAIL (B1) | — |
| VAL-E046 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-E047 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-E050 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE (500 kısmı ✓; "son sorgu bulur" kısmı ödeme süresinin son sorgusunun task'ında) | — |
| VAL-E051 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-E052 | ORTA | mock | BEKLEMEDE | PASS | — |
| VAL-E036, E048, E049, E053, D082, A585 | — | — | BEKLEMEDE | BEKLEMEDE (T203'ün kriteri değil: DV-35 sandbox T402/T311; işaretli kayıt T311 m.5; hız sınırı T215 m.4; bildirimin tekilliği T311; ödeme oturumu `05 §3.6.3`) | — |

### Doğrulama Kontrol Listesi
- [x] KK1: imza işleyiciden önce; bozuk/eksik imzada iş satırı yok
- [x] KK2: sabit zamanlı işlev
- [x] KK3: bozuk imzada 400, gövde boş, satır yok
- [ ] KK4: güvenlik olayı satırı seyreltilmiş — süpürmeyle sayı adresin satırından kopuyor ve başka isteğe bağlanıyor (B1)
- [x] KK5: kesinleşmeden sonra yanıt
- [x] KK6: tekrarda ikinci satır yok, yanıt aynı
- [x] KK7: veritabanı hatasında 500
- [x] KK8: kayıtta kişisel alan yok
- [x] KK9: 64 KB üstü imzadan önce 413; sahtecilik denetimi bu uca uygulanmıyor; 429 T215 KK4'te
- [ ] `09 §25`: "İstisna yalnız beklenmeyen hâlde mi; ... hatayı yutan catch altı yerin dışında yok mu" ve "Kod hiçbir doküman cümlesiyle çelişmiyor mu" — hayır (B2, B1). Öteki satırlar evet: yeni sabit `§3.5`'te, klasör `CONTEXT.md`'de, zaman `TimeProvider`'dan, yeni paket yok, göç yok, kayıtta sır/kişisel veri yok, koruma dışı uç gerekçeli (`07 §2.5.5`)
- [x] Güvenlik taraması (aşağıda)

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 1175/1175 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (PostgreSQL 18, `shopfolio-t203-val-pg`) | ✓ | `--filter-namespace` ile: SignedEndpoints 10, Jobs 77, Web 75, Concurrency 26, Security 18, Database 35, Model 499 + 394 (iki parça), Logging + Disposal + FileSecrets 72 | 1206/1206, 0 hata |
| Restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; `git status` temiz; konteyner silindi |
| Dal CI | ✓ | `gh run list --branch task/T203-imzali-makine-ucu` | 42048534251 success (head d5c004a) |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (testteki HMAC anahtarı yer tutucu metin; kayıt satırlarında gövde/imza/anahtar/IP yok)
- Auth etkisi: Temiz (uç kimliksiz ve sahtecilik korumasının dışında — `07 §4.30.1`, §2.5.5; tek güvence imza)
- Input validation: Temiz (tavan okumadan önce; form değilse, iki kez verilen alan ya da dosya varsa 400; çatının form sınırları aşılırsa `InvalidDataException` → 400)
- Yeni bağımlılık: Yok

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| B1 | VAL-E045 | ORTA (eksik/yanlış kayıt — `12 §8.1`) | S1 | Beklenen: pencereden sonra gelen yeni geçersiz istek yeni satır doğurur ve önceki pencerenin sayısını kendi alanında taşır (`05 §4.5.2`, `07 §4.30.1`, `09 §18.2` m.3). Gerçekleşen: pencere bittikten sonra herhangi bir başka adresin isteği süpürmeyi tetiklerse sayı o başka isteğin satırı olarak yazılır (o isteğin `requestId`/`method`/`path`'iyle — `RequestLogEnricher`), adres dönünce satırı `suppressedCount: 0` taşır. Örnek: A t=0 ve t=1'de düşer; B t=61'de düşer → A'nın sayısı (1) B'nin istek kimliğiyle yazılır; A t=62'de düşer → 0. Dalın `09 §18.2` eklemesi ("IP dönmezse sayı bir sonraki süpürmede kendi satırıyla yazılır") `05`/`07`'yi karar kaydı olmadan değiştiriyor ve "kendi satırı" başka isteğe bağlanıyor. Bellek sınırı gerekçesi geçerli; çelişki ya karar kaydıyla belgelere girmeli ya da sayı adresin satırında kalacak, başka isteğe bağlanmayacak biçimde çözülmeli | VAL-E053 (aynı ret yolunun hız sınırı payı, T215) | bu PR'da düzeltilmeli | `src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignatureFailureLog.cs`, `tests/Shopfolio.UnitTests/SignedEndpoints/SignatureFailureLogTests.cs`, `Docs/09_CODING_GUIDELINES.md` §18.2 m.3 |
| B2 | — (`09 §25` "İstisna ... altı yer"; `09 §8.1` m.3, m.6, m.7) | ORTA (eksik kayıt/sayaç — `12 §8.1`) | S1 | Beklenen: makine ucunun beklenmeyen hatası tek yakalama katmanının "Makine ucu" satırına düşer (gövdesiz 500, `UnexpectedOperationError`, `05 §10.3.6` alanları, durum ucunun 15 dakikalık 500 sayacı); işleyici beklenmeyen hatayı yakalayıp yutmaz (m.6'nın "Yanlış" örneği: "yakalama katmanı onu hiç görmez, kayıt ve sayaç eksik kalır"). Gerçekleşen: `SignedEndpointHandler` `DbUpdateException` ve `DbException`'ı yakalayıp kendi `UnexpectedError` satırını yazıyor ve 500 dönüyor (`SignedEndpointHandler.cs:62-75`, `:151-158`); T212'nin katmanı geldiğinde bildirimin kaydedilemediği 500'ler katmana ve sayaca hiç ulaşmaz. Yapım raporu bunu "T212 gelene kadar" diye anıyor ama kayıtlı bir borç yok (backlog'da T203 kaydı yok). Düzeltme yolu: yakalamayı kaldırmak (katman yokken Kestrel zaten gövdesiz 500 döner; test barındırıcısı katmanın makine ucu satırını taklit eden ince bir ara katmanla ya da istisnanın yayıldığını ve hiçbir şeyin yazılmadığını sınayarak), ya da katmanın görmesini sağlayacak biçimde yeniden atmak | VAL-E050 | bu PR'da düzeltilmeli | `src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignedEndpointHandler.cs`, `tests/.../SignedEndpointHandlerTests.cs`, `tests/.../SignedEndpointDatabaseTests.cs` |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Seyreltme belleğinin sayı tavanı yok: bellek son iki pencerede düşen farklı adres sayısıyla sınırlı (süpürme ≥ 60 sn'de bir, yalnız ret geldiğinde). Zamanla büyümüyor (`EndedWindows_AreDroppedAtTheSweep_...` 50 → 1), ama çok sayıda farklı IPv6 /64'ten gelen kısa bir taşkında iki dakikalık adres kümesi kadar büyür; genel hız sınırının sayaçlarıyla aynı nitelik (`05 §11.2.9`). B1'in düzeltmesi süpürmeyi değiştireceği için aynı turda değerlendirilir | düzeltme turunda |
| K2 | Öz denetim tablosunun `09 §25` satırının kanıtı yalnız "PR açıklaması"; madde madde değil | düzeltme turunda |

### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Kriter 4 / KK4: rapor ✓ ve "sayı kaybolmaz" diyor; sayı kaybolmuyor ama adresin satırından kopuyor ve başka isteğin kimliğine bağlanıyor (B1). Raporun kanıt testi (`AnEndedWindowWithACount_...`) sapmayı sabitliyor.
- Kriter 7 / `09 §25`: rapor ✓; 500'ün işleyicide üretilmesi `09 §8.1` m.6–7 ile çelişiyor ve ertelemesi kayıtsız (rapor Notlar: "500 ayrımı T212'nin yakalama katmanı gelene kadar işleyicide") (B2).
- Öz denetim tablosu var, her maddesi kanıtlı; `09 §25` satırının kanıtı dolaylı (K2).
- Geri kalan 8 kriter ve KK'lar bağımsız doğrulamayla aynı sonuçta.

### Merge ve Post-merge
- Squash: — (FAIL; merge yok)
- Post-merge run'lar: —
