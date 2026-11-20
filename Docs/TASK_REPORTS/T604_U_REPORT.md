# T604 (U) — F5 kapısından kalan ürün kodu kalemleri

**Faz:** F6 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-20

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
| KK2 — para ve güvenlik kalemleri ayrı başlıkta; kırmızı–yeşil; eski testler aynı | ✓ | "Para ve güvenlik kalemleri (KK2)"; tek değişen eski test spec gereği, tabloda; D-1375 kırmızısı kurulamaz ⚠ |
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

**BÖLÜNDÜ (süre).** Beş kalemin üçü kapandı (D-1617, D-1758, D-1759); D-892 ve D-610 sonraki U dilimine (hedef T604 kalır, satırlarında okuma ve yapılacak; devir notu `devir-UE2b.md`).

### Yapılan İşler

| Kalem | Değişiklik |
|---|---|
| D-1617 | `Pages/Panel/OrderDetail.Refunds.cs` iki özet yorumu yalnız bağlanan modeli anar (`ProcessRefundInput`, `RetryCardRefundInput`); kalkan üç model adı kodda geçmez |
| D-1758 | Karar K-1629 (1): `DownloadRefusal.ItemClosed`; `OrderPageReader.ClosedReason` sırası ödeme onaysız → kalem kapanmış (`OpenQuantity <= 0`) → hak dolmuş; `OrderPageModel.ItemClosedText`; `07` 4.16.3 ve §6.2 `DownloadNotAvailable` (v0.46), `04` 5.16.13 (v0.59) |
| D-1759 | Karar K-1629 (2): `Carts.AddAsync` (kilitten önce ve kilit altında yeniden) ve `UpdateItemAsync` sepetin kendi ayırmasını (`OwnReservedUnitsAsync`) geçer; `ICatalogModule.JudgeCartAddAsync` / `CartQuantityReader.JudgeAddAsync` yeni `ownReservedUnits` parametresi; form alan kanıt tablosunun 7.1.4.1 satırı yeni çağrı satırına hizalandı (`FormFieldRuleEvidence.cs`) |
| D-892 | yapılmadı — okuma satırında: UC-140 talebin malını teslim alır ve izi yazar, bedel indirimi UC-133'tür; eksik olan teslim almadan sonraki geri ödemenin hatta düşmesidir (`ListOwedRefundRecordsAsync` talebin teslim almasını borçlu saymaz; `RescindDefectClaimAsync` çağrılmaz) |
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

- **Unit:** TAM koşusu 8097: 8094 ✓, 3 ✗ (form alan kanıt tablosunun 7.1.4.1 alıntı satırı; düzeltildi) → üç sınıf (`FormFieldRuleClausesTests`, `FormInventoryConsistencyTests`, `FormFieldEnforcementTests`) 61/61 ✓.
- **Integration:** koşulan sınıflar — `Ordering.OrderConfirmationTests`, `Files.OrderPageTests`, `Ordering.CartQuantityTests`, `Ordering.CartTests`, `Ordering.MainFlowTests`: 89/89 ✓. TAM: CI.

### Notlar

- ⚠ K-1629 (2) okuması: `02 §3.17.7`'nin "müşteri kendi ayırdığı stoğa takılmaz" cümlesi ekleme ve adet yolunu da kapsar sayıldı (belge metni değişmedi).
- ⚠ D-1758 metni (`ItemClosedText`) yeni bir müşteri cümlesidir; `04` 5.16.13'e birebir yazıldı.
- ⚠ D-892 bu dilimde kod almadı: borçlu kayıt kuralı (AfterSales), UC-131 başlatma yolu, panel sayacı ve geri ödeme satırı birlikte değişir; para işidir, süreye sığmadı.
