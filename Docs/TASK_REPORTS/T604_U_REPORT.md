# T604 (U) — F5 kapısından kalan ürün kodu kalemleri

**Faz:** F6 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-18

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
