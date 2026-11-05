# T201 — İmha düzenleyicisi, süre ayarları ve gecelik temizlik

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

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
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (iz fonksiyonu T106'dan)
- **Config / ortam değişkeni:** Yok (erişim kaydı dizini sabit varsayılan `/var/log/shopfolio/access`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T201-imha-duzenleyicisi`
- **PR:** #PRNO
- **CI:** doğrulama başlatılırken izlenir

## Dış Varsayımlar

Dış varsayım: yok. Veritabanı fonksiyonu ve tablolar T106/T225/T221'den, yerel Postgres 18 konteyneriyle sınandı.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Dosya silen imha parçaları (DigitalFile yüklemesi, Image/DigitalFile/OrphanFile) | D-102 | Dosya deposu T223'tedir |
| 2 | Hız sınırı sayıları pencere sabitlerini kullanacak | D-103 | Sayım T215'tedir |
| 3 | Sunucu kayıt süreleri ve veritabanı hata cümlesi | D-91 K2, K3 | Sahibi başka task |
