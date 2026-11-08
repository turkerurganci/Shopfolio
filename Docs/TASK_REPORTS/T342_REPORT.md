# T342 — Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri

**Faz:** F3 | **Durum:** ✗ doğrulama tur 2 FAIL (tur 1 Bulgu 1 kapandı; tur 2 Bulgu 1–2 açık) | **Tarih:** 2026-11-08

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
| 2 | Sıfırlamada bütün oturumlar kapanır; değiştirmede öteki kapanır, değiştiren kalır; işaretler düşer | ✓ | tur 2: değiştirme ↔ sıfırlama ve değiştirme ↔ giriş eşzamanlı, kilitlenmesiz (`MembershipPasswordLockOrderTests`); `TheReset_…EndsTheOtherMarks…`, `TheChange_AsksForTheReauthenticationValue_…_KK2_KK5`; eşzamanlı giriş (tur 1 Bulgu 1): `AccountLocks` + `ASignInCheckedBeforeTheReset_…_KK2`, `ASignInThatWaitsForTheReset_…_KK2` (müşteri ve yönetici) |
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
| KK2: sıfırlamada tüm, değiştirmede öteki oturumlar kapanır; işaretler düşer | ✓ | `TheReset_…`, `TheChange_…_KK2_KK5` (`InvalidateMarksAsync`, `PasswordRegime`); eşzamanlı giriş: `AccountLocks.LockByEmailAsync` (`CheckPasswordAsync`) + `SessionStore.OpenAsync` aynı işlemde kilit altında; `ASignIn…_KK2` ×4 |
| `09 §25` — eşzamanlılık: kilit yerinde mi | ✓ | tek kilit evi `AccountLocks` (`LockKind.Account`, `FOR UPDATE`, `LockSequence` bildirimi, işlem başına bir kez); tur 2 Bulgu 1: kilit sırası tek — hesap satırı EN BAŞTA: `PasswordRegime.ResetPasswordAsync`/`ChangePasswordAsync`/`ReauthenticateAsync` `accounts.LockAsync` ile (`IPasswordAccounts.LockAsync`, müşteri ve yönetici), `SessionStore.EndAllAsync`/`GrantReauthenticationAsync`/`TryRedeemReauthenticationAsync` oturum satırından önce `AccountLocks.LockAsync`; `MembershipPasswordLockOrderTests` 4 test: `TheChange_TakesTheAccountLockBeforeTheSessionRow_…_KK2` (hesap satırı dışarıdan tutulurken oturum satırı `NOWAIT` ile alınabilir), `TheChangeAndTheReset_…_WithNoDeadlock_KK2`, `ASignInThatWaitsForTheChange_…_KK2`, `AChangeThatWaitsForASignIn_…_KK2`; kaynak taraması `ConcurrencySourceScan.AccountLocksFile` |
| `09 §25` — kod hiçbir doküman cümlesiyle çelişmiyor | ✓ | tur 2 Bulgu 2: giriş `FOR UPDATE` kalır (özet yenilemesi paylaşımlı kilidi yükseltir, iki eşzamanlı giriş kilitlenir); `09 §15.1` m.3 ve `05 §6.1.2` koda uydu (`grep -n "FOR UPDATE" Docs/05_TECHNICAL_ARCHITECTURE.md`), karar D-428'de |
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
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --no-build` | 4291 / 0 başarısız (Architecture 969, `PasswordPolicyTests` 20 dahil) |
| Integration (Web) | ✓ | `--filter-namespace "Shopfolio.IntegrationTests.Web"` | 594 / 0 başarısız |
| Integration (Membership) | ✓ | `--filter-class "Shopfolio.IntegrationTests.Model.MembershipTests"` | 58 / 0 başarısız (tur 1: +4 eşzamanlılık; tur 2 düzeltmesi: +4 kilit sırası; tur 2 düzeltmesinde `Unit` 4349, `Concurrency` 26 yeşil) |
| Integration (komşu) | ✓ | `SessionLinkAttemptLoginTablesTests`, `SessionAccessTests`, `OneTimeValueTests` | 114 / 0 başarısız |
| Derleme / biçim | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; biçim temiz |
| Contract | — | — | bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | **tur 2: ✗ FAIL** (2026-11-08; head `1fa9150` + `origin/main` `7ddf334` birleştirmesi, temiz) · tur 1: ✗ FAIL (head `0fadc90`) |
| Bulgu sayısı | tur 2: 2 (ORTA S1, DÜŞÜK S1) · tur 1: 1 (KRİTİK S1, kapandı) |
| Düzeltme gerekli mi | Evet — tur 2 iterasyon sınırındadır (`12 §8.5`); yol yardımcının/proje sahibinin kararı |
| Bloke-etmeyen not (K) sayısı | tur 2: 1 (D-431'e forward) · tur 1: 2 (kapatıldı) |

### Doğrulama tur 2

**Başlangıç kapıları:** working tree temiz, `core.hooksPath` ana checkout'un `scripts/git-hooks`'u; ana dal CI 42436765868, 42436765863, 42434282714 success; MEMORY satır 22. Dal CI 42442007381 (head `1fa9150`) success.

**Kabul kriterleri (validator kanıtı):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `RequestResetAsync` (P-13 = 60 dk, `SupersedeEarlierAsync`), `ResetPasswordAsync` (`TryConsumeAsync`); `TheReset_TakesTheLinkOnce_…_KK1_KK2`, `…AnExpiredOrAlienLinkIsInvalid`, `AskingAgain_…` |
| 2 | ✓ (davranış) | `EndAllAsync`, `InvalidateMarksAsync`, sıfırlayan tarayıcıya işaret; tur 1 Bulgu 1 kapandı (`AccountLocks`, `ASignIn…_KK2` ×4 yeşil); eşzamanlı değiştirmenin kilit sırası → Bulgu 1 |
| 3 | ✓ | `TheResetRequest_IsNeutral_…_KK3`; kayıt `EmailAlreadyRegistered` |
| 4 | ✓ | `OpenAsync` (P-16 = 30 gün çerez / tarayıcı oturumu), işleyici P-15 = 2 sa; `SignIn_OpensTheLongSession…_KK4` |
| 5 | ✓ (T342 payı) | `TheChange_…`, `TheReauthenticationValue_IsForOneOperationAndOneOwnSession`; ad değişimi yeniden doğrulamasız; e-posta/silme T517 (D-429) |
| 6 | ✓ | `AWrongPasswordAtTheReauthentication_…_KK6` |
| 7 | ✓ | `PasswordPolicy` (`LengthInTextElements` ≥ 10, `OrdinalIgnoreCase`, gömülü liste); `PasswordPolicyTests`, `Register_…_KK7` |
| 8 | ✓ | `TheAdministrator_…_KK8`, `TheAdministratorsReset_…_KK8`, yönetici eşzamanlılık testleri |
| 9 | ✓ | L-1 5/20, L-2 3/10; `SignIn_IsBlockedAfterFiveFailures_…_KK9`, `TheResetRequest_OverL2_…_KK9`, `ARecognizedBrowser_…`; L-3/L-9 T341 |

KK1–KK9 ✓; `09 §25` eşzamanlılık satırı ✗ (Bulgu 1) ve doküman uyumu satırı ✗ (Bulgu 2); tek işlev, kaynak taraması + ihlal örneği, `SecretKey`, deneme sınırı kapsamı, `LockSequence` bildirimi ✓.

**Task satırları (`12`):** `VAL-A096`, `A097`, `A098`, `A103` (KRİTİK), `D017` (KRİTİK) `BEKLEMEDE` kaldı (kanıt türü uç yanıtı; D-431, D-429). `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

