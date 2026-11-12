# T516 — Şifre sıfırlama ve yeniden doğrulama (E-23, E-24)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama bekliyor) | **Tarih:** 2026-11-12

---

## Yapılan İşler

- Şifre sıfırlama `Pages/Storefront/PasswordReset.cshtml(.cs)` (`/sifre-sifirlama`, `/sifre-sifirlama/{anahtar}`): `GET` e-posta alanını verir (müşteri oturumunda hesabın e-postasıyla dolu, adreste taşınmaz). `RequestPasswordReset` (UC-57) nötr hâliyle 303 → `/sifre-sifirlama`; kayıtlı ve kayıtsız adreste yanıt aynı, kayıt L-2 e-posta ekseninde yapılır (reddedilen de sayılır), 422 boş/biçim, 429 nötr (`Retry-After` yok). Doğrulanmış hesapta `PasswordReset` bağlantısı (Z-2) ve e-postası yazılır; şifresiz hesapta aynı bağlantı şifre belirlemedir; kayıtsız adrese ve bekleyen kayda e-posta gitmez. Kurma adımı (UC-58) beş bağlantı durumundan yalnız geçerli olanda açılır; `ResetPassword` (UC-59) şifreyi kurar, bağlantıyı tek koşullu tüketir, bütün oturumları siler, oturum açmaz, 303 → kurulduğu hâl; P-17 altı ya da yaygın şifre 422 ve bağlantı tüketilmez; arada geçersizleşen bağlantıda 200. Tanınan tarayıcı işareti sıfırlama sonucunda yazılır.
- Yeniden doğrulama `Pages/Account/Reauthentication.cshtml(.cs)` (`/hesap/yeniden-dogrulama`): işlemin adı, şifre alanı (şifresi varsa), Google düğmesi (Google bağlıysa ve tanımlıysa); işlem kapalı listede yoksa 303 → `/hesap`; ekranı açmak önceki izni düşürür; oturum yoksa 302 girişe. `Reauthenticate` (UC-61) doğru şifrede işlem ve tek kullanımlık değerle 303 → `/hesap`, oturumda tek izin kalır; yanlış şifre alan mesajıyla 422 ve L-1'e sayılır, 429'da doğru şifre de kabul edilmez. `StartSocialReauthentication` (UC-62) taze giriş ister (`auth_time`), Google'sız hesapta ya da Google tanımsızsa 422 ve L-1'e girmez.
- Google dönüşü `GoogleReturn` (D-1351): `kind == Reauthentication` dalı; `auth_time` tazeliği, e-posta eşleşmesi ve doğrulanmışlık sağlanırsa işlem izni yazılır ve 303 forma/E-24'e; eski/eksik `auth_time`, başka ya da doğrulanmamış e-posta, başarısız ya da ulaşılamayan Google izin yazmadan kendi koduyla (`SocialReauthenticationFailed`, `SocialReauthenticationUnavailable`; `GoogleIdentityFailureRule`, D-109 payı) E-24'e döner. `GoogleIdentityFailureRule` `Infrastructure/Integrations` altına taşındı.
- Modül: `IMembershipModule` ve `ISessionService`'e yeniden doğrulama izni için sözleşme yöntemleri, `GoogleReauthentications` (modül içi), `MembershipRecords` yeni kayıtlar. Metinler `PasswordResetTexts` ve `ReauthenticationTexts` (tek yerde); `FramePaths` yeni yollar; `ReauthenticationAddresses` adres üretimi.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Storefront/{PasswordReset,GoogleReturn}.cshtml(.cs)`, `Pages/Storefront/ReauthenticationAddresses.cs`, `Pages/Account/Reauthentication.cshtml(.cs)`, `Pages/Components/{PasswordResetTexts,ReauthenticationTexts}.cs`, `Pages/Frame/FramePaths.cs`, `Infrastructure/Integrations/{GoogleIdentityFailureRule,GoogleSignInFlow}.cs`, `Modules/Membership/{GoogleReauthentications,MembershipModule,SessionService}.cs`, `Modules/Membership/Contracts/{IMembershipModule,ISessionService,MembershipRecords}.cs`; testler `tests/Shopfolio.IntegrationTests/Web/CustomerPasswordResetAndReauthenticationPageTests.cs`, `tests/Shopfolio.UnitTests/Pages/ReauthenticationTextsTests.cs`, `Infrastructure/GoogleSignInFlowTests.cs`, `Outages/OutageRulesTests.cs`, `Consistency/OutageConsistencyTests.cs`. Kayıt: `Docs/STATUS_CHANGELOG.d/T516.md`, `Docs/DEFERRED_BACKLOG.d/{T515,T516}.md`, `Docs/DEFERRED_BACKLOG.md` (D-109, D-572, D-801, D-806 payları), `Docs/IMPLEMENTATION_STATUS.md`.

## Kabul Kriterleri Kontrolü

