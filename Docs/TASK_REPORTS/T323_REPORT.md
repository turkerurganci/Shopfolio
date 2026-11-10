# T323 — Durum × rol × işlem tablosu

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-08

## Yapılan İşler

- `Ordering/Contracts/OrderOperation.cs`: 41 işlemin kimliği (üye sırası = `03 §1.11` sırası; satır kimliği `1.11.` + sıra no). `IOrderOperations.cs`: `ReadAsync(orderId, actor)` ve `CheckAsync(orderId, op, itemId, qty)`.
- `Ordering/OrderOperationCatalog.cs`: 41 satırlık karar tablosu (`Rows`), `Decide` (ekran/panel listesi), `Check` (POST denetimi, `OperationGate.Decide`). `OrderOperationFacts.cs`: düz değerler. `OrderOperations.cs`: kayıtlardan ve AfterSales/Payment açık okumalarından olguları kuran yükleyici.
- AfterSales: `ItemPeriodStatesAsync`, `RefundIbanStateAsync` (`AfterSalesModule.OperationFacts.cs`, `Contracts/ItemPeriodStates.cs`); Z-13/14/18/42 mevcut `AfterSalesPeriodRules` ile.
- Testler: Unit `OrderOperationCatalogTests` (20), `OperationTableConsistencyTests` (3); Integration `OrderOperationsTests` (5). `ApplicationEnums.NotStored`'a `OrderOperation` eklendi (saklanmaz, sonuç/kimlik değeri).

## Etkilenen Modüller / Dosyalar

Ordering, AfterSales (arayüze 2 üye), testler (`Ordering/`, `Consistency/`, `Support/ApplicationEnums.cs`), `Docs/IMPLEMENTATION_STATUS.md` T323 satırı, `Docs/STATUS_CHANGELOG.d/T323.md`, `Docs/DEFERRED_BACKLOG.d/T323.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 41 işlem tek kararda, açık aralık karardan okunur | ✓ | `OrderOperationCatalog.Rows`; `Rows_AreTheFortyOne_...` |
| 2 | Karar sipariş/ödeme durumu, kalem tipi, açık adetten; ekran ve uç aynı | ✓ | `ADecisionRead_AndACheck_AgreeOnEveryOperationOfEveryState`; `TheScreensList_AndThePostsCheck_...KK2` |
| 3 | Aralık dışı işlem sunulmaz | ✓ | `Decide` kapalıyı listeye almaz; `AHiddenOperation_CalledByHand_...KK3_KK6_KK8` |
| 4 | Fiziksel kalemde cayma/ayıp talebi yalnız kargolu kalemde | ✓ | `APhysicalItemThatHasNotShipped_...KK4`; `PhysicalWithdrawal_IsClosedBeforeTheShipment_...` |
| 5 | Dijitalde iptal/cayma yok; hizmet iptali "tamamlandı"da kapanır | ✓ | `DigitalItem_HasNoCancellationAndNoWithdrawal_AtAnyState`; `ServiceItemCancellation_ClosesAtTheCompletedMark_...`; `...KK5` |
| 6 | Her işlem sunucuda yeniden denetlenir | ✓ | `CheckAsync` olguları kayıttan yeniden kurar; kapalı işlem elle çağrılınca Closed |
| 7 | Rol modeli yok, panel yönetici | ✓ | `ThereIsNoRoleModel_TheSidesNeverMix`; `TheSidesNeverMix_...KK7` (System için ReadAsync reddi) |
| 8 | Tablo `03 §1.11` satır kimlikleriyle birebir; tabloda olmayan kapalı | ✓ | `OperationTableConsistencyTests` (belgeden okunan 41 kimlik + taraf, değişmiş kopya başarısız); `AnOperationNotInTheTable_IsClosed` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt |
|---|---|---|
| KK1 41 işlem tam sayılır | ✓ | `Rows_AreTheFortyOne_...`, `Rows_AreTheDocumentsFortyOne_CustomerFourteen_AdministratorTwentySeven` (belgeden sayılır, test sabitten türetmez) |
| KK2 ekran ve uç aynı | ✓ | yukarıdaki iki test |
| KK3 kapalı sayfada yok, uca gelirse reddedilir | ✓ | `AHiddenOperation_CalledByHand_IsRefused_...` |
| KK4 kargosuz fiziksel kalemde cayma yok | ✓ | `...KK4` |
| KK5 dijital/hizmet | ✓ | `...KK5` |
| KK6 gizli düğmenin ucu reddedilir | ✓ | `...KK3_KK6_KK8` |
| KK7 rol ayrımı yok | ✓ | `...KK7` |
| KK8 tabloda olmayan kapalı | ✓ | `AnOperationNotInTheTable_IsClosed`, `CheckAsync((OrderOperation)99)` |
| `09 §25` / güvenlik | ✓ | enum kuralları (`EnumSourceScanTests`, `EnumTableConsistencyTests`, `EnumLockTests`) ve statik koleksiyon taraması (`FrozenDictionary`) yeşil; yeni arayüz üyeleri tek uygulayıcıda |
| Sık FAIL: modül kurucusu değişmedi | ✓ | `OrderingModule` kurucusu aynı; Integration tam koşuldu |

Not: `CheckAsync` yazıcının işlem içinde, sipariş satır kilidinden sonra çağrılacağı yerdir; mevcut yazıcıların bağlanması D-610.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 4870/4870 |
| Integration | ~ | `dotnet test tests/Shopfolio.IntegrationTests` (yerel PostgreSQL 18 kapsayıcı) | 2381/2383; yeni 5 test yeşil. Başarısız 2: `AdminAndAuditTablesTests.Migration_WithoutTheApplicationUserRoleSetting...`, `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled...` — çıplak kapsayıcıda `pg_trgm`/rol ayarı olmaması (ortam), T323 dosyalarına dokunmaz; CI ortamında doğrulanır |
| Format | ✓ | `dotnet format ... --verify-no-changes` | Web, Unit, Integration temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 2, 2026-11-08, head `e5644a69`; tur 1 ✗ FAIL, head `1c68a3b7`) |
| Bulgu sayısı | Tur 2: 0 bulgu, K-notu 1 (D-660). Tur 1: 3 (B1 ORTA S1, B2 DÜŞÜK S1, B3 DÜŞÜK S1); K-notu 2 |
| Düzeltme gerekli mi | Hayır — B1–B3 Düzeltme 1 ile kapandı (tur 2 kanıtıyla) |

**Validator kabul kriterleri tablosu (tur 2):** K1 ✓ (41 satır; 1.11.25, 1.11.28, 1.11.29 ve 1.11.32 düzeltildi — `OrderOperationCatalogTests`, `OrderOperationsTests` 1.11.25/1.11.28 testleri; 1.11.32 adım ayrımı katalogda, `Contracts` yüzüne taşınması D-657/D-660) · K2 ✓ · K3 ✓ · K4 ✓ · K5 ✓ · K6 ✓ (karar düzeyinde; yazıcıların bağlanması D-610/D-614) · K7 ✓ · K8 ✓. Tur 1 tablosu: K1 ✗ (B1–B3), öteki ✓.

- **B1 (ORTA, S1) — 1.11.28 ve 1.11.29 teslimden önceki caymanın ve gecikme feshinin malını saymıyor.** `AfterSalesModule.OperationFacts.cs` `ReceivableUnits`/`RejectableUnits`'i yalnız `IsAfterDelivery` doğru olan cayma beyanlarından sayar; `IsAfterDelivery` teslim tarihi olmayan fiziksel kalemde `false` döner, fesih kayıtları hiç sayılmaz. `03 §1.11.28` "cayma beyanlı ya da feshedilmiş fiziksel kalemin malı ulaşınca", `02 §6.4.22`, §6.7.3 ve §7.4.7 kargodayken cayılıp dönen gönderiyi ve feshin malını teslim alma adımıyla işler. Senaryo: Kargoya verildi, teslim tarihi yok, müşteri cayar, gönderi firmaya döner → karar 1.11.28'i kapalı verir; mevcut T320 yazıcısı (`ReceiptRefusalAsync`) aynı teslim almayı kabul eder. D-612 yalnız ayıp talebi kolunu (K-808) erteler.
- **B2 (DÜŞÜK, S1) — 1.11.25 ulaşma tarihi kolu teslim almadan önce açık, sonra kapalı.** Kural `ReceivableUnits > 0 || RejectableUnits > 0` okur (henüz teslim alınmamış adet); `03 §1.11.25` "ulaşma tarihinde iade teslim almadan sonra" der. Teslim tarihi olmayan kalemde teslim alınmış dönüşün ulaşma tarihi düzeltilemez. B1 ile birlikte düzelir (teslim alma/ret kaydı varlığı okunmalı).
- **B3 (DÜŞÜK, S1) — 1.11.32 "geri ödeme sonucu bekleniyor"da yeniden deneme ve havale yolunu da açık veriyor.** Satır `CardRefundOutcomePending` iken açıktır; `03 §1.11.32` ve `02 §7.2.8` "sonuç beklenirken yeniden deneme ve havale yolu kapalıdır" der, yalnız "gerçekleşmedi" işareti açıktır. Karar alt işlemi ayırmadığı için ekran ve POST yeniden denemeyi bu hâlde reddedemez.

## Düzeltme 1 (doğrulama tur 1 FAIL — B1, B2, B3)

| Bulgu | Düzeltme | Kanıt (03 §1.11 satırına bağlı test) |
|---|---|---|
| B1 ORTA — 1.11.28/29 yalnız teslimden sonraki caymayı sayıyordu | `ItemPeriodStatesAsync` teslim alınacak adedi teslim alma yazıcısıyla (`ReceiptRefusalAsync`) aynı kuraldan sayar: `ItemQuantities.AwaitingReceipt`, `IsAfterDelivery` süzgeci yok (tek kural evi); fiziksel kalemin gecikme feshinin malı da sayılır; hizmet/dijital kalemin cayması mal doğurmaz; "mal dönmedi" kapanışı (1.11.30) teslim sonrası cayma olarak kaldı | Integration `TheReturnedGoods_AreTakenIn_BeforeTheDeliveryDateToo_...1_11_28`, `TheGoodsOfALateDeliveryTermination_AreTakenIn_1_11_28`, `AServiceItemsWithdrawal_HasNoGoodsToTakeIn_1_11_28`; Unit `ReceivingReturnedGoods_FollowsTheUnitsTheAfterSalesReadSays_...1_11_28` |
| B2 DÜŞÜK — 1.11.25 yönü ters | Kural "teslim tarihi var ya da teslim alma/ret kaydı var" (`ItemPeriodState.HasArrivalRecord`); bekleyen adet okunmaz | Unit `CorrectArrivalDate_NeedsAReceiptOnFile_NotGoodsStillAwaited_1_11_25`; Integration (aynı 1.11.25 testi: teslim almadan önce kapalı, sonra açık) |
| B3 DÜŞÜK — 1.11.32 sonuç beklenirken tümüyle açık | İki alt adım ayrıldı: `OrderOperationCatalog.IsCardRefundStepOpen` (`RetryOrRedirect`: "gerçekleşmedi" işareti, IBAN yok, sonuç beklenmiyor; `MarkNotRealized`: yalnız "sonucu bekleniyor") ve POST denetimi `CheckCardRefundStep`; satırın kuralı ikisinden biri açıkken açık | Unit `CardRefund_WhileTheOutcomeIsAwaited_OnlyTheNotRealizedMarkIsOpen_1_11_32` |

Öz denetim (düzeltilmiş diff): kabul kriterleri 1–8 ve KK1–KK8 yeniden denetlendi, yukarıdaki ✓ tablosu geçerli; `09 §25` "kod hiçbir doküman cümlesiyle çelişmiyor mu" → B1–B3 kapandı. Sık FAIL satırı ("açılma koşulu yazıcının kabul ettiği durumlardan dar/ters") için koşul yazıcıyla aynı işlevden gelir. Yeni backlog: D-656 (fesih malının teslim alma yazıcısı yok), D-657 (1.11.32'nin iki adımının yazıcıları/ekranı kararın adım denetimini çağırmalı). Test: Unit 4960/4960; Integration `Ordering` ad alanı 293/293 (kalan ad alanları CI'da).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T323-durum-rol-islem`
- **Main CI:** son 3 koşu başarılı (`gh run list --branch main --limit 3`, head `a71c6f02`)
- **PR:** #474
- **Post-merge run'lar:** squash `48dadb9` (#474) — CI 42518519754 success, Publish 42518519762 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Mevcut yazıcılar kararı çağırmıyor | D-610 | Ekran task'ları bağlar |
| 2 | IBAN isteği kalemsiz istekte eksik | D-611 | Payment sahibi okuma |
| 3 | 1.11.26/28/31 yaklaşık karar | D-612 | Panel task'ları |
| 4 | 1.11.36 `EmailNotDelivered` sabit | D-613 | Notifications iç yüzü |
| 5 | Panel yazıcıları kararı çağırmıyor | D-614 | D-610 ile aynı |
| 6 | 1.11.33 kart hattının havale yolunda sunulmuyor | D-643 | Geri ödeme paneli task'ı |
| 7 | Gecikme feshinin malını teslim alma yazıcısı yok | D-656 | Panel task'ları |
| 8 | 1.11.32 adımlarının ekran/POST bağlaması | D-657 | Panel task'ları |
| 9 | 1.11.32 adım ayrımı `Contracts` yüzünde yok (doğrulama tur 2 K-notu) | D-660 | Panel task'ları |
