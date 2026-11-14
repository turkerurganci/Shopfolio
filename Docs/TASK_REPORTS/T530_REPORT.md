# T530 — Sipariş ayrıntısı: adres, kargo, teslim tarihi, durum geçişi ve e-posta düzeltmeleri (E-37)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-14

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
| 1 | `CorrectAddress` teslimat/fatura adresi ve telefon; eski-yeni geçmişte, adres ize girmez | ✓ | `OrderDetailPageTests.CorrectAddress_TheShippingAddressWithItsPhoneAndTheBillingAddress_Are303_WriteTheHistory_AndTheTrailHasNoAddress_KK1` |
| 2 | UC-146 422: alan kuralı; Kargoya verildi'den (fiziksel kalemsizde Teslim edildi'den) sonra kapalı | ✓ | `…CorrectAddress_AFieldItRefuses_Is422WithTheFieldsMessage_AndItIsClosedFromShipped_OrWithNoPhysicalItemFromDelivered_KK2` |
| 3 | `CorrectCarrier`; boş bilgi ya da kargoya verilmemiş sipariş 422 | ✓ | `…CorrectCarrier_OnAShippedOrder_Is303_SendsB12_AndWritesTheTrail_TheEmptyInfoAndAnOrderNotShipped_Are422_KK3` |
| 4 | `CorrectDeliveryDate` pencere ve ayıp süresi yeni tarihten; 422 tarih hâlleri, işaretsiz teslim | ✓ | `…CorrectDeliveryDate_RunsTheWithdrawalWindowAndTheDefectPeriodFromTheNewDate_AndWritesTheTrail_KK4`, `…_AFutureDate_ADateBeforeTheShipment_ADateAfterAReturnsArrival_AndAnUnmarkedOrder_Are422_KK4` |
| 5 | `CorrectArrivalDate` Z-16 yeni tarihten; sınır dışı tarih 422 | ✓ | `…CorrectArrivalDate_RunsTheRefundPeriodFromTheNewDate_AndAFutureDateOrOneBeforeTheDelivery_Is422_KK5` |
| 6 | `CorrectTransition` yeni geçiş, dönülecek durum kayıttan, sebep ve "diğer"de açıklama | ✓ | `…CorrectTransition_AsksAReason_AndTheNoteOfOther_ThenWritesTheCorrectionOnTheCorrectedRecord_AndTheTrail_KK6`; `PanelCorrectionTextsTests.TheBackState_*_KK6` |
| 7 | UC-150 422: dört engel, engel mesajıyla | ✓ | `…_RefundPending_KK7`, `…_StockShort_KK7`, `…_ItemRecordExists_KK7`, `…_CardPayment_KK7`; `PanelCorrectionTextsTests.EveryObstacle_HasItsOwnMessage_KK7` |
| 8 | Yanlış iptal düzeltmesi yeniden ayırır; geri ödeme, IBAN isteği, IBAN kapanır | ✓ | `…CorrectTransition_OfAWrongCancellation_TakesTheUnitsAgain_VoidsTheRefund_ClosesTheIbanRequest_AndDeletesTheIban_KK8` |
| 9 | `CorrectOrderEmail` anahtar, eski bağlantı, hesap bağı, gönderilmemiş bildirimler | ✓ | `…CorrectOrderEmail_RenewsTheKey_ReroutesTheUnsentNotices_SendsB1ToTheNewAndB15ToTheOld_AndKeepsTheHistory_KK9_KK10`, `…_AVerifiedAccountOfTheNewAddress_TakesTheOrder_KK9` |
| 10 | B-1 yeni, B-15 eski adrese; adresler geçmişte, izde yok; üyeli/imha/geçersiz 422 | ✓ | `…_KK9_KK10` (yukarıda), `…CorrectOrderEmail_AnInvalidAddress_AnAccountsOrder_AndADisposedOrder_Are422_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: geçmiş satırı; izde adres yok | ✓ | `_KK1` testi: iki `OrderAddressHistory` satırı (eski/yeni hat ve telefon); dört serileştirilmiş iz satırında adres ve telefon parçası yok |
| KK2: alan 422; iki sınır | ✓ | `_KK2`: boş açık adres 422 + `AddressFieldsText`; fiziksel Shipped 422; fiziksel kalemsiz Preparing 303, Delivered 422 |
| KK3: iki 422 sebebi | ✓ | `_KK3`: boş bilgi `ShipmentMissingText`, Preparing `NotOpenText`; 303'te B-12 ve `CorrectShippingInfo` (takip no izde yok) |
| KK4: pencere, süre yeni tarihten; tarih reddi | ✓ | `_KK4` iki test: `PanelItemFacts.WithdrawalLastDay`/`DefectLastDay` geriye kayar; ileri, kargodan önce, ulaşmadan sonra ve işaretsiz sipariş 422 |
| KK5: Z-16; iki tarih reddi | ✓ | `_KK5`: ileri ve teslimden önceki tarih 422 (`ArrivalDateInvalidText`, K-554 tek metin); `WithdrawalRefundStartDay` yeni tarih |
| KK6: sebep zorunlu; geçiş kaydı | ✓ | `_KK6`: sebepsiz ve "diğer"de notsuz 422; `CorrectedTransitionRecordId` + not kayıtta; iz `CorrectTransition` (S5, not yok); B-13 |
| KK7: dört engel ayrı testte | ✓ | dört `_KK7` testi, her biri engelin kendi mesajı + 422 + yazım yok |
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

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ TAM | `dotnet test --project tests/Shopfolio.UnitTests/...` | 7590/7590 |
| Integration | ✓ sınıflar — TAM: CI | `--filter-class` | `Web.OrderDetailPageTests` 38/38; `Ordering.OrderCorrectionTests` + `Ordering.OrderOperationsTests` 68/68; `Ordering.PaymentWindowTests` + `Ordering.StatusCorrectionTests` 78/78 |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
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
- **Commit:** `e947183b` — kod ve testler
- **PR:** #573
- **CI:** — (yardımcı izler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan tier, kota, dış servis, paket sürümü tetikleyicisi yok; yeni paket yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | ⚠ Adres/e-posta teyidi istekte yok; panel formu teyidi beyan sayar (`FromOrderEmail`, kimlik dört alan) | D-1525 | `07`/`04` alan koymuyor, T375 yazıcısı istiyor — belge kararı |
| 2 | ⚠ Kayıtlı değerle aynı düzeltme 422 + `ValueOutOfBounds`; `07`'de bu hâl yok | D-1526 | `07` 422 listesi kararı |
| 3 | ⚠ Adres düzeltmesi iki ayrı formla (`AddressType`), tek istekte tek adres | D-1527 | T375 sözleşmesi tek adres tipi |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 43566446647 success, 43566446578 success, 43560690928 success.
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- **Ortak sayfa:** T529/T531 ile paylaşılan dosyalarda yalnız kısa ekleme: `OrderDetailModel` `partial` oldu, `LoadAsync`'e `await LoadCorrectionsAsync(id, ct);`, `OrderDetail.cshtml`'e tek `<partial>` satırı; işleyiciler ve formlar ayrı parça dosyalarında.
- **⚠ kararları (kurala en uygun seçenek):** D-1525…D-1527; ayrıca ret mesajı T527 düzenindeki gibi sayfanın ortak mesaj yerinde (`RefusalText`) çizilir.
