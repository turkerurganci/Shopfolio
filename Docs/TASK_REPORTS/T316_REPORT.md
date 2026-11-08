# T316 — Sipariş durumu geçişleri: sevkiyat ekseni

**Faz:** F3 | **Durum:** ✓ Doğrulama tur 2 PASS | **Tarih:** 2026-11-07

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
| 5 | S3 müşteri/sebepli yönetici/sistem, S4 müşteri/sebepli yönetici, S9 sebepli yönetici; B-7 gider | ✓ | `S3_ByTheCustomer_...KK5`, `S4_ByTheCustomer_...KK5`, `TheAdministratorsCancellation_ByTheState_...KK5`, `S3_BySystem_...KK5`, `TheOtherActors_AreRefusedByEachCancellation_...KK5`, `TheCancellationsTriggers_AreTheOnesOfTheTable_KK5`; düzeltme r1: `TheAdministratorsWholeOrderCancellation_IsRefused_WhileAnItemIsMarkedDelivered_AndWritesNothing_KK5` (S3, S4, S9) |
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
| k5 / KK5 her iptal yalnız kendi tetikleyenleriyle; B-7; teslim işaretli kalemli sipariş İptal edildi'ye geçmez | ✓ | KK5 testleri; girdi reddi `TheAdministratorsCancellation_WithoutItsInputs_...KK5` (5 durum); `CancellationConditionAsync` kalemsiz yolda `DeliveryMarkedAt != null` kalem varken false; `TheAdministratorsWholeOrderCancellation_IsRefused_...KK5` S3 (hizmet açık + teslim dijital), S4, S9 (fiziksel açık + teslim dijital): ret `ConditionNotMet`, `AssertNothingWrittenAsync` |
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
| Unit (düzeltme r1, `origin/main` birleştirmesi sonrası) | ✓ | `--filter-namespace` Architecture*, Consistency* ve `--filter-not-namespace` ikisi | Architecture 906; Consistency 230; öteki 2840 yeşil |
| Integration (düzeltme r1) | ✓ | `--filter-class` `ShipmentTransitionsTests`, `OrderTransitionsTests` + `OrderTransitionsDefinition` (PostgreSQL 18 konteyneri) | 125 yeşil: `ShipmentTransitionsTests` 94 (91 + B1 için 3), öteki 31 |
| Contract | — | — | Bu task'ta sözleşme testi türü yok |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes` (üç proje) | temiz |
| Derleme | ✓ | `dotnet build ... -warnaserror` (Web, Unit, Integration) | 0 uyarı, 0 hata |

## Düzeltme (tur 1 → tur 2)

