# T325 — Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; merge yardımcıda) | **Tarih:** 2026-11-08

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
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-08, head `908b740a` + `origin/main` `02ec5357` (T342 #453) birleştirmesi). Kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓ |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-531 (faiz uyarısı kuralı kargoya verme anını okumaz; T368), K2 → D-532 (Teslim edilemedi'de ayıp kanalı okuması belgede açık değil; T374) |

**Doğrulayıcı kanıtları (tur 1):**

| # | Kriter / KK | Sonuç | Doğrulayıcı kanıtı |
|---|---|---|---|
| 1 / KK1 | Z-10 ve Z-39 ödeme onayından; aşımda işlem yok, sayaç var | ✓ | `OrderPeriods.PaymentApprovedAtAsync`: düzeltme kaydının göstermediği son Ö1'in anı (kartta başarı bildirimi, havalede işaret; düzeltilen işaret sonrası yeni işaret) — `02 §4.2` Z-10/Z-39 "ödemenin onaylandığı an", `03 §4.1.10` "düzeltmeden sonra yeni ödeme onayı". Z-10 iş günü (`PeriodCounting` `ShippingPeriod`), Z-39 takvim günü (`02 §4.1.3`). Integration KK1 üç test: kaynak (`IDueRecordSource`) siparişi bulmaz, `jobs` boş, sipariş Hazırlanıyor/Ödendi ve sürüm aynı (sayaç bu durumdan okunur). Panel sayacının kendisi T382 (D-513) |
| 2 / KK2 | Aşımdan sonra iptalde faiz uyarısı | ✓ | `IsLegalInterestWarningDue`: fiziksel Z-10, hizmet Z-39, dijital ve onaysız ödeme false; sınır anı Unit + Integration (`End.AddTicks(-1)` false, `End` true). Uyarının iptal işlemine bağlanması T367/T368 (D-513). Kenar durum K1 |
| 3 / KK3 | Z-11 sonrası fesih kargodaki siparişte de açık | ✓ | `IsLateDeliveryTerminationOpen`: sipariş onayından (`Order.CreatedAt`) 30 takvim günü (`02 §4.2` Z-11), ödeme onaylı, iptal değil, teslim tarihi yok + açık adedi kalan fiziksel kalem; kişiye özel üretim dışlanmaz (K-597). Integration: Shipped siparişte sınırda açılır, kayıtla kapanan adet yolu kapatır, sınır aşımı işlem üretmez. Ayıpta dönülen adedin ayrıca düşülmesi (`02 §7.1.6`, K-1077) T368 KK2'de ve D-513'te |
| 4 / KK4 | Teslim tarihi yoksa Z-13 ve Z-18 işlemez | ✓ | `IsWithdrawalWindowOpen` fiziksel kalemde tarih yoksa düğmeyi kapatmaz; `IsDefectClaimChannelOpen` kargodaki tarihsiz fiziksel kalemde açık ve kapanmaz (`02 §7.5.2`, `VAL-C024`'ün kuralı); tarih girilmiş kalem Z-11'i açmaz. Unit KK4 üç test |
| 5 / KK5 | Pencere kapanınca beyan korunur; hizmette "tamamlandı" kapatır; Z-15 | ✓ | Kural saf işlev, kayıt okumaz/yazmaz; Z-13 teslim tarihinden, Z-14 sipariş tarihinden ya da fiziksel kalemli siparişte teslim tarihinden (girilmemişse başlamaz; `02 §4.2` Z-14), `DeliveryMarkedAt` doluysa kapalı; dijital hiç açık değil. Integration `..._KK6_KK7` beyan kaydı süre sonrası durur |
| 6 / KK6 | Z-16, Z-17, Z-47 için iş yok; Z-47 gösterilmez; Z-17 yalnız havalede | ✓ | `PeriodTimerScanTests` (iş türü adları; yansımayla bulunan her kaynak/kesinti etkisi/gecelik adım/işleyici bu kuralları ve tarih alanlarını okumaz; `ImpossibilityNoticePeriod` yalnız `CountedPeriod.cs` ve `PeriodCounting.cs`'te); `CancellationRefundStanding` kartta null; `WithdrawalRefundStartDay` `02 §4.2` Z-16 başlangıcıyla birebir (ikisinden geç olanı; mal ulaşmadan null) |
| 7 / KK7 | Z-42 sonrası "mal dönmedi" kapanışı açılır | ✓ | `IsNotReturnedClosureOpen` = teslimden sonraki cayma ∧ bekleyen adet ∧ beyandan 14 gün dolmuş (`02 §4.2` Z-42 "teslimden sonraki cayma"); Integration'da süre geçince `not_returned_closures` ve `returns` boş, kalem bekler |
| 8 / KK8 | Z-18 sonrası yeni talep reddedilir | ✓ | `DefectClaimChannelEnd` = teslim günü + 2 yıl, ertesi gün 00:00 (`02 §4.1.4`); fiziksel Kargoya verildi/Teslim edildi'den, hizmet "tamamlandı"dan, dijital onaydan (onayda `DeliveryDate` yazıldığı `OrderTransitions.Payment.cs` `CompleteApprovalAsync`'te teyit edildi). Talep işlemi T374 (D-513). Kenar durum K2 |
| 9 / KK9 | Z-19 için zamanlayıcı yok | ✓ | `PeriodTimerScanTests` deseni `DownloadCount`/`DownloadAllowance`'ı da arar; desen örnek testi yakaladığını ve yakalamadığını sınar |
| `09 §25` | ilgili satırlar | ✓ | zaman yalnız çağırandan (kurallar saat okumaz, okuyucu anı parametre alır); modül sınırı (`IAfterSalesModule`, `Contracts`); sayım tek evden (`PeriodCounting`, `TurkeyTime`, `BusinessCalendar`), ödeme onayı anını okuyan ikinci işlev yok; yasal süre sabitleri `02 §11` envanteri dışında, kuralın sınıfında; geçiş/iş/iz/göç/paket/uç yok |
| Güvenlik | taraması | ✓ | sır yok (diff'te yalnız test yer tutucuları ve örnek IBAN); yeni uç, POST, form, dış çağrı yok; yetki yüzeyi değişmedi; yeni bağımlılık yok (csproj ve kilit dosyası değişmedi) |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana çalışma ağacının `scripts/git-hooks`'u); ana dal son 3 tamamlanmış run 42475025470 (Publish, success — `02ec5357` T342), 42474266979 (CI, cancelled — `240844aa` #463; eşzamanlılık grubunda `cancel-in-progress` ile T342 itmesi iptal etti, iptal anında Integration dışındaki işler yeşildi), 42474266982 (Publish, success — `240844aa`). Başarısız run yok; `02ec5357`'in CI'ı doğrulama sırasında koşuyordu. Memory kapısı: kaldırıldı (Adım 0b).

**Testler (doğrulayıcı, yerel):** `908b740a` üzerinde `dotnet restore`, `dotnet build -warnaserror` 0 uyarı/0 hata; `dotnet format --verify-no-changes` üç projede çıkış 0; Unit (Architecture + Consistency dahil) 4662/4662; betik testleri 4/4; Integration (PostgreSQL 18, ayrı konteyner) `DeliveryPeriodTests` 7/7, `PaymentTransitionsTests` + `ShipmentTransitionsTests` 119/119. `origin/main` birleştirmesinden sonra (`02ec5357`) build 0 uyarı, Unit 4699/4699. Koşu sonrası `git status` temiz, `dotnet restore --locked-mode` çıkış 0. Dal CI 42474305471 (`908b740a`) bütün işler yeşil; önceki 42470498287'deki Paketleme kırmızısı #463 öncesi restic indirmesiydi.

**`12` satırları:** task tanımı KP kimliği taşımaz (`11 §7.1`'de T325 yalnız `02`/`03` öğeleriyle eşlenir); içerikçe ilgili satırlar `VAL-C009`, `C012`, `C024`, `A067`, `A069`, `A074`, `A075`, `A228`, `A229`, `A532`, `B134`…`B140` (KP-21, KP-22, KP-23, KP-47; uç yanıtı ya da tarayıcı gözlemi isterler) işlemlerin task'larındadır (T367, T368, T370, T371, T374, T382). T325 uç yazmadığı için hiçbiri bu task'ta kanıtlanamaz; `Durum` hücreleri `BEKLEMEDE` kaldı. `12_matris_denetle.py --kapsam 1-77` hata 0.

**Yapım raporu karşılaştırması (adım 13):** hüküm uyumlu; öz denetim tablosu her kriter, KK, `09 §25` ve güvenlik için kanıtlı. Uyuşmazlık: Notlar'daki Z-18 "Teslim edilemedi'de kapalı" kolay kararı backlog'a yazılmamıştı → K2 / D-532. K1 (faiz uyarısında kargoya verme anı) yapım raporunda yok → D-531.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T325-zaman-asimi-teslim`
- **Commit:** `577f6159` — kurallar ve süre göstergesi; `e34bd542` — Integration testi; `e1118849` — `origin/main` birleştirmesi; kayıt commit'i PR'da
- **PR:** #464
- **CI:** dal CI 42474305471 (`908b740a`) bütün işler yeşil; doğrulama kayıt commit'inin (`origin/main` birleştirmesiyle) CI'ını yardımcı izler
- **Post-merge run'lar:** squash `0a66996` (#464) — CI 42480365166 success, Publish 42480365007 success (F3 gate'te dolduruldu)

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
