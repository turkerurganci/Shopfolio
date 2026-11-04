# T233 — Veritabanı oturum süreleri

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

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
| 1 | Süre değerleri `DatabaseTimeouts`'ta adlı sabit; tek yer `09 §3.5` | ✓ | `DatabaseTimeouts.cs`; `Timeouts_AreTheRowsOfTheTable`; `WebRowOfTheTable_EqualsTheWebConstants`, `MaintenanceRowOfTheTable_EqualsTheMaintenanceConstants` (tablo satırını okur) |
| 2 | Bağlantı kuruluşu tek yerde: üç sunucu ayarı ve istemci süresi | ✓ | `DatabaseConnection.Configure`; `Configure_WebProfile_WritesTheThreeServerTimesAndTheClientTime`; `Program.cs` `WebConnectionString`; `Show_EachProfile_GivesTheServerTimesOfThatProfile` (gerçek sunucu) |
| 3 | İki profil; ayrı bağlantı dizisi (ayrı havuz); `SET` yok; tarama yalnız `DatabaseConnection`'da ve testlerde bulur | ✓ | `Create_TheTwoProfiles_AreTwoConnectionStrings`, `Show_TheTwoProfiles_AreNotOneSessionSetting`; `ApplicationSource_ChangesNoSessionTimeoutOutsideDatabaseConnection`, `SessionScan_OnViolationSample_...`, `Applies_LeavesOutOnlyTheDatabaseConnectionFile`; Compose: `MigrationService_ConnectsWithTheMaintenanceProfile` |
| 4 | İstemci süresi her profilde sunucu sürelerinden uzun; komut başına süre yok | ✓ (⚠ boşta işlem süresi hariç) | `Configure_EachProfile_LeavesTheClientTimeLongerThanTheLockAndStatementTimes`; `CommandTimeout` ataması yalnız `DatabaseConnection`'da (`grep -rn "CommandTimeout" src/` → `DatabaseConnection.cs`, `DatabaseTimeouts.cs`, `DatabaseOptionsValidator.cs`); boşta işlem 30 sn > istemci 20 sn: `09` kendi cümlesiyle çelişir, D-83 |
| 5 | Aşım beklenmeyen hâle düşer; kod yakalamaz/çevirmez/yeniden denemez; `ITransactionRunner` gövdesinde dış çağrı yok | ✓ | `LockWait_...FailsWithTheServersLockError` (55P03), `LongQuery_...IsCancelledByTheServerBeforeTheClientTime` (57014, 14–20 sn), `IdleOpenTransaction_...LosesItsConnection`; hata kodu anan kod yok: `SessionScan_...` örnek satır 15; `ApplicationSource_UsesNoOutsideServiceInsideATransactionBody`, `TransactionBodyScan_OnViolationSample_...` |
| 6 | Otomatik yeniden deneme kapalı | ✓ | `AppDbContext_AsConfigured_HasNoRetryStrategy`; kırmızı gösterimi `AppDbContext_WithRetryOnFailureTurnedOn_ShowsTheCheckCanGoRed`; kaynakta `EnableRetryOnFailure` taraması (örnek satır 14) |
| 7 | `Options` ve `CommandTimeout` adları Npgsql sürümüne karşı doğrulandı | ✓ | Kilit dosyası `src/Shopfolio.Web/packages.lock.json`: `Npgsql` 10.0.3 (çözülen); `NpgsqlConnectionStringBuilder.Options` ve `.CommandTimeout` bu sürümde derlenir; `Show_EachProfile_...` sunucuya gerçekten gittiğini gösterir; fark: PR açıklamasında |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1/KK1: sabitler `09 §3.5` satırlarıyla bire bir | ✓ | `DatabaseTimeouts.cs` altı sabit; tablo satırını ayrıştıran `DatabaseTimeoutsConsistencyTests` iki testi (Lint adımında koşar); `Timeouts_PassTheContradictionScanOfTheTable` |
| k2/KK2: tek kuruluş noktası; başka yerde bağlantı dizgesi kurulmuyor | ✓ | `grep -rn "NpgsqlDataSource.Create\|UseNpgsql(\|new NpgsqlConnectionStringBuilder" src/` → `Program.cs` (yalnız `DatabaseConnection.Create` çıktısıyla), `AppDbContext.Configure` (dizeyi alır), `DatabaseConnection.cs`, `DatabaseOptionsValidator.cs` (yalnız ayrıştırma sınaması), `AppDbContextDesignTimeFactory` (bağlantısız yer tutucu, tasarım zamanı); `Options`/`CommandTimeout` yazan başka yer yok |
| k3/KK3: Maintenance daha uzun sorgu süresi; göç hizmeti `Maintenance` ile bağlanır; iki dizi ayrı; `SET statement_timeout` taramayı kırar | ✓ | `Configure_MaintenanceProfile_*`, `Show_EachProfile_*` (Maintenance `10min`), `MigrationService_ConnectsWithTheMaintenanceProfile` + `MigrationServiceConnectionInTheComposeFile_ParsesToTheMaintenanceTimes` + `MigrationServiceCheck_WithTheWebProfile_WouldDiffer`; tarama kırılışı `SessionScan_OnViolationSample_...` (satır 5, 6, 9) ve `SET LOCAL`/`set_config` (satır 7, 8). Yönetim komutu `CommandHost` T303 KK4'ündür |
| k4/KK4: istemci süresi her iki profilde sunucu sürelerinden uzun | ✓ (kilit ve sorgu; boşta işlem ⚠) | `Configure_EachProfile_LeavesTheClientTimeLongerThanTheLockAndStatementTimes` (Theory, iki profil); boşta işlem Web'de 30 > 20: D-83 |
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
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | 544/544 (Unit adımı `--filter-not-trait "Consistency=true"`: 433; Consistency: 111) |
| Integration | ✓ | PostgreSQL 18, konteyner `shopfolio-t233-pg` (127.0.0.1:55233); `Database` ad alanı 35/35 (yeni 6 dahil), `Web`+`FileSecrets` 58/58, `Model` 893 geçti, 29 "Test Collection Cleanup Failure" (`SettingsTablesTests` ve `StartupDataReapplyTests` sınıf veritabanını düşürürken zaman aşımı; hiçbir test gövdesi kırmızı değil — D-68'in bilinen deseni, T233'e bağlı değil) | yeni testler tek başına: 6/6 (~55 sn gerçek bekleme) |
| Biçim ve derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` (Debug ve Release); `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; format çıkış 0 |
| Contract | — | yok | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (yeni kurulum ayarı yok); `deploy/compose.yaml` `migrate` bağlantı dizisine `Options` ve `Command Timeout` anahtarları eklendi
- **Paketleme / dağıtım:** Compose `migrate` hizmeti değişti (yukarıdaki satır)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T233-veritabani-oturum-sureleri`
- **Commit:** *(PR açıklamasında head SHA)*
- **PR:** #PRNO
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

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
| 2 | `09 §10.7` madde 2 cümlesi boşta işlem süresiyle çelişir | D-83 | Doküman düzeltmesi sahibin kararı |

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
