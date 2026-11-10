# T380 — İşlem izi: sekiz alan, kim-ne zaman-ne değişti, değiştirilemez ve silinemez, sistem olayları ize girmez, düz liste, on yıl

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-07

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
| Doğrulama durumu | ✓ PASS (tur 2, 2026-11-07, head `1180a80` + `origin/main` birleştirmesi) · tur 1 ✗ FAIL (head `4345bf9`) |
| Bulgu sayısı | Tur 2: 0 · tur 1: 1 (KRİTİK, S1 — kriter 3 / KK3 / `09 §9.2` m.4; Düzeltme 1'de kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | Tur 2: 2 (K1 raporda kapatıldı, K2 → D-176) · tur 1: 1 (K1 → D-164) |

Tur 2'nin tam sonucu raporun sonundaki "Doğrulama Sonucu — T380 İşlem izi — tur 2" bölümündedir.

### Doğrulama Sonucu — T380 İşlem izi — tur 1
**Tarih:** 2026-11-07   **Branch:** task/T380-islem-izi   **Commit:** 4345bf9 (doğrulanan head; doğrulama kaydı bunun üstüne commit'lendi)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `scripts/git-hooks` |
| Ana dal CI (son 3) | ✓ | 42335449709 success · 42335449717 success · 42306192052 success |
| Memory drift | ✓ | `grep -nE "\bT380\b" .claude/memory/MEMORY.md` → satır 22 |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz alan | ✓ | `AuditOperationText.AreaOf` 43 kod → 8 alan; `AuditEntryRulesTests.TheEightAreas_AreEightAndEveryOperationBelongsToOne`, `AnEntryOfEachOfTheEightAreas…`; tümleşik `EveryOperation_IsWritten_AndAllEightAreasAreOnTheRows` yeşil |
| 2 | Kim, ne zaman, neyi; ayarda eski-yeni | ✓ | `AuditTrail.WriteAsync` yöneticiyi `IAuditActorSource`'tan, anı `TimeProvider`'dan alır; `UpdateCompany/UpdateBranding/UpdateSettings` `Changes` olmadan reddedilir; `ASettingsChange_KeepsTheOldAndTheNewValue_AndTheListReturnsThem` yeşil |
| 3 | Sipariş satırı kişisel veriyi değer olarak taşımaz; iç not metni ize girmez | ✗ | Koruma yalnız ad listesi (alan/değer adı) ve `@`/TR IBAN deseniyle; `DisplayName` serbest metin ve hiçbir ad/adres/telefon/not denetimi görmez. Doğrulayıcı yoklaması (geçici birim test, silindi; 4/4 "kabul edildi"): `CorrectAddress` + `DisplayName` "Atatürk Mah. 12. Sk. No:3 Kadıköy" · `SaveInternalNote` + `DisplayName` "Müşteri kapıda aranmak istiyor" · `DeleteCustomerAccount` + `DisplayName` "Ayşe Yılmaz" · `MarkShipped` + `Reason` = "0532 123 45 67" — dördü de `AuditEntryRules.Validate`'ten geçer ve yazılır. Bulgu 1 |
| 4 | Aynı veritabanı işleminde | ✓ | `WriteAsync` açık işlem yoksa `InvalidOperationException`; satır çağıranın `SaveChanges`'ıyla gider; `ARefusedResult_UndoesTheRow_AndAnExceptionUndoesIt`, `ACommittedOperation_KeepsEveryRowItWrote`, `Write_OutsideATransaction_IsRefused…` yeşil |
| 5 | Değiştirilemez/silinemez; yalnız ekle-oku; imha fonksiyonu | ✓ | Uygulama kullanıcısıyla `ExecuteUpdate`/`ExecuteDelete` → 42501 (`TheApplicationUser_CannotChangeOrRemoveARow_ThroughTheContext`); T106 `AdminAndAuditTablesTests` (yetki, fonksiyon) yeşil; arayüzde güncelleme/silme yöntemi yok (`AuditTrailSurfaceTests`) |
| 6 | Düzeltme yeni satır | ✓ | `ACorrectionOfAWrongTransition_IsANewRow_AndTheWrongOnesRowStaysAsItWas` yeşil |
| 7 | Sistem olayı/komut ize yazılmaz | ✓ | Yönetici yoksa `InvalidOperationException` (`WithoutAnAdministrator…`, `TheRequestSource_HasNoActor…`); `AuditTrailSourceScan` paylaşılan altyapının `IAuditTrail`/`AuditEntry`'ye erişimini kırar (ihlal örneğiyle); kodlarda sistem olayı yok |
| 8 | Düz liste; tarih aralığı + yönetici süzgeci; arama/gruplama/dışa aktarma yok | ✓ | `AuditQuery` yalnız `From/To/AdminId/Page`; Türkiye günü sınırı, en yeni önce + `Id`, `PageSizes.Panel`, sayfa sınırı testleri yeşil (arayüz düzeyi; ekran T522) |
| 9 | On yıl; kaldırılan yönetici ad/e-postayla | ✓ | Satır yönetici adını/e-postasını değer olarak taşır; `TheRowsOfARemovedAdministrator_StayWithTheNameWritten…`, `ARowNineYearsOld_StaysWhenTheDisposalRuns…` + T106 sınır testleri yeşil |
| 10 | İhlal kaydı modülü/toplu bildirim yok | ✓ | `AuditTrailSourceScan` `BreachModule` kuralı (tanımlayıcı, rota, görünüm, dosya adı) + ihlal örneği; kaynak taraması yeşil |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-A310 (kriter 2) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D036 (kriter 1) | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D037 (kriter 1) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D038 (kriter 1) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D039 (kriter 2) | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D040 (kriter 2) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D041 (kriter 1, 3) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-A311 (kriter 8) | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D042 (kriter 5) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |

`grep -nE 'KP-63([^0-9]|$)|KP-77([^0-9]|$)|KD-58([^0-9]|$)'`: KP-63 dokuz satır; KP-77 satırları (VAL-A104, A107–A110, D024, C031) üye hesabı ekranı/silme akışınındır (T380'in payı yalnız silme satırının yazılabilmesi); KD-58'in satırı yok (kapsam dışı). Satırların ön koşulu (panel oturumu, ilgili işlemin akışı — T522 ve çağıran task'lar) yok; kanıt türü `Uç yanıtı`/`Tarayıcı gözlemi` arayüz testiyle üretilemez → `12 §5` kural 1 gereği `BEKLEMEDE` kalır. `12_matris_denetle.py --kapsam 1-77` hata 0.

#### Doğrulama Kontrol Listesi
- [x] KK1: sekiz alan türü kabul edilir
- [x] KK2: satır alanları; ayarda eski–yeni değer
- [ ] KK3: adres, e-posta ve iç not metni satırda yok — e-posta reddedilir; adres ve iç not metni `DisplayName`'de (ve serbest değer adlarında) kabul edilip yazılır (Bulgu 1)
- [x] KK4: işlem geri alınınca iz de geri alınır
- [x] KK5: güncelleme ve silme yetkisi reddedilir
- [x] KK6: düzeltme yeni satır, eskisi durur
- [x] KK7: sistem olayı ve komut satır üretmez
- [x] KK8: süzgeçler var, arama ve dışa aktarma yok
- [x] KK9: on yıl dolmadan silinmez
- [x] KK10: ihlal kaydı modülü ve toplu bildirim yok
- [ ] `09 §25`: "Yeni ya da değişen her kuralın bölümündeki Test maddesi karşılanmış mı?" — `09 §9.2` m.4 Test'i ("bilinen ad, adres, telefon, e-posta, IBAN ve not metniyle yazılan satırın `Details`'i bu değeri içermez") yalnız kişisel adlı alan yuvasında sınanmış; `DisplayName` ve izinli adlı değer yuvası sınanmamış ve bu yuvalarda değer yazılıyor (Bulgu 1). Öteki ilgili satırlar (§9.2 kod listesi/aynı işlem, §10.5 sayfalama `Id` + `ToPageAsync` + sınır testi, §12 yük örneği `Payloads/AuditDetails/1.json`, §6 modül sınırı + mimari tarama, TimeProvider) ✓
- [x] Güvenlik taraması (aşağıda)

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (Web tam; Unit ve Integration değişen dosyalarla `--include`; Unit tam koşu 150 sn'yi aştı — CI Lint yeşil) | çıkış 0 |
| Unit (+Consistency) | ✓ 2908/2908 | `dotnet test --project tests/Shopfolio.UnitTests … --filter-namespace` altı parça | 68 + 786 + 916 + 491 + 440 + 207; 0 hata |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration | ✓ (parçalı, kısmi) | PostgreSQL 18 (`shopfolio-t380-val-pg`, 127.0.0.1:55710, koşu sonrası silindi): `MigrationBundleTests` 2 · `AuditTrailTests` + `AdminAndAuditTablesTests` 63 · `Database` 36 · `Security` + `Disposal` 50 · Model komşuları (Enum, ModelRule, ModuleCallTransaction, StartupData, SettingsTables, SampleDatabase, SchedulerBackupDisposalAndKey) 148 | 299/299 yeşil; kalan (Web, Pages, Jobs, Concurrency, öteki Model) için dal CI'ına dayanıldı |
| Dal CI | ✓ | `gh run list --branch task/T380-islem-izi` | 42339490480 success (head `4345bf9`) |
| Restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; `git status` temiz |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz — yönetici yalnız `AdminScheme` kimliğinden ve `Admins` satırından; oturumsuz/istek dışı kodda yazma istisnayla reddedilir
- Input validation: Bulgu — müşteri kişisel verisinin ize girmesine karşı doğrulama `DisplayName` ve serbest değer yuvasını kapsamıyor (Bulgu 1; on yıl silinemeyen tabloya kişisel veri)
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | Kriter 3, KK3, `09 §9.2` m.4 (+ Test cümlesi) | KRİTİK | S1 | Beklenen: `Order` kodlarında ve `DeleteCustomerAccount`'ta müşterinin adı, adresi, telefonu, e-postası, IBAN'ı ve not metni `Details`'in **hiçbir** yuvasında değer olarak yazılamaz, `WriteAsync` doğrular; hesap silme satırı "hesabı değil işlemi ve tarihini adlandırır" (`06 §4.63`, `02 §10.3.1` (8)). Gerçekleşen: `DisplayName` serbest metin — adres, iç not metni ve üyenin adı `DisplayName` olarak kabul edilip yazılır; izinli adlı değerlerde (`Reason` vb.) telefon/ad/adres metni de geçer; testler yalnız kişisel adlı alanı sınar. Düzeltme önerisi: (a) `DeleteCustomerAccount`'ta `DisplayName` yok (null değilse `ArgumentException`); (b) `Order` kodlarında `DisplayName` yalnız sipariş numarası biçiminde; (c) korunan kodlarda serbest metin taşıyabilen değer adları (`reason`, `correctionreason` vb.) kapalı kod/tarih/sayı biçimine bağlanır ya da listeden çıkar; (d) `09 §9.2` m.4 Test'i her yuvada (`DisplayName`, `Values`, `Changes` eski/yeni) bilinen ad, adres, telefon ve not metniyle sınanır | VAL-D041 (silinen üyenin kişisel verisi izde yok), VAL-D024 | düzeltme turu (bu PR) | `src/Shopfolio.Web/Modules/Administration/AuditEntryRules.cs` · `tests/Shopfolio.UnitTests/Administration/AuditEntryRulesTests.cs` · `tests/Shopfolio.IntegrationTests/Model/AuditTrailTests.cs` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `AuditEntryRules.TargetTypesOf(UpdateSettings)` yalnız `CompanySettings`/`Province`'i kabul eder (`09 §9.2` m.3 tablosuyla aynı); `02 §10.3.1` (6) ve `VAL-D039` ürün formundaki P-6, P-9, P-44 değişikliklerini de eski-yeni değerle ize ister — konusu `Product` olan satır bugün reddedilir. Ayrıca müşteri verisi için kapalı ad listesi ve hedef tür haritası yalnız kodda (`09`'da yok; yapım ⚠ (1)). | DEFERRED_BACKLOG'a forward: D-164 (T381 + D-159 doküman geçişi) |

#### Yapım Raporu Karşılaştırması
- Uyum: 1 uyuşmazlık
- Yapım raporu kriter 3'ü ve KK3'ü ✓ sayar ve öz denetimde `09 §25` satırlarını "aynen uygulandı" diye ✓ geçer; kanıt olarak gösterilen testler yalnız kişisel adlı alan yuvasını ve e-posta/IBAN desenini sınar, `DisplayName` yuvasını sınamaz. Öteki dokuz kriter ve test sayıları (Unit 2908, `AuditTrailTests` 21) doğrulayıcı sonucuyla uyumlu. Öz denetim tablosu var, maddeleri kanıtlı.

#### Merge ve Post-merge
- FAIL — merge yok; dal düzeltme turuna döner.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`Details` yalnız model tipi, sütun `jsonb`; `MigrationBundleTests` yeşil)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T380-islem-izi`
- **PR:** #419
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** squash `7f8d6c3` (#419) — CI 42356915419 success, Publish 42356915467 success (T331 yapımında dolduruldu)

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

## Düzeltme 1 (doğrulama tur 1 FAIL → bulgu 1 KRİTİK S1)

**Yapılan:** `AuditEntryRules` müşteri kişisel verisini `Details`'in her yuvasında denetler (`WriteAsync` doğrular, ihlal `ArgumentException`).

- `Order` alanı satırlarında görünen ad yalnız sipariş numarasıdır (`yıl-blok`, `02 §3.22.1`); `DeleteCustomerAccount`'ta görünen ad yoktur (satır işlemi ve tarihini adlandırır; `06 §4.63`, `02 §10.3.1` (8)).
- Değer adları kapalı listededir ve her ad tek bir kapalı biçime bağlıdır: kod (ASCII harf/rakam, harfle başlar, boşluk ve Türkçe harf yok, en çok 64), ISO tarih, adet (en çok 6 hane), tutar, kimlik (Guid). Alan adı ve eski/yeni değer çiftleri aynı biçim denetiminden geçer.
- Serbest metin taşıyabilen adlar (`carrier`, `carriername`, `trackingnumber`, `item`) listeden çıkarıldı.
- E-posta/IBAN deseni her yuvada korundu (kod biçimine uyan `TR…` IBAN'ı için).
- `09 §9.2` m.4'e tek cümle: yasak `Details`'in her yuvası için geçerli, `Order`'da görünen ad sipariş numarası, hesap silmede yok, değer adı/biçim kapalı, Test her yuvada. `02`/`06` ile çelişmez (yeni kural değil, `02 §10.3.1` (3), (8) ve `06 §4.63`'ün yuva düzeyinde uygulanışı).
- Backlog: dalın doğrulayıcı kaydı D-161 → **D-164** (main'deki D-161 T378'in); "Aktif kalem" main'e göre yeniden sayıldı (93 satır); dal içindeki bütün atıflar güncellendi.

**Testler:** Unit `AuditEntryRulesTests`: bilinen ad ("Ayşe Yılmaz"), adres, telefon (üç biçim), not metni örnekleriyle her yuva (görünen ad; `Field/Operation/Reason/ReasonCode/CorrectionReason/TransitionCode/FromStatus/ToStatus/Channel` değeri ve eski/yeni; adet/tutar/tarih/kimlik adları) × bütün `Order` işlemleri ve `DeleteCustomerAccount` reddi; görünen ad yalnız sipariş numarası; hesap silmede görünen ad yok; kapatılan adlar; yanlış biçimli değer; kabul edilen kod/tarih/adet/tutar/kimlik; ayar satırı (`UpdateSettings`) biçime bağlı değil. Integration `AKnownNameAddressPhoneAndNote_AreRefusedInEverySlotOfTheDetails…`: reddedilen girişlerden sonra `audit_logs` boş.

### Öz denetim (Düzeltme 1)

| # | Kriter / KK | Sonuç | Kanıt |
|---|---|---|---|
| 1 / KK1 | Sekiz alan | ✓ | Değişmedi; `TheEightAreas_AreEightAndEveryOperationBelongsToOne`, `EveryOperation_IsWritten_AndAllEightAreasAreOnTheRows` yeşil |
| 2 / KK2 | Kim, ne zaman, neyi; ayarda eski-yeni | ✓ | Değişmedi; `ASettingsChange_KeepsTheOldAndTheNewValue…`, `TheValuesOfASetting_AreNotHeldToTheShapes…` yeşil |
| 3 / KK3 | Kişisel veri/iç not metni değer olarak yazılamaz (her yuva) | ✓ | `AnOrderRow_RefusesAKnownNameAddressPhoneAndNote_InEverySlotOfTheDetails` (8 örnek), `TheDeletionOfAnAccount_Refuses…`, `TheDisplayNameOfAnOrderRow_IsTheOrderNumberOnly`, `TheDeletionOfAnAccount_HasNoDisplayName…`, `AValueNameThatCanCarryFreeText_IsNotOnTheClosedList`; Integration `AKnownNameAddressPhoneAndNote_AreRefused…` |
| 4 / KK4 | Aynı veritabanı işlemi | ✓ | Değişmedi; `AuditTrailTests` 22/22 |
| 5 / KK5 | Değiştirilemez/silinemez | ✓ | Değişmedi; tablo/yetki testleri `AuditTrailTests` içinde yeşil |
| 6 / KK6 | Düzeltme yeni satır | ✓ | `ACorrectionOfAWrongTransition_IsANewRow…` yeşil (`TransitionCode` = `S6` kod biçimi) |
| 7 / KK7 | Sistem olayı ize girmez | ✓ | Değişmedi; `AuditTrailSourceScanTests` 23/23 |
| 8 / KK8 | Düz liste, süzgeçler | ✓ | Değişmedi |
| 9 / KK9 | On yıl | ✓ | Değişmedi |
| 10 / KK10 | İhlal modülü yok | ✓ | `AuditTrailSourceScanTests` yeşil |
| `09 §25` Test | Her yuvada sınama | ✓ | Yukarıdaki yuva testleri |

**Sonuçlar:** build `-warnaserror` 0/0 (Unit, Integration); Unit `Administration` 100/100, `AuditTrailSourceScanTests` 23/23; Integration `AuditTrailTests` 22/22 (PostgreSQL 18, `t380-pg`, silindi); `dotnet format --verify-no-changes` değişen üç dosyada temiz.

**⚠ Kolay karar:** kod biçimi tek bir ASCII sözcüğü ("Ayse") ayıramaz; yalnız boşluklu/Türkçe harfli/rakamla başlayan metni reddeder. Çağıran task'lar kod olarak enum adı verir; daha sıkısı kodun gerçek sabit kümesini bilmeyi gerektirir (çağıran task'larda).

## Düzeltme 2 (düzeltme 1 PR CI kırmızı: "3. Unit test")

**Yapılan:** Hata `ForbiddenTestPatternScanTests.TestProjects_Sources_HaveNoForbiddenPatterns`'ti. Yerelde koşuldu: `AuditEntryRulesTests.TheValuesOfASetting_AreNotHeldToTheShapes_TheFirmsOwnIbanAndReturnAddressAreWritten` doğrulama çağırıyor ama hiç `Assert` yoktu (`TestWithoutAssertion`). Test kodu, sibling testlerle aynı biçimde `Assert.Null(Record.Exception(() => ...))` ile "istisna atmaz" iddiasına bağlandı. Tarama, bekçi ya da istisna listesi değiştirilmedi; kişisel veri düzeltmesine (KK3) dokunulmadı.

- `origin/main` dala birleştirildi. Çakışmalar yalnız kayıt belgelerinde: `IMPLEMENTATION_STATUS` "Son güncelleme" T380 girişi kaldı, main'in T313 girişi `STATUS_CHANGELOG.md`'ye taşındı; `MEMORY` üç satır da kaldı (T380, T313, T314); `DEFERRED_BACKLOG` main'in D-162/D-163 kayıtları korundu, T380'in kaydı D-164 kaldı; "Aktif kalem" yeniden sayıldı: 94 (91 açık + 3 sürüyor), main ile aynı. Yeni backlog kaydı yok.

**Testler (birleştirilmiş dal, Postgres 18 `t380`, silindi):** Unit `Architecture` 787/787 (taramanın kendisi dahil), Unit geri kalan (`Consistency=true` hariç) 2082/2082, kırmızı yok. Integration `Model` içinde `AuditTrailTests` + `AdminAndAuditTablesTests` 64/64. Build 0 uyarı, 0 hata.

### Öz denetim (Düzeltme 2)

| # | Madde | Sonuç | Kanıt |
|---|---|---|---|
| 1 | CI hatası kapandı | ✓ | `ForbiddenTestPatternScanTests` yerelde yeşil (Architecture 787/787) |
| 2 | Bekçi/tarama istisnası eklenmedi, tarama gevşetilmedi | ✓ | `tests/Shopfolio.UnitTests/Architecture/` değişmedi; yalnız `AuditEntryRulesTests.cs` tek satır |
| 3 | Kabul kriterleri 1–10, KK1–KK10 | ✓ | Düzeltme 1 tablosundaki testler birleştirilmiş dalda yeşil (Administration, AuditTrailTests) |
| 4 | Kişisel veri düzeltmesi (KK3) bozulmadı | ✓ | `AuditEntryRules` değişmedi; yuva testleri yeşil |
| 5 | Kural değişmedi, sayı anan başka yer yok | ✓ | Belge kuralı/09 metni değişmedi; yalnız test satırı |
| 6 | 06 Varsayılan/FK dizini | ✓ (kapsam dışı) | Şema değişmedi |
| 7 | Kayıt belgeleri tutarlı | ✓ | STATUS tek giriş, eskisi changelog'da; backlog sayacı 94 |

## Doğrulama tur 2

### Doğrulama Sonucu — T380 İşlem izi — tur 2
**Tarih:** 2026-11-07   **Branch:** task/T380-islem-izi   **Commit:** `1180a80` (doğrulanan head; 16. adımda `origin/main` birleştirmesi + kayıt commit'i eklendi)

#### Verdict: ✓ PASS

Kapsam TAM: kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması baştan koşuldu. Tur 1 bulgusu (müşterinin kişisel verisi `Details`'in `DisplayName` yuvasında) kapandı: `AuditEntryRules` `Order` kodlarında ve `DeleteCustomerAccount`'ta her yuvayı denetler — `Order`'da görünen ad yalnız sipariş numarası (`^\d{4}-[A-Za-z0-9]{4,12}$`), hesap silmede görünen ad yok, değer ve alan adları kapalı listede ve her adın değeri tek kapalı biçimde (kod, ISO tarih, ≤6 haneli adet, tutar, Guid), `@` ve TR IBAN deseni her yuvada reddedilir, `CorrectAddress`/`CorrectContactEmail`/`SaveInternalNote`/`DeleteCustomerAccount`'ta eski–yeni çifti yok.

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana dizinin bekçisi) |
| Ana dal CI (son 3) | ✓ | 42351368236 success, 42351368096 success, 42346300012 success |
| Memory drift | ✓ | `grep -nE "\bT380\b" .claude/memory/MEMORY.md` → satır 22 (+ satır 98, T106) |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz alan | ✓ | `AuditOperationText.AreaOf` 43 kod → 8 alan; `AuditEntryRulesTests.AnEntryOfEachOfTheEightAreas_IsAccepted…`, `EveryOperation_HasAValidEntry…`; tümleşik `EveryOperation_IsWritten_AndAllEightAreasAreOnTheRows` (yerelde yeşil) |
| 2 | Kim, ne zaman, neyi; ayarda eski–yeni | ✓ | `AuditTrail.WriteAsync` yönetici kimliği/ad/e-postayı `IAuditActorSource`'tan, anı `TimeProvider`'dan yazar; `UpdateCompany/UpdateBranding/UpdateSettings` çiftsiz reddedilir; `Write_InsideTheRunner_AddsOneRow…`, `ASettingsChange_KeepsTheOldAndTheNewValue…` |
| 3 | Sipariş satırında kişisel veri yok; iç not metni yok | ✓ | `AuditEntryRules.ValidateDetails` (kod okundu); tümleşik `AKnownNameAddressPhoneAndNote_AreRefusedInEverySlotOfTheDetails…` ("Ayşe Yılmaz", adres, telefon, not × 7 yuva → `ArgumentException`, 0 satır), `TheRowOfAnAddressCorrection_NamesTheOrderTheFieldAndTheOperationOnly` (`details::text` "Mah." içermez) |
| 4 | Aynı veritabanı işleminde | ✓ | `WriteAsync` işlem yoksa `InvalidOperationException`, `SaveChanges` çağırmaz; `ARefusedResult_UndoesTheRow_AndAnExceptionUndoesIt`, `Write_OutsideATransaction_IsRefused…` |
| 5 | Değiştirilemez/silinemez; yalnız ekle–oku; imha fonksiyonu | ✓ | `TheApplicationUser_CannotChangeOrRemoveARow_ThroughTheContext` (uygulama kullanıcısıyla `ExecuteUpdate`/`ExecuteDelete` → 42501); T106 `AdminAndAuditTablesTests` yetki/katalog/sınır testleri yeşil; arayüzde güncelleme/silme yok (`AuditTrailSurfaceTests`) |
| 6 | Düzeltme yeni satır | ✓ | `ACorrectionOfAWrongTransition_IsANewRow_AndTheWrongOnesRowStaysAsItWas` |
| 7 | Sistem olayı ve kurulum komutu yazılmaz | ✓ | yönetici yoksa istisna (`WithoutAnAdministrator_…`); `TheCodes_NameNoSystemEvent…`; kaynak taraması `SystemCodeReachesTheTrail` (`Infrastructure/` `IAuditTrail`'e erişemez) |
| 8 | Düz liste; tarih aralığı + yönetici süzgeci; arama/gruplama/dışa aktarma yok | ✓ | `AuditQuery(From, To, AdminId, Page)`; en yeni önce + `Id`, `PageSizes.Panel`=25; `TheListQuery_HasTheDateRangeTheAdministratorAndThePageOnly`, `TheTrailInterface_…NoSearchNoGroupingNoExport…`, `TheDateRange_IsTurkeyDays_…`, `ThePages_AreTwentyFiveRows_…` |
| 9 | On yıl; kaldırılan yöneticinin satırı ad/e-postayla | ✓ | `ARowNineYearsOld_StaysWhenTheDisposalRuns…`, `TheRowsOfARemovedAdministrator_StayWithTheNameWritten…`; T106 sınır testleri |
| 10 | İhlal kaydı modülü ve toplu bildirim yok | ✓ | kaynak taraması `BreachModule` kuralı + ihlal örneği; `ApplicationSource_…_AndNoBreachModule` yeşil |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-A310 (kriter 2) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D036 (kriter 1) | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D037 (kriter 1) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D038 (kriter 1) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D039 (kriter 2) | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D040 (kriter 2) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D041 (kriter 1, 3) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-A311 (kriter 8) | ORTA | yok | BEKLEMEDE | BEKLEMEDE | — |
| VAL-D042 (kriter 5) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE | — |

KP-63 satırlarının (`11 §7.1`: KP-63 → T380, T522) kanıt türü "Uç yanıtı / Tarayıcı gözlemi + Veri kaydı"dır; panel ekranı (T522) ve izi yazan panel işlemleri (T381, sipariş ve yönetici task'ları) gelmeden uç ve ekran kanıtı üretilemez → `BEKLEMEDE` (T314 emsali). T380'in payı (arayüz, kurallar, veritabanı koruması) kriter 1–10'da kanıtlandı. KP-77 satırları (VAL-A104, A107–A110, D024, C031) üye kaydı ekranı/silme akışınındır (T345, T533). KD-58'in satırı yok. `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

#### Doğrulama Kontrol Listesi
- [x] KK1: sekiz alan türü kabul edilir
- [x] KK2: satır alanları; ayarda eski–yeni değer
- [x] KK3: adres, e-posta ve iç not metni satırda yok (her yuvada)
- [x] KK4: işlem geri alınınca iz de geri alınır
- [x] KK5: güncelleme ve silme yetkisi reddedilir
- [x] KK6: düzeltme yeni satır, eskisi durur
- [x] KK7: sistem olayı ve komut satır üretmez
- [x] KK8: süzgeçler var, arama ve dışa aktarma yok
- [x] KK9: on yıl dolmadan silinmez
- [x] KK10: ihlal kaydı modülü ve toplu bildirim yok
- [x] `09 §25`: arayüz metodunda `SaveChanges` yok; yalnız `AuditTrail` `AuditLogs`'a dokunur (tarama); zaman `TimeProvider`; sayfalı liste `Id` ile biter, `ToPageAsync` + `PageSizes`, sınır testi var; okuma izlemesiz; süzme eksenleri 06'nın iki dizininde (T106); yük biçimi `PayloadJson` + `HasConversion`/`ValueComparer`, örnek `Payloads/AuditDetails/1.json`; yeni paket/ayar/göç yok; yeni kural için kaynak taraması ve ihlal örneği var
- [x] Güvenlik taraması (aşağıda)

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format <Web|Unit|Integration csproj> --no-restore --verify-no-changes` | üçü çıkış 0 |
| Consistency | ✓ | `dotnet test --project tests/Shopfolio.UnitTests … --filter-trait "Consistency=true"` | 207/207 |
| Unit | ✓ | `… --filter-not-trait "Consistency=true"` | 2869/2869 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (PostgreSQL 18, `shopfolio-t380-val-pg`, 127.0.0.1:55640, silindi) | ✓ | `--filter-class` / `--filter-namespace` parçaları | MigrationBundle 2/2; Audit sınıfları (`AuditTrailTests`, `AdminAndAuditTablesTests`, `DisposalTests`) 82/82; Model 915/915; Security+Concurrency+Jobs+Disposal 153/153; Database+FileSecrets+Files+Logging+Outages+SignedEndpoints 142/142; Web A–E 80/80, F–L 149/149, M–O 98/98, P–Se 128/130, Si–W 132/132 |
| Integration notu | — | `SearchEngineDataTests` (kestrel: True) iki test | aynı koşuda paralel Kestrel testlerinin 127.0.0.1:5000 çakışması ("address already in use"); sınıf tek başına 18/18 yeşil; T380'e ilgisiz, yerel ortam |
| Dal CI (8a) | ✓ | `gh run list --branch task/T380-islem-izi --limit 3` | 42346985169 success (head `1180a80`); önceki 42344210981 failure (düzeltme 1, iddiasız test — Düzeltme 2'de kapandı) |
| Restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; koşu sonrası `git status` temiz |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (diff'te sır yok; yalnız test fixture'ının parola özelliğine atıf)
- Auth etkisi: Temiz — yeni uç yok; yönetici panel şemasının (`SessionSchemes.AdminScheme`) kimliğinden ve hesap satırından okunur, yoksa yazım reddedilir
- Input validation: Temiz — `AuditEntryRules` uzunluk/sayı tavanı, hedef tür haritası, müşteri verisi yasağı; regex'lerde zaman aşımı var
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| — | — | — | — | Tur 2'de bulgu yok | — | — | — |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Kod biçimi (`^[A-Za-z][A-Za-z0-9]*(?:[._\-][A-Za-z0-9]+)*$`) tek bir ASCII sözcüğünü ("Ayse") kod sanar; boşluklu, Türkçe harfli, rakamlı ad/adres/telefon/not her yuvada reddedilir. `09 §9.2` m.4 "her adın değeri tek bir kapalı biçimdedir (kod…)" der ve test cümlesinin örnekleri ("Ayşe Yılmaz", adres, telefon, not) reddediliyor; biçim denetimi tek sözcüklü adı anlamsal olarak ayıramaz, çağıran task'lar kod yuvasına enum adı verir. Yapım raporu da ⚠ olarak yazmış. | Bu PR'da kapatıldı — kural metniyle uyumlu, kod değişikliği gerekmez |
| K2 | `RequestAuditActorSource`'un oturumlu yolu sınanmadı: test adı "ForARequestWithoutASession" der ama yalnız `HttpContext` olmayan yolu kurar; geçerli yönetici oturumundan kimlik/ad/e-postanın geçtiği yol ve hesabı kaldırılmış yöneticinin oturumu sınanmadı. Kimlik ayrıştırması mevcut `SessionSchemes.AccountId(ClaimsPrincipal)`'ı yeniden yazar (`09 §24` m.4). Bugün izi istekten çağıran üretim kodu yok. | DEFERRED_BACKLOG'a forward: D-176 (izi istekten ilk yazan panel task'ı; en geç F3 gate) |

#### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu (kriter ve KK sonuçları); 2 küçük fark
- Yapım raporunun ilk öz denetim tablosu `09 §25` satırında "yeni 09 metni yok" der; Düzeltme 1 `09 §9.2` m.4'e yuva cümlesi ekledi ve Düzeltme 1 bölümü bunu yazar (eski satır geçersiz kaldı, düzeltme bölümü doğru).
- K1'i yapım raporu Düzeltme 1'de ⚠ kolay karar olarak yazmış; K2 yapım raporunda yok. Öz denetim tabloları (ana, Düzeltme 1, Düzeltme 2) kriter ve KK başına kanıtlı. Sayılar main birleştirmeleriyle değişti (Unit 2908 → doğrulamada 2869 + Consistency 207; `AuditTrailTests` 22).

#### 16. adım kayıtları
- Dal `origin/main` (`268ab8d`, T304) ile birleştirildi; çakışmalar yalnız kayıt belgelerinde (MEMORY, DEFERRED_BACKLOG, IMPLEMENTATION_STATUS) — iki tarafın kayıtları korundu; PR #419 bu commit'le yeniden birleştirilebilir.
- Rapor (`T380_REPORT.md` tur 2 bölümü), status (`T380` ✓ PASS (tur 2); önceki iki "Son güncelleme" `STATUS_CHANGELOG.md`'ye), memory, backlog (D-176; aktif 97) commit ve push edildi. `12` `Durum` hücresi değişmedi.

#### Merge ve Post-merge
- Merge yok (yardımcının işi, 17–18. adım). Push sonrası dal CI'ının yeşil olması gerekir (birleştirme commit'i yeni bir koşu tetikler).
