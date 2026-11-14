# T510 — Sipariş teyit ve bekleme ekranı, ödeme sağlayıcısına geçiş ve dönüş (E-14)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; kriter 10'un DV-5 gözlem payı OWNER_INPUT — D-1466) | **Tarih:** 2026-11-14

---

## Yapılan İşler

- **Teyit ekranı (UC-28):** `{kök}/teyit` üye kökünde (`/siparis/{no}/teyit`) ve erişim kökünde (`/siparis/erisim/{anahtar}/teyit`); `ConfirmationPageModel` (`[AllowPaymentFrame]`, iki alt sınıf `[NoStore]`). Kartta sipariş numarası, bekleme cümlesi ve "Ödeme sayfasını aç" formu; havalede siparişe donmuş IBAN, toplam, son ödeme günü ve siparişe ulaşma yolu. Kendiliğinden yönlendirmez, `noindex`. Alındı + Bekliyor değilse 303 (üye sipariş sayfası, misafir sipariş takibi); kök tutmuyorsa 404.
- **Ödeme sayfası (UC-29):** `OnPostStartPaymentAsync` siparişi yeniden okur, `Payment` sözleşmesi `IPaymentStart` (`PaymentStartFace` → `PaymentStarts`) ile işlem dışında tek jeton ister; `OpenPaymentForm`'da çerçeveli sayfa 200 (adres yalnız yanıtta, saklanmaz), `CancelAsUnavailable`'da sayfa kendi işleminde `IOrderConfirmationScreen.CancelUnopenedAsync` ile iptal eder (L-7 dışı sebep `PaymentPageNotOpened`, sepet işareti) ve 303 `/sepet`, `StayOnConfirmation`'da (ikinci jeton reddi) 303 teyit. Tutar sunucudaki sipariş kaydından (`ConfirmationScreenFacts.Total`) gelir; sayfa tutar hesaplamaz.
- **Çerçeve izni:** `ContentSecurityPolicy` + `SecurityHeadersMiddleware` — `frame-src 'self' https://www.paytr.com` yalnız `[AllowPaymentFrame]` sayfası ve `PaymentFrameResponse.Mark` işaretli yanıtta (çift kapı); `form-action` değişmez. `PaymentFrameSource.Holds` çerçeve adresini sağlayıcının form köküne bağlar.
- **Dönüş geçiş sayfası (UC-30):** `GET /odeme/donus/{anahtar}` (`Pages/Payment/Return`) başlıksız, altlıksız tek cümle + `target="_top"` bağlantısı; `return.js` üst pencereyi `/siparis/erisim/{anahtar}`'a götürür; durum yazmaz, çerez okumaz; `frame-ancestors 'self'` yalnız bu yanıtta. Anahtar biçimi erişim kökünün kalıbı; biçim dışı 404.
- **Kilit sırası düzeltmesi (KK5):** iptal önce sepeti kilitler (`Carts.LockCartAsync`, `MarkPaymentUnavailableLockedAsync`), sonra siparişi (`05 §7.3.5` `LockSequence`).
- **Bu tur (devam 2):** `MainFlowTests` 6. adımına ödeme başlatma (D-840 ✅); `VAL-E089`, `E094`, `E049`, `E050` testlerle güçlendirildi; D-1080 ve D-981 satırlarına T510 payı notu; STATUS T509 Commit hücresi `edf4eb2a`.

## Etkilenen Modüller / Dosyalar

- **Ordering:** `OrderConfirmationScreens.cs`, `Contracts/ConfirmationScreenContracts.cs`, `Contracts/UnopenedPaymentCancellation.cs`, `Carts.cs`, `OrderingModule.cs`
- **Payment:** `PaymentStartFace.cs`, `Contracts/IPaymentStart.cs`, `Contracts/PaymentStartDecision.cs`, `PaymentModule.cs`
- **Infrastructure:** `Integrations/PaymentFrameSource.cs`, `PayTrStartForm.cs`, `PayTrRegistration.cs`, `Security/ContentSecurityPolicy.cs`, `Security/SecurityHeadersMiddleware.cs`
- **Pages:** `Pages/Orders/ConfirmationScreenModels.cs`, `MemberConfirmation.cshtml`, `AccessConfirmation.cshtml`, `_ConfirmationScreen.cshtml`, `_PaymentFrame.cshtml`; `Pages/Payment/Return.cshtml(.cs)`; `wwwroot/js/pages/payment/return.js`; `wwwroot/css/storefront.css`
- **Testler:** Integration `Web/CheckoutPageTests.Confirmation.cs`, `Ordering/MainFlowTests.cs`, `Payment/PaymentNotificationTests.cs`; Unit `Payment/PaymentPageTests.cs`, `Architecture/ThirdPartyContentScan(Tests).cs`, `Architecture/OperationHandlerMarkTests.cs`, `Support/ApplicationEnums.cs`
- **Belgeler:** `08 §4.1` DV-5 Durum hücresi; `INTEGRATION_RUNBOOKS/PAYTR.md` §6.1; `DEFERRED_BACKLOG.md` (D-840, D-1077 kapandı; D-1080, D-981 T510 payı); `DEFERRED_BACKLOG.d/T510.md` (D-1466, D-1467); `DEFERRED_BACKLOG.d/T537.md` (D-1127 kapandı)

