# T387 — Kurulum tarafı yönetim komutları: iş satırını yeniden kuyruğa alma ya da kapatma, anahtar yenilemesi sonrası ödeme sonucunun uygulanması, kayıt okuma komutu

**Faz:** F3 | **Durum:** ⏳ Devam ediyor — yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-09

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Commands/JobCommand` (`job --id <iş kimliği> --action requeue|close`): yalnız "Başarısız" satır; `requeue` satırı `Pending`'e alır, deneme sayısını ve `NextAttemptAt`'i boşaltır (`09 §13.2` madde 7), `close` `Completed` yapar; satır yoksa ya da "Başarısız" değilse 3. Satır işlemleri `Infrastructure/Jobs/FailedJobRows` (koşullu güncelleme), durum ucunun koşulu `JobQueueStatusReader` ile aynı tanımdan okunur.
- `Commands/ApplyPaymentResultCommand` (`apply-payment-result --order-number --result paid|none [--amount]`): hedef `IOrderingModule.FindInstallerPaymentTargetAsync` (işaret tek kural evi `PaymentWindowRules.IsPaymentOutcomeUnknown`); kart değil ya da işaretsizse 3; `paid` tutarı `PaymentAmounts.ToKurus(Order.Total)`'a eşit değilse 3 ve yazma yok; `paid` → `IOrderTransitions.ApplyCardPaymentAsync` (Ö1, B-4), `none` → `CancelOrderAsync(BySystem, PaymentWindowExpired)` (S3 + Ö2, B-7). Komut sağlayıcı çağırmaz, sır istemez, e-posta göndermez, iş kuyruğunu çalıştırmaz.
- `Commands/ReadLogsCommand` (`read-logs --source login|system|disposal --from --to [--filter]`): süzgeç `event:`, `request:`, `account:`, değersiz `ip` ve `email`; IP ve e-posta değeri yalnız terminalden; yönlendirilmiş girdide satır yazmadan 2; e-posta → yönetici, sonra müşteri iç kimliği. Satır (`AuditLine.ReadLogs`) okumadan önce yazılır ve `Flush(flushToDisk: true)` ile diske iner. Sonuç `LogExportFile` ile geçici dizinde CSV (`LogCsv`); yarım dosya silinir.
- `Infrastructure/LogRead/` (`LogReader`, `LogSource`, `LogFilterKind`, `LogCsv`, `LogExportFile`) — yeni klasör, `CONTEXT.md §3`'e satır eklendi.
- `AdminCommandCatalog` `05 §8.5` sırasıyla dokuz komut; `CommandHost`'a `FailedJobRows`, `LogDirectories`, `LogReader`; `CommandDispatcher` tarih (ISO gün) ve tutar (noktalı ondalık, tam kuruş) seçenekleri.
- `deploy/compose.yaml`: `command` → `app-logs`; `read-command` → `app-logs`, `access-logs:ro`, dışa aktarma dizini.
- `DEPLOY_RUNBOOK §M` (M.1 read-logs + şifreli iletim + silme, M.2 job, M.3 apply-payment-result).
- Backlog: D-540 ✅ (T303 parçası); D-538/539/541 (T303 parçası), D-598/599/602/633 (T389 parçası) hedef "F3 faz kapısı"; yeni kalemler `DEFERRED_BACKLOG.d/T387.md` D-738–D-742.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Commands/` (`JobCommand`, `ApplyPaymentResultCommand`, `ReadLogsCommand`, `AdminCommandCatalog`, `CommandHost`, `CommandDispatcher`, `CommandContracts`, `StatusCommand`)
- `src/Shopfolio.Web/Infrastructure/Jobs/` (`FailedJobRows`, `JobQueueStatusReader`), `Infrastructure/LogRead/`, `Infrastructure/Logging/` (`AuditLine`, `AuditLogWriter`), `Infrastructure/Integrations/PaymentAmounts`
- `Modules/Ordering` (`IOrderingModule.FindInstallerPaymentTargetAsync`), `Modules/Payment/Contracts/CardPaymentReference`
- `deploy/compose.yaml`, `Docs/DEPLOY_RUNBOOK.md`, `.claude/CONTEXT.md`
- Testler: `tests/Shopfolio.UnitTests/Commands/SetupCommandsTests.cs`, `tests/Shopfolio.IntegrationTests/Commands/AccountAndStatusCommandTests.SetupCommands.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `job` "Başarısız" satırı yeniden kuyruğa alır ya da kapatır; koşul ancak böyle kalkar | ✓ | `JobCommand.cs`, `FailedJobRows.cs`; Integration `Job_FailedRow_IsRequeuedOrClosed_AndTheJobQueueConditionClears` (iki eylem), `Job_RowThatIsNotFailed_OrIsGone_IsRefusedWithThree` |
| 2 | `read-logs` üç kaynağı kaynak, aralık ve süzgeçle okur | ✓ | `LogReading.cs`; Integration `ReadLogs_System_ByEvent_...`, `ReadLogs_Login_ByEvent_AndDisposal_ByRange_...`; Unit `Filter_OfEachSource_IsRead` |
| 3 | IP ve e-posta argümanla verilmez; terminalden okunur, e-posta iç kimliğe çevrilir | ✓ | Unit `ReadLogs_IpOrEmailAsAnArgument_IsAUsageError_...`, `Filter_ThatCarriesPersonalData_...`, `ReadLogs_IpAndEmailFilters_HaveNoValueInTheOption_...`; Integration `ReadLogs_Login_ByEmailFromTheTerminal_ReadsTheNormalizedAddress_...` |
| 4 | Sonuç CSV; kayıt ve yedek dışı geçici dizin; şifreli iletim ve silme `DEPLOY_RUNBOOK` adımı | ✓ | `LogExportFile.cs`, `LogCsv`; `compose.yaml` `read-command`; `DEPLOY_RUNBOOK §M.1` adım 1, 5, 6; Unit `Csv_HasSemicolons_CrLf_Quotes_AndNeutralizesFormulas` |
| 5 | Her çalışmada satır okumadan önce yazılır ve diske iner | ✓ | `ReadLogsCommand.cs` (satır → okuma), `AuditLine.ReadLogs` `FlushToDisk`; Unit `ReadLogsLine_CarriesTheKindNotTheValue_AndWaitsForTheDisk`; Integration `ReadLogs_System_ByEvent_FindsItsOwnLine_WrittenBeforeTheRead_...` |
| 6 | Üç kaynak panelde görünmez | ✓ | Unit `Panel_HasNoScreenOfTheThreeSources` + ihlal örneği `Scans_OnViolationSamples_ReportEachForm` |
| 7 | `apply-payment-result` yalnız işaretli kart siparişinde, başkasını reddeder | ✓ | Integration `ApplyPaymentResult_UnmarkedOrder_AndAnAmountThatIsNotTheTotal_AreRefusedWithThree_AndWriteNothing` (işaretsiz kart, işaretli havale, olmayan numara) |
| 8 | Numara + sonuç, kayda yazar; eşit tutarda başarı, yok/başarısızda başarısızlık tek yoldan; tutmazsa 3 | ✓ | Aynı test (119.99 ≠ 120 → 3, anlık görüntü değişmez); `ApplyPaymentResult_Paid_WithTheTotal_...` (Ö1+S1, B-4 satırı adıyla); `ApplyPaymentResult_None_CancelsTheOrder_...` (Ö2+S3, B-7 satırı adıyla); satır `AuditLine.AdminCommandWithoutTarget` |
| 9 | Sağlayıcı, sır, e-posta, iş kuyruğu yok; B-4/B-7 çalışan uygulamanın kuyruğundan; `--help` = `05 §8.5` | ✓ | Unit `CommandsFolder_NamesNoProviderGateway_NoEmailGateway_AndNoJobRunner` + ihlal örnekleri; Integration B-4/B-7 testlerinde `jobs` satırları `Pending` (komut koşmadı); Unit `HelpList_And_The05Table_AreEqualBothWays_InTheTablesOrder` |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi (2. devam) bütün maddeleri diff'te yeniden denetledi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: iş satırı iki eylemle, koşul kalkar | ✓ | `Job_FailedRow_IsRequeuedOrClosed_AndTheJobQueueConditionClears` (`[Theory]` requeue/close); requeue'da koşul iş koşunca kalkar (⚠ b, D-742) |
| KK2: kaynak, aralık ve süzgeç çalışır | ✓ | `ReadLogs_System_ByEvent_...` (başka gün ve olay dışarıda), `ReadLogs_Login_ByEvent_AndDisposal_ByRange_...`, `ReadLogs_FromAfterTo_IsAUsageError_AndWritesNoLine`, `ReadLogs_UnknownSource_IsAUsageError` |
| KK3: IP ve e-posta argümanı reddedilir; `stdin` boruda satır yazmadan 2 | ✓ | Unit `ReadLogs_RedirectedInput_WithAnIpOrEmailFilter_ExitsWithTwo_ReadsNothing_AndWritesNoLine`; Integration gerçek süreç `ReadLogs_RealProcess_PipedInput_WithAnIpFilter_ExitsWithTwo_AndWritesNoLine` |
| KK4: yedekte değil; şifreli iletim ve silme `DEPLOY_RUNBOOK`'ta | ✓ | `DEPLOY_RUNBOOK §M.1` adım 1 (dizin §J yedek ve §L.3 listesinde değil), 5 (7z/age), 6 (silme) |
| KK5: kayıt satırı okumadan önce yazılır | ✓ | `ReadLogsLine_CarriesTheKindNotTheValue_AndWaitsForTheDisk`; Integration okuma kendi satırını buluyor |
| KK6: panelde üç kaynağın ekranı yok | ✓ | `Panel_HasNoScreenOfTheThreeSources`, ihlal örneği `Scans_OnViolationSamples_ReportEachForm` |
| KK7: işaretsiz sipariş reddedilir | ✓ | `ApplyPaymentResult_UnmarkedOrder_...` |
| KK8: tutmayan tutarda 3; başarıda B-4, başarısızlıkta iptal ve B-7 | ✓ | `ApplyPaymentResult_Paid_...` `dispatch_type = B4`; `ApplyPaymentResult_None_...` `order_status = Cancelled`, `dispatch_type = B7` (bu turda zayıf "satır > 0" denetimi adıyla denetime çevrildi) |
| KK9: sağlayıcı çağrısı ve e-posta yok; `--help` tabloyla eşit | ✓ | `CommandsFolder_NamesNoProviderGateway_...`, `HelpList_And_The05Table_AreEqualBothWays_...` (iki yönlü, sıra dahil) |
| `09 §25` doküman uyumu | ✓ | Adlar 05 §8.5 tablosu ve 06 §5 değerleriyle (`JobState`, `DispatchType`); kapsam dışı değişiklik yok; yeni ayar yok (`SHOPFOLIO_EXPORT_DIR` yalnız compose'un sunucu bağlama yolu, uygulama ayarı değil) |
| `09 §25` mimari | ✓ | Komutlar yalnız `Contracts` arayüzlerini çağırır; yeni klasör `CONTEXT.md §3`'te; mimari testler (Architecture ad alanı) yeşil; tutar yalnız `PaymentAmounts.ToKurus`; zaman `TimeProvider`'dan |
| `09 §25` durum/olay | ✓ | Durum değişikliği yalnız geçiş birimiyle (`IOrderTransitions`); iş satırı koşullu güncellemeyle; yönetim komutu tabloyla bire bir |
| `09 §25` veri/göç | ✓ | Göç yok, şema değişikliği yok |
| `09 §25` test | ✓ | Yeni tarama kuralları ihlal örnekleriyle kırmızı (`Scans_OnViolationSamples_ReportEachForm`); birim testi dosya sistemine yalnız geçici dizinde yazar ve siler; veritabanı davranışı gerçek PostgreSQL'de |
| `09 §25` performans | ✓ | Kayıt okuma satır satır akışla; giriş kaydı sorgusu izlemesiz, aralık süzgeci veritabanında |
| Güvenlik taraması | ✓ | Sır yok; kayıt satırında süzgeç değeri, IP ve e-posta yok (`ReadLogsLine_HasNoParameterForAFilterValue`); hata iletisinde girilen kişisel değer yinelenmez (`..._AndDoesNotRepeatIt`); CSV formül etkisizleştirme; dışa aktarma dosyası 0600, dizin 0700; okuma `read-command` (okuyucu kullanıcı) ile |
| Sık FAIL #51 (kurucu/servis değişti, Integration TAM) | ✓ | Integration tam koşu (aşağıda) |
| Sık FAIL #52 (format) | ✓ | `dotnet format --verify-no-changes` üç projede tam, temiz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 5355/5355 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build` (postgres:18) | 3085 toplam: 3077 ✓ + 8 ortam kaynaklı (yerel konteynerde `track_commit_timestamp` kapalıydı; CI açar) → `-c track_commit_timestamp=on` ile `RestoreCommandsTests` + `SchedulerBackupDisposalAndKeyTablesTests` 67/67 ✓ |
| Contract | — | — | Bu task'ta sözleşme değişikliği yok |
| Lint | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (Web, UnitTests, IntegrationTests) | Temiz (bir boşluk düzeltildi) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulama doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — compose'da `SHOPFOLIO_EXPORT_DIR` (sunucudaki dışa aktarma dizini, varsayılan `/var/tmp/shopfolio-export`; `DEPLOY_RUNBOOK §M.1`)
- **Paketleme / dağıtım:** Var — `command` ve `read-command` hizmetlerine birim bağlamaları
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T387-kurulum-komutlari`
- **Commit:** *(PR head)* — T387: Kurulum tarafı yönetim komutları
- **PR:** #487
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok: komut sağlayıcıyı çağırmaz, sonuç kurulumcunun panelde okuduğu değerdir | `ApplyPaymentResultCommand.cs`; `CommandsFolder_NamesNoProviderGateway_...` | ✓ |
| 2 | CI PostgreSQL'i `track_commit_timestamp = on` ile koşar (yerel 8 hatanın nedeni) | `.github/workflows/ci.yml` "PostgreSQL kesinlesme anini tutsun" adımı | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sunucunun kendi kayıtları (`journald`, DB sunucu kaydı) `read-logs`'ta yok | D-738 | Komut konteynerine bağlanmadı; §L.4 araçları |
| 2 | `LogCsv` ortak CSV yazıcısından ayrı | D-739 | Ortak yazıcı henüz yok |
| 3 | `read-logs` satırı süzgecin değerini yazmıyor | D-740 | Faz kapısı kararı |
| 4 | `PaymentAmounts` yalnız `ToKurus` | D-741 | Öteki işlevler sağlayıcı task'ında (T403) |
| 5 | Requeue sonrası koşul iş koşana dek açık | D-742 | `09 §13.2` m.7 gereği; gözlem |

## Notlar

- **Working tree:** temiz (devam işçisi kendi worktree'sinde; dal `origin/task/T387-kurulum-komutlari` head `64543f51` üzerinden; `origin/main` birleştirildi, çakışma yok).
- **Hook kurulumu:** `core.hooksPath` = `scripts/git-hooks` ✓.
- **Ana dal CI (devam başlangıcı):** 42548005029 success, 42548005043 success, 42546628955 success.
- Yapım üç işçide yürüdü (ilk ikisi bağlam ve süre sınırında bölündü; devir notları yardımcının durum dizininde).

### ⚠ Kolay kararlar (kurala en uygun seçenekle verildi)

- (a) `LogReading.cs` ve `LogExportFile.cs` `Infrastructure/LogRead/`'de: `Logging/` klasöründe kayıt dosyası silen kod `SystemLogsScan` ile yasak; yarım CSV'yi silen kod oraya konamazdı. `CONTEXT.md §3` güncel.
- (b) `requeue` sonrası iş kuyruğu koşulu, çalışan uygulama satırı alıp koşana dek açık kalır (`09 §13.2` m.7: `NextAttemptAt` boşaltılır, satırın yaşı oluşturulma anı); `close` hemen kaldırır. Test runner'ın işini elle yaparak koşulun kalktığını gösterir (D-742).
- (c) `read-logs` satırında süzgecin değeri yok, yalnız türü ve hesabın iç kimliği (D-740).
- (d) `PaymentAmounts` yalnız `ToKurus` ile geldi (D-741).
- (e) E-posta süzgecinde sıra: önce yönetici, sonra müşteri hesabı.
- (f) Terminalden girilen geçersiz IP 3 ile biter (kullanım hatası 2 argümanlar içindir; terminal değeri reddi 3).
