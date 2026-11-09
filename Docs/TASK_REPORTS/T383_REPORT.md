# T383 — Satış özeti ve ölçüler: dönem seçimi, ciro, sayılar, dört mağaza ölçüsü, sıfır payda, tepe gün sayısı

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Administration:** `ISalesSummary` (`GetAsync`, `CountDailyOrdersAsync`), `SalesSummaryPeriod` (hazır dönemler, özel aralık reddi, tek gün), `SummaryRatio` (pay/payda, `IsAssessable`), `SalesSummaryService` (yalnız modül yüzeyleri + `TimeProvider`; tablo, bağlam, saklama, dışa aktarma yok), `SalesSummaryRules` (en çok satan 10, ödeme tamamlama, kargo sözü uyumu, iptal ve iade oranı). `AddAdministrationModule`'a kayıt.
- **Ordering:** `SalesPeriodReader` + `IOrderingModule.ReadSalesPeriodAsync` (dönemin onaylı sipariş sayısı/ciro/yöntem/ürün toplamları veritabanında; kohort kalemleri; sepet denemeleri; süresi dönemde dolan kargo sözleri) ve `FindPaidOrdersOfItemsAsync`.
- **AfterSales:** `ListItemsWithLossInRangeAsync` (iptal/çıkarma kayıt günü, fesih bildirim damgası, cayma beyan günü; düzeltilmiş iptal hariç) ve `FindItemsWithLossAsync`.
- **CorporateContent:** `ContactRequests.SummaryTopics` + `CountSummaryContactRequestsAsync` (yalnız Genel soru, Ürün hakkında, Diğer).
- **Dizinler (devam turu, 09 §20 m5):** özet yedi yeni süzme ekseni getiriyor; her biri dizinini `AddSalesSummaryPeriodIndexes` göçünde getirir — `orders.created_at`, `transition_records(transition_code, created_at)`, `cancellations.created_at`, `item_removals.created_at`, `late_delivery_terminations.declaration_date`, `withdrawals.declaration_date`, `contact_requests.created_at`. `06` §4.17, §4.19, §4.22–§4.25, §4.57 "İndeksler" maddeleri ve §7.5.1 tablosu güncellendi; Model testlerinin dizin listeleri genişletildi.
- **Devam turu düzeltmeleri:** Integration test derleme hataları (tür adı çakışması, `WithHosting` ad alanı, IBAN dondurma, sepetsiz sipariş yerine ayrı sepet — `ck_orders_disposal_fields_together` canlı siparişte `cart_id` ister); iki enum kendi dosyasına taşındı (enum dosyası taraması); iki enum `ApplicationEnums.NotStored`'a eklendi (saklanmaz, istek değeri).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Administration/` — `Contracts/SalesSummaryContracts.cs`, `Contracts/SalesSummaryPreset.cs`, `Contracts/SalesSummaryPeriodRefusal.cs`, `SalesSummaryRules.cs`, `SalesSummaryService.cs`, `AdministrationModule.cs`
- `src/Shopfolio.Web/Modules/Ordering/` — `SalesPeriodReader.cs`, `Contracts/SalesPeriodFacts.cs`, `Contracts/IOrderingModule.cs`, `OrderingModule.cs`, `Order.cs`, `TransitionRecord.cs` (dizin)
- `src/Shopfolio.Web/Modules/AfterSales/` — `AfterSalesModule.SalesSummary.cs`, `Contracts/IAfterSalesModule.cs`, `Cancellation.cs`, `ItemRemoval.cs`, `LateDeliveryTermination.cs`, `Withdrawal.cs` (dizin)
- `src/Shopfolio.Web/Modules/CorporateContent/` — `ContactRequests.cs`, `ContactRequest.cs` (dizin), `CorporateContentModule.cs`, `Contracts/ICorporateContentModule.cs`
- `src/Shopfolio.Web/Infrastructure/Migrations/20261109143919_AddSalesSummaryPeriodIndexes.cs` (+ Designer, snapshot)
- `Docs/06_DATA_MODEL.md` (§4.17, §4.19, §4.22–§4.25, §4.57, §7.5.1)
- Testler: `tests/Shopfolio.UnitTests/Administration/SalesSummaryRulesTests.cs`, `tests/Shopfolio.IntegrationTests/Administration/SalesSummaryTests.cs`, `Model/{CartAndOrderCoreTablesTests,ItemRecordTablesTests,BranchContentLinkAndContactRequestTablesTests}.cs`, `UnitTests/Support/ApplicationEnums.cs`, `UnitTests/Infrastructure/SitemapSourceTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Analitik değil; ziyaretçi verisi/çerez/yeni kişisel veri yok; saklanmaz, dışa aktarılmaz | ✓ | `SalesSummaryService` yalnız `IOrderingModule`, `IAfterSalesModule`, `ICorporateContentModule`, `TimeProvider` alır; tablo/bağlam yok — `SalesSummaryRulesTests.TheSummary_ReadsOnlyTheModulesSurfaces_KeepsNothing_AndHasNoExport_KK1` |
| 2 | Sayı ve ciro ödemesi tamamlanmışları sayar; Başarısız ve bekleyen girmez | ✓ | `SalesPeriodReader.Approved` (Paid/PartiallyRefunded/Refunded) — `SalesSummaryTests.TheCountAndTheRevenue_..._KK2_KK3_KK4_KK10` (failed + pending dışarıda, 4 sipariş) |
| 3 | Ciro onaylanan toplam, geri ödeme düşülmez; iptal/iade ayrı; en çok satanlar ve yöntem dağılımı | ✓ | aynı test: Refunded ve PartiallyRefunded tam `Total` ile; `PaymentMethods`, `TopSellers` doğrulanır; `TheBestSellers_AreTheTopTenByUnits_ThenByAmount` |
| 4 | Sipariş oluştuğu günün dönemine; sonradan ödenen havale de | ✓ | aynı test: havale O1'i iki gün sonra, yine doğduğu günde sayılır; dünkü sipariş dünün özetinde |
| 5 | Aynı sepetin art arda siparişleri tek deneme; ilk siparişin dönemine; süresi dolmamış deneme paydada değil | ✓ | `SalesSummaryRules.PaymentCompletion` — `TheOrdersOfOneCart_UntilOneIsPaid_AreOneAttempt_KK5`, `AnOrderAfterAPaidOne_..._KK5`, `TheAttempt_BelongsToItsFirstOrdersPeriod_..._KK5`; Integration `ThePaymentCompletionRatio_..._KK5` (1/2 → pencere dolunca 1/3) |
| 6 | İletişim talebi yalnız Genel soru, Ürün hakkında, Diğer | ✓ | `ContactRequests.SummaryTopics` — `TheContactRequestCount_TakesGeneralProductAndOther_Only_KK6` (beş tipten 3) |
| 7 | Kargo sözü: süresi dolan fiziksel siparişler; geç gönderilen/iptal/fesih uymamış; süre dolmadan iptal girmez | ✓ | `ReadShippingPromisesAsync` + `ShippingPromiseCompliance` — `ThePromise_IsKeptOnlyByAShipmentBeforeItRanOut_..._KK7`; Integration `TheShippingPromiseRatio_..._KK7` (1/4; düzeltilmiş S5 sayılmaz) |
| 8 | İptal ve iade oranı: iptal, fesih (bildirim damgası), cayma (beyan anı); "mal dönmedi" ve iade reddi değiştirmez | ✓ | `AfterSalesModule.SalesSummary.cs` — `TheCancellationAndReturnCount_IsByEventDay_AndTheClosureOrTheRejectionChangesNothing_KK8`; `TheCancellationAndReturnRatio_CountsAnOrderOnce_OverItsItems_KK8` |
| 9 | Paydası sıfır oran "değerlendirilemez", sıfır yüzde değil | ✓ | `SummaryRatio.IsAssessable` — `ARatioWithAZeroDenominator_IsNotAssessable_KK9` |
| 10 | Tek gün özeti tepe gün tetikleyicisinin günlük sayısı | ✓ | `CountDailyOrdersAsync` tek gün özetinin `OrderCount`'u — Integration KK10 kolu (`DailyCountAsync` = `OrderCount`) |

### Öz denetim (bitiş kapısı 2)

> PR açılmadan önce planın kabul kriterleri ve doğrulama kontrol listesinin **her** maddesi kendi diff'inde denetlenir. Devam ya da düzeltme turu bütün maddeleri yeniden denetler, yalnız kalanları değil.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 ziyaretçi verisi ve saklama yok | ✓ | `SalesSummaryService.cs` kurucusu; KK1 birim testi |
| KK2 Başarısız ve bekleyen sayılmaz | ✓ | `SalesPeriodReader.cs` `Approved`; Integration KK2 testi |
| KK3 ciro geri ödemeden düşmez | ✓ | `SumAsync(o => o.Total)`; Integration KK3 kolu (Refunded/PartiallyRefunded tam toplam) |
| KK4 dönem sipariş tarihine göre | ✓ | `born = CreatedAt in [from,to)`; Integration KK4 kolu |
| KK5 art arda siparişler tek deneme | ✓ | `SalesSummaryRules.PaymentCompletion`; 3 birim + 1 Integration testi |
| KK6 iki tip sayıya girmez | ✓ | `SummaryTopics`; Integration KK6 testi |
| KK7 geç gönderilen ya da feshedilen uymamış | ✓ | `ShippingPromiseCompliance`; birim + Integration KK7 testi |
| KK8 fesih ve cayma sayılır, kapanış değiştirmez | ✓ | `ListItemsWithLossInRangeAsync` kapanış/red tablolarını okumaz; Integration KK8 testi kapanış ve red sonrası aynı sayı |
| KK9 sıfır paydada değerlendirilemez | ✓ | `SummaryRatio.IsAssessable`; KK9 birim testi |
| KK10 tek gün sayısı tetikleyiciyle aynı | ✓ | `CountDailyOrdersAsync`; Integration KK10 kolu |
| 09 §20 m5 — yeni eksen dizinle gelir | ✓ | `AddSalesSummaryPeriodIndexes` (7 dizin); `06 §7.5.1`; `IndexConsistencyTests` (Unit Consistency 291/291), Model `Indexes_AreTheDocumentedOnes` 14/14 |
| 09 §20 m2–m4 — sayfasız okuma, projeksiyon, N+1 | ✓ | Sayım, ciro, yöntem ve ürün toplamları veritabanında (`CountAsync`, `SumAsync`, `GroupBy`); bellekte kalan kümeler dönemle sınırlı ve modül sınırını (09 §6 m2) aşmak için kimlik listesidir — özet bir liste değil tek toplamdır; döngüde sorgu yok; hepsi `AsNoTracking` + `Select` |
| 09 §6 — modül sınırı | ✓ | Administration yalnız `Contracts` kullanır; Unit `Architecture` ad alanı yeşil (tam Unit 5563/5563) |
| 09 §10.6 — göç | ✓ | tek konu, `Down` silindi, gerekçe yorumda, yalnız genişletme |
| 06 §5.1.3 — enum dosyası taraması | ✓ | iki enum ayrı dosyada; `EnumSourceScanTests` yeşil |
| 09 §25 / güvenlik taraması | ✓ | Yeni uç, kullanıcı girdisi, SQL birleştirme, sır yok; yalnız okuma; kişisel veri döndürülmez (ürün adı ve sayılar) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build` | 5563/5563 |
| Integration | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Model` + `--filter-not-namespace Shopfolio.IntegrationTests.Model` (Postgres 18, `track_commit_timestamp=on`) | Model 1399/1399 (3 dizin testi düzeltme sonrası yeniden 14/14), öteki 1854/1854; `SalesSummaryTests` 5/5 |
| Contract | — | — | yeni uç yok (ekran T524'ün) |
| Biçim | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (3 proje) | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-09; head `9f7b3620` + `origin/main` `8cd6652e` birleşik ağaç, çakışmasız) |
| Bulgu sayısı | 1 (B1 ORTA S1) |
| Düzeltme gerekli mi | Evet — B1 |
| Bloke-etmeyen not (K) sayısı | 0 |

**Validator kabul kriterleri tablosu (tur 1):**

| # | Sonuç | Validator kanıtı |
|---|---|---|
| 1 | ✓ | `SalesSummaryService` yalnız üç modül yüzeyi + `TimeProvider`; tablo/bağlam/uç yok; KK1 birim testi yeşil |
| 2 | ✓ | `SalesPeriodReader.Approved` = Paid/PartiallyRefunded/Refunded (`OrderOperationFacts.PaymentApproved` ile aynı); Integration KK2 yeşil |
| 3 | ✓ | `SumAsync(o => o.Total)`; `Order.Total` yalnız oluşturmada yazılır (T369 birleşiminde de); Integration KK3 yeşil |
| 4 | ✓ | `CreatedAt ∈ [from,to)`; havale O1'i iki gün sonra, doğduğu günde sayılır |
| 5 | ✓ | `SalesSummaryRules.PaymentCompletion`; 3 birim + 1 Integration yeşil |
| 6 | ✓ | `ContactRequests.SummaryTopics`; Integration KK6 yeşil |
| 7 | ✗ | **B1:** paydadan yalnız sipariş düzeyindeki S3/S4 çıkarılıyor. Fiziksel kalemi süre dolmadan kalem düzeyinde iptal edilen (ya da çıkarılan), hizmet kalemi açık kalan karışık sipariş S3/S4 almaz (`OrderTransitions.Shipment.cs:247-251`), S5 de alamaz (açık fiziksel kalem yok) → "uyulmadı" sayılıyor. `02 §10.6.3` ve K-535: payda "süre dolmadan iptal edilmemiş fiziksel kalem taşıyan" siparişlerdir; K-535'in gerekçesi tam bu hâldir. Validator sondajı (geçici test, geri alındı): beklenen `0/0`, gelen `0/1` |
| 8 | ✓ | `ListItemsWithLossInRangeAsync` olay günü; kapanış/ret okunmaz; Integration KK8 yeşil |
| 9 | ✓ | `SummaryRatio.IsAssessable`; KK9 birim testi yeşil |
| 10 | ✓ | `CountDailyOrdersAsync` = tek gün özetinin `OrderCount`'u; Integration KK10 kolu yeşil |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `AddSalesSummaryPeriodIndexes`: yedi tekil olmayan dizin (genişletme; satır değişmez)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T383-satis-ozeti`
- **Commit:** `8aabbf28` — T383: Satış özeti ve ölçüler (rapor güncellemesi bir sonraki commit)
- **PR:** #503
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok (yalnız ürünün kendi kayıtları okunur) | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Ekran, yüzde gösterimi, "Değerlendirilemez" metni ve satışın ilk açıldığı günün türetilmesi | — | T524'ün kapsamı (11); bu task yüzeyi verir |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** ilk işçi: 42619534143, 42619534329, 42616087470 success.
- **⚠ kararlar (kurala en uygun seçenekle verildi):**
  1. Sepet denemesi: aynı sepetin siparişleri ödenen siparişe kadar tek deneme; ödemeden sonraki sipariş yeni deneme (sepet ödemede boşalır). İmha edilmiş siparişin sepet bağı yoktur, kendi denemesidir (`02 §10.6.3`).
  2. "Ödemesi tamamlanmış" = Paid / PartiallyRefunded / Refunded (`OrderOperationFacts.PaymentApproved` ile aynı).
  3. Kargo sözü: son düzeltilmemiş O1'den `DeliveryPeriodRules.ShippingPeriodEnd`; uyum = süre bitmeden ilk düzeltilmemiş S5; süre dolmadan düzeltilmemiş S3/S4 paydadan çıkar.
  4. İptal/iade sayısı olay gününe göre (`07` 9.22.1; K-580), oran dönem kohortu; kalem çıkarma da sayılır (`07` "iptal ve çıkarma kaydı").
  5. Oran pay/payda döner; yüzde ve "Değerlendirilemez" metni T524'ün.
  6. Ölçüm penceresi: satışın ilk açıldığı gün saklanmıyor (`06 §10.6` m6) — `SalesSummaryPeriod.Of(..., salesOpenDay)` çağırandan alır; türetme T524.
  7. **Dizin (devam turunda yeniden değerlendirildi):** önceki işçi "H-1/H-2 hacminde gerekmez" deyip dizin eklememişti; `09 §20` m5 "yeni süzme ekseni dizinini aynı PR'ın göçünde getirir" der ve Elenenler'deki hacim gerekçesi eksensiz sorgular içindir — yedi eksen için dizin eklendi, `06` güncellendi.
  8. **Sayfasız dönem okumaları:** `09 §20` m2 liste sorgularınındır; özet tek bir toplamdır. Sayılar veritabanında yapılır; bellekte kalan kümeler (kohort kalem kimlikleri, sepet denemeleri, süresi dolan sözler, kayıp kalemleri) dönemle sınırlıdır ve modüller arası birleşim bağlam üzerinden yapılamadığı için (`09 §6` m2) kimlik listesi olarak taşınır.
- **Diğer:** Sepetsiz canlı sipariş yoktur (`ck_orders_disposal_fields_together`); okuyucunun `CartId == null` kolu yalnız imha edilmiş siparişler içindir.