## Kabul Kriterleri Kontrolü

Testler: **C** = `CheckoutPageTests` (Integration, `Web/CheckoutPageTests.Confirmation.cs`), **U** = `PaymentPageTests` (Unit).

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET {kök}/teyit` numara; kartta geçiş formu, havalede donmuş IBAN, toplam, son ödeme günü, ulaşma yolu; kendiliğinden yönlendirmez | ✓ | C `TheCardScreen_ShowsTheNumberAndTheButton_..._KK1_KK3_KK4` (numara, form, IBAN yok, `refresh` yok, `noindex`); C `TheTransferScreen_ShowsTheFrozenIbanTotalLastDayAndTheWay_..._KK1_KK6` |
| 2 | Alındı + Bekliyor değilse 303 (üye sipariş sayfası, misafir takip); kök tutmuyorsa 404 | ✓ | C `TheMembersScreen_LinksTheOrderPage_GoesThereOnceTheOrderMovedOn_AndAWrongRootIs404_KK2`; misafir 303 takip: C `AnUnreachableProvider_..._KK5` sonu, C `TheSecondTokenRefusal_..._KK6` (Ödendi) |
| 3 | `StartPayment` 200, çerçeve; jeton saklanmaz, her açılış yeni jeton | ✓ | C `..._KK1_KK3_KK4` (iki açılış iki adres, iki `StartAsync` çağrısı, `orders` satırında adres yok); U `TheStartsFace_GivesEachClassItsDecision_..._KK3_KK5_KK6`, U `TheStartsValues_ComeFromTheFrozenOrder_..._KK3` |
| 4 | Çerçeve izni yalnız bu sayfada, sağlayıcı + `'self'`; `form-action` sağlayıcısız | ✓ | C `..._KK1_KK3_KK4` (ödeme sayfası `frame-src 'self' https://www.paytr.com`, GET ve sepet `frame-src 'none'`, `form-action` sağlayıcısız); U `ThePolicy_OpensTheFrameOnlyForTheSiteAndTheProvider_..._KK4_KK8`, U `TheFrameSource_IsTheProvidersOrigin_..._KK4`; `ThirdPartyContentScanTests.PaymentFramePages_IsClosedToThePaymentPagesFrameView_..._KK4` |
| 5 | Bağlanılamazsa iptal, 303 `/sepet`, sepet bir kez söyler; L-7'ye sayılmaz | ✓ | C `AnUnreachableProvider_CancelsTheOrderOutsideL7_MarksTheCart_AndTheCartTellsItOnce_KK5` (Cancelled + Failed, O2 + S3 sistem kaydı `PaymentPageNotOpened`, sebep `UnpaidCancellationLimit.CountedReasons` dışında, ayırma serbest, mesaj bir kez, sepet kalemi durur, kart iki dakika kullanılamaz) |
| 6 | Alındı + Bekliyor değilse k2 yönlendirmesi; havalede ve ikinci jeton reddinde iptalsiz teyit | ✓ | C `TheSecondTokenRefusal_ReopensTheScreenWithoutACancellation_AndAPaidOrderGoesToTheOrderTracking_KK6`; C `TheTransferScreen_..._AndItsStartReopensIt_KK1_KK6` |
| 7 | `GET /odeme/donus/{anahtar}` başlıksız, altlıksız tek cümle; durum yazmaz, çereze dayanmaz | ✓ | C `TheReturn_IsAOneSentenceHandOverWithoutTheFrame_WritesNothing_..._KK7_KK8_KK9` (çerezsiz istemci, sipariş satırı ve kayıt sayısı aynı, ağ geçidi çağrısı yok, POST kolu yok); U `TheReturnPage_IsTheLayersReturnRoot_..._KK7_KK9` |
| 8 | Üst pencereyi `/siparis/erisim/{anahtar}`'a götürür, betiksizde `target="_top"`; `frame-ancestors 'self'` yalnız bu yanıtta | ✓ | C `..._KK7_KK8_KK9` (bağlantı `target="_top"` + `data-js`, modül betiği, dönüşte `frame-ancestors 'self'`, sipariş sayfasında `'none'`); U `ThePolicy_..._KK4_KK8` |
| 9 | Biçim dışı anahtar 404; geçersiz anahtarın 404'ünü sipariş sayfası verir | ✓ | C `..._KK7_KK8_KK9` (`a.b`, `a%3Cb`, `a%20b` 404 ve `frame-ancestors 'self'` yok; biçimli bilinmeyen anahtar 200, sipariş sayfası 404) |
| 10 | DV-5 maddeleri gözlenir; sonuç satıra tarihle yazılır | ✓ (gözlem OWNER_INPUT) | `08 §4.1` DV-5 Durum hücresi "Bekliyor — OWNER_INPUT (2026-11-14, T510 …)"; `PAYTR.md` §6.1 satırı; D-1466. **Açıkça:** satıra tarihle yazıldı; gözlemin kendisi (PayTR test mağazası ve anahtarı) sahip girdisi bekliyor — kod tarafı DV-5'in iki ihtimaline de hazırdır (çerçeve izni yalnız bu sayfada; çerçevesiz açılış 303'ü DV-5 sonucuna bağlı) |

