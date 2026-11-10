# Amazon SES (e-posta gönderimi) — Entegrasyon Runbook

**Son güncelleme:** 2026-11-10 | **İlgili task:** T407, T408 (geri dönen adreslerin öğrenilmesi: §10) | **Spec:** [`../08_INTEGRATION_SPEC.md`](../08_INTEGRATION_SPEC.md) §2.2, §2.2.2, §2.2.3, §4.1

> **Kural:** Bir dış servise ilk kez bağlanan task, aynı PR'da bu runbook'u da yazar. Bu runbook SES'e ilk bağlanan T407'nin işidir; geri dönen adres sorgusu (T408) kendi bölümünü (§10) buraya ekledi.

Ürün SES'in API v2'sini kullanır (`AWSSDK.SimpleEmailV2`). Üç çağrı vardır: ileti gönderme (SendEmail), tek adresin askıda olup olmadığını sorma (GetSuppressedDestination; her gönderimden hemen önce) ve askıdaki adresleri listeleme (ListSuppressedDestinations; §10). **Gelen istek yoktur**: SES'ten ürüne web kancası, SNS aboneliği ya da olay yayını kurulmaz.

---

## 1. Hesap açma

Hesap firmanındır (firmanın kendi AWS hesabı, kredi kartıyla). Sırayla:

1. **AWS hesabı aç** (aws.amazon.com → "Create an AWS Account"). Hesabın iletişim e-postası firmanın okuduğu adrestir: inceleme ve durdurma bildirimleri oraya düşer. Kök kullanıcıyı çok adımlı doğrulamayla koru; günlük iş için kullanma.
2. **Bölgeyi seç:** sağ üstteki bölge menüsünden **Europe (Frankfurt) `eu-central-1`** (kurulum önerisi). Kimlikler, kota ve askıdaki adres listesi bölge başınadır; uygulama ayarındaki bölge (`Email:Region`) bu bölgeyle aynı olmalıdır (DV-17).
3. **Alan adı kimliği oluştur:** SES konsolu → "Configuration: Identities" → "Create identity" → "Domain" → gönderen adresinin alan adı. "Easy DKIM" (RSA 2048) seçili kalır; SES üç CNAME kaydı üretir, bunları alan adının DNS'ine gir. Kimlik durumu "Verified" olana kadar bekle. Gönderen adresi bu alan adındadır (ör. `info@firma.example`).
4. **DNS (kurulum önerisi):** isteğe bağlı özel MAIL FROM alt alan adı (MX ve SPF TXT kaydı) ve `_dmarc` TXT kaydı (`rua` rapor adresiyle, geçiş hâli olarak `p=none`). Raporlar alan adının bütün gönderenlerinin hizalandığını gösterince politika en az `p=quarantine`'e sıkılaştırılır; sıkılaştırmanın günü kurulumun kaydına yazılır (`DEPLOY_RUNBOOK`, `08 §2.2` DNS kayıtları).
5. **Üretim erişimi iste:** yeni hesap sandbox'tadır (yalnız doğrulanmış alıcıya, günde 200 ileti, saniyede 1). Konsol → "Account dashboard" → "Request production access": mail türü **Transactional**, web sitesi adresi ve kullanım açıklaması. İlk yanıt 96 saat içindedir. Onaydan sonra `GetAccount` → `SendQuota` ile günlük kotanın **350'nin üstünde** olduğunu ve `MaxSendRate` değerini oku; ikisini kurulumun kaydına yaz (DV-18). Kota yetmezse kota artışı istenir (en az 1.000).
6. **Askıdaki adres listesi (hesap düzeyi):** konsol → "Configuration: Suppression list" → "Edit account-level settings" → yalnız **Bounces** açık, Complaints kapalı. Ürün listeye adres eklemez ve listeden adres silmez; liste adresi geldiği yazılışla tutar (§10; `08 §2.2.3`).
7. **Veri işleme eki ve yurt dışı aktarım:** AWS veri işleme ekinin hesapta kabulü ve Kurul standart sözleşmesinin imzalanıp beş iş günü içinde Kurum'a bildirilmesi kurulum denetimidir (`10 §4.1` ÖK-2; DV-24).
8. **İtibar alarmı:** CloudWatch'ta `Reputation.BounceRate` (≥ 0,05) ve `Reputation.ComplaintRate` (≥ 0,001) için alarm kur; bildirim firmanın izlediği adrese gider.

## 2. Kimlik bilgisi alma

IAM kullanıcısı **yalnız bu kuruluma aittir** ve konsol erişimi yoktur (yalnız programatik erişim).

