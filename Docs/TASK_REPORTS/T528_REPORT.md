# T528 — Sipariş ayrıntısı: firma iptali, bildirim kaydı, kalem çıkarma, iade malı ve ayıp talebi (E-37)

**Faz:** F5 | **Durum:** ⏳ Yapım tamam, doğrulama bekliyor | **Tarih:** 2026-11-15

---

## Yapılan İşler

- Panel kalem adımları `PanelItemSteps` (`IPanelItemSteps`): firma iptali (UC-129), başka kanal cayma ve fesih kaydı (UC-130), kalem çıkarma (UC-138), iade malı teslim alma ve ret (UC-140), stoğa ekle (UC-141), "mal dönmedi" kapatma (UC-142), ayıp talebini çözme ve yeniden açma (UC-144, UC-145). Her adım: sipariş satır kilidi → ekrandaki sürüm (409) → `OrderOperationCatalog.Check` (422) → geçiş birimi; iz satırı aynı işlemde (`09 §8.3` m.1–3, §9.2).
- Sayfa: `OrderDetail.ItemSteps.cs` işleyicileri `[GatedOperation]` + `OperationRunner`; `_OrderDetailItemSteps.cshtml` formları; ret sebepleri `PanelItemObstacle`.
- D-656 / D-911 (yönetici kararıyla kapsam içi, KK7/KK8 "her teslim alma"): fesih malı ve ayıpta dönülen mal teslim alma adımından geçer — `AfterSalesModule.AddTakenBackReceiptAsync` (`Return.LateDeliveryTerminationId` / `Return.DefectClaimId`; üst sınır feshedilen ya da dönülen − teslim alınan; tarih teslim tarihinden önce değil), geçiş `IOrderTransitions.ReceiveTakenBackAsync` (geri ödeme, IBAN, bildirim yok), `ReturnStepFacts.TakenBack` ve `ItemPeriodState.ReceivableUnits` aynı sayımla; panel "İade malını teslim al" beyan yokken önce fesih, sonra ayıp talebini alır. Ret yalnız cayma beyanında.
- D-1590: başka kanal fesih kaydında bildirimin IBAN'ı (geçersizse `IbanInvalid` 422).
- D-838: müşteri iptalinin kanuni faiz uyarısı testi.
- `OrderDetailPageTests.MarkBankTransferRefundFailed_…_KK6`: "satırda adım yok" denetimi ayıp talebinin kendi çöz/yeniden aç formunu (aynı `Input.DefectClaimId` adı) hariç tutar (`WithoutClaimStep`).

## Etkilenen Modüller / Dosyalar

- Ordering: `PanelItemSteps.cs`, `OrderTransitions.Returns.cs`, `OrderTransitions.LateDeliveryTermination.cs`, `Contracts/IPanelItemSteps.cs`, `Contracts/ItemRecordRequests.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/PanelItemObstacle.cs`, `OrderingModule.cs`
- AfterSales: `AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.OperationFacts.cs`, `AfterSalesModule.ReturnSteps.cs`, `Contracts/ItemRecordWrites.cs`, `Contracts/ReturnStepFacts.cs`, `Contracts/ItemPeriodStates.cs`, `Contracts/ItemRecordRefusal.cs`, `Contracts/IAfterSalesModule.cs`
- Sayfa: `Pages/Panel/OrderDetail.ItemSteps.cs`, `OrderDetail.cshtml(.cs)`, `_OrderDetailItemSteps.cshtml`
- Şema ve göç değişikliği yok.

## Kabul Kriterleri Kontrolü — öz denetim (bütün maddeler, önceki işçilerinkiler dahil)

