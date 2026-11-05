# T221 — Zamanlayıcı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (doğrulama tur 1 FAIL) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Jobs/`: `SchedulerService` (uygulamada tek `BackgroundService`; iki ayrı döngü: dakikalık tarama ve otuz saniyelik nabız; tek zamanlayıcı oturum kilidi), `SchedulerScan` (kaynaklar arası zamanı gelen kayıtları zaman, kaynak sırası, kimlik sırasıyla işler; tavansız: bir uyanışın bulduğu bütün kayıtlar tek sırada, sonra gecelik iş), `NightlyJobRunner` (son başarı son 03:00'ten eskiyse başlar; başarısızsa bir saat sonra yinelenir; gecelik iş kilidi), `HeartbeatPulse` (canlılık isteği 200 ise nabız yazılır), `OutageRecovery` (iki dakikayı aşan boşluk kesinti kaydıdır; kesinti kaydı, ilk nabız ve son anların taşınması tek işlemde), `NightlyJobSchedule`, `TransferSchedule` (08:00 hatırlatma, beş dakikalık yanıtsız sorgu), `SchedulerTimings`, `SchedulerContracts`, `SchedulerRegistration`, `JobQueueCleanupStep`.
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
| KK2 yavaş iş nabzı geciktirmez; başarısız istekte (200 dışı, bağlantı reddi, zaman aşımı) nabız yok ve döngü sürer; kısa boşluk kesinti açmaz | ✓ | `Service_ASlowScan_…`; `Beat_NotAnsweredWith200_WritesNothing`, `Beat_ARefusedConnection_WritesNothing`, `Beat_NoAnswerWithinTheUserWaitingLimit_WritesNothing`, `Service_AFailedBeat_DoesNotEndTheLoop_TheNextBeatWrites`; `Beat_AGapOfTheThreshold_IsNoOutage_…` |
| KK3 iş siparişin kilidi altında | ✓ | `OrderSource_TakesTheOrderRowLock_…` |
| KK4 iki örnek aynı işi aynı anda çalıştırmaz | ✓ | `Service_TwoInstances_ScanOnce_…`; `RunIfDue_WhileOneInstanceRuns_…` |
| KK5 iş zamanı kayıt alanından | ✓ | `OrderSource_TheTimeIsTheOrdersOwnField_…` |
| KK6 08:00 ve 03:00 Türkiye anları; yanıtsız sorgu beş dakikada | ✓ | `TransferScheduleTests`, `NightlyJobScheduleTests`, `SchedulerTimingsConsistencyTests` |
| KK7 kesinti kaydı + ilk nabız tek işlemde; taşınana hatırlatma gitmez; bayat nabızda iptal işlenmez; 03:00 kesintisinde gecelik iş bir kez ve sonda | ✓ | madde 7 testleri; `Scan_AfterALongOutage_…` (gecelik iş bütün kayıtlardan sonra) |
| KK8 kaçırılan işler sırayla (tavansız, kaynaklar arası tek sıra; gecelik iş sonda) | ✓ | `Scan_PutsTheRecordsOfAllSourcesInOrderOfTheirMoments_…`; `Scan_AfterALongOutage_HandsEveryMissedRecordInOneOrder_AndTheNightlyJobRunsAfterTheLastOne` (1200 + 4 kayıt) |
| KK9 kodda sistem saati doğrudan okunmaz | ✓ | `TimeAndMoneyRulesTests` |
| `09 §25` ilgili satırlar (düzeltme 1: tavan kalktı, yeni tavan/sabit yok; m.6 ve m.7 metni kodla eşit) | ✓ | `§25` arka plan satırı, `§3.5` yeni satır (dış çağrı süresi ve zamanlayıcı süreleri), `§13.1` m.10 koda eşit; `CONTEXT.md` |
| Güvenlik taraması | ✓ | zamanlayıcı kayıtlarında ham hata iletisi yok (özet: tür adı); `JobSourceScan` zamanlayıcı dosyaları tam yol, kendi kuralıyla sınırlı, ayrı ihlal örnekleri (`JobSourceScanTests`); `ConcurrencySourceScan` `pg_try_advisory_*` ihlal örneği; sır yok |
| Sık FAIL nedenleri | ✓ | tarama istisnaları tam yol ve dar; her yeni tarama kuralının ayrı ihlal örneği var; beklenen süreler `SchedulerTimings`'ten (`SchedulerTimingsConsistencyTests` ↔ `09 §3.5`); birim testi dosya yazmaz; kural değişimi `09`, `CONTEXT.md`, backlog, `compose.dev.yaml` (D-39 (f)) ve STATUS/MEMORY'de eşitlendi; hedefi T221 olan backlog satırları (D-98, D-97, D-39 (f)) kapatıldı/yeniden yazıldı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet run --project tests/Shopfolio.UnitTests` | 996 geçti |
| Integration | ✓ (parça parça) | `dotnet run --project tests/Shopfolio.IntegrationTests -- -namespace …` | Jobs 77, Web 72, Concurrency+Security+Logging+FileSecrets 98, Database+AutoGenerated 35, Model 893 — hepsi geçti |
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

- **⚠ Kararlar:** (1) Dönüş gününden sonraki ilk iş günü sayılır, pencere sonu o günün sonu. (2) Son nabız bayatken yalnız taze nabız bekleyen kaynaklar durur; hiç nabız yoksa da bayat. (3) Kaynaklar arası kayıtlar zaman, kaynak sırası, kimlik sırasıyla; tavan yok (düzeltme turu 1: bkz. aşağıda). (4) `ExternalCallTimeouts` bu task'ta oluşturuldu. (5) Nabız hatası hizmeti durdurur. (6) D-39 (f) bu task'ta uygulandı. (7) Gecelik iş hatası yutulur ve loglanır, bir saat sonra yeniden denenir; üçüncü geniş `catch` yeri `SchedulerScan` + `NightlyJobRunner` dosyalarıdır.
- **Ana dal CI startup (Adım 0):** kontrol edilmedi (yardımcının işi).
- **Repo memory (bitiş kapısı 9):** güncellendi.

---

## Doğrulama Sonucu — T221 Zamanlayıcı — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T221-zamanlayici   **Commit:** b43db71 (doğrulanan head)

### Verdict: ✗ FAIL

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (switch öncesi ve sonrası) |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 41999269209 success · 41999269502 success · 41981392743 success |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22 T221 |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek zamanlayıcı; dakikada bir tarama; gecelik iş son 03:00'ten eskiyse, başarısızsa en erken 1 saat sonra | ✓ | Tek `AddHostedService<SchedulerService>` (`SchedulerRegistration.cs`), `JobSourceScan.ServiceFiles` iki dosya; `NightlyJobSchedule.IsDue` `09 §13.1` m.9 kalıbıyla birebir; `Service_ScansOncePerScanInterval`, `RunIfDue_AFailedRun_…`, `NightlyJobScheduleTests` yeşil |
| 2 | Nabız ayrı döngüde, 30 sn, yalnız istek başarılıysa; > 2 dk boşluk kesinti, kısası değil | ✓ | `SchedulerService` iki döngü `Task.WhenAny`; `HeartbeatPulse` yalnız 200'de `OutageRecovery.ApplyAsync`; `IsOutage` `>` eşik; `Beat_NotAnsweredWith200_WritesNothing` (503/502/404), `Beat_AGapOfTheThreshold_IsNoOutage_…`, `Service_ASlowScan_DoesNotDelayTheHeartbeat_…` yeşil (bağlantı/zaman aşımı yolu testsiz → K2) |
| 3 | Zamanı gelen kayıt kendi geçişiyle, sipariş kilidi altında | ✓ | `ScheduledOrderTransition.ProcessAsync`: işlem → `OrderLocks.LockAsync` → yeniden okuma → `IsDue` → `Apply` → `BumpVersion`; `OrderSource_TakesTheOrderRowLock_…` (ikinci bağlantının kilidi reddedilir), `OrderSource_AnOrderThatStoppedBeingDueBeforeItsLock_IsNotChanged` yeşil |
| 4 | Oturum kapsamlı kilit; aynı iş aynı anda iki kez çalışmaz | ✓ | `AdvisoryLocks.TryHoldAsync` `Scheduler` ve `NightlyJob` adlarıyla, her turda `EnsureAliveAsync`; `Service_TwoInstances_ScanOnce_…`, `RunIfDue_WhileOneInstanceRuns_TheNightlyLockIsHeld_…` yeşil |
| 5 | İşin zamanı kendi kaydında; takvim tablosu yok | ✓ | `IDueRecordSource.FindDueAsync` kaydın alanından okur; gecelik anlar `HeartbeatRecord`'da; dalda göç yok; `OrderSource_TheTimeIsTheOrdersOwnField_…`, `Scan_AMissedNight_…NotACalendar` yeşil |
| 6 | Hatırlatma son iş gününün 08:00'i, gecelik 03:00 (Türkiye); yanıtsız son sorgu 5 dk'da bir | ✓ | `TransferSchedule.ReminderAt`, `NightlyJobSchedule.LatestDueMoment` + `TransferScheduleTests`/`NightlyJobScheduleTests` (UTC 05:00/00:00); 5 dk `JobTimings.FinalPaymentQueryInterval` (T220) + `JobRunnerTests` `RescheduleAfter`; `SchedulerTimingsConsistencyTests` ↔ `09 §3.5` |
| 7 | Dönüşte ilk nabız + kesinti kaydı + taşıma tek işlem; iptal ertelenir, kaçırılmış hatırlatma düşer, yenisi gitmez; 03:00 kesintisinde gecelik iş sonda bir kez; bayat nabızda havale iptali işlenmez | ~ | Tek işlem ✓ (`Beat_WhenThePutOffFails_NeitherTheBeatNorTheOutageRecordIsWritten`, tetikleyiciyle düşürülen yazım: üçü de yok); erteleme + `PaymentReminderAt = null` + sürüm ✓ (`Beat_ReturnFromAnOutage_…`); bayat nabızda bekleme ✓ (`Scan_ASourceThatWaitsForAFreshHeartbeat_…`, `…GoingOldBetweenTheLookAndTheRecord…`, `Scan_WithNoHeartbeatAtAll_…`). **"Gecelik iş sonda"** yalnız uyanışın kayıtları kaynak başına 500'ü aşmazsa tutar (B1) |
| 8 | Süreler dondurulmaz; kaçırılan işler sırayla | ~ | `Scan_PutsTheRecordsOfAllSourcesInOrderOfTheirMoments_…` yeşil; ama kaynak başına 500 tavanında sıra kaynaklar arası bozulur (B1) |
| 9 | Saat tek kaynaktan | ✓ | Yeni `src/` dosyalarında `DateTime(Offset).(Utc)Now`/`now()` yok (grep); `TimeAndMoneyRulesTests` yeşil |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
T221'in `10 KP` birimi yok (`11 §7.1` satırı: `05` MK-41…44, §5.1–§5.4, `09 §13`); satırlar `12`'de `05 §5`/K-898/K-975/nabız/gecelik iş geçen kayıtlardan grep'le bulundu. `VAL-C109`, `VAL-A826` (gerçek havale iptali kaynağı, T327), `VAL-C110` (iptal + kart son sorgusu, T327/T403), `VAL-A827` (tek işlem payı burada kanıtlı — `Beat_WhenThePutOffFails_…`; "ilk iki dakikadaki son an ertelenmez" iptal kaynağını ister, T327) `BEKLEMEDE` kaldı. `VAL-A830` T220'nin satırıdır, değişmedi.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-C111 | KRİTİK | yok | BEKLEMEDE | FAIL (B1) | — |

`python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77`: Hata 0.

### Doğrulama Kontrol Listesi
- [x] KK1 sahte saatle tarama aralık başına bir kez; başarısız gecelik iş 1 saat dolmadan yinelenmez
- [x] KK2 yavaş tarama nabzı geciktirmez; 200 dışı yanıtta nabız yok; ≤ 2 dk boşluk kesinti açmaz (bağlantı/zaman aşımı testi K2)
- [x] KK3 kaynak sipariş kilidi altında koşar
- [x] KK4 iki örnek taramayı ve gecelik işi bir kez çalıştırır
- [x] KK5 iş zamanı kayıt alanından
- [x] KK6 08:00 ve 03:00 Türkiye anları; son sorgu 5 dk
- [ ] KK7 tek işlem, hatırlatma düşer, bayat nabızda iptal yok ✓; "03:00 kesintisinde gecelik iş sonda" tavanda tutmaz (B1)
- [ ] KK8 kaçırılan işler sırayla — tavanda kaynaklar arası sıra bozulur (B1)
- [x] KK9 sistem saati doğrudan okunmaz
- [ ] `09 §25` — "Yeni ayar, süre, tavan … §3.5'in tablosunda mı?" ✗ (`DueBatchLimit`, B1); "Kod hiçbir doküman cümlesiyle çelişmiyor mu?" ✗ (B1); öteki ilgili sorular ✓ (işlemde ağ çağrısı yok, geniş `catch` yalnız §8.1 m.6 yerlerinde, `LockSequence` `OrderLocks` üzerinden, `RunAsync` lambda)
- [x] Güvenlik taraması (aşağıda)

**Hata hâllerinin uygulamaya etkisi (`09 §13.1` m.1, m.6–m.9 karşısında):** nabız isteğinin başarısızlığı (200 dışı, `HttpRequestException`, 15 sn aşımı) yazmaz, döngü sürer ✓; nabzın veritabanı yazımı ya da bir kesinti etkisi istisna atarsa döngü biter, hizmet istisnayı fırlatır, ev sahibi durur (m.1 — belgeyle uyumlu); bir kaydın hatası kaydedilir, tarama sürer (m.6) ✓; gecelik adımın hatası yakalanır, başarı yazılmaz, tarama sürer, 1 saat sonra yeniden (m.9) ✓; kilit bağlantısı koparsa `EnsureAliveAsync` istisnası hizmeti bitirir (m.7) ✓; deneme/başarı anının yazımı ya da kaynak sorgusu düşerse ev sahibi durur (m.1).

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 996/996 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti, çıkış 0 |
| Integration — Jobs | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Jobs` (PostgreSQL 18, `shopfolio-t221-val-pg`) | 73/73 |
| Integration — Web | ✓ | `--filter-namespace …Web` | 72/72 |
| Integration — Concurrency | ✓ | `--filter-namespace …Concurrency` | 26/26 |
| Integration — Security | ✓ | `--filter-namespace …Security` | 18/18 |
| Integration — Database+FileSecrets+Logging | ✓ | üç `--filter-namespace` | 89/89 |
| Integration — Model 1 | ✓ | `--filter-class …Model.{A,B,C,D,E,G,I}*` | 482/482 |
| Integration — Model 2 | ✓ | `--filter-class …Model.{M,P,R,S}*` | 411/411 (toplam 1171) |
| Restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; `git status` temiz |
| Dal CI | ✓ | `gh run list --branch task/T221-zamanlayici` | 42005154029 success (head `b43db71`) |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz — yeni sır, ayar ya da kayıt alanı yok; kayıt satırları kaynak adı ve kayıt kimliği taşır (iç kimlik), istisna iletisi `LoggerMessage` istisna parametresinde.
- Auth etkisi: Temiz — yeni uç yok; nabız yalnız canlılık ucuna `GET` atar.
- Input validation: Temiz — dış girdi yok; nabız yalnız durum kodunu okur, gövdeyi işlemez.
- Yeni bağımlılık: Yok (`packages.lock.json` değişmedi; `--locked-mode` geçti).

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| B1 | `VAL-C111`; kriter 7 ("gecelik iş sonda"), 8; KK7, KK8, `09 §25` | ORTA | S1 | **Beklenen:** her uyanış zamanı gelmiş kayıtların hepsini sorar (`09 §13.1` m.6), zamanlarına göre tek sırada işler ve gecelik iş "bunlardan sonra" koşar (`05 §5.4.3`, §5.1.1; m.9); yeni tavan `09 §3.5` tablosundadır (`§25`). **Gerçekleşen:** `SchedulerTimings.DueBatchLimit = 500` belgede yok (yorumundaki "design value, 09 13.1 item 6" atfı yanlış — m.6 tavan anmaz; sınıf yorumu "Every value is a row of that table" der, değildir). `SchedulerScan.RunAsync` her kaynaktan en çok 500 alır, sonra koşulsuz `_nightly.RunIfDueAsync` çağırır: kesintiden dönüşte bir kaynakta 500'den çok kaçırılmış kayıt varsa (1) gecelik iş kalan kayıtlardan önce koşar, (2) kaynak A'nın 501. kaydından daha geç anlı B kayıtları önce işlenir. Tavanı sınayan test yok. | `VAL-C110` ("işler zaman sırasındadır"; T327 gelince) | bu PR'da düzeltilir: ya tavan kaldırılır, ya tavana takılan uyanışta yalnız tavanlı kaynakların son anına kadar olan kayıtlar işlenir ve gecelik iş o uyanışta koşmaz; her iki yolda `§3.5`/yorum eşitlenir (tavan kalırsa satır + `SchedulerTimingsConsistencyTests`) ve tavanlı uyanışı sınayan entegrasyon testi eklenir | `src/Shopfolio.Web/Infrastructure/Jobs/SchedulerScan.cs`, `SchedulerTimings.cs`, `Docs/09_CODING_GUIDELINES.md` §3.5 |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Zamanlayıcı kilidi alamayınca `ScanInterval` (1 dk) bekler; `09 §13.1` m.7 "Çalıştırıcı kilidi alamazsa beş saniye sonra yeniden dener" der (`05 §5.1`'de iki çalıştırıcı vardır). Ya kod beş saniyeye iner ya m.7 zamanlayıcının kendi tarama aralığıyla yeniden denediğini yazar | bu PR'da kapatılır (düzeltme turu) |
| K2 | `HeartbeatPulse` bağlantı reddini (`HttpRequestException`) ve 15 sn aşımını "yazmaz" diye yakalar (yorum da böyle der) ama testler yalnız 200 dışı durum kodunu sınar; `FakeLivenessHandler`'a istisna ve bekleme kipi eklenip iki yolun satır yazmadığı ve döngünün sürdüğü gösterilmeli (`09 §13.1` m.11 "nabız başarısız istekte satır yazmaz") | bu PR'da kapatılır (düzeltme turu) |
| K3 | Uygulama yeniden başlayınca taramanın ilk uyanışı dönüşün ilk nabzından önce gelebilir (testin kendisi de bunu gösterir); beklemeyen kaynaklar kesinti kaydı ve erteleme yazılmadan işlenir. Bugün zararsız, ama gelecek havale hatırlatması kaynağı yalnız `PaymentReminderAt`'e bakarsa penceresi kesintide dolmuş siparişin kaçırılmış hatırlatması gider (KK7) | `DEFERRED_BACKLOG` D-100'e forward (T327, T324) |
| K4 | D-39'un başlık cümlesi hâlâ "(e), (f) açık" diyor; (f) T221'de ✅ | bu PR'da kapatılır (düzeltme turu) |

