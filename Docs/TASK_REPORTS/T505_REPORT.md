# T505 — İçerik liste ve içerik sayfaları (E-07, E-08)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-11

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

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` (main birleştirmesinden sonra) | 7032/7032 |
| Integration | ✓ | Parçalı (namespace bayraklarıyla), Postgres 18 `track_commit_timestamp=on`. Birleştirmeden önce TAM: Web 636, CorporateContent+Catalog+Administration 345, Security+SignedEndpoints+Commands+Concurrency+Disposal+Files+FileSecrets 332, Jobs+Logging+Ordering+Outages+Payment+Database 900, Model 1407. Birleştirmeden sonra: `ContentPagesTests` 9/9; `NotFoundPageTests`, `SessionAccessTests`, `SearchEngineDataTests`, `StorefrontFrameTests`, `DomesticProductionLogoTests` 110/110 | Birleştirmeyle gelen T504 değişiklikleri yalnız 404 çizimi ve bu testlerle ilgiliydi |
| Düzeltme 1 (birleşik ağaç) | ✓ | restore, `build -warnaserror` (0 uyarı), `dotnet format --verify-no-changes` ×3; Unit TAM 7131/7131; Integration TAM ad alanı parçalarıyla (`shopfolio-t505-fix1-pg`, `track_commit_timestamp=on`, 127.0.0.1:58905): CorporateContent 194, Web 733, Ordering 637, Model 1407, Pages+Administration+Catalog+Files+Payment+Security+SignedEndpoints+Outages+FileSecrets 434, Database+Jobs+Disposal+Commands+Logging+Concurrency 312 | Model parçası paralel koşuda 3 kırmızı verdi, tek başına yeniden koşuldu 1407/1407 (makinede eşzamanlı koşu) |
| Contract | — | — | Yok |
| Derleme / biçim | ✓ | `dotnet build -c Release` (Unit ve Integration derlemeleri); `dotnet format` üç projede birleştirme öncesi koşuldu (tek değişiklik using sırası, commit'li); birleştirmeden sonra C# kaynağı değişmedi | 0 hata |

## Doğrulama

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
- **PR:** açıldığında dönüşte bildirilir
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
