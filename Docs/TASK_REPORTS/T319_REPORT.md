# T319 — Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Geri alınamaz onay metinleri** (`Infrastructure/Operations/IrreversibleConfirmations.cs`, yeni): `IrreversibleAction` — `03 §1.6.1` tablosunun sekiz işlemi, tablo sırasıyla (kapalı liste; sistemin iptaldeki kart iadesi yok). `IrreversibleConfirmations` metinlerin tek evi: her işlemin K-797 sonuç cümlesi, ardından "Bu işlem geri alınamaz."; dijital kalemli siparişte havale "ödendi" penceresi K-798 (5) uyarısını ("…bu işaret sonradan düzeltilemez.") cümleden önce söyler. Sayı, tutar, tarih ve IBAN `DisplayFormat` ile yazılır.
- **Durum düzeltmesi** (`Modules/Ordering/OrderTransitions.Correction.cs`, yeni; `IOrderTransitions.CorrectTransitionAsync`): yöneticinin yeni geçişi; yanlış satır ve düzeltme satırı kayıtta kalır (çekirdeğin `CorrectAsync`'i: kilit, sürüm, düzeltilen satır hâlâ durum, çift kuralı, ardından bu dosyanın koşulu). Kapalı sebep listesi zorunlu, "Diğer" not taşır, başka sebep taşımaz; B-13 her düzeltmede.
  - S6 (teslim işareti): fiziksel kalemlerin teslim işareti ve tarihi boşalır, dijital kalem kendi işaretini korur; cayma/ayıp süreleri yeni tarihi bekler.
  - Firma iptali S3/S4/S9: yalnız ödeme Ödendi'deyken ve iade yolda değilken; ayırmalar `OrderCounters.RetakeAsync` ile yeniden alınır (önce `ShortVariantsAsync` ile okunur, yetmezse `StockShort` + kalem kimlikleri); iptalin gönderilmemiş geri ödemesi geçersizleşir (`VoidedAt`), açık IBAN istekleri kapanır; düzeltilmiş geçişe bağlı iptal açık adet hesabından düşer. Kapanışın ve müşterinin/sistemin iptali düzeltilmez.
  - S5/S8/S7: eksen bağı korunarak önceki duruma; arada doğan cayma beyanı ve ayıp talebi kalır; açık kalemi kalmayan sipariş aynı işlemde kapanış kuralından geçer; kargo bilgisi kalır.
  - Havale Ö1 (S1 ile birlikte): yalnız havale hattında, dijital kalemsiz siparişte ve teslim işareti, düzeltilmemiş kargo, geri ödeme, kalem kaydı (iptal, çıkarma, gecikme feshi, cayma) yokken; Z-8 düzeltme anından `PaymentWindows.StartAsync` ile yeniden başlar, B-3 yeni pencerede bir kez daha gider.
  - Ret sonucu `TransitionRefusal.CorrectionNotPossible` + `CorrectionObstacle` (`NotCorrectable`, `CardPayment`, `PaymentNotPaid`, `RefundPending`, `StockShort`, `ItemRecordExists`, `DigitalItem`; saklanmayan sonuç kodu, `ApplicationEnums.NotStored`).
- Modül yüzleri: `IAfterSalesModule` (`FindCancellationsOfTransitionAsync`, `HasClosingRecordAsync`, `CloseCancellationIbanRequestsAsync`; `ClosedQuantitiesAsync` düzeltilmiş iptali saymaz), `IPaymentModule` (`HasRefundAsync`, `IsCancellationRefundUnderwayAsync`, `VoidCancellationRefundsAsync`), `IOrderingModule.FindCorrectedTransitionRecordsAsync`. Hepsi çağıranın işlemine yazar, `SaveChanges` çağırmaz.
- Testler: birim `IrreversibleConfirmationTests` (7 test + 10 satırlık ihlal örneği Theory'si), Integration `StatusCorrectionTests` (34).
- Kayıtlar: D-275, D-456 kapandı; D-453 ve D-465'in T319 payı kapandı; yeni D-517, D-518, D-519 (`Docs/DEFERRED_BACKLOG.d/T319.md`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Operations/IrreversibleConfirmations.cs` (yeni)
- `src/Shopfolio.Web/Modules/Ordering/`: `OrderTransitions.Correction.cs` (yeni), `OrderTransitions.cs`, `OrderTransitions.Shipment.cs` (çekirdek düzeltme retlerinin açık yüz eşlemesi), `OrderCounters.cs` (`ShortVariantsAsync`), `OrderingModule.cs`, `Contracts/CorrectionObstacle.cs` (yeni), `Contracts/CorrectionRequests.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/IOrderingModule.cs`, `Contracts/ShipmentTransitionRequests.cs` (`OrderTransitionResult.Obstacle`), `Contracts/TransitionRefusal.cs` (`CorrectionNotPossible`)
- `src/Shopfolio.Web/Modules/AfterSales/`: `AfterSalesModule.cs`, `Contracts/IAfterSalesModule.cs`
- `src/Shopfolio.Web/Modules/Payment/`: `PaymentModule.cs`, `Contracts/IPaymentModule.cs`
- Testler: `tests/Shopfolio.UnitTests/Ordering/IrreversibleConfirmationTests.cs` (yeni), `Ordering/ShipmentTransitionContractTests.cs`, `Support/ApplicationEnums.cs`; `tests/Shopfolio.IntegrationTests/Ordering/StatusCorrectionTests.cs` (yeni), `Ordering/OrderingTestServices.cs`
- Kayıt: bu rapor, `Docs/IMPLEMENTATION_STATUS.md` (T319 satırı), `Docs/STATUS_CHANGELOG.d/T319.md`, `Docs/DEFERRED_BACKLOG.md` (D-275, D-453, D-456, D-465 yerinde), `Docs/DEFERRED_BACKLOG.d/T319.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz işlem onaydan önce tek cümle; dijital kalemli havale "ödendi" düzeltilemezliği söyler; "geri al" yok | ✓ | `IrreversibleConfirmations.cs` (sekiz `IrreversibleAction`, `DigitalItemPaidMarkWarning`); `TheCatalog_HasOneOperationPerRowOf03_1_6_1_InTheTablesOrder_KK1`, `EveryOperation_HasAWindow_ThatSaysItsResult_AndEndsWithTheIrreversibleSentence_KK1`, `TheWindows_WriteTheScreensValues_InTheDisplayFormat_KK1`, `TheTransferMark_OfAnOrderWithADigitalItem_WarnsFirstThatTheMarkCanNotBeCorrected_KK1`, `NoModuleSurface_OffersAnUndo_TheOneWayBackIsTheCorrectionOfATransition_KK1_KK9` |
| 2 | Sistemin iptalde başlattığı kart iadesi onay istemez | ✓ | `TheSystemsOwnCardRefund_AsksNoConfirmation_WhileTheFirmsIrreversibleRequestsCarryOne_KK2` (`CardRefundOutcome`, `StartLatePaymentRefundAsync` onay taşımaz; katalogda sistem işlemi yok) |
| 3 | Düzeltme yeni geçiş; yanlış geçiş ve düzeltme kayıtta kalır | ✓ | `OrderTransitions.Correction.cs:42` çekirdek `CorrectAsync`; `TheOtherReason_KeepsItsTrimmedNote_OnTheCorrectionRow_BesideTheWrongRow_AndB13Goes_KK3_KK4`, `ARowCorrectedOnce_IsNotCorrectedAgain_AndACorrectionRowHasNoCorrection_KK3`, `TheTransferMarksCorrection_TakesBothAxesBack_…_KK3_KK8` (iki düzeltme satırı + iki yanlış satır) |
| 4 | Kapalı listeden sebep; B-13 | ✓ | `OrderTransitions.Correction.cs:35-39`, `:101`; `ACorrection_WithoutAReasonFromTheClosedList_OrWithAMisplacedNote_IsRefused_AndWritesNothing_KK4` (sebepsiz, "Diğer" notsuz/boşluk, notlu başka sebep, tanımsız değer), `ACorrection_ByAnAdministratorWhoseDataIsMissing_IsRefused_KK4`; B-13 sayımı KK3/KK4 testlerinde |
| 5 | Teslim düzeltmesi önceki duruma; cayma/ayıp süresi yeni tarihe kadar işlemez | ✓ | `:88-97` (fiziksel kalemin `DeliveryMarkedAt`/`DeliveryDate` boşalır); `TheDeliveryMarksCorrection_TakesTheOrderBackToShipped_AndEmptiesThePhysicalItemsMarks_TheDigitalItemKeepsItsOwn_KK5` |
| 6 | Firma iptali yalnız Ödendi + iade beklenmiyorken; ayırmalar yeniden, yetmezse ret; kapanış düzeltilmez | ✓ | `CancellationIsCorrectableAsync` `:176-220`, yazım `:70-81`; `TheFirmsCancellation_OfAPaidOrder_IsCorrected_TheUnitsAreTakenAgain_TheRefundVoided_TheIbanRequestClosed_KK3_KK6` (havale, kart, başarısız kart isteği), `TheFirmsCancellation_WhoseRefundIsUnderway_IsNotCorrected_RefundPending_KK6` (4 durum), `TheFirmsCancellation_WhoseUnitsAreNoLongerLeft_IsNotCorrected_StockShort_NamingTheItem_KK6`, `TheFirmsCancellation_WhenThePaymentIsNoLongerPaid_IsNotCorrected_PaymentNotPaid_KK6`, `TheClosuresCancellation_AndTheCustomersCancellation_AreNotCorrected_KK6` |
| 7 | Yanlış Kargoya verildi / Teslim edilemedi eksen bağı korunarak geri; cayma ve ayıp kalır; açık kalemsiz sipariş kapanır | ✓ | `:83-86`, `:124-125`; `TheShipmentMarksCorrection_KeepsTheWithdrawalAndTheDefectClaim_AndClosesTheOrderWithNoOpenItem_InTheSameOperation_KK7`, `TheShipmentMarksCorrection_OfAnOrderWithAnOpenItem_GoesBackToPreparing_AndKeepsTheCarrierData_KK7`, `TheDeliveryFailedMarksCorrection_TakesTheOrderBackToShipped_KK7` |
| 8 | Havale "ödendi" düzeltmesi dört koşulla; Z-8 yeniden, B-3 bir kez daha | ✓ | `TransferMarkIsCorrectableAsync` `:138-169`, pencere `:62-68`; `TheTransferMarksCorrection_WithAnyOfItsObstacles_IsRefused_ItemRecordExists_AndWritesNothing_KK8` (7 engel), `TheTransferMarksCorrection_TakesBothAxesBack_StartsTheWindowAgain_AndTheReminderGoesASecondTime_KK3_KK8`, `ACorrectedShipmentMark_IsNoShipment_TheTransferMarkIsCorrectedAfterIt_KK8`, `TheTransferMark_OfAnOrderWithADigitalItem_IsNotCorrected_DigitalItem_KK8` |
| 9 | Kart hattında ödeme ekseni düzeltmesi yok; cayma düzeltilmez | ✓ | `:140-144`; `TheCardPayment_HasNoCorrection_CardPayment_KK9`, `AWithdrawal_IsNotATransition_AndIsNeverCorrected_KK9`; birim `NoModuleSurface_OffersAnUndo_…_KK1_KK9` (cayma düzeltme yöntemi yok) |

### Öz denetim (bitiş kapısı 2)

> Bütün maddeler bu turda (yapım işçisi 4) önceki işçilerin yazdıkları dahil yeniden denetlendi: 11 satır 2164–2188 (dokuz kriter, KK1–KK9, `09 §25`, güvenlik), `12`'nin ilgili satırları (`grep -n "03 §1\.6"`), sık FAIL listesi. Bu turdaki tek kod değişikliği null-forgiving `!` temizliğidir (davranış aynı); derleme, format, Unit, Consistency ve Integration `Ordering` + `Jobs` ad alanları yeniden koşuldu.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–9 | ✓ | Üstteki tablo |
| KK1 sekiz işlemde onay cümlesi, geri al yok; dijital kalemli havale onayı düzeltilemezliği söyler | ✓ | `IrreversibleConfirmationTests` KK1 testleri (4) + `NoModuleSurface_OffersAnUndo_…`; tarama ayrı ihlal örneğiyle sınanır: `TheUndoWordDetector_FindsAWholeWord_AndNotTheLettersInsideAnother` (6 ihlal, 4 temiz ad; "RefundOutcome" içindeki harfler yakalanmaz) |
| KK2 sistem iadesinde onay yok | ✓ | `TheSystemsOwnCardRefund_AsksNoConfirmation_…_KK2` |
| KK3 geçiş kaydında iki satır kalır | ✓ | `TheOtherReason_KeepsItsTrimmedNote_OnTheCorrectionRow_BesideTheWrongRow_…`, `ARowCorrectedOnce_IsNotCorrectedAgain_…_KK3`, Ö1+S1 düzeltmesinde iki düzeltme satırı |
| KK4 sebepsiz düzeltme reddedilir, B-13 yazılır | ✓ | `ACorrection_WithoutAReasonFromTheClosedList_…_KK4` (Theory), `…AdministratorWhoseDataIsMissing…_KK4`; B-13 `CountDispatchesAsync(…, DispatchType.B13)` |
| KK5 teslim düzeltmesi pencereyi öteler | ✓ | `TheDeliveryMarksCorrection_…_KK5` (teslim işareti boş → pencere sayımı başlamaz) |
| KK6 bekleyen iadede, yetmeyen stokta, kapanışla iptalde ret | ✓ | KK6 testleri (5 test, 9 satır) |
| KK7 kargo işareti geri döner; cayma ve ayıp kalır; açık kalemsiz sipariş kapanır | ✓ | KK7 testleri (3) |
| KK8 dört koşuldan biri bozukken ret; geçerse Z-8 yeniden, ikinci B-3 | ✓ | KK8 testleri (4; 7 engelli Theory) |
| KK9 kart ödemesi ve cayma düzeltilemez | ✓ | KK9 testleri (2) + birim |
| `12` VAL-C102 (bir önceki duruma, eksen bağı, iki işlem izde, B-13) | ✓ (birim katmanı) | KK3/KK5 testleri; işlem izi (`AuditOperation.CorrectTransition`) sayfa kapısında — UC-150 ucu T530 |
| `12` VAL-C103 / C104 / C105 / C106 | ✓ (birim katmanı) | sırasıyla KK5, KK6, KK7, KK8 testleri; uç yanıtı UC-150 (T530) |
| `12` VAL-B150, VAL-A713 (tarayıcı/uç: U5 penceresi, kart çıkarma onayı) | uygulanamaz | Sayfa task'ları (T530 ve çıkarma ucu); metin kaynağı bu task'ta (`IrreversibleConfirmations`), `12` `Durum` hücresi doğrulamanın işi |
| `09 §25` okunan bölümler | ✓ | 11 T319 bloğu; `03 §1.6` (1.6.1 tablosu, 1.6.2.1–1.6.2.4), §4.1.40; `02 §5.4`, §5.9, §10.4.6, §10.4.8; `05 MK-35`, §4.1; `06 §4.19`, §4.22, §4.31, §4.33; K-797, K-798 (5); `09 §9`, §24, §25 |
| `09 §25` yeni adlar belgeden; çelişki yok | ✓ | `CorrectionReason`, `TransitionCode.Correction`, `Refund.VoidedAt`, `RefundIbanRequest.ClosedAt` 06'dan; yeni sonuç kodu `CorrectionObstacle` saklanmaz (`ApplicationEnums.NotStored`) |
| `09 §25` diff yalnız görev | ✓ | Panel ucu, sayfa ve IBAN silme yazılmadı (⚠ ve D-517) |
| `09 §25` modül sınırı; arayüzde `SaveChanges` yok | ✓ | Yeni yüz yöntemleri çağıranın izleyicisine yazar; `AfterSalesModule` sipariş kayıtlarını `IOrderingModule.FindCorrectedTransitionRecordsAsync` ile sorar; Architecture testleri yeşil (Unit 4366) |
| `09 §25` tek işlev | ✓ | Pencere `PaymentWindows.StartAsync` (D-465), yeniden alma `OrderCounters.RetakeAsync` (D-453), kapanış `EvaluateClosureAsync`; ayrı hesap yok |
| `09 §25` `!` / `async void` / `dynamic` | ✓ | Bu turda diff'teki beş null-forgiving `!` kaldırıldı (`09 §3.1` madde 5): `CorrectTransitionAsync`'te `admin is not { } actor`, `OrderingModule.FindCorrectedTransitionRecordsAsync` ve `AfterSalesModule.AddSavedClosedAsync`'te `OfType<Guid>()`; `git diff origin/main -- src` içinde `!` kullanımı yok; Release derleme 0 uyarı |
| `09 §25` zaman yalnız `TimeProvider` | ✓ | `_time.GetUtcNow()` (`OrderTransitions.Correction.cs:64`, `AfterSalesModule.CloseCancellationIbanRequestsAsync`) |
| `09 §25` durum değişikliği yalnız geçiş birimiyle | ✓ | Durum yazımı çekirdek `CorrectAsync`; `TransitionUnitSourceScan` yeşil |
| `09 §25` iç yan etki aynı işlem, dış yan etki `Job`; ret geri alır | ✓ | B-13 `QueueNoticeAsync`; ret değeri `IsRejected` (koşucu geri alır); `…AndWritesNothing` testleri |
| `09 §25` işlem izi (`AuditOperation`) | uygulanamaz | Yönetici işleminin izi sayfa kapısında (UC-150, T530); birim iz yazmaz (T315, T317 ile aynı) |
| `09 §25` eşzamanlılık | ✓ | Çekirdeğin kilidi ve sürüm karşılaştırması; stok okuması kilit altında, `RetakeAsync`'in koşullu yazımı son söz (`:72-75`) |
| `09 §25` para hesabı | uygulanamaz | Tutar üretilmez; geri ödeme yalnız geçersizleşir |
| `09 §25` veri/göç | uygulanamaz | Göç yok; saklanan enum eklenmedi |
| Güvenlik taraması | ✓ | Sır yok; yetki: yalnız yönetici (`TryAdminActor`, eksik yönetici → `ActorNotAllowed`); girdi: sebep kapalı listede (`Enum.IsDefined`), not kırpılır; yeni dış bağımlılık yok |
| Sık FAIL — tarama istisnası geniş / ayrı ihlal örneği | ✓ | Geri al taraması PascalCase tam sözcük; ihlal örneği Theory'si |
| Sık FAIL — aynı sayıyı anan yerler | ✓ | Backlog sayacına ve "Son güncelleme"ye dokunulmadı (parça kuralı); `ShipmentTransitionContractTests` beklenen liste güncellendi |
| Sık FAIL — hedefi bu task olan backlog | ✓ | D-275, D-456 kapandı; D-453, D-465 T319 payı kapandı, hedeften çıktı |
| Sık FAIL — beklenen değer sabitten | ✓ | Pencere sonu `PeriodCounting.EndOf` + `TransferSchedule.ReminderAt`'ten; onay cümlesi testleri sabitlerden |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release --filter-not-trait "Consistency=true"` | 4366/4366 (bu tur) |
| Consistency | ✓ | `… --filter-trait "Consistency=true"` | 277/277 (bu tur, kayıt değişikliklerinden sonra) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj -c Release` (PostgreSQL 18, Docker) | Tam koşu 2637/2637, `StatusCorrectionTests` 34 (yapım işçisi 3, `fd127e3`); `!` temizliğinden sonra bu tur `--filter-namespace Shopfolio.IntegrationTests.Ordering` 219/219 ve `…Jobs` 77/77 |
| Build / format | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror`; `dotnet format Shopfolio.slnx --verify-no-changes` | 0 uyarı, temiz (bu tur) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-08; bağımsız doğrulama ajanı, `cf6ed041` + `origin/main` birleştirmesi) |
| Bulgu sayısı | 0 (S1/S2/S3 yok) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 bu PR'da kapatıldı; K2 → D-533 (`Docs/DEFERRED_BACKLOG.d/T319.md`) |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `scripts/git-hooks`; ana dal son üç tamamlanmış run: `42475025470` Publish success, `42474266979` CI cancelled (yeni push'un eşzamanlılık iptali — `ci.yml` `cancel-in-progress: true`; başarısız iş yok, Integration iptal edildi), `42474266982` Publish success. Dal CI'ı: `42474311101` (head `cf6ed041`) success — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate geçti.

