# T378 — Zaman sayımı: tek saat dilimi, iş günü ve resmî tatil listesi, takvim ve iş günü sayımı, gün sonu, tarih aralığı, çitler

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `BusinessCalendar.LastDayOfBusinessPeriod`: iş günüyle verilen sürenin son günü (başlangıç günü sayılmaz, N. iş günü son gündür).
- `PeriodCounting` (yeni): `CountingOf` (yalnız kargoya verme ve havale ödeme süresi iş günü, öteki sekiz süre takvim günü), `LastDay`, `EndOf`, `IsWithin` (sürenin sonu son günden sonraki günün 00:00'ı; `StartOf`/`DateOf` üzerinden), `SpanEnd` (dakika/saat süresi başladığı andan işler).
- `TurkeyTime.EndOfRange` ve `IsWithinRange` (tarih aralığı: başlangıç gününün 00:00'ı, bitiş günü dahil).
- `PeriodFences` (yeni): üç çitli sürenin denetimi; sayılar `ParameterFences`'ten (tek tanım); karar yönü ve çit değerini taşır. `ParameterFences.ShippingPeriodLower = 0` eklendi.
- `DayCounting`, `CountedPeriod`, `FenceSide` enum'ları (`Infrastructure/`, kayıtlı depolanan enum değil).
- D-77 kapatıldı: `TimeAndMoneyRulesTests.DateTimeSignatures` dizi, genel tür argümanı ve demet öğesini de açar; ihlal örneğine satırlar eklendi; `09 §7.1` madde 5 notu hizalandı.
- D-75: iş günü sayımı ve kapsam dışı yıl kuralı sınandı; kalan iş yalnız 2028 satırları (hedef değişmedi).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/`: `PeriodCounting.cs`, `PeriodFences.cs`, `DayCounting.cs`, `CountedPeriod.cs`, `FenceSide.cs`, `BusinessCalendar.cs`, `TurkeyTime.cs`, `ParameterFences.cs`
- `tests/Shopfolio.UnitTests/Infrastructure/`: `PeriodCountingTests.cs`, `PeriodFencesTests.cs`, `TurkeyDateRangeTests.cs`, `SingleTimeZoneTests.cs`
- `tests/Shopfolio.UnitTests/Architecture/`: `TimeAndMoneyRulesTests.cs`, `ViolationSamples/TimeAndMoney/TimeViolations.cs`
- `Docs/09_CODING_GUIDELINES.md` (§7.1 madde 5 test tarifi), `Docs/DEFERRED_BACKLOG.md` (D-75, D-77), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek saat dilimi, yaz saati yok | ✓ | `TurkeyTime.Offset` sabit +03:00 (T227); `SingleTimeZoneTests` |
| 2 | İş günü pzt–cum, tatil hariç; arife tam gün; firma kapalı günü yok | ✓ | `BusinessCalendar.IsBusinessDay`; `PeriodCountingTests.LastDay_*`; kapalı gün kavramı kodda yok |
| 3 | Tatil listesi ürünle gelir; liste dışı yılda yalnız hafta sonu | ✓ | `PublicHolidays.txt` gömülü kaynak; `LastDay_YearOutsideTheList_SkipsOnlyTheWeekend`; `BusinessCalendarTests` (T227) |
| 4 | Yalnız kargoya verme ve havale iş günü; yasal süreler takvim günü | ✓ | `PeriodCounting.CountingOf`; `CountingOf_*`; `LastDay_LegalPeriodOverAHoliday_CountsTheHolidayDays` |
| 5 | Gün süresi ertesi gün başlar, son günün sonunda dolar; saat bazlı sayım yok | ✓ | `PeriodCounting.EndOf/IsWithin`; `IsWithin_*` testleri; `Source_PeriodCountingFiles_*` (23:59 yok) |
| 6 | Dakika/saat/oturum süreleri gün sayımına girmez | ✓ | `PeriodCounting.SpanEnd`; `SpanEnd_*` testleri |
| 7 | Tarih aralığı başlangıç 00.00, bitiş günü dahil | ✓ | `TurkeyTime.IsWithinRange/EndOfRange`; `TurkeyDateRangeTests` |
| 8 | Üç süre çitli, üst çit yasal otuzdan geriye, çit dışı sebebiyle reddedilir | ✓ | `PeriodFences`; `PeriodFencesTests` (13 iş günü ≤ 26 takvim günü, 2026–2027 listesi üzerinde). Panelde uygulama ve mesaj T381/ayar sayfası görevi; burada karar + sebep verilir |
| 9 | Sayım `TimeProvider` + `TurkeyTime`'dan; ikinci yardımcı yok | ✓ | `PeriodCounting` anı parametre alır, saat okumaz; `SingleTimeZoneTests.Source_*`; `TimeAndMoneyRulesTests` yeşil |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: yaz saati ve ikinci dilim yok | ✓ | `SingleTimeZoneTests.StartOf_EveryDayOf...`, `TurkeyTime_HasNoZoneChoice...`, `Source_OnlyTurkeyTimeDefinesTheOffset...` |
| KK2: hafta sonu, tatil, arife | ✓ | `PeriodCountingTests.LastDay_BusinessPeriodOverAWeekend/OverAHoliday/TheEveOfAHoliday` |
| KK3: liste dışı yılda yalnız hafta sonu | ✓ | `LastDay_YearOutsideTheList_SkipsOnlyTheWeekend` (2030, 1 Ocak ve 23 Nisan iş günü) |
| KK4: iki süre iş günü, gerisi takvim günü | ✓ | `CountingOf_EachPeriod_IsAsTheRuleSays` (10 süre), `CountingOf_EveryPeriodOfTheList_IsDecidedAndOnlyTwo...`, `CountingOf_ValueOutsideTheList_IsRefused` |
| KK5: sahte saatle son günün son anı içinde, ertesi 00:00 dışında | ✓ | `IsWithin_CalendarPeriod_...`, `IsWithin_BusinessPeriod_...` (`ManualTimeProvider`, bir tik) |
| KK6: saatli süre gün sayımında yok | ✓ | `SpanEnd_HourSpanStartedLateInTheDay...` (23:30 + 1 saat = ertesi 00:30) |
| KK7: aralığın iki ucu | ✓ | `TurkeyDateRangeTests` (başlangıç 00:00 bir tik önce/sonra, bitiş günü dahil ve ertesi 00:00 dışarıda, tek günlük aralık) |
| KK8: çit dışı değer reddedilir | ✓ | `PeriodFencesTests.CheckShippingPeriod/CheckBankTransferPaymentWindow/CheckFulfilmentPeriod` |
| KK9: sayım `TurkeyTime`'dan geçer | ✓ | `EndOf_CalendarPeriod_IsTheSameAsTheTurkeyTimeHelper`; `Source_PeriodCountingFiles_CountThroughTurkeyTimeAndComparesNoClockTime` |
| `09 §25` zaman satırı | ✓ | Yeni kodda `DateTime.Now/UtcNow`, `TimeZoneInfo` yok; `TimeAndMoneyRulesTests` 34/34 |
| Güvenlik taraması | ✓ | Dış girdi, sır, auth, yeni bağımlılık yok; saf işlevler |
| Sık FAIL (4): aynı sayıyı anan yerler | ✓ | Çit sayıları yalnız `ParameterFences`'te (yeni kopya yok); `09 §7.1` madde 5 notu güncellendi; D-75/D-77 işlendi |
| Sık FAIL (5): 09 metni karar kaydısız | ✓ | `09` değişikliği yalnız mevcut testin kapsamını anlatır, kural (madde 2) değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build --filter-namespace Shopfolio.UnitTests.Infrastructure` ve `--filter-not-namespace` (Consistency dahil) | 966 + 1910 = 2876, 0 hata |
| Integration | — | Test beklentisi yalnız Unit; şema/sayfa değişmedi | koşulmadı |
| Contract | — | — | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `33e4d94`) |
| Bulgu sayısı | 0 (S1/S2/S3 yok) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 bu PR'da kapatıldı (`TimeAndMoneyRulesTests`: `DateTimeSignatures`'ın özet yorumu araya giren `HoldsDateTime`'ın üstünde kalmıştı; yerine taşındı, `HoldsDateTime`'a kendi özeti yazıldı); K2 → D-161 (P-5 alt çiti 0 çit içi, ama iş günü sayımı `days < 1`'de istisna atar; sıfırın anlamı T325'te) |

