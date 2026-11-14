# T539 — Betiksiz yol: JavaScript kapalıyken temel akışlar ve betikli iyileştirme

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-14

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- Çok ekranlı denetim: var olan ekranlar yeniden yazılmadı; betiksiz yolun eksikleri dar düzeltildi, betikli iyileştirmeler `requestSubmit(submitter)` kalıbına çekildi.
- Ara gönderim: satış özeti dönemi `form.submit()` yerine `requestSubmit(düğme)`; yeni sayfa modülleri `pages/storefront/cart.js` (adet değişince aynı form kendi düğmesiyle gider) ve `pages/storefront/checkout.js` (il, kayıtlı adres, ödeme yöntemi → "Hesapla" düğmesiyle; kupon kodu → "Uygula" düğmesiyle; "teslimat adresimle aynı" işaretliyken fatura alanları katlanır).
- Çift gönderim: ürün sayfasının sepete ekle formu artık `disabled` yapmıyor, `aria-busy` + `pageshow` kullanıyor; ödeme adımının sipariş düğmesindeki `data-js="submit-once"` bir formda değil düğmedeydi ve hiç bağlanmıyordu — ortak betik düğme işaretini de bağlıyor (yalnız o düğmenin gönderimi kilitler).
- D-1443: `pages/panel/shipping-and-defaults.js` "Tümünü seç" ve "Seçimi kaldır" düğmelerini betikle ekler (işaretlemede yok).
- Ürün formu: `_ProductVariantEntry.cshtml` (satır ile şablon aynı işaretleme), seçenek ve değer şablonları, gizli "Ekle" düğmeleri, `pages/panel/product-form.js` (`__index__` değiştirme, boşluksuz numaralama, `innerHTML` yok).
- Kaynak taraması: `NoScriptPathSourceScanTests` (düz `submit()`/`requestSubmit()` yok, `disabled` ile kilit yok, uygulama parçası yok, sayfa modülü yolu sayfa yolunu izler ve bir görünüm yükler, manifest/service worker dosyası yok); `FrameSourceScanTests` betik taraması alt klasörleri de kapsıyor.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/wwwroot/js/common.js`, `product.js`, `js/pages/storefront/{cart,checkout}.js`, `js/pages/panel/{shipping-and-defaults,product-form}.js`
- `src/Shopfolio.Web/Pages/Panel/{_ProductFormBody,_ProductVariantEntry,ProductForm,NewProduct,ShippingAndDefaults}.cshtml`, `ProductVariantEntryView.cs`; `Pages/Storefront/{Cart,Checkout}.cshtml`
- Testler: `NoScriptPathSourceScanTests` (yeni), `FrameSourceScanTests`, `MessageSourceScanTests`, `ProductFormPageTests`, `CartPageTests`, `CheckoutPageTests`, `PaymentAndShippingPagesTests`, `FrameLayoutTests`, `SalesSummaryPageTests`, `StorefrontSearchAndProductTests`
- Kayıt: `DEFERRED_BACKLOG.d/T536.md` (D-1443 kapandı), `DEFERRED_BACKLOG.d/T539.md`, `STATUS_CHANGELOG.d/T539.md`, `IMPLEMENTATION_STATUS.md` tablo satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Temel akışlar JS kapalıyken form gönderimiyle tamamlanır; betikli yol ayrı uç çağırmaz | ✓ | Önceki task'ların sayfa testleri (sepet `UpdateCartItem`, ödeme `Recalculate`, panel formları) + `FrameSourceScanTests.TheScripts_Import…` (betikte `fetch`/XHR yok, artık alt klasörler dahil) |
| 2 | Ara gönderim aynı işleyici; `requestSubmit(submitter)`; parça yerleştirme yok | ✓ | `NoScriptPathSourceScanTests…SendFormsOnlyWithTheSubmitter…` (+ ihlal örnekleri); `cart.js`, `checkout.js`, `common.js` (submit-on-change) |
| 3 | Varyant seçimi, satış özeti dönemi, adres defteri ekleme/düzenleme GET | ✓ | `StorefrontSearchAndProductTests…T503_KK4`; `SalesSummaryPageTests…KK1` (yeni `method="get"` iddiası); `AccountAddressAndOrderHistoryPageTests` (`?create=true`) |
| 4 | `aria-busy`, ikinci `submit` durur, `disabled` yok, önbellek dönüşünde kaldırılır | ✓ | `MessageSourceScanTests.TheSubmitOnceScript_…KK7`; `StorefrontSearchAndProductTests…T503_KK5` (ürün sayfası); `CheckoutPageTests` (sipariş düğmesi `submit-once`); tarama `disabled`'ı yasaklar |
| 5 | Betik yalnız sunucunun `data-` değerlerini toplar, kural yazmaz | ✓ | Yeni betikler kural/oran içermez; durum adı ve tarih aritmetiği taraması T217 `OperationSourceScan` (c) betikleri de kapsar; tutar toplayan betik yok (müşteri kalem işlemlerinin son adımı sunucu cümlesidir) |
| 6 | GET boş boyut/değer/varyant satırı; boş okunmaz; `row-template` + `__index__` boşluksuz | ✓ | `ProductFormPageTests.TheRowTemplates_…KK6`, `TheNewForm_HasAValueTemplateForEveryPossibleDimension_…_D1554` (her olası boyut için değer şablonu; yeni boyut satırı kendi "Değer ekle" düğmesini taşır), `SaveProduct_ASecondValueUnderTheSecondDimension_…_D1554`; mevcut `…BlankNewEntriesAreNotRead_KK3` |
| 7 | "Tümünü seç" betiksiz yok; kopyalama/paylaşım düğmesi `hidden`, değer seçilebilir metin | ✓ | `PaymentAndShippingPagesTests…KK7` (metin yok, kanca ve modül var); mevcut T363/T512 düğmeleri `hidden` + `common.js` |
| 8 | "Teslimat adresimle aynı" fatura formuyla birlikte gider | ✓ | `CheckoutPageTests` (`name="IsBillingSameAsShipping"` fatura formunda; yeni iddia) + `CheckoutPageTests.Helpers` işaretli gönderim |
| 9 | Parçalı yükleme tek istisna, betiksiz yolu yok | ✓ | `ProductFormPageTests.ADigitalProductsForm_CarriesTheFileFields_HookedForTheChunkedUpload_KK2` (kanca; betik T548'dir, D-1399) |
| 10 | Mobil uygulama ve PWA yok | ✓ | `StorefrontFrameTests…T539_KK10` (manifest bağlantısı yok, `/manifest*`, `/sw.js`, `/offline` 404); `NoScriptPathSourceScanTests…HasNoManifest…` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 beş alandan birer akış, betikli yol ayrı uç çağırmıyor | ✓ | Sepet adedi: `CartPageTests.TheQuantityForm_WorksWithoutScript_…_T539_KK1_KK2_KK4`; ödeme: `CheckoutPageTests` `Recalculate`; panel: `ProductFormPageTests.SaveProduct_…`; hesap adresi: `AccountAddressAndOrderHistoryPageTests`; satış özeti GET; `fetch` taraması `FrameSourceScanTests` |
| KK2 ara gönderim aynı işleyici | ✓ | `cart.js` (adet formunun kendi düğmesi), `checkout.js` (`data-js="recalculate"`/`apply-coupon` düğmeleri aynı form), `common.js` submit-on-change; `NoScriptPathSourceScanTests` (`requestSubmit()` argümansız ve `.submit()` kırmızı) |
| KK3 üç GET | ✓ | Varyant `T503_KK4`, dönem `SalesSummaryPageTests…KK1`, adres `?create=true`/`?addressBookId` |
| KK4 ikinci gönderim durur; önbellek dönüşü | ✓ | `common.js bindSubmitOnce` (`pageshow`, `event.persisted`), `product.js` (aynı kalıp), `MessageSourceScanTests…KK7`; tarama `disabled` kırmızı |
| KK5 betikte kural yok | ✓ | Yeni betikler yalnız DOM işi; `NoScriptPathSourceScanTests` + `OperationSourceScan` (c) |
| KK6 iki yolda satır ekleme | ✓ | `ProductFormPageTests.TheRowTemplates_…KK6`, `TheNewForm_HasAValueTemplateForEveryPossibleDimension_…_D1554`, `SaveProduct_ASecondValueUnderTheSecondDimension_…_D1554`, `…BlankNewEntriesAreNotRead_KK3` |
| KK7 düğmeler betiksiz yok | ✓ | `PaymentAndShippingPagesTests…KK7` |
| KK8 fatura kutusu | ✓ | `CheckoutPageTests` yeni iddia |
| KK9 istisna | ✓ | `ProductFormPageTests…KK2` (kanca), D-1399 |
| KK10 uygulama ve PWA yok | ✓ | `StorefrontFrameTests…T539_KK10`, `NoScriptPathSourceScanTests` |
| `09 §25` ilgili satır (ön yüz kodu dosya düzeni ve betiksiz yol kalıpları) | ✓ | `NoScriptPathSourceScanTests…EveryPageModule_…` (yeni modüller sayfa yolunu izler; `common.js` ve `product.js` adlı istisna, T503'ten kalma) |
| Hedefi T539 olan backlog: D-1443 | ✓ kapandı | `DEFERRED_BACKLOG.d/T536.md` satırı ✅; test `PaymentAndShippingPagesTests…KK7` |
| Sık FAIL (2) tarama bir çağrı biçimini kaçırıyor | ✓ | `submit ( )` boşluklu, `requestSubmit()`, `setAttribute('disabled')`, `caches.` ihlal örnekleri teoride |
| Sık FAIL (4) kural değişince öteki yerler | ✓ | `common.js` döngüsü değişince `MessageSourceScanTests`'in bitiş işaretçisi güncellendi; `product.js` kilidi değişince T503 testi güncellendi; `FrameSourceScanTests` alt klasör |
| Güvenlik taraması | ✓ | Yeni dış bağımlılık yok; yeni betikler `fetch`/`innerHTML`/`eval` içermez, satır içi betik yok, CSP `script-src 'self'` ile uyumlu (`StaticAsset` modülleri) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 7374/7374 (TAM) | `dotnet test --project tests/Shopfolio.UnitTests --filter-not-trait "Consistency=true"` | geçti |
| Integration | ✓ ilgili sınıflar: ProductFormPageTests, StorefrontFrameTests, CheckoutPageTests, CartPageTests, PaymentAndShippingPagesTests (170/170); SalesSummaryPageTests, MessageComponentTests, AccountAddressAndOrderHistoryPageTests, StorefrontSearchAndProductTests, PageFormPatternTests (sonra düzeltilen 1 test dahil geçti) — **TAM: CI** | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-class …` | geçti |
| Contract | — | NOT_APPLICABLE | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-14) |
| Bulgu sayısı | 1 (B1: S1, DÜŞÜK) |
| Düzeltme gerekli mi | Evet — B1 |
| Bloke-etmeyen not (K) sayısı | 3 (K1–K3) |

