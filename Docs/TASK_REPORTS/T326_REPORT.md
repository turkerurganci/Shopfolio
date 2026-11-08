# T326 — Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

Bu task'ın süreleri büyük ölçüde kendi kural evlerinde (T337, T338, T322, T306, T215) zaten yaşıyordu; T326 eksik kalan tek kuralı ekledi ve geri kalanını zaman aşımı olarak sınadı. Yeni zamanlayıcı, iş türü, tablo, göç ya da paket yok.

- **Z-20 özet farkı kuralı** — `DiscountRules.IsSummaryDifference(terms, shownAt, confirmedAt)` (`Modules/Catalog/DiscountRules.cs`): indirimin durumu (yürüyor / yürümüyor) özetin gösterildiği an ile onay anı arasında değiştiyse onay bir özet farkıdır (`03 §4.1.23`, §1.10.1). İndirimi bitiren yine T337'nin `DiscountBoundarySource`'udur; fiyatın dönüşü `DiscountRules.AppliedPrice` ile bitiş anında (`EndsAt`, bitiş gününü izleyen günün 00:00'ı) sınandı. Onay işleminin kuralı çağırması T348'dedir (D-570).
- **Z-22** — kupon bitiş gününün 23:59:59'unda kabul edilir, bitişten sonraki 00:00'dan itibaren yeni siparişte kabul edilmez; onaylı siparişin kullanılmış ve ayrılmış hakkı süre dolunca yerinde kalır, ancak o siparişin kendi geçişleriyle (serbest bırakma) değişir. Kural evi `CouponRules.Evaluate` (T338), değiştirilmedi.
- **Z-23** — süresi dolan duyuru bitişi izleyen günün 00:00'ında kendiliğinden görünmez (`ContentVisibility.Decide`, T322); kayıt yayımlanmış kalır, elle kaldırma gerekmez. Ziyaretçi okuyucusu T360'tadır (D-571).
- **Zamanlayıcı yok taraması** — `tests/Shopfolio.UnitTests/Architecture/ExpiryTimerScanTests.cs`: (1) hiçbir `JobType` adı kupon, duyuru, referans penceresi, yayın, KVKK/ihlal, yeniden deneme, kilit açma sözcüklerini anmaz; (2) yansımayla bulunan her zamanlayıcı kaynağı, kesinti etkisi, gecelik adım ve iş işleyicisi kupon, duyuru, referans fiyat kuralı, deneme tabloları/sayacı ve yeniden deneme sürelerini okumaz; (3) Katalog ve Kurumsal İçerik modüllerinde ileri tarihli yayın anı (alan/ad) yok (Z-24); (4) KVKK cevabı ve ihlal bildirimi için kayıt/sayaç/son tarih yok (Z-25, Z-37); (5) yeniden deneme süreleri (P-38) yalnız `DispatchRetrySchedule`, `ProductConstants`, `ParameterInventory`'de (Z-26: bu task'ta ikinci deneme evi yok); (6) `AttemptRecords` yalnız sayaç, bağlam ve süreyle imha parçasında; (7) hesap satırlarında (`Customer`, `Admin`) kilit/engel alanı yok (Z-27); desen örnek testi yakaladığını ve yakalamadığını sınar.
- **Z-27 kayan pencere** — `AttemptCounterTests.The_window_slides_with_every_attempt_and_the_block_opens_by_itself_when_the_count_falls_to_the_threshold_T326_KK8` (gerçek PostgreSQL, sahte saat): eşik aşılınca engel, ilk satır pencereden çıkınca (satır silinmeden, kimse açmadan) engelin açılması, yeni denemeyle yeniden kapanması, tüm satırlar çıkınca temiz.
- **Backlog** — yeni D-570 (özet farkı kuralı, T348), D-571 (duyuru okuyucusu, T360), D-572 (sayfa düzeyi engel mesajı, T377) — `Docs/DEFERRED_BACKLOG.d/T326.md`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/DiscountRules.cs` (`IsSummaryDifference`)
- `tests/Shopfolio.UnitTests/Catalog/DiscountRulesTests.cs`, `CorporateContent/ContentVisibilityTests.cs`, `Architecture/ExpiryTimerScanTests.cs` (yeni)
- `tests/Shopfolio.IntegrationTests/Model/CouponTests.cs`, `Security/AttemptCounterTests.cs`
- Kayıt: `Docs/IMPLEMENTATION_STATUS.md` (T326 satırı), `Docs/STATUS_CHANGELOG.d/T326.md`, `Docs/DEFERRED_BACKLOG.d/T326.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Z-20 bitişinde sistem indirimi bitirir; onay anında bitiş özet farkıdır | ✓ | `DiscountBoundarySource` (T337, `DiscountAndReferenceTests.TheScheduler_OpensTheDiscount...EvenWhenLate`); `DiscountRules.IsSummaryDifference`; `DiscountRulesTests.TheAppliedPrice_RevertsToTheOwnPrice_AtTheExactMomentTheRangeEnds_Z20_KK1`, `ADiscountThatEndsBetweenTheSummaryAndTheConfirmation_IsASummaryDifference_Z20_KK1`, `ADiscountThatStartsBetween..._AndAnUnchangedStateIsNot_Z20_KK1` |
| 2 | Z-21 hesap penceresidir; dolması işlem başlatmaz | ✓ | `ExpiryTimerScanTests.NoJobType_...`, `NoScheduledSource_..._ReadsTheseRulesOrTables` (`ReferencePriceRules` hiçbir zamanlayıcıda yok); pencere okuma anında hesap (`ReferencePriceRules`, T337) |
| 3 | Z-22 dolunca kupon yeni siparişte kabul edilmez; ayrılmış haklar kalır | ✓ | `CouponTests.AfterTheEndDay_ANewOrderIsRefused_AndTheRightsOfConfirmedOrdersStayWhereTheyWere_T326_KK3` (`EndOfRange` − 1 tick kabul, `EndOfRange` ve +40 gün ret; kullanılmış 1 ve ayrılmış 1 hak aynı, sürüm 1); `CouponRulesTests.TheRange_StartsAtMidnightOfTheStartDay_...` |
| 4 | Z-23 dolunca duyuru görünmez; elle kaldırma gerekmez | ✓ | `ContentVisibilityTests.AnAnnouncement_IsGoneFromTheSite_AtMidnightAfterItsEndDay_WithoutAnyoneRemovingIt_Z23_KK4` (yayın durumu Published kalır); okuyucu D-571 |
| 5 | Z-24 için ileri tarihli yayın yoktur | ✓ | `ExpiryTimerScanTests.NothingIsPublishedOnADate_...KK5` (Catalog + CorporateContent'te `Publish(At\|On\|Date\|From\|After\|Start)`/`ScheduledPublish` adı yok); `JobType` adı taraması |
| 6 | Z-25 ve Z-37 için sayaç ve iş yok | ✓ | `ExpiryTimerScanTests.TheKvkkReplyAndTheBreachNotice_HaveNoRecordCounterOrDeadline_KK6`; `JobType` adı taraması (`Kvkk`, `Breach`) |
| 7 | Z-26 için bu task'ta deneme kodu/zamanlayıcı yok; davranış T385'te | ✓ | `ExpiryTimerScanTests.TheEmailRetryTimes_AreReadOnlyByTheDispatchSchedule_AndTheProductConstantsTable_KK7` (üç dosya, tam eşitlik); bu PR'ın diff'inde Notifications altında değişiklik yok |
| 8 | Z-27 kayan pencerede sayar; eşik altında engel kendiliğinden açılır, hesap kilitlenmez | ✓ | `AttemptCounterTests.The_window_slides_...T326_KK8`; `ExpiryTimerScanTests.TheAttemptRecords_AreWrittenAndReadOnlyByTheCounter_...KK8`, `NoAccountRowCarriesALock_...KK8`; mevcut `A_row_exactly_one_window_old_counts_...` |

### Öz denetim (bitiş kapısı 2)

> Diff `origin/main...HEAD`; kabul kriterleri 1–8 (`11` T326), KK1–KK8, `09 §25` ve `sik-fail-nedenleri.md` maddeleri tek tek denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| K1 / KK1: biten indirimin fiyatı eski haline döner | ✓ | `DiscountRules.AppliedPrice` bitiş anında kendi fiyatına döner; `DiscountRulesTests...RevertsToTheOwnPrice_..._KK1`; zamanlayıcı yolu T337 Integration'da (fiyat geçmişine bitiş satırı) |
| K2 / KK2: referans penceresi dolunca iş çalışmaz | ✓ | `ExpiryTimerScanTests` (iş türü adı ve zamanlayıcı kaynağı taraması) |
| K3 / KK3: biten kupon yeni siparişte reddedilir, eski hak kalır | ✓ | `CouponTests...T326_KK3` |
| K4 / KK4: biten duyuru sitede yok | ✓ (kural düzeyi) | `ContentVisibilityTests...Z23_KK4`; sayfa düzeyi D-571 (okuyucu yok) |
| K5 / KK5: ileri tarihli yayın tablosu ve işi yok | ✓ | `ExpiryTimerScanTests.NothingIsPublishedOnADate_...` |
| K6 / KK6: Z-25 ve Z-37 için zamanlayıcı yok | ✓ | `ExpiryTimerScanTests.TheKvkkReplyAndTheBreachNotice_...` |
| K7 / KK7: Z-26 için bu task'ta kod yok | ✓ | `ExpiryTimerScanTests.TheEmailRetryTimes_...`; değişen dosyalar listesinde Notifications yok |
| K8 / KK8: engel pencere dolunca açılır, hesap kilitli değil | ✓ | `AttemptCounterTests.The_window_slides_...`; `NoAccountRowCarriesALock_...` |
| `09 §25` — doküman uyumu / adlar | ✓ | yeni ad yalnız `IsSummaryDifference` (`Modules/Catalog`, kural evi `DiscountRules`); mevcut `DiscountState`, `TurkeyTime` kullanıldı |
| `09 §25` — tek işlev | ✓ | Z-22, Z-23, Z-27, Z-26 için ikinci kural yazılmadı; `AfterSalesPeriodRules` gibi var olan evler çağrıldı/sınandı, yeniden yazılmadı |
| `09 §25` — zaman yalnız `TimeProvider`'dan | ✓ | kural an alır (`shownAt`, `confirmedAt`); testler `ManualTimeProvider`/sabit anlar |
| `09 §25` — modül sınırı | ✓ | `DiscountRules` Catalog içinde; tarama Katalog/İçerik/Notifications'a yalnız dosya okur |
| `09 §25` — durum/iş/olay, veri ve göç | ✓ (uygulanamaz) | geçiş, iş, tablo, göç, değer eklenmedi |
| Güvenlik taraması | ✓ | sır/kişisel veri yok; yeni uç, form, yükleme, dış çağrı, paket yok; test veritabanı parolası yalnız yerel ortam değişkeninde, dosyada yok |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | izinli dosya listeleri tam eşitlik (`Assert.Equal` sıralı üç dosya); tarama desenleri ad kalıbıyla sınırlı, örnek testle yakalanan/yakalanmayan gösterilir |
| Sık FAIL (2) kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | tarama sınıf bildirimi + alan adı + tablo + kural sınıfı adlarını (`Coupons`, `CouponRules`, `AttemptRecords`, `DispatchRetrySchedule`, ...) tek desende arar; set/tip/atama biçimleri örnek testte |
| Sık FAIL (3) DB default / FK dizini | ✓ (uygulanamaz) | şema değişmedi |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | sayı/etiket değişmedi; test sayıları sabitlerden (eşik/pencere `AttemptLimits.Of`'tan) |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | belge değişmedi |
| Sık FAIL (45) projenin tek kural evi yerine ikinci kural | ✓ | `AfterSalesPeriodRules` gibi var olan evlere dokunulmadı; yeni kural `DiscountRules`'ta |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release` (Architecture + Consistency dahil) | 4768 / 4768 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-class "*AttemptCounterTests" --filter-class "*CouponTests"` (PostgreSQL 18, ayrı konteyner) | 35 / 35 |
| Contract | — | | Uç değişmedi |
| Build | ✓ | `dotnet build ... -c Release -warnaserror` (Unit ve Integration projeleri) | 0 uyarı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T326-zaman-asimi-katalog`
- **Commit:** *(PR açılınca)*
- **PR:** *(PR açılınca)*
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(merge'ü yapan yardımcı doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan tier, kota, paket sürümü, dış API ya da ortam kısıtı içermez).

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Özet farkı kuralının onay işleminde çağrılması | D-570 | Sipariş onayı T348'in işi |
| 2 | Süresi dolan duyurunun ziyaretçi okuyucusunda sınanması | D-571 | Okuyucu T360'ın işi |
| 3 | Kayan pencere engelinin sayfa/uç düzeyinde sınanması | D-572 | Giriş, sıfırlama, kayıt sayfaları T377'nin işi |

## Notlar

- Working tree: temiz. Hook kurulumu: `core.hooksPath` = `scripts/git-hooks`.
- Ana dal son 3 run: 42483789393, 42483789437, 42480365166 — hepsi success.
- ⚠ (kolay, kurala en uygun seçenekle karar): task planı kupon (Z-22), duyuru (Z-23) ve engel penceresi (Z-27) için yeni davranış istemez; bu davranışlar T338, T322 ve T215'te kuruldu. T326 bunları süre sonu açısından sınadı, eksik tek kuralı (özet farkı) ekledi. Sayfa/uç düzeyi kanıtlar henüz var olmayan okuyuculara bağlı olduğu için D-570–D-572 olarak hedef task'lara yazıldı.
- `Docs/IMPLEMENTATION_STATUS.md` "Son güncelleme" satırı ve MEMORY'ye dokunulmadı.
