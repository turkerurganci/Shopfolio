# T333 — Kategori ağacı, site içi arama ve liste süzgeçleri

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kategori düzeyi (`09 §9.4` madde 4):** `CategoryLevels` saf hesap (düzey = üst + 1, dal kayması, derinlik sınırı `ProductConstants`'tan, kendi dalına taşıma reddi, alt dal listesi). Düzeyi yazan tek yer `CategoryTree`; `Category.Level` özel ayarlayıcılı, `Category.Create` (düzeyle) ve `Category.Relocate` yalnız ağaçtan çağrılır (kaynak taraması).
- **Ağaç yazımı:** `CategoryTree` — `CreateAsync`, `MoveAsync`, `DeleteAsync` ağaç danışma kilidini (`AdvisoryLockScope.CategoryTree`) ilk deyim olarak alır, `RenameAsync` kilitsizdir (düzeyi etkilemez). Modül `SaveChanges` çağırmaz, çağıranın işlemine katılır; ret dönüş değeridir (`CategoryWriteResult`). Dolu kategori (ürünü ya da alt kategorisi olan) silinmez; ret sayıları taşır; veritabanı son savunmadır. Yazım `Menu` + `CategoryTree` (oluşturma, silme ve ad değişiminde ayrıca `Sitemap`) önbellek alanlarını `CacheInvalidations`'a bildirir.
- **Ürün–kategori bağı:** `ProductCategoryLinks.SetAsync` — bir ürün için kategori kümesi + tek ana kategori (boş küme = ana yok; dolu küme = ana zorunlu ve kümede). Değişiklik izleyiciden geçer (toplu deyim `ProductVersionGuard`'ı atlar); işareti değişen bağ silinip yeniden yazılır (izleyici iki güncellemeyi dolu-süzgeçli tekil dizine göre sıralamaz). Değişimde ürün sürümü artar.
- **Listeler:** `ProductListReader` — `ListCategoryAsync(adres)` (kategori + bütün alt dal, ürün başına tek satır) ve `SearchAsync` (`SearchTerm.ToLikePattern` + `NameSearchForm`). Yalnız yayındaki ürün; sıralama `CreatedAt` azalan, `Id` azalan; sayfa `PageSizes.StorefrontProducts`; sayfa ürün üzerinde sıralanır, kartın en düşük fiyatı ve satın alınabilirliği sayfanın anahtarlarıyla ikinci sorguda okunur. Süzgeç: fiyat aralığı + stokta; ters aralık uygulanmaz; iki süzgeç birlikteyse aynı varyant hem aralıkta hem satın alınabilir olmalıdır.
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
| Sık FAIL (5) yeni sayfalı listeye sınır testi | ✓ | `TheLists_AreNewestFirst_EqualMomentsByTheKeyDescending_…` (aynı `CreatedAt`, anahtar azalan), `AnEmptyCategory_IsAnEmptyPage_…AndAPastLastPageShowsTheLast` (`page: int.MaxValue`, son sayfa, boş sayfa), `TheSearchPage_TakesTheSameFiltersAndTheSamePageSize_…` (iki sayfa); eşzamanlılık: `TwoOpposingMoves_…` gerçek DB'de |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 3983 geçti, 0 kaldı (Architecture ve Consistency dahil) |
| Integration | ✓ (parçalı) | `dotnet test tests/Shopfolio.IntegrationTests --no-build --filter-class …` / `--filter-namespace …` | `CategoryTreeAndListsTests` 29/29; Model ad alanı 1105 (1104 geçti; süre sınırında `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled…` yük altında kesildi, tek başına geçti); Model dışı 866 (2 kırmızı: `SearchEngineDataTests…kestrel` yeniden koşuda geçti; `VersionedFormPageTests.OrderPost_ByTheAdministrator_…_KK6` sınıf içinde yerel DB'de `origin/main` üzerinde de kırmızı — `legal_text_versions` tekil dizini, bu task'la ilgisiz, tek başına geçer) |
| Format / derleme | ✓ | `dotnet format <proje> --verify-no-changes`; `dotnet build -warnaserror` | üç proje temiz, 0 uyarı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T333-kategori-arama-suzgec`
- **Commit:** PR açıklamasında head SHA
- **PR:** bkz. status
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
