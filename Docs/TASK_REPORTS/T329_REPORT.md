# T329 — İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi)

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `02 §2.2`'nin sekiz adımı (`03 §2.10.1.1`–`2.10.1.8`, akış 1 + 6) tek Integration testinde servis katmanından baştan sona geçer: `MainFlowTests.ThePhysicalCardOrder_GoesTheEightStepsOfTheMainFlow_WithTheFakePayment_KK1_To_KK7`.
- Test gerçek PostgreSQL'de, kendi koleksiyon veritabanında (`MainFlow`) koşar; saat `ManualTimeProvider`'dır (ödeme, kargo ve teslim arasında ilerletilir; gerçek bekleme yok).
- Sunucu: uygulamanın kendi ödeme bildirimi ucu (`MapPaymentNotificationEndpoint`) imzalı uç hattıyla test sunucusunda; Catalog, Ordering, Payment, AfterSales, Settings modülleri; Notifications yazıcıları; iş koşucusu (`JobRunnerService.RunOneAsync`) aynı saatle; sağlayıcı `FakePaymentGateway` (`09 §19.1` m.3, m.6).
- Adımlar:
  1. `ICatalogModule.SearchProductsAsync("lamba")` yayındaki ürünü bulur, `GetProductPageAsync` sayfası iki varyantı verir; "Beyaz" varyantı stok koduyla seçilir.
  2. `IOrderingModule.AddToCartAsync` (2 adet) → `Added`; sepet canlı, ödeme adımı açık; `reserved_quantity` 0, `stock_reservations` boş, sipariş yok.
  3. `OrderConfirmation.SummarizeAsync` — teslimat ve fatura adresi, kupon `T329` (%10), kart; toplam sabitlerden türetilir (1800).
  4. `IConfirmationStep.GetViewAsync` — iki kutu, gösterilen form ve sözleşme sürümleri, kutuların üstündeki cayma özeti; sipariş hâlâ yok.
  5. `OrderConfirmation.ConfirmAsync` — Alındı + Bekliyor, sürüm 1, numara `2026-…`, adresler, sürümler, kart ödeme süresi; stok 2 ayrılır (stok değişmez), kupon hakkı 1 ayrılır, iki ayırma satırı; B-1 müşteriye, F-1 firmaya; işlem izi satırı yok. Sonradan değişen ürün adı ve fiyat donmuş kaleme ulaşmaz.
  6. Sahte sağlayıcı başarı bildirimini doğrular, uç 200 `OK` döner, kayıt yazılır; sipariş iş koşana dek kıpırdamaz. İş koşunca Hazırlanıyor + Ödendi, sürüm 2; Ö1 + S1 sistemce; ödeme kaydı Ö1 satırına bağlı; stok kesin düşer (5→3, ayrılmış 0), kupon kullanılmış 1 / ayrılmış 0, ayırma satırları silinir, sipariş kalemi sepetten çıkar; kargoya verme süresi ödeme anından başlar (`GetShippingViewAsync`); tek B-4.
  7. Yönetici `ShipOrderAsync` (onaylı, YurticiKargo + takip no) → Kargoya verildi + Ödendi, sürüm 3; S5 yönetici adıyla; tek B-5.
  8. Yönetici `MarkDeliveredAsync` (dünün tarihi) → Teslim edildi + Ödendi, sürüm 4; S6; kalemin teslim tarihi ve işaret anı; yeni bildirim yok (`02 §9.2`).
- Son onay: işlem izinin bütünü `OrderLines.Rows`'un kart hattı satırına (`03 §1.5.1`) eşittir; yöneticinin satırları sırayla S5, S6'dır ve sayısı `OrderLines.StepsOf([Physical], Card)` ile ikidir (`03 §8.5.1`).
- Üretim kodu değişmedi.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.IntegrationTests/Ordering/MainFlowTests.cs` (yeni)
- `Docs/TASK_REPORTS/T329_REPORT.md`, `Docs/IMPLEMENTATION_STATUS.md` (T329 satırı ⏳), `Docs/STATUS_CHANGELOG.d/T329.md`, `Docs/DEFERRED_BACKLOG.d/T329.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz adım tek Integration testinde, servis katmanından, `FakePaymentGateway` ile | ✓ | `MainFlowTests` tek `[Fact]`; adım başlıkları `Step 1`…`Step 8`; `_gateway.EnqueueVerification` |
| 2 | Adım 1–2: varyant bulunur, sepete eklenir; stok ayrılmaz | ✓ | `SearchProductsAsync` + `GetProductPageAsync`; `AddToCartAsync`; `AssertStockAsync(_variantId, 5, 0)`, `stock_reservations` 0 |
| 3 | Adım 3–5: adres, kupon, yöntem; onayda sipariş, numara, donma, stok ve kupon ayrılır | ✓ | `SummarizeAsync`/`GetViewAsync`/`ConfirmAsync`; numara, adres, sürüm onayları; `AssertStockAsync(_variantId, 5, 2)`, `AssertCouponAsync(0, 1)`; donmuş kalem onayı |
| 4 | Başarılı sahte ödeme, uçta kayıt, Ö1 kuyruk işinde; B-4 | ✓ | uç 200 `OK`; `SingleInboundAsync` (işlenmemiş) + eksen değişmedi; `RunDueJobsAsync` sonrası iş `Completed`, Ö1 + S1, tek B-4 |
| 5 | S5 ve S6; kart hattında iki zorunlu elle adım | ✓ | `ShipOrderAsync`, `MarkDeliveredAsync`; `adminCodes` = kart hattı satırının yönetici adımları, sayı `OrderLines.StepsOf` = 2 |
| 6 | Her geçişte iki eksen, yan etkiler ve işlem izi | ✓ | `AssertAxesAsync` her adımda (eksen + sürüm 1→4); stok, kupon, ayırma, sepet, ödeme kaydı, bildirim sayıları; `ReadTrailAsync` |
| 7 | Yalnız ana akış; dal anlatıları T390'da | ✓ | tek fiziksel kalemli kartla ödenen sipariş; karışık sipariş, cayma, iade, e-posta düzeltme yok |

