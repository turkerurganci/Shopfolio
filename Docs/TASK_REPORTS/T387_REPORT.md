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
| 2 | `read-logs` üç kaynağı kaynak, aralık ve süzgeçle okur | ✓ | `LogReading.cs`, `ServerLogs.cs` (düzeltme 1: sistem kayıtlarının üç kaynağı — uygulama kaydı, erişim kaydı ve sunucunun kayıtları: veritabanı, pgBackRest, yedek aracı, işletim sisteminin günlüğü; `compose.yaml` `read-command` `db-logs`/`backup-logs`/`/var/log/journal` `:ro`, `group_add: 10002`); Integration `ReadLogs_System_ByEvent_...`, `ReadLogs_System_ReadsTheServersLogs_TheDatabasesTheBackupToolsAndTheJournal_ByRangeAndFilter` (aralık dışı, zamansız ve sıkıştırılmış satır dışarıda; `ip` ve `event:sshd` süzgeci; okunan dosya değişmez), `ReadLogs_Login_ByEvent_AndDisposal_ByRange_...`; Unit `Filter_OfEachSource_IsRead`, `ServerTextLine_ReadsTheDatabasesPgBackRestsAndTheBackupToolsTimes_...`, `ServerTextLine_MatchesTheFilterValueAsAWholeToken` |
| 3 | IP ve e-posta argümanla verilmez; terminalden okunur, e-posta iç kimliğe çevrilir | ✓ | Unit `ReadLogs_IpOrEmailAsAnArgument_IsAUsageError_...`, `Filter_ThatCarriesPersonalData_...`, `ReadLogs_IpAndEmailFilters_HaveNoValueInTheOption_...`; Integration `ReadLogs_Login_ByEmailFromTheTerminal_ReadsTheNormalizedAddress_...` |
| 4 | Sonuç CSV; kayıt ve yedek dışı geçici dizin; şifreli iletim ve silme `DEPLOY_RUNBOOK` adımı | ✓ | `LogExportFile.cs`, `LogCsv`; `compose.yaml` `read-command`; `DEPLOY_RUNBOOK §M.1` adım 1, 5, 6; Unit `Csv_HasSemicolons_CrLf_Quotes_AndNeutralizesFormulas` |
| 5 | Her çalışmada satır okumadan önce yazılır ve diske iner | ✓ | `ReadLogsCommand.cs` (satır → okuma), `AuditLine.ReadLogs` `FlushToDisk`; Unit `ReadLogsLine_CarriesTheKindNotTheValue_AndWaitsForTheDisk`; Integration `ReadLogs_System_ByEvent_FindsItsOwnLine_WrittenBeforeTheRead_...` |
| 6 | Üç kaynak panelde görünmez | ✓ | Unit `Panel_HasNoScreenOfTheThreeSources` + ihlal örneği `Scans_OnViolationSamples_ReportEachForm` |
| 7 | `apply-payment-result` yalnız işaretli kart siparişinde, başkasını reddeder | ✓ | Integration `ApplyPaymentResult_UnmarkedOrder_AndAnAmountThatIsNotTheTotal_AreRefusedWithThree_AndWriteNothing` (işaretsiz kart, işaretli havale, olmayan numara) |
| 8 | Numara + sonuç, kayda yazar; eşit tutarda başarı, yok/başarısızda başarısızlık tek yoldan; tutmazsa 3 | ✓ | Aynı test (119.99 ≠ 120 → 3, anlık görüntü değişmez); `ApplyPaymentResult_Paid_WithTheTotal_...` (Ö1+S1, B-4 satırı adıyla); `ApplyPaymentResult_None_CancelsTheOrder_...` (Ö2+S3, B-7 satırı adıyla); düzeltme 1: satır `AuditLine.PaymentResult` — siparişin iç kimliği (`OrderId`; numarası olmayan siparişte boş kimlik), `PaymentResult` (`Paid`/`None`) ve `AmountInKurus` kapalı değerlerle, salt okuma aramasından sonra, ret ve işlemden önce, diske inmesi beklenerek; Unit `PaymentResultLine_CarriesTheOrdersIdAndTheResultAsClosedValues_AndWaitsForTheDisk`; Integration üç testte satır içeriği (sipariş kimliği, sonuç, 12000 kuruş; sipariş numarası satırda yok) |
| 9 | Sağlayıcı, sır, e-posta, iş kuyruğu yok; B-4/B-7 çalışan uygulamanın kuyruğundan; `--help` = `05 §8.5` | ✓ | Unit `CommandsFolder_NamesNoProviderGateway_NoEmailGateway_AndNoJobRunner` + ihlal örnekleri; Integration B-4/B-7 testlerinde `jobs` satırları `Pending` (komut koşmadı); Unit `HelpList_And_The05Table_AreEqualBothWays_InTheTablesOrder` |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi (2. devam) bütün maddeleri diff'te yeniden denetledi. Düzeltme 1 işçisi bütün kriterleri ve KK'ları düzeltilmiş diff'te yeniden denetledi (KK2, KK8 kanıtı güncellendi; öteki satırlar değişmedi ve testleri yeniden koşuldu).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: iş satırı iki eylemle, koşul kalkar | ✓ | `Job_FailedRow_IsRequeuedOrClosed_AndTheJobQueueConditionClears` (`[Theory]` requeue/close); requeue'da koşul iş koşunca kalkar (⚠ b, D-742) |
| KK2: kaynak, aralık ve süzgeç çalışır | ✓ | `ReadLogs_System_ByEvent_...` (başka gün ve olay dışarıda), düzeltme 1: `ReadLogs_System_ReadsTheServersLogs_...` (sunucunun dört dosya türü + günlüğün düz ve sıkı biçimi; satır 7, okunamayan 2; `ip` 2 satır, `event:sshd` 1 satır), `ReadLogs_Login_ByEvent_AndDisposal_ByRange_...`, `ReadLogs_FromAfterTo_IsAUsageError_AndWritesNoLine`, `ReadLogs_UnknownSource_IsAUsageError` |
| KK3: IP ve e-posta argümanı reddedilir; `stdin` boruda satır yazmadan 2 | ✓ | Unit `ReadLogs_RedirectedInput_WithAnIpOrEmailFilter_ExitsWithTwo_ReadsNothing_AndWritesNoLine`; Integration gerçek süreç `ReadLogs_RealProcess_PipedInput_WithAnIpFilter_ExitsWithTwo_AndWritesNoLine` |
| KK4: yedekte değil; şifreli iletim ve silme `DEPLOY_RUNBOOK`'ta | ✓ | `DEPLOY_RUNBOOK §M.1` adım 1 (dizin §J yedek ve §L.3 listesinde değil), 5 (7z/age), 6 (silme) |
| KK5: kayıt satırı okumadan önce yazılır | ✓ | `ReadLogsLine_CarriesTheKindNotTheValue_AndWaitsForTheDisk`; Integration okuma kendi satırını buluyor |
| KK6: panelde üç kaynağın ekranı yok | ✓ | `Panel_HasNoScreenOfTheThreeSources`, ihlal örneği `Scans_OnViolationSamples_ReportEachForm` |
| KK7: işaretsiz sipariş reddedilir | ✓ | `ApplyPaymentResult_UnmarkedOrder_...` |
| KK8: tutmayan tutarda 3; başarıda B-4, başarısızlıkta iptal ve B-7 | ✓ | `ApplyPaymentResult_Paid_...` `dispatch_type = B4`; `ApplyPaymentResult_None_...` `order_status = Cancelled`, `dispatch_type = B7` (bu turda zayıf "satır > 0" denetimi adıyla denetime çevrildi). Düzeltme 1 (B2, D-745): kayıt satırının içeriği; `Preparing`, stok 10/1 → 9/0 ve ayırma satırı yok; iptalde 10/1 → 10/0; işaret kalkar (ikinci çalışma 3, anlık görüntü değişmez); işaretsiz kart siparişinde `paid` 3 |
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
| Sık FAIL "kabul kriterinin parçası backlog'a ertelendi" (T387 r1) | ✓ | D-738 kapatıldı; kriter 2 bütünüyle kodda; kalan tek iş gerçek journald dosyasıyla deneme (D-749, T603 — D-40 ile aynı ortam sınırı, kriterin parçası değil) |
| Sık FAIL "hedefi bu task olan backlog kaydı" | ✓ | D-745 (hedef: düzeltme turu) kapatıldı; D-738 kapatıldı |
| Güvenlik (düzeltme 1) | ✓ | Günlük okuyucusu dosyayı yalnız okur (`FileShare.ReadWrite \| Delete`); her uzaklık ve boy dosya boyuna karşı sınanır, nesne 1 MB tavanlı; sıkıştırılmış alan açılmaz; ACL yalnız `/var/log/journal`'a ve yalnız uid 1654'e, sürekli çalışan `app` hizmeti günlüğü bağlamaz; ödeme satırında sipariş numarası ve kişisel veri parametresi yok (`PaymentResultLine_...` yansıma denetimi) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 5355/5355 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build` (postgres:18) | 3085 toplam: 3077 ✓ + 8 ortam kaynaklı (yerel konteynerde `track_commit_timestamp` kapalıydı; CI açar) → `-c track_commit_timestamp=on` ile `RestoreCommandsTests` + `SchedulerBackupDisposalAndKeyTablesTests` 67/67 ✓ |
| Contract | — | — | Bu task'ta sözleşme değişikliği yok |
| Lint | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (Web, UnitTests, IntegrationTests) | Temiz (bir boşluk düzeltildi) |
| Düzeltme 1 — build | ✓ | `dotnet build -c Release -warnaserror` (çözüm) | 0 uyarı, 0 hata |
| Düzeltme 1 — Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` (`origin/main` birleştirmesinden sonra, tutarlılık dahil) | 5403/5403 |
| Düzeltme 1 — Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (postgres:18, `-c track_commit_timestamp=on`; birleşik ağaç) | 3095/3095 (tam koşu) |
| Düzeltme 1 — Lint | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (Web, UnitTests, IntegrationTests) | Temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1) |
| Bulgu sayısı | 2 (B1 ORTA, S1 · B2 ORTA, S1) |
| Düzeltme gerekli mi | Evet — B1, B2 |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-745) |

