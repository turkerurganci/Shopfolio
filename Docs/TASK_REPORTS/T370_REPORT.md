# T370 — Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Tek kural evi (k1, k2, k8, k9):** `Payment/RefundLineRules.cs` — `WayOf` (para geldiği yoldan döner: kart → kart, havale → IBAN), `IsStartedBySystem` (kartta iptal, çıkarma, gecikme feshi, geç ödeme sistemce onaysız; havale hattının tamamı ve kartta cayma/ayıp/tutar iadesi firmanın panel adımı), `NeedsIbanField` (yalnız havalede, kayıtla IBAN gelmediyse), `ActionsOf` (son kart isteği reddedildiyse yeniden deneme ve havale yolu açık, IBAN girilince ikisi kapanır, `CanTurnToTransfer` açılır; sonuç bekleniyorsa hepsi kapalı). Modül, sipariş işlemleri ve panel bu kuralları `IPaymentModule.RefundActionsOf` üzerinden okur.
- **Kaydın geri ödemesi (k1, k2, k6; D-791, D-818, D-843):** `OrderTransitions.StartRefundOfRecordAsync` kaydın işleminde, sipariş kilidi altında: yalnız parası gelmiş siparişte (Ödendi, Kısmen geri ödendi); tutar T369'un `RefundAmountAsync` (ürün bedeli − kupon payı) + kargo kuralı eklerse kargo ücreti (`IsShippingFeeIncludedAsync`). Kartta `IPaymentModule.StartRecordRefundAsync` geri ödemeyi ve `CardRefund` işini yazar; havalede satır yazılmaz, kayıtla IBAN gelmediyse istek (`RefundIbanRequest`) ve B-14 açılır (aynı kalemde açık istek varsa yeniden kullanılır, tek B-14). Çağıranlar: müşteri iptali, firma kalem iptali, firmanın bütün sipariş iptali, gecikme feshi.
- **Müşteri iptali IBAN'ı (k4; D-790):** havaleyle ödenmiş siparişte müşteri iptali IBAN'ı istekle alır; projenin tek IBAN kuralı (`InvalidIbanAttribute`, biçim + sağlama) — yoksa ya da geçersizse `InvalidInput`. Kartta IBAN alınmaz, alan açılmaz.
- **Yeni sipariş işlemleri (`IOrderTransitions`; k7, k8):** `RetryCardRefundAsync`, `OpenRefundTransferPathAsync` (B-14), `EnterRefundIbanAsync` (kart iadesinin havale yolunda geri ödemenin yolunu havaleye çevirir: `TurnRefundToTransferAsync`), `ChangeRefundIbanAsync` (IBAN dosyada durdukça — işlenmemiş — müşterice düzeltilir), `ReportTransferFailedAsync` (IBAN imha kaydıyla silinir, istek yeniden açılır, B-14; süre durmaz). Hepsi sipariş kilidi + sürüm denetimiyle, `OrderTransitionResult` döner, geçiş yazmaz.
- **IBAN'ın ömrü (k5; Z-44, D-517):** `AfterSalesModule.DeleteUnusedRefundIbansAsync` — havale geri ödemesi işlenince ve düzeltmeyle kayıt geri alınınca, siparişin hiçbir bekleyen geri ödemenin kullanmadığı IBAN'ları imha kaydıyla siler (ortak kullanımda son geri ödemeye kadar ertelenir; istek bağlantılı geri ödeme ve kaydı geri alınmış kalem sayılır). Kart iadesi gidince açık havale yolu isteği karşılanmadan kapanır.
- **İşaretler (k8, k9; D-322):** `IPaymentModule.ReadRefundMarksAsync` — "geri ödeme gerçekleşmedi" ve "geri ödeme sonucu bekleniyor" işaretleri (yalnız kart yolu; havaleye dönünce kalkar).
- **Bu devamda:** öz denetim; gecikme feshinin geri ödeme satırı Integration testleri (kart ve havale/başka kanal); `origin/main` `d3ff0f8b` (T375 #505) birleştirildi; D-898; rapor, changelog parçası.

## Etkilenen Modüller / Dosyalar

- Payment: `PaymentModule.cs`, `RefundLineRules.cs` (yeni), `Contracts/IPaymentModule.cs`, `Contracts/PaymentFacts.cs`
- Ordering: `OrderTransitions.RefundLine.cs` (yeni), `OrderTransitions.CustomerCancellation.cs`, `.FirmCancellation.cs`, `.LateDeliveryTermination.cs`, `.Payment.cs`, `.Correction.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/RefundLineRequests.cs` (yeni), `Contracts/ShipmentTransitionRequests.cs`
- AfterSales: `AfterSalesModule.RefundLine.cs` (yeni), `AfterSalesModule.Cancellations.cs`, `AfterSalesModule.DefectRemedies.cs` (birleştirme düzeltmesi), `Contracts/IAfterSalesModule.cs`, `Contracts/RefundLine.cs` (yeni), `Contracts/ItemRecordWrites.cs`
- Testler: `tests/Shopfolio.UnitTests/Payment/RefundLineRulesTests.cs` (yeni), `tests/Shopfolio.UnitTests/Ordering/ShipmentTransitionContractTests.cs`, `tests/Shopfolio.IntegrationTests/Ordering/StatusCorrectionTests.RefundLine.cs` (yeni), `StatusCorrectionTests.cs`, `LateDeliveryAndFirmCancellationTests.cs`, `ItemRecordsTests.DefectRemedyIban.cs` (yeni, birleştirme düzeltmesi)
- Kayıt: `Docs/DEFERRED_BACKLOG.d/T370.md` (yeni), `DEFERRED_BACKLOG.md` ve öteki parça dosyalarında yerinde kapanış/not, `IMPLEMENTATION_STATUS.md` satırı, `STATUS_CHANGELOG.d/T370.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Geri ödeme ödemenin geldiği yoldan | ✓ | `RefundLineRules.WayOf`, `StartRecordRefundAsync`; Unit `TheRefundGoesTheWayThePaymentCame_KK1`; Integration `TheFirmsCancellation_OnTheCard_StartsTheRefundWithItsJob_AndOpensNoIbanField_KK1_KK2`, `TheLastPhysicalUnitCancelledBeforeTheCarrier_AddsThePaidShippingFee_ToItsRefund_KK1`, `TheTerminationsRefund_OnTheCard_IsStartedByTheSystemWithItsJob_PerRecord_AndOpensNoIbanField_T370_KK1_KK2` |
| 2 | İptalde kart iadesi onaysız, IBAN alanı yok; havale ve caymada firma panelden | ✓ | `IsStartedBySystem`, `NeedsIbanField`; Unit `OnTheCard_TheSystemStartsTheCancellationsRefund_AndAsksForNoIban_KK2`, `TheWithdrawal_AndTheTransferLine_AreTheFirmsStepOnThePanel_KK2`; Integration `…_KK1_KK2` (iş satırı `CardRefund`, istek yok, B-14 yok) |
| 3 | Süre başlangıcı: iptalden; teslimden önce caymada ve hizmette beyandan; teslimden sonra ulaşma ile beyandan geç olanı | ✓ | `AfterSalesPeriodRules.CancellationRefundStanding`, `WithdrawalRefundStartDay` (tek kural evi); Unit `ThePeriod_StartsAtTheCancellation_AtTheDeclarationBeforeTheDelivery_AndAtTheLaterOfArrivalAndDeclarationAfter_KK3` (mal beyandan önce ulaştıysa beyandan) |
| 4 | IBAN biçim + sağlama; işlenene kadar müşterice düzeltme; firma IBAN girmez | ✓ | `InvalidIbanAttribute` (`IbanRule.Allows`) girişte, düzeltmede ve müşteri iptalinde; firma işlemlerinde IBAN parametresi yok; Integration `TheCustomersCancellation_OnTheTransfer_TakesACheckedIban_KK4`, `TheIban_IsEnteredAndCorrected_UntilProcessed_ThenDeletedWithItsDisposalRecord_KK4_KK5` |
| 5 | IBAN son bekleyen geri ödemeyle silinir; her silme imha kaydına | ✓ | `DeleteUnusedRefundIbansAsync`, `IsRefundIbanInUseAsync` + `IsUsedByAPendingTransferRefundAsync` (kaynağı ne olursa olsun bekleyen havale geri ödemesi; `IPaymentModule.FindPendingTransferRefundsAsync`); Integration `…_KK4_KK5`, `TheIban_SharedByAPendingRefund_StaysUntilTheLastOneGoes_KK5`, `TheCorrection_OfTheFirmsCancellation_DeletesTheIbanEnteredForIt_WithItsDisposalRecord_D517`, `ItemRecordsTests.TheIban_OfTheItemsRequest_StaysWhileTheItemsOtherDefectRefundIsPending_AndGoesWithTheLast_KK5`, `ItemRecordsTests.TheIban_OnTheOrdersFile_StaysWhileAnotherItemsDefectRefundIsPending_AndGoesWithTheLast_KK5` |
| 6 | Havalede firma iptalinde IBAN isteği kayıtla düşer, B-14, süre durmaz | ✓ | `StartRefundOfRecordAsync` (`NeedsIbanField` + `OpenRefundIbanRequestAsync`); Integration `TheFirmsCancellation_OnTheTransfer_OpensTheIbanRequest_WithB14_AndWritesNoRefundYet_KK6` (ikinci olayda tek B-14), `TheTerminationsRefund_OnTheTransfer_FromANoticeWithoutAnIban_OpensTheIbanRequest_WithB14_AndWritesNoRefundYet_T370_KK6`; süre kayıttan okunur, IBAN'ı beklemez (`CancellationRefundStanding`) |
| 7 | "Havale gerçekleşmedi" IBAN'ı siler, alanı açar, B-14; kalem "IBAN bekleniyor"a döner, süre durmaz | ✓ | `ReportTransferFailedAsync` (Ordering + AfterSales); Integration `TransferFailed_DeletesTheIban_OpensTheFieldAgain_AndSendsB14_KK7` |
| 8 | Kart reddi: eksen sabit, işaret, bekleyen iş; yeniden deneme ya da havale yolu (B-14); IBAN'dan sonra deneme kapalı | ✓ | `ActionsOf`, `RetryCardRefundAsync`, `OpenRefundTransferPathAsync`, `EnterRefundIbanAsync`, `ReadRefundMarksAsync`; Unit `AFailedCardRefund_OpensTheRetryAndTheTransferPath_UntilAnIbanIsEntered_KK8`; Integration `AFailedCardRefund_IsRetried_OrTurnedToTheTransfer_AndAfterTheIbanTheRetryIsClosed_KK8` |
| 9 | Sonuç bilinmezse "sonuç bekleniyor"; deneme ve havale yolu kapalı | ✓ | `ActionsOf`; Unit `APendingResult_ClosesTheRetryAndTheTransferPath_KK9`, `ASentOrVoidedRefund_OrOneOnTheTransferWay_OpensNothing`; Integration `APendingCardResult_ClosesTheRetryAndTheTransferPath_KK9` |

**Doğrulama kontrol listesi:** KK1 ✓ (k1 satırı) · KK2 ✓ (k2) · KK3 ✓ (k3; mal beyandan önce ulaştıysa beyandan) · KK4 ✓ (k4) · KK5 ✓ (k5; ortak kullanımda erteleme) · KK6 ✓ (k6) · KK7 ✓ (k7) · KK8 ✓ (k8) · KK9 ✓ (k9).

### `09 §25` ilgili satırlar

| Soru | Cevap | Kanıt |
|---|---|---|
| Yeni adlar 06/07'den birebir | Evet | `Refund`, `RefundIbanRequest`, `RefundIban`, `RefundIbanRequestReason`, `JobType.CardRefund` var olan adlar; yeni enum/değer yok |
| Diff yalnız görevin istediği | Evet | Ekran payı yok (D-875, D-873 backlog'da) |
| Yeni ayar/sabit §3.5'te | Uygulanamaz | Yeni ayar, süre ya da tavan yok |
| Modül sınırı (yalnız `Contracts`, arayüzde `SaveChanges` yok) | Evet | Diff'te `SaveChanges`/`BeginTransaction` yok; Ordering → `IPaymentModule`/`IAfterSalesModule` sözleşmeleri; mimari testler Unit içinde yeşil |
| Tek işlev | Evet | IBAN `InvalidIbanAttribute`; süre `AfterSalesPeriodRules`; tutar `RefundAmountAsync`/`IsShippingFeeIncludedAsync` (T369); yol/işlemler `RefundLineRules` |
| Zaman `TimeProvider`'dan | Evet | `_time.GetUtcNow()`; diff'te `DateTime.Now/UtcNow` yok |
| İstisna/`catch` | Evet | Diff'te `catch` yok; ret `TransitionRefusal` ile |
| Durum değişikliği yalnız geçiş birimiyle | Evet | Yeni işlemler geçiş yazmaz; ödeme ekseni değişimi var olan `ApplyRefundSentAsync` |
| Dış yan etki aynı işlemde `Job` | Evet | Kart iadesi `CardRefund` işi kaydın işleminde (`…_KK1_KK2`) |
| Yeni işlem `AuditOperation`'da, iz aynı işlemde | Hayır → backlog | D-646/D-774 emsali: Ordering modülü iz satırı yazmaz, panel/sayfa işlemi yazar → **D-898** (T527) |
| Eşzamanlılık | Evet | Her işlem `LockAndLoadAsync` (sipariş satır kilidi); yönetici işlemleri sürüm denetimli (`LockForAdminAsync`) |
| Para hesabı | Evet | Toplama yalnız kayıt tutarı + kargo ücreti; yuvarlama yok (`Money` dışında `Math.Round` yok) |
| Göç / tablo / değer | Uygulanamaz | Şema değişikliği yok |
| Sır ve kişisel veri | Evet | IBAN kayda yazılmaz; silme imha kaydıyla |
| Yetkilendirme sunucu tarafı | Evet | Yönetici işlemleri `TryAdminActor`; müşteri işlemleri isteğin siparişe aitliğini denetler (`IsOrdersAsync`) |
| Test katmanı | Evet | Kural Unit, veri davranışı gerçek PostgreSQL (Integration); sahte saat |
| Performans (döngüde sorgu) | Evet (sınırlı) | `DeleteUnusedRefundIbansAsync` siparişin IBAN'ları üzerinde döner (sipariş başına birkaç satır); büyüyen küme yok |
| PR tek amaçlı, CI | Evet / bekliyor | CI sonucu doğrulamada okunur |

**Güvenlik taraması:** yeni uç, sayfa, POST, dosya ucu ya da dış çağrı yok; IBAN yalnız müşterinin kendi siparişine (istek siparişin kalemine/geri ödemesine bağlı olmalı) girilir; geçersiz IBAN `InvalidInput`; IBAN değeri iz/kayıt satırına inmez. Bulgu yok.

### Sık FAIL nedenleri öz denetimi

- Tarama/bekçi istisnası, kaynak taraması: uygulanamaz (yeni tarama yok).
- 06 "Varsayılan" / FK dizini: uygulanamaz (göç yok).
- Aynı sayıyı anan öteki yerler: doküman kuralı değişmedi.
- 09 metni: değişmedi.
- Ardışık yazma: aynı kalemde ikinci firma iptali tek istek + tek B-14 (`…_KK6`); IBAN ikinci kez düzeltme `…_KK4_KK5`.
- Devredilen backlog kalemleri: D-322, D-517, D-790, D-791, D-817, D-818, D-843 kapandı (D-817'nin kalanı D-873); D-526, D-583, D-666, D-667, D-696'nın T370 payı D-874–877'ye taşındı.
- Main birleştirmesi: `origin/main` `d3ff0f8b` (T375) birleştirildi, birleşik ağaçta Unit + Integration TAM koşuldu.

## Birleştirme ve düzeltme (2026-11-09, tur 1 PASS sonrası)

- **Birleştirme:** `origin/main` `64d0e44a` (T374 #504, T383 #503) dala alındı (rebase yok). Çakışmalar yalnız ekleme: `IAfterSalesModule.cs` (T374'ün dört ayıp üyesi + T383'ün üç okuması, T370 üyeleri yanında), `PaymentFacts.cs` (T374'ün `DefectRemedyRefundStart`, `DefectRescissionShares` + T370'in `RefundOfRecord`, `CardRefundActions`), `DEFERRED_BACKLOG.d/T369.md` (D-842 main'den — T374 kapatması; D-843 daldan — T370 kapatması). Göç yok; ortak kayıt satırları main sürümünde.
- **Davranış düzeltmesi (yardımcının sahip yerine kararı; `02 §7.4.5`):** birleşik kodda `IsRefundIbanInUseAsync` T374'ün bekleyen ayıp geri ödemelerini (dönme, bedel indirimi) saymıyordu: kalemin iptal/çıkarma/fesih/cayma kaydı olmadığı için kalem isteği atlanıyor, biri işlenince IBAN siliniyor, öteki IBAN'sız kalıyordu. Kural tek yerde: `IsUsedByAPendingTransferRefundAsync` — siparişin işlenmemiş ve geçersizleşmemiş, havale yolundaki her geri ödemesi (kaynağı ne olursa olsun) ödeneceği IBAN'ı kullanımda tutar; ödeneceği IBAN kaydının IBAN'ı, yoksa kendi isteğinin, sonra kaleminin isteğinin IBAN'ı, hiçbiri yoksa dosyadaki IBAN (siparişin her IBAN'ı). Okuma `IPaymentModule.FindPendingTransferRefundsAsync` (açık işlemin değişiklikleri dahil).
- **Tek yol:** T374'ün `RequestDefectRemedyIbanAsync`'i kalem isteğini artık T370'in tek istek yazıcısıyla (`OpenRefundIbanRequestAsync`) açar; dosyada IBAN varsa istek yok kuralı korunur (`Remedies_OnTheTransferLine…_KK8` testleri yeşil).
- **T374 kalem tavanı:** `CommittedItemRefundTotalAsync` kalemin geçersizleşmemiş bütün `Refund` satırlarını toplar; T370'in kayıt geri ödemeleri `OrderItemId` ile yazılır (`StartRecordRefundAsync`), sayılır. Havalede kaydın satırı firmanın adımıyla doğduğu için o tutar tavana henüz girmez → **D-906** (T527).
- **Yeni backlog:** D-906 (yukarıda), D-907 — ayıp geri ödemesinin "havale gerçekleşmedi"si dosyadaki IBAN'ı bulamıyor (`ReportTransferFailedAsync` yalnız kalemin kayıt ve isteklerine bakar) → T373.
- **Öz denetim:** kriter 1–9 ve KK1–KK9 birleşik ağaçta yeniden denetlendi (yukarıdaki tablo; kanıt testleri birleşik ağaçta yeşil). Sık FAIL nedenleri: birleştirmede iki yol (istek yazıcısı tek yola indirildi); yeni kapanış türünün okuyuculara yayılması (IBAN ömrü T374 geri ödemelerini okur; "gerçekleşmedi" payı D-907); Integration TAM koşuldu; `dotnet format` üç proje.

## Test Sonuçları

Birleşik ağaç (bu dal + `origin/main` `64d0e44a`, düzeltmeden sonra):

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build -warnaserror` | 0 uyarı, 0 hata |
| Lint | ✓ | `dotnet format <proje> --verify-no-changes` (3 proje) | temiz |
| Unit (Consistency dahil) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 5736/5736 |
| Integration Ordering + Payment | ✓ | `--filter-namespace …Ordering --filter-namespace …Payment` | 571/571 (yeni 2 IBAN testi dahil) |
| Integration Model | ✓ | `--filter-namespace …Model` | 1404/1404 |
| Integration kalan ad alanları | ✓ | `--filter-not-namespace` Ordering/Payment/Model | 1395/1395 |

Önceki birleşik ağaç (bu dal + `origin/main` `d3ff0f8b`):

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx` | 0 uyarı, 0 hata |
| Unit (Consistency dahil) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 5717/5717 |
| Integration Ordering + Payment | ✓ | `--filter-namespace …Ordering --filter-namespace …Payment` | 554/554 (yeni 2 gecikme feshi testi dahil) |
| Integration Model | ✓ | `--filter-namespace …Model --max-threads 3` | 1404/1404 (ilk koşuda 1 kararsız hata, yeniden koşuda yeşil) |
| Integration kalan ad alanları | ✓ | `--filter-not-namespace` Ordering/Payment/Model | 1387/1387 |
| Lint | ✓ | `dotnet format … --verify-no-changes` (değişen dosyalar; önceki turda 3 proje) | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS — tur 2 (2026-11-09, head `fba7f20f`; `origin/main` `64d0e44a` (T374 #504, T383 #503) dalın içinde, main ilerlememiş — birleşik ağaç dalın kendisi; kapsam TAM). Kriter 1–9 ve KK1–KK9 birleşik ağaçta yeniden ✓ (kod okuması `RefundLineRules.cs`, `OrderTransitions.RefundLine.cs`, `AfterSalesModule.RefundLine.cs` — `IsRefundIbanInUseAsync` + yeni `IsUsedByAPendingTransferRefundAsync`, `PaymentModule.FindPendingTransferRefundsAsync`, T374'ün `RequestDefectRemedyIbanAsync`'inin tek istek yazıcısına bağlanması; Unit `RefundLineRulesTests`, Integration `StatusCorrectionTests.RefundLine`, `LateDeliveryAndFirmCancellationTests` T370 testleri ve yeni `ItemRecordsTests.DefectRemedyIban` iki KK5 testi yeşil). T374 etkileşimi: dönme ve bedel indirimi havale yolunda bekledikçe IBAN — kalem isteğinden ya da dosyadan — kalır, sonuncunun işlenmesiyle imha kaydıyla silinir; kart hattında ayıp geri ödemesi IBAN tutmaz. D-906 (havale kaydının doğmamış satırı tavana girmiyor → T527) ve D-907 (dosyadaki IBAN'la ödenen ayıp geri ödemesinin "havale gerçekleşmedi"si → T373) kabul edilebilir erteleme: ikisi de T374 birleşmesinin yeni kesişimi, T370 kriterlerinin kendi yolları (kayıt geri ödemesi, kalem/geri ödeme isteği) karşılanmış, hedef task'lar açık ve T370'e bağımlı ya da onun ardında. Yerel: derleme `-warnaserror` 0/0, format 3 proje temiz, Unit 5736/5736 (Consistency, Architecture dahil), betik 4/4, Integration Ordering 557/557, Payment+Disposal+Jobs+Model+Concurrency 1582/1582, `restore --locked-mode` 0. **Dal CI:** head run 42686592582 ✓ (9 iş yeşil, Guard skip). Ana dal son 3 run ✓ (42680032675, 42680031920, 42677418964). |
| Bulgu sayısı (tur 2) | 0 |
| Bloke-etmeyen not (tur 2) | 1 — K1: firmanın yeniden denemesi (`RetryCardRefundAsync`) yalnız `CardRefund` iş satırı yazar, kayıt yazmaz; iş işleyicisi "gönderiliyor" kaydını yazana kadar son kayıt `Failed` kalır, bu arada müşterinin IBAN'ı (`EnterRefundIbanAsync` → `CanTurnToTransfer`) geri ödemeyi havaleye çevirir ve sıradaki iş kart isteğini yine gönderebilir — aynı para iki yoldan (`02 §7.2.8`). Gönderen işleyici yok (T405), bugün para çıkmıyor → **D-908** (T405). |
| Doğrulama durumu (tur 1) | ✓ PASS — tur 1 (2026-11-09, head `6a9ed6b1`; `origin/main` `d3ff0f8b` dalın içinde, main ilerlememiş — birleşik ağaç dalın kendisi). Kriter 1–9 ve KK1–KK9 ✓ (validator kanıtı: kod okuması `RefundLineRules.cs`, `OrderTransitions.RefundLine.cs`, `AfterSalesModule.RefundLine.cs`, `PaymentModule.StartRecordRefundAsync`/`ReadRefundMarksAsync`, `AfterSalesPeriodRules.CancellationRefundStanding`/`WithdrawalRefundStartDay`; Unit `RefundLineRulesTests` ve Integration `StatusCorrectionTests.RefundLine`, `LateDeliveryAndFirmCancellationTests` T370 testleri yeşil). Validator yoklaması (geçici test, commit edilmedi): müşteri iptali kartta sistem geri ödemesini ve `CardRefund` işini açar, IBAN almaz ve istek açmaz (KK2'nin müşteri yarısı); başka siparişin isteğine IBAN girişi `ConditionNotMet`. Yerel: derleme `-warnaserror` 0/0, format 3 proje temiz, Unit 5717/5717 (Consistency dahil), betik 4/4, Integration Ordering 540/540, Payment+Disposal+Jobs+Model 1556/1556, `restore --locked-mode` 0. **Dal CI:** head run 42672074277 `failure` ama hiçbir iş koşmadı (yalnız "6. Migration dry-run" `skipped` kaydı var; aynı dakikada T374 run 42671989182 de işsiz `failure`) — kod kaynaklı değil, altyapı; önceki run 42672002917 yeni push'la `cancelled`. Kayıt commit'inin push'u yeni run tetikler; merge'den önce dal CI'ının yeşili yardımcıca okunur. |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1: havale hattında çok kalemli siparişin bütün sipariş firma iptali kalem başına ayrı B-14 gönderir (yoklama: 2 kalem → 2 B-14; `02 §9.2` olayı kalem başına, sapma değil) → **D-902** (T528). K2: `DeleteUnusedRefundIbansAsync` IBAN başına döngüde sorgu yapar (sipariş başına sınırlı küme; `09 §20`) → **D-903** (T373, D-876 ile). |

**Task satırları (`12 §7.5`):** KP-15, KP-21, KP-22, KP-47, KP-74 grep'i 98 matris satırı getirir; T370'in konusu `VAL-A067`, `A068`, `A069` (geri ödeme yarısı), `A078`, `A079`, `A210`, `A211`, `A212`, `A213`, `A216`, `A217`, `A230` ve `D049`'un IBAN silme yarısı. Hepsi uç yanıtı, tarayıcı gözlemi ya da uygulama kaydı (sağlayıcıya giden istek — işleyici T405) ister; uçlar ve ekranlar T512/T527/T528'de → **BEKLEMEDE** kaldı (T367–T369, T372, T375 emsali). `A214`, `A215` (sorgu sonucu ve firmanın "gerçekleşmedi" işareti) T405/T527'nin. Öteki satırlar T367–T369, T371–T373'ün. `12_matris_denetle.py --kapsam 1-77` Hata 0.

**Yapım raporu karşılaştırması:** tam uyumlu; öz denetim tablosu kriter ve KK başına kanıtlı. Tek fark: KK2'nin müşteri iptali yarısı için raporda doğrudan test yok (kanıt firma iptali ve fesih testleri) — validator yoklamasıyla doğrulandı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (Ordering kurucusu zaten `IPaymentModule` ve `IAfterSalesModule` alıyordu)

## Commit & PR

- **Branch:** `task/T370-geri-odeme-hatti`
- **Commit:** önceki işçilerin ara kayıtları, `51ae72f3` (gecikme feshi testleri), `origin/main` birleştirmesi, rapor commit'i
- **PR:** #506
- **CI:** head `6a9ed6b1` run 42672074277 işsiz `failure` (altyapı; ayrıntı Doğrulama'da); kayıt commit'inin run'ı merge'den önce okunur
- **Post-merge run'lar:**

## Dış Varsayımlar

Dış varsayım: yok (kart iadesi sağlayıcı çağrısı var olan `CardRefund` işinin işleyicisindedir; bu task yalnız satırı ve işi yazar).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Müşterinin gecikme feshi formu IBAN'ı istekle almıyor | D-873 | Fesih ekranı/E-17 T512'de |
| 2 | Kalemsiz "IBAN bekleniyor" işaretinin sipariş satırı okuması | D-874 | Panel listesi T527 |
| 3 | Ret ve bekleme işaretinin satırda gösterimi | D-875 | Ekran T527, T382 |
| 4 | Z-38 kaynağının bekleyen geri ödeme konuları | D-876 | Zamanlanmış silme T373 |
| 5 | "Mal ulaşmadı" hâlinde IBAN silme günü | D-877 | T373 |
| 6 | Beş yeni geri ödeme işleminin işlem izi satırı | D-898 | D-646/D-774 emsali: iz panel/sayfa işleminde (T527) |
| 7 | Havalede kaydın doğmamış geri ödemesi tutar tavanlarına girmiyor | D-906 | Satır firmanın panel adımıyla doğar (T527) |
| 8 | Ayıp geri ödemesinin "havale gerçekleşmedi"si dosyadaki IBAN'ı bulamıyor | D-907 | "Gerçekleşmedi" IBAN bulma kuralı ömür kuralıyla birleştirilecek (T373) |

## Notlar

- **Working tree (Adım -1):** temiz (devam 3 işçisi; dal `origin/task/T370-geri-odeme-hatti` `91ffdfe5`'ten alındı).
- **Repo memory:** dokunulmadı.
- **⚠ kararlar (kurala en uygun seçenekle):**
  1. Müşteri iptalinde havale + ödenmiş siparişte IBAN zorunlu (yoksa/geçersizse `InvalidInput`) — D-790'ın "iptal ekranı IBAN'ı istekle alır" seçeneği.
  2. Gecikme feshinin başka kanal kaydında IBAN isteğinin nedeni `NoticeWithoutIban`.
  3. Yeni geri ödeme işlemleri `OrderTransitionResult` döner, geçiş yazmaz.
  4. Düzeltmeyle kaydı geri alınmış kalemin isteği IBAN'ı tutmaz (IBAN imha kaydıyla silinir).
  5. Backlog hedefleri: D-874/D-875 T527, D-876/D-877 T373.
  6. İşlem izi (yardımcının sahip yerine kararı): beş işlemin iz satırı kod yazılmadan D-898'e (T527).
