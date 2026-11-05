# T231 — Maskelemenin üç katmanının testleri

**Faz:** F2 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Yazım katmanı:** `LogTemplateSourceScan` (`ILogger` çağrısı, `[LoggerMessage]` ve `AuditLine` şablonlarında yasaklı alan adı ve `{Path}`/`{Method}` taraması; `src/` altında `EnableSensitiveDataLogging` ve `Include Error Detail` taraması) ve iki ihlal örneği.
- **Yol katmanı:** `PathMaskerTests` genişletildi (boş bölüm, büyük harf, anahtarsız yol, boş sorgu). **Kod düzeltmesi:** `PathMasker` boş bölümlü yolda (`//siparis/erisim/ABC123`) anahtarı maskelemiyordu (D-88 K1 sonda); boş bölümler eşleşmede atlanır.
- **Biçimleyici katmanı:** `CommonLogFields` (her ortak alanın adı ve türü) + `CommonLogFieldsTests`; çatı kapsamı özellikleri zenginleştirici yanında; yasak adlı özellik yanında izinli özellik.
- **İstisna katmanı:** `ExceptionMaskerTests` (tür zinciri, veri anahtarları, yığın izi, `PostgresException` her derinlikte, ileti/`ToString()` ezilmiş istisna).
- **Tutarlılık:** `LoggingConsistencyTests` iki denetim aldı (yasak adlar listesi ve ortak alan tablosu 09 §18.2 ile eşit).
- **Belge:** `09 §18.2` — `level` satırı (D-88 K2), madde 2a'ya `Path`/`Method` cümlesi, madde 2b'ye boş bölüm cümlesi.
- **D-88** ✅ kapatıldı (K1, K2).

## Etkilenen Modüller / Dosyaları

