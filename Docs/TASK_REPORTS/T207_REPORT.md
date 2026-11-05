# T207 — Kimlik şemaları ve erişim reddi

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-06

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
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T207-kimlik-semalari-erisim-reddi`
- **Commit:** `ff848b9` — yapım; ardından PR numarası commit'i geldi
- **PR:** #403
- **CI:** dal CI'ı yapım chat'inde izlenmez
- **Post-merge run'lar:** *(validator doldurur)*

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
