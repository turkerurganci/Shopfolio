# T306 — Müşteri bildirimleri B-1…B-8

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (PR açıldı, doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Olay kayıtları** (`Modules/Notifications/Contracts/CustomerNotices.cs`): `CustomerNotice` altında B-1…B-8 için sekiz kayıt tipi; tür `DispatchType` (`B1`…`B8`).
- **Olay değerleri** (`CustomerNoticeValues.cs`): olayın değerlerini `DispatchRecord` satırına yazar ve okur; yalnız olayın tuttuğunu taşır (ad, adres, IBAN, sipariş numarası yok). Türün kuralını bozan olay `ArgumentException` ile reddedilir; eksik ya da bozuk liste bildirim sayılmaz, okuyucu atmaz.
- **Gövde şablonları** (`CustomerNoticeTemplates.cs`): sekiz tür için konu ve gövde; ortak iskelet T305 `EmailFrame`/`EmailContent`, kargo bloğu T310 `CarrierCatalog`, bağlantı kalıpları `EmailLinks`. Hitapta ad yok; ana düğme misafir ve hesabı silinmiş siparişte erişim anahtarlı adres, üyede hesap adresi.
- **Kuyruk yazıcısı** (`CustomerNoticeQueue.cs`): `DispatchRecord` satırını çağıranın işleminde yazar (işlem yoksa yazmaz); B-3 süre başına bir kez, süresi dolmuş pencere için satır yok. `INotificationsModule.QueueCustomerNoticeAsync` ve `NotificationsModule` kaydı.
- **Ortak kalıp** (`EmailContent.cs`, `EmailFrame.razor`): `EmailResend(OriginalAt, CarriesCurrentInfoNote)` ile B-2, B-3, B-5'te güncel bilgi cümlesi; gövdede 12 punto ve metin küçültme kapalı (`-webkit-text-size-adjust:100%`).
- **Belgeler:** bu rapor, status, memory; T305, T310 ve T331 raporlarının "Post-merge run'lar" satırı ve T331 status Commit hücresi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Notifications/`: `Contracts/CustomerNotices.cs`, `Contracts/INotificationsModule.cs`, `CustomerNoticeValues.cs`, `CustomerNoticeTemplates.cs`, `CustomerNoticeQueue.cs`, `NotificationsModule.cs`
- `src/Shopfolio.Web/Infrastructure/EmailComposition/`: `EmailContent.cs`, `EmailFrame.razor`
- `tests/Shopfolio.UnitTests/Notifications/CustomerNoticeTemplateTests.cs`
- `tests/Shopfolio.IntegrationTests/Model/CustomerNoticeQueueTests.cs`
- `Docs/TASK_REPORTS/` (T306, T305, T310, T331), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tür `DispatchType` (`B1`…`B8`), iskelet ortak, hitapta ad yok; B-2/B-3/B-5 yeniden gönderimde güncel bilgi cümlesi | ✓ | `TheEightNotices_AreTheEightClosedTypes_InOrder`, `EveryNotice_StartsWithMerhaba_WithoutAName_…`, `AResend_CarriesTheSentenceOfTheFirstEmail_AndOnlyB2B3B5CarryTheCurrentInfoSentence`, `ANewEventOfTheSameType_CarriesNeitherSentence` |
| 2 | Sipariş numarası konuda ve cümlede; ana düğme misafir/silinmiş hesapta anahtarlı, üyede hesap adresi | ✓ | `TheSubjects_AreTheTitlesOfTheSpec`, `ThePageButton_OfAGuestOrder_…`, `…OfAnOrderWhoseAccountIsDeleted_…`, `…OfAMembersOrder_…`, `AGuestOrderWithoutAnAccessKey_IsRefused_…` |
| 3 | B-1 sipariş oluşunca ve e-posta düzeltilince; iki yasal metin donan sürümle, ≥ 12 punto tam metin, ek yok | ✓ | `B1_CarriesTheSentenceTheButtonAndThenTheTwoTextsInFull_AsTwoSections`, `TheTwoTexts_AreSetAtTwelvePointsAtLeast_…`, `B1_HasNoAttachment_…`, `B1_WithoutOneOfTheTexts_IsRefused`; tümleşik `B1_IsOneQueuedRowToTheOrdersEmail_…`, `TheB1AfterACorrection_IsANewRowToTheNewAddress_…` |
| 4 | B-2 donan IBAN, sipariş numarası, son ödeme günü | ✓ | `B2_CarriesTheFrozenIbanInGroupsOfFour_…`, `B2_WritesTheIbanTheOrderFroze_NotAnotherOne`, `B2AndB3_WithoutAFrozenIban_AreRefused` |
| 5 | B-3 süre başına bir kez; kesinti dönüş koşulu T324'te | ✓ | `B3_CarriesTheIbanBlockTheEventsWindowEndAndTheClosingSentence`; tümleşik `B3_IsWrittenOnceForAWindow_…`, `…AgainForAWindowThatRestarted_…`, `…IsNotWritten_ForAWindowThatHasRunOut_…`, `…OfAnotherOrder_IsItsOwn_…` |
| 6 | B-4 kart onayı ve "ödendi"; dijital kalemde indirme cümlesi sipariş sayfasına işaret eder | ✓ | `B4_WithADigitalItem_PointsTheDownloadToTheOrderPage_AndWritesNoDownloadAddress` |
| 7 | B-5 kargo bloğu şirket türüne göre; takip bağlantısı yalnız listedeki şirkette ve `noreferrer` | ✓ | `B5_OfAListedCompany_…NoReferrer`, `…OfAnotherCompany_…NoLink`, `…OfARetiredCompany_…`, `…WithoutANumber_…`, `…OfOwnVehicleDelivery_…`, `…ATrackingNumberWithMarkup_…` |
| 8 | B-6 ve B-7 (üç söyleyiş) firma e-postası ve telefonu | ✓ | `B6_WritesTheSentence_AndTheFirmsEmailAndTelephoneInTheText`, `B6AndB7_WithoutTheFirmsEmailOrTelephone_AreRefused`, `B7_ByTheCustomer_…`, `B7_ByTheAdministrator_…`, `B7_BySystem_…` |
| 9 | B-7 üç söyleyişi ayırır; sistem iptalinde sebep; kalem iptali ayrı konu, kalem ve adet yalnız onda; yönetici sebebi aynen + on dört gün cümlesi | ✓ | `B7_ByTheAdministrator_WritesTheReasonAsItIs_AndTheFourteenDaySentence_EvenOnStock`, `…WithOther_…`, `B7_BySystem_WritesTheReason_AndNoFourteenDaySentence_…`, `B7_OfAnItem_HasItsOwnSubject_AndTheItemAndQuantityOnlyThere`, `B7_OfAnItemWithoutOptions_…` |
| 10 | B-8 neden, tutar, yöntem; IBAN yok; geç ödemede ayrı cümle | ✓ | `B8_WritesTheReasonTheAmountAndTheCardMethod_AndNoIban`, `B8_OfATransfer_SaysItWentToTheIbanTheCustomerGave_…`, `B8_OfALatePayment_HasItsOwnSentence…` |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçinin kodu dahil bütün maddeleri diff'te yeniden denetledi.

| Madde (KK) | Sonuç | Kanıt |
|---|---|---|
| KK1 sekiz tür kapalı listede; hitapta ad yok; B-2/B-3/B-5 yeniden gönderim | ✓ | `TheEightNotices_AreTheEightClosedTypes_InOrder`, `EveryNotice_StartsWithMerhaba…`, `AResend_…` |
| KK2 misafir ve silinmiş hesapta anahtarlı, üyede hesap adresi | ✓ | `ThePageButton_Of…` üç test |
| KK3 yasal metinler tam, ≥ 12 punto, ek yok | ✓ | `TheTwoTexts_AreSetAtTwelvePointsAtLeast_…`, `B1_HasNoAttachment_…` |
| KK4 B-2 donan IBAN | ✓ | `B2_WritesTheIbanTheOrderFroze_NotAnotherOne` |
| KK5 B-3 süre başına bir | ✓ | tümleşik `B3_IsWrittenOnceForAWindow_…` |
| KK6 dijital kalemde indirme cümlesi | ✓ | `B4_WithADigitalItem_…` |
| KK7 üç kargo biçimi ayrı | ✓ | `B5_OfAListedCompany_…`, `…OfAnotherCompany_…`, `…OfOwnVehicleDelivery_…` |
| KK8 B-6 ve üç B-7 söyleyişinde telefon, e-posta | ✓ | `B6_…`, `B7_ByTheCustomer_…`, `B7_ByTheAdministrator_…`, `B7_BySystem_…` |
| KK9 kalem iptalinde ayrı konu, kalem ve adet; sistem ve yönetici sebebi, on dört gün cümlesi | ✓ | `B7_OfAnItem_…`, `B7_ByTheAdministrator_…`, `B7_BySystem_…` |
| KK10 B-8 neden, tutar, yöntem, geç ödeme cümlesi; IBAN yok | ✓ | `B8_…` üç test |
| `09 §25` ilgili satırlar | ✓ | Architecture 809 test yeşil (modül sınırı, `EmailRenderingSourceScan`, isimlendirme); yeni 09 metni yok |

**`sik-fail-nedenleri` kontrolü:**

1. Tarama/bekçi istisnası geniş: yeni tarama ve istisna eklenmedi; mevcut taramalar yeni kodu görür ve yeşil.
2. Kaynak taraması çağrı biçimi kaçırma: yeni tarama yok; `EmailRenderingSourceScan` yeni dosyaları kapsar (Architecture yeşil).
3. "Varsayılan"/FK dizini: şema değişikliği yok (`DispatchRecord` T117'den).
4. Sayı anan öteki yerler: kayıtlı tür sayısı değişmedi (`B1`…`B8` T117'de); backlog "Aktif kalem" 104 (yeni kayıt yok).
5. Karar kaydı gerektiren yeni 09 metni: yok; belge kuralını değiştiren davranış yok.

**⚠ Devir notundan:**

- B-2, B-3 ve B-8 geç ödeme cümlesine sipariş numarası eklendi (`08 §2.2.6` madde 2 "olayın cümlesinde"); metin birebir değil.
- B-7 sebep "Sebep: {metin}." paragrafı olarak yazılır (spec yalnız "+ sebep" der).
- B-5 "Takip et" bağlantısı ikinci bir düğme bloğu (`EmailButton`, dış bağlantı) olarak, ana düğmeden önce gelir.
- B-1 yasal metinler `OrderNoticeContext`'te doldurulmuş gelir (`PlaceholderFiller` çağrısı gönderim task'ının işi); B-3'ün "son iş günü başı" zamanlaması T324/T353'te, burada yalnız süre başına bir + dolmuşsa yok.
- `INotificationsModule`/`NotificationsModule` T308 (PR #424) ile aynı dosyalara dokunuyor; ikisi merge edilirken küçük çakışma beklenir.
- Aynı sipariş için eşzamanlı iki B-3: kilidi çağıran tutar (`09 §9.1` m.5); kuyruk yazıcısı kendisi kilit almaz.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 3418/3418 | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | 0 hata; Architecture 809 ayrıca yeşil; `CustomerNoticeTemplateTests` 105 |
| Integration | ✓ 11/11 | `--filter-class "*CustomerNoticeQueueTests"` (Postgres 18, 127.0.0.1:55371) | 0 hata |
| Derleme | ✓ | `dotnet build -warnaserror` (Unit, Integration) | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (Web, Unit, Integration) | değişiklik yok |
| Consistency | ✓ | `--filter-trait "Consistency=true"` (docs commit'inden sonra) | 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T306-musteri-bildirimleri-1`
- **Commit:** bkz. PR (head SHA PR açıklamasında)
- **PR:** PR başlığı `T306: Müşteri bildirimleri B-1…B-8`
- **CI:** dal CI'ı validator izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | `CarrierCatalog` kodda (T310), `DispatchRecord` tablosu T117'den; yeni paket, harici servis, yapılandırma yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | B-3'ün "son iş günü başı" zamanlaması ve kesintide kaçanın dönüş koşulu | — (T324/T353 kapsamında) | Planda o task'larda |
| 2 | T305 ikinci cümle (`09 §11.4` m.7) | D-190 | T305'ten; T306 aynı okumayla ilerledi, kod değişikliği gerekmedi |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** 42364842661 (Publish) success · 42364842660 (CI) success · 42362112644 (Publish) success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** `origin/main` (T331 #425) dala birleştirildi, çakışma yok.
