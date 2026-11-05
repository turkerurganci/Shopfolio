# T204 — Dış servis çökmesi davranışı ve sahtelerle sınama

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Sahtelerin üç hata türü:** `UnitTests/Fakes/FakeFailures` (zaman aşımı, ulaşılamayan servis, listede olmayan yanıt → ailenin `FromFailure` sınıfı, çalıştırıcının neden kodlarıyla); üç sahteye `ExternalCallFailureKind? Outage` (servis kesikken her çağrı o türün sınıfını alır, çağrı kaydedilir, kuyruk tüketilmez; ödeme sahtesinde bildirim doğrulaması yerel hesap olduğu için etkilenmez).
- **Ödeme başlatma kuralı** `Modules/Payment/PaymentStartRule`: `Started` → ödeme formu, `ConnectionFailed` (bağlantı hatası, 15 sn zaman aşımı, sunucu hatası, listede olmayan yanıt) → "kullanılamıyor" iptali, `SecondTokenRejected` → onay sayfasında kalır (ayrı sınıf).
- **Son sorgu kuralı** `Modules/Payment/FinalPaymentQueryRule`: `Paid` → uygula, `NoPayment` → iptal, `NoResponse` → "sonucu bilinmeyen": siparişe dokunulmaz, `AskAgainOutcome()` = `JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval)` (5 dk).
- **E-posta deneme kuralı** `Modules/Notifications/DispatchRetrySchedule` (saf): kanal hatası da alıcı hatası da ilk denemeden 5 dk / 30 dk / 2 sa sonra yeniden denenir (anlar ilk denemeye göre; geç başlayan deneme sonrakileri kaydırmaz), üçüncü yeniden deneme de başarısızsa satır "ulaşmadı". Sayılar `ProductConstants` P-38 (üç sabit).
- **Tek denemenin yazımı** `Modules/Notifications/DispatchAttempts`: ağ geçidi çağrısı işlem dışında; sonuç satıra ve kanal hâline tek işlemde yazılır — kanal hatası diziyi bir artırır (ilk hata anı sıfırdan bire geçişte), gönderim sıfırlar ve son başarıyı yazar, alıcı hatası ne sayar ne bozar; "kuyrukta" olmayan satır sonuç almaz (ikinci yazım etkisiz). Başka satır, sipariş ya da yeni gönderim kaydı yazılmaz.
- **Google hata kuralı** `Modules/Membership/GoogleIdentityFailureRule`: ulaşılamayan Google → giriş/kayıt `SocialLoginFailed`, yeniden doğrulama `SocialReauthenticationUnavailable`; başarısız doğrulama → giriş `SocialLoginFailed`, yeniden doğrulama `SocialReauthenticationFailed`; yeniden deneme yok.
- **Kaynak taraması** `UnitTests/Architecture/OutageIndependenceSourceScan` + kasıtlı ihlal örneği `ViolationSamples/Outages/DeadlineWaitsForGateway.cs`: e-posta ağ geçidi yalnız `Modules/Notifications/` ve bağlantı katmanında, Google ağ geçidi yalnız `Modules/Membership/` ve bağlantı katmanında anılır — bir süre, iş ya da sayfa bu servisleri bekleyemez.
- **Testler:** Unit `Outages/` (kurallar, deneme anları), `Integrations/GatewayFakesTests` (üç hata türü), `Consistency/OutageConsistencyTests` (P-38 satırı ↔ sabitler, `07 §6.3.2` kodları, `08 §2.3` "Google'a ulaşılamadı" satırı); Integration `Outages/` (gerçek PostgreSQL: e-posta kesintisi, son sorgu kesintisi).
- **Belgeler:** `CONTEXT.md` klasör haritası (Unit/Integration `Outages/`, tarama); `DEFERRED_BACKLOG` D-97 notu, D-109.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/ProductConstants.cs`; `Modules/Notifications/DispatchRetrySchedule.cs`, `DispatchAttempts.cs`; `Modules/Payment/PaymentStartRule.cs`, `FinalPaymentQueryRule.cs`; `Modules/Membership/GoogleIdentityFailureRule.cs`
- `tests/Shopfolio.UnitTests/Fakes/` (`FakeFailures.cs` yeni; üç sahte), `Integrations/GatewayFakesTests.cs`, `Outages/` (2), `Consistency/OutageConsistencyTests.cs`, `Architecture/OutageIndependenceSourceScan*.cs` + ihlal örneği
- `tests/Shopfolio.IntegrationTests/Outages/EmailOutageTests.cs`, `FinalPaymentQueryOutageTests.cs`
- `.claude/CONTEXT.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sahte zaman aşımı, bağlantı hatası, beklenmeyen yanıt üretir; gerçek çağrı yok | ✓ | `FakeFailures`; `GatewayFakesTests.FakeFailures_HaveTheRunnersReasonCodes_ForTheThreeKinds`, `PaymentFake_/EmailAndGoogleFakes_QueuedFailure_IsTheFamilysClassOfThatFailure`, `Outage_AnswersEveryCallWithTheFailure_KeepsTheCalls_AndLeavesTheQueues`; `IntegrationSourceScanTests` kural 5 (sahteler ağ türü anmaz) |
| 2 | Başlatmada bağlantı hatası ve 15 sn zaman aşımı "bağlantı hatası", ikinci jeton reddi ayrı | ✓ | `PaymentStartRule`; `OutageRulesTests.PaymentStart_EachClass_HasItsOwnStep`, `PaymentStart_EveryFailureKindOfTheFake_IsAConnectionError_NotASecondTokenRefusal`, `PaymentStart_Timeout_IsAConnectionError_WithTheTimeoutReason` |
| 3 | Son sorgu yanıtsızlığı "sonucu bilinmeyen"; katman siparişe dokunmaz | ✓ | `FinalPaymentQueryRule`; Unit `FinalQuery_*` (3); Integration `FinalPaymentQueryOutageTests.UnansweredFinalQuery_IsAskedAgainEveryFiveMinutes_PastThePaymentPeriod_AndTheOrderRowNeverChanges` (üç hata türü; sipariş satırı `row_to_json` ile bayt bayt aynı) |
| 4 | Alıcı/kanal hatası ayrı; ikisi de 5/30/120 dk; kanal hâli: kanal hatası sayar, başarı sıfırlar, alıcı hatası dokunmaz | ✓ | `DispatchRetrySchedule`, `DispatchAttempts`; Unit `DispatchRetryScheduleTests` (8); Integration `EmailOutageTests.ChannelOutage_*`, `RecipientErrors_HaveTheSameMoments_AndNeitherCountNorResetTheChannelRun`, `SendingAfterAnOutage_SendsTheRow_ResetsTheChannelRun_AndWritesTheLastSuccess` |
| 5 | Daha uzun kesintide "ulaşmadı"; toplu yeniden gönderim yok | ✓ | `DispatchRetrySchedule.Apply` (`next is null` → `Undelivered`); Unit `FailingEveryTime_SetsTheMomentsFromTheFirstAttempt_ThenUndelivered`, `RowThatIsNotQueued_TakesNoResult`; Integration `ChannelOutage_*` (dördüncü denemeden sonra "ulaşmadı", sonraki yazım etkisiz) |
| 6 | Yanıtsız Google: giriş/kayıt `SocialLoginFailed`, yeniden doğrulama `SocialReauthenticationUnavailable`; yeniden deneme yok | ✓ | `GoogleIdentityFailureRule`; `OutageRulesTests.Google_EachClassInEachFlow_HasItsCode`, `Google_Unanswered_SignInFails_ReauthenticationIsUnavailable_WithOneCallEach` (sahte başına tek çağrı); Consistency `GoogleFailureCodes_AreNoticeCodesOf07`, `GoogleUnavailableRow_Of08_NamesTheCodesOfTheRule` |
| 7 | Hiçbir süre e-postaya/Google'a bağlı değil; ödeme süresi işler, iptal ertelenir; kesinti süreyi durdurmaz | ✓ | `OutageIndependenceSourceScanTests` (gerçek ağaç temiz + ihlal örneği); Integration son sorgu testi (sahte saat ödeme süresinin sonunu geçer, `PaymentWindowEndsAt` değişmez, iptal yok, sorgu 5 dk'da bir); `ChannelOutage_*` (geç başlayan deneme sonraki anları kaydırmaz) |
| 8 | Ürün kesinti için müşteriye bildirim üretmez | ✓ | Integration `ChannelOutage_*` (gönderim kaydı sayısı 2'de kalır, öteki gönderilmiş satır `row_to_json` ile aynı), son sorgu testi (`dispatch_records` 0) |

### Öz denetim (bitiş kapısı 2) — bütün maddeler yeniden denetlendi

Kaynak: `11` satır 953–970 (T204 kabul kriterleri ve kontrol listesi), `12` VAL-E056, E063–E065, `09 §25`; diff `origin/main...HEAD`. DRS = `Modules/Notifications/DispatchRetrySchedule.cs`, DA = `Modules/Notifications/DispatchAttempts.cs`.

| Madde (KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 sahteler üç hata türünü üretir | ✓ | `FakeFailures.cs:20`, `:32`; `FakePaymentGateway.cs:36`, `:95`; `FakeEmailGateway.cs:23`, `:68`, `:83`; `FakeGoogleIdentityGateway.cs:19`, `:50`; `GatewayFakesTests` dört yeni test |
| KK2 iki ödeme sonucu ayrı sınıf; başlatmada zaman aşımı "bağlantı hatası" | ✓ | `PaymentStartRule.cs:31`–48; `OutageRulesTests.cs:23`, `:32`, `:43` |
| KK3 yanıtsız sorguda sipariş satırı değişmez | ✓ | `FinalPaymentQueryRule.cs:31`, `:44`; `FinalPaymentQueryOutageTests` (dört tur, her turda satır metni aynı; durum, ödeme durumu, sürüm, süre sonu) |
| KK4 anlar 5/30/120 dk, alıcı hatasında da; kanal hâli | ✓ | DRS:50–79, :88–104; DA:55–94; `DispatchRetryScheduleTests.cs:39`, `:64`, `:80`; `EmailOutageTests` üç test (anlar `DispatchRetrySchedule.RetriesFromFirstAttempt`'ten türetilir, sayı elle yazılmaz) |
| KK5 son denemeden sonra "ulaşmadı" | ✓ | DRS:70–75; `DispatchRetryScheduleTests.cs:39`, `:119`; `EmailOutageTests.ChannelOutage_*` |
| KK6 yanıtsız Google'da giriş `SocialLoginFailed`, yeniden doğrulama `SocialReauthenticationUnavailable` | ✓ | `GoogleIdentityFailureRule.cs:39`; `OutageRulesTests.cs:82`, `:99` |
| KK7 sahte kesintide süre ilerler | ✓ | `OutageIndependenceSourceScan.cs` (iki kural, tam satır yorum boşaltılır, `.cs` + `.cshtml`); ihlal örneği 6 bulgu, klasör başına izin testleri, benzer adlar (`EmailGatewayOptions`, `GoogleIdentityResult`) bulgu değil, `Modules/NotificationsArchive/` önek kaçağı bulgu; Integration son sorgu testi (`Time > Start + PaymentPeriod`, sipariş Alındı + Bekliyor) |
| KK8 kesinti için müşteriye e-posta satırı yok; olağan satırlar etkilenmez | ✓ | DA:16–24 (yalnız satır + kanal hâli yazılır); `EmailOutageTests.ChannelOutage_*` (satır sayısı 2, öteki satır aynı); `FinalPaymentQueryOutageTests` (`dispatch_records` 0) |
| `09 §25` ilgili satırlar | ✓ | aşağıdaki tablo |
| Güvenlik taraması | ✓ | sır yok (test veritabanı bağlantısı ortam değişkeninden, yer tutucu şifre yalnız yerel konteynerde); kayıt satırı eklenmedi; dış çağrı işlem dışında (DA:45–46, işlem :58); yeni giriş yüzeyi yok |
| Sık FAIL listesi | ✓ | ayrı ihlal örneği var; tarama istisnası dar (klasör öneki `/` ile biter, önek kaçağı testli; yalnız tam satır yorum); test gerçek saate bağlı değil (`ManualTimeProvider`, `JobQueueHarness.Time`); beklenen anlar sabitten türetilir; geçici çözümler backlog'da (D-97 notu, D-109); aynı etiketi anan yerler: `CONTEXT.md` iki satır, D-97 kanıt sütunu |

**`09 §25` madde madde** (dokunulmayan alan "uygulanamaz" + gerekçe):

| Soru (kısaltılmış) | Cevap | Kanıt |
|---|---|---|
| Çekirdek ve Kaynak dokümanlar okunmuş mu | evet | `09 §19.1`, `08 §2.1.8`, §2.2.2, §2.3, §3.1, §3.3, `05 §4.4.3`, §10.1, §10.2, `03 §10.1`, `06 §4.58`, §4.60, `07 §6.3.2`, `02 §11.2` P-38 |
| Yeni ad sözlükten/06/07'den birebir mi | evet | bildirim kodları `07 §6.3.2` ile birebir (Consistency testi); alanlar `06 §4.58`, §4.60 (`AttemptCount`, `NextAttemptAt`, `ConsecutiveChannelErrorCount`, `FirstChannelErrorAt`, `LastSucceededAt`) |
| Kod hiçbir doküman cümlesiyle çelişmiyor mu | evet | anlar ilk denemeden (`08 §2.2`, P-38); alıcı hatası da yeniden denenir (`05 §4.4.3`); sonucu bilinmeyen sorgu iptal etmez (`08 §3.1` kural 3–4) |
| Diff yalnız görevin istediğini mi içeriyor | evet | kurallar, sahteler, testler, `CONTEXT.md`, backlog, rapor/status/hafıza |
| "Bilgim tarihli" olgular doğrulanmış mı | uygulanamaz | dış sürüm/API varsayımı yok |
| Yeni ayar/süre/tavan §3.5'te mi; `.env.example` | evet | P-38 üç ürün sabiti `ProductConstants`'ta (`09 §3.5` m.1: ürün sabiti evi), Consistency `DispatchRetryConstants_AreTheP38Row` + kırmızı sınaması; ortam değişkeni yok |
| Dosya düzeni §4.1; `CONTEXT.md` güncel | evet | kurallar sahibi modülde; `CONTEXT.md` Unit/Integration `Outages/` ve tarama |
| Modül sınırları; mimari testler yeşil | evet | Unit 1074 + Consistency 154 (mimari testler dahil) |
| Aynı işi yapan tek işlev çağrılmış mı | evet | işlem `ITransactionRunner`, yeniden zamanlama `JobOutcome.RescheduleAfter`, süre `JobTimings.FinalPaymentQueryInterval` |
| C# standartları, derleme ayarları | evet | `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 0; bastırma yok |
| Zaman yalnız `TimeProvider`'dan | evet | DA:45, :67 (`_time.GetUtcNow()`); testler `ManualTimeProvider` |
| Yeni paket | uygulanamaz | `*.csproj`, `packages.lock.json`, `*.props` değişmedi |
| Dış servis çağrısı işlem dışında; yeniden deneme yok | evet | DA:45–46 (çağrı işlemi açan `RunAsync`'ten önce, :58); kurallar yeniden denemez (Google tek çağrı testi) |
| İstisna yalnız beklenmeyen hâlde | evet | kurallar sınıfa göre dallanır; bilinmeyen sınıf `UnreachableException` |
| Ön yüz kodu | uygulanamaz | sayfa yok (sayfalar T404, T343, T409) |
| Durum geçişi, işlem izi, eşzamanlılık, para | uygulanamaz | sipariş geçişi yok (dokunulmadığı test edildi); kanal hâli artışı tek `UPDATE` ile satırın kendi değerinden (K-1072) |
| Arka plan işi | evet | son sorgu yeniden zamanlaması kuyruğun `RescheduleAfter`'ı; deneme sayısı değişmez (Integration) |
| Veri ve göç | uygulanamaz | göç, tablo, enum sütunu yok |
| Kayıtta sır ve kişisel veri yok | evet | yeni kayıt satırı yok |
| Güvenlik (sahtecilik, yetki, hız sınırı, `SecretKey`, yükleme) | uygulanamaz | yeni uç ya da giriş yüzeyi yok |
| Değişen kuralın Test maddesi karşılanmış mı | evet | yukarıdaki KK tablosu |
| Davranışı kilitleyen test; test silinmemiş/zayıflatılmamış | evet | `GatewayFakesTests` yalnız eklendi; mevcut test değişmedi |
| Doğru katman, sahteler `Fakes/`'ten, gerçek saat yok | evet | saf kurallar Unit, satır/kanal hâli/sipariş Integration (kendi koleksiyonu `Outages`) |
| Yeni yasak için tarama + ihlal örneği | evet | `OutageIndependenceSourceScan` + `DeadlineWaitsForGateway.cs` |
| Eşitlik denetimleri | evet | `OutageConsistencyTests` (`[Trait("Consistency","true")]`) |
| Performans | evet | deneme başına tek satır okuma + tek `UPDATE`; döngüde sorgu yok |
| PR tek amaçlı; commit/PR kurala uygun | evet | commit'ler `T204: ...`; dal izolasyonu yalnız T204 |
| CI yeşil mi | — | CI'ı yönetici izler (işçi beklemez) |
| Yama sürümü göç, refactor | uygulanamaz | yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... -c Release --filter-not-trait "Consistency=true"` | 1074/1074 |
| Unit (Consistency) | ✓ | `... --filter-trait "Consistency=true"` | 154/154 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... -c Release` (PostgreSQL 18 konteyneri `shopfolio-t204-pg`) | 1157/1157 (yeni `Outages` 8/8) |
| Contract | — | — | yok |

Derleme: `dotnet build Shopfolio.slnx -c Release -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 0.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-05, head `548c4db`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1, K2 → D-110) |

**VAL satırları (öneri):** `VAL-E064` PASS önerisi (deneme anları, "ulaşmadı", toplu gönderim yok — Integration). `VAL-E056` kısmi: kural ve sipariş satırının değişmemesi burada; gerçek son sorgu işleyicisi T403. `VAL-E063` kısmi: süre bağımsızlığı (tarama, son sorgu) burada; sipariş/ödeme geçişlerinin e-posta kesintisinde sürmesi T305 ve geçiş task'larında. `VAL-E065` kısmi: kural burada; ekranlar T343/T409 (kodların listeye bağlanması D-109).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (P-38 üç ürün sabiti)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T204-dis-servis-cokmesi`
- **Commit:** PR head
- **PR:** #398
- **CI:** 42077481379 success (head `548c4db`; doğrulayıcı okudu)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Bu task'ta dış varsayım yok (dış servis çağrısı yok; sahteler süreç içinde).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Kuyrukta" satırları seçen, gövdeyi kuran, `DispatchAttempts.SendAsync`'i çağıran gönderici ve DI kaydı | D-97 (T305/T385/T407) | gerçek `IEmailGateway` yalnız geliştirme yakalama modunda kayıtlı; kayıt şimdi `ValidateOnBuild`'i kırar |
| 2 | Google hata kodlarının kapalı `NoticeCode` listesine bağlanması ve ekranlar | D-109 (T214; T343; T409) | `NoticeCode` listesi T214'te |
| 3 | Son sorgu işleyicisi (siparişi okur, uygular ya da iptal eder) | — (planlı: T403; D-97) | sağlayıcı task'ı |
| 4 | Ödeme sayfasının iptali, 2 dk "kullanılamıyor" işareti, jeton hata dizisi | — (planlı: T404, UC-29) | sayfa task'ı |
| 5 | Yeniden denenmeyen sınır aşımı | — (planlı: T385) | `11` T204 kriter 4 |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** CI 42068910959 success, 42068911080 success (T203 #397)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** üç yapım işçisiyle yapıldı (devir notları 1–2). Test veritabanı konteyneri `shopfolio-t204-pg` iş bitince silindi.

## Doğrulama Sonucu — T204 Dış servis çökmesi davranışı — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T204-dis-servis-cokmesi   **Commit:** 548c4db (PR #398)

### Verdict: ✓ PASS

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (dala geçişten sonra ve koşulardan sonra) |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` (ana deponun bekçisi; daldaki kopya değil) |
| Ana dal CI (son 3) | ✓ | 42068910959 success, 42068911080 success (T203 #397), 42042657292 success (T202 #396) |
| Memory drift | ✓ | `grep -nE "\bT204\b" .claude/memory/MEMORY.md` → satır 22 |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sahte zaman aşımı, bağlantı hatası, beklenmeyen yanıt üretir; gerçek servise çağrı yok (`09 §19.1`) | ✓ | `Fakes/FakeFailures.cs` üç tür → ailenin `FromFailure`'ı (çalıştırıcının neden kodlarıyla); üç sahtede `Outage`; `GatewayFakesTests` 4 yeni test (×3 tür) yeşil; sahteler ağ türü anmaz (`IntegrationSourceScan` kural 5); Google'da `Unrecognized` → `Failed`, ötekiler `Unavailable` (`08 §3.1` "tabloda olmayan yanıt Google'da başarısızdır") |
| 2 | Başlatmada bağlantı hatası ve 15 sn zaman aşımı "bağlantı hatası", ikinci jeton reddi ayrı sonuç (`08 §3.1`, §2.1.8, §3.3) | ✓ | `PaymentStartRule.Decide`: `ConnectionFailed` → `CancelAsUnavailable`, `SecondTokenRejected` → `StayOnConfirmation`; `PaymentStartResult.FromFailure` her türü `ConnectionFailed` yapar; `IntegrationOperations`: `PaymentStart` → `UserWaiting` (15 sn, T202); `OutageRulesTests` (PaymentStart ×5) yeşil |
| 3 | Son sorgu yanıtsızlığı "sonucu bilinmeyen"; katman siparişe dokunmaz (`08 §3.1` k3–4) | ✓ | `FinalPaymentQueryRule`: `NoResponse` → `AskAgain` → `RescheduleAfter(FinalPaymentQueryInterval)`; Integration `FinalPaymentQueryOutageTests` (×3 tür, gerçek PG 18 + gerçek çalıştırıcı): 4 turda sipariş satırı `row_to_json` ile aynı, `Placed`+`Pending`, `Version` 1, iş satırı `Pending`, `AttemptCount` 0, sonraki an +5 dk |
| 4 | Alıcı/kanal hatası ayrı; ikisi de ilk denemeden 5/30/120 dk; kanal hâli: kanal hatası sayar, başarı sıfırlar, alıcı hatası dokunmaz (`08 §3.3`, §2.2.2; `05 §4.4.3`, §10.2.3) | ✓ | `DispatchRetrySchedule` (anlar ilk denemeden, geç başlayan deneme kaydırmaz; sabitler `ProductConstants` P-38); `DispatchAttempts` (çağrı işlem dışında, satır + kanal hâli tek işlemde, sayaç veritabanında `+1`); Unit `DispatchRetryScheduleTests` 8 test; Integration `EmailOutageTests` 3 test (×3 tür) — anlar, sayaç 1→4 ve ilk hata anı sabit, alıcı hatasında sayaç 2'de kalır, başarıda 0 + `LastSucceededAt`; Consistency `DispatchRetryConstants_AreTheP38Row` + kırmızı sınaması |
| 5 | Daha uzun kesintide "ulaşmadı"; toplu yeniden gönderim yok (`08 §3.3`) | ✓ | `Apply`: üçüncü yeniden denemenin hatasında `NextAttemptAt` boş + `Undelivered` (`06 §4.58` kısıtı: Queued dışında `NextAttemptAt` boş); "kuyrukta" olmayan satır sonuç almaz; Integration'da 4. denemeden sonra `Undelivered`, sonraki yazım `Applied=false`, satır sayısı 2 |
| 6 | Yanıtsız Google: giriş ve kayıt `SocialLoginFailed`, yeniden doğrulama `SocialReauthenticationUnavailable`; yeniden deneme yok (`08 §2.3`, §3.1) | ✓ | `GoogleIdentityFailureRule` (`08 §2.3` hata tablosunun "Google'a ulaşılamadı" satırı ile birebir; başarısız doğrulamada yeniden doğrulama `SocialReauthenticationFailed`); `OutageRulesTests.Google_*` (iki akış, akış başına tek çağrı); Consistency: kodlar `07 §6.3`'te, `08` satırı kodları anar |
| 7 | Hiçbir süre e-postaya ya da Google'a bağlı değil; ödeme süresi işler, son sorgu yanıtsızsa iptal ertelenir; kesinti süreyi durdurmaz (`08 §3.3`) | ✓ | `OutageIndependenceSourceScan` (e-posta ağ geçidi yalnız `Modules/Notifications/` + bağlantı katmanında, Google yalnız `Modules/Membership/` + katmanda; `.cs` + `.cshtml`) — gerçek ağaç temiz, ihlal örneği 6 bulgu, önek kaçağı testli; Integration son sorgu: sahte saat `PaymentWindowEndsAt`'i geçer, sipariş iptal edilmez, `PaymentWindowEndsAt` değişmez |
| 8 | Ürün kesinti için müşteriye bildirim üretmez (`08 §3.3`) | ✓ | `EmailOutageTests.ChannelOutage_*`: `dispatch_records` 2 satırda kalır, öteki gönderilmiş satır `row_to_json` ile aynı; `FinalPaymentQueryOutageTests`: `dispatch_records` 0 |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
T204'ün `10 KP` birimi yok; `11 §7.1` kaynağı `08 §3.3`. Satırlar `12`'de kaynağı `08 §3.3` olanlardan grep'le bulundu. Hiçbir satırın `Durum`'u değişmedi.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-E056 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE (T204 payı ✓: kural, siparişe dokunmama, 5 dk yeniden zamanlama — test-yerel işleyiciyle; gerçek `FinalPaymentQuery` işleyicisi T403, D-97) | — |
| VAL-E060 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE (T204 payı ✓: `PaymentStartRule`; iptal, L-7, sepet T404) | — |
| VAL-E061 | ORTA | mock | BEKLEMEDE | BEKLEMEDE (iki dakikalık hâl ve 422 T404/UC-22) | — |
| VAL-E062 | ORTA | mock | BEKLEMEDE | BEKLEMEDE (havaleyle satış — sayfa task'ları) | — |
| VAL-E063 | ORTA | mock | BEKLEMEDE | BEKLEMEDE (süre bağımsızlığı taraması ✓; sipariş/ödeme/durum geçişleri henüz yok) | — |
| VAL-E064 | ORTA | mock | BEKLEMEDE | BEKLEMEDE (T204 payı ✓: anlar, "ulaşmadı", toplu gönderim yok; satırları anında alan gönderici T385/T305/T407, D-97, D-99) | — |
| VAL-E065 | ORTA | mock | BEKLEMEDE | BEKLEMEDE (kural ✓; ekranlar T343/T409, kodlar D-109) | — |
| VAL-E066 | DÜŞÜK | yok | BEKLEMEDE | BEKLEMEDE (kargo takibi, T204 dışı) | — |

### Doğrulama Kontrol Listesi
- [x] KK1: sahteler üç hata türünü üretir — `GatewayFakesTests` (tek çağrı + kalıcı kesinti, kuyruk korunur, bildirim doğrulaması etkilenmez)
- [x] KK2: iki ödeme sonucu ayrı sınıfta; başlatmada zaman aşımı "bağlantı hatası" (`ConnectionFailed("timeout")`)
- [x] KK3: yanıtsız sorguda sipariş satırı değişmez (gerçek PG, bayt bayt)
- [x] KK4: deneme anları ilk denemeden 5/30/120 dk; alıcı hatasında da; kanal hâli kanal hatasında artar, başarıda sıfırlanır, alıcı hatasında değişmez
- [x] KK5: son denemeden sonra "ulaşmadı"
- [x] KK6: yanıtsız sahte Google'da giriş `SocialLoginFailed`, yeniden doğrulama `SocialReauthenticationUnavailable`
- [x] KK7: sahte kesintide süre ilerler (sahte saat ödeme süresinin sonunu geçer; tarama)
- [x] KK8: kesinti için müşteriye e-posta satırı yok; öteki satır etkilenmez
- [x] `09 §25` ilgili satırlar — evet (K1 dışında); uygulanamaz alanlar: göç, sayfa, yetki, para, yeni paket
- [x] Güvenlik taraması — temiz

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 1228/1228 |
| Unit (yeni sınıflar) | ✓ | `--filter-class` Outage*/DispatchRetrySchedule*/GatewayFakes* | 54/54 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration — Outages (yeni) | ✓ | `--filter-namespace "*Outages*"` | 8/8 |
| Integration — Jobs | ✓ | sınıf filtreleriyle üç parça | 46 + 16 + 15 = 77/77 |
| Integration — Web | ✓ | `--filter-namespace "*.Web"` | 74/75 birlikte; düşen `StartupDatabaseCheckTests.Start_WithTheTableOfARecordMissing_*` sınıf tek başına 18/18 |
| Integration — Concurrency + Security | ✓ | namespace filtresi | 44/44 |
| Integration — Database + Disposal + FileSecrets + Logging + SignedEndpoints | ✓ | namespace filtresi | 116/117 birlikte; düşen `DatabaseUsersTests.FourUsers_*` sınıf tek başına 17/17 |
| Integration — Model 1/2 | ✓ | `*.Model.A*…M*` | ilk koşuda koleksiyon fikstürü kuruluşu düştü (0 ms), yeniden koşu 499/499 |
| Integration — Model 2/2 | ✓ | `*.Model.P*`, `R*`, `S*` | 393/394 birlikte; düşen `SchedulerBackupDisposalAndKeyTablesTests` sınıf tek başına 55/55 |
| Dal CI | ✓ | `gh run list --branch task/T204-dis-servis-cokmesi` | 42077481379 success (head 548c4db; Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, Gate yeşil) |
| Temizlik | ✓ | `docker rm -f shopfolio-t204-val-pg`; `git status`; `dotnet restore --locked-mode` | temiz; restore 0 |

Yerel düşmeler T204'ün dokunmadığı sınıflarda, yalnız çok koleksiyonlu yük altında (Windows'ta Docker, ~15–20 sn süre aşımı) görüldü; her biri tek başına yeşil, dal CI'ında Integration yeşil. T204 bulgusu değildir.

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (sır, anahtar, bağlantı dizesi eklenmedi; testlerde yer tutucu değerler)
- Auth etkisi: Temiz (yeni uç, sayfa ya da yetki yok; Google kuralı yalnız kod döndürür)
- Input validation: Temiz (yeni girdi yüzeyi yok)
- Yeni bağımlılık: Yok (`*.csproj`, `packages.lock.json`, `*.props` değişmedi)

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| — | — | — | — | Bulgu yok | — | — | — |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `PaymentStartRule.Decide` sonuç ailesini `if … is` zinciriyle ele alıyor; `09 §11.1` m.2 "çağıran `switch` ifadesiyle her sınıfı ayrı kolda ele alır, son kol `_ => throw new UnreachableException()`" der. Davranış aynı (her sınıf ayrı dal, son satır `UnreachableException`, her sınıfın testi var). Commit 1549271 biçim düzeltmesinde `switch`'i zincire çevirdi; `switch` biçimi (`SecondTokenRejected =>` boşluklu) `dotnet format --verify-no-changes` ve `-warnaserror` derlemeden geçiyor (doğrulayıcı geçici olarak sınadı, geri aldı). | DEFERRED_BACKLOG'a forward — D-110 (T404) |
| K2 | `DispatchRetryScheduleTests.cs:12` yorumu var olmayan `DispatchRetryConsistencyTests`'i anıyor; eşitlik denetimi `OutageConsistencyTests.DispatchRetryConstants_AreTheP38Row`'dadır. | DEFERRED_BACKLOG'a forward — D-110 (T385) |

### Yapım Raporu Karşılaştırması
- Uyum: 1 uyuşmazlık
- Kabul kriterleri 1–8 ve KK1–KK8: tam uyumlu; öz denetim tablosu her madde için dosya:satır ve test adı taşıyor.
- Uyuşmazlık 1: yapım raporu `VAL-E064` için PASS önerdi; doğrulayıcı `BEKLEMEDE` bıraktı — satırın beklenen sonucu "üç deneme bu aralıklarla yapılır" ürünün satırları anında alıp gönderen göndericisini ister (T385/T305/T407; D-97, D-99); T204 anları yazan kuralı ve tek denemenin yazımını kurdu, testler denemeleri elle sürüyor. `VAL-E056`, `E063`, `E065` için raporun "kısmi" değerlendirmesiyle uyumlu (BEKLEMEDE).
- Yapım raporu `09 §25`'in §11 sorusuna (dış servis çağrısı sonuç sınıfıyla) "evet" yazdı; doğru, ama K1 o cevabı `09 §11.1` m.2'nin `switch` biçimi açısından sınırlar (bloke etmez).

### Merge ve Post-merge
- Squash: yardımcı yapar (doğrulama ajanı 16. adımda durur)
- Post-merge run'lar: yardımcı doldurur
