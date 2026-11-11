# T514 — Kayıt ve doğrulama bağlantısının inişi (E-20, E-21)

**Faz:** F5 | **Durum:** Doğrulama tur 1 ✗ FAIL, düzeltme bekliyor | **Tarih:** 2026-11-11

---

## Yapılan İşler

- Kayıt sayfası `Pages/Storefront/Register.cshtml(.cs)`: `GET /kayit` ad, e-posta, şifre formunu çizer (Google düğmesi yok, T515); kapı kapalıyken form yerine cümle. `Register` (UC-48) doğrulanmamış bekleyen kaydı açar, doğrulama e-postası satırı aynı işlemde yazılır (D-252), 303 `/kayit?pendingRegistration=...`; oturum açılmaz. `ResendVerificationLink` (UC-49, `/kayit`) yeni bağlantı gönderir.
- Doğrulama sayfası `Pages/Storefront/Verification.cshtml(.cs)`: `GET /dogrulama/{anahtar}` yalnız okur (`ReadEmailLinkAsync`), altı hâlden birini çizer; `POST VerifyEmail` bağlantı türüne göre tüketir, 303 sonuç koduyla döner, oturum açmaz. `[NoStore]`, Referer göndermez.
- Üye modülü: `IMembershipModule.ReadPendingRegistrationAsync` + `PendingRegistrationReading`; `RegisterInput.RetryOnUniqueIndexes` (aynı e-postayla eş zamanlı iki kayıtta tekil dizin ihlali yeniden denenir).
- Bileşen: `FieldKind.Concealed` + `Fields.Concealed` (şifre alanı, değeri hiç taşımaz); `FramePaths.Register`, `FramePaths.Verification`; `RegistrationTexts`, `VerificationLandings` (`Pages/Components/RegistrationTexts.cs`).
- Form modeli adları `07 §5.1.5`'e göre: `RegisterInput` (UC-48) ve `ResendVerificationLinkInput` (UC-49); `Password` ve `PendingRegistration` `07`'nin adlarıdır (`[SpecAssignedName]`).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Storefront/{Register,Verification}.cshtml(.cs)`, `Pages/Components/{RegistrationTexts,FieldModels}.cs`, `Pages/Frame/FramePaths.cs`, `Modules/Membership/{MembershipModule.cs,Contracts/IMembershipModule.cs,Contracts/MembershipRecords.cs}`, `tests/Shopfolio.IntegrationTests/Web/RegistrationPageTests.cs`, `tests/Shopfolio.UnitTests/Pages/RegistrationTextsTests.cs`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /kayit` formu; kapalı kapıda form kapalı; Google düğmesi T515'te | ✓ | `RegistrationPageTests.TheForm_HasTheThreeFields_NoValueInTheSecretField_AndNoGoogleButton_T514_KK1`, `WhileTheGateIsClosed_TheFormIsReplacedByTheSentence_HasNoButton_AndTheSendWritesNothing_T514_KK1` |
| 2 | Gönderim sonrası hâli koda ve `pendingRegistration`'a bağlı; değer şifreli, kayıtta tutulmaz | ✓ | `TheAfterSendState_IsReadFromTheValue_AndTheValueIsNotStoredAnywhere_T514_KK2`; `RegistrationTextsTests.AConcealedField_HidesWhatIsTyped_AndNeverHoldsAValue_KK2` |
| 3 | `Register` bekleyen kaydı açar; Z-1 bağlantı; e-posta satırı aynı işlemde; oturum yok; 303 `/kayit` | ✓ | `Register_OpensAnUnverifiedAccount_ALinkOfZ1_AndTheEmailRowInTheSameTransaction_AndNoSession_T514_KK3` |
| 4 | 422: ad boş, e-posta geçersiz, şifre P-17 altı/yaygın, e-posta kayıtlı, kapı kapalı; 429 (L-3) | ✓ | `ARefusedField_Answers422_...` (Theory), `AnAddressOfAnAccount_PendingOrVerified_Answers422_...`, `WhileTheGateIsClosed_...`, `PastTheLimitL3_TheAnswerIs429_WithTheNeutralSentence_AndNoRetryAfter_T514_KK4` |
| 5 | Tekrar gönderim 422; ikinci kayıt açılmaz | ✓ | `ASecondSend_OfTheSameForm_IsRefused_AndOpensNoSecondRegistration_T514_KK5` |
| 6 | `ResendVerificationLink` yeni bağlantı; geçersizde göndermez; öncekiler `SupersededAt`; ömür uzamaz; 429 (L-9) | ~ `/kayit` ✓; `/giris` T515 | `TheResend_SendsANewLink_SupersedesTheEarlierOnes_AndDoesNotLengthenTheRegistration_T514_KK6`, `TheResend_OfAValueOrARegistrationThatIsNotPending_SendsNothing_T514_KK6`, `PastTheLimitL9_...`; `/giris` sayfası yok (T515): D-1249, hedef T515 ⚠ |
| 7 | `GET /dogrulama/{anahtar}` altı hâl; açılış doğrulamaz | ✓ | `OpeningARegistrationLink_ShowsTheStepAndTheButton_AndWritesNothing_T514_KK7`, `TheOtherStates_...`, `AResultCode_OnAnAddressThatWasNotUsed_TellsNothing_T514_KK7`, `TheLandingPage_SendsNoReferrer_AndIsNotStored_T514_KK7`; `RegistrationTextsTests` (altı hâl, ResultCode yalnız Used'da) |
| 8 | Başka tür / yönetici bağlantısı tanınmaz, tüketilmez | ✓ | `AnotherTypesLink_AndAnAdministratorsLink_AreNotRecognized_AndNothingIsConsumed_T514_KK8` |
| 9 | `VerifyEmail` tek koşullu tüketim, 303 kodla, oturum yok; `EmailVerifiedAt`, misafir siparişleri bağlanır; Google teyidinde `GoogleLinkedAt` | ✓ | `VerifyEmail_ConsumesTheLinkOnce_Answers303WithTheCode_OpensNoSession_AndBindsTheGuestOrders_T514_KK9`, `TheGoogleConfirmationButton_LinksTheAccount_Answers303_AndOpensNoSession_T514_KK9` |
| 10 | UC-53: o arada geçersizse 200; adres başka hesapta doğrulandıysa 422 + tüketim döner; değilse bekleyen kayıt silinir | ✓ | `TwoPressesAtOnce_ConsumeTheLinkOnce_T514_KK10`, `TheNewAddressButton_ChangesTheAddress_DeletesThePendingRegistrationOfTheAddress_AndAnswers303_T514_KK9_KK10`, `TheNewAddressButton_OnAnAddressThatBecameAnotherAccounts_Answers422_ReturnsTheConsumption_AndChangesNothing_T514_KK10` |

Kriter 6'nın `/giris` yarısı: `/giris` sayfası T515'in inişidir (T515 k5 "yeniden gönderim T514'tedir" der); işleyici mantığı `/kayit` sayfasında yazıldı ve D-1249 ile T515'e bağlandı. Kısmi karşılama sayılırsa plan düzeltmesi gerekir; yönetici kararı.

### Öz denetim (bitiş kapısı)

Önceki işçinin kodu dahil bütün kriterler ve KK'ler diff'te denetlendi. İlk koşuda iki kırmızı bulundu ve düzeltildi: model adları `07 §5.1.5`'e uymuyordu (`RegistrationFormInput` → `RegisterInput`, `[SpecAssignedName]`), üç yerde `!` işareti gerekçesiz idi (kaldırıldı). `sik-fail-nedenleri.md` maddeleri: (1) yeni tarama istisnası eklenmedi; (2) yeni kaynak taraması yok, `LinkKeySourceScan` (`VerifyEmail` yalnız `OnPost…`) ve sır bekçisi mevcut haliyle yeşil; (3) şema değişmedi; (4) `FieldKind` eklendi, değerleri sayan ayrı bir yer Unit koşusunda kırılmadı; (5) belge kuralı değişmedi, karar kaydı gerekmedi.

| KK | Sonuç | Kanıt |
|---|---|---|
| KK1 kapalı kapıda düğme yok | ✓ | kriter 1 |
| KK2 değer kayıtta yok | ✓ | kriter 2 |
| KK3 bekleyen kayıt, Z-1, oturum yok | ✓ | kriter 3 |
| KK4 beş 422 sebebi ve 429 | ✓ | kriter 4 |
| KK5 ikinci kayıt açılmaz | ✓ | kriter 5 |
| KK6 eski bağlantı geçersiz, ömür sabit | ✓ | kriter 6 |
| KK7 altı hâl; açılış yazmaz | ✓ | kriter 7 |
| KK8 yabancı tür tüketilmez | ✓ | kriter 8 |
| KK9 tek tüketim; sipariş bağlama | ✓ | kriter 9 |
| KK10 yarış; başka hesap; kayıt silme | ✓ | kriter 10 |
| `09 §25` satırları; güvenlik taraması | ✓ | şifre alanı değer taşımaz ve günlüğe girmez (`NotTrimmed`/`PasswordField`); 429 nötr, `Retry-After` yok; yanıtlar `[NoStore]`, doğrulama sayfası Referer göndermez; GET yazmaz; yazan uçlar sahtecilik denetimli; yeni bağımlılık, sır, şema yok |

`12`: bu task için ayrı VAL satırı yok; D-801'in T514 payı (429 uç yanıtı: `PastTheLimitL3_...`, `PastTheLimitL9_...`) kanıtlandı.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | tam koşu 7154: 2 kırmızı (model adı, `!`) düzeltildi; ilgili sınıflar (`ModelBindingRuleTests`, `NullForgivingSourceScanTests`, `RegistrationTextsTests`) 72/72 yeşil |
| Integration | ✓ | `--filter-class`, Postgres 18 (`track_commit_timestamp=on`) | `RegistrationPageTests` 25/25; sayfa/uç tarayan sınıflar (Forgery, FormComponent, PageFormPattern, SecurityHeaders, SessionAccess, SitemapEndpoint, WritingEndpointAudit) 120/120. Tam Integration süre bütçesi yüzünden koşulmadı (doğrulayıcı birleşik ağaçta koşar) |
| Biçim | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, Unit temiz; Integration koşulmadı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-11) |
| Bulgu sayısı | 2 (S2 ×1, S1 ×1); K-notu 3 |
| Düzeltme gerekli mi | Evet — aynı PR'da |

