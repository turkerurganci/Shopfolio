# T02 — Çözüm ve proje düzeni

**Faz:** F0 | **Durum:** ✓ PASS (doğrulama tur 2) — merge `f83876a` (#335) | **Tarih:** 2026-11-02

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `Shopfolio.slnx` (.NET 10 SDK'nın `.slnx` biçimi) üç projeyi toplar: `Shopfolio.Web`, `Shopfolio.UnitTests`, `Shopfolio.IntegrationTests`.
- `src/Shopfolio.Web`: .NET 10 Razor Pages uygulaması (şablon içeriği temizlendi; yalnız `Program.cs` ve `appsettings.json`); `Modules/`, `Infrastructure/`, `Pages/`, `Endpoints/` (`.gitkeep`) ve `wwwroot/css/base.css`, `wwwroot/js/common.js` (düz CSS/JS, `09 §16.1`).
- İki test projesi: davranış kodu ve test çatısı yok (xUnit v3 T03'tedir); ikisi de `Shopfolio.Web`'e başvurur.
- `Directory.Build.props` (`09 §3.1` madde 2), `Directory.Packages.props` (merkezî sürüm, kilit dosyası, geçişli sabitleme), `nuget.config` (yalnız nuget.org), `global.json` (SDK 10.0.400, `latestPatch`), `packages.lock.json` (üç proje), `.editorconfig` (tek, kökte).
- D-01 kalemleri: `.claude/CONTEXT.md` §1 (yığın; dönem "Implementation") ve §3 (klasör düzeyinde, `deploy/` dahil; "kod klasörü henüz yok" satırı kalktı) · `.gitignore` (.NET, test sonucu, IDE, sır dosyası desenleri) · `.gitattributes` (woff2, woff, webp ikili) · `deploy/.env.example` (`09 §4.3` yolu; sır taşımaz) · ~~`SETUP.md` §2 altı kutu işaretlendi~~ (düzeltme turu 1'de geri alındı; `SETUP.md` bu PR'da değişmez — validator tur 2 notu). `SETUP_COMPLETE` ve `.github/ci-commands.env` değişmedi.
- `09`: iki "bilgim tarihli" satır sonuçla güncellendi (§3.1 madde 4, §3.2).

## Etkilenen Modüller / Dosyalar

- Kök: `Shopfolio.slnx`, `global.json`, `nuget.config`, `Directory.*.props`, `.editorconfig`, `.gitignore`, `.gitattributes`
- `src/Shopfolio.Web/**`, `tests/*/**`, `deploy/.env.example`
- `.claude/CONTEXT.md`, `Docs/09_CODING_GUIDELINES.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç proje, dördüncü yok | ✓ | `Shopfolio.slnx` üç `<Project>`; `dotnet build Shopfolio.slnx -c Release` 3 proje derler |
| 2 | Web .NET 10 Razor Pages; dört klasör + wwwroot | ✓ | `Microsoft.NET.Sdk.Web`, `net10.0`, `AddRazorPages`/`MapRazorPages`; klasörler `.gitkeep` ile izlenir |
| 3 | `Directory.Build.props` ayarları | ✓ | Nullable, ImplicitUsings, AnalysisLevel `latest-recommended`, EnforceCodeStyleInBuild, TreatWarningsAsErrors; sondaj: null başvuru hatası CS8602 ve IDE0062 derlemede hata verdi |
| 4 | Tek `.editorconfig`, `dotnet format` temiz | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` çıkış 0 |
| 5 | Merkezî sabit sürüm, kilit, nuget.config, global.json | ✓ | `ManagePackageVersionsCentrally`, `packages.lock.json` üç projede, `dotnet restore --locked-mode` geçti; projelerde `Version` yok |
| 6 | wwwroot düz JS/CSS, npm yok | ✓ | `package.json` yok |
| 7 | D-01 yığın parçaları | ✓ | CONTEXT §1/§3, `.gitignore`, `.gitattributes`, `deploy/.env.example` |
| 8 | Bilgim tarihli ayarlar SDK'ya karşı sınandı | ✓ | Aşağıda ve PR açıklamasında |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | Yok (davranış kodu yok) | — | — |
| Integration | Yok | — | — |
| Contract | NOT_APPLICABLE | — | — |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release --no-incremental` | 0 uyarı, 0 hata |
| Lint | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 2 (2026-11-02, head `01de8f4`) · önceki: ✗ FAIL — tur 1 (head `09c4d41`) |
| Bulgu sayısı | Tur 2: 0 · tur 1: 4 (S1 ×4; KRİTİK yok; hepsi düzeltme turu 1'de kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | Tur 2: 5 (`DEFERRED_BACKLOG.md` D-07…D-11'e forward) · tur 1: 4 (D-03…D-06, kapandı) |

### Validator kanıtları — tur 2

**Başlangıç kapıları:** working tree temiz · `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (mutlak yol, aynı repo; etkin) · ana dal son 3 run 41473772963, 41473773052, 41365430173 — üçü `success` · memory satırı var (MEMORY.md satır 22). Dal CI: run 41487386671 (head `01de8f4`) `success`, 10 check (Guard skipped, 9 success) — `SETUP_COMPLETE=false` olduğu için Lint/Build komutları boştur; derleme ve biçim kanıtı yereldir (SDK 10.0.400).

| # | Kriter | Validator sonucu | Validator kanıtı |
|---|---|---|---|
| 1 | Üç proje | ✓ | `dotnet sln Shopfolio.slnx list` → 3 proje; repoda 3 `.csproj`; `restore --locked-mode` geçti; `build -c Release` ve `-c Debug` 0 uyarı 0 hata |
| 2 | Razor Pages + klasörler | ✓ | `Microsoft.NET.Sdk.Web`, `net10.0`, `AddRazorPages`/`MapRazorPages`; `Modules/`, `Infrastructure/`, `Pages/`, `Endpoints/` (`.gitkeep`), `wwwroot/css/base.css`, `wwwroot/js/common.js` (`09 §16.1` adları) |
| 3 | `Directory.Build.props` | ✓ | Beş ayar `09 §3.1` madde 2 örneğiyle birebir; sondaj: CS8600/CS8602 ve IDE0161 `-c Release` derlemede hata; CA1304/CA1305/CA1311 hata (`latest-recommended` etkin) |
| 4 | Tek `.editorconfig`, format temiz | ✓ | `find` tek dosya; `dotnet format Shopfolio.slnx --verify-no-changes` çıkış 0; sondaj: IDE0008 ve IDE1006 format'ta hata |
| 5 | Merkezî sabit sürüm, kilit, kaynak, SDK | ✓ | csproj'larda `Version` yok; üç kilit dosyası; `nuget.config` `<clear />` + nuget.org; `global.json` 10.0.400 (`latestPatch` — K-notu D-08) |
| 6 | Düz JS/CSS, npm yok | ✓ | `package.json` yok |
| 7 | D-01 yığın parçaları | ✓ | CONTEXT §1 yığın satırı `05 §2.3.1` ile eşit (dosya yedeği aracı dahil); §3 yalnız klasör düzeyinde, `09 §4.2` madde 1'in sekiz satırı var, "kod klasörü henüz yok" kalktı · `.gitignore`: `git check-ignore` ile `.env`, `**/secrets/`, `*.pem`, `appsettings.Production.json`, `bin/`, `obj/`, `TestResults/` yok sayılır; `.env.example`, `packages.lock.json`, `appsettings.json` izlenir; izlenen dosyalardan yok sayılan yok · `.gitattributes` woff2/woff/webp ikili · `.env.example` sır taşımaz, tek anahtar `Database__LivenessTimeoutSeconds` (`09 §10.7` madde 6) · `git diff origin/main -- SETUP.md` boş |
| 8 | Bilgim tarihli ayarlar | ✓ | PR #335 "Dış Varsayımlar" listeliyor; fark (IDE0008, IDE1006 yalnız format'ta) `09 §3.1` madde 4'te, `09 §3.2` güncel; validator sondajı: `RootNamespace=Shopfolio` ile IDE0130 `Infrastructure/` için `Shopfolio.Infrastructure` bekliyor |

**`09 §25`:** okunan bölümler açıklamada (evet) · bulunmayan ad yok (evet) · çelişki yok · diff yalnız görev (evet) · bilgim tarihli doğrulandı (evet) · `.env.example` güncel (evet) · §4.1 düzeni ve CONTEXT §3 (evet) · §3.1 standartları (evet) · sır yok (evet) · öteki sorular uygulanamaz (paket, POST, göç, test yok).

**Güvenlik:** diff'te sır deseni taraması temiz (eşleşmeler yalnız `.gitignore` desenleri ve `.env.example` yorumları); yeni paket yok; auth ve girdi etkisi yok.

**K-notları (forward):** D-07 Dependabot kilit sınaması · D-08 `global.json` `latestPatch` · D-09 IDE0130 Lint'te zorlanmıyor · D-10 `.env.example` başlığı ve `Export:TempDirectory` · D-11 `09`'da kalan bilgim tarihli işaretler.

**Yapım raporu karşılaştırması (tur 2):** verdict uyumlu (8/8 ✓). Uyuşmazlıklar: (1) "Yapılan İşler"de `SETUP.md` §2 kutularının işaretlendiği yazıyordu — düzeltme turunda geri alınmıştı; satır bu turda düzeltildi. (2) Known Limitations 1 (Dependabot) backlog ID taşımıyordu — D-07 olarak forward edildi. (3) D-03 kanıtı "sondaj sayfası `Shopfolio.Pages.Storefront` ad alanıyla derlendi" — sayfa sınıfının ad alanı açıkça yazıldığında `RootNamespace`'ten bağımsız derlenir; `RootNamespace`'in etkisi validator'ın IDE0130 sondajıyla doğrulandı (sonuç değişmez).

### Validator kanıtları — tur 1

**Başlangıç kapıları:** working tree temiz · `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (mutlak yol; hook dosyaları orada, etkin) · ana dal son 3 run 41473772963, 41473773052, 41365430173 — üçü `success` · memory satırı var (MEMORY.md satır 22). Dal CI: run 41484146289 `success` (10 check; Guard skipped) — ama `SETUP_COMPLETE=false`, CI komutları boş, Lint/Build adımları "henüz boş — atlandı"; derleme ve biçim kanıtı yereldir.

| # | Kriter | Validator sonucu | Validator kanıtı (SDK 10.0.400) |
|---|---|---|---|
| 1 | Üç proje | ✓ | `dotnet sln Shopfolio.slnx list` → 3 proje; `restore --locked-mode` çıkış 0; `build -c Release` ve `-c Debug --no-incremental` 0 uyarı 0 hata |
| 2 | Razor Pages + klasörler | ✓ | `Microsoft.NET.Sdk.Web`, `net10.0`, `AddRazorPages`/`MapRazorPages`; beş klasör var; uygulama açıldı, `/css/base.css` 200 |
| 3 | `Directory.Build.props` | ✓ | `msbuild -getProperty`: Nullable=enable, AnalysisLevel=latest-recommended, EnforceCodeStyleInBuild=true, TreatWarningsAsErrors=true; sondaj: CS8600/CS8602 ve IDE0161 derlemede hata |
| 4 | Tek `.editorconfig`, format temiz | ✓ | `git ls-files` tek dosya; `dotnet format --verify-no-changes` çıkış 0 |
| 5 | Merkezî sabit sürüm, kilit, kaynak, SDK | ✓ | csproj'larda `Version` yok; üç kilit dosyası; `nuget.config` `<clear/>` + nuget.org; `global.json` 10.0.400 `latestPatch`; geçişli sabitleme sondajı kilitte `CentralTransitive` |
| 6 | Düz JS/CSS, npm yok | ✓ | `package.json` yok; `wwwroot/css/base.css`, `js/common.js` |
| 7 | D-01 yığın parçaları | **~ Kısmi** | `CONTEXT.md` §3 ilk satırı altı kök dosyayı tek tek sayıyor (`09 §4.2` "Dosya listesi yazılmaz"; KK7 "klasör düzeyinde") · §1 yığın satırı `05 §2.3.1`'in "dosya yedeği aracı (§2.2.9)" ögesini atlıyor · `.gitignore`, `.gitattributes` ✓ · `.env.example` sır taşımıyor ✓, ama anahtar adlarının çoğu dokümanda yok (bulgu 3) |
| 8 | Bilgim tarihli ayarlar | ✓ | PR açıklaması listeliyor; IDE0008 farkı sondajla doğrulandı ve `09 §3.1` madde 4'te; `09 §3.2` güncel |

**`09 §25`:** soru 1 (okunan bölümler açıklamada) **hayır** — PR'ın "Dokuman Referanslari"nda çekirdek yedi bölüm (§1, §2, §5, §6, §23, §24, §25) ve `05 §3.1`, §3.8 yok, §25 cevapları da yok; soru 2 (bulunmayan ad yok) **hayır** — `.env.example` anahtarları; öteki sorular evet ya da uygulanamaz (yeni paket, sır, POST, göç yok).

**Güvenlik:** sır deseni taraması temiz (eşleşmeler yalnız `.gitignore` desenleri ve `.env.example` yorumları); `appsettings.json` bağlantı dizisi taşımıyor; yeni paket yok (yalnız `ProjectReference`; kilit dosyaları boş); auth ve girdi etkisi yok.

**Bulgular (S1):** (1) CONTEXT §3 dosya listesi — KK7 · (2) CONTEXT §1 yığında dosya yedeği aracı eksik — KK7 · (3) `deploy/.env.example` anahtarları uydurma: `Shopfolio__Domain`, `Shopfolio__SenderAddress`, `Shopfolio__Google__ClientId` hiçbir dokümanda yok; `RateLimit__…`, `StatusThreshold__…` bölüm adları yalnız seçenek sınıfı adından türetilmiş; belgedeki tek anahtar `Database:LivenessTimeoutSeconds` (`09 §10.7` madde 6) — `09 §1` "ad uydurulmaz", §25 soru 2 · (4) PR açıklamasında okunan bölümler eksik — §25 soru 1, `09 §24` madde 1. **Ek gözlem (bulgu 3'e bağlı değil, düzeltme turunda bakılır):** `SETUP.md` §2 kutuları işaretlendi, ama `05 §2.3` ve `09 §4.2` madde 2 "SETUP.md'nin değişikliği proje sahibinin onayıyla" der — PR'da ve bu raporda onay kaydı yok; ayrıca SETUP'ın "Teknoloji yığını: *Belirlenmedi*" satırı (185) ve "Kurulum durumu" bölümü (§2 ertelendi, 198) bayat kaldı.

**K-notları (forward):** D-03 `RootNamespace` · D-04 IDE1006 yalnız format'ta · D-05 `.editorconfig` `s_camelCase` · D-06 `UseExceptionHandler("/Error")` sayfasız.

**Yapım raporu karşılaştırması:** 3 uyuşmazlık — KK7'yi rapor ✓ sayıyor (validator ~); rapor "anahtar adları örnektir" diyerek uydurma adları kabul ediyor; SETUP onayı raporda yok. Notlardaki `core.hooksPath` = `scripts/git-hooks` gözlemi validator ortamında mutlak yoldur (etkin; fark önemsiz).

## Düzeltme turu 1

| Bulgu | Yapılan |
|---|---|
| 1 — `CONTEXT.md` §3 dosya listesi | Kök dosyaları sayan ilk satır kaldırıldı; §3 yalnız klasör düzeyinde (`09 §4.2`) |
| 2 — yığında dosya yedeği aracı yok | §1 yığın satırına "dosya yedeği aracı (aday restic, §2.2.9)" eklendi (`05 §2.3.1`) |
| 3 — `.env.example` uydurma anahtarlar | `Shopfolio__Domain`, `SenderAddress`, `Google__ClientId`, `RateLimit__…`, `StatusThreshold__…` çıkarıldı; yalnız dokümandaki `Database__LivenessTimeoutSeconds` (`09 §10.7` madde 6, §3.5 varsayılan 2) kaldı; başlıkta "anahtarı getiren task ekler" notu; sır değeri yok |
| 4 — PR açıklaması | Okunan `09` bölümleri, `05` birimleri ve `09 §25` cevapları PR #335 açıklamasına yazıldı |
| K1 (D-03) | `Shopfolio.Web.csproj`'a `<RootNamespace>Shopfolio</RootNamespace>`; sondaj sayfası `Shopfolio.Pages.Storefront` ad alanıyla derlendi, `msbuild -getProperty:RootNamespace` = `Shopfolio`; sondaj silindi |
| K2 (D-04) | `09 §3.1` madde 4 notuna IDE1006 eklendi; sondajla doğrulandı (yalnız `dotnet format`'ta hata) |
| K3 (D-05) | `.editorconfig` özel statik alan kuralı `_camelCase` (`warning`); `s_camelcase` stili silindi |
| K4 (D-06) | Var olmayan `/Error` sayfasına işaret eden `UseExceptionHandler` bloğu `Program.cs`'ten kaldırıldı; hata katmanı `09 §8.1`'in task'ında yazılır |
| Ek gözlem — `SETUP.md` | `SETUP.md` değişiklikleri geri alındı (`git diff origin/main -- SETUP.md` boş): değişiklik proje sahibi onayıyla yapılır (GUARDRAILS §2; `09 §4.2` madde 2); kutular ve parametre tablosu T09'dan önce tek öneriyle sorulacak |

`DEFERRED_BACKLOG.md`: D-03…D-06 kapalı işaretlendi, aktif kalem 1. Doğrulama: `dotnet build Shopfolio.slnx -c Release` 0 uyarı, `dotnet format Shopfolio.slnx --verify-no-changes` temiz.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `deploy/.env.example` (sır olmayan ayar şablonu; yalnız dokümanda adı geçen anahtar; ayarı getiren task satırını ekler — `09 §4.3` madde 3)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (NuGet paketi eklenmedi)

## Commit & PR

- **Branch:** `task/T02-cozum-ve-proje-duzeni`
- **PR:** #335
- **Post-merge run'lar:** squash `f83876a` (#335) — CI 41489370654 ve Publish 41489370463 success (F0 gate'inde doğrulandı, 2026-11-03)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | .NET 10 SDK kurulabilir/kuruludur | `dotnet --list-sdks`: 10.0.400 | ✓ |
| 2 | `.slnx` SDK tarafından üretilir | `dotnet new sln --format slnx`; `dotnet sln add` ve `build` çalıştı | ✓ |
| 3 | `Directory.Build.props` ayar adları geçerli | Derleme uyarısız; sondaj projesinde CS8602 ve IDE0062 hata verdi (Nullable ve EnforceCodeStyleInBuild etkin) | ✓ |
| 4 | `RestorePackagesWithLockFile` kilit üretir | Üç `packages.lock.json` oluştu; `--locked-mode` geçti | ✓ |
| 5 | Geçişli sabitleme `CentralPackageTransitivePinningEnabled` ile | Sondaj: Serilog.Sinks.File→Serilog 4.2.0 yerine pinli 4.3.1, kilitte `CentralTransitive` | ✓ |
| 6 | IDE0008 ayarı | `dotnet format` hata verir; `dotnet build` raporlamaz → `09 §3.1` madde 4'e yazıldı | ✓ (fark) |
| 7 | Dependabot PR'ı kilit dosyasını günceller | Çevrimdışı sınanamaz; depoda `dependabot.yml` yoktur, ilk Dependabot PR'ında gözlenir | ? Doğrulanamadı |
| 8 | Sondajlar paket kaynağına erişir (nuget.org) | Sondajda Serilog paketleri geri yüklendi | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Dependabot kilit dosyası güncellemesi (`09 §3.2`) sınanmadı | D-07 | Dependabot yapılandırması bu task'ın kapsamı değil; ilk PR'ında gözlenir ve gerekirse `09 §3.2` aynı PR'da düzeltilir |
| 2 | Test projeleri çatısız; xUnit v3 T03'te | — | Plan T03'ü işaret eder |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** run 41473772963 success, 41473773052 success, 41365430173 success
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Küçük kararlar:** `.env.example` `09 §4.3`'ün yoluna (`deploy/.env.example`) konuldu; `global.json` SDK 10.0.400, `rollForward: latestPatch`; CONTEXT §1 "Mevcut dönem" Implementation yapıldı; CRLF'li dotnet çıktıları LF'e çevrildi.
