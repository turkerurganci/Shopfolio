# T307 — Müşteri bildirimleri B-9…B-16

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Olay kayıtları** (`Modules/Notifications/Contracts/CustomerNoticesB9ToB16.cs`): `CustomerNotice` altında B-9…B-16 için sekiz kayıt tipi (`DeclarationReceivedNotice`, `AddressCorrectedNotice`, `ItemRemovedNotice`, `CarrierCorrectedNotice`, `StatusCorrectedNotice`, `RefundIbanRequestedNotice`, `OrderEmailChangedNotice`, `ReturnRejectedNotice`) ve ortak `NoticeItem`, `NoticeAddress`. Yeni enum açılmadı: B-9'un başlığı `DispatchRelatedRecordType` (cayma, gecikme feshi, ayıp talebi) ile `IsReopening` işaretinden türer.
- **Olay değerleri** (`CustomerNoticeValues.AfterOrder.cs`; `CustomerNoticeValues` partial oldu): B-9…B-16 için yazma ve okuma; kalemler indeksli anahtarlarla, adresler `Return`/`New` önekiyle. `CustomerNoticeRow` sonuna `RelatedRecordType`/`RelatedRecordId` eklendi; B-9 bağlı kaydını (cayma, fesih ya da ayıp talebi kaydı) değer olarak taşır. Türün kuralını bozan olay `ArgumentException` ile reddedilir; eksik ya da bozuk liste bildirim sayılmaz.
- **Gövde şablonları** (`CustomerNoticeTemplates.AfterOrder.cs`; `CustomerNoticeTemplates` partial oldu): sekiz tür için konu ve gövde, T306'nın iskeleti ve yeniden gönderim cümleleri üstünde. B-5'in kargo bloğu `ShippingBlocks` olarak ortak yapıldı (B-5/B-12). `OrderNoticeContext`'e sonda opsiyonel `ProvinceNames` (il adı) eklendi.
- **Kuyruk yazıcısı** (`CustomerNoticeQueue.cs`): B-9'un bağlı kaydını `DispatchRecord.Queue`'ya geçirir; ötekiler bağlı kayıtsız (06 §4.58 tür tablosu). `INotificationsModule` imzası değişmedi.
- **Belgeler:** bu rapor, status, changelog, memory; T306 ve T309 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri; "B-1…B-8" ifadesini anan genel yorumlar (`CustomerNotices.cs`, `NotificationsModule.cs`) B-1…B-16 yapıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Notifications/`: `Contracts/CustomerNoticesB9ToB16.cs` (yeni), `Contracts/CustomerNotices.cs`, `Contracts/INotificationsModule.cs`, `CustomerNoticeValues.AfterOrder.cs` (yeni), `CustomerNoticeValues.cs`, `CustomerNoticeTemplates.AfterOrder.cs` (yeni), `CustomerNoticeTemplates.cs`, `CustomerNoticeQueue.cs`, `NotificationsModule.cs`
- `tests/Shopfolio.UnitTests/Notifications/CustomerNoticeAfterOrderTemplateTests.cs` (yeni)
- `tests/Shopfolio.IntegrationTests/Model/CustomerNoticeAfterOrderQueueTests.cs` (yeni)
- `Docs/TASK_REPORTS/` (T307, T306, T309), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | B-9 dört başlık; cayma/fesihte tarih, ayıp talebi/yeniden açmada tarih ve saat; kalemler ve adetler; fiziksel kalemde caymada ve ayıp talebinde iade adresi | ✓ | `B9_HasFourHeadings_EachWithTheOrderNumberAtTheEnd`, `B9_OfAWithdrawalAndATermination_WritesTheDateAlone_NoTime`, `B9_OfADefectClaimAndItsReopening_WritesTheDateAndTheTime_InTheTurkeyClock`, `B9_ListsTheItemsAndTheQuantities_…`, `B9_OfAWithdrawalAndOfAClaim_WritesTheReturnAddress_…`, `B9_OfATermination_HasNoReturnAddress_…`, `B9_OfADigitalOnlyDeclaration_…` |
| 2 | B-10 yeni adres, eski yok; B-10, B-15, B-16 firma e-postası ve telefonu | ✓ | `B10_WritesTheNewAddressOfTheDelivery_WithTheTelephone_AndNoOldAddress`, `B10_OfTheBilling_…`, `B10AndB15_WriteTheFirmsEmailAndTelephone_…`, `B16_WritesTheFirmsContact_…`, `B10AndB15AndB16_WithoutTheFirmsEmailOrTelephone_AreRefused` |
| 3 | B-11 kalem ve adet, tutar yok | ✓ | `B11_WritesTheItemAndTheQuantity_AndNoAmount` |
| 4 | B-12 düzeltilmiş kargo bloğu | ✓ | `B12_WritesTheCorrectedShippingBlock_OfAListedCompany_…`, `B12_OfAnotherCompany_…`, `B12_IsTheShippedBlockOfB5_…`, `B12_BreakingTheRuleOfB5_IsRefused` |
| 5 | B-13 durum adı kapalı listeden, sebep yok | ✓ | `B13_WritesTheStatusNameOfTheClosedList_AndNoReason`, `B13_AWindowEndWithAnotherStatusThanPlaced_AndAnUnknownStatus_AreRefused` |
| 6 | B-13 "ödendi" düzeltmesinde yeni son ödeme günü ve IBAN bloğu | ✓ | `B13_OfThePaidMarksCorrection_WritesTheNewLastPaymentDay_AndTheFrozenIbanBlock`, `…WithoutAFrozenIban_IsRefused` |
| 7 | B-14 altı olay tek bildirim; sebebe göre cümle; kendi düğme etiketi; IBAN yok, IBAN göndermeme cümlesi | ✓ | `B14_IsOneNotice_WithTheSentenceOfTheEvent` (altı sebep), `B14_HasItsOwnButtonLabel_CarriesNoIban_AndSaysNotToReplyWithAnIban`, `B14_TheSecurityLine_FollowsTheButton`, `B14_AnUnknownReason_IsRefused` |
| 8 | B-15 eski adrese gider; yeni adres, ad, bağlantı, düğme yok | ✓ | `B15_WritesTheDateOfTheChange_AndNoNewAddressNoNameNoAddressNoLinkAndNoButton`, `B15_OfAMembersOrder_HasNoAccountLinkEither`, `B15_WorksForAnOrderWithNoAccessKeyAtAll_…`; tümleşik `B15_GoesToTheAddressItIsGiven_TheOldOne_…` |
| 9 | B-16 kalem, ambalaj sebebi, geri ödeme yok, geri gönderim, uyuşmazlık yolları, firma iletişimi; sınır rakamı yok | ✓ | `B16_WritesTheItem_TheReason_NoRefund_TheReturnShipment_TheDisputeWays_AndTheFirmsContact`, `B16_WritesNoMonetaryLimit_AsAFigure`, `B16_ForAnyOtherExemption_IsRefused` |
| 10 | Yeniden gönderim ilk olayın değerleri ve cümlesiyle; yalnız B-10/B-12/B-13'te güncel bilgi cümlesi; yeni olay iki cümleyi taşımaz | ✓ | `AResend_CarriesTheSentenceOfTheFirstEmail_AndOnlyB10B12B13CarryTheCurrentInfoSentence`, `ANewEventOfTheSameType_CarriesNeitherSentence_B12Included`, `AResentB12_CarriesTheValuesOfTheFirstEvent_NotALaterCorrection`, `AResentB13_CarriesTheStatusAndTheWindowOfTheFirstEvent` |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçinin kodu dahil bütün maddeleri diff'te yeniden denetledi (`CustomerNoticeValues.AfterOrder.cs`, `CustomerNoticeTemplates.AfterOrder.cs`, iki test dosyası).

| Madde (KK) | Sonuç | Kanıt |
|---|---|---|
| KK1 dört başlık, tarih biçimi; kalemler; iade adresi caymada ve ayıp talebinde var, fesihte yok | ✓ | `B9_HasFourHeadings_…`, `B9_OfAWithdrawalAndATermination_…`, `B9_OfATermination_HasNoReturnAddress_…` |
| KK2 eski adres yok; B-10, B-15, B-16'da telefon ve e-posta | ✓ | `B10_…AndNoOldAddress`, `TheEventsValues_HoldOnlyWhatTheEventHolds_NoIbanNoOrderNumberNoEmailNoOldAddress`, `B10AndB15_…`, `B16_WritesTheFirmsContact_…` |
| KK3 B-11'de tutar yok | ✓ | `B11_…AndNoAmount` |
| KK4 düzeltilen takip bilgisi yeni | ✓ | `B12_WritesTheCorrectedShippingBlock_…` |
| KK5 durum adı listede, sebep yok | ✓ | `B13_WritesTheStatusNameOfTheClosedList_AndNoReason` |
| KK6 ödendi düzeltmesinde yeni gün ve IBAN | ✓ | `B13_OfThePaidMarksCorrection_…` |
| KK7 altı olay cümlesi; IBAN yok, güvenlik cümlesi var | ✓ | `B14_IsOneNotice_…`, `B14_HasItsOwnButtonLabel_…`, `B14_TheSecurityLine_…` |
| KK8 B-15'te bağlantı ve düğme yok | ✓ | `B15_Writes…NoLinkAndNoButton` |
| KK9 B-16'da sebep, geri ödeme yok, geri gönderim, uyuşmazlık yolları, firma iletişimi; sınır rakamı yok | ✓ | `B16_WritesTheItem_…`, `B16_WritesNoMonetaryLimit_AsAFigure` |
| KK10 yeniden gönderimde ilk olayın değerleri; B-10/B-12/B-13'te ikinci cümle; yeni B-12 olayında ikisi de yok | ✓ | `AResend_…`, `ANewEventOfTheSameType_…`, `AResentB12_…`, `AResentB13_…` |
| `09 §25` ilgili satırlar | ✓ | Architecture (tüm Unit 3749 içinde) yeşil: modül sınırı, `EmailRenderingSourceScan`, enum kilitleri; yeni 09 metni yok |
| `12` kontrol listesi (A324, A335) | ✓ | İkisi de gönderim task'ına bağlı (`BEKLEMEDE`); T307'nin payı olay başına tek satır (`ASecondEventOfTheSameType_IsANewRow_NeverMergedOrDeduplicated`) ve her satırın değerinin satırda kalıcı olması (`TheBody_IsMadeFromTheStoredRow_TheSameAsFromTheNotice`, `TheEventsValues_RoundTrip_…`); `Durum` değişmedi |

**`sik-fail-nedenleri` kontrolü (T306'nın üç FAIL nedeni dahil):**

1. Tarama/bekçi istisnası geniş: yeni tarama ve istisna eklenmedi; `EmailRenderingSourceScan` yeni dosyaları kapsar, yeşil.
2. Kaynak taraması çağrı biçimi kaçırma: yeni tarama yok.
3. "Varsayılan"/FK dizini: şema değişikliği yok (`DispatchRecord` T117'den; B-9 bağlı kayıt sütunları orada).
4. Sayı anan öteki yerler: `grep -rn "B-1…B-8" src tests` ile bakıldı; kalanlar T306'nın kendi parçasını (B-1…B-8 şablon/değer/test dosyaları) doğru anlatır, genel yorumlar (`CustomerNotices.cs`, `NotificationsModule.cs`, `CustomerNoticeQueue.cs`) B-1…B-16 yapıldı. Backlog aktif sayısı değişmedi (108).
5. Karar kaydı gerektiren 09 metni: yok.
6. T306 dersi 1 — olay değeri satırın `EventValues`'ında: sekiz türün hepsi için `TheEventsValues_RoundTrip_ForEveryShapeOfTheEightNotices`; B-9'un bağlı kaydı `…OfB9_RoundTrip_WithTheRelatedRecordId`.
7. T306 dersi 2 — ilk gönderim anı: B-9…B-16'nın yeniden gönderiminde değerler satırdaki ilk olayın değerleridir (`AResentB12_…`, `AResentB13_…`); B-1'in düzeltme satırı mantığı bu task'ta değişmedi.
8. T306 dersi 3 — `OriginalDispatchId` kökü: yalnız B-1 düzeltmesinde vardır; B-9…B-16 `OriginalDispatchId` yazmaz (`CustomerNoticeQueue`, B-1 dalı dışında dokunulmadı).

**⚠ Karar notları:**

- B-9 iade adresi yeniden açmada da (isteğe bağlı) yazılabilir; fesihte yasak. Şablon "İade adresi:" paragrafı + Alıcı adı/Adres/İlçe/İl/Telefon satırları.
- B-9 kalemleri "Kalem: {ad (seçenekler)}, {n} adet" satırları.
- B-14'ün olay cümlesi sipariş numarasını taşımaz (spec sırası); numara ikinci cümlede ve konuda.
- B-13: pencere sonu yalnız "ödendi" düzeltmesinde (durum Placed şart); IBAN bloğu B-2'nin bloğu (donmuş IBAN yoksa reddedilir).
- İl adı için `OrderNoticeContext.ProvinceNames` (gövdeyi üreten çağıran doldurur; adres yazan bildirimde eksikse `ArgumentException`). Bu üretici D-232'nin kapsamındadır.
- B-15 için bağlantı/düğme/ad/yeni adres yok; alıcı adresini çağıran verir (eski adres).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 3749/3749 (Architecture dahil) | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | 0 hata |
| Integration | ✓ 22/22 (14 T306 + 8 T307; `*CustomerNotice*`) | `--filter-class "*CustomerNotice*"` (Postgres 18, 127.0.0.1:55400) | 0 hata |
| Derleme | ✓ | `dotnet build -warnaserror` (Web, Unit, Integration) | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (Web, Unit, Integration) | değişiklik yok |
| Consistency | ✓ | `--filter-trait "Consistency=true"` (docs commit'inden sonra) | bkz. Notlar |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-293) |

### Doğrulama tur 1 — validator kanıtları (2026-11-07)

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `scripts/git-hooks`; ana dal son 3 run 42382907093, 42382907080, 42379036303 success; memory'de T307 satırı var.
- **Kabul kriterleri 1–10:** hepsi ✓ — kod `08 §2.2.6` B-9…B-16 ve madde 1–6 ile satır satır karşılaştırıldı (`CustomerNoticeTemplates.AfterOrder.cs`, `CustomerNoticeValues.AfterOrder.cs`); konu başlıkları, olay cümleleri, B-14'ün altı sebep cümlesi ve düğme etiketi, B-13 durum adları, B-16 metinleri spec'le birebir; yeniden gönderimde güncel bilgi cümlesi yalnız B-10/B-12/B-13'te (`Make(..., true, ...)`). Testler: `CustomerNoticeAfterOrderTemplateTests` 120/120, `CustomerNoticeAfterOrderQueueTests` + `CustomerNoticeQueueTests` 22/22 (Postgres 18, 127.0.0.1:55611).
- **Yerel sınama:** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı/0 hata; `dotnet format --verify-no-changes` Web/Unit/Integration çıkış 0; Unit 3536 + Consistency 213 (birleştirme sonrası Unit 3572) geçti; betik testleri 4 dosya geçti.
- **Dal CI:** 42385697297 (head `43be541`) success.
- **`12` satırları:** `VAL-A321`–`A323` (KP-66; B-9…B-16) gönderilmiş e-posta ve tetik ister; T307 gönderen ve tetik yazmaz (D-97, D-166, D-232, D-293) — `BEKLEMEDE` → `BEKLEMEDE` (T305/T306 emsali).
- **Güvenlik:** diff'te sır, `Html.Raw`/`MarkupString`/`HtmlString`, saat okuması ve yeni paket yok; değerler `EmailComposer` kaçışıyla basılır (`B9_AMarkupInAnItemOrAnAddress_IsTextInTheHtml`); B-14 IBAN taşımaz, B-15 bağlantı/ad/yeni adres taşımaz.
- **K1 → D-293:** gönderen ve yeniden gönderim yazıcısı için T307'nin eklediği bağlar (il adları `ProvinceNames`, B-9'un `RelatedRecordId`'sinin `Read`'e verilmesi ve kopyalanması, iade adresinin yalnız fiziksel kalemde caymada/ayıp talebinde verilmesi) D-232'de yazılı değildi.
- **Yapım raporu karşılaştırması:** tam uyumlu; öz denetim tablosu her madde için kanıtlı. Yapım raporu ProvinceNames'i D-232 kapsamında saydı ama D-232 metnine eklenmemişti — D-293 ile kapatıldı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`DispatchRecord` ve B-9 bağlı kayıt sütunları T117'den)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T307-musteri-bildirimleri-2`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #435
- **CI:** 42385697297 success (head `43be541`; validator)
- **Post-merge run'lar:** squash `7150d1a` (#435) — CI 42388017617 success, Publish 42388017677 success (T336 yapımında dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Satırdan gövdeyi kuracak `OrderNoticeContext` üreticisi (il adları dahil) | D-232 | T306'da açıldı; gönderim task'ının işi |
| 2 | B-9…B-16'ya özgü bağlar: `ProvinceNames`, B-9 `RelatedRecordId`'nin `Read`'e verilmesi ve yeniden gönderimde kopyalanması, iade adresi koşulu | D-293 | Doğrulama tur 1 K1; gönderen ve tetik task'larının işi |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** 42364842660, 42362112617, 42364842661 success (önceki işçi); `main` son koşular 42382907080 CI, 42382907093 Publish success
- **Repo memory (bitiş kapısı 9):** güncellendi (`.claude/memory/MEMORY.md` T307 satırı)
- **Devir:** önceki işçi kodu T306 henüz merge değilken yazdı; bu işçi `origin/main`'e (T306 dahil) patch'i `--3way` ile uyguladı, tek çakışma `CustomerNoticeQueue.cs` xml-doc'unda çıktı (T306'nın son hâli korundu, B-9 ibaresi eklendi)
- **Önceki task'ların merge kaydı:** T306 (`9e3f8c3`, #427) ve T309 (`987e438`, #429) post-merge satırları ve status Commit hücreleri dolduruldu
