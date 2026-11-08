# T338 — Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması

**Faz:** F3 | **Durum:** ✓ Doğrulama PASS (tur 1; merge yardımcıda) | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Saf kurallar tek evde — `CouponRules`:** `CheckDefinition` (yeni kuponun alanları: boş kod `FieldRequired`; yüzde 0 < x < 100 `InvalidPercentage` — `InvalidPercentageAttribute`; sabit tutar > 0 ve asgari sepet tutarı dolu ve tutardan büyük `CouponValueInvalid`; kuruş/para tipi taşması `InvalidAmount` — `InvalidAmountAttribute`; bitiş < başlangıç `InvalidDateRange`; adet < kullanılmış + ayrılmış (yeni kuponda 0) `CouponUsageBelowUsed`; bütün sorunlar birlikte döner), `Evaluate` (tarih aralığı `TurkeyTime.IsWithinRange` — başlangıç günü 00:00, bitiş günü bütün, Türkiye saati; hak dolu; asgari sepet altı ayrı sonuç; sabit ≥ sepet; yuvarlamayla sıfır), `RejectUnknown` (bilinmeyen/boş kod aynı ret — K-845), `PercentageShares` (`Money.Round`, kalem içinde, yarım yukarı), `AmountShares` (`Money.Allocate`: orantılı, aşağı, kalan kuruş en büyük kaleme; taşan kuruş sonraki büyük kaleme — D-482), `ReturnsRight` (yalnız bütün kalemlerin bütün adetleri kapanınca).
- **KDV paydan sonra:** her kalem `ItemPricing.Price(new ItemPriceInput(unitPrice, quantity, share, vatRate))` ile — T336'nın sabit hesap sırası (`02 §3.8.3`); ayrı KDV hesabı yazılmadı.
- **Okuma ve tanım — `Coupons`:** `CreateAsync` (alan sorunları + kimlik biçiminde kod tekilliği `DuplicateCouponCode`; kayıt çağıranın iş birimine eklenir, `SaveChanges` yok — `09 §9.1` madde 3), `EvaluateAsync` (düz okuma, `NormalizedCode` üzerinden harf ayrımsız; `I`/`İ`/`ı`/`i` tek harf — K-1016), `ApplyAsync` (L-6: `AttemptCounter` sırası Record → IsExceeded → Forget; yalnız `Rejected` sayılır).
- **Arayüz:** `ICatalogModule.CreateCouponAsync`, `EvaluateCouponAsync`, `ApplyCouponAsync`, `ReturnsCouponRight`; hakkın ayrılması/kesinleşmesi/dönüşü mevcut `ReserveAsync`/`CommitAsync`/`ReleaseAsync`/`ReturnAsync` (`StockCounters`, koşullu güncelleme) ile — yeni sayaç yazımı yok (`09 §10.2` madde 4).
- **Sözleşme türleri** (`Contracts/Coupons/`): `CouponDefinition`, `CouponProblem`, `CouponWriteResult`, `CouponLine`, `CouponApplication` (`BasketTotal` kupon öncesi eşik tabanı, `Items`, `Discount`, `PayableTotal`), `ItemClosure`; enum'lar `CouponKind`, `CouponOutcome` ayrı dosyada (`EnumSourceScan`).
- **Kayıt:** `CatalogModule` `AddScoped<Coupons>()`, `TryAddScoped<AttemptCounter>()` (uygulamanın kendi kaydı ikilenmez).
- **Backlog:** D-482 (sabit kuponda kalan kuruş taşması — belge düzeltmesi), D-483 (modül yöntemlerinin üretim çağıranları T348/T349/T509/T544/T367/T373).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/CouponRules.cs`, `Coupons.cs`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs`, `Contracts/Coupons/CouponRecords.cs`, `CouponKind.cs`, `CouponOutcome.cs`
- Testler: `tests/Shopfolio.UnitTests/Catalog/CouponRulesTests.cs`, `tests/Shopfolio.IntegrationTests/Model/CouponTests.cs`, `tests/Shopfolio.UnitTests/Infrastructure/SitemapSourceTests.cs` (`FakeCatalog` dört üye)
- Belgeler: `DEFERRED_BACKLOG.md` (D-482, D-483), `IMPLEMENTATION_STATUS.md` (T338 satırı), `STATUS_CHANGELOG.d/T338.md`, bu rapor

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kupon ödeme adımında girilen, sepet toplamına inen koddur | ✓ | `ICatalogModule.ApplyCouponAsync` (ödeme adımının alanı, sepet kimliğiyle); Integration `ThePaymentStep_AppliesTheCode_WithTheSharesOfEveryItem`; alan arayüzü T509/T544, çağıran D-483 |
| 2 | Yüzde ya da sabit tutar | ✓ | Unit `APercentageCoupon_AndAFixedAmountCoupon_AreBothDefinitionsThatSave`; Integration `APercentageCoupon_AndAFixedAmountCoupon_AreWritten_WithTheValueOfTheirKindOnly` |
| 3 | Pay kalem kalem; yüzde her kalemde, sabit orantılı, kalan kuruş en büyük kaleme | ✓ | Unit `AFixedAmount_IsSpreadInProportion_PartsRoundedDown_TheLeftOverKurusOnTheLargestItem` ([50,150,100] → 1,66/5,01/3,33; eşitlerde ilki), `APercentageCoupon_MakesEveryItemSmaller_ItsShareRoundedInTheItem_AHalfKurusUp`, `FixedAmountShares_AlwaysAddUpToTheAmount_AndNoShareIsNegativeOrPassesItsItem`; kenar durum D-482 |
| 4 | Pay kaleme donar, KDV paydan sonra | ✓ | Unit `TheAcceptedCoupon_GivesEveryItemItsShare_AndTheVatIsSeparatedAfterTheShare`; Integration `TheShareTheOrderItemFroze_StaysWhenTheCouponChanges_AndANewBasketGetsTheNewShare` (sipariş kalemine yazılan pay ve KDV kupon değişince değişmez); onaydaki yazım T348 (D-483) |
| 5 | Sepete eşit/aşan ve yuvarlamayla sıfıra indiren ret; asgari ve %100 kayıtta | ✓ | Unit `AFixedCouponEqualToOrAboveTheBasket_IsRejected`, `APercentageCoupon_ThatTheRoundingTakesToATotalOfZero_IsRejected`, `APercentageOutsideAboveZeroAndUnderOneHundred_IsTheInvalidPercentageMessage` (100 dahil), `AFixedAmountOfZero_OrAMinimumNotLargerThanIt_OrNoMinimum_IsTheCouponValueMessage` |
| 6 | Tarih aralığı ve toplam adet; kişiye özel kod = adet 1 | ✓ | Unit `TheRange_StartsAtMidnightOfTheStartDay_AndTheEndDayIsIncludedAsAWhole_InTurkeyTime`, `ACouponWhoseUsedAndReservedRightsFillItsTotal_IsRejected`, `AOneDayRange_AndAPersonalCodeWithACountOfOne_Save`; Integration `ACodeOutsideItsDates_OrWithItsRightsTaken_IsRefused`; düzenleme/adet alt sınırı T526 |
| 7 | Kod tekil, harf ayrımsız eşleşir; art arda geçersizde alan kapanır | ✓ | Integration `ACodeThatDiffersOnlyInLetterCase_IsNotWritten`, `TwoSavesThatRaceForOneCode_MeetTheUniqueIndex`, `TheCodeTyped_MatchesWithoutLetterCase_AndTheCodeAsDefinedComesBack`, `TheTurkishDottedAndDotlessI_AreOneLetter_WhenTheCodeIsMatched`, `InvalidCodesPastTheLimit_CloseTheCartsField_EvenForAValidCode_AndAnotherCartIsNotTouched`, `AnAcceptedCode_AndAValidCodeUnderItsMinimum_DoNotCountTowardTheLimit` |
| 8 | Hak onayda ayrılır, ödemede kullanılır, süre/başarısızlıkta döner; iptal/iadede yalnız tamamı kapanınca | ✓ | Integration `TwoConfirmationsOfTheLastRight_ReserveItOnce_TheSecondWaitsAndIsRefused`, `TheRight_IsReservedAtConfirmation_UsedAtPayment_AndGivenBackOnlyWhenTheWholeOrderCloses`, `AnUnpaidOrdersRight_ComesBack_WhenItIsReleased`; Unit `TheRight_ComesBack_OnlyWhenEveryUnitOfEveryItemIsClosed`, `TheRightsRule_RefusesAnOrderWithoutItems_AndAClosureOutsideTheUnits`; onay/süre ucu T348, iptal ucu T367 (D-483) |
| 9 | Kupon eşik ve asgari sipariş tabanını düşürmez | ✓ | `CouponApplication.BasketTotal` (kupon öncesi toplam); Unit `TheBasketTotal_TheThresholdsAreComparedWith_IsTheTotalBeforeTheCoupon`; karşılaştırma T349 KK2 (D-483) |

