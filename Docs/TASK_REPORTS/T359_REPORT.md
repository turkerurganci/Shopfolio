# T359 — Kurumsal içerik: ürün bağı, elle sıralama, zorunlu alanlar, alan tavanları, kapsam dışı yetenekler

**Faz:** F3 | **Durum:** ✓ Doğrulama tur 3 PASS | **Tarih:** 2026-11-08

> Taslağı yapım işçisi doldurur; doğrulama finalize eder.

---

## Yapılan İşler

- **Galeri ortak kuralları** `Infrastructure/GalleryText` (ana işaret, boş alt metin yerine "Ad — görsel p / n", firma metni kırpılır): Catalog `GalleryRules`/`ImageAltText` ona devredildi (tek işlev, `09 §25`).
- **Ürün bağı** `ContentProductLinks` (hizmet ve referans iş): ürün düzleminde, varyantsız; sıra seçim sırası; tekrar eden ürün bütün listeyi reddeder, var olmayan (silinmiş) ürün atlanır ve kalan bağlar seçim sırasıyla yazılır (düzeltme 2); taslak ürün bağda kalır. `ICatalogModule` iki okuma üyesi aldı (`ListPublishedProductsAsync`: yalnız yayındaki kartlar, `FindProductIdsAsync`); ters bağ ve öneri üyesi yok.
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
| 1 | Ürün bağı, yayındakiler, geri gelir, silinen kalkar, ters bağ/öneri yok | ✓ | Integration `ADraftProductStaysLinked…KK1`, `AnArchivedProduct…`, `ADeletedProductGoesFromTheLink…`, `ADeletedProductInTheListIsSkipped…KK1` (iki sahip tipi; sıra korunur, hata değil), `ARefusedListWritesNothing…` (yinelenen, bilinmeyen kayıt), `TheLinkTablesHaveNoVariantColumn…`; Unit `TheCatalogContractHasNoReverseLinkAndNoSuggestionBlock_KK1` |
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
| `09 §10.5` m.6, `09 §25`, Sık FAIL (yeni sayfalı listeye sınır testi) | ✓ | `TheOfferingsPage_WithEqualOrderAndCreatedAtAcrossThePageBoundary…` ve `ThePortfolioPage_…` (Integration `ContentRelationsTests`): aynı `SortOrder` ve `CreatedAt`'li 34 yayında kayıt + 5 taslak; iki sayfa yürünür, kayıt yinelenmez/kaybolmaz, sıra `Id` artan, taslak sayılmaz, aşan `page` son sayfa, `0` ilk sayfa |
| Sık FAIL (format test dosyasında koşuldu mu) | ✓ | `dotnet format` üç projede `--verify-no-changes` temiz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` | 5140/5140 (düzeltme 2 sonrası, main birleşik) |
| Integration | ✓ | `--filter-namespace "*CorporateContent*"` ve `"*Catalog*"` (PostgreSQL 17, kendi konteyner) | 150/150 ve 72/72 (düzeltme 2 sonrası; ContentRelationsTests +2) |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <değişen dosyalar>` | temiz |
| Contract | — | yok | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-09, head `5cdc79c4`) |
| Bulgu sayısı | 1 (B1 DÜŞÜK S1) |
| Düzeltme gerekli mi | Evet — B1 bu dalda düzeltilir, yeni doğrulama turu |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-687) |

**Validator kabul kriterleri tablosu:** K1 ✓ · K2 ✓ · K3 ✓ · K4 ✓ (servis düzeyi; çizim D-674) · K5 ✓ · K6 ✓ · K7 ✓ · K8 ✓; kontrol listesi `09 §25` maddesi ✗ (B1). `12` satırları (`VAL-A118`, `A119`, `A148`, `A150`, `C033`, `C034`) `BEKLEMEDE` kaldı: kanıt ortamı uç yanıtı / tarayıcı gözlemi, sayfa ve uç sahibi T505/T537/T360/T363 (T358 emsali).

