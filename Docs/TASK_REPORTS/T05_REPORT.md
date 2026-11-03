# T05 — Compose geliştirme ortamı

**Faz:** F0 | **Durum:** ✗ Doğrulama tur 1 FAIL — düzeltme ve plan kararı bekliyor | **Tarih:** 2026-11-03

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
| 2 | Vekil–uygulama, uygulama–veritabanı ayrı iç ağlar; vekil ve yedek veritabanına ulaşamaz | ~ | Yapı `config` ile doğrulandı (`edge`, `app-data` internal, `db-egress`, `backup-egress`); gerçek bağlanamama denemesi (KK2) kurulumda — D-31 (a) |
| 3 | Yalnız 80 ve 443 yayımlanır | ✓ | `config` çıktısında yalnız `proxy` 80/443 tcp; HTTP/3 (443/udp) yayımlanmaz (KK3) |
| 4 | Veritabanı ve yedek çıkışı `DOCKER-USER`'da hedef adresli kuralla sınırlı | ~ | `docker-user-egress.sh` yazıldı (`sh -n` temiz); kuralın etkisi (KK4) kurulumda — D-31 (b) |
| 5 | Veri/dosya/kayıt/sertifika/durum adlandırılmış bölümlerde; vekilde yalnız görsel salt okuma | ✓ | Dokuz bölüm `config` ile; vekil `files-images` (ro), `caddy-data`, `access-logs` bağlar (KK5) |
| 6 | `compose.dev.yaml` bindirme, e-posta sırrı yok, ayrı imaj/sürüm yok | ✓ | Bindirme `config` geçerli; yalnız Mailpit eklenir, sır dosyası boş (KK6; yakalayıcının ayağa kalkışı kurulumda) |
| 7 | Her imaj etiket + özetle sabit; `latest` yok | ✓ | `grep -rniw latest deploy` boş (KK7); özetler Docker Hub `docker-content-digest` ile |
| 8 | HTTP→HTTPS, bir yıllık HSTS, alt alan/ön yükleme yok, uygulama başlık yazmaz | ~ | Caddyfile yazıldı; sözdizimi ve çalışma denemesi (KK8) kurulumda — D-31 (c), (e) |
| 9 | Vekil istemci adresini görür (IPv6 açık) | ~ | `edge` ağında IPv6 açık; IPv4/IPv6 isteğinde aynı adres (KK9) kurulumda — D-31 (d) |

> `~` = yapı olarak karşılandı, sınama kurulumda (T09); FAIL değildir. Yerelde Docker kapalıydı.

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
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-03, `cc554ea`); rapor: yardımcı durum klasöründe `dogrulama-raporu-r1.md`. Kriter 1, 3, 5, 7 ✓; 2, 4, 6, 8, 9 `?` (daemon/sunucu yok); dal CI yeşil; FAIL `09 §25` gereğidir ("hayır" cevabı düzeltilmeden kabul edilmez). VAL-A982 (KRİTİK) `BEKLEMEDE` kalır |
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
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt (erişim 2026-11-03) | Sonuç |
|---|---|---|---|
| 1 | `DOCKER-USER` zinciri Docker'ın kendi kurallarından önce işlenir; `-s` ile `-d` birleşir | `docs.docker.com/engine/network/firewall-iptables/` | ✓ (belge); konteynerden çıkan trafiğin `OUTPUT`'a değil `FORWARD`'a gittiği belgede açık değil → KK4 kurulumda (D-31) |
| 2 | pgBackRest yerel soketle bağlanır | `pgbackrest.org/configuration.html`: `pg1-socket-path`, `pg1-user` | ✓ ima (belge `pg1-host` yokken soketi açıkça yazmaz); soket dizini hiçbir bölüme bağlı değil |
| 3 | Caddy gelen `X-Forwarded-For`'u güvenmez, kendi değerini yazar | `caddyserver.com/docs/caddyfile/directives/reverse_proxy` | ✓ |
| 4 | IPv6 istemci adresi ters vekile korunarak ulaşır | Docker IPv6 belgesi `enable_ipv6` örneğini verir, adres korunumunu yazmaz | ? Doğrulanamadı → KK9 kurulumda (D-31) |
| 5 | Caddy 2.11.7-alpine ve Mailpit v1.33.0 var; özetler | Docker Hub kayıt defteri `docker-content-digest` (Caddy `sha256:e70b164d…`, Mailpit `sha256:9b3a463b…`) | ✓ |
| 6 | Compose dosyası geçerli | `docker compose … config` (v5.1.0, daemon'suz) | ✓ |
| 7 | PG 18 imajında veri dizini `/var/lib/postgresql` altında | Bilgim tarihli; yerelde Docker kapalı | ? (D-31) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | KK2, KK4, KK8, KK9 ve Caddyfile/Compose'un gerçek daemon'da çalıştırılması; `release.env` özetleri yer tutucu; göç bağlantı dizisinde parola `;` içermemeli | D-31 | Docker kapalı, gerçek sunucu yok; T09'da sınanır |
| 2 | `SHOPFOLIO_DOMAIN` ile uygulama alan adı ayarının tek kaynağa bağlanması; sır dosyası adları ve bağlama yolları (T07, T08) | D-32 | Ayar ve sır düzeni ilgili task'larda |
| 3 | Erişim kaydı: alan listesi, maskeleme, istek kimliği, günlük dosya adı, dışa aktarma süresi | D-33 | Kayıt/dışa aktarma task'larının işi |
| 4 | K-987 doğrulamaları yapıldı, D-01 kalemi kapanmadı | D-01 | `SETUP_COMPLETE` T09'da true olur |

D-29 (c) bu PR'da kapandı.

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 41619599572, 41619599581, 41619412883 — hepsi success
- **Repo memory (bitiş kapısı 8):** güncellendi (`.claude/memory/MEMORY.md` Güncel Durum)
- **Diğer:** ⚠ kararlar (12 madde) ve okunan bölümler PR #348 açıklamasında. `SETUP.md`, `00`, `CLAUDE.md` değişmedi. Docker kapalıydı; daemon gereken sınamalar D-31 ile T09'a bağlandı.
