# T361 — Ana sayfa düzeni ve üst menü

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama tur 1 ✗ FAIL — ürün kalıcı silme yolu menü önbelleğini düşürmüyor) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Ayarlar:** `Settings/Contracts/HomepageSettingRecords.cs` (`MenuNames`, `HomepageSection`), `ISettingsModule.GetHomepageAsync`, `FirmSettings.ReadHomepageAsync`. Düzen seçimi kaydı T535'tir; bu task yalnız okur.
- **Katalog:** `Contracts/Categories/CatalogMenu.cs`, `CatalogMenuReader` (kendisinde ve alt dallarında yayında ürünü olan kategoriler), `ICatalogModule.GetMenuAsync`. `ProductTransitions` (durum geçişi) ve `ProductCategoryLinks` (kategori bağı) Menu ve Sitemap önbelleğini düşürür.
- **Kurumsal içerik:** `StorefrontMenuReader` (sabit iskelet, düzene göre sıra, gizli öğe nedeni, altbilgi sayfaları; yapı `CacheArea.Menu`, ad ve düzen her çağrıda ayarlardan), `HomepageReader` (kurumsal blok, işaretli hizmet ve referans en çok 3, vitrin 4 ya da 8; `CacheArea.HomepageBlocks`, vitrin önbelleksiz), `ICorporateContentModule.GetMenuAsync/GetHomepageAsync`. Sözleşme tipleri `MenuItemKind`, `MenuHiddenReason`, `HomepageBlockKind` (enum dosya kuralı gereği her biri ayrı dosyada, `ApplicationEnums.NotStored` listesinde: hesaplanan okuma biçimi, sütun değil).
- **Adresler:** `AddressRules` sabit sayfa adreslerini (`ServiceListPage`, `ReferenceListPage`, `AboutPage`, `FaqPage`) taşır; site haritası okuyucusu ve menü aynı sabiti kullanır.
- **Main birleştirmesi:** `origin/main` (T362) birleştirildi; `CorporateContentModule`, `ICorporateContentModule`, `SitemapSourceTests` çakışmaları iki tarafı koruyarak çözüldü; `BrandingTests` DI kurulumuna `AddSettingsModule` eklendi (yeni `StorefrontMenuReader` bağımlılığı).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Settings/`, `Catalog/`, `CorporateContent/`, `Infrastructure/Routing/AddressRules.cs`
- Testler: `tests/Shopfolio.UnitTests/CorporateContent/StorefrontMenuAndHomepageTests.cs`, `tests/Shopfolio.IntegrationTests/CorporateContent/StorefrontReadersTests.cs`; sahte modül güncellemeleri (`SitemapSourceTests`, `AddressTests`, `MembershipTests`, `CategoryTreeAndListsTests`, `BrandingTests`), `Support/ApplicationEnums.cs`
- Kayıt: `Docs/IMPLEMENTATION_STATUS.md` (T361 satırı), `Docs/STATUS_CHANGELOG.d/T361.md`, `Docs/DEFERRED_BACKLOG.d/T361.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki düzen, varsayılan tanıtım öncelikli; vitrin en yeni yayındaki ürünler, ürüne işaret yok; ürün yoksa vitrin yok; afiş ve kampanya görseli yok | ✓ | Integration `AFreshSite_HasTheContentFirstLayout_…_KK1`, `TheShowcase_IsTheNewestPublishedProducts_…_KK1`, `ASiteWithoutAPublishedProduct_HasNoShowcaseBlock_…_KK1`, `TheLayoutSetting_HoldsOnlyTwoValues_AndTheHomepageHasNoBannerCampaignOrProductMark_KK1`; Unit `TheLayoutHasTwoValues_AndTheBlockListIsClosed_KK1`, `TheDataModel_HasNoBannerCampaignPictureOrHomepageMarkOnAProduct_…_KK1` |
| 2 | Kurumsal blok her zaman Hakkımızda; boş/yayında değilken blok kalır (hâli T359/T362) | ✓ | Integration `TheCorporateBlock_ShowsTheShortIntroductionAndTheMainImage_…_KK2`, `AnUnpublishedAboutPage_GivesNoIntroduction_ButTheCorporateBlockStays_KK2`; Unit `TheBlocks_FollowTheLayout_…AndTheCorporateBlockStays_KK1` |
| 3 | İşaretli hizmet ve referanslar elle sırayla; işaretli yoksa blok çıkmaz | ✓ | Integration `TheServiceAndReferenceBlocks_ShowOnlyPublishedFlaggedRecords_InTheManualOrder_AtMostThree_KK3`, `ABlockWithoutAFlaggedPublishedRecord_IsNotOnThePage_KK3` |
| 4 | Menü düzenleyici yok; yayında kaydı olmayan tip gizli; menü adı değişir | ✓ | Integration `ATypeWithOnlyDrafts_IsHidden_WithItsReason_…_KK4`, `ARenamedMenuItem_ShowsTheNewNameAtOnce_…_KK4`; Unit `TheMenuHasNoWayToAddRemoveOrReorderAnItem_KK4`, `ATypeShowsExactlyWhenItHasAPublishedRecord_KK4` |
| 5 | Genel sayfa altbilgide, işaretliyse menüde | ✓ | Integration `AGeneralPage_IsInTheFooter_AndInTheTopMenuOnlyWhenMarked_BeforeContact_KK5`; Unit `AGeneralPage_IsInTheFooterAlways_…_KK5` |
| 6 | Ürünsüz kategori menüde yok; yalnız alt dalda ürün varsa üst var; boş katalogda ürünler kısmı yok; boş kategori adresi boş sayfa | ✓ | Integration `ACategoryWithoutAPublishedProduct_IsNotInTheMenu_AParentWhoseOnlyProductsAreBelowIs_KK6`, `WithNoPublishedProduct_TheProductsItemIsHidden_…_KK6`, `AnEmptyCategoryAddress_ShowsAnEmptyPage_AndAnUnknownAddressIsNotFound_KK6`, `ThePublicationOfAProduct_DropsTheMenu_…_KK6`, `TheCategoriesOfAProduct_DropTheMenu_WhenTheyChange_KK6` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 düzen iki değer, varsayılan tanıtım öncelikli, işaret/afiş/kampanya alanı yok, ürünsüz sitede vitrin yok | ✓ | Yukarıdaki kriter 1 testleri; `HomepageReader.cs` |
| KK2 blokta kısa tanıtım ve ana görsel | ✓ | `TheCorporateBlock_ShowsTheShortIntroductionAndTheMainImage_…_KK2`; `HomepageView.cs` |
| KK3 işaretsiz blok hiç görünmez | ✓ | `ABlockWithoutAFlaggedPublishedRecord_IsNotOnThePage_KK3` |
| KK4 menü adı değişir, yalnız taslaklı tip gizli | ✓ | `ARenamedMenuItem_…_KK4`, `ATypeWithOnlyDrafts_…_KK4` |
| KK5 genel sayfa altbilgide, işaretliyse menüde | ✓ | `AGeneralPage_IsInTheFooter_…_KK5` |
| KK6 boş kategori yok / alt dalda ürünlü üst var / boş katalogda ürünler yok / boş kategori adresi 404 vermez | ✓ | KK6 testleri (kriter 6) |
| `09 §25` tek işlev: sabit sayfa adresleri tek yerde | ✓ | `AddressRules` sabitleri; `FixedPages` ve `CorporateContentSitemapReader.AboutPagePath` aynı sabitten; `TheFixedItemsPaths_AreTheFixedPagesOfTheAddressRules_AndProductsHasNoPage_KK4` |
| `09 §25` / T102: enum dosyası yalnız enum; sütun olmayan enum kilit dosyası istemez | ✓ | `EnumSourceScanTests`, `EnumLockTests`, `EnumTableConsistencyTests` Unit koşusunda geçti; üç enum ayrı dosya + `ApplicationEnums.NotStored` (tam adla, dar) |
| `09 §6` modül sınırı, `09 §9.1` kayıt işlemin sahibinde | ✓ | Yalnız okuma metotları, `SaveChanges`/işlem yok; `ModuleBoundaryTests` Unit koşusunda geçti |
| Sık FAIL: geniş istisna, kaçan çağrı biçimi, DB default/FK dizini, aynı sayıyı anan yerler, karar kaydısız 09 metni | ✓ | Yeni bekçi/tarama/istisna yok (tek istisna tam ad listesi); şema ve migration yok; `Docs/09` değişmedi; sayı tekrarı: kart 3 / vitrin 4–8 `HomepageRules` sabitleri, testler sabitten türetir |
| Sayfa uçları ve POST | ~ (kapsam dışı) | Sahipleri T505/T537/T535 (D-502); kabul kriterleri okuma yüzünü ister, uç istemez |
| Güvenlik taraması | ✓ | Yeni uç, giriş, sır, bağımlılık yok; yalnız okuma; kişisel veri yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release --no-build` | 4537/4537 (birleştirme sonrası) |
| Integration (CorporateContent) | ✓ | `--filter-namespace "*CorporateContent*"` | 85/85 (`StorefrontReadersTests` 21 dahil) |
| Integration (Catalog) | ✓ | `--filter-namespace "*Catalog*"` | 41/41 |
| Integration (kalan) | ✓ | `--filter-not-namespace "*CorporateContent*"` | birleştirme öncesi 2406/2407: tek başarısız `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled_…` (kararsız, tek başına 60/60); birleştirmeden sonra 28 `BrandingTests` hatası (DI kaydı) giderildi, `BrandingTests` 29/29 |
| Biçim / derleme | ✓ | `dotnet build -c Release -warnaserror`, `dotnet format Shopfolio.slnx --verify-no-changes` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-08, head `9610aa44` = `16480c3b` + `origin/main` `37d581f7`). Kriter 1–5 ve KK1–KK5 ✓, kriter 6 / KK6 ~ Kısmi; güvenlik taraması temiz; yerelde derleme `-warnaserror` 0/0, biçim temiz (3 proje), Unit 4649/4649, betik testleri 4/4, Integration (dokunulan sınıflar) 130/130, Catalog + CorporateContent ad alanı 157/157. Dal CI 42468942806: 1–6 ve 8 yeşil, 7. Paketleme restic indirmesinde dış `HTTP 504` ile kırmızı (kod değil; yeniden koşulmalı). Ana dalın son üç tamamlanmış run'ı yeşil değil (42468912436 CI `cancelled` — T338 yeniden denemesi iptal etti; 42468912494 Publish dış 504; 42466502303 CI success) — T361 kaynaklı değil |
| Bulgu sayısı | 1 — B1 DÜŞÜK S1 (kriter 6 / KK6; `09 §25` önbellek maddesi; `05 §7.10.1`; komşu satır VAL-A003): `ProductRemoval.DeleteProductAsync` `CacheArea.Menu` (ve `Sitemap`) bildirmiyor; yayındaki ürün kalıcı silinebiliyor (T335), menü yapısı süresiz önbellekte. Sonda testi (commit edilmedi): tek yayındaki ürün silinince "Ürünler" `IsVisible` = `True` kalıyor, kategori listede kalıyor; ana sayfa vitrin kategori bağlantıları da aynı önbellekten. D-499 ve "Known Limitations" 2 yolu "henüz yok" sayıyordu; yol main'de |
| Düzeltme gerekli mi | Evet — `ProductRemoval` `CacheInvalidations` alır, `DeleteProductAsync` başarıda `CacheArea.Menu` ve `CacheArea.Sitemap` bildirir; entegrasyon testi (yayındaki tek ürün silinince Ürünler gizli, kategori yok); D-499 metni ve rapordaki "yol henüz yok" düzeltilir |
| Bloke-etmeyen not (K) sayısı | 1 — K1: D-498 "önbellek süresi kadar" diyordu, `AppCache`'in süresi yok → bu PR'da metin düzeltildi |

**Task satırları (`12 §7.5`):** `grep -nE 'KP-(1|31|55)([^0-9]|$)'` → T361'e bağlı satırlar VAL-A001, VAL-A003 (KP-1), VAL-A111…A114 (KP-31), VAL-A152…A154 (KP-55); hepsi uç yanıtı / tarayıcı gözlemi ister, T361 uç ve sayfa kurmaz (D-502) → `BEKLEMEDE` kalır; `12` değişmedi; `12_matris_denetle.py --kapsam 1-77` Hata 0. B1 kabul kriteri 6'ya bağlıdır; VAL-A003'ün beklenen sonucu (yayına alınınca görünür) kod yüzünde karşılanıyor, satırın kanıtı uç ister.

**Doğrulayıcı uyuşmazlık notları (adım 13):** (1) Kriter 6 yapımda ✓, doğrulamada ~ Kısmi (B1). (2) Notlar'daki ana dal CI kanıtı yapım anının; bugün son üç tamamlanmış main run'ı yeşil değil (iptal + dış 504; T361 kaynaklı değil). Öz denetim tablosu tam ve kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T361-ana-sayfa-menu`
- **Commit:** dal head'i (PR #461 sayfasında)
- **PR:** #461
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok (plan tier, kota, paket sürümü, dış servis yok; yalnız yerel kod ve PostgreSQL) | Task okuma yüzüdür; yeni paket yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İçerik ana görseli değişince `HomepageBlocks` düşmüyor | D-498 | Görsel bağlama yolu başka task'ta |
| 2 | Ürün oluşturma/kalıcı silme menü ve site haritasını düşürmüyor | D-499 | Yol henüz yok |
| 3 | Görsel alternatif metni varsayılanı | D-500 | Sayfa task'ında karar |
| 4 | T535 düzen kaydı için düşürme notu | D-501 | T535 |
| 5 | Sayfa uçları ve düzen kaydı | D-502 | T505/T535/T537 |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi, `git status --short` boş); `core.hooksPath` = `scripts/git-hooks` (ana depo yolu).
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: 42460749345 success, 42460749391 success, 42457733364 success.
- **Repo memory (bitiş kapısı 9):** `MEMORY.md`, STATUS "Son güncelleme", `STATUS_CHANGELOG.md` ve backlog sayacına dokunulmadı.
- **⚠ kararlar:** (1) Menü sırası: içerik-önce Hakkımızda, Hizmetlerimiz, Referanslarımız, SSS, Ürünler, genel sayfalar, İletişim; mağaza-önce Ürünler, Hizmetlerimiz, Referanslarımız, Hakkımızda, SSS, genel sayfalar, İletişim (04 2.2.3 metninden; SSS yeri yorum). (2) "Ürünler" görünürlüğü = katalogda yayında ürün var mı. (3) Kurumsal blok boşken marka adı + logo geri düşüşü `Intro == null` ile işaretlenir; çizimi sayfa task'ında. (4) Üç enum saklanmayan listeye alındı (sütun değil).
- **Birleştirme notu (main ile, T335 yayın kapısı):** `StorefrontReadersTests.SeedProductAsync` taslak ürünü yayınlanmış bir varyant ve kargoya verme süresiyle tohumlar; kapı taslağı yayına çeken geçişi böylece reddetmez. Üretim koduna dokunulmadı.
