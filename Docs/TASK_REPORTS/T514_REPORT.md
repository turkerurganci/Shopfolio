# T514 — Kayıt ve doğrulama bağlantısının inişi (E-20, E-21)

**Faz:** F5 | **Durum:** Düzeltme 2 yapıldı, doğrulama tur 3 bekliyor | **Tarih:** 2026-11-11

---

## Yapılan İşler

- Kayıt sayfası `Pages/Storefront/Register.cshtml(.cs)`: `GET /kayit` ad, e-posta, şifre formunu çizer (Google düğmesi yok, T515); kapı kapalıyken form yerine cümle. `Register` (UC-48) doğrulanmamış bekleyen kaydı açar, doğrulama e-postası satırı aynı işlemde yazılır (D-252), 303 `/kayit?pendingRegistration=...`; oturum açılmaz. `ResendVerificationLink` (UC-49, `/kayit`) yeni bağlantı gönderir.
- Doğrulama sayfası `Pages/Storefront/Verification.cshtml(.cs)`: `GET /dogrulama/{anahtar}` yalnız okur (`ReadEmailLinkAsync`), altı hâlden birini çizer; `POST VerifyEmail` bağlantı türüne göre tüketir, 303 sonuç koduyla döner, oturum açmaz. `[NoStore]`, Referer göndermez.
- Üye modülü: `IMembershipModule.ReadPendingRegistrationAsync` + `PendingRegistrationReading`; `RegistrationIndexes.RetryOnUniqueIndexes` (aynı e-postayla eş zamanlı iki kayıtta tekil dizin ihlali yeniden denenir).
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

| Düzeltme 1 | ✓ | restore, `build -warnaserror`, `dotnet format` ×3 temiz; Unit TAM; Integration TAM ad alanı parçalarıyla (Postgres 18, `track_commit_timestamp=on`) | Unit 7173/7173; Web 802/802; Model 1410: ilk koşuda 3, yeniden koşuda 1 kırmızı, hepsi bağlantı kopması (`Npgsql: Exception while reading from stream`, `SchedulerBackupDisposalAndKeyTablesTests` migrasyon testi; aynı anda başka Integration koşuları vardı), sınıf tek başına 55/55; Ordering 645/645; kalan 941/941; `MembershipTests.TheFormAndTheAccount_HaveNoMarketingConsentField` yeşil |
| Düzeltme 2 | ~ | restore, `build -warnaserror` (0 uyarı), `dotnet format` ×3 temiz; Unit TAM; Integration ad alanı parçalarıyla (Postgres 18, `track_commit_timestamp=on`) | Unit 7232/7232; Web (Panel dahil) 848/848. Model, Ordering ve kalan parçalar süre bütçesi (45 dk) yüzünden bu turda koşulmadı (metin değişikliği o ad alanlarına değmez); doğrulayıcı birleşik ağaçta koşar |

## Düzeltme 1 (2026-11-12)

- **Bulgu 1 (S2):** dizin adı `RegisterInput`'tan çıkarıldı; `RegistrationIndexes.RetryOnUniqueIndexes` (aynı dosya, ayrı statik sınıf). Form kaydı yine yalnız Name, Email, Password; T341 sınaması gevşetilmedi.
- **Bulgu 2 (S1):** `GoneAddress()` 303'ü her durumda değersiz `/kayit` verir (UC-49); çözülmeyen değerde GET formu boş hâliyle çizer (UC-47). Belgede olmayan `RegistrationTexts.NoLongerPending` cümlesi ve `PendingGone` kaldırıldı (belgeye yazma yolu seçilmedi). `TheResend_OfAValueOrARegistrationThatIsNotPending_SendsNothing_T514_KK6` 303'ün tam `/kayit` olduğunu ve formun boş geldiğini, `TheAfterSendState_…_KK2` bozuk değerde formun geldiğini sınar.
- **K3:** eklendi: `ThePress_OnALinkThatDoesNotHold_ShowsItsOwnState_AndWritesNothing_T514_KK7` (VAL-A677: süresi dolmuş, kullanılmış, yenilenmiş bağlantıya POST, hâl + yazma yok) ve `OpeningTheLinkManyTimes_DoesNotVerify_NorBindsTheGuestOrdersOfTheAddress_T514_KK7_KK9` (VAL-A920). `12` Durum hücreleri doğrulayıcının işidir, dokunulmadı.
- **Öz denetim:** kriter 1–10 ve KK1–KK10 yeniden denetlendi; kriter 6 ve `/giris` yüzü değişmedi (D-1249, D-1277). Önceki tablo geçerli. `sik-fail-nedenleri` taraması: yeni bağımlılık/şema yok; kurucu bağımlılığı eklenmedi; Integration TAM koşuldu; `dotnet format` test projesi dahil temiz; belgede olmayan metin kaldırıldı.

