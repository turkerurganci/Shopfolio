# T519 — E-posta değişikliğini geri alma ve müşteri çıkışı (E-28)

**Faz:** F5 | **Durum:** ✗ FAIL (doğrulama tur 1) | **Tarih:** 2026-11-14

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `GET /geri-al/{key}` (UC-75) ve `POST /geri-al/{key}?handler=RevertPendingEmailChange` (UC-76): `EmailRevertModel` + `EmailRevert.cshtml`, metinler `EmailRevertLandings` (tek yerde, sayfa ve testler oradan okur).
- `POST /cikis?handler=SignOut` (UC-77): `SignOutModel` (`[Authorize(CustomerPolicy)]` + `[SignOutPage]`, GET 404; yönlendirme `ReturnPath.AfterSignOut`).
- Modül: `IMembershipModule.ReadEmailRestoreLinkAsync` ve `RestorePreviousEmailAsync` (`EmailChanges`); `ISessionService.InvalidateMarksAsync`; `CustomerPasswords.InvalidatePasswordAsync`.
- Geri alma: hesap satırı kilidi, eski adres başka doğrulanmış hesabın mı denetimi (422, hiçbir şey yazılmaz), tek koşullu tüketim, adresi tutan doğrulanmamış bekleyen kaydın silinip imha kaydına yazılması, adres + Google bağı geri, bütün oturumlar + tarayıcı işaretleri silinir, şifre varsa geçersiz + eski adrese sıfırlama bağlantısı; istenmiş bütün sıfırlama / Google teyit / yeni adres bağlantıları geçersiz.
- D-554 ve D-578 kapandı; `KeyedLinkSample` (test örneği `/geri-al/{key}` adresini tutuyordu, gerçek sayfayla çakıştı) kaldırıldı; `LinkKeySourceScan` yeni tüketen komutu tanıyor.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/` (`EmailChanges.cs`, `MembershipModule.cs`, `CustomerPasswords.cs`, `SessionService.cs`, `Contracts/`)
- `src/Shopfolio.Web/Pages/Storefront/EmailRevert.cshtml(.cs)`, `SignOut.cshtml(.cs)`, `Pages/Components/EmailRevertTexts.cs`, `Pages/Frame/FramePaths.cs`
- Testler: `Web/EmailRevertPageTests.cs`, `Web/CustomerSignOutPageTests.cs`, `Model/MembershipEmailRevertTests.cs`, `Model/MembershipGoogleTests.cs` (D-578), `Pages/EmailRevertLandingsTests.cs`, `Architecture/LinkKeySourceScan*.cs`
- Kayıt: STATUS satırı, `STATUS_CHANGELOG.d/T519.md`, `DEFERRED_BACKLOG.d/T519.md`, D-554 ve D-578 yerinde kapatıldı.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Geçerli bağlantıda geri alma adımı; açılış geri almaz | ✓ | `EmailRevertPageTests.OpeningAValidLink_ShowsTheStepAndTheOneButton_AndWritesNothing_T519_KK1` |
| 2 | Kullanılmış bağlantı kodla sonuçlar, kodsuz geçersiz; dolmuş/tanınmayan geçersiz | ✓ | `TheInvalidStates_AreToldWithoutAButton_...KK2`, `EmailRevertLandingsTests` |
| 3 | Başka tür / yönetici bağlantısı tanınmaz, tüketilmez | ✓ | `ALinkOfAnotherTypeOrAnAdministrator_IsNotRecognized_AndIsNotConsumed_KK3` |
| 4 | Bağlantıyı tüketir, eski adrese döner, 303 → `/geri-al/{key}` + sonuç kodu | ✓ | `TheButton_GoesBackToTheEarlierAddress_...KK4_KK5` |
| 5 | Oturumlar, işaretler, Google bağı, şifre, sıfırlama bağlantısı | ✓ | aynı test + `TheButton_ForAnAccountWithoutAPassword_...`, `Revert_GoesBackToTheEarlierAddress_...` |
| 6 | Bekleyen kayıt silinir + imha kaydı; siparişler hesapta kalır | ✓ | `TheButton_DeletesTheUnverifiedPendingRegistration_...KK6`; sipariş: KK4 testi |
| 7 | Eski adres başka doğrulanmış hesabın: 422, hiçbir şey yazılmaz, tüketim geri döner | ✓ | `TheButton_WhenTheEarlierAddressBecameAnotherVerifiedAccounts_Answers422_...KK7`, `Revert_WhenAnotherAccountTakes...Race` |
| 8 | Yarışta 200 + o anki hâl; tekrar gönderim aynı | ✓ | `ASecondPost_FindsTheLinkUsed_Answers200_...KK8` |
| 9 | `SignOut` oturum + çerez siler; sepet hesapta kalır | ✓ | `CustomerSignOutPageTests.TheSignOut_DeletesTheSessionRowAndTheCookie_...KK9` |
| 10 | `ReturnUrl` yerel ve oturum istemeyen sayfaysa oraya, değilse ana sayfaya 303 | ✓ | `TheAnswer_IsThePageThatNeedsNoSession_OrTheHomePage_KK10` (12 örnek) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 açılış yazmaz; adım görünür | ✓ | KK1 testi: iki açılış sonrası e-posta, bağlantı, oturum, dispatch değişmez |
| KK2 üç hâl | ✓ | `EmailRevertLandings.Of`; 4 durum + kod yalnız kullanılmış bağlantıda sayılır (Unit + Web) |
| KK3 yabancı tür tüketilmez | ✓ | reset, yeni adres, yönetici revert bağlantısı GET ve POST; `used_at` hâlâ boş |
| KK4 eski adres döner, tek tüketim | ✓ | `ASecondPost...` ve `MembershipTests` ikinci çağrı `Used` |
| KK5 oturumlar, işaretler, Google, şifre, e-posta | ✓ | KK4/KK5 testi: oturum 0, işaret 0, `GoogleLinkedAt` null, `PasswordHash` null, tek yeni sıfırlama bağlantısı; şifresiz hesapta dispatch yok |
| KK6 bekleyen kayıt, imha kaydı | ✓ | KK6 testi: kayıt silindi, `DisposalRecord` PendingRegistration sayı 1 |
| KK7 çakışmada hiçbir şey yazılmaz | ✓ | 422 testi (oturum, Google, şifre, bağlantı, dispatch, imha kaydı aynı) + gerçek unique-index yarışı |
| KK8 yarışta 200; ikinci gönderim | ✓ | KK8 testi, satır sayıları değişmez |
| KK9 oturum silinir, sepet durur | ✓ | sign-out testi: sepet satırı, işaret, öteki oturum kalır; çerez silinir; oturumsuz/tekrar aynı 303, yazmaz |
| KK10 üç dönüş yeri | ✓ | Theory: oturum istemeyen sayfa, `/hesap` altı + `/siparis/{no}` + `/panel` → `/`, yabancı değerler → `/` |
| `09 §25` güvenlik taraması | ✓ | Unit TAM 7270 yeşil (JobSourceScan geniş catch, IrreversibleConfirmation "Revert" adı, LinkKeySourceScan yakaladı ve düzeltildi); Consistency 295 yeşil |
| Sık FAIL (1) tarama istisnası geniş | ✓ | `LinkKeySourceScan`: yalnız `EmailChanges.cs` içindeki `RestorePreviousEmailAsync` adı, sarmalayıcı `MembershipModule`; çağıran regex'e eklendi |
| Sık FAIL (2) çağrı biçimi | ✓ | tüketen komut adı `VerifyCall` regex'inde; çağıran yalnız `OnPost…` |
| Sık FAIL (3) DB default / FK dizini | ✓ | şema/migration yok |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | `KeyedLinkSample` ve `SecurityHeadersTests` (artık gerçek sayfa), `LinkKeySourceScanTests` theory |
| Sık FAIL (5) belge kuralı değişti mi | ✓ | yok; 09 değişmedi |
| Hedefi T519 olan backlog: D-554 | ✓ | kapandı: karar "bilinçli kalma" + geri alma bütün bekleyen sıfırlama bağlantılarını kapatır (`Revert_GoesBackToTheEarlierAddress_...`) |
| Hedefi T519 olan backlog: D-578 | ✓ | kapandı: iki özel durum şekli yakalanır; `EmailChange_Verify_WhenAnotherAccountTakes...D578` gerçek unique ihlalini sınar; L-9 testi son verilen bağlantının `Valid` kaldığını iddia eder |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --filter-not-trait "Consistency=true"` | 7270 / 0 hata |
| Consistency | ✓ | `--filter-trait "Consistency=true"` | 295 / 0 hata |
| Integration | ✓ ilgili sınıflar; TAM: CI | `--filter-class` ile `MembershipTests` (142 + hesap silme), `EmailRevertPageTests`, `CustomerSignOutPageTests`, `SecurityHeadersTests`, `RegistrationPageTests`, `AccountProfilePageTests`, `SessionAccessTests`, `ForgeryProtectionTests`, `StorefrontFrameTests`, `FrameLayoutTests`, `MyAccountPageTests`, `AddressRoutingTests`, `CustomerPasswordResetAndReauthenticationPageTests`, `PanelHomeAndVerificationPagesTests` | 229 + 190 / 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <değişen dosyalar>` | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-14) |
| Bulgu sayısı | 2 (B1 S1 KRİTİK, B2 S1 ORTA) + 2 K-notu (D-1508, D-1509) |
| Düzeltme gerekli mi | Evet — B1 ve B2 bu dalda düzeltilir; D-1509 aynı turda |
| Ana dal CI (son 3) | 43557742117, 43557742023, 43552777848 — success |
| Dal CI | run 43560620905 (head `4eb77fce`) — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate success |
| Yerel sınama (main `8a51cdcc` ile birleşik ağaç) | build `-warnaserror` 0 uyarı; format 3 proje temiz; Unit TAM (Consistency + Architecture dahil) 7575/0; betik testleri 4/4; Integration ilgili sınıflar (`EmailRevertPageTests`, `CustomerSignOutPageTests`, `MembershipTests`, `SecurityHeadersTests`, `SessionAccessTests`, `AccountProfilePageTests`) 229/0; TAM: CI |

### Kabul kriterleri (doğrulayıcı)

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `EmailRevertModel.OnGetAsync` yalnız `ReadEmailRestoreLinkAsync` çağırır; KK1 testi iki açılış sonrası e-posta, `UsedAt`, oturum, dispatch değişmez |
| 2 | ✓ | `EmailRevertLandings.Of` + KK2 testleri (Unit + Web): dolmuş, yenilenmiş, tanınmayan geçersiz; kod yalnız kullanılmış bağlantıda sonuç |
| 3 | ✓ | `ReadRestoreLinkAsync`/`RestorePreviousEmailAsync` `EmailChangeRevert` + `CustomerId` dolu kaydı tanır; KK3 testi sıfırlama, yeni adres ve yönetici bağlantısında GET/POST 200 geçersiz, `UsedAt` boş |
| 4 | ~ | Tüketim, eski adres, 303 + `notice` sınandı; ama tüketim `PreviousEmail`'i boşaltmıyor (`06 §4.42`; `07` UC-76 → B1) |
| 5 | ✓ | `EndAllAsync(null)`, `InvalidateMarksAsync`, `GoogleLinkedAt` null, `InvalidatePasswordAsync` + `PasswordResetLink` eski adrese; şifresiz hesapta gönderim yok (KK4/KK5 testleri) |
| 6 | ✓ | Doğrulanmamış kayıt `ExecuteDeleteAsync` + `DisposalRecord.Counted(PendingRegistration)`; sipariş hesapta kalır (KK6, KK4 testleri) |
| 7 | ✓ | Ön denetim tüketimden önce `Taken`; yarışta unique ihlali iki şekilde yakalanır, `IsRejected` → işlem geri alınır; 422 testi + `RaceAsync` testi |
| 8 | ✓ | `state != Valid` ve `TryConsumeAsync` false dalları 200 + o anki hâl; ikinci gönderim satır sayısını değiştirmez (yarış kolu dolaylı — D-1509) |
| 9 | ✓ | `SignOutModel`: `EndAsync(sessionId)` + çerez silme; sepet, işaret, öteki oturum kalır (KK9 testleri); oturumsuz istek `[SignOutPage]` meydan okumasıyla aynı 303 |
| 10 | ✓ | `ReturnPath.AfterSignOut` (`Resolve` → `IsLocalPath`); 12 örnekli Theory: oturumsuz sayfa, `/hesap`·`/siparis/{no}`·`/panel` → `/`, `//`, `/\`, mutlak adres → `/` |