- **B1 (DÜŞÜK, S1) — iki yeni sayfalı listenin sayfa sınırı testi yok.** `ContentReaders.ListOfferingsAsync` ve `ListPortfolioItemsAsync` (`ToPageAsync`, `PageSizes.ContentList` = 24) bu task'ta eklendi; `09 §10.5` madde 6 "eşit anahtarlı kayıtlar sayfa sınırında ne yinelenir ne kaybolur (… elle sıralı liste için ayrı ayrı); son sayfayı aşan `page` son sayfayı verir … Her sayfalı liste için sınır testi zorunludur: yeni liste, testi olmadan eklenmez (§25)" der. `ListOfferingsAsync` yalnız ilk sayfası 3 kayıtla (KK2 testi), `ListPortfolioItemsAsync` hiç sınanmıyor (`grep -rn ListPortfolioItemsAsync tests` boş). Düzeltme: iki liste için Integration sınır testi — aynı `SortOrder`'lı (ve aynı `CreatedAt`'li) 25+ yayında kayıt sayfa sınırına düşer, iki sayfa yürününce kayıt ne yinelenir ne kaybolur; taslak sayılmaz; son sayfayı aşan `page` son sayfayı verir (emsal `CategoryTreeAndListsTests.ACategoryPage_WithEqualCreatedAtAcrossThePageBoundary_…`, `AuditTrailTests.ThePages_AreTwentyFiveRows_…`).
- **K1 → D-687:** D-671 "D-500'ün karar kısmı bu şekilde kapanır" der ama `DEFERRED_BACKLOG.d/T361.md` D-500 açık kaldı; iki kalem tek konu.
- **Yapım raporu karşılaştırması:** öz denetim tablosunda `09 §10.5` sınır testi maddesi yok (B1 kaçmış); öteki maddeler validator kanıtıyla uyumlu.

### Düzeltme 1 (2026-11-09)

B1 düzeltildi: `ContentRelationsTests`'e iki Integration sınır testi eklendi (hizmet listesi ve referans listesi; ayrıntı yukarıdaki öz denetim satırında). Üretim kodu değişmedi. Öz denetimde kabul kriterleri 1-8 ve kontrol listesi yeniden denetlendi, hepsi ✓. Backlog kalemi eklenmedi.

### Doğrulama tur 2 (2026-11-09) — ✗ FAIL

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, head `f77197f5`; `origin/main` birleştirildi, yalnız STATUS tablo satırı çakıştı, iki taraf kaldı) |
| Bulgu sayısı | 1 (B2 DÜŞÜK S1) |
| Düzeltme gerekli mi | Evet — iki iterasyon doldu (`12 §8.5`): eskalasyon (`INSTRUCTIONS.md` §3.3) |
| Bloke-etmeyen not (K) sayısı | 1 (K2 → D-712) |

**Validator kabul kriterleri tablosu (tur 2):** K1 ~ (B2) · K2 ✓ · K3 ✓ · K4 ✓ (servis düzeyi; çizim D-674) · K5 ✓ · K6 ✓ · K7 ✓ · K8 ✓; B1 kapandı (iki sınır testi yeşil). `12` satırları `BEKLEMEDE` kaldı (tur 1 gerekçesi); `12_matris_denetle.py --kapsam 1-77` Hata 0. Yerel (birleşik ağaç): build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 5132/5132, betik testleri 4/4, Integration `CorporateContent` 148/148, `Catalog` 72/72. Dal CI 42527884835 yeşil.

