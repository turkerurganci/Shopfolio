# T306 — Müşteri bildirimleri B-1…B-8

**Faz:** F3 | **Durum:** ✓ Tamamlandı — doğrulama tur 3 ✓ PASS (tur 1 ✗ FAIL: B1, B2; tur 2 ✗ FAIL: Bulgu 1; düzeltme 2 sonrası tam doğrulama) | **Tarih:** 2026-11-07

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
- B-5 "Takip et" bağlantısı (`EmailButton`, dış bağlantı) ana düğmeden önce gelir; **düzeltme 1'den beri dolu düğme değil, sabit renkli bağlantıdır** (B2).
- B-1 yasal metinler `OrderNoticeContext`'te doldurulmuş gelir (`PlaceholderFiller` çağrısı gönderim task'ının işi); B-3'ün "son iş günü başı" zamanlaması T324/T353'te, burada yalnız süre başına bir + dolmuşsa yok.
- `INotificationsModule`/`NotificationsModule` T308 (PR #424) ile aynı dosyalara dokunuyor; ikisi merge edilirken küçük çakışma beklenir.
- Aynı sipariş için eşzamanlı iki B-3: kilidi çağıran tutar (`09 §9.1` m.5); kuyruk yazıcısı kendisi kilit almaz.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ Notifications 202/202; Architecture 830/830 (düzeltme 2) | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build --filter-namespace …` | 0 hata |
| Integration | ✓ 14/14 | `--filter-class "*CustomerNoticeQueueTests"` (Postgres 18, 127.0.0.1:55600; düzeltme 2) | 0 hata |
| Derleme | ✓ | `dotnet build -warnaserror` (Unit, Integration) | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (Web, Unit, Integration) | değişiklik yok |
| Consistency | ✓ | `--filter-trait "Consistency=true"` (docs commit'inden sonra) | 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 3 — eskalasyon sonrası tam doğrulama, 2026-11-07; tur 2 ✗ FAIL, tur 1 ✗ FAIL) |
| Bulgu sayısı | tur 3: 0 · tur 2: 1 (Bulgu 1 ORTA S1 — düzeltme 2'de kapandı) · tur 1: 2 (B1 ORTA S1, B2 DÜŞÜK S1 — kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | tur 3: 1 (K1 → D-272) · tur 2: 0 · tur 1: 2 (K1 → D-231, K2 → D-232) |

### Doğrulama Sonucu — T306 Müşteri bildirimleri — tur 1
**Tarih:** 2026-11-07   **Branch:** task/T306-musteri-bildirimleri-1   **Commit:** `deb3a3f` (+ `origin/main` `1112ca6` birleştirmesi, kayıt commit'i)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42364842661 success · 42364842660 success · 42362112644 success |
| Memory drift | ✓ | `grep -nE "\bT306\b" .claude/memory/MEMORY.md` → satır 22 |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tür `DispatchType` B1…B8; iskelet ortak, hitapta ad yok; B-2/B-3/B-5 yeniden gönderimde güncel bilgi cümlesi | ✓ | `CustomerNoticeValues.For` sekiz tür; `CustomerNoticeTemplates.Make(…, carriesCurrentInfoNote)` yalnız B-2/B-3/B-5'te `true`; `EmailResend.Sentence` iki cümle; testler `TheEightNotices_…`, `EveryNotice_StartsWithMerhaba_WithoutAName_…`, `AResend_…OnlyB2B3B5…`, `ANewEventOfTheSameType_CarriesNeitherSentence` geçti |
| 2 | Sipariş numarası konuda ve olayın cümlesinde; ana düğme misafir/silinmiş hesapta anahtarlı, üyede hesap adresi | ✓ | `OrderLink`: `IsGuestOrder || IsAccountDeleted` → `EmailLinks.GuestOrder(AccessKey)`, aksi `MemberOrder`; anahtarsız misafir reddedilir; sekiz şablonun olay cümlesinde `{number}`; `ThePageButton_…` üç test, `TheSubjects_AreTheTitlesOfTheSpec` geçti |
| 3 | B-1 sipariş oluşunca ve misafir e-postası düzeltilince; iki yasal metin donan sürümle, ≥ 12 punto tam metin, ek yok | ~ Kısmi | Gövde ✓ (`Received`: düğmeden sonra iki `EmailLegalText`, sırayla; `EmailFrame.razor` `font-size:12pt` = gövde 16px, `-webkit-text-size-adjust:100%`; ek yok). **Düzeltme tetiği ✗:** `OrderReceivedNotice(OrderId, OriginalDispatchId)` → `CustomerNoticeRow(B1, OriginalDispatchId, null)` — satır ilk e-postanın oluşma anını olay değerlerine yazmaz; `06 §4.58` `EventValues` "düzeltme sonrası B-1'de ilk e-postanın değerleri ve oluşma anı", `08 §2.2.5` m.6 "ilk yeniden gönderim bu anı olayın değerlerine yazar … ilk satır kendi süresiyle önce silinebilir", `08 §2.2.6` B-1 "yeniden gönderim cümlesini o satırın tarihiyle taşır". `CustomerNoticeValues.Read(B1, …)` anı döndüremez; `OrderNoticeContext.ResendOf`'un kaynağı yalnız silinebilir ilk satır kalır. Test `TheB1AfterACorrection_IsANewRowToTheNewAddress_CarryingTheFirstRowsId` yalnız kimliği sınar → Bulgu B1 |
| 4 | B-2 donan IBAN, sipariş numarası, son ödeme günü | ✓ | `IbanBlock`: `order.BankTransferIban` (siparişin donan değeri) `DisplayFormat.Iban` ile; açıklama satırında numara; `LastPaymentDay(windowEnd−1 tick)`; `B2_…` üç test geçti |
| 5 | B-3 süre başına bir kez; son iş günü başı; kesinti dönüşü T324'te | ✓ | `CustomerNoticeQueue.ReminderIsDueAsync`: aynı sipariş ve aynı pencere sonunda ikinci satır yok, yeniden başlayan pencereye yeni satır, dolmuş pencereye satır yok; zamanlama T324 m.7'nin (`11` satır 2304); tümleşik `B3_IsWrittenOnceForAWindow_…`, `…Restarted_…`, `…HasRunOut_…`, `…OfAnotherOrder_…` geçti |
| 6 | B-4 kart onayı ve "ödendi"; dijital kalemde indirme cümlesi sipariş sayfasına | ✓ | `Confirmed`: `HasDigitalItem` → cümle, indirme adresi yok, düğme sipariş sayfası; `B4_WithADigitalItem_…` geçti |
| 7 | B-5 kargo bloğu şirket türüne göre; takip bağlantısı yalnız listedeki şirkette, `noreferrer` | ✓ | `Shipped` + `CarrierCatalog.Describe`: listede ad/numara/bağlantı, "Diğer" ve listeden çıkmışta bağlantısız, kendi araçta yalnız beyan; `EmailButton(isExternal: true)` → `rel="noreferrer"`; `B5_…` altı test geçti (biçim sapması ayrıca Bulgu B2) |
| 8 | B-6 ve üç B-7 söyleyişinde firma e-postası ve telefonu | ✓ | `FirmContact` B-6 ve B-7'nin her dalında; eksikse ret; `B6_…`, `B7_By…` testleri geçti |
| 9 | B-7 üç söyleyiş, sistem sebebi, kalem iptali ayrı konu, kalem ve adet yalnız onda, yönetici sebebi aynen + on dört gün | ✓ | `Cancelled`: Buyer/Admin/System dalları; `AdministratorReasonText`/`SystemReasonText` `08`'in listesiyle birebir; Closure reddedilir; on dört gün cümlesi yalnız yönetici dalında; kalem konusu `"Siparişinizdeki bir kalem iptal edildi"`; `B7_…` testleri geçti |
| 10 | B-8 neden, tutar, yöntem; IBAN yok; geç ödemede ayrı cümle | ✓ | `Refunded`: `RefundReason` listesi `08` ile birebir, `DisplayFormat.Amount`, kart/havale cümlesi, IBAN yazılmaz, `LatePayment` ayrı cümle + tutar satırı; `B8_…` üç test geçti |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
T306'nın KP birimi KP-66 (`11 §7.1`). `VAL-A317`–`A320` ve `A324` B-1…B-8'i anar, `A321`–`A323` B-9…B-16'yı (T307), `A325` bildirim ayarının yokluğunu. Hepsi bildirimin **gerçekten gönderilmesini** (sahte e-posta altyapısı, olayın tetiği) ister; T306 gönderen ve tetik çağrısı yazmaz (D-97, D-166, D-232) — T305'teki gibi hiçbiri T306'nın kriterine eşlenmez, `Durum` değişmedi.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-A317 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE | — |
| VAL-A318 | ORTA | mock | BEKLEMEDE | BEKLEMEDE | — |
| VAL-A319 | ORTA | mock | BEKLEMEDE | BEKLEMEDE | — |
| VAL-A320 | ORTA | mock | BEKLEMEDE | BEKLEMEDE | — |
| VAL-A324 | ORTA | mock | BEKLEMEDE | BEKLEMEDE | — |

#### Doğrulama Kontrol Listesi
- [x] KK1 sekiz tür kapalı listede; hitapta ad yok; B-2, B-3, B-5 yeniden gönderiminde ikinci cümle
- [x] KK2 misafir ve silinmiş hesapta anahtarlı, üyede hesap adresi
- [x] KK3 yasal metinler tam, en az 12 punto, ek yok
- [x] KK4 B-2 donan IBAN'ı yazar
- [x] KK5 B-3 süre başına bir
- [x] KK6 dijital kalemde indirme cümlesi
- [x] KK7 üç kargo biçimi ayrı
- [x] KK8 B-6 ve üç B-7 söyleyişinde telefon, e-posta
- [x] KK9 kalem iptalinde ayrı konu, kalem ve adet; sistem ve yönetici sebebi, on dört gün cümlesi
- [x] KK10 B-8'de neden, tutar, yöntem, geç ödeme cümlesi; IBAN yok
- [ ] `09 §25` — "Kod hiçbir doküman cümlesiyle çelişmiyor mu?" hayır: B1 (`06 §4.58`, `08 §2.2.5` m.6), B2 (`08 §2.2.5` m.2). Öteki satırlar ✓: modül sınırı (Architecture 809 yeşil), `TimeProvider`, tarih/tutar `DisplayFormat`, şema/paket/ayar yok, kaçış Razor'da (`MarkupString` yok)
- [x] Güvenlik taraması (e-posta gövdesinde kaçış, bağlantı, kişisel veri) — aşağıda

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (Web, Unit, Integration) | üçü çıkış 0 |
| Unit — Notifications | ✓ | `--filter-namespace Shopfolio.UnitTests.Notifications` | 142/142 |
| Unit — Architecture | ✓ | `--filter-namespace Shopfolio.UnitTests.Architecture` | 809/809 |
| Unit — Consistency | ✓ | `--filter-namespace Shopfolio.UnitTests.Consistency` | 209/209 |
| Unit — kalan | ✓ | `--filter-not-namespace` (üçü) | 2258/2258 (toplam 3418) |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration | ✓ | `--filter-class "*CustomerNoticeQueueTests"` (Postgres 18, `shopfolio-t306-val-pg`, 127.0.0.1:55610) | 11/11 |
| Integration komşu | ✓ | `JobQueueRegistrationTests`, `StatusEndpointTests`, `RevertLinkDigestRewriteStartupTests` · `RecipientSendLimitTests`, `DispatchAndChannelStateTablesTests`, `EmailOutageTests` | 46/46 · 46/46 |
| Dal CI | ✓ | `gh run list --branch task/T306-musteri-bildirimleri-1` | 42365665924 success (head `deb3a3f`) |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz (uç yok; `INotificationsModule` yalnız çağıranın işleminde yazar, işlem yoksa istisna)
- Input validation: Temiz — gövde değerleri (`ReasonNote`, ürün adı, takip numarası, kargo adı) Razor ile kodlanır; takip numarası adrese `Uri.EscapeDataString` ile girer (`CarrierCatalog.BuildLink`); `EmailButton` yalnız mutlak `https` kabul eder; olay değerlerinde ad, adres, IBAN, sipariş numarası yok (`TheEventsValues_HoldOnlyWhatTheEventHolds_…`); B-8 IBAN yazmaz
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| B1 | kriter 3 (satıra bağlı değil) | ORTA | S1 | Beklenen: düzeltme sonrası B-1 satırı olay değerlerine ilk e-postanın oluşma anını yazar; yeniden gönderim cümlesi bu değerden üretilir (`06 §4.58` `EventValues`; `08 §2.2.5` m.6; `08 §2.2.6` B-1 düzeltme tetiği). Gerçekleşen: `EventValues = null`; okuyucu anı döndüremez; ilk satır silinince cümlenin tarihi kaybolur. Düzeltme önerisi: kuyruk düzeltmede ilk satırın `CreatedAt`'ini okuyup (aynı sipariş ve `B1` olduğunu denetleyerek) değer olarak yazar ya da olay kaydı anı taşır; `Read` anı döndürür; tümleşik ve birim test | T385 panelden yeniden gönderimi (aynı değeri kopyalar) | bu PR'da düzeltilir | `CustomerNoticeValues.cs`, `CustomerNoticeQueue.cs`, `Contracts/CustomerNotices.cs`, testler |
| B2 | — (doküman uyumu) | DÜŞÜK | S1 | Beklenen: marka rengi yalnız ana düğmenin zemininde; bağlantılar ürünün sabit renginde (`08 §2.2.5` m.2; B-5 "Takip et" bir bağlantıdır, m.4 ana eylem düğmedir). Gerçekleşen: "Takip et" `EmailButton` ile marka renkli ikinci dolu düğme olarak, ana düğmeden önce çizilir (yapım raporu ⚠ notunda kabul etmiş, çözmemiş). Öneri: dış bağlantı için sabit renkli bağlantı bloğu (ya da `EmailButton`'a ikincil görünüm), `noreferrer` ve düz metindeki tam adres korunur | — | bu PR'da düzeltilir | `CustomerNoticeTemplates.cs` (`Shipped`), `EmailContent.cs`/`EmailFrame.razor` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `08 §2.2.6` B-2, B-3 ve B-8 geç ödeme örnek cümleleri numarasız; kod madde 2'yi izleyip numarayı ekledi — belge geri beslemesi | DEFERRED_BACKLOG'a forward: D-231 |
| K2 | `OrderNoticeContext`'i siparişten kuran gönderen yok (B-1 metinleri `PlaceholderFiller` ile onay anı değerleri, `ResendOf`, firma iletişimi) | DEFERRED_BACKLOG'a forward: D-232 |

#### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Kriter 3: yapım ✓ diyor; düzeltme sonrası B-1'in olay değerlerine ilk e-postanın anını yazmadığı (B1) raporda yok.
- B-5 "Takip et" ikinci düğme: yapım ⚠ notunda yazmış, `08 §2.2.5` m.2 ile çelişkisini değerlendirmemiş (B2).
- Öz denetim tablosu var, her madde kanıtlı (kriter 1–10, KK1–KK10, `09 §25`). "Known Limitations"ta `PlaceholderFiller` çağrısının gönderene kaldığı backlog kimliksiz yazılmış → K2/D-232.
- Test sayıları uyumlu (Unit 3418, Integration 11).

#### Merge ve Post-merge
- FAIL — merge yok. Kayıt commit'i dalda (rapor, status, changelog, memory, backlog D-231/D-232); `12` `Durum` değişmedi.

## Düzeltme 1

`origin/main` (T308 #424) dala birleştirildi: `INotificationsModule` ve `NotificationsModule`'de iki tarafın üyeleri de kaldı (`QueueCustomerNoticeAsync` + `QueueCompanyNoticeAsync` + `QueueAdminNoticeAsync`; kurucu iki kuyruğu birlikte alır).

| Bulgu | Yapılan | Kanıt |
|---|---|---|
| B1 (ORTA) olay değerine ilk e-postanın anı yazılmıyordu | `OrderReceivedNotice`'a `FirstEmailSentAt` eklendi (çağıran null bırakır). `CustomerNoticeQueue` düzeltmede ilk satırı okur — aynı sipariş ve `B1` olmak zorunda, değilse `ArgumentException` — ve satırın `CreatedAt`'ini (ilk satır kendisi düzeltme satırıysa onun taşıdığı anı) `EventValues`'a `FirstEmailSentAt` olarak yazar. `CustomerNoticeValues.Read(B1, …)` anı döndürür; şablon B-1'de yeniden gönderim cümlesini bu değerden üretir (yoksa `OrderNoticeContext.ResendOf`). | birim `TheB1AfterACorrection_CarriesTheFirstEmailsMoment_AsItsEventValue_AndTheReaderReturnsIt`, `TheB1AfterACorrection_MakesItsResendSentenceFromItsOwnEventValue_WithoutTheFirstRow`; tümleşik `TheB1AfterACorrection_WritesTheFirstRowsCreationMoment_AsItsEventValue_AndKeepsItAfterTheFirstRowIsDeleted` (ilk satır silinir, an kalır; ikinci düzeltme aynı anı taşır), `TheB1AfterACorrection_NeedsTheFirstRowToBeThisOrdersOwnB1` |
| B2 (DÜŞÜK) "Takip et" marka renkli ikinci dolu düğme | `EmailFrame.razor`: dış bağlantı (`EmailButton { IsExternal: true }`) artık düğme değil; `EmailColors.LinkText` sabit renkli, altı çizili bağlantı, `rel="noreferrer"` ve altında tam adres; marka rengi yalnız ana düğmenin zemininde. Düz metin aynı. | birim `B5_TheTrackingLink_IsAPlainLinkInTheFixedColour_AndTheBrandColourIsOnlyTheMainButtons` (markalı örnekte marka rengi yalnız tek düğmede, bağlantıda `background` yok); mevcut `B5_…NoReferrer` ve `AnExternalAddress_CarriesNoReferrer_…` yeşil |

**Öz denetim (düzeltme 1):** kriter 1–10, KK1–KK10 ve `09 §25` yeniden kendi diff'imde denetlendi: kriter 3 ve KK3 artık tam ✓ (düzeltme tetiği olay değerini yazar); kriter 7 ve KK7 ✓ (bağlantı yalnız listedeki şirkette, `noreferrer`, düğme değil); öteki maddeler değişmedi, ilgili testler yeşil. Sık FAIL nedenleri: (1) yeni tarama/istisna yok; (2) yeni tarama yok, `EmailRenderingSourceScan` Razor değişikliğini görür ve yeşil (`MarkupString` yok); (3) şema değişikliği yok; (4) sayı anan başka yer yok (backlog aktif 105 = `⚪`+`🟡` satırları, `origin/main` 103 + D-231/D-232); (5) olay değeri artık `EventValues`'ta.

### Doğrulama Sonucu — T306 Müşteri bildirimleri — tur 2
**Tarih:** 2026-11-07   **Branch:** task/T306-musteri-bildirimleri-1   **Commit:** `26e99f2` (+ kayıt commit'i; `origin/main` yeni commit taşımıyordu)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42368786653 success · 42368786665 success · 42366197659 success |
| Memory drift | ✓ | `grep -nE "\bT306\b" .claude/memory/MEMORY.md` → satır 22 |

#### Kabul Kriterleri
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | Sekiz kayıt → `B1`…`B8`; `"Merhaba,"` adsız; güncel bilgi cümlesi yalnız B-2, B-3, B-5; birim `TheEightNotices_…`, `EveryNotice_…`, `AResend_…`, `ANewEventOfTheSameType_…` |
| 2 | ✓ | Konu sonu ve olay cümlesinde numara; `OrderLink` misafir/silinmiş hesapta `/siparis/erisim/{key}`, üyede `/siparis/{no}`; birim `ThePageButton_…` ×3, `AGuestOrderWithoutAnAccessKey_…` |
| 3 | ~ Kısmi | Gövde, yerleşim, `12pt` (gövde 16px), yazı küçültme kapalı, ek yok ✓; düzeltmede ilk e-postanın anı olay değerinde ✓; **düzeltme satırının `OriginalDispatchId`'si ilk satırın kimliği değil** (Bulgu 1) |
| 4 | ✓ | `IbanBlock` donmuş `BankTransferIban` dörderli + sipariş numarası; son ödeme günü cümlesi; birim `B2_…` ×3 |
| 5 | ✓ | `ReminderIsDueAsync` aynı süre sonunda ikinci satır yok, dolmuş sürede yok, yeniden başlayan sürede yeni; zamanlama `TransferSchedule.ReminderAt`, tetik T324; tümleşik `B3_…` ×4 |
| 6 | ✓ | `HasDigitalItem` cümlesi, indirme adresi yok; birim `B4_…` |
| 7 | ✓ | `CarrierCatalog.Describe`/`BuildLink`; dış bağlantı `rel="noreferrer"`, sabit renk, düğme değil; birim `B5_…` ×7 |
| 8 | ✓ | `FirmContact` B-6 ve B-7'nin üç dalında; birim `B6_…`, `B7_…` |
| 9 | ✓ | Ayrı konu, `ItemText` + adet yalnız kalem iptalinde, sebep listeleri birebir, on dört gün cümlesi yalnız yönetici dalında; birim `B7_…` ×6 |
| 10 | ✓ | Neden listesi, `DisplayFormat.Amount`, kart/havale cümlesi, IBAN yok, geç ödeme cümlesi; birim `B8_…` ×3 |

#### Task Satırları (`12 §7.5`)
`VAL-A317` (KRİTİK), `A318`, `A319`, `A320`, `A324` (ORTA) — mock — `BEKLEMEDE` → `BEKLEMEDE`: gönderilmiş e-posta ve olayın tetiğini ister; T306 gönderen ve tetik yazmaz (D-97, D-166, D-232), kritere eşlenmez (T305 emsali). `A321`–`A323` T307, `A325` ayar yokluğu.

#### Test Sonuçları
| Tür | Sonuç | Çıktı |
|---|---|---|
| Derleme `-warnaserror` | ✓ | 0 uyarı, 0 hata |
| Format (Web, Unit, Integration) | ✓ | çıkış 0 |
| Unit (Consistency dışı) | ✓ | 3363/3363 |
| Consistency | ✓ | 209/209 |
| Betik testleri | ✓ | 4 dosya |
| Integration `CustomerNoticeQueueTests` | ✓ | 13/13 (Postgres 18, 127.0.0.1:55800) |
| Integration `*CompanyNotice*`, `*Dispatch*`, `*Email*` | ✓ | 49/49 |
| `dotnet restore --locked-mode` | ✓ | çıkış 0, `git status` temiz |
| Dal CI | ✓ | 42370396257 success (head `26e99f2`) |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz · Auth etkisi: Temiz · Input validation: Temiz (Razor kodlaması, `Uri.EscapeDataString`, `noreferrer`, konuda müşteri metni yok, `EventValues` kişisel veri taşımaz) · Yeni bağımlılık: Yok

#### Bulgular
| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Karar | Dosya |
|---|---|---|---|---|---|---|
| 1 | — (kriter 3, KK3) | ORTA | S1 | Düzeltme sonrası B-1 satırı siparişin ilk B-1 satırının kimliğini taşır; yeniden gönderilen satır kendisi bir yeniden gönderimse onun taşıdığı kimliği (`08 §2.2.6` B-1, `08 §2.2.5` m.6, `06 §4.58` `OriginalDispatchId`); grup ve "e-posta ulaşmadı" işareti bu kimlikten okunur ↔ `CustomerNoticeQueue` çağıranın verdiği kimliği aynen yazar; verilen satır bir düzeltme satırıysa anını taşır, kimliğini taşımaz. Daldaki tümleşik test ikinci düzeltmeyi tam bu yolla yazar (`OrderReceivedNotice(orderId, remaining.Id)`) ve üçüncü satırın kimliği `remaining.Id` olur — grup bölünür; ilk satır silinmişken asıl ilk kimlik `ArgumentException` alır. Öneri: `first.OriginalDispatchId ?? firstId` yaz, testte doğrula | bu PR'da düzeltilir | `CustomerNoticeQueue.cs`, `CustomerNoticeQueueTests.cs` |

#### Bloke-etmeyen Notlar
Yeni K-notu yok (tur 1'in D-231, D-232 kayıtları açık).

#### Yapım Raporu Karşılaştırması
1 uyuşmazlık: düzeltme 1 öz denetimi kriter 3 / KK3'ü "artık tam ✓" yazar; düzeltme yalnız anı taşır, kimliği taşımaz (Bulgu 1). Öteki kriterler ve ⚠ notları uyumlu; öz denetim tablosu kanıtlı.

#### Merge ve Post-merge
FAIL — merge yok. Tur 2 FAIL: `12 §8.5` iki iterasyon sınırı, eskalasyon (`INSTRUCTIONS.md` §3.3).

## Düzeltme 2

`origin/main` ilerlemedi (ad8c48d); birleştirme gerekmedi.

| Bulgu | Yapılan | Kanıt |
|---|---|---|
| 1 (ORTA) düzeltme sonrası B-1'in `OriginalDispatchId`'si verilen satır düzeltme satırıysa kökü taşımıyordu | `CustomerNoticeQueue` verilen satırdan kimliği de okur ve `OriginalDispatchId` olarak `given.OriginalDispatchId ?? givenId` yazar (zincir köke iner; kök satırı silinmiş olsa da arama gerekmez, kimlik ve an verilen satırdan gelir). Satır yine aynı sipariş ve `B1` olmak zorunda. Çağıran ilk satırı ya da zincirin herhangi bir satırını verebilir; aynı kimlik grubu belirler, "e-posta ulaşmadı" işareti grubun en son satırını okur (`DispatchMarks`, değişmedi). Sözleşme açıklaması güncellendi. | tümleşik `TheB1AfterACorrection_WritesTheFirstRowsCreationMoment_…` (ikinci düzeltmede üçüncü satır `OriginalDispatchId == first`, ilk satır silinmişken), yeni `TheSecondB1AfterACorrection_JoinsTheFirstRowsGroup_WhateverRowOfTheChainIsGiven` (ilk satır ya da ikinci satır verilse de bütün satırlar `first` kimliğinde, an ilk satırınki); `TheB1AfterACorrection_NeedsTheFirstRowToBeThisOrdersOwnB1` yeşil |

**Öz denetim (düzeltme 2):** kriter 1–10, KK1–KK10 ve `09 §25` kendi diff'imde yeniden denetlendi: kriter 3 ve KK3 artık kimlik ve an için tam ✓ (kök çözülür, ilk an taşınır); öteki maddeler değişmedi, ilgili testler yeşil (Notifications 202, Architecture 830, tümleşik kuyruk 14). Sık FAIL nedenleri: (1) yeni tarama/istisna yok; (2) yeni tarama yok; (3) şema değişikliği yok (`original_dispatch_id` T117'den, FK yok); (4) sayı anan başka yer yok, backlog yeni kayıt açılmadı; (5) olay değeri `EventValues`'ta, kimlik zinciri kökü artık çözülüyor (T306 r2 satırı).

### Doğrulama Sonucu — T306 Müşteri bildirimleri — tur 3
**Tarih:** 2026-11-07   **Branch:** task/T306-musteri-bildirimleri-1   **Commit:** `3e9eb28` (+ kayıt commit'i; `origin/main` yeni commit taşımıyordu)

#### Verdict: ✓ PASS

Eskalasyon sonrası tam yeniden doğrulama: kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması baştan.

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42368786653 success · 42368786665 success · 42366197659 success |
| Memory drift | ✓ | `grep -nE "\bT306\b" .claude/memory/MEMORY.md` → satır 22 |

#### Kabul Kriterleri
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `CustomerNoticeValues.For` sekiz kayıt → `B1`…`B8`; `"Merhaba,"` adsız; ikinci cümle yalnız B-2, B-3, B-5 (`carriesCurrentInfoNote`); birim `TheEightNotices_…`, `EveryNotice_…`, `AResend_…`, `ANewEventOfTheSameType_…` |
| 2 | ✓ | Konu sonu `(Sipariş {n})` ve sekiz olay cümlesinde numara (D-231); `OrderLink` misafir/silinmiş hesapta `/siparis/erisim/{key}`, anahtarsızsa red, üyede `/siparis/{no}`; birim `ThePageButton_…` ×3, `AGuestOrderWithoutAnAccessKey_…` |
| 3 | ✓ | Cümle · düğme · iki `EmailLegalText` sırayla; `12pt` (gövde 16px), yazı küçültme kapalı, ek yok; düzeltmede `OriginalDispatchId = given.OriginalDispatchId ?? givenId` (zincir kökü) ve ilk an olay değerinde; birim `B1_…`, `TheTwoTexts_…`, `TheB1AfterACorrection_…`; tümleşik `TheSecondB1AfterACorrection_JoinsTheFirstRowsGroup_…`, `…KeepsItAfterTheFirstRowIsDeleted`, `…NeedsTheFirstRowToBeThisOrdersOwnB1` |
| 4 | ✓ | `IbanBlock` donmuş IBAN dörderli + sipariş numarası; son gün `TransferSchedule` kuralı; birim `B2_…` ×3 |
| 5 | ✓ | `ReminderIsDueAsync` aynı süre sonunda ikinci satır yok, dolmuş sürede yok, yeniden başlayan sürede yeni; tetik T324 m.7; tümleşik `B3_…` ×4 |
| 6 | ✓ | Dijital kalem cümlesi, indirme adresi yok; birim `B4_…` |
| 7 | ✓ | `CarrierCatalog.Describe`/`BuildLink`; dış bağlantı sabit renk, `rel="noreferrer"`, düğme değil; birim `B5_…` ×7 |
| 8 | ✓ | `FirmContact` B-6 ve B-7'nin üç dalında; birim `B6_…`, `B7_…` |
| 9 | ✓ | Ayrı konu, kalem + adet yalnız kalem iptalinde, iki sebep listesi birebir, `Other` aynen, on dört gün yalnız yönetici dalında; birim `B7_…` ×6 |
| 10 | ✓ | Neden listesi, `DisplayFormat.Amount`, kart/havale cümlesi, IBAN yok, geç ödeme cümlesi; birim `B8_…` ×3 |

#### Task Satırları (`12 §7.5`)
`VAL-A317` (KRİTİK), `A318`, `A319`, `A320`, `A324` (ORTA) — mock — `BEKLEMEDE` → `BEKLEMEDE`: gönderilmiş e-postayı ve olayın tetiğini ister; T306 gönderen ve tetik yazmaz (D-166, D-232). `A321`–`A323` T307, `A325` ayar yokluğu. `12_matris_denetle.py --kapsam 1-77` hata 0.

#### Test Sonuçları
| Tür | Sonuç | Çıktı |
|---|---|---|
| Derleme `-warnaserror` | ✓ | 0 uyarı, 0 hata |
| Format (Web, Unit, Integration) | ✓ | çıkış 0 |
| Unit Architecture / öteki (Consistency dahil) | ✓ | 830/830 · 2742/2742 (T306 sınıfı 108) |
| Betik testleri | ✓ | 4 dosya |
| Integration `CustomerNoticeQueueTests` | ✓ | 14/14 (Postgres 18, 127.0.0.1:55760) |
| Integration `CompanyNoticeQueueTests`, `DispatchAndChannelStateTablesTests` | ✓ | 11/11, 30/30 |
| `dotnet restore --locked-mode` | ✓ | çıkış 0, `git status` temiz |
| Dal CI | ✓ | 42373330980 success (head `3e9eb28`) |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz · Auth etkisi: Temiz · Input validation: Temiz (Razor kodlaması, düz metinde tek satır, `Uri.EscapeDataString`, `noreferrer`, konuda müşteri metni yok, `EventValues` kişisel veri taşımaz) · Yeni bağımlılık: Yok

#### Bulgular
Yok.

#### Bloke-etmeyen Notlar
| # | Not | Karar |
|---|---|---|
| K1 | `DispatchRecord.EventValues` `jsonb` yük biçimi `PayloadJson`/`HasConversion` dışında ve `UnitTests/Payloads/` örnek dosyasız (`09 §12` m.5, m.8); T304/T308 kalıbı, T306 aynı listeye B-1…B-8 adlarını ekler | DEFERRED_BACKLOG'a forward → D-272 |

#### Yapım Raporu Karşılaştırması
Tam uyumlu; öz denetim tablosu kanıtlı. Küçük ifade farkı: ⚠ notu B-7 sebebini "Sebep: {metin}." yazar, kodda sonda nokta yok.

#### Merge ve Post-merge
Yönetici yardımcı düzeni: merge ve post-merge izleme yardımcının (adım 17–18).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T306-musteri-bildirimleri-1`
- **Commit:** bkz. PR (head SHA PR açıklamasında)
- **PR:** #427 — `T306: Müşteri bildirimleri B-1…B-8`
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
