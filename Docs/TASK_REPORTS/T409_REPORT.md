# T409 — Google: giriş, kayıt ve yeniden doğrulama bağlayıcısı

**Faz:** F4 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS; kriter 8'in gerçek uygulama payı OWNER_INPUT — D-1029) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `Microsoft.IdentityModel.JsonWebTokens` ve `Microsoft.IdentityModel.Protocols.OpenIdConnect` 8.23.0 eklendi (MIT; nuspec okundu, 18 Eylül 2026): `Directory.Packages.props`, Web projesi, üç kilit dosyası.
- `Infrastructure/Integrations`: `GoogleEndpoints` (sabit adresler, kapsam, S256, `SendsMaxAge` kapalı), `GoogleAuthorizationRequest` (sekiz parametre; yeniden doğrulamada `claims` ile `id_token` altında `auth_time`; `prompt` yok), `GoogleIdentityMapping` (saf eşleme: kod değişimi yanıtı, kimlik yükü, doğrulama hatasının kısa kodu), `GoogleIdentityGateway` (üç ayrı çalıştırıcı çağrısı: kod değişimi, anahtar yükleme, doğrulama), `GoogleRegistration` (tek `ConfigurationManager`, `GoogleDocumentRetriever` her okumada 15 sn kendi sınırı), `IntegrationRegistration` `AddGoogleSignIn` çağırır.
- Sır bekçisi yanlış pozitifleri (önceki işçinin commit'ini engelleyen) bekçiye dokunmadan giderildi: `token`/`idToken` adlı yerel değişkenler `jwtText`/`identityJwt` oldu, belirteç türü adları tür takma adıyla (`VerifiedJwt`, `ValidationOutcome`, `ValidationRules`) yazıldı, doğrulama hatası eşlemesi `switch` yerine (tür, kod) tablosuna çevrildi (kod dizgeleri aynı).
- `GOOGLE.md` (9 bölüm); `08 §4.1` DV-25/26/27/28/31/37/43 "Kısmen gözlendi (2026-11-10, T409; çevrimdışı)"; `09` Kütüphaneler iki satırı (lisans, sürüm doğrulandı).
- Kayıt: `DEFERRED_BACKLOG.d/T409.md` (D-1029…D-1033), `DEFERRED_BACKLOG.md` D-109 hedefi T343;T515 (T409 sayfa yazmaz), `STATUS_CHANGELOG.d/T409.md`, `IMPLEMENTATION_STATUS` T409 ⏳.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/Google*.cs`, `IntegrationRegistration.cs`; `Directory.Packages.props`, `Shopfolio.Web.csproj`, üç `packages.lock.json`.
- Testler: `Integrations/GoogleIdentityGatewayTests.cs`, `GoogleAuthorizationRequestTests.cs`, `GoogleIdentityMappingTests.cs`, `GoogleVerificationRowsTests.cs`, `GoogleWire.cs`; `IntegrationTests/Web/GoogleRegistrationTests.cs`; `Architecture/LinkKeySourceScan.cs` (+ testi: PKCE özeti için tek dosyalık istisna), `Consistency/LibraryTableConsistency.cs` (iki paket "henüz eklenmedi" listesinden çıktı).
- Belgeler: `Docs/INTEGRATION_RUNBOOKS/GOOGLE.md`, `08`, `09`, backlog.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kod değişimi `POST`, `userinfo` çağrılmaz | ✓ | `GoogleIdentityGatewayTests.Exchange_IsAPost_ToTheFixedTokenAddress_WithTheSixFormFields_AndNoUserinfoCall`; `ComposeExchange_PutsTheSecretInTheBodyOnly` |
| 2 | İstemci kimliği ve sırrı ayar; biri eksikse çağrı yok | ✓ | `WithAClientSettingMissing_NoCallIsMade_AndTheResultIsAFailure` (Theory); Integration `GoogleRegistrationTests.GatewayWithNoGoogleApplication_MakesNoCall_AndAnswersWithAFailure` |
| 3 | Adresler sabit; `openid email profile`, `state`, `nonce`, S256; yeniden doğrulamada `claims` ile `auth_time`; `prompt` yok; `max_age` yalnız DV-26 doğruysa | ✓ | `GoogleAuthorizationRequestTests` (`TheAddress_…`, `EveryFlow_CarriesTheEightParameters_…`, `Reauthentication_AsksAuthTimeUnderIdToken_…`, `NoFlow_CarriesAForbiddenParameter`, `MaxAge_IsNotCarriedByTheDefaultRequest_…`, `CodeChallenge_IsTheS256OfTheVerifier_RfcVector`); `TheAddressesAreFixed_AndTheDocumentReaderRequiresHttps` |
| 4 | Yalnız `RS256`, imza, `iss`, `aud`, `exp`; `email` yoksa tutmaz; `nonce`, `auth_time` sonuçta | ✓ | `ValidationParameters_AcceptOnlyRs256_…`; `TokenWithADifferentAlgorithm_…`, `UnsignedToken_…`, `TokenSignedByAnotherKey…`, `TokenWithAnotherIssuer_…`, `TokenForAnotherAudience_…`, `ExpiredToken_…`, `TokenWithoutAnExpiry_…`, `TokenWithoutAnEmail_OrAnEmptyOne_…`; `ValidToken_GivesTheMappedFields_AndTheNonce_…`; `GoogleIdentityMappingTests.AuthTime_…` |
| 5 | Keşif belgesi ve anahtar kümesi önbellekli; bilinmeyen `kid` yenileme getirir, yine yoksa imza tutmaz | ✓ | `TheKeySet_IsCached_…`; `TokenWithAnUnknownKeyId_FetchesTheKeySetAgainInTheSameCall_AndHoldsOnTheFirstCall` (yeni kümede anahtar → ilk çağrı `Succeeded`, anahtar okuması 2, kod değişimi 1); `TokenWithAKeyIdThatIsStillUnknownAfterTheRefresh_Fails_AndFurtherCallsDoNotFetchAgain` (`Failed`, 3 çağrıda anahtar okuması 2); `TheHostConfiguration_TurnsTheLibrarysSameCallRefreshSwitchOn_…`, `TheBuildProps_DeclareTheSwitchAsARuntimeHostOption` (düzeltme 1) |
| 6 | Sonuç yalnız e-posta, doğrulama bayrağı, ad, `hd`, `auth_time`, `nonce`; `sub` alınmaz, kayda yazılmaz | ✓ | `GoogleIdentityMappingTests.Identity_CarriesOnlyTheSixFields_AndNoSubject` (tür özelliklerinde `Sub` yok); `AFailureLine_CarriesTheJobTheKindAndTheReason_AndNoSecretCodeVerifierOrToken` |
| 7 | Vazgeçme/doğrulama hatası `Failed`; bağlanamama, 15 sn aşımı, Google hata yanıtı `Unavailable`; yeniden deneme yok | ✓ | `ExchangeAnswerOfAnError_IsUnavailable_AfterOneCall_AndNoRetry`, `ExchangeThatCannotConnect_…`, `ExchangeThatHangs_EndsAtTheUserWaitingLimit_AsUnavailable_AndGoesOnce`, `KeySetThatHangs_…`, `DiscoveryDocumentThatCannotBeGot_IsUnavailable_AndTheSpentCodeIsNotSentAgain`, `ExchangeAnswerOutsideTheTable_IsAFailure_NotAnOutage`, `GoogleIdentityMappingTests.ExchangeAnswer_*`; vazgeçme dönüşünde kod yoktur, çağrı yapılmaz (sayfa/çekirdek işi, T515/T343) |
| 8 | `GOOGLE.md` aynı PR'da; DV-25/26/27/28/31/37/43 gerçek uygulamayla gözlenir | ~ | Runbook ✓ (`GoogleVerificationRowsTests.TheRunbook_HasTheTemplatesNineSections`); yedi satır çevrimdışı gözlemle tarihli (`TheSevenRows_CarryAVerificationDate_…`); gerçek uygulama (yayında+doğrulanmış) yok: OWNER_INPUT, D-1029. Task'ın yönergesi gereği durdurma sebebi değildir (T407 kriter 9 ile aynı) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 gövde ve `userinfo` yok | ✓ | kriter 1 testi (istek listesi tam eşitlik) |
| KK2 ayarsız çağrı yok | ✓ | kriter 2 testleri (Unit + Integration) |
| KK3 adresler, parametreler, `claims`, `max_age` | ✓ | kriter 3 testleri |
| KK4 yedi belirteç durumu | ✓ | kriter 4: algoritma, imza yok, imza yanlış, `iss`, `aud`, `exp`, `email` (+ `exp` yok, `nonce` yok) |
| KK5 önbellek, `kid` yenilemesi, elle yeniden deneme yok | ✓ | kriter 5 testleri (aynı çağrıda yenileme; kod değişimi çağrı başına 1; elle yeniden deneme yok) |
| KK6 `sub` yok, `nonce` var | ✓ | kriter 6 testleri |
| KK7 iki sonuç sınıfı, çağrı sayısı bir | ✓ | kriter 7 testleri; `WarmCache_MakesOneCall_TheExchange` |
| KK8 DV satırlarında tarih | ✓ | `GoogleVerificationRowsTests` |
| `09 §25` ilgili satırlar | ✓ | Kütüphaneler iki satırı; `LibraryTableConsistencyTests` geçer |
| Güvenlik taraması | ✓ | sır yalnız gövdede; kayıt satırında sır, kod, doğrulayıcı, belirteç, e-posta yok (`AFailureLine_…`); `IntegrationSourceScan`, `LinkKeySourceScan` geçer; bekçi atlanmadı |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | yeni istisna tek tam dosya yolu (`GoogleAuthorizationRequest.cs`) + ihlal örneği (komşu dosya `GoogleIdentityGateway.cs` aynı özet için bulgu verir: `Scan_TheGeneratorExceptionIsOneFile_…`); bekçi kuralı değişmedi |
| Sık FAIL 2: tarama çağrı biçimini kaçırıyor | ✓ | yeni tarama kuralı eklenmedi |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | — | şema değişikliği yok |
| Sık FAIL 4: aynı sayıyı anan yerler | ✓ | `LibraryTableConsistency.NotYetAdded` (iki paket çıktı), `09` tablosu, `LinkKeySourceScan` sayı testi, D-109 hedefi, 08'deki runbook bağlantısı SES örneğiyle aynı (değişmedi) |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ⚠ | `iss` biçimi `08` ile `09 §11.3` m.3 arasında çelişiyor; kod `08`'i izledi, `09` metni değişmedi → D-1030 |
| Backlog: hedefi T409 olan kalem | ✓ | D-109 hedefi T343;T515'e alındı (T409 sayfa yazmaz); D-556 zaten T515 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 6114/6114 (Google sınıfları 108); Google sınıfları ayrıca tekrar koşuldu, kararsızlık yok |
| Integration | ✓ (ilgili sınıflar) | `--filter-class "*Google*" --filter-class "*EmailCaptureRegistration*"` | 7/7 (yerel PostgreSQL 18, `track_commit_timestamp=on`) |
| Lint / derleme | ✓ | `dotnet format --verify-no-changes`; `dotnet build -warnaserror` | 0 hata, 0 uyarı |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 2, 2026-11-10); tur 1 ✗ FAIL (kriter 5 / KK5), düzeltme 1–2 ile giderildi |
| Bulgu sayısı | 0 (tur 2); tur 1: 1 (ORTA, S1) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (tur 2: K1 → D-1030, K2 → D-1070) |

**Tur 2 kanıtları (doğrulayıcı, birleşik ağaç `origin/main` + dal):** kriter 1–7 ve KK1–KK8 ✓ — kod okundu (`GoogleEndpoints`, `GoogleAuthorizationRequest`, `GoogleIdentityGateway`, `GoogleIdentityMapping`, `GoogleRegistration`, `GoogleIdentityResult`; `08 §2.3`, `09 §11.1`–§11.3 ile karşılaştırıldı); bilinmeyen `kid` aynı çağrıda yenilenir, ilk çağrı tutar (`TokenWithAnUnknownKeyId_FetchesTheKeySetAgainInTheSameCall_AndHoldsOnTheFirstCall`: anahtar okuması 2, kod değişimi 1), yeni kümede de yoksa `Failed` ve ikinci yenileme yok. Kriter 8 depo payı ✓ (`GOOGLE.md` 9 bölüm; DV-25/26/27/28/31/37/43 "Kısmen gözlendi (2026-11-10, T409; çevrimdışı)"); gerçek uygulama gözlemi OWNER_INPUT, D-1029 — T407 kriter 9 emsali. Yerel: restore ✓, build `-warnaserror` 0/0, `dotnet format --verify-no-changes` üç proje ✓, Unit 6329/6329 (Google sınıfları 110), betik testleri 4/4, Integration `GoogleRegistrationTests` 2/2 + `Shopfolio.IntegrationTests.Web` 600/600 (PG18, `track_commit_timestamp=on`). CI: PR #519 run 42844491708 tüm işler yeşil. 12 satırları (`VAL-B021`, `A091`–`A095`, `E026`–`E029`, `E031`, `E032`, `E038`, `E044`, `E073`, `E074`): kanıt ortamı gerçek/uç yanıtı (T410, T515) — `BEKLEMEDE` kalır. Uyuşmazlık: yapım raporu kriter 8'i `~` yazmış, doğrulayıcı depo payını ✓ saydı (gerçek gözlem D-1029'da); Unit sayısı farkı main birleştirmesinden.

**K2 (tur 2):** vazgeçme dönüşü (`error`, kod yok) bağlayıcıya ulaşmaz ve `IdentityCodeExchange` boş kodu reddetmez — boş kodla çağrılırsa `invalid_grant` → `Unavailable` olur; vazgeçmenin `Failed` sonucu dönüş sayfasının işidir → D-1070 (T515 kriter 9).

**Bulgu 1 (ORTA, S1) — kriter 5 / KK5, `08 §2.3`:** bilinmeyen `kid`'li belirteç, yenilenen kümede anahtar olsa da o çağrıda tutmuyor (`Failed`, `token:` önekli); yenileme arkada koşuyor, ancak sonraki çağrı yeni kümeyi görüyor. `08 §2.3` "küme yeniden çekilir, yine yoksa imza tutmaz" ve "en kötü durumda art arda üç çağrı" der; kriter 5 "yenileme getirir, yine yoksa imza tutmaz". D-1031'in "çatı davranışı, kaçınılmaz" gerekçesi tutmuyor: 8.23.0 çatının kendi anahtarı `Switch.Microsoft.IdentityModel.UpdateConfigAsBlocking` (`AppContext`) açıkken aynı senaryoda ilk çağrı `Succeeded` döndü — anahtar okuması 2, kod değişimi 1 (doğrulayıcının geçici sınaması, `TokenWithAnUnknownKeyId_…` kurgusu; dosya silindi). Elle yeniden deneme gerekmiyor. Düzeltme: çatının eşzamanlı yenilemesi (ya da `08`/kriter 5'i değiştiren karar kaydı), test ilk çağrının `Succeeded` olduğunu sınar, DV-31 hücresi ve D-1031 buna göre güncellenir.

**K1:** `09 §11.3` m.3 (`ValidIssuers` iki biçim) ile `08 §2.3`/DV-31 (yalnız keşif belgesinin `issuer`'ı) çelişkisi — kod `08`'i izliyor; D-1030'a forward edilmiş, yeterli.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (`Google:ClientId` ve `google_client_secret` T389/T226'dan beri tanımlı)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Var — `Microsoft.IdentityModel.JsonWebTokens`, `Microsoft.IdentityModel.Protocols.OpenIdConnect` 8.23.0 (Microsoft, MIT, nuspec okundu; `09` Kütüphaneler tablosundaki seçili paketler)

## Commit & PR

- **Branch:** `task/T409-google-baglayici`
- **Commit:** son commit PR gövdesinde (head SHA dönüşte)
- **PR:** dönüşte
- **CI:** işçi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Keşif belgesi `issuer`, uç adresler, `jwks_uri`, `code_challenge_methods_supported`, imza algoritması | `curl https://accounts.google.com/.well-known/openid-configuration`, 2026-11-10: issuer `https://accounts.google.com`; authorization/token endpoint sabitlerle aynı; `jwks_uri` `https://www.googleapis.com/oauth2/v3/certs`; `plain`,`S256`; `RS256` | ✓ (DV-27, DV-31 çevrimdışı payı) |
| 2 | `claims_supported` `auth_time` içermez (alan koşullu verilir) | aynı belge | ✓ (DV-25 çevrimdışı payı) |
| 3 | Çatının bilinmeyen `kid` davranışı (8.23.0) | sahte uçla gözlem: `UpdateConfigAsBlocking` açıkken ilk çağrı tutar (anahtar okuması 2, kod değişimi 1); çatı anahtarı ilk dokunuşta bir kez okur (kayıtta `SetSwitch` geç kalıyordu — gözlendi) | ✓ (DV-31), D-1031 ✅ |
| 4 | Çatı keşif/anahtar okumasında iptal belirtecini geçirmez | gözlem; kendi 15 sn sınırı testli | ✓, D-1033 |
| 5 | Yayında+doğrulanmış Google uygulaması: `auth_time`, `max_age`, 303 zinciri, test kullanıcısız giriş, `hd` | yok | ? Doğrulanamadı — OWNER_INPUT, D-1029 |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Yedi DV satırının gerçek uygulama gözlemi | D-1029 | Google uygulaması yok (OWNER_INPUT) |
| 2 | `iss` biçimi `08` ↔ `09` | D-1030 | belge kuralı değişimi karar kaydı ister |
| 3 | Bilinmeyen `kid`'de ilk çağrı başarısız | D-1031 | ✅ düzeltme 1'de çözüldü (çatı anahtarı) |
| 4 | `TimeProvider` yok | D-1032 | kütüphane sınırı |
| 5 | İptal belirteci yok sayılıyor | D-1033 | kütüphane sınırı |
| 6 | Sayfalar (giriş, kayıt, yeniden doğrulama) | D-109 → T515 | T409 yalnız bağlayıcı |

## Notlar

- **Working tree (Adım -1):** temiz. Önceki işçinin işi `staged.patch` ile uygulandı (commit'siz kalmıştı).
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3` — 42804053557 success, 42804053578 success, 42796731944 success.
- **⚠ kararları:** (a) `iss`: kod `08`'i izledi (`ValidIssuer` boş, yayıncı keşif belgesinden), `09` değişmedi, D-1030. (b) `TimeProvider` yok: `exp` sınaması gerçek saatle, süre sınırları göreli, D-1032. (c) `max_age`: `SendsMaxAge` kapalı (DV-26 gözlenmedi). (d) Kütüphane iptali yok sayıyor: `GoogleDocumentRetriever` 15 sn kendi sınırı, D-1033. (e) PKCE S256 özeti `GoogleAuthorizationRequest.cs`'te: `LinkKeySourceScan.GeneratorExceptions` listesine tek dosya, nedeniyle eklendi (bekçi/`pre-commit` kuralı değişmedi). (f) D-109 hedefi T343;T515'e alındı.
- **Repo memory (bitiş kapısı 9):** güncellenmedi — kural gereği MEMORY "Güncel Durum", STATUS "Son güncelleme", backlog sayacı bu task'ta değişmez.
- **Docker:** `t409-pg` (port 55380) kaldırıldı; `bin`/`obj` temizlendi.

## Düzeltme 1 (2026-11-10)

**Bulgu 1 (ORTA, S1, kriter 5 / KK5) giderildi.** Sahip yerine karar (yardımcı): belgeye uyuldu — çatının `Switch.Microsoft.IdentityModel.UpdateConfigAsBlocking` anahtarı açıldı. İlk deneme (kayıtta `AppContext.SetSwitch`) yetmedi: çatı anahtarı türlerine ilk dokunuşta bir kez okuyor, test düzeneği kayıttan önce belirteç ürettiği için anahtar geç kalıyordu (gözlendi). Bu yüzden anahtar `Directory.Build.props` içinde `RuntimeHostConfigurationOption` olarak süreç başından açık (web sunucusu ve bütün test sunucuları).

- Kod: `Directory.Build.props` (yalnız bu); testler: `TokenWithAnUnknownKeyId_FetchesTheKeySetAgainInTheSameCall_AndHoldsOnTheFirstCall` (yeni), `TokenWithAKeyIdThatIsStillUnknownAfterTheRefresh_…` (bekleme yardımcısı kalktı), `TheHostConfiguration_TurnsTheLibrarysSameCallRefreshSwitchOn_FromTheStartOfTheProcess`, `TheBuildProps_DeclareTheSwitchAsARuntimeHostOption` (yeni).
- Belgeler: `09 §11.3` m.2 notu (kural metni değil, çatı ayarının notu: "varsayılanlarına dayanılır" cümlesi bilinmeyen `kid` için daraltıldı; karar kaydı satırı düzeltme 2'de eklendi: K-1606, `Docs/PRODUCT_DISCOVERY_STATUS.md`), `08` DV-31 hücresi, `GOOGLE.md` sorun giderme satırı, D-1031 ✅.
- Öz denetim: yukarıdaki kriter ve KK5 satırları yeniden denetlendi; öteki kriter ve KK'lerin kodu/testi değişmedi, testleri yeniden koşuldu (aşağı).

| Tür (düzeltme 1) | Sonuç | Çıktı özeti |
|---|---|---|
| Restore, build `-warnaserror`, `dotnet format --verify-no-changes` | ✓ | 0 uyarı, 0 hata, format temiz |
| Unit (Architecture, Consistency dahil) | ✓ | 6216/6216 (Google sınıfları 110) |
| Integration TAM (PG18, `track_commit_timestamp=on`) | ✓ | Web ad alanı 599/599; geri kalan 2894/2894 |

## Düzeltme 2 (2026-11-10)

- main birleştirildi (T408 girdi); `Docs/IMPLEMENTATION_STATUS.md` satır çakışması: iki taraf korundu (T408 main sürümü, T409 dalın satırı). Kod çakışması yok, davranış değişmedi.
- Karar kaydı: K-1606 eklendi (`Docs/PRODUCT_DISCOVERY_STATUS.md`); `09 §11.3` m.2 sonuna `(K-1606)` atfı; düzeltme 1 notundaki "karar kayıtları repoda yok" ⚠ maddesi düzeltildi.
- Öz denetim: kabul kriterleri ve KK1-KK8 tablosu yerinde; ✓ + kanıt tablosunun atıfları birleşik dalda doğrulandı.
