# T523 — Yöneticinin kendi hesabı (E-50)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /panel/hesabim` (E-50): ad, e-posta ve şifre bölümleri; işlem formu yalnız oturum izni o işlem içinse ve değer özeti oturum kaydıyla aynıysa açık; tutmayan parametre yok sayılır ve izin düşer.
- İşleyiciler `ChangeName`, `ChangePassword`, `ChangeEmail`, `ResendEmailChangeLink`.
- Ortak e-posta değişikliği kuralı `Infrastructure/Security/EmailChangeRules` (D-617 kapandı); yönetici (`AdminEmailChanges`) ve müşteri (`EmailChanges`) akışı aynı yardımcıyı çağırır.
- "E-posta başka yöneticide" alan mesajı K-819 metni (`MyAccountTexts.AdminEmailTaken`, "Bu e-posta adresi başka bir yönetici hesabında kayıtlı."); K-820 metni yalnız Yönetici Hesapları sayfasındadır.
- Bu tur (devam 4): `Model.MembershipTests` kırmızılığı teşhisi (ortam kaynaklı, aşağıda), main (T515 #560) birleştirildi, kayıtlar, öz denetim.

## Etkilenen Modüller / Dosyalar

- `Modules/Administration` (`AdminEmailChanges`, `AdministrationModule`, `Contracts/IAdministrationModule`, `Contracts/Results/AdminAccountRecords`), `Modules/Membership` (`EmailChanges`, `AccountDeletions`, `MemberRecords`), `Infrastructure/Security` (`EmailChangeRules`, `ReauthenticationValues`, `SessionStore`)
- `Pages/Panel/MyAccount.cshtml(.cs)`, `MyAccountTexts.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Web/MyAccountPageTests.cs`; `tests/Shopfolio.UnitTests/Infrastructure/EmailChangeRulesTests.cs`
- Kayıt: `Docs/DEFERRED_BACKLOG.d/T523.md` (D-1389, D-1421, D-1422), `Docs/DEFERRED_BACKLOG.md` (D-617 ✅), `Docs/STATUS_CHANGELOG.d/T523.md`, `Docs/IMPLEMENTATION_STATUS.md` T523 satırı, `Docs/12_VALIDATION_PROTOCOL.md` (`Durum` satırları)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç bölüm; form yalnız izin ve özet tutarsa; tutmayan yok sayılır | ✓ | `MyAccountPageTests.TheForm_Opens_OnlyForTheOperationAndValueTheSessionHolds_T523_KK1`, `AValueThatDoesNotMatch_OrNoParameters_…_KK1` | Ad bölümünde `04` 9.21.3 cümlesi de aynı testte sınanır (`MyAccountTexts.NameNote`).
| 2 | Bekleyen değişiklik; geri alma açıkken kapalı, sebep yerinde | ✓ | `ThePendingChange_ShowsTheNewAddress_…_KK2`, `AnOpenRevertLink_ClosesTheAddressChange_…_KK2` |
| 3 | `ChangeName` 303; boş ad 422; eski kayıtlarda ad değer | ✓ | `ChangingTheName_Is303WithTheNotice_…_KK3` |
| 4 | `ChangePassword` tek tüketim, öteki oturumlar silinir | ✓ | `ChangingThePassword_UsesTheValueOnce_EndsTheOtherSessions_…_KK4_KK6` |
| 5 | Değer yok/tutmuyor 303 `PasswordChange`; zayıf şifre 422, tüketmez | ✓ | `APasswordChange_WithoutTheValue_Is303…_KK5` |
| 6 | Şifre değişikliği işlem izine yazılmaz | ✓ | aynı test (iz satırı yok), `…_KK4_KK6` |
| 7 | `ChangeEmail` bekleyen değişiklik, bağlantı, iz; eski adres çalışır; firma e-postası değişmez | ✓ | `ChangingTheAddress_WritesThePendingChange_…_KK7` (iz satırı `ChangeAdminEmail` + istenen adres, firma e-postası aynı) |
| 8 | Değer yok 303 `EmailChange`; 422 üç hâl (başka yönetici: K-819 metni); müşteri adresi engel değil; tüketilmez | ✓ | `AnAddressRequest_WithoutTheValue_Is303_…_KK8` (K-819 metni sınanır, K-820 metni yok), `AnAddressRequest_WhileARevertLinkIsOpen_…_KK8` |
| 9 | L-9 yalnız yerine geçen istekte; yeniden bağlantı: `SupersededAt`, ömür uzamaz; bekleyen yoksa 422 | ✓ | `AskingTheLinkAgain_IssuesANewLink_…_KK9`, `AskingTheLinkAgain_IsLimited429_…_KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 izin ve özet eşleşmesi; ad bölümü cümlesi | ✓ | `MyAccountPageTests` KK1 iki test (`NameNote` içerir, B3) |
| KK2 bekleyen değişiklik; kapalı işlem | ✓ | KK2 iki test |
| KK3 ad, eski kayıt adı | ✓ | KK3 testi |
| KK4 tek tüketim; oturumlar | ✓ | KK4_KK6 testi |
| KK5 iki ret; tüketilmez | ✓ | KK5 testi |
| KK6 iz satırı yok | ✓ | KK4_KK6 testi |
| KK7 bekleyen değişiklik, bağlantı, iz; firma iletişim e-postası aynı | ✓ | KK7 testi (iz satırı + firma e-postası eklendi, B1) |
| KK8 üç 422, yeniden doğrulama; alınmış adres K-819 metni | ✓ | KK8 iki test (B2) |
| KK9 L-9 koşulu; eski bağlantılar geçersiz | ✓ | KK9 iki test |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | yeni istisna eklenmedi; model bağlama için `[SpecAssignedName]` yalnız 06/07 adı olan alanlarda (e704b116) |
| Sık FAIL 2: kaynak taraması çağrı biçimi | ✓ | yeni tarama kuralı yok; mimari testler yeşil (Unit TAM) |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ | şema/migration yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | alan mesajı tek yerde (`MyAccountTexts.AdminEmailTaken`, K-819); `Model.MembershipTests` KK6 iz sayısı 1 → 2 (istek + doğrulama) güncellendi; `UC-188` yorumları `UC-181` (D-1422) |
| Sık FAIL 5: belge kuralını değiştiren 09 metni | ✓ | 09/05/07 değişmedi (diff Docs'ta yalnız kayıt dosyaları) |
| Hedefi bu task olan D-617 | ✓ | kapatıldı (`DEFERRED_BACKLOG.md` 425. satır) |
| Hedefi bu task olan D-553, D-593, D-1301, D-1375 (K1, D-1421) | ✓ | D-553: yönetici e-posta akışı bu PR'da (`RequestAsync`, `ResendAsync`, `VerifyNewEmailAsync`; L-9, Z-45). D-593: geri alma tüketimi `RestorePreviousEmailAsync` T521'de yazılı ve sınanmış. İkisinin satır kapanışı (backlog tablosu bu turda değişmez) faz kapısında. D-1301 (modül düzeyi test sınıfı, iki eşzamanlı tüketim) ve D-1375 (`changed != 1` dalı): T523 kabul kriterleri bu koda dokunmaz, düzeltme diff'i dar tutuldu; hedefleri F5 kapısına verilmeli (gerekçe bu satır) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ TAM 7472/7472 (düzeltme 1) | `dotnet test tests/Shopfolio.UnitTests` | `EmailChangeRulesTests` dahil |
| Integration | ✓ ilgili sınıflar; TAM: CI | `dotnet test … --filter-class` | Düzeltme 1: `Web.MyAccountPageTests` 14/14, `Model.MembershipTests` (iz sayacı değişen test yeniden koşuldu 1/1; toplu koşu 207 testte 205 yeşil, 2 hata aşağıda), `Web.AdminAccountsPageTests`, `Web.PanelHomeAndVerificationPagesTests` yeşil; TAM: CI |
| Format | ✓ | `dotnet format --verify-no-changes` | Web, Unit, Integration temiz |
| Contract | — | — | bu task'ta yok |

**Düzeltme 1 (2026-11-12):** B1 `AdminEmailChanges.RequestAsync` istekte `ChangeAdminEmail` iz satırı yazar (yönetici kararı; istenen adres değer olarak; doğrulama ve geri alma kendi satırını yazar, `Model.MembershipTests` KK6 sayacı 2; `MyAccountPageTests` `AuditLogs` tablosunu da temizler); KK7 testi iz satırını ve firma e-postasını sınar. B2 alan mesajı K-819 metni (`MyAccountTexts.AdminEmailTaken`), KK8 testi K-820 metnini dışlar. B3 ad bölümünde `04` 9.21.3 cümlesi (`MyAccountTexts.NameNote`), KK1 testi sınar. Karşılaştırma (3): "Etkilenen dosyalar" dal diff'iyle eşitlendi (`SessionAccessTests`/`AccessSamples` çıktı); K1 kalemleri öz denetim tablosunda; K2 (`UC-188` → `UC-181`) bu turda düzeltildi. Öz denetim: T523 kabul kriterleri 1–9 ve KK1–KK9 yeniden denetlendi, hepsi ✓. Yapım changelog'undaki "işlem izi" iddiası artık doğru.

`Model.MembershipTests` önceki koşuda 136/136 kırmızıydı; temiz konteynerde (`track_commit_timestamp=on`) tek başına 136/136 yeşil: ortam kaynaklı, regresyon değil.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL — dal ucu `199c1e51` (origin/main `73e36672` birleşik); dal CI run 43281683423 yeşil |
| Bulgu sayısı (tur 1) | 3 — **B1** (ORTA, S1, kriter 7/KK7): `ChangeEmail` işlem izine satır yazmaz; `07` 9.21.3 yan etkiler ve KK7 "iz" ister — geçici sınamada istekten sonra yöneticinin iz satırı 0; iz yalnız `VerifyNewEmailAsync`'te (doğrulamada) yazılır. Kod yazılır ya da `07` 9.21.3 / KK7 belge düzeltmesi kararla kaydedilir. **B2** (ORTA, S1, kriter 8/KK8): başka yöneticinin adresinde alan mesajı K-820'nin E-49 metni (`AdminAccountsModel.AdminEmailTakenText`, "Bu adres zaten bir yönetici hesabına ait."); `04` 7.2.24.2, 9.21.8, `07` 9.21.3 ve K-819 "Bu e-posta adresi başka bir yönetici hesabında kayıtlı." ister (8cb1f7a6 doğru metni sildi, gerekçe kaydı yok). **B3** (DÜŞÜK, S1, kriter 1/`04` 9.21.3): ad bölümü adın yönetici listesinde, işlem izinde ve bildirimlerde okunduğunu, müşteriye görünmediğini söyleyen cümleyi taşımıyor. |
| Düzeltme gerekli mi | Evet — B1, B2, B3 |
| Bloke-etmeyen not (K) sayısı (tur 1) | 2 — **K1** (forward D-1421): hedefi T523 olan D-553, D-593, D-1301, D-1375 (D-1395) anılmadı. **K2** (forward D-1422): yorumlarda şifre değiştirme `UC-188` (07'de `UC-181`). |
| `12` satırları (tur 1) | PASS: `VAL-A303`, C073, C074, D035 (iz doğrulamada; satırın ön koşulu "e-postası değiştirilmiş hesap"), A309 (geçici sınamada firma e-postası aynı). BEKLEMEDE kalır: `VAL-A494`, A560, A566 (`07 §6.3.2` kodları, öteki ekranlarla ortak), B090 (`[tarayıcı]`). Matris betiği hata 0. |
| Testler (tur 1) | Build `-warnaserror` 0/0; format üç proje temiz; Unit 7472/7472; betik 4/4; Integration parçalı 222/222 (`MyAccountPageTests`, `PanelHomeAndVerificationPagesTests`, `MembershipTests`, `MembershipAccountDeletionTests`, `MemberRecordPageTests`, `AdminAccountsPageTests`, `PanelAccessPagesTests`, `MembershipPasswordLockOrderTests`); TAM: CI. `restore --locked-mode` 0. |
| Yapım raporu karşılaştırması (tur 1) | Öz denetim tablosu var. Uyuşmazlık 3: (1) KK7 "iz; firma iletişim e-postası aynı" ✓ kanıtı `…_KK7` testi — test ne iz satırını ne firma e-postasını denetler, iz istekte yazılmaz (B1); changelog "`ChangeEmail` (… işlem izi …)" diyor. (2) KK8 ✓ — mesaj K-819 değil (B2). (3) "Etkilenen dosyalar"da `Web/SessionAccessTests.cs`, `Pages/AccessSamples.cs` anılıyor; dal diff'inde yoklar. Hedefi T523 olan D-553/D-593/D-1301/D-1375 anılmıyor (K1). |
| Doğrulama durumu (tur 2) | ✓ PASS — dal ucu `81198536`, origin/main `9cf8d1f7` ile birleşik ağaç `71a256a8` (çakışma yalnız STATUS tablosunda; iki satır kaldı); dal CI run 43300502173 yeşil (birleştirme öncesi uç) |
| Bulgu sayısı (tur 2) | 0. Tur 1 bulguları kapandı: **B1** `RequestAsync` istekte `ChangeAdminEmail` satırı yazar (aynı işlemde, `AdminAccounts` alanı, eski ve istenen adres değer olarak); `MyAccountPageTests` KK7 iz satırını ve firma e-postasını sınar, `Model.MembershipTests` iki satır sayar. **B2** alan mesajı `MyAccountTexts.AdminEmailTaken` = K-819 metni (`PRODUCT_DISCOVERY_STATUS` K-819 satırıyla birebir); KK8 K-820 metnini dışlar. **B3** ad bölümünde `NameNote` (`04` 9.21.3); KK1 sınar. |
| Bloke-etmeyen not (K) sayısı (tur 2) | 1 — **K1** D-1421 (D-553/D-593 kapanışı, D-1301/D-1375 hedefi) öz denetimde kanıtlı, tablo satırları kapanmadı → hedef F5 kapısına çevrildi. D-1422 ✅ (`UC-188` yok). |
| `12` satırları (tur 2) | Değişmedi: PASS `VAL-A303`, C073, C074, D035 (istek ve doğrulama satırları), A309; BEKLEMEDE `VAL-A494`, A560, A566, B090. Matris betiği hata 0. |
| Testler (tur 2) | Birleşik ağaçta: build `-warnaserror` 0/0; format üç proje temiz; Unit 7497/7497; betik 4/4; Integration parçalı 221/221 (`MyAccountPageTests`, `MembershipTests`, `PanelHomeAndVerificationPagesTests`, `AdminAccountsPageTests` 185; `MembershipAccountDeletionTests`, `MemberRecordPageTests`, `PanelAccessPagesTests`, `MembershipPasswordLockOrderTests` 36); TAM: CI. `restore --locked-mode` 0. |
| Yapım raporu karşılaştırması (tur 2) | Öz denetim tablosu var, maddeleri kanıtlı. Uyuşmazlık 1 (bloke etmez): "Etkilenen dosyalar" düzeltmenin dokunduğu `tests/Shopfolio.IntegrationTests/Model/MembershipTests.AdminAccounts.cs`'yi anmıyor. |

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
