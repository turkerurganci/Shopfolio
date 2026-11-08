# T335 — Ürün yayın durumu: yayın kapısı, arşiv, silme ve engelleme kalıbı

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Yayın kapısı (kriter 3, 4, 10):** `UnwrittenPublishGate` kalktı. `PublishGateRules` (saf koşullar: ad, ana kategori, tip, en az bir Yayında varyant, dijital üründe her Yayında varyantın dosyası, hizmette ifa süresi) ve onu okuyan `CatalogPublishGate`. Koşul adları kapalı listedir: `Contracts/PublishCondition.All`. Yaş, izin, engelleme mekanizması, yasaklı kategori ve mevzuat belgesi alanı yoktur; kapı ürünün ne olduğuna bakmaz.
- **Dijital dosya tek okuma (kriter 4, 5):** `DigitalFileCoverage` — varyantın kendi güncel dosyası ya da ürün düzeyi güncel dosya. Yayın kapısı ve dosya silme aynı yeri kullanır; ikisi ayrışamaz. `ICatalogModule.ListVariantsWithoutFileAsync` panelin göstereceği eksik varyantları verir.
- **Son Yayında varyant tek kural (kriter 9):** `PublishedProductGate`; durum geçişi (`ProductTransitions`) ve varyant silme aynı kuralı çağırır; ret `LastPublishedVariant` ve her ikisinde aynı `SuggestedProductMoves` (Taslak, Arşiv) listesi (`LastVariantGuidance`).
- **Kalıcı silme ve dosya kaldırma (kriter 5, 8):** `ProductRemoval` / `IProductRemoval` / `RemovalResult`: ürün ve varyant silme (`RecordSave` ile sürüm karşılaştırması, sepet kalemleri önce işaretlenir), `RemoveDigitalFileAsync` (satır kalır, `DetachedAt` yazılır; yayındaki dijital ürünün Yayında varyantını dosyasız bırakacaksa `DigitalFileRequired`), etki sayımı (`GetProductImpactAsync`, `GetVariantImpactAsync`: açık sipariş sayısı, ayırma adedi, son Yayında varyant mı). Ordering tarafı: `IOrderingModule.CountOpenOrdersOfProductAsync/OfVariantAsync`, `TransitionTable.TerminalStatuses`.
- **Adres cevabı (kriter 6, 7):** `ProductPageReader` / `ICatalogModule.GetProductPageAsync`, `ProductPage` + `ProductPageKind` (taslak ziyaretçiye NotFound, yöneticiye bantlı önizleme ve taslak varyantlar; arşiv varyantsız, fiyatsız, sepetsiz; silinmiş ya da bilinmeyen adres NotFound).
- **Bu turda (devam işçisi):** tümleşik testler (`ProductPublishGateTests`, 31 test, gerçek kapıyla), `ProductPageKind` kendi dosyasına alındı (enum dosyası taraması) ve `ApplicationEnums.NotStored` listesine eklendi (saklanmayan sonuç türü; kilit dosyası gerekmez), silme taramasına ham SQL biçimi eklendi (`DELETE FROM`/`TRUNCATE` ürün ve varyant tablolarında), belgeler.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `PublishGate.cs`, `DigitalFileCoverage.cs`, `PublishedProductGate.cs`, `ProductRemoval.cs`, `ProductPageReader.cs`, `ProductTransitions.cs`, `DigitalFileReader.cs`, `CatalogModule.cs`, `ProductDefinitionRules.cs`; `Contracts/`: `PublishCondition.cs`, `IProductRemoval.cs`, `RemovalResult.cs`, `ProductPage.cs`, `ProductPageKind.cs`, `PublishChangeResult.cs`, `ICatalogModule.cs`.
- `src/Shopfolio.Web/Modules/Ordering/`: `OrderingModule.cs`, `Contracts/IOrderingModule.cs`, `TransitionTable.cs`.
- Testler: `tests/Shopfolio.UnitTests/Catalog/PublishGateTests.cs`, `PublishTransitionsTests.cs`, `Infrastructure/SitemapSourceTests.cs`, `Support/ApplicationEnums.cs`; `tests/Shopfolio.IntegrationTests/Catalog/ProductPublishGateTests.cs`.
- Belgeler: bu rapor, `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `DEFERRED_BACKLOG.md`, `.claude/memory/MEMORY.md`; T316, T321, T334 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri (skill adım 15).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün ve varyant durumu ayrı; geçişler T321 makinesinden | ✓ | `ProductPublishGateTests.APublishedProduct_KeepsItsStatus_WhenOneOfItsVariantsGoesToDraft_KK1`, `ADraftProduct_CanHoldAPublishedVariant_*_KK1`; T321 `ProductTransitionsTests` 41/41 |
| 2 | Arşiv silme değil | ✓ | `AnArchivedProduct_KeepsItsRecordAndVariants_AndLeavesTheListAndTheOptions_KK2` |
| 3 | Yayına: ad, ana kategori, tip, Yayında varyant; açıklama/görsel zorunlu değil | ✓ | `AProductWithoutNameCategoryAndPublishedVariant_IsRefused_*_KK3`, `ACategoryThatIsNotTheMainOne_*_KK3`, `AProductWithNameCategoryAndAPublishedVariant_GoesPublished_WithoutDescriptionOrImage_KK3`; Unit `PublishGateTests.*_KK3` |
| 4 | Dijital: her Yayında varyant dosyalı; hizmet: ifa süresi | ✓ | `ADigitalProduct_WithAPublishedVariantWithoutAFile_*_KK4`, `AProductLevelFile_ServesEveryVariant_*_KK4`, `ADigitalVariantWithoutAFile_CannotGoPublished_*_KK4`, `AServiceProduct_NeedsItsFulfilmentPeriod_*_KK4` |
| 5 | Yayındaki dijital varyantı dosyasız bırakan dosya silme engelli | ✓ | `RemovingTheOnlyFileOfAPublishedDigitalVariant_IsRefused_*_KK5`, `RemovingAProductLevelFile_IsRefused_*_KK5`, `RemovingAVariantsOwnFile_IsAllowed_*_KK5` |
| 6 | Taslak adres: ziyaretçi 404, yönetici bant; taslak varyant ziyaretçiye görünmez | ✓ | `ADraftProductsAddress_IsNotFoundForAVisitor_*_KK6`, `APublishedProductsAddress_ShowsTheVisitorOnlyThePublishedVariants_*_KK6` |
| 7 | Arşiv adresi "artık satılmıyor": fiyat, sepet yok; liste ve aramada yok | ✓ | `AnArchivedProductsAddress_IsTheNoLongerSoldPage_WithoutVariantsPriceOrCart_KK7` (iki kullanıcı türü) |
| 8 | Silme kalıcı; geçmiş sipariş bozulmaz; açık siparişli silinir, açık sipariş sayısı söylenir, ayırma düşer | ✓ | `ADeletedProduct_LeavesItsOldOrder_*_KK8`, `AProductWithAnOpenOrder_CanBeDeleted_AndTheImpactCountsOnlyTheOpenOrders_KK8`, `ADeletedVariant_TakesItsReservationWithIt_*_KK8`, `ADeletion_WithAStaleVersion_OrOfAGoneRecord_*_KK8` |
| 9 | Son Yayında varyant arşivlenemez, taslağa çekilemez, silinemez; panel yönlendirir | ✓ | `TheLastPublishedVariant_IsKeptByTheDraftMove_TheArchiveMove_AndTheDeletion_KK9`, `AVariant_IsFree_WhenAnotherVariantIsPublished_*_KK9`, `ThePublishedVariantOfAProductThatIsNotPublished_IsFree_*_KK9` |
| 10 | Kapı ürünün ne olduğuna bakmaz; yaş/izin/yasaklı kategori alanı yok | ✓ | Unit `TheConditionSet_IsClosed_AndNamesNothingAboutWhatTheProductIs_KK10`, `TheGateAnswer_DoesNotDependOnTheNameOrTheCategoryOrTheProductsNature_KK10`, `TheModelAndTheGate_CarryNoFieldOfAgeCheckPermissionBlockOrLegalDocument_KK10` |

### Öz denetim (bitiş kapısı 2)

> Önceki işçinin yaptıkları dahil bütün maddeler bu işçi tarafından diff'te yeniden denetlendi (`grep -n` + satır aralığı).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 ürün/varyant bağımsız | ✓ | `ProductPublishGateTests` KK1 iki test; `PublishGate.cs` ürün ve varyant ayrı `CheckProductAsync/CheckVariantAsync` |
| KK2 arşivde kayıt durur, vitrinde yok | ✓ | `ProductPublishGateTests` KK2 (`ListShowcaseAsync`, `ListOptionVariantsAsync` boş; satır ve varyant sayısı değişmez) |
| KK3 eksik ad/kategori/varyantla Yayında reddedilir, adlarıyla | ✓ | `UnmetConditions == [Name, MainCategory, PublishedVariant]`; ürün sürümü ve iz satırı değişmez (aynı test) |
| KK4 dosyasız dijital varyantla kapı kapalı | ✓ | `ListVariantsWithoutFileAsync` varyant kimliğini döner; ayrılmış dosya sayılmaz (`detached: true` tohumu) |
| KK5 dosya silme yayındaki varyantı dosyasız bırakıyorsa engelli | ✓ | `DigitalFileRequired` + `VariantsWithoutFile`; taslak/arşivde serbest, tekrar `Unchanged`, bağlanmamış yükleme `NotFound`, `DetachedAt` yazılır, satır kalır |
| KK6 taslak adres ziyaretçide yok, yöneticide bant | ✓ | `ProductPage.ShowsDraftBand`, `NotFound`; taslak varyant yalnız yöneticide `Status` ile |
| KK7 arşiv sayfasında fiyat ve sepet yok | ✓ | `Variants` boş, `ShowsPrice/OffersCart/IsIndexable` false; Unit `ThePageKinds_ShowWhatTheyShow_KK6_KK7`, `OnlyAPublishedPage_OffersTheCartAndTheSearchEngines_*` |
| KK8 silinen ürünün eski siparişi bozulmaz | ✓ | sipariş parmak izi aynı; kalem `ProductId/VariantId` null, ad ve tutarlar aynı; ayırma satırı düşer; sepet kalemi `RemovedProductName = "Masa"`; iz satırı yazılmaz |
| KK9 son Yayında varyant üç yolla korunur | ✓ | varyant Draft, Archived ve `DeleteVariantAsync` aynı `LastPublishedVariant` ve aynı `SuggestedProductMoves`; `GetVariantImpactAsync.IsLastPublishedVariant` |
| KK10 yaş/izin/yasaklı kategori alanı yok | ✓ | Unit KK10 üç test; `PublishCondition.All` kapalı liste, integration testi her ret adının listede olduğunu doğrular |
| `09 §25` Doküman uyumu ve kapsam | ✓ | yeni ad yok (`PublishCondition` adları `07 §6.2` hata kodlarıyla uyumlu); yeni ayar/sabit yok; diff yalnız T335 |
| `09 §25` Mimari: modül sınırı, `SaveChanges` | ✓ | `ProductRemoval.cs` içinde `SaveChanges`/`BeginTransaction` yok (Unit `TheRemovalUnit_SavesOnlyThroughTheRecordSave_*`); `RemoveDigitalFileAsync` yalnız izlenen değişiklik bırakır, kaydı işlemin sahibi yapar; kapı ve silme tek kural evinden (`PublishedProductGate`, `DigitalFileCoverage`) |
| `09 §25` Durum/olay: iş yan etkisi, iz | ✓ (⚠) | silme ve dosya kaldırma bildirim üretmez, iz satırı yazmaz (⚠ ii) |
| `09 §25` Eşzamanlılık | ✓ (⚠) | silmede ürün sürümü `RecordSave` ile; dosya satırı kilidi alınır (`DigitalFileLocks`); dosya silme ile yayın kapısı yarışı kilitsiz, D-401 |
| `09 §25` Veri: göç, FK, kilit dosyası | ✓ | şema ve göç değişikliği yok; yeni saklı enum yok; `ProductPageKind` saklanmaz (`NotStored`, kilit dosyası yok) |
| `09 §25` Sayfalı liste | uygulanamaz | yeni sayfalı liste yok |
| Silme taraması (sık FAIL 1, 2) | ✓ | `CatalogDeletionScan`: izinli yer tam yolla `ProductRemoval.cs` (dosya adı değil); biçimler: izlenen silme, toplu silme, `Set<>`, bağlam, `EntityState.Deleted`, ham SQL `DELETE FROM`/`TRUNCATE` (bu turda eklendi); olumsuz örnekler (`cart_items`, `UPDATE variants`, yorum) |
| DB varsayılanı / FK dizini (sık FAIL 3) | uygulanamaz | yeni sütun ya da tablo yok |
| Kural değişince öteki yerler (sık FAIL 4) | ✓ | `UnwrittenPublishGate` için kalan başvuru yok (`grep`); T321 testleri `PublishTransitionsTests` güncel; backlog D-248/D-300/D-303/D-304/D-301 işlendi; `ProductDefinitionRules` açıklaması `PublishGateRules`'a işaret eder |
| Tek kural evi (sık FAIL 5) | ✓ | ifa süresi `ProductDefinitionRules.IsFulfilmentPeriodEntered`; son Yayında varyant `PublishedProductGate`; dosya kapsamı `DigitalFileCoverage` |
| Güvenlik taraması | ✓ | sır/kişisel veri yok; silme ve dosya kaldırma yönetici panelinin işlemleridir (uç yok, çağıran yetkiyi uygular); adres cevabı taslak ürünü ziyaretçiden `NotFound` ile gizler (taslak/silinmiş ayırt edilmez, `02 §3.30.4`); ham SQL yok (parametresiz biçim yok); yeni paket yok; arşiv sayfası arama motoruna kapalı (`IsIndexable` false) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency hariç) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build --filter-not-trait "Consistency=true"` | 3925/3925 |
| Unit Consistency | ✓ | aynı, `--filter-trait "Consistency=true"` | 241/241 |
| Integration `ProductPublishGateTests` | ✓ | `--filter-class "*ProductPublishGateTests"` (PostgreSQL 18, yerel) | 31/31 |
| Integration `ProductTransitionsTests` (T321) | ✓ | `--filter-class "*ProductTransitionsTests"` | 41/41 |
| Integration Ordering | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Ordering` | 125/125 |
| Integration Concurrency, Disposal | ✓ | ilgili ad alanları | 26/26, 45/45 |
| Integration Web | ✓ (⚠ 1 kararsız) | `--filter-namespace Shopfolio.IntegrationTests.Web` | 156/157; kırılan `AppCacheTests.ThePriceShowsAtOnce_*(kestrel: True)` tek başına koşunca 14/14 geçti (zamanlamaya bağlı, T335 koduna dokunmuyor) |
| Format / derleme | ✓ | `dotnet build -warnaserror`, `dotnet format --verify-no-changes` (Web, Unit, Integration) | temiz |

### Main birleştirmesi (2026-11-08, `origin/main` T339 #445)

`origin/main` dala birleştirildi (rebase yok). Çakışma: `CatalogModule.cs` (kurucuya `ProductPageReader pages` ve `ProductGallery gallery` birlikte; alanlar ve yöntemler iki taraftan korundu), `IMPLEMENTATION_STATUS.md` (son güncelleme T335, T339 girişi `STATUS_CHANGELOG.md`'ye), `DEFERRED_BACKLOG.md` (main kayıtları + D-401…D-403; aktif kalem 153). Main'in galeri kodu yayın kapısına ve dosya silme kuralına dokunmaz: `ProductGallery` yalnız galeri bağlarını (`ProductImages`) yazar, `PublishedProductGate` ve `DigitalFileCoverage` tek kural evi olarak durur. Birleşik ağaçta bütün kabul kriterleri (1–10), KK1–KK10, `09 §25` ve güvenlik taraması yeniden denetlendi; yukarıdaki tablolar geçerli, sonuçlar değişmedi:

| Tür | Sonuç (birleşik ağaç) |
|---|---|
| Unit (Architecture 943, Consistency 241, öteki 3084) | ✓ 4268/4268 |
| Integration `ProductPublishGateTests` | ✓ 31/31 |
| Integration `ProductTransitionsTests` | ✓ 41/41 |
| Integration `ProductGalleryTests` (T339) | ✓ 18/18 |
| `build -warnaserror`, `dotnet format --verify-no-changes` | ✓ temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-08) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama Sonucu — T335 Ürün yayın durumu — tur 1
**Tarih:** 2026-11-08   **Branch:** task/T335-urun-yayin-kapisi   **Commit:** `7241002` (+ `origin/main` `e2e9dbc` (T341, #433) birleştirmesi, kayıt commit'inde)

#### Verdict: ✓ PASS

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un `scripts/git-hooks`'u, mutlak yol; daldaki kopya kullanılmadı — talimat kuralı); değiştirilmedi |
| Ana dal CI (son 3) | ✓ | 42426248059 success (T341), 42423723835 success (T339), 42423723933 success (T339) |
| Memory drift | ✓ | `grep -nE "\bT335\b" .claude/memory/MEMORY.md` → satır 22 (yapım satırı) |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün ve varyant durumu ayrı; geçişler T321 makinesinden | ✓ | `ProductTransitions.ChangeProductStatusAsync/ChangeVariantStatusAsync` kapıyı (`IPublishGate`) ve `PublishedProductGate`'i çağırır; durum alanını yalnız `ApplyPublishStatus` yazar. Integration `*_KK1` (2 test) ✓, `ProductTransitionsTests` 41/41 ✓ |
| 2 | Arşiv silme değil; kayıt durur, vitrinden çıkar | ✓ | `AnArchivedProduct_KeepsItsRecordAndVariants_AndLeavesTheListAndTheOptions_KK2` ✓ (satır ve varyant sayısı aynı; vitrin listesi ve seçenek listesi boş) |
| 3 | Yayına: ad, ana kategori, tip, ≥1 Yayında varyant; açıklama ve görsel zorunlu değil | ✓ | `PublishGateRules.UnmetOfProduct` (`PublishGate.cs`) dört koşul; `CatalogPublishGate` ana kategoriyi `IsPrimaryCategory`'den, Yayında varyantı sayımla okur. Unit `*_KK3` (6 test) ✓; Integration `*_KK3` (3 test) ✓ — ana olmayan kategori kapıyı açmaz, açıklamasız ve görselsiz ürün yayına geçer |
| 4 | Dijital: Yayında her varyant dosyalı (kendi ya da ürününki); hizmet: ifa süresi | ✓ | `DigitalFileCoverage.FindPublishedWithoutFileAsync` (ayrılmış dosya sayılmaz); ifa süresi `ProductDefinitionRules.IsFulfilmentPeriodEntered` (tek kural evi). Unit `*_KK4` ✓; Integration `*_KK4` (ürün düzeyi dosya, ayrılmış dosya, taslak ürün altında dijital varyant, hizmet 0/null/1 gün) ✓ |
| 5 | Yayındaki dijital varyantı dosyasız bırakan dosya silme engellenir | ✓ | `ProductRemoval.RemoveDigitalFileAsync`: dosya satırı kilidi (`DigitalFileLocks`) → yayındaki dijital üründe `FindPublishedWithoutFileAsync(..., exceptFileId)` → `DigitalFileRequired` + varyant listesi; satır kalır (`DetachedAt`). Integration `*_KK5` (5 test/teori) ✓ |
| 6 | Taslak adres: ziyaretçiye NotFound, yöneticiye "Taslak" bandı; taslak varyant ziyaretçiye görünmez | ✓ | `ProductPageReader.ReadAsync` + `ProductPage.ShowsDraftBand`; ziyaretçi varyantları `InOptionList` (T321 kuralı). Integration `ADraftProductsAddress_*_KK6`, `APublishedProductsAddress_*_KK6`, `AnAddressThatHoldsNoProduct_*_KK6` ✓. Sayfa/uç T503 ve T549'da (`11` T503 k3, T549 k1–k3) |
| 7 | Arşiv adresi "Bu ürün artık satılmıyor": fiyat ve sepet yok; liste ve aramada yok | ✓ | `ProductPageKind.Archived` → varyant listesi boş, `ShowsPrice`/`OffersCart`/`IsIndexable` false; liste/arama dışlaması T321 `ShowcaseReader`. Integration `AnArchivedProductsAddress_*_KK7` (ziyaretçi ve yönetici) ✓; Unit `ThePageKinds_*`, `OnlyAPublishedPage_*` ✓ |
| 8 | Silme kalıcı; geçmiş sipariş bozulmaz; açık siparişli de silinir, açık sipariş sayısı onaydan önce; varyantın ayırması düşer | ✓ | `ProductRemoval.DeleteProductAsync/DeleteVariantAsync` (sürüm `RecordSave`, sepet kalemleri önce işaretlenir); `GetProductImpactAsync/GetVariantImpactAsync` açık sipariş sayısı (`IOrderingModule.CountOpenOrdersOf*`, terminal = `TransitionTable.TerminalStatuses`) ve ayrılmış adet. Integration `*_KK8` (5 test): sipariş parmak izi aynı, kalemin `ProductId/VariantId` null, ad/tutarlar donmuş; 2 açık + 2 kapalı siparişte sayım 2; `StockReservations` 0 ✓ |
| 9 | Son Yayında varyant arşivlenemez, taslağa çekilemez, silinemez; panel yönlendirir | ✓ | `PublishedProductGate.IsLastPublishedVariantAsync` tek kural; durum geçişi ve silme aynı `LastPublishedVariant` + `SuggestedProductMoves` = [Draft, Archived]. Integration `*_KK9` (4 test) ✓ — sürüm ve sepet değişmez |
| 10 | Kapı ürünün ne olduğuna bakmaz; yaş/izin/engelleme/yasaklı kategori/mevzuat belgesi alanı yok | ✓ | `PublishCondition.All` kapalı altı ad; Unit `*_KK10` (3 test + tarama sınaması) ✓ — model ve kapı adlarında yasak sözcük yok, ad ne olursa olsun aynı cevap |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-A021, A022, A023, D001, A024 (KP-7) | ORTA | yok | BEKLEMEDE | BEKLEMEDE (kanıt türü uç yanıtı; adres sayfası T503/T549 — modül okuması ✓) | — |
| VAL-C037, C038 (KP-40) | ORTA | yok | BEKLEMEDE | BEKLEMEDE (veri kaydı ✓; panel/uç kanıtı T525) | — |
| VAL-A177, A178, A179, A180 (KP-40) | ORTA | yok | BEKLEMEDE | BEKLEMEDE (önizleme ve silme onayı panel/tarayıcı kanıtı; T525/T503) | — |
| VAL-D025 (KP-40) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE (veri kaydı ✓ — KK8; "sipariş sayfası açılır" uç kanıtı sayfa task'larında) | — |
| VAL-A163, A171 (KP-39) | ORTA | yok | BEKLEMEDE | BEKLEMEDE (kapının veri kaydı ✓ — KK3, KK4; uç yanıtı T525) | — |

KP-39'un öteki satırları (A164–A170, A172–A176) T335 kriterlerine bağlı değil (T332/T334/T336/T525). KD-28 ve KD-38 için `12`'de satır yok. Hücre değişmedi; `python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77` hata 0.

#### Doğrulama Kontrol Listesi
- [x] KK1: ürün ve varyant durumu bağımsız değişir
- [x] KK2: arşivde kayıt durur, vitrinde yok
- [x] KK3: eksik ad, kategori, tip ya da varyantla Yayında reddedilir
- [x] KK4: dosyasız dijital varyantla kapı kapalı
- [x] KK5: dosya silme yayındaki varyantı dosyasız bırakıyorsa engellenir
- [x] KK6: taslak adres ziyaretçide yok, yöneticide bant
- [x] KK7: arşiv sayfasında fiyat ve sepet düğmesi yok
- [x] KK8: silinen ürünün eski siparişi bozulmaz
- [x] KK9: son Yayında varyant üç yolla da korunur
- [x] KK10: yaş, izin ve yasaklı kategori alanı yok; kapı türe bakmaz
- [x] `09 §25` ilgili satırlar: modül sınırı (Contracts, `ProductRemoval`'da `SaveChanges`/işlem açma yok — Unit tarama testi), tek kural evi (`PublishedProductGate`, `DigitalFileCoverage`, `ProductDefinitionRules`), şema/göç/paket/ayar yok, zaman `TimeProvider`'dan, iz alanı kapalı (`02 §10.3.1` ürün silmeyi saymaz), eşzamanlılık: silme ve geçiş ürün sürümüyle; dosya silme–yayın yarışı D-401'de ileri kayıtlı
- [x] Güvenlik taraması

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | 3 proje |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata (birleşik ağaçta da) |
| Format | ✓ | Web: `dotnet format --verify-no-changes --no-restore` 0; Unit ve Integration: `format whitespace`/`style`/`analyzers --verify-no-changes` her biri 0 (tam `format` Unit'te 150 sn'yi aştığı için alt komutlara bölündü) | temiz |
| Unit (Consistency hariç) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build --filter-not-trait "Consistency=true"` | 4027/4027 (birleşik ağaç 4039/4039) |
| Unit Consistency | ✓ | `--filter-trait "Consistency=true"` | 241/241 (birleşik ağaçta da) |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration `ProductPublishGateTests` | ✓ | `--filter-class ...ProductPublishGateTests` + `--filter-method "*KKn"` üç grup | 6 + 16 + 9 = 31/31 |
| Integration `ProductTransitionsTests` | ✓ | `--filter-class "*ProductTransitionsTests"` | 41/41 |
| Ortam | ✓ | `postgres:18`, konteyner `shopfolio-t335-val-pg` 127.0.0.1:55731; koşu sonrası `docker rm -f -v` ile silindi; `git status` temiz; `dotnet restore --locked-mode` 0 | — |
| Dal CI (adım 8a) | ✓ | `gh pr checks 447`; `gh run list --branch task/T335-urun-yayin-kapisi` | 42425264272 success (head `7241002`): Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate pass |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (diff'te sır, bağlantı dizesi, anahtar yok)
- Auth etkisi: Temiz — yeni uç yok; `GetProductPageAsync(isAdministrator)` bayrağını çağıran sayfa oturumdan verecek (T503); taslak ve silinmiş adres aynı NotFound (sebep ayırt edilmez)
- Input validation: Temiz — ham SQL yok; slug boşsa NotFound; silmede ürün sürümü karşılaştırılır
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
Yok.

#### Bloke-etmeyen Notlar (K-notları)
Yok. (Dosya silme ile ürünü yayına alma arasındaki kilitsiz yarış yapımda D-401'e, yayındaki ürünün zorunlu alanını boşaltan düzenleme D-403'e ileri atılmış; doğrulayıcı ikisini de gördü, yeni kayıt açmadı.)

#### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu. Öz denetim tablosu her kriter ve KK için ✓ + kanıt taşıyor. Yapımın Unit sayısı birleşik ağaçta 4268 (= 4027 + 241) ile bu doğrulamanın sayısı aynı. ⚠ kararları (açık sipariş = terminal olmayan sipariş; silmede iz satırı yok; yönetici Yayında üründe taslak varyantı görür) belgeyle çelişmiyor (`02 §3.7.7`, §10.3.1, §3.7.5; ödemesi Başarısız sipariş İptal edildi'dir — `02 §3.17.6`).

#### Merge ve Post-merge
- Squash: yardımcı yapar (doğrulayıcı merge etmez)
- Post-merge run'lar: yardımcı izler


## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T335-urun-yayin-kapisi`
- **Commit:** son head SHA PR gövdesinde
- **PR:** #447
- **CI:** yöneticide (yapım işçisi CI izlemez)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Yerel PostgreSQL CI ile aynı sürümde | `.github/workflows/ci.yml` `postgres:18`; yerelde `postgres:18` konteyneri | ✓ |
| 2 | Dış servis, plan katmanı, kota, paket sürümü: yok | task kapsamı yalnız modül kuralı ve veritabanı; yeni paket yok | Dış varsayım: yok |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Dosya silme ile yayın kapısının eşanlı yarışı kilitsiz | D-401 | `09 §10.2` m.5 kilit listesi değişikliği ve eşanlı test ayrı iş |
| 2 | Kategori silme ve `CategoryNotEmpty` engeli | D-402 | kategori ağacı T333'te |
| 3 | Yayındaki ürünün zorunlu alanını boşaltan düzenleme `PublishedRecordGateBroken` | D-403 | ürün formu kaydı henüz yok (UC-114) |
| 4 | Siparişe girememe sebepleri | D-303 (kalan kısım) | T346 |
| 5 | `PublishStatus` ayarlayıcıları public, test tohumları doğrudan yazar | D-301 (yeniden ileri) | ~40 test yerinin tek kurucuya taşınması ayrı yeniden düzenleme; en geç F3 gate-check |

## Notlar

- **Kararlar (⚠, kolay, kurala en uygun seçenekle):**
  - ⚠ (i) Açık sipariş = terminal olmayan sipariş (Teslim edildi ve İptal dışındaki); `02 §3.7.7`'nin "ödeme beklenen ya da ödenmiş ve tamamlanmamış" tanımının geçiş tablosuyla karşılığı.
  - ⚠ (ii) Silme ve dosya kaldırma iz satırı yazmaz: `02 §10.3.1`'in iz alanları kapalı ve ürün silme listede yok.
  - ⚠ (iii) Yönetici Yayında üründe taslak varyantları da görür (`Status` alanıyla); `02 §3.7.5` yöneticinin taslağı gördüğünü söyler, Yayında üründeki taslak varyant için açık cümle yok.
  - ⚠ (iv) D-301 yeniden ileri atıldı (en geç F3 gate-check).
  - ⚠ (v) Sayfa metinleri ve Razor sayfaları yok (`Pages` boş): adres cevabı modül okuması olarak yazıldı; panel iletisi ve sayfa panel/vitrin task'larının işi (D-304 modül kısmı çözüldü).
  - ⚠ (vi) T316'nın main CI koşusu (42408991723) `cancelled` görünüyor (aynı ref'in yarışan koşusu); sonraki üç squash'ın CI'ı yeşil. Merge kaydına işlendi.
  - ⚠ (vii) `ProductPageKind` saklanmayan bir sonuç türü olduğundan `06 §5` tablosuna girmedi; `ApplicationEnums.NotStored` listesine eklendi (T316'nın `TransitionRefusal`'ı gibi).
- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** son üç tamamlanmış koşu success: 42419095662 (CI), 42419095630 (Publish), 42411903876 (Publish).
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Devam kaydı:** önceki işçi kod ve birim testlerini yazıp bölündü; bu işçi tümleşik testleri, taramayı, belgeleri ve öz denetimi tamamladı.
