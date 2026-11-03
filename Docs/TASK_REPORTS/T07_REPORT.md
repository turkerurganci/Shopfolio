# T07 — Yedek düzeni

**Faz:** F0 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Veritabanı imajı** (`deploy/images/database/`): resmî PostgreSQL 18 imajının giriş noktasını saran `database-entrypoint.sh` (iki yedek sır dosyasını pgBackRest ortamına çevirir, zamanlayıcıyı ve tek seferlik stanza oluşturmayı sunucunun yanında başlatır, sunucu kullanıcısını değiştirmez); `database-job.sh` (gecelik tam yedek, iki depo ayrı ayrı adlanır); `stanza-bootstrap.sh`; `database-restore-drill.sh`; `pgbackrest.conf` (depo 1 yerel kopya, depo 2 şifreli nesne depolama, saklama zaman tipli: 1 gün / 30 gün); `postgresql.conf.d/20-archive.conf` (`archive_timeout = 50s`, kuyruk sınırı yok).
- **Yedek aracı imajı** (`deploy/images/backup/`, `deploy/images/shared/`): `file-backup-job.sh` (dört adım: yedek, `forget --keep-within 30d`, `prune --max-unused 0`, denetim; `&&` zinciri), `file-restore-drill.sh`, `write-status.sh` (durum dosyası atomik), zamanlayıcıda `BACKUP_TIME_ZONE` zorunlu.
- **Compose ve ayarlar:** `database` ve `backup` hizmetlerine `BACKUP_*` ortamı (varsayılan 01:30, `Europe/Istanbul`), `backup` hizmetine iş komutu; `deploy/.env.example` ve `DEPLOY_RUNBOOK` ayar tablosu.
- **Kova ve anahtar** (`deploy/backup/`): `bucket-lifecycle.json`, `key-policy.template.json`, `bucket-checks.sh setup|keys|lifecycle` (kurulumda koşar).
- **Testler:** `tests/scripts/` betik testleri (sh, alpine ash, debian dash'te), `.github/ci-commands.env` `UNIT_TEST_CMD` başına bağlandı; `Consistency/BackupLayoutConsistencyTests.cs` (15 eşitlik denetimi).
- **CI Paketleme:** `scripts/ci/package-build.sh` adım 6 artık veritabanı konteynerini yedek ayarları ve yer tutucu sır dosyalarıyla başlatır, `archive_timeout`'u ve pgBackRest yapılandırmasının okunmasını sınar; adım 7 yedek betiklerinin varlığını sınar.
- **Dokümanlar:** `09 §17` (ağaç, madde 6 pgBackRest istisnası, madde 7 betik testi, yeni madde 8), `DEPLOY_RUNBOOK` §J (J.1–J.5), backlog (D-28, D-30 (b), D-34 (a) kapandı; D-32 ve D-01 notları; yeni D-37), `CONTEXT.md`, `IMPLEMENTATION_STATUS.md`, hafıza.

## Etkilenen Modüller / Dosyalar

- `deploy/images/database/*`, `deploy/images/backup/*`, `deploy/images/shared/*`, `deploy/compose.yaml`, `deploy/.env.example`, `deploy/backup/*`
- `tests/scripts/*`, `tests/Shopfolio.UnitTests/Consistency/BackupLayoutConsistencyTests.cs`
- `scripts/ci/package-build.sh`, `.github/ci-commands.env`
- `Docs/09_…` §17, `Docs/DEPLOY_RUNBOOK.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İşlem günlüğü ≤ 1 dk; `archive_timeout` 50 s | ✓ | `20-archive.conf`; `ArchiveSettings_…` testi; yerelde çalışan imajda `SHOW archive_timeout` → `50s`; CI Paketleme adım 6 sınar |
| 2 | Gecelik iki iş imhadan (03:00) önce biter | ✓ | tek `BACKUP_RUN_TIME` 01:30, bölge `Europe/Istanbul`; `BackupSchedule_…` testi (≤ 01:30, iki hizmette aynı) |
| 3 | Dosya yedeği kapsamı ve dört adım | ✓ | `file-backup.test.sh` (117 doğrulama: sıra, düşen adımdan sonrası çağrılmaz, `--max-unused 0`); `FileBackupPaths_…` testi (yollar = salt-okunur bağlamalar, ayar/sır yok) |
| 4 | Durum dosyası atomik; başarısız turda `lastSuccessAt` önceki | ✓ | `write-status.sh`; `file-backup.test.sh` (şema, önceki değer, ilk koşuda boş) |
| 5 | Ayrı anahtar ve önek; kova kuralı; anahtar yetkisi | ~ | iki ayrı sır dosyası; `bucket-lifecycle.json`, `key-policy.template.json` ve testleri; reddedilen eylemlerin denemesi `bucket-checks.sh`'tadır, **gerçek kova ister, kurulumda koşar** (D-01 notu) |
| 6 | Saklama 30 gün, zaman tipli; sınırdan eski en yeni tam yedek | ✓ | `PgBackRestRetention_…`, `FileBackupJob_KeepsThirtyDays…`, `BucketLifecycleRule_…` testleri |
| 7 | Kurtarma provası runbook adımı | ✓ | `DEPLOY_RUNBOOK` §J.5; `RunbookDrill_HasTheFourChecks…` testi; iki prova betiği üç kayıt alanını basar; koşum kurulumda (T09) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Betik testleri | ✓ | `sh tests/scripts/run-script-tests.sh` | 3 dosya geçti (11 + 117 + 27 doğrulama) |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release` | 76/76 |
| Consistency (Lint) | ✓ | `… --filter-trait "Consistency=true"` | 26/26 |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | temiz (bir boş satır düzeltmesinden sonra) |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı, 0 hata |
| Integration | — | koşulmadı | bu task kod dosyası değiştirmez (yalnız `Consistency` testi eklendi); CI koşar |
| Contract | — | — | NOT_APPLICABLE |
| İmaj | ✓ | `docker build` (veritabanı, yedek) + çalıştırma | `archive_timeout 50s`, `track_commit_timestamp on`, `pgbackrest info` yapılandırmayı okudu; yerel imajlar ve denemede açılan kaynaklar adıyla silindi |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulayıcı doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `BACKUP_S3_ENDPOINT`, `BACKUP_S3_REGION`, `BACKUP_S3_BUCKET`, `BACKUP_S3_URI_STYLE`, `BACKUP_RUN_TIME`, `BACKUP_TIME_ZONE` (`.env.example`, runbook tablosu)
- **Paketleme / dağıtım:** Var — veritabanı imajı giriş noktası ve betikler, yedek imajı betikleri, Compose `environment`/`command`
- **Yeni bağımlılık:** Yok (pgBackRest ve restic T06'dan; pgBackRest sürümü sabit değildir, `09 §17` madde 6 istisnası)

## Commit & PR

- **Branch:** `task/T07-yedek-duzeni`
- **Commit:** *(PR açılışında)*
- **PR:** *(PR açılışında)*
- **CI:** *(yapım chat'i izlemez)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | pgBackRest PGDG apt deposundan kurulur ve `pgbackrest version` döner | imaj derlendi, `pgbackrest info` çalıştı (2026-11-03) | ✓ |
| 2 | `pg1-socket-path` ile yerel soket yeterli, `pg1-host` gerekmez | çalışan imajda `pgbackrest info` hatasız (`pgbackrest.org/configuration.html`, erişim 2026-11-03) | ✓ (kurucu yetkisiyle gerçek yedek kurulumda) |
| 3 | Gerçek kova ve hesap | yok; runbook adımı | ? kurulumda (D-01) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Veritabanı yedeğinin sonucu `backup_status_records`'a yazılmıyor | D-37 (a) | tablo ve `shopfolio_backup_status` kullanıcısı T10 / veri modeli task'ındadır |
| 2 | Depo 1 saklaması ve disk kullanımı gerçek sunucuda gözlenecek | D-37 (b) | kurulum denemesi (T09) |
| 3 | Uygulama dosya/kayıt klasör yolları | D-32 (a)/(b) kalan | dosya deposunu ve kaydı yazan task |
| 4 | Kova kuralı, anahtar yetkisi ve kurucu yetkili yedek denemeleri | D-01 | gerçek kova gerekir; kurulumda koşar |

## Notlar

- **Working tree (Adım -1):** temiz (devir alındı, dal `f6a9b67`)
- **Ana dal CI startup (Adım 0):** önceki yapım işçisinde; bu devamda yeniden izlenmedi
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** ⚠ kararlar PR açıklamasındadır (numaralı).