### Öz denetim (bitiş kapısı 2)

> PR açılmadan önce planın kabul kriterleri ve doğrulama kontrol listesinin **her** maddesi kendi diff'inde denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1 / KK1: sekiz adım tek testte, sahte ödemeyle | ✓ | `MainFlowTests.cs` tek `[Fact]` `ThePhysicalCardOrder_GoesTheEightStepsOfTheMainFlow_WithTheFakePayment_KK1_To_KK7`; koşu: 1/1 geçti |
| Kriter 2 / KK2: sepete eklemede stok ayrılmaz | ✓ | Step 2 bloğu: `AssertStockAsync(_variantId, stock: 5, reserved: 0)`, `stock_reservations` 0, `orders` 0 |
| Kriter 3 / KK3: onayda sipariş, numara, kalem ve adres donar, stok ve kupon ayrılır | ✓ | Step 5 bloğu: `Assert.Matches("^2026-")`, adres ve sürüm onayları, `AssertStockAsync(…, 5, 2)`, `AssertCouponAsync(0, 1)`, iki ayırma satırı; ad ve fiyat değişikliği sonrası kalem aynı |
| Kriter 4 / KK4: bildirim Ö1'i işler, B-4 üretilir | ✓ | Step 6 bloğu: uç yanıtı, işlenmemiş kayıt ve değişmeyen eksen, `RunDueJobsAsync`, `job_state = Completed`, `ProcessedAt` dolu, Ö1 + S1 sistem, ödeme kaydı ↔ Ö1, `Assert.Single(… B4 …)` |
| Kriter 5 / KK5: kargo ve teslim iki zorunlu elle adımla biter | ✓ | Step 7–8 blokları; son blok `adminCodes` = `OrderLines.Rows` kart hattının yönetici adımları (S5, S6), sayı `OrderLines.StepsOf(...).Count` = 2 |
| Kriter 6 / KK6: her adımda iki eksen doğru | ✓ | `AssertAxesAsync` adım 2'den sonra yok (sipariş yok), adım 5 (Alındı/Bekliyor/1), 6 öncesi (aynı), 6 (Hazırlanıyor/Ödendi/2), 7 (Kargoya verildi/Ödendi/3), 8 (Teslim edildi/Ödendi/4); yan etkiler ve iz satırları |
| Kriter 7 / KK7: kapsam dışı anlatılar yok | ✓ | Test tek fiziksel varyant, kart; T390'ın anlatıları sınıfta yok (sınıf özetinde yazılı) |
| `09 §25` — diff yalnız görevin istediği | ✓ | Yalnız yeni test dosyası + kayıt dosyaları; üretim kodu yok |
| `09 §25` — zaman `TimeProvider`'dan | ✓ | Sunucuya `ManualTimeProvider` tekil kayıt; iş koşucusu aynı saat; `Task.Delay`/`Thread.Sleep` yok |
| `09 §25` — yeni paket / ayar / sabit | ✓ | Yok; sayılar testin kendi sabitleri, beklenen toplam onlardan türetilir (`09 §10.7` m.1) |
| `09 §19.1`–§19.2 — gerçek PostgreSQL, koleksiyon veritabanı, elle yazılmış sahteler, kendi verisi | ✓ | `CollectionDatabaseFixture`, `FakePaymentGateway`, `ManualTimeProvider`; `InitializeAsync` kendi tohumunu kurar |
| Güvenlik taraması | ✓ | Sır yok (`digest-placeholder`, örnek e-posta/IBAN yok); dış ağ yok; yetki/doğrulama yüzeyine dokunulmadı |
| Sık FAIL nedenleri (`sik-fail-nedenleri.md`) | ✓ | Bekçi/tarama yok; göç/DB default yok; sayıyı anan başka yer yok; 09 metni değişmedi; devredilen backlog kalemi yok (`grep T329` boş); test sabitten türetiyor; Integration'da üretim kodu değişmediği için başka sınıf etkilenmez |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --filter-namespace "Shopfolio.UnitTests.Architecture"` | 1150/1150 (test projelerini tarayan üç tarama dahil; tarama sınıfları ayrıca 100/100) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-class "Shopfolio.IntegrationTests.Ordering.MainFlowTests"` | 1/1 |
| Contract | — | `NOT_APPLICABLE` | — |
| Build | ✓ | `dotnet build tests/Shopfolio.IntegrationTests/... -c Release -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format tests/Shopfolio.IntegrationTests/... --verify-no-changes --include .../MainFlowTests.cs` | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (D-840, D-841) |

