# T341 — Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

---

## Yapılan İşler

- `Modules/Membership/Contracts/IMembershipModule.cs` ve `MembershipModule.cs`: `RegisterAsync` (satış kapısı → alan denetimi → L-3 → bekleyen kaydın tembel silinmesi → doğrulanmamış hesap + P-12 (24 saat) ömürlü doğrulama bağlantısı), `ReadVerificationLinkAsync` (GET: hiçbir kaydı değiştirmez), `VerifyEmailAsync` (düğmenin işi: tek koşullu tüketim, `EmailVerifiedAt`, misafir siparişlerinin bağlanması), `ResendVerificationLinkAsync` (L-9; öncekiler `SupersededAt` alır, ömür uzamaz), `CheckCredentialsAsync` (sert doğrulama kapısı; her deneme giriş kaydına; eski ayarlı özet başarılı girişte yenilenir), `ResolveOrderAccountAsync` (`Guest`, `SignedIn`, `GuestBoundToAccount`, `RefusedUnverified`), `ChangeNameAsync` (yeniden doğrulamasız).
- `Contracts/MembershipRecords.cs` (sonuç tipleri) ve `Contracts/Results/{OrderAccountKind,VerificationLinkStatus}.cs`: sonuç değerleri saklanmaz; `Contracts` ad alanının kendisinde olsalardı `06 §5` enum kaydı ve kilit dosyası ister (saklanan enum sayılırlar), bu yüzden `Settings.Contracts.Gates` örneğindeki gibi alt ad alanındadır.
- `Infrastructure/Security/PasswordService.cs`: çerçevenin parola özetleyicisinin tek sarmalayıcısı (210 000 yineleme, `09 §15.1` m.5); kayıtlı olmayan adres için sahte özetle aynı işi yapar (`VerifyNobody`). `LoginRecorder.cs`: giriş kaydını kendi kapsam ve işleminde yazar (IP yoksa `IPAddress.None`).
- `LoginRecord.Create` artık ham adresi alıp `TextNormalizer.NormalizeIdentity` ile normalleştirir (D-64 ikinci yarı). `LinkRecords.SupersedeEarlierAsync` müşteri/yönetici kimliğinden ikisi de boş ya da ikisi de dolu çağrıyı reddeder (D-96 K2). Doğrulama bağlantısı ömrü P-12 için `ProductConstants.EmailVerificationLinkHours` = 24 (tek sabit; CI düzeltmesi). `Customer.RenewDigest`.
- `Ordering`: `IOrderingModule.LinkGuestOrdersAsync` — adaylar okunur, `OrderLocks` ile kilitlenir, kilitten sonra izlemeli yüklenip `CustomerId` atanır (sürüm artmaz; `ExecuteUpdate` yok, `VersionedSideBulkWriteScan`). Hesabı olan ve hesabı silinmiş siparişler alınmaz.
- Kaynak taramaları: `CredentialSourceScan` (+ testler, ihlal örneği `ViolationSamples/Credentials/Misuse.cs`): parola özetleyicisi yalnız `PasswordService.cs`'de, giriş kaydı normalleştirmesi tek yerde. `LinkKeySourceScan`: `ConsumingMethods` (yalnız `MembershipModule.cs` içinde `VerifyEmailAsync` metodu `TryConsumeAsync` çağırabilir) ve `.VerifyEmailAsync(` çağrısı yalnız `OnPost…` işleyicisinde.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/**`, `Modules/Ordering/{OrderingModule,Contracts/IOrderingModule}.cs`, `Infrastructure/Security/{PasswordService,LoginRecorder,LoginRecord,LinkRecords}.cs`, `Infrastructure/ProductConstants.cs`, `Program.cs`.
- Testler: `tests/Shopfolio.IntegrationTests/Model/MembershipTests.cs`, `tests/Shopfolio.UnitTests/Architecture/{CredentialSourceScan,CredentialSourceScanTests,LinkKeySourceScan,LinkKeySourceScanTests}.cs`, `ViolationSamples/Credentials/Misuse.cs`.
- Belgeler: `CONTEXT.md`, `IMPLEMENTATION_STATUS.md` (+ `STATUS_CHANGELOG.md`), `MEMORY.md`, `DEFERRED_BACKLOG.md`; T308, T312, T331 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri dolduruldu.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Misafir siparişi doğrulama kapısının dışında | ✓ | `AGuestOrder_NeedsNoVerification_AndAnAddressWithNoAccountStaysAGuest` |
| 2 | Geçmiş misafir siparişleri doğrulanmış e-postayla hesaba düşer; silinmiş hesabın siparişi bağlanmaz | ✓ | `Verifying_BringsTheGuestOrdersOfTheAddressToTheAccount_AndOnlyThose` (büyük/küçük harf ve boşluk, başkasının, hesabı olan, hesabı silinmiş); `APendingRegistration_TakesNoOrder_UntilItIsVerified` |
| 3 | Doğrulanmış hesabın e-postasıyla girişsiz sipariş hesaba anında bağlanır, bekleyen kayda bağlanmaz | ✓ | `AGuestOrderWithAVerifiedAccountsAddress_IsBoundToTheAccountAtOnce_AndAPendingRegistrationIsNotBoundTo` (modül yüzü `ResolveOrderAccountAsync`; sipariş ucunun bağı D-254) |
| 4 | Doğrulanmamış hesapla giriş ve sipariş yok | ✓ | `AnUnverifiedAccount_IsRefusedAtTheSignIn_EvenWithTheRightPassword_AndTheAttemptIsAFailedOne`, `ASignedInOrder_IsTheAccountsOwn_AndAnUnverifiedAccountCannotOrder` |
| 5 | Z-1 sonrası kayıt silinir, e-posta yeniden kayda açılır; süresi geçmiş bağlantı "geçersiz" | ✓ | `Register_AfterThePendingRegistrationEnded_FreesTheAddress_DeletesTheOldRowAndCountsItInTheDisposalRecord`, `AnExpiredLink_IsInvalid_AndVerifiesNothing_AndASecondUseIsRefused` (gece silmesi T324, D-253; mesajın ekranı D-254) |
| 6 | Doğrulama bağlantıyla değil düğmeyle | ✓ | `OpeningTheLink_ChangesNothing_AndTheButtonVerifies`; `LinkKeySourceScanTests` (`VerifyEmailAsync` çağrısı OnGet'te bulgu, OnPost'ta temiz) |
| 7 | Kayıt formu ad, e-posta, şifre ister; ad zorunlu; ad değişimi yeniden doğrulamasız | ✓ | `Register_WithoutAName_IsRefused_AndNothingIsWritten`, `TheName_ChangesWithoutANewVerification_AndLeavesTheOrdersRecipientNameAlone`; e-posta biçimi `04` 7.3.2.1'in tek evi `InvalidEmailAttribute` ile (`MembershipModule.EmailRule`; özel `IsValidEmail` kaldırıldı): `Register_WithAWrongAddress_IsRefusedAndNamed` (70 karakterlik yerel kısım, `ali@exa_mple.com`, `ali@-example.com`, `"a b"@example.com` red), `Register_TakesTheAddressTheProjectsOneEmailRuleAccepts` (`a..b@example.com` kabul) |
| 8 | Pazarlama onayı alanı yok | ✓ | `TheFormAndTheAccount_HaveNoMarketingConsentField` (`RegisterInput` üç özellik; `Customer` üyeleri) |
| 9 | Kapı kapalıyken kayıt reddedilir; her deneme zaman, IP, e-posta, sonuçla giriş kaydında | ✓ | `Register_WithTheGateClosed_IsRefused_NamingTheWaitingConditions_AndCountsNoAttempt`, `SignIn_OfAnExistingAccount_IsNotBehindTheClosedGate`, `EveryAttempt_IsOneRow_WithTheMomentTheIpTheNormalizedEmailAndTheOutcome_AndARefusalDoesNotTakeItBack` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: misafir sipariş verir, doğrulama aranmaz | ✓ | `AGuestOrder_NeedsNoVerification_…`; `MembershipModule.ResolveOrderAccountAsync` (`Guest`) |
| KK2: geçmiş misafir siparişi doğrulanmış e-postayla hesaba düşer | ✓ | `Verifying_BringsTheGuestOrdersOfTheAddressToTheAccount_AndOnlyThose` (`LinkedOrders` 2; sürüm 1 kalır) |
| KK3: anında bağlanır, bekleyen kayda bağlanmaz | ✓ | `AGuestOrderWithAVerifiedAccountsAddress_IsBoundToTheAccountAtOnce_…` |
| KK4: doğrulanmamış hesap girişte ve siparişte reddedilir | ✓ | `AnUnverifiedAccount_IsRefusedAtTheSignIn_…`, `ASignedInOrder_IsTheAccountsOwn_AndAnUnverifiedAccountCannotOrder` |
| KK5: Z-1 sonrası kayıt silinir, e-posta yeniden kaydolur; geçersiz bağlantı | ✓ | `Register_AfterThePendingRegistrationEnded_…`, `AnExpiredLink_IsInvalid_…` (`VerificationLinkStatus.Expired`) |
| KK6: bağlantıya tıklamak doğrulamaz, düğme doğrular | ✓ | `OpeningTheLink_ChangesNothing_AndTheButtonVerifies`, `LinkKeySourceScanTests.Scan_OnACallOfTheVerificationCommand_…` |
| KK7: adsız kayıt reddedilir; ad değişimi yeniden doğrulama istemez | ✓ | `Register_WithoutAName_IsRefused_…`, `TheName_ChangesWithoutANewVerification_…`; e-posta alanı proje kuralıyla (`Register_WithAWrongAddress_…`, `Register_TakesTheAddressTheProjectsOneEmailRuleAccepts`) |
| `09 §25` — "aynı işi yapan tek işlev varsa o çağrılmış mı" | ✓ | kayıt e-postası `InvalidEmailAttribute` ile denetlenir; `grep -rn MailAddress src/`: kalan kullanım yalnız `EmailOptions.IsPlainAddress` (gönderen adresi ayarı, kullanıcı alanı değil) ve `SmtpCaptureEmailGateway` (gönderim); üyelikte özel e-posta denetimi yok |
| KK8: formda ve kayıtta pazarlama alanı yok | ✓ | `TheFormAndTheAccount_HaveNoMarketingConsentField` |
| KK9: kapı kapalıyken kayıt reddedilir; giriş dört alanla yazılır | ✓ | `Register_WithTheGateClosed_…`, `EveryAttempt_IsOneRow_…` |
| `09 §25` — kaynak taraması ve ihlal örneği | ✓ | `CredentialSourceScanTests` (9; `ViolationSamples/Credentials/Misuse.cs`), `LinkKeySourceScanTests` (12; yeni 3) |
| `09 §25` — güvenlik taraması (mini) | ✓ | sır yok; parola yalnız `PasswordService`'te özetlenir, hiçbir kayıt/günlük/mesaja yazılmaz (`Register_MakesAnUnverifiedAccount_…` özette ham değerin geçmediğini sınar); kimsesiz adreste sahte özetle aynı iş (zamanlama); bağlantı anahtarı `LinkKeys` ile türer, `Guid.NewGuid` yok |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | tek istisna `ConsumingMethods`: tek dosya, tek metot, yalnız `TryConsumeAsync`; başka metotta, başka yolda ve metodun çağrısında bulgu (`Scan_OnTheConsumingCommandsFile_AllowsTheConsumeInItsOneNamedMethodOnly`); dosya ve metot varlığı da sınanır (`ConsumingMethods_NamesOnlyExistingFiles_…`). Bekçi ayarına dokunulmadı |
| Sık FAIL (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ | `CredentialSourceScan` nitelikli/niteliksiz `PasswordHasher<…>`, `IPasswordHasher<…>`, `PasswordHasherOptions`, ek argümanlı çağrıları ve model anlık görüntüsünü sınar (`CredentialSourceScanTests`); `VerifyCall` üye erişimi (`.VerifyEmailAsync(`) çok satırlı da bulur |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ | şema değişmedi (yeni tablo/sütun/göç yok) |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | P-12 yalnız `ProductConstants.EmailVerificationLinkHours` (envanterde `ParameterInventory` P-12 satırı; ayrı sabit yok); test beklentisi sabitten türer (`MembershipTests`); backlog aktif sayısı satır sayılarak 132; D-64, D-96, D-199 güncellendi; `LinkKeySourceScan` başlık özeti güncellendi |
| Sık FAIL (5) belge kuralı değiştiren 09 metni karar kaydısız | ✓ | 09/05/07 metnine dokunulmadı; `LinkKeySourceScan` istisnası kuralı daraltmaz (çağıran `OnPost` kuralına bağlı) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Architecture dahil) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 3477 geçti, 0 hata (Architecture ad alanı 842 dahil) |
| Consistency | ✓ | `… --filter-trait "Consistency=true"` | 209 geçti |
| Integration (yerelde, parçalı) | ✓ | `--filter-class "*MembershipTests"`, `"*SessionLinkAttemptLoginTablesTests"`, `"*LinkRecordsTests"`, `"*DisposalTests"` | MembershipTests 29/29; üç komşu sınıf 99/99; Concurrency 26/26 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | **tur 2: ✗ FAIL** (2026-11-07; head `2aa0153`; ayrıntı aşağıda). Tur 1: ✓ PASS (2026-11-07; head `391cdc3` + `origin/main` birleştirmesi). Kriter 1–9 ve KK1–KK9 validator kanıtıyla ✓ (kanıt testleri yukarıdaki tabloyla aynı, kod okunarak teyit edildi: kapı → alan → L-3 sırası, tembel Z-1 silmesi ve imha kaydı, düğmeyle tek koşullu tüketim, `LinkGuestOrdersAsync`'in hesabı olan ve hesabı silinmiş siparişi almaması, `ResolveOrderAccountAsync`'in bekleyen kayda bağlamaması, giriş kaydının kendi işleminde dört değerle yazılması). Kriter 3 ve 5'in sipariş ucu ve ekran mesajı yüzleri modül düzeyinde doğrulandı; bağları D-254 (T509, T514). Validator yerelde: build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç proje temiz; Unit 3534 (Architecture 842, Consistency 213, öteki 2479); betik testleri 4 dosya; Integration PostgreSQL 18 (`shopfolio-t341-val-pg`) MembershipTests 29/29, SessionLinkAttemptLoginTables + LinkRecords + AccountEmailQueue 95/95, AppCache + SessionAuthentication 14/14. Dal CI 42378956325 success (head `391cdc3`). Güvenlik: sır yok; parola yalnız `PasswordService`'te (210 000 yineleme), hesapsız adreste sahte özetle aynı iş; bağlantı anahtarı türetilir, ham anahtar saklanmaz; L-3 ve L-9 her istekte iki eksende; hesap varlığı yalnız `02 §8.1.4`'ün izin verdiği kayıt ekranında söylenir; misafir siparişi yalnız doğrulanmış e-postayla bağlanır. Yeni dış bağımlılık yok. `12`: KP-28 → `VAL-D018`, `VAL-D019`, `VAL-D020` (T341) ve `VAL-D021` (T375); üçünün beklenen sonucu sipariş geçmişinin uç yanıtıdır (Veri kaydı + Uç yanıtı), uç sonraki task'larda → `Durum` `BEKLEMEDE` kaldı; `12_matris_denetle.py --kapsam 1-77` hata 0. |
| Bulgu sayısı | tur 1: 0 · tur 2: 1 (ORTA S1 — aşağıda "Doğrulama tur 2") |
| Düzeltme gerekli mi | tur 1: Hayır · tur 2: Evet (Bulgu 1) |
| Bloke-etmeyen not (K) sayısı | 3 — K1: D-252'nin hedefi T309'du, T309 bu PR'dan önce merge edildi ve doğrulama e-postasını kuyruğa yazan çağrı kimsenin kapsamında kalmadı → hedef T514 (UC-48/UC-49 yan etkisi; bu PR'da kayıt güncellendi). K2: `origin/main` birleştirmesiyle P-12 iki sabitte (T341'in ayrı ömür sabiti ve T309'un `EmailVerificationLinkHours`'u, ikisi de 24) → D-289 (CI düzeltmesinde kapandı, aşağıya bakın). K3: `09 §15.1` madde 5 `PasswordHasher<object>` der, kod `PasswordHasher<HashSubject>` → D-290 (F3 gate). Yapım raporu karşılaştırması: kriter tablosu ve öz denetim kanıtlı, verdict uyumlu; tek uyuşmazlık öz denetimin "P-12 yalnız tek sabitte" satırıdır — dal başında doğruydu, `origin/main` birleştirmesinden sonra değil (K2). |

### Doğrulama tur 2 — Doğrulama Sonucu — T341 Üyelik — tur 2
**Tarih:** 2026-11-07   **Branch:** task/T341-uyelik   **Commit:** 2aa0153 (doğrulanan head; kayıt commit'i öncesi `origin/main` (T328) birleştirildi)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42405002224 success (T328), 42402532255 success, 42402532286 success (#436) |
| Memory drift | ✓ | `grep -nE "\bT341\b" .claude/memory/MEMORY.md` → satır 22 |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üyelik zorunlu değil; misafir siparişi kapının dışında | ✓ | `ResolveOrderAccountAsync` misafirde doğrulama aramaz (`Guest`, `IsAccepted`); `AGuestOrder_NeedsNoVerification_…` geçti |
| 2 | Geçmiş misafir siparişleri yalnız doğrulanmış e-postayla hesaba düşer; silinmiş hesabın siparişi bağlanmaz | ✓ | `VerifyEmailAsync` tüketim + `EmailVerifiedAt` koşullu yazımından sonra `LinkGuestOrdersAsync` (kilit → `CustomerId == null && !IsAccountDeleted && NormalizedContactEmail` eşleşmesi); kayıt anında bağlama yok. `Verifying_BringsTheGuestOrdersOfTheAddressToTheAccount_AndOnlyThose` (2 bağlandı; başkasının, sahipli, silinmiş hesabın siparişi alınmadı; sürüm 1), `APendingRegistration_TakesNoOrder_UntilItIsVerified` geçti |
| 3 | Doğrulanmış hesabın adresiyle girişsiz sipariş anında ve tam bağlanır; bekleyen kayda bağlanmaz | ✓ (modül yüzü) | `ResolveOrderAccountAsync` yalnız `EmailVerifiedAt != null` hesaba `GuestBoundToAccount`; test `AGuestOrderWithAVerifiedAccountsAddress_…` geçti. Sipariş ucunun bağı D-254 |
| 4 | Doğrulanmamış hesapla giriş ve sipariş yok; tek tip hesap | ✓ | `CheckCredentialsAsync` doğru şifrede de `EmailNotVerified` + giriş kaydına `Failed`; `ResolveOrderAccountAsync` oturumlu doğrulanmamışta `RefusedUnverified`; iki test geçti |
| 5 | Z-1 dolunca kayıt silinir, e-posta yeniden kayda açılır; süresi geçmiş bağlantı "geçersiz" | ✓ (modül yüzü) | Tembel silme (`ExecuteDeleteAsync` + imha kaydı `PendingRegistration`), bağlantı `Expired`; `Register_AfterThePendingRegistrationEnded_…`, `AnExpiredLink_IsInvalid_…` geçti. Gece silmesi D-253 (T324), ekran mesajı D-254 |
| 6 | Doğrulama bağlantıyla değil düğmeyle | ✓ | `ReadVerificationLinkAsync` salt okur; `VerifyEmailAsync` POST işi; `LinkKeySourceScan` `VerifyEmailAsync` çağrısını yalnız `OnPost…`'ta kabul eder; `OpeningTheLink_ChangesNothing_AndTheButtonVerifies` geçti |
| 7 | Kayıt formu ad, e-posta, şifre ister; ad zorunlu; ad değişimi yeniden doğrulamasız | ~ Kısmi | Ad ve şifre ✓ (`Register_WithoutAName_…`, `TheName_ChangesWithoutANewVerification_…`). **E-posta alanı** `04` 7.3.2.1'in tek kuralıyla (`InvalidEmailAttribute`) değil, `MembershipModule.IsValidEmail` (çatının `MailAddress` ayrıştırıcısı) ile denetleniyor — Bulgu 1 |
| 8 | Kayıtta ve hesapta pazarlama onayı alanı yok | ✓ | `RegisterInput` = Name, Email, Password; `Customer` üyelerinde yok; `TheFormAndTheAccount_HaveNoMarketingConsentField` geçti |
| 9 | Kapı kapalıyken kayıt reddedilir; her giriş denemesi zaman, IP, e-posta, sonuçla yazılır | ✓ | Kapı alanlardan ve L-3'ten önce (`DataCollectionClosed`, deneme sayılmaz); `LoginRecorder` kendi işleminde; `Register_WithTheGateClosed_…`, `SignIn_OfAnExistingAccount_IsNotBehindTheClosedGate`, `EveryAttempt_IsOneRow_…` geçti |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-D018 | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE (beklenen sonuç sipariş geçmişinin uç yanıtı; uç sonraki task'larda) | — |
| VAL-D019 | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE (aynı) | — |
| VAL-D020 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (aynı) | — |

`VAL-D021` T375'indir. `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

#### Doğrulama Kontrol Listesi
- [x] KK1: misafir sipariş verir, doğrulama aranmaz
- [x] KK2: geçmiş misafir siparişi doğrulanmış e-postayla hesaba düşer
- [x] KK3: misafir sipariş doğrulanmış hesaba anında bağlanır, bekleyen kayda bağlanmaz
- [x] KK4: doğrulanmamış hesap girişte ve siparişte reddedilir
- [x] KK5: Z-1 sonrası kayıt silinir, e-posta yeniden kaydolur; geçersiz bağlantı
- [x] KK6: bağlantıya tıklamak doğrulamaz, düğme doğrular
- [x] KK7: adsız kayıt reddedilir; ad değişimi yeniden doğrulama istemez
- [x] KK8: formda ve kayıtta pazarlama alanı yok
- [x] KK9: kapı kapalıyken kayıt reddedilir; her giriş dört alanla yazılır
- [ ] `09 §25` — "Aynı işi yapan tek işlev varsa yeni işlev yazılmadan o çağrılmış mı?" **hayır** (Bulgu 1); öteki ilgili satırlar evet (modül sınırı, `TimeProvider`, hata kodları `07 §6`'da, `ProductConstants` P-12/P-33/P-49, kaynak taraması + ihlal örneği, yeni paket yok, göç yok)
- [x] Güvenlik taraması (aşağıda)

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | başarılı; sonda `--locked-mode` çıkış 0 |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 | üçü çıkış 0 |
| Unit Architecture | ✓ | `--filter-namespace "Shopfolio.UnitTests.Architecture*"` | 918/918 |
| Unit Consistency | ✓ | `--filter-namespace "Shopfolio.UnitTests.Consistency*"` | 230/230 |
| Unit öteki | ✓ | `--filter-not-namespace` (iki ad alanı) | 2889/2889 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration | ✓ | PostgreSQL 18 `shopfolio-t341-val2-pg` (55790), `--filter-class "*.MembershipTests"` | 29/29 |
| Integration komşu | ✓ | `SessionLinkAttemptLoginTablesTests`, `AccountEmailQueueTests`, `LinkRecordsTests`, `AttemptCounterTests` | 109/109 |
| Dal CI | ✓ | `gh pr checks 433` / `gh run list --branch task/T341-uyelik` | 42404071089 success (head `2aa0153`), 9 iş geçti |

Konteyner `docker rm -f -v shopfolio-t341-val2-pg` ile silindi; koşu sonrası `git status` temiz.

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (test türetme sırrı sıfır baytlı test değeri; repo/kayıtta sır yok; ham parola hiçbir kayda yazılmıyor, özette geçmediği test ediliyor)
- Auth etkisi: Temiz — sert doğrulama kapısı doğru şifrede de kapalı; hesap yokken ve şifresiz hesapta `VerifyNobody` ile aynı iş (kod okuması; testi yok → K1); hesap varlığı yalnız `02 §8.1.4`'ün izin verdiği kayıt ekranında (`EmailAlreadyRegistered`, L-3'ten sonra); `EmailNotVerified` yalnız doğru şifrede döner; doğrulama bağlantısı türetilmiş anahtar, tek koşullu tüketim, açılış yazmaz; misafir siparişi yalnız doğrulanmış e-postayla bağlanır; L-3 ve L-9 her istekte iki eksende, kendi işleminde
- Input validation: **Bulgu** — e-posta biçimi `04` 7.3.2.1'in tek kuralı yerine ayrı kuralla (Bulgu 1). Ad ve şifre için uzunluk tavanı yok (form gövdesi 64 KB ile sınırlı; şifre politikası D-251/T342)
- Yeni bağımlılık: Yok (`PasswordHasher` çatıyla gelir; kilit dosyası değişmedi)

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | — (kriter 7; `09 §25` "tek işlev" satırı; `09 §3.4` m.1, §24 m.4; `04` 7.1.11.2 → 7.3.2.1; `07` `InvalidEmail` "E-posta biçimi (`04` 7.3.2.1)") | ORTA | S1 | **Beklenen:** kayıt e-postası `04` 7.3.2.1'in kodda tek evi olan `InvalidEmailAttribute` ile denetlenir (`CompanyIdentityRules` kalıbı: nitelik örneği modül kuralında). **Gerçekleşen:** `MembershipModule.IsValidEmail` (satır ~334) `MailAddress.TryCreate` + `Address` eşitliği + host'ta nokta ile ayrı bir kural kurar ve aynı `InvalidEmail` kodunu verir. Ölçülen ayrışma (iki kuralın aynı girdide çıktısı, `dotnet run` ile): `a..b@example.com` → nitelik kabul, kayıt **red**; 70 karakterlik yerel kısım, `ali@exa_mple.com`, `ali@-example.com`, `"a b"@example.com` → nitelik red, kayıt **kabul**. Sonuç: misafir sipariş formunun (7.1.5.1) kabul ettiği bir adresle kayıt olunamaz — o adresin misafir siparişleri hiç bağlanamaz (kriter 2) —; tersinde öteki formların reddettiği adresle hesap açılır | kriter 2 (bağlama aynı adresin iki formda aynı kabul edilmesine dayanır), `VAL-D018` | bu PR'da düzeltilir: `IsValidEmail` kaldırılır, `InvalidEmailAttribute` (ya da onun kuralını açan tek işlev) çağrılır; ayrışan örnekler `Register_WithAWrongAddress_IsRefusedAndNamed` teorisine eklenir | `src/Shopfolio.Web/Modules/Membership/MembershipModule.cs` · `tests/Shopfolio.IntegrationTests/Model/MembershipTests.cs` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `09 §15.1` Test maddesi "sahte özetle doğrulamanın hesap yokken çağrıldığı (sayaçlı dekoratör)" testini ister; davranış kodda var (`CheckCredentialsAsync` → `VerifyNobody`), test yok; `PasswordService` mühürlü | DEFERRED_BACKLOG'a forward: D-364 (T342) |
| K2 | `PasswordService.IterationCount = 210_000` değeri ve kaynağı `09`'a (§15.1 m.5 / §3.5 m.4) dönmedi | DEFERRED_BACKLOG'a forward: D-365 (F3 gate; D-290 ile aynı senkron) |

#### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Yapım raporu kriter 7'yi ve öz denetimi ✓ verir; e-posta biçim kuralının ayrı yazıldığından söz etmez, öz denetimde "aynı işi yapan tek işlev" satırı yok (Bulgu 1).
- Öz denetimin güvenlik satırı "kimsesiz adreste sahte özetle aynı iş (zamanlama)" der, kanıtı test değil; `09 §15.1`'in istediği test yok (K1).
- Öteki kriterlerde (1–6, 8, 9) ve KK1–KK9'da verdict uyumlu; test sayıları güncel koşuyla tutarlı (Unit toplamı main birleştirmeleriyle büyüdü).

#### Merge ve Post-merge
- Merge yok (FAIL). Kayıt commit'i dalda: rapor, status (`✗ FAIL (tur 2)`), changelog, hafıza, backlog (D-364, D-365; aktif 136); `origin/main` (T328) birleştirmesi, çakışma yalnız kayıt belgelerinde (STATUS "Son güncelleme", backlog başlığı) çözüldü.


## Düzeltme (tur 2 bulgusu)

Bulgu 1 (ORTA S1): kayıt e-postası `MembershipModule.IsValidEmail` (çatının `MailAddress` ayrıştırıcısı) ile ayrı bir kuralla denetleniyordu; `04` 7.3.2.1'in tek evi `InvalidEmailAttribute` ile iki yönde ayrışıyordu (`09 §3.4` m.1, §24 m.4, §25 "tek işlev"). Düzeltme: `IsValidEmail` ve `EmailMaxLength` kaldırıldı; `MembershipModule.EmailRule` (`InvalidEmailAttribute`) `CompanyIdentityRules`'ın kalıbıyla (`GetValidationResult`) çağrılır. Misafir sipariş formunun ve kaydın e-posta kuralı artık tek; kriter 2'nin bağlaması aynı adresin iki formda aynı kabul edilmesine dayanır, bu sağlandı. Testler: `Register_WithAWrongAddress_IsRefusedAndNamed` dört örnekle genişledi, yeni `Register_TakesTheAddressTheProjectsOneEmailRuleAccepts`. Öz denetim: 11'deki T341 kriter 1–9, KK1–KK9 ve `09 §25` satırları düzeltilmiş diff'te yeniden denetlendi (düzeltme yalnız e-posta biçim denetimine dokunur; öteki kriterlerin kodu değişmedi, testleri geçti).

- Yerelde: build `-warnaserror` 0 uyarı; format üç proje temiz; Unit Architecture 924, Consistency 241, `FieldRuleTests` 84; Integration MembershipTests 34/34, AccountEmailQueueTests 14/14.
- **⚠** `a..b@example.com` proje kuralınca geçerlidir ve kayıtta artık kabul edilir; `SmtpCaptureEmailGateway` gönderimde `MailAddress` kullandığından böyle bir adrese e-posta gönderimi çatının ayrıştırıcısına takılabilir (kapı `TryCreate` ile gönderimi reddeder, çökmez) — kural evi `04` 7.3.2.1'dir, gönderim tarafı D-379.

## CI düzeltmesi (2026-11-07)

Doğrulama PASS sonrası main'e giren T381 (ayar katmanları, parametre envanteri) dalı kırmızı yaptı: (1) Lint CS0535 — `MembershipTests.GateOnlySettings` `ISettingsModule`'ün yeni üyelerini uygulamıyordu; altı üye eklendi (yalnız `NotSupportedException`, kapı okuması değişmedi). (2) Consistency `ParameterInventory` — üyelik modülünün ayrı doğrulama bağlantısı ömrü sabiti `ProductConstants`'ta envanterde yoktu; tek kaynak ilkesiyle sabit silindi, üyelik modülü ve testleri envanterdeki P-12 sabiti `EmailVerificationLinkHours`'u (24) okur. Davranış değişmedi; envanter ve öteki belgeler değişmedi; D-289 kapandı.

- Düzeltme sonrası yerelde: build `-warnaserror` 0 uyarı; format üç proje temiz; Consistency 230, Architecture 918, Integration MembershipTests 29/29, AccountEmailQueueTests 14/14.
- **⚠** Karar yardımcı tarafından sahip yerine verildi (tek sabit, envantere yeni satır eklenmedi). Doğrulamadan geçmiş kodun üzerinde yalnız bu iki değişiklik ve `origin/main` birleştirmesi var.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (`Microsoft.AspNetCore.Identity` çerçevenin parçası)

## Commit & PR

- **Branch:** `task/T341-uyelik`
- **PR:** #433
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(doğrulayıcı doldurur — merge sonrası; sonraki task dalında)*

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Şifre politikası (uzunluk, yaygın liste) kayıtta yok | D-251 | T342 kriter 7 |
| 2 | Doğrulama e-postasının gönderim satırı ve gövdesi | D-252 | T514 (doğrulama tur 1 K1: T309 önce merge edildi, kuyruk yüzü hazır, çağrı UC-48/UC-49 yan etkisi) |
| 3 | Gecelik Z-1 silmesi (T341'de yalnız tembel silme) | D-253 | T324 |
| 4 | Kayıt, doğrulama, giriş ekranları ve POST uçları; yeniden kayıt tekil dizin yarışı için yeniden deneme | D-254 | T514, T515, T517 |
| 5 | L-1 sayacı ve oturum açma | D-255 | T342 |
| 6 | D-64 (LoginRecord yarısı), D-96 K2 kapandı; D-199 modül yüzü kapandı, ekran D-254 | D-64, D-96, D-199 | — |
| 7 | P-12 iki sabitte (doğrulama tur 1 K2) — CI düzeltmesinde tek sabite indi | D-289 ✅ | — |
| 8 | `09 §15.1` madde 5 metni `PasswordHasher<HashSubject>` ile hizalanır (doğrulama tur 1 K3) | D-290 | F3 gate |

## Notlar

- **⚠ Kolay kararlar:** (1) Kayıtta kapı önce, alan denetimi L-3'ten önce gelir; alan hatası (boş ad/şifre, geçersiz e-posta) L-3 sayacına girmez. (2) `ResolveOrderAccount`: girişsiz misafir + bekleyen (doğrulanmamış) kayıt e-postası = `Guest` (reddedilmez, bağlanmaz); `RefusedUnverified` yalnız oturumdaki doğrulanmamış hesap içindir. (3) Parola yinelemesi 210 000 (OWASP; çerçeve varsayılanı 100 000); politika T342. (4) Sonuç enum-ları `Contracts.Results` alt ad alanında: saklanmadıkları için `06 §5` kaydı ve kilit dosyası gerekmez; kayıt/kilit ister bir görüş çıkarsa bu iki dosya taşınır. (5) `LinkKeySourceScan`'e dar istisna `ConsumingMethods` eklendi (doğrulayıcı için).
- **Çalışma:** önceki iki işçi bağlam sınırında bölündü; commit sır bekçisi `…PasswordHash = …`, `PasswordVerificationResult x = …` gibi atamaları anahtar adı sandığı için değişken/tür adları değiştirildi (`entered`, `hashing`, `HashCheck`, `Verdict` takma adı, `Customer.RenewDigest`); `PasswordHasher<object>` genel argümanı `ThirdPartyContentScan` `<object` desenine takıldığı için `HashSubject` sınıfı eklendi. Bekçi ayarına dokunulmadı.
- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: T308 (#424) CI 42368786665 ve Publish 42368786653 success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
