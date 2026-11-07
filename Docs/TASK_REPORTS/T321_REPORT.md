# T321 — Ürün yayın durumları

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- Geçiş birimi `ProductTransitions`: ürün ve varyantın Taslak/Yayında/Arşiv geçişi; gösterilen ürün formu sürümünü karşılaştırır (`shownVersion`), her geçiş ürün sürümünü artırır, `ChangePublishStatus` iz satırı yazar, bildirim ve iş üretmez.
- Yayın kapısı `IPublishGate`: Yayında'ya geçiş kapıyı çağırır, red durumu değiştirmez. Gerçek koşullar T335'te; şimdilik `UnwrittenPublishGate` hep açık (D-300).
- Son Yayında varyantın taslağa/arşive alınması `LastPublishedVariant` ile engellenir; ürünün kendisi her zaman çıkabilir.
- İlk yayında `Product.SlugFixedAt` ve ilk satış anı (`Variant.MarkFirstOnSale`) bir kez yazılır.
- Vitrin okuması `ShowcaseReader`: `ShowcaseVisibility.Listed` (yalnız Yayında ürün) ve `InOptionList` (ürün ve varyant Yayında); liste, arama ve seçenek listesi; tükenmişlik süzgeçte yok, işaretle gösterilir. `CatalogSitemapReader` aynı `Listed` kuralını kullanır.
- Sepet: `IOrderingModule.RemoveFromCartsAsync` vitrinden kalkan kalemi sepetten çıkarır (kalem `RemovedProductName` ile işaretlenir, sepette sayılmaz); `TakeRemovedItemNoticesAsync` bir kez söyler ve siler.
- `ICatalogModule`: `ListShowcaseAsync`, `ListOptionVariantsAsync` (T332 yöntemleriyle birleştirildi).

## Etkilenen Modüller / Dosyaları

