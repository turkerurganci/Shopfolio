# T512 — İptal ve gecikme feshi ekranı, geri ödeme IBAN girişi (E-17, E-16 IBAN formu)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 ✓ PASS) | **Tarih:** 2026-11-12

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
- **Düzeltme 1 (tur 1 ✗ FAIL bulguları):**
  - **B1:** son adım işlemin aldığı kalemleri adıyla ve adetle yazar (`04` 5.17.6, 5.17.15; K-795): `ConfirmView.Items` + `ConfirmItem(Name, Quantity)`, `_ConfirmForm.cshtml` listeyi sonuç cümlesinin üstünde çizer. Sipariş iptali ve fesih bütün kalemleri açık adetle (`AllOpenUnits()`), kalem iptali yalnız seçilenleri seçilen adetle.
  - **B2:** `AfterSalesModule.ListRefundIbanFieldsAsync` IBAN başına döngü yerine IBAN kimlikleriyle dört toplu sorgu, bellekte eşleme (`09 §20` m.4).
  - **B3:** kapalı alana IBAN gönderiminin 422 ret satırı `RefundIbanNotRequested` (`07 §6.2.4`; VAL-A490).
  - **K1:** onaysız kalem iptalinde adet üst sınırı aşıyor ya da açık adet ekrandakinden farklıysa son adım gösterilmez; onaylı yol gibi 303 kök `QuantityExceedsOpen`.
  - **Öz denetimde:** VAL-A881 — adet alanı ve sınır satırı yalnız seçilen kalemin satırında açılır (`components.css`, `:has()`; `04` 2.12.1.8); seçilmeyen satır sunucuda zaten işlenmez.

## Etkilenen Modüller / Dosyalar