**Kabul kriterleri (doğrulayıcı kanıtı):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `IrreversibleConfirmations.cs` sekiz `IrreversibleAction` (`03 §1.6.1` tablo sırası); cümleler K-797'nin metniyle, uyarı K-798 (5) ile elle karşılaştırıldı (birebir); "Bu işlem geri alınamaz." son satır; `IOrderTransitions` ve bütün `Contracts` arayüzlerinde geri al yöntemi yok (birim `NoModuleSurface_OffersAnUndo_…`) |
| 2 | ✓ | `CardRefundOutcome` ve `StartLatePaymentRefundAsync` onay alanı taşımaz; katalogda sistem işlemi yok (`TheSystemsOwnCardRefund_AsksNoConfirmation_…_KK2`) |
| 3 | ✓ | Çekirdek `CorrectAsync` yanlış satıra dokunmadan `TransitionCode.Correction` satırı ekler (`CorrectedTransitionRecordId`); Integration KK3 testleri |
| 4 | ✓ | `CorrectTransitionAsync`: sebepsiz, tanımsız, "Diğer" notsuz, notlu başka sebep → `InvalidInput`; B-13 `StatusCorrectedNotice` her düzeltmede |
| 5 | ✓ | S6 düzeltmesi fiziksel kalemlerin `DeliveryMarkedAt`/`DeliveryDate`'ini boşaltır; pencere saklanmaz, teslim tarihinden hesaplanır — tarih yokken işlemez |
| 6 | ✓ | `CancellationIsCorrectableAsync`: firma iptali (yönetici + iptal sebebi, kapanış değil), ödeme Ödendi, iade yolda değil (gönderilmiş / Sending / OutcomePending / Succeeded), `ShortVariantsAsync` + koşullu `RetakeAsync`; KK6 testleri (9 satır) |
| 7 | ✓ | S5/S7/S8 çekirdeğin çift kuralıyla geri; S5 düzeltmesinde `EvaluateClosureAsync` (açık kalemsiz sipariş S4 kapanışı), cayma ve ayıp kaydı kalır; KK7 testleri |
| 8 | ✓ | `TransferMarkIsCorrectableAsync` dört koşul + dijital kalem; `PaymentWindows.StartAsync` ile Z-8 ve hatırlatma anı yeniden; ikinci B-3 testte sayıldı |
| 9 | ✓ | Kart Ö1 → `CardPayment`; cayma kaydı geçiş değil → `NotCorrectable`; düzeltme yöntemi yalnız geçiş kaydı alır |

