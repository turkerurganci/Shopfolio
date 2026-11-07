# T334 — Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama ✓ PASS, tur 1) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Stok kuralı:** `StockRules` (saf): `Check(type, stock)` — fiziksel üründe sayı zorunlu ve eksi değil, dijital üründe alan yok, hizmette boş = sınırsız; `IsSoldOut`, `IsProductSoldOut` (bütün görünür varyantlar tükenmişse ürün tükenmiş, varyantsız ürün asla).
- **Bekçi:** `VariantStockGuard` (SaveChanges kesicisi, `AppDbContext.Configure`'a kayıtlı): eklenen varyantın stoku tipe uymuyorsa ya da ürün tipi değişince mevcut varyantlar uymuyorsa reddeder (iki yön).
- **Sayaç:** `StockCounters.SaveStockQuantityAsync` tipe göre `StockRequired` / `NotTracked` döner, ayrılmış adedin altına inmeyi `BelowReservedBy(ayrılmış adet)` ile reddeder (artırma ve kontenjanı boşaltma serbest); `GetStateAsync`, `GetAvailabilityAsync`.
- **Modül yüzü:** `ICatalogModule.GetStockStateAsync`, `GetAvailabilityAsync`; `Contracts/Variants/{VariantAvailability, ProductAvailability, VariantStockState}`; `StockQuantityWriteResult` genişledi; `IOrderingModule.ListReservationHoldersAsync` + `ReservationHolder` (ayırmayı tutan siparişler).
- **Varyant fabrikası:** `Variant.Create(..., int? stockQuantity = null)`; fiziksel ürün varyantı kuran mevcut testler stok 100 ile güncellendi.
- **Yasak kalıplar:** `SalesModelShape` stok kalıpları (stokta haber ver, düşük stok eşiği, bekleme listesi vb.) + pozitif/negatif örnekler.
- **Merge kaydı:** T301, T307, T315, T322, T332, T366, T381 status Commit hücreleri ve raporlarının "Post-merge run'lar" satırı dolduruldu.
- **Backlog:** D-335 (panel stok ekranı, T525), D-336 (vitrin "Tükendi" + sepet engeli, T503), D-337 (iptal/cayma/kalem çıkarma çağrıları, T367); aktif kalem 128 → 131.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `StockRules.cs`, `VariantStockGuard.cs`, `StockCounters.cs`, `Variant.cs`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs`, `Contracts/StockQuantityWriteResult.cs`, `Contracts/Variants/*`
- `src/Shopfolio.Web/Modules/Ordering/`: `OrderingModule.cs`, `Contracts/IOrderingModule.cs`
- `src/Shopfolio.Web/Infrastructure/AppDbContext.cs` (bekçi kaydı)
- `tests/Shopfolio.UnitTests/`: `Catalog/StockRulesTests.cs`, `Architecture/SalesModelShape.cs`, `Architecture/SalesModelShapeTests.cs`, `Infrastructure/SitemapSourceTests.cs`
- `tests/Shopfolio.IntegrationTests/Model/StockAndCapacityTests.cs` (27 test) + stok verilen mevcut testler
- `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Stok varyantın sayısal adedi | ✓ | `StockRulesTests.TheStock_IsRequiredForAPhysicalVariant...`; `StockAndCapacityTests.APhysicalVariant_NeedsAStockNumber_AndZeroIsANumber_KK1_KK2` |
| 2 | Fiziksel zorunlu, dijital alansız, hizmet isteğe bağlı | ✓ | `ADigitalVariant_HasNoStockField...`, `AServiceVariant_TakesACapacityOrNone_EmptyIsUnlimited_KK2`, `ChangingAProductsType_UnderVariantsWhoseStockWouldNotFit_IsRefused_KK2`, `ThePanelsWrite_FollowsTheType...` |
| 3 | Tükenmiş varyant "Tükendi" işaretiyle listelenir, seçilemez | ✓ | `AVariantWhoseLastUnitIsReserved_IsSoldOut...`, `ASoldOutVariant_IsListedWithTheMark_NotHidden...` (vitrin gösterimi D-336) |
| 4 | Tükenmiş ürün vitrinde kalır; haber ver kaydı yok | ✓ | `AProductWithEveryVariantSoldOut_StaysPublished...`, `NoTableOrColumn_KeepsANotifyMeRecord...`, SalesModelShape stok kalıpları |
| 5 | Stok adedi ve eşik uyarısı ziyaretçiye yok | ✓ | `WhatTheVisitorIsGiven_CarriesNoNumberAndNoThreshold_KK5` (birim), `APhysicalVariantsAvailability_IsOnlyTheFlag_NeverACount_KK5` |
| 6 | İptalde dönüş, iadede dönmeme, caymada kontenjan dönüşü | ✓ | `ACancelledUnpaidOrder...`, `ACancelledPaidOrder...`, `AServicesWithdrawal...`, `ARefundedPhysicalUnit_IsNotReturnedByItself...` (otomatik çağrılar T367/T348/T386/T319: D-337) |
| 7 | Ayrılmış adedin altına indirme reddi, ret adedi ve tutan siparişleri söyler; artırma serbest | ✓ | `ALoweringBelowTheReservedQuantity_IsRefused...`, `TheHoldersOfAVariant...`, `ALoweringToExactlyTheReservedQuantity...`, `IncreasingTheStock_AndEmptyingTheCapacity_AreFree...`, `ACapacityEnteredForAnUnlimitedService...` (panel ekranı D-335/T525) |
| 8 | Ayırma onayda, iki eşzamanlı onaydan biri tutar | ✓ | `TwoConfirmationsOfTheLastUnit_AtTheSameTime_ReserveItOnce_KK8`, `AConfirmationThatTakesMoreThanIsLeft_ReservesNothing...`; T228 `ConcurrencyTests` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 stok tam sayı, eksiye düşmez | ✓ | `TheStock_NeverGoesBelowZero_ByTheGuardAndByTheDatabase_KK1`, `ThePanelsWrite_RefusesANegativeNumber...` (CHECK kısıtı mevcut) |
| KK2 dijitalde alan yok, hizmette sınırsız | ✓ | `ADigitalVariantAndAServiceWithoutCapacity_AreNeverSoldOut_KK2_KK3`, `AServiceVariant_TakesACapacityOrNone...` |
| KK3 tükenmiş seçilemez, işaret görünür | ✓ | `AServiceWithACapacity_IsSoldOutWhenTheCapacityIsFull...`, `ASoldOutVariant_IsOneWithNoUnitLeftAfterTheReservedOnes_KK3` |
| KK4 tükenmiş ürün vitrinde, haber ver yok | ✓ | `AProduct_IsSoldOutOnlyWhenEveryVisibleVariantIs...`, `NoTableOrColumn_KeepsANotifyMeRecord...` |
| KK5 hiçbir yanıtta adet/eşik yok | ✓ | `VariantAvailability` / `ProductAvailability` yalnız bayrak; `WhatTheVisitorIsGiven_CarriesNoNumber...` yansıma taraması |
| KK6 iptalde döner, iadede dönmez, caymada döner | ✓ | KK6 test dörtlüsü (yukarıda) |
| KK7 ayrılanın altına ret, artırma serbest | ✓ | `TheWriteResult_HasOneAnswerForEachWayTheWriteEnds...`, KK7 testleri; `AValueThatChangedUnderTheForm_IsReportedAsChanged...` |
| KK8 eşzamanlı onayda fazla ayırma yok | ✓ | `TwoConfirmationsOfTheLastUnit...` |
| `09 §25` ilgili satırlar | ✓ | 09 değişmedi; sürüm denetimli yazım (K-980) korunur; yeni sayfalı liste yok; yeni sayı/sabit yok |
| Güvenlik taraması | ✓ | yeni uç nokta/giriş yok; bekçi ve sayaç parametreli EF/SQL; ret iletisi yalnız yöneticiye (sipariş tutan listesi) |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | tarama istisnası eklenmedi; bekçi hem eklenen varyantı hem tip değişimini denetler |
| Sık FAIL (2) tarama çağrı biçimini kaçırır | ✓ | shape örnekleri pozitif ve negatif; `EveryProductType_IsJudgedByTheRule_NoTypeIsLeftOut` |
| Sık FAIL (3) DB default / FK dizini | ✓ | şema ve FK değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | backlog `Aktif kalem` 131 (125 ⚪ + 3 🟡 main + 3 yeni), status/changelog/memory, merge kaydı |
| Sık FAIL (5) 09 kuralı karar kaydısız | ✓ | 09 değişmedi |
| Sayfalı liste sınır testi | ~ | `ListReservationHoldersAsync` sayfalı değil, tek varyantın ayırma satırlarıyla sınırlı; sayfalama D-335 ile T525'te |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` (origin/main birleştirmesi sonrası) | 4069/4069 (Consistency dahil) |
| Integration | ✓ | `--filter-class` parçalı, `SHOPFOLIO_TEST_DATABASE` yerel Postgres | StockAndCapacity 27/27 (birleştirme sonrası); yapım turunda SalesModel+CatalogCore+CartAndOrderCore+StockReservation+PriceCoupon 200, Concurrency+Files 107, Ordering+Pages+Disposal 76, Jobs+Logging+Outages+Security+SignedEndpoints+FileSecrets 193, ModelRule/ModuleCall/ItemRecord/EnumDatabase/SalesGate/StartupData/SampleDatabase/AuditTrail 177 yeşil; Web 494 içinde 1 hata (aşağıda) |
| Format / derleme | ✓ | `dotnet format --verify-no-changes`, `dotnet build -warnaserror` | temiz |

⚠ `Web.VersionedFormPageTests.OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8` sınıf birlikte koşulunca düşüyor (`SeedOrderAsync`, `ix_legal_text_versions_legal_text_type_version_number` yinelenen); tek başına geçiyor, bekçi kapalıyken de düştü, T334 değişikliğinden bağımsız görünüyor (main CI yeşil). Kalan Model sınıfları ve Database ad alanı yerelde koşulmadı (varyantla ilgisiz); CI/validator koşar.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07; head `2b3e600` + `origin/main` (T328, `fe78e7b`) birleştirmesi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (D-362) |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = ana checkout'un `scripts/git-hooks`'u; ana dal CI son üç tamamlanmış run success — 42405002224 (`fe78e7b`, Publish), 42402532255, 42402532286 (`6eec072`); `MEMORY.md` T334 satırı var.

**Kabul kriterleri (validator kanıtı):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `Variant.StockQuantity` `int?` (06: `integer`); `StockRules.Check` eksi değeri `NegativeStock` sayar; bekçi eksi stoklu eklemeyi SQL'den önce reddeder, ham `UPDATE … -1` `ck_variants_stock_not_below_reserved` ile düşer (`TheStock_NeverGoesBelowZero_…_KK1`); panel yazımı eksiyi `ArgumentOutOfRangeException` ile reddeder, değer değişmez |
| 2 | ✓ | `StockRules.Check`: fiziksel + boş → `PhysicalWithoutStock`, dijital + sayı → `DigitalWithStock`, hizmet her iki hâlde temiz (Unit teorisi + `EveryProductType_IsJudgedByTheRule…`); `VariantStockGuard` eklemede ve tip değişiminde (iki yön) reddeder; `SaveStockQuantityAsync` fizikselde boşu `StockRequired`, dijitalde sayıyı `NotTracked` döner; kontenjansız hizmet `IsSoldOut` = false (sınırsız) |
| 3 | ✓ | `StockRules.IsSoldOut(stock, reserved)` = stok − ayrılmış ≤ 0; `GetAvailabilityAsync` tükenmiş yayındaki varyantı listeden düşürmez, bayrakla verir, taslak/arşiv varyantı vermez (`02 §3.7.5`); son adet ayrılınca tükenir, ayırma serbest kalınca geri gelir (Integration KK3 testleri); vitrin işareti ve sepete ekleme reddi T503'ün işi (D-336) |
| 4 | ✓ | Bütün varyantları tükenmiş ürün `Published` kalır ve `IsSoldOut` = true; yayından çeken yol yok; DB'de haber ver/bekleme listesi tablo ya da sütunu yok (`NoTableOrColumn_…_KK4_KK5`); `SalesModelShape` "notify me", "back in stock", "waitlist" vb. kalıplarını kod adlarında yasaklar (pozitif/negatif örnekli) |
| 5 | ✓ | Ziyaretçi türleri `VariantAvailability(VariantId, IsSoldOut)` ve `ProductAvailability(Variants, IsSoldOut)` sayı taşımaz (Unit yansıma taraması `WhatTheVisitorIsGiven_…_KK5`); `StockQuantity`'yi okuyan tek yer `StockCounters` (grep) ve panel türü `VariantStockState` yalnız `ICatalogModule.GetStockStateAsync`'tedir; "low stock", "only left", "stock threshold" kalıpları yasak listesinde |
| 6 | ✓ | Sayaç rejimi: ödeme beklerken iptal `ReleaseAsync` (ayrılan serbest), ödenmiş iptal ve caymada `ReturnAsync` işleme giren adet kadar ekler (kısmi 2/3 → 9), sınırsız hizmete ekleme yok; iade hiçbir sayaç hareketi yapmaz, yönetici panelden `SaveStockQuantityAsync` ile ekler (Integration KK6 dörtlüsü); bu çağrıları yapan iptal/cayma/kalem çıkarma/düzeltme işlemleri T367, T386, T319'da (D-337) |
| 7 | ✓ | Koşullu güncelleme `newValue == null \|\| newValue >= ReservedQuantity`; ret `BelowReservedBy(ayrılmış)` ayrılmış adedi taşır (5), `ListReservationHoldersAsync` tutan siparişleri numara ve adetle verir (toplamı ayrılmışa eşit), ayırması bitmiş siparişi vermez; ayrılmış adede eşit değer kabul, bir altı ret; artırma ve kontenjan boşaltma ayrılmış adet varken kabul; değişmiş gösterilen değer `ShownValueChanged` (ayrılmış adet söylenmez); panel ekranı T525 (D-335) |
| 8 | ✓ | `Reserve` koşullu güncellemesi (`StockQuantity == null \|\| Stock − Reserved ≥ adet`); `Commit` stok ve ayrılmışı birlikte düşer; iki eşzamanlı işlemde son adet bir kez ayrılır, kaybeden `false` alır, sayaç (1, 1) (`TwoConfirmationsOfTheLastUnit_…_KK8`); fazlasını isteyen onay hiçbir şey ayırmaz |

**Task satırları (`12 §7.5`):** `11 §7.1` T334 kaynakları KP-4, KP-16, KP-39, KD-9, KD-33. KP grep'i `VAL-A009`…`VAL-A011` (KP-4), `VAL-D005`…`VAL-D008`, `VAL-B013` (KP-16), `VAL-A163`…`VAL-A176` (KP-39) satırlarını getirir. KP-4 ve KP-39 satırlarının beklenen sonucu ürün sayfası ya da panel ekranıdır (tarayıcı gözlemi, uç yanıtı; KP-4 → T503, KP-39 → T525) ve T334 uç ya da sayfa kurmaz; KP-16 satırlarının ön koşulu onaylanan/ödenen sipariştir (onay ve ödeme akışı T348, kupon T338). Hepsi `BEKLEMEDE` kalır, `Durum` değişmedi; T334'ün kabul kriterleri bu satırların modül düzeyindeki ön koşullarını sağlar (VAL-A011, A167, A171…A174, D005, D007, B013). KD-9 ve KD-33'ün matriste satırı yok.

**Doğrulama kontrol listesi:** KK1–KK8 ✓ (yukarıdaki kanıtlar); `09 §25` ✓ — göç yok, yeni paket yok, yeni ayar yok; modül sınırı korunur (yeni yüz yalnız `Contracts`: `ICatalogModule.GetStockStateAsync`, `GetAvailabilityAsync`, `IOrderingModule.ListReservationHoldersAsync`); sayaç yazımı tek yerde koşullu güncelleme, `LockSequence` bildirimi yerinde, işlem dışı çağrı reddedilir; okumalar `AsNoTracking`; bekçi `AppDbContext` kesici listesinde. Güvenlik: sır yok; yetki etkisi yok (yeni uç yok); girdi — EF parametreli sorgular, ham SQL yok; ziyaretçiye giden türde stok adedi yok.

**Testler (validator):** `dotnet build -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç proje çıkış 0; Unit 4069/4069 (birleştirmeden önce), 4093/4093 (sonra); betik testleri 4 dosya geçti; Integration (yerel `postgres:18`) StockAndCapacity 27/27, CatalogCore+SalesModel 88/88, Concurrency+ImageFiles+CartAndOrderCore+StockReservationAndHistory 133/133, AppCache+SearchEngineData+VersionedFormPage 53/53 (yapım raporundaki `VersionedFormPageTests` düşmesi yinelenmedi), birleştirme sonrası NotificationMapQueue+SalesModel 31/31; `dotnet restore --locked-mode` çıkış 0; dal CI 42403928586 (head `2b3e600`) success — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate.

**Bloke-etmeyen notlar:** K1 — `SaveStockQuantityAsync` ürün tipini kilitsiz okur, `VariantStockGuard` tip değişiminde kayıtlı stokları kilitsiz okur; kontenjansız hizmet dijitale çevrilirken aynı anda kontenjan yazılırsa stoklu dijital varyant kalabilir. Bugün tip değiştiren yazma yolu yok → D-362 (T525).

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Uyuşmazlıklar: (1) raporda `12` task satırları bölümü yoktu (bu doğrulamada yazıldı); (2) backlog "Aktif kalem" 131 yazılmıştı, gerçek sayım (⚪ + 🟡) main birleştirmesiyle 133'tü — D-362 ile 134 olarak düzeltildi; (3) "Sayfalı liste sınır testi" `~` — kabul kriteri değil, `ListReservationHoldersAsync` tek varyantın ayırma satırlarıyla sınırlı, sayfalama gereği T525'te değerlendirilir (D-335).

**Merge notu:** dal `origin/main` (T328, #440) ile birleştirildi; çakışma yalnız kayıt belgelerinde (STATUS "Son güncelleme", backlog başlığı), main kayıtları korunarak çözüldü; T328 PASS girişi `STATUS_CHANGELOG.md`'ye taşındı, T328'in Commit hücresi ve post-merge satırı dolduruldu; kodda çakışma yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T334-stok-kontenjan`
- **Commit:** `1c8bf42` (dal başı, PR açılışında)
- **PR:** #443
- **CI:** dal CI 42403928586 (head `2b3e600`) success
- **Post-merge run'lar:** squash `af6e7d3` (#443) — CI 42411903829 success, Publish 42411903876 success (T335 dalında dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok. (Ayrılmış adet ve ayırma onayı mevcut T228/T348 sözleşmelerine dayanır; yeni dış bağımlılık eklenmedi.)

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Panelin stok alanı ekranı ve ret iletisinin gösterimi | D-335 | Panel T525'in işi |
| 2 | Vitrinin "Tükendi" gösterimi ve sepete ekleme engeli | D-336 | Vitrin T503'ün işi |
| 3 | İptal, cayma, kalem çıkarmada sayaç dönüşünü çağıran işlemler | D-337 | T367, T348, T386, T319'un işi |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** main son run'lar (`f9ea0fe` CI 42401437807, Publish 42401437836) success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** devam işçisi yapımı devraldı (önceki işçi bağlam sınırında bölündü); yapım turunda yerel Postgres `shopfolio-t334-db`.
