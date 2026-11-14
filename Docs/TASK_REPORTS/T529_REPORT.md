# T529 — Sipariş ayrıntısı: geri ödeme işlemleri (E-37)

**Faz:** F5 | **Durum:** ⏳ Doğrulama tur 3 ✗ FAIL — düzeltme bekliyor | **Tarih:** 2026-11-14

---

## Yapılan İşler

- **Sayfa:** E-37'ye geri ödeme satırları (`_OrderRefundLine.cshtml`: kalemin satırı kalem satırında, kalemsiz satır "Geri ödemeler" bölümünde — düzeltme 1; `_OrderDetailRefunds.cshtml`, `OrderDetail.Refunds.cs`): her geri ödeme satırı sunucunun çıkardığı tutar, yol ve IBAN'la; yalnız modülün açtığı adımların düğmesi çizilir. Yedi `[GatedOperation]` işleyici (`ProcessRefund`, `MarkBankTransferRefundFailed`, `ProcessAmountBasedRefund`, `RequestRefundIban`, `RetryCardRefund`, `SwitchRefundToBankTransfer`, `MarkCardRefundFailed`), işlemi `OperationRunner` açar.
- **Ordering:** `IPanelRefundSteps`/`PanelRefundSteps` (kapı: sipariş kilidi → sürüm → `OrderOperationCatalog`; kart adımları `IsCardRefundStepOpen`/`CheckCardRefundStep`), `ApplyFirmRefundSentAsync` (havalede Ö3–Ö5, B-8, IBAN silme), `FirmRefundAmounts` (tutarın tek kural evi: kayıt tutarı − kupon payı, kargo kuralı, K-1105 düşümü; saf `NotReturnedDeduction`).
- **Payment:** `StartFirmRefundAsync` (havale: `SentAt`; kart: geri ödeme + `CardRefund` işi aynı işlemde), `MarkCardRefundFailedByAdminAsync`, `ListPanelRefundsAsync`.
- **AfterSales:** `ListOwedRefundRecordsAsync` (panel ana sayfa sayacıyla tek kural `OwedWithoutRefundAsync`), `ListOpenNotReturnedClosuresAsync` (UC-133 kapanış seçimi).
- **UC-133 kapanış alanı (bu tur):** `ProcessAmountBasedRefundInput.NotReturnedClosureId` 07 girdi tablosuyla birebir eklendi; form, siparişin açık "mal dönmedi" kapanışlarını seçtirir; seçilen kapanış seçilen kalemin açık kapanışı değilse 422 `InvalidListValue`; seçilmezse kalemin son açık kapanışı bağlanır.
- **Testler:** `OrderDetailPageTests.Refunds.cs` (19 test yöntemi, düzeltme 2 sonrası), Unit `NotReturnedDeductionTests`, `OperationHandlerMarkTests` listesi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/` — `OrderDetail.Refunds.cs`, `_OrderDetailRefunds.cshtml`, `_OrderRefundLine.cshtml` (düzeltme 1), `OrderDetail.cshtml`, `OrderDetail.cshtml.cs`
- `src/Shopfolio.Web/Modules/Ordering/` — `PanelRefundSteps.cs`, `FirmRefundAmounts.cs`, `OrderTransitions.FirmRefunds.cs`, `OrderTransitions.RefundLine.cs` (düzeltme 2, D-1563), `OrderOperationCatalog.cs` (düzeltme 2, D-1563), `Contracts/IPanelRefundSteps.cs`, `Contracts/PanelRefundObstacle.cs`, `Contracts/IOrderTransitions*.cs`, `OrderingModule.cs`
- `src/Shopfolio.Web/Modules/Payment/` — `PaymentModule.FirmRefunds.cs`, `Contracts/FirmRefunds.cs`, `Contracts/IPaymentModule.cs`
- `src/Shopfolio.Web/Modules/AfterSales/` — `AfterSalesModule.RefundLine.cs` (düzeltme 1, `ReportTransferFailedAsync`; düzeltme 2, `ListRefundIbanFieldsAsync`), `Contracts/RefundLine.cs` (düzeltme 2), `AfterSalesModule.PendingWork.cs`, `AfterSalesModule.NotReturnedClosures.cs`, `Contracts/OwedRefundRecords.cs`, `Contracts/OpenNotReturnedClosure.cs`, `Contracts/IAfterSalesModule.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `ProcessRefund` doğuran kaydın anahtarı; onaysız 200 tutar + IBAN, onaylı 303; yabancı 404 | ✓ | kayıt bağı: `ProcessRefund_OnTheTransfer_RepeatsTheAmountAndTheIban_ThenSendsIt_MovesTheAxis_AndDeletesTheIban_KK1_KK2`; ayıp çözümü bağı: `..._D878`; kalemsiz doğmuş geri ödeme (düzeltme 2, bulgu 1–2): `PaymentRecordId` bağı `ProcessRefund_OnTheLatePaymentsRefundTurnedToTheTransfer_ByThePaymentRecord_RepeatsItsOwnIbanNotTheItems_ThenSendsIt_KK1_KK4`, `RefundId` bağı `ProcessRefund_OnAnItemlessAmountBasedRefundTurnedToTheTransfer_ByTheRefundId_RepeatsItsOwnIban_ThenSendsIt_KK1` |
| 2 | Havalede işlemle gönderim, IBAN son geri ödemeyle silinir; IBAN yoksa 422 | ✓ | aynı test + `ProcessRefund_OnTheTransferWithoutAnIban_Is422RefundIbanMissing_..._KK2` |
| 3 | Kart: sağlayıcı çağrılmaz; satır, iş, iz, sürüm tek işlemde | ✓ | `ProcessRefund_OnTheCard_WritesTheRefundItsJobTheAuditRowAndTheVersion_..._KK3` |
| 4 | İkinci gönderim ve açık pencerede IBAN düzeltmesi 409, güncel hâl | ✓ | `ProcessRefund_SentTwice_IsTheVersionsConflict_AndAnIbanCorrectedMeanwhile_Is409WithTheCurrentIban_KK4`; karşılaştırılan `ShownIban` iki IBAN'lı siparişte geri ödemenin kendi IBAN'ı (düzeltme 2: `..._ByThePaymentRecord_..._KK1_KK4`) |
| 5 | Tutar bazlı geri ödeme açık kapanışa bağlanır; sonraki teslim almalardan sırayla, tükenene kadar düşülür | ✓ | `ProcessAmountBasedRefund_IsBoundToTheChosenOpenClosure_AForeignOneIs422_AndTheLaterReceiptsRefundBearsTheBoundAmount_KK5`; `NotReturnedDeductionTests` (sıra, tükenme, kenarlar) |
| 6 | `MarkBankTransferRefundFailed` IBAN siler, alanı açar, "IBAN bekleniyor"; işlenmişte 422 | ✓ | kayıt geri ödemesi: `MarkBankTransferRefundFailed_DeletesTheIban_OpensTheField_AndOnAProcessedRefundIs422_KK6`; dosyadaki IBAN'a ödenen ayıp çözümü geri ödemesi (düzeltme 1, D-912): `MarkBankTransferRefundFailed_OnADefectRemedysRefundPaidToTheIbanOnFile_OnTheItemsRow_DeletesThatIban_AndOpensTheField_KK6_D912` (ekran düğmeyi çizer → POST 303, IBAN silindi, `TransferFailed` isteği, "IBAN bekleniyor"); iki IBAN'lı kart siparişinde havaleye dönmüş geç ödeme geri ödemesi (düzeltme 2, bulgu 2, D-1563): `MarkBankTransferRefundFailed_OnTheLatePaymentsRefundTurnedToTheTransfer_OnACardOrder_DeletesItsOwnIban_AndOpensTheItemlessRequest_KK6_D1563` (satır B'yi gösterir, POST B'yi siler, A kalır; kalemsiz `TransferFailed` isteği; satır "IBAN bekleniyor", ikinci POST 422; kart siparişinin doğmamış kaydında 422) |
| 7 | `ProcessAmountBasedRefund` boş/sıfır/aşan tutar ve IBAN'sız havale 422; kapalı kalem kapalı | ✓ | `ProcessAmountBasedRefund_AnEmptyZeroOrExceedingAmount_Is422_AndWritesNothing_KK7` (5 durum: boş, 0, 50,01, abc, 10,505 — üç ondalık düzeltme 1'de 500 → 422 `InvalidAmount`), `..._OnTheTransferWithoutAnIban_Is422_..._AndTheClosedItemStaysClosed_KK7`, `ProcessAmountBasedRefund_FormIsDrawnOnlyWhileProcessRefundIsOpen_AndAnItemNotOfTheOrderIs422WithTheItemListsMessage` |
| 8 | `RequestRefundIban` sebebi sunucu yazar; kalemsiz ya da havale dışı 422 | ✓ | `RequestRefundIban_WritesTheServersReasonAndB14_AndWithoutAnItemOrOnTheCardIs422_KK8` |
| 9 | `RetryCardRefund` onayla yeni istek; sonuç beklenirken / IBAN varken 422; ters ibraz uyarısı | ✓ | `RetryCardRefund_AsksTheConfirmation_TheChargebackWarningStaysOnTheLineOnly_ThenQueuesANewRequest_AndWhileTheResultIsAwaitedIs422_KK9` (düzeltme 1: U4 200 yanıtında bir kez, satırda; pencerede yok — `12 §12.13` K-1514 (3)) |
| 10 | `SwitchRefundToBankTransfer` CardRefundFailed istek + B-14, beklerken 422; `MarkCardRefundFailed` işaret, sorgu durur, bildirim yok, beklenmezken 422; geç sonuç işlenmez | ✓ | `SwitchRefundToBankTransfer_OpensTheCardRefundFailedRequestWithB14_..._KK10`, `MarkCardRefundFailed_TurnsTheAwaitedRequestFailed_SendsNoNotice_..._AndALateSuccessShowsTheWarning_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–10, KK1–KK10 | ✓ | yukarıdaki tablo — düzeltme 2'de birleşik dalda (main `866dc688`) bütün maddeler yeniden denetlendi; değişen yollar KK1, KK4, KK6 (aşağıda bağ başına ayrı test), öteki maddelerin kodu değişmedi, testleri birleşik ağaçta yeşil |
| KK1 "yedi bağdan biri" (düzeltme 2, bulgu 1) | ✓ | `ProcessRefundInput` yedi kayıt anahtarı + `RefundId` (`SelectedKey()`; ⚠ 3 → D-1552 `07` boşluğu); 404 yabancı anahtar. Bağ başına kanıt: `CancellationId` `..._KK1_KK2`; `DefectClaimId` `..._D878`; `PaymentRecordId` (kalemsiz doğmuş, havaleye dönmüş geç ödeme) `..._ByThePaymentRecord_..._KK1_KK4` — onaysız 200 tutar 20 + IBAN B, onaylı 303, gönderildi; `RefundId` (kalemsiz tutar bazlı) `..._ByTheRefundId_..._KK1` — 200 + 303. Kalemsiz yolda öğe kimliği doğmuş geri ödemenin kendisinden (`born is not null ? born.OrderItemId : owed!.OrderItemId`), artık 500 yok |
| KK1/KK6 IBAN tek kural (düzeltme 2, bulgu 2) | ✓ | `ListRefundIbanFieldsAsync` dosyadaki IBAN alanına onu getiren isteğin `RefundId`'sini yazar; `Lines.IbanOf` sırası: geri ödemenin kendi isteğinin IBAN'ı → kalemin (kayıtları ve kalemin istekleri; başka geri ödemenin kendi IBAN'ı değil) → dosyadaki ilk IBAN (`PendingTransferPayeesAsync` ile aynı sıra); kendi isteği açık geri ödeme IBAN bekler (`VAL-A912`). Satır, pencere (`ShownIban`), "işle" ve "gerçekleşmedi" aynı `IbanOf`'u okur; "gerçekleşmedi" POST'u satırın IBAN'ı yoksa 422. İki IBAN'lı testler: satır B (kalem satırı A), pencere B, işlemden sonra B silinir A kalır; "gerçekleşmedi" B'yi siler |
| KK2 havale hattı; IBAN yok | ✓ | KK1_KK2 + `..._OnTheTransferWithoutAnIban_Is422RefundIbanMissing_..._KK2` |
| KK3 kart hattı sırası | ✓ | `..._OnTheCard_..._KK3` (iş 1, `SentAt` boş, sürüm 2, iz 1) |
| KK4 "iki 409 sebebi" | ✓ | sürüm çakışması ve `RefundIbanChanged` (`ShownIban` gizli alanı; çakışmada karşılaştırılan güncel IBAN satırın `IbanOf`'u — düzeltme 2'de iki IBAN'lı siparişte geri ödemenin kendi IBAN'ı, `..._KK1_KK4`) |
| KK5 bağ; sıra ve tükenme | ✓ | `..._KK5` + `NotReturnedDeductionTests` |
| KK6 IBAN silinir; işlenmişte 422 (düzeltme 1 bulgu 1; düzeltme 2 bulgu 2, D-1563) | ✓ | ekran ve POST aynı kural: geri ödemenin kendi isteğinin IBAN'ı, kalemin, yoksa dosyadaki ilk IBAN (D-907; `AfterSalesModule.ReportTransferFailedAsync` kalemsizde geri ödemenin istekleri, kalemde kalemin kayıt/istekleri, yoksa dosyadaki ilk; ekran `Lines.IbanOf`); satırın IBAN'ı yoksa düğme yok, POST 422 (`PanelRefundSteps` artık aynı `IbanOf`'u sorar). Kart siparişinde yol doğmuş geri ödemenin kendi yolu (havaleye dönmüş), doğmamış kayıtta siparişin yolu → 422. Testler `..._KK6`, `..._KK6_D912`, `..._KK6_D1563` |
| KK7 "dört 422 sebebi" + iki ondalık (düzeltme 1, bulgu 3) | ✓ | boş, sıfır, aşan, okunamaz ve üç ondalık (theory) + IBAN'sız havale; `FieldRuleAttribute.HasAtMostDecimals(amount, AmountDecimals)` → 422 `InvalidAmount` |
| KK8 sebep sunucuda | ✓ | `..._KK8` (`Input.Reason=CardRefundFailed` gönderilse de `AdminRequest`) |
| KK9 iki ret, uyarı (düzeltme 1, bulgu 2) | ✓ | `RetryConfirm` `Warning` taşımaz; U4 yalnız satırda (`_OrderRefundLine.cshtml`); test pencerede yok, sayfada tek |
| KK10 iki işlem, 422'leri; geç sonuç | ✓ | `..._KK10` ×2, `..._D874` |
| K1 `RefundId` 8. bağ | ✓ | `07` girdi tablosu boşluğu `Docs/DEFERRED_BACKLOG.d/T529.md` D-1552 (F5 kapısı, `doc-drift`): doğuran kaydı olmayan doğmuş geri ödeme (kartta gerçekleşmeyip havaleye dönmüş tutar bazlı) yedi bağı taşımaz; davranış korunur |
| K2 tutar bazlı form; yabancı kalem metni | ✓ | form yalnız `IsOpen(ProcessRefund)` iken (`_OrderDetailRefunds.cshtml`); yabancı kalem `PanelRefundObstacle.ItemNotOfOrder` → 422 `InvalidListValue` "Siparişin kalemlerinden birini seçin ya da kalemi boş bırakın." (test `..._FormIsDrawnOnlyWhileProcessRefundIsOpen_...`) |
| K3 satır kalem satırında (`04` 9.9.6 (4), 9.9.21) | ✓ | `_OrderRefundLine.cshtml` kalem satırında bekleyen geri ödemenin altında (`RefundLinesOf(item)`); kalemsiz satır (geç gelen ödeme) bölümde (`OrderRefundLines`); test `..._KK6_D912` satırın `kalem-…` içinde olduğunu sınar |
| main birleştirmesi | ✓ | `OrderingModule.cs` üç kayıt (`IPanelRefundSteps`, `IPanelOrderCorrections`, `IPanelOrderUpkeep`); `OperationHandlerMarkTests` 24 işleyici ordinal sırada; `ApplicationEnums` T529 ve T534 girdileri birlikte; çakışma işareti yok (`git grep`) |
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
| Enum kilitleri | ✓ | `PanelRefundObstacle` kendi dosyasında, `ApplicationEnums.NotStored`'da (düzeltme 1: `ItemNotOfOrder` eklendi, saklanmaz) |
| Giriş modeli adları (`07 §5.3.1`) | ✓ | `ProcessRefundInput`, `ProcessAmountBasedRefundInput`, `RequestRefundIbanInput`, `RetryCardRefundInput` (UC-135–137 ortak) |
| Devredilen backlog kalemleri | ✓ | D-657 ✅, D-660 ✅, D-874 ✅ (`..._D874`), D-878 havale payı ✅ (`ProcessRefund_OnADefectRemedysBornTransferRefund_SendsIt_AndMovesTheAxis_D878`), D-898 ✅, D-1084 ✅; D-906 T529 payı kapandı (UC-133 tavanı), kalem tavanı payı → F5 gate; D-912 ✅ (düzeltme 1; `..._KK6_D912`); D-1382 → F5 gate; D-981 T529 payı yazıldı (`VAL-C090`/`C091` tam geri ödeme sınaması F5 kapısında); D-1563 ✅ (düzeltme 2; `..._KK6_D1563`) |
| `09 §25` / güvenlik | ✓ | panel oturumu ve sürüm; yabancı anahtar 404; IBAN iz satırına yazılmaz (KK1 testi); sebep sunucuda (K-1201); sayfa tutar hesaplamaz; yeni bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build (düzeltme 2, birleşik ağaç) | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Unit (düzeltme 2) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | TAM 7661/7661 |
| Biçim (düzeltme 2) | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <origin/main'e göre değişen 27 .cs dosyası>` | çıkış 0 |
| Integration (düzeltme 2) | ✓ (ilgili sınıflar) | tek çağrı `--filter-class` `OrderDetailPageTests`, `RefundAmountTests`, `PaymentTransitionsTests`, `StatusCorrectionTests`, `CardRefundJobTests`, `PanelHomeTests`, `CustomerOperationsTests`, `OrderOperationScreenTests`, `OrderPageTests` (son kod) + ayrı çağrı `ReturnRejectionAndIbanRequestTablesTests`, `ItemRecordsTests`, `OrderCorrectionTests`, `OrderOperationScreenTests`, `OrderPageTests`; Postgres 18 `shopfolio-t529-fix2-pg`, `127.0.0.1:57411`, `track_commit_timestamp=on` | 255/255 + 190/190. **TAM: CI** |
| Build (düzeltme 1, birleşik ağaç) | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Unit (düzeltme 1) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | TAM 7639/7639 |
| Integration (düzeltme 1) | ✓ (ilgili sınıflar) | tek çağrı, `--filter-class` `OrderDetailPageTests` (T531'in `Upkeep` testleri aynı sınıfta), `StatusCorrectionTests`, `CardRefundJobTests`, `PanelHomeTests`; Postgres 18 `shopfolio-t529-fix1-pg`, `127.0.0.1:57529`, `track_commit_timestamp=on` | 137/137. **TAM: CI** |
| Biçim (düzeltme 1) | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <origin/main'e göre değişen 25 .cs dosyası>` | çıkış 0 |
| Unit (yapım) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | TAM 7636/7636 (Consistency 295/295 dahil) |
| Integration (yapım) | ✓ (ilgili sınıflar) | `--filter-class`, Postgres 18 (`track_commit_timestamp=on`) | `OrderDetailPageTests` 40/40 (D-878 testi düzeltme sonrası ayrıca 1/1), `PanelHomeTests` yeşil; önceki tur: `StatusCorrectionTests` + `CardRefundJobTests` 98/98, `OrderListPageTests` + `ItemRecordsTests` 95/95. **TAM: CI** |

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

- D-906 kalem tavanı payı, D-1382 → F5 kapısı (gerekçeleri backlog satırlarında); D-1552 (`07` girdi tablosunda `RefundId`) → F5 kapısı.

## Notlar — ⚠ kararlar

1. UC-134 kalem okuması izlenen kayıt (`SingleOrDefaultAsync`, kilit altında): sürüm bekçisi (`OrderVersionGuard`, D-95) yeni `RefundIbanRequest`'in siparişini izlenen kalemden bulur.
2. UC-133 kapanış: seçilmezse kalemin son açık kapanışı bağlanır (04 7.2.13.2 "kapanış da seçilir ve geri ödeme ona bağlanır" — bağsız kalmasın); seçilen kapanış kalemin açık kapanışı değilse 422 `InvalidListValue`.
3. `ProcessRefundInput` sekizinci bağ olarak `RefundId` taşır: doğuran kaydı olmayan doğmuş geri ödeme (tutar bazlı). `07` girdi tablosu bunu anmıyor → D-1552 (düzeltme 1, K1).
4. UC-133 tavanı = ödenen − (gönderilmiş + başlatılmış) − doğmamış borçlu kayıt tutarları (D-906 payı).
5. Owed satırların tutarı kayıt başına `FirmRefundAmounts.OfAsync` ile okunur; bir siparişteki kayıt sayısı küçüktür, toplu okuma F5 kapısındaki performans denetimine bırakıldı.
6. ~~Geri ödeme satırları ayrı "Geri ödemeler" bölümünde~~ — düzeltme 1 (K3): kalemin satırı kalem satırında (`04` 9.9.6 (4), 9.9.21), kalemsiz satır bölümde.
7. IBAN önceliği: geri ödemenin → kalemin → siparişin dosyadaki IBAN'ı (düzeltme 2: 1. basamak artık çalışır — alan `RefundId` taşır; kendi isteği açık geri ödeme IBAN bekler).
8. İz adları: ProcessRefund/AmountBased → `RefundManually`, UC-132 → `ReportRefundTransferNotCompleted`, UC-134 → `OpenRefundIbanRequest`, UC-135 → `RetryCardRefund`, UC-136 → `RefundByBankTransfer`, UC-137 → `MarkCardRefundNotCompleted`.
9. `RefundIbanChanged` 409: onay penceresi `ShownIban` gizli alanını taşır; sürüm çakışmasında güncel IBAN farklıysa bu kod ve metin.
10. KK5 testi iş çalıştırıcısız açılır: kart işinin arka planda sonuçlanması sürümü testin adımları arasında artırıyordu.

---

## Doğrulama — tur 1 (2026-11-14): ✗ FAIL

- **Durum:** FAIL · **Bulgu:** 3 (S1 ×3, ORTA) · **K-notu:** 3 · **Düzeltme gerekli:** evet (aynı PR'da).
- **Kabul kriterleri (validator):** 1–5, 7–10 ✓ · 6 ✗ (bulgu 1). KK9 karşılandı; ancak onay penceresi U4'ü yineliyor (bulgu 2). KK7'nin dört 422'si var; `07 §2.9.4`'ün iki ondalık sınırı yok (bulgu 3).
- **Bulgu 1 (KK6, D-912):** doğmuş ayıp çözümü havale geri ödemesi siparişin dosyadaki IBAN'ına ödeniyor (`IbanOf` 3. basamak; D-907 kuralı); ekran "Havale gerçekleşmedi" düğmesini çiziyor, POST 422 dönüyor, IBAN silinmiyor (`ReportTransferFailedAsync` adayları yalnız kalemin kayıt/isteklerinden). Doğrulayıcının geçici testi: `buttonShown=True status=422 ibanLeft=True`. F3 kapısı D-912'yi T529'a ve KK6'ya bağlamıştı; kural D-907'de yazılı olduğundan "karar ister" gerekçesi geçerli değil.
- **Bulgu 2 (`12 §12.13` K-1514 (3), `VAL-B147`):** `RetryConfirm` pencereye `Warning = CardRefundNotRealizedWarning` koyuyor; 200 yanıtında U4 hem satırda hem pencerede. Kural: pencere uyarıyı yinelemez (tek istisna "ödendi"nin U5'i).
- **Bulgu 3 (`07 §2.9.4`; UC-133 alan mesajı):** `Input.Amount=10,505` onaylı gönderim 500 (`DisplayFormat.Amount` iki ondalık bekçisi); 422 `InvalidAmount` olmalı. Yazım yok (işlem geri alındı).
- **K-notları:** K1 `ProcessRefundInput`'un 8. bağı `RefundId` `07` girdi tablosunda yok (⚠ 3) · K2 tutar bazlı form yalnız `RequestRefundIban` açıkken de çiziliyor (POST 422), UC-133'te yabancı kalem "IBAN istenecek kalemi seçin." metniyle reddediliyor · K3 geri ödeme satırı `04` 9.9.6'nın kalem satırında değil ayrı bölümde (⚠ 6). Üçü düzeltme turunda bu PR'da kapatılır ya da gerekçeyle backlog parçasına yazılır.
- **⚠ kararların denetimi:** ⚠ 2 (kapanışın kendiliğinden bağlanması) `07` UC-133 "kapanış da seçilir ve geri ödeme ona bağlanır" ile uyumlu, düşüm kalem düzeyinde olduğundan tutarı değiştirmez · ⚠ 4 (UC-133 tavanı) güvenli yönde · ⚠ 7 IBAN önceliği D-907 kuralı · ⚠ 10 KK5'in iş çalıştırıcısız koşması davranışı gizlemiyor (düşüm bağlı geri ödemenin gönderim hâline bakmaz).
- **Testler:** Unit 7636/7636; Integration `OrderDetailPageTests`, `PanelHomeTests`, `RefundAmountTests`, `ItemRecordsTests`, `PaymentTransitionsTests`, `StatusCorrectionTests` 224/224 + `BranchFlowTests`, `CustomerOperationsTests`, `OrderCorrectionTests` 88/88 (TAM: CI); betik testleri 4/4; biçim temiz; derleme 0 uyarı.
- **CI:** PR #575 main'le çakışık (`OrderingModule.cs`, `OperationHandlerMarkTests.cs` — T531 #572); dalda CI koşusu yok.
- **`12`:** `VAL-A211`, `A212`, `A215`, `A216`, `A221`, `A222`, `A224` → PASS; `VAL-A230`, `VAL-B147` → FAIL.

## Düzeltme 1 (2026-11-14)

- **main birleştirmesi:** `git merge origin/main` (T531 #572). Kod çakışması iki dosyada, iki taraf korunarak çözüldü: `OrderingModule.cs` (`IPanelRefundSteps` ve `IPanelOrderUpkeep` kayıtları birlikte), `OperationHandlerMarkTests.cs` (T529'un yedi ve T531'in beş işleyicisi, ordinal sırada 18 ad). Kayıt belgeleri kendiliğinden birleşti (STATUS "Son güncelleme" satırı main'in). Birleşik ağaç derlendi, birleştirme commit'i hemen push'landı.
- **Bulgu 1 (KK6, VAL-A230, D-912):** `AfterSalesModule.ReportTransferFailedAsync` kaydın ve isteğin IBAN'ı yoksa siparişin dosyadaki ilk IBAN'ını (`CreatedAt, Id`; panel satırının gösterdiği, geri ödemenin gönderildiği IBAN) siler ve alanı açar — D-907 "kayıt, istek, yoksa dosyadaki IBAN", IBAN'ın ömrü (`PendingTransferPayeesAsync`) aynı sırayı sayar. Ekran düğmeyi çiziyorsa POST siler; IBAN yoksa düğme yok, POST 422. D-912 ✅. Test: `MarkBankTransferRefundFailed_OnADefectRemedysRefundPaidToTheIbanOnFile_OnTheItemsRow_DeletesThatIban_AndOpensTheField_KK6_D912`.
- **Bulgu 2 (KK9, VAL-B147):** `RetryConfirm` pencereye `Warning` koymaz; U4 yalnız satırda. KK9 testi pencerede U4 olmadığını, 200 yanıtında bir kez (satırda) bulunduğunu bekler (yeni adı `RetryCardRefund_AsksTheConfirmation_TheChargebackWarningStaysOnTheLineOnly_...`).
- **Bulgu 3 (UC-133):** tutar iki ondalıktan fazlaysa `FieldRuleAttribute.HasAtMostDecimals` ile 422 `InvalidAmount` alan mesajı; KK7 theory'sine `10,505`.
- **K1:** `RefundId` 8. bağın `07` boşluğu D-1552 (`Docs/DEFERRED_BACKLOG.d/T529.md`, F5 kapısı); gerekçe ⚠ 3.
- **K2:** tutar bazlı form yalnız `ProcessRefund` açıkken çizilir; UC-133'te siparişin olmayan kalemi `ItemNotOfOrder` → 422 `InvalidListValue` "Siparişin kalemlerinden birini seçin ya da kalemi boş bırakın.". Test `ProcessAmountBasedRefund_FormIsDrawnOnlyWhileProcessRefundIsOpen_AndAnItemNotOfTheOrderIs422WithTheItemListsMessage`.
- **K3:** geri ödeme satırı ve adımları yeni `_OrderRefundLine.cshtml` ile kalem satırında, bekleyen geri ödemenin altında (`04` 9.9.6 (4), 9.9.21); kalemsiz satır (geç gelen ödeme, K-1075) "Geri ödemeler" bölümünde, tutar bazlı ve IBAN isteği formlarıyla. Backlog'a atılmadı.
- **⚠ (düzeltme 1):** (a) "Dosyadaki IBAN" birden çoksa ekranın gösterdiği ilk IBAN silinir; kalemsiz geç ödeme geri ödemesinde ekran `IbanOf`'un 1. basamağı IBAN alanlarında `RefundId` taşımadığından dosyadaki ilk IBAN'ı gösterir, POST isteğin IBAN'ını siler — tek IBAN'lı siparişte aynıdır, birden çok IBAN'lı geç ödeme siparişi T529 kapsamında sınanmadı. (b) Testler `TAM: CI` — Integration'ın yalnız ilgili dört sınıfı yerelde koşuldu.

## Birleştirme (2026-11-14)

- `git merge origin/main` — main `866dc688` (T534 #574, T530 #573). Kod çakışmaları iki taraf korunarak çözüldü, davranış değişmedi: `OrderingModule.cs` (`IPanelRefundSteps` + `IPanelOrderCorrections` kayıtları), `Pages/Panel/OrderDetail.cshtml` (`_OrderDetailRefunds` + `_OrderDetailCorrections` partial satırları), `Pages/Panel/OrderDetail.cshtml.cs` (`LoadRefundLinesAsync` + `LoadCorrectionsAsync`), `OperationHandlerMarkTests.cs` (T529'un yedi ve T530'un altı işleyicisi, ordinal sırada 24 ad), `Support/ApplicationEnums.cs` (`PanelRefundObstacle` + T534'ün yedi enum'u). Kayıt belgesi: `IMPLEMENTATION_STATUS.md` tablosunda T529 satırı dalın, T530 satırı main'in.
- Sınama (birleşik ağaç): build Release `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes --include` çakışan dört .cs dosyası çıkış 0; Unit TAM 7661/7661; Integration `OrderDetailPageTests`, `StatusCorrectionTests`, `OrderCorrectionTests`, `CardRefundJobTests`, `PanelHomeTests` tek çağrıda 215/215 (Postgres 18, `track_commit_timestamp=on`). **TAM: CI**

## Doğrulama — tur 2 (2026-11-14): ✗ FAIL

- **Durum:** FAIL · **Bulgu:** 2 (S1 ×2; KRİTİK 1, ORTA 1) · **K-notu:** 1 (D-1563) · **Düzeltme gerekli:** evet. İkinci iterasyon FAIL → eskalasyon (`12 §8.5`, `INSTRUCTIONS.md` §3.3).
- **Tur 1 bulguları kapandı:** bulgu 1 (KK6, D-912: `..._KK6_D912` yeşil), bulgu 2 (KK9: pencerede U4 yok, sayfada tek), bulgu 3 (UC-133 `10,505` → 422 `InvalidAmount`); K1 (D-1552), K2 (form yalnız `ProcessRefund` açıkken; `ItemNotOfOrder`), K3 (satır kalem satırında). main birleşimi (T530, T531, T534): 24 işleyici, `OrderingModule` kayıtları, `LoadAsync` iki satır birlikte; ilgili sınıflar yeşil.
- **Bulgu 1 (KK1; `04` 9.9.21; KRİTİK):** kalemsiz doğmuş havale geri ödemesi — kartta gerçekleşmeyip havale yoluna dönmüş geç gelen ödeme geri ödemesi (`Input.PaymentRecordId`), aynı biçimde kalemsiz tutar bazlı (`Input.RefundId`) — satırında ekran "Geri ödemeyi işle"yi çiziyor; POST 500. `PanelRefundSteps.ProcessRefundAsync` satır 80: `born?.OrderItemId ?? owed!.OrderItemId` — `born` dolu ama `OrderItemId` boşken `owed!` (null) okunuyor → `NullReferenceException`. Geri ödeme panelden hiç işlenemiyor. Geçici doğrulayıcı testi: `lineHasProcess=True`, `ask=InternalServerError`, yığın satır 80.
- **Bulgu 2 (KK1, KK6; `02 §7.4.5`, D-907; ORTA):** `Lines.IbanOf`'un 1. basamağı (geri ödemenin kendi IBAN'ı) hiç eşleşmiyor: `ListRefundIbanFieldsAsync` IBAN taşıyan alanlarda `RefundId`'yi hep boş yazar. Siparişte birden çok IBAN varken (kalemin havale yolundaki IBAN'ı A, sonra ikinci ödemenin — K-1211 — geri ödemesine girilen B) geç ödeme satırı A'yı gösteriyor (onay penceresi de `IbanOf`'u yineler); IBAN'ın ömrü (`PendingTransferPayeesAsync`) ve "Havale gerçekleşmedi" POST'u (`ReportTransferFailedAsync`) geri ödemenin kendi isteğinin IBAN'ını (B) sayar. Geçici test: `lineHasA=True lineHasB=False`. Tur 1 bulgu 1 ile aynı türden GET/POST ayrışması (`09 §8.3` m.5); düzeltme 1'in ⚠ (a) notu doğrulandı.
- **K1 → D-1563:** kart siparişinde havaleye dönmüş geri ödemenin "Havale gerçekleşmedi"si kapalı (katalog `ReportTransferRefundFailed` ve `OrderTransitions.ReportTransferFailedAsync` siparişin yolunu `BankTransfer` ister); `VAL-A912` erişilemez. Koşul T529 diff'i dışında.
- **Öz denetim uyuşmazlığı:** KK6 satırı "ekran ve POST aynı kural … `IbanOf` 3. basamağı ile aynı sıra" diyor; kalemsiz geri ödemede 1. basamak ölü olduğundan ekran ve POST ayrışıyor (bulgu 2). KK1 "yedi bağdan biri" ✓ diyor; `PaymentRecordId` bağıyla işleme sınanmamış, 500 (bulgu 1). Öteki maddeler kanıtlı.
- **Testler:** build `-warnaserror` 0 uyarı; `dotnet format --include` dalın 25 .cs dosyası çıkış 0; Unit 7661/7661; betik 4 dosya; Integration `OrderDetailPageTests`, `PanelHomeTests`, `RefundAmountTests`, `ItemRecordsTests`, `PaymentTransitionsTests`, `StatusCorrectionTests` 254/254 + `BranchFlowTests`, `CustomerOperationsTests`, `OrderCorrectionTests`, `OrderListPageTests`, `OrderDisposalTests`, `DeliveryPeriodTests`, `AutomaticMomentsTests`, `SalesSummaryTests` 151/151. **TAM: CI**.
- **CI:** `43585484901` (head `9c0826e6`) bütün işler yeşil; PR `MERGEABLE`.
- **`12`:** `VAL-A230`, `VAL-B147` → PASS; öteki yedi satır PASS kalır. Bulgular satır dışı (KK1, `04` 9.9.21).

## Düzeltme 2 (2026-11-14)

- **Birleştirme:** `git merge origin/main` gerekmedi — main `866dc688` dalda; çakışma yok.
- **Bulgu 1 (KK1, KRİTİK):** `PanelRefundSteps.ProcessRefundAsync` öğe kimliğini doğmuş geri ödemeden alır (`born is not null ? born.OrderItemId : owed!.OrderItemId`); kalemsiz doğmuş havale geri ödemesi (`PaymentRecordId` ve `RefundId` bağları) onaysız 200 (tutar + IBAN), onaylı 303. İz satırı aynı öğe kimliğini yazar (kalemsizde `ItemId` yok).
- **Bulgu 2 (KK1, KK6, D-907, ORTA):** `AfterSalesModule.ListRefundIbanFieldsAsync` dosyadaki IBAN alanına onu getiren karşılanmış isteğin `RefundId`'sini yazar (beşinci sabit okuma); `Lines.IbanOf`: kendi isteğinin IBAN'ı → kalemin (başka geri ödemenin kendi IBAN'ı hariç) → dosyadaki ilk. Satır, pencere, `ShownIban` (KK4), "işle" ve "gerçekleşmedi" aynı evden okur. `PanelOrderReader.IbanOf`'un 1. basamağı da artık eşleşir (bekleyen geri ödeme özeti aynı IBAN'ı anar).
- **K1 → D-1563 ✅:** katalog `ReportTransferRefundFailed` `Open(HasRefundIban)` (`03` 1.11.33 siparişin yolunu sormaz); `OrderTransitions.ReportTransferFailedAsync` kart siparişinde adı geçen geri ödemenin ya da kalemin havale yolunda bekleyen geri ödemesi olmasını ister; `PanelRefundSteps.MarkBankTransferRefundFailedAsync` yolu doğmuş geri ödemeden, doğmamışta siparişten alır ve satırın IBAN'ı yoksa 422 (ekranla aynı karar). `VAL-A912` akışı testle sınandı (satır `12`'de doğrulayıcının).
- **Yeni testler (`OrderDetailPageTests.Refunds.cs`):** `ProcessRefund_OnTheLatePaymentsRefundTurnedToTheTransfer_ByThePaymentRecord_RepeatsItsOwnIbanNotTheItems_ThenSendsIt_KK1_KK4`, `ProcessRefund_OnAnItemlessAmountBasedRefundTurnedToTheTransfer_ByTheRefundId_RepeatsItsOwnIban_ThenSendsIt_KK1`, `MarkBankTransferRefundFailed_OnTheLatePaymentsRefundTurnedToTheTransfer_OnACardOrder_DeletesItsOwnIban_AndOpensTheItemlessRequest_KK6_D1563`; düzenek `SeedTurnedRefundsAsync` (kart siparişi; kalemin havaleye dönmüş ayıp çözümü geri ödemesi IBAN A, önce; kalemsiz geri ödeme IBAN B, sonra).
- **Öz denetim notu:** tur 2'de KK1 ve KK6 kanıtsızdı; bu kez her bağ (kayıt, ayıp çözümü, `PaymentRecordId`, `RefundId`) ve her IBAN durağı (satır, pencere, işle, gerçekleşmedi) ayrı testle kanıtlandı. Sık FAIL listesi (kalemsiz yolda null varsayımı, ekran/POST ayrışması, backlog kalemi) tarandı.
- **⚠ (düzeltme 2):** (a) Kendi isteği açık geri ödeme (havale gerçekleşmedi sonrası) dosyadaki başka IBAN'a düşmez, "IBAN bekleniyor" der (`VAL-A912`); IBAN'ın ömrü (`PendingTransferPayeesAsync`) böyle bir geri ödemede dosyadaki IBAN'ları kullanımda sayar — silmeyi geciktirir, ödeme yerini değiştirmez. (b) Kaydın kendi IBAN'ı ile geri ödemenin kendi isteğinin IBAN'ı aynı geri ödemede birlikte doğmaz (kendi istek yalnız kartta gerçekleşmeyen ve geç ödeme geri ödemelerinde; kart siparişinin kaydı IBAN taşımaz) — `IbanOf` kendi isteği önce sorar. (c) Testler `TAM: CI`.

## Doğrulama — tur 3 (2026-11-14): ✗ FAIL

- **Durum:** FAIL · **Bulgu:** 2 (S1 ×2; ORTA 2) · **K-notu:** yok · **Düzeltme gerekli:** evet. Üçüncü tur FAIL → eskalasyon (`12 §8.5`, `INSTRUCTIONS.md` §3.3).
- **Tur 2 bulguları kapandı:** bulgu 1 (KK1, kalemsiz doğmuş havale geri ödemesi 500 → 200/303: `..._ByThePaymentRecord_..._KK1_KK4`, `..._ByTheRefundId_..._KK1`), bulgu 2 (geri ödemenin kendi isteğinin IBAN'ı satırda, pencerede, `ShownIban`'da ve "gerçekleşmedi"de aynı: alan `RefundId` taşıyor), K1 / D-1563 (kart siparişinde havaleye dönmüş geri ödemenin "Havale gerçekleşmedi"si: `..._KK6_D1563`). Katalog genişlemesi (`ReportTransferRefundFailed` → `Open(HasRefundIban)`) yalnız `PanelRefundSteps`'te okunuyor; T512/T530/T531 sınıfları yeşil.
- **Bulgu 1 (KK6; `02 §7.4.5`, `04` 9.9.21, 6.3.9.18; ORTA):** "Havale gerçekleşmedi"den sonra kalemin satırı "IBAN bekleniyor"a dönmüyor; siparişte başka IBAN varsa satır onu gösterip "Geri ödemeyi işle"yi çiziyor (POST 200). `Lines.IbanOf` yalnız geri ödemenin kendi açık isteğinde bekliyor (⚠ (a)); kalemin açık isteğinde (kalemde açılan `TransferFailed` isteği `RefundId` taşımaz) 3. basamağa — dosyadaki ilk IBAN'a — düşüyor. Geçici doğrulayıcı testi (`SeedTurnedRefundsAsync`; kalemin ayıp çözümü geri ödemesinde "gerçekleşmedi"): `reported=303 itemIban=TR32…(B) process=True awaited=False ask=200 openItemRequests=1` — B kalemsiz geç ödeme geri ödemesinin kendi IBAN'ı; `IbanOf` belgesi "başka geri ödemenin kendi IBAN'ı değil" diyor. Aynı yol havale siparişinde iki kalemin kayıt IBAN'ında da işler (A silinir, kalem satırı öteki kalemin B'sini gösterir).
- **Bulgu 2 (KK2; `02 §7.4.5` K-356, K-1110, D-907; ORTA):** ekranın IBAN kuralı (`Lines.IbanOf`) IBAN'ın ömrüyle (`PendingTransferPayeesAsync`) aynı kural değil. (i) Kendi isteği açık bekleyen geri ödeme ekranda hiçbir IBAN'a ödenmiyor, ömür kuralı ise onu dosyadaki her IBAN'ın alıcısı sayıyor: A'nın son geri ödemesi gönderilince A silinmiyor, bekleyen geri ödeme kapanana kadar (K-525: müşteri IBAN girmezse süresiz) kalıyor. Geçici test (kalemsiz geri ödemede "gerçekleşmedi", sonra kalemin geri ödemesi A'ya işlenir): `itemlessIban=null itemSent=True left=[A]`. ⚠ (a)'nın "yalnız silmeyi geciktirir" notu belgeye uymuyor. (ii) Ömür kuralı kaydın geri ödemesini kaydın IBAN'ına öder; `IbanOf` kalemin ilk IBAN'ını alır — aynı kalemin iki iptali iki IBAN taşıyorsa ikinci iptalin satırı birincinin IBAN'ını gösterir (kod okuması). Öz denetimin "`PendingTransferPayeesAsync` ile aynı sıra" cümlesi tutmuyor.
- **Önerilen düzeltme:** ekranın ve ömrün IBAN'ı tek evden (geri ödeme başına ödenecek IBAN: kayıt → kendi isteği → kalemin isteği → dosya; açık istek — kendi ya da kalemin — "IBAN bekleniyor", ömür kuralında da alıcı değil); testler: kalemde "gerçekleşmedi" + başka IBAN → satır bekler, POST 422; bekleyen geri ödeme varken öteki IBAN son geri ödemesiyle silinir.
- **Testler:** build `-warnaserror` 0 uyarı; `dotnet format --include` dalın 27 .cs dosyası çıkış 0; Unit 7661/7661; betik 4 dosya; Integration `OrderDetailPageTests`, `PanelHomeTests`, `RefundAmountTests`, `ItemRecordsTests`, `PaymentTransitionsTests`, `StatusCorrectionTests` 257/257 + `BranchFlowTests`, `CustomerOperationsTests`, `OrderCorrectionTests`, `OrderListPageTests`, `OrderDisposalTests`, `DeliveryPeriodTests`, `AutomaticMomentsTests`, `SalesSummaryTests`, `OrderOperationScreenTests`, `LateDeliveryAndFirmCancellationTests`, `WithdrawalDeclarationTests` 203/203. **TAM: CI**.
- **CI:** `43590699726` (head `23743132`) bütün işler yeşil.
- **`12`:** dokuz satır PASS kalır; bulgular satır dışı (KK2, KK6 metni; `04` 9.9.21). `VAL-A912` T529'un satırı değil, BEKLEMEDE.
