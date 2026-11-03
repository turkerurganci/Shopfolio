# T07 — Yedek düzeni

**Faz:** F0 | **Durum:** ✗ Doğrulama tur 3 FAIL (2026-11-03, `c929163`; main ile birleşik hâl); düzeltme turu gerekli | **Tarih:** 2026-11-03

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
- **Dokümanlar:** `09 §17` (ağaç, madde 6 pgBackRest istisnası, madde 7 betik testi, yeni madde 8), `DEPLOY_RUNBOOK` §J (J.1–J.5), backlog (D-28, D-30 (b), D-34 (a) kapandı; D-32 ve D-01 notları; yeni D-39), `CONTEXT.md`, `IMPLEMENTATION_STATUS.md`, hafıza.

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

## Düzeltme 1

**Karar (yardımcı, proje sahibi yetkisiyle): kod dokümana uyar.** Tur 1 FAIL'ın (bulgu 1–4) düzeltmesi; K1, K2, K4, K6, K7 bu turda kapandı, K3 ve K5 D-39'ye eklendi.

| # | Bulgu / not | Durum | Ne yapıldı |
|---|---|---|---|
| 1 | KRİTİK: yedek kullanıcısı pgBackRest kaydını ve sistem günlüğünü okuyamaz | ✓ (pgBackRest kaydı gerçek imajlarla); sistem günlüğü kısmı T09'da | `logread` grubu (gid 10002) yalnız `/var/log/postgresql/pgbackrest` dizinine, setgid + `2750` (`deploy/images/database/Dockerfile`); `backup` hizmeti `group_add: ["10002"]` (`deploy/compose.yaml`). Veri dizini, ayar ve sır dosyaları aynı kaldı (`05 §7.6.3`). Sistem günlüğü: yalnız `/var/log/journal` için `setfacl` (runbook §J.4 madde 5); Linux sunucu ister, deneme kurulumda (T09), D-39 (c) |
| 2 | `/var/log/backup` yedek kapsamında değil | ✓ | `DEFAULT_BACKUP_PATHS`'e eklendi; eşitlik testi salt okuma bağlamalar + bu kayıt dizini (yazılabilir bağlama olarak Compose'da bulunmak zorunda) ile karşılaştırır; `09 §17` m.7'nin eşitlik cümlesi buna göre yazıldı |
| 3 | `09 §17` m.7 çift metin | ✓ | yinelenen kuyruk silindi |
| 4 | Prova bütün görselleri geri alıyor | ✓ | `file-restore-drill.sh` kayıt dizinini ve görsellerin ilk üç dosyasını (`restic ls --json`) geri alır |
| K1 | İlk açılışta geçici sunucu kapanmıyor | ✓ | runbook §J.4 madde 4: beklenen davranış, yeniden başlatma politikası düzeltir |
| K2 | Nesne kilidi red denemesi yok | ✓ | `bucket-checks.sh keys`: `put-object-lock-configuration` ve `put-bucket-policy` red denemesi; eşitlik testi ve runbook §J.3 madde 4 |
| K3 | Prova `archive-get` | ertelendi | D-39 (d), T09 |
| K4 | `init` hatası `set -e` altında yutulur | ✓ | `ensure_repository` ve `step_backup` hatayı elle döndürür; betik testi (`cat init` düşer, `backup` çağrılmaz) |
| K5 | Paketleme taramaları geniş | ertelendi | D-39 (e) |
| K6 | D-39 metni | ✓ | yanlış `05 §7.6.7` atfı ve olmayan "kayıt dosyasını okuyan durum ucu" cümlesi düzeltildi; "`09 §17` madde 8" yerine `pgbackrest.conf`'taki `repo1-retention-full` |
| K7 | PR açıklaması `09 §25` | ✓ | PR açıklaması soru başına evet/hayır/uygulanamaz + gerekçe verir |

