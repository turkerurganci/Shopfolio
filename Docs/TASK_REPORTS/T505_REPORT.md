# T505 — İçerik liste ve içerik sayfaları (E-07, E-08)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 3 PASS) | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- Liste sayfaları `/hizmetler` ve `/referanslar` (`Pages/Content/ContentListPages.cs`, `_ContentListBody.cshtml`): tipin yayındaki kayıtları firmanın sırasıyla, 24'lük sayfalarla, tip adı menüdeki ad; kayıt yoksa boş hâl satırı.
- Kayıt sayfaları `/hakkimizda`, `/hizmet/{slug}`, `/referans/{slug}`, `/sayfa/{slug}` (`ContentRecordPages.cs`, `_ContentRecordBody.cshtml`): ad, galeri, metin, hizmette "Bize ulaşın", "Paylaş", yayındaki bağlı ürünlerle "İlgili ürünler".
- Taslak: panel oturumuyla "Taslak" bandı ve noindex; ziyaretçiye, olmayan ve başka tipin adresine aynı 404.
- Modül: `StorefrontContentPages`, `ICorporateContentModule.GetContentListAsync/GetContentPageAsync`, `ICatalogModule.ListPublishedProductCardsAsync` (`ProductListCard`), `ContentProductLinks.ListRelatedCardsAsync`; `RecordPages.FixedContent/ContentList`.
- Integration örnek sayfaları gerçek adreslerle çakışmasın diye `/sss/ornek-*` altına taşındı.
- Birleştirme: main (T504, #540) dala birleştirildi, çakışma yok.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Content/*`, `Pages/Shared/_Content{ListBody,RecordBody}.cshtml`, `Modules/CorporateContent/{StorefrontContentPages,CorporateContentModule,ContentProductLinks}.cs` ve `Contracts/*`, `Modules/Catalog/{ProductListReader,CatalogModule}.cs` ve `Contracts/*`, `Infrastructure/SearchEngine/RecordPages.cs`, `wwwroot/css/storefront.css`; testler `tests/Shopfolio.IntegrationTests/CorporateContent/ContentPagesTests.cs`, örnek sayfalar, `tests/Shopfolio.UnitTests/Infrastructure/{RecordPagesTests,SitemapSourceTests}.cs`, `Support/ApplicationEnums.cs`; `.claude/CONTEXT.md` (klasör haritası), kayıt dosyaları.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Liste, elle sıra, 24'lük sayfa, tipin adı firmanın verdiği ad | ✓ | `ContentPagesTests.TheServiceList_IsInTheFirmsOrder_24APage_AndTheNameIsTheFirmsOwn_T505_KK1`, `TheReferenceList_UsesTheReferenceTypeAndItsOwnName_T505_KK1` |
| 2 | Taslak listede yok; yayında kaydı olmayan tip boş hâl, menü öğesi yok | ✓ | `ADraft_IsNotInTheList_AndATypeWithNoPublishedRecord_IsTheEmptyStateAndHasNoMenuItem_T505_KK2` |
| 3 | Hakkımızda sabit adres; hizmet, referans, genel sayfa `{Slug}` ile | ✓ | `TheFourAddresses_OpenTheirRecords_AndHakkimizdaIsAtItsFixedAddress_T505_KK3` |
| 4 | Ad/başlık, görseller, metin, türe özgü bölge | ✓ | `ARecordPage_ShowsTheNameTheGalleryInTheFirmsOrderTheTextAndTheTypesRegion_T505_KK4` |
| 5 | "İlgili ürünler" yalnız yayındakiler; hiçbiri yoksa başlık da yok | ✓ | `LinkedProducts_AreTheLinkedPublishedOnes_AndTheHeadingIsGoneWhenNoneIsPublished_T505_KK5` |
| 6 | "Bize ulaşın" E-10 bağlantısı; video ve "Paylaş" uç doğurmaz | ✓ | `AServicePage_LinksToTheContactPage_AndNoContentPageWritesAnything_T505_KK6` (altı adrese POST ve `<form` yok); çapa D-1209 |
| 7 | Taslak panel oturumuyla bant ve noindex; oturumsuza 404 | ✓ | `ADraftRecord_OpensForTheAdministratorWithTheBandAndNoindex_AndIs404ForAVisitor_T505_KK7` (dört tip) |
| 8 | Kayda ait olmayan Slug 404 | ✓ | `ASlugThatIsNotTheTypesRecord_Is404_AlikeForUnknownAndForeignAndDraft_T505_KK8` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil 8 kabul kriteri ve KK1–KK8 diff'te yeniden denetlendi (kod: `Pages/Content/`, `StorefrontContentPages.cs`; test: yukarıdaki dokuz Integration testi, `RecordPagesTests` T505 testleri). `sik-fail-nedenleri.md` tarandı: (1) bekçi/tarama istisnası — yeni tarama ya da istisna eklenmedi; (2) kaynak taraması — yeni tarama yok; (3) 06 "Varsayılan"/FK dizini — şema değişikliği yok; (4) aynı sayıyı anan öteki yerler — `ApplicationEnums` (iki yeni sözleşme enum'u), `SitemapSourceTests` sahteleri, T504 birleştirmesinden sonra `FrameSourceScanTests` görünüm listesi güncel ve yeşil; (5) belge kuralını değiştiren yeni 09 metni — yok. Modül kurucusuna yeni bağımlılık eklenen yerlerde modülü elle kuran test düzenekleri güncel (Integration TAM koşuldu). Hedefi T505 olan backlog kalemleri (D-315, D-502, D-671, D-672, D-674, D-676, D-677, D-690, D-691, D-692) yerinde kapatıldı ya da ileri atıldı.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–8 | ✓ | yukarıdaki tablo |
| KK1 elle sıra, 24'lük sayfa, ad değişikliği | ✓ | `…_T505_KK1` iki test |
| KK2 taslak listede yok; boş tip | ✓ | `…_T505_KK2` |
| KK3 dört adres, sabit Hakkımızda | ✓ | `…_T505_KK3` |
| KK4 türe özgü bölge | ✓ | `…_T505_KK4` |
| KK5 taslak ürün yok; başlık da yok | ✓ | `…_T505_KK5` |
| KK6 bağlantı var, ek uç yok | ✓ | `…_T505_KK6` |
| KK7 bant, noindex, oturumsuz 404 | ✓ | `…_T505_KK7` |
| KK8 yabancı Slug 404 | ✓ | `…_T505_KK8` |
| `09 §25` ilgili satırlar | ✓ | sayfalar yazmaz (KK6); metin `RichText.Render` ile yeniden süzülür; iç anahtar yalnız panel bağlantısında; sayfa modül klasörüne girmez; her şey `ICorporateContentModule`/`ICatalogModule` arayüzünden |
| Güvenlik taraması | ✓ | ham HTML yalnız süzülmüş metin; POST 200/303 vermez; taslak ziyaretçiye 404 ve noindex; yeni bağımlılık, sır, şema yok |

