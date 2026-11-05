# T231 — Maskelemenin üç katmanının testleri

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-05

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
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-05, head `e20aef8`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (K1–K3 → D-89) |

Kabul kriterleri tablosundaki yapım kanıtları validator tarafından yeniden koşularak doğrulandı (aşağıdaki "Doğrulama Sonucu — tur 1" bölümü).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T231-maskeleme-testleri`
- **Commit:** yapım commit'i
- **PR:** #387
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
| 1 | Şablon taraması tek satırlı dizge sabitlerini okur; çok satırlı ya da ham dizge şablonları görmez (bugün `src/`'de yok) | D-89 (validator ekledi) | Kapsam dışı; kural `ILogger` çağrısının ilk dizge bağımsız değişkeni |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** 41946974227 success, 41946974061 success, 41928364198 success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Kolay ⚠ kararları (öneriyle):** (a) D-88 K1 için "biçimleyici istek alanı adlarını sahiplenir" yerine "kaynak taraması bu adları yasaklar" seçildi (`accountId` `AdminCommand` şablonunda meşru); (b) taramanın yasakladığı istek adları yalnız `Path` ve `Method`; (c) `PathMasker` boş bölümlü yolu maskeler (yeni davranış, belgeye yazıldı).

---

## Doğrulama Sonucu — T231 Maskelemenin üç katmanının testleri — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T231-maskeleme-testleri   **Commit:** e20aef8 (incelenen head; doğrulama kaydı commit'i bunun üstüne)

### Verdict: ✓ PASS

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un `scripts/git-hooks`'u, mutlak yol; worktree için işlevsel eşdeğer) |
| Ana dal CI (son 3) | ✓ | 41946974227 success · 41946974061 success · 41928364198 success |
| Memory drift | ✓ | `.claude/memory/MEMORY.md:22` T231 satırı var |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Yazım katmanı: iç kimlik yazılır; şablonda kişisel veri adı yok (2a) | ✓ | `LogTemplateSourceScanTests.ExampleCall_WritesTheInternalIdAndNotTheAddress` (satırda Guid var, `a@b.com` yok; doğru biçim taramada temiz) · `ApplicationSource_EveryLogTemplate_*` gerçek `src/` üzerinde 0 bulgu; validator `src/`'deki bütün kayıt çağrılarını grep'le listeledi (3+2+1 `[LoggerMessage]`, 2 `AuditLine`): hepsi taramanın yakaladığı biçimde |
| 2 | Tarama yasak adlarda, `EnableSensitiveDataLogging` / `Include Error Detail`'de kırılır (2a, 2c, 7) | ✓ | İhlal örnekleri `ViolationSamples/Logging/PersonalDataTemplates.cs` (satır 9, 10, 12, 16, 19) ve `SensitiveDataLogging.cs` (7, 8, 9) beklenen satırlarla kırmızı; dokuz ad × dört biçim (`{X}`, `{CustomerX}`, `{@X}`, `{X:l}`); örnekler derlemeden çıkarılmış (`<Compile Remove="Architecture/ViolationSamples/**">`) |
| 3 | `PathMasker.Mask` anahtarlı yolu maskeler; sorgu değerleri `*` (2b, 7) | ✓ | `PathMaskerTests` satır 19: `/siparis/erisim/ABC123/teyit?email=a@b.com` → `/siparis/erisim/{key}/teyit?email=*`; çift eğik çizgili biçim satır 79 |
| 4 | Her anahtarlı yol için tablo satırı; üye sipariş yolu ve anahtarsız/sorgusuz yol değişmez (7) | ✓ | Theory'ler `PathMasker.KeyedPaths`'ten beslenir; liste 09 ile `LoggingConsistencyTests.KeyedPaths_EqualTheListOfTheGuidelines` ile eşit (10 yol); `/siparis/12345`, `/siparis/12345/fatura` değişmez; anahtarsız önek değişmez |
| 5 | İleti (iç dahil) yazılmaz; yasak adlı özellik, `RequestPath`/`RequestId`/`ConnectionId` yazılmaz; `requestId` yazılır (2c, 7) | ✓ | `CommonLogFieldsTests.Line_OfAnUnexpectedError_*` (üç katmanlı zincir, adres ve e-posta yok), `Line_WithTheFrameworkRequestScopeAndTheEnricher_*` (ayırt edici kapsam değeri ve `ABC123` satırda yok, `requestId`=`req-42`, `path` maskeli), `Line_WithAForbiddenNamedPropertyNextToAnAllowedOne_*` (9 ad) |
| 6 | `ExceptionMasker`: veri sözlüğünden yalnız anahtar; ileti yok; `stackTrace` yalnız yığın izi (2c, 7) | ✓ | `ExceptionMaskerTests`: `Describe_DataDictionary_*` (zincirdeki tüm anahtarlar, değer yok), dizge olmayan anahtar tür adıyla, `Describe_StackTrace_*` (`StackTrace` = `exception.StackTrace` ≠ `ToString()`), `Message`/`ToString()` ezilmiş istisna satıra sızmaz |
| 7 | Her ortak alan adı ve türü; `exceptionType`, `innerExceptionTypes`, `PostgresException`'da `sqlState` (7) | ✓ | `CommonLogFields.All` (19 alan) = 09 §18.2 madde 1 tablosunun ilk sütunu + `fields` (`LoggingConsistencyTests.CommonLogFields_EqualTheFirstColumnOfTheFieldTable`); ad ve tür bozma testleri kırmızı (`requestID`, dizge `innerExceptionTypes`, `field`'sız öğe); `sqlState`=`23505`, PostgresException yokken alan yok |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| — | — | — | — | — | — |

Task'ın kaynak birimleri `05:§9.1`, `09:§18`, `devir:K-1354` (11 §7.1 satır 7319, 7351, 7515); KP birimi yok, matriste task'a ait satır yok. Maskelemeyi anan `VAL-A991` (uçtan uca, ters vekil + uygulama kaydı, `Uç yanıtı + Uygulama kaydı`) ve `VAL-A960` (beklenmeyen hata, uçtan uca) birim testle kapanmaz; `Durum` `BEKLEMEDE` kalır (T230 emsali). `12_matris_denetle.py --kapsam 1-77` hata 0.

### Doğrulama Kontrol Listesi
- [x] KK1: örnek çağrı kimlik yazar, adres yazmaz
- [x] KK2: yasaklı adlı sahte şablon ve `EnableSensitiveDataLogging` içeren sahte dosya taramayı kırar
- [x] KK3: `/siparis/erisim/ABC123/teyit?email=a@b.com` maskelenir
- [x] KK4: tablo her anahtarlı yolu kapsar
- [x] KK5: `a@b.com` içeren istisna kayıtta yok
- [x] KK6: iç istisna iletisi yok; veri değeri yok, anahtarı var; `stackTrace`'te ileti yok
- [x] KK7: ad/tür bozulunca kırılır; `sqlState` var; `RequestPath`/`RequestId`/`ConnectionId` ve yasak ad yok, `requestId` var
- [x] `09 §25` ilgili satırlar: kayıtta kişisel veri yok (şablon taraması), yeni kural için kaynak taraması + kasıtlı ihlal örneği kırmızı, beklenen değerler koddan türetilir, testler zayıflatılmadı, CI yeşil; diff kapsamı: `PathMasker` düzeltmesi ve 09 §18.2 üç cümlesi D-88'in T231'e yönlendirdiği işler (K1, K2)
- [x] Güvenlik taraması (aşağıda)

**09 §18.2 değişikliği (gevşetme yok):** `level` satırı kodun zaten yazdığı düzey adlarını belgeler (D-87 K2 kararı, davranış değişmez); madde 2a'ya `Path`/`Method` yasağı eklendi (daha sıkı); madde 2b'ye "boş bölümler atlanır, anahtar yine maskelenir" eklendi (daha çok maskeleme). `PathMasker`'da `segments[0]` boşluk şartı kalktı: göreli yol (`siparis/erisim/X`) da maskelenir — daha sıkı.

**Validator sondaları (geçici birim testi, silindi):** `/siparis/erisim/X;x=1`, `%53ECRETKEY`, büyük harf, sondaki `/` → maskeli. `/./siparis/…` maskelenmez ama Kestrel nokta bölümlerini `Request.Path`'ten önce çözer (ulaşılamaz). `/siparis%2Ferisim/KEY` maskelenmez (yönlendirici eşlemez → K3). Şablon taraması: `"https://x {Email}"`, konumlu `[LoggerMessage(LogLevel.Warning, "{Email}")]`, `LogWarning(GetEx(), "{Email}")`, `$"…"`, `@"…"`, kaçışlı tırnak, birleştirilmiş `Message`, `BeginScope` → 0 bulgu (K1); `{PhoneNumber}`/`{CardNumber}`/`{PasswordHash}`/`{TokenValue}` → 0 (K1). Şablon özelliği olarak verilen istisna nesnesi `detail` ve `message`'a `ToString()`'i (ileti dahil) yazar (K2). `ForContext("path", ham)` maskesiz `path` yazar (K1).

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet run --project tests/Shopfolio.UnitTests` | Total 729, Failed 0, Skipped 0 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 dosya |
| Integration | — (yerelde koşulmadı) | task beklentisi Unit; dal CI'ında Integration job success | — |
| Dal CI | ✓ | `gh run list --branch task/T231-maskeleme-testleri` | 41948281331 success (head `e20aef8`; Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, Gate success); önceki 41948256833 cancelled (eski head) |
| Locked restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; `git status` koşu sonrası temiz |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (örnek değerler uydurma: `a@b.com`, `Rua das Flores…`, `ABC123`)
- Auth etkisi: Temiz (dokunulmadı)
- Input validation: Temiz; `PathMasker` değişikliği maskelemeyi genişletir, daraltmaz
- Yeni bağımlılık: Yok

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| — | — | — | — | Bulgu yok | — | — | — |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Şablon taramasının kaçırdığı biçimler: dizge içindeki `//` yorum sanılır; konumlu `[LoggerMessage]`, parantezli ilk bağımsız değişken, `$"…"`/`@"…"`/kaçışlı tırnak, birleştirilmiş dizge, `BeginScope`/`ForContext`/`PushProperty` taranmaz; sonek kuralı `{PhoneNumber}`, `{CardNumber}` gibi adları geçirir (biçimleyici de). Bugün `src/`'de örneği yok; KK2'nin harfi karşılanır. Yapım raporunun backlog'suz "Known Limitations" 1. maddesi buraya katıldı | DEFERRED_BACKLOG'a forward (D-89) |
| K2 | İstisna nesnesi şablon bağımsız değişkeni olarak verilirse `ToString()` (ileti dahil) özelliğe ve `message`'a yazılır; madde 2c güvencesi yalnız olayın `Exception`'ı için | DEFERRED_BACKLOG'a forward (D-89) |
| K3 | Ters vekilin maskesi (T225) boş bölümü `PathMasker` gibi atlamalı; `%2F` kodlu ayırıcı iki katmanda da maskelenmez (DÜŞÜK) | DEFERRED_BACKLOG'a forward (D-89) |

