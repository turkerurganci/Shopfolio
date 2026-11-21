# T604 (U) — F5 kapısından kalan ürün kodu kalemleri

**Faz:** F6 | **Durum:** ⏳ Devam ediyor (UE dilimleri UE1–UE5 kapandı; UH dalı ayrı) | **Tarih:** 2026-11-21

T604 iki parça ve iki PR'da yürür (11 T604 KK1): **U** ürün kodu (bu rapor, dal `task/T604-urun-kodu`), **A** test aracı (dal `task/T604-test-araci`, PR #594). U parçası dilimlerle aynı dalda ilerler; her dilim bu dosyaya kendi başlığıyla ekler.

---

## Dilim UE1 — iade, geri ödeme, IBAN (para)

### Yapılan İşler

| Kalem | Değişiklik |
|---|---|
| D-568 | Yoldaki adet formülü tek işlevde: `Contracts/ItemUnitFacts.cs` `ReturnUnits.AwaitingReceipt` (sıfır tabanı); `ItemQuantities.AwaitingReceipt` ve `ItemUnitFacts.AwaitingReceipt` ona bağlı (Contracts modül içine erişmez). |
| D-906 | Ayıp bedel indiriminin kalem tavanı doğmamış kayıt tutarını düşer (`OrderTransitions.DefectRemedies.cs` `OwedRecordAmountsAsync` → `FirmRefundAmounts`); sipariş tavanı T529'dan beri sayıyordu. |
| D-1195 | İade reddi kaydı müşterinin kalem satırında sebebi ve malın firmanın bedeliyle teslimat adresine geri gönderileceğini söyler (`ItemRecordLabels.ReturnRejectionDetail`; `02 §7.4.9`, K-657). |
| D-1281 | Firmanın bütün sipariş iptali kalemleri azalan uuid sırasıyla yazar (`OrderTransitions.FirmCancellation.cs`): kargo ücreti K-1107'deki gibi tek kayıtta döner. |
| D-1382 | Doğmuş bekleyen geri ödemenin süresi kaydın anından işler: `PendingRefund.PeriodRecordId`, `IAfterSalesModule.RefundPeriodStartsAsync`, `PanelOrderReader.PeriodStartOf`. |
| D-1439 | Cayma son adımının cümlesi karma seçimde iki başlangıcı adetleriyle söyler — **karar K-1622**; `OrderScreenTexts.WithdrawalSentence(fromDeclarationUnits, fromArrivalUnits, way, days)`. |
| D-1440 | Çok kalemli cayma beyanları azalan uuid sırasıyla yazılır (`WithdrawalPageModel.InWriteOrder`; K-1107 (1)); K-1076 hâlinin (ret / mal dönmedi) ekran testi eklendi. |
| D-1588 | `LoadPayeeBookAsync` istekleri izlenen hâlle (`LocalAsync`) okur; aynı işlemde kapatılan istek kapalı görünür (`AfterSalesModule.RefundPayees.cs`). |
| D-1598 | Kart hattında kalem çıkarmanın onay penceresi K-797 cümlesini tutarla söyler: "{n} ürün siparişten çıkarılacak ve {tutar} müşterinin kartına geri ödenecek." (`IrreversibleConfirmations.CardRefundingRemoval`). Tutar kayıt yazıcısının kuralından okunur — `AfterSalesModule.ItemRemovalAmountAsync` (yazıcı `AddItemRemovalAsync` ile aynı `RemovalAmountsAsync`), kargo ücreti `02 §7.2.9` tetiğiyle (`IPanelItemSteps.RemovalRefundAsync`). Teslim alma varsayılanı (`Awaiting + NotReturned`) T528 düzeltme 1'de kapanmıştı. |
| D-1702 | İade ekranındaki IBAN alanı katalog metnini çizer, ham kodu (`FieldRequired`) değil: `OrderScreenModels`, `AfterSalesScreenModels` alanı çizimde bağlar (süzgeç çevirisinden sonra). |

### Etkilenen Modüller / Dosyalar

- AfterSales: `ItemQuantities`, `Contracts/ItemUnitFacts.cs`, `AfterSalesModule.Interventions.cs`, `AfterSalesModule.RefundPayees.cs`, `AfterSalesModule.RefundLine.cs`, `ItemRecordLabels`, `IAfterSalesModule`
- Ordering: `OrderTransitions.DefectRemedies.cs`, `OrderTransitions.FirmCancellation.cs`, `PanelItemSteps.cs`, `IPanelItemSteps`, `PanelOrderReader.cs`
- Payment: `PendingRefund` kaydı (`PeriodRecordId`)
- Sayfalar: `Pages/Orders/OrderScreenTexts.cs`, `AfterSalesScreenModels.cs`, `OrderScreenModels.cs`; `Pages/Panel/OrderDetail.ItemSteps.cs`
- Belgeler: `DEFERRED_BACKLOG.md` (10 satır yerinde), `PRODUCT_DISCOVERY_STATUS.md` (K-1622), `STATUS_CHANGELOG.d/T604-U.md`

### Para ve güvenlik kalemleri (KK2)

Her kalem para kalemidir (iade, geri ödeme, IBAN). Kırmızı: düzeltilmiş test değişiklikten önceki kodda koşuldu.

| Kalem | Test | Kırmızı (eski kod) | Yeşil |
|---|---|---|---|
| D-568 | Unit `ItemRecordRulesTests.TheItemsAndTheDeclarationsAwaitedUnits_AreTheOneContractsFormula_D568` | (2,2,1,0): beklenen 0, gelen -1 | ✓ |
| D-906 | `ItemRecordsTests.PriceReduction_OnTheTransfer_CountsTheItemsRecordWhoseRefundIsNotBornYet_D906` | InvalidInput beklendi, null | ✓ |
| D-1195 | `AfterSalesScreenTests.TheReturnRejection_OnTheItemsRow_SaysItsReasonAndTheGoodsWayBack_D1195` | sebep metni yok | ✓ |
| D-1281 | `LateDeliveryAndFirmCancellationTests.TheFirmsWholeOrderCancellation_OfTwoPhysicalItems_ReturnsTheShippingFeeOnce_OnTheSmallestItem_D1281` | ücretli satır yok | ✓ |
| D-1382 | `AfterSalesScreenTests.APendingRefundBornAfterItsRecord_RunsItsPeriodFromTheRecordsMoment_D1382` | 22.11 beklendi, 02.12 | ✓ |
| D-1439 | `AfterSalesScreenTests.TheLastStepOfAMixedChoice_NamesBothRefundStartsWithTheirUnits_D1439`; Unit `OrderScreenTextsTests.TheWithdrawalSentence_OfAMixedChoice_..._D1439` | eski sayfa satırıyla (UE1 devamı, sayfa satırı geçici geri alındı): beklenen "geri ödeme 1 ürün için beyanınızdan,", gelen "geri ödeme malın firmaya ulaşmasından" (tek başlangıç) | ✓ |
| D-1440 | `AfterSalesScreenTests.TheDeclarationsOfOneRequest_AreWrittenInDescendingUuidOrder_SoTheSmallestItemIsTheLast_D1440`; `TheShippingFeeLines_AreGone_OnceAUnitWasRejectedOrClosedAsNotComeBack_K1076_D1440` | sıra testi kırmızı; K-1076 teorisi önce de yeşildi (eksik testti) | ✓ |
| D-1588 | `ItemRecordsTests.ThePayeeRead_SeesAnItemRequestClosedInTheSameTransaction_AsClosed_D1588` | (null, null) geldi | ✓ |
| D-1598 | `OrderDetailPageTests.CardRemovalWindow_SaysTheAmountTheRecordPaysBack_WithTheShippingFeeOnlyWhenTheRemovalTakesTheLastPhysicalUnit_AndWritesNothing_D1598`; `OrderCorrectionTests.PanelRemoval_TheWindowsAmount_IsTheRefundTheRecordThenWrites_TheFeeWithTheLastPhysicalUnit_AndNoneBeforeThePaymentCame_D1598` | eski pencere cümlesiyle (geçici geri alındı): "Not found: 1 ürün siparişten çıkarılacak ve 130 TL müşterinin…" — eski metin tutarsız "tutarı" diyordu; ikinci test yeni okumanın (pencere tutarı = yazılan geri ödeme satırı) sınamasıdır, eski kodda API yoktu | ✓ |
| D-1702 | `AfterSalesScreenTests.TheWithdrawalsEmptyIban_Is422WithTheCatalogText_NotTheCode_D1702` | `>FieldRequired<` bulundu | ✓ |

- **Var olan testler:** değiştirilmedi; Unit TAM 8092/8092, ilgili Integration sınıfları yeşil (aşağıda). D-1598'de eski pencere metni K-797'den sapıyordu (ad + "tutarı"); onu sınayan test yoktu.
- **Çıkış yolu ve geri alma:** her kalem kendi küçük değişikliğidir; şema değişikliği ve göç yok. Geri alma ilgili commit'in `git revert`'üdür (87b9a992 kalemler, 3dc9dd68 null-forgiving ve test günü, 903f7b0b D-1598). D-1598'in yeni okuması yalnız okur (kilitsiz, yazmaz); kayıt tutarı yine kilit altında yazıcıda hesaplanır. Pencere okuması tutar döndürmezse (adetler iki okuma arasında kapandı) adım "kapalı" cevabıyla döner, pencere açılmaz.
- **Güvenlik taraması:** yeni dış bağımlılık, sır, yetki ya da girdi yolu yok; D-1598 okuması var olan `[GatedOperation]` POST'unun içinde, aynı yönetici kimliğiyle.

### Kabul Kriterleri Kontrolü (UE1 payı)

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 — hedefi T604 olan kalemler (UE1'in 10'u) kapanır, ölçü kalemin kabul cümlesi | ✓ | Yukarıdaki tablo; D-1598'in kendi cümlesi "pencere `CardRefundingRemoval`'ı kaydın hesapladığı tutarla çağırır" — K-797 cümlesi tutarı ister (önceki işçinin "yorum düzeltilir" seçeneği geri alındı, kod yapıldı); F5 kapısı notu da kod istiyordu |
| KK2 — para ve güvenlik kalemleri ayrı başlıkta, kırmızı–yeşil, çıkış yolu, eski testler değişmeden | ✓ | "Para ve güvenlik kalemleri" başlığı |
| KK3 — tarayıcı kalemleri | — | A parçası (A4 dilimi); UE1'de yok |
| KK4 — D-1678 | — | A parçası (A4); UE1'de yok |
| KK5 — kapanan satır `✅ Çözüldü → T604`; sayaç | ✓ | `DEFERRED_BACKLOG.md` 10 satır yerinde ✅; sayaç faz kapısı kayıt PR'ında (talimat: sayaca dokunulmaz); yeni kalem yok |
| `09 §25` ilgili satırlar | ✓ | tek kural evi: pencere tutarı yazıcının `RemovalAmountsAsync`'inden, ücret `ShippingFeeRefunds`'tan (ikinci kopya yok); okuma döngüde sorgu değil (kalem başına sabit sayıda okuma); test beklenen tutarı yazılan satırla karşılaştırır |
| Güvenlik taraması | ✓ | yukarıda |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil bütün UE1 kalemleri diff'te yeniden denetlendi; `sik-fail-nedenleri.md` okundu.

