# T368 — Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-09

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

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (D-819 güncellendi, D-838, D-817 güncellendi) |

### Doğrulama tur 1 — Doğrulama Sonucu — T368 Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi — tur 1
**Tarih:** 2026-11-09   **Branch:** task/T368-gecikme-feshi   **Commit:** `246071f7` (doğrulanan dal ucu), PR #498; kayıt commit'inden önce `origin/main` `78a3f598` çakışmasız birleştirildi (yalnız T352 kayıt belgeleri)

**Verdict: ✓ PASS**

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook | ✓ | `core.hooksPath` ana çalışma kopyasının `scripts/git-hooks`'u |
| Ana dal CI (son 3) | ✓ | 42600847174 success, 42600846924 success, 42596386708 success |

#### Kabul Kriterleri (doğrulayıcı kanıtı)
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | Kargoya verme süresi aşımında yol T367'nin iptal düğmesi (`CancelItemAsync`, kayıt `Cancellation`); Z-11 yolu ayrı kayıt tipi `LateDeliveryTermination` (`06 §4.24`). Katalog `IsLateTerminationOpen` Hazırlanıyor/Kargoya verildi/Teslim edilemedi; `Termination_OfAShippedOrder_…(Shipped, DeliveryFailed)` kargodayken açık |
| 2 | ✓ | `DeliveryPeriodRules.IsLateDeliveryTerminationOpen` (ödendi, Z-11 = `Order.CreatedAt` + 30 gün, teslim tarihsiz açık adetli fiziksel kalem); `Termination_OpensExactlyThirtyDays…` (−1 tick ret / tam açık), `…DoesNotOpen_OnAnItemWithADeliveryDate_OnADigitalOnlyOrder_OrOnAnUnpaidOne_KK2`, `…CoversTheItemExemptFromWithdrawal_TheCustomMadeOneToo_KK2` |
| 3 | ✓ | İşlem adet almaz; her fiziksel teslim tarihsiz kalemin `Quantity − kapalı` adedi; yazıcı başka adedi `QuantityExceedsOpen` ile reddeder ve fiziksel olmayanı `StatusNotAllowed` ile; `…OfAPreparingOrder_…` (1 adet önceden iptal → 2 adet), `…LeavesTheDigitalAndTheServiceItemOut_…_KK3` |
| 4 | ✓ | Feshedilen adet açık adetten düşer → firma iptali kapalı (`…OfAShippedOrder_…`: firma iptali ret, `Cancellations` boş); Hazırlanıyor'da S4 `Closure` (sebepsiz), kargodakinde geçiş yok; B-7 sayısı değişmez, B-9 + F-3 gider |
| 5 | ✓ | `FromOtherChannel`: yönetici, onay, sürüm, ulaşma tarihi ≤ bugün; aynı yol (`TerminateForLateDeliveryAsync`), `DeclarationDate` = ulaşma tarihi (`ANoticeFromAnotherChannel_IsRecordedLikeTheButton_…_KK5`), ileri tarih/onaysız/eski sürüm ret, müşteri yolu tarih taşıyamaz; geri ödemenin yazımı D-818 (T370) — süre başlangıcı kayıtta |
| 6 | ✓ | `OrderPeriods.LegalInterestWarningsAsync`: her fesih + Z-10/Z-39 sonrası her iptal (aktörden bağımsız), düzeltilmiş geçişe bağlı iptal hariç; ilk düzeltilmemiş S5 sözü bitirir (D-531, `DeliveryPeriodRulesTests` D531); `…TheLegalInterestWarning_…_KK6`; faiz alanı/hesabı yok. Panel gösterimi D-819 (T528) |
| 7 | ✓ | `CancellationReason` altı üye = `02 §7.2.4` listesi (`FirmReasonRulesTests`); "diğer" ↔ not (modül + `reason_note_only_on_other` kısıtı); `Confirmed` yoksa `ConfirmationRequired`; B-7 `CancelledNotice` sebep ve notla; son adetler firmanın S4/S9'u ve kayıt bağlı (`…CarriesTheReasonAndTheNote_SendsB7_…_KK7`, `…ReturnedShipmentsLastUnits_…S9_KK7`) |
| 8 | ✓ | `IrreversibleConfirmations.FirmCancellation(..., isOutOfStock: true)` uyarı satırı onay cümlesinden önce (`Lines[0]`), öteki sebeplerde yok — ödenmiş ve ödenmemiş (`…ForOutOfStock_…_KK8` teorisi) |
| 9 | ✓ | `FirmReasonRules.IsComplete`: `AgreedWithBuyer` ↔ `OwnerConfirmed`; kalem ve bütün sipariş iptalinde teyitsiz `InvalidInput`, hiçbir şey yazılmaz (`AgreedWithTheCustomer_…_ItemOrWholeOrder_…_KK9`) |

