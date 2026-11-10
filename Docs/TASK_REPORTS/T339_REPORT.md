# T339 — Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-07

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Metin biçimi temizleyicisi:** `Infrastructure/Security/RichTextSanitizer.cs` (izin listesi `09 §15.3`: `strong em ul li a h3 p br`; `href` yalnız `a`'da; şema `http`, `https`, `mailto`; göreli ve şemasız adres düşer; `rel="noopener noreferrer"` zorla; `script`, `style`, `iframe`, `svg`, `math` içeriğiyle düşer; iş parçacığı başına bir temizleyici) ve `RichText.Render` (gösterimde yeniden süzer, saklı değere dokunmaz).
- **Ürün açıklaması:** `Product.Description` `private set`; tek yazım yolu `SetDescription` (temizler, boşluk-yalnız metin `null` olur). Karakter tavanı yok (`text`); ad tavanı `ProductNameMaxLength` (mevcut).
- **Galeri:** `ProductGallery` (`SaveAsync`, `SetVariantImageAsync`, `ReadAsync`, `GetCardAltTextAsync`, `GetVariantImageAsync`) — yazımlar çağıranın işleminde, `SaveChanges` çağırmaz, işlem dışı çağrı `InvalidOperationException`; `GalleryRules` (ana işaret, adet, saf), `ImageAltText` (üretilen metin), `GalleryContracts`, `ICatalogModule` + `CatalogModule` beş yeni yöntem.
- **Ana işaret taşıma:** tek ana dizini (`ix_… is_main`) ardışık iki izlenen güncellemede ara adımda çakışabildiğinden eski ana işaret `GalleryMainMark.ClearAsync` ile (tek toplu deyim, çağıranın işleminde) önce kaldırılır; dosya tek deyimlik ve bulgu taramasının kapalı yer listesinde tek başına durur (⚠ 6, D-361).
- **Paket:** `HtmlSanitizer` 10.0.1120 (K-1577; ⚠ 1), kilit dosyaları güncel, `LibraryTableConsistency.NotYetAdded`'dan çıkarıldı, `09` Kütüphaneler satırı.
- **Statik küme:** `RichTextSanitizer.DroppedWithContent` `FrozenSet` (önbellek taraması `HashSet` alanını bulgu sayıyordu).
- **Eski kullanımlar:** `Product.Description = …` atayan testler ve sayfa `SetDescription`'a çevrildi (`ConcurrencyTests`, `VersionedProduct.cshtml.cs`, `VersionedFormPageTests` iki yer, `SearchEngineDataTests` `<b>` → `<strong>`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/`: `RichTextSanitizer.cs`, `RichText.cs`
- `src/Shopfolio.Web/Modules/Catalog/`: `Product.cs`, `ProductGallery.cs`, `GalleryRules.cs`, `GalleryMainMark.cs`, `ImageAltText.cs`, `Contracts/GalleryContracts.cs`, `ICatalogModule.cs`, `CatalogModule.cs`
- `Directory.Packages.props`, `packages.lock.json` dosyaları
- Testler: `tests/Shopfolio.UnitTests/{Security/RichTextSanitizerTests.cs + rich-text-allowlist.json, Architecture/RichTextSourceScan(.Tests).cs + ViolationSamples/RichText/RawMarkupLeak.cs, Architecture/VersionedSideBulkWriteScan(.Tests).cs, Catalog/ProductGalleryRulesTests.cs, Catalog/ProductTextTests.cs}`, `tests/Shopfolio.IntegrationTests/Model/ProductGalleryTests.cs`
- Belge ve kayıt: `Docs/09_CODING_GUIDELINES.md` (Kütüphaneler satırı), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1577), `Docs/DEFERRED_BACKLOG.md` (D-357…D-361), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Görsel ürün düzleminde; varyant kendi görselini taşır, yoksa ürününkini devralır | ✓ | Integration `AVariantWithoutItsOwnImage_ShowsTheProductsMainImage_AndAVariantWithOneShowsItsOwn`, `AVariantOfAProductWithoutImages_ShowsNone` |
| 2 | Ayrı ana işaret, sıradan bağımsız; ilk eklenen ana; kaldırılınca sıradaki ilk ana | ✓ | Integration `TheFirstImageAdded_BecomesMain_WhenTheListMarksNone`, `WhenTheMainImageLeaves_TheFirstImageInTheOrderBecomesMain`, `TheMainMark_IsIndependentOfTheOrder_AndMovingItNeverBreaksTheOneMainIndex`, `AListThatMarksSeveralImages_TakesTheFirstMarkedOne`; Unit `ProductGalleryRulesTests` (ana kuralları) |
| 3 | Galeri sırasını firma elle belirler | ✓ | Integration `TheOrderOfTheList_IsTheOrderOfTheGallery` |
| 4 | Format, boyut, adet tavanı; adede varyant görselleri dahil; değerler ürün sabiti | ✓ | Adet: Integration `TheGallery_TakesTheCeilingAndRefusesOneMore_AndWritesNothingOnTheRefusal`, `TheVariantsImages_CountWithTheGalleryOnes_AndAnImageUsedTwiceCountsOnce`; Unit `TheLimit_IsTheProductConstant_AndItIsExceededOnlyAboveIt`, `TheCountOfAProductsImages_IsTheGalleryAndTheVariantsTogether_EachImageOnce` (`ProductConstants.ImagesPerProductMax`). Format ve boyut tavanı T223'tedir (kriter metni: "tür denetimi ve piksel tavanı T223'tedir"): Integration `Files/ImageFilesTests` (boyut tavanı), `ImageUploadResult.UnsupportedType` |
| 5 | Yeniden kodlama; yön uygulanır, meta veri atılır, hareketli görsel ilk karesiyle | ✓ | T223 testleri: Unit `ImageProcessorTests.TheOrientationIsAppliedToThePixels_AndNoMetadataIsLeft`, `AMovingWebP_IsEncodedFromItsFirstFrame_AsOneFrame` (T339 yeni işlem eklemez; yüklenen görsel `ImageUploads`'tan geçer) |
| 6 | Alt metin isteğe bağlı, geri düşüşlü, boş kalmaz, görselleri ayırır | ✓ | Integration `TheAlternativeText_IsTheFirmsWhenWritten_AndGeneratedWhenEmpty`, `TheCardText_IsTheMainImagesFirmText_ElseTheProductName`, `AVariantsOwnImageText_NamesTheProductAndTheValues_UnlessTheGalleryHoldsTheSameImageWithAText`; Unit `ThePlacesOfTheImagesOfOneProduct_GiveDifferentTexts_AndSoDoTheVariantsOfOneProduct`, `TheGeneratedText_IsNeverEmpty_EvenForANameThatIsEmpty`, `AVariantsOwnImage_NamesTheProductAndTheOptionValuesInOrder`, `AGalleryImage_NamesTheProductAndItsPlaceOutOfTheCount`, `TheCardAndTheList_UseTheProductNameAlone` |
| 7 | Ada karakter tavanı; açıklama serbest | ✓ | Integration `TheNameTakesItsCeiling_AndOneMoreCharacterIsRefusedByTheDatabase` (22001; `CatalogCoreTablesTests.ProductNameLongerThanTheCeiling_IsRefused` mevcut), `TheDescription_HasNoCeiling_AndIsCleanedOnItsWayThroughTheDatabase` (100 K+), Unit `TheNameCeiling_IsTheProductConstant_AndTheModelCarriesIt`, `TheDescription_HasNoLengthCeiling` |
| 8 | Açıklama, kurumsal uzun metinler ve iki yasal metin tek kapalı set; ham HTML yok; set dışı düşer, `p` ve `br` kalır | ✓ (⚠ 2) | Tek set ve tek çağrı noktası: `RichTextSanitizer`/`RichText.Render`; Unit `RichTextSanitizerTests` (saldırı kümesi, beş biçim + `p`/`br`, bağlantı kuralları, eşkuvvetlilik, iş parçacığı, üst küme anlık görüntüsü, tam belge kümesi), `ProductTextTests`; mimari `RichTextSourceScan` (temizleyici türüne yalnız sınıf, `Html.Raw`/`HtmlString`/`MarkupString` yalnız gösterim yardımcısı; ayrı ihlal örneği `RawMarkupLeak.cs`); Integration `SetDescription` gidiş-dönüş |
| 9 | Garanti bilgisinin ayrı alanı yok | ✓ | Integration `NoTableOrColumn_CarriesAWarrantyOrAGuarantee` (information_schema); Unit `NoTypeMemberOrParameterOfTheApplication_IsAWarrantyOrAGuarantee` (+ tarama kendi testi) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| k1–k9 | ✓ | Üstteki tablo |
| KK1 varyantsız ürün, varyantlı varyant | ✓ | `AVariantWithoutItsOwnImage_ShowsTheProductsMainImage_AndAVariantWithOneShowsItsOwn` |
| KK2 ana kaldırılınca sıradaki ana | ✓ | `WhenTheMainImageLeaves_TheFirstImageInTheOrderBecomesMain` (DB'de tam bir ana: `MainCountAsync`) |
| KK3 sıra değişir | ✓ | `TheOrderOfTheList_IsTheOrderOfTheGallery` |
| KK4 format, boyut, adet aşılınca reddedilir | ✓ | Adet: `TheGallery_TakesTheCeilingAndRefusesOneMore_…` (21. görsel `LimitExceeded`, hiçbir şey yazılmaz), `…SetVariantImage` tavanda `VariantImageResult.LimitExceeded`; format/boyut: T223 testleri (k4 satırı) |
| KK5 meta veri atılmış, yön uygulanmış | ✓ | T223 `ImageProcessorTests.TheOrientationIsAppliedToThePixels_AndNoMetadataIsLeft` |
| KK6 üretilen metin dolar; varyant, galeri, kart ayrı metin | ✓ | k6 satırı |
| KK7 ad tavanı aşılınca kaydedilmez | ✓ | `TheNameTakesItsCeiling_…` (DbUpdateException 22001) |
| KK8 betik temizlenir, set dışı düşer, `p`/`br` kalır | ✓ | `AnAttack_LeavesNoTagNoAttributeAndNoDangerousScheme`, `AFormatOutsideTheSet_Falls_AndItsTextStays`, `ParagraphsAndLineBreaks_AreTheCarriersOfTheText_AreKept` |
| KK9 garanti alanı yok | ✓ | k9 satırı |
| `09 §25` ilgili satırlar | ✓ | Yeni dış bağımlılık: Dış Varsayımlar B1; tek-işlev (`RichTextSanitizer`) ve işlem kuralı (`ProductGallery` işlemde zorunlu, `SaveChanges` çağırmaz: `TheGalleryWrites_AreRefusedOutsideATransaction`); ayrı ihlal örneği her yeni tarama kuralı için var |
| Güvenlik taraması | ✓ | Temizleyici saldırı kümesi, `javascript:`/`data:`/`vbscript:`, kodlanmış şema, karışık harf, iç içe etiket; ham işaretleme taraması; yeni paketin açığı `NuGetAudit` temiz |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `SearchEngineData.cs` istisnası yalnız `HtmlString` için, tek dosya, sayım testi (`TheSearchHeadException_IsTheTwoHeadBuildersOfTodayAndGrowsByNothing_UntilD357MovesThem`, tam 2 yer; D-357); `GalleryMainMark.cs` tek deyimlik ayrı dosya, test tek toplu deyimi ve yalnız `ProductImages` olduğunu sınar, galerinin öteki dosyaları yer değil (`TheGalleryMainMarkPlace_HoldsExactlyOneBulkStatement_…`) |
| Sık FAIL 2: tarama bir çağrı biçimini kaçırıyor | ✓ | `HtmlString` her yazılışta (`\bHtmlString\b`), `Html.Raw`, `MarkupString`, `Ganss.Xss` adı; ihlal örneği `RawMarkupLeak.cs` her kuralı kendi satırında kırmızıya çevirir (`Scan_OnViolationSampleInAModule_ReportsEveryRuleOnItsLine`); tam satır yorumu bulgu değil |
| Sık FAIL 3: 06 varsayılanı / FK dizini | ✓ | Şema değişikliği yok (`product_images`, `variants.image_id` T104/T116'dan; `ix … is_main` tekil dizini kullanıldı) |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `9.0.892` için `grep` (md, cs, props, json): yalnız `09` Kütüphaneler satırı ve K-1315 kaydı (tarihsel kayıt, değişmez; K-1577 düzeltir); Aktif kalem 135 → 140 (D-357…D-361 beş yeni) |
| Sık FAIL 5: modül metodu SaveChanges çağırıyor | ✓ | `ProductGallery`/`GalleryMainMark` `SaveChanges` çağırmaz; Integration testlerde kaydı işlemin sahibi (`InTransactionAsync`) yapar; ret commit edilmez ve hiçbir şey yazılmadığı sınanır (`ARefusedWrite_NamesItsReason_AndWritesNothing`) |
| `09` değişikliği karar kaydıyla | ✓ | Kütüphaneler satırı K-1577'ye bağlı (`PRODUCT_DISCOVERY_STATUS.md`) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (tamamı) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 4159/4159 (Architecture, Consistency, Catalog, Security dahil); `GalleryMainMark` ve istisna sayım testi sonrası yeniden koşuldu |
| Integration `ProductGalleryTests` | ✓ | `--filter-class "*ProductGalleryTests"` | 18/18 |
| Integration Model ad alanı | ✓ | `--filter-namespace "Shopfolio.IntegrationTests.Model" --max-threads 3` | 690/690 |
| Integration Concurrency | ✓ | `--filter-namespace "Shopfolio.IntegrationTests.Concurrency"` | 26/26 |
| Integration `VersionedFormPageTests` / `SearchEngineDataTests` | ✓ | `--filter-class` | 21/21 · 18/18 (Description yazımı değişti) |
| Derleme | ✓ | `dotnet build -warnaserror` (Unit, Integration, Web) | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` (üç proje) | temiz |
| Restore | ✓ | `dotnet restore --locked-mode` | temiz; `NuGetAudit` uyarısı yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 2 (2026-11-08, head `8a5b0a9` + `origin/main` (`a89710b`, T321) birleştirmesi). Kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi (birleşik ağaçta `ProductGalleryTests` 18/18). Dal CI 42419057833 (head `8a5b0a9`) tüm adımlar yeşil. |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 yeni — yerelde düşen tek test `VersionedFormPageTests.OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8` (23505) zaten D-392'de (T339 dışı) |
| Tur 1 (geçmiş) | Doğrulama durumu: ✗ FAIL — tur 1 (2026-11-08, head `7d65caf`). Dalın kendi tabanında kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓ (dal CI 42411851018 yeşil; yerelde Unit 922 + 241 + 2996, Integration `ProductGalleryTests` 18/18, Concurrency 26/26, `SearchEngineDataTests` 18/18). `origin/main` (`af6e7d3`, T334) birleştirilince `ProductGalleryTests` 14/18 düşer: T334'ün `VariantStockGuard`'ı fiziksel varyantta stok numarası ister, testin üç tohumu (`Variant.Create(..., Now)`, satır 356, 368, 385) stoksuz varyant kurar. Birleştirme geri alındı, push edilmedi. Bulgu sayısı: 1 (B1 ORTA S2 — birleşik durumda `ProductGalleryTests` kırık) Düzeltme gerekli mi: Evet — `origin/main` birleştirilir, üç tohum T334'ün kalıbıyla stok alır (`Variant.Create(..., Now, 100)`), birleşik durumda `ProductGalleryTests` ve dalın dokunduğu Integration sınıfları yeniden koşulur Bloke-etmeyen not (K) sayısı: 2 — K1 → D-383 (`VersionedFormPageTests.OrderPage_…_KK2` yerelde main'de de düşüyor), K2 → D-384 (yanıltıcı birim test adı) |

**Doğrulayıcı uyuşmazlık notları (adım 13):** (1) Notlar'daki ana dal run'ı 42408991723 `failure` değil `cancelled`'dır (eşzamanlılık iptali; aynı içeriği taşıyan `af6e7d3` CI 42411903829 yeşil). (2) Test Sonuçları'ndaki `VersionedFormPageTests` 21/21 yerelde yinelenmedi: `OrderPage_TheAdministratorsFormCarriesTheVersion_TheCustomersCarriesOnlyTheShownOpenQuantity_KK2` `legal_text_versions` tekil dizinine takılıyor; aynı test `origin/main`'de de aynı biçimde düşüyor (T339 dışı; D-383). (3) Öz denetim tablosu tam ve kanıtlı.

**Doğrulayıcı kanıtları (tur 2, birleşik ağaç: dal + `origin/main` `a89710b`):** başlangıç kapıları — working tree temiz, `core.hooksPath` git-hooks, ana dal son 3 run 42419095630, 42411903829, 42411903876 `success`, `MEMORY.md`'de T339 satırı var. Restore ✓, build `-warnaserror` 0 uyarı 0 hata, format üç proje temiz; Unit 3032 (Architecture ve Consistency dışı) + Architecture 943 + Consistency 241 geçti; betik testleri 4 dosya geçti. Integration (PostgreSQL 18): `ProductGalleryTests` 18/18 (k1, k2, k3, k4 adet, k6, k7, k9), `ConcurrencyTests` + `SearchEngineDataTests` 44/44, `GalleryLinkTablesTests` + `CatalogCoreTablesTests` + `SampleDatabaseTests` + `StockAndCapacityTests` 144/144, `ImageFilesTests` 14/14 (k4 boyut), Unit `ImageProcessorTests` 13/13 (k4 tür, k5), `VersionedFormPageTests` + `ProductTransitionsTests` 61/62 (D-392). Güvenlik: `RichTextSanitizer`'a repoya girmeyen 24 ek örnek (olay niteliği + izinli bağlantı, `&colon;`, sekme ve hex varlıklı şema, ters eğik çizgili ağ yolları, `noscript`/`template`/`svg style`/`select style`/`xmp` mXSS, `base`/`link`/`meta`, `iframe srcdoc`, `data-`/`aria-`/`dir` nitelikleri) — çıktıda izinli dışı etiket, olay niteliği, izinli dışı şema ve `href`/`rel` dışı nitelik yok; ikinci süzme ve `RichText.Render` aynı çıktı. `12`: KP-5 (VAL-A012…A019) ve KP-42 (VAL-A186…A190) satırları uç yanıtı / tarayıcı gözlemi ister, sayfa T503/T525'te — `Durum` `BEKLEMEDE` kaldı. Uyuşmazlık: yok; öz denetim tabloları (ilk ve düzeltme 1) tam ve kanıtlı.

## Düzeltme 1 (tur 1 FAIL sonrası, 2026-11-08)

**Yapılan:** `origin/main` (`af6e7d3`, T334) dala birleştirildi (rebase yok). B1 giderildi: `ProductGalleryTests.cs` içindeki üç stoksuz `Variant.Create(..., Now)` çağrısı T334'ün kalıbıyla `Variant.Create(..., Now, 100)` oldu; ürün koduna dokunulmadı. Birleşik ağaçta düşen başka test yok. K2 (D-384) kapatıldı: `ProductGalleryRulesTests` testi `TheFirstImageAdded_ComesAsTheMainImage_WhenTheFormMarksNone` olarak yeniden adlandırıldı. Kayıt belgesi çakışmaları (`IMPLEMENTATION_STATUS` Son güncelleme: yalnız en son giriş, T334 doğrulama girişi ve T339 tur 1 FAIL girişi `STATUS_CHANGELOG`'a; `DEFERRED_BACKLOG` Aktif kalem yeniden sayıldı) çözüldü. K-1577 `origin/main`'de başka kayda verilmemişti, numara aynı kaldı.

**Öz denetim (birleşik ağaçta, 11'deki k1–k9 ve KK1–KK9 yeniden):**

| Madde | Sonuç | Kanıt (birleşik ağaç, head üstü) |
|---|---|---|
| k1 / KK1 varyant kendi görseli, yoksa ürününki | ✓ | `ProductGalleryTests` 18/18 (`AVariantWithoutItsOwnImage_…`, `AVariantOfAProductWithoutImages_ShowsNone`) |
| k2 / KK2 ana işaret sıradan bağımsız; kaldırılınca sıradaki ana | ✓ | `ProductGalleryTests` 18/18; Unit `ProductGalleryRulesTests` |
| k3 / KK3 galeri sırası | ✓ | `TheOrderOfTheList_IsTheOrderOfTheGallery` |
| k4 / KK4 format, boyut, adet tavanı | ✓ | `TheGallery_TakesTheCeilingAndRefusesOneMore_…`, `TheVariantsImages_CountWithTheGalleryOnes_…` (18/18); T223 Unit testleri (3019 içinde) |
| k5 / KK5 yeniden kodlama, yön, meta veri | ✓ | T223 `ImageProcessorTests` (Unit 3019/3019) |
| k6 / KK6 alt metin | ✓ | `TheAlternativeText_…`, `TheCardText_…`; Unit `ImageAltText` testleri |
| k7 / KK7 ad tavanı, açıklama serbest | ✓ | `TheNameTakesItsCeiling_…`, `TheDescription_HasNoCeiling_…`; `CatalogCoreTablesTests` |
| k8 / KK8 tek kapalı set | ✓ | Unit `RichTextSanitizerTests` + `RichTextSourceScan` (Architecture 943/943) |
| k9 / KK9 garanti alanı yok | ✓ | `NoTableOrColumn_CarriesAWarrantyOrAGuarantee`; Unit `NoTypeMemberOrParameterOfTheApplication_…` |
| `09 §25` ilgili satırlar | ✓ | Değişmedi; Consistency 241/241, Architecture 943/943 |
| Sık FAIL: main'in yeni zorunlu alanını tohumlamama | ✓ | Birleşik ağaçta `ProductGalleryTests`, `StockAndCapacityTests`, Model ve Web ad alanları koşuldu |

**Birleşik ağaç sınaması:** restore ✓; build `-c Release -warnaserror` 0 uyarı 0 hata; format 3 proje temiz; Unit 943 (Architecture) + 241 (Consistency) + 3019 (öteki) hepsi geçti; Integration `ProductGalleryTests` 18/18, `StockAndCapacityTests` 27/27, Model ad alanı 1123/1123 (parçalı: 18 + 27 + 182 + 213 + 234 + 449), Concurrency 26/26, Web ad alanı parçalı: 151 + 174 + 103 + 134 (+ 1 düşen, D-392) + `LivenessEndpointTests` 12 (aşağıya bak). Kalan düşme: `VersionedFormPageTests.OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8` (23505, sıra bağımlı sayaç; D-383 ailesi, T339 dışı → D-392).

**⚠ Düzeltme turu:** (1) `LivenessEndpointTests` 12 testi geçer ama test süreci yerelde kapanmaz (100 sn `timeout` ile öldü); T339 dışı → D-391. (2) Model ad alanının ilk parçalı koşusunda (10 sınıf, 141 sn) bir test düştü (ayrıntı kesildi); aynı sınıflar iki yarıda yeniden koşulunca 182 + 213 hepsi geçti — geçici dalgalanma, yinelenmedi.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Var — `HtmlSanitizer` 10.0.1120 (MIT), geçişli `AngleSharp` 1.9.0, `AngleSharp.Css` 1.3.1 (MIT); `09` Kütüphaneler tablosu ve K-1577

## Commit & PR

- **Branch:** `task/T339-urun-gorsel-metin`
- **Commit:** `e950c0c` — T339: Ürün görseli ve metni: rapor, K-1577, backlog D-357..D-361, durum kayıtları (PR kaydı sonrası commit ile ilerler)
- **PR:** #445
- **CI:** *(yönetici izler)*
- **Post-merge run'lar:** squash `9bb46eb` (#445) — CI 42423723835 success, Publish 42423723933 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| B1 | `HtmlSanitizer` sürümü, lisansı ve geçişli bağımlılığı | 9.0.892 `AngleSharp` 0.17.1'e `[exact]` bağlı ve o sürüm GHSA-pgww-w46g-26qg taşır (`dotnet restore` `NU1902`, `NuGetAudit` hata); 10.0.1120 `AngleSharp` 1.9.0 + `AngleSharp.Css` 1.3.1 çeker (`packages.lock.json`); nuspec lisans ifadesi üçünde MIT (okundu, erişim 2026-11-07) | ✓ — K-1577 |
| B2 | Kütüphane `net10.0` altında çalışır | `net10.0` derleme ve 64 Unit testi (saldırı kümesi, eşkuvvetlilik, iş parçacığı) geçer | ✓ |
| B3 | Ara başlık etiketi `h3` | `04`'ün başlık sırasında çelişen kural yok (sayfa başlığı `h1`, bölüm `h2`) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Arama motoru başlığı `HtmlString` kuruyor; istisna dar | D-357 | T363'te `StructuredData.Render`'a taşınır |
| 2 | Panel ürün formu galeri/açıklama işlemini çağırmıyor | D-358 | Ekran T525'in işi |
| 3 | Vitrin kart/liste/ürün sayfası alt metin ve açıklama gösterimi | D-359 | Ekran T503'ün işi |
| 4 | Kurumsal uzun metin alanları temizleyiciyi çağırmıyor | D-360 | T358 alan yazımı; D-219 yasal metinler için |
| 5 | `GalleryMainMark` toplu deyimi sürüm bekçisini görmez | D-361 | Ürün formu işlemi T525'te sürüm testi |

## Notlar

- **Working tree (Adım -1):** temiz (önceki işçinin devri `git status` temiz; kendi worktree'm)
- **Hooks:** `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** son 3 run (`gh run list --branch main`): 42408991705 success, 42408991723 **failure** (924ef92, "4. Integration test"; T339 dışı, yöneticiye bildirildi), 42405002224 success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ 1** HtmlSanitizer 10.0.1120 (K-1577; 09 tablosu 9.0.892 diyordu; neden açık denetimi).
- **⚠ 2** Kriter 8 yorumu: "tek kapalı biçim setini kullanır" = tek set tanımı, tek yazım ve gösterim çağrı noktası kuruldu ve ürün açıklaması uçtan uca bağlandı; kurumsal uzun metin alanları (5 alan) ve iki yasal metin (T366, D-219) yazım işlevi o task'larda temizleyiciyi çağıracak (D-360, D-219). Gösterim her durumda `RichText.Render` ile yeniden süzer.
- **⚠ 3** Adet sayımı: aynı görsel galeri ve varyantta bir sayılır (belge "birlikte"; dosya başına).
- **⚠ 4** Birden çok ana işaretli listede ilk işaretli kazanır (ret değil).
- **⚠ 5** `RichTextSourceScan`: `SearchEngineData.cs` için `HtmlString` istisnası, sayım testiyle tam 2 yer (D-357).
- **⚠ 6** `GalleryMainMark` tek toplu deyim: tek ana dizini iki izlenen güncellemeyi sırasız gönderince ara adımda çakışır; deyim `VersionedSideBulkWriteScan.AllowedPlaces`'e tek dosyalık yer olarak girdi (kapalı liste dar kalır: test tek deyimi ve yalnız `ProductImages`'i sınar). Sürüm bekçisi güvencesi aynı kayıtta başka bir bağın değişmesidir (D-361).
- **⚠ 7** K-1577 numarası: `origin/main`'de son K-1576; başka açık task aynı numarayı almışsa birleşmede yeniden numaralanır.
