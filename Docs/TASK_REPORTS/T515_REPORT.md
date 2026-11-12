# T515 — Müşteri girişi ve Google ile giriş (E-22)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-12

---

## Yapılan İşler

- Giriş sayfası `Pages/Storefront/SignIn.cshtml(.cs)`: `GET /giris` e-posta, şifre, "beni hatırla" formunu; Google düğmesini (yalnız Google tanımlıysa) ve aydınlatma bağlantısını; "şifremi unuttum" ve kayıt bağlantılarını çizer. Kapı kapalıyken giriş çalışır. `SignIn` (UC-55) her girişte yeni oturum anahtarı açar (işaretliyse Z-5, değilse Z-4), misafir sepetini birleştirir (`GuestCartMerge`, çerez silinir), dönüş yerine 303 verir. Tek mesaj (hesabı ele vermez), 422 alan hataları, 429 (L-1) nötr; doğrulanmamış hesapta oturum açılmaz, doğrulama beklenir. `ResendVerificationLink` işleyicisi (`/giris`) T514 mantığını `PendingRegistrations` ortak yerinden çağırır (D-1249).
- Google akışı: `StartSocialLogin` (UC-50) akış çerezine `state`, `nonce`, PKCE doğrulayıcısı ve dönüş yeri yazar, 303 → Google; Google tanımsızsa 422. `Pages/Storefront/GoogleReturn.cshtml(.cs)` (`GET /giris/google`, UC-51): akış değerleri bir kez kullanılır (`GoogleSignInFlow`, `Infrastructure/Integrations`); eşleşen hesaba girer ve bağ yazar; eşleşme yoksa kapı açıkken şifresiz doğrulanmış hesap açar (bekleyen kayıt silinir); yetkisizde `GoogleLinkConfirmation` bağlantısı kuyruğa girer, oturum açılmaz; doğrulanmamış e-posta 303 → `/kayit` (kodla); kapı kapalıyken yeni hesap 303 → `/giris` (kodla); yenilenen dönüş, Google hatası, vazgeçme, bozuk durum yazmadan 303 → `/giris` (`SocialLoginFailed`).
- Sıra ve kilit: Google dönüşünde hesap kaydedilir, sonra oturum açılır, sonra yeni hesabın misafir siparişleri bağlanır (D-577); sıra `LockSequence` (05 §7.3.5: hesap başta) ile uyumludur. İlk sayfa testi 500 verince bulundu ve düzeltildi.
- Dönüş yeri: `ReturnPath` (yerel yol denetimi; giriş, kayıt, sıfırlama, iniş ekranları ve eksik değer → ana sayfa). Metinler `SignInTexts` (`ResultOf`, `RegisterMessageOf`); `FramePaths.GoogleReturn`; `FormResults` yeni yardımcılar.
- Plan hizalaması (D-1277): `11` T514 kriter 6 `/kayit` yüzüyle sınırlandı, T515 kriter 5'e `/giris` yüzü yazıldı. Örnek `/giris` sayfası (`SignInSample`) ve onun erişim testi kaldırıldı (gerçek sayfa geldi).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Storefront/{SignIn,GoogleReturn,Register}.cshtml(.cs)`, `Pages/Storefront/{GuestCartMerge,PendingRegistrations}.cs`, `Pages/Components/SignInTexts.cs`, `Pages/Frame/FramePaths.cs`, `Infrastructure/{LockSequence.cs,Forms/FormResults.cs,Integrations/GoogleSignInFlow.cs,Integrations/GoogleRegistration.cs,Security/ReturnPath.cs,Security/PasswordRegime.cs,Security/PasswordResults.cs}`, `Modules/Membership/{GoogleSignIns.cs,MembershipModule.cs,Contracts/MembershipRecords.cs}`; testler `tests/Shopfolio.IntegrationTests/Web/{CustomerSignInPageTests,SessionAccessTests}.cs`, `Ordering/CartTests.cs`, `Pages/AccessSamples.cs`; `tests/Shopfolio.UnitTests/Infrastructure/{GoogleSignInFlowTests,ReturnPathTests}.cs`, `Pages/SignInTextsTests.cs`. Belgeler: `Docs/11_IMPLEMENTATION_PLAN.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/DEFERRED_BACKLOG.d/{T514,T515}.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.d/T515.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /giris` form, Google, şifremi unuttum, kayıt bağlantıları; Google tanımsızsa düğme yok; kapı kapalıyken giriş çalışır | ✓ | `CustomerSignInPageTests.TheForm_HasTheFieldsAndTheLinks_AndNoGoogleButtonWhileGoogleIsUndefined_T515_KK1`, `WhenGoogleIsDefined_TheButtonIsThere_T515_KK1`, `WhileTheGateIsClosed_TheSignInStillWorks_T515_KK1` |
| 2 | `returnUrl` yerel yol denetimi; giriş/kayıt/sıfırlama/iniş ya da yoksa ana sayfa | ✓ | `TheReturnPlace_IsALocalPageOrTheHomePage_T515_KK2`; `ReturnPathTests.AfterSignIn_KeepsAPageOfTheSite_AndAnswersTheHomePageForAnEntryScreenOrForeignOrMissingValue`, `IsLocalPath_*` |
| 3 | Her girişte yeni oturum anahtarı, sepetler birleşir, Z-5/Z-4 | ✓ | `EverySignIn_OpensANewSessionKey_ShortOrLongByTheBox_AndWritesTheMark_T515_KK3`, `ASignIn_WithAGuestCartCookie_DeletesThatCookie_T515_KK3`; modül: `CartTests.AGuestAdd_ThatWaitsForTheSignInMerge_...D619` |
| 4 | 422 boş/biçim; tek mesaj; sahte özet; 429 (L-1) nötr, hesap kilitlenmez; tanınan tarayıcı engellenmez | ✓ | `AWrongPassword_AndAnUnknownAddress_GiveTheSameSentenceAnd422_AndOpenNoSession_T515_KK4`, `AnEmptyOrMalformedField_Gives422WithoutTheWrongPairSentence_T515_KK4`, `TooManyWrongPasswordsOfOneAddress_Give429WithoutLockingTheAccount_AnotherAddressStillSignsIn_T515_KK4`; `SignInTextsTests.TheWrongCredentialsSentence_SaysNeitherWhichOneNorWhetherTheAccountExists` |
| 5 | Doğrulanmamış hesapta oturum açılmaz; yeniden gönderim | ✓ | `TheRightPairOfAnUnverifiedAccount_OpensNoSession_AndTellsTheAwaitedVerification_T515_KK5`, `TheRightPairOfAnAccountWhoseLinkEnded_IsToldAsAWrongPair_T515_KK5`, `TheResendOfTheSignInPage_SendsANewLink_AndSupersedesTheEarlierOnes_D1249_T515_KK5` (bu turda eklendi) |
| 6 | `StartSocialLogin` 303 → Google; çerez `state`, `nonce`, PKCE; tanımsızsa 422 | ✓ | `StartSocialLogin_Gives303ToGoogle_WithStateNonceAndTheProofKey_AndTheFlowCookie_T515_KK6`, `StartSocialLogin_WhileGoogleIsUndefined_Gives422_AndNoFlowCookie_T515_KK6`; `GoogleSignInFlowTests.Start_WritesTheFlowCookieAndAnAddressWithStateNonceAndTheProofKey` |
| 7 | Dönüş: eşleşen hesaba giriş + bağ; eşleşme yoksa şifresiz hesap, bekleyen kayıt silinir; yetkisizde teyit bağlantısı, oturum yok | ✓ | `TheReturn_OfAVerifiedAddressOfAnExistingAccount_SignsIn_AndLinksWhenGoogleIsTheAuthority_T515_KK7`, `TheReturn_OfAnAddressWithNoAccount_WhileTheGateIsOpen_OpensAnAccountWithoutPassword_AndSignsIn_T515_KK7`, `..._OfANewAddress_BindsTheGuestOrdersOfThatAddress_D577_T515_KK7`, `..._OfAnAddressWithAPendingRegistration_WhileTheGateIsOpen_LeavesOneVerifiedAccount_T515_KK7`, `TwoFirstReturns_..._D556_T515_KK7`, `..._OfAnUnlinkedAccountWhereGoogleIsNotTheAuthority_OpensNoSession_AndQueuesTheConfirmationLink_T515_KK7` |
| 8 | Doğrulanmamış e-posta 303 → `/kayit`; kapı kapalıyken yeni hesap 303 → `/giris` kodla | ✓ | `TheReturn_OfAnUnverifiedGoogleAddress_Goes303ToTheRegistration_WithTheCode_AndWritesNothing_T515_KK8`, `TheReturn_OfANewAddressWhileTheGateIsClosed_Goes303ToTheSignIn_WithTheCode_AndWritesNothing_T515_KK8`; `SignInTextsTests.RegisterMessageOf_...` |
| 9 | Akış değerleri bir kez; yenilenen dönüş, hata, vazgeçme yazmadan 303 → `/giris` | ✓ | `ARenewedReturn_HasNoFlowCookie_CallsNoGoogle_AndWritesNothing_T515_KK9`, `AReturnWithAnErrorOrNoCode_CallsNoGoogle_AndWritesNothing_T515_KK9`, `AWrongNonce_AFailedVerification_OrAnUnreachableGoogle_AreTheSameEnding_AndWriteNothing_T515_KK9`, `ADamagedState_IsTheSameEnding_AndOpensNoSession_T515_KK9`; `GoogleSignInFlowTests.Consume_*` (9 test) |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin kodu dahil kriter 1–9 ve KK1–KK9 diff'te yeniden denetlendi (yalnız kalanlar değil). Bu turda bulunan eksik: `/giris` yeniden gönderim işleyicisi (D-1249) için sayfa testi yoktu; eklendi. Önceki turda bulunan gerçek hata: Google dönüşünde sipariş bağlama oturumdan önce çağrılıyordu (kilit sırası ihlali, 500); düzeltildi ve D-577 testi eklendi. `sik-fail-nedenleri.md` okundu:

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–9, KK1–KK9 | ✓ | yukarıdaki tablo; sınıf `CustomerSignInPageTests` 33/33 (bu tur 32/32 + yeni test) |
| (1) Bekçi/tarama istisnası geniş | ✓ | yeni istisna eklenmedi; `SignInSample` ve erişim örneği kaldırıldı; tarama sınıfları 84/84 |
| (2) Kaynak taraması çağrı biçimi | ✓ | yeni tarama yok; Google doğrulaması akışta (`GoogleSignInFlow.VerifyAsync`), anahtar üretimi `SecretKey`; Unit tarama testleri TAM yeşil |
| (3) 06 "Varsayılan"/FK dizini | ✓ | şema ve göç değişikliği yok |
| (4) Kural değişince aynı sayıyı anan yerler | ✓ | `11` T514 k6 ve T515 k5 hizalandı (D-1277); `AccessSamples`/`SessionAccessTests` örnek giriş sayfası kalktı, ilgili sınıflar 56/56 |
| (5) Belge kuralını değiştiren 09 metni | ✓ | `09` değişmedi; `05 §7.3.5` notu D-1353 ile sahibe |
| Eşzamanlılık / kilit sırası (09 §15.1) | ✓ | `LockSequence` hesap başta; `TwoFirstReturns_..._D556_T515_KK7`, D-577 testi |
| Biçim (`dotnet format`) | ✓ | Web, IntegrationTests (bu turda yeni test sonrası yeniden), UnitTests temiz |
| `09 §25` / güvenlik | ✓ | `[NoStore]` sayfalar; GET yazan tek uç `/giris/google` (UC-51, `WritingEndpointAudit` yeşil); akış çerezi bir kez kullanılır; hata hâlleri aynı sonla biter (hesap/akış durumu sızdırmaz); tek mesaj; 429 nötr; yeni bağımlılık, sır, şema yok |

