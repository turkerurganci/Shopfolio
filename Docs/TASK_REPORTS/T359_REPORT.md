# T359 — Kurumsal içerik: ürün bağı, elle sıralama, zorunlu alanlar, alan tavanları, kapsam dışı yetenekler

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-08

> Taslağı yapım işçisi doldurur; doğrulama finalize eder.

---

## Yapılan İşler

- **Galeri ortak kuralları** `Infrastructure/GalleryText` (ana işaret, boş alt metin yerine "Ad — görsel p / n", firma metni kırpılır): Catalog `GalleryRules`/`ImageAltText` ona devredildi (tek işlev, `09 §25`).
- **Ürün bağı** `ContentProductLinks` (hizmet ve referans iş): ürün düzleminde, varyantsız; sıra seçim sırası; tekrar ya da olmayan ürün bütün listeyi reddeder; taslak ürün bağda kalır. `ICatalogModule` iki okuma üyesi aldı (`ListPublishedProductsAsync`: yalnız yayındaki kartlar, `FindProductIdsAsync`); ters bağ ve öneri üyesi yok.
- **Elle sıra** `ContentOrdering`: beş liste (hizmet, referans, SSS, şube, genel sayfa); koşullu `ExecuteUpdate` (okunan değer değişmişse 409), eşit değerde bütün liste yeniden numaralanır, sürüm artmaz, menü ve ana sayfa önbelleği düşer; Hakkımızda ve duyuru `NotOrderable`.
- **Galeri** `ContentGallery` (Hakkımızda, hizmet, referans, SSS, genel sayfa) + şube tek görsel: P-25 tavanı, tekrar/yok görsel reddi, tek ana işaret, etkin alt metin. Kayıt sonrası `CacheArea.HomepageBlocks` düşer (D-498 kapandı).
- `ContentReaders.ListOfferingsAsync/ListPortfolioItemsAsync` (yayındakiler, elle sıra, 24/sayfa), `ContentMainImages` (HomepageReader'ın özel kopyası ortaklaştı).
- Testler: Unit `ContentRelationsTests` (19), Integration `ContentRelationsTests` (35).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/GalleryText.cs`; `Modules/Catalog/` (`CatalogModule`, `Contracts/ICatalogModule`, `GalleryRules`, `ImageAltText`, `ShowcaseReader`)
- `src/Shopfolio.Web/Modules/CorporateContent/` (`ContentProductLinks`, `ContentOrdering`, `ContentGallery`, `ContentMainImages`, `ContentReaders`, `HomepageReader`, beş `*Image` varlığı, `CorporateContentModule`)
- `tests/Shopfolio.UnitTests/CorporateContent/ContentRelationsTests.cs`, `tests/Shopfolio.IntegrationTests/CorporateContent/ContentRelationsTests.cs`, `tests/Shopfolio.UnitTests/Infrastructure/SitemapSourceTests.cs` (FakeCatalog)
- Belgeler: `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.d/T359.md`, `DEFERRED_BACKLOG.d/T359.md` (D-671–675), `DEFERRED_BACKLOG.d/T361.md` (D-498 ✅), `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün bağı, yayındakiler, geri gelir, silinen kalkar, ters bağ/öneri yok | ✓ | Integration `ADraftProductStaysLinked…KK1`, `AnArchivedProduct…`, `ADeletedProductGoesFromTheLink…`, `TheLinkTablesHaveNoVariantColumn…`; Unit `TheCatalogContractHasNoReverseLinkAndNoSuggestionBlock_KK1` |
| 2 | Beş liste elle sıralanır; vitrin/ana sayfa/menü izler; ürün listesi etkilenmez | ✓ | Integration `AMoveUpAndDown…KK2` (5 tip), `TheListPageTheHomePageBlockAndTheMenuFollowTheSameOrder…KK2` |
| 3 | Hiçbir tip zorunlu değil; yayın kapısı tipe göre | ✓ | Integration `ARecordWithAnEmptyRequiredField_CannotBePublished…KK3` (7 tip), `NoTypeIsRequired…KK3`; Unit `ContentGateTests` (T322) |
| 4 | Hakkımızda boş/yayında değil: marka bloğu | ✓ | Integration `AnAboutPage_NotThereDraftOrEmpty…KK4` (3 durum; çizim D-674) |
| 5 | Galeri: ana görsel sıradan bağımsız, alt metin isteğe bağlı, tavanlar parametrede | ✓ | Integration `TheMainImageIsAMarkOfItsOwn…`, `WithoutAnAltText…`, `TheTwentyFirstImageIsRefused…`; Unit `TheCeilingsOfTheContentFieldsAreTheParameters…` |
| 6 | Uzun metin ürünün biçim seti | ✓ | Unit `TheLongTextDropsEmbeddedFrames…`, `TheLongTextUsesTheProductsFormatSet…`; Integration `TheLongTextOfARecordIsKeptInTheFormatSet…KK6` |
| 7 | Gömülü video ve dosya eki yok, video bağlantısı açılır | ✓ | Unit `ThePolicyAllowsNoFrame…KK7`, `TheContentModuleHasNoTypeMemberOrColumn…KK7_KK8`; video bağlantısı (`<a href>`) korunur |
| 8 | Müşteri görüşü tipi, dönen alıntı, blog yok | ✓ | Unit `TheContentModuleHasNoTypeMemberOrColumn…`, `TheClosedAddressListHasNoPrefixForABlog…KK8` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 taslak bağda kalır/görünmez/geri gelir; silinen kalkar; varyant alanı yok; ters bağ ve öneri yok | ✓ | `ContentRelationsTests` (Integration) KK1 testleri; bilgi şeması sütun taraması |
| KK2 sıra liste, ana sayfa, menüde aynı | ✓ | `TheListPageTheHomePageBlockAndTheMenuFollowTheSameOrder…KK2` |
| KK3 eksik alanlı kayıt yayına alınamaz | ✓ | `ARecordWithAnEmptyRequiredField_CannotBePublished…KK3` (yedi tip; boşluk-yalnız ad) |
| KK4 boş Hakkımızda blokta marka | ✓ | `AnAboutPage_NotThereDraftOrEmpty…KK4` |
| KK5 alt metin boşsa ad ve sıra | ✓ | `WithoutAnAltText…`, `TheAboutPageAndTheQuestionNameTheirImages…` |
| KK6 biçim seti dışı etiket temizlenir | ✓ | Unit + Integration KK6 |
| KK7 video oynatıcı ve ek yükleme ucu yok | ✓ | CSP `frame-src 'none'`; tip/üye/EF sütun taraması (`Video|Embed|Attachment|Pdf`) |
| KK8 görüş tipi ve blog uçları yok | ✓ | tip/sütun taraması; kapalı adres listesi taraması |
| `09 §25` tek işlev | ✓ | `GalleryText` ürün ve içerik galerisinin ortak kural evi; `ContentMainImages` ikinci kopyayı kaldırdı |
| `09 §9.1` madde 3 (modül `SaveChanges` çağırmaz) | ✓ | `ContentProductLinks`, `ContentOrdering`, `ContentGallery` çağırmaz; `ExecuteUpdate` işlem içinde zorunlu (istisna atar) |
| Sık FAIL 1 (bekçi/tarama istisnası geniş) | ✓ | taramalar sözcük listesiyle tüm CorporateContent tip/üye/sütun adlarına bakar; istisna yok (Catalog taramasında `ReferencePrice` anlamlı olduğu için `ReferenceWork` sözcüğü kullanıldı) |
| Sık FAIL 2 (tarama çağrı biçimi kaçırır) | ✓ | tipler + üyeler (public/non-public) + EF tablo/sütun adları üç ayrı katmanda taranır |
| Sık FAIL 3 (06 "Varsayılan" / FK dizini) | ✓ | şema değişikliği yok; bağ tabloları T358/T322 göçlerinde |
| Sık FAIL 4 (aynı sayıyı anan öteki yerler) | ✓ | `.claude/CONTEXT.md` klasör haritası güncellendi; ceza sabitleri `ProductConstants`'tan türer, testte elle sayı yok (`ImagesPerContentRecordMax`) |
| Sık FAIL 5 (belge kuralını değiştiren 09 metni karar kaydısız) | ✓ | 09 metni değişmedi; sıra kaydırma yöntemi k-note olarak D-675'te |
| Sık FAIL: kurucuya yeni bağımlılık, elle kuran düzenekler | ✓ | `ContentGallery` kurucusu `CacheInvalidations` aldı; Integration CorporateContent (121) + Catalog (72) koşuldu |
| Önbellek geçersiz kılma tüm yazma yollarında | ✓ | galeri kaydı `HomepageBlocks`'u düşürür (D-498; `AGallerySaveDropsTheHomePageBlocks…`), sıra kaydırma `Menu`+`HomepageBlocks` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` | 5024/5024 (ContentRelationsTests 19) |
| Integration | ✓ | `--filter-namespace "*CorporateContent"` ve `"*.Catalog"` (PostgreSQL 18, kendi konteyner) | 121/121 ve 72/72 (ContentRelationsTests 35) |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <değişen dosyalar>` | temiz |
| Contract | — | yok | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T359-kurumsal-icerik`
- **PR:** açılışta eklenir
- **CI:** — (yardımcı izler)

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Kart/liste görselinde etkin alt metin çizimi | D-671 | sayfa katmanı (T505/T537) |
| 2 | İçerik sayfası okuyucusu ve "İlgili ürünler" çizimi | D-672 | sayfa task'ı |
| 3 | `ContentPublishing.DeleteAsync` kendi `SaveChanges`'ini çağırıyor | D-673 | T322'den; panel silme task'ı T360 |
| 4 | Marka adı/logo bloğu çizimi | D-674 | sayfa katmanı |
| 5 | Sıra kaydırma koşullu `ExecuteUpdate` (kilit yerine) | D-675 | k-note; panel uçları T360/T363 |

## Notlar

- **Kararlar (kolay ⚠, kurala uygun seçim):** sıra kaydırma satır kilidi yerine koşullu `ExecuteUpdate`; Hakkımızda galeri adı firmanın menü adı (`MenuNames.AboutPage`); şube görseli tek sütun (`SetBranchImageAsync` sürümlü), etkin metin "Şube adı — görsel 1 / 1".
- **Doğrulama için:** `Offering`/`PortfolioItem`/`CustomPage` adı veritabanında NOT NULL olduğundan kapı testinde eksik alan boşluk-yalnız metinle üretildi.
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum" parça kuralı gereği değiştirilmedi.