- `src/Shopfolio.Web/Infrastructure/Logging/PathMasker.cs` (boş bölüm düzeltmesi)
- `tests/Shopfolio.UnitTests/Architecture/LogTemplateSourceScan.cs`, `LogTemplateSourceScanTests.cs`, `ViolationSamples/Logging/PersonalDataTemplates.cs`, `SensitiveDataLogging.cs`
- `tests/Shopfolio.UnitTests/Logging/ExceptionMaskerTests.cs`, `CommonLogFieldsTests.cs`, `PathMaskerTests.cs`
- `tests/Shopfolio.UnitTests/Support/CommonLogFields.cs`
- `tests/Shopfolio.UnitTests/Consistency/LoggingConsistencyTests.cs`
- `Docs/09_CODING_GUIDELINES.md` (§18.2), `Docs/DEFERRED_BACKLOG.md` (D-88), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Yazım katmanı: kimlik yazılır, şablonda kişisel veri adlı özellik yok | ✓ | `LogTemplateSourceScanTests.ExampleCall_WritesTheInternalIdAndNotTheAddress`, `AuditLineShapes_HaveTemplatesWithNoForbiddenName` |
| 2 | Kaynak taraması yasak adlarda, `EnableSensitiveDataLogging` ve `Include Error Detail`'de kırılır | ✓ | `ApplicationSource_EveryLogTemplate_*`, `ApplicationSource_NeverSwitchesOnValuesInExceptionMessages`; ihlal: `ScanTemplates_OnViolationSample_*`, `ScanExceptionMessageSwitches_OnViolationSample_*`, `ScanTemplates_AnyForbiddenName_*` (dokuz ad × dört biçim) |
| 3 | `PathMasker.Mask` anahtarlı yolu maskeler, sorgu değerleri `*` | ✓ | `PathMaskerTests.Mask_TheDocumentedExample_*`, `Mask_QueryString_*`, `Mask_AQueryTokenWithoutAnEqualsSign_*` |
| 4 | Her anahtarlı yol için satır; üye sipariş yolu ve anahtarsız yol değişmez | ✓ | `Mask_EveryKeyedPath_*` (Theory `KeyedPaths`, liste 09 ile eşit: `LoggingConsistencyTests.KeyedPaths_EqualTheListOfTheGuidelines`), `Mask_APathWithoutAKey_*`, `Mask_AMembersOrderPathAndOtherPaths_*`, `Mask_EveryKeyedPathWithoutItsKey_*` |
| 5 | İleti (iç dahil) yazılmaz; yasak adlı özellik ve `RequestPath`/`RequestId`/`ConnectionId` yazılmaz; `requestId` yazılır | ✓ | `ExceptionMaskerTests.FormattedLine_*`, `CommonLogFieldsTests.Line_WithTheFrameworkRequestScopeAndTheEnricher_*`, `Line_WithAForbiddenNamedPropertyNextToAnAllowedOne_*` |
| 6 | `ExceptionMasker` yalnız veri anahtarlarını yazar; ileti yok; `stackTrace` yığın izi | ✓ | `ExceptionMaskerTests.Describe_DataDictionary_*`, `Describe_ADataKeyThatIsNotAString_*`, `Describe_StackTrace_*`, `FormattedLine_OfAnExceptionWithAnOverriddenMessageAndToString_*` |
| 7 | Her ortak alan adı ve türü doğrulanır; `exceptionType`, `innerExceptionTypes`, `sqlState` | ✓ | `CommonLogFieldsTests.Line_WithEveryCommonField_*`, `Problems_OnALineWith*` (bozma örnekleri), `Line_OfAnUnexpectedError_*`; tablo = 09 §18.2 madde 1: `LoggingConsistencyTests.CommonLogFields_EqualTheFirstColumnOfTheFieldTable` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 örnek çağrı kimlik yazar, adres yazmaz | ✓ | `LogTemplateSourceScanTests.ExampleCall_WritesTheInternalIdAndNotTheAddress` |
| KK2 yasaklı adlı sahte şablon ve `EnableSensitiveDataLogging` içeren sahte dosya taramayı kırar | ✓ | `PersonalDataTemplates.cs` (satır 9, 10, 12, 16, 19) ve `SensitiveDataLogging.cs` (satır 7–9) testlerde beklenen satırlarla |
| KK3 `/siparis/erisim/ABC123/teyit?email=a@b.com` maskelenir | ✓ | `PathMaskerTests.Mask_TheDocumentedExample_*` (+ çift eğik çizgili biçim) |
| KK4 tablo her anahtarlı yolu kapsar | ✓ | Theory beklenen değeri `PathMasker.KeyedPaths`'ten türetir; liste 09 ile eşitliği `LoggingConsistencyTests` korur |
| KK5 `a@b.com` içeren istisna kayıtta yok | ✓ | `ExceptionMaskerTests.FormattedLine_OfTheWholeChain_*`, `...FromTheOverriddenMessage...` |
| KK6 iç ileti satırda yok; veri değeri yok, anahtarı var; `stackTrace`'te ileti yok | ✓ | `ExceptionMaskerTests.FormattedLine_OfTheWholeChain_*`, `Describe_StackTrace_*` |
| KK7 alan adı/türü bozulunca test kırılır; `sqlState`; çatı özellikleri yok; `requestId` var | ✓ | `CommonLogFieldsTests.Problems_OnALineWithARenamedField_*`, `..AFieldOfAnotherType_*`, `Line_OfAnUnexpectedError_*`, `Line_WithTheFrameworkRequestScopeAndTheEnricher_*` |
| `09 §25` ilgili satır: kayda iç kimlik yazılıyor, şablonda kişisel veri adı yok | ✓ | `LogTemplateSourceScanTests.ApplicationSource_EveryLogTemplate_*` (gerçek `src/`) |
| Güvenlik taraması: sır/kişisel veri yok, yeni bağımlılık yok | ✓ | Yalnız test kodu ve `PathMasker` düzeltmesi; yeni paket yok; örnek değerler uydurma (`a@b.com`, `Rua das Flores…`) |
| Sık FAIL (1): beklenen değer sabitten türetilmeli | ✓ | Anahtarlı yol, yasak ad, olay ve çatı adı listeleri koddan okunur; ortak alan tablosu ve yasak ad listesi 09 ile `LoggingConsistencyTests`'te eşitlenir |
| Sık FAIL (2): kural değişince öteki yerler | ✓ | 09 §18.2 üç cümle güncellendi; `KeyedPaths_EqualTheListOfTheGuidelines` ilk denemede belge cümlesi yüzünden kırıldı, cümle yeniden yazıldı; `T230` yorumlarındaki "T231'in" atıfları güncellendi (`PathMaskerTests`) |
| Sık FAIL (3): tarama istisnası geniş değil | ✓ | Tarama yalnız `//` yorumlarını atar; dosya türü listesi açık; hiçbir yol/dosya istisnası yok |
| Sık FAIL (4): ihlal örneği var, birim testi dosyaya yazmıyor | ✓ | İki ihlal örneği; testler yalnız bellek ve depo dosyalarını okur, yazmaz |
| Sık FAIL (5): maskelemesiz ikinci çıktı | ✓ | `ScanTemplates` `{Path}`/`{Method}`'u yasaklar (D-88 K1); T230'un sağlayıcı kapatması değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 729 geçti, 0 kaldı/atlandı (önceki 648) |
| Integration | — | Task beklentisi yalnız Unit; kod değişikliği (`PathMasker`) Unit ile kapsanır | — |
| Format | ✓ | `dotnet format ... --verify-no-changes` (UnitTests ve Web projeleri) | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T231-maskeleme-testleri`
- **Commit:** yapım commit'i
- **PR:** #PR_NO
- **CI:** yöneticide
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan tier, kota, sürüm, API yok; Unit testleri yerel).

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | T230 tamamlandı | `IMPLEMENTATION_STATUS.md` T230 `✓ PASS (tur 2)`, #386 | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Şablon taraması tek satırlı dizge sabitlerini okur; çok satırlı ya da ham dizge şablonları görmez (bugün `src/`'de yok) | — | Kapsam dışı; kural `ILogger` çağrısının ilk dizge bağımsız değişkeni |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** 41946974227 success, 41946974061 success, 41928364198 success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Kolay ⚠ kararları (öneriyle):** (a) D-88 K1 için "biçimleyici istek alanı adlarını sahiplenir" yerine "kaynak taraması bu adları yasaklar" seçildi (`accountId` `AdminCommand` şablonunda meşru); (b) taramanın yasakladığı istek adları yalnız `Path` ve `Method`; (c) `PathMasker` boş bölümlü yolu maskeler (yeni davranış, belgeye yazıldı).
