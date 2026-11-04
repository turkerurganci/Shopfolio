# T118 — Zamanlayıcı, yedek, imha ve anahtar kayıtları

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

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
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — altı tablo, iki yetki ayrımı (genişletme göçü).
- **Config / ortam değişkeni:** Yok (betik `POSTGRES_DB`'yi okur; `compose.yaml`'da zaten var).
- **Paketleme / dağıtım:** Var — veritabanı imajındaki `database-job.sh` yedek sonucunu durum kaydına yazar (imaj tanımı değişmedi).
- **Yeni bağımlılık:** Var — `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.14 (Microsoft, MIT, 25 Ekim 2026; EF Core 10.0.14 ile aynı çizgi; NuGetAudit temiz).

## Commit & PR

- **Branch:** `task/T118-zamanlayici-yedek-imha-anahtar`
- **PR:** #PRNO
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
