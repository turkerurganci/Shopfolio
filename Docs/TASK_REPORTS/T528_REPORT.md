# T528 — Sipariş ayrıntısı: firma iptali, bildirim kaydı, kalem çıkarma, iade malı ve ayıp talebi (E-37)

**Faz:** F5 | **Durum:** Düzeltme 1 (1b–1e dahil) tamam — doğrulama tur 2 bekliyor | **Tarih:** 2026-11-15

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
| KK1 / kontrol "uyarılar, onay hâli" | ✓ (düzeltme 1) | `OrderCorrectionTests.PanelItemSteps.cs`: `PanelFirmCancellation_BeforeThePayment_TakesNoItem_AndCancelsTheWholeOrder_T528_KK1_KK3`, `…_AfterThePayment_StopsAtTheWindowUnconfirmed_ThenCancelsTheUnits_AndStartsTheCardRefund_T528_KK1_KK3`; kanuni faiz (U2) formda, Z-10/Z-39 aşılmışsa, onay hâlinde de yerinde: `OrderDetailPageTests.FirmCancellation_AfterTheShippingPromiseRanOut_ShowsTheLegalInterestWarningInTheForm_AlsoWithTheWindow_AndBeforeIt_Not_T528_KK1` (fiziksel Z-10, hizmet Z-39, süre içinde negatif); fesih kaydı formunda U2 + kalemler açık adetleriyle: `LateTerminationRecord_…_T528_KK1_KK5`; metnin harfi `TheItemStepsWarnings_AreTheFixedTextsOfK798_LetterForLetter_T528_KK1_KK5`; U1/U6a formda sebep seçimiyle, pencere yinelemez (düzeltme 1b): `FirmCancellation_TheReasonsWarning_StandsInTheFormBeforeAndWithTheWindow_U1ForOutOfStock_U6aForAgreed_AndTheWindowRepeatsNone_T528_KK1`; kalem kaydı satırında U2 (düzeltme 1d, `04` 9.9.6): `ItemRecords_OfACancellationAfterThePromise_AndOfATermination_CarryTheLegalInterestWarning_AndACancellationWithinThePromise_Not_T528_KK1_VAL_A228` |
| KK2 / "dört 422 sebebi" | ✓ | `PanelFirmCancellation_RefusesTheReasonTheItemAndTheUnits_AndAppliesNoPartOfTheRequest_T528_KK2` (sebep/açıklama, kalem yok, adet, iptale açık değil — ödenmiş dijital kalem `ConditionNotMet`) |
| KK3 / "iki hat ve ayırma" | ✓ | KK1 testleri + `PanelFirmCancellation_OnTheTransferLine_SaysSoInTheWindow_AndAsksTheIban_ForEveryChosenItem_InsteadOfARefund_T528_KK1_KK3` |
| KK4 / "cayma ve fesih alanları" | ✓ (düzeltme 1) | `PanelWithdrawalNotice_WritesTheItemAndItsUnits_AfterTheWindow_T528_KK4`, `PanelLateTermination_TakesNoItem_FallsOnTheOpenUnits_AsksTheConfirmation_AndRefusesItsDateAndRange_T528_KK4_KK5`; cayma kaydı birden çok kalem ve kalem başına adet (`IPanelItemSteps.RecordWithdrawalsAsync`, form kalem listesi): `OrderDetailPageTests.WithdrawalRecord_TakesManyItemsWithTheirUnits_AsksOnce_ThenWritesOneDeclarationPerItem_T528_KK4` |
| KK5 / "altı 422 sebebi" | ✓ (düzeltme 1) | `PanelWithdrawalNotice_RefusesTheItemTheUnitsTheDateTheIbanAndTheOrderAnswer_BeforeTheWindow_T528_KK5`, `PanelWithdrawalNotice_OutsideTheWindow_WithoutTheSeparateBox_…_T528_KK5`, `PanelLateTermination_…_KK4_KK5`, `PanelLateTermination_OnTheTransferLine_…_T528_KK5_D1590`; fesih formunda IBAN alanı, retten sonra tarih/IBAN/seçenek korunur: `LateTerminationRecord_…_T528_KK1_KK5`; U3a (+ bir yıl cümlesi) → U3b sırası, U3c kutu metni, işaret korunur: `WithdrawalRecord_BeyondTheYear_OnAnExemptItem_…_T528_KK5`; U6a K-798 (6) birebir (`OwnerCheckReminderText = AddressReminderText`), her kayıt formunun başında, IBAN alanı seçenekle açılır (düzeltme 1b): `NoticeRecords_OpenTheIbanFieldWithTheContactEmailOption_InOneMarkup_AndCarryTheOwnerCheckAtTheirHead_T528_KK5`; adet ve IBAN retlerinde U3a/U3b ve girilen değerler yerinde (düzeltme 1c): `WithdrawalRecord_KeepsTheOutsideWindowAndExemptionWarnings_AndTheSentValues_OnTheUnitsAndTheIbanRefusals_T528_KK5_VAL_B159` |
| KK6 / "iki hat" | ✓ | `PanelRemoval_AsksTheConfirmationOnTheCardLine_AndNotOnTheTransferLine_T528_KK6`, `PanelRemoval_RefusesTheUnitsTheDeliveredDigitalItemTheUnpaidOrderAndTheShippedOne_T528_KK6` |
| KK7 / "ayrı kayıt; ret koşulu" | ✓ | `ItemRecordsTests.PanelSteps.cs`: `PanelReceipt_IsARecordOfItsOwnEachTime_StartsNoRefund_AndRefusesTheDateAndQuantityBounds_T528_KK7`, `PanelRejection_IsOnlyOnAConditionalExemption_AndOnlyConfirmed_T528_KK7`; "mal dönmedi"yi yeniden açma `PanelNotReturnedClosure_…_T528_KK9_KK7`; fesih ve ayıp malı `ItemRecordsTests.TakenBack.cs` (aşağıda) |
| KK8 / "ikinci ekleme" | ✓ | `PanelRestock_AddsTheReceivedUnitsOnce_AndTheSecondAddition_IsRefused_T528_KK8`, `PanelRestock_OfAReceiptWhoseVariantIsGone_IsRefused_T528_KK8`; ret kaydı stoğa girmez (`RestockReturnAsync` yalnız `Returns`) |
| KK9 / "Z-42 sınırı" | ✓ | `PanelNotReturnedClosure_IsRefusedBeforeZ42_ClosesTheUnitsAfterIt_DecidesNoRefund_AndALaterReceiptReopens_T528_KK9_KK7` |
| KK10 / "durum geçmişi" | ✓ | `PanelClaimSteps_ResolveAndReopenTheSameClaim_WithTheStatusHistory_AndRefuseTheWrongStatusAndZ18_T528_KK10` |
| KK7/KK8 fesih malı (D-656) | ✓ | `TerminationGoods_AreReceivedPerArrival_UpToTheTerminatedUnits_WithoutARefund_T528_KK7_D656`, `PanelReceipt_OfATerminationsGoods_TakesTheTerminatedUnits_AndARejectionIsNotOpen_T528_KK7_D656` |
| KK7/KK8 ayıp talebinin malı (D-911) | ✓ (düzeltme 1) | sınır talebin ayıplı adedi − teslim alınan (K-808, `04` 7.2.12.2), geri ödemeden önce; panel formu açılır: `ClaimedGoods_AreReceivedAgainstTheClaim_BeforeAnyRefund_UpToTheClaimsDefectiveUnits_AndTheReceiptIsOfferedToTheStockStep_T528_KK7_KK8_D911` |
| D-1598 (2) teslim alma varsayılanı | ✓ (düzeltme 1) | `ArriveAsync` ve `ReceivableUnitsOf`: ilk beyanın `Awaiting + NotReturned`'ı (ret için `Awaiting`) |
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
| D-819 | ✅ kalan pay (tur 1'de yeniden açıldı, düzeltme 1'de kapandı) | UC-129, UC-130 ekranı + iz satırları + panel uyarıları (U2 firma iptali ve fesih kaydı formunda; düzeltme 1d: kalem kaydı satırında da — T368 metni "panel satırındaki kanuni faiz uyarısı (VAL-A228) sipariş ayrıntısı panelinindir (T528)", `…_T528_KK1_VAL_A228`) |
| D-1591 | ✅ (düzeltme 1, K3) | hizmet kaleminde Z-39: `DeliveryPeriodTests.CancellationAfterThePromise_…_KK2` + ekranda `FirmCancellation_…_T528_KK1` |
| D-884 | T528 payı ✅ (2, 7, 8) | `CloseNotReturned`, başka kanal kayıtları; kalan T530, T531 |
| D-838 | 🟡 müşteri payı ✅ | `TheLegalInterestWarning_MarksTheCustomersCancellationAfterThePromise_NotTheOneBefore_T528_D838`; hizmet Z-39 → D-1591 (düzeltme 1'de kapandı) |
| D-854 | ✅ fark gerekçeli | `02 §8.3.8` teyidi yalnız "müşteriyle anlaşıldı" iptaline bağlar; `06` eşleme `02 §10.4` m1 başka kanal cayma kaydında "teyit kaydedilmez"; ürün ikisinde de teyidi denetlemez — kural değişmediğinden karar kaydı gerekmez |
| D-902 | ✅ ⚠ | `02 §9.2` B-14 kalem başına; olduğu gibi kalır (yönetici kararı) |
| D-1302 | ✅ | ayıp satırı `id="ayip-talebi-…"` T527'den (`OrderDetail.cshtml`), `OrderDetailPageTests.Steps.cs` |
| D-1590 | ✅ | `…_T528_KK5_D1590` |
| D-1542 | not eklendi | `PanelItemSteps.LockAsync` üçüncü kopya; ortak kapı F5 (yönetici kararı, kod değişmedi) |
| D-1544 | not eklendi | T528 girdileri aynı sayfa yolunda; F5 |
| D-568 | → F5 | Contracts modül içine erişemez (`ModuleBoundaryTests` kırmızı oldu, geri alındı); davranış aynı |
| D-880, D-892 (ve D-513'ün panel payı) | → F5 (gerekçe düzeltme 1'de düzeltildi) | `07`'de ayıp çözümünün ayrı panel ucu yok (UC-143 indirme hakkını yenilemedir — `grep`); K-808: dönme UC-140 teslim alma (T528 ✓, bulgu 2 düzeltmesi) + UC-131 geri ödeme, bedel indirimi UC-133 — `RescindDefectClaimAsync`/`ReduceDefectClaimPriceAsync`'i o uçlara bağlamak ve iz satırı T528 kabul kriterlerinde yok |
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

Dal `task/T528-firma-iptali-iade-mali`; PR #578 (base `main`).

## Notlar — ⚠ kararlar

- ⚠ D-656/D-911 kapsam içi okundu (KK7 "her teslim almayı"); taken-back kaynağı enum yerine iki bağlantılı kayıt (`TakenBackSource`) — yeni enum `06` enum tablosu ve kilit dosyası isterdi, iki bağlantı `Return`'ün kendi alanlarıyla birebir.
- ⚠ Panel teslim alma sırası: önce en eski cayma beyanı, beyan yoksa en eski fesih, sonra ayıp talebi; ret yalnız beyanda (`02 §7.4.9`).
- ⚠ D-902 kalem başına B-14 olduğu gibi; D-1281 F5'e.
- ⚠ Devir notunun öngördüğü öteki yeni kalemler (çıkarma penceresi tutarı, cayma kaydı tek kalem) ayrıntısı devredilmediği için açılmadı; doğrulamada aranmalı.
- Makine: IntegrationTests ilk derlemesi 66 sn (artımlı), boş RAM ~22 GB.

## Doğrulama — tur 1 (2026-11-15): ✗ FAIL

- **Durum:** FAIL · **Bulgu:** 5 (S1 ×5; KRİTİK 2, ORTA 3) · **K-notu:** 3 (D-1598, D-1599; D-1591 düzeltme turuna) · **Düzeltme gerekli:** evet (aynı PR'da).
- **Kabul kriterleri (validator):** 2, 3, 5, 6, 8, 9, 10 ✓ · 1 ~ (bulgu 1) · 4 ~ (bulgu 5) · 7 ~ (bulgu 2).
- **Bulgu 1 (KK1; `07` UC-129 200 yanıtı, `04` 9.9.19, 9.9.20, §2.4.3; K-798 (2), K-706; `VAL-A228`; KRİTİK):** kanuni faiz uyarısı ("gecikme feshi — kanuni faiz yükümlülüğü") panelin hiçbir yerinde gösterilmiyor. Firma iptalinin onay hâli (`CancelConfirm` → `IrreversibleConfirmations.FirmCancellation`) Z-10 ya da Z-39 aşılmışken uyarı taşımıyor; başka kanal fesih kaydının formu uyarıyı ve feshin kapsadığı kalemleri açık adetleriyle listelemiyor (yalnız genel bir cümle). `OrderPeriods.IsLegalInterestWarningDueAsync` / `LegalInterestWarningsAsync` sayfada kullanılmıyor (`grep` boş). D-819'un kapanış notu "panel uyarıları"nı T528'in kalan payı sayıyordu; kapanış gerekçesi belgeyle tutarsız.
- **Bulgu 2 (KK7; K-808, `06 §4.26`, `07` UC-140, `04` 7.2.12.2, 9.9.30; `VAL-A218`; KRİTİK):** ayıp talebinin malını teslim almanın üst sınırı "talebin ayıplı adedinden henüz teslim alınmamış olan"dır ve sözleşmeden dönme teslim almayla başlar, geri ödeme ardından gelir (K-808). `AfterSalesModule.TakenBackAwaitingAsync` / `ClaimTakenBackStandingsAsync` sınırı ödeme modülündeki dönme iadesinden (`RescindedSharesAsync`) okur: dönme kaydı yoksa adım kapalı (`RescindedGoods_…_D911` testi bunu açıkça bekler: "Nothing rescinded yet: no goods come back on the claim"). Panelde dönme kaydını yazan uç yok (D-880 F5'e atıldı), bu yüzden ayıp talebinin malı panelden hiç teslim alınamaz. D-911'in kapanışı ve D-880/D-892'nin ileri atma gerekçesi ("dönme/bedel indirimi düğmeleri UC-143") tutarsız: `07`'de UC-143 indirme hakkını yenilemedir; `04` 9.9.30 dönmenin para yolunu teslim alma (9.9.27, UC-140) → geri ödeme diye T528'in ekranına bağlar.
- **Bulgu 3 (KK5; `04` 7.2.10.4, 9.9.20; `07` UC-130; D-1590; ORTA):** başka kanal fesih kaydı formunda IBAN alanı yok (`_OrderDetailItemSteps.cshtml` `record-late-termination-form` yalnız tarih ve "iletişim e-postasından geldi" kutusu). Sunucu `Input.Iban`'ı okuyor ve kaydediyor, ama yönetici ekrandan giremiyor; D-1590'ın "sayfa IBAN'ı taşır" kapanışı yalnız elle kurulmuş istekte doğru. Ret sonrası girilen tarih de formda korunmuyor.
- **Bulgu 4 (KK5; K-798 (3), (6), K-1563, K-1547, `04` 9.9.20; `VAL-A235`, `VAL-A236`, `VAL-A081`; ORTA):** başka kanal cayma kaydında pencere dışı, mutlak istisna ve "bir yıldan sonra" uyarılarının sabit metinleri yok; `WithdrawalNoticeWarnings`'in üç bayrağı ayrı gösterilmiyor, form tek genel cümleyi (`ExceptionUnacknowledgedText`) onay kutusunun etiketi yapıyor. Kutu metni K-798'deki "Uyarıyı değerlendirdim, kaydı yapıyorum." değil; bir yıldan sonraki kayıtta bildirimin süresinde gönderilmesi ve ispatın tüketicide olduğu cümlesi yok. Teyit hatırlatması (K-798 (6)) da sabit metinden ayrılmış ("Talep … işlemden önce" yerine "Bildirim … kaydetmeden önce"; T530'un `OrderDetail.Corrections.cs` sabiti birebir).
- **Bulgu 5 (KK4; `07` UC-130 "kalem başına iç anahtar ve adet", `04` 7.2.10.2 "en az bir kalem"; ORTA):** `RecordWithdrawal` birden çok kalemli isteği `ItemChoiceInvalid` 422 ile reddediyor, form tek kalem seçtiriyor. Kayıt kalem başına ayrıdır (`06 §4.25`), ama istek birden çok kalem taşıyabilir. Devir notunun planladığı "cayma kaydı tek kalem" kalemi açılmamış; daraltma kayıtsız.
- **K-notları:** K1 → D-1598 (kart hattı çıkarma penceresinde tutar yok, kod yorumu yanlış kaleme — D-1591 — bağlı; "mal dönmedi" ile kapatılmış adetlerde teslim alma varsayılanı 0) · K2 → D-1599 (sağlama basamağı tutmayan bildirim IBAN'ında `07`/`04` 422 ↔ `03 §8.4.8`/`VAL-A238` IBAN'sız kayıt; satır `BEKLEMEDE`) · K3: D-1591 (hizmet kaleminde Z-39 uyarısı testi) bulgu 1'in düzeltmesiyle aynı okumadır; düzeltme turunda kapatılması önerilir.
- **Uyumlu bulunanlar:** (a) D-568 (Contracts modül içine erişemez; davranış aynı), D-1195 (sebep cümlesi müşteri ekranında, E-17), D-1197/D-1259 (firma iptali ödenmiş dijital kaleme kapalı), D-1281 (yazım sırası T528 kriterinde yok) ileri atmaları gerekçeli; (b) `OrderDetailPageTests…_KK6` daraltması (`WithoutClaimStep`) yalnız T528'in çöz/yeniden aç formunu çıkarıyor, geri ödeme adımlarının denetimi kalıyor — kural kaybı yok; (c) `TakenBackSource` iki bağlantısı `06 §4.26`'nın `LateDeliveryTerminationId`/`DefectClaimId` alanlarıyla ve "yazımda tam biri" kuralıyla uyumlu; (d) firma iptalinde kart iadesi kendiliğinden, havalede kalem başına IBAN isteği (testli), fesih kaydında IBAN yolu sunucuda doğru (bulgu 3 yalnız form); (e) D-656 fesih malı yazıcısı `ReceivableUnits` ile aynı sayımı kullanıyor (fesih payı ✓; ayıp payı bulgu 2).
- **Testler:** derleme `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes --include` (dalın 27 .cs dosyası) çıkış 0; Unit TAM 7715/7715; betik testleri 4/4; Integration `ItemRecordsTests`, `LateDeliveryAndFirmCancellationTests`, `OrderCorrectionTests` 163/163 + `OrderDetailPageTests` 83/83 (Postgres 18, `track_commit_timestamp=on`). **TAM: CI.** On bir işleyicinin HTTP düzeyinde (200/303/422 eşlemesi) testi yok; kanıt modül testleri ve kod okuması — düzeltme turunda bulgu 1, 3, 4 için sayfa testi önerilir.
- **CI:** PR #578 run `43643222483` — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate ✓. Ana dal son 3 run (`43618088406`, `43618088489`, `43601265789`) ✓. main `99427473` ilerlemedi, birleştirme gerekmedi.
- **Güvenlik:** sır yok; POST'lar `[GatedOperation]` + panel oturumu; kayıt/kalem sahipliği siparişin kalemlerinden denetleniyor; iz satırı kod, tarih ve sayı taşıyor (IBAN yok); yeni bağımlılık yok — temiz.
- **Yapım raporu karşılaştırması:** öz denetim tablosu var ve kanıtlı; 5 uyuşmazlık — KK1 ✓ (validator ~, bulgu 1), KK7 ayıp malı ✓ (validator ~, bulgu 2), KK5 D-1590 ✓ (bulgu 3), KK5 uyarı metinleri (bulgu 4), KK4 (bulgu 5). Rapor iki planlı kalemin açılmadığını kendisi not ediyor; ikisi bulgu 5 ve D-1598'de.
- **`12`:** `VAL-C049`, `C050`, `C051`, `A209`, `A210`, `A217`, `A219`, `A223`, `A225`, `A226`, `A229`, `A231`, `A234`, `A237`, `C054` → PASS; `VAL-A081`, `A218`, `A228`, `A235`, `A236` → FAIL; `VAL-A213`, `A214` (T529 kapsamı), `A227` (D-1195, F5), `C052` (B-16 bildirimi T528 testinde sınanmadı), `C053` (tutar bazlı ödeme T529), `A232`, `A233` (teyidin kaydı ve süre hesabı sınanmadı), `A238` (D-1599) → BEKLEMEDE.

## Düzeltme 1 (2026-11-15)

- **Bulgu 1:** `OrderPeriods.LegalInterestDueItemsAsync` (aynı `DeliveryPeriodRules.IsLegalInterestWarningDue`) → `IPanelOrderReader.LegalInterestDueItemsAsync`; firma iptali formu Z-10/Z-39 aşılmış iptale açık kalemde U2'yi gösterir (onay hâlinde form yerinde kalır, pencere yinelemez); fesih kaydı formu U2'yi ve feshin kalemlerini açık adetleriyle salt okunur listeler (`TerminatedItems`). D-819 kapandı, D-1591 aynı okumayla kapandı (K3).
- **Bulgu 2:** ayıp talebinin malında sınır talebin ayıplı adedi (`DefectClaim.Quantity`) − teslim alınan (`TakenBackAwaitingAsync`, `ClaimTakenBackStandingsAsync`); dönme iadesine bağlılık kalktı, panel formu geri ödemeden önce açılır. Test "Nothing rescinded yet" beklentisi kurala göre değişti (`ClaimedGoods_…_D911`). D-880/D-892/D-911 gerekçeleri düzeltildi (`07` UC-143 = indirme hakkını yenileme, `grep` ile doğrulandı).
- **Bulgu 3:** fesih kaydı formunda "iletişim e-postasından geldi" seçeneğinin ardından IBAN alanı; ret sonrası tarih, seçenek ve IBAN korunur (`Termination`).
- **Bulgu 4:** U3a (bir yıldan sonra K-1563 cümlesiyle), U3b ayrı sabitler, ikisi birlikteyse bu sırayla; kutu metni U3c; U6a = `AddressReminderText` (K-798 (6) birebir). 422 mesajı (`ExceptionUnacknowledgedText`) yalnız ret mesajı.
- **Bulgu 5:** `IPanelItemSteps.RecordWithdrawalsAsync` — birden çok kalem, kalem başına adet; hepsi yazmadan önce denetlenir, kalem başına bir beyan ve iz satırı; tek kalemli yol aynı yönteme iner. Form kalem listesi + adet.
- **K1:** teslim alma varsayılanı `Awaiting + NotReturned` (D-1598 (2) ✓); çıkarma penceresi tutarı D-1598 (1) olarak F5'te (`04` 9.9.25 tutarı istemiyor). **K4:** dört sayfa testi + bir harf testi (`OrderDetailPageTests.ItemSteps.cs`).
- ~~⚠ Firma iptalinde U1 ve U6a hâlâ onay penceresinde~~ — **kapandı (düzeltme 1b):** U1/U6a formda, sebep seçimiyle; pencere yinelemez (aşağıda).
- **Testler:** restore ✓; build `-warnaserror` 0 uyarı (ilk derleme 141 sn); `dotnet format --verify-no-changes --include` (değişen 14 .cs) çıkış 0; Unit TAM 7715/7715 (2110 + 5605); Integration `ItemRecordsTests`, `OrderCorrectionTests`, `LateDeliveryAndFirmCancellationTests`, `OrderDetailPageTests` tek çağrı 251/251; `CustomerOperationsTests`, `OrderOperationsTests`, `WithdrawalRightTests`, `WithdrawalDeclarationTests`, `DeliveryPeriodTests` 67/67 (Postgres 18, `track_commit_timestamp=on`). **TAM: CI.**
- **Öz denetim:** KK1–KK10 ve backlog satırları yukarıdaki tabloda yeniden denetlendi; `sik-fail-nedenleri.md` okundu (kısmi kriter, backlog kapanış gerekçesi, uyarı metni harfi, sayfa testi satırları).

## Düzeltme 1b (devamları 1c, 1d, 1e — aynı tur, işçi süreyle bölündü; 2026-11-15)

- **1b (`9532fdb1`):** firma iptalinde U1 (stokta bulunamadı) ve U6a (müşteriyle anlaşıldı) formda, sebep seçimiyle görünür (`cancel-out-of-stock-warning`, `cancel-owner-check-reminder`; betiksiz yolda `admin.css` `:has` kuralları 1044–1050); onay penceresi U1/U2/U6a'yı yinelemez (K-1514 (2)). U1 K-798 metni; bildirim kayıtlarında IBAN alanı "iletişim e-postasından geldi" seçeneğiyle açılır (tek işaretleme); U6a her kayıt formunun başında. Düzeltme 1'in ⚠ satırı kapandı.
- **1c (`e2b5e846`):** cayma kaydının U3a/U3b uyarıları adet, IBAN ve kalem seçimi retlerinde de formda kalır, girilen değerler korunur (`VAL-B159`); çok kalemli kayıt testinde satır sırası belirsizliği giderildi.
- **1d (`3712e54d`):** öz denetimde bulunan eksik — D-819'un T368 metni panel satırındaki kanuni faiz uyarısını (`VAL-A228`) T528'e bırakıyordu, `OrderPeriods.LegalInterestWarningsAsync` sayfada okunmuyordu. `ItemRecordFact.RecordId` (kapanış kayıtlarında), `IPanelOrderReader.LegalInterestRecordsAsync`, `ItemRecordLabels.EntriesOf(..., warningOf:)`; `_ItemRecords.cshtml` satırında `item-records__warning` (`role="alert"`). Müşteri sayfası `warningOf` geçmez (`VAL-B160`). `04` 9.9.6, K-798 (2).
- **1e:** kod değişmedi; sınama, rapor, kayıt.
- **`12` tarama (T528 ekranının tarayıcı satırları; kanıt sunucu HTML'i — tarayıcı gözlemi doğrulamanın):**

| Satır | Sonuç | Kanıt |
|---|---|---|
| `VAL-B130`, `B131`, `B132`, `B154` | ✓ | U1/U6a formda sebep seçimiyle, kapsamdan bağımsız; öteki sebepte gizli (`hidden`, `:has` 1044–1050): `FirmCancellation_TheReasonsWarning_…_T528_KK1` |
| `VAL-B133` | ✓ | uyarılar formda, pencere açılınca yerinde; pencere U1/U2/U6a metnini yinelemez (`DoesNotContain`): aynı test |
| `VAL-B134` | ✓ | Z-10 (fiziksel) ve Z-39 (hizmet) aşılmışken formda U2, tutar/oran yok; süre içinde yok: `FirmCancellation_AfterTheShippingPromiseRanOut_…_T528_KK1` |
| `VAL-B135`, `B136` | ✓ (düzeltme 1d) | süre sonrası iptalin ve gecikme feshinin kalem kaydı satırında U2: `ItemRecords_OfACancellationAfterThePromise_…_T528_KK1_VAL_A228` |
| `VAL-B140` | ✓ | negatif kollar: süre içi iptalin formunda (`FirmCancellation_…_KK1`) ve kayıt satırında (`ItemRecords_…_VAL_A228`) U2 yok |
| `VAL-B137`, `B153` | ✓ | fesih kayıt formunda U2 + kalemler açık adetleriyle; U6a form başında, U2'den ayrı: `LateTerminationRecord_…_T528_KK1_KK5`, `NoticeRecords_…_T528_KK5` |
| `VAL-B141` | ✓ | pencere içi tarih, istisnasız kalem: kutu istenmeden onay penceresi doğrudan açılır (`ConfirmationRequired`): `PanelWithdrawalNotice_WritesTheItemAndItsUnits_AfterTheWindow_T528_KK4`; sayfada U3a/U3b'nin yokluğu ayrıca iddia edilmedi — tarayıcı gözlemi doğrulamanın |
| `VAL-B142`–`B145` | ✓ | U3a (bir yıldan sonra K-1563 cümlesiyle) → U3b sırası, ayrı öğeler (`withdrawal-outside-window-warning`, `withdrawal-exemption-warning`), tek U3c kutusu, işaret korunur: `WithdrawalRecord_BeyondTheYear_…_T528_KK5`; kutusuz 422, kutuyla pencere: `PanelWithdrawalNotice_OutsideTheWindow_WithoutTheSeparateBox_Is422BeforeTheWindow_AndWithItOpensTheWindow_T528_KK5` |
| `VAL-B146`, `B156` | ✓ | U6a cayma kaydı formunun başında, U3a/U3b'den ayrı: `NoticeRecords_…_T528_KK5` |
| `VAL-B159` | ✓ (düzeltme 1c) | retten sonra U3a/U3b ve girilen değerler yerinde: `WithdrawalRecord_KeepsTheOutsideWindowAndExemptionWarnings_…_T528_KK5_VAL_B159` |
| `VAL-B160` | ✓ | kayıt satırının U2'si yalnız panelde (`warningOf` müşteri sayfasında geçilmez) |
| `VAL-B161` | ✓ | U1, U2, U3a–U3c, U6a sabitleri K-798 ile harfi harfine: `TheItemStepsWarnings_AreTheFixedTextsOfK798_LetterForLetter_T528_KK1_KK5` |
| `VAL-B138`, `B139` | T528'e düşmez | "Kargoya ver"/"Tamamlandı" yanındaki U2 `PeriodIndicator.ShowsLegalInterest` (T527/T542; D-1151 T542 ⚠'si); T528 kabul kriterinde yok |

- **Testler (1e, head `3712e54d` kodu):** restore ✓ (4 sn); build `-warnaserror` 0 uyarı (210 sn; makine yük altında); Unit TAM 7715/7715 (Architecture 1815 + kalan 5605, ikisi CI süzgeciyle `--filter-not-trait "Consistency=true"`; ayrıca `Consistency=true` 295); `dotnet format --verify-no-changes --include` (dalın 36 .cs dosyası) çıkış 0. Integration: 1d'de `3712e54d` üzerinde `OrderDetailPageTests` `--filter-method "*T528*"` 9/9 (yeni test dahil; sınıfın tamamı makine yükü yüzünden 590 sn'ye sığmadı); 1e kod değiştirmedi, Integration koşulmadı. **TAM: CI.**
- **Öz denetim (1e):** KK1–KK10 yukarıdaki tabloda yeniden denetlendi (KK1 ve KK5 satırlarına 1b–1d testleri eklendi); backlog: D-819 kayıt satırı payı eklendi (`DEFERRED_BACKLOG.d/T528.md` notu); `sik-fail-nedenleri.md` okundu — tarayıcı satırları tarayıcısız PASS'a çekilmedi (✓ = sunucu HTML kanıtı), uyarı metni harfi testli, hedefi T528 olan kalem (D-819) karşılandı.

## Doğrulama — tur 2 (2026-11-15): ✓ PASS

- **Durum:** PASS · **Bulgu:** 0 · **K-notu:** 1 (D-1604) · **Düzeltme gerekli:** hayır. Tam yeniden doğrulama (para task'ı): bütün kabul kriterleri, KK1–KK10, `09 §25`, güvenlik taraması.
- **Kabul kriterleri (validator):** 1–10 ✓.
- **Tur 1 bulguları kapandı:** (1) firma iptali formunda Z-10/Z-39 aşılmışsa U2 (`LegalInterestDueItemsAsync`, `DeliveryPeriodRules.IsLegalInterestWarningDue` — kayıt satırıyla aynı kural), fesih formunda U2 ve kalemler açık adetleriyle, süre sonrası iptalin ve feshin kayıt satırında U2 (`LegalInterestRecordsAsync`, `ItemRecordFact.RecordId`); hizmet kaleminde Z-39 da (D-1591). (2) ayıp malının teslim alma sınırı talebin ayıplı adedi − teslim alınan (`TakenBackAwaitingAsync`, `ClaimTakenBackStandingsAsync`), dönme iadesinden bağımsız. (3) fesih formunda iletişim e-postası seçeneği + IBAN (`_OrderNoticeContactIban`), retten sonra değerler korunur. (4) U1, U2, U3a (+ K-1563 cümlesi), U3b, U3c, U6a sabitleri K-798 ile harfi harfine; U3a → U3b sırası; kutu etiketi U3c. (5) `RecordWithdrawalsAsync` çok kalem, kalem başına adet, hepsi yazmadan önce denetlenir, ret işlemi geri alır.
- **Özel denetim:** kilitsiz uyarı okuması (`WithdrawalNoticeWarningsAsync`) yazma yolunun kuralını (`OrderOperationCatalog.Check` + `WithdrawalNoticeRules.Warnings`) aynı olgularla okur; adet 1 ile süzer, böylece adedi ret sebebi olan kalemin uyarısı da formda kalır — yazma yolunun kabul ettiği kalemlerin üst kümesi, ret dışı bir kalemi katmaz. `admin.css` `:has` kuralları: sunucu gönderilen sebebe göre `hidden` çizer, CSS seçimle açar/kapatır; `:has` yoksa uyarı onay hâliyle formda gelir; IBAN alanı `:has` yoksa açık kalır (T539 yolu).
- **K-notu:** K1 → D-1604 (cayma kaydı formu kargoya verilmemiş fiziksel kalem için K-604 açıklamasını taşımıyor; sunucu yolu doğru, `VAL-C054` PASS).
- **Testler:** restore ✓; build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes --include` (dalın 36 .cs dosyası) çıkış 0; Unit 7715/7715; betik 4/4; Integration `ItemRecordsTests` + `LateDeliveryAndFirmCancellationTests` + `OrderCorrectionTests` 163/163, `OrderDetailPageTests` 92/92, `DeliveryPeriodTests` + `OrderOperationsTests` + `CustomerOperationsTests` + `RefundAmountTests` + `WithdrawalDeclarationTests` + `WithdrawalRightTests` + `OrderTransitionsTests` + `CustomerCancellationTests` 136/136 (Postgres 18, `track_commit_timestamp=on`). **TAM: CI.**
- **CI:** PR #578 run `43662141149` (head `bac948f2`) — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate ✓. Ana dal son 3 run (`43618088406`, `43618088489`, `43601265789`) ✓. main `99427473` (ilerlemedi; birleştirme gerekmedi).
- **Güvenlik:** sır yok; on POST `[GatedOperation]` + panel oturumu; yeni okuyucular sipariş kimliğiyle süzer (seçilen kalemler siparişin kalemleriyle kesişir); iz satırı IBAN taşımaz; yeni bağımlılık yok, göç yok.
- **Yapım raporu karşılaştırması:** tam uyumlu; öz denetim tablosu kanıtlı. Rapor tarayıcı satırlarını sunucu HTML kanıtıyla ✓ sayıyor; validator `12`'de yalnız task satırlarını (KP-47 ve kaynak grep'i) güncelledi: `VAL-B134` PASS (statik sunucu çıktısı), `VAL-B132` BEKLEMEDE (sebep değişince uyarının kalkması tarayıcı gözlemi ister).
- **`12`:** `VAL-A081`, `A218`, `A228`, `A235`, `A236`, `B134` → PASS; `VAL-A213`, `A214`, `A227`, `A232`, `A233`, `A238`, `C052`, `C053`, `B132` BEKLEMEDE kalır (tur 1 gerekçeleri; B132 tarayıcı). `12_matris_denetle.py --kapsam 1-77` Hata: 0.
