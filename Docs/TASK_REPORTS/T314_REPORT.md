# T314 — Durum ucu: uyarı koşulları ve 500 sayacı

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-07

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
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `a57c4ca`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1 kapatıldı — kod kanıtı; K2 → D-162) |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; main son üç tamamlanmış run 42335449709, 42335449717, 42306192052 `success`; hafızada T314 satırı var.

**Kriterler 1–10 ✓, KK1–KK10 ✓, `09 §25` ✓.** Kanıtlar (doğrulayıcı):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `StatusEndpoint`: `MapGet("/sistem/durum")`, `open.Count == 0 ? 200 : 503`, `Cache-Control: no-store`, gövde yok; `Get_WithNoOpenCondition_Is200_*`, `Get_WithOneConditionOpen_Is503_*` (13 senaryo: gövde boş, `Content-Length` 0, çerez yok) |
| Kriter 2 / KK2 | ✓ | Koşul adı yalnız `StatusEvaluator` dönüşünde, yanıta yazılmaz; çok koşulda yanıt tek koşulla aynı (`Get_WithSeveralConditionsOpen_*`); imza/oturum metaverisi yok, yalnız `MachineEndpointMetadata` (canlılık ucuyla aynı eşleme) |
| Kriter 3 / KK3 | ✓ | `DiskFull` `>=` eşik (%85 → açık, `VAL-A964` ile uyumlu); `LogShippingStalled` bekleyen > 10 dosya ya da son başarıdan beri ≥ 10 dk ve son hata son başarıdan yeni; sahte okuyucularla `disk-full`, `log-files-waiting`, `log-shipping-stalled`; gerçek `pg_stat_archiver` + `pg_ls_archive_statusdir()` okuması (yetki `40-schema.sql`, `pg_monitor` yok) |
| Kriter 4 / KK4 | ✓ | `FullBackupCondition` (`backup_status_records.last_succeeded_at` — yalnız sunucu dışı yedek başarısı yazar, `database-job.sh`) ve `FileBackupCondition` (`backup-status.json`, `write-status.sh` biçimi) iki ayrı koşul; `EachBackup_OpensTheStatusAlone_*` ikisini ayrı açar, öteki taze iken kapanmaz |
| Kriter 5 / KK5 | ✓ | `HeartbeatTooOld` `> 5 dk`, kayıt yoksa açık; `Heartbeat_WithinTheAge_*` |
| Kriter 6 / KK6 | ✓ | `ChannelWarningCondition` kanal hâli kaydından okur (saklanan işaret yok, `05 §10.2.1`), 5 hata + ≥ 15 dk; `TheChannelWarning_ClosesWhenTheCountIsReset_KK6` açık → sıfırlanınca 200 |
| Kriter 7 / KK7 | ✓ | Sayaç yalnız `UnexpectedErrorLayer.Resolve`'da artar (tek `Record()` çağrısı) ve `Resolve` yalnız atılan istisnada çağrılır; 409/422/429 `FormResults` sonucudur, katmana düşmez (K1). Sayaç bellek içi `ConcurrentQueue`, pencere ayardan; `NineFailedRequestsAreClosed_TheTenthOpens_*` gerçek katmanla 9 → 200, 10 → 503, pencere geçince 200 |
| Kriter 8 / KK8 | ✓ | `PaymentStartFailureSeries` (bellek, kilitli) + `PaymentStartSeriesOpen` (≥ 5, ≥ 15 dk); `ThePaymentSeries_*_KK8` genç dizi 200, yaşlı 503, ilk başarıda 200. Sertifika: nabız istemcisinin (`https://{Domain}/`) TLS geri çağrısı bitişi DER'den okur, doğrulama kararını platforma bırakır; `< 14 gün` açık, okunmamışsa açık |
| Kriter 9 / KK9 | ✓ | `JobQueueStatusCondition` (`Infrastructure/Jobs/` altında): başarısız satır var ya da zamanı gelmiş en eski bekleyen satır `> 15 dk`; zamanı gelmemiş ve taze satır kapalı (`AJobNotYetDue_*`), `job-overdue`, `job-failed`; `NightlyJobTooOld` `> 26 saat` (`nightly-old`) |
| Kriter 10 / KK10 | ✓ | Eşikler `StatusThresholdOptions` (`DEPLOY_RUNBOOK §C` satırları önceden var); `TheDiskThreshold_IsTheOneInTheSettings_*` ayar 0.95 iken %90 → 200; `TheLivenessEndpoint_StaysAt200_*` 13 senaryoda 200; okunamayan dosya ve boşaltılmış `heartbeat_records` → 503; bilinmeyen istisna 503'e çevrilmez, gövdesiz 500 ve sayaç +1 |
| D-147 | ✓ | `ConnectionResetException` → `ClientGone`, `BadHttpRequestException` → kendi kodu, gövdesiz, `no-store`; ikisi de satır/sayaç yok (birim + gerçek Kestrel testi) |
| `09 §25` | ✓ | geniş `catch` yok (yalnız bilinen okuma hata türleri); zaman `TimeProvider`'dan; yeni ayar/paket/göç yok; modül sınırı (iş tablosu `Jobs/`, kanal kaydı `Notifications/`); `09 §8.1` m.3, m.8 ve `CONTEXT.md` güncel |
| Güvenlik | ✓ | sır yok; uç imzasız ve gövdesiz, koşul adı sızmaz; hız sınırı sayfa sınıfında; sertifika geri çağrısı yalnız bitişi saklar, kabul kararı `errors == None`; yeni bağımlılık yok (`System.Formats.Asn1` çatıda) |

