# T04 — CI hattı ve eşitlik denetimleri

**Faz:** F0 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `.github/ci-commands.env`: Lint (`restore --locked-mode`, `dotnet format --verify-no-changes`, `Consistency` özellikli testler), Build (kilitli geri yükleme, `NuGetAudit` tüm düzey/geçişli, uyarılar hata, Release), Unit (`--filter-not-trait "Consistency=true"`), Integration (`dotnet tool restore` + entegrasyon projesi), Contract `NOT_APPLICABLE`, Migration dry-run (`scripts/ci/migration-dry-run.sh`). Paketleme, E2E, Publish boş (T06; uyarıyla atlanır).
- `ci.yml`: `actions/setup-dotnet@v4` (`global-json-file`), `postgres:18` hizmeti (4. ve 6. adım), `TESTINGPLATFORM_TELEMETRY_OPTOUT=1`, migration işinde `fetch-depth: 0`.
- `scripts/ci/migration-dry-run.sh`: model–göç farkı (`has-pending-model-changes`), demet boş veritabanına, son yayımlanan `vX.Y.Z` etiketinin demeti + yeni demet; yayın yoksa atlama satırı; geri alma bölümü T06'ya kadar `::warning`.
- `publish.yml`: `push: main` + `tags: vX.Y.Z`, `packages: write`; etikette `patch-migration-check` işi (`scripts/ci/patch-migration-check.sh`), publish ona bağlı.
- `.github/dependabot.yml`: nuget + github-actions, haftalık.
- D-08 (`rollForward: disable`), D-09 (IDE0130 `warning`), D-14 (09 §3.2 yazıldı), D-15 (telemetri), D-16 (`SkippedDataRow` kuralı), D-17 (koleksiyon nitelikleri).
- `09` §3.2, §19.1 madde 6, §21.4 madde 1 güncellendi.

## Etkilenen Modüller / Dosyalar