### Düzeltme 1 (B1) — öz denetim

Tur 1 FAIL'i (B1) kapatıldı: `_ContentRecordBody.cshtml` kısa metin paragrafı, `ContentRecordPageModel.Summary` ve `.content-page__summary` stili kaldırıldı; `page.Summary` yalnız arama motoru açıklamasına (`RecordPages.Content`/`FixedContent`) gider. `git grep` ile aynı alanı anan başka yer kalmadı (`ContentRecordPage.Summary` sözleşmesinin açıklaması "yalnız arama motoru açıklaması" diye güncellendi). Dalın main ile birleştirmesinde `storefront.css` (T502 ve T505 blokları ikisi de kaldı) ve `ProductListReader.cs` (T502 `ProductListItem` ana görseli + T505 `ListCardsAsync`; kart görseli artık `ProductListItem.MainImageId/MainImageAltText`'ten gelir) çakıştı, elle çözüldü. `sik-fail-nedenleri.md` okundu; B1 türü ("sayfa hiyerarşisinde olmayan alan") için sayfa bölge listesi `04` 5.8.3/5.8.4 ile yeniden karşılaştırıldı.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–3, 5–8 | ✓ | değişmedi; ilgili KK testleri yeşil |
| Kriter 4 / KK4 | ✓ | `…_T505_KK4` artık kısa metnin `<main>`'de olmadığını ve `description` meta'sında olduğunu sınar; bölgeler: ad, görseller, metin, türe özgü bölge, "Paylaş" |
| KK1–KK3, KK5–KK8 | ✓ | `ContentPagesTests` yeşil |
| `09 §25` | ✓ | kısa metin sayfada yok; modül sınırı ve süzme değişmedi |

### Düzeltme 2 (B2) — öz denetim

