# T333 — Kategori ağacı, site içi arama ve liste süzgeçleri

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kategori düzeyi (`09 §9.4` madde 4):** `CategoryLevels` saf hesap (düzey = üst + 1, dal kayması, derinlik sınırı `ProductConstants`'tan, kendi dalına taşıma reddi, alt dal listesi). Düzeyi yazan tek yer `CategoryTree`; `Category.Level` özel ayarlayıcılı, `Category.Create` (düzeyle) ve `Category.Relocate` yalnız ağaçtan çağrılır (kaynak taraması).
- **Ağaç yazımı:** `CategoryTree` — `CreateAsync`, `MoveAsync`, `DeleteAsync` ağaç danışma kilidini (`AdvisoryLockScope.CategoryTree`) ilk deyim olarak alır, `RenameAsync` kilitsizdir (düzeyi etkilemez). Modül `SaveChanges` çağırmaz, çağıranın işlemine katılır; ret dönüş değeridir (`CategoryWriteResult`). Dolu kategori (ürünü ya da alt kategorisi olan) silinmez; ret sayıları taşır; veritabanı son savunmadır. Yazım `Menu` + `CategoryTree` (oluşturma ve silmede ayrıca `Sitemap`; ad değişimi adresi değiştirmediği için `Sitemap` bildirmez) önbellek alanlarını `CacheInvalidations`'a bildirir.
- **Ürün–kategori bağı:** `ProductCategoryLinks.SetAsync` — bir ürün için kategori kümesi + tek ana kategori (boş küme = ana yok; dolu küme = ana zorunlu ve kümede). Değişiklik izleyiciden geçer (toplu deyim `ProductVersionGuard`'ı atlar); işareti değişen bağ silinip yeniden yazılır (izleyici iki güncellemeyi dolu-süzgeçli tekil dizine göre sıralamaz). Değişimde ürün sürümü artar.
- **Listeler:** `ProductListReader` — `ListCategoryAsync(adres)` (kategori + bütün alt dal, ürün başına tek satır) ve `SearchAsync` (`SearchTerm.ToLikePattern` + `NameSearchForm`). Yalnız yayındaki ürün; sıralama `CreatedAt` azalan, `Id` azalan; sayfa `PageSizes.StorefrontProducts`; sayfa yalnız kartın sütunlarıyla (Id, Ad, Adres, CreatedAt; açıklama okunmaz) `init` üyeli özel sınıfa projeksiyonla okunur ve o üyelerde sıralanır (düzeltme 2), kartın en düşük fiyatı ve satın alınabilirliği sayfanın anahtarlarıyla ikinci sorguda okunur. Süzgeç: fiyat aralığı + stokta; ters aralık uygulanmaz; iki süzgeç birlikteyse aynı varyant hem aralıkta hem satın alınabilir olmalıdır.
- **Modül yüzü:** `ICatalogModule` + `CatalogModule`: `GetCategoryTreeAsync`, `Create/Rename/Move/DeleteCategoryAsync`, `SetProductCategoriesAsync`, `ListCategoryProductsAsync`, `SearchProductsAsync`. Türler `Contracts.Categories` ve `Contracts.Lists` alt ad alanlarında (enum'lar alt ad alanında — `EnumLockTests`).
- Önceki task kaydı: T332 status Commit hücresi ve raporunun "Post-merge run'lar" satırı dolduruldu.
- Backlog: D-330…D-334.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `CategoryLevels.cs`, `CategoryTree.cs`, `ProductCategoryLinks.cs`, `ProductListReader.cs`, `Category.cs` (`Relocate`), `CatalogModule.cs`; `Contracts/ICatalogModule.cs`, `Contracts/Categories/*`, `Contracts/Lists/*`
- `tests/Shopfolio.UnitTests/`: `Catalog/CategoryLevelsTests.cs`, `Architecture/CategoryLevelWriterTests.cs`, `Infrastructure/SitemapSourceTests.cs` (sahte modül yeni yöntemleri taşır)
- `tests/Shopfolio.IntegrationTests/Model/CategoryTreeAndListsTests.cs`
- `Docs/DEFERRED_BACKLOG.md` (D-330…D-334), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T332_REPORT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | En çok üç seviye; dördüncü kaydedilmez; kardeşler Türkçe alfabetik, elle sıra yok | ✓ | Integration `AFourthLevel_IsRefused_AndNothingIsWritten`, `Siblings_AreInTheTurkishAlphabeticalOrder_AtEveryLevel_WhateverTheCreationOrder`; Unit `TheDepth_ComesFromTheProductConstant_AndAFourthLevelDoesNotFit` |
| 2 | Ürün her düğüme asılabilir | ✓ | Integration `AProduct_HangsOnAnInnerNode_AndTheNodesPageHoldsItBesideItsSubCategories` |
| 3 | Kategori sayfası alt dalı tek listede gösterir; sayaç, sayfalama, süzgeç aynı kapsamda | ✓ | Integration `ACategoryPage_ListsTheBranchOnce_PerProduct_AndTheCountPagesAndFiltersFollowThatScope`; Unit `TheSubtree_IsTheCategoryFirstThenEveryCategoryBelowIt_…` |
| 4 | Ürün birden çok kategoriye asılır, biri ana kategori | ✓ | Integration `AProductOnSeveralCategories_HasExactlyOneMainCategory_AndTheMarkMoves`, `TheMainMark_MovesBetweenLinksThatStay_InBothDirections_EveryTime`, `TheMainCategoryRule_RefusesAMissingMark_…`, `ChangingTheCategories_RaisesTheProductsVersion_…` |
| 5 | Ürünü ya da alt kategorisi olan kategori silinemez | ✓ | Integration `ACategoryWithAProductOrASubCategory_IsNotDeleted_AndTheRefusalCarriesTheCounts`, `AnEmptyCategory_IsDeleted_…`, `TheDatabase_IsTheLastDefenseOfADeleteThatSlipsPastTheCounts` |
| 6 | Taşıma serbest; iki sınır (üç seviye, döngü) korunur | ✓ | Integration `AMove_UnderItsOwnBranchOrPastTheDepth_IsRefused_AndNoLevelChanges`, `AMove_ShiftsTheWholeBranchTogether_…`, `TwoOpposingMoves_AreInOrderedByTheTreeLock_SoOnlyOneHoldsAndNoCycleForms`, `EveryWrite_LeavesTheLevelAtItsParentsPlusOne`; Unit `ACategoryMovedUnderItselfOrItsOwnBranch_IsRefused`, `AMoveThatPutsAnyCategoryOfTheBranchPastTheDepth_IsRefused_…`, `CategoryLevelWriterTests` |
| 7 | Arama yalnız ürün adında; sadeleştirilmiş ad üzerinde içerir eşleşmesi; harf ve Türkçe karakter duyarsız | ✓ | Integration `TheSearch_ReadsTheProductNameOnly_NotTheDescriptionTheCategoryNameOrAnOptionValue`, `TheSearch_IsBlindToLetterCaseAndTurkishCharacters_AndMatchesAPartInTheMiddleOfTheName`, `TheSearch_TakesPercentAndUnderscoreAsLetters_AndAnEmptyWordMakesNoList` |
| 8 | Yalnız fiyat aralığı ve stok süzgeci; sıralama seçeneği ve elle sıra yok; en yeni önce; tükenmiş ürünler süzgeçsiz listede; tek satın alınabilir varyant ürünü stokta tutar | ✓ | Integration `TheLists_HaveNoSortParameterNoHandOrderAndNoOptionValueFilter_AndExactlyTheTwoFilterAxes`, `TheLists_AreNewestFirst_EqualMomentsByTheKeyDescending_AndAnUnfilteredListShowsSoldOutProducts`, `TheStockFilter_KeepsAProductWithOneBuyableVariant_…`, `ThePriceFilter_ReadsThePublishedVariantsPrices_AndAnInvertedRangeIsNotApplied`, `BothFilters_TogetherAskOneVariantToBeInRangeAndBuyable`, `TheSearchPage_TakesTheSameFiltersAndTheSamePageSize_AsTheCategoryPage`; Unit `AnInvertedPriceRange_IsVoid_AndAnyOtherRangeIsNot` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yazdıkları dahil, bütün kriter ve KK maddeleri diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1–k8 | ✓ | yukarıdaki tablo |
| KK1 dördüncü seviye reddedilir; kardeşler Cam, Çay, Dal sırasıyla | ✓ | `AFourthLevel_IsRefused_AndNothingIsWritten`, `Siblings_AreInTheTurkishAlphabeticalOrder_…` (Ç Cam ile Dal arasında) |
| KK2 ürün ara düğüme asılabilir | ✓ | `AProduct_HangsOnAnInnerNode_…` |
| KK3 üst kategori alt dal ürünlerini listeler | ✓ | `ACategoryPage_ListsTheBranchOnce_PerProduct_…` |
| KK4 ana kategori her üründe tek | ✓ | `AProductOnSeveralCategories_…`, `TheMainMark_MovesBetweenLinksThatStay_…` (tekil dizin son savunma) |
| KK5 dolu kategori silinemez | ✓ | `ACategoryWithAProductOrASubCategory_IsNotDeleted_…` |
| KK6 kendi altına ve dördüncü seviyeye taşıma reddedilir | ✓ | `AMove_UnderItsOwnBranchOrPastTheDepth_IsRefused_…` |
| KK7 açıklama, kategori adı, seçenek değeri aranmaz; AMPUL/ampul, ısıtıcı/isıtıcı aynı; adın ortası eşleşir | ✓ | `TheSearch_ReadsTheProductNameOnly_…`, `TheSearch_IsBlindToLetterCaseAndTurkishCharacters_…` |
| KK8 sıralama parametresi, elle sıra, seçenek değeri süzgeci yok; iki süzgeç var; süzgeçsiz listede tükenmiş ürün; tek satın alınabilir varyant stok süzgecinde kalır | ✓ | `TheLists_HaveNoSortParameter…`, `TheLists_AreNewestFirst_…`, `TheStockFilter_KeepsAProductWithOneBuyableVariant_…` |
| `09 §9.4` madde 4 (düzeyi yazan tek yer; ayarlayıcı çağıranları) | ✓ | `CategoryLevelWriterTests.TheLevelSetter_IsPrivate`, `TheCallsThatWriteALevel_AreInTheCategoryTreeAlone`, `TheScan_FindsACallFromAnotherFile_…` (ihlal örneği: regex `]`'den sonraki `.Relocate`'i de yakalar); `StoredDerivedValue` tutarlılık testi `Category.Level` |
| `09 §10` / `§25` | ✓ | kilit ilk deyim (`TwoOpposingMoves_…`); modül `SaveChanges` çağırmaz, kendi işlemini açmaz; okumalar `AsNoTracking`; ürün bağı izleyiciden (`VersionedSideBulkWriteScan` toplu yazıyı yasaklar, istisna listesi genişletilmedi); önbellek bildirimi `TheTreeWrites_NameTheCacheAreasTheyMakeStale`; `Invalidate` sözcüğü yalnız çalıştırıcıda (özel yöntem `MarkAreas` olarak adlandırıldı) |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | hiçbir tarama istisnası eklenmedi: önceki işçinin `ExecuteUpdate`/`ExecuteDelete` yazımı tarama bulgusu verdi, kod izleyiciye taşındı; `Invalidate` adı yeniden adlandırıldı; `AllowedPlaces` değişmedi |
| Sık FAIL (2) eşitlik denetimi tek yönlü | ✓ (uygulanmaz) | yeni enum ya da sabit tablo yok; yeni enum'lar alt ad alanında (`EnumLockTests` yeşil); `ProductCategoriesOutcome`, `CategoryWriteOutcome` saklanmaz |
| Sık FAIL (3) DB varsayılanı / FK dizini | ✓ (uygulanmaz) | şema ve göç değişmedi; kullanılan `categories`/`product_categories` kısıtları ve dizinler `CreateCatalogCoreTables` göçünde (ana işaret için dolu-süzgeçli tekil dizin `ix_product_categories_product_id`) |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | derinlik yalnız `ProductConstants`'tan; sayfa boyu yalnız `PageSizes.StorefrontProducts`; test beklentileri bu sabitlerden okur |
| Sık FAIL (5) yeni sayfalı listeye sınır testi | ✓ (düzeltme 1) | `ACategoryPage_WithEqualCreatedAtAcrossThePageBoundary_RepeatsNoProductAndLosesNone_TheLastPageIncluded` (2×boy+7 ürün hep aynı `CreatedAt`, gerçek `ProductListReader.ListCategoryAsync`, bütün sayfalar yürünür: yinelenme yok, kayıp yok, son sayfa boyu doğru; stok süzgeçli koşu da), `TheSearchPage_WithEqualCreatedAtAcrossThePageBoundary_…` (aynı sınama `SearchAsync` için; fiyat süzgeçli koşu da), `TheSearch_TakesABackslashAsALetter` (ters bölü harfi harfine aranır); ayrıca `TheLists_AreNewestFirst_EqualMomentsByTheKeyDescending_…`, `AnEmptyCategory_IsAnEmptyPage_…AndAPastLastPageShowsTheLast` (`page: int.MaxValue`), `TwoOpposingMoves_…` gerçek DB'de |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 4064 geçti, 0 kaldı (Architecture ve Consistency dahil; düzeltme 1 sonrası, `origin/main` birleşik) |
| Integration (düzeltme 1) | ✓ (parçalı) | `--filter-class …CategoryTreeAndListsTests` (yöntem süzgeçleriyle üç parça); `…ConcurrencyTests` | `CategoryTreeAndListsTests` 32/32 (3 yeni); `ConcurrencyTests` 26/26 |
| Integration (düzeltme 2) | ✓ (parçalı) | `--filter-class …CategoryTreeAndListsTests --filter-not-method "*EqualCreatedAt*"`; iki sınır testi `--filter-method` ile ayrı (postgres:18, konteyner sonra `docker rm -f -v`) | 31/31 + 1/1 + 1/1 (yeni `TheListsQueryCount_…` dahil) |
| Unit (düzeltme 2) | ✓ (parçalı) | `--filter-namespace …UnitTests.Architecture` / `…Consistency` / `…Catalog` | 936 / 241 / 61 geçti |
| Integration | ✓ (parçalı) | `dotnet test tests/Shopfolio.IntegrationTests --no-build --filter-class …` / `--filter-namespace …` | `CategoryTreeAndListsTests` 29/29; Model ad alanı 1105 (1104 geçti; süre sınırında `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled…` yük altında kesildi, tek başına geçti); Model dışı 866 (2 kırmızı: `SearchEngineDataTests…kestrel` yeniden koşuda geçti; `VersionedFormPageTests.OrderPost_ByTheAdministrator_…_KK6` sınıf içinde yerel DB'de `origin/main` üzerinde de kırmızı — `legal_text_versions` tekil dizini, bu task'la ilgisiz, tek başına geçer) |
| Format / derleme | ✓ | `dotnet format <proje> --verify-no-changes`; `dotnet build -warnaserror` | üç proje temiz, 0 uyarı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, 2026-11-07, head `d42a5a4`) — önce tur 1 ✗ FAIL (head `62d03a8`) |
| Bulgu sayısı | Tur 2: 2 (B1 DÜŞÜK S1, B2 DÜŞÜK S1) · tur 1: 1 (düzeltildi) |
| Düzeltme gerekli mi | Evet — tur 2 B1, B2 (düzeltme 2; iterasyon 1/2 kullanıldı, `12 §8.5`) |
| Bloke-etmeyen not (K) sayısı | Tur 2: 1 (K1 → D-372) · tur 1: 1 (kapandı) |

### Tur 2 (2026-11-07, head `d42a5a4` + `origin/main` birleştirmesi)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana deponun `scripts/git-hooks`'u, worktree paylaşır); ana dal CI 42408991705, 42405002224, 42405001882 success; MEMORY'de T333 satırı var.

**Kabul kriterleri (validator):** 1–8 ✓, KK1–KK8 ✓. Kanıt: kod okuması (`CategoryTree`, `CategoryLevels`, `ProductCategoryLinks`, `ProductListReader`) ve yerel koşu — Integration `CategoryTreeAndListsTests` 32/32 (iki sınır testi ayrı koşuldu: 33 sn, 41 sn), `ConcurrencyTests` 26/26 (gerçek PostgreSQL 18); Unit 4064/4064 (Architecture, Consistency dahil); build `-warnaserror` 0 uyarı; format ×3 temiz; betik testleri 4/4; dal CI 42406747948 (head `d42a5a4`) bütün işler pass. Tur 1 B1 düzeltmesi yerinde: iki listede `2×24+7` aynı `CreatedAt`'li ürün bütün sayfalarda yinelenmeden ve kaybolmadan yürünür, süzgeçli koşular da sınırı geçer; `\` harfi harfine aranır.

**Task satırları (`12 §7.5`):** VAL-A001…A008, VAL-A181…A185 uç/tarayıcı kanıtı ister; T333 uç kurmaz (D-331, D-333) → `BEKLEMEDE` kalır, `12` değişmedi.

**B1 (DÜŞÜK, S1):** `09 §25` Performans "okuma sorgusu izlemesiz ve projeksiyonlu mu?" — hayır. `09 §20` madde 3 (okuma tablosu "Liste, sorgu sınırı, performans" → §20): "listede yalnız ekranın gösterdiği sütunlar `Select` ile projeksiyonla alınır, varlığın tamamı alınmaz." `ProductListReader.PageOfAsync` `IOrderedQueryable<Product>`'u `ToPageAsync`'e verir; sayfa `Product`'ın bütün sütunlarıyla (sınırsız `Description` gövdesi dahil) okunur, karta yalnız `Id`, `Name`, `Slug` gider. D-134 (T210 K1) bunu açıkça T333'e yönlendirmişti ("projeksiyonla okur") ve kayıt açık kaldı. Koddaki gerekçe ("kayda projeksiyon edilip üyesine göre sıralanamaz") yalnız konumlu `record` için doğrudur; üyeleri `init` olan bir sınıfa (`new Row { Id = …, CreatedAt = …, Name = …, Slug = … }`) projeksiyon `OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)` ile çevrilir. **Düzeltme:** sayfa projeksiyonla okunur; D-134 kapatılır (fiyat ve stok kısmı T333'te karşılandı).

**B2 (DÜŞÜK, S1):** `09 §20` madde 4: "Zorunlu test … kalem ya da ilişki yükleyen liste ve ekranlara istenir" — liste kart rakamlarını (en düşük fiyat, satın alınabilirlik) varyantlardan yükler; sorgu sayısının sayfadaki ürün sayısından bağımsız olduğunu komut yakalayıcıyla sayan entegrasyon testi yok (`09 §25` Test: "Yeni ya da değişen her kuralın bölümündeki Test maddesi karşılanmış mı?"). Sayfa boyu sabit olduğundan test aynı listeyi az ve çok ürünle (ör. 1 ve 24) koşup komut sayısının eşit olduğunu onaylar. Üretim kodu bugün sabit sayıda sorgu çalıştırır; eksik olan kilitleyen testtir.

**K1 → D-372 (T361):** `ProductCategoryLinks.SetAsync` `Menu`/`CategoryTree` alanını bildirmez; menü önbelleği kurulunca bağ değişikliği bayat menü bırakır. Bugün o önbelleği dolduran kod yok.

**Güvenlik:** sır yok; uç yok (yetki D-333); arama deseni parametreli ve kaçışlı (`EF.Functions.Like`, `\`); ham SQL yok; yeni bağımlılık yok — temiz.

**Yapım raporu karşılaştırması:** 1 uyuşmazlık — öz denetimin "`09 §10` / `§25` ✓" satırı okumaların `AsNoTracking` olduğunu yazar, projeksiyonu ve §20 madde 4 testini anmaz (B1, B2). Kabul kriterleri ve KK tabloları validator'la uyumlu.

### Düzeltme 2 (tur 2 B1, B2) — 2026-11-08

**Düzeltme 2 (B1, B2):** B1 — `ProductListReader.PageOfAsync` sayfayı artık yalnız kartın ve sıranın sütunlarıyla (`Id`, `Name`, `Slug`, `CreatedAt`) okur: ürün sorgusu `init` üyeli özel `CardRow` sınıfına `Select` ile projeksiyon edilir, sıralama (`CreatedAt` azalan, `Id` azalan) o üyelerde kalır; `Description` ve öteki sütunlar okunmaz (`09 §20` madde 3; D-134 ✅). Davranış değişmedi: sayfa boyu, sıra, süzgeç, kart rakamları aynı. B2 — `CategoryTreeAndListsTests.TheListsQueryCount_DoesNotGrowWithThePageFill_ACategoryPageAndASearchPage`: `DbCommandInterceptor` ile komutları sayar; 1 ürünlü sayfa ile dolu sayfa (boy + 3 ürün, ikişer varyant) için kategori sayfası ve arama aynı sayıda komut gönderir (`09 §20` madde 4). `origin/main` (T334) birleşince `Physical` varyantın stok sayısı zorunlu olduğundan testlerin tohumu uyarlandı: `stock: null` yerine sayı; "sınırsız" varyant için `Digital` ürün (tohum yardımcısına `productType` parametresi). Öz denetim: kabul kriterleri 1–8 ve KK1–KK8 düzeltilmiş diff'te yeniden denetlendi (Integration `CategoryTreeAndListsTests` 31/31 + iki sınır testi ayrı 1/1 + 1/1 = 33); `09 §10` / `§25` Performans ve Test satırları artık ✓. Sınama: build `-warnaserror` 0 uyarı; format ×3 temiz; Unit Architecture 936, Consistency 241, Catalog 61 geçti.

### Tur 1 (2026-11-07, head `62d03a8`)

**Başlangıç kapıları:** working tree temiz, `core.hooksPath` = `scripts/git-hooks`; ana dal CI 42402532255, 42401437836, 42401437807 success; memory satırı var.

**Kabul kriterleri (validator):** 1–8 ✓ ve KK1–KK8 ✓ — kanıtlar yukarıdaki tabloyla aynı testler, validator yerelde koştu (Integration `CategoryTreeAndListsTests` 29/29, `ConcurrencyTests` 26/26; Unit 3983/3983; build `-warnaserror` 0 uyarı; format ×3 temiz; betik testleri; `--locked-mode` restore 0); dal CI 42401392964 (head `62d03a8`) bütün işler pass.

**Task satırları (`12 §7.5`):** VAL-A001…A008 (KP-1…KP-3), VAL-A181…A185 (KP-41) uç/tarayıcı kanıtı ister; T333 uç kurmaz (D-331, D-333) → hepsi `BEKLEMEDE` kalır; `12` değişmedi; `12_matris_denetle.py --kapsam 1-77` Hata 0.

**B1 (DÜŞÜK, S1):** `09 §25` "Yeni sayfalı liste … sınır testi var mı?" — hayır. `09 §10.5` madde 6 ve §19.2 madde 5 her yeni sayfalı liste için eşit `CreatedAt`'li ürünlerin sayfa sınırında ne yinelenmediğini ne kaybolmadığını sınayan entegrasyon testini zorunlu tutar. T333'ün iki yeni listesinde (kategori sayfası, arama) böyle test yok: `TheLists_AreNewestFirst_EqualMoments…` eşit anları tek sayfada sınar; `ACategoryPage_…` ve `TheSearchPage_…` sınırı farklı `CreatedAt`'lerle geçer; T210 `ListPagingTests` gerçek okuyucuyu değil yalın sorguyu yürür. Öz denetimin "Sık FAIL (5) ✓" satırı bu yüzden yanlış. **Düzeltme:** her iki liste için `PageSizes.StorefrontProducts`'tan fazla aynı `CreatedAt`'li ürünü sınıra düşürüp bütün sayfaları yürüyen test (yinelenme yok, kayıp yok); aynı dosyada `\` harfi harfine aranır (madde 6).

**Düzeltme 1 (B1):** yalnız test eklendi (üretim kodu değişmedi; sıralama `CreatedAt desc, Id desc` zaten ikinci anahtarlı): `CategoryTreeAndListsTests`'e iki liste için sayfa sınırı testleri (kategori sayfası, arama; ikisinde de süzgeçli koşu) ve `TheSearch_TakesABackslashAsALetter`. Öz denetimde kabul kriterleri 1–8 ve KK1–KK8 düzeltilmiş diff'te yeniden denetlendi: ilgili Integration testleri ve Unit yeşil, üretim diff'i değişmedi, kayıt belgeleri çakışması (STATUS, backlog) çözüldü, birleşme kalıntısı yok.

**K1:** "Yapılan İşler" ad değişiminde `Sitemap` bildirildiğini yazıyordu; kod ve `TheTreeWrites_NameTheCacheAreasTheyMakeStale` bildirmez (adres değişmez) — cümle düzeltildi.

**Güvenlik:** sır yok; uç yok (yetki D-333); arama deseni parametreli ve kaçışlı, ad kırpılır; yeni bağımlılık yok.

**Yapım raporu karşılaştırması:** 2 uyuşmazlık — öz denetim "Sık FAIL (5)" (B1) ve ad değişiminin `Sitemap` bildirimi (K1).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T333-kategori-arama-suzgec`
- **Commit:** PR açıklamasında head SHA
- **PR:** #442
- **CI:** *(yapım chat'i CI yoklamaz)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; ayrı `chore:` PR'ında veya sonraki task dalında)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Yerel Docker `postgres:18.6` (CI ile aynı büyük sürüm), tetikleyici ya da uzantı farkı yok | `docker run postgres:18.6`; Integration `SHOPFOLIO_TEST_DATABASE` ile Model ad alanı geçti (`pg_trgm` göçü dahil) | ✓ |
| 2 | Ana dal CI son üç tamamlanmış run success | `gh run list --branch main --limit 3`: 42398252751 (Publish), 42398252920 (CI) — `a3c9d1a`; 42393301185 (CI) — `c4c872c` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İndirimli fiyatla süzme ve kart fiyatı | D-330 | İndirim T337'nin işi |
| 2 | Kategori sayfası, arama sonucu, süzgeç formu ekranı | D-331 | Sayfa task'ları (T502, T503) |
| 3 | Kartta ana görsel | D-332 | Görsel modeli T339'da |
| 4 | Ağaç yazımının panel ekranı ve yetkisi | D-333 | T526 |
| 5 | "Ana kategori zorunlu" yayın kapısı, ürün formunda kategori seçimi | D-334 | T335 / T525 |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** 42398252751 (Publish), 42398252920 (CI) `a3c9d1a`; 42393301185 (CI) `c4c872c`: üçü success.
- **⚠ Kolay kararlar (kurala en uygun seçenek):** (1) iki süzgeç birlikteyken aynı varyant hem aralıkta hem satın alınabilir olmalı — `02 §3.5.2` ürün düzeyi fiyat anlamı tanımlamıyor, bir alışverişin gerçekten alınabilir varyantı ölçüt alındı; (2) ters fiyat aralığı (en az > en çok) uygulanmaz, süzgeç yokmuş gibi çalışır; (3) fiyat = varyant liste fiyatı (indirimli fiyat D-330); (4) ana görsel kartta yok (D-332); (5) ana kategori zorunluluğu yayın kapısına bırakıldı (D-334), `SetProductCategoriesAsync` yalnız küme ve ana işaretin tutarlılığını denetler; (6) işareti değişen bağ silinip yeniden yazılır (bağ kimliği değişir, hiçbir tablo bağ kimliğine başvurmaz).
- **Önceki işçi devri:** `src` derlenmişti, testler derlenmemişti; bu işçi test derlemesini, yeniden adlandırmayı (`ListCategoryOrNullAsync`, `SearchOrNullAsync`), liste sorgusunu (sıralama ürün üzerinde), ürün bağı yazımını (izleyici), tarama bulgularını (`Invalidate` adı, `Relocate` regex'i, `NoAssertion`) ve seçenek tohumlamasını düzeltti.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Kod birleştirmesi (doğrulama tur 3 BLOCKED sonrası):** `origin/main` (T321 #439, T334 #443) dala birleştirildi (rebase yok). Kod çakışması yalnız `CatalogModule` kurucusunda: T333'ün `_categories`/`_links`/`_lists` ataması ile main'in `_showcase` ataması birlikte tutuldu. `ProductListReader.Published()` ürün görünürlüğünü artık kendi koşuluyla yazmaz, `ShowcaseVisibility.Listed()`'a bağlıdır (`09 §24` m.4 tek kural evi); davranış testleri değişmedi. Kayıt belgeleri: STATUS "Son güncelleme" yalnız en son giriş, main'in T321 girişi `STATUS_CHANGELOG.md`'ye taşındı; backlog aktif kalem 150 (yeniden sayıldı); MEMORY iki tarafın satırları.

| Birleşmiş ağaçta yeniden denetim | Sonuç | Kanıt |
|---|---|---|
| k1–k8, KK1–KK8 | ✓ | `CategoryTreeAndListsTests` 33/33 (13 anahtar test adı yerinde: `AFourthLevel_…`, `Siblings_…`, `AProduct_HangsOnAnInnerNode_…`, `ACategoryPage_ListsTheBranchOnce_…`, `AProductOnSeveralCategories_…`, `ACategoryWithAProductOrASubCategory_…`, `AMove_UnderItsOwnBranch…`, `TheSearch_ReadsTheProductNameOnly_…`, `TheSearch_IsBlindToLetterCase…`, `TheLists_HaveNoSortParameter…`, `TheLists_AreNewestFirst_…`, `TheStockFilter_KeepsAProduct…`, `TheListsQueryCount_…`) |
| `09 §25`, güvenlik taraması | ✓ | Unit tamamı 4129/4129 (Architecture ve Consistency dahil); Integration `ProductTransitionsTests` 41/41; `build -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` temiz; `ProductListReader` yalnız okur (`AsNoTracking`), `SaveChanges`/işlem yok; çakışma işareti kalmadı |
