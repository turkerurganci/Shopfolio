# T408 — SES: geri dönen adreslerin öğrenilmesi

**Faz:** F4 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `SesEmailGateway.QueryBouncesAsync`: `ListSuppressedDestinations` (`StartDate`, `Reasons` = `BOUNCE`, `PageSize` = 1000, `NextToken` döngüsü); bütün sayfalar tek çalıştırıcı çağrısındadır (bir sınır, yeniden deneme yok); sayfalardan biri düşerse kısmi liste verilmez (`QueryFailed`); `BOUNCE` dışı özet okunmaz; adres listedeki yazılışla döner.
- `BounceQueryJobHandler` (iş türü işleyicisi): ağ geçidi işlem dışında çağrılır; adresler `NormalizeIdentity` ile harf ayrımsız eşlenir; `SentAt` ≥ pencere başı olan `Sent` satırlar `Undelivered` + `RecipientError` olur, `SentAt` kalır, kanal hâline dokunulmaz, güncelleme `DispatchState = Sent` koşulludur (ikinci uygulama etkisiz); her tur (başarısız dahil) `RescheduleAfter(15 dk)`; başarısız tur boş geçer, imleç yoktur.
- `BounceQueryRowSource` (zamanlayıcı kaynağı): ağ geçidi kayıtlıysa her uyanışta kuyruğun kendi ekleme çağrısını ister; var olan bekleyen satırı kuyruğun tekil dizini olduğu gibi bırakır (`RelatedRecordId` boş). İş tablosunu doğrudan okumaz (09 §13.1 m.2).
- `JobTimings.BounceQueryWindow` (24 sa; 09 §3.5 satırına ve tutarlılık testine bağlandı). `DispatchAttempts.GatewayIsRegistered`. `NotificationsModule` kaydı.
- `NotificationChannelSourceScan`: tek tam dosya adı eklendi (handler); handler'ın `SendAsync`/`EmailMessage` içermediğini sınayan test.
- `AMAZON_SES.md` §10 "Geri dönen adresler"; 08 §4.1 DV-22 son hücresi; 09 §3.5 satırı; backlog D-1017 ve D-97 payı kapandı, D-1022 T410'a ileri atıldı, yeni D-1024, D-1025.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/SesEmailGateway.cs`, `Infrastructure/Jobs/JobTimings.cs`
- `src/Shopfolio.Web/Modules/Notifications/` (`BounceQueryJobHandler.cs`, `BounceQueryRowSource.cs`, `DispatchAttempts.cs`, `NotificationsModule.cs`)
- `tests/Shopfolio.UnitTests/Integrations/` (`SesEmailGatewayTests`, `SesWire`, `BounceQueryRulesTests`), `Architecture/NotificationChannelSourceScan*`, `Consistency/JobTimingsConsistencyTests`
- `tests/Shopfolio.IntegrationTests/Jobs/BounceQueryTests.cs`
- `Docs/INTEGRATION_RUNBOOKS/AMAZON_SES.md`, `Docs/08_INTEGRATION_SPEC.md` (DV-22), `Docs/09_CODING_GUIDELINES.md` (§3.5 satırı), `Docs/DEFERRED_BACKLOG.md` ve `.d/`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.d/T408.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sorgu yöntemi, 15 dk, web kancası/SNS yok | ✓ | `BounceQueryJobHandler` + `RescheduleAfter(JobTimings.BounceQueryInterval)`; `BounceQueryTests` KK1; `AMAZON_SES.md` §5 "Yoktur" |
| 2 | `StartDate`, `Reason` = `BOUNCE`, sayfalama; `COMPLAINT` işlenmez | ✓ | `SesEmailGatewayTests.QueryBouncesAsync_AsksTheListFromTheGivenMoment_…`, `…_ReadsEveryPage_…`, `…_AComplaintSummary_IsNotRead_…` |
| 3 | Harf ayrımsız eşleşme; `SentAt` 24 sa içindeki `Sent` satırlar `Undelivered` + `RecipientError`, `SentAt` kalır | ✓ | `BounceQueryTests` KK3 (büyük harf, noktalı i, 24 sa'ten eski satır değişmez) |
| 4 | Kanal hâli etkilenmez; ikinci uygulama etkisiz | ✓ | `BounceQueryTests` KK4 |
| 5 | Başarısız tur boş, kanal hâli yazılmaz, imleç yok, hata kayda, satır başarısız olmaz | ✓ | `BounceQueryTests` KK5 (429 turu; sonraki tur 24 sa öncesinden sorar) |
| 6 | Tek bekleyen satır, ilgili kayıt kimliği boş | ✓ | `BounceQueryTests` KK6 (kaynak iki kez yazar, tek satır; veritabanı ikinci bekleyen satırı reddeder) |
| 7 | Silme çağrısı yok; liste yalnız `BOUNCE`; `AMAZON_SES.md`'de yazılı | ✓ | `BounceQueryRulesTests` KK7 (src taraması, tek `ListSuppressedDestinationsAsync` kullanıcısı, runbook metni); `AMAZON_SES.md` §1 adım 6 ve §10 |
| 8 | DV-22 satıra yazılır | ✓ (kısmen gözlendi; gerçek hesap OWNER_INPUT) | 08 §4.1 DV-22; `BounceQueryRulesTests.TheDv22Row_…_KK8`; D-1024 |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil bütün maddeler bu diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 on beş dakikalık yineleme sahte saatle | ✓ | `BounceQueryTests` KK1 (`FakeTimeProvider`, `RescheduleAfter`) |
| KK2 `StartDate` 24 sa öncesi, şikâyet işlenmez, ikinci sayfa işlenir | ✓ | `SesEmailGatewayTests` (üç test yukarıda) + `BounceQueryTests` KK1_KK2 (her tur 24 sa öncesinden sorar) |
| KK3 büyük harf eşleşir, eski satır değişmez | ✓ | `BounceQueryTests` KK3 |
| KK4 kanal hâli değişmez, ikinci uygulama etkisiz | ✓ | `BounceQueryTests` KK4 |
| KK5 429 turu boş, sonraki tur yine 24 sa öncesinden | ✓ | `BounceQueryTests` KK5; `SesEmailGatewayTests` hata eşleme |
| KK6 ikinci bekleyen satır yok | ✓ | `BounceQueryTests` KK6 |
| KK7 kodda silme çağrısı yok; runbook'ta yalnız `BOUNCE` | ✓ | `BounceQueryRulesTests` KK7; `NotificationChannelSourceScanTests.TheReturnedAddressHandler_…` |
| KK8 DV-22 satırı | ✓ | `BounceQueryRulesTests` KK8 |
| 09 §25 ilgili satırlar | ✓ | alt tablo |
| Güvenlik taraması | ✓ | alt tablo |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | tarama listesine yalnız tek tam dosya adı; handler'ın gönderme çağırmadığını sınayan test; `JobSourceScan` istisnası eklenmedi (kaynak iş tablosunu okumayacak biçime çevrildi) |
| Sık FAIL 2: kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | silme taraması `Delete*/Put*Suppress*` işlem adı kalıbı, yorumlar boşaltılarak; liste okuması tek dosyada |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ | şema değişikliği yok; dizin eksiği D-1025 olarak kayıtlı (06 §7.5 değişikliği ister) |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | AMAZON_SES.md başlık/atıflar (§1 adım 6, §5, giriş), 09 §3.5 satırı + `JobTimingsConsistencyTests` (pencere), tarama listesi dokümanı |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | 09 §3.5 satırına yalnız 08 §2.2.3'teki 24 saat eklendi (kural değişmedi) |
| Sık FAIL: hedefi bu task olan backlog kaydı | ✓ | D-1017 kapandı, D-97 payı kapandı, D-1022 T410'a ileri atıldı |
| Sık FAIL: yeni süre sabiti 09 §3.5'te yok | ✓ | `BounceQueryWindow` 09 §3.5 satırında ve testle bağlı |

**09 §25 ve güvenlik.** Gateway çağrısı yalnız `QueryBouncesAsync`; `ListSuppressedDestinationsAsync` tek dosyada; silme/yazma çağrısı yok. Handler çağrıyı işlem dışında yapar, adres kayda yazılmaz (hata kaydı yalnız kod ve sınıf). `TimeProvider` kullanıldı, kural `JobTimings`'te. Geniş `catch` yok (yalnız özel AWS/iptal yakalamaları). Modül sınırı: kaynak ve handler Notifications içinde, iş tablosuna yalnız `IJobQueue` ile dokunur.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 6018/6018 |
| Integration | ✓ (ilgili sınıflar) | `--filter-class "*BounceQueryTests"`; `--filter-namespace` Jobs, Outages, Commands, Concurrency | BounceQuery 8/8; Jobs 85/85; Outages+Commands+Concurrency 149/149 |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | temiz |
| Derleme | ✓ | `dotnet build` (Unit, Integration) | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | (doğrulayıcı doldurur) |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (hesap listesi ayarı `AMAZON_SES.md` §1 adım 6 / §10, kurulum adımı)
- **Yeni bağımlılık:** Yok (`AWSSDK.SimpleEmailV2` T407'de)

## Commit & PR

- **Branch:** `task/T408-ses-geri-donen`
- **Commit:** (PR'da)
- **PR:** #PRNO
- **CI:** (yönetici izler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `ListSuppressedDestinations` istek alanları `StartDate`, `Reason` (SDK: `Reasons`), `PageSize`, `NextToken`; yanıt `SuppressedDestinationSummaries[].EmailAddress/Reason/LastUpdateTime` | AWSSDK.SimpleEmailV2 4.0.102.1 tipleri; 08 §2.2.3 tablosu; sahte HTTP testi istek biçimini sınar | Çevrimdışı ✓; gerçek hesapta gözlenmedi (DV-22, D-1024) |
| 2 | 429/kimlik/zaman aşımı hatası tek sorgu hatasıdır, tur boş geçer | 08 §2.2.3 "Sorgu hatası"; `SesErrorMapping` T407 | ✓ (`BounceQueryTests` KK5) |
| 3 | Hesap listesi yalnız `BOUNCE` açık | AWS konsol adımı, kurulum | Kurulum adımı, `AMAZON_SES.md` §1/§10 |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | DV-22 gerçek hesap gözlemi | D-1024 | OWNER_INPUT: üretim erişimli AWS hesabı |
| 2 | `dispatch_records` `(DispatchState, SentAt)` dizini | D-1025 | 06 §7.5 değişikliği ister; hacim küçük |
| 3 | Ön sorgu aşımı kayıt öneki (T407 K1) | D-1022 (T410'a ileri atıldı) | T408 `AskRecipientAsync`'e dokunmadı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** son 3 run `gh run list --branch main --limit 3`: 42804053557 (Publish), 42804053578 (CI), 42796731944 (CI) — hepsi success
- **Repo memory (bitiş kapısı 9):** ortak kayıt dosyalarına dokunulmadı (parça dosyaları kullanıldı)
- **⚠ kararlar:** 24 sa penceresi `JobTimings`'te ve 09 §3.5 satırında; satır kaynağı iş tablosunu okumaz, her uyanışta kuyruğun tekil ekleme çağrısını ister (günde 1440 boş ekleme, ucuz); D-1022 T410'a ileri atıldı; test fabrikası gateway'i sildiği için test host'ta satır doğmaz
