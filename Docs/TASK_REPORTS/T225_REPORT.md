# T225 — Sistem kayıtları

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (doğrulama tur 1 FAIL) | **Tarih:** 2026-11-05

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
| 3 | Yasaklı değerler hiçbir kayda yazılmaz | ✓ (düzeltme 1) | `SystemLogsTests.SampleLines_ContainNoFormBodyCookieAddressEmailOrPassword`; Caddyfile `request>uri` satır 48, 50, 53, 54 (anahtar bölümü `{key}`, sorgu değerleri `*`); `Caddyfile_UriFilters_GiveWhatPathMaskerGivesForEveryKeyedPathAndQueryShape`, `Caddyfile_MasksTheKeyedPathsOfPathMasker_AndNoOthers`; `30-logging.conf` (terse, parametre 0); `package-build.sh` adım 6 parola denetimi |
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
| KK3 örnek satırlarda yasaklı alan yok (uygulama kaydı ve vekil erişim kaydı) | ✓ | `SampleLines_ContainNoFormBodyCookieAddressEmailOrPassword`; vekil: Caddyfile:48-54 + `Caddyfile_UriFilters_…` (her anahtarlı yol, `//`, çıplak belirteç, büyük harf); gerçek `caddy:2.11.7-alpine` sondası: `/siparis/erisim/GIZLI123/teyit?search=gizli&email=a@b.com` → `/siparis/erisim/{key}/teyit?search=*&email=*`, `Cookie` başlığı satırda yok |
| KK4 düzey ayarı uygulamayı değiştirir, çatıyı/veri erişimini değiştirmez | ✓ | `LevelSetting_…` |
| KK5 gün dönümü ve yeniden başlatmada dosyalar kalır | ✓ | `NormalWriter_WhenItStartsANewDaysFile_KeepsTheFilesOfTheEarlierDays_…`, `AuditAndCommandWriters_AtTheTurnOfTheTurkeyDay_KeepTheOldFile`, `Restart_KeepsEveryFileOfTheDirectory_…` |
| KK6 blokta dosya silen kod ve imha satırı yok | ✓ | `LogFileDeletionSourceScan` (silme, ezme, açık kütüphane sınırı, imha satırı); ihlal örneği `ViolationSamples/Logging/LogFileDeletion.cs` her biçimi kırmızı yapar |
| KK7 metrik paketi yok | ✓ | `MetricsPackageScan` (paket bildirimleri, kilit dosyası, compose ve Dockerfile'lar); her yasaklı ad için örnek satır testi |
| `09 §25` ilgili satır (repoda/imajda/kayıtta sır ve kişisel veri yok) | ✓ | KK3 testi; Caddyfile:48-54 (vekil kaydı maskeli); `30-logging.conf`; `package-build.sh` adım 6 |
| Güvenlik taraması | ✓ | istemci başlığı değiştirilir (`header_up` öneksiz); vekil kaydı yol anahtarını ve sorgu değerlerini yazmaz (Caddyfile:48-54), başlıklardan yalnız `User-Agent` (satır 27); veritabanı kaydı parametresiz ve ayrıntısız; yeni sır, uç nokta, `src/` kodu yok |
| `05 §9.1.2` alan listesi belgeyle birebir (erişim kaydı tarayıcı tanıtımı) | ✓ | Caddyfile:27 `log_append user_agent {http.request.header.User-Agent}`; `Caddyfile_WritesTheBrowserIdentificationAsItsOwnFieldAndNoOtherHeader` yalnız `request_id` ve `user_agent` eklendiğini sınar; runbook L.2 (satır 473), L.5 madde 3 |
| `DEFERRED_BACKLOG` `grep -n "T225"`: hedefi bu task olan kayıtlar | ✓ | D-33 (satır 88): maske, `User-Agent`, istek kimliği, günlük düzen kapandı; KALAN yalnız dışa aktarma `response_header_timeout` → T384, hedef sütunu T384; D-89 K3 (satır 144) ✅ çözüldü, hedef sütunundan T225 çıkarıldı; D-91 (satır 145) K-notları, dokunulmadı |
| Öteki yerler: aynı olguyu anan runbook, 09, STATUS, MEMORY | ✓ | `DEPLOY_RUNBOOK` satır 31 (K-915 özeti), L.2 (473-474), L.5 madde 3; `09 §18.2` madde 2b (satır 1979) ve madde 7 (1992); `IMPLEMENTATION_STATUS` T225 satırı; `MEMORY.md` satır 22 |
| Sık FAIL: tarama/süzgeç istisnası geniş değil | ✓ | boş bölüm ve ön ek yalnız kapalı liste için; `/siparis/erisim-x/…`, `/panel/…` (listede olmayan), `/gorsel/…` örnekleri değişmeden geçer (`Caddyfile_UriFilters_…` örnek listesi) |
| Sık FAIL: test beklenen değeri kaynaktan türetiyor | ✓ | anahtarlı yol listesi `PathMasker.KeyedPrefixes`'ten, beklenen çıktı `PathMasker.Mask`'ten; süzgeçler Caddyfile'dan okunur, elle yazılmış ikinci liste yok |
| Sık FAIL (1) beklenen değer sabitten | ✓ | testler başlık değerini ve `LoggingOptions`/Compose/Dockerfile değerlerini dosyadan okur; düzey testi `LogEventLevel` değerlerinden |
| Sık FAIL (2) öteki yerler | ✓ | `roll_size`/`roll_keep`/`50MiB` tüm depoda arandı: kalan yok; runbook, `09 §18`, IMPLEMENTATION_STATUS, MEMORY güncel; `SystemLogsDeployConsistencyTests` Caddyfile/compose/imaj/runbook eşitliğini sınar |
| Sık FAIL (3) tarama istisnası geniş değil | ✓ | `LogFileDeletionSourceScan.AppliesTo` yalnız `Infrastructure/Logging/`; `LogFileDeletionScan_AppliesToTheLoggingFolderOnly`; yalnız yorum satırı ve blok yorum atlanır |
| Sık FAIL (4) ihlal örneği; birim testi dosya sistemine yazmıyor | ✓ | ihlal örneği var; dosya yazan testler Integration'da |
| Sık FAIL (5) kayıt satırı maskelemesiz ikinci çıktıya inmiyor | ✓ | T230 `writeToProviders: false`; konteyner çıktısı yalnız açılış/çökme (compose `x-container-log`, veritabanı kaydı dosyada ve parametresiz) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 764/764 (düzeltme 1 sonrası; yapımda 680 Consistency dışı) |
| Integration | ✓ | PostgreSQL 18 (Docker, `shopfolio-t225-pg`), ad alanına göre: Logging, Database+FileSecrets+Model, Web | 29 + 952 + 54 = 1035, hepsi yeşil |
| Diğer | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format --verify-no-changes`; `tests/scripts/run-script-tests.sh`; tam `scripts/ci/package-build.sh` (yerel) | 0 uyarı; temiz; 4 betik dosyası; "Package build: all checks passed." |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, 2026-11-05, head `bf2ecc8`; tur 1 FAIL, head `a8390bc`) |
| Bulgu sayısı | Tur 2: 2 (1 KRİTİK S1, 1 DÜŞÜK S1) · tur 1: 2 (1 KRİTİK S1, 1 ORTA S1) |
| Düzeltme gerekli mi | Evet — erişim kaydında bir çiftten sonra gelen çıplak sorgu belirteçleri maskelenmiyor; satırda `05 §9.1.2` dışı Caddy alanları (aşağıdaki "Doğrulama Sonucu — tur 2"). İki iterasyon doldu: eskalasyon |
| Bloke-etmeyen not (K) sayısı | Tur 2: 1 (D-92) · tur 1: 3 (D-91) |

## Düzeltme 1 (doğrulama tur 1 FAIL sonrası, 2026-11-05)

- **B1 (KRİTİK) kapandı:** `deploy/caddy/Caddyfile` `format filter` içinde `request>uri` üstünde dört `regexp` süzgeci (sırayla çalışır): (1) kapalı listedeki ön ekten sonraki bölüm `{key}` (`(?i)`, `/+` ile boş bölüm atlanır — D-89 K3); (2) her sorgu çiftinin değeri `*`; (3) `=` içermeyen çıplak belirteç `*` (iki geçiş: eşleşme `&`'i tükettiği için). Liste `PathMasker.KeyedPrefixes` ile aynı. Sözdizimi `caddy validate` ile (`caddy:2.11.7-alpine`) "Valid configuration"; çalışan Caddy sondası: on dört yol, anahtar ve sorgu değeri satırda yok.
- **B2 (ORTA) kapandı:** `log_append user_agent {http.request.header.User-Agent}`; başlık silme korunur, satırda yalnız bu alan var, `Cookie` yok (sonda).
- **Testler (`SystemLogsDeployConsistencyTests`, +3):** `Caddyfile_MasksTheKeyedPathsOfPathMasker_AndNoOthers` (liste eşitliği), `Caddyfile_UriFilters_GiveWhatPathMaskerGivesForEveryKeyedPathAndQueryShape` (süzgeçler Caddyfile'dan okunur, çıktı `PathMasker.Mask` ile karşılaştırılır), `Caddyfile_WritesTheBrowserIdentificationAsItsOwnFieldAndNoOtherHeader`.
- **Belgeler:** `DEPLOY_RUNBOOK` K-915 özeti, L.2, L.5 madde 3; `09 §18.2` madde 2b ve 7; `DEFERRED_BACKLOG` D-33 (kısmen çözüldü, kalan T384) ve D-89 K3 (çözüldü); `IMPLEMENTATION_STATUS`; `MEMORY.md`.
- **Sınama:** build `-warnaserror` 0/0; `dotnet format --verify-no-changes` temiz; Unit 764/764; Integration Logging 29, Web 54 yeşil (PostgreSQL 18, `shopfolio-t225-fix-pg`, silindi); `run-script-tests.sh` 4 dosya geçti.

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
| 4 | Dışa aktarma yollarının `response_header_timeout` bloğu | D-33 | dışa aktarma ucu T384'te |
| 5 | Süzgeç `&` içeren yol bölümünü de (`/a&b`) sorgu belirteci sayıp `*` yapar (fazla maskeleme, güvenli yön); `%2F` kodlu ayırıcı iki katmanda da maskelenmez | D-89 | bugün anahtarlı yolda `&` yok; `%2F` DÜŞÜK |

## Notlar

- **Gün dönümü:** normal yazıcı sunucu saatine bağlı, saat oynatılamıyor; test yeni gün dosyasında eskilerin silinmediğini gösterir; denetim/komut yazıcıları sahte saatle sınanır.
- **`log_min_error_statement`** varsayılan: `STATEMENT:` satırı kayda girebilir (EF parametreli, değer sızmaz); gerekirse D- kaydı.
- **Yerel paketleme sınaması:** Git Bash'te `mktemp` dizini Docker'a bağlanamadığı için betik `TMPDIR` Windows yoluna alınarak koşuldu (betik değişmedi).
- **Repo memory (bitiş kapısı 9):** güncellendi

---

## Doğrulama Sonucu — T225 Sistem kayıtları — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T225-sistem-kayitlari   **Commit:** a8390bc (doğrulanan head)

### Verdict: ✗ FAIL

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (switch sonrası da) |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` (ana depo kopyası, worktree ortak) |
| Ana dal CI (son 3) | ✓ | 41953520461 success · 41953520476 success · 41946974061 success |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22 T225 |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç kaynak, günlük dosyalar; yazanlar; ayarlar runbook'ta, kurulumda denetlenir | ✓ | compose: `access-logs`, `app-logs`, `db-logs`, `backup-logs` ayrı birim; `30-logging.conf` dosyaya yazar (`logging_collector=on`), CI Paketleme adımı `show` denetimleriyle yeşil (run 41953918692); runbook §L.1–L.5; `SystemLogsDeployConsistencyTests` yeşil |
| 2 | İstek kimliğini vekil üretir, `X-Request-Id` ile iletir; uygulama satırları başlık değerini taşır | ✓ | Validator sondası (gerçek `caddy:2.11.7-alpine`, daldaki Caddyfile, başlığı yansıtan sahte arka uç): istemci `X-Request-Id: sahte` gönderdi → arka uca `e9626c27-…` iletildi, erişim satırında `request_id` aynı UUID; uygulama tarafı `SystemLogsTests` (500/422/401, iki istek) yeşil; `RequestLogEnricher` UUID biçimini kabul eder |
| 3 | Şifre, anahtar, sır, … e-posta, form gövdesi, çerez hiçbir kayda yazılmaz | ✗ | Aynı sonda: `GET /siparis/erisim/GIZLIANAHTAR123/teyit?search=gizlisozcuk&email=a@b.com` erişim kaydına `"uri":"/siparis/erisim/GIZLIANAHTAR123/teyit?search=gizlisozcuk&email=a@b.com"` olarak yazıldı; `/dogrulama/TOKENXYZ` da ham. Caddyfile `format filter`'da `request>uri` için maske yok. Uygulama kaydı ve veritabanı kaydı tarafı temiz (`SampleLines_…` yeşil; `terse`, parametre 0) |
| 4 | Düzey tasarım değeri; çatı ve veri erişimi uyarıda | ✓ | `LevelSetting_…` (6 düzey) yeşil |
| 5 | P-45 saklama; dosyayı yalnız imha işi siler; sunucu kayıtları kendi döngüsüyle | ✓ | `roll_disabled` (Caddy `validate` geçti); logrotate erişim kaydında silmesiz (`rotate 36500`, `maxage` yok), sunucu kayıtlarında 183 gün; `journald MaxRetentionSec=6month`; `NormalWriter_…`, `AuditAndCommandWriters_…`, `Restart_…` yeşil (gün sınırı kenarı K2) |
| 6 | İmha işi ve imha satırı T201'de, bu task yazmaz | ✓ | `LogFileDeletionSourceScan` + ihlal örneği; `SystemLogsScanTests` yeşil |
| 7 | Ayrı metrik toplama ve pano yok | ✓ | `MetricsPackageScan` yeşil; `src/` altında `Metrics`/OpenTelemetry yok (grep) |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
Task'ın KP birimi `10 KP-74`; grep'in getirdiği dokuz satır (`VAL-A368`–`A370` aydınlatma, `VAL-D046`–`D051` imha) T225'in kabul kriterlerine eşlenmez (kriter 6 imhayı T201'e bırakır) — `Durum` değişmedi. Task'ın `05 §9.1.5` kaynağını ana alan uçtan uca satır `VAL-A991`'in ters vekil yarısı bu task'ındır (T230/T231 raporları onu T225'e bıraktı).
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-A991 | KRİTİK | yok | BEKLEMEDE | FAIL | — |

