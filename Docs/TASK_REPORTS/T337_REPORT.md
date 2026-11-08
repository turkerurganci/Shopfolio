# T337 — İndirim ve referans fiyat: tarihli indirim, fiyat geçmişinden referans, kaydı engelleyen koşullar

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1: PASS) | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Saf kurallar:** `DiscountRules` — yüzde (0 < oran < 100, en çok iki ondalık) ve tarih aralığı denetimi, indirimli fiyat (`Money.Round`), sıfıra inme denetimi, başlangıç (`TurkeyTime.StartOf(StartDate)`) ve bitiş (`TurkeyTime.EndOfRange(EndDate)` = bitişten sonraki günün 00.00'ı) anları, durum (başlamadı / sürüyor / bitti), uygulanan fiyat, sınırların borcu (`OwedBounds`: tanımdan ve varyantın oluşmasından sonraki, son satırdan yeni sınırlar), beyan satırı (`LineOf`). `ReferencePriceRules` — ürün kuralı (fiziksel/dijital ve son kullanma tarihli mal: 10 günün en düşüğü; hizmet ve çabuk bozulan: hemen önceki fiyat), pencere `LookbackDays = 10` (Z-21), 10 günden kısa geçmişte varyantın satışa sunulduğu andan beri en düşük.
- **Fiyat geçmişinin tek yazıcısı `PriceHistoryRecorder`:** iş biriminin bıraktığı fiyata göre önce saklı indirimin borçlu olduğu sınırları kendi anlarıyla (geç gelse de), sonra uygulanan fiyat son satırdan farklıysa `now` satırını ekler; uygulanan fiyatı değiştirmeyen işlem satır yazmaz. Tek anda tek fiyat: `now`'a düşen sınır `now` satırına bırakılır (aynı anda iki satır yazılmaz).
- **Yazıcılar:** `Discounts` (Save/Remove/Get) — oran ve aralık denetimi, sıfıra inen varyant kayıttan önce reddedilir, indirim satırı yerine geçer (sil + ekle; `CreatedAt` tanım anı), yazıcı çağrılır, her varyant kendi referansıyla denetlenir; ret iş biriminden bu çağrının eklediklerini çıkarır (yine de kaydeden çağıran hiçbir şey yazmaz). `VariantPrices.ChangeAsync` — fiyat denetimi, indirim bitmemişse (başlamamış ya da süren) sıfıra indiren fiyatın reddi, aynı fiyat `Unchanged`. Yazıcılar çağıranın işlemine katılır, işlem dışında atar, `SaveChanges` çağırmaz (`09 §9.1` madde 3).
- **Zamanlayıcı:** `DiscountBoundarySource : IDueRecordSource` (`AddDueRecordSource` ile kayıtlı) — zaman indirimin kendi tarihlerinden, borç varyantın son satırından; işleme kendi işleminde (`ITransactionRunner`) yazıcıyı çağırır.
- **Bekçi:** `CatalogRecordsGuard` güncellenen `Discount` satırını SQL'den önce reddeder (`06 §4.12`: yeni tanım yerine geçer).
- **Okumalar:** `PriceHistoryReads` — çok soruluk referans okuması (`ReferencesAsync(ReferenceAsk[])`, sabit sorgu sayısı), süren indirimler, varyantların iş biriminin bıraktığı hâli. `PriceLabelReader` etiket indirim satırını (indirimli fiyat, referans, yüzde, iki tarih) taşır ve birim fiyatı uygulanan fiyattan türetir (D-345). `ProductListReader(db, TimeProvider)` kartta en düşük liste fiyatlı yayındaki varyantın indirim satırını üç sabit sorguyla okur (K-1578).
- **Sözleşme:** `ICatalogModule` `SaveDiscountAsync`, `RemoveDiscountAsync`, `GetDiscountAsync`, `ChangeVariantPriceAsync`; `DiscountLine`, `DiscountView`, `DiscountVariantLine`, `DiscountSaveResult`, `DiscountRemoveResult`, `VariantPriceResult`, `DiscountState`; `PriceLabel.Discount`, `ProductListItem.Discount`. Referans parametresi ya da sütunu yoktur.
- **Testler:** Unit `Catalog/DiscountRulesTests`, `Catalog/ReferencePriceRulesTests`; Integration `Model/DiscountAndReferenceTests` (20 test, kendi koleksiyonu). `CategoryTreeAndListsTests` ve `SitemapSourceTests` (sahte Katalog) yeni imzalara uyarlandı.
- **Belgeler:** K-1578, backlog D-330 (kart yarısı kapandı, süzgeç yarısı → T502), D-345 ✅, yeni D-475–D-477; bu rapor, STATUS satırı, `STATUS_CHANGELOG.d/T337.md`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `DiscountRules.cs`, `ReferencePriceRules.cs`, `PriceHistoryRecorder.cs`, `PriceHistoryReads.cs`, `Discounts.cs`, `VariantPrices.cs`, `DiscountBoundarySource.cs`, `CatalogRecordsGuard.cs`, `PriceLabelReader.cs`, `ProductListReader.cs`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs`, `Contracts/Pricing/DiscountRecords.cs`, `Contracts/Pricing/DiscountState.cs`, `Contracts/Pricing/PriceLabel.cs`, `Contracts/Lists/ProductListItem.cs`
- `tests/Shopfolio.UnitTests/`: `Catalog/DiscountRulesTests.cs`, `Catalog/ReferencePriceRulesTests.cs`, `Infrastructure/SitemapSourceTests.cs`
- `tests/Shopfolio.IntegrationTests/`: `Model/DiscountAndReferenceTests.cs` (yeni), `Model/CategoryTreeAndListsTests.cs`
- Belgeler: `PRODUCT_DISCOVERY_STATUS.md` (K-1578), `DEFERRED_BACKLOG.md`, `IMPLEMENTATION_STATUS.md` (T337 satırı), `STATUS_CHANGELOG.d/T337.md`, bu rapor

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İndirim ürün düzleminde yüzde, her varyanta kendi fiyatından iner | ✓ | `Discounts.SaveAsync` (ürün başına tek satır, `DiscountRules.DiscountedPrice(variant.Price, …)`); Integration `ADiscount_ComesDownToEveryVariantOnItsOwnPrice_AndItsAppliedPriceIsWrittenToTheHistory` (1000→800, 1250→1000) |
| 2 | Sıfıra inmez; %100 girilemez; sıfıra yuvarlanan indirim ve yürüyen indirimde sıfıra indiren fiyat reddedilir | ✓ | `DiscountRules.CheckTerms`, `ReachesZero`; `VariantPrices.ChangeAsync`; Unit `ARateOutsideTheRule_IsAProgrammingErrorInTheCalculation`, `ADiscountedPriceRoundedToZero_IsRefusedBeforeTheReference`, `APriceThatTheDiscountWouldBringToZero_IsTold`; Integration `AHundredPercent_AndADiscountRoundedToZero_AreRefused_AndWriteNothingEvenWhenTheCallerSaves`, `WhileTheDiscountHasNotEnded_APriceItWouldBringToZeroIsRefused` |
| 3 | Başlangıç günü 00.00'da başlar, bitiş günü 23.59'da biter | ✓ | `DiscountRules.StartsAt`/`EndsAt`/`StateAt`; Unit `TheRange_StartsAtMidnightOfTheStartDay_AndEndsAfterTheWholeEndDay`, `TheState_IsReadFromTheDates_AtTheTwoBounds`; Integration `TheScheduler_OpensTheDiscountAtMidnightOfItsStartDay_AndClosesItAfterItsEndDay_EvenWhenLate` (23.59'da sürüyor, ertesi 00.00'da kapanır, 6 saat geç gelen zamanlayıcı satırı 00.00'a yazar) |
| 4 | Beyan gösterilen her yerde başlangıç ve bitiş tarihi görünür | ✓ | Sözleşme düzleminde: `DiscountLine(DiscountedPrice, ReferencePrice, Percentage, StartDate, EndDate)` hem etikette (`PriceLabel.Discount`) hem kartta (`ProductListItem.Discount`); Integration `TheRunningDiscountsDates_AreOnTheLabelAndOnTheCard_TheCardDeclaringTheLowestPricedVariant`. Bu sözleşmeleri gösteren sayfa henüz yok — gösterim T502/T503'te (D-477) |
| 5 | Referansı sistem hesaplar, yönetici giremez; her varyantın geçmişi tutulur | ✓ | `ReferencePriceRules` + `PriceHistoryReads.ReferencesAsync`; tek yazıcı `PriceHistoryRecorder`; Integration `TheReference_HasNoColumn_AndTheSaveTakesNoReference` (sütun yok, `SaveDiscountAsync` parametrelerinde referans yok) |
| 6 | Mal: 10 günün en düşüğü (kapalı aralık dahil, indirimli fiyat da geçmişe yazılır); hizmet ve çabuk bozulan: hemen önceki | ✓ | Unit `OnGoods_TheReference_IsTheLowestPriceAppliedInTheWindow_ThePriceInForceAtItsOpeningIncluded`, `ThePriceOfAStretchOffSale_Counts_TheRuleDoesNotLookAtThePublicationStatus`, `AnEarlierDiscountedPrice_IsAnAppliedPriceAndLowersTheReference`, `AnExpiryDatedProduct_TakesTheWindowsLowest_APerishableOneThePriceRightBefore`, `ImmediatelyBefore_IsTheLastPriceBeforeTheStart_AndRowsFromTheStartOnAreIgnored`; Integration `OnGoods_APriceSetWhileOffSale_IsInTheWindowAndBecomesTheReference`, `AnEarlierCampaignsDiscountedPrice_LowersTheReference`, `TheReference_FollowsTheProductsRule` (4 durum) |
| 7 | 10 günlük geçmişi olmayan malda satışa sunulduğu andan beri en düşük; öteki varyantlar referans olmaz | ✓ | Unit `AVariantOnSaleForLessThanTheWindow_TakesTheLowestSinceItWentOnSale`, `AVariantWithoutARowBeforeTheStart_TakesItsOwnPrice`, `AVariantNeverOnSaleBeforeTheStart_TakesThePriceRightBefore`; Integration `ANewVariant_TakesItsReferenceFromItsOwnHistory_NotFromItsSiblings` |
| 8 | İndirimli fiyat referansın altına inmiyorsa kaydedilmez, sebebi söylenir | ✓ | `DiscountSaveResult.NotBelowReference(variantId, discountedPrice, referencePrice)`; Integration `ARefusedDiscount_LeavesTheEarlierDiscountAndTheHistoryAsTheyWere`, `OnGoods_APriceSetWhileOffSale_IsInTheWindowAndBecomesTheReference` (75 ≥ 70 reddi) |
| 9 | Beyan edilen referans sipariş kalemine T355'te donar | ✓ | Kriter T355'e bağlı: T337 donacak değeri `DiscountLine.ReferencePrice` olarak etikette taşır; donma ve sınaması T355 KK1 (D-476) |

### Öz denetim (bitiş kapısı 2)

> Önceki iki yapım turunun işi dahil bütün maddeler bu turda diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 her varyant kendi fiyatından | ✓ | `Discounts.cs` (`CheckVariant(variant.Price, …)`); `ADiscount_ComesDownToEveryVariantOnItsOwnPrice_…` |
| KK2 %100 ve sıfıra yuvarlanan ret; yürüyen indirimde sıfıra indiren fiyat | ✓ | `DiscountRules.CheckTerms`/`ReachesZero`; `VariantPrices.ChangeAsync`; `AHundredPercent_AndADiscountRoundedToZero_…`, `WhileTheDiscountHasNotEnded_…` (başlamamış ve süren indirimde ret, bitince serbest) |
| KK3 00.00 açılış, 23.59 sonrası kapanış | ✓ | `TheRange_StartsAtMidnight…`, `TheScheduler_OpensTheDiscountAtMidnight…` |
| KK4 tarihler ürün sayfasında ve kartta | ✓ | `PriceLabel.Discount`, `ProductListItem.Discount`; `TheRunningDiscountsDates_AreOnTheLabelAndOnTheCard_…`; sayfa gösterimi D-477 |
| KK5 referans panelde girilemez | ✓ | `TheReference_HasNoColumn_AndTheSaveTakesNoReference` |
| KK6 10 günün en düşüğü (kapalı aralık, önceki indirimli, son kullanma tarihli), hizmette/çabuk bozulanda hemen önceki | ✓ | Kriter 6 satırındaki testler |
| KK7 yeni varyant kendi geçmişinden | ✓ | `ANewVariant_TakesItsReferenceFromItsOwnHistory_NotFromItsSiblings` |
| KK8 referansın altına inmeyen indirim kaydedilmez | ✓ | `ARefusedDiscount_LeavesTheEarlierDiscountAndTheHistoryAsTheyWere` (önceki satır ve geçmiş aynı kalır, çağıran kaydetse de) |
| KK9 donma T355 KK1'de | ✓ | D-476 |
| `09 §25` doküman uyumu | ✓ | Okunan: çekirdek, `09` §10, §14, §13; `02 §3.9.1`…§3.9.6, §4.1.4, Z-20, Z-21, §5.11.1, §6.6, §12.1.6; `06 §4.11`, §4.12; `10` KP-5, KP-43. Adlar `06`/sözlükten (`Discount`, `PriceHistory`, `ReferencePrice`). Kapsam dışı davranış yok |
| `09 §25` §3.5 tablosu | ✓ | Yeni ayar/tavan yok. `LookbackDays = 10` Z-21'in yasal süresidir, kılavuzun tasarım değeri değil; kuralıyla `ReferencePriceRules`'ta yaşar (Z-44 imha süresinin `RecordTablesDisposalParts`'taki emsali) — ⚠ 5 |
| `09 §25` mimari | ✓ | Modül sınırı: yalnız `Contracts`; arayüz yöntemi işlem açmıyor/kaydetmiyor (`RequireTransaction`, `TheWriters_JoinTheCallersTransaction_AndThrowOutsideOne`); Unit Architecture yeşil |
| `09 §25` zaman ve para | ✓ | Saat yalnız `TimeProvider`; diff'te `DateTime.Now/UtcNow` yok; yuvarlama `Money.Round` |
| `09 §25` zamanlayıcı | ✓ | `AddDueRecordSource<DiscountBoundarySource>()` (`CatalogModule.cs`); zaman indirimin kendi tarih alanlarında |
| `09 §25` veri/göç | ✓ | Göç yok (tablolar önceki task'larda); FK/varsayılan değişmedi |
| `09 §25` performans | ✓ | Kart indirimleri sayfa doluluğundan bağımsız sabit sorgu: `TheCardsDiscounts_AreReadInAFixedNumberOfQueries_WhateverThePageHolds`; okumalar izlemesiz ve projeksiyonlu |
| `09 §25` güvenlik | ✓ | Yeni uç, sayfa, POST, sır, paket yok; kişisel veri yok |
| Sık FAIL 1 — bekçi/tarama istisnası geniş | ✓ | Yeni tarama yok; bekçi `Modified Discount`'u koşulsuz reddeder (istisnasız) |
| Sık FAIL 2 — tarama çağrı biçimi kaçırıyor | ✓ | Yeni kaynak taraması yok |
| Sık FAIL 3 — 06 varsayılanı / FK dizini | ✓ | Şema değişmedi |
| Sık FAIL 4 — aynı sayıyı anan öteki yerler | ✓ | 10 gün tek sabitte (`LookbackDays`); D-330/D-345 metinleri ve hedefleri güncel; K-1578 koddaki atıflarla eşleşir |
| Sık FAIL 5 — 09 metni karar kaydısız | ✓ | 09 değişmedi; kart/süzgeç kararı K-1578 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --filter-not-trait "Consistency=true"` | 4192/4192 |
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --filter-trait "Consistency=true"` | 242/242 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests` (yerel PostgreSQL 18) | 2499/2499 (DiscountAndReference 20/20) |
| Format / Release | ✓ | `dotnet format --verify-no-changes`; `dotnet build -c Release -warnaserror` | temiz; 0 uyarı |
| Contract | — | — | Uç yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-08; head `7006bcc`, `origin/main` `e7f6ba0` dalın tabanında — birleştirme gerekmedi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — D-487, D-488 (backlog'a forward) |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = ana checkout'un `scripts/git-hooks`'u; ana dal CI son üç tamamlanmış run success — 42452259880 ve 42452259999 (`e7f6ba0`, T324 #454), 42450627470 (`ea62688`, chore #450).

**Kabul kriterleri (validator kanıtı):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | Ürün başına tek `discounts` satırı (oran + iki tarih); `Discounts.SaveAsync` her varyantı `DiscountRules.DiscountedPrice(variant.Price, oran)` ile kendi fiyatından denetler, `PriceHistoryRecorder` her varyanta kendi uygulanan fiyatını yazar; `ADiscount_ComesDownToEveryVariantOnItsOwnPrice_…` (1000 → 800, 1250 → 1000) |
| 2 | ✓ | Oran kuralı `InvalidPercentageAttribute` (0 < oran < 100, en çok iki ondalık; `FieldRules.cs`) — 100 ve 0 red (`TheRate_IsAboveZeroAndBelowOneHundred_…`); sıfıra yuvarlanan varyant kayıttan önce `DiscountedPriceZero` (0,01 × 0,40 = 0,004 → 0,00); `VariantPrices.ChangeAsync` indirim bitmemişken sıfıra indiren fiyatı reddeder, bitince serbest (`WhileTheDiscountHasNotEnded_…`); ret, çağıran kaydetse de satır bırakmaz |
| 3 | ✓ | `StartsAt` = `TurkeyTime.StartOf(StartDate)`, `EndsAt` = `TurkeyTime.EndOfRange(EndDate)` (T378 yardımcısı; `09 §7.1` (3) — 23:59 karşılaştırması yok); `TheScheduler_OpensTheDiscountAtMidnight…`: başlangıçtan bir tik önce borç yok, 00.00'da var; 6 saat geç gelen zamanlayıcı satırı 00.00'a yazar; bitiş günü 23:59'da hâlâ indirimli, ertesi 00.00'da kapanır ve liste fiyatı o anla yazılır (`02 §4.1.4`, K-1559) |
| 4 | ✓ | Sözleşme düzleminde: `DiscountLine(…, StartDate, EndDate)` etikette (`PriceLabel.Discount`) ve kartta (`ProductListItem.Discount`), yalnız indirim sürerken; `TheRunningDiscountsDates_AreOnTheLabelAndOnTheCard_…` (başlamadan önce null). Sayfalar `11`'de T502/T503'ün (T503 kaynakları `02 §3.9.2`'yi anar) — D-477 |
| 5 | ✓ | `SaveDiscountAsync(productId, percentage, startDate, endDate, ct)` referans almaz; `products`, `variants`, `discounts`, `price_histories`'te `%reference%` sütunu yok (`TheReference_HasNoColumn_…`); referans her okumada geçmişten hesaplanır (`PriceHistoryReads.ReferencesAsync`); geçmişin tek yazıcısı `PriceHistoryRecorder`, satır güncellenmez (`CatalogRecordsGuard`) |
| 6 | ✓ | `ReferencePriceRules.RuleOf`: hizmet ve `Perishable` → hemen önceki; fiziksel, dijital ve `ExpiryDated` → 10 günün en düşüğü (pencere açılışında yürürlükteki fiyat dahil, yayın durumuna bakılmaz); `OnGoods_APriceSetWhileOffSale_…` (taslakta 70 → referans 70; %25 = 75 red, %31 = 69 kabul), `AnEarlierCampaignsDiscountedPrice_LowersTheReference` (dijital; 85 ≥ 80 red), `TheReference_FollowsTheProductsRule` (4 durum: 80 / 80 / 100 / 100) |
| 7 | ✓ | Pencere `max(FirstOnSaleAt, başlangıç − 10 gün)`ten başlar, yalnız varyantın kendi satırları okunur; `AVariantOnSaleForLessThanTheWindow_…`, `ANewVariant_TakesItsReferenceFromItsOwnHistory_NotFromItsSiblings` (kardeşi 50, yeni varyant 100) |
| 8 | ✓ | `CheckVariant`: indirimli ≥ referans → `NotBelowReference(variantId, indirimli, referans)`; `Undo` bu çağrının eklediği satırları ve indirimi çıkarır, eski satır kalır (`ARefusedDiscount_LeavesTheEarlierDiscountAndTheHistoryAsTheyWere`) |
| 9 | ✓ | Kriter T355'e bağlı (KK9: "T355 KK1'de sınanır"); T337'nin payı donacak değerin (`DiscountLine.ReferencePrice`) etikette taşınması — D-476 |

**Elle sınama (para):** 1000 × 0,80 = 800,00; 1250 × 0,80 = 1000,00; 0,01 × 0,40 = 0,004 → 0,00 (red), 0,02 × 0,40 = 0,008 → 0,01; 45 × 0,80 = 36,00 → 36 × 1000 / 250 = 144,00 TL/kg; 100 × 0,75 = 75 ≥ 70 (red), 100 × 0,69 = 69 < 70; 100 × 0,85 = 85 ≥ 80 (red), 100 × 0,79 = 79 < 80 — testlerin beklentileriyle birebir.

**Task satırları (`12 §7.5`):** VAL-A018, VAL-A191, A192, A193, A194, A195 `BEKLEMEDE` kalır — kanıt türleri uç yanıtı / tarayıcı gözlemidir; KP-5'in ve KP-43'ün satırlarını kapatan task'lar T503 ve T525'tir (`11 §7`); T337 modül düzeyindeki ön koşulları kurar ve sınar. `12_matris_denetle.py --kapsam 1-77`: hata 0.

**Testler (head `7006bcc`):** `dotnet restore` temiz; `dotnet build -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç proje temiz; Unit 4434 / 4434 (Architecture ve Consistency dahil); betik testleri 4 / 4; Integration (yerel PostgreSQL 18, parçalı) 87 / 87 (DiscountAndReference 20, CategoryTreeAndLists, PriceAndVat, ListPaging) + 118 / 118 (Jobs ve Catalog ad alanları — zamanlayıcıya yeni kaynak) + 68 / 68 (StockAndCapacity, PaymentWindow, Concurrency). CI (#457, run 42453730158): Lint, Build, Unit, Contract, Migration dry-run, Paketleme, E2E pass; Integration doğrulama anında koşuyordu.

**`09 §25`:** doküman uyumu ✓ (adlar `06 §4.11`, §4.12; tek kural evleri `Money.Round`, `TurkeyTime`, `InvalidPercentageAttribute`); kapsam ✓ (uç, sayfa, şema, paket, ayar yok); modül sınırı ✓ (yazıcılar çağıranın işlemine katılır, `SaveChanges` yok, işlem dışında atar); zaman yalnız `TimeProvider` ✓; zamanlayıcı kaynağı `AddDueRecordSource` ile ✓; kart indirimleri sabit sorgu sayısıyla ✓. Ret sebebi adlarından ikisi `07 §6.2` koduyla birebir değil — galeri emsaliyle bloke etmez, D-487.

**Güvenlik:** sır yok; yeni uç / yetki yüzeyi yok; girdi (oran, tarih, fiyat) modülde alan kurallarıyla denetlenir; ham SQL yok; yeni bağımlılık yok.

**Bloke-etmeyen notlar:** K1 → D-487 (iki ret sebebi adı `07` koduna T525'te eşlenir); K2 → D-488 (indirim kaydından sonra fiyat artışı beyanı referansın üstüne çıkarabilir; `02 §3.9.5` yalnız kayıtta denetler — karar, T525).

**Yapım raporu karşılaştırması:** tam uyumlu; öz denetim tablosu kriter ve KK başına kanıtlı. Validator'ın iki K-notu yapım raporunda yok (yeni gözlem).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`discounts`, `price_histories` önceki task'larda kuruldu)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T337-indirim-referans-fiyat`
- **Commit:** PR'ın head'i
- **PR:** #457
- **CI:** run 42453730158 (`7006bcc`) doğrulama anında Integration koşuyordu, öteki işler pass; doğrulama commit'inin CI'ını yardımcı izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Referans penceresi 10 gündür (Fiyat Etiketi Yönetmeliği m.11/1, RG 11/10/2025-33044) | `02 §3.9.3`, Z-21 (belgeye karşı; K-431'de doğrulanmış) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Fiyat aralığı süzgecinin indirimli fiyatla çalışması | D-330 → T502 | Süzme ekseni ve dizini vitrin sayfasıyla gelir (K-1578) |
| 2 | Ürün formu varyant oluşturma/fiyat yazımında yazıcıyı çağırır, sürümü artırır | D-475 → T525 | Ürün formu yok |
| 3 | Referans ve indirim satırının sipariş kalemine donması | D-476 → T355 | Kriter 9'un kendisi |
| 4 | Tarihlerin vitrin sayfalarında gösterimi | D-477 → T502/T503 | Sayfalar yok |

## Notlar

- **Working tree (Adım -1):** temiz (üç yapım turunun her birinde)
- **Ana dal CI startup (Adım 0):** tur 1'de geçti
- **Repo memory (bitiş kapısı 9):** kayıt kuralı (#450) gereği MEMORY "Güncel Durum"a dokunulmadı
- **⚠ Kararlar (kolay, kurala en uygun seçenekle verildi):**
  1. İndirim satırı güncellenmez, yerine geçer (sil + ekle; `CreatedAt` = tanım anı, sınırların "tanımdan sonra" koşulu onu okur); bekçi `Modified` satırı reddeder. Toplu `ExecuteUpdate` yolu için ayrı tarama eklenmedi (fiyat geçmişi bekçisinin var olan kalıbı).
  2. Sıfır kontrolü: varyant fiyatı değişiminde başlamamış ve süren indirim için uygulanır (bitmiş indirimde fiyat serbest).
  3. Eşzamanlı sınır yazımı (zamanlayıcı + yönetici) en çok aynı satırı (aynı an, aynı fiyat) iki kez yazar; referans (en düşük) ve etiket tarihi etkilenmez → kilit eklenmedi.
  4. Bu iş birimiyle eklenen ve sınırdan sonra oluşturulan varyant sınır borçlanmaz (`since = max(DefinedAt, variant.CreatedAt)`).
  5. Pencere sabiti `LookbackDays = 10` yasal süredir (Z-21), `09 §3.5` tablosunda değil, kuralıyla yaşar.
  6. Yazıcılar ürün sürümünü artırmaz; formun sahibi (T525) bir kez artırır (galeri kalıbı; D-475).
  7. Tek anda tek fiyat: yazıcı `now`'a düşen sınır satırını yazmaz, `now` satırı iş biriminin bıraktığı fiyatı taşır (aynı `StartedAt`'lı iki satırda sıra UUIDv7 ile ms içinde belirsizdi).
  8. Zamanlayıcı kaynağı her taramada başlamış indirimleri (ürün başına en çok bir satır) ve varyantlarını iki sorguyla okur; küme ürün sayısıyla sınırlıdır, sayfalanmadı.
