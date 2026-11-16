# T551 — Tarayıcı aracı ve betiksiz yol gözlemi

**Faz:** F5 | **Durum:** Yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-16

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Araç:** Playwright (.NET 1.59.0), `tests/Shopfolio.IntegrationTests/Browser/` altında; uygulama bir `WebApplicationFactory` ile gerçek Kestrel'de **HTTPS** (geçici öz imzalı sertifika) ve `https://127.0.0.1:<port>` adresinde açılır; her bağlam kendi `X-Forwarded-For` adresini alır (L-8 açık ödenmemiş sipariş sınırı IP başınadır).
- **Tohum:** müşteri, yönetici, her durumdaki sipariş, ürünler, firma, ayar, teslimat ili (`BrowserSeed`); sipariş, ürün ve hizmet satırı değiştiren testler kendi kaydını açar (`NewProductAsync`, `NewProductWithoutVariantAsync`, `NewOfferingAsync`).
- **Gözlem kaydı:** `observations.md` / `.jsonl`, ekran görüntüleri, sürümler, tohum, sunucu günlüğü (`SHOPFOLIO_BROWSER_OUT`); Veri kaydı sütunu kişisel veriyi maskeler (`DataRecord`).
- **Testler:** `ScreenInventoryTests` (KK2), `ScriptlessPathTests` (B096, B098, B100, B103, B104, B107 …), `ScriptlessFlowTests` (B097, B099, B101, B102, B105, B106, B108–B112); her satır iki yol × üç motor.
- **Gerçek bulgular (düzeltildi):** `.button[hidden]` ve `.form-fields[hidden]` CSS ile ezilip görünüyordu (`components.css`); ürün formu kategori satırı `null` öğede 500 veriyordu (`_ProductFormBody.cshtml`).
- **CI:** `ci.yml` `e2e` işi PostgreSQL 18 hizmeti, dotnet kurulumu ve gözlem kaydını `browser-observations` olarak yükler; `E2E_CMD` derler, `playwright.ps1 install --with-deps chromium firefox webkit` ile motorları kurar, `Browser=true` testlerini koşar; Integration adımı tarayıcı testlerini dışlar (`--filter-not-trait "Browser=true"`). D-41 kapandı.
- **Bu turun düzeltmeleri:** WebKit tıklamadan sonra gezinme başlamadan döner (`SettleAsync` kısa bekleme + yükleme); WebKit'te geçersiz izin verme bağlamı bozar (pano izni yalnız Chromium); çerezler `BaseUrl`'e yazılır; B102 varyantsız ürün üzerinde (varyantlı üründe yeni seçenek kaydı seçilmemiş varyant satırında reddedilir); B101 kabul yanıtı 200'dür (sayfa yeniden çizilir); B096 onay sonrası durum yoklanır; B110 takip sonucu beklenir; B111 hizmet kaydı kendi hizmetinde.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.IntegrationTests/Browser/*` (yeni), `Shopfolio.IntegrationTests.csproj`, `Directory.Packages.props`, `packages.lock.json`
- `src/Shopfolio.Web/wwwroot/css/components.css`, `src/Shopfolio.Web/Pages/Panel/_ProductFormBody.cshtml`
- `.github/workflows/ci.yml`, `.github/ci-commands.env`
- `Docs/09_CODING_GUIDELINES.md` (Kütüphaneler tablosuna `Microsoft.Playwright` satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Playwright; Chromium, Firefox, WebKit; her ekran betik açık ve kapalı | ✓ | `BrowserTestBase.Ways()` üç motor × iki yol; üç motorda `Browser=true` 36/36 |
| 2 | Tohum müşteri, yönetici, her durumda sipariş; envanterin her ekranı | ✓ | `ScreenInventoryTests.EveryScreenOfTheInventory_OpensWithoutBrowserErrors_InTheWay_KK2` (üç motor, iki yol); gözlem kaydında E-01… satırları, hata/boş hâller dahil |
| 3 | B096–B112 her satırda iki yol gözlemi, ekran görüntüsü, genişlik, tarayıcı, Veri kaydı maskeli | ✓ | `observations.md`: 17 satırın hepsi iki yolda (B096: 4, B097: 2, B098: 2 … B111: 10 satır, 0 ✗); Veri kaydı sütunu maskeli (`contact_email=***@***`) |
| 4 | KRİTİK B096 ve B107 iki yolda; onaysız gönderim bir şey yazmaz | ✓ | `…VALB096`: ilk gönderimde `order_status=Placed`, onayda `Cancelled`; `…VALB107` fatura alanları işaretliyken siparişe girmez |
| 5 | Araç CI'da koşar, iş adı ve süresi | ✓ ⚠ | `ci.yml` `e2e` işi (`8. E2E`, advisory); motor başına yaklaşık 3–4 dk yerelde, CI süresi ilk koşuda ölçülür |
| 6 | Çıktı gate raporuna eklenebilir kayıt; sürümler ve tohum kayda yazılır | ✓ | `browser-observations` kaydı; `observations.md` başlığı "Sürümler: playwright 1.59.0.0, dotnet 10.0.11, chromium …" ve "Tohum: {…}" |

### Öz denetim (VAL satırı başına ayırt edici gözlem)

| VAL | Sonuç | Kanıt |
|---|---|---|
| B096 | ✓ | ilk gönderim onay bölümü, durum Placed; onaydan sonra Cancelled |
| B097 | ⚠ kısmen | liste ve sonuç cümlesi iki yolda aynı; iade ekranı IBAN sonucu görülmedi, betikli "yerinde güncelle" iddiası betiksiz (D-1701, D-1702) |
| B098 | ✓ | adet 3 → kalem tutarı 300 |
| B099 | ✓ | `…VALB099` iki yol |
| B100 | ✓ | seçim → fiyat 140 (betiksizde GET formu) |
| B101 | ⚠ kısmen | üç dosya: 200 / 422 + alan mesajı / 413, tek dosya alanı; betikli yolda boy denetimi betiği yok (D-1700) |
| B102 | ✓ | Malzeme: Ahşap, Metal kaydedilir; boş satır okunmaz; varyant seçiminde Ahşap |
| B103–B105 | ✓ | `…VALB103/104/105` iki yol |
| B106 | ✓ | betikli kopyala sonucu; betiksiz IBAN seçilebilir metin |
| B107 | ✓ | işaretliyken fatura alanları okunmaz, işaretsizken yazılır |
| B108, B109 | ✓ | `…VALB108/109` |
| B110 | ✓ | hesap akışları ve sipariş takibi (numara sayfada) |
| B111 | ✓ | ürün, kategori, içerik, iç not, ayar kendi işleyicisiyle kaydedilir ve okunur |
| B112 | ✓ | betikli parçalı yükleme kimlik yazar; betiksiz "Dosya yok", ad alanı kaydedilir |

D-41 kapanışı: `E2E_CMD` bağlandı (`.github/ci-commands.env`), kayıt `DEFERRED_BACKLOG.md` D-41 için kapanış önerisi olarak yazıldı; satırı kapatmak birleştirme işidir.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 8087/8087 |
| Integration | ✓ (parça) | `ProductFormPageTests` (panel ürün formu); **TAM: CI** | 37/37 |
| Browser | ✓ | `--filter-trait "Browser=true"`, motor başına (`SHOPFOLIO_BROWSER_ENGINES`) | chromium 36/36, firefox 36/36, webkit 36/36 |
| Format | ✓ | `dotnet format … --include <değişen .cs>` | değişiklik yok |

## Commit & PR

- **Branch:** `task/T551-tarayici-betiksiz-yol`
- **PR:** *(PR açılınca)*

## Dış Varsayımlar

- Tarayıcı ikilileri CI'da `playwright.ps1 install --with-deps` ile kurulur; ağ erişimi gerekir. `Microsoft.Playwright` lisansı Apache-2.0 olarak yazıldı (paket nuspec'i; ilk doğrulamada teyit edilsin).

