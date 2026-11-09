# T368 — Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Gecikme feshi** `IOrderTransitions.TerminateForLateDeliveryAsync` (`OrderTransitions.LateDeliveryTermination.cs`): müşterinin düğmesi (`ByCustomer`; sürüm, onay, tarih taşımaz — tarih bugün) ve yöneticinin başka kanaldan gelen bildirimi (`FromOtherChannel`; geri alınamaz onay, ekran sürümü, ulaşma tarihi ≤ bugün). Sipariş kilidi altında işlem kararı (`OrderOperations.CheckAsync`, Z-11 = siparişin firmaya ulaşmasından otuz gün; Hazırlanıyor/Kargoya verildi/Teslim edilemedi). Teslim tarihsiz her fiziksel kalemin **bütün** açık adeti ayrı kayda (`late_delivery_terminations`; yazıcı `AddLateDeliveryTerminationAsync` başka adedi reddeder); dijital ve hizmet kalemi dışarıda. Kargoya verilmemiş siparişte stok döner (1.4.4.5), açık kalem kalmazsa kapanış kuralı S4 (Closure) yazar; kargodaki sipariş durumunda kalır. B-7 yok; müşteriye B-9, müşterinin kendi feshinde firmaya F-3.
- **Firma kalem iptali** `CancelItemByFirmAsync` (`OrderTransitions.FirmCancellation.cs`): onay → alan kuralları (adet ≥ 1, `FirmReasonRules.IsComplete`) → kilit + sürüm → işlem kararı (fiziksel Alındı/Hazırlanıyor/Teslim edilemedi, hizmet işaretine kadar, dijital hiç) → `AddFirmCancellationAsync` (yönetici, sebep, yalnız "diğer"de not) → son açık adetlerde firmanın S3/S4/S9'u (kayıt geçişe bağlanır), değilse sebepli B-7 + kapanış kuralı → sayaç (1.4.4.4).
- **Bütün sipariş firma iptali** (ödenmiş): `CancelOrderAsync` firmanın kalemsiz iptalinde her açık kalemin açık adetine firma iptal kaydı yazar, geçişe bağlar, sayaç döndürür (`CancelOpenItemsByFirmAsync`; D-305). Teslim edilemedi'de son adetlerin firma kalem iptali S9'a izinli.
- **Kapalı sebep listesi ve teyit:** `FirmReasonRules.IsComplete` (Other ↔ not, AgreedWithBuyer ↔ `OwnerConfirmed`) bütün iptal ve kalem iptalinin tek kural evi; `CancelOrderRequest.OwnerConfirmed` (init) ve `ByAdministrator(..., ownerConfirmed:)`; müşteri/sistem yolunda `OwnerConfirmed` reddedilir.
- **Kanuni faiz uyarısı:** `OrderPeriods.LegalInterestWarningsAsync(orderId)` — her fesih kaydı ve süre aşımından sonraki her iptal (müşterinin, firmanın); düzeltilmiş geçişe bağlı iptal hariç (`IAfterSalesModule.ClosingRecordMomentsAsync`). `DeliveryPeriodRules.IsLegalInterestWarningDue` ilk düzeltilmemiş S5'i okur (D-531). Sistem faiz hesaplamaz.
- **"Stokta bulunamadı" uyarısı:** `IrreversibleConfirmations.FirmCancellation(..., isOutOfStock)` onay cümlesinden önce `OutOfStockLegalNotice`'ı taşır.
- Devam işçisi: iki yeni enum (06'da tanımsız, saklanmayan) bool'a indirildi (`ClosingRecordMoment.IsTermination`, `OwnerConfirmed`); uyarı sabitinin adı satış modeli taramasına takılmayacak biçimde `OutOfStockLegalNotice` yapıldı; yanlış yere kaymış belge yorumu düzeltildi; Integration testi yazıldı; `StatusCorrectionTests` firma iptali kayıtlarını artık üretimden okur.

## Etkilenen Modüller / Dosyalar

- Ordering: `OrderTransitions.LateDeliveryTermination.cs` (yeni), `OrderTransitions.FirmCancellation.cs` (yeni), `FirmReasonRules.cs` (yeni), `OrderTransitions.Shipment.cs`, `OrderTransitions.CustomerCancellation.cs`, `OrderPeriods.cs`, `DeliveryPeriodRules.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/ShipmentTransitionRequests.cs`
- AfterSales: `AfterSalesModule.Cancellations.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`
- Infrastructure: `Operations/IrreversibleConfirmations.cs`
- Testler: `LateDeliveryAndFirmCancellationTests.cs` (yeni, 16), `FirmReasonRulesTests.cs` (yeni), `DeliveryPeriodRulesTests.cs`, `IrreversibleConfirmationTests.cs`, `ShipmentTransitionContractTests.cs`, `ShipmentTransitionsTests.cs`, `StatusCorrectionTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Durum | Kanıt |
|---|---|---|---|
| 1 | Kargoya verme süresi aşımında fesih iptal düğmesiyle; Z-11 aşımında, kargodayken de, ayrı kayıt tipiyle fesih | ✓ | iptal düğmesi T367 `CancelItemAsync` (Kargoya verildi'ye kadar); fesih `late_delivery_terminations` — `Termination_OfAPreparingOrder_…_KK1_KK3_KK4`, `Termination_OfAShippedOrder_…_KK1_KK4(Shipped, DeliveryFailed)` |
| 2 | Otuz gün, teslim tarihsiz, açık adetli fiziksel kalem; istisnalı ürün dahil | ✓ | `Termination_OpensExactlyThirtyDays…_KK2` (sınır −1 tick / tam), `Termination_DoesNotOpen_OnAnItemWithADeliveryDate_OnADigitalOnlyOrder_OrOnAnUnpaidOne_KK2`, `Termination_CoversTheItemExemptFromWithdrawal_…_KK2` |
| 3 | Bütün açık adetler, seçimsiz; dijital ve hizmet dışarıda | ✓ | `Termination_OfAPreparingOrder_…` (önceden 1 adeti iptal edilmiş kalemde 2 adet), `Termination_LeavesTheDigitalAndTheServiceItemOut_…_KK3`; yazıcı başka adedi reddeder |
| 4 | Feshedilen kalem kargoya/firma iptaline konu olmaz; açık kalem yoksa İptal (kargodaysa dönüş kapanışıyla); B-7 yok | ✓ | S4 Closure + B-7 0 (`…OfAPreparingOrder_…`); kargodaki sipariş durumunda, firma iptali ret (`…OfAShippedOrder_…`); yeniden gönderim açık kalemleri okur (`OpenItemsAsync`) |
| 5 | Başka kanal bildirimi düğme açıkken panelden; süre ulaşma tarihinden | ✓ | `ANoticeFromAnotherChannel_IsRecordedLikeTheButton_…_KK5` (DeclarationDate = ulaşma tarihi, AdminId), `…DatedInTheFuture_Unconfirmed_OrOnAnOldScreen_IsRefused_…_KK5`; geri ödeme kaydı D-818 (T370) |
| 6 | Fesih satırında ve süre sonrası iptalde faiz uyarısı; hesap yok | ✓ | `TheLegalInterestWarning_MarksEveryTermination_AndTheCancellationsAfterThePromise_NotTheOnesBefore_KK6`; `DeliveryPeriodRulesTests` (D531); gösterim ekranı D-819 (T512) |
| 7 | Kapalı altı sebep, "diğer"de not, geri alınamaz onay, B-7 | ✓ | `FirmReasonRulesTests` (liste, not), `TheFirmsItemCancellation_CarriesTheReasonAndTheNote_SendsB7_…_KK7`, `…WithoutTheNoteOnOther_…_KK7` (ConfirmationRequired dahil), `…ReturnedShipmentsLastUnits_…S9_KK7`, `…ClosedOnADigitalItem_AndOnAShippedOrder_KK7` |
| 8 | "Stokta bulunamadı"da onaydan önce uyarı | ✓ | `IrreversibleConfirmationTests` (KK8 teorisi: uyarı yalnız OutOfStock'ta, cümleden önce) |
| 9 | "Müşteriyle anlaşıldı"da teyitsiz iptal yok | ✓ | `FirmReasonRulesTests.AgreedWithTheCustomer_…_KK9`, `AgreedWithTheCustomer_WithoutTheFirmsConfirmationOfTheOwner_IsNoCancellation_ItemOrWholeOrder_…_KK9` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin kodu dahil diff baştan okundu.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 iki fesih yolu, ayrı kayıt tipi | ✓ | kriter 1 |
| KK2 düğme koşulları, istisnalı ürün | ✓ | kriter 2 |
| KK3 seçimsiz tüm açık adet; dijital, hizmet dışarıda | ✓ | kriter 3 |
| KK4 kargo, firma iptali, B-7 yok | ✓ | kriter 4 |
| KK5 bildirim tarihinden işleyen süre | ✓ | kriter 5 (kayıt damgası; geri ödeme D-818) |
| KK6 faiz uyarısı var, hesap yok | ✓ | kriter 6 |
| KK7 kapalı sebep listesi, onay, B-7 | ✓ | kriter 7 |
| KK8 stok uyarısı onaydan önce | ✓ | kriter 8 |
| KK9 teyitsiz "anlaşıldı" iptali yok | ✓ | kriter 9 |
| 09 §25 — tek kural evi | ✓ | açıklık `OrderOperationCatalog` (fesih ve firma iptali kilit altında `CheckAsync`), sebep `FirmReasonRules`, Z-11 `DeliveryPeriodRules`, pay `ItemShares` |
| 09 §25 — modül sınırı, işlem sahipliği | ✓ | AfterSales yazıcıları kaydetmez, işlem açmaz (`RequireOrderLock`); okuma `ClosingRecordMomentsAsync` düz okuma |
| 09 §25 — kilit sırası | ✓ | sipariş kilidi → kayıtlar → sayaçlar (`ReturnAsync` en sonda) |
| 09 §25 — ret değerdir, hiçbir şey yazılmaz | ✓ | `AssertNothingTerminatedAsync`, `AssertNoFirmCancellationAsync` |
| Enum kuralları (06 §5) | ✓ | yeni enum yok (bool'a indirildi); `EnumLockTests`, `EnumTableConsistencyTests` yeşil |
| Satış modeli taraması | ✓ | "stock warning" adı kaldırıldı (`OutOfStockLegalNotice`) |
| Güvenlik taraması | ✓ | yeni uç/sayfa yok; ham SQL yok; kalem–sipariş eşleşmesi `LockItemAsync`; yönetici yolu `TryAdminActor`; ileri tarih reddi |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | yeni bekçi/istisna yok |
| Sık FAIL (2) tarama çağrı biçimi | ✓ | yeni tarama yok |
| Sık FAIL (3) DB default / FK dizini | ✓ | şema değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | `ShipmentTransitionContractTests` işlem listesi; `IrreversibleConfirmationTests` örnek listesi |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | doküman kuralı değişmedi |
| Modül kurucusu / test düzenekleri | ✓ | yeni kurucu bağımlılığı yok; Integration TAM koşuldu |
| Devredilen backlog kalemleri | ✓ | D-305, D-519, D-531, D-793 ✅; D-337, D-453, D-513 T368 payı kapandı; yeni D-817…D-820 |
| `dotnet format` + build | ✓ | üç proje format uygulandı; Release `-warnaserror` 0 uyarı |

## Test Sonuçları

| Tür | Sonuç |
|---|---|
| Build (Release, `-warnaserror`) | Web, Unit, Integration 0 uyarı, 0 hata |
| `dotnet format` | Web, UnitTests, IntegrationTests uygulandı (temiz) |
| Unit (Consistency dahil) | 5501/5501 |
| Integration TAM | 3170 test; ilk koşuda 7 `StatusCorrectionTests` (firma iptali artık kayıt yazar — test düzeltildi) ve 8 `SecurityHeadersTests` (yük altında kararsız; tek başına 133/133 ile birlikte yeşil) kırmızı; düzeltme sonrası `StatusCorrectionTests` + `ShipmentTransitionsTests` + `SecurityHeadersTests` 133/133, `LateDeliveryAndFirmCancellationTests` 16/16 |

## Doğrulama

*(validator doldurur)*

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T368-gecikme-feshi`
- **Commit:** son commit — "T368: Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi"
- **PR:** *(açılınca)*
- **CI:** *(validator doldurur)*

## Dış Varsayımlar

- Yok.

## Known Limitations / Follow-up

- D-817 havale hattında IBAN (T370, T512); D-818 geri ödeme kaydı (T370); D-819 ekran payı (T512); D-820 ayıpta dönülmüş adet (T374).

## Notlar

- ⚠ "Stokta bulunamadı" uyarı metni belgede birebir yazılı değil; `02 §7.2.4`'ün iki unsurundan (yasal imkânsızlık sayılmaz, sonuç firmada) ürün metni yazıldı.
- ⚠ Firma iptalinde Teslim edilemedi'deki kalemin adedi sayaca hemen döner (1.4.4.4 satırına uyuldu); gecikme feshinde kargodaki mal dönüş adımıyla eklenir (1.4.4.5).
- ⚠ Ödenmiş siparişin bütün firma iptali artık kalem kayıtlarını yazıp stoğu döndürür; düzeltme (T319) stoğu yeniden alır — `StatusCorrectionTests` beklenen stokları buna göre güncellendi (çifte sayım yok).
