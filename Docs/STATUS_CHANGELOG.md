# Shopfolio — Status Changelog

> [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md)'nin **tarihsel arşivi**.
>
> **Neden ayrı dosya:** Güncel snapshot kısa kalmalı, tarihsel detay büyüyebilir (00 §G.2).
> Status dosyasının "son güncelleme" bloğu bir sonraki güncellemeye yer açarken **eski giriş buraya taşınır** — status dosyasında birikmez.
>
> **Sıra:** En yeni giriş en üstte.

---

## 2026-11-11 — T406 (PayTR: 3D Secure kurulum şartı, test kanıtı ve canlı kanıtın satırları) doğrulama tur 1 ✓ PASS

Dal ucu `e482f70f`, PR #530; `origin/main` ilerlemedi (`cda5032e`), birleştirme gerekmedi. Kriter 1–8 ve KK1–KK8, `09 §25` ve güvenlik taraması ✓ (kriter 3'ün teyit ve deneme payı, kriter 6'nın sandbox koşusu ve kriter 8'in DV-3 kapanışı OWNER_INPUT, D-1089/D-1090; çevrimdışı payı ve kayıtları tam). Uygulama kodu değişmedi; `src` altında 3D alanı adlandıran dizge yok (grep ve kaynak taraması). CI run 42919459743 `success`; yerel Unit 6598 (Consistency ve Architecture dahil), betik testleri, Integration `PaymentNotificationTests`, `PaymentWindowTests`, `PaymentStartTests` (76; T406 adlı 12 test) yeşil. `12` satırları (`VAL-E003`, `VAL-F001`, `VAL-F003`, `VAL-F009`, `VAL-F010` ve ilgili E satırları) `BEKLEMEDE` kaldı. K1/K2 bu PR'da kapatıldı: `DEPLOY_RUNBOOK §C` satır 1 §N.1 şartını anar; §0 özetine eksik §M satırı eklendi.

## 2026-11-11 — T406 yapım: PayTR 3D Secure kurulum şartı, test kanıtı ve canlı kanıtın satırları (dal `task/T406-paytr-3d-secure`)

Kod payı testlerle kanıtlandı, uygulama kodu değişmedi: `failed` + `failed_reason_code` `1`, `2`, `10` siparişi İptal + Başarısız yapar, ödeme kaydı doğmaz; uygulamanın kaynağında 3D alanı adlandıran dizge yok (kaynak taraması + sabitlerin yansıması), 3D'ye benzeyen alan sonucu değiştirmez; `payment_type` yalnız `card` (ordinal); canlıda `test_mode` = `1` işaretli (`PaymentNotificationTests.PayTrThreeDSecure`, `PayTrPaymentGatewayTests.ThreeDSecure`). Anahtar taraması (`PaymentKeysScanTests`): ayar, deploy, `.github` ve betik dosyaları anahtara değer vermez (çok satırlı JSON/YAML biçimi dahil), CI dosyaları PayTR'yi ve anahtarları anmaz, yerel referansın sır dosyası ayarı tanımlamaz (kart hattı kapalı). Belgeler: `DEPLOY_RUNBOOK §N` (N.1 kurulum şartı kaydı — üç satır ✓ olmadan `payment_provider_keys` yazılmaz; N.2 test modu kanıtı; N.3 `VAL-F001`…`VAL-F012` canlı kanıt satırları), §0 ve §B satır 12; `PAYTR.md` §6.1 DV-3, yeni §6.2 sandbox tablosu, §9; `08 §4.1` DV-3 son hücresi. PayTR test mağazası ve yazılı teyit yok (OWNER_INPUT): D-1089 (DV-3 teyidi ve 3D'siz deneme), D-1090 (sandbox koşusu); D-1034, D-1038, D-1075, D-1076, D-1082'nin "en geç T406" hedefi D-1090'a ileri atıldı. Yerel Unit 6598, Integration 3575 (beş parça) yeşil.

## 2026-11-11 — T405 (PayTR: kart iadesi) doğrulama tur 1 ✓ PASS

Dal ucu `be7accaf`, PR #529; `origin/main` ilerlemedi (`5a29775e`), birleştirme gerekmedi. Kriter 1–10 ve KK1–KK10, `09 §25` ve güvenlik taraması ✓ (kriter 10'un test mağazası gözlem payı OWNER_INPUT, D-1082/D-1083). Bilinen iade özeti openssl ile ayrıca hesaplandı ve testteki değere eşit. CI run 42906065591 `success`; yerel Unit 6483 (Consistency ve Architecture dahil), betik testleri, Integration `CardRefundJobTests`, `PaymentTransitionsTests`, `AutomaticMomentsTests`, `RefundAmountTests`, `ItemRecordsTests`, `PaymentRecordTablesTests`, `JobCleanupAndRefundTests`, `PanelHomeTests`, `StatusCorrectionTests`, `OrderCorrectionTests`, `JobRunnerTests`, `JobEnqueueTests`, `PaymentWindowTests`, `ShipmentTransitionsTests` 517 yeşil. `12`: `VAL-E093`, `VAL-A990` → `PASS`; `VAL-E007`, `E011` (DV-6/DV-10 gerçek kayıt), `E008`, `E009`, `A594` (panel uyarısı T529, D-1084), `A985` (panel uyarısı ekran task'ında) `BEKLEMEDE`. Bulgu 0; K1 (gecikmeli koşuda sorgu takvimi bir aralığı atlayabilir) → D-1087.

## 2026-11-11 — T405 yapım: PayTR kart iadesi (dal `task/T405-paytr-kart-iadesi`)

Kart iadesi `POST /odeme/iade` ile istenir (`PayTrRefund`): token dört girdili, `reference_no` (kaydın tiresiz kimliği) özete girmez, `return_amount` kültürden bağımsız iki ondalık; `success` → `Accepted`, `failed`/`error` → `NotCompleted`, yanıtsız → `Unknown`; anahtarsız istek yok. "Gönderiliyor" kaydı siparişin kilidi altında kesinleşmeden istek gitmez ve yinelenmez; geçersizleşmiş ya da havaleye çevrilmiş geri ödemeye istek yok (D-518 ✅, D-908 ✅). Sonucu bilinmeyen iade 1 dk sonra, sonra 1/5/15/60 dk, sonra saatte bir üst sınırsız sorulur (`RefundOutcomeQueryRule`, `09 §3.5` satırı); eşit öğe `Completed`, başkası `NoResult`. İki sonuç yolu aynı işlevle uygulanır; firma işaretinden sonra sorgu yok, işaret anındaki "gerçekleşti" `LateSuccessAt`'te kalır. İki iş işleyicisi kayıtlı (D-97 ✅); ayıp çözümünün kart iadesi iş satırı yazar (D-878 kart payı ✅). DV-6/DV-10 gözlemi OWNER_INPUT (D-1082, D-1083); işaret komutu ve panel uyarısı T529 (D-1084). Unit 6482, Integration 3563 yeşil; `dotnet format` üç projede temiz.

## 2026-11-10 — T404 (PayTR: bağlantı hatası ve ayrım) doğrulama tur 1 ✓ PASS

Dal ucu `91d8ad6d`, PR #528, `origin/main` `83938803` ile çakışmasız birleştirildi; kriter 1–8 ve KK1–KK8, `09 §25` ve güvenlik taraması ✓ (kriter 2 ve 8'in test mağazası gözlem payı OWNER_INPUT, D-1075/D-1076; kriter 5'in iade sorgusu payı T405 k4–k5). CI run 42889751461 `success`; yerel Unit 6161 + Consistency 294, Integration `Payment` + `Ordering` 668 ve `StatusEndpointTests` yeşil. `12` satırları değişmedi: `VAL-E061`, `E089`, `E094`, `A970` sayfa kanıtı ister, `BEKLEMEDE` (T509/T510; K1 → D-1080); `VAL-E013` DV-12 gözlemi (D-1076). Bulgu 0.

## 2026-11-10 — T404 yapım: PayTR bağlantı hatası ve ayrım (dal `task/T404-paytr-baglanti-hatasi`)

Başlatma yanıtının tablosu (`08 §2.1.8`) `PayTrPaymentGateway.ReadAnswer`'da: bağlanılamama, 15 sn aşımı, 2xx dışı, `failed` ve tablosuz yanıt bağlantı hatasıdır; `failed` yanıtında kayda `failed:` ve sağlayıcının `reason`'ı yazılır, istek alanı değerleri `[field]` ile değiştirilir (D-1035 ✅). İkinci jeton reddi `SecondTokenReasonTexts`'teki metinle ayrılır; metin gözlenmediği için liste boş (DV-11, D-1075). `PaymentStarts` bağlantı hatasında hata dizisini sayar (T314 k8) ve kartı iki dakika "şu an kullanılamıyor" tutar (`CardUnavailability`, bellekte; `09 §3.5` satırı); başarılı jeton diziyi sıfırlar, ikinci ret ne sayar ne bozar. Kart hâldeyken açık yöntemlerden düşer ve kartla onay jetonsuz `PaymentMethodUnavailable` alır; onaylanmış siparişin başlatması jetonu yine ister. Açılamayan form `PaymentPageNotOpened` ile iptal edilir, L-7'ye sayılmaz. `PaymentStartRule.Decide` `switch` ifadesi (D-110 K1 ✅). DV-12 gözlemi OWNER_INPUT (D-1076); ekran bağlantısı T509/T510 (D-1077). Unit 6161 + Consistency 294, Integration 3571 yeşil; `dotnet format` üç projede temiz.

## 2026-11-10 — D-1074: Dependabot güncellemeleri (F4 chore, dal `chore/deps-2026-11-f4`)

`AWSSDK.SimpleEmailV2` 4.0.106.10, EF Core ailesi + `dotnet-ef` 10.0.15, `Microsoft.AspNetCore.Mvc.Testing` 10.0.15 ve `HtmlSanitizer` 11.0.26 tek PR'da alındı (Dependabot #521, #523, #524, #525); `dotnet-ef` 11.0.0 (#522) `09 §4.3` gereği alınmadı. Kilitler `--force-evaluate` ile yenilendi, `--locked-mode` çıkış 0; `09` Kütüphaneler tablosu güncellendi. Yerel: `-warnaserror` 0 uyarı, Unit 6415/6415 (Consistency ve Architecture dahil), Integration Database 36, Security/Outages/Jobs/Model 1579, Web/Pages 600 geçti, betik testleri 4/4.

## 2026-11-10 — T403 (PayTR: son sorgu ve tek tahsilat) doğrulama tur 1 ✓ PASS

Dal ucu `c08172ff`, PR #526; kriter 1–9 ve KK1–KK9, `09 §25` ve güvenlik taraması ✓ (kriter 9'un test modu gözlem payı OWNER_INPUT, D-1065; `VAL-E013` `BEKLEMEDE`, `Durum` değişmedi). `VAL-E090` (mock, KRİTİK) `BEKLEMEDE` → `PASS`. Son sorgunun `paytr_token`'ı (`123456` + `20267K4M9P` + `salt-sample`, anahtar `key-sample`) openssl ile .NET dışında yeniden hesaplandı, eşleşti. Yerelde restore, `-warnaserror` derleme, üç projede `dotnet format`, Unit 6123 + Consistency 292, betik testleri, Integration parçalı (`PaymentWindowTests` 33, `PaymentNotificationTests` 29, komşu sınıflar 92) yeşil; dal CI'ı (run 42876712837) yeşil. Bir K-notu: K1 → D-1072 (`NoResponse.CodeError` `09 §11.1`'de yazılı değil).

## 2026-11-10 — T403 yapım: PayTR son sorgu ve tek tahsilat (dal `task/T403-paytr-son-sorgu`)

`PayTrPaymentQuery` son sorguyu `POST /odeme/durum-sorgu` form kodlu üç alanla (`paytr_token` = `merchant_id` + `merchant_oid` + `merchant_salt`) kurar ve yanıtı okur: `success` → `Paid` (sorulan `merchant_oid` ödeme kimliği), `004` → `NoPayment`, `001`–`003` ve tanınmayan tutar metni → kod hatası kuralı, kesinti ve tablosuz yanıt → beş dakika; sipariş yanıtsızlıkta iptal edilmez. Başarının beş koşulu çekirdekte (`FinalPaymentQueryRule.NotAppliedReason`, `PaymentAmounts.QueryMatches`); uygulanmayan başarı `FinalPaymentQueryNotApplied` uyarı satırı. `FinalPaymentQueryJobHandler` `PaymentModule`'de kayıtlı (D-449), ağ geçidisiz host kurulur. `PaymentAmounts.TryParseProviderText` DV-12 kuralıyla; `09 §14.2` m.4'ün mimari testi derlenmiş koddan. İkinci başarılı bildirim tekrar sayılır (KK8). D-448, D-449, D-741 ✅; DV-12 gözlemi OWNER_INPUT (D-1065). Unit 6113 + Consistency 292, Integration 3546 yeşil; `dotnet format` temiz.

## 2026-11-10 — T409 doğrulama tur 2: ✓ PASS

Kriter 1–7 ve KK1–KK8 ✓ (birleşik ağaçta: build `-warnaserror` 0 uyarı, format temiz, Unit 6329/6329 — Google sınıfları 110, betik testleri 4/4, Integration `GoogleRegistrationTests` 2/2 ve Web ad alanı 600/600; PR #519 CI yeşil, run 42844491708). Kriter 8 depo payı ✓: `GOOGLE.md` 9 bölüm, yedi DV satırı çevrimdışı gözlemle tarihli; gerçek uygulama gözlemi OWNER_INPUT (D-1029), 12'nin `VAL-E026`…`E044` Google satırları `BEKLEMEDE` kalır. K-notları: K1 `iss` biçimi `08`↔`09` → D-1030 (var); K2 vazgeçme dönüşü bağlayıcıya ulaşmaz, boş kod `Unavailable`'a düşer → D-1070 (T515). main birleştirmesinde `PRODUCT_DISCOVERY_STATUS.md` karar tablosu: K-1605 (main) ve K-1606 (dal) sırayla tutuldu.

## 2026-11-10 — T409 düzeltme 1: bilinmeyen `kid` aynı çağrıda yenileniyor

Bulgu 1 (kriter 5 / KK5) giderildi: çatının `Switch.Microsoft.IdentityModel.UpdateConfigAsBlocking` anahtarı `Directory.Build.props` içinde süreç başından açıldı (çatı anahtarı ilk dokunuşta bir kez okur; kayıtta `AppContext.SetSwitch` test ve sunucuda geç kalıyordu). Bilinmeyen `kid` + yeni kümede anahtar → ilk çağrı `Succeeded`, kod değişimi 1; yeni kümede de yoksa `Failed`. `09 §11.3` m.2 notu, DV-31 hücresi, GOOGLE.md satırı, D-1031 ✅. Doğrulama yeniden bekliyor.

## 2026-11-10 — T409 doğrulama tur 1: ✗ FAIL

Bir bulgu (ORTA, S1): kriter 5 / KK5 — bilinmeyen `kid`'li belirteç, yenilenen kümede anahtar olsa da o çağrıda tutmuyor (yenileme arkada); `08 §2.3` "yeniden çekilir, yine yoksa imza tutmaz" der. Çatının `Switch.Microsoft.IdentityModel.UpdateConfigAsBlocking` anahtarıyla ilk çağrı tutuyor (elle yeniden deneme olmadan), D-1031'in gerekçesi düşüyor. Öteki kriterler karşılandı; CI yeşil; Unit 6214/6214. Dal merge edilmedi; düzeltme turu gerekiyor.

## 2026-11-10 — T409 (Google: giriş, kayıt ve yeniden doğrulama bağlayıcısı) yapım tamam, doğrulama bekliyor

`GoogleIdentityGateway` yetki kodu akışının dönüşünü uygular: kod değişimi tek `POST` (sır gövdede, `userinfo` yok), kimlik belirteci `Microsoft.IdentityModel.JsonWebTokens`/`Protocols.OpenIdConnect` 8.23.0 (MIT) ile doğrulanır — yalnız `RS256`, imza, `iss` (keşif belgesinin yayıncısı), `aud`, `exp` (60 sn). Keşif belgesi ve anahtar kümesi tek `ConfigurationManager`'da önbelleklenir; bilinmeyen `kid` çatının arka plan yenilemesini ister, elle yeniden deneme yoktur. Sonuç yalnız e-posta, doğrulama bayrağı, ad, `hd`, `auth_time` ve `nonce` taşır; `sub` yok. Yetkilendirme isteği (`GoogleAuthorizationRequest`) `openid email profile`, `state`, `nonce`, S256'yı taşır, yeniden doğrulamada `claims` ile `auth_time` ister; `prompt` ve `max_age` yok (DV-26 gözlenmedi). `GOOGLE.md` açıldı (9 bölüm); DV-25/26/27/28/31/37/43 "kısmen gözlendi" (gerçek uygulama gözlemi OWNER_INPUT, D-1029). `LinkKeySourceScan` istisna listesine PKCE özet dosyası eklendi.

## 2026-11-10 — T412 doğrulama tur 3: PASS

Kriter 1–9 ve KK1–KK9 ✓ (tur 2 B1 kapandı: `09 §10.2` m.3 yeni sipariş muafiyetini yazar, K-1605). CI yeşil (run 42836235245, tam Integration dahil); yerelde derleme 0 uyarı, Unit 6154/6154, betik testleri ve `test-pre-commit.sh` yeşil, Integration dokunulan ve komşu sınıflar 958 yeşil. Bulgu yok, yeni K-notu yok. STATUS satırı ✓ Tamamlandı.

## 2026-11-10 — T412 düzeltme 2: sipariş bekçisinin yeni sipariş muafiyeti belgeye yazıldı

Tur 2 B1 kapandı, kod değişmedi: `09 §10.2` m.3 aynı `SaveChanges`'te eklenen siparişin kayıtlarının kilit ve artış denetiminin dışında olduğunu ve gerekçesini yazar (satırı işlem kesinleşmeden görülmez; sipariş onayı önceki siparişin kilidi altında yeni siparişi yazar); var olan siparişte sıra kilit → artış. Karar kaydı K-1605; `05 §7.3.2` ve `06 §2.6.4` hizalı. İki yeni Integration testi (`ConcurrencyTests.AnOrderAddedInTheTransaction_…_T412_KK1`): eklenen sipariş kilitsiz yazılır; var olan kilitsiz siparişe yazım aynı kayıtta yine kilit iletisiyle reddedilir. Yerel sınama yeşil (Unit 5850 + 292, Integration Concurrency 30, Ordering 611).

## 2026-11-10 — T412 doğrulama tur 2: FAIL (iki iterasyon sınırı, eskalasyon)

Kriter 2–9 ✓ (tur 1 B1 kapandı), kriter 1 ~ Kısmi: `OrderVersionGuard` bu kayıtta eklenen siparişin kayıtlarını kilit ve artış denetiminden muaf tutar, `09 §10.2` m.3 bu muafiyeti yazmaz ve "kilidi tutulmayan siparişe yazım … reddedilir" der; muafiyet aynı sepetten yeni sipariş onayında yük taşır (B1, S1 DÜŞÜK; düzeltme yalnız paragrafa bir cümle). CI yeşil (run 42821586998), yerel sınama yeşil. Yeni K-notu yok.

## 2026-11-10 — T412 düzeltme 1: ödeme kümesinin tümleyen kopyaları tek eve bağlandı

Tur 1 B1 kapandı: `DeliveryMarkRules.IsPaymentOpenToMark` `PaymentApproval.IsApproved`'u, `PendingWorkReader` `ServiceUnmarked` sorgusu `PaymentApproval.Statuses`'u çağırır; S9 teslim kapanışındaki yasak çift denetimi çift tablosunu okur (`TransitionTable.IsPairAllowed`). `PaymentApprovalTests` taraması kümeyi tümleyeniyle yazan beş biçimi de (sıra, boşluk, satır kırılımı dahil) yakalar; her kalıp için ihlal örneği. Davranış aynı. Unit 6142 ✓; Integration Ordering 620, Administration 69 ✓. Doğrulama tur 2 bekliyor.

## 2026-11-10 — T412: F3 kapanış düzeltmeleri — sipariş bekçisi, satış sonrası kapanış ve IBAN kullanımı (doğrulama tur 1: FAIL)

Kriter 1–8 ✓, kriter 9 ~ Kısmi: "ödeme onaylanmış" kümesini okuyan `DeliveryMarkRules.IsPaymentOpenToMark` ve `PendingWorkReader` `ServiceUnmarked` sorgusu `PaymentApproval`'ı kullanmıyor, kümeyi tümleyeniyle (`Pending`/`Failed` dışı) yazıyor; tarama bu biçimi görmüyor (B1, S1 DÜŞÜK). CI yeşil, yerel sınama yeşil. K1 → D-1036 (sır bekçisi öneki yalnız satır başında soyuyor). Düzeltme turu bekliyor.

## 2026-11-10 — T412 yapım: F3 kapanış düzeltmeleri — sipariş bekçisi, satış sonrası kapanış ve IBAN kullanımı (dal `task/T412-f3-kapanis-duzeltmeleri`)

`OrderVersionGuard` kökü bilinmeyen kaydı her durumda reddeder, kilitsiz siparişe yazıma sürümden önce kilit iletisi verir; kökü yüklemeyen iki yol kökü yükler (K-1581; `09 §10.2` m.3). L-7/L-8 e-posta ekseni üyenin `CustomerId`'siyle (`09 §15.4` m.5). IBAN kullanımı sipariş başına tek yüklemeyle (`RefundIbanUses`) ve Z-38 bütün konuları sayar: kargodan önceki ve hizmet caymasının kaydı, istek bağlantılı geri ödeme. Bütün adetleri dönülmüş kalem S5/S8'e ve teslim işaretine girmez, S9 kapanıştır (`02 §7.1.6`). Ret de "mal dönmedi" kapanışını açar (`06 §4.26`). Kargo bilgisi tavanı 64/100 (`ShipmentLimits`; K-1586). E-posta yerel kısmının nokta kuralı (`04` 7.3.2.1). Sır bekçisi `-e`/`--env` önekini soyar. Ödeme kümesi tek evde (`PaymentApproval`). Backlog: D-80, D-95, D-309, D-379, D-808, D-847, D-896, D-903, D-905, D-910, D-915 ✅. Unit 6018 ✓; Integration 3502 ✓ (parçalı, tamamı).

## 2026-11-10 — T402 (PayTR: bildirim imzası ve alan eşlemesi) doğrulama tur 1 ✓ PASS

Dal ucu `27720f76`, PR #520; kriter 1–8 ve KK1–KK8, `09 §25` ve güvenlik taraması ✓ (kriter 8'in test modu gözlem payı OWNER_INPUT, D-1038; `VAL-E001`/`E002`/`E004`/`E010`/`E036`/`E037`/`E043` `BEKLEMEDE`, `Durum` değişmedi). İki bilinen `hash` örneği openssl ile bağımsız yeniden hesaplandı, eşleşti. Dal CI 42836227559 success; ana dal son 3 run success. Yerel: `-warnaserror` 0 uyarı, `format` temiz, Unit 5891/5891, Consistency 292/292, betik testleri 4/4, Integration ilgili sınıflar 101/101. Bulgu 0; K1 → D-1059 (okunamayan tutarın işaret nedeni `09 §14.2` m.3 ile çekirdek arasında ayrışıyor), K2 bu PR'da kapatıldı (rapordaki `VAL-E005` → `VAL-E004`).

## 2026-11-10 — T402 yapım: PayTR bildirim imzası ve alan eşlemesi (dal `task/T402-paytr-bildirim`)

`PayTrNotification.Verify` `merchant_oid` + `merchant_salt` + `status` + `total_amount`'ın `merchant_key` ile HMAC-SHA256 Base64'ünü `hash` ile sabit zamanlı karşılaştırır; `hash` yok/bozuk, imzalı alan ya da anahtar ayarı eksikse `SignatureInvalid`; `payment_amount`, `payment_type`, `currency`, `test_mode` imzasız, yalnız tutarlılık. Eşleme: `merchant_oid` tiresiz iki alana, `status` → `Outcome`, `total_amount` → `Amount`, olay `Sale`; tutar koşulu `PaymentAmounts.NotificationMatches` (kuruşta, taksit farkı geçer). `failed_reason_code`/`failed_reason_msg` saklanmaz, `PaymentFailureReasonLog` ile seyreltilerek kayda (`PaymentFailedReasonSampled`, 60 sn). Katman HTTP yanıtı üretmez. D-108 (imza hatası satırının pencere tavanı 10 000, K-1582), D-455 ✅; D-741 T402 payı. `08 §4.1` ve `PAYTR.md` §6.1'de DV-1/2/4/9/35/36/42 "Bekliyor — OWNER_INPUT" (D-1038). Unit 6171, Integration parçalı tam koşu 3505 geçti. PR #520. Doğrulama bekliyor.

## 2026-11-10 — T408 doğrulama tur 2: PASS

Kabul kriterleri 1–8 ve KK1–KK8 karşılandı (KK8: DV-22 satırı çevrimdışı payla yazılı, gerçek hesap gözlemi OWNER_INPUT → D-1024; `VAL-E023` BEKLEMEDE kalır). `VAL-E084`, `VAL-E085` PASS. Yerel: derleme 0 uyarı, biçim temiz, Unit 6118/6118, betik testleri 4/4, Integration `BounceQueryTests` + `EmailCaptureRegistrationTests` 14/14; dal CI run 42828644455 yeşil. Bulgu yok; K1 (sorgunun sayfa boyu `09 §3.5`'te yazılı değil) → D-1053.

## 2026-11-10 — T408 düzeltme 1: ağ geçidisiz açılış

`BounceQueryJobHandler` ağ geçidini yapıcıdan değil, her turda `IServiceProvider.GetService` ile ister; ağ geçidi kayıtlı değilken tur yalnız 15 dakika sonrasına yeniden zamanlar, hiçbir şey yazmaz ve uygulama hizmet doğrulamasında açılır. Kayıt `AddBounceQuery()` içinde toplandı; `BounceQueryTests` onu kullanır. Yeni test: `EmailCaptureRegistrationTests.Application_WithNoGateway_Opens_AndTheReturnedAddressQueryRunsWithoutOne`. K1 kapandı: `JobTimings.BounceQueryWindow` yorumu düzeltildi, D-1045 ✅. Integration paketi TAM koşuldu (yeşil; yük altında bir kez görülen zaman aşımı ve geçici dizin yarışı tek başına yeniden koşulunca geçti), Unit 6118/6118.

## 2026-11-10 — T408 doğrulama tur 1: FAIL

`NotificationsModule` `BounceQueryJobHandler`'ı koşulsuz kaydeder; işleyicinin yapıcısı `IEmailGateway` ister. Ağ geçidi kayıtlı değilken (test sunucusu `ShopfolioWebApplicationFactory` onu varsayılan olarak kaldırır; ağ geçidi `DispatchAttempts`'te isteğe bağlıdır) uygulama hizmet doğrulamasında açılmaz: CI Integration 620/3499 kırmızı (run 42818255272), yerelde `JobQueueRegistrationTests` ve `FormatPageTests` aynı hatayla düşer. Kabul kriterlerinin davranışı (`BounceQueryTests`, `SesEmailGatewayTests`, `BounceQueryRulesTests`) yeşil; `VAL-E084`, `VAL-E085` PASS, `VAL-E023` BEKLEMEDE. K1 → D-1045.

## 2026-11-10 — T408 (SES: geri dönen adreslerin öğrenilmesi) yapım tamam, doğrulama bekliyor

`SesEmailGateway.QueryBouncesAsync` `ListSuppressedDestinations` çağrısını yazar: `StartDate`, `Reason` = `BOUNCE`, sayfa boyu 1000 ve `NextToken` döngüsü, tek çalıştırıcı çağrısı; sayfalardan biri düşerse kısmi liste verilmez (`QueryFailed`), `BOUNCE` dışı özet okunmaz. `BounceQueryJobHandler` iş türünün işleyicisidir: adresleri `NormalizeIdentity` ile harf ayrımsız eşler; `SentAt` son 24 saatte olan bütün `Sent` satırlar `Undelivered` ve `RecipientError` olur, `SentAt` kalır, kanal hâli kaydına dokunulmaz, ikinci uygulama etkisizdir; her tur (başarısız dahil) tek bekleyen satırı 15 dakika sonrasına bırakır, başarısız tur boş geçer ve satırı başarısız etmez. `BounceQueryRowSource` zamanlayıcı kaynağıdır: ağ geçidi kayıtlıysa ve bekleyen satır yoksa tek satırı yazar (ilgili kayıt kimliği boş). Kodda silme ya da liste yazma çağrısı yoktur (kaynak taraması); `AMAZON_SES.md` §10 eklendi (hesap listesi yalnız `BOUNCE`, elle çıkarma adımı); DV-22 "kısmen gözlendi" (gerçek hesap gözlemi OWNER_INPUT, D-1024). D-1017 ve D-97'nin `BounceQuery` payı kapandı; D-1022 T410'a ileri atıldı; yeni: D-1024, D-1025.

## 2026-11-10 — T401 (PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve) doğrulama tur 1 ✓ PASS

Head `d7062c8`, PR #516; kriter 1–9 ve KK1–KK9, `09 §25` ve güvenlik taraması ✓ (kriter 9'un test modu gözlem payı OWNER_INPUT, D-1034; `VAL-E012`/`E015`/`E039`/`E040`/`E042` `BEKLEMEDE`; KP-15 satırları T510/T402 ile tamamlanır, `Durum` değişmedi). `paytr_token` bilinen örneği openssl ile bağımsız yeniden hesaplandı, eşleşti. Dal CI 42806712266 success; ana dal son 3 run success. Yerel: `-warnaserror` 0 uyarı, `format` temiz, Unit 5814/5814, Consistency 292/292, betik testleri 4/4, Integration ilgili sınıflar 63/63. Bulgu 0; K1 → D-1034 (DV gözlemi izsizdi), K2 → D-1035 (`reason` metni kayda yazılmıyor; T404).

## 2026-11-10 — T401 yapım: PayTR anahtarları, ödemenin başlatılması, dönüş ve çerçeve (dal `task/T401-paytr-baslatma`)

`PayTrPaymentGateway.StartAsync` jeton isteğini `https://www.paytr.com/odeme/api/get-token` adresine sunucudan sunucuya, form kodlu gönderir; `merchant_id`, `merchant_key`, `merchant_salt` tek ayardır (sır dosyası, üç `name=value` satırı), biri eksikse kart yöntemi kapalı ve çağrı yok. `paytr_token` on bir girdinin HMAC-SHA256 Base64'ü (`MessageSignature`, `Infrastructure/Security`); tutar kuruş, `user_basket` tek kalem, `merchant_oid` tiresiz, yedek e-posta/telefon ve kesme sınırları, `user_ip` ve `timeout_limit` 08 §2.1.4'e göre; jeton saklanmaz, dönüş adresleri `/odeme/donus/{erişim anahtarı}`. `INTEGRATION_RUNBOOKS/PAYTR.md` açıldı (DV-11/14/38/39/41 Bekliyor — OWNER_INPUT). D-105 (sağlayıcı adı taraması: `ses`/`AWS`/büyük harf içi biçimler, `.razor` ve `.js`), D-786 ✅; D-840 T401 payı kapandı, kalan T510'a. Unit 5681 + Consistency 292, Integration parçalı tam koşu geçti. Doğrulama bekliyor.

## 2026-11-10 — T407 (SES: gönderim ve sonuç sınıfları) doğrulama tur 1 ✓ PASS

Head `ed0866f`, PR #515; kriter 1–9 ve KK1–KK9, `09 §25` ve güvenlik taraması ✓ (kriter 9'un gerçek hesap payı OWNER_INPUT, D-1015; `VAL-E017`/`E018`/`E020`/`E022`/`E024` `BEKLEMEDE`). Dal CI 42797568953 success; ana dal son 3 run success. Yerel: `-warnaserror` 0 uyarı, `format` temiz, Unit 5924/5924 (main birleştirmesi sonrası 6006/6006), betik testleri 4/4, Integration ilgili sınıflar 97/97. Bulgu 0; K1 → D-1022, K2 → mevcut D-442/D-966.

## 2026-11-10 — T407 (SES: gönderim ve sonuç sınıfları) yapım tamam, doğrulama bekliyor

`SesEmailGateway` AWS'nin resmî istemcisiyle (`AWSSDK.SimpleEmailV2` 4.0.102.1, Apache-2.0) gönderir: bir çağrı bir alıcı, her gönderimden önce `GetSuppressedDestination` ön sorgusu (200 ve `BOUNCE` → `RecipientError`, 404 → gönderilir, başka yanıt gönderimi durdurmaz ve kanal hâline yazılmaz); istemci tekil, yeniden deneme kapalı, 30 sn, açık kimlik, ortam zinciri ve ortam uç adresi yok; hata eşlemesi yalnız SendEmail yanıtını alan saf işlev; `MessageId` saklanmaz. `email_credential` iki satırdır (erişim anahtarı kimliği, gizli anahtar) ve açılış denetimi `EmailCapture:Host` boşken biçimi sınar (D-79 ✅); `SmtpCaptureEmailGateway` yanıt adresini `TryCreate` ile kurar (D-106 ✅). `AMAZON_SES.md` açıldı (9 bölüm, IAM politikası testli); DV-16/17/19/21/23 "kısmen gözlendi" (gerçek hesap gözlemi OWNER_INPUT, D-1015). Unit 5924/5924, Integration (Web, Jobs, Outages, Pages, Administration, Ordering ve öteki ad alanları; Model ve Database hariç) yeşil, `-warnaserror` 0 uyarı, `dotnet format` temiz. Yeni backlog: D-1015, D-1016, D-1017 (T408 devri).

## 2026-11-10 — T411 doğrulama tur 1: ✓ PASS (head `dcbae57a`)

Kriter 1–7 ve KK1–KK7 ✓; bulgu 0. Dal CI 42790472058 success; yerelde derleme `-warnaserror` temiz, Unit 5873/5873, betik testleri 4/4, Integration Logging 30/30. Bir K-notu: sabit adıyla verilen şablonda istisna bağımsız değişkeni ve şablon olarak istisna iletisi taranmaz → D-1020 (bugün `src/`'de örneği yok). `12`'de T411 satırı yok.

## 2026-11-10 — T411 yapım: kayıt maskeleme taramasının dizge farkında yeniden yazımı (dal `task/T411-kayit-maskeleme-taramasi`)

`LogTemplateSourceScan` ve `RejectionLineSourceScan` ortak sözcüksel okuyucu `LogSourceText` ile yeniden yazıldı (dizge ve yorum farkında; düz, ara değerli, sözcük, ham ve birleştirilmiş dizge; konumlu `[LoggerMessage]`, `Define`, `BeginScope`/`ForContext`/`PushProperty`/`WithProperty`, `const string`); ad kuralı "içerir" ve biçimleyici ile tarama aynı `IsForbiddenName` yöntemini kullanır, açık izin listesi `AllowedPropertyNames`; şablona bağımsız değişken olarak verilen istisna bulgu; red satırı taraması `ErrorCode` türlü bağımsız değişkeni, `ErrorCode`/`RejectionCode` içeren adları ve `.cshtml`/`.razor` satırlarını kapsar. `CoreEventId.SaveChangesFailed` üçüncü bastırılan olay oldu; `09 §18.1` madde 4 ve 8 ile `§18.2` madde 2a, 2c, 7 eşitlendi. D-89 ve D-90 ✅; yeni D-1005. Unit 5873 geçti, Integration Logging ad alanı 30 geçti. Doğrulama bekliyor.

## 2026-11-10 — F3 (İş mantığı) faz kapısı ✓ PASS (`chore: F3 gate check PASS`, dal `chore/F3-gate`)

2026-11-10 — F3 gate check ✓ PASS (tur 1, bağlam sınırında iki kez bölünüp üç parçada bitti). 90/90 task ✓ (97 merge: 90 task + 7 chore). Release `-warnaserror` 0 uyarı, `dotnet format` üç proje temiz, Unit 5791/5791, Integration 3487/3487 (Ordering 614, Web 595, Model 1404, kalan 874), betik testleri 4/4; 19 göç boş veritabanına ve F2 şemasından özdeş; paket denetimi 7/7 (uygulama imajı 792 MB → 315 MB, D-160). `12 §6.2` F3: B (T329 `MainFlowTests`, `BranchFlowTests`) ve C (`12 §12.3` iki tablo, 16 geçiş, `TransitionTableConsistencyTests`) ✓; `11 §7.1` F3 hücreleri dolu, `GATE_DOLU` += F3. Backlog: F3 kapısına bırakılmış 105 kalem karara bağlandı — 73 kapandı, 32 yeniden hedeflendi; yeni F4 task'ları T411 (kayıt maskeleme taramasının dizge farkında yeniden yazımı) ve T412 (F3 kapanış düzeltmeleri, para), T601'den T601b bölündü; karar kaydı K-1579…K-1604; D-159 doküman geçişi (`02` v0.115, `03` v0.49, `05` v0.52, `06` v0.48, `08` v0.23, `09` v0.32, `11` v0.64, `12` v0.35); yeni D-980 (test sunucularının ortak geçici klasörü), D-981 (`12 §12.3` satırlarının `Durum` sahibi), D-982 (568 zayıf matris kanıtı). D-961: `Files` + `Web` üç paralel koşu 721/721, 721/721, 720/721 (başka test, D-980) — üst sınır F4 gate. Kabul edilen riskler ve öğrenim terfisi raporda (`GATE_CHECK_F3.md`). Parçalar toplandı; "Aktif kalem" yeniden sayıldı: 285.

## 2026-11-10 — T364: Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler (doğrulama tur 2: ✓ PASS)

2026-11-10 — T364 doğrulama tur 2 ✓ PASS (#511, head `dd5fb1cb`; `origin/main` `ed1ee67c` dal tabanı, birleştirme gerekmedi; dal CI 42747369700 bütün işler yeşil). Kapsam tam: kriter 1–6, KK1–KK6, `09 §25`, güvenlik ✓. B1 kapandı: yöneticinin sürümsüz sipariş isteği `ShownVersion.Check` ile `InvalidInput` (altı müdahale, yönetici kalem işlemleri, geçiş biriminin gövdesi, geç teslim feshi); öteki yönetici istekleri sürümü `int` olarak zorunlu taşır; müşterinin ve sistemin işlemi sürümsüz, güncel hâle karşı. Yerel: build 0 uyarı, format ×3 temiz, Unit 5786/5786, betik 4/4, Integration değişen dört sınıf 145/145, Ordering + Concurrency 638/638, kalan ad alanları 2812/2812. `12` KP-76 beş satırı uç/tarayıcı kanıtı beklediği için `BEKLEMEDE` kaldı; D-961 açık.

## 2026-11-10 — T364: Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler (düzeltme 1 tamam, doğrulama tur 2 bekliyor)

2026-11-10 — T364 düzeltme 1 tamamlandı (#511), doğrulama tur 2 bekliyor: B1 — yöneticinin siparişe yönelik her isteğinde gösterilen sürüm zorunlu (D-291 (b)); karar tek yerde `ShownVersion.Check`. T386'nın altı müdahalesi (`OrderCorrections.LockAsync`, `OrderTransitions.LockItemAsync`), geçiş biriminin gövdesi ve geç teslim feshi sürümsüz yönetici isteğini `InvalidInput` ile reddeder, hiçbir şey yazmaz; müşterinin kalem işlemleri ayrı yoldan sürümsüz geçer. Tarama (`ShownVersionSourceScan`) boş sürümde karşılaştırmayı atlayan biçimleri de yakalar (yeni kural `SkippedComparison`, ihlal örneğine 10 satır). Yeni testler: altı müdahale × sürümsüz / eski / güncel sürüm, sürümsüz yönetici geçişi, tarama biçimleri. `09 §10.2 m.3`'e cümle. Testler: Unit 5786/5786, Integration Ordering + Concurrency 638/638, Administration 69/69, Payment 14/14; derleme 0 uyarı, biçim temiz. K1 / D-961 açık; yeni backlog kalemi yok.

## 2026-11-10 — T364: Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler (doğrulama tur 1: FAIL)

2026-11-10 — T364 doğrulama tur 1 ✗ FAIL (#511, head `b5c6c4e4`; dal CI 42739621625 yeşil): kriter 4 / KK4'te S1 KRİTİK bulgu B1: T386'nın altı yönetici müdahalesi (`RemoveItemRequest`, `CorrectShipmentRequest`, `CorrectDeliveryDateRequest`, `CorrectReturnArrivalDateRequest`, `CorrectAddressRequest`, `CorrectGuestEmailRequest`) `int? ExpectedVersion` alır ve boş sürümde karşılaştırma atlanır (sondaj: sürümsüz kalem çıkarma değişmiş siparişte uygulandı). Öteki kriterler ✓; K1 → D-961. KP-76 satırları BEKLEMEDE kaldı.

## 2026-11-10 — T364 (Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler) yapım tamam, doğrulama bekliyor

2026-11-10 — T364 yapım (PR #511; iki işçi; ilki bağlam sınırında bölündü). Sistemin kendiliğinden geçişinin reddi uygulama kaydına yazılıyor (K-1559; `02 §3.31.2`, `05 §9.1.3`): yeni olay `AutomaticTransitionRefused`, tek yazıcı `Ordering/AutomaticTransitionLog` (kod + siparişin iç kimliği, bildirim yok); geçiş biriminin sistem aktörlü reddi, artık uymayan kart ödemesi ve kart iadesi sonucu, zamanlayıcının vadesi geçmemiş bulduğu sipariş. Yöneticinin sürüm karşılaştırması her yerde `ShownVersion.Matches`; yeni kaynak taraması `ShownVersionSourceScan` + ihlal örneği (`09 §10.2 m.3`). Eşzamanlı testler: iki yönetici aynı ödendi işareti / havale geri ödemesi / kalem çıkarma → yalnız biri; sistem ve müşteri reddi, kilit kalmaz. `origin/main` `206f918c` birleştirildi. Testler: Unit 5762/5762 (Release), Integration 3444/3444 (ad alanı ad alanı TAM), derleme 0 uyarı, biçim temiz. Backlog kalemi yok.

## 2026-11-10 — T373 (İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal) doğrulama tur 2 ✓ PASS

2026-11-10 — T373 doğrulama tur 2 ✓ PASS (head `ef958abf`, PR #507; `origin/main` `338279ea` dal tabanı, birleştirme gerekmedi; kapsam TAM). Kriter 1–10 ve KK1–KK10 ✓; B1 kapandı — kapanışı açan geç teslim alma kupon hakkını bir kez döndürür (yapımın iki testi + doğrulayıcının üç geçici çok adetli sınaması: kısmi teslim alma → kapanış → geç mal; iki adetlik kapanış → bir adet → hak dönmez → ikinci adet → döner; yeniden kapanışta hak dönmez). Dal CI 42707150031 yeşil; yerelde Integration 568 + 2813 = 3381/3381. Bulgu 0. K1 dal CI 42699862671'de (head `0d7d232f`) `StartupSettingsCheckTests` `ObjectDisposedException` ile kararsız → D-919 (F3 gate). `12`: VAL-A220 `PASS` kaldı, öteki satırlar `BEKLEMEDE` (uç / tarayıcı / sahte gönderim).

## 2026-11-10 — T373 düzeltme 1 tamam, doğrulama tur 2 bekliyor

2026-11-10 — T373 düzeltme 1: **B1** — "mal dönmedi" kapanışını geç gelen malın teslim alması açınca kupon hakkı siparişin son adediyle bir kez döner: `UnitFactsAsync(includeUnsaved: true)` kapanışı bağlamdan okur (aynı işlemdeki `ReopenedAt` görülür), `IsCouponReturnDueAsync` "önce" hâlini kayıpla bitmiş sayar (tek ev, kopya yok). K1 → D-915 açık kalır (F3 gate). Yeni Integration testleri 2 (kapanışlı ve kapanışsız kupon dönüşü; çift dönüş yok). Derleme 0/0, biçim temiz, Unit 5448 + Consistency 291, Integration Ordering 568/568, Payment + Concurrency + Disposal 101/101, Model 1404/1404.

## 2026-11-10 — T373 (İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal) doğrulama tur 1 ✗ FAIL

2026-11-10 — T373 doğrulama tur 1 ✗ FAIL (head `0d7d232f`, PR #507; `origin/main` `338279ea` dal tabanı, birleştirme gerekmedi). Kriter 1–10 ve KK1–KK10 ✓ (modül düzeyi); `09 §25` doküman uyumu hayır → FAIL. **B1 ORTA S1** (`06 §4.14`, §4.28; `02 §7.4.8`; `03 §1.4.4.6`): "mal dönmedi" kapanışını geç gelen malın teslim alması açınca kupon hakkı dönmüyor — `IsCouponReturnDueAsync` aynı işlemde yazılan `ReopenedAt`'i görmüyor, "önce" hâli kapanışla bitmiş sayılıyor. K1 → D-915 (ret açık kapanışın adetlerini reddedemez; F3 gate), K2 kapandı (D-911). `12` VAL-A220 `PASS`, öteki T373 satırları `BEKLEMEDE` (uç yok). Testler: Unit 5448 + Consistency 291, Integration tam 3379/3379; dal CI 42699862671 koşuyordu.

## 2026-11-10 — T382: Panel ana sayfası verisi (doğrulama tur 3: PASS)

2026-11-10 — T382 doğrulama tur 3 ✓ PASS (#510, head `e915b0a8`; dal CI 42737094455 yeşil): B3 düzeldi — kalemine bağlı açık IBAN isteği olan iade (ayıp çözümünün havale iadesi, dosyada IBAN yok) "IBAN bekleniyor"dadır ve sayaçta sayılmaz (`02 §10.6.1`; K-686); kabul kriterleri 1–9 ve KK1–KK9 birleşik ağaçta ✓. Unit 5770/5770, Integration (`PanelHomeTests`, `SalesSummaryTests`, `SitemapSourceTests`) 31/31. Task satırları `BEKLEMEDE` kaldı (uç ve ekran T521). Bulgu 0, K-notu 0.

## 2026-11-10 — T382: Panel ana sayfası verisi (düzeltme 2 tamam, doğrulama tur 3 bekliyor)

2026-11-10 — T382 düzeltme 2 tamamlandı (#510), doğrulama tur 3 bekliyor: B3 — "IBAN bekleniyor" artık iadenin kendisine ya da iadenin kalemine (`RefundId == null && OrderItemId == refund.OrderItemId`) bağlı açık IBAN isteğidir (`02 §10.6.1`; K-686); böyle iade "iade ve geri ödeme bekleyen" sayacına girmez, IBAN girilince sayılır, gönderilince düşer. Kural tek yerde (`AfterSalesModule.FindRefundsAwaitingIbanAsync`; `OpenRefundWork` kalemi taşır). Main (T386, T390) birleştirildi; `IAfterSalesModule.cs` çakışmasında iki tarafın üyeleri korundu. Sayacın bütün dışlama ve geçişleri raporda tek tabloda, testleriyle.

## 2026-11-10 — T382: Panel ana sayfası verisi (doğrulama tur 2: FAIL)

2026-11-10 — T382 doğrulama tur 2 ✗ FAIL (#510, head `be29b223`): B1 ve B2 düzeldi; kriter 4 / KK4'te yeni S1 bulgu B3: dosyada IBAN yokken ayıp çözümünün havale iadesi kalemin IBAN isteğini `RefundId` olmadan açar ("IBAN bekleniyor", K-686), `FindRefundsAwaitingIbanAsync` yalnız `RefundId`'ye baktığı için iade sayaçta sayılır (sondaj: beklenen 0, gelen 1). İkinci iterasyon FAIL — `12 §8.5`. Birleştirme notu: main (T386) ile `IAfterSalesModule.cs` kod çakışması, birleştirme geri alındı.

## 2026-11-10 — T382: Panel ana sayfası verisi (düzeltme 1 tamam, doğrulama tur 2 bekliyor)

2026-11-10 — T382 düzeltme 1 tamamlandı (#510), doğrulama tur 2 bekliyor: B1 — kart iadesi gerçekleşmeyip havale yolu açılan iade ("IBAN bekleniyor") artık "iade ve geri ödeme bekleyen" sayacına girmez (`AfterSalesModule.FindRefundsAwaitingIbanAsync` açık IBAN isteği olan iadeyi düşürür; kural tek evden, IBAN isteği okuması). B2 — satırı doğmuş ama gönderilmemiş havale iadesi (karttan havaleye dönen, ayıp çözümünün) işlenecek geri ödeme sayılır (`PaymentModule.ListOpenRefundWorkAsync`, eski `ListOrdersWithOpenRefundWorkAsync`); gönderilince düşer. D-927 kapandı. Öteki dışlama ve geçişler (mal dönmedi, ret, yeniden açılma, iki tür iş tek sipariş) aynı gözle tarandı, değişmedi.

## 2026-11-10 — T382: Panel ana sayfası verisi (doğrulama tur 1: FAIL)

2026-11-10 — T382 doğrulama tur 1 ✗ FAIL (#510, head `595d5d5f`, dal CI 42728221524 yeşil): kriter 4 / KK4'te iki S1 bulgu. B1: kart iadesi gerçekleşmeyip havale yolu açılan ("IBAN bekleniyor") sipariş "iade ve geri ödeme bekleyen" sayacında kalıyor. B2: satırı doğmuş ama gönderilmemiş havale iadesi (karttan havaleye dönen, ayıp çözümünün) IBAN girildikten sonra sayılmıyor. Öteki sekiz kriter ✓; Unit 5755/5755, Integration PanelHomeTests + SalesSummaryTests 26/26. Merge yok; düzeltme bekliyor.

## 2026-11-10 — T382: Panel ana sayfası verisi (yapım, PR açıldı)

2026-11-10 — T382 yapımı tamamlandı, doğrulama bekliyor: Administration'da `IPanelHome` (altı bekleyen iş sayacı, kanal ve firma bildirimi uyarıları, kurulum kontrol listesi; her çağrıda yeniden hesaplanır, hiçbir şey saklanmaz); Ordering, AfterSales, Payment, CorporateContent ve Notifications yüzeylerine okumalar (siparişler sayfa sayfa, "mal bekleniyor" kümesi kayıt sayfalarıyla okunur). "Kargoya verilecek" Teslim edilemedi siparişlerini koşulsuz, Hazırlanıyor'dakileri açık fiziksel kalemle sayar; "mal dönmedi" kapanışı ve iade reddi düşürür, yeniden açılan geri gelir; "IBAN bekleniyor" sayılmaz. D-601 kapandı. Şema değişikliği yok. Unit 5755, Integration PanelHomeTests 18/18; ⚠ yerel tam Integration koşuda Web/Logging "HeartbeatRecord row is missing" kırıkları `origin/main`'de de var (CI yeşil). Rapor: `Docs/TASK_REPORTS/T382_REPORT.md`.

## 2026-11-10 — T385 (Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti) doğrulama tur 2 ✓ PASS

2026-11-10 — T385 doğrulama tur 2 ✓ PASS (head `aac1b8b4`, PR #512). Kriter 1–9 ve KK1–KK9 ✓; tur 1'in B1'i kapandı (`ClaimHasMarkAsync` yalnız `DefectClaim`; cayma/fesihte işaret yalnız sipariş satırında). Yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 5789/5789, betik 4/4, Integration Outages+Ordering 666/666 ve CorporateContent+Model+Notifications 1589/1589; dal CI 42758342082 doğrulama anında Integration dışında yeşil, Integration koşuyordu. `12` `Durum` hücreleri değişmedi (VAL-A333 PASS; A332, A334–A336 BEKLEMEDE). Açık kalemler: D-974 (doc-drift, faz kapısı), D-975 K3 payı (D-968).

## 2026-11-10 — T385 düzeltme 1 (B1, K2) tamamlandı, doğrulama bekliyor

2026-11-10 — T385 düzeltme 1 tamamlandı, doğrulama tur 2 bekliyor (PR #512): **B1** kapandı — `DispatchMarks.ClaimHasMarkAsync` ve `INotificationsModule.ClaimHasUndeliveredMarkAsync` B-9 işaretini yalnız ayıp talebinin (`DefectClaim`) satırında verir, cayma beyanı ve gecikme feshinde yalnız sipariş satırında görünür (`02 §9.1.6`, `03 §1.7.3.1`, `06 §4.58`); testler `ADefectClaimsB9_ShowsTheMarkOnTheClaimToo_…` ve `AWithdrawalsOrALateTerminationsB9_ShowsTheMarkOnTheOrdersRowOnly_…` (Withdrawal, LateDeliveryTermination); **K2 / D-975 K2 payı** kapandı — `TheSchedulerSource_FindsNoDueRowWithoutAGateway_AndTheDueRowWithOne` `DispatchRetrySource`i çağırır (D-975 kalan payı K3, D-968 ekranına); D-974 (K1) dokunulmadı. Yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 5789/5789, Integration Outages 53/53, Ordering 613/613 (`DispatchRetryAndMarkTests` 34/34).

## 2026-11-10 — T385 (Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti) doğrulama tur 1 ✗ FAIL

2026-11-10 — T385 doğrulama tur 1 ✗ FAIL (head `3de1e988`, PR #512). Yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 5789/5789, betik testleri 4/4, Integration Outages+Ordering 663/663 ve CorporateContent+Model+Notifications 1589/1589 (`DispatchRetryAndMarkTests` dahil); dal CI 42755118412 doğrulama anında Lint/Build/Unit/Contract/Migration/Paketleme yeşil, Integration koşuyordu. Kriter 1, 3–9 ✓; **kriter 2 ~ Kısmi** → **B1 DÜŞÜK S1:** `DispatchMarks.ClaimHasMarkAsync` (ve `INotificationsModule.ClaimHasUndeliveredMarkAsync`) B-9 işaretini ayıp talebinin yanında cayma beyanının ve gecikme feshinin satırında da gösteriyor; `02 §9.1.6`, `03 §1.7.3.1`, `06 §4.58` ("Ayıp talebinin satırı": `RelatedRecordType` `DefectClaim`) ve `04` 2.5.3.1 yalnız ayıp talebini sayar; KK2'nin talep satırı testi (`AClaimsB9_ShowsTheMarkOnTheClaimToo_…`) cayma (`Withdrawal`) ile kurulu, ayıp talebi sınanmıyor. `12`: `VAL-A333` PASS; `VAL-A334`, `A335`, `A336` BEKLEMEDE (tarayıcı gözlemi ve panel çağıranı T531/D-967, D-968); `VAL-A332` T385'in kriteri değil. K-notları D-974, D-975 parça dosyasında.

## 2026-11-10 — T385: Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti (yapım tamamlandı, doğrulama bekliyor)

2026-11-10 — T385 (Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti) yapım tamamlandı, doğrulama bekliyor (dal `task/T385-gonderim-yeniden-deneme`; kriter 1–9, KK1–KK9 öz denetimde ✓; `DispatchSender` kuyruktaki B-1…B-16, F-1…F-6 ve hesap e-postası satırlarını gönderir, alıcı başına sınırı gövdeden önce sorar; `DispatchRetrySource` zamanlayıcı kaynağıdır (5 dk, 30 dk, 2 saat, ilk denemeden sayılır); `OrderNoticeContexts` gövde bağlamını siparişten kurar (D-232, D-293 yerinde kapandı); `DispatchMarks` işareti grubun son satırından okur, kalıcı geri dönmede ve sınır reddinde de düşer, işlem izine yazmaz; `DispatchResending` panelden yeniden gönderimi kuyruğa yazar (B-15 eski adrese) ve izine `ResendEmail` işler; `OrderOperations.EmailNotDelivered` gerçek değeri okur; Unit 5773, Integration (tüm ad alanları) yeşil; D-166, D-232, D-233, D-293 yerinde kapandı, D-97, D-99, D-110 K2, D-526 T385 payı kapandı; yeni kalemler D-966–D-969 parça dosyasında; ⚠: firma iletişimi güncel ayardan okunur, B-1 dışı gövde gönderim anındaki siparişten kurulur, `LinkNotValid` kapanışı için `06 §4.58` notu faz kapısında).

## 2026-11-10 — T386 (Sipariş müdahaleleri: kalem adedi azaltma ve çıkarma, kargo bilgisinin ve teslim tarihinin düzeltilmesi, iç not) doğrulama tur 2 ✓ PASS

2026-11-10 — T386 doğrulama tur 2 ✓ PASS (head `a2045292`, PR #509; `origin/main` `29d3ce6f` dal tabanı, birleştirme gerekmedi). Kapsam tam: kriter 1–10, KK1–KK10, `09 §25`, güvenlik ✓. B1 kapandı (iç not `09 §10.9`: `FOR SHARE` paylaşımlı kilit, gösterilen metne koşullu yazım, `DisplayedValueConflict`, `23505` yeniden yürütme; madde 5 (a)–(h) testli); K1, K2 kapandı. Bulgu 0, K-notu 0. Dal CI 42729406621 yeşil; yerelde Unit 5754/5754, Integration `OrderCorrectionTests` 51/51, `DataExportTests` + `LateDeliveryAndFirmCancellationTests` 57/57. `12` satırları (VAL-C057, C058, A240, A241, A247, A251, D028, A254, A255) `BEKLEMEDE` kalır; `12` değişmedi.

## 2026-11-10 — T386 düzeltme 1 (tur 1 FAIL → B1, K1, K2)

2026-11-10 — T386 düzeltme 1 (PR #509). **B1:** iç not `09 §10.9`'a getirildi — `OrderLocks.LockSharedAsync` (`FOR SHARE`, iletişim e-postası boşsa `OperationNotAvailable`), `SaveInternalNoteRequest.OriginalText`, gösterilen metne koşullu `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (`Ordering/SaveInternalNote.cs`, toplu yazım taramasında tek yer), `SaveInternalNoteResult` (`DisplayedValueConflict` + güncel not), eşzamanlı ilk yazım `RetryOnUniqueIndexes` ile yeniden yürütülür; özel kilit kalktı; madde 5 (a)–(e) iki gerçek işlemli Integration testleri. **K1:** D-884'e T386 payı (2, 3, 4, 6; kalan 5, 7, 8). **K2:** KK9 testi geri ödeme süresi başlangıcını iddia eder. D-942 (T373 testinde kupon kodu çakışması, F3 kapısı). Derleme 0/0, format temiz, Unit 5754/5754, Integration `OrderCorrectionTests` 51/51, Ordering 593/594 (1 ⚠ D-942, sınıf ayrı 63/63), Administration 46/46, Concurrency 26/26, Disposal 61/61.

## 2026-11-10 — T386 (Sipariş müdahaleleri: kalem adedi azaltma ve çıkarma, kargo bilgisinin ve teslim tarihinin düzeltilmesi, iç not) doğrulama tur 1 ✗ FAIL

2026-11-10 — T386 doğrulama tur 1 ✗ FAIL (head `3a5b0673`, PR #509; `origin/main` `29d3ce6f` dal tabanı, birleştirme gerekmedi). Kriter 1–10 ve KK1–KK10 ✓ (modül düzeyi); `09 §25` "iç notun kilidi yerinde mi (§10.9)" hayır → FAIL. **B1 ORTA S1** (`09 §10.9` madde 1–3, 5; `06 §4.20`; K-1051, K-1095): `SaveInternalNoteAsync` paylaşımlı kilit (`OrderLocks.LockSharedAsync`, `FOR SHARE`) yerine özel `FOR UPDATE` kilidi alıyor, gösterilen metinle (`OriginalText`) koşullu yazım ve `DisplayedValueConflict` yok — iki yöneticinin notu sessizce birbirinin üzerine yazılıyor; §10.9 madde 5'in (a)–(e) testleri yok. K1 (D-884'e T386 payı yazılmadı) ve K2 (KK9 geri ödeme süresinin düzeltilmiş ulaşma tarihinden başladığını gösteren test iddiası yok) düzeltme turunda bu PR'da kapanır. Dal CI 42719787280 yeşil; yerelde derleme 0/0, format temiz, Unit 5753/5753, betik testleri 4/4, Integration `OrderCorrectionTests` 46/46, `DataExportTests` + `LateDeliveryAndFirmCancellationTests` 57/57.

## 2026-11-10 — T386 yapım tamam, doğrulama bekliyor

2026-11-10 — T386 (dal `task/T386-siparis-mudahaleleri`): yöneticinin kalem çıkarma/adet azaltma (iptal rejimi, sebepsiz, B-11), kargo bilgisi düzeltmesi (B-12), sipariş düzeyi teslim tarihi ve iade ulaşma tarihi düzeltmesi (bildirimsiz, izde eski/yeni tarih, beyan gününde sıra sorusu) ve iç not (müşteriye görünmez, iz metinsiz). D-848 kapandı (dışa aktarma çıkarmayı sayar, testle); D-309 F3 kapısına. Unit 5753, Integration tam koşu yeşil (`OrderCorrectionTests` 46).

## 2026-11-10 — T390 (Uçtan uca dal akış testleri (servis düzeyi): karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme) doğrulama tur 2 ✓ PASS

2026-11-10 — T390 doğrulama tur 2 ✓ PASS (head `3ea4fb27`, PR #508; `origin/main` `29d3ce6f` dal tabanı, birleştirme gerekmedi; kapsam TAM). Kriter 1–10 ve KK1–KK10 ✓; B1 kapandı — e-posta düzeltmesi testinde müşterinin IBAN'lı iptali düzenekte var, düzeltmeden sonra IBAN satırı (kimlik, metin, giriş ve silinme anı) ve iptal bağı aynı. Dal CI 42727820308 success. Yerel: derleme 0 uyarı, biçim üç proje temiz, Unit 5739/5739, betik 4/4, Integration dokunulan üç sınıf 20/20. `12`'de task satırı yok; bulgu ve K-notu yok.

## 2026-11-10 — T390 düzeltme 1 tamam, doğrulama tur 2 bekliyor

2026-11-10 — T390 düzeltme 1: **B1** — e-posta düzeltmesi testinde müşteri, düzeltmeden önce iki adetli kalemin birini sipariş sayfasından IBAN'ıyla iptal eder (`CancelItemAsync`, uygulamanın kendi yolu; sipariş Hazırlanıyor + Ödendi kalır); düzenek IBAN satırının tek ve girilen metin olduğunu, iptale bağlı olduğunu iddia eder, düzeltmeden sonra satırın kimliği, metni, giriş anı ve silinme anı aynı kalır. Ödeme onayı ve söz bitişi dolu, iz ve kalemler boş değil iddiaları da eklendi. Öteki anlatıların "değişmez/olmaz" iddiaları tarandı; korunan değerler düzenekte var, başka boş karşılaştırma yok. Üretim kodu değişmedi. Derleme 0/0, biçim üç proje temiz, Integration dokunulan üç sınıf 20/20, Unit 5739/5739.

## 2026-11-10 — T390 (Uçtan uca dal akış testleri (servis düzeyi): karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme) doğrulama tur 1 ✗ FAIL

2026-11-10 — T390 doğrulama tur 1 ✗ FAIL (head `d5838e2b`, PR #508; `origin/main` `29d3ce6f` dal tabanı, birleştirme gerekmedi). Kriter 1–8 ve 10, KK1–KK8 ve KK10 ✓; kriter 9 / KK9 `~ Kısmi`. **B1 DÜŞÜK S1** (`03 §2.10.5` adım 4; `02 §10.4.11`): e-posta düzeltmesi testinde "IBAN değişmez" kanıtı boş — siparişte müşteri IBAN'ı yok, `refund_ibans` sayısı 0 = 0 karşılaştırılıyor; düzeltme IBAN'ı silse de test geçer. Yerel: derleme 0 uyarı, biçim temiz, Unit 5739/5739, betik 4/4, Integration dokunulan üç sınıf 20/20. `12`'de task satırı yok; K-notu yok.

## 2026-11-10 — T390 (Uçtan uca dal akış testleri (servis düzeyi): karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme) yapım tamam, doğrulama bekliyor

2026-11-10 — T390 yapım: `03 §2.10.2`…§2.10.5'in dört dal anlatısı `BranchFlowTests` altında dört Integration testinde (IBAN'sız iade iki kollu: "mal dönmedi" kapanışı / Z-38), gerçek PostgreSQL, elle saat ve `FakePaymentGateway` ile; her adımda iki eksen, iz, bildirim ve kayıtlar okunur (KK1–KK10). Kart iadesi işleyicisi (T405) ve havale cayma iadesinin panel adımı testte taklit edilir. D-828 kapandı: ödeme bildirimi test sunucusu tek kurucuda (`PaymentNotificationHost`), üç sınıf onu kullanır. Üretim kodu değişmedi. Testler: Integration 3386/3386 (Ordering 573, Model 1404, geri kalan 1409), Unit 5739/5739, biçim temiz.

2026-11-09 — chore: `LivenessEndpointTests` kararlılığı. Süre testlerinin (`AssertAnswers503WithinTheLimitAsync`) kronometresi ilk isteğin soğuk maliyetini (uç noktanın, sürücünün ve kayıt yolunun ilk kullanımı, iş parçacığı havuzunun büyümesi) de ölçüyordu; yoğun CI'da 4,5 sn / 1 sn çıktı (main run 42558302344, PR #487 run 42560074567). Kronometreden önce ayrı bir sunucuda sağlıklı bir canlılık isteği koşturulur; canlılık ucu sınırı (ayar 1 sn, üst sınır 3 sn) ve üretim kodu değişmedi. D-391'in nedeni: sunucu kapanışı, takılı kalan bağlantı açılışının (bağlantı dizisi `Timeout` 15 sn) bitmesini bekliyor — iki test 61 sn sürer; D-391 açık kalır.

## 2026-11-09 — T327 (Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler) doğrulama tur 1 PASS

2026-11-09 — T327 (Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler) doğrulama tur 1 ✓ PASS (head `9003ac2f`, `origin/main` `a504ff84` ile güncel; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; bulgu 0; K-notu 3 → D-696 (Z-38 konu kümesinin eksikleri, T370), D-697 (kayıttaki veri üyesi adları T369/T373; `VAL-D052`…`D055` ve `12 §12.6` kesinti satırlarının sahibi F3 gate-check); `12` `Durum` değişmedi; Unit 5065/5065, Integration 284/284 (T327 ve dokunduğu sınıflar), dal CI run 42525030881 yeşil).

## 2026-11-09 — T327 (Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler) yapım bitti (PR #480)

2026-11-09 — T327 (Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler) yapım bitti, ⏳ doğrulama bekliyor (dal `task/T327-otomatik-anlar`, PR #480; `origin/main` (T323 #474, T345 #477, T349 #476) birleştirildi). `03 §4.2`'nin 12 anı `AutomaticMoments` kaydında; Z-38 IBAN silme günü `RefundIbanDeletionDay` zamanlayıcı kaynağı (kilit, yeniden okuma, imha kaydı, başka bekleyen geri ödemede gün boşaltma); sipariş sayfası verisine `TransferPaymentEndsAt`. Kriter 1–8, KK1–KK8 öz denetimde ✓; Unit 5065, Integration bütün ad alanları yeşil (Model'de paralel koşuya bağlı tek `DROP DATABASE` başarısızlığı, sınıf tek başına 60/60). Şema yok. Backlog D-666, D-667, D-668 yeni; D-99'a T327 payı notu.

## 2026-11-09 — T329 (İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi)) doğrulama tur 1 ✓ PASS

2026-11-09 — T329 doğrulama tur 1 ✓ PASS (head `288f56d2` + `origin/main` (T365 #496) birleştirmesi, PR #499). Kriter 1–7 ve KK1–KK7 ✓, `09 §25` ve güvenlik taraması temiz, bulgu 0. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5203/5203 + tutarlılık 288/288, betik 4/4, Integration `MainFlowTests` 1/1 + `PaymentNotificationTests` 14/14 (birleşik ağaçta da 15/15), `LivenessEndpointTests` 16/16. Dal CI 42609814871 yalnız `4. Integration test`'te kırmızı: `LivenessEndpointTests.Get_WhenTheServerIsUnreachable_Returns503WithinTheLimit` zamanlama (5.3 sn / 1 sn; dal o alana dokunmaz); kayıt commit'inin CI'ı merge koşuludur. `12`'de task satırı yok (kaynak birimlerinde KP yok). Yeni backlog D-840 (ödeme başlatma adımı, T401) ve D-841 (`LivenessEndpointTests` kararsızlığı, F3 gate).

## 2026-11-09 — T329 (İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi)) yapım tamamlandı

2026-11-09 — T329 yapım tamamlandı, doğrulama bekliyor (dal `task/T329-ilk-uctan-uca`; kriter 1–7, KK1–KK7 öz denetimde ✓). `02 §2.2`'nin sekiz adımı tek Integration testinde (`MainFlowTests`) servis katmanından geçer: arama ve ürün sayfasıyla varyant seçimi, sepete ekleme (stok ayrılmaz), ödeme adımı (adres, kupon, kart), onay adımı (iki kutu), siparişin doğuşu (numara, donma, stok ve kupon ayırma, B-1 ve F-1), `FakePaymentGateway`'in doğruladığı başarı bildiriminin uygulamanın kendi ucunda kaydı ve iş kuyruğunda Ö1 + S1 (B-4, stok kesin düşer, kupon hakkı kullanılır, sepet satırı çıkar), yöneticinin S5'i (B-5) ve S6'sı (bildirim yok). Her adımda iki eksen, sürüm, yan etkiler ve işlem izi satırları onaylanır; yöneticinin elle adımları `OrderLines`'ın kart hattı satırından türetilir (iki). Üretim kodu değişmedi. Yeni backlog D-828 (`DEFERRED_BACKLOG.d/T329.md`).

## 2026-11-09 — T348 (Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal) doğrulama tur 2 ✓ PASS

2026-11-09 — T348 doğrulama tur 2 ✓ PASS (head `b2dd4384` + `origin/main` `94cad6d5` (#490) birleştirmesi, PR #489; dal CI 42570842338 yeşil). Kriter 1–8 ve KK1–KK8 ✓; tur 1 bulguları kapandı: **B1** dijital ve hizmet kutu kaydı onayla aynı işlemde donuyor (`AMixedConfirmation_..._KK1`; D-732 ✅), **B2** teslimat alanı onayda yeniden okunuyor (`ADeliveryAreaThatNoLongerServes..._KK2`; D-494 ✅), **K4** kargoya verme süresi karşılaştırılıyor, **K3** devredilen kalemler kapatıldı ya da gerekçeyle ileri atıldı. Yerelde derleme `-warnaserror` 0/0, `dotnet format` 3 proje temiz, Unit 5417/5417, betik 4/4, Integration Ordering 390/390, `AddressTests` + `MembershipTests` 142/142, `FirmSettingsTests` 24/24. Matris: VAL-D016 FAIL → BEKLEMEDE (veri kaydı kanıtlı, uç yanıtı T509/T518); VAL-C001, D006, D008, D012 PASS kalır. K1 (gerekçe yorumsuz `!`, kod tabanında yaygın) → D-778 (F3 gate).

## 2026-11-09 — T348 düzeltme 1 (doğrulama tur 1 bulguları)

2026-11-09 — T348 düzeltme 1 (dal `task/T348-siparis-onayi`, PR #489). **B1:** onay dijital ve hizmet kutusunun kaydını (tür, gösterilen metin, kapsanan kalemler) `ConfirmationAcceptances.Records` ile siparişle aynı işlemde yazar (D-732 ✅). **B2:** onay teslimat alanını yeniden okur; fiziksel siparişte teslimat ili hizmet dışındaysa ya da teslimat adresi yoksa sipariş oluşmaz, güncel özet farkı söyler (`02 §6.2.3`; D-494 ✅). **K4:** özet karşılaştırması kargoya verme süresini de denetler. **K3:** T348'e yönlenmiş backlog kalemleri kapatıldı (D-465, D-570, D-621) ya da T348 payı kapanıp kalanı ileri atıldı (D-437 → T509/T518, D-453, D-483, D-563 → T509, D-626 → T509, D-705). Build `-warnaserror` temiz; Unit 5417/5417; Integration Ordering 389/389, Address+Membership+Cart+CartQuantity+SettingsTables 209/209. Yeniden doğrulama bekliyor.

## 2026-11-09 — T348 (Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal) doğrulama tur 1 ✗ FAIL

2026-11-09 — T348 doğrulama tur 1 ✗ FAIL (head `83000d3a` + `origin/main` `f7d2febe` (T371) birleştirmesi, PR #489; dal CI 42564653239 yeşil). Kriter 3–8 ve KK3–KK8 ✓; **kriter 1 / KK1 ~** ve **kriter 2 / KK2 ~**. **B1 KRİTİK S1** (`02 §3.17.1` → §3.23.3, §3.24.6; VAL-D016; D-732): onay dijital ve hizmet kutularının kaydını yazmıyor — `ConfirmationAcceptances.Records` üretimde hiç çağrılmıyor, `CheckboxAcceptance` satırı doğmuyor. **B2 ORTA S1** (`02 §3.17.2`, §6.2.3; D-494): teslimat illerinin onay anındaki değişimi siparişi durdurmuyor — onay teslimat alanını okumaz, teslimat adresinin ili denetlenmez. Yerelde derleme `-warnaserror` 0/0, biçim 3 proje temiz, Unit 5416/5416, betik 4/4, Integration Ordering 384 + `AddressTests`/`MembershipTests` 142 yeşil. `12`: VAL-C001, D006, D008, D012 → PASS; VAL-D016 → FAIL; öteki KP-8/11/14–17 satırları uç ya da tarayıcı kanıtı ister → BEKLEMEDE. K1 → D-769 (T509), K2 → D-770 (T353); K3 (T348'e yönlenmiş D-437, D-453, D-465, D-483, D-563, D-570, D-621, D-626, D-705, D-494, D-732 kapatılmadı) ve K4 (kargoya verme sözü özet karşılaştırmasında yok) düzeltme turunda bu PR'da kapatılır.

## 2026-11-09 — T348 yapım tamam, doğrulama bekliyor

2026-11-09 — T348 yapımı (dal `task/T348-siparis-onayi`, PR #489). Sipariş onayı tek işlem: sepet kilidi altında aynı sepetin ödenmemiş eski siparişi sistemce iptal edilir (birleşen misafir sepeti istisna), sepet yeniden değerlendirilir ve gösterilen özetle iki yönlü karşılaştırılır (her fark durdurur), onay adımı denetlenir; sonra numara, donma, ödeme süresi, stok/kontenjan/kupon ayırma ve B-1/F-1 aynı işlemde. Ret değerdir, işlem geri alınır. Müşteri listesinin görünürlük kuralı `OrderVisibility` (hiç ödenmemiş, sistemin dört gerekçeyle iptal ettiği sipariş müşteride yok, panelde var). Kabul kriterleri 1–8 ve KK1–KK8 ✓. Unit 5313/5313, Integration TAM 2897/2897; göç yok. ⚠: il kontrolü (T350), L-7/L-8 ve havale açık mı denetimi başka task'larda; sayfa/POST Pages F5'te.

## 2026-11-09 — T352 (Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz) doğrulama tur 1 ✓ PASS

2026-11-09 — T352 doğrulama tur 1 ✓ PASS (head `10da8ed7`, PR #491; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 1 → D-795; Unit 5439/5439, Integration dokunulan ad alanları ve sınıflar 754/754, dal CI 42582347076 yeşil; KP-15'in 10 satırı değişmedi — uç yanıtı/tarayıcı ister ya da başka task'ın kaynağıdır, VAL-C001 PASS kalır; `12_matris_denetle.py` Hata 0).

## 2026-11-09 — T352 (Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz) yapım tamamlandı

2026-11-09 — T352 yapım tamamlandı, doğrulama bekliyor (dal `task/T352-odeme`; kriter 1–8, KK1–KK8 öz denetimde ✓). Kart hattı bağlandı: `CardPaymentCondition` açılışta hesaplanan `ConditionalLine` tekilini okur ve Settings'in bağsız varsayılanının yerine Payment modülünün kaydıyla gelir (D-81 ve D-198'in T352 payı kapandı). `IPaymentMethods.ListOpenAsync` açık yöntemleri verir (yalnız kart ve havale); sipariş onayı açık olmayan yöntemi `PaymentMethodUnavailable` ile reddeder, hiçbir şey doğmaz ve sepet durur. Kart verisi, taksit, 3D'yi atlayan anahtar ve ters ibraz için uygulamanın bütün adları taranır (her tarama ihlal örneğiyle). Yeni backlog D-785–D-787 (`DEFERRED_BACKLOG.d/T352.md`).

## 2026-11-09 — T353 (Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş) doğrulama tur 2 ✓ PASS

2026-11-09 — T353 doğrulama tur 2 ✓ PASS (head `3189dd97` + `origin/main` `d5676e75` birleştirmesi, PR #500; kapsam TAM: kriter 1–7, KK1–KK7, `09 §25` ve güvenlik taraması ✓; bulgu 0, yeni K yok; Unit 5559/5559, Integration dokunulan ve komşu sınıflar 99 + 257 geçti, dal CI 42625668294 yeşil; KP-15/46/59 satırları BEKLEMEDE kalır — uç/ekran kanıtı T511/T536; birleştirmede yalnız `DEFERRED_BACKLOG.d/T327.md` çakıştı, iki taraf korundu).

## 2026-11-09 — T353 (Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş) düzeltme 1 (main birleştirmesi)

2026-11-09 — T353 düzeltme 1: `origin/main` (`9d0d611c`, T377 #497) birleştirildi. L-8'in tek yolu main'in `OrderPlacementLimits.FindExceededAsync`'i (`AttemptLimitExceeded`); T353'ün `TransferCeilingReached` reddi, `IsTransferCeilingReachedAsync` ve `excluding` aşırı yüklemeleri kaldırıldı. Aynı sepetin onayda iptal edilen, henüz kaydedilmemiş siparişi `OpenUnpaidOrderLimit` sayımına girmez (bağlamın değiştirdiği girdiler düşülür; main testleri değişmedi). KK2 testi `AttemptLimitExceeded` + `L8` bekler. D-853 metni güncellendi. Unit 5550/5550; Integration Ordering 468, Payment 14, T377 limit 16, ayar/kapı 51 — hepsi yeşil.

## 2026-11-09 — T353 (Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş) doğrulama tur 1 ✓ PASS

2026-11-09 — T353 doğrulama tur 1 ✓ PASS (head `9204f00d`, PR #500; kriter 1–7, KK1–KK7, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 2 → D-852 (T530), D-853 (T509); Unit 5526/5526, Integration dokunulan sınıf ve ad alanları 1194/1194, dal CI 42616258907 yeşil; KP-15/46/59 satırları değişmedi — uç yanıtı/tarayıcı ister; `12_matris_denetle.py` Hata 0). Birleştirme notu: `origin/main` T377 ile `OpenUnpaidOrderLimit.cs` ve `OrderConfirmation.cs`'te kod çakışması (iki L-8 reddi); birleştirme yapılmadı.

## 2026-11-09 — T353 (Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş) yapım tamamlandı

2026-11-09 — T353 yapım tamamlandı, doğrulama bekliyor (dal `task/T353-havale-eft`; kriter 1–7, KK1–KK7 öz denetimde ✓). Ödeme yöntemleri bölümü (`IPaymentMethodSettings`): açıkken IBAN zorunlu, tek IBAN kuralı, iz satırı, IBAN girilince/değişince bütün yöneticilere F-5. Havale hattı Settings'in kaydıyla bağlandı (`BankTransferCondition`; D-198'in T353 payı). Havale onayında L-8 doluysa `TransferCeilingReached` (kart açık), B-2 kuyruğa girer (D-770). Kapanan: D-151, D-770, D-787, D-795; D-668 ve D-566 T511'e. Yeni backlog D-833 (`DEFERRED_BACKLOG.d/T353.md`). Unit 5526/5526, Integration 3211/3211 (birleşik ağaç).

## 2026-11-09 — T355 (Sipariş anında donan değerler) doğrulama tur 1 PASS

2026-11-09 — T355 (Sipariş anında donan değerler) doğrulama tur 1 ✓ PASS (head `e958219b`, `origin/main` ile güncel; kriter 1–6, KK1–KK6, `09 §25` ve güvenlik taraması ✓; bulgu 0; K-notu 0; `12` satırları `VAL-D010`…`D016` `BEKLEMEDE` (uç ve onay akışı T348, T356); Unit 5134/5134, Integration `FrozenValuesTests` 6/6 + `DigitalDeliveryTests` 21/21, dal CI 42533334898 yeşil).

## 2026-11-09 — T355 (Sipariş anında donan değerler) yapım bitti

T355 (Sipariş anında donan değerler) yapımı bitti; PR #484 açıldı, doğrulama bekliyor. Katalog okuması `ICatalogModule.ListOrderItemSourcesAsync` (`OrderItemSourceReader`: ad, seçenekler, görsel, fiyat, KDV, yürüyen indirim ve referans fiyatı, tipe bağlı istisna, ifa süresi, o anki dosya); donma birimi `Ordering/OrderFreeze.cs` (`FreezeItem` kalemin düz değerleri ve P-8 indirme hakkı, `FreezeLegalIdentity` firma kimliği tipe göre + üç sürüm + havale IBAN'ı, `PartiesAtConfirmation`); belgelerin onay anı tarafları `IOrderingModule.GetConfirmationPartiesAsync`. Dosya kuralının tek evi `DigitalFileReader.CurrentFileAsync`. Kriter 1–6 ve KK1–KK6 testli (Unit `OrderFreezeTests`, Integration `FrozenValuesTests`; KK2 düğmelerin donmuş değeri okuduğunu da sınar). D-476 kapandı, D-437 T348'e ileri atıldı, D-508 hedefinden T355 çıktı; yeni kalem D-705 (`DEFERRED_BACKLOG.d/T355.md`).

## 2026-11-09 — T356 (Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi) doğrulama tur 2 ✓ PASS

2026-11-09 — T356 doğrulama tur 2 ✓ PASS (head `166e750b`, PR #486). Dal CI 42554853076 bütün işler yeşil. B1 kapandı: `PlaceholderFiller.Inert` her ardışık açılış ayracını ayırır, üç ve dört ayraçlı değer `{{` bırakmaz (D-268 (4)). Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5306/5306, betik 4/4, Integration Ordering 369/369, Settings/LegalText sınıfları 90/90. Kriter 1–9 ve KK1–KK9 ✓ (6 ve 9 modül payı; ekran D-729, D-730). Yeni bulgu ve K-notu yok; D-743 açık. `12` satırları `BEKLEMEDE` kaldı.

## 2026-11-09 — T356 (Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi) düzeltme 1

T356 düzeltme 1 (PR #486): B1 kapandı. `PlaceholderFiller.Inert` her ardışık açılış ayracını ayırır; üç ve dört ayraçlı değer sonuçta `{{` bırakmaz, ikinci doldurmada genişlemez (`09 §9.3` madde 3; D-268 (4) kanıtı güncel). Testlere üç ve dört ayraçlı örnekler eklendi. Yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 5306/5306, Integration Ordering 369/369, LegalText 16/16. Doğrulama tur 2 bekliyor.

## 2026-11-09 — T356 (Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi) doğrulama tur 1 ✗ FAIL

2026-11-09 — T356 doğrulama tur 1 ✗ FAIL (head `0663fc3d`, PR #486). Dal CI 42550296580 bütün işler yeşil. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5302/5302, betik 4/4, Integration Ordering 369/369. Kriter 1–9 ve KK1–KK9 ✓ (6 ve 9 modül düzeyinde; ekran D-729, D-730). B1 (DÜŞÜK, S1): `PlaceholderFiller.Inert` tek geçişte çalıştığı için `{{{Ad}}` bir `{{Ad}}` bırakır; ürün adı ya da firma değeri ikinci doldurmada müşterinin kişisel değerine açılır (`09 §9.3` madde 3; D-268 (4) tam kapanmadı). K1 → D-743 (cayma özeti fiziksel kalemsiz sepette fiziksel cümleleri yazıyor). `12` satırları `BEKLEMEDE` kaldı.

## 2026-11-09 — T356 (Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi) yapım bitti

T356 (Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi) yapımı bitti; PR açıldı, doğrulama bekliyor. `IConfirmationStep` iki sabit kutuyu, dijital ve hizmet kutusunu sepete göre ister, düğmede ödeme yükümlülüğünü yazar, sürüm artışında siparişi durdurur; `IOrderDocuments` özeti ve formu üretir (donmuş ve kişisel değerler iki aşamalı); kutu kaydı kutuyu, metni ve kalemleri taşır; form dört ek bilgiyi, sabit iade ve istisnayı, ifa süresini, meslek odasını ve "sipariş nasıl kurulur" metnini içerir; `ProcessGuide` işlem rehberinin sabit metnidir. Belge klasörü e-posta kaçış taramasına eklendi (D-167 kapandı, D-268 1–4 kapandı). Ekran ve altbilgi bileşeni T509/T540'tadır (D-729, D-730); `09 §9.3` belirteç kümesi kararı bekliyor (D-731). Unit 5302 (bir test kimlik sırası hatası düzeltildi), Integration Ordering 369/369.

## 2026-11-09 — T357 (Fatura verisi ve dışa aktarmada donmuş kimlik) doğrulama tur 1 PASS

2026-11-09 — T357 (Fatura verisi ve dışa aktarmada donmuş kimlik) doğrulama tur 1 ✓ PASS (head `5c08e626` + `origin/main` `e958c598` (T359 #479, T388 #483) birleştirmesi, çakışma yok; kriter 1–4, KK1–KK4, `09 §25` ve güvenlik taraması ✓; bulgu 0; K-notu 1 → D-736 (T384); KP-52 satırları T384'ün, `BEKLEMEDE` kaldı; Unit 5220/5220 birleştirme sonrası, Integration `Ordering` 360/360, `Commands` 59/59; dal CI 42542237326 success).

## 2026-11-09 — T357 (Fatura verisi ve dışa aktarmada donmuş kimlik) yapım bitti

T357 (Fatura verisi ve dışa aktarmada donmuş kimlik) yapımı bitti; PR #485 açıldı, doğrulama bekliyor. Fatura verisi okuması `IOrderingModule.ListInvoiceDataAsync` (`Ordering/InvoiceDataReader.cs`): kalemlerin donmuş tutarları, kargo ücreti ve donmuş KDV bölüşümü, fatura adresi, siparişin donmuş firma kimliği (panelin o anki kimliği değil), ödeme yöntemi ve onay anı, kalem tipine göre kargoya verme anı (fizikselde gönderim, hizmette tamamlama, dijitalde ödeme onayı), kargo şirketinin adı ve alan adı. Ödeme onayı ve gönderimin tek tanımı `OrderPeriods.PaymentApprovals` / `Shipments`. Ürün fatura kesmez, saklamaz: mimari tarama (`InvoiceSurfaceTests`) fatura kesen, yükleyen, ileten ya da e-Arşiv/e-Fatura bağlayan ad bulmaz. Yeni kalemler D-724 (T384 dosyaya yazım), D-725 (iki okumada düzeltilmiş gönderim) — `DEFERRED_BACKLOG.d/T357.md`.

## 2026-11-09 — T359 doğrulama tur 3 ✓ PASS

2026-11-09 — T359 doğrulama tur 3 ✓ PASS (head `29e187eb`, PR #479; `origin/main` `8772edbc` çakışmasız birleştirildi). Dal CI 42535094509 yeşil (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate). Yerelde build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 5140/5140 (birleşik ağaçta 5170/5170), betik testleri 4/4, Integration `CorporateContent` 150/150, `Catalog` 72/72. B2 kapandı: `ContentProductLinks.SaveAsync` var olmayan ürünü atlar, sıra korunur. Sekiz kabul kriteri ✓ (servis düzeyi; çizim D-671/672/674). `12` satırları `BEKLEMEDE` kaldı (sayfa/uç sahibi sonraki task'lar). K3 → D-720 (silme ile kayıt arasındaki yarış).

## 2026-11-09 — T359 düzeltme 2 (B2: ürün bağı kümesinde silinmiş ürün atlanır)

2026-11-09 — T359 düzeltme 2: `ContentProductLinks.SaveAsync` listede var olmayan ürünü atlar, kalan bağları seçim sırasıyla yazar, hata değildir (`06 §4.53`, `07 §9.13.2`); `ProductNotFound` sonucu kalktı; yinelenen ürün ve bilinmeyen kayıt reddi korundu. `ContentRelationsTests`: reddi kilitleyen test daraltıldı, silinmiş ürünün atlandığını ve sırayı sınayan test eklendi. Build `-warnaserror` temiz, format temiz, Unit 5140/5140, Integration `CorporateContent` 150/150, `Catalog` 72/72.

## 2026-11-09 — T359 doğrulama tur 2 ✗ FAIL

2026-11-09 — T359 doğrulama tur 2 ✗ FAIL (head `f77197f5`, PR #479; `origin/main` birleştirildi, yalnız STATUS tablo satırı çakıştı). Dal CI 42527884835 yeşil. Yerelde build `-warnaserror` temiz, format temiz, Unit 5132/5132, betik 4/4, Integration `CorporateContent` 148/148, `Catalog` 72/72. B1 kapandı. **B2 DÜŞÜK S1:** `ContentProductLinks.SaveAsync` var olmayan ürünü atlamak yerine bütün listeyi reddediyor (`06 §4.53`, `07 §9.13.2`: "atlanır, hata değildir"). K2 → D-712 (D-673 hedefi T360 birleşti, kalem açık). İki iterasyon doldu: eskalasyon.

## 2026-11-09 — T359 düzeltme 1 (B1: iki içerik listesinin sayfa sınırı testi)

2026-11-09 — T359 düzeltme 1: `ContentRelationsTests`'e hizmet ve referans listeleri için Integration sayfa sınırı testi eklendi (aynı `SortOrder`/`CreatedAt`, 34 yayında + 5 taslak; iki sayfa yürünür, yineleme/kayıp yok, `Id` artan, aşan `page` son sayfa). Üretim kodu değişmedi. Build `-warnaserror` temiz, format temiz, Unit 5076/5076, Integration `CorporateContent` 123/123, `Catalog` 72/72.

## 2026-11-09 — T359 (Kurumsal içerik: ürün bağı, elle sıralama, zorunlu alanlar, alan tavanları, kapsam dışı yetenekler) doğrulama tur 1 ✗ FAIL

2026-11-09 — T359 doğrulama tur 1 ✗ FAIL (head `5cdc79c4`, PR #479). Dal CI 42522009160: Lint, Build, Unit, Contract, Migration, Paketleme yeşil, Integration doğrulama anında koşuyordu. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5050/5050, betik testleri 4/4, Integration `CorporateContent` 121/121, `Catalog` 72/72, `CategoryTreeAndListsTests` + `BranchContentLinkAndContactRequestTablesTests` + `ListPagingTests` 87/87. Sekiz kabul kriteri servis düzeyinde ✓. **B1 DÜŞÜK S1:** yeni iki sayfalı liste (`ContentReaders.ListOfferingsAsync`, `ListPortfolioItemsAsync`) `09 §10.5` madde 6'nın zorunlu sayfa sınırı testini taşımıyor. K1 → D-687 (D-500 / D-671 tek konu).

## 2026-11-09 — T360 (Duyuru şeridi, boş kurulum, içerik yayın durumu, önizleme ve silme) doğrulama tur 1 ✓ PASS

2026-11-09 — T360 doğrulama tur 1 ✓ PASS (head `b7284315`, PR #481). Kriter 1–8, KK1–KK8, `09 §25`, güvenlik taraması, yerel testler (Unit 5055, birleştirme sonrası 5081; Integration CorporateContent 111, Branding+AppCache+CookiePolicy 54; betik testleri 4 dosya) ve dal CI 42525208897 ile ✓; bulgu yok. K1 → D-698 (şubenin iz satırı içerikçe sınanmıyor). VAL-A133, A146, C032, A147, C033, C034, A148…A151, C035, A161, C036, A162 `BEKLEMEDE` (uç/tarayıcı kanıtı sayfa ve panel task'larında: T534, T537). Kayıt commit'inden önce `origin/main` (`a504ff84`) birleştirildi, çakışma yok.

## 2026-11-09 — T360 (Duyuru şeridi, boş kurulum, içerik yayın durumu, önizleme ve silme) yapım tamamlandı ⏳

2026-11-09 — T360 yapım tamamlandı, doğrulama bekliyor (dal `task/T360-duyuru-icerik-yayin`). Duyuru yazımı (`CreateAnnouncementAsync` / `SaveAnnouncementAsync`, P-30 tavanı, bağlantı biçimi, bitiş ≥ başlangıç), duyuru okuyucusu (`GetAnnouncementAsync`; Türkiye gününe göre aralık, önbelleksiz), taslak önizleme okumaları (sayfa, SSS, şube; `IsDraft`), yayına alma ve kaldırmada `ChangePublishStatus` iz satırı (metin düzenlemesi ve silme iz yazmaz), boş kurulum testi. Yerel: Unit 5055 yeşil (Consistency 283), Integration 2864+ yeşil parçalı koşuda (CorporateContent 111, Model 1379 [bir yük zaman aşımı tek başına 42/42 yeşil], Ordering/Catalog/Payment/Outages 432, Web 594, öteki 414), `dotnet format` temiz, `-warnaserror` derleme temiz. D-571 ✅ kapandı; D-315 iz kısmı kapandı (adres sabitleme T363'e), D-317 T363'e ileri atıldı; yeni D-676–D-679.

## 2026-11-09 — T363 (Sayfa adresleri, arama motoru bilgileri, kapalı sayfalar ve paylaşım) doğrulama tur 1: ✓ PASS

2026-11-09 — T363 doğrulama tur 1 ✓ PASS (PR #482, head `4519fe2b`, dal CI 42528397096 yeşil). Kriter 1–7, KK1–KK7, `09 §25` ve güvenlik taraması ✓; yerelde derleme `-warnaserror` 0/0, biçim üç projede temiz, Unit 5105/5105, betik testleri 4/4, Integration dokunulan sınıflar 67/67 ve Catalog + CorporateContent + Storefront 172/172. Task satırları (`VAL-A023`, `VAL-D001`, `VAL-A134`, `VAL-A135`, `VAL-A373`…`VAL-A407`, `VAL-D056`…`VAL-D058`, `VAL-A625`, `VAL-A647`) uç yanıtı ister, vitrin sayfaları yok → `BEKLEMEDE` (PASS'ler T313'ten). K1: D-690…D-693 hedefi T549'dan sayfaları yazan task'lara çevrildi. Birleştirme notu: main (T360) ile kod çakışması, birleştirme yardımcıda.

## 2026-11-09 — T363 (Sayfa adresleri, arama motoru bilgileri, kapalı sayfalar ve paylaşım) yapım bitti

2026-11-09 — T363 yapım bitti (dal `task/T363-sayfa-adresleri-seo`, PR `T363: ...`). Adres ilk yayına kadar adla yeniden üretilir, ilk yayında `SlugFixedAt` ile sabitlenir (içerik kayıtları `ContentAuthoring`, ürün `ProductAddresses` + `Product.SetAddress`); `RecordPages` ürün, kategori, içerik ve kapalı ekran için arama verisini üretir (taslak ve arşiv `noindex`); tek "Paylaş" düğmesi `common.js` + `_ShareButton.cshtml`; yönlendirme, platform paylaşımı ve Slug atamasını kaynak taraması (`AddressSourceScan`) sınar. Unit 4822/4822 (+ Consistency 283/283), Integration `PageAddressTests` 10/10, CorporateContent + Storefront + Catalog ad alanları 172/172, `dotnet format` üç projede temiz. Backlog D-689…D-693; D-357 hedefi T549'a ileri atıldı, D-315'in adres sabitleme yarısı kapandı (iz kaydı T360'ta).

## 2026-11-09 — T365 (İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması) doğrulama tur 1 ✓ PASS

2026-11-09 — T365 doğrulama tur 1 ✓ PASS (head `69144a13`, PR #496; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 1 → D-824; Unit 5509/5509, Integration CorporateContent 185/185 ve BrandingTests + RecordDisposalTests 42/42, dal CI 42600283047 yeşil; KP-30, KP-34 ve KP-49'un T365'e bağlı satırları uç ve panel kanıtı istediği için BEKLEMEDE kaldı — uç T506, panel T532).

## 2026-11-09 — T365 (İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması) yapım bitti

2026-11-09 — T365 yapım bitti (dal `task/T365-iletisim-talebi`, PR `T365: ...`). `ICorporateContentModule` iki yeni yüz kazandı: `GetContactFormAsync` (açık mı, ön dolu üye değeri, beş konu, aydınlatma bağlantısı) ve `SendContactRequestAsync` (kapı, tuzak alan, alan kuralları, L-4 IP sınırı, Açık talep ve F-2 bildirimi; göndericiye numara ve e-posta yok); `ContactFormSending` sırayı tek yerde tutar. İki devam turu: ilk işçi kodu yazdı ve bağlamda bölündü; ikincisi Integration `CorporateContent` alanındaki 35 kırmızıyı teşhis etti (modül çözülürken `ContactRequests` `INotificationsModule` ister; modülü elle kuran beş düzeneğe `UnusedNotifications` kaydı eklendi). main (T367) birleştirildi. Unit 5509/5509, Integration tamamı koşuldu (birleşme öncesi 3162/3162; birleşme sonrası CorporateContent 185/185 ve Model 1393/1393), `dotnet format` üç projede temiz. Backlog D-810…D-812 (L-4 ortak yardımcı, form ekranı ve POST ucu, `HasContactForm` sabiti); D-199'un iletişim formu yarısı kapandı.

## 2026-11-09 — T367 (Müşteri iptali: kalem ve adet düzeyi, kargoya verilene kadar, dijital ve hizmet sınırı, sayaçların dönmesi, kısmi iptalde kupon) doğrulama tur 1 ✓ PASS

2026-11-09 — T367 doğrulama tur 1 ✓ PASS (head `5e076701`, PR #494). Dal CI 42588669661 bütün işler yeşil. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5153 + Consistency 283 = 5436/5436 (main birleştirmesi sonrası 5491/5491), betik 4/4, Integration Ordering 413/413 (birleşik ağaç 428/428), Catalog + Payment + Concurrency 122/122, Model + Commands + Web 2057/2057. Kriter 1–8 ve KK1–KK8 ✓. `12` satırları (`VAL-C003`…`C007`, `B019`, `A085`…`A087`) uç yanıtı ister — iptal ekranı T512, geri ödeme T370 — `BEKLEMEDE` kaldı (T351, T371 emsali). Bulgu yok; K-notları D-803 (Alındı + Ödendi'de son adetle S3 kolu testsiz → T512), D-804 (B-7 gövdesinde işlem tarihi yok → `08` güncelleme turu).

## 2026-11-09 — T367 yapım tamam, doğrulama bekliyor

2026-11-09 — T367 yapımı (dal `task/T367-musteri-iptali`). Müşterinin ödenmiş siparişte kalem ve adet iptali `IOrderTransitions.CancelItemAsync`: kilit altında `OrderOperations.CheckAsync` (fiziksel kalem Kargoya verildi'ye kadar, hizmet "tamamlandı"ya kadar, dijital hiç; ödenmemiş siparişte yalnız bütün iptal), `IAfterSalesModule.AddCancellationAsync` (adet kuralı + `ItemShares`/`Money.ShareOfUnits` ile kupon payı ve KDV aşağı yuvarlanır, son açık adetler kalanı alır), son açık adetlerde müşterinin S3/S4'ü + kayıt geçişe bağlanır (D-519), öteki iptallerde B-7 + kapanış kuralı (karışık sipariş kalan hatta düşer), en sonda stok/kontenjan dönüşü ve kupon hakkı yalnız siparişin son adediyle. Kabul kriterleri 1–8 ve KK1–KK8 ✓. Unit 5436/5436, Integration TAM 3139/3139 (Ordering 413 + Model 1393 + Commands 70 + öteki 1263); göç yok. Kapanan backlog: D-156, D-306, D-685; T367 payı kapanıp ileri atılan: D-153/154/155 → T512, D-305, D-337, D-453, D-483, D-513 → T368, D-519. Yeni: D-790–D-793.

## 2026-11-09 — T368 (Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi) doğrulama tur 1 ✓ PASS

2026-11-09 — T368 doğrulama tur 1 ✓ PASS (head `246071f7`, PR #498; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 3 — K1 D-819 hedefi T528'e genişletildi ve iz satırı eklendi, K2 → D-838, K3 D-817'ye eklendi; Unit 5501/5501 (Consistency 288 dahil), betik testleri 4 dosya, Integration dokunulan sınıflar 278/278, dal CI 42604944280 yeşil; KP-21 ve KP-47'nin T368'e eşlenen 11 satırı `BEKLEMEDE` kaldı — uç yanıtı ya da tarayıcı gözlemi ister, uç T512/T528; `12_matris_denetle.py` hata 0). `origin/main` (`78a3f598`, T352 kaydı) dala çakışmasız birleştirildi.

## 2026-11-09 — T368 yapım tamam, doğrulama bekliyor

2026-11-09 — T368 yapımı (dal `task/T368-gecikme-feshi`; iki işçi, ilki bağlam sınırında bölündü). Gecikme feshi `IOrderTransitions.TerminateForLateDeliveryAsync`: müşterinin düğmesi (tarih = bugün, B-9 + F-3) ve yöneticinin başka kanal kaydı (onay + sürüm + ulaşma tarihi ≤ bugün, B-9); Z-11 işlem kararıyla kilit altında; teslim tarihsiz her fiziksel kalemin bütün açık adeti ayrı kayıt tipine (`late_delivery_terminations`), dijital ve hizmet dışarıda; Hazırlanıyor'da stok döner ve kapanış kuralı S4 (Closure) yazar, kargodaki sipariş durumunda kalır; B-7 yok. Firma kalem iptali `CancelItemByFirmAsync` (kapalı altı sebep, "diğer"de not, "müşteriyle anlaşıldı"da `OwnerConfirmed`, geri alınamaz onay, B-7; son adetler firmanın S3/S4/S9'u, kayıt geçişe bağlanır); ödenmiş siparişin bütün firma iptali her açık kaleme kayıt yazar ve sayaç döndürür (D-305). Kanuni faiz uyarısı `OrderPeriods.LegalInterestWarningsAsync` (her fesih, süre aşımından sonraki iptal; ilk S5 sözü bitirir, D-531). "Stokta bulunamadı" uyarısı `IrreversibleConfirmations.FirmCancellation(..., isOutOfStock)`. Testler: Unit 5501/5501, Integration TAM 3170 (yeni `LateDeliveryAndFirmCancellationTests` 16/16; `StatusCorrectionTests` firma iptali kayıtları artık üretimden okunur). Backlog: D-817…D-820 (`Docs/DEFERRED_BACKLOG.d/T368.md`); D-305, D-519, D-531, D-793 ✅; D-337, D-453, D-513 T368 payı kapandı.

## 2026-11-09 — T369 (İşlemin adet birimi ve geri ödeme tutarı: açık adet, kupon payı, KDV, kargo ücretinin iptalde ve caymada ayrılması) doğrulama tur 1 ✓ PASS

2026-11-09 — T369 doğrulama tur 1 ✓ PASS (head `a604985d`, PR #501; dal CI 42627605629 yeşil; `origin/main` `d5676e75` (T372 #492) birleştirildi, birleşik ağaçta sınandı). Kriter 1–8 ve KK1–KK8 ✓; birleşik ağaçta para payı tek kural (`ItemShares.SetAside` + `SetAsideAsync` cayma yazıcısında da, `SetAsideParts` teslim almada, yuvarlama yalnız `Money`). Yerelde derleme `-warnaserror` 0/0, format 3 proje temiz, Unit 5575/5575 (Consistency dahil), betik 4/4, Integration Ordering 497/497 + Payment/Disposal/Commands 145/145, kilitli restore 0. Bulgu 0; K1: D-820'ye T369 payı yazıldı; K2: `VAL-A803`…`A806`'nın "Veri kaydı" yarısı D-843'e (T370) not edildi. Task satırları BEKLEMEDE (uç yok, geri ödeme satırı D-843). Birleştirme notu: yalnız `DEFERRED_BACKLOG.md` D-74 satırı çakıştı, main sürümü alındı (D-74 ✅ T367, T372); dalın ekinin özü: T369 `ShareOfUnits` kuralını parçalı teslim almaya genişletti (`ItemShares.SetAsideParts`, `02 §7.1.7` (5)). `12_matris_denetle.py` Hata 0.

## 2026-11-09 — T369 yapım tamam, doğrulama bekliyor

2026-11-09 — T369 (İşlemin adet birimi ve geri ödeme tutarı: açık adet, kupon payı, KDV, kargo ücretinin iptalde ve caymada ayrılması) yapım tamam: açık adet ayıpta dönülen adetleri düşer (`IPaymentModule.RescindedSharesAsync`), parçalı teslim almanın payı (`ItemShares.SetAsideParts`), kargo ücretinin tek kural evi (`ShippingFeeRefunds`), modül okumaları `RefundAmountAsync` ve `IsShippingFeeIncludedAsync`; 03 4.2.6 kaydı kuralın eylemlerine çevrildi (D-697 K2'nin T369 payı). D-74 kapatıldı; yeni kalemler D-842 (T374), D-843 (T370). Unit 5275/5275, Consistency 291/291, Integration tam koşu yeşil (1 yük zaman aşımı tek başına yeşil). Doğrulama bekliyor.

## 2026-11-09 — T370 (Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları) doğrulama tur 2 ✓ PASS

2026-11-09 — T370 doğrulama tur 2 ✓ PASS (head `fba7f20f`, PR #506; birleşik ağaç `origin/main` `64d0e44a` dahil, kapsam TAM). Kriter 1–9 ve KK1–KK9 ✓; T374'ün dönme ve bedel indirimi geri ödemeleriyle IBAN'ın ömrü birlikte doğru (yeni KK5 testleri); D-906 (T527) ve D-907 (T373) ertelemeleri kabul edilebilir. Bulgu 0. K1 kuyruktaki yeniden deneme ile müşterinin IBAN'ı aynı geri ödemeyi iki yoldan gönderebilir → D-908 (T405). `12` satırları `BEKLEMEDE` kaldı (uç, ekran, sağlayıcı işleyicisi bekleniyor). Dal CI head run 42686592582 ✓. Unit 5736/5736, Integration Ordering 557/557 + Payment/Disposal/Jobs/Model/Concurrency 1582/1582, betik 4/4, matris denetimi Hata 0.

## 2026-11-09 — T370 main birleştirmesi ve IBAN ömrü düzeltmesi, tur 2 bekliyor

2026-11-09 — T370 dalına `origin/main` `64d0e44a` (T374 #504, T383 #503) birleştirildi; çakışmalar (`IAfterSalesModule.cs`, `PaymentFacts.cs`, `DEFERRED_BACKLOG.d/T369.md`) yalnız ekleme (D-842 main'den, D-843 daldan). Davranış düzeltmesi (yardımcının sahip yerine kararı, `02 §7.4.5`): IBAN, siparişte onu kullanabilecek, işlenmemiş ve geçersizleşmemiş havale yolundaki her geri ödeme (T374'ün dönme ve bedel indirimi dahil) bitene kadar silinmez — tek yer `IsRefundIbanInUseAsync` → `IsUsedByAPendingTransferRefundAsync` (yeni okuma `IPaymentModule.FindPendingTransferRefundsAsync`). Integration `TheIban_OfTheItemsRequest_StaysWhileTheItemsOtherDefectRefundIsPending_AndGoesWithTheLast_KK5`, `TheIban_OnTheOrdersFile_StaysWhileAnotherItemsDefectRefundIsPending_AndGoesWithTheLast_KK5`. Yeni backlog D-906 (T527), D-907 (T373). Unit 5736/5736, Integration 3370/3370.

## 2026-11-09 — T370 (Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları) doğrulama tur 1 ✓ PASS

2026-11-09 — T370 doğrulama tur 1 ✓ PASS (head `6a9ed6b1`, PR #506). Kriter 1–9 ve KK1–KK9 ✓; bulgu 0. K1 bütün sipariş firma iptalinde kalem başına B-14 → D-902 (T528); K2 IBAN kullanım denetiminde döngüde sorgu → D-903 (T373). `12` satırları uç yanıtı / ekran beklediği için `BEKLEMEDE`. Dal CI head run 42672074277 işsiz `failure` (altyapı; iş koşmadı) — kayıt commit'inin run'ı merge'den önce okunur. Unit 5717/5717, Integration Ordering 540/540 + Payment/Disposal/Jobs/Model 1556/1556, betik 4/4, matris denetimi Hata 0.

## 2026-11-09 — T370 yapım tamam, doğrulama bekliyor

2026-11-09 — T370 (Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları) yapım tamam: tek kural evi `RefundLineRules` (yol, sistem mi firma mı, IBAN alanı, kart reddinde açık işlemler); kaydın geri ödemesi kaydın işleminde (`StartRefundOfRecordAsync`; müşteri ve firma iptali, bütün sipariş iptali, gecikme feshi — kartta sistem başlatır ve `CardRefund` işi, havalede IBAN gelmediyse istek + B-14; tutar T369 kuralı + kargo ücreti); müşteri iptali havalede IBAN'ı alır; yeni işlemler `RetryCardRefundAsync`, `OpenRefundTransferPathAsync`, `EnterRefundIbanAsync`, `ChangeRefundIbanAsync`, `ReportTransferFailedAsync`; IBAN son bekleyen geri ödemeyle imha kaydıyla silinir (düzeltmede de, D-517); ret ve bekleme işareti okuması (D-322). Kriter 1–9, KK1–KK9 ✓. Birleşik ağaçta (`origin/main` `d3ff0f8b`) Unit 5717/5717, Integration TAM (Ordering+Payment 554, Model 1404, kalan 1387) yeşil. Backlog: D-322, D-517, D-790, D-791, D-817, D-818, D-843 kapandı; yeni D-873–877, D-898 (beş işlemin işlem izi, T527). Şema değişikliği yok.

## 2026-11-09 — T371 (Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim) doğrulama tur 1 ✓ PASS

2026-11-09 — T371 doğrulama tur 1 ✓ PASS (head `d1a42218`, PR #488). Dal CI 42562463736 bütün işler yeşil. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5312/5312 (birleşik ağaç 5409/5409), betik 4/4, Integration ilgili sınıflar 79/79. Kriter 1–10 ve KK1–KK10 ✓ (7 modül payı: kesinti alanı yok, tutar T369). Bulgu yok; K1 `VAL-A206` "dört koşullu sebep" `02 §7.3.4`/`06 §5.3.7` ile uyuşmuyor → D-767. `12` satırları `BEKLEMEDE` kaldı (uç T372, panel T525, tutar T369). `origin/main` (T387) çakışmasız birleşti.

## 2026-11-09 — T371 (Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim) yapım tamamlandı

2026-11-09 — T371 (Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim) yapım tamamlandı, doğrulama bekliyor (dal `task/T371-cayma-hakki`, PR #488; kriter 1–10, KK1–KK10 öz denetimde ✓; koşullu istisna kuralı tek evde: `AfterSales/Contracts/WithdrawalExemptions` (genel, saf), `ConfirmationSummaryText.IsConditional` kopyası ve B-16 desenleri kaldırıldı; pencere/düğme kuralları T323/T325'te kurulmuştu, T371 Unit `WithdrawalRightTests` 6 + Integration `WithdrawalRightTests` 6 ile kriter kriter sınar; göç, ayar, uç yok; yerelde Unit 5312 + Consistency 283, Integration ilgili sınıflar 72 yeşil).

## 2026-11-09 — T372 (Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi) doğrulama tur 3 ✓ PASS

2026-11-09 — T372 doğrulama tur 3 ✓ PASS (head `794ab050`, PR #492; düzeltme 2 sonrası; dal CI 42613505273 yeşil). Kriter 1–9 ve KK1–KK9 ✓; para payı tek evde (`ItemShares.SetAside` + `SetAsideAsync`, `Money.ShareOfUnits`), `ItemAmountSplit`/`TakenSharesAsync` src ve testlerde yok. Yerelde dal ucunda derleme `-warnaserror` 0/0, format 3 proje temiz, Unit 5212/5212 + Consistency 288/288, betik 4/4, Integration Ordering 449/449, kilitli restore 0. Task satırları BEKLEMEDE (uç/sayfa yok, D-776; T371 emsali). K1: D-522'nin kapanış metnindeki silinmiş `ItemAmountSplit` anışı düzeltildi; K2 → D-854 (iki teyit yolunun farkı, T527). **Birleştirme notu:** `origin/main` (`357f5bad`: T365, T368, T329) ile birleştirme belge çakışması verir (`DEFERRED_BACKLOG.md` D-306/D-453, `DEFERRED_BACKLOG.d/T325.md` D-513) ve birleşik ağaçta `WithdrawalDeclarationTests.APhysicalItemNotShippedYet_…_KK8` düşer — T368'in `FirmReasonRules` "müşteriyle anlaşıldı" iptalinde `ownerConfirmed: true` ister; birleştirme geri alındı, kayıt birleştirmesiz.

## 2026-11-09 — T372 (Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi) düzeltme 2 (main birleştirmesi)

2026-11-09 — T372 düzeltme 2 (PR #492): `origin/main` (T367 #494) birleştirildi. Para payı tek evde, main'in sürümü: `Money.ShareOfUnits`, `ItemFrozenAmounts`, `ItemShares.SetAside`, `SetAsideAsync`; T372'nin `ItemAmountSplit` ve `TakenSharesAsync` kopyaları silindi, cayma yazıcısı main'in işlevlerini çağırır (kenar girdi farkı kabul, yardımcı kararı). Tekrar eden `MoneyTests` ve bölme testleri birleşti; backlog main sürümü üstüne T372'nin yerinde kapatmaları (D-74 ✅, D-306, D-453, D-513) yeniden işlendi, D-775 güncellendi. Derleme `-warnaserror` 0/0, format temiz, Unit 5500/5500, Integration TAM 3175/3175 (Ordering 449, `WithdrawalDeclarationTests` 21, `CustomerCancellationTests` 23).

## 2026-11-09 — T372 (Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi) doğrulama tur 2 ✓ PASS

2026-11-09 — T372 doğrulama tur 2 ✓ PASS (head `7901c14e`, PR #492; `origin/main` `2e55a220` birleştirildi, çakışma yok). Kriter 1–9 ve KK1–KK9 ✓; B1 kapandı (iki ardışık IBAN'sız firma kaydı tek açık isteği kullanır, 23505 yok), D-797 ve D-798 testleri yeşil. Yerelde derleme `-warnaserror` 0/0, format temiz, Unit 5177/5177 + Consistency 283/283, betik 4/4, Integration 206/206 (11 sınıf); birleştirmeden sonra Unit 5493/5493, Integration 108/108. Bulgu 0, K-notu 0. Task satırları BEKLEMEDE kaldı (uç/sayfa yok, D-776); `12_matris_denetle.py` Hata 0.

## 2026-11-09 — T372 (Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi) düzeltme 1 tamam, doğrulama tur 2 bekliyor

2026-11-09 — T372 düzeltme 1 (PR #492). B1: cayma yazıcısı kalemin açık IBAN isteğini okur; havale hattında ikinci IBAN'sız kayıt yeni istek eklemez, açık isteği kullanır (`06 §4.29`) — kayıt yazılır, 23505 yok; B-14 her IBAN'sız kayıtta gider (`02 §9.2`, `03 §1.11.34`). K1: IBAN getiren beyan ya da kayıt açık isteği karşılar (`FulfilledAt`, `RefundIbanId`) → D-797 ✅. K2: e-postadan gelen, sağlaması tutmayan IBAN testi → D-798 ✅. Üç yeni Integration testi. `origin/main` (T352) birleştirildi. Yerelde build 0 uyarı, format temiz, Unit 5177 + Consistency 283, Integration Ordering 416 ve AfterSales/IBAN tabloları 72 yeşil.

## 2026-11-09 — T372 (Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi) doğrulama tur 1 ✗ FAIL

2026-11-09 — T372 doğrulama tur 1 ✗ FAIL (head `79ce2083`, PR #492; `origin/main` `5f9b4c43` ile güncel; dal CI 42583106374 yeşil). Kriter 2–5, 8, 9 ✓; kriter 1 ve 6 ~ (firma yolunda ardışık kayıt); kriter 7 / KK7 ✗. **B1 KRİTİK S1** (`02 §10.4.10`, §7.3.5, §7.1.6): havale hattında kalemde açık IBAN isteği varken ikinci IBAN'sız kayıt ikinci `RefundIbanRequest` ekler, `OneOpenIbanRequestPerItem` tekil dizininde 23505 ile düşer (500), kayıt yazılmaz. Yerelde derleme 0/0, biçim temiz, Unit 5155 + Consistency 283, betik 4/4, Integration dokunulan 6 sınıf 86/86. `12` satırları BEKLEMEDE kaldı (uç/sayfa yok, D-776). K1 → D-797 (açık IBAN isteği karşılanmıyor), K2 → D-798 (e-postadan gelen geçersiz IBAN testi).

## 2026-11-09 — T372 (Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi) yapım tamamlandı

2026-11-09 — T372 yapım tamamlandı, doğrulama bekliyor (dal `task/T372-cayma-beyani`, PR #492; kriter 1–9, KK1–KK9 öz denetimde ✓). Cayma geçiş biriminde: `DeclareWithdrawalAsync` (müşteri) ve `RecordWithdrawalAsync` (başka kanal; ileri tarih yok, pencere dışı / bir yıl / mutlak istisna / teslim günü uyarısı `WithdrawalNoticeRules`); kilit, sürüm, kilitli siparişte işlem kararı, AfterSales yazıcısı (tutar `ItemAmountSplit` + yeni `Money.ShareOfUnits`, havale IBAN'ı, Z-38 silme günü, donan iade adresi), 1.4.4.6 sayaçları, tek yol kapanış kuralı, B-9/F-3/B-14. Beyan ekranı okuması `IOrderOperations.ReadWithdrawalScreenAsync`. Kapanan: D-522, D-74; T372 payları: D-667, D-513, D-453, D-306. Yeni: D-773…D-776. Göç, ayar, uç yok. Yerelde Unit 5430 + Consistency 283, Integration TAM 3118 yeşil, format temiz.

## 2026-11-09 — T373 (İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal) yapım tamam, doğrulama bekliyor

2026-11-09 — T373 yapım: teslim alma geçişi `ReceiveReturnAsync` (ulaşma tarihi ileri olamaz, alt sınır kalemin teslim tarihi; teslimden önce cayılanda alt sınır yok), ayrı "Stoğa ekle" adımı `AddReturnedStockAsync` (teslim alınan adet kadar, bir kez), Z-42 sonrası "mal dönmedi" kapanışı `CloseAsNotReturnedAsync` (geri ödeme ve bildirim yok, paylaşılmayan IBAN silinir; geç mal kapanışı yeniden açar), IBAN silinmişse teslim almada yeniden istek + B-14; ret paylaşılmayan IBAN'ı siler. İade kargo bedeli, etiket, taşıyıcı, kesinti ve değer kaybı alanı yok (birim testi). Testler: Unit 5448 + Consistency 291 ✓, Integration `Ordering` 566 ✓ (tam koşuda 43 ortam hatası `Web.RequestLimitsTests` / `Logging.LoggingFilesTests`, ayrı koşuda ✓). Backlog: D-523, D-877 kapandı; D-453, D-483 T373 payları kapandı; D-876 → D-910, D-879 → D-911, D-907 → D-912 (F3 gate); D-903, D-568 hedefi F3 gate.

## 2026-11-09 — T374 (Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi) doğrulama tur 2 ✓ PASS

2026-11-09 — T374 doğrulama tur 2 ✓ PASS (head `4edde518`, PR #504; `origin/main` `d3ff0f8b` dalda). Kriter 1–10, KK1–KK10 ✓ (modül düzeyi; ekran D-880). B1 kapandı (sipariş bütününün firma iptali dönülen adedi düşer, açık adedi kalmayan kalem atlanır); D-893 uygulandı (dönme ve bedel indirimi kalemin kalan ödenmiş tutarıyla sınırlı). Yerelde derleme `-warnaserror` temiz, format temiz, Unit 5417/5417 + Consistency 291/291, betik 4/4, Integration Ordering 543/543, Payment + Files + Administration + Model + Disposal 1617/1617. Dal CI 42671989182 altyapı hatası (iş başlamadı; T383, T370 koşuları da aynı) — merge kapısı kayıt commit'inin yeşil koşusu. K-notları: D-904 (kalem tavanı öteki geri ödeme yollarında, T370), D-905 (bütün adetleri dönülmüş kalemin kapanıştaki yeri, karar). `12` satırları `BEKLEMEDE` (uç yanıtı; T513, T528).

## 2026-11-09 — T374 düzeltme 1 tamam, doğrulama tur 2 bekliyor

2026-11-09 — T374 düzeltme 1: **B1** — sipariş bütününün firma iptali (`CancelOpenItemsByFirmAsync`) işleme açık adedi `UnavailableQuantitiesAsync` ile okur, dönülen adedi iptal kaydına yazmaz, açık adedi kalmayan kalemi atlar (500 yerine 200); kalem iptalinin "son açık adetler" kararı kapanış sorusu olduğundan kapanan adede (`ClosedQuantitiesAsync`) döndü — dönülen adedin yanında kalan adetlerin firma iptali artık reddedilmiyor. **K2 / D-893** (sahip yerine karar): kalemin geri ödemeleri kalemin ödenmiş tutarını aşmaz; dönme ve bedel indirimi bu kalanla sınırlanır, sıfırsa D-881. Yeni Integration testleri 6 (`…_KK8_B1` ×3, `…_D893` ×3). `origin/main` `d3ff0f8b` (T375) birleştirildi; birleşik ağaçta derleme 0/0, biçim temiz, Unit 5708/5708, Integration Ordering 543/543, Payment 14/14, `ItemRecordTablesTests` + `PaymentRecordTablesTests` 153/153.

## 2026-11-09 — T374 (Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi) doğrulama tur 1 ✗ FAIL

2026-11-09 — T374 doğrulama tur 1 ✗ FAIL (head `bbe9c7de`, PR #504; `origin/main` `5352fadd` T384 ile birleştirildi, çakışma yok; dal CI 42659236737 yeşil). Kriter 1–7, 9, 10 ✓; kriter 8 / KK8 ~ → FAIL. **B1 ORTA S1** (`02 §7.1.6`, §7.5.3; K-1077): sipariş bütününün firma iptali (`CancelOpenItemsByFirmAsync`) açık adedi `ClosedQuantitiesAsync` ile okur; Teslim edilemedi'de dönülmüş adedi olan kalemde S9 iptali `InvalidOperationException` ile düşer (500). K1 → D-892 (çözümlerin iz satırı, T528), K2 ⚠ → D-893 (bedel indiriminden sonra dönme ödenmiş tutarı aşar; karar → T370). `12` KP-23/KP-49 satırları `BEKLEMEDE` (uç yok). Yerelde derleme 0/0, biçim temiz, Unit 5412 + Consistency 291, betik 4/4, Integration Ordering 511/511, Files + Administration 138/138, Model/Disposal sınıfları 166/166.

## 2026-11-09 — T374 yapım tamam, doğrulama bekliyor

2026-11-09 — T374 (Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi) yapım tamam: ayıp talebinin para çözümleri — sözleşmeden dönme (`DefectClaimRescission` iadesi, adetlerin kupon payı ve KDV'siyle) ve bedel indirimi (tutar bazlı iade, tavan ödenen − iptal edilmemiş iadeler); dönülen adet açık adetten düşer (`UnavailableQuantitiesAsync`; fesih, firma iptali ve işlem ekranı aynı okuma); havalede IBAN yoksa yöneticinin IBAN isteği ve B-14; Z-18 bitişi işlem durumunda (`DefectChannelExpired`). Kanal, süre, durumlar, bildirimler ve dijital dosya güncellemesi T320/T325/T340/T351'den hazırdı, kanıtlandı. D-524, D-820, D-842 kapatıldı, D-513'ün T374 payı kapandı; yeni kalemler D-878 (T370/T405), D-879 (T373), D-880 (T513/T528), D-881 (⚠ karar). Unit 25/25 (yeni + Z-18), Integration Ordering 511/511, Payment+Concurrency+Disposal+Jobs 178/178, Model 1403/1404 (1 yük zaman aşımı tek başına yeşil). Doğrulama bekliyor.

## 2026-11-09 — T375 (Yöneticinin sipariş düzeltmeleri: ileri yönlü kayıt, tutar değişmez, firmanın teyidiyle e-posta ve adres düzeltmesi, cayma beyanı geri alınmaz) doğrulama tur 1 ✓ PASS

2026-11-09 — T375 doğrulama tur 1 ✓ PASS (head `45002c4c`, PR #505; dal CI 42660122152 yeşil). Kriter 1–9 ve KK1–KK9 ✓; bulgu 0. K1 DI kaydı yorumu bu PR'da düzeltildi; K2 `VAL-A239` teyit kaydı belirsizliği → D-894 (T530); K3 D-883 hedefi T530, D-884 müdahale 5 eklendi ve hedef T528/T530/T531. `12` satırları uç yanıtı / D-884 beklediği için `BEKLEMEDE`. Unit 5701/5701, Integration (iki sınıf) 54/54, betik 4/4, matris denetimi Hata 0.

## 2026-11-09 — T375 yapım tamam, doğrulama bekliyor

2026-11-09 — T375 (Yöneticinin sipariş düzeltmeleri) yapım tamam: `IOrderCorrections` adres düzeltmesi (firmanın teyidi, `OrderOperations` kararı, tek alan kuralı evi, geçmiş satırı, B-10) ve misafir e-postası düzeltmesi (firma teyidi, `RenewAccessKey`, hesap bağı, bildirimlerin yeni adrese yönlenmesi, B-1 yeni / B-15 eski adrese); dokuz müdahalenin kapalı listesi (`OrderInterventions`); D-66 (geçmiş ayarlayıcıları `private`), D-564, D-567 (`OrderAccessKeySourceScan`) kapandı; D-647 → D-883 (T527), müdahale iz satırları 2-4, 6-8 → D-884 (T527). Unit 5701/5701, Integration Ordering 528/528, Model geçmiş sınıfı 28/28; lint temiz.

## 2026-11-09 — T376 (Deneme limitleri: dokuz limit, kayan pencere, hesap kilitlenmez, nötr mesaj) doğrulama tur 1 ✓ PASS

2026-11-09 — T376 doğrulama tur 1 ✓ PASS (head `81bdbd1e` + `origin/main` birleştirmesi, PR #493). Dal CI 42586923780 bütün işler yeşil. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5472/5472, betik 4/4, Integration ilgili sınıflar 105/105. Kriter 1–9 ve KK1–KK9 ✓. Bulgu yok; K1 T376'nın `12` satırları (`VAL-A351`, `A352`, `A353`, `C075`, `A357`, `A362`) uç yanıtı kanıtı beklediği için `BEKLEMEDE` kaldı → D-801.

## 2026-11-09 — T376 (Deneme limitleri: dokuz limit, kayan pencere, hesap kilitlenmez, nötr mesaj) yapım ⏳

2026-11-09 — T376 yapım tamam, doğrulama bekliyor (PR #493, dal `task/T376-deneme-limitleri`). Mekanizma T215/T326/T342'den hazırdı; T376 dokuz limit envanterini (`AttemptLimitInventory`, `02 §8.2` ile eşit), L-8 sayımını (`OpenUnpaidOrderLimit`: pencere yok, e-posta ekseni yalnız üye siparişi, IP /64), birleşen sepette L-6 sayacının büyüğünü taşımasını (D-561 ✅) ve 429 yardımcısının limit koduyla red satırı yazmasını (D-145 ✅; `09 §8.2` m.3 çağıran listesi altıya çıktı) ekledi; sayı taraması (`AttemptNumberScan`) sayaç dosyalarında 0 ve 1 dışında sayı yazılmasını engeller. Unit 5446/5446, Integration 35 + 433 + 1987 geçti, format temiz. Yeni backlog D-780–D-783; D-108 F3 gate'e yeniden hedeflendi (⚠: bellek ekseni sahip kararı). Rapor: `TASK_REPORTS/T376_REPORT.md`.

## 2026-11-09 — T377 (Limit ilkeleri: iki eksen, ödenmemiş sipariş sayımı, yönetici muafiyeti yok, tanınan tarayıcı işareti) doğrulama tur 1 ✓ PASS

2026-11-09 — T377 doğrulama tur 1 ✓ PASS (head `c6b2a167`, PR #497; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 1 → D-826 (L-8 tavanı eşzamanlı onaylarda); Unit 5515/5515, Integration dokunulan sınıflar 87/87 + `MembershipTests` 133/133, dal CI 42600674503 yeşil; KP-72'den `VAL-A354`–`A356` PASS, `VAL-A358`–`A361` uç yanıtı ister, BEKLEMEDE (D-807); `12_matris_denetle.py` Hata 0).

## 2026-11-09 — T377 yapım tamam, doğrulama bekliyor

2026-11-09 — T377 yapımı (dal `task/T377-limit-ilkeleri`). Mekanizma T342/T376'dan hazırdı (iki eksen, tanınan tarayıcı sayacı, işaret kayıtları, L-6 sepet ekseni); T377 eksikleri kapattı: L-7 sayımı (`UnpaidCancellationLimit`: S3 geçişi, sistem aktörü, yalnız ödeme süresi dolan ve kart ödemesi başarısız sebebi; kayan pencere), L-7 ve L-8'in ortak eksen süzgeci (`OrderAxes`: IP ve yalnız üye siparişi için e-posta) ve sipariş onayının tek kapısı (`OrderPlacementLimits`: L-7 her yöntemi, L-8 yalnız havaleyi kapatır, hesap türü almaz); tanınan tarayıcı çerezi yazıcısı ve okuyucusu (`BrowserCookies.WriteKnownDevice`, `ReadKnownDevice`). `OrderConfirmationRequest.IpAddress` null olamaz. Testler: sipariş onayında üç uçtan uca senaryo, L-1 eksenleri, yönetici, işaret (uydurma, başka hesap, 90 gün), çerez, Google'ın L-1'e girmemesi (kaynak taraması), sepet ve stok limiti yokluğu. Backlog: D-781/782/783 ✅; yeni D-805–D-809 (⚠: L-7 `>=` eşiği, e-posta ekseni iletişim e-postasıyla, L-8 sayımı iki yöntemi sayar). Rapor: `TASK_REPORTS/T377_REPORT.md`.

## 2026-11-09 — T383 (Satış özeti ve ölçüler: dönem seçimi, ciro, sayılar, dört mağaza ölçüsü, sıfır payda, tepe gün sayısı) doğrulama tur 2 ✓ PASS

2026-11-09 — T383 doğrulama tur 2 ✓ PASS (head `e289f0b5`, PR #503; dal CI 42658024056 yeşil). Kriter 1–10 ✓; B1 kapandı — fiziksel kalemlerinin tamamı süre dolmadan kalem düzeyinde iptal edilen ya da çıkarılan karışık sipariş uyum oranının paydasından çıkıyor, düzeltilmiş iptal paydada kalıyor (validator sondajı 0/1, beklenen 0/1). `origin/main` (`5352fadd`, T384 #502) birleştirmesi kod dosyalarında çakıştı (`ICorporateContentModule.cs`, `CorporateContentModule.cs`, `ApplicationEnums.cs`, `IMPLEMENTATION_STATUS.md`) — geri alındı, doğrulama dalın ucunda; çakışmayı yardımcı çözdürür. `VAL-A272`…`A283`, `D030` uç ister (T524), BEKLEMEDE kalır. K-notları D-896 (ödemesi tamamlanmış kümesi, D-847 ile F3 kapısı), D-897 (`VAL-A887`'nin sahibi yok, F3 kapısı). Unit 5594/5594, Integration (dokunulan sınıflar) 333/333.

## 2026-11-09 — T383 (Satış özeti ve ölçüler) düzeltme 1

2026-11-09 — T383 düzeltme 1 (B1): kargoya verme sözüne uyum oranı, fiziksel kalemlerinin tamamı süre dolmadan kalem/adet düzeyinde iptal edilen ya da çıkarılan siparişi de paydadan çıkarır (K-535); hizmet/dijital kalemin açık kalması siparişi paydaya sokmaz, kısmi kapanma paydada kalır. Süre bitimindeki açık adet `IAfterSalesModule.OpenQuantitiesAtAsync` ile tek kapanış listesinden ve `ItemQuantities.Open`'dan okunur. 1 birim + 3 Integration testi; Unit 5594/5594, Integration SalesSummary 8/8, Ordering 502/502, Disposal+Administration+Payment 83/83. Doğrulama tur 2 bekliyor.

## 2026-11-09 — T383 (Satış özeti ve ölçüler: dönem seçimi, ciro, sayılar, dört mağaza ölçüsü, sıfır payda, tepe gün sayısı) doğrulama tur 1 ✗ FAIL

2026-11-09 — T383 doğrulama tur 1 ✗ FAIL (head `9f7b3620`, PR #503; `origin/main` `8cd6652e` ile çakışmasız birleştirildi; dal CI 42640832094 yeşil). Kriter 1–6 ve 8–10 ✓; kriter 7 ✗. **B1 ORTA S1:** kargoya verme sözüne uyum oranının paydası yalnız sipariş düzeyindeki S3/S4'ü çıkarıyor; fiziksel kalemi süre dolmadan kalem düzeyinde iptal edilen ya da çıkarılan, hizmet kalemi açık kalan karışık sipariş "uyulmadı" sayılıyor (`02 §10.6.3`, K-535; validator sondajı: beklenen 0/0, gelen 0/1). `VAL-A272`…`A283`, `D030` uç ister (T524), BEKLEMEDE kaldı. K-notu yok.

## 2026-11-09 — T383: Satış özeti ve ölçüler (yapım, PR açıldı)

2026-11-09 — T383 yapımı tamamlandı, doğrulama bekliyor: Administration'da `ISalesSummary` (dönem seçimi, onaylı sipariş sayısı ve ciro, en çok satanlar, ödeme yöntemi dağılımı, iptal ve iade sayısı, dört mağaza ölçüsü pay/payda olarak, sıfır paydada değerlendirilemez, tepe gün tetikleyicisinin günlük sayısı); Ordering, AfterSales ve CorporateContent yüzeylerine dönem okumaları; özetin yedi yeni süzme ekseni için `AddSalesSummaryPeriodIndexes` göçü ve `06` §7.5.1 satırları. Unit 5563/5563, Integration 3253/3253. Rapor: `Docs/TASK_REPORTS/T383_REPORT.md`.

## 2026-11-09 — T384 (Dışa aktarma: sipariş CSV, üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları) doğrulama tur 2 ✓ PASS

2026-11-09 — T384 doğrulama tur 2 ✓ PASS (head `2640fb6a`, PR #502; `origin/main` `8cd6652e` dalda). Dokuz kriter ✓; B1 giderildi (gönderimden önce kapanan adet toplamı, beş yeni test). Unit 5696/5696, Integration (dokunulan sınıflar) 279/279. `VAL-D029` PASS; `VAL-A266`…`A271` uç ister (T524), BEKLEMEDE. K1 → D-848'e not (çıkarma kaydı kapanış anı okumasında yok; T386).

## 2026-11-09 — T384 (Dışa aktarma: sipariş CSV, üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları) düzeltme 1 yapıldı

2026-11-09 — T384 düzeltme 1 (tur 1 B1): `OrderExport.FullyClosedBeforeShipmentAsync` gönderimden önce kapanan adetlerin toplamına bakar (`ClosingRecordMoment` kapanan adedi taşır); kısmi kapanma ya da gönderim sonrası kapanma (cayma) kargoya verme tarihini korur, tam kapanma boşaltır. Beş yeni entegrasyon testi (`DataExportTests` 36/36), D-736 kapanış satırına test adları yazıldı. Doğrulama tur 2 bekliyor.

## 2026-11-09 — T384 (Dışa aktarma: sipariş CSV, üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları) doğrulama tur 1 ✗ FAIL

2026-11-09 — T384 doğrulama tur 1 ✗ FAIL (head `f2ad23c3`, PR #502; `origin/main` `d5676e75` ile güncel; dal CI 42634024845 yeşil). Kriter 1–3 ve 5–9 ✓; kriter 4 ~. **B1 ORTA S1:** `OrderExport.FullyClosedBeforeShipmentAsync`, bir kısmı gönderimden önce iptal edilen, kalanı gönderilip sonra cayılan kalemde kargoya verme tarihini boş yazıyor (validator sondajı: beklenen gün, gelen boş). D-736'nın istediği test yok. `VAL-D029` PASS; `VAL-A266`…`A271` uç ister (T524), BEKLEMEDE kaldı. K1 → D-867.

## 2026-11-09 — T384 (Dışa aktarma: sipariş CSV, üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları) yapım tamamlandı

2026-11-09 — T384 yapım bitti (PR açıldı, doğrulama bekliyor). Sipariş dosyası kalem başına satır yazar (fatura verisi donmuş değerden, ödeme, kargo, iletişim ve geç ödeme sütunları; imha edilmişte ad, e-posta ve adres boş), üye ve iletişim talebi listeleri aralıksızdır; dosya geçici dizinde (`Infrastructure/CsvExportWriter`, `ExportTempDirectory`) yapılır, iz satırı dosyadan sonra yazılır, boş küme, yanlış aralık ve tavan aşımı iz yazmaz. `Caddyfile`a `/panel/disa-aktarma*` için `response_header_timeout 150s` ve `.env.example`a `Export__TempDirectory` eklendi (D-10, D-33, D-724, D-736 kapandı). `OrderExport` ilk sürümde `IAfterSalesModule` ile döngü kuruyordu; AfterSales ve Settings çağrıları gecikmeli çözülür. `ExportDirectoryStartup` yeniden-yazma açılışından sonra gelsin diye Administration kaydına taşındı. Yeni kalemler D-847–D-851.

## 2026-11-09 — T387 (Kurulum tarafı yönetim komutları) doğrulama tur 2 ✓ PASS

2026-11-09 — T387 doğrulama tur 2 ✓ PASS (head `4a872add`, PR #487). B1 ve B2 kapandı: `read-logs --source system` sunucunun kayıtlarını (veritabanı, pgBackRest, yedek aracı, işletim sisteminin günlüğü) okuyor, `read-command` onları salt okuma bağlıyor; `apply-payment-result` satırı sipariş kimliğini, sonucu ve tutarı taşıyor. Dal CI 42560074567 ilk denemede yalnız `LivenessEndpointTests.Get_WhenTheCommandIsSlow_Returns503WithinTheLimit` zamanlama testinde kırmızı (4.5 sn / 1 sn; dal o alana dokunmaz, yerelde 16/16), iş yeniden başlatıldı. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5120/5120 + tutarlılık 283/283, betik 4/4, Integration `Commands` 70/70, `Jobs` + `Logging` + `Ordering` 476/476. Kriter 1–9 ve KK1–KK9 ✓; VAL-C115 `PASS`. Yeni K-notları: D-763 (imha kaydı komut testi boş tabloyla), D-764 (Ctrl+C ile kesilen `read-logs` yarım CSV bırakır; D-538'e bağlı).

## 2026-11-09 — T387 (Kurulum tarafı yönetim komutları) düzeltme 1 — yeniden doğrulama bekliyor

2026-11-09 — T387 düzeltme 1 (PR #487): B1 — `read-logs --source system` sistem kayıtlarının üç kaynağını okur; `read-command` veritabanının, yedek aracının kayıtlarını ve işletim sisteminin günlüğünü salt okuma bağlar, okuyucu metin satırlarını zamanıyla ve systemd günlük dosyasını (düz ve sıkı biçim) okur; `DEPLOY_RUNBOOK §M.1` madde 8, §L.4 `Compress=no`; D-738 kapandı, gerçek journald denemesi D-749 (T603). B2 — `apply-payment-result` satırı siparişin iç kimliğini, sonucu ve tutarı kapalı değerlerle taşır, işlemden önce yazılır. K1 — D-745 kapandı (beş sonuç komut testinde). Yerelde build `-warnaserror` 0 uyarı, format temiz, `origin/main` birleştirmesinden sonra Unit 5403/5403, Integration 3095/3095.

## 2026-11-09 — T387 (Kurulum tarafı yönetim komutları) doğrulama tur 1 ✗ FAIL

2026-11-09 — T387 doğrulama tur 1 ✗ FAIL (head `dbb730f4`, PR #487). Dal CI 42552921022 bütün işler yeşil. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5355/5355, betik testleri 4/4, Integration `AccountAndStatusCommandTests` 43/43, `CreateAdminCommandTests` + `RestoreCommandsTests` 26/26. B1 (ORTA, S1; kabul kriteri 2): `read-logs` sistem kayıtlarında sunucunun kayıtlarını (`05 §9.1.4`) okumuyor, `read-command`'a bağlanmamışlar (`05 §9.3.1`); D-738 kabul kriterinin parçasını erteliyor. B2 (ORTA, S1; kabul kriteri 8): `apply-payment-result` satırı sipariş ve sonucu taşımıyor (yalnız komut adı ve boş kimlik). VAL-C115 → PASS. K1 → D-745.

## 2026-11-09 — T387 (Kurulum tarafı yönetim komutları) yapım sürüyor ⏳

2026-11-09 — T387 yapımı iki işçiyle sürüyor (ilk işçi bağlam sınırında bölündü): `job` (başarısız iş satırını yeniden kuyruğa alma ya da kapatma), `apply-payment-result` (işaretli kart siparişine panelde okunan sonucu olağan geçiş yolundan uygulama, tutmayan tutarda 3), `read-logs` (giriş kaydı, sistem kayıtları, imha kaydı; kaynak, aralık, süzgeç; IP ve e-posta yalnız terminalden; satır okumadan önce diske; sonuç yedek dışı geçici dizinde CSV); `--help` = `05 §8.5` tablosu iki yönlü testi; `Date`/`Amount` seçenek türleri (D-540 kapandı); compose `command`/`read-command` birimleri; `DEPLOY_RUNBOOK §M`. Yeni backlog D-738–D-742 (`DEFERRED_BACKLOG.d/T387.md`); D-538, D-539, D-541, D-598, D-599, D-602, D-633 hedefi F3 faz kapısı.

## 2026-11-09 — T388 (Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması) doğrulama tur 2 ✓ PASS

2026-11-09 — T388 doğrulama tur 2 ✓ PASS (head `7ed42256` + main `241f8669` birleştirmesi, çakışmasız; PR #483). Dal CI 42538348706 bütün işler yeşil. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5161/5161 (birleşik ağaçta 5193/5193), betik testleri 4/4; Integration (`postgres:18 -c track_commit_timestamp=on`) Commands 59/59, Commands + Logging + Disposal + Membership* + SessionLinkAttemptLoginTablesTests paralel 347/347, birleşik ağaçta Commands + Membership* + Ordering 543/543. Tur 1 B1 kapandı: salt okuma testi testin kendi veritabanını salt okunur açarak sınanıyor, öteki koleksiyonlarla paralelde kararlı. KK1–KK9 ✓. Task'ın KP birimi yok; VAL-A808, A810–A813 `BEKLEMEDE` kaldı. Bloke etmeyen K1 → D-734 (uygulama kaydı okuyucusunda metin olmayan alan istisnası; kullanılmayan `using`).

## 2026-11-09 — T388 (Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması) düzeltme 1 bitti

2026-11-09 — T388 düzeltme 1 bitti, ⏳ doğrulama tur 2 bekliyor (PR #483). B1: salt okuma testi sunucu genelindeki sayaç yerine testin kendi veritabanını salt okunur açarak sınanıyor (komut çıkış 0, yazma 25006 ile reddedilir kontrolü); öteki koleksiyonlarla paralel iki koşuda yeşil. main (T389 #472) birleştirildi: `AdminCommandCatalog` ve `CommandHost` kayıtları iki tarafın birleşimi, kopya servis kayıtları tek kayda indi, STATUS satırlarında iki taraf. Unit 5161/5161, Integration parçalı yeşil, build `-warnaserror` ve format temiz.

## 2026-11-09 — T388 (Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması) doğrulama tur 1 ✗ FAIL

2026-11-09 — T388 doğrulama tur 1 ✗ FAIL (head `5dc4f221`, PR #483). Dal CI 42533329123 bütün işler yeşil. Yerelde build `-warnaserror` 0 uyarı, format temiz (üç proje), Unit 5144/5144, betik testleri 4/4, Integration `RestoreCommandsTests` tek başına 12/12; başka koleksiyonlarla birlikte iki koşuda da `RollbackCheck_OnlyReads_NoTransactionIsStartedThatWrites` düştü. B1 (ORTA, S1, `09 §19.1`): test sunucu genelindeki işlem sayacını (`pg_snapshot_xmax`) karşılaştırıyor, paralel koleksiyonlar sayacı ilerletiyor; test kararsız. Kabul kriterleri 1–9 kodda karşılanıyor. K1 → D-716 (`refund_iban_requests` sınıflandırması).

## 2026-11-09 — T388 (Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması) yapım bitti

2026-11-09 — T388 yapım bitti, ⏳ doğrulama bekliyor (dal `task/T388-donus-denetimi`). `rollback-check --restore-point` (salt okuma; 24 sipariş tarafı tablosunda `pg_xact_commit_timestamp(xmin)` ile dönüş noktasından sonra kesinleşen satırları sayar; sıfır → 0, sıfırdan büyük ya da `track_commit_timestamp` kapalı → 3) ve `reapply-deletions --restore-point` (uygulama kaydındaki `InstantDeletion` satırlarını dönüş anından bugüne okur, her birini kendi işleminde müşteri/yönetici modülünün yeniden uygulama yöntemiyle uygular; kapılar yok, kaydı olmayan atlanır, e-posta kuyruğa girmez; sonra bütün oturumları siler ve kullanılmamış hesap bağlantılarının ömrünü dönüş anına çeker, e-posta değişikliğini geri alma bağlantısı hariç). Komut kipi servis listesine modüllerin kurucularının istediği `ImageProcessor`, `SiteAddress`, kurulum ayarı satırları ve `DiskUsageGuard` eklendi; denetim tarama testine `RollbackAudit` tablo listesi için tek girişlik dar istisna; CI entegrasyon işi `track_commit_timestamp` açık PostgreSQL ile koşar. Unit 5080, Integration 2935 yeşil. Backlog D-700 (anlık silme satırını yazan çağıran yok; T517/T522), D-701–D-704 yeni.

## 2026-11-08 — chore: "Son güncelleme" satırının sahibi birleştiren taraf (faz kapısı kayıt PR'ı, chore/kayıt PR'ı)

2026-11-08 — chore: "Son güncelleme" satırını yalnız birleştiren taraf (faz kapısı kayıt PR'ı, chore/kayıt PR'ı) günceller; task, düzeltme ve doğrulama commit'leri bu satıra dokunmaz, olayı `STATUS_CHANGELOG.md`'nin en üstüne ve task'ın tablo satırına yazar; main birleştirmesinde bu satır çakışırsa main sürümü alınır, dalın girişi changelog'a taşınır.

## 2026-11-08 — chore: Backlog parça dosyaları kuralı (`Docs/DEFERRED_BACKLOG.d/<TXXX>.md`; yeni kalemler parçaya yazılır, tabloya satır eklenmez; faz kapısı kayıt PR'ı ve chore/kayıt PR'ı D sırasıyla aktarıp parçaları siler; kalem araması iki yere bakar; `DEFERRED_BACKLOG.md`, task/validate/gate-check skill'leri, INSTRUCTIONS ve PR şablonu güncellendi)

## 2026-11-08 — chore: Yedek imajında restic indirmesi tek kaynağa bağlı olmasın

2026-11-08 — GitHub sürüm indirmesi 504 dönünce CI "7. Paketleme" adımı ve main kırmızı oldu; yedek imajı Dockerfile'ında restic indirmesi beş denemeyle (10 sn arayla) tekrarlanır, SHA-256 doğrulaması aynen kalır (yol b: resmî `restic/restic` imajından kopyalama CI'daki "yanlış özet derlemeyi düşürür" sınamasını, yani `scripts/ci/package-build.sh` adım 3'ü, bozacaktı; yerel derleme ve `restic version` 0.19.1 ✓, betik testleri ✓).

## 2026-11-08 — T303 (İlk yönetici yönetim komutu ve komut çatısı) doğrulama tur 1 ✓ PASS

2026-11-08 — T303 (İlk yönetici yönetim komutu ve komut çatısı) doğrulama tur 1 ✓ PASS (head `46b16782` + `origin/main` `26fd3a96` (T361 #461) birleştirmesi, PR #469; dal CI 42483739529 yeşil; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 0; `12` matrisinde task satırı yok (kaynak birimlerinde KP yok), `12_matris_denetle.py` hata 0; yerelde Unit 4777 + birleştirme sonrası Commands 43, betik 4, Integration Commands 14 + Membership/Address/AdminAndAudit 113 yeşil).

## 2026-11-08 — T303 (İlk yönetici yönetim komutu ve komut çatısı) yapım tamamlandı

2026-11-08 — T303 (İlk yönetici yönetim komutu ve komut çatısı) yapım tamamlandı, doğrulama bekliyor (dal `task/T303-ilk-yonetici-komut`; kriter 1–9, KK1–KK9 öz denetimde ✓; `Program.cs` ilk argüman `admin` ise `CommandHost.ExecuteAsync`: hizmet koleksiyonu `Maintenance` profiliyle, dokuz modül + `AddJobQueue`, sonunda `RemoveAll<IHostedService>()`; `CommandDispatcher` (çıkış 0/1/2/3, tek geniş `catch`), `AdminCommandCatalog.All`, `ITerminal`/`TerminalPrompt` (yönlendirilmiş girdi → 2, yankısız şifre), `create-admin` + `IAdministrationModule.CreateAdminAsync`; komut satırı `command-*.jsonl` dosyasında işlemden önce; `09 §4.1` beş klasör (`Commands/`), `09 §13.2` metodu adı `ExecuteAsync`; yeni backlog D-538–D-541; gerçek süreç testleri dahil Integration `Commands` 14 yeşil, Unit 4742+ yeşil).

## 2026-11-08 — T311 main birleştirmesi: `origin/main` (T324 zaman aşımı #454, T344 adres #451) dala alındı; `OrderingModule.cs` (`FindOrderForPaymentAsync` + `GetCardPaymentQueryTargetAsync`) ve `PaymentModule.cs` (bildirim ucu kayıtları + T324'ün son sorgu notu) iki tarafın eklemeleri yan yana korunarak çözüldü (yalnız ekleme, davranış değişmedi); backlog D-97'ye T324 notu eklendi, tablo sonu D-464, D-465, D-468, D-480 sırasıyla

## 2026-11-08 — T311 (Ödeme sağlayıcısının bildirim ucu) doğrulama tur 1 ✓ PASS (PR #456)

2026-11-08 — T311 (Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım) doğrulama tur 1 ✓ PASS (head `9bb8e06` + `origin/main` (ea62688c, chore #450) birleştirmesi, PR #456; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓; bulgu 0; K1 → D-480 (`IPaymentGateway`'in iki yeni işlevi `08 §3.1` ve `09 §11.1`'de yok), K2 D-97'de T311 ✅; dal CI 42450573628 yeşil; Unit 4393, betik testleri 4/4, Integration PaymentNotification + RequestLimits + JobRunner 72 ve SignedEndpoints/Ordering/Web/Security/Model 2005 yeşil; `12` VAL-A545, A586, A595, A706, A707, A821, A824, D082 PASS, A822 BEKLEMEDE (son sorgu T403); şema yok).

## 2026-11-08 — T311 (Ödeme sağlayıcısının bildirim ucu) yapım bitti (PR #456)

2026-11-08 — T311 (Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım) yapım bitti, ⏳ doğrulama bekliyor (dal `task/T311-odeme-bildirim-ucu`, PR #456; `origin/main` (T336, #444) birleştirildi). İmzalı uç `POST /sistem/odeme-bildirimi`; imza ve tutar bağlantı katmanında; `08 §2.1.5` sınıflaması; kayıt ve iş satırı aynı işlemde; iş ödeme sonucunu geçiş biriminin tek yolundan uygular. Kriter 1–10, KK1–KK10 öz denetimde ✓; Unit 4393, Integration `PaymentNotificationTests` 14 ve dokunulan sınıflar yeşil. Backlog D-468 (`06 §4.35` `ProcessedAt` metni) yeni.

## 2026-11-08 — T318 (İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi) doğrulama tur 2 PASS

2026-11-08 — T318 (İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi) doğrulama tur 2 (TAM yeniden doğrulama) ✓ PASS (head `7f662e0` = `ef5eff9` (T324 #454 birleştirmesi: ödenmemiş iptalin ayırma bırakması tek biçimde, `OrderCounters.ReleaseAsync`) + `origin/main` (`4ff04bc`, T344, #451), PR #455; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓; T324'ün ödenmemiş iptal davranışı bozulmadı; Unit Consistency 277 + öteki 4179, betik 4/4, Integration yerelde Ordering 185 (PaymentWindowTests dahil), Concurrency + StockAndCapacity + ModuleCallTransaction + OrderDisposal + ProductTransitions 124, Disposal 61; dal CI `7f662e0` 42456190315 yeşil; bulgu 0; `12` satırları değişmedi).

## 2026-11-08 — T318 (İki eksenin bağı) merge sonrası CI düzeltmesi

2026-11-08 — T318 (İki eksenin bağı) merge sonrası CI düzeltmesi: `AxesTieTests` fiziksel ürün tohumu T336 kuralı gereği `.WithSeededOrigin()` alır (yalnız test tohumu; PR #455, dal `task/T318-eksen-bagi`); Axes 20, Payment 33, Shipment 86 yerelde yeşil.

## 2026-11-08 — T318 (İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi) doğrulama tur 1 PASS

2026-11-08 — T318 (İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi) doğrulama tur 1 ✓ PASS (head `393bd9f` = `d36a461` + `origin/main` (`b3455838`, T358, #449) birleştirmesi, PR #455; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓; Unit Consistency 277 + öteki 4070, betik testleri 4/4, Integration yerelde Ordering 170, Concurrency + StockAndCapacity + ModuleCallTransaction + OrderDisposal 83, ProductTransitions 41; dal CI `d36a461` 42444593460 yeşil; bulgu 0; K1 bu PR'da kapatıldı (yapım raporunun `12` satırları KP-46/KP-51 grep'iyle düzeltildi); KP-46 ve KP-51 satırları `BEKLEMEDE` kaldı — uç ve hizmet "tamamlandı" işlemi yok, D-308, D-454).

## 2026-11-08 — T318 (İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi) yapım tamamlandı

2026-11-08 — T318 (İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi) yapım tamamlandı, doğrulama bekliyor (dal `task/T318-eksen-bagi`, PR #455; iki işçi — ilki bağlam sınırında bölündü, ikincisi yamayı `origin/main` `bcb856dc` üstüne uyguladı; kriter 1–10, KK1–KK10 öz denetimde ✓; birleşim tablosu kodda, Ö1'in eşzamanlı geçişleri (stok/kupon kesin düşüş, dijital teslim işareti ve tarihi, kapanış kuralı), S1 yalnız Ö1 ile (`StartPreparationAsync` kaldırıldı, D-307 kapandı), ödenmemiş iptal ayırmayı bırakır, havale düzeltmesi iki eksen + iki kayıt + ayırmanın geri yazılması, dokuz an tablosu `OrderCounters` ile, yedi hat ve elle adım bütçesi `OrderLines` ile; Unit 4289 (yeni tutarlılık 35), Integration 2271 (Ordering 170, yeni `AxesTieTests` 20) yeşil; format temiz; şema yok; backlog D-453…D-456 açıldı).

## 2026-11-08 — T319 (Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi) doğrulama tur 1 ✓ PASS

2026-11-08 — T319 doğrulama tur 1 ✓ PASS (PR #465, dal CI `42474311101` success; kriter 1–9 ve KK1–KK9 ✓, bulgu 0; doğrulayıcı yerelde Unit 4643/4643, Integration `StatusCorrectionTests` 34/34, `Ordering` 219/219, `Disposal` 61/61, `Catalog` 72/72; `origin/main` birleştirmesinden sonra Unit 4699/4699, `Ordering` 219/219; K1 bu PR'da kapatıldı (`12` KP-46 satırları `BEKLEMEDE`), K2 → D-533 (onay metinlerinin tek evini sayfa task'ları okur → T527, T528, T529, T533)).

## 2026-11-08 — T319 (Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi) yapım tamamlandı

2026-11-08 — T319 (Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi) yapım tamamlandı, doğrulama bekliyor (dal `task/T319-geri-alinamaz`; kriter 1–9, KK1–KK9 öz denetimde ✓; `IrreversibleConfirmations` sekiz işlemin onay cümlesi ve dijital kalemli havale işaretinin uyarısı; `OrderTransitions.CorrectTransitionAsync` düzeltmesi — S6, firma iptali S3/S4/S9, S5, S7, S8 ve havale Ö1 (S1 ile), kapalı sebep listesi, B-13, `CorrectionObstacle`; Unit 4366 + Consistency 277, Integration 2637 (`StatusCorrectionTests` 34) yerelde yeşil; D-275, D-456 kapandı, D-453 ve D-465'in T319 payı kapandı; yeni D-517 (düzeltmede IBAN silme → T370), D-518 (geçersiz iadeyi kart iadesi işleyicisi göndermez → T405), D-519 (`Cancellation.TransitionRecordId` üretim yazanı → T367, T368)).

## 2026-11-08 — T320 (Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri) doğrulama tur 2 ✓ PASS

2026-11-08 — T320 doğrulama tur 2 ✓ PASS (head `be854ace` + `origin/main` `f1603761` (T303) birleştirmesi, PR #466; dal CI 42486773053 yeşil). Kapsam tam: kriter 1–10, KK1–KK10 ve `09 §25` yeniden; tur 1 B1 kapandı — Z-18 kanalı yalnız `AfterSalesPeriodRules.IsDefectClaimChannelOpen`'da (Teslim edilemedi açık, İptal edildi kapalı; `03 §1.11.5`, §1.11.9), `IsAfterDelivery` tek; `DefectClaimWindow` yok. Yerelde derleme `-warnaserror` 0/0, biçim 3 proje temiz, Unit 4783/4783, betik 4/4, Integration `ItemRecordsTests` 35, Disposal 61, Ordering 261, Model 1315; `has-pending-model-changes` temiz; `restore --locked-mode` 0. `12` satırları (KP-23, VAL-C107/C108/D107) uç yanıtı ister, `BEKLEMEDE` kalır (T374 / T528). K1 → D-583 (kalemsiz "IBAN bekleniyor", T370).

## 2026-11-08 — T320 (Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri) tur 1 düzeltmesi (B1)

T320 tur 1 B1 düzeltildi (`origin/main` `26fd3a96` birleştirildi): ayıp talebinin açılması ve yeniden açılması Z-18 kanalını T325'in tek kuralından okur (`AfterSalesPeriodRules.IsDefectClaimChannelOpen`); `DefectClaimWindow` kaldırıldı, tek sabit `DefectClaimYears`. `ItemMarksAsync` "beyan teslimden sonra mı" için `AfterSalesPeriodRules.IsAfterDelivery`'yi çağırır (`ItemDelivery` ürün tipini taşır); `ItemRecordSubject.DeliveryMarkedAt` okunmadığı için kalktı. ⚠ Main kuralı değişti: teslim tarihsiz fiziksel kalemde Teslim edilemedi'de kanal açık (`03 §1.11.9`, §2.9.1; aynı sözü §1.11.5 açar) — D-532 ✅. Release derleme `-warnaserror` 0/0, biçim 3 proje temiz, Unit 4783/4783, Integration Ordering 261/261 + `ItemRecordsTests`/`OrderDisposalTests` 62/62.

## 2026-11-08 — T320 (Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri) doğrulama tur 1 ✗ FAIL

2026-11-08 — T320 doğrulama tur 1 ✗ FAIL (head `94695c67` = `7ed9e510` + `origin/main` `0a669969` (T325) birleştirmesi, PR #466; dal CI 42480410576 yeşil; kriter 1–10 ve KK1–KK10 ✓; Unit 4762 + betik 4, Integration `ItemRecordsTests` + `RecordDisposalTests` 44, Disposal 61, komşu Ordering 226, Catalog 72 yerelde yeşil). **B1 ORTA S1** (`09 §1`, §24 m.4, §25 mimari): Z-18 kanalı ve "beyan teslimden sonra mı" kuralı iki yerde — T325'in `AfterSalesPeriodRules` (`IsDefectClaimChannelOpen`, `DefectClaimYears`, `IsAfterDelivery`) ile T320'nin `DefectClaimWindow` (`IsOpen`, `Years`) ve `ItemMarksAsync`'teki özel `IsAfterDelivery`; Teslim edilemedi'deki fiziksel kalemde biri açık, öteki kapalı der. K1 → D-568 (iki `AwaitingReceipt` formülü). `12` KP-23 satırları ve VAL-C107/C108/D107 `BEKLEMEDE` (kanıt türü uç yanıtı).

## 2026-11-08 — T320 (Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri) yapım bitti, doğrulama bekliyor

2026-11-08 — T320 yapım tamamlandı (dal `task/T320-kalem-kayitlari`, PR #466; dört işçi, üçü bağlam sınırında bölündü). Kalem durum taşımaz; yolunu `ItemRecordTable` (`03 §1.7.1`'in on bir satırı, hat etkisi, adet ve yapan/an sütunları) anlatır, açık adet yalnız kapatan kayıtlardan düşer. AfterSales yazıcıları (`OpenDefectClaimAsync`, `ChangeDefectClaimStatusAsync`, `AddWithdrawalAsync`, `AddReturnReceiptAsync`, `AddReturnRejectionAsync`) çağıranın işlemine katılır ve sipariş kilidini ister; her beyan ve her parça teslim alma kendi satırı. Geçiş birimine dört işlem: ayıp talebi açma (B-9 + F-3), çözüldü işareti, yeniden açma (müşteri: B-9 + F-3; yönetici: bildirim yok), iade reddi (yalnız koşullu istisna, B-16). Ayıp talebinin durumu ve geçmişi yalnız `DefectClaimStatusWriter`'dan; Z-18 penceresi `DefectClaimWindow`'da; panel işaretleri (`ItemMarksAsync`: iade malı bekleniyor, IBAN bekleniyor) saklanmaz, kayıtlardan okunur. Durum geçmişi tipli liste (`DefectClaimHistoryEntry`), yük örneği `Payloads/DefectClaimHistoryEntry/1.json` ve testi. Kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması öz denetimde ✓. Unit 4412 + Consistency 280, Integration 2658 tam koşu yeşil. D-67, D-274, D-374 çözüldü; yeni D-522–D-526 (`DEFERRED_BACKLOG.d/T320.md`). `12` VAL-C107/C108 `BEKLEMEDE` (kanıt türü uç yanıtı).

## 2026-11-08 — T323 (Durum × rol × işlem tablosu) doğrulama tur 2 ✓ PASS

2026-11-08 — T323 doğrulama tur 2 ✓ PASS (head `e5644a69`, PR #474; kayıt commit'inden önce `origin/main` birleştirildi, çakışma yok). Dal CI 42513887058 yeşil (9 iş); yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 5017/5017, betik testleri 4/4, Integration `OrderOperationsTests` + `ItemRecordsTests` + `AxesTieTests` + `DeliveryPeriodTests` + `OperationPatternTests` 83/83, `OrderDisposalTests` + `ItemRecordTablesTests` 95/95. Tur 1 bulguları B1–B3 kapandı; kabul kriterleri 1–8 ve KK1–KK8 ✓. K-notu: 1.11.32'nin adım ayrımı `Contracts` yüzünde yok → D-660. `12`'de T323 satırı yok; `12_matris_denetle.py` Hata 0. Rapor: `Docs/TASK_REPORTS/T323_REPORT.md`.

## 2026-11-08 — T323 (Durum × rol × işlem tablosu) doğrulama tur 1 düzeltmesi

2026-11-08 — T323 tur 1 bulguları düzeltildi (B1–B3). 1.11.28/29 teslim alınacak adedi teslim alma yazıcısıyla aynı kuraldan sayar (teslimden önceki cayma ve gecikme feshinin malı dahil; hizmet ve dijital kalemin cayması mal doğurmaz); 1.11.25 teslim alma/ret kaydı varlığını okur; 1.11.32 iki adıma ayrıldı (sonuç beklenirken yalnız "gerçekleşmedi" işareti açık). Her biri `03 §1.11` satırına bağlı testle. Unit 4960/4960, Integration `Ordering` 293/293. Backlog: D-656–D-657. Rapor: `Docs/TASK_REPORTS/T323_REPORT.md`.

## 2026-11-08 — T323 (Durum × rol × işlem tablosu) doğrulama tur 1 ✗ FAIL

2026-11-08 — T323 doğrulama tur 1 ✗ FAIL (head `1c68a3b7`, PR #474). Dal CI 42504445458 yeşil; yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 4934/4934, betik testleri 4/4, Integration `OrderOperationsTests` + `ItemRecordsTests` 40/40. **B1 ORTA S1:** 1.11.28 ve 1.11.29 teslim alınacak adedi yalnız teslimden sonraki caymadan sayıyor; kargodayken cayılıp dönen gönderi ve gecikme feshinin malı sayılmıyor (`03 §1.11.28`; `02 §6.4.22`, §6.7.3, §7.4.7) — mevcut T320 teslim alma yazıcısı aynı durumu kabul ediyor. **B2 DÜŞÜK S1:** 1.11.25'in ulaşma tarihi kolu teslim almadan önce açık, sonra kapalı. **B3 DÜŞÜK S1:** 1.11.32 "geri ödeme sonucu bekleniyor"da yeniden deneme ve havale yolunu ayırmıyor (`02 §7.2.8`). K-notu: D-643 (1.11.33 kart hattının havale yolu). Rapor: `Docs/TASK_REPORTS/T323_REPORT.md`.

## 2026-11-08 — T323 (Durum × rol × işlem tablosu) yapım tamam, doğrulama bekliyor

2026-11-08 — T323 yapım tamam (PR açıldı). `03 §1.11`'in 41 işlemi (müşteri 14, yönetici 27) tek karar tablosunda: `OrderOperationCatalog` (41 satır, her satırın tek kural işlevi), `IOrderOperations.ReadAsync` (ekran/panel listesi) ve `CheckAsync` (her POST'un yeniden denetimi) aynı kuralı kullanır; tabloda olmayan işlem, başka siparişin kalemi ve eksik sipariş kapalıdır. Dönem durumları (Z-13/14/18/42) AfterSales'in kendi kurallarından (`ItemPeriodStatesAsync`) gelir. Testler: Unit `OrderOperationCatalogTests` 20 + `OperationTableConsistencyTests` 3 (Unit toplam 4870 yeşil), Integration `OrderOperationsTests` 5 (tam koşuda 2381/2383; 2 başarısızlık yerel kapsayıcıda `pg_trgm` ve uygulama kullanıcısı rolü ayarının olmamasından, T323 dışı). Backlog: D-610–D-614 (`DEFERRED_BACKLOG.d/T323.md`). Rapor: `Docs/TASK_REPORTS/T323_REPORT.md`.

## 2026-11-08 — T324 main sonrası Integration düzeltmesi: `PaymentWindowTests` fiziksel ürün tohumu `.WithSeededOrigin()` alır (T336 üretim yeri kuralı; yalnız test tohumu, PR #454, 18/18 yeşil)

## 2026-11-08 — T324 (Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri) doğrulama tur 1 PASS

2026-11-08 — T324 (Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri) doğrulama tur 1 ✓ PASS (head `2b5e108` = `6a8d862` + `origin/main` (`b345583`, T358, #449) birleştirmesi, PR #454; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; Unit Architecture 961 + Consistency 242 + öteki 3128, betik testleri 4/4, Integration yerelde PaymentWindow + PendingRecordDisposal 18, Shipment/Payment transitions + StockAndCapacity 154, Membership + Scheduler + AuditTrail 68, Disposal + SessionAccess + OrderTransitions + AppCache 99; dal CI 42445269145 yeşil; bulgu 0; K1 → D-464 (Z-3 davetin öteki koşulları T379/T520), K2 → D-465 (`PaymentWindows.StartAsync` T348/T319'da çağrılır); `12` KP-74 satırlarının `Durum`'u değişmedi — `VAL-D049` IBAN kolu T370'te).

## 2026-11-08 — T324 (Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri) yapım tamamlandı

2026-11-08 — T324 (Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri) yapım tamamlandı, doğrulama bekliyor (dal `task/T324-zaman-asimi`; kriter 1–8, KK1–KK8 öz denetimde ✓; Unit 4273, Integration PaymentWindowTests 15, PendingRecordDisposalTests 3 ve ilgili sınıflar yeşil; havale iptali, havale hatırlatması ve kart penceresi sonu kaynakları, son sorgu işleyicisi (kaydı T403, D-449), ödenmemiş iptalde ayırmanın serbest kalması, Z-1 gecelik silmesi; D-100 ✅, D-253 ✅, D-99 T324 payı ✅; yeni D-448, D-449).

## 2026-11-08 — T325 (Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri) doğrulama tur 1 PASS

2026-11-08 — T325 (Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri) doğrulama tur 1 ✓ PASS (head `908b740a` + `origin/main` `02ec5357` (T342 #453) birleştirmesi; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; bulgu 0; K1 → D-531 (faiz uyarısı kuralı kargoya verme anını okumaz, T368), K2 → D-532 (Teslim edilemedi'de ayıp kanalı okuması, T374); `12` `Durum` hücreleri değişmedi — task'ın KP kimliği yok, içerikçe ilgili satırlar uç/tarayıcı kanıtı ister ve işlemlerin task'larındadır; yerel Unit 4662 (birleştirme sonrası 4699), Integration `DeliveryPeriodTests` 7 + `PaymentTransitionsTests` + `ShipmentTransitionsTests` 119 yeşil; dal CI 42474305471 yeşil; merge yardımcıda).

## 2026-11-08 — T325 (Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri) yapım tamamlandı

2026-11-08 — T325 (Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri) yapım tamamlandı, doğrulama bekliyor (dal `task/T325-zaman-asimi-teslim`, PR #464; kriter 1–9, KK1–KK9 öz denetimde ✓; teslim süreleri kuralları `DeliveryPeriodRules` (Z-10, Z-39, Z-11, kanuni faiz uyarısı) ve okuyucusu `OrderPeriods` (ödeme onayı = düzeltilmemiş son Ö1), cayma/geri ödeme/ayıp süreleri `AfterSalesPeriodRules` (Z-13–Z-18, Z-42), panel süre göstergesi değeri `PeriodStanding`; hiçbir süre için iş türü, zamanlayıcı kaynağı ya da gecelik adım yok (mimari tarama); Unit 4662 (yeni 35), Integration `DeliveryPeriodTests` 7, Ordering ad alanı 192, AfterSales + Payment + Jobs 77 yeşil; D-161 ✅; yeni D-512, D-513).

## 2026-11-08 — T326 (Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri) doğrulama tur 1 PASS

2026-11-08 — T326 (Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri) doğrulama tur 1 ✓ PASS (head `0670e10f` + `origin/main` `f1603761` (T303 #469) birleştirmesi; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; bulgu 0; K-notu 0; task'ın `12` satırı yok; Unit 4811/4811 birleştirme sonrası, Integration `CouponTests` + `AttemptCounterTests` + `DiscountAndReferenceTests` 55/55).

## 2026-11-08 — T326 (Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri) yapım tamamlandı

2026-11-08 — T326 (Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri) yapım tamamlandı, doğrulama bekliyor (dal `task/T326-zaman-asimi-katalog`; kriter 1–8, KK1–KK8 öz denetimde ✓; Z-20 bitişinde fiyatın dönüşü ve onay anındaki bitişin özet farkı kuralı `DiscountRules.IsSummaryDifference`, Z-22 ve Z-23 bitişleri mevcut kural evlerinde sınır anlarıyla sınandı, Z-24, Z-25, Z-26, Z-37 için zamanlayıcı/kayıt/deneme kodu yok ve Z-21, Z-23, Z-27 hiçbir işi başlatmaz (mimari tarama `ExpiryTimerScanTests`), Z-27 kayan pencere Integration'da; D-570, D-571, D-572 yeni).

## 2026-11-08 — T335 (Ürün yayın durumu) doğrulama tur 1 ✓ PASS

2026-11-08 — T335 (Ürün yayın durumu: yayın kapısı, arşiv, silme ve engelleme kalıbı) doğrulama tur 1 ✓ PASS (head `7241002` + `origin/main` (`e2e9dbc`, T341, #433) birleştirmesi, PR #447; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4027 + Consistency 241 (birleşik ağaçta 4039 + 241), Integration `ProductPublishGateTests` 31/31 ve `ProductTransitionsTests` 41/41 (PostgreSQL 18, yerel); dal CI 42425264272 success; `12` satırları (`VAL-A021`…`A024`, `D001`, `C037`, `C038`, `A163`, `A171`, `A177`…`A180`, `D025`) `BEKLEMEDE` kaldı — kanıt türü uç yanıtı/tarayıcı, sayfalar T503/T549/T525'te; bulgu yok, K-notu yok (dosya silme–yayın yarışı yapımda D-401'e ileri atılmış). T341 birleştirmesinde yalnız STATUS çakıştı; ardından `origin/main` (`7097c01`, T317, #441) ikinci birleştirmesinde `TransitionTable.cs` ile üç kayıt belgesi çakıştı, iki taraf birlikte korundu; ardından `origin/main` (T333, #442) üçüncü birleştirmesinde `CatalogModule.cs` (kurucu üyeleri ve ağaç/liste yöntemleri birlikte korundu, yalnız ekleme) ve STATUS çakıştı.)

## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 ✓ PASS

2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 ✓ PASS (head `24be2b5` + `origin/main` (`9bb46eb`, T339, #445) birleştirmesi, PR #441; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; tur 2 B1 giderildi — ödenmemiş iptalin tek yolu `CancelOrderAsync`, Ö1'in tek yolu ödeme ekseni (D-389); Unit Architecture 933 + Consistency 241 + öteki 2942, betik testleri, Integration yerelde PaymentTransitions 33, OrderTransitions 31, ShipmentTransitions 94, Disposal 45 ve 4 geçici yoklama; dal CI 42421489591 yeşil; bulgu 0; K1 bu PR'da kapatıldı (D-291 metni güncel yola ve test adına bağlandı); `12` `Durum` değişmedi (KP-18 satırları sayfa ve uç kanıtı ister); backlog aktif 154).

## 2026-11-08 — T341 (Üyelik) doğrulama tur 3 ✓ PASS

2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `b2c6cca`, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 giderildi — kayıt e-postası `InvalidEmailAttribute` ile denetlenir; Unit 4069 (Consistency 241), Integration MembershipTests 34/34 ve komşu üç sınıf 95/95; dal CI 42410595147 success; `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (uç yanıtı sonraki task'larda); K1 → D-385 (T509). `origin/main` (T334) birleştirildi: `Modules/Ordering` iki dosyasında iki tarafın üyeleri birlikte korundu; backlog aktif sayısı yeniden sayıldı: 153; ikinci main birleştirmesi: `origin/main` (T321, #439) de birleştirildi, `OrderingModule` kurucusu ve `IOrderingModule` üyeleri iki tarafın eklemeleriyle birlikte korundu). (T335 doğrulama dalında STATUS'tan taşındı.)

## 2026-11-08 — T335 (Ürün yayın durumu) yapım bitti

2026-11-08 — T335 (Ürün yayın durumu: yayın kapısı, arşiv, silme ve engelleme kalıbı) yapım bitti, ⏳ doğrulama bekliyor (PR #447, dal `task/T335-urun-yayin-kapisi`; kriter 1–10 ve KK1–KK10 öz denetimde ✓: gerçek `CatalogPublishGate` (`UnwrittenPublishGate` kalktı), `DigitalFileCoverage` tek okuma (kapı + dosya silme), `PublishedProductGate` (son Yayında varyant tek kural), `ProductRemoval` (ürün/varyant kalıcı silme, dosya kaldırma, etki sayımı), `ProductPageReader` (adres cevabı); Unit 3921 + Consistency 241, Integration `ProductPublishGateTests` 31 + `ProductTransitionsTests` 41; backlog D-248, D-300, D-304 ✅, D-303 kısmen ✅, D-301 yeniden ileri atıldı, yeni D-401…D-403); önceki task'ların (T316, T321, T334) merge kaydı dolduruldu.

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153).

## 2026-11-08 — T337 (İndirim ve referans fiyat) doğrulama tur 1: ✓ PASS

2026-11-08 — T337 bağımsız doğrulaması tur 1'de PASS: dokuz kabul kriteri ve KK1–KK9 kanıtla karşılandı (kriter 4 sözleşme düzleminde, gösterim D-477 → T502/T503; kriter 9 T355'e bağlı, D-476). Yerel: Debug build `-warnaserror` 0 uyarı, format üç projede temiz, Unit 4434/4434 (Consistency ve Architecture dahil), betik testleri 4/4, Integration parçalı 273/273 (DiscountAndReference, CategoryTreeAndLists, PriceAndVat, ListPaging, Jobs, Catalog, StockAndCapacity, PaymentWindow, Concurrency). Matris satırları VAL-A018, VAL-A191…A195 `BEKLEMEDE` kalır (uç yanıtı/tarayıcı kanıtı T503 ve T525'te). Bloke etmeyen iki not backlog'a: D-487 (iki ret sebebi adı `07` koduyla birebir değil → T525 eşler), D-488 (indirim kaydından sonra fiyat artışı beyanı referansın üstüne çıkarabilir; dokümanda kural yok → karar, T525).

## 2026-11-08 — T337 (İndirim ve referans fiyat: tarihli indirim, fiyat geçmişinden referans, kaydı engelleyen koşullar) yapım bitti, PR açıldı

2026-11-08 — T337 yapımı üç turda bitti (iki tur bağlam sınırında bölündü), PR açıldı, doğrulama bekliyor: `DiscountRules`, `ReferencePriceRules`, fiyat geçmişinin tek yazıcısı `PriceHistoryRecorder` (sınırlar kendi anlarında, tek anda tek fiyat), `Discounts` (sil + ekle, sıfır ve referans reddi geri alır), `VariantPrices.ChangeAsync`, zamanlayıcı kaynağı `DiscountBoundarySource`, etikette ve kartta indirim satırı (D-345 ✅; kart K-1578 ile, süzgeç yarısı D-330 → T502); yeni D-475 (T525), D-476 (T355), D-477 (T502/T503). Unit 4192 + Consistency 242, Integration 2499 (DiscountAndReference 20), format ve Release `-warnaserror` temiz.

## 2026-11-08 — T338 (Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması) doğrulama tur 1 ✓ PASS (head `564c86f` = `600df1d` + `origin/main` (`4ff04bc`, T344, #451) birleştirmesi, PR #458; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; bulgu 0; dal CI 42456195767 yeşil; Unit 4204/4204 + Consistency 242/242, betik testleri 4/4, Integration `CouponTests` 19/19 ve ilgili sınıflar 255/255; `VAL-A036`–`A038`, `A196`–`A198`, `D008` BEKLEMEDE (uç ve onay task'ları T509, T526, T348); K-notları D-482, D-483 backlog'da)

## 2026-11-08 — T338 (Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması) yapım tamamlandı

2026-11-08 — T338 (Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması) yapım tamamlandı, doğrulama bekliyor (dal `task/T338-kupon`, PR #458; kriter 1–9, KK1–KK9 öz denetimde ✓; `CouponRules` saf kurallar — tanım alanları, yüzde/sabit kalem payı, kalan kuruş, KDV paydan sonra, ret kuralları, hakkın dönüş kararı; `Coupons` tanım/değerlendirme/L-6'lı uygulama; `ICatalogModule` dört yöntem; Unit 4414, Integration 2497 (CouponTests 19) yerelde yeşil; yeni D-482 (kalan kuruş taşması belgesi), D-483 (üretim çağıranları T348/T349/T509/T544/T367/T373)).

## 2026-11-08 — T340 (Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma) doğrulama tur 1 PASS

2026-11-08 — T340 doğrulama tur 1 ✓ PASS (PR #462, head `99b644a1`): kriter 1–9 ve KK1–KK9 kanıtla ✓; build `-warnaserror`, format, Unit 4646, betik testleri, Integration (Files 102, Disposal 61, Jobs 77, Address+Membership 47, Catalog 72) yerelde yeşil; dal CI'ında yalnız paketleme adımı dış 504 (restic indirmesi) ile kırmızı, merge yeni CI yeşil olunca. `12` KP-9/20/44 satırları uç kanıtı beklediği için BEKLEMEDE. Bloke-etmeyen iki not forward: D-527 (kalemin son dosya bağının ilerletilmesi, T525), D-528 (`Refunded` okuması, D-510 ile).

## 2026-11-08 — T340 (Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma) yapım tamamlandı

2026-11-08 — T340 (Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma) yapım tamamlandı, doğrulama bekliyor (dal `task/T340-dijital-urun`; kriter 1–9, KK1–KK9 öz denetimde ✓; `DownloadCounters` indirme sayacının tek yazıcısı — tek koşullu güncelleme ve sipariş kilidi altında yenileme; `DownloadRules` ödeme onayı ve dijital adet; `IDigitalDelivery`/`IOrderingModule` dört yeni yöntem; indirme ucu ödemesi onaylanmamış kalemde 404; tarama kuralları indirme sayacına genişledi (D-94 (1) kapandı); Unit 4369 + Consistency 277, Integration `DigitalDeliveryTests` 21 ve tüm Integration yerelde yeşil; yeni D-507–D-511).

## 2026-11-08 — T342 (Giriş ve oturum) doğrulama tur 3 ✓ PASS

2026-11-08 — T342 doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `87de279` + `origin/main` (`cca21e4`, T336) birleştirmesi, PR #453; dal CI 42446113821 yeşil; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; Unit 4349 (birleştirme sonrası 4407) + betik 4, Integration Membership 58 + komşu 258 yerelde yeşil; bulgu 0). Tur 2 Bulgu 1 kapandı (tek kilit sırası: hesap satırı `FOR UPDATE` en başta, sonra oturum ve bağlantı satırları; `MembershipPasswordLockOrderTests`), Bulgu 2 kapandı (`05 §6.1.2`, `09 §15.1` m.3). K1 bu PR'da kapatıldı: `06 §4.39` (×2) ve `07`'nin iki giriş ucu "paylaşımlı kilit" diyordu, `09 §10.2` m.6 izinli `FOR UPDATE` listesinde `AccountLocks.cs` yoktu. K2 → D-431 (`05 §7.3.5` hesap kilidi ailesini anmıyor). `VAL-A096/A097/A098/A103/D017` BEKLEMEDE (kanıt türü uç yanıtı; sayfa task'ları). Bu girişle dalın `STATUS_CHANGELOG.d/T342.md` parçası (aşağıdaki üç giriş) buraya taşındı — `origin/main`de parça klasörü yok (chore #450 açık).

2026-11-08 — T342 doğrulama tur 2 düzeltmesi (PR #453): Bulgu 1 (ORTA, S1) kapatıldı — şifre sıfırlama, değiştirme ve yeniden doğrulama hesap satır kilidini EN BAŞTA alır (`IPasswordAccounts.LockAsync`; `SessionStore` oturum satırı yazan yöntemleri de önce hesabı kilitler), kilit sırası tek: hesap, sonra oturum ve bağlantı satırları; `MembershipPasswordLockOrderTests` 4 test (değiştirme ↔ sıfırlama kilitlenmesiz, değiştirme ↔ giriş eski şifreli oturum bırakmaz). Bulgu 2 (DÜŞÜK): giriş `FOR UPDATE` kalır (özet yenilemesi paylaşımlı kilidi yükseltir, iki eşzamanlı giriş kilitlenir), `09 §15.1` m.3 ve `05 §6.1.2` koda uydu, karar D-428'de. Unit 4349, Integration Membership 58, Concurrency 26 yeşil.

2026-11-08 — T342 doğrulama tur 2 ✗ FAIL (head `1fa9150` + `origin/main` `7ddf334` birleştirmesi, PR #453; dal CI 42442007381 yeşil; kriter 1–9 ve KK1–KK9 ✓, tur 1 Bulgu 1 kapandı; Unit 4291, Integration Membership 54 + komşu 268 yerelde yeşil). Bulgu 1 ORTA S1 (`09 §25` eşzamanlılık): `ChangePasswordAsync` oturum satırını (yeniden doğrulama değerinin tüketimi) hesap satır kilidinden önce kilitler, sıfırlama ters sırayla — eşzamanlı değiştirme ↔ sıfırlama `40P01` kilitlenmesi (sondayla üretildi; sonuç güvenli, istek düşer); değiştirme ↔ giriş sınaması yok. Bulgu 2 DÜŞÜK S1: girişte `FOR UPDATE`, `09 §15.1` m.3 / `05 §6.1.2` paylaşımlı kilit der. K1 → D-431 (hesap kilidi ayrı aile; sepet birleşmesi aynı işlemde olamaz). `VAL-A096/A097/A098/A103/D017` BEKLEMEDE. Tur 2 iterasyon sınırı (`12 §8.5`).

2026-11-08 — T342 tur 1 düzeltmesi (PR #453, dal `task/T342-giris-oturum`): doğrulama Bulgu 1 (KRİTİK, S1: giriş ↔ şifre sıfırlama/değiştirme yarışı) kapatıldı. `AccountLocks` (`LockKind.Account`, hesap satırı `FOR UPDATE`, `LockSequence` bildirimli, işlem başına bir kez) girişte hesabı kilitleyip özeti kilit altında okur; `SessionStore.OpenAsync` oturumu aynı işlemde aynı kilit altında ekler; `SetPasswordAsync` (sıfırlama, değiştirme, özet yenileme) aynı kilidi alır; müşteri ve yönetici aynı kodu paylaşır. Eşzamanlılık testi: iki bağlantı, iki sıra (giriş önce / sıfırlama önce) × müşteri ve yönetici (+4, Membership 54); kilit kaldırılınca dördü de kırmızı. Unit 4291, Integration Membership 54 + komşu 99 yerelde yeşil. D-428: giriş/sıfırlama/değiştirme payı kapandı, hesap silme ve e-posta değiştirme payı T517'de açık.

## 2026-11-08 — T342 (Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri) doğrulama tur 1 ✗ FAIL

2026-11-08 — T342 doğrulama tur 1 ✗ FAIL (head `0fadc90` + `origin/main` (`bcb856d`, T302) birleştirmesi, PR #453; dal CI 42436720596 yeşil; Unit 4035 + Consistency 241 + betik 4, Integration Membership 43 + komşu 103 yerelde yeşil; kriter 1, 3–9 ve KK1, KK3–KK9 ✓). **B1 KRİTİK S1:** giriş hesabın satırını paylaşımlı kilitle okumuyor ve özetin hâlâ aynı olduğunu o kilit altında denetlemiyor (`09 §15.1` m.3, `05 §6.1.2`); şifre sıfırlama/değiştirmeyle aynı anda süren eski şifreli giriş, sıfırlamanın oturum silmesinden sonra kesinleşen oturumla açık kalır — kriter 2 / KK2 ~ Kısmi, `09 §25` eşzamanlılık satırı ✗. K1 D-251 metni düzeltildi (yönetim komutu T303'te), K2 durum satırı doğrulamadan önce ✓ yazılmıştı → ⏳. `12` KP-27 satırları `BEKLEMEDE` kaldı (uç yanıtı sayfa task'larında). Backlog aktif 163 (D-428 notu).

## 2026-11-08 — T342 (Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri) yapım tamamlandı, doğrulama bekliyor (dal `task/T342-giris-oturum`, PR #453; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması öz denetimde ✓; Unit 4276, Integration Membership 50 + komşu 114 + Web 594; D-114, D-133, D-251, D-255, D-364 çözüldü, D-127 T342 payı çözüldü; yeni D-428–D-432)

## 2026-11-08 — T343 (Google ile giriş, hesap bağlama, e-posta değişikliği) doğrulama tur 1 ✓ PASS

2026-11-08 — T343 doğrulama tur 1 ✓ PASS (PR #468, head `b067fd6d` + `origin/main` `26fd3a96` birleştirmesi; dal CI `42483471276` success; kriter 1–9 ve KK1–KK9 ✓, bulgu 0; doğrulayıcı yerelde Unit 4743/4743 (birleştirmeden sonra 4765/4765), betik 4/4, Integration `MembershipTests` + `AddressTests` 110/110; `12` satırları `BEKLEMEDE` kaldı (uç yanıtı, sayfa task'ları); K1 → D-577 (Google ile doğan hesapta misafir siparişlerinin bağlanması → T515), K2 → D-578 (`ExecuteUpdate` çevresindeki `DbUpdateException` yakalaması ve L-9 aşım testinin iddiası → T519)).

## 2026-11-08 — T343 (Google ile giriş, hesap bağlama, e-posta değişikliği) yapım tamam

2026-11-08 — T343 yapım tamam, doğrulamaya hazır (dal `task/T343-google-giris`). Membership modülü: Google kimliğinin hesaplara anlamı (`SignInWithGoogleAsync`: koşullu hat kapalıysa Unavailable; doğrulanmamış adres hiçbir şey bağlamaz; eşleşen doğrulanmış hesap tek kalır, `@gmail.com` tam eşleşmesi ya da `hd` ile bağ hemen kurulur, yetkisiz adreste bağ ve oturum yok, hesabın adresine teyit bağlantısı gider; bekleyen kayıt silinir ve imha kaydı yazılır; yeni hesap şifresiz, doğrulanmış ve bağlı doğar, kapı kapalıysa hiçbir şey yazılmaz), `ConfirmGoogleLinkAsync` (tek kullanımlık düğme, oturum açmaz), `RequestEmailChangeAsync` / `ResendEmailChangeLinkAsync` / `VerifyNewEmailAsync` (e-posta düğmeye kadar değişmez; doğrulanınca bağlama kuralı yeni adres için işler, Google bağı kalkar, eski adrese bildirim ve P-47 gün geri alma bağlantısı gider; L-9 yalnız yeniden istek ve yerine geçen istekte sayılır, Google girişi hiçbir limite girmez), `ReadEmailLinkAsync` (üç bağlantı türü için yalnız okuma). D-234 kapandı: iki eş zamanlı Google girişi hesap satırı kilidi altında tek bağlantı ve tek e-posta üretir. Facebook yolu yok (kaynak taraması). Unit 4743, Integration Membership 97 + Address/AccountEmailQueue 27 yerelde yeşil. Yeni backlog D-553…D-557.

## 2026-11-08 — T344 (Adres: adres defteri, fatura adresi, kapalı il listesi, tipe göre istenen adresler, siparişe donma) doğrulama tur 1 ✓ PASS (head `7e82f4f` + `origin/main` (`bcb856d`, T302, #446) birleştirmesi, PR #451; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; bulgu 0; dal CI 42436178107 yeşil; Unit 4277/4277 (birleştirmeden sonra Architecture + Membership 990/990), Integration `AddressTests` 13/13 ve `MembershipTests` 34/34; `VAL-A039`–`A042`, `A101` BEKLEMEDE (uç yanıtı ödeme adımı ve defter sayfası task'larında); K1: D-127'nin T344 payı kapandı; backlog aktif 166)

## 2026-11-08 — T344 (Adres: adres defteri, fatura adresi, kapalı il listesi, tipe göre istenen adresler, siparişe donma) yapım tamam, PR açıldı (dal `task/T344-adres`; kriter 1–9, KK1–KK9 öz denetimle ✓; Unit 4277, Integration `AddressTests` 13/13 ve `MembershipTests` 34/34; backlog D-435–D-437; doğrulama bekliyor)

## 2026-11-08 — T345 (Hesabın silinmesi ve kişisel veri başvurusu) doğrulama tur 2 ✓ PASS

2026-11-08 — T345 doğrulama tur 2 ✓ PASS (head `1a100354`, PR #477; kayıt commit'inden önce `origin/main` `48dadb92` birleşti, çakışma yok). Dal CI 42517577979 yeşil; yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 5008/5008 (birleşik ağaçta 5034/5034), betik testleri 4/4, Integration `MembershipTests` + `AddressTests` 142/142, `ConcurrencyTests` + `OperationPatternTests` + `OrderTransitionsTests` + `CartTests` + `CartQuantityTests` + `SessionAccessTests` 142/142. Tur 1 B1 kapandı: silme hesabın satırını `AccountLocks` `FOR UPDATE` ile kilitler, eşzamanlı sipariş yarış testi eskiden kırmızı olacak biçimde ayırt edici. Bulgu 0; K1 → D-683 (eskimiş açıklama ve gereksiz `TryRedeemReauthenticationHeldAsync`). `12`: VAL-A104 PASS; öteki satırlar uç yanıtı/uygulama kaydı/tarayıcı kanıtı beklediği için BEKLEMEDE (sayfa task'ları T517, T533; iletişim formu ve aydınlatma).

## 2026-11-08 — T345 düzeltme 1 (doğrulama tur 1 B1)

2026-11-08 — T345 düzeltme 1: silme hesabın satırını `AccountLocks` ile `SELECT … FOR UPDATE` kilitler (`09 §15.1` m.3; `05 §6.1.2`); `LockSequence`'te hesap kilidi sıranın başı (hesap → sipariş → varyant → kupon); eşzamanlı sipariş + silme için iki oturumlu Integration testi (sipariş ya işaretli ya hiç oluşmamış). `main` birleşti (`MembershipModule.cs` çakışmasında iki taraf korundu). Unit 5008, Membership+Address 142, Concurrency/OperationPattern/OrderTransitions 70 yeşil. Backlog D-664 (belge), D-665 (olası ölü kilit).

## 2026-11-08 — T345 (Hesabın silinmesi ve kişisel veri başvurusu) doğrulama tur 1 ✗ FAIL

2026-11-08 — T345 doğrulama tur 1 ✗ FAIL (head `7f842037`, PR #477). Dal CI 42509369740 yeşil; yerelde build `-warnaserror` 0 uyarı, format temiz, Unit 4934/4934, betik testleri 4/4, Integration `MembershipTests` + `AddressTests` 123/123, `ConcurrencyTests` + `OperationPatternTests` + `OrderTransitionsTests` 70/70. **B1 ORTA S1:** silme hesabın satırını kuru bir `UPDATE` ile tutuyor (`FOR NO KEY UPDATE`); bu kilit siparişin yabancı anahtar denetiminin `FOR KEY SHARE` kilidiyle çakışmaz, eşzamanlı onaylanan üye siparişi işaretin dışında kalır, hesap silinince `ON DELETE SET NULL` ile `customer_id` boşalır ama `is_account_deleted` yanlış kalır ve sipariş aynı adresle açılan yeni hesaba bağlanabilir (`05 §6.1.2` K-1084: "eşzamanlı onaylanan sipariş işaretin dışında kalmaz"; `09 §15.1` madde 3: hesabı silen işlem `AccountLocks` ile `FOR UPDATE`; `02 §3.15.3`) — PostgreSQL 18'de iki oturumla yeniden üretildi. K1 → D-652. main birleştirmesi `MembershipModule.cs`'te kod çakışması verdi, geri alındı (birleştirme notu raporda).

## 2026-11-08 — T345 yapım tamamlandı

2026-11-08 — T345 yapım tamamlandı: hesap silme (üye ve yönetici), imha kaydı, sipariş işareti (toplu yazma yok, kilitli izlenen satır; Account kilidi tek başına kaldı, `TryRedeemReauthenticationHeldAsync`), düzenlemesiz üye kaydı görünümü. Unit 4934, Membership 108 yeşil, öteki Integration sınıfları yeşil. Yeni D-605–D-608.

## 2026-11-08 — T346 doğrulama tur 2: ✓ PASS

2026-11-08 — T346 doğrulama tur 2 ✓ PASS (head `7eb682aa`, kapsam tam). Kabul kriterleri 1–7 ve KK1–KK7 ✓; dal CI run 42490284273 yeşil (Integration dahil); yerelde build `-warnaserror` temiz, format temiz, Unit 4773 (main birleştirmesinden sonra 4864), betik testleri 4/4, Integration Ordering 246 + Model (Address, Membership) + Sitemap 71 + Disposal 61 + Catalog 72 yeşil; main birleştirmesinden sonra Ordering 281, Address + Membership 110, Disposal 61 yeşil. `12` satırları (`VAL-A028`, `A029`, `B006`, `B007`, `D002`) uç ve iş kaydı kanıtı beklediği için `BEKLEMEDE` kaldı. Yeni K-notları: D-589 (misafir ekleme ile giriş birleşmesi aynı anda gelirse ekleme istisna verir), D-590 (birleşmede vitrinden kalkan kalem işaretinin taşınması test edilmemiş).

## 2026-11-08 — T346 düzeltme 1

2026-11-08 — T346 düzeltme 1: `origin/main` birleştirildi (`CatalogModule.cs` kurucusunda iki okuyucu birlikte kaldı); elle kurulan üç test düzeneği ortak `CartTestServices.AddCartServices` ile `Carts`'ı aldı, Integration tam paket yeşil (Unit 4773). D-579 (birleşme "ödeme yöntemi yok" işaretini taşır) ve D-580 (test adı) kapandı. Üretim DI'ı değişmedi.

## 2026-11-08 — T346 doğrulama tur 1: ✗ FAIL

2026-11-08 — T346 doğrulama tur 1 ✗ FAIL (head `859f957a`). Kabul kriterleri 1–7 davranış olarak karşılandı (`CartTests` 23/23 yeşil); dal CI'ında Integration kırmızı: `OrderingModule`'ü elle kuran test düzenekleri (`DisposalHarness`, `AddressTests`, `MembershipTests`) `Carts` kaydını almadığı için 110 test düşüyor (S2, ORTA). `12` satırları (`VAL-A028`, `A029`, `B006`, `B007`, `D002`) uç kanıtı beklediğinden `BEKLEMEDE`. D-302 hedefi T508'e, D-561 hedefi T376'ya taşındı; yeni D-579, D-580.

## 2026-11-08 — T346 (Sepet: sahiplik, giriş anında birleşme, canlı kalem, siparişe girmeyen kalemler) yapım tamamlandı

2026-11-08 — T346 yapım tamamlandı (dal `task/T346-sepet-sahiplik`, yapım iki işçide: ilk işçi disk dolduğu için kodu yazıp durdu, devam işçisi testleri, taramaları ve raporu bitirdi). Sepet işlemleri `IOrderingModule`'de: `OpenCartAsync` (canlı satır: güncel fiyat, "fiyatı değişti" yönsüz tek işaret, "Tükendi", vitrinden kalkan kalemin bir kez söylenmesi, siparişe girebilecek kalem yoksa `PaymentBlock`, `OrderableSubtotal`), `AddToCartAsync` (ilk ekleme fiyatı hatırlanır, aynı varyant yalnız adedi artırır), `MergeGuestCartAsync` (büyük adet kalır, önce eklenen satırın fiyatı ve anı belirler, görülmeyen ürün ya da adet bir kez adıyla söylenir). Katalog tarafı `ICatalogModule.ListCartVariantFactsAsync`. Eşzamanlı yazılan satırlar tek koşullu deyimle (`CartStatements`, `ON CONFLICT`) yazılır; bu dosya `VersionedSideBulkWriteScan` kapalı yerler listesine alındı (yalnız `carts` ve `cart_items`, test dosyayı dört deyim ve iki tabloya bağlar). `CartAddOutcome` ve `CartPaymentBlock` saklanmayan sonuç türleridir (`ApplicationEnums.NotStored`, kendi dosyalarında). Şema ve göç değişikliği yok. Unit 4716 (yeni 16 `CartRulesTests` + 1 tarama testi), Integration `CartTests` 19 + `ProductTransitionsTests` 41 yerelde yeşil. D-303 yerinde kapandı; yeni kalemler D-558–D-562 (`DEFERRED_BACKLOG.d/T346.md`).

## 2026-11-08 — T347 (Sepet adet kuralları: stok ve sipariş başına sınır, boşalma, istek listesi yokluğu) doğrulandı — tur 1 ✓ PASS

2026-11-08 — T347 bağımsız doğrulaması tur 1: ✓ PASS (head `a8bfecd1`, PR #475). Kabul kriterleri 1–7 ve KK1–KK7 kanıtla ✓; dal CI'ı run 42502184068 yeşil; validator koşusu build 0/0, Unit 4951, Integration Ordering 301 + Catalog/Files/Web/Concurrency 794 yeşil; `origin/main` (T311) çakışmasız birleşti, birleşik ağaçta Unit 4974 ve Ordering + Payment 315 yeşil. Bulgu 0. `12` satırları (`VAL-A030`, `A031`, `A033`, `A979`, `A978`, `A989`, `D072`) uç yanıtı ister → `BEKLEMEDE`. K-notları: D-637 (D-560'ın dijital adet payı, T508), D-638 (sonuç adları ↔ `07` hata kodları, T508).

## 2026-11-08 — T347 (Sepet adet kuralları: stok ve sipariş başına sınır, boşalma, istek listesi yokluğu) yapım tamamlandı

2026-11-08 — T347 yapım tamamlandı, PR açıldı (dal `task/T347-sepet-adet`, yapım iki işçide: ilk işçi kodu ve testleri yazıp bağlam sınırında durdu, devam işçisi tam paketleri, biçimi ve kayıtları bitirdi; kriter 1–7 ve KK1–KK7 öz denetimle ✓). Stok kuralı ve "bir siparişte en fazla" sınırı Katalog'da tek saf kuraldadır (`CartQuantityRules`; okuyucu `CartQuantityReader`, arayüz `JudgeCartQuantitiesAsync` / `JudgeCartAddAsync`): ekleme, artırma ve birleşme aynı kuraldan geçer, sınır varyantların toplamına uygulanır ve stoktan önce söylenir, ayrılmış adet stoktan düşülür, sonradan aşılan kalem sepette kalır, adet kendiliğinden indirilmez, siparişe ve toplama girmez. Ödeme başarılı olunca sepet yalnız siparişin varyantlarından boşalır (`CartStatements.DeleteOrderedItemsAsync`, tek koşullu deyim); birleşip silinen misafir sepetinin ödemesi hesabın sepetine dokunmaz. İstek listesi, favori ve sepet hatırlatma yokluğu kaynak taramasıyla sabitlendi. D-589 kod düzeltmesi (ekleme kilitten sonra silinmiş misafir sepetinde yeni sepet açar) ve D-590 testi yapıldı; D-560, D-589, D-590 kapandı; D-150 hedefi T508'e çevrildi; yeni D-619–D-621. Şema ve göç değişikliği yok. Unit 4951, Integration tam 2848 (`origin/main` birleştirmesinden sonra) yeşil, biçim temiz.

## 2026-11-08 — T349 doğrulama tur 2: ✓ PASS

2026-11-08 — T349 doğrulama tur 2 ✓ PASS (head `0451c5d5`, `origin/main` `48dadb92` birleştirildi). Kriter 1–9, KK1–KK9 ve `09 §25` ✓ (tur 1 bulgusu kapandı: `Payloads/ShippingFeeVatShare/1.json` + örnek dosya testi); dal CI run 42517753555 yeşil; yerelde build, format, Unit 5028 (birleşik ağaçta 5054), betik 4/4, Integration Ordering 323 + Model.CouponTests yeşil. `12` satırları BEKLEMEDE (uçlar T508/T509/T536, onay T348). K1 → D-681 (D-221 kapanışı ↔ `02 §3.33.3`); K2 rapordaki `VAL-A296` etiketi düzeltildi.

## 2026-11-08 — T349 düzeltme 1 bitti

2026-11-08 — T349 düzeltme 1 bitti, ⏳ doğrulama tur 2 bekliyor (PR #476). Bulgu 1: `tests/Shopfolio.UnitTests/Payloads/ShippingFeeVatShare/1.json` + `ShippingFeeVatSharePayloadTests` (5 test). K2: `VAL-A797` örnek değerli birim testi (`ShippingChargeTests …_VAL_A797`; 3,33 / 6,67, KDV 0,56 / 0,61). `origin/main` (T347, T379, T354) birleştirildi: `CartContracts.cs` iki tarafı korur (`ProductType` + `QuantityProblem`, `OrderLimit`), `Order.cs` iki `using`, `DEFERRED_BACKLOG.d/T346.md` main satırları + D-562 ✅; `CartRulesTests` ve `CartQuantityTests` birleşik ağaca uyarlandı. Build `-warnaserror` 0/0, format üç proje temiz, Unit 5028, Integration Ordering 323, Model 1380 yeşil. Yeni backlog kalemi yok.

## 2026-11-08 — T349 doğrulama tur 1: ✗ FAIL

2026-11-08 — T349 doğrulama tur 1 ✗ FAIL (head `302cb1bf`). Kriter 1–9 ve KK1–KK9 davranış olarak ✓ (onay çağrısı T348 — D-626, sayfalar T508/T509 — D-627); dal CI run 42509314185 yeşil; yerelde build, format, Unit 4951, betik 4/4, Integration Cart/Coupon/OrderTransitions 76, Ordering 289, Disposal 61 yeşil. Bloke eden bulgu (ORTA, S1): yeni yük tipi `ShippingFeeVatShare`'in örnek dosyası `Payloads/ShippingFeeVatShare/1.json` ve örnek dosya testi yok (`09 §12` m.8, `09 §25`). K1 → D-650 (0 TL ücrette eşik mesajı); K2 düzeltme turunda (`VAL-A797` örnek testi). `12` satırları BEKLEMEDE. Birleştirme notu: `origin/main` (T347) `CartContracts.cs`'te kod çakışması — birleştirme geri alındı, düzeltme turu çözer.

## 2026-11-08 — T349 (Asgari sipariş tutarı, kargo ücreti ve ücretsiz kargo eşiği) yapım bitti (PR #476)

2026-11-08 — T349 (Asgari sipariş tutarı, kargo ücreti ve ücretsiz kargo eşiği) yapım bitti, ⏳ doğrulama bekliyor (dal `task/T349-asgari-tutar-kargo`, PR #476; `origin/main` (T311, #456) birleştirildi, çakışma yok). Saf kural `ShippingCharge.Assess` (taban, fiziksel kalem, ücret, eşik, kalan, eksik tutar, `MinimumOrderAmountNotMet`); sepet `CartView.Shipping` ve `CartPaymentBlock.MinimumOrderAmountNotMet`; `IOrderingModule.OpenCartAsync` ayarı çağırandan alır; `Order.ShippingFeeVatBreakdown` değer listesi + `HasPayloadConversion()`, `FreezeShippingFeeVatBreakdown` bir kez (göç yok). Kriter 1–9, KK1–KK9 öz denetimde ✓. Backlog D-65, D-221, D-562 kapandı; D-483'e T349 payı yazıldı; yeni D-626 (T348 onay çağrısı), D-627 (T508/T509 sayfa).

## 2026-11-08 — T350 (Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi) doğrulama tur 1 ✓ PASS (head `bfdc36c`, PR #460; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 0; Unit 4649/4649, Integration dokunulan + komşu sınıflar 457/457; dal CI `9c97e24` 42461958983 yeşil, `bfdc36c` 42464103500 yalnız Paketleme restic indirmesi HTTP 504 — geçici; task satırları uç/ekran task'larına kadar BEKLEMEDE)

## 2026-11-08 — T350 (Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi) yapım tamam, PR açıldı (dal `task/T350-teslim`; kriter 1–9, KK1–KK9 öz denetimle ✓; Unit 4562, Integration 2552; backlog D-493–D-496, D-436 kapandı; doğrulama bekliyor)

## 2026-11-08 — T351 (Teslim işaretleri: kalem tipine göre teslim, karışık sipariş, fiziksel teslim tarihi) doğrulama tur 1 ✓ PASS (head `7adf3e66`, PR #478; kriter 1–7, KK1–KK7, `09 §25` ve güvenlik taraması ✓; bulgu 0, K-notu 2 → D-685, D-686; Unit birleşik ağaçta 5063/5063, Integration dokunulan sınıflar 140/140, dal CI 42516860761 yeşil; `VAL-B001`, `VAL-A020` BEKLEMEDE)

## 2026-11-08 — T351 (Teslim işaretleri) yapım bitti

T351 (Teslim işaretleri: kalem tipine göre teslim, karışık sipariş, fiziksel teslim tarihi) yapımı bitti; PR açıldı, doğrulama bekliyor. Hizmet kalemi için `IOrderTransitions.MarkServiceCompletedAsync` ("tamamlandı" işareti, işaretin günü, son işaret siparişi S10/S11 ile kapatır; Ordering `OrderTransitions.ServiceMark.cs`); işaret koşulları tek yerde `DeliveryMarkRules` (ödeme Bekliyor ya da Başarısız dışı, Kısmen geri ödendi kapatmaz); teslim tarihi reddi (ileri tarih, kargoya verme gününden önce) `OrderTransitionResult.DateObstacle` ile sebebini söyler; kalem için durum makinesi yok taraması (`ItemHasNoStateMachineTests`, KK4). Kriter 1–7 ve KK1–KK7 testli; D-454 kapandı, D-508 hedefinden T351 çıktı; yeni kalemler D-645…D-648 (`DEFERRED_BACKLOG.d/T351.md`).

## 2026-11-08 — T354 (Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı) doğrulama tur 4 ✓ PASS

2026-11-08 — T354 doğrulama tur 4 ✓ PASS (head `061064f4`, PR #470; dalın içeriğine göre). Kriter 1–6, KK1–KK6, `09 §25`, güvenlik taraması, yerel testler (Unit 4878, Integration OrderAccess+Address+Membership 128 / Disposal 61) ve dal CI 42500104111 ile ✓; içerik bulgusu yok. VAL-D009, B014, B015, B016, A054, A977, A055, A057, A058, B017 `BEKLEMEDE` (uç/onay kanıtı T511, T348). Birleştirme notu: `origin/main` (T311 #456) `Modules/Ordering/OrderingModule.cs` ve `Contracts/IOrderingModule.cs`'te ekleme çakışması verdi; merge iptal edildi, merge öncesi iki taraf kalacak biçimde çözülmeli.

## 2026-11-08 — T354 (Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı) doğrulama tur 3 ⛔ BLOCKED

2026-11-08 — T354 doğrulama tur 3 ⛔ BLOCKED (head `6c2919e8`, PR #470). Dalın içeriği kriter 1–6, KK1–KK6, `09 §25`, güvenlik taraması, yerel testler (Unit 4878, Integration Ordering 299 / Disposal+Model+Concurrency 1442 / Catalog+CorporateContent+Files+Pages+Web 854) ve dal CI 42500104111 ile ✓; içerik bulgusu yok. `git merge origin/main` (T350 #460) yine `Modules/Ordering/OrderingModule.cs`'te çakıştı (T350 `GetShippingViewAsync` ve `_shippingViews` ekledi). Merge iptal edildi; çakışma çözülüp birleşik uç yeniden doğrulanacak. `12` hücreleri `BEKLEMEDE` kaldı.

## 2026-11-08 — T354 (Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı) doğrulama tur 2 ⛔ BLOCKED

2026-11-08 — T354 doğrulama tur 2 ⛔ BLOCKED (head `12de6aad`, PR #470). Dalın içeriği kriter 1–6, KK1–KK6, güvenlik taraması, yerel testler (Unit 4822, Integration OrderAccess 18 / Ordering 244 / Disposal 61) ve dal CI 42492570840 ile ✓; içerik bulgusu yok. `git merge origin/main` kod dosyasında çakıştı: `Modules/Ordering/OrderingModule.cs` (T346 #467 kurucuya `Carts`, kayda `CartStatements`/`Carts` ekledi). Merge iptal edildi; çakışma çözülüp birleşik uç yeniden doğrulanacak. `12` hücreleri `BEKLEMEDE` kaldı.

## 2026-11-08 — T354 düzeltme 1: üç test düzeneğine OrderAccess kaydı

2026-11-08 — T354 düzeltme 1 (doğrulama tur 1 B1): `DisposalHarness`, `Model/AddressTests` ve `Model/MembershipTests` düzeneklerine `services.AddScoped<OrderAccess>()` eklendi; `tests/` taraması başka elle kuran düzenek bulmadı. Build `-warnaserror`, format ve Integration TAM küme (2782 test, klasör parçalarıyla) yeşil; üretim kodu değişmedi.

## 2026-11-08 — T354 (Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı) doğrulama tur 1 ✗ FAIL

2026-11-08 — T354 doğrulama tur 1 ✗ FAIL (head `53a71cd0` + `origin/main` `f1603761` (T303) birleştirmesi, PR #470). Dal CI 42485052233 kırmızı: 4. Integration test 110/2729 başarısız — 109'u `Unable to resolve service for type 'Shopfolio.Modules.Ordering.OrderAccess' while attempting to activate 'Shopfolio.Modules.Ordering.OrderingModule'`. **B1 ORTA S2:** `OrderingModule` kurucusuna `OrderAccess` eklendi, ama `OrderingModule`'ü elle kaydeden üç test düzeneği (`Disposal/DisposalHarness.cs`, `Model/AddressTests.cs`, `Model/MembershipTests.cs`; `services.AddScoped<IOrderingModule, OrderingModule>()`) `OrderAccess`'i kaydetmiyor — Disposal, `AddressTests`, `MembershipTests` kümeleri düşüyor (yerelde de yeniden üretildi). Kriter 1–6 ve KK1–KK6 modül düzeyinde ✓; Unit 4770, betik 4, Ordering Integration 244 yerelde yeşil; build `-warnaserror` ve format temiz. Düzeltme: üç düzeneğe `services.AddScoped<OrderAccess>()` (gerekirse `OrderIdentifiers`) eklenir, tam Integration yeşil görülür. Yapım raporu öz denetiminde "OrderingModule'ü elle kuran yer yok" iddiası yanlış (uyuşmazlık).

## 2026-11-08 — T354 (Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı) yapım tamamlandı

2026-11-08 — T354 (Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı) yapım tamamlandı, doğrulama bekliyor (dal `task/T354-siparis-no-takip`; kriter 1–6, KK1–KK6 öz denetimde ✓; `OrderNumber.Create` yıl + P-19 blok, 32 karakterlik alfabe; `OrderIdentifiers` çakışmasız numara ve erişim anahtarı verir, iptalin numarası da atlanır; misafir numara + e-posta sorgusu `FindOrderByNumberAndEmailAsync`, donmuş e-postayla eşleşir; `Order.RenewAccessKey`; `GetOrderPageSummaryAsync` durum, takip bağlantısı, indirme düğmeleri ve donmuş IBAN; Unit 4748, Integration `OrderAccessTests` 18, Ordering ad alanı 244 yerelde yeşil; yeni D-563–D-567).

## 2026-11-08 — T359: Kurumsal içerik: ürün bağı, elle sıralama, galeri (yapım tamam, doğrulama bekliyor)

Hizmet ve referans işine ürün bağı (`ContentProductLinks`: ürün düzleminde, varyantsız; yalnız yayındakiler "İlgili ürünler"de, taslak bağda kalır, silinen kalkar), beş listenin elle sırası (`ContentOrdering`: koşullu `ExecuteUpdate`, eski ekran 409, eşit değerde yeniden numaralama, sürüm artmaz), galeri kuralları (`GalleryText` ürünle tek işlev; `ContentGallery`: ana işaret sıradan bağımsız, boş alt metinde "Ad — görsel p / n", P-25 tavanı, şubede tek görsel) ve D-498 kapanışı (galeri kaydı ana sayfa blok önbelleğini düşürür) eklendi. Unit 5024, Integration CorporateContent 121 + Catalog 72 yeşil; `dotnet format` temiz. Yeni backlog D-671–675.

## 2026-11-08 — T361 (Ana sayfa düzeni ve üst menü) doğrulama tur 2 ✓ PASS

T361 doğrulama tur 2 ✓ PASS (head `2266a634`, PR #461; kayıt commit'inden önce `origin/main` `02ec5357` birleştirildi, çakışma yok). Kriter 1–6 ve KK1–KK6 ✓, tur 1 B1 kapandı, bulgu 0; güvenlik taraması temiz; dal CI 42474299037 yeşil. K1: D-372 (T361'de karşılanmış T333 K-notu) ✅ kapatıldı, kalanı (kategori ağacı önbelleği) D-529'a. Task satırları (VAL-A001, A003, A111…A114, A152…A154) uç/tarayıcı kanıtı ister → `BEKLEMEDE`; `12` değişmedi.

## 2026-11-08 — T361 (Ana sayfa düzeni ve üst menü) tur 1 düzeltmesi (B1)

T361 tur 1 B1 düzeltildi: `ProductRemoval.DeleteProductAsync` başarıda `CacheArea.Menu` ve `CacheArea.Sitemap` bildirir (işlem kesinleştikten sonra düşer); yayındaki son ürün kalıcı silinince "Ürünler" ve ürünsüz kalan kategori menüden düşer. Entegrasyon testi eklendi; D-499 ✅ kapatıldı; yapım raporu ("Known Limitations" 2, öz denetim, düzeltme turu notu) güncellendi. Yayında ürünü/kategoriyi değiştiren öteki yazma yolları main'de tarandı: eksik yol yok. Doğrulama tur 2 bekliyor.

## 2026-11-08 — T361 (Ana sayfa düzeni ve üst menü) doğrulama tur 1 ✗ FAIL

T361 doğrulama tur 1 ✗ FAIL (head `9610aa44`, PR #461). Kriter 1–5 ve KK1–KK5 ✓; kriter 6 ~ Kısmi — B1 DÜŞÜK S1: `ProductRemoval.DeleteProductAsync` `CacheArea.Menu`/`Sitemap` bildirmiyor, yayındaki son ürün kalıcı silinince "Ürünler" ve ürünsüz kalan kategori süresiz önbellekte menüde kalıyor (`02 §3.28.6`, `05 §7.10.1`; yerel sonda testiyle kanıtlandı; D-499'un "yol henüz yok" varsayımı yanlış — yol T335'te). K1: D-498 metni düzeltildi (önbelleğin süresi yok). Yerelde derleme `-warnaserror` 0/0, biçim temiz, Unit 4649/4649, betik testleri geçti, Integration 130/130 + 157/157. Dal CI 42468942806: 7. Paketleme restic indirmesinde dış HTTP 504 ile kırmızı (yeniden koşulmalı). `12` satırları (VAL-A001, A003, A111…A114, A152…A154) uç/tarayıcı kanıtı ister → `BEKLEMEDE`.

## 2026-11-08 — T361 (Ana sayfa düzeni ve üst menü) yapım bitti

T361 yapım bitti (PR açıldı; doğrulama bekliyor). Okuma yüzü: `ISettingsModule.GetHomepageAsync` (düzen ve menü adları), `ICatalogModule.GetMenuAsync` (yayında ürünlü kategori ağacı), `ICorporateContentModule.GetMenuAsync` (sabit iskelet, gizli öğelerin nedeni, altbilgi sayfaları) ve `GetHomepageAsync` (iki düzene göre bloklar: kurumsal blok, işaretli hizmet ve referans, vitrin 4/8 en yeni yayındaki ürün). Menü yapısı `CacheArea.Menu`, ana sayfa kurumsal kısmı `CacheArea.HomepageBlocks` önbelleğinde; ürün durum geçişi ve kategori bağı değişimi menü ve site haritası önbelleğini düşürür; vitrin önbelleksiz. Sayfa uçları ve düzen kaydı T505/T537/T535'te (D-502). Sabit sayfa adresleri `AddressRules` sabitlerine toplandı. Enum'lar ayrı dosya, saklanmayan listeye eklendi. Unit 4537/4537, Integration `CorporateContent` 85/85, `Catalog` 41/41, `BrandingTests` 29/29, geri kalan Model/Ordering ilk koşuda 1 kararsız test (`CatalogCoreTablesTests`, tek başına 60/60). Yeni backlog D-498…D-502. main (T362) birleştirildi; `CorporateContentModule`, `ICorporateContentModule`, `SitemapSourceTests` çakışması iki tarafı koruyarak çözüldü.

## 2026-11-08 — T362 (Marka kimliği) doğrulama tur 2 ✓ PASS (PR #448)

T362 (Marka kimliği: logo, marka adı, site simgesi, marka rengi, platform imzası) doğrulama tur 2 ✓ PASS (TAM yeniden doğrulama; head `659eb11`, PR #448; kriter 1–6, KK1–KK6, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi — renk biçimi kırpılmış değere `InvalidColorAttribute` ile denetlenir; dal CI 42435465085 success; Unit 4294/4294, betik testleri 4/4, Integration `BrandingTests` 29/29 + komşu üç sınıf 115/115; bulgu 0, yeni K-notu yok; D-442 açık; `VAL-A157`…`VAL-A160` `BEKLEMEDE` kaldı (sayfa T534/T537, gönderen D-442); `12_matris_denetle.py` Hata 0). Dal main ile birleştirilmedi: main'de T302 ile `CorporateContentModule.cs` kod çakışması var, birleştirmeyi yardımcının işçisi yapar.

## 2026-11-08 — T362 (Marka kimliği) doğrulama tur 1 ✗ FAIL; B1 düzeltildi (PR #448)

Bulgu B1 (DÜŞÜK, S1, `09 §25` "tek işlev"): `BrandingRules.TryNormalizeColor` kendi `#RRGGBB` denetimini yazıyordu; tek kural evi `InvalidColorAttribute`. Düzeltme: biçim denetimi kırpılmış değere `new InvalidColorAttribute()` ile yapılır (`CompanyIdentityRules` kalıbı), büyük harfe çevirme kalır; ayrışan örnek (`"  #ffff00 "`) ve ret örnekleri `BrandingRulesTests` teorisine girdi; `T362_REPORT.md` öz denetim tablosu güncellendi. T362 diff'inin geri kalanında kendi denetimi yok (grep). `origin/main` (T333, #442 dahil) birleştirildi; durum satırı main sürümü, backlog sayacı 160.

Tur 1 sonucu: kriter 1–6 ve KK1–KK6 ✓, güvenlik taraması temiz, dal CI yeşil; tek bulgu B1; K1 `D-442`'ye ileri atıldı.

## 2026-11-08 — T362 (Marka kimliği) yapım bitti

2026-11-08 — T362 (Marka kimliği: logo, marka adı, site simgesi, marka rengi, platform imzası) yapım bitti, PR #448 açık, doğrulama bekliyor (dal `task/T362-marka-kimligi`; task `⏳`; kriter 1–6 ve KK1–KK6 öz denetimde ✓; Unit 955 (Architecture) + 3315 + Consistency 241, Integration `BrandingTests` 29/29 ve komşu dört sınıf 157/157; şema, paket, ortam değişikliği yok). `CorporateContent` modülü marka formunu okur ve kaydeder (`SaveBrandingAsync`: sürüm, ad boşaltılamaz, renk `#RRGGBB`, görsel yüklenmiş olmalı), vitrin ve e-postanın tek okuması `GetBrandIdentityAsync`; iz: dört ayar `UpdateBranding`, imza anahtarı ayrı `UpdateSettings` satırı (hedef `Branding`, `09 §9.2` tablosu güncellendi). Marka formu sayfası ve vitrin/e-posta bağlama T534/T537'de. Ana dal CI 42426248097 (T341 birleşmesi) T362 dışı: `UploadEndpointsTests` port 5000 çakışması.

## 2026-11-08 — T379 doğrulama tur 2: ✓ PASS

2026-11-08 — T379 doğrulama tur 2 ✓ PASS (head `5f0e456f`, kapsam tam; PR #473). Kabul kriterleri 1–9 ve KK1–KK9 ✓, `09 §25` ve güvenlik taraması temiz; dal CI run 42504976109 yeşil (Lint, Build, Unit, Integration, Contract, Migration, Paketleme); ana dal son 3 run yeşil. Yerelde build `-warnaserror` 0 uyarı, format üç projede temiz, Unit 4928/4928 (main birleşmesinden sonra 4951/4951), betik testleri 4/4, Integration `MembershipTests` 114/114, `AuditTrailTests` + `AppCacheTests` + `CompanyIdentityTests` 60/60, Web ad alanı 592/594 (iki `EmailCaptureRegistrationTests` düşmesi tek başına 3/3 geçer, T379'la bağı yok → D-639). B1 kapandı. `12` KP-62 satırları uç/tarayıcı kanıtı istediği için `BEKLEMEDE` kaldı. D-617 açık; yeni K-notu D-639.

## 2026-11-08 — T379 düzeltme turu 1 tamamlandı (B1: yedi IDE0008 açık türle düzeltildi)

2026-11-08 — T379 düzeltme 1. `MembershipTests.AdminAccounts.cs` içindeki yedi `var` açık demet türüyle değişti; `dotnet format --verify-no-changes` Web, UnitTests ve IntegrationTests için ayrı ayrı temiz, build `-warnaserror` 0 uyarı, main birleşmesi sonrası Unit 4928/4928 ve Integration TAM küme (Model 1379, Web 594, Database 36, öteki 840) yeşil. D-617 açık kaldı.

## 2026-11-08 — T379 (Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı) doğrulama tur 1 ✗ FAIL

2026-11-08 — T379 doğrulama tur 1 ✗ FAIL (head `beae4623`, PR #473). Dal CI 42495939596 kırmızı: 1. Lint `dotnet format --verify-no-changes` yedi `IDE0008` hatası verdi (`tests/Shopfolio.IntegrationTests/Model/MembershipTests.AdminAccounts.cs` satır 61, 267, 289, 350, 382, 398, 445: `var` yerine açık tür); Build, Unit, Integration ve öteki adımlar CI'da hiç koşmadı. **B1 ORTA S1:** biçim kuralı ihlali, yerelde aynı yedi hata yeniden üretildi. Yerelde build `-warnaserror` 0 uyarı, Unit 4881/4881, Integration `MembershipTests` 114/114 ve ilgili dört sınıf 27/27, betik testleri 4 dosya yeşil. Kabul kriterleri 1–9 ve KK1–KK9 modül düzeyinde kodla ve testle ✓; KP-62 satırları uç yanıtı/tarayıcı kanıtı istediği için `BEKLEMEDE` kaldı. Yeni K-notu D-617 (e-posta değişikliği yardımcılarının kopyası). Düzeltme: yedi `var`'ı açık türle değiştirip `dotnet format Shopfolio.slnx --verify-no-changes` temiz, dal CI yeşil.

## 2026-11-08 — T379 (Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı) yapım tamamlandı, doğrulama bekliyor (dal `task/T379-yonetici-hesaplari`; kriter 1–9, KK1–KK9 ve `09 §25` öz denetimde ✓)

2026-11-08 — T379 yapım tamamlandı. `IAdministrationModule` dokuz yöntem kazandı: davet gönder, geri çek, bağlantıyı oku, kabul et, yönetici kaldır, ad değiştir, e-posta değişikliği iste/yeniden gönder/doğrula. Davet gönderimi önceki kullanılmamış daveti bağlantısıyla birlikte geçersiz kılar (Z-3) ve yalnız gönderim F-6 üretir; kaldırma yönetici kümesi kilidi altında, kendini ve son yöneticiyi reddeder, onaysız çağrıda yalnız ad, adres ve geçersizleşecek davet sayısını söyler; onayla oturumlar (FK ile) ve gönderdiği kullanılmamış davetler düşer, iz satırı ve F-6 kaldırılan dahil bütün yöneticilere gider. Yöneticinin e-posta değişikliği müşterinin rejimiyle (yeniden doğrulama, L-9, geri alma açıkken kapalı) çalışır; bekleyen değişiklik tablosuna Membership'in `IAdminPendingEmailChanges` yüzü üzerinden erişilir. Sonuç enum'ları `Contracts/Results` ad alanına, bağlantı tüketen iki yöntem `LinkKeySourceScan` listesine girdi; `AdminAccountRulesScanTests` kayıt, kurtarma ucu, `CreateAdminAsync` çağrı yeri ve rol adı kurallarını tarar. Birim 4864, Integration Model 1372 + Model dışı 1444 yerelde yeşil; yeni backlog D-593–D-595, D-464'ün T379 payı kapandı.

## 2026-11-08 — T389 doğrulama tur 3: ✓ PASS

2026-11-08 — T389 bağımsız doğrulama tur 3 PASS (dal `task/T389-hesap-durum-komutlari` ucu `2b73afed`, PR #472). Kabul kriterleri 1–9 ve KK1–KK9 ✓; tur 2'nin bulgusu (büyüyen küme bütün okunuyordu) ve iki K-notu kapandı; PR CI run 42516494053 yeşil; `VAL-A984` `PASS`. Yerel: build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 4742 + Consistency 280, betik testleri 4/4, Integration Commands 47, Web 594, Security 33. K1 (`ExposurePageOrders` 500 `09 §3.5` tablosunda yok) `D-658` olarak ileri alındı. Birleştirme notu: `origin/main` (`48dadb92`) ile `IAfterSalesModule.cs`'te kod çakışması; birleştirme geri alındı, kayıt birleştirmesiz yazıldı — merge öncesi yardımcı çakışmayı çözdürür.

## 2026-11-08 — T389 düzeltme 2 tamam, doğrulama bekliyor

2026-11-08 — T389 düzeltme 2 (PR #472): kartla ödenmiş kalemlerin okunması sipariş sayfalarıyla yapılır, bellekte tek sayfa (`09 §20` m.2); iş kuyruğu okuyucusu sayımı SQL'de alır; `D-601` hedefi T382. Sayfa boyutundan bağımsız sayı Integration, kaynak taraması Unit testlerle sınandı.

## 2026-11-08 — T389 doğrulama tur 2: ✗ FAIL

2026-11-08 — T389 bağımsız doğrulama tur 2 FAIL (dal `task/T389-hesap-durum-komutlari`, PR #472; dal `origin/main` `4c049223` ile birleştirildi, çakışma yok). Kabul kriterleri 1–9 ve KK1–KK9 ✓, tur 1'in iki bulgusu ve K1 kapandı, CI run 42505970515 yeşil; `VAL-A984` `PASS`. Tek bulgu: (1) ORTA S1 — `09 §25` Performans / `09 §20` madde 2: `OrderingModule.ListPaidCardItemsAsync` kartla ödenmiş bütün siparişlerin kalemlerini sayfasız `ToListAsync` ile belleğe alıyor (büyüyen küme), `CountCardOrdersWithRefundExposureAsync` süzmeyi bellekte yapıyor. K1: `JobQueueStatusReader` en eski zamanı bellekte buluyor (`MinAsync`); K2: `D-601` hedefi `T382` yazılmalı — ikisi düzeltme turunda.

## 2026-11-08 — T389 düzeltme 1 tamam, doğrulama tur 2 bekliyor

2026-11-08 — T389 düzeltme 1 (dal `task/T389-hesap-durum-komutlari`, PR #472; `origin/main` ile birleştirildi). (1) Komut kipinin eksik kayıtları giderildi: imaj işleyicisi web ile ortak `AddImageProcessing` çağrısıyla, `IHttpContextAccessor` komut kipine eklendi; dokuz modülün komut kipinde çözüldüğünü sınayan test eklendi. (2) `status` "Hatlar" bölümü: kart ödemesi ve Google ile giriş hatları web'in `ConditionalLines` kuralından, yalnız eksik anahtar adıyla yazılır; sır dosyası anahtarı komut sürecinde olmadığından "ölçülemez" (⚠, D-633). K1: `OutcomePending` kart iadesi teste eklendi. Unit 4926/4926, Integration TAM küme 2861/2861.

## 2026-11-08 — T389 doğrulama tur 1: ✗ FAIL

2026-11-08 — T389 bağımsız doğrulama tur 1 FAIL (dal `task/T389-hesap-durum-komutlari`, PR #472; dal `origin/main` ile birleştirildi). İki bulgu: (1) KRİTİK S2 — main ile birleşik hâlde `status` komutu `InvalidOperationException` ile 1 döner (`IAfterSalesModule` çözülürken `Shopfolio.Modules.Catalog.ImageUploads` → `ImageProcessor` komut kipinin hizmet koleksiyonunda kayıtlı değil, `CommandHost`); CI run 42495682161 "4. Integration test" 9 `status` testi kırmızı, yerelde birleşik dalda yeniden üretildi; (2) ORTA S1 — `05 §8.5.6`/§8.4.6 "açık ve kapalı hatlar" (koşullu ayarın açtığı/kapattığı kart ödemesi ve Google ile giriş hattı, `ConditionalLine`) `status` çıktısında yok, backlog kaydı da yok. `VAL-A984` `Durum` → FAIL. Kriter 1–5 ✓. Rapor `Docs/TASK_REPORTS/T389_REPORT.md` (Doğrulama bölümü).

## 2026-11-08 — T389 (Hesap ve durum yönetim komutları) yapım tamamlandı

2026-11-08 — T389 (Hesap ve durum yönetim komutları: oturumları sonlandırma, şifreyi geçersiz kılma, durum) yapım tamamlandı, doğrulama bekliyor (dal `task/T389-hesap-durum-komutlari`; kriter 1–9, KK1–KK9 öz denetimde ✓; `end-sessions` ve `invalidate-password` ortak `AccountTargetFlow` ile: e-posta terminalden → `IAdministrationModule.FindAdminIdAsync` → kayıt satırı → `ITransactionRunner` içinde iş (hesap kilidi, özet boşaltma, oturum silme); hesap yoksa satır boş kimlikle (`AuditLine.AdminCommandWithoutTarget`) ve çıkış 3; `status` sürümü, `StatusEvaluator.StandingsAsync` ile bütün koşulları (açık/kapalı/okunamayan/ölçülemez), iş kuyruğunu (`JobQueueStatusReader`), sağlayıcı anahtarı iki sayısını (`CountCardOrdersAwaitingPaymentAsync`, `CountOpenCardRefundsAsync`, `CountCardOrdersWithRefundExposureAsync`) ve `IDataConversion` kalanlarını yazar; üçü de işlem izine/bildirime/iş tablosuna yazmaz; `09 §13.2` madde 5 ve `§13.3` madde 5, `.claude/CONTEXT.md` güncellendi; ⚠ kararlar: hesap yoksa satır = boş kimlik, açık kart işi = bekleyen kart siparişi + bitmemiş kart iadesi, `status` bellek koşullarını "ölçülemez" yazar; yeni backlog D-598…D-602 (`DEFERRED_BACKLOG.d/T389.md`), D-541'e not (`DEFERRED_BACKLOG.d/T303.md`); rapor `Docs/TASK_REPORTS/T389_REPORT.md`).

## 2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 4 ✓ PASS (TAM yeniden doğrulama; head `f6412bf` + `origin/main` (T341, #433) birleştirmesi, PR #442; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; tur 3 BLOCKED (main kod çakışması) iki kod birleştirmesiyle giderildi; Unit 4231/4231 (Architecture, Consistency dahil), betik testleri 4/4, Integration `CategoryTreeAndListsTests` 33/33 (parçalı, gerçek PostgreSQL 18), `build -warnaserror` ve `dotnet format` temiz; dal CI 42425396677 success; bulgu 0; K1: D-332 hedefi tamamlanmış T339'dan T502'ye taşındı; `VAL-A001`…`A008`, `VAL-A181`…`A185` `BEKLEMEDE` kaldı (uç ve tarayıcı kanıtı sayfa task'larında); backlog aktif kalem yeniden sayıldı: 159). Önceki kayıtlar `STATUS_CHANGELOG.md`'de.
## 2026-11-08 — T336 (Fiyat ve KDV) doğrulama tur 4 ✓ PASS (PR #444)

2026-11-08 — T336 (Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri) doğrulama tur 4 ✓ PASS (head `32173b1` + `origin/main` (T317, `7097c01`) birleştirmesi, PR #444; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; tur 1–3 içerikte temiz, main ile kod çakışması nedeniyle BLOCKED idi; birleşik ağaçta Unit 4288 (Architecture 955, Consistency 241), betik testleri, Integration parçalı yeşil (PriceAndVat 19, StockAndCapacity + ProductGallery 45, ProductTransitions + SearchEngineData 59, Payment + OrderTransitions 64, ShipmentTransitions 94, dokunulan Model 220, Web/Concurrency 85); dal CI 42430213489 yeşil; para kodu elle sınandı; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-39 satırları T503/T525'in sayfa ve uç kanıtını ister); backlog aktif 165).

## 2026-11-08 — T336 (Fiyat ve KDV) doğrulama tur 3 BLOCKED, main (T339, T341) birleştirildi

2026-11-08 — T336 (Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri) doğrulama tur 3 içerikte temiz, main ile kod çakışması nedeniyle BLOCKED → `origin/main` (T339 `9bb46eb`, T341 `e2e9dbc`) dala birleştirildi (dal `task/T336-fiyat-kdv`, PR #444): `CatalogModule.cs`, `ICatalogModule.cs`, `SitemapSourceTests.cs`, `SearchEngineDataTests.cs` iki tarafın eklemeleriyle birlikte; T339'un `ProductGalleryTests` tohumu fiziksel ürüne üretim yeri alır (`.WithSeededOrigin()`, yalnız tohum; iki tarafın davranışı ve tutar/KDV mantığı değişmedi); birleşik dalda derleme `-warnaserror` 0 uyarı, format temiz, Unit 4286 / 4286 (Architecture 955, Consistency 241 dahil), Integration parçalı yeşil (ProductGallery 18, PriceAndVat + StockAndCapacity 46, ProductTransitions 41, SearchEngineData + SitemapEndpoint 28 ve fiziksel ürün tohumlayan on bir sınıf 359; yerel PostgreSQL 18); kriter 1–8 ve KK1–KK8 öz denetimde birleşik ağaçta yeniden ✓; backlog Aktif kalem satırlar sayılarak 164 (⚪ 161 + 🟡 3); doğrulama tur 4 bekliyor.

## 2026-11-08 — T336 (Fiyat ve KDV) doğrulama tur 2 BLOCKED, main (T321) birleştirildi

2026-11-08 — T336 (Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri) doğrulama tur 2 içerikte temiz, main ile kod çakışması nedeniyle BLOCKED → `origin/main` (T321, `a89710b`) dala birleştirildi (dal `task/T336-fiyat-kdv`, PR #444): `CatalogModule.cs` kurucusunda `_priceLabels` ve `_showcase` birlikte; T321'in `ProductTransitionsTests` tohumu fiziksel ürüne üretim yeri alır (`.WithSeededOrigin()`, yalnız tohum; iki tarafın davranışı ve tutar/KDV mantığı değişmedi); birleşik dalda derleme `-warnaserror` 0 uyarı, format temiz, Unit 4172 / 4172 (Architecture 933, Consistency 241 dahil), Integration parçalı yeşil (ProductTransitions 41, PriceAndVat + StockAndCapacity 46, SitemapEndpoint + SearchEngineData + ContentStatus 62; yerel PostgreSQL 18); kriter 1–8 ve KK1–KK8 öz denetimde birleşik ağaçta yeniden ✓; backlog Aktif kalem satırlar sayılarak 148 (⚪ 145 + 🟡 3); doğrulama tur 3 bekliyor. Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-08 — T358 (Kurumsal içerik tipleri) doğrulama tur 1 PASS

2026-11-08 — T358 (Kurumsal içerik tipleri: Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube, genel sayfa, sosyal bağlantılar) doğrulama tur 1 ✓ PASS (head `ec355d3` + `origin/main` (`7097c01`, T317, #441) birleştirmesi, PR #449; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓; Unit Architecture 955 + Consistency 241 + öteki 3090, betik testleri, Integration yerelde CorporateContent 64 (`ContentAuthoringTests` 19), ContentRecordAndBrandingTables + SearchEngineData 97, CompanyIdentity + SalesGate 46; dal CI 42430159867 yeşil; bulgu 0; K1 → D-440 (Architecture'da bir kez görülen, adı yakalanamayan düşme); `12` `Durum` değişmedi (KP-32/33/36/54 satırları uç ve tarayıcı kanıtı ister); backlog aktif 154). Önceki kayıt `STATUS_CHANGELOG.md`'de.
## 2026-11-08 — T302 (Öteki kayıtların süreyle imhası) doğrulama tur 1 PASS

2026-11-08 — T302 (Öteki kayıtların süreyle imhası) doğrulama tur 1 ✓ PASS (PR #446; kriter 1–9, KK1–KK9, `09 §25` ✓, güvenlik taraması temiz, bulgu 0, K 0; `VAL-D051` PASS; dal CI 42417352638 yeşil; `origin/main` (T321, #439) kod çakışması yalnız ekleme olarak çözüldü, birleşik ağaçta Unit 4123, Integration dokunulan sınıflar yeşil; backlog aktif 157). Üçüncü main birleştirmesi: `origin/main` (T341, #433) birleştirildi; `IOrderingModule` ve `OrderingModule` kurucusu iki tarafın eklemeleriyle birlikte korundu, Unit 4237, Integration RecordDisposal 13, OrderDisposal 27, Membership 34, OrderTransitions 31 yeşil; backlog aktif 157 aynı. Dördüncü main birleştirmesi: `origin/main` (T317 ödeme ekseni, #441) birleştirildi; `PaymentModule` kullanımları ve `OrderTransitionsTests` kurulumu (`OrderingTestServices.Build`) iki tarafın eklemeleriyle birlikte korundu, ödeme kodunun davranışı değişmedi; backlog aktif 158. Merge ve post-merge CI yardımcıda. Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 PASS

2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 ✓ PASS (head `24be2b5` + `origin/main` (`9bb46eb`, T339, #445) birleştirmesi, PR #441; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; tur 2 B1 giderildi — ödenmemiş iptalin tek yolu `CancelOrderAsync`, Ö1'in tek yolu ödeme ekseni (D-389); Unit Architecture 933 + Consistency 241 + öteki 2942, betik testleri, Integration yerelde PaymentTransitions 33, OrderTransitions 31, ShipmentTransitions 94, Disposal 45 ve 4 geçici yoklama; dal CI 42421489591 yeşil; bulgu 0; K1 bu PR'da kapatıldı (D-291 metni güncel yola ve test adına bağlandı); `12` `Durum` değişmedi (KP-18 satırları sayfa ve uç kanıtı ister); backlog aktif 154).

## 2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `b2c6cca`, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 giderildi — kayıt e-postası `InvalidEmailAttribute` ile denetlenir; Unit 4069 (Consistency 241), Integration MembershipTests 34/34 ve komşu üç sınıf 95/95; dal CI 42410595147 success; `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (uç yanıtı sonraki task'larda); K1 → D-385 (T509). `origin/main` (T334) birleştirildi: `Modules/Ordering` iki dosyasında iki tarafın üyeleri birlikte korundu; backlog aktif sayısı yeniden sayıldı: 153; ikinci main birleştirmesi: `origin/main` (T321, #439) de birleştirildi, `OrderingModule` kurucusu ve `IOrderingModule` üyeleri iki tarafın eklemeleriyle birlikte korundu).

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153).

## 2026-11-08 — T302 (Öteki kayıtların süreyle imhası) yapım tamamlandı

2026-11-08 — T302 (Öteki kayıtların süreyle imhası) yapım tamamlandı, doğrulama bekliyor (dal `task/T302-kayit-imha`, PR #446; kriter 1–9, KK1–KK9 öz denetimde ✓; Unit 4110 (Architecture 933, Consistency 242, Disposal 23), Integration RecordDisposal 13, Disposal 18, OrderDisposal 27, Files 81, Ordering 125, Concurrency 26, Model 1106, Database 36 yeşil; format temiz; şema yok; `12` `Durum` değişmedi; ⚠ gönderim kaydının kolu yalnız ödeme kaydından okunur, siparişi kilitlemez; ayıp talebi silmesi sipariş sürümünü artırmaz; gelen bildirim 10 yıl tasarım değeridir (09 §3.5 satırı eklendi); `VersionedFormPageTests` sınıf birlikte koşunca titriyor (D-378); backlog D-102 ✅, D-99 T302 payı ✅, yeni D-374…D-378, aktif 143). Önceki kayıt `STATUS_CHANGELOG.md`'de.
## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 ✓ PASS (head `24be2b5` + `origin/main` (`9bb46eb`, T339, #445) birleştirmesi, PR #441; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; tur 2 B1 giderildi — ödenmemiş iptalin tek yolu `CancelOrderAsync`, Ö1'in tek yolu ödeme ekseni (D-389); Unit Architecture 933 + Consistency 241 + öteki 2942, betik testleri, Integration yerelde PaymentTransitions 33, OrderTransitions 31, ShipmentTransitions 94, Disposal 45 ve 4 geçici yoklama; dal CI 42421489591 yeşil; bulgu 0; K1 bu PR'da kapatıldı (D-291 metni güncel yola ve test adına bağlandı); `12` `Durum` değişmedi (KP-18 satırları sayfa ve uç kanıtı ister); backlog aktif 154).

## 2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 PASS

2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `b2c6cca`, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 giderildi — kayıt e-postası `InvalidEmailAttribute` ile denetlenir; Unit 4069 (Consistency 241), Integration MembershipTests 34/34 ve komşu üç sınıf 95/95; dal CI 42410595147 success; `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (uç yanıtı sonraki task'larda); K1 → D-385 (T509). `origin/main` (T334) birleştirildi: `Modules/Ordering` iki dosyasında iki tarafın üyeleri birlikte korundu; backlog aktif sayısı yeniden sayıldı: 153; ikinci main birleştirmesi: `origin/main` (T321, #439) de birleştirildi, `OrderingModule` kurucusu ve `IOrderingModule` üyeleri iki tarafın eklemeleriyle birlikte korundu).

---

## 2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) ikinci kod birleştirmesi (T339)

2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) `origin/main` (T339, #445) ile ikinci kod birleştirmesi yapıldı ⏳ yeniden doğrulama bekliyor (PR #442; `CatalogModule`, `ICatalogModule` ve `SitemapSourceTests` çakışmaları iki tarafın eklemesiyle çözüldü, `CategoryTreeAndListsTests` tohumu main'in özel `Description` ayarlayıcısı için `SetDescription` çağırır; birleşik ağaçta Integration `CategoryTreeAndListsTests` 33/33, `ProductGalleryTests` 18/18, `ProductTransitionsTests` 41/41; Unit Architecture 946, Consistency 241, kalan 3044; `build -warnaserror` ve `dotnet format` temiz; backlog aktif kalem 158). Önceki kayıtlar `STATUS_CHANGELOG.md`'de.

---

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 PASS

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) kod birleştirmesi (T321, T334)

2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) `origin/main` (T321, T334) ile kod birleştirmesi yapıldı ⏳ yeniden doğrulama bekliyor (PR #442; kurucu çakışması iki tarafın eklemesiyle çözüldü, vitrin görünürlüğü `ShowcaseVisibility.Listed()` tek yerine bağlandı, backlog aktif kalem 150; önceki durum: düzeltme 2 bitti, doğrulama tur 3 bekliyordu; tur 2 ✗ FAIL B1 ve B2 (DÜŞÜK S1) giderildi: B1 `ProductListReader` sayfayı artık yalnız kartın sütunlarıyla `Select` projeksiyonuyla okuyor (`init` üyeli özel sınıf; sıra `CreatedAt` azalan, `Id` azalan aynı), D-134 ✅; B2 `TheListsQueryCount_DoesNotGrowWithThePageFill_…` komut yakalayıcıyla 1 ürünlü ve dolu sayfada sorgu sayısının eşitliğini sınar; `origin/main` (T334) birleşince tohum uyarlandı (Physical varyantta stok sayısı zorunlu, sınırsız varyant için Digital ürün); Integration `CategoryTreeAndListsTests` 31/31 + iki sınır testi ayrı 1/1 + 1/1; Unit Architecture 936, Consistency 241, Catalog 61; backlog aktif kalem 144; `12` değişmedi). Önceki kayıtlar `STATUS_CHANGELOG.md`'de.
## 2026-11-08 — T341 (Üyelik) doğrulama tur 3 PASS

2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `b2c6cca`, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 giderildi — kayıt e-postası `InvalidEmailAttribute` ile denetlenir; Unit 4069 (Consistency 241), Integration MembershipTests 34/34 ve komşu üç sınıf 95/95; dal CI 42410595147 success; `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (uç yanıtı sonraki task'larda); K1 → D-385 (T509). `origin/main` (T334) birleştirildi: `Modules/Ordering` iki dosyasında iki tarafın üyeleri birlikte korundu; backlog aktif sayısı yeniden sayıldı: 153; ikinci main birleştirmesi: `origin/main` (T321, #439) de birleştirildi, `OrderingModule` kurucusu ve `IOrderingModule` üyeleri iki tarafın eklemeleriyle birlikte korundu).

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 PASS

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153).

## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) düzeltme turu 3 yapıldı

2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) düzeltme turu 3 yapıldı (PR #441; tur 2 B1 giderildi: ödenmemiş iptalin tek yolu T316'nın `CancelOrderAsync`'i — `CancelUnpaidOrderAsync`, `UnpaidOrderCancellation` ve kendi denetimi kaldırıldı, KK4 testleri `CancelOrderAsync` üzerinden; onaysız yönetici iptali `ConfirmationRequired`, "Diğer" dışı notlu / "Diğer" notsuz yönetici iptali `InvalidInput` sınandı; K1/D-389 kapandı: `StartPreparationAsync` yalnız S1, Ö1'in tek yolu ödeme kaydı yazan `ApplyCardPaymentAsync`/`MarkTransferPaidAsync`, D-307 T318'e daraldı; `origin/main` `a89710b` (T321) birleştirildi, kayıt çakışmaları çözüldü; birleşik ağaçta build `-warnaserror` 0/0, format temiz, Unit 4116, Integration Payment 33, Shipment + OrderTransitions 125, Disposal + Concurrency 71 (postgres:18); backlog aktif 146). Doğrulama tur 3 bekliyor.

## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 2 FAIL

2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 2 ✗ FAIL (head `66bcf16` — T316 ile kod birleştirmesi, `origin/main` `af6e7d3` güncel, PR #441; kriter 1–8 ve KK1–KK8 ✓, `09 §25` ✗, güvenlik taraması temiz; B1 (ORTA, S1): birleştirmeden sonra ödenmemiş iptalin iki açık yolu var, `CancelUnpaidOrderAsync`'in yönetici yolu firma iptalini (S3) geri alınamaz onaysız ve "Diğer ⇔ not" kuralı denetlenmeden yazar (`02 §5.9`, `03 §1.6.1.3`, `06 §4.19`); K1 → D-389 (Ö1'in iki yolu, D-307); Unit 3862 + Consistency 241, betik testleri, Integration Payment+OrderTransitions 60, Shipment 94, Disposal 45; dal CI 42414305022 yeşil; `12` `Durum` değişmedi; backlog aktif 141).
## 2026-11-08 — T339 doğrulama tur 2 ✓ PASS (PR #445)

T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-08 — T358 (Kurumsal içerik tipleri) yapım bitti

2026-11-08 — T358 (Kurumsal içerik tipleri: Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube, genel sayfa, sosyal bağlantılar) yapım tamamlandı, doğrulama bekliyor (dal `task/T358-kurumsal-icerik`, PR #449; kriter 1–10, KK1–KK10 ve `09 §25` öz denetimde ✓; Unit 4286 (Consistency ve Architecture dahil), Integration CorporateContent 64, format ve derleme `-warnaserror` temiz; şema yok; D-360 ✅; yeni backlog yok, aktif 152; `12` `Durum` değişmedi).

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) düzeltme turu 1 yapıldı

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) düzeltme turu 1 yapıldı, doğrulama tur 2 bekliyor (PR #445; `origin/main` (`af6e7d3`, T334) dala birleştirildi; B1 giderildi: `ProductGalleryTests.cs` üç tohum `Variant.Create(..., Now, 100)` ile stoklu, ürün koduna dokunulmadı; K2 → D-384 kapatıldı (test adı `…WhenTheFormMarksNone`), K1 → D-383 açık; kayıt belgesi çakışmaları çözüldü; backlog aktif 147; yeni K-notları D-391, D-392).

---

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 1 FAIL

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 1 ✗ FAIL (head `7d65caf`, PR #445; dalın kendi tabanında kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓, dal CI 42411851018 yeşil, Unit 922 + 241 + 2996, Integration `ProductGalleryTests` 18, Concurrency 26, `SearchEngineDataTests` 18; B1 ORTA S2: `origin/main` (`af6e7d3`, T334) birleştirilince `ProductGalleryTests` 14/18 düşer — T334'ün `VariantStockGuard`'ı fiziksel varyantta stok numarası ister, testin üç tohumu stoksuz varyant kurar; birleştirme push edilmedi; düzeltme: birleştir + `Variant.Create(..., Now, 100)`; K1 → D-383, K2 → D-384; `12` `Durum` değişmedi (satırlar uç yanıtı ve tarayıcı gözlemi ister — T503, T525); backlog aktif 142).

---

## 2026-11-07 — T336 (Fiyat ve KDV) doğrulama tur 1 BLOCKED, main (T334) birleştirildi

2026-11-07 — T336 (Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri) yapım tamamlandı, doğrulama bekliyor (dal `task/T336-fiyat-kdv`, PR #444; kriter 1–8, KK1–KK8 öz denetimde ✓; Unit 4115, Integration 2133 (yeni PriceAndVat 19), format ve derleme `-warnaserror` temiz; şema yok; `12` `Durum` değişmedi; D-76 ✅, D-74 kısmi, yeni backlog D-344…D-346; `origin/main` (T316) birleştirildi; doğrulama tur 1 içerikte temiz, main ile kod çakışması nedeniyle BLOCKED → `origin/main` (T334) birleştirildi: kesici listesinde `PriceLabelGuard` ve `VariantStockGuard` birlikte, dört entegrasyon testinin tohumu iki tarafı birlikte taşır, `StockAndCapacityTests` fiziksel ürüne üretim yeri, `PriceAndVatTests` fiziksel varyanta stok adedi alır (yalnız tohum); birleşik dalda Unit 4159, Integration 2267 parçalı yeşil; doğrulayıcı K1 → D-381 (T525); backlog Aktif kalem 142). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 3 PASS

2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 3 ✓ PASS (head `c5b9afd` + `origin/main` (T316, #437) birleştirmesi, PR #439; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 — `IOrderingModule` sepet metotlarının kendi `SaveChangesAsync`'i — giderildi: iki metot yalnız izlenen değişiklik bırakır, kaydı `ITransactionRunner` yapar (`TheCartMethods_DoNotSaveOnTheirOwn_...KK7`); bulgu 0, yeni K-notu 0; dal CI 42406697716 yeşil; yerelde derleme `-warnaserror` 0 uyarı, biçim üç proje temiz, Unit 4062/4062 (Architecture ve Consistency dahil), betik testleri 4/4, Integration `ProductTransitionsTests` 41, Sitemap + SearchEngine + Scheduler + ContentStatus 65, OrderTransitions + Concurrency 57 (postgres:18); `12` KP-7/KP-40 satırları uç yanıtı, panel ya da adres sayfası ister → `BEKLEMEDE` kalır; kayıt belgeleri çakışması `origin/main` ile çözüldü, backlog Aktif kalem 145). Merge ve post-merge CI yardımcıda.

## 2026-11-07 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 1 PASS

2026-11-07 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 1 ✓ PASS (head `0926ba7`, `origin/main` `c4c872c` güncel, PR #441; kriter 1–8, KK1–KK8, `09 §25` ✓, güvenlik taraması temiz; Unit 3965 (Architecture 906, Consistency 230, öteki 2829), betik testleri, Integration yerelde Ordering 60, Concurrency 26, PaymentRecordTables + JobCleanupAndRefund 92, web açılışı 18; dal CI 42396093293 yeşil; bulgu 0, K-notu 1 (D-322 → T370); `12` `Durum` değişmedi (KP-18 satırları sipariş sayfası kanıtı ister); backlog aktif 140).
## 2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 PASS

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); Integration yerelde StockAndCapacity 27 ve dokunulan sınıflar; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi; backlog aktif sayısı yeniden sayıldı: 139).
2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); betik testleri; Integration yerelde StockAndCapacity 27, CatalogCore+SalesModel 88, Concurrency+ImageFiles+CartAndOrderCore+StockReservation 133, AppCache+SearchEngineData+VersionedFormPage 53 (yapım raporundaki `VersionedFormPageTests` düşmesi yinelenmedi), birleştirme sonrası NotificationMapQueue+SalesModel 31; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi (satırların kanıtı uç yanıtı, tarayıcı gözlemi ya da ödeme onayı — T503, T525, T348); backlog aktif sayısı yeniden sayıldı: 139). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T341 (Üyelik) doğrulama tur 2 FAIL

2026-11-07 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 2 ✗ FAIL (CI düzeltmesi sonrası tam yeniden doğrulama; head `2aa0153`, PR #433; kriter 1–6, 8, 9 ve KK1–KK9 ✓, kriter 7 ~ Kısmi; `09 §25` bir "hayır": Bulgu 1 ORTA S1 — `MembershipModule.RegisterAsync` e-posta biçimini `04` 7.3.2.1'in tek kuralı `InvalidEmailAttribute` yerine çatının `MailAddress` ayrıştırıcısıyla kurulan ayrı bir `IsValidEmail` ile denetliyor (`09 §3.4` m.1, §24 m.4); iki kural iki yönde ayrışıyor (`a..b@example.com` kuralın kabul ettiği adres kayıtta red, 70 karakterlik yerel kısım, `_`'lı ve `-` ile başlayan alan adı, boşluklu tırnaklı yerel kısım kayıtta kabul); build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 4037 (Architecture 918, Consistency 230, öteki 2889), betik testleri, Integration yerelde MembershipTests 29 + komşu dört sınıf 109; dal CI 42404071089 yeşil; K-notu 2: K1 D-364 (`09 §15.1` Test: hesap yokken sahte özetle doğrulamanın sayaçlı dekoratör testi yok → T342), K2 D-365 (`PasswordService.IterationCount` 210000 `09`'da yazılı değil → F3 gate); `12` `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı; `origin/main` (T328) birleştirildi; backlog aktif 136). Düzeltme r2 yapıldı (Bulgu 1: `IsValidEmail` kaldırıldı, kayıt e-postası `InvalidEmailAttribute` ile; `a..b@example.com` kabul, 70 karakterlik yerel kısım, `_`'lı ve `-` ile başlayan alan adı, boşluklu tırnaklı yerel kısım ret testleri; Integration MembershipTests 34, AccountEmailQueueTests 14; D-379 gönderim tarafı; backlog aktif 142), tur 3 bekliyor. Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 PASS

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 ✓ PASS (head `2d04809` + `origin/main` birleştirmesi, PR #437; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 1 bulgusu B1 giderildi ve S3, S4, S9 için sınandı; bulgu 0, K-notu 0; dal CI 42399324842 yeşil; Unit Architecture 906 + Consistency 230 + öteki 2840, Integration `ShipmentTransitionsTests` 94, `OrderTransitionsTests` 31, `ConcurrencyTests` 26; `12` KP-46 satırları `BEKLEMEDE` kalır). Merge ve post-merge CI yardımcıda.

## 2026-11-07 — T328 (Bildirim haritası) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

## 2026-11-07 — T341 (Üyelik) doğrulama tur 1 PASS

2026-11-07 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 1 ✓ PASS (head `391cdc3` + `origin/main` birleştirmesi, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓ (ekran, POST ve sipariş ucu yüzleri modül düzeyinde; bağlar D-254), güvenlik taraması temiz; build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 3534 (Architecture 842, Consistency 213, öteki 2479), betik testleri, Integration yerelde MembershipTests 29, SessionLinkAttemptLoginTables + LinkRecords + AccountEmailQueue 95, AppCache 14; dal CI 42378956325 yeşil; bulgu 0; K-notu 3: K1 D-252 hedefi T309 → T514, K2 D-289 (P-12 iki sabitte; CI düzeltmesinde tek sabite indi, ✅), K3 D-290 (`09 §15.1` m.5 metni); `12` `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (sipariş geçmişinin uç yanıtı sonraki task'larda); CI düzeltmesi: main'e giren T381 yüzünden `GateOnlySettings` yeni üyeleri uyguladı ve P-12 için tek sabit `EmailVerificationLinkHours` kullanıldı; backlog aktif 132).
---

## 2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 PASS

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); betik testleri; Integration yerelde StockAndCapacity 27, CatalogCore+SalesModel 88, Concurrency+ImageFiles+CartAndOrderCore+StockReservation 133, AppCache+SearchEngineData+VersionedFormPage 53 (yapım raporundaki `VersionedFormPageTests` düşmesi yinelenmedi), birleştirme sonrası NotificationMapQueue+SalesModel 31; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi (satırların kanıtı uç yanıtı, tarayıcı gözlemi ya da ödeme onayı — T503, T525, T348); backlog aktif sayısı yeniden sayıldı: 139).

---

## 2026-11-07 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) yapım bitti

2026-11-07 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) yapım tamamlandı, doğrulama bekliyor (dal `task/T339-urun-gorsel-metin`, PR #445; kriter 1–9, KK1–KK9 öz denetimde ✓; Unit 4159 (Architecture + Consistency dahil), Integration `ProductGalleryTests` 18, Model 690, Concurrency 26, `VersionedFormPageTests` 21, `SearchEngineDataTests` 18; derleme `-warnaserror` ve biçim üç proje temiz; şema yok; yeni bağımlılık `HtmlSanitizer` 10.0.1120 (K-1577, 09 tablosu 9.0.892 yerine; AngleSharp açığı nedeniyle); kapı: kurumsal uzun metin ve yasal metin yazım işlevleri temizleyiciyi o task'larda çağıracak (D-360, D-219); yeni backlog D-357…D-361, Aktif kalem 140).

---
## 2026-11-07 — T334 (Stok ve hizmet kontenjanı) doğrulama tur 1 PASS

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); betik testleri; Integration yerelde StockAndCapacity 27, CatalogCore+SalesModel 88, Concurrency+ImageFiles+CartAndOrderCore+StockReservation 133, AppCache+SearchEngineData+VersionedFormPage 53 (yapım raporundaki `VersionedFormPageTests` düşmesi yinelenmedi), birleştirme sonrası NotificationMapQueue+SalesModel 31; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi (satırların kanıtı uç yanıtı, tarayıcı gözlemi ya da ödeme onayı — T503, T525, T348); backlog aktif sayısı yeniden sayıldı: 139). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 2 FAIL

2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 2 ✗ FAIL (head `d42a5a4` + `origin/main` birleştirmesi, PR #442; kriter 1–8 ve KK1–KK8 ✓, tur 1 B1 düzeltmesi doğrulandı (iki listede sayfa sınırı testleri yeşil); güvenlik taraması temiz; Unit 4064, Integration `CategoryTreeAndListsTests` 32/32 + `ConcurrencyTests` 26/26; dal CI 42406747948 yeşil). **Bulgular:** B1 DÜŞÜK S1 — `09 §25` Performans "okuma sorgusu projeksiyonlu mu" hayır: `ProductListReader` sayfayı `Product` varlığının bütün sütunlarıyla (açıklama gövdesi dahil) okur, `09 §20` madde 3 yalnız gösterilen sütunların `Select` ile alınmasını ister (D-134 bunu T333'e yönlendirmişti); B2 DÜŞÜK S1 — `09 §20` madde 4: varyant rakamlarını yükleyen liste için sorgu sayısının sayfadaki ürün sayısından bağımsız olduğunu komut yakalayıcıyla sınayan entegrasyon testi yok. K1 → D-372 (ürün–kategori bağı değişince menü/ağaç önbellek alanı bildirimi, T361). Düzeltme 2 gerekli: B1 + B2 (ve D-134 kapanışı); iterasyon 1/2 kullanıldı (`12 §8.5`).
## 2026-11-07 — T321 (Ürün yayın durumları) düzeltme 2 yapıldı

2026-11-07 — T321 (Ürün yayın durumları) düzeltme 2 yapıldı (tur 2 Bulgu 1: `IOrderingModule.RemoveFromCartsAsync` ve `TakeRemovedItemNoticesAsync` artık `SaveChangesAsync` çağırmaz — kaydı işlemin sahibi (`ITransactionRunner`) yapar, `09 §6` m.3, m.7; yeni test `TheCartMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK7`; `TakeNoticesAsync` test yardımcısı `ITransactionRunner` üzerinden; `origin/main` (T328, #440) birleştirildi; 11'deki kriterler ve KK1–KK9 düzeltilmiş diff'te yeniden denetlendi; doğrulama tur 3 bekliyor; backlog aktif 136). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 PASS

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 ✓ PASS (head `2d04809` + `origin/main` birleştirmesi, PR #437; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 1 bulgusu B1 — yöneticinin bütün sipariş iptali teslim işaretli kalemi denetlemiyordu — giderildi ve S3, S4, S9 için sınandı; bulgu 0, K-notu 0; dal CI 42399324842 yeşil; yerelde derleme `-warnaserror` 0 uyarı, biçim üç proje temiz, Unit Architecture 906 + Consistency 230 + öteki 2840, betik testleri 4/4, Integration `ShipmentTransitionsTests` 94, `OrderTransitionsTests` 31, `ConcurrencyTests` 26 (postgres:18); `12` KP-46 satırları uç yanıtı ister → `BEKLEMEDE` kalır; kayıt belgeleri çakışması `origin/main` (T322) ile çözüldü, backlog Aktif kalem 135). Merge ve post-merge CI yardımcıda.

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) düzeltme 1 bitti

2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) düzeltme 1 bitti ⏳ doğrulama tur 2 bekliyor (PR #442; tur 1 ✗ FAIL B1 DÜŞÜK S1: kategori sayfası ve arama listesi için eşit `CreatedAt`'li ürünleri sayfa sınırına düşüren test yoktu → eklendi: iki liste için gerçek okuyucuyla bütün sayfalar yürünür, yinelenme ve kayıp yok, son sayfa dahil, süzgeçli koşular dahil; arama sözcüğündeki ters bölü harfi harfine aranır; üretim kodu değişmedi; `origin/main` birleştirildi; build, format ×3, Unit 4064, Integration `CategoryTreeAndListsTests` 32 + `ConcurrencyTests` 26 yerelde yeşil; `12` `Durum` değişmedi; backlog aktif 135). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T328 (Bildirim haritası) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 1 FAIL

2026-11-07 — T333 doğrulama tur 1 ✗ FAIL (head `62d03a8` + `origin/main` birleştirmesi, PR #442; kriter 1–8 ve KK1–KK8 ✓, güvenlik taraması temiz; `09 §25` "yeni sayfalı liste sınır testi" HAYIR → B1 DÜŞÜK S1: iki yeni sayfalı liste (kategori sayfası, arama) için eşit `CreatedAt`'li ürünleri sayfa sınırına düşüren test yok (`09 §10.5` madde 6, §19.2 madde 5); build, format, Unit 3983, betik testleri, Integration `CategoryTreeAndListsTests` 29 + `ConcurrencyTests` 26 yerelde yeşil, dal CI 42401392964 yeşil; `12` `Durum` değişmedi; K1 rapor cümlesi düzeltildi; backlog aktif 133).

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) yapım bitti

2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) yapım bitti ⏳ doğrulama bekliyor (dal `task/T333-kategori-arama-suzgec`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 3983, Integration `CategoryTreeAndListsTests` 29, Model ad alanı 1105; şema değişmedi; backlog D-330…D-334, aktif 130; T332 Commit hücresi `c4c872c`).
## 2026-11-07 — T328 (Bildirim haritası) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 2 FAIL

2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 2 ✗ FAIL (head `921491a` + `origin/main` (T322, #436) birleştirmesi, PR #439; kriter 1–9 ve KK1–KK9 ✓, tur 1'in iki bulgusu kapandı; `09 §25` bir "hayır": Bulgu 1 DÜŞÜK S1 — `IOrderingModule` arayüz metotları `RemoveFromCartsAsync` ve `TakeRemovedItemNoticesAsync` `SaveChangesAsync` çağırıyor (`09 §6` m.3, m.7); güvenlik taraması temiz; Unit Consistency 230 + Architecture 906 + öteki 2845, betik testleri, Integration yerelde ProductTransitions 40, Catalog + Ordering + Concurrency 97, Sitemap + SearchEngineData + Scheduler 31, Web host 77 (birleştirme öncesi `921491a`); dal CI 42399951671 yeşil; K-notu 1 (D-353 → T502: sayfalı liste projeksiyonsuz); `12` `Durum` değişmedi (KP-7/KP-40 satırları uç ve panel kanıtı ister); iki iterasyon sınırı doldu — eskalasyon (`12 §8.5`); backlog aktif 134). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 1 FAIL, düzeltme yapıldı

2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 1 ✗ FAIL (head `8d71d89`, PR #439; kriter 1–9 ve KK1–KK9 ✓, `09 §25` iki "hayır": Bulgu 1 DÜŞÜK S1 — yeni sayfalı vitrin listesi `ShowcaseReader.ListAsync` sınır testi olmadan eklendi (`09 §19.2` m.5, §10.5 m.6); Bulgu 2 DÜŞÜK S1 — `OrderingModule.TakeRemovedItemNoticesAsync` `DbUpdateConcurrencyException`'ı kayıt varyantı dışında ret olarak yakalıyor (`09 §10.2` m.2); güvenlik taraması temiz; Unit Architecture 906 + öteki 3070, betik testleri, Integration yerelde ProductTransitions 35 + SitemapEndpoint 10; dal CI 42394867534 yeşil; K-notu yok; `12` `Durum` değişmedi (KP-7/KP-40 satırları uç ve panel kanıtı ister); backlog aktif 130). Düzeltme yapıldı (iki bulgu: vitrin listesi sınır testleri, `TakeRemovedItemNoticesAsync` sepet kilidi altında ve yakalamasız; Integration ProductTransitions 40, Unit 906 + 3075), doğrulama tur 2 bekliyor.
---

## 2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

## 2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) yapım bitti

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) yapım tamamlandı, doğrulama bekliyor (dal `task/T334-stok-kontenjan`; kriter 1–8, KK1–KK8 öz denetimde ✓; Unit 4069 (Consistency dahil), Integration StockAndCapacity 27 ve ilgili Model/Concurrency/Ordering/Jobs sınıfları yeşil, format ve derleme `-warnaserror` temiz; şema yok; `12` `Durum` değişmedi; ⚠ `VersionedFormPageTests` OrderPost_..._KK8 sınıf birlikte koşunca düşüyor, tek başına geçiyor, T334'ten bağımsız görünüyor; yeni backlog D-335…D-337, aktif 131; T301, T307, T315, T322, T332, T366, T381 merge kaydı dolduruldu).
T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

---

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 1 FAIL

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 1 ✗ FAIL (head `620b368` + `origin/main` birleştirmesi, PR #437; kriter 5 ~: yöneticinin bütün sipariş iptali (S3/S4/S9) teslim işaretli kalemi denetlemez ve teslim edilmiş dijital kalemli siparişi İptal edildi'ye alır — `02 §5.4`, §7.2.2, §7.2.5 (B1 ORTA S1); öteki kriterler, KK1–KK4, KK6–KK9 ve `09 §25` ✓, güvenlik taraması temiz; Unit Architecture 868 + Consistency 230 + öteki 2809, Integration Ordering 122, betik testleri ✓; dal CI 42393227539 yeşil; `12` `Durum` değişmedi (KP-46 satırları uç yanıtı ister); backlog aktif 130). Düzeltme r1 yapıldı (B1: teslim işaretli kalemli sipariş bütün sipariş iptaliyle İptal edildi'ye geçmez; S3/S4/S9 testleri; Integration Ordering 125), tur 2 bekliyor. Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T332 (Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu) doğrulama tur 1 PASS

2026-11-07 — T332 (Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu) doğrulama tur 1 ✓ PASS (head `48db1e8` + `origin/main` (T309) birleştirmesi, PR #432; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3585 (birleştirme sonrası), betik testleri, Integration yerelde SalesModel 28, AccountEmailQueue 14, ImageDisposal + Sitemap 10; dal CI 42376259158 yeşil; bulgu 0, K-notu 1 (D-283 → T525); `12` `Durum` değişmedi (KP-4/KP-5/KP-6/KP-39 satırları vitrin ve panel kanıtı ister); backlog aktif 125).
## 2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) doğrulama tur 1 PASS

2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) doğrulama tur 1 ✓ PASS (head `106c8ae`, PR #438; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3790 + Consistency 230, betik testleri, Integration yerelde CorporateContent 45; dal CI 42394560527 yeşil; bulgu 0, K-notu 0; `12` `Durum` değişmedi (T322'nin KP satırı yok); backlog aktif 128).

## 2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) yapım bitti

2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) yapım tamamlandı, doğrulama bekliyor (dal `task/T322-icerik-iletisim-durumlari`; kriter 1–9, KK1–KK9 öz denetimde ✓; Unit Consistency 230, Architecture 906, CorporateContent 57, Integration CorporateContent 45, format ve derleme `-warnaserror` temiz; şema yok; `12` `Durum` değişmedi; yeni backlog D-315…D-317, aktif 128).

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) yapım bitti

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) yapım bitti, doğrulama bekliyor (dal `task/T316-sevkiyat-ekseni`, PR #437; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit Architecture 868 + öteki 3039, Integration Ordering 122 (ShipmentTransitionsTests 91); backlog D-305…D-309 yeni, D-291 (b) sevkiyat ekseni için kapandı, aktif 125).

## 2026-11-07 — T301 (Siparişin imha parçası) doğrulama tur 1 PASS

2026-11-07 — T301 (Siparişin imha parçası) doğrulama tur 1 ✓ PASS (head `78b8449` + `origin/main` birleştirmesi, PR #431; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3469 (Architecture 830, Consistency 209, öteki 2430), betik testleri, Integration yerelde OrderDisposal 27, Disposal + ModuleCallTransaction 21, CookiePolicy + LoggingFiles 25; dal CI 42376054521 yeşil; bulgu 0, K-notu 0; `12` `Durum` değişmedi (KP-74'ün T301'e düşen satırı `VAL-D047` zaten PASS); backlog aktif 125).

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 2 PASS

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 2 ✓ PASS (head `25e420c`, PR #430; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Unit Consistency 219, Architecture 838, öteki 2529; betik testleri; Integration yerelde FirmSettings + FirmSettingDefaults 24, CompanyIdentity + SalesGate + StartupData 71; dal CI 42381345942 yeşil; bulgu 0, K-notu 1 (rapordaki eski metin, bu PR'da kapandı); `12` `Durum` değişmedi (KP-64 satırları ekran/ürün formu/uçtan uca — BEKLEMEDE); backlog aktif 120, main ile birleşik).

## 2026-11-07 — T315 (Durum makinesi çekirdeği) doğrulama tur 1 PASS

2026-11-07 — T315 (Durum makinesi çekirdeği) doğrulama tur 1 ✓ PASS (head `0d30a6f` + `origin/main` birleştirmesi, PR #434; kriter 1–8 ve KK1–KK8 ✓, bulgu yok; dal CI 42381390535 yeşil; Unit 3570 (Architecture 844, Consistency 224), Integration OrderTransitionsTests 31; `12`'de T315 satırı yok; K1 → D-291 (S1'in sistem aktörü ve havale onayı; yönetici geçişinde sürüm — T316/T317), backlog aktif 118).

## 2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) doğrulama tur 1 PASS

2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) doğrulama tur 1 ✓ PASS (head `43be541` + `origin/main` birleştirmesi, PR #435; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓, bulgu yok; Unit 3536 + Consistency 213 (birleştirme sonrası Unit 3572), betik testleri, Integration yerelde parçalı (CustomerNoticeAfterOrderQueueTests + CustomerNoticeQueueTests 22) geçti, dal CI 42385697297 yeşil; `12` `Durum` değişmedi (A321–A323 gönderimi ve tetiği ister — T305/T306 emsali); K1 → D-293 (gönderen ve yeniden gönderim yazıcısı: il adları, B-9 bağlı kaydı, iade adresi koşulu); backlog aktif 116). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) yapım bitti

2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) yapım tamamlandı, doğrulama bekliyor (PR #435; kriter 1–10, KK1–KK10 öz denetimde ✓; Unit 3749, Integration CustomerNotice 22, format ve derleme `-warnaserror` temiz; `12` `Durum` değişmedi (A324, A335 gönderimi ister); yeni backlog kaydı yok, aktif 108).

## 2026-11-07 — T366 (Yasal metinler) doğrulama tur 2

2026-11-07 — T366 (Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin) doğrulama tur 2 ✓ PASS (head `171fc5d`, PR #428; tam yeniden doğrulama: kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓ — tur 1 bulgusu kapandı: aydınlatma kimlik eksikken yayımlanmaz, yer tutucular gösterimde güncel kimlikle HTML kodlu dolar; 4, 5, 6 modül düzeyinde; Unit 3500, betik testleri, Integration yerelde parçalı 121 geçti, dal CI 42371931110 yeşil; `12` `Durum` değişmedi; K1 → D-268 (T356), K2 → D-269 (T507); backlog aktif 115).
## 2026-11-07 — T341 (Üyelik) yapım bitti

2026-11-07 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) yapım bitti, PR #433 açık, doğrulama bekliyor (task `⏳ Devam ediyor`; Unit 3477 (Architecture dahil), Integration yerelde MembershipTests 29 + SessionLinkAttemptLoginTables/LinkRecords/Disposal 99 geçti; modül yüzü: kayıt, doğrulama bağlantısı okuma ve doğrulama, yeniden gönderim, şifre ve kapı denetimi, sipariş hesabı çözümü, misafir siparişlerinin bağlanması, hesap adı, giriş kaydı; ekran ve POST bağları D-254, şifre politikası D-251, e-posta gönderimi D-252, gece silmesi D-253, L-1 ve oturum D-255; D-64 ve D-96 kapandı, backlog aktif 106). Önceki kayıt `STATUS_CHANGELOG.md`de.

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 PASS

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 ✓ PASS — eskalasyon sonrası tam doğrulama (head `3e9eb28`, PR #427; kriter 1–10 ve KK1–KK10 ✓, bulgu yok; düzeltme 2 ile B-1 düzeltmesinin `OriginalDispatchId`'si zincirin köküne iner; dal CI 42373330980 yeşil; Unit 3572 (Architecture 830), Integration CustomerNoticeQueueTests 14 + komşu 41; `12` `Durum` değişmedi (A317–A320, A324 gönderimi ister, `BEKLEMEDE`); K1 → D-272 (`EventValues` yük biçimi örnek dosyası), backlog aktif 108).

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 PASS

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 ✓ PASS — eskalasyon sonrası tam doğrulama (head `3e9eb28`, PR #427; kriter 1–10 ve KK1–KK10 ✓, bulgu yok; düzeltme 2 ile B-1 düzeltmesinin `OriginalDispatchId`'si zincirin köküne iner; dal CI 42373330980 yeşil; Unit 3572 (Architecture 830), Integration CustomerNoticeQueueTests 14 + komşu 41; `12` `Durum` değişmedi (A317–A320, A324 gönderimi ister, `BEKLEMEDE`); K1 → D-272 (`EventValues` yük biçimi örnek dosyası), backlog aktif 108).

---

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) düzeltme turu 1 bitti

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) düzeltme turu 1 bitti, doğrulama bekliyor (PR #430; B1: çit dışı değer `07 §6.2.6`'daki `ValueOutOfBounds` koduyla reddediliyor; B2: `PeriodsInput`/`PeriodsSection.ShippingPeriod` adı `DefaultShippingPeriod` oldu; `origin/main` birleştirildi, T309'un yinelenen ömür sabitleri kaldırıldı).

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 1 FAIL

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 1 ✗ FAIL (head `98f5550`, PR #430; B1 ORTA S1: çit dışı değer `07 §6.2`'nin `ValueOutOfBounds` kodu yerine `InvalidQuantity` ile reddediliyor (UC-170, UC-171); B2 DÜŞÜK S1: `PeriodsInput`/`PeriodsSection` alanı `ShippingPeriod`, `07 UC-170`/`06 §4.2` adı `DefaultShippingPeriod`; öteki kriterler ✓; build, format, Unit 3318 + Consistency 215, betik testleri, Integration FirmSetting 24 + SalesGate/CompanyIdentity 46 yerelde yeşil, dal CI 42375448881 yeşil; `12` `Durum` değişmedi; K1 → D-279, backlog aktif 105; dal merge edilmez, düzeltme turu gerekir).

---

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) yapım bitti

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) yapım tamamlandı, PR açıldı, doğrulama bekliyor (dal `task/T381-ayar-katmanlari`; Unit 3318 + Consistency 215, Integration FirmSetting 24 + SalesGate/CompanyIdentity 46; D-164 ve D-176 yeniden ileri atıldı, D-256 yeni, backlog aktif 104).

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 2

T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 2 ✗ FAIL (head `26e99f2`, PR #427; kriter 3 ~ Kısmi — Bulgu 1 ORTA S1: düzeltme sonrası B-1 satırı, verilen ilk satır kendisi bir düzeltme satırıysa onun taşıdığı ilk kimliği değil kendi kimliğini `OriginalDispatchId`'ye yazar, grup bölünür — `06 §4.58`, `08 §2.2.5` m.6; öteki kriterler ✓; dal CI 42370396257 yeşil; Unit 3363 + Consistency 209, Integration CustomerNoticeQueueTests 13 + komşu 49; `12` `Durum` değişmedi; yeni K-notu yok, backlog aktif 105; iki iterasyon sınırı — eskalasyon).

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) düzeltme 1

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) düzeltme 1 yapıldı, doğrulama tur 2 bekliyor (PR #427; tur 1 ✗ FAIL bulguları kapatıldı: B1 ORTA — düzeltme sonrası B-1 satırı olay değerlerine ilk e-postanın oluşma anını (`FirstEmailSentAt`) yazar, kuyruk ilk satırı (aynı sipariş, `B1`) okur, `Read` anı döndürür, yeniden gönderim cümlesi bu değerden üretilir, ilk satır silinse de tarih kalır; B2 DÜŞÜK — B-5 "Takip et" dolu düğme değil, sabit renkli bağlantı, `noreferrer` ve tam adres korunur, marka rengi yalnız ana düğmede; `origin/main` (T308 #424) birleştirildi, `INotificationsModule`/`NotificationsModule` iki tarafın üyeleriyle; Unit Notifications 202, Architecture 830, Infrastructure 1162, kalan 2331, Integration yerelde CustomerNoticeQueueTests 13, format ve derleme `-warnaserror` temiz; `12` `Durum` değişmedi; backlog aktif 105).

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 1 FAIL

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 1 ✗ FAIL (head `deb3a3f` + `origin/main` birleştirmesi, PR #427; kriter 3 ~ Kısmi — düzeltme sonrası B-1 satırı ilk e-postanın oluşma anını olay değerlerine yazmıyor (`06 §4.58` `EventValues`, `08 §2.2.5` m.6) — bulgu B1 ORTA S1; B2 DÜŞÜK S1 — B-5 "Takip et" marka renkli ikinci düğme (`08 §2.2.5` m.2); öteki kriterler ve KK ✓; Consistency 209, Unit 3418, betik testleri, Integration yerelde parçalı (CustomerNoticeQueueTests 11, altı komşu sınıf 92) geçti; dal CI 42365665924 yeşil; `12` `Durum` değişmedi; K1 → D-231, K2 → D-232; backlog aktif 104). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 PASS

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 ✓ PASS — eskalasyon sonrası tam doğrulama (head `3e9eb28`, PR #427; kriter 1–10 ve KK1–KK10 ✓, bulgu yok; düzeltme 2 ile B-1 düzeltmesinin `OriginalDispatchId`'si zincirin köküne iner; dal CI 42373330980 yeşil; Unit 3572 (Architecture 830), Integration CustomerNoticeQueueTests 14 + komşu 41; `12` `Durum` değişmedi (A317–A320, A324 gönderimi ister, `BEKLEMEDE`); K1 → D-272 (`EventValues` yük biçimi örnek dosyası), backlog aktif 108).

---

## 2026-11-07 — T366 (Yasal metinler) doğrulama tur 1 FAIL

2026-11-07 — T366 (Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin) doğrulama tur 1 ✗ FAIL (head `24163d5` + `origin/main` birleştirmesi, PR #428; kriter 2 ✗: aydınlatma metni firma kimliği eksikken yayına alınabiliyor, kimlik yer tutucuları boş değerle donuyor ve kimlik sonradan dolunca liste "tamamlandı" der, kapının aydınlatma koşulu açılır, vitrindeki metin veri sorumlusunun unvanı ve KEP'i boş kalır (`06 §4.56`, `04` 9.19.7, 9.19.11, `02 §3.1.5`); öteki kriterler ✓ (4, 5, 6 modül düzeyinde); Consistency 209, Unit 3234, betik testleri, Integration yerelde parçalı 118 geçti, dal CI 42368746924 yeşil; `12` `Durum` değişmedi; K1 → D-240; backlog aktif 108; düzeltme turu gerekli).

---

## 2026-11-07 — T366 (Yasal metinler) yapım bitti

2026-11-07 — T366 (Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin) yapım tamamlandı, doğrulama bekliyor (dal `task/T366-yasal-metinler`, PR #428; kriter 1–10, KK1–KK10 öz denetimde ✓ — 4, 5, 6'nın ekran ve e-posta yüzleri T356/T306/T385/T540/T507'de, D-222; Unit Architecture 846 + öteki 2597, Integration yerelde LegalText 13, SalesGate 22, CompanyIdentity 24, Model 974, Web 492, Files 81, Logging 30 geçti; Lint yerelde temiz; yeni D-219…D-222, D-165 (b) kapandı, D-167 T356'ya ileri, D-198 T366 payı kapandı; backlog aktif 106).

---

## 2026-11-07 — T308 (Firma bildirimleri F-1…F-6) doğrulama tur 1 PASS

2026-11-07 — T308 (Firma bildirimleri F-1…F-6) doğrulama tur 1 ✓ PASS (head `77c623d` + `origin/main` birleştirmesi, PR #424; kriter 1–10, KK1–KK10 ve `09 §25` ✓; Unit 3317 (Architecture 797, öteki 2520), betik testleri, Integration yerelde CompanyNoticeQueue + MigrationBundle 13, Dispatch/Email/Job komşuları 84, Logging 30, dal CI 42363737559 yeşil; `12` `Durum` değişmedi (KP-34, KP-67 satırları olayın tetiğini ve sahte altyapıyla gönderimi ister, `BEKLEMEDE`); doğrulamada `origin/main` birleştirildi; K1 → D-224, backlog aktif 105). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) yapım tamamlandı

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) yapım tamamlandı, PR açıldı, doğrulama bekliyor (Unit 3418, Integration yerelde CustomerNoticeQueueTests 11, format ve derleme `-warnaserror` temiz; öz denetim kriter 1–10, KK1–KK10 ✓; backlog aktif 104).
---

## 2026-11-07 — T309 (Hesap ve yönetim e-postaları) doğrulama tur 1 PASS

2026-11-07 — T309 (Hesap ve yönetim e-postaları) doğrulama tur 1 ✓ PASS (head `2ca3caa`, PR #429; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3521 (Consistency 213, Architecture 830, öteki 2478), betik testleri, Integration yerelde AccountEmailQueue 14, CompanyNoticeQueue 11, RecipientSendLimit 11; dal CI 42371984874 yeşil; bulgu 0, K-notu 0; `12` `Durum` değişmedi (KP-66/KP-67 satırlarında hesap e-postası yok); backlog aktif 105).

---

## 2026-11-07 — T332 (Satış modeli) yapım tamamlandı

**Son güncelleme:** 2026-11-07 — T332 (Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu) yapım tamamlandı, PR açıldı (dal `task/T332-satis-modeli`; ürün tanımı kuralları bekçide, seçenek yapısı ve stok kodu okumaları modül yüzünde; Unit 3528, Integration yerelde parçalı 1638; yeni backlog D-246…D-249, backlog aktif 107). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T309 (Hesap ve yönetim e-postaları) yapım tamamlandı

**Son güncelleme:** 2026-11-07 — T309 (Hesap ve yönetim e-postaları) yapım tamamlandı, PR açıldı (dal `task/T309-hesap-yonetim-epostalari`; yedi hesap e-postası, ömürler P-12…P-14/P-47 sabitlerinden, Google teyidi için tek bağlantı kuralı; Unit 3521 (Architecture 830), Integration `AccountEmailQueueTests` 14; yeni backlog D-233, D-234, backlog aktif 105).


---

## 2026-11-07 — T312 (Dosya uçları) doğrulama tur 1 PASS

2026-11-07 — T312 (Dosya uçları: imzalı indirme ve parçalı yükleme) doğrulama tur 1 ✓ PASS (head `93e5beb` + `origin/main` birleştirmesi, PR #426; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Consistency 209, Unit 3114, betik testleri; Integration yerelde parçalı — Files 81, Web üç sınıf 97, Concurrency 26 — hepsi geçti; dal CI 42362008048 yeşil; bulgu 0; `12` `Durum` değişmedi (KP-20/KP-44 satırları T511 ve formun kaydını bekler); K1 → D-217 (indirme 404'ünün E-06 sayfası, T504), K2 → D-218 ("kalemi indirilemez"in kapsamı, T511), K3 kod kanıtıyla kapandı; backlog aktif 100).

---

## 2026-11-07 — T331 (Satış kapısı) doğrulama tur 1 PASS

2026-11-07 — T331 (Satış kapısı) doğrulama tur 1 ✓ PASS (head `461c66b` + `origin/main` birleştirmesi, PR #425; kriter 1–8, KK1–KK8 ve `09 §25` ✓ (4 ve ekran yüzleri modül düzeyinde; ekran/POST bağları D-198, D-199 ile T346/T347/T341/T365/T508/T535'e); Unit 3282 (Architecture+Consistency+InstallationSettings 1185, öteki 2097), betik testleri, Integration yerelde SalesGate 22 + CompanyIdentity/AuditTrail 46, dal CI 42361992653 yeşil; `12` `Durum` değişmedi (KP-25, KP-38, KP-59, KP-60 satırları uç yanıtı/tarayıcı kanıtı ister, `BEKLEMEDE`); doğrulamada `origin/main` birleştirildi, backlog aktif 104; K-notu yok).

---

## 2026-11-07 — T308 (Firma bildirimleri F-1…F-6) yapım bitti

2026-11-07 — T308 (Firma bildirimleri F-1…F-6) yapım bitti, PR #424 açık, doğrulama bekliyor (task `⏳ Devam ediyor`; Unit 3286 (T308 57), Integration T308 11 yerelde geçti; `12` `VAL-A326…A330` ve `VAL-A754` olayı üreten task'lar `QueueCompanyNoticeAsync` / `QueueAdminNoticeAsync`'i bağlayana kadar `BEKLEMEDE`; T304, T305, T330, T380 squash hash'leri status'a işlendi).

---

## 2026-11-07 — T310 (Kargo şirketi listesi) doğrulama tur 1 PASS

2026-11-07 — T310 (Kargo şirketi listesi) doğrulama tur 1 ✓ PASS (head `c3e1670` + `origin/main` birleştirmesi, PR #423; kriter 1–9, KK1–KK9 ve `09 §25` ✓; plan düzeltmesi K-1576 kurala uygun (DV-32 tarayıcı denemesi T601 kriter 12'de); Consistency 209, Unit 2815, betik testleri; Integration dokunulmadı, dal CI 42353336593 yeşil; `12` `Durum`: `VAL-E034` → `PASS`, `VAL-E033` (T601) ve `VAL-A060`…`A063` (sipariş sayfası) `BEKLEMEDE`; K1 → D-196).

---

## 2026-11-07 — T305 (E-posta ortak kalıbı ve gövde üretimi) doğrulama tur 1 PASS

2026-11-07 — T305 (E-posta ortak kalıbı ve gövde üretimi) doğrulama tur 1 ✓ PASS (head `2ee4f63`, PR #422; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 3049 (T305 56), betik testleri, Integration yerelde parçalı 1793 hepsi geçti, dal CI 42352393327 yeşil; `12` `Durum` değişmedi — KP-66/KP-68 satırları gönderen yolunu ister, `BEKLEMEDE`; K1 raporda kapatıldı (D-97/D-166 sahibi), K2 → D-189, K3 → D-190). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T330 (Firma kimliği) doğrulama tur 1 PASS

2026-11-07 — T330 (Firma kimliği) doğrulama tur 1 ✓ PASS (head `04eafd6`, PR #420; kriter 1–8, KK1–KK8 ve `09 §25` ✓ — sayfa ve panel bağı T535/T506/T537/T361'in kendi kriterleri; Unit 3010 (Architecture 769, Consistency 207, kalan 2034; birleşik dalda 3050), betik testleri; Integration yerelde parçalı 1806 hepsi geçti; dal CI 42348609633 yeşil; `12` `Durum` değişmedi (KP-35, KP-58 satırları sayfa/uç kanıtı ister, `BEKLEMEDE`); doğrulamada `origin/main` (T304) dala birleştirildi; K-notu yok). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## YYYY-AA-GG — <özet başlık>

`<Taşınan status girişi buraya olduğu gibi yazılır.>`

---

---

## 2026-11-07 — T380 düzeltme turu 1

**Son güncelleme:** 2026-11-07 — T380 (İşlem izi) düzeltme turu 1 yapıldı, doğrulama tur 2 bekliyor (PR #419; tur 1 bulgu 1 KRİTİK S1 kapatıldı: `AuditEntryRules` Order satırında ve `DeleteCustomerAccount`'ta `Details`'in her yuvasını denetler — Order'da görünen ad yalnız sipariş numarası, hesap silmede görünen ad yok, değer adları kapalı listede ve her adın değeri kapalı biçimde (kod, tarih, adet, tutar, kimlik), serbest metin taşıyabilecek adlar (kargo adı, takip numarası, kalem adı) listeden çıkarıldı; `09 §9.2` m.4 yuva cümlesi eklendi; K1 backlog numarası D-161 → D-164 (main'deki D-161 ile çakışma); Unit Administration 100/100, Integration AuditTrailTests 22/22; `12` `Durum` değişmedi; doğrulama ve merge yardımcıda).

---

## 2026-11-07 — T304 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T304 (Alıcı başına bildirim gönderim sınırı) doğrulama tur 1 ✓ PASS (head `a8f38c1`, PR #421; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Consistency 209, Unit 2759, betik testleri; Integration yerelde `RecipientSendLimitTests` 11 ve dal CI 42346270356 yeşil; `12` `Durum`: `VAL-E086`, `VAL-E095` → `PASS`; K1 → D-172). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T313 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T313 (Site haritası ve robots.txt) doğrulama tur 1 ✓ PASS (head `336f075`, PR #417; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2851, Integration yerelde parçalı hepsi geçti (dal CI 42339118325 yeşil); `12` `Durum`: `VAL-A378`, `VAL-A379`, `VAL-A381`, `VAL-A398` → `PASS`, öteki KP-37 satırları `BEKLEMEDE`; K1–K3 raporda kapatıldı, açık uçlar D-163). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T314 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T314 (Durum ucu: uyarı koşulları ve 500 sayacı) doğrulama tur 1 ✓ PASS (head `a57c4ca`, PR #418; kriter 1–10, KK1–KK10 ve `09 §25` ✓; Unit 2878, Integration yerelde parçalı (StatusEndpointTests 41) ve dal CI 42339425149 yeşil; `12` `Durum` değişmedi — KP-50 satırları ve durum ucu satırları `BEKLEMEDE`; K1 kod kanıtıyla kapandı, K2 → D-162).

---

## 2026-11-07 — T313 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T313 (Site haritası ve robots.txt) doğrulama tur 1 ✓ PASS (head `336f075`, PR #417; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2851, Integration yerelde parçalı hepsi geçti (dal CI 42339118325 yeşil); `12` `Durum`: `VAL-A378`, `VAL-A379`, `VAL-A381`, `VAL-A398` → `PASS`, öteki KP-37 satırları `BEKLEMEDE`; K1–K3 raporda kapatıldı, açık uçlar D-163). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T378 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T378 (Zaman sayımı) doğrulama tur 1 ✓ PASS (head `33e4d94`, PR #416; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2669 + Consistency 207, betik testleri; Integration dokunulmadı (dal CI 42337007822 yeşil); `12` `Durum` değişmedi — task'ın KP satırı yok, ilgili `VAL-A801` tüketici task'larda `BEKLEMEDE`; K1 bu PR'da kapatıldı (`DateTimeSignatures` özet yorumu yerine taşındı); K2 → D-161 (sıfır iş günlük kargoya verme süresi sayımda istisna atar → T325)). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — F2 gate check PASS

**Son güncelleme:** 2026-11-07 — F2 (Çekirdek servisler) gate check: ✓ PASS (main `f4da03c`; T201–T233; Consistency 207/207, Unit 2826/2826, Integration 1730/1730 (parçalı), betik testleri 4 dosya; Release `-warnaserror` 0/0; üç imaj `--no-cache`; yerel referans kurulum sıfırdan, canlılık 200, denetimler yeşil; F2'de göç yok, 18 göç ileri ve idempotent; `11 §6.2` F2: mimari testler kasıtlı ihlalle kırmızı, maskeleme testleri yeşil; matris F2 189 satır, S3 0; `VAL-A991` gate'te yeniden PASS; backlog: D-32, D-60, D-68, D-85, D-123, D-128, D-157 ✅, yeni D-159, D-160; rapor `CHECKPOINT_REPORTS/GATE_CHECK_F2.md`; merge ve `phase/F2-pass` yardımcıda).

---

## 2026-11-07 — T229 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T229 (Uygulama belleğinde önbellek) doğrulama tur 1 ✓ PASS (head `6d25c8d`, PR #414; kriter 1–7, KK1–KK7 ve `09 §25` ✓; Unit 2826, Integration 1729 (yerelde dokuz parça; dal CI 42300942345 yeşil); `12` `Durum` değişmedi — ilgili `VAL-A835` `BEKLEMEDE`; K1 → D-158 (`09 §20` m.9'un yorumu yalnız testte, taramanın darlığı)). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T229 yapım tamamlandı, doğrulama bekliyor

**Son güncelleme:** 2026-11-07 — T229 (Uygulama belleğinde önbellek) yapım tamamlandı, doğrulama bekliyor (dal `task/T229-uygulama-bellegi-onbellek`, PR #414; kriter 1–7, KK1–KK7 ve `09 §25` öz denetim tablosu raporda; Unit 2826, Integration 1729; ⚠ K1–K4 raporda). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T219 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T219 (Arama motoru verileri kalıbı) doğrulama tur 1 ✓ PASS (head `f4e4505`, PR #413; kriter 1–8, KK1–KK8 ve `09 §25` ✓; Unit 2773, Integration 1715 (dal CI 42287139738 yeşil); `12` `Durum` değişmedi — KP-37'nin 38 satırı `BEKLEMEDE`; K1 → D-157 (Integration'ın göç paketi her koşuda yeniden kuruluyor)). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T219 yapım tamamlandı, doğrulama bekliyor

**Son güncelleme:** 2026-11-06 — T219 (Arama motoru verileri kalıbı) yapım tamamlandı, doğrulama bekliyor (dal `task/T219-arama-motoru-verileri`, PR #413; kriter 1–8, KK1–KK8 ve `09 §25` öz denetim tablosu raporda; Unit 2773, Integration 1715; yeni tablo/göç yok; backlog'da hedefi T219 olan kayıt yok). T217 Commit hücresi `d2b5379` ve raporunun Post-merge satırı dolduruldu. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T217 doğrulama tur 3 PASS

**Son güncelleme:** 2026-11-06 — T217 (Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim) doğrulama tur 3 ✓ PASS (head `e6cfe5a`, PR #412; tam yeniden doğrulama: kriter 1–8, KK1–KK8 ve `09 §25` ✓; B1-r2 kapandı — kural modülde, sayfa yalnız `Contracts` okuma/yazma yöntemini çağırır, `09 §8.3` 05/07/11 ile çelişmiyor; ret sonrası bütün tablolar değişmedi, Türkiye gece yarısı sınırı doğru (doğrulayıcı sondası); Unit 2640, Integration 1695; dal CI 42267704705 yeşil; bulgu 0; `12` `Durum` değişmedi (KP kaynak birimi yok); K1 → D-156 (iptal–cayma kuralının ve yazımının modülü; örnek modül başka modülün varlığını yazar)).

---

## 2026-11-06 — T217 düzeltme 2, doğrulama bekliyor

**Son güncelleme:** 2026-11-06 — T217 (Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim) yapım tamamlandı, doğrulama bekliyor (dal `task/T217-yapilabilir-islemler`, PR #412; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2640, Integration 1695 (doğrulama tur 1: ✗ FAIL, B1; düzeltme 1: çalıştırıcı ortak altyapıya; doğrulama tur 2: ✗ FAIL, B1-r2 — `09 §8.3` m.1 ürün kuralını sayfa katmanına koyuyordu, `05 §3.3` ve T323 ile çelişti; yeni D-155; düzeltme 2 yapıldı: kural modülde yaşar, sayfa yalnız `Contracts` okuma/yazma yöntemini çağırır, sıra `OperationGate.ExecuteAsync`'te, `09 §8.3` m.1/m.3/m.6/(5) buna göre — 05/07/11 ile aynı, karar kaydı gerekmedi); `09 §8.3` + `§25` satırı; kalıp `IOperationRule`/`OperationGate`/`OperationRunner`/`OperationRefusals` + `[GatedOperation]` kaynak taraması; gerçek ekran kuralları F3–F5'te; yeni D-153 → T367; T216 Commit hücresi `9ab9bec` dolduruldu). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T216 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-06 — T216 (Tarih, sayı ve para biçimi: istek ve gösterim) doğrulama tur 1 ✓ PASS (head `05fd381`, PR #411; kriter 1–8, KK1–KK8 ve `09 §25` ✓; tutar ayrıştırması Unicode rakam, görünmez boşluk, `0x10`, işaret, üs ve hassasiyet dışı metinle yoklandı — değer hiçbir girdide değişmez; Unit 2569, Integration 1682; dal CI 42224854554 yeşil; bulgu 0; `12` `Durum` değişmedi (KP kaynak birimi yok); K1 → D-150 (tam sayıda sondaki `\0`), K2 → D-151 (IBAN/telefonda bölünmez boşluk), K3 → D-152 (öteki tam sayı türleri ve `TimeSpan`)). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T216 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T216 (Tarih, sayı ve para biçimi: istek ve gösterim) yapım tamamlandı, doğrulama bekliyor (dal `task/T216-tarih-sayi-para-bicimi`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2569, Integration 1682; tek bağlayıcı `RequestValueBinder` (decimal/DateOnly/int/long), hassasiyeti aşan tutar metni okunmaz; D-124 ✅, D-123 (a), D-125 ve D-127 T216 payları ✅, yeni D-149 (`MachineJson` → T312)).

---

## 2026-11-06 — T215 doğrulama tur 2 PASS

2026-11-06 — T215 (Hız sınırı, deneme limitleri ve gövde tavanı) doğrulama tur 2 ✓ PASS (head `1ba392d`, PR #409; kriter 1–8, KK1–KK8 ve `09 §25` ✓; tur 1 B1 kapandı — gerçek Kestrel'de chunked gövde tavana kadar kabul, tavan+1 413, bildirim ucunda gövdesiz 413; Unit 2438, Integration 1649; dal CI 42211263821 yeşil; bulgu 0; `12` satırlarında `Durum` değişmedi — KP-72 satırları limit task'larının; K1 → D-147 (chunked okumada istemcinin vazgeçmesi ve bozuk çerçeve beklenmeyen hata sayılır), K2 → D-148 (büyük sınıfın chunked gövdesi kimlik doğrulamasından önce bellekte)).

---

## 2026-11-06 — T215 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T215 (Hız sınırı, deneme limitleri ve gövde tavanı) yapım tamamlandı, doğrulama bekliyor (dal `task/T215-hiz-siniri-deneme-govde`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2438, Integration 1649; `origin/main` (T214) birleştirildi; D-103, D-112 ve D-143 ✅, D-64 `AttemptRecord` yarısı ✅ (`LoginRecord` yarısı T341'e), D-108 tavan konmadı → F2 gate, D-135'e 429/413 yalın sayfa notu).

---

## 2026-11-06 — T214 doğrulama tur 1 PASS

2026-11-06 — T214 (Kısa süreli bildirim kodları) doğrulama tur 1 ✓ PASS (head `479c891`, PR #410; kriter 1–6, KK1–KK6 ve `09 §25` ✓; Unit 2307, Integration 1599; dal CI 42201202210 yeşil; bulgu 0; `12` satırlarında `Durum` değişmedi — `07 §6.3` ve VAL-A607 satırları uç ve giriş ekranı task'larının; K1 → D-146 (`notice` kaynak taramasının tırnaksız bağlama boşluğu)). Önceki kayıt aşağıda.

---

## 2026-11-06 — T214 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T214 (Kısa süreli bildirim kodları) yapım tamamlandı, doğrulama bekliyor (dal `task/T214-bildirim-kodlari`; kriter 1–6 ve KK1–KK6 öz denetimde ✓; Unit 2307, Integration 1600; D-47 (2) ve D-121 ✅, D-109 T214 payı ✅ — kalan T343/T409). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T213 doğrulama tur 1 PASS

2026-11-06 — T213 (Hata kodu kayıt defteri) doğrulama tur 1 ✓ PASS (head `a8f172b`, PR #408; kriter 1–8, KK1–KK8 ve `09 §25` ✓; Unit 2247, Integration 1537; dal CI 42187650016 yeşil; bulgu 0; `12` satırlarında `Durum` değişmedi — `07 §6.2`'nin 119 satırı uç task'larının; K-notları: K1 → D-142 (`Result`/`Result<T>`, F3), K2 → D-143 (429 ve içerik başlıklarının kod sızma taraması, T215), K3 → D-89'a not (red satırı taramasının darlığı)). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T213 yapım tamamlandı

2026-11-06 — T213 (Hata kodu kayıt defteri) yapım tamamlandı, doğrulama bekliyor (dal `task/T213-hata-kodu-kayit-defteri`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2247, Integration 1538; `ErrorCode` 115 üye + `ErrorStatuses` + tek `RejectionLog`; D-47 (2) `ErrorCode` payı, D-132, D-136, D-140 ✅; D-90 K1/K2/K4 ✅; D-89 ve D-90 K3 gerekçeyle yeniden hedeflendi; D-141 açıldı).

---

## 2026-11-06 — T212 doğrulama tur 1 PASS (merge #407)

2026-11-06 — T212 (Hata gösterimi ve HTTP durumları) doğrulama tur 1 ✓ PASS (head `ec13690`, PR #407; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2221, Integration 1536; dal CI 42172712753 yeşil; bulgu 0; `VAL-A424`, `A425`, `A429`, `A430`, `A433` PASS; K-notları: K1 → D-138'e not (`09 §15.1` m.6 istisnası, F2 gate), K2 → D-135 hedefi T537 + T540, en geç F5 gate, K3 → D-140 (katmanın `no-store` başlığı, T213)). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T212 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T212 (Hata gösterimi ve HTTP durumları) yapım tamamlandı, doğrulama bekliyor (dal `task/T212-hata-gosterimi-http`; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 2221, Integration Web/Pages/SignedEndpoints/Security/Logging/Jobs/Outages/Files/Disposal/Concurrency ad alanları yerelde gerçek PostgreSQL ile yeşil, Database ve Model ad alanları dal CI'ında; D-25, D-84, D-107, D-113, D-115, D-117, D-130 kapandı; yeni D-135, D-136).

---

## 2026-11-06 — T210 doğrulama tur 1 PASS (merge #406)

2026-11-06 — T210 (Liste kalıbı) doğrulama tur 1 ✓ PASS (head `5476b9f`, PR #406; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2197, Integration 1510; dal CI 42165996784 yeşil; bulgu 0; `VAL-A006`–`A008` BEKLEMEDE (gerçek vitrin listesi T333/T503); K-notu D-134 (örnek vitrin listesinin fiyat/stok süzgeci ve projeksiyonu, T333); D-127'nin T210 payı kapandı).

---

## 2026-11-06 — T210 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T210 (Liste kalıbı) yapım tamamlandı, doğrulama bekliyor (dal `task/T210-liste-kalibi`; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 2197; Integration: Web ad alanı 338 yerelde (liste testleri 64 dahil), kalanı dal CI'ında; D-127'nin T210 payı kapandı; `09 §10.5` madde 3 `FromQuery` ekiyle hizalandı, ⚠ 1).

---

## 2026-11-06 — T209 doğrulama tur 1 PASS (merge #405)

2026-11-06 — T209 (Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları) doğrulama tur 1 ✓ PASS (head `449f94e`, PR #405; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2035, Integration 1446; dal CI 42159301722 yeşil; `VAL-A982` PASS; K-notları D-132 (sahtecilik reddinin red satırı, T213), D-133 (yeniden doğrulama değerinin oturum yazımı `ISessionService` altına, T342); `09 §15.2` madde 2 yazım düzeltmesi). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T209 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T209 (Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları) yapım tamamlandı, doğrulama bekliyor (kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 2035, Integration 1446; D-131 açıldı (UC-83 ucunun başlık jetonu, T312); `09 §15.2` madde 1, 2, 5 metni gerçek mekanizmaya uyarlandı, 404 yanıtı da `no-store`).

---

## 2026-11-06 — T208 doğrulama tur 1 PASS

2026-11-06 — T208 (Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme) doğrulama tur 1 ✓ PASS (head `3247026`, PR #404; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 1931, Integration 1413; dal CI 42150380946 yeşil; `VAL-A610`–`A614` PASS; D-95 (c) kapandı; K-notları D-129, D-130).

---

## 2026-11-06 — T208 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T208 (Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme) yapım tamamlandı, doğrulama bekliyor (kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 1931, Integration 1413 yerelde gerçek PostgreSQL 18’de yeşil; main ile birleştirildi (T211); D-95 (c) kapandı, (a)(b) T348; ⚠ hata kodları T213’te, öteki değerlerin yanıtı T212 madde 4).

---

## 2026-11-06 — T211 doğrulama tur 2 PASS (merge #402)

2026-11-06 — T211 (Model bağlama kalıbı) doğrulama tur 2 ✓ PASS (head `50622d5`, PR #402; kriter 1–8, KK1–KK8 ve `09 §3.4` m.1–7 ✓; tur 1 bulguları B1 ve B2 kapandı; Unit 1757, Integration 1392; dal CI 42143462814 yeşil; K-notları D-126–D-128).

---

## 2026-11-06 — T207 doğrulama tur 1 PASS (merge #403)

2026-11-06 — T207 (Kimlik şemaları ve erişim reddi) bağımsız doğrulama tur 1: ✓ PASS (head `cade300`, PR #403; kriter 1–9 ve KK1–KK9 ✓; Unit 1628, Integration 1360 yerelde gerçek PostgreSQL 18'de yeşil, betik testleri yeşil; dal CI 42133128685 yeşil; bulgu 0; D-118 kapandı; K1 → D-120 (açık sayfa listesi adres şablonuyla, dönüş adresi kendi denetimiyle — `09 §15.1` m.2, §15.2 m.6 cümleleri karar kaydıyla hizalanır, F2 kapısı), K2 → D-121 (oturumu kapanmış POST'un 303'ü `notice=SessionExpired` taşımıyor — T214), K3 → D-122 (örnek üye kökü oturumsuz 404, `07` UC-33 302 ister; VAL-A609 gerilimi — T511); `12` VAL-A606 PASS, VAL-A602…A605, A607…A609, A736 BEKLEMEDE (kayıt ve ekranları sonraki task'larda)).

---

## 2026-11-06 — T211 yapım bitti, doğrulama bekliyor

2026-11-06 — T211 (Model bağlama kalıbı) yapım tamamlandı, doğrulama bekliyor (dal `task/T211-model-baglama-kalibi`; kriter 1–8, KK1–KK8 ve `09 §3.4` m.1–7 öz denetimde ✓; girdi doğrulama kalıbı yönetici kararıyla T211'de yazıldı, D-119 ✅; Unit 1757, Integration 1392; main ile birleştirildi (T207) yerelde gerçek PostgreSQL 18'de yeşil).

---

## 2026-11-06 — T207 yapım bitti, doğrulama bekliyor; T206 doğrulama tur 1 PASS

2026-11-06 — T207 (Kimlik şemaları ve erişim reddi) yapım tamamlandı, doğrulama bekliyor (dal `task/T207-kimlik-semalari-erisim-reddi`; Unit 1628, Integration 1360; D-118 kapandı). Önceki: T206 (Adres yapısı, önekler ve sayfa–form işleyici kalıbı) bağımsız doğrulama tur 1: ✓ PASS (head `686f019`, PR #401; kriter 1–9 ve KK1–KK9 ✓; Unit 1515, Integration 1324 yerelde gerçek PostgreSQL 18'de yeşil, betik testleri yeşil; dal CI 42123791130 yeşil; bulgu 0; K1 → D-117 (`HandlerRequiredFilter` PUT/PATCH/DELETE'e 400 verir, `07 §2.7.1` yöntem uymazsa 405 ister — T212), K2 → D-118 (`FormResults.IsLocalPath` sekme ve öteki denetim karakterlerini geçirir — T207), K3 D-73 ve D-116 metin düzeltmesi bu PR'da; `12`'de T206'nın satırı yok, `Durum` hücresi değişmedi).

---

## 2026-11-06 — T206 yapım bitti, doğrulama bekliyor

2026-11-06 — T206 (Adres yapısı, önekler ve sayfa–form işleyici kalıbı) yapım tamamlandı (dal `task/T206-adres-sayfa-form-kalibi`; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 1515 (Consistency 165 dahil), Integration Web 153 yerelde yeşil). Adres kuralları `Infrastructure/Routing/` (`AddressRules` beş önek, kapalı ilk bölüm listesi, kayıt adresi; `AddressTemplates`, `OrderRoutes` iki sipariş kökü, `AddressConvention` listede olmayan ilk bölümü açılışta reddeder), form sonuçları `Infrastructure/Forms/` (`FormResults` 303 / 422, `CsvFileResult`, `HandlerRequiredFilter`); taramalar `PageSourceScan` (API/sürüm, görünüm bağlamı, `post` formunda işleyici) ve `PageHandlerRuleTests`, ayrı ihlal örnekleriyle; eşitlik `AddressListConsistencyTests`. D-73 kapandı; D-116 açıldı (`09 §16.1` ile `07 §2.1.2` statik yol çelişkisi, T537). ⚠: işleyicisiz ya da bilinmeyen işleyicili POST 400 (`07 §2.7.1` "404 ya da 400"); yol deseni sonu `$` yerine `\z`; ÖRNEK sayfa `[IgnoreAntiforgeryToken]` yalnız test için, sahtecilik koruması T209'da; yerel `localhost` IPv6 gecikmesi canlılık testlerini kırar (CI etkilenmez).

---

## 2026-11-06 — T224 doğrulama tur 2 PASS

2026-11-06 — T224 (Çerez politikası) bağımsız doğrulama tur 2: ✓ PASS (head `b684a29`, PR #400; kriter 1–9 ve KK1–KK9 ✓; tur 1'in B1–B2'si ve K1–K2'si kapandı; Unit 1347, Integration 1258 yerelde yeşil; dal CI 42107824627 yeşil; bulgu 0; K1 → D-113 notu (`09 §15.1` m.6 ile sıra), K2 → D-114 (tarama kesinliği, T342), K3 → D-115 (kararsız canlılık testi, T212); `12` satırları A363–A367 BEKLEMEDE — akış task'ları). Önceki: T224 bağımsız doğrulama tur 1: ✗ FAIL (head `4737a61`, PR #400; B1 KK9 `~` — ödeme çerçevesi istisnası `object`/`embed`/`frameset`'i de serbest bırakıyor; B2 KK3 `~` — çerez taraması ham `Set-Cookie` başlık yazımını görmüyor; K1 kapının veritabanı hatası yakalama katmanının dışında, K2 çerçeve kaynağı — düzeltme turunda; Unit 1330, Integration 1258 yerelde yeşil; dal CI 42102317150 yeşil; `12` satırları değişmedi). Önceki: T224 yapım tamamlandı (kriter 1–9 ve KK1–KK9 öz denetimde ✓; ters vekil başlıkları D-112 → T215). Önceki: T223 (Dosya deposu) bağımsız doğrulama tur 2: ✓ PASS (head `6614402`, PR #399; kriter 1–9 ve KK1–KK9 ✓; tur 1'in B1–B3'ü ve K1–K4'ü kapandı; Unit 1288, Integration 1246 yerelde yeşil; dal CI 42089604427 yeşil; bulgu 0; K1–K5 → D-111; `12` satırları A186–A190, A465, A834 BEKLEMEDE — T339/T525/T302). Önceki: T204 (Dış servis çökmesi davranışı ve sahtelerle sınama) bağımsız doğrulama tur 1: ✓ PASS (head `548c4db`, PR #398; kriter 1–8 ve KK1–KK8 ✓; Unit 1228, Integration yerelde yeşil; dal CI 42077481379 yeşil; bulgu 0; K1–K2 → D-110; `12` satırları E056, E060–E066 sahip task'larda — BEKLEMEDE) (dal `task/T204-dis-servis-cokmesi`; sahtelere üç hata türü ve kesinti anahtarı, ödeme başlatma ve son sorgu kuralları `Modules/Payment/`, e-posta deneme kuralı `DispatchRetrySchedule` — ilk denemeden 5 dk / 30 dk / 2 sa, sonra "ulaşmadı" — ve tek denemenin yazımı `DispatchAttempts` — satır + kanal hâli tek işlemde, gönderici T305/T385/T407'de bağlanır (D-97), Google hata kuralı `Modules/Membership/` (kodların `NoticeCode`'a bağlanması D-109); e-posta ve Google ağ geçidinin kendi klasörleri dışında anılmaması taraması; Unit 1074 + Consistency 154, Integration 1157 yerelde yeşil).

---

## 2026-11-05 — T203 doğrulama tur 2 PASS

2026-11-05 — T203 (İmzalı makine ucu altyapısı) bağımsız doğrulama tur 2: ✓ PASS (head `af2e64f`, PR #397; kriter 1–9 ve KK1–KK9 ✓; tur 1 B1, B2 kapandı; gerçek Kestrel'de veritabanı hatası 500 + boş gövde görüldü; `VAL-E045` PASS, `E046`/`E047`/`E051`/`E052` PASS, `E050` ve öteki satırlar sahip task'larda — `BEKLEMEDE`; Unit 1176, Integration 1206 yerelde yeşil; dal CI 42056504243 yeşil; bulgu 0; K1 → D-108'e eklendi: sayılı pencere yeniden başlatmaya kadar kalır, T215; K2 → D-107 kapsar, T212) (dal `task/T203-imzali-makine-ucu`; ortak ardışık düzen `Infrastructure/SignedEndpoints/` — 64 KB tavan okumadan ve imzadan önce 413, imza ucun `Verify`ı ile 400 gövdesiz + seyreltilmiş güvenlik satırı, kayıt ve iş satırı tek işlemde, "alındı" kesinleşmeden sonra, veritabanı hatası yakalanmaz — tek yakalama katmanı 500 verir (T212, D-107); sabit zamanlı `SignatureComparison.Matches` ve kaynak taraması; gerçek uç T311, PayTR imzası T402).

---

## 2026-11-05 — T202 yapım bitti, doğrulama bekliyor

2026-11-05 — T202 (Dış servislerin bağlantı katmanı) yapım bitti, PR #396 açıldı, doğrulama bekliyor (dal `task/T202-dis-servis-katmani`; üç arayüz ve sekiz sonuç ailesi `Infrastructure/Integrations/`'ta, ortak çağrı yüzü `ExternalCallRunner` — TimeProvider'lı süre, tek çağrı, `ExternalCallFailed` kaydı; adlandırılmış HTTP istemcileri yeniden denemesiz; geliştirmede `SmtpCaptureEmailGateway` → Mailpit, ayar `compose.dev.yaml`; sahteler `UnitTests/Fakes/`; Unit 1126 yeşil; D-101 kapandı). Önceki giriş (T201 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T201 doğrulama tur 1 PASS

2026-11-05 — T201 (İmha düzenleyicisi, süre ayarları ve gecelik temizlik) bağımsız doğrulama tur 1: ✓ PASS (head `bed034e`, PR #395; kriter 1–9 ve KK1–KK9 ✓; parça başına tek işlemde silme + imha kaydı, kayıt reddinde silme geri alınır; kalıcı kayıt, hesap ve yönetici bağlantı kaydı, üye sepeti ve "başarısız" iş satırı yerinde; iz yalnız DB fonksiyonuyla; kayıt dosyası satırı dosyadan önce, döngü satırı aralıkla; `VAL-D046`/`D047`/`D078`/`D117`/`D118` PASS, `VAL-D048`–`D051`, `D080` sahip task'larda — `BEKLEMEDE`; Unit 1026, Integration 1192 yerelde yeşil; dal CI 42025909535 yeşil; bulgu 0; K1–K2 → D-104).

---

## 2026-11-05 — T201 yapım bitti, doğrulama bekliyor

2026-11-05 — T201 (İmha düzenleyicisi, süre ayarları ve gecelik temizlik) yapım bitti, PR #395 açıldı, doğrulama bekliyor (dal `task/T201-imha-duzenleyicisi`; gecelik işin ikinci adımı `DisposalStep`, parça başına tek işlemde silme ve imha kaydı; altı geçici kayıt parçası, işlem izi fonksiyonu ve kayıt dosyası parçası; misafir sepeti ve davet parçaları kendi modüllerinde; Unit 1026, `DisposalTests` 18; D-59 ve D-91 K1 ✅, yeni D-102, D-103). Önceki giriş (T221 doğrulama tur 3 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T221 bağımsız doğrulama tur 3: PASS

2026-11-05 — T221 (Zamanlayıcı) bağımsız doğrulama tur 3: ✓ PASS (head `f277e08`, PR #394; kriter 1–9 ve KK1–KK9 ✓; tur 2 B1 kapandı — `JobRunnerTests` kilit kopması testi yalnız kendi veritabanını vurur, `Jobs` ad alanı paralel koleksiyonlarla 3/3 yeşil; K1 `compose.dev` tanımı, K2 nabız sınırı `TimeProvider`'da; `VAL-C111` PASS, `VAL-C109`/`C110`/`A826`/`A827` iptal kaynağı gelince — `BEKLEMEDE`; Unit 996, Integration 1175 yerelde yeşil; dal CI 42017091348 yeşil; bulgu 0; K1 → D-101).

---

## 2026-11-05 — T221 yapım bitti, doğrulama bekliyor

2026-11-05 — T221 (Zamanlayıcı) yapım bitti, PR açıldı, doğrulama bekliyor (dal `task/T221-zamanlayici`; uygulamada tek `SchedulerService`: dakikalık tarama ve ayrı otuz saniyelik nabız döngüsü, zamanlayıcı ve gecelik iş oturum kilitleri, gecelik iş 03:00 / bir saatlik yineleme, kesinti kaydı + ilk nabız + taşıma tek işlemde, havale iptali erteleme; D-98 ve D-39 (f) kapandı, yeni D-99; Unit 996, Integration 1171, betik testleri 4/4 yerelde yeşil).

---

## 2026-11-05 — T220 doğrulama tur 1 PASS

2026-11-05 — T220 (İş kuyruğu çalıştırıcısı) bağımsız doğrulama tur 1: ✓ PASS (head `2deb469`, PR #393; kriter 1–9 ve KK1–KK9 ✓; tek `JobRunnerService`, oturum kilidi + `FOR UPDATE SKIP LOCKED`, işleyici işlem dışında, kesinleşince tek uyandırma, varsayılan 1/5/15/60/240 dk ve altıncı hatada `Failed`, başarısız satır silinmez, gönderilmiş kart iadesi yeniden gönderilmez; `KP-68` satırları e-posta task'larında, `VAL-A828`/`A823`/`A829`/`A987` işleyici ve gecelik iş gelince — `BEKLEMEDE`; Unit 972, Integration 1141 yerelde yeşil; dal CI 41994864604 yeşil; bulgu 0; K1 → D-98, K2 raporda düzeltildi).

---

## 2026-11-05 — T220 yapım bitti

2026-11-05 — T220 (İş kuyruğu çalıştırıcısı) yapım bitti, PR açıldı, doğrulama bekliyor (dal `task/T220-is-kuyrugu`; tek arka plan döngüsü `JobRunnerService`: tek çalışma oturum kilidi, sinyal ya da beş saniye, `FOR UPDATE SKIP LOCKED` ile satır alma, işleyici işlem dışında; `IJobQueue.EnqueueAsync` çağıranın işleminde ve kesinleşince tek uyandırma; varsayılan yeniden deneme 1/5/15/60/240 dk, altıncı hata `Failed`; kart iadesi kuralı; açılış kapısı, D-96 K1 kapandı; Unit 972, Integration 1141 yerelde yeşil; işleyiciler sahip task'larda → D-97). Önceki giriş (T222 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T222 doğrulama tur 1 PASS

2026-11-05 — T222 (Anahtarlar ve tek kullanımlık bağlantılar) bağımsız doğrulama tur 1: ✓ PASS (head `fcd3009`, PR #392; kriter 1–8 ve KK1–KK8 ✓; OS üreteci `SecretKey`, HMAC türetme `LinkKeys`, yalnız özet saklayan bağlantı kaydı, beş hâl, salt okuyan `ReadAsync`, tek koşullu `TryConsumeAsync` (eşzamanlı iki istekte biri), açılışta kuyruktan önce geri alma bağlantısı özetinin yeniden yazımı; KP satırı yok, ilgili `VAL-A616`/`A677` sayfa task'larında `BEKLEMEDE`; Unit 932, Integration 1093 yerelde yeşil; dal CI 41977809448 yeşil; bulgu 0; K1–K2 → D-96).

---

## 2026-11-05 — T222 yapım bitti

2026-11-05 — T222 (Anahtarlar ve tek kullanımlık bağlantılar) yapım bitti, PR açıldı, doğrulama bekliyor (dal `task/T222-anahtarlar-baglantilar`; `SecretKey`, `LinkKeys` (HMAC türetme, `DigestFor`), `LinkRecords` (hâl, salt okuma, tek koşullu tüketim, öncekini geçersiz kılma), açılışta geri alma bağlantısı özetinin yeniden yazımı `RevertLinkDigestRewrite(Startup)`; Unit 932, Integration 1093 (yerelde 1 ortam düşüşü, rapor ⚠ 7)). Önceki giriş (T228 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T228 doğrulama tur 1 PASS

2026-11-05 — T228 (Eşzamanlılık denetimi) bağımsız doğrulama tur 1: ✓ PASS (head `c67a1e2`, PR #391; kriter 1–9 ve KK1–KK9 ✓; tek işlem sınırı `ITransactionRunner`, kilit sırasını çalışma anında tutan `LockSequence`, sayaçların tek yazıcısı `StockCounters`, ürün ve sipariş sürüm bekçileri; KP-76 satırları uç düzeyinde, `BEKLEMEDE`; CI 41969947908 yeşil; Unit 897/897, Integration 1072/1072; bulgu 0; K1 → D-95, K2 raporda kapandı).

---

## 2026-11-05 — T225 doğrulama tur 3 PASS

2026-11-05 — T225 (Sistem kayıtları) bağımsız doğrulama tur 3: ✓ PASS (head `820fd66`, PR #389; kriter 1–7 ve KK1–KK7 ✓; tur 2 B1 kapandı — erişim kaydı sorguyu parça parça maskeler, çift sonrası çıplak belirteç dahil (gerçek `caddy:2.11.7-alpine` sondası, 24 istek); B2 kapandı — satır yalnız `05 §9.1.2` alanları ve zarf; `VAL-A991` PASS; CI 41962307990 yeşil; Unit 849/849, Integration 1046/1046; bulgu 0; K1 → D-93).

---

## 2026-11-05 — T232 doğrulama tur 2 PASS

2026-11-05 — T232 (Veri koruma halkası) bağımsız doğrulama tur 2: ✓ PASS (head `c8e798b`, PR #388; kriter 1–8 ve KK1–KK8 ✓; tur 1 B1 kapandı — veri erişim kategorileri uyarıda, yalnız iki EF hata olayı kimlikle süzülür, aynı kategorinin uyarısı yazılır; B2 kapandı — tarama alt amaç argümanını ve `GetDataProtector`'ı bulur; CI 41954716044 yeşil; Unit 811/811, Integration 1030/1030; bulgu 0; K1–K4 → D-90).

---

## 2026-11-05 — T231 doğrulama tur 1 PASS

2026-11-05 — T231 (Maskelemenin üç katmanının testleri) bağımsız doğrulama tur 1: ✓ PASS (head `e20aef8`, PR #387; kriter 1–7 ve KK1–KK7 ✓; yazım, yol, biçimleyici ve istisna katmanları birim testli, kaynak taraması ihlal örnekleriyle kırmızı; `PathMasker` boş bölümlü yolda anahtarı maskeler; CI 41948281331 yeşil; Unit 729/729; bulgu 0; K1–K3 → D-89). Önceki giriş (T230 doğrulama tur 2) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T230 doğrulama tur 2 PASS

2026-11-05 — T230 (Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi) bağımsız doğrulama tur 2: ✓ PASS (head `9938934`, PR #386; kriter 1–9 ve KK1–KK9 ✓; tur 1 B1 kapandı — satırlar çatının sağlayıcılarına iletilmez, konteyner çıktısına uygulama satırı inmez; bozma sınaması: `writeToProviders: true` iken sağlayıcı testi, `retainedFileCountLimit: 31` iken KK3 testi kırmızı; CI 41943697506 yeşil; Unit 648/648, Integration 1018/1018; bulgu 0; K1–K2 → D-88).

---

## 2026-11-05 — T205 doğrulama tur 1 PASS

2026-11-05 — T205 (Açılışta veritabanı fail-fast denetimi) bağımsız doğrulama tur 1: ✓ PASS (head `501f37f`, PR #385; kriter 1–7 ve KK1–KK7 ✓; boş il/ülke listesi, altı kayıttan her biri ve tablosu olmayan kayıt açılışı adıyla reddeder, değer iletide yok, Hakkımızda/duyuru/teslimat illeri/taslak yokluğu açılışı engellemez, eksik ayar veritabanına sorulmadan reddeder; bozma sınaması: atma kapatılınca 11 test kırmızı; `package-build.sh` adım 5 kurucu SQL + göç demeti sonra uygulama, CI 41924447176 yeşil; Unit 548/548, Integration 1004/1004; bulgu 0; K1–K3 → D-86).

---

## 2026-11-05 — T205 yapım bitti

2026-11-05 — T205 (Açılışta veritabanı fail-fast denetimi) yapım bitti, doğrulama bekliyor (dal `task/T205-acilista-veritabani-fail-fast`, PR #385; `StartupDatabaseCheck` ikinci hosted service, `DatabaseAudit` saf işlev, varlığı `AppDbContext.ReadStartupRecordPresenceAsync` okur; il, ülke ve altı tek satırlık kayıt; Unit 548, Integration 1004).

---

## 2026-11-05 — T233 doğrulama tur 2 PASS

2026-11-05 — T233 (Veritabanı oturum süreleri) bağımsız doğrulama tur 2: ✓ PASS (head `cbdfe10`, PR #384; kriter 1–7 ve KK1–KK7 ✓; tur 1'in B1'i ve K2–K4 kapandı: testler süreleri sabitlerden türetir, web ev sahibinin `Web` profili testle korunur, `09 §10.7` madde 2–3 ve `11` kriter 4/KK4 "kilit ve sorgu sürelerinden uzun" diye daraltıldı (D-83 ✅); bozma sınamaları: `Program.cs` ham dizi, sabit 16 sn, `src/`'e `SET`/`SET LOCAL` ve gövdede `HttpClient` → kırmızı; Unit 543/543, Integration 987/987; K1 → D-85 (sabitlerin özet yorumları değeri yineler)).

---

## 2026-11-04 — T233 yapım bitti

2026-11-04 — T233 (Veritabanı oturum süreleri) yapım bitti, doğrulama bekliyor (dal `task/T233-veritabani-oturum-sureleri`; `DatabaseTimeouts` altı sabit, tek kuruluş noktası `DatabaseConnection.Configure` ve iki profil `Web`/`Maintenance`; web ev sahibi `Web`, göç hizmetinin Compose bağlantısı `Maintenance` ile bağlanır; iki yeni kaynak taraması (`SET` biçimleri, `ITransactionRunner` gövdesinde dış çağrı) kasıtlı ihlal örnekli; yeniden deneme stratejisi kapalı; Npgsql 10.0.3 ile `Options` ve `CommandTimeout` adları doğrulandı; D-22 ve D-26 kapandı). Doğrulama tur 1 (2026-11-04): ✗ FAIL (B1: testler süre sayılarını sabitten değil kendileri yazıyordu); düzeltme 1 (2026-11-05, head `cbdfe10`).

---

## 2026-11-04 — T226 doğrulama tur 1 PASS

2026-11-04 — T226 (Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi) bağımsız doğrulama tur 1: ✓ PASS (head `2381b8b`, PR #383; kriter 1–6 ve KK1–KK6 ✓; derlenmiş uygulamayla gerçek açılış sondası: ürün sabiti adlı ayar, eksik gönderen adresi, kısa türetme sırrı, veritabanı yokluğu ve tip hatası açılışı durdurdu, iletide değer yok; sekiz kasıtlı bozma ilgili testleri kırmızıya çevirdi; Unit 520/520, Integration 980/980; dal CI 41898811183 yeşil; bulgu 0; K1 → D-80, K2 → D-81). Merge yardımcıda. Önceki giriş (T226 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T226 yapım bitti

2026-11-04 — T226 (Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi) yapım bitti, doğrulama bekliyor (dal `task/T226-ayar-katmanlari`; ayar sınıfları ve açılış denetimi `Infrastructure/InstallationSettings/`, beş sır ayarı sır dosyasından, iki yeni tarama kuralı ihlal örnekli; ayrıntı `TASK_REPORTS/T226_REPORT.md`). Önceki giriş (T227 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T227 doğrulama tur 1 PASS

2026-11-04 — T227 (Zaman ve para) bağımsız doğrulama tur 1: ✓ PASS (head `cdc6648`, PR #382; kriter 1–7 ve KK1–KK7 ✓; kuruş yuvarlaması ve orantılı bölme sınır değerleriyle yeniden sınandı (0,015 → 0,02, 0,025 → 0,03, 9,595 → 9,60, 4,995 → 5,00; 10 TL 50:100:150 → 1,66 + 3,33 + 5,01; 0,02 TL üç eşit ağırlık → 0,02 + 0 + 0); on kaynak mutasyonu ilgili testleri kırmızıya çevirdi — `TurkeyTime.DateOf`'un sunucu dilimiyle okunması +03:00 makinede yalnız mimari testle yakalandı; Unit 458/458 (Consistency dahil); dal CI 41889663878 yeşil; bulgu 0; K1 → D-76, K2 → D-77). Merge yardımcıda. Önceki giriş (T227 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T227 yapım bitti

2026-11-04 — T227 (Zaman ve para) yapım bitti, doğrulama bekliyor (dal `task/T227-zaman-ve-para`, PR #382; tek zaman kaynağı `TimeSource` (`TimeProvider.System` tekil), sabit +03:00 `TurkeyTime` (gün, gün başı, gün süresinin sonu), ürünle gelen tatil dosyası ve `BusinessCalendar` (2026–2027; kapsam dışı yılda yalnız hafta sonu), `Money.Round` (tam yarım yukarı) ve `Money.Allocate` (parçalar aşağı, kalan en büyük ağırlığa); derlenmiş kod üzerinden zaman ve para mimari testi ile göç dışı SQL saat taraması, ikisi de ihlal örnekli; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 458/458 (Consistency 101 dahil); D-74, D-75 açıldı; T218 Commit hücresi `d82b943`). Önceki giriş (F1 gate check PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — F1 gate check PASS

2026-11-04 — F1 (Veri katmanı) gate check: ✓ PASS (main `c59321d`; T101–T120; Consistency 101/101, Unit 241/241, Integration 952/952, betik testleri 4 dosya; Release `-warnaserror` 0/0; yerel referans kurulumda 18 göç ileri ve idempotent, seed sayıları beklenen, canlılık 200; matriste F1'e bağlı 144 satır implement edildi, S3 0; `VAL-A838` PASS; S2 yok). Gate PR'ı bu commit'i taşır; merge ve `phase/F1-pass` etiketi yardımcıda. Önceki giriş (T120 doğrulama tur 2 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T120 doğrulama tur 2 PASS

2026-11-04 — T120 (Model eşitlik denetimleri) bağımsız doğrulama tur 2: ✓ PASS (head `e0827c3`, PR #378; kapsam tam, 8 kriter ve KK1–KK8 ✓; tur 1 bulgusu kapandı: §7.2.1'de kendi dizini vaat edilmeyen bağa eklenen tek sütunlu dizin artık §7.5.1 denetimini kırar — kaynak mutasyonuyla sınandı; KK1–KK6 kaynak mutasyonlarıyla (davranış, tekil dizin ve filtresi, kısıt, sorgu ve bağ dizini, saklanan alan) yeniden sınandı; 130 çizgi = 130 satır = 130 FK; Consistency 101/101, Unit 241/241; dal CI `41855256783` yeşil; bulgu 0; K2 bu PR'da kapandı, K1 → D-70). Merge yardımcıda. Önceki giriş (T119 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T119 doğrulama tur 1 PASS

2026-11-04 — T119 (Başlangıç verisi göçü ve seed olmayanların sınırı) bağımsız doğrulama tur 1: ✓ PASS (head `7bf60e6`, PR #377; kapsam tam, 8 kriter ve KK1–KK8 ✓; 81 il plakayla, 249 ülke, altı tek satır, 81 teslimat ili ve iki yayımlanmamış taslak üretilen SQL'de ve gerçek PostgreSQL'de doğrulandı; `company_settings` varsayılanları `02 §11.1` ile birebir (P-1 0,00 · P-3/P-4 boş · P-5 2 · P-7 2 · P-8 5 · P-10 20,00), eşitlik denetimi mutasyonla sınandı; D-52 genişletme göçü 11 sütun `DEFAULT`; ikinci uygulama değer değiştirmez; Unit 267/267 (Consistency dahil), Integration 952/952; dal CI `41843874734` yeşil; bulgu 0; K1 → D-69, K2 bu PR'da kapandı). Merge yardımcıda. Önceki giriş (T118 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T118 doğrulama tur 1 PASS

2026-11-04 — T118 (Zamanlayıcı, yedek, imha ve anahtar kayıtları) bağımsız doğrulama tur 1: ✓ PASS (head `01d6473`, PR #376; kapsam tam, 8 kriter ve KK1–KK8 ✓; altı tablonun sütun, varsayılan, dizin ve kontrol kısıtları `06 §4.59`, §4.64–§4.68, §5, §7.1–§7.5 ile birebir; bekleyen iş tekil dizini `NULLS NOT DISTINCT`; anahtar halkası tamsayı kimlik, `created_at` yok; yetki ayrımı rol ayarından — mutasyonla sınandı, iki deyim kaldırılınca beş test kırmızı; D-39 (a) betiği sabit deyimli, yerel soket, parolasız; yeni paket MIT, kilitli; Unit 245/245, betik 4/4, Integration 926 (T118 sınıfı 55/55); dal CI `41835537615` yeşil; bulgu 0; K-notu yok). Merge yardımcıda. Önceki giriş (T118 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T118 yapım bitti

2026-11-04 — T118 (Zamanlayıcı, yedek, imha ve anahtar kayıtları) yapım bitti, doğrulama bekliyor (dal `task/T118-zamanlayici-yedek-imha-anahtar`, PR #376; altı tablo tek göçle, `Down` yok; bekleyen iş (tür, ilgili kayıt) `NULLS NOT DISTINCT` tekil; nabız ve yedek durumu tek satır; kesinti > 2 dk; imha sayı/tarih türe göre; anahtar halkası paketin şeması, salt okuma kullanıcısına kapalı; durum kullanıcısına iki sütunda güncelleme — adlar rol ayarından; D-39 (a) kapandı: veritabanı yedeği sonucunu durum kaydına yazar; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 245/245, Integration 926, betik testleri 4/4). Önceki giriş (T117 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T117 doğrulama tur 1 PASS

2026-11-04 — T117 (Gönderim kaydı ve kanal hâli) bağımsız doğrulama tur 1: ✓ PASS (head `ba81b23`, PR #375; kapsam tam, 6 kriter ve KK1–KK6 ✓; iki tablonun sütun, varsayılan, dizin, FK ve kontrol kısıtları `06 §4.58`, §4.60, §2.2.5, §2.2.6, §7.1–§7.5 ile birebir; türe göre sipariş ve ilgili kayıt türü `CASE` kısıtı tür tablosunun 11 satırıyla eşit, boş tür `IN` listesinden geçmiyor; `Undelivered`'da gönderim anı iki yönlü (K-1120); sipariş bağı `NO ACTION`, ilgili kayıt ve ilk e-posta değer; kanal hâli tek satır; Unit 245/245, Integration 871 (T117 sınıfı 30/30); dal CI `41827953732` yeşil; bulgu 0; K-notu yok). Merge yardımcıda.

---

## 2026-11-04 — T117 yapım bitti

2026-11-04 — T117 (Gönderim kaydı ve kanal hâli) yapım bitti, doğrulama bekliyor (dal `task/T117-gonderim-kaydi-kanal-hali`, PR #375; `DispatchRecord` ve `ChannelStateRecord` `Notifications` modülünde tek göçle, `Down` yok; sipariş bağı kısıtlayıcı, ilgili kayıt ve ilk e-posta değer; türe göre sipariş ve ilgili kayıt türü kontrol kısıtı, `Undelivered` satırında gönderim anı dolu da boş da kabul; kanal hâli tek satır, ilk hata anı sayı sıfırsa ve yalnız o zaman boş; kriter 1–6 ve KK1–KK6 öz denetimde ✓; Unit 245/245, Integration 871; öncesi: T116 doğrulama tur 1 PASS, giriş `STATUS_CHANGELOG.md`'ye taşındı).

---

## 2026-11-04 — T116 doğrulama tur 1 PASS

2026-11-04 — T116 (Galeri bağları) bağımsız doğrulama tur 1: ✓ PASS (head `2c19c0c`, PR #374; kapsam tam, 6 kriter ve KK1–KK6 ✓; beş tablonun sütun, varsayılan, dizin ve FK'leri `06 §4.71`, §2.2.5, §7.1, §7.2 ile birebir; (sahip, görsel) çifti tekil, `is_main` filtreli tekil dizin, sahip bağı basamaklı, görsel bağı `NO ACTION`, sıra tekil değil; Unit 245/245, Integration 841 (T116 sınıfı 47/47); dal CI `41822520597` yeşil; bulgu 0; K-notu yok). Merge yardımcıda.

---

## 2026-11-04 — T116 yapım bitti

2026-11-04 — T116 (Galeri bağları) yapım bitti, doğrulama bekliyor (dal `task/T116-galeri-baglari`; beş galeri bağı tek göçle, `Down` yok; öncesi: T115 doğrulama tur 1 PASS, giriş `STATUS_CHANGELOG.md`'ye taşındı).

---

## 2026-11-04 — T115 doğrulama tur 1 PASS

2026-11-04 — T115 (Şube, içerik–ürün bağları ve iletişim talebi) bağımsız doğrulama tur 1: ✓ PASS (head `cbced3e`, PR #373; kapsam tam, 7 kriter ve KK1–KK7 ✓; dört tablonun sütun, varsayılan, dizin, FK ve kontrol kısıtları `06 §4.50`, §4.53, §4.57, §7.1–§7.5 ile birebir; telefon `^[0-9]{10}$`, görselsiz şubede alt metin NULL, yayın durumu iki değer, il ve görsel bağı `NO ACTION`, içerik–ürün çifti tekil ve iki uca basamaklı, kapalı talepte kapanış anı dolu, imha taramasının ifade dizini; Unit 245/245, Integration 794 (T115 sınıfı 39/39); dal CI `41818311284` yeşil; bulgu 0; K-notu yok). Merge yardımcıda.

---

## 2026-11-04 — T115 yapım bitti

2026-11-04 — T115 (Şube, içerik–ürün bağları ve iletişim talebi) yapım bitti, doğrulama bekliyor (dal `task/T115-sube-icerik-urun-iletisim`, PR #373; dört entity — `Branch`, `OfferingProduct`, `PortfolioItemProduct`, `ContactRequest` — `CorporateContent` modülünde tek göçle, `Down` yok; şube ve iletişim talebi telefonu on rakam, görselsiz şubede alt metin boş, içerik–ürün çifti tekil ve içerik kaydıyla ve ürünle basamaklı, kapalı talepte son kapanış anı dolu, talebin imha taraması için ifade dizini; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 245/245, Integration T115 sınıfı 39/39). Önceki giriş (T114 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T114 doğrulama tur 1 PASS

2026-11-04 — T114 (İçerik kayıtları ve marka) bağımsız doğrulama tur 1: ✓ PASS (head `c976977`, PR #372; kapsam tam, 9 kriter ve KK1–KK9 ✓; sekiz tablonun sütun, varsayılan, dizin, FK ve kontrol kısıtları `06 §4.46`…§4.55, §7.1–§7.5 ile birebir; üç tek satırlık tablo `((true))` tekil dizinli, `Slug` filtresiz tekil ve biçim kısıtlı, marka rengi `^#[0-9A-F]{6}$`, logo ve simge bağı `NO ACTION`; Unit 245/245, Integration 882 (T114 sınıfı 79/79); bekleyen model değişikliği yok; dal CI `41812838046` yeşil; bulgu 0; K-notu yok — yerel sınıf temizliği zaman aşımı zaten D-68'de). Merge yardımcıda.

---

## 2026-11-04 — T114 yapım bitti

2026-11-04 — T114 (İçerik kayıtları ve marka) yapım bitti, doğrulama bekliyor (dal `task/T114-icerik-kayitlari-marka`; sekiz entity — `AboutPage`, `Offering`, `PortfolioItem`, `FaqItem`, `CustomPage`, `Announcement`, `SocialLink`, `Branding` — `CorporateContent` modülünde tek göçle, `Down` yok; üç tek satırlık tablo sabit ifadenin tekil dizinini taşır; yayın durumu yalnız taslak ve yayımlanmış; `CorporateContentRecordsGuard` hakkımızda ve marka satırının silinmesini reddeder; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 245/245, Integration yerel 754/755, kalan bir test yerel zaman aşımı). Önceki giriş (T113 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T113 doğrulama tur 1 PASS

2026-11-04 — T113 (İade reddi ve IBAN isteği) bağımsız doğrulama tur 1: ✓ PASS (head `429ce56`, PR #371; kapsam tam, 7 kriter ve KK1–KK7 ✓; `refund_by_reason` ve `itemless_on_refund_failure` `06 §4.29`/§7.3 ile birebir, iki filtreli tekil dizinin filtresi `06 §7.1` ile eşit; FK silme davranışları `06 §7.2` ile birebir; iade reddi bekçide silinmez, IBAN isteği imhanın açık silmesine açık; Unit 245/245, Integration 676 (T113 sınıfı 45/45); bekleyen model değişikliği yok; dal CI `41807318144` yeşil; bulgu 0; K1 → D-68 (yerel test altyapısında sınıf veritabanı düşürme zaman aşımı)). Merge yardımcıda.

---

## 2026-11-04 — T113 yapım bitti

2026-11-04 — T113 (İade reddi ve IBAN isteği) yapım bitti, doğrulama bekliyor (dal `task/T113-iade-reddi-iban-istegi`, PR #371; iki entity — `ReturnRejection`, `RefundIbanRequest` — `AfterSales` modülünde tek göçle, `Down` yok; iade reddinin silinmesini `AfterSalesRecordsGuard` reddeder, IBAN isteği imhanın açık silmesine açık; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 245/245, Integration 676 (T113 sınıfı 45/45); D-55 eki (numaralı dizin adları)).

---

## 2026-11-04 — T112 doğrulama tur 1 PASS

2026-11-04 — T112 (Ödeme ve geri ödeme kayıtları) bağımsız doğrulama tur 1: ✓ PASS (head `d452256`, PR #370; kapsam tam, 7 kriter ve KK1–KK7 ✓; tutar sütunları `numeric(12,2)` (`06 §2.5.1`), `ck_refunds_links_by_type` `06 §4.33` tür tablosuyla birebir, altı filtreli tekil dizin `06 §7.1` ile eşit; FK ve dizinler `06 §7.2`, §7.5 ile birebir; Unit 245/245, Integration 631 (T112 sınıfı 85/85); bekleyen model değişikliği yok; dal CI `41802135141` yeşil; bulgu 0; K-notu yok). Merge yardımcıda. Önceki giriş (T112 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T112 yapım bitti

2026-11-04 — T112 (Ödeme ve geri ödeme kayıtları) yapım bitti, doğrulama bekliyor (dal `task/T112-odeme-geri-odeme-kayitlari`, PR #370; dört entity — `PaymentRecord`, `Refund`, `RefundRecord`, `InboundNotificationRecord` — `Payment` modülünde tek göçle, `Down` yok; üç kalıcı kaydın silinmesini `PaymentRecordsGuard` reddeder; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 245/245, Integration 631 (T112 sınıfı 85/85); D-51 eki (iki kırpılmış ad), D-55 eki (numaralı dizin adları)). Önceki giriş (T111 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T111 doğrulama tur 1 PASS

2026-11-04 — T111 (Kalem kayıtları) bağımsız doğrulama tur 1: ✓ PASS (head `6cefd1c`, PR #369; kapsam tam, 8 kriter ve KK1–KK8 ✓; tutar sütunları `numeric(12,2)` ve tutar kısıtları `06 §2.5`, §4.22–4.25 ile eşit; FK ve dizinler `06 §7.2`, §7.5 ile birebir; Unit 245/245, Integration 546 (T111 sınıfı 68/68); yetki matrisi sapmasız (59/59); dal CI `41796325905` yeşil; bulgu 0; K1 → D-67, K2 → D-51 eki). Merge yardımcıda. Önceki giriş (T111 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T111 yapım bitti

2026-11-04 — T111 (Kalem kayıtları) yapım bitti, doğrulama bekliyor (dal `task/T111-kalem-kayitlari`; sekiz entity `AfterSales` modülünde tek göçle, `Down` yok; altı kalıcı kaydın silinmesini `AfterSalesRecordsGuard` reddeder; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 245/245, Integration 546 (T111 sınıfı 68/68); D-67 yeni — `DefectClaim.StatusHistory` düz `jsonb`, hedef T320; D-51 eki). Önceki giriş (T110 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T110 doğrulama tur 1 PASS

2026-11-04 — T110 (Ayırma ve düzeltme geçmişleri) bağımsız doğrulama tur 1: ✓ PASS (head `a7954b7`, PR #367; kapsam tam, 7 kriter ve KK1–KK7 ✓; 15 ham SQL yoklaması, `confdeltype` `06 §7.2` ile eşit; Unit 245/245, Integration 478 (T110 sınıfı 28/28); yetki matrisi 8/8; dal CI `41784121098` yeşil; bulgu 0; K1 → D-66 — iki geçmişin public ayarlayıcıları, hedef T375). Merge yardımcıda. Önceki giriş (T110 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T110 yapım bitti

2026-11-04 — T110 (Ayırma ve düzeltme geçmişleri) yapım bitti, doğrulama bekliyor (dal `task/T110-ayirma-duzeltme-gecmisleri`; `StockReservation`, `OrderEmailHistory`, `OrderAddressHistory` tek göçle, `Down` yok; iki geçmiş `OrderingRecordsGuard` ile yalnız eklenir; Unit 245/245, Integration 478/478 (T110 sınıfı 28/28); D-54 ✅ — kupon bağı kısıtlayıcı, ham `DELETE` `fk_stock_reservations_coupons_coupon_id` ile reddedilir).

---

## 2026-11-04 — T109 doğrulama tur 1 PASS

2026-11-04 — T109 (Sepet ve sipariş çekirdeği) bağımsız doğrulama tur 1: ✓ PASS (head `de1ae6c`, PR #366; kapsam tam, 8 kriter ve KK1–KK8 ✓; ~95 ham SQL yoklaması, tutar sütunları `numeric(12,2)`; Unit 245/245, Integration 450 (T109 sınıfı 65/65); yetki matrisi sapmasız; dal CI `41780080190` yeşil; bulgu 0; K1 → D-65 — `Order.ShippingFeeVatBreakdown` `PayloadJson` dönüştürücüsü bekliyor; `12`'de T109'a bağlı altı satır BEKLEMEDE). Merge yardımcıda. Önceki giriş (T109 yapım) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T109 yapım bitti

2026-11-04 — T109 (Sepet ve sipariş çekirdeği) yapım bitti, doğrulama bekliyor (dal `task/T109-sepet-siparis-cekirdegi`; sekiz tablo tek göçle, `Down` yok; `OrderingRecordsGuard`; Unit 245/245, Integration 450/450 (T109 sınıfı 65/65); D-54'ün T109 payı kapandı, T110 payı açık; D-51'e dört kırpılmış ad eklendi). Önceki giriş (T108 doğrulama tur 2) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T108 doğrulama tur 2 PASS

2026-11-04 — T108 (Oturum, bağlantı, deneme ve giriş kayıtları) bağımsız doğrulama tur 2: ✓ PASS (head `04113f7`, PR #365; kapsam tam, 8 kriter ✓; tur 1 bulgusu — kayıtların yeri — giderildi; Unit 245/245, Integration 385 (T108 sınıfı 70/70); dal CI `41775169027` yeşil; bulgu 0; K1 → D-64 — normalleştirilmiş değerin tek yazarı `09 §9.4` m.1 ile `§15.4` m.5–6 arasında; `12`'de T108'e bağlı beş satır BEKLEMEDE). Merge yardımcıda. Önceki giriş (T108 yapım ve düzeltme 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T108 yapım bitti, doğrulama tur 1 FAIL ve düzeltme 1

2026-11-04 — T108 (Oturum, bağlantı, deneme ve giriş kayıtları) yapım bitti; doğrulama tur 1 FAIL sonrası düzeltme 1 yapıldı (beş varlık Infrastructure/Security/ altına taşındı, D-62 kapandı), doğrulama tur 2 bekliyor (dal `task/T108-oturum-baglanti-deneme-giris`): beş entity (`Session`, `RecognizedBrowserMark`, `LinkRecord`, `AttemptRecord`, `LoginRecord`) tek göçle; dört modüller arası bağ D-61 kalıbıyla adla kuruldu ve D-61 kapandı (`09 §10.1` + kaynak taraması). Önceki giriş (T107 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T107 doğrulama tur 1 PASS

2026-11-04 — T107 (Müşteri hesabı) bağımsız doğrulama tur 1: ✓ PASS (head `e765c68`, PR #364; kriter 1–6 ve KK1–KK6 ✓; Unit 240/240, Integration 315/315, betik testleri 4/4; dal CI `41769181335` yeşil; bulgu 0; K-notu yok — modüller arası bağın ad kalıbı D-61'de, gerekçesi yerinde; `12`'de T107'ye bağlı satır yok, `VAL-D121` BEKLEMEDE). Merge yardımcıda. Önceki giriş (T107 yapım) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T107 yapım bitti

2026-11-04 — T107 (Müşteri hesabı) yapım bitti, doğrulama bekliyor (dal `task/T107-musteri-hesabi`): üç entity (`Customer`, `PendingEmailChange`, `AddressBook`) tek göçle; ilk modüller arası yabancı anahtarlar adla bağlandı (⚠ D-61); Unit 240/240, T107 sınıfı 39/39. Önceki giriş (T106 doğrulama tur 3) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T106 doğrulama tur 3 PASS

2026-11-04 — T106 (Yönetici hesabı, davet ve işlem izi) bağımsız doğrulama tur 3 (eskalasyon sonrası): ✓ PASS (head `d3baaa6`, PR #363; kapsam tam; kriter 1–7 ve KK1–KK7 ✓; tur 1–2'nin sır bekçisi bulguları kapandı; Unit 240/240, Integration 276/276, bekçi betiği 208/208; dal CI `41765579435` yeşil; bulgu 0; `VAL-A838` PASS; K1 → D-59 — imha testlerinin sabit 2020 tarihi 2031'de eskir, K2 → D-60 — bekçi istisnasının kalan dar açıkları README'de). Merge yardımcıda.

---

## 2026-11-04 — T106 yapım bitti, doğrulama tur 1–2 FAIL ve düzeltmeler

2026-11-04 — T106 (Yönetici hesabı, davet ve işlem izi) yapım bitti, doğrulama bekliyor: üç entity (`Admin`, `AdminInvitation`, `AuditLog`) tek göçle; işlem izi uygulama kullanıcısına yalnız `INSERT`/`SELECT`, imha fonksiyonu `dispose_expired_audit_logs`; `AuditLog.Operation` düz metin, `Details` `string` + `jsonb`; `MigrationSourceScan` genişledi; sır bekçisine EF göç sütun kurucusu istisnası; D-48 (a) ve D-49 ✅, D-57 açıldı; Unit 240/240, Integration 276/276. Doğrulama tur 1 FAIL (sır bekçisi istisnası geniş); düzeltme 1: istisna yalnız `type: "…"` dizgisine daraltıldı, tur 2 FAIL (açık çok satırlı biçim istisna dışında kalmalı); düzeltme 2: istisna yalnız aynı satırda kapanan `table.Column<Tür>(…)` biçimine uygulanır, yeniden doğrulama bekliyor. Önceki giriş (T105 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T105 doğrulama tur 1 PASS

2026-11-04 — T105 (Fiyat, kupon ve dijital dosya) bağımsız doğrulama tur 1: ✓ PASS (head `67e983a`, PR #362; kriter 1–7 ve KK1–KK7 ✓ — göç PostgreSQL 18'e uygulandı, 34 sütun, kısıtlar, dizinler ve silme eylemleri `06` ile birebir, 42 ham SQL yoklamasıyla sınandı; Unit 230/230, Integration 234/234; dal CI `41749429553` yeşil; bulgu 0; K1 → D-56 — `06 §7.3`'ün kupon satırında yüzde aralığı yok). Merge yardımcıda.

---

## 2026-11-04 — T105 yapım bitti

2026-11-04 — T105 (Fiyat, kupon ve dijital dosya) yapım bitti: dört entity (`PriceHistory`, `Discount`, `Coupon`, `DigitalFile`) tek göçle; fiyat geçmişi güncellemesi ve kupon silmesi `CatalogRecordsGuard` ile reddedilir; Unit 230/230, Integration 234/234; yeni D-54 (bağlı kuponun DB testi T109/T110'a), D-55 (aynı sütunda iki dizinin `…_id1` adı). Doğrulama bekliyor. Önceki giriş (T104 doğrulama tur 3 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T104 doğrulama tur 3 PASS

2026-11-04 — T104 (Ürün çekirdeği ve görsel) bağımsız doğrulama tur 3: ✓ PASS (head `fa4c2a0`, PR #361; kapsam tam; kriter 1–8 ve KK1–KK8 ✓ — göç PostgreSQL 18'e uygulandı, altı sütunun veritabanı varsayılanı `06` ile birebir (tur 2 bulgusu kapandı), kısıtlar ve silme eylemleri 22 ham SQL yoklamasıyla sınandı; Unit 230/230, Integration 187/187; dal CI `41743500401` yeşil; bulgu 0; K1 → D-53 — kaynak kopyanın 2560 piksellik kenarı `ImagingLimits` gelince tek yerde kalmalı). Merge yardımcıda. Önceki giriş (T104 düzeltme 2 ve doğrulama tur 2 FAIL) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T104 düzeltme 2 ve doğrulama tur 2 FAIL

2026-11-04 — T104 düzeltme 2: altı sütuna veritabanı varsayılanı verildi (`HasDefaultValue`, göç yerinde; Unit 230, Integration 187), yeniden doğrulama bekliyor. Önceki: T104 (Ürün çekirdeği ve görsel) bağımsız doğrulama tur 2: ✗ FAIL (head `1b9132d`, PR #361; kapsam tam; kriter 2–8 ve KK1–KK8 ✓, tur 1'in üç bulgusu kapandı, D-50 (a)(b)(c) doğru; Unit 230/230, Integration 185/185; dal CI `41739943430` yeşil; bulgu 1 S1 DÜŞÜK, kriter 1: `06 §4` girişi "Varsayılan veritabanının varsayılanıdır" der, altı sütunda DB default yok — `products.publish_status` 'Draft', `products.version` 1, `variants.publish_status` 'Draft', `variants.reserved_quantity` 0, `product_categories.is_primary_category` false, `product_images.is_main` false; ham `INSERT` 23502 ile düşüyor; K1 → D-52 — aynı boşluk T103'te). İkinci iterasyon FAIL → `12 §8.5` eskalasyon — yardımcı karar verecek. Önceki giriş (T104 doğrulama tur 1 FAIL) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T104 doğrulama tur 1 FAIL

2026-11-04 — T104 (Ürün çekirdeği ve görsel) bağımsız doğrulama tur 1: ✗ FAIL (head `b4c73c8`, PR #361; kriter 1–8 ve KK1–KK8 ✓ — göç PostgreSQL 18'e uygulandı, silme eylemleri `pg_constraint`'te, trigram dizini planda sınandı; Unit 225/225, Integration 183/183; dal CI `41735064804` yeşil; `09 §25` üç soruda "hayır" → bulgu 3, hepsi S1 DÜŞÜK: (1) trigram dizinini kuran göçün ilk deyiminde `pg_trgm` denetimi yok ve `09 §10.6` m.11'in iki testi eksik — eklentisiz veritabanında göç çatının iletisiyle (`42704 operator class "gin_trgm_ops" does not exist`) düşüyor; (2) `Variant.Sku`/`NormalizedSku` — `06 §4.6` ve §4'ün girişi `SKU`/`NormalizedSKU` der; (3) `TextNormalizer.Simplify` Latin olmayan harflerin de işaretini atıyor (`й`→`и`, `ά`→`α`, `कु`→`क`) — `09 §9.4` m.2 "yalnız aksanlı Latin harfler; başka karaktere dokunulmaz"; K1 → D-51, K2 → D-45). Sırada: T104 düzeltme 1. Önceki giriş (T103 doğrulama tur 2 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T103 doğrulama tur 2 PASS

2026-11-04 — T103 (Başlangıç listeleri, firma ve yasal metin sürümü) bağımsız doğrulama tur 2: ✓ PASS (head `7c04318`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — göç kurucu SQL'le kurulan PostgreSQL 18'e göç kullanıcısıyla uygulandı, KK2–KK7 uygulama ve salt okuma kullanıcısıyla ham SQL'le de sınandı; Unit 217/217, Integration 123/123; dal CI `41725873130` yeşil; tur 1 bulgusu kapandı — tek satırlık iki tabloda yalnız `pk_*` ve `ix_*_single_row`, yeni dizin kuralı öteki tablolarda çerçevenin kuralıyla aynı dizinleri üretir; bulgu 0, K1–K3 → D-50). PR #360 merge ve post-merge CI yardımcıda. Önceki giriş (T103 doğrulama tur 1 FAIL) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T103 doğrulama tur 1 FAIL

2026-11-04 — T103 (Başlangıç listeleri, firma ve yasal metin sürümü) bağımsız doğrulama tur 1: ✗ FAIL (`6e88812`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — göç kurucu SQL'le kurulan PostgreSQL 18'e uygulandı, KK2–KK6 ham SQL'le de sınandı; dal CI run 41721390301 yeşil; bulgu 1 S1 DÜŞÜK: tek satırlık `companies` ve `company_settings`'te yabancı anahtar dizini var, `06 §7.5.2` "yalnız tek satırın tekil dizini" der — kaldırılabilir olduğu gösterildi; K1, K3 düzeltme turunda, K2 → D-49). Sırada: T103 düzeltme 1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T103 yapım bitti).

---

## 2026-11-04 — T103 yapım bitti

2026-11-04 — T103 (Başlangıç listeleri, firma ve yasal metin sürümü) yapım bitti: altı tablo tek göçle, tekil satır dizinleri, kontrol kısıtları, kalıcı satır bekçisi, göç geçmişi tablosu kapatma; Unit 217/217 + Integration 119/119 (PostgreSQL 18); D-37 (a), D-46 (a)(b), D-48 (b) çözüldü. Önceki giriş (T102 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-03 — T102 doğrulama tur 1 PASS

2026-11-03 — T102 (Enum tanımları ve değer dönüşümü) bağımsız doğrulama tur 1: ✓ PASS (head `8277427`; kriter 1–6 ve KK1–KK6 ✓; Unit 199/199, Integration 94/94 PostgreSQL 18; dal CI `41712894839` yeşil; bulgu 0, K1–K2 → D-48; D-47 hedefleri düzeltildi). PR #359 merge ve post-merge CI yardımcıda.

---

## 2026-11-03 — T102 yapım bitti

2026-11-03 — T102 (Enum tanımları ve değer dönüşümü) yapım bitti: 42 enum, metin dönüşümü, izinli değer kısıtı, kilit dosyaları; Unit 199/199, Integration 94/94 (PostgreSQL 18); doğrulama bekliyor.

---

## 2026-11-03 — T101 doğrulama tur 1 PASS

2026-11-03 — T101 (Veri modeli temeli) bağımsız doğrulama tur 1: ✓ PASS (head `f7b33c7`; kriter 1–8 ve KK1–KK8 ✓; Unit 128/128, Integration 81/81 PostgreSQL 18.6; dal CI `41706961892` yeşil; bulgu 0, K1–K3 → D-46). PR #358 merge ve post-merge CI yardımcıda.

---

## 2026-11-03 — T101 yapım bitti

2026-11-03 — T101 (Veri modeli temeli) yapım bitti, PR #358, doğrulama bekliyor.

---

## 2026-11-03 — F0 faz kapısı PASS

2026-11-03 — F0 (Proje iskeleti) faz kapısı: ✓ PASS (`GATE_CHECK_F0.md`; ana dal `93ba91f`; testler, üç imaj, yerel referans kurulumda taze ortam ve canlılık 200, Lint kırmızı→yeşil; `11 §7.1`'in F0'a bağlı 66 satırı dolduruldu; backlog hijyeni: D-07, D-11, D-18, D-23 ✅, yeni D-43, D-44). Sırada: gate PR'ının merge'ü ve `phase/F0-pass` etiketi (yardımcı), sonra F1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 doğrulama tur 2 PASS).

---

## 2026-11-03 — T09 doğrulama tur 2 PASS

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) bağımsız doğrulama tur 2: ✓ PASS (`303c074`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — kurulum validator tarafından reçeteyle sıfırdan kuruldu; dal CI run 41692602124 yeşil; tur 1 bulgu 1 ve K1–K5 kapandı; K-notları D-42). Sırada: merge (yardımcı), sonra F0 gate. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 doğrulama tur 1 FAIL).

---

## 2026-11-03 — T09 doğrulama tur 1 FAIL

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) bağımsız doğrulama tur 1: ✗ FAIL (`a4632b1`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — kurulum validator tarafından reçeteyle sıfırdan kuruldu; dal CI run 41687547294 yeşil; bulgu 1 S1 DÜŞÜK: `E2E_CMD=NOT_APPLICABLE` gerekçesi "uçtan uca faz yok" `11` F6 (Uçtan uca doğrulama) ve `05 §8.3.11` ile çelişir, bağlama izlenmiyor; K1–K5 düzeltme turunda). Sırada: T09 düzeltme 1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 yapım bitti).

---

## 2026-11-03 — T09 yapım bitti

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) yapım bitti, ⏳ doğrulama bekliyor: yerel referans kurulum (dind sunucu, MinIO, Caddy iç sertifikası, sahte sırlar) iki kez sıfırdan koştu; canlılık 200, sır ve kullanıcı doğrulamaları, `DOCKER-USER` çıkış denemesi, IPv4 ve IPv6 istemci adresi, yedek ve kurtarma provası, imaj kimlikleri `DEPLOY_RUNBOOK` §K'ya yazıldı; `SETUP_COMPLETE=true`, D-01 ✅, gerçek sunucu artıkları D-40/T603. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 plan düzeltmesi).

---

## 2026-11-03 — T09 plan düzeltmesi

2026-11-03 — T09 plan düzeltmesi (PLAN_CORRECTION_REQUIRED; PR #353; proje sahibinin kararı): T09 "İlk iniş provası: yerel referans kurulumda canlılık" oldu (Docker içinde ayrıcalıklı Linux konteyneri, MinIO, sahte sırlar) ve ⬚ Bekliyor'a döndü; gerçek sunucuya ilk iniş yeni F6 task'ı T603'e taşındı (⬚); D-01'in gerçek sunucu artıkları `DEFERRED_BACKLOG.md` D-40'ta. Toplam 215 task. T01–T08 ve T10 ✓. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 BLOCKED OWNER_INPUT).

---

## 2026-11-03 — T09 BLOCKED (OWNER_INPUT)

2026-11-03 — T09 (İlk yayın ve referans kuruluma ilk iniş) ⛔ BLOCKED (OWNER_INPUT): kabul kriteri 3–8 referans sunucuyu (Linux, iptables, IPv4 + IPv6), firmanın nesne depolama kovası ve anahtarlarını, alan adını ve kurulum sırlarını ister; bunlar proje sahibinden beklenir. T09 bu girdiler gelince yapılır; F0 gate'i T09'dan sonra (`00`: gate PASS olmadan sonraki faza geçilmez). T01–T08 ve T10 ✓.

---

## 2026-11-03 — T07 doğrulama tur 4 PASS

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 4 (düzeltme 2 sonrası, `main` ile birleşik hâl): ✓ PASS (`aa1cfe6`; kapsam tam; 7/7 kabul kriteri, KK1–KK7 ✓; dal CI run 41670381320 yeşil; tur 3'ün bulgusu (`VAL-F016`, prova son anı) kapandı — yerel Docker'da gerçek imajla prova kurtarmanın bitmesini bekledi, `last-moment` birincil sunucudaki son işlemle aynı, günlükler `--repo=2` ile dış depodan; bulgu 0; K-notları: `core.hooksPath` yine mutlak yol (düzeltildi), D-39 (d) kapandı). Merge yardımcıda (PR #351). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 doğrulama tur 3 FAIL).

---

## 2026-11-03 — T07 doğrulama tur 3 FAIL

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 3 (tur 2 PASS'ten sonra `main` ile birleşik hâl): ✗ FAIL (`c929163`; kapsam tam; kriter 1–6 ✓, 7 ✗; dal CI run 41665382289 yeşil; bulgu 1 S1 ORTA `VAL-F016`: `database-restore-drill.sh` sıcak beklemede açılan kopyada son anı günlük uygulaması bitmeden okur — kayıt son tam yedeğin anını ve yalnız temel yedeğin indirme süresini taşır; K-notları: `core.hooksPath` mutlak yol (düzeltildi), PR açıklamasındaki test sayıları; iki doğrulayıcıyla, ilki bölündü). Düzeltme turu gerekli; merge yok (PR #351). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 doğrulama tur 2 PASS).

---

## 2026-11-03 — T07 doğrulama tur 2 PASS

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 2: ✓ PASS (`501c24b`; kapsam tam; 7/7 kabul kriteri, KK1–KK7; dal CI run 41660924636 yeşil; yerel Docker'da gerçek iki imajla dosya yedeğinin dört adımı `success`, prova üç kayıt alanını yazdı; tur 1'in dört bulgusu ve K1–K7 kapandı; bulgu 0; K-notu D-39 (f); iki doğrulayıcıyla, ilki bölündü); merge yardımcıda (PR #351). F0 sırada sonraki: T10. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 yapım bitti).

---

## 2026-11-03 — T10 doğrulama tur 2 PASS

2026-11-03 — T10 (Veritabanı kullanıcıları) bağımsız doğrulama tur 2: ✓ PASS (`76979dc`; kapsam tam; 8/8 kabul kriteri, KK1–KK8; dal CI run 41661056056 yeşil; gerçek veritabanı imajında `log_statement=all` ile ilk kurulum — günlükte açık parola yok, yalnız üç SCRAM özeti; tur 1'in bulgu 1'i kapandı; bulgu 0; K-notları D-38); merge yardımcıda (PR #350). F0'da kalan: T07, ardından T09 (T09 T07'ye bağlı). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T10 yapım bitti).

---

## 2026-11-03 — T07 yapım bitti, doğrulama bekliyor

2026-11-03 — T07 (Yedek düzeni) yapım bitti, PR açıldı, bağımsız doğrulama bekliyor: gecelik tam yedek (pgBackRest, iki depo) ve dosya yedeği (restic, dört adım) betikleri, `archive_timeout` 50 s, kova kuralı ve anahtar politikası, kurtarma provası runbook adımı (`DEPLOY_RUNBOOK` §J); betik testleri ve 15 eşitlik denetimi yerelde yeşil, imaj çalıştırılarak sınandı; D-28, D-30 (b), D-34 (a) kapandı, D-39 açıldı. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T08 doğrulama tur 4 PASS).

---

## 2026-11-03 — T10 yapım bitti

2026-11-03 — T10 (Veritabanı kullanıcıları) yapım bitti, doğrulama bekliyor: kurucu betik (`deploy/images/database/initdb/10-bootstrap.sh`; SQL'ler ve `pg_hba`/`pg_ident` şablonları `bootstrap/`'ta) dört kullanıcıyı, veritabanını (UTF-8, ICU tr-TR), yetkileri, `pg_trgm`'i ve üç rol ayarını kurar; yedek durum kullanıcısı `peer` + `pg_ident`, kurucu ağdan `reject`, `trust` yok; yetki matrisi ve kullanıcı testleri gerçek PostgreSQL 18'de betiğin SQL'leriyle koşar; CI Paketleme adımı ilk kurulumu sınar; D-01 ve D-31'e not. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T08 doğrulama tur 4 PASS).

---

## 2026-11-03 — T08 doğrulama tur 4 PASS

2026-11-03 — T08 (Sır düzeni) bağımsız doğrulama tur 4: ✓ PASS (`363dd4d`; kapsam tam; 6/6 kabul kriteri, KK1–KK6; dal CI run 41646506249 yeşil; yerel Docker'da 15 sır dosyası × 3 kullanıcı ve dalın yayını uygulama imajında — dosya ortamı yener, geliştirme yolu ortamdan, ikisi yoksa açılmaz; tur 3'ün iki bulgusu ve K1–K5 kapandı; bulgu 0; K-notları D-36); merge yardımcıda (PR #349). F0 sırada sonraki: T07. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T05 doğrulama tur 2 PASS).

---

## 2026-11-03 — T05 doğrulama tur 2 PASS

2026-11-03 — T05 (Compose geliştirme ortamı) bağımsız doğrulama tur 2: ✓ PASS (9/9 kabul kriteri, KK1–KK9; dal CI run 41627237288 yeşil; yerel Docker'da üç imaj dal Dockerfile'larından derlendi — yalıtım, HTTP→HTTPS + bir yıllık HSTS, Mailpit, `DOCKER-USER` kuralı atılık konteynerde sınandı; plan düzeltmesi `11`/`12`/D-31'e tutarlı; bulgu 0; K-notları D-34; iki doğrulayıcıyla, ilki bölündü); merge yardımcıda (PR #348). F0 sırada sonraki: T08. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T05 yapım bitti).

---

## 2026-11-03 — T05 yapım bitti, doğrulama bekliyor

2026-11-03 — T05 (Compose geliştirme ortamı) yapım bitti, PR #348 açık, doğrulama bekliyor (⏳); `deploy/` altında Compose, geliştirme bindirmesi, Caddyfile, sürüm dosyası ve güvenlik duvarı betikleri; yerelde Docker kapalıydı, `docker compose config` geçerli, daemon gereken sınamalar (KK2, KK4, KK8, KK9) D-31 ile T09'a bağlandı; D-01 K-987 doğrulamaları yazıldı (`SETUP_COMPLETE` false), D-29 (c) kapandı, D-31..D-33 açıldı; önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T06 doğrulama tur 2 PASS).

---

## 2026-11-03 — T06 doğrulama tur 2 PASS

2026-11-03 — T06 (Üç imaj ve kayıt defteri) bağımsız doğrulama tur 2: ✓ PASS (8/8 kabul kriteri, KK1–KK8; dal CI run 41594485717 yeşil — Paketleme 7/7: üç imaj, yanlış restic özeti `did NOT match` ile düşer, kullanıcılar kök değil, canlılık 200, `track_commit_timestamp` on; taban imaj özetleri ve restic özeti/lisansı kayıt defterlerinden doğrulandı; yerelde Docker kapalıydı; K-notları D-29, D-30); merge yardımcıda (PR #347). F0 sırada sonraki: T05.

---

## 2026-11-03 — T06 yapım bitti, doğrulama bekliyor

2026-11-03 — T06 (Üç imaj ve kayıt defteri) yapım bitti, PR #347 açık, doğrulama bekliyor (⏳); D-01 kalemleri (PACKAGE_BUILD/PUBLISH, doğrulanacaklar) ve D-19 kararı raporda; önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T01 doğrulama tur 4 PASS).

---

## 2026-11-03 — T01 doğrulama tur 4 PASS

2026-11-03 — T01 (Canlılık ucu) bağımsız doğrulama tur 4: ✓ PASS (6/6 kabul kriteri; 62/62 test — Lint 9, Unit 24, Integration 29 PostgreSQL 18.6'da; Kestrel'de dondurulmuş veritabanında 503 ayarın süresinde; dal CI yeşil; K-notları D-25, D-26); merge yardımcıda (PR #338). F0 sırada sonraki: T06.

---

## 2026-11-03 — T01 yapım bitti, doğrulama bekliyor

**Son güncelleme:** 2026-11-03 — T01 (Canlılık ucu) yapım bitti, PR açık, doğrulama bekliyor (⏳); önceki: T03 (Test altyapısı) bağımsız doğrulama tur 3: ✓ PASS (9/9 kabul kriteri; 36/36 test PostgreSQL 18.6'da, kilit dosyaları koşu sonrası değişmiyor; K-notu D-17'ye eklendi); merge yardımcıda (PR #337). F0 sırada sonraki: T01.

---

## 2026-11-03 — T04 doğrulama tur 2 PASS

T04 (CI hattı ve eşitlik denetimleri) bağımsız doğrulama tur 2: ✓ PASS (9/9 kabul kriteri; dal CI'ı run 41548360499 yeşil — Lint 9, Unit 15, Integration 14 PostgreSQL 18.6 hizmetinde, dry-run atlama satırları; sondajlar geçici kopyada; tur 1'in iki bulgusu kapandı; K-notları D-23, D-24); merge yardımcıda (PR #339).

---

## 2026-11-03 — T03 doğrulama tur 3 PASS

**Son güncelleme:** 2026-11-03 — T03 (Test altyapısı) bağımsız doğrulama tur 3: ✓ PASS (9/9 kabul kriteri; 36/36 test PostgreSQL 18.6'da, kilit dosyaları koşu sonrası değişmiyor; K-notu D-17'ye eklendi); merge yardımcıda (PR #337). F0 sırada sonraki: T01.

---

## 2026-11-03 — T03 yapım bitti, doğrulama bekliyor

2026-11-03 — T03 (Test altyapısı) yapım bitti, ⏳ doğrulama bekliyor (PR #337). T02 ✓ (PR #335).

---

## 2026-11-02 — Implementation dönemi açıldı

Implementation dönemi açıldı: tracker `11`'in faz ve task listesiyle dolduruldu (7 faz, 214 task, hepsi ⬚); sıradaki iş `/task T02` (önce D-01 kapısı, `Docs/PRODUCT_DISCOVERY_STATUS.md` §25).

---
