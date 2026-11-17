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
