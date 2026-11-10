# T379 — Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-08

---

## Yapılan İşler

- `IAdministrationModule`'e dokuz yöntem: `SendInvitationAsync`, `WithdrawInvitationAsync`, `ReadInvitationLinkAsync`, `AcceptInvitationAsync`, `RemoveAdminAsync`, `ChangeNameAsync`, `RequestEmailChangeAsync`, `ResendEmailChangeLinkAsync`, `VerifyNewEmailAsync`.
- `AdminAccounts` (davet, geri çekme, kabul, kaldırma, ad) ve `AdminEmailChanges` (müşterinin rejimiyle e-posta değişikliği); durum eşlemesi tek yerde (`AdminAccounts.StatusOf`).
- Membership'in bekleyen e-posta tablosuna Administration'ın erişimi `IAdminPendingEmailChanges` (Contracts) üzerinden; modül sınırı korunur.
- `AuditTrail.WriteForAsync`: oturumsuz davetli ve e-postası doğrulanan yönetici için aktörü modülün kendisi söyler.
- `CreateAdminAsync` başında `AdminSet` kilidi (komut ile davet kabulü aynı adreste aynı anda tek hesap açar).
- Sonuç enum'ları `Contracts/Results` ad alanında (kayıtlı değer değildir; EnumLock/Tablo testleri); bağlantı tüketen iki yöntem `LinkKeySourceScan` listesine girdi.
- `AddAdministrationModule` `AddHttpContextAccessor` kaydeder (komut ana makinesinde istek yok, aktör boş).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Administration/` (AdminAccounts, AdminEmailChanges, AdministrationModule, AuditTrail, Contracts/IAdministrationModule, Contracts/Results/*), `Modules/Membership/` (AdminPendingEmailChanges, Contracts).
- Testler: `tests/Shopfolio.IntegrationTests/Model/MembershipTests.AdminAccounts.cs`, `SwitchableAuditActor.cs`, `AccountEmailOnlyNotifications.cs`, `MembershipTests.cs` (kayıtlar); `tests/Shopfolio.UnitTests/Architecture/AdminAccountRulesScanTests.cs`, `LinkKeySourceScan.cs`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İlk yönetici kurulumda doğar; sonrakiler davetle, kayıt yolu yok | ✓ | `CreateAdminCommandTests` (T303 komutu, ilk yönetici); `Invitation_IsSentWith…KK1_KK2`; `AdminAccountRulesScanTests` (kayıt sayfası/rota yok, `CreateAdminAsync` yalnız komutlarda) |
| 2 | Bağlantı tek kullanımlık ve sürelidir; ad yazılır, şifre kurulur, hesap o anda doğar | ✓ | `Invitation_IsSentWith…KK1_KK2` (ad boş, zayıf şifre bağlantıyı tüketmez; ikinci kabul `Used`; giriş yapılır); `Invitation_RunsOutAfterItsLinkLife…` (72 saat, P-14) |
| 3 | Başkasını kaldırır; son yönetici ve kendini kaldırma engellenir, sebep söylenir; onay ister | ✓ | `Removal_RefusesTheActingAdministratorThemselves…KK3` (`LastAdminRemoval`); `Removal_AsksForTheConfirmation…KK4` (onaysız çağrı yazmaz) |
| 4 | Oturumlar anında biter, davetler geçersizleşir, onay sayıyla söyler, iz satırları ad ve e-postayla kalır, F-6 hepsine | ✓ | `Removal_AsksForTheConfirmation…KK4` (2 oturum, 2 davet, iz satırları, F-6 kaldırılan dahil 3 adres) |
| 5 | Yöneticiler aynı yetkide; rol ve yetki matrisi yok | ✓ | `TheAccountAndTheInvitation_HaveNoRoleOrPermissionField_KK5`; `AdminAccountRulesScanTests` (rol adı taraması) |
| 6 | Müşterinin rejimi ve limitleri; e-posta değişikliği ize; firma iletişim e-postası değişmez; ad yeniden doğrulamasız | ✓ | `TheEmail_ChangesAfterTheReauthentication…KK6` (yeniden doğrulama, `companies.email` aynı, iz satırı eski/yeni, geri alma açıkken kapalı); `TheEmailRequestsThatReplaceAPendingOne_AreLimitedByL9…`; `TheName_ChangesWithoutReauthentication…` |
| 7 | Sepet/sipariş yok; iki hesap türü ayrı; e-posta tür içinde tekil, harf ayrımsız | ✓ | `AdministratorAccounts_HaveNoCartOrOrderRelation_KK7`; `Send_RefusesAnAdministratorsAddressInAnyLetterCase…KK7_KK8`; `ACustomerAddress_DoesNotSignInToThePanel…KK7` |
| 8 | Geri çekme; aynı adrese yeni davet öncekini geçersiz kılar; yöneticisi olan adrese gönderilemez; kabulde çakışma (K-1085); yalnız gönderim F-6 | ✓ | `Withdrawal_EndsTheLinkAtOnce…KK8`; `ANewInvitationToTheSameAddress_MakesTheEarlierOneInvalid…KK8`; `Acceptance_WhenTheAddressBecameAnAdministratorsMeanwhile…K1085_KK8`; `TheInvitationsButton_AndTheManagementCommand…K1085` |
| 9 | Erişim kaybında ürün içinde kurtarma akışı yok; kurtarma yönetici açma komutu | ✓ | `AdminAccountRulesScanTests` (kurtarma sayfası/rota yok, `CreateAdminAsync` komut dışında çağrılmaz); `CreateAdminCommandTests` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yazdıkları dahil bütün maddeler diff'te denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 kayıt yolu yok, davetle açılış | ✓ | `AdminAccountRulesScanTests` + kendi ihlal örnekleri (`Scan_FindsEachViolation_AndOnlyThose`); `Invitation_IsSentWith…` |
| KK2 bağlantı tek kullanımlık, ad zorunlu | ✓ | `Invitation_IsSentWith…KK1_KK2` |
| KK3 son yönetici ve kendini kaldırma | ✓ | `Removal_RefusesTheActingAdministratorThemselves…`, `Send_ByAnAdministratorWhoseRowIsGone…` |
| KK4 oturum ve davet düşer, F-6 gider | ✓ | `Removal_AsksForTheConfirmation…KK4` |
| KK5 rol alanı yok | ✓ | `TheAccountAndTheInvitation_HaveNoRoleOrPermissionField_KK5`, tarama |
| KK6 limitler ve yeniden doğrulama aynı | ✓ | `TheEmail_ChangesAfterTheReauthentication…`, `…AreLimitedByL9…` |
| KK7 iki hesap türü ayrı kapılardan | ✓ | `ACustomerAddress_DoesNotSignInToThePanel…KK7`, `Send_RefusesAnAdministratorsAddress…` |
| KK8 geri çekme, yeni davet, kabulde çakışma, F-6 | ✓ | `Withdrawal_…`, `ANewInvitationToTheSameAddress_…`, `Acceptance_When…K1085` |
| KK9 kurtarma ucu yok | ✓ | `AdminAccountRulesScanTests` |
| `09 §25` tek işlev | ✓ | durum eşlemesi tek yerde (`AdminAccounts.StatusOf`, e-posta tarafı onu çağırır); e-posta kuralı `InvalidEmailAttribute`, şifre kuralı `PasswordPolicy` (kendi denetim yazılmadı) |
| `09 §25` kilit sırası | ✓ | hesap satırı kilidi gönderen/doğrulayan için; `AdminSet` yalnız kabul, kaldırma ve komutta, tek başına (`LockSequence` testlerde koştu); bağlantı satırı davet satırından önce |
| `09 §25` bekçi/tarama istisnası dar | ✓ | `LinkKeySourceScan` listesine yalnız iki tam dosya + tam yöntem adı; yeni tarama istisnası eklenmedi |
| `09 §25` modül kayıt bağımlılığı (elle kurulan düzenekler) | ✓ | `MembershipTests` kayıtları güncellendi; Integration TAM koşuldu |
| `09 §25` DB default / FK dizini | ✓ (yeni tablo yok) | şema değişmedi |
| Belge kuralını değiştiren 09 metni | ✓ (yok) | belge kuralı değişmedi; karar kaydı gerekmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` (Düzeltme 1, main birleşmesi sonrası) | 4928/4928 |
| Integration (Model) | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Model` | 1379/1379 |
| Integration (Web) | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Web` | 594/594 |
| Integration (Database) | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Database` | 36/36 |
| Integration (öteki) | ✓ | `--filter-not-namespace` Web, Model, Database | 840/840 |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore`, Web + UnitTests + IntegrationTests ayrı ayrı | üçünde de çıktı yok (Düzeltme 1; ilk raporda IntegrationTests koşulmamıştı) |

