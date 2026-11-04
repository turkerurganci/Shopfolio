# T233 — Veritabanı oturum süreleri

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Süre sabitleri:** `Infrastructure/DatabaseTimeouts` altı adlı `TimeSpan` sabitidir: `LockTimeout` 5 sn, `StatementTimeout` 15 sn, `MaintenanceStatementTimeout` 10 dk, `IdleInTransactionTimeout` 30 sn, `CommandTimeout` 20 sn, `MaintenanceCommandTimeout` 10 dk 5 sn (`09 §3.5` tablosunun iki satırı; `§10.7` madde 1). Sayı başka yerde yazılmaz; T01'den kalan tek `CommandTimeout` sabiti genişletildi, `DatabaseOptionsValidator` onu kullanmaya devam eder.
- **Tek kuruluş noktası:** `Infrastructure/DatabaseConnection.Configure(NpgsqlConnectionStringBuilder, DatabaseSessionProfile)` üç sunucu ayarını `Options` anahtarıyla, istemci süresini `CommandTimeout` ile kurar; `Create(connectionString, profile)` aynı işi kurulum ayarının dizisi için yapar. `DatabaseSessionProfile` = `Web`, `Maintenance` (`§10.7` madde 2).
- **Çağıranlar:** `Program.cs` veri kaynağını ve `AppDbContext`'i `Web` profiliyle kurar (bağlantı dizisi tek yerde, `WebConnectionString`); göç hizmeti (`deploy/compose.yaml` `migrate`) bağlantı dizisinde `Maintenance` profilinin `Options` ve `Command Timeout` değerlerini taşır — göç demeti kod değil `efbundle` olduğu için dize Compose'tadır ve bir eşitlik denetimi onu `DatabaseConnection`'ın yazdığıyla eşit tutar. Yönetim komutları (`CommandHost`) T303'ün; `Maintenance` profili hazır.
- **Yeniden deneme kapalı:** `AppDbContext.Configure` `EnableRetryOnFailure` çağırmaz (yorum eklendi); `Database.CreateExecutionStrategy().RetriesOnFailure` yanlıştır.
- **Kaynak taramaları (ikisi de kasıtlı ihlal örnekli, salt okunur metin):**
  - `DatabaseSessionSourceScan`: `SET statement_timeout|lock_timeout|idle_in_transaction_session_timeout`, `SET LOCAL`, `set_config(...)` biçimleri (çok satırlı dahil), `EnableRetryOnFailure` ve üç süre hata kodunun (`55P03`, `57014`, `25P03`) anılması; istisna yalnız `src/Shopfolio.Web/Infrastructure/DatabaseConnection.cs`, test projeleri hiç taranmaz. Örnek: `ViolationSamples/DatabaseSession/SessionTimeoutChange.cs`.
  - `TransactionBodySourceScan`: `ITransactionRunner` anan dosyada her `RunAsync(...)` çağrısının argümanında `HttpClient`, `IHttpClientFactory`, `I...Gateway` türleri ya da dosyada bu türlerle bildirilmiş alan/parametre/değişken adları bulgudur. Örnek: `ViolationSamples/TransactionBody/OutsideCallInTransaction.cs`. Çalıştırıcı henüz yazılmadığından tarama bugün gerçek çağrı bulmaz; D-82.