**Testler (doğrulayıcı, yerel):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` Web ve Unit projelerinde temiz, Integration projesinde değişen iki dosyada temiz (projenin tamamı 150 sn'yi aştı; CI Lint yeşil); Unit 2878/2878 (Consistency 207 + diğer 2671); betik testleri 4/4; Integration yerelde PostgreSQL 18 ile parçalı: `MigrationBundleTests` 2/2, `StatusEndpointTests` 41/41, `StatusEndpointTests`+`RequestLimitsTests`+canlılık+`SignedEndpointDatabaseTests` 85/85 (süreç kapanışta 160 sn sınırına takıldı, 85'in hepsi geçti), `UnexpectedErrorLayerTests`+`SchedulerHeartbeatTests`+`EmailOutageTests`+`VersionedFormPageTests`+`OperationPatternTests` 79/79; kalan sınıflar için dal CI'ı. Koşu sonrası `git status` temiz, `dotnet restore --locked-mode` 0. **Dal CI:** run 42339425149 (head `a57c4ca`) `success` — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate.

**Task satırları (`12 §7.5`):** `11 §7.1`'de T314'ün `10` birimleri KP-50 ve KD-60'tır. `KP-50` grep'i panel ana sayfasının satırlarını getirir (`VAL-A257`…`VAL-A264`; KP-50 → T314, T382, T521): beklenen sonuçları panel ana sayfasının sayaçları ve uyarılarıdır, T314 panel ekranı kurmaz → `BEKLEMEDE` kalır. `KD-60` satırı yok. Durum ucuyla ilgili satırlar (`VAL-A446`, `A709`, `A946`, `A959`, `A964`…`A971`) KP grep'iyle gelmez; T314'ün testleri bunların sınırlarını sahte ölçümle karşılar, ama satırların ön koşulu gerçek kurulum (gerçek disk, gerçek arşivleme, sahte sağlayıcıyla jeton isteği — T404) ister → `BEKLEMEDE` kalır (T219, T229 emsali). Durum hücresi değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**K1 (kapatıldı — kod kanıtı):** KK7 "409 sayılmaz" der; tümleşik test 409 yerine 404 sınar. 409 (sürüm çakışması), 422 ve 429 `FormResults`/`OperationRunner` sonucudur, istisna olarak katmana düşmez; sayacın tek yazımı `Resolve` içindedir. Kod değişikliği gerekmez.

**K2 → D-162:** işlem günlüğü aktarımının durması son başarılı aktarımdan sayılır; uzun süre WAL üretmeyen sistemde tek bir geçici aktarım hatası koşulu hemen açar (bir sonraki başarılı denemede kalkar). Dış kontrol iki ardışık başarısızlıkta uyardığı için etkisi düşük; okuma `VAL-A965`'in "9 ve 11 dakika durmuş" ölçüsüyle F6'da teyit edilir.

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. İki küçük fark: KK7 satırı "404/409 sayılmaz" der ama test yalnız 404'ü sınar (K1); raporda `12` task satırları bölümü yok (bu doğrulamada yazıldı).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (eşikler mevcut `StatusThreshold` ayarları)
- **Paketleme / dağıtım:** Yok (yedek durum dosyası dizini `backup-status` `09 §18`/`05 §9.4.10`'un birimidir)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T314-durum-ucu`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #418
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
