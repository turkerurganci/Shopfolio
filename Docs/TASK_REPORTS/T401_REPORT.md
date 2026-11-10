# T401 — PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve

**Faz:** F4 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; kriter 9'un test modu gözlem payı OWNER_INPUT — D-1034) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `PayTrPaymentGateway.StartAsync`: jeton isteği `PayTrStartForm.TokenAddress` (`/odeme/api/get-token`) adresine tek POST, form kodlu, sunucudan sunucuya; başarılı yanıt `PayTrStartForm.FrameAddressRoot` + jetonla çerçeve adresini verir; jeton saklanmaz ve kayda yazılmaz.
- `PaymentProviderKeys`: `merchant_id`, `merchant_key`, `merchant_salt` tek ayardır; sır dosyası üç `name=value` satırı; biri eksikse kart hattı kapalı (`ConditionalSettings`) ve çağrı yapılmaz.
- `PayTrStartForm.Build`: on bir girdili `paytr_token` (`PayTrSignature` → `Infrastructure/Security/MessageSignature.HmacSha256Base64`), `payment_amount` kuruş (`PaymentAmounts.ToKurus`), `user_basket` tek kalem Base64, `merchant_oid` tiresiz (`OrderNumber.WithoutSeparator`, geri eşleme `CardPaymentReference`), yedek e-posta/telefon, ad 60 / adres 400 kesme, `user_ip`, `timeout_limit`, `no_installment`/`max_installment` = 0, dönüş adresleri `/odeme/donus/{erişim anahtarı}`.
- `Docs/INTEGRATION_RUNBOOKS/PAYTR.md` açıldı; `DEPLOY_RUNBOOK.md` §B ve §C'ye sır dosyası biçimi.
- D-105: `IntegrationSourceScan` kural 1 deseni genişletildi/daraltıldı, `.razor` ve `.js` taranır.
- `origin/main` (`8e1e6215`, T411) birleştirildi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/` (`PayTrPaymentGateway`, `PayTrStartForm`, `PayTrSignature`, `PayTrRegistration`, `PaymentProviderKeys`, `PaymentAmounts`, `IntegrationRegistration`)
- `src/Shopfolio.Web/Infrastructure/Security/MessageSignature.cs` (yeni), `OrderNumber.cs`; `Infrastructure/InstallationSettings/ConditionalSettings.cs`; `Modules/Payment/Contracts/CardPaymentReference.cs`
- Testler: `UnitTests/Integrations/PayTrPaymentGatewayTests.cs`, `UnitTests/Architecture/IntegrationSourceScan*.cs` (+ `ViolationSamples/Integrations/ProviderLeak.razor`, `provider-leak.js`), `IntegrationTests/Payment/PaymentStartTests.cs`, `StartupSettingsCheckTests`, `AccountAndStatusCommandTests`, `SettingsAuditTests`, `PaymentMethodTests`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Başlatma `POST /odeme/api` altındaki uçla, sunucudan sunucuya, form kodlu | ✓ | `Start_PostsOnceToTheTokenAddress_FormEncoded_ServerToServer_KK1`, `Start_ThatHangs_EndsAtTheUserWaitingLimit_WithOneRequest_KK1` |
| 2 | Üç anahtar tek ayar, sır dosyasında; eksikse kart görünmez, çağrı yok | ✓ | `Start_WithoutTheWholeSetting_MakesNoCall_KK2`, `CardLine_WithoutTheWholeSetting_IsClosed_KK2`, `Keys_ThreeLinesInAnyOrder_…_KK2` |
| 3 | `paytr_token` on bir girdinin HMAC-SHA256 Base64'ü; key/salt istekle gitmez | ✓ | `Token_MatchesTheKnownExample_AndTheKeyAndTheSaltDoNotGoWithTheRequest_KK3`, `Token_IsTheHmacOfTheElevenInputsInTheirOrder_KK3` |
| 4 | Tutar `PaymentAmounts`'tan; kuruş tam sayı; sepet tek kalem Base64 | ✓ | `Amount_IsWholeKurus_AndTheBasketIsOneItemInBase64_WhateverTheCulture_KK4`, `ToPlainText_…_KK4` (2 test) |
| 5 | `merchant_oid` tiresiz; geri eşleme aynı kuralla | ✓ | `OrderReference_…_KK5` (3 test) |
| 6 | Yedek e-posta/telefon; ad 60, adres 400 kesme | ✓ | `Email_…_KK6`, `Phone_…_KK6`, `NameAndAddress_…_KK6`, `Cut_…_KK6` (2 test) |
| 7 | `user_ip` istek anındaki adres; `timeout_limit` yukarı yuvarlanmış kalan dakika, en az 1 | ✓ | `ClientAddress_…_KK7` (2 test), `TimeoutMinutes_…_KK7`, `TimeoutLimit_IsReadOnTheClockAtTheRequest_KK7` |
| 8 | Yanıt çerçeve adresini verir, jeton saklanmaz; dönüş `/odeme/donus/{erişim anahtarı}` | ✓ | `Answer_GivesTheFrameAddress_BothReturnAddressesAreTheAccessKeyPath_…_KK8`, Integration `Start_GivesTheFrameAddress_AndTheTokenIsInNoLogLineAndNoRow_KK8` |
| 9 | `PAYTR.md` aynı PR'da; DV-11/14/38/39 test modunda gözlenir; geliştirme ortamı DV-41'e göre | ✓ (gözlem OWNER_INPUT) | `Docs/INTEGRATION_RUNBOOKS/PAYTR.md` §6.1: satırlar tarih 2026-11-10 ve kanıt yeriyle "Bekliyor — OWNER_INPUT"; gerçek test mağazası ve anahtar olmadan gözlem yapılamaz. Doğrulayıcı: depo payı ✓; gözlem payının izi D-1034, `VAL-E012`/`E015`/`E039`/`E040`/`E042` `BEKLEMEDE` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 sahte işleyiciyle form kodlu | ✓ | `PayTrPaymentGatewayTests.Start_PostsOnceToTheTokenAddress_FormEncoded_ServerToServer_KK1`; `Start_TestMode_IsZeroOnlyOnTheLiveInstallation_KK1` |
| KK2 üç ayardan biri eksikse çağrı yok | ✓ | `Start_WithoutTheWholeSetting_MakesNoCall_KK2` (üç eksik kolu), `CardLine_WithoutTheWholeSetting_IsClosed_KK2` |
| KK3 özet bilinen örnekle eşleşir | ✓ | `Token_MatchesTheKnownExample_…_KK3` (openssl ile üretilmiş örnek) |
| KK4 34,56 → 3456; kültür etkisiz | ✓ | `Amount_IsWholeKurus_…_WhateverTheCulture_KK4` (tr-TR ve en-US) |
| KK5 numara tiresiz, geri eşleme tam | ✓ | `OrderReference_MapsBackFully_ForEveryNewNumber_KK5`, `OrderReference_ThatIsNotAYearAndABlock_MapsToNothing_KK5` |
| KK6 yedek değer ve kesme sınırları | ✓ | `Email_OutsideTheLimit_…_KK6`, `Phone_…_KK6`, `NameAndAddress_AreCutAtTheirLimits_…_KK6`, `Cut_DoesNotSplitASurrogatePair_KK6` |
| KK7 IP biçimi ve dakika yuvarlaması | ✓ | `ClientAddress_GoesAsItIs_MappedIPv4TurnedBack_NoZone_KK7`, `ClientAddress_ThatIsNoAddress_MakesNoCall_KK7`, `TimeoutMinutes_IsTheTimeLeftRoundedUp_AtLeastOne_KK7` |
| KK8 jeton kayıtta ve veritabanında yok; çerçeve adresi; iki dönüş adresi | ✓ | Unit `Answer_…_KK8` (2 test); Integration `PaymentStartTests.Start_GivesTheFrameAddress_AndTheTokenIsInNoLogLineAndNoRow_KK8` (pozitif kontrol: göç kimliği bulunur), `Start_WithAFailedAnswer_WritesOnlyTheReasonCode_…_KK8` |
| KK9 DV satırlarında tarih ve kanıt yeri | ✓ | `PAYTR.md` §6.1 tablosu (DV-11, 14, 38, 39, 41) |
| 09 §25: tek işlev (tutar `PaymentAmounts`, numara `OrderNumber`, imza tek fonksiyon) | ✓ | `PayTrStartForm.Build`; `MessageSignature` |
| 09 §25: sabit belgede (§3.5) / sınır testleri | ✓ | 08 §2.1.4 sınırları `PayTrStartForm` sabitleri; testler sabitten türetir |
| 09 §15.1 m.1: özet işlevi yalnız `Infrastructure/Security/`'de | ✓ | HMAC `Infrastructure/Security/MessageSignature.cs`'e taşındı; `LinkKeySourceScan` istisna listesi main ile aynı (yeni istisna yok) |
| 08 §3.1 kural 1–2: sağlayıcı adı katman dışında yok, yeniden deneme yok | ✓ | `IntegrationSourceScanTests.ApplicationSourceAndFakes_…` (artık `.razor`, `.js` dahil) |
| Backlog: hedefi T401 olan kalemler | ✓ | D-105 ✅, D-786 ✅, D-840 T401 payı kapandı → T510 |
| Güvenlik taraması | ✓ | Sır yalnız sır dosyasından; key/salt istekte yok (KK3 testi); jeton kayıt/DB'de yok (KK8); `failed` yanıtında sağlayıcının `reason` metni yazılmaz; IP/kişisel alanlar yalnız istek gövdesinde; istek tek, yeniden deneme yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --filter-not-trait "Consistency=true"` (Architecture ve geri kalanı ayrı) | 1350 + 4331 = 5681/5681; Consistency 292/292 |
| Integration | ✓ | parçalı `--filter-namespace`/`--filter-class` (Postgres 18, `track_commit_timestamp=on`) | Web 595, Commands 70, Payment+FileSecrets+Security+SignedEndpoints 85, Administration+Catalog+Concurrency 177, CorporateContent+Files+Outages 340, Database+Disposal+Pages 97, Jobs+Logging 107, Ordering A–I 200 (main birleşmesinden önce); main birleşmesinden sonra Ordering O 165, L–P 74, R–W 175, Model 1404, Payment+Logging 46 |
| Contract | — | — | Bu task'ta sözleşme testi yok |

