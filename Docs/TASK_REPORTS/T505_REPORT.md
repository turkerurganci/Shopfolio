# T505 — İçerik liste ve içerik sayfaları (E-07, E-08)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- Liste sayfaları `/hizmetler` ve `/referanslar` (`Pages/Content/ContentListPages.cs`, `_ContentListBody.cshtml`): tipin yayındaki kayıtları firmanın sırasıyla, 24'lük sayfalarla, tip adı menüdeki ad; kayıt yoksa boş hâl satırı.
- Kayıt sayfaları `/hakkimizda`, `/hizmet/{slug}`, `/referans/{slug}`, `/sayfa/{slug}` (`ContentRecordPages.cs`, `_ContentRecordBody.cshtml`): ad, özet, galeri, metin, hizmette "Bize ulaşın", "Paylaş", yayındaki bağlı ürünlerle "İlgili ürünler".
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

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` (main birleştirmesinden sonra) | 7032/7032 |
| Integration | ✓ | Parçalı (namespace bayraklarıyla), Postgres 18 `track_commit_timestamp=on`. Birleştirmeden önce TAM: Web 636, CorporateContent+Catalog+Administration 345, Security+SignedEndpoints+Commands+Concurrency+Disposal+Files+FileSecrets 332, Jobs+Logging+Ordering+Outages+Payment+Database 900, Model 1407. Birleştirmeden sonra: `ContentPagesTests` 9/9; `NotFoundPageTests`, `SessionAccessTests`, `SearchEngineDataTests`, `StorefrontFrameTests`, `DomesticProductionLogoTests` 110/110 | Birleştirmeyle gelen T504 değişiklikleri yalnız 404 çizimi ve bu testlerle ilgiliydi |
| Contract | — | — | Yok |
| Derleme / biçim | ✓ | `dotnet build -c Release` (Unit ve Integration derlemeleri); `dotnet format` üç projede birleştirme öncesi koşuldu (tek değişiklik using sırası, commit'li); birleştirmeden sonra C# kaynağı değişmedi | 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

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
| 2 | Panel kayıt adresi tip adları `07`'den teyit edilmedi | D-1210 | Panel içerik formu task'ı doğrular |
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
- **`12`:** `12`'de T505'e eşlenen VAL satırı yok (E-07/E-08 için VAL-B062, VAL-B065, VAL-B066 tarayıcı gözlemi, BEKLEMEDE; `Durum` hücresine dokunulmadı — doğrulayıcının işi).
