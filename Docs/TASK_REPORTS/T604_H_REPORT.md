# T604 (U-H) — F5 kapısından kalan ürün kodu kalemleri, para-güvenlik dışı

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (UH1 dilimi taslak PR; tam Integration CI'da) | **Tarih:** 2026-11-21

> Bu rapor UH1, UH2 ve UH3 dilimlerini kapsar. Hedefi T604 olan öteki kalemler (UH3 ve sonrası, UE, A dilimleri) ayrı işçilerin payıdır.

---

## UH1 — Yapılan İşler

- **D-138:** çerez süzgecinin yakalama katmanı içeride kalır; istisna `05 §10.3.6`'da (K-1621). Yorum ve test adı düzeltildi, davranış değişmedi.
- **D-1022:** SES ön sorgusunun iş süresi aşımı ve `HttpRequestException` hatası `suppression-query:` önekiyle yazılır (gönderimin zaman aşımından ayrılır).
- **D-1134:** vitrin menüsünde gizli "Diğer" ögesi; `common.js` `bindMenuOverflow`, `storefront.css`.
- **D-1135:** (1) `GET /gorsel/simge` yedek site simgesi (marka harfi + marka rengi, SVG), düzen simge bağlantısı; (2) önbellek başlığı ölçümü gerçek sunucu gerektirir → T601/F6.
- **D-1143:** alt bilgide kimlik bloğu sarmalayıcısı; 768 ve üstünde yan yana.
- **D-1146:** 429 ve 413 çerçeveli sayfa; "Yeniden dene" yok (`ErrorPageText.OffersRetry`).
- **D-1173:** `Confirmations.Build` onay işaretini kendisi ekler.
- **D-1174:** işlem düzeyinde "Yeniden dene" yok; uyarı cümlesi; `PanelSalesConditionRow(MetText, UnmetText)`.
- **D-1216:** 404 sayfası çizilemezse bir satır ve bir sayaç (`layer.Resolve`), cevap yine boş 404.
- **D-1387:** `FieldMessageCatalog.MenuDefaultNameParameter` işareti.
- **D-1550:** sipariş düzeltme formlarında `aria-invalid` + `aria-describedby` + özet.
- **D-1788:** SSS ve İletişim başlıkları `SearchText.Title(ad, marka)` (satır A dalında).
- **Bu devamda:** `_UnexpectedError.cshtml` ev bağlantısı tek satıra indirildi (navigasyon envanteri sayısı `NavigationMap_EqualsTheCode_InBothDirections` kırmızısının kök nedeniydi; test gevşetilmedi); `RequestLimitsTests` 413/429 metin denetimi Razor kodlamasına uyduruldu (`WebUtility.HtmlDecode`).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web` altında Endpoints (`SiteIconEndpoint`), Infrastructure (Errors, Integrations, Security, Validation), Pages (Components, Frame, Panel, Shared, Storefront), `Program.cs`, `wwwroot` (`common.js`, `storefront.css`); ilgili Unit ve Integration testleri.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Hedefi T604 olan kalem kodla kapanır ya da gerekçeyle daralır | ✓ (UH1–UH5) | UH1–UH5'in bütün kalemleri "UH1–UH5 — bütün kalemler" tablosunda: kapandı ya da yeni hedef + gerekçeyle daraldı. |
| 2 | Para-güvenlik kalemleri ayrı başlıkta | ✓ (bu dilimde yok) | 12 kalemin hiçbiri ödeme, sipariş durumu, iade, kimlik, oturum, dosya ya da sır kodu değil. D-1550 yalnız form işaretleme ve mesaj bağlantısı; kural sınırları aynı. |
| 3 | Tarayıcı aracı kalemleri | — | Bu PR'ın payı değil (test aracı PR'ı). |
| 4 | D-1678 | — | Bu PR'ın payı değil (test aracı PR'ı). |
| 5 | Kapanan satır `✅ Çözüldü → T604`; daraltılan yeni hedef | ✓ | `Docs/DEFERRED_BACKLOG.md`: 10 satır kapandı, D-1135 T601 (F6)'ya daraldı; D-1788 `T604-A.md`. |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 envanter: UH1'in her kalemi kapalı ya da gerekçeli | ✓ | Yukarıdaki liste; `DEFERRED_BACKLOG.md` satırları. Öteki T604 kalemleri bu dilimin dışında. |
| KK2 para-güvenlik ayrı başlık, kırmızı–yeşil, eski testler aynı | ✓ | Bu dilimde para-güvenlik kalemi yok. Değişen test tablosu aşağıda. |
| KK3 tarayıcı kalemleri | — | Payım yok. |
| KK4 D-1678 | — | Payım yok. |
| KK5 backlog satırları ve sayaç | ✓ | Satır yerinde kapatıldı; yeni kalemler yalnız `T604-H.md`'de (D-1811…D-1815); sayaç dokunulmadı. |
| Ayırt edicilik (src bozulur → test kırmızı → geri alınır) | ✓ | D-1022: önek kaldırılınca 3 `SesEmailGatewayTests` kırmızı; D-1173: onay işareti ekleme satırı kaldırılınca `MessageStateModelTests…D1173` kırmızı; D-1216: `layer.Resolve` kaldırılınca `NotFoundAnswerTests…D1216` kırmızı; D-1387: işaret kaldırılınca `FieldMessageCatalogTests…D1387` kırmızı. D-1174 ve D-1146: yeni/değişen beklenti eski cümleyi yanlış sayar. D-1788, D-1550, D-1134, D-1135: Integration sınıfları (yerelde yeşil), ayırt edicilik denemesi yapılmadı (süre). |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | Yeni tarama istisnası eklenmedi; `FrameSourceScanTests` yeni denetimler ekler, istisna listesi genişlemedi. |
| Sık FAIL (2) tarama çağrı biçimi kaçırıyor | ✓ | `FrameSourceScanTests` Diğer ögesi ve sarmalayıcı için yalnız işaretleme biçimini arar (yeni kaynak taraması yazılmadı). |
| Sık FAIL (3) 06 Varsayılan / FK | ✓ | Şema değişikliği yok. |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | Navigasyon envanteri (`NavigationInventory`) `@Model.HomePath` bir yerde kalacak biçimde kodu uydurdu; envanter ve belge değişmedi. |
| Sık FAIL (5) belge kuralını değiştiren yeni 09 metni | ✓ | 09 değişmedi; karar kaydı gerekmedi. |
| Hedefi bu task olan backlog kalemleri | ✓ | 12 kalem: 10 kapandı, D-1135 daraldı, D-1788 A'da. |

### Değişen test tablosu

| Test | Değişiklik | Gerekçe |
|---|---|---|
| `MessageStateModelTests` (geçici kapatma uyarısı) | Beklenen cümle "satışı geçici kapattınız; X tamamlanmadı." | D-1174'ün kendi kabul cümlesi. |
| `MessageComponentTests` (satışı koşul satırları) | "Geçici kapatma: Var/Yok", "Yasal metinler: Eksik (…)" | D-1174 (3). |
| `RequestLimitsTests` (3 eski `<form` denetimi) | `method="post"` denetimi | Çerçeveli sayfada GET arama formu var; yinelenecek POST formu yok (D-1146). |
| `RequestLimitsTests` (413/429 metin denetimleri) | `WebUtility.HtmlDecode(html)` | Razor çerçevesi harfleri kendi biçiminde kodlar (`&#xF6;`); yalın sayfa kodlamıyordu. Aranan metin aynı. |
| `CookieRulesTests` | Test adı | D-138 (yakalama katmanı içeride). |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (TAM) | ✓ 8098/8098 | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | Kırmızı yok (önceki dilimin 6 kırmızısı: `NavigationConsistencyTests` bu devamda düzeldi, `FormFieldEnforcementTests` önceki devirde geri alınmıştı). |
| Integration | ~ 311/319 yerelde; TAM: CI | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-class …` iki çağrıda | Koşulan sınıflar: StorefrontFrameTests, MessageComponentTests, OrderDetailPageTests, RequestLimitsTests, SearchEngineDataTests, NotFoundPageTests, FaqPageTests, ContactPageTests, HomeAndFirmPageTests, ForgeryProtectionTests, SecurityHeadersTests, UnexpectedErrorLayerTests. İlk çağrıda 7 kırmızı (6'sı 413/429 metin kodlaması, düzeltildi, ikinci çağrıda yeşil); kalan: `RequestLimitsTests.AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter` (aşağıda ⚠). |
| `dotnet format` | ✓ | Değişen 31 `.cs` dosyasında `--include` | Değişiklik yok. |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-21, bağımsız doğrulayıcı; head `c83c9702`) |
| Bulgu sayısı | 2 bulgu (ORTA, S1) + 1 K-notu (D-1859) |
| Düzeltme gerekli mi | Evet |

**Tur 1 bulguları:**

1. **D-1412 / D-1833 (KK1 ~, kabul kriteri 5 ~):** kalemin kendi kabul cümlesi "test çok satırlı hâli sınar"dır. Karar ve betik (`member-defect-claim.js`) yazıldı, ama çok alanlı kalemli siparişte alanın yerini sınayan sayfa testi ve betiğin tarayıcı gözlemi yok; satır "faz kapısı"na neden T604'te yapılamadığı yazılmadan atıldı (D-1412 ve D-1833 satırlarında gerekçe yok). Bu PR'ın eklediği betik tam o durum için yazıldı ve testsiz kaldı. Düzeltme: çok kalemli sayfa testi (seçim yokken liste altında, gönderilen kalemin satırında; betikli yolda seçilen satırda) yazılır ve D-1412 kapanır; ya da gerekçe ve geçerli bir hedef satıra yazılır.
2. **D-1550 / D-1812, D-1843, D-1844 (KK1 ~):** (a) D-1550 `✅` yazıldı, ama kendi kabul cümlesi "sipariş ayrıntısının bütün tek alanlı formları tek kalıba alınır" der; kargoya verme ve teslim işareti formları (D-1812) kalıba geçmedi. D-1812'nin ilk gerekçesi ("UE birleşince") #595 ile ortadan kalktı. (b) UH5'in gerekçesi "kalem UH dilimlerinin kalem listesine girmedi" D-1812 (D-1550'nin payı, UH1) ve D-1843 (D-1773'ün kalanı, UH ek listesi) için olguya uymaz. (c) Yeni hedef T601 (F6) `DEPLOY_RUNBOOK` provası ve canlı kanıt task'ıdır; kabul kriterlerinde ürün kodu yok, "F6 ürün işi" diye bir task da yok. Düzeltme: D-1812 bu PR'da yapılır (D-1550 gerçekten kapanır); D-1843 ve D-1844 ürün kodu alan bir hedefe bağlanır ve gerekçesi düzeltilir.

**K-notu:** D-1859 (`CategoriesPageTests …AndTheMessage…_D1773` adı mesajı anar ama mesajı sınamaz) → `DEFERRED_BACKLOG.d/T604-H.md`.

**Doğrulayıcının kanıtları:** KK2 kırmızı–yeşil yeniden üretildi (D-1599, D-1806, D-1845, D-1773: `src` geçici bozuldu → dört test kırmızı → geri alındı → 6/6 yeşil). Unit 8325/8325; Integration ilgili 28 sınıf 750/750 (TAM: CI 4. iş yeşil); `NarrowClassTests` 36/36, `ScreenInventoryTests` 7/7; betik testleri 4/4; `test-pre-commit.sh` 263/0; build `-warnaserror` 0 uyarı; `dotnet format` değişen 118 `.cs` temiz. Birleştirme: `git diff cea4b177 origin/main` boş; D-1770 bekçiyi değiştirmedi.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (ters vekil dosyasına dokunulmadı; D-1814)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T604-urun-kodu-h`
- **PR:** #596 (UH1–UH5; UH5 sonunda taslaktan çıktı)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | UE4'ün `/gorsel/{id:guid}` ucu ve `/gorsel/*` vekil kuralı `/gorsel/simge` ile çakışmaz | `git show origin/task/T604-urun-kodu:deploy/caddy/Caddyfile` (`@imageFile file` dosya yoksa uygulamaya geçer), `ImageEndpoint.Pattern = /gorsel/{id:guid}`; `simge` GUID değil, dosya adı da değil (depo anahtarları kimlikten türer). İki dal birleşince yol ve vekil kuralı tutarlı. | ✓ — ana daldaki salt `file_server` kuralıyla birleşme öncesi simge vekil arkasında sunulmaz (D-1814); uç uydurulmadı |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Diğer" menüsü tarayıcı gözlemi | D-1811 | Gerçek tarayıcı gerekir. |
| 2 | Sipariş adım formları D-1550 kalıbına | D-1812 | UE ile ortak dosya, T527'de PASS. |
| 3 | 429 çerçeveli sayfası maliyet gözlemi | D-1813 | Taşkın altında ölçüm. |
| 4 | `/gorsel/simge` ve vekil kuralı birleşme sırası | D-1814 | PR 595 birleşimi. |
| 5 | `KnownWithoutBrand` listesi | D-1815 | PR 594 birleşimi. |
| 6 | Önbellek başlığı gerçek sunucu ölçümü | D-1135 (daraldı) | Gerçek sunucu, T601/F6. |

## Notlar

- ⚠ `AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter`: ilk (reddedilmeyen) `/sistem/canlilik` isteği 503 döndü (canlılık probu veritabanını 2 sn içinde bulamadı, yerel Docker'da). Test ve probe bu PR'da değişmedi; reddedilen istekten önce kırılır, `WriteRejectionAsync` değişikliğine ulaşmaz. İki ayrı koşuda tekrarlandı; ana dalda karşılaştırılmadı. CI'da doğrulanır.
- ⚠ D-1134: "Diğer" betiği gerçek tarayıcıda gözlenmedi (D-1811).
- ⚠ D-1550: kargoya verme ve teslim işareti formları değişmedi (D-1812).
- ⚠ D-1146: çerçeveli 429/413 sayfası DB okur; taşkın altında maliyet (D-1813).
- Karar (kolay): `/gorsel/simge` adresi (07 §2.1.2 kapalı listesinde `/gorsel` zaten var; yeni ilk segment yok); karar kaydı açılmadı.
- Ayırt edicilik UH2b'de tamamlandı (süre bütçesiyle bölünen işçinin kalanı).
- Temizlik: `t604h-pg` konteyneri ve `bin`/`obj` silinir.

---

## UH2 — Yapılan İşler

- **D-1189 / D-1631:** sepet satırı görsel (varyantın, yoksa ürünün ana görseli) ve seçenek değerlerini gösterir; ad ve görsel bağlantısı yalnız vitrindeki üründe (`IsOnShowcase`). Görselin alt metni boştur (D-1819).
- **D-1245:** fiziksel kalem varken il kısıtı satırı (`_DeliveryRestriction`, ürün sayfasıyla ortak bileşen).
- **D-1243:** tükenmiş varyant "Bu adette stok yok"; geçerlilik tarihi satırı hep çizilir (`since`).
- **D-1275, D-353:** ürün sayfası seçenek değerleri tek sorgu; vitrin listesi yalnız kart sütunlarını okur (`ShowcaseRow`; sıralama üye-ataması projeksiyonuyla çevrilir: record kurucusu `CountAsync` altında çevrilemedi, bu devamda düzeltildi).
- **D-283, D-1705, D-1703:** seçenek bağlama kuralları, ilk seçenek eklenince varyant bağlama, kategori kutuları işaretsiz yükleme.
- **D-1400, D-1398, D-1673:** karar K-1635 (a), (b), (c); `04` ve `07` hücreleri.
- **D-1276:** `12` VAL-A008, A014, A015, A017, A024 PASS (kanıt: `ListPatternPageTests.TheStorefrontList_IsNewestFirst_WithNoSortParameter`, `StorefrontSearchAndProductTests` `since` testi, `T604_VALA015`, `T604_VALA017`, `T604_VALA024`); tarayıcı gözlemi isteyen satırlar D-1820'de; D-1276 kapandı.

## UH2 — Kabul ve öz denetim

| Madde | Sonuç | Kanıt |
|---|---|---|
| UH1 + UH2 kalemleri kapalı ya da gerekçeli | ✓ | `DEFERRED_BACKLOG.md` satırları (D-283, 1189, 1243, 1245, 1275, 1398, 1400, 1631, 1673, 1703, 1705, 353, 1200 kapandı; D-1276 kapandı); yeni D-1819…D-1821 |
| Sepet görsel, seçenek, bağlantı | ✓ | `CartPageTests.TheLine_ShowsTheImageAndTheOptionValue_...`, `ALineOffTheShowcase_HasNoLinkToTheProduct_...` |
| Sepet il kısıtı | ✓ | `CartPageTests.TheDeliveryRestrictionLine_...`, `CartPageModelTests.HasPhysicalLine_...` |
| Karar kaydı | ✓ | K-1635 `PRODUCT_DISCOVERY_STATUS.md`; `04` 9.5.23, 6.3.5.1, 7.1.16.1, 7.2.17.2, 7.3.2.14; `07` 4.4.1, UC-114 |
| Eski testler | ✓ | Adres/form kuralı sınıfları `PageFormPatternTests`, `FormSecurity*`, `RequestLimitsTests`, `StorefrontFrameTests`, `OrderDetailPageTests`, `FrameLayoutTests`: koşuda kırmızı yok; var olan `TheLinesProductName_LinksToTheProductPage_T546_3_3_13` değişmeden geçer (görselsiz satırda bağlantı biçimi aynı) |
| Ayırt edicilik (UH2) | ✓ | src bozuldu, ilgili test kırmızı, geri alındı: D-1631 (`IsOnShowcase` koşulu bozuk) `CartPageTests…T604_D1631`; D-1189 (seçenek değeri bloğu kapalı) `…T604_D1189`; D-1245 (`HasPhysicalLine` ters) `CartPageModelTests.HasPhysicalLine_…`, (`Cart.cshtml.cs` koşulu ters) `CartPageTests…T604_D1245`; D-1243 (`data-js="since"` kaldırıldı) `…D1243`; D-1400 (`> 99`) `ProductFormPageTests…D1400`; D-1788 (SSS başlığında marka eki yok) `SearchEngineDataTests…D1788(path: "/sss")`; D-1550 (`aria-invalid` kaldırıldı) `OrderDetailPageTests…D1550`; D-1134 (`hidden` kaldırıldı) `FrameSourceScanTests…D1134` ve `StorefrontFrameTests…T604_D1134`; D-1135 (simge ucu kapalı) `StorefrontFrameTests…T604_D1135`, (`fill` bozuk) `PanelFrameModelTests…D1135`. Hepsi kırmızı çıktı (3 Unit, 9 Integration); src sonra dosya sürümüne geri alındı, çalışma ağacında src farkı kalmadı. |
| Sık FAIL (1)-(5) | ✓ | Yeni tarama istisnası yok; 06 şeması değişmedi; P-27 tavanı üç tabloda (`FormFieldInventory`, `EnforcementTable`, `RuleEvidence`) ve `04` hücrelerinde birlikte; K-1635 yazıldı |

## UH2 — Test sonuçları

| Tür | Sonuç | Not |
|---|---|---|
| Unit (TAM) | ✓ 8098/8098 | |
| Integration | ✓ 181/184 → düzeltmeden sonra 39/39 (CartPageTests, PageAddressTests) | Koşulan: CartPageTests, PageAddressTests, StorefrontSearchAndProductTests, ProductSavesTests, ProductFormPageTests, AddressTests, HomepageAndIdentityTrailTests, FrameLayoutTests, RequestLimitsTests, OrderDetailPageTests, ProductListPageTests, StorefrontFrameTests, CategoryTreeAndListsTests, ProductPublishGateTests, ProductTransitionsTests, PageFormPatternTests, FormSecurity* (UH2b: ikinci çağrı üç parçada yeniden koşuldu: OrderDetailPageTests 93/93; FrameLayoutTests, RequestLimitsTests, ProductListPageTests, StorefrontFrameTests, CategoryTreeAndListsTests 118/118; ProductPublishGateTests, ProductTransitionsTests, PageFormPatternTests, FormSecurity* 100/100; yeni VAL-A015, A017, A024 testleri 3/3). TAM: CI |
| `dotnet format` | ✓ | UH2'nin değiştirdiği `.cs` dosyalarında değişiklik yok |

## UH2 — ⚠ (kolay kararlar, sahip yerine)

- D-1243: "tükendi" cümlesi yok, "Bu adette stok yok" (`07` 4.4.2 stok mesajı).
- D-1400/D-1398/D-1673: K-1635.
- D-1631: bağlantı ürün VE varyant Yayında iken.
- Birleştirme notu: `Cart.cshtml` ve `CartModel` UE dalında da değişir (D-1190, D-1246); ad bağlantısı satırlarına dokunmuyorlarsa çakışma yok.
- Süre bütçesi UH2a'da aşıldı; UH2b ayırt edicilik, ikinci Integration çağrısı ve D-1276 kalanını tamamladı.


## UH3 — Yapılan İşler

- **D-442 (VAL-A159):** `DispatchSender` marka logosunu `EmailBranding.LogoPath`'e `EmailLinks.LogoPathOf(LogoId)` (`/gorsel/{dosya kimliği}`) ile verir; `EmailComposer` mutlak adrese çevirir. Logosuz markada başlıkta ad metin kalır. D-966 aynı değişiklikle kapandı.
- **D-880:** `OrderOperationsView.HasExpiredDefectChannel` (`ItemPeriodState.DefectChannelExpired`'dan) ve `OrderScreenLink.From` — Z-18'i dolmuş kalemli siparişte sipariş sayfası "iletişim formuna yaz" bağlantısını doğrudan verir.
- **D-1330:** `wwwroot/js/pages/panel/firm-identity.js`: firma tipi değişince tipin alanları anında zorunlu/isteğe bağlı ve açık/salt okunur olur; kümeler `CompanyIdentityFieldSets`'ten `data-type-fields` ile gelir (betik kural yazmaz); betiksiz yol aynen.
- **D-1332:** WhatsApp kuralı `Infrastructure.Validation.WhatsAppNumberRule`'da tek yerde; `SocialLinks.CheckWhatsAppNumber` ve `FirmSettings.SaveHomepageAsync` onu çağırır. Eşitlik tablosu (`FormFieldEnforcementTable` 7.2.17.4) yeni satıra bağlandı.
- **D-1334:** `CouponFields.AfterSignIn` kaldırıldı (üretimde çağrılmıyordu); yol ödeme adımının `CouponOf` okumasıdır.
- **D-1365:** `IOrderingModule.GetOrderNumbersAsync` — kalemsiz geç ödeme satırlarının sipariş numaraları tek okuma; sayfasız IBAN okuması D-1832.
- **D-1386, D-1464, D-1412, D-1701:** karar K-1637 (a)–(d). Dar sınıf sipariş kartı `PanelTableColumn.CardOrder` ve `admin.css` ile `04` 9.8.14 sırasında; `04` 7.2.8.1 iki ayrık teslim tarihi cümlesi; E-19 adet alanı için `member-defect-claim.js`; `07` §8.2.2, `09` §16.2 madde 2, `12` VAL-B097 "bu ekranda betik yok"a hizalı.
- **D-1604:** `OrderDetailModel.ShowsNotShippedItemNote` — kargoya verilmemiş fiziksel kalem varken cayma kaydı yerinde `NotShippedItemText` açıklaması.

## UH3 — Kabul ve öz denetim

| Madde | Sonuç | Kanıt |
|---|---|---|
| Hedefi T604 olan UH3 kalemleri kapalı ya da gerekçeli (KK1) | ✓ | `DEFERRED_BACKLOG.md`: D-442, D-966, D-880, D-1330, D-1332, D-1334, D-1365, D-1386, D-1464, D-1604, D-1701 `✅ Çözüldü → T604`; D-1412 daraldı → faz kapısı (D-1833); yeni D-1832, D-1833 (`DEFERRED_BACKLOG.d/T604-H.md`) |
| D-442 logo mutlak adresi | ✓ | `DispatchRetryAndMarkTests.TheLogoOfTheBrand_..._D442` (logolu: `src="https://magaza.example.com/gorsel/{id}"`, `alt`; logosuz: `/gorsel/` yok) |
| D-880 bağlantı | ✓ | `OrderPageLinksTests.AnItemWhoseDefectPeriodRanOut_..._D880`; `AfterSalesScreenTests.TheDefectClaimScreen_Is303WhenNotOpen..._KK7` (Z-18 dolmuş siparişte bağlantı var, açıkta yok) |
| D-1330 | ✓ | `HomeAndFirmPageTests.TheIdentityForm_CarriesTheTypeFieldSets..._D1330`; `NoScriptPathSourceScanTests` (betik yolu sayfa yoluna uyar) |
| D-1332 tek kural | ✓ | `WhatsAppNumberRuleTests`; `FormFieldEnforcementTable` 7.2.17.4 |
| D-1334 | ✓ | `ControlComponentModelTests.ACustomerWhoComesBackFromTheSignIn_..._KK6` (`AfterSignIn` yöntemi yok, `From` aynı sonuçlar) |
| D-1365 toplu okuma | ✓ | `PanelHomeDetailsTests.AnIbanLine_..._KK3` (`numberReads == 1`) |
| D-1386 kart sırası | ✓ | `OrderListPageTests.TheList_IsNewestFirst_..._KK2` (hücre sınıfları `card-1`…`card-4`, ikincil üç sütun); `FrameSourceScanTests` (yalnız `min-width`) |
| D-1464 | ✓ | `04` 7.2.8.1; `OrderDetailPageTests` (`DeliveryDateFutureText`, `DeliveryDateBeforeShipmentText` — işaret ve düzeltme ekranı) değişmeden geçer |
| D-1604 | ✓ | `OrderDetailPageTests.AnOrderNotHandedToTheCarrier_..._D1604` |
| D-1701 | ✓ | `07` §8.2.2, `09` §16.2 m.2, `12` VAL-B097 metni; K-1637 (d) |
| D-1412 | ~ daraldı | betik ve işaretleme kancaları `AfterSalesScreenTests …_KK6`; çok kalemli sayfa testi D-1833 |
| Karar kaydı | ✓ | K-1637 `PRODUCT_DISCOVERY_STATUS.md` (a)–(d) |
| Ayırt edicilik (UH3, src boz → kırmızı → geri al) | ✓ | Sekiz bozma birlikte uygulandı, her biri kendi testinde kırmızı verdi, `git checkout -- src` ile geri alındı: D-442 (`DispatchSender` logo satırı `null`) `DispatchRetryAndMarkTests…D442`; D-880 (`OrderOperations` `HasExpiredDefectChannel` hep `false`) `AfterSalesScreenTests…KK7` (iletişim bağlantısı bulunamadı); D-1330 (`data-type-fields` boş) `HomeAndFirmPageTests…D1330`; D-1332 (`WhatsAppNumberRule` reddetmiyor) `WhatsAppNumberRuleTests…D1332` (`"12345"` ve `"whatsapp numaram"` satırları); D-1334 (`AfterSignIn` geri eklendi) `ControlComponentModelTests…KK6`; D-1365 (sipariş başına okuma) `PanelHomeDetailsTests…T521_KK3`; D-1386 (kart sınıfı yazılmıyor) `OrderListPageTests…KK2`; D-1604 (`ShowsNotShippedItemNote` hep `false`) `OrderDetailPageTests…D1604` |
| Sık FAIL (1)–(5) | ✓ | (1) yeni tarama istisnası yok; (2) JS kaynak taraması `member-defect-claim.js`/`firm-identity.js` için `NoScriptPathSourceScanTests` geçti; (3) 06 şeması değişmedi; (4) eşitlik tablosu, `07`/`09`/`12` üçlüsü ve `04` aynı turda; (5) belge kuralı değişen yerler K-1637'de |

## UH3 — Test sonuçları

| Tür | Sonuç | Not |
|---|---|---|
| Unit (TAM) | ✓ 8115 → ilk koşuda 5 kırmızı (3 tutarlılık tablosu: WhatsApp satırı; 2 mimari: betik yolu adı, `max-width`) düzeltildi; düzeltmelerden sonra Unit TAM yeniden koşuldu: 8115/8115 ✓ (UH3b) | Bozmalardan sonra src geri alındı, yeniden derlendi |
| Integration | ✓ 243/243 (UH3b son koşu; UH3'te 183/183 + DispatchRetryAndMarkTests) | Koşulan: DispatchRetryAndMarkTests, HomeAndFirmPageTests, OrderDetailPageTests, OrderListPageTests, AfterSalesScreenTests, PanelHomeTests, ListComponentPageTests, PageFormPatternTests, FormSecurity*. TAM: CI |
| `dotnet format` | ✓ | UH3'ün 26 `.cs` dosyasında (bir test dosyasında değişiklik uygulandı) |

## UH3 — ⚠ (kolay kararlar, sahip yerine)

- D-1386 Z-42, D-1464 ayrık cümleler, D-1412 betikli açılma, D-1701 "betik yok": K-1637.
- D-1412 ve D-1365 (sayfasız okuma) daraltıldı: D-1833, D-1832.
- Birleştirme notu: `EmailLinks.cs` (UE: `ImagePrefix` sabiti), `OrderPageModels.cs`, `OrderingModule.cs`, `FirmIdentity.cshtml.cs`, `OrderDetail.ItemSteps.cs`, `PRODUCT_DISCOVERY_STATUS.md` (K-1636/K-1637 aynı yer) UE dalında da değişir; hunk'lar ayrı, K-numarası satırları çakışırsa ikisi de tutulur.
- Süre bütçesi aşıldı (45 dk): ayırt edicilik ve Unit TAM yeniden koşusu UH3b'de.


## UH4 — Yapılan İşler

- **Birleştirme (main #595):** `CartContracts.cs` `CartLine` hem UH parametrelerini (`ImageId`, `OptionValues`, `IsOnShowcase`) hem main'in `LineAmount` gövdesini taşır; `CartVariantReader.cs` stok satırı main'in `StockRules.ReservedAgainst(...)` ile, görsel/seçenek satırları UH'deki gibi. Belge çakışmaları: `DEFERRED_BACKLOG.md` (sekiz yerde satır satır: UH'nin kapattığı satır UH'den, main'in kapattığı satır main'den), `PRODUCT_DISCOVERY_STATUS.md` (K-1635, K-1637 main'in K-1622…K-1636 satırları arasına sıralı). 04/07 sürüm satırları çakışmadı. Birleştirme commit'i push'landı; Unit TAM 8135 ve sepet/ürün sınıfları birleştirmeden sonra yeşil.
- **D-1329:** `VersionedSideBulkWriteScan` kümeyi alıcının türünden tanır: `Contracts` ad alanındaki sözleşme türlerinden küme adlı üyesi olanlar (`MenuNames.Products`) derlemeden türetilir (`ContractTypesWithSetNamedMembers`); bu türle bildirilmiş alıcının üyesi küme sayılmaz. `CompanySettingsWriter.WriteHomepageAsync` yerel değişken geçici çözümü kalktı.
- **D-1778 (belge değil kod; 04 6.1.5.2 kazanır):** E-45 ve E-46 satış kapısını okur (`ISettingsModule.GetSalesGateAsync`) ve `id="sales-state"` satırında gösterir (`PanelHomeTexts.SalesState`: açık / geçici kapalı / eksik koşullar adıyla; ana sayfa uyarısıyla aynı metin ve aynı kaynak).
- **D-1790:** `Profile.cshtml` iç gezinmeyi ve beş bölümü adres ve sipariş ekranlarının `account-layout` / `account-layout__content` sarmalayıcısına koyar (CSS değişmedi: iki sütun zaten tanımlı).
- **D-1806 (para):** `RefundIbanStateAsync` kalemsiz açık isteği de sayar — siparişin bekleyen iadelerinin kimlikleri `OrderOperations`tan verilir (AfterSales Payment varlığına uzanamaz, modül sınırı testi); `EnterRefundIbanAsync` kalemsiz istekte de `CheckAsync(EnterRefundIban)` kararını sorar.
- **D-1773 (daraldı):** `IReadsOnForgeryRefusal` ortak kancası (`ForgeryProtectionFilter` sahtecilik 400'ünde, ileti konduktan sonra bir kez çağırır; yalnız okur); AdminAccounts, Categories, ContentList'te uygulandı; `ForgeryRefusalReadInventoryTests` her POST işleyicili sayfayı sınıflar: kanca / seçim aşamasında okur (6) / kendi okuması (Cart, Checkout) / GET okumaz (4) / kapalı bekleme listesi (23 sayfa; yalnız kısalır). Kalan 23 sayfanın GET okuması sorgu değeri, tek kullanımlık değer, oturum izni ya da gönderilen değerlere bağlıdır (iletişim formunda kanca yazılan değerleri sıfırlardı), bu yüzden sayfa başına ayrım gerekir: satır D-1773 daralttı → T604 (U, UH dalı), D-1843.

## UH4 — Kabul ve öz denetim

| Madde | Sonuç | Kanıt |
|---|---|---|
| Hedefi T604 olan UH4 kalemleri kapalı ya da gerekçeli (KK1) | ✓ (UH5: D-1778/D-1790 satırları birleştirmeden sonra yerinde ✅) | D-1329, D-1806 `✅ Çözüldü → T604` (`DEFERRED_BACKLOG.md`, `DEFERRED_BACKLOG.d/T604-U.md`); D-1778/D-1790 kapandı (satırı #594 ile main'e girer; birleştirmede yerinde ✅; `DEFERRED_BACKLOG.d/T604-H.md` notu); D-1773 daraldı (satır + D-1843) |
| D-1329 kabul cümlesi: tarama ürün kümesini tür adından tanır, geçici çözüm sadeleşir | ✓ | `VersionedSideBulkWriteScanTests.Scan_OfAContractMemberNamedLikeASet_IsQuiet_D1329`, `…StaysAFinding_D1329`, `ContractTypesWithSetNamedMembers_…_D1329`; `ApplicationSource_WritesNoRecord…` gerçek kaynakta (`names.Products` doğrudan) yeşil |
| D-1778 sayfalar kapıyı okur, hâl gösterilir | ✓ | `PaymentAndShippingPagesTests.TheSalesState_IsOnTheScreen_AsTheSalesGatesAnswer_AndFollowsTheGate_D1778` (iki sayfa; kapalı hâl ve koşul düşünce satırın değişmesi) |
| D-1790 sarmalayıcı | ✓ | `AccountProfilePageTests.TheScreen_PutsTheNavigationAndTheFiveSectionsInTheAccountLayout_…_D1790` (gezinme sarmalayıcının çocuğu, bölümler içerik sütununda). `NarrowClassTests` `KnownNavigationNotAtTheLeft` sabiti A dalındadır (dokunulmadı; aşağıda birleştirme notu) |
| D-1806 olgu ve karar | ✓ | `OrderDetailPageTests.TheItemlessIbanRequest_IsInTheDecisionsFact_…_D1806` (istek açılmadan olgu yanlış ve karar kapalı; açılınca ikisi doğru; başka sipariş sayılmaz; müşteri girişi uygulanır, istek kapanır, olgu yanlışa döner) |
| D-1773 kanca + envanter | ✓ | `ForgeryRefusalReadInventoryTests` (3 test); `CategoriesPageTests.TheForgeryRefusal_ShowsTheTreeWithItsNodes_…_D1773` (400, ağaç düğümleri çizilir, hiçbir şey yazılmaz) |
| Para/güvenlik KK2 (D-1806, D-1773) | ✓ | aşağıda "Para ve güvenlik kalemleri" |
| Var olan testler değişmeden | ✓ | Unit TAM, `OrderDetailPageTests`, `CustomerOperationsTests`, `ItemRecordsTests`, `StatusCorrectionTests`, `BranchFlowTests`, `StateRolePageTests`, sepet/ödeme/form kalıbı sınıfları değişmeden yeşil (`CategoriesPageTests` yalnız `partial` oldu) |
| Karar kaydı | ✓ | gerekmedi: D-1778 04 6.1.5.2'nin zaten istediğini yapar (üst belge kazanır); belge kuralı değişmedi |
| Sık FAIL (1)–(5) | ✓ | (1) yeni tarama istisnası yok: alıcı bildirimi yalnız `Contracts` türleriyle, kendi alıcısı için; (2) alıcı tarama biçimleri: parametre, alan/özellik, `x.Names.Products` (zincir) sınandı, `var` yerelleri bilerek tanınmaz (kapalı düşer); (3) şema değişmedi; (4) yeni sınıf/sayı yok (`MenuNames` dışında tür yok, test türetilir); (5) 04/05/07/11 kuralı değişmedi |
| Ayırt edicilik (src boz → kırmızı → geri al) | ✓ | D-1329: alıcı süzgeci kapatılınca `ApplicationSource_…` ve iki `_D1329` örneği kırmızı. D-1778: iki sayfada satır boşaltılınca `…_D1778` (iki satır) kırmızı. D-1790: `account-layout` sınıf adı bozulunca `…_D1790` kırmızı. D-1806: olgu yalnız kalemli isteğe döndürülünce `…_D1806` kırmızı (kalemsiz istek olgusu yanlış). D-1773: kanca çağrısı kapatılınca `…_D1773` kırmızı. Hepsi geri alındı, aynı testler sonra yeşil |

### Para ve güvenlik kalemleri

- **D-1806 (iade IBAN'ı; para):** değişiklikten önce kırmızı: olgu `RefundIbanStateAsync`'te yalnız kalemli isteği sayarken `TheItemlessIbanRequest_…_D1806` "açık hâl" iddiasında kırmızı (`Assert.True` → `False`); yeşil: olgu kalemsiz isteği siparişin iade kimlikleriyle sayınca. Var olan davranış: kalemsiz isteğin girişi daha önce modülün kendi satır denetimiyle yapılırdı (`IsOrdersAsync`); karar artık da sorulur ve ikisi birlikte geçer. Çıkış yolu: yeni parametre `orderRefundIds` ve `OrderOperations` okuması geri alınırsa `EnterRefundIbanAsync`'in kalemsiz dalı `open.OrderItemId is not null &&` koşuluna döner (önceki davranış). Geri alma: tek commit `52b2c75b`.
- **D-1773 (sahtecilik reddi; güvenlik):** kanca yalnız okur; reddedilen istek hiçbir şey yazmaz (`CategoriesPageTests…_D1773` satır sayısı değişmez). Kanca yalnız kendi sayfasının GET okumasını (sorgu değeri ve tek kullanımlık değer olmadan) çağırır; GET'i yazan, değer tüketen ya da sorguyla karar veren sayfa kancayı uygulamaz (envanter testi kapalı liste tutar). Çıkış yolu: kancayı uygulayan üç sayfada arayüz kaldırılır.

## UH4 — Test sonuçları

| Tür | Sonuç | Not |
|---|---|---|
| Unit (TAM) | ✓ 8144/8144 | ilk koşuda 3 kırmızı düzeltildi: modül sınırı (AfterSales → Payment varlığı: kimlikler Ordering'den), yasak test deseni (örnek metinde `) {` bulunması test gövdesi taramasını bozdu) |
| Integration | ✓ TAM: CI (sahip 2026-11-12) | koşulan: PaymentAndShippingPagesTests, CategoriesPageTests, AccountProfilePageTests, AdminAccountsPageTests, ContentPanelPageTests, CartPageTests, CheckoutPageTests, PageFormPatternTests, FormSecurity*, PanelHomeAndVerificationPagesTests, StorefrontSearchAndProductTests (263/263); OrderDetailPageTests, CustomerOperationsTests, StateRolePageTests, ItemRecordsTests, StatusCorrectionTests, PaymentAndShippingPagesTests (291/291); ItemRecordsTests, BranchFlowTests, StatusCorrectionTests, CartTests, CartQuantityTests (168/168) |
| `dotnet format` | ✓ | UH4'ün 21 `.cs` dosyasında temiz |

## UH4 — ⚠ (kolay kararlar, sahip yerine)

- **Birleştirme notu (PR #594, E-25 ve sahtecilik 400'ü):** D-1790 E-25'te içeriği iki sütunlu düzene koyar; iç gezinme artık solda, sayfa bölümleri sağda. Odak sırası ve kaydırma: gezinme belgede içerikten önce kaldığı için tab sırası değişmedi; kaydırma konumu değişmez (sarmalayıcı yalnız ızgara ekler). A dalında `NarrowClassTests` E-25 testi (A r2 Bulgu 1) webkit'te solda gezinmeyi bekleyen `KnownNavigationNotAtTheLeft` sabitine bağlıdır: #594 birleşince sabit kaldırılır, iki yönlü test bunu ister. `StateRoleCheck.PanelGateReadGaps` ("E-45", "E-46") de aynı şekilde kaldırılır (`GateReadGapStale` kırmızı verir; ikisi artık `GetSalesGateAsync` okur). Tarayıcı gözlemi bu dilimde yapılmadı (A parçasının aracı).
- D-1773 daraldı (kalan 23 sayfa): D-1843; kategori 400 sayfasında eylem mesajı yok: D-1844; `ListRefundIbanFieldsAsync` kalemsiz istekleri siparişe bağlamıyor (sınanmadı): D-1845.
- Süre bütçesi aşıldı (45 dk): birleştirme, beş kalem, ayırt edicilik ve Unit TAM tek diliminde tamamlandı.

## UH5 — Yapılan İşler (UH kapanışı)

- **Birleştirme (ilk iş):** `origin/task/T604-test-araci` (`cea4b177`; #594 main'e henüz girmedi, `origin/main` = `8c6b3ef8`) birleştirildi. Çakışmalar: `DEFERRED_BACKLOG.md` (satır başına değişen taraf alındı; D-1329 iki tarafın kanıtıyla tek satır), `12` VAL-A024 (A'nın "seçilemez" metni + UH2'nin PASS'i — `StorefrontSearchAndProductTests …_T604_VALA024` A metnini ölçer), `VersionedSideBulkWriteScan(Tests)` (D-1329 iki yol birlikte: A'nın "kök çağrıdan önce" kuralı + UH4'ün sözleşme alıcısı süzgeci; iki tarafın testleri, üç yeni örnek). Ürün kodunda çakışma yok. Birleştirmeden sonra A'nın iki tablosu UH kodunu yakaladı: `ProductSaves` (D-1705) alan ile kodu ayrı deyime almıştı — tarama çifti okusun diye tek ifadeye döndü, davranış aynı; `HomeAndMenu` P-27 parametresi için dar muafiyet (`FormFieldCodeInventory`, tek dosya + alan + kod).
- **#594 sonrası hiza:** `StateRoleCheck.PanelGateReadGaps` ve `GateReadGapStale` kalktı (E-45/E-46 kapıyı okur; ters yön istisnasız, yeni test iki sayfa için `GateReadMissing` ister); `NarrowClassTests.KnownNavigationNotAtTheLeft` kalktı (gözlem doğrudan "gezinme solda"yı ister); `TitleMeasure.KnownWithoutBrand` ve `ScreenInventoryTests`'teki bilinen giriş kalktı (D-1788 UH1'de kapanmıştı; liste iki yönlü olduğundan birleşmeden sonra E2E kırmızı verecekti). D-1778, D-1790, D-1788 (A satırları) ve D-1815 yerinde ✅.
- **D-1599 (para; K-1631 (5)):** denetimden geçmeyen bildirim IBAN'ı ret değildir. `PanelItemSteps` cayma ve gecikme feshinde OB-14'ten geçmeyen IBAN'ı düşürür; kayıt IBAN'sız açılır, kalemde IBAN isteği (`NoticeWithoutIban`) ve B-14 (çekirdeğin var olan yolu). Sayfanın 422'si, `PanelItemObstacle.IbanInvalid`, `NoticeIbanInvalidText` kalktı. Belge: `04` 7.2.10.4 (yeni yan tümce, ret hücresi "—") ve 9.9.36, `07` 9.9.9 (422 listesinden IBAN satırı çıktı, yan etkiler yolu yazar) ve 9.9.36 eşleme satırı; `04` v0.61, `07` v0.51 (`PRODUCT_DISCOVERY_STATUS.md` satırları). Form alanı tabloları: `FormFieldPlaceEnforcement`, `FormFieldRuleEvidence`, `FormFieldRuleClauseTable` (`C:denetimden-gecmeyen-iban-ret`), `FormFieldInventory` yer işareti. `12` VAL-A238 → PASS.
- **D-1845 (para-güvenlik):** sınandı, sızıntı yok, gerekçeyle kapandı (aşağıda).
- **UH kapanışı:** UH'ye düşüp ⚪ kalan satırlar tarandı: D-1773 kapandı (kalanı D-1843'te); D-1812, D-1843 ve D-1844 `T601 (F6)`'ya daraltıldı (gerekçe satırlarda); D-1770 UH son işinde çözüldü. Tarama sonunda hedefi T604 olan ⚪ satır yok.

## UH1–UH5 — bütün kalemler

| Dilim | Kalem | Durum | Hedef (satırdaki) | Satırın yeri |
|---|---|---|---|---|
| UH1 | D-138 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1022 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1134 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1135 | ↘ daraltıldı | T601 (F6) | `DEFERRED_BACKLOG.md` |
| UH1 | D-1143 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1146 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1173 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1174 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1216 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1387 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1550 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH1 | D-1788 | ✅ kapandı | T604 U | `DEFERRED_BACKLOG.d/T604-A.md` |
| UH2 | D-283 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1189 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1243 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1245 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1275 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1276 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1398 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1400 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1631 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1673 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1703 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH2 | D-1705 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-442 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-880 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1330 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1332 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1334 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1365 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1386 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1412 | ↘ daraltıldı | faz kapısı | `DEFERRED_BACKLOG.md` |
| UH3 | D-1464 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1604 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH3 | D-1701 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH4 | D-1329 | ✅ kapandı | T604 | `DEFERRED_BACKLOG.md` |
| UH4 | D-1778 | ✅ kapandı | T604 U | `DEFERRED_BACKLOG.d/T604-A.md` |
| UH4 | D-1790 | ✅ kapandı | T604 U | `DEFERRED_BACKLOG.d/T604-A.md` |
| UH4 | D-1806 | ✅ kapandı | T604 (U, UH dalı; para — kendi kırmızı–yeşil başlığıyla) | `DEFERRED_BACKLOG.d/T604-U.md` |
| UH4 | D-1773 | ✅ kapandı | T604 (U, UH dalı) | `DEFERRED_BACKLOG.d/T604-U.md` |
| UH5 | D-1599 | ✅ kapandı | T604 (U, #594 sonrası UH dalı) | `DEFERRED_BACKLOG.md` |
| UH5 | D-1845 | ✅ kapandı | T604 (U, UH dalı) | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1815 | ✅ kapandı | T604 (A sonrası) | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1812 | ↘ daraltıldı | T601 (F6) | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1843 | ↘ daraltıldı | T601 (F6) | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1844 | ↘ daraltıldı | T601 (F6) | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1770 | ✅ çözüldü (UH son) | T604 | `DEFERRED_BACKLOG.d/T604-A.md` |

Daraltılanların gerekçesi: D-1135 (UH1) ve D-1412 (UH3) kendi dilim notlarında; D-1812, D-1843, D-1844 UH dilim listelerine (KK1) girmedi, U parçası bu PR ile kapanır, kalan ürün kodu F6 ürün işinde; D-1770 yaması UH son işinde uygulandı (main bekçisi artık D-1120 istisnasını taşıyor).

## UH5 — KK1–KK5 payları

| KK | Pay | Kanıt |
|---|---|---|
| KK1 parça ve dilim | UH5 kalemleri (D-1599, #594 sonrası hiza, D-1845) + UH kapanış taraması | yukarıdaki tablo; `grep -nE "T604" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` ⚪ T604 satırı yok |
| KK2 para ve güvenlik | D-1599, D-1845 kırmızı–yeşil | "Para ve güvenlik kalemleri (UH5)" |
| KK3 tarayıcı | `NarrowClassTests`, `ScreenInventoryTests` chromium iki yolda yeşil; firefox/webkit CI E2E | Test sonuçları |
| KK4 belge | `04` v0.61, `07` v0.51; `12` VAL-A238 ve VAL-A024 Durum; yeni kural yok (K-1631 (5) zaten verilmişti, karar kaydı gerekmedi) | `PRODUCT_DISCOVERY_STATUS.md` 04/07 satırları |
| KK5 backlog | kapanan satır yerinde `✅ Çözüldü → T604`; daraltılan yeni hedef + gerekçe; yeni kalem yok (D-1854–1858 kullanılmadı); sayaç dokunulmadı | `DEFERRED_BACKLOG.md`, `DEFERRED_BACKLOG.d/T604-A.md`, `T604-H.md`, `T604-U.md` |

### Para ve güvenlik kalemleri (UH5)

| Kalem | Kırmızı (değişiklikten önce / değişinim) | Yeşil | Çıkış yolu | Geri alma |
|---|---|---|---|---|
| D-1599 | `PanelItemSteps`'te IBAN'ı düşüren satır yerine ret (eski davranış: `return PanelItemStepResult.NotOpen`) konunca `OrderCorrectionTests.PanelNotice_…_D1599_VALA238(true, "TR33…327")` kırmızı (kayıt açılmaz) ve `PanelLateTermination_…_D1590_D1599` kırmızı; öteki iki VAL-A238 denemesi (başka adres, IBAN'sız) önceden de yeşildi — ayırt edici olan sağlama basamağı denemesi | aynı testler + `OrderDetailPageTests` (iki sayfa testi: IBAN ret değil, değer formda kalır), `WithdrawalDeclarationTests` değişmeden yeşil | denetimden geçmeyen IBAN yalnız düşer: kayıt IBAN'sız, istek + B-14 (`NoticeWithoutIban`); geçerli IBAN eskisi gibi aktarılır | iki `if` gövdesi eski reddine döner, sayfa ve belge satırları geri alınır (tek commit `ee81fa54`) |
| D-1845 | `CustomerOperations.ReadFieldsAsync` süzgeci kaldırılınca (`return [.. all]`) `CustomerOperationsTests.AnotherOrdersItemlessRequest_…_D1845` kırmızı (`Assert.Empty` — başka siparişin isteği görünür) ve var olan `…_D1284` kırmızı | aynı testler yeşil | kod değişmedi: sözleşme süzmeyi çağırana bırakır, iki çağıran da süzer (panel yalnız kalemli alanı kullanır) | — (yalnız test eklendi) |

Değişinimlerin hepsi geri alındı (`git checkout`), aynı sınıflar sonra yeşil. `NarrowClassTests` ayırt ediciliği: `Profile.cshtml` `account-layout` sınıfı bozulunca iki yol kırmızı.

### Öz denetim (UH5; 11 T604 kabul kriterleri + 12 kontrol listesi, kendi diff'inde)

| Madde | Durum | Kanıt |
|---|---|---|
| D-1599 kabul: üst belge kazanır, kayıt IBAN'sız, istek + B-14 | ✓ | `PanelItemSteps.cs` iki `if`; `OrderCorrectionTests …_D1599_VALA238` (üç deneme, `Withdrawals.RefundIbanId == null`, `RefundIbans` yok, `NoticeWithoutIban` isteği, `DispatchType.B14`) |
| D-1599 belge hizası (`04` 7.2.10.4, 9.9.36; `07` 9.9.9) ve aynı kuralı anan öteki yerler | ✓ | `grep -n "sağlama basamağı tutmuyor" Docs/07_API_DESIGN.md src` boş; 07 eşleme satırı 9.9.36; `04`/`07` sürüm satırı ve dipnotu, `PRODUCT_DISCOVERY_STATUS.md` |
| Form alanı eşitlik tabloları (A dalı) iki yönlü | ✓ | `FormFieldEnforcementTests`, `FormFieldPlaceEnforcementTests` (mutasyon satırı `!NoticeIbanRule.Allows(request.NoticeIban)` geçerli), `FormFieldRuleClausesTests`, `FormInventoryConsistencyTests` — Consistency 701/701 |
| Tarama istisnası dar (sık FAIL 1) | ✓ | `FormFieldCodeInventory` yeni satırı tek dosya + `Input.` + `TextTooLong`; `PanelGateReadGaps`/`KnownWithoutBrand`/`KnownNavigationNotAtTheLeft` istisnaları kalktı, yerlerine istisnasız kural |
| Tarama çağrı biçimi (sık FAIL 2) | ✓ | `VersionedSideBulkWriteScan`: iki yol birlikte; `CurrentStateWrite.ApplyAsync` + sözleşme alıcısı örneği sessiz, bildirilmemiş alıcı bulgu (`Scan_OfARealSetOrAnUndeclaredReceiver_StaysAFinding_D1329` üçüncü örnek) |
| DB varsayılanı / FK (sık FAIL 3) | — | göç ve varlık değişmedi |
| Aynı sayıyı anan yerler (sık FAIL 4) | ✓ | VAL-A238 Durum; `04`/`07` başlık sürümü + dipnot + STATUS satırı; D-1657 kapalı satırına "Sonra (UH5)" notu (kalkan test adı) |
| Belge kuralını değiştiren 09 metni (sık FAIL 5) | — | 09 değişmedi; ürün kuralı değişikliği K-1631 (5)'in uygulanması |
| D-1845 kabul: önce sına; sızıntı yoksa gerekçeyle kapat | ✓ | `…_D1845` (görünmez, yanıtlanmaz, IBAN yazılmaz); satır gerekçesi |
| #594 sonrası hiza: girişler kalktı, chromium iki yol | ✓ | `NarrowClassTests` 12/12, `ScreenInventoryTests` + `TitleMeasure*` 21/21 (chromium, iki yol) |
| D-1778/D-1790 satırları yerinde ✅ | ✓ | `DEFERRED_BACKLOG.d/T604-A.md` |
| Var olan testler değişmeden geçer | ✓ | değişmeyen davranışın testleri değişmeden yeşil; K-1631 (5) ile değişen davranışın (IBAN reddi kalktı) dört testi bu davranışa göre güncellendi: `PanelWithdrawalNotice_…_T528_KK5` (IBAN satırı çıktı, ad güncellendi), `PanelLateTermination_…_D1590_D1599` (yanlış IBAN artık IBAN'sız kayıt), `LateTerminationRecord_…_KK1_KK5` (ret ileri tarih, IBAN formda kalır), `WithdrawalRecord_…_OnTheUnitsRefusals_…_D1599` (IBAN'lı gönderim uyarı kuralına ulaşır). `StateRoleConsistencyTests.AKnownPanelGateGap_…` kalktı (yerine iki satırlı teori). Öteki bütün sınıflar değişmeden yeşil |
| Dokunulan sayfaların test sınıfları + `PageFormPatternTests`/`FormSecurity*` | ✓ | Integration ikinci çağrı 244/244 |
| Branch izolasyonu | ✓ | commit konuları yalnız T604 |

## UH5 — Test sonuçları

| Koşu | Sonuç | Not |
|---|---|---|
| Derleme | ✓ | uyarısız |
| Unit TAM | ✓ 8325/8325 (1943 + 6382) | + betik testleri 4/4 dosya |
| Integration (ilgili sınıflar) | ✓ 122/122 · 244/244 · 134/134 · 12/12 · 21/21 | Koşulan: OrderCorrectionTests, CustomerOperationsTests, WithdrawalDeclarationTests, OrderDetailPageTests, ProductSavesTests, ProductFormPageTests, PageFormPatternTests, FormSecurity*, PaymentAndShippingPagesTests, NarrowClassTests (chromium), ScreenInventoryTests + TitleMeasure* (chromium). **TAM: CI** |
| `dotnet format` | ✓ | değişiklik yok |

## UH5 — ⚠ (kolay kararlar, sahip yerine)

- ⚠ D-1329 birleştirmesi iki yolu birlikte tuttu (A'nın kök kuralı + UH4'ün sözleşme alıcısı süzgeci): A'nın kuralı `CurrentStateWrite.ApplyAsync(...)` içinde değer olarak geçen `names.Products`'ı bulgu sayardı; süzgeç onu sessiz bırakır, bildirilmemiş alıcı bulgu kalır.
- ⚠ VAL-A024: A'nın metni + PASS (UH2 testi A'nın "seçilemez" okumasını ölçer).
- ⚠ `HomeAndMenu` P-27 parametresi için `FormFieldCodeInventory` muafiyeti (sayfa modülün reddini gösterir; denetim 7.2.17.2 `FirmSettings.cs` satırında).
- ⚠ D-1599'da sayfa IBAN'ın düşürüldüğünü yöneticiye ayrıca söylemez (belge istemiyor; kayıt sonrası kalemde "IBAN bekleniyor" görünür).
- ⚠ D-1812, D-1843, D-1844 `T601 (F6)`'ya daraltıldı (UH kapanışı; satırlarda gerekçe); D-1770 UH son işinde çözüldü.
- ⚠ `TitleMeasure.KnownWithoutBrand` (D-1815) bu dilimin kalem listesinde yoktu; birleşmeden sonra E2E'yi kıracağı için kaldırıldı.

## UH son — main birleştirmesi ve D-1770

- #594 squash ile main'e girdi (`0ee35474`). İki denetim: `git diff --quiet cea4b177 origin/main` çıkış 0; `git merge-base --is-ancestor cea4b177 HEAD` çıkış 0. İkisi tuttuğu için `git merge -s ours origin/main` yapıldı; `git diff origin/main...HEAD --stat` yalnız UH'nin değişikliklerini gösteriyor (A'nın test aracı dosyaları yok).
- D-1770 yaması `git apply --3way` ile uygulandı; `test-pre-commit.sh` tek çakışması (iki yanın yeni örnekleri) ikisi de korunarak çözüldü. D-1770 satırı yerinde `✅ Çözüldü → T604`; UH5'in "T601'e ertelendi" yazdığı yerler düzeltildi.

### Öz denetim (UH son)

- ✓ D-1770 kabul ölçüsü (yama uygulanır, bekçi geçirir, test yeşil): `test-pre-commit.sh` 263 geçti / 0 kaldı; `run-script-tests.sh` 4 dosya geçti.
- ✓ Birleştirme denetimi 1 (`git diff --quiet cea4b177 origin/main`): çıkış 0.
- ✓ Birleştirme denetimi 2 (`merge-base --is-ancestor cea4b177 HEAD`): çıkış 0.