- **B1 (ORTA, S1) giderildi:** `OrderTransitions.Shipment.cs` `CancellationConditionAsync` — sistem ve kalem iptali dışındaki (yöneticinin ve müşterinin bütün sipariş) yolunda, teslim işaretli (`DeliveryMarkedAt`) kalem varsa koşul sağlanmaz; S3, S4 ve S9 `ConditionNotMet` ile reddedilir, hiçbir şey yazılmaz (`02 §5.4`, §7.2.2, §7.2.5; `03 §1.2`).
- **Test:** `ShipmentTransitionsTests.TheAdministratorsWholeOrderCancellation_IsRefused_WhileAnItemIsMarkedDelivered_AndWritesNothing_KK5` — üç durum (Alındı/S3, Hazırlanıyor/S4, Teslim edilemedi/S9), her biri ret + durum, sürüm, kayıt ve bildirim satırı değişmedi.
- `origin/main` birleştirildi; kayıt belgesi çakışmaları çözüldü. Backlog değişmedi (D-340–D-341 gerekmedi).
- Öz denetim: kriter 1–9 ve KK1–KK9 düzeltilmiş diff'te yeniden denetlendi (yukarıdaki tablo).

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 2 (2026-11-07; head `2d04809` + `origin/main` birleştirmesi — çakışma yalnız kayıt belgelerinde); tur 1 ✗ FAIL (B1) |
| Bulgu sayısı | Tur 2: 0 · tur 1: 1 (B1 ORTA S1, bu PR'da giderildi) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 2 — ✓ PASS (2026-11-07)

Kapsam tam (dokuz kriter, KK1–KK9, `09 §25`, güvenlik); önceki tur raporu okunmadan bağımsız üretildi.

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` kurulu |
| Ana dal CI (son 3) | ✓ | 42402532255, 42401437836, 42401437807 success |
| Memory drift | ✓ | MEMORY'de T316 satırları var |

#### Kabul Kriterleri (validator)
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `TransitionTable.Shipment` 11 satır, geri yön yok; her işlem `TryPlan`'den geçer (kaynak ≠ `From` → `NotInTheTable`); `CorrectAsync` tabloyu okumaz; KK1 testleri yeşil |
| 2 | ✓ | Terminalden çıkan satır yok; kapanış kuralı terminalde erken döner; KK2 (17) yeşil |
| 3 | ✓ | Fiziksel kalem koşulu; Ö1+S1 tek kilit/sürüm, B-4 yalnız onaylı yolda; onaysız + Bekliyor çift kuralıyla ret; KK3 yeşil |
| 4 | ✓ | S2/S10/S11 yöntemi yok; kapanış işlem sonunda; KK4 yeşil |
| 5 | ✓ | Aktör/sebep (`TryCancellationActor`, `ActorsOf`, `ReasonsAllowed`) ve koşul (`CancellationConditionAsync`: sistem yalnız Alındı+Bekliyor; müşteri ödenmemişte bütün, ödenmişte son açık kalem; teslim işaretli kalemli bütün sipariş iptali ret); B-7 her kabulde bir; KK5 testleri (B1 testi dahil) yeşil |
| 6 | ✓ | S3/S4 kapanışı sistem + Closure, bildirim yok; S9 kapanışı yöneticinin sebepsiz işlemi, açık/teslimli kalemde ret; KK6 yeşil |
| 7 | ✓ | Yönetici, onay, kargo verisi normalizasyonu, yalnız açık fiziksel kalem, B-5; KK7 yeşil |
| 8 | ✓ | Tarih ileri ve son S5/S8 gününden önce olamaz; açık fiziksel kalemlere işaret; bildirim yok; KK8 yeşil |
| 9 | ✓ | Yönetici + sürüm, B-6 tek satır; KK9 yeşil |

#### Task Satırları (`12 §7.5`)
KP-46 satırları (`VAL-A207`, `A208`, `C039`…`C048`) uç yanıtı ister (D-308 → T527); T316 uç yazmaz — `Durum` hepsinde `BEKLEMEDE` → `BEKLEMEDE`. KD-24 satırı yok. `12_matris_denetle.py --kapsam 1-77` hata 0.

#### Test Sonuçları
| Tür | Sonuç | Çıktı |
|---|---|---|
| Derleme `-warnaserror` | ✓ | 0 uyarı, 0 hata |
| Biçim (Web, Unit, Integration) | ✓ | çıkış 0 |
| Unit Architecture / Consistency / öteki | ✓ | 906/906 · 230/230 · 2840/2840 |
| Betik testleri | ✓ | 4 dosya geçti |
| Integration (postgres:18) | ✓ | `ShipmentTransitionsTests` 94/94, `OrderTransitionsTests` 31/31, `ConcurrencyTests` 26/26 |
| Dal CI | ✓ | 42399324842 (head `2d04809`) yeşil |

#### Güvenlik
Secret temiz · auth temiz (uç yok; yönetici kimlik, ad ve sürüm zorunlu) · girdi doğrulaması temiz (kargo verisi, teslim tarihi, "Diğer" notu; uzunluk sınırı D-309) · yeni bağımlılık yok.

#### Bulgular
Yok. K-notu yok.

#### Yapım raporu karşılaştırması
Tam uyumlu: dokuz kriter ✓, test sayıları birebir; öz denetim tablosu kanıtlı.

### Doğrulama tur 1 — ✗ FAIL (2026-11-07)

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` kurulu |
| Ana dal CI (son 3) | ✓ | 42393301185, 42393301195, 42391220711 success |
| Memory drift | ✓ | MEMORY T316 satırı var |

#### Kabul Kriterleri (validator)
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `TransitionTable.Shipment` 11 satır, geri yön yok; KK1 testleri yeşil; `CorrectAsync` tablo reddine takılmaz |
| 2 | ✓ | Terminalden çıkan satır yok; KK2 17 satır yeşil |
| 3 | ✓ | Fiziksel kalem koşulu; Ö1+S1 tek işlem, tek B-4; KK3 yeşil |
| 4 | ✓ | S2/S10/S11 yöntemi yok, kapanış kuralı işlem sonunda; KK4 yeşil |
| 5 | ~ | Tetikleyenler ve B-7 doğru; ama yöneticinin kalemsiz iptali teslim işaretli kalemi denetlemez — sonda Alındı+Ödendi, hizmet + teslim işaretli dijital → `S3`, `Cancelled` (B1) |
| 6 | ✓ | Kapanış Closure nedeniyle, B-7 yok; S9 kapanışı açık/teslimli kalemde ret; KK6 yeşil |
| 7 | ✓ | Yönetici, onay, kargo verisi, yalnız açık fiziksel kalem, B-5; KK7 yeşil |
| 8 | ✓ | Tarih sınırları, bildirim yok, Delivered'da kapanış erken döner; KK8 yeşil |
| 9 | ✓ | B-6 tek satır; KK9 yeşil |

#### Task Satırları (`12 §7.5`)
KP-46 satırları `VAL-A207`, `A208`, `C039`…`C048` uç yanıtı ister (panel; D-308 → T527); T316 uç yazmaz, kritere eşlenmez (T305/T306 emsali) — `Durum` hepsinde `BEKLEMEDE` → `BEKLEMEDE`. `12_matris_denetle.py --kapsam 1-77` hata 0.

#### Test Sonuçları
| Tür | Sonuç | Çıktı |
|---|---|---|
| Derleme `-warnaserror` | ✓ | 0 uyarı, 0 hata |
| Biçim (Web, Unit, Integration) | ✓ | çıkış 0 |
| Unit Architecture / Consistency / öteki | ✓ | 868/868 · 230/230 · 2809/2809 |
| Betik testleri | ✓ | 4 dosya geçti |
| Integration Ordering (postgres:18) | ✓ | 122/122 |
| Dal CI | ✓ | 42393227539 yeşil |

#### Güvenlik
Secret temiz · auth temiz (uç yok; yönetici kimlik, ad ve sürüm zorunlu) · girdi doğrulaması temiz (kargo verisi, teslim tarihi, "Diğer" notu) · yeni bağımlılık yok.

#### Bulgular
| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Karar | Dosya |
|---|---|---|---|---|---|---|
| B1 | kriter 5 / KK5 | ORTA | S1 | Teslim işaretli (teslim edilmiş dijital, tamamlanmış hizmet) kalemli sipariş firma iptaliyle İptal edildi'ye geçmez (`02 §5.4` İptal edildi tanımı ve "Kalem ve adet düzeyinde iptal" maddesi, §7.2.2, §7.2.5; `03 §1.2`) ↔ `CancellationConditionAsync` kalemsiz yönetici isteğinde yalnız Teslim edilemedi'de açık kalem arar; S3, S4, S9 Cancelled yazar | Bu PR'da: yöneticinin kalemsiz yolu teslim işaretli kalem varken reddeder; S3, S4, S9 için sınanır | `OrderTransitions.Shipment.cs` · `ShipmentTransitionsTests.cs` |

#### Yapım raporu karşılaştırması
1 uyuşmazlık: kriter 5 yapımda ✓, doğrulamada ~ (B1). Öz denetim tablosu tam ve kanıtlı; test sayıları birebir.

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
- **Post-merge run'lar:** squash `924ef92` (#437) — Publish 42408991705 success; CI 42408991723 cancelled (aynı ref'in yarışan koşusu; sonraki squash'ların CI'ları yeşil: T334 42411903829, T321 42419095662) (T335 dalında dolduruldu)

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
