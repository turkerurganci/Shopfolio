# T202 — Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahteler

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-05

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
| Doğrulama durumu | *(doğrulama doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — geliştirme ayarı `EmailCapture:Host`/`Port` (yalnız `compose.dev.yaml`; kurulum ayarı değil)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (`System.Net.Mail.SmtpClient`, `IHttpClientFactory` çatıyla gelir)

## Commit & PR

- **Branch:** `task/T202-dis-servis-katmani`
- **Commit:** dal ucu PR'da
- **PR:** #396
- **CI:** *(PR'da)*
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
