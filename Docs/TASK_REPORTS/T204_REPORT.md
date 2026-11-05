# T204 — Dış servis çökmesi davranışı ve sahtelerle sınama

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-05

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
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

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
- **CI:** *(izlenmedi — yönetici izler)*
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