#### Task Satırları (`12 §7.5`)
KP-21 ve KP-47'den T368'e eşlenen 11 satır: `VAL-C008` (k1), `C009` (k1–k3), `C010` (k4), `A070` (k5), `C019` (k2), `C049`, `C050` (k7), `C051` (D-305), `A209` (k8), `A228`, `A229` (k6). Hepsi KRİTİK, kanıt türü uç yanıtı ya da tarayıcı gözlemi; uç ve ekran T512/T528'in → `BEKLEMEDE` kaldı (T367, T352 emsali). Geri ödeme satırları (`A069`, `A210`, `A216`, `A217`) T370'in. `12_matris_denetle.py --kapsam 1-77` hata 0.

#### Doğrulama Kontrol Listesi
- [x] KK1–KK9 (yukarıdaki kriterler)
- [x] `09 §25`: modül sınırı (AfterSales yazıcıları kilit altında, kaydetmez), tek kural evi (`OrderOperationCatalog`, `FirmReasonRules`, `DeliveryPeriodRules`, `ItemShares`), `TimeProvider`, yeni paket/göç/ayar yok, mimari ve enum kilit testleri yeşil; iz satırı Ordering'de yazılmaz (D-646 kalıbı) → K1
- [x] Güvenlik taraması

#### Test Sonuçları
| Tür | Sonuç | Komut / kapsam |
|---|---|---|
| Build | ✓ 0 uyarı | `dotnet build Shopfolio.slnx --no-restore -warnaserror` |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 |
| Unit | ✓ 5213/5213 + Consistency 288/288 | `--filter-not-trait` / `--filter-trait "Consistency=true"` |
| Betik | ✓ | `tests/scripts/run-script-tests.sh` 4 dosya |
| Integration | ✓ 278/278 | `LateDeliveryAndFirmCancellation`, `StatusCorrection`, `ShipmentTransitions`, `DeliveryPeriod`, `CustomerCancellation` 170; `AxesTie`, `ItemRecords`, `OrderConfirmation`, `PaymentTransitions` 108 (PostgreSQL 18, `track_commit_timestamp=on`) |
| Dal CI | ✓ | run 42604944280 (head `246071f7`) success — Integration tam koşu dahil |

#### Güvenlik Kontrolü
Secret sızıntısı temiz · auth: yönetici yolu `TryAdminActor`, kalem–sipariş eşleşmesi `LockItemAsync`; müşterinin sipariş sahipliği uç işidir (T512) · girdi: ileri tarih, adet < 1, not/teyit kuralı ret · yeni bağımlılık yok.

#### Bulgular
Yok.

#### Bloke-etmeyen Notlar
| # | Not | Karar |
|---|---|---|
| K1 | D-819 panel payını müşteri ekranı T512'ye yazıyor; firma iptali, başka kanal fesih kaydı ve iki uyarı E-37'nin (T528); `CancelByCompany` ve `RecordLateDeliveryTerminationFromOtherChannel` iz satırları ileri atılmamış | Bu PR'da kapatıldı: D-819 hedefi T512, T528; iz satırı eklendi |
| K2 | KK6 testi müşteri iptalinin uyarısını sınamıyor (kod aktörden bağımsız); D-793'ün kapanışı bu teste dayanıyor | D-838 (`DEFERRED_BACKLOG.d/T368.md`), hedef T528 |
| K3 | Başka kanal fesih kaydı IBAN'sızsa IBAN isteği ve B-14 (`03 §8.4.8`) D-817'de yazılı değildi | Bu PR'da kapatıldı: D-817'ye eklendi, hedefe T528 |

#### Yapım Raporu Karşılaştırması
Tam uyumlu; öz denetim tablosu var ve kanıtlı. Farklar: K1 (yapım D-819'u yalnız T512'ye bağladı), K2, K3 yapım raporunda yok. ⚠ notları yerinde: uyarı metni `02 §7.2.4`'ün iki unsurunu taşır; Teslim edilemedi'deki firma iptalinde stok hemen döner (`03` 1.4.4.4), fesihte kargodaki mal dönüş adımıyla (1.4.4.5).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T368-gecikme-feshi`
- **Commit:** son commit — "T368: Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi"
- **PR:** #498
- **CI:** run 42604944280 (head `246071f7`) success
- **Post-merge run'lar:** squash `5e9f715` (#498) — CI 42613944248 success, Publish 42613944083 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

- Yok.

## Known Limitations / Follow-up

- D-817 havale hattında IBAN (T370, T512, T528); D-818 geri ödeme kaydı (T370); D-819 ekran payı ve iz satırı (T512, T528); D-820 ayıpta dönülmüş adet (T374); D-838 müşteri iptalinin faiz uyarısı testi (T528).

## Notlar

- ⚠ "Stokta bulunamadı" uyarı metni belgede birebir yazılı değil; `02 §7.2.4`'ün iki unsurundan (yasal imkânsızlık sayılmaz, sonuç firmada) ürün metni yazıldı.
- ⚠ Firma iptalinde Teslim edilemedi'deki kalemin adedi sayaca hemen döner (1.4.4.4 satırına uyuldu); gecikme feshinde kargodaki mal dönüş adımıyla eklenir (1.4.4.5).
- ⚠ Ödenmiş siparişin bütün firma iptali artık kalem kayıtlarını yazıp stoğu döndürür; düzeltme (T319) stoğu yeniden alır — `StatusCorrectionTests` beklenen stokları buna göre güncellendi (çifte sayım yok).
