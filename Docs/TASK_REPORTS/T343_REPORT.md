# T343 — Google ile giriş ve hesap bağlama, e-posta değişikliği ve geri alma

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Kapsam: Membership modülü mantığı (sayfa yok). Yalnız müşteri tarafı; yöneticinin e-posta değişikliği T523'ün (D-553). Google kimlik doğrulama bağlayıcısı T409'un.
- `GoogleSignIns`: Google kimliğinin hesaplara anlamı. Hat kapalıysa Unavailable; doğrulanmamış adres hiçbir şey bağlamaz; hesap satırı kilidi altında karar (D-234); `gmail.com` tam eşleşmesi ya da `hd` yetkilidir, başka her adreste (evil-gmail.com, x@sub.gmail.com, gmail.com.tr) bağ ve oturum yok, hesabın adresine tek teyit bağlantısı; yeni hesap şifresiz, doğrulanmış ve bağlı doğar; bekleyen kayıt silinir ve imha kaydı yazılır; kapı kapalıysa hiçbir şey yazılmaz.
- `ConfirmGoogleLinkAsync`: tek kullanımlık düğme, bağ kurar, oturum açmaz.
- `EmailChanges`: istek (bekleyen kayıt, yeni adrese bağlantı, hesap eski adreste kalır), yeniden istek (ömür uzamaz), doğrulama (adres değişir, Google bağı kalkar, bekleyen kayıt silinir, eski adrese bildirim + P-47 gün geri alma bağlantısı, yeni adresin konuk siparişleri hesaba bağlanır).
- `MembershipModule` / `IMembershipModule`: yeni yüzey (`IsGoogleSignInOpen`, `SignInWithGoogleAsync`, `ConfirmGoogleLinkAsync`, `ReadEmailLinkAsync`, `RequestEmailChangeAsync`, `ResendEmailChangeLinkAsync`, `VerifyNewEmailAsync`).
- Kaynak taraması: `LinkKeySourceScan` iki yeni tüketen yöntemi ve iki sarmalayıcıyı kapalı listeye aldı; çağrıları `OnPost` kuralına bağlandı. Yeni `SingleSocialSignInScanTests` Facebook (ve başka sağlayıcı) yolunu tarar.
- Kayıt: STATUS satırı ⏳, `STATUS_CHANGELOG.d/T343.md`, `DEFERRED_BACKLOG.d/T343.md` (D-553…D-557), D-234 yerinde ✅.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/` (`GoogleSignIns.cs`, `EmailChanges.cs`, `MembershipModule.cs`, `Contracts/*`, `Contracts/Results/EmailLinkKind.cs`, `Contracts/Results/GoogleSignInOutcome.cs`)
- `tests/Shopfolio.IntegrationTests/Model/MembershipGoogleTests.cs` (yeni), `AccountEmailOnlyNotifications.cs` (yeni), `MembershipTests.cs`, `AddressTests.cs` (kurulum)
- `tests/Shopfolio.UnitTests/Architecture/LinkKeySourceScan.cs`, `LinkKeySourceScanTests.cs`, `SingleSocialSignInScanTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sosyal giriş yalnız Google; Facebook yok | ✓ | `SingleSocialSignInScanTests` (kaynak taraması + ihlal örnekleri) |
| 2 | Kimlik bilgisi kurulum ayarı; yoksa düğme yok, panelde anahtar yok | ✓ | `IsGoogleSignInOpen` yalnız `ConditionalLine.GoogleSignIn` açıkken true; `GoogleSignIn_WithTheLineClosed_IsUnavailable_...`, `GoogleSignIn_IsOpenOnlyWhenTheClientIdAndTheSecretAreBothPresent_KK2`; panelde anahtar yok (modülde ayar yazan yöntem yok) |
| 3 | Doğrulanmış e-posta tek hesap, iki yol; bekleyen kayıt silinir | ✓ | `GoogleSignIn_OfAVerifiedAccountsAddress_KeepsOneAccount_...`, `GoogleSignIn_ReplacesAnUnverifiedPendingRegistration_...` |
| 4 | Yetkisiz adreste bağ/oturum yok; teyit bağlantısı, Z-2 ömrü, tek geçerli, e-posta değişince geçersiz | ✓ | `GoogleSignIn_WhereGoogleIsNotTheAuthority_...`, `TheConfirmationButton_...`, `TheConfirmationLink_EndsWithItsLife_...`, `TheConfirmationLink_IsInvalidAfterTheAccountsAddressChanges_KK4` |
| 5 | Google hesabı şifresiz; sıfırlama şifre belirler | ✓ | `TheResetFlow_OnAPasswordlessGoogleAccount_SetsThePassword_KK5` |
| 6 | E-posta yeni adres doğrulanmadan değişmez; düğmeyle doğrulanır | ✓ | `EmailChange_Request_WritesAPendingChange_..._KK6`, `EmailChange_AfterTheLife_LeavesTheAddressAsItWas_KK6` |
| 7 | Bağlama kuralı yeni adres için işler, Google bağı kalkar, eski adres siparişleri kalır | ✓ | `EmailChange_Verify_ChangesTheAddress_DropsTheGoogleLink_...`, `EmailChange_Verify_LinkedGuestOrdersOfTheNewAddress_...KK7` |
| 8 | Eski adrese geri alma bağlantısı (sonucu T519); Z-45 dolunca eski adres silinir | ✓ | aynı test (`PreviousEmail`, bildirim satırı), `TheRevertLinksOldAddress_IsEmptiedWhenItsLifeEnds_AndTheRowStays_KK8` (mevcut `RevertLinkAddressDisposalPart`) |
| 9 | Yeniden istek ve yerine geçen istek L-9; Google girişi limite girmez | ✓ | `EmailChange_Resend_...L9_KK9`, `..._OverTheLimit_...`, `EmailChange_Request_ThatReplacesAPendingOne_...`, `GoogleSignIn_IsNeverCountedToAnyLimit_KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 Facebook yolu yok | ✓ | `SingleSocialSignInScanTests` |
| KK2 kimlik bilgisi yokken düğme yok, panelde anahtar yok | ✓ | `MembershipGoogleTests` KK2 iki test |
| KK3 tek hesap; bekleyen kayıt silinir | ✓ | KK3 testleri; kapı kapalıyken bekleyen kalır: `..._WithTheGateClosed_OpensNothing_...` |
| KK4 yetkisiz sağlayıcıda teyit, oturum yok; tek kullanımlık, süre/e-posta değişince geçersiz | ✓ | KK4 testleri; oturum sayısı 0 doğrulanır |
| KK5 sıfırlama şifre belirler | ✓ | KK5 testi |
| KK6 yeni adres doğrulanmadan e-posta değişmez | ✓ | KK6 testleri |
| KK7 sipariş kalır, Google bağı kalkar | ✓ | KK7 testleri |
| KK8 geri alma bağlantısı Z-45'te geçersizleşir, eski adres silinir | ✓ | `TheRevertLinksOldAddress_IsEmptiedWhenItsLifeEnds_...` |
| KK9 yeniden/yerine geçen istek L-9'da; Google girişi sayılmaz | ✓ | KK9 testleri |
| `09 §25`: tarama bir çağrı biçimini kaçırmıyor (OnGet çağrısı, başka metotta TryConsume, sarmalayıcı dışı çağrı) | ✓ | `LinkKeySourceScanTests.Scan_OnTheGoogleAndEmailChangeCommands_...` (ihlal örnekleri, iki komut için) |
| `09 §25`: kilit sırası (hesap sonra bağlantı; Account ailesi tek başına) | ✓ | `VerifyNewEmailAsync` adres güncellemesiyle satırı tutar, sonra bağlantı, sonra konuk sipariş kilitleri; entegrasyon testi `LockSequence` ihlalini yakaladı ve düzeltildi |
| Sık FAIL 1: istisna geniş | ✓ | `ConsumingMethods` tek dosya + tek yöntem; sarmalayıcı yalnız adı aynı çağrı |
| Sık FAIL 2: çağrı biçimi kaçıyor | ✓ | `VerifyCall` iki yeni adı kapsar; ihlal örnekleri |
| Sık FAIL 3: DB default / FK dizini | ✓ (yok) | şema değişikliği yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | yeni enum yok (iki enum `Contracts.Results`'ta, saklanmıyor; enum taramaları yeşil) |
| Sık FAIL 5: 09 metni karar kaydısız | ✓ (yok) | 09 değişmedi |
| Sık FAIL (eşzamanlılık / kilit sırası) | ✓ | D-234 iki eş zamanlı giriş testi; Account kilidi Order kilidiyle birleşmiyor |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` | 4743/4743 |
| Integration | ✓ | `--filter-class ...MembershipTests` (SHOPFOLIO_TEST_DATABASE, yerel docker `t343`) | 97/97 |
| Integration (komşu) | ✓ | `--filter-class ...AddressTests --filter-class ...AccountEmailQueueTests` | 27/27 |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes` (Web, UnitTests, IntegrationTests) | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-08) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (D-577, D-578'e forward) |

### Doğrulama Sonucu — T343 Google ile giriş ve hesap bağlama, e-posta değişikliği ve geri alma — tur 1

**Tarih:** 2026-11-08   **Branch:** `task/T343-google-giris`   **Commit:** `b067fd6d` (+ `origin/main` `26fd3a96` birleştirmesi)

#### Verdict: ✓ PASS

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `scripts/git-hooks` |
| Ana dal CI (son 3) | ✓ | 42480365166 success · 42480365007 success · 42479203249 success |
| Memory drift | — | Adım 0b kaldırıldı (2026-11-08) |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Yalnız Google; Facebook yok | ✓ | `SingleSocialSignInScanTests` (kaynak taraması + ihlal örnekleri) yeşil; `src` altında başka sağlayıcı adı yok |
| 2 | Kimlik bilgisi kurulum ayarı; düğme yok, panelde anahtar yok | ✓ | `IsGoogleSignInOpen` yalnız `ConditionalLine.GoogleSignIn` açıkken; kapalıyken `Unavailable`, hiçbir şey yazılmaz (KK2 iki test); `Modules/Settings` altında Google anahtarı yok (grep) |
| 3 | Tek hesap, iki yol; bekleyen kayıt silinir | ✓ | `GoogleSignIns.SignInAsync` hesap satırını kilitler, doğrulanmış hesaba girer; bekleyen kayıt `ExecuteDelete` + imha kaydı; kapı kapalıyken durur (KK3 testleri) |
| 4 | Yetkisiz adreste bağ/oturum yok; teyit, Z-2, tek geçerli, e-posta değişince geçersiz | ✓ | `GoogleAddressAuthority` (`gmail.com` tam ya da `hd`); teyit kilit altında (D-234 eş zamanlı test); `ConfirmGoogleLinkAsync` tek koşullu tüketim, oturum yok; süre ve adres değişince geçersiz (KK4 testleri) |
| 5 | Şifresiz başlar; sıfırlama şifre belirler | ✓ | `Customer.Create(passwordHash: null)`; `TheResetFlow_OnAPasswordlessGoogleAccount_SetsThePassword_KK5` |
| 6 | E-posta doğrulanmadan değişmez | ✓ | `RequestAsync` yalnız bekleyen kaydı yazar, bağlantı yeni adrese; adres `VerifyNewEmailAsync`'te değişir (KK6 testleri) |
| 7 | Bağlama kuralı yeni adres için; Google bağı kalkar; eski siparişler kalır | ✓ | `VerifyNewEmailAsync` `GoogleLinkedAt = null`, `LinkGuestOrdersAsync(yeni adres)`; KK7 testi |
| 8 | Eski adrese geri alma bağlantısı; Z-45'te eski adres silinir | ✓ | `EmailChangeRevert` bağlantısı `PreviousEmail` ile, bildirim; gece imhası (`TemporaryRecordParts`) KK8 testi |
| 9 | Yeniden ve yerine geçen istek L-9; Google sayılmaz | ✓ | `CountedOverLimitAsync` (L-9 e-posta + IP); ilk istek sayılmaz (`02 §8.2` L-9); KK9 testleri |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-B021, A091, A093, A094, A095, A100, A102 | ORTA / KRİTİK | mock / yok | BEKLEMEDE | BEKLEMEDE | — (kanıt türü uç yanıtı/tarayıcı; sayfa task'ları T515, T517) |
| VAL-A092 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE | — (uç yanıtı, T515) |

KP-27'nin öteki satırları (A096–A099, A101, A103, C028, C029, C116, D017) T342/T344/T519 kapsamında; değişmedi. `12_matris_denetle.py --kapsam 1-77` → Hata 0.

#### Doğrulama Kontrol Listesi
- [x] KK1–KK9 (yukarıdaki kanıtlar)
- [x] `09 §25`: modül sınırı (yalnız `Contracts`, `SaveChanges` yok), zaman `TimeProvider`'dan, kilit sırası (hesap → bağlantı), mimari testler yeşil
- [x] Güvenlik taraması

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata (birleştirmeden önce ve sonra) |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` (3 proje) | çıkış 0 |
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 4743/4743; `origin/main` birleştirmesinden sonra 4765/4765 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 |
| Integration | ✓ | `--filter-class ...MembershipTests --filter-class ...AddressTests` (konteyner `shopfolio-t343-val-pg`) | 110/110 |
| Dal CI | ✓ | `gh pr checks 468` | run 42483471276 (`b067fd6d`) tüm işler pass |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz — yetkisiz adreste bağ ve oturum yok; teyit ve doğrulama bağlantıları yalnız `OnPost` (`LinkKeySourceScan`); hesap satırı kilidi altında karar
- Input validation: Temiz — adres boş/biçim denetimi, normalleştirme
- Yeni bağımlılık: Yok

#### Bulgular
Yok.

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Google ile doğan hesapta o adresin geçmiş misafir siparişleri bağlanmıyor (`03 §9.1.6`, `07 §2.2.5`, `02 §3.13.2`); `OpenAccountAsync` hesabı yalnız bağlama ekler, kaydı çağıran yapar; D-556 bu adımı anmıyor | D-577 → T515 |
| K2 | `VerifyNewEmailAsync`'teki `catch (DbUpdateException)` `ExecuteUpdateAsync` çevresinde; EF toplu güncellemede sağlayıcı hatasını sarmıyorsa yarışta `Taken` yerine istisna döner (işlem yine geri alınır). `EmailChange_Resend_OverTheLimit_...` testi önceki geçerli bağlantıyı değil ilk (yerine geçilmiş) bağlantının `Missing` olmadığını sınıyor | D-578 → T519 |

#### Yapım Raporu Karşılaştırması
- Uyum: kriterlerde tam uyumlu; öz denetim tablosu var, her madde kanıtlı.
- Uyuşmazlık 1: rapor ve D-556, Google ile doğan hesapta misafir siparişlerinin bağlanmasını anmıyor (K1).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (Google hattı mevcut `ConditionalLine`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T343-google-giris`
- **PR:** #468
- **CI:** (yönetici izler)

## Dış Varsayımlar

Dış varsayım: yok. Google'a gerçek çağrı yok; kimlik `GoogleIdentity` olarak verilir (bağlayıcı T409).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Yönetici e-posta değişikliği | D-553 | T523'ün işi |
| 2 | E-posta değişince bekleyen şifre sıfırlama bağlantıları | D-554 | `02` susuyor; sahip kararı |
| 3 | Kendi adresine istek için ayrı hata kodu | D-555 | `ErrorCode`/`07` değişikliği (⚠: `EmailTakenByOtherAccount` kullanıldı) |
| 4 | Doğan hesapta kaydet-sonra-oturum sırası ve yeniden deneme | D-556 | sayfa task'ı (T515) |
| 5 | `07` UC-51/53 sözleşme metni ekleri | D-557 | ilk sayfa task'ı |
| 6 | Google ile doğan hesapta misafir siparişlerinin bağlanması (doğrulama K1) | D-577 | kayıttan sonra; sayfa task'ı (T515) |
| 7 | `ExecuteUpdate` çevresindeki `DbUpdateException` yakalaması ve L-9 aşım testinin zayıf iddiası (doğrulama K2) | D-578 | T519 aynı dosyaya dokunur |

## Notlar

- **Working tree (Adım -1):** temiz
- **Kilit sırası:** `VerifyNewEmailAsync` `LockKind.Account` almaz (Account ailesi tek başına alınır, `05 §7.3.5`; konuk siparişlerin satır kilitleri aynı işlemde gelir); adres güncellemesi satır kilidini tutar, sonra bağlantı tüketilir. Diğer komutlar `AccountLocks` kullanır.
- **Boş adres:** Google'dan boş adres gelirse giriş kaydı yazılmaz (kayıtta adres zorunlu).
- **Docker:** `t343` kapsayıcısı iş sonunda `-v` ile silindi.
