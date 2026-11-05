# T220 — İş kuyruğu çalıştırıcısı

**Faz:** F2 | **Durum:** ✓ Tamamlandı (yapım; doğrulama bekliyor) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Jobs/`: `JobRunnerService` (tek `BackgroundService`; tek çalışma oturum kilidi, sinyal ya da beş saniye, satırları birer birer işler, bir satırın hatası döngüyü durdurmaz), `JobQueue` (`IJobQueue.EnqueueAsync`; `INSERT … ON CONFLICT DO NOTHING`, çağıranın işleminde), `JobEnqueueTracker` + `TransactionRunner` (kesinleşince tek uyandırma, geri alınanda yok), `JobSignal` (kapasitesi bir kanal), `JobClaim` (`FOR UPDATE SKIP LOCKED` ile satır alma), `JobRetryPolicy` (varsayılan kural 1/5/15/60/240 dk; altıncı hata `Failed`), `JobOutcome`, `JobCleanup` (yalnız tamamlananı siler), `JobTimings` (tek sabit yeri), `JobQueueRegistration` (`AddJobQueue`, `AddJobHandler`, `AddTransactionalJobHandler`, `AddJobStartGate`).
- `Payment/CardRefundJobRule`: kart iadesi kararı (kayıt yok → gönder; "gönderiliyor" ya da sonuç bekleniyor → sonucu sor; sonuçlanmış → hiçbir şey).
- `AdvisoryLocks.TryHoldAsync` (oturum kapsamlı, ayrı bağlantı); koparsa `DisposeAsync` susar (NpgsqlException ve InvalidOperationException).
- D-96 K1: `IJobStartGate` + `RevertLinkDigestRewriteGate` (`HasRun`); veritabanı açılıştan sonra gelirse adım bir kez koşar, kuyruk ancak sonra satır alır.
- Kaynak taramaları: `JobSourceScan` (5 kural, ayrı ihlal örnekleri), `ConcurrencySourceScan` JobClaim.cs izni (yalnız düz `FOR UPDATE`).
- Belgeler: `09 §13.1` m.2, m.3, m.7, m.10; `§10.2` m.6; `§18.1` m.3 (`JobTypeUnknown`); `§25` arka plan satırı; `CONTEXT.md`; backlog D-96/D-97.

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
| Doğrulama durumu | bekliyor |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`jobs` tablosu önceden vardı)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T220-is-kuyrugu`
- **PR:** #PRNO

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
