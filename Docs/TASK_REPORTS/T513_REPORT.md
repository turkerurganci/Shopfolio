# T513 — Cayma beyanı ve ayıp talebi ekranları (E-18, E-19)

## Yapılan İşler

- E-18 (`{kök}/cayma`) ve E-19 (`{kök}/ayip-talebi`) sayfaları üye ve erişim anahtarı kökleri için kuruldu; üç POST işleyicisi (`SubmitWithdrawal`, `SubmitDefectClaim`, `ReopenDefectClaim`) `[GatedOperation]` taşır ve modülün yazıcılarını çağırır, kuralı yeniden yazmaz (D-153, D-776).
- Kök çözümü sipariş sayfasıyla ortak (`OrderRoots`): ekran, sayfanın açık olmadığı yerde açılamaz; dönüş adresi ekranın adresinden son parça atılarak yazıldığı gibi kurulur (`RootOf`).
- Sipariş sayfası dönüş sebebini (`OperationClosed`, `QuantityExceedsOpen`) işlemlerin yerinde söyler (`OrderScreenTexts.ReasonOf`).
- Cayma ekranı için modül değerleri genişledi: gönderme süresinin son günü, geri ödeme süresi (Z-16) ve başlangıcı (teslimden sonra malın firmaya ulaşması, önce beyan), ödeme yolu (`WithdrawalNotes`, `WithdrawalScreen`); ayıp talebi bilgisi ilk açıklamayı taşır (`DefectClaimInfo.Description`).
- Ekran metinleri `OrderScreenTexts`'te; süreler metne yazılmaz, modülden gelir (K-785).
- Ekran testlerinin tohumu başlangıç verisinin ayar satırını yerinde doldurur (ikinci satır `return_address_all_or_none` kısıtına takılıyordu); üç null-forgiving (`!`) kaldırıldı (`NullForgivingSourceScanTests`); ekran metinleri ve kök için birim testleri eklendi.
- **D-1195 (T513 payı):** kalem kaydı ayrıntıyı taşır (`ItemRecordFact.ReturnAddress`, `ClaimStatus`): fiziksel kalemin cayma beyanı sipariş sayfasındaki satırda beyana yazılan iade adresiyle görünür ve adres sonradan değişse de değişmez (K-665); ayıp talebi kaydı durum rozetini taşır (`04` 5.16.5, 2.5.1.5). Kalem kendi rozetini hâlâ taşımaz. İade reddinin sebebi payı T528'e (ret orada yazılır).
- **`origin/main` birleştirmesi (T512, T521, T533):** `OrderRoots` tek evde (`Pages/Orders/OrderRoots.cs`); dönüş sebebi metninin tek evi `OrderScreenTexts` — T512'nin E-17 sebep metni (`OrderPageModel.OperationReasonText`) artık `OrderScreenTexts.ReasonOf` ile kurulur.
- **D-1196 (T513 payı):** sipariş sayfasının cayma ve ayıp talebi düğmelerinin adresi açılır (`AfterSalesScreenTests.TheOrderPagesWithdrawalAndDefectClaimButtons_OpenTheirScreens_D1196`).

### Düzeltme 1 (doğrulama tur 1 FAIL — B1–B4, K2)

