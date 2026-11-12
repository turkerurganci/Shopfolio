# T513 — Cayma beyanı ve ayıp talebi ekranları (E-18, E-19)

## Yapılan İşler

- E-18 (`{kök}/cayma`) ve E-19 (`{kök}/ayip-talebi`) sayfaları üye ve erişim anahtarı kökleri için kuruldu; üç POST işleyicisi (`SubmitWithdrawal`, `SubmitDefectClaim`, `ReopenDefectClaim`) `[GatedOperation]` taşır ve modülün yazıcılarını çağırır, kuralı yeniden yazmaz (D-153, D-776).
- Kök çözümü sipariş sayfasıyla ortak (`OrderRoots`): ekran, sayfanın açık olmadığı yerde açılamaz; dönüş adresi ekranın adresinden son parça atılarak yazıldığı gibi kurulur (`RootOf`).
- Sipariş sayfası dönüş sebebini (`OperationClosed`, `QuantityExceedsOpen`) işlemlerin yerinde söyler (`OrderScreenTexts.ReasonOf`).
- Cayma ekranı için modül değerleri genişledi: gönderme süresinin son günü, geri ödeme süresi (Z-16) ve başlangıcı (teslimden sonra malın firmaya ulaşması, önce beyan), ödeme yolu (`WithdrawalNotes`, `WithdrawalScreen`); ayıp talebi bilgisi ilk açıklamayı taşır (`DefectClaimInfo.Description`).
- Ekran metinleri `OrderScreenTexts`'te; süreler metne yazılmaz, modülden gelir (K-785).
- Tur 3 (bu tur): ekran testlerinin tohumu başlangıç verisinin ayar satırını yerinde doldurur (önceki tohum ikinci satır açmaya çalışıp `return_address_all_or_none` kısıtına takılıyordu); üç null-forgiving (`!`) kaldırıldı (`NullForgivingSourceScanTests`); ekran metinleri ve kök için birim testleri eklendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Orders/AfterSalesScreenModels.cs` (yeni), `_WithdrawalScreen.cshtml`, `_DefectClaimScreen.cshtml`, `MemberWithdrawal.cshtml`, `AccessWithdrawal.cshtml`, `MemberDefectClaim.cshtml`, `AccessDefectClaim.cshtml` (yeni)
- `src/Shopfolio.Web/Pages/Orders/OrderRoots.cs`, `OrderScreenTexts.cs` (yeni); `OrderPageModels.cs`, `_OrderPage.cshtml`
- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.ItemRecords.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`
- `src/Shopfolio.Web/Modules/Ordering/OrderOperations.cs`, `Contracts/WithdrawalRequests.cs`
- `tests/Shopfolio.IntegrationTests/Files/AfterSalesScreenTests.cs` (yeni, 9 test), `tests/Shopfolio.UnitTests/Pages/OrderScreenTextsTests.cs` (yeni, 11 test)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET {kök}/cayma` içerik; kapalıysa 303 kök + sebep | ✓ | `AfterSalesScreenTests.TheWithdrawalScreen_ShowsTheItemsUnitsReturnAddressAndRefund_AndAClosedOneIs303WithTheReason_KK1` |
| 2 | Onaysız 200 son adım; onaylı 303 kök | ✓ | `…TheUnconfirmedPost_IsTheLastStepAndWritesNothing_AndTheConfirmedOneFreezesTheReturnAddress_KK2_KK3` |
| 3 | Kalem başına tarih + adet; iade adresi donar | ✓ | aynı test (adres değiştirildikten sonra kayıt eski adresi tutar) |
| 4 | Üç 422, pencere/hak 303, açık adet farkı 303, yabancı kalem 404 | ✓ | `…TheWithdrawalsThree422s_TheClosedWindow_TheStaleUnits_AndTheForeignItem_WriteNothing_KK4` |
| 5 | Kısmi seçimin tekrarı ikinci cayma olmaz | ✓ | `…TheRepeatOfAPartialSelection_IsNoSecondWithdrawal_But303WithTheReason_KK5` |
| 6 | Yeni talep: açık kalemler, varsayılan 1, üst sınır, iade adresi; yeniden açma: talep + Çözüldü rozeti | ✓ | `…TheDefectClaimScreen_NewClaimHasDefaultOneAndTheLimit_AndTheReopeningShowsTheResolvedClaim_KK6` |
| 7 | Üç 303 (açık değil, Çözüldü değil, Z-18 doldu); 404 (kök, yabancı, talep yok) | ✓ | `…TheDefectClaimScreen_Is303WhenNotOpenNotResolvedOrPastZ18_And404ForAForeignItemOrNoClaim_KK7` |
| 8 | Tek talep Açık; iade adresi talepte; onay adımı yok | ✓ | `…SubmitDefectClaim_OpensTheOneClaimAsAcikWithTheReturnAddress_WithoutAConfirmationStep_KK8` |
| 9 | Üç 422, üç 303, yabancı kalem 404 | ✓ | `…SubmitDefectClaim_Three422s_Three303s_AndTheForeignItems404_KK9` |
| 10 | Z-18 içinde Açık + müşteri satırı; dışında 303; talep yoksa 404 | ✓ | `…ReopenDefectClaim_InsideZ18MakesItAcikWithTheCustomersLine_OutsideIs303_AndNoClaimIs404_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: sayfa içeriği; kapalı hâl | ✓ | `AfterSalesScreenTests …_KK1`; `OrderScreenTextsTests.TheRefund_NamesItsWayAndItsStart_KK1`, `TheSendingPeriod_…_KK1`, `TheRootOfAScreen_…_KK1` |
| KK2: onaysızda kayıt yok | ✓ | `…_KK2_KK3` (`WithdrawalCountAsync` = 0 son adımda); `TheResultSentence_CountsTheUnitsChosen_KK2` |
| KK3: iade adresi donar | ✓ | `…_KK2_KK3` (`SetReturnAddressLineAsync` sonrası kayıt "Depo Sokak 3") |
| KK4: üç 422 sebebi, pencere, 404 | ✓ | `…_KK4`; K-1202 karşılaştırması `AfterSalesScreenModels.cs` `SubmitWithdrawal` (`now.OpenQuantity != shown`) |
| KK5: ikinci gönderim cayma açmaz | ✓ | `…_KK5` |
| KK6: iki hâl, varsayılan adet | ✓ | `…_KK6`; `FurtherInformation_GoesToTheFirmsEmail_WhenThereIsOne_KK6` |
| KK7: üç 303 sebebi, 404 | ✓ | `…_KK7`; `TheTwoReturnCodes_HaveTheirOwnWords_KK4_KK7` |
| KK8: tek talep, Açık | ✓ | `…_KK8` (durum, adet, açıklama, iade adresi) |
| KK9: üç 422, üç 303, 404 | ✓ | `…_KK9` |
| KK10: Z-18 içi ve dışı; müşteri satırı | ✓ | `…_KK10`; yeniden açma açıklama almaz (`ReopenDefectClaimInput` yalnız adet) |
| 09 §25: tarama ve mimari testleri | ✓ | Unit tam koşu 7378/7378 (`OperationSourceScanTests`, `FormSourceScanTests`, `ModelBindingRuleTests`, `NullForgivingSourceScanTests`, Consistency dahil) |
| Güvenlik: POST'lar kapılı, antiforgery, yetki | ✓ | üç işleyici `[GatedOperation]`; kök çözümü sipariş sayfasıyla ortak (`OrderRoots.ResolveMemberAsync`/`ResolveAccessAsync`), yönetici oturumu ve yabancı sipariş 404; ham SQL yok; null-forgiving yok |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | yeni tarama/bekçi istisnası eklenmedi |
| Sık FAIL (2) tarama çağrı biçimi | ✓ | yeni tarama yok; `OperationSourceScan` aynı dosyadaki çağrıyı izler, işleyiciler `AfterSalesScreenModels.cs`'te |
| Sık FAIL (3) 06 varsayılanı / FK dizini | ✓ | şema değişikliği ve göç yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | sayı/etiket kuralı değişmedi |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | `09` değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --max-threads 3` | 7378/7378 |
| Integration (T513) | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/… --filter-class "*AfterSalesScreenTests"` | 9/9 |
| Integration TAM | bekliyor | ad alanı parçalarıyla | — |
| Format | ✓ | `dotnet format <proje> --no-restore` (Web, UnitTests, IntegrationTests) | değişiklik yok |

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

## Notlar

⚠ kararlar (kurala en uygun seçenekle verildi):

- Karışık seçimde (teslim edilmiş fiziksel + hizmet) son adımın sonuç cümlesi "malın firmaya ulaşmasından" başlangıcını söyler; geç başlayan süre müşterinin aleyhine yanlış beklenti yaratmaz.
- K-1202 karşılaştırması `ReadWithdrawalScreenAsync` ile yazım işleminin içinde, ilk kilitten önce okunur; adedin kesin denetimi modülün kilit altındaki yazıcısındadır.
- Yeni ayıp talebinde tek `Quantity` alanı; kalem başına üst sınır modülde, aşım 303.
- D-880'in sipariş sayfası payı ("sorun bildir" yerine iletişim formu): E-19 Z-18 dolunca 303 kök + `OperationClosed` sebebi verir, sebep metni iletişim formunu anar; sayfada doğrudan iletişim formu bağlantısı F5 kapısına ileri atıldı, satır T528 (panel) payı için açık. D-776'nın T513 payı yerinde kapatıldı.
