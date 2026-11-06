# T215 — Hız sınırı, deneme limitleri ve gövde tavanı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Gövde tavanı** (`Infrastructure/Security/RequestBodyLimits.cs`): uç sınıfı başına tavan (`RequestBodyClass`; GET gövde taşımaz, genel form 64 KB, uzun metin 1 MB, görsel yükleme P-23 + 1 MB, parça 5 MB, bildirim 64 KB), sınıf uç meta verisi `RequestBodyClassAttribute`; aşan istek gövde okunmadan 413 alır. Uzunluğu bildirilmeyen gövde tavan + 1 bayta kadar okunur. Sayfa 413'ü yalın HTML, makine ucu 413'ü gövdesiz.
- **Genel hız sınırı** (`RequestRateLimiting`, `RateLimitScope`): sayfa ve form gönderimi sınırı zincirli; eksen `ClientAddress.AxisOf` (IPv6 /64); `/statik`, `/gorsel` dışarıda; parçalı yükleme yalnız sayfa sınırında; imzası doğru bildirim dışarıda, bozuk imza `CountSignatureFailure` ile form sayacına; 429 `Retry-After`'sız, sayfada nötr mesaj, makine ucunda gövdesiz.
- **`TimeDrivenSlidingWindowLimiter`**: çatının `SlidingWindowRateLimiter`'ı `TimeProvider` almıyor ve `TryReplenish` gerçek saate bağlı olduğu için sahte saatle çalışmıyor; sınırlayıcı kendi dilim sayacını tutar (altı dilim, 60 sn), `RateLimitPartition.Get` ile kurulur.
- **İstemci adresi ve ters vekil güveni** (`ClientAddress`, `ForwardedHeadersSetup`, `ReverseProxyTrust`): `RemoteIpAddress` tek yerde okunur; `ForwardedHeaders` ilk ara katman; iki iç ağ (IPv4 + IPv6), `ForwardLimit = 1` (D-112).
- **Deneme limitleri** (`AttemptCounter`, `AttemptSubject`, `AttemptLimits`): `RecordAsync` kendi kısa işleminde, `IsExceededAsync`, `ForgetAsync`, `MergeCartAsync`; eşik ve pencere `ProductConstants`'tan (P-31…P-37, P-46, P-49 sabitleri eklendi); `FormResults.AttemptLimitExceeded` (429, değerler korunur, mesaj düğmenin yanında, `Retry-After` yok).
- **Testler**: birim (hız sınırı, gövde tavanı, adres, ters vekil, tutarlılık, kaynak taraması + ihlal örnekleri), entegrasyon (`RequestLimitsTests` 24, `AttemptCounterTests` 14, `AttemptLimitPatternTests` 2).
- **Belgeler**: `09 §9.4` m.1, `§15.1` m.8, `§15.4` m.1/3/4 ve Test paragrafı; `DEFERRED_BACKLOG` (D-31 (d), D-64, D-103, D-108, D-112, D-135); `DEPLOY_RUNBOOK` D-31 satırı; `deploy/compose.yaml` yorumu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/` (`AttemptCounter`, `AttemptLimits`, `AttemptRecord`, `AttemptSubject`, `ClientAddress`, `ForwardedHeadersSetup`, `RequestBodyLimits`, `RequestRateLimiting`, `TimeDrivenSlidingWindowLimiter`), `Infrastructure/ProductConstants.cs`, `Infrastructure/Forms/FormResults.cs`, `Infrastructure/SignedEndpoints/*` (imza hatası sayımı), `Program.cs`.
- `tests/Shopfolio.UnitTests/{RateLimiting,Consistency,Architecture}`, `tests/Shopfolio.IntegrationTests/{Web,Security,Pages/Panel}`.
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/DEPLOY_RUNBOOK.md`, `deploy/compose.yaml`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Gövde tavanı uç sınıfı başına | ✓ | `RequestBodyLimitMiddlewareTests.TheCeilings_AreTheDocumentsNumbers`, `EveryClass_HasACeiling…`, `AGetOrHead_HasNoBody…`; `RequestLimitsTests.ABodyAtTheClassCeiling_ReachesTheEndpoint…` (genel, uzun metin, makine), `TheGeneralClass_RefusesWhatTheLongTextClassAccepts` |
| 2 | Tavanı aşan 413, form yeniden gösterilmez | ✓ | `RequestLimitsTests.TheRefusal_OfAPage_IsAPlainPageWithNoFormAndNoRetryAfter_AndNothingWasRead`, `TheRefusal_OfAMachineEndpoint_HasNoBody`, `ABodyWithNoDeclaredLength_IsMeasured…`; `RequestBodyLimitMiddlewareTests.APageRefusal_IsThePlainOperationPage_NoForm_NoStoredValues` |
| 3 | Kayan pencere 60 sn, 6 dilim; eksen IP, IPv6 /64 | ✓ | `TimeDrivenSlidingWindowLimiterTests` (dilim dilim, sahte saat), `RequestRateLimitingTests`, `RequestLimitsTests.TwoAddressesOfOneV6Prefix_ShareOneCounter`, `ClientAddressTests` |
| 4 | Statik/görsel muaf; bildirim imzası; parçalı yükleme | ✓ | `RequestLimitsTests.StaticFilesAndImages_AreOutsideTheLimit`, `ANotificationWithARightSignature_IsOutsideTheLimit_AndBadSignaturesCountTowardsTheFormLimit…`, `TheChunkedUpload_IsUnderThePageLimitOnly_NotTheFormLimit`, `PostsAreRefusedWith429AfterTheFormLimit_AGetStillPasses…` |
| 5 | Genel sınır 429, nötr mesaj, makine ucunda gövdesiz, `Retry-After` yok | ✓ | `RequestLimitsTests.APage_IsRefusedWith429AfterThePageLimit_NeutralMessageNoFormNoRetryAfter_AndTheNextWindowOpensIt`, `AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter` |
| 6 | Deneme limitleri 429, değerler, mesaj düğmenin yanında | ✓ | `AttemptLimitPatternTests.TheAttemptOverTheLimit_Answers429_KeepsTheValue_ShowsTheNeutralMessage_AndWritesNoRetryAfter`; `AttemptCounterTests` (14: L-1 5/15 dk, Forget, eşzamanlı, L-2, pencere sınırı, birleşme, normalleştirme) |
| 7 | L-7/L-8 formdan değil hesaptan; sayım T377'de | ✓ | `AttemptLimitsConsistencyTests.L7AndL8_AreNotCountedByTheAttemptRecords_TheTableRefusesThem`; `AttemptCounterTests.The_limits_counted_from_the_orders_are_refused` |
| 8 | Parametre değerleri `02 §11`'den; genel sınır kurulum ayarı | ✓ | `AttemptLimitsConsistencyTests.EveryRowOfTheDocument_IsInTheCodesTable…`, `TheWindowOfEveryLimit_IsTheWindowTheDisposalDeletesBy`; `RateLimitOptions.PageRequestsPerMinute` / `FormSubmissionsPerMinute` (`TunableDesignValues.cs`), `DEPLOY_RUNBOOK` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 her uç sınıfının tavanı yoklanır | ✓ | `RequestLimitsTests.ABodyAtTheClassCeiling…` (Theory: genel, uzun metin, makine), `RequestBodyLimitMiddlewareTests.ABodyAtTheCeiling_PassesWithItsDeclaredLength_ForEveryClass` / `…OneByteOverTheCeiling_IsRefusedWith413_ForEveryClass`, `TheImageUploadClass_IsTheImageCeilingPlusOneMegabyte` |
| KK2 64 KB üstü 413, form gösterilmez | ✓ | `RequestLimitsTests.TheRefusal_OfAPage_IsAPlainPageWithNoFormAndNoRetryAfter_AndNothingWasRead` |
| KK3 dilimler sahte saatle; /64; vekil dışı başlık adresi değiştirmez | ✓ | `TimeDrivenSlidingWindowLimiterTests.ASegmentsPermitsReturn_WhenThatSegmentLeavesTheWindow_NotBefore`, `TwoAddressesOfOneV6Prefix_ShareOneCounter`, `ForwardedHeadersTests.FromAnyOtherSource_TheForwardedHeadersAreIgnored…`, `RequestLimitsTests.TheForwardedAddressAndScheme_AreBelievedFromTheProxysNetworkOnly` |
| KK4 statik ve imzası doğru bildirim sınıra girmez; bozuk imza sayılır; parça form sınırına sayılmaz | ✓ | `RequestLimitsTests.StaticFilesAndImages_AreOutsideTheLimit`, `ANotificationWithARightSignature…`, `ABadSignature_IsAnsweredByTheEndpointEvenWhenTheLimitIsReached`, `TheChunkedUpload_IsUnderThePageLimitOnly…` |
| KK5 429 sayfası nötr, form yok, `Retry-After` yok | ✓ | `RequestLimitsTests.APage_IsRefusedWith429AfterThePageLimit…`, `AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter` |
| KK6 limit aşımında 429, değer korunur, mesaj düğmenin yanında | ✓ | `AttemptLimitPatternTests` (örnek sayfa `Pages/Panel/AttemptSample.cshtml`: özet düğmenin yanında) |
| KK7 misafirin yazdığı e-posta eksene girmez | ✓ | L-7/L-8 tabloda yok: `AttemptLimitsConsistencyTests.L7AndL8_AreNotCountedByTheAttemptRecords_TheTableRefusesThem`, `AttemptCounterTests.The_limits_counted_from_the_orders_are_refused` (sayım ve hesap anahtarı T377) |
| KK8 limit sayısı yalnız `ProductConstants`'ta; uçta sayı yok | ✓ | Çağrı imzası sayı almaz (`RecordAsync(AttemptLimit, AttemptSubject, ct)`); `AttemptLimits` tablosu yalnız `ProductConstants` okur; `AttemptLimitsConsistencyTests`; gövde tavanları `RequestBodyLimitMiddlewareTests.TheCeilings_AreTheDocumentsNumbers`; `AttemptSample` ve `RequestLimitsTests` sayı yazmaz (`AttemptLimits.Of(...).Threshold`) |
| `09 §25` ilgili satır (yeni giriş/deneme yüzeyi §15.4 kapsamında) | ✓ | `AttemptSample` deneme yüzeyi sayacı kullanır; kaynak taraması `RequestOriginSourceScanTests` |
| Güvenlik taraması | ✓ | Mini kontrol aşağıda; ham `RemoteIpAddress` / başlık güveni / `AttemptRecord` yazımı kaynak taramasıyla tek yere kilitli |

Sık FAIL nedenleri taraması: (1) istisna darlığı: `RequestOriginSourceScanTests` dosya başına bir kural için istisna, ihlal örneği `Misuse.cs` + `DisposalMisuse.cs` (çok satırlı, adlı argüman, nitelikli ad, `?.`, desen); (2) çağrı biçimleri: ek/adlı argüman ve çok satır ihlal örneğinde; (3) sayı/etiket: `T215`, `L-7` anan yerler (`11`, `DEPLOY_RUNBOOK`, `FormResults` yorumu, `compose.yaml`, backlog) tarandı ve güncellendi; (4) belgede olmayan tavan sabiti yok (D-108 tavan konmadı); (5) hedefi T215 olan D-103, D-108, D-112 ve D-31 (d) kapatıldı ya da gerekçeyle ileri atıldı.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 2349 geçti, 0 kalan | `dotnet test --project tests/Shopfolio.UnitTests` | total 2349, failed 0 |
| Integration | ✓ 1576 geçti, 0 kalan | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-namespace/--filter-class …` (parçalı) | Concurrency 26, Database 35, Disposal 18, FileSecrets 24, Files 33, Jobs 77, Logging 30, Model 893, Outages 8, Security 32, SignedEndpoints 11, Web 96 + 106 + 77 + 84 + RequestLimitsTests 24 + AttemptLimitPatternTests 2 |
| Contract | — | — | bu task'ta yok |
| Biçim / derleme | ✓ | `dotnet format --verify-no-changes`; `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (genel sınır ayarları T-önceki kurulum ayarıdır: `PageRequestsPerMinute`, `FormSubmissionsPerMinute`)
- **Paketleme / dağıtım:** `deploy/compose.yaml` yalnız yorum (edge ağının iki alt ağı artık `ReverseProxyTrustConsistencyTests`'le sınanır)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T215-hiz-siniri-deneme-govde`
- **Commit:** son baş (PR sayfasında)
- **PR:** *(açılınca)*
- **CI:** *(yapım işçisi izlemez)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `ForwardedHeadersOptions.KnownIPNetworks` ve `System.Net.IPNetwork` .NET 10'da | `ForwardedHeadersSetup.Configure` derlenir; `ForwardedHeadersTests` | ✓ doğrulandı |
| 2 | Çatının `SlidingWindowRateLimiter`'ı `TimeProvider` almaz ve `TryReplenish` gerçek saate bağlıdır | sahte saatle çalışmadığı denemeyle görüldü; yerine `TimeDrivenSlidingWindowLimiter` | ✓ doğrulandı (`09 §15.4` m.1 notu) |
| 3 | `AttemptAcquire` ile zincirden izin alıp bırakmak yalnız form sayacını artırır | `RequestLimitsTests.ANotificationWithARightSignature…` | ✓ doğrulandı |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `SignatureFailureLog` sayılı pencerelerinin bellek tavanı konmadı | D-108 (hedef F2 gate) | `09 §3.5`'te sabit yok; taşma hâli `05 §4.5.2`'den ayrılır, sahibin kararı |
| 2 | `LoginRecord.Create` hâlâ `normalizedEmail` alıyor | D-64 (LoginRecord yarısı → T341) | giriş kaydını ilk yazan task |
| 3 | 429 ve 413 sayfaları yalın HTML (çerçeve yok) | D-135 notu (T537 + T540) | çerçeve henüz yok |

Kapatılan: D-103, D-112; D-31 (d) uygulama yanı; D-64 `AttemptRecord` yarısı. Açık kalan hedef değişiklikleri yukarıda.

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: CI 42182118653 (T212 #407) success, Publish 42182118563 success, CI 42169337584 (T210 #406) success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Kararlar:**
  1. Genel sınırlayıcı çatının sınırlayıcısı değil, kendi dilim sayacıdır (`TimeDrivenSlidingWindowLimiter`; `RateLimitPartition.Get`); `09 §15.4` m.1 güncellendi.
  2. `TrustedNetwork` iki alt ağdır (IPv4 + IPv6, `TrustedNetworkV6`, liste `TrustedNetworks`); `09 §15.1` m.8 güncellendi.
  3. 429 ve 413 sayfaları yalın HTML; `09 §15.4` m.4 güncellendi, D-135'e not eklendi.
  4. Uzunluğu bildirilmeyen gövde tavan + 1 bayta kadar (en çok 5 MB) tamponlanır.
  5. Makine ucu ayrımı hız sınırında ve gövde ara katmanında yol tabanlıdır (`/sistem`, `/indir`, `/panel/yukleme`); `09 §8.1` m.3 "yoldan türetilmez" der, ama hız sınırı yönlendirmeden önce çalıştığı için uç meta verisi yoktur; `09 §15.4` m.4 yolu açıkça sayar. Gövde ara katmanı `MachineEndpointMetadata` VEYA yolu kullanır.
  6. `ProductConstants`'a P-31…P-37, P-46, P-49 eşik sabitleri eklendi (`02 §11.2` satırlarıyla `AttemptLimitsConsistencyTests` eşitler).
  7. Zincirde reddedilen POST sayfa iznini de tüketir (`CreateChained` geri vermez); test yorumunda belirtildi.
  8. `AttemptRecord` yazımı için D-64 uzlaştırması yol B: `09 §9.4` m.1 deneme kaydını `AttemptSubject` + `AttemptCounter`'a verir.
  9. D-108 tavanı konmadı (belgede yok); F2 gate'e ileri atıldı.
- **Mini güvenlik kontrolü:** sır yok; yetkilendirme etkisi yok (örnek sayfa panel kimliğiyle); girdi doğrulaması: gövde tavanı ve eksen metni sınırlı; yeni dış bağımlılık yok.
- **Önceki task kaydı:** T212 status Commit hücresi (`4694519`) ve rapor "Post-merge run'lar" satırı dolduruldu (skill adım 15); eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı.
