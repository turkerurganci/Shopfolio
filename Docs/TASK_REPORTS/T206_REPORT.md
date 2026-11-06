# T206 — Adres yapısı, önekler ve sayfa–form işleyici kalıbı

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- Adres kuralları (`07 §2.1`): yol bölümü kuralı, beş kayıt öneki, kapalı ilk bölüm listesi, kayıt adresi `/{önek}/{Slug}`, iki sipariş kökünün yol kalıbı; sayfa şablonu listede olmayan ilk bölümle açılışta reddedilir (`AddressConvention`).
- Sayfa–form kalıbı (`07 §2.2`): `FormResults` (303 gönder–yönlendir–göster, 422 yeniden gösterim), `CsvFileResult` (200, ek olarak sunuş), `HandlerRequiredFilter` (işleyicisiz ya da bilinmeyen işleyicili POST 400).
- Statik dosyalar `/statik` altında, içerikten türeyen adla (`MapStaticAssets`); dışarıya açık API ve sürüm yok (`07 §2.10`).
- Taramalar: `PageSourceScan` (API/sürüm, görünüm bağlamı ve modül içi, `post` formunda işleyici), `PageHandlerRuleTests` (adsız `OnPost`, GET/POST dışı fiil, kontrolcü yok); hepsinin ayrı ihlal örneği var (`ViolationSamples/Pages`, `ViolationSamples/ModuleBoundary/PageHandlers.cs`).
- D-73 kapandı (derlenen `.cshtml` sınıfı sayfa kuralına girer); D-116 açıldı.
- Önceki task kayıtları (T201–T204, T220–T224): Commit hücreleri ve rapor "Post-merge run'lar" satırları dolduruldu (skill adım 15).
- `CONTEXT.md` Routing ve Forms satırları; `DEFERRED_BACKLOG.md`; `IMPLEMENTATION_STATUS.md` + eski blok `STATUS_CHANGELOG.md`'ye; `MEMORY.md` satırı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Routing/` (AddressRules, AddressTemplates, OrderRoutes, AddressConvention, AddressSetup), `Infrastructure/Forms/` (FormResults, CsvFileResult, HandlerRequiredFilter), `Program.cs` (`AddAddressRules`, `/statik` grubu).
- Örnek sayfa: `tests/Shopfolio.IntegrationTests/Pages/Panel/PatternForm.cshtml(.cs)`, `_ViewImports.cshtml` (yalnız test sunucusunun uygulama parçası; doğrulama tur 1 düzeltmesi — yapım taslağı yolu `src/Shopfolio.Web/Pages/` yazıyordu).
- Testler: `tests/Shopfolio.UnitTests/{Infrastructure,Consistency,Architecture}`, `tests/Shopfolio.IntegrationTests/Web/{AddressRoutingTests,PageFormPatternTests}.cs`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Yol bölümü küçük ASCII harf, rakam, tekli tire; kayıt adresi `/{önek}/{Slug}` | ✓ | `AddressRulesTests.IsValidSegment_*`, `RecordAddress_*`; `AddressRoutingTests.RecordAddress_OfEachPrefix_*`, `RecordAddress_WithoutPrefixOrWithAnotherShape_Is404` |
| 2 | Beş önek kapalı, dil öneki yok; ilk bölüm kapalı liste | ✓ | `AddressRulesTests.RecordPrefixes_AreTheFiveAndHaveNoLanguagePrefix`, `ClosedFirstSegments_*`; `AddressListConsistencyTests`; `AddressConventionTests` |
| 3 | Sipariş sayfasının iki kökü; yol kalıbı kuruldu, sayfalar T511'de | ✓ | `OrderRoutes.MemberRoot` / `AccessRoot`; `AddressRoutingTests.MemberRoot_*`, `AccessRoot_*`, `OrderScreens_SitUnderTheRootTheyWereReachedFrom` |
| 4 | Panel `/panel` altında; vitrinde iç anahtar yola girmez | ✓ | `AddressRulesTests.Check_OnAStorefrontTemplateWithAnInternalKey_Reports`, `Check_OnAPanelTemplateWithAnInternalKey_FindsNothing`, `Check_OnTheStorefrontTemplatesTheTasksAfterThisOneWillWrite_FindsNothing` |
| 5 | Harf büyüklüğüne ve sondaki eğik çizgiye duyarsız, yönlendirme yok; anahtar harf duyarlı | ✓ | `AddressRoutingTests.Path_WithAnotherLetterCaseOrATrailingSlash_*`, `AccessKey_WithAChangedLetterCase_IsAnotherValue`; `PageFormPatternTests.Get_WithAnotherLetterCaseOrATrailingSlash_*`, `Post_WithAnotherLetterCaseAndATrailingSlash_*` |
| 6 | Bir ekran bir Razor sayfası; her POST adlandırılmış işleyici, `handler` ile | ✓ | `PageFormPatternTests.Post_WithTheHandlerParameter_ReachesTheNamedHandlerOnly`, `Post_WithAnUnknownHandler_Is400AndWritesNothing`, `Post_WithoutAHandler_Is400AndWritesNothing`; `PageHandlerRuleTests`; `PageSourceScanTests.Scan_OfAPostFormInAnyWriting_*` |
| 7 | Yazan POST 303, dışa aktarma 200 CSV; yazmayan POST 422 ile yeniden gösterir; sebebi söyleyen ret 303 | ✓ | `PageFormPatternTests.Post_ThatWrites_Answers303ToTheGet_*`, `Post_ThatFailsValidation_DoesNotRedirect_Answers422_*`, `Post_ThatNamesItsReasonByARedirect_Answers303_*`, `Export_Answers200WithTheCsvAsAnAttachment_*`; `FormResultsTests` |
| 8 | GET kaydı değiştirmez | ✓ | `PageFormPatternTests.Get_ChangesNoRecord_NotEvenWithAHandlerOrTheFormValuesInTheQuery` |
| 9 | Değişen statik dosya yeni adres; API yok; sürüm yok | ✓ | `PageFormPatternTests.StaticFiles_AreUnderStatik_AndEachHasAnAddressDerivedFromItsContent`, `Application_HasNoApiAndNoVersionInTheAddressTheHeaderOrTheParameter`; `PageSourceScanTests`; `PageHandlerRuleTests.Application_HasNoControllerType` |