**Tur 1 bulguları (bağımsız doğrulama):**

1. **S2 — kırmızı Integration testi.** `MembershipTests.TheFormAndTheAccount_HaveNoMarketingConsentField` (`tests/Shopfolio.IntegrationTests/Model/MembershipTests.cs:243`) kırmızı: `typeof(RegisterInput).GetProperties()` artık `["Name", "Email", "Password", "RetryOnUniqueIndexes"]` verir. Dalın `Modules/Membership/Contracts/MembershipRecords.cs`'teki `RegisterInput`'a eklediği statik `RetryOnUniqueIndexes` formun "yalnız üç değer" sınamasını kırdı. Yapım Integration'ı TAM koşmadığı için görülmedi. Düzeltme: dizin adı formun kaydından çıkarılır (ayrı sabit) ya da sınama yalnız örnek özelliklerini sayar — sınamanın amacı (formda pazarlama onayı yok, `02 §3.13.18`, §3.13.21) korunarak.
2. **S1 — `07 §4.20.1`, §4.20.3 sapması.** UC-49'da değer çözülmüyor, kayıt bekleyen kayıt değil ya da bağlantının ömrü dolmuşsa belge "303 geldiği sayfaya **değersiz** döner — form boş hâliyle" der; `RegisterModel.GoneAddress` 303'ü değerle verir ve GET bu değerde formu belgede olmayan bir cümleyle (`RegistrationTexts.NoLongerPending`, "Bu kayıt artık beklemede değil…") çizer; aynı cümle UC-47'de çözülmeyen değerde de çıkar (belge: "kod yok sayılır ve form boş hâliyle gelir"). Kaynak gösterilen `04` 5.20.12 bu cümleyi yazmaz. Düzeltme: 303 değersiz; form ek cümlesiz (ya da cümle önce belgeye yazılır).

