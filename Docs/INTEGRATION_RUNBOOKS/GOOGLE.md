# Google ile giriş, kayıt ve yeniden doğrulama — Entegrasyon Runbook

**Son güncelleme:** 2026-11-10 | **İlgili task:** T409 | **Spec:** [`../08_INTEGRATION_SPEC.md`](../08_INTEGRATION_SPEC.md) §2.3, §4.1

> **Kural:** Bir dış servise ilk kez bağlanan task, aynı PR'da bu runbook'u da yazar. Bu runbook Google'a ilk bağlanan T409'un işidir.

Ürün Google'ın OpenID Connect yetki kodu akışını kullanır. Sunucunun Google'a yaptığı tek çağrı kod değişimidir (`POST https://oauth2.googleapis.com/token`); kimlik belirteci keşif belgesinden ve anahtar kümesinden alınan anahtarla doğrulanır. `userinfo` çağrılmaz. **Gelen webhook yoktur**: Google'ın ürüne tek dönüşü tarayıcıdan gelen `GET /giris/google`'dur. Bu runbook'u yazarken gerçek bir Google uygulaması yoktu (OWNER_INPUT); aşağıdaki "Gözlendi" notları çevrimdışı alınan kanıttır, "Gözlenmedi" notları sahibin kurulumuna bekler.

---

## 1. Hesap açma

Google uygulamasının sahibi firmadır (ÖK-6); kurulumu yapan firmanın Google hesabıyla çalışır.

