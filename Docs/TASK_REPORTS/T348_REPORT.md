# T348 — Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 ✓ PASS; tur 1 ✗ FAIL → düzeltme 1) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Onay işlemi:** `Ordering/OrderConfirmation.cs` — `ConfirmAsync` çağıranın işleminde (09 §9.1 m.3) çalışır: sepet kilidi → aynı sepetin ödenmemiş eski siparişinin sistem iptali (`SameCartReorder`, kilitten hemen sonra) → sepetin yeniden değerlendirilmesi → gösterilen özetle iki yönlü karşılaştırma → fiziksel siparişte teslimat ilinin teslimat alanına karşı yeniden denetimi (`GetDeliveryAreaAsync`, düzeltme 1) → onay adımı denetimi (`IConfirmationStep`, T356) → numara ve erişim anahtarı, `Order.Create`, adres/kimlik/IBAN/sürüm/kalem/belge/kargo KDV bölüşümü/teslimat sözü ve dijital/hizmet kutu kaydı (`ConfirmationAcceptances.Records`, düzeltme 1) dondurma → ödeme süresi (`PaymentWindows.StartAsync`, T324) → stok, kontenjan ve kupon hakkı ayırma → B-1 ve F-1 kuyruğu. Ret bir değerdir (`OrderConfirmationResult : IOperationResult`); işlem bütünüyle geri alınır, eski siparişin iptali de. `SummarizeAsync` ödeme adımının özetini aynı değerlendirmeyle üretir (gösterilen ve karşılaştırılan özet tek yoldan).
- **Özet karşılaştırması:** `Ordering/SummaryComparison.cs` — saf işlev; kalem çıktı/girdi, adet, birim fiyat, döküm (liste fiyatı, indirim, kupon payı, KDV oranı ve tutarı, ödenecek), kargo ücreti, toplam, kargoya verme süresi (`ShippingPeriod`, düzeltme 1 K4); ayrılamayan kalem adetsiz (`ItemNotReserved`); teslimat alanı farkı `DeliveryArea` (düzeltme 1).
- **Görünürlük kuralı:** `Ordering/OrderVisibility.cs` — müşteri listesinin tek kural evi: Ö1 kaydı olmayan ve sistemin dört gerekçeyle (süre doldu, kart başarısız, ödeme sayfası açılmadı, aynı sepetten yeni onay) S3/Ö2 yazdığı sipariş listede yok; panel süzmez. Tek sorguya çevrilir. Liste sayfası T518'in.
- **Yardımcılar:** `OrderCounters.ReserveAsync(Order, IReadOnlyList<OrderItem>)` (kaydedilmemiş kalemlerden ayırma; 1.4.4.1 ile aynı yazım), `Carts.OpenAsync(..., afterLock, ...)` aşırı yüklemesi, `ISettingsModule.GetBankTransferIbanAsync` (havale siparişinde IBAN dondurma), DI kaydı, `AutomaticMoments` 4.2.1'e `OrderConfirmation.ConfirmAsync`.

## Etkilenen Modüller / Dosyalar

- Ordering: `OrderConfirmation.cs` (yeni), `SummaryComparison.cs` (yeni), `OrderVisibility.cs` (yeni), `OrderCounters.cs`, `Carts.cs`, `OrderingModule.cs`
- Settings: `Contracts/ISettingsModule.cs`, `FirmSettings.cs`, `SettingsModule.cs`
- Infrastructure: `Jobs/AutomaticMoments.cs`
- Testler: `IntegrationTests/Ordering/OrderConfirmationTests.cs` (yeni, 15), `UnitTests/Ordering/SummaryComparisonTests.cs` (yeni, 8), `IntegrationTests/Ordering/OrderingTestServices.cs`, `IntegrationTests/Model/AddressTests.cs`, `MembershipTests.cs` (ayar sahteleri yeni yöntemi taşır)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Onay anında oluşma; numara, donma, ayırma birlikte; iki yolda numara; B-1, F-1 | ✓ | `OrderConfirmation.ConfirmAsync`; `Confirmation_IssuesTheNumber_FreezesTheOrder_ReservesStockAndCoupon_StartsTheWindow_AndQueuesB1AndF1_KK1_KK3` (kart), `BankTransferConfirmation_FreezesTheIban_AndStartsTheWindowInBusinessDays_KK4` (havale); kutu kaydı (B1 düzeltmesi, `02 §3.23.3`, §3.24.6): `AMixedConfirmation_FreezesTheDigitalAndTheServiceBox_WithTheTextShownAndTheItemsTheyCover_InTheSameGo_KK1`, `AnUntickedDigitalBox_StopsTheOrder_AndNoBoxRecordIsBorn_KK1_KK2` |
| 2 | Yeniden değerlendirme; her fark durdurur, güncel özet yeniden onaylanır | ✓ | `SummaryComparison.Compare`; `APriceChange_EitherWay_StopsTheOrder_AndTellsWhatChanged_NothingIsBorn_KK2`, `ACartEmptiedAtConfirmation_BearsNoOrderNumberPaymentOrReservation_KK2`, `ARisenLegalTextVersion_StopsTheOrder_KK2`; teslimat illeri (B2 düzeltmesi, `02 §6.2.3`): `ADeliveryAreaThatNoLongerServesTheShippingProvince_OrAPhysicalOrderWithoutAShippingAddress_StopsTheOrder_KK2`, `TheDeliveryArea_RestrictsTheShippingProvinceOnly_ADigitalOrderWithNoShippingAddressIsBorn_WhenNoProvinceIsServed_KK2`; kargo ücreti ve asgari tutar `AShippingFeeOrAMinimumAmountSetAfterTheSummary_StopsTheOrder_KK2`; kargoya verme süresi (K4) `AShippingPeriodChange_AfterTheSummary_StopsTheOrder_AndTellsBothPeriods_KK2_K4`; `SummaryComparisonTests` (8) |
| 3 | Stok, kontenjan, kupon hakkı ayrılır; ödeme başarılı olunca kesin düşer | ✓ | `OrderCounters.ReserveAsync(Order, items)`; KK1_KK3 testi (reserved_quantity, reserved_count); kesin düşme mevcut `OrderCounters.CommitAsync` — `AxesTieTests.AMixedOrdersCardPayment_..._CommitsTheStockAndTheCoupon_KK4_KK8` |
| 4 | Kart süresi dakika/ürün sabiti; havale iş günü/panel girdisi, çit dışı ret; havalede ayırma süre boyunca | ✓ | `PaymentWindows.StartAsync` (T324) onayda çağrılır; `BankTransferConfirmation_..._KK4`; çit: `FirmSettingsTests.APeriodOutsideItsFence_IsRefusedWithTheSideAndTheFence_AndNothingIsStored`, `ThePeriodsAtTheirFences_AreAccepted` (T381) |
| 5 | Süre dolan / kart başarısız sipariş kendiliğinden iptal; ayırmalar tek anda serbest, ödeme Başarısız; kartta önce son sorgu | ✓ | `ExpiredOrder_FreesItsReservation_AndTheRetryIsANewOrderWithANewNumber_KK5_KK6`; mevcut `PaymentWindowTests.TransferWindowEnd_CancelsTheOrder_WithS3AndO2AndB7_AndReleasesItsReservation_KK6`, `CardWindowEnd_WritesOneFinalQueryRow_AndLeavesTheOrderAsItIs`, `FinalQuery_ThatFindsNoPayment_CancelsTheOrder_AndReleasesItsReservation_KK4`; `PaymentNotificationTests.Failure_CancelsTheUnpaidOrderInTheJob_S3WithO2AndTheCardFailureReason` |
| 6 | Tekrar yeni siparişle; aynı siparişi sürdürme yolu yok | ✓ | `ExpiredOrder_..._KK5_KK6` (yeni Id, yeni numara, eski İptal kalır); `PaymentWindowTests.CancelledTransferOrder_CannotBeMarkedPaidAfterwards_KK6` |
| 7 | Aynı sepetten yeni onayda ödenmemiş eski sipariş iptal; birleşen misafir sepeti istisna | ✓ | `ConfirmAsync` → `CancelEarlierUnpaidOrdersAsync`; `ANewConfirmationFromTheSameCart_CancelsTheEarlierUnpaidOrder_AndIsNotStoppedByItsReservation_KK7`, `TheMergedGuestCartsOrder_IsNotCancelledByTheAccountCartsConfirmation_KK7` |
| 8 | Hiç ödenmemiş, kendiliğinden iptal sipariş müşteri listesinde yok, panelde var | ✓ | `OrderVisibility.ShownToCustomer`; `TheNeverPaidOrderTheSystemCancelled_LeavesTheCustomersList_AndStaysInThePanel_KK8` |

> **Doğrulayıcı kanıtı (tur 2):** sekiz kriter ✓. Kod `OrderConfirmation.cs` satır satır `02 §3.17.1`–§3.17.8, §6.2.3, §6.2.15 ile karşılaştırıldı; yukarıdaki testlerin hepsi yerelde `--filter-namespace Shopfolio.IntegrationTests.Ordering` (390/390), `FirmSettingsTests` (24/24, KK4 çiti) ve Unit (5417/5417, `SummaryComparisonTests` dahil) koşusunda yeşil; dal CI 42570842338 yeşil. Kriter 1 kutu kaydı ve kriter 2 teslimat alanı tur 1'de `~` idi, düzeltme 1 ile ✓.

### Öz denetim (bitiş kapısı 2)

> Devam turu: önceki işçinin yaptıkları dahil bütün maddeler diff'te yeniden denetlendi. **Düzeltme 1 (2026-11-09):** bütün kabul kriterleri, KK1–KK8 ve kontrol listesi birleşik ağaçta (`origin/main` `f7d2febe` dahil) yeniden denetlendi; T348'e yönlenmiş backlog kalemleri tek tek tarandı (aşağıdaki satır).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: numara, donma, ayırma birlikte; B-1, F-1 | ✓ | `OrderConfirmation.cs` `ConfirmAsync` (IssueAsync → Create → Freeze* → `ConfirmationAcceptances.Records` → ReserveAsync → QueueCustomerNoticeAsync/QueueCompanyNoticeAsync tek işlemde); `Confirmation_IssuesTheNumber_..._KK1_KK3`; kutu kaydı `AMixedConfirmation_..._KK1` (tür, gösterilen metin = adımın teklifi, iki dijital + hizmet kalemi bağlı, fiziksel bağlı değil), işaretsiz kutuda kayıt yok `AnUntickedDigitalBox_..._KK1_KK2` |
| KK2: her fark durdurur, yeniden onay; boşalan sepette sipariş/numara/ödeme/ayırma doğmaz | ✓ | `NoOrderableItem` dalı numaradan önce; `AssertNothingBornAsync` (orders, stock_reservations, dispatch_records, reserved_quantity = 0); yedi KK2 testi (fiyat, boşalan sepet, metin sürümü, teslimat alanı + adressiz fiziksel sipariş, dijitalde alan kısıtı yok, kargo ücreti + asgari tutar, kargoya verme süresi) + `SummaryComparisonTests` |
| KK3: ayırma ve kesin düşme ödeme sonucuna göre | ✓ | Ayırma KK1_KK3 testi; kesin düşme `AxesTieTests ..._KK4_KK8`; serbest bırakma KK5 testi |
| KK4: havalede çit dışı süre reddedilir; ayırma süresince sürer | ✓ | `FirmSettingsTests.APeriodOutsideItsFence_...`; `BankTransferConfirmation_..._KK4` (süre iş günüyle, ayırma yerinde); `PaymentWindowTests.TransferWindow_ThatStillRuns_OrAPaidOrder_IsNotCancelled` |
| KK5: süre dolunca ayırmalar tek anda döner, ödeme Başarısız | ✓ | `ExpiredOrder_..._KK5_KK6`; `PaymentWindowTests.TransferWindowEnd_..._KK6`, `FinalQuery_ThatFindsNoPayment_..._KK4` |
| KK6: tekrar yeni numarayla | ✓ | `ExpiredOrder_..._KK5_KK6` |
| KK7: önceki ödenmemiş sipariş iptal, istisna sınanır | ✓ | iki KK7 testi (S3 + `SameCartReorder`, ayırma satırı yok; misafir siparişi Bekliyor kalır) |
| KK8: müşteri listesinde yok, panelde var | ✓ | `OrderVisibility.cs`; KK8 testi |
| 09 §25 — doküman uyumu ve kapsam | ✓ | Yeni ad yok (06'daki alanlar, `TransitionReason` değerleri mevcut); yeni ayar/sabit yok (§3.5 değişmez); sayfa/POST yok (Pages F5) — kapsam dışı davranış eklenmedi |
| 09 §25 — modül sınırı, arayüzde SaveChanges yok | ✓ | `ConfirmAsync` SaveChanges çağırmaz, işlem açmaz (çağıranın işlemi); Settings'e yalnız `ISettingsModule` (Contracts) ile, Catalog'a `ICatalogModule` ile erişim |
| 09 §25 — tek işlev | ✓ | Para: `_catalog.PriceItem`, `SumOrderTotal`, `SplitShippingFee`, `EvaluateCouponAsync` (§14); süre `PaymentWindows`; numara `OrderIdentifiers`; iptal `OrderTransitions.CancelOrderAsync`; ayırma `OrderCounters` yazımı (`WriteRows`) paylaşılır; özet satırı `ConfirmationSummaryText.LineOf` (T356) |
| 09 §25 — zaman `TimeProvider` | ✓ | `_time.GetUtcNow()`; testler `FakeTimeProvider` ile (`OrderingTestServices.Build(dsn, TimeProvider)`) |
| 09 §25 — durum geçişi, işlem sırası, ret geri alır | ✓ | İptal yalnız `OrderTransitions` geçiş birimiyle; ret değer → çalıştırıcı geri alır (KK2/KK7 testlerinde eski sipariş ve ayırma geri dönüyor) |
| 09 §25 — eşzamanlılık / kilit sırası | ✓ | Sıra: sepet danışma kilidi (başta, tek) → siparişler Id sırasıyla (`OrderBy(o => o.Id)` + `OrderLocks`) → varyantlar → kupon; `LockSequence` çalışma anında denetler, Integration testleri yeşil |
| 09 §25 — arka plan işi (§13) | ✓ | Yeni iş türü yok; B-1/F-1 mevcut bildirim kuyruğu; 4.2.1 `AutomaticMoments` kaydı |
| 09 §25 — para (§14) | ✓ | Tutarlar yalnız Catalog para işlevlerinden |
| 09 §25 — veri ve göç | uygulanamaz | Göç, yeni tablo, yeni değer, yük biçimi yok |
| 09 §25 — sayfalı liste | uygulanamaz | Liste sayfası yok (T518); `ShownToCustomer` yalnız süzgeç, `IQueryable` döner |
| Güvenlik taraması | ✓ | Yeni POST/uç/sayfa yok; sır ve kişisel veri kayda yazılmıyor (kayıt satırı yok); erişim anahtarı ve sipariş anahtarı `OrderIdentifiers` / `SecretKey.Create()` (§15.1; `Guid`/`Random` yok); `Html.Raw` yok; ham SQL yok (testlerdeki sayım sorguları hariç) |
| Devredilen backlog kalemleri (sık FAIL: T348 r1) | ✓ | D-732 ✅ (B1), D-494 ✅ (B2), D-465 ✅, D-570 ✅ (değer karşılaştırması; gerekçe kayıtta), D-621 ✅; T348 payı kapanıp kalanı ileri atılan: D-437 → T509/T518, D-453 → T367…T373, D-483 → T349/T509/T544/T367/T373, D-563 → T509 (benzersiz dizinle yeniden deneme işlemin sahibinde), D-626 → T509 (303 yanıtı), D-705 → T356 |
| Test — kural testleri, silinen/zayıflatılan test yok | ✓ | 23 yeni test (düzeltme 1: Integration 6, Unit 1); diff'te silinen test yok; tohum adımına teslimat ili satırı eklendi (`CompanyDeliveryProvince`) |
| Sık FAIL listesi | ✓ | Bekçi/tarama istisnası eklenmedi; 06 varsayılanı/FK değişmedi; sayı/etiket anan başka yer yok; 09 metni değişmedi; modül kurucusu değişmedi (yeni sınıf DI'da); Integration TAM koşuldu; `dotnet format` değişen dosyalarda temiz. Düzeltme 1: modül kurucusu değişmedi, yeni göç/değer yok; `dotnet format --verify-no-changes` üç projede temiz; devredilen backlog kalemleri tarandı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 5313/5313 (head `f84d3c30`); düzeltme 1 birleşik ağaçta 5417/5417 (Release, `--no-build`) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/...` namespace parçalarıyla (Postgres `track_commit_timestamp=on`) | 2897/2897 (Ordering 350, Model 1353, Catalog+Concurrency+CorporateContent 262, Jobs+Payment+Disposal+Outages 171, Commands+Database+FileSecrets+Files+Logging 251, Pages+Security+SignedEndpoints+Web+kök 510); düzeltme 1 (konteyner `shopfolio-t348-fix-pg`, `track_commit_timestamp=on`): `--filter-namespace Shopfolio.IntegrationTests.Ordering` 389/389 (`*OrderConfirmationTests` 15/15, son ekten sonra); `*.AddressTests *.MembershipTests *.CartTests *.CartQuantityTests *.SettingsTablesTests` 209/209 |
| Contract | uygulanamaz | — | Dış sözleşme değişmedi |
| Lint | ✓ | `dotnet format --verify-no-changes --include <değişen dosyalar>`; düzeltme 1: `dotnet format <proje> --verify-no-changes --no-restore` ×3 | temiz (exit 0) |
| Build | ✓ | düzeltme 1: `dotnet build Shopfolio.slnx --no-restore -c Release -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS — tur 2 (2026-11-09, head `b2dd4384` + `origin/main` `94cad6d5` (#490) birleştirmesi, PR #489). Kriter 1–8 ve KK1–KK8 ✓, `09 §25` ve güvenlik taraması temiz; dal CI 42570842338 (`b2dd4384`) yeşil (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate); ana dal son 3 run (42567222932, 42567222878, 42564337700) success. Yerelde (uçta): `dotnet restore` + `--locked-mode` exit 0, derleme `-warnaserror` 0/0 (birleştirmeden sonra da), `dotnet format --verify-no-changes` 3 proje temiz, Unit (+Consistency, Architecture) 5417/5417, betik testleri 4/4, Integration (`shopfolio-t348-val-pg`, `track_commit_timestamp=on`): `--filter-namespace Shopfolio.IntegrationTests.Ordering` 390/390, `AddressTests` + `MembershipTests` 142/142, `FirmSettingsTests` 24/24 |
| Tur 1 bulgularının kapanışı | **B1** ✓ — `ConfirmAsync` onay adımı `Ready` olduktan sonra `ConfirmationStepRules.Offers(özet satırları, yöntem)`'i `ConfirmationAcceptances.Records`'a verir, kutu ve kalem bağı satırları siparişle aynı işlemde eklenir; `AMixedConfirmation_..._KK1` (dijital + hizmet kaydı, gösterilen metin = adımın teklifi, iki dijital kalem bağlı, fiziksel bağlı değil), `AnUntickedDigitalBox_..._KK1_KK2` (kayıt doğmaz). **B2** ✓ — fiziksel kalemde `GetDeliveryAreaAsync().IsServed(teslimat ili)` ya da teslimat adresi yokluğu `DeliveryArea` farkıyla durdurur; dijital siparişe alan uygulanmaz (`02 §3.20.1`); iki test. **K3** ✓ — D-732, D-494, D-465, D-570, D-621 ✅; D-437, D-453, D-483, D-563, D-626, D-705 T348 payı kapandı, kalan gerekçeyle ileri. **K4** ✓ — `SummaryChangeKind.ShippingPeriod`; unit + integration testi |
| Bulgu sayısı (tur 2) | 0 |
| Bloke-etmeyen not (tur 2) | 1 — K1: T348'in üç yeni null-forgiving `!` kullanımı gerekçe yorumu taşımıyor (`09 §3.1` m.5); kalıp `origin/main`'de 61 satır, emsal doğrulamalar bulgu açmamış → D-778 (F3 gate, tek tarama) |
| Düzeltme gerekli mi (tur 2) | Hayır |
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-09, head `83000d3a` + `origin/main` `f7d2febe` (T371) birleştirmesi). Kriter 3–8 ve KK3–KK8 ✓; kriter 1 / KK1 ~ (B1), kriter 2 / KK2 ~ (B2); güvenlik taraması temiz; dal CI 42564653239 yeşil (9 iş). Yerelde derleme `-warnaserror` 0/0, `dotnet format --verify-no-changes` 3 proje temiz, Unit 5416/5416, betik testleri 4/4, Integration: Ordering ad alanı 384/384, `AddressTests` + `MembershipTests` 142/142 (birleştirme öncesi ağaçta da dokunulan 10 sınıf 252/252) |
| Bulgu sayısı (tur 1) | 2 — **B1 KRİTİK S1** (`02 §3.17.1` → §3.23.3, §3.24.6; VAL-D016; D-732): onay, dijital ve hizmet kalemi için işaretlenen kutuların kaydını siparişe dondurmuyor. `ConfirmationAcceptances.Records` (T356; sınıf yorumu "T348 calls this in the confirmation's transaction") üretimde hiç çağrılmıyor; `CheckboxAcceptance` / `CheckboxAcceptanceOrderItem` satırını yazan üretim yolu yok (`grep -rn "ConfirmationAcceptances.Records\|CheckboxAcceptances.Add" src` boş). Dijital kalemli sipariş cayma hakkının düşmesine dair kanıtsız doğar (`02 §3.24.6`: "ispat firmadadır"). T348 testleri yalnız fiziksel kalem kurar. **B2 ORTA S1** (`02 §3.17.2`, §6.2.3; D-494 "T348 onayda alanı yeniden okur"): onay teslimat alanını okumaz (`ConfirmAsync`, `Carts.OpenAsync`, `ShippingFeeSection` il taşımaz); özet gösterildikten sonra teslimat illeri değişirse — teslimat adresinin ili artık hizmet dışında olsa bile — sipariş doğar; `request.Shipping` hiç denetlenmez (fiziksel siparişte `null` ise teslimat adressiz donar). Raporun ⚠ a'sı il kontrolünü T350'ye bırakıyor; T350 bunu D-494 ile T348'e yönlendirmişti |
| Düzeltme gerekli mi (tur 1) | Evet — (B1) `ConfirmAsync`, onay adımı denetimi hazır olduktan sonra `ConfirmationStepRules.Offers(current.Summary.Lines, method)`'ın kutu tekliflerini `ConfirmationAcceptances.Records(order.Id, offers, items, now)`'a verir ve satırları aynı işlemde ekler; dijital ve hizmet kalemli onay testi kayıtları (tür, gösterilen metin, kapsanan kalemler) okur. (B2) Onay teslimat alanını aynı işlemde okur; fiziksel kalemli siparişte teslimat adresinin ili hizmet dışındaysa ya da gösterilen özetten sonra alan değiştiyse sipariş oluşmaz ve fark söylenir (`02 §6.2.3`); fiziksel siparişte teslimat adresi yoksa ret; test alanı değiştirip onaylar. D-494 ve D-732 kapatılır |
| Bloke-etmeyen not (K) sayısı (tur 1) | 4 — K1: `SummarizeAsync` müşterinin kendi önceki ödenmemiş siparişinin ayırmasına takılır, ödeme adımı açılmayabilir → D-769 (T509). K2: havale onayında B-2 kuyruğa girmiyor (`03 §4.2.1`) → D-770 (T353). K3: T348'e yönlenmiş backlog kalemleri (D-437, D-453 1.4.4.1 payı, D-465, D-483, D-563, D-570, D-621, D-626, D-705; B1/B2 ile D-732, D-494) kodda büyük ölçüde karşılandı ama kayıtta açık kaldı ve rapor anmıyor → düzeltme turunda bu PR'da kapatılır (çözüldü işareti ya da kalan payın yeni hedefi). K4: `SummaryComparison` özetin `ShippingPromise`'ını karşılaştırmaz; ürünün kargoya verme süresi özet ile onay arasında değişirse donan form müşterinin görmediği sözü yazar (`02 §3.24.3` formun eklediği kargoya verme süresi) → düzeltme turunda bu PR'da kapatılır |

**Task satırları — tur 2 (`12 §7.5`):** VAL-D016 FAIL → **BEKLEMEDE**: kutu kaydının veri kaydı yarısı kanıtlı (`AMixedConfirmation_..._KK1`; kayıt sonradan değişmez — onay dışında `CheckboxAcceptances`'a yazan yol yok), uç yanıtı yarısı sipariş sayfası ister (T509/T518; D013–D015 emsali). VAL-C001, D006, D008, D012 PASS kalır (Ordering 390/390 ile yeniden koştu). Öteki 31 satır BEKLEMEDE (tur 1 gerekçesi değişmedi). `12_matris_denetle.py --kapsam 1-77` Hata 0.

**Doğrulayıcı uyuşmazlık notları — tur 2 (adım 13):** Öz denetim tablosu kriterlerde ve tur 1 bulgularında doğrulayıcıyla uyumlu. Tek uyuşmazlık: tablo `09 §25` "C# standartları (`!`, …)" sorusunu satır olarak taşımıyor; üç yeni gerekçe yorumsuz `!` bu yüzden görünmedi (K1 → D-778).

**Task satırları — tur 1 (`12 §7.5`):** KP-8, KP-11, KP-14…KP-17 (`grep -nE '^\| VAL-[A-Z0-9]+ \|[^|]*\|[^|]*\| 10 KP-(8|11|14|15|16|17)([^0-9]|$)'`) → 36 satır. **PASS:** VAL-C001 (süre dolan sipariş kendiliğinden iptal — `PaymentWindowTests.TransferWindowEnd_..._KK6`, `FinalQuery_ThatFindsNoPayment_..._KK4`), VAL-D006 ve VAL-D008 (ayırma onayda — `Confirmation_..._KK1_KK3`; kesin düşme Ö1'de — `AxesTieTests.AMixedOrdersCardPayment_..._KK4_KK8`), VAL-D012 (söz onayda donar — `Confirmation_..._KK1_KK3` `ShippingPeriod`). **FAIL:** VAL-D016 (B1). **BEKLEMEDE:** öteki 31 satır uç yanıtı ya da tarayıcı gözlemi ister (T348 uç ve sayfa kurmaz; T509/T510) ya da başka task'ın kaynağıdır (VAL-D007 kontenjan T348 testinde yok; VAL-D009 numara T354). `12_matris_denetle.py --kapsam 1-77` Hata 0.

**Doğrulayıcı uyuşmazlık notları (adım 13):** (1) Öz denetim KK1'i ✓ veriyor; donma listesi `02 §3.23.3`'ün kutu kaydını içermiyor (B1) ve D-732 rapor ile kayıtta anılmıyor. (2) ⚠ a teslimat ili kontrolünü T350'ye veriyor; T350 bunu D-494 ile T348'e yönlendirmişti (B2). (3) "Known Limitations: Yok" — T348'e yönlenmiş on bir backlog kalemi açık kaldı (K3). (4) Test sayıları birleştirme öncesi ağacın (Unit 5313); doğrulama birleştirme sonrası ağaçta da koştu (Unit 5416).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T348-siparis-onayi`
- **Commit:** `d7bee8a9` — sipariş onayı işlemi, özet karşılaştırması, görünürlük kuralı; `f84d3c30` — onay testleri ve 4.2.1 kaydı; rapor/kayıt commit'i
- **PR:** #489
- **CI:** *(PR CI'ı — yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok — dış servis çağrısı yok (kart yönlendirmesi işlem kesinleştikten sonra, T317), yeni paket yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ödeme adımının özeti müşterinin kendi önceki ödenmemiş siparişinin ayırmasına takılır (doğrulama K1) | D-769 | Ödeme adımı sayfası (T509) |
| 2 | Havale onayında B-2 kuyruğu (doğrulama K2) | D-770 | Havale bildirimi T353 |
| 3 | Onayı çağıran sayfanın işi: adres formu çözümü, benzersiz dizinle yeniden deneme, ret yanıtları (303 `/sepet`, 422) | D-437, D-563, D-626 (D-570 gerekçesi) | Sayfa ve POST ucu T509'da; `ConfirmAsync` çağıranın işleminde çalışır (09 §9.1 m.3) |

## Notlar

- **Working tree (Adım -1):** temiz
- **hooksPath:** `scripts\git-hooks` ✓
- **Ana dal CI startup (Adım 0):** `42558302378` success, `42548005029` success, `42548005043` success
- **Repo memory (bitiş kapısı 9):** dokunulmadı (faz kapısı günceller)
- **⚠ a — kapsam:** teslimat ili kontrolü düzeltme 1'de onaya bağlandı (D-494); L-7/L-8 limitleri (T377 ve havale yolu task'ı) ve havalenin açık olup olmadığı denetimi onay işleminde yok; o task'lar kendi kurallarını onay yoluna bağlar. Onay sayfası ve POST ucu yok (Pages F5); kriter 1'in "numara gösterilir"i sonuç değerindeki `OrderNumber` ile kanıtlı.
- **⚠ b — kargo KDV bölüşüm tabanı:** kupon öncesi indirimli kalem tutarı (`DiscountedUnitPrice ?? UnitPrice` × adet), `ShippingBasisLine` sözleşmesiyle.
- **⚠ c — aynı sepetten yeni onay:** eski siparişin iptali sistem iptalidir; B-7 bildirimi `SameCartReorder` şablonuyla gider.
- Devam turu: önceki işçi bağlam sınırında bölündü (head `f84d3c30`); bu tur 09 §25 ve güvenlik taramasını diff'te denetledi, raporu, STATUS satırını ve changelog parçasını yazdı, PR'ı açtı. Kod değişmedi.
- **Düzeltme 1 (2026-11-09; doğrulama tur 1 B1, B2, K3, K4):** (B1) `ConfirmAsync` onay adımı denetimi hazır olduktan sonra `ConfirmationStepRules.Offers(özet satırları, yöntem)`'i `ConfirmationAcceptances.Records`'a verir; dijital ve hizmet kutusunun türü, gösterilen metni ve kapsadığı kalemler siparişle aynı işlemde yazılır (D-732 ✅). (B2) Onay teslimat alanını aynı işlemde `GetDeliveryAreaAsync` ile yeniden okur; fiziksel siparişte teslimat ili hizmet dışındaysa ya da teslimat adresi yoksa sipariş oluşmaz, güncel özet `DeliveryArea` satırıyla döner; dijital/hizmet siparişine alan kısıtı uygulanmaz (`02 §3.20.1`) (D-494 ✅). (K4) `SummaryComparison` kargoya verme süresini de karşılaştırır (`ShippingPeriod`). (K3) T348'e yönlenmiş kalemler kapatıldı ya da kalan payları gerekçeyle ileri atıldı (öz denetim tablosundaki satır). K1/K2 backlog'da (D-769, D-770). `origin/main` birleştirmesi değişiklik getirmedi (Already up to date).
