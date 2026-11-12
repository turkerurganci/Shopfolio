# T512 — İptal ve gecikme feshi ekranı, geri ödeme IBAN girişi (E-17, E-16 IBAN formu)

**Faz:** F5 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İptal ekranı (UC-38…UC-40, E-17):** `GET {kök}/iptal` iki kökte (üye `MemberCancellationModel`, erişim anahtarı `AccessCancellationModel`). Bekliyor'daki siparişte sipariş iptalini (`CancelOrder`), ödenmişte kalem iptalini (`CancelOrderItems`) açar; kalem başına açık adet, varsayılan ve üst sınır sunucudandır (`CustomerScreenItem`), dijital ve adedi bir kalemde adet alanı yoktur, IBAN alanı yalnız ödenmiş havalededir. Onaysız gönderim son adımı 200 verir, yazmaz; onaylı gönderim 303 köke.
- **Gecikme feshi ekranı (UC-41):** `GET {kök}/gecikme-feshi` ve `SubmitLateDeliveryTermination`; kalem ve adet alanı yok sayılır, bütün teslim edilmemiş fiziksel kalemlerin açık adetleri kapsanır, kargo ücreti hep döner; havalede IBAN boş ya da geçersizse 422. Müşterinin IBAN'ı fesih kaydında durur, IBAN isteği açılmaz (D-873).
- **IBAN formu (UC-35, E-16):** sipariş sayfası açık IBAN alanını kalemin satırında, geç gelen ödemenin kalemsiz alanını "müşteriden beklenen"de çizer (`_OrderIbanForm.cshtml`); `SaveRefundIban` isteği karşılar ya da işlenene dek yerinde düzeltir, boşlukları yok sayar, 303 kök `Saved`. Kart iadesi gerçekleşmeyip havale yolu açıldıysa geri ödeme havaleye döner, e-posta üretmez.
- **Tek kural evi (para):** işlemler sayfadan `OperationRunner.RunAsync` ile açılır, modülde `ICustomerOperations` (`CustomerOperations`) → `IOrderTransitions` ile kapı altında işler. Geri ödeme tutarı kayıt yazılırken AfterSales'te hesaplanır; son adımın kargo ücreti cümlesi `ShippingFeeRefunds.OnCancellationPreview`'dan (`IAfterSalesModule.CancellationRefundsShippingFee`), süre `IAfterSalesModule.RefundPeriodDays`'ten gelir — sayfa tutar hesaplamaz. Çok kalemli iptalde satırlar azalan uuid sırasıyla yazılır; kargo ücretini tetiği karşılayan son kayıt taşır (K-789, K-1107).
- **Mimari:** işaretsiz işlem POST'unu yakalayan kural `OperationHandlerMarkTests` (D-153); işlem taraması `OperationRunner` için `.RunAsync(` çağrısını ister (D-154 (a)).
- **Öteki:** havale bloğunda "IBAN'ı kopyala" (D-1258; paylaş düğmesinin betiksiz-yol kalıbı); kalemsiz IBAN alanı süzgeci tek sorguya indi (`IPaymentModule.OrderRefundIdsAsync`; `09 §20` m.4).

## Etkilenen Modüller / Dosyalar

