# T309 — Hesap ve yönetim e-postaları

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- Yedi hesap e-postası kapalı listede (`Contracts/AccountEmails.cs`): doğrulama bağlantısı, şifre sıfırlama bağlantısı, yeni adres doğrulaması, adres değişikliği bildirimi, yönetici daveti, hesap silinme bildirimi ve Google teyit bağlantısı. Google teyidi kayıt taşımaz; `QueueGoogleLinkConfirmationAsync` ile kuyruğa girer (yeni enum açılmadı: `EnumLock` testi Contracts enum'larını sayar).
- `INotificationsModule.QueueAccountEmailAsync` (çağıranın işleminde tek satır) ve `QueueGoogleLinkConfirmationAsync` (true: kuyruğa girdi; false: geçerli bağlantı var, yenisi üretilmedi).
- `AccountEmailValues`: olayın değerleri satıra yazılır; bağlantı kaydı kimliği `LinkEventValues` ile aynı adla durur, ham anahtar satırda yoktur.
- `AccountEmailTemplates`: yedi şablon; ömür metinde değeriyle, `ProductConstants` P-12 (24 saat), P-13 (60 dakika), P-14 (72 saat), P-47 (7 gün) sabitlerinden; anahtar gövde üretilirken `keys.Derive` ile kayıt kimliğinden türetilir.
- `AccountEmailBodies`: gönderim anında bağlantı kaydı okunur; kullanılmış, değiştirilmiş, süresi dolmuş ya da sahibi silinmişse gövde üretilmez (`LinkNotValid`), türü ya da sahibi uymuyorsa `Unreadable`.
- `AccountEmailQueue`: satır yazıcı; `DispatchMarks.AccountEmailHasMarkAsync` yalnız yönetici davetinin ulaşmama işaretini düşürür.
- `EmailLinks.Account`: müşteri hesabı `/x/anahtar`, yönetici hesabı `/panel/x/anahtar`; metin aynı.
- Belgeler: durum, hafıza, backlog (D-233, D-234); T308, T312, T331 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/ProductConstants.cs`, `Infrastructure/EmailComposition/EmailLinks.cs`
- `src/Shopfolio.Web/Modules/Notifications/`: `Contracts/AccountEmails.cs`, `Contracts/INotificationsModule.cs`, `AccountEmailValues.cs`, `AccountEmailTemplates.cs`, `AccountEmailQueue.cs`, `AccountEmailBodies.cs`, `DispatchMarks.cs`, `NotificationsModule.cs`
- `tests/Shopfolio.UnitTests/Notifications/AccountEmailTemplateTests.cs`, `tests/Shopfolio.UnitTests/Consistency/LinkLifetimesConsistencyTests.cs`, `tests/Shopfolio.IntegrationTests/Model/AccountEmailQueueTests.cs`
- `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/DEFERRED_BACKLOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/` (T308, T312, T331, T309)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Yedi e-posta kapalı listede; konuda sipariş soneki, hitapta ad yok | ✓ | `TheSevenKinds_AreExactlyTheSevenTypesOfTheClosedList`, `TheSubject_IsBrandAndTitleOnly_AndTheGreetingCarriesNoName`, `TheGoogleConfirmation_HasItsOwnTitle_AndNoAccountName` |
| 2 | Anahtar gövdede kayıt kimliğinden türer; ham anahtar satırda yok | ✓ | `TheKey_IsDerivedFromTheRecordsId_ItIsInTheBodyAndNeverInTheRowsValues`; `EachKind_IsOneQueuedRowToItsRecipient_..._TheLinkRecordsIdButNeverTheKey` |
| 3 | Geçersizleşmiş bağlantının e-postası gönderilmez | ✓ | `ALinkUsedSupersededOrExpiredByTheTimeOfSending_GivesNoBody`, `ALinkWhoseOwnerIsGone_GivesNoBody` |
| 4 | Bağlantı işlem yapmaz; onay ekranı cümlesi yalnız dört gövdede | ✓ | `TheConfirmationScreenSentence_IsInTheFourLinkBodiesAndInNoOther`, `ThePasswordResetAndTheInvitation_SayNothingOfAConfirmationScreen` |
| 5 | Yeniden gönderim cümlesi yok; işaret yalnız davette | ✓ | `TheBody_HasNoResendSentence_AndNoResendNote`, `OnlyTheInvitationsUndeliveredRow_PutsTheMark_NoOtherKindAndNotTheFirmNoticeOfTheSameInvitation` |
| 6 | Müşteri ve yönetici hesabı aynı metin; yol `/…` ya da `/panel/…` | ✓ | `ACustomerAccountAndAnAdministratorAccount_ShareTheTextAndOnlyThePathDiffers`, `TheInvitationLink_IsAlwaysAPanelAddress_...` |
| 7 | Ömür değeriyle metinde, olay anından işler; yeniden denemede tam süre | ✓ | `TheLife_IsWrittenWithItsValue_FromTheProductConstants`, `TheBody_ReadsNoClock_...`, `AValidLink_GivesTheBody_AndARetriedEmailSaysTheWholeLifeUntilTheLinkEnds`; `LinkLifetimesConsistencyTests` (P-12/13/14/47 ↔ `02 §11`) |
| 8 | Yeni adres doğrulaması yeni, bildirim eski adrese; bildirim yeni adresi yazmaz, geri alma bağlantısı taşır | ✓ | `TheNewAddressVerification_GoesToTheNewAddress_AndTheChangeNotice_ToTheOldOne_NeitherWritesTheOther`, `TheChangeNotice_CarriesTheMomentAndTheRevertLink` |
| 9 | Davet davetlinin adresine, davet edenin adını yazar; silinme bildirimi tarih, silinenler, saklananlar, açık siparişe yol; bağlantı ve geri alma yok | ✓ | `TheInvitation_GoesToTheInvitedAddress_AndNamesTheInvitingAdministrator`, `TheDeletionNotice_HasTheDateTheDeletedTheKeptAndTheWayToAnOpenOrder_AndNoLinkAndNoRevert`, `TheDeletionNoticeRow_KeepsTheAddressAndTheDate_AfterTheCustomerIsDeleted` |
| 10 | Google teyidi yalnız müşteri hesabına; geçerli bağlantı varken yenisi yok | ✓ | `TheGoogleConfirmation_MakesTheLinkAndTheRow_AndNothingWhileAValidOneExists`, `TheGoogleConfirmation_MakesTheNextLink_WhenTheEarlierOneIsUsedSupersededOrExpired_AndEachCustomerHasItsOwn`, `TheGoogleConfirmation_RolledBackWithItsTransaction_...` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| k1 / KK1: yedi tür kapalı listede | ✓ | `AccountEmailTemplateTests.TheSevenKinds_AreExactlyTheSevenTypesOfTheClosedList` (`Contracts/AccountEmails.cs` altı kayıt + Google yedincisi) |
| k2 / KK2: satırda ham anahtar yok | ✓ | `AccountEmailQueueTests.EachKind_IsOneQueuedRowToItsRecipient_...` (değerlerde yalnız `LinkRecordId`); `TheKey_IsDerivedFromTheRecordsId_...` |
| k3 / KK3: geçersiz bağlantının e-postası gitmez | ✓ | `ALinkUsedSupersededOrExpiredByTheTimeOfSending_GivesNoBody` (üç neden), `ALinkWhoseOwnerIsGone_GivesNoBody`, `ALinkOfAnotherTypeOrAnotherOwner_IsUnreadable_...` |
| k4 / KK4: onay ekranı cümlesi dört gövdede; davet, şifre sıfırlama, silinmede yok | ✓ | `TheConfirmationScreenSentence_IsInTheFourLinkBodiesAndInNoOther`, `ThePasswordResetAndTheInvitation_SayNothingOfAConfirmationScreen` |
| k5 / KK5: yeniden gönderim cümlesi yok; davet dışı satıra işaret düşmez | ✓ | `TheBody_HasNoResendSentence_AndNoResendNote`; `OnlyTheInvitationsUndeliveredRow_PutsTheMark_...` (davet dışı altı tür ve aynı davetin firma bildirimi işaret düşürmez) |
| k6 / KK6: yönetici hesabında `/panel/` yolu | ✓ | `ACustomerAccountAndAnAdministratorAccount_ShareTheTextAndOnlyThePathDiffers` |
| k7 / KK7: ömür metinde, olay anından, yeniden denemede tam süre | ✓ | `TheLife_IsWrittenWithItsValue_FromTheProductConstants`, `TheBody_ReadsNoClock_...`, `AValidLink_GivesTheBody_AndARetriedEmailSaysTheWholeLifeUntilTheLinkEnds` |
| k8 / KK8: doğrulama yeni, bildirim eski adrese; bildirim yeni adresi yazmaz, geri alma bağlantısı; silinmede tarih var bağlantı yok | ✓ | `TheNewAddressVerification_...NeitherWritesTheOther`, `TheChangeNotice_CarriesTheMomentAndTheRevertLink`, `TheDeletionNotice_HasTheDate...`, `OnlyTheDeletionNoticeHasNoAddressInItsBody_TheOtherSixHaveExactlyOne` |
| k9 / KK9: davet satırına ulaşmama işareti düşer | ✓ | `OnlyTheInvitationsUndeliveredRow_PutsTheMark_...` (`DispatchMarks.AccountEmailHasMarkAsync`) |
| k10 / KK10: ikinci Google girişinde e-posta gitmez | ✓ | `TheGoogleConfirmation_MakesTheLinkAndTheRow_AndNothingWhileAValidOneExists` |
| `09 §25` ilgili satırlar | ✓ | Yeni kodda `DateTime.Now/UtcNow` yok (gövde saat okumaz: `TheBody_ReadsNoClock_...`); satırda kişisel değer yok (`TheAccountEmailRecords_HoldNoMemberThatCouldCarryAPersonalValue...`); mimari testler 830/830 |
| Güvenlik taraması | ✓ | Ham anahtar satırda ve kayıtta yok; `keys.Derive` yalnız `Modules/Notifications` (LinkKeySourceScan yeşil); gönderen adı HTML'de kaçışlı (`ASendersName_WithMarkupIsEscapedInTheHtml_...`); yeni bağımlılık yok |
| Sık FAIL (1): bekçi/tarama istisnası geniş | ✓ | Yeni tarama ya da istisna eklenmedi; mevcut `LinkKeySourceScan` kuralına uyuldu |
| Sık FAIL (2): kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama yok; `Derive` çağrısı yalnız `AccountEmailTemplates` içinde, Architecture yeşil |
| Sık FAIL (3): 06 "Varsayılan" / FK dizini | ✓ | Şema/göç yok; satır mevcut `Dispatch` tablosuna yazılır |
| Sık FAIL (4): aynı sayıyı anan öteki yerler | ✓ | Ömür sayıları yalnız `ProductConstants`'ta; `LinkLifetimesConsistencyTests` `02 §11.2` satırlarıyla eşler; kopya yok |
| Sık FAIL (5): belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09`, `02`, `08` metnine dokunulmadı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` (tümü; ayrıca `--filter-namespace *.Architecture*` ve `*.Notifications*`/`*.Consistency*`/`*.Infrastructure*`) | 3521/3521 (Architecture 830; Notifications + Consistency + Infrastructure 1518), 0 hata; `LinkLifetimesConsistencyTests` 4/4 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... --no-restore --filter-class "*.AccountEmailQueueTests"` | 14/14 (PostgreSQL 18, konteyner `shopfolio-t309-pg`, iş bitince silindi) |
| Derleme / biçim | ✓ | `dotnet build ... -warnaserror` (Unit, Integration); `dotnet format <proje> --verify-no-changes` (Web, Unit, Integration); `dotnet restore Shopfolio.slnx --locked-mode` | 0 uyarı, 0 hata; biçim farkı yok |
| Consistency (docs sonrası) | ✓ | `--filter-trait "Consistency=true"` (docs commit'inden sonra) | bkz. Notlar |
| Contract | — | — | Bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T309-hesap-yonetim-epostalari`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** açıldığında PR gövdesinde
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; ayrı `chore:` PR'ında veya sonraki task dalında)*

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Gönderilmedi, bağlantı geçersiz" kapanışının `DispatchState` karşılığı yok; `LinkNotValid` satırı kapatan durum ve davette işaretin bu yolda düşüşü | D-233 | `06 §4.58` yalnız Queued/Sent/Undelivered der; gönderim yeniden denemesi ve işaret T385'in işi |
| 2 | Google teyit bağlantısında kontrol-sonra-yaz yarışı (iki eş zamanlı giriş iki bağlantı üretebilir) | D-234 | Müşteri satırı kilidi Google girişini yazan T343'ün alanı; bu task Google girişini çağırmaz |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi: `git status --short` boş)
- **Ana dal CI startup (Adım 0):** `origin/main` son üç run (ad8c48d: CI 42368786665, Publish 42368786653; 1112ca6: CI 42366197638) success; `origin/main` dal tabanında zaten vardı (birleştirme "Already up to date")
- **Repo memory (bitiş kapısı 9):** güncellendi (T309 satırı)
- **⚠ maddeleri:** (1) `PasswordResetLink` "geri alma sonrası" cümlesi konumu: spec sırasız, paragraf 2'den sonra konuldu; (2) spec'e ek sapma yok; (3) D-233/D-234 yeni kayıt, D-235…D-237 kullanılmadı
- **Diğer:** önceki task'ların boş "Post-merge run'lar" ve Commit hücreleri (T308, T312, T331) dolduruldu
