# T545 — Liste ve tablo, ürün kartı (OB-20, OB-11)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-11

---

## Yapılan İşler

- Ürün kartı, içerik kartı, kart ızgarası (2/3/4 sütun), numaralı sayfa gezinmesi, boş hâl satırı, arama alanı ve panel tablosu: modeller `src/Shopfolio.Web/Pages/Lists/`, görünümler `Pages/Shared/_ProductCard, _ContentCard, _ProductGrid, _ContentGrid, _Pager, _EmptyState, _ListSearch, _PanelTable`.
- Stiller: `components.css` (kart, `.card-grid`, pager, boş hâl, arama) ve `admin.css` (`.panel-table`).
- Testler: Unit (`ListComponentModelTests`, `ListSourceScanTests`, `FrameSourceScanTests` görünüm listesi) ve Integration (`ListComponentPageTests` + dört örnek sayfa).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Lists/*`, `Pages/Shared/_*` (8 görünüm), `wwwroot/css/{components,admin}.css`, `tests/Shopfolio.UnitTests/{Pages,Architecture}`, `tests/Shopfolio.IntegrationTests/{Pages,Web}`, `.claude/CONTEXT.md` (klasör haritası satırı), kayıt dosyaları.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kart: görsel, ad, KDV dahil fiyat; tek eylem; sepet ve stok yok | ✓ | `ListComponentPageTests.TheGrid_DrawsOneCardPerProduct_…_KK1_KK2`, `ACard_CarriesTheImageTheNameAndThePrice_…_KK1`; `ListSourceScanTests.TheCardViews_AreOneLink_…_KK1`; `ListComponentModelTests.TheCardRecords_HaveNoCartAndNoStockCountAndNoRatingMember_KK1` |
| 2 | Ürün başına tek kart | ✓ | `ListComponentModelTests.TheCards_AreOnePerListItem_InTheListsOrder_KK2`; `CategoryTreeAndListsTests` (varyant başına satır yok) |
| 3 | İndirim satırı, Tükendi kalır | ✓ | `ADiscountedCard_ShowsTheReferencePriceBesideThePriceAndTheDatesInOneLine…_KK3`, `ASoldOutProduct_StaysInTheListAndSaysTukendi_…_KK3` |
| 4 | İçerik kartı; görselsiz yalnız metin; fiyat/sepet yok | ✓ | `TheContentList_FollowsTheGivenOrder_ARecordWithoutAnImageIsTextOnly_…_KK4_KK6`; `ListComponentModelTests.AContentCard_CarriesTheImage…_NoPriceMember_KK4` |
| 5 | Izgara 2/3/4; ortak kart | ✓ | `ListSourceScanTests.TheCardGrid_IsTwoColumnsNarrow_ThreeMedium_FourWide_KK5`, `TheRelatedProductsAndTheShowcase_UseTheGridAndTheCard…_KK5` |
| 6 | Sabit düzen, sıralama yok; içerik panel sırası | ✓ | `ListSourceScanTests.TheListViews_HaveNoSortControl_KK6`; `ListComponentPageTests.TheGrid_KeepsTheListsOrder_AndThePageHasNoSortControl_…_KK6`; veri katmanı `CategoryTreeAndListsTests.TheLists_HaveNoSortParameter…` |
| 7 | Arama ve süzgeç yalnız sayılı listelerde | ✓ | `ListComponentModelTests.TheSearchAndTheFilter_ExistInTheListsOfTheTableOnly_KK7`; `ListComponentPageTests.TheSearchField_IsDrawnInTheListedListsOnly_…_KK7` (10 örnek) |
| 8 | Numaralı sayfalar, sonsuz kaydırma yok; 24/25 | ✓ | `ListComponentModelTests.ThePageSize_IsTheTablesNumber_KK8`, `ThePager_HasPreviousNextAndNumberedPages_…_KK8`; `ListComponentPageTests.ThePager_*`, `APageOutOfRangeOrWrong_*_KK8`; `ListSourceScanTests.NothingLoadsAListByScrolling_KK8` |
| 9 | Panel listesi: tablo/ikincil satır altı/kart; toplu işlem yok | ✓ | `ListSourceScanTests.ThePanelTable_HasNoFormNoInputNoButtonAndNoCheckbox_KK9`, `…IsNeverScrolledSidewaysAndNoCellIsHidden_KK9`; `ListComponentPageTests.ThePanelTable_IsOneMarkup…_KK9`, `ACellOfThePanelTable_IsEncoded_T545_KK9` |
| 10 | Boş liste boş hâl satırı | ✓ | `ListComponentPageTests.AnEmptyList_IsOneSentenceAndAWayOut_…_KK10`; `ListComponentModelTests.AnEmptyState_IsASentenceAndAWayOut_…_KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil kabul kriterlerinin 1-10'u ve KK1-KK10 diff'te yeniden denetlendi (yukarıdaki tablo ve aşağıdaki KK tablosu). `sik-fail-nedenleri.md` taranarak beş sık neden denetlendi: (1) tarama istisnası dar (kart taraması yalnız `_ProductCard`/`_ContentCard`, satır kısaltma yalnız `.product-card__name`); (2) kaynak taraması çağrı biçimleri: sıralama (`<select`, `sort`/`orderBy`/`siralama` parametresi), sonsuz kaydırma (`IntersectionObserver`, `scroll` dinleyicisi, `load-more`), kaydırma ve gizli hücre için ihlal örnekleri `Theory` ile var; (3) veritabanı alanı eklenmedi, "Varsayılan"/FK konusu yok; (4) kural sayıları: sayfa boyutu 24/25 tablodan türetilir (`ThePageSize_IsTheTablesNumber_KK8`), başka yerde elle sayı yok; (5) belge kuralını değiştiren yeni 09 metni yok. Dört yeni büyük hata yakalandı ve giderildi: örnek sayfa adres kuralları, örnek sayfanın `page`ı bağlamaması, `PreviousPath` adının eski adres taramasına takılması ve `[InlineData]` içindeki CSS'in test-deseni taramasını şaşırtması.

| KK | Sonuç | Kanıt |
|---|---|---|
| KK1 kart alanları; olmayan iki alan | ✓ | yukarıda kriter 1 |
| KK2 varyantsız kart | ✓ | `TheCards_AreOnePerListItem…_KK2` + `CategoryTreeAndListsTests.ACategoryPage_ListsTheBranchOnce_PerProduct…` |
| KK3 indirim satırı; Tükendi | ✓ | kriter 3 |
| KK4 görselsiz içerik kartı | ✓ | kriter 4 |
| KK5 üç sütun; ortak kart | ✓ | kriter 5 (sütun sayısı CSS taramasıyla; tarayıcıda görsel gözlem `12`'de bu task için satır yok) |
| KK6 sıralama yok | ✓ | kriter 6 |
| KK7 süzgeç yalnız sayılı listelerde | ✓ | kriter 7 |
| KK8 24/25; numaralı gezinme | ✓ | kriter 8 |
| KK9 üç sınıf; toplu işlem yok | ✓ | kriter 9 (üç sınıf düzeni CSS taramasıyla) |
| KK10 boş hâl | ✓ | kriter 10 |
| `09 §25` satırları; güvenlik taraması | ✓ | kart/liste görünümlerinde ham HTML yok (hücre kodlaması `ACellOfThePanelTable_IsEncoded`), form/girdi yok, yeni bağımlılık yok, sır yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... -c Release --filter-not-trait "Consistency=true"` | 6560 koşuldu; ilk koşuda iki mimari tarama kırmızıydı (adlandırma, `InlineData`), giderildi; ilgili sınıflar yeniden yeşil (126) |
| Consistency | ✓ | `--filter-trait "Consistency=true"` | 295/295 |
| Integration | ✓ | parçalı: `ListComponentPageTests` 25/25; Web 648/648; Model 1407 (iki göç testi yük altında zaman aşımına düştü, tek başına yeşil); geri kalan 1568/1568 | `SHOPFOLIO_TEST_DATABASE` Postgres 18 (`track_commit_timestamp=on`) |
| Derleme / biçim | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format <proje> --no-restore --verify-no-changes` (Web, Unit, Integration) | 0 uyarı, 0 hata; biçim temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T545-liste-tablo-urun-karti`
- **Commit:** `f06b4a4b` — T545: örnek sayfa adresleri, tarama uyumu, rapor ve kayıtlar
- **PR:** #538
- **CI:** yapım işçisi izlemez; yardımcı yürütür
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (plan tier, kota, API, paket sürümü, bölge kısıtı geçmiyor; yalnız görünüm ve CSS) | Kabul kriterleri tarandı | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Dar sınıfta panel kartında ikincil hücreler gizlenmez, etiketli satır olarak görünür (`04` 2.13.4 "ayrıntıda" der, 2.1.2 gizlemeyi yasaklar) | D-1155 | İlgili panel liste task'ı karar verir |
| 2 | Ürün kartı görsel adresi çağıranın verdiği `CardImage`; veri katmanında görsel alanı yok | D-1156 | Sayfa task'ları doldurur |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** son 3 run success: 42964801854, 42964801839, 42951686078.
- **Repo memory (bitiş kapısı 9):** `MEMORY.md`'ye dokunulmadı.
- **⚠ Kararlar:** ikincil hücreler dar sınıfta gösterilir (D-1155); görselsiz kartta boş yer tutucu (`product-card__image--empty`); `ListCatalog` `04` 2.13.2 tablosudur (kupon ve kurumsal içerikte arama yok); örnek sayfalar `/odeme/liste-*` (adres kuralı kapalı ilk bölüm listesi ve "kayıt adresi tam `/{önek}/{slug}`" kuralı yüzünden).
- **`12`:** bu task için `12`'de VAL satırı yok (grep T545 boş); tarayıcı gözlemi isteyen satır eklenmedi.
