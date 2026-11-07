# T315 — Durum makinesi çekirdeği: envanter, geçiş tablosu ve kimlikleri, durum makinesi olmayan varlıkların sınırı

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-07

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Sipariş durum makinesinin tek geçiş birimi `OrderTransitions` (Sepet ve sipariş modülü): satır kilidi → koşulun siparişin o anki hâline karşı denetimi → durum alanlarına yazım → geçiş kaydı; kapanış değerlendirmesi aynı işlemin sonunda (`EvaluateClosureAsync`). Birim kendi işlemini açmaz, çağıranın işleminde çalışır.
- Tek geçiş tablosu `TransitionTable`: S1…S11 sevkiyat ve Ö1…Ö5 ödeme satırları, her satırın tetikleyen aktör kümesi (`ActorsOf`), iki eksenin çift tablosu (`IsPairAllowed`, veritabanının `status_pair_allowed` kuralıyla aynı). Yöneticinin düzeltmesi (`CorrectAsync`) tablonun dışında, aynı birimin adlı işlemidir ve yalnız yönetici aktörüyle çalışır.
- Durum alanlarının tek yazıcısı: `Order.OrderStatus` / `PaymentStatus` özel setter; yazım `Order.ApplyTransition` ve `ApplyCorrection` ile. `DefectClaim.DefectClaimStatus` özel setter, tek yazım `ApplyStatus` (çağıranı T320'dedir).
- `IAfterSalesModule.ClosedQuantitiesAsync`: kalemin kayıtlarıyla kapanan adedi (iptal, çıkarma, geç teslim sonlandırma, cayma; çağıranın bağlamındaki eklenmiş satırlar dahil).
- `TransitionRecord` yalnız eklenir (güncelleme reddedilir); kalem kaydı geçiş kaydına satır yazmaz.
- Beş enum (`OrderStatus`, `PaymentStatus`, `PublishStatus`, `DefectClaimStatus`, `ContactRequestStatus`) `02 §5` ile eşitlik testinde; içerik kaydı `Archived` almaz.
- Mimari tarama `TransitionUnitSourceScan`: durum alanı yazımı yalnız birim dosyasında; toplu/ham yazımlar (`ExecuteUpdate`, ham SQL) ve birimde HTTP türleri bulgudur; ihlal örnekleri `ViolationSamples/TransitionUnit/`.
- D-72 kapatıldı: `ModuleSourceScan.EventPublisherName` `09 §9.1` madde 7'ye eşitlendi.
- Lint öncesi yerel taramada bulunan düzeltme: `OrderTransitions` içinde `List<object>` yerine adlı çift listesi (üçüncü taraf içerik taraması `<object` kalıbını yakaladı).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/`: `OrderTransitions.cs`, `TransitionTable.cs`, `TransitionRequests.cs`, `Order.cs`, `OrderingModule.cs`, `Contracts/IOrderTransitions.cs`
- `src/Shopfolio.Web/Modules/AfterSales/`: `AfterSalesModule.cs`, `Contracts/IAfterSalesModule.cs`, `DefectClaim.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Ordering/OrderTransitionsTests.cs`; `tests/Shopfolio.UnitTests/{Consistency/TransitionTableConsistencyTests, Consistency/StatelessEntitiesConsistencyTests, Ordering/TransitionTableTests, Architecture/TransitionUnitSourceScan(+Tests), Architecture/ModuleSourceScan(+Tests), Architecture/ViolationSamples/*}`
- Kayıt: `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`, `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Beş enum, altı durum seti | ✓ | `StatusEnums_EqualTheStateTablesOfTheRequirementsDocument`, `ContentRecords_TakeTwoOfTheThreePublishValues_NeverArchived` |
| 2 | Tek geçiş birimi, durum alanına yalnız o yazar | ✓ | `Order.cs` özel setter + `ApplyTransition`; `ApplicationSource_WritesTheStatusFieldsOnlyInTheTransitionUnit` |
| 3 | Tek tablo, kimlikli; tabloda olmayan reddedilir; düzeltme tablo dışı, yalnız panel | ✓ | `Apply_ATransitionOutsideTheTable_IsRefused_AndWritesNothing_KK3`, `Correction_FromTheBuyerOrTheSystem_IsRefused_AndOnlyTheAdministratorsCallWorks_KK3` |
| 4 | Geçişi adıyla ister; her satırda aktör türü | ✓ | `EveryRowOfTheTable_HasAtLeastOneActorKind_KK4`, `Apply_ByAKindOfActorTheTableDoesNotName_IsRefused_AndWritesNothing_KK4` |
| 5 | Sıra: kilit, koşul, yazım, kayıt | ✓ | `OrderTransitions.cs` `ApplyAsync` (LockAsync → TryPlan → ApplyTransition → TransitionRecord.Add); `Race_*_KK5` |
| 6 | Kayıt yalnız eklenir; kalem kaydı geçiş satırı yazmaz | ✓ | `TransitionRecord_CannotBeChanged_ButItsAdministratorLinkMayBeEmptied_KK6`, `AnItemRecord_WritesNoTransitionRecordRow_ByItself_KK6` |
| 7 | Açık kalemi kalmayan siparişin kapanışı aynı işlemin sonunda | ✓ | `Closure_WhenTheLastOpenItemIsCancelled_CancelsTheOrderInTheSameOperation_WithOneSystemRecord_KK7` |
| 8 | Kalem, indirim, kupon, stok ayırma, hesap, oturum, davet, satış açıklığı durum makinesi taşımaz | ✓ | `TheEntitiesWithoutAStateMachine_HaveNoStatusProperty_KK8` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1 beş enum, içerik Archived almaz | ✓ | `Consistency/TransitionTableConsistencyTests.cs`: `StatusEnums_EqualTheStateTablesOfTheRequirementsDocument`, `ContentRecords_TakeTwoOfTheThreePublishValues_NeverArchived` |
| k2 tek birim, yazım yalnız birimde | ✓ | `Order.cs` (`OrderStatus`/`PaymentStatus` `private set`), `DefectClaim.cs` (`private set` + `ApplyStatus`); tarama `TransitionUnitSourceScanTests.ApplicationSource_WritesTheStatusFieldsOnlyInTheTransitionUnit` |
| k3 tek tablo, kimlikler, tablo dışı red, düzeltme panelden | ✓ | `TransitionTable.cs`; `TransitionTableConsistencyTests.Table_HasSixteenRows_EachCodeExceptCorrectionOnce`; `Correction_IsNotInTheTable_AndHasNoActorKinds`; Integration KK3 testleri |
| k4 geçişi adıyla ister, aktör türü tabloda | ✓ | `TransitionTableTests.ActorsOf_TheTriggersOf03Section1Point2`, `TheClosureDeliveries_TakeTheActorOfTheEventThatCompletesTheClosure` |
| k5 sıra | ✓ | `OrderTransitions.cs` satır 56 (kilit) ... yazım ... kayıt; `Closure_WithoutTheOrdersRowLock_Throws` |
| k6 kayıt yalnız eklenir | ✓ | `TransitionRecord_CannotBeChanged_...`, `AnItemRecord_WritesNoTransitionRecordRow_ByItself_KK6` |
| k7 kapanış | ✓ | `Closure_*_KK7` (iki test), `Apply_TwoTransitionsTogether_AreOneVersionRaise...` |
| k8 durum makinesiz varlıklar | ✓ | `Consistency/StatelessEntitiesConsistencyTests.cs` (üç test) |
| KK1 enum eşitliği, Archived yok | ✓ | k1 ile aynı |
| KK2 birim dışı yazım derlenmez | ✓ | özel setter (derleme) + `Scan_OnTheWriterSample_ReportsEachNameOutsideTheUnit_AndEachOpenStatusField`, `Scan_OnTheBulkSample_ReportsEachBulkAndRawWriteOfAStatusColumn`, `Scan_AFileOfTheSameNameAsTheUnitElsewhere_IsNotTheAllowedPlace`; Integration `Apply_AnAllowedTransition_WritesTheStatus_OneRecord_...KK2` |
| KK3 tabloda olmayan hata verir; düzeltme birimden ve yalnız panelden | ✓ | `Apply_ATransitionOutsideTheTable_...KK3` (30 çift), `Correction_FromTheBuyerOrTheSystem_...KK3`, `Correction_OfARecordThatWasCorrectedAlready_OrIsAForeignOne_IsRefused` |
| KK4 her geçişin aktör türü | ✓ | `EveryRowOfTheTable_HasAtLeastOneActorKind_KK4`; Integration `..._KK4` |
| KK5 yarışan ikinci geçiş | ✓ | `Race_TheSecondTransition_WaitsForTheFirst_...KK5`, `Race_TheConditionOfTheSecondTransition_...KK5`, `Race_AnAdministratorWhoseScreenShowedTheOlderVersion_...KK5` |
| KK6 kayıt güncellenemez, kalem kaydı satır yazmaz | ✓ | k6 ile aynı |
| KK7 son kalem kapanınca aynı işlemde kapanır | ✓ | `Closure_WhenTheLastOpenItemIsCancelled_...KK7`, `Closure_WhileAnOpenPhysicalItemRemains_ClosesNothing_KK7` |
| KK8 varlıklarda durum alanı yok | ✓ | k8 ile aynı |
| `09 §25` satırları | ✓ | tarama ihlal örnekleriyle (`ViolationSamples/TransitionUnit/*`), tablo eşitliği `09 §9.1` madde 2 ile (`Table_EqualsTheTableOfTheCodingGuidelines`), D-72 eşitlemesi |
| `12 §6.1` kanıt, mini güvenlik (sır, kimlik doğrulama, girdi, bağımlılık) | ✓ | yeni paket yok; ham SQL yok (tarama); aktör/yönetici kimliği zorunluluğu `Apply_ByAnAdministratorWithoutAnIdentity_IsRefused`; düzeltme yalnız yönetici |
| `12 §12.3` tablo eşitliği (S1…S11, Ö1…Ö5) | ✓ | `Table_EqualsTheTransitionTablesOfTheRequirementsDocument` (02 §5.4/5.5), `GuidelinesCheck_OnAChangedSource_ReportsTheRow` (değişmiş kopya sınaması) |
| Sık FAIL (1) tarama istisnası geniş | ✓ | istisna yalnız birim dosyasının tam yolu; aynı adlı başka dosya bulgu (`Scan_AFileOfTheSameNameAsTheUnitElsewhere_IsNotTheAllowedPlace`) |
| Sık FAIL (2) çağrı biçimi kaçırma | ✓ | yazıcı adları, açık setter, `ExecuteUpdate`, ham SQL, `ApplyStatus` ayrı örneklerle; yansıma dizgisi de bulgu |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişmedi (altyapı: şema yok) |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | backlog "Aktif kalem" 105 → 106 (D-72 kapandı, D-274/D-275 eklendi); STATUS satırı ⏳; MEMORY ve CONTEXT güncellendi |
| Sık FAIL (5) belge kuralını değiştiren 09 metni | ✓ | 09/05/07 metni değişmedi (karar kaydı gerekmez) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (tamamı) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 3570 geçti, 0 kaldı (Consistency 224, Architecture 844, Infrastructure 1162) |
| Integration (dalın) | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Ordering` | 31/31 (`OrderTransitionsTests`) |
| Integration (komşu) | ✓ | Concurrency 26, Database (MigrationBundle dahil) 36, Model: `CartAndOrderCoreTablesTests` + `ItemRecordTablesTests` + `EnumDatabaseTests` 146, `ModuleCallTransactionTests` + `StartupDataTests` + `SalesGateTests` 50 | hepsi geçti |
| Lint (yerel) | ✓ | `dotnet restore --locked-mode`; `dotnet format --verify-no-changes` (üç proje); `dotnet build Shopfolio.slnx -warnaserror` | temiz, 0 uyarı |
| Contract | — | — | task kapsamında yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07; head `0d30a6f`, `origin/main` birleştirildi — çakışma yalnız kayıt belgelerinde) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-291) |

Validator kanıtları (bağımsız): sekiz kriter ve KK1–KK8 kodda ve testte ✓ — `TransitionTable` on altı satırı `09 §9.1` m.2 ve `02 §5.4`/§5.5 ile eşit, aktör kümeleri `03 §1.2`/§1.3 tetikleyenleriyle uyumlu; `ApplyAsync` sırası kilit → sürüm → beyaz liste/aktör/sebep → çift kuralı → koşul → yazım → kayıt → kapanış (`05 §4.1.2`, §4.1.3); düzeltme yalnız kimlikli yönetici aktörü ve sürümle, düzeltilmiş ya da başka siparişin kaydı reddedilir; durum ayarlayıcıları `private`, yazıcıları birim dışında anan kaynak `TransitionUnitSourceScan` ile kırmızı (ihlal örnekleriyle sınanır; `09 §9.1` m.8 mekanizması). Yerel: `dotnet build -warnaserror` 0 uyarı, `dotnet format --verify-no-changes` üç proje temiz, Unit 3570/3570 (Architecture 844, Consistency 224, öteki 2502), betik testleri 4/4, Integration `OrderTransitionsTests` 31/31 (gerçek PostgreSQL 18), `dotnet restore --locked-mode` 0; `origin/main` birleştirmesinden sonra yeniden `build -warnaserror` temiz ve Unit 3678/3678. Dal CI 42381390535 yeşil. `12`'de T315'in KP satırı yok (`03 §1.1`, §1.10 kaynaklı; `Durum` hücresi yazılmadı). K1 (bloke etmez) → D-291: S1 yalnız sistem aktörüne açık olduğundan havale onayı Ö1+S1 tek istekle yazılamaz (`TransitionRequest` yorumu bunu tek istek gibi anlatıyor); yönetici aktöründe `ExpectedVersion` çekirdekte zorunlu değil — T316/T317.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T315-durum-makinesi-cekirdegi`
- **Commit:** son baş SHA PR açıklamasında
- **PR:** PR açıklamasında
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `DefectClaim.ApplyStatus` çağıranı yok; tarama çağrıyı bugün tümden yasaklıyor | D-274 | talebin kendi geçişleri T320'nindir |
| 2 | Ödeme düzeltmesi iki ekseni birlikte geri alınca çift reddine takılır | D-275 | düzeltmenin koşulları T319'un işlemidir |
| 3 | `IOrderTransitions` bilerek boş: sevkiyat/ödeme/iptal geçiş işlemleri T316/T317/T319'da eklenir | — | task kapsamı çekirdektir |

⚠ (doğrulayıcıya): (a) `TransitionTable.ActorsOf` aktör kümesi ayrı bir sözlüktür; `09 §9.1` madde 2 imzası üç alandır ve metin değişmedi, karar kaydı gerekmedi. (b) Kapanış (S3/S4/S9) Placed + Pending'de çalışmaz çünkü çift tablosu Beklemede ödeme ile iptal edilmiş siparişi yasaklar; ödemesi bekleyen siparişin iptali S3 + Ö2 ikilisiyle birlikte yazılır (`Apply_TwoTransitionsTogether_...CancellationOfAnUnpaidOrderTakesBothAxes`).

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3` 42379036303 (CI) success, 42379036312 (Publish) success, 42368786653 (Publish) success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** devam işçisi 2; `origin/main` birleştirildi (çakışma yok)