**Testler (yerel):** build `-warnaserror` 0/0 (birleştirme sonrası da); biçim ×3 temiz; Unit 4291/0; betik 4 dosya; Integration Membership 54/0 + komşu 143/0 ve 125/0; kilitli restore çıkış 0. Kilitlenme sondası (commit edilmedi): değiştirmenin deyim sırası ↔ sıfırlama iki bağlantıda → `40P01 deadlock detected`.

**Güvenlik:** sır yok; auth temiz (Bulgu 1 güvenli düşer: işlem geri alınır); girdi doğrulaması temiz; yeni bağımlılık yok.

**Bulgu 1 — ORTA, S1 (KK2; `09 §25` eşzamanlılık; `09 §10.2` m.3/m.5, `05 §7.3.5` sıra ilkesi; `VAL-A098`, `VAL-D017`):** `ChangePasswordAsync` önce yeniden doğrulama değerini tüketir (oturum satırı güncellemesi = satır kilidi), sonra `SetPasswordAsync` içinde hesap satırını kilitler; `ResetPasswordAsync` önce hesabı, sonra `EndAllAsync` ile oturum satırlarını kilitler. Aynı hesapta değiştirme ve sıfırlama aynı anda → karşılıklı bekleme, PostgreSQL `40P01` (yerelde üretildi: değiştirme düştü). İki oturumdan aynı anda iki değiştirmede de aynı kalıp. Sonuç güvenli ama düşen istek beklenmeyen hata alır; kusuru tur 1 düzeltmesi getirdi. Tur 1'in istediği "değiştirme ↔ giriş" eşzamanlı sınaması da yok. **Düzeltme:** `ChangePasswordAsync` hesap kilidini değerin tüketiminden önce, işlemin ilk kilidi olarak alır; iki bağlantılı sınama değiştirme ↔ sıfırlama ve değiştirme ↔ giriş.

