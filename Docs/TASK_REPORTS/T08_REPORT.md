# T08 — Sır düzeni

**Faz:** F0 | **Durum:** ✗ Doğrulama tur 2 FAIL — ikinci iterasyon; proje sahibine eskalasyon (`12 §8.5`) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Uygulama sırları dosyadan okur** (`src/Shopfolio.Web/Infrastructure/FileSecrets/`): `SecretFiles` `/run/secrets/<ad>` dosyasını kapanış satır sonu olmadan okur (dosya yoksa `null`; yol ayırıcılı ad reddedilir; değer hiçbir yere yazılmaz). `SecretFileConfigurationSource` `Database:ConnectionString` ayarının değerini tamamen bağlı bağlantı dizisi dosyasından (`db_app_connection` ya da `db_reader_connection`) alır; dosya bağlı değilse kaynak bir şey katmaz (geliştirme ve test). Ortam değişkeninden sır okuyan kod yok. Yeni NuGet bağımlılığı yok. (Düzeltme 1'den önce parolayı sonradan ekleyen `DatabasePasswordPostConfigure` vardı; kaldırıldı.)
- **Compose** (`deploy/compose.yaml`): bağlantı için ortam satırı yok; `app` ve `command` `db_app_connection`, `read-command` `db_reader_connection`, `migrate` `db_migrator_password` alır; tek seferlik iki komut hizmeti, profil `tools`; `database` hizmeti kalıcı olarak yalnız iki yedek anahtarını alır.
- **İlk kurulum bindirmesi** (`deploy/compose.install.yaml`, yeni): kurucu parola ve veritabanı kullanıcısına (999) ait üç ayrı kopya (`db_setup_migrator_password`, `db_setup_app_password`, `db_setup_reader_password`) veritabanına yalnız ilk başlatmada geçer (`05 §6.x.6`); `POSTGRES_PASSWORD_FILE` de burada.
- **`DEPLOY_RUNBOOK §B`** dolduruldu: 15 sır dosyası için kaynak, alan bileşen, ALMAMALI, döndürme ve dosya sahibi/izni; **§B.1** konteyner kullanıcıları (1654, 999, 10001), dizin 0700, `install -o -g -m /dev/stdin` ile yazma, her dosyayı tek kullanıcının okuması, döndürmede iki dosyanın birlikte yazılması ve doğrulama.
- **Testler** (`tests/Shopfolio.UnitTests/FileSecrets/` ve `Architecture/`): sır okuyucusu ve ayar kaynağı; Compose hizmet–sır eşlemesi `05 §6.x.2` tablosuna eşit; her sır tek konteyner kullanıcısına gider; bindirme; sır adları Compose'ta tanımlı; runbook §B ile Compose eşitliği; ortam değişkeninden sır okuma taraması `Architecture/` altında, kasıtlı ihlal örnekleri `Architecture/ViolationSamples/SecretSources/*.cs` (ayrı dosyalar, derlemeden çıkarılmış); `.gitignore` desenleri; `.env.example` sır taşımıyor.
- Dokümanlar: `.claude/CONTEXT.md`, `09 §4.3` ağacına `compose.install.yaml` satırı, `compose.dev.yaml` başlığı, `.gitignore` açıklaması, `DEFERRED_BACKLOG.md`.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Program.cs`, `src/Shopfolio.Web/Infrastructure/FileSecrets/*` (3 yeni), `deploy/compose.yaml`, `deploy/compose.install.yaml` (yeni), `deploy/compose.dev.yaml`, `tests/Shopfolio.UnitTests/FileSecrets/*` (4 yeni), `Docs/DEPLOY_RUNBOOK.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/09_CODING_GUIDELINES.md` (ağaçta tek satır), `.claude/CONTEXT.md`, `.gitignore` (yorum), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sırlar bileşen başına ayrı dosya, köke açık dizin, Compose sır tanımıyla yalnız ilgili konteynere | ✓ | `compose.yaml` `secrets:` blokları; `SecretLayoutTests.Compose_GivesEachServiceExactlyTheSecretsOfItsTableRow`, `Compose_DefinesEverySecretOnceAsAFileNamedLikeIt`; dizin 0700: runbook §B.1 |
| 2 | Uygulama sırları dosyadan okur; ortam değişkenine konmaz | ✓ | `SecretFiles` + `DatabasePasswordPostConfigure`; yerel deneme A: uygulama imajı, parola yalnız `/run/secrets/db_app_password`'de, `GET /sistem/canlilik` → `200`; B: dosya yokken `503`; kayıtta parola satırı 0 |
| 3 | Her sırrın sahibi onu okuyan konteyner kullanıcısı, yalnız o okur | ✓ (yerelde Linux birimiyle; ilk kurulum istisnası ⚠ 2) | Yerel deneme C: uid 1654 yalnız `db_app_password`'u, 999 yalnız `db_superuser_password`'ü, 10001 yalnız `file_backup_storage_key`'i okudu, 3 kullanıcı x 3 dosya denendi: 3 çift (kendi dosyası) okundu, 6 çift reddedildi. Compose `uid/gid/mode` alanlarını yok sayar (denendi) — izin sunucuda dosya yazılırken verilir (§B.1) |
| 4 | Paylaşılan sır yok; her bileşen ve tek seferlik komut yalnız kendi satırının sırlarını alır | ✓ | `SecretLayoutTests` (tablo satırı satır yazılı), `Compose_GivesNoSecretToTheContinuouslyRunningApplicationBeyondItsRow` |
| 5 | Döndürme `05 §6.x.9` tablosunda; döndürülmeyen yalnız iki yedeğin şifreleme anahtarları | ✓ | Runbook §B "Rotation" sütunu; `SecretLayoutTests.Runbook_RotatesEverySecretExceptTheTwoBackupCipherKeys` |
| 6 | Repo yalnız `.env.example` şablonunu izler; sır değeri repoya ve kayda girmez | ✓ | `.gitignore` desenleri ve `.env.example` taraması testleri; sır bekçisi (pre-commit) commit'te temiz; kayıtta parola yok (A) |

### Doğrulama kontrol listesi (KK)

| KK | Sonuç | Kanıt |
|---|---|---|
| KK1 | ✓ | `docker compose config --services` (profil `tools`): 7 hizmet; sır eşlemesi testle sabit. Veritabanı üç kullanıcının parolasını yalnız `compose.install.yaml`'da alır; `app`'te `db_reader_password` yok |
| KK2 | ✓ | `SecretSourceScanTests.ApplicationSource_ReadsNoSecretFromTheEnvironment` (`src/**/*.cs`; `GetEnvironmentVariable*`, `AddEnvironmentVariables`, `PGPASSWORD`) + 4 ihlal örneği kırmızı |
| KK3 | ✓ (yerel) | Yukarıda 3. kriter |
| KK4 | ✓ | `SecretLayoutTests.Runbook_ListsEverySecretWithTheComponentsThatReceiveIt` |
| KK5 | ✓ | `SecretLayoutTests.Runbook_RotatesEverySecretExceptTheTwoBackupCipherKeys`; sütun metinleri `05 §6.x.9`'dan |
| KK6 | ✓ | Commit sır bekçisi temiz; `SettingsTemplate_CarriesNoSecretKeyAndNoSecretLikeValue` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 69/69 | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release` | 9 Consistency + 60 Unit; 36'sı yeni |
| Lint | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes`; `dotnet build -c Release -warnaserror` | 0 uyarı, 0 hata |
| Integration | CI'da | PostgreSQL hizmeti gerekir; bu task'ta entegrasyon kodu değişmedi | — |
| Contract | — | NOT_APPLICABLE | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 2 (2026-11-03, `848da1f`); tur 1 ✗ FAIL (`dda60d3`) |
| Bulgu sayısı | Tur 2: 3 (S1, DÜŞÜK) — tur 1'in üç bulgusu kapandı, üç yeni `09 §25` bulgusu · Tur 1: 3 (S1, DÜŞÜK) |
| Düzeltme gerekli mi | Evet — aynı PR'da; ikinci iterasyon FAIL olduğu için önce proje sahibine eskalasyon (`12 §8.5`; kod düzeltmesi ya da `09` cümlesinin değişmesi kararı) |
| Bloke-etmeyen not (K) sayısı | Tur 2: 3 (düzeltme turunda kapanır) · Tur 1: 2 |

### Doğrulama tur 2 — validator kanıtları (tam kapsam)

Başlangıç kapıları: working tree temiz; `core.hooksPath` kurulu; ana dal son 3 run (41630669794, 41630669803, 41619599572) success; memory'de T08 satırı var. `12`'de T08'in kaynak birimlerine bağlı VAL satırı yok (`grep -nE 'MK-59([^0-9]|$)|MK-60([^0-9]|$)|05 §6\.x|09 §17'` boş; T08'in `10 KP-n`'i yok) — `Durum` hücresi yazılmadı; `12_matris_denetle.py --kapsam 1-77` hata 0. Dal CI run 41639445142 (head `848da1f`) success — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E, CI Gate yeşil.

| # | Validator sonucu | Validator kanıtı |
|---|---|---|
| 1 | ✓ | `docker compose ... --profile tools config` (taban; taban+dev+install): `app` = `db_app_connection` + dört dış servis/türetme sırrı; `command` = `db_app_connection`; `read-command` = `db_reader_connection`; `migrate` = `db_migrator_password`; `backup` = iki dosya yedeği anahtarı; `database` kalıcı = iki veritabanı yedeği anahtarı, install bindirmesiyle + `db_superuser_password` ve üç `db_setup_*`; `proxy` yok. 15 sır `${SHOPFOLIO_SECRETS_DIR:-/etc/shopfolio/secrets}/<ad>` |
| 2 | ✓ | Kod okundu (`SecretFiles`, `SecretFileConfigurationSource`, `Program.cs`); hiçbir hizmette bağlantı ortam satırı yok. Dalın derlemesi uygulama imajında koşturuldu: `db_app_connection` bağlıyken uygulama açıldı, canlılık `503` (veritabanı yok), kayıtta sır değeri 0 satır; dosya yokken açılış doğrulamada durdu (çıkış 134). Mutasyon: `Program.cs`'e `Environment.GetEnvironmentVariable` → `ApplicationSource_ReadsNoSecretFromTheEnvironment` kırmızı |
| 3 | ✓ | Yerel Docker (29.2.1): bölüme runbook §B.1'in 15 dosyası sahibi/izniyle yazıldı (hepsi 0400); üç imaj kendi kullanıcısıyla okudu — 1654 yalnız kendi 7 dosyasını, 999 yalnız kendi 6 dosyasını, 10001 yalnız kendi 2 dosyasını; iki kullanıcıya açık dosya yok. Mutasyon: install bindirmesine `db_migrator_password` → `Compose_GivesEverySecretToOneContainerUserOnly` ve iki test kırmızı. Compose `uid/gid/mode` uyarısı yeniden görüldü |
| 4 | ✓ | Compose çıktısı `05 §6.x.3`–§6.x.7, §6.x.18, §6.x.19 ile satır satır eşleşti; mutasyon `app`'e `db_reader_connection` → dört test kırmızı |
| 5 | ✓ | Runbook §B "Rotation" `05 §6.x.10`–§6.x.12, §6.x.20 ile uyumlu; mutasyon `file_backup_cipher_key` döndürme hücresi → `Runbook_RotatesEverySecretExceptTheTwoBackupCipherKeys` kırmızı |
| 6 | ✓ | `git ls-files` sır deseninde yalnız `deploy/.env.example`; `git check-ignore` `.env`, `**/secrets/`, `*.secret`, `appsettings.Production.json` tutuyor; dal farkında yalnız uydurma değerler; `BYPASS_LOG.md` farkı boş, commit'lerde `PB_ALLOW_*` yok |

Tur 1 bulguları: (1) ✓ kapandı — her dosya tek kullanıcıya, 0440 yok (Docker'da gösterildi); (2) ✓ kapandı — kod `09 §3.5` madde 2'ye uydu; (3) ✓ kapandı — tarama `Architecture/`, örnekler derlemeden çıkarılmış ayrı dosyalar. K1 kapandı, K2 D-35 (b)'de.

Tur 2 bulguları (`09 §25` "hayır" cevapları; üçü S1 DÜŞÜK): (1) `09 §4.3` madde 2 — zorunlu ayarın geliştirme değeri `compose.dev.yaml`'da ya da test kurulumunda durur; geliştirme yığınında `Database:ConnectionString`'in değeri geliştiricinin elle yazdığı, izlenmeyen yerel `db_app_connection` dosyasında; D-34 (b) bu kurala rağmen "çözüldü" işaretli. (2) `09 §19.2` madde 2 — davranış sınayan birim testi dosya sistemine dokunmaz; `SecretFilesTests` geçici dizine dosya yazıp okuyor (UnitTests'te dosya yazan tek test). (3) `09 §21.4` madde 1 — doküman tablosu ile kod envanterinin eşitlik denetimleri `UnitTests/Consistency/`'de, `Consistency` özelliğiyle ve Lint'te koşar; runbook §B ↔ Compose eşitliği (`Runbook_*`) `FileSecrets/`'ta, özelliksiz, Unit adımında. K-notları (düzeltme turunda): runbook §B.1 tablosu 1 numaralı dosyayı 999'un kalıcı dosyaları arasında sayıyor (satırı "yalnız ilk kurulumda"); bu raporun kabul kriterleri ve test tabloları eski adları (`DatabasePasswordPostConfigure`, `db_app_password`, 69/69, "⚠ 2") taşıyor; `GetEnvironmentVariables` için ihlal örneği yok (desen kapsıyor).

### Doğrulama tur 1 — validator kanıtları

Başlangıç kapıları: working tree temiz; `core.hooksPath` kurulu; ana dal son 3 run (41630669794, 41630669803, 41619599572) success; memory'de T08 satırı var. `12`'de T08'in kaynak birimlerine (`05 MK-59`, `MK-60`, §6.x; `09 §17`) bağlı VAL satırı yok — `Durum` hücresi yazılmadı; `12_matris_denetle.py --kapsam 1-77` hata 0. Dal CI run 41633970318 (head `dda60d3`) success — 9 iş yeşil.

| # | Validator sonucu | Validator kanıtı |
|---|---|---|
| 1 | ✓ | `docker compose ... config` (taban `--profile tools`; taban+dev+install): `app` 5 sır, `command` yalnız `db_app_password`, `read-command` yalnız `db_reader_password`, `migrate` yalnız `db_migrator_password`, `backup` iki dosya yedeği anahtarı, `database` kalıcı olarak iki veritabanı yedeği anahtarı + install bindirmesinde kurucu ve üç kullanıcı parolası, `proxy` hiç — `05 §6.x.2` ile birebir |
| 2 | ✓ | `SecretFiles` + `DatabasePasswordPostConfigure` kodu okundu; `ApplicationSource_ReadsNoSecretFromTheEnvironment` `Program.cs`'e geçici `Environment.GetEnvironmentVariable` eklenince kırmızı (geri alındı) |
| 3 | ✗ | Yerel Docker'da bölüme runbook §B.1'in izinleri yazıldı ve üç imajın kendi kullanıcısıyla (1654, 999, 10001) okundu: 0400 dosyaları yalnız sahibi okudu; ilk kurulum düzenindeki `db_migrator_password` (1654:999 0440) **iki konteyner kullanıcısınca (1654 ve 999) okundu**. `05 §6.x.1` "dosyayı yalnız o okur" ve "bileşen başına ayrı dosyalarda" der; belgeye uygun yol (veritabanına ayrı dosya) varken paylaşılan dosya seçildi; D-35 (c) proje sahibi onayı değildir (`12 §6.1`, §8.4) — bulgu 1 |
| 4 | ✓ | `05 §6.x.2` satırları Compose çıktısıyla tek tek karşılaştırıldı; `SecretLayoutTests` `app`'e `db_reader_password` eklenince iki testte kırmızı (geri alındı) |
| 5 | ✓ | Runbook §B "Rotation" sütunu `05 §6.x.10`–§6.x.12, §6.x.20 ile karşılaştırıldı; döndürülmeyen yalnız iki şifreleme anahtarı |
| 6 | ✓ | `git ls-files` sır deseni yalnız `deploy/.env.example`; `git check-ignore` desenleri tutuyor; dal farkında sır değeri yok (testlerde uydurma değer); `BYPASS_LOG.md` farkı boş |

Bulgular: (1) kriter 3 — ilk kurulumda üç parola dosyası iki konteyner kullanıcısına açık (`05 §6.x.1`); (2) `09 §3.5` madde 2 bağlantı dizisinin sır dosyasında olduğunu söyler, kod dizinin parolasız kısmını Compose ortamından alıyor — çelişki BLOCKED akışı yerine D-35 (a) ile backlog'a yazıldı (`09 §2`, §24 madde 8, §25); (3) yeni kaynak taraması `UnitTests/Architecture/` yerine `FileSecrets/`'ta ve ihlal örnekleri ayrı dosya yerine satır içi (`09 §19.2` madde 3). K-notları: K1 D-32 (b) kodu `Infrastructure/Secrets` diye anıyor (klasör `FileSecrets/`); K2 D-35 (b) `05 §8.2.4` hizmet listesi cümlesi — backlog'da.

## Düzeltme 1 (doğrulama tur 1 FAIL sonrası)

Karar (yardımcı, proje sahibi yetkisiyle 2026-11-03): kod dokümana uyar, doküman değişmez. Bu karar ilk yapım raporundaki ⚠ 2 ve ⚠ 3'ü kaldırır.

| Bulgu | Durum | Ne yapıldı |
|---|---|---|
| 1 (`05 §6.x.1`) | Düzeltildi | İlk kurulumdaki 0440/grup-999 ortak okuma kalktı. Veritabanının kurucu betiği için `db_setup_migrator_password`, `db_setup_app_password`, `db_setup_reader_password` (999'a ait 0400, yalnız `compose.install.yaml`'da); uygulama imajı tüketicileri 1654'e ait 0400 dosyalarını okur. Aynı değer iki dosyada durur, her dosyayı bir kullanıcı okur. Runbook §B/§B.1, `SecretLayoutTests` (yeni `Compose_GivesEverySecretToOneContainerUserOnly`), Compose tanımları güncellendi |
| 2 (`09 §3.5` madde 2) | Düzeltildi | `Database:ConnectionString` tamamen sır dosyasından (`db_app_connection`, `db_reader_connection`); `Database__ConnectionString` ortam satırları kalktı; `DatabasePasswordPostConfigure` yerine `SecretFileConfigurationSource`; `CONTEXT.md`, `compose.dev.yaml` başlığı, D-34 (b) güncellendi |
| 3 (`09 §19.2` madde 3) | Düzeltildi | Kaynak taraması ve testleri `tests/Shopfolio.UnitTests/Architecture/`; kasıtlı ihlaller `Architecture/ViolationSamples/SecretSources/` altında ayrı dosyalarda; `CONTEXT.md` §3 `tests/` satırı |
| K1 | Kapandı | D-32 (b) notunda `Infrastructure/FileSecrets/` |
| K2 | D-35 (b)'de kalır | `05 §8.2.4` cümlesi |

- **Sır adları değişti:** `db_app_password` → `db_app_connection` (bağlantı dizisinin tamamı), `db_reader_password` → `db_reader_connection`; yeni `db_setup_*_password` üç dosya. Toplam 15 sır dosyası.
- **Testler:** Unit 66/66 (eski 69'dan; parola ekleme testleri ayar kaynağı testleriyle değişti), Integration 29/29 (yerel PostgreSQL 18 konteyneri, adıyla silindi). `dotnet build -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` temiz; `docker compose config` (taban+tools, +dev, +dev+install) geçerli; `restore --locked-mode` çıkış 0.
- **KK3 yeniden:** konteynerlerde (alpine, geçici bölüm) 1654 ve 999 kullanıcıları ayrı 0400 dosyalar aldı; her kullanıcı yalnız kendi dosyalarını okudu, çapraz okuma reddedildi (`db_app_connection`/`db_migrator_password` yalnız 1654; `db_setup_*` yalnız 999). Bölüm ve imaj adıyla silindi.
- **D-35:** (a) ve (c) "çözüldü (T08 düzeltme 1)"; (b) ve (d) açık.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok — bağlantı dizisi sır dosyasından gelir (düzeltme 1)
- **Paketleme / dağıtım:** Var — `compose.yaml` (iki komut hizmeti, `database` sırları), `compose.install.yaml` (yeni), runbook §B/§B.1
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T08-sir-duzeni`
- **Commit:** *(PR'da)*
- **PR:** *(PR'da)*
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Compose dosya kaynaklı sırrın sahipliğini ve iznini değiştirmez (`05 §6.x.1`, bilgim tarihli) | Docker Compose v5.1.0, 2026-11-03: `secrets uid, gid and mode are not supported, they will be ignored`; konteynerde dosya `0777 root` (Windows ana makine) | Doğrulandı — izin sunucuda dosya yazılırken verilir; Linux sunucuda dosya sahipliği ana makinedekiyle aynı gelir (T09'da referans sunucuda `ls -ln /run/secrets` ile sınanacak, D-31 değil — runbook §B.1 "Doğrulama") |
| 2 | Veritabanı imajının kullanıcısı `postgres` 999, uygulama imajınınki `app` 1654, yedek imajınınki `backup` 10001 | `docker run --rm --entrypoint id` üç imajda | Doğrulandı |
| 3 | `Npgsql` bağlantı dizisinde `Password` eklenebilir; dizi yazımı değeri hata iletilerine sızdırmaz | `NpgsqlConnectionStringBuilder` testleri; `ValidatorFailure_NeverPrintsThePasswordFromTheFile` | Doğrulandı |
| 4 | Resmî `postgres` imajı `POSTGRES_PASSWORD_FILE`'ı okur; veri dizini doluyken bir daha istemez | Yerel deneme: parolalı dosyayla `postgres:18` açıldı; mevcut dizinde yeniden başlatma bu task'ta denenmedi | Kısmen — dosyayla açılış doğrulandı; "dolu dizinde parolasız başlama" T09'da sınanır (kurucu betik T10'da) |
| 5 | Windows'ta `.gitignore` `secrets/` deseni `Secrets/` klasörünü de yutar | `git status` ilk denemede `Infrastructure/Secrets/` ve `tests/.../Secrets/` göstermedi | Doğrulandı — klasörler `FileSecrets/` olarak adlandırıldı |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Üst dokümanlarda dört uyumsuzluk (`09 §3.5` bağlantı dizisi cümlesi, `05 §8.2.4` hizmet listesi, `05 §6.x.1` ilk kurulum istisnası, döndürme adımlarının ayrıntısı) | D-35 (yeni) | `05`/`09` doküman değişikliği (GUARDRAILS §3) |
| 2 | Dosya/kayıt klasörlerinin yolları, pgBackRest kaydı | D-32 (b) kalanı | T07 |
| 3 | `SETUP_COMPLETE` ve D-01 | D-01 | T09 (T08 ilerleme notu eklendi) |
| 4 | Komut hizmetlerinin bağlandığı komut kipi (`command`/`read-command` imajın giriş noktasını kullanır) | — | Yönetim komutlarını yazan task'lar (`05 §8.5`) |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 41630669794 success, 41630669803 success, 41619599572 success
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Yerel deneme temizliği:** `t08-pg`, `t08-app`, `t08-net`, `t08-secrets`, `shopfolio-t08-app:t` adıyla silindi; `prune` kullanılmadı; başka kaynağa dokunulmadı.
- **Dokümanlar dışı yerel sınır:** `migrate` hizmeti şema sahibinin parolasını kendi sürecinin `PGPASSWORD` ortam değişkenine verir (T05'ten, `compose.yaml`'da gerekçeli; EF demeti parolayı komut satırında kabul etmez) — uygulama kodu değildir, KK2 taraması uygulama kaynağını kapsar.
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce
