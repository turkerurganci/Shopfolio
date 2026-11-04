# T227 — Zaman ve para

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; merge yardımcıda) | **Tarih:** 2026-11-04

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Tek zaman kaynağı:** `Infrastructure/TimeSource.AddTimeSource()` `TimeProvider.System`'i bir kez tekil kaydeder; `Program.cs` çağırır. Testte kayıt `ManualTimeProvider` ile değiştirilir (`09 §7.1` madde 1, 4; `05 §7.2.4`).
- **Türkiye saati:** `Infrastructure/TurkeyTime` (statik): sabit `Offset` +03:00, `DateOf(an)`, `StartOf(gün)` (ofseti sıfır UTC anı), gün süresi yardımcıları `LastDayOfPeriod`, `EndOfPeriod` (= son günün ertesi gününün `StartOf`'u), `IsWithinPeriod`. Sunucu dilimi ve saat dilimi veritabanı okunmaz (`09 §7.1` madde 3; `05 §7.2.1`, §7.2.2).
- **İş günü takvimi:** `Infrastructure/BusinessCalendar` + gömülü kaynak `PublicHolidays.txt` (2026 ve 2027 resmî tatilleri; satır `yyyy-MM-dd ad`). Kapsam = satırı olan yıl; kapsam dışı yılda yalnız hafta sonu dışlanır. Biçim, çift ve sıra hatası satır numaralı `FormatException` (`05 §7.2.3`; `02 §4.1.2`).
- **Para:** `Infrastructure/Money` (`public static`): `Round` (en yakın kuruş, `MidpointRounding.AwayFromZero`; negatif girdi `ArgumentOutOfRangeException`) ve `Allocate` (parçalar kuruşa aşağı, kalan kuruş en büyük ağırlığa, eşitte ilki; sıfır/negatif ağırlık, boş liste, negatif ya da ikiden fazla ondalıklı toplam reddedilir) (`09 §14.1` madde 1; `05 §7.2.6`; `02 §3.8.3`).
- **Mimari test (derlenmiş kod, ArchUnitNET + yansıma):** `Architecture/TimeAndMoneyRulesTests` — yasak saat okumaları ve sunucu dilimini örtük kullanan dönüşümler, `TimeProvider.System`'in `TimeSource` dışında anılması, `DateTime`/`DateTime?` türlü alan–özellik–parametre–dönüş, `Money` dışında yuvarlama (`Math`/`MathF`/`decimal`/`double`/`float` `Round`/`Floor`/`Ceiling`/`Truncate` ve `MidpointRounding` alan her çağrı), `Modules` ve `Money`'de `double`/`float`. İhlal örnekleri ayrı derlemede (`TimeAndMoneyViolationSample.dll`, `ViolationSamples/TimeAndMoney/*.cs`).
- **SQL saat taraması:** `Architecture/SqlClockSourceScan(+Tests)` göç dışı `src/` kaynağında `now()`, `current_timestamp`, `clock_timestamp()` vb.; ihlal örneği `ViolationSamples/SqlClock/SqlClockQuery.cs` (salt okunur metin, derlenmez).
- **Öz denetimde eklenenler (devam işçisi):** yuvarlama kuralı `double`/`float` ve `MidpointRounding` alan çağrıları da kapsar (genel matematik `T.Round` örnekli); `09 §7.1` madde 2'nin `DateTime` yasağı için test + örnek (`LocalTimeRecord`).
- **Doküman:** `09 §7.1` madde 5 ve §14.1 (kalıp yorumu + madde 5) "T227'de sınandı" notları; `.claude/CONTEXT.md` §3 Infrastructure ve UnitTests satırları; `DEFERRED_BACKLOG` D-74, D-75 (başlık sayacı 33 → 42, gerçek açık satır sayısına eşitlendi).
- **Önceki task kaydı (skill adım 15):** T218 status Commit hücresi `d82b943`; T218 raporu "Post-merge run'lar" satırı dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/TimeSource.cs`, `TurkeyTime.cs`, `BusinessCalendar.cs`, `PublicHolidays.txt`, `Money.cs`; `src/Shopfolio.Web/Program.cs`; `src/Shopfolio.Web/Shopfolio.Web.csproj` (EmbeddedResource)
- `tests/Shopfolio.UnitTests/Infrastructure/MoneyTests.cs`, `TurkeyTimeTests.cs`, `BusinessCalendarTests.cs`, `TimeSourceTests.cs`
- `tests/Shopfolio.UnitTests/Architecture/TimeAndMoneyRulesTests.cs`, `SqlClockSourceScan.cs`, `SqlClockSourceScanTests.cs`, `ViolationSamples/TimeAndMoney/TimeViolations.cs`, `MoneyViolations.cs`, `ViolationSamples/SqlClock/SqlClockQuery.cs`; `tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` (ikinci Csc hedefi)
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T218_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`
- Modül klasörlerine, varlıklara, göçlere ve paketlere dokunulmadı.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Zaman UTC'de saat dilimli damga; sayım ve gösterim Türkiye saatiyle | ✓ | `TurkeyTime.StartOf` ofseti sıfır an döner (`TurkeyTimeTests.StartOf_Day_IsTurkeyMidnightAsUtcInstant`); `DateOf_*`, `StartOf_AndDateOf_TurnAnInstantToTurkeyTimeAndBack`; `DateTime` imzası yasak (`Application_DeclaresNoDateTimeFieldPropertyParameterOrReturn` + örnek); an sütunu `timestamptz` (F1, `ColumnTypeConsistencyTests`) |
| 2 | Tek zaman kaynağı; testte değiştirilir | ✓ | `TimeSourceTests.AddTimeSource_RegistersTheSystemClockOnceAsSingleton`, `TimeSource_ReplacedInATest_GivesTheFakeClocksInstant`; `SystemClock_InApplication_IsNamedOnlyByTimeSource`; `Application_ReadsTimeOnlyThroughTimeProvider` |
| 3 | Gün süresi ertesi gün başlar, son günün 23:59'unda dolar | ✓ | `LastDayOfPeriod_StartDayIsNotCounted`, `IsWithinPeriod_StartDay_IsNotCounted`, `IsWithinPeriod_LastMomentOfLastDay_IsInsideAndNextMidnightIsOutside`, `IsWithinPeriod_LastMinuteAfter2359_IsStillInside`, `EndOfPeriod_IsTheNextDaysTurkeyMidnight` |
| 4 | İş günü takvimi ürünle gelen dosya; kapsam dışı yılda yalnız hafta sonu | ✓ | `BusinessCalendarTests.Product_LoadsTheShippedHolidayFile` (gömülü kaynak), `IsBusinessDay_YearOutsideTheList_ExcludesOnlyTheWeekend`, `IsBusinessDay_EveryDayOfAnUncoveredYear_IsBusinessExactlyOnWeekdays` |
| 5 | Para ondalıklı; kayan noktalı tür yok | ✓ | `Money` yalnız `decimal`; `ModulesAndMoney_InApplication_HaveNoFloatingPointType` + `FloatingPointUsers_OnViolationSample_CatchTheModuleType` |
| 6 | Tek kuruş yuvarlama işlevi: en yakın kuruş, tam yarım yukarı | ✓ | `MoneyTests.Round_Amount_GoesToNearestKurusWithExactHalfUp` (12,345→12,35; 0,005→0,01; 0,004→0,00; 1.000.000,005→1.000.000,01), `Round_ExactHalf_DoesNotGoToTheEvenDigit`; tek yer: `Rounding_InApplication_IsOnlyInMoney` + örnekler |
| 7 | Orantılı bölme: parçalar aşağı, kalan en büyük parçaya, toplam tutara eşit | ✓ | `Allocate_HundredInThreeEqualWeights_GivesTheLeftOverKurusToTheFirst` (33,34+33,33+33,33), `Allocate_LeftOverKurus_GoesToTheLargestWeightWhereverItIs`, `Allocate_EqualLargestWeights_GivesTheLeftOverToTheFirstOfThem`, özellik testi `Allocate_RandomTotalsAndWeights_KeepsTheSumAndTheRoundingDownRule` (1000 tur, tohum 20261104) |

## Öz Denetim (PR öncesi; her madde kendi diff'inde yeniden denetlendi)

| Madde | Sonuç | Kanıt |
|---|---|---|
| k1/KK1: an +03:00 ile Türkiye saatine ve geri doğru çevrilir; sunucu dilimi değişince sonuç değişmez | ✓ | `DateOf_SameInstantWrittenWithAnyOffset_IsTheSameTurkeyDay` (ofset 0, 3, −8, 14), `StartOf_AndDateOf_TurnAnInstantToTurkeyTimeAndBack`; `TurkeyTime` yalnız `ToOffset`/`DateTimeOffset(DateTime, TimeSpan)` kullanır, sunucu dilimine bağlı her okuma `Application_ReadsTimeOnlyThroughTimeProvider` ile yasak; yerelde Turkey Standard Time (+03:00) makinede geçti, CI UTC'de koşar |
| k2/KK2: sahte saatle süre testi | ✓ | `TurkeyTimeTests.PeriodTest_WithFakeClock_MovesTimeWithoutWaiting` (`ManualTimeProvider.Advance`/`SetUtcNow`), `TimeSource_ReplacedInATest_GivesTheFakeClocksInstant` |
| k3/KK3: son günün son anı içeride, ertesi gün 00:00 dışarıda; yazılı 23:59 yok | ✓ | `IsWithinPeriod_LastMomentOfLastDay_IsInsideAndNextMidnightIsOutside` (bir tik), `IsWithinPeriod_LastMinuteAfter2359_IsStillInside`; `grep -rn "23:59\|59, 59" src/` → yalnız `TurkeyTime.cs` yorumu ("no clock time such as 23:59 is compared") |
| k4/KK4: kapsam dışı yılda hafta sonu dışında tatil yok | ✓ | `IsBusinessDay_YearOutsideTheList_ExcludesOnlyTheWeekend` (2030), `IsBusinessDay_EveryDayOfAnUncoveredYear_IsBusinessExactlyOnWeekdays` (2031 tüm günler); dosya gömülü, veritabanında ve panelde değil |
| k5/KK5: kodda `double`/`float` para yok | ✓ | `ModulesAndMoney_InApplication_HaveNoFloatingPointType`; ihlal `FloatingPrice` (alan, özellik, parametre, dönüş) |
| k6/KK6: 0,005 yukarı, 0,004 aşağı | ✓ | `Round_Amount_GoesToNearestKurusWithExactHalfUp` satırları; yuvarlama tek yerde: `Rounding_InApplication_IsOnlyInMoney`, örnekte `MoneyHelper` yakalanır, `Money` muaf (sınıf tam adıyla, ad alanı değil) |
| k7/KK7: toplam tutara eşit; parçalar aşağı; kalan en büyükte | ✓ | `Allocate_*` testleri ve özellik testi (toplam, negatif yok, iki ondalık, en büyük dışındaki parça aşağı yuvarlanmış paya birebir eşit, fark ≤ n−1 kuruş) |
| `09 §7.1` m.1, m.5: yasak okuma listesi mimari test, ayrı derlemede ihlal örneği | ✓ | `ForbiddenTimeCalls_OnViolationSample_CatchTheRead` (15 üye), lambda/asenkron/statik başlatıcı testi; SQL: `SqlClockSourceScanTests` (örnekte 3 bulgu, yorum ve `@now` parametresi bulgu değil); göçlerin taraması `MigrationSourceScan`'de (değişmedi) |
| `09 §7.1` m.2: `DateTime` yok | ✓ | `Application_DeclaresNoDateTimeFieldPropertyParameterOrReturn`; ihlal `DateTimeSignatures_OnViolationSample_CatchEachPlace` (`LocalTimeRecord`: özellik, alan, parametre, kurucu parametresi, dönüş; `DateTimeOffset` `StoredAt` bulgu değil) |
| `09 §14.1` m.5: `Math.Round(` … `MidpointRounding` yalnız `Money`'de | ✓ | `Rounding_OnViolationSample_IsCaughtOutsideMoney` (8 üye, `double.Round` dahil), `Rounding_OnViolationSample_CatchesAnyCallThatTakesMidpointRounding` (`T.Round` genel matematik) |
| 09 §25 — doküman uyumu ve kapsam | ✓ | Okunan: 09 §1, §2, §3.1, §3.2, §5, §6, §7, §14, §19, §23–§25; 05 §7.2, MK-19; 02 §3.8.3, §4.1; GUARDRAILS §8. Adlar 09'dan (`TimeProvider`, `TurkeyTime.DateOf/StartOf`, `Money.Round/Allocate`, `ManualTimeProvider`); "bilgim tarihli" iki olgu (ArchUnitNET çağrı denetimi, `AwayFromZero`) sınandı ve 09'a yazıldı; yeni ayar/sabit yok (§3.5 değişmez) |
| 09 §25 — mimari ve kod | ✓ | Yeni dosyalar `Infrastructure/` ve `UnitTests/Architecture|Infrastructure/`'da; `CONTEXT.md` §3 güncel; modül sınırı testleri yeşil; zaman yalnız `TimeProvider`; para yalnız `Money`; yeni paket yok |
| 09 §25 — test | ✓ | Her yeni kuralın ayrı ihlal örneği var; birim testleri dosya yazmaz (`BusinessCalendar.Parse` metin alır; taramalar repo dosyasını yalnız okur; `grep -n "Write\|CreateTemp\|Delete"` yeni testlerde 0); sahte `UnitTests/Fakes/ManualTimeProvider`; gerçek saat yok (`TimeProvider.System` yalnız `AddTimeSource` testinde örnek kimliği için karşılaştırılır, okunmaz) |
| 09 §25 — veri/göç, durum, performans | uygulanamaz | Göç, varlık, geçiş, önbellek, sayfa yok |
| Sık FAIL nedenleri | ✓ | Kural genişleyince aynı listeyi anan yerler: 09 §7.1 m.5 ve §14.1 m.5 notları, test sınıf yorumu birlikte güncellendi; istisnalar sınıf tam adıyla (`TimeSource`, `Money`; benzer adlı `TimeSourceCopy`/`MoneyHelper` yakalanır); DB varsayılanı/FK uygulanamaz; her kural ayrı örnekli; dosyaya yazan test yok; varlık eklenmedi |
| Güvenlik taraması | ✓ | Diff'te sır, parola, token, bağlantı dizisi yok; yetki ve girdi doğrulama yüzeyi yok; gömülü kaynak salt okunur; yeni bağımlılık yok (ArchUnitNET 0.13.5 mevcut; `System.Numerics` çatıdan) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 458 geçti, 0 kaldı (Consistency dahil; `--filter-trait "Consistency=true"` 101/101) |
| Biçim ve derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` (Debug ve Release); `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; format çıkış 0 |
| Integration | — | koşulmadı | Test beklentisi yalnız Unit; veritabanına dokunan değişiklik yok |
| Contract | — | yok | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, bağımsız doğrulayıcı; dal head `cdc6648`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-76, K2 → D-77 |

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Zaman UTC'de saat dilimli damga; sayım ve gösterim Türkiye saatiyle | ✓ | `TurkeyTime.StartOf` ofseti sıfır an döner, `DateOf` yalnız `ToOffset(+03:00)` kullanır (kod okundu); `TurkeyTimeTests` yeşil; mutasyon: `DateOf` → `instant.LocalDateTime` yapılınca +03:00 makinede davranış testleri geçer, yalnız `Application_ReadsTimeOnlyThroughTimeProvider` kırmızı — sunucu dilimine bağlılığı mimari test yakalar; mutasyon: modüle `DateTime` özellik → `Application_DeclaresNoDateTimeFieldPropertyParameterOrReturn` kırmızı |
| 2 | Tek zaman kaynağı; testte değiştirilir | ✓ | `TimeSource.AddTimeSource` tek tekil kayıt, `Program.cs` çağırır; `src/`'de `TimeProvider.System` yalnız `TimeSource.cs`'te (grep); mutasyon: modülde `TimeProvider.System` ve özellik getter'ında `DateTimeOffset.UtcNow` → `SystemClock_InApplication_IsNamedOnlyByTimeSource` ve `Application_ReadsTimeOnlyThroughTimeProvider` kırmızı (getter da yakalanır) |
| 3 | Gün süresi ertesi gün başlar, son günün 23:59'unda dolar | ✓ | `EndOfPeriod = StartOf(start + n + 1)`, `09 §7.1` madde 3'ün formülüyle birebir; mutasyon: `EndOfPeriod` bir gün kısaltılınca üç `TurkeyTimeTests` kırmızı (`EndOfPeriod_IsTheNextDaysTurkeyMidnight`, `IsWithinPeriod_LastMinuteAfter2359_IsStillInside`, `PeriodTest_WithFakeClock_MovesTimeWithoutWaiting`); `src/`'de yazılı `23:59` yalnız yorumda (grep) |
| 4 | İş günü takvimi ürünle gelen dosya; kapsam dışı yılda yalnız hafta sonu | ✓ | `PublicHolidays.txt` gömülü kaynak (`EmbeddedResource`, `LogicalName`); 2026–2027 satırları `02 §4.1.2` ile uyumlu (arife ve 28 Ekim listede yok); mutasyon: Mart tatilleri yok sayılınca `IsBusinessDay_WeekdayHolidayAndWeekend_InACoveredYear` kırmızı |
| 5 | Para ondalıklı; kayan noktalı tür yok | ✓ | `src/**/*.cs`'de `double`/`float`/`Double`/`Single` 0 (grep); mutasyon: modülde yalnız yöntem gövdesinde `double` yerel değişken → `ModulesAndMoney_InApplication_HaveNoFloatingPointType` kırmızı |
| 6 | Tek kuruş yuvarlama işlevi: en yakın kuruş, tam yarım yukarı | ✓ | Validator sondası (geçici test, silindi): 0,015 → 0,02; 0,025 → 0,03; 2,675 → 2,68; 0,0049999999 → 0,00; 0,0050000001 → 0,01; 99.999.999,995 → 100.000.000,00; 9.999.999.999,994 → 9.999.999.999,99; `VAL-A793` örneği 9,595 → 9,60; `VAL-A794` örneği 4,995 → 5,00 — hepsi geçti; mutasyon: `Math.Round(x, 2)` (tam yarım çifte) → dört tablo satırı (12,345; 12,365; 0,005; 1.000.000,005) ve `Round_ExactHalf_DoesNotGoToTheEvenDigit` kırmızı; mutasyon: modülde `decimal.Round` → `Rounding_InApplication_IsOnlyInMoney` kırmızı |
| 7 | Orantılı bölme: parçalar aşağı, kalan en büyük parçaya, toplam tutara eşit | ✓ | Validator sondası: `VAL-A795` örneği 10 TL 50:100:150 → 1,66 + 3,33 + 5,01; 0,01 ve 0,02 TL üç eşit ağırlık → kalanın tamamı ilk parçaya; 1:1,0001:1 → kalan en büyük ağırlığa; 0,1:0,2:0,3 → 0,16 + 0,33 + 0,51; 9.999.999.999,99 TL yedi ve üç ağırlıkta toplam birebir, parçalar iki ondalıklı; ağırlık 1/3 (28 hane) → 33,34 + 33,33 + 33,33; mutasyon: kalan son parçaya yazılınca üç `Allocate` testi kırmızı |

KK1–KK7: KK1 ✓ (gidiş-dönüş testi; yerel +03:00 ve CI UTC'de yeşil; mutasyon kanıtı kriter 1'de) · KK2 ✓ (`PeriodTest_WithFakeClock_MovesTimeWithoutWaiting`, `ManualTimeProvider.Advance`/`SetUtcNow`) · KK3 ✓ (bir tik önce içeride, ertesi gün 00:00 dışarıda; yazılı 23:59 yok) · KK4 ✓ (2030 ve 2031 bütün günleri) · KK5 ✓ · KK6 ✓ · KK7 ✓. `12` satırları: task kaynak birimleriyle (`05` MK-19, §7.2; `09` §7, §14) grep `VAL-A801` ve `VAL-D060`'ı getirir; ikisi indirim, kupon ve duyurunun davranışıdır (T337, T378 k7) ve T227'nin kriterine bağlı değildir — `Durum` `BEKLEMEDE` kalır.

Testler (validator, yerel, head `cdc6648`): build `-warnaserror` 0 uyarı · `dotnet format --verify-no-changes` 0 · Unit 458/458 (Consistency dahil) · `restore --locked-mode` 0 · dal CI 41889663878 success (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E advisory, Gate). Integration bu task'ta beklenmiyor, koşulmadı. Mutasyonlar ve sonda dosyası geri alındı; `git status` temiz.

K-notları: **K1** `Money.Round`/`Allocate` eksi işaretli sıfırı (`-0.00m`) negatif sayıp istisna atar → D-76 (T336). **K2** `DateTime` imza taraması `DateTime[]`, `List<DateTime>` gibi öğe ve genel argüman türlerini görmez → D-77 (T378).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** `PublicHolidays.txt` uygulama derlemesine gömülü kaynak (`LogicalName` `Shopfolio.Infrastructure.PublicHolidays.txt`)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T227-zaman-ve-para`
- **Commit:** `873796c` — T227: Zaman kaynağı, Türkiye saati, iş günü takvimi ve kuruş yuvarlaması; düzeltme ve rapor commit'leri ardından (head SHA PR açıklamasında)
- **PR:** #382
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | 2026 ve 2027 dinî bayram tarihleri (Diyanet takvimi): 2026 Ramazan 20–22 Mart, Kurban 27–30 Mayıs; 2027 Ramazan 9–11 Mart, Kurban 16–19 Mayıs (19 Mayıs millî bayramla çakışık, tek satır) | Önceki işçi Diyanet takvimini web'den doğruladı (2026-11-04); sabit millî tatiller 2429 sayılı Kanun (1 Ocak, 23 Nisan, 1 Mayıs, 19 Mayıs, 15 Temmuz, 30 Ağustos, 29 Ekim) — `Product_EveryCoveredYear_HasTheFixedNationalHolidays` | ✓ |
| 2 | Yarım günler (arife, 28 Ekim) tam iş günüdür | `02 §4.1.2` ("arife tam iş günüdür"); `Product_HalfDays_AreBusinessDays` | ✓ |
| 3 | ArchUnitNET 0.13.5 yöntem çağrısını IL'den görür (özellik getter'ı, lambda, asenkron, statik başlatıcı) | `ForbiddenTimeCalls_OnViolationSample_CatchReadsInLambdaAsyncMethodAndFieldInitializer` yeşil; `Directory.Packages.props` `TngTech.ArchUnitNET` 0.13.5 | ✓ |
| 4 | `MidpointRounding.AwayFromZero` pozitif `decimal`'de tam yarımı yukarı götürür | `Round_ExactHalf_DoesNotGoToTheEvenDigit`, 12,355→12,36, 12,365→12,37 | ✓ |
| 5 | Türkiye sabit +03:00, yaz saati yok | `02 §4.1.1`; `09 §7.1` madde 3 | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `Money.VatOf` → T336, `Money.ShareOfUnits` → T369 | D-74 | `09 §14.1` m.1'in dört işlevinden ikisi; ilk hesaplayan task'larla gelir (kalem KDV'si, işlem adet payı) |
| 2 | Tatil listesi 2028+ satırları; iş günü sayımı (son iş günü, N iş günü sonrası) | D-75 (T378 k3, k9) | Liste bakım sürümüyle yenilenir (`05 §8.8`); iş günü sayımı T378'in kapsamı |
| 3 | İstekteki tarih ve tutar biçimi (`RequestFormat`, `09 §7.2`) → T216 k2–k4; sağlayıcı tutar dönüşümü (`PaymentAmounts`, `09 §14.2`) → T401 k4 | — (hedef task'ın kabul kriterinde) | T227'nin kriterlerinde yok; hedef task'lar kendi kriterinde taşır |

## Notlar

- **Working tree (Adım -1):** temiz (önceki işçi, başlangıçta); devam işçisi kendi worktree'si temizken dala geçti. `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** önceki işçi: 41868842172, 41868842265, 41858025821 success. Devamda yeniden: 41884524385 (CI, `d82b943`) success, 41884524541 (Publish, `d82b943`) success, 41868842172 success.
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **⚠ Kararlar (sahibe sorulmadan, kurala en uygun seçenekle):**
  - (a) `VatOf`/`ShareOfUnits` eklenmedi — T336/T369'un (D-74).
  - (b) `RequestFormat` T216'nın, `PaymentAmounts` T401'in.
  - (c) `double`/`float` yasağı `Modules` ad alanının bütün türlerine uygulandı (para alanı ayırt edilemiyor; 09 §14.1 m.5 notu).
  - (d) Yasak listesi 09'un listesinden geniş: sunucu dilimini örtük kullanan dönüşümler ve bütün `TimeZoneInfo` üyeleri eklendi (09 §7.1 m.3 "sunucunun saat dilimi sonucu etkilemez"in sınaması; 09 §7.1 m.5 notu).
  - (e) Tatil dosyası biçimi `yyyy-MM-dd ad`, kapsam = satırı olan yıl, yalnız 2026–2027; 28 Ekim ve arifeler iş günü (D-75).
  - (f) `TurkeyTime`'a gün süresi yardımcıları (`LastDayOfPeriod`, `EndOfPeriod`, `IsWithinPeriod`) eklendi — k3'ün tek hesap yeri; iş günü sayımı T378'e kaldı.
  - (g) Zaman ve yuvarlama kuralı kaynak taraması yerine derlenmiş kod üzerinden (09 §7.1 m.5 izin verir; 09'a "T227'de sınandı" notu).
  - (h) Devam işçisi: yuvarlama kuralına `double`/`float` ve `MidpointRounding` alan çağrılar, `09 §7.1` m.2'ye `DateTime` imzası testi eklendi (09 notları güncel).
- **Diğer:** Devam işçisi önceki işçinin worktree'sine yazamadığından (harness yalıtımı) aynı dalı kendi worktree'sinde açtı (`git switch --ignore-other-worktrees`); önceki worktree `873796c`'de kaldı.