## Doğrulama kontrol listesi (11 T510)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 kart ve havale görünümü | ✓ | C `..._KK1_KK3_KK4`, C `TheTransferScreen_..._KK1_KK6` |
| KK2 iki yönlendirme ve 404 | ✓ | C `TheMembersScreen_..._KK2`; misafir yönü KK5/KK6 testleri |
| KK3 jeton kayıtta yok; her açılışta yeni | ✓ | C `..._KK1_KK3_KK4` |
| KK4 çerçeve izni yalnız bu sayfada; `form-action`'da sağlayıcı yok | ✓ | C `..._KK1_KK3_KK4`; U `ThePolicy_...`; tarama testi |
| KK5 iptal, işaret, tek mesaj, L-7 | ✓ | C `AnUnreachableProvider_..._KK5` |
| KK6 yönlendirme, havale ve ikinci ret dönüşü | ✓ | C `TheSecondTokenRefusal_..._KK6`, C `TheTransferScreen_..._KK1_KK6` |
| KK7 geçiş sayfası durum yazmaz | ✓ | C `TheReturn_..._KK7_KK8_KK9` |
| KK8 betikli ve betiksiz geçiş; `frame-ancestors 'self'` yalnız bu yanıtta | ✓ | C `TheReturn_..._KK7_KK8_KK9`; `return.js` |
| KK9 biçim 404; geçersiz anahtar 200 | ✓ | C `TheReturn_..._KK7_KK8_KK9` |
| KK10 DV-5 satırında tarih ve ihlal kaydı | ✓ (OWNER_INPUT) | `08 §4.1` DV-5; D-1466 — gözlem sahip girdisi bekliyor (yukarıda) |
| Güvenlik taraması | ✓ | iframe istisnası tek ifadeye dar (`@Model.PaymentFrameAddress` + ayraç), ihlal örnekleri `@Model.OtherAddress`, `@Model.PaymentFrameAddressOfAnotherKind` (`ThirdPartyContentScanTests`); `PaymentFramePages` tek görünüm; sağlayıcı adı uygulama metninde geçmez (`PaymentFrameSource`) |

### `12` satır eşlemesi (Durum hücrelerini doğrulama yazar)

