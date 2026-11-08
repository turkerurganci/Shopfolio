# T346 — Sepet: sahiplik, giriş anında birleşme, canlı kalem, siparişe girmeyen kalemler

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Sepet işlemleri `IOrderingModule`'e eklendi: `OpenCartAsync` (sayfanın canlı okuması), `AddToCartAsync`, `MergeGuestCartAsync`. Üçü de çağıranın işlemine katılır, sepet kilidini en başta alır ve kaydetmez (`09 §10.2` m.5, `09 §6` m.3).
- `CartOwner` (üye: hesap kimliği; misafir: tarayıcı çerezindeki ham anahtar, yalnız özeti saklanır, `ToString` anahtarı yazmaz), `CartLine`, `CartView`, `CartAddResult`, `CartMergeResult`, `CartAddOutcome`, `CartPaymentBlock` sözleşmeleri.
- `CartRules` (saf kurallar): `PriceChanged` (yönsüz tek işaret), `EntersOrder`, `PaymentBlockOf`, `OrderableSubtotal`, `MergeShowsNew`.
- `Carts` (işlemler) ve `CartStatements` (eşzamanlı yazılan satırlar için tek `ON CONFLICT` deyimleri): sepet bir kez yapılır, aynı varyant ikinci kez eklenince yalnız adet artar (ilk fiyat ve an kalır), birleşmede büyük adet kalır, önce eklenen satır fiyatı ve anı belirler.
- Katalog: `ICatalogModule.ListCartVariantFactsAsync` + `CartVariantReader` (güncel fiyat, indirim dahil; vitrinde mi; Tükendi mi; stok sayısı taşımaz).
- Görülmeyen ekleme `IsMergeNoticePending` ile işaretlenir, `OpenCartAsync`'te bir kez söylenir. Vitrinden kalkan kalem `OpenCartAsync`'te bir kez söylenip silinir (`TakeRemovedNamesAsync`, T321'in `TakeRemovedItemNoticesAsync`'i buna devredildi).
- Bu turda: test projeleri derlendi, 16 Unit + 19 Integration test yazıldı; `CartAddOutcome` ve `CartPaymentBlock` kendi dosyalarına alındı ve `ApplicationEnums.NotStored`'a eklendi (saklanmayan sonuç türü); ham SQL `CartStatements.cs`'e toplandı ve `VersionedSideBulkWriteScan` kapalı yerler listesine (test dosyayı dört deyim, yalnız `carts`/`cart_items` ve `UPDATE`/`DELETE` yok olarak bağlar) alındı; belgeler.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/`: `Carts.cs`, `CartStatements.cs`, `CartRules.cs`, `OrderingModule.cs`; `Contracts/`: `CartContracts.cs`, `CartAddOutcome.cs`, `CartPaymentBlock.cs`, `IOrderingModule.cs`.
- `src/Shopfolio.Web/Modules/Catalog/`: `CartVariantReader.cs`, `CatalogModule.cs`; `Contracts/`: `CartVariantFact.cs`, `ICatalogModule.cs`.
- `tests/Shopfolio.UnitTests/`: `Ordering/CartRulesTests.cs`, `Architecture/VersionedSideBulkWriteScan(.Tests).cs`, `Support/ApplicationEnums.cs`, `Infrastructure/SitemapSourceTests.cs`.
- `tests/Shopfolio.IntegrationTests/Ordering/CartTests.cs`.
- Kayıt: `Docs/STATUS_CHANGELOG.d/T346.md`, `Docs/DEFERRED_BACKLOG.d/T346.md` (D-558–D-562), `Docs/DEFERRED_BACKLOG.md` (D-303 yerinde kapandı), `Docs/IMPLEMENTATION_STATUS.md` (tablo satırı).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üyenin sepeti hesapta, her cihazda aynı; misafirin sepeti tarayıcıya bağlı, üye çıkınca o tarayıcıda boş | ✓ | `CartTests.AMembersCart_IsTheSameInEveryBrowser_AndTheGuestKeyIsEmptyAfterTheSignIn_KK1`, `TwoGuestBrowsers_HaveTheirOwnCarts_AndAMemberAddsToTheAccountCart_KK1`; `Carts.FindCartIdAsync` |
| 2 | Sepet kendiliğinden boşalmaz, süre parametresi yok; yarım ödeme, kesinti, kapalı satışta durur | ✓ | `CartTests.ACartThatWasLeftForTwoYears_AndAnOpenOrderOfTheCart_AreNotEmptied_KK2`, `ASoldOutLine_DoesNotEmptyTheCartOnItsOwn_KK2` (D-580 ile yeniden adlandırıldı); `CartTimerScanTests` (mevcut, zamanlayıcı yok); `Carts` hiçbir yerde sepeti boşaltmaz |
| 3 | Girişte iki sepet birleşir; aynı varyantta büyük adet, farklı varyant ayrı kalem; giriş yolundan bağımsız | ✓ | `CartTests.TheMerge_KeepsTheLargerQuantity_*_KK3`, `TheMerge_IsOneMethod_ForEveryWayIn_*_KK3`, `TwoMergesAtOnce_*_KK3`, `TwoAddsAtOnce_*_KK3`; tek yöntem `IOrderingModule.MergeGuestCartAsync` |
| 4 | Görülmeyen ürün/adet bir kez ve adıyla söylenir; eklenmediyse mesaj yok | ✓ | `CartTests.TheMerge_TellsOnceWhatTheGuestCartDidNotShow_ByName_KK4`, `TheMerge_TellsNothing_WhenTheMemberCartAddedNothingTheGuestDidNotSee_KK4`; `CartRulesTests.MergeShowsNew_*_KK4` |
| 5 | Güncel fiyat ve alınabilirlik; ilk fiyattan farklıysa tek metin, yönsüz; adet artışı referansı değiştirmez; birleşmede önce eklenen belirler | ✓ | `CartTests.ThePriceMark_IsOneFlag_WithoutDirection_AndGoesWhenThePriceReturns_KK5`, `AQuantityIncrease_DoesNotMoveTheReferencePrice_KK5`, `TheMerge_TakesTheReferencePriceOfTheLineAddedFirst_KK5` (iki yön); `CartRulesTests.PriceChanged_*` |
| 6 | Tükendi kalem sepette kalır, siparişe girmez; stok dönünce alınır; özet gösterilmeden düşen kalem özete girmez | ✓ | `CartTests.ASoldOutLine_StaysInTheCart_DoesNotEnterTheOrder_AndEntersAgainWhenTheStockReturns_KK6`, `ASoldOutVariant_CannotBeAdded_*_KK6`; `CartRulesTests.EntersOrder_*_KK6` |
| 7 | Vitrinden kalkan kalem çıkar ve bir kez söylenir; siparişe girecek kalem yoksa ödeme açılmaz, sebep sepette; (onayda boşalma T348); girmeyen kalem tutara girmez | ✓ | `CartTests.AnItemTakenOffTheShowcase_LeavesTheCart_AndIsToldOnce_KK7`, `WithoutAnOrderableLine_ThePaymentStepDoesNotOpen_AndTheReasonIsInTheCart_KK7`; `CartRulesTests.PaymentBlock_*_KK7`, `OrderableSubtotal_*_KK7`. Onayda boşalma T348 KK2; kupon asgarisi, kargo ve eşik T349 (D-562) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–7 | ✓ | yukarıdaki tablo; önceki işçinin kodu (`Carts.cs`, `CartRules.cs`, `CartVariantReader.cs`) diff'te satır satır okundu, testlerle çalıştırıldı |
| KK1 üye iki tarayıcı / çıkışta boş | ✓ | `CartTests` KK1 üç test (üçüncüsü: kaydedilmeyen işlemde birleşme geri alınır) |
| KK2 süre, yarım ödeme, sağlayıcı kesintisi, kapalı satış | ✓ | `CartTests` KK2 iki test: iki yıllık misafir sepeti ve sepete bağlı açık sipariş durur. Sağlayıcı kesintisi ve satış kapısı sepet modülünün okumadığı durumlardır (hiçbir sepet yöntemi ödeme ya da ayar okumaz: `Carts.cs` yalnız `ICatalogModule`'e bakar); sayfadaki gösterimi D-559 (T508) |
| KK3 büyük adet, giriş yolu fark etmez | ✓ | `CartTests` KK3 dört test (iki eşzamanlı birleşme ve iki eşzamanlı ekleme dahil) |
| KK4 bir kez, eklenmediyse yok | ✓ | `CartTests` KK4 iki test |
| KK5 yönsüz tek işaret, referans, önce eklenen | ✓ | `CartTests` KK5 dört test |
| KK6 Tükendi siparişe girmez, stok dönünce girer | ✓ | `CartTests` KK6 iki test |
| KK7 kalkan kalem çıkar, boş sepette ödeme açılmaz; Tükendi tutara girmez | ✓ | `CartTests` KK7 iki test; `CartRulesTests` |
| `09 §25` ilgili satırlar | ✓ | aşağıdaki tablo |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | kapalı yer yalnız `CartStatements.cs` tam yolu; `TheCartStatementsPlace_*` dört deyim, iki tablo, `UPDATE`/`DELETE`/`FROM` yok olarak sabitler ve aynı metnin başka yolda sessiz kaldığını sınar |
| Sık FAIL (2) tarama çağrı biçimini kaçırıyor | ✓ | tüm ham deyimler tek dosyada (`CartStatements.cs`); `Carts.cs` ve `CartVariantReader.cs`'te `ExecuteSql*`/`ExecuteUpdate` yok; Unit'te bütün taramalar yeşil |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok; `carts`/`cart_items` mevcut (T-önceki), `IsMergeNoticePending` mevcut DB default'lu |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `ApplicationEnums.NotStored`, `VersionedSideBulkWriteScan.AllowedPlaces` + kapsamı sabitleyen yeni test; `AllowedPlaces` sayısını anan belge yok (grep) |
| Sık FAIL (5) belge kuralını değiştiren yeni 09 metni | ✓ | 09/05/06/02 metni değişmedi |
| Eşzamanlılık: kilit sırası, deadlock | ✓ | birleşme yalnız misafir sepet kilidini alır (tek kilit, `05 §7.3.5`); üye tarafı koşullu deyimlerle yazılır; `TwoMergesAtOnce_*`, `TwoAddsAtOnce_*` istisnasız |
| Modül yöntemi kendi işlemini açmıyor / kaydetmiyor | ✓ | `Carts` yalnız çağıranın işlemine katılır; `TheCartMethods_DoNotSaveOnTheirOwn_*` |
| Testte gerçek saate bağlılık yok | ✓ | testler bekleme/Stopwatch kullanmaz; sınırlar veri ile kurulur (iki yıllık erişim anı) |

`09 §25` ilgili satırlar:

| Satır | Sonuç | Kanıt |
|---|---|---|
| Veri: göç, FK, kilit dosyası | ✓ | şema değişikliği yok; yeni enum saklanmıyor (`NotStored`, kilit dosyası yok) |
| Eşzamanlılık | ✓ | yukarıda |
| Modül sınırı | ✓ | Ordering Catalog'a yalnız `ICatalogModule.ListCartVariantFactsAsync` ile bakar; Catalog tablosuna Ordering'den sorgu yok |
| Kayıt / gizli veri | ✓ | `CartOwner.ToString` anahtarı yazmaz; veritabanında yalnız `SecretKey.Hash` özeti (`CartOwner_DoesNotPrintTheGuestKey_KK1`) |
| Tek işlev | ✓ | sepet işlemleri `Carts`'ta; birleşme tek yöntem |

Güvenlik taraması: kullanıcı girdisi ham SQL'e yalnız parametre olarak (`ExecuteSqlAsync` interpolasyonu) girer; misafir anahtarı ham saklanmaz; başka müşterinin sepetine erişim yolu yoktur (sahip her çağrıda parametredir; sayfa katmanı sahibi oturumdan kurar, D-559).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 4716 geçti, 0 kaldı (16 yeni `CartRulesTests`, 1 yeni tarama testi) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests/... --filter-class *Ordering.CartTests --filter-class *ProductTransitionsTests` (PostgreSQL 18, docker) | 60 geçti (19 yeni `CartTests` + 41 `ProductTransitionsTests`) |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` (Web, UnitTests, IntegrationTests) | değişiklik yok |
| Contract | — | — | bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | **tur 2: ✓ PASS** (2026-11-08; head `7eb682aa`) · tur 1: ✗ FAIL (head `859f957a`) |
| Bulgu sayısı | tur 2: 0 · tur 1: 1 (ORTA, S2; düzeltme 1'de kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | tur 2: 2 (K1 → D-589, K2 → D-590) · tur 1: 4 (K1, K2 bu PR'da; K3 → D-579, K4 → D-580) |

### Doğrulama tur 2 (2026-11-08)

**Verdict: ✓ PASS** (kapsam tam: bütün kabul kriterleri, KK1–KK7, `09 §25`, güvenlik taraması).

Başlangıç: working tree temiz; `core.hooksPath` depo bekçisi; ana dal son 3 run (42489399568, 42489399534, 42487801055) success.

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Üyenin sepeti hesapta, misafirinki tarayıcıda; çıkışta boş | ✓ | `Carts.FindCartIdAsync` (üye `customer_id`, misafir `SecretKey.Hash`); `CartTests` KK1 üç test yeşil (birleşmeden sonra misafir anahtarı boş sepet, okuma sepet açmaz) |
| 2 | Kendiliğinden boşalmaz | ✓ | `carts`/`cart_items` silen tek yerler `Carts.TakeRemovedNamesAsync` (bir kez söylenen işaret) ve `MergeAsync` (misafir sepet); süre parametresi, ödeme ya da ayar okuması yok (grep); `CartTests` KK2 iki test |
| 3 | Büyük adet kalır, farklı varyant ayrı, giriş yolundan bağımsız | ✓ | `CartStatements.MergeItemAsync` (`GREATEST`); tek yöntem `MergeGuestCartAsync` (bağlama D-558 → T515); KK3 dört test, eşzamanlı birleşme ve ekleme istisnasız |
| 4 | Görülmeyen ürün/adet bir kez, adıyla | ✓ | `CartRules.MergeShowsNew`; `IsMergeNoticePending` okumada düşer; KK4 üç test + birim |
| 5 | Canlı fiyat, yönsüz tek işaret, adet artışı ve birleşme referansı | ✓ | `CartVariantReader` (indirimli fiyat `DiscountRules.AppliedPrice`), `CartRules.PriceChanged`, `UpsertItemAsync` fiyata dokunmaz, `MergeItemAsync` küçük ana göre; KK5 dört test |
| 6 | Tükendi kalır, siparişe girmez, stok dönünce girer | ✓ | `CartRules.EntersOrder`; KK6 iki test |
| 7 | Kalkan kalem çıkar ve bir kez söylenir; ödeme adımı engeli; tutar | ✓ | `RemoveFromCartsAsync` + `TakeRemovedNamesAsync`; `PaymentBlockOf`, `OrderableSubtotal`; KK7 iki test + birim (onayda boşalma T348, kargo/eşik T349 — D-562) |

Testler: build `-warnaserror` 0/0; format üç proje çıkış 0; Unit 4773/4773; betik testleri 4/4; Integration (PG18, `shopfolio-t346-val-pg`) Ordering 246, Address + Membership + Sitemap 71, Disposal 61, Catalog 72 — hepsi yeşil; `origin/main` birleştirmesinden sonra (çakışmasız) build temiz, Unit 4864, Ordering 281, Address + Membership 110, Disposal 61 yeşil; `--locked-mode` çıkış 0. Dal CI run 42490284273 (head `7eb682aa`): bütün işler success.

Güvenlik: misafir anahtarı yalnız özetle saklanır, `CartOwner.ToString` yazmaz; ham SQL `ExecuteSqlAsync` interpolasyonuyla parametreli ve tek kapalı yerde (`CartStatements.cs`, tarama testi dört `INSERT`); sahip her çağrıda parametre; yeni bağımlılık yok.

`12` satırları: `VAL-A028`, `A029` (KRİTİK), `B006`, `B007` uç yanıtı (T508, giriş T515), `D002` iş kaydı ister → `BEKLEMEDE` kalır (tur 1 ve T318/T338 emsali); `12_matris_denetle.py --kapsam 1-77` hata 0.

K-notları: K1 → D-589 (misafir ekleme ile birleşme aynı anda: ekleme kilitten sonra silinmiş sepete düşer), K2 → D-590 (birleşmede `RemovedProductName` işaretinin taşınması test edilmemiş).

Yapım raporu karşılaştırması: davranışta tam uyum. Uyuşmazlıklar (bu turda düzeltildi): kriter 2 kanıtında eski test adı (D-580 öncesi); "Known Limitations" D-561 hedefi T347 yazıyordu (T376). "Test Sonuçları" tablosu ilk yapımın sayılarını taşır (Unit 4716, Integration 60); güncel sayılar "Düzeltme 1" ve bu bölümdedir.

### Doğrulama tur 1 (2026-11-08)

**Verdict: ✗ FAIL.** Kabul kriterleri 1–7 ve KK1–KK7 davranış olarak ✓ (kanıt: `CartTests` 23/23 yerelde PG18'de yeşil; `carts`/`cart_items`'a yazan bütün yerlerin grep taraması KK2 için; kod okuması). Bloke eden tek sebep:

| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Etkilenen dosya |
|---|---|---|---|---|---|
| 1 | — (CI/regresyon) | ORTA | S2 | Dal CI'ı yeşil olmalı ↔ run 42483081386'da 4. Integration test FAIL: 2708 testten 110'u `Unable to resolve service for type 'Shopfolio.Modules.Ordering.Carts' while attempting to activate 'Shopfolio.Modules.Ordering.OrderingModule'` ile düşüyor (MembershipTests 49, OrderDisposalTests 24, DisposalTests 13, RecordDisposalTests 12, AddressTests 12). `OrderingModule`'ü elle kuran düzenekler `Carts`, `CartStatements`, `CartVariantReader` kaydını almadı. Yerelde OrderDisposalTests + AddressTests 36/36 aynı hatayla yeniden üretildi | `tests/Shopfolio.IntegrationTests/Disposal/DisposalHarness.cs`, `Model/AddressTests.cs`, `Model/MembershipTests.cs` |

Testler: build `-warnaserror` temiz; format temiz; Unit 4751/4751; betik testleri 4/4; `--locked-mode` çıkış 0. Güvenlik: temiz (anahtar özetle saklanır, ham SQL parametreli, yeni bağımlılık yok).

`12` satırları: `VAL-A028`, `A029`, `B006`, `B007`, `D002` uç yanıtı / iş kaydı kanıtı ister; T346 modül düzeyidir (uç T508, giriş T515) → `BEKLEMEDE` kalır (T318/T338/T380 emsali). KP-9/KP-10'un öteki satırları T347 ve T340'ındır.

K-notları: K1 D-302 hedefi T508'e taşındı (modül tarafı `OpenCartAsync`'te); K2 D-561 hedefi T347 → T376 (L-6 `11 §7.1`'de T376/T338'e bağlı); K3 → D-579 (birleşme `IsPaymentUnavailableNoticePending`'i taşımıyor); K4 → D-580 (KK2 testinin adı gövdesiyle uyuşmuyor).

Yapım raporu karşılaştırması: Integration satırı "✓ 60 geçti" diyor ama tam suite koşulmamış; regresyon buradan kaçtı. CartTests sayısı teorilerle 23'tür (raporda 19).

### Düzeltme 1 (2026-11-08)

`origin/main` birleştirildi (rebase değil): tek kod çakışması `CatalogModule.cs` kurucusunda; iki tarafın üyeleri birlikte kaldı (`CatalogMenuReader menu` + `CartVariantReader cartVariants`). Kayıt belgelerinde main sürümü alındı, dalın girişleri `STATUS_CHANGELOG.d/T346.md` / `DEFERRED_BACKLOG.d/T346.md` parçalarında.

Bulgu 1: `OrderingModule`'ü elle kuran üç düzenek (`DisposalHarness`, `AddressTests`, `MembershipTests`) tek ortak yardımcıyı kullanır: `tests/Shopfolio.IntegrationTests/Ordering/CartTestServices.cs` (`AddCartServices`: `Carts`, `CartStatements`, Catalog modülü ve ona gereken `ImageProcessor`, dosya deposu, disk eşikleri). Catalog kaydı dijital dosya silme parçalarını (`IncompleteDigitalUploadPart`, `DigitalFileDisposalPart`, `OrphanDigitalFilePart`) da getirdiğinden `DisposalHarness`'ın kendi üç kaydı kaldırıldı (çift kayıt `Sequence contains more than one element` veriyordu). Üretim DI'ı değişmedi. K3 (D-579): `Carts.MergeAsync` misafir sepetin "ödeme yöntemi yok" işaretini üye sepetine taşır (+ yeni `CartTests`); K4 (D-580): test adı gövdesine uydu.

Öz denetim: 11'deki T346 kabul kriterleri 1–7 ve `12` kontrol listesi birleşik ağaçta yeniden denetlendi; `CartTests` (24) ve bütün `Ordering` ad alanı (246) yeşil, T361'in menü okuyucusu ile sepet okuyucusu birlikte kayıtlı. `impl/sik-fail-nedenleri.md` okundu: "main ilerleyince dalın testleri yeni alanı tohumlamıyor" ve "birleşik ağaçta test koş" satırları nedeniyle Integration tam paket birleşik ağaçta koşuldu.

| Tür | Sonuç | Çıktı |
|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` 0 uyarı, 0 hata |
| Format | ✓ | Web, UnitTests, IntegrationTests `--verify-no-changes` çıkış 0 |
| Unit | ✓ | 4773/4773 |
| Integration (tam, ad alanı grupları, PG18 `shopfolio-t346-fix-pg`) | ✓ | Model 1315 (1 test paralel koşuda bir kez düştü, tek başına 55/55 geçti: `SchedulerBackupDisposalAndKeyTablesTests` göç/rol testi), Catalog+Concurrency+CorporateContent 184, Disposal+Jobs+Logging+Files 270, Ordering+Outages+Pages 264 (düzeltme sonrası Ordering 246), Security+SignedEndpoints+Web+FileSecrets+Database 697; başka düşen yok |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T346-sepet-sahiplik`
- **PR:** başlıkta ve STATUS satırında
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Giriş yolları birleştirmeyi ayrı işlemde çağırır | D-558 | giriş sayfası task'ı (T515) |
| 2 | Sepet sayfası `OpenCartAsync` ve satış kapısı okuması | D-559 | T508 |
| 3 | Stok, sipariş başına sınır, dijital adet, `int` taşması koruması | D-560 | T347 |
| 4 | L-6 sayaç taşıması birleşmede | D-561 | T376 (doğrulama tur 1 K2) |
| 5 | `OrderableSubtotal` kupon asgarisi, kargo ve eşik okuması | D-562 | T349 |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yönetici izler
- **Devir:** ilk işçi disk dolduğu için testlere geçemeden durdu (`e5176a9a`); devam işçisi `task/T346-sepet-sahiplik-devam` yerel dalından `task/T346-sepet-sahiplik`'e push etti.
- **Diğer:** ⚠ yok; kolay kararlar: saklanmayan enum'lar `NotStored`'a, ham SQL tek kapalı yere (D-361 / `GalleryMainMark` kalıbı).