`12`: VAL-A351, VAL-A607, VAL-C075 Durum hücreleri doğrulayıcının işidir, dokunulmadı (BEKLEMEDE); kanıt testleri: A351/C075 `TooManyWrongPasswordsOfOneAddress_...KK4`; A607 metni `SignInTexts.SessionClosed` (D-1349).

Backlog payı: D-81, D-556, D-558, D-577, D-619, D-1070 ✅ (yerinde kapatıldı); D-1249, D-1277 ✅ (`DEFERRED_BACKLOG.d/T514.md`); pay satırları: D-254, D-557 (D-1352), D-572, D-801 (429 uç yanıtı kanıtı: KK4), D-806, D-109 payı raporda. Yeni: D-1349–D-1353 (`DEFERRED_BACKLOG.d/T515.md`).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | TAM 7462/7462 (main a56a1735 birleşimi sonrası; sonraki değişiklik yalnız `GoogleReturn` sıra düzeltmesi, ardından ilgili sınıflar yeşil) |
| Integration | ✓ (parçalı) | `--filter-class`, Postgres 18 (`track_commit_timestamp=on`) | `CustomerSignInPageTests` 32/32 + yeni test 1/1; `SessionAccessTests` + `Ordering.CartTests` 56/56; Forgery, FormComponent, PageFormPattern, SecurityHeaders, SitemapEndpoint, WritingEndpointAudit 84/84. **Integration TAM doğrulayıcıda koşulacak** (yönetici kararı: yük altında devam işçisi TAM koşmaz) |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes` | Web, IntegrationTests, UnitTests temiz |

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

- **Branch:** `task/T515-musteri-girisi`
- **Commit:** PR açıklamasındaki son commit
- **PR:** PR açıklamasında
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (Google ağ geçidi testlerde taklit; gerçek Google çağrılmaz) | `StubGateway` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Dört cümlenin tam metni `04`'te yok | D-1349 | sahip kararı |
| 2 | Google girişi "beni hatırla"sız kısa oturum | D-1350 | sahip kararı |
| 3 | T516 yeniden doğrulama dalı (`kind != SignInOrRegistration`) | D-1351 | T516'nın işi |
| 4 | `07` UC-51/UC-52 sonuç tablosu | D-1352 | belge, sahip |
| 5 | `05 §7.3.5` kilit sırası notu | D-1353 | belge, sahip |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** devam işçisi; yönetici izler
- **Repo memory (bitiş kapısı 9):** parça dosyalarıyla güncellendi (STATUS tablo satırı, `STATUS_CHANGELOG.d/T515.md`, `DEFERRED_BACKLOG.d/T515.md`)
- **⚠ (kolay, kararla):** kriter 5 "yeniden gönderim T514'tedir" plan metni `/giris` işleyicisini T515'e aldı (D-1277); `12` Durum hücreleri doğrulayıcıya bırakıldı.
