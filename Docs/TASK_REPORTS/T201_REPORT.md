# T201 — İmha düzenleyicisi, süre ayarları ve gecelik temizlik

**Faz:** F2 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Disposal/`: `DisposalStep` (gecelik işin ikinci adımı; parçaları kayıt sırasıyla koşar), `DisposalPart` (`IDisposalPart`, `DisposalRun.ChunksAsync` / `RecordAsync`: parça başına tek işlemde silme + imha kaydı), `DisposalRegistration` (`AddDisposal`, `AddDisposalPart<T>`), `DisposalTimings` (parça boyu 500, misafir sepeti 400 gün).
- Parçalar: işlem izi (veritabanı fonksiyonu `dispose_expired_audit_logs`), kayıt dosyaları (`LogFileDisposalPart`, `LogFileRetention`), deneme kayıtları, oturumlar, tanınan tarayıcı işaretleri, geri alma bağlantısı adresi (altyapıda); misafir sepeti ve yönetici daveti kendi modüllerinde (`Modules/Ordering/GuestCartDisposalPart.cs`, `Modules/Administration/AdminInvitationDisposalPart.cs`), modülün kaydında `AddDisposalPart` ile eklenir.
- `ProductConstants`: P-15, P-16, P-31..P-36, P-45, P-49 sabitleri. `09 §3.5` tablosuna iki satır (imha parçası 500, misafir sepeti 400 gün).
- `DEPLOY_RUNBOOK` L.3: imha dosya gününü addan değil son yazım anından okur (D-91 K1). D-59 test tarihleri düzeltildi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Disposal/*`, `Infrastructure/ProductConstants.cs`, `Program.cs`, `Modules/Ordering/{GuestCartDisposalPart,OrderingModule}.cs`, `Modules/Administration/{AdminInvitationDisposalPart,AdministrationModule}.cs`
- Testler: `UnitTests/Architecture/DisposalNumber*`, `UnitTests/Consistency/DisposalTimingsConsistencyTests`, `UnitTests/Disposal/LogFileRetentionTests`, `IntegrationTests/Disposal/{DisposalHarness,DisposalTests}.cs`, `IntegrationTests/Model/AdminAndAuditTablesTests.cs`
- Belgeler: `Docs/09_CODING_GUIDELINES.md` §3.5, `Docs/DEPLOY_RUNBOOK.md` L.3, `Docs/DEFERRED_BACKLOG.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Her gece 03:00 TR, envanter sırayla | ✓ | `DisposalTests.TheNightlyJob_DoesNotRunAMinuteBefore0300InTurkey_AndRunsAt0300`; `TheNightlyJob_RunsTheQueueCleanupBeforeTheDisposal`; `DisposalRegistration` |
| 2 | Süre kaydın değerinden hesaplanır, kaçırılan gece telafi | ✓ | `AMissedNight_IsMadeUp_WithBothNightsRecords_AndASecondRunChangesNothing` |
| 3 | Süre değerleri tek evinde | ✓ | `ProductConstants`; `DisposalTimings` + `09 §3.5`; `DisposalNumberScanTests`; `DisposalTimingsConsistencyTests` |
| 4 | Fiziksel silme, işaret sütunu yok | ✓ | `NoTableHasADeletedMarkColumn_AndTheDisposalRecordHoldsOnlyTypeNumberAndDates`; parçalar `ExecuteDeleteAsync` |
| 5 | Anlık silmeden yalnız Z-44 imha kaydı yazar, silme işleminde | ✓ | `DisposalRun.ChunksAsync` (silme ve kayıt tek işlem); T201 anlık silme eklemez |
| 6 | Temizlik aynı çalışmada, tür ve sayıyla yazılır | ✓ | `DisposalTests.Sessions_*`, `AttemptRecords_*`, `GuestCarts_*`, `AdminInvitations_*`, `ARevertLinksAddress_*` |
| 7 | Kalıcı kayıt, bağlantı kaydı, "başarısız" iş satırı yerinde | ✓ | `AFailedJobRow_IsLeftInPlace`; `ARevertLinksAddress_IsEmptied...` (satır kalır); `GuestCarts_...AMembersCartStays` |
| 8 | İz satırını yalnız veritabanı fonksiyonu siler | ✓ | `AuditLogDisposalPart`; `AuditLog_ARowPastTenYearsGoes...`; `TheApplicationUser_CannotDeleteFromTheAuditTrailItself` |
| 9 | Kayıt dosyaları, sayı dosyadan önce, sunucu döngüsü aralıkla | ✓ | `LogFiles_AreJudgedByTheirLastWrite...`; `TheServerLogRotation_IsWrittenAsARange...`; `LogFileDisposalPart` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 03:00 TR tetik, 02:59:59 tetiklemez | ✓ | `TheNightlyJob_DoesNotRunAMinuteBefore0300InTurkey_AndRunsAt0300` |
| KK2 iki gece atlanınca üçüncüde ikisi, ikinci koşu değişmez | ✓ | `AMissedNight_IsMadeUp_...` |
| KK3 süre tek evinde, imha işinde sayı yok | ✓ | `DisposalNumberScanTests` (ayrı ihlal örneği `ViolationSamples/Disposal/DisposalNumbers.cs`; iki modül parçası da taranır); `DisposalTimingsConsistencyTests`; `AttemptWindows_AreTheProductConstants` |
| KK4 silinen satır yok, işaret sütunu yok | ✓ | `NoTableHasADeletedMarkColumn_...`; oturum/sepet testleri kalan kümeyi okur |
| KK5 imha kaydı hata verince silme geri alınır | ✓ | `WhenTheDisposalRecordIsRefused_TheDeletedRowsComeBack` |
| KK6 kayıt tür ve sayı taşır, değer taşımaz | ✓ | `NoTableHasADeletedMarkColumn_AndTheDisposalRecordHoldsOnlyTypeNumberAndDates` (sütun kümesi) |
| KK7 kalıcı kayıt, bağlantı kaydı, başarısız iş yerinde | ✓ | `AFailedJobRow_IsLeftInPlace`; süresi dolmuş `EmailVerification` bağlantı kaydı `ARevertLinksAddress_...` testinde yerinde |
| KK8 uygulama kullanıcısı iz satırını silemez | ✓ | `TheApplicationUser_CannotDeleteFromTheAuditTrailItself` (42501) |
| KK9 eski dosya silinir, sayı kayda girer, döngü satırı aralıkla | ✓ | `LogFiles_*`, `TheServerLogRotation_*` |
| `09 §25`: süre `§3.5` tablosunda, zaman `TimeProvider`'dan, arka plan işi kalıba uyar | ✓ | `09 §3.5` iki satır; testlerde `ManualTimeProvider`, gerçek saat/Stopwatch yok; `DisposalStep : INightlyStep` (`AddNightlyStep`) |
| Sık FAIL: belgede olmayan süre sabiti | ✓ | Her sayı `06 §9.1/§9.4`, `05 §11.1.32` veya `02 §11.2` kaynaklı; `DisposalTimings` iki satırı `09 §3.5`'te |
| Sık FAIL: beklenen değer sabitten türetilir | ✓ | Testler `ProductConstants.*`, `AttemptRecordDisposalPart.WindowMinutes`, `DisposalTimings.*`, `LogFileRetention.Cutoff` kullanır |
| Sık FAIL: öteki yerler (09, STATUS, MEMORY, README, DEPLOY_RUNBOOK) | ✓ | `09 §3.5`, `IMPLEMENTATION_STATUS`, `STATUS_CHANGELOG`, `MEMORY`, `DEPLOY_RUNBOOK` L.3; README bu sayıları anmaz |
| Sık FAIL: koleksiyon yalıtımı, gerçek saat | ✓ | `[Collection("Disposal")]` kendi veritabanı + kendi geçici dizin; `TRUNCATE` yalnız bu veritabanında; sahte saat |
| Backlog D-59, D-91 K1, D-97, D-99 | ✓ | D-59 ✅, D-91 K1 ✅, D-97 gerekçeli, D-99 T201 payı ✅; yeni D-102, D-103 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 1026/1026 |
| Integration | ✓ | `--filter-class "*DisposalTests"`, `*AdminAndAuditTablesTests` | 18/18, 42/42 |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-05, bağımsız doğrulayıcı; dal head `bed034e`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1, K2 → `DEFERRED_BACKLOG` D-104 |

### Doğrulama tur 1 — validator kanıtları

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `scripts/git-hooks`; ana dal CI son 3 run `success` (42021631131, 42021631103, 41999269502); `MEMORY.md` satır 22 T201'i anar.

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | 03:00 TR, envanter sırayla; parçalar T301/T302'de (KK1) | ✓ | `DisposalStep : INightlyStep`, kendi zamanlayıcısı ve kilidi yok; `Program.cs` `AddDisposal` `AddScheduler`'dan sonra → `JobQueueCleanup` önce; `TheNightlyJob_DoesNotRunAMinuteBefore0300InTurkey_AndRunsAt0300` (02:59:59 koşmaz, 03:00 koşar, başarı anı yazılır); T301/T302 için `AddDisposalPart<T>` kayıt noktası (D-99) |
| 2 | Süre kaydın değerinden; kaçırılan gece telafi (KK2) | ✓ | Parçalar `run.Now`'a karşı kaydın alanını okur (`LastActivityAt`, `CreatedAt`, `ExpiresAt`, `LastAccessedAt`); "son çalışma" okunmaz. `AMissedNight_…` (iki gecenin kaydı, ikinci koşu değişmez). Validator geçici testi: süresi dolmuş + bir günü kalan iki işaret, saat bir gün ileri, iş yalnız `RunIfDueAsync` kararıyla → yalnız dolan silindi, `RecognizedBrowserMark` satırı 1, `CreatedAt` çalışmanın anı (4/4 yeşil; test silindi) |
| 3 | Süre tek evinde (KK3) | ✓ | P-15, P-16, P-31…P-36, P-45, P-49 `ProductConstants`'ta, değerler `02 §11.2` ile eşit (2 sa, 30 g, 15/60/60/60/15/15 dk, 6 ay, 60 dk); 500 ve 400 gün `DisposalTimings` + `09 §3.5` iki satır + `DisposalTimingsConsistencyTests`; iz süresi DB fonksiyonunda (`06 §9.1.2` istisnası); P-14/P-47/P-48 kaydın `ExpiresAt`'inden okunur. `DisposalNumberScanTests` (ihlal örneği 6 bulgu). `src`'de 2 sa/30 g/400 g'nin başka evi yok (grep) |
| 4 | Fiziksel silme, işaret sütunu yok (KK4) | ✓ | Bütün parçalar `ExecuteDeleteAsync` (geri alma adresi `ExecuteUpdateAsync` boşaltma, `06 §9.4.3`); `NoTableHasADeletedMarkColumn_…` |
| 5 | Anlık silmeden yalnız Z-44 listesi kayıt yazar, silmenin işleminde | ✓ | `DisposalRecords`'a yalnız `Infrastructure/Disposal/` yazar (grep); kişisel verisiz `JobCleanup` satır yazmaz (`AFailedJobRow_…` sayılı kayıt boş); Z-44 listesinin anlık silmeleri kendi task'larında (`11` T3xx: hesap silme, IBAN, bekleyen kayıt, yönetici kaldırma) |
| 6 | Temizlik aynı çalışmada, tür ve sayıyla (KK6) | ✓ | `06 §9.4.3` tablosunun altı kişisel veri satırı parça (deneme, oturum, işaret, misafir sepeti, davet, geri alma adresi); `DigitalFile` → D-102; `Job` T221; tür+sayı testlerde; `disposal_records` sütunları yalnız tür/sayı/tarih |
| 7 | Kalıcı kayıt, bağlantı kaydı, "başarısız" iş yerinde (KK7) | ✓ | Parçalar kalıcı ve ticari tablolara dokunmaz (kod okuması); `AFailedJobRow_IsLeftInPlace`; süresi dolmuş `EmailVerification` ve geri alma satırı kalır; üye sepeti kalır. Validator geçici testi: dört davet durumu (kabul, geri çekme, geçersiz, ömrü dolan) bağlantısıyla gitti, sayı 4; bekleyen davet ve yöneticinin `PasswordReset` bağlantısı yerinde |
| 8 | İz satırını yalnız DB fonksiyonu siler (KK8) | ✓ | `AuditLogDisposalPart` → `dispose_expired_audit_logs(max, as_of)`; `TheApplicationUser_CannotDeleteFromTheAuditTrailItself` (42501); `AdminAndAuditTablesTests` (D-59 tarihleri gerçek saate göre) yeşil |
| 9 | Kayıt dosyaları; satır dosyadan önce; döngü satırı aralıkla (KK9) | ✓ | `LogFileDisposalPart`: `RecordAsync` önce, `File.Delete` sonra; dosya yaşı son yazımdan (D-91 K1); yalnız `app/audit/command-yyyyMMdd.jsonl` ve `access.log-yyyyMMdd`, `access.log` ve yabancı dosya kalır; `ServerLogRotation` satırı sayısız, `RangeStart/EndDate`; atlanan günleri tek aralıkla doldurur, aynı günü iki kez yazmaz; ay sonu kırpması `LogFileRetentionTests`'te |

Ek sınama (validator geçici testi): 1201 süresi dolmuş deneme → üç işlem, imha satırları 500 + 500 + 201; taze L1 ve 59 dakikalık L2 satırı kaldı. Misafir sepeti 399 gün kaldı, 401 gün gitti.

**`09 §25` (ilgili satırlar):** süre/tavan §3.5 tablosunda; zaman `TimeProvider`'dan (`run.Now`); arka plan işi kalıba uyar (`AddNightlyStep`, geniş `catch` yok, `Jobs` tablosuna dokunulmaz); parça modülün kaydında (`09 §6` madde 2); yabancı anahtar davranışları açık (sepet kalemi, davet bağlantısı `Cascade`); yeni paket, göç, uç yok.

**Güvenlik:** sır yok; yetki etkisi yok (uç yok; iz silme yetkisi uygulama kullanıcısında yok); dosya silme yalnız iki yapılandırılmış dizinde, özyinelemesiz ve ad desenli; yeni bağımlılık yok.

**Task satırları (`12 §7.5`):** `KP-74` grep'i dokuz satır getirir. `VAL-D046` ve `VAL-D047` T201'in kriter 1, 2, 6'sına eşlenir → PASS. `VAL-D048`, `D049`, `D050` (hesap/IBAN/bekleyen kayıt silmeleri, T3xx), `VAL-D051` (imha kaydının üç yılı, T302) ve `VAL-A368`–`A370` (aydınlatma ekranları) eşlenmez → `BEKLEMEDE`. `KD-56` satırı yok. Kaynakları gecelik temizliği anan satırlar grep'le bulundu: `VAL-D078`, `VAL-D117`, `VAL-D118` PASS; `VAL-D080`'in T201 payı (adres boşalır, `LinkRecord` satırı, kayıt kalır) yeşil, "bağlantı açılınca geçersiz" geri alma akışının task'ında → `BEKLEMEDE`; `VAL-B128`/`D123` (yükleme, D-102), `VAL-D085` (T302), `VAL-D002` (sepet) eşlenmez.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-D046 | KRİTİK | yok | BEKLEMEDE | PASS | — |
| VAL-D047 | KRİTİK | yok | BEKLEMEDE | PASS | — |
| VAL-D078 | ORTA | yok | BEKLEMEDE | PASS | — |
| VAL-D117 | ORTA | yok | BEKLEMEDE | PASS | — |
| VAL-D118 | ORTA | yok | BEKLEMEDE | PASS | — |

`python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77`: Hata 0.

**K-notları:** (K1) `VAL-D078`'in geri çekilen/geçersiz davet ve yöneticinin bağlantı kaydı, `VAL-D046`'nın birleşik ön koşulu ve 500'lük parça sınırı depoda testsiz; davranış validator geçici testiyle yeşil → D-104. (K2) `DisposalRegistration` yorumu üretimdeki parça sırasını yanlış anlatır (modül parçaları önce koşar); kural ihlali değil → D-104.

Testler (validator, yerel, head `bed034e`): build `-warnaserror` 0/0 · `dotnet format --verify-no-changes` 0 · Unit 1026/1026 (Consistency dahil) · betik testleri 4/4 · Integration 1192/1192 (Disposal 18, Jobs 77, Web 72, Concurrency 26, Security 18, Database 35, FileSecrets 24, Logging 30, Model 496 + 396; PG 18 konteyneri, koşu sonrası silindi) · `git status` temiz · `dotnet restore --locked-mode` 0. Dal CI 42025909535 `success` (head `bed034e`).

**Yapım raporu karşılaştırması:** kriter ve öz denetim tabloları her kriter ve KK için kanıtlı; verdict uyumlu. Uyuşmazlık: yapım raporunda task satırı (`12`) eşlemesi yok; KK7 kanıtı davetin dört durumunu ve kalıcı kayıtları saymaz (K1). Integration'ı yapım yalnız iki sınıfla koştu; validator tamamını koştu.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (iz fonksiyonu T106'dan)
- **Config / ortam değişkeni:** Yok (erişim kaydı dizini sabit varsayılan `/var/log/shopfolio/access`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T201-imha-duzenleyicisi`
- **PR:** 395
- **CI:** doğrulama başlatılırken izlenir

## Dış Varsayımlar

Dış varsayım: yok. Veritabanı fonksiyonu ve tablolar T106/T225/T221'den, yerel Postgres 18 konteyneriyle sınandı.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Dosya silen imha parçaları (DigitalFile yüklemesi, Image/DigitalFile/OrphanFile) | D-102 | Dosya deposu T223'tedir |
| 2 | Hız sınırı sayıları pencere sabitlerini kullanacak | D-103 | Sayım T215'tedir |
| 3 | Sunucu kayıt süreleri ve veritabanı hata cümlesi | D-91 K2, K3 | Sahibi başka task |
| 4 | Doğrulama K1–K2: üç test boşluğu, kayıt sırası yorumu | D-104 | Davranış doğru; test T302'nin imha testleriyle eklenir |
