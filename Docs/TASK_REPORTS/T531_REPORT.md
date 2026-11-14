# T531 — Sipariş ayrıntısı: hizmet kalemi, indirme hakkı, iç not ve e-posta yeniden gönderim (E-37)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-14

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Modül yazma yüzü (`IPanelOrderUpkeep`, `PanelOrderUpkeep`):** dört işlemin yazma yarısı (09 §8.3 madde 3). Sürümlü dördü `PanelOrderSteps`'in sırasıyla işler: sipariş satır kilidi → ekrandaki sürüm (409) → karar tablosunun satırı (`OrderOperationCatalog.Check`; 422) → yazıcı. `CompleteServiceItem` geçiş biriminin `MarkServiceCompletedAsync`'ini çağırır ve `CompleteService` iz satırını (`ItemId`, kapanışta `TransitionCode`) yazar (D-646). `ResetDownloadCount` `DownloadCounters.RenewAsync`'i çağırır; bildirim ve iz satırı yok (`02 §3.12.6`, K-683). `ResendNotification`/`ResendAllNotifications` `INotificationsModule.ResendCustomerNoticeAsync`'i kilit altında çağırır; "hepsi" tek işlemde, biri reddedilirse hepsi geri alınır (K-761). İç not modülün var olan birimine (`OrderCorrections.SaveInternalNoteAsync`, 09 §10.9) gider; sonucu `PanelNoteResult` ile kapıya çevrilir.
- **Sayfa (`OrderDetail.Upkeep.cs`, kısmi sınıf):** beş `[GatedOperation]` işleyici (`CompleteServiceItem`, `ResetDownloadCount`, `SaveInternalNote`, `ResendNotification`, `ResendAllNotifications`), beş `…Input` modeli. İç not `OperationRunner.RunAsync(…, SaveInternalNoteRequest.RetryOnUniqueIndexes, ct)` ile koşar (yeni aşırı yükleme; iki ilk yazım tekil dizinde buluşur, ikinci koşu çakışmayı verir — 09 §10.9 madde 3). 409'da not bölümünde 04 2.10.2 mesajı ve güncel not; imha edilmiş siparişte alan kapalı ve POST 422.
- **Görünüm:** `_OrderItemUpkeep` (kalemin satırında hizmet "Tamamlandı olarak işaretle" ve dijital "İndirme hakkını yenile"; red mesajı kalemin yerinde), `_OrderServicePrimary` (birincil düğmenin dördüncü kolu, Z-39 göstergesiyle; D-1491), `_OrderInternalNote` (tek metin alanı + `Original` ikizi). Ortak `OrderDetail.cshtml`'e yalnız üç partial çağrısı ve bir `case`; `PrimaryOf`'a dördüncü kol; `OrderPrimaryStep.CompleteService`.
- **Ulaşmayan e-postalar bileşeni:** `UndeliveredEmailList`'e isteğe bağlı `Version`; formlar `Input.DispatchRecordId` ve `Input.Version` taşır; sipariş ayrıntısı düğmeleri açar (`CanResend: true`) ve yeniden gönderim yine "ulaşmadı" bittiyse (grubun son satırı ilk satırı değil) kalıcı mesajı gösterir (04 2.5.4).
- **Backlog:** D-509, D-645, D-646, D-967 (`DEFERRED_BACKLOG.md`), D-1385, D-1491 (`DEFERRED_BACKLOG.d/T527.md`) yerinde kapatıldı; D-884'ün T531 payı (müdahale 6) kapandı, hedefi T528, T530'a indi.
- **Önceki task'ların merge kaydı:** T527, T536, T510, T517, T518'in status "Commit" hücresi squash hash'ine çevrildi, raporlarının "Post-merge run'lar" satırı main run'larıyla dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/Contracts/IPanelOrderUpkeep.cs` (yeni) · `Modules/Ordering/PanelOrderUpkeep.cs` (yeni) · `Modules/Ordering/OrderingModule.cs` (+1 kayıt)
- `src/Shopfolio.Web/Infrastructure/Operations/OperationRunner.cs` (yeniden deneme aşırı yüklemesi)
- `src/Shopfolio.Web/Pages/Panel/OrderDetail.Upkeep.cs` (yeni) · `_OrderItemUpkeep.cshtml`, `_OrderServicePrimary.cshtml`, `_OrderInternalNote.cshtml` (yeni) · `OrderDetail.cshtml`, `OrderDetail.cshtml.cs` (kısa ekler)
- `src/Shopfolio.Web/Pages/Components/UndeliveredEmails.cs` · `Pages/Shared/_UndeliveredEmails.cshtml`
- Testler: `tests/Shopfolio.IntegrationTests/Web/OrderDetailPageTests.Upkeep.cs` (yeni, 8 test) · `OrderDetailPageTests.Steps.cs` (hizmet kalemine `FulfilmentPeriod`; yeniden gönder düğmesi beklentisi) · `tests/Shopfolio.UnitTests/Ordering/PanelNoteResultTests.cs` (yeni) · `Architecture/OperationHandlerMarkTests.cs` (beş işleyici) · `Architecture/InternalNoteSourceScanTests.cs` (izin listesi: `OrderDetail.cshtml` yerine `_OrderInternalNote.cshtml` ve `OrderDetail.Upkeep.cs`)
- Dokümanlar: bu rapor, `IMPLEMENTATION_STATUS.md` (T531 satırı + beş commit hücresi), `STATUS_CHANGELOG.d/T531.md`, backlog satırları, beş raporun post-merge satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `CompleteServiceItem` açık adetlerin tamamı; cayma düşer, "sorun bildir" açılır; son açık kalemde kapanış | ✓ | `OrderDetailPageTests.CompleteServiceItem_MarksTheWholeOpenQuantity_TheWithdrawalFalls_TheDefectChannelOpens_AndTheLastItemClosesTheOrder_KK1` |
| 2 | Ödenmemiş sipariş / kapanmış kalem 422; onay yok | ✓ | `CompleteServiceItem_OnAnUnpaidOrder_AndOnAClosedItem_Is422_AndWritesNothing_KK2`; KK1'de onaysız ilk POST 303 |
| 3 | Sayaç sıfır, bildirim yok; dijital olmayan 422 | ✓ | `ResetDownloadCount_PutsTheCounterAtZero_SendsNoNotice_AndAnItemThatIsNotDigital_Is422_KK3` |
| 4 | Not yazılır, boşaltılan silinir; sürüm artmaz; izde metin yok | ✓ | `SaveInternalNote_WritesChangesAndEmptiesTheNote_TheVersionStays_AndTheTrailHasNoText_KK4` |
| 5 | 409 güncel notla; imha edilmiş sipariş 422, alan kapalı | ✓ | `SaveInternalNote_AgainstAChangedNote_Is409WithTheCurrentNote_AndADisposedOrder_Is422WithTheFieldClosed_KK5` |
| 6 | Yeni "gönderilecek" satırı gruba; sonuç sonraki gösterimde | ✓ | `ResendNotification_WritesANewQueuedRowInTheGroup_AndTheResultIsReadAtTheNextDrawing_KK6` |
| 7 | B-1 donmuş sürüm; kilit altında koşul, eşzamanlı ikinci istek satır yazmaz | ✓ | `ResendNotification_TwoConcurrentRequests_WriteOneRow_AndTheSecondSending_Is422NotA409_KK7_KK8` (satır B-1, grubun ilk e-posta anı); gövdenin donmuş metinlerle üretimi `DispatchRetryAndMarkTests.EveryCustomerNotice_IsMadeFromTheOrder_SentAgainThroughTheQueue_…` |
| 8 | İşaretsiz / son olmayan satır 422; 409 yok | ✓ | aynı test: üçüncü gönderim 422, sürüm 1'de |
| 9 | Hepsi tek işlemde, hepsi ya da hiç; siparişler arası yok | ✓ | `ResendAllNotifications_WritesARowForEveryMarkedNotice_InOneOrderOnly_AndIsAllOrNone_KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: tamamlama, cayma düşer, kapanış | ✓ | `PanelOrderUpkeep.CompleteServiceItemAsync`; KK1 testi: kalem işaretli, S10, `CompleteService` izi (`ItemId`, `TransitionCode`), müşteri tarafında `WithdrawServiceItem` kapandı, `OpenDefectClaim` açıldı; karışık siparişte sipariş Hazırlanıyor'da kaldı |
| KK2: iki 422 sebebi | ✓ | KK2 testi: ödenmemiş ve işaretli kalem 422, red satırı `OperationNotAvailable`, mesaj kalemin satırında, sürüm 1, iz yok |
| KK3: sayaç sıfır; bildirim yok | ✓ | KK3 testi: `DownloadCount` 5 → 0, `DispatchRecords` ve iz 0; fiziksel kalem 422; eski sürüm 409 |
| KK4: boşaltma siler; izde metin yok | ✓ | KK4 testi: yaz → değiştir → boşalt; not silindi, sürüm 1, üç `SaveInternalNote` izinde metin yok |
| KK5: 409 ve imha edilmiş sipariş | ✓ | KK5 testi: 409 + `ConflictControls.OrderMessage` + güncel not `Original` alanında, kod `DisplayedValueConflict`; imhada alan yok, POST 422 `OperationNotAvailable` |
| KK6: yeni satır ve sonuç | ✓ | KK6 testi: `OriginalDispatchId` = ilk satır, "gönderilecek", sürüm değişmedi; satır yine "ulaşmadı" → `ResendFailedMessage`; gönderildi → işaret kalktı |
| KK7: donmuş sürüm; eşzamanlı iki istek | ✓ | KK7_KK8 testi: iki eşzamanlı POST → bir 303 bir 422, tek yeni satır, tek iz |
| KK8: ikinci gönderim 422 | ✓ | KK7_KK8 testi: üçüncü gönderim 422 (409 değil) |
| KK9: hepsi ya da hiç | ✓ | KK9 testi: iki satır yazıldı; B-15 adressiz siparişte 422 ve B-4 satırı da yazılmadı; öteki siparişe dokunulmadı |
| `09 §8.3` işaret ve kapı | ✓ | `OperationHandlerMarkTests` (beş işleyici `[GatedOperation]`), `OperationSourceScanTests` (işleyiciler `OperationRunner`'a ulaşır; görünümde durum adı/tarih aritmetiği yok) |
| `09 §8.2` red satırı kodu | ✓ | KK2, KK5 testleri `CodeOfRejection` |
| `09 §10.9` iç notun kilidi | ✓ | modülün var olan birimi; sayfa tekil dizin yeniden denemesiyle koşar (`OperationRunner.RunAsync(…, RetryOnUniqueIndexes, …)`) |
| `09 §15.2` sahtecilik / CSP | ✓ | yeni formlar `asp-antiforgery="true"`, satır içi stil/betik yok; `ForgeryProtectionTests`, `SecurityHeadersTests`, `PageFormPatternTests` yeşil |
| `09 §16.2` çift gönderim | ✓ | formlar `data-js="submit-once"`, yeniden gönder `data-js="resend-once"` |
| `09 §3.4` / 07 §5.1 model adları | ✓ | `ModelBindingRuleTests` yeşil (`OriginalText` `[SpecAssignedName]`) |
| İç not müşteriye sızmaz (T386 KK10) | ✓ | `InternalNoteSourceScanTests` izin listesi daraldı (`OrderDetail.cshtml` artık notu anmıyor, yerine `_OrderInternalNote.cshtml`) |
| Backlog: D-509 | ✓ kapandı | `ResetDownloadCount` ucu; KK3 testi |
| Backlog: D-645 | ✓ kapandı | hizmet düğmesi; KK1, KK2 testleri |
| Backlog: D-646 | ✓ kapandı | `CompleteService` izi; KK1 testi |
| Backlog: D-884 (müdahale 6) | ✓ T531 payı kapandı | iz satırı T386'da yazılmıştı; sayfa ucu KK4 testiyle; hedef T528, T530 |
| Backlog: D-967 | ✓ kapandı | düğme, koşul, kilit, eşzamanlı istek; KK6, KK7_KK8 testleri |
| Backlog: D-1385 | ✓ kapandı | düğmeler listeye bağlı; KK6 ve `TheScreen_ShowsTheDefectClaimsSection_…_KK4` |
| Backlog: D-1491 | ✓ kapandı | `PrimaryOf` dördüncü kol; KK1 testi (hizmet siparişinde birincil, karışıkta kargoya ver) |
| Sık FAIL (1) istisna geniş | ✓ | yeni istisna yok; not izin listesi daraldı |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | `OperationHandlerMarkTests` yorumu ve listesi, `OrderDetailModel` özeti, `UndeliveredEmailList` belge yorumu, T527 testinin "düğme yok" beklentisi güncellendi |
| Sık FAIL (3), (5) | — | şema ve 09 metni değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --max-threads 3` | 7582/7582 |
| Integration | ✓ (sınıflar) — TAM: CI | `--filter-class` `OrderDetailPageTests`, `OrderCorrectionTests`, `DispatchRetryAndMarkTests`, `DigitalDeliveryTests` (145/145); `ComponentMarkupTests`, `ForgeryProtectionTests`, `PageFormPatternTests`, `SecurityHeadersTests`, `SessionAccessTests` (83/83) | tam koşuyu CI'ın Integration işi yapar |
| Format | ✓ | `dotnet format <proje> --verify-no-changes --include <değişen dosyalar>` (Web, Unit, Integration) | değişiklik yok |
| Contract | — | — | uygulanmaz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T531-hizmet-indirme-not`
- **Commit:** *(PR açılışında)*
- **PR:** *(PR açılışında)*
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Tetikleyici tarama (plan tier, kota, API key, sandbox …) kabul kriterlerinde ve plan metninde eşleşme vermedi; yeniden gönderim ürünün kendi iş kuyruğundadır, dış servis çağrısı işlemin içinde yoktur.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Yok | — | — |

## Notlar

- **Working tree (Adım -1):** temiz; hook yolu `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 43566446647 success, 43566446578 success (T527), 43560690928 success, 43560690926 success (T536).
- **Repo memory (bitiş kapısı 9):** dokunulmadı (faz kapısı kayıt PR'ı günceller).
- ⚠ **Ortak dosya kuralı:** T529 ve T530 aynı sayfaya ekleme yapıyor. Ortak dosyalarda yalnız kısa ekler: sınıf `partial`, `OrderPrimaryStep.CompleteService`, `PrimaryOf`'un son kolu, ulaşmayan e-postalar satırındaki işleyici adları ve sürüm, `OrderDetail.cshtml`'de üç partial çağrısı ve bir `case`; iç not bölümünün iki gösterim satırı `_OrderInternalNote`'a taşındı (bölüm UC-152'nin). `PanelOrderUpkeep` kapısı `PanelOrderSteps.GateAsync`'in kalıbını kendi dosyasında yazar (ortak dosyanın özel yöntemini açmamak için); kural evi aynıdır (`OrderOperationCatalog.Check`).
- ⚠ **Kalem bu siparişin değilse ya da yoksa** `ResetDownloadCount` ve `CompleteServiceItem` 422 verir (karar tablosu "kapalı"); D-509'un andığı modül `NotFound`'u (404) bu yoldan doğmaz — UC-143 ve UC-139'un hata listesi yalnız 422'dir, 404 siparişin yokluğudur.
- ⚠ **`ResendAllNotifications` döngüsü** her işaretli bildirim için modülün `ResendCustomerNoticeAsync`'ini çağırır (bildirimin satırlarını her çağrı yeniden okur); küme sipariş başına en çok on altı bildirim tipiyle sınırlı bir yazma yoludur, ekranın GET'i değildir (09 §20 madde 4 GET'in kuralı).
- ⚠ **Yeniden gönderim yine başarısız** işareti, grubun son satırının ilk satırı olmamasından okunur (`ResendFailedAgain`): panelden ya da e-posta düzeltmesinden doğan yeniden gönderimin "ulaşmadı" bitişi.
- **Belge paketi:** `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/impl/T531/belge-paketi.md` (09 §3.4, §8, §15.2, §16.2; 07 UC-139, UC-143, UC-152–154; 03 §8.2, §8.4, §8.9; 02 §3.12.6, §3.20.9; 05 MK-31; 10 KP-44, KP-48); 04 E-37 `sed -n` ile.
