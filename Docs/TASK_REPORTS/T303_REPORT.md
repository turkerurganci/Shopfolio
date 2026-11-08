# T303 — İlk yönetici yönetim komutu ve komut çatısı

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Program.cs`: ilk argüman `admin` ise `CommandHost.ExecuteAsync(args)`; değilse web ev sahibi. Komut kipinde web ev sahibi kurulmaz.
- `src/Shopfolio.Web/Commands/` (ad alanı `Shopfolio.Commands`): `CommandContracts` (`CommandExit` 0/1/2/3, `IAdminCommand`, `CommandOption`, `CommandInput`), `Terminal` (`ITerminal`, `ConsoleTerminal`, `TerminalPrompt`), `CommandDispatcher` (tek geniş `catch`, kullanım hatasında hizmetler kurulmaz), `AdminCommandCatalog.All`, `CreateAdminCommand`, `CommandHost`.
- `CommandHost.CreateServices`: `Maintenance` profilli `AppDbContext`, dokuz modül + `AddJobQueue`, sonra `RemoveAll<IHostedService>()`; komut kaydı `AuditFile.Command` dosyasına (uygulamanın dosyasına ikinci süreç yazmaz). Ayarlar web'in kullandığı kaynaklardan (ana bilgisayar kurucusunun varsayılanları + sır dosyaları).
- Administration modülü: `IAdministrationModule.CreateAdminAsync` + `Contracts/AdminCreationResult` (+ `AdminCreationFields` sabitleri); e-posta `InvalidEmailAttribute`, şifre `PasswordPolicy.Validate` (web ile aynı işlev), yinelenen e-posta `EmailAlreadyRegistered`.
- Tarama/mimari: `CommandSourceScan` (+ `JobSourceScan` geniş-catch listesine dağıtıcı, `SessionWriteSourceScan` izinli listesine `CommandHost.cs` kayıt satırı).
- Dokümanlar: `09 §4.1` beş klasör (`Commands/`), `09 §13.2` madde 1 (`RemoveAll<IHostedService>`) ve madde 2 (`ExecuteAsync`), `CONTEXT.md` satırı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Commands/*` (yeni), `Program.cs`, `Modules/Administration/{AdministrationModule.cs, Contracts/IAdministrationModule.cs, Contracts/AdminCreationResult.cs}`.
- Testler: `tests/Shopfolio.UnitTests/{Commands/*, Fakes/FakeTerminal.cs, Architecture/{CommandSourceScan,JobSourceScan,SessionWriteSourceScan}.cs}`, `tests/Shopfolio.IntegrationTests/Commands/CreateAdminCommandTests.cs`.
- `Docs/09_CODING_GUIDELINES.md`, `.claude/CONTEXT.md`, `Docs/IMPLEMENTATION_STATUS.md` (T303 satırı), `Docs/STATUS_CHANGELOG.d/T303.md`, `Docs/DEFERRED_BACKLOG.d/T303.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `admin` ilk argümanı komut kipi; web ev sahibi, zamanlayıcı, nabız, iş kuyruğu, imha işi yok | ✓ | `Program.cs` ilk dal; `CommandStructureTests.CommandMode_IsSelectedByTheFirstArgumentAdminOnly`, `CommandModeServices_HaveNoHostedServiceAndNoServerService` |
| 2 | Tek statik liste; bilinmeyen komut/seçenek 2 ve değersiz ileti; beklenmeyen hata 1 + tür | ✓ | `AdminCommandCatalog.All`; `CommandDispatcherTests.UnknownCommand_*`, `UnknownOptionOrStrayArgument_*`, `UnexpectedError_ExitsWithOne_AndWritesTheTypeOnly`; gerçek süreç `RealProcess_WithAnUnknownCommand_*` |
| 3 | `--help` komut ve seçenekleri yazar; her komut `05 §8.5` tablosunda | ✓ | `CommandDispatcherTests.Help_*`, `CommandStructureTests.EveryListedCommand_IsARowOfThe09TableAndOfThe05Table`, `ListedOptions_AreTheOnesThe09TableNames`; `RealProcess_WithHelp_*` |
| 4 | Modüllerin `Contracts` arayüzü + `Maintenance` profili | ✓ | `CreateAdminCommand` yalnız `IAdministrationModule`; `CommandModeContext_UsesTheMaintenanceSessionTimes_*`; `CommandsSource_*` (bağlam adı yalnız `CommandHost`) |
| 5 | Ad/e-posta/şifre terminalden; boş ad reddi; şifre yankısız ve iki kez | ✓ | `CreateAdminCommandTests.EmptyName_*`, `TwoDifferentPasswords_*`; `TerminalPromptTests.AskSecret_*`; Integration `CreateAdmin_EmptyNameAndTwoDifferentPasswords_*` |
| 6 | Şifre politikası web'in işleviyle; ileti değer yazmaz | ✓ | `AdministrationModule.CreateAdminAsync` → `PasswordPolicy.Validate`; Integration `CreateAdmin_RefusedPasswordOrAddress_ExitsWithThree_MakesNoAccount_AndPrintsNoValue` |
| 7 | Yönlendirilmiş girdide satır yazmadan 2 | ✓ | `CreateAdminCommandTests.RedirectedInput_ExitsWithTwo_ReadsNothing_AndWritesNoLine`; gerçek süreç `RealProcess_WithARedirectedInput_ExitsWithTwo_WritesNoCommandFile_AndChangesNoRow` |
| 8 | Yönetici doğrulanmış doğar, rol yok | ✓ | Integration `CreateAdmin_OpensAVerifiedAccountWithTheGivenValues_*` (`AdminPasswords.FindByEmailAsync` IsVerified), `CreateAdmin_AdminRowHasNoRoleColumn_*` |
| 9 | İşlem izine/bildirime yazmaz; komut adı işlemden önce kendi dosyasına; satırda ad/e-posta yok | ✓ | Integration `CreateAdmin_WritesNothingToTheTrailTheSentRecordsOrTheJobTables`, `CreateAdmin_CommandFile_HoldsOneLine*`, `CreateAdmin_LineIsWrittenBeforeTheAccountExists_*`, `CreateAdmin_WhenTheLineCannotBeWritten_*` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil kabul kriterleri ve KK1–KK9 diff'te yeniden denetlendi; `impl/sik-fail-nedenleri.md` okundu.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 hizmet koleksiyonunda `IHostedService` ve sunucu hizmeti yok | ✓ | `CommandModeServices_HaveNoHostedServiceAndNoServerService` (hem tanım listesi hem kurulan sağlayıcıdan `GetServices<IHostedService>()` boş) |
| KK2 yanlış komut 2 / değer yok; beklenmeyen hata 1 + tür | ✓ | `CommandDispatcherTests` (yukarıda), gerçek süreç bilinmeyen komut |
| KK3 `--help` her komut tabloda | ✓ | `EveryListedCommand_IsARowOfThe09TableAndOfThe05Table` |
| KK4 `Maintenance` profili | ✓ | `CommandModeContext_UsesTheMaintenanceSessionTimes_NotTheWebProfiles` |
| KK5 şifre görünmez; farklı iki giriş ve boş ad reddi | ✓ | `TerminalPromptTests.AskSecret_ReadsHiddenKeys_NeverTheLineReader_AndEchoesNothing`; `TwoDifferentPasswords_*`; `EmptyName_*` |
| KK6 zayıf şifre reddi, ileti değer taşımaz | ✓ | Integration `..._PrintsNoValue` (üç örnek) |
| KK7 `stdin` boruda 2 | ✓ | gerçek süreç testi (`ProcessStartInfo.RedirectStandardInput`) |
| KK8 doğrulanmış, rolsüz | ✓ | Integration (yukarıda) |
| KK9 iz ve gönderim kaydında satır yok; komut adı işlemden önce; kişisel veri yok | ✓ | Integration (yukarıda; tablo anlık görüntüsü `xmin` dahil değişmez) |
| `09 §25` ilgili satırlar / güvenlik taraması | ✓ | Sır bekçisi (değişken adları), `CommandSourceScan` (Console yalnız `Terminal.cs`, `using static System.Console` dahil; `ReadKey(intercept: true)`; ortam/komut satırı okuma), `SecretSourceScan` ve `SessionWriteSourceScan` yeşil; yeni dış bağımlılık yok |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | `SessionWriteSourceScan` izni yalnız `CommandHost.cs` dosyasına ve yalnız `PermissionValues` kuralına, gerekçesiyle; `JobSourceScan` geniş-catch listesine yalnız dağıtıcı |
| Sık FAIL (2) tarama bir çağrı biçimini kaçırıyor | ✓ | `CommandsScan_OnViolationSamples_*`: yorum satırı, `ReadKey()`/`ReadKey(false)`, `using static`, `System.Console` satır bölünmüş, `GetEnvironmentVariable`, `GetCommandLineArgs`, `AppDbContext` örnekleri |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ (uygulanmaz) | Şema değişikliği yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `09 §4.1` "dört" → "beş klasör" (başka metinde yok: grep), `09 §13.2` adı güncellendi, `CONTEXT.md` satırı, dokuz komut sayısı değişmedi |
| Sık FAIL (5) belge kuralını değiştiren karar kaydısız 09 metni | ✓ (⚠ ile) | `Commands/` klasör ekidir, belge (05/07/11) kuralını değiştirmez; ⚠-1 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 4777/4777 (Commands 43) | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --max-threads 3` | Consistency ve Architecture dahil |
| Integration | ✓ Commands 14/14 | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-namespace Shopfolio.IntegrationTests.Commands` | gerçek PostgreSQL 18.6, gerçek süreç testleri dahil |
| Build/Format | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format --verify-no-changes` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-08; head `46b16782` + `origin/main` `26fd3a96` birleştirmesi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

