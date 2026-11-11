# T503 — Vitrin: arama sonuçları, ürün sayfası ve sepete ekleme (E-03, E-04)

**Faz:** F5 | **Durum:** ⏳ Düzeltme 1 yapıldı, tur 2 doğrulaması bekliyor | **Tarih:** 2026-11-11

---

## Yapılan İşler

- Arama sayfası `GET /arama` (`Pages/Search.cshtml(.cs)`, `Pages/Storefront/SearchPage.cs`, `Pages/Shared/_ProductFilter.cshtml`): `ProductQuery`, süzgeç formu, boş hâl satırı, noindex.
- Ürün sayfası `GET /urun/{slug}` ve `POST ?handler=AddCartItem` (`Pages/Product.cshtml(.cs)`, `Pages/Storefront/ProductView.cs`, `VariantSelection.cs`, `ProductHeads.cs`, `wwwroot/js/product.js`): varyant seçimi, fiyat bloğu, teslimat satırı, Tükendi, önizleme, arşiv sayfası, sepete ekleme.
- Katalog sözleşmesi: `ProductPage` varyant/seçenek alanları, `ProductPageReader`; `ICountryList`; `IOrderingModule.FindCartIdAsync`; `DomesticProductionLogo.Address` statik alan önekli (D-1166).
- Stil: `storefront.css` (ürün sayfası blokları; T502'deki `breadcrumb`, `category-layout`, `filters` blokları aynen kopyalandı — main'e T502 girince çakışma çözümü gerekir).
- Test örnek sayfaları `/urun/{slug}` ve `/arama` rotalarından `/sss/urun-ornegi/{slug}` ve `/sss/arama-listesi` rotalarına alındı (gerçek sayfalarla çakışıyordu; T502'nin `/sss/kategori-listesi` çözümüyle aynı).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Arama: büyük-küçük harf/Türkçe, `%` `_`, 150 kırpma | ✓ | `StorefrontSearchAndProductTests.TheSearchPage_IsBlindToCase…_KK1`, `ASearchWord_LongerThan150Characters_IsCutAt150_KK1`; katalog düzeyi `CategoryTreeAndListsTests.TheSearch_*` |
| 2 | Boş arama yalnız alan; sonuçsuz boş hâl; arşiv yok; süzgeç; noindex | ✓ | `AnEmptyWord_ShowsOnlyTheSearchField…_KK2`, `ASearchWithNoProduct_IsTheEmptyStateLine_AndAnArchivedProduct…_KK2`, `TheFilters_TakeThePriceRange…_KK2`, `ProductPageModelTests.EmptyStateOf_*` |
| 3 | Yayında E-04, taslakta panel önizlemesi, taslak/yok aynı 404 | ✓ | `APublishedProduct_IsE04_ADraftIsAPreview…_KK3`, `AnArchivedProduct_ShowsTheNoLongerSoldPage…_KK3` |
| 4 | Seçim iki parametre; bozuk/tükenmiş → varsayılan | ✓ | `TheSelection_IsTheTwoOrders_AndAWrongOrMissing…_KK4`; `ProductPageModelTests.ParseOrder_*`, `Resolve_*`, `Default_*` |
| 5 | Varyant fiyatı, tipe göre teslimat, Tükendi, ayrılmış adet, T501 logosu (betikli yol dahil) | ✓ | `TheDeliveryLine_IsByTheProductsType_AndTheLogoIsT501s_KK5`, `AVariantWhoseStockIsAllReserved_IsSoldOut…_KK5`, `ASoldOutValue_CarriesTukendiInTheSelection…_KK5`; düzeltme 1: `TheScriptData_HoldsEachCombinationsPlacesAndSoldOutMark_ForTheOtherOptionsChoices_KK5`, `TheScript_DrawsTheOtherOptionsChoicesAgainOnEachChange_WithTheServersRule_KK5`, `TheVariableLines_AreAlwaysDrawn_AndHiddenWhenEmpty_SoTheScriptCanFillThem_KK5`; `ProductPageModelTests.Fields_*`, `ModeOf_*`, `PriceBlockOf_*`, `DeliveryOf_*` |
| 6 | Betik verisi stok adedsiz; Paylaş | ✓ | `ThePageData_CarriesThePublishedVariantsWithNoStockCount_AndThereIsAShareButton_KK6`; `ProductPageModelTests.ScriptDataOf_*`, `ProductBuyAreaAndScriptRecords_HaveNoStockCountMember_KK6` |
| 7 | 303: iki parametre, bildirim, sepet sayısı | ✓ | `AnAdd_Is303BackToThePageWithTheTwoPlacesAndTheNotice_AndTheCartHoldsTheLine_KK7` (düzeltme 1: çerçevenin sepet sayısı eklemeden önce 0, sonra 1 kalem okunur) |
| 8 | Stok, P-9 (sınır önce), dijital tekrar, tükenmiş/seçilmemiş, kapalı satış, geçersiz adet 422; yayında olmayan ürün/`VariantId` 404 | ✓ | `EachRefusal_Is422_WithItsMessageInPlace…_KK8`, `ASalesClosedGate_RefusesTheAdd…_KK8`, `ADigitalVariantAlreadyInTheCart_IsRefused…_KK8`, `ANotPublishedProductOrAnUnknownVariant_Is404…_KK8`; `CartQuantity_*` |
| 9 | Tekrar gönderim ikinci ekleme; betikli yolda düğme kilidi | ✓ | `ASecondSend_IsASecondAdd_AndTheQuantityGrows_KK9`; kilit `wwwroot/js/product.js` (`cartForm.addEventListener("submit"`, `button.disabled = true`, `event.preventDefault()`; `TheScript_DrawsTheOtherOptionsChoicesAgainOnEachChange_…_KK5` kaynak sınaması) |

## Koşular

- `origin/main` birleştirildi (T504 sonrası; tek çakışma `IMPLEMENTATION_STATUS.md` satırları). Derleme Release `-warnaserror`: temiz.
- Unit: 6785 (Consistency hariç) + Consistency 295 = hepsi geçti. `dotnet format --verify-no-changes`: Web, UnitTests, IntegrationTests temiz.
- Integration `Shopfolio.IntegrationTests.Web`: 720/720 geçti. Bu turda bulunan hata düzeltildi: T504'ün 404 çerçevesi nonce taşıyor; `StorefrontSearchAndProductTests` KK3 gizli/bilinmeyen 404 gövde karşılaştırması nonce ve returnUrl normalleştirilerek yapılıyor.
- Integration `Catalog` ad alanı: 82/82 geçti. Web dışındaki öteki ad alanları (Administration, Commands, Concurrency, CorporateContent, Database, Disposal, FileSecrets, Files, Jobs, Logging, Model, Ordering, Outages, Pages, Payment, Security, SignedEndpoints) süre bütçesi yüzünden bu turda koşulmadı (Catalog tek başına 5m47); T503 bunlara dokunmaz (değişen kaynak Pages, Catalog okuyucusu, Settings/Ordering ek yöntemleri) — CI tam koşar.
- Mimari taramalar (ModelBinding, NullForgiving, NoticeParameter, ModuleBoundary, Forgery, Cookie) Unit içinde yeşil.

## ⚠ Kararlar ve açık noktalar

- Varsayılan varyant tanımı belgede yok: ilk yayında ve tükenmemiş, yoksa ilk yayında, yoksa liste başı (`VariantSelection.Default`).
- Mesaj metinleri: "Bu ürün tükendi", "Adet 1 veya daha büyük bir tam sayı olmalı.", "Bir seçenek belirleyin", "Bu sayfa bir önizlemedir; sepete ekleme yoktur."
- `IOrderingModule.FindCartIdAsync` arayüz eki (T508 ile çakışma riski).
- Kart görseli ürün başına `GetProductGalleryAsync` (N+1); `MainImageId` gelince toplu okumaya geçilir (D-1204).
- Backlog (yerinde): D-247, D-336, D-1166 ✅ kapatıldı; D-344 (T525 form payı), D-359 (T502), D-477 (T502), D-507 (T508), D-691 (T502, T505), D-692 (T505), D-693 (sepet, ödeme, takip, hesap, panel) T503 payı yapılıp ileri atıldı; D-146 (`notice` tarama boşluğu) kapanmadı, hedef en geç F5 gate. Yeni kalemler D-1204…D-1208 `DEFERRED_BACKLOG.d/T503.md`'de.
- `Docs/12` Durum hücreleri: hiçbir satırın kaynak hücresi `11 T503` atfı taşımıyor; tarayıcı satırları BEKLEMEDE kalır (Implementation doğrulaması), değişiklik yok.
- Integration Web dışı ad alanlarının tam koşusu (yukarıda) bu turda yapılmadı; CI'da doğrulanır.

## Doğrulama

| Alan | Değer |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-11, kod head'i `34cd89da` + `origin/main` `157e7ec3` birleştirmesi; çakışmasız). |
| Bulgu sayısı | 2 (ORTA, S1) — ikisi de betikli yolda, kriter 5 / `04` 5.4.4, 5.4.6, 5.4.9. |
| Düzeltme gerekli mi | Evet — bu PR'da (yeni yapım turu, sonra tur 2 doğrulaması). |
| K-notları | 5 — K1, K3 → D-1243; K2 → D-1244; K4, K5 düzeltme turunda bu PR'da kapatılır. |

**Bulgu 1 (ORTA, S1; VAL-A009, VAL-A011; kriter 5):** `wwwroot/js/product.js` bir seçenek değişince yalnız fiyatı, görseli ve gizli `VariantId` alanını günceller; öteki boyutun `<option disabled>` ve "(Tükendi)" işaretleri sunucunun ilk gösterdiği varyanta göre kalır (`VariantSelection.Fields`, "the other option keeps its place"). Örnek: Kırmızı-S, Mavi-S, Mavi-M açık, Kırmızı-M açılmamış; sayfa Kırmızı-S ile açılır, M devre dışıdır; ziyaretçi rengi Maviye çevirir → betik Mavi-S'i gösterir, sayfa yenilenmez, M hâlâ devre dışıdır — açılmış ve satın alınabilir Mavi-M betikli yolda seçilemez ("Seçimi uygula" betikle gizli). Tersine, açılmamış kombinasyon etkin kalıp seçilebilir görünür; tükenmiş değerde "Tükendi" işareti görünmez. `04` 5.4.6 (açılmamış ve tükenmiş seçilemez, ötekiler seçilir), 5.4.9 (seçimle satın alınabilirlik güncellenir). Öneri: değişimde iki boyutun seçilebilirlik ve "Tükendi" durumunu sayfa verisinden yeniden hesapla (veride olmayan birleşim = açılmamış) ya da değişimde GET formunu her zaman gönder.

**Bulgu 2 (ORTA, S1; kriter 5; `04` 5.4.4, komşu VAL-A017):** `Product.cshtml` birim fiyat satırını (`data-js="unit-price"`) yalnız ilk gösterilen varyantın `UnitPriceText`'i doluysa çizer; `product.js` `setText` öğe yoksa hiçbir şey yapmaz. Varyantların net miktarı farklıysa (ör. 1 kg'lık varsayılan varyantta birim fiyat satış fiyatına eşit, satır yok; 500 g'lık varyantta birim fiyat farklı) betikle seçilen varyantın birim fiyatı hiç görünmez. `04` 5.4.4: "seçim fiyatı, birim fiyatı ve indirim satırını günceller". (İndirim satırı ürün düzeyinde olduğu için — `Discount` her varyanta uygulanır — aynı sorunu doğurmaz; ama aynı kalıpla yazılmıştır.) Öneri: fiyat bloğunun değişebilen satırlarını her zaman `hidden` ile çiz.

**Kabul kriterleri (validator kanıtı):** 1 ✓ (`SearchTerm.ToLikePattern`, `TheSearchPage_IsBlindToCase…_KK1`, `ASearchWord_LongerThan150…_KK1`) · 2 ✓ (`AnEmptyWord_…_KK2`, `ASearchWithNoProduct_…_KK2`, `TheFilters_…_KK2`; `SearchModel` `ClosedScreen`) · 3 ✓ (`APublishedProduct_IsE04_…_KK3`; `OnGetAsync` NotFound) · 4 ✓ (`VariantSelection.Resolve`, `TheSelection_IsTheTwoOrders_…_KK4`) · 5 ✗ sunucu yolu ✓ (`TheDeliveryLine_…_KK5`, `AVariantWhoseStockIsAllReserved_…_KK5`, `ASoldOutValue_…_KK5`), betikli yol bulgu 1–2 · 6 ✓ (`ThePageData_…_KK6`; `VariantScriptData` stok alanı yok) · 7 ✓ (`AnAdd_Is303…_KK7`; sepet sayısı çerçeveden, testte sayı sınanmıyor → K4) · 8 ✓ (`EachRefusal_Is422…_KK8`, `ASalesClosedGate_…_KK8`, `ADigitalVariantAlreadyInTheCart_…_KK8`, `ANotPublishedProductOrAnUnknownVariant_Is404…_KK8`) · 9 ✓ (`ASecondSend_…_KK9`; `product.js` submit kilidi).

**Koşular:** build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` Web, UnitTests, IntegrationTests temiz; Unit 7162/7162; betik testleri 4/4; Integration tam 3713/3713 (Web 738 + öteki ad alanları 2975; PostgreSQL 18, `track_commit_timestamp=on`); `restore --locked-mode` 0. Ana dal son 3 run success (43026292839, 43026292752, 43011159643). Dal CI: head `34cd89da` run 43030869524 success (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate).

**`12` satırları:** VAL-A004, VAL-A005 (KK1), VAL-A019 (KK6), VAL-A022 (KK3) → PASS; VAL-A009, VAL-A011 → FAIL (bulgu 1). Öteki KP-2…KP-7 satırları BEKLEMEDE kalır: kategori sayfasıyla ortak (A006–A008, A010 → T502), tarayıcı gözlemi (A012, A013, A016, A018, A020), E-05/T549 (A021), kanıtı bu turda üretilmeyen uç satırları (A014, A015, A017, A023, A024, B001, D001). `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** K1 tükenmiş ret metni "Bu ürün tükendi" belgede yok (`07` 4.4.2 "stok mesajı") → D-1243 · K2 yalnız taslak/arşiv varyantın değeri ziyaretçide devre dışı değer olarak görünür (VAL-A024 okuması) → D-1244 · K3 betik verisinde fiyat tarihi yok, betikli seçimde varsayılan varyantın tarihi kalır (`07` 4.4.1 listesi) → D-1243 · K4 KK7 testi çerçevenin yeni sepet sayısını sınamıyor → bu PR'da (düzeltme turu) · K5 yapım raporu kriter 9 kanıtında `product.js`'te olmayan `submit-once` adını anıyor → bu PR'da.

**Yapım raporu karşılaştırması:** 2 uyuşmazlık — rapor kriter 5'i ✓ veriyor (betikli yolun seçilebilirlik ve birim fiyat güncellemesi denenmemiş); kriter 9 kanıtı var olmayan `submit-once` adını anıyor. Rapor "Web dışı ad alanları koşulmadı" diyor; bu turda tam koşuldu, hepsi geçti. Öz denetim tablosu var, her satırda kanıt adı var.

## Düzeltme 1 (2026-11-11)

**Bulgu 1 (kriter 5; VAL-A009, VAL-A011):** `product.js` her seçim değişiminde (`show`) `refreshChoices` ile iki boyutun seçilebilirliğini ve "(Tükendi)" işaretini sunucu kuralıyla (`VariantSelection.Fields`: değer ya seçili ya da diğer boyut yerinde kalırken var olan ve tükenmemiş kombinasyon) sayfa verisinden yeniden hesaplar. Sayfa verisine yalnız bayrak `soldOut` eklendi (`VariantScriptData.SoldOut`; yayında ve ayrılmış adet düşülünce tükenmiş; stok adedi yok), seçenekler `data-label` taşır. Rapordaki örnek (Kırmızı-S, Mavi-S, Mavi-M açık; Kırmızı-M yok) `TheScriptData_HoldsEachCombinationsPlacesAndSoldOutMark_…_KK5` ile sayfa verisi ve ilk açılış durumu olarak, betik kuralı `TheScript_DrawsTheOtherOptionsChoicesAgainOnEachChange_…_KK5` ile kaynak olarak kilitlendi (betiği çalıştıran test yok → D-1247).

**Bulgu 2 (kriter 5; VAL-A017):** `Product.cshtml` birim fiyat, indirim satırı (`reference`, `mark`) ve tarih (`dates`) öğelerini her zaman çizer, boşken `hidden`; betik seçilen varyantın değerini yazar ya da gizler. Sınama: `TheVariableLines_AreAlwaysDrawn_AndHiddenWhenEmpty_SoTheScriptCanFillThem_KK5`.

**K-notları:** K4 kapandı (KK7 testi çerçevenin sepet sayısını 0 → 1 kalem olarak sınar); K5 kapandı (kriter 9 kanıtı `product.js`teki gerçek adlara bağlandı, `submit-once` anılmıyor); K1, K3 → D-1243 ve K2 → D-1244 (bu PR dışı, belgeye hizalama; değişmedi).

**Öz denetim:** T503 kriter 1–9 ve KK1–KK9 yeniden denetlendi; tablodaki her satırın kanıt testi mevcut ve yeşil (kriter 5 betikli yol ✓, 7 ✓, 9 ✓). `sik-fail-nedenleri.md` iki T503 satırı (betikli yol yeniden hesabı, değişken alanın yer tutucusu) kapandı; aynı kalıp ürün sayfasının öteki değişken satırlarında (`reference`, `mark`, `dates`) da düzeltildi.