1. IAM konsolu → "Users" → "Create user" (ad: `shopfolio-ses-{firma}`); izin olarak aşağıdaki **satır içi politikayı** (§4) ekle; başka politika ekleme.
2. Kullanıcı → "Security credentials" → "Create access key" → kullanım türü "Application running outside AWS". Erişim anahtarı kimliği ve gizli anahtar yalnız bu ekranda görünür.
3. İki değeri **`email_credential` sır dosyasına iki satır** olarak yaz:

```
<satır 1: erişim anahtarı kimliği>
<satır 2: gizli anahtar>
```

Başka içerik, boşluk ya da ek satır yoktur. Biçim yanlışsa uygulama **açılmaz** ve açılış hatası ayarın adını söyler, değeri söylemez. Geliştirmede (`EmailCapture:Host` dolu) dosya hiç okunmaz; her boş olmayan metin açar ve gerçek kimlik geliştirme makinesine geçmez.

| Bilgi | Nereden alınır | Nerede saklanır | Rotasyon |
|---|---|---|---|
| Erişim anahtarı kimliği (1. satır) | IAM → kullanıcı → Security credentials | `email_credential` sır dosyası (`DEPLOY_RUNBOOK §B` satır 13, §J.2) | Olay üzerine; uygulama yeniden başlar, kuyruktaki e-postalar gönderim kaydında bekler |
| Gizli anahtar (2. satır) | Aynı ekran (yalnız bir kez görünür) | Aynı dosya | Aynı |
| Bölge (`Email:Region`) | Hesapta seçilen bölge (§1 adım 2) | Ayar dosyası / `Email__Region` | Bölge değişirse kimlik, kota ve liste yeniden kurulur |
| Gönderen adresi (`Email:SenderAddress`) | Doğrulanmış alan adındaki adres | Ayar dosyası / `Email__SenderAddress` | Adres değişirse **politika da değişir** (§4) |

Döndürme: yeni anahtar üret, dosyayı güncelle, uygulamayı yeniden başlat, eski anahtarı devre dışı bırak, sonra sil. Döndürme sırasında reddedilen deneme kanal hatasıdır ve P-38 aralıklarıyla yeniden denenir.

## 3. Plan / kota

| Plan | Limit | Ücret | MVP'ye yeter mi |
|---|---|---|---|
| Sandbox (yeni hesap) | Yalnız doğrulanmış alıcı; 24 saatte 200 ileti; saniyede 1 | Kullanım kadar | Hayır: tepe gün 350 e-posta |
| Üretim erişimi | Günlük kota hesapta okunur (`GetAccount` → `SendQuota`); talepte en az 1.000 istenir | 1.000 e-posta başına 0,10 $ (à la carte), asgari ücret yok | Evet, kota 350'nin üstündeyse (DV-18) |

> **Dış varsayım kanıtı:** fiyat ve sandbox sınırları aws.amazon.com/ses/pricing ve AWS SES belgesi, 2026-10-29 (`08 §2.2`, §2.2.1). Üretim kotasının sayısı belgede yazılı değildir: hesapta okunur (DV-18).

## 4. Konfigürasyon

### IAM politikası (kurulum adımı)