**Validator kanıtları (tur 1):** kriter 1–9 ve KK1–KK9 kod okuması + testlerle ✓ — `Program.cs` ilk dal `CommandHost.IsCommandMode` (web kurucusundan önce); `CommandHost.CreateServices` sonunda `RemoveAll<IHostedService>()`, `Maintenance` bağlantı dizisi; `CommandDispatcher` iletileri yalnız komut/seçenek adı, üst `catch` yalnız `GetType().Name`; `CreateAdminCommand` `IsInteractive` bakışı her okumadan önce, şifre `ReadKey(intercept: true)` ile iki kez; `AdministrationModule.CreateAdminAsync` → `PasswordPolicy.Validate` (kayıt, sıfırlama ve `PasswordRegime` ile aynı işlev), `InvalidEmailAttribute`; kayıt satırı `AuditLine.AdminCommand(ad, kimlik)` → `command-*.jsonl`, hesap işleminden önce. Yerel: build `-warnaserror` 0/0, `dotnet format` üç proje temiz, Unit 4777/4777, betik testleri 4/4, Integration `Commands` 14/14 ve Administration'ı kullanan komşu sınıflar 113/113 (PostgreSQL 18), birleştirme sonrası build + Unit `Commands` 43/43; dal CI 42483739529 tüm adımlar yeşil. `12` matrisinde T303'ün kaynak birimlerine (06 §8.3, 05 §8.5, 09 §13.2) bağlı satır yok; `12_matris_denetle.py --kapsam 1-77` hata 0.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (komut mevcut `Database:ConnectionString` ve `Logging:Directory` ayarlarını okur)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T303-ilk-yonetici-komut`
- **Commit:** son commit PR'ın head'idir
- **PR:** #469
- **CI:** yapım işçisi izlemez (yönetici yürütür)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | Plan metni tetikleyici terimler için tarandı (plan tier, kota, paket sürümü vb.); task yeni paket ya da dış servis kullanmıyor | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Parola isteminde Ctrl+C varsayılan | D-538 | İptal işleyicisi çıkılamaz ekran bırakırdı; T387 |
| 2 | Komut kipi `StartupSettingsCheck` çalıştırmıyor | D-539 | Komut yalnız kullandığı ayarı okur; T387 |
| 3 | `Date`/`Amount` seçenek türleri | D-540 | İlk kullanan komut T387 |
| 4 | Reddedilen açılışta satır var olmayan hesap kimliği taşır | D-541 | Belge "satır işlemden önce" der; kimlik önceden üretilir; T387 |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi başlangıçta `git status --short` boş). Hook: `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: 42480365166 success, 42480365007 success, 42479203249 success.
- **Repo memory (bitiş kapısı 9):** `MEMORY.md`'ye dokunulmadı; STATUS "Son güncelleme" satırına dokunulmadı (olay `STATUS_CHANGELOG.d/T303.md`).
- **⚠ listesi (kolay, kurala en uygun seçenekle karar):**
  1. `09 §4.1` metni değişti (dört → beş klasör, `Commands/`): `Infrastructure/` modül tiplerini görmediği için komutlar oraya konamaz; belge kuralı değil klasör eki, karar kaydı açılmadı.
  2. Çıkış kodu: boş ad, farklı iki şifre, zayıf şifre, geçersiz/alınmış e-posta = 3 (Refused); yönlendirilmiş girdi ve kullanım hataları = 2.
  3. Metot adı `RunAsync` yerine `ExecuteAsync`: `TransactionBodySourceScan` `RunAsync` çağrılarında lambda arar; `09 §13.2` metni güncellendi.
  4. `AdminCreationField` enum'u kalıcı enum kurallarına (kilit dosyası, `06` tablosu) takıldığı için kaldırıldı; yerine `AdminCreationFields` sabitleri (hiçbir şey saklanmıyor).
  5. Yerel Docker: `t303-pg` PostgreSQL 18.6 kapsayıcısı (55555); iş sonunda silindi.
