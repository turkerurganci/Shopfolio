# T325 — Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Teslim sürelerinin tek kural evi** — `Modules/Ordering/DeliveryPeriodRules.cs`: Z-10 kargoya verme sözünün son günü, bitişi ve göstergesi (iş günü, ödeme onayından); 0 iş günlük söz sayımdan önce ayrı kolla — son gün onayın kendi günü (D-161); Z-39 ifa süresi (takvim günü, ödeme onayından); Z-11 yasal teslim üst sınırı (`LegalDeliveryLimitDays = 30`, sipariş onayından) ve gecikme feshinin açık olup olmadığı (ödeme onaylı, iptal değil, teslim tarihi girilmemiş ve işleme açık adedi kalan fiziksel kalem; kargodaki siparişte de); süre aşıldıktan sonraki iptalde kanuni faiz uyarısı (fiziksel kalemde Z-10, hizmette Z-39; dijitalde yok).
- **Okuyucu** — `Modules/Ordering/OrderPeriods.cs` (DI'da scoped): ödeme onayı saklanmaz, siparişin hiçbir düzeltme kaydının göstermediği son Ö1 kaydının anıdır (kartta başarı bildirimi, havalede "ödendi" işareti; düzeltilip yeniden verilen işaret yeni andan sayılır); Z-10 göstergesi yalnız Hazırlanıyor'da, Z-39 göstergesi "tamamlandı" işaretine ve kalemin kayıtlarla kapanmasına kadar; Z-11 açık mı (kapanan adet `IAfterSalesModule.ClosedQuantitiesAsync`'ten); faiz uyarısı. Yalnız okuma: yazmaz, kilitlemez, süre dolunca işlem yapmaz.
- **Cayma, geri ödeme ve ayıp sürelerinin tek kural evi** — `Modules/AfterSales/AfterSalesPeriodRules.cs`: Z-13 (fiziksel: teslim tarihinden 14 gün; tarih yoksa başlamaz, düğme kapanmaz), Z-14 (hizmet: sipariş tarihinden ya da fiziksel kalem taşıyan siparişte siparişin teslim tarihinden; "tamamlandı" işareti de kapatır, hangisi önce), Z-15 (dijital: düğme hiç açık değil), Z-16 başlangıç günü ve göstergesi (teslimden önce/hizmette beyandan, teslimden sonra ulaşma ile beyanın geç olanından; mal ulaşmadan başlamaz), Z-17 göstergesi yalnız havale hattında (kartta null), Z-42 gönderme süresi ve "mal dönmedi" kapanışının açılması, Z-18 ayıp kanalının bitişi (teslim gününden 2 yıl, ertesi gün 00:00) ve açık olup olmadığı. Z-47 ve Z-19 için kural yoktur (sınıf yorumunda gerekçesiyle).
- **Panel süre göstergesi değeri** — `Infrastructure/PeriodStanding.cs`: son gün, bitiş anı, kalan gün (iş günlü dönemde iş günüyle), aşılan gün (takvim günü); okuma anında hesaplanır, saklanmaz (`04 2.6.3`, 2.6.6).
- **Zamanlayıcı yok taraması** — `tests/Shopfolio.UnitTests/Architecture/PeriodTimerScanTests.cs`: hiçbir `JobType` adı bu süreleri anmaz; yansımayla bulunan her zamanlayıcı kaynağı, kesinti etkisi, gecelik adım ve iş işleyicisi bu kuralları, sayımlı dönemleri, teslim/beyan/ulaşma tarihlerini ve indirme sayacını okumaz; Z-47'nin sayımlı dönemi yalnız listelendiği ve sayıldığı iki dosyada geçer; desenin yakaladığı ve yakalamadığı örnekler ayrı testte.
- **Backlog** — D-161 yerinde ✅; yeni D-512 (`doc-drift`, 0 iş günlük sözün anlamı `02`'ye karar kaydıyla), D-513 (`task`, kuralların üretimde bağlanması: T367, T368, T370–T374, T382) — `Docs/DEFERRED_BACKLOG.d/T325.md`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/PeriodStanding.cs` (yeni)
- `src/Shopfolio.Web/Modules/Ordering/DeliveryPeriodRules.cs` (yeni), `OrderPeriods.cs` (yeni), `OrderingModule.cs` (`AddScoped<OrderPeriods>()`)
- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesPeriodRules.cs` (yeni)
- `tests/Shopfolio.UnitTests/Infrastructure/PeriodStandingTests.cs`, `Ordering/DeliveryPeriodRulesTests.cs`, `AfterSales/AfterSalesPeriodRulesTests.cs`, `Architecture/PeriodTimerScanTests.cs` (yeni)
- `tests/Shopfolio.IntegrationTests/Ordering/DeliveryPeriodTests.cs` (yeni)
- Kayıt: `Docs/IMPLEMENTATION_STATUS.md` (T325 satırı), `Docs/STATUS_CHANGELOG.d/T325.md`, `Docs/DEFERRED_BACKLOG.d/T325.md`, `Docs/DEFERRED_BACKLOG.md` (D-161 ✅)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Z-10 ve Z-39 ödeme onayından; aşım kendiliğinden işlem üretmez, kalem sayaçta görünür | ✓ | `DeliveryPeriodRules.cs:27-60`, `OrderPeriods.cs:31-68`; `DeliveryPeriodTests.ShippingPromise_CountsFromTheCardsSuccessNotice_KK1`, `..._TransfersPaidMark_AndAfterItsCorrection_...KK1`, `ShippingAndFulfilmentPeriods_RunOut_WithoutAnAction_AndTheOrderStaysInItsCounter_KK1`; `PeriodTimerScanTests` |
| 2 | Aşımdan sonraki iptalde kanuni faiz uyarısı | ✓ | `DeliveryPeriodRules.IsLegalInterestWarningDue` (`:89`), `OrderPeriods.IsLegalInterestWarningDueAsync` (`:85`); `DeliveryPeriodRulesTests.InterestWarning_*_KK2`, `DeliveryPeriodTests.CancellationAfterThePromise_CarriesTheLegalInterestWarning_AndBeforeIt_Not_KK2` |
| 3 | Z-11 dolunca gecikme feshi, kargodaki siparişte de | ✓ | `DeliveryPeriodRules.IsLateDeliveryTerminationOpen` (`:74`); `DeliveryPeriodRulesTests.LateDeliveryTermination_OpensOnAShippedOrderToo_KK3`, `DeliveryPeriodTests.LateDeliveryTermination_OpensAfterThirtyDays_OnAShippedOrder_AndClosesWithTheItemsRecords_KK3` |
| 4 | Z-12 süre değil; teslim tarihi yoksa Z-13 ve Z-18 başlamaz | ✓ | `AfterSalesPeriodRules.IsWithdrawalWindowOpen` (`:54`), `IsDefectClaimChannelOpen` (`:170`), Z-11'de tarih girilmiş kalem feshi açmaz; `PhysicalWindow_WithoutADeliveryDate_DoesNotStart_AndTheButtonDoesNotClose_KK4`, `DefectClaimChannel_OfAShippedPhysicalItemWithoutADeliveryDate_IsOpen_AndDoesNotClose_KK4`, `LateDeliveryTermination_StaysClosed_WithADeliveryDate_..._KK4` |
| 5 | Z-13/Z-14 kapanınca düğme kapanır, beyan geçerli; Z-14'ü "tamamlandı" da kapatır; Z-15 onayda düşer | ✓ | `IsWithdrawalWindowOpen` kayıtlara dokunmaz; `PhysicalWindow_RunsFourteenDaysFromTheDeliveryDate_AndThenCloses_KK5`, `ServiceWindow_TheCompletedMark_ClosesTheButtonBeforeTheWindowsEnd_KK5`, `ServiceWindow_OnAnOrderWithAPhysicalItem_...`, `DigitalItem_HasNoWithdrawalWindow` |
| 6 | Z-16, Z-17, Z-47 için iş kaydı ve kendiliğinden işlem yok; Z-16 ve havalede Z-17 kalan süresi gösterilir; Z-47 gösterilmez | ✓ | `RefundStanding` (`:113`), `CancellationRefundStanding` (`:124`, kartta null); Z-47 kuralı yok; `RefundPeriod_ShowsTheDaysLeft_AndTurnsOverWithoutAnAction_KK6`, `CancellationRefund_ShowsItsPeriod_OnlyOnTheBankTransferLine_KK6`, `PeriodTimerScanTests.NoJobType_...`, `ImpossibilityNoticePeriod_IsNamedOnlyWhereThePeriodsAreListedAndCounted_Z47`, `DeliveryPeriodTests.RefundAndSendingPeriods_RunOut_WithoutAJobOrAnAction_AndTheRecordsStay_KK6_KK7` |
| 7 | Z-42 dolunca kalem bekler, "mal dönmedi" kapanışı açılır | ✓ | `IsNotReturnedClosureOpen` (`:147`), `ReturnShipmentStanding` (`:130`); `NotReturnedClosure_OpensWhenTheSendingPeriodRunsOut_ForAnAwaitedWithdrawalAfterDelivery_KK7`, Integration `..._KK6_KK7` (kayıt yazılmaz) |
| 8 | Z-18 dolunca kanal kapanır; yeni talep yok, çözülmüş talep yeniden açılmaz | ✓ | `DefectClaimChannelEnd` (`:158`), `IsDefectClaimChannelOpen` (`:170`); `DefectClaimChannel_ClosesTwoYearsAfterTheDeliveryDate_KK8`, `..._FromALeapDay_...`, `..._OfAServiceItem_...`; talep işlemi T374 bağlar (D-513) |
| 9 | Z-19 süre değil adet; zamanlayıcı yok | ✓ | `PeriodTimerScanTests` deseni `DownloadCount`/`DownloadAllowance`'ı da arar; hiçbir zamanlayıcı türü okumaz |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçilerin yaptıkları dahil bütün maddeleri yeniden denetledi (diff `origin/main...HEAD`).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| K1 / KK1: Z-10 kartta başarı bildiriminden, havalede "ödendi" işaretinden; aşımda işlem yok, sayaç var | ✓ | `OrderPeriods.cs:31` (düzeltilmemiş son Ö1); Integration KK1 üç test (kart, havale + düzeltme + yeni işaret, 0 günlük söz aşımı: kaynak bulmaz, `jobs` boş, sipariş Hazırlanıyor/Ödendi, sürüm aynı) |
| K2 / KK2: aşımdan sonraki iptalde faiz uyarısı | ✓ | `DeliveryPeriodRules.cs:89`; sınır anı (`End.AddTicks(-1)` false, `End` true) Unit + Integration |
| K3 / KK3: Z-11 sonrası fesih kargodaki siparişte de açık | ✓ | `DeliveryPeriodRules.cs:74`; Integration'da sipariş Shipped, kayıtla kapanan adet yolu kapatır |
| K4 / KK4: teslim tarihi yoksa Z-13 ve Z-18 işlemez | ✓ | `AfterSalesPeriodRules.cs:54`, `:170`; Unit KK4 üç test |
| K5 / KK5: pencere kapanınca beyan korunur; hizmette "tamamlandı" düğmeyi kapatır | ✓ | Kural salt işlev, kayıt okumaz/yazmaz; Unit KK5 testleri; Integration `..._KK6_KK7` beyan kaydı süre sonrası duruyor |
| K6 / KK6: Z-16, Z-17, Z-47 için iş kaydı yok; Z-47'de gösterge yok, Z-17 yalnız havalede | ✓ | `PeriodTimerScanTests` (iş türü adı, zamanlayıcı türleri, Z-47 dönemi iki dosyada); `CancellationRefundStanding` kartta null |
| K7 / KK7: Z-42 sonrası "mal dönmedi" kapanışı açılır | ✓ | `AfterSalesPeriodRules.cs:147`; Unit + Integration |
| K8 / KK8: Z-18 sonrası yeni talep reddedilir | ✓ | `IsDefectClaimChannelOpen` sınırı; talep işlemi yok (T374), kural tek evde — D-513 |
| K9 / KK9: Z-19 için zamanlayıcı yok | ✓ | `PeriodTimerScanTests.NoScheduledSource_...KK9` (desen indirme sayacını kapsar; desen örnek testi) |
| `09 §25` — doküman uyumu: adlar `06`/`02`'den (`ShippingPeriod`, `FulfilmentPeriod`, `DeliveryDate`, `DeclarationDate`, `ArrivalDate`; `CountedPeriod` mevcut değerleri) | ✓ | yeni ad yalnız kural/okuyucu sınıf adları |
| `09 §25` — yeni süre sabiti §3.5'te mi | ✓ (uygulanamaz) | Yasal süreler (14, 30, 2 yıl) `02 §11` "Envantere girmeyenler"; `09 §3.5` m.1 "kendi evlerindedir" — kuralın sınıfında `const`; testler beklentiyi sabitten türetir |
| `09 §25` — modül sınırı (yalnız `Contracts`), mimari testler | ✓ | `OrderPeriods` → `IAfterSalesModule`; `AfterSalesPeriodRules` → Ordering/Payment `Contracts`; Unit Architecture yeşil |
| `09 §25` — tek işlev | ✓ | Sayım `PeriodCounting`/`BusinessCalendar`/`TurkeyTime`'dan; kapanan adet `ClosedQuantitiesAsync`'ten |
| `09 §25` — zaman yalnız `TimeProvider`'dan | ✓ | Kurallar saat okumaz, an çağırandan gelir; testler `ManualTimeProvider` |
| `09 §25` — durum değişikliği/iş/olay | ✓ (uygulanamaz) | Geçiş, `Job`, işlem izi yazılmaz; `AssertNothingActsAsync` |
| `09 §25` — veri ve göç | ✓ (uygulanamaz) | Göç, tablo, değer eklenmedi |
| Güvenlik taraması | ✓ | Sır/kişisel veri yok; yeni uç, POST, form, yükleme, dış çağrı, paket yok; yetkilendirme yüzeyi değişmedi |
| Sık FAIL listesi | ✓ | Geniş istisna yok (Z-47 izinli dosya listesi tam eşitlik); test sayısı sabitten; hedefi T325 olan D-161 kapatıldı; karar kaydısız 09 metni yok (belge değişmedi, 0 gün anlamı D-512) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release` (Consistency dahil) | 4662 / 4662 (yeni 35) |
| Betik | ✓ | `sh tests/scripts/run-script-tests.sh` | 4 / 4 dosya |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-class "*DeliveryPeriodTests"`; `--filter-namespace "*Ordering*"`; `--filter-namespace "*AfterSales*" "*Payment*" "*Jobs*"` (yerel PostgreSQL 18) | 7 / 7; 192 / 192; 77 / 77 |
| Contract | — | | Uç değişmedi |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T325-zaman-asimi-teslim`
- **Commit:** `577f6159` — kurallar ve süre göstergesi; `e34bd542` — Integration testi; `e1118849` — `origin/main` birleştirmesi; kayıt commit'i PR'da
- **PR:** #464
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | 0 iş günlük kargoya verme sözünün anlamının `02`'ye karar kaydıyla yazılması | D-512 | Belge değişikliği doküman turunun işi |
| 2 | Kuralların işlemlere ve panele bağlanması (iptal, fesih, cayma, geri ödeme, mal dönmedi, ayıp talebi, sayaçlar) | D-513 | İşlemlerin sahibi açık task'lar: T367, T368, T370–T374, T382 |

## Notlar

- **Working tree (Adım -1):** temiz (üç işçide de).
- **Ana dal CI startup (Adım 0):** ilk işçide main son 3 tamamlanmış run success (42464077677, 42463181128, 42463181214); `core.hooksPath` = `scripts/git-hooks`.
- **Repo memory (bitiş kapısı 9):** dokunulmadı (parça dosyaları kuralı).
- **⚠ Kolay kararlar (kurala en uygun seçenekle verildi):**
  - İşlemler ve sayaçlar açık task'larda: T325 kuralların tek evini ve "zamanlayıcı yok" taramasını kurar; düğmeleri/uyarıları/sayaçları T367, T368, T370–T374, T382 bağlar (D-513).
  - Z-10 0 iş günü = onay günü kargoya verme (son gün onayın kendi günü, ertesi gün 00:00'da dolar); belge yazmıyor → D-512.
  - Z-18: fiziksel kalemde kanal sipariş Kargoya verildi ya da Teslim edildi'deyken açılır; Teslim edilemedi'de (mal firmaya döndü, teslim tarihi yok) kapalı sayıldı.
  - Z-11 ödemesi onaylı (Ödendi / Kısmen geri ödendi) ve iptal edilmemiş siparişte açılır.
  - Panel göstergesinde aşılan gün takvim günüdür (kalan gün iş günlü dönemde iş günü).
  - Önceki task'ların STATUS "Commit" hücreleri (T317, T318, T324, T333–T339, T341, T344, T358, T362) ve raporlarının "Post-merge run'lar" satırları bu dalda doldurulmadı: paralel task düzeninde ortak satırlar kayıt PR'ının işi; bu PR'da çakışma riski.
- **Diğer:** `12` `Durum` hücrelerine dokunulmadı (VAL-A074, A075, C024, A532, B134–B140 tarayıcı/uç satırları işlemlerin task'larında).
