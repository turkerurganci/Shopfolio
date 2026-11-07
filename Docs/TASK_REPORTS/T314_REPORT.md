# T314 — Durum ucu: uyarı koşulları ve 500 sayacı

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Uç** (`Endpoints/StatusEndpoint`): `GET /sistem/durum`; koşul yoksa 200, varsa 503; gövde boş, `no-store`; imzasız; `MachineEndpointMetadata` taşır (D-139 durum ucu payı). Açık koşulun adı yanıtta yoktur.
- **Koşullar** (`Infrastructure/Status/`): saf kurallar `StatusRules` (eşikler `StatusThresholdOptions`, kurulum ayarı), koşullar `StatusConditions` (disk, işlem günlüğü aktarımı, tam yedek, dosya yedeği, nabız, gece işi, beklenmeyen hata, ödeme başlatma dizisi, sertifika), okuyucular `StatusSources`, değerlendirici `StatusEvaluator` (bilinen okuma hata türleri = açık koşul; bilinmeyen istisna yakalama katmanına çıkar), bellekteki kayıtlar `StatusMemory` (`PaymentStartFailureSeries`, `CertificateExpiryMemory`).
- **İş kuyruğu koşulu** `Infrastructure/Jobs/JobQueueStatusCondition` (iş tablosu yalnız `Jobs/` altında); **kanal uyarısı** `Modules/Notifications/ChannelWarningCondition` (5 hata + 15 dk).
- **D-147:** `UnexpectedErrorLayer.Resolve` istemci kaynaklı hataları ayırır: `ConnectionResetException` → `ClientGone`; `BadHttpRequestException` → kendi durum koduyla gövdesiz, `no-store` (`ErrorResolution.ClientStatus`); ikisi de hata satırı yazmaz, sayacı artırmaz.
- **Belgeler:** `09 §8.1` m.3 (durum ucunu T314 yazdı) ve m.8 (istemci kusuru, D-147); `CONTEXT.md` (`Status/` ve Errors satırı); backlog D-147 ✅, D-139 payı daraldı (aktif 93 → 92).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/`: `Endpoints/StatusEndpoint.cs`; `Infrastructure/Status/*` (yeni, 6 dosya); `Infrastructure/Jobs/JobQueueStatusCondition.cs`; `Modules/Notifications/ChannelWarningCondition.cs`, `NotificationsModule.cs`; `Infrastructure/Errors/UnexpectedErrorLayer.cs`, `UnexpectedErrorSetup.cs`; `Program.cs`
- `tests/Shopfolio.UnitTests/Infrastructure/`: `StatusRulesTests.cs`, `StatusEvaluatorTests.cs`, `UnexpectedErrorLayerTests.cs`
- `tests/Shopfolio.IntegrationTests/Web/`: `StatusEndpointTests.cs`, `RequestLimitsTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /sistem/durum`, 200/503, gövde boş, `no-store` | ✓ | `Get_WithNoOpenCondition_Is200_EmptyBody_NoStore_*`, `Get_WithOneConditionOpen_Is503_*` |
| 2 | Koşul adı yanıtta yok; imzasız | ✓ | `Get_WithSeveralConditionsOpen_Is503_AndTheAnswerIsTheSameAsWithOne_KK2`; ilk test imza/oturumsuz |
| 3 | Disk %85, aktarım 10 dk / 10 dosya | ✓ | `Disk_BelowTheThreshold_*`, `LogShipping_*` (6 test), `ThePostgresLogShippingReader_ReadsTheArchiverStatistics_*` |
| 4 | Tam yedek ve dosya yedeği 26 saat; yerel kopya başarı değil | ✓ | `Backup_WithinTheAge_*`, `EachBackup_OpensTheStatusAlone_*`, `TheFileBackupReader_*`, `StatusFile_*` |
| 5 | Nabız 5 dk | ✓ | `Heartbeat_WithinTheAge_*` |
| 6 | Kanal uyarısı | ✓ | `ChannelWarning_*` (3), `TheChannelWarning_ClosesWhenTheCountIsReset_KK6` |
| 7 | 10 hata / 15 dk, yalnız 500; sayaç bellekte | ✓ | `Errors_OneBelowTheCountIsClosed_*`, `ErrorCounter_CountsOnlyTheWindow_*`, `NineFailedRequestsAreClosed_TheTenthOpens_AndANotFoundIsNotCounted_KK7` |
| 8 | Ödeme dizisi 5 hata + 15 dk, ilk başarıda kalkar; sertifika 14 gün | ✓ | `PaymentSeries_*` (3), `PaymentSeriesMemory_*`, `ThePaymentSeries_*_KK8`, `Certificate_*`, `NotAfterOf_*`, `CertificateMemory_*` |
| 9 | İş kuyruğu 15 dk / başarısız satır; imha 26 saat | ✓ | `JobQueue_*` (3), `NightlyJob_*`, `AJobNotYetDue_AndAFreshDueJob_AreClosed_KK9` |
| 10 | Eşikler ayardan; okunamayan koşul açık; canlılık ucuna girmez | ✓ | `ChangingTheThreshold_MovesTheRule`, `TheDiskThreshold_IsTheOneInTheSettings_KK3_KK10`, `ASourceThatCannotBeRead_*`, `ADatabaseConditionThatCannotBeRead_*`, `TheLivenessEndpoint_StaysAt200_*` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–10 | ✓ | yukarıdaki tablo; önceki işçinin kodu dahil yeniden denetlendi |
| KK1 koşulsuz 200, koşulda 503, gövde boş | ✓ | `StatusEndpointTests` ilk iki test |
| KK2 yanıtta koşul adı yok | ✓ | `...Is503_AndNamesNothing_KK1_KK2` |
| KK3 disk ve aktarım sahte ölçümle | ✓ | `FakeDisk`, `FakeLogShipping`; `StatusRulesTests` |
| KK4 iki yedek ayrı açar | ✓ | `EachBackup_OpensTheStatusAlone_*` |
| KK5 nabız 5 dk | ✓ | `Heartbeat_WithinTheAge_*` |
| KK6 kanal uyarısı açık/kapalı | ✓ | `TheChannelWarning_ClosesWhenTheCountIsReset_KK6` |
| KK7 dokuz kapalı, on açık; 404/409 sayılmaz | ✓ | `NineFailedRequestsAreClosed_TheTenthOpens_AndANotFoundIsNotCounted_KK7` (422/409/429 katmana hiç düşmez, `07 §2.7.5`) |
| KK8 dizi ve sertifika | ✓ | `ThePaymentSeries_*_KK8`, `Certificate_*` |
| KK9 eski satır, başarısız satır, imha | ✓ | `JobQueue_*`, `NightlyJob_*` |
| KK10 eşik değişir; canlılık 200; okuma hatasında 503 | ✓ | `TheLivenessEndpoint_StaysAt200_*`, `ASourceThatCannotBeRead_*` |
| `09 §25` — yeni ayar `§3.5` tablosunda | ✓ | eşikler mevcut `StatusThreshold` satırlarındadır (`§3.5` m.3, satır "Disk doluluğu eşiği"); kanal 5/15 sabiti `05 §10.2.3` değeridir, ayar değil |
| `09 §25` — geniş `catch` yeri | ✓ | `StatusEvaluator` yalnız bilinen türleri yakalar (DbException, IOException, UnauthorizedAccess, Json, Format, InvalidOperation, Timeout, kendi zaman aşımı); `AnUnknownFailure_IsNotHiddenAsAWarning_ItPassesUp`; `JobSourceScan` yeşil |
| `09 §25` — iş tablosu yalnız `Jobs/`'ta | ✓ | `JobQueueStatusCondition` `Infrastructure/Jobs/` altında; `JobSourceScan` (Unit 2878 yeşil) |
| `09 §25` — zaman `TimeProvider`'dan, DateTime yok | ✓ | `TimeAndMoney` taraması yeşil; sertifika bitişi `AsnReader` ile (`NotAfterOf_*`) |
| `09 §25` — makine ucu işareti, gövdesiz 500 | ✓ | `AnUnknownFailureOfAReading_IsA500WithoutABody_TheCatchingLayerCountsIt_AndItIsNeverReadAsOpen` |
| `09 §25` — doküman uyumu: `09 §8.1` m.3/m.8, `CONTEXT.md` | ✓ | bu PR'ın doküman değişiklikleri; sayı/liste anan başka yer yok (`README` yok) |
| Sık FAIL (1) darlık | ✓ | yakalama yalnız bilinen tür; istemci hatası yalnız iki tür (`ConnectionResetException`, `BadHttpRequestException`), bütün `OperationCanceledException` değil |
| Sık FAIL (2) kaynak taraması | ✓ | iş tablosu taraması `JobSourceScan`; yeni kaynak taraması eklenmedi |
| Sık FAIL (3) 06 "Varsayılan"/FK | ✓ | göç ve şema yok |
| Sık FAIL (4) sayı anan yerler | ✓ | backlog sayacı 93 → 92 (D-147 ✅); `CONTEXT.md` iki satır güncel |
| Sık FAIL (5) belge kuralı | ✓ | `09 §8.1` m.8 eklemesi D-147'ye dayanır (backlog gate kaydı bu düzeltmeyi öngörüyordu); karar kaydı dosyası implementation döneminde D-numaralarıdır |
| Güvenlik taraması | ✓ | sır yok; uç imzasız ve gövdesiz, koşul adı sızmaz; sertifika okuma yalnız bitiş tarihini saklar; yeni paket yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency dahil) | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 2878/2878 |
| Integration | ✓ | `--filter-class *StatusEndpointTests` / `*UnexpectedErrorLayerTests` / `*RequestLimitsTests` | 41 + 59 (hata katmanı + istek sınırları) yeşil; ayrıca önceki işçi 143 ilgili sınıf (canlılık, zamanlayıcı, iş kuyruğu...) yeşil |
| Biçim | ✓ | `dotnet format --verify-no-changes` | temiz |

Entegrasyon paketinin tamamı yerelde koşulmadı (parçalı); CI'a bırakıldı.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (eşikler mevcut `StatusThreshold` ayarları)
- **Paketleme / dağıtım:** Yok (yedek durum dosyası dizini `backup-status` `09 §18`/`05 §9.4.10`'un birimidir)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T314-durum-ucu`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** bkz. `Docs/IMPLEMENTATION_STATUS.md` T314 satırı
- **CI:** yapım chat'i beklemedi (yönetici düzeni); yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış servis, ağ, ücretli hizmet kullanılmıyor | PostgreSQL 18 ve .NET yerel; testler yerel Docker'da | Dış varsayım: yok |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Parçalı yükleme ucunun makine ucu işareti | D-139 (hedef T312) | uç henüz yok |

## ⚠ Kararlar (kurala en uygun seçenekle verildi)

- Hiç başarı kaydı olmayan yedek, gece işi ve nabız "açık" sayıldı (taze kurulumda ilk yedeğe kadar 503; kaynağı okunamayan açıktır, `05 §9.4.10`).
- Disk eşiği `>=` (eşiğe ulaşınca açık); kanal uyarısı 5/15 sabitleri `ChannelWarningCondition`'da (`05 §10.2.3`; `09 §3.5` tablosunda satır yok).
- Sertifika bitişi henüz okunmadıysa (ilk nabızdan önce) açık sayılır.
- Bilinmeyen istisna "açık"a çevrilmez, yakalama katmanına çıkar (`09 §8.1` m.6; yeni geniş `catch` açılmadı).
- D-147: `BadHttpRequestException` kendi durum koduyla yanıtlanır (sayfa hatası değil); `09 §8.1` m.8 bu kapsamla genişletildi.
- Gerçek Kestrel testi bozuk chunk (400) ve sıfırlanan bağlantıyı sınar; "sayaç artmadı" doğrulaması bağlantı sıfırlamasında 1 sn bekleyerek yapılır.
