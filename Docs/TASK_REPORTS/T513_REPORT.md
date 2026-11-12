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

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Orders/AfterSalesScreenModels.cs` (yeni), `_WithdrawalScreen.cshtml`, `_DefectClaimScreen.cshtml`, `MemberWithdrawal.cshtml`, `AccessWithdrawal.cshtml`, `MemberDefectClaim.cshtml`, `AccessDefectClaim.cshtml` (yeni)
- `src/Shopfolio.Web/Pages/Orders/OrderRoots.cs`, `OrderScreenTexts.cs` (yeni); `OrderPageModels.cs`
- `src/Shopfolio.Web/Pages/Components/ItemRecordLabels.cs`, `src/Shopfolio.Web/Pages/Shared/_ItemRecords.cshtml`
- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.ItemRecordList.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`, `Contracts/ItemRecordFacts.cs`
- `src/Shopfolio.Web/Modules/Ordering/OrderOperations.cs`, `Contracts/WithdrawalRequests.cs`
- `tests/Shopfolio.IntegrationTests/Files/AfterSalesScreenTests.cs` (yeni, 11 test), `tests/Shopfolio.UnitTests/Pages/OrderScreenTextsTests.cs` (yeni, 12 test durumu)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET {kök}/cayma` içerik; kapalıysa 303 kök + sebep | ✓ | `AfterSalesScreenTests.TheWithdrawalScreen_ShowsTheItemsUnitsReturnAddressAndRefund_AndAClosedOneIs303WithTheReason_KK1`, `…TheWithdrawalScreen_ShowsTheConditionalExemptionsLine_OnlyOnTheItemThatHasIt_KK1` |
| 2 | Onaysız 200 son adım; onaylı 303 kök, tarih ve adres kökte | ✓ | `…TheUnconfirmedPost_IsTheLastStepAndWritesNothing_AndTheConfirmedOneFreezesTheReturnAddress_KK2_KK3` |
| 3 | Kalem başına tarih + adet; iade adresi donar | ✓ | aynı test (adres değiştirildikten sonra kayıt eski adresi tutar) |
| 4 | Üç 422, pencere/hak 303, açık adet farkı 303, yabancı kalem 404 | ✓ | `…TheWithdrawalsThree422s_TheClosedWindow_TheStaleUnits_AndTheForeignItem_WriteNothing_KK4` |
| 5 | Kısmi seçimin tekrarı ikinci cayma olmaz | ✓ | `…TheRepeatOfAPartialSelection_IsNoSecondWithdrawal_But303WithTheReason_KK5` |
| 6 | Yeni talep: açık kalemler, varsayılan 1, üst sınır, iade adresi; yeniden açma: talep + Çözüldü rozeti | ✓ | `…TheDefectClaimScreen_NewClaimHasDefaultOneAndTheLimit_AndTheReopeningShowsTheResolvedClaim_KK6` |
| 7 | Üç 303 (açık değil, Çözüldü değil, Z-18 doldu); 404 (kök, yabancı, talep yok) | ✓ | `…TheDefectClaimScreen_Is303WhenNotOpenNotResolvedOrPastZ18_And404ForAForeignItemOrNoClaim_KK7` |
| 8 | Tek talep Açık; iade adresi talepte; onay adımı yok | ✓ | `…SubmitDefectClaim_OpensTheOneClaimAsAcikWithTheReturnAddress_WithoutAConfirmationStep_KK8` |
| 9 | Üç 422, üç 303, yabancı kalem 404 | ✓ | `…SubmitDefectClaim_Three422s_Three303s_AndTheForeignItems404_KK9` |
| 10 | Z-18 içinde Açık + müşteri satırı; dışında 303; talep yoksa 404 | ✓ | `…ReopenDefectClaim_InsideZ18MakesItAcikWithTheCustomersLine_OutsideIs303_AndNoClaimIs404_KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki turların işi dahil, `11` T513 kabul kriterleri ve doğrulama kontrol listesi, `12 §6.1` task çıkışı ve sık FAIL nedenleri diff'te madde madde tarandı (birleşik ağaç, head `98dd9836` üstüne yalnız belge).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: sayfa içeriği; kapalı hâl | ✓ | `AfterSalesScreenTests …_KK1` (iki test; koşullu istisna satırı yalnız istisnalı kalemde); `OrderScreenTextsTests.TheRefund_NamesItsWayAndItsStart_KK1`, `TheSendingPeriod_NamesTheDaysAndTheLastDay_KK1`, `TheRootOfAScreen_KeepsTheCaseOfTheKey_KK1` |
| KK2: onaysızda kayıt yok | ✓ | `…_KK2_KK3` (`WithdrawalCountAsync` = 0 son adımda; onaylıda kökte iade adresi başlığı ve tarih); `TheResultSentence_CountsTheUnitsChosen_KK2` |
| KK3: iade adresi donar | ✓ | `…_KK2_KK3` (`SetReturnAddressLineAsync` sonrası kayıt "Depo Sokak 3"); `TheWithdrawalRecord_NamesTheReturnAddressWrittenIntoIt_KK2_KK3` |
| KK4: üç 422 sebebi, pencere, 404 | ✓ | `…_KK4`; K-1202 karşılaştırması `AfterSalesScreenModels.cs` `SubmitWithdrawal` (`now.OpenQuantity != shown`) |
| KK5: ikinci gönderim cayma açmaz | ✓ | `…_KK5` |
| KK6: iki hâl, varsayılan adet | ✓ | `…_KK6`; `FurtherInformation_GoesToTheFirmsEmail_WhenThereIsOne_KK6` |
| KK7: üç 303 sebebi, 404 | ✓ | `…_KK7`; `TheTwoReturnCodes_HaveTheirOwnWords_KK4_KK7` |
| KK8: tek talep, Açık | ✓ | `…_KK8` (durum, adet, açıklama, iade adresi; kalemin satırında talep kaydı Açık rozetiyle — `04` 5.16.5) |
| KK9: üç 422, üç 303, 404 | ✓ | `…_KK9` |
| KK10: Z-18 içi ve dışı; müşteri satırı | ✓ | `…_KK10`; yeniden açma açıklama almaz (`ReopenDefectClaimInput` yalnız adet) |
| 09 §25: tarama ve mimari testleri | ✓ | Unit ilgili sınıflar 2127/2127 (`*SourceScan*`, `*ModelBindingRule*`, `*Architecture*` dahil); Integration uç taraması `PageFormPatternTests` (kapalı ilk bölüm listesi), `ForgeryProtectionTests`, `SecurityHeadersTests`, `SessionAccessTests`, `WritingEndpointAudit` 92/92 |
| Güvenlik: POST'lar kapılı, antiforgery, yetki | ✓ | üç işleyici `[GatedOperation]` (`AfterSalesScreenModels.cs`); kök çözümü sipariş sayfasıyla ortak (`OrderRoots.ResolveMemberAsync`/`ResolveAccessAsync`), yönetici oturumu ve yabancı sipariş 404; ham SQL yok; null-forgiving yok; yeni dış bağımlılık ve sır yok |
| Devredilen backlog kalemleri (`12 §6.1`) | ✓ | D-776 T513 payı kapandı; D-1196 kapandı (`…_D1196`); D-1195 T513 payı karşılandı (KK2/KK3 ve KK8 testleri), iade reddi sebebi T528'e ileri atıldı (satırda gerekçeyle); D-880 T513 payı (`…_KK7`, sebep metni iletişim formunu anar) satırda yazılı |
| Birleştirme: aynı işi yapan iki yol kalmadı | ✓ | `OrderRoots` tek dosya; sebep metni tek evi `OrderScreenTexts.ReasonOf` (T512'nin sabitleri silindi); `OrderOperationScreenTests` + `OrderPageTests` yeşil |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | yeni tarama/bekçi istisnası eklenmedi |
| Sık FAIL (2) tarama çağrı biçimi | ✓ | yeni tarama yok; `OperationSourceScan` aynı dosyadaki çağrıyı izler, işleyiciler `AfterSalesScreenModels.cs`'te |
| Sık FAIL (3) 06 varsayılanı / FK dizini | ✓ | şema değişikliği ve göç yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | sayı/etiket kuralı değişmedi; test sayıları rapor ve changelog parçasında aynı (11 Integration, 12 Unit) |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | `09` değişmedi |
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
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes --include <değişen .cs>` (Web, UnitTests, IntegrationTests) | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

Yok.

## Commit & PR

Dal `task/T513-cayma-ayip`.

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Kargo ücreti satırı (04 5.18.6) koşullu sabit cümle; seçime göre hesap yok | D-1340 | modülde kargo ücretinin cayma payını hesaplayan sorgu yok |
| 2 | İade reddi kaydında reddin sebebi ve geri gönderim cümlesi (04 5.16.5) | D-1195 (hedef T528) | ret E-37'nin iade malı işlemiyle (T528) yazılır; sebep cümlesi ve testi reddi kuran task'ta |

## Notlar

⚠ kararlar (kurala en uygun seçenekle verildi):

- Karışık seçimde (teslim edilmiş fiziksel + hizmet) son adımın sonuç cümlesi "malın firmaya ulaşmasından" başlangıcını söyler; geç başlayan süre müşterinin aleyhine yanlış beklenti yaratmaz.
- K-1202 karşılaştırması `ReadWithdrawalScreenAsync` ile yazım işleminin içinde, ilk kilitten önce okunur; adedin kesin denetimi modülün kilit altındaki yazıcısındadır.
- Yeni ayıp talebinde tek `Quantity` alanı; kalem başına üst sınır modülde, aşım 303.
- D-880'in sipariş sayfası payı ("sorun bildir" yerine iletişim formu): E-19 Z-18 dolunca 303 kök + `OperationClosed` sebebi verir, sebep metni iletişim formunu anar; sayfada doğrudan iletişim formu bağlantısı F5 kapısına ileri atıldı, satır T528 (panel) payı için açık. D-776'nın T513 payı yerinde kapatıldı.
- E-17'nin (T512) dönüş sebebi metni birleştirmede `OrderScreenTexts`'in sözcüklerine geçti (tek ev); T512 testleri metni sabitle sınamıyor, `OrderOperationScreenTests` yeşil.
- Kalem satırındaki ayıp rozeti `04` 5.16.5'e göre yalnız talep kaydındadır; kalem kendi rozetini taşımaz (§2.5.2).
- D-1195'in iade reddi sebebi payı T528'e ileri atıldı (satır açık kaldı, hedef "T528 (ret sebebi); T513 ✓").
