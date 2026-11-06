# T219 — Arama motoru verileri kalıbı

**Faz:** F2 | **Durum:** ✓ Doğrulama tur 1 PASS (merge bekliyor) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Infrastructure/Slug.cs`: `Slug.From(name, rootName)` (ortak `TextNormalizer.Simplify` sadeleştirmesi, `[^a-z0-9]+` tek tire, boşsa kök ad), `FirstFree` (çakışmada 2'den başlayan sayı), `IsBaseOrNumbered`, `ForSaveAsync` (`SlugFixedAt` doluysa dokunulmaz ve tablo okunmaz), `StockCodeSuggestion.Suggest` (adresten stok kodu önerisi; aynı sadeleştirme).
- `ITransactionRunner.RunAsync(work, retryOnUniqueIndexes, ct)` aşırı yüklemesi: yalnız adı verilen tekil dizinin `23505` ihlalinde işlemin bütünü yeniden yürütülür, ilk deneme dahil en çok 3 (`UniqueConflictAttempts`); başka dizin ya da başka hata hemen fırlar. `TransactionRunner` sınıf özeti ("no retry") bu aşırı yüklemeyi anacak biçimde güncellendi.
- `Infrastructure/SearchEngine/`: `SearchEngineData` (JSON-LD ürün, firma, kırıntı yolu, SSS; `Head`: başlık, açıklama, Open Graph, ana görsel yoksa logo), `SearchText` (başlık `ad – marka`, açıklama, sözcükten kesme, düz metne çevirme), `SiteAddress` (mutlak adres), `SearchEngineClosure` (kapalı sayfa kararı + ara katman: HTML olmayan kapalı yanıtta tek `X-Robots-Tag: noindex`), `SearchEngineSetup` (`Program.cs` kaydı).
- `ProductConstants.ShortTextMaxLength` (P-28, 200); `ContentRules.ShortDescriptionMaxLength` buna bağlandı (aynı sayının ikinci kopyası kalmadı).
- Test ana bilgisayarında örnek sayfalar: `Pages/SearchDataSamples.cs`, `Pages/Storefront/SearchDataSample.cshtml` (`/hizmet/{slug}`), liste ve panel örneklerine `@Model.Head`.
- Mimari taramalar: `SearchEngineSourceScan` (tek katlama, arama motoru/adres form alanı yok, site haritası/robots adı yalnız `AddressRules`'ta, robots işareti yalnız `SearchEngine/` + `CsvFileResult`), ihlal örnekleri `ViolationSamples/SearchEngine/`, `Architecture/SearchDataPagePattern.cs` (gerçek `Shopfolio.Pages.Storefront` ad alanında örnek kullanım; `ModuleBoundaryTests`).
- Belgeler: `09 §9.4` madde 6 (yeni) + `§25` satırı; `CONTEXT.md` klasör satırı; T217 raporu "Post-merge run'lar" ve status Commit hücresi dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/{Slug.cs, ITransactionRunner.cs, TransactionRunner.cs, ProductConstants.cs}`, `Infrastructure/SearchEngine/*`, `Modules/CorporateContent/ContentRules.cs`, `Program.cs`
- `tests/Shopfolio.UnitTests/{Infrastructure,Architecture}/…`, `tests/Shopfolio.IntegrationTests/{Web/SearchEngineDataTests.cs, Pages/*}`
- `Docs/09_CODING_GUIDELINES.md`, `.claude/CONTEXT.md`, status ve rapor dosyaları

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Adres addan sunucuda üretilir, sadeleşir, sayı eklenir, ilk yayında sabitlenir | ✓ | `Slug.From/FirstFree/ForSaveAsync`; `SlugTests` (From_Folds…, FirstFree_ATakenBase…, ForSaveAsync_AFixedAddress…) |
| 2 | Sadeleştirme aramanın ve stok kodunun ortak işlevi | ✓ | `SearchEngineSourceScanTests.TheThreeUsersOfTheFolding…`, `TheCommonFolding_IsTheSameResult…` |
| 3 | Başlık ve açıklama ilk yanıtta | ✓ | `SearchEngineDataTests` (Integration) `RecordPage_TheFirstHtml…` (TestServer ve Kestrel) |
| 4 | JSON-LD: ürün, firma, kırıntı yolu, SSS | ✓ | `SearchEngineData.Product/Organization/Breadcrumbs/Faq`; Unit `SearchEngineDataTests.Product_…`, `Organization_…`, `Breadcrumbs_…`, `Faq_…` |
| 5 | Open Graph; ana görsel yoksa logo | ✓ | `Head_APageWithoutAMainImage_UsesTheLogo`; Integration `RecordPage_WithoutAMainImage_TheOpenGraphImageIsTheLogo` |
| 6 | Değerler kayıttan; elle SEO alanı yok | ✓ | `RecordPage_ThePriceAndTheStockStatusFollowTheRecord…`; `ApplicationTypes_HaveNoSearchEngineField…`; `PanelPages_HaveNoSearchEngineField…` |
| 7 | Site haritası ve robots.txt kurulmaz | ✓ | `SitemapAndRobotsTxt_HaveNoEndpointInThisTask`; `TheApplicationsEndpoints_HaveNoSitemapAndNoRobotsFileYet…` |
| 8 | Kapalı sayfa ve panel `noindex` (HTML'de meta, HTML olmayanda `X-Robots-Tag`) | ✓ | `ClosedPages_CarryTheRobotsMetaTag…`, `ClosedAnswersThatAreNotHtml_CarryOneXRobotsTag…` |

### Öz denetim (bitiş kapısı 2)

Bütün maddeler bu turda kendi diff'imde yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: "Çiçekli Şal" → `cicekli-sal`; çakışmada sayı; yayından sonra değişmez | ✓ | `SlugTests.From_FoldsTurkishLetters…` (veri satırı), `FirstFree_ATakenBase…`, `ForSaveAsync_AFixedAddress_NeverChanges…`; Integration `TwoRecordsMadeWithTheSameNameAtTheSameTime_GetCelikKapiAndCelikKapi2`, `TheAddressOfAFixedRecord_DoesNotChangeWhenTheNameDoes` (gerçek PostgreSQL) |
| KK2: üç kullanım aynı işlevi çağırır | ✓ | `SearchEngineSourceScan` (tek katlama kuralı) + `TheFoldingRule_SeesEveryCallFormOfTheDecomposition` (çağrı biçimleri) + ihlal örneği `SearchEngineMisuse.cs` |
| KK3: başlık ve açıklama ilk HTML'de | ✓ | Integration `RecordPage_TheFirstHtml…` |
| KK4: dört türün JSON-LD'si geçerli, alanlar kayıttan | ✓ | Unit `SearchEngineDataTests` (geçerli JSON, `AValueThatHoldsAScriptEnd_CannotLeave…`); Integration kayıttan değer testi |
| KK5: görselsiz sayfada logo | ✓ | `Head_APageWithoutAMainImage_UsesTheLogo`, Integration `…OpenGraphImageIsTheLogo`, yedeksiz durum `…WithoutALogo_HasNoOpenGraphImage` |
| KK6: panelde SEO alanı yok | ✓ | `PanelPages_HaveNoSearchEngineFieldAndNoAddressField`; kaynak taraması `SeoFields.cshtml` ihlal örneği |
| KK7: `/sitemap.xml` ve `/robots.txt` ucu yok | ✓ | `SitemapAndRobotsTxt_HaveNoEndpointInThisTask` (T313 bu testi değiştirir), `TheApplicationsEndpoints_…` |
| KK8: panel sayfasında ve `?minPrice=1` listesinde `noindex` | ✓ | `ClosedPages_CarryTheRobotsMetaTag…` (panel, süzgeçli liste, arama; açık sayfalarda yok) |
| `09 §25` mimari/kod: klasör düzeni + `CONTEXT.md` | ✓ | `CONTEXT.md` yeni satır; `09 §9.4` madde 6 |
| `09 §25`: modül sınırı (sayfada `DbContext` yok; Pages ad alanında kalıp) | ✓ | `Architecture/SearchDataPagePattern.cs` + `ModuleBoundaryTests` iki yeni test |
| `09 §25`: yeni sabit/ayar §3.5 tablosunda | ✓ | `UniqueConflictAttempts=3` zaten §9.4 m3 + §3.5; `ShortTextMaxLength` P-28 `ProductConstants`'ta; yeni ayar yok (`.env.example` değişmedi: `SiteOptions.Domain` mevcut) |
| `09 §25`: zaman `TimeProvider` | ✓ | yeni kodda saat okuması yok |
| `09 §25`: işlem ve eşzamanlılık (tekil dizin yeniden denemesi, ChangeTracker.Clear) | ✓ | Integration `RunAsyncWithRetry_*` (dört test); `TransactionBody` ihlal örneği retry aşırı yüklemesi (`OutsideCallInTransaction.cs`) |
| `09 §25`: veri/göç (yeni tablo, FK dizini, varsayılan) | ✓ | T219 tablo, sütun, göç eklemedi; FK dizini ve DB varsayılanı konusu yok |
| `09 §25`: güvenlik — JSON-LD'den çıkış, HTML kodlama | ✓ | `AValueThatHoldsAScriptEnd_CannotLeaveTheJsonLdBlock`, `Head_AValueWithQuotesAndTags_IsEncoded…` |
| Güvenlik taraması: sır, yetki, girdi, bağımlılık | ✓ | sır/yeni paket yok; mutlak adres `SiteOptions.Domain`'dendir, istek ana bilgisayarından değil (`SiteAddress_IsHttpsAndTheInstallationDomain_NeverTheRequestHost`); kapalı karar adrese bakar, yetkiyi değiştirmez; `X-Robots-Tag` ara katmanı tek değer yazar |
| Üst dokümanla çelişki: `05 §3.7.2/§3.7.4`, `02 §3.30.5/§3.30.6`, `07 §4.32.2` | ✓ | Karşılaştırıldı; açıklama sınırı P-28 (200), sözcük ortasında kesme yok, robots.txt engellemez ve ucu T313'ün; yeni karar kaydı gerekmedi |
| Kural değişince aynı sayıyı/adı anan yerler | ✓ | `TransactionRunner` özeti güncellendi; `ContentRules.ShortDescriptionMaxLength` → `ProductConstants.ShortTextMaxLength`; `ThePageDescriptionCeiling_IsTheShortFieldCeilingOfTheContentRecords` tutarlılık testi; `DatabaseSessionSourceScanTests` beklentisi |
| Bekçi/tarama istisnası dar | ✓ | `TheRulesExceptions_AreOneFileEach…`, `TheRobotsException_IsNotAFolder…` |
| Taramanın ayrı ihlal örneği ve çağrı biçimleri | ✓ | `ViolationSample_Code_…`, `ViolationSample_Razor_…`, `TheFoldingRule_SeesEveryCallFormOfTheDecomposition`, `TheSearchEngineFieldRule_SeesIdentifiersColumnsAndFormFieldsInEveryNamingStyle…` |
| Davranış Kestrel'de de sınandı | ✓ | Integration testlerde `kestrel` parametresi |
| Hedefi T219 olan backlog kaydı | ✓ | `grep -n "T219" Docs/DEFERRED_BACKLOG.md` boş; kapatılacak/ileri atılacak kayıt yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --no-build` | total 2773, failed 0 (T219 öncesi 2640; yeni testler SlugTests, SearchTextTests, SearchEngineDataTests, SearchEngineSourceScanTests, ModuleBoundaryTests +2) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release --no-build --filter-class/--filter-namespace …` (parçalı koşu, gerçek PostgreSQL 18 konteyneri; 1715 test keşfedildi, hepsi koştu) | total 1715, failed 0 (T219 öncesi 1695; yeni 20: `Web/SearchEngineDataTests`). Parçalar: Web 528, Security 32, Pages/Concurrency/Disposal/FileSecrets/Logging/Outages/SignedEndpoints 117, Jobs/Files/Database 145, Model 893 |
| Build | ✓ | `dotnet build -c Release -warnaserror` | 0 uyarı, 0 hata |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-06, head `f4e4505`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-157) |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; main son üç run 42276704951, 42276704953, 42230581953 `success`; hafızada T219 satırı var.