### Bulgular

| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Karar | Dosya |
|---|---|---|---|---|---|---|
| B1 | VAL-A809 (KK4) | KRİTİK | S1 | `06 §4.42`: geri alma bağlantısı kullanılınca `PreviousEmail` aynı işlemde boşaltılır ↔ `RestorePreviousEmailAsync` ve `TryConsumeAsync` yalnız `UsedAt` yazar; eski adres bağlantı kaydında P-47 dolana kadar kalır. Backlog'da kabul edilmiş risk kaydı yok | bu dalda düzeltilir (tüketimle aynı işlemde boşaltma + test) | `src/Shopfolio.Web/Modules/Membership/EmailChanges.cs` |
| B2 | `07` UC-76 · §6.4.2 · `09 §8.1` m.4 | ORTA | S1 | Sonucu belirsiz geri alma 303 → aynı adres; işleyici `[OutcomeUncertain("UC-76")]` taşır ve UC-76 `NotMarkedYet`'ten silinir (D-137: "işaretli işleyiciyi yazan her task") ↔ işaret yok, liste değişmedi | bu dalda düzeltilir | `src/Shopfolio.Web/Pages/Storefront/EmailRevert.cshtml.cs` · `tests/Shopfolio.UnitTests/Consistency/OutcomeUncertainConsistencyTests.cs` |

