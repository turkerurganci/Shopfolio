# T320 — Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri

**Faz:** F3 | **Durum:** ⏳ Yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kayıt tablosu — `ItemRecordTable`** (`AfterSales/ItemRecordTable.cs`): `03 §1.7.1`'in on bir satırı tek yerde — kayıt türü (`ItemRecordKind`), hat etkisi (`LineEffect`: teslim, kapanış, iptal sayılır, teslim işaretine bağlı, etkisiz), adet ve yapan/an taşıyıp taşımadığı. `ClosesOpenQuantity` açık adetten düşen kayıtları (iptal, çıkarma, gecikme feshi, cayma) tablodan okur; `ItemQuantities` adet kuralını (`IsSelectable`: 1 ≤ adet ≤ açık adet; `Open`; `AwaitingReceipt`) taşır. Kalem (`OrderItem`) durum taşımaz.
- **AfterSales kayıt yazıcıları** (`AfterSalesModule.ItemRecords.cs`, `IAfterSalesModule`): `OpenDefectClaimAsync`, `ChangeDefectClaimStatusAsync`, `AddWithdrawalAsync`, `AddReturnReceiptAsync`, `AddReturnRejectionAsync`, `FindDefectClaimAsync`, `ItemMarksAsync`. Hepsi çağıranın işlemine katılır, sipariş satır kilidini şart koşar (`RequireOrderLock` → `LockSequence.LockedOrders`), `SaveChanges` çağırmaz; her beyan ve her parça teslim alma kendi satırını yazar (K-790, K-794); açık adet kaydedilmemiş satırları da sayar. Ret bir değerdir (`ItemRecordRefusal`, kendi dosyasında) ve hiçbir şey yazmaz.
- **Ayıp talebi durumu:** `DefectClaim.StatusHistory` tipli liste (`DefectClaimHistoryEntry(At, Status, ActorKind, Quantity)`, `PayloadJson.HasPayloadConversion`); `ApplyStatus` (`internal`, ayarlayıcı `private`) durumu ve geçmiş girdisini birlikte yazar; tek çağıranı `DefectClaimStatusWriter.TryChange` (Açık → Çözüldü yalnız yönetici; Çözüldü → Açık müşteri ya da yönetici; adedi yalnız müşteri artırır, azaltılamaz). Z-18 penceresi `DefectClaimWindow` (fiziksel kalemde kargo ya da teslim işaretiyle açılır; süre teslim tarihinden iki yıl, Türkiye saatiyle; teslim tarihi yokken işlemez).
- **Geçiş birimi işlemleri** (`OrderTransitions.ItemRecords.cs`, `IOrderTransitions`, `ItemRecordRequests.cs`): `OpenDefectClaimAsync` (B-9 müşteriye, F-3 firmaya), `ResolveDefectClaimAsync` (bildirim yok), `ReopenDefectClaimAsync` (müşteri: B-9 + F-3; yönetici: bildirim yok, ekran sürümüyle), `RejectReturnAsync` (yalnız koşullu istisna kalemi, geri alınamaz onay, varış tarihi bugünden sonra olamaz, B-16). Sıra: kilit → sipariş ve sürüm → kalem → AfterSales yazıcısı → `RaiseVersionOnce` → bildirim. Sipariş durumları değişmez, geçiş kaydı yazılmaz.
- **Panel işaretleri:** `ItemMarksAsync` "iade malı bekleniyor" (teslimden sonra beyan edilmiş, alınmamış/reddedilmemiş/kapanmamış adet) ve "IBAN bekleniyor" (karşılanmamış ve kapanmamış talep) işaretlerini her okumada kayıtlardan hesaplar; saklanmaz (K-675).
- **İmha uyumu:** `DefectClaimRetention.LastChange` tipli geçmişin son `At`'ını okur (D-374); `DefectClaimDisposalPart` aynı tipi kullanır.
- **Kaynak taraması** (`TransitionUnitSourceScan`): `ApplyStatus` yalnız `DefectClaimStatusWriter.cs`'te (tam yol), `ChangeDefectClaimStatusAsync` yalnız geçiş biriminin kayıt dosyası ve AfterSales yüzü/uygulamasında (D-274). Kasıtlı ihlal örneği `ViolationSamples/TransitionUnit/WriterAndSetters.cs` iki biçimi de taşır (satır 31 `ApplyStatus`, satır 32 `ChangeDefectClaimStatusAsync`; `TransitionUnitSourceScanTests` beklenen bulgu listesi).
- **Saat sahtesi:** `ItemRecordsTests` ortak `UnitTests/Fakes/ManualTimeProvider`'ı kullanır (4. işçi yerel `MovableClock`'u kaldırdı; `09 §19.2`).
- **Test düzenleri:** AfterSales artık `ISettingsModule` ister (iade adresi kopyası, K-817) → `TransitionTestServices.AddSettingsDependencies()`; `OrderingTestServices`, `DisposalHarness` çağırır.
- **Yük örneği** (`09 §12` m.8): `tests/Shopfolio.UnitTests/Payloads/DefectClaimHistoryEntry/1.json` ve `DefectClaimHistoryPayloadTests` (her örnek okunur, `[]` boş liste, bilinmeyen alan atlanır, eksik alan varsayılan, bugünkü yazım biçimi en son örnekle aynı alan kümesi).
- **Backlog:** D-67, D-274, D-374 ✅; yeni D-522–D-526 (`DEFERRED_BACKLOG.d/T320.md`).