### Doğrulama tur 1 — Doğrulama Sonucu — T329 İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi) — tur 1
**Tarih:** 2026-11-09   **Branch:** task/T329-ilk-uctan-uca   **Commit:** `288f56d2` (doğrulanan dal ucu) + `origin/main` `824662a8` (T365 #496) birleştirmesi, PR #499

**Verdict: ✓ PASS**

Kabul kriterleri 1–7 ✓, KK1–KK7 ✓, `09 §25` ve güvenlik taraması temiz, bulgu 0. `12`'de task satırı yok (kaynak birimlerinde KP yok; `12_matris_denetle.py` hata 0). Yerel: build `-warnaserror` 0 uyarı, format üç projede temiz, Unit 5203/5203 + tutarlılık 288/288, betik 4/4, Integration `MainFlowTests` + `PaymentNotificationTests` 15/15 (dal ucu ve birleşik ağaç), `LivenessEndpointTests` 16/16. Dal CI 42609814871 yalnız `4. Integration test`'te kırmızı: `LivenessEndpointTests.Get_WhenTheServerIsUnreachable_Returns503WithinTheLimit` zamanlama (5.3 sn / 1 sn; dal `Web/`'e ve üretim koduna dokunmaz) → D-841; merge koşulu kayıt commit'li ucun CI yeşili. K1 ödeme başlatma adımı testte yok (başlatan servis henüz yok) → D-840 (T401). Yapım raporuyla tam uyumlu.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T329-ilk-uctan-uca`
- **Commit:** `8611eaa6` — T329: İlk uçtan uca akış testi — sekiz adım tek Integration testinde, sahte ödemeyle
- **PR:** #499
- **CI:** 42609814871 (`288f56d2`) — `4. Integration test` yalnız `LivenessEndpointTests` zamanlama testinde kırmızı (D-841), öteki işler yeşil; kayıt commit'inin CI'ı merge koşulu
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok. Ödeme sağlayıcısı `FakePaymentGateway` (ağ yok, `09 §19.1` m.6); tetikleyici tarama (plan tier, kota, API key, sandbox…) kabul kriterlerinde eşleşme vermedi. | Kabul kriterleri metni | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ödeme bildirimi ucunu barındıran test sunucusu kurulumu `PaymentNotificationTests` ile yineleniyor; T390 için ortak kurucuya alınmalı | D-828 | Kapsam dışı yeniden düzenleme (§23); T390 aynı sunucuyu isteyecek |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = ana deponun `scripts/git-hooks` dizini (mutlak yol).
- **Ana dal CI startup (Adım 0):** 42600847174 success, 42600846924 success, 42596386708 success (ayrıca 42596386862 success).
- **Bağımlılıklar:** T311, T318, T332, T335, T346, T348, T350, T351, T352 STATUS'ta ✓.
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- **⚠ Kolay kararlar:**
  - ⚠ Ödeme bildirimi servis çağrısıyla değil, uygulamanın kendi ucuna test sunucusundan POST ile verildi: kriter 4 "uçta kaydedilir" der; geri kalan bütün adımlar servis katmanındandır.
  - ⚠ Ödemenin başlatılması (jeton/3D Secure çerçevesi) bu testte yok: başlatan servis henüz bağlı değil (`PaymentStartRule` yalnız kural; sağlayıcı task'ı T401, D-786). Kriter "sahte ödeme sonucu" ister; sonuç bildirimle gelir.
  - ⚠ Müşteri üyedir, adres yeni yazılır (deftere kaydetme isteğe bağlı, `02 §2.2` adım 3); havale hattı kapalı — akış kartla öder.
  - ⚠ Göç demetinin ilk derlemesi 150 sn'yi aştı (çıkış 143); demet önbelleğe alındıktan sonraki koşular sınır içinde kaldı (tek seferlik 330 sn sınırlı koşu).
- **Not:** Test Ö1'in sipariş kalemini sepetten çıkardığını da onaylar (`cart_items` 0); bu D-795'in (hedef T353) istediği davranıştır, ancak D-795 `OrderConfirmationTests`'teki ad/gövde farkıdır ve bu task'ta kapatılmadı.