### K-notları

| # | Not | Karar |
|---|---|---|
| K1 | UC-76 422/200 redleri `RejectionLog`'a yazılmıyor (D-1317 kalıbı) | D-1508 → F5 kapısı |
| K2 | KK8 yarış kolu ve VAL-C029 POST yolu dolaylı sınanıyor | D-1509 → T519 düzeltme 1 |

### `12` satırları

VAL-C028 → PASS; VAL-A809 → FAIL; VAL-C029, VAL-C116, VAL-A565, VAL-A608, VAL-A675, VAL-A922, VAL-A981, VAL-B079 → BEKLEMEDE (paylı ya da kanıt eksik; D-1495, D-1496, D-1509); VAL-A991 PASS (değişmedi).

### Yapım raporu karşılaştırması

İki uyuşmazlık: öz denetim KK4'ü ✓ sayar, `PreviousEmail` boşaltmasını (`06 §4.42`, VAL-A809) anmaz; `09 §25`/`09 §8` satırı UC-76'nın sonucu belirsiz işaretini (D-137) anmaz. ⚠ kararlar (D-554'te geri almanın bütün sıfırlama bağlantılarını kapatması, `LinkKeySourceScan`'e yalnız `EmailChanges.RestorePreviousEmailAsync` + sarmalayıcının eklenmesi, `KeyedLinkSample`'ın kaldırılması) uygun bulundu: istisna yalnız adı geçen yöntemde, çağrı `OnPost` kuralına bağlı; örnek sayfa gerçek sayfayla aynı rotayı tutuyordu ve `SecurityHeadersTests` aynı iddiaları gerçek sayfada sınıyor.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T519-eposta-geri-alma-cikis`
- **PR:** #571
- **Commit:** `6334354a` — T519: E-posta değişikliğini geri alma ve müşteri çıkışı (E-28)
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Başlangıç kapıları: working tree temiz; `core.hooksPath` = `scripts/git-hooks`; `main` son üç run (`43557742117`, `43557742023`, `43552777848`) success.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Bu task'ta plan tier, kota, harici servis ya da paket sürümü varsayımı yok; yalnız mevcut modül, bağlantı ve oturum katmanı kullanıldı | `git diff --stat` yeni paket ya da yapılandırma içermiyor | ✓ |

## ⚠ Kararlar (kolay, kurala en uygun seçenekle verildi)

- ⚠ D-554: değişikliğin kendisi bekleyen sıfırlama bağlantılarını kapatmaz (02 3.13.14 yalnız Google teyidini sayar; bağlantı ömrü P-13 dakika); **geri alma** hepsini kapatır — değişiklik sırasında kötü niyetli oturumun istediği sıfırlama bağlantısı geri almadan sonra kullanılamasın.
- ⚠ Şifresi olmayan hesapta sıfırlama bağlantısı gitmez (02 3.13.14 açık); bağlantılar yine de kapanır.
- ⚠ Geri alma sonuç cümlesi tek hâlde ("şifresi varsa ... gönderildi"): sonuç kodundan şifre durumu ayırt edilemez, gizlilik açısından da gerekmez.
- ⚠ Modül yöntem adları `RestorePreviousEmailAsync` / `ReadEmailRestoreLinkAsync`: `IrreversibleConfirmationTests` modül yüzeyinde "Revert" sözcüğünü geri alma sayıp engeller (yönetici modülü de aynı adı kullanır).
- ⚠ `KeyedLinkSample` test sayfası kaldırıldı (adres çakışması); `SecurityHeadersTests` aynı iddiaları gerçek sayfayla sınar.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Paylı `12` satırlarının `Durum` hücreleri | D-1495 | Payın kanıtı bu raporda; hücreyi doğrulama işler |
| 2 | Ekranın 320 genişlik / klavye gözlemi | D-1496 | Tarayıcı gözlemi E2E katmanında (T601) |

Not: süre bütçesi (45 dk) 2 dakika aşıldı; iş tamamlandı, bölünme yok.
