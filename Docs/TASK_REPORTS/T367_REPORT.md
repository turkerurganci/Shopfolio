# T367 — Müşteri iptali: kalem ve adet düzeyi, kargoya verilene kadar, dijital ve hizmet sınırı, sayaçların dönmesi, kısmi iptalde kupon

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `IOrderTransitions.CancelItemAsync(CancelItemRequest)` — müşterinin ödenmiş siparişte kalem/adet iptali, tek işlemde: adet < 1 → `InvalidInput` (kilitten önce); sipariş satır kilidi; `OrderOperations.CheckAsync` ile `03 §1.11.2–1.11.4` kararı kilit altında yeniden sorulur (fiziksel: Hazırlanıyor/Alındı ve ödeme onaylı; hizmet: "tamamlandı" işaretine kadar, kargo durumundan bağımsız; dijital: hep kapalı; açık adeti aşan adet → `InvalidInput`); kayıt; sürüm bir kez artar; sonra sipariş hattı; en sonda sayaçlar.
- `IAfterSalesModule.AddCancellationAsync` — adet kuralı + `ItemShares.SetAside` ile tutar: ürün tutarı = birim fiyat (indirimliyse indirimli) × adet; kupon payı ve KDV `Money.ShareOfUnits` ile aşağı yuvarlanır; kalemin son açık adetlerini kapatan kayıt kalanı alır, hiçbir kayıt kalanı aşmaz (`02 §7.1.7` (1)–(4)). Daha önce ayrılan pay kaydedilmiş + bağlamdaki eklenmiş satırlardan okunur; düzeltilmiş geçişe bağlı iptal pay ayırmış sayılmaz.
- `IAfterSalesModule.LinkClosingTransitionAsync` — siparişin son açık adetlerini kapatan iptal kaydı müşterinin S3/S4 kaydına bağlanır (D-519).
- Hat (D-306 tek yol): açık adet kalmadı + teslim işaretli kalem yok + sipariş Alındı/Hazırlanıyor → `CancelOrderAsync(ByCustomer(order, item))` (müşterinin S3/S4'ü + kalem biçimli tek B-7); aksi hâlde B-7 (kalem ve adet) + `EvaluateClosureAsync` (karışık siparişte teslim edilmiş dijitalle Teslim edildi'ye S10, açık hizmetle Hazırlanıyor kalır; Teslim edilemedi siparişinde son hizmet adedi iptal edilince sipariş firmanın S9 kapanışını bekler).
- Sayaçlar: `OrderCounters.ReturnAsync` — fiziksel/hizmet varyantına iptal edilen adet döner (dijitalde sayaç yok); kupon hakkı yalnız iptal siparişin bütününü kapatınca döner. `OrderCounters.IsFinal` "bütün adetler kapandı" koşulunu kataloğun tek kural evinden (`ICatalogModule.ReturnsCouponRight` → `CouponRules.ReturnsRight`) okur (D-483).
- `Money.ShareOfUnits` (09 §14.1 madde 1'in dördüncü işlevi).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Money.cs`
- `src/Shopfolio.Web/Modules/AfterSales/ItemShares.cs` (yeni), `AfterSalesModule.Cancellations.cs` (yeni), `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`
- `src/Shopfolio.Web/Modules/Ordering/OrderTransitions.CustomerCancellation.cs` (yeni), `OrderTransitions.cs` (kurucuya `OrderOperations`), `OrderCounters.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/ShipmentTransitionRequests.cs`
- Testler: `tests/Shopfolio.UnitTests/AfterSales/ItemSharesTests.cs` (yeni), `Infrastructure/MoneyTests.cs`, `Ordering/ShipmentTransitionContractTests.cs`; `tests/Shopfolio.IntegrationTests/Ordering/CustomerCancellationTests.cs` (yeni, 23 test)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Fiziksel kalem Kargoya verildi'ye kadar onaysız iptal; sonrası cayma | ✓ | `CustomerCancellationTests.APhysicalItem_OfAPaidOrderNotYetShipped_…_KK1_KK2` (Placed, Preparing); `APhysicalItem_OnceTheOrderIsShipped_…_KK1` (Shipped, DeliveryFailed, Delivered → ConditionNotMet, hiçbir şey yazılmaz). İstek sürüm/onay taşımaz (`CancelItemRequest`). |
| 2 | Firma hazırlanan siparişin iptalini engelleyemez | ✓ | aynı test `Preparing` kolu; `OrderOperationCatalog.ItemCancellable` Preparing'i açık sayar |
| 3 | Kalem/adet düzeyi; tam iptalde İptal edildi; fiziksel kalemi biten karışık sipariş kalan hatta | ✓ | `APartialCancellation_KeepsTheOrder_…_KK3_KK8`; `AMixedOrder_…DeliveredDigitalItem_ToDelivered_…_KK3` (S10); `AMixedOrder_…OpenServiceItemsLine_KK3`; `AReturnedShipments_LastOpenServiceUnit_…_KK3_KK5` |
| 4 | Kalem iptali ödeme onayından sonra; önce yalnız bütün | ✓ | `BeforeThePaymentsApproval_AnItemIsNotCancelled_OnlyTheWholeOrderWithS3AndO2_KK4` |
| 5 | Dijital onaydan sonra iptal edilmez; hizmet "tamamlandı"ya kadar adetle | ✓ | `ADigitalItem_IsNotCancelledAfterThePaymentsApproval_KK5`; `AServiceItem_IsCancelledByUnits_WhateverTheShipment_…_KK5_KK6` (Preparing, Shipped); `AServiceItem_MarkedCompleted_IsNotCancelled_KK5` |
| 6 | Stok/kontenjan döner; kupon hakkı yalnız tam iptal/iadede | ✓ | `TheCancelledUnitsGoBackToStock_AndTheCouponRightOnlyWithTheOrdersLastUnit_KK6` (stok 5→7, kupon used 1 kısmi iptalde 1, son adette 0, sınırsız kontenjan değişmez); hizmet testi (kontenjan 4→6) |
| 7 | Kısmi iptalde kupon yeniden hesaplanmaz; iptal adetlerinin payı geri ödemeye; asgari sepete bakılmaz | ✓ | `TheCouponShare_IsSetAsideByTheUnits_…_KK7` (33,33/33,33/33,34; 266,67/266,67/266,66; toplam = kalem payı; asgari sepet 1000'lik kuponda kalan kalem altında kaldı, ret yok, donmuş paylar aynı); `ItemSharesTests` (5); `MoneyTests.ShareOfUnits_*` |
| 8 | Müşteri kendi iptalinin tarihli kopyasını (B-7) alır | ✓ | `EveryCancellation_QueuesOneB7_…_KK8` (tek B-7, `CreatedAt` = iptal anı = kaydın anı); `APartialCancellation_…_KK3_KK8` (iptal başına bir B-7) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–8 | ✓ | yukarıdaki tablo; önceki işçinin kodu dahil diff baştan okundu |
| KK1 kargoya verildi sonrası ret, öncesi onaysız | ✓ | `APhysicalItem_OnceTheOrderIsShipped_…_KK1`, `…NotYetShipped_…_KK1_KK2` |
| KK2 hazırlanan siparişte iptal engellenmez | ✓ | `…NotYetShipped_…_KK1_KK2(Preparing)` |
| KK3 kısmi yaşar, tam kapanır, karışık kalan hatta | ✓ | `APartialCancellation_…`, iki `AMixedOrder_…`, `AReturnedShipments_…` |
| KK4 ödeme öncesi kalem iptali ret | ✓ | `BeforeThePaymentsApproval_…_KK4` |
| KK5 dijital ret, hizmet adetle | ✓ | `ADigitalItem_…_KK5`, `AServiceItem_…_KK5_KK6`, `AServiceItem_MarkedCompleted_…_KK5` |
| KK6 stok, kontenjan, kupon sayaçları | ✓ | `TheCancelledUnitsGoBackToStock_…_KK6`, `AServiceItem_IsCancelledByUnits_…` |
| KK7 kupon payı geri ödemeye, kalanlar korunur | ✓ | `TheCouponShare_…_KK7`, `ItemSharesTests` |
| KK8 iptal e-postası tarihle | ✓ | `EveryCancellation_QueuesOneB7_…_KK8` |
| 09 §25 — tek kural evi | ✓ | açıklık kararı tek yerde (`OrderOperationCatalog`, ekran da aynı kuraldan); pay kuralı tek işlev (`ItemShares`, `Money.ShareOfUnits`); kupon dönüşü `CouponRules.ReturnsRight` (D-483) |
| 09 §25 — modül sınırı, işlem sahipliği | ✓ | AfterSales yazıcıları kaydetmez, işlem açmaz (`RequireOrderLock`); Ordering kalemi değer olarak verir (`ItemFrozenAmounts`) |
| 09 §25 — kilit sırası | ✓ | sipariş kilidi → (aynı kilit `CancelOrderAsync` içinde yeniden, `LockSequence` izinli) → varyant/kupon (`ReturnAsync` en sonda) |
| 09 §25 — ret değerdir, hiçbir şey yazılmaz | ✓ | `UnitsUnderOneOrAboveTheOpenUnits_…` (0, −1, 3), `UnitsAboveWhatEarlierCancellationsLeftOpen_…`, `AnItemOfAnotherOrder_…` |
| 09 §3.1 m.5 null-forgiving `!` | ✓ | yeni kodda `!` yok (T348 K1 dersi) |
| Satış modeli taraması / mimari testler | ✓ | parametre adı `closesTheItem` ("last units" yasak ifadesi kaldırıldı); Unit 5436/5436 |
| Güvenlik taraması | ✓ | yeni uç/sayfa yok; ham SQL yok; müşteri isteği sipariş-kalem eşleşmesiyle doğrulanır (`AnItemOfAnotherOrder_IsAWrongValue`); erişim anahtarı denetimi ekranın (T512) |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | yeni bekçi/istisna yok |
| Sık FAIL (2) tarama çağrı biçimi | ✓ | yeni tarama yok |
| Sık FAIL (3) DB default / FK dizini | ✓ | şema değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | `ShipmentTransitionContractTests` işlem listesi güncellendi; `Money` sınıf yorumu (dördüncü işlev) güncellendi |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | doküman kuralı değişmedi |
| Devredilen backlog kalemleri | ✓ | D-156, D-306, D-685 ✅; D-153/154/155 → T512 (D-792); D-305, D-337, D-453, D-483, D-519 T367 payı kapandı; D-513 → T368 (D-793) |
| Integration TAM + `dotnet format` | ✓ | aşağıdaki tablo |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/...` (Consistency dahil, tümü) | 5436/5436 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/...` dört parçada (`--filter-namespace` Ordering; Model; Commands; `--filter-not-namespace` üçü) — Postgres 18 `track_commit_timestamp=on` | 413 + 1393 + 70 + 1263 = 3139/3139 |
| Integration (son düzeltme sonrası) | ✓ | `CustomerCancellationTests` + `AxesTieTests` + `StatusCorrectionTests` | 77/77 |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --include <değişen dosyalar>` (Web, UnitTests, IntegrationTests) | temiz |
| Contract | — | uç yok | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-09; head `5e076701`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — D-803, D-804 (`Docs/DEFERRED_BACKLOG.d/T367.md`) |

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = ana çalışma ağacının `scripts/git-hooks`'u; main son 3 run 42586725445, 42586725547, 42578567856 → success.
- **Kabul kriterleri (validator kanıtı):** 1 ✓ `OrderOperationCatalog.ItemCancellable` (fiziksel: ödeme onaylı + Alındı/Hazırlanıyor), istek sürüm/onay taşımaz; `…NotYetShipped_…_KK1_KK2` (Placed, Preparing) ve `…OnceTheOrderIsShipped_…_KK1` (Shipped, DeliveryFailed, Delivered → `ConditionNotMet`, hiçbir şey yazılmaz). 2 ✓ aynı testin Preparing kolu. 3 ✓ `APartialCancellation_…_KK3_KK8` (tek S4 Buyer, sürüm 3, kayıt S4'e bağlı), `AMixedOrder_…DeliveredDigitalItem…` (S10 → Teslim edildi, S4 yok), `AMixedOrder_…OpenServiceItemsLine…`. 4 ✓ `BeforeThePaymentsApproval_…_KK4` (kalem → `ConditionNotMet`; bütün → S3 + Ö2). 5 ✓ `CancelDigitalItem` hep kapalı; hizmet `!IsDeliveryMarked` (Preparing, Shipped kolları; işaretli → ret). 6 ✓ `OrderCounters.ReturnAsync` + `IsCouponReturnDueAsync` (önce/sonra "bütün adetler kapandı" — `CouponRules.ReturnsRight`); test stok 5→7, kupon kısmi iptalde 1, son adette 0. 7 ✓ `ItemShares.SetAside` + `Money.ShareOfUnits` (aşağı; son açık adet kalanı alır, hiçbiri kalanı aşmaz) — 33,33/33,33/33,34, geri ödeme 266,67/266,67/266,66, toplam = kalem payı; asgari sepet okunmaz, donmuş paylar değişmez. 8 ✓ B-7 iptal anında kuyruğa (`CreatedAt` = iptal anı), iptal başına bir.
- **`12` satırları:** `VAL-C003`, `C004` (k1), `C005` (k5), `C006` (k3, k4), `C007` (k4), `B019`, `A085` (k3), `A086`, `A087` (k7) — kanıt türü uç yanıtı (+ veri kaydı); uç T512, geri ödeme T370 → `BEKLEMEDE` kaldı (T351, T371 emsali). KP-21'in öteki satırları (`A067`…`A070`, `C008`…`C010`, `C014`, `C019`) ve KP-24'ün `A088`, `A089`'u fesih, cayma ve geri ödeme task'larınındır.
- **Testler (validator):** build `-warnaserror` 0/0; `dotnet format --verify-no-changes` üç proje temiz; Unit 5153 + Consistency 283 (main birleştirmesi sonrası 5491); betik 4/4; Integration Ordering 413 (birleşik 428), Catalog + Payment + Concurrency 122, Model + Commands + Web 2057 — hepsi geçti (Model/Commands/Web ilk koşuda 1 kararsız başarısızlık, iki yeniden koşu temiz; dalın dokunmadığı ad alanları).
- **Güvenlik:** sır yok; yeni uç/sayfa yok (müşteri erişim denetimi ekranın, T512); kalem–sipariş eşleşmesi kilit altında (`AnItemOfAnotherOrder_IsAWrongValue`); adet < 1 ve açık adeti aşan adet `InvalidInput`; yeni bağımlılık yok.
- **Yapım raporu karşılaştırması:** tam uyumlu; öz denetim tablosu kriter ve KK başına kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (`OrderTransitions` kurucusuna var olan `OrderOperations` eklendi; DI `AddOrderingModule` ikisini de kaydeder, elle kuran test yok)

## Commit & PR

- **Branch:** `task/T367-musteri-iptali`
- **Commit:** son commit — "T367: Müşteri iptali: kalem ve adet düzeyi, kargoya verilene kadar, dijital ve hizmet sınırı, sayaçların dönmesi, kısmi iptalde kupon"
- **PR:** #494
- **CI:** run 42588669661 (head `5e076701`) — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E, CI Gate yeşil
- **Post-merge run'lar:** squash `c5d3a58` (#494) — CI 42596386862 success, Publish 42596386708 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok (yalnız uygulama içi modüller ve PostgreSQL).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Havale hattında iptalin IBAN'ı (`RefundIbanId` null) | D-790 | IBAN formu ve geri ödeme hattı T370/T512 |
| 2 | İptalin geri ödeme kaydı (`Refund`) | D-791 | geri ödeme hattı T370 |
| 3 | İptal ekranı: POST, `[GatedOperation]`, 422, 303 | D-792 | ekran T512 |
| 4 | Müşteri iptalinde kanuni faiz uyarısı (panel) | D-793 | panel satırı T368 k6 |

## Notlar

- **⚠ (öneriyle karar):** `08` B-7 gövdesinde ayrı bir tarih cümlesi yok; "tarihli kayıt kopyası" e-postanın gönderim kaydının anıyla (iptal anı) karşılanır, şablon değiştirilmedi (karar kaydısız 08 sapması olmasın).
- **⚠ (öneriyle karar):** Teslim edilemedi siparişinde son açık adet hizmetinse müşteri iptali siparişi kapatmaz (`03 §1.2` S9 firmanın kapanışıdır); kayıt + B-7 yazılır, sipariş S9 kapanışını bekler.
- **⚠ (öneriyle karar):** D-483'ün isteğe bağlı maddesi yapıldı — `OrderCounters.IsFinal` kapalı adet koşulunu `ICatalogModule.ReturnsCouponRight`'tan okur (iki kural evi kalmasın).
- **Working tree (Adım -1):** temiz; önceki işçinin ara commit'i `941571fb` üstüne devam.
- **Diğer:** önceki işçi bağlam sınırında bölündü (devir notu); bu turda derleme, testler, kayıt ve PR.