**Kriterler 1–8 ✓, KK1–KK8 ✓, `09 §25` ✓.** Kanıtlar (doğrulayıcı):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `Slug.From` = `Simplify` → `[^a-z0-9]+` tek tire → boşsa kök (`02 §3.30.2`, K-1070); `FirstFree` 2'den; `ForSaveAsync` `fixedAt` doluysa tabloyu okumaz, adresi korur. `SlugTests` ("Çiçekli Şal" → `cicekli-sal`); eşzamanlılık: `TwoRecordsMadeWithTheSameNameAtTheSameTime_GetCelikKapiAndCelikKapi2` (engel ile iki işlem aynı tabanı okur, kaybeden `ix_products_slug` `23505` ile geri alınır, yeniden denemede `celik-kapi-2`; 3 deneme), başka dizindeki `23505` yeniden denenmez, kalıcı çakışma 3 denemeden sonra fırlar ve `ChangeTracker` boş kalır. Yeniden deneme `RunAsync`'in geri alma + `ChangeTracker.Clear()` yolunu kullanır; yeni aşırı yükleme `TransactionBody` taramasının kapsamında (ihlal örneği satır 36, 38) |
| Kriter 2 / KK2 | ✓ | `Simplify`'ın uygulamadaki çağıranları yalnız `SearchTerm.cs`, `Slug.cs`, `Product.cs` (grep); stok kodu önerisi `Slug.From`'un çıktısından başlar; `TheCommonFolding_IsTheSameResult…` dört kullanımı aynı sonuçla sınar; katlama/adres deseni başka dosyada kırmızı (ihlal örneği) |
| Kriter 3 / KK3 | ✓ | `SearchEngineData.Head` başlık ve açıklamayı sunucu HTML'ine yazar; `RecordPage_TheFirstHtml…` (TestServer + Kestrel) |
| Kriter 4 / KK4 | ✓ | `Product` (fiyat `offers.price`, `TRY`, `InStock`/`OutOfStock` kayıttan), `Organization`, `BreadcrumbList`, `FAQPage`; `@context` `https://schema.org`; `RecordPage_ThePriceAndTheStockStatusFollowTheRecord…` (fiyat/stok değişince JSON-LD izler). XSS: `JavaScriptEncoder.Create` `<`, `>`, `&`, `'`, `+`'yı her zaman kaçırır; `AValueThatHoldsAScriptEnd_CannotLeave…`, `Head_AValueWithQuotesAndTags…` |
| Kriter 5 / KK5 | ✓ | `og:image` = ana görsel ?? logo; `RecordPage_WithoutAMainImage_TheOpenGraphImageIsTheLogo` (TestServer + Kestrel), logosuz hâlde `og:image` yok |
| Kriter 6 / KK6 | ✓ | değerler çağıranın verdiği kayıttan; `SearchEngineSourceScan` (SEO alan adı, `Slug` form alanı) + ihlal örnekleri; `PanelPages_HaveNoSearchEngineFieldAndNoAddressField` |
| Kriter 7 / KK7 | ✓ | `sitemap.xml`/`robots.txt` adı yalnız `AddressRules.cs`'te (tarama); `SitemapAndRobotsTxt_HaveNoEndpointInThisTask` (uç listesi + 404) |
| Kriter 8 / KK8 | ✓ | HTML'de `<meta name="robots" content="noindex">` (kapalı sayfada başka arama motoru etiketi yok); HTML olmayan kapalı yanıtta ara katman `OnStarting`'de başlığı **atar** (ekleme değil) → CSV dahil tek değer; `ClosedPages_…` (panel, `?minPrice=1`, arama; `page=2` açık), `ClosedAnswersThatAreNotHtml_…` (yönlendirme, CSV, panel 404). Kapalı ilk parçalar `07 §4.32.2` ve `02 §3.30.6` ile uyumlu (sepet, ödeme, sipariş takibi, `siparis` kökü — `{kök}/teyit` dahil —, hesap sayfaları, panel, arama, indirme); taslak/arşiv/süzgeç sayfanın işaretiyle |
| `09 §6` modül sınırı | ✓ | `SearchDataPagePattern.cs` gerçek `Shopfolio.Pages.Storefront` ad alanında; `ModuleBoundaryTests` iki yeni test (sayfa kuralı temiz, yalnız `Catalog.Contracts`; kalıp hiçbir modül türüne bağlı değil) |
| Mutlak adres | ✓ | `SiteAddress` = `https://` + `SiteOptions.Domain` (alan adı açılışta yalnız ana bilgisayar adı olarak doğrulanır); istek `Host`'u kullanılmaz (`09 §11.4` m.2 ile uyumlu) |
| Tarama istisnaları dar | ✓ | robots istisnası tek dosya (`CsvFileResult.cs`, gerekçeli); katlama `TextNormalizer.cs`, adres deseni `Slug.cs`, protokol adları `AddressRules.cs` — her biri tek dosya; `TheRobotsException_IsNotAFolder…` |
| Belge uyumu | ✓ | `09 §9.4` m.6 ve `§25` satırı `05 §3.7.2`/§3.7.4, `02 §3.30.5`/§3.30.6, `07 §4.32.2` ile karşılaştırıldı; üst belgeden ayrılan cümle yok (kategori açıklaması kategori adı + marka adı taşır; başlık `ad – marka`; açıklama P-28 sınırında sözcükten kesilir) — karar kaydı gerekmez. Yeniden deneme sabiti (3) `09 §3.5` tablosunda zaten var (K-1336). `ContentRules.ShortDescriptionMaxLength` → `ProductConstants.ShortTextMaxLength` (P-28, 200) `09 §3.5` m.1'in "aynı P satırına tek sabit" kuralına uyar; `src`'de 200'ün başka kopyası yok |
| Güvenlik | ✓ | sır yok; yeni paket yok (csproj değişmedi); yetki değişmedi (kapalı kararı yalnız başlık/meta yazar); girdi: kayıt değerleri JSON ve HTML kodlamasıyla çıkar, `sameAs` yalnız http(s) |

