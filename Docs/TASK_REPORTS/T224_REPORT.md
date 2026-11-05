# T224 — Çerez politikası

**Faz:** F2 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Çerez kuralları (`Infrastructure/Security/`):** `CookieNames` (yedi adın kapalı listesi, hepsi `__Host-`), `CookieDefaults` (Secure, HttpOnly, SameSite=Lax, yol `/`, Domain yok — niteliklerin tek yeri; `Apply` çatının kendi yapıcısını ezer, `Create` yalnız listedeki adı kabul eder), `BrowserCookies` (çerezin yazıldığı, okunduğu ve silindiği tek yer; çatının parçalı çerez yöneticisi; ödeme adımı ve Google akışı çerezi veri koruma amacıyla şifreli ve imzalı; misafir sepeti 400 gün, her ziyarette yeniden yazılır; Google akışı çerezi 10 dk, bir kez kullanılır ve aynı yanıtta silinir).
- **Başlık süzgeci:** `CookieHeaderFilterMiddleware` + `CookiePolicyGate` — politika ilk kez yayına alınana kadar vitrin yanıtlarından `Set-Cookie` başlığını `OnStarting` ile çıkarır, kim yazmış olursa olsun; panel (`/panel`) ve makine uçları (`/sistem`) kapsam dışı. Kapı bir kez açılınca kalıcı açık kalır. Yayın sorusu altyapıda `ICookiePolicyPublication` arayüzüdür, `Settings` modülü `CookiePolicyPublication` ile yanıtlar.
- **Kurulum:** `CookieSetup.AddCookieRules` (form güvenliği çerezinin adı, nitelikleri ve başlık adı) ve `UseCookieHeaderFilter` (`UseRouting` öncesi); `Program.cs`.
- **Taramalar ve eşitlik (bu turda):** `CookieSourceScan` (+ ayrı ihlal örneği `ViolationSamples/Cookies/CookieMisuse.cs`), `ThirdPartyContentScan` (+ ayrı ihlal örnekleri `ViolationSamples/ThirdParty/Page.html`, `Packages.props`), `CookieNamesConsistencyTests` (`09 §5.3` m.5 ↔ `CookieNames.All`, iki yönlü, `[Trait("Consistency","true")]`), çerez politikası taslağı yer tutucu testi.
- **Bu turda ayrıca:** `CookieRulesTests`'teki başlık süzgeci testi yalın bağlamın `OnStarting` çağırmamasından kırıktı; test sunucunun yaptığı gibi geri çağırmaları çalıştıran küçük bir yanıt özelliği kullanır. `.claude/CONTEXT.md` klasör haritasına çerez satırı eklendi. Backlog D-112 (ters vekil başlıkları → T215).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/` (`CookieNames`, `CookieDefaults`, `BrowserCookies`, `CookieHeaderFilterMiddleware`, `CookiePolicyGate`, `ICookiePolicyPublication`, `CookieSetup`), `Modules/Settings/CookiePolicyPublication.cs` ve `SettingsModule.cs` (kayıt), `Program.cs`.
- Testler: `UnitTests/Infrastructure/CookieRulesTests.cs`, `UnitTests/Architecture/CookieSourceScan*.cs`, `ThirdPartyContentScan*.cs`, `ViolationSamples/Cookies/`, `ViolationSamples/ThirdParty/`, `UnitTests/Consistency/CookieNamesConsistencyTests.cs`; `IntegrationTests/Web/CookieProbe.cs`, `CookiePolicyTests.cs`.
- Belgeler: `Docs/DEFERRED_BACKLOG.md` (D-112), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Bütün çerezler birinci taraf, Secure, HttpOnly, SameSite=Lax | ✓ | `CookieDefaults.Apply`; `CookieRulesTests.Create_ForEachName_GivesSecureHttpOnlyLaxRootPathAndNoDomain_EvenOverPlainHttp` (7 ad); `CookiePolicyTests.AllCookies_AfterThePublication_CarryTheThreeAttributes…` gerçek `Set-Cookie` başlığında |
| 2 | `__Host-` önekli, alana kilitli; koşullar tek yazım yerinde | ✓ | Gerçek başlıkta `Path=/`, `Domain` yok (aynı entegrasyon testi); `Apply_OnABuilderTheFrameworkOwns_OverwritesWhatWouldBreakThePrefix`; `CookieSourceScan` (yazım yalnız `BrowserCookies`, nitelik yalnız `CookieDefaults`, `__Host-` dizgisi yalnız `CookieNames`) |
| 3 | Envanter kapalı: yedi çerez | ✓ | `CookieNames.All` (7); `CookieNamesConsistencyTests` iki yönlü; `ANameOutsideTheList_IsAProgrammingError_…`; `Violations_OnACookieOutsideTheInventoryOrWithoutAnAttribute_AreFound` |
| 4 | Misafir sepeti 400 gün, her ziyarette yenilenir | ✓ | `GuestCartCookie_Lives400DaysAndIsWrittenAgainAtEveryVisit` (Max-Age 400 gün, ikinci ziyarette yeniden yazım); `BrowserCookies.GuestCartLifetime` = `DisposalTimings.GuestCartIdleDays` |
| 5 | Ödeme adımı çerezi oturumluk, şifreli, imzalı; onayda silinir | ✓ | `CheckoutCookie_IsEncryptedLastsTheSessionIsReadBackAndIsDeletedOnConfirmation`, `…ThatIsSplitIntoChunks_IsReadBackAndEveryChunkIsDeleted`, `Checkout_WrittenWithTheCookieWriter_IsNotReadableAsPlainTextAndRoundTrips`. Tek silme yolu `BrowserCookies.DeleteCheckout`; onay adımındaki çağrı T509 `PlaceOrder`'ındadır, boşalmadaki T508'dedir (kriterin kendi metni) |
| 6 | Politika yayına alınana kadar vitrin çerez yazmaz; ara katman başlığı çıkarır | ✓ | `StorefrontResponse_BeforeThePublication_CarriesNoSetCookie…`, `…WhileOnlyTheDraftExists…`, `…OpensAtTheFirstRequestAfterThePublication_AndStaysOpen`; birim: `HeaderFilter_BeforeThePublication_RemovesWhatAnyMiddlewareWrote…`, `Gate_OnceItHasSeenThePublication_StaysOpen…` |
| 7 | Süzgeç yalnız vitrin yanıtlarına | ✓ | `HeaderFilter_AppliesToTheStorefrontOnly` (`/panel`, `/sistem` dışarıda); panel yanıtı süzülmez (entegrasyon testi); `MachineEndpoint_BeforeThePublication_StillAnswers_WithoutTheDatabase` |
| 8 | Google akış çerezi yolu `/`, `__Host-` onu kapsar | ✓ | `ExternalLoginFlowCookie_HasTheRootPathLastsTenMinutesAndIsUsedOnce` (gerçek `Set-Cookie`: `Path=/`, Max-Age 600) |
| 9 | Analitik, üçüncü taraf çerez, piksel, reklam, onay bandı, gömme yok; tek istisna ödeme çerçevesi; taslakta yer tutucu | ✓ | `ThirdPartyContentScanTests.ApplicationSource_HasNoAnalyticsTagBannerEmbedOrForeignResource` (wwwroot `common.js`, `base.css`, kaynak `.cs`), `PackageFiles_NameNoAnalyticsAdvertisingOrTagPackage` (props, csproj, kilit), `PaymentFramePages_IsClosedAndEmptyUntilThePaymentPageTaskAddsItsPage`, `CookiePolicyDraft_CarriesThePlaceholderForThePaymentFrameCookiesAndTheSevenCookies`; `CookieSourceScan` `CheckConsentNeeded`/`UseCookiePolicy`/`CookiePolicyOptions` yasağı |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 her çerezde üç nitelik | ✓ | Kriter 1 |
| KK2 `__Host-` adı; gerçek başlıkta `Path=/`, Domain yok | ✓ | Kriter 2 |
| KK3 envanter dışı ad testi kırar | ✓ | Kriter 3; ayrıca `CookieSourceScan` ihlal örneği ham `Cookies.Append("sid"…)` ve `"__Host-cart"` dizgisini bulur |
| KK4 sepet ömrü ve yenilenmesi | ✓ | Kriter 4 |
| KK5 ödeme çerezi şifreli; onayda silinir | ✓ | Kriter 5 (çağrı yeri T509) |
| KK6 yayında değilken `Set-Cookie` yok | ✓ | Kriter 6 |
| KK7 panel süzülmez | ✓ | Kriter 7 |
| KK8 akış çerezi yolu `/` | ✓ | Kriter 8 |
| KK9 analitik/izleme/reklam/onay bandı/gömme yok; yalnız ödeme çerçevesi; taslakta yer tutucu | ✓ | Kriter 9; istisna tek sayfa ve yalnız çerçeve öğesi genişliğinde: `Scan_OnAPaymentFramePage_AllowsTheFrameElementAndNothingElse` |
| `09 §25` ilgili satırlar | ✓ | PR açıklamasında |
| Güvenlik taraması | ✓ | Çerez değerinde açık metin yok (`ReadableAsPlainText` testi); çerezin nitelikleri tek yerde; yeni paket yok; dış kaynak yok |
| Sık FAIL: kabul kriteri kısmen | ✓ | 9 kriter ✓; onaydaki silme çağrısı sipariş verme task'ının (T509) işidir — T224 silme yolunu ve testini verir |
| Sık FAIL: aynı sayıyı anan yerler | ✓ | Yedi ad: `CookieNames.All`, `09 §5.3` m.5, taslak yer tutucu testi (7 madde), CONTEXT; eşitlik testi ikisini bağlar |
| Sık FAIL: tarama istisnası geniş / ayrı ihlal örneği yok | ✓ | `CookieSourceScan`: üç dosya, her biri tek kural için; `ThirdPartyContentScan`: çerçeve istisnası kapalı boş liste; üç ayrı ihlal örneği ve istisna testleri |
| Sık FAIL: karar kaydı olmadan belgeden sapma | ✓ | `09 §15.1` m.6 uygulandı; sapma yok. Süzgeç `/sistem`'i de dışarıda bırakır (05 §3.5.5 "vitrin" yorumu, `07 §4.33.1` canlılık ucu) — PR ⚠ 1 |
| Sık FAIL: modül arayüz yöntemi kendi işlemini açıyor | ✓ | `CookiePolicyPublication` tek okuma yapar, işlem açmaz; çerez kodu veritabanına dokunmaz |
| Sık FAIL benzeri: biçim denetimi | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` çıkış 0 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 1330 geçti, 0 kaldı |
| Integration | ✓ | aynı komut, `Shopfolio.IntegrationTests`, `--filter-namespace` parçaları | Web 86; Security+SignedEndpoints+Outages+Logging+Concurrency+FileSecrets+Disposal 134; Jobs+Files+Database 145 — hepsi geçti. `Model` (şema testleri, ana uygulamayı kurmaz) koşulmadı |
| Contract | — | — | Bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T224-cerez-politikasi`
- **Commit:** PR'ın başlık SHA'sı dönüşte
- **PR:** dönüşte
- **CI:** yapım işçisi izlemez

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Çatının `ChunkingCookieManager`'ı uzun değeri böler, okurken birleştirir ve silmede parçaların hepsini siler | `CheckoutCookie_ThatIsSplitIntoChunks_IsReadBackAndEveryChunkIsDeleted` | ✓ |
| 2 | `Response.OnStarting` geri çağrıları son kayıtlıdan başlayarak çalışır; süzgeç öteki ara katmanların yazdığını görür | `HeaderFilter_BeforeThePublication_…` (sunucudaki sıra taklit edilir), entegrasyon testleri | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `ForwardedHeaders` ara katmanı uygulamada yok; ters vekil arkasında https şeması görünmeyebilir | D-112 | `09 §15.1` m.8 T215'in işi; T224 testleri https `BaseAddress` kullanır |
| 2 | Onay adımında `DeleteCheckout` çağrısı | (T509 kriteri 7) | Sipariş verme T509'dadır |

## Notlar

- **⚠ 1:** Süzgeç panel (`/panel`) dışında `/sistem` makine uçlarını da dışarıda bırakır: canlılık ucu veritabanı kapalıyken 503 vermeli, yayın sorusu veritabanını okur (`07 §4.33.1`). Bu uçlar çerez yazmaz.
- **⚠ 2:** Yayın sorusu `ICookiePolicyPublication` arayüzüyle altyapıdan sorulur; ortak altyapı modül türüne başvurmaz (`09 §6` m.2), `Settings` yanıtlar.
- **⚠ 3:** Form güvenliği çerezi `SecurePolicy=Always`; `ForwardedHeaders` gelene kadar üretimde ters vekil arkasında HTTP görünürse `GetAndStoreTokens` atar (D-112 → T215).
- **⚠ 4:** Ödeme onayında silme çağrı yeri T509'dadır; T224 yalnız tek silme yolunu ve testini verir.
