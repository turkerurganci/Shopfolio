# T527 — Sipariş listesi, sipariş ayrıntısı ve durum geçişleri (E-36, E-37)

**Faz:** F5 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-12

---

## Yapılan İşler

- Sipariş listesi `Pages/Panel/Orders.cshtml(.cs)` (`GET /panel/siparisler`, UC-91): tam numara ya da e-posta araması (`search`), tekrarlı sipariş ve ödeme durumu süzgeçleri, tek bekleyen iş süzgeci (`pendingWork`); kapalı listenin dışındaki değer süzgeç uygulamaz. Süzgeç çipleri, boş sonuç cümlesi. Liste `IPanelOrderList` (`Modules/Administration/PanelOrderListService.cs`): bekleyen iş kümeleri ana sayfa sayaçlarının tek kural evi `PendingWorkOrders`'tan okunur (sayaç ile liste ayrışmaz; `PanelHomeService` aynı evi kullanır). Süresiz sıra en yeni önce (veritabanında sayfalı, eşit anlarda iç anahtarla); süreli süzgeçte (kargoya verilecek, hizmet, iade/geri ödeme, IBAN bekleniyor) en az kalan süre önce, aşılmış en başta, süresi işlemeyen en sonda (`OrderByTimeLeft`). Satır işaretleri ve en yakın süre sayfa başına toplu okumadır (`FindListMarksAsync`, `FindRunningPeriodsAsync`, `ReadRefundMarksOfOrdersAsync`).
- Sipariş ayrıntısı `Pages/Panel/OrderDetail.cshtml(.cs)` (`GET /panel/siparisler/{iç anahtar}`, UC-92): bölümler, panel işaretleri, kalem başına açık adet, süre ve kalanı, bekleyen geri ödeme tutarı (sunucunun okuması `IPanelOrderReader`; sayfa tutar hesaplamaz), adres ve e-posta geçmişi, iç not (salt okunur), ayıp talepleri (açılış, son yeniden açılış, iade adresi, B-9 ulaşmadı işareti), "e-posta ulaşmadı" altında ulaşmayan e-postalar. Birincil düğme hattın sıradaki adımıdır; yabancı anahtar 404.
- Altı durum adımı (UC-123…UC-128) `IPanelOrderSteps` (`Modules/Ordering/PanelOrderSteps.cs`): `MarkPaid` (onaysızda donmuş IBAN ve tutarla 200), `MarkShipped`/`MarkReshipped` (kargo bilgisi ya da araç beyanı; yalnız açık fiziksel kalem), `MarkDelivered` (ileri ya da kargodan önceki tarih 422; beyan günüyle çakışmada K-608 sıra sorusu), `MarkDeliveryFailed` (S7, gerekirse S11), `CloseDeliveryFailed` (iptal kaydı, geri ödeme, B-7 yok). Hepsi `Version` taşır; onay metinleri `IrreversibleConfirmations`'tan; her geçiş işlem izine yazılır.
- Bu turda: adım redleri `07 §6.2` koduyla ret kaydına yazılır (`OperationNotAvailable`, `ShipmentInfoMissing`, `InvalidDate`, `FieldRequired`, `DeliverySequenceRequired`, `OrderVersionConflict`; `09 §8.2` madde 3); süreli bekleyen iş süzgecinin sayfa testi eklendi (KK1, KK2); T527'ye devredilen backlog kalemleri tek tek kapatıldı ya da yeniden hedeflendi.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Panel/{Orders,OrderDetail}.cshtml(.cs)`, `Pages/Panel/_OrderShipmentFields.cshtml`, `Pages/Components/ItemRecordLabels.cs`, `Pages/Orders/OrderPageModels.cs`; `Modules/Administration/{PanelOrderListService,PendingWorkOrders,PanelHomeService,PanelHomeRules,AdministrationModule}.cs`, `Contracts/PanelOrderListContracts.cs`; `Modules/Ordering/{PanelOrderListReader,PanelOrderReader,PanelOrderSteps,OrderPeriods,PendingWorkReader,OrderingModule}.cs` ve sözleşmeleri; `Modules/AfterSales/*` (kalem kayıtları, ayıp talepleri, bekleyen iş), `Modules/Payment/PaymentModule.cs`, `Modules/Notifications/{DispatchMarks,NotificationsModule}.cs`. Testler: `tests/Shopfolio.IntegrationTests/Web/{OrderListPageTests,OrderDetailPageTests,OrderDetailPageTests.Steps,SessionAccessTests}.cs`, `Pages/Panel/AccessSample.cshtml`, `Administration/PanelHomeTests.cs`; `tests/Shopfolio.UnitTests/Administration/PanelOrderListOrderTests.cs`, `tests/Shopfolio.UnitTests/Architecture/{TransitionUnitSourceScan,TransitionUnitSourceScanTests,InternalNoteSourceScanTests}.cs`, `ViolationSamples/TransitionUnit/QueryFilters.cs`, `Support/ApplicationEnums.cs`. Belgeler: `Docs/DEFERRED_BACKLOG.md`, `Docs/DEFERRED_BACKLOG.d/{T521,T527,T532,T533}.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.d/T527.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Arama (tam numara / e-posta), iki durum süzgeci, bekleyen iş süzgeci; geçersiz değer süzgeç uygulamaz | ✓ | `OrderListPageTests.TheSearch_MatchesTheWholeNumberOrEmail_TheStatusFiltersApply_AndAnInvalidValueFiltersNothing_KK1`, `ATimedPendingWorkFilter_KeepsOnlyItsOrders_OverrunFirstThenLeastTimeLeft_AndAnOrderWithoutAPeriodLast_KK1_KK2` |
| 2 | En yeni önce; süreli süzgeçte en az kalan, aşılmış en başta; kendiliğinden iptal edilen ödenmemiş sipariş görünür | ✓ | `TheList_IsNewestFirst_ShowsTheSystemCancelledUnpaidOrder_AndPagesWithoutLossOnEqualMoments_KK2` (sayfa sınırı dahil), `ATimedPendingWorkFilter_..._KK1_KK2`; Unit `PanelOrderListOrderTests` |
| 3 | Bölümler, işaretler, açık işlemler; birincil düğme sıradaki adım; yabancı anahtar 404 | ✓ | `OrderDetailPageTests.TheScreen_ShowsTheOrderAndItsNextStepAsThePrimaryButton_AndAForeignKeyIsNotFound_KK3` |
| 4 | Açık adet, süre, kalanı, bekleyen geri ödeme sunucunun; adres/e-posta geçmişi, iç not yalnız panelde | ✓ | `TheScreen_ShowsThePanelOnlyParts_TheInternalNoteAndTheEmailHistory_FromTheServer_KK4`, `TheScreen_ShowsTheDefectClaimsSection_..._KK4`, `TheScreen_WithoutAClaim_HasNoClaimsSection_KK4`, `OrderListPageTests.TheRow_ShowsTheListMarks_AndTheRunningPeriodTheServerWorkedOut_KK4` |
| 5 | `MarkPaid` onaysız 200 (donmuş IBAN, tutar); onaylı Ö1 etkileri; açık değilse 422 | ✓ | `MarkPaid_Unconfirmed_Is200WithTheFrozenIbanAndTheAmount_AndWritesNothing_KK5`, `MarkPaid_Confirmed_Is303_MovesTheOrder_AndWritesTheAuditRow_KK5_KK10`, `MarkPaid_Confirmed_CommitsTheReservedUnitsAndTheCoupon_AndTakesOnlyTheOrderedVariantOutOfTheCart_KK5` (sayfadan: ayrılan adet kesin düşer, kupon hakkı kullanılmış olur, ayırma satırları silinir, sepetten yalnız siparişteki varyant çıkar); 422 `AStepThatIsNotOpen_Is422_..._KK6_KK8` |
| 6 | `MarkShipped`/`MarkReshipped` kargo bilgisi ya da beyan; onaysız 200, onaylı 303; yalnız açık fiziksel kalem; eksikse 422 | ✓ | `MarkShipped_Unconfirmed_Is200AndWritesNothing_Confirmed_Is303_SendsOnlyTheOpenPhysicalItem_AndWritesTheAuditRow_KK6_KK10`, `MarkReshipped_AfterTheReturn_Is303_..._KK6_KK10`, `AStepThatIsNotOpen_Is422_AndTheShipmentWithoutItsCarrierData_Is422_KK6_KK8` |
| 7 | `MarkDelivered` tarih; ileri ya da kargodan önce 422; beyanla çakışan tarih sıra sorusu | ✓ | `MarkDelivered_AFutureDateAndOneBeforeTheShipment_Are422_ADeclarationsDayAsksTheOrder_AndTheAnsweredDateIsWritten_KK7_KK10` |
| 8 | `MarkDeliveryFailed` S11 ya da Teslim edilemedi; `CloseDeliveryFailed` iptal kaydı ve geri ödeme açmaz | ✓ | `MarkDeliveryFailed_WithADeliveredItemAndNoOpenOne_EndsInDelivered_WithoutOne_StaysReturned_KK8_KK10`, `CloseDeliveryFailed_ClosesTheReturnedOrder_WithoutACancellationRecordOrARefund_AndWritesTheAuditRow_KK8_KK10` |
| 9 | `Version`; eşzamanlı ikinci gönderim yazmaz | ✓ | `ASecondSendingWithTheOldVersion_Is409_AndWritesNothing_KK9` |
| 10 | Her geçiş işlem izine | ✓ | KK5, KK6, KK7, KK8 testlerinin `AuditLogs` iddiaları (`MarkBankTransferPaid`, `MarkShipped`, `MarkDelivered`, `MarkDeliveryFailed`) |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin kodu dahil kriter 1–10 ve KK1–KK10 diff'te yeniden denetlendi. Bulunan eksikler: (a) adım redleri ret kaydı yazmıyordu (`09 §8.2` madde 3) — `OrderDetail` her reddi `07 §6.2` koduyla yazar, test eklendi; (b) KK1'in geçerli bekleyen iş süzgeci ve KK2'nin süreli sırası sayfa düzeyinde sınanmıyordu — test eklendi; (c) T527'ye devredilen 17 backlog kalemi taranmamıştı — her biri kapatıldı ya da yeniden hedeflendi; (d) komut sayısı testi yoktu (`09 §20` m.4) — iki test eklendi; (e) Unit TAM koşulmamıştı ve 8 sınıf kırmızıydı (giriş modeli adları, enum dosyaları ve kilidi, iki tarama, ret satırı değişkeni, null-forgiving gerekçeleri) — düzeltildi; (f) KK5'in Ö1 etkileri sayfadan sınanmıyordu — test eklendi. `sik-fail-nedenleri.md` okundu:

| Madde | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–10, KK1–KK10 | ✓ | yukarıdaki tablo |
| (1) Bekçi/tarama istisnası geniş | ✓ | iki dar istisna (aşağıda "Tarama istisnası" ve "İç not izinli dosyaları" satırları; ihlal örneği ve ters test yerinde); örnek erişim sayfası `AccessSample.cshtml` adına taşındı (`SessionAccessTests`) |
| (2) Kaynak taraması çağrı biçimi | ✓ | yeni tarama yok; ret satırı `RejectionLog.Rejected` ile (tek yazar; `RejectionLineSourceScan`) |
| (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok |
| (4) Aynı sayıyı anan öteki yerler | ✓ | bekleyen iş kümeleri tek evde (`PendingWorkOrders`), ana sayfa sayaçları aynı evi okur (`PanelHomeTests`); `UndeliveredNotice` yeni alanları varsayılanlı |
| (5) Belge kuralını değiştiren 09 metni | ✓ | `09` değişmedi |
| Ret kaydı kodu (`09 §8.2` m.3) | ✓ | `AStepThatIsNotOpen_Is422_..._KK6_KK8` (kayıtta `ShipmentInfoMissing`, `OperationNotAvailable`; sayfada kod yok) |
| Sayfalı liste sınır testi | ✓ | `TheList_IsNewestFirst_..._PagesWithoutLossOnEqualMoments_KK2` (eşit an, son sayfa ötesi) |
| Döngüde sorgu (`09 §20` m.4) | ✓ | okumalar sayfa başına toplu; komut sayısı testleri `OrderListPageTests.TheList_SendsTheSameNumberOfCommands_ForOneOrderAndForFour_KK1_KK4`, `OrderDetailPageTests.TheScreen_SendsTheSameNumberOfCommands_ForOneItemAndForThree_KK3_KK4` |
| Tarama istisnası (bu turda eklenen, sık FAIL 1) | ✓ (⚠ 1) | `TransitionUnitSourceScan`: yalnız `string[]` tipli sorgu süzgeci (`OrderQuery.OrderStatus/PaymentStatus`) geçer — tek metin, metin listesi ve durum dizisi hâlâ bulgu; örnek `ViolationSamples/TransitionUnit/QueryFilters.cs`, test `TransitionUnitSourceScanTests.Scan_LeavesOutOnlyTheQueryModelsArrayOfTexts_...` |
| İç not izinli dosyaları | ✓ (⚠ 1) | `InternalNoteSourceScan` izinli listesine yalnız panel ekranının üç dosyası (`PanelOrderReader`, `PanelOrderContracts`, `Pages/Panel/OrderDetail.cshtml`; `04` 9.9; KK4); müşteri tarafı kapalı kalır (`TheScreen_ShowsThePanelOnlyParts_..._KK4`) |
| Enum kilitleri | ✓ | `PanelOrderWork`, `PendingOrderKind` kendi dosyalarında; okuma biçimi oldukları için (sütun değil) `ApplicationEnums.NotStored`'da — `EnumTableConsistency`, `EnumLock` yeşil |
| Giriş modeli adları (`07 §5.3.1`) | ✓ | `MarkPaidInput`/`MarkShippedInput.Confirmed`, `MarkDeliveryFailedInput`, `CloseDeliveryFailedInput`; `OrderQuery` süzgeç adları `06` adlarıyla — ModelBinding tarama testleri yeşil |
| Para: tutar sunucunun tek kural evinden | ✓ | bekleyen geri ödeme ve tutarlar `IPanelOrderReader` / `IPaymentModule`'den; sayfa hesaplamaz |
| Devredilen backlog kalemleri | ✓ | D-648 ✅; D-308, D-614, D-645, D-875, D-1306 T527 payı kapandı; D-646, D-774, D-776, D-854 → T528/T531; D-874, D-898, D-906 → T529; D-981 C079 payı; D-1348, D-1365, D-1373 → faz kapısı |
| `09 §25` / güvenlik | ✓ | panel oturumu ve sürüm; yabancı anahtar 404; onaylı adımlar geri alınamaz onay ister; kod sayfaya yazılmaz; yeni bağımlılık, sır, şema yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | (devirde) | `dotnet test tests/Shopfolio.UnitTests` | TAM koşusu sonraki adımda |
| Integration | ✓ (parçalı) | `--filter-class`, Postgres 18 (`track_commit_timestamp=on`) | koşulan: `OrderDetailPageTests` 13/13, `OrderListPageTests` 4/4 (main 73e36672 birleşimi sonrası), `Files.OrderPageTests` 10/10 (önceki tur). **TAM: CI** |
| Biçim | (devirde) | `dotnet format <proje> --verify-no-changes` | sonraki adımda |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T527-siparis-listesi`
- **Commit:** *(PR açılınca)*
- **PR:** *(açılacak)*
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Geri ödeme süresinin başlangıcı | D-1382 | T529 |
| 2 | Ortak iz adları (UC-125, UC-128) | D-1383 | sahip kararı |
| 3 | E-posta geçmişinde kod adı; iç not yazma formu | D-1384 | sahip / UC sahibi task |
| 4 | Ulaşmayan e-postaları yeniden gönderme düğmeleri | D-1385 | T531 (UC-153) |
| 5 | Dar sınıf kartı, Z-42 sırası | D-1386 | faz kapısı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** devam işçisi; yönetici izler
- **Repo memory (bitiş kapısı 9):** parça dosyalarıyla (STATUS tablo satırı, `STATUS_CHANGELOG.d/T527.md`, `DEFERRED_BACKLOG.d/T527.md`)
- **⚠ (kolay, kararla):** (1) İki tarama genişletildi: `TransitionUnitSourceScan`'e `string[]` tipli sorgu süzgeci istisnası, `InternalNoteSourceScan`'e panel ekranının üç dosyası; ikisi de dar (ihlal örneği ve müşteri tarafı testi yerinde) — doğrulayıcı örnekle görsün. (2) Ayıp talebi işlemleri (çözüldü/yeniden aç) T528'in. (3) Ret satırı 409'da da `OrderVersionConflict` ile yazılır (`07 §6.1.3` "her ret"). (4) Dar sınıf kartı ve Z-42 sırası faz kapısında (D-1386); `04` 9.8.7 süreli süzgeç listesinde ayıp talebi yok.