**Testler (doğrulayıcı, yerel):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` temiz; Unit 2773/2773; betik testleri 4/4; Integration yerelde yalnız `ClosedAnswersThatAreNotHtml_…` (TestServer + Kestrel) 2/2 — her Integration koşusu göç paketini yeniden kurar (yerelde 2 dk 32 sn) ve 170 sn sınırına sığmadı (K1). Integration'ın tamamı dal CI'ında: run 42287139738 (head `f4e4505`), "4. Integration test" 1715/1715 (PostgreSQL 18), bütün işler yeşil. Koşu sonrası `git status` temiz, `dotnet restore --locked-mode` çıkış 0 (göç paketinin kurulumu `packages.lock.json`'a `win-x64` bölümü ekliyordu; geri alındı).

**Task satırları (`12 §7.5`):** `KP-37` grep'i 38 satır getirir (VAL-A373…A407, VAL-D056…D058; `11 §7.1`: KP-37 → T219, T313, T363). Beklenen sonuçları gerçek ürün/kategori/içerik sayfasının, panel formunun, site haritası ve `robots.txt` ucunun uç yanıtıdır; T219 yalnız kalıbı ve test ana bilgisayarındaki örnek sayfaları kurar → hepsi `BEKLEMEDE` kalır (T210 emsali). `KD-48`, `MK-18` satırı yok. Matris betiği hata 0.

**K1 → D-157:** Integration test derlemesi her koşuda göç paketini `dotnet ef migrations bundle` ile yeniden kurar; yerelde tek koşu bu yüzden 170 sn'yi aşar ve doğrulayıcı Integration'ı parçalayarak koşamaz.

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Tek fark: yapım raporu Integration'ı yerelde parçalı koşmuş (1715); doğrulayıcı ortamında koşu süresi yüzünden CI sonucuna dayanıldı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok — yeni tablo, sütun, dizin ya da göç yok (FK dizini ve DB varsayılanı konusu doğmadı)
- **Config / ortam değişkeni:** Yok (`SiteOptions.Domain` mevcut)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T219-arama-motoru-verileri`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #413
- **CI:** yapım chat'i beklemedi (yönetici düzeni); yönetici izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `/sitemap.xml` ve `/robots.txt` ucu; T313 bu task'ın `SitemapAndRobotsTxt_HaveNoEndpointInThisTask`, `TheApplicationsEndpoints_…` testlerini ve robots tarama kuralını (ProtocolFileOutsideRules) değiştirecek | — (11'de T313 ve T229 kabul kriteri) | Kabul kriteri 7: bu task'ın işi değil |
| 2 | Gerçek ürün/kategori/kurumsal sayfalarının `Head` kullanımı | — (11'de ilgili sayfa task'ları) | Bu task kalıbı ve test ana bilgisayarındaki örneği kurar |

### ⚠ Kolay kararlar (öneriyle verildi)

1. Kategori açıklaması cümlesi uygulamanın kendi metni (`SearchText.CategoryDescription`); 02 §3.30.5 "kategori adından ve marka adından" der.
2. Kapalı sayfa HTML'de yalnız başlık ve robots meta taşır (Open Graph ve JSON-LD yok).
3. Kapalı ilk yol parçaları: sepet, odeme, siparis-takibi, arama, sipariş alanı, hesap bağlantıları, panel, indir.
4. Mutlak adres `SiteOptions.Domain` + https; istek ana bilgisayarı kullanılmaz.
5. HTML'de başlık meta etiketiyle, HTML olmayanda `X-Robots-Tag: noindex` (tek değer).
6. `ContentRules.ShortDescriptionMaxLength` `ProductConstants.ShortTextMaxLength`'e (P-28) bağlandı.
7. En tire (`–`) HTML'de `&#x2013;` olarak kodlanır (geçerli HTML).
8. Örnek ürün sayfası test rotası `/hizmet/{slug}`.

## Notlar

- **Working tree (Adım -1):** temiz (devam 2 işçisi `git status --short` boş buldu)
- **Ana dal CI startup (Adım 0):** main run'ları 42276704953 (Publish), 42276704951 (CI), 42230581953 (Publish) success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** T217 raporunun "Post-merge run'lar" satırı ve status Commit hücresi (`d2b5379`) bu dalda dolduruldu; hooksPath `scripts/git-hooks`.