- Ordering: `CustomerOperations.cs`, `Contracts/ICustomerOperations.cs`, `CustomerScreen.cs`, `CustomerScreenOperation.cs`, `CustomerOperationOutcome.cs`, `OrderTransitions.LateDeliveryTermination.cs`, `OrderTransitions.CustomerCancellation.cs`, `OrderingModule.cs`
- AfterSales: `AfterSalesModule.RefundLine.cs`, `AfterSalesModule.Cancellations.cs`, `ShippingFeeRefunds.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`, `Contracts/RefundLine.cs`
- Payment: `Contracts/IPaymentModule.cs`, `PaymentModule.cs` (`OrderRefundIdsAsync`)
- Sayfalar: `Pages/Orders/OrderScreenModels.cs`, `OrderPageModels.cs`, `_OperationScreen.cshtml`, `_OrderIbanForm.cshtml`, `_OrderPage.cshtml`, dört ekran görünümü; `wwwroot/js/common.js`
- Testler: `IntegrationTests/Ordering/CustomerOperationsTests.cs`, `IntegrationTests/Files/OrderOperationScreenTests.cs`, `IntegrationTests/Files/OrderPageTests.cs`, `UnitTests/AfterSales/CancellationPreviewTests.cs`, `UnitTests/Architecture/OperationHandlerMarkTests.cs`, `OperationSourceScan(Tests).cs`, `Support/ApplicationEnums.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `SaveRefundIban` yazar / yerinde düzeltir, boşluk yok sayılır, 303 `Saved` | ✓ | `CustomerOperationsTests.SaveRefundIban_CorrectsTheIbanOnFileInPlace_IgnoringBlanks_…_KK1`; `OrderOperationScreenTests.SaveRefundIban_Is303WithSaved_…_KK1_KK2` |
| 2 | UC-35 422 (biçim/sağlama, alan kapalı), 404 yabancı kalem | ✓ | `CustomerOperationsTests.SaveRefundIban_ABadIban_AClosedField_AndAForeignItem_…_KK2`; ekran testi `…_KK1_KK2` |
| 3 | Havale yoluna dönüş, e-posta yok | ✓ | `CustomerOperationsTests.SaveRefundIban_AnswersTheFailedCardRefundsTransferPath_…_AndNoEmailIsWritten_KK3` |
| 4 | İki adres, kapsam, geri ödeme, son adım; Bekliyor → sipariş iptali, ödenmiş → kalem iptali | ✓ | `CustomerOperationsTests.TheCancellationAddress_OfAnUnpaidOrder_…_KK4_KK5`, `…_OfAPaidOrder_…_KK4_KK5`, `TheTerminationAddress_…_KK4_KK6`; `OrderOperationScreenTests.TheCancellationAddress_OfAnUnpaidOrder_…_KK4_KK7` |
| 5 | Üç değer sunucudan; dijital/adedi birde adet alanı yok; IBAN alanı yalnız ödenmiş havalede | ✓ | `…_TheServersThreeValues_TheQuantityFieldOnlyOnAnItemOfSeveralUnits_KK4_KK5`; `TheIbanField_IsDrawnOnlyOnThePaidTransferOrder_KK5` |
| 6 | Kapalı işlem 303 kök + sebep (GET ve POST), kök tutmuyorsa 404 | ✓ | `OrderOperationScreenTests.AClosedOperation_Is303ToTheRootWithItsReason_OnTheScreenAndOnThePost_AndAnUnknownRootIs404_KK6`; `Terminate_BeforeTheLimit_IsClosed_…_KK6` |
| 7 | `CancelOrder` onaysız 200 yazmaz; onaylı 303, ayırmalar serbest | ✓ | `CustomerOperationsTests.CancelOrder_OnPlacedAndPending_CancelsTheOrder_AndFreesTheReservations_…_KK7`; ekran `…_KK4_KK7` |
| 8 | `CancelOrderItems` 422 (seçimsiz, adet, IBAN); aşım/eski açık adet 303, uygulanmaz | ✓ | `OrderOperationScreenTests.TheItemCancellation_AQuantityUnderOne_ARepeatedItemRow_AndNoIbanOnTheTransfer_Are422_…_KK8_D155`, `…_AboveTheOpenUnits_OrOnAStaleOpenQuantity_Is303_…_KK6_KK8`; `CustomerOperationsTests.CancelOrderItems_…_KK8` (3 test) |
| 9 | Kartta kendiliğinden, havalede IBAN'la firma; bütün fiziksel adetler → kargo ücreti | ✓ | `CancelOrderItems_OnTheCard_StartsTheRefundItself_AndEveryPhysicalUnitCoveredPaysTheShippingFeeBack_KK9` (iki sıra), `…_OfSomeUnitsOnly_PaysNoShippingFee_KK9`, `…_OnTheTransfer_KeepsTheIbanOnTheRecords_…_KK9`; `CancellationPreviewTests` |
| 10 | Fesih bütün açık adetleri kapsar, alanları yok sayar, kargo hep döner, IBAN 422 | ✓ | `Terminate_CoversEveryOpenUnit_…_KK10`, `Terminate_OnThePaidTransfer_WithoutAValidIban_…_KK10`, `…_D873_KK10`; `OrderOperationScreenTests.TheTermination_IgnoresTheItemAndQuantityFields_…_KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin işi dahil dalın bütün diff'i (`origin/main...HEAD`) madde madde denetlendi; `sik-fail-nedenleri.md` okundu.