`12_matris_denetle.py --kapsam 1-77`: Hata 0.

### Doğrulama Kontrol Listesi
- [x] KK1 uygulama kaydı ayrı birim ve dizinde; vekil/sunucu kayıtları runbook L.5'te
- [x] KK2 aynı istek aynı kimlik; 500/422/401 satırları; `requestId` başlık değeri
- [x] KK3 örnek uygulama satırlarında yasaklı alan yok (erişim kaydı için bkz. kriter 3)
- [x] KK4 düzey ayarı yalnız uygulama kategorisini değiştirir
- [x] KK5 gün dönümü ve yeniden başlatmada dosya kalır
- [x] KK6 kayıt klasöründe silen kod ve imha satırı yok
- [x] KK7 metrik paketi yok
- [ ] `09 §25` "kayıtta sır ve kişisel veri yok" — erişim kaydı anahtar ve sorgu değeri taşıyor
- [ ] Güvenlik taraması — bkz. Bulgu 1

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 761/761 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration | ✓ | PostgreSQL 18 (`shopfolio-t225-val-pg`, 127.0.0.1:56371 — 55370–55569 Windows'ta dışlanmış aralık, bağlanamadı), `--filter-namespace` Logging+Web+Database+FileSecrets / Model | 142 + 893 = 1035, hepsi yeşil |
| Caddy | ✓/✗ | `caddy validate` + çalışan Caddy sondası (`shopfolio-t225-val-*`) | geçerli yapılandırma; istek kimliği doğru; yol/sorgu maskesiz |
| Sonrası | ✓ | `git status`; `dotnet restore Shopfolio.slnx --locked-mode` | temiz; çıkış 0; konteynerler ve ağ adıyla silindi |
| Dal CI | ✓ | `gh run list --branch task/T225-sistem-kayitlari` | 41953918692 success (head a8390bc), 10 iş yeşil |

### Güvenlik Kontrolü
- Secret sızıntısı: **Bulgu** — erişim kaydı bağlantı anahtarını (sipariş erişim, doğrulama, şifre sıfırlama, davet, geri alma, indirme imzası) ve sorgu değerlerini ham yazıyor (Bulgu 1). Repoda/imajda yeni sır yok.
- Auth etkisi: Temiz — istemcinin `X-Request-Id`'si değiştirilir (sondayla kanıtlı); uygulama portu yayımlanmıyor.
- Input validation: Temiz — `RequestLogEnricher` başlığı uzunluk ve karakterle süzer.
- Yeni bağımlılık: Yok.

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | VAL-A991 (kriter 3) | KRİTİK | S1 | Beklenen (`05 §9.1.5`, §6.4.8; runbook K-1162 (1)): erişim kaydı yolun anahtar bölümünü `{key}`, sorgu değerlerini `*` yazar; boş bölüm atlanır (D-89 K3). Gerçekleşen: Caddyfile'da maske yok, `request>uri` ham (sonda kanıtı yukarıda). Runbook L.5 madde 3 maskeli çıktıyı bekliyor — kurulum denetimi düşer; `SystemLogsDeployConsistencyTests` maskeyi sınamıyor. D-33 (maskeleme kısmı) ve D-89 K3 T225'i hedef gösteriyor, dal ikisini de ne kapatıyor ne yeniden ileri atıyor | `09 §25` sır/kişisel veri satırı; D-33, D-89 K3 | bu PR'da düzeltilmeli: `format filter`'a `request>uri` için anahtarlı yol listesinin (K-1162 (1), `09 §18.2` madde 2b ile aynı) ve sorgu değerlerinin maskesi (`regexp`/`query` filtresi), boş bölüm dahil; maskeyi Caddyfile'dan sınayan tutarlılık testi (anahtar listesi `PathMasker`'ınkiyle eşit); D-33/D-89 kayıtlarının güncellenmesi | `deploy/caddy/Caddyfile` · `tests/.../SystemLogsDeployConsistencyTests.cs` · `Docs/DEFERRED_BACKLOG.md` |
| 2 | — (kriter 1; `05 §9.1.2`) | ORTA | S1 | Beklenen: erişim kaydı satırı tarayıcı tanıtımını taşır (`05 §9.1.2`; D-33). Gerçekleşen: `request>headers delete` `User-Agent`'ı da siler (sondada satırda yok); runbook L.5 "hiçbir başlık yazılmaz" der | D-33 | bu PR'da: `log_append user_agent {http.request.header.User-Agent}` gibi tek alan; runbook L.2/L.5 ve tutarlılık testi | `deploy/caddy/Caddyfile` · `Docs/DEPLOY_RUNBOOK.md` §L |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Döndürülmüş erişim kaydının adındaki gün (`dateext`) döndürme günüdür, içerik bir önceki günündür; imha bir gün geç siler | DEFERRED_BACKLOG'a forward (D-91 K1) |
| K2 | Sunucu kayıtları 183 gün / journald `6month` (≈182,6 gün) — 184 günlük takvim penceresinde süre dolmadan bir iki gün erken silme | DEFERRED_BACKLOG'a forward (D-91 K2) |
| K3 | `log_min_error_statement` varsayılanı `STATEMENT:` metnini yazar; değeri metne gömülü cümlede değer sızabilir (yapım raporu kararsız bırakmıştı) | DEFERRED_BACKLOG'a forward (D-91 K3) |

