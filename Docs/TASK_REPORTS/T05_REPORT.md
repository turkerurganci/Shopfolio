# T05 — Compose geliştirme ortamı

**Faz:** F0 | **Durum:** ✓ PASS (doğrulama tur 2) — merge `5509c5d` (#348) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `deploy/compose.yaml`: dört hizmet (`proxy`, `app`, `database`, `backup`) ve tek seferlik `migrate`; dört ağ, dokuz adlandırılmış bölüm, bileşen başına sır dosyaları (ortam değişkeni yok, yalnız `_FILE` yolu).
- `deploy/compose.dev.yaml`: üstüne bindirilir; yalnız e-posta yakalayıcıyı (Mailpit) ekler; arayüz yalnız `127.0.0.1:8025`.
- `deploy/caddy/Caddyfile`: HTTP→HTTPS, bir yıllık HSTS (alt alan ve ön yükleme yok), `/gorsel/*` salt okuma bölümden, erişim kaydı.
- `deploy/release.env`: bir sürümün üç imaj başvurusu (etiket + özet; özet şimdilik yer tutucu).
- `deploy/firewall/docker-user-egress.sh` (veritabanı ve yedek aracı için `DOCKER-USER` çıkış kuralı) ve `deploy/firewall/installation-checks.sh` (KK2, KK4, KK8, KK9 kurulum denemeleri).
- Üç Dockerfile'a bölüm bağlama dizinleri (sahiplik için); `app` Dockerfile yorumu "tek çalıştırılabilir dosya" (D-29 (c) kapandı).
- `.env.example`'a `SHOPFOLIO_DOMAIN`; `09 §4.3` ağacı ve `CONTEXT.md` §3 güncellendi.
- Backlog: D-01 T05 ilerleme notu (K-987 iki doğrulama, `SETUP_COMPLETE` false kalır), D-29 (c) çözüldü, D-31, D-32, D-33.

## Etkilenen Modüller / Dosyalar

`deploy/**` (yeni), `deploy/.env.example`, `deploy/images/{app,database,backup}/Dockerfile` (dizinler, yorum), `.claude/CONTEXT.md`, `Docs/09_CODING_GUIDELINES.md` (§4.3 ağacı), `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`. Uygulama kodu değişmedi.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `compose.yaml` dört hizmet + tek seferlik göç | ✓ | `docker compose --env-file deploy/release.env --env-file deploy/.env.example -f deploy/compose.yaml config` geçerli, beş hizmet (KK1) |
| 2 | Vekil–uygulama, uygulama–veritabanı ayrı iç ağlar; vekil ve yedek veritabanına ulaşamaz | ✓ | Yapı `config` ile doğrulandı; çalışma zamanı: `installation-checks.sh isolation` yerel Docker'da çıkış 0 — kontrol bağlantısı geçti, vekil ve yedek veritabanına ad ve iki adresle (`172.30.4.2` app-data, `172.30.2.2` db-egress) bağlanamadı (düzeltme turu 1, aşağıda) |
| 3 | Yalnız 80 ve 443 yayımlanır | ✓ | `config` çıktısında yalnız `proxy` 80/443 tcp; HTTP/3 (443/udp) yayımlanmaz (KK3) |
| 4 | Veritabanı ve yedek çıkışını nesne depolamayla sınırlayan hedef adresli `DOCKER-USER` kuralı betikte tanımlı; gerçek deneme T09'da | ✓ (statik) | `deploy/firewall/docker-user-egress.sh` (`sh -n` temiz): iki alt ağ için `DOCKER-USER` başına zincir atlaması; zincir yalnız nesne depolama adres ve kapısına `RETURN`, geri kalanı `REJECT`. Gerçek deneme T09 KK7; DNS riski D-31 (b)'de |
| 5 | Veri/dosya/kayıt/sertifika/durum adlandırılmış bölümlerde; vekilde yalnız görsel salt okuma | ✓ | Dokuz bölüm `config` ile; vekil `files-images` (ro), `caddy-data`, `access-logs` bağlar (KK5) |
| 6 | `compose.dev.yaml` bindirme, e-posta sırrı yok, ayrı imaj/sürüm yok | ✓ | Bindirme `config` geçerli; yalnız Mailpit eklenir, sır dosyası boş; yerel Docker'da `mailpit` `Up (healthy)`, `GET 127.0.0.1:8025/api/v1/info` → 200, `v1.33.0` (düzeltme turu 1) |
| 7 | Her imaj etiket + özetle sabit; `latest` yok | ✓ | `grep -rniw latest deploy` boş (KK7); özetler Docker Hub `docker-content-digest` ile |
| 8 | HTTP→HTTPS, bir yıllık HSTS, alt alan/ön yükleme yok, uygulama başlık yazmaz | ✓ | Yerel Docker'da `installation-checks.sh redirect-hsts` (`CURL_OPTS=-k`, `SITE_DOMAIN=localhost`) çıkış 0: HTTP `308` → `Location: https://localhost/`; HTTPS yanıtında `strict-transport-security: max-age=31536000`; uygulama başlık yazmaz (kodda HSTS yok) (düzeltme turu 1, aşağıda) |
| 9 | Vekil istemci adresini görecek biçimde yapılandırılmış (IPv6 açık ya da userland proxy kapalı); uygulama tarafı T215'te, IPv4/IPv6 denemesi T09'da | ✓ (yapılandırma) | `compose.yaml` satır 168: `edge` ağında `enable_ipv6: true`. Uygulama tarafı T215 kriter 3 (vekilin ilettiği IP ekseni), gerçek deneme T09 KK8; D-31 (d) |

> Kriter 4 ve 9 plan düzeltmesiyle yeniden yazıldı (aşağıda, "Plan düzeltmesi"); gerçek denemeler T09 KK7, KK8 ve T215 KK3'te.

**Validator kanıtı (tur 2, 2026-11-03, `098642a`; iki doğrulayıcıyla — ilki bağlam sınırında bölündü; dal CI'ı run 41627237288 success, 9 pass + Guard skipping):** 1–9 ✓. Üç imaj dalın Dockerfile'larından yerel derlendi (Docker 29.2.1, Compose v5.1.0; bellek hatası yok), göç rolü uydurma parolayla elle kuruldu → `migrate` Exited (0) (`PGPASSWORD` yolu çalıştı), app healthy. KK2: `isolation` çıkış 0 (vekil ve yedek → `database` adı, `172.30.4.2`, `172.30.2.2` reddedildi; pozitif kontroller proxy→app:8080, backup→1.1.1.1:443 geçti). KK4: `docker-user-egress.sh` NET_ADMIN'li atılık konteynerde `apply` iki kez idempotent, `DOCKER-USER` başında iki `-s` atlaması, zincirde yalnız tcp/443 `RETURN` + `REJECT`, `remove` temiz. KK5: vekilde `files-images` salt okuma (`touch` → Read-only). KK6: `mailpit` healthy, `/api/v1/info` v1.33.0, app konteynerinden `mailpit:1025` → `220 … Mailpit ESMTP`. KK8: `redirect-hsts` çıkış 0 — HTTP 308 → https, HTTPS 200 `Strict-Transport-Security: max-age=31536000`, uygulamanın doğrudan yanıtı HSTS'siz; `caddy validate` Valid, `caddy fmt` farksız. KK9: `edge` IPv6 açık, vekil `fd7a:5f0c:9e11:1::4` aldı. Taban `config`: yalnız 80/443; bindirme Mailpit arayüzünü yalnız `127.0.0.1:8025`'e açar. Yerel sapmalar (repoya girmedi): 80. kapı Windows'ta ayrılmış → `18080:80`; backup zamanlayıcısı iş komutu olmadan exit 64 döngüsü → `sleep` (K1, D-34).

**Validator kanıtı (tur 1, 2026-11-03, `cc554ea`; dal CI'ı run 41621640998 success, 9 pass + Guard skipping):** 1 ✓ · 2 ? · 3 ✓ · 4 ? · 5 ✓ · 6 ? · 7 ✓ · 8 ? · 9 ?. `docker compose … config` (v5.1.0, daemon'suz; taban ve bindirmeli): hizmetler `database, migrate, app, proxy, backup` (+ bindirmede `mailpit`); yalnız `proxy` `80/tcp`, `443/tcp`; bölümler dokuz; vekilde dosya alanından yalnız `files-images:ro`; bindirme yalnız `mailpit` ve `SITE_DOMAIN=localhost` ekler. `grep -rniw latest deploy` boş; Caddy ve Mailpit özetleri Docker Hub'dan yeniden okundu, aynı. Caddy 2.11.7 (SHA-512 doğrulanmış ikili) `caddy adapt` çıkış 0: HSTS yalnız `max-age=31536000`, `h1 h2`, yönetim ucu kapalı. `src`/`tests`'te HSTS yok. Betikler `bash -n`/`sh -n` temiz, mantık okundu. Docker daemon kapalı → 2, 4, 6 ("ayakta"), 8, 9'un çalışma zamanı kanıtı üretilemedi (`?`, FAIL değil ama PASS için gerekir). **`~` notu `12 §6.1` ile çelişir:** `~` PASS sayılmaz (K-1521) ve task'a bağlı KRİTİK satır (VAL-A982) BEKLEMEDE ile çıkamaz; D-31 risk kaydı değildir, T09'un kriterleri bu denemeleri içermez — erteleme kabul edilmedi. Kriter 9'un uygulama yanı T215'in kodudur (`09 §15.1` madde 8) → plan düzeltmesi proje sahibine.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | Uygulanamaz | — | Task test beklentisi koymaz |
| Integration | Uygulanamaz | — | Kurulum denemeleri betik (D-31) |
| Contract | Uygulanamaz | — | — |
| Yapı | ✓ | `docker compose … config` (v5.1.0, daemon'suz), `sh -n` betikler, `grep -rniw latest deploy` | 5 hizmet, yalnız 80/443, 9 bölüm; dev bindirmesi geçerli; `latest` yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS — tur 2 (2026-11-03, `098642a`, kapsam tam; iki doğrulayıcıyla, ilki bağlam sınırında bölündü); rapor: yardımcı durum klasöründe `dogrulama-raporu-r2.md`. Kabul kriterleri 9/9 ✓, KK1–KK9 ✓ (KK2, KK4, KK6, KK8 yerel Docker'da), `09 §25` cevapları PR'da; dal CI run 41627237288 yeşil; tur 1 bulgusu ve K1–K7 kapandı; plan düzeltmesi `11`/`12`/D-31'e tutarlı işlendi (`12_matris_denetle.py` Hata: 0). Bulgu 0. `12` matrisinde T05'e bağlı satır yok (VAL-A982 T209'un) |
| Bulgu sayısı (tur 2) | 0 |
| Düzeltme gerekli mi (tur 2) | Hayır |
| Bloke-etmeyen not (K) sayısı (tur 2) | 5 — K1–K4 → D-34: (a) `backup` hizmetinin Compose'da `command`'ı yok, zamanlayıcı exit 64 ile yeniden başlama döngüsüne girer (T07); (b) geliştirme yığınında `Database:ConnectionString` değeri yok, uygulama açılmaz (T07/T08); (c) `installation-checks.sh` başlığı KK4/KK9'u hâlâ T05'e bağlıyor (T09 KK7/KK8); (d) T09'un Dokümanlar satırı ve matris `05:MK-14` T09'u taşımıyor. K5 (bu raporda ve status'ta bayat "Docker kapalıydı" cümleleri, Dış varsayım 7) bu commit'te kapandı |
| Doğrulama durumu (tur 1) | ✗ FAIL (düzeltme turu 1 yapıldı) — tur 1 (2026-11-03, `cc554ea`); rapor: yardımcı durum klasöründe `dogrulama-raporu-r1.md`. Kriter 1, 3, 5, 7 ✓; 2, 4, 6, 8, 9 `?` (daemon/sunucu yok); dal CI yeşil; FAIL `09 §25` gereğidir ("hayır" cevabı düzeltilmeden kabul edilmez). VAL-A982 (KRİTİK) `BEKLEMEDE` kalır |
| Bulgu sayısı | 1 — S1 DÜŞÜK: `09` Kütüphaneler tablosunun Mailpit satırı kanıtsız ("1.28.x ikincil kaynakta"); aracı repoya sokan PR satırı tam sürüm (v1.33.0, 31 Ekim 2026), LICENSE'tan lisans (MIT), bakımcı ve erişim tarihiyle günceller (`09 §3.3` madde 2, 3, 8; §25) |
| Düzeltme gerekli mi | Evet — yeni yapım turu: Mailpit satırı; K1, K3, K4, K7 aynı turda; K2, K5, K6 backlog eki. Daemon'lu kanıt: KK2, KK6, KK8 yerelde (imajlar yerel derlenip `SHOPFOLIO_*_IMAGE` ile; `installation-checks.sh isolation`, `redirect-hsts` `CURL_OPTS=-k`), KK4 Linux Docker motorunda. **Plan kararı (proje sahibi):** KK9 (uygulama yanı T215, IPv6 yanı gerçek sunucu) ve VAL-A982'nin hangi task'ta çevrileceği (`PLAN_CORRECTION_REQUIRED`) |
| Bloke-etmeyen not (K) sayısı | 7 — K1 `compose.yaml:162` olmayan `deploy/firewall/README`'ye atıf, :161 "a test keeps the two equal" (test T215'te); K2 çıkış kuralı DNS'i hesaba katmıyor (gömülü çözücü dış sorguyu konteyner ad alanından iletiyorsa ad çözülemez; bilgim tarihli); K3 göç parolası `efbundle` komut satırında (süreç listesinde görünür; `PGPASSWORD` daha dar); K4 `isolation` denemesi veritabanının `db-egress` adresini denemiyor; K5 `/var/log/pgbackrest` bölümde değil (`05 §9.1.4`) → D-32 (b); K6 Caddyfile tarayıcı tanıtımını da siliyor (`05 §9.1.2`) → D-33; K7 Caddyfile `caddy fmt` biçiminde değil |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — Compose değişkeni `SHOPFOLIO_DOMAIN` (`.env.example`)
- **Paketleme / dağıtım:** Var — `deploy/compose.yaml`, `compose.dev.yaml`, `caddy/Caddyfile`, `release.env`, `firewall/*`
- **Yeni bağımlılık:** Var — Caddy 2.11.7-alpine (MK-08) ve Mailpit v1.33.0 (`09` tablosu), ikisi özetle sabit; ikisi de dokümanda adı geçen ürünler

## Commit & PR

- **Branch:** `task/T05-compose-gelistirme-ortami`
- **Commit:** `3efc774` — T05: Compose geliştirme ortamı (rapor/status/hafıza commit'i ayrı)
- **PR:** #348
- **CI:** yöneticide izlenir
- **Post-merge run'lar:** squash `5509c5d` (#348) — CI 41630669803 ve Publish 41630669794 success (F0 gate'inde doğrulandı, 2026-11-03)

## Dış Varsayımlar

| # | Varsayım | Kanıt (erişim 2026-11-03) | Sonuç |
|---|---|---|---|
| 1 | `DOCKER-USER` zinciri Docker'ın kendi kurallarından önce işlenir; `-s` ile `-d` birleşir | `docs.docker.com/engine/network/firewall-iptables/` | ✓ (belge); konteynerden çıkan trafiğin `OUTPUT`'a değil `FORWARD`'a gittiği belgede açık değil → KK4 kurulumda (D-31) |
| 2 | pgBackRest yerel soketle bağlanır | `pgbackrest.org/configuration.html`: `pg1-socket-path`, `pg1-user` | ✓ ima (belge `pg1-host` yokken soketi açıkça yazmaz); soket dizini hiçbir bölüme bağlı değil |
| 3 | Caddy gelen `X-Forwarded-For`'u güvenmez, kendi değerini yazar | `caddyserver.com/docs/caddyfile/directives/reverse_proxy` | ✓ |
| 4 | IPv6 istemci adresi ters vekile korunarak ulaşır | Docker IPv6 belgesi `enable_ipv6` örneğini verir, adres korunumunu yazmaz | ? Doğrulanamadı → KK9 kurulumda (D-31) |
| 5 | Caddy 2.11.7-alpine ve Mailpit v1.33.0 var; özetler | Docker Hub kayıt defteri `docker-content-digest` (Caddy `sha256:e70b164d…`, Mailpit `sha256:9b3a463b…`) | ✓ |
| 6 | Compose dosyası geçerli | `docker compose … config` (v5.1.0, daemon'suz) | ✓ |
| 7 | PG 18 imajında veri dizini `/var/lib/postgresql` altında | Doğrulama tur 2'de yerel Docker'da: imajın `PGDATA`'sı `/var/lib/postgresql/18/docker` (`db-data` bölümünün altında) | ✓ |

## Düzeltme Turu 1 (2026-11-03)

Doğrulama tur 1 FAIL'ine yanıt. Değişiklikler:

- **Bulgu 1:** `09` Kütüphaneler tablosunda Mailpit satırı restic satırı biçiminde güncellendi: v1.33.0, 31 Ekim 2026 (önceki 22 ve 15 Ekim 2026), bakımcı axllent/mailpit, lisans MIT (GitHub API `repos/axllent/mailpit/license`, LICENSE dosyası), erişim 2026-11-03, imaj özeti.
- **K1:** `compose.yaml` yorumu düzeltildi (olmayan `firewall/README` yerine `installation-checks.sh client-address`; eşitlik testinin T215'te ekleneceği yazıldı).
- **K2:** DNS riski D-31 (b)'ye denenecek madde olarak yazıldı (çözücü adresi sunucuya özgü olduğu için kuralda sabit yazılamaz; Linux motoru yerelde yok).
- **K3:** `migrate` parolayı `PGPASSWORD` ortamıyla verir, komut satırından çıktı. Yerel denemede göç rol yokluğundan düştü (rolleri T10 kurar); sunucu günlüğü `password authentication failed for user shopfolio_migrator` gösterdi, yani parola istemciden gönderildi. Gerçek göçte doğrulama D-31 (g).
- **K4:** `isolation` denemesi artık veritabanının `db-egress` adresini de dener (vekil ve yedekten).
- **K5:** D-32 (b)'ye eklendi. **K6:** D-33'e eklendi. **K7:** `caddy fmt` farkı (global bloktan önceki boş satır) giderildi; `caddy fmt --diff` fark yok, `caddy adapt` çıkış 0.
- **KK4 ve KK9:** yeni kriter metnine göre statik kanıtla kapandı (aşağıda, "Plan düzeltmesi").

**Yerel kanıt (Docker Desktop 29.2.1, WSL 3.8 GiB):** üç imaj T06 Dockerfile'larından yerel derlendi (`docker build -f deploy/images/<ad>/Dockerfile -t shopfolio-<ad>:t05 .`), `SHOPFOLIO_*_IMAGE` ile verildi; bindirmeyle (`compose.dev.yaml`) `SITE_DOMAIN=localhost`; sırlar uydurma değerli geçici dizinde. Bellek hatası olmadı.

- `docker compose … up -d`: `database` healthy, `mailpit` healthy; `migrate` çıkış 1 (roller T10'da kurulur, bu turun işi değil) → `app` başlamadı. Denemeler için `up -d --no-deps proxy backup` kullanıldı.
- Sapmalar (yalnız yerel yerleşim, repoya girmedi): ana makinede 80. kapı Windows'ta ayrılmış, bu yüzden vekil geçici bindirmeyle 18080/18443'e yayımlandı; `backup` imajının zamanlayıcısı iş komutu ister (T07) ve yeniden başlama döngüsüne girdiği için bindirmede `sleep` ile ayakta tutuldu.
- KK2: `sh deploy/firewall/installation-checks.sh isolation` → `ok: control: a container on app-data reaches database:5432`; `ok: proxy cannot reach database:5432 by name`; `… by address 172.30.4.2`; `… 172.30.2.2`; aynı üç satır `backup` için; `exit=0`.
- KK6: `shopfolio-mailpit-1 Up (healthy)`, `127.0.0.1:8025/api/v1/info` → HTTP 200, `"Version":"v1.33.0"`.
- KK8: vekil ağında curl konteynerinden `CURL_OPTS="-k --resolve localhost:80:<vekil> --resolve localhost:443:<vekil>" sh installation-checks.sh redirect-hsts` → `ok: HTTP answers with a redirect`; `ok: redirect points to HTTPS`; `ok: HSTS is exactly: max-age=31536000`; `exit=0`. Ham yanıt: `HTTP/1.1 308 Permanent Redirect … Location: https://localhost/`; `HTTP/2 502 … strict-transport-security: max-age=31536000` (502: uygulama çalışmıyor, başlık yine yazıldı).
- Temizlik: `docker compose down -v`; konteynerler, `shopfolio-*` ağları ve bölümleri, üç yerel imaj, curl/mailpit/caddy imajları silindi.

## Plan düzeltmesi (2026-11-03, `00 §D.8`)

Tur 1 doğrulaması `PLAN_CORRECTION_REQUIRED` verdi (KK4 ve KK9 T05'te karşılanamaz; VAL-A982 kanıtı T209'undur). Proje sahibi seçenek 1'i onayladı; karar = plan güncellemesi. **K numarası yok:** karar kaydı (`PRODUCT_DISCOVERY_STATUS.md`) arşivlidir ve yeni karar kabul etmez (başlık, K-1574; §25); karar bu raporda ve PR #348'de tutulur.

- **Seçenekler:** (1) KK4'ün gerçek denemesi T09'a, KK9'un uygulama tarafı T215'e, IPv4/IPv6 denemesi T09'a taşınır; T05 statik ve yerel kanıtla geçer. (2) Denemeler T05'te kalır, T05 referans sunucu ve T215 kodunu bekler. (3) Kriterler gevşetilir, kanıtsız geçilir. **Seçilen: 1.**
- **Gerekçe:** iki deneme kurulum işidir; T09 zaten referans sunucu ister; uygulama tarafı T215'in kodudur.
- **Değişenler:** `11` T05 kriter 4, 9, test beklentisi, KK4, KK9; T09 kriter 7, 8, KK7, KK8 (+ Dokümanlar, matris satırları); T215 değişmedi (uygulama tarafı zaten kriter 3 + KK3'te). `12` VAL-A982 kaynak sütunundan `11 T05 KK8` atfı çıkarıldı (satır T209'a kalır, `Durum` değişmedi); T05 KK8 yerel kanıtla geçer.
- **Tarama:** `12_matris_denetle.py --kapsam 1-77` Hata: 0; `11_task_blok_denetle.py` yalnız önceden var olan 3 sorun (T330, T601).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | KK4, KK9 ve DNS denemesi (Linux sunucu); `release.env` özetleri yer tutucu; göç hizmetinin `PGPASSWORD`'ü gerçek göçte denenmesi | D-31 | Linux Docker motoru ve gerçek sunucu yok; KK2, KK6, KK8 düzeltme turunda yerelde kanıtlandı |
| 2 | `SHOPFOLIO_DOMAIN` ile uygulama alan adı ayarının tek kaynağa bağlanması; sır dosyası adları ve bağlama yolları (T07, T08) | D-32 | Ayar ve sır düzeni ilgili task'larda |
| 3 | Erişim kaydı: alan listesi, maskeleme, istek kimliği, günlük dosya adı, dışa aktarma süresi | D-33 | Kayıt/dışa aktarma task'larının işi |
| 4 | K-987 doğrulamaları yapıldı, D-01 kalemi kapanmadı | D-01 | `SETUP_COMPLETE` T09'da true olur |
| 5 | `backup` hizmetinin iş komutu, geliştirme yığınında bağlantı dizisi, `installation-checks.sh` başlığı, T09'un `MK-14` izi (doğrulama tur 2, K1–K4) | D-34 | Yedek, ayar/sır ve kurulum task'larının işi |

D-29 (c) bu PR'da kapandı.

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 41619599572, 41619599581, 41619412883 — hepsi success
- **Repo memory (bitiş kapısı 8):** güncellendi (`.claude/memory/MEMORY.md` Güncel Durum)
- **Diğer:** ⚠ kararlar (12 madde) ve okunan bölümler PR #348 açıklamasında. `SETUP.md`, `00`, `CLAUDE.md` değişmedi. İlk yapımda Docker kapalıydı; KK2, KK6, KK8 düzeltme turu 1'de, KK4 (atılık konteyner) doğrulama tur 2'de yerel Docker'da kanıtlandı; sunucu denemeleri T09 KK7, KK8 (D-31).
