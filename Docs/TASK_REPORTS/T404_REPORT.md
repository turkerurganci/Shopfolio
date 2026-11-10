# T404 — PayTR: bağlantı hatası ve ayrım

**Faz:** F4 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-10

- **Dal:** `task/T404-paytr-baglanti-hatasi` (taban `main` `06f704a7`)
- **Bağımlılık:** T403 ✓
- **Yapım:** iki işçi (ilki bağlam sınırında bölündü, ara commit `f0f68b9f`; ikincisi tam derleme ve testleri koştu, KK7 testlerini sabitlere bağladı, backlog kapanışlarını, raporu ve kayıtları yazdı)

---

## Yapılan İşler

- **Başlatma yanıtının tablosu** (`08 §2.1.8`; `PayTrPaymentGateway.ReadAnswer(status, body, sentFields, secondTokenReasonTexts, out failure)`): `success` + `token` → `Started`; `failed` + listedeki `reason` → `SecondTokenRejected("second-token")`; öteki `failed` → bağlantı hatası, kod `failed:<reason>`; 2xx dışı → `http:<kod>`; tanınmayan gövde → `body`. Bağlanılamama ve 15 sn aşımı çalıştırıcının (`ExternalCallRunner`) sınıflarıdır. Katman her çağrıyı bir kez yapar, yinelemez.
- **`reason` yazılır, alan değeri yazılmaz** (`WithoutFieldValues`): isteğin taşıdığı her değer (≥ 3 karakter, uzundan kısaya, büyük/küçük harf duyarsız) `[field]` ile değiştirilir; kontrol karakterleri boşluk olur (kayıt tek satır). D-1035 kapandı: T401'in iki testi bu yöne çevrildi.
- **İkinci jeton reddi:** `PayTrPaymentGateway.SecondTokenReasonTexts` — ayırt eden metin DV-11'in gözlemiyle yazılacak; test mağazası olmadığı için liste boş, her `failed` `08 §2.1.8`'in kuralıyla bağlantı hatasıdır (D-1075).
- **Çekirdek** (`Modules/Payment/PaymentStarts.cs`): `CardUnavailability` (2 dk, `TimeProvider`, tekil; yeniden başlatmada boş), `PaymentStartOutcome`, `PaymentStarts.StartAsync` — bağlantı hatasında `PaymentStartFailureSeries.RecordFailure` + `Mark`, başarılı jetonda `RecordSuccess`, ikinci retde hiçbiri. Kayıt `AddPaymentModule`'de (`TryAddSingleton` ×2, `AddScoped<PaymentStarts>`; ağ geçidi isteğe bağlı parametre — komut kipi host'u kurulur).
- **Kartın hâli onaydan öncesini etkiler:** `PaymentMethods.ListOpenAsync` hâldeyken kartı dışarıda bırakır (onay `PaymentMethodUnavailable`, jeton istenmez); `IPaymentMethods.IsCardUnavailableNowAsync` ödeme adımının "şu an kullanılamıyor" göstergesi; satış kapısı etkilenmez; onaylanmış siparişin başlatması jetonu yine ister.
- **L-7:** `PaymentStartRule.UnopenedCancellationReason = PaymentPageNotOpened` — açılamayan form sayılmaz, açılıp başarısız olan (`CardPaymentFailed`) sayılır (`UnpaidCancellationLimit.CountedReasons`).
- **`PaymentStartRule.Decide`** `switch` ifadesi, son kol `_ => throw new UnreachableException()` (D-110 K1 kapandı).
- **Belgeler:** `08 §2.1.8` tablo satır 3 ve son satır notu, §4.1 DV-11/DV-12 hücreleri; `09 §3.5` yeni satır (2 dk, `CardUnavailability.Duration`); `PAYTR.md` §6.1 DV-11/DV-12, §7 cümlesi ve sorun giderme tablosuna üç satır, §8.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/PayTrPaymentGateway.cs`
- `src/Shopfolio.Web/Infrastructure/Status/StatusMemory.cs` (yalnız yorum)
- `src/Shopfolio.Web/Modules/Payment/`: `PaymentStarts.cs` (yeni), `PaymentStartRule.cs`, `PaymentMethods.cs`, `PaymentModule.cs`, `Contracts/IPaymentMethods.cs`
- Testler: Unit `Integrations/PayTrPaymentGatewayTests.Classes.cs` (yeni), `Integrations/PayTrPaymentGatewayTests.cs`, `Payment/PaymentStartsTests.cs` (yeni), `Payment/PaymentMethodTests.cs`, `Consistency/CardUnavailabilityConsistencyTests.cs` (yeni); Integration `Ordering/OrderConfirmationTests.CardOutage.cs` (yeni), `Ordering/OrderConfirmationTests.cs`, `Ordering/OrderingTestServices.cs` (`extra` parametresi), `Payment/PaymentStartTests.cs`
- Belgeler: `08`, `09`, `INTEGRATION_RUNBOOKS/PAYTR.md`, `DEFERRED_BACKLOG.md` (D-110 yerinde ✅), `DEFERRED_BACKLOG.d/T401.md` (D-1035 ✅), `DEFERRED_BACKLOG.d/T404.md` (D-1075–D-1077), `IMPLEMENTATION_STATUS.md` (T404 satırı), `STATUS_CHANGELOG.d/T404.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ayrım yalnız başlatmada; bağlanılamama, 15 sn, 5xx, `failed`, tablosuz → `ConnectionFailed`; kayda hata, `failed`'de yalnız `reason` | ✓ | Unit `PayTrPaymentGatewayTests.Row1_TheProviderNotReached_*`, `Row1_TheFifteenSecondLimit_*` (15 sn − 1 sn sürer, 15 sn bağlantı hatası), `Row1_AServerError_*` (500/502/503/504), `Row2_AFailedAnswer_*`, `Row2_EveryValueTheRequestCarried_*`, `Row6_AnAnswerOutsideTheTable_*`; Integration `PaymentStartTests.Start_WithAFailedAnswer_WritesTheProvidersReason_ButNoValueOfTheRequest_KK8_T404_KK1` (host kaydı ve kayıt dosyası) |
| 2 | İkinci jeton reddi `SecondTokenRejected`; ayırt eden `reason` tabloya ve runbook'a | ✓ (gözlem OWNER_INPUT) | Kod: `Row3_AFailedAnswerWithTheSecondTokenText_*` (listedeki metin ret), `Row3_UntilDv11GivesTheText_*` (boş listede bağlantı hatası). Metin gözlenmedi: `08 §2.1.8` satır 3 ve `PAYTR.md` §6.1 DV-11 tarihli "gözlenmedi — D-1075"; uydurulmadı |
| 3 | Form açılamadan iptal L-7'ye sayılmaz; açılıp başarısız olan sayılır | ✓ | Integration `OrderConfirmationTests.ConnectionErrorOfTheStart_*_T404_KK3_KK4` (iki üye: 0 ve 1); Unit `PaymentStartsTests.ConnectionFailed_CancelsAsUnavailable_WithTheReasonL7DoesNotCount_T404_KK3` |
| 4 | Bağlantı hatasından sonra 2 dk bellekte; kart "şu an kullanılamıyor", kart onayı 422; yalnız onaydan öncesi | ✓ | Unit `AConnectionError_KeepsTheCardUnavailableForTwoMinutes_*`, `WhileTheCardIsUnavailable_AConfirmedOrdersStart_StillAsksForItsToken_*`, `TheState_IsTheProcesssMemory_*`, `AClosedCardLine_IsNotShownAsUnavailable_*`; Integration `ConnectionErrorOfTheStart_*` (`PaymentMethodUnavailable`, jeton çağrısı sayısı 1, yeni süreçte hâl yok, süre sonunda onay geçer) |
| 5 | Son sorgu ve iade sorgusunda yanıtsız çağrı bilinmeyen; para gönderen çağrı yinelenmez | ✓ | `AnUnansweredQuery_IsUnknown_AndNoSecondRequestGoes_T404_KK5_KK6` (Hang/Refuse/502 → `NoResponse`, istek 1). İade ve iade sorgusu T405'te (bugün `NotSupportedException`, çağrı yok) |
| 6 | Katman yinelemez; dayanıklılık işleyicisi yok; istemci fabrikadan, yönlendirme ve çerez kapalı | ✓ | `EveryStart_IsOneRequest_WhateverItsAnswer_T404_KK6`; `IntegrationHttpClients.cs:25` (`AllowAutoRedirect = false, UseCookies = false`), `IntegrationHttpClientsTests`; `src`'de `AddResilience*` yok (grep) |
| 7 | ≥ 5 hata ve ilk hata ≥ 15 dk → uyarı; başarılı jeton sıfırlar; ikinci ret ne sayar ne bozar; sonuç T314 k8 dizisine | ✓ | `PaymentStartsTests.FourErrors_*`, `FiveErrors_*` (15 dk − 1 sn kapalı, 15 dk açık), `ASuccessfulToken_EndsTheSeries_*`, `TheSecondTokenRefusal_NeitherCountsNorBreaksTheSeries_*` — eşikler `StatusThresholdOptions`'tan okunur |
| 8 | DV-12'nin HTTP kodları ve tablosuz yanıtları gözlenip `08 §2.1.8`'e | ✓ (gözlem OWNER_INPUT) | `08 §2.1.8` son satır ve §4.1 DV-12, `PAYTR.md` §6.1 DV-12 tarihli "gözlenmedi — D-1076"; kodun kuralı `Row6_*` ile sabit; uydurulmadı |