Üç izin vardır; gönderme kimlik kaynağına ve gönderen adresine bağlıdır, listeleme ve tek adres sorgusu hesap düzeyidir ve **koşulsuz, ayrı ifadede** durur (gönderme koşulu bu isteklerde bulunmadığından aynı ifadede onları reddettirir). Silme ve ekleme izni (`DeleteSuppressedDestination`, `PutSuppressedDestination`) yoktur. Yer tutucular: `{bölge}` ör. `eu-central-1`, `{hesap}` 12 haneli AWS hesap numarası, `{alan adı}` doğrulanmış alan adı, `{gönderen adresi}` ayardaki gönderen adresi.

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "SendFromTheInstallationSenderOnly",
      "Effect": "Allow",
      "Action": "ses:SendEmail",
      "Resource": "arn:aws:ses:{bölge}:{hesap}:identity/{alan adı}",
      "Condition": {
        "StringEquals": {
          "ses:FromAddress": "{gönderen adresi}"
        }
      }
    },
    {
      "Sid": "ReadTheAccountSuppressionList",
      "Effect": "Allow",
      "Action": [
        "ses:ListSuppressedDestinations",
        "ses:GetSuppressedDestination"
      ],
      "Resource": "*"
    }
  ]
}
```

Gönderen adresi değişirse politikadaki `ses:FromAddress` değeri de değişir.

### Uygulama ayarları

| Ayar | Değer | Nerede tanımlı |
|---|---|---|
| `Email:Region` | `eu-central-1` | `DEPLOY_RUNBOOK §A` satır 3 |
| `Email:SenderAddress` | Doğrulanmış alan adındaki adres | `DEPLOY_RUNBOOK §A` satır 2 |
| `Email:Credential` | İki satırlı sır dosyası `email_credential` | `DEPLOY_RUNBOOK §A` satır 4, §B satır 13, §J.2 |
| `EmailCapture:Host` | Yalnız geliştirmede dolu; doluysa SES kullanılmaz | `compose.dev.yaml` |

İstemci tekildir; yeniden deneme kapalıdır (`MaxErrorRetry = 0`), süre 30 saniyedir, kimlik açıktır ve ortam kimlik zinciri (ortam değişkenleri, profil, örnek rolü) ile ortamdaki uç adresi kullanılmaz (`09 §11.2`).

## 5. Geri çağrım / webhook kurulumu

**Yoktur.** SES'ten ürüne bildirim, web kancası, SNS aboneliği, yapılandırma kümesi ya da olay yayını kurulmaz; istekte yapılandırma kümesi adı gönderilmez ve açılma ya da tıklama izlemesi yoktur (DV-19). Kalıcı geri dönme sorguyla öğrenilir (§10; her on beş dakikada hesap listesi). `07`'ye uç, güvenlik duvarına gelen kural eklenmez.

## 6. Test etme

1. **Açılış denetimi:** üç ayardan biri eksikse ya da kimlik dosyası iki satırlı değilse uygulama açılmaz; hata ayarın adını söyler.
2. **Deneme iletisi (kurulum kabulü):** uygulama açıkken gönderen adresinden doğrulanmış bir alıcıya (sandbox'ta alıcı da doğrulanmalıdır) panelden ya da bir B-türü bildirimle ileti gönder; gönderim kaydında satır `Sent` olur.
3. **Başlıklar:** iletinin ham başlıklarında SPF, DKIM ve DMARC sonuçlarını oku (DV-20); açılma izleme bağlantısının olmadığını ve gövdenin hem HTML hem düz metin olduğunu doğrula (DV-19).
4. **Türkçe ve kaçışlı ad:** marka adını `Örnek Mağaza` ve `A"B\C` yaparak iki deneme gönder; ad iki durumda da kabul edilmeli (DV-21).
5. **Politika:** başka bir gönderen adresiyle yapılan gönderimin reddedilmesi beklenir (DV-16); listeleme ve tek adres sorgusu çalışmalıdır.
6. **Askıdaki adres (DV-22):** üretim erişimli hesapta konsoldan listeye bir deneme adresi ekle; o adrese gönderim `RecipientError` olur ve SendEmail çağrısı yapılmaz. Sandbox'ta listeye ekleme kapalıdır ve posta kutusu simülatörünün geri dönme adresi listeye girmez.

**Beklenen çıktı:** `Sent` satırı, `MessageId`'nin hiçbir yerde saklanmadığı, kayıtta adres ve anahtar bulunmadığı.

Otomatik testler gerçek hesap gerektirmez: gerçek SDK istemcisi sahte HTTP üstünde koşar (`tests/Shopfolio.UnitTests/Integrations/Ses*`).

## 7. Sorun giderme

| Belirti | Olası sebep | Çözüm |
|---|---|---|
| Uygulama açılmıyor, hata `Email:Credential` diyor | Kimlik dosyası iki satırlı değil | Dosyayı §2'deki biçimle yeniden yaz |
| Kanal hatası, `MessageRejected` ve "not verified" | Gönderen adresi ya da alan adı bu bölgede doğrulanmamış; sandbox'ta alıcı doğrulanmamış | Alan adı kimliğini doğrula, bölgeyi kontrol et, alıcıyı doğrula ya da üretim erişimi al |
| Kanal hatası, `AccessDeniedException` | Politika gönderen adresiyle uyuşmuyor ya da kaynak ARN'i yanlış | §4 politikasını gönderen adresi ve kimlik ARN'iyle karşılaştır |
| Kanal hatası, `SendingPausedException`, `AccountSuspendedException` | AWS hesabı durduruldu ya da incelemede | Hesabın iletişim e-postasındaki AWS bildirimine yanıt ver |
| Kanal hatası, `ThrottlingException` | Günlük kota ya da saniyelik hız aşıldı | `GetAccount` ile kotayı oku; kota artışı iste |
| Kanal hatası, imza hatası | Anahtar yanlış, devre dışı ya da saat sapması | Anahtarı yeniden üret ve §2 dosyasını güncelle |
| Alıcı hatası, çağrı yok | Alıcı hesap listesinde `BOUNCE` ile askıda | Adresin çalıştığını kullanıcıyla teyit et, listeden konsolda elle çıkar (ürün silmez) |
| Gönderim zaman aşımına düşüyor | Ağ ya da servis yavaş; süre 30 sn | Servis durumunu ve çıkış ağını kontrol et; satır P-38 aralıklarıyla yeniden denenir |