### Doğrulama Sonucu — tur 1 (validator)

**Head:** `dbb730f4` (origin/main ile güncel; birleştirme gerekmedi) · **PR:** #487 · **Verdict:** ✗ FAIL

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = depo `scripts/git-hooks` (ana checkout'un mutlak yolu); ana dal CI son 3: 42548005029, 42548005043, 42546628955 hepsi `success`.
- **Dal CI:** 42552921022 (`dbb730f4`) ✓ — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E (advisory), CI Gate `pass`. Önceki run 42552896442 (`f02f2b9a`) `cancelled`.
- **Kabul kriterleri (kod + test okundu, testler koşuldu):** 1 ✓ (yalnız `Failed` satır, `JobClaim.LockAsync` kilidi; requeue `Pending`/0/boş `NextAttemptAt`, close `Completed`; koşul testte açık → kapalı) · **2 ~ Kısmi (B1)** · 3 ✓ (`ip`/`email` değersiz; argümanda `@`, `ip:`, IP biçimli değer 2, ileti değeri yinelemez; e-posta yönetici → müşteri iç kimliğine, giriş kaydında normalize adrese) · 4 ✓ (CSV `/tmp/shopfolio-export` ↔ sunucuda `/var/tmp/shopfolio-export`; `backup` hizmetinin birimlerinde yok; 0600; `DEPLOY_RUNBOOK §M.1` 5–6) · 5 ✓ (`AuditLine.ReadLogs` → `Flush(true)`, satır okumadan önce; okuma kendi satırını buluyor) · 6 ✓ (`Pages/` taraması + ihlal örnekleri) · 7 ✓ (işaretsiz kart, işaretli havale, olmayan numara 3; anlık görüntü değişmez) · **8 ~ Kısmi (B2)** · 9 ✓ (`Commands/` taraması; komut kipinde hosted service yok; B-4/B-7 `jobs` satırı `Pending`; `--help` = `05 §8.5` tablosu iki yönlü ve sıralı).
- **KK1–KK9:** KK2 ~ (B1), öteki KK'lar ✓ (kriterlerle aynı kanıt; KK8 tutar ve iki yol ✓, kayıt satırı B2). **`09 §25`:** komutlar yalnız `Contracts` arayüzü; durum yalnız geçiş birimiyle; tutar `PaymentAmounts.ToKurus`; zaman `TimeProvider`; göç ve yeni paket yok; düzenli ifadeler zaman aşımlı. "Kod hiçbir doküman cümlesiyle çelişmiyor mu; çelişki varsa BLOCKED bildirilmiş mi?" sorusu B1'de hayır (D-738 "İyileştirme" diye ertelendi).
- **Task satırları:** kaynak birimleri `03:§1.7`, `05:MK-66`, `MK-72`, `§6.9`, `§8.5`, `§9.3`, `09:§13` — KP birimi yok, adım 2a grep'i satır getirmez. Kaynağı `05 §8.5.12` olan **VAL-C115** (KRİTİK, mock) kabul kriteri 7–8'e eşlendi: işaretsiz ve tutmayan tutar reddi, Ö1+S1+B-4, Ö2+S3+B-7, komutun adıyla satır → `PASS`. Stok ve ayırma etkisi ortak geçiş yolunun kendi testlerinde (K1).
- **Testler (yerel, `postgres:18 -c track_commit_timestamp=on`):** restore 0; build `-warnaserror` 0 uyarı; format Web/UnitTests/IntegrationTests temiz; Unit 5355/5355; betik testleri 4/4; Integration `AccountAndStatusCommandTests` 43/43, `CreateAdminCommandTests` + `RestoreCommandsTests` 26/26.
- **B1 — ORTA, S1 Sapma (kabul kriteri 2, KK2; `05 §9.3.1`, §9.1.1, §9.1.4):** "sistem kayıtları" üç kaynaktır — erişim kaydı, uygulama kaydı ve **sunucunun kayıtları** (işletim sisteminin günlüğü, veritabanının ve pgBackRest'in kaydı, yedek aracının kaydı) — ve `05 §9.3.1` "erişim kaydı ve sunucunun kayıtları komutun konteynerine salt okuma bağlanır" der. `read-command` yalnız `app-logs` ve `access-logs:ro` bağlar; `LogReader.SystemLogFiles` yalnız `app-`/`audit-`/`command-` ve `access.log*` okur. `db-logs`, `backup-logs` ve `/var/log/journal` aynı compose'ta `backup` hizmetine zaten salt okuma bağlı. Yapım bunu D-738'e "İyileştirme" diye erteledi; kabul kriterinin parçası backlog'a yazılarak geçilmez (`12 §6.1`), onaylı kabul edilmiş risk de yok. Düzeltme: sunucunun dosya kayıtları `read-command`'a salt okuma bağlanır ve `system` kaynağı onları okur (biçimi JSON olmayan satırlar zamanıyla), ya da sapma belgede karara bağlanır (BLOCKED/karar kaydı).
- **B2 — ORTA, S1 Sapma (kabul kriteri 8, KK8; `05 §8.5.12` "dayanağı panelin kaydıdır ve uygulama kaydına yazılır"):** kriter "sipariş numarasını ve panelde okunan sonucu alır ve uygulama kaydına yazar" der; komutun satırı `AuditLine.AdminCommandWithoutTarget` — yalnız komut adı ve boş kimlik. Satırdan hangi siparişe hangi sonucun (paid/none, tutar) uygulandığı okunmaz. Düzeltme: satır siparişin iç kimliğini ve sonucu kapalı değerlerle taşır (`AuditLine.ReadLogs` kalıbı; kişisel veri yok), işlemden önce yazılır; testte satır içeriği denetlenir.
- **K1 → D-745:** VAL-C115 testleri ödeme durumunu ve geçiş kodlarını denetler; siparişin Hazırlanıyor oluşu, stoğun kesin düşmesi, iptalde ayırmaların serbest kalması, işaretin kalkması ve işaretsiz siparişte `paid` reddi komut testinde adıyla denetlenmez (ortak yolun kendi testleri kapsar).
- **Güvenlik:** sır yok; kayıt satırında IP, e-posta ve süzgeç değeri yok; ileti girilen değeri yinelemez; CSV formül etkisizleştirme; dosya 0600; okuma okuyucu kullanıcıyla (`default_transaction_read_only`); yetki: komutlar yalnız sunucudan; yeni bağımlılık yok.
- **Yapım raporu karşılaştırması:** 2 uyuşmazlık — kriter 2 ve KK2 ✓ yazılmış, sunucunun kayıtları D-738 ile dışarıda (B1); kriter 8 ✓ yazılmış, kayıt satırı numara ve sonucu taşımıyor (B2). Öz denetim tablosu var ve kanıtlı; öteki satırlar validator bulgularıyla uyumlu.

### Düzeltme 1 (tur 1 bulguları)

- **B1 → düzeltildi:** `read-command` sunucunun kayıtlarını salt okuma bağlar (`db-logs` → `/var/log/shopfolio/server/db`, `backup-logs` → `.../backup`, `/var/log/journal` → `.../journal`; `group_add: 10002` veritabanı kayıt dizinleri için). `LogReader` `--source system`'da üç kaynağı okur: uygulama kaydı ve erişim kaydı (JSON), veritabanı/pgBackRest/yedek aracı metin satırları (`ServerTextLine`: zaman satırın başından, yoksa satırdaki ISO UTC anından, yoksa aynı dosyadaki önceki satırdan; hiçbiri yoksa okunamayan) ve systemd günlük dosyaları (`JournalFile`: belgelenmiş dosya biçimi, düz ve sıkı biçim; sıkıştırılmış alan açılmaz — `DEPLOY_RUNBOOK §L.4` `Compress=no`). Süzgeç: `request`/`account`/`ip` sunucu satırında bütün sözcük; `event:` günlükte program ya da birim adı. `DEPLOY_RUNBOOK §M.1` madde 8 (bağlamalar, uid 1654 ACL'i, sütun, zaman, süzgeç, kurulum denemesi), §L.4 ve §L.5 madde 6. D-738 kapandı; gerçek journald denemesi D-749 (T603).
- **B2 → düzeltildi:** `apply-payment-result` satırı `AuditLine.PaymentResult` — `OrderId`, `PaymentResult`, `AmountInKurus` kapalı değerlerle; salt okuma aramasından sonra, ret ve işlemden önce yazılır, diske inmesi beklenir. Para akışı değişmedi (geçiş yolu ve tutar karşılaştırması aynı).
- **K1 → D-745 kapandı:** beş sonuç komut testinde adıyla (`Preparing`, stok kesin düşüş, iptalde ayırmanın serbest kalması, işaretin kalkması, işaretsiz siparişte `paid` reddi).
- **Birleştirme:** düzeltmeden sonra `origin/main` dala birleştirildi (çakışma yok); build, Unit ve Integration birleşik ağaçta yeniden koşuldu.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — compose'da `SHOPFOLIO_EXPORT_DIR` (sunucudaki dışa aktarma dizini, varsayılan `/var/tmp/shopfolio-export`; `DEPLOY_RUNBOOK §M.1`); düzeltme 1: journald `Compress=no` (`DEPLOY_RUNBOOK §L.4`)
- **Paketleme / dağıtım:** Var — `command` ve `read-command` hizmetlerine birim bağlamaları; düzeltme 1: `read-command` `db-logs`, `backup-logs`, `/var/log/journal` salt okuma ve `group_add: 10002`; sunucuda `/var/log/journal`'a uid 1654 ACL'i (`DEPLOY_RUNBOOK §M.1` madde 8)
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
| 1 | ~~Sunucunun kendi kayıtları `read-logs`'ta yok~~ — düzeltme 1'de kapandı | D-738 ✅ | — |
| 1a | Günlük okuyucusu yalnız sentetik dosyayla sınandı; gerçek journald dosyasıyla deneme | D-749 | Gerçek `systemd-journald` dind'de yok (D-40 ile aynı sınır); T603 |
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
