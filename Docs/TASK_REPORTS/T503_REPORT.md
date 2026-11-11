# T503 — Vitrin: arama sonuçları, ürün sayfası ve sepete ekleme (E-03, E-04)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama bekliyor) | **Tarih:** 2026-11-11

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
| 5 | Varyant fiyatı, tipe göre teslimat, Tükendi, ayrılmış adet, T501 logosu | ✓ | `TheDeliveryLine_IsByTheProductsType_AndTheLogoIsT501s_KK5`, `AVariantWhoseStockIsAllReserved_IsSoldOut…_KK5`, `ASoldOutValue_CarriesTukendiInTheSelection…_KK5`; `ProductPageModelTests.Fields_*`, `ModeOf_*`, `PriceBlockOf_*`, `DeliveryOf_*` |
| 6 | Betik verisi stok adedsiz; Paylaş | ✓ | `ThePageData_CarriesThePublishedVariantsWithNoStockCount_AndThereIsAShareButton_KK6`; `ProductPageModelTests.ScriptDataOf_*`, `ProductBuyAreaAndScriptRecords_HaveNoStockCountMember_KK6` |
| 7 | 303: iki parametre, bildirim, sepet sayısı | ✓ | `AnAdd_Is303BackToThePageWithTheTwoPlacesAndTheNotice_AndTheCartHoldsTheLine_KK7` |
| 8 | Stok, P-9 (sınır önce), dijital tekrar, tükenmiş/seçilmemiş, kapalı satış, geçersiz adet 422; yayında olmayan ürün/`VariantId` 404 | ✓ | `EachRefusal_Is422_WithItsMessageInPlace…_KK8`, `ASalesClosedGate_RefusesTheAdd…_KK8`, `ADigitalVariantAlreadyInTheCart_IsRefused…_KK8`, `ANotPublishedProductOrAnUnknownVariant_Is404…_KK8`; `CartQuantity_*` |
| 9 | Tekrar gönderim ikinci ekleme; betikli yolda düğme kilidi | ✓ | `ASecondSend_IsASecondAdd_AndTheQuantityGrows_KK9`; kilit `wwwroot/js/product.js` (`data-js="add-to-cart"`, `submit-once`) |

## Koşular

- Derleme `-warnaserror`: temiz. Unit: 7065/7065. Integration: `StorefrontSearchAndProductTests` 18/18; `SecurityHeadersTests`, `SessionAccessTests`, `ListPatternPageTests`, `SearchEngineDataTests` ile birlikte 130/130. Integration tamamı bu turda koşulmadı (CI'a bırakıldı).
- Mimari taramalar (ModelBinding, NullForgiving, NoticeParameter, ModuleBoundary, Forgery, Cookie) Unit içinde yeşil.

## ⚠ Kararlar ve açık noktalar

- Varsayılan varyant tanımı belgede yok: ilk yayında ve tükenmemiş, yoksa ilk yayında, yoksa liste başı (`VariantSelection.Default`).
- Mesaj metinleri: "Bu ürün tükendi", "Adet 1 veya daha büyük bir tam sayı olmalı.", "Bir seçenek belirleyin", "Bu sayfa bir önizlemedir; sepete ekleme yoktur."
- `IOrderingModule.FindCartIdAsync` arayüz eki (T508 ile çakışma riski).
- Kart görseli ürün başına `GetProductGalleryAsync` (N+1); `MainImageId` gelince toplu okumaya geçilir (D-1204).
- Süre bütçesi yüzünden yapılmayanlar: `Docs/12` Durum hücreleri ve D-146, D-247, D-336, D-344, D-359, D-477, D-507, D-691..693 kayıtlarının yerinde kapatılması/ilerletilmesi; D-1166 kapatma. Doğrulayıcı/ikinci tur bunları tamamlar.
- `dotnet format` bu turda koşulmadı.