- Ordering: `CustomerOperations.cs`, `Contracts/ICustomerOperations.cs`, `CustomerScreen.cs`, `CustomerScreenOperation.cs`, `CustomerOperationOutcome.cs`, `OrderTransitions.LateDeliveryTermination.cs`, `OrderTransitions.CustomerCancellation.cs`, `OrderingModule.cs`
- AfterSales: `AfterSalesModule.RefundLine.cs`, `AfterSalesModule.Cancellations.cs`, `ShippingFeeRefunds.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`, `Contracts/RefundLine.cs`
- Payment: `Contracts/IPaymentModule.cs`, `PaymentModule.cs` (`OrderRefundIdsAsync`)
- Sayfalar: `Pages/Orders/OrderScreenModels.cs`, `OrderPageModels.cs`, `_OperationScreen.cshtml`, `_OrderIbanForm.cshtml`, `_OrderPage.cshtml`, dört ekran görünümü; `Pages/Components/ConfirmModels.cs`, `Pages/Shared/_ConfirmForm.cshtml`; `wwwroot/js/common.js`, `wwwroot/css/components.css`
- Testler: `IntegrationTests/Ordering/CustomerOperationsTests.cs`, `IntegrationTests/Files/OrderOperationScreenTests.cs`, `IntegrationTests/Files/OrderPageTests.cs`, `UnitTests/AfterSales/CancellationPreviewTests.cs`, `UnitTests/Architecture/OperationHandlerMarkTests.cs`, `OperationSourceScan(Tests).cs`, `Support/ApplicationEnums.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `SaveRefundIban` yazar / yerinde düzeltir, boşluk yok sayılır, 303 `Saved` | ✓ (doğrulama tur 2: ✓) | `CustomerOperationsTests.SaveRefundIban_CorrectsTheIbanOnFileInPlace_IgnoringBlanks_…_KK1`; `OrderOperationScreenTests.SaveRefundIban_Is303WithSaved_…_KK1_KK2` |
| 2 | UC-35 422 (biçim/sağlama, alan kapalı), 404 yabancı kalem | ✓ (doğrulama tur 2: ✓) | `CustomerOperationsTests.SaveRefundIban_ABadIban_AClosedField_AndAForeignItem_…_KK2`; ekran testi `…_KK1_KK2`; kapalı alanın ret kodu `SaveRefundIban_OnAFieldThatIsNotOpen_Is422_WithOneRefundIbanNotRequestedRejectionLine_VALA490` |
| 3 | Havale yoluna dönüş, e-posta yok | ✓ (doğrulama tur 2: ✓) | `CustomerOperationsTests.SaveRefundIban_AnswersTheFailedCardRefundsTransferPath_…_AndNoEmailIsWritten_KK3` |
| 4 | İki adres, kapsam, geri ödeme, son adım; Bekliyor → sipariş iptali, ödenmiş → kalem iptali | ✓ (doğrulama tur 2: ✓) | `CustomerOperationsTests.TheCancellationAddress_OfAnUnpaidOrder_…_KK4_KK5`, `…_OfAPaidOrder_…_KK4_KK5`, `TheTerminationAddress_…_KK4_KK6`; `OrderOperationScreenTests.TheCancellationAddress_OfAnUnpaidOrder_…_KK4_KK7`; son adımın kalemleri (düzeltme 1, B1): `TheLastStep_OfTheOrdersCancellation_ListsEveryItemByNameWithItsUnits_KK4_K795`, `TheLastStep_OfTheItemCancellation_ListsTheChosenItemsByNameWithTheChosenUnits_AndOnlyThem_KK4_K795`, `TheLastStep_OfTheTermination_ListsEveryUndeliveredItemByNameWithItsOpenUnits_KK4_K795` |
| 5 | Üç değer sunucudan; dijital/adedi birde adet alanı yok; IBAN alanı yalnız ödenmiş havalede | ✓ (doğrulama tur 2: ✓) | `…_TheServersThreeValues_TheQuantityFieldOnlyOnAnItemOfSeveralUnits_KK4_KK5`; `TheIbanField_IsDrawnOnlyOnThePaidTransferOrder_KK5` |
| 6 | Kapalı işlem 303 kök + sebep (GET ve POST), kök tutmuyorsa 404 | ✓ (doğrulama tur 2: ✓) | `OrderOperationScreenTests.AClosedOperation_Is303ToTheRootWithItsReason_OnTheScreenAndOnThePost_AndAnUnknownRootIs404_KK6`; `Terminate_BeforeTheLimit_IsClosed_…_KK6`; doğrulama tur 2 sondası (üye kökü, geçici test, silindi): başka hesabın `/siparis/{n}/iptal`, `/gecikme-feshi` GET/POST ve `?handler=SaveRefundIban` 404, oturumsuz 302 `/giris?returnUrl=…`, kendi siparişi 303 `OperationClosed` (kalıcı test D-1368) |
| 7 | `CancelOrder` onaysız 200 yazmaz; onaylı 303, ayırmalar serbest | ✓ (doğrulama tur 2: ✓) | `CustomerOperationsTests.CancelOrder_OnPlacedAndPending_CancelsTheOrder_AndFreesTheReservations_…_KK7`; ekran `…_KK4_KK7` |
| 8 | `CancelOrderItems` 422 (seçimsiz, adet, IBAN); aşım/eski açık adet 303, uygulanmaz | ✓ (doğrulama tur 2: ✓) | `OrderOperationScreenTests.TheItemCancellation_AQuantityUnderOne_ARepeatedItemRow_AndNoIbanOnTheTransfer_Are422_…_KK8_D155`, `…_AboveTheOpenUnits_OrOnAStaleOpenQuantity_Is303_…_KK6_KK8`; `CustomerOperationsTests.CancelOrderItems_…_KK8` (3 test); onaysız gönderimde de 303 (K1): `TheItemCancellationsUnconfirmedPost_AboveTheOpenUnits_OrOnAStaleOpenQuantity_Is303WithTheReason_NotTheLastStep_K1` |
| 9 | Kartta kendiliğinden, havalede IBAN'la firma; bütün fiziksel adetler → kargo ücreti | ✓ (doğrulama tur 2: ✓) | `CancelOrderItems_OnTheCard_StartsTheRefundItself_AndEveryPhysicalUnitCoveredPaysTheShippingFeeBack_KK9` (iki sıra), `…_OfSomeUnitsOnly_PaysNoShippingFee_KK9`, `…_OnTheTransfer_KeepsTheIbanOnTheRecords_…_KK9`; `CancellationPreviewTests` |
| 10 | Fesih bütün açık adetleri kapsar, alanları yok sayar, kargo hep döner, IBAN 422 | ✓ (doğrulama tur 2: ✓) | `Terminate_CoversEveryOpenUnit_…_KK10`, `Terminate_OnThePaidTransfer_WithoutAValidIban_…_KK10`, `…_D873_KK10`; `OrderOperationScreenTests.TheTermination_IgnoresTheItemAndQuantityFields_…_KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin işi dahil dalın bütün diff'i (`origin/main...HEAD`) madde madde denetlendi; `sik-fail-nedenleri.md` okundu. Düzeltme 1'den sonra bütün satırlar yeniden denetlendi (tur 1'in üç uyuşmazlığı: kriter 4, "Döngüde sorgu", VAL-A490 artık kanıtıyla); `12`'de E-17'yi anan VAL-A881 satırı eksikti, eklendi ve stili yazıldı.

| Madde (KK / 12 / backlog / sık FAIL) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 boşluklu IBAN, yerinde düzeltme | ✓ | `SaveRefundIban_CorrectsTheIbanOnFileInPlace_IgnoringBlanks_…_KK1` |
| KK2 iki 422 sebebi ve 404 | ✓ | `SaveRefundIban_ABadIban_AClosedField_AndAForeignItem_…_KK2`; ekran `…_KK1_KK2` |
| KK3 havale yoluna dönüş, e-posta yok | ✓ | `…_AndNoEmailIsWritten_KK3` |
| KK4 iki adres, iki işlem; son adım kalemleri adıyla ve adetle (B1) | ✓ | `TheCancellationAddress_OfAnUnpaidOrder…`, `…_OfAPaidOrder…`, `TheTerminationAddress…`; `TheLastStep_OfTheOrdersCancellation_…_KK4_K795`, `TheLastStep_OfTheItemCancellation_…_AndOnlyThem_KK4_K795`, `TheLastStep_OfTheTermination_…_KK4_K795` |
| KK5 üç değer; alan görünürlüğü | ✓ | `…_TheServersThreeValues_…_KK4_KK5`, `TheIbanField_IsDrawnOnlyOnThePaidTransferOrder_KK5` |
| KK6 sebep kodlu 303 ve 404 | ✓ | `AClosedOperation_Is303ToTheRootWithItsReason_…_KK6` |
| KK7 onaysızda kayıt yok; onaylıda iptal | ✓ | `TheCancellationAddress_OfAnUnpaidOrder_OffersTheOrdersCancellation_TheUnconfirmedPostIsTheLastStep_AndTheConfirmedOneCancels_KK4_KK7` |
| KK8 bütün istek reddi (onaylı ve onaysız gönderim) | ✓ | `CancelOrderItems_AQuantityAboveTheOpenUnits_OrAnOpenQuantityOtherThanTheShownOne_RefusesTheWholeRequest_KK8`; onaysız: `TheItemCancellationsUnconfirmedPost_…_Is303WithTheReason_NotTheLastStep_K1` |
| KK9 iki hat; kargo iadesi | ✓ | `…_OnTheCard_…_KK9`, `…_OnTheTransfer_…_KK9`, `…_OfSomeUnitsOnly_PaysNoShippingFee_KK9` |
| KK10 alan yok sayma; kargo iadesi | ✓ | `TheTermination_IgnoresTheItemAndQuantityFields_CoversEveryOpenUnit_…_KK10`, `Terminate_CoversEveryOpenUnit_…_KK10` |
| VAL-A454 `InvalidIban` 422, kaydedilmez | ✓ | `…_KK2` (iki katman), `CancelOrderItems_OnThePaidTransfer_WithoutAValidIban_…_KK8`, `Terminate_…_WithoutAValidIban_…_KK10` |
| VAL-A490 `RefundIbanNotRequested` 422 | ✓ (düzeltme 1, B3) | `SaveRefundIban_OnAFieldThatIsNotOpen_Is422_WithOneRefundIbanNotRequestedRejectionLine_VALA490` (tek ret satırı, kod `RefundIbanNotRequested`); veri yazılmaz: `SaveRefundIban_ABadIban_AClosedField_…_KK2` |
| VAL-A554 `Saved` bildirimi | ✓ (uç yanıtı) | `SaveRefundIban_Is303WithSaved_…`; tarayıcı gözlemi yapılmadı (⚠ 3) |
| VAL-A577 `QuantityExceedsOpen` bütün istek, 303 | ✓ | `…_AboveTheOpenUnits_OrOnAStaleOpenQuantity_Is303WithTheReason_…_KK6_KK8` |
| VAL-A581 aynı kalem iki satır 422, hiçbiri işlenmez | ✓ | `…_ARepeatedItemRow_…_Are422_AndWriteNothing_KK8_D155` (`ItemRows.RejectRepeated`) |
| VAL-A686 kapalı ekran 303, 404 değil | ✓ | `AClosedOperation_Is303ToTheRootWithItsReason_…_KK6`, `TheTerminationAddress_IsClosedBeforeTheLimit_…` |
| VAL-A688 açık adedi aşan istek bütünüyle red | ✓ | `CancelOrderItems_AQuantityAboveTheOpenUnits_…_KK8` |
| VAL-A692 fesihte adet yok sayılır | ✓ | `TheTermination_IgnoresTheItemAndQuantityFields_…_KK10` |
| VAL-A693 [güvenlik] sahiplik kökten, yabancı kalem 404 | ✓ | `…_AnUnknownRootIs404_KK6`, `CancelOrderItems_AnItemOfAnotherOrder_IsNotFound_…_KK6_KK8`, `SaveRefundIban_…_AndAForeignItemIs404_…` |
| VAL-A895 fesih bütün açık adetler, kargo hep | ✓ | `Terminate_CoversEveryOpenUnitOfTheUndeliveredPhysicalItems_AndPaysTheShippingFeeBack_KK10` |
| VAL-A881 [tarayıcı] adet alanı kalem seçilince satırında açılır; adedi bir ve dijital kalemde yok; satır üst sınırı söyler | — (uç yanıtı yarısı ✓) | Sunucu: `…_TheServersThreeValues_TheQuantityFieldOnlyOnAnItemOfSeveralUnits_KK4_KK5` (`HasQuantityField` yalnız çok adetli fiziksel kalemde; açık adet, varsayılan, üst sınır); `_OperationScreen.cshtml` sınır satırı "Açık adet: N · en çok N". Seçilince açılma öz denetimde eksik bulundu, `components.css`'e yazıldı (⚠ 6). Tarayıcı gözlemi doğrulamaya (⚠ 3) |
| VAL-B071 [tarayıcı] 320'de E-17 | — | Tarayıcı gözlemi yapılmadı; doğrulamaya (⚠ 3). Son adımın dört parçası (kalemler, sonuç cümlesi, "geri alınamaz", iki düğme) artık aynı bölümde (B1) |
| `09 §25` / güvenlik taraması | ✓ | Yeni ham SQL, gizli bilgi, kimlik doğrulamasız uç yok; işlem POST'ları antiforgery + `[GatedOperation]`; sahiplik kökten (`FindOrderAsync`) |
| Backlog: hedefi T512 olan kalemler | ✓ | D-153, D-154, D-155, D-792, D-803, D-873, D-1194, D-1258 kapandı; D-819, D-1196 T512 payı; D-1197, D-1259 T528'e gerekçeyle (⚠ 2) |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `OperationHandlerMarkTests` muafiyeti tek işleyici adıyla, ölü muafiyet bulgu (`ANamedExemptionThatNoLongerExists_IsAFinding`) |
| Sık FAIL 2: tarama çağrı biçimi kaçırıyor | ✓ | İşaret yansımayla okunur (öznitelik türü; yazım biçiminden bağımsız); `Gate()` için iki yeni ihlal satırı; kalan darlık D-1285 |
| Sık FAIL 3: DB default / FK dizini | ✓ | Şema ve göç değişmedi |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | Kural/sayı değişikliği yok; enum dosyaları `ApplicationEnums` listesinde |
| Sık FAIL 5: belge kuralını değiştiren 09 metni | ✓ | `09` ve öteki kural belgeleri değişmedi |
| Tek kural evi (para) | ✓ | Sayfa tutar hesaplamaz; kargo ön izlemesi `ShippingFeeRefunds.OnCancellationPreview`, süre `AfterSalesPeriodRules` üzerinden |
| Döngüde sorgu (`09 §20` m.4) | ✓ (düzeltme 1, B2) | `AfterSalesModule.ListRefundIbanFieldsAsync` IBAN kimlikleriyle dört toplu sorgu + bellekte eşleme: `CustomerOperationsTests.TheIbanFields_AreReadInTheSameNumberOfCommands_WhateverTheNumberOfIbansOnFile_EachOnItsOwnItem_09_20` (komut sayacı, bir ve üç IBAN aynı sayı); kalemsiz IBAN alanı süzgeci tek sorgu (`OrderRefundIdsAsync`); çok kalemli iptal satır başına modül işlemi (kural tek kalemli geçişte, K-1202 işlem içinde bütün-ya-da-hiç) |
| Ret kaydı istek başına bir satır (`09 §8.2` m.3; sık FAIL T503 r2, T506 r1) | ✓ (son öz denetimde düzeltildi) | Kalem iptalinde alan hatalı 422'ler (tekrarlı satır, adet) hem süzgecin alan satırını hem sayfanın kendi satırını yazıyordu (iki satır); sayfa satırı kaldırıldı, yalnız alan kodu olmayan "kalem seçilmemiş" 422'si `FieldRequired` satırını sayfada yazar. `OrderOperationScreenTests.TheItemCancellation_…_Are422_AndWriteNothing_KK8_D155` altı 422 için tam altı ret satırı sayar (`FileEndpointsHost.Logs`). Düzeltme 1: kapalı alan 422'si tek satır (`…_VALA490`, `Assert.Single`); K1'in onaysız 303'ü onaylı yolun ret yolunu (`OperationRefusals.ToOrderPage`) kullanır, ayrı satır yazmaz |
| Sık FAIL: kabul kriteri kısmen karşılandı | ✓ | Kriter 4'ün son adım parçası (tur 1 B1) karşılandı; ~ satırı yok |
| Sık FAIL: tarayıcı isteyen `12` satırı tarayıcısız PASS'a çekildi | ✓ | VAL-B071, VAL-A881 ve VAL-A554'ün tarayıcı yarısı "—", doğrulamaya (⚠ 3) |
| Sık FAIL: sayfa bölge listesi belgeyle birebir | ✓ | Son adım `04` 5.17.15 sırasıyla: kalemler, sonuç cümlesi, "geri alınamaz", iki düğme (`_ConfirmForm.cshtml`); yeni bölge yok |
| Sık FAIL: Integration TAM koşulmadan PR | ✓ | Kod düzeltmesinden sonra Integration TAM 3947/3947 (Test Sonuçları); sonraki değişiklik yalnız stil (⚠ 7) |

## Test Sonuçları

| Tür | Sonuç |
|---|---|
| Derleme | Uyarısız, hatasız (birleşik ağaç, origin/main birleştirildi) |
| Unit | 7222/7222 (filtresiz TAM; ilk koşuda yük kaynaklı tek regex zaman aşımı, ikinci koşu yeşil) + `OperationHandlerMarkTests` 4/4 |
| Integration | TAM ad alanı parçalarıyla 3857/3857 yeşil: Ordering 668, Web 821 (head `45641703`); main `33a9d21a` birleştirildikten sonra Model 1410 (sınıf harf gruplarıyla dört parça 319+427+370+294), Files+Payment 178, Administration+Catalog+Commands+Concurrency+CorporateContent 446, Database+Disposal+FileSecrets+Jobs+Logging+Outages+Security+SignedEndpoints 334; birleştirmenin getirdiği T549 testleri için `StorefrontSearchAndProductTests` 32/32 yeniden. `Pages` ad alanı yalnız yardımcı sınıflar (test yok). |
| Unit (birleştirme sonrası) | `Architecture` 1702/1702, `Pages` 339/339 |
| main `856f5441` (T544) birleştirmesi + ret satırı düzeltmesi sonrası | Unit `Architecture` 1750/1750, `Pages` 420/420; Integration `Web` 842/842 (üç parça 124+350+368; ortadaki parça ilk koşuda 1 kırmızı, adı alınamadı, değişmeden yeniden koşuda 350/350 yeşil), `Files` 119/119, `Ordering.CustomerOperationsTests` yeşil; `dotnet format --verify-no-changes` değişen üç dosyada temiz |
| main `368d978d` (T514) birleştirmesi sonrası | Derleme uyarısız; Unit `Architecture` + `Pages` 2190/2190; Integration `Web` 869/869 (dört parça 124+218+245+282; T514'ün `RegistrationPageTests` dahil). Birleştirmede tek çakışma `DEFERRED_BACKLOG.md` D-801/D-803 satırlarında: D-801 main'den (T514 payı düştü), D-803 daldan (T512 kapanışı) |
| `dotnet format --verify-no-changes` | Web, UnitTests, IntegrationTests (değişen dosyalar `--include` ile) temiz |
| Düzeltme 1 (head `c4cf0936`; main `29562ee9` birleştirildi) | restore ✓; derleme `-warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` Web, UnitTests, IntegrationTests (proje TAM) çıkış 0; Unit TAM 7416/7416; Integration TAM (postgres:18, `track_commit_timestamp=on`) 3947/3947: Files 124, Ordering 669 (423+246), Model 1410 (727+683), kalan 15 ad alanı 839, Web 905 (395+510). Files'ın bir ara koşusu öldürülmüş önceki koşunun veritabanı durumuyla 124 kırmızı geldi; değişmeden yeniden koşuda 124/124 |
| Öz denetim stil düzeltmesi sonrası (VAL-A881, yalnız `components.css`) | Derleme `-warnaserror` 0 uyarı 0 hata; Unit `Architecture` 1750/1750 (stil taramaları dahil); Integration `OrderOperationScreenTests` 12/12. Integration TAM yeniden koşulmadı (⚠ 7) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL (2026-11-12; dal ucu `43fad73f`, birleşik ağaç origin/main `bc915800` ile çakışmasız) — build, format, Unit 7395/7395, Integration TAM 3922/3922, dal CI yeşil |
| Bulgu sayısı (tur 1) | 3 (1 ORTA, 2 DÜŞÜK; hepsi S1) + 1 K-notu |
| Düzeltme gerekli mi | Evet — B1 (ORTA): onaysız POST'un 200 son adımı seçilen kalemleri adıyla ve adetle yazmıyor, yalnız sonuç cümlesi (`04` 5.17.6, 2.4.5, 5.17.15; `07` 4.17.3; K-795; kriter 4). B2 (DÜŞÜK): `AfterSalesModule.ListRefundIbanFieldsAsync` IBAN başına döngüde dört sorgu (`09 §20` m.4). B3 (DÜŞÜK): alan kapalıyken 422 ret satırının kodu `OperationNotAvailable`, `07 §6.2.4` UC-35 `RefundIbanNotRequested` ister. K1: onaysız kalem iptalinde aşan/eski adet son adımı gösteriyor, ret ancak onaylı gönderimde |
| Yapım raporu karşılaştırması (tur 1) | 3 uyuşmazlık: kriter 4 ✓ sayılmış (B1); öz denetimin "Döngüde sorgu ✓" satırı B2'yi atlamış; VAL-A490 ✓ sayılmış (B3) |
| Düzeltme 1 | B1, B2, B3 düzeltildi; K1 de düzeltildi (onaysız gönderimde 303), backlog'a aktarılmadı. Yeni backlog kalemi yok (D-1356–1357 kullanılmadı). Ayrıntı "Yapılan İşler" > Düzeltme 1 |
| Doğrulama durumu (tur 2) | ✓ PASS (2026-11-12; dal ucu `f76de0e3`, origin/main `29562ee9` ile birleşik ağaç dalın kendisi) — restore, derleme `-warnaserror` 0 uyarı 0 hata, format ×3 temiz, Unit TAM 7416/7416 (ad alanı parçalı), betik 4/4, Integration TAM 3947 (postgres, `track_commit_timestamp=on`), dal CI (run 43181168517) yeşil; KK1–KK10 ✓, `09 §25` ilgili satırlar ve güvenlik taraması temiz |
| Bulgu sayısı (tur 2) | 0 bulgu + 2 K-notu: K1 → D-1367 (kalem iptali sınır satırı kalemin adedini yazmıyor, `04` 2.12.1.8; T513), K2 → D-1368 (üye kökünde işlem ekranları ve UC-35 için kalıcı test yok, VAL-A693; F5 kapısı) |
| Düzeltme gerekli mi (tur 2) | Hayır |
| `12` satırları (tur 2) | VAL-C003, VAL-A068, VAL-C006, VAL-C007, VAL-C009 → PASS; VAL-C004, C005, C008, C010, C014, C019, A067, A069, A070 BEKLEMEDE (kanıtın bir yarısı başka task'ın ya da bu task'ın testlerinde yok) |
| Yapım raporu karşılaştırması (tur 2) | Tam uyumlu: öz denetim tablosu KK başına ✓ + kanıt taşır; tarayıcı satırları (VAL-A881, VAL-B071, VAL-A554 yarısı) doğru biçimde "—" bırakılmış |

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
5. **Web parçasında tek kararsız kırmızı:** T544 birleştirmesinden sonra `Web` F*–O* parçasının ilk koşusunda 1 test kırmızı geldi (çıktı dosyaya alınmadığı için adı bilinmiyor); kod değişmeden yeniden koşuda 350/350 yeşil. T512 dosyası bu parçada yok; doğrulayıcı birleşik ağaçta Web'i koşarken dikkat etsin. T514 birleştirmesinden sonraki `Web` koşusu (869) ilk seferde yeşil.
6. **VAL-A881 adet alanının açılması (öz denetim, düzeltme 1 sonrası):** `04` 2.12.1.8 "alan kalem seçildiğinde kalemin satırında açılır" der; alan her satırda açıktı. Betiksiz yol korunarak stil kuralıyla çözüldü: seçilmemiş kalemin satırında adet alanı ve sınır satırı gizlenir (`:has()`; desteklemeyen tarayıcıda alan açık kalır, sunucu seçilmemiş satırı zaten işlemez). Sayfa `required` niteliği taşımadığı (`aria-required`) için gizli alan gönderimi durdurmaz.
7. **Stil düzeltmesinden sonra Integration TAM yeniden koşulmadı:** değişiklik yalnız `components.css`'te; Integration testlerinden hiçbiri bu dosyanın içeriğini okumaz (yalnız `base.css` yolu). Makine yükünden tek sınıf 7,5 dk sürdüğü için süre bütçesinde TAM sığmadı; derleme, Unit `Architecture` ve E-17 ekran sınıfı yeniden koşuldu, TAM dal CI'ında koşar.

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
