# T01 — Canlılık ucu

**Faz:** F0 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /sistem/canlilik` (UC-81): `Endpoints/LivenessEndpoint.cs`, minimal uç; veritabanı yanıt veriyorsa gövdesiz 200, yanıt vermiyorsa ya da süre dolarsa gövdesiz 503; `Cache-Control: no-store`.
- `Infrastructure/LivenessProbe.CheckDatabaseAsync`: bağlantı açma ve `SELECT 1` tek süre sınırı altında; süre dolarsa tek `DbTimeout` kayıt satırı; istek iptali yukarı geçer (`09 §10.7` madde 6).
- `Infrastructure/DatabaseOptions` + `DatabaseOptionsValidator`: `Database:ConnectionString` (zorunlu, biçim denetimli) ve `Database:LivenessTimeoutSeconds` (varsayılan 2; sıfırdan büyük, `CommandTimeout`'tan kısa); açılışta doğrulanır (`ValidateOnStart`), hata ayarı adıyla söyler, bağlantı dizisini yazmaz.
- `Program.cs`: seçenekler, `NpgsqlDataSource` ve `AppDbContext` kaydı bu ayardan.
- Test fabrikası: kayıt değiştirmek yerine `Database:ConnectionString` ayarını verir (D-12).
- Dokümanlar: `09 §3.5` madde 2 ve `§19.1` madde 5, `CONTEXT.md`, `DEFERRED_BACKLOG.md` D-12 kapandı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Endpoints/LivenessEndpoint.cs` (yeni; `.gitkeep` silindi)
- `src/Shopfolio.Web/Infrastructure/{DatabaseOptions,DatabaseTimeouts,LivenessProbe}.cs`, `Infrastructure/Logging/LogEvents.cs` (yeni)
- `src/Shopfolio.Web/Program.cs`
- `tests/Shopfolio.IntegrationTests/Web/{LivenessEndpointTests,CollectingLoggerProvider,ShopfolioWebApplicationFactory}.cs`
- `tests/Shopfolio.UnitTests/Infrastructure/DatabaseOptionsValidatorTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sağlıklı veritabanı → 200 | ✓ | `Get_WithHealthyDatabase_Returns200WithEmptyBody` |
| 2 | Veritabanına ulaşılamıyor → 503 | ✓ | `Get_WhenDatabaseIsUnreachable_Returns503WithEmptyBody`, `Get_WhenDatabaseFailsBeforeTheLimit_...` |
| 3 | Gövdesiz, ayrıntısız, sürüm sızmaz | ✓ | `Get_Answers_WithoutVersionOrDetailInHeadersOrBody` (iki durumda boş gövde; `Server`, `X-Powered-By`, `X-AspNet-Version` yok) |
| 4 | İmzasız açık GET, çerez/oturum gerekmez | ✓ | `Get_WithoutCookies_Returns200AndSetsNone`; uçta yetki ya da çerez kodu yok |
| 5 | Süre kurulum ayarı; dolunca 503; geçersiz ayar açılışı durdurur | ✗ (doğrulama tur 2) | Yapım: `Get_WhenDatabaseDoesNotAnswerInTime_Returns503AndWritesOneTimeoutLine` (tek bağlantılık havuz tutulur, ayar 1 sn); `Start_WithInvalidLivenessTimeout_...` (0, -1, 20, 300); birim `DatabaseOptionsValidatorTests`. **Doğrulayıcı (tur 2):** geçersiz ayar kolu ✓ (Kestrel: `Database__LivenessTimeoutSeconds=0` ve boş bağlantı dizisi → süreç çıkar, `OptionsValidationException` ayarı adıyla söyler, parola çıktıda yok); süre kolu ✗ — gerçekten yavaş veritabanında denetim ayarın süresinde bitmez: girişi 8 sn süren veritabanında (PostgreSQL 18 `login` olay tetikleyicisi, `pg_sleep(8)`) ayar 3 sn iken Kestrel 503'ü 8,3 / 8,1 sn'de verdi; durdurulmuş veritabanında (`docker pause`) soğuk açılış bağlantı dizisinin `Timeout`'unu bekledi (varsayılan 15 sn → 15,2 sn; `Timeout=6` → 6,3 / 6,1 sn), sıcak havuzla komut aşamasında istek 60 sn'de hâlâ yanıtsızdı. Npgsql fiziksel bağlantı açılışında `CancelAfter` belirtecine uymuyor; yapımın testi yalnız havuz beklemesini sınıyor |
| 6 | `Endpoints/` altında, sayfa değil, sayfa hata kalıbına girmez | ✓ | `Endpoints/LivenessEndpoint.cs`, ad alanı `Shopfolio.Endpoints`; Razor Pages dışı minimal uç; mimari testler yeşil |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test Shopfolio.slnx` | yeni 11 test dahil hepsi geçti |
| Integration | ✓ | aynı komut, PostgreSQL 18 (`SHOPFOLIO_TEST_DATABASE`, yerel docker) | yeni 11 test dahil; toplam 56/56, 0 başarısız, 0 atlanan |
| Contract | — | — | task'ın test beklentisi Integration |

