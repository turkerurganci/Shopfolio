# T380 — İşlem izi: sekiz alan, kim-ne zaman-ne değişti, değiştirilemez ve silinemez, sistem olayları ize girmez, düz liste, on yıl

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İz arayüzü** (`Modules/Administration/Contracts`): `IAuditTrail` (`WriteAsync`, `ListAsync`, `ListAdminsAsync`), `AuditEntry`, `AuditQuery` (yalnız tarih aralığı, yönetici, sayfa), `AuditRow`, `AuditPage`, `AuditAdminOption`, `AuditDetails`. Arama, gruplama, dışa aktarma, güncelleme ve silme yöntemi yok.
- **Yazma** (`AuditTrail.WriteAsync`): çağıranın açık veritabanı işlemine katılır (kendi işlemini açmaz); açık işlem yoksa ya da çağıran yönetici değilse `InvalidOperationException`, satır yazılmaz. Ret ve istisnada işlemle birlikte geri alınır.
- **Kurallar** (`AuditEntryRules`): hedef türü ve kimliği birlikte, işlemin sahip olduğu tür; firma/marka/ayar değişikliği eski-yeni değer çiftiyle; sipariş satırları ve hesap silme için kişisel ad kara listesi + kapalı ad listesi + e-posta/TR IBAN deseni; adres/e-posta düzeltmesi, iç not ve hesap silmede çift yok. Değer koleksiyonları `FrozenSet`.
- **Kim** (`AuditActorSource`): istekteki yönetici oturumundan (`AdminScheme` + `Admins` tablosu); istek dışında kod ya da oturumsuz istekte yönetici yok.
- **Okuma** (`AuditTrail.ListAsync`): Türkiye günü aralığı (son gün dahil), yönetici süzgeci (yazılı kimlik üzerinden), en yeni önce, kararlı sıra, 25 satır/sayfa, sayfa sonu aşılırsa son sayfa; kaldırılmış yöneticiler süzgeçte "kaldırıldı" ile listelenir; sonraki sürümün yazdığı bilinmeyen kod hata vermeden gösterilir.
- **Model:** `AuditLog.Details` `AuditDetails?` tipinde, `Infrastructure/PayloadJson.HasPayloadConversion` ile bağlı (göç yok, sütun `jsonb`; D-57 ✅). `Payloads/AuditDetails/1.json` örnek dosyası.
- **Taramalar:** `AuditTrailSourceScan` (tek iz sınıfı, başka tablo adı yok, iz yazan dosya komşu varlığa dokunmaz, sistem kodu izde yok, ihlal kaydı modülü/adı yok) + kasıtlı ihlal örneği `ViolationSamples/AuditTrail/AuditTrailMisuse.cs`.
- **Belgeler:** `DEFERRED_BACKLOG` D-57 ✅, D-47 (1) → T522; status ve memory satırları.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/PayloadJson.cs` (yeni)
- `src/Shopfolio.Web/Modules/Administration/`: `AuditTrail.cs`, `AuditEntryRules.cs`, `AuditActorSource.cs`, `AuditLog.cs`, `AdministrationModule.cs`; `Contracts/AuditDetails.cs`, `Contracts/IAuditTrail.cs`
- `tests/Shopfolio.UnitTests/`: `Administration/AuditEntryRulesTests.cs`, `Administration/AuditTrailSurfaceTests.cs`, `Architecture/AuditTrailSourceScan(.cs, Tests.cs)`, `Architecture/ViolationSamples/AuditTrail/*`, `Payloads/AuditDetails/1.json`
- `tests/Shopfolio.IntegrationTests/Model/AuditTrailTests.cs` (yeni), `Model/AdminAndAuditTablesTests.cs`
- `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz alan | ✓ | `AnEntryOfEachOfTheEightAreas_IsAccepted_AndTheAreaComesFromTheOperation`, `TheEightAreas_AreEightAndEveryOperationBelongsToOne`; tümleşik `EveryOperation_IsWritten_AndAllEightAreasAreOnTheRows` |
| 2 | Kim, ne zaman, neyi; ayarda eski-yeni | ✓ | `Write_InsideTheRunner_AddsOneRowWithTheAdministratorTheMomentTheAreaAndTheDetails`, `ASettingsChange_KeepsTheOldAndTheNewValue_AndTheListReturnsThem`, `ACompanyBrandingOrSettingsChange_WithoutTheChangedFields_IsRefused` |
| 3 | Sipariş satırı kişisel veri taşımaz; iç not metni yok | ✓ | `AnOrderRow_Refuses…` (üç test), `TheCorrectionOfAnAddressOrAnEmailAndTheNote_CarryNoOldAndNewPair`, tümleşik `AnOrderEntryWithTheCustomersData_IsRefused_AndNoRowIsWritten`, `TheRowOfAnAddressCorrection_NamesTheOrderTheFieldAndTheOperationOnly` |
| 4 | Aynı işlemde yazılır | ✓ | `ARefusedResult_UndoesTheRow_AndAnExceptionUndoesIt`, `ACommittedOperation_KeepsEveryRowItWrote`, `Write_OutsideATransaction_IsRefused_AndNothingIsWritten` |
| 5 | Değiştirilemez, silinemez; yalnız ekle-oku; imha fonksiyonu siler | ✓ | `TheApplicationUser_CannotChangeOrRemoveARow_ThroughTheContext` (+ T106 `AdminAndAuditTablesTests`, `DisposalTests`); `TheRowEntity_HasNoPublicSetter_AndTheOneWayToMakeIsCreate` |
| 6 | Düzeltme yeni satır | ✓ | `ACorrectionOfAWrongTransition_IsANewRow_AndTheWrongOnesRowStaysAsItWas` |
| 7 | Sistem olayı/komut ize yazılmaz | ✓ | `TheCodes_NameNoSystemEvent_TheDisposalNoLoginNoUndeliveredEmailAndNoInstallationCommand`, `AuditTrailSourceScanTests.TheSystemRule_…`, `WithoutAnAdministrator_ANonAdminCallerGetsAnErrorAndNoRow` |
| 8 | Düz liste; tarih aralığı + yönetici süzgeci; arama/gruplama/dışa aktarma yok | ✓ | `TheListQuery_HasTheDateRangeTheAdministratorAndThePageOnly`, `TheTrailInterface_WritesAndReads_AndHasNoSearchNoGroupingNoExportNoUpdateNoDelete`, tümleşik `TheDateRange_IsTurkeyDays_…`, `TheAdministratorFilter_…`, `ThePages_AreTwentyFiveRows_…`, `TheList_IsNewestFirst_…` |
| 9 | On yıl saklanır; kaldırılan yönetici satırları ad/e-postayla durur | ✓ | `TheRowsOfARemovedAdministrator_StayWithTheNameWritten_…`, `ARowNineYearsOld_StaysWhenTheDisposalRuns_…` (+ T106 sınır testleri) |
| 10 | İhlal kaydı modülü ve toplu bildirim yok | ✓ | `AuditTrailSourceScanTests.TheBreachRule_SeesTheNameInAnIdentifierARouteAndAView`, `TheBreachRule_SeesAFileNamedAfterABreach`, `ApplicationSource_HasOneTrailClass_…_AndNoBreachModule` |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçinin kodu dahil bütün maddeleri diff'te yeniden denetledi.

| Madde (KK) | Sonuç | Kanıt |
|---|---|---|
| KK1 sekiz alan kabul | ✓ | `AuditEntryRulesTests.AnEntryOfEachOfTheEightAreas…`, `EveryOperation_HasAValidEntry…`; `AuditTrailTests.EveryOperation_IsWritten…` |
| KK2 satır alanları; eski-yeni | ✓ | `AuditTrailTests.ASettingsChange_KeepsTheOldAndTheNewValue…` |
| KK3 adres/e-posta/iç not metni satırda yok | ✓ | `AnOrderRow_Refuses…`, `AnOrderEntryWithTheCustomersData_IsRefused…` |
| KK4 geri alma | ✓ | `ARefusedResult_UndoesTheRow_AndAnExceptionUndoesIt`, `Write_OutsideATransaction…` |
| KK5 güncelleme/silme reddi | ✓ | `TheApplicationUser_CannotChangeOrRemoveARow_ThroughTheContext` |
| KK6 düzeltme yeni satır | ✓ | `ACorrectionOfAWrongTransition…` |
| KK7 sistem olayı ve komut satır üretmez | ✓ | `TheCodes_NameNoSystemEvent…`, `TheSystemRule_…` |
| KK8 süzgeçler var; arama/dışa aktarma yok | ✓ | `AuditTrailSurfaceTests`, tümleşik liste testleri |
| KK9 on yıl dolmadan silinmez | ✓ | `ARowNineYearsOld_StaysWhenTheDisposalRuns…` |
| KK10 ihlal kaydı modülü/bildirim yok | ✓ | `AuditTrailSourceScan` BreachModule kuralı |
| `09 §25` ilgili satırlar | ✓ | `09 §9.2` aynen uygulandı; yeni 09 metni yok |

**`sik-fail-nedenleri` kontrolü:**

1. Tarama istisnası geniş: kaynak taramasında istisna yalnız iz dosyaları ve imha fonksiyonu; her kuralın ihlal örneği var (`AuditTrailMisuse.cs`), yorum/dize/imha fonksiyonu temiz sayılır (`ViolationSample_EachRuleReportsItsLines_…`).
2. Çağrı biçimi kaçırma: kural başına çok biçimli testler (`TheTouchRule_SeesEveryCallFormOfTheSetAndTheEntity`, `TheTableRule_SeesTheNameInEveryStatementForm…`, `TheSystemRule_…`, `TheBreachRule_…`).
3. "Varsayılan"/FK dizini: şema değişikliği yok (`audit_logs` T106'da); `MigrationBundleTests` yeşil.
4. Sayı anan öteki yerler: 43 kod/8 alan sayıları değişmedi; backlog "Aktif kalem" 93 → 92 güncellendi.
5. Karar kaydı gerektiren yeni 09 metni: yok.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 2908/2908 | `dotnet test tests/Shopfolio.UnitTests --no-build` | 0 hata (T229'daki 2826'dan +82) |
| Integration | ✓ | `--filter-class` `AuditTrailTests` + `AdminAndAuditTablesTests` (63), `DisposalTests` + `MigrationBundleTests` (+ `AdminAndAuditTablesTests`, 62) | 0 hata; `AuditTrailTests` 21 test |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (Web, Unit, Integration) | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`Details` yalnız model tipi, sütun `jsonb`; `MigrationBundleTests` yeşil)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T380-islem-izi`
- **PR:** açıldığında PR gövdesinde
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Ana dal CI startup: `gh run list --branch main --limit 3` → 42335449709 success, 42335449717 success, 42306192052 success.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `AuditOperation` görünen etiket testi (mesaj kataloğu) | D-47 (1) → T522 | Etiket panel ekranı ve katalogla gelir |
| 2 | Tarih aralığı ileri/ters doğrulaması | — (T522 sayfa katmanı) | Servis ters aralıkta boş liste döner; kullanıcıya ileti sayfanın işi |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** yukarıda.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ Kolay kararlar:** (1) Müşteri verisi için kapalı ad listesi ve hedef tür haritası `09`'da yok, `AuditEntryRules`'ta; (2) IBAN deseni yalnız TR; (3) kişisel ad kara listesi harf-dışı karakterleri atarak küçük harfle karşılaştırır.
- **Devir:** önceki işçi bağlam sınırında bölündü; bu işçi derleme/test düzeltmelerini (Integration yardımcı kopyası, `FrozenSet`, iddiasız testler) yaptı ve raporu yazdı.