`.github/ci-commands.env`, `.github/dependabot.yml`, `.github/workflows/ci.yml`, `.github/workflows/publish.yml`, `scripts/ci/migration-dry-run.sh`, `scripts/ci/patch-migration-check.sh`, `.editorconfig`, `global.json`, `tests/Shopfolio.UnitTests/Architecture/{ForbiddenTestPatterns.cs, ForbiddenTestPatternScanTests.cs, ViolationSamples/ForbiddenPatterns/ForbiddenTests.cs}`, `tests/Shopfolio.IntegrationTests/Database/{TestDatabaseSettingsTests.cs, MigrationBundleTests.cs}`, `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `ci-commands.env` Paketleme/E2E dışı dolu; Contract NOT_APPLICABLE | ✓ | `.github/ci-commands.env` |
| 2 | Lint: format + Consistency; Unit onları dışarıda bırakır | ✓ | Lint 9 test, Unit 15 test (toplamda çakışmaz); bozulan eşitlik (props'a tablo dışı paket) Lint testini kırdı (1 başarısız), Unit yeşil kaldı; düzeltme yeşil |
| 3 | Build: Release, uyarı hata, kilitli geri yükleme, bilinen açık taraması | ✓ | 0 uyarı/0 hata; `Newtonsoft.Json 9.0.1` eklenince `NU1903 Warning As Error`, geri yükleme düştü |
| 4 | Integration gerçek PostgreSQL 18'e karşı | ✓ (yerel) / ? (Actions) | yerelde `postgres:18` konteyneri, 14/14; Actions hizmeti bu PR'ın CI'ında görülür |
| 5 | Migration dry-run: boş + son sürüm şeması + geri alma; sürüm yoksa yalnız ilki, atlama yazılır; göçsüz model adımı düşürür | ~ | boş DB: ✓; yayın yok: `SKIPPED:` satırları ✓; yerel `v0.0.1` etiketiyle önceki-şema yolu ✓; göçsüz model: `has-pending-model-changes` çalışır (şu an değişiklik yok → geçer), düşürme yolu betikte; geri alma imajı bölümü T06 (D-19) |
| 6 | Paketleme/yayın komutları T06'da; boş komut uyarıyla atlanır | ✓ | `PACKAGE_BUILD_CMD=`/`PUBLISH_CMD=` boş, `ci-run.sh` uyarı verir |
| 7 | publish.yml yama etiketinde göç eklenmişse düşer; `migration-fix` istisnası; ilk etikette koşmaz | ✓ | geçici depoda 6 etiket: patch=0 geçer, göçsüz yama geçer, göçlü yama exit 1, annotated + `migration-fix: K-123` uyarıyla geçer, hafif etiket düşer |
| 8 | publish.yml main merge + sürüm etiketi; yazma izni | ✓ | `on.push.branches/tags`, `permissions.packages: write` |
| 9 | Dependabot açık; boş komut kapıyı geçmez | ✓ | `dependabot.yml`; `ci-run.sh` `SETUP_COMPLETE=true` + boş → exit 1 (değişmedi) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Lint | ✓ | `ci-run.sh LINT_CMD` | format temiz; Consistency 9/9 |
| Build | ✓ | `ci-run.sh BUILD_CMD` | 0 uyarı, 0 hata |
| Unit | ✓ | `ci-run.sh UNIT_TEST_CMD` | 15/15 |
| Integration | ✓ | `ci-run.sh INTEGRATION_TEST_CMD` (PostgreSQL 18, `SHOPFOLIO_TEST_DATABASE`) | 14/14 |
| Contract | — | NOT_APPLICABLE | |
| Migration dry-run | ✓ | `bash scripts/ci/migration-dry-run.sh` | tag yokken ve `v0.0.1` ile |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — CI: `SHOPFOLIO_TEST_DATABASE`, `PGHOST/PGPORT/PGUSER`, `TESTINGPLATFORM_TELEMETRY_OPTOUT`, `DOTNET_CLI_TELEMETRY_OPTOUT`
- **Paketleme / dağıtım:** `publish.yml` tetikleyicileri; imaj yok (T06)
- **Yeni bağımlılık:** `actions/setup-dotnet@v4` (resmî GitHub eylemi); NuGet paketi yok

## Commit & PR

- **Branch:** `task/T04-ci-hatti`
- **Commit:** `532cfe1` — T04: CI hattı ve eşitlik denetimleri
- **PR:** #339
- **CI:** yapım işçisi beklemez (yardımcı izler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `dotnet test --project … --filter-trait/--filter-not-trait` (MTP) çalışır | yerelde Lint 9 test / Unit 15 test, ayrı kümeler | ✓ |
| 2 | `TESTINGPLATFORM_TELEMETRY_OPTOUT` MTP'de gerçek ad | `Microsoft.Testing.Platform.dll` 1.9.1 içinde dize bulundu (UTF-16 tarama) | ✓ |
| 3 | NuGet bilinen açık taraması geçişli paketleri kapsar ve uyarılar hata olunca geri yüklemeyi düşürür | `NU1903` örneği (Newtonsoft.Json 9.0.1) | ✓ |
| 4 | `dotnet ef migrations has-pending-model-changes` EF 10'da var | betik çalıştı, "No changes have been made" | ✓ |
| 5 | IDE0130 `warning` hem `dotnet format --verify-no-changes` hem Release derlemesinde hata verir | geçici yanlış ad alanı dosyası: ikisi de `error IDE0130` | ✓ |
| 6 | Docker Hub `postgres:18` imajı var | yerelde çekildi ve çalıştırıldı | ✓ (Actions hizmeti ? — CI'da görülür) |
| 7 | `actions/setup-dotnet@v4` `global-json-file` ve `10.0.400` sürümünü kurar | yerelde sınanamaz | ? (bu PR'ın CI'ında görülür) |
| 8 | ubuntu-latest'te `psql` ve `git worktree` var | yerelde `psql` shim; runner imajında psql yüklü bilgisi | ? (CI'da görülür) |
| 9 | Dependabot NuGet kilit dosyasını günceller | ilk Dependabot PR'ı yok | ? → D-07 açık |
| 10 | `actions/checkout` tag çağrısında açıklamalı etiket nesnesini korur | `git fetch --force --tags` adımı eklendi; Actions'ta etiketle sınanmadı | ? (ilk sürüm etiketinde görülür) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Geri alma sınamasının imaj ve önceki sürüm testi bölümü | D-19 | imajlar T06'da |
| 2 | Dependabot'un kilit dosyasını güncellemesi | D-07 | ilk Dependabot PR'ı gerekir |

## Notlar

- **Working tree (Adım -1):** temiz; hook yolu `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 41542556242 success, 41542556291 success, 41528790256 success.
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **⚠ kararlar:** PR #339 açıklamasında listelendi (rollForward disable, IDE0130 warning, geri alma imaj bölümü D-19, NuGet audit komut özellikleri, D-17 koleksiyon, parolasız CI PostgreSQL, D-07 açık, etiket seçimi).
- `SETUP.md` değiştirilmedi; `SETUP_COMPLETE=false`.