- **B1 (kriter 1, para):** E-18'in kargo ücreti satırları modülün kararı. Kural evi AfterSales: `IAfterSalesModule.WithdrawalFeeUnitsAsync` (kargoya verilen adet − kargodan önce iptal/çıkarılan − bütün beyanlar; ret ya da "mal dönmedi" kapanışı varsa ya da donmuş ücret sıfırsa tetik yok — K-1076) ve `WithdrawalRefundsShippingFee` → `ShippingFeeRefunds.OnWithdrawalPreview` (E-17'nin `OnCancellationPreview`'uyla aynı `IsMetAtOnce`). GET'teki koşullu cümle (`ShippingFeeText`) yalnız listelenen bütün adetlerden cayma ücreti geri getirebilecekken (`ShippingFeeCanReturn`); son adımdaki "Kargo ücreti de geri ödenir." (`ShippingFeeRefundedText`) yalnız seçim önceki beyanlarla birlikte kargoya verilen bütün adetleri kapatınca (`RefundsShippingFee`). Sayfa hesap yapmaz. D-1340 kapandı.
- **B2 (`09 §20` m.4):** ekran bütün kalemleri tek okumayla yükler — `IOrderOperations.ReadWithdrawalScreensAsync` siparişin olgularını bir kez yükler, iade adresi ve notları toplu okur (`WithdrawalNotesAsync`); onaylı yazımın K-1202 karşılaştırması da tek okuma. Komut sayısı kalem sayısına bağlı değil (komut sayacı testi; `FileEndpointsHost` isteğe bağlı `interceptor` alır).
- **B3 (`04` 2.12.1.8; D-1367):** satırlar kalemin sipariş adedini sınırla birlikte söyler — E-18 `OrderScreenTexts.OpenUnits(quantity, open)`, E-19 `ClaimLimit(quantity, max)`, E-17 (`_OperationScreen`) "Siparişteki adet: N · açık adet: X · en çok Y". E-19'un tek `Quantity` alanı seçili (gönderilen ya da tek listelenen) kalemin satırında, seçim yokken tek alanlı satırda açılır (`DefectClaimPageModel.QuantityFieldRow`); birden çok alanlı satırda seçim yokken liste altında kalır (⚠, D-1412). D-1367 kapandı.
- **B4 (kriter 4; `07 UC-43`):** onaysız ilk gönderimde de açık adedi aşan ya da gösterilenden farklı açık adet (`OriginalOpenQuantity`) 303 kök + `QuantityExceedsOpen`, kayıt yok (`WithdrawalPageModel.QuantityChanged`); T512'nin E-17 kalıbı.
- **K2:** E-18'de aynı kalemin iki satırı 422, çok kalemli istekte ikinci kalemin aşan adedi bütün isteği reddeder — testle kilitli.
- **Onarım:** `_WithdrawalScreen.cshtml`'de sınır satırı `if`/`else` arasına girmişti (else metin olarak basılıyordu); satır blok sonrasına alındı.
- **Yeni testler:** Integration (`AfterSalesScreenTests.ShippingFeeAndRows.cs`, sınıf `partial`) `TheUnconfirmedPost_WithUnitsAboveTheOpenOnesOrStaleOpenUnits_Is303QuantityExceedsOpen_AndWritesNothing_B4`, `TwoRowsOfOneItem_Are422_AndAnExceedingSecondItem_RefusesTheWholeRequest_K2`, `TheShippingFeeLine_IsOnTheLastStepOnlyWhenTheChoiceCoversEveryShippedUnit_B1`, `TheShippingFeeLines_AreAbsentOnFreeShipping_AndCountTheEarlierDeclarations_B1`, `TheWithdrawalScreen_SendsTheSameNumberOfCommands_ForOneItemAndForThree_B2`, `TheRows_SayTheItemsUnitsWithTheLimit_AndTheDefectClaimsFieldOpensInTheItemsRow_B3`; Unit `CancellationPreviewTests.TheWithdrawalsLastStep_PaysTheFeeBackOnlyWhenTheChoiceCoversTheUndeclaredUnits_T513B1`; `OrderScreenTextsTests` sabit metin listesine `ShippingFeeRefundedText`.
- **K1** (alan kuralı 422'si yabancı kalemin 404'ünün önünde) D-1401 ile F5 kapısında; kapsam dışı (`07 §2.3.3` süzgeç sırası bütün ekranların ortak kalıbı).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Orders/AfterSalesScreenModels.cs` (yeni), `_WithdrawalScreen.cshtml`, `_DefectClaimScreen.cshtml`, `MemberWithdrawal.cshtml`, `AccessWithdrawal.cshtml`, `MemberDefectClaim.cshtml`, `AccessDefectClaim.cshtml` (yeni)
- `src/Shopfolio.Web/Pages/Orders/OrderRoots.cs`, `OrderScreenTexts.cs` (yeni); `OrderPageModels.cs`
- `src/Shopfolio.Web/Pages/Components/ItemRecordLabels.cs`, `src/Shopfolio.Web/Pages/Shared/_ItemRecords.cshtml`
- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.ItemRecordList.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`, `Contracts/ItemRecordFacts.cs`
- `src/Shopfolio.Web/Modules/Ordering/OrderOperations.cs`, `Contracts/WithdrawalRequests.cs`
- `tests/Shopfolio.IntegrationTests/Files/AfterSalesScreenTests.cs` (yeni, 11 test), `tests/Shopfolio.UnitTests/Pages/OrderScreenTextsTests.cs` (yeni, 12 test durumu)
- Düzeltme 1: `Modules/AfterSales/AfterSalesModule.RefundAmounts.cs`, `AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.RefundLine.cs`, `ShippingFeeRefunds.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`; `Modules/Ordering/OrderOperations.cs`, `OrderPeriods.cs`, `OrderTransitions.RefundLine.cs`, `Contracts/IOrderOperations.cs`, `Contracts/WithdrawalRequests.cs`; `Pages/Orders/_OperationScreen.cshtml`; `tests/Shopfolio.IntegrationTests/Files/AfterSalesScreenTests.ShippingFeeAndRows.cs` (yeni, 6 test), `FileEndpointsHost.cs`; `tests/Shopfolio.UnitTests/AfterSales/CancellationPreviewTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET {kök}/cayma` içerik; kapalıysa 303 kök + sebep | ✓ (düzeltme 1: kargo ücreti satırı modülün kararı — B1) | `AfterSalesScreenTests.TheWithdrawalScreen_ShowsTheItemsUnitsReturnAddressAndRefund_AndAClosedOneIs303WithTheReason_KK1`, `…TheWithdrawalScreen_ShowsTheConditionalExemptionsLine_OnlyOnTheItemThatHasIt_KK1`, `…TheShippingFeeLine_IsOnTheLastStepOnlyWhenTheChoiceCoversEveryShippedUnit_B1`, `…TheShippingFeeLines_AreAbsentOnFreeShipping_AndCountTheEarlierDeclarations_B1`, `…TheRows_SayTheItemsUnitsWithTheLimit_…_B3`; Unit `CancellationPreviewTests.TheWithdrawalsLastStep_PaysTheFeeBackOnlyWhenTheChoiceCoversTheUndeclaredUnits_T513B1` |
| 2 | Onaysız 200 son adım; onaylı 303 kök, tarih ve adres kökte | ✓ | `…TheUnconfirmedPost_IsTheLastStepAndWritesNothing_AndTheConfirmedOneFreezesTheReturnAddress_KK2_KK3` |
| 3 | Kalem başına tarih + adet; iade adresi donar | ✓ | aynı test (adres değiştirildikten sonra kayıt eski adresi tutar) |
| 4 | Üç 422, pencere/hak 303, açık adet farkı 303, yabancı kalem 404 | ✓ (düzeltme 1: onaysız gönderimde de aşan/eski adet 303 — B4) | `…TheWithdrawalsThree422s_TheClosedWindow_TheStaleUnits_AndTheForeignItem_WriteNothing_KK4`, `…TheUnconfirmedPost_WithUnitsAboveTheOpenOnesOrStaleOpenUnits_Is303QuantityExceedsOpen_AndWritesNothing_B4`, `…TwoRowsOfOneItem_Are422_AndAnExceedingSecondItem_RefusesTheWholeRequest_K2` |
| 5 | Kısmi seçimin tekrarı ikinci cayma olmaz | ✓ | `…TheRepeatOfAPartialSelection_IsNoSecondWithdrawal_But303WithTheReason_KK5` |
| 6 | Yeni talep: açık kalemler, varsayılan 1, üst sınır, iade adresi; yeniden açma: talep + Çözüldü rozeti | ✓ | `…TheDefectClaimScreen_NewClaimHasDefaultOneAndTheLimit_AndTheReopeningShowsTheResolvedClaim_KK6`; üst sınır kalemin adediyle ve alan kalemin satırında `…TheRows_SayTheItemsUnitsWithTheLimit_AndTheDefectClaimsFieldOpensInTheItemsRow_B3` |
| 7 | Üç 303 (açık değil, Çözüldü değil, Z-18 doldu); 404 (kök, yabancı, talep yok) | ✓ | `…TheDefectClaimScreen_Is303WhenNotOpenNotResolvedOrPastZ18_And404ForAForeignItemOrNoClaim_KK7` |
| 8 | Tek talep Açık; iade adresi talepte; onay adımı yok | ✓ | `…SubmitDefectClaim_OpensTheOneClaimAsAcikWithTheReturnAddress_WithoutAConfirmationStep_KK8` |
| 9 | Üç 422, üç 303, yabancı kalem 404 | ✓ | `…SubmitDefectClaim_Three422s_Three303s_AndTheForeignItems404_KK9` |
| 10 | Z-18 içinde Açık + müşteri satırı; dışında 303; talep yoksa 404 | ✓ | `…ReopenDefectClaim_InsideZ18MakesItAcikWithTheCustomersLine_OutsideIs303_AndNoClaimIs404_KK10` |

