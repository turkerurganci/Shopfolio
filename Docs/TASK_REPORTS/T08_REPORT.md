# T08 — Sır düzeni

**Faz:** F0 | **Durum:** ⏳ Devam ediyor — yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Uygulama sırları dosyadan okur** (`src/Shopfolio.Web/Infrastructure/FileSecrets/`): `SecretFiles` `/run/secrets/<ad>` dosyasını kapanış satır sonu olmadan okur (dosya yoksa `null`; yol ayırıcılı ad reddedilir; değer hiçbir yere yazılmaz). `DatabasePasswordPostConfigure` bağlantı dizisinin kullanıcısına göre (`shopfolio_app` → `db_app_password`, `shopfolio_reader` → `db_reader_password`) parolayı dosyadan diziye ekler; dosya bağlı değilse dizi olduğu gibi kalır (geliştirme ve test). Ortam değişkeninden sır okuyan kod yok. Yeni NuGet bağımlılığı yok.
- **Compose** (`deploy/compose.yaml`): `app` hizmetine `Database__ConnectionString=Host=database;Database=shopfolio;Username=shopfolio_app` (sır değil, parola yok); tek seferlik iki komut hizmeti, profil `tools` (`command`: yalnız `db_app_password`; `read-command`: yalnız `db_reader_password`, kullanıcı `shopfolio_reader`); `database` hizmeti kalıcı olarak yalnız iki yedek anahtarını alır.
- **İlk kurulum bindirmesi** (`deploy/compose.install.yaml`, yeni): kurucu kullanıcının ve göç, uygulama, salt okuma parolalarının veritabanına yalnız ilk başlatmada geçmesi (`05 §6.x.6`); `POSTGRES_PASSWORD_FILE` de burada.
- **`DEPLOY_RUNBOOK §B`** dolduruldu: 12 sır dosyası için kaynak, alan bileşen, ALMAMALI, döndürme ve dosya sahibi/izni; **§B.1** konteyner kullanıcıları (1654, 999, 10001), dizin 0700, `install -o -g -m /dev/stdin` ile yazma, ilk kurulumdaki grup istisnası ve doğrulama.
- **Testler** (`tests/Shopfolio.UnitTests/FileSecrets/`, 36 yeni test): sır okuyucusu ve parola ekleme; Compose hizmet–sır eşlemesi `05 §6.x.2` tablosuna eşit; bindirme; sır adları Compose'ta tanımlı; runbook §B ile Compose eşitliği (bileşenler ve döndürülmeyen iki anahtar); ortam değişkeninden sır okuma taraması (+ kasıtlı ihlal örnekleri); `.gitignore` desenleri; `.env.example` sır taşımıyor.
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
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `Database__ConnectionString` (`app`, `command`, `read-command` hizmetlerinde; parolasız)
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