| Madde | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 / kontrol "uyarılar, onay hâli" | ✓ | `OrderCorrectionTests.PanelItemSteps.cs`: `PanelFirmCancellation_BeforeThePayment_TakesNoItem_AndCancelsTheWholeOrder_T528_KK1_KK3`, `…_AfterThePayment_StopsAtTheWindowUnconfirmed_ThenCancelsTheUnits_AndStartsTheCardRefund_T528_KK1_KK3` |
| KK2 / "dört 422 sebebi" | ✓ | `PanelFirmCancellation_RefusesTheReasonTheItemAndTheUnits_AndAppliesNoPartOfTheRequest_T528_KK2` (sebep/açıklama, kalem yok, adet, iptale açık değil — ödenmiş dijital kalem `ConditionNotMet`) |
| KK3 / "iki hat ve ayırma" | ✓ | KK1 testleri + `PanelFirmCancellation_OnTheTransferLine_SaysSoInTheWindow_AndAsksTheIban_ForEveryChosenItem_InsteadOfARefund_T528_KK1_KK3` |
| KK4 / "cayma ve fesih alanları" | ✓ | `PanelWithdrawalNotice_WritesTheItemAndItsUnits_AfterTheWindow_T528_KK4`, `PanelLateTermination_TakesNoItem_FallsOnTheOpenUnits_AsksTheConfirmation_AndRefusesItsDateAndRange_T528_KK4_KK5` |
| KK5 / "altı 422 sebebi" | ✓ | `PanelWithdrawalNotice_RefusesTheItemTheUnitsTheDateTheIbanAndTheOrderAnswer_BeforeTheWindow_T528_KK5`, `PanelWithdrawalNotice_OutsideTheWindow_WithoutTheSeparateBox_…_T528_KK5`, `PanelLateTermination_…_KK4_KK5`, `PanelLateTermination_OnTheTransferLine_…_T528_KK5_D1590` |
| KK6 / "iki hat" | ✓ | `PanelRemoval_AsksTheConfirmationOnTheCardLine_AndNotOnTheTransferLine_T528_KK6`, `PanelRemoval_RefusesTheUnitsTheDeliveredDigitalItemTheUnpaidOrderAndTheShippedOne_T528_KK6` |
| KK7 / "ayrı kayıt; ret koşulu" | ✓ | `ItemRecordsTests.PanelSteps.cs`: `PanelReceipt_IsARecordOfItsOwnEachTime_StartsNoRefund_AndRefusesTheDateAndQuantityBounds_T528_KK7`, `PanelRejection_IsOnlyOnAConditionalExemption_AndOnlyConfirmed_T528_KK7`; "mal dönmedi"yi yeniden açma `PanelNotReturnedClosure_…_T528_KK9_KK7`; fesih ve ayıp malı `ItemRecordsTests.TakenBack.cs` (aşağıda) |
| KK8 / "ikinci ekleme" | ✓ | `PanelRestock_AddsTheReceivedUnitsOnce_AndTheSecondAddition_IsRefused_T528_KK8`, `PanelRestock_OfAReceiptWhoseVariantIsGone_IsRefused_T528_KK8`; ret kaydı stoğa girmez (`RestockReturnAsync` yalnız `Returns`) |
| KK9 / "Z-42 sınırı" | ✓ | `PanelNotReturnedClosure_IsRefusedBeforeZ42_ClosesTheUnitsAfterIt_DecidesNoRefund_AndALaterReceiptReopens_T528_KK9_KK7` |
| KK10 / "durum geçmişi" | ✓ | `PanelClaimSteps_ResolveAndReopenTheSameClaim_WithTheStatusHistory_AndRefuseTheWrongStatusAndZ18_T528_KK10` |
| KK7/KK8 fesih malı (D-656) | ✓ | `TerminationGoods_AreReceivedPerArrival_UpToTheTerminatedUnits_WithoutARefund_T528_KK7_D656`, `PanelReceipt_OfATerminationsGoods_TakesTheTerminatedUnits_AndARejectionIsNotOpen_T528_KK7_D656` |
| KK7/KK8 ayıpta dönülen mal (D-911) | ✓ | `RescindedGoods_AreReceivedAgainstTheClaim_UpToTheRescindedUnits_AndTheReceiptIsOfferedToTheStockStep_T528_KK7_KK8_D911` |
| `09 §25` ilgili satırlar | ✓ | GET'in okuması ve POST'un kuralı aynı örnek (`ReturnStepFactsAsync` ↔ `ItemPeriodState.ReceivableUnits` aynı standing yardımcılarından; form varsayılanı `ReceivableUnitsOf` adımın sırasıyla); POST'lar `[GatedOperation]` (`OperationHandlerMarkTests`) |
| Güvenlik taraması | ✓ | yeni dış girdi: `Input.*` alanları bağlanır, sunucu kalem/kayıt sahipliğini siparişin kalemlerinden denetler (`TakenBackAwaitingAsync` kayıt + kalem eşleşmesi; yabancı kayıt 0 → `ConditionNotMet`); yeni sır, dosya, dış çağrı yok |
| (1) Bekçi/tarama istisnası geniş | ✓ | yeni tarama/istisna yok |
| (2) Kaynak taraması çağrı biçimi | ✓ | yeni tarama yok; işleyiciler `OperationRunner` üzerinden |
| (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok (`Return` bağlantıları ve dizinleri T320'den) |
| (4) Aynı sayıyı anan öteki yerler | ✓ | `ReceivableUnits` belgesi (`ItemPeriodStates.cs`) ve `ItemRecordRefusal.WithdrawalNotAwaiting` açıklaması fesih/ayıp payıyla güncellendi |
| (5) Belge kuralı değiştiren yeni 09 metni | ✓ | 09 değişmedi |

### Backlog (hedefi T528)

| Kalem | Sonuç | Kanıt / gerekçe |
|---|---|---|
| D-656 | ✅ | yukarıda (KK7/KK8 fesih malı) |
| D-911 | ✅ | yukarıda (ayıpta dönülen mal) |
| D-308 | ✅ | yönetici iptali UC-129 (KK1–KK3 testleri) |
| D-614 | T528 payı ✅ | `PanelItemSteps.LockAsync` sırası; T529/T530 payları kendi task'larında |
| D-774 | ✅ | `AuditOperation.RecordWithdrawalFromOtherChannel`; iz sayımı `OrderCorrectionTests.PanelItemSteps.cs` |
| D-776 | ✅ panel payı | KK4/KK5 cayma kaydı testleri |
| D-819 | ✅ kalan pay | UC-129, UC-130 ekranı + iz satırları |
| D-884 | T528 payı ✅ (2, 7, 8) | `CloseNotReturned`, başka kanal kayıtları; kalan T530, T531 |
| D-838 | 🟡 müşteri payı ✅ | `TheLegalInterestWarning_MarksTheCustomersCancellationAfterThePromise_NotTheOneBefore_T528_D838`; hizmet Z-39 → D-1591 (F5) |
| D-854 | ✅ fark gerekçeli | `02 §8.3.8` teyidi yalnız "müşteriyle anlaşıldı" iptaline bağlar; `06` eşleme `02 §10.4` m1 başka kanal cayma kaydında "teyit kaydedilmez"; ürün ikisinde de teyidi denetlemez — kural değişmediğinden karar kaydı gerekmez |
| D-902 | ✅ ⚠ | `02 §9.2` B-14 kalem başına; olduğu gibi kalır (yönetici kararı) |
| D-1302 | ✅ | ayıp satırı `id="ayip-talebi-…"` T527'den (`OrderDetail.cshtml`), `OrderDetailPageTests.Steps.cs` |
| D-1590 | ✅ | `…_T528_KK5_D1590` |
| D-1542 | not eklendi | `PanelItemSteps.LockAsync` üçüncü kopya; ortak kapı F5 (yönetici kararı, kod değişmedi) |
| D-1544 | not eklendi | T528 girdileri aynı sayfa yolunda; F5 |
| D-568 | → F5 | Contracts modül içine erişemez (`ModuleBoundaryTests` kırmızı oldu, geri alındı); davranış aynı |
| D-880, D-892 (ve D-513'ün panel payı) | → F5 | dönme/bedel indirimi düğmeleri UC-143; T528 kabul kriterlerinde yok |
| D-1195 | → F5 | ret kuralı T528'de; sebep cümlesi müşterinin kalem kaydı satırında (E-17), T528 KK'sı dışında |
| D-1197, D-1259 | → F5 | firma iptali ödenmiş dijital kaleme kapalı (`02 §7.1.6`; KK2 testi) → T528 kapanmış ödenmiş dijital kalem doğurmaz |
| D-1281 | → F5 ⚠ | T528 firma iptalinin yazım sırasına dokunmadı; aday sınanmadı |

## Test Sonuçları

- Unit TAM: 7715/7715 (iki parça: 5875 + 1840).
- Integration (koşulan sınıflar): `ItemRecordsTests`, `OrderCorrectionTests`, `LateDeliveryAndFirmCancellationTests`, `CustomerOperationsTests`, `OrderDetailPageTests` — tek çağrıda 270/270. **TAM: CI.**
- `dotnet format --include <dalın değişen .cs dosyaları>`: yalnız iki boşluk düzeltmesi (`PanelItemSteps.cs`).

## Altyapı Değişiklikleri

Yok.

## Commit & PR

Dal `task/T528-firma-iptali-iade-mali`; PR base `main`.

## Notlar — ⚠ kararlar

- ⚠ D-656/D-911 kapsam içi okundu (KK7 "her teslim almayı"); taken-back kaynağı enum yerine iki bağlantılı kayıt (`TakenBackSource`) — yeni enum `06` enum tablosu ve kilit dosyası isterdi, iki bağlantı `Return`'ün kendi alanlarıyla birebir.
- ⚠ Panel teslim alma sırası: önce en eski cayma beyanı, beyan yoksa en eski fesih, sonra ayıp talebi; ret yalnız beyanda (`02 §7.4.9`).
- ⚠ D-902 kalem başına B-14 olduğu gibi; D-1281 F5'e.
- ⚠ Devir notunun öngördüğü öteki yeni kalemler (çıkarma penceresi tutarı, cayma kaydı tek kalem) ayrıntısı devredilmediği için açılmadı; doğrulamada aranmalı.
- Makine: IntegrationTests ilk derlemesi 66 sn (artımlı), boş RAM ~22 GB.
