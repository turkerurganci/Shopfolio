# T230 — Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama bekliyor) | **Tarih:** 2026-11-05

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
| Integration | ✓ | gerçek PostgreSQL 18 (Docker), `Web` ad alanı ve geri kalanı ayrı koşuldu | 1016 test, hepsi yeşil (`SchedulerBackupDisposalAndKeyTablesTests` iki göç testi yük altında 18–25 sn sürüp bir kez kırıldı, tek başına koşunca 55/55 yeşil; bu task'ın kodu değil) |
| Diğer | ✓ | `dotnet build Shopfolio.slnx -warnaserror`, `dotnet format --verify-no-changes`, `dotnet restore --locked-mode`, `tests/scripts/run-script-tests.sh` | hepsi 0 / 4 betik dosyası geçti |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

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

## Notlar

- **⚠ (öneriyle karar):** (1) Denetim/komut dosya günü Türkiye gününe göre, normal dosya kütüphanenin yerel gününe göre; sunucu saat dilimi Türkiye değilse gün sınırı kayar (`09 §18.1` madde 2'ye yazıldı; saat dilimini Türkiye'ye sabitlemek kurulum kararıdır). (2) `LoggingOptions.Level` türü `LogEventLevel`; runbook geçerli değerler Verbose..Fatal. (3) `Logging:Directory` runbook C bölümüne 19. satır. (4) `LoggingOptions` `TunableDesignValues.cs`'ten `Infrastructure/Logging/`'a taşındı. (5) Hesap claim adları (yukarıda).
- **Açılan/kapanan D-kaydı:** yok.
- **Repo memory (bitiş kapısı 9):** güncellendi
