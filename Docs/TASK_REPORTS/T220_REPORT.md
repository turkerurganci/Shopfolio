# T220 — İş kuyruğu çalıştırıcısı

**Faz:** F2 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Jobs/`: `JobRunnerService` (tek `BackgroundService`; tek çalışma oturum kilidi, sinyal ya da beş saniye, satırları birer birer işler, bir satırın hatası döngüyü durdurmaz), `JobQueue` (`IJobQueue.EnqueueAsync`; `INSERT … ON CONFLICT DO NOTHING`, çağıranın işleminde), `JobEnqueueTracker` + `TransactionRunner` (kesinleşince tek uyandırma, geri alınanda yok), `JobSignal` (kapasitesi bir kanal), `JobClaim` (`FOR UPDATE SKIP LOCKED` ile satır alma), `JobRetryPolicy` (varsayılan kural 1/5/15/60/240 dk; altıncı hata `Failed`), `JobOutcome`, `JobCleanup` (yalnız tamamlananı siler), `JobTimings` (tek sabit yeri), `JobQueueRegistration` (`AddJobQueue`, `AddJobHandler`, `AddTransactionalJobHandler`, `AddJobStartGate`).
- `Payment/CardRefundJobRule`: kart iadesi kararı (kayıt yok → gönder; "gönderiliyor" ya da sonuç bekleniyor → sonucu sor; sonuçlanmış → hiçbir şey).
- `AdvisoryLocks.TryHoldAsync` (oturum kapsamlı, ayrı bağlantı); koparsa `DisposeAsync` susar (NpgsqlException ve InvalidOperationException).
- D-96 K1: `IJobStartGate` + `RevertLinkDigestRewriteGate` (`HasRun`); veritabanı açılıştan sonra gelirse adım bir kez koşar, kuyruk ancak sonra satır alır.
- Kaynak taramaları: `JobSourceScan` (5 kural, ayrı ihlal örnekleri), `ConcurrencySourceScan` JobClaim.cs izni (yalnız düz `FOR UPDATE`).
- Belgeler: `09 §13.1` m.2, m.3, m.7, m.10; `§10.2` m.6; `§18.2` m.3 (`JobTypeUnknown`; doğrulamada `§18.1`'den düzeltildi); `§25` arka plan satırı; `CONTEXT.md`; backlog D-96/D-97.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Infrastructure/Jobs/*`, `AdvisoryLocks.cs`, `TransactionRunner.cs`, `Logging/LogEvents.cs`, `Program.cs`, `Modules/Notifications/{NotificationsModule,RevertLinkDigestRewrite}.cs`, `Modules/Payment/CardRefundJobRule.cs`; testler: `UnitTests/{Jobs,Architecture,Consistency,Fakes}`, `IntegrationTests/{Jobs,Web,Concurrency,Security}`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek arka plan döngüsü; uyanma + beş saniye; birer birer; hata durdurmaz | ✓ | `JobRunnerService`; `RunnerService_AfterACommitThatWroteARow_Wakes…`, `RunnerService_ARowWrittenWithoutAWakeUp_IsFoundWhenTheFiveSecondsPass`, `RunOne_AFailingRowDoesNotHoldTheRowsBehindIt` |
| 2 | Satır kilidiyle alma, kilit sütunu yok; çöken süreç satırı yeniden alınır | ✓ | `JobClaim.cs`; `Claim_ARowAnotherTransactionHolds_IsSkipped_NotWaitedFor`, `TwoRunnerServices_HandleEveryRowExactlyOnce`, `RunOne_CancelledWhileTheHandlerWorks_…` |
| 3 | En az bir kere teslim; ikinci görüşte aynı sonuç | ✓ | `RunOne_ARowSeenTwice_BecauseTheOutcomeWasNeverWritten_GivesTheSameResult` |
| 4 | İç yan etki geçişle aynı işlemde; satır aynı işlemde | ✓ | `JobEnqueueTests.Enqueue_InATransaction*`; `RunOne_ATransactionalHandler_WritesItsEffectAndTheCompletionInOneCommit` |
| 5 | Yeniden deneme iş türünün kuralı; varsayılan kural; sıradakini bekletmez | ✓ | `JobRetryPolicyTests`; `RunOne_AHandlerThatAlwaysFails_…`, `RunOne_TheBounceQueryThatFails_…` |
| 6 | Başarısız satır silinmez | ✓ | `Cleanup_WithOnlyFailedRows_DeletesNothing`, `Cleanup_DeletesTheCompletedRows_AndKeepsTheFailedAndThePendingOnes` |
| 7 | Gönderilmiş iade tekrar gönderilmez; istekten sonra çöken süreç sonucu sorar | ✓ | `CardRefundJobRuleTests`; `CardRefund_*` (4 test; gerçek sipariş, ödeme kaydı ve iade satırıyla) |
| 8 | E-posta ayrı kuyruk satırı yazmaz | ✓ | `QueueingAnEmail_WritesTheDispatchRecordAndNoJobRow`, `JobTypes_HaveNoEmailMember_…` |
| 9 | Dış çağrı veritabanı kilidinin içinde beklenmez | ✓ | `RunOne_CallsAnOutsideHandlerWithNoTransactionOpen_NoRowLockHeld_AndNoOrderLockPossible` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 beş saniyeyi beklemeden uyanır | ✓ | `RunnerService_AfterACommit…WithoutTheClockMoving`; `JobSignalTests` |
| KK2 iki çalıştırıcı aynı satırı almaz; öldürülen sürecin satırı işlenir | ✓ | `Claim_ARowAnotherTransactionHolds…`, `SingleRunLock_*`, `TwoRunnerServices_*`, `RunnerService_WhileAnotherHolds…`, `RunOne_CancelledWhileTheHandlerWorks…` |
| KK3 iki görüşte aynı sonuç | ✓ | `RunOne_ARowSeenTwice…` |
| KK4 geri alınan geçişte kuyruk satırı yok | ✓ | `Enqueue_InATransactionThatIsRefused_…`, `…Throws_…`, `Commit_OfATransactionThatWroteNoRow_WakesNobody` |
| KK5 aralıklar `05 §4.3.4`, altıncı hata `Failed`, sıradaki işlenir | ✓ | `RunOne_AHandlerThatAlwaysFails…`; aralıklar `JobTimings`'ten (`JobTimingsConsistencyTests` ↔ `09 §3.5` tablosu) |
| KK6 temizlik başarısızı silmez | ✓ | `Cleanup_*` |
| KK7 gönderilmiş iade tekrar gönderilmez; çöken süreçte sonuç sorulur | ✓ | `CardRefund_ASentRequest_IsNeverSentAgain_AndTheProcessThatDiedRightAfterItAsksTheResult` ve öteki üç |
| KK8 e-posta için ayrı satır yok | ✓ | `QueueingAnEmail_…` |
| KK9 işleyici sipariş kilidi altında dış çağrı yapmaz | ✓ | `RunOne_CallsAnOutsideHandlerWithNoTransactionOpen…` |
| `09 §25` ilgili satırlar | ✓ | `§25` arka plan satırı güncellendi; `§13.1` m.2/3/7/10, `§10.2` m.6 koda eşit |
| Güvenlik taraması | ✓ | işleyici hata iletisi satıra yazılmaz (`RunOne_ADatabaseErrorOfTheHandler_IsSummarizedAsTheTypeNameAndTheSqlState_NeverTheMessage`); `JobSourceScan` (genel `catch`, `Task.Run`, `Ignore`, `DbSet<Job>`); sır yok |
| Sık FAIL nedenleri | ✓ | tarama istisnaları tam yol ve dar (`JobClaim.cs` yalnız `FOR UPDATE`); ayrı ihlal örnekleri (`BackgroundWorkViolations`, `ConcurrencySourceScanTests`); beklenen değerler `JobTimings`'ten; birim testleri dosya yazmaz; D-96 K1 kapandı, kural değişimi `09`, `CONTEXT.md`, `§25`'e işlendi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet run --project tests/Shopfolio.UnitTests` | 972 geçti |
| Integration | ✓ (parça parça) | `dotnet run --project tests/Shopfolio.IntegrationTests -- -namespace …` | Jobs 46, Web 69, Concurrency 26, Security+Logging+FileSecrets 72, Database 35, Model 893 — hepsi geçti |
| Lint | ✓ | `dotnet build -warnaserror`, `dotnet format --verify-no-changes` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-05, bağımsız doğrulayıcı; dal head `2deb469`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → `DEFERRED_BACKLOG` D-98; K2 bu PR'da kapatıldı (rapor etiketi) |

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Tek arka plan döngüsü; işlem bitince uyanır, beş saniyede bir bakar; birer birer; satırın hatası durdurmaz (KK1) | ✓ | `JobRunnerService` tek `BackgroundService` (`JobSourceScan`: başka dosyada türeyen yok, `Ignore` yok); `ServeWhileHoldingAsync` satır yoksa `IJobSignal.WaitAsync(PollInterval)`; `TransactionRunner` yalnız kesinleşen ve `EnqueueAsync` çağıran işlemden sonra `Notify`. `RunnerService_AfterACommitThatWroteARow_WakesAndHandlesItWithoutTheClockMoving` (saat ilerlemeden işlenir), `RunnerService_ARowWrittenWithoutAWakeUp_IsFoundWhenTheFiveSecondsPass`, `JobSignalTests` (5), `RunOne_AFailingRowDoesNotHoldTheRowsBehindIt`; geniş `catch` yalnız satır döngüsünde, `current` boşken (alma hatası) hizmet düşer (`05 §4.3.2`) |
| 2 | Satır kilidiyle alma, kilit sütunu yok; aynı satır iki kez alınmaz; çöken sürecin satırı yeniden alınır; madde 9'un kilidi siparişinki (KK2) | ✓ | `JobClaim.TakeNextAsync` `FOR UPDATE SKIP LOCKED LIMIT 1`, yalnız bilinen türler, `COALESCE(next_attempt_at, created_at), id`; `jobs`'a sütun eklenmedi (göç yok). Tek çalışma: `AdvisoryLocks.TryHoldAsync` (`pg_try_advisory_lock`, ayrı bağlantı), her tur `EnsureAliveAsync`. `Claim_ARowAnotherTransactionHolds_IsSkipped_NotWaitedFor`, `TwoRunnerServices_HandleEveryRowExactlyOnce`, `RunnerService_WhileAnotherHoldsTheSingleRunLock_…`, `SingleRunLock_*` (2), `RunOne_CancelledWhileTheHandlerWorks_…_AndANewRunnerFinishesIt` (sayı 0, hata boş, yeni çalıştırıcı bitirir) |
| 3 | En az bir kez teslim; ikinci görüşte sonuç değişmez (KK3) | ✓ | Sonuç yazılmadan düşen tur satırı `Pending`/0 bırakır; `RunOne_ARowSeenTwice_BecauseTheOutcomeWasNeverWritten_GivesTheSameResult` (iki çağrı, tek etki, `Completed`) |
| 4 | İç yan etki geçişle aynı işlemde, dış yan etki satırı aynı işlemde (KK4) | ✓ | `JobQueue.EnqueueAsync` açık işlem yoksa atar, `INSERT … ON CONFLICT (job_type, related_record_id) WHERE job_state = 'Pending' DO NOTHING` çağıranın işleminde; `Enqueue_InATransactionThatCommits_…`, `…IsRefused_LeavesNoRowNoInnerEffectAndNoWakeUp`, `…Throws_…`, `Commit_OfATransactionThatWroteNoRow_WakesNobody`; işlemsel işleyicide etki + tamamlanma tek commit (`RunOne_ATransactionalHandler_…`, `…FailsAfterItWrote_RollsTheEffectBack_…`) |
| 5 | Yeniden deneme türün kuralı; varsayılan 1/5/15/60/240 dk, altıncı hata "başarısız", sıradakini bekletmez (KK5) | ✓ | `JobRetryPolicy.AfterFailure` saf işlev; `BounceQuery` 15 dk sayı artmaz, `CardRefund` ve öteki türler varsayılan; değerler `JobTimings`'te, `JobTimingsConsistencyTests` `09 §3.5` satırına eşit. `RunOne_AHandlerThatAlwaysFails_IsRetriedAtTheDocumentedMoments_…` (her anın 1 ms öncesi alınmaz, anında alınır; 6. hatada `Failed`, `AttemptCount` 6, `NextAttemptAt` boş; 30 gün sonra alınmaz), `RunOne_AFailingRowDoesNotHoldTheRowsBehindIt`, `JobRetryPolicyTests` |
| 6 | "Başarısız" satır silinmez (KK6) | ✓ | `JobCleanup.DeleteCompletedAsync` yalnız `Completed` siler (tek silme yeri); `Cleanup_DeletesTheCompletedRows_AndKeepsTheFailedAndThePendingOnes`, `Cleanup_WithOnlyFailedRows_DeletesNothing` |
| 7 | Gönderilmiş kart iadesi yeniden gönderilmez, sonuç sorulur; kayıt yazılmadan düşen deneme varsayılan kuralla (KK7) | ✓ | `CardRefundJobRule.Decide`: kayıt yok → gönder, `Sending`/`OutcomePending` → sor, `Succeeded`/`Failed` → tamamla; kayıt satırdan sonra yazılmış olandır (`CreatedAt ≥ job.CreatedAt`; `05 §4.6.1`). Gerçek sipariş/ödeme/iade satırlarıyla `CardRefund_ASentRequest_IsNeverSentAgain_AndTheProcessThatDiedRightAfterItAsksTheResult` (1 gönderim, 1 sorgu), `…AFailureBeforeTheRecordWasWritten_…` (0 gönderim, 1 dk sonra gönderir), `…AResultAlreadyWritten_…`, `…NoRecordYet_…` (gönderim anında kayıt `Sending`); `CardRefundJobRuleTests` (4). Gerçek işleyici T405'in (D-97) |
| 8 | E-posta ayrı kuyruk satırı yazmaz (KK8) | ✓ | `JobType`'ta e-posta üyesi yok (`JobTypes_HaveNoEmailMember_…`), `EnqueueAsync` tanımsız türü reddeder; `QueueingAnEmail_WritesTheDispatchRecordAndNoJobRow` |
| 9 | Dış çağrı veritabanı kilidinin içinde beklenmez (KK9) | ✓ | Dış işleyici adım 1'in commit'inden sonra, işlem yokken çağrılır; sonuç ayrı kısa işlemde (`JobClaim.LockAsync` izleyiciyi temizler). `RunOne_CallsAnOutsideHandlerWithNoTransactionOpen_NoRowLockHeld_AndNoOrderLockPossible` (işlem yok, sipariş kilidi reddedilir, `idle in transaction` 0, satır `NOWAIT` ile alınabilir) |

**`09 §25` (ilgili satırlar):** geçiş satırı (iç yan etki aynı işlemde, dış yan etki `Job` satırı) ve arka plan satırı (iş türü kayıtlı, yeniden deneme saf işlevde, alma/kilit yalnız `JobClaim.cs`, işleyici işlem dışında) kodla birebir. `09` değişiklikleri (`§13.1` m.2, m.3, m.7, m.10; `§10.2` m.6; `§18.2` m.3) kodu anlatır ve `05 §4.3`, §5.1, `06 §4.59` ile çelişmez.

**Güvenlik sınaması (talimatın ayrıca istediği):** (a) satır alma yarışı: `SKIP LOCKED` + oturum kilidi, iki çalıştırıcıda her satır bir kez; (b) geri alınan işlem: satır, iç etki ve uyandırma yok (ret ve istisna ayrı); (c) gönderilmiş iade: yeniden gelen satır göndermez, sorar; (d) işleyici işlem ve kilit dışında; (e) varsayılan aralıklar anı anına; (f) "başarısız" satır silinmez, yeniden alınmaz. `LastErrorSummary` yalnız tür adı + SQLSTATE (`…_NeverTheMessage`); hata satırının iletisi biçimleyicide düşer (`09 §18.2` m.2c). Sır yok, yeni paket yok, yetki/girdi yüzeyi yok (uç eklenmedi).

**Task satırları (`12 §7.5`):** `KP-68` grep'i `VAL-A332`–`A336`'yı getirir; hepsi e-postanın yanıt adresi, P-38 denemeleri, "ulaşmadı" işareti ve panelden yeniden gönderimdir (T305, T385) — T220'nin kriterlerine eşlenmez, `Durum` `BEKLEMEDE` kaldı. Kaynakları `05 §4.3`'ü anan `VAL-A828` (kriter 5–6) gerçek görsel türevi işini ve gecelik temizliği ister — çalıştırıcı payı yukarıda kanıtlı, işleyici (T223/T104) ve gecelik işin `JobCleanup` çağrısı (T201/T221) gelmeden satır kapanmaz: `BEKLEMEDE` kaldı. `VAL-A823`, `A829`, `A987` gerçek kart iadesi işleyicisini (T405) ister: `BEKLEMEDE`. `12_matris_denetle.py --kapsam 1-77`: Hata 0.

**K-notları:** (K1) `ConcurrencySourceScan`'ın danışma kilidi deseni `\bpg_advisory\w*` `pg_try_advisory_lock` / `pg_try_advisory_xact_lock` metnini yakalamaz (bu dalın getirdiği `pg_try_advisory_lock` izinli `AdvisoryLocks.cs`'te; başka dosyada ya da `JobClaim.cs`'te yazılırsa tarama susar; desen T228'den) → D-98. (K2) Yapım raporu olay listesini `§18.1` m.3 diye anıyordu, doğrusu `§18.2` m.3 → bu PR'da düzeltildi.

Testler (validator, yerel, head `2deb469`): build `-warnaserror` 0/0 · `dotnet format --verify-no-changes` 0 · Unit 972/972 (Consistency dahil) · betik testleri 4/4 · Integration 1141/1141 (Jobs 46, Web 69, Concurrency 26, Security 18, Database+FileSecrets+Logging 89, Model 496 + 396 + 1; PG 18 konteyner `shopfolio-t220-val-pg`, silindi) · koşu sonrası `git status` temiz, `dotnet restore --locked-mode` 0 · dal CI 41994864604 `success` (head `2deb469`) · ana dal son 3 run (41981392743, 41981392945, 41973187361) `success`.

**Yapım raporu karşılaştırması:** kriter ve öz denetim tabloları her kriter ve KK için kanıtlı; verdict uyumlu. Uyuşmazlık: yalnız `§18.1`/`§18.2` etiketi (K2). Integration sayısı (1141) validator koşusuyla aynı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`jobs` tablosu önceden vardı)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T220-is-kuyrugu`
- **PR:** #393
- **Post-merge run'lar:** squash `6a1ca3a` (#393) — CI 41999269209 success, Publish 41999269502 success (T206 dalında dolduruldu, skill adım 15)