### Yapım Raporu Karşılaştırması
- Uyum: 3 uyuşmazlık
- Kriter 3 ve `09 §25` satırı yapımda ✓; kanıtı yalnız uygulama kaydı, başlık silme ve veritabanıdır — erişim kaydının yol/sorgu maskesi hiç anılmıyor, oysa runbook L.5 madde 3 onu bekliyor (Bulgu 1). Öz denetim tablosu var ve maddeleri kanıtlı, ama bu kanıt kriteri karşılamıyor.
- D-33 ve D-89 K3 (T225 hedefli) "Known Limitations"ta da yok (Bulgu 1, 2).
- "Notlar"daki `log_min_error_statement` "gerekirse D- kaydı" — üçüncü seçenek; D-91 K3'e forward edildi.
- Dış varsayım 1 "davranış gerçek sunucuda T603": validator davranışı yerel Caddy 2.11.7'de sınadı — istek kimliği doğru.
- Unit sayısı 680 (yapım) ↔ 761 (validator, Consistency dahil) — kapsam farkı, uyuşmazlık değil.

### Merge ve Post-merge
- Merge yok (FAIL). Rapor, status (`✗ FAIL (tur 1)`), hafıza, `VAL-A991` `Durum` ve D-91 dala commit'lendi.

## Doğrulama Sonucu — T225 Sistem kayıtları — tur 2
**Tarih:** 2026-11-05   **Branch:** task/T225-sistem-kayitlari   **Commit:** bf2ecc8 (doğrulanan head)

