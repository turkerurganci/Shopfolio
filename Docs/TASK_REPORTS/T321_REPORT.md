# T321 — Ürün yayın durumları

**Faz:** F3 | **Durum:** ⏳ Düzeltme 2 yapıldı (tur 2 FAIL sonrası; doğrulama tur 3 bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- Geçiş birimi `ProductTransitions`: ürün ve varyantın Taslak/Yayında/Arşiv geçişi; gösterilen ürün formu sürümünü karşılaştırır (`shownVersion`), her geçiş ürün sürümünü artırır, `ChangePublishStatus` iz satırı yazar, bildirim ve iş üretmez.
- Yayın kapısı `IPublishGate`: Yayında'ya geçiş kapıyı çağırır, red durumu değiştirmez. Gerçek koşullar T335'te; şimdilik `UnwrittenPublishGate` hep açık (D-300).
- Son Yayında varyantın taslağa/arşive alınması `LastPublishedVariant` ile engellenir; ürünün kendisi her zaman çıkabilir.
- İlk yayında `Product.SlugFixedAt` ve ilk satış anı (`Variant.MarkFirstOnSale`) bir kez yazılır.
- Vitrin okuması `ShowcaseReader`: `ShowcaseVisibility.Listed` (yalnız Yayında ürün) ve `InOptionList` (ürün ve varyant Yayında); liste, arama ve seçenek listesi; tükenmişlik süzgeçte yok, işaretle gösterilir. `CatalogSitemapReader` aynı `Listed` kuralını kullanır.
- Sepet: `IOrderingModule.RemoveFromCartsAsync` vitrinden kalkan kalemi sepetten çıkarır (kalem `RemovedProductName` ile işaretlenir, sepette sayılmaz); `TakeRemovedItemNoticesAsync` bir kez söyler ve siler (sepetin tavsiye kilidi altında; istisna yakalamaz).
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
| Unit (düzeltme sonrası) | ✓ | `--no-build`, Architecture ve öteki ayrı | 906 + 3075 |
| Integration (T321, düzeltme sonrası) | ✓ | `--filter-class *ProductTransitionsTests` (5 yeni test) | 40/40; `SitemapEndpointTests` 10/10 |
| Integration (komşu: Sitemap, CatalogCoreTables, CartAndOrderCoreTables, ModuleCallTransaction) | ✓ | `--filter-class` dörtlü | 138/138 |
| Integration (SearchEngineData, ListPaging + ProductTransitions) | ✓ | `--filter-class` üçlü | 68/68 |
| Integration (Ordering + Concurrency; önceki işçi) | ✓ | — | 57/57 |
| Derleme / biçim | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format --verify-no-changes` (üç proje) | 0 uyarı, 0 hata; değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, 2026-11-07; iki iterasyon sınırı doldu — eskalasyon, `12 §8.5`) · tur 1 ✗ FAIL |
| Bulgu sayısı | tur 2: 1 (Bulgu 1 DÜŞÜK S1 — `IOrderingModule` arayüz metotları `SaveChangesAsync` çağırıyor, `09 §6` m.3, m.7) · tur 1: 2 (sınır testi yok; `DbUpdateConcurrencyException` yakalama — ikisi kapandı) |
| Düzeltme gerekli mi | Evet — tur 2 Bulgu 1 (eskalasyon kararıyla) |
| Bloke-etmeyen not (K) sayısı | tur 2: 1 (K1 → D-353) · tur 1: 0 |

### Doğrulama Sonucu — T321 Ürün yayın durumları — tur 1
**Tarih:** 2026-11-07   **Branch:** task/T321-urun-yayin-durumlari   **Commit:** `8d71d89` (origin/main `c4c872c` zaten içeride; birleştirme "Already up to date")

#### Verdict: ✗ FAIL

Kabul kriterleri 1–9 ve KK1–KK9 davranış olarak karşılandı; testler ve CI yeşil. FAIL sebebi `09 §25` kontrol listesinin iki "hayır" cevabı (§25: "hayır düzeltilir ve düzeltilmeden kabul edilmez"): yeni sayfalı vitrin listesinin sınır testi yok (Bulgu 1) ve `DbUpdateConcurrencyException` kayıt varyantı dışında ret olarak yakalanıyor (Bulgu 2).

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un bekçisi; daldaki kopya kullanılmadı) |
| Ana dal CI (son 3) | ✓ | 42393301185 success, 42393301195 success (T332), 42391220711 success (T381) |
| Memory drift | ✓ | `.claude/memory/MEMORY.md:22` T321 satırı |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün ve varyant Taslak/Yayında/Arşiv'i ayrı taşır | ✓ | `ProductTransitions.ChangeProductStatusAsync`/`ChangeVariantStatusAsync` ayrı alan yazar; `APublishedProduct_CanHaveADraftVariant_AndTheOtherWayRound_KK1`, `EveryMoveBetweenTheThreeStatuses_IsFree_..._KK1` (6 geçiş × ürün+varyant) yerelde yeşil |
| 2 | Geçiş panel işlemi, bildirim yok, iz satırı var | ✓ | `ChangePublishStatus` iz satırı aynı işlemde; Job/Dispatch 0 — `AMove_WritesOneTrailRowOfThePublishStatus_AndNoJobAndNoDispatch_KK2`; iz yazılamazsa geri alınır `AMoveWhoseTrailRowCannotBeWritten_..._KK2_KK7` |
| 3 | Taslak ürün liste/aramada yok; taslak varyant seçenek listesinde yok | ✓ | `ShowcaseVisibility.Listed`/`InOptionList`; `TheList_AndTheSearch_ShowOnlyThePublishedProducts_..._KK3_KK4`, `TheOptionList_ShowsOnlyThePublishedVariants_..._KK3_KK4`; birim `Listed_...`, `InOptionList_..._InEveryPairOfStatuses` |
| 4 | Arşiv ürün liste/aramada yok, kayıt korunur; arşiv varyant vitrinden kalkar | ✓ | aynı testler (arşiv kaydı okunur) + `AVariantArchivedUnderAPublishedProduct_LeavesTheOptionList_..._KK4` |
| 5 | Yayında'ya geçiş kapıdan geçer, red durumu değiştirmez | ✓ | `IPublishGate` yalnız hedef Published iken sorulur; `AMoveToPublished_AsksTheGate_..._KK5`, `AGateThatRefuses_LeavesTheStatusAsItWas_NoRowAndNoVersionRise_KK5` (Draft, Archived); gerçek koşullar T335 (D-300) |
| 6 | Kapıyı bozan işlem engellenir, sebep söylenir, yönlendirilir | ✓ (birim düzeyi) | `LastPublishedVariant` (`07 §6.2`, 422) döner, iz/sürüm/sepet yazılmaz — `TheLastPublishedVariantOfAPublishedProduct_CannotLeavePublished_KK6`, `ARefusedMove_LeavesTheCartsAlone_KK7`; panel metni T335 m.9'un kriteri (D-304) |
| 7 | Vitrinden kalkan kalem sepetten çıkar, bir kez söylenir | ✓ | `RemoveFromCartsAsync` durum yazımından sonra `RemovedProductName` işaretler (`06` CartItem); `TakeRemovedItemNoticesAsync` verip siler — `AProductTakenOffTheShowcase_LeavesTheCarts_AndIsToldOnce_KK7` (ikinci çağrı boş), `AVariantTakenOffTheShowcase_..._KK7`; sayfa bağı D-302 |
| 8 | Yayın durumu siparişi etkilemez; satış kapatma durumu değiştirmez | ✓ | `AnOpenOrder_IsNotTouchedByAnyMoveOfItsProductOrVariant_KK8` (durum, sürüm, tutar, geçiş kaydı parmak izi), `TemporarilyClosingTheSales_ChangesNoPublishStatus_AndTheListStays_KK8` |
| 9 | Tükenmişlik ayrı durum değil, tükenmiş Yayında ürün görünür | ✓ | `ASoldOutPublishedProduct_IsInTheSearch_AndInTheList_MarkedSoldOut_KK9`; birim `Visibility_DoesNotLookAtTheStock_..._KK9` |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
T321'in birimleri KP-7 ve KP-40. Satırların hepsi uç yanıtı, panel ya da tarayıcı kanıtı ister (adres sayfaları, önizleme bandı, panel geçişi, silme); T321 uç ve sayfa yazmaz — adres ve panel T335 m.6–9'da. `A024` (taslak varyant seçenek listesinde yok) ve `C037`/`C038` (panel geçişi) T321 kriteri 3 ve 1'in davranışını anar ama kanıt türü "Uç yanıtı"dır; üretilemez. Hiçbir satır T321'in kriteriyle tam kanıtlanmadı, `Durum` değişmedi.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-A021, A022, A023, D001, A024 | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-C037, C038, A177, A178, A179, A180 | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D025 | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — (silme, T335) |

#### Doğrulama Kontrol Listesi
- [x] KK1: ürün Yayında iken varyantı Taslak olabilir.
- [x] KK2: geçişte bildirim yok, iz satırı var.
- [x] KK3: taslak ürün listede ve aramada yok; taslak varyant seçenek listesinde yok.
- [x] KK4: arşiv ürün listede ve aramada yok, kaydı okunur; arşivlenmiş varyant vitrinde yok.
- [x] KK5: Yayında'ya geçiş kapıyı çağırır; sahte kapının reddi durumu değiştirmez.
- [x] KK6: son Yayında varyantın arşive alınması engellenir; ret kodu `LastPublishedVariant` (panel metni T335 m.9, D-304).
- [x] KK7: arşive alınan ürün sepetten çıkar, bir kez söylenir.
- [x] KK8: açık sipariş yayın durumundan etkilenmez.
- [x] KK9: tükenmiş Yayında ürün aramada görünür.
- [ ] `09 §25` ilgili satırlar — iki "hayır": "Yeni sayfalı liste … sınır testi var mı?" (Bulgu 1); "İstisna yalnız beklenmeyen hâlde mi …" / `09 §10.2` m.2 (Bulgu 2). Öteki satırlar evet ya da uygulanamaz (göç, yeni paket, dış servis, POST/form yok).
- [x] Güvenlik taraması temiz.

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx`; sonda `--locked-mode` | çıkış 0 |
| Derleme | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` (Web, Unit, Integration) | üçü çıkış 0 |
| Unit Architecture | ✓ | `--filter-namespace Shopfolio.UnitTests.Architecture` | 906/906 |
| Unit öteki (Consistency dahil) | ✓ | `--filter-not-namespace Shopfolio.UnitTests.Architecture` | 3070/3070 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya |
| Integration `ProductTransitionsTests` | ✓ | Postgres 18, `shopfolio-t321-val-pg`, 127.0.0.1:55871 | 35/35 |
| Integration `SitemapEndpointTests` | ✓ | aynı | 10/10 |
| Dal CI | ✓ | `gh pr checks 439`, run 42394867534 (head `8d71d89`) | Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate pass |

Koşu sonrası konteyner adıyla silindi, `git status` temiz.

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz — yeni uç, sayfa ya da form yok; geçiş birimi işlem dışında çağrılırsa `InvalidOperationException`, iz satırı oturumdaki yöneticiden
- Input validation: Temiz — arama sözcüğü `SearchTerm.ToLikePattern` ve parametreli `EF.Functions.Like`'tan geçer; hedef durum `Enum.IsDefined` ile denetlenir
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | `09 §25` (satır yok) | DÜŞÜK | S1 | Beklenen: her yeni sayfalı liste sınır testiyle eklenir (`09 §19.2` m.5, `§10.5` m.6, `§25`) — aynı `CreatedAt`'li ürünler 24'lük sayfa sınırında ne yinelenir ne kaybolur, son sayfayı ve `int` aralığını aşan `page` son sayfayı verir, tek sayfalık listede `PageCount` 1. Gerçekleşen: `ShowcaseReader.ListAsync` / `ICatalogModule.ListShowcaseAsync` yeni sayfalı liste (`ToPageAsync`, `PageSizes.StorefrontProducts`); testlerin hepsi `page = 1` ile çağırır, sınır testi yok (T210'un `ListPagingTests`'i elle kurulan sorguyu sınar, bu okuyucuyu değil). | — | bu PR'da düzeltilir: `ListShowcaseAsync` üzerinden sınır testi (ya da sayfalama task kapsamında değilse listeyi sayfasız bırakmak) | `src/Shopfolio.Web/Modules/Catalog/ShowcaseReader.cs` · `tests/Shopfolio.IntegrationTests/Catalog/ProductTransitionsTests.cs` |
| 2 | `09 §25` (satır yok) | DÜŞÜK | S1 | Beklenen: `DbUpdateConcurrencyException` yalnız kayıt varyantının kaydında (`RecordSave`) ret olarak yakalanır, öteki her yerde beklenmeyen hatadır (`09 §10.2` m.2, `§8.1`). Gerçekleşen: `OrderingModule.TakeRemovedItemNoticesAsync` silmede bu istisnayı yakalayıp boş liste döner (`OrderingModule.cs:83`). | KK7 ("bir kez söylenir") | bu PR'da düzeltilir: yakalama kaldırılır ya da eşzamanlı ikinci isteğin boş dönmesi istisnasız bir yolla sağlanır (ör. sepet satırını kilitleyip okuma) | `src/Shopfolio.Web/Modules/Ordering/OrderingModule.cs` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| — | Yeni K-notu yok (KK6 panel metni D-304, sayfa bağı D-302, kapı D-300 yapımda zaten forward edildi) | — |

#### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Öz denetim tablosunda `09 §25` satırı ✓; validator iki "hayır" buldu (Bulgu 1, 2). Raporun "ListPaging + ProductTransitions" koşusu `ShowcaseReader`'ın sayfalamasını kapsamaz.
- KK6 kabul kriterleri tablosunda "✓", öz denetimde "✓ kod; ~ ileti metni" — iki tablo tutarsız. Validator kriteri birim düzeyinde ✓ saydı (panel metni T335 m.9).
- Öteki kriterlerde kanıtlar validatorınkiyle uyumlu.

#### Merge ve Post-merge
- FAIL — merge yok.

## Düzeltme — tur 1 sonrası

| Bulgu | Düzeltme | Kanıt |
|---|---|---|
| 1 Vitrin listesi sınır testi yok | `ListShowcaseAsync` üzerinden üç test: aynı `CreatedAt`'li 49 ürün 24+24+1 sayfada ne yinelenir ne kaybolur; sayfa 4 ve `int.MaxValue` son sayfayı, 0 ilk sayfayı verir; tek sayfalık listede `PageCount` 1 | `TheShowcaseList_WithProductsOfTheSameCreatedAt_NeitherRepeatsNorLosesOneAtThePageBorders`, `AShowcasePage_PastTheLast_IsTheLastPage_AndOneBelowTheFirst_IsTheFirst`, `AShowcaseListOfOnePage_HasPageCountOne` |
| 2 `DbUpdateConcurrencyException` yakalanıyor | Yakalama kaldırıldı; `TakeRemovedItemNoticesAsync` işlemin ilk deyimi olarak sepetin tavsiye kilidini (`AdvisoryLocks`, `AdvisoryLockScope.Cart`, 09 §10.2 m.5) alır, sonra okur ve siler; eşzamanlı ikinci istek kilitte bekler, işaretli kalem bulamaz ve boş döner. Toplu yazma yok (`VersionedSideBulkWriteScan` yeşil); kilit metni yeni dosyaya girmedi | `TwoRequestsTakingTheNoticesTogether_TellThemOnce_AndNeitherThrows_KK7` (iki eşzamanlı çağrı, toplam 2 ad), `AProductTakenOffTheShowcase_..._KK7` (ikinci çağrı boş) |

Not: `TakeRemovedItemNoticesAsync` artık çağıranın işleminde çalışmalıdır (kilit işlem dışında alınamaz); sayfa bağı D-302'de (T346) işlem içinde çağrılacak.

### Öz denetim — düzeltme turu (11'deki kriterlerin ve KK1–KK9'un bütünü, düzeltilmiş diff'te)

| Madde | Sonuç | Kanıt |
|---|---|---|
| 1 / KK1 ürün ve varyant durumu ayrı | ✓ | `EveryMoveBetweenTheThreeStatuses_IsFree_...KK1`, `APublishedProduct_CanHaveADraftVariant_...KK1` (40/40 yeşil) |
| 2 / KK2 panel işlemi, bildirim yok, iz var | ✓ | `AMove_WritesOneTrailRow...KK2`, `AMoveWhoseTrailRowCannotBeWritten_...KK2_KK7` |
| 3 / KK3 taslak vitrinde yok | ✓ | `TheList_AndTheSearch_...KK3_KK4`, `TheOptionList_...KK3_KK4`; yeni sayfalama testleri yalnız Yayında ürünleri sayar (taslak tohum sayıya girmez) |
| 4 / KK4 arşiv vitrinde yok, kayıt korunur | ✓ | aynı testler, `AVariantArchivedUnderAPublishedProduct_...KK4` |
| 5 / KK5 kapı, red durumu değiştirmez | ✓ | `AGateThatRefuses_...KK5` |
| 6 / KK6 son Yayında varyant engeli | ✓ (panel iletisi D-304) | `TheLastPublishedVariantOfAPublishedProduct_CannotLeavePublished_KK6` |
| 7 / KK7 sepetten çıkar, bir kez söylenir | ✓ | `AProductTakenOffTheShowcase_...KK7` (ikinci çağrı boş), `TwoRequestsTakingTheNoticesTogether_...KK7` (eşzamanlı) |
| 8 / KK8 sipariş etkilenmez | ✓ | `AnOpenOrder_IsNotTouchedBy...KK8`, `TemporarilyClosingTheSales_...KK8` |
| 9 / KK9 tükenmiş görünür | ✓ | `ASoldOutPublishedProduct_...KK9` |
| `09 §25` sayfalı liste sınır testi | ✓ | üç sayfalama testi (Bulgu 1) |
| `09 §25` / `§10.2` m.2 istisna yalnız beklenmeyen hâlde | ✓ | `OrderingModule.cs`'de `DbUpdateConcurrencyException` yok (`grep` boş) |
| `09 §25` bekçi/tarama | ✓ | `VersionedSideBulkWriteScanTests`, `ConcurrencySourceScan` yeşil (Architecture 906/906) |
| Güvenlik | ✓ | yeni uç/girdi yok; kilit anahtarı sepet kimliği, parametreli |

### Doğrulama Sonucu — T321 Ürün yayın durumları — tur 2
**Tarih:** 2026-11-07   **Branch:** task/T321-urun-yayin-durumlari   **Commit:** `921491a` (doğrulanan head; kayıt commit'i öncesi `origin/main` `6eec072` birleştirildi)

#### Verdict: ✗ FAIL

Tek bulgu: `09 §25` "Modül sınırları korunuyor mu — … arayüz metodunda `SaveChanges` yok" sorusunun cevabı **hayır** (Bulgu 1). Kabul kriterleri 1–9 ve KK1–KK9 kanıtla karşılanıyor; tur 1'in iki bulgusu kapandı. Bu ikinci iterasyondur: `12 §8.5` iki iterasyon sınırı doldu — yeni doğrulama turu açılmaz, eskalasyon (`INSTRUCTIONS.md` §3.3).

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (dal `T321-val` = `origin/task/T321-urun-yayin-durumlari` `921491a`) |
| Hook | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` (ana kopyanın `scripts/git-hooks`'u) |
| Ana dal CI (son 3) | ✓ | 42402532255 success (#436), 42401437836 success (#438), 42401437807 success (#438) |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22'de T321 |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün ve varyant Taslak/Yayında/Arşiv'i ayrı taşır | ✓ | `ProductTransitions.ChangeProductStatusAsync`/`ChangeVariantStatusAsync` ayrı alanlara yazar; `APublishedProduct_CanHaveADraftVariant_AndTheOtherWayRound_KK1`, `EveryMoveBetweenTheThreeStatuses_IsFree_…_KK1` (6 geçiş × ürün+varyant) yeşil |
| 2 | Geçiş yönetici işlemi, bildirim yok, işlem izine yazılır | ✓ | Tek `ChangePublishStatus` iz satırı, `Job`/`DispatchRecord` 0 (`AMove_WritesOneTrailRow…_KK2`); iz yazılamazsa durum ve sepet etkisi geri alınır (`AMoveWhoseTrailRowCannotBeWritten_…_KK2_KK7`); `AuditOperation.ChangePublishStatus` `09 §9.2` listesinde; tek yazıcı taraması `ApplicationSource_WritesThePublishStatus…_KK2` + ihlal örnekleri |
| 3 | Taslak ürün listede/aramada yok; taslak varyant seçenek listesinde yok | ✓ | `ShowcaseVisibility.Listed`/`InOptionList`; `TheList_AndTheSearch_ShowOnlyThePublishedProducts_…_KK3_KK4`, `TheOptionList_ShowsOnlyThePublishedVariants_…_KK3_KK4`, birim `Listed_…`, `InOptionList_…InEveryPairOfStatuses` |
| 4 | Arşiv ürün listede/aramada yok, kaydı korunur; arşivlenmiş varyant vitrinden kalkar | ✓ | Aynı testler (arşiv kaydı okunur) + `AVariantArchivedUnderAPublishedProduct_LeavesTheOptionList_…_KK4` |
| 5 | Yayında'ya geçiş kapıdan geçer, red durumu değiştirmez | ✓ | `AMoveToPublished_AsksTheGate_…_KK5`, `AGateThatRefuses_LeavesTheStatusAsItWas_NoRowAndNoVersionRise_KK5` (Draft/Archived), `TheGateIsNotAskedForAMoveOutOfPublished_…_KK5`; gerçek koşullar D-300 → T335 |
| 6 | Kapıyı bozan işlem engellenir, sebep söylenir, yönlendirilir | ✓ | `LastPublishedVariant` (07 §6.2, 422) döner, hiçbir şey yazılmaz (`TheLastPublishedVariantOfAPublishedProduct_CannotLeavePublished_KK6` Draft/Archived, `APublishedVariantMayLeave_…_KK6`, `TheProductItself_MayLeavePublished_…_KK6`); panel metni T335 m.9'da (D-304) — T321'de panel sayfası yok |
| 7 | Vitrinden kalkan kalem sepetten çıkar, bir kez söylenir | ✓ | `AProductTakenOffTheShowcase_LeavesTheCarts_AndIsToldOnce_KK7` (Archived/Draft), `AVariantTakenOffTheShowcase_…_KK7`, `AMoveThatDoesNotTakeAnythingOffTheShowcase_…_KK7`, `ARefusedMove_LeavesTheCartsAlone_KK7`, eşzamanlı `TwoRequestsTakingTheNoticesTogether_TellThemOnce_AndNeitherThrows_KK7`; sayfa bağı D-302 → T346 |
| 8 | Yayın durumu siparişi etkilemez; satış kapatma durumu değiştirmez | ✓ | `AnOpenOrder_IsNotTouchedByAnyMoveOfItsProductOrVariant_KK8` (durum, ödeme, sürüm, tutar, kalem parmak izi aynı), `TemporarilyClosingTheSales_ChangesNoPublishStatus_AndTheListStays_KK8` |
| 9 | Tükenmişlik ayrı durum değil; tükenmiş Yayında ürün listede ve aramada | ✓ | `ASoldOutPublishedProduct_IsInTheSearch_AndInTheList_MarkedSoldOut_KK9`, birim `Visibility_DoesNotLookAtTheStock_…_KK9` |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-A024 (taslak varyant seçenek listesinde yok) | ORTA | yok | BEKLEMEDE | BEKLEMEDE (kanıt türü uç yanıtı; T321'de uç yok — T503/T335) | — |
| VAL-C037 (ürünü üç durum arasında taşır) | ORTA | yok | BEKLEMEDE | BEKLEMEDE (panel + uç kanıtı; T335/T525) | — |
| VAL-C038 (varyantı üç durum arasında taşır) | ORTA | yok | BEKLEMEDE | BEKLEMEDE (panel + uç kanıtı; T335/T525) | — |
| VAL-A021, A022, A023, D001, A177–A180, D025 | ORTA/KRİTİK | yok | BEKLEMEDE | BEKLEMEDE (adres sayfaları, önizleme, kalıcı silme — T335, T549, T363) | — |

Matris betiği `python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77` hata 0 (hücre değişmedi).

#### Doğrulama Kontrol Listesi
- [x] KK1 ürün Yayında iken varyant Taslak olabilir
- [x] KK2 geçişte bildirim yok, iz satırı var
- [x] KK3 taslak ürün listede ve aramada yok; taslak varyant seçenek listesinde yok
- [x] KK4 arşiv ürün listede/aramada yok, kaydı okunur; arşiv varyant vitrinde yok
- [x] KK5 Yayında'ya geçiş kapıyı çağırır; sahte kapının reddi durumu değiştirmez
- [x] KK6 son Yayında varyantın arşive alınması engellenir; ret kodu yönlendirmenin anahtarı (metin T335)
- [x] KK7 arşive alınan ürün sepetten çıkar, bir kez söylenir
- [x] KK8 açık sipariş etkilenmez
- [x] KK9 tükenmiş Yayında ürün aramada görünür
- [ ] `09 §25` — bir "hayır": arayüz metodunda `SaveChanges` (Bulgu 1). Öteki ilgili satırlar evet: durum yalnız geçiş biriminde (tarama + ihlal örneği); sıra yükleme → sürüm → koşul → durum → iç yan etki → iz; olay yayını yok; ret işlemi geri alıyor; `ChangePublishStatus` listede, `Details` ürün adı/stok kodu (kişisel veri yok); eşzamanlılık `RecordSave` ürün sürüm belirteciyle (iki eşzamanlı son-varyant çıkışından ikincisi `RecordVersionConflict`), sepet kilidi `AdvisoryLocks` üzerinden; sayfalı liste `Id` ile biter, `ToPageAsync` + `PageSizes`, sınır testleri var; `TimeProvider`; yeni paket/göç/ayar yok; geniş `catch` yok.
- [x] Güvenlik taraması (aşağıda)

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | çıkış 0 |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata (birleştirme sonrası da 0/0) |
| Format | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` (Web, IntegrationTests, UnitTests) | üçü çıkış 0 |
| Unit Consistency | ✓ | `--filter-trait "Consistency=true"` | 230/230 |
| Unit Architecture | ✓ | `--filter-not-trait Consistency=true --filter-namespace Shopfolio.UnitTests.Architecture` | 906/906 |
| Unit öteki | ✓ | `--filter-not-trait Consistency=true --filter-not-namespace …Architecture` | 2845/2845 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (PostgreSQL 18, `shopfolio-t321-val2-pg`, 127.0.0.1:55680) | ✓ | `ProductTransitionsTests` | 40/40 |
| Integration | ✓ | namespace Catalog + Ordering + Concurrency | 97/97 |
| Integration | ✓ | SitemapEndpoint + SearchEngineData + SchedulerRegistration | 31/31 |
| Integration | ✓ | ShopfolioWebApplicationFactory + ListPaging + VersionedFormPage + SessionAccess + JobQueueRegistration | 77/77 |
| Sonrası | ✓ | `git status` temiz; `dotnet restore --locked-mode` | çıkış 0; konteyner `docker rm -f -v` ile silindi |
| Dal CI | ✓ | `gh pr checks 439` | run 42399951671 (`921491a`): Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate pass |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (diff'te sır, anahtar, bağlantı dizesi yok)
- Auth etkisi: Temiz — yeni uç/sayfa yok; iz yazımı oturumdaki yöneticiyi ister, yönetici yoksa `InvalidOperationException` ve geri alma (test)
- Input validation: Temiz — tek kullanıcı girdisi arama sözcüğü, `SearchTerm.ToLikePattern` + parametreli `LIKE`; `target` tanımsızsa `ArgumentOutOfRangeException`
- Yeni bağımlılık: Yok (kilit dosyaları değişmedi)

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | — (`09 §25` Mimari; KK7'nin yolu) | DÜŞÜK | S1 | Beklenen: arayüz metodu çağıranın işlemine katılır, `SaveChanges` çağırmaz; kaydı işlemin sahibi (`ITransactionRunner`) yapar (`09 §6` m.3, m.7; §9.1 m.3). Gerçekleşen: `IOrderingModule.RemoveFromCartsAsync` (`OrderingModule.cs` satır 65) ve `TakeRemovedItemNoticesAsync` (satır 85) `_db.SaveChangesAsync` çağırıyor. `src/Shopfolio.Web/Modules` altında `SaveChangesAsync` çağıran başka yer yok; komşu arayüz `IAfterSalesModule.ErasePersonalDataAsync` (T301) "saves nothing (09 6 item 3)" der. `RecordSave` çağrısı ayrıdır: `09 §10.2` m.2 kayıt varyantı için onu açıkça buyurur. Düzeltme: iki çağrı kaldırılır (izlenen değişiklikleri çalıştırıcının sonundaki kayıt yazar); `TakeNoticesAsync` test yardımcısı ham işlem yerine `ITransactionRunner.RunAsync` ile çağırır ya da kaydı çağıran yapar; eşzamanlı "bir kez söylenir" testi yeşil kalmalı | KK7, kriter 7 | bu PR'da düzeltilir (eskalasyon kararıyla) | `src/Shopfolio.Web/Modules/Ordering/OrderingModule.cs` · `tests/Shopfolio.IntegrationTests/Catalog/ProductTransitionsTests.cs` (`TakeNoticesAsync`) |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `ShowcaseReader.ListAsync` `Product` varlığını bütün sütunlarıyla okuyup eşliyor; `09 §20` m.3 listede projeksiyon ister. `Paging.ToPageAsync` yalnız `IOrderedQueryable` aldığı için sıralama sonrası `Select` mümkün değil; merge edilmiş `AuditTrail` listesi de aynı kalıpta (emsal) | DEFERRED_BACKLOG'a forward: D-353 → T502 |

#### Yapım Raporu Karşılaştırması
- Uyum: 1 uyuşmazlık
- Kabul kriterleri 1–9 ve KK1–KK9 için öz denetim tablosu var, her madde test adıyla kanıtlı; benim sonuçlarımla aynı (KK6 ileti metni D-304, KK7 sayfa bağı D-302 iki tarafta da aynı okunmuş).
- Uyuşmazlık: öz denetimin `09 §25` satırları modül sınırı sorusunu ("arayüz metodunda `SaveChanges` yok") anmıyor; düzeltme turu notu `TakeRemovedItemNoticesAsync`'in çağıranın işleminde çalıştığını yazıyor ama metodun kendi `SaveChangesAsync`'ini görmüyor (Bulgu 1).
- Yapım raporunun test sayıları (Unit 906 + 3075) birleştirme öncesi; benim koşumum 906 + 230 + 2845 = 3981, aynı toplam.

#### Merge ve Post-merge
- Squash: yok (FAIL)
- Post-merge run'lar: yok

## Düzeltme 2 — tur 2 sonrası

| Bulgu | Düzeltme | Kanıt |
|---|---|---|
| 1 Modül arayüz metotları kendisi `SaveChanges` çağırıyor (`09 §6` m.3, m.7; §9.1 m.3) | `RemoveFromCartsAsync` ve `TakeRemovedItemNoticesAsync`'ten `_db.SaveChangesAsync` kaldırıldı; izlenen değişiklikleri işlemin sahibi yazar: geçiş birimi `ITransactionRunner.RunAsync` içinde çalışır ve çalıştırıcı sonda kaydeder (`TransactionRunner.cs`). Davranış aynı: yayından kaldırmada sepetten çıkarma işaretlenir, bildirim bir kez alınır. `TakeNoticesAsync` test yardımcısı ham işlem yerine `ITransactionRunner.RunAsync` ile çağırır | `TheCartMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK7` (işlem kaydedilmeden tamamlanınca iki metodun değişikliği kalmaz); `TwoRequestsTakingTheNoticesTogether_...KK7` ve öteki KK7 testleri yeşil |

Aynı kalıp taraması: `grep -n SaveChangesAsync src/Shopfolio.Web/Modules` yalnız `CorporateContent/ContentPublishing.cs` (T322, bu dalın dışı) döner; `OrderingModule.cs`'de yok.

### Öz denetim — düzeltme 2 (11'deki kriterlerin ve KK1–KK9'un bütünü, düzeltilmiş diff'te)

| Madde | Sonuç | Kanıt |
|---|---|---|
| 1 / KK1 ürün ve varyant durumu ayrı | ✓ | `EveryMoveBetweenTheThreeStatuses_IsFree_...KK1`, `APublishedProduct_CanHaveADraftVariant_...KK1` (yeşil) |
| 2 / KK2 panel işlemi, bildirim yok, iz var | ✓ | `AMove_WritesOneTrailRow...KK2`, `AMoveWhoseTrailRowCannotBeWritten_...KK2_KK7` (yeşil) |
| 3 / KK3 taslak vitrinde yok | ✓ | `TheList_AndTheSearch_...KK3_KK4`, `TheOptionList_...KK3_KK4` |
| 4 / KK4 arşiv vitrinde yok, kayıt korunur | ✓ | aynı testler, `AVariantArchivedUnderAPublishedProduct_...KK4` |
| 5 / KK5 kapı, red durumu değiştirmez | ✓ | `AGateThatRefuses_...KK5` |
| 6 / KK6 son Yayında varyant engeli | ✓ (panel iletisi D-304) | `TheLastPublishedVariantOfAPublishedProduct_CannotLeavePublished_KK6` |
| 7 / KK7 sepetten çıkar, bir kez söylenir | ✓ | `AProductTakenOffTheShowcase_...KK7`, `TwoRequestsTakingTheNoticesTogether_...KK7`, yeni `TheCartMethods_DoNotSaveOnTheirOwn_...KK7` (KK7 grubu 9/9) |
| 8 / KK8 sipariş etkilenmez | ✓ | `AnOpenOrder_IsNotTouchedBy...KK8`, `TemporarilyClosingTheSales_...KK8` |
| 9 / KK9 tükenmiş görünür | ✓ | `ASoldOutPublishedProduct_...KK9` |
| `09 §25` modül arayüz metodunda `SaveChanges` yok | ✓ | `OrderingModule.cs`'de `SaveChangesAsync` yok (`grep` boş); yeni test |
| `09 §25` sayfalı liste sınır testi, `DbUpdateConcurrencyException` istisnası | ✓ | tur 1 düzeltmeleri korunur (sayfalama testleri yeşil; `OrderingModule.cs`'de yakalama yok) |
| `09 §25` bekçi/tarama | ✓ | Architecture 912/912 |
| Sık FAIL (modül metodunda SaveChanges, T223 r1 / T321 r2) | ✓ | yukarıdaki `grep` |
| Güvenlik | ✓ | yeni uç/girdi yok |

Sınama (düzeltme 2): derleme `-warnaserror` 0 uyarı; `dotnet format` üç proje temiz; Unit Architecture 912, Consistency 241, Catalog + Ordering 99; Integration ProductTransitions grupları (KK7 9, KK1/2/5/6 17, KK3/8/9 + sayfalama 14), ModuleCallTransaction + CartAndOrderCoreTables + Sitemap 78, Ordering + Concurrency 57 — hepsi yeşil.

### Doğrulama Sonucu — T321 Ürün yayın durumları — tur 3
**Tarih:** 2026-11-07   **Branch:** task/T321-urun-yayin-durumlari   **Commit:** `c5b9afd` (doğrulanan head; kayıt commit'i öncesi `origin/main` (T316, #437) birleştirildi)

#### Verdict: ✓ PASS

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un bekçisi) |
| Ana dal CI (son 3) | ✓ | 42408991705 success (T316 #437) · 42405002224 success · 42405001882 success (T328 #440) |
| Memory drift | ✓ | `grep -nE "\bT321\b" .claude/memory/MEMORY.md` → satır 22 |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün ve varyant Taslak/Yayında/Arşiv'i ayrı taşır | ✓ | `Product.ApplyPublishStatus`, `Variant.ApplyPublishStatus`; `ChangeProductStatusAsync` varyantlara dokunmaz, `ChangeVariantStatusAsync` ürünün durumuna dokunmaz. Testler `APublishedProduct_CanHaveADraftVariant_...KK1`, `EveryMoveBetweenTheThreeStatuses_IsFree_...KK1` (6 geçiş × ürün + varyant) yeşil |
| 2 | Bütün geçişler panel işlemi, bildirim yok, iz satırı var | ✓ | Her değişiklikte `IAuditTrail.WriteAsync(ChangePublishStatus, Product/Variant, id, PublishStatus eski→yeni)` aynı işlemde; `Job`/`DispatchRecord` yazımı yok. `AMove_WritesOneTrailRow...KK2` (2 iz satırı, 0 job, 0 dispatch), `AMoveWhoseTrailRowCannotBeWritten_IsRolledBack...` (iz yazılamazsa durum, sürüm ve sepet işareti geri alınır). Yöneticisiz çağrı `InvalidOperationException` |
| 3 | Taslak ürün liste/aramada yok; yayındaki ürünün taslak varyantı seçenek listesinde yok | ✓ | `ShowcaseVisibility.Listed` / `InOptionList`; `ShowcaseReader.ListAsync`, `ListOptionsAsync`; site haritası `Listed()`'a geçti. Birim: her durum ve her ürün×varyant durum çifti; Integration `TheList_AndTheSearch_...KK3_KK4`, `TheOptionList_...KK3_KK4`. Adres yanıtı plan gereği T335 m.6 |
| 4 | Arşiv ürün liste/aramada yok, kaydı korunur; arşivlenmiş varyant vitrinden kalkar | ✓ | Aynı süzgeç; arşiv kaydı okunur (test kaydı okur). `AVariantArchivedUnderAPublishedProduct_LeavesTheOptionList_...KK4`. "Artık satılmıyor" sayfası plan gereği T335 m.7 |
| 5 | Yayında'ya geçiş yayın kapısından geçer, red durumu değiştirmez | ✓ | `AskGateAsync(_gate.Check...)` yalnız hedef Published iken, yazımdan önce. Sahte kapı: `AMoveToPublished_AsksTheGate...KK5`, `AGateThatRefuses_LeavesTheStatusAsItWas_NoRowAndNoVersionRise_KK5` (Draft, Archived); yayından çıkışta kapı sorulmaz. Gerçek koşullar D-300 → T335 |
| 6 | Kapıyı bozan işlem engellenir; panel sebebini söyler, yönlendirir | ✓ | Yayındaki ürünün son Yayında varyantı Draft/Archived'a gidemez → `LastPublishedVariant` (07 §6.2, 422). `TheLastPublishedVariantOfAPublishedProduct_CannotLeavePublished_KK6` (iki hedef), `APublishedVariantMayLeave_...`, `ARefusedMove_LeavesTheCartsAlone_KK7`. Eşzamanlı iki "son varyant" çıkışı ürün sürümü belirteciyle ikincisini reddeder (`RecordSave`). Panel metni: T321'de panel yok, T335 m.9/KK9 sahibi; kayıt D-304 |
| 7 | Vitrinden kalkan kalem sepetten çıkar, bir kez söylenir | ✓ | Ürün ya da varyant Published'tan çıkınca `IOrderingModule.RemoveFromCartsAsync` (`RemovedProductName` işareti; 06 `02 §3.16.7` satırı, K-1044 (3)); `TakeRemovedItemNoticesAsync` danışma kilidi altında adları verir ve siler. Tur 2 bulgusu: iki metotta `SaveChangesAsync` yok (kod okundu); `TheCartMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK7`, `AProductTakenOffTheShowcase_...KK7`, `AVariantTakenOffTheShowcase_...KK7`, `TwoRequestsTakingTheNoticesTogether_TellThemOnce_...KK7` yeşil. Sayfa bağı D-302 → T346 |
| 8 | Yayın durumu siparişi etkilemez; satışın geçici kapatılması durumu değiştirmez | ✓ | Geçiş birimi `Orders`/`OrderItems`'a dokunmaz; `AnOpenOrder_IsNotTouchedByAnyMoveOfItsProductOrVariant_KK8` (parmak izi eşit), `TemporarilyClosingTheSales_ChangesNoPublishStatus_AndTheListStays_KK8` |
| 9 | Tükenmişlik durum değil; tükenmiş Yayında ürün liste/aramada | ✓ | Süzgeç stoka bakmaz; `IsSoldOut` yalnız işaret. Birim `Visibility_DoesNotLookAtTheStock_...KK9`, Integration `ASoldOutPublishedProduct_IsInTheSearch_AndInTheList_MarkedSoldOut_KK9` |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-A024 (taslak varyant seçenek listesinde yok) — kriter 3 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (kanıt türü uç yanıtı; T321 uç yazmaz, modül okuması ✓ — ürün sayfası T503/T335) | — |
| VAL-C037 (ürünü üç durum arasında taşır) — kriter 1, 2 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (veri kaydı ✓; panel/uç kanıtı T335/T525) | — |
| VAL-C038 (varyantı üç durum arasında taşır) — kriter 1, 3 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (aynı gerekçe) | — |
| VAL-A021, A022, A023, D001, A177, A178, A179, A180 (ORTA), D025 (KRİTİK) | — | yok | BEKLEMEDE | BEKLEMEDE (adres sayfaları, önizleme, kalıcı silme — T335, T549, T363; T321 kriterlerine bağlı değil) | — |

KP-7 ve KP-40 birden çok task'a bölünmüş (`11 §7.1`: KP-7 → T321, T335, T363, T503, T504, T549; KP-40 → T321, T335, T525); T321 kriterleri adres ve sayfa yanıtını açıkça T335'e bırakır. Matris betiği `12_matris_denetle.py --kapsam 1-77` hata 0 (hücre değişmedi).

#### Doğrulama Kontrol Listesi
- [x] KK1: ürün Yayında iken varyantı Taslak olabilir
- [x] KK2: geçişte bildirim yok, iz satırı var
- [x] KK3: taslak ürün listede ve aramada yok; taslak varyant seçenek listesinde yok
- [x] KK4: arşiv ürün listede ve aramada yok, kaydı okunur; arşivlenmiş varyant vitrinde yok
- [x] KK5: Yayında'ya geçiş kapıyı çağırır; sahte kapının reddi durumu değiştirmez
- [x] KK6: son Yayında varyantın arşive alınması engellenir; kod yönlendiren ret (`LastPublishedVariant`), metin T335 (D-304)
- [x] KK7: arşive alınan ürün sepetten çıkar, bir kez söylenir
- [x] KK8: açık sipariş yayın durumundan etkilenmez
- [x] KK9: tükenmiş Yayında ürün aramada görünür
- [x] `09 §25`: adlar 07/06'dan (`ChangePublishStatus`, `PublishGateIncomplete`, `LastPublishedVariant`, `RecordVersionConflict`, `RemovedProductName`); diff kapsamda; yeni ayar yok; modül sınırı — başvurular yalnız `Contracts` (`IOrderingModule`, `IAuditTrail`), Architecture testleri yeşil; arayüz metodunda doğrudan `SaveChanges` yok (`OrderingModule` iki metodu yalnız izlenen değişiklik bırakır; `ProductTransitions` kaydı `09 §10.2` m.2'nin kayıt varyantı kalıbıyla, `RecordSave.SaveAsync` üzerinden ve çağıranın açık işlemi içinde — `RequireTransaction`); zaman `TimeProvider`; yeni sayfalı liste `Id` ile biten sıra + `ToPageAsync` + `PageSizes` + sınır testleri; tek yazıcı taraması (`ApplyPublishStatus`/`MarkFirstOnSale` yalnız geçiş biriminde) birim testiyle; olay yayını, `Job`, göç, yeni paket yok; `DbUpdateConcurrencyException` yalnız `RecordSave`'de
- [x] Güvenlik taraması (aşağıda)

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx`; sonda `--locked-mode` | çıkış 0 |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata (birleştirme sonrası yeniden: 0/0) |
| Format | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 | üçü çıkış 0 |
| Unit (Architecture + Consistency dahil) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 4062/4062; `origin/main` birleştirmesi sonrası 4070/4070 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 dosya |
| Integration | ✓ | `--filter-class ...Catalog.ProductTransitionsTests` | 41/41 |
| Integration | ✓ | Sitemap + SearchEngineData + SchedulerRegistration + ContentStatus | 65/65 |
| Integration | ✓ | Ordering.OrderTransitionsTests + Concurrency.ConcurrencyTests | 57/57 |
| Dal CI | ✓ | `gh pr checks 439`; `gh run list --branch task/T321-urun-yayin-durumlari` | 42406697716 success (head `c5b9afd`; Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate pass) |

Konteyner `shopfolio-t321-val3-pg` (postgres:18, 127.0.0.1:55821) koşudan sonra `docker rm -f -v` ile silindi; `git status` temiz.

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (yeni yapılandırma, sır ya da kayıt şablonu yok)
- Auth etkisi: Temiz (yeni uç yok; iz yazımı oturumdaki yöneticiyi ister, yoksa istisna)
- Input validation: Temiz (`RequireDefined(target)`; arama kelimesi `SearchTerm.ToLikePattern` ile kaçışlı `LIKE`; sürüm karşılaştırması)
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| — | — | — | — | Bulgu yok | — | — | — |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| — | Yeni K-notu yok. Açık devirler doğrulandı: D-300 (gerçek kapı, T335), D-301 (public ayarlayıcılar, T335), D-302 (bir kez söylenen iletinin sayfa bağı, T346), D-303 (kalıcı silme, siparişe girememe; T335/T346), D-304 (panel metni, T335), D-353 (liste projeksiyonu, T502) | — |

#### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu (kriter 1–9 ve KK1–KK9 düzeltme 2 öz denetim tablosunda test adıyla kanıtlı; benim sonuçlarımla aynı; KK6 metni D-304 ve KK7 sayfa bağı D-302 iki tarafta da aynı biçimde).
- Not: yapım raporunun "aynı kalıp taraması" `SaveChangesAsync`'i yalnız metin olarak arar; `ProductTransitions`'ın `RecordSave.SaveAsync` üzerinden kaydı bu taramada görünmez. Doğrulayıcı bunu `09 §10.2` m.2'nin belgede yazılı kayıt varyantı kalıbı (çağıranın işlemi içinde, kesinleştirmeden) olarak uyumlu saydı; bulgu değildir.

#### Merge ve Post-merge
- Squash: yardımcıda (doğrulama ajanı 16. adımda durur)
- Post-merge run'lar: yardımcıda

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
