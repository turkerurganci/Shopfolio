# T215 — Hız sınırı, deneme limitleri ve gövde tavanı

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Gövde tavanı** (`Infrastructure/Security/RequestBodyLimits.cs`): uç sınıfı başına tavan (`RequestBodyClass`; GET gövde taşımaz, genel form 64 KB, uzun metin 1 MB, görsel yükleme P-23 + 1 MB, parça 5 MB, bildirim 64 KB), sınıf uç meta verisi `RequestBodyClassAttribute`; aşan istek gövde okunmadan 413 alır. Uzunluğu bildirilmeyen gövde tavan + 1 bayta kadar okunur. Sayfa 413'ü yalın HTML, makine ucu 413'ü gövdesiz.
- **Genel hız sınırı** (`RequestRateLimiting`, `RateLimitScope`): sayfa ve form gönderimi sınırı zincirli; eksen `ClientAddress.AxisOf` (IPv6 /64); `/statik`, `/gorsel` dışarıda; parçalı yükleme yalnız sayfa sınırında; imzası doğru bildirim dışarıda, bozuk imza `CountSignatureFailure` ile form sayacına; 429 `Retry-After`'sız, sayfada nötr mesaj, makine ucunda gövdesiz.
- **`TimeDrivenSlidingWindowLimiter`**: çatının `SlidingWindowRateLimiter`'ı `TimeProvider` almıyor ve `TryReplenish` gerçek saate bağlı olduğu için sahte saatle çalışmıyor; sınırlayıcı kendi dilim sayacını tutar (altı dilim, 60 sn), `RateLimitPartition.Get` ile kurulur.
- **İstemci adresi ve ters vekil güveni** (`ClientAddress`, `ForwardedHeadersSetup`, `ReverseProxyTrust`): `RemoteIpAddress` tek yerde okunur; `ForwardedHeaders` ilk ara katman; iki iç ağ (IPv4 + IPv6), `ForwardLimit = 1` (D-112).
- **Deneme limitleri** (`AttemptCounter`, `AttemptSubject`, `AttemptLimits`): `RecordAsync` kendi kısa işleminde, `IsExceededAsync`, `ForgetAsync`, `MergeCartAsync`; eşik ve pencere `ProductConstants`'tan (P-31…P-37, P-46, P-49 sabitleri eklendi); `FormResults.AttemptLimitExceeded` (429, değerler korunur, mesaj düğmenin yanında, `Retry-After` yok).
- **Testler**: birim (hız sınırı, gövde tavanı, adres, ters vekil, tutarlılık, kaynak taraması + ihlal örnekleri), entegrasyon (`RequestLimitsTests` 28, `AttemptCounterTests` 14, `AttemptLimitPatternTests` 2).
- **`origin/main` birleştirmesi ve D-143 (2026-11-06):** T213 (#408) merge edildiği için dal `origin/main` ile birleştirildi (rebase değil). Kod çakışması yoktu (`FormResults.cs` kendiliğinden birleşti); belge çakışmaları (`MEMORY.md`, `DEFERRED_BACKLOG.md`, `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `T212_REPORT.md`) iki tarafın içeriği korunarak çözüldü. T215'in 429 ve 413 yolları T213'ün `RejectionLog`'una bağlandı (`09 §8.2` m.3): genel hız sınırının reddi `RejectionLog.RateLimited()` ile (olay `RateLimitExceeded`, `code` = `ErrorCode.RateLimitExceeded`; önceki kodsuz `RateLimitLog` silindi), gövde tavanı reddi `RejectionLog.Rejected(ErrorCode.PayloadTooLarge)` ile birer satır yazar; satırın `code` alanı yalnız `RejectionLog`'da yazılır (kaynak taraması geçer). D-143 kapandı: `ErrorCodeLeakCheck` (yanıt gövdesi + yanıt başlıkları + `response.Content.Headers`, `Enum.GetNames<ErrorCode>()`) 429 (sayfa, form, makine ucu), 413 (sayfa, makine ucu) ve deneme limitinin 429 sayfasında çalışır; dört eski test (`ForgeryProtectionTests`, `SecurityHeadersTests`, `VersionedFormPageTests`, `SignedEndpointDatabaseTests`) elle kurdukları başlık metni yerine aynı yardımcıyı kullanır. `09 §8.2` m.3'ün çağıran sayısı (üç → beş) hizalandı.
- **İkinci `origin/main` birleştirmesi (2026-11-06):** T214 (#410, `9a56f83`) merge edildiği için dal yeniden `origin/main` ile birleştirildi (rebase değil); yalnız belge çakışmaları vardı, iki tarafın içeriği korundu; önceki task kaydı (T214 Commit hücresi `9a56f83`, `T214_REPORT` post-merge satırı) dolduruldu.
- **Belgeler**: `09 §9.4` m.1, `§15.1` m.8, `§15.4` m.1/3/4 ve Test paragrafı; `DEFERRED_BACKLOG` (D-31 (d), D-64, D-103, D-108, D-112, D-135); `DEPLOY_RUNBOOK` D-31 satırı; `deploy/compose.yaml` yorumu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/` (`AttemptCounter`, `AttemptLimits`, `AttemptRecord`, `AttemptSubject`, `ClientAddress`, `ForwardedHeadersSetup`, `RequestBodyLimits`, `RequestRateLimiting`, `TimeDrivenSlidingWindowLimiter`), `Infrastructure/ProductConstants.cs`, `Infrastructure/Forms/FormResults.cs`, `Infrastructure/SignedEndpoints/*` (imza hatası sayımı), `Program.cs`.
- `tests/Shopfolio.UnitTests/{RateLimiting,Consistency,Architecture}`, `tests/Shopfolio.IntegrationTests/{Web,Security,Pages/Panel}`.
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/DEPLOY_RUNBOOK.md`, `deploy/compose.yaml`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Gövde tavanı uç sınıfı başına | ✓ | `RequestBodyLimitMiddlewareTests.TheCeilings_AreTheDocumentsNumbers`, `EveryClass_HasACeiling…`, `AGetOrHead_HasNoBody…`; `RequestLimitsTests.ABodyAtTheClassCeiling_ReachesTheEndpoint…` (genel, uzun metin, makine), `TheGeneralClass_RefusesWhatTheLongTextClassAccepts`; gerçek Kestrel: `OnARealServer_ABodyBelowAtAndOverTheCeiling_…` (genel 64 KB ve uzun metin 1 MB sınıfı × chunked/Content-Length × tavan−1, tavan, tavan+1, +4 MB, 40 MB) |
| 2 | Tavanı aşan 413, form yeniden gösterilmez | ✓ | `RequestLimitsTests.TheRefusal_OfAPage_IsAPlainPageWithNoFormAndNoRetryAfter_AndNothingWasRead`, `TheRefusal_OfAMachineEndpoint_HasNoBody`, `ABodyWithNoDeclaredLength_IsMeasured…` (TestServer); gerçek Kestrel: `OnARealServer_…` (chunked dahil 413, form yok, kod yanıtta yok, `PayloadTooLarge` satırı) ve `OnARealServer_ARefusedChunkedBody_IsNotAnUnexpectedError_…`; `RequestBodyLimitMiddlewareTests.APageRefusal_IsThePlainOperationPage_NoForm_NoStoredValues`, `AServerThatThrowsItsOwn413WhileTheBodyIsRead_…` |
| 3 | Kayan pencere 60 sn, 6 dilim; eksen IP, IPv6 /64 | ✓ | `TimeDrivenSlidingWindowLimiterTests` (dilim dilim, sahte saat), `RequestRateLimitingTests`, `RequestLimitsTests.TwoAddressesOfOneV6Prefix_ShareOneCounter`, `ClientAddressTests` |
| 4 | Statik/görsel muaf; bildirim imzası; parçalı yükleme | ✓ | `RequestLimitsTests.StaticFilesAndImages_AreOutsideTheLimit`, `ANotificationWithARightSignature_IsOutsideTheLimit_AndBadSignaturesCountTowardsTheFormLimit…`, `TheChunkedUpload_IsUnderThePageLimitOnly_NotTheFormLimit`, `PostsAreRefusedWith429AfterTheFormLimit_AGetStillPasses…` |
| 5 | Genel sınır 429, nötr mesaj, makine ucunda gövdesiz, `Retry-After` yok | ✓ | `RequestLimitsTests.APage_IsRefusedWith429AfterThePageLimit_NeutralMessageNoFormNoRetryAfter_AndTheNextWindowOpensIt`, `AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter` |
| 6 | Deneme limitleri 429, değerler, mesaj düğmenin yanında | ✓ | `AttemptLimitPatternTests.TheAttemptOverTheLimit_Answers429_KeepsTheValue_ShowsTheNeutralMessage_AndWritesNoRetryAfter`; `AttemptCounterTests` (14: L-1 5/15 dk, Forget, eşzamanlı, L-2, pencere sınırı, birleşme, normalleştirme) |
| 7 | L-7/L-8 formdan değil hesaptan; sayım T377'de | ✓ | `AttemptLimitsConsistencyTests.L7AndL8_AreNotCountedByTheAttemptRecords_TheTableRefusesThem`; `AttemptCounterTests.The_limits_counted_from_the_orders_are_refused` |
| 8 | Parametre değerleri `02 §11`'den; genel sınır kurulum ayarı | ✓ | `AttemptLimitsConsistencyTests.EveryRowOfTheDocument_IsInTheCodesTable…`, `TheWindowOfEveryLimit_IsTheWindowTheDisposalDeletesBy`; `RateLimitOptions.PageRequestsPerMinute` / `FormSubmissionsPerMinute` (`TunableDesignValues.cs`), `DEPLOY_RUNBOOK` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 her uç sınıfının tavanı yoklanır | ✓ | `RequestLimitsTests.OnARealServer_ABodyBelowAtAndOverTheCeiling_…` (gerçek Kestrel, chunked ve Content-Length, tavan−1/tavan/tavan+1/büyük; genel ve uzun metin sınıfı), `ABodyAtTheClassCeiling…` (TestServer Theory: genel, uzun metin), `RequestBodyLimitMiddlewareTests.ABodyAtTheCeiling_PassesWithItsDeclaredLength_ForEveryClass` / `…OneByteOverTheCeiling_IsRefusedWith413_ForEveryClass`, `TheImageUploadClass_IsTheImageCeilingPlusOneMegabyte` |
| KK2 64 KB üstü 413, form gösterilmez | ✓ | `RequestLimitsTests.TheRefusal_OfAPage_IsAPlainPageWithNoFormAndNoRetryAfter_AndNothingWasRead`; gerçek Kestrel'de chunked ve Content-Length'li: `OnARealServer_ABodyBelowAtAndOverTheCeiling_…` (413, `<form` yok, kod adı yanıtta yok) |
| KK3 dilimler sahte saatle; /64; vekil dışı başlık adresi değiştirmez | ✓ | `TimeDrivenSlidingWindowLimiterTests.ASegmentsPermitsReturn_WhenThatSegmentLeavesTheWindow_NotBefore`, `TwoAddressesOfOneV6Prefix_ShareOneCounter`, `ForwardedHeadersTests.FromAnyOtherSource_TheForwardedHeadersAreIgnored…`, `RequestLimitsTests.TheForwardedAddressAndScheme_AreBelievedFromTheProxysNetworkOnly` |
| KK4 statik ve imzası doğru bildirim sınıra girmez; bozuk imza sayılır; parça form sınırına sayılmaz | ✓ | `RequestLimitsTests.StaticFilesAndImages_AreOutsideTheLimit`, `ANotificationWithARightSignature…`, `ABadSignature_IsAnsweredByTheEndpointEvenWhenTheLimitIsReached`, `TheChunkedUpload_IsUnderThePageLimitOnly…` |
| KK5 429 sayfası nötr, form yok, `Retry-After` yok | ✓ | `RequestLimitsTests.APage_IsRefusedWith429AfterThePageLimit…`, `AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter` |
| KK6 limit aşımında 429, değer korunur, mesaj düğmenin yanında | ✓ | `AttemptLimitPatternTests` (örnek sayfa `Pages/Panel/AttemptSample.cshtml`: özet düğmenin yanında) |
| KK7 misafirin yazdığı e-posta eksene girmez | ✓ | L-7/L-8 tabloda yok: `AttemptLimitsConsistencyTests.L7AndL8_AreNotCountedByTheAttemptRecords_TheTableRefusesThem`, `AttemptCounterTests.The_limits_counted_from_the_orders_are_refused` (sayım ve hesap anahtarı T377) |
| KK8 limit sayısı yalnız `ProductConstants`'ta; uçta sayı yok | ✓ | Çağrı imzası sayı almaz (`RecordAsync(AttemptLimit, AttemptSubject, ct)`); `AttemptLimits` tablosu yalnız `ProductConstants` okur; `AttemptLimitsConsistencyTests`; gövde tavanları `RequestBodyLimitMiddlewareTests.TheCeilings_AreTheDocumentsNumbers`; `AttemptSample` ve `RequestLimitsTests` sayı yazmaz (`AttemptLimits.Of(...).Threshold`) |
| Birleştirme: 429/413 `ErrorCode` ile, red satırı `RejectionLog` ile (`09 §8.2` m.1, m.3) | ✓ | `RequestLimitsTests.EachRefusal_WritesOneRejectionLineWithItsCode_Through_TheRejectionLog` (429: olay `RateLimitExceeded`, `code` `RateLimitExceeded`; 413: `PayloadTooLarge`; `fields` yok); `RejectionLineSourceScanTests` geçer (olay ve `code` alanı yalnız `RejectionLog`'da) |
| Birleştirme: D-143 (429 ve içerik başlıklarında kod yok; `09 §8.2` m.5) | ✓ | `RequestLimitsTests.The429_OfAPageAFormAndAMachineEndpoint_NamesNoErrorCode_…`, `The413_…`, `TheLeakCheck_FindsACodeNameInAContentHeader_AndInTheBody` (yardımcının kendi ihlal örneği: içerik başlığı, yanıt başlığı, gövde); `AttemptLimitPatternTests` 429 sayfası; dört eski test aynı yardımcıyı kullanır |
| 2. birleştirme (T214 #410, `9a56f83`): kriter 1–8 ve KK1–KK8 birleşik diff'te yeniden denetlendi | ✓ | Kod çakışması yok (`Program.cs`, `FormResults.cs` kendiliğinden birleşti; T214'ün `NoticeCode`/`NoticeParameter`/`FormResults.SeeOther` eklemeleri ile T215'in 429/413 yolları ayrı yerlerde); belge çakışmaları (`MEMORY.md`, `DEFERRED_BACKLOG.md`, `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `T213_REPORT.md`) iki tarafın içeriği korunarak çözüldü; D numaraları çakışmadı (T215: D-144, D-145; T214: D-146; D-143 ✅ T215), aktif sayaç 74; kriter 1–8 ve KK1–KK8 testleri yukarıdaki sayılarla geçti (`RequestLimitsTests`, `AttemptLimitPatternTests`, `AttemptCounterTests`, tutarlılık ve kaynak taraması testleri); Release `-warnaserror` derleme 0 uyarı, `dotnet format --verify-no-changes` temiz; sık FAIL listesi (geniş istisna, çağrı biçimi, aynı sayıyı anan yerler, belgede olmayan sabit, yalnız TestServer) bu birleşmede yeni kod eklemediği için değişmedi, gerçek Kestrel testleri geçti |
| Birleştirme: sonrası tüm kriter ve KK yeniden denetlendi | ✓ | Düzeltme 1 sonrası: Unit 2378 geçti; Integration ilgili parçalar geçti (aşağıda); Release `-warnaserror` derleme 0 uyarı; `dotnet format --verify-no-changes` temiz |
| `09 §25` ilgili satır (yeni giriş/deneme yüzeyi §15.4 kapsamında) | ✓ | `AttemptSample` deneme yüzeyi sayacı kullanır; kaynak taraması `RequestOriginSourceScanTests` |
| Güvenlik taraması | ✓ | Mini kontrol aşağıda; ham `RemoteIpAddress` / başlık güveni / `AttemptRecord` yazımı kaynak taramasıyla tek yere kilitli |

Sık FAIL nedenleri taraması: (1) istisna darlığı: `RequestOriginSourceScanTests` dosya başına bir kural için istisna, ihlal örneği `Misuse.cs` + `DisposalMisuse.cs` (çok satırlı, adlı argüman, nitelikli ad, `?.`, desen); (2) çağrı biçimleri: ek/adlı argüman ve çok satır ihlal örneğinde; (3) sayı/etiket: `T215`, `L-7` anan yerler (`11`, `DEPLOY_RUNBOOK`, `FormResults` yorumu, `compose.yaml`, backlog) tarandı ve güncellendi; (4) belgede olmayan tavan sabiti yok (D-108 tavan konmadı); (5) hedefi T215 olan D-103, D-108, D-112, D-143 ve D-31 (d) kapatıldı ya da gerekçeyle ileri atıldı (birleştirmede D-143 `Docs/DEFERRED_BACKLOG.md`'de ✅, aktif sayaç 72); (6) kural değişince sayıyı anan yerler: `09 §8.2` m.3 çağıran sayısı üç → beş; `RateLimitLog` silindi, başka yerde anılmıyor.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (2. birleştirme, T214) | ✓ 2438 geçti, 0 kalan | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` | total 2438, failed 0 |
| Integration (2. birleştirme, T214) | ✓ 1649 geçti, 0 kalan (`--list-tests` keşfi 1649) | `dotnet test tests/Shopfolio.IntegrationTests -c Release --no-build --filter-namespace/--filter-class …` (parçalı, tek koşu) | Concurrency+Database+Disposal 79, FileSecrets+Files+Jobs+Logging+Outages+Pages+Security+SignedEndpoints 215, Model 893, Web 462. Not: `Model` tümü tek koşuda iki kez `SampleDatabaseFixture`/`EnumDatabaseTests` sınıf temizliğinde (veritabanı silme, `Npgsql` okuma zaman aşımı) yük kaynaklı düştü; bu sınıflar ve geri kalan `Model` ayrı koşuda 0 kalanla geçti (kod değişmedi) |
| Unit (1. birleştirme, T213) | ✓ 2375 geçti, 0 kalan (`origin/main` birleştirildikten sonra) | `dotnet test --project tests/Shopfolio.UnitTests` | total 2375, failed 0 |
| Integration | ✓ 1582 geçti, 0 kalan (birleştirmeden sonra) | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-namespace/--filter-class …` (parçalı) | Concurrency 26, Database 35, Disposal 18, FileSecrets 24, Files 33, Jobs 77, Logging 30, Model 893, Outages 8, Security 32, SignedEndpoints 11, Web 395 (RequestLimitsTests 28, Forgery/SecurityHeaders/VersionedForm/UnexpectedError/FieldValidation/AttemptLimitPattern 91, geri kalan 130 + 146) |
| Unit (Düzeltme 1) | ✓ 2378 geçti, 0 kalan | `Shopfolio.UnitTests.exe` | +3 test (`RequestBodyLimitMiddlewareTests`) |
| Integration (Düzeltme 1) | ✓ ilgili parçalar 0 kalan | `Shopfolio.IntegrationTests.exe -class/-namespace …` (parçalı) | RequestLimits+UnexpectedError+Forgery+SecurityHeaders 85 (gerçek Kestrel testleri 5 dahil); Security+SignedEndpoints+Logging 73; AttemptLimit/OneTime/Versioned/PageForm/List/FieldValidation/ModelBinding 154; öteki Web sınıfları 161 |
| Contract | — | — | bu task'ta yok |
| Biçim / derleme | ✓ | `dotnet format --verify-no-changes`; `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 2 (2026-11-06, head `1ba392d`); tur 1 ✗ FAIL (head `ea3cdc7`) |
| Bulgu sayısı | Tur 2: 0 (tur 1 B1 kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | Tur 2: 2 (K1 → D-147, K2 → D-148); tur 1: 2 (K1 → D-144, K2 → D-145) |

### Doğrulama tur 2 — ✓ PASS

Kapsam tam: kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması baştan denetlendi. **B1 kapandı:** doğrulayıcının kendi geçici gerçek Kestrel denemesinde (`UseKestrel()`, geri alındı) 1 baytlık chunk'larla 65536 bayt → 200, 65537 → 413 yalın sayfa; bildirim ucunda chunked 1000 bayt → 400 (imza), 65537 ve 204800 → gövdesiz 413; yapımın `OnARealServer_…` testleri de geçti. Düzeltmenin yan etkisi yok: Content-Length'li gövdede sunucu tavanı sınıf tavanı kalır; chunked gövdede okunan bellek sınıf tavanıyla sınırlı; bildirim ucu tamponlanmış gövdeyi okur; düz HTTP'de h2c sunulmadığı için (`HTTP_1_1_REQUIRED`) uzunluksuz HTTP/2 gövdesi ara katmanı atlayamaz; IPv4-eşlemeli vekil adresi (`::ffff:172.30.1.x`) güvenilir, eşlemeli dış adres güvenilmez (geçici birim denemesi). T214 birleştirmesi `NoticeCode`/`NoticeParameter` düzenini bozmadı (`FormResults.SeeOther(string, NoticeCode)` ve `VersionedFormPageTests` T214 hâlinde; `NoticeParameterTests` geçti). Unit 2438, Integration 1649 (gerçek PostgreSQL 18, parçalı), betik testleri 4/4, kilitli restore 0; dal CI 42211263821 yeşil. K1 → D-147: chunked okumada istemcinin bağlantıyı sıfırlaması ve bozuk chunk çerçevesi 500 + `UnexpectedError` (eşleşmeyen yolda da) — ters vekil üzerinden etkisi düşük. K2 → D-148: büyük sınıfın chunked gövdesi kimlik doğrulamasından önce belleğe alınır (bugün o sınıfta uç yok; T312, T525). `12` satırlarının `Durum`'u değişmedi (KP-72 satırları limit task'larının). Ayrıntı: doğrulama raporu (yardımcı durum dizini, `dogrulama-raporu-r2.md`).

### Doğrulama tur 1 — ✗ FAIL

**B1 (ORTA, S1; kriter 1, 2; KK1, KK2):** uzunluğu bildirilmeyen (`Transfer-Encoding: chunked`) gövdede `RequestBodyLimitMiddleware` sunucunun tavanını `limit`'e kurup okumaya başlar; gerçek Kestrel tavana varınca `BadHttpRequestException` (StatusCode 413) atar, ara katman yakalamaz, istisna yakalama katmanına düşer: yanıt 500 ve "Yazdıklarınız duruyor" mesajlı beklenmeyen hata sayfası, `UnexpectedError` satırı, hata sayacı artar. Tavanın altındaki gövde de reddedilir. Kanıt: uygulamanın ardışık düzeniyle `WebApplicationFactory.UseKestrel()` üzerinde geçici test (geri alındı): 1000 → 200; 65535, 65536, 65537, 204800 → 500; Content-Length'li 65537 → 413. TestServer sunucu tavanını uygulamadığı için `ABodyWithNoDeclaredLength_IsMeasuredAsItIsRead_AndRefusedPastTheCeiling` geçer. `Caddyfile`'da gövde tavanı yoktur; chunked gövde uygulamaya ulaşır. Düzeltme yönü: T203'ün `SignedEndpointHandler` kalıbı (`BadHttpRequestException` 413 → `RefuseAsync`) ya da tamponlama yolunda sunucu tavanını ayrı kurmak; tavanın tam sınırı ve üstü gerçek Kestrel testiyle sınanır.

Kriter 3–8 ve KK3–KK8 ✓. Unit 2375, Integration 1582 geçti; dal CI 42198823274 yeşil. K1: ters vekilde uç başına gövde tavanı yok → D-144. K2: `FormResults.AttemptLimitExceeded` red satırı yazmaz → D-145 (T376). `12` satırlarının `Durum`'u değişmedi (KP-72 satırları limit task'larının). Ayrıntı: doğrulama raporu (yardımcı durum dizini, `dogrulama-raporu-r1.md`).

### Düzeltme 1 (tur 1 FAIL sonrası)

**B1 kök neden:** gerçek Kestrel, `MaxRequestBodySize`'ı chunked gövdede çözülmüş bayta değil okunan ham bayta (chunk boyut satırları ve son chunk dahil) uygular; çerçeve bayt sayısı istemcinin chunk boyutuna bağlıdır, tavana eklenecek sabit bir pay yoktur. Bu yüzden tavan = sunucu tavanı kurulunca tavanın altındaki 65535 baytlık gövde bile `BadHttpRequestException(413)` ile düşüyordu (denemeyle doğrulandı: aynı gövde Content-Length'li 200).

**Çözüm** (`RequestBodyLimits.cs`):
- Content-Length'li gövde: sunucu tavanı sınıf tavanı kalır (sunucu tam bayt sayar); tavandaki gövde geçer, tavan+1 okunmadan 413.
- Uzunluğu bildirilmeyen gövde: sunucu tavanı kaldırılır (`null`), gövdeyi ara katman sınıf tavanı + 1 bayta kadar belleğe okur (`ReadWithinAsync`; bellek tavanı sınıf tavanıdır); tavan üstü → 413. Okuma sırasında sunucunun kendi 413 istisnası da (`BadHttpRequestException`, T203 `SignedEndpointHandler` kalıbı) aynı 413 yoluna girer.
- Ret yanıtı `Connection: close` taşır (HTTP/1.x); okunmamış gövdeyi sunucu boşaltmaya çalışmaz. Tavan yine tek yerde (`RequestBodyLimits`); red satırı `RejectionLog` + `PayloadTooLarge`, kod yanıtta yok, form yeniden gösterilmez (gerçek sunucu testinde doğrulanır).
- Bildirim ucu (`SignedEndpointHandler`) aynı yoldan geçer: ara katman gövdeyi önce tamponlar, işleyici bellek akışını okur.

**Testler:** `OnARealServer_ABodyBelowAtAndOverTheCeiling_…` (Theory; `WebApplicationFactory.UseKestrel()`, uygulamanın kendi ardışık düzeni; genel 64 KB ve uzun metin 1 MB sınıfı; chunked ve Content-Length; 1000, tavan−1, tavan → 200 ve gövde uzunluğu eşit; tavan+1, +4 MB → 413, sayfa yalın, `<form` yok, kod adı yok, `PayloadTooLarge` satırı; 40 MB'ta sunucu bağlantıyı kapatırsa istemcinin sıfırlanması kabul, uç çağrılmaz); `OnARealServer_ARefusedChunkedBody_…` (makine ucu gövdesiz 413, tavan−1 → 200); birim: sunucu tavanının kaldırılması, sunucunun kendi 413 istisnası, `Connection: close` (HTTP/2'de eklenmez). Düzeltme öncesi aynı test chunked'da 65535 → 500 ile kırmızıydı (denendi). TestServer testi kalır, tek kanıt değildir. Bu görevde 64 KB sınıfı ve bir büyük sınıf (1 MB) gerçek sunucuda sınanır; görsel (P-23+1 MB) ve parça (5 MB) sınıfları aynı koddan geçer, uçları sonraki task'larda doğar.

**K notları:** K1 (D-144, Caddy uç başına tavan) ve K2 (D-145, `AttemptLimitExceeded` red satırı, T376) yerinde ileri atılmış kalır: ikisi de T215 kapsamı dışı (dağıtım dosyası kararı; ilk gerçek çağıran T376); kayıtlara dokunulmadı. B1 çözümü D-144'ü etkilemez (uygulama tarafı tavanı artık Kestrel'den bağımsız tek başına uygular).

**Öz denetim:** kabul kriterleri 1–8 ve KK1–KK8 yukarıdaki tablolarda yeniden denetlendi; 1, 2, KK1, KK2 gerçek sunucu kanıtıyla güncellendi, öteki satırlara dokunulmadı ve ilgili testleri (Security, SignedEndpoints, Logging, Web) yeniden geçti. Sık FAIL nedenleri: davranış yalnız TestServer'da sınandı → gerçek Kestrel testi eklendi; geniş tarama istisnası, tarama çağrı biçimi, sayı anan öteki yerler (sunucu tavanı anlatımı: `RequestBodyLimitMiddleware` özet yorumu güncellendi, başka yerde anılmıyor — `grep` ile tarandı), belgede olmayan sabit (yeni sabit yok) bu düzeltmeye değmedi.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (genel sınır ayarları T-önceki kurulum ayarıdır: `PageRequestsPerMinute`, `FormSubmissionsPerMinute`)
- **Paketleme / dağıtım:** `deploy/compose.yaml` yalnız yorum (edge ağının iki alt ağı artık `ReverseProxyTrustConsistencyTests`'le sınanır)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T215-hiz-siniri-deneme-govde`
- **Commit:** dalın başı (PR sayfasında)
- **PR:** #409
- **CI:** *(yapım işçisi izlemez)*
- **Post-merge run'lar:** squash `5ebe163` (#409) — CI 42217406499 success, Publish 42217406517 success (T216 dalında dolduruldu, skill adım 15)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `ForwardedHeadersOptions.KnownIPNetworks` ve `System.Net.IPNetwork` .NET 10'da | `ForwardedHeadersSetup.Configure` derlenir; `ForwardedHeadersTests` | ✓ doğrulandı |
| 2 | Çatının `SlidingWindowRateLimiter`'ı `TimeProvider` almaz ve `TryReplenish` gerçek saate bağlıdır | sahte saatle çalışmadığı denemeyle görüldü; yerine `TimeDrivenSlidingWindowLimiter` | ✓ doğrulandı (`09 §15.4` m.1 notu) |
| 3 | `AttemptAcquire` ile zincirden izin alıp bırakmak yalnız form sayacını artırır | `RequestLimitsTests.ANotificationWithARightSignature…` | ✓ doğrulandı |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `SignatureFailureLog` sayılı pencerelerinin bellek tavanı konmadı | D-108 (hedef F2 gate) | `09 §3.5`'te sabit yok; taşma hâli `05 §4.5.2`'den ayrılır, sahibin kararı |
| 2 | `LoginRecord.Create` hâlâ `normalizedEmail` alıyor | D-64 (LoginRecord yarısı → T341) | giriş kaydını ilk yazan task |
| 3 | 429 ve 413 sayfaları yalın HTML (çerçeve yok) | D-135 notu (T537 + T540) | çerçeve henüz yok |

Kapatılan: D-103, D-112, D-143 (T213 doğrulama K2; birleştirmede); D-31 (d) uygulama yanı; D-64 `AttemptRecord` yarısı. Açık kalan hedef değişiklikleri yukarıda.

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: CI 42182118653 (T212 #407) success, Publish 42182118563 success, CI 42169337584 (T210 #406) success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Kararlar:**
  1. Genel sınırlayıcı çatının sınırlayıcısı değil, kendi dilim sayacıdır (`TimeDrivenSlidingWindowLimiter`; `RateLimitPartition.Get`); `09 §15.4` m.1 güncellendi.
  2. `TrustedNetwork` iki alt ağdır (IPv4 + IPv6, `TrustedNetworkV6`, liste `TrustedNetworks`); `09 §15.1` m.8 güncellendi.
  3. 429 ve 413 sayfaları yalın HTML; `09 §15.4` m.4 güncellendi, D-135'e not eklendi.
  4. Uzunluğu bildirilmeyen gövde tavan + 1 bayta kadar (en çok 5 MB) tamponlanır.
  5. Makine ucu ayrımı hız sınırında ve gövde ara katmanında yol tabanlıdır (`/sistem`, `/indir`, `/panel/yukleme`); `09 §8.1` m.3 "yoldan türetilmez" der, ama hız sınırı yönlendirmeden önce çalıştığı için uç meta verisi yoktur; `09 §15.4` m.4 yolu açıkça sayar. Gövde ara katmanı `MachineEndpointMetadata` VEYA yolu kullanır.
  6. `ProductConstants`'a P-31…P-37, P-46, P-49 eşik sabitleri eklendi (`02 §11.2` satırlarıyla `AttemptLimitsConsistencyTests` eşitler).
  7. Zincirde reddedilen POST sayfa iznini de tüketir (`CreateChained` geri vermez); test yorumunda belirtildi.
  8. `AttemptRecord` yazımı için D-64 uzlaştırması yol B: `09 §9.4` m.1 deneme kaydını `AttemptSubject` + `AttemptCounter`'a verir.
  9. D-108 tavanı konmadı (belgede yok); F2 gate'e ileri atıldı.
  10. (birleştirme) 429 (genel sınır) ve 413 reddi kayıt satırını `RejectionLog` üzerinden yazar (`09 §8.2` m.3 "her reddin kodu"); genel sınırın olayı `RateLimitExceeded` kalır (`09 §18.2` m.3 listesi), kod alanı eklendi. 413 öncesi satır hiç yazılmıyordu. `09 §8.2` m.3 çağıran sayısı üçten beşe güncellendi.
  11. (birleştirme) Deneme limitlerinin 429'u (`FormResults.AttemptLimitExceeded`) satır yazmaz; satırı, limite özgü kodla (`SignInLimitExceeded` vb.) limiti kullanan task'ın işleyicisi `RejectionLog` ile yazar (T341 ve sonrası); kod sızma taraması bu 429 için de var.
- **Mini güvenlik kontrolü:** sır yok; yetkilendirme etkisi yok (örnek sayfa panel kimliğiyle); girdi doğrulaması: gövde tavanı ve eksen metni sınırlı; yeni dış bağımlılık yok.
- **Önceki task kaydı:** T212 status Commit hücresi (`4694519`) ve rapor "Post-merge run'lar" satırı dolduruldu (skill adım 15); eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı. Birleştirmede T213 için aynısı yapıldı: status Commit hücresi `11a2c7d`, `T213_REPORT.md` post-merge satırı (CI 42195116854, Publish 42195116675); T213'ün T212 satırındaki iki run numarası ters yazılmıştı, düzeltildi (CI 42182118653, Publish 42182118563).
