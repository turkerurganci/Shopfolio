# T386 — Sipariş müdahaleleri: kalem adedi azaltma ve çıkarma, kargo bilgisinin ve teslim tarihinin düzeltilmesi, iç not

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kalem çıkarma / adet azaltma (k1–k5):** `IOrderCorrections.RemoveItemAsync` → `OrderTransitions.RemoveItemAsync` (`Ordering/OrderTransitions.Removal.cs`). Sipariş satır kilidi + ekranın sürümü, kilit altında `OrderOperations.CheckAsync(RemoveOrReduceItem)` (ödeme onayından önce kapalı; fiziksel kalem kargodan, dijital/hizmet kalemi teslim işaretinden sonra kapalı; açık adetten fazlası `InvalidInput`), karta geri ödemede geri alınamaz onay, AfterSales yazıcısı `AddItemRemovalAsync` (iptalin donmuş tutar ve kupon payı kuralı), sürüm bir kez, kapanış kuralı (`EvaluateClosureAsync`), B-11, `StartRefundOfRecordAsync(ItemRemoval)` (§7.2.8 hattı; §7.2.9 tetiğiyle kargo ücreti; havalede IBAN isteği + B-14), `OrderCounters.ReturnAsync` (stok, kontenjan, kupon hakkı iptaldeki gibi). İstek kaydında sebep ve fiyat alanı yok. İz: `ReduceItemQuantity` / `RemoveItem` (ItemId, Quantity).
- **Kargo bilgisi düzeltmesi (k6):** `CorrectShipmentAsync` — `TryNormalizeShipment` tek kuralı, `CheckAsync(CorrectCarrier)`, aynı değer reddi, B-12, iz yalnız alan adını taşır (takip no ve şirket adı yok).
- **Teslim tarihi düzeltmesi (k7, k8):** `CorrectDeliveryDateAsync` — sipariş düzeyi; ileri tarih, son kargo gününden önce ve iade ulaşma tarihinden sonra ret (`DeliveryDateObstacle.AfterReturnArrival` yeni); beyan gününe düşerse cevapsız ret (`AsksDeliveryOrder`), cevap `Withdrawal.IsDeliveredBeforeDeclaration`'a yazılır, beyan kalır; bildirimsiz; iz eski + yeni tarih.
- **İade ulaşma tarihi düzeltmesi (k9):** `CorrectReturnArrivalDateAsync` → `IAfterSalesModule.CorrectArrivalDateAsync` (Return ve ReturnRejection, yerinde; ileri ve teslimden önce ret); bildirimsiz; iz eski + yeni tarih + ItemId. Geri ödeme süresi ulaşma tarihinden okunduğu için yeni tarihten işler.
- **İç not (k10):** `SaveInternalNoteAsync` — sürüm dışında (kilit var, sürüm artmaz), boş metin notu siler, aynı metin reddi, imha edilmiş siparişte `CheckAsync(WriteInternalNote)` reddi; iz metinsiz.
- **D-848:** `ClosingRecordMomentsAsync` çıkarmayı taşır (`IsRemoval`); dışa aktarmanın "kargodan önce kapandı" okuması çıkarmayı sayar; kanuni faiz uyarısı çıkarmayı dışlar.

## Etkilenen Modüller / Dosyalar

- Ordering: `OrderCorrections.cs` (partial, kurucuya `OrderTransitions`, `IAfterSalesModule`), `OrderCorrections.Interventions.cs`, `OrderTransitions.Removal.cs`, `OrderPeriods.cs`, `Contracts/IOrderCorrections.cs`, `Contracts/DeliveryDateObstacle.cs`, `Contracts/ShipmentTransitionRequests.cs`
- AfterSales: `AfterSalesModule.Interventions.cs`, `AfterSalesModule.Cancellations.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`
- Testler: `OrderCorrectionTests.Interventions.cs` (yeni), `OrderCorrectionTests.cs`, `DataExportTests.cs`, `LateDeliveryAndFirmCancellationTests.cs`, `InternalNoteSourceScanTests.cs` (yeni), `IrreversibleConfirmationTests.cs`
- Göç, yeni tablo, yeni ayar yok.

## Kabul Kriterleri Kontrolü

### Öz denetim (bitiş kapısı 2)

Devam işçisi bütün maddeleri (önceki işçinin yaptıkları dahil) diff'te yeniden denetledi; `sik-fail-nedenleri.md` okundu.

| Madde | Sonuç | Kanıt |
|---|---|---|
| k1 / KK1 kalem eklenmez, fiyat değişmez | ✓ | `RemoveItemRequest` fiyat/kalem alanı taşımaz; `OrderCorrectionTests` KK1 testi `IOrderCorrections`'ın 7 yöntemini sayar |
| k2 / KK2 ödeme öncesi, kargo sonrası fiziksel, teslim işaretli kalemde ret | ✓ | `BeforeThePaymentsApproval_NoItemIsRemoved_…_KK2`, `APhysicalItem_IsNotRemovedOnceTheOrderWasShipped_KK2` (3 durum), `ADeliveredDigitalItem_IsNotRemoved_…_KK2`, `MoreUnitsThanOpen_OrNone_AreRefused_…_KK2` |
| k3 / KK3 tutar donmuş değerden, hat doğru, havalede IBAN isteği | ✓ | `TheReduction_RefundsTheUnitsFrozenAmount_ToTheCard_…_KK3`, `OnTheTransferLine_TheRemovalOpensTheIbanRequest_AndSendsB14_KK3` |
| k4 / KK4 sayaçlar ve kargo ücreti iptal gibi | ✓ | `TheRemovedUnits_GoBackToStock_AndTheCouponRightOnlyWithTheOrdersLastUnit_…_KK4`, `TheLastPhysicalUnits_CarryTheShippingFee_…_KK4` |
| k5 / KK5 sebepsiz, iz, B-11 | ✓ | `TheRemoval_AsksNoReason_WritesTheTrail…_AndSendsB11_KK5` |
| k6 / KK6 kargo düzeltmesinde B-12 | ✓ | `TheCarrierCorrection_ChangesTheShipmentsValues_SendsB12_…_KK6`, `TheCarrierCorrection_IsClosedBeforeTheShipment_KK6` |
| k7 / KK7 tarih sınırları | ✓ | `TheDeliveryDate_IsNotInTheFuture_NotBeforeTheShipment_NorAfterAReturnsArrival_KK7`, `TheDeliveryDate_IsCorrectedForTheWholeOrder_…_KK7_KK9` |
| k8 / KK8 pencere yeni tarihten, eski beyan sürer, çakışmada cevapsız ret | ✓ | `TheWithdrawalWindow_RunsFromTheCorrectedDate_KK8`, `OnADeclarationsDay_TheCorrectionAsksWhichCameFirst_…_KK8` |
| k9 / KK9 ulaşma tarihi düzeltilir, ileri olamaz, bildirimsiz, izde eski/yeni | ✓ | `TheArrivalDate_IsCorrectedInPlace_…_WithoutANotice_KK9`; teslim tarihi testi de bildirimsiz |
| k10 / KK10 müşteriye görünmez, iz metinsiz, imha sonrası ret | ✓ | `TheInternalNote_IsWrittenChangedAndEmptied_…_KK10`, `ADisposedOrder_TakesNoInternalNote_KK10`, `InternalNoteSourceScanTests` (sayfa, e-posta şablonu, sözleşme notu okuyamaz; ihlal örnekleri kırmızı) |
| `09 §25` modül sınırı, arayüzde `SaveChanges` yok | ✓ | AfterSales yazıcıları çağıranın işleminde, `ModuleBoundaryTests` yeşil |
| `09 §25` zaman `TimeProvider` | ✓ | `_time.GetUtcNow()` |
| `09 §25` göç / yeni tablo / yeni ayar | ✓ (yok) | — |
| Güvenlik taraması | ✓ | Yalnız yönetici (`TransitionAdmin` yoksa `ActorNotAllowed`); karta para giden çıkarmada onay; iz satırları serbest metin taşımaz (test); yeni paket/uç yok |
| Sık FAIL 1: tarama istisnası geniş | ✓ | `InternalNoteSourceScanTests` istisnaları tam yol (5 dosya); göç ve bin/obj dışlaması ayrı testle sınandı |
| Sık FAIL 2: tarama bir biçimi kaçırıyor | ✓ | varlık, set, `Set<InternalNote>`, ham tablo adı, `InternalNoteId` ayrı örnekler; `SaveInternalNote`/`WriteInternalNote` sessiz |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ (yok) | yeni tablo/sütun yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `IrreversibleConfirmationTests` "Correct" sayımı AfterSales tarih yazıcısını adıyla dışlar; Unit tam koşu yeşil |
| Sık FAIL 5: 09 metni karar kaydısız | ✓ (yok) | 02/03/06/09 metni değişmedi |
| Sık FAIL 51: kurucuya bağımlılık, Integration TAM | ✓ | Integration tam koşu (aşağıda) |
| Sık FAIL 66: yeni kapanış türü açık adet okuyucularına | ✓ | `ItemRemovals` zaten `ClosedQuantitiesAsync`, kupon payı, geri ödeme tutarı, satış özeti okuyucularında; kapanış anları okuyucusuna bu task ekledi |
| Devredilen backlog | ✓ | D-848 kapatıldı (testle, `DEFERRED_BACKLOG.d/T384.md`); D-309 → F3 kapısı (belge sabit vermiyor, ürün sahibi kararı) |

## ⚠ Kolay kararlar (kurala en uygun seçenekle verildi)

1. Çıkarma düzeltmedir, iptal değildir (`02 §10.4.3`): kanuni faiz uyarısı çıkarmayı saymaz.
2. İade ulaşma tarihi düzeltmesi hem iade alımını (Return) hem iade reddini (ReturnRejection) kapsar.
3. `ExpectedVersion` T375'teki gibi isteğe bağlı; iç not sürüm dışında (`09 §10.9`), kilitle sıralanır.
4. D-309: belge uzunluk sabiti vermediği için sınır konmadı, F3 kapısına ileri atıldı.
5. `IrreversibleConfirmationTests`: AfterSales `CorrectArrivalDateAsync` geri alma değil yerinde tarih düzeltmesi olduğundan "tek Correct" sayımından adıyla dışlandı.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build -- --max-threads 3` | 5753/5753 |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests --no-build` (`postgres:18`, `track_commit_timestamp=on`) | tam koşu 3310: 3308 ✓, 2 göç testi yük altında okuma zaman aşımı → sınıfları ayrı koşuda 115/115 ✓; `OrderCorrectionTests` 46/46 |
| Lint | ✓ | `dotnet format <proje> --no-restore --verify-no-changes --include <değişen>` (3 proje) | temiz |
| Build | ✓ | `dotnet build` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
