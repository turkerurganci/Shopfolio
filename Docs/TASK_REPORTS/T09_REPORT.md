# T09 — İlk iniş provası: yerel referans kurulumda canlılık

**Faz:** F0 | **Durum:** ✓ PASS (doğrulama tur 2) — merge `93ba91f` (#354) | **Tarih:** 2026-11-03

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
| Doğrulama durumu | ✓ PASS — tur 2 (2026-11-03, head `303c074`); tur 1 ✗ FAIL (head `a4632b1`) |
| Bulgu sayısı | 0 (tur 1'in bulgu 1'i ve K1–K5'i kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (tur 2; `DEFERRED_BACKLOG.md` D-42'ye forward) |

### Doğrulama tur 1 — ✗ FAIL (2026-11-03)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un hook klasörü); ana dal son 3 run `41681819821`, `41681819807`, `41675738538` success; `MEMORY.md`'de T09 satırı var.

**Kabul kriterleri (validator kanıtı, kurulum validator tarafından reçeteyle sıfırdan kuruldu, 2026-11-03 17:54Z–18:03Z):** 1–7 ✓, KK1–KK7 ✓. Üç imaj dal Dockerfile'larından derlendi (kimlikler §K.6 ile birebir); `host-up` → `storage-setup` → `install-installation` → `checks.sh` hepsi çıkış 0: `t09-host` `privileged=true`, `docker:dind`, `daemon.json` ipv6/ip6tables açık; WSL'de yalnız önceden var olan `Ubuntu` (2023) ve `docker-desktop`; `git ls-remote --tags` 0. Canlılık konteyner içi `HTTP/1.1 200 OK`, alan adıyla `-> 200`; `migrate` çıkış 0. `image-ids` 3 `ok:`; çalışan konteynerin kendi `.Image`'ından ayrıca `docker save` ile yapılandırma özeti okundu, dış `.Id` ile eşit. `egress` 6 `ok:` + kontrol: süzülmeyen ağdan (`bridge`, `shopfolio-edge`) `1.1.1.1:443` geçer, `db-egress`'ten reddedilir; çalışan `backup` ve `database` konteynerlerinden `1.1.1.1:443` reddedilir, `t09-minio:443` geçer; MinIO'nun 9001'i süzülen ağdan reddedilir. `client-address`: vekil kaydı `172.31.90.4` / `fd09:0:0:90::4`, uygulamaya giden `X-Forwarded-For` aynı iki adres. Kriter 3: dal CI'ı `41687547294` günlüğünde Migration dry-run `##[notice]No published release yet … rollback test are skipped`. Kriter 5: aynı koşuda CI Gate success, Contract ve E2E `NOT_APPLICABLE` notu; D-01 ✅, artıklar D-40. Ayrıca yedek (iki depoya tam yedek, `check` 0, `cipher: mixed`), dosya yedeği `success` (`access.log` `-rw-r--r--`), iki kurtarma provası çıkış 0, sunucu yeniden başlayınca `DOCKER-USER` boş (§K.8 iddiası doğru). Temizlik `down.sh all` + `docker rmi`; `skinora-*` dokunulmadı.

**Bulgu 1 (DÜŞÜK, S1 Sapma; `09 §25` "Kod hiçbir doküman cümlesiyle çelişmiyor mu" → hayır):** `.github/ci-commands.env` `E2E_CMD=NOT_APPLICABLE` gerekçesi "Uctan uca dogrulama fazi yok (11)"; aynı iddia `DEPLOY_RUNBOOK §K.11` ("uçtan uca faz yok"), D-01 satırı ve PR ⚠ karar 2'de. `11 §3` F6'nın adı "Uçtan uca doğrulama"dır, T601'in test beklentisi E2E'dir ve `05 §8.3.11` / §2.3.5 E2E adımının "uçtan uca doğrulama fazında bağlanır" dediğini yazar; `SETUP.md` §4'e göre `NOT_APPLICABLE` "bu katman projede yok" demektir. Bağlama yükümlülüğü hiçbir yerde izlenmiyor. Ayrıca E2E işi advisory'dir (`ci.yml` `continue-on-error`, `ci-gate.needs` dışında): boş komut yalnız advisory işi kırar, kapıyı değil. Düzeltme: gerekçe metinleri F6'da bağlanacağını yazsın (`05 §8.3.11`) ve bağlama bir backlog kalemiyle (F6 / T601) izlensin.

**K-notları (bu PR'ın düzeltme turunda kapatılır):** K1 `down.sh all` `t09-minio`'yu `docker rm -f` ile `-v`'siz siler; `bitnamilegacy/minio` imajının `VOLUME /bitnami/minio/data`'sı her turda adsız birim bırakır (validator turu 1, yapım turları 2 birim bıraktı: `7ce3c9ab…`, `9b4b3c58…`). K2 D-40 (f) ve (k) aynı maddedir ("gerçek IPv6 isteğinde istemci adresi"). K3 Üç paket kimliksiz çekilebiliyor (GHCR anonim belirteçle `65250d8` manifesti HTTP 200, üçü de): `05 §8.2.5` ve T06 dış varsayım 6'nın "ilk yayın paketi özel doğurur" iddiası tutmuyor; D-40 (b) ve `CI_CD_SETUP.md` §3.4 "açık yap" adımı "doğrula"ya çevrilmeli (T09 görünürlüğü değiştirmedi). K4 `DEPLOY_RUNBOOK §J.5` prova kaydı tablosu boş; §K.7'deki iki provanın satırı oraya yazılmadı. K5 Yerel betikler sabitlenmemiş imajlar kullanır (`docker:dind`, `bitnamilegacy/minio:latest`, `minio-client:latest`, `amazon/aws-cli:latest`, `alpine`); ayrıca `deploy/compose.local-reference.yaml` `09 §4.3` `deploy/` ağacında ve `CONTEXT.md` §3 `deploy/` satırında yok (başlığı "gerçek sunucuda asla" der; `09 §4.3` `deploy/`'u sürümle gelen ürün dosyası sayar — yer ya da ağaç satırı netleşmeli).

**Yapım raporu karşılaştırması:** kriter 1, 2, 4, 6, 7 uyumlu; yapımın `~` bıraktığı 3 ve 5'i validator CI günlüğü ve CI Gate ile ✓ yaptı; uyuşmazlık 1 — yapım ⚠ karar 2'yi ve PR'ın `09 §25` "çelişmiyor" cevabını uygun saydı, validator `05 §8.3.11` / `11` F6 ile çelişki sayar.

### Düzeltme turu 1 (2026-11-03)

- **Bulgu 1:** seçilen çözüm — `E2E_CMD=NOT_APPLICABLE` kalır ama gerekçe düzeldi: E2E katmanı henüz kurulmadı, komut F6'da (T601) bağlanır (`05 §8.3.11`); `NOT_APPLICABLE` bir yer tutucudur. Gerekçe: advisory bir yer tutucu komut (`true` benzeri) "yaniltici yesil" doğurur; boş komut advisory işi düşürürdü; `NOT_APPLICABLE` kapıyı ve `ci.yml`'i değiştirmeden en dar çözümdür. Bağlama yeni **D-41** (T601, F6) ile izlenir. Metinler: `ci-commands.env`, `SETUP.md` §4/§8, `DEPLOY_RUNBOOK` §K.11, D-01 satırı.
- **K1:** `down.sh all` `docker rm -f -v` kullanır; bırakılan iki eski adsız birim (`7ce3c9ab…`, `9b4b3c58…`; boş, kapsayıcısız, MinIO imajının `/bitnami/minio/data` birimi) silindi. Yeniden kurulum + `down.sh all` sonrası yeni adsız birim kalmadı.
- **K2:** D-40 (f) ve (k) birleşti (uygulamanın kendi adres değeri, T603 kriter 7, (f)'de).
- **K3:** GHCR paketlerinin kimliksiz çekilebildiği gözlemi `CI_CD_SETUP.md` §3.4, D-40 (b) ve T06 raporu dış varsayım 6'ya yazıldı; `05 §8.2.5`/MK-11 metni D-40 (b)'de "yansıtılacak" olarak izlenir; `11` ve paket görünürlüğü değişmedi.
- **K4:** `DEPLOY_RUNBOOK` §J.5 prova tablosuna iki satır (veritabanı 8 sn, dosya 1 sn; yaşam döngüsü sütunu D-40 (g)).
- **K5:** (a) yerel betiklerdeki araç imajları etiket + özetle sabitlendi (`docker:29.9.2-dind`, `bitnamilegacy/minio:2025.5.24`, `bitnamilegacy/minio-client:2025.5.21`, `amazon/aws-cli:2.39.3`, `alpine:3.24.3`, `curlimages/curl:8.21.0`; özetler etiketle aynı doğrulandı); (b) `deploy/compose.local-reference.yaml` `09 §4.3` ağacına ve `CONTEXT.md` §3 `deploy/` satırına eklendi.
- **Sınama:** `dotnet build -warnaserror` 0/0, Unit 102/102, betik testleri 4 dosya; reçete sıfırdan koşuldu (üç imaj kimliği §K.6 ile aynı, `host-up`, `storage-setup` "All checks passed", `install-installation`, `liveness` 200, `egress` 6 `ok:`, `client-address` çıkış 0, iki kurtarma provası), sonunda `down.sh all`; `skinora-*` dokunulmadı.

### Doğrulama tur 2 — ✓ PASS (2026-11-03; yeniden doğrulama, kapsam tam)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks`; ana dal son 3 run `41688713398`, `41681819821`, `41681819807` success; `MEMORY.md` satır 22'de T09 satırı var. Doğrulanan head `303c074` (PR #354).

**Kabul kriterleri (validator kanıtı; kurulum validator tarafından reçeteyle sıfırdan kuruldu, 2026-11-03 18:23Z–18:33Z, Docker Desktop 4 GB):** 1–7 ✓, KK1–KK7 ✓.
1. Üç imaj `docker build -f deploy/images/<ad>/Dockerfile` (paketleme betiği adım 2'nin komutu) → `4ee0b1ad…`, `b89b75a6…`, `45a6c9da…` (§K.6 ile birebir). `t09-host` `privileged=true`, imaj `docker:29.9.2-dind@sha256:9e79b8…`; iç Docker 29.9.2, `iptables`/`ip6tables` v1.8.13 (nf_tables), `daemon.json` ipv6/ip6tables açık; `t09-net` IPv4 + IPv6. `wsl -l -v`: yalnız önceden var olan `Ubuntu` ve `docker-desktop`. MinIO `bitnamilegacy/minio:2025.5.24@sha256:451fe6…`; alan adı `shopfolio.internal`, Caddy iç CA kökü ile `curl --cacert`; ödeme/e-posta/Google sırları `t09-made-up-…`. `git ls-remote --tags origin` = 0; diff'te yayın ya da görünürlük adımı yok.
2. `checks.sh liveness`: dört hizmet Up/healthy, `migrate` `Exited (0)`; konteyner içi `HTTP/1.1 200 OK`; `https://shopfolio.internal/sistem/canlilik -> 200`.
3. Dal CI `41692602124` (head `303c074`) günlüğü: `6. Migration dry-run … ##[notice]No published release yet (no vMAJOR.MINOR.PATCH tag): … the rollback test are skipped.`
4. `checks.sh image-ids` 3 `ok:`; ayrıca iç Docker'da çalışan konteynerin `.Image`'ı başvurunun `.Id`'siyle eşit (`c4b11a35…`, `3cdf8d09…`, `6e4eddfa…` = §K.6 iç manifest sütunu).
5. §K.10 prova tablosu; `.github/ci-commands.env` `SETUP_COMPLETE=true`; `41692602124`: 9 iş success + Guard skipped, `CI Gate` success ("tum zorunlu job'lar gecti"), Contract ve E2E `NOT_APPLICABLE` notu, boş komut hatası/uyarısı yok; D-01 ✅, artıklar D-40 (a)–(j), D-41; aktif kalem sayımı tuttu.
6. `status`: iki çıkış alt ağı → `SHOPFOLIO-EGRESS`, `RETURN 172.31.90.2:443`, gerisi `REJECT`; `checks.sh egress` 6 `ok:`, DNS 2 adres. **Kontrol:** süzülmeyen `shopfolio-edge` ve `bridge`'den `1.1.1.1:443` geçer; çalışan `database`'ten `1.1.1.1:443`, `8.8.8.8:53`, `t09-minio:9001` reddedilir, `t09-minio:443` geçer; çalışan `backup`'tan aynı (REJECT sayacı 20). İç Docker'da `db-egress`/`backup-egress` `EnableIPv6=false`. Yedekler adla depoya yazdı (aşağıda). `docker restart t09-host` sonrası `DOCKER-USER` boş, `SHOPFOLIO-EGRESS` yok, `apply` ile geri geldi (§K.8 doğru).
7. `checks.sh client-address` çıkış 0: istemci `172.31.90.4` / `fd09:0:0:90::4`; vekil kaydı `remote_ip` aynı iki adres; uygulama ağ ad alanındaki paket yakalamada `X-Forwarded-For` aynı iki adres. **Ek:** istemci sahte `X-Forwarded-For: 203.0.113.9` / `2001:db8::9` gönderdiğinde uygulamaya yine yalnız istemcinin kendi adresi ulaştı (sahte değer 0) — `09 §15.1` m8 "vekil başlığı ezer" iddiası yerelde doğru.

**Ayrıca:** `checks.sh secrets` (app 5 / database 2 / backup 2 dosya, `0400`, kendi sahibinde), `bootstrap` (dört rol + kurucu, `\drds` üç ayar + salt okuma, kurucu ağdan `pg_hba` reddi, `trust` 0, günlükte düz parola 0), `isolation` 7 `ok:`, `redirect-hsts` 3 `ok:`; `storage-setup` setup ✓, keys "All checks passed", lifecycle 7 günlük kural FAIL (beklenen, D-40 (g)); `database-job.sh` 0, `pgbackrest check` 0, `info` `status: ok`, `cipher: mixed`, kurucu soketten (`inet_server_addr` boş); uydurma günlük + ACL ile `file-backup-job.sh` 0, `result: success`; `access.log` `-rw-r--r--`; `database-restore-drill.sh` 0 (depo 2, 7 sn), `file-restore-drill.sh` 0 ("no errors were found"). Restore `--locked-mode` 0, build `-warnaserror` 0/0, Unit 102/102 (Consistency 34/34), betik testleri 4 dosya.

**Tur 1'in kapanışı:** bulgu 1 ✓ (gerekçe metinleri `ci-commands.env`, `SETUP.md` §4 ve parametre kaydı, `DEPLOY_RUNBOOK` §K.11, D-01 "F6/T601'de bağlanır, yer tutucu"; `05 §8.3.11` ile uyumlu; bağlama D-41, hedef T601). K1 ✓ (`down.sh all` sonrası MinIO'nun adsız birimi `0c9939ae…` silindi, birim sayısı 81 → 77, `t09-*` kalıntısı 0). K2 ✓ (D-40 (k) → (f)). K3 ✓ (`CI_CD_SETUP.md` §3.4, D-40 (b), T06 raporu). K4 ✓ (§J.5 iki satır). K5 ✓ (altı araç imajı etiket + özetle; etiketler kayıt defterinde aynı özete çözülüyor — `docker buildx imagetools inspect`; `09 §4.3` ağacı ve `CONTEXT.md` §3 `deploy/` satırı).

**Bulgular:** yok. **K-notları (D-42'ye forward):** K1 `docker-user-egress.sh:23` "T09 installs the unit" eskidi (birim T603, D-40 (h)). K2 D-40 (b)'nin yansıtma listesine `05 §2.3.8` ve `09 §21.4` m.3 eksik. K3 §J.5'in iki T09 satırında `Sonuç` "✓", oysa §J.5 m.3 kural yokken provayı başarısız sayar; satırların anları (18:16Z) §K.10'un ikinci turuna değil düzeltme turunun koşusuna ait.

**Yapım raporu karşılaştırması:** kriter 1, 2, 4, 6, 7 uyumlu; yapımın `~` bıraktığı 3 ve 5 validator CI günlüğüyle ✓. Düzeltme turu notları gözlemle uyumlu. Known Limitation 3'ün izi kapalı D-31 (d)'de; açık iz D-40 (f) / `11` T603 kriter 7. PR açıklamasının "`CONTEXT.md` §3 etkilenmedi" satırı düzeltme notuyla ("etkilendi") yan yana duruyor — repo dışı, bulgu değil.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** `SETUP_COMPLETE=true`, `E2E_CMD=NOT_APPLICABLE`; Caddy erişim kaydı `mode 0644`
- **Paketleme / dağıtım:** Yerel referans kurulum betikleri ve `compose.local-reference.yaml` (yalnız yerel)
- **Yeni bağımlılık:** Yerel deneme için `bitnamilegacy/minio`, `docker:dind`, `curlimages/curl:8.21.0`, `amazon/aws-cli`; hiçbiri üretime girmez

## Commit & PR

- **Branch:** `task/T09-ilk-yayin`
- **PR:** #354
- **Post-merge run'lar:** squash `93ba91f` (#354) — CI 41696545511 ve Publish 41696545899 success (F0 gate'inde doğrulandı, 2026-11-03)
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
| 1 | Etiket, paketler, firmanın kovası, VPS, gerçek alan adı ve IPv6, systemd birimi, `journald` ACL'i | D-40 (a)–(j), D-41 | Gerçek sunucu T603; D-41 E2E komutu F6 |
| 2 | Yedi günlük yarım yükleme kuralı MinIO'da tutmuyor | D-40 (g) | Gerçek kovada sınanır |
| 3 | Uygulamanın kendi kaydında istemci adresi | D-31 (d) → T215 | Alanı uygulama yazar |
| 4 | Egress denemesinde ad yolu yalnız yedek işleriyle kanıtlı | — | Throw-away konteyner dış ağın adını çözmez |
| 5 | Doğrulama tur 2 K-notları (eski birim yorumu, D-40 (b) yansıtma listesi, §J.5 sonuç hücresi) | D-42 | Bloke etmez; T603 ya da ilgili belgeye dokunan task |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** son koşu `41681819821` yeşil (`65250d8`).
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Okunan bölümler ve `09 §25`:** PR açıklamasında.