### Verdict: ✗ FAIL

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (switch öncesi ve sonrası) |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 41958746129 success · 41958746107 success · 41953520461 success |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22 T225 |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç kaynak, günlük dosyalar; yazanlar; ayarlar runbook'ta, kurulumda denetlenir | ✓ | compose `x-container-log` (json-file 10m×3) her serviste; `access-logs`/`app-logs`/`db-logs`/`backup-logs`; `30-logging.conf` dosyaya yazar, döndürmesi kapalı; Dockerfile `/var/log/postgresql/server` 2750 `logread`; `package-build.sh` adım 6 `show` denetimleri + dizin modu; runbook L.1–L.5 dosyalarla tutarlı; dal CI Paketleme yeşil (41958706379); `SystemLogsDeployConsistencyTests` yeşil. Erişim satırında `user_agent` alanı var (gerçek Caddy sondası: `"user_agent":"ProbeUA/1.0"`) — tur 1 Bulgu 2 kapandı |
| 2 | İstek kimliğini vekil üretir, `X-Request-Id` ile iletir; uygulama satırları başlık değerini taşır | ✓ | Gerçek `caddy:2.11.7-alpine` + daldaki Caddyfile + başlığı yansıtan arka uç: istemci `X-Request-Id: sahte` gönderdi → arka uca `c4a07c7d-…` gitti, erişim satırında `request_id` ve `uuid` aynı UUID, `sahte` kayıtta yok; `SystemLogsTests` (500/422/401, iki istek) yeşil |
| 3 | Şifre, anahtar, sır, … e-posta, form gövdesi, çerez hiçbir kayda yazılmaz | ✗ | Anahtarlı yolların hepsi (10 ön ek, büyük harf, `//`) ve çift/tek sorgu biçimleri gerçek Caddy'de maskelendi; `Cookie`, `Authorization`, POST gövdesi kayıtta yok. **Ama** sorguda bir `ad=değer` çiftinden sonra gelen çıplak belirteçler maskelenmiyor: `/arama?a=1&b&ali@x.com` → `"uri":"/arama?a=*&*&ali@x.com"`; `/arama?q=1&a&b&c&d` → `"uri":"/arama?q=*&*&b&*&d"` (gerçek Caddy sondası). E-posta erişim kaydına ham yazıldı. Uygulama kaydı (`PathMasker`) aynı girdiyi tam maskeler; `SystemLogsDeployConsistencyTests`'in örnek listesinde bu biçim yok |
| 4 | Düzey tasarım değeri; çatı ve veri erişimi uyarıda | ✓ | `SystemLogsTests` düzey testleri yeşil (Integration Logging 142/142) |
| 5 | P-45 saklama; dosyayı yalnız imha işi siler; sunucu kayıtları kendi döngüsüyle | ✓ | `roll_disabled` (`caddy validate` geçti, adapt çıktısında); runbook L.3 erişim kaydı `rotate 36500` silmesiz, sunucu dosyaları 183 gün; L.4 `MaxRetentionSec=6month`; PG `log_rotation_age/size = 0`; gün dönümü/yeniden başlatma testleri yeşil (süre kenarları D-91 K1/K2'de) |
| 6 | İmha işi ve imha satırı T201'de, bu task yazmaz | ✓ | `LogFileDeletionSourceScan` + ihlal örneği; `SystemLogsScanTests` yeşil (Unit 764/764) |
| 7 | Ayrı metrik toplama ve pano yok | ✓ | `MetricsPackageScan` yeşil |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
`10 KP-74` grep'i dokuz satır getirir (`VAL-A368`–`A370`, `VAL-D046`–`D051`); T225'in kabul kriterlerine eşlenmez (imha T201'in) — `Durum` değişmedi. `05 §9.1.5` kaynağını ana alan `VAL-A991`'in ters vekil yarısı bu task'ındır.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-A991 | KRİTİK | yok | FAIL | FAIL | — |

`12_matris_denetle.py --kapsam 1-77`: Hata 0.

### Doğrulama Kontrol Listesi
- [x] KK1 uygulama kaydı ayrı birim ve dizinde; vekil/sunucu kayıtları runbook L.5'te
- [x] KK2 aynı istek aynı kimlik; 500/422/401 satırları; `requestId` başlık değeri
- [x] KK3 örnek uygulama satırlarında yasaklı alan yok (erişim kaydı için bkz. kriter 3)
- [x] KK4 düzey ayarı yalnız uygulama kategorisini değiştirir
- [x] KK5 gün dönümü ve yeniden başlatmada dosya kalır
- [x] KK6 kayıt klasöründe silen kod ve imha satırı yok
- [x] KK7 metrik paketi yok
- [ ] `09 §25` satır 2386 "kayıtta sır ve kişisel veri yok" — erişim kaydı çıplak sorgu belirtecindeki e-postayı taşıyor
- [ ] Güvenlik taraması — bkz. Bulgu 1

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 764/764 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration | ✓ | PostgreSQL 18 (`shopfolio-t225-val2-pg`, 127.0.0.1:56372), `--filter-namespace` Logging+Web+Database+FileSecrets / Model | 142 + 893 = 1035, hepsi yeşil |
| Caddy (gerçek imaj) | ✗ | `caddy validate` + `caddy adapt` + çalışan Caddy sondası (`shopfolio-t225-val2-*`, iki tur, 17 istek) | geçerli; dört süzgeç `multi_regexp` olarak sırayla; istek kimliği ve `user_agent` doğru; anahtarlı yollar ve çiftler maskeli; çift sonrası çıplak belirteç ve `%`-kodlu ön ek ham |
| Sonrası | ✓ | `git status`; `dotnet restore Shopfolio.slnx --locked-mode` | temiz; çıkış 0; konteynerler ve ağ adıyla silindi |
| Dal CI | ✓ | `gh run list --branch task/T225-sistem-kayitlari` | 41958706379 success (head bf2ecc8) |

### Güvenlik Kontrolü
- Secret sızıntısı: **Bulgu** — erişim kaydı, sorguda bir çiftten sonra gelen çıplak belirteçleri ham yazıyor (e-posta dahil; Bulgu 1). Bağlantı anahtarları normal biçimde maskeli. Repoda/imajda yeni sır yok.
- Auth etkisi: Temiz — istemcinin `X-Request-Id`'si değiştirilir (sondayla kanıtlı); `trusted_proxies` yok.
- Input validation: Temiz.
- Yeni bağımlılık: Yok.

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | VAL-A991 (kriter 3) | KRİTİK | S1 | Beklenen (`05 §9.1.5`; runbook L.2 "`=` içermeyen çıplak belirteç de `*` olur"; `PathMasker` ile aynı kural): sorgunun her değeri `*`. Gerçekleşen: süzgeç 3–4'ün "iki geçiş" hilesi yalnız baştan çıplak dizide çalışır; önce bir `ad=değer` çifti varsa 2. süzgecin yazdığı `*` 4. süzgeçte belirteç sayılıp `&`'leri tüketir ve hizayı kaydırır — `?a=1&b&ali@x.com` → `?a=*&*&ali@x.com`, `?q=1&a&b&c&d` → `?q=*&*&b&*&d` (gerçek Caddy). Tutarlılık testi bu biçimi örneklemediği için eşdeğerlik iddiası sınanmamış | `09 §25` satır 2386; D-33 "maske kapandı" ibaresi | bu PR'da düzeltilmeli: RE2'de ileri bakış yok — örneğin önce `&` → `&&` (ve `?` → `?&` benzeri), sonra `([?&])[^=&]+(&|$)`, sonra `&&` → `&`; ya da çift ve belirteci tek düzenle ele alan başka bir dizilim. Test örneklerine en az `?a=1&b&c`, `?a=1&b&c&d&e`, `?a&b=1&c&d` eklenir (`PathMasker.Mask` ile eşitlik) ve sonda gerçek Caddy ile sınanır | `deploy/caddy/Caddyfile` · `tests/Shopfolio.UnitTests/Consistency/SystemLogsDeployConsistencyTests.cs` · `Docs/DEFERRED_BACKLOG.md` D-33 |
| 2 | VAL-A991 (`05 §9.1.2`, §9.1.5 "yalnız 9.1.2'nin alanları yazılır") | DÜŞÜK | S1 | Beklenen: erişim satırı yalnız zaman, IP, yöntem, yol, yanıt kodu, boyut, süre, tarayıcı tanıtımı ve istek kimliğini taşır. Gerçekleşen (sonda satırı): Caddy'nin varsayılan alanları da yazılıyor — `request.remote_port`, `request.proto`, `request.host`, `bytes_read`, `user_id`, ayrıca `uuid` (`request_id`'nin ikinci kopyası). Kişisel veri değiller; satır belgenin kapalı listesini aşıyor ve runbook L.5 madde 3 bunu anmıyor | — | bu PR'da: `format filter`'a bu alanlar için `delete` satırları (zarf alanları `ts`, `level`, `logger`, `msg` için runbook L.2'de tek cümlelik gerekçe) ya da runbook/`05` uyumlu açık istisna listesi; tutarlılık testi silinen alanları sınar | `deploy/caddy/Caddyfile` · `Docs/DEPLOY_RUNBOOK.md` L.2/L.5 |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Yüzde kodlu ön ek (`/sipar%69s/erisim/ENCKEY/teyit`) Caddy'de ham kalır (sonda: `"uri":"/sipar%69s/erisim/ENCKEY/teyit"`); ASP.NET Core yolu çözdüğü için uygulama bunu `/siparis/erisim/…` olarak yönlendirir ve `PathMasker` maskeler. Meşru bağlantılar kodlanmış gelmez; yalnız elle kurulmuş istek. D-89 K3'ün `%2F` notundan farklı (orada yönlendirici eşlemez) | DEFERRED_BACKLOG'a forward (D-92) |

### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Kriter 3 / KK3 yapımda ✓; öz denetim "çıplak belirteç" sınandı der, ama örnekler yalnız baştan çıplak dizileri (`?a&b&c&d`, `…?bare`) ve tek çıplak belirteçli `?x=1&bare&y=`'yi kapsıyor; çift + iki ve daha çok çıplak belirteç biçimi sızıyor (Bulgu 1). Rapor "(3) çıplak belirteç `*` (iki geçiş)" kuralını genel doğru sayıyor.
- "`05 §9.1.2` alan listesi belgeyle birebir" satırı ✓, kanıtı yalnız eklenen iki alan; Caddy'nin varsayılan alanları sınanmamış (Bulgu 2).
- Öz denetim tablosu var, maddeleri kanıtlı. Tur 1 Bulgu 2 (`User-Agent`) ve Bulgu 1'in anahtarlı yol kısmı gerçekten kapandı.

### Merge ve Post-merge
- Merge yok (FAIL, tur 2 — `12 §8.5` iki iterasyon sınırı: eskalasyon, `INSTRUCTIONS.md` §3.3). Rapor, status (`✗ FAIL (tur 2)`), hafıza ve D-92 dala commit'lendi; `VAL-A991` `Durum` FAIL kaldı.
