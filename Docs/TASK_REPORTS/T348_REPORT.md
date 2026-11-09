# T348 — Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Onay işlemi:** `Ordering/OrderConfirmation.cs` — `ConfirmAsync` çağıranın işleminde (09 §9.1 m.3) çalışır: sepet kilidi → aynı sepetin ödenmemiş eski siparişinin sistem iptali (`SameCartReorder`, kilitten hemen sonra) → sepetin yeniden değerlendirilmesi → gösterilen özetle iki yönlü karşılaştırma → onay adımı denetimi (`IConfirmationStep`, T356) → numara ve erişim anahtarı, `Order.Create`, adres/kimlik/IBAN/sürüm/kalem/belge/kargo KDV bölüşümü/teslimat sözü dondurma → ödeme süresi (`PaymentWindows.StartAsync`, T324) → stok, kontenjan ve kupon hakkı ayırma → B-1 ve F-1 kuyruğu. Ret bir değerdir (`OrderConfirmationResult : IOperationResult`); işlem bütünüyle geri alınır, eski siparişin iptali de. `SummarizeAsync` ödeme adımının özetini aynı değerlendirmeyle üretir (gösterilen ve karşılaştırılan özet tek yoldan).
- **Özet karşılaştırması:** `Ordering/SummaryComparison.cs` — saf işlev; kalem çıktı/girdi, adet, birim fiyat, döküm (liste fiyatı, indirim, kupon payı, KDV oranı ve tutarı, ödenecek), kargo ücreti, toplam; ayrılamayan kalem adetsiz (`ItemNotReserved`).
- **Görünürlük kuralı:** `Ordering/OrderVisibility.cs` — müşteri listesinin tek kural evi: Ö1 kaydı olmayan ve sistemin dört gerekçeyle (süre doldu, kart başarısız, ödeme sayfası açılmadı, aynı sepetten yeni onay) S3/Ö2 yazdığı sipariş listede yok; panel süzmez. Tek sorguya çevrilir. Liste sayfası T518'in.
- **Yardımcılar:** `OrderCounters.ReserveAsync(Order, IReadOnlyList<OrderItem>)` (kaydedilmemiş kalemlerden ayırma; 1.4.4.1 ile aynı yazım), `Carts.OpenAsync(..., afterLock, ...)` aşırı yüklemesi, `ISettingsModule.GetBankTransferIbanAsync` (havale siparişinde IBAN dondurma), DI kaydı, `AutomaticMoments` 4.2.1'e `OrderConfirmation.ConfirmAsync`.

## Etkilenen Modüller / Dosyalar

- Ordering: `OrderConfirmation.cs` (yeni), `SummaryComparison.cs` (yeni), `OrderVisibility.cs` (yeni), `OrderCounters.cs`, `Carts.cs`, `OrderingModule.cs`
- Settings: `Contracts/ISettingsModule.cs`, `FirmSettings.cs`, `SettingsModule.cs`
- Infrastructure: `Jobs/AutomaticMoments.cs`
- Testler: `IntegrationTests/Ordering/OrderConfirmationTests.cs` (yeni, 9), `UnitTests/Ordering/SummaryComparisonTests.cs` (yeni, 7), `IntegrationTests/Ordering/OrderingTestServices.cs`, `IntegrationTests/Model/AddressTests.cs`, `MembershipTests.cs` (ayar sahteleri yeni yöntemi taşır)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Onay anında oluşma; numara, donma, ayırma birlikte; iki yolda numara; B-1, F-1 | ✓ | `OrderConfirmation.ConfirmAsync`; `Confirmation_IssuesTheNumber_FreezesTheOrder_ReservesStockAndCoupon_StartsTheWindow_AndQueuesB1AndF1_KK1_KK3` (kart), `BankTransferConfirmation_FreezesTheIban_AndStartsTheWindowInBusinessDays_KK4` (havale) |
| 2 | Yeniden değerlendirme; her fark durdurur, güncel özet yeniden onaylanır | ✓ | `SummaryComparison.Compare`; `APriceChange_EitherWay_StopsTheOrder_AndTellsWhatChanged_NothingIsBorn_KK2`, `ACartEmptiedAtConfirmation_BearsNoOrderNumberPaymentOrReservation_KK2`, `ARisenLegalTextVersion_StopsTheOrder_KK2`; `SummaryComparisonTests` (7) |
| 3 | Stok, kontenjan, kupon hakkı ayrılır; ödeme başarılı olunca kesin düşer | ✓ | `OrderCounters.ReserveAsync(Order, items)`; KK1_KK3 testi (reserved_quantity, reserved_count); kesin düşme mevcut `OrderCounters.CommitAsync` — `AxesTieTests.AMixedOrdersCardPayment_..._CommitsTheStockAndTheCoupon_KK4_KK8` |
| 4 | Kart süresi dakika/ürün sabiti; havale iş günü/panel girdisi, çit dışı ret; havalede ayırma süre boyunca | ✓ | `PaymentWindows.StartAsync` (T324) onayda çağrılır; `BankTransferConfirmation_..._KK4`; çit: `FirmSettingsTests.APeriodOutsideItsFence_IsRefusedWithTheSideAndTheFence_AndNothingIsStored`, `ThePeriodsAtTheirFences_AreAccepted` (T381) |
| 5 | Süre dolan / kart başarısız sipariş kendiliğinden iptal; ayırmalar tek anda serbest, ödeme Başarısız; kartta önce son sorgu | ✓ | `ExpiredOrder_FreesItsReservation_AndTheRetryIsANewOrderWithANewNumber_KK5_KK6`; mevcut `PaymentWindowTests.TransferWindowEnd_CancelsTheOrder_WithS3AndO2AndB7_AndReleasesItsReservation_KK6`, `CardWindowEnd_WritesOneFinalQueryRow_AndLeavesTheOrderAsItIs`, `FinalQuery_ThatFindsNoPayment_CancelsTheOrder_AndReleasesItsReservation_KK4`; `PaymentNotificationTests.Failure_CancelsTheUnpaidOrderInTheJob_S3WithO2AndTheCardFailureReason` |
| 6 | Tekrar yeni siparişle; aynı siparişi sürdürme yolu yok | ✓ | `ExpiredOrder_..._KK5_KK6` (yeni Id, yeni numara, eski İptal kalır); `PaymentWindowTests.CancelledTransferOrder_CannotBeMarkedPaidAfterwards_KK6` |
| 7 | Aynı sepetten yeni onayda ödenmemiş eski sipariş iptal; birleşen misafir sepeti istisna | ✓ | `ConfirmAsync` → `CancelEarlierUnpaidOrdersAsync`; `ANewConfirmationFromTheSameCart_CancelsTheEarlierUnpaidOrder_AndIsNotStoppedByItsReservation_KK7`, `TheMergedGuestCartsOrder_IsNotCancelledByTheAccountCartsConfirmation_KK7` |
| 8 | Hiç ödenmemiş, kendiliğinden iptal sipariş müşteri listesinde yok, panelde var | ✓ | `OrderVisibility.ShownToCustomer`; `TheNeverPaidOrderTheSystemCancelled_LeavesTheCustomersList_AndStaysInThePanel_KK8` |