### Yapım Raporu Karşılaştırması
- Uyum: 3 uyuşmazlık
- Rapor kriter 7, 8, KK7, KK8 ve `09 §25`'i ✓ verir; bu doğrulama B1 nedeniyle ~/✗ verir. Rapor tavanı "⚠ Karar (3)" olarak anar ama `§3.5` satırı yoktur.
- "Yapılan İşler" `TransferSchedule`'a "beş dakikalık yanıtsız sorgu" yazar; o değer `JobTimings.FinalPaymentQueryInterval`'dadır (T220). Öz denetim satırı "`§3.5` yeni satır (dış çağrı süresi ve zamanlayıcı süreleri)" der; dalda eklenen tek satır hatırlatma/kesinti eşiği satırıdır.
- Öz denetim tablosu vardır, her maddesi kanıtlıdır; "Ana dal CI startup: kontrol edilmedi" notu yardımcı düzenine uygundur.

### Merge ve Post-merge
- FAIL: merge yok. Dal `task/T221-zamanlayici`'da rapor, durum, hafıza, matris ve D-100 commit'i duruyor.

## Düzeltme turu 1

Doğrulama tur 1 FAIL (B1) ve K1, K2, K4 için. K3 D-100'e gitti, dokunulmadı.

- **B1 — tavan kaldırıldı.** `SchedulerTimings.DueBatchLimit` ve `FindDueAsync`'in `limit` parametresi silindi (`SchedulerContracts`, `ScheduledOrderTransition`, `SchedulerScan`, test kaynakları). Bir uyanış bütün kaynakların zamanı gelmiş kayıtlarının hepsini alır, tek sırada (zaman, kaynak sırası, kimlik) işler, gecelik iş sonda sorulur. `09 §13.1` m.6'ya bu cümle eklendi; yeni sabit olmadığı için `§3.5`'e satır gerekmedi. Yanlış atıflı yorum silindi. Yeni test: `Scan_AfterALongOutage_HandsEveryMissedRecordInOneOrder_AndTheNightlyJobRunsAfterTheLastOne` (iki kaynak, 1200 + 4 kayıt, sıra kayıtların zamanlarından türetilir; gecelik adım çalıştığı an işlenen sayı toplam kayıt sayısına eşit).
- **K1 — madde kodla eşitlendi (belge değişti).** `09 §13.1` m.7: çalıştırıcı kilidi alamazsa beş saniye, zamanlayıcı kendi tarama aralığıyla (§3.5; bir dakika) yeniden dener. Kod değişmedi: zamanlayıcının tek işi dakikalık tarama olduğundan beş saniyelik yoklama kazanç getirmez, yeni sabit ve `§3.5` satırı da gerektirmez.
- **K2 — testler eklendi.** `FakeLivenessHandler`'a `Refuse` ve `Hang` kipleri; `Beat_ARefusedConnection_WritesNothing`, `Beat_NoAnswerWithinTheUserWaitingLimit_WritesNothing` (gerçek 15 sn sınır; kayıt yok), `Service_AFailedBeat_DoesNotEndTheLoop_TheNextBeatWrites` (döngü sürer, sonraki nabız yazar).
- **K4 — D-39 başlık cümlesi** "(e) açık, hedef güncellendi; (f) ✅ T221 (2026-11-05)" oldu.

