# T504 — Hata sayfaları: bulunamayan adres ve beklenmeyen hata (E-06, 500)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-11

---

## Yapılan İşler

- Gövdesiz 404 yanıtı E-06 sayfasına çevrilir: `NotFoundAnswer.WriteIfNotFoundAsync` (`Infrastructure/Errors/UnexpectedErrorLayer.cs`), `UnexpectedErrorMiddleware` hata yakalamazsa bu yolu çağırır. Çizici arayüzü `INotFoundPageRenderer`, uygulaması `Pages/Frame/FramedNotFoundPageRenderer` (vitrinin tam çerçevesi; yöneticinin tarayıcısında şerit), görünüm `Pages/Shared/_NotFound.cshtml`, stil `.not-found` (`components.css`).
- Sayfa noindex (`SearchEngineClosure.MarkClosed`), no-store, çerezsizdir; 404 durum ucunun sayacına ve hata satırına girmez. Makine uçları, `/statik` altındaki eksik dosya ve gövdeli 404 gövdesiz/olduğu gibi kalır. Çizim düşerse boş 404 + no-store.
- İndirme ucu tarayıcıda sayfa açtığından `NotFoundPageMetadata` ile işaretlendi (D-217 kapandı); 500 gövdesiz kalır.
- 500 çerçeveleri (vitrin GET, panel GET, çerçevesiz panel girişi), POST 500 ve 303 hedefi T212 mekanizmasıyla; bu task'ın testleri çerçeve ve içeriği kilitler.
- Devam turu: `DownloadEndpointTests` 500 testi gövdesiz beklentisine geri alındı; `DigitalDeliveryTests`, `SessionAccessTests` ("aynı 404" karşılaştırması artık içerik bölgesini, `<main>`, karşılaştırır — çerçeve izleyiciye göre değişir), `SearchEngineDataTests` (panel 404 HTML + meta noindex) güncellendi; çerçevesiz hata örneği `/panel/davet/{key:regex(^hata-ornegi$)}` oldu (`PanelBareSample`'ın açık şablonuyla çakışmayacak, açık şablon eşlemesini koruyacak biçimde); main (T545) birleştirildi (`FrameSourceScanTests.FrameViews` iki tarafın görünümlerini taşır).
- Kayıt: önceki beş task'ın (T501, T540, T541, T542, T545) "Post-merge run'lar" satırı ve status "Commit" hücreleri squash hash'iyle dolduruldu.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Infrastructure/Errors/{ErrorMarks,INotFoundPageRenderer,UnexpectedErrorLayer,UnexpectedErrorSetup}.cs`, `Endpoints/Files/DownloadEndpoint.cs`, `Pages/Frame/{FramedNotFoundPageRenderer,FramePaths,StorefrontFrameFilter}.cs`, `Pages/Components/NotFoundModels.cs`, `Pages/Shared/_NotFound.cshtml`, `wwwroot/css/components.css`, ilgili testler, `.claude/CONTEXT.md` (Errors klasör satırı), kayıt dosyaları.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 404 vitrinin tam çerçevesinde E-06; başlık, ana sayfaya ve ürünlere dönüş | ✓ | `NotFoundPageTests.AnUnknownAddress_IsE06_InTheFullStorefrontFrame_…_KK1`, `WhileNoProductIsPublished_TheWayToTheProductsIsNotShown_…_KK1`, `ThePageIsE06_NotTheUnexpectedErrorPage_…_KK1` |
| 2 | 404 hâlleri: bilinmeyen ilk bölüm, sahiplik reddi, erişim anahtarı, ömrü dolmuş indirme değeri (öteki hâller kendi task'ında) | ✓ | `…TheUnknownAddress_TheDeniedOrderPages_AndTheInvalidDownloadValue_AreThePageWithTheSameMarkup_…_KK2_KK3`, `APostThatFailsTheOwnershipCheck_…_KK2`, `ARecordOfThePanelThatIsNotThere_…_KK2`; `DownloadEndpointTests.*Is404WithThePageE06*`; `SessionAccessTests.*GetTheSame404*`; `AddressRoutingTests` |
| 3 | Yanıt sebebi ayırt etmez; sayfa noindex; sayaca girmez; boş kategori bu yanıt değil | ✓ (boş kategori ⚠) | `SessionAccessTests.AnotherCustomersOrderAndAnOrderThatDoesNotExist_GetTheSame404_…`, `AWrongOrRenewedAccessKeyAndAMissingPanelRecord_GetTheSame404_…`; `NotFoundPageTests.TheNotFoundPage_IsNoindex_NoStore_WritesNoCookie_AndNeitherCountsNorLogsAnError_…_KK3`, `APageThatIsFound_EvenWithNothingToShow_StaysA200_…_KK3`; `SearchEngineDataTests.ClosedAnswersThatAreNotHtml_…` |
| 4 | 500 vitrinde vitrin, panelde panel çerçevesi; çerçevesiz panel ekranında çerçevesiz | ✓ | `TheStorefront500_IsInTheStorefrontFrame_…_KK4_KK7`, `ThePanel500_IsInThePanelFrame_…_KK4`, `TheSignInScreensOfThePanel_HaveNoFrame_AndTheir500_IsBare_…_KK4` |
| 5 | POST 500 değerlerle ve kalıcı mesajla yeniden gösterir | ✓ | `APost500_ShowsTheFormAgainWithTheSentValue_AndALastingMessage_…_KK5`; `UnexpectedErrorLayerTests` |
| 6 | Sonucu belirsiz işlemde 303 sonucu gösteren ekrana | ✓ | `AnUncertainOperationsError_IsA303ToTheScreenThatShowsTheResult_…_KK6`; `UnexpectedErrorLayerTests` (T212) |
| 7 | Yanıt ayrıntı ve kod taşımaz; hata kayda yazılır, sayaca girer | ✓ | `TheStorefront500_…_KK4_KK7` (gövdede istisna metni yok, `UnexpectedPageError` kayıt satırı, sayaç) |
| 8 | Makine ucunda 500 gövdesiz | ✓ | `DownloadEndpointTests.Download_OfACompletedRowWhoseFileIsMissingFromTheStore_Is500WithNoBody`; `NotFoundPageTests.AMachineEndpointsAndAStaticFilesNotFound_StayEmpty_…`; `NotFoundAnswerTests.AMachineEndpointsEmpty404_StaysEmpty`; `StatusEndpointTests` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–8 ve KK1–KK8 | ✓ | yukarıdaki tablo; önceki işçinin işi dahil dalın bütün diff'inde denetlendi |
| KK1 yayında ürün varsa ürünlere dönüş | ✓ | `AnUnknownAddress_IsE06_…_KK1` (ürünler bağlantısı) ve `WhileNoProductIsPublished_…` (yokken gösterilmez) |
| KK2 T206 ilk bölüm, T207 sahiplik, T312 indirme değeri ayrı testte | ✓ | `AnUnknownAddress…`, `SessionAccessTests.*GetTheSame404*`, `DownloadEndpointTests.Download_WithABadSignature_Is404WithThePageE06`, `…WithASpentLife_…` |
| KK3 boş kategori 200 | ⚠ | kategori sayfası (T502) bu task'ta yok; katmanın 200'e dokunmadığı `APageThatIsFound_EvenWithNothingToShow_StaysA200_…_KK3` ile sınandı; kategori sayfasının kendi testi T502'de |
| KK4 panel girişinde 500 çerçevesiz | ✓ | `TheSignInScreensOfThePanel_HaveNoFrame_…_KK4` (`panel-bare__title` var, `panel-top`, `site-header`, `admin-strip` yok) |
| 09 §25 ilgili satırlar: geniş `catch` yalnız izinli dosyada | ✓ | `NotFoundAnswer` `UnexpectedErrorLayer.cs` içinde (izinli); `JobSourceScan`/geniş-catch taramaları Unit'te yeşil |
| 09 §25 görünüm kuralları (satır içi stil, olay özniteliği yok, BEM) | ✓ | `FrameSourceScanTests.FrameViews` `_NotFound`'u kapsar |
| Güvenlik taraması | ✓ | gövdede istisna/kod yok (KK7); çerezsiz, no-store; açık yönlendirme yok (dönüş yolları sabit adres) |
| Sık FAIL 1 — bekçi/tarama istisnası geniş | ✓ | istisnalar üçü: makine ucu işareti, `/statik` öneki, gövdeli 404; her biri ayrı testte (`NotFoundAnswerTests.*`, `AMachineEndpointsAndAStaticFilesNotFound_…`) |
| Sık FAIL 2 — kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | yeni tarama kuralı eklenmedi; `_NotFound` mevcut görünüm taramasına girdi |
| Sık FAIL 3 — 06 Varsayılan / FK dizini | ✓ (yok) | şema değişikliği yok |
| Sık FAIL 4 — kural değişince öteki yerler | ✓ | gövdesiz 404 varsayan öteki testler tarandı: `HttpStatusCode.NotFound` geçen bütün test dosyaları (Files, Jobs, Web) koşuldu; `.claude/CONTEXT.md` Errors satırı güncellendi |
| Sık FAIL 5 — belge kuralını değiştiren 09 metni karar kaydısız | ✓ (yok) | belge değişmedi; makine ucu 404'ünün gövdesizliği `07 §2.7.2` ile uyumlu |
| Backlog: bu task'a devredilen D-217, D-690 | ✓ | D-217 kapatıldı; D-690 hedefi T505'e indi, 404 sayfasının yazıldığı notlandı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-restore` (main birleştirmesi sonrası) | 7029/7029 (Consistency dahil) |
| Integration | ✓ (parçalı) | `--filter-namespace` parçaları (`SHOPFOLIO_TEST_DATABASE`, Postgres 18, `track_commit_timestamp=on`) | Web 676/676; Files + Payment + SignedEndpoints + Outages + Security + Administration + CorporateContent + FileSecrets + Pages 537 (üç `DigitalDeliveryTests` KK1 kırığı düzeltildi, 33/33); Jobs 85/85; birleştirme sonrası ilgili sınıflar (`NotFoundPageTests`, `StorefrontFrameTests`, `ListComponentPageTests`, `SessionAccessTests`, `SearchEngineDataTests`, `DownloadEndpointTests`, `DigitalDeliveryTests`, `UnexpectedErrorLayerTests`, `StatusEndpointTests`) 229/229 |
| Derleme / biçim | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format <proje> --no-restore --verify-no-changes --include <değişen dosyalar>` (Web, Unit, Integration) | 0 uyarı, 0 hata; biçim temiz |

Model, Database, Catalog, Commands, Concurrency, Disposal, Logging, Ordering ad alanları yerelde koşulmadı (404 gövdesi varsayan test içermiyorlar: `HttpStatusCode.NotFound` taraması); CI koşar.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-11; head `e59bfa5f`) |
| Bulgu sayısı | 0 (S1/S2/S3 yok) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-1216 (E-06 çizimi düşerse kayıt/sayaç yok; forward), K2 → D-1184 metni düzeltildi (T502 KK8; bu PR'da kapatıldı) |

**Validator kanıtları (tur 1):**

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 404 vitrinin tam çerçevesinde E-06 | ✓ | `NotFoundPageTests` KK1 üç testi (çerçeve, başlık, "Ana sayfaya dön", birinci seviye kategori; ürünsüzken yol yok; 500 sayfası değil, yönetici şeridi); `_NotFound.cshtml` `04` 5.6.2–5.6.4 ile birebir |
| 2 | 404 hâlleri | ✓ | bilinmeyen ilk bölüm, üye kökü/erişim anahtarı reddi, POST sahiplik reddi, panel kaydı, imzası bozuk/ömrü dolmuş/indirilemez indirme değeri (`NotFoundPageTests`, `SessionAccessTests`, `DownloadEndpointTests.*Is404WithThePageE06*`); öteki hâller kendi task'ında (`11` T504 KK2) |
| 3 | Sebep ayırt edilmez; noindex; sayaç yok; boş kategori bu yanıt değil | ✓ | dört kaynağın işaretlemesi nonce/dönüş adresi dışında aynı; meta robots noindex, no-store, çerez yok, sayaç ve `UnexpectedPageError` satırı yok; bulunan boş sayfa 200 (kategori sayfasının kendi testi T502 KK8, D-1184) |
| 4 | 500 çerçeveleri | ✓ | `TheStorefront500_…`, `ThePanel500_…`, `TheSignInScreensOfThePanel_…_IsBare_…` |
| 5 | POST 500 değerlerle ve kalıcı mesajla | ✓ | `APost500_ShowsTheFormAgainWithTheSentValue_…` |
| 6 | Sonucu belirsiz işlemde 303 | ✓ | `AnUncertainOperationsError_IsA303_…` |
| 7 | Ayrıntı/kod yok; kayıt ve sayaç | ✓ | `TheStorefront500_…_KK4_KK7` (istisna metni ve `ErrorCode` adları yok; sayaç +1; kayıtta `UnexpectedPageError`) |
| 8 | Makine ucunda 500 gövdesiz | ✓ | `Download_OfACompletedRowWhoseFileIsMissingFromTheStore_Is500WithNoBody`; makine ucu 404'ü gövdesiz (`AMachineEndpointsAndAStaticFilesNotFound_StayEmpty_…`, `NotFoundAnswerTests`) |

Yerel sınama (validator): derleme `-warnaserror` 0/0; `dotnet format` üç proje temiz; Unit 6734/6734 + Consistency 295/295; betik testleri 4/4; Integration TAM 3677/3677 (Web 702 — ilk koşuda 5 sınıf yerel Postgres zaman aşımıyla düştü, yeniden koşuda 29/29 —, Files+Pages+SignedEndpoints+Security+FileSecrets 171, Payment+Outages+Administration+CorporateContent+Jobs+kök 451, Model+Database+Catalog+Commands 1595, Concurrency+Disposal+Logging+Ordering 758). CI run 43000656410 (head `e59bfa5f`) 1–8 ve CI Gate yeşil.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T504-hata-sayfalari`
- **Commit:** PR başlığına ve dal geçmişine bakınız (kod: `bdfba2d3`, devam: `51d4ffa5`, birleştirme `0f94d56b`)
- **PR:** #540
- **CI:** yeşil — run 43000656410 (head `e59bfa5f`, doğrulama tur 1)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Boş kategori 200 sınaması | D-1184 (T502) | kategori sayfası T502'de yazılır; bu task'ta katmanın 200'e dokunmadığı sınandı |
| 2 | VAL-B064 [tarayıcı]: E-06 320 genişlikte bozulmadan açılır | tarayıcı sınaması (`12` VAL-B064, BEKLEMEDE) | tarayıcısız PASS'a çekilmez; tek sütun düzeni için kaynak taraması ve CSS var, gözlem doğrulayıcının tarayıcı adımında |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** main son üç run: Publish `42994459437`, CI `42994459582` (`f40f3a9d`), Publish `42991019324` — hepsi success
- **Repo memory (bitiş kapısı 9):** `.claude/CONTEXT.md` Errors satırı güncellendi; MEMORY'ye dokunulmadı (parça kuralı)
- **⚠:** `/statik` altındaki eksik dosya ve makine uçlarının 404'ü gövdesiz bırakıldı (`07 §2.7.2`); admin tarayıcısında panel 404'ü vitrin çerçevesinde E-06 + yönetici şeridi (`07 §5` `NotFound` satırı, `04` 2.9.1).
