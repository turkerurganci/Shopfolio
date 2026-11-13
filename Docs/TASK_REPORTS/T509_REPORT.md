# T509 — Ödeme adımı (E-13)

**Faz:** F5 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /odeme` (`Pages/Storefront/Checkout.cshtml(.cs)`, `[NoStore]`, noindex): tek sayfa, tek form; bölümler `03 §2.4` sırasıyla (iletişim, fiziksel kalemde teslimat adresi, fatura adresi, yöntem, özet, onay). Her gösterimde yeni `ConfirmationKey`, özet parmak izi ve iki metnin sürüm kimlikleri; misafir e-postası onay özetinde, e-posta alanına düzeltme bağlantısıyla (D-729).
- Beş işleyici (`ApplyCoupon`, `RemoveCoupon`, `Recalculate`, `Leave`, `PlaceOrder`) aynı formda `formaction` ile. Geçiş kapalıysa (T508 k3) GET ve üç ara gönderim 303 → `/sepet`.
- Çerez (`CheckoutForms.cs`, `CheckoutCookieValues`) yalnız `05 §6.7.9` alanlarını taşır, yalnız ayrılışta yazılır, sipariş oluşunca silinir; kutular, anahtar, parmak izi, sürüm yazılmaz.
- Onay `05 §4.2.2`'nin tek işleminde (`ITransactionRunner.RunAsync(work, PlaceOrderRequest.RetryOnUniqueIndexes, ct)`; D-563 tek ev); sonuç eşlemesi: teyit 303, 422 (eksik alan/kutu, il, özet farkı, kapanan satış, yöntem), 429 (L-7, L-8; adressiz istek de nötr 429 — D-805), 404 (yabancı defter girdisi), 303'ler (kalem yok / asgari tutar → `/sepet`, oluşmuş sipariş → teyit, belirsiz sonuç → `IOutcomeTarget` sonuç ekranı `[OutcomeUncertain("UC-27")]`, kapanan oturum → giriş).
- Modül yüzü: `ICheckout` (okuma, onay, `MarkPaymentUnavailableAsync`), `OrderConfirmation.ReadStepAsync` (kilitli okuma, kupon kararı, önceki ödenmemiş sipariş), `SummaryChanges` (değişiklik satırları), `Carts.MarkPaymentUnavailableAsync` (UC-26).
- Bu tur: cayma özetinde iki fiziksel cümle yalnız fiziksel kalemde (D-743); hesabı silinen oturumun siparişi misafir yoluna düşer ve doğrulanmış hesaba bağlanır (D-385); hedefi T509 olan backlog kalemleri kapatıldı ya da gerekçeyle ileri atıldı.
- **Düzeltme 1 (doğrulama tur 1 B1, VAL-E061):** sağlayıcıya iki dakika erişilemeyen kart artık listeden çıkmıyor: sayfa `IsCardUnavailableNowAsync`'i `ListOpenAsync`'ten ayrı okur, `ShownMethods` kartı sabit sırada (kart, havale) listede tutar, radyosu `disabled`, yanında `CheckoutTexts.CardUnavailableNow`; `ChosenMethod` bu kartı seçmez, `HasNoUsableMethod` = seçilebilir yöntem yok (`OpenMethods` boş). Kart seçili sahte onay 422 `PaymentMethodUnavailable`, sipariş ve bildirim yok. Test: `CheckoutPageTests.TheCardUnavailableNow_StaysInTheListUnselectableWithItsReason_AndItsConfirmationIsRefusedWith422_VAL_E061` (yardımcıya `cardOpen` düzeneği). D-1077'nin T509 payı tur 1'de "kapandı" yazılmıştı ama sayfada karşılanmıyordu; bu düzeltmeyle karşılandı.
- **Düzeltme 2 (doğrulama tur 2 B2–B4; VAL-A044, D-627, D-853):** (B2) `CheckoutStep` sepetin kargo hesabını (`ShippingAssessment Shipping`) taşır; özet kargo ücretinin hemen altında eşiğe kalan tutarı modülün metniyle yazar (`ThresholdMessages.FreeShipping` → `_ThresholdMessage`; `04` 5.13.10, `02 §3.19.5`); eşiğe ulaşınca kargo satırının kendisi "Kargo ücretsiz" der, ikinci satır çizilmez. (B3) kargo satırı yalnız fiziksel kalemde (`04` 5.13.26); metin ücret varsa "Kargo: N TL", yoksa `ShippingAssessment.FreeShippingText` ("Kargo: Kargo ücretsiz" çift sözcüğü gitti; metnin tek evi, `FreeShippingMessage` da onu kullanır). (B4) L-8 doluyken havale listede kalır, radyosu `disabled`, yanında `NeutralMessages.LimitExceeded`; kart açık kalır; kart da erişilemezse kullanılabilir yöntem yok, onay düğmesi yerine sepete dönüş ve `Leave` sepeti işaretler (`04` 5.13.8, 5.13.21; `07 4.13.1`; `03 §3.2.1.21`). L-8'in tek kuralı `OrderPlacementLimits.ClosesBankTransferAsync` (onaydaki `FindExceededAsync` de onu çağırır); sayfa `ICheckout.ClosesBankTransferAsync` ile sorar, sepetin onayda iptal edilecek kendi Alındı + Bekliyor siparişleri sayılmaz (`OpenUnpaidOrderLimit` `exceptCartId` aşırı yüklemeleri; K-837). Sayfa yöntem kararlarını tek yerde okur (`ReadMethodsAsync`; GET ve `Leave` aynı yol), `IsSelectable`/`ReasonOf` radyoyu ve sebebi verir. Testler: `CheckoutPageTests.TheSummary_TellsTheRemainingAmountToFreeShippingUnderTheFee_SaysFreeShippingOnce_AndADigitalCartHasNoShippingLine_VAL_A044`, `AFullOpenUnpaidCeiling_KeepsTheTransferListedUnselectable_TheCardOpen_AndWithTheCardUnavailableLeavesNoMethod_T509_KK1`.
- **Düzeltme 3 (doğrulama tur 3 B5–B6; VAL-A841, VAL-A842; K-756):** (B5) onay bölümünde Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi'nin tam metni artık `<details>` arkasında değil: her biri başlığıyla (`h3`) sayfada açık, sabit yükseklikte kaydırılan bir kutuda durur (`checkout__legal-box`, `role="region"`, `tabindex="0"` — klavyeyle kaydırılır; metin `legal-text__plain` ile satır sonlarını korur; `04` 5.13.9 (c)–(d), 5.13.32). Kutunun stili `storefront.css`'e eklendi (var olan sınıf yoktu): `height: 16rem`, `overflow-y: auto`, metin gövde boyutunda (`--font-size-base`; K-832, küçültülmez); hiçbir ekran sınıfında katlanmaz, medya sorgusu yok. (B6) misafirin "Siparişiniz şu adrese gönderilecek: …" satırı ve "Düzelt" bağlantısı onay özetinin (b) parçası olarak özet metninin hemen altına taşındı; onay kutuları ile ödenecek toplam ve düğme arasında artık yalnız (h) durur (`04` 5.13.9 (b), (h), 2.12.7.2). Test: `CheckoutPageTests.TheConfirmation_ShowsBothLegalTextsOpenInAScrollingBox_AndTheEmailLineInTheSummary_T509_KK1` (sayfada `<details>`/`<summary>` yok; iki metin kendi kutusunda, dolu ve birbirinden farklı; sıra özet → e-posta satırı → yasal kutular → onay kutuları; kutuların sonu ile toplam arasında e-posta satırı yok, satır tek). Yardımcı `LegalBox`.
- Tarama kuralları gevşetilmeden kod kurala uyduruldu: enum başına dosya (`PlaceOrderOutcome.cs`, `SummaryChangeKind.cs` + `SummaryChange.cs`), iki enum saklanmayan değer listesinde (`ApplicationEnums.NotStored`; sütun değil, sonuç kodu ve okuma biçimi), form `asp-antiforgery` + sayfa yoluyla `action`, gerekçesiz `!` gerekçe aldı, `07 §5.2` kalın alan adları ve dört kutu alanı `[SpecAssignedName]`, erişim anahtarı parametresi varsayılansız (ölü `OrderConfirmationResult.Placed` silindi), KK2 testi iddiayı testin kendisinde yapar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Storefront/Checkout.cshtml(.cs)`, `CheckoutForms.cs`, `PlaceOrderInput.cs`; düzeltme 3: `src/Shopfolio.Web/wwwroot/css/storefront.css` (`checkout__legal-box`)
- `src/Shopfolio.Web/Modules/Ordering/` (`Checkout.cs`, `Contracts/CheckoutContracts.cs`, `Contracts/PlaceOrderOutcome.cs`, `Contracts/SummaryChangeKind.cs`, `Contracts/SummaryChange.cs`, `OrderConfirmation.cs`, `OrderIdentifiers.cs`, `Carts.cs`, `ConfirmationStepRules.cs`, `OrderingModule.cs`; düzeltme 2: `OpenUnpaidOrderLimit.cs`, `OrderPlacementLimits.cs`, `Contracts/ShippingCharge.cs`; `SummaryComparison.cs` kaldırıldı), `Modules/Membership/` (`IMembershipModule`, `MembershipModule`)
- Testler: `tests/Shopfolio.IntegrationTests/Web/CheckoutPageTests.cs` + `.Helpers.cs`, `Web/StorefrontFrameTests.cs`, `Model/MembershipTests.cs`, `Ordering/MainFlowTests.cs`, `Ordering/OrderConfirmationTests.cs`, `Ordering/OpenSalesGate.cs`, `Ordering/OrderingTestServices.cs`, `Administration/PanelHomeTests.cs`, `OutcomeUncertainConsistencyTests.cs`; `tests/Shopfolio.UnitTests/Pages/CheckoutFormsTests.cs`, `Ordering/ConfirmationStepTests.cs`, `Support/ApplicationEnums.cs`
- Kayıt: `Docs/DEFERRED_BACKLOG.md` (yerinde kapatma/ileri atma), `Docs/DEFERRED_BACKLOG.d/T508.md`, `T537.md`, `T544.md`, `T509.md` (D-1333, D-1334), `Docs/STATUS_CHANGELOG.d/T509.md`, `Docs/IMPLEMENTATION_STATUS.md` T509 satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Bölümler tek sayfada; yeni anahtar, parmak izi, iki sürüm; misafir e-postası özette, kodsuz, düzeltilebilir | ✓ | `CheckoutPageTests.TheGet_DrawsTheSectionsOfTheCart_WithAFreshKey_TheFingerprintAndTheVersions_T509_KK1`; yöntem bölümünde iki dakikalık kart hâli (`04` 5.13.8, `07 4.13.1`): `TheCardUnavailableNow_StaysInTheListUnselectableWithItsReason_AndItsConfirmationIsRefusedWith422_VAL_E061` (düzeltme 1); `07 4.13.1`'in öteki iki hâli (düzeltme 2): özetin eşik satırı ve dijital sepette kargo satırı yok `TheSummary_TellsTheRemainingAmountToFreeShippingUnderTheFee_SaysFreeShippingOnce_AndADigitalCartHasNoShippingLine_VAL_A044` (`04` 5.13.10, 5.13.26), L-8 doluyken havale listede seçilemez + nötr mesaj, kart açık, kart da erişilemezse sepete dönüş `AFullOpenUnpaidCeiling_KeepsTheTransferListedUnselectable_TheCardOpen_AndWithTheCardUnavailableLeavesNoMethod_T509_KK1` (`04` 5.13.8, 5.13.21); onay bölümü (düzeltme 3): iki yasal metin açık, kaydırılan kutuda; misafir e-posta satırı onay özetinde `TheConfirmation_ShowsBothLegalTextsOpenInAScrollingBox_AndTheEmailLineInTheSummary_T509_KK1` (`04` 5.13.9 (b)–(d), (h), 5.13.32; K-756) |
| 2 | Geçiş kapalıysa UC-22…UC-25 303 → `/sepet` | ✓ | `TheStepThatCannotOpen_SendsTheGetAndTheThreeSubmissionsToTheCart_T509_KK2` |
| 3 | Çerezden dolar; yalnız ayrılışta, yalnız `05 §6.7.9` alanları | ✓ | `TheLeaving_WritesTheCookieWithoutValidating_…_T509_KK3_KK6`; Unit `CheckoutFormsTests.TheCookie_KeepsOnlyTheDocumentedFields_…` |
| 4 | `ApplyCoupon` 200 yazmaz; 422 iki biçim; 429 L-6; hak ayrılmaz | ✓ | `TheCoupon_IsAppliedWithoutWriting_RefusedWith422_AndRemoved_T509_KK4_KK5`, `TheCouponAttempts_AreClosedByL6With429_T509_KK4` |
| 5 | `RemoveCoupon`; `Recalculate` zorunluluk denetlemez; hatırlatma L-3'e sayılmaz | ✓ | `…_KK4_KK5`, `TheRecalculation_ChecksNoRequiredField_TellsAnUnservedProvince_AndTheRegisteredEmailReminder_T509_KK5` |
| 6 | `Leave` doğrulamasız çerez; üç hedef; yöntem yoksa işaret | ✓ | `…_KK3_KK6`, `TheReturnToTheCart_WithNoUsableMethod_MarksTheCart_T509_KK6` |
| 7 | Tek işlem, 303 teyit, çerez silinir | ✓ | `TheConfirmation_PlacesTheOrder_GoesToItsConfirmation_DeletesTheCookie_AndTheRepeatOpensNoSecondOrder_T509_KK7_KK10` |
| 8 | 422 beş sebep; 429 L-7/L-8; 404 yabancı defter | ✓ | `…RefusesMissingFieldsAndBoxesTogether_With422_…_KK8`, `…AnUnservedProvince_AChangedSummary_AndAnUnusableMethod_With422_T509_KK8`, `…_VAL_E061` (iki dakikalık kart 422, sipariş ve bildirim yok), `…TheClosedSale_With422_AndAForeignBookEntry_With404_T509_KK8`, `AFullOpenUnpaidCeiling_RefusesTheNextConfirmationWith429_T509_KK8`, `AConfirmationWithoutAClientAddress_IsRefusedWithTheNeutral429_AndNoOrder_T509_KK8` |
| 9 | 303'ler: kalem yok / asgari tutar → sepet; oluşmuş → teyit; belirsiz → sonuç ekranı; kapanan oturum → giriş | ✓ | `…SendsTheCartsLostLinesToTheCart_AndAClosedMemberSessionToTheSignIn_T509_KK9`, `…UnderTheMinimumAmount_GoesToTheCart_T509_KK9`, `AnUnexpectedError_…_T509_KK9`, `…_KK7_KK10` (aynı anahtar → teyit) |
| 10 | Aynı anahtar ikinci sipariş açmaz; iki sekme kilitle sıraya girer | ✓ | `…_KK7_KK10`, `TwoTabs_WithTwoKeys_AreQueuedByTheCartsLock_AndTheSecondCancelsTheFirstsUnpaidOrder_T509_KK10` |

> `? Doğrulanamadı` **FAIL değildir** — kanıt eksikliğidir. FAIL'den ayrı tutulur.

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil kriter 1–10 ve KK1–KK10 diff'te denetlendi; `sik-fail-nedenleri.md` ana türleri ve hedefi T509 olan backlog kalemleri ayrıca tarandı.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1: sepete göre bölüm, taze anahtar; misafir e-postası özette | ✓ | KK1 testi (dijital sepette teslimat bölümü yok; iki GET farklı anahtar; e-posta + düzeltme bağlantısı) |
| Düzeltme 1 — KK1 yöntem bölümü: kullanılamayan yöntem listede kalır, seçilemez, sebebi yanında (`04` 5.13.8; `07 4.13.1`; VAL-E061) | ✓ | `…_VAL_E061`: önce açık kart seçili ve sebepsiz (testin boş kümeyi karşılaştırmadığının kanıtı), `Mark()` sonrası kart radyosu `disabled`, seçili değil, "şu an kullanılamıyor" yanında, havale seçili; yalnız kart açıkken kart listede + "kullanılabilecek yöntem yok" mesajı |
| Düzeltme 2 — KK1 özet: eşiğe kalan tutar ücretin hemen altında, eşikte tek "Kargo ücretsiz", dijital sepette kargo satırı yok (`04` 5.13.10, 5.13.26; `02 §3.19.5`; VAL-A044, D-627) | ✓ | `…_VAL_A044`: beklenen metin modülden türetilir (`ThresholdMessages.FreeShipping(ShippingCharge.Assess(...))`, elle sayı yok); ücret satırı + eşik satırı bitişik (düzenli ifade), eşikte eşik satırı ve "Kargo: Kargo ücretsiz" yok, dijital sepette `checkout__shipping` ve `threshold-message` yok |
| Düzeltme 2 — KK1 yöntem: L-8 doluyken havale listede, seçilemez, nötr mesaj; kart açık; kart da erişilemezse yöntem yok (`04` 5.13.8, 5.13.21; `07 4.13.1`; D-853) | ✓ | `…ListedUnselectable…_T509_KK1`: tavan IP ekseninde gerçek onaylarla doldurulur; havale `disabled` + `NeutralMessages.LimitExceeded`, kart `checked`, "yöntem yok" yok; sepetin kendi siparişi tavanı kapatmaz (K-837); sahte havale onayı yine 429, sipariş sayısı değişmez; `Mark()` sonrası "yöntem yok" + düğme yerinde sepete dönüş, `Leave` sepeti işaretler |
| Düzeltme 2 — tek kural evi (L-8) | ✓ | Liste ve onay aynı işlevi sorar: `OrderPlacementLimits.ClosesBankTransferAsync` (`FindExceededAsync` → onu çağırır); sayfa kendi sayımını yazmaz; `OpenUnpaidOrderLimit` eski imzalar `exceptCartId: null`'a iner (davranış aynı). Sepetin dışlanan siparişi yalnız `Placed` + `Pending` (onayın `CancelEarlierUnpaidOrdersAsync` kümesi) |
| Düzeltme 2 — KK6/KK8 yan etkisi | ✓ | `Leave` ve GET yöntem kararını tek yoldan okur (`ReadMethodsAsync`; `HasNoUsableMethod => !ShownMethods.Any(IsSelectable)`); onay yolu değişmedi (L-8 yine onayın işleminde, 429); KK6/KK8 testleri yeşil (Test Sonuçları) |
| Düzeltme 3 — KK1 onay bölümü: iki yasal metin açık, kaydırılan kutuda, hiçbir sınıfta katlanmaz (`04` 5.13.9 (c)–(d), 5.13.32; K-756, K-832; VAL-A841, VAL-A842) | ✓ | `…ShowsBothLegalTextsOpenInAScrollingBox…_T509_KK1`: sayfada `<details>`/`<summary>` yok, iki kutu (`checkout__legal-box`, başlığa `aria-labelledby`) dolu ve farklı metin taşır (eski işaretlemede test kırmızı: `<details>` var). `storefront.css`: `height` + `overflow-y: auto`, medya sorgusu yok (her sınıfta aynı), metin `--font-size-base` |
| Düzeltme 3 — KK1 e-posta satırının yeri ve onay bölümünün tam sırası (`04` 5.13.9 (a)–(h), 2.12.7.2) | ✓ | Satır özet metninin hemen altında (b); işaretleme sırası (a) değişiklik/önceki sipariş mesajı → (b) özet + e-posta satırı → (c) form kutusu → (d) sözleşme kutusu → (e) nasıl kurulur + aydınlatma bağlantısı → (f) cayma özeti → (g) kutular → (h) toplam + düğme; test sırayı ve kutular ile toplam arasında e-posta satırı olmadığını sınar. ⚠ kutular ile düğme arasında yalnız ret anında görünen yerinde mesajlar (`ActionMessage`, genel form mesajları; `04` 5.13.18) ve düğmenin yükümlülük cümlesi (K-544) durur — değişmedi |
| Düzeltme 1 — KK6/KK8 yan etkisi | ✓ | `Leave` işaret koşulu değişmedi (`ListOpenAsync` + `IsCardUnavailableNowAsync`); KK6 ve KK8 testleri yeşil; kart seçili onay 422, sipariş 0, `dispatch_records` 0 |
| KK2: üç sebep, dört uçta | ✓ | KK2 testi (GET, ApplyCoupon, RemoveCoupon, Recalculate; satış kapalı, kapı, özet yok) |
| KK3: çerez alan listesi; kutu yazılmaz | ✓ | Unit `CheckoutFormsTests` (alan listesi = `05 §6.7.9`), KK3_KK6 testi |
| KK4: üç sonuç, L-6; 200 yazmaz, hak ayırmaz | ✓ | KK4 testleri (`reserved_count` değişmez) |
| KK5: kaldırma, ara gönderim, hatırlatma | ✓ | KK4_KK5, KK5 testleri |
| KK6: üç hedef, boş formla da; işaret | ✓ | KK3_KK6, KK6 testleri |
| KK7: tek işlem, çerez silinir | ✓ | KK7_KK10 testi; işlem `Transactions.RunAsync` sayfada (modül işlem açmaz, `09 §9.1` m.3) |
| KK8: beş 422 sebebi, 429, 404 | ✓ | KK8 testleri (5) |
| KK9: beş 303 sebebi | ✓ | KK9 testleri (3) + KK7_KK10 |
| KK10: iki sekme sıraya girer | ✓ | `TwoTabs_…_KK10` |
| `09 §8.1` m.4/m.6: geniş `catch` yok, tek hata katmanı | ✓ | `[OutcomeUncertain("UC-27")]`; `OutcomeUncertainConsistencyTests` (UC-27 `NotMarkedYet`'ten çıkarıldı) |
| `09 §15`: istemci adresi tek kaynaktan | ✓ | `ClientAddress.Of` (`RequestOriginSourceScan` kuralı) |
| `09 §9.4` m.3: tekil dizin yeniden denemesi işlemin sahibinde, liste tek ev | ✓ | `PlaceOrderRequest.RetryOnUniqueIndexes`; `OrderIdentifiers.UniqueIndexes` ona bağlı; `OrderAccessTests` |
| `09 §3.4` tek kural evi | ✓ | e-posta `EmailRule.Allows`, kutular `ConsentBoxes.Missing`, il `AddressForms.CheckProvince`, yöntemler `IPaymentMethods`, L-8 `OrderPlacementLimits.FindExceededAsync` (onayda) |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | Yeni istisna yok; `OutcomeUncertainConsistencyTests` listesinden bir öğe çıktı (daraldı) |
| Sık FAIL 2: kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama kuralı yok; mevcut taramalar yeni sayfayı da görür (Integration TAM) |
| Sık FAIL 3: "Varsayılan" DB default'u / FK dizini | ✓ | Şema değişikliği yok |
| Sık FAIL 4: kural değişince aynı sayıyı anan öteki yerler | ✓ | UC-27 işareti tek listede güncellendi; tekil dizin listesi tek evde |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09` değişmedi |
| Hedefi T509 olan backlog kalemleri | ✓ | Kapandı: D-385, D-563, D-626, D-729, D-743, D-785, D-805, D-807, D-853 (düzeltme 2; tur 2'ye kadar yanlış gerekçeyle F5 gate'e atılmıştı — B4); T509 payı kapandı: D-627 (düzeltme 2; tur 2'ye kadar bu satırda sayılmamıştı — B2), D-254, D-437, D-801, D-1077 (tur 1'de iddia sayfada karşılanmıyordu — FAIL B1; düzeltme 1'de sayfa + `…_VAL_E061` testiyle karşılandı; backlog metni artık doğru), D-1225 (D-1080: yapım payı düzeltme 1'de karşılandı, `VAL-E061` tarayıcı gözlemi doğrulamanın); ileri atıldı (gerekçeli): D-435, D-650, D-769, D-1189, D-1190, D-1245, D-1246 → F5 gate; D-1127 → T510 |
| Modül kurucusuna yeni bağımlılık, elle kuran test düzenekleri | ✓ | `Checkout` kurucusuna `Carts`; `OrderingTestServices` kullanan bütün sınıflar koşuldu (Test Sonuçları); Integration TAM doğrulayıcıda |
| Sık FAIL (modül/akış değişince elle kuran test düzenekleri) | ✓ | Satış kapısı onaya girince `OrderingTestServices`/`MainFlowTests` düzeneği güncellendi (`OpenSalesGate`); gerçek kapıyı okuyan testler gerçek `SettingsModule`'ü kurar (`OrderConfirmationTests` iki satır, `PanelHomeTests` kurulum listesi KK8/KK9) |
| `09 §25` doküman uyumu: yeni adlar `07`/`06`'dan; diff yalnız görev; yeni ayar/sabit yok | ✓ | Model alanları `07 §5.2` (sapma ⚠ (a), D-1333); `appsettings`/`.env.example` değişmedi; diff `Pages/Storefront`, `Modules/Ordering`, `Modules/Membership` ve testlerle sınırlı |
| `09 §25` mimari: modül sınırı, `SaveChanges` yok, tek işlev, `!`, `TimeProvider` | ✓ | Sayfa yalnız `Contracts`'a başvurur; modül diff'inde `SaveChanges`/işlem açma yok (işlem sayfada `RunAsync`); `OrderConfirmation` zamanı `TimeProvider`'dan; gerekçesiz `!` taraması yeşil (Unit TAM) |
| `09 §25` hata katmanı ve ret kaydı | ✓ | Adlı 422 retleri `RejectionLog.Rejected(code)` (`RefuseAsync`, `ApplyCoupon`); belirsiz sonuç `[OutcomeUncertain("UC-27")]`; geniş `catch` yok |
| `09 §25` `[GatedOperation]` | — | Uygulanmaz: ödeme adımı `OrderOperationCatalog` işlemi değil (kapı panel sipariş işlemlerinin); geçiş kapısı `ConfirmationStepRules` (KK2) |
| `09 §25` eşzamanlılık, para, veri/göç, sayfalı liste | ✓ | Yeni kilit yok: onay var olan sepet kilidini `Carts.OpenAsync` ile alır (KK10 testi); yeni para hesabı yok, tutarlar var olan sepet/sipariş hesabından; göç, yeni tablo, yeni sayfalı liste, yeni enum değeri yok (iki yeni enum saklanmaz, `ApplicationEnums.NotStored`) |
| `09 §25` güvenlik: `NoStore`, sahtecilik, hız sınırı, anahtar `SecretKey`'den | ✓ | `[NoStore]` + noindex; form `asp-antiforgery` (`Forgery` taraması yeşil); L-6/L-7/L-8; `ConfirmationKeys.NewKey()` → `SecretKey.Create()` |
| Güvenlik taraması | ✓ | Beş POST sahtecilik süzgecinden geçer; yabancı defter girdisi 404 (sahiplik); çerez yalnız belgeli alanlar, parmak izi e-posta/adres taşımaz ve sunucu özeti yeniden hesaplar; L-6/L-7/L-8 429 nötr mesaj; sayfa `[NoStore]` + noindex |

### ⚠ Kararlar (kurala en uygun seçenekle verildi)

- (a) `07 §5.2` `AcceptedCheckboxes` yerine T544 bileşeninin dört kutu adı (`AcceptPreInformation`, `AcceptDistanceSales`, `AcceptDigital`, `AcceptService`) kullanıldı → D-1333. Model bağlama adı taraması için bu dört alan ve `07 §5.2`'de kalın yazılan `SaveToAddressBook`, `IsBillingSameAsShipping`, `SummaryFingerprint`, `LeaveTarget` `[SpecAssignedName]` ile işaretlendi; kutu adları `07`'nin adı değil, bileşenin adıdır — D-1333 kapanınca işaret de kalkar ya da `07` adına döner (`PlaceOrderInput` sınıf özetinde not).
- (b) Çereze `AddressBookId` yazılıyor (teslimat adresi alanı sayıldı).
- (c) `SavedAddressName` `07`'de yok → boş verilir, defter adı alıcı adı olur (D-435 F5 gate'e).
- (d) Oturum kapandı tespiti: formda `ContactEmail` anahtarı yok ve üye yok.
- (e) Parmak izi istemciden gelir, güvenilmez ama zararsızdır: sunucu özeti yeniden hesaplar.
- (f) Değişiklik satırı metinleri `04`'te yalnız örnekle var; örneğe göre yazıldı.
- (g) Test derlemesindeki örnek sayfa da `/odeme` rotasını taşır; testlerde rota sırası (`AttributeRouteModel.Order`) ile ayrıldı.
- (h) İstemci adresi bulunamayan onay, limit dolmuş gibi nötr 429 alır; sipariş oluşmaz (D-805).
- (i) Girişten dönen müşterinin kuponu çerezden gelir ve GET okumasında yeniden değerlendirilir; bileşenin `CouponFields.AfterSignIn` yardımcısı kullanılmadı → D-1334.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Integration (T509 sınıfları) | ✓ | `--filter-class "Shopfolio.IntegrationTests.Web.CheckoutPageTests"`, `"*OrderAccessTests"`, `"*OutcomeUncertainConsistencyTests"`, `"*MembershipTests"` | 17/17; üçü birlikte 35/35; Membership 137/137; tarama düzeltmelerinden ve `main` birleştirmesinden sonra `CheckoutPageTests` yeniden 17/17 |
| Unit TAM | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` (`main` `a56a1735` birleştirmesinden sonra) | 7435/7435; `CheckoutFormsTests` dahil |
| Integration (değişen/ilgili parçalar) | ✓ | `--filter-class` parçaları (`main` `a56a1735` birleştirmesinden sonra) | Önceki turlarda ad alanı parçaları 350 + 209; birleştirmeden sonra `CheckoutPageTests` 17/17, `StorefrontFrameTests` yeşil, `OrderConfirmationTests` + `MainFlowTests` 29/29; `Ordering` ad alanının öteki 28 sınıfı 264/264 + 367/367; `OrderingTestServices` kullanan öteki sınıflar (`DataExportTests`, `PanelHomeTests`, `SalesSummaryTests`, `CardRefundJobTests`, `RequestsPageTests`) ve `main`'in yeni sayfa testleri (`OrderOperationScreenTests`, `PanelHomeAndVerificationPagesTests`, `SessionAccessTests`) 171/171 (`PanelHomeTests` düzeltmesinden sonra 25/25) |
| Düzeltme 1 — derleme | ✓ | `dotnet restore Shopfolio.slnx --locked-mode`; `dotnet build Shopfolio.slnx --no-restore -warnaserror` | restore çıkış 0; 0 uyarı, 0 hata |
| Düzeltme 1 — `dotnet format` | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` (Web, Integration, Unit) | üçü çıkış 0 |
| Düzeltme 1 — Unit TAM | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 7440/7441; kalan 1 `DomesticProductionLogoSourceScanTests` regex zaman aşımı (5 s, makine yükü; T509 diff'i dokunmuyor) — sınıf yeniden 38/38 |
| Düzeltme 1 — Integration | ✓ | `--filter-class "Shopfolio.IntegrationTests.Web.CheckoutPageTests"` (Postgres 18, `track_commit_timestamp=on`, 127.0.0.1) | 18/18 (yeni VAL-E061 testi dahil). **TAM: CI** |
| Düzeltme 2 — derleme ve biçim | ✓ | `dotnet restore Shopfolio.slnx --locked-mode`; `dotnet build Shopfolio.slnx --no-restore -warnaserror`; `dotnet format <proje> --verify-no-changes --no-restore` (Web, Unit, Integration) | restore çıkış 0; 0 uyarı, 0 hata; format üçü çıkış 0 |
| Düzeltme 2 — Unit TAM | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` parçalı: `--filter-namespace` Consistency · Architecture · `--filter-not-namespace` ikisi | 295/295 + 1750/1750 + 5433/5433 = 7478/7478 |
| Düzeltme 2 — Integration | ✓ | `--filter-class` (Postgres 18, `track_commit_timestamp=on`, 127.0.0.1): `OpenUnpaidOrderLimitTests`, `OrderPlacementLimitTests`, `OrderConfirmationTests` (+ partial) · `CheckoutPageTests`, `MainFlowTests`, `CartPageTests` | 49/49 · 43/43 (`CheckoutPageTests` 20/20, iki yeni test dahil). **TAM: CI** |
| Düzeltme 3 — derleme ve biçim | ✓ | `dotnet restore Shopfolio.slnx`; `dotnet build Shopfolio.slnx --no-restore -warnaserror`; `dotnet format <proje> --verify-no-changes --no-restore` (Web, Integration, Unit) | restore çıkış 0; 0 uyarı, 0 hata; format üçü çıkış 0 (origin/main `9b2a9db8` birleştirmesinden sonra) |
| Düzeltme 3 — Unit TAM | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 7521/7521 |
| Düzeltme 3 — Integration | ✓ | `--filter-class` (Postgres 18, `track_commit_timestamp=on`, 127.0.0.1): `CheckoutPageTests`, `CartPageTests`, `StorefrontFrameTests` | 79/79 (`CheckoutPageTests` 21/21, yeni test dahil). **TAM: CI** |
| Integration TAM | ⏳ | — | Yönetici kararı (makine yükü, "Yük altında Integration TAM"): yapımda koşulmadı; **TAM doğrulayıcıda koşulacak** |
| `dotnet format` | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (Web, Unit, Integration; son turda değişen 5 test dosyası `--include` ile) | Temiz (çıkış 0) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL (2026-11-12; dal ucu `42e1e6a7`, origin/main `4b37cf0c` birleşik) — build `-warnaserror`, format, Unit 7441/7441, betik 4/4, Integration parçalı 215/215 + 57/57, dal CI (run 43262776784) yeşil |
| Bulgu sayısı (tur 1) | 1 (1 ORTA, S1); K-notu yok |
| Düzeltme gerekli mi | Evet — B1 (ORTA, VAL-E061, D-1077, D-1080; kriter 1/UC-22): sağlayıcıya iki dakika erişilemeyen kart listeden çıkıyor; `04` 5.13.8 / `07 4.13.1` listede kalmasını, seçilemez olmasını ve yanında "şu an kullanılamıyor" yazmasını ister |
| Yapım raporu karşılaştırması (tur 1) | 1 uyuşmazlık: kriter 1 ✓ ve D-1077 "T509 payı kapandı" sayılmış; kartın iki dakikalık hâli sayfada çizilmiyor, testi yok |
| Düzeltme 1 | B1 düzeltildi: iki dakikalık kart listede kalır, radyosu `disabled`, yanında "şu an kullanılamıyor"; kart seçili onay 422, sipariş yok. Test `…_VAL_E061`. K-notu yoktu. Yeni backlog kalemi yok (D-1414–1415 kullanılmadı). Ayrıntı "Yapılan İşler" > Düzeltme 1 |
| Doğrulama durumu (tur 2) | ✗ FAIL (2026-11-12; dal ucu `dee44196`, origin/main `73e36672` birleşik, main ilerlemedi) — build `-warnaserror` 0 uyarı, format ×3 temiz, Unit 7478/7478 (`DomesticProductionLogoSourceScanTests` tek başına 38/38), betik 4/4, Integration parçalı 216/216 (CheckoutPageTests, StorefrontFrameTests, MembershipTests, PanelHomeTests) + 58/58 (OrderConfirmationTests, MainFlowTests, CartTests), TAM: CI — dal CI (run 43281914736) yeşil; `restore --locked-mode` 0 |
| Bulgu sayısı (tur 2) | 3 (2 ORTA, 1 DÜŞÜK; S1); tur 1 bulgusu B1 kapandı (`VAL-E061` PASS) |
| Düzeltme gerekli mi | Evet — kriter 1 (`07 4.13.1` görünümü `04` 5.13.2–5.13.11) ödeme özetinde ve yöntem listesinde belgeyle ayrışıyor (doğrulayıcının geçici sınaması, silindi): **B2 (ORTA, `VAL-A044`, D-627 T509 payı):** firma eşik tanımlıyken (ücret 30, eşik 500, sepet 100) özet yalnız `Kargo: 30 TL` yazar; `04` 5.13.10 / `02 §3.19.5`'in "Ücretsiz kargoya 400 TL kaldı" satırı yok (`ShippingAssessment.FreeShippingMessage` sayfaya gelmiyor). D-627'nin hedefi T508, T509'dur ve "T509 payı açık" kalmış; ne kapatılmış ne ileri atılmış. **B3 (DÜŞÜK, `04` 5.13.26):** yalnız dijital sepette özet `Kargo: Kargo ücretsiz` yazar; fiziksel kalem yokken kargo satırı görünmez. **B4 (ORTA, `04` 5.13.8, 5.13.21; `07 4.13.1`, 4.13.5; D-853):** L-8 doluyken havale radyosu seçili ve seçilebilir gelir; belge "havale listede kalır, seçilemez, nötr mesaj; kart açık" der ve "L-8 dolu + kart erişilemez" bileşimini kullanılabilir yöntemsiz hâl sayar. D-853 yapımda T509 → F5 gate'e atıldı ("kabul kriterlerinde yok"); kriter 1'in kaynağı `07 4.13.1` bu hâli açıkça sayar — kapanış ya düzeltme ya sahip onaylı kabul edilmiş risk |
| Yapım raporu karşılaştırması (tur 2) | 2 uyuşmazlık: (1) "Hedefi T509 olan backlog kalemleri" satırı D-627'yi saymıyor (kapandı da, ileri atıldı da yok); (2) kriter 1 ✓ sayılmış, özetin eşik satırı ve yöntem listesinin L-8 hâli yok. Öteki kriterler ve kanıtlar uyumlu; öz denetim tablosu var |
| Düzeltme 2 | B2, B3, B4 düzeltildi (ayrıntı "Yapılan İşler" > Düzeltme 2). D-627 (T509 payı) ve D-853 `DEFERRED_BACKLOG.md`'de yerinde ✅. Yapım raporu karşılaştırmasının iki uyuşmazlığı kapandı: (1) "Hedefi T509 olan backlog kalemleri" satırı artık D-627'yi sayar (kapandı); (2) kriter 1'in ✓'ü `07 4.13.1`'in üç hâlinin (iki dakikalık kart, L-8'de havale, özetin eşik/kargo satırı) testleriyle kanıtlanır; D-853'ün "kabul kriterlerinde yok" gerekçesi geri alındı. K-notu yoktu ("Kargo: Kargo ücretsiz" çift sözcüğü B3 ile gitti). Yeni backlog kalemi yok (D-1427–1428 kullanılmadı). ⚠ L-8 doluyken havale seçili gönderilen onay 429 sonrası sayfayı kart seçili gösterir (havale seçilemez) — belgeyle uyumlu |
| Doğrulama durumu (tur 3) | ✗ FAIL (2026-11-13; dal ucu `d1fc40f3`, origin/main `9cf8d1f7` ile çakışmasız birleştirildi) — build `-warnaserror` 0 uyarı, format ×3 temiz, Unit 7503/7503, betik 4/4, Integration parçalı 63/63 (CheckoutPageTests, OpenUnpaidOrderLimitTests, OrderPlacementLimitTests, CartPageTests) + 29/29 (OrderConfirmationTests, MainFlowTests), TAM: CI — dal CI (run 43304858862) yeşil; `restore --locked-mode` 0 |
| Bulgu sayısı (tur 3) | 2 yeni (1 KRİTİK, 1 DÜŞÜK; S1); tur 2 bulguları B2, B3, B4 kapandı (`VAL-A044` PASS, D-853; `VAL-A357` PASS — üye ekseni doğrulayıcının geçici sınamasıyla) |
| Düzeltme gerekli mi | Evet — kriter 1 (`07 4.13.1` görünümü `04` 5.13.2–5.13.11; `04` E-13 bütünüyle T509'un): **B5 (KRİTİK, `VAL-A841`, `VAL-A842`; K-756):** onay bölümünde Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi'nin tam metni kapalı `<details><summary>…</summary>` içinde (`Checkout.cshtml`, `open` yok, açan CSS/JS yok); `04` 5.13.9 (c)–(d) "sayfanın içinde kaydırılan bir kutuda, açık", 5.13.9 "hiçbirini bir bağlantının ya da pencerenin arkasına koymadan", 5.13.32 "hiçbir sınıfta metinler katlanmaz" der. Önceki iki turda gözden kaçtı. **B6 (DÜŞÜK, `04` 5.13.9 (b), (h), 2.12.7.2):** misafirin "Siparişiniz şu adrese gönderilecek: …" satırı ve "Düzelt" bağlantısı onay özetinde (b) değil, onay kutularıyla ödenecek toplam/düğme arasında çizilir; kutularla düğme arasına başka içerik girmez |
| Yapım raporu karşılaştırması (tur 3) | 1 uyuşmazlık: kriter 1 ve öz denetimin KK1 satırı ✓; onay bölümünün yasal metinlerinin açık durması (5.13.9 (c)–(d)) ne öz denetimde ne testte sınanmış. Düzeltme 2'nin iddiaları ve kanıtları doğrulayıcınınkiyle uyumlu |
| Düzeltme 3 | B5, B6 düzeltildi (ayrıntı "Yapılan İşler" > Düzeltme 3). K-756: iki yasal metin onay bölümünde açık, sabit yükseklikte kaydırılan kutuda; bağlantı, pencere ya da katlama yok. E-posta satırı onay özetinin (b) parçası, kutular ile düğme arasında değil. Yapım raporu karşılaştırmasının uyuşmazlığı kapandı: öz denetim KK1'i `04` 5.13.9'un (a)–(h) sırasıyla ayrı satırlarda denetler, test sınar. K-notu yoktu. Yeni backlog kalemi yok (D-1448–1449 kullanılmadı). Diff yalnız B5–B6 (`Checkout.cshtml`, `storefront.css`, `CheckoutPageTests` + `.Helpers`) |
| Doğrulama durumu (tur 4, tam yeniden doğrulama) | ✓ PASS (2026-11-13; dal ucu `a46fcd0f`, origin/main `9474668f` ile çakışmasız birleştirildi — `10f11d13`) — build `-warnaserror` 0 uyarı, format ×3 temiz, Unit 7538/7538, betik 4/4, Integration parçalı 64/64 (CheckoutPageTests, OpenUnpaidOrderLimitTests, OrderPlacementLimitTests, CartPageTests) + 227/227 (OrderConfirmationTests, MainFlowTests, MembershipTests, StorefrontFrameTests, PanelHomeTests), TAM: CI — dal CI (run 43489405862, `a46fcd0f`) yeşil; `restore --locked-mode` 0 |
| Bulgu sayısı (tur 4) | 0 yeni; tur 3 bulguları B5 (KRİTİK) ve B6 (DÜŞÜK) kapandı: iki yasal metin açık, `h3` başlıklı, sabit yükseklikte (`16rem`, `overflow-y: auto`, medya sorgusu yok, metin `--font-size-base`) kaydırılan kutuda, sayfada `<details>`/`<summary>` yok; onay bölümü sırası `04` 5.13.9 (a)–(h) ile birebir, e-posta satırı (b) içinde. `VAL-A841`, `VAL-A842` uç yanıtı yarısı karşılandı, tarayıcı gözlemi yarısı üretilmedi → `BEKLEMEDE` (FAIL'den). K-notu yok |
| Yapım raporu karşılaştırması (tur 4) | Tam uyumlu: düzeltme 3'ün iddiaları, öz denetim satırları ve test kanıtı doğrulayıcının bulgularıyla aynı |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T509-odeme-adimi`
- **Commit:** son baş SHA PR açıklamasında
- **PR:** #559
- **CI:** tur 1: run 43262776784 yeşil · tur 4: run 43489405862 (`a46fcd0f`) yeşil

## Dış Varsayımlar

Dış varsayım: yok. Kaynaklar (belge, gerçek PostgreSQL 18, ASP.NET Core test sunucusu) kendi depodan; ağ çağrısı yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok | — | — |
