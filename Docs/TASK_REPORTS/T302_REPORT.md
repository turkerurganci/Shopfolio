# T302 — Öteki kayıtların süreyle imhası

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- İmha parçaları kendi modüllerinin kaydında `AddDisposalPart<T>` ile eklendi: `ContactRequestDisposalPart` (CorporateContent), `DispatchRecordDisposalPart` (Notifications), `InboundNotificationDisposalPart` (Payment), `DefectClaimDisposalPart` (AfterSales), `DigitalFileDisposalParts` (`IncompleteDigitalUploadPart`, `DigitalFileDisposalPart`, `OrphanDigitalFilePart`) ve ortak silme deyimi `UnlinkedRowDeletion` (Catalog); giriş kaydı ve imha kaydı parçaları `RecordTablesDisposalParts` içinde.
- Kol okuması: `IOrderingModule.FindPaymentArmsAsync`, `FindOrdersOfItemsAsync`, `FindUnpaidFailedOrdersAsync` (yalnız okuma; siparişin ödeme kaydından).
- Süre sayımı tek işlevde: `RetentionCutoff.YearsBefore` (Türkiye saatinde takvim yılı). Gelen bildirim süresi 10 yıl `DisposalTimings.InboundNotificationRetentionYears` ve `09 §3.5` tablosundaki satırla birebir; yasal süreler (`DefectClaimRetention`, `DispatchRecordRetention`, `DisposalRecordPeriod`) `OrderRetention` kalıbıyla parçanın yanında.
- Hata düzeltme: `LoginRecordDisposalPart` kesimi `ToUniversalTime()` ile verilir (Npgsql +03:00 ofsetli değeri yazmıyor; D-377).
- `ImageDisposalParts` ortak silme deyimine taşındı (davranış değişmedi).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Disposal/` (`DisposalPart.cs`, `DisposalRegistration.cs`, `DisposalTimings.cs`, `RecordTablesDisposalParts.cs`, `RetentionCutoff.cs`)
- `Modules/AfterSales`, `Catalog`, `CorporateContent`, `Notifications`, `Ordering` (+ `Contracts/IOrderingModule.cs`, `OrderPaymentArms.cs`), `Payment`
- Testler: `UnitTests/Disposal/RecordRetentionTests.cs`, `Architecture/DisposalNumberSourceScan.cs`, `DisposalNumberScanTests.cs`, `VersionedSideBulkWriteScan.cs`, `Consistency/DisposalTimingsConsistencyTests.cs`; `IntegrationTests/Disposal/RecordDisposalTests.cs`, `DisposalHarness.cs`, `Model/StockAndCapacityTests.cs`
- Belgeler: `Docs/09_CODING_GUIDELINES.md` (§3.5 satırı), `DEFERRED_BACKLOG.md`, `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Her parça kendi işleminde küçük parçalarla siler, satırını yazar; kaydı olmayan parça satır yazmaz | ✓ | `RecordDisposalTests.AFailingChunk_DoesNotRollBackTheChunkBefore`, `APartThatFindsNothing_WritesNoDisposalRow` |
| 2 | Gönderim kaydı bağsızsa Z-34, bağlıysa Z-43; kol ödeme kaydından | ✓ | `ADispatchRecordOfNoOrder_GoesAtOneYear`, `ADispatchRecordOfAnOrder_GoesAtTenYears_OnBothArms`, `OnTheUnpaidArm_TheAddressAndTheEventValuesGoAtThreeYears_AndAPaidOrderKeepsThem`; `FindPaymentArmsAsync` |
| 3 | İletişim talebi konu süresi dolunca, açıkken de silinir | ✓ | `AContactRequest_GoesAtTwoYearsFromItsLastClosing_AndAnAboutOrderRequestAtTenYears` |
| 4 | Giriş kaydı Z-40, imha kaydı Z-44; kendi silmesi satır | ✓ | `ASignInRecord_GoesAtSixMonths_AndADisposalRecordAtThreeYears_WithItsOwnRowForTheDeletion`; Unit `DisposalRecordPeriod_IsThreeYears` |
| 5 | İşlem izini yalnız imha fonksiyonu siler, sayı yazılır | ✓ | T201 yolu: `DisposalTests.AuditLog_ARowPastTenYearsGoes_TodaysStays_AndTheRecordCountsOne` (306), `TheApplicationUser_CannotDeleteFromTheAuditTrailItself` (321); bu task'ta yeni parça yok |
| 6 | Ayıp talebi 10 yıl (Z-29), gelen bildirim 10 yıl, siparişten önce silinmez, ödemesiz Failed anından (Z-41), talep silinince bağlar boşalır | ✓ | `ADefectClaim_GoesTenYearsFromItsLastChange_OnlyAfterItsOrderIsDisposed_AndTheLinksAreEmptied`, `AnInboundNotification_OfNoOrder_GoesAtTenYears`, `..._OfAPaidOrder_StaysUntilTheOrderIsDisposed`, `..._OfAnUnpaidOrder_GoesTenYearsFromTheMomentTheOrderFailed` |
| 7 | Bağı kalmamış ve bir günden eski görsel ve dosya silinir; önce satır, sonra dosya | ✓ | `ADigitalFile_GoesWhenDetachedOrNeverAttached_...`, `AnOrphanDigitalFile_GoesAfterItsRowWasRecorded_...`; görsel: T223 `ImageFilesTests` |
| 8 | Bağlı görsel ve açılabilen kalemin dosyası silinmez | ✓ | `ADigitalFile_GoesWhenDetached...NotWhenAnOrderItemLinksToIt`; `ImageFilesTests.TheDeleteStatement_NamesEveryTableThatHoldsAForeignKeyToTheImageTable` |
| 9 | Dosya silen parça satırı dosyadan önce yazar; diskte kalan sonraki gece silinir | ✓ | `AnOrphanDigitalFile_GoesAfterItsRowWasRecorded_AndAFileOfARowStays` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 parça hatası ötekini geri almaz; boş parça satır yazmaz | ✓ | `RecordDisposalTests` 98, 123 |
| KK2 bağlı/bağsız gönderim ayrı kural | ✓ | `RecordDisposalTests` 186, 199, 215 |
| KK3 açık iletişim talebi süresi dolunca silinir | ✓ | `RecordDisposalTests` 136 |
| KK4 giriş kaydı süresi dolan gider, dolmayan kalır; imha kaydı silmesi satır yazar | ✓ | `RecordDisposalTests` 161 |
| KK5 uygulama kullanıcısı iz satırını silemez | ✓ | `DisposalTests` 321 (T201) |
| KK6 sipariş imhadan önce talep/bildirim kalır; süre başlangıçları; bağlar boşalır | ✓ | `RecordDisposalTests` 253, 269, 285 |
| KK7 bağsız görsel ve dosya silinir | ✓ | `RecordDisposalTests` 315, 344 |
| KK8 bağlı görsel ve dosya yerinde | ✓ | `RecordDisposalTests` 315 |
| KK9 satır dosyadan önce; kalan dosya sonraki gece | ✓ | `RecordDisposalTests` 344 |
| `09 §25` Süre/ayar: yeni süre `§3.5` tablosunda (gelen bildirim 10 yıl); yasal süreler `OrderRetention` kalıbıyla parçanın yanında | ✓ | `09_CODING_GUIDELINES.md` §3.5 satırı; `DisposalTimingsConsistencyTests` |
| `09 §25` Modül arayüzü yöntemi `SaveChanges` çağırmıyor; yeni üç yöntem yalnız okuma | ✓ | `IOrderingModule.cs` (`FindPaymentArmsAsync`, `FindOrdersOfItemsAsync`, `FindUnpaidFailedOrdersAsync`); Architecture 933 yeşil |
| `09 §25` Zaman `TimeProvider`'dan (`run.Now`); `DateTime.Now/UtcNow` yok | ✓ | diff taraması: eşleşme yok |
| `09 §25` Tarama istisnası: dosya düzeyinde iki yer (`DigitalFileDisposalParts.cs`, `InboundNotificationDisposalPart.cs`) gerekçeli: dijital dosya satırı ve gelen bildirim kaydı sipariş tarafı satırı değildir; `AllowedPlaces` özetinde yazılı. Ayıp talebi parçası listede değil (izlenen satırla `RemoveRange`) | ✓ | `VersionedSideBulkWriteScan.cs` 57–66 |
| `09 §25` Sayı kaynağı taraması yeni dosyaları kapsıyor | ✓ | `DisposalNumberSourceScan.AppliesTo`; `DisposalNumberScanTests` |
| Sık FAIL nedenleri: aynı sayıyı anan öteki yerler (backlog sayacı 143, STATUS/CHANGELOG/MEMORY, §3.5, D-99/D-102) güncel | ✓ | bu PR'ın belge diff'i |
| Sık FAIL nedenleri: main'in yeni zorunlu alanı (T334) testleri düşürmüyor | ✓ | birleşik ağaçta Unit 4110, `StockAndCapacityTests` 27 yeşil |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` (Architecture / Consistency / kalan olarak üç parça) | 933 + 242 + 2935 = 4110 geçti, 0 hata |
| Integration | ✓ | `--filter-class` ile: RecordDisposalTests 13, DisposalTests 18, OrderDisposalTests 27, StockAndCapacityTests 27 | hepsi geçti; ilk koşuda Files 81, Ordering 125, Concurrency 26, Model 1106, Database 36 de yeşildi |
| Format / derleme | ✓ | `dotnet build -warnaserror`, `dotnet format --verify-no-changes` | 0 uyarı, 0 hata, değişiklik yok |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-08) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### main birleştirmesi (doğrulama sonrası, T321 #439 girince)