### Öz denetim (bitiş kapısı 2)

Düzeltme 1 sonrası yeniden tarandı (2026-11-12, `origin/main` `73e36672` ile birleşik ağaç): `11` T513'ün on kabul kriteri ve doğrulama kontrol listesinin bütün maddeleri, doğrulama tur 1 raporunun bulguları (B1–B4), K-notları (K1, K2) ve yapım raporu karşılaştırmasının dört uyuşmazlığı, `12 §6.1` task çıkışı ve sık FAIL nedenleri listesinin bütün satırları (T513 r1'in kendi dört satırı dahil). Önceki turların işi dahil.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: sayfa içeriği; kapalı hâl | ✓ | `AfterSalesScreenTests …_KK1` (iki test; koşullu istisna satırı yalnız istisnalı kalemde); `OrderScreenTextsTests.TheRefund_NamesItsWayAndItsStart_KK1`, `TheSendingPeriod_NamesTheDaysAndTheLastDay_KK1`, `TheRootOfAScreen_KeepsTheCaseOfTheKey_KK1`. Kargo ücreti (kriter 1 "geri ödeme bilgisi"; `04` 5.18.6; `07 UC-42`): GET cümlesi ve son adım satırı modülün kararı (`ShippingFeeRefunds.OnWithdrawalPreview`) — `…_B1` iki test (ücretsiz kargoda ikisi de yok; önceki beyan sayılır; seçim bütün kargolanan adetleri kapatmazsa son adımda satır yok), Unit `…_T513B1` |
| KK2: onaysızda kayıt yok | ✓ | `…_KK2_KK3` (`WithdrawalCountAsync` = 0 son adımda; onaylıda kökte iade adresi başlığı ve tarih); `TheResultSentence_CountsTheUnitsChosen_KK2` |
| KK3: iade adresi donar | ✓ | `…_KK2_KK3` (`SetReturnAddressLineAsync` sonrası kayıt "Depo Sokak 3"); `TheWithdrawalRecord_NamesTheReturnAddressWrittenIntoIt_KK2_KK3` |
| KK4: üç 422 sebebi, pencere, 404 | ✓ | `…_KK4` (onaylı); onaysız ilk gönderimde aşan ve eski açık adet 303 `QuantityExceedsOpen`, kayıt 0 — `…_B4`; aynı kalemin iki satırı 422 ve ikinci kalemin aşan adedi bütün isteği reddeder — `…_K2` (VAL-A581, VAL-A688); K-1202 karşılaştırması onaylıda yazımın işleminde tek okumayla (`ReadWithdrawalScreensAsync`) |
| KK5: ikinci gönderim cayma açmaz | ✓ | `…_KK5` |
| KK6: iki hâl, varsayılan adet | ✓ | `…_KK6`; `FurtherInformation_GoesToTheFirmsEmail_WhenThereIsOne_KK6` |
| KK7: üç 303 sebebi, 404 | ✓ | `…_KK7`; `TheTwoReturnCodes_HaveTheirOwnWords_KK4_KK7` |
| KK8: tek talep, Açık | ✓ | `…_KK8` (durum, adet, açıklama, iade adresi; kalemin satırında talep kaydı Açık rozetiyle — `04` 5.16.5) |
| KK9: üç 422, üç 303, 404 | ✓ | `…_KK9` |
| KK10: Z-18 içi ve dışı; müşteri satırı | ✓ | `…_KK10`; yeniden açma açıklama almaz (`ReopenDefectClaimInput` yalnız adet) |
| KK6 ek: üst sınır kalemin adediyle, alan kalemin satırında (`04` 2.12.1.8; VAL-A881 sunucu yarısı; D-1367) | ✓ ⚠ | `…_B3` (E-18 `OpenUnits(quantity, open)`, E-19 `ClaimLimit(quantity, max)`, E-19 alanı seçili/tek alanlı kalemin satırında); E-17 satırı da aynı kalıp. ⚠ E-19'da birden çok alanlı satırda seçim yokken alan liste altında (`07 §5` tek `Quantity`) — D-1412, F5 kapısı |
| 09 §25: "Döngüde sorgu yok mu?" | ✓ | E-18 GET, POST yeniden yüklemesi ve onaylı K-1202 okuması kalem sayısından bağımsız tek okuma (`IOrderOperations.ReadWithdrawalScreensAsync`, `IAfterSalesModule.WithdrawalNotesAsync`/`WithdrawalFeeUnitsAsync` toplu); `…TheWithdrawalScreen_SendsTheSameNumberOfCommands_ForOneItemAndForThree_B2` (bir ve üç kalemde aynı komut sayısı) |
| 09 §25: "Kod hiçbir doküman cümlesiyle çelişmiyor mu?" | ✓ | B1 (`04` 5.18.6, K-789, K-1076), B3 (`04` 2.12.1.8), B4 (`07 UC-43`) kapandı; kalan tek sapma ⚠ D-1412 (karar ve gerekçe backlog satırında) |
| 09 §25: kural modülde (tek kural evi) | ✓ | kargo ücreti tetiği AfterSales `ShippingFeeRefunds` (E-17 ile aynı `IsMetAtOnce`); sayfa yalnız modülün cevabını çizer (`WithdrawalRefundsShippingFee`) |
| 09 §25: tarama ve mimari testleri | ✓ | Unit TAM 7481/7481 (`*SourceScan*`, `*ModelBindingRule*`, `*Architecture*`, Consistency dahil); Integration uç taraması `PageFormPatternTests`, `ForgeryProtectionTests`, `SecurityHeadersTests`, `SessionAccessTests`, `WritingEndpointAudit` 92/92 (yapım turu; düzeltme yeni sayfa/uç eklemedi) |
| Güvenlik: POST'lar kapılı, antiforgery, yetki | ✓ | üç işleyici `[GatedOperation]` (`AfterSalesScreenModels.cs`); kök çözümü sipariş sayfasıyla ortak (`OrderRoots.ResolveMemberAsync`/`ResolveAccessAsync`), yönetici oturumu ve yabancı sipariş 404; ham SQL yok; null-forgiving yok; yeni dış bağımlılık ve sır yok |
| Devredilen backlog kalemleri (`12 §6.1`) | ✓ | D-776 T513 payı kapandı; D-1196 kapandı (`…_D1196`); D-1195 T513 payı karşılandı (KK2/KK3 ve KK8 testleri), iade reddi sebebi T528'e ileri atıldı (satırda gerekçeyle); D-880 T513 payı (`…_KK7`, sebep metni iletişim formunu anar) satırda yazılı; düzeltme 1: D-1340 (hedef T513'e döndü) ✅ `…_B1` testleriyle, D-1367 (T512 doğrulaması, hedef T513) ✅ `…_B3` testiyle; D-1401 (K1) F5 kapısında; yeni D-1412 (⚠ yukarıda) |
| Birleştirme: aynı işi yapan iki yol kalmadı | ✓ | `OrderRoots` tek dosya; sebep metni tek evi `OrderScreenTexts.ReasonOf` (T512'nin sabitleri silindi); `OrderOperationScreenTests` + `OrderPageTests` yeşil |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | yeni tarama/bekçi istisnası eklenmedi |
| Sık FAIL (2) tarama çağrı biçimi | ✓ | yeni tarama yok; `OperationSourceScan` aynı dosyadaki çağrıyı izler, işleyiciler `AfterSalesScreenModels.cs`'te |
| Sık FAIL (3) 06 varsayılanı / FK dizini | ✓ | şema değişikliği ve göç yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | sınır metni değişince onu anan yerler: E-18/E-19 testleri `OpenUnits(2, 2)`/`ClaimLimit` sabitten türer; E-17 `_OperationScreen` aynı kalıba geçti (`OrderOperationScreenTests` koşuldu, aşağıda); test sayıları rapor ve changelog parçasında aynı (Integration `AfterSalesScreenTests` 17) |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | `09` değişmedi |
| Sık FAIL: kabul kriteri kısmen (~) ile PR | ✓ | on kriter ✓; kriter 1'in kargo ücreti payı artık backlog'da değil, kodda ve testte |
| Sık FAIL: hedefi bu task olan backlog kaydı | ✓ | D-1340, D-1367 kapandı ve testle karşılandı (kayıtlarda test adları) |
| Sık FAIL: döngüde sorgu, komut sayısı sınaması yok | ✓ | `…_B2` komut sayacı testi |
| Sık FAIL: kardeş ekranın düzeltmesi aynı kalıba taşınmadı (onaysız açık adet) | ✓ | E-18 onaysız dal E-17 (T512) kalıbıyla 303 — `…_B4` |
| Sık FAIL: modül kurucusuna bağımlılık / Integration TAM | ✓ | modül kurucusu değişmedi; `FileEndpointsHost` yeni parametresi isteğe bağlı; Integration TAM: CI |
| Sık FAIL: test beklenen değeri sabitten türetmiyor | ✓ | yeni testler metinleri `OrderScreenTexts` sabitlerinden, adetleri tohumdan türetir |
| Sık FAIL: boş küme karşılaştırması (0 = 0) | ✓ | B4/K2 testlerinin tohumu açık adetli, kargoya verilmiş kalemdir (aynı tohumla onaylı gönderim `…_KK2_KK3`'te kayıt yazar); 0 kayıt 303/422 durumu ile birlikte sınanır; B1'de satırın hem çıktığı hem çıkmadığı hâl, B2'de bir ve üç kalem karşılaştırılır |
| Tarayıcı satırları (VAL-B072, VAL-B073, VAL-A881) | — | tarayıcı gözlemi doğrulayıcının işi (`12 §4.17`); yapımda tarayıcısız PASS iddiası yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Derleme | ✓ | `dotnet build <proje> -warnaserror` (Web, UnitTests, IntegrationTests) | 0 uyarı, 0 hata (`origin/main` `a56a1735` ile birleşik ağaç) |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/… --max-threads 3 --filter-class` `*OrderScreen*`, `*SourceScan*`, `*ModelBindingRule*`, `*Architecture*`, `*ItemRecord*`, `*OrderPage*`, `*Iban*`, `*Notice*`, `*StatusBadge*` | 2127/2127 (birleşik ağaç); önceki birleştirmede (`29562ee9`) Unit TAM 7413/7413 |
| Integration (T513) | ✓ | `--filter-class` `*AfterSalesScreenTests`, `*OrderPageTests`, `*OrderOperationScreenTests`, `*ComponentMarkupTests` | 32/32 + D-1196 testleri 2/2 (`AfterSalesScreenTests` 11 test) |
| Integration (uç taramaları) | ✓ | `--filter-class` `*PageFormPatternTests`, `*ForgeryProtectionTests`, `*SecurityHeadersTests`, `*SessionAccessTests`, `*WritingEndpointAudit*` | 92/92 (Postgres 18, `track_commit_timestamp=on`) |
| Integration (önceki birleştirmede ad alanı parçaları) | ✓ | `--filter-namespace` | Administration 69, Catalog 82, Commands 70, Concurrency 30, CorporateContent 195, Database 36, Disposal 61, FileSecrets 24, Files 121, Jobs 85, Logging 30, Model 1410, Ordering A–O 399 |
| Integration TAM | — | — | **TAM doğrulayıcıda koşulacak** (yönetici kararı: makine yükü yüzünden yapımda yalnız değişen/ilgili sınıflar; kalan parçalar Ordering P–W, Outages, Payment, Security, SignedEndpoints, Web'in öteki sınıfları) |
| **Düzeltme 1:** Derleme + format | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format <proje> --verify-no-changes` (Web, UnitTests, IntegrationTests) | 0 uyarı, 0 hata; üçü temiz (head `f82e8297`, `origin/main` `73e36672` birleşik; düzeltme 1c yalnız belge) |
| **Düzeltme 1:** Unit TAM | ✓ | `dotnet test tests/Shopfolio.UnitTests` (parçalı) | 7481/7481 (Consistency, Architecture dahil) |
| **Düzeltme 1:** Integration (koşulan sınıflar) | ✓ | `--filter-class` `Files.AfterSalesScreenTests` (Postgres 18, `track_commit_timestamp=on`); `Files.OrderOperationScreenTests` (E-17 sınır metni değişti) | `AfterSalesScreenTests` 17/17; `OrderOperationScreenTests` 12/12 (düzeltme 1c, birleşik ağaç, 127.0.0.1:58761) |
| **Düzeltme 1:** Integration TAM | TAM: CI | `WithdrawalDeclarationTests`, `CustomerOperationsTests`, `ItemRecordsTests` ve öteki sınıflar yerelde koşulmadı (sahip 2026-11-12) | CI yeşili merge ön koşulu |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes --include <değişen .cs>` (Web, UnitTests, IntegrationTests) | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS (2026-11-13, head `af9deb99` + main `9cf8d1f7` birleşik; CI run 43295039843 yeşil — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate) |
| Bulgu sayısı (tur 2) | 0. Tur 1'in dört bulgusu kapandı: B1 kargo ücreti satırları modülün kararı (`WithdrawalFeeUnitsAsync`, `OnWithdrawalPreview`; `…_B1` iki test), B2 toplu okuma + komut sayısı testi (`…_B2`), B3 satırlar kalemin adediyle (`OpenUnits`, `ClaimLimit`; `…_B3`), B4 onaysız gönderimde aşan/eski adet 303 (`…_B4`); K2 testle kapandı (`…_K2`) |
| Kabul kriterleri (tur 2) | 10/10 ✓ — KK1 içerik + kargo satırı (`…_KK1` ×2, `…_B1` ×2); KK2/KK3 (`…_KK2_KK3`); KK4 (`…_KK4`, `…_B4`, `…_K2`); KK5–KK10 (`…_KK5`…`…_KK10`); `09 §25` evet (döngüde sorgu: `…_B2`) |
| Test (tur 2, doğrulayıcı) | Build `-warnaserror` 0/0; format üç proje 0; Unit 295 + 1750 + 5461 yeşil; betik 4 dosya; Integration parçalı 270/270 (`AfterSalesScreenTests`, `OrderPageTests`, `OrderOperationScreenTests`, `ItemRecordsTests`, `WithdrawalDeclarationTests`, `RefundAmountTests`, `BranchFlowTests`, `CustomerOperationsTests`, `StatusCorrectionTests`, `OrderCorrectionTests`); TAM: CI |
| `12` satırları (tur 2) | VAL-A577, VAL-A688 FAIL → PASS; VAL-A581, VAL-C011, VAL-C020 → PASS; VAL-A072, VAL-A686 PASS kalır; öteki KP-22/KP-23 satırları ve tarayıcı/güvenlik satırları (A693 — D-1368; A881, A882, B072, B073) BEKLEMEDE (kanıtın yarısı başka task'ta ya da tarayıcıda) |
| Bloke-etmeyen not (tur 2) | 2 — K1 → D-1439 (karma seçimde son adımın tek sonuç cümlesi geri ödemenin başlangıcını tek andan söylüyor; F5 kapısı); K2 → D-1440 (çok kalemli beyan form sırasıyla ayrı anlarda yazılıyor — K-1107'nin "en küçük kalem kimliği" seçimi; K-1076 ekran testi yok; cayma geri ödemesini başlatan task) |
| Doğrulama durumu (tur 1) | ✗ FAIL (tur 1, 2026-11-12, head `926f70ff`; CI run 43262766424 yeşil) |
| Bulgu sayısı (tur 1) | 4 — B1 ORTA, B2/B3/B4 DÜŞÜK (hepsi S1) |
| Düzeltme gerekli mi | Evet. **B1 (ORTA, para; kriter 1):** E-18'in kargo ücreti satırı (`OrderScreenTexts.ShippingFeeText`) her fiziksel kalemde sabit koşullu cümle; seçime ve önceki beyanlara göre karar verilmiyor, ücretsiz kargolu siparişte ve K-1076'nın dışladığı hâlde de geri ödeme vaat ediyor (`04` 5.18.6; `07 UC-42` "kargo ücreti"; `02 §7.4.6`; K-789). E-17'nin `CancellationRefundsShippingFee` emsali gibi modül kararı gerekir; D-1340 kabul edilmiş risk değildir. **B2 (DÜŞÜK; `09 §20` m.4, `09 §25` "döngüde sorgu"):** `WithdrawalPageModel.LoadScreenAsync` açık kalem başına `ReadWithdrawalScreenAsync` çağırıyor; her çağrı siparişin işlem olgularını baştan yüklüyor (`OrderOperations.LoadAsync`) + kalem sorgusu + iade adresi; GET ve her POST yeniden yüklemesi N+1; komut sayısı testi yok. **B3 (DÜŞÜK; `04` 2.12.1.8, VAL-A881; D-1367 hedefi T513):** E-18 satırı "İşleme açık adet: X", E-19 "Ayıplı adet: en çok X" — üst sınır kalemin adediyle söylenmiyor; E-19'un adet alanı kalemin satırında değil, formda tek alan. **B4 (DÜŞÜK; `07 UC-43`, kriter 4, VAL-A577/A688):** onaysız `SubmitWithdrawal` açık adedi aşan ya da eskimiş `OriginalOpenQuantity` taşıyan gönderimde 303 yerine 200 son adım veriyor (ör. açık 2'de "Seçtiğiniz 3 ürün…"); E-17 bunu T512'de 303'e çevirmişti |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-1401 (alan kuralı 422'si yabancı kalemin 404'ünün önünde; `07 §2.3.3`; F5 kapısı); K2 → bu PR'ın düzeltme turu (E-18'de aynı kalemin iki satırı 422 ve çok kalemli bütün-ya-da-hiç için ekran testi yok; VAL-A581, VAL-A688) |

## Altyapı Değişiklikleri

Yok.

## Commit & PR

Dal `task/T513-cayma-ayip`; PR #558 (base `main`).

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | ~~Kargo ücreti satırı (04 5.18.6) koşullu sabit cümle~~ — **kapandı** (düzeltme 1, B1) | D-1340 ✅ | — |
| 2 | İade reddi kaydında reddin sebebi ve geri gönderim cümlesi (04 5.16.5) | D-1195 (hedef T528) | ret E-37'nin iade malı işlemiyle (T528) yazılır; sebep cümlesi ve testi reddi kuran task'ta |
| 3 | E-19'da birden çok adet alanlı satır varken ve seçim yokken tek `Quantity` alanı liste altında (04 2.12.1.8) | D-1412 (F5 kapısı) | `07 §5` `SubmitDefectClaimInput` tek alan taşır; satır başına alan istemci betiği ya da giriş modeli değişikliği ister |
| 4 | Alan kuralı 422'si yabancı kalemin 404'ünün önünde (K1) | D-1401 (F5 kapısı) | süzgeç sırası bütün ekranların ortak kalıbı (`07 §2.3.3`) |

## Notlar

⚠ kararlar (kurala en uygun seçenekle verildi):

- Karışık seçimde (teslim edilmiş fiziksel + hizmet) son adımın sonuç cümlesi "malın firmaya ulaşmasından" başlangıcını söyler; geç başlayan süre müşterinin aleyhine yanlış beklenti yaratmaz.
- K-1202 karşılaştırması `ReadWithdrawalScreenAsync` ile yazım işleminin içinde, ilk kilitten önce okunur; adedin kesin denetimi modülün kilit altındaki yazıcısındadır.
- Yeni ayıp talebinde tek `Quantity` alanı (`07 §5`); düzeltme 1'den beri alan seçili ya da tek alanlı kalemin satırında açılır, birden çok alanlı satırda seçim yokken liste altında (D-1412); kalem başına üst sınır modülde, aşım 303.
- Düzeltme 1: E-18'de onaysız gönderimin açık adet karşılaştırması IBAN 422'sinden önce yapılır (aşan adet istek uygulanamaz, 303 kök; E-17 kalıbı).
- D-880'in sipariş sayfası payı ("sorun bildir" yerine iletişim formu): E-19 Z-18 dolunca 303 kök + `OperationClosed` sebebi verir, sebep metni iletişim formunu anar; sayfada doğrudan iletişim formu bağlantısı F5 kapısına ileri atıldı, satır T528 (panel) payı için açık. D-776'nın T513 payı yerinde kapatıldı.
- E-17'nin (T512) dönüş sebebi metni birleştirmede `OrderScreenTexts`'in sözcüklerine geçti (tek ev); T512 testleri metni sabitle sınamıyor, `OrderOperationScreenTests` yeşil.
- Kalem satırındaki ayıp rozeti `04` 5.16.5'e göre yalnız talep kaydındadır; kalem kendi rozetini taşımaz (§2.5.2).
- D-1195'in iade reddi sebebi payı T528'e ileri atıldı (satır açık kaldı, hedef "T528 (ret sebebi); T513 ✓").
