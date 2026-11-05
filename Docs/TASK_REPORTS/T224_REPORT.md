# T224 — Çerez politikası

**Faz:** F2 | **Durum:** ✗ Doğrulama tur 1 FAIL — düzeltme bekliyor | **Tarih:** 2026-11-05

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
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-05, head `4737a61`) |
| Bulgu sayısı | 2 (B1, B2 — DÜŞÜK, S1); K-notu 3; düzeltme gerekli: evet |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T224-cerez-politikasi`
- **Commit:** PR'ın başlık SHA'sı dönüşte
- **PR:** #400
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

---

## Doğrulama Sonucu — T224 Çerez politikası — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T224-cerez-politikasi   **Commit:** 4737a61 (incelenen head; rapor commit'i bunun üstündedir)

### Verdict: ✗ FAIL

İki kriter `~ Kısmi`: tarama istisnalarından biri belgelenenden geniş (KK9), çerez yazımının tek-yer taraması ham `Set-Cookie` başlığını görmüyor (KK3). Kalan yedi kriter, testler, CI ve güvenlik temiz. Düzeltmeler küçük ve yalnız test projesindedir.

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (başta ve test koşularından sonra) |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana çalışma kopyasının aynı bekçisi; worktree) |
| Ana dal CI (son 3) | ✓ | 42097339118 success, 42097338902 success (T223 #399), 42086513414 success (T204 #398) |
| Memory drift | ✓ | `.claude/memory/MEMORY.md:22` T224 satırı |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Secure, HttpOnly, SameSite=Lax | ✓ | `CookieDefaults.Apply` (Always, HttpOnly, Lax); entegrasyon `AllCookies_AfterThePublication_…` gerçek `Set-Cookie` başlıklarında yedi adın hepsi + parçalar `Violations` boş |
| 2 | `__Host-` önekli, koşullar tek yazım yerinde | ✓ | `Path="/"`, `Domain=null` yalnız `CookieDefaults`; bütün yazım/silme `BrowserCookies` → `CookieDefaults.Create(...).Build(ctx)` → `ChunkingCookieManager`; form güvenliği çerezi `CookieSetup` içinde `CookieDefaults.Apply(options.Cookie)`; gerçek başlıkta `Path=/`, Domain yok |
| 3 | Envanter kapalı (yedi çerez) | ~ | Ad listesi ✓ (`CookieNames.All` 7 ad; `CookieNamesConsistencyTests` `09 §5.3` m.5 ile iki yönlü eşit; listede olmayan ad `Create`/`Write`/`Read`/`Delete`'te atar). Eksik: KK3 "envanter dışı çerez adı testi kırar" ham başlık yolunda tutmuyor — bkz. B2 |
| 4 | Misafir sepeti 400 gün, her ziyarette yenilenir | ✓ | `GuestCartLifetime` = `DisposalTimings.GuestCartIdleDays` (400); entegrasyon testi iki ziyarette `Max-Age` 400 gün ve yeniden yazım. Çağıran sepet modülü sonraki task'ta (altyapı payı) |
| 5 | Ödeme çerezi oturumluk, şifreli ve imzalı; onayda silinir | ✓ | `DataProtectionPurposes.CheckoutCookie` ile `Protect`; `Max-Age`/`Expires` yok; değerde e-posta yok; değiştirilmiş değer `invalid`; `DeleteCheckout` parçalar dahil siler (iki entegrasyon testi). Onaydaki çağrı T509, boşalma T508 (kriter metni) |
| 6 | Politika yayına alınana kadar `Set-Cookie` yok | ✓ | `CookieHeaderFilterMiddleware` `OnStarting`'de `Set-Cookie`'yi siler, ardışık düzenin en dışında (`Program.cs`, `UseRouting` öncesi); entegrasyon: yayın öncesi, yalnız taslak varken başlık yok; yayından sonraki ilk istekte açılır ve açık kalır |
| 7 | Süzgeç yalnız vitrine | ✓ | `AppliesTo`: `/panel` ve `/sistem` dışarıda (makine uçları vitrin değil — `07 §2.1.9`); panel yanıtında yedi çerez süzülmeden geliyor (entegrasyon) |
| 8 | Akış çerezinin yolu kök | ✓ | `ExternalLoginFlowCookie_…`: `Path=/`, `Max-Age` 600 (`09 §3.5` tablosu 10 dk), Lax; okunduğu yanıtta silinir |
| 9 | Analitik/piksel/reklam/onay bandı/gömme yok; tek istisna ödeme çerçevesi; taslakta yer tutucu | ~ | Kaynak ve paket taraması temiz, ayrı ihlal örnekleri kırmızı, taslakta yer tutucu ve yedi madde var ✓. Eksik: çerçeve istisnası `object`, `embed`, `frameset` ve `createElement('object'/'embed')`'i de serbest bırakıyor — bkz. B1 |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
T224'ün `10` birimi KP-73; satırları `VAL-A363`…`VAL-A367` vitrin/ödeme akışının tarayıcı gözlemidir (sepet, ödeme, sipariş sonraki fazlarda). KD-15/19/41/55'in `12` satırı yok. Çerez kuralına dokunan `VAL-A926`, `A927`, `A932`, `A934`, `A937`, `A938`, `A940` uçtan uca akış ister (giriş, sepet, ödeme, Google) — T224'ün altyapı payı kriterlerde sınandı, satırlar akışın task'larında kapanır. FAIL turu: hiçbir `Durum` değişmedi.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-A363…A367 | ORTA/KRİTİK | yok/mock | BEKLEMEDE | BEKLEMEDE (akış task'ları) | — |

`python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77`: Hata 0.

### Doğrulama Kontrol Listesi
- [x] KK1 her çerezde üç nitelik
- [x] KK2 `__Host-` adı; gerçek başlıkta `Path=/`, Domain yok
- [ ] KK3 envanter dışı çerez adı testi kırar — ham `Set-Cookie` başlığıyla yazılan ad hiçbir testi kırmıyor (B2)
- [x] KK4 sepet çerezi ömrü ve yenilenmesi
- [x] KK5 ödeme çerezi şifreli; onayda silinir (silme yolu + testi; çağrı T509)
- [x] KK6 yayında değilken `Set-Cookie` yok
- [x] KK7 panel yanıtı süzülmez
- [x] KK8 akış çerezi yolu `/`
- [ ] KK9 "yalnız ödeme çerçevesi istisna" — istisna çerçeve dışındaki gömme öğelerini de kapsıyor (B1)
- [x] `09 §25` ilgili satırlar (adlar `09 §5.3` m.5'ten; 10 dk §3.5 tablosunda; CONTEXT güncel; yeni paket yok; mimari kural için tarama + ihlal örneği; eşitlik denetimi `Consistency` trait'inde)
- [x] Güvenlik taraması

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 1330/1330 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration | ✓ | `--filter-namespace` parçaları, PostgreSQL 18 (`shopfolio-t224-val-pg`, 127.0.0.1:55571, sonra silindi) | Web 86, Security 18, Concurrency 26, Jobs 77, Database 35, Disposal+FileSecrets+Files+Logging+Outages+SignedEndpoints 123, Model (iki parça, `--max-threads 3`) 499 + 394 — toplam 1258, 0 kaldı |
| Restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; koşu sonrası `git status` temiz |
| Dal CI | ✓ | `gh run list --branch task/T224-cerez-politikasi` | 42102317150 success (head `4737a61`) |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz (oturum işleyicisi yok; yalnız çerez yazıcısı)
- Input validation: Temiz (çerez okuma veri koruma ile; bozuk değer `invalid`)
- Yeni bağımlılık: Yok

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| B1 | KK9 / kriter 9 (`10` KD-41, KD-55; `02 §12.2.5`) | DÜŞÜK | S1 | Beklenen: ödeme sayfasında yalnız çerçeve öğesi serbest ("tek istisna ödeme sağlayıcısının çerçevesi"; sınıfın kendi belgesi "the frame element alone is allowed there"; yapım raporu "yalnız çerçeve öğesi genişliğinde"). Gerçekleşen: `EmbedElement()` tek regex'tir (`iframe|frame|frameset|object|embed` ve `createElement('iframe'/'object'/'embed')`) ve `PaymentFramePages`'teki sayfada bütünüyle atlanır; test bunu sabitler (`Scan_OnAPaymentFramePage_…`: satır 10'daki `<object>` sayfada bulgu değil). Düzeltme: `iframe` öğesini (ve `createElement('iframe')`) ayrı regex'e ayır, istisna yalnız ona; `frame`, `frameset`, `object`, `embed` her sayfada bulgu; test satır 10'un ödeme sayfasında da bulgu olduğunu doğrular | KK9 | düzeltme turunda | `tests/Shopfolio.UnitTests/Architecture/ThirdPartyContentScan.cs`, `ThirdPartyContentScanTests.cs` |
| B2 | KK3 / kriter 3 (`09 §15.1` m.6, §5.3 m.5) | DÜŞÜK | S1 | Beklenen: envanter dışı çerez adı testi kırar; çerez yazımı tek yerdedir. Gerçekleşen: `CookieSourceScan` kural 1 yalnız `Cookies.Append/Delete(`, `AppendResponseCookie(`, `DeleteCookie(`'i arar; `context.Response.Headers.Append("Set-Cookie", "_ga=1")`, `Headers.SetCookie = …`, `Headers[HeaderNames.SetCookie] = …` hiçbir kurala takılmaz ve entegrasyon testi yalnız sonda uçlarını okur — envanter dışı ad bu yolla yeşil kalır. Düzeltme: ham `Set-Cookie` başlığına erişimi (`"Set-Cookie"`, `HeaderNames.SetCookie`, `.SetCookie`) yeni kurala bağla; tek izinli dosya silme yapan `CookieHeaderFilterMiddleware.cs`; ihlal örneğine bir satır ve izinli dosya testine durum ekle | KK3, KK2 (tek yazım yeri) | düzeltme turunda | `tests/Shopfolio.UnitTests/Architecture/CookieSourceScan.cs`, `CookieSourceScanTests.cs`, `ViolationSamples/Cookies/CookieMisuse.cs` |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `CookiePolicyGate.IsOpenAsync` yayından önce her vitrin isteğinde veritabanını okur ve süzgeç ardışık düzenin en dışında, `09 §8.1`'in yakalama katmanının (T212) da dışındadır: veritabanı kapalıyken vitrin isteği yakalanmamış istisnayla boş 500 döner, hata sayfası ve kayıt olmaz. Öneri: sorgu hatasında kapıyı kapalı say (başlığı sil) ve isteği sürdür; birim testiyle sına | düzeltme turunda (aynı PR) |
| K2 | `ThirdPartyContentScan` ödeme sayfasındaki çerçevenin kaynağını sınamaz (istisna her kaynaktan `iframe`'e açık); çalışma anında `frame-src` CSP'si yalnız PayTR'ye izin verir (`09 §15.2` m.4, sonraki task) | B1 düzeltmesiyle birlikte `iframe` kaynağı `https://www.paytr.com` ya da kaynaksız olarak daraltılabilir; düzeltme turunda karar verilir, değilse CSP task'ına not |
| K3 | D-112 (ters vekil başlıkları → T215): gerekçe doğru (form güvenliği çerezi `SecurePolicy=Always`; `ForwardedHeaders` `src/`'de yok — `09 §15.1` m.8), hedef uygun (T215 ilk IP/ters vekil tüketicisi, T224'ten sonra sırada) | kabul — DEFERRED_BACKLOG'da |

### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Öz denetim tablosu var, her madde kanıtlı.
- KK9 öz denetimi "istisna tek sayfa ve yalnız çerçeve öğesi genişliğinde" der; kod ve test `object`/`embed`/`frameset`'i de serbest bırakır (B1).
- KK3 öz denetimi ✓; ham başlık yolu taranmıyor (B2).
- Rapor Integration'da `Model` ad alanını koşmadığını yazar; doğrulamada koşuldu, 893 test yeşil.

### Merge ve Post-merge
- FAIL: merge yok. Rapor + durum + hafıza dala yazıldı (16. adım).
