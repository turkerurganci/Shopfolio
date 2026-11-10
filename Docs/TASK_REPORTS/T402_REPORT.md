# T402 — PayTR: bildirim imzası ve alan eşlemesi

**Faz:** F4 | **Durum:** ⏳ Doğrulama bekliyor (kriter 8'in test modu gözlem payı OWNER_INPUT — D-1038) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `PayTrNotification` (yeni, `Infrastructure/Integrations/`): `Verify` — `merchant_oid` + `merchant_salt` + `status` + `total_amount`'ın `merchant_key` ile HMAC-SHA256 Base64'ü (`MessageSignature.HmacSha256Base64`, T401'in tek imza işlevi) `hash` ile tek karşılaştırma işlevi `SignatureComparison.Matches` (`CryptographicOperations.FixedTimeEquals`) üzerinden karşılaştırılır; `hash` yok/boş/bozuk, imzalı alan eksik ya da anahtar ayarı yoksa `SignatureInvalid`. İmzasız `payment_amount`, `payment_type`, `currency`, `test_mode` imzaya girmez, yalnız tutarlılık denetimine taşınır.
- Eşleme: `merchant_oid` tiresiz hâliyle iki alana (`CardPaymentReference` ile geri eşlenen sipariş numarası ve sağlayıcı referansı), `status` → `Outcome`, `total_amount` → `Amount` (`PaymentAmounts.FromKurus`), olay türü `Sale`. Kuruş olmayan tutar okunmaz, imza yine tutar.
- `ReadResult`: yalnız `Sale` + `success`/`failed` tanınır; dört kol (`status`, `payment_type` ≠ `card`, `currency` ≠ `TL`, canlıda `test_mode` = 1) tanınmayan değerdir. `CheckAmount`: `payment_amount` = `Order.Total` × 100 ve `total_amount` ≥ aynı değer (kuruşta, `PaymentAmounts.NotificationMatches`); çözülemeyen tutar uyuşmaz (`09 §14.2` m.3).
- `PayTrPaymentGateway`: üç bildirim işlevi `NotSupportedException` yerine `PayTrNotification`'a bağlandı; doğrulanmış `failed` bildiriminin `failed_reason_code`/`failed_reason_msg`'si `PaymentFailureReasonLog` ile seyreltilerek uygulama kaydına (`PaymentFailedReasonSampled`) yazılır, başka yerde saklanmaz. Katman HTTP yanıtı üretmez; `OK`/400/500 T311'in ucunda.
- D-108: `SignatureFailureLog` pencere tavanı `SignedEndpointLimits.FailureLogMaxAddresses` = 10 000; taşmada önce sayısız bitmiş pencereler, sonra en eski sayılı pencere sayısı hemen yazılarak düşer (K-1582).
- Belgeler: `09 §3.5` (tavan + red nedeni penceresi satırları), `09 §18.2` m.3 (`PaymentFailedReasonSampled`), `08 §4.1` DV-1/2/4/9/35/36/42 "Doğrulama tarihi" hücreleri, `INTEGRATION_RUNBOOKS/PAYTR.md` (kapsam, §1, §5, §6, §6.1 yedi DV satırı). Backlog: D-108 ✅, D-455 ✅, D-741 güncellendi (hedef T403), yeni D-1038 (`DEFERRED_BACKLOG.d/T402.md`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/` (`PayTrNotification` yeni, `PaymentFailureReasonLog` yeni, `PayTrPaymentGateway`, `PayTrRegistration`, `PaymentAmounts`), `Infrastructure/Logging/LogEvents.cs`, `Infrastructure/SignedEndpoints/` (`SignatureFailureLog`, `SignedEndpointLimits`)
- Testler: `UnitTests/Integrations/PayTrNotificationTests.cs` (yeni), `PaymentAmountsNotificationTests.cs` (yeni), `PayTrPaymentGatewayTests.cs`, `IntegrationTestSupport.cs`, `UnitTests/SignedEndpoints/SignatureFailureLogTests.cs`; `IntegrationTests/Payment/PaymentNotificationTests.PayTr.cs` (yeni), `PaymentNotificationTests.cs`
- Dokümanlar: `08_INTEGRATION_SPEC.md`, `09_CODING_GUIDELINES.md`, `INTEGRATION_RUNBOOKS/PAYTR.md`, `DEFERRED_BACKLOG.md`, `DEFERRED_BACKLOG.d/T402.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dört alanın `merchant_key` ile HMAC-SHA256 Base64'ü `hash` ile sabit zamanlı karşılaştırılır | ✓ | `PayTrNotificationTests.KnownExample_TheComputedHashMatchesTheValueComputedOutsideDotNet_…_KK1` (openssl ile üretilmiş örnek), `ChangingASignedField_BreaksTheSignature_KK1` (4 alan), `TheComparison_GoesThroughTheOneFixedTimeComparison_KK1` |
| 2 | `hash` yok/tutmuyorsa `SignatureInvalid`; dört alan imzasız, yalnız tutarlılık | ✓ | `BrokenHash_…_KK2`, `MissingHash_…_KK2`, `EmptyHash_…_KK2`, `MissingSignedField_…_KK2`, `WithoutTheOneSettingOfKeys_…_KK2`, `TheUnsignedFields_AreNotInTheSignature_…_KK2`; Integration `PayTr_Row1_HashThatDoesNotHold_…_KK2`, `PayTr_Row1_MissingHash_…_KK2` |
| 3 | Yedi sıralı sınıflama | ✓ | Integration `PaymentNotificationTests.PayTr_Row1` … `PayTr_Row7` (Row4 dört kol ayrı test); Unit `ReadNotificationResult_KnowsOnlySaleWithSuccessOrFailed_KK3` |
| 4 | `payment_amount` = Total × 100, `total_amount` ≥; `failed`'de tutar aranmaz | ✓ | `CheckNotificationAmount_PaymentAmountEqual_TotalAmountAtLeast_InKurus_KK4`, `CheckNotificationAmount_InstalmentDifferencePasses_ShortAmountDoesNot_KK4`, `PaymentAmountsNotificationTests`; Integration `PayTr_Row5_Failure_IsProcessedWithoutAnAmountCheck_…_KK6`, `PayTr_Row6_…_KK4`, `PayTr_Row7_SuccessWithTheInstalmentDifference_…_KK1_KK4_KK5` |
| 5 | `merchant_oid` iki alana tiresiz; `status` → `Outcome`, `total_amount` → `Amount`; `Sale` | ✓ | `Mapping_MerchantOidToBothReferences_StatusToOutcome_TotalAmountToAmount_EventTypeSale_KK5` + üç `Mapping_…_KK5`; Integration `PayTr_Row7_…` (kayıt alanları) |
| 6 | `failed_reason_code`/ileti saklanmaz, seyreltilerek kayda | ✓ | `FailedPaymentsReason_IsWrittenThinned_…_KK6`, `FailedPaymentsReasonLine_CarriesNoReferenceNoAmountNoHash_AndTheNoticeHasNoFieldForTheReason_KK6`, `SuccessAndSignatureFailure_WriteNoReasonLine_KK6`; Integration `PayTr_Row5_…_AndItsReasonIsInTheLogOnly_KK6` |
| 7 | Yanıt kararı T311'in ucunda; katman yalnız sınıf verir | ✓ | `OddForms_AreSignatureInvalid_WithoutAnException_KK7`, `TheLayersNotificationMembers_ReturnOnlyTheirClasses_NoHttpAnswer_KK7` |
| 8 | Test modunda gerçek bildirimle DV-1, 2, 4, 9, 35, 36, 42 gözlenir | ✓ (gözlem OWNER_INPUT) | `PAYTR.md` §6.1 ve `08 §4.1` yedi satır tarihli "Bekliyor — OWNER_INPUT" + kod tarafı kanıtı; gerçek PayTR test mağazası ve anahtarı yok, gözlem uydurulmadı — iz D-1038; `VAL-E001`/`E002`/`E005`/`E010`/`E036`/`E037`/`E043` `BEKLEMEDE` kalır |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 örnek bildirimde özet tutar | ✓ | `PayTrNotificationTests.KnownExample_…_KK1` (değer .NET dışında openssl ile hesaplandı) |
| KK2 bozuk ve eksik `hash` ayrı testte | ✓ | `BrokenHash_IsSignatureInvalid_KK2`, `MissingHash_IsSignatureInvalid_KK2`, `EmptyHash_IsSignatureInvalid_KK2`; Integration `PayTr_Row1_HashThatDoesNotHold_…`, `PayTr_Row1_MissingHash_…` |
| KK3 yedi sıranın her biri bir test; 4. sıranın dört kolu ayrı test | ✓ | `PaymentNotificationTests.PayTr.cs`: Row1 (2), Row2, Row3, Row4 × 4 (`UnknownStatus`, `PaymentTypeThatIsNotCard`, `CurrencyThatIsNotTl`, `TestModeOnTheLiveInstallation`), Row5, Row6, Row7 |
| KK4 taksit farkı geçer, eksik tutar geçmez | ✓ | `CheckNotificationAmount_InstalmentDifferencePasses_ShortAmountDoesNot_KK4`; Integration `PayTr_Row6_…_KK4`, `PayTr_Row7_SuccessWithTheInstalmentDifference_…` |
| KK5 üç alan eşlemesi | ✓ | `Mapping_MerchantOidToBothReferences_StatusToOutcome_TotalAmountToAmount_EventTypeSale_KK5` |
| KK6 red kodu kayda seyreltilmiş yazılır | ✓ | `FailedPaymentsReason_IsWrittenThinned_TheFirstLineCarriesCodeAndMessage_TheNextAfterTheWindowTheCount_KK6` (sahte saat, pencere sabitten) |
| KK7 katman 400 ya da 500 üretmez | ✓ | `TheLayersNotificationMembers_ReturnOnlyTheirClasses_NoHttpAnswer_KK7`, `OddForms_…_KK7` |
| KK8 DV-1, 2, 4, 9, 35, 36, 42 satırları | ✓ | `08 §4.1` satır 845–886 ve `PAYTR.md` §6.1: yedi satır tarih 2026-11-10, kanıt yeri ve OWNER_INPUT izi (D-1038) |
| 09 §25: tek işlev | ✓ | İmza `MessageSignature` (T401), tutar `PaymentAmounts` (`FromKurus`, `NotificationMatches`), numara `CardPaymentReference`/`OrderNumber`; ikinci kopya yok |
| 09 §25: sabit belgede (§3.5) ve sınır testi | ✓ | `FailureLogMaxAddresses` 10 000 ve `PaymentFailureReasonLog.Window` 60 sn `09 §3.5` satırlarında; `SignatureFailureLogTests.AtTheCeiling_*` (3 test), KK6 testi pencereyi sabitten türetir |
| 09 §18.2 m.3: kapalı olay listesi | ✓ | `LogEvents.PaymentFailedReasonSampled` ve `09 §18.2` m.3 metni aynı PR'da |
| 09 §19: sahte saat, gerçek saate bağlılık yok | ✓ | `PaymentFailureReasonLog` ve `SignatureFailureLog` `TimeProvider` alır; testler sahte saatle |
| Backlog: hedefi T402 olan kalemler | ✓ | D-108 ✅, D-455 ✅ (yerinde kapatıldı); D-741 T402 payı yazıldı, kalan T403 |
| Sık FAIL listesi | ✓ | Tarama/bekçi istisnası eklenmedi; yeni kaynak taraması yok; şema değişmedi; 10 000 ve 60 sn sayıları `09 §3.5`, `05 §4.5.2` (F3 kapısı, K-1582) ve kodla aynı; Integration TAM koşuldu (yeni kurucu bağımlılığı `PaymentFailureReasonLog` DI'da) |
| Güvenlik taraması | ✓ | Karşılaştırma sabit zamanlı; anahtar/salt istek ya da kayıtta yok; `hash`, referans ve tutar red nedeni satırında yok (KK6 testi); red nedeni yalnız kayıtta, maskeleyiciden geçer; biçimsiz form istisna sızdırmaz (KK7); imzası tutmayan istekte işlem yok, seyreltilmiş güvenlik satırı (Row1); bellekteki pencereler 10 000 ile sınırlı (D-108) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` (Consistency dahil) | 6171/6171 |
| Integration | ✓ | parçalı `--filter-namespace` (Postgres 18, `track_commit_timestamp=on`) | Model+Payment+SignedEndpoints+Logging 1475, Ordering 614, Jobs+Files+Catalog 285, Security 34, Web 597 (1 kararsız: `StartupSettingsCheckTests` `Email:Region` kolu `ObjectDisposedException`; sınıf tekrar 17/17, T402 diff'i o yola dokunmaz), geri kalan 500 — toplam 3505 |
| Contract | — | — | Bu task'ta sözleşme testi yok |

Derleme: `dotnet build -c Release -warnaserror` Unit ve Integration (Web dahil) 0 uyarı; `dotnet format --verify-no-changes` değişen 15 .cs dosyasında temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (T401'in anahtar ayarı kullanılır)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T402-paytr-bildirim`
- **Commit:** `T402: PayTR: bildirim imzası ve alan eşlemesi` (+ rapor commit'i)
- **PR:** #PRNO
- **CI:** dal CI'ı doğrulamada okunur
- **Post-merge run'lar:** *(validator doldurur)*
- **Süre:** yapım üç işçide (iki bölünme: bağlam, süre); devam 2 ~40 dk (Integration B/C, rapor, kayıt, PR)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Bildirimin alanları, imza sırası ve değer yazımı `08 §2.1.5`'teki gibidir | `08 §2.1.5`; gerçek test modu gözlemi DV-1/2/4/9/35/36/42 (OWNER_INPUT) | Bekliyor — gerçek test mağazası gerekli (D-1038) |

## ⚠ Kararlar (öneriyle verildi)

- Eksik imzalı alan ya da anahtar ayarının yokluğu da `SignatureInvalid` (imza kurulamıyor; 1. sıra).
- Çözülemeyen tutar `CheckAmount`'ta uyuşmaz (`09 §14.2` m.3); T311'in tanınmayan değer işaretlemesi değiştirilmedi.
- D-108 taşma satırı (en eski sayılı pencerenin sayısı) başka adresin isteği bağlamında yazılır (K-1582 kabulü).
- `failed_reason_msg` imzasızdır; maskeleyici ve JSON kaçışıyla olduğu gibi yazılır, uzunluk sınırı konmadı (gövde 64 KB tavanı).
- Red nedeni penceresi ürün genelinde tektir (bildirim sağlayıcının sunucularından gelir, IP başına pencere anlamsız); 60 sn genel kayan pencere (K-1570), `09 §3.5`'e satır olarak yazıldı — `08 §2.1.5`'in "seyreltilerek" hükmünün uygulanışı, belge kuralını değiştirmez.
- 4. sıra Integration testleri yasak desen taraması gereği her kolda `Row4Expected` ile iddia kurar.
- Kriter 8 gözlem payı OWNER_INPUT; `VAL-E001`/`E002`/`E005`/`E010`/`E036`/`E037`/`E043` `BEKLEMEDE`.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | DV-1, 2, 4, 9, 35, 36, 42 test modu gözlemi | D-1038 | Gerçek PayTR test mağazası ve anahtarı gerekiyor (OWNER_INPUT) |
| 2 | `PaymentAmounts`'un kalan işlevleri (son sorgu tutarı) | D-741 (→ T403) | Son sorgu T403'ün kapsamı |