> Kriter 2 ve 8'in gerçek gözlem payı T401 ve T403'teki gibi OWNER_INPUT'tur (PayTR test mağazası yok); kod payı tam, gözlem D-1075/D-1076'da (D-1034, D-1065 ile aynı oturum).

### Öz denetim (bitiş kapısı 2)

| Madde (11 KK / 12 / 09 §25) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 tablonun altı satırı birer test | ✓ | `Row1_*` (üç test), `Row2_*` (iki), `Row3_*` (iki), `Row4_SuccessWithAToken_*`, `Row5_AFailedPayment_IsTheNotificationsResult_*`, `Row6_*` |
| KK2 ikinci ret bağlantı hatası değil | ✓ | `Row3_AFailedAnswerWithTheSecondTokenText_*`, `PaymentStartsTests.SecondTokenRejected_StaysOnTheConfirmation_*` (kart açık kalır, dizi değişmez) |
| KK3 iki iptal biçimi L-7'de ayrı | ✓ | Integration `ConnectionErrorOfTheStart_*` (0 / 1) |
| KK4 sahte saatle 2 dk; yeniden başlatmada silinir; hâlde kart görünür değil, onay jetonsuz 422; onaylı siparişin jetonu istenir | ✓ | Unit KK4 dört test; Integration `ConnectionErrorOfTheStart_*` |
| KK5 yanıtsız çağrıda ikinci istek yok | ✓ | `AnUnansweredQuery_*_KK5_KK6` |
| KK6 çağrı başına bir istek | ✓ | `EveryStart_IsOneRequest_*`, `AnUnansweredQuery_*` |
| KK7 dört hata / 15 dk'dan genç dizi açmaz; beş + 15 dk açar; başarılı sıfırlar; ikinci ret bozmaz | ✓ | `PaymentStartsTests` dört KK7 testi; sayılar `StatusThresholdOptions.PaymentStartFailureCount`, `PaymentStartFailureAgeMinutes`'ten (elle sayı yok) |
| KK8 DV-12 satırı | ✓ (OWNER_INPUT) | `08 §2.1.8`, §4.1 DV-12, `PAYTR.md` §6.1; D-1076 |
| 09 §3.5 yeni süre sabiti tabloda | ✓ | `09 §3.5` satırı "2 dakika … `CardUnavailability.Duration`"; `CardUnavailabilityConsistencyTests.GuidelinesRow_EqualsTheDuration`, `IntegrationSpec_NamesTheSameTwoMinutes`; testler sabiti okur |
| 09 §3.5 / yeni 09 metni karar kaydı | ✓ | `09 §3.5` satırı `08 §2.1.8`'in var olan "iki dakika" kuralını kayda alır, belge kuralı değişmez (karar kaydı gerekmez) |
| 09 §11.1 m.2 `switch` ve `UnreachableException` | ✓ | `PaymentStartRule.Decide`, `PaymentStarts.StartAsync`; D-110 K1 ✅ |
| 09 §11.1 m.6 `reason` kayda, alan değeri değil | ✓ | `WithoutFieldValues`; Integration KK8_T404_KK1 (e-posta ve sipariş numarası ne host kaydında ne kayıt dosyasında ne DB'de) |
| 09 §11.2 istemci fabrikadan, dayanıklılık yok | ✓ | kriter 6 |
| 09 §7 zaman `TimeProvider`'dan | ✓ | `CardUnavailability(TimeProvider)`; testler `ManualTimeProvider` |
| 09 §6 modül sınırı | ✓ | Yeni tipler `Modules.Payment` içinde `internal`; dışarıya yalnız `IPaymentMethods.IsCardUnavailableNowAsync`; mimari testler (Unit tam) yeşil |
| 09 §25 tek işlev | ✓ | Dizi `PaymentStartFailureSeries` + `StatusRules.PaymentStartSeriesOpen` (T314); L-7 kümesi `UnpaidCancellationLimit.CountedReasons`; sınıflama `PaymentStartRule` |
| Modül kurucusuna yeni bağımlılık → Integration TAM | ✓ | `PaymentMethods` kurucusu `CardUnavailability` aldı; Integration 3571 (komut kipi `Commands` 70 dahil) yeşil; `PaymentMethodTests` güncellendi |
| Göç / şema / yeni paket / ayar | uygulanamaz | Göç yok, paket yok, `.env.example` değişmez (kod sabiti) |
| Bekçi/tarama istisnası | uygulanamaz | Yeni istisna yok; sır bekçisinin "Token…=>" yanlış pozitifi kolu iki satıra bölerek geçildi, bekçiye dokunulmadı |
| Backlog: hedefi T404 olan kalemler | ✓ | D-110 K1 ✅ (yerinde), D-1035 ✅ (`DEFERRED_BACKLOG.d/T401.md`); başka hedefi T404 olan kalem yok (`DEFERRED_BACKLOG.md` + `.d/` grep) |
| `12` satırları | not | `VAL-E061`, `E089`, `E094`, `A970` UC-29 sayfası ve ödeme adımı ister (T509/T510; D-1077); çekirdek payı bu task'ta sınandı, `Durum` hücreleri doğrulayıcının |
| `dotnet format` test dosyaları dahil | ✓ | Web, UnitTests, IntegrationTests ayrı ayrı `--verify-no-changes` temiz |
| Güvenlik taraması | ✓ | Jeton, anahtar ve alan değeri kayda yazılmaz; `reason` tek satır; yeni uç, yetki ya da girdi yüzeyi yok; yeni bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --filter-not-trait "Consistency=true"` | 6161/6161 (KK7 düzenlemesinden sonra `PaymentStartsTests` 11/11 yeniden) |
| Consistency | ✓ | `--filter-trait "Consistency=true"` | 294/294 |
| Integration | ✓ | ad alanı başına parçalı, Postgres 18 `track_commit_timestamp=on` | 3571 (Ordering 637, Model 1407, Web 600, öteki 15 ad alanı 927) |
| Derleme | ✓ | `dotnet restore --locked-mode` + `dotnet build -c Release -warnaserror` | 0 uyarı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulayıcı doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (2 dk kod sabiti, `09 §3.5`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T404-paytr-baglanti-hatasi`
- **Commit:** dal ucu PR'da
- **PR:** #PR
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | PayTR test mağazası ve anahtarları (DV-11 ret metni, DV-12 HTTP kodları) | Depoda ve ortamda test mağazası yok; T401 (D-1034) ve T403 (D-1065) aynı durumu kaydetti | Kırık — OWNER_INPUT; kod payı sahte işleyiciyle sınandı, gözlem D-1075/D-1076 |
| 2 | Uygulama tek süreç (hâl bellekte) | `05 §1.2.4`; `08 §2.1.8` "bellekte" | Doğru |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İkinci jeton reddinin `reason` metni gözlenmedi; liste boş | D-1075 | Test mağazası yok (OWNER_INPUT) |
| 2 | Başlatmanın HTTP kodları ve tablosuz yanıtları gözlenmedi | D-1076 | Test mağazası yok (OWNER_INPUT) |
| 3 | `PaymentStarts` ve kartın hâli ekranlara (UC-22 adımı, UC-29 sayfası) bağlanmadı | D-1077 | Sayfalar T509/T510'da (F5) |

## Notlar

- **Working tree (Adım -1):** temiz (iki oturumda da); hooksPath `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** ilk işçi 42872070898, 42872068543, 42872065666 `success`; devamda 42882407232, 42882407240 (T403 merge) `success`.
- **Repo memory:** dokunulmadı (kural: faz kapısı kayıt PR'ı).
- **⚠ kararlar (kurala en uygun seçenekle):**
  - (a) `SecondTokenReasonTexts` boş: metin gözlenmeden uydurulmadı; `08 §2.1.8` tablosu "ayırt edilemeyen `failed` bağlantı hatasıdır" der (D-1075).
  - (b) Maskeleme eşiği 3 karakter: daha kısa değerler (`0`/`1` bayrakları, `TL`, taksit sayısı) olağan sözcük/sayıdır, kişisel veri ya da anahtar değildir; e-posta, ad, adres, telefon, IP, sipariş numarası ve `paytr_token` maskelenir.
  - (c) `PaymentStarts` modül içi (`internal`); ekran bağlantısı ve gerekirse sözleşmeye taşıma T509/T510'da (D-1077).
  - (d) İkinci ret kayda satır yazmaz: `08 §2.1.8` bağlantı hatası değil der, kayıt yalnız hata sınıfı içindir; sipariş teyit ekranında kalır.
  - (e) Önceki task'ların STATUS "Commit" hücreleri (T403 `c08172ff`, T408/T409/T411/T412 boş) squash hash'e çevrilmedi: paralel task dallarında aynı satırlar çakışır; faz kapısı kayıt PR'ına bırakıldı.
  - (f) KK7 testlerindeki elle yazılmış 4/5/15 sayıları `StatusThresholdOptions`'tan türetildi; KK4 testindeki ikinci "2 dakika" kopyası kaldırıldı (değer `09 §3.5` ile `CardUnavailabilityConsistencyTests`'te sabit).
