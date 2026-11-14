# T530 — Sipariş ayrıntısı: adres, kargo, teslim tarihi, durum geçişi ve e-posta düzeltmeleri (E-37)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-14

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- E-37'nin "Düzeltmeler" grubu (`04` 9.9.31): altı panel ucu — `CorrectAddress` (UC-146), `CorrectCarrier` (UC-147), `CorrectDeliveryDate` (UC-148), `CorrectArrivalDate` (UC-149), `CorrectTransition` (UC-150), `CorrectOrderEmail` (UC-151). Hepsi `[GatedOperation]`, sürüm taşır; eski sürüm 409, kapalı işlem ve reddedilen değer 422 (mesaj yerinde, form korunur), başarı 303.
- Yazımlar var olan birimlerin: T375/T386 `IOrderCorrections` (adres geçmişi, e-posta geçmişi, erişim anahtarı, hesap bağı, gönderilmemiş bildirimlerin yeni adrese yönelmesi, B-1/B-10/B-12/B-15, iz satırları) ve T319 `IOrderTransitions.CorrectTransitionAsync` (yeni geçiş + `CorrectedTransitionRecordId`, yeniden ayırma, geri ödemenin iptali, IBAN isteği ve IBAN'ın kapanması, B-13).
- Yeni panel cephesi `IPanelOrderCorrections` / `PanelOrderCorrections`: ret sebebini ekran için adlandırır (`CorrectionValueProblem`, `DateObstacle`, `CorrectionObstacle`), teslim tarihinin sıra sorusunda beyanın anlarını okur, ulaşma kaydının kalemini bulur, durum düzeltmesinde 09 §8.3 m3 sırasını uygular (kilit → sürüm 409 → işlem kuralı 422 → çekirdek) ve **durum düzeltmesinin iz satırını** yazar (`CorrectTransition`: düzeltilen geçişin kodu + sebebin kodu; açıklama metni ize girmez — D-884 m5).
- Düzeltilebilir geçişin seçimi tek kurala taşındı: `CorrectableTransitions.Find` — işlem kararı (`OrderOperations.HasCorrectableTransitionAsync`) ve formun "düzeltilecek geçiş / dönülecek durum" satırı aynı yerden okur (davranış değişmedi; `OrderOperationsTests`, `StatusCorrectionTests` yeşil).
- Ekran: `_OrderDetailCorrections.cshtml` bölümü (`id="duzeltmeler"`), adres ve e-posta formlarının başında K-798 (6)'nın teyit hatırlatma metinleri; durum düzeltmesinde OB-15 kapalı listesi (`CorrectionReason`, `ReasonNote` — `06 §4.19` adları).
- D-852: `PaymentWindowTests` KK7 testi reddin sebebini de doğrular.

## Etkilenen Modüller / Dosyalar

- Ordering: `Contracts/IPanelOrderCorrections.cs`, `Contracts/CorrectionValueProblem.cs`, `PanelOrderCorrections.cs`, `CorrectableTransitions.cs` (yeni); `OrderOperations.cs` (seçim tek kurala), `OrderingModule.cs` (kayıt).
- Pages/Panel: `OrderDetail.Corrections.cs`, `_OrderDetailCorrections.cshtml` (yeni); `OrderDetail.cshtml.cs` (`partial` + `LoadAsync`'e tek satır), `OrderDetail.cshtml` (tek `<partial>` satırı).
- Testler: `OrderDetailPageTests.Corrections.cs`, `PanelCorrectionTextsTests.cs` (yeni); `OperationHandlerMarkTests`, `IrreversibleConfirmationTests`, `ApplicationEnums`, `OrderCorrectionTests` (panel cephesi istisnası), `PaymentWindowTests` (D-852).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `CorrectAddress` teslimat/fatura adresi ve telefon; eski-yeni geçmişte, adres ize girmez | ✓ | `OrderDetailPageTests.CorrectAddress_TheShippingAddressWithItsPhoneAndTheBillingAddress_Are303_WriteTheHistory_AndTheTrailHasNoAddress_KK1` (düzeltme 1: sayfadaki adres geçmişi, VAL-D026) |
| 2 | UC-146 422: alan kuralı; Kargoya verildi'den (fiziksel kalemsizde Teslim edildi'den) sonra kapalı | ✓ | `…CorrectAddress_AFieldItRefuses_Is422WithTheFieldsMessage_AndItIsClosedFromShipped_OrWithNoPhysicalItemFromDelivered_KK2` (düzeltme 1: reddedilen alan `aria-invalid` + kendi mesajı, formun başında özet, gönderilen değer korunur); `…TheCorrections_RepeatNoElementId_AndEachAddressFormsLabelsAreItsOwnFields_B3`; `…CorrectAddress_ThatNamesNoAddress_Is422WithItsOwnMessage_AndOneWithoutTheVersion_Is409_K4` |
| 3 | `CorrectCarrier`; boş bilgi ya da kargoya verilmemiş sipariş 422 | ✓ | `…CorrectCarrier_OnAShippedOrder_Is303_SendsB12_AndWritesTheTrail_TheEmptyInfoAndAnOrderNotShipped_Are422_KK3` (düzeltme 1: mesaj kargo formunda; düzeltilmiş takip no sayfada, VAL-A240) |
| 4 | `CorrectDeliveryDate` pencere ve ayıp süresi yeni tarihten; 422 tarih hâlleri, işaretsiz teslim | ✓ | `…CorrectDeliveryDate_RunsTheWithdrawalWindowAndTheDefectPeriodFromTheNewDate_AndWritesTheTrail_KK4`, `…_AFutureDate_ADateBeforeTheShipment_ADateAfterAReturnsArrival_AndAnUnmarkedOrder_Are422_KK4`; sıra (K-608): `…CorrectDeliveryDate_OnAWithdrawalDeclarationsDay_AsksWhichCameFirstInItsForm_ThenTheAnswerWritesTheDate_ShownOnThePage_K608` |
| 5 | `CorrectArrivalDate` Z-16 yeni tarihten; sınır dışı tarih 422 | ✓ | `…CorrectArrivalDate_RunsTheRefundPeriodFromTheNewDate_AndAFutureDateOrOneBeforeTheDelivery_Is422_KK5` (düzeltme 1: mesaj kaydın formunda, gönderilen tarih korunur; yeni tarih sayfada) |
| 6 | `CorrectTransition` yeni geçiş, dönülecek durum kayıttan, sebep ve "diğer"de açıklama | ✓ | `…CorrectTransition_AsksAReason_AndTheNoteOfOther_ThenWritesTheCorrectionOnTheCorrectedRecord_AndTheTrail_KK6`; `PanelCorrectionTextsTests.TheBackState_*_KK6` |
| 7 | UC-150 422: dört engel, engel mesajıyla | ✓ | `…_RefundPending_KK7` (havale, gönderilmiş), `…_WhileTheCardRefundsOutcomeIsAwaited_Is422_RefundPending_KK7` (kart `OutcomePending`), `…_StockShort_KK7`, `…_ItemRecordExists_KK7`, `…_CardPayment_KK7`; `PanelCorrectionTextsTests.EveryObstacle_HasItsOwnMessage_KK7`; mesaj durum düzeltmesi formunda (düzeltme 1) |
| 8 | Yanlış iptal düzeltmesi yeniden ayırır; geri ödeme, IBAN isteği, IBAN kapanır | ✓ | `…CorrectTransition_OfAWrongCancellation_TakesTheUnitsAgain_VoidsTheRefund_ClosesTheIbanRequest_AndDeletesTheIban_KK8` |
| 9 | `CorrectOrderEmail` anahtar, eski bağlantı, hesap bağı, gönderilmemiş bildirimler | ✓ | `…CorrectOrderEmail_RenewsTheKey_ReroutesTheUnsentNotices_SendsB1ToTheNewAndB15ToTheOld_AndKeepsTheHistory_KK9_KK10`, `…_AVerifiedAccountOfTheNewAddress_TakesTheOrder_KK9`; e-posta geçmişi sayfada (VAL-D027) |
| 10 | B-1 yeni, B-15 eski adrese; adresler geçmişte, izde yok; üyeli/imha/geçersiz 422 | ✓ | `…_KK9_KK10` (yukarıda), `…CorrectOrderEmail_AnInvalidAddress_AnAccountsOrder_AndADisposedOrder_Are422_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: geçmiş satırı; izde adres yok | ✓ | `_KK1` testi: iki `OrderAddressHistory` satırı (eski/yeni hat ve telefon); dört serileştirilmiş iz satırında adres ve telefon parçası yok |
| KK2: alan 422; iki sınır | ✓ | `_KK2`: boş açık adres 422, `Input_ShippingAddressLine` `aria-invalid="true"` + `Input_ShippingAddressLine-message`, özet bağlantısı `#Input_ShippingAddressLine` teslimat formunun başında, fatura formu işaretsiz; fiziksel Shipped 422 (mesaj "Düzeltmeler" bölümünde); fiziksel kalemsiz Preparing 303, Delivered 422 |
| KK3: iki 422 sebebi | ✓ | `_KK3`: boş bilgi `ShipmentMissingText`, Preparing `NotOpenText`; 303'te B-12 ve `CorrectShippingInfo` (takip no izde yok) |
| KK4: pencere, süre yeni tarihten; tarih reddi | ✓ | `_KK4` iki test: `PanelItemFacts.WithdrawalLastDay`/`DefectLastDay` geriye kayar; ileri, kargodan önce, ulaşmadan sonra ve işaretsiz sipariş 422 |
| KK5: Z-16; iki tarih reddi | ✓ | `_KK5`: ileri ve teslimden önceki tarih 422 (`ArrivalDateInvalidText`, K-554 tek metin); `WithdrawalRefundStartDay` yeni tarih |
| KK6: sebep zorunlu; geçiş kaydı | ✓ | `_KK6`: sebepsiz ve "diğer"de notsuz 422; `CorrectedTransitionRecordId` + not kayıtta; iz `CorrectTransition` (S5, not yok); B-13 |
| KK7: dört engel ayrı testte | ✓ | dört `_KK7` testi + kart `OutcomePending` sayfa testi; her biri engelin kendi mesajı durum düzeltmesi formunda (`correction-message`), seçilen sebep korunur, 422, yazım yok |
| KK8: ayırma; geri ödeme, IBAN isteği ve IBAN kapanır | ✓ | `_KK8`: stok 5→3, `Refund.VoidedAt`, `RefundIbanRequest.ClosedAt`, `RefundIban` silindi |
| KK9: anahtar, hesap bağı, alıcı | ✓ | `_KK9_KK10`: `OrderAccessKey` değişti, kuyruktaki B-4 yeni adrese; `_KK9`: doğrulanmış hesap siparişi alır |
| KK10: iki e-posta; izde adres yok; üç 422 | ✓ | B-1 yeni, B-15 eski adrese; `OrderEmailHistory`; iz satırında iki adres yok; geçersiz, üyeli, imha 422 |
| 09 §25: `[GatedOperation]`, tek kural, iz satırı aynı işlemde, `Details` kişisel veri yok | ✓ | `OperationHandlerMarkTests` (altı yeni işleyici listede); `CorrectableTransitions` tek kural; `PanelOrderCorrections.CorrectTransitionAsync` iz satırı runner işleminde |
| 09 §25: yeni adlar `06`/`07`'den | ✓ | giriş alanları `AddressType`, `ContactEmail`, `DeliveryDate`, `CorrectionReason`, `ReasonNote`, `TransitionRecordId`; `ArrivalRecordId` `[SpecAssignedName]`; `ModelBindingRules` yeşil |
| Güvenlik taraması | ✓ | uçlar panel filtresinin arkasında (yönetici oturumu), antiforgery formda; yeni sır/bağımlılık yok; misafir e-postası düzeltmesinde müşteri yüzeyi yok (`OrderCorrectionTests.TheCustomersSurfaces_*_KK6` panel cephesini yalnız `TransitionAdmin` istekleriyle kabul eder) |
| Backlog D-852 | ✓ kapandı | `PaymentWindowTests` KK7: `(CorrectionNotPossible, NotCorrectable)` |
| Backlog D-883 | ✓ kapandı | UC-148 ucu + `_KK4` testleri |
| Backlog D-884 (m5) | ✓ m5 kapandı; hedef T528'e daraldı (m7, m8) | `_KK6` iz satırı |
| Backlog D-894 | ↷ doğrulamaya | kalem T530 doğrulamasında okunur (`12 VAL-A239`); yapımın okuması D-1525'te |
| Backlog D-981 (T530 payı: "uç yanıtı" VAL satırları) | ↷ doğrulamaya / F5 | uçlar artık var; `Durum` hücrelerini doğrulama yazar |
| Backlog D-1464 | ↷ faz kapısı | ekran T527'nin iki ayrık teslim tarihi metnini kullanır; karar faz kapısında |
| Sık FAIL (4): aynı sayıyı anan yerler | ✓ | `OperationHandlerMarkTests` işleyici listesi, `ApplicationEnums` güncel |
| Düzeltme 1 — `04` 2.3.1.1, 2.3.2 (B1) | ✓ | `KeepAddress` hataları `FormErrorSummary.From(ModelState, FieldOrder)` ile formun kendi görünümüne `Bind` eder; özet yalnız reddedilen adres formunun başında (`_KK2`) |
| Düzeltme 1 — `04` 2.3.3, 2.7.3.1, 9.9.41 (B2) | ✓ | `CorrectionRefusalText` + `RefusedCorrection` (form kimliği) `_OrderDetailCorrections.cshtml`'de okunur; mesaj gönderilen formun içinde düğmenin yanında, form çizilmiyorsa "Düzeltmeler"in başında; `RefusalText`/`order-step-message` düzeltmelerde kullanılmaz; ulaşma tarihi, geçiş seçimi, sebep, teslim tarihi, e-posta korunur (`_KK2`, `_KK3`, `_KK5`, `_KK6`, `_RefundPending_KK7`, `_K608`, `_KK10`) |
| Düzeltme 1 — `04` 2.3.1.1 kimlikler (B3) | ✓ | adres formları `Input.Shipping…`/`Input.Billing…` (07 UC-146 adları); iki geçiş tek formda seçimle, tek sebep alanı; `_B3` testi iki sayfada (iki geçişli kargolanmış, iki adres formlu hazırlanan) yinelenen kimlik yok |
| Düzeltme 1 — K2, K3, K4 | ✓ | kart `OutcomePending` sayfa testi; K-608 kalıcı testi; VAL-A240/D026/D027 sayfa gözlemi `_KK1`, `_KK3`, `_KK5`, `_K608`, `_KK9_KK10`; tanımsız adres türü 422 kendi metniyle, sürümsüz istek 409 (`_K4`) |
| Sık FAIL dosyası (T530 r1 üç satırı dahil) | ✓ | alan bağlama, mesaj yeri + korunan değer, yinelenen kimlik bu düzeltmede kapandı; öteki satırlar bu diff'e uymaz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata (main `6e6d4a41` birleşik) |
| Unit | ✓ TAM | `dotnet test --project tests/Shopfolio.UnitTests/...` | 7643/7643 (düzeltme 1, main birleşik) |
| Integration | ✓ sınıflar — TAM: CI | `--filter-class` `*.OrderDetailPageTests`, `*.OrderCorrectionTests`, `*.StatusCorrectionTests` (tek çağrı) | 154/154 (düzeltme 1, main birleşik; konteyner `shopfolio-t530-fix1-pg`, `track_commit_timestamp=on`) |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <dalın .cs dosyaları>` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS (2026-11-14; dal ucu `02146b34`, origin/main `6e6d4a41` dalda birleşik, main ilerlemedi) — build `-warnaserror` 0 uyarı, format ×3 (Web, UnitTests, IntegrationTests) temiz, Unit 7348/7348 + Consistency 295/295, betik 4/4, Integration parçalı `OrderDetailPageTests` 50/50 + `OrderCorrectionTests`, `PaymentWindowTests`, `StatusCorrectionTests`, `OrderOperationsTests` 146/146 (TAM: CI; konteyner `shopfolio-t530-val-pg`, port 56731, `track_commit_timestamp=on`, silindi); dal CI'ı run 43577917177 bütün işler yeşil; `dotnet restore --locked-mode` çıkış 0 |
| Kabul kriterleri (tur 2, doğrulayıcı) | 10/10 ✓. Kriter 2: `_KK2` (boş açık adres 422, `Input_ShippingAddressLine` `aria-invalid="true"` + kendi mesajı, özet formun başında, fatura formu işaretsiz; Shipped 422, dijital Preparing 303, Delivered 422) + doğrulayıcının geçici sınaması (telefon reddinde `Input_ShippingPhone` işaretli ve özet bağlantısı, gönderilen hat korunur; fatura reddinde yalnız fatura formu işaretli). Kriter 7: dört engel ayrı testte + kart `OutcomePending` sayfa testi; mesaj durum düzeltmesi formunda, sebep korunur. Kriter 9: eski bağlantı uçta kapalı — geçici sınama: düzeltmeden sonra `/siparis/erisim/{eski anahtar}` 404, yeni anahtar 200. Öteki kriterler tur 1 kanıtlarıyla yeniden koşuldu |
| Önceki bulgular (tur 2) | B1 kapandı (`KeepAddress` → `AddressForms.AddRefusals` + `FormErrorSummary.From` + `Bind`; `_KK2`). B2 kapandı (mesaj `RefusedCorrection` formunda, form çizilmiyorsa bölümün başında; `order-step-message` düzeltmelerde yok; ulaşma tarihi, geçiş seçimi, sebep, teslim tarihi, e-posta korunur — `_KK2`, `_KK3`, `_KK5`, `_KK6`, `_RefundPending_KK7`, `_K608`, `_KK10`). B3 kapandı (`Input.Shipping…`/`Input.Billing…` — `07` `CorrectAddressInput` adları; `_B3` iki sayfada yinelenen kimlik yok; doğrulayıcı ayrıca iki ulaşma formlu teslim edilmiş, teslim edilemedi ve reddedilmiş fatura formlu sayfalarda yinelenen kimlik bulmadı). K2, K3, K4 kapandı. ⚠ denetimi: iki geçişte tek form + seçim `04` 9.9.31 "form düzeltilecek geçişi ve dönülecek durumu gösterir" ile uyumlu (her seçenek geçişi ve dönülecek durumu gösterir), kapsam dışı değil; `AddressTypeMissingText` D-1527'nin iki form kararının sonucu, D-1527 notunda kayıtlı; D-1544 ortak sayfa riski, F5 kapısı uygun (yalnız elle bozulmuş istek, yazım yok, T527 uçları da aynı yolda) — doğrulayıcı notu D-1544'e eklendi |
| `12` satırları (tur 2) | PASS: `VAL-C055`, `C056`, `C059`, `A246` (tur 1), `VAL-A240` (`_KK3` takip no, `_K608` teslim tarihi, `_KK5` ulaşma tarihi sayfada ve kayıtta), `VAL-D026` (`_KK1` adres geçmişi sayfada iki adres + tarih, kayıtta iki satır), `VAL-D027` (`_KK9_KK10` e-posta geçmişi sayfada + kayıt), `VAL-A245` (uç: geçici sınamada eski anahtar 404, yeni 200; veri: `_KK9_KK10` anahtar değişti). BEKLEMEDE: `VAL-A239`, `A244` (D-894 → D-1525), `VAL-D028` (D-884 m7, m8 T528), `A254` (B-11 T528), `A255` (mal dönmedi T528). `12 §12.3` satırları `VAL-C103`, `C104`, `C106` D-981'in F5 kapısı kapsamında, değişmedi. Matris betiği `--kapsam 1-77` hata 0 |
| Bulgu sayısı (tur 2) | 0 |
| Düzeltme gerekli mi (tur 2) | Hayır |
| Bloke-etmeyen not (tur 2) | 3 — **K1** (forward → D-1550): kargo, teslim tarihi, ulaşma tarihi ve e-posta düzeltmesinin değer reddi formun içinde `role="alert"` mesajıdır, alana bağlı değildir (`aria-invalid`/`aria-describedby` yok); `04` 9.9.36 "Ret alan mesajıdır", 7.2.8.1, 7.2.8.3, 7.2.14.2, 7.2.14.4 "Alan mesajı" ile 2.3.3'ün "kural dışı değerin reddi yerinde kalıcı mesaj" cümlesi yan yana; T527'nin kargoya verme ve teslim işareti formları aynı düzende — sayfa düzeyinde karar. **K2** (forward → D-1551): UC-149 formu `ArrivalRecordId` (`[SpecAssignedName]`) taşır, `07` `CorrectArrivalDateInput` `ReturnId` der; alan teslim almanın ve iade reddinin kaydını alır (`04` 6.3.9.16 retta da ulaşma tarihi düzeltilir; `06 §4.27` `ArrivalDate`) — `07`'nin adı dar, kodun adı belgede yok. **K3** (D-1544'e not): doğrulayıcının geçici sınaması — T530'un kendi girişleri de (`Input.TransitionRecordId=abc`, `Input.IsOwnVehicleDelivery=maybe`, `Input.Version=abc`) 500 verir; T527'nin `MarkDelivered`'ı da; kalemin "T530 kendi riskini kapattı" cümlesi yalnız `AddressType` için doğru |
| Yapım raporu karşılaştırması (tur 2) | 2 uyuşmazlık: (1) öz denetimin "09 §25: yeni adlar `06`/`07`'den ✓" satırı `ArrivalRecordId`'yi `07`'nin adı sayar; `07` `ReturnId` der (K2). (2) Known Limitations 4 "T530 kendi girişini metin aldı" — T530'un Guid, bool ve sürüm girişleri de 500'e düşer (K3). Öteki iddialar uyumlu: B1–B3, K2–K4 kapanışları testli; sayılar yeniden koşuldu (Unit 7643 = 7348 + 295; Integration sınıfları 50 + 146 doğrulayıcının seçimiyle) |
| Doğrulama durumu (tur 1) | ✗ FAIL (2026-11-14; dal ucu `13c7393a`, origin/main `0289add9` ile birleşik, main ilerlemedi) — build `-warnaserror` 0 uyarı, format ×3 temiz, Unit 7345/7345 + Consistency 295/295, betik 4/4, Integration parçalı `OrderDetailPageTests` 38/38 + `OrderCorrectionTests`, `PaymentWindowTests`, `StatusCorrectionTests`, `OrderOperationsTests` 146/146 (TAM: CI); dal CI'ı run 43571321510 yeşil |
| `12` satırları (tur 1) | PASS: `VAL-C055`, `C056`, `C059`, `A246`; BEKLEMEDE: `VAL-A239`, `A244` (KRİTİK; beklenen sonuç kaynağını aşıyor — D-894 okuması D-1525'e bağlandı), `VAL-A240`, `D026`, `D027` (yeni değerin sayfada görünmesi testte sınanmıyor — K3), `VAL-A245` (eski bağlantının uçta açılmaması yalnız servis düzeyinde), `VAL-D028`, `A254`, `A255` (T528/T531 payı var). Matris betiği hata 0 |
| Bulgu sayısı (tur 1) | 3 (ORTA, S1) |
| Düzeltme gerekli mi | Evet — **B1 (kriter 2, KK2; `04` 7.2.14.1 "Alan mesajı", 2.3.1.1, 2.3.2):** reddedilen adres düzeltmesi 422 ve sayfa üstünde "Adresin işaretli alanlarını düzeltin." der, formda hiçbir alan işaretlenmez ve alan mesajı yoktur — `KeepAddress` `AddressForms.AddRefusals` ile `ModelState`'e yazar ama çizilen `AddressFormView` `Bind` edilmez (Checkout `.Bind(errors)` yapar). Doğrulayıcının geçici sınaması: boş açık adresle 422 yanıtında `aria-invalid="true"` ve alan mesajı yok. KK2 testi yalnız sayfa metnini arar. **B2 (`04` 2.3.3 "yapılamayan durum düzeltmesi bu biçimdedir", 2.7.3.1, 9.9.41):** düzeltmelerin ret ve engel mesajı `order-step-message` olarak "Sıradaki adım"ın üstünde çizilir, işlemin başlatıldığı "Düzeltmeler" bölümünde değil; `RefusedCorrection` yazılır, hiçbir yerde okunmaz (sınıf yorumu "message in the form's place" der). Ulaşma tarihi formu reddedilince gönderilen tarihi taşımaz (`09 §16.2` m5). **B3 (`04` 2.3.1.1 "alanın etiketine bağlı"):** teslimat ve fatura formu aynı sayfada aynı `Input.` önekiyle çizilir; `Input_RecipientName`, `Input_ProvinceId`, `Input_District`, `Input_AddressLine` iki kez geçer, fatura formunun `label for` değerleri teslimat alanlarını gösterir (GET HTML'inde `id="Input_AddressLine"` ×2) |
| Bloke-etmeyen not (tur 1) | 4 — **K1** (forward, D-894 → D-1525): `12 VAL-A239`/`A244`'ün "teyit alınmadan düzeltme yapılamaz; teyit kayda geçer" beklentisi `02 §10.4.2` ("Ürün teyidi denetlemez"), `06` ("teyit kaydedilmez") ve `07 UC-146`/`UC-151` ("Onay istemez") ile çelişir; yapımın okuması (form gönderimi beyan, değer saklanmaz) bu belgelerle uyumlu. **K2** (bu PR'da): KK7'nin `RefundPending` sayfa testi gönderilmiş havale geri ödemesiyle kurulur; kriterin adı "kart iadesi beklenirken" — kart `OutcomePending` hâli yalnız çekirdekte (`StatusCorrectionTests` `CardOutcomePending`); sayfa testine kart hâli eklenir. **K3** (bu PR'da): kriter 4'ün sıra sorusu (K-608) panel yolu kalıcı testsiz — doğrulayıcının geçici sınaması geçti (beyan günü → 422, soru alanı ve "Beyanın anı", cevapla 303 ve tarih yazıldı); `VAL-A240`, `D026`, `D027`'nin "sayfada görünür" yarısı da sınanmıyor — sayfa sınaması eklenirse satırlar PASS olur. **K4** (bu PR'da): çekirdeğin `InvalidInput`'u — tanımsız `AddressType`, sürümsüz istek — panelde "kayıtlı değerle aynı" (`Unchanged`) metnine düşer |
| Yapım raporu karşılaştırması (tur 1) | 3 uyuşmazlık: (1) "Yapılan İşler" ve kriter 2 "mesaj yerinde, form korunur" der; mesaj sayfa üstünde, adres alanları işaretsiz, ulaşma tarihi korunmuyor (B1, B2); (2) öz denetim KK2 "alan 422 ✓" kanıtı yalnız sayfa metni — alan mesajı sınanmamış (B1); (3) "Notlar"daki "ret mesajı T527 düzenindeki gibi ortak yerde" kararı ⚠ olarak kaydedilmemiş ve `04` 2.3.3 ile çelişir (B2). Öteki iddialar uyumlu: D-852, D-883 kapanışları ve D-884 m5 iz satırı testli; D-1525…D-1527 okumaları belgeyle çelişmiyor; komut sayıları yeniden koşulup tutuldu (Unit sayısı main'in ilerlemesiyle 7590 → 7640) |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T530-siparis-duzeltmeleri`
- **Commit:** `e947183b` — kod ve testler; düzeltme 1 `92f5219f` + main birleştirmesi `1ce5cf11`
- **PR:** #573
- **CI:** run 43577917177 (head `02146b34`) yeşil; post-merge yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan tier, kota, dış servis, paket sürümü tetikleyicisi yok; yeni paket yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | ⚠ Adres/e-posta teyidi istekte yok; panel formu teyidi beyan sayar (`FromOrderEmail`, kimlik dört alan) | D-1525 | `07`/`04` alan koymuyor, T375 yazıcısı istiyor — belge kararı |
| 2 | ⚠ Kayıtlı değerle aynı düzeltme 422 + `ValueOutOfBounds`; `07`'de bu hâl yok | D-1526 | `07` 422 listesi kararı |
| 3 | ⚠ Adres düzeltmesi iki ayrı formla (`AddressType`), tek istekte tek adres | D-1527 | T375 sözleşmesi tek adres tipi |
| 4 | ⚠ Bağlanamayan değer (ör. `Version=abc`, sayı olmayan enum) sipariş ayrıntısında `ValidationPageFilter`'ın sayfayı yüklemeden `Redisplay`'i ile 500 | D-1544 | ortak sayfanın (T527) yükleme düzeni; T530 kendi girişini metin aldı |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 43566446647 success, 43566446578 success, 43560690928 success.
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- **Ortak sayfa:** T529/T531 ile paylaşılan dosyalarda yalnız kısa ekleme: `OrderDetailModel` `partial` oldu, `LoadAsync`'e `await LoadCorrectionsAsync(id, ct);`, `OrderDetail.cshtml`'e tek `<partial>` satırı; işleyiciler ve formlar ayrı parça dosyalarında.
- **⚠ kararları (kurala en uygun seçenek):** D-1525…D-1527, D-1544. (Yapımdaki "ret mesajı ortak yerde" kararı düzeltme 1'de kaldırıldı — B2.)

## Düzeltme 1 (2026-11-14; doğrulama tur 1 FAIL → B1–B3, K2–K4)

- **Alan bağlama (B1):** reddedilen adres alanı `aria-invalid="true"` ve altında kendi mesajıyla çizilir; formun başında hata sayısını söyleyen özet, bağlantısı alanın kimliğine (`#Input_ShippingAddressLine`) gider, odak ilk hatalı alana (`autofocus`). Ayrı "Adresin işaretli alanlarını düzeltin." metni kaldırıldı (özet söyler).
- **Mesaj yeri ve korunan değer (B2):** düzeltmelerin ret ve engel mesajı (`CorrectionRefusalText`) gönderilen formun içinde, düğmenin yanında (`correction-message`); form artık çizilmiyorsa (işlem kapandı) "Düzeltmeler" bölümünün başında. Ortak `RefusalText` (Sıradaki adım) düzeltmelerde kullanılmaz. Ulaşma tarihi formu gönderilen tarihi, durum düzeltmesi seçilen geçişi ve sebebi korur.
- **Form kimlikleri (B3):** adres formları `07 UC-146` adlarıyla (`Input.Shipping…`, `Input.Billing…`) ayrı kimlik üretir; etiketler kendi alanına, özet doğru forma. Durum düzeltmesi iki geçişte tek form + seçim (tek sebep alanı) — aynı sayfada iki `Input_CorrectionReason` olmasın.
- **K2:** kart iadesinin sonucu beklenirken (`OutcomePending`) yanlış iptal düzeltmesi sayfa testi. **K3:** K-608 sıra sorusunun panel yolu kalıcı testli; düzeltilen takip no, teslim tarihi, ulaşma tarihi, adres ve e-posta geçmişi sayfada sınanır (VAL-A240, D026, D027). **K4:** `AddressType` metin alınır; tanımsız değer 422 + `AddressTypeMissingText` ("kayıtlı değerle aynı" değil), sürümsüz istek 409 (sürüm 0 eşleşmez) — ikisi de yazım yapmaz. K1 değişmedi.
- **Yeni testler:** `TheCorrections_RepeatNoElementId_AndEachAddressFormsLabelsAreItsOwnFields_B3`, `CorrectAddress_ThatNamesNoAddress_Is422WithItsOwnMessage_AndOneWithoutTheVersion_Is409_K4`, `CorrectDeliveryDate_OnAWithdrawalDeclarationsDay_AsksWhichCameFirstInItsForm_ThenTheAnswerWritesTheDate_ShownOnThePage_K608`, `CorrectTransition_OfAWrongCancellation_WhileTheCardRefundsOutcomeIsAwaited_Is422_RefundPending_KK7`; `_KK1`, `_KK2`, `_KK3`, `_KK5`, `_KK6`, `_RefundPending_KK7`, `_KK9_KK10`, `_KK10` güçlendirildi.
- **Main birleştirmesi:** `6e6d4a41` (T531) — `12` (C059 bizim, A241 main), `DEFERRED_BACKLOG` (D-883 bizim, D-884'e iki notu birlikte, hedef T528), `IMPLEMENTATION_STATUS` (T530 bizim, T531 main), `OrderingModule` ve `OperationHandlerMarkTests` iki taraf.
