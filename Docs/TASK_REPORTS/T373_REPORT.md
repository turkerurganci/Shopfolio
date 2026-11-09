# T373 — İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Teslim alma geçişi** `OrderTransitions.ReceiveReturnAsync`: sipariş kilidi, ekran sürümü, `AfterSalesModule.AddReturnReceiptAsync`; ulaşma tarihi ileri olamaz (geçiş), alt sınırı kalemin girilmiş teslim tarihidir (beyan tarihi değil — mal beyandan önce gönderilmiş olabilir, K-554); teslimden önce cayılan gönderide alt sınır yok. Stok değişmez; kupon hakkı son adet kapanınca `_counters.ReturnAsync(order, [])` ile döner — "mal dönmedi" kapanışını açan geç teslim almada da bir kez (düzeltme 1, B1).
- **Stoğa ekle** `OrderTransitions.AddReturnedStockAsync` + `AfterSalesModule.MarkReturnStockAddedAsync`: teslim alınan adet kadar, `Return.StockAddedAt` bir kez; varyant yoksa sunulmaz (`ConditionNotMet`).
- **"Mal dönmedi" kapanışı** `OrderTransitions.CloseAsNotReturnedAsync` + `AfterSalesModule.AddNotReturnedClosureAsync`: yalnız teslim sonrası cayma, Z-42 dolmadan ret, üst sınır yoldaki adet; geri ödeme ve bildirim yok; paylaşılmayan IBAN `DeleteUnusedIbansAsync` ile silinir (K-1127). Kapanıştan sonra gelen mal kapanışı `ReopenedAt` ile yeniden açar.
- **IBAN yeniden istenir** (havale, teslim sonrası beyanın IBAN'ı silinmişse): teslim alma `RefundIbanRequest` (`ReturnAfterIbanDeleted`) açar, B-14 kuyruğa girer; firma IBAN girmez.
- **Ret** (T372'de kuruldu): `RejectReturnAsync`'e paylaşılmayan IBAN'ın silinmesi eklendi; `ClosedWithdrawalUnitsAsync` aynı işlemdeki kaydedilmemiş ret/kapanışı da sayar.
- Yeni sözleşmeler: `ItemRecordRefusal.SendingPeriodNotOver`, `ReceiptNotFound`, `StockNotAddable`; `NotReturnedClosureEntry`, `ReturnStockEntry`, `ReturnStockResult`; `ItemRecordWriteResult.IsIbanRequested`; istekler `ReceiveReturnRequest`, `CloseAsNotReturnedRequest`, `AddReturnedStockRequest`.
- İade kargo bedeli, etiket, taşıyıcı, kesinti, değer kaybı için hiçbir alan/ayar eklenmedi; yokluk birim testiyle sabitlendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.RefundLine.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordRefusal.cs`, `Contracts/ItemRecordWrites.cs`
- `src/Shopfolio.Web/Modules/Ordering/OrderTransitions.Returns.cs` (yeni), `OrderTransitions.ItemRecords.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/ItemRecordRequests.cs`
- Düzeltme 1: `src/Shopfolio.Web/Modules/Ordering/OrderCounters.cs` (`IsCouponReturnDueAsync`), `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.cs` (`UnitFactsAsync`), `Contracts/IAfterSalesModule.cs` (belge yorumu)
- `tests/Shopfolio.IntegrationTests/Ordering/ItemRecordsTests.Returns.cs` (yeni, 9 test; düzeltme 1'de +2), `tests/Shopfolio.UnitTests/AfterSales/ReturnShapeTests.cs` (yeni, 3 test)
- Migration yok (tablolar T320/T318'de).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İade kargo bedeli firmaya ait, ayarı yok, önden ödeme yok | ✓ | `ReturnShapeTests.NoSettingOfTheShop_NamesTheReturn_AndNoReturnRecordCarriesAShippingCost_KK1` |
| 2 | Taşıyıcıyı müşteri seçer; etiket ve geri alma teklifi yok | ✓ | `ReturnShapeTests.NoReturnRecordCarriesALabel_ACarrier_OrAPickup_KK2` |
| 3 | Z-42 dolunca otomatik işlem yok; dolmadan kapanış reddedilir | ✓ | `Closure_IsRefusedBeforeTheSendingPeriodEnds_NothingHappensByItself_AndAfterItTheIbanGoes_KK3_KK10` |
| 4 | Ulaşma tarihi ileri ya da teslimden önce olamaz; teslimden önce cayılanda alt sınır yok | ✓ | `Receipt_RefusesAFutureDate_AndADateBeforeTheDelivery_ButTakesOneBeforeTheDeclaration_KK4`, `Receipt_OfAParcelWithdrawnBeforeItsDelivery_HasNoLowerBound_KK4` |
| 5 | Stoğa kendiliğinden eklenmez; ayrı adım, adet kadar | ✓ | `Receipt_LeavesTheStock_AndTheStockStepAddsTheReceivedUnitsOnce_KK5`, `StockStep_IsNotOffered_WhenTheVariantIsGone_KK5` |
| 6 | IBAN silindikten sonra ulaşan malda istek + B-14; firma IBAN girmez | ✓ | `Receipt_AfterTheClosure_ReopensIt_AndAsksForTheDeletedIbanAgain_WithB14_KK6_KK10`, `Receipt_WhileTheIbanStands_AsksForNoIban_KK6` |
| 7 | Ret yalnız koşullu istisnalı kalemde | ✓ | `ItemRecordsTests.RejectReturn_OnAnItemWithoutAConditionalExemption_IsRefused_AndWritesNothing` (T372; `AddReturnRejectionAsync` → `NotConditionalExemption`) |
| 8 | Ret: geri ödeme yok, stok yok, IBAN silinir, B-16, geri alınmaz | ✓ | `Rejection_DeletesTheUnsharedIban_AddsNoStock_AndStartsNoRefund_KK8`; geri alınmazlık T372 testleri |
| 9 | Kullanılmış/hasarlı mal reddedilmez; kesinti/değer kaybı yok | ✓ | `ReturnShapeTests.NoReturnRecordCarriesADeduction_OrALossOfValue_OrAGoodsCondition_KK9` |
| 10 | Z-42 sonrası "mal dönmedi"; geri ödeme/bildirim yok; paylaşılmayan IBAN silinir; geç mal kalemi açar | ✓ | `Closure_…_KK3_KK10`, `Closure_KeepsTheIban_WhileAnotherPendingWithdrawalUsesIt_KK10`, `Receipt_AfterTheClosure_ReopensIt_…_KK6_KK10`; düzeltme 1 (B1): `Receipt_ThatReopensTheClosure_OfTheOrdersLastUnit_GivesTheCouponRightBack_Once_KK10` (tek adetli kuponlu sipariş: kapanışta `used_count` 2 kalır, geç teslim almada 1, stoğa eklemede yine 1; düzeltmesiz kodda kırmızı koşturuldu: Expected 1, Actual 2), `Receipt_OfTheOrdersLastUnit_GivesTheCouponRightBack_Once` (kapanışsız yol: 2 → 1, stoğa eklemede çift dönüş yok) |

### Öz denetim (bitiş kapısı 2)

Bütün maddeler (önceki işçinin yaptıkları dahil) diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 iade masrafı ayarı yok | ✓ | `ReturnShapeTests` KK1 (Settings ad alanında Return* yalnız iade adresi alanları) |
| KK2 etiket yok, taşıyıcı müşteride | ✓ | `ReturnShapeTests` KK2 |
| KK3 süre dolunca otomatik işlem yok | ✓ | `Closure_…_KK3_KK10` (son gün ret, saat ilerledi, kayıt/IBAN değişmedi) |
| KK4 tarih sınırları | ✓ | iki KK4 testi |
| KK5 stoğa ekleme ayrı adım | ✓ | iki KK5 testi |
| KK6 IBAN yeniden istenir, B-14 | ✓ | iki KK6 testi |
| KK7 ret yalnız koşullu kalemde | ✓ | `RejectReturn_OnAnItemWithoutAConditionalExemption_…` |
| KK8 ret sonuçları, IBAN, B-16 | ✓ | KK8 testi |
| KK9 kesinti/değer kaybı alanı yok | ✓ | `ReturnShapeTests` KK9 (ret nedenleri dahil) |
| KK10 erken kapatma reddi; paylaşılan IBAN kalır; geç mal açar | ✓ | üç KK10 testi; düzeltme 1: kapanışı açan teslim alma kupon hakkını bir kez döndürür (iki kupon testi) |
| Kapanış yeniden açılınca sonuçları (sık FAIL, T373 r1) | ✓ (düzeltme 1) | Kural tek evde: `OrderCounters.IsCouponReturnDueAsync` "hak önce dönmemişti, şimdi dönüyor" (`ReturnsRight(after) && !ReturnsRight(before)`; `ReturnsRight` = `IsFinal && !HasLoss`, `IsCouponReturnedAsync` aynı yöntemi okur); `AfterSalesModule.UnitFactsAsync(includeUnsaved: true)` kapanışı bağlamın tuttuğu hâliyle okur (`LocalAsync`, `RefundAmounts.IsWithdrawalTriggerAsync` emsali) — aynı işlemde yazılan `ReopenedAt` sayılmaz; `includeUnsaved: false` (önce) veritabanından açık kapanışı kayıp sayar. Davranış yalnız kaybın kalktığı hâlde değişir (ret kalıcı; kaybı yalnız teslim almanın kapanışı açması kaldırır). Teslim alma işlemindeki öteki okumalar: `WithdrawalProgressAsync` `ReopenedAt` yazılmadan önce okunur; iade tetiği `LocalAsync` ile zaten bağlamı okur; "mal dönmedi" bekleme işareti `Receipt_AfterTheClosure_…` testinde |
| Devredilen backlog kalemleri | ✓ | D-523, D-877 yerinde ✅; D-453 T373 payı kapandı; D-483 T373 payı (kupon hakkının teslim almada dönüşü) düzeltme 1'de iki Integration testiyle kanıtlandı; D-915 (K1) belge açık, F3 gate |
| `09 §25` ilgili satırlar | ✓ | geçişler `09 §9.1` madde 4 sırasını izler (kilit → sürüm → yazıcı → sayaç → sürüm bir kez → bildirim); modül sınırı: kayıt ve IBAN AfterSales'te (`09 §6` madde 2); döngüde sorgu yok (yeni kod) |
| Güvenlik taraması | ✓ | geçişler yalnız yönetici aktörü (`TryAdminActor`); ham SQL yalnız testlerde; yeni uç nokta yok; IBAN loglanmaz |

## Test Sonuçları

| Koşu | Sonuç |
|---|---|
| Unit (`--filter-not-trait Consistency=true`) | 5448 / 5448 ✓ |
| Unit Consistency | 291 / 291 ✓ |
| Integration `Ordering.ItemRecordsTests` | 61 / 61 ✓ |
| Integration `Ordering` ad alanı (son kod) | 566 / 566 ✓ |
| Integration tam (yerel, Windows, tek Postgres 18 `track_commit_timestamp=on`) | 3336 / 3379 — 43 hata yalnız `Web.RequestLimitsTests` (33) ve `Logging.LoggingFilesTests` (10): "HeartbeatRecord row is missing" (başlangıç denetimi). İki sınıf ayrı koşuda 48 / 48 ✓. Diff bu alanlara dokunmuyor; tam koşuda paylaşılan veritabanında başlangıç verisini boşaltan bir testle (ör. `StartupDatabaseCheckTests.EmptyTableAsync`) paralel çalışma şüphesi ⚠ |
| `dotnet format Shopfolio.slnx --verify-no-changes` | temiz |

**Düzeltme 1 sınaması** (konteyner `shopfolio-t373-fix1-pg`, postgres:18, `track_commit_timestamp=on`): `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı / 0 hata; `dotnet format --verify-no-changes` Web, Unit, Integration temiz; Unit 5448/5448 (Architecture dahil) + Consistency 291/291; Integration `--filter-namespace` Ordering 568/568, Payment + Concurrency + Disposal 101/101, Model 1404/1404 (Integration'da ayrı AfterSales ad alanı yok; AfterSales yazıcıları Ordering testlerinden geçer). Yeni iki kupon testi düzeltmesiz kodda: kapanış vakası kırmızı, kapanışsız vaka yeşil.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS — tur 2 (2026-11-10, head `ef958abf`; `origin/main` `338279ea` dal tabanı, birleştirme gerekmedi; kapsam TAM). Kriter 1–10 ve KK1–KK10 ✓; `09 §25` satırları evet — B1 kapandı: `UnitFactsAsync(includeUnsaved: true)` kapanışı bağlamdan okur (`LocalAsync`, aynı işlemde yazılan `ReopenedAt` sayılmaz), `IsCouponReturnDueAsync` = `ReturnsRight(after) && !ReturnsRight(before)`. Yapımın iki kupon testine ek olarak doğrulayıcının üç geçici çok adetli sınaması (silindi) geçti: 2 adet, 1 teslim alma → 1 kapanış → geç 1 teslim alma: `used_count` 2→1; 2 adetlik kapanış → 1 teslim alma (kapanış bütünüyle açılır, 1 bekler): hak dönmez → 2. teslim alma: döner; 2 adetlik kapanış → 1 teslim alma → kalan 1 yeniden kapanış: hak dönmez. IBAN silme kuralı main'deki tek evden (`IAfterSalesModule.DeleteUnusedRefundIbansAsync`, T370). Ana dal CI son 3 yeşil (42691939512, 42691939453, 42680032675). Dal CI 42707150031 (head `ef958abf`) dokuz iş yeşil. Yerelde: derleme 0 uyarı / 0 hata (`-warnaserror`), `dotnet format --verify-no-changes` üç proje temiz, Unit 5448/5448 + Consistency 291/291, betik 4/4, Integration Ordering 568/568 + öteki ad alanları 2813/2813 (tam 3381/3381), `restore --locked-mode` 0. |
| Bulgu sayısı (tur 2) | 0 |
| Bloke-etmeyen not (tur 2) | 1 — K1: dal CI 42699862671 (head `0d7d232f`) Integration'da `StartupSettingsCheckTests.Start_WithARequiredSettingMissingOrMalformed_…` `ObjectDisposedException` ile düştü; sonraki iki koşu ve yerel tam koşu yeşil, diff başlangıca dokunmaz (D-115 yeniden görüldü) → D-919 (F3 gate). |
| Task satırları (tur 2) | VAL-A220 `PASS` kaldı; VAL-A073, A075, A078, A218, A219, A223, A225, A226, A227, C052, C053 `BEKLEMEDE` (uç yanıtı / tarayıcı / sahte gönderim uçtan uca; T372/T374 emsali); `12_matris_denetle.py --kapsam 1-77` Hata 0. |
| Yapım raporu karşılaştırması (tur 2) | 1 uyuşmazlık: tam Integration için yapım 3336/3379 (43 "HeartbeatRecord row is missing") bildiriyor; doğrulayıcının iki parçalı koşusunda hata yok — ortam kaynaklı (tur 1'deki gibi). Öz denetim tablosu kabul kriteri ve KK başına kanıtlı; B1 düzeltmesinin kanıtı (iki test, düzeltmesiz kırmızı) raporda. |
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-10, head `0d7d232f`; `origin/main` `338279ea` dal tabanı, birleştirme gerekmedi). Kriter 1–10 ve KK1–KK10 ✓ (kriter 10 modül payı); `09 §25` "kod hiçbir doküman cümlesiyle çelişmiyor mu?" hayır (B1) → FAIL. Ana dal CI son 3 run yeşil (42691939512, 42691939453, 42680032675). Dal CI 42699862671 (head `0d7d232f`) koşuyordu (Lint, Build, Contract, E2E pass). Yerelde: derleme 0 uyarı / 0 hata (`-warnaserror`), `dotnet format --verify-no-changes` üç proje temiz, Unit 5448/5448 + Consistency 291/291, betik testleri 4/4, Integration Ordering 566/566, Integration tam 3379/3379 (yapım raporunun 43 "HeartbeatRecord row is missing" hatası bu koşuda yok — ortam kaynaklı). |
| Bulgu sayısı | 1 — **B1 ORTA S1** (`06 §4.14`, §4.28; `02 §7.4.8`, §10.4.9; `03 §1.4.4.6`; K-1104, K-1055; D-483 T373 payı): "mal dönmedi" kapanışı geç gelen malın teslim almasıyla bütünüyle açılınca (`ReopenedAt`) bu teslim alma siparişin son adedini kapatıyorsa kupon hakkı dönmeli; dönmüyor. `ReceiveReturnAsync` → `_counters.ReturnAsync(order, [])` → `IsCouponReturnDueAsync`: (a) `UnitFactsAsync(includeUnsaved: true)` kapanışı veritabanından `ReopenedAt == null` ile okur, aynı işlemde yazılan `ReopenedAt`'i görmez → `HasLoss(after)` true; (b) "önce" hâli açık kapanışla `AwaitingReceipt` 0 → `IsFinal(before)` true. Geçici sınama (doğrulayıcı, silindi): tek adetli kuponlu sipariş, kapanış + geç teslim alma → `used_count` 1 kaldı; kapanışsız kontrol vakası 1→0. |
| Bloke-etmeyen not (K) sayısı | 2 — K1: ret `includeClosed: false` ile açık "mal dönmedi" kapanışının adetlerini reddedemez (`06 §4.26`, §4.27; `02 §7.4.7` — belge açıkça söylemiyor) → D-915 (F3 gate). K2: D-879'un hedefi T373'tü, yapım D-911'e ileri attı; kriterler `02 §7.5.3`'ü içermez → kapandı (D-911 yerinde). |
| Task satırları (`12 §7.5`) | VAL-A220 → `PASS` (Veri kaydı, kanıt ortamı yok); VAL-A073, A218, A219, A225, A226, A227, C053 (uç yanıtı / tarayıcı), VAL-C052, A223 (iş kaydı / uygulama kaydı, sahte gönderim uçtan uca sınanmadı), VAL-A078 (1. hâl), A075 (T370 ile ortak, uç yok) → `BEKLEMEDE` (T372/T374 emsali); `12_matris_denetle.py --kapsam 1-77` Hata 0. |
| Yapım raporu karşılaştırması | 2 uyuşmazlık: (1) yapım raporu "kupon hakkı son adet kapanınca `_counters.ReturnAsync(order, [])` ile döner" ve D-483 "T373 payı kapandı" diyor; kapanışı açan teslim almada dönmüyor (B1), kupon dönüşünün testi yok. (2) Tam Integration: yapım 3336/3379 (43 ortam hatası) bildiriyor; doğrulayıcının koşusunda 3379/3379 — ortam kaynaklı olduğu doğrulandı. Öz denetim tablosu kabul kriteri ve KK başına kanıtlı. |
| Düzeltme gerekli mi | Tur 1: evet — B1 (düzeltme 1'de yapıldı). Tur 2: hayır |

## Altyapı Değişiklikleri

- Yok (migration, paket, CI değişikliği yok).

## Commit & PR

- Dal `task/T373-iade`; PR base `main`. PR: #507
- Doğrulama tur 1 kayıt commit'i `4fc009f7` (doğrulayıcının bağlam sınırında yazamadığı kayıt, düzeltme işçisi yazdı).
- **Düzeltme 1 (2026-11-10, tur 1 FAIL sonrası):**
  - **B1:** "mal dönmedi" kapanışını geç gelen malın teslim alması açınca kupon hakkı siparişin son adediyle bir kez döner. `UnitFactsAsync(includeUnsaved: true)` kapanışı bağlamdan okur (aynı işlemde yazılan `ReopenedAt` görülür); `IsCouponReturnDueAsync` "önce" hâlini kayıpla bitmiş sayar (`ReturnsRight(after) && !ReturnsRight(before)`). Kopya kural yazılmadı; `IsCouponReturnedAsync` aynı `ReturnsRight`'ı okur.
  - **K1 → D-915:** belge açık; koda dokunulmadı, F3 gate.
  - Öz denetim: kriter 1–10 ve KK1–KK10 düzeltilmiş diff'te yeniden denetlendi; değişiklik yalnız kupon hakkı kararını (kriter 10 / KK10, `06 §4.14`) etkiler, öteki satırların kanıtı değişmedi. `sik-fail-nedenleri.md` okundu: tek kural evi (kopya yok), kapanışın yeniden açılması sonuçları, ikinci yazma yolu (stoğa ekleme sonrası çift dönüş yok) testli.

## Dış Varsayımlar

- Yok.

## Known Limitations / Follow-up

- **D-910** (D-876'nın devamı): Z-38 kaynak konu kümesi genişletmesi — F3 gate.
- **D-911** (D-879'un devamı): ayıpta dönmenin malının teslim alma adımına bağlanması — F3 gate.
- **D-912** (D-907'nin devamı): ayıp geri ödemesinin "havale gerçekleşmedi"sinde dosyadaki IBAN. Önceki işçinin "en son girilen IBAN" yorumuyla yazdığı değişiklik testsiz olduğu için geri alındı ⚠.
- D-903, D-568 (k-note) hedefi F3 gate'e alındı.
- **D-915** (doğrulama tur 1 K1): ret açık "mal dönmedi" kapanışının adetlerini reddedemez — belge açık, F3 gate.
- Kapananlar: D-523, D-877; D-453 ve D-483'ün T373 payları.

## Notlar

⚠ Kolay konularda verilen kararlar:
1. Teslim alma tarihinin alt sınırı beyan tarihi değil teslim tarihi (kriter 4'ün metni ve K-554); önceki `ArrivalDateOutOfRange` testleri bu kuralla geçiyor.
2. D-907 kodu geri alındı, D-912 ile ileri atıldı ("dosyadaki IBAN" tanımı testle sabitlenmeli).
3. D-910/D-911/D-912 hedefi F3 gate (yeniden hedeflenecek).
4. Yerel tam Integration koşusundaki 43 hata ortam kaynaklı sayıldı (yukarıda).
