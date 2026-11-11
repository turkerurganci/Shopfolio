# T502 — Vitrin: ana sayfa ve kategori sayfası (E-01, E-02)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-11

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Ana sayfa `GET /` (`Pages/Index.cshtml`, `IndexModel`): dört blok firmanın düzeninde (kurumsal blok, Hizmetlerimiz, Referanslarımız, ürün vitrini); modelin tek kaynağı `HomePage.Of(HomepageView)`. Boş siteye kurumsal blok marka adı ve logosuyla düşer. Başlıkta Organization yapılandırılmış verisi ilk yanıttadır (`StorefrontHeads.Home`).
- Kategori sayfası `GET /kategori/{slug}` (`Pages/Category.cshtml`, `CategoryModel : IListPage`): kırıntı, ad, süzgeç formu (fiyat aralığı, stokta), kart ızgarası, sayfalama, boş hâl. Bilinmeyen adres 404; ürünsüz kategori 200 boş sayfa; ters aralık 200 + alan mesajı; `page` aşımı son sayfa; `page` dışı parametre noindex (liste deseni + `StorefrontHeads.Category`).
- Modül yüzü: `ProductListItem` ana görsel kimliği ve alt metni (D-332), `ICatalogModule.ListNewestProductsAsync`, `HomepageShowcase.Products` artık `ProductListItem` listesi (D-1178 (2)); `ProductCards.Of(item)`.
- Stil: `storefront.css` ana sayfa ve kategori bölümü; orta sınıf eksik satır kuralı yalnız `.home-showcase` için (yalnız `min-width` koşulları: 768'de gizler, 1024'te geri gösterir).
- Test örnek sayfasının rotası `/sss/kategori-listesi/{slug}` oldu (gerçek `/kategori/{slug}` ile çakışmasın).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/{Index,Category}.cshtml(.cs)`, `Pages/Shared/_HomeCardBlock.cshtml`, `Pages/Storefront/{HomePage,CategoryPage,StorefrontHeads}.cs`, `Pages/Lists/ProductCard.cs`, `Modules/Catalog/*` (ProductListReader, CatalogModule, ICatalogModule, ProductListItem), `Modules/CorporateContent/{HomepageReader,Contracts/HomepageView}.cs`, `wwwroot/css/storefront.css`, testler: `tests/Shopfolio.UnitTests/Pages/StorefrontHomeAndCategoryModelTests.cs`, `tests/Shopfolio.IntegrationTests/Web/StorefrontHomeAndCategoryTests.cs`, güncellenen `ListPatternPageTests`, `SearchEngineDataTests`, `SitemapSourceTests`, `CategoryListSample.cshtml`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dört blok, firmanın düzeni | ✓ | `StorefrontHomeAndCategoryTests.TheHomePage_ShowsTheFourBlocks_…_KK1`; Unit `TheHomePage_KeepsTheModulesBlockOrder_…_KK1` |
| 2 | İlk üç işaretli kayıt; sekiz/dört ürün; orta sınıf kuralı stilde, tarayıcı sınaması 12'de | ✓ | `TheServiceAndReferenceBlocks_ShowTheFirstThreeFlaggedRecords_…_KK2`, `TheShowcase_IsTheNewestPublishedProducts_Eight…_KK2`; Unit `TheStyleSheet_HidesTheIncompleteShowcaseRow_…_KK2`; tarayıcı: D-1199 (VAL-A408–A410) |
| 3 | Yayında ürünü olan birinci seviye kategori bağlantıları | ✓ | `TheCategoryLinks_AreTheFirstLevelCategories…_KK3` (alt dal ürünü üst kategoriyi gösterir; ürünsüz ve yalnız taslaklı yok) |
| 4 | Satış kapalıyken vitrin aynı; yapılandırılmış veri ilk yanıtta | ✓ | `WhileSalesAreClosed_TheShowcaseIsTheSame_…_KK4` |
| 5 | Alt dallı tek liste, en yeni önce | ✓ | `TheCategoryPage_ListsTheProductsOfTheCategoryAndOfItsBranches_NewestFirst_KK5` |
| 6 | page/minPrice/maxPrice/inStock; aşım son sayfa; noindex | ✓ | `APageBeyondTheLast_ShowsTheLastPage_…_KK6`; Unit `TheQuery_ReadsThePriceBounds…_KK6` |
| 7 | Ters aralık: süzgeç yok, alan mesajı, 200 | ✓ | `AnInvertedPriceRange_IsNotApplied_…_KK7` ("En düşük fiyat üst değerden büyük olamaz.") |
| 8 | Boş kategori 200; boş süzgeçte kaldırma yolu | ✓ | `ACategoryWithoutAPublishedProduct_IsAnEmptyPage_…_KK8`; Unit `TheEmptyState_…_KK8` |
| 9 | Yabancı slug 404 | ✓ | `ASlugThatIsNoCategorys_Is404_KK9` (iki örnek) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1–KK9 (yukarıdaki tablo; önceki işçinin kodu dahil hepsi diff'te denetlendi) | ✓ | yukarıdaki testler |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | Yeni bekçi/tarama istisnası eklenmedi; mevcut mimari taramalar (FrameSourceScan, ModelBindingRule, PageSourceScan, SearchEngineSourceScan, SalesGateSourceScan, ListSourceScan, ModuleBoundary) tam Unit koşusunda yeşil |
| Sık FAIL 2: tarama çağrı biçimi | ✓ | Yeni kaynak taraması yok |
| Sık FAIL 3: "Varsayılan" DB default / FK dizini | ✓ | Şema değişikliği yok (`ProductListItem` yalnız okuma modeli) |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | Test örnek sayfası rotası değişince ona bağlı iki test adresi (`ListPatternPageTests`, `SearchEngineDataTests`) güncellendi; `HomepageShowcase.Products` tipi değişince `SitemapSourceTests.FakeCatalog` ve `StorefrontReadersTests` derleniyor ve koşuyor |
| Sık FAIL 5: belge kuralını değiştiren 09 metni | ✓ | 09/05/07 metni değişmedi; tek karar D-330 (aşağıda ⚠) |
| Ana sayfa/kategori sayfası modül sınırı (09 §6, §25) | ✓ | Sayfalar yalnız `ICatalogModule` ve `ICorporateContentModule` sözleşmelerini kullanır; `ModuleBoundary` taraması yeşil |
| Sayfalı listeye sınır testi (09 §19.2 m.5) | ✓ | `page=99` son sayfa, 30 üründe 24+6 (`…KK6`) |
| Stil: yalnız min-width koşulu, satır içi style/olay yok | ✓ | `FrameSourceScanTests.TheBaseFile_DefinesTheTwoLimitsOnce_…` (ilk denemede orta sınıf `max-width` ile kırdı, 768/1024 çiftiyle düzeltildi) |
| Model bağlama kuralı (sorgu modelinde her ortak özellik adlı) | ✓ | `ModelBindingRuleTests` (`HasAppliedFilter` özelliği yöntem `IsFilterApplied()` yapıldı) |
| Mini güvenlik taraması | ✓ | Yeni sır/oturum/yetki yok; giriş değerleri (`minPrice`, `maxPrice`, `inStock`, `page`) metin okunur, hatalı değer süzgeç olmaz; çıktı Razor ile kaçışlı, `@Html.Raw` yok; yeni bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --filter-not-trait "Consistency=true"` | tam koşu 6735 test, ilk koşuda 2 kırık (stil koşulu, model bağlama); düzeltmeden sonra ilgili sınıflar 141/141 |
| Integration | ✓ (hedefli) | `--filter-class StorefrontHomeAndCategoryTests, ListPatternPageTests, SearchEngineDataTests, StorefrontReadersTests, StorefrontFrameTests` | 137/137 (yeni sınıf 12) |
| Contract | n/a | — | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-11) |
| Bulgu sayısı | 2 — **B1** (S2, ORTA; kriter 1 / KK1, `07 §2.1.2`): yeni `/` sayfası `PageFormPatternTests.EveryMappedEndpoint_StartsWithAFirstSegmentOfTheClosedList`'i kırıyor — `AddressRules.IsClosedFirstSegment("")` yanlış döner, kapalı listede ana sayfa yok; yerelde tek başına koşuda da kırmızı, dal CI'ı Integration adımında 3675'te 1 kırık (run 43006706756). **B2** (S1, DÜŞÜK; kriter 1 / KK1, `04` 5.1.4, 5.1.13; komşu satır VAL-A112): `home-intro--band` için stil yok, `storefront.css` yorumu "band layout stacks"; dar hâl her genişlikte alt alta, `04` 5.1.4 orta ve geniş sınıfta "kısa tanıtım ile ana görsel yan yana, tek bant" ister (alt alta yalnız dar sınıfta) |
| Düzeltme gerekli mi | Evet — B1: ana sayfanın kökü kapalı listede sayılır (`AddressRules` ya da sınamanın kökü `07 §2.1.2` uyarınca kabul etmesi); B2: `.home-intro--band` orta sınıftan itibaren iki sütun (görsel ve metin yan yana), dar sınıfta alt alta; yorum düzeltilir |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-1218 (D-477 ✅ erken, D-691 kalan parça, D-353 ele alınmadı); K2 → D-1219 (D-1202 gerekçesi yanlış: sıra okuyucuda zaten alfabetik) |
| Task satırları (`12 §7.5`) | VAL-A002 → PASS (KK5 Integration sınaması, uç yanıtı); VAL-A001, VAL-A003 ve VAL-A408–A423 tarayıcı/menü kanıtı ister → BEKLEMEDE |
| Yerel sınama | restore ✓, build `-warnaserror` 0 uyarı ✓, format üç proje ✓, Unit 7030/7030 ✓, betik testleri 4/4 ✓, Integration `StorefrontHomeAndCategoryTests` 12/12 ✓, `PageFormPatternTests` 26/27 (B1); `Web` ad alanı makine yükü altında DB zaman aşımlarıyla kırıldı (ilgisiz), tam Integration kanıtı dal CI'ı |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T502-vitrin-ana-kategori`
- **Commit:** PR açılışında yardımcıya döndürülür
- **PR:** yardımcıya döndürülür
- **CI:** yardımcı yürütür
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok (plan katmanı, kota, anahtar, dış servis ya da sürüm bağımlılığı yok; yalnız yerel PostgreSQL 18 ve .NET paketleri) | Task metni tetikleyici terimleri taşımıyor | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Orta sınıf eksik satır kuralının tarayıcı sınaması | D-1199 | Plan kriter 2: tarayıcı sınaması `12`'dedir (VAL-A408–A410) |
| 2 | Arama sonucunda birden çok kategori bağlantısı (D-1178 (1)) | D-1200 | T503'ün işi |
| 3 | Hakkımızda bağlantı metni sabit "Hakkımızda"; menü adı değil | D-1201 | Ürün kararı: menü adı ayrı açılabilir |
| 4 | Vitrin kategori bağlantılarının alfabetik sırası modülden gelsin | D-1202 | Modül sözleşmesi değişikliği (T361 okuyucusu) |
| 5 | Dar/orta sınıfta süzgeç `<details>` kapalı gelir; geniş sınıfta `::details-content` ile görünür (eski tarayıcıda kapalı kalabilir) | D-1203 | Tarayıcı sınamasıyla `12`'de doğrulanır |

## Notlar

- **Working tree (Adım -1):** temiz (iki işçi; devam işçisi `git status --short` boş buldu, hook yolu `scripts/git-hooks`).
- **Ana dal CI startup (Adım 0):** 42994459437 success, 42994459582 success, 42991019324 success (devam işçisi, 2026-11-11).
- **⚠ Kolay karar (D-330):** fiyat süzgeci liste fiyatında kalır; kart indirimli fiyatı ve referansı beyan eder.
- **⚠ Tam Integration koşulmadı:** süre bütçesi nedeniyle yalnız etkilenen sınıflar koşuldu; tam koşu CI'dadır. Katalog ve CorporateContent modül yüzü değişti, derleme tüm test projelerinde temiz.
- **Repo memory (bitiş kapısı 9):** dokunulmadı (parça kuralı).