### Öz denetim (bitiş kapısı 2)

Her madde `git diff origin/main...HEAD` üzerinde yeniden denetlendi (test adları `grep` ile dosyada bulundu; testler bu turda koşuldu).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: bölümler küçük ASCII/rakam/tekli tire; adres `/{önek}/{Slug}` | ✓ | `AddressRulesTests.IsValidSegment_*`, `RecordAddress_*`; `AddressRoutingTests.RecordAddress_*` |
| KK2: listede olmayan ilk bölüm 404; kayıtlı her sayfa listede | ✓ | `PageFormPatternTests.Get_OnAFirstSegmentOutsideTheList_Is404`, `EveryMappedEndpoint_StartsWithAFirstSegmentOfTheClosedList`; `AddressConventionTests.Apply_OnAPageWithATemplateOutsideTheList_Throws*`; `AddressListConsistencyTests.FirstSegmentsOf07_EqualTheClosedListOfTheCode_InBothDirections` |
| KK3: iki kök ayrı; `/siparis/erisim` numara kalıbına düşmez | ✓ | `AddressRoutingTests.MemberRoot_OnAnythingThatIsNotAnOrderNumber_Is404_AndErisimIsNeverANumber`, `OrderScreen_UnderTheAccessRootWithoutAKey_IsNotTheMembersScreen`; `AddressListConsistencyTests.TheOrderRootsOf07_AreTheOnesOfTheCode` |
| KK4: vitrin kalıplarında iç anahtar yok | ✓ | `AddressRulesTests.Check_OnAStorefrontTemplateWithAnInternalKey_Reports` (ihlal örneği), `Check_OnTheStorefrontTemplatesTheTasksAfterThisOneWillWrite_FindsNothing`; `AddressConventionTests.Apply_*` açılışta yürür |
| KK5: büyük harf ve eğik çizgi aynı sayfa; harfi değişmiş anahtar eşleşmez | ✓ | `AddressRoutingTests.Path_WithAnotherLetterCaseOrATrailingSlash_*`, `AccessKey_WithAChangedLetterCase_IsAnotherValue`, `AccessRoot_KeepsTheLettersOfTheKeyAsTyped`; `PageFormPatternTests.Get_WithAnotherLetterCase*` |
| KK6: işleyici adı `handler` ile eşlenir | ✓ | `PageFormPatternTests.Post_WithTheHandlerParameter_ReachesTheNamedHandlerOnly`; `PageHandlerRuleTests.PageModels_InViolationSample_*` (ihlal örneği) |
| KK7: başarılı POST 303; hatalı POST 422 ve değerlerle | ✓ | `PageFormPatternTests.Post_ThatWrites_Answers303ToTheGet_*`, `Post_ThatFailsValidation_DoesNotRedirect_Answers422_AndShowsTheSentValues` |
| KK8: GET sonrası kayıtlar değişmez | ✓ | `PageFormPatternTests.Get_ChangesNoRecord_*` |
| KK9: statik dosya adresi içerikle değişir | ✓ | `PageFormPatternTests.StaticFiles_AreUnderStatik_AndEachHasAnAddressDerivedFromItsContent` |
| `09 §25`: dokümanla çelişki yok / BLOCKED bildirimi | ✓ | Tek çelişki `09 §16.1` ↔ `07 §2.1.2` (D-116); `07` izlendi, `09` düzeltmesi T537 |
| `09 §25`: diff yalnız görevin istediği; yeni sabit/tavan belgede | ✓ | Sabitler `07 §2.1`/`§2.7.1` adresleri ve durum kodlarıdır; ayar, süre ya da tavan eklenmedi (`.env.example` değişmez). Tek belge dışı seçim: işleyicisiz POST 400 (aşağıda ⚠) |
| `09 §25`: modül sınırları, mimari testler yeşil, ayrı ihlal örneği | ✓ | `ModuleBoundaryTests.PageAndEndpointTypes` (D-73) + `ViolationSamples/ModuleBoundary/{Sample,PageHandlers}.cs`; `PageSourceScanTests` ve `PageHandlerRuleTests` ihlal örnekli |
| `09 §25`: `CONTEXT.md` aynı PR'da güncel | ✓ | `.claude/CONTEXT.md` Routing ve Forms satırları |
| `09 §25`: yeni POST sahtecilik koruması | ✓ (not) | Örnek sayfada `[IgnoreAntiforgeryToken]` yalnız test için; koruma T209'un |
| Güvenlik taraması | ✓ | Gerçek uç yok (örnek sayfa test projesinde); yol deseni sonu `\z` (satır sonu atlatması, `%0A` testleri); `PageSourceScan` API/sürüm/işleyici taraması |
| Sık FAIL 1: bekçi istisnası geniş | ✓ | Tarama istisnası yok; `PageSourceScan` yalnız tam satır ve Razor yorumunu sayar (`Scan_OfAFullLineCommentAndOfARazorComment_IsQuiet`) |
| Sık FAIL 2: çağrı biçimi kaçırma / ihlal örneği | ✓ | `Scan_OfAPostFormInAnyWriting_*`, `Scan_OnTheApiSample_*`, `Scan_OnTheViewSample_*` ayrı örneklerle |
| Sık FAIL 3: sayı/etiket yinelenen yerler | ✓ | Önek ve ilk bölüm listesi `07` ile iki yönlü eşitlik testinde (`AddressListConsistencyTests`) |
| Sık FAIL 4: hedefi bu task olan backlog | ✓ | D-73 kapandı (`DEFERRED_BACKLOG.md`); T206'ya hedefli başka açık kayıt yok |
| Sık FAIL 5: belgede olmayan davranış | ✓ | İşleyicisiz POST 400 `07 §2.7.1` "404 ya da 400" içinde; gerekçe aşağıda, belge değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 1515 (Consistency 165 dahil) | `dotnet test tests/Shopfolio.UnitTests --no-build` | failed 0 |
| Integration | ✓ PageFormPattern 27, AddressRouting 40, `Web` ad alanı 153 | `dotnet test tests/Shopfolio.IntegrationTests --no-build -- --filter-class "*PageFormPatternTests"` vb. | failed 0; Security/SignedEndpoints/Outages 36 ve Logging+Liveness 30 önceki turda yeşil |
| Contract | — | — | Bu task'ta yok |
| Biçim/Derleme | ✓ | `dotnet build`, `dotnet format --verify-no-changes` | 0 uyarı, 0 hata, fark yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-06, head `686f019`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — K1 → D-117 (T212), K2 → D-118 (T207), K3 bu PR'da kapatıldı (D-73 kalın yazım, D-116 dosya adı ve "dosya adında parmak izi" iddiası, bu rapordaki örnek sayfa yolu) |