**Bulgu 2 — DÜŞÜK, S1 (`09 §25` doküman uyumu; `09 §15.1` m.3, `05 §6.1.2`):** belge girişte paylaşımlı kilit (`LockSharedAsync`) ve ayrı `LockForUpdateAsync` der; kod tek `LockAsync`, girişte de `FOR UPDATE` (D-428 yazar, belge düzeltilmemiş, çelişki bildirilmemiş). Gerekçe makul (paylaşımlı kilit altında özet yenilemesi eşzamanlı iki girişte kilitlenirdi). **Düzeltme:** belge gerekçesiyle `FOR UPDATE`'e çekilir (karar kaydıyla) ya da kod belgeye uyar.

**K-notları:** K1 — `LockKind.Account` ayrı aile: giriş sayfası sepeti girişle aynı işlemde birleştirirse `LockSequence` sepet kilidini reddeder → D-431 metnine forward edildi.

**Yapım raporu karşılaştırması:** 2 uyuşmazlık — öz denetimde `09 §25` eşzamanlılık ✓ (validator ✗, Bulgu 1; yapım yalnız giriş ↔ sıfırlama sırasını sınadı); öz denetim `FOR UPDATE` der, belgenin paylaşımlı kilidiyle çelişkiyi anmaz (Bulgu 2). Öz denetim tablosu var ve kanıtlı.

### Doğrulama tur 1

**Başlangıç kapıları:** working tree temiz, `core.hooksPath` ana checkout'un `scripts/git-hooks`'u; ana dal CI 42436765863, 42434282714, 42434282725 success; MEMORY satır 22.

**Kabul kriterleri (validator kanıtı):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `RequestResetAsync` (P-13, `SupersedeEarlierAsync`), `ResetPasswordAsync` (`TryConsumeAsync`); `TheReset_TakesTheLinkOnce_…_KK1_KK2`, `…AnExpiredOrAlienLinkIsInvalid`, `AskingAgain_…` |
| 2 | ~ | Sıralı akışta ✓ (`EndAllAsync`, `InvalidateMarksAsync`; `TheReset_…`, `TheChange_…_KK2_KK5`); eşzamanlı girişte ✗ — Bulgu 1 |
| 3 | ✓ | `TheResetRequest_IsNeutral_…_KK3`; kayıt `EmailAlreadyRegistered` |
| 4 | ✓ | `SessionStore.OpenAsync`, `SignIn_OpensTheLongSession…_KK4`, işleyici P-15/P-16 (T207) |
| 5 | ✓ (T342 payı) | `TheChange_…`, `TheReauthenticationValue_IsForOneOperationAndOneOwnSession`; `ChangeNameAsync` yeniden doğrulamasız; e-posta/silme T517 k2 (D-429) |
| 6 | ✓ | `AWrongPasswordAtTheReauthentication_…_KK6` |
| 7 | ✓ | `PasswordPolicy`, 313 girdi hepsi ≥ 10; `PasswordPolicyTests`, `Register_…_KK7` |
| 8 | ✓ | `TheAdministrator_…_KK8`, `TheAdministratorsReset_…_KK8` |
| 9 | ✓ | `SignIn_IsBlockedAfterFiveFailures_…_KK9`, `TheResetRequest_OverL2_…_KK9`, `ARecognizedBrowser_…`; L-3/L-9 T341 testleri |

