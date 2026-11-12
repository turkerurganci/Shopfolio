# T522 — Yönetici hesapları ve işlem izi (E-49, E-52)

**Faz:** F5 | **Durum:** Tamamlandı (PR açıldı) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /panel/yonetici-hesaplari` (E-49): yöneticiler Türkçe alfabetik sırada, yalnız geçerli davetler en yeni önce; davet satırı gönderen adı, son gün ve "e-posta ulaşmadı" işaretini taşır. İşleyiciler `SendAdminInvitation`, `RevokeAdminInvitation`, `RemoveAdmin` (onay hâli onaysızda, yazma onaylıda).
- `GET /panel/islem-izi` (E-52): en yeni önce, sayfada 25; işlem anındaki ad ve e-posta; tarih ve yönetici süzgeci birlikte; ters/ileri aralıkta alan mesajı, süzgeç uygulanmaz, durum 200; boş sonuçta süzgeç kaldırma yolu.
- Okuma servisi `IAdminAccountList` (`AdminAccountListService`); davet geçerliliğinin "canlı bağlantı" koşulu tek ifadede (`AdminAccounts.LiveLink`), kaldırma onayının sayımı ile okuma aynı ifadeyi kullanır, `LinkRecords.StateOf` Valid durumu ile aynı kural (D-594 kapandı; ikinci kopya yok).
- "E-posta ulaşmadı" işareti tek okumada: `DispatchMarks.InvitationsWithMarkAsync` (liste büyüse de okuma sayısı artmaz).
- `AuditTexts` (alan ve işlem adlarının Türkçe metni), onay metni (`ConfirmModels`), `admin.css` `admin-accounts__*` stili.
- Bu tur: `LiveLink` birleştirmesi, yönetici olmayan işlemin 303 testi, `InvitationsWithMarkAsync` testi, model bağlama mimari testi düzeltmesi (`RemoveAdminInput.Confirmed` `[SpecAssignedName]`; `AuditLogQuery.StartDate/EndDate` 06 adı olduğundan öznitelik kalktı), rapor ve öz denetim.

## Etkilenen Modüller / Dosyalar

- `Modules/Administration` (`AdminAccountListService`, `AdminAccounts.LiveLink`, sözleşmeler `AdminAccountListContracts`), `Modules/Notifications` (`DispatchMarks.InvitationsWithMarkAsync`, `INotificationsModule.FindAdminInvitationsWithUndeliveredMarkAsync`)
- `Pages/Panel/AdminAccounts.cshtml(.cs)`, `AuditTrail.cshtml(.cs)`, `Pages/Components/AuditTexts.cs`, `ConfirmModels.cs`, `wwwroot/css/admin.css`
- Testler: `tests/Shopfolio.IntegrationTests/Web/AdminAccountsPageTests.cs`, `AuditTrailPageTests.cs`, `Model/AccountEmailQueueTests.cs`; `tests/Shopfolio.UnitTests/Pages/AdminAccountsTextsTests.cs`; sahte bildirim modülleri (`UnusedNotifications`, `AccountEmailOnlyNotifications`, `ContactFormSendingTests`, `ContactRequestStatusTests`) yeni yöntemi taşır
- Kayıt: `Docs/DEFERRED_BACKLOG.d/T522.md` (D-1358, D-1359), `Docs/DEFERRED_BACKLOG.md` (D-594 ✅), `Docs/STATUS_CHANGELOG.d/T522.md`, `Docs/IMPLEMENTATION_STATUS.md` T522 satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Türkçe alfabetik sıra; kendi satırında kaldırma yok; tek yöneticide hiçbirinde yok | ✓ | `AdminAccountsPageTests.TheList_IsInTurkishAlphabeticalOrder_SaysSizOnTheOwnRow_…_KK1`, `WithOneAdministrator_NoRowHasARemovalButton_…_KK1_KK2` |
| 2 | Yalnız geçerli davet, en yeni önce; gönderen, son gün, "e-posta ulaşmadı" | ✓ | `TheListHidesInvitationsThatAreNotValid_…_KK2`, `OnlyTheInvitationWhoseEmailEndedUndelivered_CarriesTheMark_KK2`, `TheListService_ReturnsAllAdministratorsAndOnlyTheValidInvitations`, `AccountEmailQueueTests.TheListReadingOfTheMark_…_T522` |
| 3 | `SendAdminInvitation`: davet, Z-3 bağlantı, e-posta, iz, bütün yöneticilere F-6; onay istemez | ✓ | `SendingAnInvitation_WritesTheRowTheLinkTheEmailTheTrailAndTheNotice_…_KK2_KK3` (303, onay yok) |
| 4 | Aynı adresin önceki daveti geçersiz; L-9'a girmez; 422 biçim ve yönetici adresi | ✓ | `ANewInvitationToTheSameAddress_EndsTheEarlierOne_…_KK4` (ikinci gönderim 429 değil 303), `AnAdministratorsAddress_AndABadAddress_Are422WithTheFieldMessage_…_KK4`, `AMarkedAddressThatHasAValidInvitation_…_KK4` |
| 5 | `RevokeAdminInvitation`: bağlantı geçersiz, iz, bildirim yok; geçersizde 303 yazmaz; yoksa 404 | ✓ | `Revoking_EndsTheLink_WritesTheTrail_RedirectsWithoutACode_AndASecondRevokeWritesNothing_Or404ForAMissingRow_KK5` |
| 6 | `RemoveAdmin` onaysızda 200 ve yazmaz; onaylıda 303; 422 kendi satırı ve son yönetici; 404 | ✓ | `RemovingWithoutTheMark_IsTheConfirmationState_AndWritesNothing_…_KK6`, `TheConfirmationCountsTheTargetsUnusedInvitations_KK6`, `RemovingTheOwnRow_AndTheLastAdministrator_Are422_…_KK6`; Unit `TheRemovalConfirmation_NamesTheAccount_…_KK6` |
| 7 | Kilit altında önce işlemi yapanın yöneticiliği, sonra son yönetici; yönetici değilse yazılmaz, 303 girişe | ✓ | `TwoAdministratorsRemovingEachOtherAtOnce_LeaveExactlyOne_KK7`, `WhenTheActingSessionIsNoLongerAnAdministrator_EveryHandlerWritesNothing_AndRedirectsToThePanelSignIn_KK7` (üç işleyici), modül düzeyi `MembershipTests.AdminAccounts` (`Send_AsSomeoneWhoIsNoAdministrator_…`, `Send_ByAnAdministratorWhoseRowIsGone_…`) |
| 8 | Kaldırma tek işlemde hesap, oturum, bağlantı siler; daveti geçersiz kılar; iz, `Admin` imha kaydı, F-6 | ✓ | `RemovingWithTheMark_DeletesTheAccountItsSessionsAndLinks_EndsItsInvitations_AndWritesTheTrailTheDisposalRecordAndTheNotice_KK8` |
| 9 | `/panel/islem-izi` en yeni önce, 25; işlem anındaki ad ve e-posta, müşteri kişisel verisi yok; kaldırılmış yönetici seçilir | ✓ | `AuditTrailPageTests.TheRowsAreNewestFirst_TwentyFiveAPage_…_KK9`, `ARowCarriesTheNameAndTheAddressOfTheMoment…_AndNoCustomerData_KK9`, `ARemovedAdministratorIsSelectable_WithTheSuffix_…_KK9`; Unit `ATrailRow_ShowsTheNameAndAddressOfThatMoment_…_KK9`, `TheAdministratorOption_CarriesTheSuffixOnlyForARemovedAccount_KK9` |
| 10 | Tarih ve yönetici süzgeci birlikte; ileri/ters aralıkta mesaj, süzgeç yok, 200; boş sonuçta kaldırma yolu | ✓ | `TheDateAndTheAdministratorFiltersApplyTogether_…_KK10`, `ARangeThatDoesNotHold_Is200WithTheFieldMessage_AndAppliesNoFilter_KK10`, `WithNoRowsTheEmptyStateGoesHome_…_KK10` |

### Öz denetim (bitiş kapısı)

Önceki işçilerin yaptıkları dahil kriter 1–10 ve KK1–KK10 bütünüyle diff'te denetlendi. `sik-fail-nedenleri.md`:

| Sık FAIL türü | Sonuç | Kanıt |
|---|---|---|
| Bekçi/tarama istisnası geniş | ✓ | Yeni istisna eklenmedi; yalnız `[SpecAssignedName]` `Confirmed` için (06'da karşılığı yok), `StartDate/EndDate`'ten kalktı (06 adı) — `ModelBindingRuleTests` yeşil |
| Kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama kuralı yazılmadı; mevcut tarama testleri Unit TAM'da yeşil |
| 06 "Varsayılan" / FK dizini | ✓ | Yeni varlık, sütun, göç yok |
| Kural değişince aynı sayıyı anan yerler | ✓ | Yeni `INotificationsModule` yöntemi dört sahte modüle eklendi (derleme `-warnaserror` temiz); D-594 metni `LiveLink` ile güncellendi |
| Belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09` ve öteki belgeler değişmedi |
| Modül kurucusuna yeni bağımlılık / elle kuran düzenek | ✓ | `AdminAccountListService` DI ile kurulur |
| Güvenlik taraması | ✓ | Üç POST işleyicisi çerçevenin sahte istek korumasından geçer (`ForgeryTokenHandler` ile testlerde); yönetici oturumu şart; kişisel veri iz sayfasında yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit TAM | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 7408/7408 (iki koşuda: ilkinde `LinkKeySourceScanTests` yük altında regex zaman aşımı verdi, tek başına 15/15; ikincisinde `ModelBindingRuleTests` kırmızıydı, düzeltildi 27/27) |
| Integration (T522 sınıfları) | ✓ | `--filter-class "*AdminAccountsPageTests"`, `"*AuditTrailPageTests"`, `"*AccountEmailQueueTests"` | 16/16, 5/5 (6 durum), 15/15 |
| Integration (kısmi; yük altında TAM koşulmadı) | ✓ | `--filter-namespace "Shopfolio.IntegrationTests.Web"` (main birleştirmesi sonrası) + `--filter-class "*AccountEmailQueueTests"`, `"*LivenessEndpointTests"` | Web 960/961 (yük altında `LivenessEndpointTests.ConnectionOpenIsSlow_Returns503WithinTheLimit` zaman aşımı; sınıf tek başına 16/16), AccountEmailQueueTests 15/15. Kalan ad alanları (Model, Ordering, CorporateContent vb.) **TAM doğrulayıcıda koşulacak** |
| `dotnet format` | ✓ (Web, Integration, Unit) | `dotnet format <proje> --no-restore` | değişiklik yok |

## Known Limitations / Follow-up

- D-1358: satırdaki "Yeni davet gönder" betiksiz yolda doğrudan gönderir (uyarı `title`'da); onay istemeyen işlem olduğundan kural karşılanır.
- D-1359: işlem izi alan/işlem adları katalog yerine `AuditTexts`'te.
