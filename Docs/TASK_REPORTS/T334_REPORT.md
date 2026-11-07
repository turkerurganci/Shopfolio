# T334 — Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama bekliyor) | **Tarih:** 2026-11-07

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

- **Branch:** `task/T334-stok-kontenjan`
- **Commit:** PR başlığındaki son commit (dal başı)
- **PR:** #PRNO
- **CI:** dal CI doğrulamada izlenir
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır sonraki task dalında güncellenir)*

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
