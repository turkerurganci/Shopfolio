# T529 — Sipariş ayrıntısı: geri ödeme işlemleri (E-37)

**Faz:** F5 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-14

---

## Yapılan İşler

- **Sayfa:** E-37'ye "Geri ödemeler" bölümü (`_OrderDetailRefunds.cshtml`, `OrderDetail.Refunds.cs`): her geri ödeme satırı sunucunun çıkardığı tutar, yol ve IBAN'la; yalnız modülün açtığı adımların düğmesi çizilir. Yedi `[GatedOperation]` işleyici (`ProcessRefund`, `MarkBankTransferRefundFailed`, `ProcessAmountBasedRefund`, `RequestRefundIban`, `RetryCardRefund`, `SwitchRefundToBankTransfer`, `MarkCardRefundFailed`), işlemi `OperationRunner` açar.
- **Ordering:** `IPanelRefundSteps`/`PanelRefundSteps` (kapı: sipariş kilidi → sürüm → `OrderOperationCatalog`; kart adımları `IsCardRefundStepOpen`/`CheckCardRefundStep`), `ApplyFirmRefundSentAsync` (havalede Ö3–Ö5, B-8, IBAN silme), `FirmRefundAmounts` (tutarın tek kural evi: kayıt tutarı − kupon payı, kargo kuralı, K-1105 düşümü; saf `NotReturnedDeduction`).
- **Payment:** `StartFirmRefundAsync` (havale: `SentAt`; kart: geri ödeme + `CardRefund` işi aynı işlemde), `MarkCardRefundFailedByAdminAsync`, `ListPanelRefundsAsync`.
- **AfterSales:** `ListOwedRefundRecordsAsync` (panel ana sayfa sayacıyla tek kural `OwedWithoutRefundAsync`), `ListOpenNotReturnedClosuresAsync` (UC-133 kapanış seçimi).
- **UC-133 kapanış alanı (bu tur):** `ProcessAmountBasedRefundInput.NotReturnedClosureId` 07 girdi tablosuyla birebir eklendi; form, siparişin açık "mal dönmedi" kapanışlarını seçtirir; seçilen kapanış seçilen kalemin açık kapanışı değilse 422 `InvalidListValue`; seçilmezse kalemin son açık kapanışı bağlanır.
- **Testler:** `OrderDetailPageTests.Refunds.cs` (15 test), Unit `NotReturnedDeductionTests`, `OperationHandlerMarkTests` listesi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/` — `OrderDetail.Refunds.cs`, `_OrderDetailRefunds.cshtml`, `OrderDetail.cshtml`, `OrderDetail.cshtml.cs`
- `src/Shopfolio.Web/Modules/Ordering/` — `PanelRefundSteps.cs`, `FirmRefundAmounts.cs`, `OrderTransitions.FirmRefunds.cs`, `OrderTransitions.RefundLine.cs`, `Contracts/IPanelRefundSteps.cs`, `Contracts/PanelRefundObstacle.cs`, `Contracts/IOrderTransitions*.cs`, `OrderingModule.cs`
- `src/Shopfolio.Web/Modules/Payment/` — `PaymentModule.FirmRefunds.cs`, `Contracts/FirmRefunds.cs`, `Contracts/IPaymentModule.cs`
- `src/Shopfolio.Web/Modules/AfterSales/` — `AfterSalesModule.PendingWork.cs`, `AfterSalesModule.NotReturnedClosures.cs`, `Contracts/OwedRefundRecords.cs`, `Contracts/OpenNotReturnedClosure.cs`, `Contracts/IAfterSalesModule.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `ProcessRefund` doğuran kaydın anahtarı; onaysız 200 tutar + IBAN, onaylı 303; yabancı 404 | ✓ | `ProcessRefund_OnTheTransfer_RepeatsTheAmountAndTheIban_ThenSendsIt_MovesTheAxis_AndDeletesTheIban_KK1_KK2` |
| 2 | Havalede işlemle gönderim, IBAN son geri ödemeyle silinir; IBAN yoksa 422 | ✓ | aynı test + `ProcessRefund_OnTheTransferWithoutAnIban_Is422RefundIbanMissing_..._KK2` |
| 3 | Kart: sağlayıcı çağrılmaz; satır, iş, iz, sürüm tek işlemde | ✓ | `ProcessRefund_OnTheCard_WritesTheRefundItsJobTheAuditRowAndTheVersion_..._KK3` |
| 4 | İkinci gönderim ve açık pencerede IBAN düzeltmesi 409, güncel hâl | ✓ | `ProcessRefund_SentTwice_IsTheVersionsConflict_AndAnIbanCorrectedMeanwhile_Is409WithTheCurrentIban_KK4` |
| 5 | Tutar bazlı geri ödeme açık kapanışa bağlanır; sonraki teslim almalardan sırayla, tükenene kadar düşülür | ✓ | `ProcessAmountBasedRefund_IsBoundToTheChosenOpenClosure_AForeignOneIs422_AndTheLaterReceiptsRefundBearsTheBoundAmount_KK5`; `NotReturnedDeductionTests` (sıra, tükenme, kenarlar) |
| 6 | `MarkBankTransferRefundFailed` IBAN siler, alanı açar, "IBAN bekleniyor"; işlenmişte 422 | ✓ | `MarkBankTransferRefundFailed_DeletesTheIban_OpensTheField_AndOnAProcessedRefundIs422_KK6` |
| 7 | `ProcessAmountBasedRefund` boş/sıfır/aşan tutar ve IBAN'sız havale 422; kapalı kalem kapalı | ✓ | `ProcessAmountBasedRefund_AnEmptyZeroOrExceedingAmount_Is422_AndWritesNothing_KK7` (4 durum), `..._OnTheTransferWithoutAnIban_Is422_..._AndTheClosedItemStaysClosed_KK7` |
| 8 | `RequestRefundIban` sebebi sunucu yazar; kalemsiz ya da havale dışı 422 | ✓ | `RequestRefundIban_WritesTheServersReasonAndB14_AndWithoutAnItemOrOnTheCardIs422_KK8` |
| 9 | `RetryCardRefund` onayla yeni istek; sonuç beklenirken / IBAN varken 422; ters ibraz uyarısı | ✓ | `RetryCardRefund_AsksTheConfirmationWithTheChargebackWarning_ThenQueuesANewRequest_AndWhileTheResultIsAwaitedIs422_KK9` |
| 10 | `SwitchRefundToBankTransfer` CardRefundFailed istek + B-14, beklerken 422; `MarkCardRefundFailed` işaret, sorgu durur, bildirim yok, beklenmezken 422; geç sonuç işlenmez | ✓ | `SwitchRefundToBankTransfer_OpensTheCardRefundFailedRequestWithB14_..._KK10`, `MarkCardRefundFailed_TurnsTheAwaitedRequestFailed_SendsNoNotice_..._AndALateSuccessShowsTheWarning_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–10, KK1–KK10 | ✓ | yukarıdaki tablo (önceki işçilerin yazdıkları dahil yeniden denetlendi) |
| KK1 "yedi bağdan biri" | ✓ | `ProcessRefundInput` yedi kayıt anahtarı + `RefundId` (`SelectedKey()`; ⚠ 3); 404 yabancı anahtar |
| KK4 "iki 409 sebebi" | ✓ | sürüm çakışması ve `RefundIbanChanged` (`ShownIban` gizli alanı) |
| KK7 "dört 422 sebebi" | ✓ | boş, sıfır, aşan (theory) + IBAN'sız havale |
| (1) Bekçi/tarama istisnası geniş | ✓ | yeni tarama/istisna yok; `OperationHandlerMarkTests` listesine yedi işleyici adı eklendi |
| (2) Kaynak taraması çağrı biçimi | ✓ | yeni tarama yok; yedi işleyici `RunRefundStepAsync` → `OperationRunner` (`OperationSourceScan` yeşil) |
| (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok |
| (4) Aynı sayıyı anan öteki yerler | ✓ | owed kuralı tek evde (`OwedWithoutRefundAsync`; panel ana sayfa sayacı aynı evi okur — `PanelHomeTests` yeşil); tutar tek evde (`FirmRefundAmounts`; `RefundLine.cs` kargo yardımcısı ona bağlandı) |
| (5) Belge kuralını değiştiren 09 metni | ✓ | `09` değişmedi |
| 07 girdi tablosu alan listesi birebir (sık FAIL "alan atıldı") | ✓ | `ProcessAmountBasedRefundInput`: `Amount`, `OrderItemId`, `NotReturnedClosureId`, `Version`, `Confirmed` |
| Para: tutar tek kural evinden | ✓ | ekran satırı ve POST `FirmRefundAmounts.OfAsync`'i okur; UC-133 tavanı sipariş kilidi altında |
| Eşzamanlılık / kilit | ✓ | `GateAsync` sipariş satır kilidi + sürüm; UC-134 kalemi izlenen kayıt (⚠ 1) |
| Ret kaydı kodu (`09 §8.2` m.3) | ✓ | adlı retler `RefuseAsync` ile kodlu (`RefundIbanMissing`, `AmountBasedRefundExceeded`, `FieldRequired`, `InvalidAmount`, `InvalidListValue`) |
| Geri alınamaz onay metni (D-533) | ✓ | onay pencereleri `IrreversibleConfirmations.RefundProcessing` |
| Döngüde sorgu (`09 §20` m.4) | ✓ (⚠ 5) | owed tutarlar sipariş başına kayıt sayısı kadar okunur (bir siparişte birkaç kayıt) |
| Enum kilitleri | ✓ | `PanelRefundObstacle` kendi dosyasında, `ApplicationEnums.NotStored`'da |
| Giriş modeli adları (`07 §5.3.1`) | ✓ | `ProcessRefundInput`, `ProcessAmountBasedRefundInput`, `RequestRefundIbanInput`, `RetryCardRefundInput` (UC-135–137 ortak) |
| Devredilen backlog kalemleri | ✓ | D-657 ✅, D-660 ✅, D-874 ✅ (`..._D874`), D-878 havale payı ✅ (`ProcessRefund_OnADefectRemedysBornTransferRefund_SendsIt_AndMovesTheAxis_D878`), D-898 ✅, D-1084 ✅; D-906 T529 payı kapandı (UC-133 tavanı), kalem tavanı payı → F5 gate; D-912 → F5 gate; D-1382 → F5 gate; D-981 T529 payı yazıldı (`VAL-C090`/`C091` tam geri ödeme sınaması F5 kapısında) |
| `09 §25` / güvenlik | ✓ | panel oturumu ve sürüm; yabancı anahtar 404; IBAN iz satırına yazılmaz (KK1 testi); sebep sunucuda (K-1201); sayfa tutar hesaplamaz; yeni bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | TAM 7636/7636 (Consistency 295/295 dahil) |
| Integration | ✓ (ilgili sınıflar) | `--filter-class`, Postgres 18 (`track_commit_timestamp=on`) | `OrderDetailPageTests` 40/40 (D-878 testi düzeltme sonrası ayrıca 1/1), `PanelHomeTests` yeşil; önceki tur: `StatusCorrectionTests` + `CardRefundJobTests` 98/98, `OrderListPageTests` + `ItemRecordsTests` 95/95. **TAM: CI** |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <dalın .cs dosyaları>` | temiz |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T529-geri-odeme-islemleri`
- **Commit:** "T529: Sipariş ayrıntısı: geri ödeme işlemleri (E-37)" (dalın son commit'i; öncekiler WIP)
- **PR:** #575
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | ✓ |

## Known Limitations / Follow-up

- D-906 kalem tavanı payı, D-912, D-1382 → F5 kapısı (gerekçeleri backlog satırlarında).

## Notlar — ⚠ kararlar

1. UC-134 kalem okuması izlenen kayıt (`SingleOrDefaultAsync`, kilit altında): sürüm bekçisi (`OrderVersionGuard`, D-95) yeni `RefundIbanRequest`'in siparişini izlenen kalemden bulur.
2. UC-133 kapanış: seçilmezse kalemin son açık kapanışı bağlanır (04 7.2.13.2 "kapanış da seçilir ve geri ödeme ona bağlanır" — bağsız kalmasın); seçilen kapanış kalemin açık kapanışı değilse 422 `InvalidListValue`.
3. `ProcessRefundInput` sekizinci bağ olarak `RefundId` taşır: doğuran kaydı olmayan doğmuş geri ödeme (tutar bazlı).
4. UC-133 tavanı = ödenen − (gönderilmiş + başlatılmış) − doğmamış borçlu kayıt tutarları (D-906 payı).
5. Owed satırların tutarı kayıt başına `FirmRefundAmounts.OfAsync` ile okunur; bir siparişteki kayıt sayısı küçüktür, toplu okuma F5 kapısındaki performans denetimine bırakıldı.
6. Geri ödeme satırları kalem satırının yanında değil, ayrı "Geri ödemeler" bölümünde (ortak dosyaya az dokunmak için).
7. IBAN önceliği: geri ödemenin → kalemin → siparişin dosyadaki IBAN'ı.
8. İz adları: ProcessRefund/AmountBased → `RefundManually`, UC-132 → `ReportRefundTransferNotCompleted`, UC-134 → `OpenRefundIbanRequest`, UC-135 → `RetryCardRefund`, UC-136 → `RefundByBankTransfer`, UC-137 → `MarkCardRefundNotCompleted`.
9. `RefundIbanChanged` 409: onay penceresi `ShownIban` gizli alanını taşır; sürüm çakışmasında güncel IBAN farklıysa bu kod ve metin.
10. KK5 testi iş çalıştırıcısız açılır: kart işinin arka planda sonuçlanması sürümü testin adımları arasında artırıyordu.
