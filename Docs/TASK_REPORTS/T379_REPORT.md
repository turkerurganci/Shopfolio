# T379 — Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-08

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
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 4864/4864 (17 yeni tarama testi) |
| Integration (Model) | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Model` | 1372/1372 (T379: 15 yeni) |
| Integration (Model dışı) | ✓ | `--filter-not-namespace Shopfolio.IntegrationTests.Model` | 1446'dan 1444 geçti; `LivenessEndpointTests` iki testi yük altında `ObjectDisposedException` verdi, tek başına ve sınıf olarak (16/16) geçti, kodla ilgisiz zamanlama kararsızlığı |
| Biçim | ✓ | `dotnet format --verify-no-changes` (değişen klasörler) | çıktı yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (tablolar T103/T380'de kuruldu)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T379-yonetici-hesaplari`
- **Commit / PR / CI:** PR açıldığında doldurulur (CI izlemeyi yönetici yapar)

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