### Düzeltme 1 (doğrulama tur 1, B1)

`MembershipTests.AdminAccounts.cs` içindeki yedi `var X = await StartAdminWorldAsync(...)` açık demet türüyle (`(Guid Id, string Name, string Email)`) değiştirildi; yalnız bildirim türü, davranış aynı. Öz denetim: kabul kriterleri 1–9 ve KK1–KK9 için dal kodu değişmedi, kanıt tablosu yukarıda geçerli ve Integration TAM küme main birleşmesi sonrası yeşil. D-617 (üç yardımcının kopyası) açık bırakıldı: iki akış farklı hesap türü ve tablolarla çalışıyor, ortak yardımcı bu turda küçük ve güvenli bir değişiklik değil.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 2, 2026-11-08) — tur 1 ✗ FAIL |
| Bulgu sayısı | 0 (tur 2); tur 1'in B1'i kapandı |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (tur 1 K1 → D-617, tur 2 K1 → D-639) |

### Doğrulama Sonucu — tur 2 (validator)

**Head:** `5f0e456f` (kayıt commit'inden önce `origin/main` `4c049223` çakışmasız birleştirildi) · **PR:** #473 · **Verdict:** ✓ PASS

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = depo `scripts/git-hooks` (ana checkout'un mutlak yolu); ana dal CI son 3: 42500144509, 42500141831, 42493884474 hepsi `success`.
- **Dal CI:** 42504976109 (`5f0e456f`) ✓ — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E (advisory), CI Gate hepsi `pass`.
- **Kabul kriterleri (kapsam tam; kod + test okundu, testler koşuldu):** 1 ✓ (kendi kaydı yok — `AdminAccountRulesScanTests`; `CreateAdminAsync` yalnız komutlarda; davet tek yol) · 2 ✓ (P-14 72 saat; ad boş / zayıf şifre bağlantıyı tüketmez; ikinci kabul `Used`; hesap şifreyle girer, oturum açılmaz) · 3 ✓ (`AdminSet` kilidi altında aktör yeniden okunur; kendini ve son yöneticiyi `Refused`/`LastAdminRemoval`; onaysız çağrı yazmaz — iki yöneticili mağazada kendi dışı hedefte sayı ≥ 2 olduğundan iki engel tek kodla doğru söylenir) · 4 ✓ (oturumlar FK ile anında düşer; gönderdiği geçerli davetler bağlantısıyla geçersizleşir; onay ad, adres ve sayıyı verir; iz satırları `admin_name`/`admin_email` ile kalır; kaldırma satırı `InvalidatedInvitations` taşır; F-6 kaldırılan dahil herkese) · 5 ✓ (rol/yetki alanı yok; rol adı taraması) · 6 ✓ (yeniden doğrulamanın tek kullanımlık değeri, L-9 yerine geçen istekte, geri alma açıkken kapalı; iz satırı eski/yeni adres; `companies.email` aynı; ad yeniden doğrulamasız ve izsiz — `02 §10.3.1` (4) adı saymaz) · 7 ✓ (`Admin`'e sepet/sipariş FK'si yok; tür içinde harf ayrımsız tekil; müşteri adresine davet ayrı yönetici hesabı açar; panel girişi müşteri adresini almaz) · 8 ✓ (geri çekme bağlantıyı o an bitirir, bildirim yok; aynı adrese yeni davet öncekini — süresi dolmuş olanı da — geçersiz kılar; K-1085 okumada ve kabulde; yarış testi tek hesap; yalnız gönderim F-6) · 9 ✓ (kurtarma sayfası/rotası yok, tarama).
- **KK1–KK9:** ✓ (kriterlerle aynı kanıt). **`09 §25`:** işlem kodları ve `DisposalRecordType.Admin` önceden kayıtlı; `Details`'te müşteri verisi yok (yalnız yönetici adı/adresi, `09 §9.2` madde 4); `AdminSet` ve hesap kilidi `LockSequence`'e bildirilir; şema/göç yok; modül sınırı `IAdminPendingEmailChanges` (Membership `Contracts`) ile; yeni paket yok; anahtarlar `LinkKeys`'ten.
- **Task satırları (KP-62):** VAL-A301–A309, C068–C074, D035 uç yanıtı / tarayıcı kanıtı ister; uçlar ve sayfalar T520/T522/T523'te. `Durum` hücreleri `BEKLEMEDE` kaldı; `12_matris_denetle.py --kapsam 1-77` Hata 0.
- **Testler (yerel, taze `postgres:18`):** restore + `--locked-mode` 0; build `-warnaserror` 0 uyarı; format Web/UnitTests/IntegrationTests temiz; Unit 4928/4928 (main birleşmesinden sonra build temiz, Unit 4951/4951); betik testleri 4/4; Integration `MembershipTests` 114/114, `AuditTrailTests` + `AppCacheTests` + `CompanyIdentityTests` 60/60, `Web` ad alanı 592/594 — iki `EmailCaptureRegistrationTests` düşmesi ("HeartbeatRecord row is missing") sınıf tek başına 3/3, CI yeşil, T379'la bağı yok → K1/D-639.
- **Güvenlik:** sır yok; yetki: gönderen, geri çeken ve kaldıran aktör istekten ve veritabanından okunur, satırı yoksa yazılmaz; girdi: e-posta tek kural (`InvalidEmailAttribute`), şifre `PasswordPolicy`, ad boş reddi; yeni bağımlılık yok.
- **K1 → D-639** (yeni); D-617 açık, hedefi F3 gate.
- **Yapım raporu karşılaştırması:** 2 küçük uyuşmazlık — (1) Test Sonuçları "Integration (Web) 594/594": yerelde 592/594 (D-639; CI yeşil); (2) "Commit & PR" bölümü PR #473 açık olduğu hâlde "PR açıldığında doldurulur" diyor. Öz denetim tablosu var ve kanıtlı; öteki satırlar validator bulgularıyla uyumlu.

### Doğrulama Sonucu — tur 1 (validator)

**Head:** `beae4623` (origin/main `a71c6f02` ile güncel; birleştirme gerekmedi) · **PR:** #473 · **Verdict:** ✗ FAIL

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = depo `scripts/git-hooks` (ana checkout'un mutlak yolu); ana dal CI son 3: 42493884421, 42493884474, 42492032713 hepsi `success`.
- **Dal CI:** 42495939596 ✗ — 1. Lint `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` yedi `IDE0008 Use explicit type instead of 'var'` (`tests/Shopfolio.IntegrationTests/Model/MembershipTests.AdminAccounts.cs` satır 61, 267, 289, 350, 382, 398, 445); CI Gate ✗; Build, Unit, Integration, Contract, Migration, Paketleme `skipping` — CI'da hiç koşmadı. Önceki run 42495922687 (`ca7a3b7d`) `cancelled`. Dalın `success` run'ı yok.
- **Kabul kriterleri (modül düzeyi, kod + test okundu):** 1 ✓ (kayıt yolu yok — `AdminAccountRulesScanTests`; davet tek yol, `CreateAdminAsync` yalnız komutlarda) · 2 ✓ (P-14 72 saat, tek kullanım, ad boş/zayıf şifre bağlantıyı tüketmez) · 3 ✓ (kendini ve son yöneticiyi `AdminSet` kilidi altında reddeder; onaysız çağrı yazmaz) · 4 ✓ (oturumlar FK ile düşer, gönderdiği davetler bağlantısıyla geçersizleşir, onay sayıyı söyler, iz satırı `InvalidatedInvitations` taşır, F-6 kaldırılan dahil herkese — `02 §9.3.4`; imha kaydı `Admin` `06 §5.5.3`) · 5 ✓ (rol/yetki alanı yok, rol adı taraması) · 6 ✓ (yeniden doğrulama, L-9, geri alma açıkken kapalı; iz satırı eski/yeni; `companies.email` değişmez; ad yeniden doğrulamasız ve izsiz — `02 §10.3.1` (4) adı saymaz) · 7 ✓ (ilişki yok; tür içinde harf ayrımsız tekil; müşteri adresi davete engel değil; iki kapı ayrı) · 8 ✓ (geri çekme bağlantıyı o an bitirir, bildirim yok; aynı adrese yeni davet öncekini geçersiz kılar; K-1085 kabulde hesap açmaz, davet geçersiz; yalnız gönderim F-6) · 9 ✓ (kurtarma ucu yok, tarama).
- **KK1–KK9:** ✓ (kriterlerle aynı kanıt). `09 §25`: yeni ad ve işlem kodları önceden kayıtlı (`AuditOperation`, `DisposalRecordType.Admin`); şema değişmedi; kilit sırası `LockSequence` testlerde koştu; K1 dışında bulgu yok.
- **Task satırları (KP-62):** VAL-A301–A309, C068–C074, D035 uç yanıtı / tarayıcı / panel kanıtı ister; uçlar ve sayfalar sonraki task'larda (T520, T522, T523). `Durum` hücreleri `BEKLEMEDE` kaldı (T343/T380 emsali).
- **Testler (yerel):** build `-warnaserror` 0 uyarı; format: Web ve UnitTests temiz, IntegrationTests 7 × IDE0008 (B1); Unit 4881/4881; Integration `MembershipTests` 114/114, `AddressTests` + `EmailCaptureRegistrationTests` + `CookiePolicyTests` + `MembershipSessionTests` 27/27; betik testleri 4 dosya.
- **B1 — ORTA, S1 Sapma (biçim kuralı, `09 §3.1`):** yedi `var` bildirimi `.editorconfig`'in IDE0008 kuralını çiğner; Lint kırmızı olduğu için dal CI'ı yeşil değil ve sonraki adımlar CI'da hiç koşmadı. Düzeltme: `StartAdminWorldAsync` dönüşünün açık türü yazılır; `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` temiz; dal CI'ı yeşil (Integration dahil).
- **K1 → D-617:** `AdminEmailChanges`'in üç yardımcısı `Membership/EmailChanges.cs`'tekilerin kopyası (`09 §24` madde 4); davranış aynı, düzeltme turunda kapatılabilir.
- **Güvenlik:** sır yok; yetki: gönderen/kaldıran aktör istekten okunur, satırı yoksa yazılmaz; girdi: e-posta tek kural (`InvalidEmailAttribute`), şifre `PasswordPolicy`, ad boş reddi; yeni bağımlılık yok.
- **Yapım raporu karşılaştırması:** 1 uyuşmazlık — Test Sonuçları "Biçim ✓ `dotnet format --verify-no-changes` (değişen klasörler) çıktı yok" iddiası yanlış: IntegrationTests projesinde yedi IDE0008 var (CI ve yerel). Öz denetim tablosu var ve kanıtlı; öteki satırlar validator bulgularıyla uyumlu.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (tablolar T103/T380'de kuruldu)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T379-yonetici-hesaplari`
- **Commit / PR / CI:** PR açıldığında doldurulur (CI izlemeyi yönetici yapar)
- **PR:** #473
- **Post-merge run'lar:** squash `02feaca` (#473) — CI 42512134573 success, Publish 42512134557 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Ana dal CI yeşil | `gh run list --branch main`: son CI 42493884474 success | ✓ |
| 2 | Git bekçisi etkin | `git config core.hooksPath` = `scripts/git-hooks` | ✓ |
| 3 | Davet bağlantısı ömrü P-14 = 72 saat | `02 §` tablo satırı P-14; `ProductConstants.AdminInvitationLinkHours` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Geri alma bağlantısının düğmesi (UC-187) | D-593 | sayfa/komut T523'ün işi |
| 2 | Yönetici ve davet listesi okumaları | D-594 | panel listesi T522 |
| 3 | Davet kabulü oturum açmaz; çerez/yönlendirme sayfada | D-595 | T520 |
| 4 | D-464 (Z-3 diğer koşullar) T379 payı | D-464 (yerinde kapatıldı, T520 payı kalır) | T379 testle gösterdi |