## Etkilenen Modüller / Dosyalar

- AfterSales: `AfterSalesModule.cs`, `AfterSalesModule.ItemRecords.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordRefusal.cs`, `Contracts/ItemRecordWrites.cs`, `DefectClaim.cs`, `DefectClaimDisposalPart.cs`, `DefectClaimStatusWriter.cs`, `DefectClaimWindow.cs`, `ItemRecordTable.cs`
- Ordering: `OrderTransitions.ItemRecords.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/ItemRecordRequests.cs`
- Testler: `IntegrationTests/Ordering/ItemRecordsTests.cs` (16 test yöntemi), `Ordering/TransitionTestServices.cs`, `Ordering/OrderingTestServices.cs`, `Disposal/DisposalHarness.cs`, `Disposal/RecordDisposalTests.cs`; `UnitTests/Ordering/ItemRecordRulesTests.cs`, `Ordering/DefectClaimHistoryPayloadTests.cs`, `Consistency/ItemRecordTableConsistencyTests.cs`, `Architecture/TransitionUnitSourceScan.cs`, `Architecture/TransitionUnitSourceScanTests.cs`, `Architecture/ViolationSamples/TransitionUnit/WriterAndSetters.cs`, `Disposal/RecordRetentionTests.cs`, `Ordering/ShipmentTransitionContractTests.cs`, `Support/ApplicationEnums.cs`, `Payloads/DefectClaimHistoryEntry/1.json`
- Belgeler: `DEFERRED_BACKLOG.md` (D-67, D-274, D-374 yerinde ✅), `DEFERRED_BACKLOG.d/T320.md`, `IMPLEMENTATION_STATUS.md` (T320 satırı ⏳), `STATUS_CHANGELOG.d/T320.md`, bu rapor

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kalem durum taşımaz; kayıt kendi satırında yapanı, anı ve adedi taşır (teslim işareti ve indirme sayacı hariç) | ✓ | Integration `ItemRecordsTests.OrderItem_CarriesNoStatusOfItsOwn`, `Records_CarryTheActorTheMomentAndTheUnits_OnTheirOwnRows`; Consistency `ItemRecordTableConsistencyTests.QuantityAndActorColumns_FollowTheUnitSentence_ExceptTheDeliveryMarkAndTheDownloadCounter` |
| 2 | Her kaydın hatta sayılışı tabloyla uyar | ✓ | `ItemRecordTable.Rows`; Consistency `Rows_AreTheDocumentsRows_InOrder_WithTheSameLineEffect` (beklenen satırlar `03 §1.7.1`'den okunur), `TheOpenQuantity_LosesExactlyTheClosingRecords_OfTheUnitRule`; Integration `ClosedQuantity_MovesByTheRecord_AsTheTableSays` |
| 3 | Birim adet; açık adedi aşan reddedilir | ✓ | `ItemQuantities.IsSelectable`; Unit `ItemRecordRulesTests.IsSelectable_IsOneToTheOpenQuantity`, `Open_AndAwaitingReceipt_NeverGoBelowZero`; Integration `OpenDefectClaim_RefusesZero_AndMoreThanTheOpenUnits`, `AddWithdrawal_RefusesMoreThanTheOpenUnits_TheUnsavedOnesIncluded` |
| 4 | Ardışık beyanlar ve parça teslim almalar ayrı kayıt | ✓ | Integration `SuccessiveDeclarations_AndPartialReceipts_AreRowsOfTheirOwn`, `Writers_RequireTheOrdersLock_InTheCallersTransaction` |
| 5 | İade reddi yalnız koşullu istisnada, teslim alma adımında; B-16 | ✓ | `WithdrawalExemptions.IsConditional`, `AddReturnRejectionAsync` → `ReceiptRefusalAsync`; Unit `IsConditional_IsOnlyTheHygieneSensitiveAndTheSealedMedia`; Integration `RejectReturn_OnAnItemWithoutAConditionalExemption_IsRefused_AndWritesNothing`, `RejectReturn_OnAConditionalExemption_WritesTheRow_AndQueuesB16` |
| 6 | Kalem başına tek talep; Açık'ken yeni yok, Çözüldü'yken aynı kayıt yeniden açılır | ✓ | `OpenDefectClaimAsync` (`ClaimExists`), tekil dizin; Integration `SecondClaim_IsNotOpened_AndTheResolvedClaimIsReopened_OnTheSameRow`; Unit `Reopen_StartsFromResolved_TheCustomerMayRaiseTheUnits_NeverLowerThem` |
| 7 | Çözüldü'ye yalnız yönetici; yeniden açma müşteri/yönetici iki yıl içinde | ✓ | `DefectClaimStatusWriter`, `DefectClaimWindow`; Unit `Resolve_IsTheAdministratorsOnly_AndAppendsTheEntryWithTheStatus`, `Reopen_ByTheAdministrator_KeepsTheUnits_AndRecordsTheKind`, `Window_*` (3); Integration `Resolve_IsTheAdministratorsOnly`, `Reopening_OnTheLastDay_IsAllowed_AndTheDayAfter_IsRefused` |
| 8 | Doğuşta ve müşteri yeniden açışında B-9 + F-3; yönetici yeniden açışı ve çözüldü bildirimsiz | ✓ | `QueueClaimNoticesAsync`; Integration `ClaimNotices_GoToTheRightRecipients_OnlyOnTheBirthAndTheCustomersReopening` (B-9 müşterinin `ContactEmail`'ine, F-3 firma iletişim adresine) |
| 9 | Ayıp talebi sevkiyat hattını etkilemez; paralı çözüm ödeme ekseninden | ✓ | Geçiş biriminin ayıp işlemleri `ApplyTransition` çağırmaz, `TransitionRecord` yazmaz; Integration `ClaimOperations_LeaveTheOrdersStatuses_AndWriteNoTransition`; `DefectClaim` hattı etkilemez (`LineEffect.None`); paralı çözüm (dönme, geri ödeme) T374 (D-524) |
| 10 | Panel işaretleri durum değil; koşul sürdükçe görünür | ✓ | `ItemMarksAsync` (saklanmaz, her okumada hesaplanır); Integration `AwaitingReturnGoods_ShowsWhileUnitsAreOnTheWay_AndGoesWithTheLastOne`, `AwaitingIban_ShowsWhileTheRequestIsOpen_AndGoesWhenFulfilledOrClosed`; sipariş düzeyi işaretler sahiplerinde (D-526) |

### Öz denetim (bitiş kapısı 2)

Devam işçisi (4.) önceki üç işçinin yaptıkları dahil bütün maddeleri diff'te yeniden denetledi (`git diff origin/main...HEAD`).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 kalemde durum alanı yok; kayıt satırı yapanı, anı, adetli kayıtta adedi taşır | ✓ | `OrderItem_CarriesNoStatusOfItsOwn` (yansıma: `Status`/`State` adlı özellik yok); `Records_CarryTheActorTheMomentAndTheUnits_OnTheirOwnRows` (cayma, teslim alma, ret, talep, geçmiş girdisi); `ItemRecordTable` sütunları + Consistency testi |
| KK2 kayıt türü başına hat etkisi tabloyla aynı | ✓ | `ItemRecordTableConsistencyTests` (3 test, `03 §1.7.1` metninden okur); `ClosedQuantity_MovesByTheRecord_AsTheTableSays` |
| KK3 açık adedi aşan adet reddedilir | ✓ | `IsSelectable_IsOneToTheOpenQuantity`; `OpenDefectClaim_RefusesZero_AndMoreThanTheOpenUnits`; `AddWithdrawal_RefusesMoreThanTheOpenUnits_TheUnsavedOnesIncluded`; teslim almada yoldaki adet (`ReceiptRefusalAsync`) |
| KK4 iki beyan iki satır, iki teslim alma iki satır | ✓ | `SuccessiveDeclarations_AndPartialReceipts_AreRowsOfTheirOwn` |
| KK5 izinsiz kalemde iade reddi reddedilir | ✓ | `RejectReturn_OnAnItemWithoutAConditionalExemption_IsRefused_AndWritesNothing`; `IsConditional_IsOnlyTheHygieneSensitiveAndTheSealedMedia` |
| KK6 ikinci talep açılmaz, çözülen talep yeniden açılır | ✓ | `SecondClaim_IsNotOpened_AndTheResolvedClaimIsReopened_OnTheSameRow` |
| KK7 iki yıldan sonra yeniden açma reddedilir | ✓ | `Reopening_OnTheLastDay_IsAllowed_AndTheDayAfter_IsRefused`; `Window_ClosesAfterTheLastDay_TwoYearsFromTheDeliveryDate_InTurkeyTime` |
| KK8 bildirim satırları doğru alıcıya yazılır | ✓ | `ClaimNotices_GoToTheRightRecipients_OnlyOnTheBirthAndTheCustomersReopening` (alıcı adresi gönderim satırından okunur); B-16 `RejectReturn_OnAConditionalExemption_WritesTheRow_AndQueuesB16` |
| KK9 ayıp talebi sipariş durumunu değiştirmez | ✓ | `ClaimOperations_LeaveTheOrdersStatuses_AndWriteNoTransition` (durumlar aynı, sürüm her işlemde bir artar, geçiş kaydı yok) |
| KK10 koşul kalkınca işaret kalkar | ✓ | `AwaitingReturnGoods_…` (teslim alma, kapanış, yeniden açılan kapanış, ret), `AwaitingIban_…` (karşılanma, kapanma) |
| `09 §25` doküman uyumu | ✓ | Okunanlar (1.–3. işçi): çekirdek, `09` §9, `03 §1.7`, §1.9, §5.3, `02 §5.4`, §5.8, §5.10, §7.5, §7.5.3, Z-18, `05 §4.1.2`, `10` KP-23, `06 §4.22`–§4.30; 4. işçi: `09 §9.1`, §12, §25, `11` T320 bloğu. Yeni adlar `06`'dan (`DefectClaimStatus`, `ActorKind`, `WithdrawalExemptionReason`, tablo/sütun adları değişmedi); `09 §9.1` m.4 metin sapması karar kaydına not olarak D-525'te (09 metni değiştirilmedi); yeni ayar/süre yok — Z-18 iki yılı yasal süre olarak kuralın yanında (`09 §3.5` m.1, `DefectClaimRetention` ile aynı kalıp) |
| `09 §25` mimari | ✓ | Ordering yalnız `IAfterSalesModule` (`Contracts`) ile; AfterSales sipariş tablosuna yazmaz, siparişe ait olgular `ItemRecordSubject` ile verilir (`09 §6` m.2); arayüz yöntemleri işlem açmaz, `SaveChanges` çağırmaz (`09 §9.1` m.3); tek işlev: `TurkeyTime.DateOf`, `LockSequence`, `PayloadJson`, `ClosedQuantitiesAsync` yeniden kullanıldı; zaman `TimeProvider`; mimari testler yeşil |
| `09 §25` durum/eşzamanlılık | ✓ | Talep durumu yalnız geçiş biriminden, AfterSales yazıcısı üzerinden (tarama tam yolla, D-274); sipariş kilidi her işlemde ilk adım, yazıcı kilidi şart koşar (`Writers_RequireTheOrdersLock_InTheCallersTransaction`); yönetici işleminde ekran sürümü kilit altında; `RaiseVersionOnce` |
| `09 §25` işlem izi | uygulanamaz | `AuditOperation.RejectReturn` kodu var; geçiş biriminin hiçbir işlemi (S1–S11 dahil) bugün iz yazmıyor — iz satırı panel işlemlerinin kapı katmanında (sayfa task'ları); T320 KK'ları izi kapsamıyor |
| `09 §25` veri ve göç | ✓ | Göç yok (tablolar T1xx'te; `StatusHistory` sütunu `jsonb`, varsayılan `'[]'::jsonb` zaten var); yük biçimi örneği eklendi, eski örnek yok (önceki sürüm geçmişe hiç yazmıyordu, satırlar `[]`); yeni kapalı değer/hata kodu yok (`ItemRecordRefusal` saklanmayan sözleşme değeri, `TransitionRefusal`'a eşlenir) |
| `09 §25` güvenlik | ✓ | Sır ve kişisel veri yok (diff taraması); yeni POST/sayfa yok; yönetici işlemleri `TryAdminActor` ile, müşteri yeniden açışında yönetici/sürüm verilirse ret; açıklama boş geçilemez; varış tarihi gelecekte olamaz; yeni bağımlılık yok |
| `09 §25` test | ✓ | Kurallar Unit'te saf; kilit, tekil dizin ve bildirim satırları gerçek PostgreSQL'de; saat ortak sahte (`UnitTests/Fakes/ManualTimeProvider`); test silinmedi/zayıflatılmadı (değişen eski testler yalnız tip değişikliği: `RecordRetentionTests`, `RecordDisposalTests`) |
| Sık FAIL listesi | ✓ | (1) tarama istisnası dar: tam yol eşitliği, `other/src/...` öneki ve sayfa yolu kırmızı (`Scan_Allows*` iki test); (2) çağrı, yöntem grubu, `nameof` ve dizge biçimleri aynı düzenli ifadeyle yakalanır, ihlal örneği `ViolationSamples/TransitionUnit/WriterAndSetters.cs` (satır 31, 32); saat sahtesi ortak `ManualTimeProvider`; (3) göç/default değişmedi; (4) aynı sayıyı anan yer: backlog sayacına dokunulmadı, `09 §9.1` m.8 cümlesi D-525'te; (5) 09 metni değişmedi; birleşik ağaçta (main `02ec5357`) testler yeşil |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; biçim temiz |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --filter-not-trait "Consistency=true"`; `--filter-trait "Consistency=true"` | 4412/4412; Consistency 280/280 (`DefectClaimHistoryPayloadTests` 5 dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (yerel PostgreSQL 18, 55640) | 2658/2658 tam koşu (5 dk 48 sn); parçalarda da Model 1315, Web 594, Ordering 216 (`ItemRecordsTests` dahil), öteki 533 |
| Contract | uygulanamaz | — | Dış sözleşme yok |

Sayılar `origin/main` `02ec5357` (T342) birleştirmesinden sonraki ağaçtır.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T320-kalem-kayitlari`
- **Commit:** `5ddba78c` (tümleşik testler), `3f8f123b` (test düzenleri), `7fedd066` (backlog, STATUS), `e559b60e` (yük örneği testi); main birleştirmesi, saat sahtesi/ihlal örneği ve rapor/kayıt commit'leri bu dalda
- **PR:** #466
- **CI:** *(yönetici izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

Dış varsayım: yok (dış servis, paket, sürüm bilgisi kullanılmadı).

## ⚠ Kararlar (kolay, kurala en uygun seçenekle)

1. **Talep durumunun yazıcısı AfterSales'te (D-525, 🟡 `k-note`):** `09 §9.1` m.4 "kalem kaydının durum yazan yöntemini yalnız `OrderTransitions` çağırır" der; talep AfterSales'in varlığı olduğundan (`09 §6` m.2) `ApplyStatus`'un tek çağıranı `DefectClaimStatusWriter`, ona yalnız geçiş birimi ulaşır (tarama ikisini de tam yolla daraltır). Sıra geçiş birimindedir; metin hizası faz kapısında.
2. **Cayma yazıcısının tutarı, IBAN'ı ve penceresi çağırandan (D-522):** geçiş biriminde cayma işlemi T372'de; yazıcı bugün yalnız testten çağrılır.
3. **Teslim alma yazıcısında stok, B-14 ve kapanıştan sonra gelen mal yok (D-523, T373).**
4. **Ayıpta dönülen adet açık adetten düşmüyor (D-524, T374).**
5. **Sipariş düzeyi panel işaretleri sahiplerinde (D-526);** "e-posta düzeltildi"nin sahibi 11'de adıyla geçmiyor, faz kapısında netleşir.
6. **Yük biçimi geri alma uyumu:** önceki sürümün `DefectClaimHistoryEntry(At)` kaydı aynı `At` alanlı listeyi okur (bilinmeyen alanlar atlanır), göç gerekmez.
7. **Test düzenlerine Settings modülü eklendi** (`AddSettingsDependencies`): AfterSales iade adresini kayıt anında kopyalar (K-817).
8. **İşlem izi:** `RejectReturn` vb. iz satırı panel işlem katmanında; geçiş birimi bugün hiçbir işlemde iz yazmıyor (mevcut S-geçişleriyle aynı).
9. **`12` VAL-C107/C108 `BEKLEMEDE`:** kanıt türü uç yanıtı (sayfa task'ları); T320 modül düzeyini kurar.
10. **Ana dal CI:** chore #459'un run'ları 42468912436 cancelled, 42468912494 failure (Publish: yedek imajında restic indirmesi) — chore #463 düzeltti, run 42474266982 success; T342 run 42475025470 success, 42475025461 sürüyordu (başlangıçta).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Cayma işleminin geçiş birimi sarmalayıcısı, tutar ve IBAN | D-522 | Sahip T372 |
| 2 | Teslim almada stok, B-14, kapanıştan sonra gelen mal | D-523 | Sahip T373 |
| 3 | Ayıpta dönülen adedin açık adetten düşmesi | D-524 | Sahip T374 |
| 4 | `09 §9.1` m.4/m.8 metin hizası | D-525 | Karar kaydı notu, faz kapısı |
| 5 | Sipariş düzeyi panel işaretleri | D-526 | Sahipler T324, T370, T385, T387 |

## Notlar

- **Working tree (Adım -1):** temiz (dört işçide de).
- **Ana dal CI startup (Adım 0):** 4. işçi: 42475025470 success (T342), 42474266982 success (chore #463), 42474266979 cancelled (#463, eşzamanlılık), 42468912494 failure (#459, Publish — #463'te giderildi). Yönetici düzeni açık; durma kararı yöneticinin.
- **Repo memory (bitiş kapısı 9):** dokunulmadı (kayıt kuralı: MEMORY "Güncel Durum" faz kapısı kayıt PR'ının).
- **Diğer:** 1.–3. işçi bağlam sınırında bölündü (son head `7fedd066`); 4. işçi yük örneği testini yazdı, `origin/main`'i birleştirdi (`02ec5357`, rebase yok), tam derleme/biçim/test koşusunu, öz denetimi, raporu ve PR'ı yaptı.