Test sınıfı `CustomerPasswordResetAndReauthenticationPageTests` (kısaltma: S).

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /sifre-sifirlama` e-posta alanı; oturumda hesabın e-postasıyla dolu, adreste taşınmaz | ✓ | `S.ASignedInMember_SeesTheOwnAddressInTheField_AndTheAddressCarriesNoEmail_T516_KK1` |
| 2 | `RequestPasswordReset` nötr 303; kayıtlı/kayıtsız aynı yanıt | ✓ | `S.TheRequest_GivesTheSameAnswerForARegisteredAndAnUnregisteredAddress_T516_KK2` |
| 3 | Doğrulanmış hesapta `PasswordReset` bağlantısı ve e-posta; şifresizde şifre belirleme; kayıtsız ve bekleyen kayda e-posta yok | ✓ | `S.TheRequest_QueuesTheEmailOnlyForAVerifiedAccount_AWithoutPasswordIncluded_T516_KK3` |
| 4 | 422 boş/biçim; 429 (L-2) nötr; her istek kaydedilir | ✓ | `S.ABrokenAddress_Gives422_AndWritesNothing_T516_KK4` (kuramsal veri), `S.TooManyRequestsOfOneAddress_Give429_AndTheRequestsAreCounted_T516_KK4` (`Retry-After` yok, L-2 e-posta satırları sayılır), `S.TheBlockOfTheResetRequest_LiftsWhenTheAttemptsLeaveTheWindow_D572_T516_KK4` |
| 5 | `GET /sifre-sifirlama/{anahtar}` geçerliyi açar; kullanılmış, dolmuş, yenilenmiş, tanınmayan açmaz | ✓ | `S.AValidLink_OpensTheSettingStep_AndEveryOtherStateOpensTheInvalidOne_T516_KK5` |
| 6 | `ResetPassword` şifreyi kurar, bağlantıyı tüketir, oturumları siler, oturum açmaz; 303 → kurulduğu hâl | ✓ | `S.TheNewPassword_ClosesEverySession_OpensNone_AndTheOldPasswordStopsWorking_T516_KK6` |
| 7 | P-17 altı/yaygın şifre 422, bağlantı tüketilmez; arada geçersizleşen bağlantıda 200 | ✓ | `S.AWeakOrCommonPassword_Gives422_AndTheLinkIsNotConsumed_T516_KK7`, `S.ALinkSpentBetweenThePageAndTheButton_Gives200WithTheInvalidState_AndChangesNothing_T516_KK7` |
| 8 | `GET /hesap/yeniden-dogrulama` işlem adı, şifre alanı, Google düğmesi; listede yoksa 303 → `/hesap`; açılış izni düşürür | ✓ | `S.TheScreen_OpensForEachOfTheThreeOperations_..._KK8`, `S.TheTwoFieldConditions_PasswordFieldNeedsAPassword_GoogleButtonNeedsALinkAndTheOpenGate_T516_KK8`, `S.AnOperationOutsideTheClosedList_IsA303ToTheAccountPage_T516_KK8`, `S.OpeningTheScreen_DropsTheEarlierPermission_T516_KK8`, `S.WithoutASession_TheScreenSendsThePersonToTheSignIn_T516_KK8`; metinler: `ReauthenticationTextsTests` |
| 9 | `Reauthenticate` doğru şifrede 303 → `/hesap`, tek izin; yanlış şifre 422, L-1'e sayılır | ✓ | `S.TheRightPassword_GivesOnePermission_AndA303ToTheAccountPageWithTheValue_T516_KK9`, `S.AWrongPassword_Gives422WithTheFieldMessage_CountsForL1_AndGivesNoPermission_T516_KK9`, `S.TooManyWrongPasswords_Give429_AndTheRightOneIsThenNotAccepted_T516_KK9`, `S.AnEmptyPassword_Gives422_T516_KK9` |
| 10 | `StartSocialReauthentication` taze giriş ister; UC-51 dönüşü izinle forma/E-24'e; Google'sızda/tanımsızda 422, L-1'e girmez | ✓ | `S.TheGoogleRoute_RedirectsToGoogleAskingForAFreshSignIn_AndTheReturnGrantsTheOperation_T516_KK10`, `S.AReturnWithAnOldOrMissingAuthTime...GrantsNothing..._T516_KK10`, `S.AFailedOrUnreachableGoogle_ComesBackWithItsOwnCode_T516_KK10`, `S.TheGoogleRoute_IsRefused422ForAnAccountNotLinkedToGoogle_AndCountsNothingForL1_T516_KK10`; `GoogleSignInFlowTests` (ExchangeAsync), `OutageRulesTests` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yazdığı dahil kriter 1–10 ve KK1–KK10 diff'te baştan denetlendi (yalnız kalanlar değil); `sik-fail-nedenleri.md` okundu. Bu turda eksik bulunanlar ve giderilenler: (a) hedefi T516 olan D-572, D-801, D-806 ve D-109 satırlarına T516 payı yazılmamıştı, yazıldı; D-572'nin "pencere dolunca engel kalkar" payı sayfa testsizdi, `...LiftsWhenTheAttemptsLeaveTheWindow_D572_T516_KK4` eklendi; (b) 429 testi `Retry-After` yokluğunu denetlemiyordu, eklendi; (c) D-1351 (T515'ten gelen dikiş) kapatıldı.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–10, KK1–KK10 | ✓ | yukarıdaki tablo; sınıf `S` 34 test |
| (1) Bekçi/tarama istisnası geniş | ✓ | yeni istisna eklenmedi; tarama sınıfları aşağıdaki Integration satırında |
| (2) Kaynak taraması çağrı biçimi | ✓ | yeni tarama kuralı yok; yeni kod `ProtectedKeys`/`SecretKey` dışında anahtar üretmez; Unit tarama testleri TAM yeşil |
| (3) 06 "Varsayılan"/FK dizini | ✓ | şema ve göç değişikliği yok |
| (4) Kural değişince aynı sayıyı anan yerler | ✓ | `GoogleIdentityFailureRule` taşındı: `OutageRulesTests`, `OutageConsistencyTests` güncellendi; `FramePaths` yeni yollar `AccessSamples`/sitemap taramasında (aşağıdaki sınıflar) |
| (5) Belge kuralını değiştiren 09 metni | ✓ | `09` değişmedi |
| Kaynak taraması: bağlantı anahtarı sayfaları `Referrer-Policy: no-referrer`, `[NoStore]` | ✓ | `SecurityHeaders` ve `Forgery` sınıfları aşağıda |
| Eşzamanlılık / kilit sırası (09 §15.1) | ✓ | `ResetPassword` bağlantıyı tek koşullu tüketir (KK7 yarış testi); hesap kilidi `LockSequence` sırasıyla |
| RejectionLog kodları | ✓ | `S.AWrongPassword_Gives422...` `ErrorCode.ReauthenticationFailed` satırını doğrular; 429 `PasswordResetLimitExceeded`, `SignInLimitExceeded` |
| "siz" hitabı, bölge sırası (04 5.23.3 / 5.24.3) | ✓ | `ReauthenticationTextsTests`; sayfalarda başlık, işlem satırı, alanlar, düğme sırası 04'e göre |
| N+1 | ✓ | sayfalar tek hesap okur; liste yok |
| Biçim (`dotnet format`) | ✓ | aşağıda |
| `09 §25` / güvenlik | ✓ | `[NoStore]`; POST sahte istek belirteci ister (genel; `Forgery` sınıfları); nötr yanıt (hesabı ele vermez); yeni bağımlılık, sır, şema yok |

`12`: Durum hücreleri doğrulayıcının işidir, dokunulmadı. Kanıt testleri: VAL-A097 (sıfırlama: KK2, KK3, KK6), VAL-A352 (L-2 429: KK4), VAL-A362 (yeniden doğrulamada L-1: KK9), VAL-A451 (KK7), VAL-A498, VAL-A499, VAL-A500, VAL-A501, VAL-A506, VAL-A561, VAL-A562 (KK2, KK4, KK5–KK9), VAL-A680 (ret tüketmez: KK9). VAL-B078 (320 genişlik) tarayıcı gözlemidir, D-1418.

Backlog payı: D-1351 ✅ (yerinde kapatıldı); D-109, D-572, D-801 (A352 payı), D-806 T516 payları kalem satırlarına yazıldı. Yeni: D-1416–D-1418 (`DEFERRED_BACKLOG.d/T516.md`).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | TAM 7486/7486 |
| Integration | ✓ (parçalı) | `--filter-class` ile ilgili sınıflar, Postgres 18 (`track_commit_timestamp=on`) | `CustomerPasswordResetAndReauthenticationPageTests` 34/34; `CustomerSignInPageTests` + `SessionAccessTests` (girişsiz yönlendirme) 60/60; SecurityHeaders, Forgery, WritingEndpointAudit, SitemapEndpoint, PageFormPattern, FormComponent 84/84. **TAM: CI** (sahip kararı 2026-11-12: tam Integration koşusunu CI'ın Integration işi yapar) |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes` | Web, IntegrationTests, UnitTests temiz (derleme `-warnaserror` temiz) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T516-sifre-sifirlama`
- **Commit:** `5c3fdb84`
- **PR:** #564
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (Google ağ geçidi testlerde taklit; gerçek Google çağrılmaz) | `StubGateway` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | E-23 metinlerinin (nötr cümle dışında) tam sözü `04`'te yok | D-1416 | sahip kararı |
| 2 | E-24 cümlelerinin tam sözü `04`'te yok | D-1417 | sahip kararı |
| 3 | `VAL-B078` 320 genişlik tarayıcı gözlemi | D-1418 | F5 kapısı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** devam işçisi; yönetici izler
- **Repo memory (bitiş kapısı 9):** parça dosyalarıyla güncellendi (STATUS tablo satırı, `STATUS_CHANGELOG.d/T516.md`, `DEFERRED_BACKLOG.d/T516.md`)
- **⚠ (kolay, kararla):** sayfa metinlerinin `04` 5.23 ve 5.24'te birebir sözü bulunamadı; nötr istek cümlesi `02 §3.13.11`'in sözüdür, kalanı panelin ve giriş ekranının sesiyle yazıldı (D-1416, D-1417). `12` Durum hücreleri doğrulayıcıya bırakıldı.
