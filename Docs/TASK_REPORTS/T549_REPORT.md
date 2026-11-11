# T549 — Vitrin: arşivlenmiş ürün sayfası (E-05)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- E-05'in gövdesi T503'te yazılmıştı (`GET /urun/{Slug}` arşivdeyken 200, ad, ana görsel, "Bu ürün artık satılmıyor", fiyat ve form yok, noindex). T549 eksik kalan iki şeyi kapattı: **dönüş bağlantıları** (`04` 5.5.3 madde 4: ana sayfa ve ana sayfanın birinci seviye kategorileri; yayında ürün yoksa yalnız ana sayfa) ve arşiv sayfasında `product.js` yüklenmemesi.
- Sekiz kabul kriteri ve KK1–KK8 için sınama yazıldı (aşağıda).
- D-1207 kapandı; D-357'nin hedefi F5 kapısına çevrildi (⚠ aşağıda).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Product.cshtml` — arşiv dalına dönüş bağlantıları; `product.js` yalnız arşiv dışı.
- `src/Shopfolio.Web/Pages/Storefront/ProductView.cs` — `ProductViews.ReturnLinksOf` (menü köklerinden dönüş bağlantıları; `FrameMenuTree.Build` kullanır, ikinci bir ağaç kurucu yok).
- `src/Shopfolio.Web/wwwroot/css/storefront.css` — `.product__return-list`.
- `tests/Shopfolio.IntegrationTests/Web/StorefrontSearchAndProductTests.ArchivedPage.cs` (8 test), `tests/Shopfolio.UnitTests/Pages/ArchivedProductPageTests.cs` (3 test).
- `Docs/IMPLEMENTATION_STATUS.md` (T549 satırı ⏳), `Docs/STATUS_CHANGELOG.d/T549.md`, `Docs/DEFERRED_BACKLOG.d/T549.md`, `Docs/DEFERRED_BACKLOG.md` (D-357 satırı).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Arşivde 200, E-05, adres değişmez, ayrı uç yok | ✓ | `AnArchivedProduct_IsE05_AtTheSameAddress_With200_AndThereIsNoOtherAddress_T549_KK1` |
| 2 | Yalnız ad, ana görsel, satılmıyor bilgisi, iki dönüş bağlantısı | ✓ | `TheArchivedPage_HasTheName_TheMainImage_TheLine_AndTheWaysOut_AndNothingElse_T549_KK2`; Unit `ReturnLinksOf_*_T549_KK2` |
| 3 | Fiyat, varyant verisi, sepete ekleme formu, birincil düğme yok | ✓ | `TheArchivedPage_HasNoPrice_NoVariantData_NoForm_AndNoPrimaryButton_T549_KK3` (fiyat metni, SKU, varyant kimliği, `<form>`, `<button>`, `button--primary`, `application/ld+json`, boş olmayan `data-variants` yok) |
| 4 | Liste, kategori, arama, site haritasında yok; noindex | ✓ | `TheArchivedPage_IsInNoCategoryNoSearchNoSiteMap_AndIsNoindex_T549_KK4` (aynı kategorideki yayındaki ürün her yerde var — zıt örnek) |
| 5 | Ziyaretçiye ve yöneticiye aynı; şerit çerçevenin parçası | ✓ | `TheArchivedPage_IsTheSameForAVisitorAndAnAdministrator_AndTheStripIsTheFrames_T549_KK5` (`<main>` içeriği eşit; şerit `<main>` dışında) |
| 6 | Arşive sepete ekleme UC-05'in 404'ü | ✓ | `AnAddToTheCart_OfAnArchivedProduct_IsThe404_AndNothingIsWritten_T549_KK6` |
| 7 | Yayına dönünce E-04, kalıcı silinince 404 | ✓ | `AProductPublishedAgain_IsE04AtTheSameAddress_AndOneDeletedForGood_IsThe404_T549_KK7` |
| 8 | Görselsiz yalnız adıyla; yükleniyor/hata OB-07 | ✓ | `AnArchivedProductWithoutAnImage_ShowsTheNameAlone_WithNoImageAndNoGap_T549_KK8`; sayfa sunucuda çizilir, betik yüklenmez (yükleniyor hâli yok); beklenmeyen hata genel hata katmanındadır (OB-07, T2xx) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: 200 ve E-05; adres sabit | ✓ | `…ArchivedPage.cs` KK1 testi; seçim parametreli adres de aynı sayfa ve kapalı |
| KK2: yalnız üç bilgi ve iki bağlantı | ✓ | KK2 testi: ikinci görsel, açıklama, kırıntı, teslimat, Paylaş, betik yok |
| KK3: fiyat ve form yok | ✓ | KK3 testi (iki varyantlı, seçenekli ürün) |
| KK4: dört yerde yok; noindex | ✓ | KK4 testi: kategori, arama, ana sayfa, `/sitemap.xml`; noindex ve canonical yok |
| KK5: iki rolde aynı | ✓ | KK5 testi |
| KK6: sepete ekleme 404 | ✓ | KK6 testi (varyantlı, varyantsız, olmayan adres aynı gövde) |
| KK7: yayına dönüş; silinince 404 | ✓ | KK7 testi (silinen ürün olmayan adresle aynı 404 gövdesi) |
| KK8: görselsiz hâl | ✓ | KK8 testi |
| `09 §25` ilgili satırlar | ✓ | ham işaretleme yok (`HtmlString` kullanılmadı; mimari tarama yeşil); satır içi stil/betik yok; yeni sınıf ihlal örneği gerektiren tarama kuralı değil; `09 §3.4` tek kural evi: dönüş bağlantıları E-06'nın `FrameMenuTree.Build`'i |
| Güvenlik taraması | ✓ | yeni girdi yok (GET, aynı slug çözümü); gizli değer yok; çıktı Razor kaçışlı; kimlik/yetki etkisi yok (yönetici ve ziyaretçi aynı gövde) |
| sık-fail-nedenleri | ✓ | (1) yeni istisna yok; (2) tarama kuralı değişmedi; (3) DB/FK değişikliği yok; (4) sayı anan başka yer yok (sayaç, README, şablon etkilenmedi); (5) 09 metni değişmedi, karar kaydı gerekmedi. Hedefi T549 olan backlog kayıtları (D-357, D-1207) ele alındı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... -c Release --filter-not-trait "Consistency=true"`; `--filter-trait "Consistency=true"` | 6917/6917; Consistency 295/295 |
| Integration | ✓ (ilgili sınıflar) | `--filter-class StorefrontSearchAndProductTests` ; `SearchEngineDataTests`, `SecurityHeadersTests`, `NotFoundPageTests` | 32/32 (8 yeni); 41/41. Integration TAM koşulmadı — CI'a bırakıldı |
| Contract | — | NOT_APPLICABLE | — |

`dotnet format --verify-no-changes` değişen üç `.cs` dosyada temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✓ PASS — tur 1 (2026-11-12, head `7973b482`; `origin/main` `9f5b8ac8` (T503 #542, T520 #549) birleşik ağaçta, birleştirme çakışmasız). Kriter 1–8 ve KK1–KK8 ✓ (kanıtlar: `StorefrontSearchAndProductTests.ArchivedPage.cs` 8 test, `ArchivedProductPageTests` 3 test; kod okuması `Product.cshtml` arşiv dalı, `ProductModel.BuildAsync` arşiv kolu ve `OnPostAddCartItemAsync`'in yayında olmayan ürüne 404'ü, `ProductHeads` kapalı sayfada yapılandırılmış veri yok, `FramedNotFoundPageRenderer` ile aynı `FrameMenuTree.Build`). Yerel: derleme `-warnaserror` 0 uyarı, format üç projede temiz, Unit 7215/7215 (Consistency ve Architecture dahil), betik testleri 4/4, Integration tam koşu ad alanı parçalarıyla yeşil (`StorefrontSearchAndProductTests` 32, Catalog 82, Administration–Files 551, Database–SignedEndpoints ve kök 308, Model 1410, Ordering 645, Web'in kalanı 797); `restore --locked-mode` 0. Dal CI'ı `43114110038` success. `12`: VAL-A021, VAL-A023 → PASS; KP-7'nin VAL-A022 (PASS, T503), VAL-D001 ve VAL-A024 (T549 kriterine bağlı değil) değişmedi. |
| Bulgu sayısı (tur 1) | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (tur 1) | 2 — **K1** (bu PR'da kapatıldı): D-1207 kapanışı yalnız `DEFERRED_BACKLOG.d/T549.md` notuna yazılmış, satır `DEFERRED_BACKLOG.d/T503.md`'de ⚪ açık kalmıştı (`DEFERRED_BACKLOG.d/README.md`: kapatma yerinde yapılır); satır ✅ yapıldı. **K2** (forward D-1311): yerel Integration `Model` ad alanının ilk koşusunda 1 test başarısız, iki yeniden koşu 1410/1410 yeşil; testin adı yakalanamadı, T549 o koda dokunmaz. |
| D-357 hedef değişikliği | Kurala uygun: kalem T549'un kabul kriterlerinde yok (hedefi T363 vermişti), erteleme backlog satırına gerekçesiyle yazıldı (`GUARDRAILS` "backlog'a yazmadan erteleme" yasağı karşılandı) ve üst sınır faz kapısıdır (`DEFERRED_BACKLOG.md` kayıt kuralı 5). Kapı kod yazmaz; kalemi yeni bir task'a hedefler. |
| Yapım raporu karşılaştırması (tur 1) | Kabul kriterlerinde tam uyum (1–8 ✓); öz denetim tablosu var, her madde kanıtlı. Uyuşmazlık 2: D-1207 "kapandı" denmiş ama satır yerinde kapatılmamış (K1); kriter 8 kanıtında "OB-07, T2xx" yer tutucusu — genel hata katmanı `UnexpectedErrorLayer` (T541) olarak doğrulandı. |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T549-arsiv-urun-sayfasi`
- **Commit:** `7973b482`
- **PR:** #550
- **CI:** `43114110038` success (doğrulama anında)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Task'ta plan tier, kota, paket sürümü, dış servis ya da ortam varsayımı geçmiyor (4a tetikleyici taraması: eşleşme yok); yeni paket eklenmedi.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Arama motoru başlığının `StructuredData.Render`'a taşınması | D-357 (hedef T549 → F5 kapısı) | Arşiv sayfasıyla ilgisiz; tüm sayfaların başlığını ve mimari taramayı etkiler |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** `43101562500` success, `43101562426` success, `43090427576` success.
- **Repo memory (bitiş kapısı 9):** "Güncel Durum"a dokunulmadı (parça dosyası kuralı).
- ⚠ **D-1207 (T503'ten, hedef T549) kapatıldı:** taslak önizlemenin panel bağlantısı T503/T540'ta `SetDraftPreview` ve Taslak bandıyla var; E-05 yöneticiye ziyaretçiyle aynıdır (`04` 5.5.7). Gerekçe `DEFERRED_BACKLOG.d/T549.md`'de.
- ⚠ **D-357 hedefi F5 kapısına çevrildi** (yukarıdaki gerekçe).
- ⚠ **Boş `data-variants=""` özniteliği:** arşiv sayfasında öznitelik boş kalır (Razor boş `null`'ı boş öznitelik yazar); veri taşımaz ve betik yüklenmez; sınama boş olmayan değeri yasaklar. Öznitelik hiç yazılsın istenirse ham işaretleme gerekir (`09 §15.3`'e aykırı), bu yüzden yapılmadı.
- Derleme ve Integration TAM CI'da; yerelde yalnız ilgili sınıflar koşuldu.
