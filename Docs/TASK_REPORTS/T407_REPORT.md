# T407 — SES: gönderim ve sonuç sınıfları

**Faz:** F4 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; kriter 9'un gerçek hesap payı OWNER_INPUT — D-1015) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `AWSSDK.SimpleEmailV2` 4.0.102.1 eklendi (Apache-2.0; nuspec okundu, geçişli `AWSSDK.Core` 4.0.100.9 aynı lisans): `Directory.Packages.props`, Web projesi ve üç kilit dosyası.
- `Infrastructure/Integrations`: `SesClientFactory` (tek istemci; `MaxErrorRetry = 0`, `Timeout` = iş süresi 30 sn, açık `BasicAWSCredentials`, `IgnoreConfiguredEndpointUrls`), `SesEmailGateway` (ön sorgu `GetSuppressedDestination` + `SendEmail`, iki ayrı çalıştırıcı çağrısı; kaçışlı gönderen adı), `SesErrorMapping` (saf eşleme), `SesRegistration` (`AddEmailSendingService`); `IntegrationRegistration` `EmailCapture:Host` yoksa bunu kaydeder.
- `InstallationSettings/EmailCredential` (`email_credential` iki satır: 1. erişim anahtarı kimliği, 2. gizli anahtar; `ToString` gizli) ve `SettingsAudit.AddCredentialShapeFailure` (yalnız `EmailCapture:Host` boşken; geliştirmede her boş olmayan metin açar). D-79 kapandı.
- D-106: `SmtpCaptureEmailGateway` yanıt adresini `TryCreate` ile kurar. Kapandı.
- `scripts/ci/package-build.sh` ve `scripts/local-reference/host-secrets.sh` kimlik dosyasını iki satıra çevirdi.
- Belgeler: `INTEGRATION_RUNBOOKS/AMAZON_SES.md` (9 bölüm, IAM politikası); `08 §4.1` DV-16/17/19/21/23 "Kısmen gözlendi 2026-11-10"; `09` Kütüphaneler satırı ve §11.2 m.1–2; `DEPLOY_RUNBOOK` §A satır 4, §B satır 13, §J.2; `compose.dev.yaml` yorumu.
- Kayıt: `DEFERRED_BACKLOG.d/T407.md` (D-1015, D-1016, D-1017), `STATUS_CHANGELOG.d/T407.md`, `IMPLEMENTATION_STATUS` T407 satırı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/Ses*.cs`, `IntegrationRegistration.cs`, `SmtpCaptureEmailGateway.cs`; `Infrastructure/InstallationSettings/EmailCredential.cs`, `SettingsAudit.cs`.
- Testler: `Integrations/Ses*Tests.cs`, `SesWire.cs`, `InstallationSettings/EmailCredentialTests.cs`, `IntegrationRegistrationTests.cs`, `NotificationChannelSourceScan.cs` (yalnız iki tam dosya adı eklendi), `LibraryTableConsistency.cs` (yeni paket eklendi listesinden çıktı), `EmailCaptureRegistrationTests.cs` (Integration).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Bir çağrı bir alıcı; Cc, Bcc ve altı yasak alan yok | ✓ | `SesEmailGatewayTests.SendAsync_SendsOneCallToOneRecipient_WithNoCcNoBccAndNoneOfTheSixFieldsOfTheNotSentRow`; `…TwoMessages_AreTwoCallsEachToItsOwnRecipient` |
| 2 | Tek istemci, yeniden deneme kapalı, 30 sn, açık kimlik, ortam zinciri yok | ✓ | `SesClientAndMappingTests.CreateConfig_HasNoRetry_…`, `Client_SignsWithTheSettingsKey_…EvenWhenTheEnvironmentHoldsAnotherIdentity`, `Client_AnswersThatTheLibraryWouldRetryByDefault_GoOnce`; Integration `EmailCaptureRegistrationTests.Application_WithoutTheDevelopmentSetting_UsesTheServicesGateway_OnOneClient` |
| 3 | Anahtar, gizli anahtar, bölge, gönderen adresi zorunlu; eksikse açılmaz | ✓ | `EmailCredentialTests.Audit_WithoutAnyOfTheThreeRequiredEmailSettings_…`, `Audit_WithAnotherShape_Fails_…`; Integration `…WithAOneLineCredentialAndNoCaptureHost_RefusesToStart_…` |
| 4 | Kaçışlı marka adı (boşsa alan adı), yanıt adresi (boşsa yok), HTML + düz metin UTF-8 | ✓ | `SendAsync_PutsTheEscapedBrandNameAndTheSenderAddressInFrom_…`, `…WithoutAReplyAddress_…`, `…WithAnEmptyBrandName_…`, `SenderText_…` (7 satır) |
| 5 | Ön sorgunun üç kolu; sorgu hatası kanal hâline yazılmaz; adres yazıldığı gibi sorulur | ✓ | `SendAsync_RecipientOnTheListAsABounce_…`, `…RecipientNotOnTheList_404_Sends_…`, `…PreQueryAnswersAnErrorThatIsNot404_…`, `…PreQueryFailsAndTheSendFails_…`, `…AsksTheRecipientAsItWasEntered_…` |
| 6 | Hata eşlemesi yalnız SendEmail yanıtını alan saf işlev: 200 `Sent`, ötekiler `ChannelError` | ✓ | `SesClientAndMappingTests.Classify_*` (tablonun her satırı `TableExceptions` Theory'si), `SendAsync_EveryAnswerOfTheTable_…`, `Classify_ANotFoundOfTheSend_IsTheTablesRow` |
| 7 | `MessageId` saklanmaz; kayıtta yalnız kod ve sınıf | ✓ | `SendAsync_Success_ReturnsNothingOfTheAnswer_…`, `…RecipientError_WritesNoLine_AndKeepsNoAddress`, `…PreQueryFailureIsInTheLogOnlyWithItsOwnPrefix_…`, `Classify_EveryOtherAnswer_…NeverTheMessage` |
| 8 | Üç izin; gönderme kimlik ARN'i ve `ses:FromAddress`; öteki ikisi koşulsuz ayrı ifade; silme yok; `AMAZON_SES.md`'de | ✓ | `AMAZON_SES.md` §4; `SesPolicyRunbookTests` (üç test) |
| 9 | `AMAZON_SES.md` aynı PR'da; DV-16/17/19/21/23 gözlenip satıra yazılır | ~ | Runbook ✓ (`SesVerificationRowsTests.TheRunbook_HasTheTemplatesNineSections`); beş satır çevrimdışı gözlemle tarihli (`TheFiveRows_CarryAVerificationDate_…`); gerçek hesap gözlemi **OWNER_INPUT** (D-1015) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 tek alıcı, yasak alanlar yok | ✓ | kriter 1 testi; gövdenin alan kümesi tam eşitlik (`Content, Destination, FromEmailAddress, ReplyToAddresses`) |
| KK2 yeniden deneme sıfır, çağrı bir, 30 sn; ortam kimliği yok | ✓ | kriter 2 testleri; `SendAsync_SendThatHangs_EndsAtTheJobLimit…` |
| KK3 ayar eksikse açılış durur | ✓ | kriter 3 testleri (Unit + Integration) |
| KK4 ad kaçışı, yanıt adresi, iki boş kol | ✓ | kriter 4 testleri |
| KK5 ön sorgunun üç kolu; sorgu hatası kanal hâlini değiştirmez | ✓ | kriter 5 testleri (404, 200 + `BOUNCE`, başka neden, başka hata, zaman aşımı) |
| KK6 tablonun her satırı bir test | ✓ | `TableExceptions` ve `SendAsync_EveryAnswerOfTheTable_…` Theory'leri (SDK türü satırı + gerçek HTTP satırı) |
| KK7 `MessageId` saklanmaz; kayıtta adres ve anahtar yok | ✓ | kriter 7 testleri; `EmailCredentialTests.ToString_DoesNotPrintTheSecret` |
| KK8 politika | ✓ | `SesPolicyRunbookTests` |
| KK9 DV satırlarında tarih | ✓ (kısmen gözlem) | `SesVerificationRowsTests` |
| `09 §25` ilgili satırlar | ✓ | `09` Kütüphaneler satırı (lisans, sürüm, sınıf adı) + §11.2; `LibraryTableConsistencyTests` geçer |
| Güvenlik taraması | ✓ | kimlik değeri hiçbir kayıt, istisna ya da `ToString` çıktısında yok; `IntegrationSourceScan` sağlayıcı adı yalnız `Ses*` dosyalarında |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `NotificationChannelSourceScan.GatewayFiles`'a yalnız iki tam dosya adı (`SesEmailGateway.cs`, `SesRegistration.cs`); ikisi de `IEmailGateway` adını gerçekten taşıyor (`TheTwoExactLists_…` geçer) |
| Sık FAIL 2: tarama çağrı biçimini kaçırıyor | ✓ | yeni tarama kuralı eklenmedi; mevcut `IntegrationSourceScan` Ses dosyaları için Theory satırı var |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | — | şema değişikliği yok |
| Sık FAIL 4: kural değişince aynı sayıyı anan yerler | ✓ | kimlik biçimi: `DEPLOY_RUNBOOK` §A/§B/§J.2, `compose.dev.yaml`, `package-build.sh`, `host-secrets.sh`, `AMAZON_SES.md`, `09 §11.2`, testlerin kimlik değerleri güncellendi; `grep` ile `Email__Credential` ve `email_credential` taraması yapıldı; `deploy/.env.example` ve `compose.yaml` biçimden bağımsız |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ⚠ | iki satırlı biçim `08`/`05`'te yok, D-79 ile istenmiş: karar kaydı gerektirebilir → D-1016 |
| Backlog: D-79, D-106 kapatıldı; T408 devri | ✓ | `DEFERRED_BACKLOG.md` iki satır yerinde ✅; D-1017 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 5924/5924 (T407'nin Ses, EmailCredential, Smtp, politika, DV sınıfları 154+5) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests --no-build --filter-namespace …` (PostgreSQL 18, `track_commit_timestamp=on`) | Web 597/597; Jobs, Outages, Pages, Administration, FileSecrets, Logging, Security, Commands, Concurrency, SignedEndpoints, Payment 408/408; Ordering, Catalog, CorporateContent, Files, Disposal 1044/1044; Model ve Database ad alanları koşulmadı (e-posta kaydına dokunmaz; CI koşar) |
| Derleme | ✓ | `dotnet build -c Release -warnaserror` (Web, UnitTests, IntegrationTests) | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes` (üç proje) | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-10; head `ed0866f`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-1022 (ön sorgunun aşım ve bağlantı hatası kayıt satırı `suppression-query:` önekini taşımaz); K2 → mevcut D-442/D-966 (kalan pay logo; T407'ye düşen yok) |

**Tur 1 kanıtları (doğrulayıcı):** kriter 1–8 ve KK1–KK8 ✓ (kod okundu: `SesEmailGateway`, `SesErrorMapping`, `SesClientFactory`, `SesRegistration`, `EmailCredential`, `SettingsAudit`; testler kendi koşumumda geçti). Kriter 9 ✓ depo payı: `AMAZON_SES.md` 9 bölümle açıldı, DV-16/17/19/21/23 "Kısmen gözlendi (2026-11-10, T407; çevrimdışı)" tarihli; gerçek hesap gözlemi OWNER_INPUT (AWS hesabı yok) — iz D-1015, `VAL-E017`/`E018`/`E020`/`E022`/`E024` `BEKLEMEDE` kalır (KK9 "DV satırlarında tarih" ✓). KP-66 satırları (`VAL-A317`…`A325`) bildirim akışlarıdır, sahte altyapıyla kanıtlanır ve T407'nin kriterlerine bağlı değildir: `BEKLEMEDE` değişmedi. Ana dal CI son 3: 42796731944, 42796731464, 42787134973 success. Dal CI 42797568953 success (head `ed0866f`). Yerel: restore, `build -warnaserror` 0 uyarı; `format --verify-no-changes` üç proje temiz; Unit 5924/5924 (main birleştirmesinden sonra 6006/6006); betik testleri 4/4; Integration (PostgreSQL 18, `track_commit_timestamp=on`) `EmailCaptureRegistrationTests`, `SecretFilesTests`, `LoggingFilesTests`, `DispatchRetryAndMarkTests`, `StartupSettingsCheckTests`, `RevertLinkDigestRewriteStartupTests` 97/97. Güvenlik: sır değeri kayıtta, istisna metninde ve `ToString`'de yok; test değerleri uydurma; auth/girdi yüzeyi yok; yeni bağımlılık `AWSSDK.SimpleEmailV2` (Apache-2.0, `09` tablosunda). Yapım raporuyla uyum: tam (raporun kriter 9 "~" işareti gerçek hesap payıdır; doğrulayıcı depo payını ✓, gerçek payı `BEKLEMEDE` olarak ayırdı).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `email_credential` içeriği iki satır (önceden "boş olmayan metin"); `package-build.sh` ve `host-secrets.sh` güncellendi
- **Paketleme / dağıtım:** Var — kimlik dosyası biçimi (`DEPLOY_RUNBOOK`)
- **Yeni bağımlılık:** Var — `AWSSDK.SimpleEmailV2` 4.0.102.1 (Apache-2.0, AWS'nin resmî istemcisi; `09 §3.3` ölçütü, `09` Kütüphaneler satırı)

## Commit & PR

- **Branch:** `task/T407-ses-gonderim`
- **Commit:** `b0952ce5` — T407: SES: gönderim ve sonuç sınıfları
- **PR:** #515
- **CI:** yoklanmadı (yönetici izler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | DV-16 üç izin ve koşul | politika metni testi; gerçek hesap yok | ~ OWNER_INPUT |
| 2 | DV-17 bölge uç adresi | SDK isteği `email.eu-central-1.amazonaws.com/v2/email/outbound-emails`; gerçek çağrı yok | ~ OWNER_INPUT |
| 3 | DV-19 izleme yok | istek gövdesinde yapılandırma kümesi alanları yok; ham başlık gözlenmedi | ~ OWNER_INPUT |
| 4 | DV-21 Türkçe ve kaçışlı ad | istek gövdesi UTF-8, kaçış testli; SES kabulü gözlenmedi | ~ OWNER_INPUT |
| 5 | DV-23 hata türleri | SDK türleri eşlenir; SES'in hangi koşulda hangisini döndürdüğü gözlenmedi | ~ OWNER_INPUT |

OWNER_INPUT: AWS hesabı ve IAM kullanıcısı (üç izinli), SES'te doğrulanmış alan adı, üretim erişimi (DV-22 için konsoldan eklenen adres). Bunlar olmadan beş DV satırı "Kısmen gözlendi" kalır; kriter 9 kısmen.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gerçek hesapta DV-16/17/19/21/23 gözlemi | D-1015 | gerçek hesap yok (uydurulmadı) |
| 2 | İki satırlı kimlik biçimi `08`/`05`'te yok; karar kaydı | D-1016 | belge kuralı değişikliği |
| 3 | `QueryBouncesAsync` yer tutucu (`not-built`) | D-1017 | T408'in işi |

## Notlar

- **⚠ (kolay, kurala en uygun seçenekle karar):** (a) kimlik iki satır (D-79'un istediği; `08`'e yazılmadı, D-1016); (b) `NotificationChannelSourceScan.GatewayFiles`'a iki tam dosya adı eklendi; (c) AWS ortam değişkeni testi: SDK ortam değişkenlerini statik önbelleğe alabildiğinden, testin "ortamda başka kimlik varken" kolu kısmen zayıftır; kimliğin ve bölgenin açık verilmesi imzadaki anahtar kimliği ve uç adresiyle ayrıca sınanır; (d) D-442: gönderen markayı `GetBrandIdentityAsync`'ten T385'te kurdu (`DispatchSender`); T407 `EmailMessage.SenderDisplayName`'i olduğu gibi gönderir, markaya dokunmaz; logo D-966'da.
- VAL-E017, VAL-E018, VAL-E020, VAL-E022, VAL-E024 (`12 §4.16`) gerçek hesap gerektirir: `BEKLEMEDE` kalır.
- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum"a dokunulmadı (parça dosyası kuralı)
