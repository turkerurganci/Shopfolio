# T217 — Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İşlemin tek kuralı (`Infrastructure/Operations/`):** `IOperationRule<TContext>` (kayıt durumundan `OperationAvailability` üretir; saat, veritabanı, istek okumaz), `OperationAvailability` (açık mı, açık adet, varsayılan, üst sınır, son gün), `OperationDecision` + `OperationGate.Decide` (saf kapı: `Closed`, `QuantityExceedsOpen`; adet üst sınırı aşarsa bütünüyle red).
- **Ret cevabı (`OperationRefusals`):** müşteri sınıfı 303 + `OperationClosed`/`QuantityExceedsOpen` bildirimi; öteki sınıf 422 (`OperationNotAvailable`/`QuantityExceedsOpen`, eylem düzeyinde mesaj); her redde `RejectionLog` satırı (müşterinin "kapalı"sı satırda `OperationNotAvailable`).
- **İşaret ve tarama:** `GatedOperationAttribute` + `OperationSourceScan` (işaretli işleyici kapıya ulaşmalı; işaret GET'te kırmızı; görünüm ve betik durum adı/tarih hesabı taşımaz) + ayrı ihlal örnekleri `ViolationSamples/Operations/`.
- **İşlem sırası (düzeltme 2 hâli):** kural MODÜLDE yaşar, sayfaya yalnız `Contracts` yüzeyiyle ulaşır. Sayfa işlemi `OperationRunner.RunAsync` (ortak altyapı; `ITransactionRunner` sarmalayıcısı) ile açar ve modülün `Contracts` yazma yöntemini içinde çağırır; yöntem işlem açmaz ve sırayı `OperationGate.ExecuteAsync`'e verir: satır kilidi ve (panelde) sürüm → durum yükleme → modülün kural örneği (`OperationGate.Decide`) → yalnız `Allowed` ise yazım; ret `GatedOperationResult` dönüş değeridir ve işlem geri alınır. GET'te modülün `Contracts` okuma yöntemi aynı kural örneğinden `OperationAvailability` verir. `OperationAvailability`, `OperationVerdict`, `OperationDecision`, `GatedOperationResult` `public` değerlerdir (Contracts yöntemlerinin türleri). Tarihçe: ilk yazım `Modules/Ordering/GatedOrderOperation`, düzeltme 1 ürün kuralını sayfa katmanına koymuştu (B1-r2: 05/07/11 ile çelişti); ikisi de kaldırıldı.
- **Örnek modül ve sayfa (yalnız test derlemeleri):** `Pages/OperationSamples.cs` modül biçiminde: `ISampleOrderOperations` + `SampleOrderOperationsView` (Ordering `Contracts` ad alanı), `SampleOrderOperations` (Ordering ad alanı; `OrderLocks`, kural, yükleme, yazım) ve `SampleCancelRule` 03 §1.11.2, `SampleWithdrawRule` 03 §1.11.5; `Pages/Panel/OperationSample.cshtml(.cs)` yalnız arayüzü ve `OperationRunner`'ı kullanır, kural bilmez.
- **Devam turunda:** birim testleri derlendi ve geçti (erişilebilirlik: test sınıfı public, kapı testinin satır verisi `bool`; tarama test verisi `MemberData` — `ForbiddenTestPatternScan` satır içi `) {` kalıbını test gövdesi sanıyordu); tam Unit, tam Integration, Release `-warnaserror`, `dotnet format` koşuldu.
- **Doküman:** `09 §8.3` (yeni), `§8` girişi, içindekiler satırı, `§8.2` m.3 yardımcı cümlesi, `§25` satırı; `CONTEXT.md`, `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `MEMORY.md`, `DEFERRED_BACKLOG.md` (D-153), T216 kayıtları.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Operations/` (7 yeni dosya; düzeltme 2: `OperationGate.cs` `ExecuteAsync`, `OperationRunner.cs`, `OperationAvailability.cs`/`OperationDecision.cs` `public`), `Program.cs` kaydı. `Modules/Ordering/{OrderingModule.cs, Contracts/IOrderingModule.cs}` main ile aynı (düzeltme 2: `LockOrderAsync` kaldırıldı; üretim modülüne işlem yöntemi eklenmedi)
- `tests/Shopfolio.UnitTests/`: `Infrastructure/OperationGateTests.cs`, `Architecture/{OperationSourceScan.cs, OperationSourceScanTests.cs}`, `Architecture/ViolationSamples/Operations/{GatedHandlers.cs, Decides.cshtml, decides.js}`
- `tests/Shopfolio.IntegrationTests/`: `Pages/OperationSamples.cs`, `Pages/Panel/OperationSample.cshtml(.cs)`, `Web/OperationPatternTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T216_REPORT.md` (post-merge satırı), `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | GET işlemleri sunucu kararından üretir; düğme yanıtın içinde | ✓ | `OperationPatternTests.Get_AnOrderBeingPrepared_ShowsTheCancelFormOnly…_KK1_KK6`, `Get_AClosedCancel_HasNoCancelButton_KK1` |
| 2 | Tarayıcı durumdan işlem türetmez | ✓ | `OperationSourceScan` `ViewDecides`/`ScriptDecides`; `OperationSourceScanTests.ScanClient_*`, `TheSamplePage_MarksItsPostHandlers_…AndItsViewDecidesNothing` |
| 3 | Her POST aynı kuralı yeniden denetler; satır kilidi altında, o anki hâle karşı | ✓ | `OperationGate.ExecuteAsync` (kilit+sürüm → yükle → kural → yaz; modülün `Contracts` yazma yöntemi çağırır, `OperationRunner` işlemi açar); `OperationGateTests.TheSequence_*`; `ModuleBoundaryTests.OperationPatternPage_UnderThePageRule_…` (sayfa ad alanında kalıp temiz); `Post_AnOpenOperation_IsAppliedUnderTheLock…_KK3`; tarama `GatedWithoutCheck` |
| 4 | Gizleme/etkisiz gösterme denetimin yerine geçmez | ✓ | `Post_ByHandForAClosedOperation_WhoseButtonWasNeverShown_IsNotApplied…_KK3_KK4_KK5` |
| 5 | Açık değilse uygulanmaz, hiçbir şey yazılmaz; yanıt sınıfa göre | ✓ | müşteri 303: `Post_ByHand…`/`Post_AQuantityAbove…`; panel 422/409: `Post_ThePanelsOperation_WithTheVersionEqual…Is422…409`; `OperationGateTests`; `09 §8.3` m.4 |
| 6 | Sunucunun türettiği değerler sayfada (açık adet, varsayılan, üst sınır, son gün) | ✓ | `Get_AnOrderBeingPrepared…` (`max=`/`value=`), `Get_TheWithdrawalOnItsLastDay…_KK6_KK7` |
| 7 | Açık aralık 03 §1.11'in aralığıdır | ✓ (örnek kuralla) | `SampleCancelRule` 1.11.2, `SampleWithdrawRule` 1.11.5; son gün açık/ertesi gün kapalı testi. ⚠ ürünün gerçek kuralları F3–F5'te (`09 §8.3` yazımın kararı 1; D-153) |
| 8 | Kural tek yerde; GET ve POST aynı işlevi çağırır | ✓ | kural modülde (örnekte `SampleOrderOperations`, `SampleRules`); okuma ve yazma yöntemi aynı örneği çağırır: `TheGetAndThePost_CallTheSameRuleInstance_KK8`; sayfa kural türüne başvurmaz: `ModuleBoundaryTests.OperationPatternPage_…ThroughTheContractsInterface` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 kapalı işlemin düğmesi yanıtta yok | ✓ | `Get_AClosedCancel_HasNoCancelButton_KK1` (durum × ödeme teorisi) |
| KK2 betik karar vermez | ✓ | `OperationSourceScanTests.ScanClient_OfAScriptThatDecides_IsAFinding` + `ViolationSamples/Operations/decides.js`; sayfa temiz testi |
| KK3 elle gönderilen kapalı POST uygulanmaz | ✓ | `Post_ByHand…_KK3_KK4_KK5` |
| KK4 gizli düğmenin POST'u da reddedilir | ✓ | aynı test (düğme hiç gösterilmemiş sipariş) |
| KK5 reddedilen işlemde veritabanı değişmez | ✓ | `Post_ByHand…`, `Post_AQuantityAbove…`, `Post_ThePanelsOperation_…_AndWritesNothing_KK5` (sipariş sürümü dahil); ret modülün yazma yönteminin dönüş değeridir, `OperationRunner`'ın işlemi geri alınır |
| KK6 açık adet ve üst sınır sunucudan | ✓ | `Get_AnOrderBeingPrepared…_KK1_KK6` (beklenen değer siparişin kalem adedinden türetilir) |
| KK7 aralığın son günü sınırda | ✓ | `Get_TheWithdrawalOnItsLastDay…_KK6_KK7`, `Post_TheWithdrawalOnTheLastDayIsApplied…_KK7` |
| KK8 GET ve POST aynı kural | ✓ | `TheGetAndThePost_CallTheSameRuleInstance_KK8` |
| `09 §25` ilgili satır | ✓ | `§25` yeni satır → `§8.3`; ayrıca `§8`, `§19.2` m.3 ihlal örnekleri |
| Güvenlik taraması (yetkilendirme/yeniden denetim) | ✓ | kapı sunucuda; istemci değeri karar vermez (`Input.*` yalnız adet, kapı üst sınırla karşılaştırır) |
| Sık FAIL 1 — bekçi/tarama istisnası geniş | ✓ | yorum, `""` ve `@""` içindeki ad kapı sayılmaz (`ScanHandlers_OfACheckThatIsOnlyInACommentAStringOrTooFarAway_IsAFinding`); zincir en çok dört halka (beşinci bulgu) |
| Sık FAIL 2 — tarama çağrı biçimi kaçırır | ✓ | işaret: `[GatedOperation]`, `…Attribute`, ad alanı nitelikli, listede başka nitelikle, `()` ile, çok satırlı imza; kapı: boşluklu `OperationGate . Decide`, nitelikli, `=>` gövde, `GatedOrderOperation` (`…InEverySpellingOfTheMark`, `…ReachesTheGate…IsClean`) |
| Sık FAIL 3 — sayı/etiket çoğaltma | ✓ | `09 §8` giriş cümlesi, içindekiler satırı (`§8.3`), `§8.2` m.3 yardımcı cümlesi, `§25`, `CONTEXT`, backlog sayacı (92 → 93), status ve changelog güncellendi |
| Sık FAIL 4 — test beklentisi elle yazılı | ✓ | statü/üye listeleri `Enum.GetNames`'ten; bildirim adresi `NoticeCode`'dan; adet beklentisi kalem adedinden. `OperationGateTests` küçük tam sayı kural girdileridir (kuralın kendi değeri) |
| Sık FAIL 5 — modül arayüz yöntemi kendi işlemini açıyor | ✓ | işlemi yalnız `OperationRunner` açar (sayfa işleyicisinin yardımcısı); `Contracts` yöntemi ve `OperationGate.ExecuteAsync` işlem açmaz (`09 §9.1` m.3); `09 §8.3` m.3 yazar |

> Bu tablo düzeltme 1 hâlidir; güncel (düzeltme 2) bütün maddeler aşağıda "Öz denetim (düzeltme 2)" tablosundadır.

> `? Doğrulanamadı` **FAIL değildir**; bu turda doğrulanamayan madde yok. Kestrel notu: kapı sayfa mantığıdır, sunucu sınırı (gövde/üst bilgi) değildir; bu yüzden TestServer yeterlidir.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | düzeltme 2 sonrası **2640**, başarısız 0 (düzeltme 1: 2636; +4 `OperationGateTests.TheSequence_*`) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build` filtreli parçalar (Postgres 18, `shopfolio-t217-fix2-pg`, 127.0.0.1:55702; koşu sonrası silindi) | **1695**, başarısız 0, atlanan 0: Concurrency+Database 61, Security 32, Disposal+FileSecrets+Outages+Files+Logging+SignedEndpoints 124, Jobs 77, Web 508 (186 + 157 + 152 + `OperationPatternTests` 13), Model 893; `--list-tests` 1695 (T216 1682 + 13) |
| Derleme/biçim (düzeltme 2) | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format Shopfolio.slnx --verify-no-changes` | 0 uyarı, 0 hata; değişiklik yok |
| Contract | — | — | bu task'ta yok |
| Derleme/biçim | ✓ | `dotnet build -c Release -warnaserror`; `dotnet format --verify-no-changes` | 0 uyarı, değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 2 (2026-11-06, head `134c45c`); tur 1 ✗ FAIL (head `3cb1380`) |
| Bulgu sayısı | Tur 2: 1 (B1-r2 ORTA, S1); tur 1'in B1'i kapandı |
| Düzeltme gerekli mi | Evet — B1-r2 (ikinci iterasyon: düzeltmesi de FAIL verirse eskalasyon, `INSTRUCTIONS §3.3`) |
| Bloke-etmeyen not (K) sayısı | Tur 2: 1 (K1 → D-155); tur 1: 2 (K1 → D-154, K2 kapatıldı) |

### Doğrulama tur 2 — ✗ FAIL

**B1 (tur 1) kapandı:** `OperationRunner` ortak altyapıda ve hiçbir `Shopfolio.Modules.*` türüne başvurmuyor; kilit `IOrderingModule.LockOrderAsync` (`Contracts`); `Shopfolio.Pages.Panel.OperationPatternPage` sayfa kuralı (`PageRule`) altında 0 ihlal ve gerçekten `OperationRunner`, `IOrderingModule`, `IOperationRule` kullanıyor (`ModuleBoundaryTests`, Unit 2636 yeşil).

**B1-r2 (ORTA, S1; kriter 7, 8; KK8; `09 §25` "kod/doküman hiçbir üst doküman cümlesiyle çelişmiyor mu"):** düzeltme, B1'i ürün kuralını sayfa katmanına taşıyarak kapatıyor ve bunu `09 §8.3` madde 1'e bağlayıcı kural olarak yazıyor: "**Ürün kuralı sayfa katmanında yazılır; modül iç türü olmaz** … `IOperationRule<TContext>`'i sayfanın katmanında (`Pages/` altında, işlemin ekranının yanında) gerçekler … Kuralın hangi dosyada durduğu o ekran task'ının kabul kriteridir". Bu, üst belgelerle çelişir: `05 §3.3.1` ve §3.3.5 (modüller `02`'nin kural gruplarından türer; iptal, cayma ve iade kuralları "İptal, cayma ve iade" modülünündür), §3.3.12 (vitrin ve panel modül değildir; sayfa modülün arayüzünü çağırır), §3.3.13 (modülleri sunum/iş/veri katmanlarına ayırmak elenmiştir: "bir kuralın değişikliği her katmanı dolaşır, `02`'nin kural grupları kodda görünmez olur"); `07 §8.1.1` ("sayfa kuralı yeniden yazmaz, uygular"); `11` T323 (F3, Sepet ve sipariş modülü: "`03 §1.11`'in 41 işlemi tek bir 'yapılabilir işlemler' kararında yaşar … ekran ve uç aynı kararı kullanır"). D-153'ün "ilk T367" hedefi de F3 modül task'ıdır (sayfaları F5'te) — `09 §8.3` m.1'e göre kuralını yazacak sayfa katmanı yoktur. Ayrıca kural sayfadayken modülün `apply` `Contracts` yöntemi kuralsız yazar; modül kendi değişmezini korumaz. Karar kaydı yok; tur 1'in beklenen düzeltmesi ("kural ve kapılı sipariş işlemi `Ordering.Contracts` yüzeyinden") ise (a) seçeneği olarak gerekçeyle reddedilmiş (rapor "Seçilen yol"), ama ret üst belge değişikliği doğuruyor. Beklenen: ürün kuralı modülde kalır ve sayfaya `Contracts` yüzeyiyle ulaşır (ör. GET için `Contracts` okuma yöntemi açıklığı ve sunucu değerlerini döner; POST'ta modülün yazma yöntemi aynı kuralı kilit altında yeniden işletir ya da kural `Contracts`'tan açılır), `09 §8.3` m.1/m.3 buna göre yazılır; ya da sayfa katmanı yolu isteniyorsa proje sahibinin kararıyla K kaydı açılır ve `05 §3.3`, `11` T323/T367 hizalanır (L2 tek başına üst belgeyi değiştiremez).

**Kriterler:** 1, 2, 4, 5, 6 ✓; 3 ✓ (kilit → sürüm → yükle → kural → yaz sırası `OperationRunner`'da; ret işlemi geri alır); 7 ✗ (örnek kurallar `03 §1.11.2`/§1.11.5 aralığını doğru uygular, ama kalıbın kuralın yerine dair bağlayıcı cümlesi üst belgeyle çelişir); 8 ✗ (GET ve POST aynı örneği çağırıyor — KK8 testi yeşil — ama "kural tek yerde" sayfa katmanında tanımlandığı için T323'ün tek kararıyla çelişir). KK1–KK7 ✓. **Doğrulayıcı sondası (geçici, silindi):** altı ret (müşteri kapalı 303, panel kapalı 422, eski sürüm 409, adet aşımı 303 ve 422, kapalı cayma 303) sonrası veritabanının **bütün tablolarının** içerik özeti değişmedi; ardından açık iptal yalnız `public.orders` ve `public.cancellations`'ı değiştirdi. Aynı sonda: `Input.Quantity=0` ve adetsiz POST 500 → K1 (D-155). **Test:** Release dışı Debug `-warnaserror` 0/0; `dotnet format --verify-no-changes` temiz; Unit 2636/2636; betik testleri 4/4; Integration 1695/1695 (parçalı, 0 atlanan; CI 42244744446 `total: 1695`, 0 atlanan; 1682 + 13 `OperationPatternTests` = 1695, keşfedilmeyen yok). Dal CI 42244744446 yeşil. `12` `Durum` değişmedi (T217'nin KP'si yok; `07 §8.1`'i anan VAL-A578, A716, B097, B117 BEKLEMEDE, ekran task'larının). Rapor: `helper-state/.../T217/dogrulama-raporu-r2.md`.

### Doğrulama tur 1 — ✗ FAIL

**B1 (ORTA, S1; kriter 3, 8; KK3, KK8; `09 §25` "kod hiçbir doküman cümlesiyle çelişmiyor mu"):** kalıp gerçek bir sayfada kullanılamıyor. `09 §8.3` madde 3 `GatedOrderOperation`'ı "giriş noktasının (sayfa işleyicisinin) işlem sahipliği" yapar; ama sınıf `Shopfolio.Modules.Ordering` ad alanındadır (`Contracts` dışı) ve `09 §6` madde 2 ve 7 sayfanın modülün `Contracts` dışı türüne başvurmasını ve onu enjekte etmesini yasaklar. Doğrulayıcı sondası: `src/Shopfolio.Web/Pages/Panel/` altına `GatedOrderOperation`'ı enjekte edip `RunAsync` çağıran geçici bir sayfa modeli eklenince `ModuleBoundaryTests.PagesAndEndpoints_InApplicationAssembly_TouchNeitherModuleInternalsNorTheContext` kırmızı ("ValidatorProbeT217Model does …"); sonda silindi. Örnek sayfa test derlemesinde (`Shopfolio.IntegrationTests.Pages`) durduğu için sınır testinin kapsamı dışındadır ve çelişkiyi gizler — örnek sayfa `AppDbContext`'e ve varlıklara da doğrudan dokunur. Aynı boşluk GET tarafında da var: ürünün kuralları modülün iç türü olursa sayfa GET'te kural örneğine `Contracts` dışından ulaşamaz; `09 §8.3` kuralın ve kapılı sipariş işleminin sayfaya hangi `Contracts` yüzeyiyle ulaştığını yazmıyor. Beklenen: kalıp `09 §6`'ya uyan bir yolla sunulur (ör. sipariş işlemini ve kural değerlendirmesini `Ordering.Contracts` arayüzünden veren bir yüzey, işlemi sayfanın `ITransactionRunner`'ı açar; `09 §6` madde 3 ve `§9.1` madde 3 ile), `09 §8.3` madde 1 ve 3 bunu yazar ve örnek sayfa ya da bir mimari test gerçek `Shopfolio.Pages` ad alanındaki kullanımın sınır testinden geçtiğini gösterir.

Kriter 1, 2, 4–7 ve KK1, KK2, KK4–KK7 ✓ (kriter 7 örnek kuralla; gerçek kurallar D-153). Doğrulayıcı ayrıca sınadı: üç reddedilen POST (müşteri 303, panel 422, panel 409) sonrası **bütün tabloların** içerik özeti değişmedi; Türkiye gece yarısı sınırında (20:59:59Z açık, 21:00:00Z kapalı) son gün doğru davranıyor (geçici sondalar, silindi). Unit 2634, Integration 1695 geçti, 0 atlanan; dal CI 42236914259 yeşil. K1: tarama darlıkları → D-154. K2: Integration sayısı 1595 → 1695 (rapor, durum, hafıza) bu PR'da düzeltildi. `12` satırlarının `Durum`'u değişmedi (T217'nin KP'si yok; `07 §8.1`'i anan VAL-A578, VAL-A716, VAL-B097, VAL-B117 ekran task'larınındır).

## Düzeltme 1 (doğrulama tur 1 sonrası)

> Tarihçe. Bu bölümün "kural sayfa katmanındadır" ve `IOrderingModule.LockOrderAsync` kararı tur 2'de B1-r2 ile reddedildi; güncel hâl "Düzeltme 2"dedir.

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

## Düzeltme 2 (doğrulama tur 2 sonrası; B1-r2)

**B1-r2 çözümü (sahip kararı: kural modülde yaşar, sayfaya yalnız `Contracts` yüzeyiyle ulaşır).**

- **Kural ve sıra modülde.** `OperationGate.ExecuteAsync` (ortak altyapı, işlem açmaz) modülün yazma yönteminin sırasıdır: `lockRecord` (satır kilidi + panelde sürüm karşılaştırması; uymazsa çakışma, hiçbir şey okunmaz/yazılmaz) → `load` (kilitten sonra) → modülün kural örneği `OperationGate.Decide` → yalnız `Allowed` ise `apply`. Sonuç `GatedOperationResult`.
- **Sayfa kural yazmaz, uygulamaz.** GET: modülün `Contracts` okuma yöntemi açıklığı ve sunucu değerlerini (açık adet, varsayılan, üst sınır, son gün) `OperationAvailability` olarak verir. POST: sayfa işlemi `OperationRunner.RunAsync` ile açar (ince `ITransactionRunner` sarmalayıcısı; kural bilmez) ve içinde modülün `Contracts` yazma yöntemini çağırır; ret geri alınır, hiçbir şey yazılmaz. Modül yöntemi kendi işlemini açmaz (`09 §6` m.3, §9.1 m.3).
- **Public değerler.** `OperationAvailability`, `OperationVerdict`, `OperationDecision`, `GatedOperationResult` `public` (Contracts yöntemlerinin türleri, `09 §6` m.1); `IOperationRule` modülün `internal` türü olarak kalır.
- **`IOrderingModule.LockOrderAsync` ve `OrderingModule` kurucu değişikliği geri alındı** (dosyalar main ile aynı); üretim modülüne bu task'ta işlem yöntemi eklenmez, gerçek işlemleri modül task'ları (ilki T367) yazar.
- **Örnek ve mimari test hizalandı.** `Pages/OperationSamples.cs`: modül biçiminde `ISampleOrderOperations` (Ordering `Contracts` ad alanı) + `SampleOrderOperations` (Ordering ad alanı; `OrderLocks`, kural, yükleme, yazım). `OperationSample.cshtml.cs`: yalnız arayüz + `OperationRunner`. `OperationPagePattern.cs`: `OperationPatternPage` yalnız `IPatternOrderOperations` (Contracts) + `OperationRunner` kullanır. `ModuleBoundaryTests.OperationPatternPage_UnderThePageRule_IsCleanAndReachesTheRuleOnlyThroughTheContractsInterface`: sayfa kuralı + Contracts kuralı temiz; sayfa `OperationRunner` ve `IPatternOrderOperations`'a bağlı, `IOperationRule`'a, `OperationGate`'e ve `Ordering.Contracts` dışındaki modül türüne bağlı değil. Yeni birim testleri `OperationGateTests.TheSequence_*` (sıra, kapalı → yazım yok, adet aşımı → yazım yok, sürüm çakışması → yükleme/kural yok).
- **Belge.** `09 §8.3` m.1 (kural modülde; 41 işlem T323'te tek kararda), m.3 (sayfa işlemi açar, modül işletir), m.6 (test ve mimari kanıt), yazımın kararı (1) ve (5) yeniden yazıldı; `§25` satırı; `CONTEXT.md`; D-153 metni; status ve hafıza. **`05 §3.3.1/§3.3.5/§3.3.12/§3.3.13`, `07 §8.1.1/§8.1.5` ve `11` T323/T367'den ayrılan cümle kalmadı; karar kaydı gerekmedi.**
- **Üst belge karşılaştırması.** `05 §3.3.1` (satır 379: her modül kendi kural grubunun/verisinin sahibi) ↔ `09 §8.3` m.1 (kural modülde); `05 §3.3.12` (satır 395: sayfa modül arayüzünü çağırır, veriye dokunmaz) ↔ m.1/m.3 (sayfa `Contracts`'tan ulaşır); `05 §3.3.13` (satır 397: teknik katman ayrımı elenmiş) ↔ kural sayfa katmanında değil; `07 §8.1.1` (satır 5405: sayfa kuralı yeniden yazmaz, uygular) ↔ sayfa kural yazmaz/uygulamaz, çağırır; `07 §8.1.2/§8.1.5` (kural iki yerde yazılmaz) ↔ okuma ve yazma yöntemi aynı örnek; `11` T323 kriter 1–2 (satır 2274–2275: 41 işlem tek kararda, ekran ve uç aynı karar) ↔ m.1; T367 (D-153) kuralını kendi modülünde yazar.

**Test (düzeltme 2).** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı/0 hata; `dotnet format Shopfolio.slnx --verify-no-changes` değişiklik yok; Unit **2640 / 2640**; Integration **1695 / 1695** (gerçek PostgreSQL 18; `--list-tests` 1695): Concurrency+Database 61, Security 32, Disposal+FileSecrets+Outages+Files+Logging+SignedEndpoints 124, Jobs 77, Web 508 (186 + 157 + 152 + `OperationPatternTests` 13), Model 893; başarısız 0, atlanan 0.

### Öz denetim (düzeltme 2; bütün maddeler yeniden, kendi diff'inde)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 GET sunucu kararından üretir | ✓ | `OperationSample.cshtml` `@if (Model.Cancel.IsOpen)`; `Get_AnOrderBeingPrepared…_KK1_KK6`, `Get_AClosedCancel_HasNoCancelButton_KK1`; `ISampleOrderOperations.ReadAsync` açıklığı modülün kuralından verir (`OperationSamples.cs` `ReadAsync`) |
| Kriter 2 tarayıcı durumdan işlem türetmez | ✓ | `OperationSourceScanTests` (örnek sayfa/görünüm temiz, `Decides.cshtml`/`decides.js` kırmızı); sayfada betik yok |
| Kriter 3 POST aynı kuralı kilit altında, o anki hâle karşı yeniden denetler | ✓ | `SampleOrderOperations.RunAsync` → `OperationGate.ExecuteAsync(LockAsync → LoadAsync → kural → WriteAsync)`; `OperationGateTests.TheSequence_RunsLockLoadRuleWriteInThatOrder…`; `Post_AnOpenOperation_IsAppliedUnderTheLock…_KK3`; sayfa `OperationRunner`'a ulaşır (`GatedWithoutCheck` taraması geçti) |
| Kriter 4 gizleme denetimin yerine geçmez | ✓ | `Post_ByHandForAClosedOperation_WhoseButtonWasNeverShown…_KK3_KK4_KK5` |
| Kriter 5 kapalıysa hiçbir şey yazılmaz; yanıt sınıfa göre | ✓ | `TheSequence_AClosedOperation_WritesNothing…`, `TheSequence_AQuantityAboveTheUpperLimit_WritesNothing…`; `Post_ByHand…`, `Post_AQuantityAbove…`, `Post_ThePanelsOperation_…_KK5` (sipariş sürümü dahil); 303/422/409 `OperationRefusals` |
| Kriter 6 sunucu değerleri sayfada | ✓ | `Get_…_KK1_KK6` (`max=`/`value=`/`open-quantity`), `Get_TheWithdrawalOnItsLastDay…_KK6_KK7` |
| Kriter 7 açık aralık `03 §1.11` aralığı | ✓ (örnek kuralla; D-153) | `SampleCancelRule` 1.11.2, `SampleWithdrawRule` 1.11.5; son gün açık/ertesi gün kapalı testleri |
| Kriter 8 kural tek yerde; GET ve POST aynı işlev | ✓ | kural modülde (`SampleRules` → `SampleOrderOperations`); `TheGetAndThePost_CallTheSameRuleInstance_KK8` (GET 1 + POST 1 çağrı, aynı örnek); sayfa kural türüne başvurmaz: `ModuleBoundaryTests.OperationPatternPage_…ThroughTheContractsInterface` |
| KK1 kapalı işlemin düğmesi yok | ✓ | `Get_AClosedCancel_HasNoCancelButton_KK1` |
| KK2 betik karar vermez | ✓ | `ScanClient_OfAScriptThatDecides_IsAFinding` + `ViolationSamples/Operations/decides.js` |
| KK3 elle gönderilen kapalı POST uygulanmaz | ✓ | `Post_ByHand…_KK3_KK4_KK5` |
| KK4 gizli düğmenin POST'u reddedilir | ✓ | aynı test |
| KK5 reddedilen işlemde veritabanı değişmez | ✓ | `Post_ByHand…`, `Post_AQuantityAbove…`, `Post_ThePanelsOperation_…_KK5`; `OperationRunner` ret sonucunda işlemi geri alır (`TransactionRunner` rollback + `ChangeTracker.Clear`) |
| KK6 açık adet ve üst sınır sunucudan | ✓ | `Get_AnOrderBeingPrepared…_KK1_KK6` |
| KK7 son gün sınırda | ✓ | `Get_TheWithdrawalOnItsLastDay…_KK6_KK7`, `Post_TheWithdrawalOnTheLastDayIsApplied…_KK7` |
| KK8 GET ve POST aynı kural işlevi | ✓ | `TheGetAndThePost_CallTheSameRuleInstance_KK8` |
| `09 §25` "kod/doküman üst dokümanla çelişmiyor mu" | ✓ | `grep -n` ile karşılaştırıldı: `09 §8.3` m.1 (satır 702), m.3 (704), m.6 (707), (5) (709) ↔ `05 §3.3.1` (379), §3.3.5 (386), §3.3.12 (395), §3.3.13 (397); `07 §8.1.1` (5405), §8.1.2 (5407), §8.1.5 (5413); `11` T323 (2270–2275), T217 kriter 8 (1271–1281). Üst belgeden ayrılan cümle yok; `§25` satırı (2378) modül-kural biçimini sorar |
| `09 §6` modül sınırı (sayfadan `Contracts` dışı tür yok) | ✓ | `ModuleBoundaryTests` yeşil: gerçek `Shopfolio.Pages` ad alanındaki `OperationPatternPage` `PageRule` ve `ContractsReachNothingRule` altında temiz; `Ordering.Contracts` dışı modül türüne bağlı değil |
| `09 §6` m.3 / §9.1 m.3 modül yöntemi kendi işlemini açmaz | ✓ | `ISampleOrderOperations` yöntemleri ve `OperationGate.ExecuteAsync` işlem açmaz; işlemi yalnız `OperationRunner` (sayfa adına) açar; `OrderLocks` işlem dışında hata atar |
| Güvenlik taraması | ✓ | karar sunucuda ve modülde; istemci değeri (`Input.*`) yalnız adet/sürüm, kapı üst sınırla karşılaştırır; yeni sır/bağımlılık yok; yeni `public` türler yalnız değerdir |
| Sık FAIL — bekçi/tarama istisnası geniş | ✓ | tarama kodu değişmedi; yeni mimari doğrulamalar yalnız `Shopfolio.Modules.` öneki ve `Ordering.Contracts.` istisnasını arar (dar: tek ad alanı) |
| Sık FAIL — tarama çağrı biçimini kaçırıyor | ✓ | işaret ve kapı biçimleri testleri geçti; sayfa artık kapıya `OperationRunner` üzerinden ulaşır ve tarama `OperationRunner` adını zaten kapı sayar (`OnPostAfterRunner` darlığı D-154'te) |
| Sık FAIL — belge kuralını değiştiren 09 metni karar kaydısız | ✓ | yukarıdaki üst belge karşılaştırması; 05/07/11 değişmedi, 09 onlara hizalandı (bu FAIL türünün sebebi kaldırıldı) |
| Sık FAIL — DB varsayılanı eksik | ✓ (yok) | bu task'ta şema/göç/varsayılan sütun yok |
| Sık FAIL — kural değişince aynı sayıyı/etiketi anan yerler | ✓ | `grep` ile tarandı: `LockOrderAsync` yalnız tarihçe anlatan satırlarda (`09` yazımın kararı (5), status, hafıza, bu rapor); `CONTEXT.md`, D-153, `§25`, status (Unit 2640), hafıza güncellendi; Integration 1695 değişmedi |

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
- **⚠ Kararlar:** (1) gerçek ekran kuralları F3–F5'te, T217'de örnek kural; (2) müşterinin "kapalı"sı bildirimde `OperationClosed`, kayıt satırında `OperationNotAvailable`; yeni kod yok; (3) düzeltme 2: kural modülde, sayfaya yalnız `Contracts` okuma/yazma yöntemiyle ulaşır (sahip kararı); `09 §8.3` 05/07/11'e hizalı, karar kaydı yok.
- **Diğer:** Integration toplamı (1595; validator: doğrusu 1695 — CI ve parça koşuları) tek tam koşudur; T216 raporundaki 1682 parça koşularıyla toplanmıştı; sayım yöntemi farklı olduğundan doğrudan karşılaştırılmadı (bu koşuda 0 başarısız, 0 atlanan).
