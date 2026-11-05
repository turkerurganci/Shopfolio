# T225 — Sistem kayıtları

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Ters vekil (`deploy/caddy/Caddyfile`): istek kimliğini Caddy üretir (`header_up X-Request-Id {http.request.uuid}`; istemcinin gönderdiği başlığın yerine geçer) ve erişim kaydı satırına aynı değeri yazar (`log_append request_id`). Caddy'nin kendi döndürmesi kapalı (`roll_disabled`; eski `roll_size`/`roll_keep` kayıt dosyası silerdi).
- `deploy/compose.yaml`: `x-container-log` (json-file, 10m, 3 dosya); yedi servisin hepsi alır.
- Veritabanı: `deploy/images/database/postgresql.conf.d/30-logging.conf` (toplayıcı açık, tek dosya, döndürme ve silme yok, `log_error_verbosity = terse`, parametre uzunluğu 0, `log_statement = none`); Dockerfile'da `/var/log/postgresql/server` (2750, `logread` grubu).
- `scripts/ci/package-build.sh` adım 6: ayar denetimleri ve "kayıtta parola yok / 3 SCRAM satırı" denetimi konteyner çıktısına ek olarak kayıt dosyasını okur; dizin izni denetimi.
- `Docs/DEPLOY_RUNBOOK.md` §L (+ hızlı özet): kaynak/yer/silen tablosu, istek kimliği, `logrotate` (erişim kaydı silinmez, sunucu kayıtları 183 gün), `journald` `MaxRetentionSec=6month`, L.5 yedi maddelik kurulum denetimi.
- `Docs/09` §18.1 "Yasak" (ayrı metrik toplama ve pano yok) ve §18.2 madde 7 (T225 testlerinin adları); `IMPLEMENTATION_STATUS` T225 satırı; `MEMORY.md`.
- Testler: Unit `LogFileDeletionSourceScan`, `MetricsPackageScan`, `SystemLogsScanTests`, `ViolationSamples/Logging/LogFileDeletion.cs`, `SystemLogsDeployConsistencyTests`; Integration `Logging/SystemLogsTests`.

## Etkilenen Modüller / Dosyalar

