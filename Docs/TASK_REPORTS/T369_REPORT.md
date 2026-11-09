# T369 — İşlemin adet birimi ve geri ödeme tutarı: açık adet, kupon payı, KDV, kargo ücretinin iptalde ve caymada ayrılması

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Açık adet (k1, k2):** `IPaymentModule.RescindedSharesAsync` (+ `RescindedShares`) geçersizleşmemiş `DefectClaimRescission` iadelerinin adet, kupon payı ve KDV toplamını verir (kaydedilmiş + bağlamdaki). `AfterSalesModule.OpenQuantityAsync` dönülen adetleri düşer (K-1077); `SetAsideAsync` dönmenin payını da "önceden ayrılan" sayar, böylece son açık adetleri kapatan kayıt dönmenin bıraktığı kalanı alır. İptal, çıkarma, gecikme feshi ve cayma beyanının kapattığı adetler T367/T368'deki `ClosedQuantitiesAsync`'ten okunur.
- **Pay ve geri ödeme (k3, k4, k6):** T367'nin tek kural evi `ItemShares.SetAside` (`Money.ShareOfUnits`, aşağı yuvarlama; son kayıt kalanı alır) kullanılır; kısmi iptalde kupon yeniden dağıtılmaz (pay kalemin donmuş payından oranla). Geri ödeme tutarı tek yerde: `RefundAmount.Amount` = ürün bedeli − kupon payı.
- **Parçalı teslim alma (k5):** `ItemShares.SetAsideParts` — beyanın payı parça adedi / beyan adediyle aynı yuvarlamayla bölünür; beyanın son adetlerini geri ödeyen parça kalanı alır; ret/"mal dönmedi" adedi varken hiçbir parça kalanı almaz.
- **Kargo ücreti (k7, k8):** saf kural evi `ShippingFeeRefunds` — kargodan önce bütün fiziksel adetlerin son iptali (ya da çıkarması), S9'da müşteri kaynaklı olmayan sebep, fesihte her zaman, caymada kargoyla gönderilen bütün adetler dönünce son dönüş (teslimden önce beyan, sonra teslim alma); aynı anda birden çok kayıtta en küçük kalem uuid'si (K-1107). Ret ya da yeniden açılmamış "mal dönmedi" adedi varsa eklenmez.
- **Modül:** `IAfterSalesModule.RefundAmountAsync(RefundSubject)` ve `IsShippingFeeIncludedAsync(ShippingFeeQuery)` (`AfterSalesModule.RefundAmounts.cs`; sözleşmeler `Contracts/RefundAmounts.cs`). `RefundSubject(RefundType, RecordId, ReturnId?)` 06'daki `Payment.Contracts.RefundType`'ı kullanır — yeni enum yok.
- **03 4.2.6 kaydı (D-697 K2'nin T369 payı):** `AutomaticMoments` "Kısmi iptal ve iade" anı artık veri üyesi (`Refund.IsShippingFeeIncluded`) yerine kuralın eylemlerini anar: `AfterSalesModule.IsShippingFeeIncludedAsync`, `ShippingFeeRefunds.OnCancellationBeforeShipment`, `ShippingFeeRefunds.OnWithdrawal`, `AfterSalesModule.RefundAmountAsync`.

## Etkilenen Modüller / Dosyalar

- AfterSales: `AfterSalesModule.cs` (kurucuya `IPaymentModule`), `AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.Cancellations.cs`, `AfterSalesModule.RefundAmounts.cs` (yeni), `ItemShares.cs`, `ShippingFeeRefunds.cs` (yeni), `Contracts/IAfterSalesModule.cs`, `Contracts/RefundAmounts.cs` (yeni)
- Payment: `PaymentModule.cs`, `Contracts/IPaymentModule.cs`, `Contracts/PaymentFacts.cs`
- Infrastructure: `Jobs/AutomaticMoments.cs`
- Testler: `tests/Shopfolio.UnitTests/AfterSales/RefundAmountRulesTests.cs`, `tests/Shopfolio.IntegrationTests/Ordering/RefundAmountTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İşlem birimi adet; açık adedi aşamaz; kapanan adetler düşülür | ✓ | `OpenQuantityAsync`; Integration `KK1_RescindedUnits_LeaveTheOpenQuantity_…`, `KK1_AVoidedRescission_GivesItsUnitsBack`; T367/T368 ItemRecords testleri (iptal/çıkarma/fesih/cayma) yeşil |
| 2 | Kapanan adet başka işleme girmez; ayıpta dönülen adet de düşer | ✓ | Integration `KK2_TheUnitsOneRecordClosed_AreNoOtherRecordsSubject`, `KK1_RescindedUnits_…` |
| 3 | Ürün bedeli = donmuş birim fiyat × adet; kupon ve KDV oranla, aşağı yuvarlanır | ✓ | Unit `KK3_ThreeUnitsOf300WithACoupon100_…`; Integration `KK3_ThreeSingleUnitCancellations_Refund266_67_…`; `MoneyTests.ShareOfUnits_*` |
| 4 | Son açık adetleri kapatan kalanı alır; paylar kuruşu kuruşuna eşit | ✓ | Unit `KK4_WhateverTheSplit_TheSharesAddUpToTheItemsShareAndVat_…` (4 bölünüş); Integration `KK1_RescindedUnits_…` (dönme payı dahil toplam = kalemin payı) |
| 5 | Parçalı teslim almada parça payı beyanın payından orantıyla; son parça kalanı | ✓ | `ItemShares.SetAsideParts`; Unit `KK5_*` (4 test); Integration `KK5_EachReceiptOfAWithdrawal_RefundsItsPartOfTheDeclaration_TheLastTheRest` |
| 6 | Kısmi iptalde kupon yeniden dağıtılmaz | ✓ | Unit `KK6_APartialCancellation_SetsAsideOnlyItsUnitsShare_…` |
| 7 | Kargo: iptalde tetik, bir adet gidecekse alıkonur, bölünmez; S9'da müşteri kaynaklı sebepte geri ödenmez; fesihte hep | ✓ | `ShippingFeeRefunds`; Unit `KK7_*` (6 test, iki kalemde en küçük uuid dahil); Integration `KK7_*` (4 test) |
| 8 | İadede kargo yeniden hesaplanmaz; son cayma/son teslim almaya eklenir; ret/"mal dönmedi" varsa eklenmez | ✓ | Unit `KK8_*` (4 test); Integration `KK8_TheReceiptOfTheLastShippedUnits_…`, `KK8_ARejectedUnit_…`, `KK8_AWithdrawalBeforeTheDelivery_…`. Kural yalnız "eklenir mi" kararı verir; ücret siparişin donmuş kargo ücretidir, yeniden hesap yok |

### Öz denetim (bitiş kapısı 2)

Devam işçisi bütün maddeleri (önceki işçinin yaptıkları dahil) diff'te yeniden denetledi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| k1 / KK1 açık adedi aşan reddedilir | ✓ | `RefundAmountTests.KK1_*` (`QuantityExceedsOpen`); `AfterSalesModule.ItemRecords.cs` `OpenQuantityAsync` |
| k2 / KK2 kapanmış adet ikinci işleme girmez | ✓ | `RefundAmountTests.KK2_TheUnitsOneRecordClosed_AreNoOtherRecordsSubject` |
| k3 / KK3 33,33/33,33/33,34 — 266,67/266,67/266,66 | ✓ | `RefundAmountRulesTests.KK3_*`, `RefundAmountTests.KK3_*` |
| k4 / KK4 paylar toplamı kalemin payı | ✓ | `RefundAmountRulesTests.KK4_*` |
| k5 / KK5 parçalı teslim almada orantı | ✓ | `RefundAmountRulesTests.KK5_*`, `RefundAmountTests.KK5_*` |
| k6 / KK6 kısmi iptalde kupon sabit | ✓ | `RefundAmountRulesTests.KK6_*` |
| k7 / KK7 kargo iptalde tetik, S9 sebep, fesih hep | ✓ | `RefundAmountRulesTests.KK7_*`, `RefundAmountTests.KK7_*` |
| k8 / KK8 iade: yeniden hesap yok, son teslim almaya, ret/"mal dönmedi" eklenmez | ✓ | `RefundAmountRulesTests.KK8_*`, `RefundAmountTests.KK8_*` |
| `09 §25` doküman uyumu: yeni ad 06/sözlükten | ✓ | Kayıt alanları 06 §4.33 adlarıyla (`Quantity`, `ProductAmount`, `CouponShare`, `VatAmount`); `RefundSubject` 06'daki `RefundType`'ı kullanır, yeni enum/kapalı değer yok (kilit dosyası değişmedi) |
| `09 §25` modül sınırı, arayüzde `SaveChanges` yok | ✓ | Yeni arayüz yöntemleri yalnız okur (`LoadAsync` + `Local`); `ModuleBoundaryTests` yeşil |
| `09 §25` tek işlev | ✓ | Pay `ItemShares.SetAside`/`Money.ShareOfUnits`; geri ödeme tutarı yalnız `RefundAmount.Amount` (devamda `ItemShares.RefundOf` ikinci kopyası kaldırıldı); dönem ayrımı `AfterSalesPeriodRules.IsAfterDelivery` |
| `09 §25` para hesabı yalnız `Money` | ✓ | Yuvarlama `Money.ShareOfUnits` üzerinden; kalan çıkarma işlemleri yuvarlamasız |
| `09 §25` zaman `TimeProvider` | ✓ | Yeni kodda saat okuması yok; anlar kayıtlardan ve çağırandan (`HandedToCarrierAt`) |
| `09 §25` göç / yeni tablo / yeni ayar | ✓ | Yok |
| Kurucuya yeni bağımlılık → bütün DI düzenekleri | ✓ | `AddAfterSalesModule` kullanan 40 dosyanın hepsinde `AddPaymentModule` var; `new AfterSalesModule(` yok; Integration tam koşu yeşil |
| Aynı sayıyı/adı anan öteki yerler | ✓ | 03 4.2.6 kaydı (`AutomaticMoments`) kuralın eylemlerine çevrildi; `AutomaticMomentsTests` (Unit + Integration) yeşil |
| Devredilen backlog kalemleri | ✓ | D-74 (`ShareOfUnits` → T369) yerinde kapatıldı; D-697 K2'nin T369 payı yapıldı, T373 payı açık |
| Güvenlik taraması | ✓ | Sır yok; yeni uç/sayfa yok; girdi yalnız modül içi sözleşmelerden (`ArgumentNullException`, bilinmeyen tipte `ArgumentOutOfRangeException`); yeni paket yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --filter-not-trait "Consistency=true" --max-threads 3` | 5275/5275 |
| Unit (Consistency) | ✓ | `--filter-trait "Consistency=true"` | 291/291 |
| Integration | ✓ | Ordering 476/476; Disposal+Commands+Payment+Model 1543/1544 (1 test `SchedulerBackupDisposalAndKeyTablesTests` yük altında Npgsql okuma zaman aşımı; sınıf tek başına 55/55); kalan ad alanları 1220/1220; devam düzeltmesinden sonra `RefundAmountTests` + `AutomaticMomentsTests` 17/17 | yeşil |
| Lint | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | temiz (devamda `AfterSalesModule.RefundAmounts.cs` girintisi düzeltildi) |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror` | 0 uyarı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T369-geri-odeme-tutari`
- **Commit:** `4ffde5ac` (ara kayıt), `2dcbeb28` (devam), rapor commit'i
- **PR:** #501
- **CI:** yardımcı izler
- **Post-merge run'lar:**

## Dış Varsayımlar

Dış varsayım: yok (tetikleyici tarama: kabul kriterleri ve plan metninde plan tier, kota, API key, paket sürümü, sandbox vb. terim geçmiyor; iş yalnız modül içi hesap).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sipariş işlemleri ekranının açık adedi (`OrderOperations`) dönülen adetleri düşmüyor; yazıcı reddeder | D-842 | Ayıp dönme yazıcısı T374'te |
| 2 | Geri ödeme satırını yazan hat `RefundAmountAsync` ve `IsShippingFeeIncludedAsync`'i kullanmalı (bugün çağıran yok) | D-843 | Geri ödeme kaydı T370'te |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; dal `origin/task/T369-geri-odeme-tutari` `4ffde5ac`'tan alındı, `origin/main` `9d0d611c` çakışmasız birleştirildi).
- **Hook:** `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks`.
- **Ana dal CI startup (Adım 0):** 42619534143 success, 42619534329 success (T377), 42616087470 success (T329).
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- **⚠ kararlar (kurala en uygun seçenekle):**
  1. "Kargodan önce/sonra" ayrımı için kargoya verilme anı çağırandan gelir (`ShippingFeeQuery.HandedToCarrierAt`, "Kargoya verildi" geçişinin anı); kaydın anı bu andan önceyse kargodan önce sayılır.
  2. Teslimden önceki caymada geri ödeyen kayıt beyandır, teslimden sonra teslim almadır (K-1057); ayrım `AfterSalesPeriodRules.IsAfterDelivery`.
  3. Ret ya da yeniden açılmamış "mal dönmedi" adedi varsa kargo ücreti hiçbir geri ödemeye eklenmez (`02 §7.4.6`; K-1076).
  4. Çıkarma kargodan önce iptalin tetiğini izler, kargodan sonra kargo ücreti taşımaz.
  5. D-820 (T368, hedef T374): `OpenQuantityAsync` dönülen adetleri artık düşüyor; dönme yazıcısı ve `ClosedQuantitiesAsync`'in kamu okuması T374'te kalır.