- **B2 (DÜŞÜK, S1) — ürün bağı kaydında olmayan ürün bütün listeyi reddediyor; belge "atlanır" diyor.** `ContentProductLinks.SaveAsync` listede var olmayan bir ürün kimliği görünce `ProductNotFound` döner ve hiçbir bağ yazmaz (`ContentProductLinks.cs` satır 91–94; test `ARefusedList_WritesNothing_…_KK1` bunu kilitler). `06 §4.53` "eski formun kaydı silinmiş ürünün bağını yazmaz — bağ kümesi kaydedilirken var olmayan ürün atlanır (`02 §3.27.10`)" ve `07 §9.13.2` "ilgili ürünlerden biri o arada silinmişse bağı atlanır, hata değildir (`06 §4.53`)" der. Etki: form açıkken bağlı bir ürün silinirse içerik kaydının kaydı reddedilir; kural modülde olmadığından panel (T534) yeniden yazmak zorunda kalır (`09 §25` "kod hiçbir doküman cümlesiyle çelişmiyor mu" → hayır). Düzeltme: var olmayan kimlikler atlanır (yalnız var olanlar seçim sırasıyla bağlanır), test atlamayı sınar; tekrar eden ürün reddi kalabilir.
- **K2 → D-712:** D-673'ün hedefi T360 main'e girdi (#481), `ContentPublishing.DeleteAsync` hâlâ `SaveChangesAsync` çağırıyor; kalem yeni hedefe bağlanmalı.
- **Yapım raporu karşılaştırması:** rapor "olmayan ürün bütün listeyi reddeder" kararını yazıyor ama `06 §4.53` / `07 §9.13.2` ile karşılaştırmıyor (B2); öteki maddeler validator kanıtıyla uyumlu. Öz denetim tablosu var, maddeler kanıtlı.

### Düzeltme 2 (2026-11-09)

B2 düzeltildi: `ContentProductLinks.SaveAsync` var olmayan (silinmiş) ürünü atlar, kalan bağları seçim sırasıyla yazar, hata değildir (`06 §4.53`, `07 §9.13.2`); `ProductNotFound` sonucu kaldırıldı, kural modülde. Yinelenen ürün (`DuplicateProduct`) ve bilinmeyen kayıt (`OwnerNotFound`) reddi aynen korundu (dokümanlar bunlara dokunmuyor; grep ile doğrulandı). Testler: reddi kilitleyen test yinelenen + bilinmeyen kayıtla sınırlandı; yeni `ADeletedProductInTheListIsSkipped_TheRestAreLinkedInTheOrderOfTheChoice_NotAnError_KK1` (hizmet ve referans iş; silinmiş + hiç olmayan kimlik atlanır, sıra korunur, ilgili ürünler aynı sırada). Öz denetimde kabul kriterleri 1-8 ve kontrol listesi KK1-KK8 yeniden denetlendi, hepsi ✓ (KK1 kanıtı güncellendi, yukarıdaki tablo). Yerel: build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 5140/5140, Integration `CorporateContent` 150/150, `Catalog` 72/72. Backlog kalemi eklenmedi (K2/D-712 bu düzeltmenin dışında).

### Doğrulama tur 3 (2026-11-09) — ✓ PASS

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 3, head `29e187eb`; `origin/main` `8772edbc` çakışmasız birleştirildi, birleşik ağaç build + Unit yeşil) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K3 → D-720) |

**Validator kabul kriterleri tablosu (tur 3):** K1 ✓ (B2 kapandı: `ADeletedProductInTheList_IsSkipped_…_KK1` iki sahip tipiyle; silinmiş ve hiç olmayan kimlik atlanır, sıra korunur; yinelenen ürün ve bilinmeyen kayıt reddi korunur) · K2 ✓ · K3 ✓ · K4 ✓ (servis düzeyi; çizim D-674) · K5 ✓ · K6 ✓ · K7 ✓ · K8 ✓; kontrol listesi KK1–KK8 ve `09 §25` ✓. `12` satırları (`VAL-A118`, `A119`, `A148`, `A150`, `C033`, `C034`) `BEKLEMEDE` kaldı (tur 1 gerekçesi; sayfa ve uç sahibi sonraki task'lar). Dal CI 42535094509 yeşil. Yerel: build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 5140/5140 (birleşik ağaçta 5170/5170), betik testleri 4/4, Integration `CorporateContent` 150/150, `Catalog` 72/72 (PostgreSQL 18).

- **K3 → D-720:** `ContentProductLinks.SaveAsync` ürün varlığını okuduktan sonra aynı ürün silinirse ekleme yabancı anahtar hatasına düşer; dar yarış, panel kayıt ucunda (T534) kapatılır.
- **Yapım raporu karşılaştırması:** uyumlu. Küçük farklar: Known Limitations madde 3'ün hedefi (T360) geçti (D-712 kayıtlı); test sonuçlarında PostgreSQL 17 yazıyor, CI 18 kullanıyor.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T359-kurumsal-icerik`
- **PR:** #479
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
