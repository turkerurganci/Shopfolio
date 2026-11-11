# T520 — Panel girişi, şifre sıfırlama, davet kabulü ve yönetici çıkışı (E-29, E-30)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-11

---

## Yapılan İşler

- `Pages/Panel/SignIn`, `Reset`, `Invite`, `SignOut` sayfaları ve `PanelAccessTexts`; adres sabitleri `PanelFramePaths`'te. Üçü çerçevesiz (`_PanelBareLayout`), `[NoStore]`.
- Şifre alanı T514 ile aynı `FieldKind.Concealed` / `Fields.Concealed`; sayfa şifreyi geri yazmaz.
- Test ornek sayfaları (SignInSample, SignOutSample, Reset, PanelBareSample) kaldırıldı; `SessionAccessTests` ve `FrameLayoutTests` gerçek sayfalara taşındı.
- Girdi modeli alan adları 07 §5.1.2 tablosuna uyduruldu: `IsLongLived` ("beni hatırla"), `Password`, `NewPassword`, `ReturnUrl` için `SpecAssignedName`.
- Yeni testler: `PanelAccessPagesTests` (Integration, 22 test), `PanelAccessTextsTests` (Unit, 3 test).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/*`, `Pages/Frame/PanelFrame.cs`, `Pages/Components/FieldModels.cs`
- `tests/Shopfolio.IntegrationTests/Web/PanelAccessPagesTests.cs`, `SessionAccessTests.cs`, `FrameLayoutTests.cs`; `tests/Shopfolio.UnitTests/Pages/PanelAccessTextsTests.cs`
- Kayıt: `Docs/STATUS_CHANGELOG.d/T520.md`, `Docs/DEFERRED_BACKLOG.d/T520.md`, `IMPLEMENTATION_STATUS.md` tablo satırı, `DEFERRED_BACKLOG.md` (D-595 kapandı, D-464 ve D-806 payları)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Giriş sayfası: işaretsiz "beni hatırla", kodla mesaj, korumalı değerle e-posta ön doldurma | ✓ | `TheSignInPage_HasTheRememberMeBoxUnticked_…_KK1`, `TheAcceptedInvitationValue_FillsTheEmail_…_KK1` |
| 2 | Dört geçersiz sıfırlama bağlantısı isteme adımına götürür | ✓ | `TheResetAddress_OpensTheSetting_OnlyForAValidLink_…_KK4` |
| 3 | `SignIn` yeni anahtar, 303 yerel `returnUrl` / `/panel`, vitrin oturumu etkilenmez | ✓ | `ASignIn_OpensANewPanelSession_AtEveryTime_…_KK2`, `TheSignInGoesToTheLocalReturnAddress_…_KK2`, `TheSignInGetCarriesALocalReturnAddressInTheForm_…_KK2` |
| 4 | 422 boş/biçim; tek mesaj; sahte özet | ✓ | `AnEmptyOrMalformedForm_Is422_…_KK3`, `AWrongPassword_AWrongEmail_AndAnAbsentAccount_GetTheOneSentence_…_KK3`; metin: `PanelAccessTextsTests` |
| 5 | 429 (L-1), hesap kilitlenmez, giriş kaydı | ✓ | `AfterTheLimit_TheSignInIs429_…_KK3` |
| 6 | `RequestPasswordReset` 303 nötr hâl, yalnız yöneticiye, L-2, 422/429 | ✓ | `TheRequest_AnswersTheSame_ForAnAdministratorAndForAnyOtherAddress_…_KK5`, `TheRequest_RefusesAnEmptyOrMalformedAddress_…_KK5` |
| 7 | `ResetPassword` tek tüketim, oturumlar silinir, oturum açmaz, kurulduğu hâl; işaret yazımı | ✓ | `TheSetting_ConsumesTheLinkOnce_ClosesEverySession_OpensNone_…_KK6`; `Reset.cshtml.cs` `WriteKnownDevice` |
| 8 | P-17 altı 422, bağlantı tüketilmez; arada geçersizleşmişse 200 | ✓ | `AWeakPassword_Is422_AndTheLinkIsNotConsumed_…_KK6`; ikinci POST (bağlantı harcanmış) 200 `again` assert'i aynı testte |
| 9 | Davet GET: salt okunur adres, geçersizde form yok; kabul: hesap, tüketim, iz, oturumsuz, girişe dönüş | ✓ | `AValidInvitation_ShowsTheAddressReadOnly_…_KK7`, `TheAcceptance_OpensTheAccountWithTheInvitationsAddress_…_KK8` |
| 10 | 422 ad boş / zayıf şifre; arada geçersizleşmişse 200; `SignOut` 303 `/panel/giris` | ✓ | `TheAcceptance_Is422_…_KK8`, `…KK8` ikinci POST 200; `SessionAccessTests` çıkış testi (handler=SignOut) |

### Öz denetim (bitiş kapısı 2)

Test adlarındaki KK ekleri yapım sırasındaki numaradır; plandaki KK1…KK10 eşlemesi aşağıdadır.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 kod, korumalı değer | ✓ | `…KK1` iki test |
| KK2 dört geçersiz bağlantı | ✓ | `…ResetAddress_OpensTheSetting_OnlyForAValidLink` (kullanılmış, dolmuş, yenilenmiş, tanınmayan) |
| KK3 iki oturum ayrı; dönüş yeri | ✓ | `ASignIn_OpensANewPanelSession_AtEveryTime_AndTouchesNoStorefrontSession`, dönüş yeri testleri |
| KK4 tek mesaj; sahte özet | ✓ | `AWrongPassword_AWrongEmail_AndAnAbsentAccount_…` |
| KK5 429, giriş kaydı | ✓ | `AfterTheLimit_TheSignInIs429_…` |
| KK6 iki adres; L-2 | ✓ | `TheRequest_AnswersTheSame_…` (yönetici / yönetici olmayan adres, gönderim kaydı) |
| KK7 tek tüketim; oturumlar, işaretler | ✓ | `TheSetting_ConsumesTheLinkOnce_…`; işaret `Reset.cshtml.cs` |
| KK8 ret tüketmez; yarışta 200 | ✓ | `AWeakPassword_Is422_AndTheLinkIsNotConsumed_…`; harcanmış bağlantıya ikinci POST 200 |
| KK9 geçersiz davet; hesap, iz, dönüş | ✓ | `AValidInvitation_…`, `TheAcceptance_OpensTheAccount…` (iz: `CreateAdminAccount`) |
| KK10 iki 422; yarışta 200; çıkış | ✓ | `TheAcceptance_Is422_…`; davet ikinci POST 200; `SessionAccessTests` çıkış |
| 09 §25 ilgili satırlar | ✓ | `grep T520 Docs/09*` boş: T520'ye bağlı satır yok |
| Güvenlik: `Html.Raw` yok, `[NoStore]` dört sayfada, şifre geri yazılmaz, panel kapalı ekran (`_PanelBareLayout` robots) | ✓ | `grep Html.Raw Pages/Panel` boş; `AnEmptyOrMalformedForm_Is422_…` |
| Sık FAIL 1 (geniş istisna) | ✓ | Yeni bekçi/tarama istisnası eklenmedi |
| Sık FAIL 2 (tarama çağrı biçimi) | ✓ | Yeni tarama kuralı yok; mevcutlar yeni sayfalar üstünde yeşil (Unit tam koşu) |
| Sık FAIL 3 (06 varsayılan / FK) | ✓ | Şema değişikliği yok |
| Sık FAIL 4 (aynı sayıyı anan yerler) | ✓ | Alan adı değişimi (`RememberMe` → `IsLongLived`) sayfa, görünüm ve testlerde birlikte; `ModelBindingRuleTests` yeşil |
| Sık FAIL 5 (09 metni karar kaydısız) | ✓ | 09 değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 7158 testten 2 kırmızı (alan adı kuralı, düzeltildi; regex zaman aşımı yük kararsızlığı, tek başına yeşil 15/15); sonra `ModelBindingRuleTests` 27/27, `PanelAccessTextsTests` 3/3 |
| Integration | ✓ | `Shopfolio.IntegrationTests.exe -maxthreads 3 -class …` | PanelAccessPagesTests, SessionAccessTests, StorefrontFrameTests 94/94; öteki Web sınıfları (form, forgery, güvenlik başlıkları, çerez, hata katmanı, vitrin, sepet, yasal metin) 221/221 |
| Format | ✓ | `dotnet format --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T520-panel-giris-sifre-davet`
- **PR:** #549

## ⚠ Maddeler

- Dönüş adresi her yerel yolu kabul eder (D-1260).
- Müşteri çerezli panel POST'u `ForgeryTokenHandler`'da 400 verir (jeton kullanıcıya bağlı); "vitrin oturumu dokunulmaz" testi çerezsiz yapıldı.

## Dış Varsayımlar

Dış varsayım: yok.