**Kontrol listesi:** KK1–KK9 ✓ (yukarıdaki kanıtlar); `09 §25` ✓ (modül sınırı, tek işlev, `TimeProvider`, durum yalnız geçiş biriminde, ret geri alır; işlem izi sayfa kapısında — UC-150 T530); güvenlik: sır yok, yalnız yönetici (`TryAdminActor`), sebep kapalı listede, yeni bağımlılık yok.

**`12` task satırları (`11 §7.1` T319 kaynağı KP-46):** `VAL-A207`, `A208`, `C039`…`C048` panel uç yanıtı ister (D-308 → T527); T319 uç yazmaz, kritere eşlenmez (T316, T318 emsali) — `Durum` hepsinde `BEKLEMEDE` → `BEKLEMEDE`. `12_matris_denetle.py --kapsam 1-77` hata 0.

**Test (doğrulayıcı, yerel):** restore + `build -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç projede temiz; Unit 4643/4643 (Consistency, Architecture dahil); betik testleri 4/4; Integration (PostgreSQL 18) `StatusCorrectionTests` 34/34, `Ordering` 219/219, `Disposal` 61/61, `Catalog` 72/72. `origin/main` (T342, T340) birleştirmesinden sonra build temiz, Unit 4699/4699, `Ordering` 219/219; `restore --locked-mode` 0.

**K-notları:**

| # | Not | Karar |
|---|---|---|
| K1 | Öz denetim tablosu `12` satırlarını `03 §1.6` grep'iyle bulmuş (`VAL-C102`…`C106`, `B150`, `A713` — §12.3, §12.13, §4.20 satırları, faz/gate okur); task'ın kaynak KP'si KP-46'nın satırlarını anmıyor | Bu PR'da kapatıldı: yukarıdaki `12` task satırları paragrafı KP-46 satırlarını kaydeder; anılan satırların `Durum`'u değişmedi |
| K2 | `IrreversibleConfirmations` üretimde çağrılmıyor; onay pencerelerini yazacak sayfa task'larının bu tek evden okuması kayıtta yok | D-533 → T527, T528, T529, T533 |

**Yapım raporu karşılaştırması:** kriter ve KK sonuçları tam uyumlu; öz denetim tablosu var ve kanıtlı. Tek uyuşmazlık K1 (`12` satır yöntemi).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T319-geri-alinamaz`
- **Commit:** `fd127e3` ve öncesi (kod), `6698b83` (kayıtlar, rapor, `!` temizliği)
- **PR:** #465
- **CI:** `42474311101` (head `cf6ed041`) success
- **Post-merge run'lar:** squash `02a79bd` (#465) — CI 42479203341 success, Publish 42479203249 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok (dış servis çağrısı yok; kart iadesinin gönderimi T405).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Düzeltmede müşterinin iptal için girdiği IBAN'ın silinmesi ve imha kaydı (`03 §1.6.2.2`, §4.1.40) | D-517 → T370 | IBAN'ı tek tek silen ve imha kaydını yazan tek işlev T370 k5'in; düzeltme ayrı yol yazmaz |
| 2 | Kart iadesi işleyicisi `VoidedAt` dolu iadeyi göndermez | D-518 → T405 | İşleyici henüz yok |
| 3 | `Cancellation.TransitionRecordId`'yi üretimde yazan yol | D-519 → T367, T368 | Kalem iptali akışları henüz yok; testler alanı tohumla yazar |

