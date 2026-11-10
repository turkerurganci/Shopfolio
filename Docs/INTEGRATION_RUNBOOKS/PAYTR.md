# PayTR — Entegrasyon Runbook

**Son güncelleme:** 2026-11-10 | **İlgili task:** T401, T402 | **Spec:** [`../08_INTEGRATION_SPEC.md`](../08_INTEGRATION_SPEC.md) §2.1 (2.1.2 anahtarlar, 2.1.4 başlatma, 2.1.5 bildirim, 2.1.10 test), §4.1 (DV tablosu)

> **Kural:** Bir dış servise **ilk kez bağlanan task**, aynı PR'da bu runbook'u da yazar.
> "Sonra yazarız" denen kurulum bilgisi, ikinci kez lazım olduğunda kaybolmuş olur.

> **Bu sürümün kapsamı (T401, T402, T403):** anahtarlar, ödemenin başlatılması (jeton isteği), dönüş adresi ve çerçeve (T401); bildirim imzası, alan eşlemesi ve red nedeninin seyreltilmiş kaydı (T402); son sorgu (`durum-sorgu`), yanıt tablosu, tutar metni ve başarının beş koşulu (T403). Hata tablosunun tamamı T404'te, kart iadesi T405'te bu dosyaya eklenir. Ödeme sayfası (çerçeveyi taşıyan sayfa, UC-29'un çağıranı) T510'dadır.

---

## 1. Hesap açma

1. Firma (referans kurulumda geliştiricinin kendi tüzel kişiliği — `10 §4.1` ÖK-12) PayTR ile sözleşme yapar ve mağaza hesabı açar; komisyon teklifi alınır, PayTR'nin ödeme kuruluşu lisansı TCMB'nin resmî kaydında doğrulanır (`DEPLOY_RUNBOOK` kurulum notu K-1254).
2. Mağaza canlıya alınmadan önce **test modundadır**; bu hâlde bütün işlemler testtir ve canlıya geçmeden önce test ödemesi beklenir (`08 §2.1.10`).
3. Mağaza panelinde bildirim adresi tanımlanır (§5). Bildirim ucu T402'den beri PayTR'nin imzasını doğrular ve bildirimi sınıflar; kart anahtarları tanımlı değilse imza hesaplanamaz ve her bildirim imzası tutmayan istek sayılır (400).

## 2. Kimlik bilgisi alma

| Bilgi | Nereden alınır | Nerede saklanır | Rotasyon |
|---|---|---|---|
| `merchant_id` | Mağaza paneli → *Bilgi* sayfası | Sır dosyası `payment_provider_keys` (`DEPLOY_RUNBOOK §B` satır 12); sır değildir ama aynı ayarın parçasıdır | Anahtar değişikliğiyle birlikte |
| `merchant_key` | Mağaza paneli → *Bilgi* sayfası | Aynı sır dosyası; yalnız `app` alır | Olay üzerine; açık kart işi kalmadığında (`08 §2.1.2`); `app` yeniden başlar |
| `merchant_salt` | Mağaza paneli → *Bilgi* sayfası | Aynı sır dosyası; yalnız `app` alır | `merchant_key` ile birlikte |

**Sır dosyasının içeriği** (`Payment:ProviderKeys`; T401): üç `ad=değer` satırı, sıra serbest — boş satır ve satır sonu `\r` yok sayılır:

```
merchant_id=<mağaza no>
merchant_key=<anahtar>
merchant_salt=<tuz>
```

Satırlardan biri eksik, boş, iki kez yazılmış ya da dosyada bu üç addan başka bir ad varsa **ayar tanımlı sayılmaz**: kart yöntemi görünmez, açılış kaydı kart hattını kapalı yazar ve sağlayıcıya çağrı yapılmaz (`08 §2.1.2`; `02 §3.21.4`). Değerler hiçbir kayda yazılmaz; kapalı hâlin kaydı yalnız ayarın adını (`Payment:ProviderKeys`) taşır. Ayrı test anahtarı yoktur: test işlemi aynı anahtarlarla imzalanır (DV-41).

## 3. Plan / kota

| Plan | Limit | Ücret | MVP'ye yeter mi |
|---|---|---|---|
| Sözleşmeye bağlı (komisyon) | Belgede çağrı kotası yazılı değil | Firmanın sözleşmesindeki komisyon | Evet — sipariş başına tek jeton isteği (sayfa her açılışta yeni jeton ister) |

> **Dış varsayım kanıtı:** dev.paytr.com iFrame API 1. adım (2026-10-29 okuması; `08 §2.1` ve §2.1.10). Çağrı kotası ve ücret firmaya özgüdür; sözleşmede okunur.

## 4. Konfigürasyon

| Ayar | Değer | Nerede tanımlı |
|---|---|---|
| Jeton isteğinin adresi | `POST https://www.paytr.com/odeme/api/get-token` — sunucudan sunucuya, form kodlu | Kodda sabit (`PayTrStartForm.TokenAddress`; `08 §2.1.4`) |
| Çerçeve adresi | `https://www.paytr.com/odeme/guvenli/{jeton}` — jeton saklanmaz, sayfa her açılışta yeni jeton ister | Kodda sabit (`PayTrStartForm.FrameAddressRoot`) |
| Dönüş adresleri | `merchant_ok_url` = `merchant_fail_url` = `https://{alan adı}/odeme/donus/{erişim anahtarı}` | Kodda; alan adı `Site:Domain` |
| `test_mode` | Canlı kurulumda (ortam `Production`) `0`, başka her ortamda `1` — kurulum ayarı değildir | Kodda (`PayTrPaymentGateway`; T311'in "canlı kurulum" anlamıyla aynı) |
| `timeout_limit` | Siparişin ödeme süresinin bitişine kalan dakika, yukarı yuvarlanır, en az 1 | Kodda (`PayTrStartForm.TimeoutMinutes`) |
| Kullanıcı bekleme sınırı | 15 sn, yeniden deneme yok (`05 §10.1.1`) | Ortak çalıştırıcı (`ExternalCallRunner`) |
| Anahtarlar | §2 | Sır dosyası `payment_provider_keys` |

## 5. Geri çağrım / webhook kurulumu

- **URL:** `https://{alan adı}/sistem/odeme-bildirimi` (UC-80; mağaza panelindeki tek bildirim adresi — test ve canlı işlemler aynı adrese gelir, `08 §2.1.10`).
- **İmza doğrulama (T402'de bağlandı):** `hash` = HMAC-SHA256(`merchant_oid` + `merchant_salt` + `status` + `total_amount`, `merchant_key`), Base64; gelenle sabit zamanlı karşılaştırılır (`SignatureComparison.Matches`). `hash` yoksa, imzalı dört alandan biri boşsa ya da kart anahtarları tanımlı değilse sonuç imzası tutmayan istektir: 400, gövdesiz yanıt, kayıt yok, güvenlik satırı seyreltilerek (`SignatureFailureSampled`). `payment_amount`, `payment_type`, `currency` ve `test_mode` imzasızdır; yalnız tutarlılık denetimidir.
- **Sınıflama (`08 §2.1.5`):** imza → tekrar → eşlenmeyen sipariş → tanınmayan değer (`status` başka, `payment_type` `card` değil, `currency` `TL` değil, canlıda `test_mode` = `1`) → `failed` (tutar aranmaz) → tutmayan `success` → tutan `success`. Tutar koşulu kuruşta: `payment_amount` = `Order.Total` × 100 ve `total_amount` ≥ aynı değer (taksit farkı geçer). Yanıt kararı (`OK`, 400, 500) ucundur; bağlantı katmanı yalnız sınıfı verir.
- **Eşleme:** `merchant_oid` tiresiz hâliyle hem sağlayıcı işlem kimliğine hem sipariş referansına yazılır; `status` → `Outcome`, `total_amount` (kuruş) → `Amount`; olay türü `Sale`. `failed_reason_code` ve `failed_reason_msg` saklanmaz; ürün genelinde 60 saniyelik pencereyle seyreltilerek kayda yazılır (`PaymentFailedReasonSampled`; referans ve tutar yazılmaz).
- **Yerelde test:** gerçek mağaza gerekmez. `dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj --filter-class "*PaymentNotificationTests"` uygulamanın bildirim ucunu PayTR'nin bağlantı katmanıyla açar ve test anahtarlarıyla imzalanmış form gövdeleri gönderir (`PaymentNotificationTests.PayTr.cs`: yedi sıra, 4. sıranın dört kolu). Elle denemede imza `PayTrSignature.Of(merchant_key, merchant_oid + merchant_salt + status + total_amount)` ile hesaplanır ve form kodlu POST `/sistem/odeme-bildirimi`'ye gönderilir.
- **Tekrar gönderim davranışı:** ürün yalnız kaydı kesinleştikten sonra `OK` der; kaydedemezse 500 verir ve PayTR'nin yeniden göndermesi beklenir (`08 §2.1.5`). Aynı bildirimin tekrarı yazılmaz, yine `OK` alır. PayTR'nin `OK` dönmeyen bildirimi kaç kez ve hangi aralıkla yeniden gönderdiği **gözlenmedi** (DV-1, §6.1 — OWNER_INPUT); gönderilmezse sipariş son sorguyla (T403) çözülür.

## 6. Test etme

Kod tarafı (sahte işleyiciyle, gerçek hesap gerekmez):

```
dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-class "*PayTrPaymentGatewayTests"
dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj --filter-class "*PaymentStartTests"
dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-class "*PayTrNotificationTests"
dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj --filter-class "*PaymentNotificationTests"
dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-class "*PaymentAmountsQueryTests" --filter-class "*FinalPaymentQueryRuleTests"
dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj --filter-class "*PaymentWindowTests"
```

**Beklenen çıktı:** bütün testler geçer; jeton isteği form kodlu tek POST, `paytr_token` bilinen örnekle eşleşir, jeton ne kayıtta ne veritabanında; bildirimin imzası bilinen örnekle eşleşir, yedi sıranın her biri beklenen kaydı ve yanıtı verir; son sorgu form kodlu tek POST, `paytr_token` bilinen örnekle eşleşir, `004` siparişi iptal eder, altı yanıtsız girdi siparişi değiştirmez, beş koşuldan biri tutmayan başarı uygulanmaz ve `FinalPaymentQueryNotApplied` satırı yazılır.

Sağlayıcının test modunda gözlem (gerçek mağaza ve anahtar gerekir — aşağıdaki tablo):

1. Geliştirme ortamının sır dosyasına mağazanın anahtarları yazılır (§2), ortam `Development` (→ `test_mode` = `1`).
2. Bir kart siparişi oluşturulur ve ödeme sayfası açılır (T510 bağlanınca); T510'dan önce jeton isteği `IPaymentGateway.StartAsync` ile tek başına denenir.
3. Her DV satırının denemesi aşağıdaki tabloya tarih ve kanıt yeriyle yazılır; sonuç `08 §4.1` satırına da geri yazılır.

### 6.1 Dış varsayımların test modundaki gözlemi (`08 §4.1`)

| DV | Ne gözlenir | Deneme | Sonuç | Tarih | Kanıt yeri |
|---|---|---|---|---|---|
| DV-11 | Aynı `merchant_oid` ile ikinci jeton isteği ve ikinci ödeme engellenir mi | Aynı sipariş numarasıyla iki jeton isteği; ret gelirse `reason` metni `08 §2.1.8` tablosuna; ikinci jeton verilirse iki ödeme denenir | **Bekliyor — OWNER_INPUT** (gerçek PayTR test mağazası ve anahtarları gerekli). Sonuçlanana kadar kart anahtarları canlıda tanımlanmaz (`08 §4.1` DV-11; ÖK-4) | 2026-11-10 (bekliyor kaydı) | Bu tablo; deneme yapıldığında jeton isteği/yanıt gövdesinin anahtarsız kopyası bu bölüme |
| DV-14 | `user_basket` biçimi (Base64 JSON, iki ondalıklı metin fiyat) kabul edilir, Türkçe karakterli ad imzayı bozmaz | "Sipariş 2026-…" adlı tek kalemle jeton alınır | **Bekliyor — OWNER_INPUT**. Kod tarafı: biçim ve UTF-8 baytları `PayTrPaymentGatewayTests.Amount_IsWholeKurus_AndTheBasketIsOneItemInBase64_WhateverTheCulture_KK4`, imza bilinen örnekle `Token_MatchesTheKnownExample_…_KK3` | 2026-11-10 (bekliyor kaydı) | Bu tablo |
| DV-38 | Fiziksel kalemsiz siparişte firma telefonu (`user_phone`) ve sınıra uymayan e-postanın yerine firma e-postası (`email`) kabul edilir | Yalnız dijital kalemli siparişle ve 100 karakteri aşan e-postayla jeton alınır | **Bekliyor — OWNER_INPUT** (ayrıca PayTR'den yazılı yanıt, sözleşme öncesi). Kod tarafı: `Email_OutsideTheLimit_…_KK6`, `Phone_…_KK6` | 2026-11-10 (bekliyor kaydı) | Bu tablo |
| DV-39 | `user_ip`'te IPv6 adresi kabul edilir | IPv6 adresli istemciyle jeton alınır | **Bekliyor — OWNER_INPUT** (ayrıca PayTR'den yazılı yanıt). Kod tarafı: tam IPv6 gönderilir, `::ffff:` IPv4'e çevrilir, en çok 39 karakter — `ClientAddress_GoesAsItIs_…_KK7` | 2026-11-10 (bekliyor kaydı) | Bu tablo |
| DV-41 | PayTR aynı firmaya ayrı test mağazası ve ayrı anahtar çifti verir mi | Kurulumu yapan PayTR'den yazılı yanıt alır | **Bekliyor — OWNER_INPUT**. Yanıt gelene kadar geliştirme ortamı **anahtarsız** kurulur: kart hattı kapalı, kod tarafı sahte işleyiciyle sınanır (§6). Yanıt "ayrı test mağazası" ise onun anahtarları, "yok" ise referans kurulumun mağazası `test_mode` = `1` ile geliştirme ortamının sır dosyasına yazılır; firmaların anahtarları hiçbir hâlde kurulum dışına çıkmaz | 2026-11-10 (bekliyor kaydı) | Bu tablo; yazılı yanıtın tarihi ve yeri buraya |
| DV-1 | `OK` dönmeyen bildirim kaç kez, hangi aralıkla yeniden gelir | Bildirim adresi 500 dönerken test ödemesi yapılır, gelen tekrarlar sayılır | **Bekliyor — OWNER_INPUT** (D-1038). Kod tarafı: kaydedilemeyen bildirim 500, tekrar yazılmaz ve `OK` alır (`PaymentNotificationTests`) | 2026-11-10 (bekliyor kaydı, T402) | Bu tablo; referans kurulumun işaretli kaydı |
| DV-2 | Bildirim gövdesi form kodlu mu | Gelen gerçek bildirimin `Content-Type`'ı okunur | **Bekliyor — OWNER_INPUT** (D-1038). Kod tarafı: form kodlu gövde (`PaymentNotificationTests.PayTr_*`) | 2026-11-10 (bekliyor kaydı, T402) | Bu tablo |
| DV-4 | `merchant_fail_url` dönüşü GET mi | Başarısız test ödemesinin dönüşünün yöntemi ve alanları okunur | **Bekliyor — OWNER_INPUT** (D-1038). Bildirim katmanının kod payı yok | 2026-11-10 (bekliyor kaydı, T402) | Bu tablo |
| DV-9 | Test kartları var, test modunda bildirim canlıdaki gibi gelir | Test kartıyla ödeme, gelen bildirimin alanları okunur | **Bekliyor — OWNER_INPUT** (D-1038). Kod tarafı: canlıda `test_mode` = `1` işaretli (`PayTr_Row4_TestModeOnTheLiveInstallation_*`) | 2026-11-10 (bekliyor kaydı, T402) | Bu tablo |
| DV-35 | Bildirim gövdesi 64 KB'ın altında | Gerçek test bildiriminin bayt boyu okunur | **Bekliyor — OWNER_INPUT** (D-1038). Kod tarafı: tavan imzadan önce (T203) | 2026-11-10 (bekliyor kaydı, T402) | Bu tablo |
| DV-36 | Tanınmayan bildirim işaretli kayıtta görünür | Kurulumu yapan duyuru kanalını izler; tanınmayan değer sınanır | **Bekliyor — OWNER_INPUT** (D-1038). Kod tarafı: dört kol işaretli kayıtta (`PayTr_Row4_*`) | 2026-11-10 (bekliyor kaydı, T402) | Bu tablo |
| DV-42 | `failed` bildirimi oturumun kesin sonucunda mı, her denemede mi gelir | Aynı çerçevede önce başarısız, sonra başarılı kart; bildirimlerin sayısı ve sırası okunur | **Bekliyor — OWNER_INPUT** (D-1038; test modunda üretilemezse PayTR'den yazılı yanıt). Kod tarafı: `failed` siparişi iptal eder, red kodu seyreltilmiş kayda (`PayTr_Row5_*`) | 2026-11-10 (bekliyor kaydı, T402) | Bu tablo |
| DV-12 | Son sorgu tutarlarının (`payment_amount`, `payment_total`) ondalık ve binlik ayracı, `currency` yazımı (`TL` mi `TRY` mi), tabloda olmayan yanıtlar ve HTTP kodları | Başarılı test ödemesinden sonra aynı `merchant_oid` sorgulanır; yanıt gövdesinin kart alanları çıkarılmış kopyası okunur; bilinmeyen `merchant_oid` ile `004` denenir | **Bekliyor — OWNER_INPUT** (D-1065). Kod tarafı: tek ayraçlı en çok iki ondalık ya da ayraçsız metin okunur, başkası `NoResponse` (`PaymentAmountsQueryTests`, `PaymentWindowTests.PayTr_SuccessWithAnUnrecognizedAmountText_*`); `TL` ve `TRY` kabul (`PayTrPaymentGatewayTests.Query`) | 2026-11-10 (bekliyor kaydı, T403) | Bu tablo; sonuç `08 §2.1.6` ve §4.1 DV-12'ye |

## 7. Sorun giderme

Başlatmanın sonucu kayda yalnız kısa kodla yazılır (`ExternalCallFailed PaymentStart <tür> <kod>`); jeton, anahtar, alan değeri ve sağlayıcının `reason` metni yazılmaz. Ayrıntılı hata tablosu T404'tedir.

| Belirti | Olası sebep | Çözüm |
|---|---|---|
| Kart yöntemi görünmüyor; açılış kaydı kart hattını `closed` yazıyor | Sır dosyası yok ya da üç satırdan biri eksik/boş/tekrar/bilinmeyen ad (§2) | Dosyanın içeriğini §2'deki biçimle düzelt, `app`'i yeniden başlat |
| Kayıtta `keys-missing` | Kart hattı kapalıyken başlatma çağrısı geldi | Yukarıdaki satır |
| Kayıtta `client-address` | İstemci adresi okunamadı ya da 39 karakteri aşıyor | Ters vekilin ilettiği istemci adresi başlığını denetle (`DEPLOY_RUNBOOK §K.4`) |
| Kayıtta `failed` | PayTR jeton isteğini reddetti (alan, imza ya da mağaza durumu) | Mağaza panelinde işlemi ve anahtarları denetle; `reason` metni kayda yazılmaz — test modunda denemeyle okunur |
| Kayıtta `http:<kod>` ya da `body` | PayTR 2xx dışı yanıt verdi ya da gövde beklenen JSON değil | Geçici ise kendiliğinden düzelir; sürerse PayTR destek |
| Kayıtta `TimedOut` | 15 sn içinde yanıt yok | PayTR'nin durumunu denetle; müşteri "ödeme yöntemi kullanılamıyor" görür |
| Kayıtta `ExternalCallFailed PaymentQuery ... err:001` / `err:003` | Son sorguda geçersiz istek, mağaza aktif değil ya da `paytr_token` tutmuyor (anahtarlar) | §2'deki anahtarları denetle; satır kod hatası kuralıyla yinelenir (1, 5, 15, 60, 240 dk), "başarısız" olursa düzeltmeden sonra iş satırı komutuyla yeniden kuyruğa al (`05 §8.5.9`); sipariş bu arada Bekliyor'da kalır |
| Kayıtta `ExternalCallFailed PaymentQuery ... err:002` | Geçersiz `merchant_oid` (kod hatası) | Aynı; sürerse hata kaydı açılır |
| Kayıtta `ExternalCallFailed PaymentQuery ... amount-text` | Son sorgunun tutar metni DV-12'nin kuralına uymadı | Ödemeyi mağaza panelinden oku, siparişi panelden elle kapat (`05 §8.5.12`); gövdenin biçimini D-1065'e yaz |
| Kayıtta `FinalPaymentQueryNotApplied <reason> <orderId>` | Son sorguda başarı var ama `amount`, `payment-type`, `currency` ya da `test-mode` koşulu tutmuyor | Ödemeyi mağaza panelinden oku; tutar tutmuyorsa komut uygulamaz, firma siparişi iptal eder (`08 §2.1.6`; K-1283) |
| Son sorgu kayıtta `TimedOut`, `connection:*`, `http:<kod>` ya da `body` | PayTR'ye 30 sn içinde ulaşılamadı ya da yanıt tabloda yok | Kendiliğinden beş dakikada bir yeniden sorulur; sipariş iptal edilmez, "ödeme sonucu alınamadı" işaretiyle bekler |

## 8. Kesinti davranışı

PayTR'ye ulaşılamazsa (zaman aşımı, bağlantı hatası, 2xx dışı yanıt, tanınmayan gövde) başlatma bağlantı hatasıdır: yeniden denenmez, UC-29 "ödeme yöntemi kullanılamıyor" sonucunu verir ve satış havaleyle sürebilir (`05 §10.1.1`; `08 §2.1.8`, §3.3). İkinci jeton reddi (DV-11) bağlantı hatası sayılmaz; ayırt eden `reason` metni test modunda gözlenince T404 tablosuna yazılır.

Son sorguda kesinti siparişi iptal ettirmez: zaman aşımı, bağlantı hatası ve tabloda olmayan yanıt beş dakikada bir yinelenir; ödeme süresi işlemeye devam eder, yalnız iptal bekler (`08 §2.1.6`, §3.3; T403).

## 9. Prod farkları

- Ayrı test adresi yoktur; canlı ve test aynı adrese gider, farkı istekteki `test_mode`'dur (canlı kurulumda `0`).
- Ayrı test anahtarı yoktur (DV-41 yanıtı bekleniyor); bir kurulumun `merchant_key` ve `merchant_salt`'ı yalnız o kurulumun sunucusunda durur (`05` 6.x.1).
- Kart anahtarları canlıda DV-11 sonuçlanana kadar tanımlanmaz (`08 §4.1` DV-11; ÖK-4).
