# T337 — İndirim ve referans fiyat: tarihli indirim, fiyat geçmişinden referans, kaydı engelleyen koşullar

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-08

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
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`discounts`, `price_histories` önceki task'larda kuruldu)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T337-indirim-referans-fiyat`
- **Commit:** PR'ın head'i
- **PR:** *(PR açılınca)*
- **CI:** *(validator doldurur)*
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
