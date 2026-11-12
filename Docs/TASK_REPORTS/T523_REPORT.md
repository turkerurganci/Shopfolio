# T523 — Yöneticinin kendi hesabı (E-50)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /panel/hesabim` (E-50): ad, e-posta ve şifre bölümleri; işlem formu yalnız oturum izni o işlem içinse ve değer özeti oturum kaydıyla aynıysa açık; tutmayan parametre yok sayılır ve izin düşer.
- İşleyiciler `ChangeName`, `ChangePassword`, `ChangeEmail`, `ResendEmailChangeLink`.
- Ortak e-posta değişikliği kuralı `Infrastructure/Security/EmailChangeRules` (D-617 kapandı); yönetici (`AdminEmailChanges`) ve müşteri (`EmailChanges`) akışı aynı yardımcıyı çağırır.
- "E-posta başka yöneticide" mesajı K-820 tek metne bağlandı (`AdminAccountsModel.AdminEmailTakenText`); `MyAccountTexts` kopyası silindi.
- Bu tur (devam 4): `Model.MembershipTests` kırmızılığı teşhisi (ortam kaynaklı, aşağıda), main (T515 #560) birleştirildi, kayıtlar, öz denetim.

## Etkilenen Modüller / Dosyalar

- `Modules/Administration` (`AdminEmailChanges`, `AdministrationModule`, `IAdministrationModule`, `AdminAccountRecords`), `Modules/Membership` (`EmailChanges`, `MembershipModule`, kayıt sözleşmeleri), `Infrastructure/Security` (`EmailChangeRules`, `ReauthenticationValues`, `SessionStore`)
- `Pages/Panel/MyAccount.cshtml(.cs)`, `MyAccountTexts.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Web/MyAccountPageTests.cs`, `Web/SessionAccessTests.cs`, `Pages/AccessSamples.cs`; `tests/Shopfolio.UnitTests/Infrastructure/EmailChangeRulesTests.cs`
- Kayıt: `Docs/DEFERRED_BACKLOG.d/T523.md` (D-1389), `Docs/DEFERRED_BACKLOG.md` (D-617 ✅), `Docs/STATUS_CHANGELOG.d/T523.md`, `Docs/IMPLEMENTATION_STATUS.md` T523 satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç bölüm; form yalnız izin ve özet tutarsa; tutmayan yok sayılır | ✓ | `MyAccountPageTests.TheForm_Opens_OnlyForTheOperationAndValueTheSessionHolds_T523_KK1`, `AValueThatDoesNotMatch_OrNoParameters_…_KK1` |
| 2 | Bekleyen değişiklik; geri alma açıkken kapalı, sebep yerinde | ✓ | `ThePendingChange_ShowsTheNewAddress_…_KK2`, `AnOpenRevertLink_ClosesTheAddressChange_…_KK2` |
| 3 | `ChangeName` 303; boş ad 422; eski kayıtlarda ad değer | ✓ | `ChangingTheName_Is303WithTheNotice_…_KK3` |
| 4 | `ChangePassword` tek tüketim, öteki oturumlar silinir | ✓ | `ChangingThePassword_UsesTheValueOnce_EndsTheOtherSessions_…_KK4_KK6` |
| 5 | Değer yok/tutmuyor 303 `PasswordChange`; zayıf şifre 422, tüketmez | ✓ | `APasswordChange_WithoutTheValue_Is303…_KK5` |
| 6 | Şifre değişikliği işlem izine yazılmaz | ✓ | aynı test (iz satırı yok), `…_KK4_KK6` |
| 7 | `ChangeEmail` bekleyen değişiklik, bağlantı, iz; eski adres çalışır; firma e-postası değişmez | ✓ | `ChangingTheAddress_WritesThePendingChange_…_KK7` |
| 8 | Değer yok 303 `EmailChange`; 422 üç hâl; müşteri adresi engel değil; tüketilmez | ✓ | `AnAddressRequest_WithoutTheValue_Is303_…_KK8`, `AnAddressRequest_WhileARevertLinkIsOpen_…_KK8` |
| 9 | L-9 yalnız yerine geçen istekte; yeniden bağlantı: `SupersededAt`, ömür uzamaz; bekleyen yoksa 422 | ✓ | `AskingTheLinkAgain_IssuesANewLink_…_KK9`, `AskingTheLinkAgain_IsLimited429_…_KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 izin ve özet eşleşmesi | ✓ | `MyAccountPageTests` KK1 iki test |
| KK2 bekleyen değişiklik; kapalı işlem | ✓ | KK2 iki test |
| KK3 ad, eski kayıt adı | ✓ | KK3 testi |
| KK4 tek tüketim; oturumlar | ✓ | KK4_KK6 testi |
| KK5 iki ret; tüketilmez | ✓ | KK5 testi |
| KK6 iz satırı yok | ✓ | KK4_KK6 testi |
| KK7 bekleyen değişiklik, bağlantı, iz; firma iletişim e-postası aynı | ✓ | KK7 testi |
| KK8 üç 422, yeniden doğrulama | ✓ | KK8 iki test |
| KK9 L-9 koşulu; eski bağlantılar geçersiz | ✓ | KK9 iki test |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | yeni istisna eklenmedi; model bağlama için `[SpecAssignedName]` yalnız 06/07 adı olan alanlarda (e704b116) |
| Sık FAIL 2: kaynak taraması çağrı biçimi | ✓ | yeni tarama kuralı yok; mimari testler yeşil (Unit TAM) |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ | şema/migration yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `MyAccountTexts` e-posta-alınmış kopyası silindi, K-820 tek metin; `SessionAccessTests`/`AccessSamples` güncel |
| Sık FAIL 5: belge kuralını değiştiren 09 metni | ✓ | 09/05/07 değişmedi (diff Docs'ta yalnız kayıt dosyaları) |
| Hedefi bu task olan D-617 | ✓ | kapatıldı (`DEFERRED_BACKLOG.md` 425. satır) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ TAM (T515 birleşmesinden önce 7435/7435) | `dotnet test tests/Shopfolio.UnitTests` | `EmailChangeRulesTests` dahil |
| Integration | ✓ ilgili sınıflar; TAM: CI | `dotnet test … --filter-class` | `Model.MembershipTests` 136/136, `Administration.PanelHomeTests` 25/25, `Web.MyAccountPageTests` 14/14, `Web.CustomerSignInPageTests` 33/33, `Web.SessionAccessTests` 27/27 (T515 birleşmesinden sonra) |
| Format | ✓ | `dotnet format --verify-no-changes` | Web, Unit, Integration temiz |
| Contract | — | — | bu task'ta yok |

`Model.MembershipTests` önceki koşuda 136/136 kırmızıydı; temiz konteynerde (`track_commit_timestamp=on`) tek başına 136/136 yeşil: ortam kaynaklı, regresyon değil.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T523-yonetici-kendi-hesabi`
- **PR:** bkz. dönüş
- **CI:** yoklanmadı (yönetici izler)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Bekleyen iznin düşmesi yalnız hesabım ve yeniden doğrulama sayfasında (⚠) | D-1389 | Güvenlik açığı yok (form yine özet ister); ara katman kararı F5 kapısına |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yönetici düzeninde yardımcıda
- **Repo memory (bitiş kapısı 9):** parça dosyaları yazıldı
- **Diğer:** Integration TAM: CI (sahip 2026-11-12, deneme).
