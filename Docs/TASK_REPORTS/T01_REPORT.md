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
| 5 | Süre kurulum ayarı; dolunca 503; geçersiz ayar açılışı durdurur | ✓ | `Get_WhenDatabaseDoesNotAnswerInTime_Returns503AndWritesOneTimeoutLine` (tek bağlantılık havuz tutulur, ayar 1 sn); `Start_WithInvalidLivenessTimeout_...` (0, -1, 20, 300); birim `DatabaseOptionsValidatorTests` |
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
| Doğrulama durumu | (validator doldurur) |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

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