| Satır | Kanıt |
|---|---|
| `VAL-E089` | C `TheSecondTokenRefusal_..._KK6` (dizide iki hata varken ret: 303 teyit, kart görünümü yeniden açılır, Alındı + Bekliyor, geçiş kaydı yok, ayırma aynı, sepet işareti yok, kart kullanılabilir, dizi 2) + `PaymentStartsTests.TheSecondTokenRefusal_NeitherCountsNorBreaksTheSeries_T404_KK7` (ardından gelen hata üçe çıkarır) |
| `VAL-E094` | C `AnUnreachableProvider_..._KK5` (iptal, L-7 dışı, sepet notu bir kez, kart iki dakika kullanılamaz, dizi 1) + `PayTrPaymentGatewayTests.Row1_AServerError_IsAConnectionError_...`, `Row2_AFailedAnswer_IsAConnectionError_AndWritesTheReason_WithoutTheRequestsValues_...`; "Tarayıcı gözlemi" doğrulamada |
| `VAL-A970` | `StatusEndpointTests.ThePaymentSeries_IsClosedWhileYoung_OpensWhenOldEnough_AndTheFirstSuccessEndsIt_KK8` (T404; yeni test gerekmedi) |
| `VAL-E048` | `PaymentNotificationTests.AmountMismatch_IsRecordedMarked_..._KK5`, `ReferenceOfNoOrder_IsRecordedMarkedOrderNotFound_..._KK5` |
| `VAL-E049` | `PaymentNotificationTests.UnrecognizedOutcomeEventTypeOrUnreadableBody_IsRecordedMarkedUnrecognized_AndReceived_KK5` (bu turda tanınmayan olay türü eklendi) |
| `VAL-E050` | `PaymentNotificationTests.RecordThatCannotBeCommitted_Gets500WithAnEmptyBody_..._KK6_KK9` (500, boş gövde, `UnexpectedError` kaydı, sipariş Bekliyor kalır — bu turda eklendi) + `PaymentWindowTests.CardWindowEnd_WritesOneFinalQueryRow_AndLeavesTheOrderAsItIs`, `FinalPaymentQueryRuleTests` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil kriter 1–10, KK1–KK10 ve `09 §25` maddeleri diff'te baştan denetlendi; `sik-fail-nedenleri.md` okundu. Bu turda bulunup giderilenler: D-840 açıktı (ana akış testine başlatma adımı eklendi); `VAL-E089`/`E094` sayfa testi ayırmayı, sepet işaretini, kartın kullanılabilirliğini ve hata dizisini sınamıyordu (eklendi); `VAL-E049` için tanınmayan olay türü testi yoktu, `VAL-E050`'nin uygulama kaydı sınanmıyordu (eklendi).

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–10, KK1–KK10 | ✓ | yukarıdaki tablolar |
| (1) Bekçi/tarama istisnası geniş | ✓ | iframe istisnası tek özellik adına ve ayraca bağlı; adı uzatan ve başka özellik ihlal sayılır (iki olumsuz örnek). `OperationHandlerMarkTests` `_notOperations`'a tek işleyici eklendi: UC-29 `03 §1.11` işlemi değil (tabloda ödeme başlatma yok; 1.11.15 havale "ödendi" işareti ayrı) |
| (2) Kaynak taraması çağrı biçimi | ✓ | yeni tarama kuralı yok; `ThirdPartyContentScan` mevcut düzenli ifadesine tek dal; Architecture ad alanı Unit TAM içinde yeşil |
| (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok |
| (4) Kural değişince aynı sayıyı anan yerler | ✓ | `PaymentFramePages` "boş" diyen belge yorumu ve testi birlikte güncellendi; D-1077, D-1127, D-840 yerinde kapatıldı |
| (5) Belge kuralını değiştiren 09 metni | ✓ | `09` değişmedi; `08 §4.1` yalnız DV-5 Durum hücresi (kriter 10'un istediği yer) |
| Hedefi T510 olan backlog | ✓ | D-840 ✅, D-1077 ✅, D-1127 ✅; D-1080 ve D-981 T510 payı eşlendi (Durum hücreleri doğrulamanın); yeni: D-1466 (DV-5 OWNER_INPUT), D-1467 (sağlayıcı çerezleri yer tutucusu) |
| Para: tek kural evi | ✓ | sayfa toplamı `ConfirmationScreenFacts.Total`'dan (siparişe donmuş tutar) gösterir ve sağlayıcıya geçirir; sayfa katmanında tutar hesabı yok; karar `PaymentStartRule.Decide`'da (T404) |
| `09 §25` işlem sahibi / modül arayüzü `SaveChanges` çağırmaz | ✓ | iptal sayfanın `ITransactionRunner` işleminde; `CancelUnopenedAsync` kendi işlemini açmaz; başlatma işlem dışında (`05 §3.6.1`) |
| `09 §25` kilit sırası | ✓ | sepet → sipariş → varyant (`LockSequence`); KK5 testi |
| `09 §25` kişisel veri taşıyan sayfa `[NoStore]`, anahtarlı sayfa `[KeyedLinkPage]` | ✓ | iki teyit modeli `[NoStore]`, erişim kökü `[KeyedLinkPage]`; dönüş `no-store`, `Referrer-Policy` anahtarlı sayfa değeri (C `..._KK7_KK8_KK9`) |
| `09 §25` yetkilendirme sunucuda | ✓ | `OrderRoots.ResolveMemberAsync` / `ResolveAccessAsync`; C `..._KK2` (yanlış kök 404) |
| `09 §25` döngüde sorgu yok | ✓ | ekran tek sipariş okur (`IOrderConfirmationScreen.ReadAsync`); liste yok |
| `09 §25` enum saklanmayan listesi | ✓ | `PaymentStartDecision`, `UnopenedPaymentCancellation` `ApplicationEnums` NotStored; EnumTable/EnumLock/EnumSourceScan yeşil |
| Hız sınırı (07 UC-29: form · 64 KB) | ✓ | genel form sınırı her POST'a uygulanır (`RateLimitScope.IsFormLimited`) |

## Test Sonuçları

| Tür | Sonuç | Komut | Not |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | TAM 7567/7567 (son tur, head `9f4fdfd8`; kaynak ve Unit değişmedi) |
| Integration | ✓ | `--filter-class` | bu tur: `CheckoutPageTests` + `PaymentNotificationTests` 68/68, `MainFlowTests` 1/1; önceki tur: `CheckoutPageTests` + `SecurityHeadersTests` + `OrderConfirmationTests` 64/64, `PaymentStartTests` + `StorefrontFrameTests` + `FrameLayoutTests` + `OrderTrackingPageTests` + `CartTests` + `CartPageTests` 96/96; **TAM: CI** |
| Biçim | ✓ | `dotnet format --verify-no-changes` | değişen test dosyalarında temiz; önceki turda değişen kaynak dosyalarda temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-14), head `dee5e68a`; main `3fe7d829` (T517) çakışmasız birleştirildi, birleşik ağaçta doğrulandı |
| Başlangıç kapıları | Working tree temiz; `core.hooksPath` = `scripts/git-hooks` (ana çalışma alanı); main son 3 tamamlanmış run `success` (43517041992, 43517041955, 43502869408) |
| CI (#569) | 1–8 yeşil (run 43551463860; Integration 12m16s pass) |
| Yerel sınama | `dotnet build -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç projede temiz; Unit 7567/7567 (birleşik ağaç; dal ucunda Architecture 1757 + öteki 5515 + Consistency 295); betik testleri 4/4; Integration sınıf bazlı: `CheckoutPageTests`, `SecurityHeadersTests`, `MainFlowTests`, `PaymentNotificationTests` 78/78, `PaymentStartTests`, `CartTests`, `CartPageTests`, `ConcurrencyTests` 83/83, `StatusEndpointTests`, `PaymentWindowTests` 74/74, birleşik ağaçta `CheckoutPageTests` + `SecurityHeadersTests` + `AccountProfilePageTests` 64/64 (Postgres 18, `track_commit_timestamp=on`); **TAM: CI** |
| Kriter 1–9 | ✓ — kod okundu (`ConfirmationScreenModels.cs`, `OrderConfirmationScreens.cs`, `ContentSecurityPolicy.cs`, `SecurityHeadersMiddleware.cs`, `PaymentFrameSource.cs`, `Return.cshtml(.cs)`, `return.js`, görünümler) ve testler kendi koşumumda geçti; metinler `04` 3.5.8 ve 5.14.3–5.14.4 ile birebir |
| Kriter 10 | ✓ kayıt payı (`08 §4.1` DV-5 Durum hücresi tarihli, ihlal kaydı "yok — gözlem yapılmadı"; `PAYTR.md` §6.1 satırı; D-1466); gözlem payı OWNER_INPUT — T401 kriter 9 emsali; `VAL-E005` BEKLEMEDE |
| Doğrulayıcı sondası (silindi) | Üye kart siparişinde erişilemeyen sağlayıcı: 303 `/sepet`, İptal + Başarısız, hesabın sepetinde işaret, `dispatch_records` B7; sepeti olmayan misafir tarayıcı: iptal, hiçbir sepet işaretlenmez; oturum çerezli dönüş çerezsizle aynı gövde, sipariş satırları değişmez |
| Güvenlik | Sır yok; `frame-src 'self' https://www.paytr.com` yalnız `[AllowPaymentFrame]` + yanıt işareti + 200'de (teyit GET'i, hata yanıtları `'none'`); `frame-ancestors 'self'` yalnız dönüşün 200'ünde; `form-action` ve `script-src` genişlemez; çerçeve adresi `PaymentFrameSource.Holds` ile sağlayıcının form köküne bağlı (değilse istisna, çerçeve çizilmez); dönüş ve teyit GET'i yan etkisiz; anahtarlı sayfalar `no-referrer` + `no-store`; dönüş yolu ters vekil ve uygulama kaydında maskeli (`PathMasker`, Caddyfile); iptalde kilit sırası sepet → sipariş (`09` kilit sırası kuralı — "sepet kilidi sipariş tarafının en başındadır"; `05 §7.3.5`; `LockSequence`); POST sahtecilik korumalı; yeni bağımlılık yok |
| Bulgu sayısı | 0 |
| Bloke-etmeyen not (K) | K1 → D-1493 (`VAL-A585` beklenen sonucu UC-29'un "her açılış yeni jeton" kararıyla ayrışıyor; DV-11'e bağlı); K2 → D-1494 (T510'a bağlı BEKLEMEDE kalan sandbox ve tarayıcı satırları) |
| `12` | PASS: `VAL-A583`, `A666`, `A667`, `A683`, `A684`, `A685`, `A970`, `E048`, `E049`, `E050`, `E070`, `E071`, `E089`, `E094`; BEKLEMEDE: `E005`, `E006`, `E067`, `E068`, `E069` (sandbox/gerçek), `B009`, `B010`, `B011` (tarayıcı gözlemi), `A585` (D-1493); `12_matris_denetle.py --kapsam 1-77` hata 0 |
| Yapım raporu karşılaştırması | Kriter ve KK sonuçları uyumlu. Uyuşmazlık 1: raporun "`12` satır eşlemesi" yalnız `E089`, `E094`, `A970`, `E048`–`E050`'yi sayıyor; UC-28…UC-30'a bağlı `A583`, `A666`, `A667`, `A683`–`A685`, `E067`, `E070`, `E071` eşlenmemişti — doğrulama eşledi ve yazdı |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T510-siparis-teyit-donus`
- **PR:** #569
- **CI:** yapım işçisi beklemez; yönetici izler
- **Post-merge run'lar:** main `ce300c0e`: CI 43557742023 success, Publish 43557742117 success

## Dış Varsayımlar

DV-5 (PayTR formunun çerçevede açılışı ve dönüşün çerçevede yüklenmesi) gözlenmedi: test mağazası ve anahtarı sahip girdisi (OWNER_INPUT, D-1466). Kod DV-5'in beklenen sonucuna göre yazıldı (çerçeve izni yalnız ödeme sayfasında, dönüş `frame-ancestors 'self'`).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | DV-5 tarayıcı gözlemi (⚠ kriter 10'un gözlem payı) | D-1466 | PayTR test mağazası yok — OWNER_INPUT |
| 2 | Çerez politikası taslağında sağlayıcı çerezleri yer tutucusu | D-1467 | test kipinde gözlenince yazılır |
| 3 | ⚠ Sağlayıcı adı taraması ile üçüncü taraf içerik taraması çakışması: iframe `src="@Model.PaymentFrameAddress"`, adres `PaymentFrameSource.Holds` ile denetlenir; taramaya tek ifade istisnası | — | karar |
| 4 | ⚠ Çerçeve izni nitelik + yanıt işareti (çift kapı); teyit GET'i `frame-src 'none'` | — | karar |
| 5 | ⚠ Teyit yönlendirmesi köke göre: üye → `/siparis/{no}`, erişim → `/siparis-takibi` | — | karar |
| 6 | ⚠ İstemci adresi yoksa başlatma yok, 303 teyit | — | karar (`08 §2.1.4` `user_ip` zorunlu) |
| 7 | ⚠ Çerçeve yüksekliği dar/orta `100vh`, geniş `45rem` | — | karar |
| 8 | ⚠ Dönüş anahtarı biçimi = erişim kökünün kalıbı | — | karar |
| 9 | ⚠ İptalde sepet kilidi ilk kilit; sepeti olmayan tarayıcıda kilit/işaret yok; KK5 testinin eski beklentisi (tek kayıt) yanlıştı, O2 + S3 sınanır | — | karar (`05 §7.3.5`, `07 UC-29`) |
| 10 | ⚠ UC-29 işleyicisi `[GatedOperation]` dışı (`03 §1.11` işlemi değil) | — | karar |