**K-notları:** K1 yapım raporu "`12`: bu task için ayrı VAL satırı yok" der; KP-25 satırları (`VAL-B020`, A090, C026, C027) ve `07`'nin uç satırları (A492, A559, A676, A678, A679…) vardır — bu raporla kapandı. K2 plan metni T514 kriter 6 ile T515 kriter 5 arasında `/giris` yüzünü birbirine bırakıyor → D-1277 (T515). K3 düzeltme turunda `VAL-A677` (süresi dolmuş ve yenilenmiş bağlantıya POST) ve `VAL-A920` (GET'ten sonra misafir siparişi bağlanmamış) uç kanıtı eklenebilir; satırlar BEKLEMEDE kaldı → bu PR'da (düzeltme turu).

**`12` Durum:** `VAL-B020`, `VAL-A492`, `VAL-A559`, `VAL-A676`, `VAL-A679` → PASS. Ayrıntı: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/impl/T514/dogrulama-raporu-r1.md`.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok
- **Ortak altyapı:** `FieldKind.Concealed`; `IMembershipModule.ReadPendingRegistrationAsync`

## ⚠ Kararlar

- `/giris` tarafındaki `ResendVerificationLink` T515'e bırakıldı (D-1249).
- Model adları `07 §5.1.5`'e uyarlandı; üye modülünün `RegisterInput` kaydıyla çakışma, sayfada `MembershipRegisterInput` takma adıyla çözüldü.
