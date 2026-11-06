# T207 — Kimlik şemaları ve erişim reddi

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- İki kimlik şeması (`07 §2.3.1`; `09 §15.1` m.2): `SessionAuthenticationHandler` müşteri (`__Host-session`) ve yönetici (`__Host-admin-session`) şemasında kayıtlı; çerez yalnız anahtarı taşır, özeti `Session.TokenHash` ile aranır, bağı şemayla uyuşmayan kayıt oturumsuzdur; süre `ProductConstants` (kısa 2 saat, uzun 30 gün); `LastActivityAt` dakikada en çok bir kez `ExecuteUpdate` ile yazılır, uzun oturumun çerezi aynı ömürle yeniden yazılır. Kimlik hesabın iç anahtarını ve oturum kimliğini taşır.
- Kapı varsayılan kapalı: `AuthorizeFolder("/Panel", AdminPolicy)` ve `AuthorizeFolder("/Account", CustomerPolicy)`; `AccessConvention` klasör ile adres uyumunu açılışta sınar ve `07 §2.3.1`'in kapalı açık-sayfa listesindeki sayfalara allow-anonymous işareti koyar. Parçalı yükleme ucu `RequireAdminSession` ile panel şemasındadır.
- Oturumsuz geliş (`07 §2.3.2`): GET 302, POST 303, şemanın giriş sayfasına; GET'te yol ve sorgu `returnUrl` olarak taşınır, POST'ta formun GET adresi; çıkış sayfaları (`SignOutPageAttribute`) bunun dışındadır — yazmaz, çıkışın yönlendirmesini alır (UC-77: oturum gerektirmeyen sayfa ya da ana sayfa; UC-109: `/panel/giris`).
- `ReturnPath` (`05 §6.5.6`): tek yerel-yol denetimi (D-118: denetim karakterleri de reddedilir), `Resolve` (alanın ana sayfası: vitrinde `/`, panelde `/panel`), `AfterSignOut`. `FormResults.IsLocalPath` ona devredildi.
- `AccessDenial.NotFound()` tek 404 (E-06 gövdesi T504'te); `IOrderingModule.FindMemberOrderAsync` / `FindOrderByAccessKeyAsync` sahiplik ve erişim anahtarı sorguları; yönetici oturumu vitrinde müşteri sayılmaz, yalnız `HasAdminSessionAsync` (şerit, önizleme).
- `Program.cs`: `AddSessionAuthentication`, `AddSessionAccess`, `UseAuthentication`/`UseAuthorization`.
- Mimari kural: yönlendirme yalnız iki dosyada yazılır (`RedirectSourceScan`, ihlal örnekli).
- D-118 kapatıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/`: `ReturnPath.cs`, `SessionSchemes.cs`, `SessionAuthenticationHandler.cs`, `SessionAccessSetup.cs` (`AccessConvention` dahil), `AccessDenial.cs`; `Infrastructure/Forms/FormResults.cs`; `Modules/Ordering/` (`IOrderingModule`, `OrderingModule`); `Program.cs`.
- Testler: `tests/Shopfolio.UnitTests/Infrastructure/{ReturnPathTests,AccessConventionTests}.cs`, `Architecture/{RedirectSourceScan,RedirectSourceScanTests}.cs` + `ViolationSamples/Redirects/RedirectMisuse.cs`, `Consistency/OpenPageListConsistencyTests.cs`, `FormResultsTests.cs` (D-118 satırları); `tests/Shopfolio.IntegrationTests/Web/{SessionAccessTests,TestSessions,PageFormPatternTests}.cs`, `Pages/AccessSamples.cs` ve 14 örnek sayfa.
- `Docs/DEFERRED_BACKLOG.md` (D-118), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`, `.claude/CONTEXT.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki şema, ayrı tablo, ayrı adlı çerez | ✓ | `SessionAccessTests.EachScheme_ReadsItsOwnCookieAndTheOtherAccountTypesSessionIsNoSession`, `ACookieThatHoldsNoSession_IsNoSession`; `OpenPageListConsistencyTests.TheTwoSessionCookiesOf07_AreTheNamesTheSchemesRead` |
| 2 | `/panel` kapalı; açık sayfalar kapalı liste | ✓ | `Panel_WithoutASession_GoesToTheSignInPage_AndTheOpenPagesDoNot`, `Panel_WithAnAdministratorSession_OpensItsPages`, `EveryEndpointUnderThePanelAndTheAccount_IsClosed_ExceptTheOpenPagesOfTheClosedList`; `AccessConventionTests.Apply_On*`; `OpenPageListConsistencyTests.OpenPanelPagesOf07_EqualTheListOfTheCode_InBothDirections` |
| 3 | `/hesap` kapalı, tek istisna `/hesap/silindi`; öteki vitrin ve makine uçları açık; parçalı yükleme panel şemasıyla | ✓ | `Account_WithoutASession_GoesToTheSignInPage_AndTheDeletedStateAndTheOpenStorefrontDoNot`, `TheChunkedUploadEndpointOfThePanel_AsksForThePanelsSession`; `OpenAccountPageOf07_EqualsTheListOfTheCode_InBothDirections` |
| 4 | Oturum isteyen GET kendi girişine 302, `returnUrl` taşır | ✓ | `AGetThatNeedsASession_Is302WithTheAddressAndTheQueryInReturnUrl` |
| 5 | Kapanmış oturumda POST yazmaz, 303; çıkış istisna | ✓ | `APostWithoutASession_Is303ToTheSignInPageWithTheFormsGetAddress_AndWritesNothing`, `APostWithAnEndedSession_WritesNothing_AndGoesToTheSignInPage`, `APostWithASession_Writes`, `TheSignOutWithoutASession_WritesNothing_AndGetsTheSignOutsRedirectNotTheSignInPages`, `TheSignOutWithASession_RunsTheHandler`; `ReturnPathTests.AfterSignOut_*` |
| 6 | `returnUrl` yalnız sitenin yolu, değilse ana sayfa | ✓ | `TheSignInPage_IgnoresAReturnAddressThatIsNotASitePath_AndGoesToTheAreasHomePage`, `TheSignInPage_KeepsASitePath_AndANetworkPathSentInTheQueryEncodedOnceIsIgnored`; `ReturnPathTests.IsLocalPath_*`, `Resolve_*`; `RedirectSourceScanTests` |
| 7 | Sahiplik reddi ve var olmayan kayıt aynı 404; POST yazmaz | ✓ | `AnotherCustomersOrderAndAnOrderThatDoesNotExist_GetTheSame404_AndAPostWritesNothing`, `AWrongOrRenewedAccessKeyAndAMissingPanelRecord_GetTheSame404_AsAnotherCustomersOrder` (durum, gövde ve başlık adları eşit) |
| 8 | Misafir sipariş yalnız erişim anahtarlı adresle | ✓ | `AGuestOrder_OpensWithItsAccessKeyOnly`, `AMembersOrder_AlsoOpensWithItsAccessKey` |
| 9 | Yönetici oturumu vitrinde müşteri sayılmaz | ✓ | `AnAdministratorSession_OpensNeitherTheAccountNorAMembersOrder_AndOnlyTheStripReadsIt` |

### Öz denetim (bitiş kapısı 2)

Her madde `git diff origin/main...HEAD` üzerinde yeniden denetlendi; testler bu turda koşuldu (aşağıdaki sayılar).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: iki çerez adı ve iki şema ayrı | ✓ | Kriter 1 testleri; `CookieNames.Session` ≠ `CookieNames.AdminSession` |
| KK2: oturumsuz `/panel/siparisler` girişe gider; açık sayfa gitmez | ✓ | `Panel_WithoutASession_GoesToTheSignInPage_AndTheOpenPagesDoNot` |
| KK3: `/hesap/silindi`, ürün sayfası, `/sistem/canlilik` oturumsuz açık | ✓ | `Account_WithoutASession_...`; `/sistem/canlilik` aynı testte 200 (makine ucu şema taşımaz) |
| KK4: 302'de `returnUrl` | ✓ | Kriter 4 |
| KK5: kapanmış oturumda POST 303 ve yazma yok; oturumsuz çıkış UC-77'nin yönlendirmesini alır | ✓ | Kriter 5 |
| KK6: `//evil.example`, `/\evil`, `https://evil.example` yok sayılır; ana sayfaya dönülür | ✓ | Kriter 6; tablo ayrıca `%2F%2F`, `javascript:`, sekme, `\n`, `\r`, U+0001, U+007F |
| KK7: başkasının siparişi ile olmayan sipariş aynı 404 | ✓ | Kriter 7 |
| KK8: anahtarsız misafir isteği sayfayı açmaz | ✓ | `AGuestOrder_OpensWithItsAccessKeyOnly` |
| KK9: yönetici oturumuyla `/hesap` açılmaz | ✓ | Kriter 9 |
| `12`: VAL-A602…A609 ile eşleşen KK'ler | ✓ | A602/A604 → KK7; A603 → KK7 (POST yazmaz); A605 → T-sonrası (sorgu ucu); A606 → KK4/KK6; A607 → KK5; A608 → KK5; A609 → KK9 |
| `09 §25`: dokümanla çelişki yok / sapma bildirimi | ✓ (⚠ 1) | Tek mekanizma sapması aşağıda, davranış aynı |
| `09 §25`: diff yalnız görevin istediği; yeni ayar/sabit belgede | ✓ | Süreler `ProductConstants`'tan (P-15, P-16); dakikalık yazma `09 §15.1` m.2; yeni ayar, `.env.example` değişikliği yok |
| `09 §25`: mimari testler yeşil, ayrı ihlal örneği | ✓ | `RedirectSourceScanTests.Scan_OnViolationSample_*` + `ViolationSamples/Redirects/RedirectMisuse.cs` (13 bulgu, dosya satırlarıyla); her çağrı biçimi ayrı testte (`Scan_OfEachCallForm_IsAFinding`, `Scan_OfEachLocationForm_IsAFinding_*`) |
| `09 §25`: `CONTEXT.md` aynı PR'da | ✓ | `.claude/CONTEXT.md` Security satırı |
| Güvenlik taraması | ✓ | Açık yönlendirme (D-118 dahil) yukarıdaki testlerle; oturum anahtarı yalnız özet olarak aranır; 404 üç durumda aynı |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `RedirectSourceScan` istisnası tam yolla iki dosya (`FormResults.cs`, `SessionAuthenticationHandler.cs`); `Scan_InTheTwoAllowedFiles_FindsNothing_AndInAnyOtherFileFindsTheSameText` yakın adlı/iç içe yolun istisna olmadığını sınar; yalnız tam satır yorumu atlanır |
| Sık FAIL 2: tarama çağrı biçimlerinden birini kaçırır; ihlal örneği yok | ✓ | `Redirect(`, `RedirectPermanent(`, `RedirectPreserveMethod(`, `LocalRedirect*`, `new RedirectResult(`, `Response.Redirect(`, `Results/TypedResults.Redirect`, `RedirectTo*`, `RedirectUri =`, `Headers.Location`, `HeaderNames.Location`, `"Location"`; çok satırlı çağrı örnekte (satır 13); `src` altındaki `.cs` ve `.cshtml` taranır |
| Sık FAIL 3: sayı/etiket tutarlılığı | ✓ | Çerez adları (yedi) ve `12` sayıları değişmedi; açık sayfa listesi kod ↔ `07 §2.3.1` iki yönlü (`OpenPageListConsistencyTests`) |
| Sık FAIL 4: hedefi T207 olan backlog kaydı | ✓ | `grep -n T207 Docs/DEFERRED_BACKLOG.md`: yalnız D-118; ✅ Çözüldü → T207 |
| Sık FAIL 5: belgeden sapma karar kaydısız | ✓ (⚠ 1) | Davranış aynı olduğundan gerekçe bu raporda; belge değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 1628 / 1628 | `dotnet test tests/Shopfolio.UnitTests` | önce 1515; +113 (ReturnPath, AccessConvention, RedirectSourceScan, OpenPageListConsistency, FormResults D-118 satırları) |
| Integration | ✓ 1360 / 1360 | `dotnet test tests/Shopfolio.IntegrationTests --no-build -- --filter-namespace …` (parça parça: Web 188, Model 893, öteki 141 + 138) | önce 1324; SessionAccessTests 35; gerçek PostgreSQL |
| Contract | — | — | Bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-06, head `cade300`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — K1 → D-120 (iki mekanizma cümlesi: `AllowAnonymousToPage` ve `IUrlHelper.IsLocalUrl`; davranış aynı; F2 kapısına kadar karar kaydı), K2 → D-121 (oturumu kapanmış POST'un 303'ü `notice=SessionExpired` taşımıyor; T214), K3 → D-122 (örnek üye kökü oturumsuz 404, `07` UC-33 302 ister; VAL-A609 gerilimi; T511) |

### Doğrulayıcının kanıtları (tur 1)

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un bekçileri, dal kopyası değil); ana dal son 3 run 42128519903, 42128519929, 42113408503 success; `grep T207 .claude/memory/MEMORY.md` satır 22.
- **Dal CI:** 42133128685 (head `cade300`) success — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E, CI Gate.
- **Yerel:** `dotnet build -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` çıkış 0; Unit (+Consistency) 1628/1628; betik testleri 4 dosya; Integration 1360/1360 gerçek PostgreSQL 18'de (127.0.0.1) parça parça — `SessionAccessTests` 35, öteki Web 153, Security+Disposal+FileSecrets+Files+Logging+Outages+SignedEndpoints 141, Concurrency 26, Jobs 77, Database 35, Model 483 + 372 + 38; koşu sonrası `git status` temiz, `dotnet restore --locked-mode` çıkış 0.
- **Kod okuması:** iki şema `AddScheme` ile, varsayılan `Customer`; politikalar şemasını taşır ve `RequireAuthenticatedUser`; `AuthorizeFolder` klasör yoluyla; `AccessConvention` klasör–adres uyumsuzluğunda açılışı durdurur, açık işaret yalnız listedeki seçiciye; bağı şemayla uyuşmayan oturum (müşteri şemasında `AdminId` dolu ya da tersi) sonuçsuz; challenge 401 yazmaz; `returnUrl` yerel yol değilse eklenmez; çıkış sayfası `SignOutPageAttribute` ile 303 → `/panel/giris` ya da `ReturnPath.AfterSignOut`; `AccessDenial.NotFound()` 404, 403 hiçbir yolda yok (politikalar yalnız kimlik ister).
- **D-118:** `ReturnPath.IsLocalPath` U+0000–U+001F ve U+007F'i reddeder, `FormResults.IsLocalPath` ona devreder; `ReturnPathTests` (`/\t/`, `/\n/`, `/\r/`, U+0001, U+007F, U+0000) ve `FormResultsTests` (+4 satır) yeşil; uç testinde `%2F%09%2Fdis.example` ana sayfaya düşer. Kapandı.
- **Yönlendirme taraması:** istisna tam yolla iki dosya (`FormResults.cs`, `SessionAuthenticationHandler.cs`), klasör ya da kalıp değil; yakın adlı ve iç içe yol istisna değil (test); ihlal örneği `ViolationSamples/Redirects/RedirectMisuse.cs` 13 bulgu, çok satırlı çağrı dahil; yalnız tam satır yorumu atlanır.
- **Mekanizma sapması (talimatın sorusu):** açık sayfa listesinin adres şablonuyla uygulanması davranışça eşit (öteki her sayfa kapalı, liste `07 §2.3.1` ile iki yönlü eşit, uç testi her ucu sayar) ve sayfa adı uydurmamak için gerekçelidir; belgede ad geçen cümle koddan ayrıldığı için karar kaydı gerekir — doğrulamayı bloklamaz (D-116 emsali), D-120'ye ileri atıldı. Aynı kayda `09 §15.2` m.6 / `05 §6.5.6`'nın "çatının `IsLocalUrl`'ü" cümlesi eklendi (kod eşdeğer, `~/`'yi ayrıca reddeder).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T207-kimlik-semalari-erisim-reddi`
- **Commit:** `ff848b9` — yapım; ardından PR numarası commit'i geldi
- **PR:** #403
- **CI:** dal CI 42133128685 (head `cade300`) success — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E, CI Gate
- **Post-merge run'lar:** squash `3226fcd` (#403) — CI 42138101595 success, Publish 42138101567 success (T208 dalında dolduruldu, skill adım 15)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | Çatının kimlik doğrulama ve Razor Pages kuralları kodla sınandı (Integration testleri gerçek sunucu ardışık düzeninde) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | E-06 "sayfa bulunamadı" gövdesi | T504 | T207 yalnız tek 404 durumunu verir (`AccessDenial.NotFound`); gövde ekranın işi |
| 2 | `Cache-Control: no-store` (`09 §15.2` m.5) ve güvenlik başlıkları | T209 | `11`'de T209'un |
| 3 | Sorgu ucundan erişim anahtarlı adrese 303 (VAL-A605) | sorgu ucunun task'ı | T207 yalnız erişim anahtarlı kökü ve sahiplik sorgusunu verir |

## Notlar

- **⚠ 1 — Açık sayfa mekanizması `09 §15.1` m.2'den ayrılıyor (davranış aynı):** belge açık sayfaları `AllowAnonymousToPage` listesi (sayfa yoluyla) ve "`Pages/Panel/` altındaki her sayfa" mimari testi olarak yazar; sayfa dosya adları henüz yok (giriş, şifre sıfırlama sayfaları sonraki task'larda yazılır), bu yüzden liste sayfa yolu yerine `07 §2.3.1`'in adresleriyle tutulur ve `AccessConvention` sayfanın adres şablonu listedeyse allow-anonymous işaretini koyar. Davranış aynıdır: öteki her sayfa klasör kuralıyla kapalı, liste `07 §2.3.1` ile iki yönlü eşit (`OpenPageListConsistencyTests`), klasör ve adres uyumsuzluğu açılışta durdurur (`AccessConventionTests`), mimari uç testi her uç noktayı sayar. Belge değişmedi; istenirse `09 §15.1` m.2 cümlesi "`AllowAnonymousToPage` listesi ya da adrese bağlı eşdeğeri" diye düzeltilebilir (öneri, soru değil).
- **Uygulama notu:** `FormResults.SeeOther` hâlâ `IsLocalPath` ister; yönlendirme yazan iki dosyadan biridir.
- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: 42128519929 (CI) ve 42128519903 (Publish) success (T206 #401)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Paralel işçi:** T211 (model bağlama kalıbı); çakışmada origin/main'e rebase
