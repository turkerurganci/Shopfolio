# T327 — Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `03 §4.2`'nin 12 anı `AutomaticMoments` kaydında kendi sistem eylemleriyle ("Namespace.Type.Member" adı; modül sınırı geçilmesin diye referans değil ad) eşlendi; Unit testi eylemleri yansımayla çözer.
- Z-38 (malı dönmeyen caymada havale hattı IBAN'ının silinmesi) zamanlayıcı kaynağı `RefundIbanDeletionDay` (`AddDueRecordSource`, `WaitsForFreshHeartbeat = false`): sipariş satır kilidi → `IAfterSalesModule.FindRefundIbanUseAsync` yeniden okuma → `IPaymentModule.FindSettledRefundSubjectsAsync` → `RefundIbanDeletionRule.ShouldDelete` → `ApplyRefundIbanDeletionDayAsync` (IBAN silinir + imha kaydı `DisposalRecordType.RefundIban`, ya da başka bekleyen geri ödeme varsa `ScheduledDeletionAt = null`) → sipariş sürümü bir kez artar.
- Satış sonrası modülüne `RefundIbanUse` sözleşmesi ve üç yöntem (`FindDueRefundIbanDeletionsAsync`, `FindRefundIbanUseAsync`, `ApplyRefundIbanDeletionDayAsync`); ödeme modülüne `FindSettledRefundSubjectsAsync`.
- `OrderPageSummary.TransferPaymentEndsAt`: bekleyen havale siparişinin son anı (ertelemeden sonra yeni son gün) sipariş sayfası verisinde.
- Kesinti davranışı (dönüşte kaçırılan işlerin sırası, ertelenen Z-8 iptali, düşen hatırlatma) T221/T324'ün yapı taşlarıyla Integration'da uçtan uca sınandı; bakım modu anahtarı ve bakım penceresi ayarı olmadığı Unit taramasıyla kilitlendi.
- `origin/main` (T323 #474, T345 #477, T349 #476) birleştirildi; `IAfterSalesModule.cs`'teki çakışma iki tarafın yöntemleri yan yana korunarak çözüldü (yalnız ekleme).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Jobs/AutomaticMoments.cs` (yeni)
- `src/Shopfolio.Web/Modules/Ordering/RefundIbanDeletionDay.cs` (yeni), `OrderingModule.cs` (kayıt), `OrderAccess.cs`, `Contracts/OrderAccessContracts.cs`
- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.RefundIbans.cs` (yeni), `Contracts/IAfterSalesModule.cs`, `Contracts/RefundIbanUse.cs` (yeni)
- `src/Shopfolio.Web/Modules/Payment/PaymentModule.cs`, `Contracts/IPaymentModule.cs`
- `tests/Shopfolio.UnitTests/Jobs/AutomaticMomentsTests.cs`, `tests/Shopfolio.IntegrationTests/Ordering/AutomaticMomentsTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 12 an kendi sistem eylemiyle kayıtta | ✓ | `AutomaticMoments.cs`; `Register_HasTheTwelveMomentsOf03_4_2_InTheirOrder_KK1`, `EveryMoment_HasAnAction_AndEveryActionIsAMemberOfTheProduct_KK1` |
| 2 | Süreler kesintide işler; bakım modu / bakım penceresi yok | ✓ | `Product_HasNoMaintenanceModeAndNoMaintenanceWindow_KK2`, `MaintenanceScan_CatchesEveryShapeOfTheSwitch_KK2`, `OnlyTheTransferWindowDeferral_IsAnOutageEffect_KK2_KK8` |
| 3 | Dönüşte kaçırılan işler sırayla; Z-8 iptali ve B-3 istisna | ✓ | Integration `Return_RecordsTheOutage_RunsTheMissedJobsInTheOrderTheyWereMissed_AndHoldsBackTheDeferredCancellationAndTheEndedReminder_KK3_KK7` |
| 4 | Kesintide dolan Z-8 iptali ertelenir, ayrılan tutulur, yeni son gün, e-posta yok | ✓ | `DeferredCancellation_KeepsTheReservation_ShowsTheNewLastDay_SendsNoEmail_AndRunsAtTheDeferredMoment_KK4_KK5`; `OrderPageSummary.TransferPaymentEndsAt` |
| 5 | Ertelenen iptal ertelenen anda işler | ✓ | aynı test (ikinci yarı) |
| 6 | Z-38: IBAN silinir, kalem açık; başka geri ödeme tutuyorsa kalır | ✓ | `RefundIbanDeletionDay.cs`; Unit `DeletionDay_DeletesTheIban_WhenItsOnlyWithdrawalsGoodsDidNotArrive_KK6`, `DeletionDay_KeepsTheIban_WhileAnotherRefundStillUsesIt_AndDeletesItOnceThatRefundIsSettled_KK6`; Integration `DeletionDay_DeletesTheIbanOfAWithdrawalWhoseGoodsDidNotArrive_KeepsTheWithdrawal_AndWritesTheDisposalRecord_KK6`, `DeletionDay_KeepsTheIbanWhileAnotherRefundUsesIt_AndEmptiesItsDay_ButDeletesItWhenThatRefundWasSent_KK6` |
| 7 | Kesinti tespiti `05 MK-44` yoluyla | ✓ | KK3 testi `SchedulerHeartbeat`/`HeartbeatPulse` yolundan (RecordingSource sarmalayıcı) geçer |
| 8 | Yasal süreler dondurulmaz, ayarla uzamaz | ✓ | `LegalPeriods_AreConstants_AndNoSettingCarriesThem_KK8`, `OnlyTheTransferWindowDeferral_IsAnOutageEffect_KK2_KK8` |

### Öz denetim (bitiş kapısı 2)

> Devam turu (2026-11-09) önceki işçinin yaptıkları dahil bütün maddeleri birleşik ağaçta yeniden denetledi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: 12 anın her biri için sistem eylemi | ✓ | `AutomaticMoments.cs`; Unit KK1 iki test |
| KK2: kesinti süreyi uzatmaz; bakım anahtarı/penceresi yok | ✓ | Unit KK2 üç test (tarama örnek kümesiyle) |
| KK3: dönüş sırası; ertelenen Z-8 ve dolmuş B-3 hemen çalışmaz | ✓ | Integration KK3/KK7 testi |
| KK4: erteleme, ayrılan tutulur, yeni son gün, e-posta yok | ✓ | Integration KK4/KK5 testi; `OrderAccess.cs` |
| KK5: erteleme anında iptal | ✓ | Integration KK4/KK5 testi |
| KK6: Z-38 silme / başka geri ödemeye dek tutma | ✓ | Unit 2 + Integration 2 test |
| KK7: `05 MK-44` yolu | ✓ | Integration KK3/KK7 testi; yeni tespit yolu yazılmadı |
| KK8: yasal süre dondurulamaz | ✓ | Unit KK8 testleri |
| `09 §25` — modül sınırı (yalnız `Contracts`, arayüz yönteminde `SaveChanges` yok) | ✓ | `ApplyRefundIbanDeletionDayAsync` çağıranın işlemine katılır, kaydetmez; mimari testler Unit tam koşuda yeşil |
| `09 §25` — zaman yalnız `TimeProvider` | ✓ | kaynak `now`'ı zamanlayıcıdan alır |
| `09 §25` — eşzamanlılık (kilit, yeniden okuma, sürüm) | ✓ | `RefundIbanDeletionDay.cs` sipariş kilidi altında yeniden okur, `RaiseVersionOnce` |
| `09 §25` — enum/değer ekleme (kilit dosyası, `06 §5`) | ✓ | yeni enum yok (`RefundIbanSubjectKind` kaldırıldı, f19466cf) |
| `09 §25` — göç / yeni tablo / yeni ayar / yeni paket | ✓ | yok |
| `09 §25` — işlem izi / imha kaydı | ✓ | IBAN silmesi imha kaydını aynı işlemde yazar (Z-44) |
| Modül kurucusuna yeni bağımlılık (sık FAIL) | ✓ | yok; Integration bütün ad alanları koşuldu |
| Güvenlik taraması | ✓ | yeni uç, kullanıcı girdisi, sır, ham SQL yok; IBAN silme yalnız kilit altında kural sonucuyla |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --max-threads 3` | birleştirmeden sonra 5065/5065 (öncesinde 5013/5013) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-namespace …` | f19466cf: Disposal+Jobs+Outages+Payment+Concurrency+Ordering 520/520; Web+Pages+Catalog+Commands+CorporateContent+FileSecrets+Logging+Security+SignedEndpoints 864/864; Database+Files 138/138; Model 1378/1379 — tek başarısız `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled_…` paralel koşuda `DROP DATABASE … WITH (FORCE)` adımında; sınıf tek başına 60/60 (T327 göç/model değiştirmez). Birleştirmeden sonra Ordering+Disposal+Outages 415/415 |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` | src ve iki test projesi temiz |
| Contract | — | | yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-09; head `9003ac2f`, `origin/main` `a504ff84` ile güncel — birleştirme gerekmedi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (D-696, D-697'ye forward) |

**Validator kanıtları (bağımsız koşum):**

| # | Kriter / KK | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | 12 an kayıtta / KK1 | ✓ | `AutomaticMoments.All` 03 §4.2'nin 12 satırını sırayla taşır; `Register_HasTheTwelveMomentsOf03_4_2_InTheirOrder_KK1` ve yansıma testi geçti. 4.2.5/4.2.6 eylemleri veri üyesi adlarıdır (K2) |
| 2 | Kesintide süreler; bakım modu/penceresi yok / KK2 | ✓ | tek `IOutageEffect` `TransferWindowDeferral`; `src` taraması ve `CompanySettings`'te `Maintenance` yok; KK2 üç Unit testi geçti; `10 KD-60`, `02 §3.34.7` ile eşit |
| 3 | Dönüşte kaçırılma sırası; Z-8 ve B-3 istisnası / KK3 | ✓ | Integration `Return_RecordsTheOutage_…_KK3_KK7`: sıra `CardPaymentExpiry` (−170 dk) → `refund-iban-deletion-day` (−120) → `TransferPaymentReminder` (−60); ertelenen siparişte iptal ve gönderim yok; `SchedulerScan` sıralaması (`DueAt`, kaynak, id) okundu |
| 4 | Z-8 ertelemesi / KK4 | ✓ | `DeferredCancellation_…_KK4_KK5`: ayırma 1, gönderim 0, `OrderPageSummary.TransferPaymentEndsAt` = `DeferredWindowEnd`; Razor görünümü T353'te (D-668) |
| 5 | Ertelenen anda iptal / KK5 | ✓ | aynı test: bir dakika önce değişiklik yok, ertelenen anda İptal edildi + Başarısız, B-7, ayırma 0 |
| 6 | Z-38 / KK6 | ✓ | `RefundIbanDeletionDay` (kilit → yeniden okuma → kural → uygulama → sürüm) okundu; Integration KK6 iki testi: IBAN silinir, cayma ve kalem kalır, imha kaydı `RefundIban`/1, sürüm +1; bekleyen iptal geri ödemesi IBAN'ı tutar ve gün boşalır. Konu kümesinin eksikleri K1 (D-666, D-696; T370) |
| 7 | `05 MK-44` yolu / KK7 | ✓ | yeni tespit yolu yok; testte `OutageRecovery.ApplyAsync` kesinti kaydını son nabızdan dönüşe yazar |
| 8 | Yasal süreler / KK8 | ✓ | `AfterSalesPeriodRules`/`DeliveryPeriodRules` yasal alanları `const`; ayarlarda karşılığı yok; Unit KK8 testi geçti |

**`12` satırları (adım 2a):** task'ın KP birimi `10 KP-75` (izlenebilirlikte KP-74 de). `VAL-D052`…`D055` (KP-75, KRİTİK) T327'nin hiçbir kriterine eşlenmez — BEKLEMEDE kaldı (K3). KP-74'ten `VAL-D049`: Z-38 silmesinin imha kaydı burada kanıtlı, IBAN'ın öteki silme yolları T370 k5'tedir — BEKLEMEDE kaldı (T324 doğrulamasıyla aynı gerekçe). `12 §12.6`'nın `VAL-C109`, `C110`, `A826`, `A827` satırları KP kimliği taşımaz; `Durum` değişmedi, sahipliği D-697. `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** K1 — Z-38 konu kümesi hizmet caymasını, kargodan önceki caymayı ve IBAN isteğiyle bağlanan geri ödemeyi saymaz; konu başına "herhangi bir geri ödeme kapandıysa kapalı" varsayımı → D-696 (T370). K2 — kayıtta 4.2.5/4.2.6 eylemleri veri üyesi adları → D-697 (T369, T373). K3 — KP-75 ve `12 §12.6` kesinti satırlarının sahibi → D-697 (F3 gate-check).

Test: Unit 5065/5065, Integration 284/284 (`AutomaticMomentsTests`, `PaymentWindowTests`, `OrderAccessTests`, `SchedulerScanTests`, `SchedulerRegistrationTests`, `SchedulerHeartbeatTests` 64; `CustomerNoticeAfterOrderQueueTests`, `ItemRecordTablesTests`, `ItemRecordsTests`, `NotificationMapQueueTests`, `OrderDisposalTests`, `ReturnRejectionAndIbanRequestTablesTests`, `StatusCorrectionTests` 220; PostgreSQL 18), build `-warnaserror` 0 uyarı, `dotnet format --verify-no-changes` üç projede 0, betik testleri 4/4. Dal CI'ı run 42525030881 yeşil. Yapım raporuyla uyuşmazlık: rapor task'ın `12` satırlarını anmıyor; öz denetim tablosu her kriter ve KK için kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T327-otomatik-anlar`
- **Commit:** son commit PR'da
- **PR:** #480
- **CI:** dal CI'ı PR'da
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (dış servis çağrısı ve yeni paket yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Kargodan önceki caymanın (ReturnId'siz Withdrawal geri ödemesi) IBAN kullanımı Z-38'de "bekleyen geri ödeme" sayılmıyor | D-666 | akış T370'te |
| 2 | `ScheduledDeletionAt`'i yazan akış yok; Z-38 kaynağı hazır | D-667 | cayma beyanı T372/T370'te |
| 3 | Sipariş sayfası Razor'da son günü göstermiyor; veri `OrderPageSummary.TransferPaymentEndsAt`'te | D-668 | sayfa T353'te |

## Notlar

- **⚠ kararlar:**
  1. "Bekleyen geri ödeme" tanımı: IBAN'a bağlı iptal/gecikme feshi ve caymanın teslim alma kayıtlarından `SentAt`/`VoidedAt`'i dolu geri ödemesi olmayan. Malı hiç ulaşmamış cayma konu sayılmaz.
  2. `FindSettledRefundSubjectsAsync` konu başına tek geri ödeme varsayar: bir konunun birden çok geri ödemesi varsa birinin `SentAt`/`VoidedAt`'i yeter.
  3. 4.2.4–4.2.6 anlarının eylemleri mevcut yapı taşlarına bağlandı; tam akışlar T367–T373'te.
  4. IBAN silmede sipariş sürümü artırılır (`06 2.6.4` bekçisi).
  5. Model ad alanındaki tek başarısızlık paralel koşu çakışması (yukarıda); T327 kapsamı dışı, kod değişmedi.
  6. `11`'in izlenebilirlik satırları (03 §4.2, §10.3; 02 §3.34.7, §3.34.8, Z-8, Z-38, §4.3; 05 MK-44, §5.4) doldurulmadı: F3 task'larının hiçbiri bu satırları yapımda doldurmuyor (T311, T324, T349 satırları boş); faz kapısındaki toplu güncellemeye bırakıldı.
  7. `dotnet format` birleştirmeden önceki ağaçta koşuldu; birleştirmede yalnız `IAfterSalesModule.cs`'e iki tarafın yöntemleri yan yana eklendi.
- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** 42512134557, 42512134573, 42510559202 success
- **Repo memory (bitiş kapısı 9):** parça dosyası düzeni gereği MEMORY'ye dokunulmadı
- **Diğer:** backlog D-99'a T327 payı notu eklendi; D-100 T324'te kapanmıştı.