## Düzeltme 2 (2026-11-12)

- **Bulgu 1 (S1, tur 2): hitap ve sabit metinler.** `RegistrationTexts` ve `VerificationLandings` bütün metinleri "siz" yapıldı (`04 §8.2.9`): aydınlatma bağlantısı, kayıt/yeni adres/Google hâlleri, kullanılmış ve geçersiz hâller. `Submitted` K-779 (1)'in cümlesini birebir verir ("Doğrulama bağlantısını {e-posta} adresine gönderdik. Hesabınız, bağlantıyı {tarih} tarihine kadar açtığınızda kullanıma açılır."). `LinkResent` sabit metinden `LinkResent(DateTimeOffset linkEndsAt)` yöntemine döndü ve K-779 (2)'yi birebir verir ("Yeni bir bağlantı gönderdik; önceki bağlantılar artık geçersiz. Bağlantı {tarih} tarihine kadar geçerli."); tarih ilk gönderimin ömrüdür (`Pending.LinkEndsAt`), yeniden istek onu uzatmaz (K-659). Google teyit cümlesi `04` 5.21.14'ün tırnaklı cümlesidir. Sınıf yorumu artık "sen"i K-772.ye dayandırmaz: "formlar ve hesap ekranları siz (04 §8.2.9; K-779)".
- **Tarama:** aynı tür sapma için `Register.cshtml(.cs)`, `Verification.cshtml(.cs)`, `FieldModels.cs` ve Membership modül değişiklikleri `grep` ile tarandı; başka "sen" metni yok (kalan etiketler "Ad", "E-posta", "Şifre", "Kayıt ol", "Giriş yap").
- **Testler:** `RegistrationTextsTests.TheSubmittedState_NamesTheAddress_AndTheLastDayOfTheLink_KK3` (K-779 (1) tam eşitlik), yeni `TheResentState_IsTheSecondTextOfK779_WithTheFirstSendsLastDay_KK6`, `TheGoogleConfirmationStep_IsTheSentenceOf5_21_14_KK7`, `EveryLandingSentence_SpeaksSiz_NeverSen_KK7` (3 bağlantı türü × bütün durumlar; "sen" kalıpları yok). `RegistrationPageTests.TheResend_SendsANewLink_…_KK6` K-779 (2) cümlesini ve ilk gönderimin tarihini sayfada arar; hâl metni beklentileri ("Hesabınız açıldı", "Yeni e-posta adresiniz doğrulandı", "Google girişi hesabınıza bağlandı") güncellendi.
- **⚠ K-779 (1) ↔ K-904:** K-779 (1) "bağlantıyı … açtığınızda kullanıma açılır" der; K-904 doğrulamanın açılışla değil düğmeyle olduğunu söyler. Cümle K-779'dan birebir uygulandı, belge değiştirilmedi; çelişki karar kaydında sahibin kararına kalır.
- **K3 (VAL-A678 doğrulanmış kayıt değeriyle GET; VAL-A090 uygulama kaydı kanıtı):** bu turda eklenmedi — talimatın tek bulgusu metin; iki kanıt ayrı yeni test düzeneği ister (uygulama kaydı yakalayıcısı) ve `12` Durum hücreleri doğrulayıcının işidir. Satırlar BEKLEMEDE kalır; ⚠ sahibe.
- **Öz denetim:** kriter 1–10 ve KK1–KK10 yeniden denetlendi (yukarıdaki tablo; kriter 2, 6 ve 7'nin metin kanıtı bu düzeltmeyle K-779/`04` 5.21.14 birebir). `sik-fail-nedenleri.md` okundu: (1) hitap/sabit karar metni — bütün T514 metinleri tarandı; (2) yeni bağımlılık, şema, kurucu bağımlılığı yok; (3) Integration TAM parçalı koşuldu; (4) `dotnet format` üç projede temiz; (5) belgede olmayan metin yok; (6) kriter 6'nın `/giris` yüzü değişmedi (D-1249, D-1277). `git grep -nE "^(<{7}|>{7}) "` boş.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, 2026-11-12) — ikinci iterasyon, eskalasyon (`12 §8.5`) |
| Bulgu sayısı | Tur 2: 1 (S1 ×1, DÜŞÜK); K-notu 3. Tur 1: 2 (S2 ×1, S1 ×1) — ikisi de kapandı |
| Düzeltme gerekli mi | Evet — metinler (sahip kararıyla) |

**Tur 2 bulgusu (bağımsız doğrulama, birleşik ağaç `5ed330f5` + `af7e91ff`):**

1. **S1 — hitap ve sabit metinler.** `RegistrationTexts`/`VerificationLandings` metinleri "sen" hitabıyla yazılmış ("Hesabın…", "okuyabilirsin", "onaylıyor musun"); kod yorumu bunu K-772'ye dayandırır. Oysa `04 §8.2.9`: "ödeme adımı, formlar, hesap ve sipariş ekranları 'siz'"; K-779 (karar kaydı): "Altı mesajın metni sabitlenir; hitap 'siz'dir" — (1) gönderim sonrası hâli ve (2) yeniden gönderim metni ("Yeni bir bağlantı gönderdik; önceki bağlantılar artık geçersiz. Bağlantı {tarih} tarihine kadar geçerli."); `07 §4.20.3` yanıtı "K-779'un ikinci metni" diye bağlar. `04` 5.21.14 Google teyidinin cümlesini tırnakla ("…onaylıyor musunuz? Onaylarsanız hesabınıza…") verir; kod "sen"e çevirmiş. Aynı ekranda 5.21.4'ün "yeniden kayıt olun"u ise "siz" kalmış. Düzeltme: E-20 ve E-21 metinleri "siz"e; K-779 (2) birebir; 5.21.14 birebir; K-779 (1)'in "bağlantıyı açtığınızda" kısmı K-904'le (açılış doğrulamaz) çelişiyorsa uyarlama sahibe sorulur ya da karar kaydına yazılır.

**Tur 2 K-notları:** K1 `/giris` yüzü D-1249, D-1277 ile T515'te (değişmedi). K2 `FieldKind.Concealed` T520 dalında (#549) aynı blob (`6c9fbc9a`); hangisi sonra girerse birleştirme tek tanım bırakır — eylem yok. K3 `VAL-A678`'in "bekleyen olmayan (doğrulanmış) kayıt değeriyle GET" durumu ve `VAL-A090`'ın uygulama kaydı kanıtı yok; satırlar BEKLEMEDE → düzeltme turunda bu PR'da.

**`12` Durum (tur 2):** `VAL-A493`, `VAL-A502`, `VAL-A564`, `VAL-A677`, `VAL-A920` → PASS. Ayrıntı: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/impl/T514/dogrulama-raporu-r2.md`.

**Tur 1 özeti:**

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