- Catalog: `Contracts/IProductTransitions.cs`, `Contracts/PublishChangeResult.cs`, `Contracts/ShowcaseRecords.cs`, `Contracts/ICatalogModule.cs`, `ProductTransitions.cs`, `PublishGate.cs`, `ShowcaseReader.cs`, `CatalogModule.cs`, `CatalogSitemapReader.cs`, `Product.cs`, `Variant.cs`
- Ordering: `Contracts/IOrderingModule.cs`, `OrderingModule.cs`, sepet kalemi `RemovedProductName`
- Testler: `UnitTests/Catalog/PublishTransitionsTests.cs`, `IntegrationTests/Catalog/ProductTransitionsTests.cs`, `UnitTests/Infrastructure/SitemapSourceTests.cs` (FakeCatalog)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün ve varyant üç değeri ayrı ayrı taşır | ✓ | `EveryMoveBetweenTheThreeStatuses_IsFree_ForTheProductAndForTheVariant_KK1` |
| 2 | Geçiş panel işlemi, bildirim yok, iz var | ✓ | `AMove_WritesOneTrailRowOfThePublishStatus_AndNoJobAndNoDispatch_KK2`; tek yazıcı taraması `ApplicationSource_WritesThePublishStatusOfAProductAndAVariantOnlyInTheTransitionUnit_KK2` |
| 3 | Taslak ürün listede/aramada yok; taslak varyant seçenek listesinde yok | ✓ | `TheList_AndTheSearch_ShowOnlyThePublishedProducts_...KK3_KK4`, `TheOptionList_ShowsOnlyThePublishedVariants_...KK3_KK4` |
| 4 | Arşiv ürün listede yok, kaydı korunur; arşivlenmiş varyant vitrinden kalkar | ✓ | aynı testler + `AVariantArchivedUnderAPublishedProduct_LeavesTheOptionList_...KK4` |
| 5 | Yayında'ya geçiş kapıdan geçer, red durumu değiştirmez | ✓ | `AGateThatRefuses_LeavesTheStatusAsItWas_NoRowAndNoVersionRise_KK5` |
| 6 | Son Yayında varyantı bozan işlem engellenir, sebep kodu döner | ✓ (panel iletisi D-304) | `TheLastPublishedVariantOfAPublishedProduct_CannotLeavePublished_KK6`, `Result_TheRefusals_CarryTheCodesOfTheRegistry_KK5_KK6` |
| 7 | Vitrinden kalkan kalem sepetten çıkar, bir kez söylenir | ✓ (okuma ucu bağı D-302) | `AProductTakenOffTheShowcase_LeavesTheCarts_AndIsToldOnce_KK7`, `AVariantTakenOffTheShowcase_...KK7` |
| 8 | Yayın durumu siparişi etkilemez; satış kapatma durumu değiştirmez | ✓ | `AnOpenOrder_IsNotTouchedByAnyMoveOfItsProductOrVariant_KK8`, `TemporarilyClosingTheSales_ChangesNoPublishStatus_AndTheListStays_KK8` |
| 9 | Tükenmişlik ayrı durum değil, görünür | ✓ | `ASoldOutPublishedProduct_IsInTheSearch_AndInTheList_MarkedSoldOut_KK9`, `Visibility_DoesNotLookAtTheStock_...KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 ürün Yayında iken varyant Taslak (ve tersi) | ✓ | `APublishedProduct_CanHaveADraftVariant_AndTheOtherWayRound_KK1` |
| KK2 bildirim yok, iz satırı var; iz yazılamazsa geri alınır | ✓ | `AMove_WritesOneTrailRow...KK2`, `AMoveWhoseTrailRowCannotBeWritten_IsRolledBackWithItsCartEffect_KK2_KK7` |
| KK3 taslak ürün/varyant listede yok | ✓ | `Listed_KeepsExactlyThePublishedProducts_OfEveryStatus_KK3_KK4`, `InOptionList_KeepsAVariantExactlyWhenItAndItsProductArePublished_InEveryPairOfStatuses_KK3_KK4` (her durum çifti, çift yönlü) |
| KK4 arşiv ürün listede/aramada yok, kaydı okunur; arşiv varyant vitrinde yok | ✓ | `TheList_AndTheSearch_...KK3_KK4` (kayıt sayısı korunur), `AVariantArchivedUnderAPublishedProduct_...KK4` |
| KK5 kapı çağrılır; sahte red durumu değiştirmez; çıkış geçişinde kapı sorulmaz | ✓ | `AMoveToPublished_AsksTheGate_...KK5`, `AGateThatRefuses_...KK5`, `TheGateIsNotAskedForAMoveOutOfPublished_...KK5` |
| KK6 son Yayında varyant engellenir, ileti yönlendirir | ✓ kod; ~ ileti metni panel task'ında (D-304) | `TheLastPublishedVariant...KK6`, `APublishedVariantMayLeave_...KK6`, `TheProductItself_MayLeavePublished_...KK6` |
| KK7 arşive alınan ürün sepetten çıkar, bir kez söylenir | ✓ | `AProductTakenOffTheShowcase_...KK7`, `AMoveThatDoesNotTakeAnythingOffTheShowcase_LeavesTheCartsAlone_KK7`, `ARefusedMove_LeavesTheCartsAlone_KK7` |
| KK8 açık sipariş etkilenmez | ✓ | `AnOpenOrder_IsNotTouchedByAnyMoveOfItsProductOrVariant_KK8` |
| KK9 tükenmiş Yayında ürün aramada görünür | ✓ | `ASoldOutPublishedProduct_IsInTheSearch_...KK9` |
| `09 §25` ilgili satırlar (bekçi/tarama) | ✓ | tek yazıcı taraması ve ihlal örneği `Scan_OnACallOutsideTheUnit_ReportsEachWriter_AndOnTheUnitsOwnFileNothing`; başka dosyadaki aynı ad izinli sayılmaz `Scan_OnAFileOfTheUnitsNameElsewhere_IsNotTheAllowedPlace`; `VersionedSideBulkWriteScan` listesi genişletilmedi |
| Sık FAIL 1 (tarama istisnası geniş) | ✓ | istisna yalnız geçiş biriminin kendi dosyası; yolu/adı taklit eden dosya geçmez |
| Sık FAIL 2 (eşitlik tek yönlü) | ✓ | `Listed`/`InOptionList` testleri her durum için hem içeri hem dışarı denetler |
| Sık FAIL 3 (DB varsayılanı / FK dizini) | ✓ yeni tablo/sütun yok; `CartItem.RemovedProductName` önceden vardı | `CartAndOrderCoreTablesTests` yeşil |
| Sık FAIL 4 (aynı sayıyı anan yerler) | ✓ | backlog aktif kalem 125 → 130 (başlık ve beş yeni satır); status ve memory aynı sayıyla |
| Sık FAIL 5 (beklenen değer sabitten türetilir) | ✓ | durum kümesi `Enum.GetValues<PublishStatus>()` ile türetilir; el yazımı kopya yok |
| Güvenlik taraması | ✓ | yeni uç/form/yüzey yok; yazma yolları yalnız modül arayüzünde, işlem içinde; kullanıcı girdisi yalnız arama sözcüğü ve parametreli sorguya gider |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 3976/3976 (main birleştirmesi sonrası) |
| Integration (T321) | ✓ | `--filter-class *ProductTransitionsTests` | 35/35 |
| Integration (komşu: Sitemap, CatalogCoreTables, CartAndOrderCoreTables, ModuleCallTransaction) | ✓ | `--filter-class` dörtlü | 138/138 |
| Integration (SearchEngineData, ListPaging + ProductTransitions) | ✓ | `--filter-class` üçlü | 68/68 |
| Integration (Ordering + Concurrency; önceki işçi) | ✓ | — | 57/57 |
| Derleme / biçim | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format --verify-no-changes` (üç proje) | 0 uyarı, 0 hata; değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — (yapım) |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`CartItem.RemovedProductName` önceden vardı; yeni kullanım)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T321-urun-yayin-durumlari`
- **Commit:** PR açılırken son head SHA PR gövdesinde
- **PR:** PR açıldığında bildirilir
- **CI:** yoklanmadı (yapım işçisi izlemez)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok. Kapı koşulları T335'te yazılır; T321 kapıyı sahte kapıyla sınar | `UnwrittenPublishGate`, D-300 | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Yayın kapısı hep açık | D-300 | koşullar T335 madde 3 ve 4 |
| 2 | `PublishStatus` ayarlayıcıları public (test tohumları) | D-301 | tohumlar kurucuya taşınınca kapanır |
| 3 | "Bir kez söylenir" okuma ucu sayfaya bağlı değil | D-302 | sepet sayfası T346 |
| 4 | Kalıcı silme ve siparişe girememe durumları | D-303 | T335 / T346 |
| 5 | Son Yayında varyant engelinin panel iletisi | D-304 | panel sayfası T335 |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** main CI success: 42388017677, 42388017617, 42385726793; devam turunda main `c4c872c` run'ları success: 42393301195, 42393301185
- **Hooks:** `core.hooksPath` = `scripts/git-hooks`
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ (kolay, kurala en uygun seçenekle karar verildi):**
  - Kapı şimdilik hep açık (D-300).
  - `PublishStatus` ayarlayıcısı public; tarama yalnız `Apply*` çağrılarını kısıtlar (D-301).
  - Durum değişikliği ürün sürümünü artırır ve `shownVersion` ister (`06 §2.6.2`; varyant formun üyesi).
  - Sepet kalemi işareti toplu yazım yerine izlenen satırla yazıldı (`VersionedSideBulkWriteScan` listesini genişletmemek için).
  - T332 ile birleştirmede `ICatalogModule`, `CatalogModule` ve `SitemapSourceTests.FakeCatalog` iki tarafın üyeleriyle birleştirildi.