### Doğrulayıcının kanıtları (tur 1)

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Yol bölümü kuralı, `/{önek}/{Slug}` | ✓ | `AddressRules.SegmentPattern` `\A[a-z0-9]+(?:-[a-z0-9]+)*\z`; `AddressTemplates.CheckRecordAddress`; Unit + `AddressRoutingTests` yerelde yeşil |
| 2 | Beş önek, dil öneki yok, kapalı ilk bölüm | ✓ | `ClosedFirstSegments` `07 §2.1.2` ile satır satır okundu (5 + 12 + 7 + siparis + panel + 4 + 2); `AddressListConsistencyTests` iki yönlü; `AddressConvention` açılışta; `Get_OnAFirstSegmentOutsideTheList_Is404` (`/css/base.css`, `/tr/sepet` dahil) |
| 3 | İki sipariş kökü | ✓ | `OrderRoutes.MemberRoot` (yılla başlar) / `AccessRoot` (base64url, `\z`); `/siparis/erisim` 404, ekranlar kendi kökünde |
| 4 | `/panel`; vitrinde iç anahtar yok | ✓ | `CheckNoInternalKey` (`id`, `…Id`, `guid`/`int`/`long`) panel dışında; `EveryMappedEndpoint_*` gerçek uygulamanın bütün uçlarına uygular |
| 5 | Harf/eğik çizgi duyarsız, yönlendirme yok; anahtar duyarlı | ✓ | `LowercaseUrls` kapalı; `Path_WithAnotherLetterCase*`, `AccessRoot_KeepsTheLettersOfTheKeyAsTyped`, `GeneratedPath_*` |
| 6 | Adlandırılmış işleyici, `handler` | ✓ | `PageHandlerRuleTests` (ihlal örneği 5 ihlal), `PageSourceScan` işleyicisiz `post` formu; sonda: süzgeçsiz çatı işleyicisiz POST'a 200 verir, süzgeçle 400 |
| 7 | 303 / 422 / CSV 200 / ret 303 | ✓ | `FormResults.SeeOther` 303, `Redisplay` 422, `CsvFileResult` 200 + `attachment` + `no-store`; ilgili 5 entegrasyon testi |
| 8 | GET kayıt değiştirmez | ✓ | `Get_ChangesNoRecord_*` (`?handler=Add&Input.Name=…` dahil) |
| 9 | Statik dosya yeni adres; API ve sürüm yok | ✓ | `MapGroup("/statik").MapStaticAssets()`; parmak izli adres + ETag içerik SHA-256'sı; `PageSourceScan` API/sürüm, `Application_HasNoControllerType` |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (`IntegrationTests.csproj` yalnız Razor derleme ayarı)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T206-adres-sayfa-form-kalibi`
- **Commit:** `686f019` — doğrulanan PR başı (yapım taslağı `762ce86` yazıyordu; sonra PR numarası commit'i geldi)
- **PR:** #401
- **CI:** dal CI 42123791130 (head `686f019`) success — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E, CI Gate
- **Post-merge run'lar:** squash `494a34c` (#401) — CI 42128519929 success, Publish 42128519903 success (T207 dalında dolduruldu, skill adım 15)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `MapGroup("/statik").MapStaticAssets()` dosyaları parmak izli adla sunar | `StaticFiles_AreUnderStatik_*` gözlem testi | ✓ |
| 2 | Razor SDK test projesinde `.cshtml` derler | `RazorCompiler_PutsACompiledPageInTheGeneratedDocumentNamespace` | ✓ |
| 3 | Yol deseninde `$` yerine `\z` sondaki satır sonunu reddeder | `%0A` testleri (`AddressRoutingTests`) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `09 §16.1` örneği `~/js/common.js` + `asp-append-version` ile `07 §2.1.2` `/statik` çelişir; `07` izlendi | D-116 (T537) | Belge düzeltmesi karar kaydı ister |
| 2 | Parmak izli statik adres test sunucusunda `Cache-Control: no-cache` döner; üretimde ortam ayarı | D-116 notu | Önbellek ayarı dağıtım task'ında |
| 3 | Yeni POST sahtecilik koruması | T209 | Örnek sayfada yalnız test için kapalı |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** 42113408503 CI, 42113408500 Publish, 42097338902 CI hepsi success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ 1 (D-116):** `09 §16.1` ↔ `07 §2.1.2` statik yol çelişkisi; `07` izlendi.
- **⚠ 2:** İşleyicisiz ya da bilinmeyen işleyicili POST 400. Çatı tek başına sayfayı 200 ile yeniden gösterirdi; `07 §2.7.1` "404 ya da 400" der, 400 seçildi (`HandlerRequiredFilter`); belge değişmedi, gerekçe bu satırdır.
- **⚠ 3:** Statik dosya `Cache-Control` notu: parmak izli adres test sunucusunda `no-cache` (D-116 notu).
- **⚠ 4:** Yerel makinede `localhost` IPv6 gecikmesi 2 sn canlılık zaman aşımını aşar (`LoggingFilesTests`, `LivenessEndpointTests`); `127.0.0.1` ile yeşil, CI etkilenmez.
- **⚠ 5:** Yol deseni sonu `$` yerine `\z` (sonda satır sonu atlatması).
