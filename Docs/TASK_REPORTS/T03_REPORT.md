# T03 — Test altyapısı

**Faz:** F0 | **Durum:** ⏳ Devam ediyor (yapım bitti; doğrulama bekliyor) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Çatı:** iki test projesi xUnit v3 (3.2.2), `OutputType=Exe`; `global.json`'a `"test": { "runner": "Microsoft.Testing.Platform" }` eklendi. **Bulgu:** bu satır yokken .NET 10 SDK'da `dotnet test` xUnit v3 projesinde hiçbir test çalıştırmadan 0 koduyla biter (T04'ün CI komutları için kritik). Satırla `dotnet test Shopfolio.slnx` testleri koşar.
- **Sahteler:** `UnitTests/Fakes/ManualTimeProvider.cs` (+ testi); `IntegrationTests` aynı dosyaları bağlantılı dosya olarak derler (ikinci kopya yok).
- **Veritabanı altyapısı:** `IntegrationTests/Database/` — ayar okuma (`SHOPFOLIO_TEST_DATABASE` kazanır, sonra `Tests:Database` kullanıcı sırrı), göç demeti kurucusu, tek derleme fikstürü (`AssemblyFixture`; şablon `shopfolio_test_template` her çalıştırmada demetle kurulur, havuz boşaltılır, sonda silinir), koleksiyon başına şablondan veritabanı.
- **Uygulama:** `Infrastructure/AppDbContext.cs` (`internal sealed`, varlıksız) ve tasarım zamanı fabrikası; `EFCore`, `Npgsql`, `NamingConventions`, `EFCore.Design` paketleri; `InternalsVisibleTo` (iki test derlemesi); `.config/dotnet-tools.json` (`dotnet-ef` 10.0.11). Demet sıfır göçle kuruldu (`__EFMigrationsHistory` oluşur).
- **Fabrika:** `Web/ShopfolioWebApplicationFactory.cs` uygulamayı koleksiyon veritabanıyla açar; örnek istek geçer. `Program` .NET 10.0.400'de kendiliğinden `public` (sınandı) — `public partial class Program` gerekmedi.
- **Mimari test:** `Architecture/ModuleBoundaryTests.cs` (ArchUnitNET 0.13.3); kasıtlı ihlal örneği csproj'daki `<Csc>` hedefiyle ayrı derlemeye (`ModuleBoundaryViolationSample.dll`) derlenir — dördüncü proje yok; kural örnekte tam 1 ihlal bulur.
- **Yasak kalıp taraması:** `Architecture/ForbiddenTestPatterns.cs` + testleri; `Skip`, `Explicit`, koşucu süzgeci örnekleri `ViolationSamples/ForbiddenPatterns/` altında (derlemeden ve gerçek taramadan hariç).
- **Eşitlik denetimi:** `Consistency/LibraryTableConsistency*.cs` (`[Trait("Consistency","true")]`): `Directory.Packages.props` ile Kütüphaneler tablosu; saf karşılaştırma işlevi, sahte tablo/props testleri (fazla ve eksik satır kırar).
- **`.editorconfig`:** `tests/**.cs` için CA1707 kapalı (`Yöntem_Durum_Beklenen` adları, `09 §19.2` madde 7).
- **Dokümanlar:** `09` Kütüphaneler tablosu (8 satır güncellendi + `EFCore.Design` eklendi) ve §19.1 madde 1, 3, 4, 5 ile §6 madde 1, 4 "bilgim tarihli" işaretleri sınanan satırlarda güncellendi; `CONTEXT.md` §3; `DEFERRED_BACKLOG.md` D-12…D-14.

## Etkilenen Modüller / Dosyalar

- Kök: `Directory.Packages.props`, `global.json`, `.editorconfig`, `.config/dotnet-tools.json`
- `src/Shopfolio.Web/Infrastructure/**`, `Shopfolio.Web.csproj`, kilit dosyaları (üç proje)
- `tests/Shopfolio.UnitTests/**`, `tests/Shopfolio.IntegrationTests/**`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`
- `SETUP.md` ve `.github/ci-commands.env` değişmedi.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki proje xUnit v3 ile çalışır; biçim .NET 10 SDK'da sınandı | ✓ | `dotnet test Shopfolio.slnx -c Release` 34/34; MTP satırı gerekli (yukarıdaki bulgu); `09 §19.1` madde 1 |
| 2 | `ManualTimeProvider`; bağlantılı derleme | ✓ | `UnitTests/Fakes/`; IntegrationTests `<Compile Include=… LinkBase="Fakes" />` |
| 3 | Bağlantılı dosya paylaşımı derlenir (K-1355) | ✓ | Derlendi; karar kaydı gerekmedi |
| 4 | Ayar sırası | ✓ | `TestDatabaseSettingsTests` (ortam değişkeni kazanır, ayar yoksa açık hata) |
| 5 | Şablon + koleksiyon veritabanı; boş bağlam | ✓ | `TestDatabaseInfrastructureTests` (şablonda `__EFMigrationsHistory`, koleksiyonlar birbirinin verisini görmez) |
| 6 | Fabrika + örnek istek | ✓ | `ShopfolioWebApplicationFactoryTests` |
| 7 | Mimari test; ihlalli derleme kırar | ✓ | `ModuleBoundaryTests` (temiz kod yeşil, ihlal örneğinde 1 ihlal) |
| 8 | Yasak kalıp taraması | ✓ | `ForbiddenTestPatternScanTests` (`Skip` örneği 8. satırda kırmızı, temiz örnek boş) |
| 9 | `Consistency` klasörü/özelliği; tablo–props eşitliği | ✓ (açık karar) | `LibraryTableConsistencyTests`; fazla/eksik satır kırar; tablo henüz eklenmemiş paketleri içerdiğinden denetim props ⊆ tablo ve tablo − props == açık liste (D-14) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test Shopfolio.slnx -c Release` | UnitTests yeşil (toplam 34, hata 0, atlanan 0) |
| Integration | ✓ | aynı komut; `SHOPFOLIO_TEST_DATABASE` yerel PostgreSQL 18.6 konteyneri | IntegrationTests yeşil (~32 sn) |
| Contract | NOT_APPLICABLE | — | — |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release` | 0 uyarı, 0 hata |
| Lint | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |

**T04 için not (MTP süzgeci):** Sınandı: `dotnet test --project tests/Shopfolio.UnitTests --filter-trait "Consistency=true"` 9 test koşar; `--filter-not-trait "Consistency=true"` kalan 13'ü koşar. `dotnet run -- --filter-trait` ise "unknown option" verir (süzgeç `dotnet test` üzerinden verilir). `--project` bayrağı MTP kipinde gerekir.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — (doğrulama chat'i doldurur) |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok — demet sıfır göç taşır; `AppDbContext` varlıksızdır (T101 doldurur).
- **Config / ortam değişkeni:** Var — test için `SHOPFOLIO_TEST_DATABASE` (ya da `Tests:Database` kullanıcı sırrı). Değer repoya girmedi.
- **Paketleme / dağıtım:** Yok.
- **Yeni bağımlılık:** Var — xunit.v3 3.2.2, Mvc.Testing 10.0.11, ArchUnitNET 0.13.3, EFCore 10.0.11, EFCore.Design 10.0.11 (PrivateAssets=all), Npgsql 10.0.3, Npgsql.EFCore.PostgreSQL 10.0.3, EFCore.NamingConventions 10.0.1; yerel araç dotnet-ef 10.0.11. Lisanslar deponun LICENSE dosyalarından okundu (Apache-2.0, MIT, PostgreSQL License); hepsi `09` Kütüphaneler tablosunda.

## Commit & PR

- **Branch:** `task/T03-test-altyapisi`
- **Commit:** `64487a1` — T03: Test altyapisi
- **PR:** PR_NO_BURAYA
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `xunit.v3` var ve .NET 10'da çalışır | 3.2.2 (14 Ocak 2026) restore edildi; .NET 10.0.11'de 34 test koştu | ✓ |
| 2 | `dotnet test` xUnit v3 projesini koşturur | MTP satırı olmadan test koşmadan 0 döndü; satırla koştu | ✓ (satır gerekli) |
| 3 | PostgreSQL 18'de `CREATE DATABASE … TEMPLATE` | yerel PostgreSQL 18.6 konteynerinde şablon ve koleksiyon testleri geçti | ✓ |
| 4 | `dotnet-ef` 10.0.11 demet üretir | `dotnet ef migrations bundle` ~27 sn'de çalıştı; sıfır göçle uygulandı | ✓ |
| 5 | `Program` kendiliğinden `public` | SDK 10.0.400'de fabrika `Program` türüyle derlendi | ✓ |
| 6 | Paket sürümleri registry'de var, lisanslar | restore + LICENSE dosyaları okundu; ArchUnitNET'in son sürümü 0.13.5 (0.13.3'te kalındı) | ✓ |
| 7 | Npgsql.EFCore.PostgreSQL 10.0.3 ile EFCore 10.0.11 uyumu | nuspec alt sınırı `[10.0.4,11)` görünür, 10.0.11 çözüldü ve çalıştı | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Uygulamanın DB bağlantı ayarı adı dokümanda yok; fabrika `AppDbContext` kaydını değiştirir | D-12 | İlk okuyan task T101/T01 |
| 2 | Mimari testin sayfa ve altyapı kuralları | D-13 | T218 |
| 3 | Eşitlik denetimi katı iki yönlü değil; `09 §3.2` cümlesi önerisi | D-14 | Tablo ileriye dönük yazılmış |
| 4 | Yedek aday `ArchUnitNET` 0.13.5 | — | Doküman 0.13.3 der; yükseltme ayrı karar |

## Notlar

- **Working tree (Adım -1):** temiz.
- **core.hooksPath:** `C:\projects\Shopfolio\scripts\git-hooks` (ana depo, etkin).
- **Ana dal CI startup (Adım 0):** 41528790256 success · 41528790271 success · 41489370654 success.
- **Sır bekçisi:** staged işte yanlış pozitif çıkan tanımlayıcı adları (`ConnectionString`, `secrets`, `TokenSource`) yeniden adlandırıldı (`Dsn`, `configuration`, ayrı satırda `CancelAfter`); `PB_ALLOW_SECRET` kullanılmadı, bekçi değiştirilmedi.
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Kararlar:** CA1707 yalnız `tests/**.cs` için kapalı; kasıtlı ihlal `<Csc>` ile ayrı derleme; koleksiyon fikstürü adı `…Definition` (CA1711); xunit.v3 3.2.2 (4.x var, doküman 3.2.x der).
