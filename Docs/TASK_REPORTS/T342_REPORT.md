# T342 — Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-08

---

## Yapılan İşler

- `Infrastructure/Security/PasswordPolicy.cs` + gömülü `CommonPasswords.txt`: uzunluk tabanlı politika (P-17, metin öğesi sayısıyla), yaygın şifre listesi büyük/küçük harfsiz, karakter türü dayatması yok; liste `FrozenSet` (değişebilir statik koleksiyon bekçisine takılmaz). `PasswordTooWeak` alan kuralı bu sınıfa bağlı.
- `Infrastructure/Security/SessionStore.cs`, `PasswordRegime.cs`, `PasswordResults.cs`: oturum açma (Z-4 kısa, Z-5 "beni hatırla" uzun), tanınan tarayıcı işareti, L-1 sayacı (giriş ve yeniden doğrulama), sıfırlama (Z-2 ömürlü, tek kullanımlık), hesapta şifre değiştirme, yeniden doğrulama değeri; müşteri ve yönetici aynı kodu `IPasswordAccounts` bağdaştırıcısıyla çalıştırır.
- `Modules/Membership`: `ISessionService`/`SessionService`, `CustomerPasswords`, `IMembershipModule` yeni yüzü (`CheckCredentialsAsync`, `RequestPasswordResetAsync`, `ReadPasswordResetLinkAsync`, `ResetPasswordAsync`, `ReauthenticateAsync`, `ChangePasswordAsync`); kayıt aynı politikayı çağırır (D-251).
- `Modules/Administration`: `AdminPasswords`, `IAdministrationModule` aynı rejim (oturum, sıfırlama, yeniden doğrulama, değiştirme); Google, hesap silme ve iki adımlı doğrulama yok.
- `PasswordService`: `Verify`/`VerifyNobody` sanal (sayaçlı dekoratörle sınanır, D-364).
- Kaynak taramaları: yeni `SessionWriteSourceScan` (oturum satırına tek yazma yolu, D-133); `LinkKeySourceScan` sıfırlama tüketimi; `ThirdPartyContentScan` (D-114) ve `CookieSourceScan` (kural 5) daraltmaları.
- Bu işçinin turu: derleme ve test koşuları; `PasswordPolicy` listesini `FrozenSet` yaptı (bekçi `CacheSourceScan` bulgusu); listedeki 9 karakter ve altı altı girdi çıkarıldı (üstbilgi "yalnız P-17 ve üstü" der); `ThirdPartyContentScanTests` örnek verisindeki `) {` kalıbı yanlış test gövdesi sayıldığı için düzeltildi; `PasswordPolicyTests` yazıldı; backlog, durum, rapor ve hafıza güncellendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/{SessionStore,PasswordRegime,PasswordResults,PasswordPolicy,PasswordService,LinkRecords,CommonPasswords.txt}`, `Modules/Membership/**`, `Modules/Administration/{AdminPasswords,AdministrationModule}.cs`, `Program.cs`.
- Testler: `tests/Shopfolio.IntegrationTests/Model/{MembershipSessionTests,MembershipTests}.cs`, `tests/Shopfolio.UnitTests/Security/PasswordPolicyTests.cs`, `tests/Shopfolio.UnitTests/Architecture/{SessionWriteSourceScan,LinkKeySourceScan,ThirdPartyContentScan,CookieSourceScan}(+Tests)`, `ViolationSamples/Sessions/SessionMisuse.cs`.
- Belgeler: `DEFERRED_BACKLOG.md`, `IMPLEMENTATION_STATUS.md` (+ `STATUS_CHANGELOG.md`), `MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sıfırlama bağlantısı Z-2 ömürlü, tek kullanımlık; kullanılmış/dolmuş geçersiz | ✓ | `TheReset_TakesTheLinkOnce_EndsEverySession_…_KK1_KK2`, `TheReset_RefusesAWeakNewPassword…AndAnExpiredOrAlienLinkIsInvalid`, `AskingAgain_MakesTheEarlierResetLinkInvalid` |
| 2 | Sıfırlamada bütün oturumlar kapanır; değiştirmede öteki kapanır, değiştiren kalır; işaretler düşer | ✓ | `TheReset_…EndsTheOtherMarks…`, `TheChange_AsksForTheReauthenticationValue_…_KK2_KK5` |
| 3 | Sıfırlama ekranı nötr, kayıt ekranı dürüst | ✓ | `TheResetRequest_IsNeutral_ForAKnownAndAnUnknownAddress_…_KK3`; kayıt: `ErrorCode.EmailAlreadyRegistered` (`MembershipModule.RegisterAsync`) |
| 4 | "Beni hatırla" Z-5, işaretsiz Z-4 | ✓ | `SignIn_OpensTheLongSessionWithRememberMe_AndTheShortOneWithout_KK4` |
| 5 | Şifre değiştirme/e-posta değiştirme/hesap silme yeniden doğrulama ister; başka işlem istemez | ✓ (şifre) / ~ | şifre: `TheChange_AsksForTheReauthenticationValue_…`, `TheReauthenticationValue_IsForOneOperationAndOneOwnSession`; ad değişimi istemez: T341 `TheName_ChangesWithoutANewVerification_…`; e-posta değişimi ve hesap silme tüketimi kendi task'ında (T517 k2) → D-429 |
| 6 | Yeniden doğrulamada yanlış şifre L-1'e sayılır | ✓ | `AWrongPasswordAtTheReauthentication_CountsToL1_AndTheSixthIsBlocked_KK6` |
| 7 | Uzunluk tabanlı politika, tür dayatması yok, yaygın şifre yerel listeyle reddedilir | ✓ | `PasswordPolicyTests` (20), `Register_RefusesAShortAndACommonPassword_AndDoesNotDictateCharacterTypes_…_KK7` |
| 8 | Yöneticiye aynı değerler; Google, hesap silme, iki adımlı doğrulama yok | ✓ | `TheAdministrator_GetsTheSameSessionLifetimes_PolicyAndLimit_AndNoAccountDeletion_KK8`, `TheAdministratorsReset_IsTheSameOneUseLink_…_KK8` |
| 9 | Giriş, sıfırlama, kayıt ve doğrulama bağlantısı isteği limitli; eşikte geçici engel, kilit yok, mesaj nötr | ✓ | `SignIn_IsBlockedAfterFiveFailures_…_KK9`, `TheResetRequest_OverL2_IsRefusedWithoutLockingTheAccount_AndSaysNoNumber_KK9`, `Register_PastTheLimitL3_…` (T341), `AskingForANewLink_IsLimitedByL9_…` (T341) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: sıfırlama bağlantısı ikinci kullanımda reddedilir | ✓ | `TheReset_TakesTheLinkOnce_…_KK1_KK2` |
| KK2: sıfırlamada tüm, değiştirmede öteki oturumlar kapanır; işaretler düşer | ✓ | `TheReset_…`, `TheChange_…_KK2_KK5` (`InvalidateMarksAsync`, `PasswordRegime`) |
| KK3: iki ekranın mesajı ayrı | ✓ | `TheResetRequest_IsNeutral_…_KK3`; kayıt `EmailAlreadyRegistered` |
| KK4: Z-5 / Z-4 | ✓ | `SignIn_OpensTheLongSessionWithRememberMe_…_KK4` |
| KK5: şifre değişimi yeniden doğrulama ister, ad değişimi istemez | ✓ | `TheChange_…_KK2_KK5`; `TheName_ChangesWithoutANewVerification_…` (T341) |
| KK6: yeniden doğrulamada yanlış şifre L-1'e | ✓ | `AWrongPasswordAtTheReauthentication_CountsToL1_…_KK6` |
| KK7: P-17 altı ve yaygın şifre reddi, karakter dayatması yok | ✓ | `PasswordPolicyTests`, `Register_RefusesAShortAndACommonPassword_…_KK7` |
| KK8: yönetici girişinde aynı süre ve politika | ✓ | `TheAdministrator_GetsTheSameSessionLifetimes_…_KK8` |
| KK9: eşikte engel, kilit yok, kalan sayı söylenmez | ✓ | `SignIn_IsBlockedAfterFiveFailures_…_KK9`, `TheResetRequest_OverL2_…_KK9` |
| `09 §25` — aynı işi yapan tek işlev | ✓ | politika tek evde (`PasswordPolicy`), kayıt/sıfırlama/değiştirme/yönetici onu çağırır; alan kuralı `PasswordTooWeakAttribute` ince sarmalayıcı; e-posta biçimi `InvalidEmailAttribute` (T341) |
| `09 §25` — kaynak taraması ve ihlal örneği | ✓ | `SessionWriteSourceScanTests` (+ `ViolationSamples/Sessions/SessionMisuse.cs`), `LinkKeySourceScanTests`, `ThirdPartyContentScanTests` (D-114), `CookieSourceScanTests` |
| `09 §25` — güvenlik taraması (mini) | ✓ | şifre yalnız `PasswordService`'te özetlenir, kayıt/günlük/mesaja yazılmaz; sıfırlama nötr; eşikte kilit yok, kalan sayı yok; `CredentialSourceScanTests` yeşil; sır yok |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | `SessionWriteSourceScan.Allowed` yalnız `SessionStore`, ilgili değer dosyası ve kimlik doğrulama işleyicisi (dosya bazlı, kural bazlı); `LinkKeySourceScan.WrapperMethods` iki dosya × tek metot (`ResetPasswordAsync`), çağıranlar `OnPost` kuralına bağlı |
| Sık FAIL (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ | `ConsumingMethods` + regex `ResetPasswordAsync`; D-114 `createElement` biçimi hâlâ bulgu (`Scan_OnACodeOrScriptFile_StillFindsAnElementMadeWithCreateElement_D114`); `Headers.Remove(HeaderNames.SetCookie)` dışı başlık erişimi bulgu |
| Sık FAIL (3) DB varsayılanı / FK dizini | ✓ | şema değişmedi (yeni tablo/sütun/göç yok) |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | P-17 yalnız `ProductConstants.PasswordMinLength` (test sabitten türer); `FieldRuleTests` 13→14; liste üstbilgisi ile içeriği uyumlu (`ShippedList_IsNotEmpty_AndHoldsNoEntryBelowTheLimit`); backlog aktif sayısı 159 (5 çözüldü, 5 yeni, D-127 açık) |
| Sık FAIL (5) tek kural evi varken kendi denetimini yazmak | ✓ | yönetici ve müşteri `PasswordRegime`'i paylaşır; politika ve L-1 sayacı (`AttemptCounter`) yeniden yazılmadı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --no-build` | 4276 / 0 başarısız (Architecture 969, `PasswordPolicyTests` 20 dahil) |
| Integration (Web) | ✓ | `--filter-namespace "Shopfolio.IntegrationTests.Web"` | 594 / 0 başarısız |
| Integration (Membership) | ✓ | `--filter-class "Shopfolio.IntegrationTests.Model.MembershipTests"` | 50 / 0 başarısız |
| Integration (komşu) | ✓ | `SessionLinkAttemptLoginTablesTests`, `SessionAccessTests`, `OneTimeValueTests` | 114 / 0 başarısız |
| Derleme / biçim | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; biçim temiz |
| Contract | — | — | bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor (yönetici başlatır) |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Gömülü kaynak `CommonPasswords.txt` (derlemeye girer, dış servis çağrılmaz)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T342-giris-oturum`
- **Commit:** son head PR açıklamasında
- **PR:** PR açıklamasında
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Yok — dış varsayım yok; ana dal CI durumunu yürütme ajanı ayrıca izler | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Giriş yolunda `AccountLocks`/`LockKind.Account` ve `FOR UPDATE` | D-428 | hesap silen/e-posta değiştiren task'ın işi |
| 2 | E-posta değişimi ve hesap silmede yeniden doğrulama tüketimi | D-429 | T517 k2 |
| 3 | Tek `__Host-known-device` çerezi müşteri ve yöneticide ortak | D-430 | sayfa task'ları |
| 4 | Giriş çerezi yazımı, sepet birleşmesi | D-431 | giriş sayfası task'ı |
| 5 | ⚠ Engelli giriş denemesi giriş kaydına `Failed` yazılır (`LoginOutcome` iki değerli) | D-432 | karar; ayrı değer gerekirse enum genişler |
| 6 | D-127 T342 payı (PasswordTooWeak) çözüldü; öteki kodlar T344, T339, T330'da | D-127 | açık kalır |

Çözülenler: D-114, D-133, D-251, D-255, D-364.

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi `origin/task/T342-giris-oturum` ucundan başladı)
- **Ana dal CI startup (Adım 0):** yürütme ajanı yeniden bakar; önceki işçi: 42426248059/42426248097 in_progress, 42423723835 success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** `PasswordService` testte dekoratör için sanal; PasswordRegime bağdaştırıcıları `AdminPasswords`/`CustomerPasswords`; çerez yazımı ve sayfalar sayfa task'larında.
