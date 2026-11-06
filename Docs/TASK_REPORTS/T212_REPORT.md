# T212 — Hata gösterimi ve HTTP durumları

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Tek yakalama katmanı** (`Infrastructure/Errors/`): `UnexpectedErrorLayer.Resolve` istisnayı dört hâlden birine çevirir (sayfa düzeyi, işlem düzeyi, sonucu belirsiz, makine ucu), `UnexpectedError` satırını (kod, durum, işlem) yazar ve sayacı artırır; istemcinin vazgeçmesi (`RequestAborted` + `OperationCanceledException`) satır ve sayaç üretmez. İki giriş: `UnexpectedErrorPageFilter` (Order -300, POST'ta formun değerleriyle yeniden gösterim) ve `UnexpectedErrorMiddleware` (yedek, çizim sırasında düşen hata). Katman hiçbir işlemi kesinleştirmez.
- **Sınıflandırma** saf işlev `ErrorClassifier.Classify(yöntem, makine ucu, işaretli)`; makine ucu işareti uç meta verisi `MachineEndpointMetadata` (`MapSignedEndpoint` ve canlılık ucu yazar), sonucu belirsiz işlem `[OutcomeUncertain("UC-nn")]` + `IOutcomeTarget` (hedef ekran).
- **Yanıtlar**: sayfa düzeyi 500 yalın hata sayfası, işlem düzeyi 500 değerlerle yeniden gösterim + kalıcı mesaj, sonucu belirsiz 303 (`FormResults.WriteSeeOther`, yerel yol denetimli), makine ucu gövdesiz 500; hiçbirinde kod, istek kimliği, çerez yok.
- **D-117**: `HandlerRequiredFilter` yeniden yazıldı — GET/POST dışı yöntem ve POST işleyicisi olmayan sayfaya POST 405 + `Allow`; POST işleyicili sayfanın işleyicisiz POST'u 400.
- **D-130**: örnek `VersionedOrder.CustomerCancel` açık adet tutmayınca 303 → `?notice=QuantityExceedsOpen`.
- **D-25**: `.editorconfig` CA1031 = error; dört birim testi yeri gerekçeli bastırma aldı; `JobSourceScan` yeni iki dosyayı yer olarak bilir.
- **D-115**: `Start_WithoutConnectionString_…` testi iş kuyruğu çalıştırıcısız açılır.
- **Dokümanlar**: `09 §15.1` m.6 (D-113 kararı), `09 §8.1` m.3 (yazımın kararı) ve m.6 (işaret); `CONTEXT.md`; backlog; durum; hafıza.
- Önceki task kaydı: T210 status Commit hücresi (`539d9cd`) ve rapor "Post-merge run'lar" satırı dolduruldu (skill adım 15); eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı.
- Bu turda önceki işçinin işi devralındı; testler ilk kez gerçek PostgreSQL ile koşuldu, bulunan üç hata düzeltildi: test `Allow` başlığını yanlış yerden okuyordu (içerik başlığı), günlük dosyası paylaşımsız açılıyordu, HTML kodlanmış Türkçe metin kodlanmamış aranıyordu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Errors/{ErrorMarks,UnexpectedErrorLayer,UnexpectedErrorSetup}.cs`, `Forms/{FormResults,HandlerRequiredFilter}.cs`, `Routing/AddressSetup.cs`, `SignedEndpoints/SignedEndpointRegistration.cs`, `Endpoints/LivenessEndpoint.cs`, `Program.cs`, `.editorconfig`.
- Testler: `IntegrationTests/Web/UnexpectedErrorLayerTests`, `Pages/{ErrorSamples.cs,Storefront/ErrorSample.cshtml,Panel/ErrorSample.cshtml,Storefront/ReadOnlySample.*,Panel/VersionedOrder.cshtml.cs}`, `SignedEndpoints/SignedEndpointDatabaseTests`, `Web/{VersionedFormPageTests,LivenessEndpointTests}`; `UnitTests/Infrastructure/UnexpectedErrorLayerTests`, `UnitTests/Consistency/OutcomeUncertainConsistencyTests`, `UnitTests/Architecture/{JobSourceScan,JobSourceScanTests}`, dört bastırma yeri (`CookieRulesTests`, `ApplicationLogFormatterTests`, `CommonLogFieldsTests`, `ExceptionMaskerTests`).
- Dokümanlar: `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T210_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Durum kodu sonucu söyler, kod gösterilmez | ✓ | `UnexpectedErrorLayerTests.AssertNoCode` (kapalı kod listesi gövde + başlıkta yok): `Get_WhenTheHandlerFails_…`, `Post_OfANamedRefusal_Is422_…` |
| 2 | GET 200, yazan POST 303, oturumlu sayfanın girişe yönlendirmesi 302 | ✓ | `Statuses_GetIs200_WritingPostIs303_AClosedPageWithoutASessionIs302_…_KK2` |
| 3 | Doğrulama/iş kuralı reddi 422 + değerler | ✓ | `Post_OfANamedRefusal_Is422_WithTheSentValues_…_KK3_KK9("Rule")` |
| 4 | Sürüm çakışması / gösterilen değer 409; iki istisna 303 | ✓ | `VersionedFormPageTests` (409 testleri); müşteri açık adedi 303: `VersionedFormPageTests` müşteri testi + `VersionedOrder.CustomerCancel` (D-130); gerçek uçlar T512, T513, T535 |
| 5 | Deneme limiti/hız sınırı 429, 413, sahtecilik 400 | ✓ (başka task'ta) | 429/413 T215, sahtecilik 400 T209; 413 imzalı uçta `SignedEndpointDatabaseTests.BodyAboveTheLimit_Gets413_…` |
| 6 | Makine ucu ekran göstermez: 200/400/500, canlılık 200/503 gövdesiz `no-store` | ✓ | `LivenessEndpointTests.Get_WithHealthyDatabase_Returns200WithEmptyBody` (+ `no-store` satır 42), `…Returns503WithEmptyBody`; `SignedEndpointDatabaseTests` (200 `OK` düz metin, 400 gövdesiz, 500 gövdesiz); ortak 429 makine ucu T215 |
| 7 | Beklenmeyen hata tek katmanda üç hâlden birine düşer | ✓ | `Get_…_KK1_KK7`, `Get_WhenThePageFailsWhileItIsDrawn_…_KK7`, `Post_WhenTheOperationFails_…_KK7(panel)`, `Classify_FollowsTheTableOfTheGuideline_KK7`; birim: `UnitTests/Infrastructure/UnexpectedErrorLayerTests` |
| 8 | Belirsiz sonuç 303; adı konmuş hata kendi koduyla | ✓ | `Post_OfAMarkedOperation_AnswersTheResultScreen303_WhereverTheErrorFalls_KK8(mode, committed)`; `Post_OfAMarkedOperation_WhoseNamedRefusalIs422_StaysA422_KK8` |
| 9 | Silinmiş yüklemeyle kayıt 422 | ✓ | `Post_OfANamedRefusal_Is422_…_KK3_KK9("Upload")` |

### Öz denetim (bitiş kapısı 2)

Kriter 1–9 ve KK1–KK9 bu turda baştan, kendi diff'inde yeniden denetlendi (yalnız kalanlar değil); önceki işçinin işi devralındı. Sık FAIL nedenleri dosyası okundu.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 gövde ve başlıkta hata kodu yok | ✓ | `UnexpectedErrorLayerTests.AssertNoCode` her hâlde (500, 422, 303, 405) çağrılır |
| KK2 üç durum kodu | ✓ | `Statuses_…_KK2` (200, 303, 302) |
| KK3 iş kuralı reddi 422, değerler korunur | ✓ | `Post_OfANamedRefusal_…("Rule")` gövdede `kept-value` |
| KK4 çakışma 409; 303 istisnaları T512/513/535 | ✓ | `VersionedFormPageTests` 409; 303 örneği D-130 |
| KK5 429 | ✓ (T215) | bu task'ta üretilmez |
| KK6 canlılık/durum gövdesiz `no-store`; bildirim 200 düz metin `OK` | ✓ | `LivenessEndpointTests`; `SignedEndpointDatabaseTests` (`Assert.Equal("OK", …)`); durum ucu T314 (D-139) |
| KK7 kaçan istisna tek katmanda | ✓ | `UnexpectedErrorLayerTests` 25 test; kaynak taraması `JobSourceScanTests.ApplicationSource_…` (geniş `catch` yalnız yerlerde, yeni ihlal örneği `Scan_InTheCatchingLayersFiles_…`) |
| KK8 belirsiz sonuç 303; işaretli işlemin 422'si 422 | ✓ | `…Post_OfAMarkedOperation_*_KK8` |
| KK9 silinmiş görselle kayıt 422 | ✓ | `…("Upload")` |
| `09 §8.1` m.9 testleri: tablo (birim) | ✓ | `UnitTests/Infrastructure/UnexpectedErrorLayerTests.Classify_FollowsTheTableOfTheGuideline` (10 satır), `TheCodeOfEachState_…` |
| m.9: hâl başına yanıt, işaretlide kesinleştikten sonra düşen hata | ✓ | `…KK8(mode, committed)` (committed: true/false) |
| m.9: kayıt satırı alanları, satırda form değeri ve çerez yok | ✓ | `Post_WhenTheOperationFails_…` (`ReadLog` içinde `kept-value` yok); `Get_…` (`t212 failure` ileti yok) |
| m.9: hata sayfası düşerse en yalın 500 | ✓ | `UnexpectedErrorLayerTests.Write_WhenTheErrorPageCannotBeWritten_FallsBackToThePlainest500_AndThrowsNothing` (birim; yazamayan akış) |
| m.9: istemci iptalinde satır yok | ✓ | `Resolve_WhenTheClientWentAway_IsNoError_…` (birim) |
| m.9: işaretli işleyici kümesi `07 §6.4.2`'ye eşit | ✓ | `OutcomeUncertainConsistencyTests` (iki yönlü; geçiş listesi D-137) |
| m.9: CA1031 ve tarama altı yerin dışında kırmızı (süzgeçli dahil) | ✓ | build 4 test yerinde CA1031 hatası verdi (kırmızı kanıt); `JobSourceScanTests.Scan_OnViolationSample_…` (süzgeçli, türsüz, `catch when`) + `…CatchingLayersFiles_…` |
| `09 §8.2` m.4/m.5 kod gösterilmez | ✓ | `AssertNoCode`; kayıt kodu `UnexpectedErrorCodes` (T213'e kadar sabit, D-136) |
| Sık FAIL 1: istisna geniş | ✓ | geniş `catch` iki dosyada, dosya yoluyla tam eşleşme; aynı adlı başka dosya yer değil (test) |
| Sık FAIL 2: tarama çağrı biçimi | ✓ | mevcut `JobSourceScan` biçimleri: `catch (Exception)`, `catch (System.Exception`, süzgeçli, türsüz, `catch when`; yönlendirme taraması: katmanın 303'ü `FormResults.WriteSeeOther`'a taşındı (`RedirectSourceScan` iki dosya kuralı bozulmadı) |
| Sık FAIL 3: aynı sayıyı/adı anan yerler | ✓ | `D-142/D-143` anıları `D-135/D-136` yapıldı; `09 §8.1` "bilgim tarihli" işareti; `CONTEXT.md`; backlog sayacı |
| Sık FAIL 4: geçici çözüm backlog'suz | ✓ | D-135…D-139 |
| Sık FAIL 5: kabul kriteri kısmen | ✓ | 1–9 karşılandı; 5. ve 6. kriterin başka task'a ait payları plan metninde zaten T215/T203/T314'te |

CI düzeltmesi: Lint `dotnet format --verify-no-changes` iki hata verdi (`Program.cs` using sırası, `ErrorSamples.cs` IDE0008); yalnız biçim değişti (using sırası, `var` yerine `IDbContextTransaction`), davranış aynı. Yerelde `restore --locked-mode`, `format --verify-no-changes` ve Consistency testleri (173/173) çıkış 0; `build -warnaserror` 0 uyarı; Unit 2221/2221. Yukarıdaki kanıt adları grep ile teyit edildi.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 2221/2221 | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 0 hata |
| Integration | ✓ yerelde gerçek PostgreSQL 18 (`t212-pg`): `UnexpectedErrorLayerTests` 25/25; ad alanları `Web` 363, `Pages*`+`SignedEndpoints`+`Security`+`Logging` 59, `Jobs`+`Outages`+`Files`+`Disposal`+`Concurrency` 162 | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-namespace …` | 0 hata. `Database` ve `Model` ad alanları yerelde koşulmadı (170 sn sınırı; bu task'ın dokunmadığı alan) — dal CI'ında |
| Contract | — | — | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-06, head `ec13690`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — K1 → D-138 notu, K2 → D-135 hedefi düzeltildi, K3 → D-140 (T213) |

**Validator kanıtı (tur 1, 2026-11-06, `ec13690`; dal CI'ı run 42172712753 success):** kapılar: working tree temiz, `core.hooksPath` `scripts/git-hooks`; ana dal son üç run (42169337584, 42169337501, 42162567568) success; hafızada T212 satırı var. Kriter 1–9 ve KK1–KK9 ✓ — kod okundu, testler doğrulama anında yeniden koşuldu: `dotnet build -warnaserror` 0/0, `dotnet format --verify-no-changes` temiz, `LivenessProbe`'un CA1031 bastırması geçici kaldırılınca `error CA1031` (geri alındı), Unit 2221/2221, betik testleri 4/4, Integration (PostgreSQL 18, yedi parçaya bölünerek) 1536/1536, kırmızı yok. Tek yakalama katmanı: sayfa süzgeci (Order -300, en dış) + ara katman (`UseCookieHeaderFilter`'dan sonra, `UseRouting`'den önce; güvenlik başlıkları `OnStarting` ile hata yanıtına da yazılır). Katman dışında kalan tek yol yayın öncesi vitrinde çerez süzgecinin veritabanı sorusu düşerse çatının çıplak 500'üdür (D-113 seçenek 1, D-138 izliyor — K1). Doğrulayıcı geçici testle (11/11 yeşil, silindi) 422 `Upload`/`Rule` gövde ve başlığında red kodunun olmadığını, 405/400'de `Location` olmadığını, PUT/DELETE/PATCH'in vitrin ve panelde 405, bilinmeyen handler'ın 400 olduğunu gösterdi. Güvenlik temiz (sır yok, 303 hedefi `IsLocalPath`, hata sayfası yolu HtmlEncode, istisna iletisi kayda yazılmıyor, yeni bağımlılık yok).

**Task satırları (`12 §7.5`):** T212 KP eşlemesi taşımaz (Ortak altyapı); satırlar kaynak bölümle (`07 §2.7`, §6.4, `09 §8.1`, `05 §10.3`) grep'lendi.

| Satır | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` |
|---|---|---|---|---|
| VAL-A424 | ORTA | yok | BEKLEMEDE | PASS (GET'lik sayfaya POST 405 + Allow) |
| VAL-A425 | ORTA | yok | BEKLEMEDE | PASS (PUT/DELETE/PATCH vitrin ve panel 405) |
| VAL-A429 | ORTA | yok | BEKLEMEDE | PASS (bilinmeyen handler 400) |
| VAL-A430 | ORTA | yok | BEKLEMEDE | PASS (çok işleyicili sayfada handler'sız POST 400) |
| VAL-A433 | ORTA | yok | BEKLEMEDE | PASS (sabitlenen değer 400) |
| VAL-A431 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (sipariş onayı sayfası) |
| VAL-A432 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (T312, T314) |
| VAL-A596 | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE (sipariş onayı task'ı) |
| VAL-A959–A963 | — | yok | BEKLEMEDE | BEKLEMEDE (T215, D-135) |
| VAL-A582, A689 | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE (T512/T513/T535) |

**K-notları:** K1 → D-138'e not: `09 §15.1` m.6'daki "Karar (D-113, T212)" `05 §10.3.6`/§9.4.8 davranışına istisnadır; boşluk kapanmazsa istisna `05 §10.3.6`'ya da yazılır, F2 gate hedefi yeniden kurar. K2 → D-135 hedefi "T537 + T540; en geç F5 gate" olarak düzeltildi (eski F2 üst sınırı çelişikti). K3 → D-140: katmanın hata yanıtlarındaki `no-store` düz metinle yazılır, `09 §15.2` m.5'te anılmaz ve süzgeç yolundaki işlem düzeyi 500 bu başlığı taşımaz (hedef T213).

**Yapım raporu karşılaştırması:** verdict uyumlu; fark 1: öz denetim `12` satırlarını anmıyor (doğrulayıcı beşini PASS yaptı); fark 2: "AssertNoCode her hâlde (500, 422, 303, 405)" — 405/400 testi `AssertNoCode` çağırmaz, `AssertNoCode` 422'nin red kodlarını aramaz (doğrulayıcı geçici testle gösterdi); fark 3: yapım Database ve Model ad alanlarını yerelde koşmadı, doğrulayıcı koştu, yeşil; fark 4: D-135/D-138 hedef–üst sınır çelişkisi (K1, K2) anılmamış. Öz denetim tablosu her madde için kanıt taşıyor.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T212-hata-gosterimi-http`
- **Commit:** `ec13690` (PR #407 head, doğrulama tur 1)
- **PR:** #407
- **CI:** dal run 42172712753 success (validator, 2026-11-06)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | CI'ın PostgreSQL sürümü yerelle aynı | `.github/workflows/ci.yml` satır 102 `postgres:18`; yerelde `postgres:18` imajı | ✓ |
| 2 | xUnit v3 / Microsoft.Testing.Platform süzgeç anahtarları | `--filter-class`, `--filter-namespace` koşuldu | ✓ |
| 3 | `HttpClient` `Allow` başlığını içerik başlıklarına koyar | düzeltilen test geçti (`response.Content.Headers.Allow`) | ✓ |
| 4 | `WebUtility.HtmlEncode` ASCII dışını sayısal varlığa çevirir | test `HtmlDecode` ile geçti | ✓ |
| Diğer | Plan tier, kota, hız sınırı, API anahtarı, paket sürümü varsayımı yok | — | Dış varsayım: yok |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Hata sayfası vitrin/panel çerçevesiz | D-135 | çerçeve sayfaları T537 ve panel çerçevesinde |
| 2 | Katmanın üç kodu metin sabiti | D-136 | `ErrorCode` kayıt defteri T213 |
| 3 | İşaretli işleyici eşitliği geçiş listesiyle | D-137 | işaretli sayfalar sonraki task'larda |
| 4 | Çerez yayın sorusunun hatası katmana ulaşmaz | D-138 | `09 §15.1` m.6 sırası (karar) |
| 5 | Durum ucu ve parçalı yükleme makine ucu işareti | D-139 | uçlar T312, T314'te |

## Notlar

- **Working tree (Adım -1):** temiz (`git status --short` boş); `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks`
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: 42169337584 success, 42169337501 success, 42162567568 success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Dal adı:** `task/T212-hata-gosterimi-http` başka worktree'de açık olduğundan yerelde `t212-work` adıyla çalışıldı, aynı uzak dala push edildi.
- **⚠ kararlar** PR açıklamasında numaralıdır.
