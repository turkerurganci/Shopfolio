# T221 — Zamanlayıcı

**Faz:** F2 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Jobs/`: `SchedulerService` (uygulamada tek `BackgroundService`; iki ayrı döngü: dakikalık tarama ve otuz saniyelik nabız; tek zamanlayıcı oturum kilidi), `SchedulerScan` (kaynaklar arası zamanı gelen kayıtları zaman, kaynak sırası, kimlik sırasıyla işler; tur başına kaynak başına en çok 500), `NightlyJobRunner` (son başarı son 03:00'ten eskiyse başlar; başarısızsa bir saat sonra yinelenir; gecelik iş kilidi), `HeartbeatPulse` (canlılık isteği 200 ise nabız yazılır), `OutageRecovery` (iki dakikayı aşan boşluk kesinti kaydıdır; kesinti kaydı, ilk nabız ve son anların taşınması tek işlemde), `NightlyJobSchedule`, `TransferSchedule` (08:00 hatırlatma, beş dakikalık yanıtsız sorgu), `SchedulerTimings`, `SchedulerContracts`, `SchedulerRegistration`, `JobQueueCleanupStep`.
- `Infrastructure/ExternalCallTimeouts.cs` (iki sabit; `09 §3.5` yeni satır), `BusinessCalendar.NextBusinessDay`.
- `Modules/Ordering/`: `ScheduledOrderTransition` (zamanı gelen siparişi kendi geçişiyle, sipariş kilidi altında işleyen taban), `TransferWindowDeferral` (kesinti etkisi: pencere süresi dolan havale siparişinin iptalini erteler, kaçırılmış hatırlatmasını düşürür).
- `Program.cs` `AddScheduler`; test fabrikasında `withScheduler=false` varsayılan.
- Kaynak taramaları: `JobSourceScan` (zamanlayıcı dosyaları için kendi kuralı, geniş `catch` yeri tam yol), `ConcurrencySourceScan` deseni `pg_(?:try_)?advisory` + ihlal örneği (D-98 kapandı).
- `deploy/compose.dev.yaml`: dev bindirmesinde veritabanı komutu imajın CMD'si + `archive_mode=off` (D-39 (f)).
- Belgeler: `09 §3.5` yeni satır, `§13.1` m.10, `§25` satırı; `CONTEXT.md`; `DEFERRED_BACKLOG` (D-98 ✅, D-97 gecelik temizlik kısmı ✅, D-39 (f) ✅, yeni D-99).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Infrastructure/Jobs/*`, `Infrastructure/{BusinessCalendar,ExternalCallTimeouts}.cs`, `Modules/Ordering/{ScheduledOrderTransition,TransferWindowDeferral,OrderingModule}.cs`, `Program.cs`, `deploy/compose.dev.yaml`; testler: `UnitTests/{Jobs,Architecture,Consistency,Infrastructure}`, `IntegrationTests/{Jobs,Web}`; belgeler: `Docs/09`, `Docs/DEFERRED_BACKLOG.md`, `.claude/CONTEXT.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek zamanlayıcı; dakikada bir tarama; gecelik iş son 03:00'ten eskiyse başlar, başarısızsa en erken bir saat sonra yinelenir | ✓ | `SchedulerRegistrationTests.Application_HasOneSchedulerService_StartedAfterTheJobRunner`; `SchedulerHeartbeatTests.Service_ScansOncePerScanInterval`; `SchedulerNightlyTests.RunIfDue_AFailedRun_WritesNoSuccess_IsNotTriedBeforeTheRetryWait_AndIsTriedAtIt`; `NightlyJobScheduleTests` |
| 2 | Nabız ayrı döngüde, 30 sn, yalnız canlılık 200 ise; iki dakikayı aşan boşluk kesinti, kısası değil | ✓ | `SchedulerHeartbeatTests.Beat_AnsweredWith200_…`, `Beat_NotAnsweredWith200_WritesNothing`, `Beat_AGapOfTheThreshold_IsNoOutage_AndALongerGapIsRecordedWithTheFirstBeat`, `Service_ASlowScan_DoesNotDelayTheHeartbeat_…`; `OutageRecoveryRuleTests` |
| 3 | Zamanı gelen kayıt kendi geçişiyle, sipariş kilidi altında | ✓ | `SchedulerScanTests.OrderSource_TakesTheOrderRowLock_BeforeItChangesTheOrder_AndRaisesTheVersion`, `OrderSource_AnOrderThatStoppedBeingDueBeforeItsLock_IsNotChanged` |
| 4 | İş çalışırken oturum kapsamlı kilit; aynı iş iki kez aynı anda çalışmaz | ✓ | `SchedulerHeartbeatTests.Service_TwoInstances_ScanOnce_BecauseTheSchedulerLockIsHeldByOne`; `SchedulerNightlyTests.RunIfDue_WhileOneInstanceRuns_TheNightlyLockIsHeld_AndASecondInstanceDoesNotRunTheJob` |
| 5 | İşin zamanı kendi kaydında; ayrı takvim tablosu yok | ✓ | `SchedulerScanTests.OrderSource_TheTimeIsTheOrdersOwnField_AnOrderNotYetDueIsLeftAlone_AndADoneOrderIsNotDoneTwice`; `Scan_AMissedNight_…TheTimeIsInTheHeartbeatRecordNotACalendar` |
| 6 | Hatırlatma son iş gününün 08:00'i, gecelik 03:00; yanıtsız son sorgu beş dakikada bir | ✓ | `TransferScheduleTests`, `NightlyJobScheduleTests`, `BusinessCalendarTests` (NextBusinessDay); `SchedulerTimingsConsistencyTests` (sabitler ↔ `09 §3.5`) |
| 7 | Dönüşte kesinti kaydı + ilk nabız + son anların taşınması tek işlem; taşınan havale iptali ertelenir, hatırlatma düşer; 03:00'ı kapsayan kesintide gecelik iş sonda bir kez; bayat nabızla havale iptali işlenmez | ✓ | `Beat_ReturnFromAnOutage_PutsOffTheTransferOrderWhoseWindowRanOut_AndDropsItsReminder`, `Beat_WhenThePutOffFails_NeitherTheBeatNorTheOutageRecordIsWritten`, `Scan_AfterAnOutageThatCoveredTheNightlyHour_RunsTheNightlyJobOnce`, `Scan_ASourceThatWaitsForAFreshHeartbeat_IsHeldBackWhileTheLastBeatIsOld_AndRunsWhenItIsFresh`, `Scan_WithNoHeartbeatAtAll_…`, `Scan_TheHeartbeatGoingOldBetweenTheLookAndTheRecord_…` |
| 8 | Süreler dondurulmaz; kaçırılan işler sırayla çalışır (7'deki hatırlatma hariç) | ✓ | `Scan_PutsTheRecordsOfAllSourcesInOrderOfTheirMoments_AndAFailedRecordDoesNotStopTheOthers`; `Scan_AMissedNight_IsMadeUpAtTheFirstWakeUpAfterIt_OnceOnly_…` |
| 9 | Saat tek kaynaktan | ✓ | `TimeAndMoneyRulesTests` (kaynak taraması; Unit yeşil) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 sahte saatle dakika taraması bir kez; başarısız gecelik iş bir saat dolmadan yinelenmez | ✓ | `Service_ScansOncePerScanInterval`; `RunIfDue_AFailedRun_…`; `Scan_JustBeforeTheHour_DoesNotRunTheNight_AndAtTheHourRunsIt` |
| KK2 yavaş iş nabzı geciktirmez; başarısız istekte nabız yok; kısa boşluk kesinti açmaz | ✓ | `Service_ASlowScan_…`; `Beat_NotAnsweredWith200_WritesNothing`; `Beat_AGapOfTheThreshold_IsNoOutage_…` |
| KK3 iş siparişin kilidi altında | ✓ | `OrderSource_TakesTheOrderRowLock_…` |
| KK4 iki örnek aynı işi aynı anda çalıştırmaz | ✓ | `Service_TwoInstances_ScanOnce_…`; `RunIfDue_WhileOneInstanceRuns_…` |
| KK5 iş zamanı kayıt alanından | ✓ | `OrderSource_TheTimeIsTheOrdersOwnField_…` |
| KK6 08:00 ve 03:00 Türkiye anları; yanıtsız sorgu beş dakikada | ✓ | `TransferScheduleTests`, `NightlyJobScheduleTests`, `SchedulerTimingsConsistencyTests` |
| KK7 kesinti kaydı + ilk nabız tek işlemde; taşınana hatırlatma gitmez; bayat nabızda iptal işlenmez; 03:00 kesintisinde gecelik iş bir kez | ✓ | madde 7 testleri |
| KK8 kaçırılan işler sırayla | ✓ | `Scan_PutsTheRecordsOfAllSourcesInOrderOfTheirMoments_…` |
| KK9 kodda sistem saati doğrudan okunmaz | ✓ | `TimeAndMoneyRulesTests` |
| `09 §25` ilgili satırlar | ✓ | `§25` arka plan satırı, `§3.5` yeni satır (dış çağrı süresi ve zamanlayıcı süreleri), `§13.1` m.10 koda eşit; `CONTEXT.md` |
| Güvenlik taraması | ✓ | zamanlayıcı kayıtlarında ham hata iletisi yok (özet: tür adı); `JobSourceScan` zamanlayıcı dosyaları tam yol, kendi kuralıyla sınırlı, ayrı ihlal örnekleri (`JobSourceScanTests`); `ConcurrencySourceScan` `pg_try_advisory_*` ihlal örneği; sır yok |
| Sık FAIL nedenleri | ✓ | tarama istisnaları tam yol ve dar; her yeni tarama kuralının ayrı ihlal örneği var; beklenen süreler `SchedulerTimings`'ten (`SchedulerTimingsConsistencyTests` ↔ `09 §3.5`); birim testi dosya yazmaz; kural değişimi `09`, `CONTEXT.md`, backlog, `compose.dev.yaml` (D-39 (f)) ve STATUS/MEMORY'de eşitlendi; hedefi T221 olan backlog satırları (D-98, D-97, D-39 (f)) kapatıldı/yeniden yazıldı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet run --project tests/Shopfolio.UnitTests` | 996 geçti |
| Integration | ✓ (parça parça) | `dotnet run --project tests/Shopfolio.IntegrationTests -- -namespace …` | Jobs 73, Web 72, Concurrency+Security+Logging+FileSecrets 98, Database+AutoGenerated 35, Model 893 — hepsi geçti |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Lint | ✓ | `dotnet build -warnaserror`, `dotnet format --verify-no-changes`, `dotnet restore --locked-mode` | temiz |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** `deploy/compose.dev.yaml` veritabanı komutu (dev bindirmesi, `archive_mode=off`; D-39 (f))
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T221-zamanlayici`
- **PR:** #394

## Dış Varsayımlar

Dış varsayım: yok (PostgreSQL yerel konteyneri; ağ çağrısı yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Zamanlayıcıya bağlı kaynaklar, kesinti etkileri ve gecelik adımlar (sahip task'larda) | D-99 | Sahip task'ın işi; kayıt noktaları hazır |
| 2 | Gecelik temizlik dışındaki işleyiciler | D-97 | İşleyici sahip task'ın işi |

## Notlar

- **⚠ Kararlar:** (1) Dönüş gününden sonraki ilk iş günü sayılır, pencere sonu o günün sonu. (2) Son nabız bayatken yalnız taze nabız bekleyen kaynaklar durur; hiç nabız yoksa da bayat. (3) Kaynaklar arası kayıtlar zaman, kaynak sırası, kimlik sırasıyla; tur başına kaynak başına en çok 500. (4) `ExternalCallTimeouts` bu task'ta oluşturuldu. (5) Nabız hatası hizmeti durdurur. (6) D-39 (f) bu task'ta uygulandı. (7) Gecelik iş hatası yutulur ve loglanır, bir saat sonra yeniden denenir; üçüncü geniş `catch` yeri `SchedulerScan` + `NightlyJobRunner` dosyalarıdır.
- **Ana dal CI startup (Adım 0):** kontrol edilmedi (yardımcının işi).
- **Repo memory (bitiş kapısı 9):** güncellendi.
