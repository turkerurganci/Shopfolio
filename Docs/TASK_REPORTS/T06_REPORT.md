# T06 — Üç imaj ve kayıt defteri

**Faz:** F0 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `deploy/images/app/Dockerfile`: çok aşamalı; derleme `mcr.microsoft.com/dotnet/sdk:10.0.400-noble` (etiket `global.json`'ın SDK sürümü), çalışma `aspnet:10.0.14-noble`, ikisi etiket ve özetle sabit. Kilitli geri yükleme, `dotnet publish`, ve `dotnet ef migrations bundle` ile üretilen `efbundle` imajda (`/app/efbundle`, `0555`). Kullanıcı `app` (UID 1654); `ASPNETCORE_URLS=http://+:8080`.
- `deploy/images/database/`: resmî `postgres:18.6-trixie@sha256:…` + PGDG deposundan pgBackRest (2.60.0) + `scheduler.sh`. `track_commit_timestamp = on` dosyası (`postgresql.conf.d/10-commit-timestamp.conf`), ana dosya (`postgresql.conf`) dizini `include_dir` ile içerir; sunucu `-c config_file=` ile bu dosyadan başlar. Kullanıcı `postgres` (imaj `USER postgres`; resmî giriş betiği kök olmayan kullanıcıyla başlatıldı ve çalıştı).
- `deploy/images/backup/Dockerfile`: `alpine:3.24.3@sha256:…`, restic 0.19.1 ikilisi **SHA-256 ile sabit** (amd64 ve arm64; özet tutmazsa `sha256sum -c` derlemeyi düşürür), `scheduler.sh`. Kullanıcı `backup` (UID 10001).
- `deploy/images/shared/scheduler.sh`: iki imajda aynı dosya (`09 §17` madde 3) — `BACKUP_RUN_TIME`/`BACKUP_TIME_ZONE`, bir sonraki anı `date +%H %M %S` ile hesaplar, işi çalıştırır, tur bitince yeniden hesaplar; kron yok; POSIX. İşi (gecelik yedek betikleri) ve durum dosyası yazıcısı T07'dedir.
- `.dockerignore` (repo kökü): üç imajın bağlamı repo kökü olduğu için `.git`, `Docs`, `tests`, `bin/obj`, `.env*` (şablon hariç) ve sır desenleri bağlam dışı.
- `scripts/ci/package-build.sh` (`PACKAGE_BUILD_CMD`): kaynak taraması (`latest` yok, her `FROM` etiket+özet, sır değeri/anahtar bloğu yok) → üç imaj `docker build -f deploy/images/<ad>/Dockerfile .` → yanlış restic özeti derlemeyi düşürür (olumsuz sınama) → üç imajda kök olmayan kullanıcı → uygulama konteyneri CI'ın PostgreSQL hizmetine bağlanır, `/sistem/canlilik` 200 verene kadar (en çok 90 sn) beklenir → veritabanı imajında `SHOW track_commit_timestamp` = `on`, `pgbackrest version`, zamanlayıcı → yedek imajında `restic version`, zamanlayıcı.
- `scripts/ci/publish-images.sh` (`PUBLISH_CMD`): GHCR girişi `GITHUB_TOKEN` ile; `ghcr.io/<sahip>/shopfolio-{app,database,backup}`; ana dal → tam commit kimliği, `vA.B.C` etiketi → commit kimliği + `A.B.C`; `org.opencontainers.image.source` ve `revision` etiketleri; özet `GITHUB_STEP_SUMMARY`'ye.
- `ci.yml` Paketleme işine PostgreSQL 18 hizmeti ve `SHOPFOLIO_TEST_DATABASE`; geri alma sınaması için Migration dry-run işine `packages: read` + `GITHUB_TOKEN`. `publish.yml` Publish adımına `GITHUB_TOKEN`.
- `.github/ci-commands.env`: `PACKAGE_BUILD_CMD`, `PUBLISH_CMD` dolu. `SETUP_COMPLETE=false` kalır (T09).
- `Docs/09` Kütüphaneler tablosu restic satırı (lisans, sürüm, özetler) ve §17 madde 6 notu; `Docs/CI_CD_SETUP.md` §3.4 paket görünürlüğü adımı; `Docs/DEFERRED_BACKLOG.md` D-01 ilerleme notu, D-19 kararı, yeni D-27.
- `scripts/ci/migration-dry-run.sh` 4. bölüm (D-19): aşağıda.

## Etkilenen Modüller / Dosyalar

`deploy/images/**` (yeni), `.dockerignore` (yeni), `scripts/ci/package-build.sh`, `scripts/ci/publish-images.sh` (yeni), `scripts/ci/migration-dry-run.sh`, `.github/ci-commands.env`, `.github/workflows/ci.yml`, `.github/workflows/publish.yml`, `Docs/09_CODING_GUIDELINES.md`, `Docs/CI_CD_SETUP.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`. Uygulama kodu değişmedi.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Uygulama imajı çok aşamalı, kök olmayan kullanıcı, göç demeti | ✓ | Yerel: imaj `app` (UID 1654) ile çalıştı; `/app/efbundle` boş değil, demet çalıştırıldı ("No migrations were applied. The database is already up to date"); `package-build.sh` adım 4–5 |
| 2 | Veritabanı imajı: pgBackRest + zamanlayıcı, kesinleşme anı açık | ✓ | Yerel koşu: `SHOW track_commit_timestamp` → `on`; `pgbackrest version` → 2.60.0; `scheduler.sh` imajda; `package-build.sh` adım 6 her koşuda sınar |
| 3 | Yedek imajı: zamanlayıcı, restic SHA-256 ile sabit; yanlış özet derlemeyi düşürür; lisans ve sürüm tabloya | ✓ | Yanlış özetle derleme çıkış kodu 1 (yerelde ve `package-build.sh` adım 3); restic 0.19.1 `BSD-2-Clause` (deponun LICENSE dosyası okundu), `SHA256SUMS` ile indirilen arşivin `sha256sum`'u uyuştu; `09` tablosu güncellendi |
| 4 | Taban imajlar etiket+özet; `latest` yok | ✓ | `package-build.sh` adım 1 (her `FROM` için `etiket@sha256:` zorunlu; `deploy` altında `latest` sözcüğü yok) |
| 5 | Derleme bağlamı repo kökü; Dockerfile `src/` altında değil | ✓ | Üç derleme `docker build -f deploy/images/<ad>/Dockerfile .` |
| 6 | Sır girmez, kullanıcılar kök değil | ✓ | Kaynak taraması (sır adı+değer, anahtar bloğu, AKIA); `docker image inspect` kullanıcıları: `app`, `postgres`, `backup`; parolalar/anahtarlar dosya olarak bağlanacak (imajda yok) |
| 7 | Kayıt defteri GHCR; görünürlük adımı yazılı | ✓ | `publish-images.sh`, `CI_CD_SETUP.md` §3.4 (açılış T09) |
| 8 | CI Paketleme adımı üç imajı derler, canlılık 200; yayın komutları | ✓ (yerel) / ? (CI) | Yerel: `package-build.sh` baştan sona geçti (PostgreSQL = yerel derlenen veritabanı imajı); `publish-images.sh` kuru koşusu. **Actions'ta koşusu ve ana dal merge'indeki gerçek yayın bu PR'ın CI'ında / merge sonrası gözlenir** |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | — | — | Task tanımı: test beklentisi yok (imaj tanımı) |
| Integration | — | — | — |
| Paketleme (yerel) | ✓ | `bash scripts/ci/package-build.sh` | 7/7 adım geçti, ~40 sn (önbellekli) |
| Zamanlayıcı işlevleri | ✓ | geçici betikle (repoda değil) debian `dash` ve alpine `ash` | önce/gece yarısı aşan/saat az önce geçmiş/tam şimdi/`08:09` (sekizlik tuzağı)/geçersiz saat `24:00` |
| Yayın kuru koşu | ✓ | `PUBLISH_DRY_RUN=1 … publish-images.sh` | üç imaj commit kimliği ve sürüm etiketiyle etiketlendi |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (yeni ayar yok; zamanlayıcı `BACKUP_RUN_TIME` ve `BACKUP_TIME_ZONE`'u okur — bu ayarları ayar tablosuna ve `.env.example`'a ekleyen T07)
- **Paketleme / dağıtım:** Var — üç imaj, GHCR yayını (ana dal merge'inde ilk gerçek yayın)
- **Yeni bağımlılık:** Var — restic 0.19.1 (BSD-2-Clause, SHA-256 sabit), pgBackRest 2.60.0 (PGDG deposu, MIT), taban imajlar; NuGet bağımlılığı yok

## Commit & PR

- **Branch:** `task/T06-uc-imaj-kayit-defteri`
- **Commit:** PR'da son commit
- **PR:** #PRNO
- **CI:** yapım işçisi izlemez (yönetici izler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | pgBackRest'in resmî konteyner imajı yok (K-937) | Docker Hub `pgbackrest` kuruluşu: yalnız `test` ("Containers for Development and Testing") ve `doc` imajları (`hub.docker.com/v2/repositories/pgbackrest/`, 2026-11-03); proje deposunda işletim imajı yok | ✓ imaj projede derlenir |
| 2 | `track_commit_timestamp` veritabanı imajında açılabilir (K-962) | İmaj çalıştırıldı: `SHOW track_commit_timestamp` → `on` (ayar dosyası `postgresql.conf.d/`, sunucu `config_file` ile) | ✓ |
| 3 | restic lisansı ve sürümü | Deponun LICENSE dosyası "BSD 2-Clause License"; `gh api …/releases` → v0.19.1, 2026-07-05, önceki v0.19.0 2026-06-09; `SHA256SUMS` ile indirilen `.bz2`'lerin `sha256sum`'u aynı | ✓ |
| 4 | pgBackRest PGDG deposunda var ve postgres:18.6-trixie imajında kurulur | Derleme günlüğü, `pgbackrest version` → 2.60.0; son sürüm `release/2.60.0` 2026-10-18 | ✓ |
| 5 | Taban imaj sürümleri var | `docker buildx imagetools inspect` özetleri; `sdk:10.0.400-noble` (= `global.json`), `aspnet:10.0.14-noble`, `postgres:18.6-trixie` (= `postgres:18`'in özeti), `alpine:3.24.3` | ✓ |
| 6 | GHCR paket görünürlüğü repodan geçmez; ilk yayın paketi özel doğurur | Bilgim tarihli (05 §8.2.5); **bu PR'da sınanamaz** (ilk yayın ana dal merge'inde). Adım `CI_CD_SETUP.md` §3.4'te; ilk yayından sonra gözlenip D-01 notunda kapanır | ? Doğrulanamadı (ilk yayına bağlı) |
| 7 | Actions `ubuntu-latest`'te `docker`, `curl`, `--network host` kullanılabilir | GitHub barındırılan Linux runner standart içeriği; **Actions'ta bu PR'ın CI'ında gözlenir** | ? |
| 8 | Yerel (Windows/Docker Desktop) sonucu CI (Linux) ile aynı | İmajlar Linux'tur; yalnız ağ kipi farklı (yerelde kullanıcı ağı, CI'da `host`) | ~ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Önceki sürümün entegrasyon testlerinin yeni şemaya karşı koşması (D-19 B kısmı) | D-19 | Test altyapısı değişikliği; ilk etikete dek sınanamaz. Yayın varken adım düşer (kararı aşağıda) |
| 2 | Zamanlayıcıyı veritabanı konteynerinde başlatan düzen ve yedek işleri | — (T07) | `database-job.sh`, `file-backup-job.sh`, `write-status.sh` ve arşiv ayarları T07'nin kapsamı |
| 3 | Uygulama imajında HEALTHCHECK yok (`curl` yok); sağlık denetimi Compose tanımında | — (Compose görevi) | Compose tanımı T05/ilgili task |
| 4 | `libgssapi_krb5` açılış satırı | D-27 | Zararsız; karar kayıt düzeninde |
| 5 | Paket görünürlüğü açılışı | D-01 | T09 |

### D-19 kararı (⚠ 1)

`migration-dry-run.sh` 4. bölüm: yayımlanmış sürüm varken (A) önceki sürümün uygulama imajı kayıt defterinden çekilir, yeni şemaya karşı açılır, canlılık 200 beklenir; (B) önceki sürümün entegrasyon testlerinin koşması bağlanmadı ve bölüm bu noktada `exit 1` verir (uyarıyla yeşil kalmaz). Sonuç: ilk sürüm etiketi atıldığında 6. adım (B) bağlanana dek kırmızıdır. (A) yerelde yalnız söz dizimi ve `package-build.sh`'taki aynı canlılık döngüsüyle sınandı (yerelde `psql` ve yayımlanmış sürüm yok).

### ⚠ Dokümanda kararlaştırılmamış seçimler

1. **D-19:** yayın varken geri alma sınaması (B) bağlanana dek adımı düşürür (yukarıda).
2. **İmaj adları:** `shopfolio-app`, `shopfolio-database`, `shopfolio-backup` (`ghcr.io/<sahip>/…`); dokümanlarda ad geçmiyor. İlk yayından sonra değiştirmek yeni paket demektir.
3. **pgBackRest apt sürümü sabitlenmedi:** PGDG eski sürümleri tutmadığı için sabit sürüm derlemeyi zamanla düşürürdü; yeniden üretilebilirlik taban imaj özetinden gelir, kurulan sürüm derleme günlüğüne yazılır (`pgbackrest version`). İstenirse sürüm kısıtı eklenir.
4. **Veritabanı yapılandırması:** `postgresql.conf` + `postgresql.conf.d/` ve sunucunun `-c config_file=` ile başlatılması; imaj `USER postgres` (kök olmayan kullanıcı). `09 §17` yalnız `postgresql.conf.d/` dizinini adlandırır; mekanizma benim seçimim.
5. **Taban imaj çeşitleri:** .NET için `noble` (Ubuntu), PostgreSQL için `trixie` (Debian; resmî imajın varsayılan çizgisi), yedek aracı için `alpine`. `chiseled` (kabuksuz) .NET imajı seçilmedi: Compose sağlık denetimi ve hata ayıklama için kabuk gerekebilir.

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 41588356448 (CI, success), 41588356390 (Publish, success), 41552961812 (CI, success).
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Dal izolasyonu:** yalnız T06.
- **Mini güvenlik kontrolü:** sır yok (CI bağlantı dizesi parolasızdır ve yalnız o işin geçici hizmetine bağlanır); kayıt defteri kimliği yalnız iş akışının `GITHUB_TOKEN`'ı (`packages: write` yalnız Publish işinde); yeni dış bağımlılıklar taban imajlar, restic (özet sabit), pgBackRest (PGDG, kurulu imajın taban özetiyle yeniden üretilebilir; apt sürümü sabitlenmedi — ⚠ 3).
- **Uzun derleme:** yerel ilk SDK imajı çekimi 170 sn'yi aştı; ikinci denemede önbellekle 40 sn.
