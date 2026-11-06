# T217 — Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İşlemin tek kuralı (`Infrastructure/Operations/`):** `IOperationRule<TContext>` (kayıt durumundan `OperationAvailability` üretir; saat, veritabanı, istek okumaz), `OperationAvailability` (açık mı, açık adet, varsayılan, üst sınır, son gün), `OperationDecision` + `OperationGate.Decide` (saf kapı: `Closed`, `QuantityExceedsOpen`; adet üst sınırı aşarsa bütünüyle red).
- **Ret cevabı (`OperationRefusals`):** müşteri sınıfı 303 + `OperationClosed`/`QuantityExceedsOpen` bildirimi; öteki sınıf 422 (`OperationNotAvailable`/`QuantityExceedsOpen`, eylem düzeyinde mesaj); her redde `RejectionLog` satırı (müşterinin "kapalı"sı satırda `OperationNotAvailable`).
- **İşaret ve tarama:** `GatedOperationAttribute` + `OperationSourceScan` (işaretli işleyici kapıya ulaşmalı; işaret GET'te kırmızı; görünüm ve betik durum adı/tarih hesabı taşımaz) + ayrı ihlal örnekleri `ViolationSamples/Operations/`.
- **İşlem sırası (`Infrastructure/Operations/OperationRunner`; düzeltme 1):** `ITransactionRunner` → satır kilidi ve (panelde) sürüm (`IOrderingModule.LockOrderAsync`, Contracts) → durum yükleme → kural → yazım; ret dönüş değeridir ve işlem geri alınır. İlk yazımda `Modules/Ordering/GatedOrderOperation` idi; sayfa `Contracts` dışı türe başvuramayacağından taşındı.
- **Örnek sayfa (yalnız test sunucusu):** `Pages/OperationSamples.cs` (`SampleCancelRule` 03 §1.11.2, `SampleWithdrawRule` 03 §1.11.5), `Pages/Panel/OperationSample.cshtml(.cs)`.
- **Devam turunda:** birim testleri derlendi ve geçti (erişilebilirlik: test sınıfı public, kapı testinin satır verisi `bool`; tarama test verisi `MemberData` — `ForbiddenTestPatternScan` satır içi `) {` kalıbını test gövdesi sanıyordu); tam Unit, tam Integration, Release `-warnaserror`, `dotnet format` koşuldu.
- **Doküman:** `09 §8.3` (yeni), `§8` girişi, içindekiler satırı, `§8.2` m.3 yardımcı cümlesi, `§25` satırı; `CONTEXT.md`, `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `MEMORY.md`, `DEFERRED_BACKLOG.md` (D-153), T216 kayıtları.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Operations/` (7 yeni dosya), `src/Shopfolio.Web/Modules/Ordering/{OrderingModule.cs, Contracts/IOrderingModule.cs}; (düzeltme 1: `Infrastructure/Operations/OperationRunner.cs`, `Program.cs` kaydı, `GatedOrderOperation.cs` silindi)`
- `tests/Shopfolio.UnitTests/`: `Infrastructure/OperationGateTests.cs`, `Architecture/{OperationSourceScan.cs, OperationSourceScanTests.cs}`, `Architecture/ViolationSamples/Operations/{GatedHandlers.cs, Decides.cshtml, decides.js}`
- `tests/Shopfolio.IntegrationTests/`: `Pages/OperationSamples.cs`, `Pages/Panel/OperationSample.cshtml(.cs)`, `Web/OperationPatternTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T216_REPORT.md` (post-merge satırı), `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | GET işlemleri sunucu kararından üretir; düğme yanıtın içinde | ✓ | `OperationPatternTests.Get_AnOrderBeingPrepared_ShowsTheCancelFormOnly…_KK1_KK6`, `Get_AClosedCancel_HasNoCancelButton_KK1` |
| 2 | Tarayıcı durumdan işlem türetmez | ✓ | `OperationSourceScan` `ViewDecides`/`ScriptDecides`; `OperationSourceScanTests.ScanClient_*`, `TheSamplePage_MarksItsPostHandlers_…AndItsViewDecidesNothing` |
| 3 | Her POST aynı kuralı yeniden denetler; satır kilidi altında, o anki hâle karşı | ✓ | `OperationRunner.RunAsync` (kilit+sürüm → yükle → kapı → yaz); düzeltme 1: `ModuleBoundaryTests.OperationPatternPage_UnderThePageRule_…` (sayfa ad alanında kalıp temiz); `Post_AnOpenOperation_IsAppliedUnderTheLock…_KK3`; tarama `GatedWithoutCheck` |
| 4 | Gizleme/etkisiz gösterme denetimin yerine geçmez | ✓ | `Post_ByHandForAClosedOperation_WhoseButtonWasNeverShown_IsNotApplied…_KK3_KK4_KK5` |
| 5 | Açık değilse uygulanmaz, hiçbir şey yazılmaz; yanıt sınıfa göre | ✓ | müşteri 303: `Post_ByHand…`/`Post_AQuantityAbove…`; panel 422/409: `Post_ThePanelsOperation_WithTheVersionEqual…Is422…409`; `OperationGateTests`; `09 §8.3` m.4 |
| 6 | Sunucunun türettiği değerler sayfada (açık adet, varsayılan, üst sınır, son gün) | ✓ | `Get_AnOrderBeingPrepared…` (`max=`/`value=`), `Get_TheWithdrawalOnItsLastDay…_KK6_KK7` |
| 7 | Açık aralık 03 §1.11'in aralığıdır | ✓ (örnek kuralla) | `SampleCancelRule` 1.11.2, `SampleWithdrawRule` 1.11.5; son gün açık/ertesi gün kapalı testi. ⚠ ürünün gerçek kuralları F3–F5'te (`09 §8.3` yazımın kararı 1; D-153) |
| 8 | Kural tek yerde; GET ve POST aynı işlevi çağırır | ✓ | `TheGetAndThePost_CallTheSameRuleInstance_KK8`; `IOperationRule` tek arayüz |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 kapalı işlemin düğmesi yanıtta yok | ✓ | `Get_AClosedCancel_HasNoCancelButton_KK1` (durum × ödeme teorisi) |
| KK2 betik karar vermez | ✓ | `OperationSourceScanTests.ScanClient_OfAScriptThatDecides_IsAFinding` + `ViolationSamples/Operations/decides.js`; sayfa temiz testi |
| KK3 elle gönderilen kapalı POST uygulanmaz | ✓ | `Post_ByHand…_KK3_KK4_KK5` |
| KK4 gizli düğmenin POST'u da reddedilir | ✓ | aynı test (düğme hiç gösterilmemiş sipariş) |
| KK5 reddedilen işlemde veritabanı değişmez | ✓ | `Post_ByHand…`, `Post_AQuantityAbove…`, `Post_ThePanelsOperation_…_AndWritesNothing_KK5` (sipariş sürümü dahil); ret `RunAsync` içinde dönüş değeridir, işlem geri alınır (`GatedOrderOperation`) |
| KK6 açık adet ve üst sınır sunucudan | ✓ | `Get_AnOrderBeingPrepared…_KK1_KK6` (beklenen değer siparişin kalem adedinden türetilir) |
| KK7 aralığın son günü sınırda | ✓ | `Get_TheWithdrawalOnItsLastDay…_KK6_KK7`, `Post_TheWithdrawalOnTheLastDayIsApplied…_KK7` |
| KK8 GET ve POST aynı kural | ✓ | `TheGetAndThePost_CallTheSameRuleInstance_KK8` |
| `09 §25` ilgili satır | ✓ | `§25` yeni satır → `§8.3`; ayrıca `§8`, `§19.2` m.3 ihlal örnekleri |
| Güvenlik taraması (yetkilendirme/yeniden denetim) | ✓ | kapı sunucuda; istemci değeri karar vermez (`Input.*` yalnız adet, kapı üst sınırla karşılaştırır) |
| Sık FAIL 1 — bekçi/tarama istisnası geniş | ✓ | yorum, `""` ve `@""` içindeki ad kapı sayılmaz (`ScanHandlers_OfACheckThatIsOnlyInACommentAStringOrTooFarAway_IsAFinding`); zincir en çok dört halka (beşinci bulgu) |
| Sık FAIL 2 — tarama çağrı biçimi kaçırır | ✓ | işaret: `[GatedOperation]`, `…Attribute`, ad alanı nitelikli, listede başka nitelikle, `()` ile, çok satırlı imza; kapı: boşluklu `OperationGate . Decide`, nitelikli, `=>` gövde, `GatedOrderOperation` (`…InEverySpellingOfTheMark`, `…ReachesTheGate…IsClean`) |
| Sık FAIL 3 — sayı/etiket çoğaltma | ✓ | `09 §8` giriş cümlesi, içindekiler satırı (`§8.3`), `§8.2` m.3 yardımcı cümlesi, `§25`, `CONTEXT`, backlog sayacı (92 → 93), status ve changelog güncellendi |
| Sık FAIL 4 — test beklentisi elle yazılı | ✓ | statü/üye listeleri `Enum.GetNames`'ten; bildirim adresi `NoticeCode`'dan; adet beklentisi kalem adedinden. `OperationGateTests` küçük tam sayı kural girdileridir (kuralın kendi değeri) |
| Sık FAIL 5 — modül arayüz yöntemi kendi işlemini açıyor | ✓ | `RunAsync`'i yalnız `OperationRunner` açar ve o giriş noktasının (sayfa işleyicisi) yardımcısıdır, `Contracts` metodu değil (`09 §9.1` m.3); `09 §8.3` m.3 yazar |

> `? Doğrulanamadı` **FAIL değildir**; bu turda doğrulanamayan madde yok. Kestrel notu: kapı sayfa mantığıdır, sunucu sınırı (gövde/üst bilgi) değildir; bu yüzden TestServer yeterlidir.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release` (Consistency ayrı) | düzeltme 1 sonrası 2636, başarısız 0 (öncesi 2634) (yeni: `OperationGateTests`, `OperationSourceScanTests` — 65 test satırı) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests -c Release` (Postgres 18 konteyneri) | 1695, başarısız 0, atlanan 0 — validator düzeltmesi: yapım 1595 yazmıştı; CI 42236914259 `total: 1695` ve doğrulayıcının parça koşuları 508 + 182 + 112 + 482 + 411 = 1695 = T216'nın 1682'si + 13 (yeni: `OperationPatternTests` 13) |
| Contract | — | — | bu task'ta yok |
| Derleme/biçim | ✓ | `dotnet build -c Release -warnaserror`; `dotnet format --verify-no-changes` | 0 uyarı, değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-06, head `3cb1380`) |
| Bulgu sayısı | 1 (B1 ORTA, S1) |
| Düzeltme gerekli mi | Evet — B1 |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-154, K2 bu PR'da kapatıldı) |

### Doğrulama tur 1 — ✗ FAIL

**B1 (ORTA, S1; kriter 3, 8; KK3, KK8; `09 §25` "kod hiçbir doküman cümlesiyle çelişmiyor mu"):** kalıp gerçek bir sayfada kullanılamıyor. `09 §8.3` madde 3 `GatedOrderOperation`'ı "giriş noktasının (sayfa işleyicisinin) işlem sahipliği" yapar; ama sınıf `Shopfolio.Modules.Ordering` ad alanındadır (`Contracts` dışı) ve `09 §6` madde 2 ve 7 sayfanın modülün `Contracts` dışı türüne başvurmasını ve onu enjekte etmesini yasaklar. Doğrulayıcı sondası: `src/Shopfolio.Web/Pages/Panel/` altına `GatedOrderOperation`'ı enjekte edip `RunAsync` çağıran geçici bir sayfa modeli eklenince `ModuleBoundaryTests.PagesAndEndpoints_InApplicationAssembly_TouchNeitherModuleInternalsNorTheContext` kırmızı ("ValidatorProbeT217Model does …"); sonda silindi. Örnek sayfa test derlemesinde (`Shopfolio.IntegrationTests.Pages`) durduğu için sınır testinin kapsamı dışındadır ve çelişkiyi gizler — örnek sayfa `AppDbContext`'e ve varlıklara da doğrudan dokunur. Aynı boşluk GET tarafında da var: ürünün kuralları modülün iç türü olursa sayfa GET'te kural örneğine `Contracts` dışından ulaşamaz; `09 §8.3` kuralın ve kapılı sipariş işleminin sayfaya hangi `Contracts` yüzeyiyle ulaştığını yazmıyor. Beklenen: kalıp `09 §6`'ya uyan bir yolla sunulur (ör. sipariş işlemini ve kural değerlendirmesini `Ordering.Contracts` arayüzünden veren bir yüzey, işlemi sayfanın `ITransactionRunner`'ı açar; `09 §6` madde 3 ve `§9.1` madde 3 ile), `09 §8.3` madde 1 ve 3 bunu yazar ve örnek sayfa ya da bir mimari test gerçek `Shopfolio.Pages` ad alanındaki kullanımın sınır testinden geçtiğini gösterir.

Kriter 1, 2, 4–7 ve KK1, KK2, KK4–KK7 ✓ (kriter 7 örnek kuralla; gerçek kurallar D-153). Doğrulayıcı ayrıca sınadı: üç reddedilen POST (müşteri 303, panel 422, panel 409) sonrası **bütün tabloların** içerik özeti değişmedi; Türkiye gece yarısı sınırında (20:59:59Z açık, 21:00:00Z kapalı) son gün doğru davranıyor (geçici sondalar, silindi). Unit 2634, Integration 1695 geçti, 0 atlanan; dal CI 42236914259 yeşil. K1: tarama darlıkları → D-154. K2: Integration sayısı 1595 → 1695 (rapor, durum, hafıza) bu PR'da düzeltildi. `12` satırlarının `Durum`'u değişmedi (T217'nin KP'si yok; `07 §8.1`'i anan VAL-A578, VAL-A716, VAL-B097, VAL-B117 ekran task'larınındır).

## Düzeltme 1 (doğrulama tur 1 sonrası)

**B1 çözümü.** Kalıbın sayfaya açılan yüzü `09 §6` ile uyumlu hâle getirildi:

- `GatedOrderOperation` / `GatedOrderResult` (Modules/Ordering, `Contracts` dışı) **silindi**. Yerine ortak altyapıda `Infrastructure/Operations/OperationRunner` + `GatedOperationResult` (modül türüne başvurmaz; `Program.cs`'te scoped kayıt). Sayfa işlemi `ITransactionRunner` ile bu çalıştırıcı üzerinden açar.
- Sipariş kilidi ve sürüm karşılaştırması modülün **`Contracts` yöntemi**: `IOrderingModule.LockOrderAsync(orderId, shownVersion, ct)` (kilidi `OrderLocks` ile alır, sürüm uymazsa `false`; çağıranın işleminde çalışır, kendi işlemini açmaz). `load` ve `apply` da modülün `Contracts` yöntemleri olur (gerçek ekranlarda; örnekte `SampleOrderOperations` taklidi).
- GET ve POST **aynı kural örneğini** çağırır: kural sayfa katmanındadır (ortak altyapı `Contracts` dahil hiçbir modül türüne başvuramadığından `Infrastructure/Operations/` ürün kuralını taşıyamaz); kuralın okuduğu bağlam `Contracts` okumasından gelen düz değerdir. `09 §8.3` m.1 bunu yazar.
- `09 §8.3` yeniden yazıldı: m.1 (kural sayfa katmanında), m.2 (a) (`OperationRunner`), m.3 (çalıştırıcı + `LockOrderAsync`), m.6 (mimari test), yazımın kararı (5). `§25` satırı, `CONTEXT.md`, `DEFERRED_BACKLOG` D-153/D-154 adları, durum ve hafıza tutarlı.
- Kaynak taraması kapı desenini `OperationRunner` adına çevirdi (ihlal örneği, test satırları aynı satır numaralarıyla).
- Örnek sayfa (`Pages/Panel/OperationSample.cshtml.cs`) artık `AppDbContext` ve varlık kullanmaz: `OperationRunner` + `IOrderingModule` + test tarafı `SampleOrderOperations` taklidi.

**Kalıbın mimari testin gördüğü yerde kanıtı.** `tests/Shopfolio.UnitTests/Architecture/OperationPagePattern.cs`: `Shopfolio.Pages.Panel.OperationPatternPage` (kalıbı kullanan, `[GatedOperation]` taşıyan sayfa; `OperationRunner`, `IOrderingModule`, `IOperationRule`). `ModuleBoundaryTests.OperationPatternPage_UnderThePageRule_IsCleanAndReallyUsesTheRunnerAndTheContractsLock` sayfa kuralını (`PageRule`) bu tür üzerinde çalıştırır: 0 ihlal; ve türün gerçekten bu üç türe bağlı olduğunu (boş geçmeyi) doğrular. `OperationRunnerAndTheGate_…` çalıştırıcının hiçbir `Shopfolio.Modules.*` türüne başvurmadığını ve `GatedOrderOperation`'ın kalmadığını doğrular. Eski biçim (sayfa modül iç türünü enjekte eder) zaten `ViolationSamples/ModuleBoundary` `ForbiddenPage*` örnekleriyle kırmızıdır.

⚠ **Seçilen yol ve gerekçe.** Üç seçenek vardı: (a) kapıyı modülde tutup `Contracts` arayüzü yapmak; (b) örnek sayfayı üretim koduna koymak; (c) mimari testi test projesini de kapsayacak biçimde genişletmek. (a) reddedildi: kapı generic delege alan bir sıradır, `Contracts` kayıtları değişmez veri taşır, delege ve ortak altyapı türü (`OperationAvailability`, `IOperationRule`) `Contracts`'a sızar; üstelik sipariş dışı kayıtlar (üye hesabı) için de aynı sıra gerekir. Seçilen: sıra ortak altyapıda (`OperationRunner`; `09 §6` ortak altyapıyı sayfalara açık sayar), modüle özgü tek parça (kilit + sürüm) `Contracts` yöntemi. Kalıp, üretim kodu eklemeden, `Shopfolio.Pages` ad alanında bir örnek türle mimari testin görüş alanına alındı (b yerine; (c)'nin odaklı biçimi). `OperationPagePattern.cs` ad alanı klasörle uyuşmaz (kural ad alanından okur); IDE0130 dar kapsamlı `#pragma` ile kapatıldı.
⚠ **K1 (D-154)** düzeltmeyle kapanmadı (kapı deseninin `.RunAsync(` istemesi ve görünüm üye adı taraması ayrı iş); T367'ye bağlı kaldı, dokunulmadı.
⚠ **Yeni arayüz yöntemi:** `IOrderingModule.LockOrderAsync` (`Contracts`'a ilk işlem yöntemi).

**Test (düzeltme sonrası).** Release `-warnaserror` 0 uyarı/0 hata; `dotnet format --verify-no-changes` çıkış 0; Unit **2636 / 2636** (2634 + 2 mimari test); Integration **1695 / 1695** (Web 150 + 133 + 225 = 508; Security+Concurrency+Disposal+Files+FileSecrets+Logging+Outages+SignedEndpoints 182; Jobs+Database 112; Model 893), başarısız 0, atlanan 0.

### Öz denetim (düzeltme 1; bütün maddeler yeniden)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 GET sunucu kararından üretir | ✓ | `OperationPatternTests.Get_AnOrderBeingPrepared…_KK1_KK6`, `Get_AClosedCancel_HasNoCancelButton_KK1` (13 test yeniden geçti) |
| Kriter 2 tarayıcı durumdan işlem türetmez | ✓ | `OperationSourceScanTests` (tarama kapı adı değişikliğiyle geçti); `Decides.cshtml`/`decides.js` kırmızı |
| Kriter 3 her POST aynı kuralı satır kilidi altında yeniden denetler | ✓ | `OperationRunner.RunAsync` (`LockOrderAsync` → `load` → `OperationGate.Decide` → `apply`); `Post_AnOpenOperation_IsAppliedUnderTheLock…_KK3`; **gerçek sayfa ad alanında** `ModuleBoundaryTests.OperationPatternPage_UnderThePageRule_…` temiz |
| Kriter 4 gizleme denetimin yerine geçmez | ✓ | `Post_ByHandForAClosedOperation_WhoseButtonWasNeverShown…` |
| Kriter 5 kapalıysa hiçbir şey yazılmaz; yanıt sınıfa göre | ✓ | `Post_ByHand…`, `Post_AQuantityAbove…`, `Post_ThePanelsOperation_…_AndWritesNothing_KK5` (geçti) |
| Kriter 6 sunucunun türettiği değerler sayfada | ✓ | `Get_…_KK1_KK6`, `Get_TheWithdrawalOnItsLastDay…_KK6_KK7` |
| Kriter 7 açık aralık `03 §1.11` aralığı | ✓ (örnek kuralla; D-153) | `SampleCancelRule`, `SampleWithdrawRule`; son gün/ertesi gün testleri geçti |
| Kriter 8 kural tek yerde; GET ve POST aynı işlev | ✓ | `TheGetAndThePost_CallTheSameRuleInstance_KK8`; `OperationPatternPage` GET ve POST'ta aynı `rule` alanı |
| KK1 kapalı işlemin düğmesi yok | ✓ | `Get_AClosedCancel_HasNoCancelButton_KK1` |
| KK2 betik karar vermez | ✓ | `ScanClient_OfAScriptThatDecides_IsAFinding`; sayfa temiz testi |
| KK3 elle gönderilen kapalı POST uygulanmaz | ✓ | `Post_ByHand…_KK3_KK4_KK5`; kalıp artık gerçek sayfa ad alanında kullanılabilir (B1) |
| KK4 gizli düğmenin POST'u reddedilir | ✓ | aynı test |
| KK5 reddedilen işlemde veritabanı değişmez | ✓ | `Post_ByHand…`, `Post_AQuantityAbove…`, `Post_ThePanelsOperation_…_KK5` |
| KK6 açık adet ve üst sınır sunucudan | ✓ | `Get_AnOrderBeingPrepared…_KK1_KK6` |
| KK7 son gün sınırda | ✓ | `Get_TheWithdrawalOnItsLastDay…_KK6_KK7`, `Post_TheWithdrawalOnTheLastDayIsApplied…_KK7` |
| KK8 GET ve POST aynı kural işlevi | ✓ | `TheGetAndThePost_CallTheSameRuleInstance_KK8` |
| `09 §25` "kod hiçbir doküman cümlesiyle çelişmiyor" | ✓ | `§8.3` m.1/m.2/m.3/m.6 + yazımın kararı (5) yeniden yazıldı, `§6` m.2/m.7 ile çelişmiyor; `§25` satırı `LockOrderAsync`'i anar; `CONTEXT`, `IMPLEMENTATION_STATUS`, `MEMORY`, D-153/D-154 adları güncellendi; `grep GatedOrderOperation` kod ve `CONTEXT`'te boş (`09` yalnız kaldırıldığını anar) |
| Güvenlik taraması | ✓ | karar sunucuda; `LockOrderAsync` yalnız kilit+sürüm karşılaştırması, istemci değeri karar vermez; yeni sır/bağımlılık yok |
| Sık FAIL 1 geniş tarama istisnası | ✓ | tarama değişmedi, yalnız ad; yorum/dizge/zincir testleri geçti. Tarama darlığı K1/D-154 (T367) |
| Sık FAIL 2 çağrı biçimi kaçırma | ✓ | işaret ve kapı biçimleri testleri geçti (`OperationRunner` adıyla aynı satırlar) |
| Sık FAIL 3 sayı/etiket çoğaltma | ✓ | eski ad `GatedOrderOperation`: kod, test, `09`, `CONTEXT`, `D-153/154`, durum, hafıza taranıp güncellendi; Unit sayısı (2636) durum ve hafızada; Integration 1695 değişmedi |
| Sık FAIL 4 test beklentisi elle yazılı | ✓ | yeni mimari testler beklentiyi tür adlarından kurar; sayı sabiti eklenmedi |
| Sık FAIL 5 modül arayüz yöntemi kendi işlemini açıyor | ✓ | `LockOrderAsync` işlem açmaz, çağıranın işleminde çalışır (`OrderLocks` işlem yoksa istisna atar); işlemi `OperationRunner` sayfa adına açar |
| Sık FAIL 6 (tur 1) kalıp yalnız testteki örnek sayfada | ✓ | `Shopfolio.Pages.Panel.OperationPatternPage` sayfa kuralı altında temiz; örnek test sayfası da artık `AppDbContext`/varlık kullanmaz |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T217-yapilabilir-islemler`
- **Commit:** son commit PR başlığında; kod son commit'i dalın ucudur
- **PR:** #412
- **CI:** (yapım işçisi izlemez; yönetici izler)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (saf sunucu mantığı; yeni paket, servis, kütüphane yok) | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gerçek ekran kuralları (iptal, gecikme feshi, cayma, ayıp talebi ve panel işlemleri) ve her işlem POST'unun `[GatedOperation]` taşıması ekran task'larının kabul kriteri | D-153 (hedef T367) | T217 kalıbı ve kapıyı kurar; kuralların bağlamı (sipariş, kalem, süreler) F3–F5 task'larındadır |

Bu task'ın hedefi olan başka backlog kaydı yoktur (`grep -n T217 Docs/DEFERRED_BACKLOG.md` boş). Kapatılan D kaydı: yok. Açılan: D-153.

## Notlar

- **Working tree (Adım -1):** temiz; önceki işçi bölündü, devam dalı `origin/task/T217-yapilabilir-islemler` (8f4255f) üstüne kuruldu.
- **Ana dal CI startup (Adım 0):** devam işçisi CI izlemez (yönetici izler).
- **Önceki task kaydı:** T216 status Commit hücresi (`9ab9bec`) ve `T216_REPORT` post-merge satırı dolduruldu (skill adım 15); eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı.
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Kararlar:** (1) gerçek ekran kuralları F3–F5'te, T217'de örnek kural; (2) müşterinin "kapalı"sı bildirimde `OperationClosed`, kayıt satırında `OperationNotAvailable`; yeni kod yok.
- **Diğer:** Integration toplamı (1595; validator: doğrusu 1695 — CI ve parça koşuları) tek tam koşudur; T216 raporundaki 1682 parça koşularıyla toplanmıştı; sayım yöntemi farklı olduğundan doğrudan karşılaştırılmadı (bu koşuda 0 başarısız, 0 atlanan).
