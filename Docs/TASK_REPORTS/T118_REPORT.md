# T118 — Zamanlayıcı, yedek, imha ve anahtar kayıtları

**Faz:** F1 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- Altı entity (`06 §4.59`, §4.64–§4.68): `Job`, `HeartbeatRecord`, `OutageRecord` → `src/Shopfolio.Web/Infrastructure/Jobs/`; `DisposalRecord` → `Infrastructure/Disposal/`; `BackupStatusRecord` → `Infrastructure/Backup/`; `DataProtectionKey` paketin kendi türüdür (`AppDbContext : IDataProtectionKeyContext`, `DbSet<DataProtectionKey> DataProtectionKeys`). Alanlar `06` alan tablolarıyla birebir; enum'lar (`JobType`, `JobState`, `DisposalRecordType`) metin + izinli değer kısıtı.
- Varsayılanlar veritabanı default'udur: `JobState` `'Pending'`, `AttemptCount` `0` (varlıkta `HasDefaultValue`, göçte `defaultValue`).
- Tek göç `20261104124425_CreateSchedulerBackupDisposalAndKeyTables` (`Down` yok): altı tablo; `jobs` kısıtları (`completed_at_with_state`, `attempt_count_not_negative`, `related_record_by_type`), bekleyen satırlarda (`job_type`, `related_record_id`) filtreli tekil dizin `NULLS NOT DISTINCT` (K-1241) ve üç filtreli dizin (`06 §4.59` İndeksler); `disposal_records` türe göre sayı/tarih `CASE` kısıtı ve `range_end_not_before_start`, `created_at` dizini; `outage_records` `ended_at - started_at > interval '2 minutes'`; `heartbeat_records` ve `backup_status_records` için `((true))` tek satır dizini (elle SQL); `data_protection_keys` paketin şeması (tamsayı kimlik, `friendly_name`, `xml`; `created_at` yok).
- Yetki (`09 §10.8` madde 3): göç `REVOKE SELECT ON data_protection_keys FROM current_setting('shopfolio.reader_user')` ve `GRANT UPDATE (last_succeeded_at, last_failed_at) ON backup_status_records TO current_setting('shopfolio.backup_status_user')` — adlar rol ayarından, `format('%I')` ile.
- **D-39 (a):** `deploy/images/database/database-job.sh` sunucu dışı yedeğin sonucunu yerel soketle (`--host=/var/run/postgresql`, parolasız `peer`) `shopfolio_backup_status` olarak yazar: başarıda `UPDATE public.backup_status_records SET last_succeeded_at = now()`, `stanza-create` ya da `--repo=2` düşerse `… last_failed_at = now()` ve pgBackRest'in koduyla çıkış; başarı yazımı düşerse yerel kopya yine alınır, iş psql'in koduyla çıkar. `tests/scripts/stubs/psql` `STUB_FAIL_ON` ve `STUB_ARGS` desteği aldı; `tests/scripts/database-job.test.sh` dört senaryo (başarı, dış depo hatası, stanza hatası, durum yazımı hatası).
- Yeni paket `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.14 (`Directory.Packages.props`, Web csproj, üç kilit dosyası); `09` Kütüphaneler satırı lisans/sürüm kanıtıyla güncellendi, `LibraryTableConsistency.NotYetAdded`'dan çıkarıldı; `09 §5.2` madde 4'e sınama notu.
- Test güncellemeleri: `DatabaseUsersTests.PrivilegeMatrix_…` artık gerçek tabloları kullanır (eski hâli iki tabloyu kendisi `CREATE` ediyordu — çakışırdı); `EnumDatabaseTests` enum sütunu 59 → 62; `ModelRuleTests.SampleModel_GuidKeysAreNeverGeneratedByEf_…` yalnız `Guid` anahtarlara bakar (paketin tamsayı anahtarı `06 §4.68`'in tek istisnası).
- `.claude/CONTEXT.md` Infrastructure satırına üç alt klasör; T117 raporunda "Post-merge run'lar" dolduruldu; T117 durum girişi `STATUS_CHANGELOG.md`'ye taşındı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/{Jobs/Job,Jobs/HeartbeatRecord,Jobs/OutageRecord,Disposal/DisposalRecord,Backup/BackupStatusRecord}.cs` (yeni), `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `Directory.Packages.props`, `src/Shopfolio.Web/Shopfolio.Web.csproj`, `src/Shopfolio.Web/packages.lock.json`, `tests/*/packages.lock.json`.
- `deploy/images/database/database-job.sh`, `tests/scripts/database-job.test.sh`, `tests/scripts/stubs/psql`.
- `tests/Shopfolio.IntegrationTests/Model/{SchedulerBackupDisposalAndKeyTablesTests (yeni),EnumDatabaseTests,ModelRuleTests}.cs`, `tests/Shopfolio.IntegrationTests/Database/DatabaseUsersTests.cs`, `tests/Shopfolio.UnitTests/Consistency/LibraryTableConsistency.cs`.
- `Docs/09_CODING_GUIDELINES.md` (kütüphane satırı, §5.2 m.4), `Docs/DEFERRED_BACKLOG.md` (D-39 (a)), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T117_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

Öz denetim (PR öncesi): her kriter ve kontrol listesi maddesi göç satırı (`grep -n` ile `20261104124425_…cs` satır 15–160) ve test adıyla tek tek karşılaştırıldı. Sık FAIL nedenleri ayrıca denetlendi: "Varsayılan" sütunu DB default'u (`Columns_HaveTheDatabaseDefaultsOfTheDefaultColumns`, `NewJob_WithoutStateAndCount_TakesTheDatabaseDefaults`); altı tabloda FK yok, dolayısıyla FK dizini gerekmez (`SixTables_HaveNoForeignKey`); filtreli tekil dizin `06 §4.59` ile aynı (`job_state = 'Pending'`, `NULLS NOT DISTINCT`; `Indexes_AreTheDocumentedOnes`); kontrol kısıtlarında `NULL` sızıntısı yok — her kısıtın sol tarafı `NOT NULL` sütundur (`job_state`, `job_type`, `disposal_record_type`), `CASE`'in `ELSE` dalı `count IS NOT NULL AND count > 0` ister, `range_end_date >= range_start_date`'in `NULL` geçişi yalnız iki tarihin de boş olması gereken türlerdedir; yetki adları göçte sabit yazılmaz (`current_setting`); yeni paket kilit dosyasında (`dotnet restore Shopfolio.slnx --locked-mode` çıkış 0) ve lisans satırında; bekçi/tarama istisnası eklenmedi. Öz denetimde bulunup düzeltilen: `ModelRuleTests`'in örnek modeli bütün anahtarları `Never` bekliyordu, paketin tamsayı anahtarıyla kırıldı → yalnız `Guid` anahtarlar; kütüphane tablosu tutarlılık testi `NotYetAdded` listesinden çıkarmayı istedi → çıkarıldı.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Altı entity `06 §4.59`, §4.64–§4.68 alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheSixTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `Columns_HaveTheDatabaseDefaultsOfTheDefaultColumns`, `Indexes_AreTheDocumentedOnes`, `CheckConstraints_AreTheDocumentedOnes`; göç `20261104124425_CreateSchedulerBackupDisposalAndKeyTables`; `dotnet ef migrations has-pending-model-changes` → "No changes" |
| 2 | `Job` bekleyen (tür, ilgili kayıt) tekil `NULLS NOT DISTINCT`; ilgili kayıt yalnız `BounceQuery`'de boş; tamamlandı ↔ tamamlanma anı; deneme ≥ 0 | ✓ | `SecondPendingJob_OfTheSameTypeForTheSameRecord_IsRefused`, `SecondPendingBounceQuery_WithoutRelatedRecord_IsRefused_BecauseEmptyValuesCountAsEqual`, `PendingJob_NextToACompletedAndAFailedOne…_IsAccepted`, `JobWithoutRelatedRecord_OfATypeOtherThanTheBounceQuery_IsRefused`, `BounceQuery_WithARelatedRecord_IsRefused`, `JobWhoseCompletionMomentDoesNotFitItsState_IsRefused`, `CompletedJobWithItsMoment_IsAccepted`, `JobWithANegativeAttemptCount_IsRefused_AndZeroIsAccepted`; göç satır 94–98, 139–145 |
| 3 | `HeartbeatRecord` ve `BackupStatusRecord` tek satır | ✓ | `SecondHeartbeatRecord_IsRefused`, `SecondBackupStatusRecord_IsRefused`; göç satır 149–150 (`((true))` tekil dizin) |
| 4 | `OutageRecord` > 2 dk | ✓ | `OutageOfTwoMinutesOrLess_IsRefused`, `OutageLongerThanTwoMinutes_IsAccepted`; göç satır 113 |
| 5 | `DisposalRecord` sayı/tarih türe göre; sayı > 0; bitiş ≥ başlangıç | ✓ | `DisposalRowWhoseNumberAndDatesDoNotFitItsType_IsRefused`, `ServerLogRotation_WhoseRangeEndsBeforeItStarts_IsRefused`, `CountedRowAndOneDayRotation_AreAccepted`; göç satır 57, 59 |
| 6 | `DataProtectionKey` paketin şeması: tamsayı anahtar, `created_at` yok | ✓ | `DataProtectionKeys_KeyIsAnIntegerTheDatabaseGenerates_AndThereIsNoCreatedAt`, `DataProtectionKey_InTheModel_IsThePackagesEntityUnderTheConventionsNames`; göç satır 29–41 |
| 7 | Göç okuyucudan `data_protection_keys` okumasını alır, durum kullanıcısına iki sütunda güncelleme verir; adlar rol ayarından | ✓ | `Reader_ReadingTheKeyRing_IsRefused_ButItReadsTheBackupStatus`, `BackupStatusUser_UpdatesTheTwoMomentsWithTheJobsStatements`, `BackupStatusUser_AnythingButTheTwoMoments_IsRefused` (8 deyim), `Migration_WithoutARoleSettingItReads_IsRolledBack_AndWithItGrantsTheRightUsers`, `DatabaseUsersTests.PrivilegeMatrix_OnTheTablesOfTheDatabase_HasNoDeviation`; göç satır 155–160 |
| 8 | Göç tek konu; `Down` yok | ✓ | `Migration_HasNoDownMethod`; göçte `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (grep; `now()` yalnız betikte) |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 boş veritabanına altı tablo; `data_protection_keys` anahtarı tamsayı, `created_at` yok | ✓ | `Migration_MakesTheSixTables`, `DataProtectionKeys_KeyIsAnIntegerTheDatabaseGenerates_AndThereIsNoCreatedAt` (test veritabanı bütün göçlerle kurulur) |
| KK2 ikinci bekleyen iş; ikinci bekleyen geri dönen adres sorgusu; ilgili kaydı boş başka tür | ✓ | `SecondPendingJob_…`, `SecondPendingBounceQuery_…`, `JobWithoutRelatedRecord_OfATypeOtherThanTheBounceQuery_IsRefused` |
| KK3 tamamlanma anı dolu bekleyen iş; eksi deneme sayısı | ✓ | `JobWhoseCompletionMomentDoesNotFitItsState_IsRefused`, `JobWithANegativeAttemptCount_IsRefused_AndZeroIsAccepted` |
| KK4 ikinci nabız ve ikinci yedek durumu satırı | ✓ | `SecondHeartbeatRecord_IsRefused`, `SecondBackupStatusRecord_IsRefused` |
| KK5 iki dakikalık kesinti | ✓ | `OutageOfTwoMinutesOrLess_IsRefused` (tam 2 dk ve daha kısa) |
| KK6 sayısız/sıfır sayılı imha; bitişi başlangıçtan önce | ✓ | `DisposalRowWhoseNumberAndDatesDoNotFitItsType_IsRefused`, `ServerLogRotation_WhoseRangeEndsBeforeItStarts_IsRefused` |
| KK7 okuyucu anahtar halkasında reddedilir; durum kullanıcısı iki sütunu günceller, başkasında reddedilir | ✓ | `Reader_ReadingTheKeyRing_IsRefused_…`, `BackupStatusUser_UpdatesTheTwoMoments…`, `BackupStatusUser_AnythingButTheTwoMoments_IsRefused`, `BackupJob_WritesTheResultWithExactlyTheStatementsTheUserMayRun` (betiğin deyimleri testin deyimleriyle aynı) |
| KK8 kaynak taraması temiz | ✓ | Unit 245/245 (mimari ve kaynak taramaları, `MigrationSourceScan` dahil); üretim kodunda `Guid.NewGuid`, `DateTime.Now`, `UtcNow` yok; anahtar `EntityIds.New()`; yeni tarama istisnası yok |
| `09 §25` | ✓ | Aşağıdaki "09 §25" bölümü |
| Güvenlik taraması | ✓ | Sır yok (durum kullanıcısı parolasız `peer`, betik parola taşımaz); anahtar halkası okuyucuya kapalı, durum kullanıcısı yalnız iki sütun (`SELECT` yok); uygulama kullanıcısına yeni yetki yok; giriş noktası yok; yeni bağımlılık Microsoft paketi, MIT, NuGetAudit (tüm düzey, geçişli) temiz |

**`09 §25` (dokunulan alanlar):**

- Doküman uyumu: adlar `06 §4.59`, §4.64–§4.68 ve §5'ten birebir; "bilgim tarihli" paket olguları (tablo/sütun adları, tamsayı anahtar, lisans) sınandı ve `09`'a yazıldı.
- Yeni dosya düzeni: `Infrastructure/` altı (`05 §3.3.11` ortak altyapı: zamanlayıcı, iş kuyruğu, imha düzenleyicisi); `CONTEXT.md` güncel.
- Yeni paket: tablo satırı, kanıt, lisans (MIT, koşulsuz), sürüm `Directory.Packages.props`'ta, kilit dosyaları güncel.
- Veri ve göç: FK yok; anahtar `EntityIds`; enum'lar metin + izinli değer kısıtı; göç genişletme; `Down`, `suppressTransaction`, `CONCURRENTLY`, `now()`, `UPDATE`, `DELETE` yok; kullanıcı adları rol ayarından; yeni tablolar varsayılan yetkide, iki istisna (`data_protection_keys`, `backup_status_records`) `PrivilegeMatrix` ve testlerde.
- Uygulanamaz: geçiş/olay/işlem izi, para, önbellek, sayfalı liste, ön yüz, dış servis, POST/hız sınırı (yalnız şema ve betik).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 245/245 (Consistency dahil) |
| Betik | ✓ | `sh tests/scripts/run-script-tests.sh` | 4/4 dosya; `database-job.test.sh` 21 doğrulama |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-class …` (PostgreSQL 18, `shopfolio-t118-pg`; sınıf grupları ≤ 170 sn) | 926/926; T118 sınıfı 55/55 |
| Build | ✓ | CI `BUILD_CMD` (kilitli geri yükleme + NuGetAudit tüm düzey, `-c Release -warnaserror`) | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |

Not: Model ad alanının tamamı tek koşuda 863 testle koşulunca yerel Docker'da bağlantı zaman aşımları görüldü (her koşuda başka testler; `TimeoutException` bağlantı açılışında); aynı sınıflar gruplar hâlinde hatasız geçti. Kod hatası değil, yerel ortamın eşzamanlı bağlantı yükü.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `01d6473`, PR #376) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Kapılar:** working tree temiz; `core.hooksPath` mutlak yolla `scripts/git-hooks` (ana checkout'un hook'ları; eşdeğer); ana dal son 3 run `41830794202` (Publish), `41830794252` (CI) — T117 squash —, `41825046782` (Publish, T116) success; `MEMORY.md`'de T118 satırı var (satır 22).

**Kabul kriterleri:** kapsam tam (8 kriter, KK1–KK8, `09 §25`, güvenlik); hepsi ✓. Validator göçü, beş yapılandırmayı ve bağlamı `06 §4.59`, §4.64–§4.68, §2.2, §2.3.1–§2.3.2, §2.4, §2.9, §4'ün girişi, §5.4.4–§5.4.5, §5.5.3, §7.1–§7.5 ve `09 §10.1`, §10.6, §10.8 satırlarına karşı elle okudu:
- Kriter 1: altı tablo tek göçte (`20261104124425_CreateSchedulerBackupDisposalAndKeyTables`); sütun, tip ve boşluk alan tablolarıyla birebir (`id`/`created_at` ortak, anahtar halkasında yok); varsayılanlar yalnız `job_state 'Pending'` ve `attempt_count 0` (DB default); dizinler `06 §7.1`, §7.5 satırlarıyla eşit — `jobs` üç filtreli dizin (`Pending` kuyruk, `Failed`, `Completed`), `disposal_records.created_at`, iki tek satır dizini, kesinti ve anahtar halkasında yalnız birincil anahtar; altı tabloda FK yok (`06 §7.2.3`); enum kısıtları `JobType` 6, `JobState` 3, `DisposalRecordType` 24 değerle `06 §5` tablolarıyla birebir.
- Kriter 2: `(job_type, related_record_id)` tekil dizini `NULLS NOT DISTINCT` ve `job_state = 'Pending'` filtreli; `related_record_by_type` iki yönlü (`BounceQuery` ↔ boş); `completed_at_with_state` iki yönlü; `attempt_count >= 0`.
- Kriter 3: `AsSingleRow()` + göçte `((true))` tekil dizini (`Company`, `ChannelStateRecord` kalıbı).
- Kriter 4: `ended_at - started_at > interval '2 minutes'` — tam 120 sn reddediliyor, 120,001 sn kabul.
- Kriter 5: `CASE` kısıtında `ServerLogRotation` ↔ sayı boş + iki tarih dolu, öteki türler sayı > 0 + tarih boş; `range_end_date >= range_start_date`.
- Kriter 6: `data_protection_keys.id` `integer` kimlik (`attidentity = d`), `friendly_name`, `xml` `text` boşalabilir, `created_at` yok; model paketin türü (`IDataProtectionKeyContext`), adlar kuraldan.
- Kriter 7: göç `REVOKE SELECT … FROM %I` (`shopfolio.reader_user`) ve `GRANT UPDATE (last_succeeded_at, last_failed_at) … TO %I` (`shopfolio.backup_status_user`), `format('%I')` ve `current_setting`; test kullanıcıları her koşuda rastgele adlı (`shopfolio_test_r…`), dolayısıyla yetki rol ayarından gelen ada gidiyor. **Mutasyon sınaması (validator):** iki yetki deyimi `PERFORM current_setting(…)` ile değiştirildi → beş test kırmızı (`Reader_ReadingTheKeyRing_…`, `BackupStatusUser_UpdatesTheTwoMoments…`, `Migration_WithoutARoleSettingItReads_…` ×2, `DatabaseUsersTests.PrivilegeMatrix_…`); geri alındı, 72/72 yeşil.
- Kriter 8: tek göç, `Down` yok; göçte `shopfolio_` sabit adı, `suppressTransaction`, `CONCURRENTLY`, `VACUUM`, `ALTER SYSTEM`, `CREATE DATABASE`, `now()`, `CREATE EXTENSION`, `UPDATE`, `DELETE FROM`, `TO PUBLIC` yok (grep).

**D-39 (a) betiği:** `database-job.sh` iki sabit deyimi (`WHERE`'siz, yalnız iki sütun) yerel soketten (`/var/run/postgresql`), parolasız, `shopfolio_backup_status` olarak `ON_ERROR_STOP=1` ile çalıştırır; kullanıcı adı `pg_hba.conf`/`pg_ident.conf` ve `initdb/10-bootstrap.sh` ile aynı sabittir (kurucu taraf; göç değil); deyimler değişken içermez (enjeksiyon yüzeyi yok); `set -eu` altında `{ … } || backup_code=$?` hatayı yakalar, başarısızlıkta `last_failed_at` yazılıp pgBackRest koduyla çıkılır, başarı yazımı düşerse yerel kopya alınıp psql koduyla çıkılır. Betik testi dört senaryo (21 doğrulama) ve entegrasyon testi deyimlerin yetkiyle geçtiğini sınar. `now()` veritabanı konteynerinin kendi işidir; `09 §7.1` kural 1 uygulama kodunu bağlar.

**Testler (validator, yerel PostgreSQL 18 `shopfolio-t118-val-pg`):** build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` çıkış 0; Unit 245/245 (Consistency dahil); betik testleri 4/4 dosya (`database-job.test.sh` 21 doğrulama); Integration 926 — T118 sınıfı 55/55, Database 29/29, Model iki grupta 137/137 ve 672/672, Web+FileSecrets+kök 33/33; yerel zaman aşımı yok; `dotnet restore --locked-mode` çıkış 0; koşu sonrası `git status` temiz. Dal CI `41835537615` (head `01d6473`) success — Lint, Build, Unit, Integration, Migration dry-run, Paketleme yeşil (önceki `41835495995` head `0798bd3` iptal — yeni push).

**Güvenlik:** sır yok (durum kullanıcısı parolasız `peer`, betik parola taşımaz); auth etkisi — anahtar halkası okuyucuya kapalı, durum kullanıcısı yalnız iki sütunun `UPDATE`'i (`SELECT`, `INSERT`, `DELETE`, başka sütun ve tablo reddediliyor), uygulama kullanıcısına yeni yetki yok; girdi doğrulama uygulanamaz (uç yok); yeni bağımlılık `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.14 — nuspec `license type="expression" MIT` (validator yerel NuGet önbelleğinden okudu), `Directory.Packages.props`'ta sabit, üç kilit dosyasında, `09` kütüphane satırı güncel, CI NuGetAudit (tüm düzey) temiz.

**`12` satırları:** kaynak birimleri `06` entity'leri, `02 §3.34`, §3.34.8, `09 §10`, `10 KP-75`. KP-75 satırları `VAL-D052`…`D055` (KRİTİK) kayıtların saklama süresince kaybolmamasının uçtan uca satırlarıdır — T07'nin ve T327'nin; `VAL-D115` `06 §4.68`'i anar ama indirme bağlantısının satırıdır; `02 §3.34` satırları (`VAL-A337`…`A339`) tarayıcı satırlarıdır. Hiçbiri T118'in kriterlerinden biriyle kapanmaz; `Durum` değişmedi (BEKLEMEDE); `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** yok.

**Yapım raporu karşılaştırması:** kriter tablosu, KK listesi, ⚠ kararlar ve dış varsayımlar validator'ın bağımsız okumasıyla tam uyumlu; Integration sayısı (926) ve T118 sınıfı (55) birebir aynı.

## Altyapı Değişiklikleri

- **Şema/migration:** Var — altı tablo, iki yetki ayrımı (genişletme göçü).
- **Config / ortam değişkeni:** Yok (betik `POSTGRES_DB`'yi okur; `compose.yaml`'da zaten var).
- **Paketleme / dağıtım:** Var — veritabanı imajındaki `database-job.sh` yedek sonucunu durum kaydına yazar (imaj tanımı değişmedi).
- **Yeni bağımlılık:** Var — `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.14 (Microsoft, MIT, 25 Ekim 2026; EF Core 10.0.14 ile aynı çizgi; NuGetAudit temiz).

## Commit & PR

- **Branch:** `task/T118-zamanlayici-yedek-imha-anahtar`
- **PR:** #376
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Paket .NET 10 / EF Core 10.0.14 ile çalışır, lisansı MIT | nuspec `license expression MIT`, dotnet/dotnet `LICENSE.TXT`; derleme ve entegrasyon testleri yeşil | ✓ |
| 2 | Paketin varlığı adlandırma kuralıyla `data_protection_keys`, `id` (tamsayı kimlik), `friendly_name`, `xml` üretir | `DataProtectionKey_InTheModel_…`, `DataProtectionKeys_KeyIsAnInteger…` | ✓ |
| 3 | Veritabanı konteynerinde zamanlayıcı `postgres` OS kullanıcısıyla koşar, `peer` eşlemesi onu `shopfolio_backup_status`'a geçirir; soket `/var/run/postgresql` | `Dockerfile` `USER postgres`; `pg_ident.conf` `backup_status postgres shopfolio_backup_status`; `pg_hba.conf` yerel satır; `stanza-bootstrap.sh` aynı soketi kullanır | ✓ (gerçek imajda koşu: T119 satırı doğunca ilk kurulum denemesi) |

## ⚠ Kararlar

1. **Varlıkların klasörü `Infrastructure/*`.** `Job`, `HeartbeatRecord`, `OutageRecord` → `Infrastructure/Jobs` (`09 §13.1`: iş kuyruğu ve zamanlayıcı `Infrastructure/Jobs/` altında; `05 §3.3.11` zamanlayıcıyı ve iş kuyruğunu ortak altyapıya koyar); `DisposalRecord` → `Infrastructure/Disposal` (`05 §3.3.11` imha düzenleyicisi ortak altyapıda; her modül kendi verisini kendi arayüzünden imha eder, kayıt düzenleyicinindir); `BackupStatusRecord` → `Infrastructure/Backup` (hiçbir modülün iş kuralı değil; yazanı veritabanı konteynerinin işi, okuyanı durum ucu). Bu kayıtların hiçbiri `05 §3.3`'teki bir modülün listesinde değildir.
2. **`DataProtectionKey` kendi sınıfımız değil, paketin türü** (`06 §4.68` "paketin varlık türü"; `09 §5.2` madde 4): `AppDbContext` `IDataProtectionKeyContext`'i uygular. `PersistKeysToDbContext` bağlaması anahtar halkasını kullanan task'ındır; bu task yalnız şemayı kurar.
3. **Yedek işinde başarısızlık tanımı.** `stanza-create` düşmesi de sunucu dışı yedeğin alınmaması demektir → `last_failed_at`. Başarı yazımı düşerse yerel kopya yine alınır (yerel kopya gerçek yedekten bağımsız bir hızlı geri dönüş kaynağıdır) ve iş sıfırdan farklı kodla çıkar; zamanlayıcı kodu kayda yazar.
4. **Satır T119'da doğar.** `BackupStatusRecord` ve `HeartbeatRecord`'un tek satırı başlangıç verisindendir (`06 §4.65`, §4.67; K-1031 (1)); o zamana kadar betiğin `UPDATE`'i sıfır satır değiştirir (hata değil).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `PersistKeysToDbContext` bağlaması ve anahtar halkasının kullanımı | — | Veri koruma amaçlarını kuran task'ın işi (`09 §15.5`); bu task şemayı kurar |
| 2 | Tek satırların başlangıç verisi | — | T119 (başlangıç verisi göçü) |
| 3 | İş kuyruğu, zamanlayıcı, nabız, imha işi davranışları ("Uygulamada" kuralları) | — | `09 §13` task'ları |

## Notlar

- **Working tree (Adım -1):** temiz (önceki işçi ve bu işçi).
- **Ana dal CI startup (Adım 0):** önceki işçi: `41825046994` (CI) ve `41825046782` (Publish) T116, `41820820561` T115 success. Devamda: T117 squash `b7bd9b2` CI `41830794252` ve Publish `41830794202` success.
- **Bölünme:** yapım iki işçide yapıldı; ilk işçi bağlam sınırında `e8619b3`'te kesildi (testler koşulmamıştı). İkinci işçi derledi, bütün testleri koştu, iki test düzeltmesi, D-39 (a), doküman ve raporu ekledi.
- **Bağlı backlog:** D-39 (a) ✅ Çözüldü → T118; öteki kalemler ((b), (c), (e), (f)) değişmedi. `grep -n "T118" Docs/DEFERRED_BACKLOG.md` yalnız D-39. Açılan kalem yok.
- **Repo memory (bitiş kapısı 8):** güncellendi.