**Yerel tam tur (`09 §17` m.7 ilk task kabulü; gerçek iki imaj, Docker):** veritabanı imajında pgBackRest'in yazdığı kayıt dosyaları `postgres:10002 0640` olur; `group_add 10002` ile koşan yedek konteyneri (uid 10001) `file-backup-job.sh`'ı koşar: backup, forget, prune (`--max-unused 0`), check çıkış 0; durum dosyası `result: success`, `lastSuccessAt` dolu; anlık görüntüde `/data/db-logs/pgbackrest/*` ve `/var/log/backup/file-backup.log` var; prova: 3 dosya geri alındı, `check --read-data-subset=5%` "no errors", üç `drill-record` alanı. Depo yerel (`/repo/files`); nesne depolama ve sistem günlüğü kurulumdadır. Docker kaynakları adıyla silindi.

**Testler:** betik testleri 11 + 129 + 27 (üç kabukta; dosya yedeği +12), Unit 76/76 (Consistency dahil), `-warnaserror` ve `dotnet format --verify-no-changes` temiz, `docker compose config` çıkış 0, paketleme adım 1 taramaları yerelde temiz.

## Düzeltme 2

Tur 3 FAIL sonrası (birleşik hâl `c929163`; bulgu 1, `VAL-F016`, S1 ORTA; K11). Karar: kod `05 §7.6.8` ve MK-27'ye uyar.