Ayrıca `DEFERRED_BACKLOG.md` başlığındaki "Aktif kalem" sayısı (48) gerçek açık satır sayısından sapmıştı (ana dalda da); D-89 ile 51'e (48 ⚪ + D-89 + 2 🟡) düzeltildi.

### Yapım Raporu Karşılaştırması
- Uyum: verdict ve KK sonuçları tam uyumlu; 3 küçük uyuşmazlık
- Öz denetim tablosu var, her madde kanıtlı.
- "Sık FAIL (3) — tarama istisnası geniş değil: yalnız `//` yorumlarını atar" iddiası eksik: ayıklayıcı dizge içindeki `//`'dan sonrasını da atar (K1).
- "Known Limitations" 1. maddesi backlog kimliksizdi (`—`); K-notu kuralı gereği D-89'a bağlandı.
- KK5 öz denetim satırındaki `...FromTheOverriddenMessage...` adı gerçek test adıyla (`FormattedLine_OfAnExceptionWithAnOverriddenMessageAndToString_WritesNeitherOfThem`) tutmuyor; format yapımda iki projede, validator'da çözümün tamamında koştu (temiz).

### Merge ve Post-merge
- Squash: yardımcıda (adım 17)
- Post-merge run'lar: yardımcıda (adım 18)
