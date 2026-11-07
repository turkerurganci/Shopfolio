# T316 — Sipariş durumu geçişleri: sevkiyat ekseni

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- Geçiş biriminin açık yüzü `IOrderTransitions` yedi işlemle genişledi: `StartPreparationAsync` (S1, onayla birlikte Ö1), `CancelOrderAsync` (S3/S4/S9 iptalleri), `CloseReturnedOrderAsync` (S9 sebepsiz kapanış), `ShipOrderAsync` (S5), `ResendOrderAsync` (S8), `MarkDeliveredAsync` (S6), `MarkDeliveryFailedAsync` (S7). S2, S10 ve S11 için ayrı yöntem yoktur: kapanış kuralının son olayı (T315 `EvaluateClosureAsync`) yazar.
- `OrderTransitions` kısmi sınıf oldu; işlemler `OrderTransitions.Shipment.cs`'tedir. Kilit sonrası kod seçimi `ApplyChosenAsync` ile yapılır: iptalin S3 mü S4 mü S9 mu olduğu kilitli siparişin durumundan çıkar.
- B-4, B-5, B-6 ve B-7 bildirimleri aynı işlemde bildirim modülünün kuyruğuna yazılır; S6'da ve kapanışla işleyen S3/S4/S9'da bildirim yazılmaz.
- `Order.RecordShipment` kargo alanlarını (kargo şirketi, diğer şirket adı, takip numarası, kendi aracı) yazar; sürüm tek işlemde bir kez artar.
- Açık yüz sözleşmeleri: `ShipmentTransitionRequests.cs` (istekler, `OrderTransitionResult`), `TransitionRefusal.cs` (ret nedenleri; veritabanında tutulmaz, `ApplicationEnums.NotStored` adlı dar istisnası var).
- Testler: Integration `ShipmentTransitionsTests` (91 test), Unit `ShipmentTransitionContractTests` (8 test). `OrderTransitionsTests` sağlayıcısı bildirim ve ayar modüllerini artık `TransitionTestServices.AddNoticeDependencies` ile kurar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/OrderTransitions.cs`, `OrderTransitions.Shipment.cs` (yeni), `TransitionRequests.cs`, `Order.cs`
- `src/Shopfolio.Web/Modules/Ordering/Contracts/IOrderTransitions.cs`, `ShipmentTransitionRequests.cs` (yeni), `TransitionRefusal.cs` (yeni)
- `tests/Shopfolio.IntegrationTests/Ordering/ShipmentTransitionsTests.cs` (yeni), `TransitionTestServices.cs` (yeni), `OrderTransitionsTests.cs`
- `tests/Shopfolio.UnitTests/Ordering/ShipmentTransitionContractTests.cs` (yeni), `Support/ApplicationEnums.cs`
- `.claude/CONTEXT.md` (Ordering satırı), `Docs/DEFERRED_BACKLOG.md` (D-305…D-309, D-291), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | S1…S11 beyaz liste; geri yönlü ve tablo dışı geçiş yasak; düzeltme tablonun dışında | ✓ | `AnOperationOutsideTheTable_IsRefused_AndWritesNothing_KK1` (17 satır), `ThereIsNoBackwardOperation_...KK1`, `TheCorrection_IsNotStoppedByTheWhiteListsRefusal_KK1` |
| 2 | Teslim edildi ve İptal edildi terminal | ✓ | `ATerminalState_IsLeftByNoOperation_KK2` (17 satır) |
| 3 | S1 ödeme onayıyla aynı anda, yalnız fiziksel kalemli siparişte; B-4 Ö1'in, S1 ayrıca bildirim üretmez | ✓ | `S1_OnAnOrderWithoutAPhysicalItem_IsRefused_...KK3`, `S1_OnAMixedOrderWithAPhysicalItem_WritesO1AndS1_...OneB4Row_KK3`, `S1_AfterTheTransfersO1_WritesS1Alone_AndNoSecondB4Row_KK3` |
| 4 | S2, S10, S11 kapanışı tamamlayan son olayla işler | ✓ | `TheLastEventThatCompletesTheClosure_WritesTheDeliveredTransition_InTheSameOperation_KK4`, `S7_OfAShipmentWhoseItemsAreClosed...EndsInS11_KK4`; ayrı yöntem yok: `TheShipmentAxis_HasAnOperationForS1S3S4S9AndS5ToS8_AndNoneForTheClosures_KK4` |
| 5 | S3 müşteri/sebepli yönetici/sistem, S4 müşteri/sebepli yönetici, S9 sebepli yönetici; B-7 gider | ✓ | `S3_ByTheCustomer_...KK5`, `S4_ByTheCustomer_...KK5`, `TheAdministratorsCancellation_ByTheState_...KK5`, `S3_BySystem_...KK5`, `TheOtherActors_AreRefusedByEachCancellation_...KK5`, `TheCancellationsTriggers_AreTheOnesOfTheTable_KK5` |
| 6 | Kapanışla işleyen S3/S4/S9 sebepsiz, B-7 yok | ✓ | `TheClosure_OfPlacedAndPreparing_IsWrittenWithoutAReasonOfTheFirm_AndWithoutB7_KK6`, `S9_AsTheClosing_AfterS7_...KK6`, `S9_AsTheClosing_IsRefused_WhileAnItemIsOpen_...KK6` |
| 7 | S5 ve S8 yalnız yönetici, geri alınamaz onay, yalnız açık fiziksel kalem; B-5 | ✓ | `S5_WithoutTheConfirmation_...KK7`, `S5_SendsOnlyTheOpenPhysicalItems_...OneB5Row_KK7`, `S8_SendsTheOpenPhysicalItemsAgain_...KK7`, `S8_WithoutTheConfirmation_...KK7`, `ByAnotherKindOfActor_TheTablesShipmentRows_AreRefused_KK7` |
| 8 | S6 yöneticinin teslim işareti ve tarihi; kendiliğinden geçiş ve bildirim yok | ✓ | `S6_MarksTheOpenPhysicalItemsWithTheDate_WritesNoNotice_AndNothingFollowsByItself_KK8`, `S6_AfterwardsNoClosureOrOperationMovesTheDeliveredOrder_KK8`, `S6_WithADateInTheFuture_...KK8` |
| 9 | S7 yöneticinin işareti; B-6 gider | ✓ | `S7_MarksTheReturnedShipment_AndWritesOneB6Row_KK9`, `S7_WithAnOlderScreenVersion_...KK9` |

> Kısmen (~) karşılanan kriter yok.

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil kriter 1–9 ve KK1–KK9'un tamamı yeniden denetlendi (yalnız kalanlar değil).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1 / KK1 tablo dışı ve geri yönlü ret; düzeltme takılmaz | ✓ | `ShipmentTransitionsTests` KK1 testleri; `TransitionTable.Shipment` içinde Shipped→Preparing satırı yok |
| k2 / KK2 terminalden çıkış yok | ✓ | `ATerminalState_IsLeftByNoOperation_KK2` |
| k3 / KK3 fiziksel kalemsiz S1 yok, karışık girer, tek B-4 | ✓ | KK3 üç test |
| k4 / KK4 son olay kapanışı doğurur | ✓ | KK4 üç test |
| k5 / KK5 her iptal yalnız kendi tetikleyenleriyle; B-7 | ✓ | KK5 testleri; girdi reddi `TheAdministratorsCancellation_WithoutItsInputs_...KK5` (5 durum) |
| k6 / KK6 kapanışla sebepsiz, B-7 yok | ✓ | KK6 üç test |
| k7 / KK7 onaysız ve yönetici dışı ret; yalnız açık fiziksel kalem; B-5 | ✓ | KK7 testleri; kargo verisi reddi `S5_WithInvalidCarrierData_...` (7 durum) |
| k8 / KK8 S6 sonrası kendiliğinden geçiş yok | ✓ | KK8 testleri |
| k9 / KK9 S7'de B-6 | ✓ | KK9 testleri |
| `09 §25` geçiş satırları: durum yalnız geçiş birimiyle, işlem sırası sabit, olay yayını yok, ret işlemi geri alır | ✓ | `Architecture/TransitionUnitSourceScan` yeşil (`OrderTransitions.Shipment.cs` `ApplyTransition` anmaz); her ret testi `AssertNothingWrittenAsync` ile tablolardan okur |
| `09 §25` yeni değer: yeni enum `TransitionRefusal` hata kodu değil, saklanmaz | ✓ | `ApplicationEnums.NotStored` yalnız bu tam ad; `EnumSourceScanTests`, `EnumLockTests`, `EnumTableConsistencyTests` yeşil |
| Güvenlik taraması | ✓ | Yeni dış girdi yok; kargo verisi liste dışında kırpılır ve boşsa reddedilir; SQL yok, EF sorguları; kişisel veri yeni günlüğe girmedi |

Sık FAIL nedenleri (`sik-fail-nedenleri.md` ilk 5):

| # | Neden | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Bekçi/tarama istisnası geniş | ✓ | Yeni istisna yalnız `ApplicationEnums.NotStored` içinde tek tam ad; tarama kuralları gevşemedi |
| 2 | Eşitlik denetimi tek yönlü | ✓ | `TheRefusalsOfTheOpenFace_AreTheCoresRefusals_ExceptTheCorrections_KK1` iki yönlü karşılaştırır (`Except` + tam dizi eşitliği) |
| 3 | 06'daki Varsayılan DB default'u değil / FK dizini eksik | ✓ | Şema değişikliği yok; yeni sütun, FK ya da göç yok |
| 4 | Kural değişince aynı sayıyı anan öteki yerler | ✓ | `.claude/CONTEXT.md` Ordering satırı güncellendi; D-291 hedefi T317'ye daraltıldı; Aktif kalem 120 → 125 |
| 5 | Test beklenen değeri sabitten türetmiyor | ✓ | Beklenen kodlar `TransitionCode`, `DispatchType`, `TransitionRefusal` üyeleridir; aktör kümeleri `TransitionTable.ActorsOf`'tan okunur; kargo şirketi listesi `CarrierCatalog`'dan (`Listed` örneği) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build --filter-namespace Shopfolio.UnitTests.Architecture` ve `--filter-not-namespace ...Architecture` | Architecture 868 yeşil; öteki 3039 yeşil (Ordering 63, bunun 8'i yeni) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests --no-build --filter-namespace Shopfolio.IntegrationTests.Ordering` (PostgreSQL 18 konteyneri) | 122 yeşil: `ShipmentTransitionsTests` 91, `OrderTransitionsTests` 31 |
| Contract | — | — | Bu task'ta sözleşme testi türü yok |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes` (üç proje) | temiz |
| Derleme | ✓ | `dotnet build ... -warnaserror` (Web, Unit, Integration) | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor (yapım chat'i doldurmaz) |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T316-sevkiyat-ekseni`
- **Commit:** kod başlığı `T316: Sevkiyat ekseni testleri: Integration derlendi ve koşuldu, Unit sözleşme testleri, TransitionRefusal kendi dosyasında`; ilk WIP `876a289`
- **PR:** #437
- **CI:** yapım chat'i izlemez; yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok | Yeni dış sistem, paket ya da ortam bağımlılığı eklenmedi; bildirim modülünün kuyruğu T306/T307'nin mevcut arayüzüdür | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İptalin yan etkileri (stok, kupon, iade, kalem iptal kayıtları) | D-305 | İptal task'larının (T348, T367…T370) işi |
| 2 | Müşteri iptalinde kalem iptali ile kapanış kuralı arasındaki yol seçimi ve B-7 kalem biçimi | D-306 | Kalem iptali task'ları (T367, T372) |
| 3 | S1+Ö1 kartı çağıranları | D-307 | T317 ve T318 çağırır |
| 4 | Panel ekranı ve ret → hata kodu eşlemesi | D-308 | T527 |
| 5 | Takip numarası uzunluk sınırı belgede yok | D-309 | Belgede olmayan sabit eklenmez |
| 6 | D-291 (b): sevkiyat ekseni kapandı, T317'de ödeme ekseni kalır | D-291 | T317 |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** devam işçisi; ilk işçinin adımı
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Çalışma yeri notu:** devam işçisi dalı kendi worktree'sine `task/T316-sevkiyat-ekseni-b` adıyla aldı ve `task/T316-sevkiyat-ekseni`'ne itti (ilk işçinin worktree'sine git erişimi aracı tarafından engellendi). Bu işçi `origin/main`'i dala birleştirdi.
- **⚠ Kolay kararlar (kurala en uygun seçenekle verildi):**
  1. S1+Ö1 kartı T316'dadır (T317/T318 çağırır): havalede önce yönetici Ö1, sonra S1 `approvesPayment=false`; kartta `true` (D-307).
  2. S6 işareti yalnız açık fiziksel kalemlere; S5/S8/S6 açık fiziksel kalem şartı arar.
  3. Müşterinin ödenmiş sipariş iptali yalnız `Item` (son açık kalem) ile işler; ödenmemiş siparişte bütün sipariş iptal edilir ve Ö2 birlikte yazılır (D-306).
  4. Sistem iptali yalnız Alındı + Bekliyor siparişte; S4 ve S9 sistem aktörüyle yalnız kapanış olarak oluşur.
  5. S2/S10/S11 için ayrı yöntem yok; T315 `EvaluateClosureAsync` kullanılır.
  6. Takip numarası uzunluk sınırı konmadı (belgede yok; D-309).
  7. `TransitionRefusal` açık yüzde bir enumdur; modül sözleşmesi enum'ları saklanan sayıldığı için `ApplicationEnums.NotStored`'a tek adla eklendi.
  8. Test sağlayıcısı: bildirim modülü Ayar modülü, denetim izi ve bağlantı anahtarlarını ister; `TransitionTestServices` ikisinin sağlayıcılarını ortak kurar.