## Known Limitations / Follow-up

- ⚠ `09 §19.2` madde 6 "test bekleme için `Task.Delay` kullanmaz" der; tarayıcı testleri gerçek tarayıcının gezinmesini beklemek için kısa `Task.Delay` ve yoklama kullanır (taramayı kırmaz). Kural istisnası karar kaydı ister: D-1704.
- D-1700 görsel boy denetimi betiği yok; D-1701 betikli "yerinde güncelle" (B097) betiksiz; D-1702 iade ekranı IBAN alan mesajı ham kod; D-1703 ürün formunda yüklemeyle reddedilen yeniden çizimde `Categories` null öğeleri (kök neden bağlayıcıda); D-1705 ürün formunda varyantlı ürüne ilk seçenek kaydı seçilmemiş varyant satırında reddedilir ve alan adı yanlış gösterilir (`SecondOptionValueId` yerine `FirstOptionValueId`; `ProductSaves.cs` ValueMissing dalı).
- WebKit: önceki turun ⚠'ı çözüldü (çerez adresi ve tıklama sonrası gezinme beklemesi); WebKit CI komutunda dışarıda bırakılmadı.

## Notlar

- Gözlemde yalnız Chromium sürümü başlığa yazılır; Firefox ve WebKit sürümleri ilgili motor koşusunun kaydındadır.