**Tur 1 bulgusu.** B1 (S1, DÜŞÜK; KK6, `09 §16.2` madde 4, `07 §8.2.2` betikli yol): betikle eklenen seçenek boyutunun kendi değer şablonu ve "Değer ekle" düğmesi yok; şablon (`product-option-template`) tek boş değer taşır. `09 §16.2` madde 4 "her boyutun kendi şablonu vardır ve üst dizin şablonda sabittir" der, `07 §8.2.2` betikli yolda değerin yerinde eklenmesini ister. Seçeneksiz yeni üründe ikinci boyutun ikinci değeri kayıttan önce eklenemez. D-1554 bunu "iyileştirme" diye erteledi; kayıt kabul edilmiş risk değildir ve öz denetim KK6'yı ✓ saymıştır (uyuşmazlık). Öneri: boyut sayısı sabit (`OptionDimensionMax` = 2) — sunucu her olası boyut dizini için dizini sabit bir değer şablonu (`product-option-<n>-value-template`) yazar, betikle eklenen boyut satırı kendi dizininin şablonuna bağlı "Değer ekle" düğmesini taşır; KK6 testine yeni boyutun değer şablonu iddiası eklenir; D-1554 kapanır.

**K-notları.** K1: `NewProduct.cshtml` modülü `pages/panel/product-form.js`'yi yükler — iki sayfa aynı bölümü (`_ProductFormBody`) paylaşır, tarama sayfa yolunu izleyen modülü kabul ediyor; davranış doğru, not. K2: ödeme adımında kupon alanı `change` (odak kaybı) ile "Uygula"yı gönderir; kupon yazılıp doğrudan sipariş düğmesine basılırsa iki gönderim yarışır — sonuç sunucunun özet parmak izi denetimiyle korunur (`02 §5.3.3`), tarayıcı davranışı D-1555'in (T601) kapsamında. K3: betikle eklenen dijital varyant satırının dosya girdisi parçalı yükleme betiğine sonradan bağlanmalıdır — betik henüz yok (T548, D-1399); T548'in bağlamasının `row-add` ile eklenen satırları da kapsaması gerekir → `DEFERRED_BACKLOG.d/T539.md` D-1559.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T539-betiksiz-yol`
- **Commit:** dal `task/T539-betiksiz-yol` (PR #576 geçmişi)
- **PR:** #576
- **CI:** yardımcı yürütür
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `requestSubmit(submitter)` ve `<template>` son iki büyük sürümde var (`05 §3.8.4`) | Bilgim tarihli; tarayıcıda sınanmadı | ~ (D-1555, T601) |
| 2 | Dış varsayım başka yok | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | ~~Betikle eklenen yeni seçenek satırı tek boş değer taşır~~ | D-1554 ✅ | Düzeltme 1'de kapandı (her boyutun kendi değer şablonu) |
| 2 | Tarayıcı davranışı (odak, önbellek dönüşü) sınanmadı | D-1555 | `12` tarayıcı senaryosu, T601 |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 43581890422, 43581890558, 43575320382 — hepsi success.
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- ⚠ Karar (kolay): ödeme adımında yalnız kapalı listeler, radyo düğmeleri ve kupon kodu değişince ara gönderim yapılır; adres metin alanları yazarken gönderim yapmaz (odak kaybı, `09 §16.2` madde 1 notu).
- **Düzeltme 1 (2026-11-14, B1):** `_ProductFormBody.cshtml` her olası boyut dizini (`OptionDimensionMax` = 2) için ayrı `product-option-<n>-value-template` yazar; `product-option-template` satırı kendi değer listesini (`product-option-__index__-values`) ve kendi "Değer ekle" düğmesini taşır, düğmenin `data-template`/`data-list` değerlerindeki yer tutucuyu betik yeni satıra yazarken değiştirir (üst dizin her şablonda sabit, iç içe çakışma yok); `product-form.js` eklenen satırdaki düğmeyi bağlar. D-1554 kapandı. K1: `NewProduct.cshtml` modül yüklemesine açıklama yorumu. Öz denetim: T539 kabul kriterlerinin ve KK1–KK10'un bütünü yeniden denetlendi; KK6 artık tam. Yeni testler: `TheNewForm_HasAValueTemplateForEveryPossibleDimension_AndTheNewDimensionsRowCarriesItsOwnAddButton_KK6_D1554`, `SaveProduct_ASecondValueUnderTheSecondDimension_AsTheScriptNumbersIt_IsSaved_KK6_D1554`. Sınama: build `-warnaserror` 0/0, `dotnet format` (dalın .cs dosyaları) temiz, Unit TAM 7680/7680, Integration ProductFormPageTests + StorefrontSearchAndProductTests + ProductSavesTests 95/95 — TAM: CI.
