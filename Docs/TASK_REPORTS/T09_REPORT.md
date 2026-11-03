# T09 — İlk iniş provası: yerel referans kurulumda canlılık

**Faz:** F0 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Yerel referans kurulum betikleri** (`scripts/local-reference/`): `host-up.sh` (ayrıcalıklı `docker:dind` sunucu `t09-host`, MinIO `t09-minio`, ağ `t09-net` IPv4+IPv6, imajların `save` → `load`), `storage-setup.sh` (kova, iki anahtar, `bucket-checks.sh`), `host-secrets.sh` ve `install-installation.sh` (uydurma sırlar, `DOCKER-USER` kuralı, ilk kurulum bindirmesi, `up -d`), `checks.sh` (liveness, secrets, bootstrap, image-ids, isolation, egress, redirect-hsts, client-address), `client-address.sh`, `host-image-ids.sh`, `down.sh` (adıyla temizlik).
- `deploy/compose.local-reference.yaml` (yalnız yerel: MinIO CA güveni, `extra_hosts`), `deploy/firewall/installation-checks.sh` (`SKIP_REQUESTS`, KK numaraları).
- **Bulunan ve düzeltilen hata:** Caddy erişim kaydı varsayılan 0600 root yazıyordu; `backup` (uid 10001) `access.log`'u okuyamadı (`file-backup-job.sh` çıkış 3). `deploy/caddy/Caddyfile` erişim kaydına `mode 0644`.
- `DEPLOY_RUNBOOK.md` §K yerel referans kuruluma göre baştan yazıldı (§K.10 prova tablosu, §I.3, §I.4, §J ifadeleri); `CI_CD_SETUP.md` §3.1 ve §3.4 gerçek sunucu T603'e hizalandı.
- `.github/ci-commands.env`: `SETUP_COMPLETE=true`, `E2E_CMD=NOT_APPLICABLE`; `SETUP.md` §2, §4, §6, parametre kaydı ve durum bloğu.
- Backlog: D-01, D-29, D-30, D-31, D-34 ✅; D-36 (b) ✅; D-39 (b)(c) yerelde; D-40'a (g)–(k) eklendi (MinIO farkları ve 7 günlük yarım yükleme kuralı, systemd birimi ve zincir boşluğu, `release.env`, `journald` ACL'i, gerçek IPv6).

## Etkilenen Modüller / Dosyalar

`scripts/local-reference/*`, `deploy/compose.local-reference.yaml`, `deploy/caddy/Caddyfile`, `deploy/firewall/installation-checks.sh`, `.github/ci-commands.env`, `SETUP.md`, `Docs/DEPLOY_RUNBOOK.md`, `Docs/CI_CD_SETUP.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ayrıcalıklı Linux konteyneri, iç Docker + iptables, IPv4+IPv6; MinIO; yerel alan adı + Caddy iç CA; sahte sırlar; etiket yok | ✓ | `host-up.sh` (iç Docker 29.9.2, iptables nf_tables, `t09-minio` TLS); `docker ps` WSL yok; etiket basılmadı, paket görünürlüğü değişmedi; `DEPLOY_RUNBOOK` §K.1 |
| 2 | Boş uygulama imajı canlılık ucuna 200 | ✓ | konteyner içi `HTTP/1.1 200 OK`; `https://shopfolio.internal/sistem/canlilik -> 200` (iki tur; §K.2) |
| 3 | İlk yayından önceki CI'da geri alma sınaması atlanır, CI çıktısında yazılı | ~ | Satırın kaynağı `scripts/ci/migration-dry-run.sh:70` (`::notice … skipped`); ana dal koşusu `41681819821` yeşil ve T04 doğrulamasında satırın görüldüğü koşu `41548360499`. **Bu oturumda koşu günlüğünün kendisi okunamadı** (yapım ajanı dal CI'ını/günlüğünü yoklayamaz); satırın günlükte görülmesi doğrulayıcıya bırakıldı |
| 4 | Üç imaj kimliği prova kaydında | ✓ | §K.6: app `4ee0b1ad…`, database `b89b75a6…`, backup `45a6c9da…`; dış `.Id` = iç çalışan yapılandırma özeti (iki tur) |
| 5 | Prova runbook'ta; `SETUP_COMPLETE=true`; CI kapısı yeşil; D-01 kapalı, artıklar D-40 | ~ | §K.10, `SETUP_COMPLETE=true`, D-01 ✅, D-40 (g)–(k). **CI kapısının yeşilliği** bu PR'ın CI koşusunda görülür (yapım ajanı beklemez) |
| 6 | `DOCKER-USER` kuralı uygulanır; veritabanı ve yedek yalnız MinIO'ya çıkar | ✓ | `egress` 6 `ok:` (iki ağdan `1.1.1.1:80/443` reddedilir, MinIO geçer); kural adla uygulandı, yedekler `t09-minio` adıyla yazdı; yeniden başlamada zincir boşalır → `apply` (§K.3, §K.8) |
| 7 | IPv4 ve IPv6 isteğinin istemci adresi erişim kaydında ve uygulamada aynı | ✓ | vekil kaydı `172.31.90.4` ve `fd09:0:0:90::4`; uygulamaya giden `X-Forwarded-For` aynı iki adres (paket yakalama); uygulamanın kendi kaydı T215 (⚠ 5) (§K.4) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-trait "Consistency=true"` | 34/34 |
| Unit (diğer + Architecture) | ✓ | `… --filter-not-trait "Consistency=true"` | 68/68 |
| Betik testleri | ✓ | `sh tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Kurulum denemeleri | ✓ | `checks.sh` (§K.3–K.6), yedek komutları (§K.7) | `/tmp/t09-state/*.log` (yerel, kayıt dışı) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** `SETUP_COMPLETE=true`, `E2E_CMD=NOT_APPLICABLE`; Caddy erişim kaydı `mode 0644`
- **Paketleme / dağıtım:** Yerel referans kurulum betikleri ve `compose.local-reference.yaml` (yalnız yerel)
- **Yeni bağımlılık:** Yerel deneme için `bitnamilegacy/minio`, `docker:dind`, `curlimages/curl:8.21.0`, `amazon/aws-cli`; hiçbiri üretime girmez

## Commit & PR

- **Branch:** `task/T09-ilk-yayin`
- **PR:** PR açıklamasında
- **CI:** yapım ajanı izlemez

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | MinIO resmî imajı kullanılabilir | Docker Hub / indirme 410 (2026) | ✗ → `bitnamilegacy/minio` yalnız yerelde |
| 2 | Docker Desktop ortamında ayrıcalıklı dind, iç iptables ve IPv6 çalışır | `host-up.sh`, `egress`, `client-address` koşumları | ✓ |
| 3 | Npgsql `PGPASSWORD`'ü okur (D-31 (g)) | `migrate` çıkış 0 | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Etiket, paketler, firmanın kovası, VPS, gerçek alan adı ve IPv6, systemd birimi, `journald` ACL'i | D-40 (a)–(k) | Gerçek sunucu T603 |
| 2 | Yedi günlük yarım yükleme kuralı MinIO'da tutmuyor | D-40 (g) | Gerçek kovada sınanır |
| 3 | Uygulamanın kendi kaydında istemci adresi | D-31 (d) → T215 | Alanı uygulama yazar |
| 4 | Egress denemesinde ad yolu yalnız yedek işleriyle kanıtlı | — | Throw-away konteyner dış ağın adını çözmez |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** son koşu `41681819821` yeşil (`65250d8`).
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Okunan bölümler ve `09 §25`:** PR açıklamasında.
