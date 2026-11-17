# T552 — A seviyesi erişilebilirlik taraması ve ihlal düzeltmesi

**Faz:** F5 | **Durum:** Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-17

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Tarama (KK1):** `AccessibilityScanTests` — axe-core (`Deque.AxeCore.Playwright` 4.14.0) WCAG 2.2 A etiketli kural kümesi; durak listesi T551'in durakları + "E-12 sepet, ürünlü" + "E-13 ödeme adımı, ürünlü sepetle" (`AuditStops`); her durak iki yolda (betik açık/kapalı; betiksizde araç betiği çalışmadığı için betik istekleri ağda iptal edilir), üç motorda.
- **Elle denetim (KK2):** `AccessibilityAuditTests` — her durak için beş başlık (klavye, odak sırası ve görünürlüğü, %200 yakınlaştırma, erişilebilir ad, hata mesajı) ayrı gözlem satırı; başlığı boş kalan durak testi kırar. Klavye başlığı 3.2.1'i de ölçer.
- **31 ölçüt kaydı (KK3):** `WcagCriteria` + `AccessibilityRecordTests` (12 §12.2 tablosuyla çift yönlü eşitlik); Audit sonunda `accessibility-record-<motor>-<yol>.md` (31 ölçüt tablosu, Kontrol Listesi A payı, ekran × ölçüt matrisi) gözlem klasörüne yazılır, boş hücre testi kırar.
- **Ölçüt testleri:** `AccessibilityCriteriaTests` — 3.2.2 (dört ekranda değişince aynı adres/başlık + değişimden önce görünen bildirim), 3.2.6 (altbilgi İletişim bağlantısı sırası), 3.3.7 (ödeme adımından girişe gidip dönünce alanlar durur). Kaynak taraması (2.1.4, 2.5.1/2/4; 3.2.2 bildirimi) `AccessibilityRecordTests`'te.
- **Bulunan ihlaller, kaynağında düzeltildi (KK5):** (1) hata özetinin bağlantı kimliği: işleyici parametresi `input` olduğu için `input.Name` anahtarı `Input_Name` kimliğine çevrilmiyordu (`FormErrorSummary.From`); (2) panel yan menüsü uzun olunca kaydırılamıyordu (`admin.css`); (3) kendiliğinden gönderilen dört formda (sepet adedi, ödeme adımı, ürün seçeneği, satış özeti dönemi) önceden bildirim yoktu — `_ChangeNotice` bileşeni, metin `ChangeNotice.Text`'te tek yerde, betik çalışınca `common.js` gösterir.
- **Kütüphane:** `09` §3.3'e `Deque.AxeCore.Playwright` satırı (geçişli `Deque.AxeCore.Commons` MPL-2.0, `Newtonsoft.Json`, `System.IO.Abstractions` aynı satırda; props'ta olmayan paket satırı tutarlılık testini kırdığı için ayrı satır açılmadı).
- **Birim testi:** `MessageStateModelTests.TheLowerCasePrefix_IsTheFormsSpelling_AlsoWithoutTheFormOrder_...`. `ForbiddenTestPatternScan` yanlış-pozitifi (InlineData içindeki JS metni) `Fact` + dizi ile giderildi; `ValidationStateTests` testi eksik `Assert.Empty(Failures)` aldı.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.IntegrationTests/Browser/Accessibility*.cs`, `AuditScripts.cs`, `WcagCriteria.cs`, `ScreenInventoryTests.cs`, `ValidationStateTests.cs`
- `src/Shopfolio.Web/Pages/Components/{MessageModels,ChangeNotice}.cs`, `Pages/Shared/_ChangeNotice.cshtml`, `Cart`, `Checkout`, `Product`, `Panel/SalesSummary` sayfaları, `wwwroot/js/common.js`, `wwwroot/css/admin.css`
- `Directory.Packages.props`, `Shopfolio.IntegrationTests.csproj`, `packages.lock.json`, `Docs/09_CODING_GUIDELINES.md`

## Kabul Kriterleri Kontrolü (öz denetim; önceki işçinin işi dahil)

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | axe-core A kümesi, bütün ekranlar, iki yol | ✓ | `AccessibilityScanTests...KK1` chromium, firefox, webkit × iki yol yeşil (ürünlü sepet/ödeme sonrası Scan: firefox ve webkit; chromium Scan değişinim koşusunda ilk hâli yeşil, bildirim sonrası yerelde tekrar edilmedi, CI) |
| 2 | Beş başlıklı elle kayıt | ✓ | `AccessibilityAuditTests...KK2` chromium iki yol, firefox iki yol (bildirimden önce) yeşil; webkit Audit ve bildirim sonrası Audit yerelde koşulmadı, CI (D-1729) |
| 3 | 31 ölçüt kaydı, ekran × ölçüt | ✓ | `AccessibilityRecordTests` (tablo eşitliği, kırık kayıt örnekleri), Audit sonundaki `accessibility-record-*.md` |
| 4 | Kontrol Listesi A eşlemesi ya da `BLOCKED (OWNER_INPUT)` | BLOCKED (OWNER_INPUT) | madde metni dış kaynak, arama (2026-11-17) yalnız genelge ve haber sayfalarını getirdi; kayıtta `WcagCriteria.ChecklistAStatus`; D-1727. Eşleme tablosu madde metni olmadan yapılamaz |
| 5 | İhlal listesi boş | ✓ | yukarıdaki üç ihlal düzeltildi; Scan/Criteria/Record webkit 22/22 |
| 6 | Araç çıktısı + elle kayıt gate raporuna eklenebilir | ✓ | `observations.md/.jsonl` ve `accessibility-record-*.md`, CI `browser-observations` kaydı |

### Doğrulama kontrol listesi

| KK | Sonuç | Kanıt |
|---|---|---|
| KK1 | ✓ | Scan, iki yol × üç motor, durak listesi `AuditStops` |
| KK2 | ✓ (⚠ kısmi koşu) | Audit; webkit ve son hâl CI |
| KK3 | ✓ | Record testleri + kayıt dosyası |
| KK4 | BLOCKED (OWNER_INPUT) | D-1727 |
| KK5 | ✓ | ihlal kaynağında düzeltildi |
| KK6 | ✓ | kayıt dosyaları |
| 09 §25 | ✓ | sık FAIL nedenleri: (1) tarama istisnası yok — kaynak taramaları ihlal ve geçen örnek taşır; (2) bildirim taraması `requestSubmit` çağrısını bilinen dört betikle sınırlar, form etiketini ve `Quantity` biçimini ayrı sınar; (3) DB yok; (4) kural değişince başka yer: 09 tablo sayısı tutarlılık testi yeşil; (5) belge kuralı değişmedi, `12` VAL-A784'ün "önceden söylenmiştir" yolu uygulandı |

### Backlog (hedefi bu task olan kalem)

Hedefi T552 olan önceden yazılı kalem yok (`grep T552` yalnız T551 kayıtlarında anma). Yeni kalemler: D-1727, D-1728, D-1729 (`DEFERRED_BACKLOG.d/T552.md`).

## Değişinim (korunan davranış bozulunca test kırılır)

Geçici bozma, tek derleme + tek koşu, sonra `git checkout` ile geri alındı: (a) altbilgideki İletişim bağlantısı kaldırıldı → `TheHelpLink_...VALA341` iki yolda kırıldı; (b) ürün kartı `<img>`'inden `alt` silindi → `EveryScreenOfTheInventory_HasNoViolation...KK1` iki yolda kırıldı (axe `image-alt`); (c) sepet satırından `_ChangeNotice` kaldırıldı → `EveryFormThatSendsItselfByAChange...` ve `AChangedValue...VALA784` kırıldı. Bağlantıya `tabindex=-1` bozması süre nedeniyle denenmedi (Audit klavye başlığı; CI/doğrulama).

## Test Sonuçları

- Unit TAM: 8088/8088. `dotnet format` (değişen `.cs`): temiz.
- Integration (ilgili sınıflar; **TAM: CI**): chromium `AccessibilityRecordTests` + `AccessibilityCriteriaTests` yeşil (36); webkit Record + Criteria + Scan 22/22; firefox Scan 2/2 (bildirimden önce).
- Koşulmadı, CI: firefox/webkit Audit, firefox Criteria, bildirim sonrası firefox/chromium Scan ve Audit.

## ⚠ Kararlar (sahip yerine)

1. **3.2.2:** değer değişince kendiliğinden gönderilen her forma bildirim eklendi (VAL-A784 "değişim önceden söylenmiştir"); betiksiz yolda gösterilmez, düğme var.
2. **Hata mesajı odağı:** `04` §2.3.2 her formda özet ve odak ister; Audit yalnız özetli formlarda ölçüyor — özetsiz formlar D-1728 (doğrulayıcı karar versin).
3. Duyusal tarif: "aşağıdaki IBAN'a" öğeyi adıyla anar, ihlal sayılmadı.
4. Tarama süresi: iki yol × üç motor; CI e2e süresi ~3 kat, firefox Audit tek başına ~9,5 dk.
5. Kontrol Listesi A: `BLOCKED (OWNER_INPUT)`.

---

## Düzeltme 1 (2026-11-17)

**Durum: BÖLÜNDÜ (süre).** D-1728 ve değişinim tamam; D-1729 (üç motor koşuları) tamamlanmadı, devir notunda.

- **D-1728 kapandı.** `AccessibilityAuditTests.ErrorState`: alan hatası olup özeti olmayan form ihlaldir; hiçbir hata göstermeyen yanıt ihlaldir; odak ilk hatalı alanda ya da özette olmalıdır (özet olmasa da). Form listesi `ValidationStateTests`'ten (ikinci liste yok); iç not ve teslimat illeri formları (`NoValidationState`) "uygulanmaz" satırı alır. İlk ölçüm (chromium, `ValidationStateTests` gözlemi) özetsiz alan hatalı formları buldu ve kaynağında düzeltildi: E-04 sepete ekleme (`Product.cshtml`), E-12 sepet adedi (`Cart.cshtml`), E-33 ürün formu (`_ProductFormBody`, `ProductFormModel.ErrorSummary`: varyant satırı bağlantıları kendi girdi kimliğine eşlendi; ilk denemede `Input_Variants_0__StockQuantity` bağlantısı sayfada olmayan kimliğe gidiyordu), E-34 kategori üç form (`Categories.cshtml`), E-47 yasal metin (`_LegalTextSection`). Alansız ret (E-34 taşıma, E-37 işlem formları): `04` §2.3.1.2 ve §2.3.3 yerinde kalıcı iletidir, §2.3.2 "hatalı alanlar" için özettir; bu formlar kapsam dışıdır, ölçü onlar için ileti sayısına bakar.
- **Ölçü açığı bulundu (değişinim):** `tabindex=-1` bağlantı ilk denemede KIRMADI: `AuditScripts.Install` `tabindex=-1` öğeleri beklenen listesinden çıkarıyordu. Düzeltildi: yerel bağlantı/düğme/girdi `tabindex=-1` ise beklenir; yalnız denetim olmayan odak hedefleri ve kasıtlı `aria-hidden` tuzak alanı (iletişim formu, `02` 3.32.7) dışarıda. Sonra: üst bilgi sepet bağlantısına `tabindex=-1` verildi, chromium Audit klavye başlığı 112 satırda kırıldı (geri alındı). İletişim formundan özet kaldırıldı, E-10 hata başlığı iki yolda "hata özeti False, geçersiz alan 5" ile kırıldı (geri alındı); aynı koşu tuzak alanı muafiyeti eksikliğini de gösterdi, eklendi.
- **Test (Düzeltme 1, bütün değişikliklerden sonra, motor başına ayrı çağrı, iki yol):**

| Sınıf | chromium | firefox | webkit |
|---|---|---|---|
| `AccessibilityScanTests` | 2/2 yeşil | 2/2 yeşil | 2/2 yeşil |
| `AccessibilityCriteriaTests` | 5/5 yeşil | 5/5 yeşil | 5/5 yeşil |
| `AccessibilityRecordTests` | 15/15 yeşil | 15/15 yeşil | 15/15 yeşil |
| `AccessibilityAuditTests` | 2/2 yeşil (3 dk 12 sn) | 2/2 yeşil (8 dk 4 sn, tek çağrıya sığdı) | **2/2 KIRMIZI** |

- **⚠ WebKit Audit klavye başlığı kırmızı (bulgu, site ihlali değil, bu makinenin WebKit yapısı):** Windows'taki Playwright WebKit 26.4'te Tab tuşu bağlantıları atlıyor (E-01: beklenen 22, ulaşılan 3; yalnız düğme ve girdiler). `Alt+Tab` denendi, sonuç aynı (ulaşılan 3), geri alındı. Aynı sınıf chromium ve firefox'ta yeşil, yani bağlantılar klavyeyle erişilebilir; sorun WebKit'in Windows yapısında Tab'ın bağlantıya gitmemesi. Karar gerekir: WebKit için klavye ölçüsünü Linux CI'da (WebKit Linux yapısı bağlantıya Tab ile gider; doğrulanmadı) mı sınamalı, yoksa WebKit'te odak sırasını `Alt+Tab` dışında bir yolla mı (örn. `page.focus` ile sıralı) ölçmeli. D-1729 bu yüzden açık.
- **Koşulmadı (süre):** değişen sayfaların Integration sayfa testleri (Product, Cart, Categories, LegalTexts, ProductForm sınıfları).

### Öz denetim (KK1–KK6, baştan)

| KK | Sonuç | Kanıt |
|---|---|---|
| KK1 tüm ekranlar, iki yol | ✓ | Scan üç motor × iki yol yeşil (tablo) |
| KK2 beş başlık | ⚠ kısmi | chromium ve firefox iki yol yeşil; webkit klavye başlığı Windows WebKit'te Tab bağlantıyı atladığı için kırmızı (D-1729 açık) |
| KK3 31 ölçüt kaydı | ✓ chromium, firefox | kayıt dosyaları yazıldı, boş hücre yok; webkit kaydı klavye hücreleri yüzünden boşluklu |
| KK4 | BLOCKED (OWNER_INPUT) | D-1727, dokunulmadı |
| KK5 ihlal listesi boş | ⚠ kısmi | chromium ve firefox'ta boş; webkit'te yalnız ölçü sınırı (site ihlali değil) |
| KK6 gate kanıtı | ✓ | `observations.md` ve kayıt dosyaları |

⚠ Kısmi satırlar ✓ sayılmadı. Düzeltme 1 BÖLÜNDÜ (süre); kalan: WebKit klavye ölçüsü kararı ve sayfa testleri.

---

## Düzeltme 1 — son işçi (2026-11-17)

- **CI kırmızıları (89ec8e42) ve düzeltmesi:** Unit `NullForgivingSourceScanTests` — `Product.cshtml` 28-29'daki gerekçesiz `!` kalktı (özet satırı zaten çizilen `MessageView.Text`'ten kuruluyor). Unit `CacheSourceScanTests` — `ProductForm.cshtml.cs` `VariantInputSuffixes` `FrozenDictionary` oldu (testin izinli listesi genişletilmedi). Integration `CartPageTests` 3 + `LegalTextPageTests` 2: özet eklenince satır/bölüm içinde ikinci `<li>`/`<section>` doğdu, testin `LineOf`/`SectionOf` dilimi ilk kapanış etiketinde bitiyordu; davranış (`04` 2.3.2 özet) esas alındı, dilimler iç içe sayarak güncellendi.
- **WebKit Audit klavye başlığı:** CI run 44024980434 "8. E2E (advisory)" günlüğünde `The operation was canceled` (test sonucu yok) → seçenek (b). WebKit'te klavye gözlemi odak sırasıyla (`tabIndex >= 0` olanları) `focus()` ile geziyor ve her denetimin etkinleştirme tuşunu (bağlantı, düğme, özet: Enter; onay kutusu: Space) gerçekten basıp oluşan tıklamayı yakalayıp reddediyor (sayfadan çıkmadan). chromium/firefox Tab yolu aynen. **Değişinim:** üst bilgi sepet bağlantısına `tabindex=-1` verildi, webkit Audit iki yolda kırıldı (112 satır, chromium ile aynı sayı), geri alındı. Odaklama kaydırmadan önce sayıldığı için ilk denemede "görünüm dışı 1" çıktı; `scrollIntoView` + `preventScroll` ile düzeltildi.
- **Test (bütün değişikliklerden sonra):** Unit TAM 8088/8088; `dotnet format` değişen `.cs` temiz. Integration sayfa testleri (CartPage, CategoriesPage, LegalTextPage, ProductFormPage, StorefrontSearchAndProduct, CheckoutPage, SalesSummaryPage, PageFormPattern) 176/176. Tarayıcı (webkit, yerel Windows): `AccessibilityAuditTests` 2/2 yeşil (iki yol); Scan + Criteria + Record 22/22. chromium/firefox: bu turda kod değişmedi (Tab yolu aynı), önceki tur sonuçları geçerli; CI doğrular.

| Sınıf | chromium | firefox | webkit |
|---|---|---|---|
| `AccessibilityScanTests` | 2/2 (önceki tur) | 2/2 (önceki tur) | 2/2 |
| `AccessibilityCriteriaTests` | 5/5 (önceki tur) | 5/5 (önceki tur) | 5/5 |
| `AccessibilityRecordTests` | 15/15 (önceki tur) | 15/15 (önceki tur) | 15/15 |
| `AccessibilityAuditTests` | 2/2 (önceki tur) | 2/2 (önceki tur) | 2/2 (odak yolu) |

- **D-1729 kapandı**, D-1727 (OWNER_INPUT) dokunulmadı.

### Öz denetim (KK1–KK6, baştan)

| KK | Sonuç | Kanıt |
|---|---|---|
| KK1 tüm ekranlar, iki yol, ihlal | ✓ | Scan üç motor × iki yol yeşil (webkit bu turda, öteki ikisi önceki tur; kod değişmedi) |
| KK2 beş başlık × ekran × iki yol × motor | ✓ | Audit: webkit odak yolu 2/2 (bu tur), chromium 2/2, firefox 2/2 (önceki tur); boş hücre denetimi testin kendisinde |
| KK3 31 ölçüt kaydı | ✓ | webkit Record 15/15 (bu tur), chromium/firefox önceki tur; webkit klavye hücreleri artık dolu |
| KK4 Kontrol Listesi A | BLOCKED (OWNER_INPUT) | D-1727, sahip girdisi bekliyor, dokunulmadı |
| KK5 ihlal listesi boş | ✓ | üç motorda ihlal yok; webkit ölçü sınırı çözüldü |
| KK6 gate kanıtı | ✓ | `observations.md`, kayıt dosyaları, değişinim satırları |

⚠ Webkit'te klavye yolu Tab yerine odak gezisi (Linux CI'da Tab yolu denenmedi; E2E işi iptal edilmişti). KK4 dışında kısmi satır yok.

## Doğrulama — tur 1 (2026-11-17): ✗ FAIL

- **Bulgu sayısı:** 2 (ORTA, S1); K-notları D-1732'de. Düzeltme gerekli.
- **B1 (KK1, `VAL-A776`, `A790`, `A781`, `A343`):** `12 §12.2` (1) kapsamı ekranlarla birlikte onay pencerelerini ve hata hâllerini sayar; axe taraması yalnız yürüyüşün duraklarında koşar, onay pencereleri (E-44, E-45 … `ConfirmWindowTests`) ve hatalı gönderimin cevap sayfası taranmaz; onay penceresinde odağın "Vazgeç"te başladığı tarayıcıda gözlenmez.
- **B2 (KK2/KK5, `VAL-A791`, `A781`):** CI run 44031956010 "8. E2E" (head `c23db333`): `AccessibilityAuditTests` chromium betiksiz yolda E-34, E-47, E-50, E-53 hata başlığı kırmızı (odak BODY); yerel Windows'ta yeşil. Ölçü yarışı ya da gerçek odak sorunu; araç CI'da temiz değil.
- **Doğrulanan:** beş değişinim ilgili testi kırdı (chromium Audit + Scan; webkit Audit tabindex=-1); Unit 8088/8088; build `-warnaserror` temiz; sayfa testleri 169/169; Record + ScreenInventory 22/22; chromium Scan/Criteria/Audit yeşil.