### Öz denetim (bitiş kapısı 2)

> Önceki işçinin (85bdd9e) yaptıkları dahil bütün maddeler yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 kupon kodu ödeme adımında uygulanır | ✓ | `ICatalogModule.cs:212` `ApplyCouponAsync`; `Coupons.cs:95`; `ThePaymentStep_AppliesTheCode_WithTheSharesOfEveryItem` |
| KK2 yüzde ve sabit iki tanım çalışır | ✓ | `CouponRules.cs:47` `CheckDefinition`; `Coupons.cs:36`; iki test (yukarıda) |
| KK3 kuruş artığı en büyük kaleme | ✓ | `CouponRules.cs:167` `AmountShares` (`Money.Allocate`); `AFixedAmount_IsSpreadInProportion_…` |
| KK4 pay donar, KDV paydan sonra | ✓ | `CouponRules.cs:96` (`ItemPricing.Price` share ile); `TheShareTheOrderItemFroze_…`, `TheAcceptedCoupon_…AndTheVatIsSeparatedAfterTheShare` |
| KK5 sepete eşit ve sıfıra indiren ret | ✓ | `AFixedCouponEqualToOrAboveTheBasket_IsRejected`, `APercentageCoupon_ThatTheRoundingTakesToATotalOfZero_IsRejected` |
| KK6 süresi ya da hakkı dolan ret | ✓ | `TheRange_StartsAtMidnight…`, `ACouponWhoseUsedAndReservedRightsFillItsTotal_IsRejected`, `ACodeOutsideItsDates_OrWithItsRightsTaken_IsRefused` |
| KK7 harf ayrımsız yinelenen kaydedilmez ve eşleşir; limit alanı kapatır | ✓ | `ACodeThatDiffersOnlyInLetterCase_IsNotWritten` (YAZ10/yaz10, İ/i), `TwoSavesThatRaceForOneCode_MeetTheUniqueIndex`, `InvalidCodesPastTheLimit_CloseTheCartsField_…` |
| KK8 son hakkı iki sipariş alamaz; yalnız tam iptal/iadede döner | ✓ | `TwoConfirmationsOfTheLastRight_ReserveItOnce_TheSecondWaitsAndIsRefused` (gerçek PostgreSQL, ikinci işlem kilitte bekler), `TheRight_IsReservedAtConfirmation_…GivenBackOnlyWhenTheWholeOrderCloses` |
| KK9 kuponlu sepet eşiği düşürmez | ✓ | `CouponRecords.cs` `BasketTotal`; `TheBasketTotal_TheThresholdsAreComparedWith_IsTheTotalBeforeTheCoupon` |
| `09 §25` doküman uyumu | ✓ | Okunanlar: çekirdek, `09` §10, §14, `02 §3.8.3`, §3.10.1–§3.10.7, §4.1.4, Z-22, §5.11.2–§5.11.4, §7.2.6, §7.4.8, §8.2 L-6, P-36, `10` KP-11/16/43, `06 §4.13`, §4.18; yeni ad: hata kodları `07 §6`'dakiler (`CouponRejected` vb. mevcut); kapsam dışı değişiklik yok; yeni ayar/süre/tavan yok (L-6 eşikleri `ProductConstants`'tan, `AttemptCounter` okur) |
| `09 §25` mimari | ✓ | Sayfa yok; arayüz yöntemi `SaveChanges`/işlem açmıyor (`Coupons` özet yorumu; `CreateAsync` yalnız `Add`); tek işlev: `Money.Round`/`Money.Allocate`/`ItemPricing.Price`/`TurkeyTime.IsWithinRange`/`InvalidPercentageAttribute`/`InvalidAmountAttribute`/`StockCounters` yeniden kullanıldı; zaman `TimeProvider`; mimari testler yeşil (Unit 4414) |
| `09 §25` durum/eşzamanlılık | ✓ | Hak sayaçları yalnız `StockCounters` koşullu güncellemesiyle; son hak yarışı gerçek PostgreSQL testinde |
| `09 §25` veri ve göç | uygulanamaz | Göç, yeni tablo, yeni enum değeri, yeni hata kodu yok (`Coupon` tablosu T105'te) |
| `09 §25` güvenlik | ✓ | Yeni deneme yüzeyi L-6 kapsamında (`Coupons.ApplyAsync`); bilinmeyen kod ve öteki retler aynı sonuç (sebep sızmaz — `EveryRefusal_CarriesNoCouponAndNoShare_SoTheReasonIsNotTold`); sır/kişisel veri yok; yeni POST/sayfa yok |
| `09 §25` test | ✓ | Kurallar Unit'te saf, veritabanına dayanan davranış (tekil dizin, kilit, L-6 satırları) gerçek PostgreSQL'de; saat `Fakes/ManualTimeProvider` (Integration'daki yerel `FixedTime` devam işçisince ortak sahteye çevrildi, CouponTests 19/19 yeniden yeşil); Unit saf işlevlere sabit an verir; test silinmedi/zayıflatılmadı |
| Sık FAIL listesi | ✓ | (1)(2) yeni bekçi/tarama yok; (3) göç yok; (4) sayı anan yer: backlog yeni satır, sayaç dokunulmadı (kayıt kuralı); (5) 09 metni değişmedi; tek kural evi attribute'ları kullanıldı; fiziksel ürün tohumu yok (kupon testleri kalem satırını doğrudan tohumlar) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror`; `dotnet format --verify-no-changes` | 0 uyarı, 0 hata; biçim temiz |
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` | 4414/4414 (CouponRulesTests dahil) |
| Integration | ✓ | `--filter-namespace` parçaları (yerel PostgreSQL 18, 127.0.0.1:55338) | Model 1229 (CouponTests 19 dahil); Catalog+Concurrency+CorporateContent+Database+FileSecrets+Files+Jobs+Logging+Security+SignedEndpoints+Disposal+Outages 502; Ordering+Pages 173; Web (Liveness hariç) 578; LivenessEndpointTests 15 — toplam 2497, hepsi geçti |
| Contract | uygulanamaz | — | Dış sözleşme yok |

Yerel notlar: Model parçasının ilk koşusu makine yüklüyken (7 dk 38 sn) 4 testte düştü; çıktı yalnız özetle alındığından adlar yok; aynı derlemeyle ikinci koşu 1229/1229 (3 dk). `LivenessEndpointTests` 15/15 geçti ama süreç bitmedi (D-391, bilinen).

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-08; head `564c86f` = `600df1d` + `origin/main` `4ff04bc` birleştirmesi) |
| Bulgu sayısı | 0 (S1/S2/S3 yok) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — ikisi de backlog'da (D-482, D-483); yeni kayıt açılmadı |

**Validator kanıtları (bağımsız koşu, yapım raporu Faz 3'e kadar açılmadı):**

| # | Kriter / KK | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 / KK1 | Ödeme adımında kod, sepet toplamına iner | ✓ | `Coupons.ApplyAsync` (sepet kimliği + L-6 → `EvaluateAsync`); Integration `ThePaymentStep_AppliesTheCode_WithTheSharesOfEveryItem` yeşil; sayfa ucu T509 (D-483) |
| 2 / KK2 | Yüzde ya da sabit | ✓ | `CouponRules.CheckDefinition` iki dal; `Coupons.CreateAsync` → `Coupon.CreatePercentage`/`CreateAmount`; Unit + Integration tanım testleri yeşil |
| 3 / KK3 | Kalem kalem pay, kalan kuruş en büyük kaleme | ✓ | `PercentageShares` = `Money.Round` kalem içinde; `AmountShares` = `Money.Allocate` (aşağı, kalan en büyük ağırlığa) + taşma dağıtımı (D-482); 1000 turluk özellik testi toplamı ve 0 ≤ pay ≤ tutar sınırını tutturuyor |
| 4 / KK4 | Pay donar, KDV paydan sonra | ✓ | `Evaluate` her kalemi `ItemPricing.Price(unitPrice, qty, share, vat)` ile kurar (`02 §3.8.3` sırası); 180×20/120 = 30,00 (paysız 33,33 değil); sipariş kalemine yazılan pay kupon %50'ye çekilince 10,00/15,00 kalıyor (Integration) |
| 5 / KK5 | Sepete eşit/aşan ve yuvarlamayla sıfır ret; asgari ve %100 kayıtta | ✓ | `amount >= basketTotal` → Rejected; `basketTotal - shares.Sum() <= 0` → Rejected (0,01×2 %50 ve %99,99); `InvalidPercentageAttribute` (0,100) açık aralık; sabitte asgari zorunlu ve > tutar (`CouponValueInvalid`) |
| 6 / KK6 | Tarih aralığı, toplam adet, kişiye özel = 1 | ✓ | `TurkeyTime.IsWithinRange` (başlangıç 00:00, bitiş günü bütün; UTC 21:00 sınırları sınandı); `UsedCount + ReservedCount >= TotalUsageLimit` → Rejected; Integration `ACodeOutsideItsDates_OrWithItsRightsTaken_IsRefused` |
| 7 / KK7 | Kod tekil, harf ayrımsız; deneme limiti alanı kapatır | ✓ | `NormalizeIdentity` + `NormalizedCode` tekil dizini; YAZ10/yaz10, İNDİRİM/indirim, KIŞ/kış yinelenmiyor, ındırım yarışı `UniqueViolation`; 5 geçersizden sonra geçerli kod da `AttemptLimitExceeded`, başka sepet etkilenmiyor (P-36 5/15 dk) |
| 8 / KK8 | Son hak tek siparişe; yalnız tam iptal/iadede döner | ✓ | `StockCounters.WriteCouponAsync` koşullu güncelleme + `LockKind.Coupon`; Integration'da ikinci işlem kilitte bekleyip `false` alıyor, sayaç used 0/reserved 1; Commit → used 1, Return → 0, Release → 0; `ReturnsRight` yalnız bütün adetler kapanınca `true` |
| 9 / KK9 | Kupon eşik tabanını düşürmez | ✓ | `CouponApplication.BasketTotal` kupon öncesi toplam (500,00 ↔ ödenecek 450,00); eşik karşılaştırması T349 (D-483) |
| `09 §25` | Review checklist | ✓ | Para yalnız `Money`/`ItemPricing`; zaman `TimeProvider`; arayüz yöntemi işlem açmıyor/kaydetmiyor; hata kodları `07 §6`'da (`DuplicateCouponCode`, `CouponValueInvalid`, `CouponUsageBelowUsed`, `InvalidPercentage`, `InvalidAmount`, `InvalidDateRange`, `FieldRequired`); göç/paket/ayar yok; mimari testler yeşil |
| Güvenlik | Tarama | ✓ | Sır yok (diff taraması); yeni deneme yüzeyi L-6 kapsamında; bilinmeyen/tarih dışı/hak dolu aynı ret (K-845); yeni POST/sayfa yok; yeni bağımlılık yok |

**Validator test koşusu:** `dotnet build Shopfolio.slnx --no-restore -warnaserror` 0 uyarı/0 hata; `dotnet format --verify-no-changes` üç proje temiz; Unit (Consistency hariç) 4204/4204, Consistency 242/242; betik testleri 4/4; Integration `CouponTests` 19/19, `StockAndCapacityTests`+`ConcurrencyTests`+`CategoryTreeAndListsTests`+`ProductTransitionsTests`+`JobQueueRegistrationTests`+`SchedulerRegistrationTests`+`AppCacheTests` 146/146, `PaymentWindowTests`+`ShipmentTransitionsTests` 109/109 (PostgreSQL 18, 127.0.0.1:55644). Dal CI run 42456195767 (head `564c86f`) bütün işler yeşil. Ana dal son 3 run (42452259880, 42452259999, 42450627496) success.

**Task satırları (`12 §7.5`):** `VAL-A036`–`A038`, `A196`–`A198` (Uç yanıtı + Veri kaydı; uç ödeme adımı ve panel kupon formu — T509, T526) ve `VAL-D008` (ödemesi beklenen sipariş ön koşulu — T348) `BEKLEMEDE` kalır; T338 modül düzeyini kurar. KP-16/KP-43'ün öteki satırları (D005–D007, B013, A191–A195) başka task'ların. `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

**Yapım raporu karşılaştırması:** tam uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Sayı farkı: yapım raporundaki Unit 4414 tek koşudur, validator Consistency'yi ayrı koştu (4204 + 242) — main birleştirmesi sonrası sayım; uyuşmazlık değil.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T338-kupon`
- **Commit:** `85bdd9e` — kod ve testler (1. işçi); rapor/kayıt commit'i bu dalda
- **PR:** #458
- **CI:** run 42456195767 (head `564c86f`) — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, CI Gate yeşil
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

Dış varsayım: yok (dış servis, paket, sürüm bilgisi kullanılmadı).

## ⚠ Kararlar (kolay, kurala en uygun seçenekle)

1. **L-6 sayımı:** yalnız `CouponRejected` sonuçları sayılır; asgari sepet altındaki geçerli kod ve kabul edilen kod sayılmaz (07'nin satırı yalnız `CouponRejected`'a "L-6'ya sayılır" der).
2. **Ret sırası:** bilinmeyen/tarih dışı/hak dolu → asgari sepet altı → sabit ≥ sepet → yuvarlamayla sıfır.
3. **Kalan kuruş taşması:** sığmayan kuruş sonraki büyük kaleme (D-482).
4. **Yeni kuponda adet alt sınırı 0** (kullanılmış + ayrılmış = 0), altı `CouponUsageBelowUsed`; kod uzunluk tavanı konmadı (belgede yok, sütun `text`).
5. **Kapsam:** yalnız yeni kupon tanımı; düzenleme, sürüm, adet alt sınırı ve silinmezlik T526 (K-654, K-806). KK4 "donma"nın onay yazımı T348; test sipariş kalemine yazılan payın kupon değişince değişmediğini gösterir.
6. **KK9:** eşik karşılaştırması T349'da `CouponApplication.BasketTotal` ile (D-483).
7. **KK8:** dönüş kararı `ReturnsCouponRight`; iptal/iade ucuna bağlanması T367/T373 (D-483).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sabit kuponda kalan kuruş taşmasının belgeye yazılması | D-482 | Belge düzeltmesi/karar; kod dar yorumu uygular |
| 2 | Modül yöntemlerinin üretim çağıranları (onay, eşik, ödeme adımı alanı, iptal/iade) | D-483 | Sahip task'lar T348, T349, T509, T544, T367, T373 |

## Notlar

- **Working tree (Adım -1):** temiz (1. işçi ve devam işçisi).
- **Ana dal CI startup (Adım 0):** 42450627470, 42450627496 (chore #450), 42446700473 (T336) success (1. işçi).
- **Repo memory (bitiş kapısı 9):** dokunulmadı (kayıt kuralı chore #450: MEMORY "Güncel Durum" yardımcının).
- **Diğer:** 1. işçi bağlam sınırında bölündü (85bdd9e); devam işçisi tam derleme/test koşusu, backlog, rapor, kayıt ve PR'ı yaptı. `12` VAL-A036/A037/A038, A196–A198, D008 satırlarının `Durum`'u değişmedi (uç yanıtı kanıtı sayfa task'larında).
