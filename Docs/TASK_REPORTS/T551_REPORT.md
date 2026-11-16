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

## Doğrulama

| Alan | Değer |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-16, head `0f3f98fc`) |
| Bulgu sayısı | 5 — 4 ORTA S1 (KK5: E2E işi `dotnet tool restore` eksikliğinden hiç koşmadı · KK2: E-05, yönetici onay pencereleri, hata ve boş hâller eksik · VAL-B101 betikli yolda boy denetimi yok · B099/B100/B102/B105/B110 kısmi gözlem) · 1 DÜŞÜK S1 (gözlem kaydının `Observed` sütunu maskesiz) |
| Düzeltme gerekli mi | Evet — beş bulgu bu dalda |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-1709, K2 → D-1710) |

Tur 1'in tam sonucu raporun sonundaki "Doğrulama Sonucu — T551 … — tur 1" bölümündedir.

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
