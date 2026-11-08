# T349 — Asgari sipariş tutarı, kargo ücreti ve ücretsiz kargo eşiği

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Saf kural `ShippingCharge.Assess(ShippingFeeSection terms, decimal basis, bool hasPhysicalItem)` → `ShippingAssessment`** (`Modules/Ordering/Contracts/ShippingCharge.cs`): tek taban (siparişe giren kalemlerin kupondan önceki indirimli toplamı), fiziksel kalem koşulu, sabit ücret (bir kez; 0 TL her siparişi ücretsiz yapar), isteğe bağlı eşik (eşit ya da üstünde ücret 0), isteğe bağlı asgari tutar (yalnız fiziksel kalemle; eksik tutar), mesajlar (`FreeShippingMessage` "Ücretsiz kargoya N TL kaldı" / "Kargo ücretsiz"; `MinimumOrderMessage` "Sipariş verebilmek için N TL'lik daha ürün ekleyin") ve ret kodu `Refusal` = `ErrorCode.MinimumOrderAmountNotMet` (`07 §6.2`). Saat, tablo, ayar okumaz. "Quote" adı `SalesModelShapeTests` gereği kullanılmadı.
- **Sepet:** `CartLine.ProductType`; `CartView.Shipping`; `CartPaymentBlock.MinimumOrderAmountNotMet`; `CartRules.AssessShipping(lines, terms)` tabanı `OrderableSubtotal`'dan okur, fiziksel kalem koşulu yalnız siparişe giren fiziksel kaleme bakar (Tükendi fiziksel kalem ne ücreti ne asgariyi doğurur); `CartRules.PaymentBlockOf(lines, shipping)` eksik tutarda ödeme adımını kapatır.
- **`IOrderingModule.OpenCartAsync(owner, ShippingFeeSection shippingTerms, ct)`:** firma ayarı çağırandan gelir (sayfa `ISettingsModule.GetShippingFeeAsync` okur).
- **Sipariş:** `Order.ShippingFeeVatBreakdown` `List<ShippingFeeVatShare>?` + `HasPayloadConversion()` (D-65); `Order.FreezeShippingFeeVatBreakdown(IReadOnlyList<ShippingVatPart>)` bir kez yazar, parçalar `ShippingFee`'ye eşit olmalı, 0 TL'de boş. Bölüşüm `ICatalogModule.SplitShippingFee` (T332) — yeni bölüşüm hesabı yazılmadı (`09 §24` m.4). Sütun aynı, göç yok.
- **Main birleştirmesi:** `origin/main` (T311, #456) dala alındı; `IOrderingModule.cs` ve `OrderingModule.cs` kendiliğinden birleşti (iki tarafın eklemeleri ayrı yerlerde), çakışma yok.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/Contracts/ShippingCharge.cs` (yeni), `ShippingFeeVatShare.cs` (yeni), `CartRules.cs`, `Carts.cs`, `Contracts/CartContracts.cs`, `Contracts/CartPaymentBlock.cs`, `Contracts/IOrderingModule.cs`, `OrderingModule.cs`, `Order.cs`
- Testler: `tests/Shopfolio.UnitTests/Ordering/ShippingChargeTests.cs` (yeni), `CartRulesTests.cs`; `tests/Shopfolio.IntegrationTests/Ordering/CartTests.cs`, `OrderTransitionsTests.cs`, `Model/CouponTests.cs`
- Doküman: `Docs/DEFERRED_BACKLOG.md` (D-65, D-221 yerinde ✅), `DEFERRED_BACKLOG.d/T346.md` (D-562 ✅), `DEFERRED_BACKLOG.d/T338.md` (D-483'e T349 payı), `DEFERRED_BACKLOG.d/T349.md` (yeni D-626, D-627), `STATUS_CHANGELOG.d/T349.md`, `IMPLEMENTATION_STATUS.md` T349 satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Asgari tutar isteğe bağlı tek; altında sipariş verilemez, eksik tutar gösterilir; sepette ve onayda denetlenir | ✓ | `ShippingCharge.Assess` (`MinimumOrderShortfall`, `MinimumOrderMessage`, `Refusal`); sepet `CartPaymentBlock.MinimumOrderAmountNotMet`; Unit `ShippingChargeTests.UnderTheMinimum_TheOrderIsRefused_WithTheMissingAmount_KK1`, `AtTheMinimum_OrWithoutOne_TheOrderMayBePlaced_KK1`, `AtConfirmation_APriceDropBelowTheMinimum_StopsTheOrder_OnTheSameRule_KK1`, `CartRulesTests.UnderTheMinimum_ThePaymentStepIsClosed_WithTheMissingAmount_T349_KK1`; Integration `CartTests.UnderTheMinimum_TheCartClosesThePaymentStep_AndTellsTheMissingAmount_T349_KK1`. Onaydaki çağrı yeri T348'dir (T348 T349'a bağımlı; ⚠ 2, D-626) |
| 2 | Taban kupondan önceki indirimli toplam; kupon siparişi engellemez | ✓ | Unit `TheCouponShare_DoesNotLowerTheBasis_ButTheDiscountedPriceDoes_KK2_KK7`; Integration `CouponTests.ACouponDoesNotLowerTheBasis_OfTheThresholdAndTheMinimumOrderAmount_T349_KK2` (`CouponApplication.BasketTotal`) |
| 3 | Asgari yalnız fiziksel kalemli siparişe; taban siparişe giren bütün kalemler | ✓ | `Assess` `hasPhysicalItem=false` dalı; Unit `ADigitalOrServiceOrder_HasNoMinimum_NoFee_AndNoMessage_KK3_KK4`, `CartRulesTests.ASoldOutPhysicalLine_CarriesNeitherTheMinimumNorTheFee_NorEntersTheThresholdBasis_T349_KK3_KK6`; Integration `CartTests.ADigitalCart_HasNoMinimumAndNoFee_AndAMixedCartsBasisCountsTheDigitalItem_T349_KK3_KK4_KK7` |
| 4 | Ücret sipariş başına sabit; fiziksel kalem varsa bir kez; 0 TL her siparişi ücretsiz yapar | ✓ | Unit `TheFee_IsTheFirmsOneAmount_WhateverTheBasis_AndZeroIsFreeForEveryOrder_KK4`, `ADigitalOrServiceOrder_…_KK3_KK4`; Integration `CartTests …_T349_KK3_KK4_KK7` |
| 5 | Ücret KDV dahil; farklı oranlarda orantılı bölünür, parçalar kendi oranıyla dökümde | ✓ | `Order.FreezeShippingFeeVatBreakdown` + `ICatalogModule.SplitShippingFee`; Unit `AMixedRateFee_IsSplit_AndTheOrderFreezesTheParts_KK5`, `TheFrozenBreakdown_MustAddUpToTheFee_AndAFreeOrderHasNone_KK5` |
| 6 | Eşik isteğe bağlı; taban eşikte/üstünde ücret yok; siparişe girmeyen kalem kargoya ve tabana girmez | ✓ | Unit `AtAndAboveTheThreshold_TheFeeIsZero_KK6`, `CartRulesTests …_T349_KK3_KK6`; Integration `CartTests.AtTheThreshold_TheFeeIsZero_AndASoldOutLine_StaysOutOfTheFeeAndTheBasis_T349_KK6_KK8` |
| 7 | Eşik tabanı siparişe giren bütün kalemlerin indirimli toplamı; indirim düşürür, kupon düşürmez | ✓ | Unit `TheCouponShare_DoesNotLowerTheBasis_ButTheDiscountedPriceDoes_KK2_KK7`; Integration `CartTests …_T349_KK3_KK4_KK7`, `CouponTests …_T349_KK2` |
| 8 | Eşik ve fiziksel kalem varsa kalan tutar ya da "Kargo ücretsiz"; yoksa mesaj yok | ✓ | `ShippingAssessment.FreeShippingMessage`; Unit `TheThresholdMessage_ShowsOnlyWithAThresholdAndAPhysicalItem_KK8`; Integration `CartTests …_T349_KK6_KK8` |
| 9 | Ücret ve ücretsiz kararı siparişte donar; kısmi iptal/iade yeniden hesaplatmaz | ✓ | `Order.ShippingFee` + `FreezeShippingFeeVatBreakdown` (ikinci çağrı programlama hatası); Unit `TheBreakdown_IsWrittenOnce_ASecondWriteIsAProgrammingError_KK9`; Integration `OrderTransitionsTests.TheCancellation_DoesNotRecalculateTheFrozenShippingFee_NorItsBreakdown_T349_KK9` (değeri veritabanından okur). Geri ödeme hâlleri T369 |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçinin yaptıkları dahil bütün maddeleri birleşik ağaçta (`origin/main` T311 dahil) yeniden denetledi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–9 | ✓ | yukarıdaki tablo |
| KK1: tutar altında sipariş durur, eksik tutar yazar | ✓ | `ShippingChargeTests …_KK1` (3 test), `CartRulesTests …_T349_KK1`, `CartTests …_T349_KK1` |
| KK2: kuponlu sepet tabanı değişmez | ✓ | `ShippingChargeTests.TheCouponShare_…_KK2_KK7`, `CouponTests …_T349_KK2` |
| KK3: dijital sepette asgari yok; karışık sepette taban dijital ve hizmeti içerir | ✓ | `ShippingChargeTests.ADigitalOrServiceOrder_…_KK3_KK4`, `CartTests …_T349_KK3_KK4_KK7` |
| KK4: ücret bir kez, dijital siparişte yok | ✓ | `ShippingChargeTests.TheFee_…_KK4`, `CartTests …_KK3_KK4_KK7` |
| KK5: karışık oranda bölüşüm ve kalan kuruş doğru | ✓ | `ShippingChargeTests.AMixedRateFee_IsSplit_AndTheOrderFreezesTheParts_KK5`, `TheFrozenBreakdown_…_KK5` (kalan kuruş kuralı `ItemPricing.SplitShippingFee`, T332) |
| KK6: eşikte ve üstünde ücret sıfır; Tükendi kalem kargoya ve tabana girmez | ✓ | `ShippingChargeTests.AtAndAboveTheThreshold_TheFeeIsZero_KK6`, `CartRulesTests …_T349_KK3_KK6`, `CartTests …_T349_KK6_KK8` |
| KK7: indirim tabanı düşürür, kupon düşürmez; taban dijital ve hizmeti içerir | ✓ | `ShippingChargeTests.TheCouponShare_…_KK2_KK7`, `CartTests …_KK3_KK4_KK7` |
| KK8: kalan tutar mesajı koşula bağlı | ✓ | `ShippingChargeTests.TheThresholdMessage_…_KK8`, `CartTests …_KK6_KK8` |
| KK9: iptalden sonra ücret yeniden hesaplanmaz | ✓ | `ShippingChargeTests.TheBreakdown_IsWrittenOnce_…_KK9`, `OrderTransitionsTests …_T349_KK9` |
| `09 §25` — doküman uyumu | ✓ | adlar `06 §4.17` (`ShippingFee`, `ShippingFeeVatBreakdown`), `07 §6.2` (`MinimumOrderAmountNotMet`), `02 §11.1` P-1, P-3, P-4 (`ShippingFeeSection`); yeni ayar/sabit yok (`KurusDecimals` `02 §3.8.4` kuruşu); kapsam dışı değişiklik yok |
| `09 §25` — mimari | ✓ | Ordering yalnız `Settings.Contracts` ve `Catalog.Contracts`'a başvurur; modül yönteminde `SaveChanges` yok (`CartTests.TheCartMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK1` yeşil); tek işlev: bölüşüm `SplitShippingFee`, taban `OrderableSubtotal`/`BasketTotal`, tutar biçimi `DisplayFormat.Amount`, kuruş denetimi `FieldRuleAttribute.HasAtMostDecimals`; mimari testler (Unit 4951) yeşil |
| `09 §25` — durum/olay/para | ✓ | durum geçişi yok; para hesabı yeni değil (bölüşüm Catalog'un tek işlevinde); `TimeProvider` gerekmez (saat okunmuyor) |
| `09 §25` — veri ve göç | ✓ | yük biçimi `HasPayloadConversion()` (`09 §12` m.5), sütun ve tip aynı; `dotnet ef migrations has-pending-model-changes` temiz (önceki işçi); yeni tablo/FK yok |
| Güvenlik taraması | ✓ | yeni uç, form, girdi, kişisel veri, sır, kayıt satırı yok; tutar girdileri negatif ve kuruştan ince değerde `ArgumentOutOfRangeException` (`ShippingChargeTests.AnAmountOutsideItsRule_IsAProgrammingError`); ayar çağıranın okuduğu firma bölümüdür, müşteri girdisi değildir |
| `12` VAL-A043, A044, A045, A291, A292 | BEKLEMEDE | ekran ve panel uçları T508/T509/T536'nın; T349 uç yazmaz (D-627). Kural düzlemi yukarıdaki testlerle karşılanır |
| `12` VAL-A797 (bölüşüm) | ✓ | `ShippingChargeTests.AMixedRateFee_…_KK5` + T332 `SplitShippingFee` testleri |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` (birleşik ağaç) | 0 uyarı, 0 hata |
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release` (birleşik ağaç, Consistency dahil) | 4951/4951 |
| Integration — Ordering | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Ordering` (birleşik ağaç) | 289 test yeşil; koleksiyon temizliğinde makine yükünden `DROP DATABASE` zaman aşımı (23 "Collection Cleanup Failure") → `CartTests` sınıfı tek başına 23/23 yeşil, temizlik temiz |
| Integration — Payment | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Payment` (birleşik ağaç) | 14/14 |
| Integration — Web | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Web --max-threads 3` (birleşik ağaç) | 594/594 |
| Integration — Model `CouponTests` | ✓ | `--filter-class …Model.CouponTests` (birleşik ağaç) | 21/21 |
| Integration — öteki parçalar | ✓ | önceki işçi, `96961866` (main birleştirmesinden önce) | Model 1363, Catalog+Disposal+Concurrency 159, Commands..Jobs 339, Logging..SignedEndpoints+kök 93 |
| Format | ✓ | `dotnet format --verify-no-changes` (değişen dosyalar, üç proje; önceki işçi) | temiz; birleştirme T349 dosyalarına dokunmadı |
| Contract | — | — | uç yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulama doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok — `ShippingFeeVatBreakdown` sütunu (`jsonb`) aynı, yalnız EF eşlemesi değer listesine döndü; `has-pending-model-changes` temiz
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T349-asgari-tutar-kargo`
- **Commit:** PR'daki son commit
- **PR:** #476
- **CI:** *(doğrulama doldurur)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

Dış varsayım: yok (dış servis, paket, sürüm ya da platform davranışı kullanılmadı).

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Onayda ayar okuma, `Assess(BasketTotal)`, 303 `/sepet`, `Order.Create(shippingFee)` + `FreezeShippingFeeVatBreakdown` çağrısı | D-626 | Sipariş onayı T348'in; T348 T349'a bağımlı |
| 2 | Sepet ve ödeme özeti sayfası kargo satırı ve mesajlar | D-627 | Sayfalar T508/T509'un |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi: kendi worktree'si, temiz)
- **Ana dal CI startup (Adım 0):** git hooksPath `C:\projects\Shopfolio\scripts\git-hooks`; main son 3 tamamlanmış run success: 42493884421, 42493884474, 42492032713 (önceki işçi)
- **Repo memory (bitiş kapısı 9):** kalıcı öğrenim yok
- **⚠ kararlar (kolay, kurala en uygun seçenek):**
  1. `OpenCartAsync` ayarı parametre olarak alır; Carts'a `ISettingsModule` bağımlılığı eklenmedi — eklemek modülü elle kuran ~12 test düzeneğini kırardı (sık FAIL: modül kurucusuna yeni bağımlılık). Sayfa ayarı `ISettingsModule.GetShippingFeeAsync` ile okur (D-627).
  2. Kriter 1'in "onayda denetlenir" payı: kural ve ret kodu T349'da, onay akışındaki çağrı T348'de (plan bağımlılığı T348 → T349); D-626 ile izlenir.
  3. D-221: `{{ShippingFee}}` sipariş belirteci kalır (ücret siparişte donar; T350'nin süre kararıyla aynı), `RegenerateAsync` çağrısı gerekmez.
- **Diğer:** Ordering koşusundaki 23 "Collection Cleanup Failure" test hatası değil, makine yükünde PostgreSQL okuma zaman aşımı; sınıf tek başına temiz.