- **Bulgu 1:** `database-restore-drill.sh` kopyayı açtıktan sonra `pg_is_in_recovery()` yanlış olana dek yoklar (`DRILL_POLL_SECONDS`, varsayılan 2 sn; üst süre `DRILL_RECOVERY_TIMEOUT`, varsayılan 600 sn; aşılırsa betik hata verir, kayıt yazmaz). `pg_ctl -w` sıcak beklemede ilk tutarlılıkta döndüğü için eski betik kurtarma bitmeden okuyordu. Süre = temel yedek indirme + günlük indirme/uygulama (kurtarma bitene dek); "dönülen son an" kurtarma bittikten sonra okunur. Runbook §J.5 madde 1 buna göre güncellendi.
- **Deneme (betik):** `tests/scripts/database-restore-drill.test.sh` (13 doğrulama; sahte `pgbackrest`, `pg_ctl`, `psql`): kopya üç yoklama boyunca kurtarmada, betik beklemeye devam eder ve arşivin son anını yazar (temel yedeğin anını değil); kurtarma hiç bitmezse üst süre içinde hata verir ve kayıt yazmaz.
- **Deneme (gerçek imaj):** son tam yedekten (16:00:40) sonra 8 000 satır yazıldı, günlük arşivlendi (son WAL `…0B`); prova çıktısı: günlük `…03`–`…0B` geri yüklendi, `archive recovery complete`, `last-moment=2026-11-03 16:01:49.329111+00` (birincil sunucudaki son işlemle aynı; eski betik 16:00:40 civarını yazardı), `restore-duration-seconds=5`, depo `repo2`. Docker kaynakları (`shopfolio-t07fix2-*` konteyner, üç birim, imaj) adıyla silindi.
- **K11:** PR #351 açıklaması birleşik hâle göre güncellendi (Unit 68 + Consistency 34 = 102; betik testleri 4 dosya). K10 kapalı.
- **Testler:** betik testleri 4 dosya geçti (11 + 13 + 129 + 27), Unit + Consistency 102/102, `-warnaserror` 0/0, `dotnet format --verify-no-changes` 0, `docker compose config` üç bileşimde 0, paketleme adım 1 taramaları (`latest`, sır adı) temiz, `dotnet restore --locked-mode` 0.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 4) | ✓ PASS — 2026-11-03 (`aa1cfe6`; düzeltme 2 sonrası, `main` ile birleşik hâl; kapsam tam). Kriter 1–7 ✓, KK1–KK7 ✓, `09 §25` evet/uygulanamaz, güvenlik temiz. Dal CI run 41670381320 yeşil (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate) |
| Bulgu sayısı (tur 4) | 0 — tur 3'ün bulgusu (`VAL-F016`, prova son anı ve süresi) kapandı |
| Düzeltme gerekli mi (tur 4) | Hayır |
| Bloke-etmeyen not (tur 4) | 2 — K12 `core.hooksPath` yine mutlak yolda bulundu (üçüncü kez), `install.sh` ile düzeltildi (kapandı; kaynağı repo dışında, yardımcıya bildirildi); K13 D-39 (d) (prova `archive-get`'inin hangi depodan çektiği) gerçek imajda gözlendi ve kapatıldı |
| Doğrulama durumu (tur 3) | ✗ FAIL — 2026-11-03 (`c929163`; tur 2 PASS'ten sonra `main` (T10, #350) ile birleşik hâl; kapsam tam; iki doğrulayıcıyla, ilki bağlam sınırında bölündü). Kriter 1–6 ✓, **7 ✗**; KK1–KK7 ✓ (KK7 adım ve alanların varlığını sınar, değerlerin doğruluğunu değil). Dal CI run 41665382289 yeşil; güvenlik temiz |
| Bulgu sayısı (tur 3) | 1 — S1 ORTA (`VAL-F016`): veritabanı provası son anı ve süreyi yanlış kaydeder (ayrıntı aşağıda) |
| Düzeltme gerekli mi (tur 3) | Evet — `database-restore-drill.sh` kurtarmanın bitmesini beklemeli; süre kurtarmanın sonuna dek ölçülmeli |
| Bloke-etmeyen not (tur 3) | 2 — K10 `core.hooksPath` paylaşılan `.git/config`'te mutlak yola dönmüş bulundu, `install.sh` ile düzeltildi (kapandı); K11 PR açıklamasındaki test sayıları birleşme öncesi (Unit 76 / Consistency 26; birleşik hâl Unit 68 + Consistency 34) — düzeltme turunda PR açıklaması güncellenir |
| Doğrulama durumu (tur 2) | ✓ PASS — 2026-11-03 (`501c24b`; kapsam tam; iki doğrulayıcıyla, ilki bağlam sınırında bölündü). Kriter 1–7 ✓, KK1–KK7 ✓, `09 §25` evet/uygulanamaz, güvenlik temiz. Dal CI run 41660924636 yeşil (10 check-run) |
| Bulgu sayısı (tur 2) | 0 — tur 1'in dört bulgusu ve K1–K7 kapandı (K3, K5 D-39'de) |
| Düzeltme gerekli mi (tur 2) | Hayır |
| Bloke-etmeyen not (tur 2) | 2 — K8 → D-39 (f); K9 (PR açıklamasında `09 §25` "Performans" ve "Git ve süreç" grupları yanıtsız) bu raporda yanıtlandı |
| Doğrulama durumu (tur 1) | ✗ FAIL — 2026-11-03 (`fc2977c`); rapor: yardımcı durum klasöründe `dogrulama-raporu-r1.md`. Kriter 1, 2, 4, 5, 6, 7 ✓; **3 ✗**. Dal CI run 41656454884 yeşil; build `-warnaserror` temiz, Unit 76/76 (Backup eşitlik 15/15), betik testleri sh/dash/ash'te yeşil, `compose config` geçerli |
| Bulgu sayısı | 4 — (1) S1 KRİTİK: yedek kullanıcısı (10001) pgBackRest kaydını (0640 postgres) ve systemd günlüğünü (0640) okuyamaz; gerçek imajlarla koşuda restic çıkış 3, durum `failure/backup`, forget/prune/check hiç koşmaz (yerel Docker kanıtı); (2) S1 ORTA: yedek aracının kendi kaydı (`/var/log/backup`, `05 §9.1.4`) dosya yedeğinin yollarında yok; (3) S1 DÜŞÜK: `09 §17` madde 7 metni çift kaldı; (4) S1 DÜŞÜK: dosya provası "birkaç dosya" yerine bütün görsel dizinini geri alır |
| Düzeltme gerekli mi | Evet — yeni yapım turu; `09 §17` m.7'nin "geçici depoda yedek–budama–denetim–kurtarma" kabulü gerçek imajlarla koşulup rapora yazılır |
| Bloke-etmeyen not (K) sayısı | 7 — K1 ilk açılışta erişilemeyen depoda geçici sunucu kapanmıyor (bir kez `Exited (1)`); K2 nesne kilidi red denemesi yok; K3 prova `archive-get`'inin hangi depodan çektiği kurulumda gözlenmeli; K4 `init` hatası durmuyor; K5 paketleme taramaları geniş; K6 D-39 atıfları; K7 PR'da `09 §25` soru başına cevap yok — düzeltme turunda ya da D-39/backlog'a |

### Doğrulama tur 4 — validator kanıtları (tam kapsam, birleşik hâl, düzeltme 2)

Başlangıç kapıları: working tree temiz; `core.hooksPath` yine mutlak yoldaydı → `install.sh` ile `scripts/git-hooks` (K12); ana dal son 3 run (41664366613, 41664366553, 41649150934) success; memory'de T07 satırı var. Dal CI run 41670381320 (head `aa1cfe6`) success — Lint (Consistency 34), Build, Unit (betik testleri 4 dosya, `database-restore-drill.test.sh` 13 doğrulama; 68 test), Integration 62, Contract, Migration, Paketleme (pgBackRest 2.60.0, `archive_timeout` 50s; restic 0.19.1), E2E, CI Gate. Yerel: `dotnet restore --locked-mode` 0; `dotnet build -c Release -warnaserror` 0/0; Unit 68/68; Consistency 34/34; `dotnet format --verify-no-changes` 0; betik testleri 4 dosya (11 + 13 + 129 + 27); `docker compose ... config -q` (temel, `compose.install.yaml`, `compose.dev.yaml`) 0. Betik testi düzeltme öncesi betiğe (`e159127`) karşı koşuldu: 13 doğrulamanın 7'si düşer — deneme hatayı yakalar.

Yerel Docker (`shopfolio-valr4-*`, hepsi adıyla silindi): dalın iki imajı derlendi; veritabanı ilk kurulumla açıldı (T10 kurucu + T07 giriş), depo 2 ortamla yerel yola çevrildi (`PGBACKREST_REPO2_TYPE=posix`). `database-job.sh` çıkış 0 (önce `--repo=2`, sonra `--repo=1` tam yedek; `pgbackrest info` status ok, depo 2 `aes-256-cbc`, kayıtta şifre `<redacted>`). Yedekten sonra dört işlem ve 30 sn arayla `archive_timeout` kapanışları; son işlem `16:12:46.42299`, son arşivlenen WAL `…08`. Prova runbook'taki gibi ayrı konteynerde (`db-data` bağlı): günlükler `…03`–`…08` geri yüklendi, "consistent recovery state" sonrasında da uygulama sürdü, `redo done`, `archive recovery complete`; `drill-record: last-moment=2026-11-03 16:12:46.42299+00` (birincil sunucudaki son işlemle aynı), `restore-duration-seconds=6`, `repository=object-storage (repo2)`. Aynı imajda `--repo=2` geri alımının `restore_command`'ı `--repo=2` taşır → günlükler dış depodan (D-39 (d) kapandı). Yedek imajı (gerçek restic, varsayılan yollar, `--group-add 10002`): dört adım çıkış 0, durum dosyası `success` ve `lastSuccessAt` dolu; anlık görüntüde `/data/db-logs/pgbackrest/*` ve `/var/log/backup/file-backup.log`; dosya provası 4 dosya, `check --read-data-subset=5%` "no errors", dört `drill-record` satırı.

| # | Validator sonucu | Validator kanıtı |
|---|---|---|
| 1 | ✓ | Çalışan imajda `SHOW archive_timeout` → `50s`, `archive_mode` on; `ArchiveSettings_…`; CI Paketleme |
| 2 | ✓ | `compose config`: iki hizmette 01:30 / `Europe/Istanbul`; `BackupSchedule_…` |
| 3 | ✓ | Gerçek restic dört adım sırayla çıkış 0; betik testi (sıra, düşen adım, `--max-unused 0`); `FileBackupPaths_…` |
| 4 | ✓ | `write-status.sh` geçici dosya + `mv`; durum dosyası şemaya uygun; betik testi önceki `lastSuccessAt`, ilk koşuda boş |
| 5 | ✓ | Ayrı sır dosyaları (`db_backup_*`, `file_backup_*`), önekler `database/`, `files/`; `bucket-lifecycle.json`, `key-policy.template.json`, `bucket-checks.sh` runbook §J.3–J.5'te, kurulumda koşar (KK5; D-01 açık, `SETUP_COMPLETE=false`) |
| 6 | ✓ | pgBackRest `repo2-retention-full-type=time`/30, restic `--keep-within 30d`; kova kuralı yalnız eski sürüm; eşitlik testleri |
| 7 | ✓ | Veritabanı provası kurtarmanın sonunu bekler, son an arşivin son işlemi, süre indirme + uygulama, depo 2 (yukarıda); dosya provası örnek + veri okuyan denetim; runbook §J.5 dört denetim ve üç kayıt alanı; koşum kurulumda (T601) |

`12`: `VAL-F016` FAIL → BEKLEMEDE (kriter 7 ✓; satırın ön koşulu — referans kurulum ve firmanın nesne depolaması — T601'de sağlanır, `12 §5` kural 1; tur 2 ile aynı); `VAL-D052`…`D055` BEKLEMEDE kalır. Atlama kaydı: iki `[ci-failure]` `PB_ALLOW_DIRECT_PUSH` `00 §F.2`'ye uygun (kök neden düzeltmeleri, kayıt sonraki commit'te); `PB_ALLOW_SECRET` yok; diff'te sır deseni yok.

Yapım raporu karşılaştırması: uyuşmazlık yok (verdict'i etkileyen). Kriter 5'te yapım `~`, doğrulayıcı ✓ (KK5 tanımı; tur 2–3 ile aynı). Düzeltme 2'nin gerçek imaj kanıtı doğrulayıcının koşusuyla örtüşür.

### Doğrulama tur 3 — validator kanıtları (tam kapsam, birleşik hâl)

Başlangıç kapıları: working tree temiz; `core.hooksPath` mutlak yoldaydı → `install.sh` ile `scripts/git-hooks` (K10); ana dal son 3 run (41664366613, 41664366553, 41649150934) success; memory'de T07 satırı var. Dal CI run 41665382289 (head `c929163`) success — Lint (34 Consistency), Build, Unit (betik testleri 11 + 129 + 27, 68 test), Integration 62, Migration, Paketleme (adım 6: `archive_timeout` 50s, pgBackRest 2.60.0, T10 kullanıcı denetimleri; adım 7: restic 0.19.1), E2E, CI Gate. Yerel: `dotnet restore --locked-mode` 0; `dotnet build -c Release -warnaserror` 0/0; Consistency 34/34; Unit 68/68; `dotnet format --verify-no-changes` 0; betik testleri 3 dosya geçti; `docker compose ... config -q` (temel, `compose.install.yaml`, `compose.dev.yaml`) çıkış 0.

| # | Validator sonucu | Validator kanıtı |
|---|---|---|
| 1 | ✓ | Birleşik veritabanı imajı ilk kurulumla (T10 kurucu betik + T07 giriş noktası) açıldı; `SHOW archive_timeout` → `50s` |
| 2 | ✓ | İki hizmette 01:30 / `Europe/Istanbul`; imha (03:00) öncesi |
| 3 | ✓ | Gerçek restic, varsayılan yollar, `--group-add 10002`: dört adım çıkış 0; anlık görüntüde `/data/db-logs/pgbackrest/*` ve `/var/log/backup/file-backup.log` |
| 4 | ✓ | Durum dosyası `result: success`, `lastSuccessAt` dolu; betik testleri şema, önceki değer, ilk koşuda boş |
| 5 | ✓ | Ayrı sır dosyaları ve önekler; depo 2 `aes-256-cbc`, kayıtta şifre `<redacted>`; kova kuralı ve anahtar politikası denemesi runbook'ta, kurulumda (KK5; D-01) |
| 6 | ✓ | Saklama zaman tipli 30 gün (pgBackRest, restic `--keep-within 30d`); kova kuralı yalnız eski sürümü siler |
| 7 | ✗ | Dosya provası ✓ (4 dosya, `check --read-data-subset=5%` no errors, kayıt alanları). **Veritabanı provası ✗:** `database-restore-drill.sh` kopyayı `pg_ctl -w start` ile açar; `hot_standby` açık olduğundan `pg_ctl` tutarlılık anında ("ready to accept read-only connections") döner, betik `pg_last_committed_xact()`'ı günlük uygulaması bitmeden okur ve hemen `pg_ctl stop` ile durdurur. Yerel kanıt: yedekten sonra ~20 WAL dosyası arşivlendi, gerçek son commit 15:46:29; prova yalnız iki WAL dosyası geri yükledi, `drill-record: last-moment=2026-11-03 15:45:00.48+00` (tam yedeğin anı), `restore-duration-seconds=6` yalnız temel yedeğin indirilmesi. `05 §7.6.8`, `MK-27` ("geri dönüşün süresi — indirme dahil — ve dönülen son an"), runbook §J.5 m.1 ("son işlemin kesinleşme anını okur") ve betiğin kendi yorumuyla ("recover to the end of the archived log") çelişir |

**Bulgu 1 (S1, ORTA, `VAL-F016`):** beklenen — prova kaydı arşivdeki son işlemin anını ve kurtarmanın sonuna dek süreyi taşır; gerçekleşen — son tam yedeğin anı ve yalnız temel yedeğin indirme süresi. Kurtarma noktasının ölçümü yanıltır (kurtarma zayıflığı). Düzeltme önerisi: kopyayı `-c hot_standby=off` ile açıp kurtarmanın bitmesini beklemek ya da `pg_is_in_recovery()` false olana dek yoklamak (`pg_ctl -t 600` sınırı); süreyi kurtarma bitince almak; son anı terfiden sonra okumak; kurtarmanın sonuna dek beklediğini sınayan bir betik ya da paketleme denemesi eklemek. Etkilenen dosya: `deploy/images/database/database-restore-drill.sh` (gerekirse runbook §J.5 m.1).

Atlama kaydı: iki `[ci-failure]` `PB_ALLOW_DIRECT_PUSH` (9a4174f, 5b68386 yalnız kök neden düzeltmesi; kayıt d17a85b, fc2977c) `00 §F.2`'ye uygun; `PB_ALLOW_SECRET` yok; diff'te sır deseni yok. D-01 🔴 açık, `SETUP_COMPLETE=false`; D-37 atıfları D-39'a çevrilmiş. `12`: `VAL-F016` → FAIL (kriter 7 ✗; `12 §6.1`); `VAL-D052`…`D055` BEKLEMEDE kalır.

Yapım raporu karşılaştırması: 1 uyuşmazlık — yapım kriter 7'yi ✓ yazmış (kayıt alanlarının varlığı sınanmış, değerlerin doğruluğu denenmemiş); kriter 5'te yapım `~`, doğrulayıcı ✓ (KK5 tanımı; tur 2 ile aynı).

### Doğrulama tur 2 — validator kanıtları (tam kapsam)

Başlangıç kapıları: working tree temiz; `core.hooksPath` kurulu; ana dal son 3 run (41649150934, 41649150970, 41630669794) success; memory'de T07 satırı var. Dal CI run 41660924636 (head `501c24b`) success — Lint (26 Consistency), Build, Unit (betik testleri 3 dosya + 50), Integration, Contract, Migration, Paketleme, E2E, CI Gate. Yerel: `dotnet build -c Release -warnaserror` 0/0; Unit+Consistency 76/76; betik testleri 11 + 129 + 27 (Git sh, BusyBox ash, Debian dash); `docker compose ... config` çıkış 0; `dotnet restore --locked-mode` 0.

| # | Validator sonucu | Validator kanıtı |
|---|---|---|
| 1 | ✓ | Çalışan veritabanı imajında `archive_timeout=50`, `archive_mode=on`, `track_commit_timestamp=on`; `ArchiveSettings_…` testi |
| 2 | ✓ | `compose config`: iki hizmette 01:30 / `Europe/Istanbul`; zamanlayıcı imha (03:00) öncesine uyuyor |
| 3 | ✓ | Gerçek iki imaj (`t07val2-*`, yerel depo `/repo/files`): grupsuz yedek konteyneri çıkış 3 / `failedStep: backup` (tur 1 hatası yeniden üretildi); `group_add 10002` ile dört adım çıkış 0, anlık görüntüde `/data/db-logs/pgbackrest/shopfolio-stanza-create.log` ve `/var/log/backup/file-backup.log`; veritabanı imajında kayıt dizini `drwxr-s--- 999:10002`, dosya `-rw-r----- 999:10002` |
| 4 | ✓ | Aynı koşuda `result: success`, `lastSuccessAt` dolu; betik testleri şema, önceki değer, ilk koşuda boş |
| 5 | ✓ | Ayrı sır dosyaları ve önekler; `bucket-lifecycle.json`, `key-policy.template.json`; `bucket-checks.sh keys` reddedilen eylemleri (nesne kilidi ve kova politikası dahil) dener — KK5 bunu runbook denemesi sayar, kurulumda koşar (D-01). Yapım `~` yazmıştı; KK5'in tanımına göre ✓ |
| 6 | ✓ | `PgBackRestRetention_…`, `FileBackupJob_KeepsThirtyDays…`, `BucketLifecycleRule_…` testleri |
| 7 | ✓ | Prova yerelde: 4 dosya (app-logs 1 + görsellerden 3) geri alındı, `check --read-data-subset=5%` no errors, `drill-record` üç alan; runbook §J.5 dört denetimi taşır |

Atlama kaydı: iki `[ci-failure]` `PB_ALLOW_DIRECT_PUSH` (9a4174f, 5b68386 — kırmızı dal CI'ının kök neden düzeltmeleri), kayıt sonraki commit'te (d17a85b, fc2977c); `00 §F.2`'ye uygun. `PB_ALLOW_SECRET` yok, dalda sır değeri yok. `12`: VAL-D052…D055 (KP-75, T327) ve VAL-F016 (T07 (7), T601 (7); `gerçek` ortam) BEKLEMEDE kalır; `12_matris_denetle.py --kapsam 1-77` hata 0.

`09 §25` PR'da yanıtsız kalan gruplar (K9): **Performans** — uygulanamaz (uygulama kodu, sorgu, eşzamansız yol, önbellek yok; performans iddiası yok). **Git ve süreç** — PR tek amaçlı (T07), dal adı ve commit/PR biçimi kurala uygun, AI atıf satırı yok; CI yeşil ve sonucu beklendi (41660924636); göç yok; refactor yok.

K8 (yeni, D-39 (f)): geliştirme bindirmesi (`compose.dev.yaml`) veritabanının arşivini kapatmıyor; `.env.example`'daki yer tutucu depoya (`s3.example.com`) arşiv düşer, günlük dosyaları `pg_wal`'da birikir.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `BACKUP_S3_ENDPOINT`, `BACKUP_S3_REGION`, `BACKUP_S3_BUCKET`, `BACKUP_S3_URI_STYLE`, `BACKUP_RUN_TIME`, `BACKUP_TIME_ZONE` (`.env.example`, runbook tablosu)
- **Paketleme / dağıtım:** Var — veritabanı imajı giriş noktası ve betikler, yedek imajı betikleri, Compose `environment`/`command`
- **Yeni bağımlılık:** Yok (pgBackRest ve restic T06'dan; pgBackRest sürümü sabit değildir, `09 §17` madde 6 istisnası)

## Commit & PR

- **Branch:** `task/T07-yedek-duzeni`
- **Commit:** `501c24b` (doğrulanan head)
- **PR:** #351
- **CI:** dal run 41660924636 success (validator)
- **CI düzeltmesi:** Paketleme doğrulamasının kaynak taraması (`05 MK-10`) için tatbikat betiklerinde `latest` kalktı; yedek tatbikatı en yeni anlık görüntünün kimliğini önce bulup `restore`/`snapshots` ile onu kullanır.
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
| 1 | Veritabanı yedeğinin sonucu `backup_status_records`'a yazılmıyor | D-39 (a) | tablo ve `shopfolio_backup_status` kullanıcısı T10 / veri modeli task'ındadır |
| 2 | Depo 1 saklaması ve disk kullanımı gerçek sunucuda gözlenecek | D-39 (b) | kurulum denemesi (T09) |
| 3 | Uygulama dosya/kayıt klasör yolları | D-32 (a)/(b) kalan | dosya deposunu ve kaydı yazan task |
| 4 | Kova kuralı, anahtar yetkisi ve kurucu yetkili yedek denemeleri | D-01 | gerçek kova gerekir; kurulumda koşar |

## Notlar

- **Working tree (Adım -1):** temiz (devir alındı, dal `f6a9b67`)
- **Ana dal CI startup (Adım 0):** önceki yapım işçisinde; bu devamda yeniden izlenmedi
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** ⚠ kararlar PR açıklamasındadır (numaralı).