## 8. Kesinti davranışı

E-posta gönderim kaydında bekler; sipariş, ödeme ve durum geçişleri etkilenmez. Satır P-38 aralıklarıyla (ilk denemeden 5 dakika, 30 dakika, 2 saat) yeniden denenir; yeniden denemeler tükenince satır "ulaşmadı" olur ve müşteri bildirimi siparişin ayrıntısından tek tek yeniden gönderilir. Art arda en az beş kanal hatası ve ilk hatadan on beş dakika sonra panelin ana sayfasında kanal uyarısı açılır. Ön sorgunun kendi hatası kanal hâline yazılmaz ve gönderimi durdurmaz. Ayrıntı: `08 §2.2` "Servis çökerse".

## 9. Prod farkları

| Konu | Sandbox / geliştirme | Üretim |
|---|---|---|
| Alıcı | Yalnız doğrulanmış alıcı | Herhangi bir alıcı (üretim erişimiyle) |
| Limit | 24 saatte 200 ileti, saniyede 1 | Hesapta okunan kota ve hız (DV-18) |
| Askıdaki liste | Listeye ekleme ve toplu işlem kapalı; simülatör adresi listeye girmez | Konsoldan eklenen adresle DV-22 gözlenir |
| Geliştirme makinesi | `EmailCapture:Host` doluysa ileti yakalama aracına gider, kimlik dosyası okunmaz | SES kullanılır |
| Uç adresi | `https://email.{bölge}.amazonaws.com/v2/email/...` (bölge ayardan) | Aynı |

## 10. Geri dönen adresler

Kalıcı geri dönme sorguyla öğrenilir (T408; `08 §2.2.3`). Her on beş dakikada bir iş, hesap düzeyindeki askıdaki adres listesinden son 24 saatte eklenen `BOUNCE` adreslerini okur (`ListSuppressedDestinations`; `StartDate`, `Reason` = `BOUNCE`, sayfa boyu ve `NextToken`); `COMPLAINT` nedeni işlenmez. Son 24 saatte `Sent` olan ve alıcısı o adres olan (harf ayrımsız) bütün satırlar `Undelivered` ve `RecipientError` olur; satırın `SentAt`'i kalır, kanal hâli kaydına dokunulmaz.

1. **Kurulum şartı:** hesap düzeyi liste yalnız **Bounces** açıktır (§1 adım 6). Listeleme izni IAM politikasındadır (§4); başka izin yoktur.
2. **Silme:** ürün listeden adres silmez. Koda silme ya da liste yazma çağrısı konmaz; IAM kullanıcısı silme izni almaz. Listeden çıkarma firmanın elle yaptığı iştir: e-postası gelmediğini başka kanaldan bildiren kullanıcının adresinin artık çalıştığını kullanıcıyla teyit et, sonra konsol → "Configuration: Suppression list" → adresi seç → "Remove". Liste adresi geldiği yazılışla tutar; konsolda adres aynı yazılışla aranır.
3. **Hata davranışı:** sorgu başarısızsa (429, kimlik ya da izin hatası, zaman aşımı) tur boş geçer, satır ve kanal hâli değişmez, hata uygulama kaydına yazılır; sonraki tur yine 24 saat öncesinden sorar. Süresiz süren bir sorgu hatasının uyarısı yoktur — kalan risktir (`08 §2.2.3`); uygulama kaydında sorgu hatalarını ara.
4. **Pencere sınırı:** gönderimden 24 saatten sonra listeye giren geri dönme hiçbir satırla eşleşmez; o ileti "gönderildi" kalır (kalan risk).
5. **Gözlem (DV-22):** üretim erişimli hesapta konsoldan listeye bir deneme adresi ekle ve aynı adrese giden `Sent` satırın bir sonraki turda `Undelivered` olduğunu panelden gör. Sandbox'ta listeye ekleme kapalıdır; bu gözlem gerçek hesapla yapılana kadar açıktır (OWNER_INPUT).