`deploy/caddy/Caddyfile`, `deploy/compose.yaml`, `deploy/images/database/*`, `scripts/ci/package-build.sh`, `Docs/DEPLOY_RUNBOOK.md`, `Docs/09_CODING_GUIDELINES.md` (§18), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`, `tests/Shopfolio.UnitTests/{Architecture,Consistency}/*`, `tests/Shopfolio.IntegrationTests/Logging/SystemLogsTests.cs`. Uygulama kodu (`src/`) değişmedi.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç kaynak, günlük dosyalar; erişim kaydını vekil, sunucu kayıtlarını OS/konteyner çalıştırıcısı/veritabanı/yedek aracı yazar; ayarlar runbook'ta, kurulumda denetlenir | ✓ | `SystemLogsDeployConsistencyTests.AccessLog_TheApplicationLogAndTheDatabaseLog_AreInThreeDifferentVolumesAndDirectories`, `EveryComposeService_TakesTheSmallContainerOutputRotation`, `DatabaseImage_WritesItsLogToAFileWithoutParametersAndWithoutDetail`; runbook §L ve L.5 |
| 2 | İstek kimliğini vekil üretir, `X-Request-Id` ile iletir; uygulama satırları başlıktaki değeri taşır | ✓ | `Caddyfile_HandsTheRequestIdToTheApplicationInTheHeaderTheApplicationReads`, `Caddyfile_WritesTheSameValueInItsAccessLineAndHandsItOn`; `SystemLogsTests.ErrorRequest_…`, `RejectedRequest_…`, `UnauthorizedRequest_…` |
| 3 | Yasaklı değerler hiçbir kayda yazılmaz | ✓ | `SystemLogsTests.SampleLines_ContainNoFormBodyCookieAddressEmailOrPassword`; `Caddyfile_DoesNotWriteRequestHeadersInTheAccessLine`; `30-logging.conf` (terse, parametre 0); `package-build.sh` adım 6 parola denetimi |
| 4 | Düzey tasarım değeri; çatı ve veri erişimi uyarıda | ✓ | `SystemLogsTests.LevelSetting_ChangesTheApplicationCategoryAndNeverTheFrameworkOrDataAccessCategory` (6 düzey) |
| 5 | Dosyayı yalnız imha işi siler; sunucu kayıtları P-45'e ayarlı kendi döngüsüyle | ✓ | `Caddyfile_HasItsOwnRollingOff_…`, `DatabaseImage_LeavesRotationAndDeletionToTheOperatingSystem`, `Runbook_KeepsTheAccessLogForTheDisposalJobAndDeletesTheServersLogsAfterSixMonths`; `NormalWriter_WhenItStartsANewDaysFile_…`, `Restart_KeepsEveryFileOfTheDirectory_…` |
| 6 | İmha işi ve imha satırı T201'de; bu task yazmaz | ✓ | `SystemLogsScanTests.LoggingFolder_DeletesNoLogFileAndWritesNoDisposalRecord` |
| 7 | Ayrı metrik toplama ve pano yok | ✓ | `SystemLogsScanTests.DependenciesAndDeploymentFiles_NameNoMetricCollectionOrDashboard`; `09 §18.1` "Yasak" |

### Öz denetim (bitiş kapısı 2)

`git diff origin/main...HEAD` üzerinde, önceki işçinin işi dahil denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–7 | ✓ | yukarıdaki tablo |
| KK1 uygulama kaydı erişim ve sunucu kayıtlarından ayrı günlük dosyada | ✓ | `AccessLog_TheApplicationLogAndTheDatabaseLog_AreInThreeDifferentVolumesAndDirectories`; vekil ve sunucu kayıtları kurulumda: runbook L.5 |
| KK2 aynı istek aynı kimlik; 500/422/401 istek alanlarıyla satır; `requestId` gelen değer | ✓ | `ErrorRequest_…`, `RejectedRequest_…`, `UnauthorizedRequest_…`, `TwoRequests_KeepTheirOwnIds_…` (başlık değeri beklenen değer) |
| KK3 örnek satırlarda yasaklı alan yok | ✓ | `SampleLines_ContainNoFormBodyCookieAddressEmailOrPassword` |
| KK4 düzey ayarı uygulamayı değiştirir, çatıyı/veri erişimini değiştirmez | ✓ | `LevelSetting_…` |
| KK5 gün dönümü ve yeniden başlatmada dosyalar kalır | ✓ | `NormalWriter_WhenItStartsANewDaysFile_KeepsTheFilesOfTheEarlierDays_…`, `AuditAndCommandWriters_AtTheTurnOfTheTurkeyDay_KeepTheOldFile`, `Restart_KeepsEveryFileOfTheDirectory_…` |
| KK6 blokta dosya silen kod ve imha satırı yok | ✓ | `LogFileDeletionSourceScan` (silme, ezme, açık kütüphane sınırı, imha satırı); ihlal örneği `ViolationSamples/Logging/LogFileDeletion.cs` her biçimi kırmızı yapar |
| KK7 metrik paketi yok | ✓ | `MetricsPackageScan` (paket bildirimleri, kilit dosyası, compose ve Dockerfile'lar); her yasaklı ad için örnek satır testi |
| `09 §25` ilgili satır (repoda/imajda/kayıtta sır ve kişisel veri yok) | ✓ | KK3 testi; `30-logging.conf`; `package-build.sh` adım 6 |
| Güvenlik taraması | ✓ | istemci başlığı değiştirilir (`header_up` öneksiz); vekil kayıt satırı başlık yazmaz; veritabanı kaydı parametresiz ve ayrıntısız; yeni sır, uç nokta, `src/` kodu yok |
| Sık FAIL (1) beklenen değer sabitten | ✓ | testler başlık değerini ve `LoggingOptions`/Compose/Dockerfile değerlerini dosyadan okur; düzey testi `LogEventLevel` değerlerinden |
| Sık FAIL (2) öteki yerler | ✓ | `roll_size`/`roll_keep`/`50MiB` tüm depoda arandı: kalan yok; runbook, `09 §18`, IMPLEMENTATION_STATUS, MEMORY güncel; `SystemLogsDeployConsistencyTests` Caddyfile/compose/imaj/runbook eşitliğini sınar |
| Sık FAIL (3) tarama istisnası geniş değil | ✓ | `LogFileDeletionSourceScan.AppliesTo` yalnız `Infrastructure/Logging/`; `LogFileDeletionScan_AppliesToTheLoggingFolderOnly`; yalnız yorum satırı ve blok yorum atlanır |
| Sık FAIL (4) ihlal örneği; birim testi dosya sistemine yazmıyor | ✓ | ihlal örneği var; dosya yazan testler Integration'da |
| Sık FAIL (5) kayıt satırı maskelemesiz ikinci çıktıya inmiyor | ✓ | T230 `writeToProviders: false`; konteyner çıktısı yalnız açılış/çökme (compose `x-container-log`, veritabanı kaydı dosyada ve parametresiz) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 680/680 |
| Integration | ✓ | PostgreSQL 18 (Docker, `shopfolio-t225-pg`), ad alanına göre: Logging, Database+FileSecrets+Model, Web | 29 + 952 + 54 = 1035, hepsi yeşil |
| Diğer | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format --verify-no-changes`; `tests/scripts/run-script-tests.sh`; tam `scripts/ci/package-build.sh` (yerel) | 0 uyarı; temiz; 4 betik dosyası; "Package build: all checks passed." |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulayıcı doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (veritabanı ayarı imaj içinde `30-logging.conf`)
- **Paketleme / dağıtım:** Var — Caddyfile, compose `logging`, veritabanı imajı kayıt dizini; çalışan kurulumda `/var/log/postgresql/server` bir kez elle oluşturulur (runbook L.5 madde 2)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T225-sistem-kayitlari`
- **Commit / PR / CI:** PR başlığında (`T225: Sistem kayıtları`); CI yapım işçisince beklenmez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Caddy 2.11.7 `{http.request.uuid}` yer tutucusu ve `header_up` başlığı değiştirir | `caddy validate`/`adapt` geçti | söz dizimi doğrulandı; davranış gerçek sunucuda T603 |
| 2 | `roll_disabled` Caddy 2.11.7'de geçerli | `caddy validate` | doğrulandı |
| 3 | PostgreSQL 18 `log_*` ayarları ve dosya modu | imaj çalıştırıldı: dosya 0640 postgres:logread; `package-build.sh` adım 6 ayarları okur | doğrulandı |
| 4 | `logrotate` ve `journald` düzeni | bilgim tarihli | runbook L.5 kurulum denetimi; gerçek sunucuda T603 |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gerçek istisna işleyicisi ve red satırı yazan çağrı (500/422/401 test hattında üretiliyor) | T213 | planda ayrı task |
| 2 | `LogFileDeletionSourceScan` yalnız `Infrastructure/Logging/`; T201 imha işi başka klasörde | T201 | imha işi o task'ta |
| 3 | `logrotate`/Caddy uuid davranışının gerçek sunucuda sınanması | T603 | sunucu gerekir |

## Notlar

- **Gün dönümü:** normal yazıcı sunucu saatine bağlı, saat oynatılamıyor; test yeni gün dosyasında eskilerin silinmediğini gösterir; denetim/komut yazıcıları sahte saatle sınanır.
- **`log_min_error_statement`** varsayılan: `STATEMENT:` satırı kayda girebilir (EF parametreli, değer sızmaz); gerekirse D- kaydı.
- **Yerel paketleme sınaması:** Git Bash'te `mktemp` dizini Docker'a bağlanamadığı için betik `TMPDIR` Windows yoluna alınarak koşuldu (betik değişmedi).
- **Repo memory (bitiş kapısı 9):** güncellendi
