# T203 — İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-05

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
| Doğrulama durumu | *(validator)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

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