**Validator kanıtları (bağımsız koşu, 2026-11-07):** başlangıç kapıları temiz (working tree boş; `core.hooksPath` ana checkout'un `scripts/git-hooks`'u; main son üç run 42335449709, 42335449717, 42306192052 `success`; memory'de T378 satırı var). `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı/0 hata; `dotnet format --verify-no-changes` Web ve UnitTests çıkış 0; Unit (Consistency hariç) 2669/2669, Consistency 207/207, betik testleri yeşil; K1 düzeltmesi sonrası `TimeAndMoneyRulesTests` 34/34; `dotnet restore --locked-mode` 0. Integration: dal integration testine dokunmuyor, dal CI 42337007822 (head `33e4d94`) dokuz işin hepsi `success`. Kriterler kod okunarak ve testlerle eşlendi: k1/KK1 `TurkeyTime.Offset` tek sabit, 2026–2027 her gün 24 saat (`SingleTimeZoneTests`), `TimeZoneInfo`/ikinci ofset kaynak taramasında yok (git grep ile de teyit); k2/KK2 tatil dosyasında arife yok, 19 Mart ve 28 Ekim 2026 iş günü; k3/KK3 2030 kapsam dışı, 1 Ocak ve 23 Nisan iş günü; k4/KK4 on sürenin yalnız ikisi iş günü (enum'un tamamı üzerinde); k5/KK5 `ManualTimeProvider` ile son günün son tiki içeride, ertesi 00:00 dışarıda (takvim ve iş günü); k6/KK6 `SpanEnd` an + süre; k7/KK7 `IsWithinRange` iki ucu ve tek günlük aralık; k8/KK8 `PeriodFences` P-5/P-7/P-44 değerleri `02 §11.1` ile birebir, panelde uygulama T381 k6'nın; 13 iş günü listede en çok 26 takvim günü; k9/KK9 `PeriodCounting` saat okumaz, `TurkeyTime.StartOf/DateOf`'tan geçer. `12`: task'ın KP kaynağı yok (`11 §7.1` kaynakları `02 §4.1.x`, `09 §7`); `02 §4.1`'e atıf veren tek satır `VAL-A801` (indirim/kupon/duyuru aralığı, uçtan uca) tüketici task'larda doğrulanır, `Durum` `BEKLEMEDE` kalır. Yapım raporuyla uyum tam; öz denetim tablosu kriter ve KK başına kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T378-zaman-sayimi`
- **Commit:** `f707331` — T378: Zaman sayımı: iş günü ve takvim günü sayımı, tarih aralığı, çitler
- **PR:** #416
- **CI:** dal CI 42337007822 (head `33e4d94`) ✓ yeşil
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (saf kod; paket, plan, servis, API ya da ortam varsayımı yok). Tek veri kaynağı `PublicHolidays.txt` (T227, 2026–2027 kapsar).

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dört–beş günlük bayram dahil 13 iş günü ≤ 26 takvim günü | `PeriodFencesTests.UpperFences_AtTheirLongest_...` 2026–2027 listesinde geçti | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | 2028 ve sonrası tatil satırları | D-75 | Bakım sürümü işi; en geç 2027 sonu |
| 2 | Çit dışı değerin panelde alan mesajı olarak gösterilmesi ve ayar sayfası | T381 / panel görevleri | Ayar katmanı T381'in kapsamı; `PeriodFences` karar ve çit değerini verir |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** 42335449709 success, 42335449717 success, 42306192052 success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Sıfır iş günü:** `P-5` alt çiti 0'dır (kargoya verme süresi 0 geçerli ayar); `LastDayOfBusinessPeriod` ve `PeriodCounting` `days < 1` için hata fırlatır (`TurkeyTime.LastDayOfPeriod` ile aynı). 0 değerinin sipariş sözüne nasıl çevrileceği (süre sözü yok) tüketici task'ın kararıdır; burada uydurulmadı.
- **⚠ Yasal süre sayıları** (14 gün, 30 gün vb.) bu task'ta kodlanmadı; `CountedPeriod` yalnız sayım biçimini söyler, değerler `02 §11` envanterinin (T381) işidir.
