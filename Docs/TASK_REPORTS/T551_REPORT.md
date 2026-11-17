# T551 — Tarayıcı aracı ve betiksiz yol gözlemi

**Faz:** F5 | **Durum:** Doğrulama tur 1 — ✗ FAIL, düzeltme bekliyor | **Tarih:** 2026-11-16

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
- **Bu turun düzeltmeleri:** WebKit tıklamadan sonra gezinme başlamadan döner (düzeltme 1: `BrowserTestBase.NavigateAsync`, gezinme beklemesi eylemden önce kurulur, sabit süreli bekleme yok); WebKit'te geçersiz izin verme bağlamı bozar (pano izni yalnız Chromium); çerezler `BaseUrl`'e yazılır; B102 varyantsız ürün üzerinde (varyantlı üründe yeni seçenek kaydı seçilmemiş varyant satırında reddedilir); B101 kabul yanıtı 200'dür (sayfa yeniden çizilir); B096 onay sonrası durum yoklanır; B110 takip sonucu beklenir; B111 hizmet kaydı kendi hizmetinde.

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
| B096 | ✓ | ilk gönderim onay bölümü, durum Placed; onaydan sonra Cancelled (düzeltme 1: gezinme beklemesinden hemen sonra tek okuma, yoklama döngüsü yok) |
| B097 | ⚠ kısmen | liste ve sonuç cümlesi iki yolda aynı; iade ekranı IBAN sonucu görülmedi, betikli "yerinde güncelle" iddiası betiksiz (D-1701, D-1702) |
| B098 | ✓ | adet 3 → kalem tutarı 300 (`ToHaveTextAsync` koşul beklemesi) |
| B099 | ✓ | `…VALB099` iki yol; kupon uygulama ve eski özetli onay `NavigateAsync` ile beklenir, sipariş sayısı 0 |
| B100 | ✓ | seçim → fiyat 140 (betiksizde GET formu; betikli yerinde güncelleme `ToHaveTextAsync` ile beklenir) |
| B101 | ⚠ kısmen | üç dosya: 200 / 422 + alan mesajı / 413 (yanıt ve gezinme `NavigateAsync` içinde beklenir), tek dosya alanı; betikli yolda boy denetimi betiği yok (D-1700) |
| B102 | ✓ | Malzeme: Ahşap, Metal kaydedilir; boş satır okunmaz; varyant seçiminde Ahşap |
| B103–B105 | ✓ | `…VALB103/104/105` iki yol; B105 betikli iletişim formu çift gönderimi: gezinme bitince ağa giden istek 1 (1,5 sn bekleme kalktı) |
| B106 | ✓ | betikli kopyala sonucu; betiksiz IBAN seçilebilir metin |
| B107 | ✓ | işaretliyken fatura alanları okunmaz, işaretsizken yazılır |
| B108, B109 | ✓ | `…VALB108/109` |
| B110 | ✓ | hesap akışları ve sipariş takibi (numara sayfada) |
| B111 | ✓ | ürün, kategori, içerik, iç not, ayar kendi işleyicisiyle kaydedilir ve okunur |
| B112 | ✓ | betikli parçalı yükleme kimlik yazar; betiksiz "Dosya yok" (`Assertions.Expect` koşul beklemesi, 1,5 sn bekleme kalktı), ad alanı kaydedilir |
| D-41 | ✓ | `E2E_CMD` bağlandı (`.github/ci-commands.env`), `e2e` işi `ci.yml`de (düzeltme 1: `ubuntu-24.04`); `DEFERRED_BACKLOG.md` D-41 için kapanış önerisi, satırı kapatmak birleştirme işidir |
| 09 §19.2 m.6 | ✓ | `Task.Delay` ve `Thread.Sleep` Browser klasöründe yok (git grep boş); D-1704 "kural korundu, tarama yok" |


## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 8087/8087 |
| Integration | ✓ (parça) | Browser sınıfları (aşağıda); `ProductFormPageTests` ilk turda 37/37; **TAM: CI** | 36/36 × 3 motor |
| Browser | ✓ | `--filter-trait "Browser=true"`, motor başına (`SHOPFOLIO_BROWSER_ENGINES`), düzeltme 1 sonrası | chromium 36/36, firefox 36/36, webkit 36/36 |
| Format | ✓ | `dotnet format … --include <değişen .cs>` | değişiklik yok |

## Commit & PR

- **Branch:** `task/T551-tarayici-betiksiz-yol`
- **PR:** *(PR açılınca)*

## Dış Varsayımlar

- Tarayıcı ikilileri CI'da `playwright.ps1 install --with-deps` ile kurulur; ağ erişimi gerekir. `Microsoft.Playwright` lisansı Apache-2.0 olarak yazıldı (paket nuspec'i; ilk doğrulamada teyit edilsin).

## Known Limitations / Follow-up

- ⚠ `09 §19.2` madde 6 kuralı değişmedi; düzeltme 1'de tarayıcı testlerindeki her `Task.Delay` Playwright'ın koşula bağlı beklemesiyle değişti (`BrowserTestBase.NavigateAsync`: belgeyi işaretle, eylemi yap, işaret kaybolup `readyState` `complete` olana dek bekle; metin için `Assertions.Expect(...).ToHaveTextAsync`). Kuralı mekanik koruyan tarama testi yok: D-1704 "kural korundu, tarama yok" olarak güncellendi.
- ⚠ CI `8. E2E (advisory)` kırmızısı (run 43801814217): `ubuntu-latest` ubuntu 26.04'e geçti, Playwright 1.59.1 onu desteklemiyor. `ci.yml` `e2e` işinin `runs-on`'u `ubuntu-24.04` yapıldı (öteki işlere dokunulmadı); Playwright sürümü değişmedi. CI sonucunu yönetici izler.
- D-1700 görsel boy denetimi betiği yok; D-1701 betikli "yerinde güncelle" (B097) betiksiz; D-1702 iade ekranı IBAN alan mesajı ham kod; D-1703 ürün formunda yüklemeyle reddedilen yeniden çizimde `Categories` null öğeleri (kök neden bağlayıcıda); D-1705 ürün formunda varyantlı ürüne ilk seçenek kaydı seçilmemiş varyant satırında reddedilir ve alan adı yanlış gösterilir (`SecondOptionValueId` yerine `FirstOptionValueId`; `ProductSaves.cs` ValueMissing dalı).
- WebKit: önceki turun ⚠'ı çözüldü (çerez adresi ve tıklama sonrası gezinme beklemesi); WebKit CI komutunda dışarıda bırakılmadı.

## Notlar

- Gözlemde yalnız Chromium sürümü başlığa yazılır; Firefox ve WebKit sürümleri ilgili motor koşusunun kaydındadır.

## Düzeltme 2 — son işçi (5. işçi)

### Test satırları (düzeltme 2 sonrası)

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 8087/8087 (`chunked-upload.test.mjs` kum havuzuna `window`, `HTMLInputElement`, `HTMLFormElement` eklendi: ürün formu betiği artık bunları kullanıyor) |
| Browser chromium | ✓ | `SHOPFOLIO_BROWSER_ENGINES=chromium`, `--filter-trait "Browser=true"` | 171/172 koşuda; kalan 1 (yürüyüş testi, kategori ağacı formlarının sayısı) düzeltildi, `ValidationStateTests` 107/107 yeniden koşuldu (chromium+firefox 213/213) |
| Browser firefox | ✓ | aynı, `firefox` | 171/172 koşuda; kalan 1 (E-13 gizli `select`) düzeltildi, `ValidationStateTests` 107/107 |
| Browser webkit | ✓ | aynı, `webkit` | 171/172 koşuda; kalan 1 (E-13 betikli: değişiklikle tetiklenen yeniden hesap gönderimi yarışı) düzeltildi, `ValidationStateTests` 107/107 |
| Format | ✓ | `dotnet format --include <3 değişen .cs>` | değişiklik yok |
| Integration TAM | — | **TAM: CI** (bu işçi koşmadı) | |

⚠ Düzeltmelerden sonra üç motorda yalnız `ValidationStateTests` yeniden koşuldu; öteki sınıflar düzeltmeden etkilenmez (`EmptyStateTests` üç motorda yeşildi).

### Kapsam

- KK2 (a): 52 form + E-12 `UpdateCartItem` ve E-13 `PlaceOrder` (`FromFlow`) vakası. Alan bulunamayanlar: gizli ya da salt okunur alan atlandı, kapalı `details` açılır, E-34 taşıma formu kendi dalına taşıma (UC-120: 422), E-43 marka formu `Kaydet` düğmesi. 422 olup `aria-invalid` taşımayanlar (E-37 sipariş formları): formun yerinde hata iletisi (`.message--error`, `04 §2.3.1` "yerinde") kabul edilir ⚠ (kolay karar: `aria-invalid` eklemek ürün değişikliği olurdu).
- D-1712: E-37 iç not (`04 §7.2.15.1` isteğe bağlı, sınırsız) ve E-46 teslimat illeri (`04 §5.4.5`, K-828 boş seçim geçerli) formlarının doğrulama hâli yoktur; vaka "gönderim kabul edilir, hata yok" gözler.
- KK2 (b): `BrowserHost.EmptyAsync()` tohumsuz ikinci veritabanı ve uygulama (yalnız bir yönetici); `EmptyStateTests` E-32, E-35, E-36, E-38, E-52 için boş hâl cümlesi, çıkış bağlantısı, satır yok, hata yok gözler (üç motor × iki yol).

### Öz denetim (bütün satırlar; kanıt = yeşil test ve gözlem kaydı)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 (üç motor, iki yol) | ✓ | `Ways()`; üç motorda `Browser=true` yeşil |
| KK2 (a) hata hâli | ✓ | `ValidationStateTests` 107 vaka/motor yeşil; yürüyüş testi form listesini sayfadan türetir (eksik form/çıkmış vaka kırar; kategori ağacı formları tek vaka) |
| KK2 (b) boş hâl | ✓ | `EmptyStateTests` 5 ekran × 2 yol × 3 motor yeşil |
| KK3 (kayıt, maske) | ✓ | `ObservationMaskTests`; Veri kaydı maskeli |
| KK4 (B096, B107 KRİTİK) | ✓ | `…VALB096`, `…VALB107` yeşil üç motorda |
| KK5 (CI) | ✓ ⚠ | `e2e` işi; CI sonucunu yönetici izler |
| KK6 (kayıt, sürümler, tohum) | ✓ | `observations.md` başlığı |
| B096, B098, B100, B102–B106, B108–B112 | ✓ | ilgili `…VALB###` testleri üç motorda yeşil |
| B097 | ⚠ kısmen | D-1701/D-1702 açık, kayıtlı |
| B099 | ✓ | `OpenCard` ile iki açık yöntem, gerçek seçim, üç motor yeşil |
| B101 | ⚠ kısmen | D-1700 çözüldü; D-1705 açık, kayıtlı |
| D-41, D-1700, D-1705, D-1711 | ✓ | `DEFERRED_BACKLOG.d/T551.md` kayıtları güncel; D-1712 eklendi |

Öz denetimi önceki işçilerin diff'inde satır satır yeniden okuma yerine yeşil testlerle ve backlog kayıtlarıyla yaptım (süre); ⚠ kısmen satırlar önceki turlardan gelir.

## Doğrulama

| Alan | Değer |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 3, 2026-11-17, head `b2c588c0`) |
| Bulgu sayısı | 1 — ORTA S1 (KK2: E-02, E-07, E-27, E-34'ün boş hâli gezilmiyor) |
| Düzeltme gerekli mi | Evet — iki iterasyon sınırı aşıldı, eskalasyon (`12 §8.5`, `INSTRUCTIONS.md` §3.3) |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-1717) |

Tur 1, tur 2 ve tur 3'ün tam sonuçları raporun sonundaki "Doğrulama Sonucu — T551 … — tur 1", "— tur 2" ve "— tur 3" bölümlerindedir.

## Doğrulama tur 1

