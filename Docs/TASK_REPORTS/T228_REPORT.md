# T228 — Eşzamanlılık denetimi

**Faz:** F2 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Tek işlem sınırı:** `Infrastructure/ITransactionRunner` + `TransactionRunner` (`09 §9.1` madde 3): ret olmayan sonuçta `SaveChangesAsync` + kesinleştirme; retçe ve istisnada `finally`'de geri alma ve izlenen değişikliklerin atılması (istisna olduğu gibi yukarı çıkar; türsüz `catch` yok — `09 §8.1` madde 6); açık işlem içinde ikinci çağrı istisna. Ret sonuç türlerinin ortak arayüzü `IOperationResult`.
- **Kilit sırası bekçisi:** `Infrastructure/LockSequence` (+ `LockKind`) — `05 §7.3.5`'in sırası işlem başına tutulur: sepet kilidi en başta ve yalnız, sonra siparişler (kimliğe göre), varyantlar (kimliğe göre), kupon en sonda; `CategoryTree`/`AdminSet` başka kilitle birleşmez. Kilit alan her yer (`AdvisoryLocks`, `OrderLocks`, `StockCounters`) deyimden önce bildirir; ters istek SQL gitmeden `InvalidOperationException`'dır. Anahtar sırası PostgreSQL `uuid` sırasıdır (`CompareKeys`, entegrasyon testiyle sınandı).
- **Danışma kilidi:** `Infrastructure/AdvisoryLocks` + `AdvisoryLockScope` (`Cart`, `CategoryTree`, `AdminSet`; `pg_advisory_xact_lock(hashtextextended(...))`).
- **Sipariş satır kilidi:** `Modules/Ordering/OrderLocks` (`SELECT … ORDER BY id FOR UPDATE`, kimlik sırasıyla) ve gösterilen açık adet karşılaştırması `ShownOpenQuantities` (kalem iptali ve cayma; K-1202).
- **Sayaçların tek yazıcısı:** `Modules/Catalog/StockCounters` — ayırma, serbest bırakma, kesin düşme, dönüş ve panelin stok yazımı birer koşullu `ExecuteUpdateAsync`; ayırma yalnız kalan adet yetiyorsa, panel yazımı gösterilen değer tutuyor ve yeni değer ayrılmış adedin altında değilse. Dışa açık yüzey `ICatalogModule` (`ReserveAsync`, `ReleaseAsync`, `CommitAsync`, `ReturnAsync`, `SaveStockQuantityAsync`), sonuç `StockQuantityWriteResult`.
- **Sürüm bekçileri (SaveChanges interceptor):** `Catalog/ProductVersionGuard` (ürünün form kayıtları ürün sürümünü artırmadan kaydedilemez; stok/ayrılmış/kupon sayaçları hiçbir `SaveChanges`'te yazılamaz), `Ordering/OrderVersionGuard` (sipariş kilidi altındaki her sipariş tarafı yazımı sürümü bir artırmalı; artırmayan kapalı liste `06 §2.6.4`). Ortak taban `Infrastructure/DataModel/VersionScope`; muaf kayıtlar `IOrderVersionExempt` işaretini taşır (6 kayıt: `InternalNote`, `DispatchRecord`, `TransitionRecord`, `AuditLog`, `Job`, `InboundNotificationRecord`).
- **Sürüm ayarlayıcıları:** kurumsal içeriğin 8 varlığı ve `LegalTextVersion` için `Version { get; private set; }` + `BumpVersion()` (`09 §10.2` madde 1, madde 6 testi).
- **Kaynak taramaları:** `ConcurrencySourceScan` (kilit metni yalnız `AdvisoryLocks.cs`/`OrderLocks.cs`'te, `FOR SHARE` yalnız `OrderLocks.cs`'te, sayaç yazımı yalnız `StockCounters.cs`'te; izinli yerler tam yol ve yalnız var olan dosya) + ihlal örneği `ViolationSamples/Concurrency/LockAndCounterWrites.cs`; `TransactionBodySourceScan.WorkNotALambda` (D-82: `RunAsync`'e yöntem grubu/temsilci değişkeni verilmesi bulgu) + ihlal örneği satır 25, 27.
- **Dokümanlar:** `09 §10.2` madde 5 (kilit sırasını `LockSequence` tutar) ve madde 6 (sayaç taraması, izinli yer kuralı), `09 §10.7` madde 5 (bütün `src/`, lambda kuralı), `09 §25` eşzamanlılık sorusu; `CONTEXT.md` klasör haritası; `DEFERRED_BACKLOG` D-82 ✅, D-94 açıldı, aktif kalem sayacı origin/main birleşiminden sonra gerçek satır sayımıyla 55.
- **Bu turun düzeltmeleri (2. devam):** yasak test kalıbı taraması üç bekleme testini iddiasız buluyordu (iddia yardımcıdaydı) → iddialar test gövdesine alındı; taramada var olmayan izinli yol (`CompanySettingsWriter.cs`) kaldırıldı; `TransactionRunner`'ın türsüz `catch`'i `finally`'ye çevrildi; eklenen koddaki null-forgiving `!` kullanımları kaldırıldı (`VersionScope.NameOf`, `OfType<Guid>`, `GetValueOrDefault`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/`: `ITransactionRunner.cs`, `TransactionRunner.cs`, `IOperationResult.cs`, `LockSequence.cs`, `LockKind.cs`, `AdvisoryLocks.cs`, `AdvisoryLockScope.cs`, `DataModel/VersionScope.cs`, `DataModel/IOrderVersionExempt.cs`, `AppDbContext.cs` (iki interceptor), `Jobs/Job.cs` (işaret)
- `Modules/Catalog/`: `StockCounters.cs`, `CounterMove.cs`, `ProductVersionGuard.cs`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs`, `Contracts/StockClaim.cs`, `Contracts/StockQuantityWriteResult.cs`
- `Modules/Ordering/`: `OrderLocks.cs`, `OrderVersionGuard.cs`, `ShownOpenQuantities.cs`, `OrderingModule.cs`, `InternalNote.cs`, `TransitionRecord.cs` (işaret)
- `Modules/CorporateContent/` 8 varlık, `Modules/Settings/LegalTextVersion.cs`, `Modules/Administration/AuditLog.cs`, `Modules/Notifications/DispatchRecord.cs`, `Modules/Payment/InboundNotificationRecord.cs`, `Program.cs`
- Testler: `tests/Shopfolio.UnitTests/Concurrency/` (3 dosya), `Architecture/ConcurrencySourceScan(.cs, Tests.cs)`, `Architecture/TransactionBodySourceScan.cs`, `Architecture/DatabaseSessionSourceScanTests.cs`, `ViolationSamples/Concurrency/`, `ViolationSamples/TransactionBody/`; `tests/Shopfolio.IntegrationTests/Concurrency/ConcurrencyTests.cs`
- Doküman: `09`, `DEFERRED_BACKLOG`, `IMPLEMENTATION_STATUS`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`, T205/T230/T231/T232 raporlarının "Post-merge run'lar" satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sürüm; yazma yalnız sürüm aynıysa, tek koşullu güncelleme | ✓ | `ConcurrencyTests.RecordSave_IsOneConditionalUpdate_…_KK1` (tek `UPDATE products SET … WHERE … version = @…`), `RecordUpdate_WithAStaleVersion_AffectsNoRow_KK1`; `VersionRulesTests.BumpVersion_OnEveryRecordWithOneVersion_RaisesItByOne` |
| 2 | Kayıt varyantında kilit yok; ürün sürümü varyantları kapsar; stok/kontenjan ve sistem hareketleri artırmaz | ✓ | `RecordSave_TakesNoLock_…_KK2`, `VariantChange_RaisesTheProductsVersion_AndWithoutTheRaiseIsRefused_KK2`, `ReserveCommitReturnAndThePanelsStockWrite_DoNotRaiseTheProductsOrTheCouponsVersion_KK2`, `Save_ThatWritesTheStockField_IsRefused_…`; `ProductVersionGuard.cs:67-86` |
| 3 | Sipariş sürümü bütün sipariş tarafında artar; artırmayanlar kapalı liste | ✓ | `ItemRecordUnderTheOrderLock_…_KK3`, `OrderChangeUnderTheLock_WithoutTheRaise_IsRefused_KK3`, `InternalNoteUnderTheOrderLock_DoesNotRaiseTheVersion_KK3`; `VersionRulesTests.OrderVersionExemptRecords_AreTheClosedListOf0624`, `OrderVersionScope_IsTheOrderSide_AndHoldsNoExemptRecord`; `OrderVersionGuard.cs:27-42` |
| 4 | Sipariş satır kilidi; eski sürümlü yönetici işlemi uygulanmaz; müşteri/sistem o anki hâle; kalem iptali ve cayma gösterilen açık adet | ✓ | `OrderLock_TheSecondOrderOperationWaitsForTheFirst_KK4`, `AdministratorOperation_ThatSawAnOlderVersion_IsNotApplied_…_KK4`, `ItemCancellation_WhoseShownOpenQuantityChanged_IsNotApplied_KK4`; `ShownOpenQuantitiesTests` (5 test); `OrderLocks.cs:24-44` |
| 5 | Ayırma koşullu güncelleme | ✓ | `LastUnit_TwoConcurrentReservations_OnlyOneGetsIt_KK5`, `Reservation_ThatDoesNotFit_WritesNothing_KK5`; `StockCounters.cs:118-120` |
| 6 | Panel yazımı ayrılmış adedin altına inmez | ✓ | `PanelStockWrite_NeverGoesBelowTheReservedQuantity_AndRefusesAChangedShownValue_KK6`; `StockCounters.cs:84-98` |
| 7 | Kilit sırası: siparişler, varyantlar, kupon | ✓ | `LockSequenceTests` (8 test), `ReversedLockOrder_IsCaughtBeforeTheStatement_KK7`, `OneTransaction_ReservingTwice_AsksAVariantLockAfterTheCouponLock_AndIsCaught_KK7`, `PostgresUuidOrder_IsTheOrderOfCompareKeys`; `LockSequence.cs:46-76` |
| 8 | Onay: önce sepet danışma kilidi, ayırmadan önce önceki siparişin satır kilidi | ✓ | `ConfirmationOrder_CartThenEarlierOrderThenItsFreeingAndTheNewReservation_IsAllowed_KK8`, `SameCart_TheSecondConfirmationWaitsForTheFirst_KK8`; `LockSequenceTests.Check_TheConfirmationsOrder_…`, `Check_CartAfterAnyOtherLockOrASecondCart_Throws` |
| 9 | Dış çağrı kilit içinde beklenmez | ✓ | `DatabaseSessionSourceScanTests.ApplicationSource_UsesNoOutsideServiceInsideATransactionBody`, `TransactionBodyScan_OnViolationSample_…EachWorkNotALambda…` (D-82); `ConcurrencySourceScanTests` (kilit metni yalnız iki dosyada) |

### Öz denetim (bitiş kapısı 2)

> Bu tur (2. devam) bütün maddeleri yeniden denetledi (`grep -n` + satır aralığı), yalnız kalanları değil.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1 | ✓ | `ConcurrencyTests.cs:163` (tek `UPDATE`, sürüm koşulu `WHERE`'de), `:190`; `VersionRulesTests.cs:60` |
| Kriter 2 | ✓ | `ConcurrencyTests.cs:203`, `:224`, `:252`, `:294`; `ProductVersionGuard.cs:22-39` (form kayıtları, koşullu alanlar), `:67-86` |
| Kriter 3 | ✓ | `ConcurrencyTests.cs:312`, `:342`, `:359`; `VersionRulesTests.cs:95`, `:104`, `:112`; `OrderVersionGuard.cs:27-51` ↔ `06 §2.6.4` "Artırmayanlar" birebir |
| Kriter 4 | ✓ | `ConcurrencyTests.cs:382`, `:394`, `:425`; `ShownOpenQuantitiesTests.cs:15-45`; `OrderLocks.cs:24-44` |
| Kriter 5 | ✓ | `ConcurrencyTests.cs:441`, `:465`; `StockCounters.cs:118-120` (koşul ve artış aynı deyimde) |
| Kriter 6 | ✓ | `ConcurrencyTests.cs:512`; `StockCounters.cs:84-98` |
| Kriter 7 | ✓ | `LockSequenceTests.cs:47-123`; `ConcurrencyTests.cs:493`, `:535`, `:644`; `LockSequence.cs:46-76` |
| Kriter 8 | ✓ | `ConcurrencyTests.cs:585`, `:607`; `LockSequenceTests.cs:78`, `:95` |
| Kriter 9 | ✓ | `TransactionBodySourceScan.cs` (+ `WorkNotALambda`), `DatabaseSessionSourceScanTests.cs:55`, `:66`; `ConcurrencySourceScanTests.cs:12` |
| KK1 eski sürümle güncelleme sıfır satır | ✓ | `RecordUpdate_WithAStaleVersion_AffectsNoRow_KK1` (`ConcurrencyTests.cs:190`), `RecordSave_IsOneConditionalUpdate_…_KK1` (`:163`) |
| KK2 kayıt kilit almaz; varyant ürün sürümünü artırır, ayırma/dönüş artırmaz | ✓ | `ConcurrencyTests.cs:203`, `:224`, `:252` |
| KK3 alt kayıt değişince sürüm artar | ✓ | `ConcurrencyTests.cs:312`, `:342`, `:359` (muaf iç not artırmaz) |
| KK4 iki sipariş işleminden biri bekler; eski sürüm ve değişmiş açık adet uygulanmaz | ✓ | `ConcurrencyTests.cs:382`, `:394`, `:425` |
| KK5 son adet yalnız birine | ✓ | `ConcurrencyTests.cs:441` |
| KK6 panel stoğu ayrılanın altına inmez | ✓ | `ConcurrencyTests.cs:512` |
| KK7 ters sıralı kilit testte yakalanır | ✓ | `ConcurrencyTests.cs:493`, `:535`; `LockSequenceTests.cs:56`, `:67`, `:95` |
| KK8 aynı sepetin iki onayı seri | ✓ | `ConcurrencyTests.cs:607` |
| KK9 kilit altında dış çağrı yok | ✓ | `TransactionBodySourceScan` + `ConcurrencySourceScan` (ihlal örnekleri ayrı: `ViolationSamples/TransactionBody/OutsideCallInTransaction.cs`, `ViolationSamples/Concurrency/LockAndCounterWrites.cs`) |
| `09 §25` Doküman uyumu | ✓ | adlar `06`/`09`'dan (`ITransactionRunner`, `AdvisoryLocks`, `OrderLocks`, `BumpVersion`); çelişki yok; yeni ayar/sabit yok |
| `09 §25` Mimari ve kod | ✓ | klasör düzeni değişti → `CONTEXT.md` güncel; modül sınırı: başka modülün türü yalnız işaret arayüzüyle (`IOrderVersionExempt`; `DataModelSourceScan` yeşil); `!`, türsüz `catch`, `async void`, `dynamic` yok (diff taraması); yeni paket yok |
| `09 §25` Durum/işlem izi — eşzamanlılık | ✓ | §10.2 maddeleri 1–6; kilit `LockSequence`'e bildiriliyor (`AdvisoryLocks.cs:33`, `OrderLocks.cs:40`, `StockCounters.cs:49,64,84`); iş gövdesi lambda (D-82 taraması) |
| `09 §25` Veri ve göç | uygulanamaz | şema değişmedi (yalnız ayarlayıcı görünürlüğü); göç yok |
| `09 §25` Güvenlik | ✓ | sır/kişisel veri yok; yeni uç, form, yetki yüzeyi yok; SQL parametreli (`ExecuteSqlAsync`/`SqlQuery` interpolasyonu parametreye çevirir) |
| `09 §25` Test | ✓ | §10.2 madde 6'nın Test listesi: birim + gerçek PostgreSQL; her yeni tarama kuralının ayrı ihlal örneği var (`ConcurrencySourceScanTests.Scan_OnViolationSample_…`, `Scan_AFileOfTheSameNameElsewhere_IsNotAnAllowedPlace`); yasak test kalıbı taraması yeşil |
| `09 §25` Performans | ✓ | kilit ve sayaç deyimleri tek satır/tek deyim; döngüde sorgu yalnız kilit sırası gereği (kimlik başına bir koşullu güncelleme) |
| Güvenlik taraması | ✓ | secret yok (pre-commit bekçisi geçti); auth etkisi yok; girdi doğrulama: `StockCounters.ApplyAsync` adet > 0 ve tekil varyant, `SaveStockQuantityAsync` negatif değer reddi; yeni dış bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Script | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 dosya (190 iddia) |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... -c Release` | 859/859 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... -c Release` (bölümlü) | Concurrency 26/26, Database 35/35, Web+FileSecrets+Logging 102/102, Model 893/893 (`--max-threads 4`) — toplam 1056 |
| Contract | uygulanamaz | — | `CONTRACT_TEST_CMD=NOT_APPLICABLE` |
| Build / Lint | ✓ | `dotnet build -c Release -warnaserror`; `dotnet format --verify-no-changes` | 0 uyarı, biçim temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulama chat'i doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok — yalnız ayarlayıcı görünürlüğü ve `BumpVersion()`; model anlık görüntüsü değişmedi
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T228-eszamanlilik-denetimi`
- **Commit:** `1685a7b` — origin/main (T225) birleştirmesi; T228 backlog kaydı D-91 → D-94 (son kod commit'i `7045b9c`; bu satırı düzelten rapor commit'i ayrı)
- **PR:** #391
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | PostgreSQL `uuid` sırası küçük harfli metin sırasıdır (kilit anahtar sırası) | `ConcurrencyTests.PostgresUuidOrder_IsTheOrderOfCompareKeys` — 66 anahtar `ORDER BY` ile karşılaştırıldı (PG18 yerel konteyner) | ✓ |
| 2 | `pg_advisory_xact_lock` ve `FOR UPDATE` beklemesi gözlenebilir (`pg_stat_activity` `wait_event_type = 'Lock'`) | bekleme testleri (`…WaitsForTheFirst…`) PG18'de | ✓ |

Başka dış varsayım yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İndirme sayacı taramada yok; ayarın satır kilidinin yolu taramaya yazıcısıyla girecek | D-94 | Yazıcılar henüz yok (T340; `CompanySettings` yazıcısı); var olmayan dosya izinli yol olamaz |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; dal `origin/task/T228-eszamanlilik-denetimi` head `f2ee24a`'dan sürdürüldü)
- **Ana dal CI startup (Adım 0):** main `daace97` — CI 41960563145 success, Publish 41960563164 success; önceki run'lar 41958746129, 41958746107, 41953520461 success
- **Repo memory (bitiş kapısı 9):** güncellendi (Güncel Durum'da T228 satırı)
- **Önceki task kaydı (skill adım 15):** status Commit hücreleri T205 `a36010b`, T230 `a2da89f`, T231 `dac48c8`, T232 `daace97`; dört raporun "Post-merge run'lar" satırı dolduruldu
- **Yerel entegrasyon notu:** Model ad alanının tamamı yerel Docker'da varsayılan paralellikte koleksiyon fikstürlerinde bağlantı zaman aşımı verebiliyor (`Npgsql … The operation has timed out`, sınıf her koşuda farklı; tek başına ve `--max-threads 4` ile 893/893). Mantık hatası değil, yerel yük; CI tek koşuda sınar.
- **⚠ Kararlar (kurala en uygun seçenek; devir notlarından ve bu turdan):**
  1. ⚠ `ITransactionRunner` bu task'ta kuruldu (kriter 9 ve kilitlerin işlem kapsamı onu gerektiriyor; `09 §9.1` madde 3 kalıbı, K-1335).
  2. ⚠ `ErrorCode` henüz yok (T213): stok yazımının sonucu `StockQuantityWriteResult` (`Written`, `ShownValueChanged`, `BelowReserved`); kayıt çakışması testte `DbUpdateConcurrencyException` olarak sınandı — `RecordVersionConflict`/`DisplayedValueConflict` eşlemesi T213'ün işi.
  3. ⚠ `FOR SHARE` (`09 §10.9`, iç not kilidi) kapsam dışı; tarama `FOR SHARE`'i `09 §10.2` madde 6 gereği yalnız `OrderLocks.cs`'te (var olan dosya) kabul eder.
  4. ⚠ `CompanySettings` kilidi `LockKind`'da yok ve tarama ayar yazıcısının yolunu izinli saymaz (önceki işçi `CompanySettingsWriter.cs`'i, dosya yokken izinli yol yazmıştı — bu turda kaldırıldı; sık FAIL "var olmayan izinli yol"); yazıcıyı kuran task ekler (D-94 (2)).
  5. ⚠ Sipariş muafiyeti dizgi listesi yerine işaret arayüzü `IOrderVersionExempt` (D-61: modül başka modülün türünü dizgiyle anmaz).
  6. ⚠ Kurumsal içerik ve `LegalTextVersion` ayarlayıcıları `private` + `BumpVersion()` — kapsam gerekçesi `09 §10.2` madde 6'nın "sürüm ayarlayıcıları `private`" testi.
  7. ⚠ `DownloadCount` taramanın sayaç listesinde yok (yazıcısı `Ordering`'in indirme task'ı, T340) → D-94 (1).
  8. ⚠ `OrderVersionGuard` yalnız sipariş kilidi tutulan işlemde bakar (kilitsiz kayıt — tohum, kaydın kendi koşullu güncellemesi — bakılmaz; her sipariş işlemi kilidi alır, `05 §7.3.2`).
  9. ⚠ `09 §10.2` madde 5 değişti: önceki metin "bu sınıf onu zorlamaz, test sınar" diyordu; KK7'yi kod yolunda yakalamanın tek yolu çalışma anı denetimi (kaynak taraması çağrılar arası sırayı göremez) → `LockSequence` sırayı tutar, ters istek deyimden önce istisnadır. `09 §25` eşzamanlılık sorusu aynı PR'da güncellendi.
  10. ⚠ Kriter 4 ve 8'in "müşterinin işlemi", "sistemin geçişi" ve "sipariş onayı" gerçek işlemleri sonraki task'larda; bu task kalıbı ve bekçileri kurar, testler `09 §10.2` madde 3'ün kalıbıyla yazılmış vekil işlemlerle (`AdministratorOperationAsync`, `CustomerItemCancellationAsync`) koşar.
  11. ⚠ `TransactionRunner` geri almayı `finally`'de yapar (türsüz `catch { …; throw; }` `09 §8.1` madde 6'ya aykırıydı); kesinleştirme `finally`'de değil (`09 §8.1` madde 5).
  12. ⚠ `DEFERRED_BACKLOG` başlık sayacı 51 → 55 (origin/main birleşiminden sonra gerçek aktif satır sayımı; D-82 kapandı, D-94 açıldı; T225 D-91–D-93).
