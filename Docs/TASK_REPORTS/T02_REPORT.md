# T02 — Çözüm ve proje düzeni

**Faz:** F0 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-02

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `Shopfolio.slnx` (.NET 10 SDK'nın `.slnx` biçimi) üç projeyi toplar: `Shopfolio.Web`, `Shopfolio.UnitTests`, `Shopfolio.IntegrationTests`.
- `src/Shopfolio.Web`: .NET 10 Razor Pages uygulaması (şablon içeriği temizlendi; yalnız `Program.cs` ve `appsettings.json`); `Modules/`, `Infrastructure/`, `Pages/`, `Endpoints/` (`.gitkeep`) ve `wwwroot/css/base.css`, `wwwroot/js/common.js` (düz CSS/JS, `09 §16.1`).
- İki test projesi: davranış kodu ve test çatısı yok (xUnit v3 T03'tedir); ikisi de `Shopfolio.Web`'e başvurur.
- `Directory.Build.props` (`09 §3.1` madde 2), `Directory.Packages.props` (merkezî sürüm, kilit dosyası, geçişli sabitleme), `nuget.config` (yalnız nuget.org), `global.json` (SDK 10.0.400, `latestPatch`), `packages.lock.json` (üç proje), `.editorconfig` (tek, kökte).
- D-01 kalemleri: `.claude/CONTEXT.md` §1 (yığın; dönem "Implementation") ve §3 (klasör düzeyinde, `deploy/` dahil; "kod klasörü henüz yok" satırı kalktı) · `.gitignore` (.NET, test sonucu, IDE, sır dosyası desenleri) · `.gitattributes` (woff2, woff, webp ikili) · `deploy/.env.example` (`09 §4.3` yolu; sır taşımaz) · `SETUP.md` §2 altı kutu işaretlendi. `SETUP_COMPLETE` ve `.github/ci-commands.env` değişmedi.
- `09`: iki "bilgim tarihli" satır sonuçla güncellendi (§3.1 madde 4, §3.2).

## Etkilenen Modüller / Dosyalar

- Kök: `Shopfolio.slnx`, `global.json`, `nuget.config`, `Directory.*.props`, `.editorconfig`, `.gitignore`, `.gitattributes`, `SETUP.md`
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
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `deploy/.env.example` (sır olmayan ayar şablonu; anahtar adları örnektir, ayarı getiren task'ta kesinleşir — `09 §4.3` madde 3)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (NuGet paketi eklenmedi)

## Commit & PR

- **Branch:** `task/T02-cozum-ve-proje-duzeni`
- **Commit / PR / CI:** PR açıklamasında (işçi CI izlemez)

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
| 1 | Dependabot kilit dosyası güncellemesi (`09 §3.2`) sınanmadı | — | Dependabot yapılandırması bu task'ın kapsamı değil; ilk PR'ında gözlenir ve gerekirse `09 §3.2` aynı PR'da düzeltilir |
| 2 | Test projeleri çatısız; xUnit v3 T03'te | — | Plan T03'ü işaret eder |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** run 41473772963 success, 41473773052 success, 41365430173 success
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Küçük kararlar:** `.env.example` `09 §4.3`'ün yoluna (`deploy/.env.example`) konuldu; `global.json` SDK 10.0.400, `rollForward: latestPatch`; CONTEXT §1 "Mevcut dönem" Implementation yapıldı; CRLF'li dotnet çıktıları LF'e çevrildi.