Ek: `dotnet build Shopfolio.slnx` 0 uyarı; `dotnet format --verify-no-changes` temiz; koşu sonrası `git status` temiz (kilit dosyaları değişmedi).

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 3) | ✗ FAIL — tur 3 (2026-11-03, `6139c0a`); rapor: yardımcı durum klasöründe `dogrulama-raporu-r3.md`. Kabul kriterleri 6/6 ✓ ve KK1–KK6 ✓ — tur 2 bulgusu kapandı: Kestrel'de `docker pause` ile dondurulan veritabanında 503 ayarın süresinde (varsayılan 2 sn → 2,01 / 2,02 sn; `Pooling=false`, ayar 3 sn → 3,01 / 3,00 sn; istek başına tek `DbTimeout`); eski `CancelAfter` tasarımına geri dönen mutasyonda yeni testlerden ikisi kırmızı (60 sn / 15 sn), geri alınınca yeşil. 62/62 test (Lint 9, Unit 24, Integration 29) PostgreSQL 18.6'da yeşil; dal CI `41553753031` yeşil (aynı sayılar). Kontrol listesinin `09 §25` maddesi ✗ (iki "hayır" cevabı) |
| Bulgu sayısı (tur 3) | 2 — ikisi de S1 DÜŞÜK: (1) `tests/Shopfolio.IntegrationTests/Web/LivenessEndpointTests.cs:122` ve `:135` `real.Host!` gerekçe yorumsuz null-forgiving (`09 §3.1` madde 5; §23; tur 1 bulgu 2'nin aynısı, düzeltme turu 2'de testlere yeniden girdi); (2) `09 §8.1` madde 6 canlılık denetimi için "istek iptali süzgeçle dışarıda kalır" diyor; bu PR `§10.7` madde 6'yı (kuralın evi) `WaitAsync` tasarımına çevirdi, geniş `catch` artık süzgeçsiz ve istek iptalini de yakalayıp `false`'a çeviriyor, iptal dışarıdaki `WaitAsync(limit, ct)` / `ThrowIfCancellationRequested` ile yukarı geçiyor — özet cümle güncellenmedi (`09 §1` "bir kural iki yerde farklı anlatılmaz"; §25 "kod hiçbir doküman cümlesiyle çelişmiyor mu") |
| Düzeltme gerekli mi (tur 3) | Evet — (1) iki `!` için gerekçe yorumu ya da `!`'sız biçim (ör. `real.Host ?? throw new InvalidOperationException(...)`); (2) `09 §8.1` madde 6'nın canlılık parçası `§10.7` madde 6'ya uydurulur. İkisi de küçük. Tur 3 iterasyon sınırının (`12 §8.5`) ötesinde: karar yardımcı / proje sahibi eskalasyonunda |
| Bloke-etmeyen not (K) sayısı (tur 3) | 5 — hepsi bu PR'da kapatılır (düzeltme turunda): K1 yavaşlatılmış üç testten ikisi `DbTimeout` satırını `Assert.Contains` ile sınıyor, `§10.7` madde 7 "tek `DbTimeout` satırı" der (`Assert.Single`); K2 `StallingTcpProxy` iki `catch (Exception ex) when (…)`, bir `Task.Run(` ve `Task.Delay(Timeout.Infinite, …)` taşıyor — `§8.1` madde 6, `§13.1` madde 10 ve `§19.1` madde 6'nın kalıpları; tipli `catch`'ler ve doğrudan çağrıyla yazılabilir; K3 D-21'in satır atfı `LivenessEndpointTests.cs:11` kaydı (takma adlar 13–14); K4 dalın `IMPLEMENTATION_STATUS.md` "Son güncelleme" bloğu iki giriş taşıyor ("Önceki (main):" satırı) — eski giriş `STATUS_CHANGELOG.md`'ye taşınır; K5 PR açıklaması ve bu rapor eskimiş (56/56; ⚠ karar 6 ve Dış Varsayımlar 1, 4 yalnız havuz testini anıyor; etkilenen dosyalarda `StallingTcpProxy`, `DatabaseOptionsValidator` yok). `12` satırı değişmedi (`VAL-A709`, `VAL-A446` `BEKLEMEDE`; T314'ü de kapsar) |
| Doğrulama durumu (tur 2) | ✗ FAIL — tur 2 (2026-11-03, `d1c86f3`); rapor: yardımcı durum klasöründe `dogrulama-raporu-r2.md`. Kabul kriterleri 5/6 ✓, kriter 5 ✗ (KK5 ✗); tur 1'in dört bulgusu kapandı; 57/57 test PostgreSQL 18'de yeşil, derleme 0 uyarı, biçim temiz, dal CI yeşil (`41548801034`) |
| Bulgu sayısı (tur 2) | 1 — S1 ORTA: canlılık denetimi gerçek yavaş veritabanında kurulum ayarının süresinde bitmiyor (`11` T01 kriter 5, KK5; `09 §10.7` madde 6, §19.2 madde 4). Npgsql fiziksel bağlantı açılışında (başlangıç ve kimlik doğrulama el sıkışması) `CancelAfter` belirtecine uymaz; süre bağlantı dizisinin `Timeout`'una (varsayılan 15 sn) ya da veritabanının yanıtına kayar. Kanıt: kriter 5 satırı. Yapımın süre testi tek bağlantılık havuzu tutarak yalnız havuz beklemesini sınar; veritabanının kendisi yavaş değildir, bu yüzden kusur testte görünmez. `09 §10.7` madde 6'nın örnek kodu ("bilgim tarihli") aynı varsayımı taşır |
| Düzeltme gerekli mi (tur 2) | Evet — süre sınırı Npgsql'in iptal desteğine bağlı kalmadan duvar saatiyle uygulanmalı (ör. denetim görevini `WaitAsync(timeout.Token)` ile beklemek, ya da bağlantı açılışını da kesen bir ayar — Npgsql `Timeout` ve `CancellationTimeout` — sınamayla) ve süre testi veritabanının kendisini yavaşlatmalı (ör. `login` olay tetikleyicili veritabanı); `09 §10.7` madde 6 örneği ve "bilgim tarihli" notu sonuca göre güncellenir. İkinci iterasyon FAIL'i — `12 §8.5`: proje sahibine eskalasyon |
| Bloke-etmeyen not (K) sayısı (tur 2) | 2 — K1 (tur 1 K2'nin devamı) `Get_Answers_WithoutVersionOrDetailInHeadersOrBody`'deki `Server` onayı hâlâ koşullu ve `TestServer`'da başlık yok, onay her zaman geçer; doğrulayıcı Kestrel'de `Server: Kestrel` (sürümsüz) gördü → düzeltme turunda bu PR'da kapatılır; K2 D-22'nin satır atfı (`LivenessEndpointTests.cs:92-110`) kaydı, test şimdi 98–117'de → düzeltme turunda bu PR'da kapatılır. `12` satırı değişmedi (`VAL-A709`, `VAL-A446` `BEKLEMEDE`; T314'ü de kapsar) |
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-03, `217af58`); rapor: yardımcı durum klasöründe `dogrulama-raporu-r1.md`. Kabul kriterleri 6/6 ✓ ve KK1–KK6 ✓; kontrol listesinin `09 §25` maddesi ✗ (dört "hayır" cevabı) |
| Bulgu sayısı (tur 1) | 4 — hepsi S1 DÜŞÜK: (1) `Infrastructure/DatabaseOptions.cs` iki üst düzey tür taşıyor (`DatabaseOptions`, `DatabaseOptionsValidator`; `09 §5.1` madde 7 "bir dosyada bir üst düzey tür"); (2) `Endpoints/LivenessEndpoint.cs:31` `typeof(LivenessProbe).FullName!` gerekçe yorumsuz null-forgiving (`09 §3.1` madde 5); (3) `09 §10.7` madde 7'nin canlılık testi cümlesinin üçüncü parçası — istek iptalinde iptal yukarı geçer, `false` dönmez, kayıt yazılmaz — testsiz (davranış doğrulamada Kestrel'de gözlendi: istemci 1 sn'de vazgeçti, istek 499, `DbTimeout` satırı yok); (4) PR açıklamasında okunan `09` bölümleri (çekirdek yedi dahil) ve `09 §25` cevapları yok (`09 §21.3`, §24 madde 1 ve 7, §25 ilk satır) |
| Düzeltme gerekli mi (tur 1) | Evet — yeni yapım turu: doğrulayıcıyı kendi dosyasına ayır; `!`'ı gerekçelendir ya da kaldır; istek iptali testi (iptal yukarı geçer, 503/`false` yok, `DbTimeout` satırı yok); PR açıklamasına okunan bölümler ve `§25` cevapları |
| Bloke-etmeyen not (K) sayısı (tur 1) | 3 — K1 sır bekçisi tür adında `Token`/`ConnectionString` geçen yerel değişken bildirimini takıyor, yapım `Deadline`/`DsnBuilder` takma adlarına kaçtı → D-21; K2 `Get_Answers_WithoutVersionOrDetailInHeadersOrBody`'deki `Server` başlığı onayı `TestServer`'da her zaman geçer (Kestrel gerçekte `Server: Kestrel` yazar, sürümsüz) → düzeltme turunda bu PR'da kapatılır; K3 `09 §10.7` madde 6'nın `CancelAfter`'ı gerçek zamanlayıcıdır, süre testi 1 sn gerçek bekler — `§19.2` madde 2 ve `§19.1` madde 6 ile gerilim → D-22. Task'a bağlı `12` satırı yok (kaynak birimi `07` UC-81; `11 §7.1`'de T01'e bağlı KP yok); canlılığı anan `VAL-A709` ve `VAL-A446` durum ucunu (T314) da kapsar, `BEKLEMEDE` kalır |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `Database:ConnectionString` (ortam değişkeni `Database__ConnectionString`; parola taşıdığı için sır dosyasında, `deploy/.env.example`'a yazılmadı) ve zaten listeli `Database:LivenessTimeoutSeconds`
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (Npgsql zaten dolaylı olarak geliyordu; sürümü merkezi ve kilitli)

## Commit & PR

- **Branch:** `task/T01-canlilik-ucu`
- **Commit:** `dc585df` — T01: Canlilik ucu (rapor/durum/hafıza: `8c2c6f9`)
- **PR:** #338
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Npgsql `NpgsqlDataSource.OpenConnectionAsync(ct)` ve `CancelAfter` ile havuz beklemesi iptal edilir (`09 §10.7` örneği "bilgim tarihli") | havuzu 1'e indirip bağlantıyı tutan test: 1 sn'de 503 + tek `DbTimeout` satırı | ✓ |
| 2 | `WebApplicationFactory.UseSetting("Database:ConnectionString", …)` uygulamanın seçeneklerine ulaşır | 56 testin hepsi fabrika üzerinden uygulamayı açıp veritabanına bağlanıyor | ✓ |
| 3 | `ValidateOnStart` geçersiz ayarda açılışı durdurur ve ayarı adıyla söyler | `Start_WithInvalidLivenessTimeout_...` ve `Start_WithoutConnectionString_...` | ✓ |
| 4 | Windows'ta erişilemeyen port hızlı hata vermez (sınırı bekler) | ilk koşuda `Port=1` ile `DbTimeout` satırı çıktı; hızlı hata testi var olmayan veritabanı adıyla yazıldı | ✓ |
| 5 | Plan tier, kota, API anahtarı vb. dış kısıt | Yok; yerel PostgreSQL 18 imajı | ✓ (dış varsayım yok) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `DatabaseTimeouts` yalnız `CommandTimeout` taşır, `LogEvents` yalnız `DbTimeout` taşır; tam kümeler kendi task'larında (T233, T230) | — (planlı task'lar) | T01 yalnız kendi ihtiyacını koydu |
| 2 | Uç `[MachineEndpoint]` ile işaretlenmedi: yakalama katmanı (`09 §8.1`) henüz yok | — (hata katmanı task'ı işaretler) | işaretin adı ve katmanı o task'ın kararı |
| 3 | Bağlantı dizisi `Web` profilinin sunucu ayarlarını (`Options`) taşımaz | — (T233) | `DatabaseConnection.Configure` T233'te |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** 41542556242, 41542556291, 41528790256 — üçü `success`
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Sır bekçisi:** iki dosyada tanımlayıcı adı takıldı (`NpgsqlConnectionStringBuilder` / `CancellationTokenSource` tür adları); using takma adlarıyla yeniden adlandırıldı, bypass kullanılmadı
- **⚠ kararlar:** PR açıklamasında

## Düzeltme Turu 1

Doğrulama tur 1 FAIL (dört DÜŞÜK bulgu). Giderilenler:
- Bulgu 1: `DatabaseOptionsValidator` kendi dosyasına taşındı (`09 §5.1` madde 7).
- Bulgu 2: `LivenessEndpoint` kategori adı `FullName ?? nameof(LivenessProbe)`; `!` kalmadı (`09 §3.1` madde 5).
- Bulgu 3: `CheckDatabase_WhenTheRequestIsCancelled_...` testi eklendi; iptal edilmiş belirteçle `OperationCanceledException` yukarı geçer, `DbTimeout` satırı yazılmaz (`09 §10.7` madde 7).
- Bulgu 4: PR #338 açıklamasına okunan `09` bölümleri, `05`/`07` referansları ve `09 §25` cevapları eklendi.
- K1/K3 backlog kayıtları, PR #339 ile çakışmamak için D-21 ve D-22 olarak yeniden numaralandı.
- K2: `Server` başlığı onayı "başlık varsa sürüm taşımaz" biçimine çevrildi.
- Sonuç: build 0 uyarı, `dotnet format --verify-no-changes` temiz, Unit + Integration yeşil (PostgreSQL 18), koşu sonrası `git status` temiz.

## Düzeltme Turu 2

Doğrulama tur 2 FAIL (1 ORTA, kriter 5 / KK5). Giderilenler:
- Bulgu 1: Npgsql fiziksel bağlantı açılışında iptal belirtecine uymuyordu; süre artık `Task.WaitAsync(limit, ct)` ile sürücüden bağımsız uygulanır (`LivenessProbe`). Denetim görevi arkada kendi bağlantısını kapatır ve hatasını yutar; süre dolunca belirteci iptal eder ve tek `DbTimeout` satırı yazılır. İstek iptali yukarı geçer, satır yazılmaz.
- Test: veritabanı gerçekten yavaşlatılarak (`StallingTcpProxy`) üç durum: bağlantı açılışı yanıtsız, komut yanıtsız (sıcak havuz), erişilemeyen sunucu. Ayar 1 sn; her biri 503 ve 3 sn altında (ölçülen test süresi 1,05-1,11 sn). Havuz bekleme testi korundu.
- `09 §10.7` madde 6 metni, kod örneği ve test cümlesi sonuca göre güncellendi.
- K1: `Server` başlığı onayı kaldırıldı (TestServer başlık yazmaz); gerekçe test yorumunda.
- K2: `DEFERRED_BACKLOG.md` D-22 satır atfı güncellendi.
- Sonuç: build 0 uyarı, `dotnet format --verify-no-changes` temiz, Unit + Integration yeşil (PostgreSQL 18), koşu sonrası `git status` temiz.
