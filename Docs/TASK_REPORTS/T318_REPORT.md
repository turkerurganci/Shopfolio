# T318 — İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

Yapım iki işçide yürüdü: ilk işçi bağlam sınırında bölündü (kod yama olarak devredildi), ikinci işçi yamayı `origin/main` (`bcb856dc`) üstüne uyguladı, testleri yazdı, bir hatayı düzeltti ve belgeleri tamamladı.

- **Birleşim tablosu kodda (k1–k3).** `TransitionTable.Combinations`, `03 §1.4.1`'in 30 hücresini hücre hücre taşır; `IsPairAllowed` onu okur. Geçiş, düzeltme ve çift kuralı (`OrderTransitions.cs:117`, `:248`, `:341`) ulaşılamaz çifti yazmadan reddeder. Belgeyle eşitlik `CombinationTableConsistencyTests`'te (belgeden okunur), veritabanı kuralıyla eşitlik T315'in `IsPairAllowed_EqualsTheCombinationRuleOfTheDatabase_ForAllPairs` testinde.
- **Ö1'in eşzamanlı geçişleri (k4).** `CompleteApprovalAsync` (`OrderTransitions.Payment.cs:221`) kartta ve havalede Ö1 ile aynı işlemde: ayrılan stok ve kupon hakkı kesin düşer (1.4.4.2), açık dijital kalemler teslim işaretini ve teslim tarihini alır, fiziksel açık kalemi olmayan siparişte kapanış kuralı işler (yalnız dijitalde S2; havalede S2'nin aktörü yönetici — 1.5.3).
- **S1 yalnız Ö1 ile (k3).** `StartPreparationAsync` kaldırıldı (D-307): fiziksel kalemli siparişte S1, Ö1 ile ödeme ekseninin yönteminde yazılır; Alındı + Ödendi'de fiziksel açık kalem olamaz (`03 §1.4.1` (b)).
- **Ödenmemiş iptal (k4, k8).** S3 + Ö2 aynı işlemde ayırmayı serbest bırakır (`OrderTransitions.Shipment.cs:52`, 1.4.4.3).
- **Havale düzeltmesi (k3, k4, k8).** Ö1 düzeltmesi yalnız havalede: Hazırlanıyor'da S1 de aynı işlemde düzeltilir (iki düzeltme kaydı), ayırma geri yazılır (1.4.4.8, `OrderTransitions.cs:206–274`). Kart Ö1'i ve tek başına S1 düzeltilmez; Teslim edildi'de Ö1 düzeltmesi çift kuralına takılır (k2).
- **An tablosu (k8).** `CounterMomentTable` (9 satır, 03 §1.4.4) ve an başına yöntem taşıyan `OrderCounters`: Reserve, Commit, Release, Return (kupon dönüş kuralıyla: yalnız siparişin bütün adetlerini son hâline getiren kayıtta ve bir kez; ret ya da "mal dönmedi" varsa hiç), Retake (yetmezse false; kupon limit aşsa da yeniden kullanılmış), Restore. Katalogda `Retake` ve `Restore` hareketleri; silinmiş varyant atlanır; varyant silinince ayırma satırı basamaklanır (1.4.4.9). `AfterSales.UnitFactsAsync` kupon kuralının adet olgularını verir.
- **Hatlar ve bütçe (k9, k10).** `OrderLines`: 7 hat (03 §1.5) + `LineOf` + `StepsOf` (zorunlu elle adım; "ödendi" işareti siparişte bir kez). Belgeyle eşitlik `OrderLinesConsistencyTests`'te (dizi, aktör, adım sayıları, 8.5.5'in dört adımı, 8.5'in tavanı — hepsi belgeden okunur).
- **Düzeltilen hata (bu tur):** Ö1'de dijital kaleme yalnız `DeliveryMarkedAt` yazılıyordu; veritabanının `delivery_mark_and_date_together` kuralı onayı düşürüyordu. Teslim tarihi (onay gününün Türkiye tarihi) birlikte yazılır (`OrderTransitions.Payment.cs:226–231`).

## Etkilenen Modüller / Dosyalar

- Ordering: `TransitionTable.cs`, `OrderTransitions.cs`, `OrderTransitions.Payment.cs`, `OrderTransitions.Shipment.cs`, `OrderingModule.cs`, `Contracts/IOrderTransitions.cs`; yeni `CounterMoments.cs`, `OrderCounters.cs`, `OrderLines.cs`
- Catalog: `StockCounters.cs`, `CounterMove.cs`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs`
- AfterSales: `AfterSalesModule.cs`, `Contracts/IAfterSalesModule.cs`, yeni `Contracts/ItemUnitFacts.cs`
- Testler: yeni `UnitTests/Consistency/CombinationTableConsistencyTests.cs`, `CounterMomentTableConsistencyTests.cs`, `OrderLinesConsistencyTests.cs`, `IntegrationTests/Ordering/AxesTieTests.cs`; değişen `PaymentTransitionsTests.cs`, `ShipmentTransitionsTests.cs`, `ShipmentTransitionContractTests.cs`, `SitemapSourceTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Birleşim tablosu kodda; ulaşılamaz çifti üreten geçiş reddedilir | ✓ | `TransitionTable.cs:113`, `:127`; `CombinationTableConsistencyTests.Combinations_EqualTheDocumentsTable_CellByCell_KK1`; T315 `IsPairAllowed_EqualsTheCombinationRuleOfTheDatabase_ForAllPairs` |
| 2 | Teslim edildi + Bekliyor/Başarısız yasak | ✓ | `Delivered_WithPendingOrFailed_IsForbidden_KK2`; `AxesTieTests.TheTransferMarksCorrection_OfADeliveredOrder_IsRefusedByThePairRule_KK2` |
| 3 | Hazırlanıyor'a yalnız ödeme onayıyla; Bekliyor'a yalnız havale düzeltmesiyle | ✓ | `APendingPayment_StandsOnlyWithPlaced_KK3`; `StartPreparationAsync` yok (`ShipmentTransitionContractTests`); `OrderTransitions.cs:206`, `:221`; `TheCardApprovalsCorrection_IsRefused_…_KK3`; `TheTransferMarksCorrection_OnPreparing_…_KK3_KK8` |
| 4 | Eşzamanlı geçişler tek işlemde | ✓ | `CompleteApprovalAsync` (`OrderTransitions.Payment.cs:221`); `ADigitalOrdersCardPayment_WritesO1S2AndTheDeliveryMark_InOneOperation_KK4`; `AMixedOrdersCardPayment_…_KK4_KK8`; `TheUnpaidCancellation_…_KK4_KK8`; havale düzeltmesi iki kayıt; T316 `TheLastEventThatCompletesTheClosure_…_KK4` |
| 5 | Kapanış: teslim varsa Teslim edildi, yoksa İptal edildi | ✓ | `EvaluateClosureAsync` (`OrderTransitions.cs:291`); T315 `Closure_WhenTheLastOpenItemIsCancelled_…_KK7`; T316 `…_KK4`; `ADeliveryFailedOrderWithADeliveredItem_…_KK6_KK7`; `TheClosure_ByATermination_…_KK7` |
| 6 | Teslimsiz Kargoya verildi/Teslim edilemedi bekler; yanıtsız son sorguda iptal ertelenir | ✓ | `AnOrderWithNoDeliveredItem_OnShippedOrDeliveryFailed_WaitsForTheShipmentsReturn_…_KK5_KK6`; T316 `S9_AsTheClosing_AfterS7_…_KK6`; `OutageRulesTests` (`FinalPaymentQueryRule.Decide(NoResponse) = AskAgain`), `FinalPaymentQueryOutageTests` |
| 7 | Kalem adetle açık; kapanışta iptal kaydı ve ikinci geri ödeme yok | ✓ | `ADeliveryFailedOrderWithADeliveredItem_…` (2 adetten 1'i kapanınca bekler); `TheClosure_ByATermination_OpensNoCancellationRecordAndNoSecondRefund_KK7` |
| 8 | Sayaç etkisi an tablosuyla uyar | ✓ | `CounterMoments.cs:100`; `CounterMomentTableConsistencyTests.Rows_EqualTheDocumentsTable_BothWays_KK8`; dokuz anın davranışı `AxesTieTests` `…_KK8` testleri (aşağıdaki tablo) |
| 9 | Yedi hattın dizisi ve adım sayısı; karışıkta toplam, "ödendi" bir kez | ✓ | `OrderLines.cs:65`, `:115`; `OrderLinesConsistencyTests` `…_KK9` (dizi, aktör, 1.5 ve 8.5 sayıları, 8.5.5 dört adım) |
| 10 | Tek tipli hatta zorunlu elle adım üçü geçmez | ✓ | `ASingleTypeLine_NeverExceedsTheCeiling_AndTheHeaviestIsThePhysicalOrderByTransfer_KK10` (tavan 8.5'ten okunur) |

### Öz denetim (bitiş kapısı 2)

Bütün maddeler (ilk işçinin yaptıkları dahil) bu turda diff'te `grep -n` ile yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: yasak çiftlerin tamamı reddedilir | ✓ | 30 hücre belgeden: `Combinations_EqualTheDocumentsTable_CellByCell_KK1`, `TheDocumentsTable_CoversEveryPairOfTheTwoAxes`; ret yolu `OrderTransitions.cs:117/248/341` |
| KK2: teslim edilmiş sipariş ödeme Bekliyor'a düşmez | ✓ | `TheTransferMarksCorrection_OfADeliveredOrder_IsRefusedByThePairRule_KK2` (ForbiddenStatePair, kayıt yok) |
| KK3: ödemesiz sipariş Hazırlanıyor'a geçmez | ✓ | `APendingPayment_StandsOnlyWithPlaced_KK3`; S1'in tek yolu Ö1 (`TheShipmentAxis_HasAnOperationForS3S4S9AndS5ToS8_AndNoneForS1OrTheClosures_KK4`) |
| KK4: Ö1 + S1 + dijital teslim işareti birlikte; son kalem kapanışla aynı işlemde | ✓ | `AMixedOrdersCardPayment_WritesO1S1_MarksOnlyTheDigitalItem_…`; `ADigitalOrdersCardPayment_…_InOneOperation_KK4`; `ADigitalOrdersTransferMark_ClosesWithS2_ByTheAdministrator_KK4`; `ADigitalAndServiceOrdersCardPayment_…_KK4`; T316 `TheLastEventThatCompletesTheClosure_…_KK4` |
| KK5: üç kapanış sonucu ayrı | ✓ | Teslim edildi: `ADeliveryFailedOrderWithADeliveredItem_…` (S11), T316 `…_KK4` (S2/S10); İptal edildi: `TheClosure_ByATermination_…` (S4), T315 `Closure_WhenTheLastOpenItemIsCancelled_…`; bekler: `AnOrderWithNoDeliveredItem_OnShippedOrDeliveryFailed_…_KK5_KK6` |
| KK6: teslimli Teslim edilemedi siparişi son açık kalem kapanınca (S7 dahil) S11, açıkken bekler; yanıtsız sorguda iptal ertelenir | ✓ | `ADeliveryFailedOrderWithADeliveredItem_…_KK6_KK7`; T316 `S7_OfAShipmentWhoseItemsAreClosedAndOthersDelivered_EndsInS11_…_KK4`; `OutageRulesTests:60` |
| KK7: kapanışta ikinci iade satırı yok | ✓ | `TheClosure_ByATermination_OpensNoCancellationRecordAndNoSecondRefund_KK7` (`cancellations`, `refunds` 0) |
| KK8: dokuz an tabloyla aynı | ✓ | 1: `AMixedOrdersCardPayment_…` (ayırma), `TheReservation_WhenTheStockIsShort_IsFalse_AndWritesNoRow_KK8`; 2: `AMixedOrdersCardPayment_…`; 3: `TheUnpaidCancellation_…`; 4: `TheReturn_GivesTheUnitsBack_AndTheCouponOnlyWithTheOrdersLastUnit_AndOnlyOnce_KK8`; 5: aynı test (fesih); 6: `APhysicalWithdrawal_GivesTheCouponBackOnlyWithTheReceipt_AndNeverAfterARejection_KK8`, `AServiceWithdrawal_GivesTheCapacityBack_…_KK8`; 7: `TheRetake_…_KK8` (iki hâl); 8: `TheTransferMarksCorrection_OnPreparing_…_KK3_KK8`; 9: `TheVariantsDeletion_DropsItsReservation_AndTheCorrectionsMovementsSkipIt_KK8`; tablo eşitliği `CounterMomentTableConsistencyTests` |
| KK9: yedi hattın adım sayısı; havale fiziksel + hizmet dört | ✓ | `TheOrdinarySequence_EqualsTheDocument_KK9`, `ThePhysicalLines_ManualSteps_…_KK9`, `TheLinesWithoutAPhysicalItem_…_KK9`, `TheBudgetRows_EqualStepsOf_KK9`, `AMixedOrder_…_PhysicalAndServiceByTransferIsFour_KK9` |
| KK10: en ağır hat üç | ✓ | `ASingleTypeLine_NeverExceedsTheCeiling_…_KK10` |
| `09 §25` — durum değişikliği yalnız geçiş birimiyle; tablo `02 §5.4/§5.5` ile eşit | ✓ | yeni geçiş yok; S1 kaldırılan yöntem dışında tablo değişmedi; `TransitionTableTests` yeşil |
| `09 §25` — iç yan etki aynı işlemde, ret geri alıyor | ✓ | sayaçlar `TransactionRunner` işleminde (`StockCounters` işlem dışı yazımı reddeder); ret `IsRejected` ile geri alınır |
| `09 §25` — eşzamanlılık: kilit | ✓ | Ö1, iptal ve düzeltme sipariş kilidi altında (`ApplyChosenAsync`, `CorrectAsync` `_locks.LockAsync`); katalog satırları `LockSequence` + koşullu `ExecuteUpdate` |
| `09 §25` — modül sınırı: yalnız `Contracts`; arayüz yönteminde `SaveChanges` yok | ✓ | `OrderCounters` → `ICatalogModule`, `IAfterSalesModule`; `UnitFactsAsync` yalnız okur; mimari testler yeşil (Unit 4289/4289) |
| `09 §25` — tek işlev | ✓ | kapanış tek yolda (`EvaluateClosureAsync`); S1'in ikinci yolu (`StartPreparationAsync`) kaldırıldı; sayaç hareketleri tek yazar `StockCounters` |
| `09 §25` — zaman yalnız `TimeProvider`'dan, tarih kurala uygun | ✓ | `_time.GetUtcNow()`; teslim tarihi `TurkeyTime.DateOf` |
| `09 §25` — testin beklenen değeri belgeden/sabitten | ✓ | üç tutarlılık testi beklenenleri `03`'ten okur; elle yazılan yalnız sözcük eşlemesi |
| `09 §25` — yeni ayar/sabit, göç, paket | ✓ | yok (göç yok, `.env` yok, paket yok) |
| Güvenlik taraması | ✓ | sır, girdi yüzeyi, yetki değişikliği yok; yeni dış bağımlılık yok; düzeltme yalnız panel aktörüyle (mevcut kontrol) |
| `12` doğrulama protokolü | ✓ | T318 etiketli satır yok; ilgili satırlar: `VAL-C079`/`C080` (Ö1 + S1/S2 tek işlem; KK4 testleri), `VAL-C083`…`C086` (kapanış; KK5–KK7 testleri), `VAL-C087` (ödenmemiş iptal ayırmayı bırakır; tek yol `ChooseCancellation`), `VAL-C088` (yanıtsız sorgu; `OutageRulesTests`, `FinalPaymentQueryOutageTests`), `VAL-C098`/`C099` (KK3/KK2), `VAL-B023`…`B026` (`StepsOf`; uçtan uca elle adım ölçümü panel işlemleriyle — D-454) |
| Sık FAIL listesi | ✓ | geniş bekçi/tarama yok; DB default/FK yok (göç yok); aynı sayıyı anan öteki yerler: 03 §1.5/§8.5 sayıları tek evde (`OrderLines`) ve testler belgeden okur; tek kural evi korunuyor; satır kilidi yukarıda |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/...` | 4289/4289 (yeni 35: Combination 7, CounterMoment 3, OrderLines 25) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-namespace ...` (beş parça, yerel PostgreSQL 18) | 2271/2271 (Ordering 170; yeni `AxesTieTests` 20); `origin/main` (`7ddf334`) birleştirildikten sonra Unit 4289/4289, Consistency 277/277 ve Ordering sınıfları (Axes 20, Payment 33, Shipment 86, OrderTransitions 31) yeniden yeşil |
| Lint | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | değişiklik yok |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı |
| Contract | — | — | NOT_APPLICABLE |

**Merge sonrası düzeltme (2026-11-08):** main'deki T336 kuralı (fiziksel ürün üretim yeri ister) `AxesTieTests` tohumunu kırdı; `SeedVariantAsync` fiziksel ürüne `.WithSeededOrigin()` alır (yalnız tohum, üretim kodu ve davranış değişmedi); Axes 20, Payment 33, Shipment 86 yerelde yeniden yeşil.

**Değişen / silinen eski testler (gerekçe):**

| Test | Değişiklik | Gerekçe ve yeni karşılık |
|---|---|---|
| T316 `S1_OnAnOrderWithoutAPhysicalItem_IsRefused_AndWritesNothing_KK3` (dijital, hizmet) | silindi | `StartPreparationAsync` yok; fiziksel kalemsiz siparişte S1 yazılmadığı `CardPayment_OfAnOrderWithoutAPhysicalItem_AppliesNoS1_…_KK2`, `ADigitalOrdersCardPayment_…`, `ADigitalAndServiceOrdersCardPayment_…` ile sınanır |
| T316 `S1_AfterTheTransfersO1_WritesS1Alone_AndNoSecondB4Row_KK3` | silindi | havalede S1, Ö1 ile aynı işlemde (T317 `MarkTransferPaid_…_KK3`; `TheTransferMarksCorrection_OnPreparing_…` [O1, S1]) |
| T316 `S1_WhileThePaymentIsPending_WithoutItsApproval_IsRefusedByThePairRule_KK3` | silindi | Hazırlanıyor + Bekliyor yasağı `APendingPayment_StandsOnlyWithPlaced_KK3` ve 30 hücrelik eşitlik testiyle |
| T316 `AnOperationOutsideTheTable_…_KK1` / `ATerminalState_…_KK2` `Op.StartPreparation` satırları | silindi | yöntem yok; S1'in kaynak durumu `TransitionTableTests.Allows_IsTrueOnlyForTheRowsSourceState_…` ile |
| T316 `TheShipmentAxis_HasAnOperationForS1S3S4S9…` | yeniden adlandırıldı (`…AndNoneForS1OrTheClosures_KK4`) | S1'in açık yüzde yöntemi yok |
| T317 `CardPayment_OfAnOrderWithoutAPhysicalItem_AppliesO1Only_…` | `…AppliesNoS1_…`; dijitalde beklenti [O1, S2] | 03 §1.4.2.1: yalnız dijital siparişte Ö1 ile S2 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 2, TAM yeniden doğrulama (2026-11-08, head `7f662e0` = `ef5eff9` (T324 #454 birleştirmesi; ödenmemiş iptalin ayırma bırakması tek biçimde, `ReleaseReservationAsync` → `OrderCounters.ReleaseAsync`) + `origin/main` (`4ff04bc`, T344, #451)). Kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓; T324'ün ödenmemiş iptal davranışı bozulmadı. Tur 1: ✓ PASS (head `393bd9f`) |
| Bulgu sayısı | 0 (tur 2), 0 (tur 1) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 — K1 bu PR'da kapatıldı: yapım raporunun "`12` doğrulama protokolü" satırı başka KP'lerin satırlarını (`VAL-C079`…`C099`) anar, task'ın kaynak KP'lerinin (KP-46, KP-51) grep'ini yapmaz; KP-46 satırları ve `VAL-A265` anılmaz. Doğru küme aşağıdaki "`12` satırları" paragrafındadır |

**Doğrulayıcı kanıtları (tur 2, `7f662e0`):** kapsam TAM — bütün kriterler ve KK'ler yeniden. Birleşim tablosu (`TransitionTable.Combinations`) `03 §1.4.1` ile 30 hücre yeniden karşılaştırıldı; Ö1'in eşzamanlı geçişleri (`CompleteApprovalAsync`: `CommitAsync`, dijital teslim işareti + Türkiye tarihi, kapanış; havalede kapanış yöneticinin) `03 §1.4.2`, §1.5 ile; düzeltme (`CorrectAsync`: S1 tek başına ve kart Ö1 `CorrectionNotPossible`, havale Ö1 Hazırlanıyor'da S1 ile iki kayıt + `RestoreAsync`) `03 §1.4.1` (c), §1.4.4.8 ile; an tablosu (`CounterMomentTable`, `OrderCounters`, `StockCounters` `Retake`/`Restore`) `03 §1.4.4` ile; `OrderLines` `03 §1.5`, §8.5 ile. Birleştirmenin yeniden yazdığı ödenmemiş iptal: `CancelOrderAsync` "ödenmemiş"i seçilen kodlarda Ö2 ile belirler (Ö2 yalnız Alındı + Bekliyor'da seçilir; main'deki `PaymentStatus == Pending` okumasıyla eşdeğer, çift kuralı Bekliyor'u Alındı'ya bağlar), bırakma tek yazar `OrderCounters.ReleaseAsync` (satırları siler, katalogda ayrılanı ve kupon ayırmasını düşürür); başka bırakma yolu yok (`StockReservations` yalnız `OrderCounters`'ta yazılır); T324'ün iki çağıranı (`PaymentWindowSources`, `FinalPaymentQueryJobHandler`) bu yoldan geçer. Testler yerelde: Unit Consistency 277/277, öteki 4179/4179; betik 4/4; Integration (PostgreSQL 18): `Ordering` ad alanı 185/185 (`AxesTieTests` 20, `PaymentWindowTests` dahil), Concurrency + StockAndCapacity + ModuleCallTransaction + OrderDisposal + ProductTransitions 124/124, Disposal 61/61; build `-warnaserror` 0/0, format temiz, `--locked-mode` 0. Dal CI `7f662e0` 42456190315 yeşil (9 iş geçti, Guard atlandı). Ana dal son 3 tamamlanmış run 42452259880, 42452259999, 42450627496 success. `12` satırları değişmedi (KP-46/KP-51 hepsi BEKLEMEDE, gerekçe aşağıda). Yapım raporu karşılaştırması: uyumlu, öz denetim tablosu kanıtlı; yeni uyuşmazlık yok.

**Doğrulayıcı kanıtları (tur 1):**

| # | Kriter / KK | Sonuç | Doğrulayıcı kanıtı |
|---|---|---|---|
| 1 / KK1 | Birleşim tablosu kodda; ulaşılamaz çift reddedilir | ✓ | `TransitionTable.Combinations` 30 hücre `03 §1.4.1` ile elle karşılaştırıldı (Alındı: Bekliyor/Ödendi/KGÖ/GÖ; ara dört durum ve Teslim edildi: Ödendi/KGÖ/GÖ; İptal edildi: Bekliyor dışı hepsi); `IsPairAllowed` tabloyu okur; ret yolu geçişte (`ApplyChosenAsync`, `IsPairAllowed` → `ForbiddenStatePair`) ve düzeltmede (`CorrectAsync`); `Combinations_EqualTheDocumentsTable_CellByCell_KK1` (belgeden okur), `Apply_ATransitionThatWouldEndInAForbiddenPair_IsRefusedBeforeAnythingIsWritten`, `IsPairAllowed_EqualsTheCombinationRuleOfTheDatabase_ForAllPairs` yeşil |
| 2 / KK2 | Teslim edildi + Bekliyor/Başarısız yasak | ✓ | `Delivered_WithPendingOrFailed_IsForbidden_KK2`; dijital havale siparişinde Ö1 düzeltmesi `ForbiddenStatePair`, düzeltme kaydı 0 (`TheTransferMarksCorrection_OfADeliveredOrder_IsRefusedByThePairRule_KK2`); kapanış Bekliyor/Başarısız'da Teslim edildi yazmaz (`EvaluateClosureAsync`) |
| 3 / KK3 | Hazırlanıyor'a yalnız ödeme onayıyla; Bekliyor'a yalnız havale düzeltmesiyle | ✓ | S1'in açık yüzde yöntemi yok (`StartPreparationAsync` kaldırıldı; `ShipmentTransitionContractTests`); S1 yalnız Ö1 ile (`ApplyCardPaymentAsync`, `MarkTransferPaidAsync`); Hazırlanıyor + Bekliyor çifti `APendingPayment_StandsOnlyWithPlaced_KK3` ve DB kuralı; `CorrectAsync`: tek başına S1 düzeltmesi ve kart Ö1 düzeltmesi `CorrectionNotPossible` (`TheCardApprovalsCorrection_IsRefused_…_KK3`), havale Ö1 düzeltmesi Hazırlanıyor'da S1'i de düzeltir — iki düzeltme kaydı, Alındı + Bekliyor (`TheTransferMarksCorrection_OnPreparing_…_KK3_KK8`) |
| 4 / KK4 | Eşzamanlı geçişler tek işlemde | ✓ | `CompleteApprovalAsync` Ö1'in işleminde: `CommitAsync`, açık dijital kaleme `DeliveryMarkedAt` + `DeliveryDate` (Türkiye tarihi; `06` DeliveryDate tanımı), kapanış kuralı; testler: dijital kart [O1, S2] tek işlem, sürüm 2 (`ADigitalOrdersCardPayment_…_KK4`); karışık kart [O1, S1], yalnız dijital kalem işaretli (`AMixedOrdersCardPayment_…_KK4_KK8`); dijital havale S2 yönetici aktörüyle (`ADigitalOrdersTransferMark_…_KK4`; `03 §1.5.3`); dijital + hizmet Alındı + Ödendi'de bekler (`…_KK4`); S3 + Ö2 ayırmayı aynı işlemde bırakır (`TheUnpaidCancellation_…_KK4_KK8`); son kalemin kapanışı T316 `TheLastEventThatCompletesTheClosure_…_KK4` |
| 5 / KK5 | Kapanış: teslim varsa Teslim edildi, yoksa İptal edildi | ✓ | `EvaluateClosureAsync` (S2/S10/S11 ya da kapanış sebebiyle S3/S4); üç sonuç ayrı: S11 (`ADeliveryFailedOrderWithADeliveredItem_…`), S2/S10 (T316 `…_KK4` teorisi), S4 kapanış (`TheClosure_ByATermination_…_KK7`), bekleme (`AnOrderWithNoDeliveredItem_OnShippedOrDeliveryFailed_…_KK5_KK6`) |
| 6 / KK6 | Teslimsiz Kargoya verildi/Teslim edilemedi bekler; S7 dahil S11; yanıtsız sorguda iptal ertelenir | ✓ | Kargoya verildi'de erken dönüş, Teslim edilemedi'de iptal satırı `null`; `AnOrderWithNoDeliveredItem_…` (iki durum); 2 adetten 1'i kapanınca bekler, son adette S11 (`ADeliveryFailedOrderWithADeliveredItem_…_KK6_KK7`); S7'nin kendisi kapanışı tamamlarsa S11 (T316 `S7_OfAShipmentWhoseItemsAreClosedAndOthersDelivered_EndsInS11_…`); yanıtsız son sorgu T204/T305 kuralı (`OutageRulesTests`) — T318 değiştirmedi |
| 7 / KK7 | Kalem adetle açık; kapanışta iptal kaydı ve ikinci geri ödeme yok | ✓ | açıklık `Quantity - ClosedQuantities > 0`; `TheClosure_ByATermination_OpensNoCancellationRecordAndNoSecondRefund_KK7` (`cancellations` 0, `refunds` 0, S4) |
| 8 / KK8 | Sayaç etkisi an tablosuyla | ✓ | `CounterMomentTable` 9 satır `03 §1.4.4` ile, `OrderCounters` yöntemleri `06 §4.14` yazım yollarıyla (1 ayırma, 2 kesin düşme, 3 serbest bırakma, 5 düzeltmede ayırmaya dönüş — satılabilir adet değişmez, 6 yeniden alma — yetmezse false, kupon limit aşsa da) karşılaştırıldı; `StockCounters` `Retake`/`Restore` koşullu `ExecuteUpdate`, silinmiş varyant atlanır; kupon dönüşü yalnız siparişi son hâline getiren kayıtta ve bir kez, ret/"mal dönmedi"de hiç (`IsCouponReturnDueAsync`); dokuz anın testleri `AxesTieTests` `…_KK8` yeşil; `CounterMomentTableConsistencyTests` yeşil. Üretim çağıranları D-453 |
| 9 / KK9 | Yedi hattın dizisi ve adım sayısı; karışıkta toplam, "ödendi" bir kez | ✓ | `OrderLines.Rows` ve `StepsOf` `03 §1.5`, §8.5 ile elle karşılaştırıldı (kart fiziksel 2, havale 3, hizmet 1/2, dijital 0/1; havale fiziksel + hizmet 4); `OrderLinesConsistencyTests` beklenenleri belgeden okur, yeşil. Uçtan uca ölçüm (`VAL-B023`…`B026`) panel işlemleriyle — D-454 |
| 10 / KK10 | Tek tipli hatta en çok üç | ✓ | `ASingleTypeLine_NeverExceedsTheCeiling_AndTheHeaviestIsThePhysicalOrderByTransfer_KK10` (tavan 8.5'ten okunur, en ağır yalnız havale fiziksel) |
| `09 §25` | ilgili satırlar | ✓ | durum yalnız geçiş biriminde; `TransitionTable` 16 satır değişmedi; sıra kilit → beyaz liste → koşul → durum → kayıt → iç yan etki (sayaç, işaret) → dış (ödeme kaydı, B-4/B-7 satırı); sayaçlar `ICatalogModule`, kalem olguları `IAfterSalesModule` `Contracts`'ından; `SaveChanges` yok; zaman `TimeProvider`; göç, paket, ayar yok; mimari testler yeşil |
| Güvenlik | taraması | ✓ | diff'te sır, dış servis, ham SQL (üretim), yeni uç ya da paket yok; düzeltme yalnız panel aktörüyle (mevcut denetim); yeni girdi yüzeyi yok |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un `scripts/git-hooks`'u; worktree'de aynı bekçi, daldaki kopya kullanılmadı); ana dal son 3 tamamlanmış run 42443632733 (success), 42443632759 (success, `7ddf334`), 42436765868 (success, `bcb856d`); memory: `grep -nE "\bT318\b"` satır 25'i getirir (D-307 → T318); kayıt kuralı (2026-11-08) gereği "Güncel Durum"a dokunulmaz.

**Testler (doğrulayıcı, yerel, `393bd9f`):** `dotnet restore` + `dotnet build -warnaserror` 0 uyarı/0 hata; `dotnet format --verify-no-changes` Web, Unit, Integration projeleri temiz; Unit Consistency 277/277, öteki (Architecture dahil) 4070/4070; betik testleri 4/4; Integration (PostgreSQL 18 konteyner): `Ordering` ad alanı 170/170 (`AxesTieTests` dahil), Concurrency + StockAndCapacity + ModuleCallTransaction + OrderDisposal 83/83, ProductTransitions 41/41; sonrasında `git status` temiz, `dotnet restore --locked-mode` 0. Dal CI: `d36a461` run 42444593460 success (Lint, Build, Unit, Integration, Migration, Paketleme, E2E, Gate); `393bd9f` run 42445310215 doğrulama anında koşuyordu.

**`12` satırları:** `11 §7.1` T318 kaynakları KP-46, KP-51. KP-46 satırları (`VAL-A207`, `A208`, `C039`…`C048`) panel uç yanıtı ister (D-308 → T527); KP-51 satırları `VAL-B023`…`B026` "Uç yanıtı + Veri kaydı" ister — hizmetin "tamamlandı" işlemi ve panel uçları yok (D-454 → T351; uç T527), `VAL-A265` (fatura) tarayıcı/uç gözlemidir. T318 uç ya da sayfa eklemez; servis düzeyi kanıt kriter 9–10 satırlarındadır. Hepsinin `Durum`'u `BEKLEMEDE` kaldı. `12_matris_denetle.py --kapsam 1-77` hata 0.

**Yapım raporu karşılaştırması (adım 13):** hüküm uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Uyuşmazlık 1 (K1): `12` satırları yukarıdaki gibi düzeltildi.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T318-eksen-bagi`
- **Commit:** `f33d781` (backlog kayıtları; üstünde yalnız PR numarası commit'i)
- **PR:** #455
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

Dış varsayım: yok — task kendi kod tabanının durum makinesi ve sayaçlarıdır; plan tier, kota, API anahtarı, paket sürümü geçmiyor (tetikleyici tarama boş).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | An yöntemlerinin üretim çağıranları: 1.4.4.1 → T348, 1.4.4.4 → T367/T368, 1.4.4.5 → T368, 1.4.4.6 → T372/T373, 1.4.4.7 → T319 | D-453 | yöntem + test burada; kaydı yazan işlemler o task'ların |
| 2 | Hizmetin "tamamlandı" işlemi yok; elle adım bütçesinin hizmet hattı `StepsOf` ile ölçüldü, uçtan uca ölçüm (VAL-B023…B026) panel işlemleriyle | D-454 | işlem T351'in |
| 3 | Kart Ö1'in çağıranları (sağlayıcı bildirimi, son sorgu) | D-455 | sağlayıcı bağlayıcısı T402/T403 |
| 4 | Havale işareti düzeltmesinin 1.6.2.4 koşulları (teslim işareti, kalem kaydı, geri ödeme yokluğu) | D-456 | düzeltme işleminin koşulu T319'un (`CorrectAsync` `condition`) |

## Notlar

- **Working tree (Adım -1):** temiz (ikinci işçi; yeni worktree)
- **Ana dal CI startup (Adım 0):** 42436765863 success, 42436765868 success, 42434282714 success
- **Repo memory (bitiş kapısı 9):** kayıt kuralı (2026-11-08, chore #450) gereği `MEMORY.md` "Güncel Durum"a dokunulmadı; changelog girişi `Docs/STATUS_CHANGELOG.d/T318.md`
- **Diğer:** İlk işçi commit edemedi (yerel `main` `origin/main`'in bir gerisindeydi, commit-msg bekçisi "Yabancı TXX: T302" ile reddetti; bekçi atlanmadı). İkinci işçide yerel `main` güncel olduğundan engel kalmadı.
- ⚠ (a) 1.4.4.4–1.4.4.7 anlarının yöntemi ve testi var, üretim çağıranı yok (D-453). (b) Hizmet "tamamlandı" adımı ölçülemedi (D-454). (c) S1'in tek başına yöntemi kaldırıldı — bağımsız çağıranı yoktu (D-307 kapandı). (d) Havale düzeltmesinde dijital kalemin teslim işareti geri alınmaz; 1.6.2.4 teslim işaretli siparişin düzeltilmesini zaten yasaklar, koşul T319'da (D-456).