| Madde (KK / 12 / backlog / sık FAIL) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 boşluklu IBAN, yerinde düzeltme | ✓ | `SaveRefundIban_CorrectsTheIbanOnFileInPlace_IgnoringBlanks_…_KK1` |
| KK2 iki 422 sebebi ve 404 | ✓ | `SaveRefundIban_ABadIban_AClosedField_AndAForeignItem_…_KK2`; ekran `…_KK1_KK2` |
| KK3 havale yoluna dönüş, e-posta yok | ✓ | `…_AndNoEmailIsWritten_KK3` |
| KK4 iki adres, iki işlem | ✓ | `TheCancellationAddress_OfAnUnpaidOrder…`, `…_OfAPaidOrder…`, `TheTerminationAddress…` |
| KK5 üç değer; alan görünürlüğü | ✓ | `…_TheServersThreeValues_…_KK4_KK5`, `TheIbanField_IsDrawnOnlyOnThePaidTransferOrder_KK5` |
| KK6 sebep kodlu 303 ve 404 | ✓ | `AClosedOperation_Is303ToTheRootWithItsReason_…_KK6` |
| KK7 onaysızda kayıt yok; onaylıda iptal | ✓ | `TheCancellationAddress_OfAnUnpaidOrder_OffersTheOrdersCancellation_TheUnconfirmedPostIsTheLastStep_AndTheConfirmedOneCancels_KK4_KK7` |
| KK8 bütün istek reddi | ✓ | `CancelOrderItems_AQuantityAboveTheOpenUnits_OrAnOpenQuantityOtherThanTheShownOne_RefusesTheWholeRequest_KK8` |
| KK9 iki hat; kargo iadesi | ✓ | `…_OnTheCard_…_KK9`, `…_OnTheTransfer_…_KK9`, `…_OfSomeUnitsOnly_PaysNoShippingFee_KK9` |
| KK10 alan yok sayma; kargo iadesi | ✓ | `TheTermination_IgnoresTheItemAndQuantityFields_CoversEveryOpenUnit_…_KK10`, `Terminate_CoversEveryOpenUnit_…_KK10` |
| VAL-A454 `InvalidIban` 422, kaydedilmez | ✓ | `…_KK2` (iki katman), `CancelOrderItems_OnThePaidTransfer_WithoutAValidIban_…_KK8`, `Terminate_…_WithoutAValidIban_…_KK10` |
| VAL-A490 `RefundIbanNotRequested` 422 | ✓ | `SaveRefundIban_ABadIban_AClosedField_…_KK2` (kapalı alan) |
| VAL-A554 `Saved` bildirimi | ✓ (uç yanıtı) | `SaveRefundIban_Is303WithSaved_…`; tarayıcı gözlemi yapılmadı (⚠ 3) |
| VAL-A577 `QuantityExceedsOpen` bütün istek, 303 | ✓ | `…_AboveTheOpenUnits_OrOnAStaleOpenQuantity_Is303WithTheReason_…_KK6_KK8` |
| VAL-A581 aynı kalem iki satır 422, hiçbiri işlenmez | ✓ | `…_ARepeatedItemRow_…_Are422_AndWriteNothing_KK8_D155` (`ItemRows.RejectRepeated`) |
| VAL-A686 kapalı ekran 303, 404 değil | ✓ | `AClosedOperation_Is303ToTheRootWithItsReason_…_KK6`, `TheTerminationAddress_IsClosedBeforeTheLimit_…` |
| VAL-A688 açık adedi aşan istek bütünüyle red | ✓ | `CancelOrderItems_AQuantityAboveTheOpenUnits_…_KK8` |
| VAL-A692 fesihte adet yok sayılır | ✓ | `TheTermination_IgnoresTheItemAndQuantityFields_…_KK10` |
| VAL-A693 [güvenlik] sahiplik kökten, yabancı kalem 404 | ✓ | `…_AnUnknownRootIs404_KK6`, `CancelOrderItems_AnItemOfAnotherOrder_IsNotFound_…_KK6_KK8`, `SaveRefundIban_…_AndAForeignItemIs404_…` |
| VAL-A895 fesih bütün açık adetler, kargo hep | ✓ | `Terminate_CoversEveryOpenUnitOfTheUndeliveredPhysicalItems_AndPaysTheShippingFeeBack_KK10` |
| VAL-B071 [tarayıcı] 320'de E-17 | — | Tarayıcı gözlemi yapılmadı; doğrulamaya (⚠ 3) |
| `09 §25` / güvenlik taraması | ✓ | Yeni ham SQL, gizli bilgi, kimlik doğrulamasız uç yok; işlem POST'ları antiforgery + `[GatedOperation]`; sahiplik kökten (`FindOrderAsync`) |
| Backlog: hedefi T512 olan kalemler | ✓ | D-153, D-154, D-155, D-792, D-803, D-873, D-1194, D-1258 kapandı; D-819, D-1196 T512 payı; D-1197, D-1259 T528'e gerekçeyle (⚠ 2) |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `OperationHandlerMarkTests` muafiyeti tek işleyici adıyla, ölü muafiyet bulgu (`ANamedExemptionThatNoLongerExists_IsAFinding`) |
| Sık FAIL 2: tarama çağrı biçimi kaçırıyor | ✓ | İşaret yansımayla okunur (öznitelik türü; yazım biçiminden bağımsız); `Gate()` için iki yeni ihlal satırı; kalan darlık D-1285 |
| Sık FAIL 3: DB default / FK dizini | ✓ | Şema ve göç değişmedi |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | Kural/sayı değişikliği yok; enum dosyaları `ApplicationEnums` listesinde |
| Sık FAIL 5: belge kuralını değiştiren 09 metni | ✓ | `09` ve öteki kural belgeleri değişmedi |
| Tek kural evi (para) | ✓ | Sayfa tutar hesaplamaz; kargo ön izlemesi `ShippingFeeRefunds.OnCancellationPreview`, süre `AfterSalesPeriodRules` üzerinden |
| Döngüde sorgu (`09 §20` m.4) | ✓ | Kalemsiz IBAN alanı süzgeci tek sorgu (`OrderRefundIdsAsync`); çok kalemli iptal satır başına modül işlemi (kural tek kalemli geçişte, K-1202 işlem içinde bütün-ya-da-hiç) |

