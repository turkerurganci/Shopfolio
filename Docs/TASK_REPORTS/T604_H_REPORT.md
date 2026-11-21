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
| Doğrulama durumu | ✗ FAIL — tur 3 (2026-11-21, bağımsız doğrulayıcı, eskalasyon sonrası TAM yeniden doğrulama; head `b76b534b`) · tur 2 ✗ FAIL (head `40ab5981`) · tur 1 ✗ FAIL (head `c83c9702`) |
| Bulgu sayısı | Tur 3: 1 bulgu (ORTA, S2) + 1 K-notu (K1 → D-1874) · tur 2: 1 bulgu (ORTA, S1) + 2 K-notu (K1 bu PR'da kapatıldı, K2 → D-1867) · tur 1: 2 bulgu + 1 K-notu |
| Düzeltme gerekli mi | Evet |

**Tur 3 bulgusu:**

1. **Erişilebilirlik denetimi, E-37 betiksiz yol (WCAG 3.3.1; D-1550, D-1812'nin kalıbı) — S2:** dalın ilk tamamlanan E2E koşusunda (run 44908644455, 8. E2E) `AccessibilityAuditTests.EveryScreenOfTheInventory_HasAResultUnderEachOfTheFiveHeadings_InTheWay_KK2` betiksiz yolda üç motorda (chromium, firefox, webkit) kırmızı: "Screen x criterion cells with no result: E-37 / 1.3.1, E-37 / 1.4.1, E-37 / 3.3.1". Doğrulayıcı yerelde chromium betiksiz koşuyla yeniden üretti; gözlem kaydında E-37'nin dört formu kalır: `CorrectCarrier`, `CorrectOrderEmail`, `MarkDelivered`, `MarkReshipped` — "hata özeti True …; sorun: the focus is not on the first invalid field or the summary: BODY; gönderim 422". Neden: bu dalın tek alanlı form kalıbına aldığı formlar (`_OrderDetailCorrections.cshtml`, `_OrderShipmentFields.cshtml`, `_OrderDeliveryFields.cshtml`) özeti çizer ama ilk hatalı alana `autofocus` vermez; `_FormErrorSummary` betiksiz yolda odağı alanın kendi `autofocus`'una bırakır (`FormErrorSummary.AutofocusFor`). Ana dalın E2E'si (run 44888714519) yeşildir. Öteki E-37 formları (ör. `RecordWithdrawal`, `CorrectAddress`) geçer. Düzeltme: bu formların ilk hatalı alanına `autofocus` (`AutofocusFor` ile), betiksiz denetim üç motorda yeşil.

**Tur 3 K-notu:** K1 — VAL-A020 gözlemi ifa süresini Ön Bilgilendirme Formu kutusunda değil, onay özetinde (`.checkout__summary-text`) arar; ürün doğru (doğrulayıcı seçiciyi form kutusuna çevirdi, altı koşu yeşil), satır PASS kalır → D-1874 (`DEFERRED_BACKLOG.d/T604-H.md`, F6 gate).

**Tur 3'te kapandığı denetlenen tur 2 maddeleri:** Bulgu 1 (D-1276/D-1820) kapandı — VAL-A010, A012, A016, A018, A020 `ProductPageObservationTests` (yerelde 42/42, üç motor × iki yol; beklenen metinler satırların "Beklenen" sütunundan), VAL-B001 `CheckoutPageTests…_T604_VALB001` (uç yanıtı + veri kaydı); D-1811, D-1819, D-1821 hedef T604 ve kapandı. K2 (D-1867) kapandı — iki kancada `readsQuery: true` yapılınca iki `…_D1867` testi kırmızı.

**Tur 3 doğrulayıcı kanıtları:** KK2 kırmızı–yeşil yeniden üretildi: D-1599 (IBAN düşürme → ret: üç test kırmızı), D-1806 (kalemsiz koşul kesildi), D-1845 (süzgeç kaldırıldı: iki test), D-1843 üç sayfada (Hesabım `keepsPermission: false`; Panel Reset kancası işleyiciyi çağırdı; Profile sarmalayıcısı 400'ü bıraktı), D-1773 (kanca çağrısı kesildi: 16 test), D-1867, D-1819 (iki görsel `alt=""`), D-1821 (`setText("since")` kaldırıldı: betikli üç motor), D-1811 (`common.js` odak satırı kaldırıldı: yalnız webkit betikli), D-1770 (iki betik main sürümüne dönünce `test-pre-commit.sh` 261/2). Geri alınınca hepsi yeşil. Unit 8325/8325; Integration ilgili 46 sınıf 1145/1145 (TAM: CI 4. iş yeşil); tarayıcı 49 + 42 + 645 yeşil; betik testleri 4/4; build `-warnaserror` 0 uyarı; `dotnet format` değişen 174 `.cs` temiz.

**Tur 2 bulgusu:**

1. **D-1276 / D-1820 (KK1 ~, kabul kriteri 1 ve 5 ~):** D-1276'nın F5 kapısı daraltması `12` KP-2…KP-7 satırlarının (A008, A010, A012, A014–A018, A020, A024, B001) BEKLEMEDE'den kanıtla çıkmasını T604'e verdi. UH2 beşini PASS yaptı, altısını (VAL-A010, A012, A016, A018, A020, B001) D-1820 ile T601'e attı; gerekçe "kanıt türü tarayıcı gözlemi; tarayıcısız PASS sayılmaz; T601/F6 tarayıcı turunda gözlenir". Gerekçe olguya uymaz: (a) VAL-B001'in kanıt türü "Uç yanıtı + Veri kaydı"dır, tarayıcı istemez; (b) öteki beşinin kanıt ortamı "yok"tur ve tarayıcı aracı (T551, T604 A) yerelde üç motorda çalışır — bu PR da `DefectClaimFieldTests`'i onunla yazdı; araç varken tarayıcı gözlemi ertelenmez (PF-278); (c) T601'in kabul kriterlerinde KP satırı gözlemi yok (KK10 yalnız VAL-F001…F012), 11 ve 12'de "F6 tarayıcı turu" diye bir iş yok — hedef işlevsiz (tur 1 Bulgu 2 (c) ile aynı örüntü). Altı satırın ikisi KRİTİK (A018 indirim satırı, A020 ifa süresi). Komşu: D-1811, D-1821 (bu PR'ın betiklerinin tarayıcı gözlemi) ve D-1819 ("F6 erişilebilirlik turu") de T601'e işlevsiz hedefle atıldı; kendi kalemlerinin kabul ölçüsü dışında oldukları için bulgu değil, ama hedefleri düzeltilmeli. Düzeltme: altı satırın kanıtı bu PR'da üretilir (B001 uç + veri kaydı testi; A010, A012, A016, A018, A020 tarayıcı gözlemi) ve `Durum` PASS olur; ya da gerekçe olguya göre yeniden yazılıp satırları gerçekten koşacak bir hedefe bağlanır. D-1811, D-1819, D-1821'in hedefi geçerli bir yere (ör. F6 gate) alınır.

**Tur 2 K-notları:** K1 — D-1770 satırı `✅ Çözüldü → T604` ama Hedef hücresi `T601 (F6)` kalmıştı; doğrulayıcı kayıt commit'inde `T604` yaptı (`DEFERRED_BACKLOG.d/T604-A.md`). K2 — `IReadsOnForgeryRefusal` sözleşmesi "sorgu değeri okunmaz" der, iki sıfırlama sayfası kullanılmış bağlantının cümlesini sorgudan seçer → D-1867 (`DEFERRED_BACKLOG.d/T604-H.md`).

**Tur 2'de kapandığı denetlenen tur 1 maddeleri:** Bulgu 1 (D-1412/D-1833) kapandı — `AfterSalesScreenTests…_D1412_D1833` ve `DefectClaimFieldTests` üç motorda iki yol; `QuantityFieldRow` seçimi yok sayınca Integration ve betiksiz yol, betikte taşıma kesilince betikli yol kırmızı (doğrulayıcı yeniden üretti). Bulgu 2 kapandı — D-1812 kalıbı (`StepInvalid` kesilince `…_D1812` kırmızı), D-1843 23 sayfa (`WaitingPages` boş), D-1844 mesajı; hedefler T604. K1 (D-1859) kapandı.

**Tur 2 doğrulayıcı kanıtları:** KK2 kırmızı–yeşil yeniden üretildi: D-1599 (IBAN düşürme → ret), D-1806 (kalemsiz istek koşulu kaldırıldı), D-1845 (sipariş süzgeci etkisiz), D-1843 dört sayfada (Hesabım `keepsPermission: false` → izin düştü; Panel Reauth kancasına GET'in düşürmesi eklendi → izin düştü; Panel Reset kancası işleyiciyi çağırdı → bağlantı tüketildi; Profile sarmalayıcısı 400'ü bırakınca içerik boş), D-1773 kanca kesildi → kancalı 16 sayfa testi kırmızı; geri alınınca 40/40 yeşil (üç motor). Unit 8325/8325; Integration ilgili 44 sınıf 1127/1127 (333 + 324 + 470; TAM: CI 4. iş yeşil); tarayıcı `DefectClaimFieldTests`, `NarrowClassTests`, `ScreenInventoryTests` 49/49 üç motor; betik testleri 4/4; `test-pre-commit.sh` 263/0; build `-warnaserror` 0 uyarı; `dotnet format` değişen 168 `.cs` temiz. `-s ours` birleştirmesi: `cea4b177` ağacı `origin/main` ile aynı ve dalın atası; A dosyalarında yalnız kapanan kalemlerin bilinen sapma girişleri kalktı.

**Tur 1 bulguları:**

1. **D-1412 / D-1833 (KK1 ~, kabul kriteri 5 ~):** kalemin kendi kabul cümlesi "test çok satırlı hâli sınar"dır. Karar ve betik (`member-defect-claim.js`) yazıldı, ama çok alanlı kalemli siparişte alanın yerini sınayan sayfa testi ve betiğin tarayıcı gözlemi yok; satır "faz kapısı"na neden T604'te yapılamadığı yazılmadan atıldı (D-1412 ve D-1833 satırlarında gerekçe yok). Bu PR'ın eklediği betik tam o durum için yazıldı ve testsiz kaldı. Düzeltme: çok kalemli sayfa testi (seçim yokken liste altında, gönderilen kalemin satırında; betikli yolda seçilen satırda) yazılır ve D-1412 kapanır; ya da gerekçe ve geçerli bir hedef satıra yazılır.
2. **D-1550 / D-1812, D-1843, D-1844 (KK1 ~):** (a) D-1550 `✅` yazıldı, ama kendi kabul cümlesi "sipariş ayrıntısının bütün tek alanlı formları tek kalıba alınır" der; kargoya verme ve teslim işareti formları (D-1812) kalıba geçmedi. D-1812'nin ilk gerekçesi ("UE birleşince") #595 ile ortadan kalktı. (b) UH5'in gerekçesi "kalem UH dilimlerinin kalem listesine girmedi" D-1812 (D-1550'nin payı, UH1) ve D-1843 (D-1773'ün kalanı, UH ek listesi) için olguya uymaz. (c) Yeni hedef T601 (F6) `DEPLOY_RUNBOOK` provası ve canlı kanıt task'ıdır; kabul kriterlerinde ürün kodu yok, "F6 ürün işi" diye bir task da yok. Düzeltme: D-1812 bu PR'da yapılır (D-1550 gerçekten kapanır); D-1843 ve D-1844 ürün kodu alan bir hedefe bağlanır ve gerekçesi düzeltilir.

**K-notu:** D-1859 (`CategoriesPageTests …AndTheMessage…_D1773` adı mesajı anar ama mesajı sınamaz) → `DEFERRED_BACKLOG.d/T604-H.md`.

**Doğrulayıcının kanıtları:** KK2 kırmızı–yeşil yeniden üretildi (D-1599, D-1806, D-1845, D-1773: `src` geçici bozuldu → dört test kırmızı → geri alındı → 6/6 yeşil). Unit 8325/8325; Integration ilgili 28 sınıf 750/750 (TAM: CI 4. iş yeşil); `NarrowClassTests` 36/36, `ScreenInventoryTests` 7/7; betik testleri 4/4; `test-pre-commit.sh` 263/0; build `-warnaserror` 0 uyarı; `dotnet format` değişen 118 `.cs` temiz. Birleştirme: `git diff cea4b177 origin/main` boş; D-1770 bekçiyi değiştirmedi.

## Düzeltme 1 — tur 1 bulguları ve K1 (2026-11-21; dilim (1)–(3), D-1843 sonraki dilimde)

Başlangıç: working tree temiz; `core.hooksPath` = `scripts/git-hooks`; ana dal son üç tamamlanmış run success (44888714519, 44871351996, 44871351968). Dış varsayım yok (tarayıcı motorları yerelde kurulu: chromium-1217, firefox-1511, webkit-2272). Yönetici kararı: ürün kodu isteyen T604 kalemleri T601'e atılamaz, bu PR'da kapanır.

| Kalem | Ne yapıldı | Kırmızı (değişinim) | Yeşil |
|---|---|---|---|
| D-1412 / D-1833 | `AfterSalesScreenTests.SeveralFieldedItems_…_D1412_D1833` (3, 2, 1 adetli üç kalem: seçim yokken liste altında; gönderilen alanlı kalemin satırında, adet ve seçimle; alansız kalem gönderilince liste altında; tek `Quantity`). `DefectClaimFieldTests` (tarayıcı; `BrowserSeed.NewOrderWithItemsAsync`): betikli yolda seçim alanı satıra taşır, alansız kalemde geri getirir; betiksiz yolda yerinde kalır, ret sayfasında sunucu seçilen satıra çizer | `QuantityFieldRow` seçimi yok sayınca Integration + betiksiz kırmızı; betikte `place(...)` kesilince betikli kırmızı | 1/1 + chromium 2/2 (betikli, betiksiz); firefox/webkit CI E2E |
| D-1812 (D-1550'nin payı) | `OrderDetail.StepForms.cs` (`RefusalIsShipmentField`, `RefusalIsDeliveryField`, `StepSummaryFor`, `StepMessageFor`, `StepMessageId`); `_OrderShipmentFields`/`_OrderDeliveryFields` özet + `aria-invalid` + `aria-describedby` + alan altı mesaj; kural sınırı yerinde, özetsiz; form çizildiğinde adım başındaki mesaj yinelenmez (`ShowsRefusalAtStep`); sıra sorusu yerinde. `OrderDetailPageTests.TheShipmentAndDeliveryMarkForms_…_D1812` (kargoya verme, yeniden kargoya verme, boş ve ileri teslim tarihi) | iki parçada `refused = false` → kırmızı | 1/1; `OrderDetailPageTests` 99/99 |
| D-1844 + K1 (D-1859) | `Categories.cshtml` model durumunun eylem düzeyi mesajlarını sayfa başında yerinde çizer (`categories-action-message`); `CategoriesPageTests …_D1773` mesajı bir kez ve kimliğiyle sınar | döngü boş diziye çevrilince kırmızı | 1/1 |
| D-1843 | hedef T604'e döndü, gerekçe düzeltildi; 23 sayfanın sayfa başına güvenli okuması iki işçide bitti, bkz. "Düzeltme 1 — D-1843 (S)" ve "(P)" ile "Düzeltme 1 — kapanış" | — | — |
| UH5 gerekçesi | rapordaki kalem tablosu, gerekçe paragrafı ve ⚠ satırı; D-1812, D-1843, D-1844, D-1773 (T604-U) backlog satırları | — | — |

Var olan testler değişmeden geçti: `OrderDetailPageTests` (ship/teslim mesaj metni testleri dahil), `AfterSalesScreenTests`, `CategoriesPageTests`, `PageFormPatternTests`, `FormSecurityTests`, `OneTimeValueTests`, `ForgeryProtectionTests`, `MessageComponentTests`, `ComponentMarkupTests`.

| Koşu | Sonuç |
|---|---|
| Derleme `-warnaserror` | ✓ 0 uyarı |
| Unit TAM | ✓ 8325/8325 |
| Integration (ilgili sınıflar) | ✓ 92/92 (AfterSalesScreenTests, CategoriesPageTests, PageFormPatternTests, FormSecurityTests, OneTimeValueTests, ForgeryProtectionTests) · 119/119 (OrderDetailPageTests, MessageComponentTests, ComponentMarkupTests) · 4/4 (D1833 + chromium iki yol + D1773); TAM: CI |
| Kırmızı–yeşil | ✓ dört değişinim → 5/5 kırmızı; geri alma → yeşil |
| `dotnet format` (dilimin değiştirdiği 8 `.cs`) | ✓ değişiklik yok |

**Öz denetim (düzeltme 1 dilimi; tam tablo PR'ı açan dilimde):** KK1 — D-1412, D-1833, D-1812, D-1550, D-1844, D-1859 kabul cümleleriyle kapandı (yukarıdaki tablo), D-1843 açık ve hedefi T604; KK2 — bu dilimde para kalemi yok; KK5 — kapanan satırlar `✅ Çözüldü → T604`, açık satırın hedefi ve gerekçesi yazılı, "Aktif kalem" sayacına dokunulmadı. Sık FAIL: (1) istisna eklenmedi; (2) tarama değişmedi; `FormFieldPlaceEnforcement` kaynak dizgileri (`RefuseAsync(id, ShipmentMissingText…)`, `…DeliveryDateMissingText…`) değişmeden tutuyor; (3) göç yok; (4) D-1550/D-1773 satırları ve rapor tablosu aynı olguyla hizalandı; (5) 09 değişmedi.

### Düzeltme 1 — D-1843 (S): hesap, ürün ve vitrin sayfaları (2026-11-21)

Bölüşüm: yönetici; bu dilim `WaitingPages`'in Account (3), ProductModel ve Storefront (6) satırlarındaki 10 sayfayı aldı; panel sayfaları öteki işçide. Kural: sahtecilik reddinin 400'ü yalnız okur — tek kullanımlık değer tüketilmez, izin düşürülmez ya da harcanmaz, sorgu değeri okunmaz; sayfa GET'inin gösterdiğini ve eylem düzeyi mesajı (`04` 2.7.3.1, D-1844 kalıbı) çizer.

| Sayfa | Okuma yolu | Ne okunur / ne okunmaz | Kırmızı (src geri alındı) | Yeşil (Integration) |
|---|---|---|---|---|
| Storefront/Verification | kanca `IReadsOnForgeryRefusal` | yoldaki anahtarla `ReadEmailLinkAsync` (yalnız okur); sonuç kodu yok, düğme yerinde | ✓ kırmızı | `RegistrationPageTests.TheLandingPagesForgeryRefusal_…_D1843` |
| Storefront/EmailRevert | kanca | yoldaki anahtarla `ReadEmailRestoreLinkAsync`; geri alma ve tüketim yok, "done" hâli yok | ✓ kırmızı | `EmailRevertPageTests.TheForgeryRefusal_…_D1843` |
| Storefront/PasswordReset | kendi sarmalayıcısı (422 yolu 400'ü de kapsar) | ayar adımında bağlantı yalnız okunur (tüketim yok), istek adımında gönderilen adres | ✓ kırmızı | `CustomerPasswordResetAndReauthenticationPageTests.TheResetPagesForgeryRefusal_…_D1843` |
| Account/Reauthentication | kendi sarmalayıcısı (iki formun işlemi) | hesabın yolları okunur; GET'in izin düşürmesi (yazma) yapılmaz | ✓ kırmızı | `…TheReauthenticationScreensForgeryRefusal_…_D1843` |
| Account/Profile | kendi sarmalayıcısı (422 yolu 400'ü de kapsar) | hesap okunur; iznin değeri yalnız denetlenir (düz okuma), harcanmaz ve düşürülmez; gönderilen değerler | ✓ kırmızı | `AccountProfilePageTests.TheForgeryRefusal_…_D1843` |
| Account/Addresses | yeni sarmalayıcı | üyenin kendi kayıtları; gönderilen form açık kalır, düzenleme formu yalnız bu hesabın kaydı için | ✓ kırmızı | `AccountAddressAndOrderHistoryPageTests.TheForgeryRefusal_…_D1843` |
| ProductModel | yeni sarmalayıcı | ürün GET'teki gibi (yönetici önizlemesi dahil), formun adlandırdığı varyant ve yazılan adet; mesaj düğmenin yanında (`BuyState`); ürün yoksa GET'in 404'ü (önceden görünüm istisna atıyordu, 500) | ✓ kırmızı | `StorefrontSearchAndProductTests.TheForgeryRefusal_…_D1843` |
| Storefront/Contact | yeni sarmalayıcı | kimlik bloğu, şubeler, formun kapısı; gönderilen değerler (tuzak alanı boş) | ✓ kırmızı (açık ve kapalı) | `ContactPageTests.TheForgeryRefusal_…_D1843` (iki durum) |
| Storefront/SignIn | var olan sarmalayıcı (okuma zaten doğruydu: GET veritabanını yalnız sorguya bağlı yeniden gönderimde okur) | gönderilen adres ve yerel dönüş yolu; eksik olan eylem mesajıydı | ✓ kırmızı (mesaj) | `CustomerSignInPageTests.TheForgeryRefusal_…_D1843` |
| Storefront/Register | var olan sarmalayıcı (okuma zaten doğruydu) | kapı ve gönderilen değerler; kapalı kapı hâline de eylem mesajı eklendi | — (okuma önceden yeşil; kapalı hâl mesajı yeni) | `RegistrationPageTests.TheRegistrationsForgeryRefusal_…_D1843` (iki durum) |

Envanter: `ForgeryRefusalReadInventoryTests.WaitingPages` bu 10 satırı çıkardı; kanca listesi `EmailRevertModel`, `VerificationModel` ile beşe çıktı; `OwnReadPages` sekiz sayfayla on oldu. `NavigationInventory` 3.7.5: ürün sayfasının 404 kanıtı iki yerde (GET ve 400). Eylem mesajı kimlikleri: `addresses-`, `account-`, `reauth-`, `contact-`, `email-revert-`, `password-reset-`, `sign-in-`, `verification-action-message`.

| Koşu | Sonuç |
|---|---|
| Derleme `-warnaserror` (Web, Unit, Integration) | ✓ 0 uyarı |
| Unit TAM | ✓ 8325/8325 (ilk koşuda `NavigationConsistencyTests` kırmızısı 3.7.5 kanıt sayısıydı; düzeltildi, sınıf yeniden ✓) |
| Integration (ilgili sınıflar, TAM: CI) | ✓ 179/179 (EmailRevertPageTests, RegistrationPageTests, CustomerPasswordResetAndReauthenticationPageTests, AccountAddressAndOrderHistoryPageTests, AccountProfilePageTests, CustomerSignInPageTests) · ✓ 146/146 (ContactPageTests, StorefrontSearchAndProductTests, PageFormPatternTests, OneTimeValueTests, ForgeryProtectionTests, CartPageTests, CategoriesPageTests) · Register kapalı hâl eki sonrası ✓ 74/74 (RegistrationPageTests, CustomerSignInPageTests) |
| Kırmızı–yeşil | ✓ src geri alınınca 12 yeni testin 10'u kırmızı (Register iki durumu önceden yeşil — okuma zaten doğruydu); geri getirince 12/12 yeşil |
| `dotnet format` (dalın değiştirdiği `.cs`) | ✓ değişiklik yok |

**Öz denetim (bu dilimin satırları; UH'nin tam tablosu PR'ı bitiren son işçide):**

| Madde | Durum | Kanıt |
|---|---|---|
| D-1843 — 10 sayfa (S payı) | ✓ | yukarıdaki tablo; `WaitingPages` yalnız panel satırlarını taşır |
| D-1844 payı — eylem mesajı bu 10 sayfada | ✓ | her testte mesaj (çoğunda tek kez ve kimliğiyle) |
| Tek kullanımlık değer tüketilmez (doğrulama, geri alma, sıfırlama bağlantısı; yeniden doğrulama izni) | ✓ | testlerde `UsedAt` boş, `ReauthenticationTokenHash` duruyor, izin verilmedi |
| Ret yolu yazmaz (oturum, deneme, sepet, iletişim isteği, gönderim satırı) | ✓ | testlerin DB denetimleri |
| Sorgu değeri ret yolunda okunmaz | ✓ | kanca ve sarmalayıcılar yalnız yol değeri ve form argümanını okur (PasswordReset'in ayar adımı istisna: bkz. ⚠) |
| Envanter kapalı liste yalnız kısalır | ✓ | `ForgeryRefusalReadInventoryTests` (3 test ✓) |
| Kural sayısını anan öteki yerler | ✓ | kanca listesi testi beşe, `NavigationInventory` 3.7.5 ikiye güncellendi |
| Var olan testler değişmeden geçer | ✓ | yukarıdaki sınıflar; yalnız iki test sınıfı `partial` oldu (`ContactPageTests`, `CustomerSignInPageTests`) |

⚠ (kolay kararlar, sahip yerine):
- Eylem mesajı sekiz sayfada sayfanın başında (D-1844'ün kategori kalıbı); ürün sayfasında düğmenin yanında (var olan `BuyState` iletisi), kayıt sayfasında var olan düğme yanı döngüsü.
- PasswordReset ayar adımı ret yolunda var olan 422 yolunun `LoadStepAsync`'ini kullanır; bu yöntem "tamamlandı" sonuç kodunu sorgudan okur — yalnız kullanılmış bağlantının ifadesini seçer, yazmaz; 422 ile aynı davranış korundu.
- `OwnReadPages` istisnası sayfanın sarmalayıcıyı bildirmesine bakar; 400 davranışının kanıtı her sayfanın Integration testidir (D-1864, DÜŞÜK).

### Düzeltme 1 — D-1843 (P): panel sayfaları (2026-11-21)

Bölüşüm: yönetici; bu dilim `WaitingPages`'in Panel satırındaki 13 sayfayı aldı. Kural S diliminin kuralıdır: 400 yalnız okur — tek kullanımlık değer tüketilmez, izin düşürülmez ya da harcanmaz, sorgu değeri okunmaz. Bütün 13 sayfa ortak kancayı (`IReadsOnForgeryRefusal`) uygular: sayfanın kendi filtresi işleyicinin bağlamını saklar, kanca sayfanın 422 yolunun okumasını (yoldaki anahtar + gönderilen değerler) çağırır. Yoldaki anahtar kayıt değilse 400 yerine GET ve 422'nin 404'ü döner (önceden görünüm boş kayıtla çizilir ya da 500 verirdi). Eylem mesajı ortak `_ForgeryRefusalMessage` parçasıyla sayfanın başında, kimliğiyle (D-1844 payı).

| Sayfa | Ne okunur / ne okunmaz | Kırmızı | Yeşil (Integration) |
|---|---|---|---|
| Panel/OrderDetail | yoldaki siparişin tamamı (422'nin `LoadAsync`'i); yabancı anahtar 404 | ✓ (M1, M2, M3) | `OrderDetailPageTests.TheForgeryRefusal_…_D1843` |
| Panel/ProductForm | yoldaki kayıt + gönderilen form, sayfa listeleri; yabancı anahtar 404 | ✓ (M1, M2, M3) | `ProductFormPageTests.TheForgeryRefusal_…_D1843` |
| Panel/ContentForm | yoldaki tür ve kayıt + gönderilen form + bağlı ürünler; yabancı anahtar 404 | ✓ (M1, M2, M3) | `ContentPanelPageTests.TheForgeryRefusal_…_D1843` |
| Panel/RequestDetail | yoldaki talep; sorgudaki kısa ömürlü bildirim okunmaz; yabancı anahtar 404 (önceden 500) | ✓ (M1, M2, M3) | `RequestsPageTests.TheForgeryRefusal_…_D1843` |
| Panel/Coupons | liste ilk sayfa + adresin kuponu, gönderilen değerler formda; yok olan kupon önceden 404 | ✓ (M1, M2) | `CouponsPageTests.TheForgeryRefusal_…_D1843` |
| Panel/Members | formun taşıdığı e-postanın kaydı (422'nin okuması); silme yok | ✓ (M1, M2) | `MemberRecordPageTests.TheForgeryRefusal_…_D1843` |
| Panel/MyAccount | hesap + gönderilen değerler; yeniden doğrulama değeri yalnız denetlenir; izin düşürülmez (`ReadOwnAccountKeepingReauthenticationAsync`, modülün yeni salt okur yolu) | ✓ (M1, M2, M4) | `MyAccountPageTests.TheForgeryRefusal_OfTheAccountPage_…_D1843` |
| Panel/Reauth | gönderilen işlemin formu; GET'in izin düşürmesi (yazma) yapılmaz | ✓ (M1, M2) | `MyAccountPageTests.TheForgeryRefusal_OfTheReauthentication_…_D1843` |
| Panel/SignIn | gönderilen adres, yalnız yerel dönüş yolu; davetin sorgu değeri okunmaz; oturum açılmaz | ✓ (M1, M2) | `PanelAccessPagesTests.TheForgeryRefusal_OfTheSignIn_…_D1843` |
| Panel/Reset | istek adımında gönderilen adres, ayar adımında bağlantı yalnız okunur (tüketim yok) | ✓ (M1, M2) | `PanelAccessPagesTests.TheForgeryRefusal_OfTheResetAndTheInvitation_…_D1843` |
| Panel/Invite | davet bağlantısı yalnız okunur, gönderilen ad; hesap açılmaz | ✓ (M1, M2) | aynı test |
| Panel/Verify | yoldaki bağlantı yalnız okunur; sonuç kodu sorgudan okunmaz (kullanılmış bağlantı geçersiz hâli) | ✓ (M1, M2) | `PanelHomeAndVerificationPagesTests.TheForgeryRefusal_…_D1843` |
| Panel/Revert | yoldaki geri alma bağlantısı yalnız okunur; sonuç kodu okunmaz | ✓ (M1, M2) | aynı test |

Kırmızı–yeşil değişinimleri: **M1** filtrenin kanca çağrısı kapatıldı → 11/11 kırmızı (yeniden doğrulama testi ilk koşuda varsayılan işlem değeriyle yeşil kaldı; test öteki işlemle yeniden kuruldu, sonra kırmızı); **M2** mesaj parçası çizmez → 11/11 kırmızı; **M3** 400→404 değişimi kapatıldı → 4/4 kırmızı (RequestDetail 500); **M4** hesap sayfası izni düşüren okumaya döndü → 1/1 kırmızı (izin silindi). Geri alınınca 11/11 yeşil.

Envanter: birleştirmeden sonra `WaitingPages` boş (S ve P birlikte 23 sayfa); kanca listesi 18 sayfa. Ortak test yardımcısı `ForgedPosts` (Integration); `CouponsPageTests`, `ContentPanelPageTests`, `ProductFormPageTests` `partial` oldu.

| Koşu | Sonuç |
|---|---|
| Derleme `-warnaserror` (Web, Unit, Integration) | ✓ 0 uyarı |
| Unit TAM | ✓ 8325/8325 (birleştirmeden sonra) |
| Integration (ilgili sınıflar, TAM: CI) | ✓ 150/150 (MyAccountPageTests, PanelAccessPagesTests, PanelHomeAndVerificationPagesTests, CouponsPageTests, MemberRecordPageTests, RequestsPageTests, ForgeryProtectionTests) · ✓ 203/203 (OrderDetailPageTests, ProductFormPageTests, ContentPanelPageTests, CategoriesPageTests, AdminAccountsPageTests) · son değişiklikten sonra MyAccountPageTests ✓ 20/20 |
| `dotnet format` (dalın değiştirdiği `.cs`) | ✓ değişiklik yok |

**Öz denetim (bu dilimin satırları; UH'nin tam tablosu PR'ı bitiren son işçide):**

| Madde | Durum | Kanıt |
|---|---|---|
| D-1843 — 13 panel sayfası (P payı) | ✓ | yukarıdaki tablo; `WaitingPages` boş |
| D-1844 payı — eylem mesajı bu 13 sayfada | ✓ | her testte mesaj; bağlantı sayfalarında kimliğiyle |
| Tek kullanımlık değer tüketilmez (sıfırlama, davet, doğrulama, geri alma bağlantısı; yeniden doğrulama değeri) | ✓ | testlerde `UsedAt` boş, `ReauthenticationTokenHash` duruyor |
| Ret yolu yazmaz (oturum, hesap, gönderim satırı, sipariş, ürün, kupon, içerik, talep, üye) | ✓ | testlerin DB denetimleri |
| İzin düşürülmez (hesap sayfası, yeniden doğrulama ekranı) | ✓ | M4; yeniden doğrulama testinde izin ve işlemi duruyor |
| Sorgu değeri ret yolunda okunmaz | ✓ | kanca yalnız yol değeri ve işleyici argümanını okur (Reset ⚠) |
| Yabancı anahtar 404 (GET ve 422 ile aynı) | ✓ | M3 |
| Envanter kapalı liste yalnız kısalır | ✓ | `ForgeryRefusalReadInventoryTests` (3 test ✓) |
| Kural sayısını anan öteki yerler | ✓ | kanca listesi testi 18 sayfaya; `IReadsOnForgeryRefusal` belge yorumu güvenli ayrımı anlatır |
| Var olan testler değişmeden geçer | ✓ | yukarıdaki sınıflar; `PageFormPatternTests`/`FormSecurity*`/`OneTimeValueTests` Unit TAM içinde |

⚠ (kolay kararlar, sahip yerine):
- Kanca bağlamı: sayfanın kendi filtresi işleyici bağlamını saklar, kanca onu okur (S dilimi aynı işi kendi sarmalayıcısıyla yaptı; iki yol envanterde ayrı sayılır).
- Reset ayar adımı ret yolunda 422'nin `LoadStepAsync`'ini kullanır; "tamamlandı" sonuç kodunu sorgudan okur ama yalnız kullanılmış bağlantının ifadesini seçer, yazmaz (S'nin PasswordReset ⚠'ıyla aynı).
- Kupon formu sahtecilik ve bayat sürüm birlikteyken çakışma görünümüyle çizilir (sürüm denetimi filtreden önce; yazma yok). Hesap sayfasının "hesap yok" 404 yolu testsiz (oturumlu hesabın silinmesi gerekir).

### Düzeltme 1 — kapanış (2026-11-21): D-1843 kaydı, tutarlılık notu, testsiz yol ve bütün öz denetim

İş: D-1843 (S) ve (P) iki işçide bitti; `ForgeryRefusalReadInventoryTests.WaitingPages` boş (`private static readonly string[] WaitingPages = [];`), üç testi yeşil. Kapanış işçisi kodu yalnız iki yerde değiştirdi: (a) testsiz yol: `MyAccountPageTests.TheForgeryRefusal_OfTheAccountPage_WhenTheAccountIsGone_IsTheGetsNotFound_D1843` (oturum duruyor, hesap satırı yabancı anahtar denetimi kapalıyken silinir; sahtecilik 400'ü ve GET ikisi de 404; `MyAccount.cshtml.cs` 404 dönüşümü kaldırılınca 400 döndü, kırmızı; geri alınca yeşil); (b) `ReadOwnAccountKeepingPermissionAsync` adı `ReadOwnAccountKeepingReauthenticationAsync` oldu — `AdminAccountRulesScanTests` (Administration modülünde rol/izin sözcüğü yasağı, 02 10.2.3) adı yakalıyordu: Unit TAM'da tek kırmızıydı (P dilimi bu testi birleştirmeden sonra koşmamış), ad değişince 8325/8325. Davranış aynı.

**Tutarlılık notu (iki kurulum yolu).** S işçisi sayfaların çoğunda sayfanın kendi sarmalayıcısını (422 yolunun okumasını 400'e de bağlayan işleyici sarmalayıcısı) kurdu, yalnız Verification ve EmailRevert kancayı kullandı; P işçisi 13 panel sayfasında ortak kancayı (sayfanın filtresi işleyici bağlamını saklar, kanca 422 okumasını çağırır) kullandı. İkisi aynı kuralı uygular: ret yolu yazmaz, tek kullanımlık değer tüketilmez, izin düşürülmez, sorgu değeri okunmaz, kayıt yoksa GET'in 404'ü, eylem mesajı sayfanın başında (ürün sayfasında düğmenin yanında). Envanter testi iki yolu ayrı sayar: kanca listesi 18 sayfa (`HasHook`), `OwnReadPages` istisnası sarmalayıcılı sayfalar; biri öteki yolun yerine geçmez. Ayrım bilerek bırakıldı (her sayfanın okuması farklı: sorgu, tek kullanımlık değer, oturum izni); tek yola indirme istenirse ayrı iyileştirme (D-1864 `OwnReadPages` sertleştirmesiyle birlikte).

**⚠ (kolay kararlar, sahip yerine):**
- Reset (hem vitrin PasswordReset hem Panel/Reset) ayar adımı ret yolunda 422'nin `LoadStepAsync`'ini kullanır; bu yöntem "tamamlandı" sonuç kodunu sorgudan okur. Yalnız kullanılmış bağlantının ifadesini seçer, yazmaz; 422 ile aynı davranış korundu (sorgu değeri "okunmaz" kuralının tek bilinçli istisnası).
- Kupon formu sahtecilik ve bayat sürüm birlikteyken çakışma görünümüyle çizilir (sürüm denetimi filtreden önce; yazma yok). Kuralla çelişmez: iki ret birlikte olunca kullanıcıya daha eyleme dönük olan gösterilir; ayrı test yazılmadı (iki hata birlikte yalnız elle üretilebilir; kod yolu P'nin `…_D1843` testi ve bayat sürüm testlerinin kesişimi).
- Hesabım "hesap yok" 404 yolu testsizdi; bu kapanışta test yazıldı (yukarıda).
- D-1864 (`OwnReadPages` istisnası yalnız `OnPageHandlerExecutionAsync` bildirimine bakar, DÜŞÜK, test aracı) ve D-1832 (panel ana sayfası IBAN okuması) hedefi faz kapısı kalır: ikisi de ürün davranışını değiştirmez/performans notudur ve T604 kabul ölçüsünde değil; ürün kodu isteyen T604 kalemi değiller.

#### Öz denetim — UH1–UH5, UH son ve düzeltme 1 (bütün kalemler) ile KK1–KK5

| Madde | Durum | Kanıt |
|---|---|---|
| KK1 UH1: D-138, D-1022, D-1134, D-1143, D-1146, D-1173, D-1174, D-1216, D-1387, D-1550, D-1788 kapalı; D-1135 daraldı | ✓ | "UH1–UH5 — bütün kalemler" tablosu; D-1135 satırı hedef `T601 (F6)` + gerekçe (`DEFERRED_BACKLOG.md` 643); D-1022 `SesEmailGatewayTests`, D-1173 `MessageStateModelTests…D1173`, D-1216 `NotFoundAnswerTests…D1216`, D-1387 `FieldMessageCatalogTests…D1387` |
| KK1 UH2: D-283, D-1189, D-1243, D-1245, D-1275, D-1276, D-1398, D-1400, D-1631, D-1673, D-1703, D-1705 | ✓ | aynı tablo; UH2 bölümü "Kabul ve öz denetim"; D-1243 tarayıcı gözlemi düzeltme 2 (B) ile yapıldı: D-1821 ✅ (`ProductPageObservationTests…_D1821`); D-1276'nın tarayıcı payı A010, A012, A016, A018, A020 PASS (B001 öteki işçide) |
| KK1 UH3: D-442, D-880, D-1330, D-1332, D-1334, D-1365, D-1386, D-1412, D-1464, D-1604, D-1701 | ✓ | tablo; D-1412 `AfterSalesScreenTests.SeveralFieldedItems_…_D1412_D1833` + `Browser/DefectClaimFieldTests` (chromium iki yol) |
| KK1 UH4: D-1329, D-1778, D-1790, D-1806, D-1773 | ✓ | tablo; D-1773 `✅` + `ForgeryRefusalReadInventoryTests`; kalan 23 sayfa D-1843'te kapandı (aşağıda) |
| KK1 UH5: D-1599, D-1845, D-1815 | ✓ | `OrderCorrectionTests …_D1599_VALA238`, `CustomerOperationsTests …_D1845`; D-1815 `TitleMeasure.KnownWithoutBrand` kalktı |
| KK1 UH son: D-1770 | ✓ | `test-pre-commit.sh` 263/0, `run-script-tests.sh` geçti |
| KK1 düzeltme 1: D-1412, D-1833, D-1812, D-1550 (tam), D-1844, D-1859 (K1) | ✓ | `AfterSalesScreenTests…_D1412_D1833`; `OrderDetailPageTests.TheShipmentAndDeliveryMarkForms_…_D1812`; `CategoriesPageTests …_D1773` (`categories-action-message`) |
| KK1 düzeltme 1: D-1843 — 23 sayfa | ✓ | `ForgeryRefusalReadInventoryTests` `WaitingPages = []`; S: 10 sayfa, P: 13 sayfa tabloları ve `…_D1843` testleri; `MyAccountPageTests…WhenTheAccountIsGone_…_D1843` |
| KK1 tarama: `grep -nE "T604" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` | ✓ | ⚪ kalan satırlar yalnız ileri atılanlar: D-1135, D-1813, D-1814, D-1819 → T601 (dağıtım provası / erişilebilirlik turu; D-1811 ve D-1821 düzeltme 2 (B) ile ✅, D-1820 hedefi T604'e döndü ve B001 kapanınca kapanır); D-1769 → sahip; D-1789, D-1803, D-1840, D-1841, D-1851, D-1864 → F6 gate; D-1832 → faz kapısı. UH'ye düşüp ⚪ kalan satır yok; hedefler geçerli, hiçbiri ürün kodu isteyen T604 kalemi değil |
| KK2 para ve güvenlik: D-1599, D-1806, D-1845 ayrı başlık, kırmızı–yeşil | ✓ | "Para ve güvenlik kalemleri" (UH4, UH5); D-1843 güvenlik payı (tek kullanımlık değer tüketilmez, izin düşürülmez): S ve P kırmızı–yeşil tabloları, M4 (izin) ve `UsedAt` boş / `ReauthenticationTokenHash` duruyor denetimleri |
| KK3 tarayıcı kalemleri | ✓ | `NarrowClassTests`, `ScreenInventoryTests`, `TitleMeasure*`, `DefectClaimFieldTests` chromium iki yol; firefox/webkit CI E2E; düzeltme 2 (B): `ProductPageObservationTests` yerelde chromium, firefox, webkit, iki yol (42/42) |
| KK1 düzeltme 2 (B): tur 2 Bulgu 1'in tarayıcı payı — VAL-A010, A012, A016, A018, A020 PASS; D-1811, D-1821 ✅; D-1820 ve D-1276 sonuçla güncel | ✓ | `ProductPageObservationTests` (`…_VALA010`, `…_VALA012`, `…_VALA016`, `…_VALA018`, `…_VALA020`, `…_D1811`, `…_D1821`); `12` satırları `PASS`; `12_matris_denetle.py --kapsam 1-77` Hata: 0; `DEFERRED_BACKLOG.d/T604-H.md` ve `DEFERRED_BACKLOG.md` D-1276 satırı; VAL-B001 ve D-1819, D-1867 öteki işçide |
| KK4 belge | ✓ | `04` v0.61, `07` v0.51, `12` VAL-A238/VAL-A024 (UH5); düzeltme 1'de belge değişmedi |
| KK5 backlog: kapanan `✅ Çözüldü → T604`, daraltılanın hedefi + gerekçe, sayaç | ✓ | D-1843 (T604-H.md) ve D-1773 (T604-U.md) `✅` + kanıt; "Aktif kalem" sayacına ve STATUS tablo satırına dokunulmadı; yeni kalem D-1864 (S dilimi) kayıtlı |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | yeni tarama istisnası yok; `ForgeryRefusalReadInventoryTests` kapalı liste yalnız kısalır, şimdi boş; `OwnReadPages` dar (D-1864 sınırı kayıtlı) |
| Sık FAIL (2) kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | kapalı liste iki yolu (kanca, sarmalayıcı) birlikte sayar; her sayfanın 400 davranışını sayfa başına Integration testi kanıtlar |
| Sık FAIL (3) 06 Varsayılan / FK | — | göç ve varlık değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | kanca listesi 18, `NavigationInventory` 3.7.5, D-1773/D-1843/D-1844 satırları ve rapor tabloları aynı olguyla hizalı; eski "kalan 23 sayfa" ifadeleri kapandı notuyla güncel |
| Sık FAIL (5) belge kuralını değiştiren yeni 09 metni | ✓ | 09 değişmedi; karar kaydı gerekmedi |
| Branch izolasyonu | ✓ | commit konuları T604 |

| Kapanış koşusu | Sonuç |
|---|---|
| Unit TAM | ✓ 8325/8325 (isim değişikliğinden sonra; ilk koşuda `AdminAccountRulesScanTests` kırmızı, düzeltildi) |
| Integration (23 sayfanın sınıfları, TAM: CI) | ✓ 361/361 (EmailRevertPageTests, RegistrationPageTests, CustomerPasswordResetAndReauthenticationPageTests, AccountAddressAndOrderHistoryPageTests, AccountProfilePageTests, CustomerSignInPageTests, ContactPageTests, StorefrontSearchAndProductTests, PanelAccessPagesTests, PanelHomeAndVerificationPagesTests, CouponsPageTests, MemberRecordPageTests, RequestsPageTests, ForgeryProtectionTests) · ✓ 224/224 (OrderDetailPageTests, ProductFormPageTests, ContentPanelPageTests, CategoriesPageTests, AdminAccountsPageTests, MyAccountPageTests — yeni test dahil) |
| Yeni test kırmızı–yeşil | ✓ 404 dönüşümü kaldırılınca 400 (kırmızı); geri alınınca yeşil |
| `dotnet format` (değişen `.cs`) | ✓ değişiklik yok |

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
| 1 | "Diğer" menüsü tarayıcı gözlemi | D-1811 ✅ | Düzeltme 2 (B) ile gözlendi (`ProductPageObservationTests`); kapandı. |
| 2 | Sipariş adım formları D-1550 kalıbına | D-1812 | UE ile ortak dosya, T527'de PASS. |
| 3 | 429 çerçeveli sayfası maliyet gözlemi | D-1813 | Taşkın altında ölçüm. |
| 4 | `/gorsel/simge` ve vekil kuralı birleşme sırası | D-1814 | PR 595 birleşimi. |
| 5 | `KnownWithoutBrand` listesi | D-1815 | PR 594 birleşimi. |
| 6 | Önbellek başlığı gerçek sunucu ölçümü | D-1135 (daraldı) | Gerçek sunucu, T601/F6. |

## Notlar

- ⚠ `AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter`: ilk (reddedilmeyen) `/sistem/canlilik` isteği 503 döndü (canlılık probu veritabanını 2 sn içinde bulamadı, yerel Docker'da). Test ve probe bu PR'da değişmedi; reddedilen istekten önce kırılır, `WriteRejectionAsync` değişikliğine ulaşmaz. İki ayrı koşuda tekrarlandı; ana dalda karşılaştırılmadı. CI'da doğrulanır.
- ⚠ (düzeltme 2 (B) ile kapandı) D-1134: "Diğer" betiği gerçek tarayıcıda gözlendi (D-1811 ✅).
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
| D-1412 | ~ daraldı → ✓ düzeltme 1 | betik ve işaretleme kancaları `AfterSalesScreenTests …_KK6`; çok kalemli sayfa testi ve tarayıcı gözlemi düzeltme 1'de (`…_D1412_D1833`, `DefectClaimFieldTests`) |
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
| UH3 | D-1412 | ✅ kapandı (düzeltme 1; D-1833 ile) | T604 | `DEFERRED_BACKLOG.md` |
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
| UH5 | D-1812 | ✅ kapandı (düzeltme 1) | T604 | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1843 | ✅ çözüldü (düzeltme 1 (S) ve (P); 23 sayfa, `WaitingPages` boş) | T604 | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1844 | ✅ çözüldü (düzeltme 1) | T604 | `DEFERRED_BACKLOG.d/T604-H.md` |
| UH5 | D-1770 | ✅ çözüldü (UH son) | T604 | `DEFERRED_BACKLOG.d/T604-A.md` |

Daraltılanların gerekçesi: D-1135 (UH1) kendi dilim notunda. **Düzeltme 1 ile düzeltildi:** UH3'ün D-1412 daraltması gerekçesizdi; UH5'in D-1812, D-1843, D-1844 için yazdığı "UH dilim listelerine (KK1) girmedi" gerekçesi olguya uymuyordu — D-1812 D-1550'nin (UH1), D-1843 ve D-1844 D-1773'ün (UH3 eki) payıdır — ve T601 dağıtım provası ürün kodu almaz. D-1412, D-1833, D-1812, D-1844, D-1859 düzeltme 1'de kapandı; D-1843'ün hedefi T604'e döndü; D-1770 yaması UH son işinde uygulandı (main bekçisi artık D-1120 istisnasını taşıyor).

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
- ⚠ (düzeltme 1 ile geri alındı) D-1812, D-1843, D-1844 `T601 (F6)`'ya daraltılmıştı; gerekçe yanlıştı (bkz. "Düzeltme 1"); D-1770 UH son işinde çözüldü.
- ⚠ `TitleMeasure.KnownWithoutBrand` (D-1815) bu dilimin kalem listesinde yoktu; birleşmeden sonra E2E'yi kıracağı için kaldırıldı.

## UH son — main birleştirmesi ve D-1770

- #594 squash ile main'e girdi (`0ee35474`). İki denetim: `git diff --quiet cea4b177 origin/main` çıkış 0; `git merge-base --is-ancestor cea4b177 HEAD` çıkış 0. İkisi tuttuğu için `git merge -s ours origin/main` yapıldı; `git diff origin/main...HEAD --stat` yalnız UH'nin değişikliklerini gösteriyor (A'nın test aracı dosyaları yok).
- D-1770 yaması `git apply --3way` ile uygulandı; `test-pre-commit.sh` tek çakışması (iki yanın yeni örnekleri) ikisi de korunarak çözüldü. D-1770 satırı yerinde `✅ Çözüldü → T604`; UH5'in "T601'e ertelendi" yazdığı yerler düzeltildi.

### Öz denetim (UH son)

- ✓ D-1770 kabul ölçüsü (yama uygulanır, bekçi geçirir, test yeşil): `test-pre-commit.sh` 263 geçti / 0 kaldı; `run-script-tests.sh` 4 dosya geçti.
- ✓ Birleştirme denetimi 1 (`git diff --quiet cea4b177 origin/main`): çıkış 0.
- ✓ Birleştirme denetimi 2 (`merge-base --is-ancestor cea4b177 HEAD`): çıkış 0.


## Düzeltme 2 — uç ve veri (2026-11-21; tur 2 FAIL sonrası eskalasyon turu, uç/veri yarısı)

İki işçi paralel, ortak dalda: bu yarı üç iş yapar — VAL-B001'in uç yanıtı ve veri kaydı kanıtı, K2 / D-1867 ve D-1819. Tarayıcı gözlemi satırları (VAL-A010, A012, A016, A018, A020; D-1820, D-1276, D-1811, D-1821) öteki yarıdadır ve bu bölüme girmez.

### Yapılanlar

1. **VAL-B001 (kanıt türü "Uç yanıtı + Veri kaydı"):** `CheckoutPageTests.APhysicalADigitalAndAServiceProduct_AreListedInOneCatalog_EnterOneCart_AndAreConfirmedInOneOrder_T604_VALB001` (yeni dosya `CheckoutPageTests.B001.T604.cs`). Yayında birer fiziksel, dijital ve hizmet ürünü: (a) `/arama` aynı sonuç sayfasında üçünün ürün sayfası bağlantısını listeler; (b) üç `AddCartItem` POST'u ilk eklemenin açtığı tek sepete gider (`carts` 1 satır, `cart_items` 3 satır); (c) onay adımı tek siparişi kurar (`orders` 1 satır) ve `order_items` üç satırı kendi varyantı ve ürün türüyle (Physical, Digital, Service) taşır. `12` VAL-B001 satırı PASS.
2. **K2 / D-1867:** sözleşme (`IReadsOnForgeryRefusal`: "sorgu değeri okunmaz") üst belge sayıldı, kod hizalandı. `Panel/Reset` kancası (`ShowSentAsync(…, readsQuery: false)`) ve vitrin `PasswordReset` sarmalayıcısı (`readsQuery: !ForgeryRefused`) 400 yolunda `LoadStepAsync`'i sorgu okumadan çağırır: kullanılmış bağlantı bu yolda "geçersiz" adımıyla çizilir; 422 ve GET yolları değişmedi. Testler: `PanelAccessPagesTests.TheForgeryRefusal_OfTheReset_ReadsNoQueryValue_…_D1867`, `CustomerPasswordResetAndReauthenticationPageTests.TheResetPagesForgeryRefusal_ReadsNoQueryValue_…_D1867` (kullanılmış bağlantı + adreste tamamlandı kodu: GET "tamamlandı", reddedilen POST "geçersiz"; bağlantının `UsedAt`'i ve gönderim kayıtları değişmez). Kırmızı–yeşil: üç değişinim (iki kancada `readsQuery: true`, görselde `alt=""`) üç testi de kırdı; geri alınca yeşil.
3. **D-1819:** karar K-1641 — sepet satırı görseli süsleyici sayılmaz, alt metni boş bırakılmaz (`02` 3.34.5 (2), 3.11.4; `12` VAL-A344). Metin firmanın o görsel için yazdığı, yoksa üretilen: varyantın kendi görselinde "Ad — Kırmızı", ürünün ana görselinde "Ad — görsel 1 / 3". `CartVariantReader` galeri bağlantılarını tek okumayla alır (satır sayısına bağlı değil), `CartVariantFact.ImageAltText` → `CartLine.ImageAltText` → `Cart.cshtml` (iki yerde). Test: `CartPageTests.TheLinesImage_HasTheAlternativeTextOf3114_…_D1819` (üretilen ana görsel metni, firma metni, varyantın kendi görseli).
4. Kayıt: `DEFERRED_BACKLOG.d/T604-H.md` D-1867 ve D-1819 satırları `✅ Çözüldü → T604`; `PRODUCT_DISCOVERY_STATUS.md` K-1641; `12` VAL-B001 PASS; `STATUS_CHANGELOG.d/T604-H.md` girişi. STATUS tablo satırına dokunulmadı.

### Hedefi T604 olan backlog kalemleri (bu yarının payı)

| Kalem | Durum | Kanıt |
|---|---|---|
| D-1867 | ✅ kapandı | iki `…_D1867` testi; sözleşme belgesi değişmedi, kod hizalandı |
| D-1819 | ✅ kapandı (hedefi T601'den T604'e alındı) | `…_D1819` testi |
| D-1820 | ✅ kapandı (B001 payı bu yarıda, öteki beş satır tarayıcı yarısında) | `…_T604_VALB001`; D-1820 ve D-1276 satırları birleştirmede kapatıldı |
| D-1811, D-1821 | ✅ öteki işçide (tarayıcı gözlemi) | — |
| Önceki kalemlerin tamamı (UH1–UH5, düzeltme 1) | ✅ (tur 2 envanteri: açık T604 hedefli satır yok) | `dogrulama-raporu-H-r2.md` KK1 |

### Para ve güvenlik notu

Bu yarıda para kodu yok. Güvenlik yönü: sahtecilik reddi yolu önceden de yazmıyor ve tüketmiyordu; D-1867 yalnız okuduğu girdiyi daraltır (sorgu değeri artık okunmaz). Geri alma: `readsQuery` bağımsız değişkenini kaldırmak. D-1819 salt okur; yeni yazma yok.

### Öz denetim — düzeltme 2 (kendi diff'imde; önceki işçilerin işi dahil `11` T604 kabul kriterleri ve `12` ilgili satırlar)

| # | Madde | Sonuç | Kanıt |
|---|---|---|---|
| H2-1 | VAL-B001: üç tür aynı katalogda, tek sepette, tek siparişte; uç yanıtı + veri kaydı | ✓ | `…_T604_VALB001` (listeleme gövdesi, `carts`/`cart_items`/`orders`/`order_items` sayıları ve türleri) |
| H2-2 | `12` VAL-B001 satırı PASS; öteki `12` satırları dokunulmadı | ✓ | `Docs/12_VALIDATION_PROTOCOL.md` tek hücre; `12_matris_denetle.py` sonucu aşağıda |
| H2-3 | K2: sözleşme ve kod uyumlu; yazma/tüketme yok testi | ✓ | `…ReadsNoQueryValue…_D1867` ×2; değişinimle kırmızı |
| H2-4 | Aynı sorgu okuma başka sahtecilik reddi yolunda var mı (kaynak taraması) | ✓ | `grep NoticeParameter.Read` sahtecilik kancalı sayfalarda: yalnız GET işleyicileri ve `LoadStepAsync`'in kapılanmış kolu; `ForgeryRefusalReadInventoryTests` yeşil |
| H2-5 | D-1819: `04`/`08` erişilebilirlik kuralına göre karar, uygulama, test, satır ✅ | ✓ | K-1641; `…_D1819`; backlog satırı |
| H2-6 | Kural değişince aynı kuralı anan öteki yerler | ✓ | sepet görseli iki yerde (`Cart.cshtml` bağlantılı ve düz metin kolu); sipariş sayfası görseli de (`_OrderPage.cshtml`, `alt=""` idi) aynı turda düzeltildi (⚠ 1); başka `alt=""` yok (`grep 'alt=""' src` boş) |
| H2-7 | Belge kuralını değiştiren metin karar kaydısız mı | ✓ | belge kuralı değişmedi; K-1641 kaydı yine de yazıldı |
| H2-8 | Var olan testler değişmeden geçer | ✓ | Integration 265/265 ve OrderPageTests 14/14 (sınıf listesi aşağıda); Unit 8325/8325 |
| H2-9 | Yeni modül kurucusu bağımlılığı | ✓ yok | `CartVariantReader` kurucusu değişmedi |
| H2-10 | Biçim, derleme | ✓ | `dotnet build -warnaserror` 0/0; `dotnet format … --include <değişen .cs>` (yalnız using sırası düzeldi, B001 dosyası) |

### Test sonuçları (düzeltme 2)

| Tür | Sonuç | Not |
|---|---|---|
| Build | ✓ | `-warnaserror` 0 uyarı 0 hata |
| Format | ✓ | değişen `.cs` dosyalarında |
| Unit TAM | ✓ 8325/8325 | |
| Integration (ilgili sınıflar) | ✓ 265/265 + OrderPageTests 14/14 | CartQuantityTests, CartTests, CartPageTests, CheckoutPageTests, CustomerPasswordResetAndReauthenticationPageTests, PanelAccessPagesTests, ForgeryRefusalReadInventoryTests, FormSecurity*, PageFormPatternTests, ForgeryProtectionTests, StorefrontSearchAndProductTests; TAM: CI |

### ⚠ (kolay kararlar, sahip yerine)

1. D-1819'un kendi kapsamı sepet satırı görselidir; aynı kuralın (`02` 3.34.5 (2)) yan örneği olan sipariş sayfasındaki satır görseli (`order-items__image`, `_OrderPage.cshtml`) de `alt=""` idi: dondurulmuş kalem adı (seçenek değerleriyle) alt metin oldu, test `OrderPageTests.TheItemsImage_HasTheFrozenNameAsItsAlternativeText_D1819`. Yeni kayıt gerekmedi.
2. D-1819 çözümünde ürün adı bağlantı içinde görsel alt metni ve görünür metin olarak iki kez okunabilir; `02` 3.34.5 (2) "her görselin alternatif metni" der ve süsleyici istisnası yazmaz, kural önceliklendi.

## Düzeltme 2 — tarayıcı (2026-11-21; tur 2 Bulgu 1'in tarayıcı payı)

Yönetici bölüşümü: iki işçi paralel; B001 (uç yanıtı + veri kaydı), D-1867 ve D-1819 öteki işçide. Bu işçinin payı: `12` VAL-A010, A012, A016, A018, A020 satırlarının kanıtı ve D-1811, D-1821'in tarayıcı gözlemi.

### Yapılan işler

- `tests/Shopfolio.IntegrationTests/Browser/ProductPageObservationTests.cs` (yeni): yedi gözlem testi, her biri `Ways()` ile üç motor × iki yol (42 koşu). Beklenen metinler satırın "Beklenen" sütunundan ve `DisplayFormat`'tan türetilir; her ürün gözlemin kendi tohumudur (`BrowserSeed.NewShowcaseProductAsync`, `ShowcaseSpec`), başka testin değişikliği ona ulaşmaz.
  - VAL-A010: üç seçenekli ürün kategori listesinde, aramada ve ana sayfa vitrininde bir kartla görünür.
  - VAL-A012: iki görselli ve açıklamalı ürünün sayfasında ana görsel ve öteki görsel yüklenmiş (doğal genişlik > 0) ve açıklama metni yazar.
  - VAL-A016: Türkiye üretimli fiziksel üründe fiyat bloğunda logo (görünür, yüklenmiş, alt metni "Yerli üretim logosu"); Almanya üretimlide logo yok, "Üretim yeri: Almanya"; yönetici panelinin ürün formunda logo ya da "yerli" adlı denetim yok, üretim yeri alanı var.
  - VAL-A018: yüzde 10'luk sürmekte olan indirimde sayfada ve vitrin kartında (aramada) referans fiyat, işaret ve başlangıç–bitiş tarihleri görünür; yanıt gövdesi de taşır (uç yanıtı).
  - VAL-A020: ifa süresi üç gün olan hizmetin sayfasında "Hizmet, ödeme onayından itibaren 3 gün içinde yerine getirilir"; sepete eklenip ödeme adımına gidilince onay özetinde (Ön Bilgilendirme Formu'nun başı, `OrderDocuments.Produce`) "İfa süresi: 3 gün (ödemenin onaylandığı andan itibaren)"; veri kaydı: `products.fulfilment_period` satırı gözlem kaydında.
  - D-1821: üç seçenekli üründe seçim değişince "Fiyatın geçerlilik başlangıcı" satırı seçilen seçeneğin tarihini yazar, tarihi olmayan seçenekte gizlenir (betikli: adres değişmeden; betiksiz: "Seçimi uygula" ile sunucu yanıtı).
  - D-1811: orta sınıfta (800) sığmayan girişler sırasıyla "Diğer" listesine taşınır, satır tek satır, giriş kaybolmaz; tıkla açılır, Escape kapatır ve odak düğmeye döner, Enter açar; betiksiz yolda "Diğer" gizli, hiçbir şey taşınmaz.
- `tests/Shopfolio.IntegrationTests/Browser/BrowserSeed.cs`: `ShowcaseSpec`, `TurkeyToday`, `NewShowcaseProductAsync`.
- **Ürün düzeltmesi (gözlemin bulduğu sapma):** `src/Shopfolio.Web/wwwroot/js/common.js` `bindMenuOverflow`: Safari düğmeye tıklamada odak vermez, bu yüzden fareyle açılan "Diğer" listesi Escape ile kapanmıyordu (Escape olayı öğede dinlenir, odak dışarıdaydı); WebKit'te D-1811 gözlemi kırmızı çıktı. Liste açılırken düğmeye `focus()` verilir. Davranış betiğin kendi başlık yorumundaki sözleşmedir (Escape kapatır, odak düğmeye döner); belge değişmedi.
- `Docs/12_VALIDATION_PROTOCOL.md`: VAL-A010, A012, A016, A018, A020 `Durum` PASS. `Docs/DEFERRED_BACKLOG.d/T604-H.md`: D-1811 ve D-1821 `✅ Çözüldü → T604` (hedef T604); D-1820 hedefi T604, tarayıcı payı kapandı, B001 öteki işçide. `Docs/DEFERRED_BACKLOG.md`: D-1276 satırı sonuçla, D-1134 satırındaki "D-1811'de gözlenecek" ifadesi güncel.

### Kanıt ve kırmızı–yeşil

- Yerel koşu, üç motor × iki yol: `ProductPageObservationTests` 42/42.
- Kırmızı–yeşil: `product.js` `setText("since", …)` satırı yorumlanınca `…_D1821` (betikli) kırmızı; `bindMenuOverflow` taşıma döngüsü bozulunca `…_D1811` (betikli) kırmızı; geri alındı. `common.js` odak düzeltmesi WebKit'te kırmızıyı yeşile çevirdi (düzeltmeden önceki koşuda "Escape'e basılır" satırı kırmızı).
- Gözlem sınırı (yazılı): tohum menüsünün altı girişi 800 genişlikte tek satıra sığar; D-1811 gözleminde sayfanın menüsü sekiz ek girişle uzatılır ve betiği gerçek `resize` olayı uyandırır (gözlem satırının "gözlenen" metninde yazılı). Betik gerçek düzen üzerinde çalışır; sunucu menüsünün uzunluğu değil, betiğin taşıma davranışı gözlenir.
- VAL-A020 "Ön Bilgilendirme Formu" gözlemi ödeme adımının onay özetindedir (sipariş verilmeden; özet, siparişin form metninin başıdır, `OrderDocuments.Produce`); siparişe yazılan metin `OrderDocuments` Integration testlerinin işidir.

### Test sonuçları (düzeltme 2 (B))

| Tür | Sonuç | Komut / sınıf |
|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` 0 uyarı, 0 hata |
| Unit TAM | ✓ 8325/8325 | `dotnet test --project tests/Shopfolio.UnitTests` |
| Integration (ilgili sınıflar) | ✓ 89/89 | FrameLayoutTests, StorefrontFrameTests, StorefrontSearchAndProductTests, PageAddressTests — TAM: CI |
| Tarayıcı (yerel, üç motor, iki yol) | ✓ 42/42 | `ProductPageObservationTests` |
| `12_matris_denetle.py --kapsam 1-77` | ✓ Hata: 0 | |
| `dotnet format` (değişen `.cs`: BrowserSeed.cs, ProductPageObservationTests.cs) | ✓ değişiklik yok | |

### ⚠ (kolay kararlar, sahip yerine)

- D-1820 satırı bu işçide kapatılmadı: VAL-B001 öteki işçide; B001 PASS olunca kalem ✅ olur. Hedefi T601'den T604'e döndü.
- D-1819 (sepet görseli alt metni) hedefi bu işçinin payı dışında (öteki işçi).
- `common.js` odak değişikliği `bindMenuOverflow`'un tek davranış değişikliğidir (Safari'de fareyle açma); Chromium ve Firefox'ta düğme zaten odaklanır, davranış aynı.
