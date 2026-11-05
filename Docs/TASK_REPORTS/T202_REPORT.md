# T202 — Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahteler

**Faz:** F2 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Integrations/`: servis başına arayüz (`IPaymentGateway`, `IEmailGateway`, `IGoogleIdentityGateway`) ve istek tipleri (`PaymentStartRequest`, `CardRefund`, `PaymentNotice`, `EmailMessage`, `IdentityCodeExchange`); `09 §11.1` tablosunun sekiz sonuç ailesi (soyut kayıt, özel kurucu, `sealed` türevler; `IExternalCallResult<TSelf>.FromFailure` ile iş başına "sınıflanmış yanıt yok" sınıfı).
- Ortak çağrı yüzü `ExternalCallRunner`: süre `IntegrationOperations` (iş → `ExternalCallTimeouts.UserWaiting`/`Job`) ve `TimeProvider`'a bağlı iptal kaynağıyla; çağrı bir kez gider, aşım/bağlanamama/tanınmayan yanıt ailenin sınıfına döner, tek kayıt satırı `ExternalCallFailed` (iş, sınıf, hata nedeni).
- `IntegrationHttpClients`: ödeme sağlayıcısı ve Google adlandırılmış istemcileri — `Timeout` sonsuz, `SocketsHttpHandler` (yönlendirme ve çerez kapalı), fabrikanın kayıt işleyicileri kaldırılmış, dayanıklılık işleyicisi yok.
- Geliştirme e-posta ağ geçidi `SmtpCaptureEmailGateway` (+`ISmtpTransport`/`SmtpTransport`, `EmailCaptureOptions` + doğrulayıcı); `IntegrationRegistration.AddIntegrations` (`Program.cs`). `deploy/compose.dev.yaml`: `EmailCapture__Host: mailpit`, `EmailCapture__Port: "1025"`.
- Sahteler `UnitTests/Fakes/Fake{Payment,Email,GoogleIdentity}Gateway.cs` (+`FakeCall`): kuyruk, varsayılan, sıralı çağrı listesi, ağ yok.
- Belgeler: `09 §4.3` m.2, §11.1 m.2/m.3/m.4, §18.2 m.3, §19.1 m.7; `CONTEXT.md` §3; `DEFERRED_BACKLOG` D-101 kapandı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/*` (yeni), `Infrastructure/ExternalCallTimeouts.cs`, `Infrastructure/ExternalCallKind.cs` (yeni), `Infrastructure/Logging/LogEvents.cs`, `Modules/Payment/CardRefundJobRule.cs` (yorum), `Program.cs`
- Testler: `UnitTests/Integrations/*`, `UnitTests/Fakes/Fake*Gateway.cs`, `UnitTests/Architecture/{IntegrationSourceScan,IntegrationSourceScanTests,PaymentGatewayImplementationTests}.cs`, `UnitTests/Architecture/ViolationSamples/Integrations/ProviderLeak.cs`, `UnitTests/Consistency/IntegrationTimeoutsConsistencyTests.cs`, `IntegrationTests/Web/EmailCaptureRegistrationTests.cs`
- `deploy/compose.dev.yaml`, `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç servis tek katmanın arkasında; modül ürün işini çağırır | ✓ | `IPaymentGateway`/`IEmailGateway`/`IGoogleIdentityGateway` (iş adları ürün düzeyinde); `IntegrationSourceScanTests.ApplicationSourceAndFakes_*` (modül/sayfa/uçta HTTP/SMTP istemci tipi yok) |
| 2 | Adlarda sağlayıcı yok; ödemede tek uygulama | ✓ | `IntegrationSourceScan` kural 1 + `Scan_FindsAProviderNameInAnyCaseAndInComments`, `Scan_OnLookalikes*`; `PaymentGatewayImplementationTests` (en çok bir `IPaymentGateway`, ihlal örneğiyle) |
| 3 | 15 sn / 30 sn; kendiliğinden yeniden deneme ve devre kesici yok | ✓ | `IntegrationOperationsTests`; `IntegrationTimeoutsConsistencyTests` (08 §3.1 sütunu = `LimitOf`); `ExternalCallRunnerTests.RunAsync_*ThatHangs_*` (sahte saat, bir tık eksikte bekler, sınırda biter, istek 1) |
| 4 | İstemci kitaplığının yeniden denemesi kapalı | ✓ | `IntegrationHttpClientsTests.Handler_*` (zincir: yaşam işleyicisi → `SocketsHttpHandler`, ek işleyici yok), `RefusedConnection_ThroughTheChain_GoesOnce_*`; tarama kural 4 (`Polly`, `Add*ResilienceHandler`, `MaxErrorRetry > 0`) |
| 5 | Aşım "bilinmeyen": iade/sorgu yeniden sorulur; başlatmada bağlantı, e-postada kanal hatası | ✓ | `ResultFamilyTests` (her aile × her başarısızlık türü); `ExternalCallRunnerTests.RunAsync_RefundRequestThatHangs_EndsAtTheJobLimitAsUnknown_AndIsNotSentAgain`, `RunAsync_PaymentStartThatHangs_*`, `RunAsync_EmailSendThatHangs_*`; `SmtpCaptureEmailGatewayTests.SendAsync_ToolThatHangs_*` |
| 6 | Bağlantı ≠ ödeme hatası, alıcı ≠ kanal hatası; ürün sonucunu çekirdek verir | ✓ | `ResultFamilyTests.Families_KeepTheConnectionErrorApart*`; `SmtpCaptureEmailGatewayTests.SendAsync_RecipientRefused_*` / `SendAsync_OtherSmtpFailure_*` / `SendAsync_RecipientExceptionWithAChannelReply_*`; arayüz modül tipi almaz |
| 7 | Okunmayan alan kayda girmez; hata kaydında yalnız neden | ✓ | `ExternalCallRunnerTests.FailureLine_CarriesTheEventTheJobTheKindAndTheReason_*` (istek alanı, anahtar, okunmayan yanıt alanı, adres, istisna metni yok); `SmtpCaptureEmailGatewayTests.SendAsync_OtherSmtpFailure_*` (alıcı, konu, yanıt metni yok); istemcilerde fabrika kayıtçısı yok |
| 8 | Tabloda olmayan yanıt: ödemede bağlantı, e-postada kanal hatası, Google'da başarısız; kayda yazılır | ✓ | `ExternalCallRunnerTests.Failed_AnAnswerNotInTheTable_*`; `ResultFamilyTests.Identity_AnAnswerNotInTheTable_IsAFailure`; `09 §11.1` m.3 iş başına liste |
| 9 | Sahteler `UnitTests/Fakes/`; geliştirmede e-posta Mailpit'e, ayar `compose.dev.yaml`'da | ✓ | `GatewayFakesTests`; tarama kural 5; `IntegrationRegistrationTests` + `IntegrationTests/Web/EmailCaptureRegistrationTests`; `deploy/compose.dev.yaml` `EmailCapture__*`; elle deneme (aşağıda) |

### Öz denetim (bitiş kapısı 2)

`git diff origin/main...HEAD` üzerinde bütün maddeler yeniden denetlendi (önceki işçinin yaptıkları dahil).

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 modül kodu sağlayıcı türüne başvurmaz | ✓ | `IntegrationSourceScan` kural 1–2; `ApplicationSourceAndFakes_KeepTheProviderNamesTheClientsAndTheRetriesInsideTheLayer`; ihlal örneği `ViolationSamples/Integrations/ProviderLeak.cs` |
| KK2 adlarda sağlayıcı yok; ödemede tek uygulama | ✓ | `Scan_OutsideTheImplementationFiles_*`, `Scan_InAnImplementationFile_*`; `PaymentGatewayImplementationTests` |
| KK3 iki süre sınıfı sahte gecikmeyle | ✓ | `ExternalCallRunnerTests` (`ManualTimeProvider`, `ScriptedHandler.Hang`); gerçek saat/bekleme yok |
| KK4 istemci yeniden deneme kapalı | ✓ | `IntegrationHttpClientsTests` (3 test × 2 istemci) |
| KK5 aşım: iade/sorgu bilinmeyen, başlatma bağlantı, e-posta kanal | ✓ | `ResultFamilyTests`, `ExternalCallRunnerTests`, `SmtpCaptureEmailGatewayTests.SendAsync_ToolThatHangs_*` |
| KK6 bağlantı ve alıcı hataları ayrı | ✓ | `ResultFamilyTests.Families_*`; `SmtpCaptureEmailGatewayTests` (alıcı kodları `RecipientStatuses`'tan türetilir) |
| KK7 kayıtta istek alanı, anahtar, okunmayan yanıt alanı yok | ✓ | `FailureLine_*`, `SendAsync_OtherSmtpFailure_*`; `LogEvents.ExternalCallFailed` + `09 §18.2` m.3 (`LoggingConsistencyTests`) |
| KK8 tanınmayan yanıt sınıf tablosuna göre | ✓ | `Failed_AnAnswerNotInTheTable_*`, `ResultFamilyTests` |
| KK9 sahteler ağa çıkmaz; elle denemede Mailpit'te ileti | ✓ | tarama kural 5 + `Scan_OnViolationSampleAsAFake_ReportsTheNetworkTypes`; elle deneme: `shopfolio-t202-mailpit` (v1.33.0), gerçek `SmtpTransport` + `SmtpCaptureEmailGateway` → `GET /api/v1/messages` `total: 1`, From "Örnek Mağaza <dev@example.com>", ReplyTo, UTF-8 konu doğru; geçici sınayıcı commit edilmedi, konteyner silindi |
| `09 §25` ilgili satırlar | ✓ | PR açıklamasında soru-cevap |
| Güvenlik taraması | ✓ | sır yok (yer tutucu işaretler); kayıt alanı listesi kapalı; istemcilerde çerez/yönlendirme kapalı; yeni paket yok |
| Sık FAIL: belgede olmayan sabit | ✓ | 15/30 sn `09 §3.5` + `08 §3.1`; SMTP portu kodda değil Compose'da; `RecipientStatuses` RFC 5321 kodları (sabit süre/tavan değil) |
| Sık FAIL: test beklenen değeri sabitten türetir | ✓ | süreler `ExternalCallTimeouts`/`LimitOf`'tan, iş→süre eşleşmesi `08 §3.1` tablosundan okunur; alıcı kodları `RecipientStatuses`'tan |
| Sık FAIL: gerçek saat / koleksiyon yalıtımı | ✓ | birim testleri `ManualTimeProvider`; entegrasyon testi koleksiyon veritabanında, sunucu geneli etki yok |
| Sık FAIL: tarama istisnası dar + ayrı ihlal örneği | ✓ | istisna yalnız `Integrations/` kökünde `PayTr`/`Ses` önekli dosya; `display tree` benzeri yanlış pozitif ve `UsePayTr` gibi deve harfli yakalama testli; ihlal örneği ayrı dosya |
| Sık FAIL: maskesiz ikinci çıktı | ✓ | `HttpClient` fabrikası kayıtçıları kaldırıldı (`RemoveAllLoggers`), katman satırı yalnız `ILogger` (uygulama biçimleyicisi) |
| Sık FAIL: aynı şeyi anan öteki yerler | ✓ | `09 §4.3` m.2, §11.1 m.2–m.4, §18.2 m.3, §19.1 m.7; `CONTEXT.md` §3; `compose.dev.yaml` başlık yorumu; `.env.example` bilerek değişmedi (kurulum ayarı değil — ⚠ 4) |
| Backlog D-101 | ✓ | ✅ Çözüldü → T202 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | uyarısız |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 1126/1126 |
| Integration | ✓ (parça) | `--filter-class Shopfolio.IntegrationTests.Web.EmailCaptureRegistrationTests` | 3/3 (PostgreSQL 18, `shopfolio-t202-pg`); tamamı CI'da |
| Contract | — | | yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-05, head `70c3d3f`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-105, K2 → D-106) |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — geliştirme ayarı `EmailCapture:Host`/`Port` (yalnız `compose.dev.yaml`; kurulum ayarı değil)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (`System.Net.Mail.SmtpClient`, `IHttpClientFactory` çatıyla gelir)

## Commit & PR

- **Branch:** `task/T202-dis-servis-katmani`
- **Commit:** dal ucu PR'da
- **PR:** #396
- **CI:** dal CI 42036902091 ✓ success (head `70c3d3f`)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Soyut kayıt + özel kurucu + `sealed` türevlerle bütün sınıfları sayan `switch` yine CS8509 verir | derleme denemesi (önceki işçi) | ✓ `09 §11.1` m.2'ye yazıldı |
| 2 | Çatının `SmtpClient`'ı ek paketsiz Mailpit'e UTF-8 ad/konu ve HTML alternatifiyle gönderir | elle deneme, Mailpit API | ✓ `09 §4.3` m.2'ye yazıldı |
| 3 | `RemoveAllLoggers` + `ConfigurePrimaryHttpMessageHandler` sonrası zincir yalnız yaşam işleyicisi + `SocketsHttpHandler` | `IntegrationHttpClientsTests` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gerçek sağlayıcı uygulamaları (ödeme, e-posta servisi, Google) ve onların eşleme tabloları | — (planlı: T401, T407, T409) | task kapsamı dışı; katman ve sahteler hazır |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; dal `d7e1456`'dan alındı, `origin/main` `cafdf86` güncel)
- **Ana dal CI startup (Adım 0):** main son run'lar success (CI 42031439198, Publish 42031439167)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** Tam Integration koşusu yerelde 170 sn sınırına sığmadı; CI'da koşar.

---

## Doğrulama Sonucu — T202 Dış servislerin bağlantı katmanı — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T202-dis-servis-katmani   **Commit:** 70c3d3f

### Verdict: ✓ PASS

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (dal `origin/task/T202-dis-servis-katmani` = `70c3d3f`) |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (worktree'de ana deponun `scripts/git-hooks`'u, mutlak yol) |
| Ana dal CI (son 3) | ✓ | 42031439167 success, 42031439198 success (T201 #395), 42021631131 success (T221 #394) |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22 (T202) |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç servis tek katmanın arkasında; modül ürün işini çağırır | ✓ | `Infrastructure/Integrations/` `IPaymentGateway` (5 iş), `IEmailGateway` (2), `IGoogleIdentityGateway` (1) — `09 §11.1` m.1 ile birebir; `Program.cs` `AddIntegrations`; tarama kural 2 (modül/sayfa/uçta HTTP/SMTP istemci tipi yok) yeşil |
| 2 | Adlarda sağlayıcı yok; ödemede tek uygulama | ✓ | `grep -rniE "paytr\|\bses\b\|amazon\|aws" src` → yalnız `packages.lock.json`'da rastlantı özet; tarama kural 1 yeşil ve ihlal örneğiyle kırmızı; `PaymentGatewayImplementationTests` (≤1 `IPaymentGateway`, ikinci örnekle 2 sayar); `PaymentNotice`/`CardRefund`'da "hangi sağlayıcı" alanı yok (KD-64). Dar/geniş sınama → K1 |
| 3 | 15/30 sn; katman yeniden denemez, devre kesici yok | ✓ | `IntegrationOperations.KindOf` (başlatma, kimlik → 15; öteki 5 iş → 30); `IntegrationTimeoutsConsistencyTests` `08 §3.1` sütununu okur; `ExternalCallRunnerTests.*ThatHangs_*` sahte saatle bir tık eksikte bekler, sınırda biter, istek sayısı 1; `ExternalCallRunner` tek `await call(...)`, döngü/Polly yok |
| 4 | İstemci kitaplığının yeniden denemesi kapalı | ✓ | `IntegrationHttpClientsTests`: zincir yalnız `LifetimeTrackingHttpMessageHandler` → `SocketsHttpHandler` (yönlendirme/çerez kapalı, `RemoveAllLoggers`), `Timeout` sonsuz, reddedilen bağlantı 1 istek; tarama kural 4 (`Polly`, `Add*ResilienceHandler`, `MaxErrorRetry > 0`); SMTP istemcisinde yeniden deneme yok; SES SDK'sı bu task'ta yok (T407) |
| 5 | Aşım/yanıtsızlık "bilinmeyen"; başlatmada bağlantı, e-postada kanal hatası | ✓ | `FromFailure`: `RefundRequestResult.Unknown` (yeniden gönderilmez), `PaymentQueryResult.NoResponse`, `RefundQueryResult.NoResult`, `PaymentStartResult.ConnectionFailed`, `EmailSendResult.ChannelError`; `ResultFamilyTests` (aile × 3 tür), runner aşım testleri 5 iş için, `SmtpCaptureEmailGatewayTests.SendAsync_ToolThatHangs_*` |
| 6 | Bağlantı ≠ ödeme hatası, alıcı ≠ kanal hatası; ürün sonucu çekirdekte | ✓ | `PaymentStartResult` `ConnectionFailed`/`SecondTokenRejected` ayrı, ödeme hatası bildirim sonucudur (`08 §2.1.8`); `EmailSendResult` `RecipientError`/`ChannelError`; SMTP 450/550–553 alıcı, öteki kanal (`SmtpCaptureEmailGatewayTests`); arayüzler modül türü almaz, iptal/"kullanılamıyor" yok |
| 7 | Okunmayan alan kayda girmez; hata kaydında yalnız neden | ✓ | `ExternalCallRunner.LogFailed`: olay, iş, tür, neden kodu — istisna nesnesi verilmez; `FailureLine_*` testi istek alanı, anahtar (`Authorization`), okunmayan yanıt alanı (`masked_pan`), adres ve istisna metninin satırda olmadığını, `exceptionType`/`stackTrace` alanı olmadığını sınar; `HttpRequestException` mesajı değil `HttpRequestError` adı yazılır; SMTP yanıt metni değil kod |
| 8 | Tabloda olmayan yanıt: ödeme bağlantı, e-posta kanal, Google başarısız; kayda yazılır | ✓ | `ExternalCallRunner.Failed` satır yazar + `FromFailure`; `Failed_AnAnswerNotInTheTable_*` (3 servis, 3 satır); `GoogleIdentityResult.FromFailure` Unrecognized → `Failed`, aşım/bağlanamama → `Unavailable` (`09 §11.3` m.4) |
| 9 | Sahteler `UnitTests/Fakes/`; geliştirmede e-posta Mailpit'e, ayar `compose.dev.yaml`'da | ✓ | `Fake{Payment,Email,GoogleIdentity}Gateway.cs` (durumlu, sıralı çağrı, ağ türü yok — tarama kural 5); `compose.dev.yaml` `EmailCapture__Host: mailpit`, `EmailCapture__Port: "1025"`; `.env.example`'da yok — `09 §4.3` m.2 (kurulum ayarı değil; ⚠ 4 gerekçesi tutarlı); **elle deneme:** `shopfolio-t202-val-mailpit` (v1.33.0), gerçek `SmtpTransport` + `SmtpCaptureEmailGateway` → `Sent`; Mailpit API `total: 1`, From "Örnek Mağaza <dev@example.com>", ReplyTo, UTF-8 konu, Text + HTML parçaları (geçici sınayıcı silindi) |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
T202'nin `10 KP` birimi yok (`11 §7.1`: `08 §3.1` yalnız T202'nin); satırlar `12`'de kaynağı `08 §3.1` olanlardan grep'le bulundu. `VAL-E056` (yanıtsız son sorgu siparişi iptal etmez, beş dakikalık sorgu — sipariş ve iş kuyruğu; `08 §3.3` T204/T403) ve `VAL-E064` (gönderim kaydının 5 dk/30 dk/2 sa denemeleri — `08 §3.3` T204, gönderim kaydı) T202'nin kriterlerine tam eşlenmez → `BEKLEMEDE` kalır.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-E054 | ORTA | mock | BEKLEMEDE | PASS (kriter 3) | — |
| VAL-E055 | ORTA | mock | BEKLEMEDE | PASS (kriter 3, 4) | — |
| VAL-E057 | KRİTİK | mock | BEKLEMEDE | PASS (kriter 7) | — |
| VAL-E058 | KRİTİK | mock | BEKLEMEDE | PASS (kriter 7; sonuç tiplerinde okunmayan alana yer yok) | — |
| VAL-E059 | ORTA | mock | BEKLEMEDE | PASS (kriter 8) | — |
| VAL-E056 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE (T204/T403) | — |
| VAL-E064 | ORTA | mock | BEKLEMEDE | BEKLEMEDE (T204, gönderim kaydı) | — |

`python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77` → satır 1503, kimlik tekil 1503, Hata: 0.

### Doğrulama Kontrol Listesi
- [x] KK1: modül kodu sağlayıcı türüne başvurmaz (tarama kural 1–2; modüllerde katman türü yok)
- [x] KK2: arayüz, sonuç tipi, uç, alan, değer adında sağlayıcı adı yok; ödemede tek uygulama
- [x] KK3: iki süre sınıfı sahte gecikmeyle (`ManualTimeProvider` + `ScriptedHandler.Hang`)
- [x] KK4: istemci yeniden deneme ayarı kapalı
- [x] KK5: aşım iade ve sorguda bilinmeyen/sonuç yok, başlatmada bağlantı, e-postada kanal hatası
- [x] KK6: bağlantı ve alıcı hataları ayrı sonuç
- [x] KK7: kayıtta istek alanı, anahtar, okunmayan yanıt alanı yok
- [x] KK8: tanınmayan yanıt sınıf tablosuna göre
- [x] KK9: sahteler ağa çıkmaz; elle denemede Mailpit'te ileti görüldü
- [x] `09 §25` ilgili satırlar: yeni ad sözlükle/`09 §11.1` tablosuyla birebir; diff kapsamda (`CardRefundJobRule` yorumu tarama kural 1 için); yeni ayar §3.5 tablosunda değil — kurulum ayarı değil (`09 §4.3` m.2), `.env.example` doğru biçimde değişmedi; `CONTEXT.md` §3 güncel; zaman `TimeProvider`'dan; yeni paket yok; dış çağrı katman arayüzünde sonuç sınıfıyla; yeni tarama ihlal örneğiyle kırmızı; `LogEvents` listesi ile `09 §18` m.3 eşit (Unit yeşil)
- [x] Güvenlik taraması (aşağıda)
- [x] D-101: `09 §11.1` m.4 ve `ExternalCallTimeouts` yorumu `new CancellationTokenSource(süre, TimeProvider)` der; `src`'de `CancelAfter` yok; runner ve `HeartbeatPulse` aynı biçim

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 1126/1126 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration Web | ✓ | `--filter-namespace …Web` | 75/75 (EmailCaptureRegistrationTests dahil) |
| Integration Jobs | ✓ | `--filter-namespace …Jobs` | 77/77 |
| Integration Concurrency + Security | ✓ | iki namespace | 26 + 18 = 44/44 |
| Integration Database + Disposal + FileSecrets + Logging | ✓ | dört namespace | 107/107 |
| Integration Model (A–I) | ✓ | `--filter-class …Model.A*…I*` | 482/482 (ilk iki koşuda yük altında `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled_*` göç demeti derlemesi düştü; tek başına 60/60 ve üçüncü koşuda 482/482 — T202 dışı, dal CI'ında yeşil) |
| Integration Model (M–S) | ✓ | `--filter-class …Model.M*,P*,R*,S*` | 411/411 |
| Integration toplam | ✓ | PostgreSQL 18 (`shopfolio-t202-val-pg`) | 1196 |
| Dal CI | ✓ | `gh run list --branch task/T202-dis-servis-katmani` | 42036902091 success (head `70c3d3f`); 42036861038 cancelled (önceki head) |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (diff'te anahtar/parola yok; test işaretleri yer tutucu; `compose.dev.yaml` uydurma değerler)
- Auth etkisi: Temiz (yeni uç yok)
- Input validation: Temiz (`EmailCapture` host varken port 1–65535 zorunlu, açılış durur; alıcı adresi `TryCreate`)
- Yeni bağımlılık: Yok (`System.Net.Mail`, `IHttpClientFactory` çatıyla)
- Not: geliştirme ağ geçidi ortam adına değil `EmailCapture:Host` ayarına bağlıdır — `09 §4.3` m.2 böyle yazar; ayar yalnız `compose.dev.yaml`'dadır.

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| — | — | — | — | Bulgu yok | — | — | — |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Sağlayıcı adı taraması — dar: `sesClient`/`_sesClient`, `AWSSDK`, `CARDPAYTR` yakalanmaz; `.razor` ve `.js` taranmaz. Geniş: `// the customer may pay transfer fees` bulgu sayılır (güvenli yön). Bugünkü kodda kaçan ad yok (geçici sınamayla gösterildi, silindi). | DEFERRED_BACKLOG'a forward → D-105 (T401) |
| K2 | `SmtpCaptureEmailGateway.Compose` biçimsiz yanıt adresinde `FormatException` sızdırır (`09 §11.1` m.2); yalnız geliştirme ağ geçidi, adres firmanın doğrulanmış ayarı. | DEFERRED_BACKLOG'a forward → D-106 (T407) |

### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu (kriter 1–9 ve KK1–KK9 aynı sonuç); öz denetim tablosu var, her maddede kanıt yazılı.
- Fark 1: yapım raporu tam Integration koşusunu yerelde yapmadı ("tamamı CI'da"); doğrulayıcı 1196 testi yerelde filtreyle böldü, yeşil.
- Fark 2: yapım raporu tarama darlığını "dar istisna" yönünden denetledi; desen boşlukları (K1) ve geliştirme ağ geçidindeki istisna sızıntısı (K2) raporda yok.
- Fark 3: yapım raporu `12` satırlarını anmıyor; doğrulayıcı `VAL-E054`/`E055`/`E057`/`E058`/`E059`'u PASS yaptı, `E056`/`E064` BEKLEMEDE.

### Merge ve Post-merge
- Squash: yardımcının işi (17. adım) — bu ajan merge etmedi
- Post-merge run'lar: yardımcının işi (18. adım)
