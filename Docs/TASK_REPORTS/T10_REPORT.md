# T10 — Veritabanı kullanıcıları

**Faz:** F0 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kurucu betik** (`deploy/images/database/initdb/10-bootstrap.sh`, POSIX `sh`): resmî imajın ilk başlatma düzeninde, boş veri dizininde bir kez koşar. Dört kullanıcıyı (`shopfolio_migrator`, `shopfolio_app`, `shopfolio_reader`, `shopfolio_backup_status`), `shopfolio` veritabanını (UTF-8, ICU `tr-TR`; imajın `POSTGRES_DB` ile boş açtığı veritabanı atılıp yeniden yaratılır), yetkileri, `pg_trgm`'i ve göç kullanıcısının üç rol ayarını kurar; `pg_hba.conf` ve `pg_ident.conf`'u şablonlardan yazar. Parolalar komut satırına, ortam değişkenine ya da kabuk değişkenine girmez: `db_setup_*_password` sır dosyalarından standart girdiyle `psql \password`'e verilir; parola istemcide SCRAM-SHA-256 özetine çevrilir, sunucuya yalnız özet gider (Düzeltme 1).
- **SQL dosyaları** (`deploy/images/database/bootstrap/`): `10-roles.sql`, `20-database.sql`, `30-database-access.sql`, `40-schema.sql`. Kullanıcı ve veritabanı adları değişkendir; entegrasyon testleri aynı dosyaları koşar (betik yalnız değerleri verir).
- **İstemci kimlik doğrulama şablonları** (`bootstrap/pg_hba.conf`, `pg_ident.conf`): `trust` satırı yok; kurucu yalnız yerel soketten `peer` ile bağlanır, ağdan `reject`; yedeğin durum kullanıcısı yalnız `local shopfolio … peer map=backup_status`; üç ağ kullanıcısı `scram-sha-256` ve yalnız `shopfolio` veritabanı.
- **Dockerfile** (`deploy/images/database/Dockerfile`): betik `/docker-entrypoint-initdb.d/`'ye, SQL ve şablonlar `/usr/local/share/shopfolio/bootstrap/`'a kopyalanır (SQL başlatma dizininde durmaz — imaj orada her `.sql`'i değişkensiz koşardı).
- **Testler (Integration):** `Database/BootstrapScript` (betiğin SQL'lerini Npgsql'le koşan yardımcı), `Database/PrivilegeMatrix` (`public` şemasındaki her tablo için beklenen yetki, istisna dışı fazlalıkta düşer), `Database/DatabaseUsersTests` (beş bayrak, `pg_trgm`, sıralama, uygulama/okuyucu/durum/göç kullanıcıları, üç rol ayarı, `PUBLIC` yetkisi), `Database/TemplateDatabaseFixture` (şablon betiğin SQL'leriyle kurulur, göç kullanıcısıyla uygulanır; koleksiyon kopyası veritabanı yetkilerini ve rol ayarlarını aynı dosyalardan alır; roller çalıştırmaya özel adlı).
- **Testler (Unit):** `Architecture/DatabaseImageScan` (+ `ViolationSamples/DatabaseImage/*`: iki temiz örnek ve dokuz ihlal örneği — `trust`, tüm kullanıcılara satır, kurucunun ağdan satırı, durum kullanıcısının her yerden satırı, sabit parola, parola argümanı, kabuk izleme, tehlikeli rol bayrağı, `PUBLIC` yetkisi), `Consistency/DatabaseUsersConsistencyTests` (`09 §10.8`/`§17` ↔ kurucu betik ↔ `DEPLOY_RUNBOOK`).
- **CI Paketleme** (`scripts/ci/package-build.sh` adım 6): ilk kurulum — dört uydurma sır dosyasıyla (kurucu parolası dahil) veritabanı imajı boş veri dizininde başlatılır, kurulum denetimleri koşar (roller, ayarlar, `pg_hba`, kurucunun ağdan reddi, günlükte parola yok); adım 1 sır taramasına `PASSWORD :'ad'` değişken kalıbı muaf tutuldu.
- **Dokümanlar:** `DEPLOY_RUNBOOK §B.2` (ilk kurulumda veritabanı kullanıcıları), `09 §10.8` madde 1 (`peer` seçildi), `09 §17` dosya ağacı ve madde 1, `09 §19.1` madde 4, `DEFERRED_BACKLOG.md` (D-01 ve D-31 notu, D-37), `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`.

## Etkilenen Modüller / Dosyalar

`deploy/images/database/` (`Dockerfile`, `initdb/10-bootstrap.sh`, `bootstrap/*` — 6 yeni dosya), `scripts/ci/package-build.sh`, `tests/Shopfolio.IntegrationTests/Database/*`, `tests/Shopfolio.UnitTests/Architecture/DatabaseImageScan*.cs`, `tests/Shopfolio.UnitTests/ViolationSamples/DatabaseImage/*`, `tests/Shopfolio.UnitTests/Consistency/DatabaseUsersConsistencyTests.cs`, `Docs/DEPLOY_RUNBOOK.md`, `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`. Uygulama kaynağına (`src/`) dokunulmadı.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kurucu betik dört kullanıcıyı kurar; beş bayrak yok | ✓ | `10-roles.sql` (`NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS`); `DatabaseUsersTests` `pg_roles` denetimi; `DatabaseImageScan` tehlikeli rol bayrağı ihlal örneği |
| 2 | `pg_trgm` kurucuyla; veritabanı UTF-8 ve ICU Türkçe | ✓ | `40-schema.sql`, `20-database.sql`; gerçek imajda `UTF8 \| i \| tr-TR` ve `pg_trgm` okundu; `DatabaseUsersTests` |
| 3 | Uygulama kullanıcısı şema değiştiremez; yeni tabloda dört yetki; `pg_ls_archive_statusdir()` | ✓ | `DatabaseUsersTests` (`CREATE/DROP/ALTER TABLE` reddi, dört deyim, işlev çağrısı); `PrivilegeMatrix` |
| 4 | Okuyucu yeni tabloda yalnız `SELECT`; oturum salt okuma | ✓ | `DatabaseUsersTests`; `ALTER ROLE … SET default_transaction_read_only = on` (`10-roles.sql`) |
| 5 | Durum kullanıcısı varsayılan yetki almaz; `pg_hba` genel `trust` yok, satır yalnız onu ve veritabanını anar | ✓ | `DatabaseUsersTests`; `bootstrap/pg_hba.conf`; `DatabaseImageScanTests` (`HbaTrust`, `HbaAllUsers`, `HbaStatusUserEverywhere` ihlal örnekleri kırmızı); gerçek imajda `peer` eşlemesi |
| 6 | Adlar göç kullanıcısının üç rol ayarında; şablon bu ayarla kurulur | ✓ | `30-database-access.sql`; `TemplateDatabaseFixture` (şablon betiğin SQL'leriyle, göç kullanıcısıyla); `DatabaseUsersTests` ayar denetimi |
| 7 | `PUBLIC` yetkisi alınır; matris `public` şemasındaki her tabloyu karşılaştırır | ✓ | `30-database-access.sql`, `40-schema.sql`; `PrivilegeMatrix` (istisna dışı fazlalıkta düşer); göç geçmişi tablosunun kapalılığı T103'ün (plan) — D-37 (a) |
| 8 | Hiçbir bileşen ağdan kurucuyla bağlanmaz | ✓ | `pg_hba.conf` `host … kurucu … reject`; `DatabaseImageScan` kurucu ağdan ihlal örneği (`HbaFounderOverNetwork`); `DatabaseUsersConsistencyTests.ConnectionSettingsOfTheComponents_NameNoUserButTheFourOfTheSetupScript` ve `ComposeFile_ConnectsTheComponentWithItsOwnUser` (hiçbir bileşenin bağlantı kullanıcısı kurucu değil); gerçek imajda kurucunun ağdan bağlantısı reddedildi; CI adım 6 aynı denetimi koşar |

> Kriter 1–8 yerelde kanıtlandı; CI Paketleme adımının ilk kurulum bölümü ilk kez bu PR'ın CI koşusunda bütün olarak koşar (Known Limitations).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 87/87 (Consistency dahil) | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release` | failed 0, skipped 0 |
| Integration | ✓ 62/62 | `SHOPFOLIO_TEST_DATABASE=… dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj -c Release` | PostgreSQL 18 (Docker `postgres:18`), failed 0, skipped 0 |
| Contract | — | — | `CONTRACT_TEST_CMD` NOT_APPLICABLE (T04) |
| Build / format | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format Shopfolio.slnx --verify-no-changes` | 0 uyarı, 0 hata; biçim temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-03, head `5534acd`) |
| Bulgu sayısı | 1 (ORTA, S1) |
| Düzeltme gerekli mi | Evet — bulgu 1 |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — ✗ FAIL (2026-11-03)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un hook klasörü); ana dal son 3 run `41649150934`, `41649150970`, `41630669794` success; `MEMORY.md`'de T10 satırı var.

**Kabul kriterleri (validator kanıtı):** 1–8 ✓. Dalın imajı (`docker build`) boş veri dizinli ilk kurulumda koşturuldu: dört rolde beş bayrak `f`, durum kullanıcısının parolası yok; `shopfolio` `UTF8|i|tr-TR`, sahibi göç kullanıcısı; `pg_trgm` sahibi kurucu; göç kullanıcısının `shopfolio` için üç ayarı kullanıcı adları; `pg_hba_file_rules` dört satır (kurucu `local peer map=founder`, kurucu `host … reject`, durum `local shopfolio … peer`, üç kullanıcı `host shopfolio … scram-sha-256`), `trust` yok; kurucu ağdan iki veritabanına da reddedildi; durum kullanıcısı soketten `shopfolio`'ya bağlandı, `postgres`'e ve ağdan reddedildi; uygulama/okuyucu/göç ağdan bağlandı (okuyucu `default_transaction_read_only=on`), uygulama `postgres`'e ve yerel soketten reddedildi; sunucu günlüğünde dört parola yok; yeniden başlatmada betik koşmadı; sır dosyası eksikken kurulum adını yazıp durdu. Unit 87/87 (Consistency dahil), Integration 62/62 (yerel `postgres:18`), `dotnet build -warnaserror` 0/0; dal CI `41655753293` (head `5534acd`) dokuz işin hepsi success. `12`'de T10'un kaynak birimlerine (`06 §2.8`, `05 §7.1`, `09 §10`, §17) eşlenen task satırı yok; `12_matris_denetle.py --kapsam 1-77` hata 0. `[ci-failure]` atlaması (`08acff8`) yalnız CI'ın kök neden düzeltmesini taşıyor, kaydı sonraki commit'te — kabul edilebilir; `PB_ALLOW_SECRET` yok, repoda sır değeri yok.

**Bulgu 1 (ORTA, S1 Sapma; `09 §25` "Kod hiçbir doküman cümlesiyle çelişmiyor mu" → hayır):** `10-roles.sql` üç parolayı `CREATE ROLE … PASSWORD :'…_password'` ile verir; psql değişkeni istemcide açılır ve sunucuya açık metin parola taşıyan deyim gider (validator denemesi: aynı kalıp `log_statement=all` ile `CREATE ROLE t10probe2 PASSWORD 'plainProbe456';` diye kayda düştü). Bu `05 §6.x.16`, `09 §10.8` madde 1 son cümlesi, karar K-973 (7) ("veritabanı parolası açık metinle bir deyime yazılmaz") ve `DEPLOY_RUNBOOK §B` satır 1 notuyla çelişir; oturumda kaydı kapatmak kuralın yerine geçmez (deyim metni `pg_stat_activity`'de ve ileride açılacak her deyim kaydı/eklentisinde görünür) ve yapım ⚠ kararı (1) çelişkiyi BLOCKED bildirmek ya da uymak yerine "yorum"la geçti. Uyan yol imajda mevcut: `createuser -P` tty yokken parolayı standart girdiden okur ve istemcide SCRAM özeti üretir (validator denemesi: deyim yalnız `PASSWORD 'SCRAM-SHA-256$4096:…'` taşıdı, açık metin kayıtta 0). **Düzeltme:** parolalar sunucuya yalnız istemcide hesaplanmış SCRAM özeti olarak gider (ör. `createuser -P`, ya da rolü parolasız kurup özeti istemcide hesaplayıp yazmak); `DEPLOY_RUNBOOK §B.2` madde 4, `DatabaseImageScan`'in parola kuralı ve `package-build.sh` adım 1'in `PASSWORD :'` muafiyeti buna göre güncellenir; entegrasyon testleri aynı dosyaları koşmayı sürdürür. Etkilenen: `deploy/images/database/bootstrap/10-roles.sql`, `initdb/10-bootstrap.sh`, `Docs/DEPLOY_RUNBOOK.md` §B.2, `tests/Shopfolio.UnitTests/Architecture/DatabaseImageScan.cs`, `scripts/ci/package-build.sh`, `tests/Shopfolio.IntegrationTests/Database/BootstrapScript.cs`.

**Yapım raporu karşılaştırması:** kriter 1–8 değerlendirmesi uyumlu; uyuşmazlık 1 — yapım ⚠ (1)'i uygun yorum saydı, validator `05 §6.x.16` / K-973 (7) sapması sayar.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (uygulama göçü yok; veritabanı kullanıcıları ve rol ayarları kurucu betikte)
- **Config / ortam değişkeni:** Var — veritabanı imajında `POSTGRES_DB` (`shopfolio`, T05'ten) ve `POSTGRES_USER` kurucu betiğin değişkenidir; yeni ayar yok
- **Paketleme / dağıtım:** Var — veritabanı imajına kurucu betik, SQL'ler ve `pg_hba`/`pg_ident` şablonları; CI Paketleme adım 6'ya ilk kurulum sınaması
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T10-veritabani-kullanicilari`
- **Commit:** `03f6252` — T10: Veritabanı kullanıcıları (kurucu betik, testler; devam gerekiyor); sonraki commit'ler: dokümanlar, rapor, status, hafıza
- **PR:** #350
- **CI:** yapım işçisi izlemez (yönetici düzeni); CI'ı yardımcı yürütür
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Resmî `postgres:18` imajı `/docker-entrypoint-initdb.d/*.sh` dosyalarını boş veri dizininde, yerel soketli geçici sunucuya karşı kurucu kullanıcı olarak koşar | Gerçek imajla Docker'da: roller, veritabanı ve `pg_hba` kuruldu; veri dizini doluyken betik koşmadı | ✓ |
| 2 | `psql \password` parolayı dosyadan standart girdiyle okur ve SCRAM özetini istemcide üretir; parola komut satırında ve günlükte yoktur | Gerçek imajda sunucu günlüğü denetlendi, parola yok; betik parolayı komut satırına vermez (`DatabaseImageScan` parola argümanı ihlal örneği) | ✓ |
| 3 | `peer` + `pg_ident` eşlemesi konteynerin `postgres` işletim sistemi kullanıcısını durum kullanıcısına geçirir | Gerçek imajda yerel soketten `shopfolio_backup_status` olarak bağlanıldı; ağdan kurucu reddedildi | ✓ |
| 4 | ICU `tr-TR` sıralaması resmî imajda kullanılabilir | `SELECT datlocprovider, datlocale` → `i`, `tr-TR` | ✓ |
| 5 | Windows'ta yerel CI betiği adım 6 (MSYS yol dönüşümü) | CI'da `datlocprovider` `"char"` birleştirme hatası düzeltildi (`::text`); bölüm Linux konteynerinde uçtan uca geçti | ✓ D-37 (b) çözüldü |
| 6 | Plan tier / kota / API anahtarı / dış hizmet | Bu task'ta yok | Dış varsayım yok (başka) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Göç geçmişi tablosunun uygulamaya ve salt okumaya kapalılık beklentisi matrise yazılmadı | D-37 (a) | Tabloyu kuran ilk göç henüz yok; planda T103 ekler |
| 2 | ~~`package-build.sh` adım 6'nın ilk kurulum bölümü yerelde koşturulamadı~~ ✅ Çözüldü: Linux konteynerinde uçtan uca geçti | D-37 (b) | CI hatası (`datlocprovider::text`) düzeltildi |
| 3 | Betiğin gerçek sunucuda ilk kurulumda koşması; pgBackRest'in yerel soketten `peer` ile bağlanması | D-01 (c), D-31 | Gerçek sunucu T09'da; pgBackRest işi T07'de |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** `41649150934` success, `41649150970` success, `41630669794` success
- **Repo memory (bitiş kapısı 8):** güncellendi
- **⚠ Kararlar (PR açıklamasında numaralı):** (1) [Düzeltme 1 ile değişti] parola sunucuya açık metinle gitmez: kurucu betik `psql \password` ile istemcide SCRAM-SHA-256 özeti üretir, sunucuya yalnız özet gider; oturumda günlük kapatma yoktur (eski karar — psql değişkeniyle açık metin, günlük kapatma — `05 §6.x.16`'ya aykırı bulundu ve kaldırıldı); (2) yedek durum kullanıcısı `peer` + `pg_ident` eşlemesi (`trust` değil); (3) kurucu ağdan `reject`; (4) `POSTGRES_DB`'yi imaj boş yaratır, betik `DROP DATABASE … FORCE` ile atıp ICU `tr-TR` ile yeniden yaratır; (5) test rolleri çalıştırmaya özel adlı, SQL'ler rol adlarını değişken alır; (6) göç geçmişi tablosu matriste şimdilik varsayılan beklenir, kapalılığı T103 ekler.
- **Yerel deneme temizliği:** `t10pg` konteyneri ve `t10-database:dev`, `shopfolio-database:dev` imaj etiketleri adıyla silindi; `prune` kullanılmadı.
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce

## Düzeltme 1 (doğrulama tur 1, bulgu 1)

**Karar:** kod dokümana uyar (`05 §6.x.16`, `09 §10.8` madde 1, `DEPLOY_RUNBOOK §B` satır 1).

- `10-roles.sql` dört kullanıcıyı parolasız yaratır (beş bayrak aynı). Oturumdaki `log_statement = none` vb. kapatmalar kalktı.
- `10-bootstrap.sh` her ağ kullanıcısı için `psql -c "\password <kullanıcı>"` koşar; parola ve tekrarı sır dosyasından standart girdiyle verilir (komut satırı, ortam değişkeni, kabuk değişkeni yok), psql SCRAM-SHA-256 özetini istemcide üretir, sunucuya yalnız özet gider.
- Entegrasyon fixture'ı kullanıcıların parolalarını (`ALTER ROLE … PASSWORD`) kendisi verir; `10-roles.sql` artık parola değişkeni almaz.
- `package-build.sh` adım 6: veritabanı konteyneri `log_statement=all` ile açılır; günlükte `PASSWORD 'SCRAM-SHA-256$…'` tam üç kez, başka biçimde `PASSWORD '` sıfır kez, dört parola değeri günlükte yok; göç, uygulama ve okuyucu kullanıcıları kendi parolasıyla ağdan bağlanır. Adım 1 taramasındaki `PASSWORD :'` muafiyeti kaldırıldı.
- `DEPLOY_RUNBOOK §B.2` madde 1, 4 ve 5 güncellendi; madde 4 ile §B satır 1 çelişkisi bitti. PR açıklaması ⚠ (1) güncellendi.
- Kanıt (gerçek imaj, `log_statement=all`, Linux konteyneri): günlükte açık parola yok, yalnız üç `SCRAM-SHA-256$` özeti; üç kullanıcı kendi parolasıyla bağlandı; adım 6 uçtan uca geçti (Windows'ta `MSYS_NO_PATHCONV=1` ve Windows biçimli yol ile). Adım 1 taraması yerelde temiz.
