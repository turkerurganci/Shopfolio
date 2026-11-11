# T508 — Sepet (E-12)

**Faz:** F5 | **Durum:** ⏳ Yapım bitti, Integration TAM sürüyor | **Tarih:** 2026-11-11

---

## Yapılan İşler

- Sepet sayfası `Pages/Storefront/Cart.cshtml(.cs)`: `GET /sepet` kalemleri, tutarın dökümünü, kargo satırını, ücretsiz kargo ve asgari tutar mesajlarını ve ödeme adımı geçişini modülün hesabıyla (`IOrderingModule.OpenCartPageAsync`, `ISettingsModule.GetShippingFeeAsync`, satış kapısı) çizer; sayfa kendi hesabını yapmaz.
- `UpdateCartItem` (mutlak adet; 303 → `/sepet`, 422, 404) ve `RemoveCartItem` (303, 404) uçları; 422 yeniden gösteriminde reddedilen satır gönderilen adetle ve satır mesajıyla kalır.
- Bir kez söylenen mesajlar (birleşmenin eklediği kalemler, vitrinden kalkan kalem, ödeme işareti) içerik başında; okuma onları siler.
- Çerez: çerezsiz misafir boş sepet, GET çerez açmaz; çerezli misafirde son erişim yazılır ve çerez yenilenir; sepet boşalınca ödeme adımı çerezi silinir.
- Vitrin ve panel çerçeve süzgecinin sırası 100 → -150 (422/400 yeniden gösterimi çerçeveyle çizilir; T511 aynı değeri ve yorumu kullanır).
- Dijital varyantın ekleme yolu 1 dışı adedi ve ikinci eklemeyi reddeder (D-637); sepet kapalı ekran başlığını `RecordPages.ClosedScreen` ile kurar (D-693 sepet payı).
- Sepet reddinin `07 §6.2.3` kodları tek yerde: `Pages/Components/CartRefusalCodes.cs`; stok/sınır reddi ret satırını bu kodla yazar (D-638).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Storefront/Cart.cshtml(.cs)`, `Pages/Components/{CartTexts,CartRefusalCodes}.cs`, `Pages/Frame/{StorefrontFrameBuilder,StorefrontFrameFilter}.cs`, `Pages/Shared/_StorefrontLayout.cshtml`, `Modules/Ordering/{Carts,OrderingModule}.cs`, `Modules/Ordering/Contracts/*` (sepet sözleşmeleri, `CartItemUpdateOutcome`, `CartAddOutcome.AlreadyInCart`), `Infrastructure/Security/ForgeryProtectionFilter.cs` (yorum); testler `tests/Shopfolio.IntegrationTests/Web/CartPageTests.cs`, `Ordering/CartTests.cs`, `tests/Shopfolio.UnitTests/Pages/CartPageModelTests.cs`; kayıt dosyaları.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kalemler, döküm, ücretsiz kargo mesajı, asgari tutar eksiği sunucunun hesabıyla | ✓ | `CartPageTests.TheBreakdown_TheFreeShippingMessage_AndTheButton_AreTheModulesComputation_T508_KK1`, `AMissingMinimumAmount_ClosesTheStepWithTheMissingAmount_T508_KK3` |
| 2 | Fiyat ve satın alınabilirlik güncel üründen; satır mesajı koşul sürdükçe | ✓ | `APriceChange_ShowsTheCurrentPrice_AndItsMessageStaysAtEveryRead_WhileTheFirstPriceIsKept_T508_KK2` |
| 3 | Ödeme adımı geçişi sunucunun kararı; üç kapalı sebep | ✓ | `NoLineThatEntersTheOrder_ClosesTheStepWithItsSentence_T508_KK3`, `AMissingMinimumAmount_…_T508_KK3`, `AClosedSale_ClosesTheStepInTheButtonsPlace_AndTheCartStaysVisible_T508_KK3`; `CartPageModelTests.ThePaymentStep_TellsAClosedSaleFirst_ThenTheCartsOwnBlock` |
| 4 | Bir kez söylenen mesajlar başta; kalem silinir, işaret kalkar, yenilemede yok | ✓ | `TheOnceToldMessages_AreToldOnceAtTheHead_TheMarkedLineIsDeleted_AndTheMarksAreCleared_T508_KK4` |
| 5 | Çerezsiz misafir boş, GET çerez açmaz; çerezli misafirde son erişim + çerez yenilenir | ✓ | `AGuestWithoutTheCartCookie_HasAnEmptyCart_AndTheGetOpensNoCookie_T508_KK5`, `AGuestWithTheCartCookie_HasTheLastAccessWritten_AndTheCookieRenewed_T508_KK5` |
| 6 | Sepet boşalınca ödeme adımı çerezi silinir | ✓ | `RemovingTheLastLine_DeletesTheCheckoutCookie_AndAnEmptyCartsGetDeletesItToo_T508_KK6` |
| 7 | Mutlak adet, 303; ilk eklenme fiyatı değişmez; dijitalde adet alanı yok | ✓ | `TheQuantity_IsWrittenAsAnAbsoluteValue_TheRepeatGivesTheSameResult_AndTheFirstPriceStays_T508_KK7`, `ADigitalLine_HasNoQuantityField_AndAQuantityOtherThanOneIsRefused_T508_KK7` |
| 8 | 422: adet < 1 / tam sayı değil; stok ya da sınır aşımı (yazılmaz, satır mesajı); 404 yabancı kalem | ✓ | `AQuantityUnderOneOrNotWhole_Is422_AndNothingIsWritten_T508_KK8`, `ARaiseOverTheStock_Is422WithTheLinesMessage_AndNothingIsWritten_T508_KK8`, `ARaiseOverTheProductsLimit_CountingItsOtherLines_Is422WithTheLimit_AndNothingIsWritten_T508_KK8`, `ALineOfAnotherCart_OrOneThatNeverWas_Is404_AndNothingIsWritten_T508_KK8`; sahtecilik `AWriteWithoutTheForgeryToken_Is400InTheFrame_AndNothingIsWritten_T508_KK8` |
| 9 | Vitrinden kalkmış kalemde adet yazılmaz; k4'le silinir | ✓ | `ALineTakenOffTheShowcase_KeepsItsQuantity_AndTheNextReadTellsItOnceAndDeletesIt_T508_KK9` |
| 10 | Çıkarma işaretli kalemi de çıkarır; yabancı/çıkarılmış kalem 404 | ✓ | `TheRemoval_Is303_TheSecondRemovalIs404_AndAForeignLineStays_T508_KK10`, `AMarkedLine_IsRemovedWhateverItsShowcaseState_T508_KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil kriter 1–10 ve KK1–KK10 diff'te yeniden denetlendi. `sik-fail-nedenleri.md` beş sık nedeni: (1) tarama istisnası eklenmedi; (2) yeni kaynak taraması yok, ret satırı mevcut yazıcıdan (`RejectionLog.Rejected`) geçer — `RejectionLineSourceScan` yeşil; (3) şema değişikliği yok; (4) süzgeç sırası değişince `ForgeryProtectionFilter` sınıf yorumundaki "every other page filter" cümlesi düzeltildi, `FrameLayoutTests` güncel; (5) 09 metni değişmedi (süzgeç sırası tablosu D-1191'de). Hedefi T508 olan backlog kalemleri tarandı: D-302, D-559, D-620, D-637, D-638 kapandı; D-198, D-507, D-627, D-693 T508 payı kapandı; D-650 gerekçeyle T509'a.

| KK | Sonuç | Kanıt |
|---|---|---|
| KK1 dökümün her satırı sunucu hesabında | ✓ | kriter 1 |
| KK2 fiyat değişince satır güncel, mesaj kalıcı | ✓ | kriter 2 |
| KK3 üç kapalı sebep ayrı testte | ✓ | kriter 3 (üç ayrı test) |
| KK4 mesaj bir kez; kalem silinir, işaret kalkar | ✓ | kriter 4 |
| KK5 çerezsiz GET çerez açmaz | ✓ | kriter 5 |
| KK6 son kalemden sonra çerez silinir | ✓ | kriter 6 |
| KK7 mutlak adet, tekrar aynı sonuç | ✓ | kriter 7 |
| KK8 iki 422 sebebi ve 404 | ✓ | kriter 8; ret kodları `CartPageModelTests.TheCartsRefusals_AreWrittenWith07sCodes_AndAResultThatIsNoRefusalHasNone_D638` |
| KK9 kalkmış kalemde adet sabit | ✓ | kriter 9 |
| KK10 çıkarma ve ikinci 404 | ✓ | kriter 10 |
| `09 §25` satırları; güvenlik taraması | ✓ | `Cart.cshtml`'de ham HTML yok (`Html.Raw` 0); yazan uçlar sahtecilik denetimli (400 testi); sahiplik denetimi modülde (yabancı kalem 404); yanıt `[NoStore]`; yeni bağımlılık, sır, şema yok |

`12`: bu task için VAL satırı yok (grep T508 boş).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build --max-threads 3` | 7040/7040 |
| Integration | ⏳ | parçalı, `SHOPFOLIO_TEST_DATABASE` Postgres 18 (`track_commit_timestamp=on`) | Web parça 1 (11 sınıf) 148/148; Web parça 2 (16 sınıf) + `Ordering.CartTests` 352/352. Kalan: Web parça 3 (RequestLimits…VersionedFormPage, 14 sınıf) ve öteki ad alanları |
| Biçim | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, Unit temiz (değişen dosyalar); Integration devir 4'te temiz |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok
- **Ortak altyapı:** vitrin/panel çerçeve süzgeci sırası -150 (T511 ile ortak)

## Commit & PR

- **Branch:** `task/T508-sepet`
- **PR:** açılmadı (Integration TAM bitmedi)
- **CI:** yapım işçisi izlemez; yardımcı yürütür

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (plan tier, kota, API, paket sürümü, bölge kısıtı geçmiyor) | Kabul kriterleri tarandı | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Satırda görsel, seçenek değerleri ve ürün sayfası bağlantısı yok (`04` 5.12.4) | D-1189 | Catalog'un sepet okuması bu alanları taşımıyor; ödeme özeti aynı satırı kullanır (T509) |
| 2 | 400 sahtecilik yeniden gösteriminde sepet içeriği yok | D-1190 | Hiçbir şey yazılmaz; aynı kalıp ödeme formunda (T509) |
| 3 | Çerçeve süzgeci sırası (-150) 09'da yazılı değil | D-1191 | Sonraki 09 geçişi |
| 4 | Kargo 0 TL iken eşik mesajı | D-650 | `02 §3.19.5` kural değişikliği, karar kaydı ister; ödeme özetiyle birlikte T509 |

## Notlar

- **⚠ Kararlar:** (1) `04` 2.12.1.8 "adedi bir olan kalemde adet alanı görünmez" cümlesi işlem adedi varyantı içindir (iptal/cayma/ayıp seçimi, 5.17.4, 5.19.3); sepette 5.12.4 "adet ve adedi değiştiren alan" der ve UC-20 adedi artırmayı ister — alan yalnız dijital kalemde gizli. (2) D-638: sonuç adları değiştirilmedi, eşleme `CartRefusalCodes`'ta; ekleme ucu eşlemesi de yazıldı (T503 okur). (3) D-650 T509'a ileri atıldı. (4) D-1189 KK'da değil, backlog'da kaldı.
- **Repo memory:** `MEMORY.md`'ye dokunulmadı.