### Öz denetim (bitiş kapısı 2)

> Devam turu: önceki işçinin yaptıkları dahil bütün maddeler diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: numara, donma, ayırma birlikte; B-1, F-1 | ✓ | `OrderConfirmation.cs` `ConfirmAsync` (IssueAsync → Create → Freeze* → ReserveAsync → QueueCustomerNoticeAsync/QueueCompanyNoticeAsync tek işlemde); `Confirmation_IssuesTheNumber_..._KK1_KK3` |
| KK2: her fark durdurur, yeniden onay; boşalan sepette sipariş/numara/ödeme/ayırma doğmaz | ✓ | `NoOrderableItem` dalı numaradan önce; `AssertNothingBornAsync` (orders, stock_reservations, dispatch_records, reserved_quantity = 0); üç KK2 testi + `SummaryComparisonTests` |
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
| Test — kural testleri, silinen/zayıflatılan test yok | ✓ | 16 yeni test; diff'te silinen test yok |
| Sık FAIL listesi | ✓ | Bekçi/tarama istisnası eklenmedi; 06 varsayılanı/FK değişmedi; sayı/etiket anan başka yer yok; 09 metni değişmedi; modül kurucusu değişmedi (yeni sınıf DI'da); Integration TAM koşuldu; `dotnet format` değişen dosyalarda temiz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 5313/5313 (head `f84d3c30`); devam turunda `--filter-class "*SummaryComparisonTests"` 7/7 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/...` namespace parçalarıyla (Postgres `track_commit_timestamp=on`) | 2897/2897 (Ordering 350, Model 1353, Catalog+Concurrency+CorporateContent 262, Jobs+Payment+Disposal+Outages 171, Commands+Database+FileSecrets+Files+Logging 251, Pages+Security+SignedEndpoints+Web+kök 510); devam turunda `*OrderConfirmationTests` 9/9 |
| Contract | uygulanamaz | — | Dış sözleşme değişmedi |
| Lint | ✓ | `dotnet format --verify-no-changes --include <değişen dosyalar>` | temiz |

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
| — | Yok (aşağıdaki ⚠ maddeleri başka task'ların kapsamında) | — | — |

## Notlar

- **Working tree (Adım -1):** temiz
- **hooksPath:** `scripts\git-hooks` ✓
- **Ana dal CI startup (Adım 0):** `42558302378` success, `42548005029` success, `42548005043` success
- **Repo memory (bitiş kapısı 9):** dokunulmadı (faz kapısı günceller)
- **⚠ a — kapsam:** teslimat ili kontrolü (T350), L-7/L-8 limitleri (T377 ve havale yolu task'ı) ve havalenin açık olup olmadığı denetimi onay işleminde yok; o task'lar kendi kurallarını onay yoluna bağlar. Onay sayfası ve POST ucu yok (Pages F5); kriter 1'in "numara gösterilir"i sonuç değerindeki `OrderNumber` ile kanıtlı.
- **⚠ b — kargo KDV bölüşüm tabanı:** kupon öncesi indirimli kalem tutarı (`DiscountedUnitPrice ?? UnitPrice` × adet), `ShippingBasisLine` sözleşmesiyle.
- **⚠ c — aynı sepetten yeni onay:** eski siparişin iptali sistem iptalidir; B-7 bildirimi `SameCartReorder` şablonuyla gider.
- Devam turu: önceki işçi bağlam sınırında bölündü (head `f84d3c30`); bu tur 09 §25 ve güvenlik taramasını diff'te denetledi, raporu, STATUS satırını ve changelog parçasını yazdı, PR'ı açtı. Kod değişmedi.
