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
