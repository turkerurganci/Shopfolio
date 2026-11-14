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
| 6 | GET boş boyut/değer/varyant satırı; boş okunmaz; `row-template` + `__index__` boşluksuz | ✓ | `ProductFormPageTests.TheRowTemplates_…KK6` (şablon alan adları sayfanın boş satırıyla aynı, yer tutucu şablon dışında yok, düğmeler `hidden`, kayıt formu şablondan alan göndermez); mevcut `…BlankNewEntriesAreNotRead_KK3` |
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
| KK6 iki yolda satır ekleme | ✓ | `ProductFormPageTests.TheRowTemplates_…KK6`, `…BlankNewEntriesAreNotRead_KK3` |
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
| Doğrulama durumu | — |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

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
| 1 | Betikle eklenen yeni seçenek satırı tek boş değer taşır | D-1554 | iç içe yer tutucu çakışması; betiksiz yolla aynı davranış |
| 2 | Tarayıcı davranışı (odak, önbellek dönüşü) sınanmadı | D-1555 | `12` tarayıcı senaryosu, T601 |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 43581890422, 43581890558, 43575320382 — hepsi success.
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- ⚠ Karar (kolay): ödeme adımında yalnız kapalı listeler, radyo düğmeleri ve kupon kodu değişince ara gönderim yapılır; adres metin alanları yazarken gönderim yapmaz (odak kaybı, `09 §16.2` madde 1 notu).
- ⚠ Karar (kolay): ürün formunda yeni seçenek şablonu tek değer girdisi taşır (D-1554).