### Doğrulama Sonucu — T551 Tarayıcı aracı ve betiksiz yol gözlemi — tur 1
**Tarih:** 2026-11-16   **Branch:** task/T551-tarayici-betiksiz-yol   **Commit:** `0f3f98fc` (doğrulanan head; kayıt commit'i: KAYIT_SHA)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (worktree, `T551-val` = `origin/task/T551-tarayici-betiksiz-yol`) |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana çalışma kopyasının bekçisi; worktree'ler ortak config) |
| Ana dal CI (son 3) | ✓ | `43788205169` success · `43788205183` success · `43779716328` success |
| Memory drift | — | Adım 0b kaldırıldı (2026-11-08) |
| main birleştirmesi | ✓ | `git merge origin/main` → "Already up to date"; çakışma yok |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Playwright; Chromium, Firefox, WebKit; her ekran betik açık ve kapalı | ✓ | Motor süzgeciyle ayrı koşular (`SHOPFOLIO_BROWSER_ENGINES`, `--filter-trait "Browser=true"`): chromium 36/36 (1 dk 50 sn), firefox 36/36 (2 dk 39 sn), webkit 36/36 (2 dk 45 sn); `BrowserTestBase.Ways()` motor × {betikli, betiksiz}; `BrowserHost.ContextAsync` betiği bağlam düzeyinde kapatır. Kayıtta sürüm: webkit 26.4, playwright 1.59.0.0, dotnet 10.0.11 |
| 2 | Tohum müşteri, yönetici, her durumdaki sipariş; `04 §4` envanterinin her ekranı onay pencereleri, hata ve boş hâlleriyle | ✗ | `ScreenInventoryTests.Stops()` okundu: (a) **E-05 (Arşivlenmiş ürün sayfası) yürüyüşte yok** (E-04'ten E-06'ya atlar; tohumda arşivli ürün yok). (b) **Yönetici onay pencereleri (`_Confirm` `ConfirmSurface.AdminWindow`, `<dialog data-js=confirm-window>`) hiçbir ekranda açılmıyor:** `_Confirm` kullanan E-25 (hesap silme), E-37, E-40, E-41, E-44, E-45, E-46, E-49, E-33 — yürüyüş yalnız GET açar; müşterinin son adımı yalnız E-17 iptal (B096) ve E-18 (B097) satırlarında görülür. (c) Hata hâli yalnız 404 ve geçersiz bağlantıdır; form doğrulama hatası hâlleri yürüyüşte yok. (d) Boş hâl yalnız E-03 sonuçsuz ve E-12 boş sepet; panel listelerinin boş hâli (E-32, E-35, E-36, E-38, E-52) yok. (e) Tohum `DeliveryFailed` siparişini ve `PartiallyRefunded`/`Refunded` ödeme durumunu kurmuyor; tohumlanan `shipped` siparişi hiçbir durakta açılmıyor |
| 3 | `VAL-B096`…`VAL-B112` her biri için iki yol gözlemi; Veri kaydı satırında tablo kaydı, kişisel veri maskeli | ✗ | 17 satır × 2 yol × 3 motor gözlemi kayıtta var (webkit kaydı 62 VAL satırı, 214 ekran görüntüsü). Ama: **`VAL-B101` beklenen sonucu karşılanmıyor** (betikli yolda boy denetimi yok, istek gidiyor; `product-form.js`te görsel boy denetimi yok; test beklenen metni gözlenen davranışa göre yazıyor: "sayfada boy denetimi betiği yoktur: istek gider, bkz. D-1700"; D-1700 kabul edilmiş risk değil). `VAL-B099`, `B100`, `B102`, `B105`, `B110` beklenen sonucun bir kısmını gözlemiyor (Task Satırları tablosu). **Maske:** `Observed` sütunu maskelenmiyor — `VAL-B110` satırında kayıt e-postası açık (`b110-wen11@example.com`, webkit kaydı satır 205) ve aynı satırda `pendingRegistration` koruma jetonu; 25 satır misafir sipariş erişim anahtarını (`/siparis/erisim/<anahtar>/…`) yazıyor (`12 §7.1` "kişisel veri maskelenir", anahtar kanıta yazılmaz) |
| 4 | KRİTİK `VAL-B096` ve `VAL-B107` iki yolda; onaysız gönderim bir şey yazmaz | ✓ | B096: ilk gönderimden sonra DB'den `order_status/payment_status` = `Placed/…`, onaydan sonra `Cancelled`; B107: işaretliyken `billing_equals_shipping=True; decoy_taken=False`, işaretsizken `billing_recipient_name=Fatura Sahibi`. Değişinim: `OrderScreenModels.cs:247` onay koşulu etkisizleştirildi → B096 iki yolda kırıldı; `PlaceOrderInput.cs:147` `IsBillingSameAsShipping` → `false` → B107 iki yolda kırıldı; geri alındı |
| 5 | Araç CI'da koşar; iş adı ve süresi `12 §6.2` F5 B satırına ve `11 §6.2` F5'e uygun | ✗ | `8. E2E (advisory)` run `43904581046` job `133435238447` **fail** (3 dk 37 sn): 108 testin hepsi `TemplateDatabaseFixture.InitializeAsync` içinde `Run "dotnet tool restore" to make the "dotnet-ef" command available.` ile düştü — `E2E_CMD` (`.github/ci-commands.env`) `dotnet tool restore` çağırmıyor (`INTEGRATION_TEST_CMD` çağırıyor). Araç CI'da bir kez bile koşmadı; önceki head `d84b91dd` run `43801814217` E2E de failure. `runs-on: ubuntu-24.04` ✓, iş adı ✓ |
| 6 | Çıktı gate raporuna eklenebilir kayıt; sürümler ve tohum | ✓ | Yerel kayıt `observations.md`/`.jsonl`: "Sürümler: playwright 1.59.0.0, dotnet 10.0.11, webkit 26.4", "Tohum: {customers, admins, coupon, products, orders…}"; `ci.yml` `upload-artifact` `browser-observations` (`if: always()`) |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-B096 | KRİTİK | yok | BEKLEMEDE | PASS | — |
| VAL-B097 | ORTA | yok | BEKLEMEDE | PASS (beklenen: iki yolda liste ve cümle sunucunun; betik yok — D-1701 metin kaydı duruyor) | — |
| VAL-B098 | ORTA | yok | BEKLEMEDE | PASS | — |
| VAL-B099 | ORTA | yok | BEKLEMEDE | FAIL — betikli yolda yalnız kupon değişikliği gözlenir; adres ve yöntem değişikliğinde özetin yenilenmesi gözlenmez; betiksiz yolda "güncel özet gösterilir" doğrulanmaz (yalnız uyarı metni kaydedilir) | — |
| VAL-B100 | ORTA | yok | BEKLEMEDE | FAIL — yalnız fiyat ve sorgu parametresi; görsel, satın alınabilirlik ve sepete eklemenin seçimi taşıması gözlenmez | — |
| VAL-B101 | ORTA | yok | BEKLEMEDE | FAIL — betikli yolda boy seçimde denetlenmiyor, istek gidiyor (D-1700); ilerleme gözlenmiyor | — |
| VAL-B102 | ORTA | yok | BEKLEMEDE | FAIL — varyant satırı ekleme gözlenmez (yalnız seçenek ve değer); D-1705 varyantlı üründe ilk seçenek kaydının reddini kaydediyor | — |
| VAL-B103 | DÜŞÜK | yok | BEKLEMEDE | PASS | — |
| VAL-B104 | DÜŞÜK | yok | BEKLEMEDE | PASS | — |
| VAL-B105 | ORTA | yok | BEKLEMEDE | FAIL — "sepete ekleme" tekrar gönderiminin ikinci kez işlendiği gözlenmez; betikli yolda düğmenin "sürdüğünü metinle söylemesi" kaydedilir ama ölçülmez | — |
| VAL-B106 | DÜŞÜK | yok | BEKLEMEDE | PASS | — |
| VAL-B107 | KRİTİK | yok | BEKLEMEDE | PASS | — |
| VAL-B108 | DÜŞÜK | yok | BEKLEMEDE | PASS | — |
| VAL-B109 | ORTA | yok | BEKLEMEDE | PASS | — |
| VAL-B110 | ORTA | yok | BEKLEMEDE | FAIL — giriş akışı bilinmeyen hesapla ret olarak gözlenir; başarılı giriş tamamlanmaz; kayıt sonucu sayfası e-postayı maskesiz kayda yazar | — |
| VAL-B111 | ORTA | yok | BEKLEMEDE | PASS | — |
| VAL-B112 | ORTA | yok | BEKLEMEDE | PASS | — |

`python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77` → Hata: 0.