**⚠ Kararlar (düzeltme):** (1) Tavan kaldırıldı, tavan korunmadı: `09 §13.1` m.6 "zamanı gelmiş kayıtları sorar" der, tavan anmaz; sıra ve "gecelik iş sonda" kuralı tavansız kendiliğinden tutar. Bellek yükü kayıt başına kimlik ve an olduğundan uzun kesintide bile küçüktür. (2) K1'de belge değiştirildi, kod değil.

**Öz denetim (düzeltilmiş diff, `git diff origin/main...HEAD`):** kriter 1–9 ve KK1–KK9 yeniden denetlendi; yukarıdaki tablolar güncellendi (KK2, KK7, KK8, `§25`). `grep -rn "DueBatchLimit\|Take(limit)\|int limit" src tests` boş. Sık FAIL nedenleri: belgede olmayan sabit kalmadı; eşlik eden yerler (rapor satırı 11 ve 88, D-39, `09` m.6 ve m.7) güncel; yeni sabit olmadığı için sabitten türetme gerekmedi, beklenen sıra kayıt zamanlarından türetilir; yeni tarama istisnası yok; backlog: D-39 (f) başlığı düzeldi, D-100 değişmedi.

**Testler:** `dotnet format --verify-no-changes` temiz; `dotnet build -warnaserror` 0 uyarı; Unit 996/996; Integration Jobs 77/77, Web 72/72; betik testleri 4 dosya geçti.