## Dış Varsayımlar

Dış varsayım: yok (PostgreSQL 18 yerel konteyneri; ağ çağrısı yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Hiçbir tür için işleyici kayıtlı değil (T311, T403, T405, T408, görsel türevi; e-posta gönderim kaydının bağlanması) | D-97 | İşleyici sahip task'ın işi |
| 2 | D-96 K2 (`SupersedeEarlierAsync` davet türü) | D-96 | T220 kapsamı dışı; K1 kapandı |

## Notlar

- **⚠ Kararlar:** (1) Satır alma: tek çalışma oturum kilidi + `FOR UPDATE SKIP LOCKED`; işleyici işlem dışında, sonuç ayrı kısa işlemde. (2) Kuralı yazılmamış her tür varsayılan kural; `BounceQuery` hatada 15 dk, sayı artmaz; `GiveUp`'ta `AttemptCount` +1 (=6). (3) Kayıtlı işleyicisi olmayan tür okunmaz; şu an hiçbiri kayıtlı değil (D-97). (4) Başlangıç kapısı kapalıyken çalıştırıcı bekler, host çökmez. (5) `09 §13.1` m.10 "`IHostedService` tam iki" → "`BackgroundService` tam iki". (6) `JobOutcome.RescheduleAfter` sıfırı kabul eder. (7) `ConcurrencySourceScan` yalnız `JobClaim.cs` için düz `FOR UPDATE`'e izin verir. (8) `SessionAdvisoryLock.DisposeAsync`, kopmuş bağlantıda `InvalidOperationException`'ı da yutar (kilit oturumla düşmüştür). (9) Test fabrikasına `withJobRunner` seçeneği: havuzu tek bağlantıya indiren iki canlılık testi çalıştırıcının oturum kilidi bağlantısıyla paylaşamaz. (10) KK7 gerçek ön koşul satırlarıyla (sipariş, ödeme kaydı, iade) koşar; FK tetikleyicisi kapatılmaz.
- **Ana dal CI startup (Adım 0):** kontrol edilmedi (yardımcının işi).
- **Repo memory (bitiş kapısı 9):** güncellendi.
