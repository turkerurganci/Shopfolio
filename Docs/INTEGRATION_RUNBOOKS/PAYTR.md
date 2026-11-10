# PayTR — Entegrasyon Runbook

**Son güncelleme:** 2026-11-10 | **İlgili task:** T401 | **Spec:** [`../08_INTEGRATION_SPEC.md`](../08_INTEGRATION_SPEC.md) §2.1 (2.1.2 anahtarlar, 2.1.4 başlatma, 2.1.10 test), §4.1 (DV tablosu)

> **Kural:** Bir dış servise **ilk kez bağlanan task**, aynı PR'da bu runbook'u da yazar.
> "Sonra yazarız" denen kurulum bilgisi, ikinci kez lazım olduğunda kaybolmuş olur.

> **Bu sürümün kapsamı (T401):** anahtarlar, ödemenin başlatılması (jeton isteği), dönüş adresi ve çerçeve. Bildirim imzası ve alan eşlemesi T402'de, son sorgu T403'te, hata tablosunun tamamı T404'te, kart iadesi T405'te bu dosyaya eklenir. Ödeme sayfası (çerçeveyi taşıyan sayfa, UC-29'un çağıranı) T510'dadır.

---

## 1. Hesap açma

1. Firma (referans kurulumda geliştiricinin kendi tüzel kişiliği — `10 §4.1` ÖK-12) PayTR ile sözleşme yapar ve mağaza hesabı açar; komisyon teklifi alınır, PayTR'nin ödeme kuruluşu lisansı TCMB'nin resmî kaydında doğrulanır (`DEPLOY_RUNBOOK` kurulum notu K-1254).
2. Mağaza canlıya alınmadan önce **test modundadır**; bu hâlde bütün işlemler testtir ve canlıya geçmeden önce test ödemesi beklenir (`08 §2.1.10`).
3. Mağaza panelinde bildirim adresi tanımlanır (§5) — T402 bağlanana kadar bildirim ucu "alınmadı" (500) yanıtlar; canlı satış T402 olmadan açılmaz.

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
- **İmza doğrulama:** `hash` = HMAC-SHA256(`merchant_oid` + `merchant_salt` + `status` + `total_amount`, `merchant_key`), Base64 — **T402'de bağlanır**; o zamana kadar uç bildirimi kabul etmez (500, `07 §4.30.1`).
- **Yerelde test:** T402'de yazılır.
- **Tekrar gönderim davranışı:** T402'de yazılır (`08 §2.1.5`).

## 6. Test etme

Kod tarafı (sahte işleyiciyle, gerçek hesap gerekmez):

```
dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-class "*PayTrPaymentGatewayTests"
dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj --filter-class "*PaymentStartTests"
```

**Beklenen çıktı:** bütün testler geçer; jeton isteği form kodlu tek POST, `paytr_token` bilinen örnekle eşleşir, jeton ne kayıtta ne veritabanında.

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

## 8. Kesinti davranışı

PayTR'ye ulaşılamazsa (zaman aşımı, bağlantı hatası, 2xx dışı yanıt, tanınmayan gövde) başlatma bağlantı hatasıdır: yeniden denenmez, UC-29 "ödeme yöntemi kullanılamıyor" sonucunu verir ve satış havaleyle sürebilir (`05 §10.1.1`; `08 §2.1.8`, §3.3). İkinci jeton reddi (DV-11) bağlantı hatası sayılmaz; ayırt eden `reason` metni test modunda gözlenince T404 tablosuna yazılır.

## 9. Prod farkları

- Ayrı test adresi yoktur; canlı ve test aynı adrese gider, farkı istekteki `test_mode`'dur (canlı kurulumda `0`).
- Ayrı test anahtarı yoktur (DV-41 yanıtı bekleniyor); bir kurulumun `merchant_key` ve `merchant_salt`'ı yalnız o kurulumun sunucusunda durur (`05` 6.x.1).
- Kart anahtarları canlıda DV-11 sonuçlanana kadar tanımlanmaz (`08 §4.1` DV-11; ÖK-4).