## Notlar

- **Working tree (Adım -1):** temiz (her işçi turunda)
- **Ana dal CI startup (Adım 0):** ilk işçi turunda denetlendi; bu tur yalnız kayıt
- **Repo memory (bitiş kapısı 9):** dokunulmadı (parça kuralı)
- **⚠ Yapım kararları (kolay ⚠, kurala en uygun seçenek):**
  1. S2, S10, S11 (kapanışın geçişleri) düzeltilmez (`NotCorrectable`) — 03 §1.6.2 yalnız 1.6.2.1–1.6.2.4'ü sayar.
  2. S5/S8 düzeltmesinde kargo bilgisi (şirket, takip numarası) kayıtta kalır (`02 §10.4.6`: dışarı giden geri alınmaz).
  3. Düzeltilmiş S5/S8 Ö1 düzeltmesi için kargo sayılmaz (yanlış işaret hiçbir şey göndermedi).
  4. Düzeltmeyle geri alınmış iptal kaydı Ö1 düzeltmesini yine engeller (kayıt kalır; K-703 "kalem kaydı yokken").
  5. "Geri ödeme yapılmamış" = geçersizleşmemiş (`VoidedAt` boş) herhangi bir geri ödeme yok.
  6. IBAN isteği kapatma kapsamı: geçersizleşen iadeye bağlı istekler ve kalemde çıkarma kaydı yoksa kalemin "iptal ya da çıkarma" isteği; çıkarması olan kalemin isteği açık kalır (`06 §4.31`).
  7. `IrreversibleConfirmations` henüz panelde kullanılmıyor: onay pencereleri sayfa task'larında (T530 vb.) bu tek evden okunur.
  8. `ShipmentTransitionContractTests`: çekirdeğin `CorrectionNotPossible` reddi artık açık yüzde kendi değeriyle (`TransitionRefusal.CorrectionNotPossible`) görünür; listeden çıkarıldı.
  9. `CorrectionObstacle` saklanmayan sonuç kodu olarak `ApplicationEnums.NotStored`'a eklendi (EnumLock).
  10. Düzeltmede IBAN silme D-517 ile T370'e bırakıldı (yukarıda).
- **Diğer:** Docker `t319-pg` silindi (`docker rm -f -v`).
