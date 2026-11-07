# T308 — Firma bildirimleri F-1…F-6

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

---

## Yapılan İşler

- `Notifications/Contracts/CompanyNotices.cs`: olay kayıtları — F-1…F-4 için `CompanyNotice` (`NewOrderNotice`, `NewContactRequestNotice`, `NewClaimNotice`, `CardRefundNotice`), F-5 ve F-6 için `AdminNotice` (`IbanChangedNotice`, `AdminInvitedNotice`, `AdminRemovedNotice`). Hiçbirinde müşteri adı, adresi, telefonu, e-postası ya da talep metni üyesi yoktur; `IbanChangedNotice` tam IBAN'ı almaz, `For(...)` yalnız son dört haneyi tutar.
- `INotificationsModule`: `QueueCompanyNoticeAsync` (F-1…F-4, firmanın iletişim e-postasına tek satır) ve `QueueAdminNoticeAsync` (F-5, F-6, verilen her yönetici adresine ayrı satır). İkisi de çağıranın açık işleminde yazar, işlem yoksa reddeder (`09 §9.1` m.3 kalıbı, `AuditTrail` gibi).
- `CompanyNoticeValues`: olayın değer listesi (`DispatchRecord.EventValues`; `LinkEventValues` ile aynı ad/değer biçimi) — iç anahtar, sipariş numarası, tutar, yöntem, yönetici adları, davet adresi, an; tek yazıcı ve tek, hoşgörülü okuyucu. Tür ve ilgili kayıt `06 §4.58` tablosundadır; kural bozan bildirim (F-3'te yanlış tür, F-4'te sistemin yapmadığı iade, kuruştan ince tutar, boş ad, dört hane dışı IBAN parçası) reddedilir.
- `CompanyNoticeTemplates`: tür ve değer listesinden `EmailContent` — konu başlığı, hitap "Merhaba,", olayın cümlesi, ana düğme; yalnız satırdaki değerlerden üretilir (tabloya ya da saate bakmaz), okunamayan değerde `null` (gönderen bunu alıcı hatası yazar). `EmailResend` hiç verilmez.
- `CompanyNoticeQueue`: satır yazıcı; F-1…F-4 alıcısı `ICompanyContactEmail` (Settings modülünün açık `GetLegalIdentityAsync`'i; iletişim e-postası yoksa satır yazılmaz, 0 döner), F-5 ve F-6'da aynı adres iki kez gelirse tek satır, adres listesi boşsa ya da boş adres varsa `ArgumentException`.
- `EmailLinks.Panel(screen, key?)`: `https://{alan adı}/panel/{ekran}[/{iç anahtar}]`, kök kurulum ayarındaki alan adı (istek başlığı değil), anahtar yüzde kodlu.
- DI kaydı `NotificationsModule.cs`; `CONTEXT.md` Infrastructure/Notifications satırı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Notifications/{CompanyNoticeValues,CompanyNoticeTemplates,CompanyNoticeQueue,NotificationsModule}.cs`, `Contracts/{CompanyNotices,INotificationsModule}.cs`
- `src/Shopfolio.Web/Infrastructure/EmailComposition/EmailLinks.cs`
- Testler: `tests/Shopfolio.UnitTests/Notifications/CompanyNoticeTemplateTests.cs`, `tests/Shopfolio.IntegrationTests/Model/CompanyNoticeQueueTests.cs`
- Belgeler: `CONTEXT.md`, `IMPLEMENTATION_STATUS.md`, `MEMORY.md`; T304, T305, T330, T380 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri dolduruldu.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | F-1…F-4 firma e-postasına; F-5, F-6 her yöneticinin kendi adresine ayrı satır | ✓ | `F1_IsOneQueuedRowToTheFirmEmail_…`, `F5_WritesOneSeparateRowForEveryAdministratorAddress_…`, `F6_RowsCarryTheInvitationOrTheRemovedAdministrator_…` |
| 2 | Konuda sipariş soneki yok; gövde kişisel veri taşımaz | ✓ | `TheSubject_IsBrandAndTitleOnly_WithNoOrderSuffix` (6 tür), `TheNoticeRecords_HoldNoMemberThatCouldCarryTheCustomersPersonalData`, F-2 değer listesi yalnız 3 ad (`F2_WritesTheTopicOnly`) |
| 3 | Ana düğme `/panel/…`; F-1…F-4'te iç anahtar değer olarak satırda; oturum yoksa girişten sonra ekrana; F-5, F-6'da geri alma yok | ✓ | `TheMainButton_GoesToThePanelScreen_OnTheDomainSetting` (6 tür; `ReturnPath.IsLocalPath` ve `Resolve`), `TheInnerKey_IsAValueOfTheRow_ForF1ToF4_AndNotForF5AndF6`, `F5AndF6_HaveNoRevertLink_AndNoSecondAddress`; giriş sonrası yönlendirme T207/T520'nin (`SessionAccessTests.Panel_WithoutASession_GoesToTheSignInPage_…`) |
| 4 | Yeniden gönderim cümlesi yok; F-1…F-6 yeniden denenir, ulaşmayan işaret düşürmez | ✓ | `…AndHasNoResendSentence…` (`Resend` null, metinde "yeniden gönderil" yok); `AFirmNoticeThatCannotBeSent_IsRetriedOnTheSameRow_AndNeverPutsTheMarkOfAnOrder`; `AFirmNoticeThatEndsUndelivered_PutsNoMarkOnTheOrderItsRelatedTo` (satırlarda `OrderId` boş — tablo kuralı). F-1…F-4 ana sayfa uyarısı T385'tedir |
| 5 | F-1 tarih-saat, numara, ödeme yöntemi, toplam | ✓ | `F1_HasTheMomentTheNumberThePaymentMethodAndTheTotal`, `F1_WritesTheMethodInItsOwnWords` |
| 6 | F-2 yalnız konu tipi | ✓ | `F2_WritesTheTopicOnly` (beş konu) |
| 7 | F-3 dört başlık | ✓ | `F3_HasFourSeparateHeadlines_…`, `F3_RefusesAKindItDoesNotKnow_…` |
| 8 | F-4 neden, tutar, işlem gerekmez | ✓ | `F4_WritesTheReasonAndTheAmount_AndSaysNothingIsToBeDone` (4 neden), `F4_ShowsTheAmountWithKurus_…` |
| 9 | F-5 yalnız son dört hane; yönetici adı ve haneler olayın değeri | ✓ | `F5_WritesTheLastFourDigitsOnly_…`, `F5_NeverWritesTheWholeIbanToTheRow_AndKeepsTheFourDigits`, `F5_Refuses…` |
| 10 | F-6 davet ve kaldırma ayrı konu ve gövde; adlar ve adres olayın değeri | ✓ | `F6_Invitation_AndRemoval_HaveTheirOwnSubjectAndBody`, `F6_RemovalBody_IsMadeFromTheRow_AfterTheRemovedAdministratorIsDeleted` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: F-5 her yöneticiye ayrı satır | ✓ | `CompanyNoticeQueue.QueueAdmins`; `F5_WritesOneSeparateRowForEveryAdministratorAddress_…` (3 adres → 3 satır, kimlikler ayrı, firma adresi yok); `TheSameAddressTwice_IsOneRow_AndNoAddressIsAnError` |
| KK2: konuda sipariş yok; gövdede kişisel veri yok | ✓ | `TheSubject_IsBrandAndTitleOnly_…`; `EmailContent.OrderNumber` hiçbir şablonda verilmez; kayıtların dize üyeleri izinli listeyle sınırlı (`TheNoticeRecords_HoldNoMember…`) |
| KK3: bağlantı panel adresine; F-1…F-4 satırında iç anahtar değer; F-5, F-6'da geri alma yok | ✓ | `TheMainButton_GoesToThePanelScreen_…`, `TheInnerKey_IsAValueOfTheRow_…`, `F5AndF6_HaveNoRevertLink_…` |
| KK4: gönderilemeyen F-1 ve F-5 yeniden denenir; ulaşmayanda işaret yok | ✓ | `AFirmNoticeThatCannotBeSent_IsRetriedOnTheSameRow_…` (F-1 ve F-5; dört denemede `Queued` → `Undelivered`, `RecipientSendLimit.TakesPart` false), `ARowThatIsRetriedAndSentLater_…`, Integration `AFirmNoticeThatEndsUndelivered_…` (`DispatchMarks.HasMarkAsync` false) |
| KK5: F-1 dört değer | ✓ | `F1_HasTheMomentTheNumberThePaymentMethodAndTheTotal` |
| KK6: F-2 gövdesinde mesaj yok | ✓ | `F2_WritesTheTopicOnly` (değer listesi `OccurredAt`, `InnerKey`, `Topic`; üç blok) |
| KK7: dört F-3 konusu ayrı | ✓ | `F3_HasFourSeparateHeadlines_…` (`Distinct().Count() == 4`) |
| KK8: F-4 tutar ve neden | ✓ | `F4_WritesTheReasonAndTheAmount_…` |
| KK9: F-5 satırında tam IBAN yok | ✓ | `F5_NeverWritesTheWholeIbanToTheRow_AndKeepsTheFourDigits` (Integration, gerçek satır), `F5_WritesTheLastFourDigitsOnly_…` |
| KK10: yönetici silinse de F-6 değerleri satırda | ✓ | `F6_RemovalBody_IsMadeFromTheRow_AfterTheRemovedAdministratorIsDeleted` (admins tablosu boşaltıldıktan sonra gövde satırdan) |
| `09 §25` — kaçış gösterim kanalında (ham işaretleme yok) | ✓ | `AValueWithMarkupAndLineBreaks_IsTextInTheHtml_AndOneLineInThePlainText`; `EmailRenderingSourceScan` tüm Unit koşusunda temiz (`EmailLinks.Panel` bu ad alanındadır) |
| `09 §25` — kayıtta kişisel veri yok; yeni olay tipinin ilgili kaydı tablo kuralına uygun | ✓ | `EveryFirmTypeF1ToF4_SatisfiesTheTablesRuleOfTypeAndRelatedRecord` (DB kısıtı), günlük satırı eklenmedi |
| Güvenlik taraması (mini) | ✓ | secret yok; yeni bağımlılık yok; panel adresi alan adı ayarından, `Host` başlığından değil; değerler HTML'de kaçışlı, düz metinde tek satır; tam IBAN hiçbir katmanda yok |

Sık FAIL nedenleri taraması: (1) bekçi/tarama istisnası eklenmedi; (2) kaynak taraması bu task'ta yeni kural eklemedi, mevcut `EmailRenderingSourceScan` ad alanını kapsar; (3) DB default'u ya da FK/dizin değişikliği yok (şema değişmedi); (4) sayı anan başka yer yok (README/başlık sayacı değişmedi, `CONTEXT.md` satırı eklendi); (5) belge kuralı (05/07/11) değişmedi, karar kaydı gerekmedi.

## Düzeltme 1 (CI Lint kırmızı)

- **Neden:** Lint adımı `CompanyNoticeTemplateTests.cs(1,1): IMPORTS` verdi (`using` sırası: `Security` / `SearchEngine`). Düzeltildi; `dotnet format --verify-no-changes` temiz.
- `origin/main` (T310 #423) birleştirildi (rebase değil); çakışma yalnız `MEMORY.md` (iki satır da kalır) ve `IMPLEMENTATION_STATUS.md` (yalnız en yeni "Son güncelleme"; T310 girişi `STATUS_CHANGELOG.md`'ye taşındı) içindeydi. `STATUS_CHANGELOG.md`'deki yinelenen T330 girişi teke indirildi. Backlog main ile aynı.
- Yerelde: restore `--locked-mode`, format temiz, derleme `-warnaserror` 0 uyarı, Consistency 209, tam Unit 3317, Integration `CompanyNoticeQueueTests` 11 geçti.
- Öz denetim yeniden yapıldı: 10 kriter ve KK1–KK10 ile `09 §25` satırlarının kanıt testleri Unit/Integration dosyalarında mevcut (ad eşleşmesi grep ile), sık FAIL nedenleri (bekçi istisnası, tarama biçimi, DB default/FK, sayı tutarlılığı, karar kaydı) bu task'a dokunmuyor.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 3286 geçti (T308 57) |
| Integration | ✓ (T308 sınıfı; yerelde, parçalı) | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-class "*CompanyNoticeQueueTests"` | 11 geçti |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes --include …` | temiz |
| Contract | — | — | bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07; head `77c623d` + `origin/main` birleştirmesi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-224) |

Validator kanıtları (bağımsız): 10 kriter ve KK1–KK10 kodda ve testte ✓ — metinler `08 §2.2.7` ile birebir, F-4 nedenleri B-8 listesiyle, satırlar `OrderId` boş (DB kısıtı), F-5'te gerçek satırda tam IBAN yok, F-6 gövdesi `admins` silindikten sonra satırdan. Build `-warnaserror` 0 uyarı; format üç proje temiz; Unit 3317 (Architecture 797 + öteki 2520; T308 57); betik testleri 4 dosya; Integration PostgreSQL 18 (`shopfolio-t308-val-pg`, 127.0.0.1:55730, silindi): CompanyNoticeQueue + MigrationBundle 13, Dispatch/Email/Job komşuları 84, Logging 30; restore `--locked-mode` 0; dal CI 42363737559 success. `12` satırları (VAL-B022, A123, A124, A326–A331) olayın tetiğini ve gönderimi ister → `BEKLEMEDE` kalır. K1: birim testinde `Assert.False(DispatchMarks.HasMark([]))` davranışı sınamaz (KK4 tümleşik testle karşılanır) → D-224. Yapım raporuyla uyum tam (Unit sayısı 3286 ↔ 3317 farkı birleştirmeden).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T308-firma-bildirimleri`
- **Commit:** `c06f88b` — T308: Firma bildirimleri F-1...F-6
- **PR:** #424
- **CI:** yapım işçisi izlemez; yardımcı yürütür
- **Post-merge run'lar:** squash `ad8c48d` (#424) — CI 42368786665 success, Publish 42368786653 success (T381 yapımında dolduruldu)

## Dış Varsayımlar

Dış varsayım yok: plan tier, kota, paket sürümü, dış servis ya da ortam durumuna dayanmaz; tetikleyici tarama (11 T308 metni) hiçbirini bulmadı. Docker yalnız yerel Integration için (`postgres:18`, CI'daki imaj).

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Olayı üreten task'lar (sipariş, iletişim formu, cayma/fesih/ayıp talebi, kart geri ödemesi, IBAN kaydı, yönetici daveti ve kaldırma) `QueueCompanyNoticeAsync` / `QueueAdminNoticeAsync`'i kendi işlemlerinde çağırır; `12` `VAL-A326…A330`, `VAL-A754` bu yüzden `BEKLEMEDE` | — (T308'in kendi kapsamı arayüz ve şablondur; çağıran task'ların kriterleri) | Üretici task'lar henüz yapılmadı; yeni kayıt açılmadı |

Hedefi T308 olan backlog kaydı yoktu (`grep -n T308 Docs/DEFERRED_BACKLOG.md` boş).

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana kopyanın hook dizini, worktree'ler paylaşır)
- **Ana dal CI startup (Adım 0):** son üç CI koşusu — 42360805990 (T305, success), 42358802050 (T330, success), 42356915419 (T380, success)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Kolay kararlar (kurala en uygun seçenekle verildi):**
  1. F-5 ve F-6 yönetici adreslerini çağıran verir (hesapların sahibi Administration; `IAdministrationModule` henüz boş) — Notifications başka modülün tablosunu okumaz (`09 §6`).
  2. F-1…F-4 alıcısı Settings'in açık `GetLegalIdentityAsync` e-postasıdır; iletişim e-postası girilmemişse satır yazılmaz (satış kapısı kapalıyken olay yoktur), sessizce 0 döner.
  3. Aynı adres iki kez verilirse F-5/F-6 tek satır yazar (adres benzersizdir; çift satır aynı kişiye iki e-posta olurdu).
  4. Hitap şablonda ilk paragraftır ("Merhaba,"); ortak çerçeve hitap eklemez (T305).
- **Docker:** `t308` konteyneri (55370) iş bitince `docker rm -f t308` ile silinir.
