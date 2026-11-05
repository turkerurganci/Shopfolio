# T230 — Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi

**Faz:** F2 | **Durum:** ✗ Doğrulama tur 1 FAIL (düzeltme bekliyor) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Paketler: Serilog 4.3.1, Serilog.Sinks.File 7.0.0, Serilog.AspNetCore 10.0.0 (Apache-2.0; nuspec okundu); kilit dosyaları güncel.
- `Infrastructure/Logging/`: `LoggingSetup` (tek yapılandırma noktası), `LoggingOptions` (`Logging:Level`, yeni `Logging:Directory`), `ApplicationLogFormatter`, `ExceptionMasker`, `PathMasker`, `RequestLogEnricher`, `LogEvents` (kapalı liste), `AuditLine` + `IAuditLineWriter`, `AuditLogWriter`.
- Normal yazıcı `WriteTo.File` (günlük, sayı ve boyut sınırı kapalı); denetim ve komut yazıcısı `AuditLogWriter` (ekleme kipinde aç, yaz, boşalt, kapat; hata çağırana atar).
- `Program.cs`: `AddApplicationLogging()` ve `AddAuditLineWriter()`; T205 ve ayar denetimi satırları `ILogger` ile aynı dosyaya iner.
- Belgeler: `09 §18.1` madde 2, 3, 5, kütüphane tablosu; `09 §18.2` madde 2c (T230'da sınandı); `DEPLOY_RUNBOOK` geçerli düzey değerleri ve C bölümü 19. satır (`Logging:Directory`).
- Bu devam turunda: derleme hataları giderildi; `AuditLogWriter` günü yerel saatten değil `TurkeyTime`'dan okur (`09 §7.1` yasağı: `GetLocalNow` ve `DateTime.ToUniversalTime` mimari testi kırdı; biçimleyicideki `DateTime` dalı kaldırıldı); `LoggingFilesTests` ve T205 dosya testi yazıldı.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Infrastructure/Logging/*`, `Program.cs`, `Infrastructure/InstallationSettings/{SettingsAudit,TunableDesignValues}.cs`, `Directory.Packages.props`, `Shopfolio.Web.csproj`, kilit dosyaları; testler: `tests/Shopfolio.UnitTests/{Logging,Architecture,Consistency,Support}`, `tests/Shopfolio.IntegrationTests/{Logging,Web}`; `Docs/09`, `Docs/DEPLOY_RUNBOOK.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Serilog; tek yapılandırma noktası `LoggingSetup` | ✓ | `LoggingSetup.cs`; `LoggingSourceScanTests.ApplicationSource_OutsideTheLoggingFolder_NamesNoLoggingLibrary` |
| 2 | Üç yazıcı üç dosya; gün adı dosya adından | ✓ | `LoggingFilesTests.Writers_WriteThreeSeparateFilesNamedByPrefixAndDay` |
| 3 | Dosya sayısı/boyut sınırı kapalı; bilgim tarihli adlar pakete karşı sınandı | ✓ | `LoggingFilesTests.Writers_WithMoreFilesThanTheLibraryDefaultKeeps_DeleteNoFile` + kontrol grubu `Library_WithItsDefaultRetention_DeletesTheOldFiles`; `AuditTo.File` bulgusu `09 §18.1` madde 2 |
| 4 | Düzey çatı ve veri erişimini indirmez; denetim/komut düzeyden bağımsız | ✓ | `NormalWriter_WithTheLevelSettingAtInformationOrBelow_...` (Debug, Information), `Writers_WithTheLevelSettingAtError_StillWrite...` |
| 5 | `IAuditLineWriter`; hata çağırana; normal yazıcı istek düşürmez; uygulama `AuditLogWriter` | ✓ | `UnwritableDirectory_MakesTheAuditWriterThrowAndDoesNotDropTheRequest`; okuma `Writers_WriteThree...` (FileShare.ReadWrite) |
| 6 | Tek JSON nesnesi, camelCase, `ApplicationLogFormatter`, `RequestLogEnricher`, `PathMasker`, `ExceptionMasker` | ✓ | `ApplicationLogFormatterTests`, `RequestLogEnricherTests`, `PathMaskerTests`; T205/ayar satırı dosyada JSON: `StartupSettingsCheck_WritesItsLine...`, `StartupDatabaseCheckTests.Start_WithAnEmptyList_WritesTheRefusalToTheApplicationFile...` |
| 7 | Olay adları kapalı `LogEvents` | ✓ | `LogEvents_EqualTheClosedListOfTheGuidelines`, `Format_EventNotOnTheList_...`, `ApplicationSource_EveryEventNameArgument_IsAConstantOfTheClosedList` (ihlal örneği `ViolationSamples/Logging/FreeTextEvent.cs`) |
| 8 | Red satırı: tek `code` / `fields` dizisi | ✓ | `Format_FieldValidationRejection_IsOneLineWithAFieldsArray`, `Format_OtherRejection_HasASingleCode` |
| 9 | Silme ve komut satırı iç kimlikle, alan değeri yok | ✓ | `AuditLineFactories_TakeOnlyANameAndAnInternalId`, `AuditAndCommandLines_CarryOnlyTheEventTheNameAndTheInternalId` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–9 | ✓ | yukarıdaki tablo |
| KK1 yapılandırma tek yerde; modül Serilog'a başvurmaz | ✓ | `LoggingSourceScanTests` (ihlal örneği `LibraryInAModule.cs`) |
| KK2 üç dosya ayrı | ✓ | `Writers_WriteThreeSeparateFilesNamedByPrefixAndDay` |
| KK3 otuz ikinci dosyada eski silinmez | ✓ | 40 dosyalı üç önek, hiçbiri silinmedi; kütüphane varsayılanı silerken |
| KK4 ayar bilgiye inince çatı satır yazmaz; düzey yükselince denetim ve komut yazılır | ✓ | `NormalWriter_...` ve `Writers_WithTheLevelSettingAtError_...` |
| KK5 yazılamayan dizin | ✓ | `UnwritableDirectory_...`; bilgim tarihli adlar PR açıklamasında |
| KK6 camelCase | ✓ | `Format_Properties_BecomeCamelCaseFieldsWithTheirTypes` |
| KK7 listede olmayan olay adı reddedilir | ✓ | tarama + biçimleyici testi (kriter 7) |
| KK8 red satırı `fields` | ✓ | `Format_FieldValidationRejection_...` |
| KK9 silme/komut satırında alan değeri yok | ✓ | kriter 9 testleri |
| `09 §25` ilgili satırlar | ✓ | `Serilog.AspNetCore` kütüphane tablosu satırı, `LibraryTableConsistency` (NotYetAdded'dan çıktı), `09 §18.1/18.2` metni test adlarıyla |
| Güvenlik taraması | ✓ | satırda istisna iletisi/ham yol/kişisel veri adı yok (biçimleyici testleri); yeni sır yok; yeni uç nokta yok |
| Sık FAIL (1) beklenen değer sabitten | ✓ | olay listesi `LogEvents.All`, ayar anahtarları `LoggingOptions.*Key`; yalnız sabit saat girdisi (2026-11-05 21:30 UTC) ve eski dosya sayısı `LibraryDefaultRetainedFiles`'tan türer |
| Sık FAIL (2) öteki yerler | ✓ | runbook C 19. satır, `09` kütüphane tablosu, `SettingLayersConsistency` ve `LoggingConsistencyTests` (dizin = Compose birimi = imaj dizini) |
| Sık FAIL (3) istisna geniş değil | ✓ | tarama yalnız `Infrastructure/Logging/` klasörünü dışarıda bırakır (`AppliesToLibraryRule_LeavesOnlyTheLoggingFolderOut`) |
| Sık FAIL (4) ihlal örneği; birim testi dosyaya yazmaz | ✓ | iki ihlal örneği; dosya yazan testler Integration'da |
| Sık FAIL (5) DB varsayılanı/FK | ✓ | bu task'ta tablo/FK yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` | 643/643 |
| Integration | ✓ | gerçek PostgreSQL 18 (Docker), `Web` ad alanı ve geri kalanı ayrı koşuldu (düzeltme turu 1 sonrası yeniden: 1017 test) | 1017 test, hepsi yeşil (`SchedulerBackupDisposalAndKeyTablesTests` iki göç testi yük altında 18–25 sn sürüp bir kez kırıldı, tek başına koşunca 55/55 yeşil; bu task'ın kodu değil) |
| Diğer | ✓ | `dotnet build Shopfolio.slnx -warnaserror`, `dotnet format --verify-no-changes`, `dotnet restore --locked-mode`, `tests/scripts/run-script-tests.sh` | hepsi 0 / 4 betik dosyası geçti |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-05, bağımsız doğrulayıcı; dal head `a4a7d8a`) |
| Bulgu sayısı | 1 (ORTA, S1, güvenlik) |
| Düzeltme gerekli mi | Evet — B1 |
| Bloke-etmeyen not (K) sayısı | 4 — K1–K4 D-87'ye forward (düzeltme turunda kapanabilir) |

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Serilog; tek yapılandırma noktası `LoggingSetup` | ✓ | `src/`'de `Serilog` adı yalnız `Infrastructure/Logging/`'de; `Program.cs` yalnız `AddApplicationLogging()` ve `AddAuditLineWriter()`'ı çağırır; `LoggingSourceScanTests` yeşil, ihlal örneği kırmızı |
| 2 | Üç yazıcı, üç dosya; gün adı | ✓ | `Writers_WriteThreeSeparateFilesNamedByPrefixAndDay` (yerel, gerçek dosya): `app-`, `audit-`, `command-<yyyyMMdd>.jsonl` |
| 3 | Sınır kapalı; yalnız imha siler; bilgim tarihli ad pakete karşı | ✓ | `WriteTo.File(..., retainedFileCountLimit: null, fileSizeLimitBytes: null)`; 40 eski dosyada hiçbiri silinmez + kontrol grubu; `Serilog.Sinks.File` 7.0.0 XML belgesinde `AuditTo.File` aşırı yüklemeleri yalnız yol/düzey/kodlama/kanca alır — `rollingInterval` yok (doc değişikliği doğru) |
| 4 | Düzey çatıyı ve veri erişimini indirmez; denetim düzeyden bağımsız | ✓ | `MinimumLevel.Warning()` + yalnız `Shopfolio` override; testler Debug/Information ve Error ile yeşil |
| 5 | `IAuditLineWriter`; hata çağırana; normal yazıcı istek düşürmez; `AuditLogWriter` | ✓ | `AuditLogWriter.Write` yakalama yapmaz (aç, yaz, `Flush`, kapat); yazılamayan dizinde `IOException`, canlılık 200 |
| 6 | Tek JSON, camelCase, biçimleyici, zenginleştirici, maskeleyiciler kurulu | ✓ | Dosya satırı için karşılandı (biçimleyici ve zenginleştirici testleri). Konsol kanalı ayrı bulgu (B1) |
| 7 | Kapalı `LogEvents` | ✓ | Liste `09 §18.2` madde 3 ile eşit (`LoggingConsistencyTests`); biçimleyici bilinmeyen adı yazmaz; tarama kapsamı K3 |
| 8 | Red satırı biçimi | ✓ | `Format_FieldValidationRejection_IsOneLineWithAFieldsArray`, `Format_OtherRejection_HasASingleCode` |
| 9 | Silme ve komut satırı iç kimlikle | ✓ | `AuditLine` fabrikaları yalnız `(string, Guid)` alır; satırın alanları birebir sınanmış |

**Task satırları (`12 §7.5`):** task'ın kaynakları `05 §9.1`, `09 §18` ve devir kararlarıdır (KP değil); matriste bu kaynağı ana kaynak alan tek satır `VAL-A991` uçtan uca maskelemedir (ters vekil + uygulama kaydı; T225/T231) — T230'un `Durum` hücresi yok (T205 emsali).

**B1 (ORTA, S1, güvenlik):** `LoggingSetup.AddApplicationLogging` `UseSerilog(..., writeToProviders: true)` ile her Serilog olayını çatının kayıtlı sağlayıcılarına da iletir; `WebApplication.CreateBuilder`'ın sağlayıcıları kalır (yorum: "the providers the host has besides stay (the console)"). Validator sondası (geçici test, silindi): sunucuda `ConsoleLoggerProvider`, `DebugLoggerProvider`, `EventSourceLoggerProvider`, `EventLogLoggerProvider` kayıtlı; `Error` düzeyinde iletisi `secret a@b.com Street 5` olan istisna sağlayıcıya istisna nesnesiyle olduğu gibi ulaşır — konsol biçimleyicisi `Exception.ToString()`'i (ileti dahil) ve şablonun bütün özelliklerini (biçimleyicinin dışarıda bıraktığı kişisel veri adlı özellikler dahil) yazar. Konsol konteynerin çıktısıdır: `05 §9.2.3` "konteynerin kendi çıktısı yalnız açılış ve çökme iletileri için … tutulur ve kişisel veri taşımaz", `05 §9.1.5` "hiçbir sistem kaydına, hiçbir seviyede". `09 §18.2` madde 2c'nin (iletiyi hiç yazmama) ve (c) katmanının dosya dışında bir yan yolu açık kalır. Düzeltme yönü: üretim sunucusunda sağlayıcılara iletmeyi kapat (`writeToProviders: false` ya da çatı sağlayıcılarını temizle); testlerin bellek sağlayıcısı (`CollectingLoggerProvider`) ayrı bir yoldan (ör. test sunucusunda Serilog'a ek yazıcı ya da dosyayı okuma) beslenir; konteyner çıktısına yalnız açılış/çökme kalır; bir test konsol sağlayıcısının olay almadığını sınar.

**K-notları:** K1–K4 → D-87 (saat iki kez okunuyor; `Fatal` düzey adı `09 §18.2` tablosunda yok; olay adı taraması yalnız `[LoggerMessage]`; yorumda madde numarası). D-86 (K2) T230'u seçeneklerden biri sayar; bu dal `05 §9.1.3`'e dokunmadı, kayıt açık kalır.

Testler (validator, yerel): build `-warnaserror` 0 uyarı · `dotnet format --verify-no-changes` 0 · Unit 643/643 (Consistency dahil) · betik testleri 4/4 · Integration 1016/1016 (Logging 11, Web 54, Database 35, FileSecrets 24, Model 482 + 410; gerçek PostgreSQL 18) · `dotnet restore --locked-mode` 0 · dal CI run 41939833278 `success` (head `a4a7d8a`).

**Yapım raporu karşılaştırması:** kriter tablosu ve öz denetim tablosu kanıtlı, uyumlu; tek uyuşmazlık güvenlik taraması satırı ("satırda istisna iletisi … yok") — yalnız dosya satırına bakıyor, `writeToProviders: true`'nun konsol kanalını görmüyor (B1).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `Logging:Directory` (`Logging__Directory`, varsayılan `/var/log/shopfolio/app`); runbook C bölümü 19. satır
- **Paketleme / dağıtım:** Yok (imaj dizini `/var/log/shopfolio/app` zaten `app` kullanıcısına ait ve yazılabilir: `deploy/images/app/Dockerfile` satır 38–39; Compose `app-logs` birimi aynı dizine bağlı)
- **Yeni bağımlılık:** Var — Serilog 4.3.1, Serilog.Sinks.File 7.0.0, Serilog.AspNetCore 10.0.0 (Apache-2.0)

## Commit & PR

- **Branch:** `task/T230-kayit-yazicilari-bicimleyici`
- **Commit / PR / CI:** PR başlığında (`T230: Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi`); CI yapım işçisince beklenmez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `AuditTo.File` vardır, hata atar, ama `rollingInterval`/`retainedFileCountLimit`/`fileSizeLimitBytes` yok | paket API'si (Serilog.Sinks.File 7.0.0) | doğrulandı; denetim ve komut yazıcısı `AuditLogWriter` |
| 2 | `WriteTo.File` `retainedFileCountLimit` varsayılanı 31 | kontrol grubu testi | doğrulandı |
| 3 | Serilog.AspNetCore 10.0.0 lisansı Apache-2.0 | nuspec | doğrulandı |
| 4 | Çatı istek kapsamı `RequestPath`/`RequestId`/`ConnectionId` özelliklerini satıra taşır | `Format_FrameworkRequestScopeProperty_IsNotWritten` | doğrulandı, biçimleyici yazmaz |
| 5 | Kütüphane günlük dosya adını yerel saatten verir | kaynak/belge | doğrulandı; denetim yazıcısı Türkiye gününü kullanır (⚠ 1) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Maskelemenin üç katmanının testleri | T231 | planda ayrı task |
| 2 | Red satırını yazan çağrı ve kod bağı | T213 | planda ayrı task |
| 3 | Hesap claim adları (`shopfolio:account-type`, `shopfolio:account-id`) bu task'ın seçimi; oturum açma task'ı bağlar | — | oturum açma henüz yok |

## Düzeltme turu 1 (CI Integration)

**Belirti:** CI "4. Integration test" (run 41932434322) kırmızı: `LivenessEndpointTests.Get_WhenTheCommandIsSlow_...` ve `Get_WhenTheConnectionOpenIsSlow_...` içinde `Assert.Single` "DbTimeout satırı yok". Yerelde (Windows, tek sınıf) yeşildi.

**Kök neden:** `UseSerilog(..., writeToProviders: true)` `preserveStaticLogger` verilmeyince oluşan kaydediciyi statik `Log.Logger`'a yazar ve `ILoggerFactory` o statik kaydediciyle çalışır. Tam Integration koşusunda birden çok test sunucusu aynı işlemde paralel açılır; sonra açılan sunucu ilk sunucunun satırlarını ve sağlayıcılarını (`CollectingLoggerProvider`) devralır, ilk sunucunun `DbTimeout` satırı yanlış sağlayıcıya gider. Linux konteynerinde tam koşuda yeniden üretildi (4 test kırıldı: biri Liveness aynı `Assert.Single` hatasıyla, üçü `StartupDatabaseCheckTests` yavaş bağlantı beklemesiyle; düzeltmeden sonra aynı koşuda Liveness yeşil); tek sınıf koşusu yeşil kaldığı için yerelde görünmedi. Üretimde tek sunucu vardır; ama statik kaydediciye bağlı kalmak yanlış.

**Düzeltme:** `LoggingSetup.AddApplicationLogging` içinde `preserveStaticLogger: true` (her sunucu kendi kaydedicisini tutar). Test gevşetilmedi; `Single(... DbTimeout)` ve T233'ün `≤ 1 DbTimeout` onayı aynen duruyor. Regresyon testi: `LivenessEndpointTests.TwoHostsInOneProcess_EachKeepTheirOwnLogProviders` (düzeltme olmadan kırılır, düzeltmeyle geçer).

**Doğrulama:** düzeltmeyle Linux konteynerinde tam koşuda Liveness testleri yeşil; Windows'ta Integration ad alanlarına bölünerek koşuldu (Web 54, Database 35, Model 893, Logging 11, FileSecrets 24); Unit 643/643; `dotnet build Shopfolio.slnx -warnaserror` ve `dotnet format --verify-no-changes` temiz. Model'de yük altında iki test (15–38 sn, bağlantı bekleme) bir kez kırıldı, tek başına yeşil; bu dalın kodu değil.

### Öz denetim tablosu (düzeltme turu sonrası, `git diff origin/main...HEAD`)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–9 | ✓ | yukarıdaki tablo; düzeltme yalnız `LoggingSetup.cs` içinde tek argüman ve bir test ekledi, kriterlerin testlerine dokunmadı |
| KK1–KK9 | ✓ | tablo değişmedi; `LoggingSourceScanTests` yeşil (Serilog yalnız `Infrastructure/Logging/`) |
| `09 §25` satırları | ✓ | belge değişikliği gerekmedi; `writeToProviders`/`preserveStaticLogger` hiçbir belgede geçmiyor (grep) |
| Güvenlik taraması | ✓ | yeni sır, uç nokta, kişisel veri yok; yeni kod yalnız kaydedici seçeneği ve test |
| Sık FAIL (1) beklenen değer sabitten | ✓ | yeni test sabit satır adlarını yalnız kendi içinde eşler; sayı yok |
| Sık FAIL (2) öteki yerler | ✓ | aynı kuralı anan yer yok (grep) |
| Sık FAIL (3) istisna geniş değil | ✓ | tarama istisnası değişmedi |
| Sık FAIL (4) ihlal örneği; birim testi dosyaya yazmaz | ✓ | yeni test Integration'da, dosya yazmaz (sağlayıcı bellekte) |
| Sık FAIL (5) DB varsayılanı/FK | ✓ | tablo/FK yok |

## Düzeltme turu 2 (doğrulama tur 1 FAIL: B1 ve K1–K4)

**B1 (ORTA, güvenlik; `05 §9.2.3`, §9.1.5, `09 §18.2` madde 2c):** `UseSerilog(..., writeToProviders: false)`. Satırlar çatının sağlayıcılarına (konsol, hata ayıklama, EventSource, EventLog) hiç gitmez; konteyner çıktısına uygulama satırı inmez, tek çıktı üç dosyadır (`05 §9.2.3` açılış/çökme iletilerine izin verir, zorunlu kılmaz). Testlerin yakalaması sağlayıcıya iletmeye dayanıyordu: `ShopfolioWebApplicationFactory` artık sağlayıcıyı sunucuya eklemez, `ProviderLogEventSink` (`ILogEventSink`) kaydeder; `LoggingSetup` servislerdeki sink'leri `ReadFrom.Services` ile okur (üretimde kayıtlı sink yok). `CollectingLoggerProvider`, `LivenessEndpointTests`, `StartupDatabaseCheckTests`, `StartupSettingsCheckTests` onayları değişmedi (`Single(DbTimeout)`, `≤ 1 DbTimeout`, `TwoHostsInOneProcess_...` duruyor). Yeni test: `LoggingFilesTests.ErrorLine_WithAnEmailAndAnAddress_ReachesNoProviderOfTheHostAndNoPersonalDataTheFile` (e-posta ve adresli `Error`/`Critical` satırı sunucuya eklenen sağlayıcıya inmez, dosyada da değer yok).

**K1:** `AuditLogWriter.Write` saati bir kez okur; test `AuditWriter_AtTheTurkeyMidnight_PutsTheLineInTheFileOfTheDayOfItsOwnTime` (okuma sayısı 1, dosya `audit-20261105`, `time` ilk okumadan).
**K2:** `level` alanı kütüphanenin düzey adını olduğu gibi yazar; ayar altı düzeyi kabul eder (`LoggingOptionsValidator`) ve açılış reddi `Fatal`'dır, eşleme reddi gizlerdi. `09 §18.2` madde 1 tablosu olağan üç satırı sayar; belge değişmedi, kural gevşemedi. Test teorisi Fatal/Debug/Verbose'u da kapsar.
**K3:** `LoggingSourceScan.ScanGeneralEventForms`: `{Event}` içeren `[LoggerMessage]` yönteminde `string @event` parametresi yoksa ve `logger.LogX("{Event}…", metin)` çağrısının ilk argümanı `LogEvents.` sabiti değilse bulgu. İhlal örneği `ViolationSamples/Logging/GeneralEventForms.cs` (satır 8, 9, 14); kaynak taraması `ApplicationSource_EveryGeneralEventForm_NamesAConstantOfTheClosedList`.
**K4:** `AuditLogWriter` özet yorumunda "09 18.1 item 2". D-87 ✅ (PR #386).

**Testler:** Unit 648/648; Integration (PostgreSQL 18, `shopfolio-t230-fix2-pg`, silindi): Logging 13, Web 54, Database 35, FileSecrets 24, Model 893. `dotnet build Shopfolio.slnx -warnaserror` ve `dotnet format --verify-no-changes` temiz.

### Öz denetim tablosu (düzeltme turu 2, `git diff origin/main...HEAD`)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–9 | ✓ | yukarıdaki tablo; düzeltme kriterlerin testlerini değiştirmedi. 1: `LoggingSourceScanTests` (Serilog adı yalnız `Infrastructure/Logging/`; test projesi `src` dışı). 4: `Format_Level_IsTheLevelName` genişledi, düzey testleri yeşil |
| KK1 yapılandırma tek yerde | ✓ | `grep -n Serilog` `src` altında yalnız `Infrastructure/Logging/` (+ csproj paket satırları) |
| KK2 üç dosya ayrı | ✓ | `Writers_WriteThreeSeparateFilesNamedByPrefixAndDay` yeşil |
| KK3 otuz ikinci dosya | ✓ | `Writers_WithMoreFilesThanTheLibraryDefaultKeeps_DeleteNoFile` yeşil |
| KK4 düzey | ✓ | `NormalWriter_...`, `Writers_WithTheLevelSettingAtError_...` yeşil |
| KK5 yazılamayan dizin | ✓ | `UnwritableDirectory_...` yeşil |
| KK6 camelCase, KK8 `fields`, KK9 alan değeri yok | ✓ | biçimleyici ve `AuditLine` testleri yeşil (Unit 648) |
| KK7 olay adı | ✓ | iki tarama (`@event` parametreli ve genel biçim) + biçimleyici; her ikisinin ihlal örneği kırmızı |
| `09 §25` satırları | ✓ | belge değişmedi; kütüphane tablosu aynı (`LibraryTableConsistency` yeşil) |
| Güvenlik taraması | ✓ | B1 kapandı: sağlayıcıya iletme kapalı, konsol çıktısı yok (test); dosya satırı değeri yazmaz; yeni sır/uç nokta yok; `ReadFrom.Services` yalnız kodun kaydettiği sink'i okur (üretimde yok) |
| Sık FAIL (1) maskelemesiz ikinci çıktı | ✓ | satır yalnız dosyaya gider; sağlayıcı yolu kapalı ve testli |
| Sık FAIL (1b) beklenen değer sabitten | ✓ | olay adı `LogEvents.UnexpectedError`; saat testinde dosya adı sabit tarih girdisinden, `time` beklentisi `ApplicationLogFormatter.TimeFormat` ve girdi anından; e-posta/adres değerleri test içinde tek yerde sabit |
| Sık FAIL (2) öteki yerler | ✓ | `writeToProviders`/konsol sağlayıcısı geçen belge yok (grep `Docs/09*`, `DEPLOY_RUNBOOK`); kural değişmedi |
| Sık FAIL (3) tarama istisnası geniş değil | ✓ | `AppliesToLibraryRule` aynı; yeni tarama tüm `src`'yi tarar, istisnasız |
| Sık FAIL (4) ihlal örneği; birim testi dosyaya yazmaz | ✓ | `GeneralEventForms.cs`; K1 ve B1 testleri dosya yazdığı için Integration'da |
| Sık FAIL (5) DB varsayılanı/FK | ✓ | tablo/FK yok |

## Notlar

- **⚠ (öneriyle karar):** (1) Denetim/komut dosya günü Türkiye gününe göre, normal dosya kütüphanenin yerel gününe göre; sunucu saat dilimi Türkiye değilse gün sınırı kayar (`09 §18.1` madde 2'ye yazıldı; saat dilimini Türkiye'ye sabitlemek kurulum kararıdır). (2) `LoggingOptions.Level` türü `LogEventLevel`; runbook geçerli değerler Verbose..Fatal. (3) `Logging:Directory` runbook C bölümüne 19. satır. (4) `LoggingOptions` `TunableDesignValues.cs`'ten `Infrastructure/Logging/`'a taşındı. (5) Hesap claim adları (yukarıda).
- **Açılan/kapanan D-kaydı:** yok.
- **Repo memory (bitiş kapısı 9):** güncellendi