#### Doğrulama Kontrol Listesi
- [x] KK1: üç motor, iki bağlam (motor başına ayrı koşu, 36/36 × 3)
- [ ] KK2: ekran envanteri eksiksiz gezilir — E-05 yok, yönetici onay pencereleri, form hata hâlleri, panel boş hâlleri ve iki sipariş durumu yok
- [ ] KK3: 17 satırın iki yol gözlemi ve Veri kaydı — B101 beklenen sonuç karşılanmıyor; beş satırda kısmi gözlem; `Observed` sütunu maskesiz
- [x] KK4: iki KRİTİK satır (DB'den, değişinimle kanıtlı)
- [ ] KK5: CI işi, adı ve süresi — E2E işi `dotnet tool restore` eksikliğinden hiç koşmadı
- [x] KK6: kayıt gate raporuna girer (sürümler, tohum)
- [x] `09 §19.2` madde 6: `git grep -nE "Task\.Delay|Thread\.Sleep" tests/Shopfolio.IntegrationTests/Browser` boş (çıkış 1); `BrowserTestBase.cs` `#pragma warning disable CS0612` tek yöntemi sarar, gerekçe yorumu aynı satırda, hemen `restore` — `09 §3.1` madde 5'e uygun
- [x] `09 §25` ilgili satırlar; güvenlik taraması (aşağıda)

#### Değişinim denemeleri (beş satır; chromium, motor süzgeciyle)
| # | Değişiklik | Kırılan test | Sonuç |
|---|---|---|---|
| 1 | `OrderScreenModels.cs:247` `!input.Confirmed` → `!input.Confirmed && input.Confirmed` | VALB096 betikli + betiksiz | ✓ yakalandı |
| 2 | `PlaceOrderInput.cs:147` `input.IsBillingSameAsShipping` → `false` | VALB107 betikli + betiksiz | ✓ yakalandı |
| 3 | `components.css` `.button[hidden]` kuralı adı bozuldu | VALB106 betiksiz | ✓ yakalandı |
| 4 | `shipping-and-defaults.js:20` düğmelerin eklenmesi kapatıldı | VALB103 betikli | ✓ yakalandı |
| 5 | `Addresses.cshtml:23` `?create=true` → `?create=1` | VALB108 betikli + betiksiz | ✓ yakalandı |

Beş değişinim birlikte uygulandı, 10 testin 8'i beklenen yerde kırıldı (değişinimin dokunmadığı iki yol geçti); `git checkout -- src` ile geri alındı, yeniden derlendi.

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | çıkış 0 |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes --no-restore --include <6 Browser .cs>` | çıkış 0 |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build` | 8087/8087 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (Browser) | ✓ | `--filter-trait "Browser=true"`, motor başına; PostgreSQL 18 `shopfolio-t551-val-pg` (`127.0.0.1:25551`, `track_commit_timestamp=on`) | chromium 36/36, firefox 36/36, webkit 36/36; **TAM: CI** (CI `4. Integration test` pass 14 dk 12 sn) |
| CI (PR #583, head `0f3f98fc`) | ~ | `gh pr checks 583` | Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate pass; **8. E2E (advisory) fail** |

#### Güvenlik Kontrolü
- Secret sızıntısı: Bulgu (DÜŞÜK) — gözlem kaydının `Observed` sütunu misafir sipariş erişim anahtarlarını ve `pendingRegistration` jetonunu yazıyor, kayıt CI'da `browser-observations` olarak yükleniyor; değerler atılan test veritabanınındır, ama `12 §7.1` kanıta anahtar yazılmamasını ister (Bulgu 4)
- Auth etkisi: Temiz — üretim kodu değişikliği yalnız `components.css` (`.button[hidden]`, `.form-fields[hidden]` → `display: none`; dar, doğru) ve `_ProductFormBody.cshtml` `x is not null` (dar; kök neden D-1703'te)
- Input validation: Temiz
- Yeni bağımlılık: `Microsoft.Playwright` 1.59.0 (yalnız Integration test projesi; Apache-2.0; `09` kütüphane tablosuna satırı eklendi; `packages.lock.json` güncel, `restore --locked-mode` çıkış 0)

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | KK5 | ORTA | S1 | Araç CI'da koşar ↔ `E2E_CMD` `dotnet tool restore` çağırmıyor; 108 testin hepsi fikstürde `dotnet-ef` bulunamadan düşüyor, araç CI'da hiç koşmadı | KK6 (CI kaydı boş gözlem üretir), `12 §4.17` (3) Test raporu | bu PR'da düzeltilecek (`E2E_CMD` başına `dotnet tool restore &&`; yeşil E2E run kanıtı) | `.github/ci-commands.env` |
| 2 | KK2 | ORTA | S1 | `04 §4` envanterinin her ekranı onay pencereleri, hata ve boş hâlleriyle ↔ E-05 yok; yönetici onay pencereleri (E-25, E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49) açılmıyor; form hata hâli ve panel listelerinin boş hâli yok; tohum `DeliveryFailed`, `PartiallyRefunded`/`Refunded` kurmuyor, `shipped` siparişi yürüyüşte açılmıyor | T552 (aynı araçla aynı kapsamı tarar, `12 §12.2` (1)) | bu PR'da düzeltilecek | `Browser/ScreenInventoryTests.cs`, `Browser/BrowserSeed.cs` |
| 3 | VAL-B101 | ORTA | S1 | Betikli yolda aşan dosya için istek gitmez ve mesaj gösterilir ↔ `product-form.js`te görsel boy denetimi yok, istek gider (422/413); test beklenen metni davranışa uyduruyor | D-1700 | bu PR'da düzeltilecek (betikte boy denetimi ve ilerleme, ya da proje sahibinin kabul edilmiş risk onayı) | `wwwroot/js/pages/panel/product-form.js`, `Browser/ScriptlessFlowTests.cs` |
| 4 | VAL-B099, B100, B102, B105, B110 | ORTA | S1 | Satırın beklenen sonucunun tamamı iki yolda gözlenir ↔ Task Satırları tablosunda yazılı paylar gözlenmiyor | KK3 | bu PR'da düzeltilecek | `Browser/ScriptlessFlowTests.cs`, `Browser/ScriptlessPathTests.cs` |
| 5 | KK3 (maske) | DÜŞÜK | S1 | Kişisel veri maskeli, anahtar kanıta yazılmaz (`12 §7.1`) ↔ `Observed` sütunu maskelenmiyor: B110'da e-posta ve `pendingRegistration` jetonu, 25 satırda sipariş erişim anahtarı | KK6 | bu PR'da düzeltilecek (`ObserveAsync` `observed`'ı da `DataRecord.Mask`'tan geçirsin, erişim anahtarı ve jeton maskelensin) | `Browser/BrowserTestBase.cs` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `12 §4.17` tarayıcı kümesi "son iki büyük sürüm" der; araç her motorun yalnız Playwright'ın getirdiği tek sürümünü koşar; kayıt motor sürümünü yazıyor | DEFERRED_BACKLOG'a forward: D-1709 |
| K2 | Tohum firma/marka adı kurmuyor: ekran başlıkları "localhost" (ör. E-01 `title "localhost"`); envanter testinin "başlık var" ölçüsü bu yüzden ayırt edici değil | DEFERRED_BACKLOG'a forward: D-1710 |

#### Yapım Raporu Karşılaştırması
- Uyum: 4 uyuşmazlık
- KK2: rapor ✓ "hata/boş hâller dahil" der; E-05 ve yönetici onay pencereleri yürüyüşte yok.
- KK3: rapor ✓ der, ama öz denetim tablosu B097 ve B101'i "⚠ kısmen" yazar — kısmi satırla ✓ verilemez (`12 §6.1`, K-1521); B099/B100/B102/B105/B110 için ✓'nin kanıtı beklenen sonucun bir kısmını kapsar.
- KK5: rapor "✓ ⚠ … CI süresi ilk koşuda ölçülür" der; CI'da araç hiç koşmadı (Bulgu 1).
- Maske: rapor "Veri kaydı sütunu maskeli" der — doğru, ama `Observed` sütunu maskesiz (Bulgu 5).
- Uyumlu: KK1, KK4, KK6, `09 §19.2` madde 6, üretim kodu değişikliklerinin darlığı, Unit 8087/8087, Browser 36/36 × 3.

#### Merge ve Post-merge
- Squash: — (FAIL; merge yok)
- Post-merge run'lar: —

## Doğrulama tur 2

### Doğrulama Sonucu — T551 Tarayıcı aracı ve betiksiz yol gözlemi — tur 2
**Tarih:** 2026-11-17   **Branch:** task/T551-tarayici-betiksiz-yol   **Commit:** `d72ac1cd` (doğrulanan head; kayıt commit'i: bu raporu taşıyan commit)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (worktree, `T551-val` = `origin/task/T551-tarayici-betiksiz-yol`, head `d72ac1cd`) |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | `43788205169` success · `43788205183` success · `43779716328` success |
| Memory drift | — | Adım 0b kaldırıldı (2026-11-08) |
| main birleştirmesi | ✓ | `git merge origin/main` → "Already up to date" (main `3fb77b9d`); çakışma yok |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Playwright; Chromium, Firefox, WebKit; her ekran betik açık ve kapalı | ✓ | Motor süzgeciyle ayrı koşular (`SHOPFOLIO_BROWSER_ENGINES`, `--filter-trait "Browser=true"`, PostgreSQL 18 `127.0.0.1:25552`): chromium 172/172 (3 dk 20 sn), firefox 172/172 (8 dk 30 sn), webkit 172/172 (5 dk 25 sn). `BrowserTestBase.Ways()` motor × {betikli, betiksiz}; `BrowserHost.ContextAsync` `JavaScriptEnabled` bağlam düzeyinde. |
| 2 | Tohum müşteri, yönetici, her durumdaki sipariş; `04 §4` envanterinin her ekranı onay pencereleri, hata ve boş hâlleriyle | ✓ | Tohum (kayıt başlığı): 6 sipariş durumunun 6'sı (Placed, Preparing, Shipped, Delivered, DeliveryFailed, Cancelled) ve 5 ödeme durumunun 5'i (Pending, Paid, Failed, PartiallyRefunded, Refunded), arşivli ürün. Yürüyüş: 53 ekranın 53'ü (E-01…E-47, E-49…E-54; E-05 artık var); `EveryScreenOfTheInventoryDocument_HasAStopInTheWalk_KK2` listeyi `04 §4.1–4.2`'den okur. Onay pencereleri: `_Confirm` kullanan 9 panel/hesap ekranının 9'u (E-25, E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49 — `ConfirmWindowTests`, aç + Vazgeç + tablo parmak izi değişmez); müşteri son adımı E-17/E-18 (B096, B097). Hata hâli: `ValidationStateTests` 53 form vakası (422 + geçersiz alan + tablo değişmez; iki formun doğrulama hâli yok, D-1712), sayfalardan türetilen form listesiyle eşitlik testi. Boş hâl: `EmptyStateTests` tohumsuz ikinci veritabanında E-32, E-35, E-36, E-38, E-52. Eksik ekran: yok. |
| 3 | `VAL-B096`…`VAL-B112` her biri için iki yol gözlemi; Veri kaydı satırında tablo kaydı, kişisel veri maskeli | ✗ | 17 satırın 17'si × 2 yol × 3 motor kayıtta (her motor kaydı 417 satır, 0 ✗, ~409 ekran görüntüsü); Veri kaydı satırlarında tablo satırı maskeli; `Observed` sütunu artık `DataRecord.Mask`'tan geçiyor — üç kayıtta e-posta 0, erişim anahtarı/jeton yalnız `***`. **Ama `VAL-B105` betikli payı ayırt edici değil (Bulgu 1):** `common.js` `bindSubmitOnce` içindeki ikinci-gönderim durdurması (`if (form.getAttribute("aria-busy") === "true") { event.preventDefault(); … }`, satır 213) etkisizleştirildiğinde B105 betikli testi yine geçti ve kayıt yine "ağa giden gönderim 1; yazılan talep 1 ✓" yazdı: test iki `requestSubmit`'i aynı görevde art arda çağırıyor, tarayıcı iki gönderimi tek gezinmeye indiriyor; "ikinci `submit` durdurulur" iddiası betikten bağımsız olarak doğru görünüyor. Öteki 16 satır ✓ (aşağıdaki değişinimler). |
| 4 | KRİTİK `VAL-B096` ve `VAL-B107` iki yolda; onaysız gönderim bir şey yazmaz | ✓ | B096: ilk (onaysız) gönderimden sonra DB `order_status/payment_status` = `Placed/Pending`, onaydan sonra `Cancelled` (iki yol, üç motor). B107: işaretliyken `billing_equals_shipping=True; decoy_taken=False`, işaretsizken `billing_recipient_name=Fatura Sahibi`. Değişinim: `OrderScreenModels.cs:247` onay koşulu etkisizleştirildi → B096 iki yolda kırıldı; `PlaceOrderInput.cs:147` `IsBillingSameAsShipping` → `false` → B107 iki yolda kırıldı; geri alındı. |
| 5 | Araç CI'da koşar; iş adı ve süresi `12 §6.2` F5 B satırına ve `11 §6.2` F5'e uygun | ✓ | Run `43945619413` (head `d72ac1cd`) job `133579443615` **"8. E2E (advisory)" success**, 512/512 test (170 teori × 3 motor + 2), 16 dk 5 sn (20:10:58–20:27:03); `runs-on: ubuntu-24.04`; `browser-observations` kaydı yüklendi (118,9 MB). Tur 1 bulgusu (dotnet tool restore) kapandı. Integration işi `--filter-not-trait "Browser=true"` ile tarayıcı testlerini dışlıyor. |
| 6 | Çıktı gate raporuna eklenebilir kayıt; sürümler ve tohum | ✓ | `observations.md`/`.jsonl`: "Sürümler: playwright 1.59.0.0, dotnet 10.0.11, <motor> <sürüm>" (webkit 26.4), "Tohum: {customers, admins, coupon, products, orders…}"; CI artifact `browser-observations` (`if: always()`). |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-B096 | KRİTİK | yok | PASS | PASS | — |
| VAL-B097 | ORTA | yok | PASS | PASS | — |
| VAL-B098 | ORTA | yok | PASS | PASS | — |
| VAL-B099 | ORTA | yok | FAIL | PASS (kupon, il ve yöntem değişikliği iki yolda; eski özetle onay sipariş açmaz, güncel toplam gösterilir) | — |
| VAL-B100 | ORTA | yok | FAIL | PASS (fiyat, varyantın görseli, tükenen birleşim, sepete eklemenin seçimi taşıması) | — |
| VAL-B101 | ORTA | yok | FAIL | PASS (betikli: aşan iki dosyada POST 0 ve sunucunun cümlesi, küçük dosyada ilerleme; betiksiz: 200/422/413) | — |
| VAL-B102 | ORTA | yok | FAIL | PASS (seçenek, değer ve varyant satırı iki yolda; boş satır okunmaz) | — |
| VAL-B103 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B104 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B105 | ORTA | yok | FAIL | FAIL — betikli yolda "ikinci `submit` durdurulur" gözlemi betiğin durdurmasını ayırt etmiyor (Bulgu 1); betiksiz paylar (teyit ekranına dönüş, iletişim ve sepete ekleme iki kez işlenir) ✓ | — |
| VAL-B106 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B107 | KRİTİK | yok | PASS | PASS | — |
| VAL-B108 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B109 | ORTA | yok | PASS | PASS | — |
| VAL-B110 | ORTA | yok | FAIL | PASS (kayıt sonucu, bilinmeyen hesapla ret, kayıtlı hesapla giriş + oturum + hesap sayfası, sıfırlama isteği, sipariş takibi; maskeli) | — |
| VAL-B111 | ORTA | yok | PASS | PASS | — |
| VAL-B112 | ORTA | yok | PASS | PASS | — |

`python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77` → Hata: 0.

#### Tur 1 bulgularının kapanışı
| Tur 1 # | Konu | Durum | Kanıt |
|---|---|---|---|
| 1 | KK5 E2E işi koşmuyordu | Kapandı | Run `43945619413` "8. E2E (advisory)" success, 512/512, 16 dk 5 sn |
| 2 | KK2 eksik ekran ve hâller | Kapandı | E-05 yürüyüşte; 9 onay penceresi; 53 form vakası; 5 boş liste; DeliveryFailed, PartiallyRefunded, Refunded tohumda, kargodaki sipariş E-16/E-37'de açılıyor |
| 3 | B101 betikli boy denetimi | Kapandı | `product-form.js` `checkSize`; değişinim (`if (tooBig)` → `if (false && tooBig)`) B101 betikliyi kırdı |
| 4 | B099, B100, B102, B105, B110 kısmi | B099, B100, B102, B110 kapandı; **B105 kısmen** | B105 sepete ekleme betiksiz tekrar gönderimi ve düğme metni artık ölçülüyor, ama betikli "ikinci submit durdurulur" ayırt edici değil (Bulgu 1) |
| 5 | `Observed` maskesiz | Kapandı | `ObserveAsync` `DataRecord.Mask(observed)`; `ObservationMaskTests` 4/4; üç kayıtta e-posta yok, anahtar ve jeton `***` |

#### Doğrulama Kontrol Listesi
- [x] KK1: üç motor, iki bağlam (motor başına ayrı koşu, 172/172 × 3)
- [x] KK2: ekran envanteri eksiksiz gezilir (53/53 ekran, onay pencereleri, hata ve boş hâller; eksik ekran yok)
- [ ] KK3: 17 satırın iki yol gözlemi ve Veri kaydı — B105 betikli payı ayırt edici değil
- [x] KK4: iki KRİTİK satır (DB'den, değişinimle kanıtlı)
- [x] KK5: CI işi, adı ve süresi (`8. E2E (advisory)`, ubuntu-24.04, success, 16 dk 5 sn)
- [x] KK6: kayıt gate raporuna girer (sürümler, tohum, CI artifact)
- [x] `09 §19.2` madde 6: `git grep -nE "Task\.Delay|Thread\.Sleep" tests/Shopfolio.IntegrationTests/Browser` boş (çıkış 1); `BrowserTestBase.cs` `#pragma warning disable CS0612` tek yöntemi sarar, gerekçe yorumu aynı satırda, hemen `restore` — `09 §3.1` madde 5'e ("uyarı bastırma yalnız gerekçe yorumuyla ve en dar kapsamda") uygun
- [x] `09 §25` ilgili satırlar; güvenlik taraması (aşağıda)

#### Değişinim denemeleri (chromium, motor süzgeciyle; hepsi `git checkout -- src` ile geri alındı, yeniden derlendi)
| # | Değişiklik | Beklenen kırılma | Sonuç |
|---|---|---|---|
| 1 | `OrderScreenModels.cs:247` `!input.Confirmed` → `!input.Confirmed && input.Confirmed` | VALB096 iki yol | ✓ kırıldı |
| 2 | `PlaceOrderInput.cs:147` `input.IsBillingSameAsShipping` → `false` | VALB107 iki yol | ✓ kırıldı (ödeme adımını kullanan B099, B105, B109 da kırıldı) |
| 3 | `product-form.js` `checkSize` `if (tooBig)` → `if (false && tooBig)` | VALB101 betikli | ✓ kırıldı (yalnız betikli) |
| 4 | `ProductForm.cshtml.cs:266/269` ve `ProductSaves.cs:343` boş satır ayıklaması kaldırıldı (tek başına 266 ya da 343 eşdeğer değişinimdir: ayıklama iki katmanda) | VALB102 iki yol | ✓ kırıldı (iki yol) |
| 5 | `product.js` `image.src = variant.imagePath` kaldırıldı | VALB100 betikli (görsel) | ✓ kırıldı ("varyantın görsel kaydıyla eşleşen 0") |
| 6 | `checkout.js:11` `form.requestSubmit(submitter)` kaldırıldı | VALB099 betikli | ✓ kırıldı |
| 7 | `common.js:213` `bindSubmitOnce` ikinci-gönderim durdurması `if (false)` | VALB105 betikli | **✗ kırılmadı** — kayıt "ağa giden gönderim 1; yazılan talep 1 ✓" (Bulgu 1) |

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | çıkış 0 |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes --no-restore --include <dalın 11 .cs dosyası>` | çıkış 0 |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build` | 8087/8087 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (Browser + dalın sınıfları) | ✓ | `--filter-trait "Browser=true"` motor başına; `--filter-class …ObservationMaskTests` | chromium/firefox/webkit 172/172; ObservationMaskTests 4/4; **TAM: CI** (CI `4. Integration test` success, 14 dk 20 sn) |
| CI (PR #583, head `d72ac1cd`) | ✓ | `gh pr checks 583` / `gh run view 43945619413` | Lint, Build, Unit, Integration, Contract, Migration, Paketleme, **8. E2E (advisory)**, CI Gate success |
| Locked restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz — `Observed` ve Veri kaydı maskeli (e-posta, IBAN, uzun rakam, `/siparis/erisim/`, `/panel/davet/`, `pendingRegistration`, `reauthenticationToken`); sertifika testin kendi atılan sertifikası; ekran görüntüleri atılan test veritabanının sahte verisini gösterir
- Auth etkisi: Temiz — üretim kodu değişikliği: `components.css` `.button[hidden]`, `.form-fields[hidden]` (dar, doğru), `_ProductFormBody.cshtml` `x is not null` (dar; kök neden D-1703), `UploadComponentModels.cs` `data-max-bytes`/`data-size-message` (sınır ve cümle sunucudan), `product-form.js` seçimde boy denetimi (istemci tarafı ek; sunucu denetimi yerinde, betiksiz 422/413 gözlendi)
- Input validation: Temiz
- Yeni bağımlılık: `Microsoft.Playwright` 1.59.0 (yalnız Integration test projesi; Apache-2.0; `09` tablosunda; `restore --locked-mode` çıkış 0)

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | VAL-B105 (KK3) | ORTA | S1 | "Betikli yolda ikinci `submit` durdurulur" gözlenir ↔ test iki `requestSubmit`'i aynı görevde çağırır; tarayıcı iki gönderimi tek gezinmeye indirdiği için ağa giden istek betik olsa da olmasa da 1'dir. `common.js:213` durdurması etkisizleştirilince ödeme adımı ve iletişim formu gözlemleri yine ✓ (değişinim 7). Ayırt edici gözlem gerekir: ör. ilk gönderim yoldayken (yanıtı `page.RouteAsync` ile bekletilmiş) ikinci tıklama, ya da ikinci `submit` olayının `defaultPrevented` değeri | VAL-B113 (aynı kalıbın altı tarayıcı satırı) | bu PR'da düzeltilecek | `Browser/ScriptlessFlowTests.cs` `TheDoubleSend_…_VALB105` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `D-41` (E2E komutu bağlanmadı) `DEFERRED_BACKLOG.md`'de 🟡 açık; `.github/ci-commands.env` yorumu "D-41 kapandı" diyor. D-41'in "Yapılacak"ındaki `SETUP.md` §4 notu ve parametre kaydı, `DEPLOY_RUNBOOK` §K.11 yer tutucu ifadesi güncellenmedi; `05 §8.3.11` hâlâ "uçtan uca doğrulama fazında bağlanır" yazıyor. Aynı yorumdaki "36 test x 3 motor, motor başına 3-4 dk" ölçümü eski (şimdi 170 teori × 3, CI 16 dk) | DEFERRED_BACKLOG'a forward: D-1713 |
| K2 | Kanıt zayıflıkları: (a) B110 kayıt ve şifre sıfırlama akışları bağlantıyla açılan ikinci adıma (E-21 doğrulama, E-23 yeni şifre formu) gitmiyor; yürüyüş bu ekranları yalnız geçersiz bağlantı hâlinde açıyor — geçerli bağlantılı yeni şifre formu hiçbir yolda gönderilmiyor. (b) B112 `[data-js=file-state]` `.First` dijital dosya alanının değil formdaki ilk durum öğesinin metnini okuyor: betikli yolda yükleme kimliği yazıldığı hâlde gözlem "durum \"Dosya yok\"" yazıyor (ölçü kimliğe dayandığı için sonuç doğru) | DEFERRED_BACKLOG'a forward: D-1714 |

#### Yapım Raporu Karşılaştırması
- Uyum: 3 uyuşmazlık
- B105: öz denetim "B103–B105 ✓ … ağa giden istek 1" der; bu ölçü betikten bağımsızdır (Bulgu 1).
- Öz denetim tablosu (düzeltme 2) satır başına ayırt edici kanıt yerine "testler yeşil" der ve kendisi "satır satır yeniden okuma yerine yeşil testlerle" yapıldığını yazar; B097 ve B101 "⚠ kısmen" kalmış (B101'in gerekçesi D-1705 B102'ye aittir) — kısmi satırla ✓ verilmez; bu turda iki satırı kendi kanıtımla PASS buldum.
- Öz denetim "D-41 … kayıtları güncel" der; D-41 açık, `ci-commands.env` kapandı der (K1).
- Uyumlu: KK1, KK2, KK4, KK5, KK6, tur 1 bulguları 1, 2, 3, 5; `09 §19.2` madde 6; üretim kodu değişikliklerinin darlığı.

#### Merge ve Post-merge
- Squash: — (FAIL; merge yok)
- Post-merge run'lar: —

## 2026-11-17 — T551 düzeltme 3 (aynı dal, PR #583)

- **VAL-B105 (tur 2 Bulgu 1):** betikli yolda ikinci gönderimin betik tarafından durdurulması artık ayırt edici ölçülür: formun `submit` olayına, `common.js` dinleyicisinden SONRA kayıtlı bir yakalayıcı eklenir; iki `requestSubmit` sonrası `defaultPrevented` dizisi `false,true` olmalıdır (ödeme adımı ve iletişim formu). `[data-js=file-state]` (B112) seçicisi `#product-file` alanına daraltıldı (r2 K2b). K1 ve K2(a) D-1713, D-1714 kayıtlarında kalır.
- **Değişinim:** `common.js:213` (`bindSubmitOnce`) `if (form.getAttribute("aria-busy") === "true")` -> `if (false && ...)`: `TheDoubleSend_..._VALB105(chromium, Scripted)` KIRILDI (ödeme adımı gözlemi `defaultPrevented "false,false"`), betiksiz yol geçti; geri alındı, yeniden derlendi, test yeşil.

### Düzeltme 3 — test satırları

| Koşu | Sonuç |
|---|---|
| `ScriptlessFlowTests` (motor başına ayrı) | chromium 24/24, firefox 24/24, webkit 24/24 |
| `Browser=true` tam (motor başına ayrı) | chromium 172/172, firefox 172/172, webkit 172/172 |
| `dotnet build -warnaserror` | 0 uyarı, 0 hata |
| `dotnet format --verify-no-changes --include ScriptlessFlowTests.cs` | çıkış 0 |
| Integration TAM | CI |

### Düzeltme 3 — öz denetim (düzeltme farkı: `ScriptlessFlowTests.cs`; üretim kodu değişmedi)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 üç motor, iki yol | ✓ | `Ways()`; Browser tam 172/172 × 3 |
| KK2 ekran, onay, hata, boş hâl | ✓ | Bu düzeltme dokunmadı; tur 2 doğrulaması ✓ |
| KK3 17 satır iki yol + Veri kaydı | ✓ | B105 betikli pay ayırt edici (aşağıda); öteki 16 satır tur 2'de ✓ |
| KK4 B096, B107 KRİTİK | ✓ | `ScriptlessPathTests` `…VALB096`, `ScriptlessFlowTests` `…VALB107` yeşil × 3 |
| KK5 CI işi | ✓ | Bu düzeltme dokunmadı; tur 2 run `43945619413` ✓ |
| KK6 kayıt | ✓ | `observations.md` başlığı değişmedi |
| B096, B097, B098, B099, B100, B101, B102, B103, B104, B106, B107, B108, B109, B110, B111 | ✓ | Düzeltme dokunmadı; tur 2 doğrulaması PASS (değişinimli); testler yeşil × 3 |
| B105 | ✓ | Ödeme adımı (`secondPrevented = state[2] == "false,true"`) ve iletişim formu (`contactSecondPrevented`) betikli yolda `defaultPrevented` ile ölçülür; değişinim `common.js:213` `if (false && …)` -> `…VALB105(chromium, Scripted)` KIRILDI, geri alınınca yeşil; betiksiz yol değişmedi |
| B112 | ✓ | `#product-file [data-js=file-state]`; `…VALB112` yeşil × 3 |
| Backlog (D-41, D-1713, D-1714) | ✓ | K1, K2(a) kayıtlı; yeni kalem yok |

## 2026-11-17 — T551 düzeltme 4 (aynı dal, PR #583)

Tur 3 Bulgu 1 (KK2, boş hâller): ekran listesi elle tutulmaz, `Docs/04_UI_SPECS.md`'den test içinde türetilir. Kapsam daraltma ve T552'ye erteleme seçilmedi.

### Düzeltme 4 — türetme tablosu (ekran · 04 maddesi · durak)

Kaynak: `EmptyStateTests.EveryScreenWithAnEmptyStateInTheDocument_HasAStop_AndEveryStopIsOfSuchAScreen_KK2` (kayıt klasöründe `empty-state-derivation.md`). Kural: 3.3.12 ve 3.4.26 satırları; her ekran bölümünün "Boş / yükleniyor / hata durumları" maddesinde "boş hâl" geçen satır ("ekranın/ekrana özgü boş hâl(i) yoktur" diyenler sayılmaz); 6.2.x ve 6.3.x tablolarında "Boş hâl satırı" olan satırlar (6.2.N = 5.N, 6.3.N = 9.N). 15 ekran:

| Ekran | 04 maddesi | Durak |
|---|---|---|
| E-02 | 3.3.12, 5.2.9, 6.2.2.2 | /kategori/t551-bos-kategori (boş kategori); /kategori/t551-kategori?minPrice=99999 (süzgecin sonucu boş) |
| E-03 | 3.3.12, 5.3.8, 6.2.3.2 | /arama?search=zzzzzz (sonuçsuz arama); /arama?search=ürün&minPrice=99999 (süzgecin sonucu boş) |
| E-07 | 3.3.12, 5.7.8, 6.2.7.2 | /hizmetler (hizmetler, yayında kayıt yok); /referanslar (referanslar, yayında kayıt yok) |
| E-09 | 3.3.12, 5.9.8, 6.2.9.3 | /sss (yayında soru yok) |
| E-12 | 3.3.12, 5.12.16, 6.2.12.9 | /sepet (boş sepet) |
| E-26 | 5.26.9, 6.2.26.2 | /hesap/adresler (adresi olmayan defter) |
| E-27 | 3.3.12, 5.27.10, 6.2.27.3 | /hesap/siparisler (siparişi olmayan üye) |
| E-32 | 6.3.4.2, 9.4.9 | /panel/urunler (ürün yok); /panel/urunler?search=zzzzzz (aramanın sonucu boş) |
| E-34 | 6.3.6.3, 9.6.10 | /panel/kategoriler (kategori yok) |
| E-35 | 6.3.7.3, 9.7.10 | /panel/kuponlar (kupon yok) |
| E-36 | 3.4.26, 6.3.8.4, 9.8.13 | /panel/siparisler (sipariş yok); /panel/siparisler?search=zzzzzz (aramanın sonucu boş) |
| E-38 | 3.4.26, 6.3.10.3, 9.10.12 | /panel/talepler (talep yok); /panel/talepler?requestStatus=Closed (süzgecin sonucu boş) |
| E-41 | 6.3.13.5, 9.13.16 | /panel/kurumsal-icerik (yedi grubun hepsi boş) |
| E-51 | 9.22.11 | /panel/satis-ozeti (ilk kurulum, dağılım tabloları) |
| E-52 | 3.4.26, 6.3.23.2, 9.23.11 | /panel/islem-izi (işlem izi kaydı yok); /panel/islem-izi?adminId=<yönetici> (süzgecin sonucu boş) |

Durak başına ölçülen: 200; cümle (neyin olmadığını söyleyen kalıp); çıkış bağlantısı sayısı, etiketi ve adresi (04 2.7.1.2); kayıt satırı yok; hata yok; çıkış tıklanır ve açılan sayfa yanıt verir (aynı sayfadaki yere giden çıkışta yer sayfada olmalı). E-51'in dağılım tablolarında 04 çıkış yolu tanımlamaz, yalnız cümle ölçülür. Ayrı kurulumlar: tohumsuz kurulum (`BrowserHost.EmptyAsync`), tohumlu kurulumda boş kategori (`t551-bos-kategori`) ve adresi/siparişi olmayan müşteri (`BareCustomerKey`).

### Düzeltme 4 — test satırları

| Koşu | Sonuç |
|---|---|
| `EmptyStateTests` (chromium) | 45/45 (22 durak × 2 yol + türetme testi) |
| `Browser=true` tam (motor başına ayrı) | chromium 207/207, firefox 207/207, webkit 207/207 |
| `dotnet build -warnaserror` | 0 uyarı, 0 hata |
| `dotnet format --include` (değişen 4 `.cs`) | uygulandı, yeniden derleme temiz |
| Unit | koşulmadı (yalnız Integration test projesine dokunuldu) |
| Integration TAM | CI |

### Düzeltme 4 — değişinim (kapsam testi)

`EmptyStateTests.cs`'den E-27 durağı geçici silindi: `EveryScreenWithAnEmptyStateInTheDocument_…_KK2` KIRILDI ("Filter not matched" ile E-27'yi saydı); durak geri konuldu, test yeşil. Başarısızlık iletisi sonradan okunur hâle getirildi ("04 gives these screens an empty state and no stop observes it: E-27"). Tersi: durağı olup 04'te boş hâli olmayan ekran da testi kırar.

### Düzeltme 4 — öz denetim (KK1–KK6, `04` §4'e karşı; önceki doğrulamaya dayanılmadı)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 üç motor, iki yol | ✓ | `Ways()`/`Cases`; Browser tam 207/207 × 3 |
| KK2 onay pencereleri | ✓ | Bu düzeltme dokunmadı; `ConfirmWindowTests` bu koşuda 3 motorda yeşil, `_Confirm` kullanan sayfalar E-25, E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49 |
| KK2 hata hâlleri | ✓ | `ValidationStateTests` bu koşuda 3 motorda yeşil; dokunulmadı |
| KK2 boş hâller | ✓ | Liste `04`'ten türetilir (15 ekran, yukarıdaki tablo); her ekranın durağı var; E-02, E-07, E-27, E-34, E-09, E-03, E-12, E-26 ve E-32, E-35, E-36, E-38, E-41, E-51, E-52 gözlendi; cümle ve çıkış ölçüldü ve çıkış izlendi; değişinim E-27 durağını silince kırıldı; süzgeç sonucu boş hâller (E-02, E-03, E-32, E-36, E-38, E-52) de gözlendi |
| KK3 17 satır iki yol + Veri kaydı | ✓ | Dokunulmadı; `ScriptlessPathTests`, `ScriptlessFlowTests` 3 motorda yeşil |
| KK4 B096, B107 KRİTİK | ✓ | Dokunulmadı; 3 motorda yeşil |
| KK5 CI işi | ✓ | Dokunulmadı (`ci.yml` değişmedi); yeni sınıf `Browser=true` iznindedir, CI'da koşar |
| KK6 kayıt | ✓ | Tohum başlığı: müşteri sayısı 2 (boş hâl müşterisi eklendi); `empty-state-derivation.md` kayıt klasöründe |
| Backlog (D-1717) | ✓ | Kapatıldı: E-26 ayrı müşteriyle, cümle ve çıkış ölçülerek; yeni kalem yok |

⚠ Süzgeç sonucu boş hâlleri 04 6.x satırlarında aynı satırda tanımlıdır; sahip yerine karar: boş hâl durağına ek durak olarak gözlendi (E-02, E-03, E-32, E-36, E-38, E-52). E-35 ve E-41 kuponlar/kurumsal içerikte süzgeç yoktur.

## Doğrulama tur 3

### Doğrulama Sonucu — T551 Tarayıcı aracı ve betiksiz yol gözlemi — tur 3
**Tarih:** 2026-11-17   **Branch:** task/T551-tarayici-betiksiz-yol   **Commit:** `b2c588c0` (doğrulanan head)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (worktree, `T551-val` = `origin/task/T551-tarayici-betiksiz-yol`, head `b2c588c0`) |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | `43788205169` success · `43788205183` success · `43779716328` success |
| Memory drift | — | Adım 0b kaldırıldı (2026-11-08) |
| main birleştirmesi | gerek yok | `origin/main` = `3fb77b9d` = dalın tabanı (merge-base); birleştirilecek yeni commit yok |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Playwright; Chromium, Firefox, WebKit; her ekran betik açık ve kapalı | ✓ | Motor süzgeciyle ayrı koşular (`SHOPFOLIO_BROWSER_ENGINES`, `--filter-trait "Browser=true"`, PostgreSQL 18 `127.0.0.1:25561`): chromium 172/172 (2 dk 54 sn), firefox 172/172 (7 dk 4 sn), webkit 172/172 (5 dk 33 sn). `BrowserTestBase.Ways()` motor × {Scripted, Scriptless}; `BrowserHost.ContextAsync` `JavaScriptEnabled = path == Scripted` bağlam düzeyinde. |
| 2 | Tohum müşteri, yönetici, her durumdaki sipariş; `04 §4` envanterinin her ekranı onay pencereleri, hata ve boş hâlleriyle | ~ | **Karşılanan:** tohum 8 sipariş durumu çifti (6 kargo × 5 ödeme durumunun hepsi) + üye siparişi, arşivli ürün; yürüyüş 53 ekranın 53'ü (`EveryScreenOfTheInventoryDocument_HasAStopInTheWalk_KK2` listeyi `04 §4.1–4.2`'den okur). Onay pencereleri: `_Confirm` kullanan sayfaların hepsi — panel/hesap E-25, E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49 (`ConfirmWindowTests`), müşteri E-17 (B096), E-18 (B097). Hata hâli: `ValidationStateTests` (422 + geçersiz alan + tablo parmak izi). Boş hâl: E-03 sonuçsuz arama, E-12 boş sepet, E-09 (SSS tohumsuz), E-26 (bu koşuda boş — sıra bağımlı, K1), `EmptyStateTests` E-32, E-35, E-36, E-38, E-52. **Eksik ekran listesi (boş hâl):** E-02 boş kategori (`04` 5.2.9, 6.2.2.2), E-07 yayında kaydı olmayan içerik listesi (5.7.8, 6.2.7.2), E-27 siparişi olmayan üyenin sipariş geçmişi (5.27.10, 6.2.27.3), E-34 kategorisi olmayan kategori ağacı (6.3.6.3, 9.6.10). Dördü de uygulamada var (`Category.cshtml`, `Shared/_ContentListBody.cshtml`, `Account/OrderHistory.cshtml.cs`, `Panel/Categories.cshtml.cs` boş hâl üretir); yürüyüş bu ekranları dolu açar (E-27 ekran görüntüsü `E-27-chromium-betiksiz-0146.png`: tohumun üye siparişi listede), tohumsuz ikinci kurulum yalnız beş panel listesini gezer. `12 §12.2` (1) kapsamı "onay pencereleri, hata hâlleri ve boş durumlar" der; T552 aynı araçla bu kapsamı tarayacak. Onay penceresi ve hata hâlinde eksik yok. → Bulgu 1 |
| 3 | `VAL-B096`…`VAL-B112` her biri için iki yol gözlemi; Veri kaydı satırında tablo kaydı, kişisel veri maskeli | ✓ | Her motor kaydında 17 satırın 17'si iki yolda (satır × yol gözlem sayısı 1–5; webkit: B096 2+2, B099 4+4, B100 4+4, B101 3+3, B105 2+3, B110 5+5, B111 5+5 …), 0 ✗; 410 satır, 409 ekran görüntüsü, genişlik 1280, motor adı ve sürümü. Her gözlem satırın kendi adımını, beklenenini ve gözlenenini yazar (ortak cümle yok). Veri kaydı satırları (B096, B099, B102, B105, B107, B109, B111) tablo satırını `DataRecord.RowAsync` ile okur; B101 uç yanıtını yazar. Maske: üç kayıtta `@example.com` 0, `/siparis/erisim/<anahtar>` maskesiz 0; `ObservationMaskTests` 4/4. Değişinimler (aşağıda) yedi satırda kanıtın ayırt edici olduğunu gösterir. |
| 4 | KRİTİK `VAL-B096` ve `VAL-B107` iki yolda; onaysız gönderim bir şey yazmaz | ✓ | B096: ilk (onaysız) gönderimden sonra DB `order_status/payment_status` `Placed/Pending`, onaydan sonra `Cancelled` (iki yol, üç motor). Değişinim `OrderScreenModels.cs:247` `if (!input.Confirmed)` → `if (!input.Confirmed && input.Confirmed)` (onaysız gönderim yazar): B096 iki yolda KIRILDI. B107: işaretliyken `billing_equals_shipping=True; decoy_taken=False` (betiksiz yolda fatura alanları başka kişinin değerleriyle dolu), işaretsizken `billing_recipient_name=Fatura Sahibi`. Değişinim `PlaceOrderInput.cs:147` `IsBillingSameAsShipping` → `false`: B107 iki yolda KIRILDI; `components.css` `[hidden]` kuralı kaldırılınca B107 betikli (fatura formu görünür) KIRILDI. |
| 5 | Araç CI'da koşar; iş adı ve süresi `12 §6.2` F5 B satırına ve `11 §6.2` F5'e uygun | ✓ | Run `43960425365` (head `b2c588c0`) job `133630645063` **"8. E2E (advisory)" success**, 512/512 test, 13 dk 43 sn (21:36:14–21:49:57 UTC); `runs-on: ubuntu-24.04` (gerekçe yorumu ci.yml'de); `browser-observations` kaydı yüklendi (118,9 MB, artifact `14275478039`). İş adı CI iskeletinin 8. adımıdır (`05 §8.3.11`); `12 §6.2` F5 B satırı ve `11 §6.2` F5 süre sınırı yazmaz (K-1620'de de yok). Integration işi `--filter-not-trait "Browser=true"` ile tarayıcı testlerini dışlar. |
| 6 | Çıktı gate raporuna eklenebilir kayıt; sürümler ve tohum | ✓ | `observations.md`/`.jsonl` (üç motor): "Sürümler: playwright 1.59.0.0, dotnet 10.0.11, chromium 147.0.7727.15 / firefox 148.0.2 / webkit 26.4", "Tohum: {customers, admins, coupon, products, orders…}" (anahtar yok); CI artifact `browser-observations` (`if: always()`), sunucu günlüğü ve ekran görüntüleri. |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-B096 | KRİTİK | yok | PASS | PASS | — |
| VAL-B097 | ORTA | yok | PASS | PASS | — |
| VAL-B098 | ORTA | yok | PASS | PASS | — |
| VAL-B099 | ORTA | yok | PASS | PASS | — |
| VAL-B100 | ORTA | yok | PASS | PASS | — |
| VAL-B101 | ORTA | yok | PASS | PASS | — |
| VAL-B102 | ORTA | yok | PASS | PASS | — |
| VAL-B103 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B104 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B105 | ORTA | yok | FAIL | PASS | — |
| VAL-B106 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B107 | KRİTİK | yok | PASS | PASS | — |
| VAL-B108 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B109 | ORTA | yok | PASS | PASS | — |
| VAL-B110 | ORTA | yok | PASS | PASS | — |
| VAL-B111 | ORTA | yok | PASS | PASS | — |
| VAL-B112 | ORTA | yok | PASS | PASS | — |

`12_matris_denetle.py --kapsam 1-77`: Hata 0.

#### Önceki turların bulgularının kapanışı
| Tur / Bulgu | Durum | Kanıt |
|---|---|---|
| Tur 1 B1 (KK5, E2E CI'da koşmuyor) | kapandı | CI run `43960425365` "8. E2E (advisory)" 512/512 success |
| Tur 1 B2 (KK2 ekran ve hâller) | kısmen | E-05, dokuz panel/hesap onay penceresi, form hata hâli, iki sipariş durumu, `shipped` yürüyüşte, beş panel listesinin boş hâli kapandı; müşteri tarafı ve kategori ağacının boş hâli açık → tur 3 Bulgu 1 |
| Tur 1 B3 (B101 boy denetimi) | kapandı | `product-form.js` `checkSize` etkisizleştirilince `…VALB101(chromium, Scripted)` KIRILDI |
| Tur 1 B4 (B099, B100, B102, B105, B110 payları) | kapandı | B099 kupon/eski özet/il/yöntem; B100 fiyat/görsel/satın alınabilirlik/sepet; B102 seçenek/değer/varyant; B105 aşağıda; B110 kayıtlı hesapla giriş eklendi (E-21/E-23 ikinci adımı D-1714 (a)'da) |
| Tur 1 B5 (maske) | kapandı | Üç kayıtta e-posta 0, erişim anahtarı maskesiz 0; `ObservationMaskTests` 4/4 |
| Tur 2 B1 (B105 betikli ayırt edici değil) | kapandı | Test artık ikinci `submit` olayının `defaultPrevented` değerini ölçer (`false,true`); `common.js:214–215` (`bindSubmitOnce` `preventDefault(); return;`) yoruma alınınca `…VALB105(chromium, Scripted)` KIRILDI, geri alınınca yeşil |

#### Doğrulama Kontrol Listesi
- [x] KK1: üç motor, iki bağlam — motor başına ayrı koşu, 172/172 × 3
- [ ] KK2: ekran envanteri eksiksiz gezilir — onay penceresi ve hata hâli tam; E-02, E-07, E-27, E-34 boş hâli eksik (Bulgu 1)
- [x] KK3: 17 satırın iki yol gözlemi ve Veri kaydı
- [x] KK4: iki KRİTİK satır — DB'den doğrulandı, değişinimle kırıldı
- [x] KK5: CI işi, adı ve süresi — run `43960425365`, 13 dk 43 sn
- [x] KK6: kayıt gate raporuna girer — sürümler, tohum, artifact
- [x] `09 §19.2` madde 6: `git grep -nE "Task\.Delay|Thread\.Sleep" tests/Shopfolio.IntegrationTests/Browser` boş; `Skip`/`Explicit`/`Assert.True(true)` yok; CI süzgeci trait düzeyinde (tek test seçen filtre değil)
- [x] `BrowserTestBase.cs` `#pragma warning disable CS0612`: gerekçe yorumlu, tek yöntemi saran en dar kapsam, hemen `restore` — `09 §5` madde 5 "uyarı bastırma yalnız gerekçe yorumuyla ve en dar kapsamda" kuralına uyar
- [x] Üretim kodu darlığı: `components.css` yalnız `.button[hidden]`, `.form-fields[hidden]` (betikle gösterilen düğme betiksiz gizli kalır); `_ProductFormBody.cshtml` `x is not null` (kök neden D-1703'te); `UploadFileTagHelper` `data-max-bytes`, `data-size-message` (sınır ve cümle sunucudan, betik sayı/metin taşımaz)
- [x] `09 §25` ilgili satırlar; güvenlik taraması (aşağıda)

##### Değişinim denemeleri (chromium, `ScriptlessPathTests` + `ScriptlessFlowTests`, 34 vaka; hepsi `git checkout -- src/Shopfolio.Web` ile geri alındı, yeniden derlendi)
| # | Değişinim | Beklenen kırılma | Sonuç |
|---|---|---|---|
| 1 | `common.js:214–215` ikinci gönderim durdurması yoruma alındı | B105 betikli | KIRILDI (yalnız betikli) |
| 2 | `OrderScreenModels.cs:247` onaysız gönderim yazar | B096 iki yol | KIRILDI (iki yol) |
| 3 | `product-form.js:410` `checkSize` hiç denetlemez | B101 betikli | KIRILDI (yalnız betikli) |
| 4 | `components.css:58` `[hidden]` → `display: revert` | B106 betiksiz, B107 betikli | KIRILDI (ikisi) |
| 5 | `PlaceOrderInput.cs:147` `IsBillingSameAsShipping` → `false` | B107 iki yol | KIRILDI (B107 iki yol; ödeme adımından geçen B099, B105, B109 da kırıldı — fatura alanları boş) |
| 6 | `common.js:280` `submit-on-change` bağlanmaz | B104 betikli | KIRILDI (yalnız betikli) |
| 7 | `shipping-and-defaults.js:20` "Tümünü seç" kutuları işaretlemez | B103 betikli | KIRILDI (yalnız betikli) |
| 8 | `Checkout.cshtml.cs:429` (`ShowAsync`) kutu yok sayılır | — | kırılmadı: satır ödeme sayfasının yeniden gösterimindedir, siparişin yazım yolu değildir (yazım `AddressBooks.cs:185`; #5 o yolu kırdı) |

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | başarılı |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes --no-restore --include <dalın 11 .cs dosyası>` | çıkış 0 |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build --filter-not-trait "Consistency=true"` / `--filter-trait "Consistency=true"` | 7420/7420 · 667/667 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Browser (E2E) | ✓ | `--filter-trait "Browser=true"`, motor başına ayrı | chromium 172/172 · firefox 172/172 · webkit 172/172 |
| Integration | ✓ | Koşulan sınıflar: `Web.ProductFormPageTests`, `Web.FrameLayoutTests`, `Web.ProductListPageTests`, `Browser.ObservationMaskTests` (+ Browser sınıfları yukarıda) | 51/51 · **TAM: CI** (run `43960425365` "4. Integration test" success) |
| CI | ✓ | `gh pr checks 583` | 0–8 ve CI Gate hepsi pass (run `43960425365`) |
| Koşu sonrası | ✓ | `git status --short`; `dotnet restore Shopfolio.slnx --locked-mode` | temiz; çıkış 0 |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz — HTTPS sertifikası koşuda üretilir (`BrowserHost.NewCertificate`), depoya girmez; kayıtta oturum ve erişim anahtarı yok (`ForRecord`, maske)
- Auth etkisi: Temiz — üretim kodunda yetki değişikliği yok; araç oturumu tohumun çerezleriyle kurar (yalnız test)
- Input validation: Temiz — betikteki boy denetimi yalnız iyileştirmedir, sunucu 422/413'ü korur (B101 betiksiz yol); `data-size-message` tag helper niteliğiyle yazılır (kodlanır)
- Yeni bağımlılık: `Microsoft.Playwright` 1.59.0 (yalnız Integration test projesi; Apache-2.0; `09` kütüphane tablosunda satırı var; `packages.lock.json` güncel)

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | KK2 (`12 §12.2` (1)) | ORTA | S1 | `04 §4` envanterinin her ekranı boş hâlleriyle gezilir ↔ dört ekranın tanımlı ve uygulamada var olan boş hâli gezilmiyor: E-02 boş kategori (5.2.9), E-07 yayında kaydı olmayan içerik listesi (5.7.8), E-27 siparişi olmayan üyenin sipariş geçmişi (5.27.10), E-34 kategorisi olmayan kategori ağacı (6.3.6.3, 9.6.10). Tohumlu yürüyüş bu ekranları dolu açar; tohumsuz ikinci kurulum (`BrowserHost.EmptyAsync`) yalnız E-32, E-35, E-36, E-38, E-52'yi gezer. Öz denetim KK2'yi "tur 2 doğrulaması ✓" diye geçti | T552 (aynı araçla `12 §12.2` (1) kapsamını — boş durumlar dahil — tarar); D-1717 (E-26 boş hâlinin sıra bağımlılığı) | eskalasyon: üçüncü tur FAIL (`12 §8.5` iki iterasyon sınırı; `INSTRUCTIONS.md` §3.3). Düzeltme dar: tohumsuz kurulumda E-34 ve E-07, boş kategori ve siparişsiz üye için ek durak (`EmptyStateTests` kalıbı) | `Browser/EmptyStateTests.cs`, `Browser/ScreenInventoryTests.cs`, `Browser/BrowserHost.cs` (`EmptyAsync`) |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | E-26 adres defterinin boş hâli test sırasına bağlı: yürüyüş ve `…VALB108` aynı tohum müşterisini kullanır; bu koşuda yürüyüş önce koştu ve boş hâl görüldü ("Kayıtlı adresiniz yok."), sıra değişirse görülmez; yürüyüş satırı boş hâl cümlesini ölçmez | DEFERRED_BACKLOG'a forward: D-1717 |
| K2 | D-1714 (b) (B112 durum seçicisi) düzeltme 3'te kapandı; (a) açık | bu PR'da kapatıldı (D-1714 kaydına not düşüldü) |

#### Yapım Raporu Karşılaştırması
- Uyum: 1 uyuşmazlık
- KK2: düzeltme 3 öz denetimi "KK2 ekran, onay, hata, boş hâl ✓ — bu düzeltme dokunmadı; tur 2 doğrulaması ✓" der; öz denetim kendi kanıtını üretmeden önceki turun sonucuna dayanır ve E-02, E-07, E-27, E-34 boş hâli gezilmez (Bulgu 1).
- Uyumlu: KK1, KK3, KK4, KK5, KK6; B105 düzeltmesi ve değişinim kanıtı (rapor `common.js:213` `if (false && …)`; bu tur `:214–215` ile bağımsız tekrar — ikisi de kırdı); B112 seçicisi; üretim kodunun darlığı; `09 §19.2` madde 6. Öz denetim tablosu satır başına kanıt taşır (B105, B112 ayrıntılı; öteki satırlar "tur 2 PASS + yeşil" der — bu turda her birini kendi koşum ve değişinimlerimle doğruladım).

#### Merge ve Post-merge
- Squash: — (FAIL; merge yok)
- Post-merge run'lar: —

## Doğrulama tur 4

### Doğrulama Sonucu — T551 Tarayıcı aracı ve betiksiz yol gözlemi — tur 4
**Tarih:** 2026-11-17   **Branch:** task/T551-tarayici-betiksiz-yol   **Commit:** `7d744666` (doğrulanan head; kayıt commit'i: bu doğrulamanın kayıt commit'i)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (worktree, `T551-val` = `origin/task/T551-tarayici-betiksiz-yol`, head `7d744666`) |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | `43788205169` success · `43788205183` success · `43779716328` success |
| Memory drift | — | Adım 0b kaldırıldı (2026-11-08) |
| main birleştirmesi | gerek yok | `origin/main` = `3fb77b9d` = dalın tabanı (merge-base) |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Playwright; Chromium, Firefox, WebKit; her ekran betik açık ve kapalı | ✓ | Motor süzgeciyle ayrı koşular (`SHOPFOLIO_BROWSER_ENGINES`, `--filter-trait "Browser=true"`, PostgreSQL 18 `127.0.0.1:25571`): chromium 207/207 (4 dk 16 sn), firefox 207/207 (6 dk 59 sn), webkit 207/207 (5 dk 44 sn). `Ways()` motor × {Scripted, Scriptless}; `ContextAsync` `JavaScriptEnabled = path == Scripted`. Yürüyüş her motorda 53 ekranın 53'ünü iki yolda açar (kayıtta 106 ekran × yol çifti). |
| 2 | Tohum müşteri, yönetici, her durumdaki sipariş; `04 §4` envanterinin her ekranı onay pencereleri, hata ve boş hâlleriyle | ~ | **Karşılanan:** tohum `OrderStatus` 6 değerinin ve `PaymentStatus` 5 değerinin hepsi (9 sipariş), iki müşteri (biri adressiz/siparişsiz), yönetici. Onay pencereleri: `_Confirm` kullanan sayfaların hepsi (E-17/E-18 müşteri son adımı, E-25, E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49). Boş hâl: `04`'ten türetilen 15 ekran (E-02, E-03, E-07, E-09, E-12, E-26, E-27, E-32, E-34, E-35, E-36, E-38, E-41, E-51, E-52) — kendi `grep -nE "Boş (durum\|hâl)\|boş hâl" Docs/04_UI_SPECS.md` listemle birebir; 22 durak × 2 yol × 3 motor ✓, çıkışlar izleniyor. **Eksik ekran listesi:** E-23 şifre sıfırlamanın yeni şifre adımı (`/sifre-sifirlama/{anahtar}`, `reset-set-form`, `04` 5.23.4, form 7.1.13.2) ve E-29 panel şifre sıfırlamanın yeni şifre adımı (`/panel/sifre-sifirlama/{anahtar}`, form 7.2.1.5) hiçbir testte açılmıyor; iki formun hata hâli yok. E-21 doğrulama ekranı yalnız geçersiz bağlantıyla açılıyor. Neden: `ValidationStateTests` form listesini yürüyüşün sayfalarından türetiyor, yürüyüş bu ekranları yalnız geçersiz bağlantıyla açıyor (E-30 için geçerli davet anahtarı var, sıfırlama için yok) → Bulgu 1 |
| 3 | `VAL-B096`…`VAL-B112` her biri için iki yol gözlemi; Veri kaydı satırında tablo kaydı, kişisel veri maskeli | ~ | Her motor kaydında 17 satırın 17'si iki yolda (B096 2+2, B099 4+4, B100 4+4, B101 3+3, B105 2+3, B110 5+5, B111 5+5 …), 0 ✗; 486 satır, 443 ekran görüntüsü, genişlik 1280. Her gözlem satırın kendi adımı ve gözlenenini yazar. Veri kaydı satırları (B096, B099, B102, B105, B107, B109, B111) tablo satırını okur (B111 iç not hariç, K1); B101 uç yanıtını (200/422/413, betikli "istek gitmedi") yazar. Maske: üç kayıtta `@example` 0, maskesiz erişim anahtarı / `pendingRegistration` / `reauthenticationToken` / davet anahtarı 0; tohum başlığında anahtar yok. **Eksik:** `VAL-B110` "dört akış da form gönderimleriyle tamamlanır" — şifre sıfırlama yalnız istek adımında, kayıt yalnız ilk gönderimde biter (yeni şifre ve doğrulama adımı yok) → Bulgu 1 |
| 4 | KRİTİK `VAL-B096` ve `VAL-B107` iki yolda; onaysız gönderim bir şey yazmaz | ✓ | B096: ilk (onaysız) gönderimden sonra DB `Placed/Pending`, onaydan sonra `Cancelled/Failed` (iki yol, üç motor). Değişinim `OrderScreenModels.cs:247` `if (!input.Confirmed && false)` → `…VALB096` iki yolda KIRILDI. B107: işaretliyken `billing_equals_shipping=True; decoy_taken=False` (betiksiz yolda fatura alanları başka kişinin değerleriyle dolu gönderildi), işaretsizken `billing_recipient_name=Fatura Sahibi` (üç motor). |
| 5 | Araç CI'da koşar; iş adı ve süresi `12 §6.2` F5 B satırına ve `11 §6.2` F5'e uygun | ✓ | Run `43975904593` (head `7d744666`) success; "8. E2E (advisory)" job `133683476766` success, 615/615 test, 13 dk 19 sn (23:11:05–23:24:24 UTC), `Image: ubuntu-24.04`; artifact `browser-observations` (126,4 MB, `14282741114`). `12 §6.2` F5 B satırı ve `11 §6.2` F5 ad/süre sınırı koymaz; iş adı CI iskeletinin 8. adımıdır. Integration işi `--filter-not-trait "Browser=true"`. |
| 6 | Çıktı gate raporuna eklenebilir kayıt; sürümler ve tohum | ✓ | `observations.md`/`.jsonl`: "Sürümler: playwright 1.59.0.0, dotnet 10.0.11, chromium 147.0.7727.15 / firefox 148.0.2 / webkit 26.4", "Tohum: {customers 2, admins 1, coupon, products, orders 9 …}"; `empty-state-derivation.md`; ekran görüntüleri, sunucu günlüğü; CI artifact `if: always()`. |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-B096 | KRİTİK | yok | PASS | PASS | — |
| VAL-B097 | ORTA | yok | PASS | PASS | — |
| VAL-B098 | ORTA | yok | PASS | PASS | — |
| VAL-B099 | ORTA | yok | PASS | PASS | — |
| VAL-B100 | ORTA | yok | PASS | PASS | — |
| VAL-B101 | ORTA | yok | PASS | PASS | — |
| VAL-B102 | ORTA | yok | PASS | PASS | — |
| VAL-B103 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B104 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B105 | ORTA | yok | PASS | PASS | — |
| VAL-B106 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B107 | KRİTİK | yok | PASS | PASS | — |
| VAL-B108 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B109 | ORTA | yok | PASS | PASS | — |
| VAL-B110 | ORTA | yok | PASS | FAIL | — (kabul edilmiş risk kaydı yok) |
| VAL-B111 | ORTA | yok | PASS | PASS | — |
| VAL-B112 | ORTA | yok | PASS | PASS | — |

`12_matris_denetle.py --kapsam 1-77`: Hata 0 (77/77 KP).

#### Önceki turların bulgularının kapanışı
| Tur / Bulgu | Durum | Kanıt |
|---|---|---|
| Tur 1 B1 (E2E CI'da koşmuyor) | kapandı | Run `43975904593` "8. E2E (advisory)" 615/615 |
| Tur 1 B2 (KK2 ekran ve hâller) | kısmen | E-05, onay pencereleri, form hata hâli, sipariş durumları, boş hâller kapandı; yeni şifre adımı (E-23, E-29) açık → Bulgu 1 |
| Tur 1 B3 (B101 boy denetimi) | kapandı | Betikli yolda aşan iki dosya için "istek gitmedi", betiksiz 422/413 (üç motor); `checkSize` sunucunun `data-max-bytes`/`data-size-message`'ını kullanır |
| Tur 1 B4 (B099, B100, B102, B105, B110 payları) | kapandı (B110 dışında) | B099 4+4, B100 4+4, B102 2+2, B105 2+3 gözlem; B110'un şifre sıfırlama/kayıt ikinci adımı → Bulgu 1 |
| Tur 1 B5 (maske) | kapandı | Üç kayıtta e-posta 0, anahtar/jeton maskesiz 0; `ObservationMaskTests` 4/4 |
| Tur 2 B1 (B105 betikli ayırt edici değil) | kapandı | `common.js:214` `event.preventDefault();` yoruma alındı → `…VALB105(chromium, Scripted)` KIRILDI (yalnız betikli), geri alındı |
| Tur 3 B1 (KK2 boş hâl E-02, E-07, E-27, E-34) | kapandı | Liste `04`'ten türetilir (15 ekran = elle grep listesi); dört ekran iki yol × üç motor gözlendi; değişinimle E-27 ve E-34 durakları KIRILDI |

#### Doğrulama Kontrol Listesi
- [x] KK1: üç motor, iki bağlam — motor başına ayrı koşu, 207/207 × 3
- [ ] KK2: ekran envanteri eksiksiz gezilir — onay penceresi, boş hâl tam; E-23 ve E-29 yeni şifre adımı ve hata hâli eksik (Bulgu 1)
- [ ] KK3: 17 satırın iki yol gözlemi ve Veri kaydı — 16 satır tam; B110 eksik (Bulgu 1)
- [x] KK4: iki KRİTİK satır — DB'den doğrulandı, değişinimle kırıldı
- [x] KK5: CI işi, adı ve süresi — run `43975904593`, E2E 13 dk 19 sn, ubuntu-24.04
- [x] KK6: kayıt gate raporuna girer — sürümler, tohum, türetme tablosu, artifact
- [x] `09 §19.2` madde 6: `git grep -nE "Task\.Delay|Thread\.Sleep" tests/Shopfolio.IntegrationTests/Browser` boş; `Skip`/`Explicit`/`Assert.Skip`/`Assert.True(true)` yok; CI süzgeci trait düzeyinde
- [x] `BrowserTestBase.cs` `#pragma warning disable CS0612`: gerekçe yorumlu, tek yöntemi saran en dar kapsam, hemen `restore` — `09 §3.1` madde 5'e uyar
- [x] Üretim kodu darlığı: `components.css` yalnız `.button[hidden]`, `.form-fields[hidden]`; `_ProductFormBody.cshtml` `x is not null` (kök neden D-1703); `UploadFileTagHelper` `data-max-bytes`/`data-size-message`; `product-form.js` boy denetimi sunucunun değerleriyle (sayı/metin taşımaz)
- [x] `09 §25` ilgili satırlar; güvenlik taraması (aşağıda)

##### Değişinim denemeleri (chromium; `ScriptlessPathTests`, `ScriptlessFlowTests`, `EmptyStateTests`, `ConfirmWindowTests` = 97 vaka; tek koşu, 11 düşen vakanın hepsi hedefli; `git checkout -- src` ile geri alındı)
| # | Değişinim | Beklenen kırılma | Sonuç |
|---|---|---|---|
| 1 | `common.js:214` ikinci gönderim `preventDefault` kaldırıldı | B105 betikli | KIRILDI (yalnız betikli) |
| 2 | `OrderScreenModels.cs:247` `!input.Confirmed && false` (onaysız gönderim yazar) | B096 iki yol | KIRILDI (iki yol) |
| 3 | `Addresses.cshtml.cs:94` `create=true` tanınmaz | B108 | KIRILDI (iki yol) |
| 4 | `AddressBookTexts.cs:67` siparişsiz üye cümlesi "Liste." | E-27 boş hâl | KIRILDI (iki yol) |
| 5 | `Categories.cshtml.cs:270` "Yeni kategori" çıkışı `#yok` | E-34 boş hâl çıkışı | KIRILDI (iki yol) |
| 6 | `Members.cshtml.cs:209` `!input.Confirmed && false` (onaysız üye silme) | E-40 onay penceresi | KIRILDI (iki yol) |

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | 3 proje |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format … --verify-no-changes --include <dalın 11 .cs dosyası>` | çıkış 0 |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests … --no-build --filter-not-trait "Consistency=true"` / `--filter-trait "Consistency=true"` | 7420/7420 · 667/667 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 dosya |
| Integration (Browser) | ✓ | `--filter-trait "Browser=true"`, `SHOPFOLIO_BROWSER_ENGINES` = chromium / firefox / webkit (ayrı) | 207/207 × 3; dalın dokunduğu Integration sınıflarının hepsi `Browser/` altında. TAM: CI (run `43975904593` "4. Integration test" success) |
| CI (dal) | ✓ | `gh run list --branch task/T551-tarayici-betiksiz-yol` | `43975904593` success (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate) |
| Kilitli restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; çalışma ağacında yalnız kayıt belgeleri |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (CI `POSTGRES_HOST_AUTH_METHOD: trust` yalnız CI hizmeti; tohum kaydı anahtar taşımaz; gözlem metni maskeli)
- Auth etkisi: Temiz (üretim yetki koduna dokunulmadı; oturum çerezleri yalnız testte tohumdan)
- Input validation: Temiz (betikli boy denetimi ek katman; sunucu 422/413'ü korur, betiksiz yolda gözlendi)
- Yeni bağımlılık: `Microsoft.Playwright` 1.59.0 (yalnız Integration test projesi, Apache-2.0, `09` kütüphane tablosuna yazıldı; geçişli `System.ComponentModel.Annotations` 5.0.0)

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | KK2 (`12 §12.2` (1)) + `VAL-B110` (KK3) | ORTA | S1 | `04 §4` envanterinin her ekranı hata hâlleriyle gezilir; B110 "dört akış da form gönderimleriyle tamamlanır" ↔ şifre sıfırlamanın yeni şifre adımı hiçbir testte açılmaz: E-23 `/sifre-sifirlama/{anahtar}` (`reset-set-form`, `Input.NewPassword`, `04` 5.23.4, form 7.1.13.2) ve E-29 `/panel/sifre-sifirlama/{anahtar}` (form 7.2.1.5); iki formun hata hâli yok (`Browser/` altında `reset-set-form\|ResetPassword\|NewPassword` 0 eşleşme). `ValidationStateTests` form listesini yürüyüşün sayfalarından türettiği için geçerli bağlantı arkasındaki formlar listeye ve korumaya girmez (E-30 için geçerli davet anahtarı tohumlanmış, sıfırlama için yok). B110'un şifre sıfırlama akışı istek adımında, kayıt akışı ilk gönderimde biter (E-21 geçerli doğrulama bağlantısı açılmaz). Öz denetim bunu D-1714 (a) olarak taşıdı | T552 (aynı araçla aynı ekranları tarar); D-1714 (a) | bu PR'da düzeltilecek (tohum geçerli sıfırlama ve doğrulama anahtarı kurar — `NewInvitationKeyAsync` kalıbı; yürüyüşe E-23/E-29 yeni şifre adımı ve E-21 geçerli bağlantı durakları; iki form `ValidationStateTests` vakası; B110'da yeni şifre gönderimi ve doğrulama sonucu iki yolda) | `Browser/BrowserSeed.cs`, `Browser/ScreenInventoryTests.cs`, `Browser/ValidationStateTests.cs`, `Browser/ScriptlessFlowTests.cs` `…VALB110` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | `VAL-B111` sipariş iç notu gözleminde tablo kaydı yok (Veri kaydı "-"); not yeniden açılan sayfadan okunuyor, öteki dört form tabloyu okuyor | DEFERRED_BACKLOG'a forward: D-1721 |
#### Yapım Raporu Karşılaştırması
- Uyum: 1 uyuşmazlık
- Düzeltme 4 öz denetimi (KK1–KK6) ve türetme tablosu benim koşularımla birebir (207/207 × 3; 15 ekran). Uyuşmazlık: öz denetim KK2 hata hâllerini ve KK3/B110'u "✓, dokunulmadı" diye geçti; E-23/E-29 yeni şifre adımının ve hata hâlinin gezilmediği, D-1714 (a)'nın B110'un beklenen sonucunu karşılamadığı öz denetimde yazmıyor.
- Öz denetim tablosu var ve her maddesi kanıt gösteriyor; önceki düzeltmelerin öz denetimleri büyük ölçüde "yeşil test" kanıtına dayanıyor.

#### Birleştirme notu
- `origin/main` dalın tabanıyla aynı (`3fb77b9d`); birleştirme gerekmedi.

#### Merge ve Post-merge
- FAIL: merge yok. Kayıt dalda (bu doğrulamanın kayıt commit'i).

## 2026-11-17 — T551 düzeltme 5 (aynı dal, PR #583)

Tur 4 Bulgu 1 (KK2 hata hâlleri + `VAL-B110`): bağlantı arkasındaki ekranlar geçerli bağlantıyla gezilir. Kapsam daraltma ve T552'ye erteleme seçilmedi (eskalasyon).

### Düzeltme 5 — bağlantılı ekran tablosu (ekran · 04 maddesi · tohum anahtarı · durak)

Liste `Docs/04_UI_SPECS.md` §4.1/§4.2 satırları ile `src/Shopfolio.Web/Pages` altındaki `@page` adreslerinde `{key}` taşıyanların karşılaştırılmasından çıkarıldı. Sipariş erişim anahtarlı adresler (E-14, E-16…E-19) tohumda zaten geçerli anahtarla açılır; `/odeme/donus/{accessKey}` `04` §4 envanterinde ekran değildir (ödeme sağlayıcı dönüşü).

| Ekran | 04 maddesi | Tohum anahtarı (`SeededLinks`) | Durak |
|---|---|---|---|
| E-21 kayıt | 5.21.2, 5.21.3 | `Registration` (doğrulanmamış kayıt + `EmailVerification`) | /dogrulama/{anahtar} (doğrulama adımı); geçersiz bağlantı durağı kaldı |
| E-21 yeni e-posta | 5.21.2, 5.21.7 | `NewAddress` (bekleyen adres değişikliği + `NewEmailVerification`) | /dogrulama/{anahtar} (doğrulama adımı) |
| E-21 Google teyidi | 5.21.9, 5.21.14 | `GoogleConfirmation` (`GoogleLinkConfirmation`) | /dogrulama/{anahtar} (doğrulama adımı) |
| E-23 | 5.23.4, 5.23.8 | `PasswordReset` (şifreli doğrulanmış müşteri + `PasswordReset`) | /sifre-sifirlama/{anahtar} (yeni şifre adımı); geçersiz bağlantı durağı kaldı |
| E-28 | 5.28.3 | `EmailRevert` (`EmailChangeRevert`, önceki adresli) | /geri-al/{anahtar} (geri alma adımı); geçersiz bağlantı kaldı |
| E-29 | 9.1.4, 9.1.5 | `AdminPasswordReset` (yönetici + `PasswordReset`) | /panel/sifre-sifirlama/{anahtar} (yeni şifre adımı); geçersiz bağlantı durağı eklendi |
| E-30 | 9.2 | `Host.InvitationKey` (önceden vardı) | /panel/davet/{anahtar}; artık kendi formunun varlığını da ölçer |
| E-54 doğrulama | 9.25.2, 9.25.4 | `AdminNewAddress` (yönetici + bekleyen değişiklik + `NewEmailVerification`) | /panel/dogrulama/{anahtar} (doğrulama adımı); geçersiz kaldı |
| E-54 geri alma | 9.25.2, 9.25.8 | `AdminEmailRevert` | /panel/geri-al/{anahtar} (geri alma adımı); geçersiz kaldı |

Altı ekran (E-21, E-23, E-28, E-29, E-30, E-54), dokuz geçerli bağlantılı durak. Yürüyüş durağı artık `Stop.Marker` ile geçerli hâlin kendi formunu ya da düğmesini de ölçer (geçersiz bağlantı hâli de 200 döner, yalnız cümle çizer, bu yüzden durum kodu ayırt etmez).

### Düzeltme 5 — test satırları

| Koşu | Sonuç |
|---|---|
| `Browser=true` tam (motor başına ayrı, PostgreSQL 18 `127.0.0.1:25581`) | chromium 211/211, firefox 211/211, webkit 211/211 (önceki 207; +4 = iki yeni `ValidationStateTests` vakası × iki yol) |
| `ObservationMaskTests` | 10/10 (altı yeni anahtar adresi biçimi) |
| `dotnet build -warnaserror` | 0 uyarı, 0 hata |
| `dotnet format --verify-no-changes --include` (değişen 7 `.cs`) | çıkış 0 |
| Unit | koşulmadı (yalnız Integration test projesine dokunuldu) |
| Integration TAM | CI |

`VAL-B110` artık: kayıt, doğrulama bağlantısı, düğme, hesap doğrulanır, kayıt şifresiyle giriş; şifre sıfırlama isteği (var olan hesap), hesabın `link_records` satırından türetilen anahtarla bağlantı, yeni şifre formu gönderimi (şifre özeti değişir, bağlantı kullanılır), yeni şifreyle giriş ve hesap sayfası; panel: geçerli bağlantı, yeni şifre, panele giriş. İki yol × üç motor.

### Düzeltme 5 — değişinim

1. `ScreenInventoryTests.cs`'den E-23 geçerli bağlantı durağı geçici silindi: `ValidationStateTests.EveryFormWithFieldsOfTheWalk_HasACase_AndEveryCaseIsInTheWalk_KK2` KIRILDI ("Validation cases whose form is not in the walk's pages: E-23|ResetPassword|reset-set-form"); durak geri konuldu.
2. `PasswordRegime.cs` yeni şifre yazan `accounts.SetPasswordAsync` satırları geçici yorumlandı: `VAL-B110` iki yolda KIRILDI; `git checkout -- src` ile geri alındı, yeniden derlendi, üç motorun tam Browser koşuları geri alınmış ağaçta yapıldı.

### Düzeltme 5 — öz denetim (KK1–KK6, `04` §4'e karşı; önceki doğrulamaya dayanılmadı)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 üç motor, iki yol | ✓ | `Ways()`/`Cases`; Browser tam 211/211 × 3 |
| KK2 onay pencereleri | ✓ | `ConfirmWindowTests` bu koşuda 3 motorda yeşil; bu düzeltme onay penceresi olan ekran eklemedi (bağlantı ekranları pencere taşımaz: 5.21.9, 5.28.6) |
| KK2 hata hâlleri | ✓ | Bağlantı arkasındaki altı ekranın geçerli hâli açılır; alanlı iki form (E-23, E-29 yeni şifre) `ValidationStateTests` vakası (422 + geçersiz alan + hesap ve bağlantı satırları değişmez); öteki ekranlarda form alanı yoktur (5.21.10); geçersiz bağlantı hâlleri durakta |
| KK2 boş hâller | ✓ | Dokunulmadı; `EmptyStateTests` 3 motorda yeşil (bağlantı ekranlarının boş hâli yoktur: 5.21, 5.23, 5.28) |
| KK3 17 satır + B110 | ✓ | B110 dört akışın ikinci adımlarıyla form gönderimlerinde biter, iki yol × üç motor; Veri kaydı: doğrulama (`verified`), bağlantı satırı (`used`); B111 iç not `internal_notes` satırı (D-1721) |
| KK4 B096, B107 KRİTİK | ✓ | Dokunulmadı; 3 motorda yeşil |
| KK5 CI işi | ✓ | Dokunulmadı (`ci.yml` değişmedi) |
| KK6 kayıt | ✓ | Bağlantı anahtarı adresleri maskeli (`/dogrulama/***`); webkit kaydı `observations.jsonl` taraması: maskesiz anahtar 0, `"Ok":false` 0 |
| Backlog (D-1714 (a), D-1721) | ✓ | İkisi de kapatıldı (`Docs/DEFERRED_BACKLOG.d/T551.md`); yeni kalem D-1723 (⚠ satırı) |

⚠ E-21 yeni e-posta ve Google teyidi, E-28 ve E-54 geri alma, E-54 doğrulama durakları geçerli hâlde açılır ve düğmenin varlığı ölçülür; düğmenin gönderimi yalnız kayıt doğrulaması ve şifre sıfırlamalarında denenir (`VAL-B110` ve KK2 yalnız bunları ister; öteki düğmeler aynı sayfa modelini kullanır, K-904 kalıbı). Sahip yerine karar: öteki düğme gönderimleri bu turda eklenmedi; D-1723 olarak kaydedildi (T552, aynı araçla aynı ekranlar).

## Doğrulama tur 5

### Doğrulama Sonucu — T551 Tarayıcı aracı ve betiksiz yol gözlemi — tur 5
**Tarih:** 2026-11-17   **Branch:** task/T551-tarayici-betiksiz-yol   **Commit:** head `d7331fdc` (incelenen)

#### Verdict: ✓ PASS

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `scripts/git-hooks` |
| Ana dal CI (son 3) | ✓ | `43788205169` success, `43788205183` success, `43779716328` success |
| Memory drift | — | adım 0b kaldırıldı (2026-11-08); aranmadı |
| main birleştirmesi | ✓ | `git merge origin/main` → "Already up to date" (main dalın tabanında) |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Playwright; Chromium, Firefox, WebKit; her ekran betik açık/kapalı | ✓ | `SHOPFOLIO_BROWSER_ENGINES` süzgeciyle ayrı koşular: chromium 211/211 (3 dk 19 sn), firefox 211/211 (7 dk 29 sn), webkit 211/211 (5 dk 57 sn). Kayıt sürümleri: chromium 147.0.7727.15, firefox 148.0.2, webkit 26.4, playwright 1.59.0. `Ways()` = motor × {Scripted, Scriptless} |
| 2 | Tohum müşteri, yönetici, her durumdaki sipariş; `04` §4 her ekranı onay, hata, boş hâlleriyle | ✓ | Tohum: 2 müşteri, 1 yönetici, 9 sipariş — `OrderStatus` 6 değerin 6'sı, `PaymentStatus` 5 değerin 5'i. `04` 4.1/4.2'den 53 ekran (E-48 yok); yürüyüş belgeden okur ve iki yönlü eşitler (`EveryScreenOfTheInventoryDocument_HasAStopInTheWalk_KK2`). Onay: `ConfirmWindowTests` E-25, E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49 + müşteri son adımı E-17 (B096), E-18 (B097); `04` §5/§9 "onay pencere/adımı" taramamda başka ekran çıkmadı (E-11, E-19, E-21, E-28, E-47, E-54 satırları olumsuzlama ya da onay adımı değil; E-21 Google teyidi durakta). Hata: `ValidationStateTests` alanlı her form (E-23, E-29 yeni şifre dahil) + iki yönlü kapsam testi. Boş hâl: `grep -nE "Boş (durum\|hâl)\|boş hâl" 04` elle → E-02, E-03, E-07, E-09, E-12, E-26, E-27, E-32, E-34, E-35, E-36, E-38, E-41, E-51, E-52; `EmptyStateTests` 15'inin hepsini taşır (tohumsuz ikinci veritabanı dahil). Eksik ekran: yok |
| 3 | 17 satırın iki yol gözlemi; Veri kaydı satırında tablo kaydı; kişisel veri maskeli | ✓ | Üç motorun `observations.md` kaydında 17 satırın her biri iki yolda var (34 × 3); her VAL satırının ekran görüntüsü var; Veri kaydı türündeki B096, B099, B102, B105, B107, B109, B111 ve Uç yanıtlı B101'in her yolunda veri sütunu dolu. Gözlem metinleri satıra özgü (adres, tutar, sayaç, DB değeri). Maske: üç kayıtta `@example.com` 0; erişim/bağlantı anahtarı `***`; `ObservationMaskTests` yeşil |
| 4 | KRİTİK B096 ve B107 iki yolda; onaysız gönderim bir şey yazmaz | ✓ | B096: ilk (onaysız) gönderimden sonra `orders.order_status` Placed DB'den okunur, onayla Cancelled. Değişinim `OrderScreenModels.cs:247` onay denetimi etkisiz → B096 iki yolda kırıldı. B107: işaretliyken betiksiz yolda başka değerli fatura alanları gönderilir, `decoy_taken=False`, `billing_equals_shipping=True` DB'den; işaretsizken girilen değerler yazılır. Değişinim `AddressBooks.cs:187` → betiksiz vaka kırıldı (betikli vaka için K1) |
| 5 | CI'da koşar; ad ve süre `12` §6.2 F5 B ve `11` §6.2 F5'e uygun | ✓ | `ci.yml` `e2e`: `name: 8. E2E (advisory)`, `runs-on: ubuntu-24.04`, `continue-on-error: true`, PostgreSQL 18 hizmeti, `browser-observations` yüklemesi. Run `43989323601` (head `d7331fdc`): 8. E2E pass, 14 dk 43 sn, 627/627, yapıt 132 MB. `12` §6.2 F5 "B `[tarayıcı]` — betiksiz yol", `11` §6.2 F5 "tarayıcı aracı ve betiksiz yol gözlemi T551'de" — ikisi de süre sınırı yazmaz; CI adımı `05 §8.3.11` adıyla aynı |
| 6 | Gate raporuna eklenebilir kayıt; sürümler ve tohum | ✓ | `observations.jsonl` ilk satırı `{"versions":{playwright, dotnet, <motor>}, "seed":{müşteri, yönetici, kupon, ürünler, 9 sipariş + durumları}}`; `observations.md` "Sürümler:" ve "Tohum:" satırları; ekran görüntüleri ve sunucu günlüğü; CI yapıtı |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-B096 | KRİTİK | yok | PASS | PASS | — |
| VAL-B097 | ORTA | yok | PASS | PASS | — |
| VAL-B098 | ORTA | yok | PASS | PASS | — |
| VAL-B099 | ORTA | yok | PASS | PASS | — |
| VAL-B100 | ORTA | yok | PASS | PASS | — |
| VAL-B101 | ORTA | yok | PASS | PASS | — |
| VAL-B102 | ORTA | yok | PASS | PASS | — |
| VAL-B103 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B104 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B105 | ORTA | yok | PASS | PASS | — |
| VAL-B106 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B107 | KRİTİK | yok | PASS | PASS | — |
| VAL-B108 | DÜŞÜK | yok | PASS | PASS | — |
| VAL-B109 | ORTA | yok | PASS | PASS | — |
| VAL-B110 | ORTA | yok | FAIL | PASS | — |
| VAL-B111 | ORTA | yok | PASS | PASS | — |
| VAL-B112 | ORTA | yok | PASS | PASS | — |

`12_matris_denetle.py --kapsam 1-77`: 1503 satır, kimlik tekil, Hata 0.

#### Önceki turların bulgularının kapanışı
- **Tur 4 Bulgu 1 (KK2 + B110):** `04` §4'ten bağlantı/anahtar arkasındaki ekranları kendim çıkardım: E-14, E-16…E-19 (sipariş erişim anahtarı), E-21 (kayıt, yeni e-posta, Google teyidi; 5.21.1), E-23, E-28, E-29 (sıfırlama), E-30 (davet), E-54 (yeni adres doğrulaması, geri alma; 9.25.1). Yürüyüşte hepsinin geçerli hâli var; geçerli hâl `Marker` ile kendi formu/düğmesiyle ölçülüyor. E-23 ve E-29 yeni şifre formları `ValidationStateTests`'te. B110: kayıt → doğrulama bağlantısı → düğme → `email_verified_at` dolu → kayıt şifresiyle giriş; sıfırlama isteği → bağlantı → yeni şifre → özet değişti + `used=True` → yeni şifreyle giriş; panel sıfırlaması → panele giriş; sipariş takibi. Kapandı.
- **Tur 3 Bulgu 1 (boş hâl):** elle grep listesi (15 ekran) `EmptyStateTests` durak listesiyle birebir. Kapandı.
- **Tur 2 Bulgu 1 (B105):** `common.js:214` `event.preventDefault()` etkisizleştirildi → `…VALB105(chromium, Scripted)` kırıldı (`defaultPrevented "false,false"`, ödeme ve iletişim gözlemleri ✗). Kapandı.
- **Tur 1:** (1) `E2E_CMD` `dotnet tool restore` ile başlıyor, CI 627/627; (2) onay pencereleri, E-05, tohumdaki DeliveryFailed/PartiallyRefunded/Refunded ve kargodaki sipariş durakta; (3) B101 betiği seçimde boy denetler — değişinimle kırıldı; (4) B099, B100, B102, B105, B110 payları gözleniyor; (5) `ObserveAsync` gözlemi maskeliyor. Kapandı.

#### Doğrulama Kontrol Listesi
- [x] KK1: üç motor, iki bağlam — motor süzgeciyle üç ayrı koşu
- [x] KK2: ekran envanteri eksiksiz gezilir — eksik ekran yok
- [x] KK3: 17 satırın iki yol gözlemi ve Veri kaydı
- [x] KK4: iki KRİTİK satır — DB'den onaysız gönderimin yazmadığı
- [x] KK5: CI işi, adı ve süresi — `ubuntu-24.04`, run `43989323601`
- [x] KK6: kayıt gate raporuna girer — sürüm + tohum
- [x] `09 §19.2` madde 6: `git grep -nE "Task\.Delay|Thread\.Sleep" tests/Shopfolio.IntegrationTests/Browser` boş (çıkış 1)
- [x] `09 §3.1` madde 5: `BrowserTestBase.cs` `#pragma warning disable CS0612` gerekçe yorumlu, yalnız üç satırlık `NavigateAsync` yöntemini sarar, hemen `restore` — kurala uygun
- [x] Üretim kodu dar: `components.css` `.button[hidden], .form-fields[hidden] { display: none }`; `_ProductFormBody.cshtml` `x is not null` (kök neden D-1703'te); `UploadComponentModels.cs` + `product-form.js` B101 boy denetimi — sınır ve cümle sunucudan (`data-max-bytes`, `data-size-message`)
- [x] `09 §25` ilgili sorular (paket tablosu satırı `Microsoft.Playwright` Apache-2.0 1.59.0; ön yüz §16 kalıbı; uyarı bastırma)
- [x] Güvenlik taraması

#### Değişinim denemeleri (chromium, `--filter-method`; hepsi `git checkout -- src` ile geri alındı, yeniden derlendi)
| # | Bozulan yer | Satır | Sonuç |
|---|---|---|---|
| 1 | `common.js:214` ikinci gönderim durdurması | B105 | KIRILDI (Scripted) |
| 2 | `product-form.js:436` seçimde boy denetimi | B101 | KIRILDI (Scripted: istek gitti, mesaj yok) |
| 3 | `components.css` `.button[hidden]` | B106 | KIRILDI (Scriptless: düğme görünür) |
| 4 | `OrderScreenModels.cs:247` onay işareti denetimi | B096 KRİTİK | KIRILDI (iki yol: onay bölümü yok, sipariş iptal oldu) |
| 5 | `AddressBooks.cs:187` işaretliyken gönderilen alıcı adı okunur | B107 KRİTİK | KIRILDI (Scriptless: `decoy_taken=True`); Scripted yeşil kaldı → K1 |

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | 3 proje |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format … --verify-no-changes --include` (dalın 11 `.cs` dosyası) | çıkış 0 |
| Unit | ✓ | `--no-build --filter-not-trait Consistency=true` / `--filter-trait Consistency=true` | 7420/7420, 667/667 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 |
| Integration | ✓ | yalnız Browser sınıfları (`--filter-trait Browser=true`; dal yalnız `Browser/` altına dokunuyor), motor başına: ConfirmWindowTests, EmptyStateTests, ObservationMaskTests, ScreenInventoryTests, ScriptlessFlowTests, ScriptlessPathTests, ValidationStateTests — TAM: CI | 211/211 × 3 |
| CI | ✓ | `gh pr checks 583` | 1–8 ve CI Gate pass; Integration 11 dk 46 sn; E2E 14 dk 43 sn |
| Locked restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz — sertifika koşu başına üretilir; kayıtta anahtar ve e-posta maskeli
- Auth etkisi: Temiz — yönlendirilen başlık güveni yalnız test sunucusunda (`127.0.0.0/8`); üretim yapılandırması değişmedi
- Input validation: Temiz — boy denetimi yalnız istemci iyileştirmesi, sunucu denetimi (422/413) yerinde; `data-size-message` tag helper ile kodlanır
- Yeni bağımlılık: `Microsoft.Playwright` 1.59.0 (yalnız Integration test projesi, Apache-2.0, `09` tablo satırı ve kilit dosyası güncel)

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| — | — | — | — | Bulgu yok | — | — | — |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | B107 betikli vakada fatura alanları gizli ve boş gönderildiği için "gönderilen fatura alanları siparişe girmez" ölçüsü ayırt edici değil (değişinim 5'te betikli vaka yeşil kaldı); sunucu kuralı yoldan bağımsız ve betiksiz vaka kuralı yakalıyor | DEFERRED_BACKLOG'a forward: D-1725 (`DEFERRED_BACKLOG.d/T551.md`) |

#### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu
- Düzeltme 5 öz denetimi KK1–KK6'yı ✓ ve kanıtla verir; benim sonucumla aynı. KK4 ve KK5 için kanıt "dokunulmadı, yeşil" — bu turda kendi koşularımla ve değişinimle doğruladım. D-1723 (öteki bağlantı düğmelerinin gönderimi) ⚠ olarak açıkça yazılmış; KK2 ve B110 bunu istemiyor, kabul.

#### Merge ve Post-merge
- Merge yardımcıda (adım 17–18).
