# T351 — Teslim işaretleri: kalem tipine göre teslim, karışık sipariş, fiziksel teslim tarihi

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `DeliveryMarkRules`: teslim işaretinin tek kural evi. `IsPaymentOpenToMark` (ödeme Bekliyor ya da Başarısız dışında açık; Kısmen geri ödendi kapatmaz), `IsServiceMarkOffered` (hizmet kalemi, açık adet, işaretsiz, ödeme açık; panelin sunum kuralı), `CheckDeliveryDate` (ileri tarih ve kargoya verme gününden önceki tarih sebebiyle reddedilir; geçmiş tarih olur).
- `DeliveryDateObstacle` (`InTheFuture`, `BeforeShipment`) ve `OrderTransitionResult.DateObstacle`: ret sebebi sonuçla taşınır; ret türü `InvalidInput` kalır.
- `IOrderTransitions.MarkServiceCompletedAsync` + `OrderTransitions.ServiceMark.cs`: hizmet kaleminin "tamamlandı" işareti ve günü; son işaret siparişi kapatır (S2/S10/S11 yönetici olayı kuralı), sürüm bir kez artar.
- S6 (`MarkDeliveredAsync`) tarih denetimini `CheckDeliveryDate`'ten alır; tek tarih yalnız iptal edilmemiş ve çıkarılmamış fiziksel kalemlere yazılır (T350'den gelen yazım, T351 testleriyle kanıtlandı).
- Kalem için durum makinesi yok taraması (`ItemHasNoStateMachineTests`): tür, üye, enum değeri, parametre, model tablosu ve sütunu.
- `ApplicationEnums.NotStored` listesine `DeliveryDateObstacle` (sonuç kodu, sütunu yok).
- Kayıtlar: D-454 kapandı; D-508 hedefinden T351 çıktı; D-645…D-648 açıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/`: `DeliveryMarkRules.cs`, `OrderTransitions.ServiceMark.cs`, `OrderTransitions.Shipment.cs`, `Contracts/DeliveryDateObstacle.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/ShipmentTransitionRequests.cs`
- `tests/Shopfolio.UnitTests/`: `Ordering/DeliveryMarkRulesTests.cs`, `Architecture/ItemHasNoStateMachineTests.cs`, `Ordering/ShipmentTransitionContractTests.cs`, `Support/ApplicationEnums.cs`
- `tests/Shopfolio.IntegrationTests/Ordering/DeliveryMarksTests.cs`
- `Docs/`: `DEFERRED_BACKLOG.md` (D-454), `DEFERRED_BACKLOG.d/T340.md` (D-508), `DEFERRED_BACKLOG.d/T351.md`, `STATUS_CHANGELOG.d/T351.md`, `IMPLEMENTATION_STATUS.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Teslim tipe göre işler: fiziksel firma işaretiyle, dijital onayda sistemce, hizmet "tamamlandı" ile | ✓ | `DeliveryMarksTests`: `APhysicalItemsMark_…_KK1`, `ADigitalItemsMark_…_KK1`, `AServiceItemsMark_…_KK1`, `AServiceMark_OfAnItemThatIsNotTheOrdersOrAlreadyMarkedOrClosed_…_KK1` |
| 2 | Ödeme Bekliyor/Başarısız iken işaret konmaz, Kısmen geri ödendi kapatmaz, panel ödenmemişte sunmaz | ✓ | `TheServiceMark_IsRefusedWhilePendingOrFailed_…_KK2`, `APartialRefund_DoesNotCloseTheMark_…_KK2`, `ThePhysicalMark_IsNotTakenWhileThePaymentIsPending_…_KK2`; Unit `DeliveryMarkRulesTests` (`ThePaymentIsOpenToTheMark_…`, `TheMarkIsOfferedOnEveryStatusButPendingAndFailed_…`) |
| 3 | Karışık siparişte sevkiyat tek eksende, fiziksel hattı izler; dijital beklemeden indirilir | ✓ | `AMixedOrder_FollowsThePhysicalLine_AndTheDigitalItemIsDownloadableWhileItIsStillPreparing_KK3`, `AMixedOrdersServiceMark_NeverMovesAShippedOrder_KK3` |
| 4 | Kalem düzeyinde yalnız teslim işareti; ayrı durum makinesi yok | ✓ | `ItemHasNoStateMachineTests` (4 test + ad taraması teorileri) |
| 5 | Tarih: geçmiş olur, ileri ve kargo gününden önce sebebiyle reddedilir; tek tarih açık fiziksel kalemlere; cayma bundan işler, işaretsizde başlamaz | ✓ | `ADeliveryDateInTheFuture_OrBeforeTheShipment_IsRefusedWithItsReason_…_KK5`, `ARefusedDateWritesNothing_…_KK5`, `TheOneDate_IsWrittenOnTheOpenPhysicalItemsOnly_…_KK5`; Unit `ADateInThePast_…`, `ADateAfterToday_…`, `ADateBeforeTheDayOfTheShipment_…`, `WithNoShipmentRecorded_…` |
| 6 | Fiziksel kalemlerin tamamı iptal edilirse hat fiziksel kalemsiz siparişe düşer | ✓ | `WhenThePhysicalItemsAreAllCancelled_TheOrderFallsToTheLineWithoutThem_…_KK6` |
| 7 | Fiziksel kalemsiz sipariş kargo adımı görmez; Alındı'da bekler, son işaretle Teslim edildi | ✓ | `AnOrderWithoutAPhysicalItem_StaysPlaced_…_KK7`, `AnOrderWithTwoServiceItems_WaitsForTheLastMark_…_KK7` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 üç tipin teslim yolu | ✓ | `DeliveryMarksTests` KK1 dört testi |
| KK2 Bekliyor'da işaret sunulmaz ve reddedilir; Kısmen geri ödendi'de konur | ✓ | `DeliveryMarkRulesTests` (sunum), `DeliveryMarksTests` KK2 üç testi (ret) |
| KK3 dijital indirme fiziksel hattı beklemez | ✓ | `AMixedOrder_FollowsThePhysicalLine_…_KK3` (`TakeDownloadAsync` Hazırlanıyor'da açık) |
| KK4 kalem için durum makinesi tablosu yok | ✓ | `ItemHasNoStateMachineTests.TheModelHasNoTableOrColumnOfAnItemStatusAStateOrATransition_KK4`, `TheTransitionRecord_BelongsToTheOrder_…_KK4`; `ShipmentTransitionContractTests` beklenen liste |
| KK5 tarih sınırları, sebep, tek tarih, cayma | ✓ | `DeliveryMarksTests` KK5 üç testi; `DeliveryMarkRulesTests` KK5 dört testi |
| KK6 fiziksel kalemler iptal edilince hat değişir | ✓ | `…_KK6` iki test |
| KK7 kalemsiz siparişte kargo adımı yok, son işaretle Teslim edildi | ✓ | `…_KK7` iki test |
| `09 §25` / güvenlik taraması: dar istisna, çağrı biçimleri | ✓ | KK4 taraması her yasak biçimi kendi yazımıyla sınar (`TheScan_FindsEachForbiddenFormInItsSpelling`) ve olağan adı yanlış saymaz (`TheScan_DoesNotMistakeAnOrdinaryNameForAForbiddenOne`); istisna listesi yok |
| Sık FAIL 1 (bekçi/tarama istisnası geniş) | ✓ | tarama istisnasız; yeni dar `NotStored` girişi tek tam adla |
| Sık FAIL 2 (tarama bir çağrı biçimini kaçırıyor) | ✓ | tarama ad biçimleri teoriyle sınanır |
| Sık FAIL 3 (06 "Varsayılan" / FK dizini) | ✓ | şema değişikliği yok (göç, tablo, sütun eklenmedi) |
| Sık FAIL 4 (kural değişince öteki yerler) | ✓ | `ShipmentTransitionContractTests` ve `ApplicationEnums.NotStored` güncellendi; Unit TAM ilk koşuda iki tutarlılık testini yakaladı, düzeltildi |
| Sık FAIL 5 (belge kuralı değişikliği karar kaydısız) | ✓ | 09 değişmedi; belge kuralı değişmedi |
| Sık FAIL: kapanış kuralı teslim işaretli kalemi denetler (T316 r1) | ✓ | `AServiceItemsMark_…_AndTheLastOneEndsTheLine_KK1`, `WhenEveryItemIsCancelled_NoServiceMarkIsLeftToSet_KK6` |
| Sık FAIL: modül kurucusu bağımlılığı / Integration TAM | ✓ | kurucu değişmedi; Integration TAM koşuldu (aşağıda) |
| Sık FAIL: `dotnet format` koşulmadan "temiz" | ✓ | `dotnet format --verify-no-changes` Web, UnitTests, IntegrationTests: temiz (birleşme sonrası, yapım işçisi 2) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 5023/5023 | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | TAM koşu |
| Integration | ✓ 2895/2895 | `dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj --no-build --filter-namespace "*.Ordering"` ve `--filter-not-namespace "*.Ordering"` | TAM, birleşik ağaçta, ad alanı parçalarıyla: Model 1379, Catalog..Files 421, Jobs..Web 778, Ordering 317 (`DeliveryMarksTests` 16/16 dahil); 0 hata. Önceki işçinin 88 Npgsql "Timeout during reading" hatası ortamdı (tek konteyner, paralel koşu); temiz konteynerde (max_connections=300) hepsi geçti |
| Contract | — | — | bu task'ta yok |

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

- **Branch:** `task/T351-teslim-isaretleri`
- **Commit:** kod başı `258819a3`; rapor commit `d71ada77`
- **PR:** #478
- **CI:** yoklanmadı (yapım işçisi CI izlemez)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Başlangıç: `gh run list --branch main` — Publish 42512134557 success, CI 42512134573 (devam ediyordu), Publish 42510559169 success.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Panel: hizmet "tamamlandı" düğmesi ve tarih ret sebebi gösterimi | D-645 | Ekran T527'de |
| 2 | Hizmet işaretinin işlem izi satırı | D-646 | Ordering hiçbir geçişte iz yazmıyor; panel işlemi açar |
| 3 | Teslim tarihi düzeltmesi (`02 §10.4.5`) | D-647 | T351 kapsamı dışı |
| 4 | Çakışan cayma beyanı günü sorusu | D-648 | Ekran düzeyi |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yukarıda
- **Repo memory (bitiş kapısı 9):** güncellenmedi (MEMORY "Güncel Durum"a dokunulmaz, talimat)
- **Kolay ⚠ kararlar:** (1) ret sebebi `TransitionRefusal`'a yeni değer eklemeden `OrderTransitionResult.DateObstacle` ile verildi; (2) hizmet işaretinin ödeme koşulu kapanış kuralıyla aynı okuma (Bekliyor ve Başarısız dışı; Refunded D-528 notuyla aynı çizgide); (3) hizmet işareti `OrderTransitionResult` döner, kapanış kaydı varsa onu taşır; (4) panel sunumu için yalnız kural (`IsServiceMarkOffered`), modül okuma görünümü yok (D-645).
- Docker: `shopfolio-t351-pg` ve `shopfolio-t351b-pg` (port 55481) iş sonunda adıyla silindi.