KK2 ve `09 §25` eşzamanlılık satırı ✗; KK1, KK3–KK9, tek işlev, kaynak taraması + ihlal örneği ✓.

**Task satırları (`12`):** KP-27'den T342'ye bağlı `VAL-A096`, `A097`, `A098`, `A103` (KRİTİK), `D017` (KRİTİK) — hepsi `BEKLEMEDE` kaldı (kanıt türü uç yanıtı, sayfalar sonraki task'larda; D-431, D-429). `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

**Testler (yerel):** build `-warnaserror` 0/0; biçim ×3 temiz; Unit 4035 + Consistency 241 (birleştirme sonrası Unit 4043); betik 4 dosya; Integration T342 16 testi + Membership kalan 43 + komşu 103 (`AttemptCounterTests`, `LinkRecordsTests`, `OneTimeValueTests`, `SessionLinkAttemptLoginTablesTests`) yeşil; `--locked-mode` 0. Dal CI 42436720596 success.

**Güvenlik:** sır yok; girdi doğrulaması temiz; yeni bağımlılık yok; auth — Bulgu 1.

**Bulgu 1 — KRİTİK, S1 (kriter 2 / KK2; `09 §15.1` m.3, `05 §6.1.2`; `VAL-D017`, `VAL-A097`, `VAL-A098`):** giriş hesabın satırını paylaşımlı kilitle okumuyor ve özetin doğrulanan özetle hâlâ aynı olduğunu o kilidin altında denetlemiyor (`AccountLocks`/`LockSharedAsync` yok; `FindByEmailAsync` `AsNoTracking`; `SessionStore.OpenAsync` özet denetimsiz). Sıfırlama ya da hesapta değiştirme eski şifreli bir girişle aynı anda koşarsa: giriş eski özeti görür, sıfırlamanın `DELETE sessions`'ı girişin kesinleşmemiş oturumunu görmez, oturum ekleyen FK `FOR KEY SHARE`'i `password_hash` güncellemesinin `FOR NO KEY UPDATE`'iyle çakışmaz → eski şifreyle açılan oturum sıfırlamadan sonra yaşar. D-428 bu kilidi T517'ye ertelemişti; yarış T342'nin kendi işlemleri arasında olduğu için ertelenemez. Düzeltme: `AccountLocks` (müşteri + yönetici satırı, `OrderLocks` kalıbı, `LockSequence` bildirimi), girişte `FOR SHARE`, oturum aynı işlemde kilit altında özet eşitliğiyle açılır; sıfırlama ↔ giriş ve değiştirme ↔ giriş eşzamanlı sınaması.

**K-notları:** K1 — D-251 metni "yönetici komutunda" diyordu, komut T303'te → metin düzeltildi. K2 — durum satırının `Durum`'u doğrulamadan önce `✓` yazılmıştı → `⏳` / `✗ FAIL (tur 1)`.

**Yapım raporu karşılaştırması:** 2 uyuşmazlık — kriter 2 (yapım ✓, validator ~; D-428 sıfırlama/değiştirme yarışını anmıyor); kriter 5 kabul tablosunda "✓ / ~", öz denetimde ✓ (validator T342 payını ✓ saydı). Öz denetim tablosu var ve kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Gömülü kaynak `CommonPasswords.txt` (derlemeye girer, dış servis çağrılmaz)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T342-giris-oturum`
- **Commit:** son head PR açıklamasında
- **PR:** #453
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Yok — dış varsayım yok; ana dal CI durumunu yürütme ajanı ayrıca izler | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Hesap silme ve e-posta değiştirmenin `AccountLocks` payı (giriş, sıfırlama ve değiştirme bu PR.da kilitli, tur 1 Bulgu 1) | D-428 | hesap silen/e-posta değiştiren task (T517) aynı kilidi alır |
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
