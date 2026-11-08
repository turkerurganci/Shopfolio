# T336 — Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **`Infrastructure/Money`:** `VatOf(gross, ratePercent)` = `Round(gross × oran / (100 + oran))` (`09 §14.1` madde 3; D-74'ün `VatOf` payı). Girdi denetimi değerle karşılaştırır (`value < 0`): `-0.00m` artık reddedilmez, `Round` düz sıfır verir (D-76).
- **`Modules/Catalog/ItemPricing` (saf):** `Price` — `02 §3.8.3` sırası: birim fiyat × adet (yuvarlanmaz) → kupon payı düşülür → kalan brütten KDV bir kez ayrılır ve kalemde kuruşa yuvarlanır, matrah kalandır. `SplitShippingFee` — kargo ücreti yalnız fiziksel kalemlerin oranlarına bölünür (tek oran tamamını alır; çok oranda `Money.Allocate`: parçalar aşağı, artık en büyük parçaya), her parçanın KDV'si kendi oranıyla. `SumOrderTotal` — sipariş toplamı kalemlerin ödenecek tutarları + kargo; matrah + KDV'den türetilmez (K-61). `IsValidPrice` (sıfırdan büyük, en çok iki ondalık, para tavanı içinde) kendi denetimini yazmaz, panelin tutar alanı kuralını (`InvalidAmountAttribute`, sıfırsız) `FieldRuleAttribute.Allows` ile sorar; `HasAtMostDecimals` projenin tek ondalık sayımına (`FieldRuleAttribute.HasAtMostDecimals`, iki ondalıklı form kuralları da onu çağırır) yönlenir — tek kural evi (`09 §3.4` m.1).
- **`Modules/Catalog/PriceLabelRules` (saf):** üretim yerinin zorunluluğu/izni/görünmesi (`IsOriginRequired`, `IsOriginAllowed`, `ShowsOrigin`), net miktarın birimle eşleşmesi (`CheckNetQuantity`: eksik, birimsiz, sıfır ve altı, üçten fazla ondalık), birim fiyat (`UnitPrice`: temel birimin bir ölçüsü için, kuruşa yuvarlanmış; satış fiyatına eşitse gösterilmez) ve temel birim tablosu (`BaseOf`).
- **`ProductDefinitionRules` / `ProductDefinitionGuard`:** fiziksel üründe üretim yeri her kayıtta zorunlu (`PhysicalProductWithoutOrigin`), hizmette yasak (`OriginOnAService`); dijitalde isteğe bağlı.
- **`PriceLabelGuard` (yeni kayıt bekçisi, `AppDbContext` kesicilerinde):** varyant ve fiyat geçmişi fiyatında ikiden fazla ondalık SQL'den önce reddedilir (sütun sessizce yuvarlardı, `02 §3.8.1`); birim–net miktar eşleşmesi iki yönden denetlenir (yazılan varyant ürününün birimine — izlenmeyen ürün veritabanından okunur —, birimi yazılan ürün bütün varyantlarına — izlenmeyenler veritabanından); net miktarda üçten fazla ondalık reddedilir. Sıfır fiyat ve sıfır net miktar veritabanının kısıtlarına kalır (`ck_variants_price_above_zero`, `ck_variants_net_quantity_above_zero`).
- **`PriceLabelReader` + `ICatalogModule.GetPriceLabelAsync`:** fiyat (girildiği gibi), ürünün KDV oranı, satırın göründüğü üründe üretim yeri, fiyatın uygulandığı tarih (başlamış son `PriceHistory` satırı; saat `TimeProvider`'dan), ölçüyle satılan üründe birim, net miktar ve birim fiyat. Düz okuma; yazmaz, kilit almaz. `ICatalogModule`'e saf `PriceItem`, `SplitShippingFee`, `SumOrderTotal`; kayıtlar `Contracts/Pricing/`.
- **Testler:** Unit `ItemPricingTests`, `PriceLabelRulesTests`, `MoneyTests` ekleri, `ProductDefinitionRulesTests` güncel. Integration yeni `Model/PriceAndVatTests` (19 test, kendi koleksiyonu). Var olan entegrasyon testleri üretim yeri zorunluluğuna uyarlandı: ortak yardımcı `Database/SeededCountry` (başlangıç verisi göçünün "TR" satırının kimliği göçün kendi listesinden türetilir; `WithSeededOrigin()`), `CatalogCoreTablesTests` ülkeleri boşalttığı için kendi "ZZ" satırını ekler; `NetQuantityOfZero_IsRefused` ürüne birim verir ki veritabanı kısıtı sınansın.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Money.cs`, `AppDbContext.cs` (kesici listesi), `Validation/FieldRuleAttribute.cs` (`Allows`, genel `HasAtMostDecimals`)
- `src/Shopfolio.Web/Modules/Catalog/`: `ItemPricing.cs`, `PriceLabelRules.cs`, `PriceLabelGuard.cs`, `PriceLabelReader.cs`, `ProductDefinitionRules.cs`, `ProductDefinitionGuard.cs`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs`, `Contracts/Pricing/PriceLabel.cs`, `Contracts/Pricing/PricingRecords.cs`
- `tests/Shopfolio.UnitTests/`: `Catalog/ItemPricingTests.cs`, `Catalog/PriceLabelRulesTests.cs`, `Catalog/ProductDefinitionRulesTests.cs`, `Infrastructure/MoneyTests.cs`, `Infrastructure/SitemapSourceTests.cs` (sahte Katalog yeni yöntemleri taşır)
- `tests/Shopfolio.IntegrationTests/`: `Database/SeededCountry.cs` (yeni), `Model/PriceAndVatTests.cs` (yeni); üretim yeri uyarlaması: `Concurrency/ConcurrencyTests.cs`, `Files/ImageFilesTests.cs`, `Model/BranchContentLinkAndContactRequestTablesTests.cs`, `Model/CartAndOrderCoreTablesTests.cs`, `Model/CatalogCoreTablesTests.cs`, `Model/SalesModelTests.cs`, `Model/StockReservationAndHistoryTablesTests.cs`, `Web/AppCacheTests.cs`, `Web/ListSeed.cs`, `Web/SearchEngineDataTests.cs`, `Web/SitemapEndpointTests.cs`, `Web/VersionedFormPageTests.cs`
- Belgeler: bu rapor, `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `DEFERRED_BACKLOG.md` (D-74 kısmi, D-76 ✅, D-344–D-346; aktif 130 → 132, `origin/main`'deki T316'nın beş kalemiyle (D-305…D-309) 137); T301, T307, T315, T322, T328, T332, T366, T381 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri; T316'nın status Commit hücresi.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Fiyat varyantta, KDV dahil; yöneticinin yazdığı = ziyaretçinin gördüğü | ✓ | `PriceAndVatTests.ThePriceTyped_IsThePriceTheLabelShows_VatIncluded` (1234.56 yazılır, etiket ve sütun aynen); üç ondalık sütuna inmeden reddedilir (`APriceWithThreeDecimals_IsRefusedBeforeTheColumnRoundsIt`) |
| 2 | KDV oranı üründe, varyant devralır, varsayılan ayardan | ✓ | `TheVatRate_IsAProductColumn_AndNoVariantColumn`; `TheDefaultRate_ComesFromTheSetting_AndEveryVariantInheritsTheProductsRate` (ayar 20 → 10, `GetProductDefaultsAsync` oranıyla açılan ürünün iki varyantının etiketi 10) |
| 3 | Matrah ve KDV sipariş kaydında ayrışır; kargo fiziksel kalemlerin oranını izler | ✓ | `TheOrderItem_KeepsItsVatAmount_AndNoNetColumn_AndTheOrderKeepsTheShippingFeesSplit`; `ItemPricingTests.TheShippingFee_TakesThePhysicalItemsRate_NotTheDigitalOrServiceItemsRate`, `TheShippingFee_OverTwoRates_IsSplitInProportionToTheirItems_PartsDownAndTheLeftOverOnTheLargest`, `AShippingFeeWithoutAPhysicalItem_IsAProgrammingError` |
| 4 | KDV kalemde hesaplanır ve yuvarlanır; toplam yuvarlanmış kalemlerin toplamı | ✓ | `ItemPricingTests.TheItemAmount_IsNotRounded_AndTheVatIsRoundedOnceInTheItem`, `TheOrderTotal_IsTheSumOfThePayableItemsAndTheShippingFee_NotTheVatOfTheSum`, `RandomItems_KeepTheOrderAndAddUp` |
| 5 | Sıra: indirimli birim × adet → kupon payı → kalandan KDV | ✓ | `AnItemWithACoupon_TakesTheShareOffFirst_ThenSeparatesTheVatFromWhatIsLeft`, `TheVatOfACouponItem_IsNotTheVatOfItsAmountWithoutTheCoupon`, `TheOrderTotal_TakesTheCouponOffEachItem` |
| 6 | Tek tutar en yakın kuruşa, yarım yukarı; orantılı parçalar aşağı, artık en büyüğe | ✓ | `ItemPricingTests.RoundingDirection_OneAmountHalfUp_ASplitDownWithTheLeftOverOnTheLargest`; `MoneyTests` (`Round` tablosu 12,345 → 12,35; `VatOf_AnExactHalfKurus_GoesUp`; `Allocate`) |
| 7 | İki ondalık, sıfırdan büyük, ondalık tür | ✓ | `ItemPricingTests.APrice_IsAboveZeroWithAtMostTwoDecimals_InTheRuleAndInTheFormsAmountField`; `PriceAndVatTests.APriceWithThreeDecimals_…` (varyant + fiyat geçmişi), `APriceOfZero_IsRefusedByTheDatabase` (`ck_variants_price_above_zero`); tutarlar `decimal` (`TimeAndMoneyRulesTests` yeşil) |
| 8 | Üretim yeri (fizikselde zorunlu, dolu dijitalde görünür, hizmette yok); fiyat tarihi geçmişten; birim ürünün, kapalı listeden; net miktar varyantın, birim seçiliyse zorunlu ve > 0; birim fiyat türetilir | ✓ | `APhysicalProductWithoutItsOrigin_IsRefused_EvenAsADraft`, `APhysicalProduct_ShowsItsOrigin`, `ADigitalProduct_IsSavedWithoutAnOrigin_WithNoLine_AndShowsAFilledOne`, `AServiceCarryingAnOrigin_IsRefused_AndWithoutOneHasNoLine`, `ThePriceDate_IsTheLastHistoryRowThatHasStarted_NotAFutureOne`, `WithoutAHistoryRow_ThePriceDateIsEmpty`, `WithAUnit_AVariantWithoutANetQuantity_IsRefused_AndZeroIsTheDatabasesRefusal`, `WithoutAUnit_AVariantCarryingANetQuantity_IsRefused`, `ANetQuantityWithFourDecimals_IsRefused`, `AUnitGivenLater_IsRefusedWhileAStoredVariantHasNoNetQuantity_AndSavedWithIt`, `TheUnitPrice_IsDerivedFromEachVariantsOwnPriceAndNetQuantity`, `WithoutAUnit_TheLabelHasNoUnitLine`; `PriceLabelRulesTests`; `ProductDefinitionRulesTests` |

### Öz denetim (bitiş kapısı 2)

> Devam turu: önceki işçinin yaptıkları dahil bütün maddeler diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| k1 | ✓ | `PriceLabelReader.cs` (fiyat sütundan aynen); `PriceLabelGuard.cs` (ondalık reddi); `PriceAndVatTests.ThePriceTyped_…` |
| k2 | ✓ | `Product.VatRate` tek sütun, `variants`'ta KDV sütunu yok; `PriceLabelReader` oranı üründen okur; `TheDefaultRate_ComesFromTheSetting_…` (formun öneriyi göstermesi T525 — D-346) |
| k3 | ✓ | `OrderItem.VatAmount` saklanır, matrah sütunu yok; `ItemPricing.SplitShippingFee`; `PriceAndVatTests.TheOrderItem_…`; `ItemPricingTests` kargo testleri |
| k4 | ✓ | `ItemPricing.Price` (KDV kalemde `Money.VatOf`), `SumOrderTotal`; `ItemPricingTests` |
| k5 | ✓ | `ItemPricing.Price` sırası; `AnItemWithACoupon_…` |
| k6 | ✓ | yuvarlama yalnız `Money` (`Round`, `Allocate`, `VatOf`); `RoundingDirection_…`, `MoneyTests` |
| k7 | ✓ | `ItemPricing.IsValidPrice`, `PriceLabelGuard`, DB kısıtı; `APriceWithThreeDecimals_…`, `APriceOfZero_…` |
| k8 | ✓ | `ProductDefinitionRules`, `PriceLabelRules`, `PriceLabelGuard`, `PriceLabelReader`; KK8 testleri (yukarıdaki satır 8) |
| KK1 girilen fiyat aynen | ✓ | `ThePriceTyped_IsThePriceTheLabelShows_VatIncluded` (ekranın kendisi T503'ün ürün sayfası) |
| KK2 oran üründe, varyant devralır | ✓ | `TheVatRate_IsAProductColumn_AndNoVariantColumn`, `TheDefaultRate_…` |
| KK3 matrah/KDV ayrı; kargo fizikselin oranı | ✓ | `TheOrderItem_…`, `TheShippingFee_TakesThePhysicalItemsRate_…` |
| KK4 toplam kalem toplamı | ✓ | `TheOrderTotal_IsTheSumOfThePayableItemsAndTheShippingFee_NotTheVatOfTheSum` |
| KK5 üç adımlı sıra | ✓ | `AnItemWithACoupon_…`, `TheVatOfACouponItem_…`, `RandomItems_KeepTheOrderAndAddUp` |
| KK6 12,345 → 12,35; parçalar aşağı, artık en büyükte | ✓ | `RoundingDirection_…`; `MoneyTests` |
| KK7 üç ondalık ve 0,00 red | ✓ | `APriceWithThreeDecimals_…`, `APriceOfZero_…`, `APrice_IsAboveZeroWithAtMostTwoDecimals_…` |
| KK8 ülke, tarih, birim/net miktar, birim fiyat | ✓ | yukarıdaki KK8 testleri |
| `09 §25` doküman uyumu ve kapsam | ✓ | yeni adlar `06 §4.5`, §4.6, §4.17, §4.18 ile birebir (`OriginCountryId`, `MeasurementUnit`, `NetQuantity`, `VatRate`, `VatAmount`, `ShippingFeeVatBreakdown`); yeni ayar/tavan yok (`AmountDecimals` ve `NetQuantityDecimals` `06`'nın sütun hassasiyetidir) |
| `09 §25` modül sınırı, `SaveChanges` | ✓ | arayüz yöntemleri ya saf ya düz okuma, `SaveChanges` çağırmaz; `ModuleBoundaryTests` yeşil |
| `09 §25` para hesabı yalnız `Money` | ✓ | yuvarlama yalnız `Money`'de; `TimeAndMoneyRulesTests` yeşil |
| `09 §25` zaman `TimeProvider`'dan | ✓ | `PriceLabelReader` "şimdi"yi `TimeProvider`'dan okur |
| `09 §25` göç / şema | uygulanamaz | şema değişmedi, göç yok |
| `09 §25` yeni paket | uygulanamaz | paket yok |
| Güvenlik taraması | ✓ | sır yok; yetki etkisi yok (uç ve sayfa eklenmedi); girdi doğrulaması sıkılaştı (bekçiler); yeni dış bağımlılık yok |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `PriceLabelGuard` istisnasız; toplu deyimle yazılan satır kapsam dışı olduğu sınıf yorumunda açık (öteki bekçilerin kalıbı) |
| Sık FAIL 2: tarama bir çağrı biçimini kaçırıyor | uygulanamaz | yeni tarama yok |
| Sık FAIL 3: 06 varsayılanı / FK dizini | uygulanamaz | şema değişmedi |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `Money` sınıf yorumu ("dördüncü işlev T369"), D-74 metni, backlog `Aktif kalem` 130 → 132 → 137 (T316 birleştirmesi; satırlar sayılarak: ⚪ 134 + 🟡 3) → 142 (T334 birleştirmesi ve D-381; satırlar sayılarak), status satırı ve "Son güncelleme", sekiz raporun Post-merge satırı |
| Sık FAIL 5: modül yöntemi kendisi `SaveChanges` | ✓ | `GetPriceLabelAsync` düz okuma; öteki üçü saf |
| Sık FAIL: tek kural evi yerine özel denetim | ✓ | İkinci devam turunda bulundu ve düzeltildi: `ItemPricing.IsValidPrice` fiyat kuralını, `HasAtMostDecimals` ondalık sayımını ayrıca yazıyordu; şimdi `ItemPricing.cs` `PriceField.Allows(price)` (`InvalidAmountAttribute`) ve `FieldRuleAttribute.HasAtMostDecimals`'a yönlenir; `APrice_IsAboveZeroWithAtMostTwoDecimals_InTheRuleAndInTheFormsAmountField` ikisinin aynı kalmasını sınar |

### Öz denetim — kod birleştirmesi 2 (`origin/main` T321 `a89710b` ile birleşik ağaç)

> `11` T336 kabul kriterleri ve doğrulama kontrol listesi (satır 2588–2611) birleşik ağaçta yeniden denetlendi; yukarıdaki tablolardaki bütün test adları ve semboller `git grep -n` ile birleşik ağaçta yeniden bulundu (eksik 0).

| Madde | Sonuç | Kanıt (birleşik ağaç, dosya:satır) |
|---|---|---|
| k1 / KK1 | ✓ | `PriceAndVatTests.cs:62` `ThePriceTyped_IsThePriceTheLabelShows_VatIncluded`, `:126` `APriceWithThreeDecimals_IsRefusedBeforeTheColumnRoundsIt` |
| k2 / KK2 | ✓ | `PriceAndVatTests.cs:83` `TheVatRate_IsAProductColumn_AndNoVariantColumn`, `:90` `TheDefaultRate_ComesFromTheSetting_…` |
| k3 / KK3 | ✓ | `PriceAndVatTests.cs:108` `TheOrderItem_KeepsItsVatAmount_…`; `ItemPricingTests.cs:86`, `:97`, `:116` (kargo) |
| k4 / KK4 | ✓ | `ItemPricingTests.cs:50` `TheItemAmount_IsNotRounded_…`, `:59` `TheOrderTotal_IsTheSumOf…`, `:173` `RandomItems_KeepTheOrderAndAddUp` |
| k5 / KK5 | ✓ | `ItemPricingTests.cs:20` `AnItemWithACoupon_…`, `:29` `TheVatOfACouponItem_…`, `:74` `TheOrderTotal_TakesTheCouponOffEachItem` |
| k6 / KK6 | ✓ | `ItemPricingTests.cs:126` `RoundingDirection_…`; `MoneyTests.cs:179` `VatOf_AnExactHalfKurus_GoesUp`; `Money.cs:41` `VatOf` |
| k7 / KK7 | ✓ | `ItemPricing.cs:18` `AmountDecimals = 2`, `:27` `IsValidPrice` → `PriceField.Allows`; `ItemPricingTests.cs:144`; `PriceAndVatTests.cs:144` `APriceOfZero_IsRefusedByTheDatabase` |
| k8 / KK8 | ✓ | `ProductDefinitionGuard.cs:55` `PhysicalProductWithoutOrigin`, `:57` `OriginOnAService`; `PriceLabelRules.cs:34`, `:37`, `:40`, `:77`; `PriceLabelReader.cs:54`, `:58`; `PriceAndVatTests.cs:159`–`:311` (12 KK8 testi) |
| Kesici listesi | ✓ | `AppDbContext.cs:252` `PriceLabelGuard.Instance`, `:253` `VariantStockGuard.Instance` (iki taraf birlikte) |
| Modül yüzü | ✓ | `CatalogModule.cs` kurucusu `_priceLabels` ve T321'in `_showcase`'i birlikte atar; `GetPriceLabelAsync` (`:65`), `PriceItem`, `SplitShippingFee`, `SumOrderTotal` ve T321'in `ListShowcaseAsync`, `ListOptionVariantsAsync`'i aynı sınıfta |
| Main testlerinin anlamı | ✓ | `ProductTransitionsTests.cs:634`, `:701` yalnız `.WithSeededOrigin()` aldı (tohum); geçiş, vitrin ve sepet beklentileri değişmedi; 41 / 41 |
| `09 §25` | ✓ | şema/göç yok, paket yok, para yuvarlaması yalnız `Money`, yeni arayüz yöntemi yok; Architecture 933 / 933, Consistency 241 / 241 |
| Güvenlik taraması | ✓ | birleştirme yeni uç, sır ya da yetki yolu eklemedi |
| Sık FAIL: main'in yeni zorunlu alanını tohumlamama | ✓ | T321'in ürün yazan tek yeni testi `ProductTransitionsTests` uyarlandı; `src` altında T321'in ürün yaratan yolu yok (`git grep "Product.Create("`) |
| Sık FAIL: aynı sayıyı anan yerler | ✓ | backlog `Aktif kalem` satırlar sayılarak 148 (⚪ 145 + 🟡 3); status "Son güncelleme" yenilendi, eski iki giriş (T336 birleştirme 1, T321 tur 3) `STATUS_CHANGELOG.md` başında |
| Sık FAIL: birleştirmede aynı işi yapan iki yol | ✓ | çakışma yalnız kurucuda iki atama; iki tarafın yöntemleri ayrı işler yapar |
| Sık FAIL: belgede olmayan sabit | ✓ | `AmountDecimals` (2) ve `NetQuantityDecimals` (3) `06`'nın `numeric(12,2)` / `numeric(12,3)` sütun hassasiyeti; `09 §3.5`'e yeni tavan/süre girmedi |
| Para kodu: yuvarlama yönü ve kuruş testle | ✓ | `RoundingDirection_…`, `VatOf_AnExactHalfKurus_GoesUp`, `APrice_IsAboveZeroWithAtMostTwoDecimals_…`, `RandomItems_KeepTheOrderAndAddUp` |

### Öz denetim — kod birleştirmesi 3 (`origin/main` T339 `9bb46eb` + T341 `e2e9dbc` ile birleşik ağaç)

> `11` T336 kabul kriterleri ve doğrulama kontrol listesi (satır 2588–2611) birleşik ağaçta yeniden denetlendi; aşağıdaki test adları ve semboller birleşik ağaçta `grep -n` ile yeniden bulundu (eksik 0, satır numaraları birleştirme 2 ile aynı).

| Madde | Sonuç | Kanıt (birleşik ağaç, dosya:satır) |
|---|---|---|
| k1 / KK1 | ✓ | `PriceAndVatTests.cs:62` `ThePriceTyped_IsThePriceTheLabelShows_VatIncluded`, `:126` `APriceWithThreeDecimals_IsRefusedBeforeTheColumnRoundsIt` |
| k2 / KK2 | ✓ | `PriceAndVatTests.cs:83` `TheVatRate_IsAProductColumn_AndNoVariantColumn`, `:90` `TheDefaultRate_ComesFromTheSetting_…` |
| k3 / KK3 | ✓ | `PriceAndVatTests.cs:108` `TheOrderItem_KeepsItsVatAmount_…`; `ItemPricingTests.cs:86`, `:97`, `:116` (kargo) |
| k4 / KK4 | ✓ | `ItemPricingTests.cs:50` `TheItemAmount_IsNotRounded_…`, `:59` `TheOrderTotal_IsTheSumOf…`, `:173` `RandomItems_KeepTheOrderAndAddUp` |
| k5 / KK5 | ✓ | `ItemPricingTests.cs:20` `AnItemWithACoupon_…`, `:29` `TheVatOfACouponItem_…`, `:74` `TheOrderTotal_TakesTheCouponOffEachItem` |
| k6 / KK6 | ✓ | `ItemPricingTests.cs:126` `RoundingDirection_…`; `MoneyTests.cs:179` `VatOf_AnExactHalfKurus_GoesUp`; `Money.cs:41` `VatOf` |
| k7 / KK7 | ✓ | `ItemPricing.cs:18` `AmountDecimals = 2`, `:27` `IsValidPrice`; `ItemPricingTests.cs:144`; `PriceAndVatTests.cs:144` `APriceOfZero_IsRefusedByTheDatabase` |
| k8 / KK8 | ✓ | `ProductDefinitionGuard.cs:55` `PhysicalProductWithoutOrigin`, `:57` `OriginOnAService`; `PriceLabelRules.cs`, `PriceLabelReader.cs` birleştirmede değişmedi; `PriceAndVatTests.cs:159`–`:311` (12 KK8 testi) |
| Kesici listesi | ✓ | `AppDbContext.cs:252` `PriceLabelGuard.Instance`, `:253` `VariantStockGuard.Instance` |
| Modül yüzü | ✓ | `CatalogModule.cs:24`–`:25` `_priceLabels` ve T339'un `_gallery` alanı birlikte; kurucu ikisini alır; `:68` `GetPriceLabelAsync`, `PriceItem`, `SplitShippingFee`, `SumOrderTotal` ve `:77` T339'un `SaveProductGalleryAsync` ile dört galeri yöntemi aynı sınıfta; DI `:155` `PriceLabelReader`, `:156` `ProductGallery`; `ICatalogModule.cs:144` / `:166` iki tarafın yöntemleri; `SitemapSourceTests` sahte modülü iki tarafın `NotSupportedException` yöntemlerini birlikte taşır |
| Main testlerinin anlamı | ✓ | `ProductGalleryTests.cs:350` `NewProduct` yalnız `.WithSeededOrigin()` aldı (tohum); `SearchEngineDataTests` tohumu `.WithSeededOrigin()` + T339'un `SetDescription(...)` satırı; galeri ve arama motoru beklentileri değişmedi; 18 / 18, 28 / 28 |
| `09 §25` | ✓ | şema/göç yok, paket yok, para yuvarlaması yalnız `Money`; birleştirme yeni yöntem yazmadı; Architecture 955 / 955, Consistency 241 / 241 |
| Güvenlik taraması | ✓ | birleştirme yeni uç, sır ya da yetki yolu eklemedi |
| Sık FAIL: main'in yeni zorunlu alanını tohumlamama | ✓ | main tarafının ürün kaydeden yeni yardımcısı yalnız `ProductGalleryTests.NewProduct` (uyarlandı); `ProductTextTests.NewProduct` birim testi, veritabanına yazmaz; T341 yalnız sipariş kalemi tohumlar; `src` altında `Product.Create(` yok |
| Sık FAIL: aynı sayıyı anan yerler | ✓ | backlog `Aktif kalem` satırlar sayılarak 164 (⚪ 161 + 🟡 3; main başlığındaki 153 satır sayımıyla tutmuyordu); status "Son güncelleme" yenilendi, eski iki giriş (T336 birleştirme 2, T341 tur 3) `STATUS_CHANGELOG.md` başında |
| Sık FAIL: birleştirmede aynı işi yapan iki yol | ✓ | çakışmalar yalnız ekleme sınırında; iki tarafın yöntemleri ayrı işler yapar (fiyat etiketi / galeri) |
| Para kodu: yuvarlama yönü ve kuruş testle | ✓ | `RoundingDirection_…`, `VatOf_AnExactHalfKurus_GoesUp`, `APrice_IsAboveZeroWithAtMostTwoDecimals_…`, `RandomItems_KeepTheOrderAndAddUp`; para kuralı değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 4115 / 4115 geçti (Consistency ve Architecture dahil; T316 birleştirmesi ve tek kural evi düzeltmesinden sonra yeniden koşuldu) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests -- --filter-namespace …` / `--filter-class …` (parçalı, yerel PostgreSQL 18) | 2133 geçti, 0 kaldı; yeni `PriceAndVatTests` 19 / 19 (T316 birleştirmesinden sonra yeniden 19 / 19; T316'nın yeni Ordering testleri ürün satırı yazmaz, üretim yeri uyarlaması gerekmedi). `LivenessEndpointTests`'in erişilemeyen sunucuya bağlanan bir testi yerel Windows ortamında askıda kaldı (T336'nın dokunmadığı sınıf; CI Linux'ta koşar) |
| Contract | uygulanamaz | — | — |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | değişiklik yok |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulama chat'i doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T336-fiyat-kdv`
- **Commit:** `7c58024` (kod), `abafdab` (entegrasyon testleri), belgeler commit'i — son hali PR'da
- **PR:** #444
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; ayrı `chore:` PR'ında veya sonraki task dalında)*

## Dış Varsayımlar

Dış varsayım: yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ülke adının Ayarlar listesinden okunması, ürün sayfasında satır ve yerli üretim logosu; formda ülke seçimi | D-344 | gösterim T503, form T525 |
| 2 | İndirimdeki varyantın birim fiyatı indirimli fiyattan; fiyat geçmişine satır yazımı | D-345 | indirim ve fiyat geçmişi T337 |
| 3 | Ürün formunda KDV oranının ayardan önerilmesi ve fiyat etiketi alanlarının alan mesajları | D-346 | ürün formu T525 |
| 4 | `Money.ShareOfUnits` | D-74 (kısmi) | T369 |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`. Devam turunda da temiz başlandı (`origin/task/T336-fiyat-kdv` head `7c58024`, `origin/main` birleştirildi, çakışma yok). İkinci devam turunda `origin/main` (T316 `924ef92`) yeniden birleştirildi; status ve backlog çakışması elle çözüldü (T315 Commit hücresi korunarak T316 satırı main'den, Aktif kalem yeniden sayıldı), T316'nın "Son güncelleme" girişi `STATUS_CHANGELOG.md` başına taşındı.
- **Ana dal CI startup (Adım 0):** ilk tur 42401437836, 42401437807 (T322 `f9ea0fe`), 42398252751 — success. Devam turu: 42405002224, 42405001882 (T328 `fe78e7b`), 42402532255 — success.
- **Main birleştirmesi (T334, `af6e7d3`) — doğrulama tur 1 BLOCKED sonrası:** `origin/main` dala birleştirildi (rebase yok). Kod çakışmaları: `AppDbContext.cs` kesici listesinde `PriceLabelGuard.Instance` ve `VariantStockGuard.Instance` ikisi de kaldı; `ConcurrencyTests`, `ImageFilesTests`, `CartAndOrderCoreTablesTests`, `StockReservationAndHistoryTablesTests` tohumu iki tarafı birlikte taşır (`.WithSeededOrigin()` + `Variant.Create(..., 100)`). Anlamsal çakışmalar (yalnız tohum, davranış ve tutar/KDV mantığı değişmedi): `StockAndCapacityTests` fiziksel ürünü `WithSeededOrigin()` ile açar, tip değişimi fiziksele dönerken de üretim yeri verir (beklenen ret stok kuralından gelsin diye); `PriceAndVatTests` fiziksel varyantlara stok adedi (100) verir (T334'ün "fiziksel varyantın stok adedi var" bekçisi). Kayıt belgeleri: status "Son güncelleme" T336 girişi, T334 girişi `STATUS_CHANGELOG.md` başına; backlog iki tarafın kayıtları + D-381 (doğrulayıcı K1, T525), Aktif kalem satırlar sayılarak 142; sekiz raporun Post-merge satırı main'den. Birleşik dalda: derleme `-warnaserror` 0 uyarı, format üç proje temiz, Unit 4159 / 4159, betik testleri 4 / 4, Integration 2267 / 2267 (parçalı, yerel PostgreSQL 18). ⚠ `StatusEndpointTests` (heartbeat tablosunu boşaltır) aynı koşuda öteki web sınıflarıyla birlikte koşunca onlar "HeartbeatRecord row is missing" ile düşüyor, ayrı koşuda hepsi geçiyor (iki tarafın dokunmadığı sınıflar). `LivenessEndpointTests` 14 / 14 geçti ama süreç yerelde askıda kaldı (zaman aşımı). Öz denetim ve kabul kriteri tablolarındaki bütün test adları ve semboller birleşik dalda `grep -n` ile yeniden bulundu.
- **Kod birleştirmesi 2 (T321, `a89710b`) — doğrulama tur 2 BLOCKED sonrası:** `origin/main` dala birleştirildi (rebase yok). Kod çakışması: `CatalogModule.cs` kurucusunda `_priceLabels = priceLabels;` ve `_showcase = showcase;` ikisi de kaldı. Anlamsal çakışma (yalnız tohum, iki tarafın davranışı ve tutar/KDV mantığı değişmedi): T321'in `ProductTransitionsTests` tohumu (`SeedSameMomentAsync`, `SeedAsync`) fiziksel ürünü `.WithSeededOrigin()` ile açar (tur 2'de 39 / 41 bu yüzden kalıyordu). Kayıt belgeleri: status "Son güncelleme" yeni T336 girişi, eski T336 ve T321 tur 3 girişleri `STATUS_CHANGELOG.md` başına; tablo iki taraf; backlog main kayıtları (D-353 dahil) aynen + dalın D-344…D-346, D-381; Aktif kalem satırlar sayılarak 148; MEMORY iki tarafın satırları. Birleşik dalda: derleme `-warnaserror` 0 uyarı, `dotnet format --verify-no-changes` temiz, Unit 4172 / 4172 (Architecture 933, Consistency 241, geri kalan 2998), Integration parçalı yeşil: `ProductTransitionsTests` 41 / 41, `PriceAndVatTests` + `StockAndCapacityTests` 46 / 46, `SitemapEndpointTests` + `SearchEngineDataTests` + `ContentStatusTests` 62 / 62 (yerel PostgreSQL 18). Çakışma işareti taraması boş.
- **Kod birleştirmesi 3 (T339 `9bb46eb`, T341 `e2e9dbc`) — doğrulama tur 3 BLOCKED sonrası:** `origin/main` dala birleştirildi (rebase yok). Kod çakışmaları, iki tarafın eklemeleri birlikte: `CatalogModule.cs` (alan, kurucu parametresi `PriceLabelReader priceLabels, ProductGallery gallery`, yöntemler, DI kaydı), `ICatalogModule.cs` (T336'nın 4 yöntemi + T339'un 5 galeri yöntemi), `SitemapSourceTests.cs` (sahte modülde iki tarafın yöntemleri), `SearchEngineDataTests.cs` (`Product.Create(...).WithSeededOrigin()` + T339'un `SetDescription(...)` satırı). Anlamsal çakışma (yalnız tohum, iki tarafın davranışı ve tutar/KDV mantığı değişmedi): T339'un `ProductGalleryTests.NewProduct` yardımcısı fiziksel ürünü `.WithSeededOrigin()` ile açar. Kayıt belgeleri: status "Son güncelleme" yeni T336 girişi, eski T336 birleştirme 2 ve T341 tur 3 girişleri `STATUS_CHANGELOG.md` başına (birleştirmenin 2026-11-08 girişlerinin üstüne düşürdüğü 2026-11-07 T336 girişi tarih sırasına alındı); tablo iki taraf; backlog main kayıtları (D-364, D-365, D-379, D-385 dahil) aynen + dalın D-344…D-346, D-381; Aktif kalem satırlar sayılarak 164 (⚪ 161 + 🟡 3); MEMORY iki tarafın satırları. Birleşik dalda: derleme `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` web projesi ve elle çözülen test dosyaları temiz; Unit 4286 / 4286 (Architecture 955, Consistency 241); Integration parçalı yeşil (yerel PostgreSQL 18): `ProductGalleryTests` 18 / 18, `PriceAndVatTests` + `StockAndCapacityTests` 46 / 46, `ProductTransitionsTests` 41 / 41, `SearchEngineDataTests` + `SitemapEndpointTests` 28 / 28, ve fiziksel ürün kaydeden yardımcıları taşıyan sınıflar: `CatalogCoreTablesTests` + `SalesModelTests` 88 / 88, `ImageFilesTests` + `AppCacheTests` 28 / 28, `VersionedFormPageTests` + `ConcurrencyTests` 47 / 47, `BranchContentLinkAndContactRequestTablesTests` + `StockReservationAndHistoryTablesTests` 67 / 67, `CartAndOrderCoreTablesTests` 65 / 65, `ListPagingTests` + `ListPatternPageTests` (`ListSeed`) 64 / 64. Çakışma işareti taraması boş.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Önceki task'ların merge kaydı:** T301 (`a3c9d1a`), T307 (`7150d1a`), T315 (`8caf0f0`), T322 (`f9ea0fe`), T328 (`fe78e7b`), T332 (`c4c872c`), T366 (`11c68fb`), T381 (`ac7d43e`) — status Commit hücreleri ve raporların "Post-merge run'lar" satırı dolduruldu. T316 (`924ef92`): status Commit hücresi dolduruldu; ana dal CI'ı (42408991723) bu turda henüz sürdüğünden raporun "Post-merge run'lar" satırı boş bırakıldı (Publish 42408991705 success).
- **⚠ Kolay kararlar (kurala en uygun seçenekle verildi):**
  1. Kargo ücreti bölüşümünün ağırlığı fiziksel kalemin kupondan önceki tutarıdır (birim fiyat × adet). `02 §3.19.2` "bu kalemlerin tutarına orantılı" der; tutar kalemin `02 §3.8.3` adım 1'deki tutarı olarak okundu.
  2. Fiyatın uygulandığı tarih, başlangıcı "şimdi"ye kadar gelmiş son `PriceHistory` satırıdır; gelecek tarihli satır sayılmaz (aynı başlangıçta kimliğe göre sonuncu).
  3. Etiket üretim yerini ülke adı yerine `OriginCountryId` olarak verir; ad Ayarlar modülünün listesindedir (modül sınırı) — D-344.
  4. Fizikselde üretim yeri taslak dahil her kayıtta zorunludur (`06 §4.5` "fiziksel üründe zorunlu (uygulamada)"; formun alanı zorunlu, `04` 7.2.4.12). Bu yüzden fiziksel ürün kaydeden var olan entegrasyon testleri tohumlanmış ülkeye bağlandı.
  5. `VersionedFormPageTests.SeedOrderAsync` yasal metin sürüm numarasını sayaçtan değil kayıtlı en büyük numaradan türetir: uygulamanın açılışı da sürüm yazar (T366) ve test sırası yerelde farklı olunca tekil dizine çarpıyordu. Yalnız test tohumu; ürün kodu değişmedi.
