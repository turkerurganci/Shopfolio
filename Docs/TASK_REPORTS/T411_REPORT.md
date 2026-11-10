# T411 — Kayıt maskeleme taramasının dizge farkında yeniden yazımı

**Faz:** F4 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Ortak sözcüksel okuyucu `LogSourceText`** (yeni): C# dosyasını yorum ve dizge ayırarak okur; düz, sözcük (`@"…"`), ara değerli (`$"…"`, `$@`, `@$`), ham (`"""…"""`, çok tırnaklı ve `$$`'lı) ve `+` ile birleştirilmiş dizgelerin hepsi tek sabit olarak çıkar; ara değerli dizgede delik (`{x}`) kod sayılır, `{{` kaçışı tek `{` olur. Kayıt çağrılarını (`LogXxx`, düzey yöntemleri, düzeyli `Write`), `[LoggerMessage]` (adlı/konumlu, `Attribute` sonekli/soneksiz, ad alanı önekli), `LoggerMessage.Define/DefineScope`, `new AuditLine`, `BeginScope/ForContext/PushProperty/WithProperty` ve `const string` bildirimlerini parantez aralığı ve bağımsız değişken listesiyle verir.
- **`LogTemplateSourceScan` yeniden yazıldı:** yorum ayıklayıcı dizge farkında (dizge içindeki `//` artık sonrasını gizlemez); ad kuralı "içerir" ve `ApplicationLogFormatter.IsForbiddenName` ile **biçimleyicinin kullandığı aynı yöntem**; kasıtlı istisnalar açık izin listesi `ApplicationLogFormatter.AllowedPropertyNames` (şimdi yalnız `MissingKeys`, tam ad); kapsam/özellik çağrılarında dizge özellik adıdır; **istisna bağımsız değişkeni yasağı** (`ExceptionAsALogTemplateArgument`): şablondan sonraki bağımsız değişken istisna ise (bildirilmiş istisna değişkeni, `ex`/`exception`, `.InnerException`; `ex.Message` ve `ex.ToString()` dahil) ya da `[LoggerMessage]` şablonu `Exception` parametresinin adını anıyorsa bulgu.
- **`RejectionLineSourceScan` aynı genişlemeyle yeniden yazıldı:** dizge/yorum farkında; `Code`/`Fields` ile `ErrorCode`/`RejectionCode` içeren adlar (`CodePoint`, `Fieldset`, `StatusCode` bulgu değil); dosyadaki her dizgede (sabitteki şablon dahil) aranır; kapsam adları; `ErrorCode` türlü değerin kayıt çağrısına bağımsız değişken olması (D-89 K3); `.cshtml`/`.razor` satır satır okunur.
- **Bastırılan olaylar üç:** `LoggingSetup.SuppressedDataAccessFailures`'a `CoreEventId.SaveChangesFailed` (`Microsoft.EntityFrameworkCore.Update`) eklendi; `09 §18.1` madde 4, kod örneği yorumu ve madde 8 "üç" olarak eşitlendi; belge ile kod eşitliği `LoggingConsistencyTests`'te sınanıyor.
- **`09 §18.2`** madde 2a (içerir kuralı, izin listesi, tarama kapsamı, istisna yasağı), 2c (biçimleyici kuralı) ve madde 7 (test ve ihlal örnekleri) güncellendi.
- **Backlog:** D-89, D-90 yerinde ✅; yeni D-1005 (`DEFERRED_BACKLOG.d/T411.md`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Logging/ApplicationLogFormatter.cs` (`IsForbiddenName`, `AllowedPropertyNames`), `LoggingSetup.cs` (üçüncü olay)
- `tests/Shopfolio.UnitTests/Architecture/LogSourceText.cs` (yeni), `LogTemplateSourceScan.cs`, `RejectionLineSourceScan.cs` ve ikisinin testleri
- `tests/Shopfolio.UnitTests/Consistency/LoggingConsistencyTests.cs`, `tests/Shopfolio.IntegrationTests/Logging/LoggingFilesTests.cs`, `SystemLogsTests.cs`
- `tests/Shopfolio.UnitTests/Architecture/ViolationSamples/Logging/`: `TemplateForms.cs`, `ScopeForms.cs`, `ExceptionArguments.cs`, `RejectionLineForms.cs`, `RejectionLineView.cshtml` (yeni)
- `Docs/09_CODING_GUIDELINES.md` §18.1 madde 4 ve 8, §18.2 madde 2a, 2c, 7; `Docs/DEFERRED_BACKLOG.md` (D-89, D-90), `Docs/DEFERRED_BACKLOG.d/T411.md`; `Docs/STATUS_CHANGELOG.d/T411.md`; `Docs/IMPLEMENTATION_STATUS.md` (T411 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dizge ve yorum farkında; K1 biçimleri taranır | ✓ | `LogSourceText.Parse`; `LogTemplateSourceScanTests.ScanTemplates_EachStringForm_IsReportedOnItsLine` (dizge içi `//`, ara değerli, sözcük, ham, `$$` ham, birleştirilmiş), `…ACommentMarkerInsideAString_HidesNothingAfterIt`, `…OnTheFormsSample_…` (`TemplateForms.cs`) |
| 2 | Ad kuralı "içerir"; istisnalar açık izin listesinde | ✓ | `ApplicationLogFormatter.IsForbiddenName` / `AllowedPropertyNames`; `…AForbiddenWordInTheMiddleOrAtTheEndOfAName_IsReported`, `…EveryNameOfTheAllowList_IsNotReported_AndOnlyTheWholeNameIs`, `AllowList_HoldsOnlyNames…`, `…IsUsedByTheApplication` |
| 3 | `BeginScope`, `ForContext`, `PushProperty`, konumlu `[LoggerMessage]` taranır | ✓ | `…OnTheScopeSample_…` (`ScopeForms.cs`), `…AScopeOrEnricherForm_NamingAForbiddenProperty_IsReported` (7 biçim), `…APositionalLoggerMessage_IsReadLikeANamedOne` |
| 4 | Kayıt çağrısında istisna bağımsız değişkeni taramayla yasak | ✓ | `…OnTheExceptionSample_…` (`ExceptionArguments.cs`), `…AnExceptionAfterTheTemplate_IsReported`, `…TheExceptionInItsSlotOrNoException_IsNotReported`; `09 §18.2` madde 2a |
| 5 | `RejectionLineSourceScan` aynı genişlemeyle tarar | ✓ | `RejectionLineSourceScanTests.OnTheWiderFormsSample_…`, `OnTheViewSample_…`, `EachForm_OutsideTheTwoWriters_IsReported` (12 biçim), `IsCodeOrFieldsName_…` |
| 6 | Bastırılan olaylar üç (`SaveChangesFailed`); `09 §18.1` m.4 ve m.8 eşit | ✓ | `LoggingSetup.SuppressedDataAccessFailures`; `LoggingConsistencyTests.SuppressedDataAccessFailures_EqualTheEventsOfTheGuidelinesAndTheirCount`; `LoggingFilesTests.NormalWriter_DoesNotWriteTheThreeDataAccessFailureEvents_…` |
| 7 | Her kuralın ihlal örneği kırmızı, düzeltilmişi yeşil | ✓ | `ViolationSamples/Logging/*` (her örnekte temiz satırlar bulgu çıkarmaz); gerçek `src/` taraması yeşil (`ApplicationSource_EveryLogTemplate_…`, `TheApplication_WritesTheRejectionEvent…`) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: dizge içi `//` ve üç dizge biçimi örnekli | ✓ | `TemplateForms.cs` satır 9 (`//`), 10 (ara değerli), 11 (sözcük), 12/16 (ham); `…EachStringForm_IsReportedOnItsLine` |
| KK2: "içerir" kuralı ve izin listesi | ✓ | `PhoneNumber/CardNumber/PasswordHash/TokenValue` `…AForbiddenWordInTheMiddle…`; izin listesi testleri |
| KK3: dört yeni çağrı biçimi taranır | ✓ | `ScopeForms.cs` (BeginScope, ForContext, PushProperty, WithProperty), `TemplateForms.cs` satır 25/28/31–37/40 (konumlu, `Attribute` sonekli, çok satırlı ham, `Define`) |
| KK4: istisna bağımsız değişkeni örneği kırmızı | ✓ | `ExceptionArguments.cs` satır 16–19, 27 |
| KK5: red satırı taraması genişledi | ✓ | `RejectionLineForms.cs`, `RejectionLineView.cshtml` |
| KK6: süzgeçte üç olay; `09 §18.1` eşit | ✓ | `LoggingConsistencyTests` (belge olayları, kimlikleri ve sayı sözcüğü); `09` satırları 1978, 1985, 1992 |
| KK7: ihlal örnekleri `ViolationSamples` altında | ✓ | `tests/Shopfolio.UnitTests/Architecture/ViolationSamples/Logging/` (beş yeni dosya) |
| `09 §25` ilgili satırlar | ✓ | §25 "kayıt şablonunda kişisel veri alan adı yok mu" satırı (§18.2 madde 2) aynı kaldı; kuralı madde 2a taşır, §25'e yeni satır gerekmedi |
| Güvenlik taraması | ✓ | Yeni dış bağımlılık yok, gizli bilgi yok; maskeleme taraması sıkılaştı |
| Sık FAIL (1) tarama istisnası geniş | ✓ | Yeni istisna listesi tek ve tam ad (`MissingKeys`); `const string` yalnız `{Event}` açılışlı şablonlar (rota kalıpları dışarıda); `.cshtml` satır satır, istisnasız |
| Sık FAIL (2) çağrı biçimi kaçıyor | ✓ | ek argüman, adlı argüman, `Attribute` soneki, çok satırlı ham, ad alanı önekli nitelik, `Define`, `Write(LogEventLevel…)` örnekli |
| Sık FAIL (4) öteki yerler güncellendi | ✓ | `SystemLogsTests` (sonek kuralı → `IsForbiddenName`), `LoggingFilesTests` adı/yorumu, `09` iki yer + kod örneği, `ApplicationLogFormatter`/`LoggingSetup` yorumları; "iki olay" geçen başka yer kalmadı (`grep`) |
| Sık FAIL (5) belge kuralı karar kaydısız | ✓ | Kural değişikliklerinin hepsi D-89 / D-90 K3 karar kayıtlarına (K-1579, K-1580) dayanır |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 5873/5873 | `dotnet test tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | 58 sn, 0 hata (F3 sonu 5791) |
| Integration | ✓ 30/30 (Logging ad alanı) | `dotnet test tests/Shopfolio.IntegrationTests/... --filter-namespace Shopfolio.IntegrationTests.Logging` | PostgreSQL 18 `track_commit_timestamp=on`; tam koşu CI'da |
| Contract | — | — | Bu task'ta yok |

`dotnet format --verify-no-changes` (değişen dosyalar, UnitTests ve Web): temiz.

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

- **Branch:** `task/T411-kayit-maskeleme-taramasi`
- **Commit:** `fad84128` — T411: Kayıt maskeleme taramasının dizge farkında yeniden yazımı
- **PR:** #514
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok (plan tier, kota, paket sürümü, API sözleşmesi yok); EF Core `CoreEventId.SaveChangesFailed` ve kategorisi `Microsoft.EntityFrameworkCore.Update` | Derleme ve `LoggingFilesTests` (olay yazılmıyor, aynı kategoride uyarı yazılıyor) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Tür bilgisi gerektiren artık boşluklar (başka dosyadaki istisna alanı, `var` ile alınan `ErrorCode`, anonim tür kapsamı, `var` ile tutulan şablon) | D-1005 | Roslyn çözümleyicisi F2 gate'te seçilmedi; bugün `src/`'de örneği yok |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** `42760836228` success, `42760836314` success, `42752063845` success (son iki yeni run devam ediyordu, sayılmadı)
- **Repo memory (bitiş kapısı 9):** MEMORY.md'ye dokunulmadı
- **⚠ Kolay kararlar (kurala en uygun seçenekle):** (a) biçimleyici de "içerir" kuralına geçti (D-89 "biçimleyicide de yakalanmaz" der; tarama ile biçimleyici aynı yöntemi paylaşır); yeni kural çatının `Key`/`Token`/`Address` içeren özellik adlarını da dosyadan çıkarır, `src/` şablonlarında etkilenen ad yok; (b) izin listesi yalnız `MissingKeys` (`KeyCount` `src/`'de kullanılmadığı için eklenmedi); (c) `const string` yalnız `{Event}` açılışlı olduğunda şablon sayılır; (d) `09` sürüm numarası bu task'ta artırılmadı (faz kapısı kayıt PR'ı artırır).
- **Süre:** yaklaşık 25 dk