## Test Sonuçları

| Tür | Sonuç |
|---|---|
| Derleme | Uyarısız, hatasız (birleşik ağaç, origin/main birleştirildi) |
| Unit | 7222/7222 (filtresiz TAM; ilk koşuda yük kaynaklı tek regex zaman aşımı, ikinci koşu yeşil) + `OperationHandlerMarkTests` 4/4 |
| Integration | TAM ad alanı parçalarıyla — sonuç PR açıklamasında |
| `dotnet format --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T512-iptal-fesih-iban`
- **Commit:** PR başlığına ve dal geçmişine bakınız
- **PR:** PR açıklamasında
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | — | — |

## ⚠ Kolay kararlar (yapımda verildi)

1. **D-155'in örnek sayfa yarısı:** alan kuralı ilk gerçek ekranda (`CancellationPageModel`) gösterildi ve 422 testi yazıldı; T217 örnek sayfası değiştirilmedi — kopyalanacak örnek artık gerçek ekrandır.
2. **D-1197 / D-1259 hedefi T528'e:** müşteri dijital kalemi iptal edemez (`CancelDigitalItem` kapalı, `02` sözlük "İptal"); T512'nin hiçbir yolu ödemesi onaylı dijital kalemi kapatmaz. Kapanmış ödenmiş dijital kalem yalnız firmanın kalem iptaliyle doğar (UC-129, T528).
3. **Tarayıcı satırları (VAL-B071, VAL-A554'ün tarayıcı yarısı):** yapımda tarayıcı gözlemi yapılmadı; PASS sayılmadı, doğrulamaya bırakıldı.
4. **D-1281 (aday):** firmanın bütün sipariş iptalinde kargo ücretinin satıra düşmeme olasılığı T528'e kayıtlandı; T512'nin müşteri yolu azalan uuid sırasıyla yazar.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Firma bütün iptalinde kargo ücreti sırası (aday) | D-1281 (T528) | firma iptali ekranı T528'in |
| 2 | UC-40 çok kalemde B-7 kalem başına | D-1282 | bildirim belgesinin okuması |
| 3 | IBAN alanlarının kenarları (kart hattı açılışı, aynı IBAN çok satır) | D-1283 | belge okuması + test |
| 4 | Kalemsiz IBAN süzgecinin küme testi | D-1284 | test genişletme |
| 5 | İşlem taramasının kalan iki darlığı | D-1285 | D-154 (b) + kapı deseni değişkeni |
| 6 | Kapanmış dijital kalemin indirme düğmesi | D-1197, D-1259 (T528) | firma kalem iptali T528'de |

## Notlar

- Bölünmüş yapım (beş işçi): ara commit'ler WIP başlıklıdır; son commit düzgün başlıkla geçer, geçmiş yeniden yazılmadı.
