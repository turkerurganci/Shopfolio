# T301 — Siparişin imha parçası

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Modules/Ordering/OrderDisposalPart.cs`: gecelik imhanın "Orders" parçası (`OrderingModule` kaydında `AddDisposalPart<OrderDisposalPart>()`). Her parça 500 siparişlik bir veritabanı işlemidir (`DisposalRun.ChunksAsync`): seçim, satır kilidi (`OrderLocks`, kimlik sırasıyla tek deyim), kilitten sonra siparişlerin ve koşulun yeniden okunması, boşaltma, sürüm artışı, `Order` türüyle tek imha satırı; silecek kaydı olmayan parça satır yazmaz.
- `Modules/Ordering/OrderRetention.cs`: iki yasal süre (Z-28: oluşturulma yılının sonundan on yıl; Z-41: Başarısız anından üç yıl), Türkiye saatinde sayılır, sınırlar UTC anı olarak döner (veritabanı parametresi).
- Kol, siparişin düzeltilmemiş ödeme kaydından okunur, saklanmaz: ödeme kaydı olan ve onu onaylayan Ö1'i bir düzeltme satırınca düzeltilmemiş (ya da Ö1'siz geç kart ödemesi) sipariş Z-28'dedir; yoksa Başarısız + Ö2 anı Z-41'dir. Seçim: SQL ön süzgeç + `JudgeAsync` kesin yargı; kilitten sonra yeniden yargı; kesin yargının reddettiği taşan sayfa atlanır, parçayı tutmaz.
- `Order.ErasePersonalData()`: iletişim (normalize e-posta dahil), erişim anahtarı, adres, hesap ve sepet bağı, IP birlikte boşalır; onay anahtarı ve ticari alanlar kalır.
- Modül yüzeyleri: `IPaymentModule.FindPaymentSourcesAsync` / `ErasePersonalDataAsync` (`PaymentSource`), `IAfterSalesModule.ErasePersonalDataAsync(OrderPersonalDataScope)`; gerçeklemeler `PaymentModule`, `AfterSalesModule` izlenen varlıkla yazar (toplu yazım yok; `VersionedSideBulkWriteScan`).
- Bağlı kayıtlar: `TransitionRecord` yönetici kimliği/adı/not boşalır, iç not + e-posta geçmişi + adres geçmişi silinir, kalemin dosya bağı boşalır, IBAN istekleri ve duran IBAN silinir, `Refund`'da yalnız yönetici alanları boşalır.
- Testler: `OrderDisposalTests` (27 Integration), `OrderRetentionTests` (5 Unit), `DisposalNumberSourceScan` kapsamına parça eklendi (+ test).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/` (`OrderDisposalPart`, `OrderRetention`, `Order`, `OrderingModule`)
- `src/Shopfolio.Web/Modules/Payment/` (`PaymentModule`, sözleşme `IPaymentModule`, `PaymentSource`)
- `src/Shopfolio.Web/Modules/AfterSales/` (`AfterSalesModule`, sözleşme `IAfterSalesModule`, `OrderPersonalDataScope`)
- `tests/Shopfolio.IntegrationTests/Disposal/` (`OrderDisposalTests`, `DisposalHarness`)
- `tests/Shopfolio.UnitTests/Ordering/OrderRetentionTests.cs`, `tests/Shopfolio.UnitTests/Architecture/DisposalNumberSourceScan.cs` (+ `DisposalNumberScanTests`)
- Belgeler: `IMPLEMENTATION_STATUS.md`, `MEMORY.md`, `DEFERRED_BACKLOG.md` (D-99 T301 payı), T308 / T312 / T331 raporları (Post-merge run'lar).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek işlem; ödemesi alınmış Z-28, hiç alınmamış Z-41; kol düzeltilmemiş ödeme kaydından | ✓ | `OrderDisposalPart.RunAsync` → `ChunksAsync`; `JudgeAsync`; testler `PaidOrder_PastZ28_IsDisposed_AndTheOneInsideItIsKept`, `UnpaidFailedOrder_IsDisposed_ThreeYearsAfterItFailed_NotBefore`, `FailedOrderWithALatePayment_IsNotDisposedAtZ41_ItWaitsForZ28`, `PaymentMarkThatWasCorrected_MakesTheOrderAnUnpaidOne` |
| 2 | Satır kilidi, kilitten sonra yeniden okuma, sürüm artışı | ✓ | `DisposeChunkAsync` (`_locks.LockAsync`, ikinci `JudgeAsync`, `BumpVersion`); `Disposal_RaisesTheVersionByOne_AndAPanelOperationThatSawTheOldVersionIsRefused` |
| 3 | Yalnız `ContactEmail` dolu sipariş seçilir | ✓ | `PickDueAsync` / `JudgeAsync` `o.ContactEmail != null`; `DisposedOrder_IsNotPickedASecondTime_AndItsVersionRisesOnce` |
| 4 | Kişisel alanlar birlikte boşalır; onay anahtarı ve ticari alanlar kalır | ✓ | `Order.ErasePersonalData`; `OrderFields_EmptyTogether_TheAccessKeyIsEmptyAndTheConfirmationKeyStays`, `OrderCommercialFields_StayTheSame`, `TheDatabaseCheck_RefusesEmptyingTheContactAlone` |
| 5 | Yönetici kimliği/adı, serbest not, IBAN bağı, kalemin dosya bağı boşalır | ✓ | `TransitionRecord.ErasePersonalData`, `item.DigitalFileId = null`, `IPaymentModule/IAfterSalesModule.ErasePersonalDataAsync`; `LinkedRecords_AreEmptiedDeletedOrLeftAsTheListSays`, `ItemRecords_KeepTheirAmountsDatesAndLinksToTheirRefunds` |
| 6 | İç not, e-posta ve adres geçmişi, IBAN istekleri, duran IBAN silinir | ✓ | `db.InternalNotes / OrderEmailHistories / OrderAddressHistories .RemoveRange`; IBAN isteği ve duran IBAN `AfterSalesModule`; `LinkedRecords_AreEmptiedDeletedOrLeftAsTheListSays` |
| 7 | Ödeme kaydı, `RefundRecord`, onay kutusu, yasal metin, gönderim kayıtlarına dokunulmaz; `Refund`'da yalnız yönetici alanları | ✓ | Parça bu tabloları yazmaz; `LinkedRecords_AreEmptiedDeletedOrLeftAsTheListSays`, `ItemRecords_KeepTheirAmountsDatesAndLinksToTheirRefunds` |
| 8 | Ödemesiz sipariş satırı kalır, kişisel verisiz | ✓ | `UnpaidOrder_AfterZ41_KeepsItsRowWithoutPersonalData_AndDeletesNothing`, `UnpaidOrder_PastTenYears_IsStillThereWithoutPersonalData` |
| 9 | İmha kaydında `Order` türü ve sayı; silecek yoksa satır yok | ✓ | `DisposalRow_HasTheOrderTypeAndTheNumber_AndTheNoteAndHistoryDeletionsWriteNoRowOfTheirOwn`, `PartWithNothingToDispose_WritesNoRow`, `MoreOrdersThanOnePart_AreDisposedInChunks_EachWithItsOwnRow` |

> `? Doğrulanamadı` **FAIL değildir** — kanıt eksikliğidir. FAIL'den ayrı tutulur.

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1–k9 | ✓ | Yukarıdaki tablo |
| KK1: iki süre ayrı anda; geç ödemeli geri ödenmiş Başarısız Z-41'de imha edilmez, Z-28'i bekler; parça tek işlem | ✓ | `PaidOrder_IsDisposedAtTheTurnOfTheCalendarYearInTurkeyTime_NotBefore`, `UnpaidFailedOrder_IsDisposed_ThreeYearsAfterItFailed_NotBefore`, `FailedOrderWithALatePayment_IsNotDisposedAtZ41_ItWaitsForZ28`, `FailedOrderWithALatePayment_IsDisposedOnceZ28IsOver`, `TheTwoPeriods_RunInOneChunk_EachByItsOwnClock`, `APartThatFails_UndoesTheWholeChunk_AndTheNextRunDoesIt`; birim: `OrderRetentionTests` (sınırlar) |
| KK2: yarışan panel işlemi sürüm uyarısı alır | ✓ | `Disposal_RaisesTheVersionByOne_AndAPanelOperationThatSawTheOldVersionIsRefused` |
| KK3: imha edilmiş sipariş ikinci kez seçilmez; seçimle kilit arasında imha edilen atlanır ve sayılmaz | ✓ | `DisposedOrder_IsNotPickedASecondTime_AndItsVersionRisesOnce`, `OrderDisposedBetweenThePickAndTheLock_IsSkipped_AndNotCounted`, `OrderCorrectedBetweenThePickAndTheLock_IsJudgedAgain_AndSkipped` (`AfterPick` dikişi) |
| KK4: kontrol kısıtı birlikte boşalmayı korur; erişim anahtarı boş, onay anahtarı dolu | ✓ | `TheDatabaseCheck_RefusesEmptyingTheContactAlone`, `OrderFields_EmptyTogether_...` |
| KK5: yönetici adı, not, kalemin dosya bağı boşalır | ✓ | `LinkedRecords_AreEmptiedDeletedOrLeftAsTheListSays`, `ItemRecords_...` |
| KK6: silinen bağlı kayıt sorguda yok | ✓ | `LinkedRecords_AreEmptiedDeletedOrLeftAsTheListSays` |
| KK7: ödeme, `RefundRecord`, gönderim satırları aynı; `Refund`'da yalnız yönetici alanları boş | ✓ | `LinkedRecords_...`, `ItemRecords_...` |
| KK8: ödemesiz sipariş satırı kalır, kişisel alan yok | ✓ | `UnpaidOrder_AfterZ41_...`, `UnpaidOrder_PastTenYears_...` |
| KK9: imha kaydında tür ve sayı | ✓ | `DisposalRow_HasTheOrderTypeAndTheNumber_...` |
| `09 §25` ilgili satırlar | ✓ | Tarama kapsamı: `DisposalNumberSourceScan.AppliesTo` parçayı sayı yazımına karşı tarar (`OrderRetention` süre sahibidir, kapsam dışı; ayrı `AppliesTo` testi); `VersionedSideBulkWriteScan` ve `ModuleBoundary` taramaları geçer (Architecture 830/830); modül yüzeyi yöntemleri kendi işlemini açmaz (`ThePaymentSurface_RefusesToEmptyOutsideATransaction`, `TheAfterSalesSurface_RefusesToEmptyOutsideATransaction`) |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | İstisna yok; kapsama bir dosya eklendi, yalnız `OrderRetention.cs` (süre sahibi) dışarıda |
| Sık FAIL 2: kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama yok; var olanlar parçaya uygulanır |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ | Yeni tablo ve sütun yok (göç yok) |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `DisposalNumberSourceScan` özet metni "iki parça" → "üç parça"; D-99 T301 payı; STATUS satırı, MEMORY satırı |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09` ve öteki belgeler değişmedi; yeni belge kuralı yok |
| Kayıt belgesi: hedefi bu task olan backlog kaydı | ✓ | D-99 T301 payı Çözüldü olarak işlendi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 3469 geçti, 0 kaldı; Architecture 830 |
| Integration | ✓ | `--filter-class "*OrderDisposalTests"`; `--filter-class "*.DisposalTests" --filter-class "*ModuleCallTransactionTests"` | 27 + 21 geçti |
| Format / derleme | ✓ | `dotnet format <proje> --verify-no-changes`; `dotnet build -warnaserror` (Web, Unit, Integration) | temiz, 0 uyarı |
| Contract | — | — | Bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — (doğrulama chat'i doldurur) |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T301-siparis-imha`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** açıldığında PR gövdesinde
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; ayrı `chore:` PR'ında veya sonraki task dalında)*

## Dış Varsayımlar

Dış varsayım: yok (PostgreSQL 18 kapsayıcısı yerel test ortamıdır; plan/kota/sürüm varsayımı yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Zamanlayıcı kaynakları ve imha adımlarının geri kalanı (T302 parçaları vb.) | D-99 | Kendi task'larında; T301 payı kapandı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: CI 42368786665 success, Publish 42368786653 success (`ad8c48d`); Publish 42366197659 success (`1112ca6`)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Tasarım kararları:** kol ayrık uçta okunur, saklanmaz; seçim SQL ön süzgeç + kesin yargı, kilit sonrası yeniden yargı; parça 500 sipariş/işlem; diğer modüllerin kayıtları kendi yüzeylerinden izlenen varlıkla (toplu yazım yasağı D-95); sınır UTC anı (Npgsql `timestamptz` yalnız offset 0 kabul eder; ilk koşuda +03:00 sınırı 23 testi düşürdü, `OrderRetention` UTC döndürür).
- **Önceki task'ların merge kaydı:** T308 (`ad8c48d`), T312 (`1112ca6`), T331 (`eb91975`) raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri dolduruldu.
- **Dış varsayım:** yok. **Yeni backlog kaydı:** yok.