- **Doküman:** `09 §10.7` — örnek yorumu "Npgsql 10.0.3'e karşı sınandı", madde 2'ye Compose eşitliği cümlesi, madde 6'ya süre testinin gerçek süreyle koştuğu istisna (D-22 kapandı), madde 7 erişilemeyen sunucu kolu daraltıldı (D-26 kapandı). `.claude/CONTEXT.md` §3; `DEFERRED_BACKLOG` D-82, D-83 açıldı.
- **Önceki task kaydı (skill adım 15):** T226 ve T227 status Commit hücreleri (`b8f20ad`, `15daf95`) ve raporlarının "Post-merge run'lar" satırı dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/DatabaseTimeouts.cs`, `DatabaseConnection.cs` (yeni), `AppDbContext.cs` (yorum), `Program.cs`; `deploy/compose.yaml` (`migrate` bağlantı dizisi)
- `tests/Shopfolio.UnitTests/Infrastructure/DatabaseConnectionTests.cs`, `Consistency/DatabaseTimeoutsConsistencyTests.cs`, `Architecture/DatabaseSessionSourceScan(+Tests).cs`, `Architecture/TransactionBodySourceScan.cs`, `Architecture/ViolationSamples/DatabaseSession/`, `ViolationSamples/TransactionBody/`
- `tests/Shopfolio.IntegrationTests/Database/DatabaseSessionTimeoutTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T226_REPORT.md`, `T227_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`
- Modül klasörlerine, varlıklara, göçlere ve paketlere dokunulmadı.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Süre değerleri `DatabaseTimeouts`'ta adlı sabit; tek yer `09 §3.5` | ✓ | `DatabaseTimeouts.cs`; `WebRowOfTheTable_EqualsTheWebConstants`, `MaintenanceRowOfTheTable_EqualsTheMaintenanceConstants` (tablo satırını okur) |
| 2 | Bağlantı kuruluşu tek yerde: üç sunucu ayarı ve istemci süresi | ✓ | `DatabaseConnection.Configure`; `Configure_WebProfile_WritesTheThreeServerTimesAndTheClientTime`; `Program.cs` `WebConnectionString`; `Show_EachProfile_GivesTheServerTimesOfThatProfile` (gerçek sunucu) |
| 3 | İki profil; ayrı bağlantı dizisi (ayrı havuz); `SET` yok; tarama yalnız `DatabaseConnection`'da ve testlerde bulur | ✓ | `Create_TheTwoProfiles_AreTwoConnectionStrings`, `Show_TheTwoProfiles_AreNotOneSessionSetting`; `ApplicationSource_ChangesNoSessionTimeoutOutsideDatabaseConnection`, `SessionScan_OnViolationSample_...`, `Applies_LeavesOutOnlyTheDatabaseConnectionFile`; Compose: `MigrationService_ConnectsWithTheMaintenanceProfile` |
| 4 | İstemci süresi her profilde kilit ve sorgu sürelerinden uzun; komut başına süre yok | ✓ | `Configure_EachProfile_LeavesTheClientTimeLongerThanTheLockAndStatementTimes`; `CommandTimeout` ataması yalnız `DatabaseConnection`'da (`grep -rn "CommandTimeout" src/` → `DatabaseConnection.cs`, `DatabaseTimeouts.cs`, `DatabaseOptionsValidator.cs`); boşta işlem 30 sn > istemci 20 sn: `09` kendi cümlesiyle çelişir, D-83 ✅ (düzeltme 1: cümle kilit ve sorgu sürelerine daraltıldı) |
| 5 | Aşım beklenmeyen hâle düşer; kod yakalamaz/çevirmez/yeniden denemez; `ITransactionRunner` gövdesinde dış çağrı yok | ✓ | `LockWait_...FailsWithTheServersLockError` (55P03), `LongQuery_...IsCancelledByTheServerBeforeTheClientTime` (57014, 14–20 sn), `IdleOpenTransaction_...LosesItsConnection`; hata kodu anan kod yok: `SessionScan_...` örnek satır 15; `ApplicationSource_UsesNoOutsideServiceInsideATransactionBody`, `TransactionBodyScan_OnViolationSample_...` |
| 6 | Otomatik yeniden deneme kapalı | ✓ | `AppDbContext_AsConfigured_HasNoRetryStrategy`; kırmızı gösterimi `AppDbContext_WithRetryOnFailureTurnedOn_ShowsTheCheckCanGoRed`; kaynakta `EnableRetryOnFailure` taraması (örnek satır 14) |
| 7 | `Options` ve `CommandTimeout` adları Npgsql sürümüne karşı doğrulandı | ✓ | Kilit dosyası `src/Shopfolio.Web/packages.lock.json`: `Npgsql` 10.0.3 (çözülen); `NpgsqlConnectionStringBuilder.Options` ve `.CommandTimeout` bu sürümde derlenir; `Show_EachProfile_...` sunucuya gerçekten gittiğini gösterir; fark: PR açıklamasında |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1/KK1: sabitler `09 §3.5` satırlarıyla bire bir | ✓ | `DatabaseTimeouts.cs` altı sabit; tablo satırını ayrıştıran `DatabaseTimeoutsConsistencyTests` iki testi (Lint adımında koşar); `Timeouts_PassTheContradictionScanOfTheTable` |
| k2/KK2: tek kuruluş noktası; başka yerde bağlantı dizgesi kurulmuyor | ✓ | `grep -rn "NpgsqlDataSource.Create\|UseNpgsql(\|new NpgsqlConnectionStringBuilder" src/` → `Program.cs` (yalnız `DatabaseConnection.Create` çıktısıyla), `AppDbContext.Configure` (dizeyi alır), `DatabaseConnection.cs`, `DatabaseOptionsValidator.cs` (yalnız ayrıştırma sınaması), `AppDbContextDesignTimeFactory` (bağlantısız yer tutucu, tasarım zamanı); `Options`/`CommandTimeout` yazan başka yer yok |
| k3/KK3: Maintenance daha uzun sorgu süresi; göç hizmeti `Maintenance` ile bağlanır; iki dizi ayrı; `SET statement_timeout` taramayı kırar | ✓ | `Configure_MaintenanceProfile_*`, `Show_EachProfile_*` (Maintenance sunucu süresi `DatabaseTimeouts.MaintenanceStatementTimeout`ten), `MigrationService_ConnectsWithTheMaintenanceProfile` + `MigrationServiceConnectionInTheComposeFile_ParsesToTheMaintenanceTimes` + `MigrationServiceCheck_WithTheWebProfile_WouldDiffer`; tarama kırılışı `SessionScan_OnViolationSample_...` (satır 5, 6, 9) ve `SET LOCAL`/`set_config` (satır 7, 8). Yönetim komutu `CommandHost` T303 KK4'ündür |
| k4/KK4: istemci süresi her iki profilde kilit ve sorgu sürelerinden uzun | ✓ (kilit ve sorgu; D-83 ✅) | `Configure_EachProfile_LeavesTheClientTimeLongerThanTheLockAndStatementTimes` (Theory, iki profil); boşta işlem Web'de 30 > 20: D-83 ✅ (boşta işlem süresi komut süresi değildir; 09 §10.7 madde 2-3) |
| k5/KK5: aşım testi beklenmeyen hata yanıtına düşer; işlem gövdesine konan `HttpClient` taramayı kırar | ✓ (⚠ yanıt katmanı yok) | Aşım testleri sunucunun ham hatasını (55P03/57014/bağlantı kaybı) görür; "tek yakalama katmanı" (`§8.1`) henüz yazılmadı, bu yüzden "yanıta düşer" doğrudan sınanamaz — kodun onu yakalamadığı kaynak taramasıyla sınanır (`SessionTimeoutHandled`, örnek satır 15). `HttpClient` örneği: `TransactionBodyScan_OnViolationSample_...` (satır 5, 15), `IPaymentGateway` (satır 10) |
| k6/KK6: yeniden deneme stratejisi etkin değil | ✓ | `AppDbContext_AsConfigured_HasNoRetryStrategy` + kırmızı gösterimi + `RetryStrategy` taraması |
| k7/KK7: ad farkı PR açıklamasında | ✓ | PR açıklamasında "Npgsql 10.0.3: `Options`, `CommandTimeout` adları aynı; fark yok" |
| `09 §25` — doküman uyumu | ✓ | Okunan: çekirdek, `09 §10.7`, §3.5 süre satırları, §19, §25; ad ve yer `09 §10.7`'den birebir (`DatabaseTimeouts`, `DatabaseConnection.Configure`, `DatabaseSessionProfile`); yeni ayar/sabit/süre yok (tablodaki satırlar); `.env.example` değişmez; "bilgim tarihli" Npgsql adları doğrulandı ve §10.7 örnek yorumuna yazıldı |
| `09 §25` — diff kapsamı | ✓ | Diff task'ın istediğini ve backlog'un T233'e hedeflediği D-22/D-26 doküman düzeltmelerini içerir (⚠ kararlar); başka davranış yok |
| `09 §25` — mimari ve kod | ✓ | Yeni dosya `Infrastructure/` ve `UnitTests/Architecture|Infrastructure|Consistency`, `IntegrationTests/Database` (§4.1); `CONTEXT.md` §3 güncel; yeni paket yok; zaman yok; `catch` eklenmedi |
| `09 §25` — test | ✓ | Her yeni kuralın ayrı ihlal örneği var (SET biçimleri/retry/hata kodu tek örnek dosyasında beş ayrı kural; işlem gövdesi ayrı dosya); birim testleri dosya yazmaz (taramalar ve Compose/09 okuması salt okunur; `grep -n "WriteAll\|CreateTemp\|File.Delete\|Directory.Create"` yeni birim testlerinde 0); Consistency dosyası bağlantı kurmaz, ortam/yapılandırma okumaz (`TestIsolationScan` yeşil) |
| `09 §25` — veri/göç | uygulanamaz | Varlık, göç, DB varsayılanı, FK dizini yok |
| Sık FAIL nedenleri | ✓ | Aynı değeri anan yerler: `DatabaseOptionsValidator` (sabit adı), `09 §10.7`, Compose, `CONTEXT.md`, status/backlog/changelog/memory birlikte güncellendi; tarama istisnası dar (tek dosya yolu, tam eşleşme; `Jobs/DatabaseConnection.cs` gibi benzer ad yakalanır: `Applies_LeavesOutOnlyTheDatabaseConnectionFile`); her kuralın ayrı ihlal örneği; birim testi dosya yazmıyor; DB varsayılanı/FK uygulanamaz; modül: altyapı (`Infrastructure/`) |
| Güvenlik taraması | ✓ | Diff'te sır/parola yok (Compose dizisi parolasız, parola `PGPASSWORD`'den); girdi doğrulama yüzeyi yok; yeni bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | 543/543 (düzeltme 1 sonrası; `Timeouts_AreTheRowsOfTheTable` kaldırıldı) |
| Integration | ✓ | PostgreSQL 18, konteyner `shopfolio-t233-pg` (127.0.0.1:55233); `Database` ad alanı 35/35 (yeni 6 dahil), `Web`+`FileSecrets` 58/58, `Model` 893 geçti, 29 "Test Collection Cleanup Failure" (`SettingsTablesTests` ve `StartupDataReapplyTests` sınıf veritabanını düşürürken zaman aşımı; hiçbir test gövdesi kırmızı değil — D-68'in bilinen deseni, T233'e bağlı değil) | yeni testler tek başına: 6/6 (~55 sn gerçek bekleme) |
| Biçim ve derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` (Debug ve Release); `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; format çıkış 0 |
| Contract | — | yok | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 2 (2026-11-05, bağımsız doğrulayıcı; dal head `cbdfe10`). Tur 1: ✗ FAIL (2026-11-04, head `439361d`) |
| Bulgu sayısı | Tur 2: 0 · tur 1: 1 (DÜŞÜK, S1; düzeltme 1'de kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | Tur 2: 1 — K1 D-85'e forward · tur 1: 4 — K1 D-84'e forward; K2, K3, K4 düzeltme 1'de kapandı |

### Doğrulama tur 2 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Adlı sabitler; değerin tek yeri `09 §3.5` | ✓ | Altı sabit `internal static`, adlar madde 1'le birebir; `DatabaseTimeoutsConsistencyTests` iki tablo satırını ayrıştırıp sabitle karşılaştırır (`StatementTimeout` 16 sn bozması → `WebRowOfTheTable_EqualsTheWebConstants` kırmızı). B1 kapandı: `DatabaseConnectionTests` beklenen diziyi sabitlerden kurar, `Timeouts_AreTheRowsOfTheTable` silindi; `DatabaseSessionTimeoutTests` `pg_settings` milisaniyesini sabitle karşılaştırır, alt sınırlar `sabit − Slack`. Testlerde kalan sayılar yalnız girdi (`statement_timeout=0`, `Command Timeout=1`) ve tolerans (`Slack`). Sabitlerin özet yorumları değeri yineler → K1 (D-85) |
| 2 | Tek kuruluş noktası | ✓ | `src/`'de `Options`/`CommandTimeout` yazan, `NpgsqlDataSource.Create`/`UseNpgsql` çağıran yerler yalnız `DatabaseConnection`, `Program.cs` (`WebConnectionString` → `Create(…, Web)`), `AppDbContext.Configure` ve bağlantısız tasarım zamanı yer tutucusu. K2 kapandı: `Program.cs`'te veri kaynağı ham diziye çevrilince `DatabaseConnections_OfTheWebHost_CarryTheWebSessionProfile` kırmızı |
| 3 | İki profil, ayrı dizi, `SET` yok, tarama dar | ✓ | İstisna tek dosya tam yolu (`Applies` testi `Jobs/DatabaseConnection.cs`'i taranır sayar); test projeleri taranmaz; `src/`'e konan `SET statement_timeout = 0` ve `SET LOCAL lock_timeout` → `ApplicationSource_ChangesNoSessionTimeoutOutsideDatabaseConnection` kırmızı; Compose göç dizisi `Maintenance` ile eşitlik testli; iki profilin eşzamanlı oturumu farklı `statement_timeout` |
| 4 | İstemci süresi kilit ve sorgu sürelerinden uzun; komut başına süre yok | ✓ | Theory her iki profilde kilit < sorgu < istemci; `CommandTimeout` ataması yalnız `DatabaseConnection`. Cümle değişikliği (D-83): `09 §10.7` madde 2–3, madde 7 çelişki taraması ve `11` kriter 4/KK4 aynı ölçüde; boşta işlem süresi komutlar arası bekleme olduğundan istemci komut süresiyle karşılaştırılmaz — gerekçeli ve `§3.5` tablosuyla tutarlı |
| 5 | Aşım beklenmeyen hâle düşer; yakalanmaz; gövdede dış çağrı yok | ✓ | Gerçek PostgreSQL 18: 55P03, 57014 (istemci süresinden önce), boşta işlemde bağlantı kaybı; `src/`'de `PostgresException`/`SqlState` yok; `src/`'e konan `RunAsync(… http.GetAsync …)` → `ApplicationSource_UsesNoOutsideServiceInsideATransactionBody` kırmızı. Yanıt kolu D-84 (T212) |
| 6 | Yeniden deneme kapalı | ✓ | `AppDbContext_AsConfigured_HasNoRetryStrategy` + tarama `EnableRetryOnFailure` kuralı |
| 7 | Npgsql adları sürüme karşı | ✓ | Kilit dosyası Npgsql 10.0.3; adlar derleniyor; sunucuda değerler görünüyor; PR #384 açıklamasında "ad farkı yok" |

Testler (validator, yerel): build `-warnaserror` 0 uyarı · `dotnet format --verify-no-changes` 0 · Unit 543/543 (Consistency dahil) · betik testleri 4/4 · Integration 987/987 (Database 35, Web 35, FileSecrets 24, Model 893 — sınıf gruplarıyla; tek komutluk `Model` koşusunda yerel Npgsql bağlantı/temizlik zaman aşımları (D-68 deseni), aynı sınıflar gruplarla ve tek başına yeşil; dal CI Integration yeşil) · `restore --locked-mode` 0 · `12_matris_denetle.py` hata 0. Dal CI `41913509367` success (head `cbdfe10`). `12`'de T233'e eşlenen satır yok.

**K-notları (tur 2):** K1 → D-85: `DatabaseTimeouts` özet yorumları değeri ("5 seconds" …) yineler; eşitlik denetimi yorumu görmez.

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Adlı sabitler; değerin tek yeri `09 §3.5` | ~ | Altı sabit adlarıyla `09 §10.7` madde 1'dekiyle birebir, `internal static`; Consistency testi tablo satırını ayrıştırır, sabit değişince kırmızı (`StatementTimeout` 16 sn bozması → `WebRowOfTheTable_EqualsTheWebConstants` kırmızı). **B1:** testler sayıyı kendileri yazar — `DatabaseConnectionTests` `"-c lock_timeout=5000 -c statement_timeout=15000 …"`, `20`, `600000`, `605`, `Timeouts_AreTheRowsOfTheTable` (tablonun ikinci kopyası); `DatabaseSessionTimeoutTests` `"5s"`, `"15s"`, `"10min"`, `"30s"` ve alt sınırlar `4`, `14` sn. Madde 1: "Sayı başka yerde yazılmaz: bağlantı dizisi, test ve canlılık denetimi bu sabitlere başvurur" |
| 2 | Tek kuruluş noktası | ✓ | `DatabaseConnection.Configure` imzası madde 2'yle birebir; `src/`'de `Options`/`CommandTimeout` yazan başka yer yok (grep); `Program.cs` veri kaynağı ve bağlam `WebConnectionString` → `DatabaseConnection.Create(…, Web)`; tasarım zamanı fabrikası bağlantısız yer tutucu (K2: web ev sahibinin profil bağlantısını koruyan test yok) |
| 3 | İki profil, ayrı dizi, `SET` yok, tarama dar | ✓ | Tarama istisnası tam yol eşleşmesi (`Jobs/DatabaseConnection.cs` taranır); test projeleri taranmaz (madde 2 "testlerde"); `src/`'e konan `SET statement_timeout = 0` ve Compose'ta `statement_timeout=15000` bozmaları ilgili testleri kırmızıya çevirdi; Integration iki profilin aynı anda açık oturumunda `SHOW` farklı |
| 4 | İstemci süresi sunucu sürelerinden uzun; komut başına süre yok | ✓ | Kilit < sorgu < istemci her iki profilde (Theory); `CommandTimeout` ataması yalnız `DatabaseConnection`; boşta işlem 30 sn > 20 sn `09`'un kendi çelişkisi (§3.5 tablosu ↔ madde 2 cümlesi), D-83'te kayıtlı (K3) |
| 5 | Aşım beklenmeyen hâle düşer; yakalanmaz; gövdede dış çağrı yok | ✓ | 55P03 (4–20 sn), 57014 (14–20 sn, istemci süresinden önce), boşta işlemde bağlantı kaybı gerçek PostgreSQL 18'de; `src/`'de `catch` ile `SqlState`/`PostgresException` yok; `src/`'e konan `RunAsync(… http.GetAsync …)` bozması gövde taramasını kırmızıya çevirdi. Yanıt katmanı (`§8.1`) henüz yok → K1 |
| 6 | Yeniden deneme kapalı | ✓ | `AppDbContext`'e `EnableRetryOnFailure` bozması hem taramayı hem `AppDbContext_AsConfigured_HasNoRetryStrategy`'yi kırmızıya çevirdi |
| 7 | Npgsql adları sürüme karşı | ✓ | `packages.lock.json` Npgsql çözülen 10.0.3; adlar derleniyor ve `SHOW` sunucuda görüyor; PR açıklamasında "ad farkı yok" |

Testler (validator, yerel): build `-warnaserror` 0 uyarı · `dotnet format --verify-no-changes` 0 · Unit 544/544 (Consistency dahil) · betik testleri 4/4 · Integration 986/986 (ad alanı ve sınıf gruplarıyla bölünmüş; `AdminAndAuditTablesTests`'te bir yerel Npgsql zaman aşımı, sınıf tek başına 42/42) · `restore --locked-mode` 0 · `12_matris_denetle.py` hata 0. Dal CI `41907477152` success (head `439361d`). `12`'de T233'e eşlenen satır yok (kaynak birimi `09 §10.7`, §3.5; KP yok).

**B1 (DÜŞÜK, S1):** testlerdeki sayılar sabitlerden türetilir (ör. `$"-c lock_timeout={(int)DatabaseTimeouts.LockTimeout.TotalMilliseconds} …"`, `SHOW` yerine `current_setting`/`pg_settings` milisaniyesi sabitle karşılaştırılır); `Timeouts_AreTheRowsOfTheTable` kaldırılır (tablo ↔ sabit eşitliği Consistency testindedir).

**K-notları:** K1 → D-84 (yanıt katmanı T212'de). K2: `Program.cs`'te web ev sahibinin `Web` profili yerine ham kurulum dizisi kullanılması (bozma) Unit 544/544 ve Web 34/34'ü yeşil bıraktı — ev sahibinin kurduğu veri kaynağında `statement_timeout`'u sınayan test eklenir. K3: D-83'ün hedefi bu PR'ın kendisiydi (`09 §10.7`'ye dokundu); madde 2 cümlesi düzeltme 1'de daraltılır ve D-83 kapanır. K4: D-26 madde 7'yi "en çok bir `DbTimeout` satırı" diye daralttı ama `Get_WhenTheServerIsUnreachable_Returns503WithinTheLimit` satır sayısını sınamıyor — tek onay eklenir.

**Yapım raporu karşılaştırması:** 2 uyuşmazlık — (1) kriter 1 için "sayı başka yerde yazılmaz" denmiş, kanıt olarak gösterilen `Timeouts_AreTheRowsOfTheTable` kuralın ihlalidir; (2) KK2 kanıtı yalnız grep, korunma testi yok (K2). Yerel Integration'da yapım "29 Test Collection Cleanup Failure" yazdı; validator koşusunda yok (bir test gövdesi zaman aşımı, tek başına yeşil). Öz denetim tablosu var ve her maddesi kanıtlı.

## Düzeltme Turu 1 (2026-11-05)

Kapanan: B1, K2, K3, K4 (K1 → D-84, dokunulmadı).

- **B1:** `DatabaseConnectionTests` beklenen bağlantı dizisini ve istemci süresini `DatabaseTimeouts` sabitlerinden kurar (`ExpectedOptions`, `Seconds`); `Timeouts_AreTheRowsOfTheTable` silindi (tablo ↔ sabit eşitliğini `DatabaseTimeoutsConsistencyTests` sağlar). `DatabaseSessionTimeoutTests` sunucu değerini `pg_settings` milisaniyesinden okur ve sabitle karşılaştırır; alt sınırlar `LockTimeout - Slack`, `StatementTimeout - Slack`.
- **K2:** `ShopfolioWebApplicationFactoryTests.DatabaseConnections_OfTheWebHost_CarryTheWebSessionProfile`: ev sahibinin `NpgsqlDataSource` ve `AppDbContext` bağlantı dizisi `Web` profilinin `Options` ve `CommandTimeout` değerine eşit, sunucuda `statement_timeout` Web sabitine eşit. Bozma sınaması: `Program.cs`'te `WebConnectionString` ham diziye çevrildi → test kırmızı; geri alındı.
- **K3:** `09 §10.7` madde 2 ve 3 ile `11` T233 kriter 4 ve KK4 "kilit ve sorgu sürelerinden uzundur" diye daraltıldı; D-83 ✅.
- **K4:** `Get_WhenTheServerIsUnreachable_Returns503WithinTheLimit` en çok bir `DbTimeout` satırı onayı aldı.

Elle yazılmış süre sayısı denetimi: `grep -rnE` ile bütün `tests/` tarandı; kalanlar yalnız girdi (`Options=-c statement_timeout=0;Command Timeout=1`, ihlal örneği `SET statement_timeout = 600000`) ve Consistency testinin doküman tablosundan ayrıştırdığı değerler.

Testler: build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 0; Unit 543/543; Integration (`DatabaseSessionTimeoutTests`, `ShopfolioWebApplicationFactoryTests`, `LivenessEndpointTests`) 24/24.

### Öz denetim — düzeltme turu (11 T233 k1–k7, KK1–KK7; 12 ilgili kontrol listesi)

| Madde | Sonuç | Kanıt |
|---|---|---|
| k1/KK1: sabitler `09 §3.5` ile bire bir; sayı başka yerde yazılmaz | ✓ | `DatabaseTimeouts.cs`; `DatabaseTimeoutsConsistencyTests` (tablo ↔ sabit); testlerde sabitten türetme (`ExpectedOptions`, `ServerTimeAsync`); `grep -rnE` ile elle yazılmış süre sayısı yok |
| k2/KK2: tek kuruluş noktası; ev sahibi Web profilini kullanır | ✓ | `DatabaseConnection.Configure`; `DatabaseConnections_OfTheWebHost_CarryTheWebSessionProfile` (bozma → kırmızı) |
| k3/KK3: Maintenance uzun sorgu; iki dizi ayrı; `SET` taraması | ✓ | `Configure_MaintenanceProfile_*`, `Show_TheTwoProfiles_AreNotOneSessionSetting`, `MigrationServiceConnectionInTheComposeFile_ParsesToTheMaintenanceTimes`, `ApplicationSource_ChangesNoSessionTimeoutOutsideDatabaseConnection` |
| k4/KK4: istemci süresi kilit ve sorgu sürelerinden uzun | ✓ | `Configure_EachProfile_LeavesTheClientTimeLongerThanTheLockAndStatementTimes`; `09 §10.7` madde 2-3 ve `11` aynı cümleyle |
| k5/KK5: aşım ham hatayla yukarı çıkar; gövdede dış çağrı yok | ✓ | `LockWait_*`, `LongQuery_*`, `IdleOpenTransaction_*`; yanıt katmanı D-84 |
| k6/KK6: yeniden deneme kapalı | ✓ | `AppDbContext_AsConfigured_HasNoRetryStrategy` |
| k7/KK7: Npgsql ad farkı PR açıklamasında | ✓ | PR #384 açıklaması (Npgsql 10.0.3) |
| Sık FAIL: beklenen değer sabitten türer | ✓ | `DatabaseConnectionTests`, `DatabaseSessionTimeoutTests` |
| Sık FAIL: kural değişince öteki yerler | ✓ | "sunucu sürelerinden uzun" cümlesi `09` (2 yer), `11` (2 yer), D-83, bu rapor; `grep -rn "o profilin sunucu"` boş |
| Sık FAIL: tarama istisnası geniş / DB varsayılanı / FK dizini | ✓ / uygulanamaz | Tarama istisnası değişmedi (tek dosya yolu); şema/göç yok |
| `09 §25` — test | ✓ | Yeni test ayrı ihlal örneğiyle sınandı (Program.cs bozması kırmızı); birim testleri dosya yazmaz |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (yeni kurulum ayarı yok); `deploy/compose.yaml` `migrate` bağlantı dizisine `Options` ve `Command Timeout` anahtarları eklendi
- **Paketleme / dağıtım:** Compose `migrate` hizmeti değişti (yukarıdaki satır)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T233-veritabani-oturum-sureleri`
- **Commit:** *(PR açıklamasında head SHA)*
- **PR:** #384
- **CI:** yardımcı izler
- **Post-merge run'lar:** squash `810fe27` (#384) — CI `41917927066` ve Publish `41917927000` success (T205 dalında dolduruldu, 2026-11-05)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `NpgsqlConnectionStringBuilder.Options` ve `.CommandTimeout` Npgsql 10.0.3'te var ve sunucuya `-c` ayarı olarak gider | Kilit dosyası `packages.lock.json` `Npgsql` 10.0.3; `Show_EachProfile_GivesTheServerTimesOfThatProfile` gerçek PostgreSQL 18'de `5s/15s/30s` ve `5s/10min/30s` gösterdi | ✓ |
| 2 | Npgsql, boşluk içeren `Options=-c ... -c ...` değerini tırnaksız bağlantı dizisinde okur (Compose dizisi tırnaksız) | `MigrationServiceConnectionInTheComposeFile_ParsesToTheMaintenanceTimes` | ✓ |
| 3 | Sunucu iptali istemci süresinden önce gelir | `LongQuery_...` 14–20 sn aralığında 57014 | ✓ |
| 4 | Windows'ta `localhost` yerel PostgreSQL konteynerine IPv6 denemesiyle 2 sn'den uzun sürebilir (canlılık testleri 503 verir) | `Host=localhost` ile 3 canlılık testi kırmızı, `Host=127.0.0.1` ile 58/58 yeşil; CI `localhost` kullanır ve etkilenmez (Linux) | ✓ (ortam notu) |
| 5 | Başka dış varsayım yok (plan katmanı, kota, anahtar, bölge) | 4a tarama terimleri task metninde yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `ITransactionRunner` gövde taraması çalıştırıcı yazılana kadar gerçek çağrı bulmaz; yöntem grubu izlenmez | D-82 | Çalıştırıcı sonraki task'ın; taramayı o PR'da sınamak gerekir |
| 2 | `09 §10.7` madde 2 cümlesi boşta işlem süresiyle çelişir | D-83 | ✅ Düzeltme 1de kapandı (09 §10.7 madde 2-3 ve 11 T233 k4 ve KK4 daraltıldı) |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 41903408101 (Publish, `b8f20ad`) success, 41903408080 (CI, `b8f20ad`) success, 41893977627 (Publish, `15daf95`) success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ Kararlar (sahibe sorulmadan, kurala en uygun seçenekle):**
  - (a) KK4/kriter 4 "sunucu sürelerinden uzun" cümlesi kilit ve sorgu süreleri için uygulandı; `Web`'de boşta işlem süresi (30 sn) istemci süresinden (20 sn) uzundur ve `09 §10.7` madde 7'nin çelişki taraması da yalnız kilit < sorgu < istemciyi ister (D-83).
  - (b) Göç hizmetinin `Maintenance` süreleri Compose bağlantı dizisinde yazılıdır (demet `efbundle`, kod değil); sayıların ikinci yazılışı eşitlik denetimiyle bağlandı. Alternatif (demetin kendisinin profili uygulaması) `dotnet ef` demetini değiştirmeyi gerektirirdi.
  - (c) `TransactionBodySourceScan` `09 §10.7` madde 5'in "`Infrastructure/` dışında" sınırından geniştir: bütün `src/` taranır (istisnasız, daha sıkı).
  - (d) Hata kodu (55P03/57014/25P03) anan kodu yasaklayan kural ve `EnableRetryOnFailure` yasağı `DatabaseSessionSourceScan`'e eklendi (madde 4'ün "yakalamaz, çevirmez, yeniden denemez" cümlesinin taraması).
  - (e) Backlog'un T233'e hedeflediği D-22 ve D-26 `09 §10.7` metin düzeltmeleriyle kapatıldı; D-22 için "süreyi `TimeProvider`'lı yapma" yolu seçilmedi (kod değişmez, istisna cümlesi yazıldı).
  - (f) Yönetim komutları kipinin `Maintenance` kullanması (`09 §13.2` madde 6; T303 KK4) bu task'ta kodlanmadı; profil hazır.
- **Diğer:** Integration `Host=localhost` yerine `127.0.0.1` ile koşuldu (Windows IPv6 gecikmesi, Dış Varsayım 4).
