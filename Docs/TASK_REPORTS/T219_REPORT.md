# T219 — Arama motoru verileri kalıbı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-06

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
| Unit | UNIT_RESULT | `dotnet test --project tests/Shopfolio.UnitTests -c Release --no-build` | UNIT_SUMMARY |
| Integration | INT_RESULT | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release --no-build --filter-namespace …` (ad alanı parçaları ayrı, PostgreSQL 18 konteyneri) | INT_SUMMARY |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | (doğrulama chat'i doldurur) |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok — yeni tablo, sütun, dizin ya da göç yok (FK dizini ve DB varsayılanı konusu doğmadı)
- **Config / ortam değişkeni:** Yok (`SiteOptions.Domain` mevcut)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T219-arama-motoru-verileri`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #PR_NO
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