1. `console.cloud.google.com` → proje seç ya da "New project" ile aç (ad: firma adı).
2. "APIs & Services" → "OAuth consent screen" (yeni arayüzde "Google Auth Platform") → kullanıcı türü "External"; uygulama adı, destek e-postası ve geliştirici e-postası doldurulur.
3. Kapsamlar: yalnız `openid`, `email`, `profile` (hassas ya da kısıtlı kapsam eklenmez; bu yüzden Google'ın kapsam incelemesi gerekmez).
4. "Credentials" → "Create credentials" → "OAuth client ID" → uygulama türü "Web application". **Authorized redirect URI:** tam olarak `https://{alan adı}/giris/google` (başka dönüş adresi tanımlanmaz). "Authorized JavaScript origins" boş kalır.
5. **Yayın durumu:** "Publishing status" → "In production" ("Push to production"). Kapsamlar yalnız `openid email profile` olduğundan test kullanıcısı tanımlamak gerekmez (DV-37; gerçek gözlem bekliyor).
6. **Yeniden doğrulama için** (`auth_time`, DV-25): uygulamanın doğrulanmış ("Verified") olması ve Google Auth Platform → Settings → Advanced Settings → "Session age claims" seçeneğinin açık olması gerekir. Doğrulama süreci için Google'ın marka doğrulaması istenir (uygulama alan adı, gizlilik politikası ve hizmet şartları bağlantıları). Bunlar sağlanmazsa `auth_time` gelmez ve Google ile yeniden doğrulama tutmaz; üç işlem şifreyle yapılır (`05 §6.1.5`).

## 2. Kimlik bilgisi alma

| Bilgi | Nereden alınır | Nerede saklanır | Rotasyon |
|---|---|---|---|
| İstemci kimliği (`Google:ClientId`) | Credentials → OAuth client → "Client ID" | Ayar dosyası / `Google__ClientId` (sır değildir) | Olay üzerine; uygulama yeniden başlar |
| İstemci sırrı | Credentials → OAuth client → "Client secret" | `google_client_secret` sır dosyası: dosyanın bütün içeriği (`DEPLOY_RUNBOOK §B` satır 14) | Olay üzerine: konsolda ikinci sırrı ekle, dosyayı güncelle, `app`'i yeniden başlat, eski sırrı sil |

Sır dosyasına istemci sırrı dışında içerik, boşluk ya da ek satır yazılmaz. İkisinden biri eksikse uygulama açılır ama "Google ile giriş" hattı kapalıdır ve açılış kaydı eksik anahtarı adıyla yazar (değeri yazmaz).

## 3. Plan / kota

| Plan | Limit | Ücret | MVP'ye yeter mi |
|---|---|---|---|
| Google Cloud OAuth (kimlik doğrulama) | Giriş başına bir kod değişimi çağrısı; Google'ın uygulamaya uyguladığı kota belgede bilgim tarihlidir (DV-30, T410) | Ücretsiz | Evet — hacim küçüktür; keşif belgesi ve anahtar kümesi önbelleğe alınır |

> **Dış varsayım kanıtı:** keşif belgesi `https://accounts.google.com/.well-known/openid-configuration` 2026-11-10'da (T409) `curl` ile okundu: `issuer` = `https://accounts.google.com`; `authorization_endpoint` ve `token_endpoint` koddaki sabitlerle aynı; `jwks_uri` = `https://www.googleapis.com/oauth2/v3/certs`; `code_challenge_methods_supported` = `plain`, `S256`; `claims_supported` içinde `auth_time` yok; `id_token_signing_alg_values_supported` = `RS256`.

## 4. Konfigürasyon

| Ayar | Değer | Nerede tanımlı |
|---|---|---|
| `Google:ClientId` | Konsoldaki istemci kimliği | Ayar dosyası (`Google__ClientId`); `DEPLOY_RUNBOOK §C` satır 2 |
| `Google:ClientSecret` | Konsoldaki istemci sırrı | `google_client_secret` sır dosyası (`DEPLOY_RUNBOOK §B` satır 14) |
| Dönüş adresi | `https://{Site:Domain}/giris/google` | Kodda sabit biçim (`GoogleEndpoints.ReturnAddress`); konsolda aynen tanımlanır |
| Yetkilendirme adresi | `https://accounts.google.com/o/oauth2/v2/auth` | Kodda sabit (`GoogleEndpoints`) |
| Kod değişimi adresi | `https://oauth2.googleapis.com/token` | Kodda sabit |
| Keşif belgesi | `https://accounts.google.com/.well-known/openid-configuration` | Kodda sabit; yalnız `issuer` ve `jwks_uri` okunur |
| İstek kapsamı | `openid email profile` | Kodda sabit |
| İmza algoritması | `RS256` yalnız | Kodda sabit |
| `exp` toleransı | 60 sn (`auth_time`'ın toleransı yok) | Kodda sabit (09 §3.5 tablosu) |
| Çağrı sınırı | 15 sn (kod değişimi, keşif ve anahtar okuması ayrı ayrı) | Çatının bağlayıcı sınırı + `GoogleDocumentRetriever` |
| `max_age` | İstekte yok (DV-26 gözlenmedi) | `GoogleEndpoints.SendsMaxAge` kapalı |

## 5. Geri çağrım / webhook kurulumu

- **URL:** `GET https://{alan adı}/giris/google` — Google'ın yetki kodu dönüşü; `code` ve `state` taşır (hata dönüşünde `error`). Başka gelen istek yoktur.
- **İmza doğrulama:** dönüşün kendisinin imzası yoktur; doğruluğunu `state` (akış çerezine bağlı), PKCE doğrulayıcısı ve kimlik belirtecinin RS256 imzası kurar. İmza doğrulaması atlanamaz.
- **Yerelde test:** Google uygulamasına yerel dönüş adresi (`https://localhost:{port}/giris/google`) ayrıca tanımlanır; tünel aracı gerekmez, çünkü dönüşü tarayıcı getirir.
- **Tekrar gönderim davranışı:** yoktur. Yetki kodu tek kullanımlıktır; başarısız değişimin kodu yeniden gönderilmez, kullanıcı akışı yeniden başlatır.

## 6. Test etme

Otomatik sınama (Google'a çıkmaz; sahte uçla):

```
dotnet test --project tests/Shopfolio.UnitTests --filter-class "*Google*"
```

**Beklenen çıktı:** sıfır hata. Kapsam: kod değişimi gövdesi, kimlik belirteci durumları (geçerli, imza, süre, hedef, yayıncı, algoritma, anahtar kimliği), bilinmeyen `kid` yenilemesi, ayarsız çağrı yok, `sub`'ın sonuçta olmayışı, sonuç sınıfları ve DV satırlarının tarihi.

Gerçek uygulamayla elle sınama (OWNER_INPUT, DV-25/26/27/28/31/37/43):

1. Kurulumda `Google:ClientId` ve sır dosyası dolu iken uygulamayı aç; giriş sayfasında "Google ile giriş" düğmesi görünür.
2. Düğmeye bas: tarayıcı Google'ın hesap seçimine gider (DV-28: zincirde başka alan adı olmamalı); hesapla dön; oturum açılır.
3. Yeniden doğrulama: açık oturumda geri alınamaz bir işlemde "Google ile doğrula" (DV-25: `auth_time` gelmeli; DV-26: açık Google oturumunda `max_age` zorlaması).
4. Kayıt altındaki sonuç kodunu (`google-error:…`, `token:…`) ve çağrı sayısını (bir) kontrol et.

## 7. Sorun giderme

| Belirti | Olası sebep | Çözüm |
|---|---|---|
| "Google ile giriş" düğmesi yok | `Google:ClientId` ya da `google_client_secret` boş; hat kapalı | Açılış kaydındaki eksik anahtar adına bak, değeri doldur, `app`'i yeniden başlat |
| Google'da `redirect_uri_mismatch` | Konsoldaki dönüş adresi `https://{alan adı}/giris/google` ile birebir değil | Konsolda adresi (şema, alan adı, yol) düzelt |
| Kayıtta `exchange:no-id-token` ya da `google-error:invalid_grant` | Kod kullanılmış/süresi dolmuş ya da sır yanlış | Akışı yeniden başlat; sırrı kontrol et |
| Kayıtta `token:signature` / `token:key-not-found` | Anahtar kümesi eski ya da yanıt başka bir yayıncıdan; bilinmeyen `kid`'de anahtar kümesi aynı çağrıda yeniden çekilir, yeni kümede de yoksa imza tutmaz | Keşif belgesi ve `jwks_uri` adresine ulaşımı denetle; ulaşım varsa belirteç Google'ın yayımladığı bir anahtarla imzalı değildir |
| Kayıtta `token:issuer` | Kimlik belirtecinin `iss`'i keşif belgesindeki `issuer` biçiminde değil | `iss` biçimini yaz ve DV-31 / backlog kalemine bildir |
| Kayıtta `token:audience` | İstemci kimliği başka uygulamaya ait | `Google:ClientId`'yi konsoldakiyle eşitle |
| Yeniden doğrulama hep `SocialReauthenticationFailed` | `auth_time` gelmiyor: uygulama yayında/doğrulanmış değil ya da "Session age claims" kapalı | §1 adım 5–6 |
| `Unavailable` (kayıtta `timeout` ya da `configuration:unavailable`) | Google ya da keşif/anahtar adresi erişilemiyor | Beklenen davranıştır (§8); ağ çıkışını denetle |

## 8. Kesinti davranışı

Google'a ulaşılamazsa (bağlanamama, 15 sn aşımı, Google hata yanıtı, keşif/anahtar okunamaması) sonuç `Unavailable`'dır: giriş ve kayıtta `SocialLoginFailed`, yeniden doğrulamada `SocialReauthenticationUnavailable`. Kendiliğinden yeniden deneme yoktur. **Şifreyle giriş, sipariş ve panel etkilenmez**; Google'a bağlı süre yoktur. Şifresi olmayan Google hesabı "şifremi unuttum" ile şifre belirler. Vazgeçme ve doğrulama hataları `Failed`'dır ve çağrı sayısı bir kalır (`08 §2.3`; `05 §10.1.5`).

## 9. Prod farkları

- Google'ın ayrı bir deneme ortamı yoktur; geliştirme ve üretim aynı uçları kullanır, fark yalnız uygulamanın yayın durumu ve dönüş adresidir.
- "Test" durumunda `auth_time` gelmez (yeniden doğrulama tutmaz); girişte test kullanıcısı gerekmez (DV-37, gözlem bekliyor).
- Yayına alma ve marka doğrulaması Google'ın onayını gerektirir ve firmanın işidir (ÖK-6); süresi Google'a bağlıdır.
- Üretimde dönüş adresi `https://{üretim alan adı}/giris/google` konsola ayrıca eklenir.