Tur 2 FAIL'i (B2) kapatıldı: "Paylaş" `</article>` ve "İlgili ürünler" bölümünden sonra, ayrı bir `content-page__actions content-page__share` satırında çizilir; "Bize ulaşın" makale içinde kalır. Sıra (`04` 5.8.3, 5.8.4, 5.8.11): ad, görseller, metin, "Bize ulaşın", "İlgili ürünler", "Paylaş". T505 kabul kriterlerinin ve KK1–KK8'in tamamı yeniden denetlendi; `sik-fail-nedenleri.md` tarandı (son satır: sayfa bölge sırası — bu düzeltmenin konusu; test artık sırayı sınar).

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–3, 5, 7, 8 | ✓ | değişmedi; `ContentPagesTests` 195/195 |
| Kriter 4 / KK4 | ✓ | `…_T505_KK4` ×2; yeni test hizmette `h1 → görsel → metin → Bize ulaşın → İlgili ürünler → data-share`, referansta `h1 → İlgili ürünler → data-share` sırasını ve referansta "Bize ulaşın" yokluğunu `<main>` içinde sınar |
| Kriter 5 / KK5 | ✓ | `…_T505_KK5` yeşil; bölge görünürlüğü değişmedi |
| Kriter 6 / KK6 | ✓ | bağlantı hizmette var, `<form` ve POST yok; yeşil |
| KK1–KK3, KK7, KK8 | ✓ | `ContentPagesTests` yeşil |
| `09 §25` | ✓ | yalnız işaretleme sırası; modül sınırı, süzme, stil/betik kuralları değişmedi |
| Kayıt | ✓ | yeni backlog kalemi yok; D-1256, D-1257 dokunulmadı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` (main birleştirmesinden sonra) | 7032/7032 |
| Integration | ✓ | Parçalı (namespace bayraklarıyla), Postgres 18 `track_commit_timestamp=on`. Birleştirmeden önce TAM: Web 636, CorporateContent+Catalog+Administration 345, Security+SignedEndpoints+Commands+Concurrency+Disposal+Files+FileSecrets 332, Jobs+Logging+Ordering+Outages+Payment+Database 900, Model 1407. Birleştirmeden sonra: `ContentPagesTests` 9/9; `NotFoundPageTests`, `SessionAccessTests`, `SearchEngineDataTests`, `StorefrontFrameTests`, `DomesticProductionLogoTests` 110/110 | Birleştirmeyle gelen T504 değişiklikleri yalnız 404 çizimi ve bu testlerle ilgiliydi |
| Düzeltme 1 (birleşik ağaç) | ✓ | restore, `build -warnaserror` (0 uyarı), `dotnet format --verify-no-changes` ×3; Unit TAM 7131/7131; Integration TAM ad alanı parçalarıyla (`shopfolio-t505-fix1-pg`, `track_commit_timestamp=on`, 127.0.0.1:58905): CorporateContent 194, Web 733, Ordering 637, Model 1407, Pages+Administration+Catalog+Files+Payment+Security+SignedEndpoints+Outages+FileSecrets 434, Database+Jobs+Disposal+Commands+Logging+Concurrency 312 | Model parçası paralel koşuda 3 kırmızı verdi, tek başına yeniden koşuldu 1407/1407 (makinede eşzamanlı koşu) |
| Düzeltme 2 | ✓ | restore, `build -warnaserror` (0 uyarı), `dotnet format --verify-no-changes` ×3; Unit TAM 7142/7142; Integration (`shopfolio-t505-fix2-pg`, `track_commit_timestamp=on`, 127.0.0.1:58321): CorporateContent 195/195 (yeni test dahil), Web+Pages+Administration 824/824 | Süre bütçesi doldu: kalan ad alanı parçaları (Catalog, Commands, Concurrency, Ordering, Payment, Database, Disposal, FileSecrets, Files, Model, Jobs, Logging, Outages, Security, SignedEndpoints, kök) bu turda koşulmadı; değişiklik yalnız bir ortak görünüm ve bir test, devir notunda |
| Contract | — | — | Yok |
| Derleme / biçim | ✓ | `dotnet build -c Release` (Unit ve Integration derlemeleri); `dotnet format` üç projede birleştirme öncesi koşuldu (tek değişiklik using sırası, commit'li); birleştirmeden sonra C# kaynağı değişmedi | 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 4) | ✓ PASS — tur 4 (2026-11-12, head `df5a8864`; `origin/main` `9f5b8ac8` (T520 #549) çakışmasız birleştirildi → `687ec955`, doğrulama birleşik ağaçta). Kriter 1–8 ve KK1–KK8 ✓; tur 3'ten sonraki iki kod birleştirmesi (`storefront.css` — main'den satır silinmedi, yalnız T505 stilleri eklendi; `SessionAccessTests.cs` — iki eski `/sayfa/…` örnek adresi `/sss/ornek-serit/…` karşılığıyla değişti) davranışı bozmadı |
| Bulgu sayısı (tur 4) | 0 |
| Düzeltme gerekli mi (tur 4) | Hayır |
| Bloke-etmeyen not (K) sayısı (tur 4) | 0 yeni — D-1209, D-1211, D-1212, D-1213, D-1256, D-1257, D-1273 açık ve hedefli; `09 §24` madde 4 notu (iki "İlgili ürünler" okuyucusu) D-1273'te |
| `12` satırları (tur 4) | `VAL-A115`…`A119` (KP-32) `BEKLEMEDE` kalır (tarayıcı gözlemi); `12_matris_denetle.py --kapsam 1-77` Hata 0 |

**Validator kanıtları (tur 4):** kod okuması (`StorefrontContentPages`, `ContentListPages`, `ContentRecordPages`, `_ContentListBody`, `_ContentRecordBody`, `ProductListReader.ListCardsAsync`, `RecordPages.FixedContent/ContentList`, `StorefrontMenuReader`) `07 §4.7.1`, §4.8.1, `04` 5.7, 5.8, 6.2.7, 6.2.8 ile birebir; menü öğeleri her zaman listede (görünürlük bayrağıyla), `First` başarısız olmaz. Yerel (birleşik ağaç `687ec955`): restore ✓, `build -warnaserror` 0/0, `dotnet format --verify-no-changes` üç proje 0, Unit Consistency 295/295 + kalan 6920/6920, betik testleri 4/4, Integration TAM ad alanı parçalarıyla 3827/3827 (CorporateContent+Pages+Pages.Panel+Administration+Catalog 346, Web 821, Ordering+Payment 704, Model+Database 1446, kök+kalan 510; Postgres 18 `track_commit_timestamp=on`, 127.0.0.1:58705), `restore --locked-mode` 0, `git status` temiz. CI: PR #543 run 43113395347 (head `df5a8864`) success. Ana dal son 3: 43110306088, 43110304288, 43101562500 success. Güvenlik: yeni POST yok, taslak yetkisi sunucuda (`HasAdminSessionAsync`), metin `RichText.Render` ile süzülüyor, iç anahtar yalnız yöneticinin bandında, yeni paket/sır yok.

**Yapım raporu karşılaştırması (tur 4):** öz denetim tabloları var ve kanıtlı; verdict'le uyumlu, yeni uyuşmazlık yok.

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 3) | ✓ PASS — tur 3 (2026-11-11, head `e70eac37`; dal main `25f1f1ee` ile birleşikti, doğrulama sonunda `origin/main` `a9270c02` (T511 #545) birleştirildi: yalnız `DEFERRED_BACKLOG.md`'de kayıt çakışması — satır satır, her satırda değiştiren taraf alındı (D-668, D-693 main; D-671, D-672, D-690 dal); `SessionAccessTests.cs` ve `ApplicationEnums.cs` kendiliğinden birleşti). Kriter 1–8 ve KK1–KK8 ✓; B2 kapandı |
| Bulgu sayısı (tur 3) | 0 |
| Düzeltme gerekli mi (tur 3) | Hayır |
| Bloke-etmeyen not (K) sayısı (tur 3) | 1 — **K1** (forward → D-1273): `ContentProductLinks.ListRelatedAsync` (`ShowcaseProduct`) T505'ten sonra üretim kodunda çağrılmıyor, yalnız `ContentRelationsTests`; sayfa `ListRelatedCardsAsync`'i okur — aynı işin iki okuyucusu (`09 §24` madde 4), D-1257 ile birlikte |
| `12` satırları (tur 3) | `VAL-A115`…`A119` (KP-32) `BEKLEMEDE` kalır: kanıt türleri tarayıcı gözlemi ister (A118, A119 yalnız tarayıcı gözlemi; A115–A117 uç yanıtı + tarayıcı gözlemi); `A117`'nin SSS yarısı T506'nındır. Uç yanıtı yarıları Integration testleriyle uyumlu (KK3, KK5). `12_matris_denetle.py --kapsam 1-77` Hata 0 |

**Validator kanıtları (tur 3):** kriter 1 ✓ (`StorefrontContentPages.ListAsync` → `ContentReaders.ListOfferingsAsync`/`ListPortfolioItemsAsync`: yalnız `Published`, `SortOrder`→`CreatedAt`→`Id`, `PageSizes.ContentList`; başlık menünün adı (`K-851`); `…_KK1` iki test: 26 kayıt elle ters sırada 24 + 2, `page=99` son sayfa, ad değişikliği, `?x=1` noindex) · 2 ✓ (`StorefrontMenuReader` yayında kaydı olmayan tipi gizler; `…_KK2`: taslak yok, boş hâl + `/` dönüşü, menüde `/referanslar` yok; `04` 6.2.7.1 yöneticiye de listede taslak yok — liste okuması kitleden bağımsız) · 3 ✓ (dört `@page`; `/hakkimizda/baska` 404; `07 §4.8.1`) · 4 ✓ (`_ContentRecordBody.cshtml` sırası ad → galeri → metin → "Bize ulaşın" (yalnız hizmet) → "İlgili ürünler" → "Paylaş" = `04` 5.8.3/5.8.4; kısa metin `<main>`'de yok, yalnız `description`; galeri firmanın sırası ve etkin alt metin; `RichText.Render` `<script>`'i süzer; `…_KK4` iki test) · 5 ✓ (`ContentProductLinks.ListRelatedCardsAsync` → `ProductListReader.ListCardsAsync` `Published()`, bağ sırası korunur; `LinkedProducts.Count > 0` başlığı da kapsar; `…_KK5` taslak/arşiv yok, hiçbiri yayında değilken ve bağsızken başlık yok, referans yolu) · 6 ✓ (`ContactPath` yalnız hizmette `/iletisim`, birincil düğme; sayfalarda `OnPost` yok; `…_KK6`: altı adreste `<form` yok, POST 200/303 değil; çapa D-1209) · 7 ✓ (`HasAdminSessionAsync` sunucuda; `RecordPages.Content/FixedContent` `open = !IsDraft`; `SetDraftPreview` `/panel/kurumsal-icerik/{tip}/{iç anahtar}` — `07 §3.1.1` `{tip}` listesiyle birebir; `…_KK7` dört tipte ziyaretçi 404, yönetici 200 + bant + noindex) · 8 ✓ (`IsValidSegment` + tip başına okuma; `…_KK8` bilinmeyen, başka tipin slug'ı, taslak, boşluk, geçersiz bölüm 404; büyük harf 200 — `07 §2.1.7`). `09 §25`: modül sınırı (`Contracts` üzerinden), arama motoru verisi `Infrastructure/SearchEngine`, sayfalı liste `Id` ile biten sıra + `ToPageAsync` + sınır testi, satır içi stil/betik yok, yeni POST yok — evet. Yerel (dal ucu `e70eac37`): restore ✓, `build -warnaserror` 0/0, `dotnet format --verify-no-changes` üç proje 0, Unit 7142/7142, betik testleri 4/4, Integration TAM ad alanı parçalarıyla 3741/3741 (CorporateContent+Pages+Catalog+Administration 346, Web 755, Ordering 638, Model 1407, kök+Commands+Concurrency+Database+Disposal+FileSecrets+Files+Jobs+Logging+Outages+Payment+Security+SignedEndpoints 595; Postgres 18 `track_commit_timestamp=on`, 127.0.0.1:58517), `restore --locked-mode` 0. Birleştirmeden sonra (`f7a356a7`): `build -warnaserror` 0/0, Unit 7153/7153, Integration CorporateContent+Pages+Ordering 840/840, `SessionAccessTests`+`StorefrontFrameTests`+`SearchEngineDataTests` 90/90. CI: PR #543 run 43078205427 (head `e70eac37`) dokuz iş yeşil. Ana dal son 3: 43046780915, 43046780990, 43038133909 success. Güvenlik: yeni POST yok, yetki sunucuda, metin süzülüyor, iç anahtar yalnız yöneticinin bandında, yeni paket/sır yok.

**Yapım raporu karşılaştırması (tur 3):** öz denetim tabloları var ve kanıtlı; verdict'le uyumlu. Uyuşmazlık 2 (bloke etmez): "Düzeltme 2" test satırı Integration'ı kısmi koştuğunu yazar (CorporateContent, Web+Pages+Administration) — doğrulama TAM koştu, 3741/3741; Notlar'daki "panel kayıt adresi tip adları teyitsiz" kararı tur 1'de teyit edilip D-1210 ile kapanmıştı, not eskidir.

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✗ FAIL — tur 2 (2026-11-11, head `e9e7f73f`; `origin/main` `25f1f1ee` (T508 #544) birleştirildi, yalnız `DEFERRED_BACKLOG.md`'de kayıt çakışması — satır satır iki tarafın değişikliği korundu; doğrulama birleşik ağaçta). Kriter 1–3, 5–8 ve KK1–KK3, KK5–KK8 ✓; B1 kapandı (kısa metin `<main>`'de yok); kriter 4 / KK4 ~ (B2) |
| Bulgu sayısı (tur 2) | 1 — **B2** (S1, DÜŞÜK; kriter 4 / KK4; satırsız bulgu): `04` 5.8.3'ün bilgi hiyerarşisi yukarıdan aşağıya (`04` §1 konvansiyonu: "yukarıdan aşağıya numaralı bölgeler; vitrinde dar sınıfın sırasıyla") (1) ad, (2) görseller, (3) metin, (4) türe özgü bölge, (5) "Paylaş"tır; hizmet ve referansta türe özgü bölge "İlgili ürünler"i de kapsar (5.8.4). `_ContentRecordBody.cshtml` "Paylaş"ı (`_ShareButton`) `content-page__actions` içinde makalenin sonunda, "İlgili ürünler" bölümünü ise `</article>`'dan sonra çizer — "Paylaş" (5) türe özgü bölgenin (4) bir parçasının üstünde kalır; tek sütunlu dar sınıfta (5.8.11) da sıra budur. KK4 testi bu sırayı sınamıyor. Düzeltme: "Paylaş" "İlgili ürünler"in altına alınır (sıra: ad, görseller, metin, "Bize ulaşın", "İlgili ürünler", "Paylaş") ve KK4 testi sırayı sınar |
| Düzeltme gerekli mi (tur 2) | Evet — B2. İkinci iterasyonun FAIL'i: `12 §8.5` iterasyon sınırı, eskalasyon (`INSTRUCTIONS.md` §3.3) |
| Bloke-etmeyen not (K) sayısı (tur 2) | 3 — **K1** (forward → D-1256): D-1212, D-502, D-674 main'e girmiş T502'yi hedef gösteriyor. **K2** (forward → D-1257): `ProductListCard` `ProductListItem`'ın ana görsel alanlarını yineler. **K3** (bu PR'da kapatıldı): PR main'le çakıştığı için (`CONFLICTING`) düzeltme commit'leri `2b7a6afa`, `e9e7f73f` CI koşusu almamıştı; doğrulama commit'i main birleştirmesini taşır, CI yeniden tetiklenir |
| `12` satırları (tur 2) | `VAL-A115`…`A119` (KP-32) `BEKLEMEDE` kalır: kanıt türleri tarayıcı gözlemi ister (A118, A119 yalnız tarayıcı gözlemi); `A117`'nin SSS yarısı T506'nındır. Davranışları Integration testleriyle uyumlu (KK3, KK5) |

**Validator kanıtları (tur 2):** kriter 1 ✓ (`ContentReaders.ListOfferingsAsync`/`ListPortfolioItemsAsync`: yalnız `Published`, `SortOrder`→`CreatedAt`→`Id`, `PageSizes.ContentList` = 24; başlık menü adı; `…_KK1` iki test) · 2 ✓ (`…_KK2`) · 3 ✓ (`Hakkimizda.cshtml` `@page "/hakkimizda"`, `/hizmet/{slug}`, `/referans/{slug}`, `/sayfa/{slug}`; `…_KK3`) · 4 ~ (B2) · 5 ✓ (`ContentProductLinks.ListRelatedCardsAsync` → `ProductListReader.ListCardsAsync` `Published()`; `LinkedProducts.Count > 0` koşulu başlığı da kapsar; `…_KK5`) · 6 ✓ (`ContactPath` yalnız hizmette `/iletisim`, D-1209; sayfalarda `OnPost` yok; `…_KK6`) · 7 ✓ (`HasAdminSessionAsync` sunucu tarafı; `SetDraftPreview` + `!page.IsDraft` → noindex; `…_KK7` dört tip) · 8 ✓ (`AddressRules.IsValidSegment`; tipe özgü sorgu; `…_KK8`). Testler: build `-warnaserror` 0 uyarı; format ×3 temiz; Unit 7142/7142; betik testleri 4/4; Integration TAM ad alanı parçalarıyla 3740 test geçti (Disposal/Database/Model/Files parçasında makine yükünden zaman aşımı alan testler tek başına yeniden koşuldu, hepsi geçti); `restore --locked-mode` 0.

**Yapım raporu karşılaştırması (tur 2):** öz denetim tablosu var ve kanıtlı; uyuşmazlık 1 — "Düzeltme 1" öz denetimi kriter 4'te bölgeleri "ad, görseller, metin, türe özgü bölge, Paylaş" diye sayar, çizim sırası ise "Paylaş"ı "İlgili ürünler"in üstüne koyar (B2).

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-11, head `9321aac8`; `origin/main` `157e7ec3` (T543 #539) birleştirildi, çakışma yok, doğrulama birleşik ağaçta). Kriter 1–3, 5–8 ve KK1–KK3, KK5–KK8 ✓; kriter 4 / KK4 ✗ (B1) |
| Bulgu sayısı (tur 1) | 1 — **B1** (S1, ORTA; kriter 4 / KK4; satırsız bulgu — KP-32 satırları `VAL-A115`…`A119` tarayıcı gözlemi bekler): içerik sayfası kaydın kısa metnini (`ContentPageView.Summary` → `ContentRecordPageModel.Summary`) `_ContentRecordBody.cshtml`'de başlığın altında `<p class="content-page__summary">` olarak çiziyor. `04` 5.8.3'ün bilgi hiyerarşisi beş öğedir — ad ya da başlık, görseller, metin, türe özgü bölge, "Paylaş" — kısa metin yoktur; `04` 5.8.4 Hakkımızda için "uzun metin ve görseller; kısa tanıtım ana sayfanın bloğundadır" der; `06 §4.47`/§4.48 `ShortDescription`'ın yerini "listedeki kartta görünür" (ve arama motoru açıklaması) diye yazar. Gerçekleşen: `/hakkimizda` kısa tanıtımı, `/hizmet/{slug}` ve `/referans/{slug}` kısa açıklamayı sayfada gösteriyor; `ContentPagesTests.ARecordPage_…_T505_KK4` bunu `Assert.Contains("Kısa tanıtım.")` ile kilitliyor. Düzeltme: sayfa gövdesinden kısa metin paragrafı (ve kullanılmıyorsa `Summary` özelliği ile `.content-page__summary` stili) kalkar, arama motoru açıklaması (`RecordPages.Content`/`FixedContent`'e giden `page.Summary`) kalır; KK4 testi kısa metnin gövdede olmadığını ve açıklama meta'sında olduğunu sınar |
| Düzeltme gerekli mi | Evet — B1 aynı PR'da (tur 2) |
| Bloke-etmeyen not (K) sayısı (tur 1) | 2 — **K1** (bu PR'da kapatıldı): D-1210 "tip adları `07`'den teyit edilmedi" diyordu; `07 §3.1.1` `{tip}` kapalı listesi `hakkimizda`, `hizmet`, `referans`, `sayfa`'yı birebir tanımlar (K-1175), `PanelType` değerleri doğru — kayıt ✅ kapatıldı. **K2** (bu PR'da kapatıldı): raporun Notlar bölümü "`12`'de T505'e eşlenen VAL satırı yok" diyordu; KP-32 grep'i `VAL-A115`…`VAL-A119`'u getirir (T505 KP-32'nin son sahibi; `11 §7.1`) — not düzeltildi; satırlar tarayıcı gözlemi beklediği ve tur FAIL olduğu için `BEKLEMEDE` kaldı |

**Validator kanıtları (tur 1):** kriter 1 ✓ (`StorefrontContentPages.ListAsync` → `ContentReaders.ListOfferingsAsync`/`ListPortfolioItemsAsync`: yalnız `Published`, `SortOrder`→`CreatedAt`→`Id`, `PageSizes.ContentList`; tip adı menünün adı — gizli öğe de `Items`'ta, `First` boş tipte düşmez; `…_T505_KK1` iki test: 26 kayıt elle ters sırada, 24 + 2, `page=99` son sayfa, ad değişikliği, `?x=1` noindex) · 2 ✓ (`…_KK2`: taslak yok, boş tip boş hâl + ana sayfa yolu, menüde `/referanslar` yok) · 3 ✓ (dört `@page`, `/hakkimizda/baska` 404; `07 §4.8.1`) · 4 ✗ (B1; galeri sırası, etkin alt metin, `RichText.Render` ile `<script>` süzülmesi, "Paylaş" ✓) · 5 ✓ (`ContentProductLinks.ListRelatedCardsAsync` → `ProductListReader.ListCardsAsync` `Listed()` ile; bağ sırası korunur; `…_KK5` taslak/arşiv yok, hiçbiri yayında değilken ve bağsızken başlık yok, referans yolu) · 6 ✓ (hizmette `href="/iletisim"` "Bize ulaşın", referans/genel sayfada yok; altı adreste `<form` yok, POST 200/303 değil) · 7 ✓ (`HasAdminSessionAsync` sunucuda; dört tipte ziyaretçi 404, yönetici 200 + `draft-band` + `noindex` + panel bağlantısı `/panel/kurumsal-icerik/hizmet/{id}`; yayındaki kayıt noindex'siz) · 8 ✓ (bilinmeyen, başka tipin slug'ı, taslak, boşluk, geçersiz bölüm 404; büyük harf 200 — `07 §2.1.7`). Yerel (birleşik ağaç): restore ✓, `build -warnaserror` 0/0, `dotnet format --verify-no-changes` üç proje 0, Unit 7114/7114, betik testleri 4/4, Integration TAM ad alanı parçalarıyla 3704/3704 (CorporateContent 194, Web 720, Pages+Pages.Panel+Administration+Catalog+Files+Payment+Security+SignedEndpoints+Outages+FileSecrets 434, Ordering 637, Model 1407, Database+Jobs+Disposal+Commands+Logging+Concurrency+kök 312; Postgres 18 `track_commit_timestamp=on`). CI: PR #543 run 43022135662 (head `9321aac8`) dokuz iş yeşil. Ana dal son 3: 43011159643, 43011159636, 42994459437 success. `12_matris_denetle.py --kapsam 1-77` Hata 0; `Durum` hücresi değişmedi. Güvenlik: yeni POST yok, yetki sunucuda, metin süzülüyor, `Html.Raw`/satır içi stil/yeni paket/sır yok.

**Yapım raporu karşılaştırması (tur 1):** öz denetim tablosu var ve kanıtlı; uyuşmazlık 2 — kriter 4'ü ✓ saymış (B1: "ad, özet, galeri, metin" bilinçli çizilmiş, `04` 5.8.3/5.8.4 ile çelişkisi kayda geçmemiş); `12` notu satırları yok saymış (K2).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T505-icerik-sayfalari`
- **Commit:** PR'ın head'i (yardımcı dönüşte bildirir) — T505: İçerik liste ve içerik sayfaları (E-07, E-08)
- **PR:** #543
- **CI:** yapım işçisi izlemez; yardımcı yürütür
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (plan tier, kota, API, paket sürümü, bölge kısıtı geçmiyor; yalnız sayfa ve okuma yüzü) | Kabul kriterleri tarandı | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Bize ulaşın" düz `/iletisim`; sayfa içi çapa iletişim formunun kimliğine bağlı | D-1209 | Form T506'da yazılır |
| 2 | Panel kayıt adresi tip adları — doğrulama tur 1'de `07 §3.1.1` ile teyit edildi, kayıt kapandı | D-1210 ✅ | — |
| 3 | Video ayrı alan değil, metnin bağlantısı (`04` 5.8.6) | D-1211 | Karar kaydı; yeni istek gelirse |
| 4 | `ListPublishedProductCardsAsync` yalnız "İlgili ürünler" için; ana sayfa vitrini `ShowcaseProduct` | D-1212 | T502 aynı yöntemi kullanır |
| 5 | Örnek sayfalar `/sss/ornek-*`; T506 `/sss/{…}` kalıbı eklerse çakışır | D-1213 | T506 |
| 6 | Hakkımızda ana sayfa bloğunda marka adı/logo; ana sayfa ucu; kategori ve ürün sayfası paylaşım ve görsel yolu; panel işleyicileri; SSS/iletişim taslakları | D-315, D-502, D-674, D-676, D-677, D-691, D-692 | Hedefleri T502, T503, T506, T535 ve panel form task'larına güncellendi |

Kapanan kalemler: D-671 (kart alt metni boşken kayıt adı; galeri etkin metni), D-672 ("İlgili ürünler"), D-690 (içerik sayfası okuyucusu ve 404).

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** son 3 run success: 42994459437, 42994459582, 42991019324.
- **Repo memory (bitiş kapısı 9):** `MEMORY.md`'ye dokunulmadı.
- **⚠ Kararlar:** "Bize ulaşın" çapasız `/iletisim`; video bağlantısı metnin bağlantısı; katalog arayüzüne `ListPublishedProductCardsAsync` eklendi; panel kayıt adresi tip adları teyitsiz; menü öğesi gizleme çerçeve (T537) tarafından, testte doğrulandı; örnek sayfalar `/sss/ornek-*`.
- **`12`:** T505'in KP-32 satırları `VAL-A115`…`VAL-A119` (doğrulama tur 1 K2; KP-32 → T358, T359, T505); kanıt türleri tarayıcı gözlemi içerir, `BEKLEMEDE`. `VAL-A117`'nin SSS payı T506'dadır. E-07/E-08'in KP-69 satırları VAL-B062, VAL-B065, VAL-B066 tarayıcı gözlemi, BEKLEMEDE.
