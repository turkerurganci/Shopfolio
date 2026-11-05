# T203 — İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (doğrulama tur 1 FAIL → düzeltme 1 push edildi, tur 2 bekliyor) | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Ortak ardışık düzen** `Infrastructure/SignedEndpoints/SignedEndpointHandler`: (1) gövde tavanı — bildirilen uzunluk tavanı aşıyorsa gövde okunmadan 413; uzunluk yoksa en çok tavan+1 bayt okunur, sunucunun gövde sınırı da tavana çekilir; (2) form okunur (okunamayan, iki kez verilen alan, dosya → imza tutmuyor sayılır) ve ucun `Verify`'ı imzayı denetler — tutmazsa 400 gövdesiz, seyreltilmiş güvenlik satırı, kayıt ve iş satırı yok; (3) kayıt ile iş satırı `ITransactionRunner` işleminde, "alındı" (200 + ucun kendi gövdesi) işlem kesinleştikten sonra; tekrar `AlreadyRecorded` → aynı yanıt; (4) yazımda ya da kesinleşmede veritabanı hatası yakalanmaz — uçtan tek yakalama katmanına çıkar, katmanın "Makine ucu" satırı gövdesiz 500 verir (`09 §8.1`; katman T212, D-107); yanıta hiçbir şey yazılmamıştır (düzeltme 1, B2).
- **Uç arayüzü** `ISignedEndpoint<TNotice>`: `Acknowledgement`, `Verify(formFields)`, `RecordAsync` (açık işleme yazar, kaydetmez; tekrar için `INSERT ... ON CONFLICT DO NOTHING`). Sonuç `InboundRecordOutcome` (`Recorded`, `AlreadyRecorded`).
- **Sabit zamanlı karşılaştırma** `Infrastructure/Integrations/SignatureComparison.Matches` (`CryptographicOperations.FixedTimeEquals`, UTF-8 bayt); boş/eksik imza asla tutmaz.
- **Seyreltilmiş satır** `SignatureFailureLog`: adres ekseni genel sınırınki (IPv4, IPv6 /64); ilk istek bir `SignatureFailureSampled` satırı (`reason`, `suppressedCount`), 60 sn içindekiler yalnız sayılır, sayı pencere sonrasındaki ilk satırdadır; IP satıra yazılmaz. Sayı yalnız o adresin pencere sonrasındaki ilk satırına yazılır, başka adresin isteğinin satırına hiç yazılmaz; süpürme yalnız sayısız bitmiş pencereyi düşürür, sayılı pencere adres dönene kadar bellekte kalır (düzeltme 1, B1; bellek tavanı D-108).
- **Sabitler** `SignedEndpointLimits`: `BodyMaxBytes = 64 × 1024`, `FailureLogWindow = 60 sn`; `09 §3.5` tablosuna satır.
- **Bağlama** `AddSignedEndpoints()` (`Program.cs`) ve `MapSignedEndpoint<TEndpoint,TNotice>(path)`: yalnız POST, `DisableAntiforgery()`. Gerçek bildirim ucu T311'de, PayTR imza hesabı T402'de bağlanır.
- **Kaynak taraması** `UnitTests/Architecture/SignatureComparisonSourceScan` + kasıtlı ihlal örneği `ViolationSamples/SignatureComparison/EarlyExitComparisons.cs` (11 ihlal biçimi; sabit zamanlı çağrı ve yorumlar bulgu değil).
- **Belgeler:** `09 §3.5` satırı, `§11.1` m.5 (ortak düzen, karşılaştırma, tarama), `§15.2` m.2 (makine ucunda `DisableAntiforgery()`), `§18.2` m.3 (alanlar; sayı yalnız o IP'nin satırında); `CONTEXT.md` klasör haritası; `DEFERRED_BACKLOG` D-107, D-108.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/SignedEndpoints/` (6 dosya), `Infrastructure/Integrations/SignatureComparison.cs`, `Program.cs`
- `tests/Shopfolio.UnitTests/SignedEndpoints/` (4), `Integrations/SignatureComparisonTests.cs`, `Architecture/SignatureComparisonSourceScan*.cs` + ihlal örneği, `Consistency/SignedEndpointLimitsConsistencyTests.cs`
- `tests/Shopfolio.IntegrationTests/SignedEndpoints/SignedEndpointDatabaseTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `.claude/CONTEXT.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`, `Docs/DEFERRED_BACKLOG.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İmza işleyiciden ve kuyruktan önce | ✓ | `SignedEndpointHandler.HandleAsync` sırası; `ValidSignature_IsVerifiedFirst_...` (`Verify, Begin, RecordAsync, Commit`); Integration `BrokenOrEmptySignature_...`/`MissingSignature_...` iş satırı 0 |
| 2 | Sabit zamanlı karşılaştırma | ✓ | `SignatureComparison.Matches` (`FixedTimeEquals`); `SignatureComparisonTests`; `SignatureComparisonSourceScanTests` (gerçek ağaç temiz + ihlal örneği 11 satır) |
| 3 | Tutmayan/imzasız → kayıt yok, işlenmez, 400 gövdesiz | ✓ | `BrokenOrEmptySignature_Gets400...`, `MissingSignature_Gets400...` (Unit + Integration: satır yok, iş satırı yok) |
| 4 | Seyreltilmiş güvenlik olayı | ✓ | `SignatureFailureLogTests` (sahte saat, pencere `FailureLogWindow`'dan); çok adresli `AnEndedWindowWithACount_WritesNoLineAtAnotherAddressesRequest_AndItsAddressesNextLineCarriesTheCount`, `ManyAddresses_EachCountIsWrittenOnlyOnItsOwnAddressesLine`; `RejectedRequest_WritesOneThinnedSecurityLine_...` (gövde/imza/IP yok) |
| 5 | Önce kayıt, kesinleşince "alındı" | ✓ | Unit sıra testi; Integration `ValidSignature_IsRecordedWithItsJobRow_AndReceivedOnlyAfterTheCommit` (kesinleşme anında dışarıdan 0 satır, yanıttan sonra 1 kayıt + 1 `InboundNotificationProcessing` iş satırı) |
| 6 | Tekrar kaydedilmez, işlenmez, "alındı" | ✓ | `SameNoticeAgain_...` (Unit + Integration: aynı durum ve gövde, tek kayıt, tek iş satırı) |
| 7 | Kaydedilemeyen → 500 | ✓ | "Alındı" yok, hata tek yakalama katmanına çıkar (500'ü katmanın makine ucu satırı yazar — `09 §8.1`; T212, D-107): Unit `DatabaseErrorOnTheWrite_/OnTheCommit_IsNotCaught_AndNothingIsWrittenToTheResponse` (istisna çıkar, yanıt başlamamış, gövde/tür/satır yok); Integration `DatabaseErrorOnTheCommit_LeavesTheEndpointForTheCatchingLayer_...` (kesinleşmede `NpgsqlException`), `RecordThatBreaksARule_LeavesTheEndpointForTheCatchingLayer_...` (`amount_not_negative`, SQLSTATE 23514) — kayıt ve iş satırı yok |
| 8 | Gelen kayıt kişisel alan taşımaz | ✓ | `InboundRecordFieldsTests` (alan → tür eşlemesi `05 §4.5.2`; kayıt ve `PaymentNotice` alan adları biçimleyicinin yasak listesinde değil; ihlal örneği `PayerCard`) |
| 9 | 64 KB tavan okumadan ve imzadan önce, 413; sahtecilik dışı | ✓ | `DeclaredLengthAboveTheLimit_Gets413WithoutReadingTheBody_...` (okunamayan akış), `UndeclaredBodyAboveTheLimit_...`, `BodyExactlyAtTheLimit_...`; Integration `BodyAboveTheLimit_Gets413...`; `MappedEndpoint_IsPostOnly_AndOutsideTheForgeryProtection`; Integration `TokenlessPost_IsHandledBySignedEndpoint_WhileTheProtectedFormOfTheSameAppIsRefused`; 429 T215 |

### Öz denetim (bitiş kapısı 2) — düzeltme 1'de bütünüyle yeniden denetlendi

Kaynak: `11` satır 931–951 (T203 kabul kriterleri ve kontrol listesi), `12` VAL-E045–E052, `09 §25` (satır 2324–2426); son diff `origin/main...HEAD`. SEH = `src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignedEndpointHandler.cs`, SFL = aynı klasörde `SignatureFailureLog.cs`.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 imza işleyiciden önce; bozuk/eksik imzada iş satırı yok | ✓ | SEH:40–59 (tavan → form → `Verify` → `RunAsync`); Unit `ValidSignature_IsVerifiedFirst_...` (`SignedEndpointHandlerTests.cs:26`); Integration `BrokenOrEmptySignature_*` (:234), `MissingSignature_*` (:244) — `jobs` 0 |
| KK2 sabit zamanlı işlev | ✓ | `Integrations/SignatureComparison.cs` (`FixedTimeEquals`); `SignatureComparisonSourceScanTests` (gerçek ağaç temiz + ihlal örneği 11 satır) |
| KK3 bozuk imza 400, gövde boş, satır yok | ✓ | SEH:50–54; `BrokenOrEmptySignature_*` (Unit :52, Integration :234) |
| KK4 güvenlik satırı seyreltilmiş (sayı yalnız o IP'nin satırında) | ✓ | SFL:70–89 (sayı yalnız `key`'in yeni satırına), SFL:94–108 (süpürme yalnız sayısız pencereyi düşürür, satır yazmaz); `SignatureFailureLogTests.cs:120` (A düşer, B pencere sonrası düşer → B'nin satırı 0, ek satır yok; A döner → A'nın satırı 1), :142 (üç adres, ters sırada dönüş, her biri kendi sayısı), :170 (sayısız 40 pencere düşer, sayılı 10 kalır); satır alanları yalnız `event`, `reason`, `suppressedCount` (:25, :107) |
| KK5 kesinleşmeden sonra yanıt | ✓ | SEH:59–63 (`RunAsync` döndükten sonra 200 + gövde); Integration `ValidSignature_IsRecordedWithItsJobRow_AndReceivedOnlyAfterTheCommit` (:198) |
| KK6 tekrar: ikinci satır yok, yanıt aynı | ✓ | Unit `SameNoticeAgain_*` (:38); Integration `SameNoticeAgain_*` (:218) |
| KK7 veritabanı hatasında 500 | ✓ | SEH:57–59 yakalamaz; hata tek yakalama katmanına çıkar, 500'ü katmanın "Makine ucu" satırı üretir (`09 §8.1` m.3; T212 m.6; D-107). Unit :116, :128 (istisna çıkar, `HasStarted` false, gövde/tür/satır yok → katman 500 yazabilir); Integration :271, :283 (kayıt ve iş satırı yok, ardışık düzenin `UnexpectedError` satırı yok) |
| KK8 kayıtta kişisel alan yok | ✓ | `InboundRecordFieldsTests.cs:39`, :47, :58 |
| KK9 64 KB üstü imzadan önce 413; sahtecilik denetimi yok; 429 T215 | ✓ | SEH:41–46, :71–99; Unit :149, :164, :180; Integration :255, :294, :305; `MappedEndpoint_*` (:193); 429 T215 m.4 |
| Güvenlik taraması | ✓ | Ret satırında gövde/imza/IP yok (`RejectedRequest_*` :98, `Line_CarriesNoAddress` :107); ardışık düzen artık hata satırı yazmaz (istisna iletisi katmanın maskelemesinden geçecek, `09 §8.1` m.7); tavan okumadan önce; tarama istisnası tek dosya |
| Sık FAIL listesi | ✓ | Kural değişince öteki yerler: `09 §18.2` m.3, `§11.1` m.5, `CONTEXT.md` SignedEndpoints satırı, rapor; karar kaydısız belge değişikliği yok (09'un yeni cümleleri `05 §4.5.2`/`07 §4.30.1`/`09 §8.1`'i yineler, yeni kural koymaz); yeni sabit yok (`09 §3.5` değişmedi; bellek tavanı D-108); geçici çözüm backlog'lu (D-107); testler `ManualTimeProvider` ile, gerçek saat yok |

**`09 §25` madde madde** (dokunulmayan alan "uygulanamaz" + gerekçe):

| Soru (kısaltılmış) | Cevap | Kanıt |
|---|---|---|
| Çekirdek ve Kaynak dokümanlar okunmuş mu | evet | `08 §3.2`, `05 §4.5.2`, §9.1.3, §11.2.9, `07 §4.30.1`, §2.8.2, `09 §3.5`, §8.1, §11.1, §15.2, §18.2 — bu raporda ve PR açıklamasında anılır |
| Yeni ad sözlükten/06/07'den birebir mi | evet | `SignatureFailureSampled`, `reason`, `suppressedCount` `09 §18.2` m.3'te; tablo adları `inbound_notification_records`, `jobs` `06`'dan |
| Kod hiçbir doküman cümlesiyle çelişmiyor mu | evet | B1: sayı o IP'nin ilk satırında (`05 §4.5.2` "o IP'nin yazılan ilk satırının alanı") — SFL:84–88; B2: geniş/yutan yakalama yok — SEH:57–59 |
| Diff yalnız görevin istediğini mi içeriyor | evet | düzeltme 1 diff'i: SFL, SEH, dört test dosyası, 09 iki cümle, CONTEXT bir hücre, backlog iki satır, rapor/status |
| "Bilgim tarihli" olgular doğrulanmış mı | evet | Dış varsayımlar tablosu 1–2; TestServer'ın yakalanmayan istisnayı istemciye taşıdığı Integration :271, :283'te görüldü |
| Yeni ayar/süre/tavan §3.5'te mi; `.env.example` | evet | iki sabit (`BodyMaxBytes`, `FailureLogWindow`) `09` satır 351'de, `SignedEndpointLimitsConsistencyTests`; düzeltme 1'de yeni sabit yok; ortam değişkeni yok, `.env.example` değişmedi |
| Dosya düzeni §4.1; `CONTEXT.md` güncel | evet | `Infrastructure/SignedEndpoints/`; `CONTEXT.md` satır 105 |
| Modül sınırları; mimari testler yeşil | evet | ortak altyapı modül türüne başvurmaz; Unit 1176/1176 (mimari testler dahil) |
| Aynı işi yapan tek işlev çağrılmış mı | evet | `ITransactionRunner`, `IJobQueue`, `LogEvents`, `SignatureComparison` tek yer |
| C# standartları, derleme ayarları | evet | `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 0; bastırma yok |
| Zaman yalnız `TimeProvider`'dan | evet | SFL:31, :73 (`_time.GetUtcNow()`); `DateTime.` yok (grep) |
| Yeni paket | uygulanamaz | `git diff --stat origin/main` `*.props`, `packages.lock.json`, `*.csproj` boş |
| Dış servis çağrısı; imza işleyiciden önce | evet | SEH:49 `Verify` kayıttan önce; sağlayıcı çağrısı yok |
| İstisna yalnız beklenmeyen hâlde; geniş `catch` altı yerin dışında yok | evet | SEH'de kalan üç `catch` belirli türdür ve bilinen reddi üretir (SEH:93 413 `BadHttpRequestException` süzgeci yalnız 413; :118 `InvalidDataException`, :122 `IOException` → okunamayan form, 400); veritabanı hatası yakalanmaz (B2) |
| Ön yüz kodu | uygulanamaz | sayfa/görünüm yok |
| Durum geçişi, işlem izi, eşzamanlılık, arka plan işi, para | uygulanamaz | geçiş, `AuditOperation`, kilit, yeni iş türü, para hesabı yok; iş satırı testte `IJobQueue.EnqueueAsync` ile aynı işlemde |
| Veri ve göç (FK, sayfalı liste, önbellek, göç, kullanıcı, değer) | uygulanamaz | göç, tablo, enum, kullanıcı, yük biçimi yok |
| Kayıtta sır ve kişisel veri yok | evet | ret satırı alanları yalnız `reason`, `suppressedCount`; IP yok (:107); testteki anahtar yer tutucu |
| Yeni POST sahtecilik korumasından geçiyor mu; dışlama gerekçeli | evet | `DisableAntiforgery()` yalnız `MapSignedEndpoint`'te, gerekçe `07 §2.5.5`, `09 §15.2` m.2; liste ve mimari testi T209 |
| Yetkilendirme sunucu tarafında | uygulanamaz | kimliksiz makine ucu, tek güvence imza |
| Yeni giriş yüzeyi hız sınırı kapsamında | evet | form gönderimi sınırı ve 429 T215 m.4'te planlı (`11` satır 939) |
| `SecretKey`, HTML temizleyici, yükleme, e-posta kaçışı, korumalı değer | uygulanamaz | bu alanlara dokunulmadı |
| Değişen kuralın Test maddesi karşılanmış mı | evet | `09 §18.2` m.3 → `SignatureFailureLogTests` :120, :142, :170; `09 §8.1` m.6 → Unit :116, :128 |
| Davranışı kilitleyen test; test silinmemiş/zayıflatılmamış | evet | sapmayı sabitleyen `AnEndedWindowWithACount_IsWrittenAtTheNextSweep_*` belgeye göre değiştirildi (:120); 500 testleri istisnanın çıktığını ve hiçbir şey yazılmadığını sınar |
| Doğru katman, sahteler `Fakes/`'ten, gerçek saat yok | evet | veritabanı davranışı gerçek PostgreSQL 18'de; `ManualTimeProvider` (`UnitTests/Fakes`) |
| Yeni yasak için tarama + ihlal örneği | evet | `SignatureComparisonSourceScan` + `EarlyExitComparisons.cs`; düzeltme 1'de yeni yasak yok |
| Eşitlik denetimleri | uygulanamaz | kütüphane tablosu, envanter, geçiş listesi değişmedi |
| Performans (sayfalı küme, döngüde sorgu, `.Result`) | evet | sorgu yok; `.Result`/`.Wait()` yok; süpürme pencere başına bir kez (SFL:96) |
| PR tek amaçlı; commit/PR kurala uygun | evet | commit `T203: düzeltme 1 — ...`; dal izolasyonu yalnız T203 |
| CI yeşil mi | — | CI'ı yönetici izler (işçi beklemez) |
| Yama sürümü göç, refactor | uygulanamaz | göç ve davranışsız refactor yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... -c Release --filter-not-trait "Consistency=true"` | 1025/1025 |
| Unit (Consistency) | ✓ | `... --filter-trait "Consistency=true"` | 150/150 |
| Integration | ✓ | `--filter-namespace Shopfolio.IntegrationTests.SignedEndpoints` / `.Web` | 10/10, 75/75 (geri kalan ad alanları CI'da) |
| Contract | — | — | yok |

Derleme: `dotnet build Shopfolio.slnx -c Release -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 0; `restore --locked-mode` 0.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-05, head `d5c004a`) |
| Bulgu sayısı | 2 (B1, B2 — ikisi de ORTA, S1) |
| Düzeltme gerekli mi | Evet — B1 ve B2 bu PR'da |
| Bloke-etmeyen not (K) sayısı | 2 (K1, K2 — düzeltme turunda) |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (iki kod sabiti, `09 §3.5`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T203-imzali-makine-ucu`
- **Commit:** PR head
- **PR:** #397
- **CI:** *(izlenmedi — yönetici izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `DisableAntiforgery()` uca `IAntiforgeryMetadata.RequiresValidation = false` yazar; `IgnoreAntiforgeryTokenAttribute` bu üst veriyi taşımaz | `MappedEndpoint_IsPostOnly_AndOutsideTheForgeryProtection` (öznitelikle denendi: üst veri bulunamadı) | ✓ `09 §15.2` m.2'ye yazıldı |
| 2 | PostgreSQL `ON CONFLICT DO NOTHING` (hedefsiz) iki kısmi benzersiz dizinin ikisini de karşılar | Integration `SameNoticeAgain_*` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gerçek bildirim ucu (`/sistem/odeme-bildirimi`), kayıt + iş satırı yazan uygulama | — (planlı: T311) | task kapsamı ortak altyapı |
| 2 | PayTR imza hesabı | — (planlı: T402) | sağlayıcı uygulaması |
| 3 | Koruma dışı uçlar listesi `AntiforgeryExemptions` ve mimari testi | — (planlı: T209 m.6, KK6) | sahtecilik korumasının task'ı |
| 4 | Bozuk imzanın form gönderimi sınırına sayılması, 429 | — (planlı: T215 m.4) | hız sınırının task'ı |
| 5 | İmzalı ucun beklenmeyen hatası tek yakalama katmanının "Makine ucu" satırına bağlanır (işaret, `UnexpectedOperationError` satırı, 500 sayacı, entegrasyon testi) | D-107 (T212) | katman T212'nin |
| 6 | Seyreltme belleğinin tavanı (taşma hâli belge kararı ister) | D-108 (T215) | tavan `05`/`07`'den ayrılır; hız sınırı sayaçlarıyla birlikte karara bağlanır |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** CI 42042657330 success, Publish 42042657292 success (T202 #396)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** iki yapım işçisiyle yapıldı (devir notu 1); düzeltme 1'den sonra 500 ardışık düzende üretilmez — hata tek yakalama katmanına çıkar (T212, D-107).

---

## Doğrulama Sonucu — T203 İmzalı makine ucu altyapısı — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T203-imzali-makine-ucu   **Commit:** d5c004a (doğrulanan head)

### Verdict: ✗ FAIL

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (switch öncesi ve sonrası, test koşularından sonra) |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42042657292 success · 42042657330 success · 42031439167 success |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22 T203 |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İmza bağlantı katmanında, işleyiciden ve kuyruktan önce | ✓ | `SignedEndpointHandler.HandleAsync` sırası: tavan → form → `Verify` → `RunAsync`; Unit `ValidSignature_IsVerifiedFirst_...` (`Verify, Begin, RecordAsync, Commit`); Integration `BrokenOrEmptySignature_*`, `MissingSignature_*`: `jobs` 0 satır |
| 2 | Sabit zamanlı karşılaştırma | ✓ | `Integrations/SignatureComparison.Matches` → `CryptographicOperations.FixedTimeEquals`; `src/` içinde başka `FixedTimeEquals`/imza karşılaştırması yok (grep); `SignatureComparisonSourceScanTests` (gerçek ağaç temiz, ihlal örneği 11 satır kırmızı) |
| 3 | İmzasız/tutmayan istek kaydedilmez, işlenmez, 400 gövdesiz | ✓ | Integration (gerçek PostgreSQL 18) `BrokenOrEmptySignature_Gets400...` ×2, `MissingSignature_Gets400...`: 400, boş gövde, kayıt 0, iş satırı 0 |
| 4 | Reddedilen istek seyreltilmiş güvenlik olayı olarak yazılır (`08 §3.2` ilke 2; `09 §18.2`) | ✗ | Tek adreste kural tutuyor (`SignatureFailureLogTests`). Süpürme ise iki sapma doğuruyor — bulgu B1: (a) bitmiş pencerenin sayısı adres geri dönmeden başka bir adresin isteğinde ayrı satır olarak yazılır, adres sonra döndüğünde ilk satırı `suppressedCount: 0` taşır (`SignatureFailureLog.cs:77`, `:90-93`, `:107-129`; dalın kendi testi `AnEndedWindowWithACount_IsWrittenAtTheNextSweep_...` satır 134-135 bunu sabitler) — `05 §4.5.2` "sayı ... o IP'nin yazılan ilk satırının alanı olur", `07 §4.30.1`, `VAL-E045` beklenen sonucu; (b) o ayrı satır, yazıldığı anki isteğin `requestId`/`method`/`path`'ini alır (`RequestLogEnricher.Enrich` satırı yazım anında `IHttpContextAccessor`'dan okur), yani başka adresin sayısı erişim kaydında yanlış IP'ye bağlanır — sınıfın kendi yorumu "the line's request id joins it to the access log" ile de çelişir |
| 5 | Önce kayıt, diske kesinleşince "alındı" | ✓ | Integration `ValidSignature_IsRecordedWithItsJobRow_AndReceivedOnlyAfterTheCommit`: kesinleşme anında dışarıdan 0 satır, yanıttan sonra 1 kayıt + 1 `InboundNotificationProcessing` iş satırı; `TransactionRunner` commit'ten sonra döner, yanıt ondan sonra yazılır |
| 6 | Tekrar kaydedilmez, işlenmez, yine "alındı" | ✓ | Integration `SameNoticeAgain_...`: aynı durum + gövde, tek kayıt, tek iş satırı (`ON CONFLICT DO NOTHING`) |
| 7 | Kaydedilemeyen bildirim "alındı" almaz, 500 | ✓ | Integration `DatabaseErrorOnTheCommit_Gets500...` (kesinleşmede `NpgsqlException`), `RecordThatBreaksARule_Gets500...` (`amount_not_negative`): 500, boş gövde, satır yok. Davranış tutuyor; 500'ün üretildiği yer `09 §8.1` ile çelişiyor — bulgu B2 |
| 8 | Gelen kayıt kişisel alan taşımaz | ✓ | `InboundRecordFieldsTests` (12 alanın her biri `05 §4.5.2` türüne eşli; kayıt ve `PaymentNotice` adları biçimleyicinin yasak listesinde değil; ihlal örneği `PayerCard`); ret satırında gövde/imza/IP yok (`RejectedRequest_WritesOneThinnedSecurityLine_...`), 500 satırında yalnız istisna türü (`ExceptionMasker`) |
| 9 | 64 KB tavan okumadan ve imzadan önce, 413; sahtecilik dışı; 429 T215 | ✓ | `ReadBodyWithinLimitAsync`: bildirilen uzunluk > tavan → okumadan null; Unit `DeclaredLengthAboveTheLimit_...` (okunamayan akış), `UndeclaredBodyAboveTheLimit_...`, `BodyExactlyAtTheLimit_...` ×2; Integration `BodyAboveTheLimit_Gets413...`; `MapPost` + `DisableAntiforgery()`: Unit `MappedEndpoint_IsPostOnly_...`, Integration `TokenlessPost_...` (aynı uygulamanın korumalı formu 400, imzalı uç 200), `OtherMethodThanPost_...`; 429 T215 m.4'te |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-E045 | KRİTİK | mock | BEKLEMEDE | FAIL (B1) | — |
| VAL-E046 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-E047 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-E050 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE (500 kısmı ✓; "son sorgu bulur" kısmı ödeme süresinin son sorgusunun task'ında) | — |
| VAL-E051 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-E052 | ORTA | mock | BEKLEMEDE | PASS | — |
| VAL-E036, E048, E049, E053, D082, A585 | — | — | BEKLEMEDE | BEKLEMEDE (T203'ün kriteri değil: DV-35 sandbox T402/T311; işaretli kayıt T311 m.5; hız sınırı T215 m.4; bildirimin tekilliği T311; ödeme oturumu `05 §3.6.3`) | — |

### Doğrulama Kontrol Listesi
- [x] KK1: imza işleyiciden önce; bozuk/eksik imzada iş satırı yok
- [x] KK2: sabit zamanlı işlev
- [x] KK3: bozuk imzada 400, gövde boş, satır yok
- [ ] KK4: güvenlik olayı satırı seyreltilmiş — süpürmeyle sayı adresin satırından kopuyor ve başka isteğe bağlanıyor (B1)
- [x] KK5: kesinleşmeden sonra yanıt
- [x] KK6: tekrarda ikinci satır yok, yanıt aynı
- [x] KK7: veritabanı hatasında 500
- [x] KK8: kayıtta kişisel alan yok
- [x] KK9: 64 KB üstü imzadan önce 413; sahtecilik denetimi bu uca uygulanmıyor; 429 T215 KK4'te
- [ ] `09 §25`: "İstisna yalnız beklenmeyen hâlde mi; ... hatayı yutan catch altı yerin dışında yok mu" ve "Kod hiçbir doküman cümlesiyle çelişmiyor mu" — hayır (B2, B1). Öteki satırlar evet: yeni sabit `§3.5`'te, klasör `CONTEXT.md`'de, zaman `TimeProvider`'dan, yeni paket yok, göç yok, kayıtta sır/kişisel veri yok, koruma dışı uç gerekçeli (`07 §2.5.5`)
- [x] Güvenlik taraması (aşağıda)

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 1175/1175 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (PostgreSQL 18, `shopfolio-t203-val-pg`) | ✓ | `--filter-namespace` ile: SignedEndpoints 10, Jobs 77, Web 75, Concurrency 26, Security 18, Database 35, Model 499 + 394 (iki parça), Logging + Disposal + FileSecrets 72 | 1206/1206, 0 hata |
| Restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; `git status` temiz; konteyner silindi |
| Dal CI | ✓ | `gh run list --branch task/T203-imzali-makine-ucu` | 42048534251 success (head d5c004a) |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (testteki HMAC anahtarı yer tutucu metin; kayıt satırlarında gövde/imza/anahtar/IP yok)
- Auth etkisi: Temiz (uç kimliksiz ve sahtecilik korumasının dışında — `07 §4.30.1`, §2.5.5; tek güvence imza)
- Input validation: Temiz (tavan okumadan önce; form değilse, iki kez verilen alan ya da dosya varsa 400; çatının form sınırları aşılırsa `InvalidDataException` → 400)
- Yeni bağımlılık: Yok

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| B1 | VAL-E045 | ORTA (eksik/yanlış kayıt — `12 §8.1`) | S1 | Beklenen: pencereden sonra gelen yeni geçersiz istek yeni satır doğurur ve önceki pencerenin sayısını kendi alanında taşır (`05 §4.5.2`, `07 §4.30.1`, `09 §18.2` m.3). Gerçekleşen: pencere bittikten sonra herhangi bir başka adresin isteği süpürmeyi tetiklerse sayı o başka isteğin satırı olarak yazılır (o isteğin `requestId`/`method`/`path`'iyle — `RequestLogEnricher`), adres dönünce satırı `suppressedCount: 0` taşır. Örnek: A t=0 ve t=1'de düşer; B t=61'de düşer → A'nın sayısı (1) B'nin istek kimliğiyle yazılır; A t=62'de düşer → 0. Dalın `09 §18.2` eklemesi ("IP dönmezse sayı bir sonraki süpürmede kendi satırıyla yazılır") `05`/`07`'yi karar kaydı olmadan değiştiriyor ve "kendi satırı" başka isteğe bağlanıyor. Bellek sınırı gerekçesi geçerli; çelişki ya karar kaydıyla belgelere girmeli ya da sayı adresin satırında kalacak, başka isteğe bağlanmayacak biçimde çözülmeli | VAL-E053 (aynı ret yolunun hız sınırı payı, T215) | bu PR'da düzeltilmeli | `src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignatureFailureLog.cs`, `tests/Shopfolio.UnitTests/SignedEndpoints/SignatureFailureLogTests.cs`, `Docs/09_CODING_GUIDELINES.md` §18.2 m.3 |
| B2 | — (`09 §25` "İstisna ... altı yer"; `09 §8.1` m.3, m.6, m.7) | ORTA (eksik kayıt/sayaç — `12 §8.1`) | S1 | Beklenen: makine ucunun beklenmeyen hatası tek yakalama katmanının "Makine ucu" satırına düşer (gövdesiz 500, `UnexpectedOperationError`, `05 §10.3.6` alanları, durum ucunun 15 dakikalık 500 sayacı); işleyici beklenmeyen hatayı yakalayıp yutmaz (m.6'nın "Yanlış" örneği: "yakalama katmanı onu hiç görmez, kayıt ve sayaç eksik kalır"). Gerçekleşen: `SignedEndpointHandler` `DbUpdateException` ve `DbException`'ı yakalayıp kendi `UnexpectedError` satırını yazıyor ve 500 dönüyor (`SignedEndpointHandler.cs:62-75`, `:151-158`); T212'nin katmanı geldiğinde bildirimin kaydedilemediği 500'ler katmana ve sayaca hiç ulaşmaz. Yapım raporu bunu "T212 gelene kadar" diye anıyor ama kayıtlı bir borç yok (backlog'da T203 kaydı yok). Düzeltme yolu: yakalamayı kaldırmak (katman yokken Kestrel zaten gövdesiz 500 döner; test barındırıcısı katmanın makine ucu satırını taklit eden ince bir ara katmanla ya da istisnanın yayıldığını ve hiçbir şeyin yazılmadığını sınayarak), ya da katmanın görmesini sağlayacak biçimde yeniden atmak | VAL-E050 | bu PR'da düzeltilmeli | `src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignedEndpointHandler.cs`, `tests/.../SignedEndpointHandlerTests.cs`, `tests/.../SignedEndpointDatabaseTests.cs` |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Seyreltme belleğinin sayı tavanı yok: bellek son iki pencerede düşen farklı adres sayısıyla sınırlı (süpürme ≥ 60 sn'de bir, yalnız ret geldiğinde). Zamanla büyümüyor (`EndedWindows_AreDroppedAtTheSweep_...` 50 → 1), ama çok sayıda farklı IPv6 /64'ten gelen kısa bir taşkında iki dakikalık adres kümesi kadar büyür; genel hız sınırının sayaçlarıyla aynı nitelik (`05 §11.2.9`). B1'in düzeltmesi süpürmeyi değiştireceği için aynı turda değerlendirilir | düzeltme turunda |
| K2 | Öz denetim tablosunun `09 §25` satırının kanıtı yalnız "PR açıklaması"; madde madde değil | düzeltme turunda |

### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Kriter 4 / KK4: rapor ✓ ve "sayı kaybolmaz" diyor; sayı kaybolmuyor ama adresin satırından kopuyor ve başka isteğin kimliğine bağlanıyor (B1). Raporun kanıt testi (`AnEndedWindowWithACount_...`) sapmayı sabitliyor.
- Kriter 7 / `09 §25`: rapor ✓; 500'ün işleyicide üretilmesi `09 §8.1` m.6–7 ile çelişiyor ve ertelemesi kayıtsız (rapor Notlar: "500 ayrımı T212'nin yakalama katmanı gelene kadar işleyicide") (B2).
- Öz denetim tablosu var, her maddesi kanıtlı; `09 §25` satırının kanıtı dolaylı (K2).
- Geri kalan 8 kriter ve KK'lar bağımsız doğrulamayla aynı sonuçta.

### Merge ve Post-merge
- Squash: — (FAIL; merge yok)
- Post-merge run'lar: —

---

## Düzeltme 1 — tur 1 bulgularının kapanışı (2026-11-05)

| # | Bulgu | Kapanış | Kanıt |
|---|---|---|---|
| B1 | Süpürme, bitmiş pencerenin sayısını başka adresin isteğinde yazıyor | Davranış `05 §4.5.2` / `07 §4.30.1`'e uyduruldu: sayı yalnız o IP'nin pencere sonrasındaki ilk satırının alanıdır. Süpürme artık satır yazmaz; yalnız sayısız bitmiş pencereyi düşürür (onunla bir şey kaybolmaz — adresin sonraki satırı zaten 0 taşır), sayılı pencere adres dönene kadar kalır. Pencerenin son nedeni (`LastReason`) gereksizleşti, silindi. `09 §18.2` m.3'teki aykırı cümle ("IP dönmezse sayı bir sonraki süpürmede kendi satırıyla yazılır") belgeye uyan cümleyle değişti; karar kaydı gerekmez, yeni kural konmadı | `SignatureFailureLog.cs` `Record`, `Sweep`; sapmayı sabitleyen test belgeye göre değişti → `AnEndedWindowWithACount_WritesNoLineAtAnotherAddressesRequest_AndItsAddressesNextLineCarriesTheCount` (A düşer, B pencere sonrası düşer → B'nin satırı 0, A'nın satırı yazılmaz; A döner → A'nın satırı 1); yeni `ManyAddresses_EachCountIsWrittenOnlyOnItsOwnAddressesLine`; `EndedWindows_AreDroppedAtTheSweep_*` → `EndedWindowsWithoutACount_AreDroppedAtTheSweep_AndThoseWithACountStayUntilTheirAddressComesBack` |
| B2 | İşleyici veritabanı hatasını yakalayıp kendi 500'ünü yazıyor | `DbUpdateException` / `DbException` yakalaması ve ardışık düzenin `UnexpectedError` satırı silindi; hata uçtan çıkar, yanıt başlamamıştır (`09 §8.1` m.3, m.6–7). Katman yokken Kestrel gövdesiz 500 verir; katmanın makine ucu satırı, kaydı ve sayacı T212'nin payıdır → **D-107** (hedef T212). `09 §11.1` m.5 ve `CONTEXT.md` aynı cümleyle güncel | Unit `DatabaseErrorOnTheWrite_IsNotCaught_*`, `DatabaseErrorOnTheCommit_IsNotCaught_*` (istisna çıkar, `HasStarted` false, gövde/tür/satır yok); Integration `DatabaseErrorOnTheCommit_LeavesTheEndpointForTheCatchingLayer_*`, `RecordThatBreaksARule_LeavesTheEndpointForTheCatchingLayer_*` (istisna istemciye çıkar; kayıt 0, iş satırı 0; `UnexpectedError` satırı yok) |
| K1 | Seyreltme belleğinin tavanı | **Karar: tavan konmadı.** Her tavan taşma hâlinde ya sayıyı düşürür ya da seyreltmeyi bırakır; ikisi de `05`/`07`'den ayrılır ve karar kaydı ister. Bellek: son pencerede düşen adresler + sayısı bekleyen (60 sn'de en az iki kez düşüp dönmeyen) adresler; yeniden başlatmada sıfırlanır (`05 §11.2.9`, hız sınırı sayaçlarıyla aynı nitelik). Tavan sorusu **D-108**'de (hedef T215): genel sınırın bellek sayaçlarıyla aynı eksende karara bağlanır; gerekirse önce `05 §4.5.2`'ye taşma hâli, `09 §3.5`'e sabit, testi | `SignatureFailureLog.cs` `Sweep`; `EndedWindowsWithoutACount_*` (50 adres → 10 sayılı + 1 kalır) |
| K2 | Öz denetimin `09 §25` satırı madde madde değil | Öz denetim tablosu bütünüyle yeniden denetlendi; `09 §25`'in her sorusu ayrı satırda cevap + kanıtla | Yukarıda "`09 §25` madde madde" |

**Testler (düzeltme 1 head'i):** build `-warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` çıkış 0; Unit 1176/1176; Integration (PostgreSQL 18 konteyneri `shopfolio-t203-fix1-pg`, sonra silindi) SignedEndpoints 10/10, Logging 30/30, Web 75/75.