| Denetim | Sonuç |
|---|---|
| Her kalemin kabul cümlesi (10 satır) | ✓ — tablo; D-1598 düzeltildi (önceki işçi kapanışı yorumla yapmıştı, K-797 tutar istiyor) |
| D-1440 "cayma geri ödemesini başlatan task kalemi testle sınar" | ✓ — cayma geri ödemesi firmanın panel adımıyla doğar (`PaymentModule.FirmRefunds`); ücret tetiği beyan anlarından okunur, sıra testi + K-1076 ekran testi |
| Sık FAIL (1) bekçi/tarama istisnası geniş | uygulanmaz — tarama değişmedi |
| (2) tarama çağrı biçimi | uygulanmaz |
| (3) 06 varsayılan / FK dizini | uygulanmaz — şema değişmedi |
| (4) aynı sayıyı/etiketi anan öteki yerler | ✓ — K-797 cümlesi tek yerde (`IrreversibleConfirmations`); K-785 cümlesine K-1622 eklendi, öteki ekran aynı yardımcıyı kullanıyor |
| (5) belge kuralını değiştiren metin karar kaydısız | ✓ — D-1439'un yeni cümlesi K-1622'de |
| "Kabul kriteri kısmen" | ✓ — ~ yok |
| Unit TAM bu turda | ✓ — 8092/8092 (null-forgiving düzeltmesinden ve D-1598'den sonra) |
| `dotnet format` değişen `.cs` | ✓ — 25 dosya, değişiklik yok |
| Branch izolasyonu | ✓ — commit başlıkları yalnız T604 |

### Test Sonuçları

- **Unit:** TAM 8092/8092 ✓.
- **Integration:** koşulan sınıflar — `AfterSalesScreenTests`, `ItemRecordsTests`, `LateDeliveryAndFirmCancellationTests`, `OrderPageTests`, `CustomerOperationsTests` (önceki tur, 154; D-1439 testi düzeltildi, yeşil); `OrderCorrectionTests`, `OrderDetailPageTests`, `AfterSalesScreenTests` (bu tur, 188; D-1598 modül testi aynı ana düşen iki kaydı sıralayamıyordu — saat ilerletildi, 2/2 yeşil). **TAM: CI.**
- **E2E:** UE1 tarayıcı akışı değiştirmez; CI'ın E2E işi.

### Commit & PR

- Commit'ler: 87b9a992, 3dc9dd68, 903f7b0b, a3b75ba2 ve bu satırın commit'i
- PR: #595 (taslak; gövde "Dilim UE1 tamam; sonraki dilimler aynı dalda")

### Dış Varsayımlar

Bu dilimde dış varsayım yok (ödeme sağlayıcısına çağrı eklenmedi; tutar okuması yereldir).

### Notlar

- Working tree: temiz (devam dalı `origin/task/T604-urun-kodu` 3dc9dd68'den). Hook yolu `scripts/git-hooks`.
- Main CI (son üç tamamlanan): 44225601946 success, 44225601836 success, 44222599276 cancelled — aynı dalın sonraki itmesinin iptali (concurrency), failure sayılmaz ⚠.
- ⚠ K-1622 numarası U parçasına ayrıldı; A parçası K-1623 kullanır.
- ⚠ D-1439 kırmızısı dolaylıydı (önceki tur test teslim gününü bugün kuruyordu); bu turda düzeltilmiş test eski sayfa satırıyla bir kez koşuldu, kırmızı yukarıda.
- STATUS T604 tablo satırına dokunulmadı (iki PR; yönetici günceller).

---

## Dilim UE2 — panel sipariş yazıcıları, sipariş ve ödeme özeti (para ve güvenlik)

### Yapılan İşler

UE2'nin 12 kalemi: 10'u kapandı, 2'si (D-610, D-892) gerekçeyle sonraki U dilimine kaldı (hedef T604).

| Kalem | Değişiklik | Durum |
|---|---|---|
| D-1450 | Son ödeme günü tek kural evinde: `Contracts/PaymentWindowDay.LastOf`; `PaymentWindowRules.LastPaymentDay`, `OrderPageSummary.TransferLastPaymentDay`, `ConfirmationScreenFacts.LastPaymentDay` ve `CustomerNoticeTemplates.LastPaymentDay` (bildirimin özel kopyası bu turda kaldırıldı) ona bağlı. Sipariş sayfası da onay ekranı da pencere bitişinin tarihini yazıyordu (+1 gün). | ✅ |
| D-1246 | `CartLine.LineAmount`, `CartView.PayableTotal` modülde; `CartRules.OrderableSubtotal` satır tutarlarını toplar; `Cart.cshtml` hesap yapmaz. | ✅ |
| D-1197 | Karar K-1624 (1): `DownloadDigitalFile` satırı `OpenQuantity > 0` ister — kayıtla kapanmış kalem indirilemez. | ✅ |
| D-1259 | `OrderPageModel.DownloadOf` düğmenin açıklığını işlem satırından okur (`Operations.IsOpen(DownloadDigitalFile, item)`): GET ve POST aynı karar. | ✅ |
| D-1542 | `Ordering/PanelOrderGate.cs` (`LockAsync`, `CheckAsync`); `PanelOrderSteps`, `PanelOrderUpkeep`, `PanelItemSteps`'teki üç kopya kalktı. | ✅ |
| D-1544 | `OrderDetailModel.OnPageHandlerExecutionAsync`: süzgecin 422'sinde sipariş yüklenir, sipariş yoksa 404 — bağlanamayan değer 500 değil. | ✅ |
| D-1401 | Karar K-1624 (3): cayma ve ayıp ekranlarının süzgeç kancası alan 422'sinden önce gönderilen kalemin sahipliğini sorar (yabancı → 404; `07 §2.3.3`). | ✅ |
| D-769 | `Carts.OwnReservedUnitsAsync` (aynı sepetin Alındı + Bekliyor siparişlerinin ayırması, tek gruplu sorgu) → `ICatalogModule.ListCartVariantFactsAsync`/`JudgeCartQuantitiesAsync` `ownReservedUnits`, `StockRules.ReservedAgainst`. Sepete ekleme ve adet yolu `null` geçer → D-1759. | ✅ |
| D-650 | Karar K-1624 (2): kargo ücreti 0 TL iken eşik mesajı yok (`ShippingCharge.Assess`); `02 §3.19.5`, `03` 2.3.2, `04` 2.6.5 ve 5.12.6. | ✅ |
| D-1525 | Karar K-1624 (4) — yönetici kararının ilk yolu: var olan testleri değiştirmeden spec'e uyan yol vardı. `07 UC-146`/`UC-151` ve `04` 7.2.14 isteğe teyit alanı koymaz; panel formu K-798 hatırlatmasıyla açılır ve gönderilmesi yöneticinin beyanıdır (`PanelOrderCorrections.FormAddressConfirmation`, `FormIdentityConfirmation`, T530). Yazıcının teyit parametresi modülün savunmasıdır (`02 §8.3.6`, §8.3.7; beyansız çağrı `ConfirmationRequired`) ve `OrderCorrectionTests` KK4, KK7 değişmedi. Kod değişmedi; `12 VAL-A239`, `VAL-A244` beklenen sonucu bu okumayla yazıldı. "Spec gereği değişen test" yok. | ✅ |
| D-610 | Müşteri iptal, cayma, ayıp talebi ve iade yazıcılarının kendi koşul denetimlerinin `IOrderOperations.CheckAsync` kararına indirilmesi dört para yazıcısını ve onların sınamalarını birlikte değiştirir; dilimin süre bütçesine sığmadı, yarım bırakmak aynı dalda iki kural evi bırakırdı. | ⚪ sonraki U dilimi |
| D-892 | Önceki işçinin "kod yok" kapanışı geri alındı: `RescindDefectClaimAsync`/`ReduceDefectClaimPriceAsync` hiçbir panel ucundan çağrılmıyor (çözüm geri ödemesi sınamalarda tohumlanıyor); F5 kapısı notu bağlama ve iz satırını birlikte istiyor. `07`'de ayrı uç yok (T528, K-808) — bağlanacak yol D-610 ile aynı dilimde. | ⚪ sonraki U dilimi |

### Etkilenen Modüller / Dosyalar

- Ordering: `Contracts/PaymentWindowDay.cs`, `PaymentWindowRules.cs`, `Contracts/OrderAccessContracts.cs`, `Contracts/ConfirmationScreenContracts.cs`, `Contracts/CartContracts.cs`, `CartRules.cs`, `Carts.cs`, `Contracts/ShippingCharge.cs`, `OrderOperationCatalog.cs`, `PanelOrderGate.cs` (yeni), `PanelOrderSteps.cs`, `PanelOrderUpkeep.cs`, `PanelItemSteps.cs`
- Catalog: `Contracts/ICatalogModule.cs`, `CatalogModule.cs`, `CartVariantReader.cs`, `CartQuantityReader.cs`, `StockRules.cs`
- Notifications: `CustomerNoticeTemplates.cs`
- Sayfalar: `Pages/Orders/OrderPageModels.cs`, `ConfirmationScreenModels.cs`, `AfterSalesScreenModels.cs`; `Pages/Panel/OrderDetail.cshtml.cs`; `Pages/Storefront/Cart.cshtml`
- Belgeler: `02` v0.117, `03` v0.50, `04` v0.57, `12` v0.37; `PRODUCT_DISCOVERY_STATUS.md` (K-1624, belge tablosu); `DEFERRED_BACKLOG.md` (12 satır yerinde); `DEFERRED_BACKLOG.d/T604-U.md` (D-1758, D-1759); `STATUS_CHANGELOG.d/T604-U.md`

### Para ve güvenlik kalemleri (KK2)

Kırmızı: yeni test, ilgili satır geçici geri alınarak (eski kod) bu turda koşuldu; sonra satır geri kondu ve yeşil koşuldu.

| Kalem | Test | Kırmızı (eski kod) | Yeşil |
|---|---|---|---|
| D-1450 | `OrderPageTests.TheTransfersLastPaymentDay_IsTheModulesDay_TheOneBeforeTheWindowsEnd_AsOnTheNoticesAndThePanel_D1450`; `CheckoutPageTests.TheTransferScreen_NamesTheLastPaymentDay_TheDayBeforeTheWindowsEnd_NotTheEndsOwnDate_D1450` (yeni, `CheckoutPageTests.T604.cs`) | "Not found: `<dd>20 Kasım 2026</dd>`"; onay ekranı "Not found: `<dd>10 Ocak 2030</dd>`" | ✓ |
| D-1246 | Unit `CartRulesTests.TheLineAmountAndThePayableTotal_AreTheModulesValues_TheSubtotalSumsTheSameLineAmounts_D1246` | eski kodda `LineAmount`/`PayableTotal` yok — derleme kırmızısı; davranış değişmedi (aynı çarpım modüle taşındı) | ✓ |
| D-1197 | `OrderPageTests.AClosedPaidDigitalItem_HasNoDownloadButton_AndItsPostIsRefused_SpendingNothing_K1624_D1197_D1259` | eski katalog satırıyla: "Sub-string found: handler=DownloadDigitalFile" | ✓ |
| D-1259 | aynı test, yalnız `DownloadDigitalFile` satırı yeni, `DownloadOf` eski çizimle (ayrı koşu) | "Sub-string found: handler=DownloadDigitalFile" | ✓ |
| D-1542 | `OrderDetailPageTests` (bütün sınıf) | taşıma; davranış değişmedi — eski testler değişmeden 165/165 (`OrderCorrectionTests` ile) | ✓ |
| D-1544 | `OrderDetailPageTests.AFormValueThatDoesNotBind_Is422WithTheOrdersScreen_NotA500_AndAKeyThatIsNoOrder_Is404_D1544` | `[500, 500, 500, 500]` geldi, `[422, 422, 422, 404]` beklendi | ✓ |
| D-1401 | `AfterSalesScreenTests.AForeignItem_WithABrokenFieldRule_Is404_NotTheFields422_OnBothScreens_AndTheOwnItemKeepsIts422_K1624_D1401` | `[422, 422, 422, 422]` geldi, `[404, 422, 404, 422]` beklendi | ✓ |
| D-769 | `OrderConfirmationTests.ThePaymentStepsSummary_AfterTheSameCartsUnpaidOrder_CountsItsReservationAsSellable_AndAnotherCartsNot_D769` | `ownReservedUnits = null` ile: "Assert.NotNull() Failure: Value is null" (özet boş) | ✓ |
| D-650 | Unit `ShippingChargeTests.AFeeOfZero_WithAThresholdSet_ShowsNoThresholdMessage_EveryOrderIsFreeAlready_K1624_D650` | eski koşulla: beklenen null, gelen 490 | ✓ |
| D-1525 | `OrderCorrectionTests.AnAddressRequestThatTheFirmDidNotConfirm_ChangesNothing_..._KK4`, `TheEmailCorrection_NeedsTheNameThePhoneTheItemsAndTheAmount_..._KK7`; `OrderDetailPageTests.CorrectAddress_..._KK1`, `CorrectOrderEmail_..._KK9_KK10` | kod değişmedi (belge hizası) — kırmızı koşusu uygulanmaz; testler değişmeden yeşil | ✓ |

- **Var olan testler:** değiştirilmedi. Tek dokunuş `UnitTests/Infrastructure/SitemapSourceTests.cs` sahte `ICatalogModule`'ün iki imzası (D-769'un yeni parametresi; davranış yok) ⚠.
- **Çıkış yolu ve geri alma:** şema ve göç yok; her kalem kendi küçük değişikliğidir, geri alma ilgili commit'in `git revert`'üdür (b997aab8 D-1450/D-1246/D-1197/D-1259/D-1542, c349f312 D-1401/D-1544, 69b4b7de D-769/D-650, ab10e430 ve 14062918 testler, 6b83b4dd onay ekranı testi ve bildirim şablonu). D-769'un okuması yalnız okur (kilitsiz, aynı sepetin ödenmemiş siparişleri); onayda önceki sipariş kancada iptal edildiği için ayırma iki kez sayılmaz. D-1544/D-1401 kancaları yalnız süzgecin kendi 422'sinde çalışır, işleyici yolu değişmez.
- **Güvenlik taraması:** yeni dış bağımlılık, sır, yetki ya da girdi yolu yok. D-1401 sahiplik sızıntısını kapatır (yabancı kalem artık alan hatasıyla "var" demez); D-1544 hata yolu sipariş anahtarı olmayan kökte 404 verir (varlık sızmaz).

### Kabul Kriterleri Kontrolü (UE2 payı)

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 — UE2'nin 12 kalemi kapalı ya da gerekçeli | ✓ | 10 ✅ (yukarıdaki tablo, her biri kalemin "Yapılacak" cümlesiyle denetlendi); D-610, D-892 gerekçeyle sonraki U dilimine, hedef T604 — satırlarında not |
| KK2 — para ve güvenlik kalemleri ayrı başlıkta; kırmızı–yeşil; eski testler aynı | ✓ | "Para ve güvenlik kalemleri (KK2)" |
| KK3 — tarayıcı kalemleri | — | A parçası; UE2'de yok |
| KK4 — D-1678 | — | A parçası; UE2'de yok |
| KK5 — kapanan satır `✅ Çözüldü → T604`; sayaç | ✓ | `DEFERRED_BACKLOG.md` 10 satır ✅, 2 satır not; yeni kalemler parça dosyasında (D-1758, D-1759); sayaca dokunulmadı (talimat) |
| `09 §25` ilgili satırlar | ✓ | tek işlev: son ödeme günü `PaymentWindowDay`, satır tutarı `CartLine.LineAmount`, indirme düğmesi işlem satırı, panel kapısı `PanelOrderGate`; `[GatedOperation]` + `OperationRunner` yolu değişmedi; döngüde sorgu yok (`OwnReservedUnitsAsync` tek gruplu sorgu); modül sınırı: Notifications yalnız `Ordering.Contracts`'a başvurur; tutar çarpımı yeni değil, `CartRules`'tan `CartLine`'a taşındı ⚠ |
| Güvenlik taraması | ✓ | yukarıda |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil 12 kalem diff'te yeniden denetlendi; `sik-fail-nedenleri.md` okundu.

| Denetim | Sonuç |
|---|---|
| D-1450 "sayfa son günü modülün tek işlevinden taşır, testle" | ✓ — sipariş sayfası ve onay ekranı; bildirimin kopyası da bağlandı (önceki işçi yalnız öneriyordu) |
| D-1246 "satır tutarı ve toplam modülde, görünüm yalnız biçimler, testle" | ✓ |
| D-1197 "karar kaydıyla açık adet > 0 ve test" | ✓ — K-1624 (1) |
| D-1259 "düğme işlem görünümünden, kapalı kalemde düğme yok sınanır" | ✓ |
| D-1542 "tek yardımcı, davranış testleri değişmeden yeşil" | ✓ — 165/165 |
| D-1544 "bağlama hatasında sipariş yüklenir, istek testiyle 422" | ✓ — doğrulayıcının saydığı dört giriş (`MarkDelivered` sürüm, `CorrectCarrier` sürüm ve bool, `CorrectTransition` Guid) |
| D-1401 "422'den önce sahiplik ya da sıra kararla; test iki hâli" | ✓ — K-1624 (3); yabancı ve kendi kalem, iki ekran |
| D-769 "özet aynı sepetin ayırmasını hesaba katar; test önceki sipariş varken onaylar" | ✓ — başka sepetin hâli de sınandı; ekleme yolu D-1759 |
| D-650 "karar kaydıyla §3.19.5 ve kod" | ✓ — K-1624 (2), `02`/`03`/`04` aynı cümle |
| D-1525 "teyit formda ya da parametre kalkar; `12` hizası" | ✓ — K-1624 (4): üçüncü yol (alan yok, parametre savunma olarak kalır) gerekçeli; `12` iki satır |
| D-610, D-892 | ⚪ gerekçeli — sonraki U dilimi; D-892'nin önceki "kod yok" kapanışı F5 notunu karşılamıyordu (sık FAIL "kapandı denip karşılanmadı"), geri alındı |
| Sık FAIL (1) bekçi/tarama istisnası geniş | uygulanmaz — tarama değişmedi |
| (2) tarama çağrı biçimi | uygulanmaz |
| (3) 06 varsayılan / FK dizini | uygulanmaz — şema değişmedi |
| (4) aynı kuralı anan öteki yerler | ✓ — eşik kuralı `02`/`03`/`04`'ün dört yerinde; teyit `12`'nin iki satırında; son ödeme günü dört okuyucuda tek işlev; belge sürümleri `PRODUCT_DISCOVERY_STATUS` tablosunda |
| (5) belge kuralını değiştiren metin karar kaydısız | ✓ — K-1624 dört madde |
| "Kabul kriteri kısmen" | ✓ — ~ yok; iki kalem gerekçeli olarak açık, kapandı denmedi |
| Unit TAM bu turda | ✓ — 8094/8094 |
| `dotnet format` değişen `.cs` | ✓ — 53 dosya (`--include`), değişiklik yok |
| Branch izolasyonu | ✓ — commit başlıkları yalnız T604; A parçasının test sınıflarına dokunulmadı |

### Test Sonuçları

- **Unit:** TAM 8094/8094 ✓.
- **Integration:** koşulan sınıflar — `CheckoutPageTests`, `OrderPageTests`, `OrderConfirmationTests`, `AfterSalesScreenTests` (94/94); `OrderDetailPageTests`, `OrderCorrectionTests` (165/165). **TAM: CI.**
- **E2E:** CI'ın E2E işi.

### Commit & PR

- Commit'ler: b997aab8, c349f312, 69b4b7de, ab10e430, 14062918, 6b83b4dd ve bu satırın commit'i
- PR: #595

### Dış Varsayımlar

Bu dilimde dış varsayım yok.

### Notlar

- ⚠ K-1624 numarası U parçasına (UE2) ayrıldı; A parçası K-1623 kullanır.
- ⚠ D-1525 kod değişikliği olmadan kapandı: F5 kapısı notu iki yolu da "kod değişikliği" sayıyordu; yönetici kararının ilk yolu (var olan testler değişmeden spec'e uyan yol) seçildi — yol T530'dan beri kodda, eksik olan karar ve `12` hizasıydı.
- ⚠ D-892 önceki işçinin devir notunda "kapanış gerekçesiyle ✅" diye işaretliydi; bu turda kapatılmadı (yukarıda).
- ⚠ D-1259'un kırmızısı D-1197'den ayrı koşuldu (katalog satırı yeni, düğme çizimi eski).
- STATUS T604 tablo satırına dokunulmadı.

---

## Dilim UE3 — kimlik, oturum, yeniden doğrulama izni, sahtecilik reddi (güvenlik)

İki işçide yapıldı (ilki süre bütçesiyle bölündü, devir notu `devir-UE3.md`); bu bölümün tabloları iki işçinin bütün işini kapsar.

### Yapılan İşler

| Kalem | Değişiklik |
|---|---|
| D-1317, D-1508 | `FormResults.Refused(page, code)` (422 + red satırı) ve `FormResults.LinkNoLongerValid(page)` (200 + `LinkNoLongerValid` satırı) tek yerde satır yazar; Verification, EmailRevert, PasswordReset (müşteri), panel Reset/Invite/Verify/Revert bunları kullanır; Register ve SignIn bekleyen kaydın 303'ünde `PendingRegistrationUnresolved` satırı yazar |
| D-1375 | `AdminEmailChanges.RestorePreviousEmailAsync` `changed != 1` → istisna (doğrulamayla aynı; tüketim işlemle geri alınır) |
| D-1473 | `AdminEmailChanges.RequestAsync` kilitten sonra önce izni okur (`HoldsReauthenticationAsync`), yoksa ReauthenticationRequired; tüketim sonda (müşteri sırası) |
| D-1474 | `EmailChanges.ResendAsync` ve `AdminEmailChanges.ResendAsync`: önce bekleyen değişiklik, sonra L-9 (VAL-A441) |
| D-1454, D-1389 | `Infrastructure/Security/ReauthenticationPermissionFilter.cs` (yeni, sıra -120, `ForgerySetup`'ta kayıt) + `[ReauthenticationFormPage]` (Profile, Reauthentication, MyAccount, Reauth; test sunucusunun jeton sayfası); `SessionAuthenticationHandler` izin varsa yalnız `MarkHeld` (kimliğe talep eklenmez, `09 §15.1` m.2); işaretli sayfa kümesi kapalı liste testiyle sabit |
| D-608 | `AccountDeletions.RemoveAsync` yalnız `Customer` imha satırı; sayım sorguları ve `AccountOrderRelease.Carts` kalktı |
| D-141 | `FieldMessageCatalog.ActionMessages` (alana bağlı olmayan kodların cümlesi); `ForgeryProtectionFilter.Message` katalogdan; `04` 2.7.3.1 (v0.58) |
| D-1190 | Sepet: 400'de sepet okunur, mesaj gönderilen satırda (`RefusedItemId`, `Cart.cshtml`); ödeme adımı: `OnPageHandlerExecutionAsync` 400'de adımı gönderilen değerlerle okur (`ShowAsync(checksGate:false)`) |
| D-1374 | yapılmadı — UE4'e (K-1627 (5)) |
| Kayıtlar | K-1627 (beş madde); `04` v0.58; `DEFERRED_BACKLOG.md` 11 satır yerinde; D-1773 parça dosyasında |

### Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/` — `Forms/FormResults.cs`, `Security/ForgeryProtectionFilter.cs`, `Security/ForgerySetup.cs`, `Security/ReauthenticationPermissionFilter.cs` (yeni), `Security/SessionAuthenticationHandler.cs`, `Validation/FieldMessageCatalog.cs`
- `src/Shopfolio.Web/Modules/` — `Administration/AdminEmailChanges.cs`, `Membership/AccountDeletions.cs`, `Membership/EmailChanges.cs`, `Ordering/Contracts/AccountOrderRelease.cs`, `Ordering/OrderingModule.cs`
- `src/Shopfolio.Web/Pages/` — Account (Profile, Reauthentication), Panel (Invite, MyAccount, Reauth, Reset, Revert, Verify), Storefront (Cart, Checkout, EmailRevert, PasswordReset, Register, SignIn, Verification)
- Testler: `*PageTests.T604.cs` (9 Web sınıfı), `Web/RejectionLines.cs`, `UnitTests/Validation/FieldMessageCatalogTests.T604.cs`, `UnitTests/Infrastructure/ReauthenticationFormPageTests.cs`
- Belgeler: `04` 2.7.3.1 ve sürüm, `PRODUCT_DISCOVERY_STATUS.md` (K-1627, belge tablosu), `DEFERRED_BACKLOG.md`, `DEFERRED_BACKLOG.d/T604-U.md`, `STATUS_CHANGELOG.d/T604-U.md`

### Para ve güvenlik kalemleri (KK2)

Kırmızı: kalemlerin `src` değişiklikleri aynı derlemede geçici geri alındı (her kalem kendi satırında, birbirine dokunmayan yerlerde; yama `t604u-kk2-red.patch`), yeni testler koşuldu; sonra `git checkout -- src` ile geri kondu ve sınıfların tamamı yeşil koşuldu (aşağıda "Test Sonuçları"). Her kırmızının nedeni kendi kaleminin iddiasıdır.

| Kalem | Test | Kırmızı (geçici bozma) | Yeşil |
|---|---|---|---|
| D-1317 | `RegistrationPageTests.TheLinkPagesRefusals_..._D1317`, `PanelAccessPagesTests.TheResetAndTheInvitation_..._D1317`, `PanelHomeAndVerificationPagesTests.ThePanelLinkPagesRefusals_..._D1317`, `CustomerPasswordResetAndReauthenticationPageTests.ALinkSpentBetweenThePageAndTheButton_..._D1317` | `Refused`/`LinkNoLongerValid` satır yazmadan: dördü kırmızı | ✓ |
| D-1508 | `EmailRevertPageTests.TheRevertsRefusals_..._D1508` | aynı bozma: kırmızı | ✓ |
| D-1473 | `MyAccountPageTests.ATakenAddress_WithoutTheValue_Is303_..._D1473` | izin okuması devre dışı: kırmızı (422 alınmış adres) | ✓ |
| D-1474 | `AccountProfilePageTests.TheLinkAskedForAgain_..._VALA441_D1474`, `MyAccountPageTests.AskingTheLinkAgain_..._D1474` | L-9 bekleyen değişiklik aranmadan önce sayıldı: ikisi kırmızı | ✓ |
| D-1454 | `CustomerPasswordResetAndReauthenticationPageTests.AnotherPageRequest_DropsThePermission_..._D1454` (3 sayfa: `/`, `/sepet`, `/hesap/adresler`) | süzgeç kaydı kaldırıldı: üçü kırmızı; karşı yön `WithNoOtherPageRequest_..._D1454` yeşil kaldı (beklenen) | ✓ |
| D-1454 (istisna kümesi) | Unit `ReauthenticationFormPageTests.ThePagesLeftToTheirOwnRule_..._D1454` | `CartModel`'e işaret eklendi: "Collections differ" | ✓ |
| D-1389 | `PanelHomeAndVerificationPagesTests.APanelPageRequest_DropsTheHeldPermission_..._D1389_D1454` | süzgeç kaydı kaldırıldı: kırmızı | ✓ |
| D-608 | `MembershipTests.DeleteOwn_RemovesTheAccountAndWhatHangsOnIt_..._KK1` (spec gereği değişti, aşağıda) | oturum sayım satırı geri eklendi: kırmızı | ✓ |
| D-141 | Unit `FieldMessageCatalogTests.TheForgeryRefusalsSentence_..._D141` | `Message` kendi kopyası (farklı cümle): "Strings differ" | ✓ |
| D-1190 | `CartPageTests.TheForgeryRefusal_..._D1190`, `CheckoutPageTests.TheForgeryRefusal_..._D1190` (yeni, `CheckoutPageTests.Forgery.T604.cs`) | iki 400 dalı devre dışı: ikisi kırmızı | ✓ |
| D-1375 | — | ⚠ dal satır kilidi altında erişilemez; kırmızı kurulamaz. Davranış: istisna işlemi geri alır (doğrulama dalıyla aynı). Yarış testi A parçasında (D-1301 `TwoRevertPressesAtOnce…`, PR #594) | — |

Kırmızı koşusunun özeti: 16 Integration testinden 15'i kırmızı (16.'sı beklenen karşı yön), Unit D-141 testinden biri kırmızı (öteki kümenin biçim testi), istisna kümesi testi kırmızı.

**Spec gereği değişen test**

| Test | Eski beklenti | Yeni beklenti | Gerekçe | Belge yeri |
|---|---|---|---|---|
| `Model/MembershipAccountDeletionTests.cs` `DeleteOwn_RemovesTheAccountAndWhatHangsOnIt_AndTheDisposalRecordCountsThem_KK1` | imha kaydında Customer 1 + Session 2 + RecognizedBrowserMark 2 + Cart 1 + PendingEmailChange 1 + LinkRecord ≥ 1 | yalnız `{ Customer: 1 }` | Eski test belgede olmayan imha türlerini sabitliyordu; hesaba bağlı satırlar Customer satırının parçasıdır ve kendi satırını yazmaz (D-608) | `06 §5.5.3`, `06 §4.64`, `05 §7.4.2`; `02` Z-31, Z-32 |

- **Var olan öteki testler:** değişmedi; 8 Web test sınıfına yalnız `partial`, test jetonu sayfasına (`Pages/ForgeryTokenSample.cs`) işaret eklendi.
- **Çıkış yolu ve geri alma:** şema ve göç yok; geri alma commit'lerin `git revert`'üdür (899c3e27, d7b8ee2a ve bu turun commit'leri). İzin süzgeci tek satırla (`ForgerySetup` kaydı) devreden çıkar; o durumda eski iki noktalı düşürme (hesap GET'i, yeniden doğrulama ekranı) yerinde kalır.
- **Güvenlik taraması:** yeni dış bağımlılık, sır, girdi yolu yok. D-1473 alınmış adres sorgusunu yeniden doğrulamanın arkasına alır (yönetici hesabının varlığı sızmaz); D-1454 izni bütün sayfa isteklerinde düşürür; istisna kümesi kapalı liste testiyle sabittir (sık FAIL (1)); D-1375 harcanmış bağlantının adres dönmeden işlenmesini kapatır.

### Kabul Kriterleri Kontrolü (UE3 payı)

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 — UE3'ün 11 kalemi kapalı ya da gerekçeli | ✓ | 10 ✅ (her biri kalemin "Yapılacak" cümlesiyle denetlendi, aşağıda); D-1374 gerekçeyle UE4'e, hedef T604 — satırında not, K-1627 (5) |
| KK2 — para ve güvenlik kalemleri ayrı başlıkta; kırmızı–yeşil; eski testler aynı | ✓ (tur 1 düzeltmesiyle) | "Para ve güvenlik kalemleri (KK2)"; tek değişen eski test spec gereği, tabloda; D-1375 kırmızısı kurulamaz ⚠. **Düzeltme (tur 1, Bulgu 2):** bu satır ilk yazıldığında "eski testler aynı" tutmuyordu — `OneTimeValueTests`'in iki KK3 testi bu dilimin D-1454 katmanıyla kırılıyordu (sınıf bu dilimde koşulmamıştı). Testler değişmedi; örnek form sayfası `[ReauthenticationFormPage]` ile işaretlendi (K-1636 (2)) ve iki test değişmeden yeşil ("UE düzeltme tur 1") |
| KK3 — tarayıcı kalemleri | — | A parçası; UE3'te yok |
| KK4 — D-1678 | — | A parçası; UE3'te yok |
| KK5 — kapanan satır `✅ Çözüldü → T604`; sayaç | ✓ | `DEFERRED_BACKLOG.md` 10 satır ✅, 1 satır not; yeni kalem parça dosyasında (D-1773); sayaca dokunulmadı (talimat) |
| `09 §25` ilgili satırlar | ✓ | tek işlev: red satırı `FormResults`'ta tek yerde, işlem düzeyi cümle katalogda, izin düşürme tek süzgeçte; modül sınırı: süzgeç `SessionStore`'u işlem içinde çağırır (`ITransactionRunner`); döngüde sorgu yok |
| Güvenlik taraması | ✓ | yukarıda |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil 11 kalem ve `11` T604 kabul kriterleri ile kontrol listesi diff'te yeniden denetlendi; `sik-fail-nedenleri.md` okundu.

| Denetim | Sonuç |
|---|---|
| D-141 "cümle katalogda, süzgeç kopya tutmaz, `04` ile aynı" | ✓ — `FieldMessageCatalogTests` iki test; `04` 2.7.3.1 cümlesi testte okunur |
| D-608 "Customer dışı satırlar için tür ya da tek satır" | ✓ — tek satır yolu (belgedeki türler), test spec gereği değişti |
| D-1190 "400'de sepet içeriği; ödeme formu aynı karar" | ✓ — sepet ve ödeme adımı, iki test; öteki sayfalar D-1773 |
| D-1317 "bağlantı ve bekleyen kayıt redleri kayda yazılır" | ✓ — dört sınıfta test, iki yardımcı |
| D-1508 "geri alma sayfasının iki reddi satır yazar" | ✓ |
| D-1375 "dal red sayılır ya da istisna" | ✓ — istisna; kırmızı kurulamaz ⚠ |
| D-1389 "izin panelde de düşer" | ✓ — panel düşer, vitrin isteği panel iznine dokunmaz |
| D-1454 "tek katman ve iki yönlü test" | ✓ — üç sayfa + karşı yön; istisna kümesi kapalı liste (bu turda eklendi) |
| D-1473 "değer önce okunur, tüketim sonda; değersiz dolu adres 303" | ✓ |
| D-1474 "bekleyen yoksa satır yok, varsa L-9" | ✓ — müşteri ve yönetici |
| D-1374 | ⚪ gerekçeli — UE4 (süre, ürün kararı ve `02`/`07` metni ister; yönetici kararı) |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ — `[ReauthenticationFormPage]` kümesi kapalı liste testiyle sabit, `Inherited = false` sınanır |
| (2) tarama çağrı biçimi | uygulanmaz — tarama değişmedi; gezinme taraması (`NavigationConsistencyTests`) için SeeOther çağrıları yerinde bırakıldı |
| (3) 06 varsayılan / FK dizini | uygulanmaz — şema değişmedi |
| (4) aynı kuralı anan öteki yerler | ✓ — sahtecilik cümlesi `04`'te tek yerde, kodda katalogda; `04` sürümü belge tablosunda; L-9 sırası iki yazıcıda aynı |
| (5) belge kuralını değiştiren metin karar kaydısız | ✓ — K-1627 beş madde; `04` 2.7.3.1 cümlesi K-1627'yi anar |
| "Kabul kriteri kısmen" | ✓ — ~ yok; D-1374 açık, kapandı denmedi |
| Unit TAM bu turda | ✓ — 8097/8097 (kayıtlardan ve yeni istisna kümesi testinden sonra) |
| `dotnet format` değişen `.cs` | ✓ — 100 dosya (`--include`) + iki yeni dosya, değişiklik yok |
| Branch izolasyonu | ✓ — commit başlıkları yalnız T604; A parçasının test sınıflarına dokunulmadı |
| PR #594 testlerine etki | ✓ — A2 testleri red satırını ölçmez, davranış aynı; çakışma beklenmez |

### Test Sonuçları

- **Unit:** TAM 8097/8097 ✓ (kayıtlardan sonra; yeni `ReauthenticationFormPageTests` dahil).
- **Integration:** koşulan sınıflar — `EmailRevertPageTests`, `RegistrationPageTests`, `PanelAccessPagesTests`, `PanelHomeAndVerificationPagesTests`, `CustomerPasswordResetAndReauthenticationPageTests`, `MyAccountPageTests`, `AccountProfilePageTests`, `CartPageTests` (197/197); `CheckoutPageTests`, `ForgeryProtectionTests`, `MembershipTests` (hesap silme ve yönetici hesapları dahil), `MembershipGoogleTests` (265/265; `ScriptlessFlowTests` adlı sınıf yok). **TAM: CI.**
- **E2E:** CI'ın E2E işi.

### Commit & PR

- Commit'ler: 899c3e27, d7b8ee2a ve bu turun commit'leri
- PR: #595

### Dış Varsayımlar

Bu dilimde dış varsayım yok.

### Notlar

- ⚠ K-1627 numarası U parçasına (UE3) ayrıldı; A parçası K-1626 ve K-1628 kullanır.
- ⚠ D-1375'in kırmızısı kurulamaz (dal kilit altında erişilemez); kapanış davranış ve kod okumasıyla.
- ⚠ D-608 eski bir testin beklentisini değiştirdi (spec gereği, tablo yukarıda).
- ⚠ D-1374 UE4'e bırakıldı (yönetici kararı).
- ⚠ Kırmızı koşusu kalemleri tek derlemede birlikte bozdu (süre); bozmalar birbirine dokunmayan satırlardadır ve her testin kırmızısı kendi kaleminin iddiasıdır.
- STATUS T604 tablo satırına dokunulmadı.

## Dilim UE2b — panel ayıp başvurusu yazıcıları, UE2 artıkları (para ve güvenlik)

**İlk işçi BÖLÜNDÜ (süre).** Beş kalemin üçü kapandı (D-1617, D-1758, D-1759); D-892 devamda kapandı (aşağıda "Devam (2026-11-21)"); D-610 sonraki U dilimine (hedef T604 kalır, satırında gerekçe).

### Yapılan İşler

| Kalem | Değişiklik |
|---|---|
| D-1617 | `Pages/Panel/OrderDetail.Refunds.cs` iki özet yorumu yalnız bağlanan modeli anar (`ProcessRefundInput`, `RetryCardRefundInput`); kalkan üç model adı kodda geçmez |
| D-1758 | Karar K-1629 (1): `DownloadRefusal.ItemClosed`; `OrderPageReader.ClosedReason` sırası ödeme onaysız → kalem kapanmış (`OpenQuantity <= 0`) → hak dolmuş; `OrderPageModel.ItemClosedText`; `07` 4.16.3 ve §6.2 `DownloadNotAvailable` (v0.46), `04` 5.16.13 (v0.59) |
| D-1759 | Karar K-1629 (2): `Carts.AddAsync` (kilitten önce ve kilit altında yeniden) ve `UpdateItemAsync` sepetin kendi ayırmasını (`OwnReservedUnitsAsync`) geçer; `ICatalogModule.JudgeCartAddAsync` / `CartQuantityReader.JudgeAddAsync` yeni `ownReservedUnits` parametresi; form alan kanıt tablosunun 7.1.4.1 satırı yeni çağrı satırına hizalandı (`FormFieldRuleEvidence.cs`) |
| D-892 | ilk işçide yapılmadı (devamda kapandı) — okuma satırında: UC-140 talebin malını teslim alır ve izi yazar, bedel indirimi UC-133'tür; eksik olan teslim almadan sonraki geri ödemenin hatta düşmesidir (`ListOwedRefundRecordsAsync` talebin teslim almasını borçlu saymaz; `RescindDefectClaimAsync` çağrılmaz) |
| D-610 | yapılmadı — süre |
| Kayıtlar | K-1629 (iki madde); `04` v0.59, `07` v0.46; `DEFERRED_BACKLOG.md` D-1617 ✅, D-892 ve D-610 not; `DEFERRED_BACKLOG.d/T604-U.md` D-1758, D-1759 ✅; `STATUS_CHANGELOG.d/T604-U.md` |

### Para ve güvenlik kalemleri (KK2)

Kırmızı: `src` geçici bozuldu (yedekten geri kondu), yeni testler koşuldu.

| Kalem | Test | Kırmızı (geçici bozma) | Yeşil |
|---|---|---|---|
| D-1758 | `Files/OrderPageTests.AClosedPaidDigitalItemsStalePost_SaysTheItemClosed_NotThatTheRightIsUsedUp_K1629_D1758` | `ClosedReason` kapanmış kalem dalı devre dışı: "Assert.Contains() Failure: Sub-string not found" (metin hakkın dolduğunu söyler) | ✓ |
| D-1759 (adet yolu) | `Ordering/OrderConfirmationTests.TheAddAndTheQuantityChange_AfterTheSameCartsUnpaidOrder_CountItsReservationAsSellable_AndAnotherCartsNot_D1759` | `UpdateItemAsync` ve `AddAsync` `null` geçti: "Expected: Updated, Actual: NoStock" | ✓ |
| D-1759 (ekleme yolu) | aynı test | yalnız `AddAsync` `null` geçti: "Expected: Added, Actual: NoStock" | ✓ |

- **Var olan testler:** değişmedi; yalnız form alan kanıt tablosunun (`FormFieldRuleEvidence.cs`) 7.1.4.1 satırı kodun yeni çağrı satırını alıntılar (tablo kod satırını birebir anar). Sahte `ICatalogModule` (`SitemapSourceTests.cs`) yalnız imza.
- **Çıkış yolu ve geri alma:** şema ve göç yok; geri alma bu dilimin commit'lerinin `git revert`'üdür. Eski ayırma davranışı `own` yerine `null` geçerek döner.
- **Güvenlik taraması:** yeni dış bağımlılık, sır, girdi yolu yok. D-1759 başka sepetin ayırmasını satılabilir saymaz (testin son iki satırı); stok sınırı aynen kalır.

### Kabul Kriterleri Kontrolü (UE2b payı)

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 — dilimin 5 kalemi kapalı ya da gerekçeli | ✓ | 3 ✅; D-892, D-610 gerekçeyle sonraki U dilimine, hedef T604 — satırlarında not |
| KK2 — para ve güvenlik kalemleri kırmızı–yeşil; eski testler aynı | ✓ | yukarıdaki tablo; tek değişen eski dosya kanıt tablosunun alıntı satırı |
| KK5 — kapanan satır `✅ Çözüldü → T604`; sayaç | ✓ | D-1617 yerinde; D-1758, D-1759 parça dosyasında; sayaca dokunulmadı |
| `09 §25` tek işlev | ✓ | sepetin kendi ayırması tek okuyucudan (`OwnReservedUnitsAsync`) ve tek kuraldan (`StockRules.ReservedAgainst`); indirme sebebi tek yerde (`ClosedReason`) |

### Öz denetim

| Denetim | Sonuç |
|---|---|
| D-1617 "yorumlar bağlanan modeli anar" | ✓ — `grep` üç eski adı `src`'de bulmaz |
| D-1758 "dördüncü sebep karar kaydıyla `07 §4.16.3` ve `04` 5.16.13'e yazılır, okuyucu onu verir, test bayat POST'un metnini sınar" | ✓ — K-1629 (1), iki belge, `ClosedReason`, test |
| D-1759 "`02 §3.17.7` ve `03` 2.3.1 okunur; kapsıyorsa ekleme ve adet yolu da sepetin kendi ayırmasını geçer ve test eder" | ✓ — K-1629 (2) okuması, iki yol, iki yönlü test (kendi sepet ve başka sepet) |
| D-892, D-610 | ⚪ gerekçeli — süre; D-892 satırında yapılacak tasarım |
| Sık FAIL (1) istisna geniş · (2) tarama çağrı biçimi | uygulanmaz — tarama değişmedi |
| (3) 06 varsayılan / FK | uygulanmaz — şema değişmedi |
| (4) aynı kuralı anan öteki yerler | ✓ — `07` §6.2 `DownloadNotAvailable` satırı ve 4.16.3 birlikte; `04` 5.16.13; belge tablosu sürümleri; enum özeti "four 422 reasons" |
| (5) belge kuralı karar kaydısız | ✓ — K-1629 |
| `dotnet format` değişen `.cs` | ✓ — 12 dosya (`--include`), değişiklik yok |

### Test Sonuçları

- **Unit:** TAM 8097/8097 ✓ (ilk koşuda 3 ✗: form alan kanıt tablosunun 7.1.4.1 alıntı satırı; düzeltildi, yeniden TAM koşuldu).
- **Integration:** koşulan sınıflar — `Ordering.OrderConfirmationTests`, `Files.OrderPageTests`, `Ordering.CartQuantityTests`, `Ordering.CartTests`, `Ordering.MainFlowTests`: 89/89 ✓. TAM: CI.

### Notlar

- ⚠ K-1629 (2) okuması: `02 §3.17.7`'nin "müşteri kendi ayırdığı stoğa takılmaz" cümlesi ekleme ve adet yolunu da kapsar sayıldı (belge metni değişmedi).
- ⚠ D-1758 metni (`ItemClosedText`) yeni bir müşteri cümlesidir; `04` 5.16.13'e birebir yazıldı.
- ⚠ D-892 bu dilimde kod almadı: borçlu kayıt kuralı (AfterSales), UC-131 başlatma yolu, panel sayacı ve geri ödeme satırı birlikte değişir; para işidir, süreye sığmadı. → Devamda kapandı.

### Devam (2026-11-21) — D-892 (para)

#### Yapılan İşler

| Kalem | Değişiklik |
|---|---|
| D-892 | Karar K-1629 (3). Borçlu kayıt: `AfterSalesModule.PendingWork` yeni `OwedKind.ClaimReceipt` — talebe bağlı teslim alma (`Return.DefectClaimId`, talep açık) → `OwedRefundRecord(DefectClaimRescission, RecordId = talep, ReturnId = teslim alma)`; `NextSubjectsAsync` (panel ana sayfa sayacı) ve `SubjectsOfItemsAsync` (sipariş ekranı satırı) aynı sorgudan (`ClaimReceipts()`). UC-131: `PanelRefundSteps.ProcessRefundAsync` bu satırda `RescindDefectClaimAsync`'i çağırır (`RescindDefectClaimRequest.ReturnId`); geri ödeme `ReturnId` taşır (`DefectRescissionShares.ReturnId` → `Refund.ReturnId`), `FindSubjectsWithRefundAsync` teslim almayı karşılanmış sayar. Havalede adım gönderimdir: `MarkTransferRefundSentAsync` (izlenen satırı `FindAsync` ile bulur) + `ApplyFirmRefundSentAsync`; kartta iş satırı yazıcıda. Sürüm bir kez (`GateAsync` kilidi + `LockItemAsync` aynı satır; `RaiseVersionOnce` ikinci artışı yapmaz — test sürüm +1'i sınar). İz `RefundManually` aynı işlemde. |
| Tutar (tek ev) | `FirmRefundAmounts.OfAsync` dönme dalı: `IAfterSalesModule.DefectClaimRescissionAmountAsync` (denetimin `RescissionSplitAsync`'i ile aynı bölüşüm) ve `FirmRefundAmounts.ItemRefundLeftAsync` (D-893 kalem tavanı; ödenen satırın kendi borcu düşülmez, öteki borçlu kayıtlar tavansız tutarlarıyla) küçüğü; kargo ücreti yok. Yazıcı aynı tavanı okur (`OrderTransitions.ItemRefundLeftAsync`/`OwedRecordAmountsAsync` kaldırıldı). |
| Bedel indirimi | `ReduceDefectClaimPriceAsync`, `ReduceDefectClaimPriceRequest`, `IAfterSalesModule.CheckDefectClaimPriceReductionAsync` kaldırıldı (hiçbir uçtan çağrılmıyordu; bedel indirimi UC-133, `09 §25`). |
| Kayıtlar | K-1629 (3); `DEFERRED_BACKLOG.md` D-892 ✅, D-610 not; `DEFERRED_BACKLOG.d/T604-U.md` D-1783 (yeni); `STATUS_CHANGELOG.d/T604-U.md` |
| D-610 | yapılmadı — süre; satırında gerekçe, hedef T604 |

#### Para ve güvenlik kalemleri (KK2)

Kırmızı: `src` dalın önceki başına (`47ceb568`) geri alındı, yeni testler koşuldu, sonra `src` geri kondu.

| Kalem | Test | Kırmızı (eski kod) | Yeşil |
|---|---|---|---|
| D-892 | `Web/OrderDetailPageTests.AClaimsGoodsReceipt_OwesTheRescissionsRefund_AndUc131PaysIt_WithTheTrailLine_AndNoSecondRefund_D892` (UC-140 teslim alma POST'u → satır → UC-131 onay penceresi 100 TL → onaylı POST → `DefectClaimRescission` + `ReturnId` + `SentAt`, sürüm +1, ödeme ekseni, `RefundManually` 1, ikinci POST yazmaz, satır kalkar) | "Expected: 3, Actual: 1" (teslim almanın geri ödeme satırı yok) | ✓ |

**Spec gereği değişen testler** (`ReduceDefectClaimPriceAsync` kalktı):

| Test | Değişiklik | Gerekçe |
|---|---|---|
| `ItemRecordsTests.PriceReduction_StartsAnAmountBasedRefundOfTheItem_AndTakesNoUnit_KK8` | silindi | UC-133'ün davranışı `OrderDetailPageTests.ProcessAmountBasedRefund_*` (KK5, KK7) ile sınanır |
| `ItemRecordsTests.PriceReduction_IsCappedAtThePaidMoneyNotRefunded_TheStartedRefundsIncluded_KK8` | silindi | UC-133 sipariş tavanı: `ProcessAmountBasedRefund_AnEmptyZeroOrExceedingAmount_Is422_AndWritesNothing_KK7` |
| `ItemRecordsTests.PriceReduction_IsHeldToTheItemsPaidAmount_WhenTheOrderPaidMore_D893` | silindi | yazıcıyla gitti; UC-133'ün kalem tavanı → D-1783 |
| `Rescission_AfterAPriceReductionOfTheWholePaidAmount_PaysNothingMore_AndIsRefused_D893`, `Rescission_AfterAPartialPriceReduction_PaysAtMostWhatTheItemsRefundsLeft_D893` | bedel indirimi UC-133'ün yazdığı satırla tohumlanır (`SeedPriceReductionAsync`); beklentiler aynı | yazıcı kalktı |
| `Remedies_OnTheTransferLineWithoutAnIban_RequestItOnce_AndSendB14Once_KK8`, `Remedies_OnTheTransferLineWithAnIbanOnFile_AskNothing_KK8`, `Remedies_WithoutMoneyToPayBack_OrByANonAdministrator_AreRefused_AndWriteNothing_KK8`, `Rescission_TakesAtMostTheClaimsUnitsNotRescindedYet_AndOnlyOnAnOpenClaim_KK8` | ikinci çözüm ikinci dönme olur ya da bedel indirimi satırı düşer; beklentiler aynı | yazıcı kalktı |
| `TheIban_OfTheItemsRequest_...`, `TheIban_OnTheOrdersFile_..._KK5` | bedel indirimi yerine ikinci dönme; beklentiler aynı | yazıcı kalktı |
| `ItemRecordsTests.PriceReduction_OnTheTransfer_CountsTheItemsRecordWhoseRefundIsNotBornYet_D906` → `Rescission_OnTheTransfer_CountsTheItemsRecordWhoseRefundIsNotBornYet_D906` | D-906 sayımı dönmenin kalem tavanında sınanır (100 TL indirim + iptalin doğmamış borcu düşülür, dönme kalanı öder) | yazıcı kalktı; UC-133'ün D-906 sayımı `ProcessAmountBasedRefund_..._KK7` |

- **Çıkış yolu ve geri alma:** şema ve göç yok (`ck_refunds_links_by_type` dönmede `return_id`'ye zaten izin veriyordu); geri alma bu commit'in `git revert`'üdür.
- **Güvenlik taraması:** yeni dış bağımlılık, sır, girdi yolu yok; anahtar sunucunun borçlu kayıt listesinden eşlenir (yabancı anahtar 404, eski davranış).

#### Öz denetim (bütün UE2b kalemleri ve KK payları)

| Denetim | Sonuç |
|---|---|
| D-1617 | ✓ — yorumlar (ilk işçi); `grep` eski üç model adını `src`'de bulmaz |
| D-1758 | ✓ — K-1629 (1), `ClosedReason`, `OrderPageTests..._D1758` (ilk işçi; Unit TAM ve bu koşu yeşil) |
| D-1759 | ✓ — K-1629 (2), `OrderConfirmationTests..._D1759` (ilk işçi) |
| D-892 "talebin teslim alması borçlu kayıt; UC-131 `RescindDefectClaimAsync` ile başlatır; sayaç ve satır aynı kuraldan; iz aynı işlemde; `ReduceDefectClaimPriceAsync` kaldırılır" | ✓ — `ClaimReceipts()` iki okuyucuda; `PanelRefundSteps` dalı; `RefundManually` aynı birimde; yazıcı ve denetimi kalktı; sayfa testi |
| D-892 kilit/sürüm çifte artışı | ✓ — test sürümün tam bir arttığını sınar (`version + 1`) |
| D-610 | ⚪ gerekçeli — süre; satırında not, hedef T604 |
| KK1 — envanter | ✓ — UE2b'nin 5 kalemi: 4 ✅, D-610 gerekçeli |
| KK2 — para ve güvenlik ayrı başlık, kırmızı–yeşil, eski testler | ✓ — yukarıdaki tablolar; değişen eski testler "spec gereği değişen testler" tablosunda gerekçeli |
| KK3 — tarayıcı | uygulanmaz — A parçasının (PR #594) |
| KK4 — D-1678 | uygulanmaz — bu dilimin kalemi değil |
| KK5 — satırlar ve sayaç | ✓ — D-892 yerinde ✅; D-1783 parça dosyasında; sayaca dokunulmadı |
| Sık FAIL (1) istisna geniş · (2) tarama çağrı biçimi | uygulanmaz — tarama değişmedi |
| (3) 06 varsayılan / FK | uygulanmaz — şema değişmedi |
| (4) aynı kuralı anan öteki yerler | ✓ — D-893 satırının "tek işlev" adı değişti (K-1629 (3) yazar); `OwedRefundRecord.ReturnId` ve `NextSubjectsAsync` "five kinds" özetleri altıya çekildi; test sınıf özetleri |
| (5) belge kuralı karar kaydısız | ✓ — K-1629 (3); belge metni değişmedi (`03` 8.3.2.1, 8.3.3.7, `07` UC-131 akışı zaten yazar) |
| Modül arayüzüne yeni yöntem, test düzenekleri | ✓ — `IAfterSalesModule.DefectClaimRescissionAmountAsync`'i uygulayan başka sınıf yok (`grep`); Unit TAM yeşil |
| `dotnet format` değişen `.cs` | ✓ — dalın 114 `.cs` dosyası (`--include`), değişiklik yok |

#### Test Sonuçları

- **Unit:** TAM 8097/8097 ✓.
- **Integration:** koşulan — `Ordering.ItemRecordsTests` (tamamı), `PanelHomeTests`, `CardRefundJobTests`, D-892 sayfa testi: 114/114 ✓; `OrderDetailPageTests.ProcessRefund*`, `ProcessAmountBasedRefund*`, `RequestRefundIban*`, `RetryCardRefund*` ve D-892: 25/25 ✓. TAM: CI.

#### Notlar

- ⚠ Kapanmış (çözülmüş) talebin teslim alması borçlu satırda görünmez; talep yeniden açılınca görünür (`03` 1.9.1; denetim açık talep ister). Sahip yerine karar: K-1629 (3).
- ⚠ D-893'ün bedel indirimi kalem tavanı yazıcıyla gitti; UC-133 `03` 8.3.3.7'nin sipariş tavanıyla kalır → D-1783 (ulaşılabilir davranış değişmedi).
- ⚠ `MarkTransferRefundSentAsync` artık izlenen satırı `FindAsync` ile bulur (aynı birimde başlatılıp gönderilen dönme için); kaydedilmiş satırlarda davranış aynı.
- ⚠ D-610 bu devama sığmadı (dört para yazıcısı ve sınamaları birlikte).

## Dilim UE4 — görsel ucu, arama motoru başlığı, şube görseli, IBAN beyanı (para ve güvenlik)

İki işçide yapıldı (ilki süre bütçesiyle bölündü, devir notu `devir-UE4.md`); bu bölümün tabloları iki işçinin bütün işini kapsar.

### Yapılan İşler

| Kalem | Değişiklik |
|---|---|
| D-1136, D-1711 | `Endpoints/Files/ImageEndpoint.cs` (yeni): `GET /gorsel/{id:guid}` → `ICatalogModule.FindServedImageAsync` (`ImageUploads.FindServedAsync`, `Contracts/ServedImage.cs`); kaynak kopya görsel bölümünden, P-22 medya türü, `Cache-Control: public, max-age=31536000, immutable`, doğrulayıcı başlık yok; satır yok / dosya yok / dijital dosya kimliği → gövdesiz 404. `AddressRules.ImageArea` tek yer (`FrameStyle.ImagePrefix`, `RateLimitScope._imagePath`, `EmailLinks.ImagePrefix` — son ikisi bu devamda). `deploy/caddy/Caddyfile`: `/gorsel/*` önce dosya, yoksa uygulamaya. Tarayıcı envanterinin `/gorsel/` 404 ayıklaması kalktı (`ScreenInventoryTests`, `EmptyStateTests`) |
| D-357 | `Infrastructure/Security/StructuredData.cs` (yeni): `Render(title, metas, blocks)` başlığı, meta değerlerini ve JSON-LD bloklarını yazar ve kodlar; `Serialize(JsonObject)`. `SearchEngineData.Head` yalnız değer listesi kurar. Tarama `RichTextSourceScan.StructuredDataFile`; `09` 15.3 madde 1 (v0.37) |
| D-1319 (1) | `ProductConstants.ImageMediaTypes`; `UploadLimits.Accept` ondan |
| D-1319 (2) | yapılmadı — UE5 (K-1631 (3); `04` 7.2.4.10 belge kararı ister) |
| D-1539 | `BranchImageInput`; `ContentAuthoring.CreateBranchAsync/SaveBranchAsync` görselli aşırı yüklemeler (alanlar + görsel tek yazım); `SetBranchImageAsync`, `PanelContentEditing.SaveBranchImageAsync` kalktı |
| D-1283 (2) | `AfterSalesModule.EnterIban`: bir isteğin tek işlemindeki kayıtlar (çok satırlı iptal, gecikme feshi, çok kalemli cayma) tek `RefundIban` satırını paylaşır (K-1631 (4)); (1) A1'de (PR #594) |
| D-1599 | karar K-1631 (5) (`02` 10.4.10 kazanır); kod ve belge hizası UE5 — 422'nin yerleri PR #594'ün eşitlik tablolarında |
| D-1374 | karar K-1631 (6) (UC-157 doğrulanmamış kaydı da siler); kod UE5 (süre) |
| Kayıtlar | K-1631 (altı madde); `05` v0.54, `07` v0.47, `09` v0.37; `DEFERRED_BACKLOG.md` 8 satır yerinde |

### Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Endpoints/Files/` — `ImageEndpoint.cs` (yeni), `FileEndpoints.cs`
- `src/Shopfolio.Web/Infrastructure/` — `Security/StructuredData.cs` (yeni), `Security/RequestRateLimiting.cs`, `SearchEngine/SearchEngineData.cs`, `SearchEngine/SearchEngineValues.cs`, `Routing/AddressRules.cs`, `ProductConstants.cs`, `EmailComposition/EmailLinks.cs`
- `src/Shopfolio.Web/Modules/` — `Catalog/` (`CatalogModule`, `ICatalogModule`, `ServedImage`, `ImageUploads`), `CorporateContent/` (`ContentAuthoring`, `ContentGallery`, `ContentInputs`, `PanelContentEditing`), `AfterSales/AfterSalesModule.Cancellations.cs`, `AfterSales/AfterSalesModule.ItemRecords.cs`
- `src/Shopfolio.Web/Pages/` — `Components/UploadComponentModels.cs`, `Frame/FrameStyle.cs`; `deploy/caddy/Caddyfile`
- Testler: `Files/ImageEndpointTests.cs` (yeni), `CorporateContent/PanelContentEditingTests.cs`, `CorporateContent/ContentRelationsTests.cs`, `Ordering/BranchFlowTests.T604.cs` (yeni), `Browser/ScreenInventoryTests.cs`, `Browser/EmptyStateTests.cs` (yalnız ayıklama satırları); Unit `RichTextSourceScan(Tests)`, `SitemapSourceTests`, `ControlComponentModelTests`
- Belgeler: `05` 7.7.1, `07` 3.1.6, `09` 15.3, `PRODUCT_DISCOVERY_STATUS.md` (K-1631, belge tablosu), `DEFERRED_BACKLOG.md`, `STATUS_CHANGELOG.d/T604-U.md`

### Para ve güvenlik kalemleri (KK2)

Kırmızı: kalemin `src` değişikliği geçici geri alındı ya da devre dışı bırakıldı, yeni test koşuldu, sonra değişiklik geri kondu ve sınıf yeşil koşuldu.

| Kalem | Test | Kırmızı (geçici bozma) | Yeşil |
|---|---|---|---|
| D-1136 | `ImageEndpointTests.TheAddressAPageWrites_AnswersTheRowsFile_WithP22sTypeOfItsFormat_CachedForGood_D1136` | `MapImageEndpoint` kapalı: OK beklendi, 404 (gövdeli sayfa) | ✓ |
| D-1711 | `ImageEndpointTests.AnIdOfNoRow_ARowWithoutItsFile_AndADigitalFilesKey_AreABare404_D1711` | aynı bozma: gövdeli 404 — kırmızı | ✓ |
| D-1539 | `PanelContentEditingTests.ABranchSave_WritesItsFieldsAndItsImageInOneWrite_TheVersionGoesUpOnce_D1539` | `ApplyBranchImage` atlandı: kırmızı | ✓ |
| D-357 | Unit `SearchEngineDataTests.Head_AValueWithQuotesAndTags_IsEncodedInTheTitleAndTheAttributes`, `RichTextSourceScanTests.TheSearchEngineDataFile_HasNoExceptionOfItsOwn_D357` | `StructuredData` başlık/içerik kodlaması kaldırıldı: kırmızı | ✓ |
| D-1319 (1) | Unit `ControlComponentModelTests.TheImageMediaTypes_AreP22sFormats_InTheirOrder_AndTheAcceptOfEveryImageSurface_D1319` | `ImageMediaTypes` sırası değişti: kırmızı | ✓ |
| D-1283 (2) | `BranchFlowTests.TwoLinesCancelledInOneRequest_ShareOneIbanRow_AndALaterRequestEntersItsOwn_D1283` | `AfterSales` değişikliği geri alındı: "Assert.Single() Failure: The collection contained 2 items" | ✓ |

- **Var olan testler:** `ContentRelationsTests` yardımcısı `BranchImageAsync` ayrı görsel yazımından tek yazıma çevrildi (kaldırılan API; sonuçlar aynı). Tarayıcı sınıflarında yalnız `/gorsel/` 404 ayıklaması kalktı ve eksik görsel sıfır istenir. Öteki testler değişmedi.
- **Çıkış yolu ve geri alma:** şema ve göç yok; geri alma commit'lerin `git revert`'üdür. Görsel ucu kapatılırsa ters vekil diskteki dosyaları sunmaya devam eder (kimlikli adresler 404'e döner); paylaşılan IBAN satırı mevcut satırlara dokunmaz, yalnız yeni yazımları etkiler.
- **Güvenlik taraması:** görsel ucu yalnız görsel bölümünü okur (`FileArea.Image`), dijital dosya kimliği 404 (testli), çerez ve ayar okumaz, istek sınırlarının dışındadır (`/gorsel`, belgeyle aynı); `HtmlString` yalnız `RichText.cs` ve `StructuredData.cs`'de (tarama); IBAN paylaşımı yalnız aynı işlem, aynı sipariş ve aynı IBAN metniyle — başka siparişin ya da önceki beyanın satırı paylaşılmaz; silme son bekleyen geri ödemeyi bekler (`RefundIbanUses`). Yeni bağımlılık, sır, girdi yolu yok.

### Kabul Kriterleri Kontrolü (UE4 payı)

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 — UE4'ün kalemleri kapalı ya da gerekçeli | ✓ | 5 ✅ (D-357, D-1136, D-1711, D-1539, D-1283); D-1319 (2), D-1599, D-1374 gerekçeyle UE5, hedef T604 — satırlarında, K-1631 (3), (5), (6) |
| KK2 — para ve güvenlik kalemleri ayrı başlıkta; kırmızı–yeşil; eski testler aynı | ✓ (tur 1 düzeltmesiyle) | yukarıdaki tablo; tek değişen eski yardımcı kaldırılan API'den. **Düzeltme (tur 1, Bulgu 1):** bu satır ilk yazıldığında "eski testler aynı" tutmuyordu — `PageFormPatternTests.EveryMappedEndpoint_StartsWithAFirstSegmentOfTheClosedList` yeni `/gorsel/{id:guid}` ucuyla kırılıyordu (sınıf bu dilimde koşulmamıştı). Ürün kodu düzeldi (`AddressRules.ImageAddressTemplate`, dar istisna, K-1636 (1)); testin var olan denetimleri değişmedi, yalnız görsel kalıbının eşlendiğini sınayan bir `Assert.Contains` eklendi ("UE düzeltme tur 1") |
| KK3 — tarayıcı kalemleri | ✓ (pay) | `ScreenInventoryTests` + `EmptyStateTests` chromium iki yolda 49/49; firefox/webkit **TAM: CI E2E** |
| KK4 — D-1678 | — | A parçası |
| KK5 — kapanan satır `✅ Çözüldü → T604`; sayaç | ✓ | 5 satır ✅, 3 satır gerekçeli not; yeni kalem yok; sayaca dokunulmadı (talimat) |
| `09 §25` ilgili satırlar | ✓ | tek yer: `AddressRules.ImageArea`, `ImageMediaTypes`, `StructuredData`, `EnterIban`; döngüde sorgu yok (paylaşım bellekte, işlem kimliğiyle) |
| Güvenlik taraması | ✓ | yukarıda |

### Öz denetim (bütün UE4 kalemleri ve KK payları)

Önceki işçinin yaptıkları dahil `11` T604 kabul kriterleri ve kontrol listesi diff'te yeniden denetlendi; `sik-fail-nedenleri.md` okundu.

| Denetim | Sonuç |
|---|---|
| D-357 "başlık ham işaretlemeyi tek yerde kurar, `09` 15.3 ile aynı" | ✓ — `StructuredData` tek yazıcı; `09` imzası ve kodlayıcı ifadesi hizalandı (önceki metin koddan farklıydı) |
| D-1136 "`/gorsel/{kimlik}` varsayımı belgeye bağlı" | ✓ — `05` 7.7.1, `07` 3.1.6, K-1631 (1); Caddy ve uç |
| D-1711 "uç var, tohumlu görseller 404 değil" | ✓ — uç testi; tarayıcı envanteri eksik görseli sıfır ister (chromium iki yol yeşil) |
| D-1539 "sürüm bir kez artar" | ✓ |
| D-1319 (1) "MIME P-22 yanında tek yer" | ✓; (2) gerekçeli UE5 |
| D-1283 (2) "istek başınaysa paylaşılır, karar kaydıyla" | ✓ — `06` 4.31 istek başı; K-1631 (4); test; (1) PR #594 |
| D-1599 "karar + belge hizası + VAL-A238 koşumu" | ⚪ karar verildi (K-1631 (5)); hizası ve koşumu UE5 — A'nın eşitlik tabloları ⚠ |
| D-1374 "karar ve olası UC-157 kodu birlikte" | ⚪ karar verildi (K-1631 (6)); kod UE5 (süre) ⚠ |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ — `SearchEngineData`'nın kendi istisnası kalktı, test bunu sabitler; görsel ucu istek sınırı istisnası belgedeki `/gorsel` öneki kadar |
| (2) tarama çağrı biçimi | ✓ — `HtmlString` taraması iki dosya; yeni çağrı biçimi yok |
| (3) 06 varsayılan / FK dizini | uygulanmaz — şema değişmedi |
| (4) aynı kuralı anan öteki yerler | ✓ — `/gorsel` öneki üç kodda tek sabitten (bu devamda `RequestRateLimiting`, `EmailLinks` düzeltildi); `09` 15.3 madde 7 ile madde 1 tutarlı |
| (5) belge kuralını değiştiren metin karar kaydısız | ✓ — `05`, `07`, `09` değişiklikleri K-1631'i anar; yeni kural yok |
| Unit TAM | ✓ — 8098/8098 |
| `dotnet format` değişen `.cs` | ✓ — dalın 137 dosyası (önceki işçi) + bu devamın 5 dosyası, değişiklik yok |
| Branch izolasyonu | ✓ — A parçasının sınıflarında yalnız iki tarayıcı ayıklama satırı (talimatla); `FormFieldPlaceEnforcement` bu yüzden değişmedi (D-1599) |

### Test Sonuçları

- **Unit:** TAM 8098/8098 ✓.
- **Integration:** koşulan sınıflar — önceki işçi: `ImageEndpointTests`, `PanelContentEditingTests`, `ContentRelationsTests`, `Web.SearchEngineDataTests`, `FaqPageTests`, `StorefrontHomeAndCategoryTests`, `ContentPagesTests`, `DownloadEndpointTests` (113/113); bu devam: `BranchFlowTests`, `CustomerCancellationTests`, `WithdrawalDeclarationTests`, `ItemRecordsTests`, `OrderCorrectionTests`, `CustomerOperationsTests`, `ImageEndpointTests` (219/219), `RequestLimitsTests`, `CustomerNoticeQueueTests`, `AccountEmailQueueTests`, `OrderDetailPageTests` (158/158). **TAM: CI.**
- **E2E / tarayıcı:** `ScreenInventoryTests`, `EmptyStateTests` chromium, betikli ve betiksiz, 49/49; firefox/webkit **TAM: CI E2E**.

### Commit & PR

- Commit'ler: 0e82245c ve bu devamın commit'i
- PR: #595

### Dış Varsayımlar

Bu dilimde dış varsayım yok.

### Notlar

- ⚠ D-1599 kod ve belge hizası UE5'e: 422'nin yerleri A parçasının (PR #594) değiştirdiği `FormFieldPlaceEnforcement` tablosunda; A'nın test sınıflarına dokunulmadı. Karar A1'in önerisiyle aynı.
- ⚠ D-1374 kodu UE5'e (süre); karar A2'nin ve UE3 devir notunun önerisiyle aynı.
- ⚠ D-1283 (1)'in kanıtı PR #594'te; satır iki parçanın birleşmesiyle tam kanıtlıdır.
- ⚠ D-1539: görsel varlık denetimi artık alan denetiminden önce (tek yazım); ulaşılan ret aynı.
- ⚠ A parçası da `07`/`09` sürümlerini yükseltti; sürüm satırı çakışması birleştirmede çözülür.
- STATUS T604 tablo satırına dokunulmadı.

---

## Dilim UE5 — müşteri yazıcılarının işlem kararı, doğrulanmamış kaydın silinmesi, son sonucu belirsiz işaretleri (para ve güvenlik)

İki işçide yapıldı (ilki kodu ve testleri yazdı, süre bütçesiyle bölündü — devir notu `devir-UE5.md`; ikincisi kayıtları, kırmızı–yeşil koşularını, D-894 ve D-1319 (2) kapanışını ve UE kapanışını yaptı); bu bölümün tabloları iki işçinin bütün işini kapsar.

### Yapılan İşler

| Kalem | Değişiklik |
|---|---|
| D-610 | Karar K-1633 (1). Müşteri iptali (`CancelItemAsync`) ve cayma (`DeclareWithdrawalAsync`) kararı zaten soruyordu. Eklendi: `OrderTransitions.ItemRecords.OpenDefectClaimAsync` (adet birden küçükse `InvalidInput`, sonra `CheckAsync(OpenDefectClaim, kalem, adet)` + `VerdictRefusal`), `ReopenDefectClaimAsync` müşteri dalı (`ReopenDefectClaimByCustomer`; adet yazıcıda), `OrderTransitions.RefundLine.EnterRefundIbanAsync` (kalemli istekte `EnterRefundIban`), `ChangeRefundIbanAsync` (`CorrectRefundIban`). AfterSales yazıcılarının denetimleri aynı kural işlevleriyle (`AfterSalesPeriodRules`, `ItemQuantities`) modül savunması olarak kaldı |
| D-1374 | Karar K-1631 (6), ayrıntı K-1633 (3). `AccountDeletions.DeleteByAdministratorAsync`: hesap kilidinden sonra doğrulanmamış satır varsa iz satırı + satır silinir + `DisposalRecordType.PendingRegistration`, e-posta yok, `Deleted(0)`; yoksa eski yol. `Members.cshtml(.cs)`: doğrulanmamış hâlde aynı düğme ve onay; ikisi de yoksa 404. `07` UC-157 amaç, 404, yan etkiler (v0.48) |
| D-137 | `[OutcomeUncertain]` UC-39, UC-40, UC-41 (`OrderScreenModels`), UC-43 (`AfterSalesScreenModels`), UC-53 (`Storefront/Verification`), UC-165 (`FirmIdentity` askıya alma), UC-177 (`AdminAccounts` kaldırma), UC-186 (`Panel/Verify`), UC-187 (`Panel/Revert`); `CustomerOperationPageModel` ve `WithdrawalPageModel` `IOutcomeTarget` → `RootPath` (`07` 6.4.2 "→ kök"); `NotMarkedYet` silindi |
| D-1783 | Bilinçli kabul, K-1633 (2): kaleme bağlı UC-133 `03` 8.3.3.7'nin sipariş tavanıyla sınırlıdır; `PanelRefundSteps.ProcessAmountBasedRefundAsync` açıklama satırı; kod davranışı değişmedi |
| D-894 | Kod yok: D-1525'in kararı (K-1624 (4)) `12` VAL-A239 ve VAL-A244'ün beklenen sonucunu UE2'de yazdı (`12` v0.37, satır 780 ve 790) — teyit isteğe alan olarak girmez, beyansız çağrı düzeltme yapmaz, teyidin değeri saklanmaz; saklama gerekmez. Satır ✅ |
| D-1319 (2) | Bilinçli kabul, K-1633 (4): dijital dosyanın boyut reddi `InvalidImage` koduyla kalır — `04` 7.2.4.10 onu 7.3.2.18'in tek kodlu sınıfına bağlar (`FormFieldEnforcement` `7.3.2.18 → Image`); kod ve belge değişmez |
| D-1599 | Daraltıldı (yönetici kararı): T604 U, PR #594 merge'ünden sonra UH dalında (`task/T604-urun-kodu-h`) — 422'nin yerleri #594'ün eşitlik tablolarında |
| D-1773 | Daraltıldı: T604 U, UH dalı — yazmayan reddin yeniden gösterimi, para yazıcısına dokunmaz |
| Kayıtlar | K-1633 (dört madde); `07` v0.48; `DEFERRED_BACKLOG.md` 6 satır yerinde (D-137, D-610, D-894, D-1319, D-1374 ✅; D-1599 daraltıldı); `DEFERRED_BACKLOG.d/T604-U.md` D-1783 ✅, D-1773 daraltıldı, yeni D-1806; `STATUS_CHANGELOG.d/T604-U.md` |

### Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/` — `Ordering/OrderTransitions.ItemRecords.cs`, `Ordering/OrderTransitions.RefundLine.cs`, `Ordering/PanelRefundSteps.cs` (yorum), `Membership/AccountDeletions.cs`
- `src/Shopfolio.Web/Pages/` — `Orders/OrderScreenModels.cs`, `Orders/AfterSalesScreenModels.cs`, `Panel/Members.cshtml(.cs)`, `Panel/AdminAccounts.cshtml.cs`, `Panel/FirmIdentity.cshtml.cs`, `Panel/Verify.cshtml.cs`, `Panel/Revert.cshtml.cs`, `Storefront/Verification.cshtml.cs`
- Testler: Integration `Web/MemberRecordPageTests.cs`, `Model/MembershipAccountDeletionTests.cs` (`MembershipTests`); Unit `Consistency/OutcomeUncertainConsistencyTests.cs`
- Belgeler: `07` UC-157 (v0.48), `PRODUCT_DISCOVERY_STATUS.md` (K-1633, belge tablosu), `DEFERRED_BACKLOG.md`, `DEFERRED_BACKLOG.d/T604-U.md`, `STATUS_CHANGELOG.d/T604-U.md`

### Para ve güvenlik kalemleri (KK2)

Kırmızı: kalemin `src` değişikliği geçici bozuldu, test koşuldu, sonra değişiklik geri kondu ve sınıf yeşil koşuldu (2026-11-21, `t604u-pg`, PostgreSQL 18 `track_commit_timestamp=on`).

| Kalem | Test | Kırmızı (geçici bozma) | Yeşil |
|---|---|---|---|
| D-610 | `ItemRecordsTests.OpenDefectClaim_OnAnUndatedPhysicalItem_FollowsTheChannelRule_DeliveryFailedIncluded` (4 durum) | AfterSales `OpenDefectClaimAsync`'in kanal denetimi ve `OrderTransitions`'ın yeni kararı birlikte devre dışı: `(status: Preparing, opens: False)` "Expected: False, Actual: True" | ✓ — yalnız modülün kanal denetimi devre dışıyken (karar yerinde) 4/4 yeşil: kuralı karar uygular; denetim geri konunca da yeşil |
| D-1374 | `MemberRecordPageTests.AnUnverifiedRegistration_IsDeletedOnTheRequest_WithTheConfirmation_ItsOwnDisposalType_TheTrailRow_AndNoEmail_D1374`; `MembershipTests.DeleteOnRequest_WithoutAnActor_Throws_AnUnverifiedRegistrationToo_D1374` | doğrulanmamış kayıt dalı kapatıldı (sorgu hiç satır bulmaz): "Expected: SeeOther, Actual: NotFound"; "Assert.Throws() Failure: No exception was thrown" | ✓ |
| D-137 | Unit `OutcomeUncertainConsistencyTests.MarkedHandlers_EqualTheUseCaseListOf07_InBothDirections_EachOnOneHandler_D137` | `Panel/Revert`'in `[OutcomeUncertain("UC-187")]` işareti kaldırıldı: "Collections differ at index 7 — Expected: UC-187" | ✓ |
| D-1783 | `OrderDetailPageTests.ProcessAmountBasedRefund_AnEmptyZeroOrExceedingAmount_Is422_AndWritesNothing_KK7` (sipariş tavanı) | — kod değişmedi (bilinçli kabul, yalnız yorum); kırmızı kurulacak değişiklik yok | ✓ (var olan test) |

- **Var olan testler:** `MemberRecordPageTests.TheResult_...KK2` (bekleyen kayıtta düğme artık VAR) ve `ADeletionOfAnAccountThatIsNotThere_Answers404_InBothSteps_KK7` (yeniden adlandı, bekleyen kayıt yerine olmayan kimlik) K-1631 (6) gereği değişti; öteki testler değişmedi — `ItemRecordsTests`, `CustomerOperationsTests`, `MemberRecordPageTests`, `MembershipTests` 255/255.
- **Çıkış yolu ve geri alma:** şema ve göç yok; geri alma commit'lerin `git revert`'üdür. D-610'da karar yazıcının önündedir, modül denetimi yerinde kaldığı için karar geri alınsa da yazıcı aynı kuralla reddeder. D-1374 geri alınırsa doğrulanmamış kayıt yine 404 alır (silinmiş kayıtlar geri gelmez; imha kaydı yazılmıştır).
- **Güvenlik taraması:** silme yalnız yönetici oturumuyla, onaylı adımda ve aynı iz satırıyla (aktörsüz çağrı istisna — test); doğrulanmamış kayıtta e-posta gönderilmez (doğrulanmamış adrese yazılmaz). Karar kilitli siparişte okunur (yarışta bayat karar yok). Yeni bağımlılık, sır, girdi yolu yok.

### Kabul Kriterleri Kontrolü (UE5 payı)

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 — UE5'in kalemleri kapalı ya da gerekçeli | ✓ | 6 ✅ (D-610, D-1374, D-137, D-894, D-1319, D-1783); D-1599, D-1773 gerekçeyle UH dalına, hedef T604 |
| KK2 — para ve güvenlik kalemleri ayrı başlıkta; kırmızı–yeşil; eski testler aynı | ✓ | yukarıdaki tablo; değişen iki eski test spec gereği (K-1631 (6)) |
| KK3 — tarayıcı kalemleri | — | UE5'te tarayıcı kalemi yok; A parçası |
| KK4 — D-1678 | — | A parçası |
| KK5 — kapanan satır `✅ Çözüldü → T604`; daraltılan satır yeni hedef + gerekçe; sayaç | ✓ | 6 satır ✅, 2 satır daraltıldı, yeni D-1806 parça dosyasında; sayaca dokunulmadı (talimat) |
| `09 §25` ilgili satırlar | ✓ | tek kural evi: düğme ve yazıcı aynı karar (`CheckAsync`); işaret kümesi tek liste (`07` 6.4.2) |
| Güvenlik taraması | ✓ | yukarıda |

### Öz denetim (UE5)

| Denetim | Sonuç |
|---|---|
| D-610 "yazıcıların kendi koşul denetimleri karara indirilir (tek işlev)" | ✓ — dört müşteri yazıcısı kararı sorar; modül denetimi aynı kural işlevini çağırır (ikinci kural evi değil, K-1633 (1)); kalemsiz IBAN isteği ⚠ D-1806 |
| D-1374 "karar ve UC-157 kodu birlikte" | ✓ — K-1631 (6) + K-1633 (3), kod, `07`, iki test |
| D-137 "son task listeyi boşaltır ve siler" | ✓ — `NotMarkedYet` yok, dokuz işaret, test iki yönlü |
| D-1783 "karar kaydıyla kalem tavanı ya da sipariş tavanının yettiği bilinçli kabul" | ✓ — K-1633 (2), yorum, var olan test |
| D-894 "`12` hücreleri D-1525'in kararıyla netleşir" | ✓ — `12` VAL-A239/A244 (v0.37); `06` saklama yok |
| D-1319 (2) "dijital dosya ekranında uygun kod seçilir" | ✓ — K-1633 (4) bilinçli kabul (belge tek kod ister) |
| Sık FAIL (1) bekçi/tarama istisnası geniş | uygulanmaz — yeni istisna yok; `NotMarkedYet` istisnası kalktı |
| (2) tarama çağrı biçimi | ✓ — işaret taraması yansımayla (öznitelik biçimi değişmedi) |
| (3) 06 varsayılan / FK dizini | uygulanmaz — şema değişmedi |
| (4) aynı kuralı anan öteki yerler | ✓ — `07` UC-157'nin amaç, 404 ve yan etki cümleleri birlikte; `07` sürüm satırı ve `PRODUCT_DISCOVERY_STATUS.md` belge tablosu v0.48 |
| (5) belge kuralını değiştiren metin karar kaydısız | ✓ — `07` metni K-1633 (3)'ü anar |
| Kısmen karşılanan kriter | yok — açık iki kalem gerekçeli daraltıldı |

### Test Sonuçları

- **Unit:** TAM 8098/8098 ✓ (`--filter-trait Consistency=true` 667 + `--filter-not-trait Consistency=true` 7431; tek çağrı 590 sn'yi aşıyor).
- **Integration:** koşulan sınıflar `ItemRecordsTests`, `CustomerOperationsTests`, `MemberRecordPageTests`, `MembershipTests` — 255/255 (tek çağrı). **TAM: CI.**
- **`dotnet format`:** dalın değiştirdiği 149 `.cs` dosyası, değişiklik yok.
- **E2E / tarayıcı:** UE5'te tarayıcı kalemi yok.

### Commit & PR

- Commit'ler: c7f55efd (kod), dfdf0435 (kayıtlar) ve kapanış commit'i
- PR: #595

### Dış Varsayımlar

Bu dilimde dış varsayım yok.

### Notlar

- ⚠ D-610'un kırmızısı ayıp talebi açma yolunda kuruldu; IBAN girişi, düzeltmesi ve müşterinin yeniden açması için ayrı kırmızı yok — modülün denetimi yerinde olduğundan karar kaldırılınca uçtan ret değişmez; o yolların sınaması var olan testlerin değişmeden yeşil kalmasıdır (`CustomerOperationsTests`, `ItemRecordsTests`).
- ⚠ D-1806 (yeni, para): kalemsiz geç ödeme IBAN isteği kararın olgusunda yok; hedef UH dalı, kendi kırmızı–yeşil başlığıyla — UE5 son UE dilimidir.
- ⚠ D-1773 UH dalına: yazmayan reddin yeniden gösterimi, para yazıcısına dokunmaz.
- ⚠ D-1599 UH dalına (yönetici kararı): PR #594 açık.
- ⚠ A parçası (`K-1632`) da `07` sürümünü yükseltti (v0.46); sürüm satırı çakışması birleştirmede çözülür.
- STATUS T604 tablo satırına dokunulmadı.

---

## UE kapanışı — UE1–UE5'in bütün kalemleri

UE dilimleri (para ve güvenlik) bu dilimle kapandı. Tablo bütün UE kalemlerini (önceki işçilerin yaptıkları dahil) satır başına denetler; her ✅ satırın kanıtı kendi diliminin "Para ve güvenlik kalemleri (KK2)" tablosundadır. Backlog satırları 2026-11-21'de yeniden okundu: aşağıdaki ✅'lerin hepsi `✅ Çözüldü → T604` taşır.

| Kalem | Dilim | Durum | Kanıt / gerekçe ve yeni hedef |
|---|---|---|---|
| D-568 | UE1 | ✅ | `ItemRecordRulesTests..._D568` (kırmızı–yeşil UE1) |
| D-906 | UE1 | ✅ | `ItemRecordsTests.PriceReduction_..._D906` |
| D-1195 | UE1 | ✅ | `AfterSalesScreenTests.TheReturnRejection_..._D1195` |
| D-1281 | UE1 | ✅ | `LateDeliveryAndFirmCancellationTests..._D1281` |
| D-1382 | UE1 | ✅ | `AfterSalesScreenTests.APendingRefundBornAfterItsRecord_..._D1382` |
| D-1439 | UE1 | ✅ | K-1622; `AfterSalesScreenTests..._D1439`, `OrderScreenTextsTests..._D1439` |
| D-1440 | UE1 | ✅ | `AfterSalesScreenTests.TheDeclarationsOfOneRequest_..._D1440` |
| D-1588 | UE1 | ✅ | `ItemRecordsTests.ThePayeeRead_..._D1588` |
| D-1598 | UE1 | ✅ | `OrderDetailPageTests.CardRemovalWindow_..._D1598`, `OrderCorrectionTests.PanelRemoval_..._D1598` |
| D-1702 | UE1 | ✅ | `AfterSalesScreenTests.TheWithdrawalsEmptyIban_..._D1702` |
| D-1450 | UE2 | ✅ | `OrderPageTests..._D1450`, `CheckoutPageTests..._D1450` |
| D-1246 | UE2 | ✅ | Unit `CartRulesTests..._D1246` |
| D-1197 | UE2 | ✅ | K-1624 (1); `OrderPageTests.AClosedPaidDigitalItem_..._D1197_D1259` |
| D-1259 | UE2 | ✅ | aynı test (ayrı kırmızı koşu) |
| D-1542 | UE2 | ✅ | `PanelOrderGate`; `OrderDetailPageTests` + `OrderCorrectionTests` değişmeden 165/165 |
| D-1544 | UE2 | ✅ | `OrderDetailPageTests.AFormValueThatDoesNotBind_..._D1544` |
| D-1401 | UE2 | ✅ | K-1624 (3); `AfterSalesScreenTests.AForeignItem_..._D1401` |
| D-769 | UE2 | ✅ | `OrderConfirmationTests.ThePaymentStepsSummary_..._D769` |
| D-650 | UE2 | ✅ | K-1624 (2); Unit `ShippingChargeTests..._D650` |
| D-1525 | UE2 | ✅ | K-1624 (4); `OrderCorrectionTests` KK4, KK7; `12` VAL-A239/A244 |
| D-610 | UE2 → UE2b → UE5 | ✅ | K-1633 (1); UE5 KK2 |
| D-892 | UE2 → UE2b | ✅ | K-1629 (3); `OrderDetailPageTests.AClaimsGoodsReceipt_..._D892` |
| D-141 | UE3 | ✅ | K-1627 (1); Unit `FieldMessageCatalogTests..._D141` |
| D-608 | UE3 | ✅ | `MembershipTests.DeleteOwn_..._KK1` (spec gereği değişti) |
| D-1190 | UE3 | ✅ | K-1627 (2); `CartPageTests..._D1190`, `CheckoutPageTests..._D1190` |
| D-1317 | UE3 | ✅ | dört sınıfta `..._D1317` |
| D-1375 | UE3 | ✅ | istisna dalı; kırmızı kurulamaz (dal kilit altında erişilemez) ⚠ UE3 |
| D-1389 | UE3 | ✅ | K-1627 (4); `PanelHomeAndVerificationPagesTests..._D1389_D1454` |
| D-1454 | UE3 | ✅ | K-1627 (4); üç sayfa + istisna kümesi (Unit `ReauthenticationFormPageTests`) |
| D-1473 | UE3 | ✅ | `MyAccountPageTests..._D1473` |
| D-1474 | UE3 | ✅ | K-1627 (3); `AccountProfilePageTests..._D1474`, `MyAccountPageTests..._D1474` |
| D-1508 | UE3 | ✅ | `EmailRevertPageTests..._D1508` |
| D-1374 | UE3 → UE4 → UE5 | ✅ | K-1631 (6), K-1633 (3); UE5 KK2 |
| D-1617 | UE2b | ✅ | yorumlar; `grep` eski üç model adını `src`'de bulmaz |
| D-1758 | UE2 (yeni) → UE2b | ✅ | K-1629 (1); `OrderPageTests..._D1758` |
| D-1759 | UE2 (yeni) → UE2b | ✅ | K-1629 (2); `OrderConfirmationTests..._D1759` |
| D-1783 | UE2b (yeni) → UE5 | ✅ | K-1633 (2) bilinçli kabul; sipariş tavanı testi |
| D-357 | UE4 | ✅ | K-1631 (2); Unit `SearchEngineDataTests`, `RichTextSourceScanTests..._D357` |
| D-1136 | UE4 | ✅ | K-1631 (1); `ImageEndpointTests..._D1136` |
| D-1711 | UE4 | ✅ | `ImageEndpointTests..._D1711`; tarayıcı envanteri eksik görsel sıfır |
| D-1539 | UE4 | ✅ | `PanelContentEditingTests..._D1539` |
| D-1283 | UE4 | ✅ | (2) K-1631 (4), `BranchFlowTests..._D1283`; (1) PR #594'te (A1) |
| D-1319 | UE4 → UE5 | ✅ | (1) Unit `ControlComponentModelTests..._D1319`; (2) K-1633 (4) bilinçli kabul |
| D-137 | UE5 (A6'dan) | ✅ | UE5 KK2 |
| D-894 | UE5 (A6'dan) | ✅ | K-1624 (4); `12` VAL-A239/A244 |
| D-1599 | UE4 → UE5 | ⚪ daraltıldı | K-1631 (5) kararı verildi; kod ve belge hizası + VAL-A238 koşumu → **T604 U, PR #594 merge'ünden sonra UH dalı** (`task/T604-urun-kodu-h`): 422'nin yerleri #594'ün eşitlik tablolarında |
| D-1773 | UE3 (yeni) | ⚪ daraltıldı | → **T604 U, UH dalı**: yazmayan reddin yeniden gösterimi (ortak kanca + sayfa envanteri), para yazıcısına dokunmaz |
| D-1806 | UE5 (yeni) | ⚪ yeni | → **T604 U, UH dalı** (para, kendi kırmızı–yeşil başlığıyla): kalemsiz geç ödeme IBAN isteği kararın olgusunda yok; davranış doğru (modül denetimi) |

**Hedefi T604 olan öteki kalemler (U-E dışı):** A parçasının kalemleri (A1–A7: D-697, D-981, D-1167, D-1284, D-1368, D-1424, D-1494, D-1676, D-1725, D-464, D-572, D-801, D-1289, D-1301, D-1394, D-1405, D-1723, D-1005, D-1020, D-1120, D-1130, D-1228, D-1418, D-1433, D-1455, D-1456, D-1678, D-1704, D-1709, D-1710, D-1732, D-1736, D-1746, D-146, D-1226, D-1285, D-1614, D-1626, D-1627, D-1636, D-1656, D-1657, D-1661, D-1671, D-1140, D-1244, D-1311, D-1348, D-1363, D-1366, D-1397, D-1452, D-1471, D-1677, D-1546, D-1547) PR #594'te; UH dilimlerinin kalemleri (UH1–UH3: D-138, D-1022, D-1134, D-1135, D-1143, D-1146, D-1173, D-1174, D-1216, D-1387, D-1550, D-283, D-1189, D-1243, D-1245, D-1275, D-1276, D-1398, D-1400, D-1631, D-1673, D-1703, D-1705, D-442, D-880, D-1330, D-1332, D-1334, D-1365, D-1386, D-1412, D-1464, D-1604, D-1701, D-1329) dal `task/T604-urun-kodu-h`'de. Bu PR onlara dokunmaz; satırları kendi parçalarının raporunda denetlenir.

### KK1–KK5 payları (UE bütünü)

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 — envanter; her kalem kapalı ya da gerekçeli | ✓ | yukarıdaki tablo: 45 ✅, 2 daraltıldı + 1 yeni, üçü yeni hedefli (UH dalı) ve gerekçeli |
| KK2 — para ve güvenlik kalemleri ayrı başlıkta; kırmızı–yeşil; eski testler aynı | ✓ (tur 1 düzeltmesiyle) | her dilimin "Para ve güvenlik kalemleri (KK2)" tablosu; spec gereği değişen eski testler dilim tablolarında adıyla (UE3 `DeleteOwn_..._KK1`, UE2b bedel indirimi yazıcısı, UE4 `ContentRelationsTests` yardımcısı, UE5 iki `MemberRecordPageTests` testi); kırmızısı kurulamayan iki kalem gerekçeli (D-1375 erişilemez dal, D-1783 kod değişmedi). **Düzeltme (tur 1):** doğrulama tur 1'de CI'da kırılan üç eski test (`OneTimeValueTests` iki KK3 testi, `PageFormPatternTests` uç kalıbı) spec gereği değişmedi; ürün/örnek kodu düzeldi, üçü değişmeden yeşil ("UE düzeltme tur 1") |
| KK3 — tarayıcı kalemleri üç motorda | — (pay) | U parçasında tarayıcı kalemi yok; UE4'ün iki tarayıcı ayıklama satırı chromium iki yolda yeşil, firefox/webkit **TAM: CI E2E** |
| KK4 — D-1678 | — | A parçası (PR #594) |
| KK5 — backlog satırları ve sayaç | ✓ (pay) | kapanan her satır `✅ Çözüldü → T604`, daraltılan satır yeni hedef + gerekçe; yeni kalemler (D-1758, D-1759, D-1773, D-1783, D-1806) `DEFERRED_BACKLOG.d/T604-U.md`'de; sayaç parça dosyasından birleştirmede (talimat: sayaca dokunulmaz) |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | her dilimin KK tablosu ve güvenlik maddesi |


---

## Doğrulama — UE (PR #595), tur 1 (2026-11-21)

- **Durum:** ✗ FAIL — 2 bulgu (ikisi S2, ORTA), düzeltme gerekli. Ayrıntı: doğrulama raporu (yardımcının `dogrulama-raporu-U-r1.md`).
- **Neden:** dal ucu `fb70d822`'nin CI Integration işi kırmızı (run 44848221151, 4645 testten 3'ü); üçü yerelde de kırmızı (`OneTimeValueTests` + `PageFormPatternTests`: 35 testten 3'ü). KK2'nin "var olan testler değişmeden geçer" şartı tutmuyor.
  1. `PageFormPatternTests.EveryMappedEndpoint_StartsWithAFirstSegmentOfTheClosedList`: yeni `/gorsel/{id:guid}` ucu `AddressTemplates.CheckNoInternalKey`'e takılıyor ("the parameter 'id' is an internal key in a storefront path", `07 2.1.6`). D-1136/D-1711 (K-1631 (1)) adres kuralının denetimine ve `07 2.1.6`/§3.1.6'ya hizalanmadı.
  2. `OneTimeValueTests.TheFormCarriesTheValueInAHiddenField_..._KK3` ve `AValidationErrorOnTheForm_DoesNotConsumeTheValue_..._KK3`: D-1454 izin düşürme katmanı (`ReauthenticationPermissionFilter`) test örnek sayfası `/hesap/ornek-yeniden-dogrulama`'nın (`ReauthenticationSample`) POST'unda izni işleyiciden önce düşürüyor; geçerli değer 303 `?asked=1` alıyor. Örnek sayfa işaretlenmedi, test de gerekçeyle değişmedi.
- **Doğrulayıcı kanıtları (kırmızı–yeşil, kendi koşumu):** `src` geçici bozuldu (D-1454/D-1389 katman kaydı kaldırıldı, D-1473 erken izin okuması kapatıldı, D-1474 L-9 sayımı öne alındı, D-1374 doğrulanmamış kayıt yolu kapatıldı, D-1283 (2) paylaşım kapatıldı, D-892 UC-131 dönme dalı kapatıldı, D-1711 dosya varlık denetimi kapatıldı) → ilgili 12 test kırmızı, 2 karşı yön testi yeşil; geri alınınca CI'da ve yerelde yeşil. Sınır örneği (geçici test, commit edilmedi): kartta 3 × 300, kupon 100, KDV 100,01, talep 2 adet → UC-131 dönmesi 533,34 / kupon 66,66 / KDV 66,67; eski sürümle POST hiçbir şey yazmadı; sürüm bir kez arttı; tek `RefundManually` iz satırı — yeşil.
- **Yerel sınama:** build `-warnaserror` 0 uyarı; Unit TAM 8098/8098 (Consistency ve Architecture dahil); betik testleri 4/4; `dotnet format` (dalın 149 `.cs` dosyası) değişiklik yok; Integration: yukarıdaki sınıflar + kırmızı–yeşil süzgeci. TAM: CI (kırmızı).
- **Kalem envanteri (KK1):** UE1–UE4'ün 36 kalemi ve eklenenler (D-1374, D-1389, D-1283 (2), D-1758, D-1759, D-1617, D-1783, D-1319 (2), D-137, D-894) `✅ Çözüldü → T604`; D-1599 ve D-1773 UH dalına gerekçe ve yeni hedefle daraltıldı; D-1806 yeni, hedefi UH dalı. ✓
- **Yapım raporu karşılaştırması:** KK2 satırındaki "eski testler aynı ✓" tutmuyor (Integration TAM CI'a bırakıldı, CI kırmızı). Öz denetim tablosu var.

---

## UE düzeltme tur 1 (2026-11-21)

Doğrulama tur 1'in iki bulgusu ve iki notu (K1, K2) bu turda kapandı. Commit'ler: `4f42e03d` (Bulgu 1–2, kod + `07` + K-1636), `657f6710` (K1), bu bölümün kayıt commit'i (K2 + kayıtlar).

### Bulgular ve notlar

| # | Kapanış | Kanıt |
|---|---|---|
| Bulgu 1 — `/gorsel/{id:guid}` vitrin iç anahtar kuralına takılıyordu | Görsel ucunun kalıbı tek evde: `AddressRules.ImageAddressTemplate` (`ImageEndpoint.Pattern` onu kullanır); `AddressTemplates.Check` iç anahtar denetimini yalnız bu birebir metinde atlar (dar istisna). `07` 2.1.6 istisna cümlesi, `07` v0.49, `PRODUCT_DISCOVERY_STATUS` 07 satırı, karar K-1636 (1) | Unit `AddressRulesTests.Check_OnTheImageEndpointsTemplate_FindsNothing_D1136` + karşı örnek teorisi `Check_OnAnyOtherTemplateWithAKey_TheImageExceptionDoesNotReach_Reports_D1136` (9 satır: `/gorsel/{id}`, `{id:int}`, `{imageId:guid}`, ek bölüm, `{orderItemId:guid}`, `/indir`, `/sepet`, `/hesap/adresler`, `/hesap/gorsel`); Integration `PageFormPatternTests.EveryMappedEndpoint_StartsWithAFirstSegmentOfTheClosedList` değişmeden yeşil (yalnız görsel kalıbının eşlendiğini sınayan bir `Assert.Contains` eklendi) |
| Bulgu 2 — D-1454 katmanı test örnek formunun POST'unda izni düşürüyordu | `ReauthenticationSampleModel` (`tests/.../Pages/FormSecuritySamples.cs`) `[ReauthenticationFormPage]` ile işaretlendi; ürün kümesi değişmedi (`ReauthenticationFormPageTests` iki yönlü eşitlik); `ReauthenticationPermissionFilter.cs` öznitelik açıklaması; K-1636 (2) | `OneTimeValueTests` iki KK3 testi değişmeden yeşil; yeni `OneTimeValueTests.TheTestHostPagesLeftToThePermissionsOwnRule_AreTheTokenPageAndTheSampleForm_AndNoOther_D1454` (test derlemesinin kümesi kapalı liste) |
| K1 — D-1283 (1) payının kanıtı #594'te | `DEFERRED_BACKLOG.md` D-1283 satırında "(1) payının kanıtı PR #594'e bağlıdır; sıra #594 → #595" | satır metni |
| K2 — D-610 iki aşamalı kırmızı–yeşil yeniden üretilmedi | Bu turda yeniden üretildi (aşağıdaki tablo) | aşağıda |

### K2 — D-610 iki aşamalı kırmızı–yeşil (2026-11-21, `t604u-pg`, PostgreSQL 18 `track_commit_timestamp=on`)

Bozma geçici `false &&` ile yapıldı; deneyden sonra `git checkout -- src`.

| Aşama | `src` bozması | Koşulan testler | Sonuç |
|---|---|---|---|
| (a) karar yerinde, modül denetimi yok | `AfterSalesModule.OpenDefectClaimAsync`'in kanal (`AfterSalesPeriodRules.IsDefectClaimChannelOpen`) ve adet (`ItemQuantities.IsSelectable`) denetimleri; `ChangeDefectClaimStatusAsync`'in yeniden açma kanal denetimi | `ItemRecordsTests` (bütün sınıf, partial'lar dahil) + `AfterSalesScreenTests.ReopenDefectClaim_InsideZ18..._KK10` + `AfterSalesScreenTests.TheUnconfirmedPost_WithUnitsAboveTheOpenOnes..._B4` — 74 test | 73 yeşil: açma kanalı (`OpenDefectClaim_OnAnUndatedPhysicalItem_...` 4 durum), adet (`OpenDefectClaim_RefusesZero_AndMoreThanTheOpenUnits`), müşterinin yeniden açması (`Reopening_..._IsRefused(actor: Buyer)`, KK10 sayfa testi) kuralı karar uygular. 1 kırmızı: `Reopening_OnTheLastDay_IsAllowed_AndTheDayAfter_IsRefused(actor: Admin)` "Expected: ConditionNotMet, Actual: null" — beklenen: yöneticinin yeniden açması `OrderTransitions`'ta kararı sormaz, panelin kapısı (1.11.37) sayfa katmanındadır; yazıcı düzeyinde yöneticiye Z-18'i modül denetimi uygular (K-1633 (1) modül savunması) |
| (b) karar da yok | (a) + `OrderTransitions.OpenDefectClaimAsync`'in karar reddi, `ReopenDefectClaimAsync`'in müşteri kararı, `EnterRefundIbanAsync` ve `ChangeRefundIbanAsync`'in kararı | `ItemRecordsTests.OpenDefectClaim*`, `Reopening_*`, `SecondClaim*`; `AfterSalesScreenTests` iki test (yukarıda); `StatusCorrectionTests.TheIban_*` (2), `TheCustomersCancellation_OnTheTransfer_TakesACheckedIban_KK4` — 13 test | 5 kırmızı: `OpenDefectClaim_OnAnUndatedPhysicalItem_...(Preparing, opens: False)` "Expected: False, Actual: True"; `OpenDefectClaim_RefusesZero_AndMoreThanTheOpenUnits` "Expected: InvalidInput, Actual: null"; `Reopening_..._IsRefused(Buyer)` ve `(Admin)` "Expected: ConditionNotMet, Actual: null"; `AfterSalesScreenTests.ReopenDefectClaim_..._KK10` (Z-18 dışı 303'ün yeri değişti). 8 yeşil: üç açık kanal durumu, `SecondClaim_...` (tek talep kuralı modülün `ClaimExists` satır denetiminde), B4 (cayma yolu — deneyin dokunmadığı yazıcı; karşılaştırma için), üç IBAN testi |
| geri alma | `git checkout -- src`; `-warnaserror` derleme 0/0 | (b)'nin 13 testi | 13/13 yeşil |

- **IBAN yolları:** `EnterRefundIbanAsync`/`ChangeRefundIbanAsync`'te modülün `AfterSalesPeriodRules`/`ItemQuantities` denetimi yoktur; yazıcının kendi koşulu isteğin/IBAN'ın satır aramasıdır (açık istek yoksa `NotFound`, kayıtlı IBAN yoksa `NotFound`) ve yazma için zorunludur — kaldırılınca yazıcı yazacak satır bulamaz. Bu yüzden IBAN yolunda (a) aşaması kurulamaz; (b)'de yalnız karar kaldırıldığında IBAN testleri yeşil kalır: karar, satır aramasının sipariş düzeyindeki özetidir (1.11.11 açık istek var, 1.11.12 kayıtlı IBAN var), ikinci bir kural evi değildir. ⚠ (kırmızı kurulamaz — gerekçe bu madde; UE5 Notlar'daki ⚠ ile aynı).
- **Sonuç:** açma ve müşterinin yeniden açması için iki aşamalı kırmızı–yeşil doğrulayıcının istediği biçimde üretildi; karar tek başına kuralı uygular, ikisi birden kaldırılınca kırmızı.

### Koşular (bu tur)

- **Build:** `dotnet build tests/Shopfolio.IntegrationTests -warnaserror` 0 uyarı, 0 hata.
- **Integration:** koşulan sınıflar/süzgeçler — önceki işçi (aynı `src`/`tests`, `4f42e03d`): `OneTimeValueTests`, `PageFormPatternTests`, `ImageEndpointTests`, `CustomerPasswordResetAndReauthenticationPageTests`, `PanelHomeAndVerificationPagesTests` 100/100 (tek çağrı); bu işçi: `ItemRecordsTests` (bütün sınıf) + `AfterSalesScreenTests` iki test + `StatusCorrectionTests` üç IBAN testi (K2 deneyi; geri alma sonrası 13/13). **TAM: CI.**
- **Unit:** TAM 8108/8108 (önceki işçi, Architecture dışı 6293 + Architecture 1815). Bu tur `src`/`tests`'e dokunmadı (`git diff 4f42e03d HEAD -- src tests` boş); yeniden koşulmadı ⚠.
- **`dotnet format`:** dalın değiştirdiği 154 `.cs` dosyası `--verify-no-changes` çıkış 0 (önceki işçi); bu turda `.cs` değişmedi.

### Öz denetim (UE'nin bütün kalemleri + KK1–KK5, düzeltme sonrası)

| Denetim | Sonuç | Kanıt |
|---|---|---|
| KK1 — hedefi T604 olan UE kalemleri kapalı ya da gerekçeli | ✓ | "UE kapanışı" tablosu (45 ✅, D-1599 ve D-1773 daraltıldı, D-1806 yeni; üçü UH dalına gerekçeli); `grep -nE "T604" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` bu turda yeniden okundu (144 satır) — UE kalemlerinin hepsi `✅ Çözüldü → T604` ya da gerekçeli; bu turda yeni kalem yok |
| KK2 — para ve güvenlik kalemleri ayrı başlıkta; kırmızı–yeşil; eski testler aynı | ✓ | dilim tabloları + K2 tablosu; tur 1'de kırılan üç eski test değişmeden yeşil (Bulgu 1–2); UE3, UE4 ve UE bütünü KK2 satırları düzeltildi |
| KK3 — tarayıcı kalemleri üç motorda | — (pay) | U parçasında tarayıcı kalemi yok; UE4'ün iki ayıklama satırı chromium, firefox/webkit TAM: CI E2E |
| KK4 — D-1678 | — | A parçası (PR #594) |
| KK5 — backlog satırları ve sayaç | ✓ (pay) | kapanan satırlar `✅ Çözüldü → T604`; daraltılanlar yeni hedef + gerekçe; yeni kalemler parça dosyasında; sayaca dokunulmadı |
| `12` KK2 kontrol listesi: "eski testler aynı" | ✓ | yukarıda |
| `09 §25` "kritik akışa dokunan değişiklik — testler silinmemiş, zayıflatılmamış" | ✓ | düzeltmede hiçbir test silinmedi ya da gevşetilmedi; `PageFormPatternTests`'e yalnız bir `Assert.Contains` eklendi |
| `09 §25` "CI yeşil mi" | TAM: CI | PR #595'in yeni ucunda CI koşar; merge ön koşulu |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | görsel istisnası yalnız birebir `AddressRules.ImageAddressTemplate` metni; 9 satırlık karşı örnek teorisi istisnanın komşu biçimlere uzanmadığını sınar; yeniden doğrulama istisnası test derlemesinde kapalı liste testiyle |
| (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ | istisna metin eşitliğiyle; `/` önekli ve öneksiz biçim ikisi de sınandı (`AddressRulesTests` 208–209) |
| (3) 06 varsayılan / FK dizini | uygulanmaz | şema değişmedi |
| (4) aynı kuralı anan öteki yerler | ✓ | `07` 2.1.6 metni, `07` başlık + altbilgi v0.49, `PRODUCT_DISCOVERY_STATUS` 07 satırı, `ReauthenticationPermissionFilter.cs` öznitelik açıklaması; rapordaki üç KK2 satırı |
| (5) belge kuralını değiştiren metin karar kaydısız | ✓ | `07` 2.1.6 değişikliği K-1636 (1)'i anar; örnek sayfa işareti K-1636 (2) |
| Kısmen karşılanan kriter | yok | IBAN yolunun (a) aşaması gerekçeyle kurulamaz (yukarıda), kriter kırmızı–yeşili kurulabilen yollarda karşılar |
| STATUS T604 tablo satırı | dokunulmadı | talimat |

### Notlar

- ⚠ K2'nin (a) aşamasında yöneticinin yeniden açması kırmızı: yönetici yolu `OrderTransitions`'ta kararı sormaz (panelin kapısı 1.11.37 sayfa katmanında); yazıcı düzeyinde Z-18'i modül denetimi uygular — K-1633 (1) modül savunmasının bilinçli işlevi; kod değişmedi.
- ⚠ IBAN yollarında (a) aşaması kurulamaz (yazıcının satır araması yazma için zorunlu); gerekçe yukarıda.
- ⚠ Unit TAM ve `dotnet format` bu turda yeniden koşulmadı: `src`/`tests` önceki işçinin koşusundan beri değişmedi; CI Unit ve LINT yeni uçta koşar.
- A (#594) ve UH (#596) dallarının dosyalarına dokunulmadı; `04`/`07` sürüm satırı çakışması birleştirmede çözülür.