- **Çakışan dosyalar:** `CatalogModule.cs` (kayıt satırları), `IOrderingModule.cs` (yöntemler), `OrderingModule.cs` (kurucu); belgelerde `DEFERRED_BACKLOG.md`, `IMPLEMENTATION_STATUS.md`.
- **Çözüm:** kod çakışmaları yalnız ekleme: iki tarafın kayıtları ve yöntemleri birlikte kaldı; `OrderingModule` kurucusu `OrderPaymentArms` ve `AdvisoryLocks`'u birlikte alır. Hiçbir tarafın davranışı değişmedi. Belgelerde STATUS "Son güncelleme" yalnız bu giriş (T321 tur 3 girişi `STATUS_CHANGELOG.md`'ye), backlog main kayıtları + dalınkiler, Aktif kalem yeniden sayıldı: 149.
- **Birleşik ağacın gerektirdiği test kurulumu:** `OrderingModule` artık `OrderPaymentArms` (→ `IPaymentModule`) ve `AdvisoryLocks` ister; `DisposalHarness` `AdvisoryLocks` kaydını, `ProductTransitionsTests`, `ConcurrencyTests`, `OrderTransitionsTests`, `ShipmentTransitionsTests` kurulumları `AddPaymentModule()` çağrısını aldı (yalnız test kurulumu).
- **Sonuç:** derleme `-warnaserror` 0 uyarı; Unit 933 + 242 + 2948 = 4123 geçti; Integration RecordDisposal 13, OrderDisposal 27, Disposal 18 + StockAndCapacity 27, ProductTransitions + ImageFiles 55, OrderTransitions + ShipmentTransitions 125, Concurrency + AuditTrail 48 geçti; format temiz; çakışma işareti yok. KK1–KK9 birleşik ağaçta ✓ (T302'nin parça kayıtları `CatalogModule`/`OrderingModule`'de duruyor, `IOrderingModule`'ün üç T302 yöntemi yerinde).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T302-kayit-imha`
- **Commit:** PR başlığında; head SHA PR'da
- **PR:** #446
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ayıp talebi durum geçmişi girdisinin `At` adı | D-374 | süre kaynağı bu alan; ad sahibi T320 |
| 2 | Gelen bildirim / gönderim kaydı taraması ve eski Failed siparişlerin yeniden okunması her gece | D-375 | ölçek; yük denemesinde ölçülür |
| 3 | Ayıp talebi parçası bütün eski talepleri her gece sayfalar | D-376 | ölçek |
| 4 | `LogFileRetention.Cutoff` +03:00 ofsetli an döner | D-377 | çağıran `ToUniversalTime` çağırır |
| 5 | `VersionedFormPageTests` sınıf birlikte koşunca titriyor | D-378 | T302'den bağımsız |

⚠ listesi: (a) gönderim kaydının kolu yalnız ödeme kaydından okunur, Failed şartı aranmaz; (b) parça siparişi kilitlemez; (c) ayıp talebi silmesi sipariş sürümünü artırmaz; (d) tarama istisnası dosya düzeyinde (yukarıda gerekçeli); (e) gelen bildirim 10 yıl tasarım değeridir, hukuki değil; (f) gelen bildirimin bağlı ve ödenmiş kolu siparişin imhasına kadar bekler; (g) test titremesi (D-378).

## Notlar

- **Working tree (Adım -1):** temiz; `hooksPath` `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** 42411903829, 42411903876 (success, af6e7d3), 42408991705 (success)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** `origin/main` birleştirmesi "Already up to date"; T334 sonrası zorunlu alan kaynaklı düşen test yok