Derleme: `dotnet build -c Release -warnaserror` Web, Unit, Integration temiz; `dotnet format --verify-no-changes` değişen dosyalarda temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-10; head `d7062c8`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-1034 (DV-11/14/38/39/41 test modu gözlemi backlog izsizdi; `08 §4.1` satırları hâlâ "ilk bağlanan task" der); K2 → D-1035 (`failed` yanıtında `reason` metni kayda yazılmıyor ve testle sabitlendi; `08 §2.1.8` ister, T404 kriter 1 çevirir) |

**Tur 1 kanıtları (doğrulayıcı):** kriter 1–8 ve KK1–KK8 ✓ (kod okundu: `PayTrPaymentGateway`, `PayTrStartForm`, `PaymentProviderKeys`, `ConditionalSettings`, `MessageSignature`, `OrderNumber`, `PaymentAmounts.ToPlainText`; testler kendi koşumumda geçti). KK3: `paytr_token` bilinen örneği (`DhiH4xqQ…/g=`) ve sepet Base64'ü `openssl dgst -sha256 -hmac` ve `base64 -d` ile .NET dışında yeniden hesaplandı, eşleşti. Kriter 2'nin "kart görünmez" payı `CardPaymentCondition`'ın okuduğu `ConditionalLines` hattından gelir (`PaymentMethodTests.CardCondition_…_KK4`). Kriter 9 ✓ depo payı: `PAYTR.md` şablonun dokuz bölümüyle açıldı, §6.1 DV-11/14/38/39/41 satırları tarih ve kanıt yeriyle "Bekliyor — OWNER_INPUT"; gerçek test mağazası yok — gözlem payı iz D-1034 (doğrulayıcı forward etti), `VAL-E012`/`E015`/`E039`/`E040`/`E042` `BEKLEMEDE` kalır. KP-15 satırları (`VAL-B008`…`B012`, `C001`, `C002`, `A053`, `D003`, `D004`) uç yanıtı, tarayıcı gözlemi ya da bildirim ister (T402, T510, T509); T401 kanıtıyla tamamlanmaz, `Durum` değişmedi. Dal CI 42806712266 success (Lint, Build, Unit, Integration, Contract, Migration, Paketleme); ana dal son 3 run (42804053557, 42804053578, 42796731944) success. Yerel: `dotnet build -warnaserror` 0 uyarı, `dotnet format --verify-no-changes` üç projede temiz, Unit 5814/5814, Consistency 292/292, betik testleri 4/4, Integration `PaymentStartTests` + `StartupSettingsCheckTests` + `AccountAndStatusCommandTests` 63/63 (Postgres 18, `track_commit_timestamp=on`). `12_matris_denetle.py --kapsam 1-77` hata 0.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — PayTR sır dosyası (üç `name=value` satırı; `PAYTR.md` §2, `DEPLOY_RUNBOOK.md` §B/§C)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T401-paytr-baslatma`
- **Commit:** `fc3257c2` — `T401: PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve` (+ rapor commit'i)
- **PR:** #516
- **CI:** dal CI'ı doğrulamada okunur
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Jeton isteğinin alanları, adresi ve imza sırası 08 §2.1, §2.1.4'teki gibidir | 08 §2.1.4 tablosu; gerçek test modu gözlemi DV-11/14/38/39 (OWNER_INPUT) | Bekliyor — gerçek test mağazası gerekli |

## ⚠ Kararlar (öneriyle verildi)

- Sır dosyası biçimi (üç `name=value` satırı) belgede yazılı değildi; runbook'a ve `DEPLOY_RUNBOOK`'a yazıldı.
- `test_mode` ortamdan (canlı = 0), T311 anlamıyla aynı.
- HMAC, kaynak taramasına istisna eklemek yerine `Infrastructure/Security/MessageSignature`'a kondu (09 §15.1 m.1 kapalı listesi değişmez).
- `failed` yanıtında sağlayıcının `reason` metni kayda yazılmaz, yalnız kod; ayrım T404'te (08 §2.1.8).
- D-840'ın kalan payı (çağıran servis, UC-29) T510'a ileri atıldı.
- D-105: çıplak küçük harfli `ses` (yerel ad, Türkçe "ses") bulgu değil; `wwwroot/lib/` üçüncü taraf kopyası taranmaz.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `StartAsync`'i çağıran ödeme sayfası (UC-29) ve uçtan uca akışta başlatma adımı | D-840 (→ T510) | Sayfa T510'un kapsamı (kriter 8) |
| 2 | DV-11/14/38/39 test modu gözlemi, DV-41 yanıtı | D-1034 (doğrulama tur 1 K1; OWNER_INPUT, `PAYTR.md` §6.1) | Gerçek PayTR test mağazası ve anahtarı gerekiyor |
| 3 | `failed` yanıtında sağlayıcının `reason` metninin kayda yazılması | D-1035 (doğrulama tur 1 K2; → T404) | `08 §2.1.8` ayrımı T404'ün kriter 1'i |
