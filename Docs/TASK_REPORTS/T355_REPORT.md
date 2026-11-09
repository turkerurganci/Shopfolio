# T355 — Sipariş anında donan değerler

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; merge yardımcıda) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Katalog okuması (onay anı):** `ICatalogModule.ListOrderItemSourcesAsync` + `OrderItemSourceReader` → `OrderItemSource` kaydı. Canlı üründen bir kez okur: ad, seçenek değerleri, görsel (`ProductGallery.GetVariantImageAsync`: varyantınki, yoksa ürünün ana görseli), saklanan fiyat, ürünün KDV oranı; yürüyen indirim varsa `DiscountLine` (indirimli fiyat, referans fiyat, oran; `PriceLabelReader` ile aynı referans kuralı); istisna yalnız fizikselde, ifa süresi yalnız hizmette, dijitalde o anki dosya. Dosya kuralının tek evi: devam turunda `DigitalFileReader.CurrentFileAsync` ayrıldı (varyantın bağlı dosyası, yoksa ürününki); `FindAsync` ve donma okuması onu çağırır, yüklemesi bitmemiş dosya ikisinde de dosya sayılmaz. "Şimdi" `TimeProvider`'dan; yazmaz, kilit almaz.
- **Donma birimi:** `Ordering/OrderFreeze.cs` (saf, veritabanı ve saat yok).
  - `FreezeItem`: okumanın her değerini kaleme düz kopyalar (birim fiyat, KDV oranı ve tutarı, indirim oranı, indirimli fiyat, referans fiyat, kupon payı, ad, seçenekler, görsel, tip, istisna, ifa süresi, dijitalde P-8 indirme hakkı ve dosya). Tutar başka fiyatınsa programlama hatası.
  - `FreezeLegalIdentity`: firmanın o günkü kimliği tipe göre (esnafta vergi no; iki tacir tipinde MERSİS ve ticaret sicil no), adres ve iletişim; Ön Bilgilendirme Formu, Mesafeli Satış Sözleşmesi ve aydınlatma sürümleri; IBAN yalnız havale siparişinde.
  - `PartiesAtConfirmation`: düzeltilen adres ya da e-posta geçmişin ilk satırının eski değerinden, düzeltilmeyen siparişin kendisinden okunur (K-1027).
- **Belgelerin onay anı tarafları:** `IOrderingModule.GetConfirmationPartiesAsync` → `ConfirmationParties` (ad, adresler, e-posta).
- **Testler:** Unit `OrderFreezeTests` (13), Integration `FrozenValuesTests` (KK1–KK6). Devam turunda KK2 testine düğme denetimi eklendi: ürünün istisnası kaldırıldıktan sonra ilk siparişin cayma düğmesi donmuş istisnayla kapalı, ikinci siparişinki açık; indirme düğmesi donmuş hakkı sayar (`IOrderOperations.ReadAsync`/`CheckAsync`).
- **Kayıtlar:** D-476 kapandı; D-437 T348'e ileri atıldı; D-508 hedefinden T355 çıktı; yeni D-705 (`DEFERRED_BACKLOG.d/T355.md`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `Contracts/OrderItemSource.cs` (yeni), `OrderItemSourceReader.cs` (yeni), `DigitalFileReader.cs` (`CurrentFileAsync` ayrıldı), `Contracts/ICatalogModule.cs`, `CatalogModule.cs` (kurucu + kayıt)
- `src/Shopfolio.Web/Modules/Ordering/`: `OrderFreeze.cs` (yeni), `Contracts/ConfirmationParties.cs` (yeni), `Contracts/IOrderingModule.cs`, `OrderingModule.cs`
- `tests/Shopfolio.UnitTests/`: `Ordering/OrderFreezeTests.cs` (yeni), `Infrastructure/SitemapSourceTests.cs` (sahte katalog yöntemi)
- `tests/Shopfolio.IntegrationTests/Ordering/FrozenValuesTests.cs` (yeni)
- `Docs/`: `DEFERRED_BACKLOG.md` (D-476, D-437), `DEFERRED_BACKLOG.d/T340.md` (D-508), `DEFERRED_BACKLOG.d/T355.md`, `STATUS_CHANGELOG.d/T355.md`, `IMPLEMENTATION_STATUS.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ödenen tutarı üreten değerler ve ürünün o anki tanımı donar | ✓ | `FrozenValuesTests.AnItem_KeepsItsPriceDiscountCouponShareVatAndDescription_WhenTheProductChangesLater_KK1`; `OrderFreezeTests` KK1 (3 test) |
| 2 | Tip, istisna, ifa süresi, indirme hakkı donar; düğmeler donmuş değeri okur; değişiklik yalnız yeni siparişe | ✓ | `FrozenValuesTests.TheDeliveryLineAndWithdrawalAttributes_Freeze_AndOnlyANewOrderTakesTheirChange_KK2` (düğme denetimi dahil); `OrderFreezeTests` KK2; `OrderOperations.cs` kalemin sütunlarını okur (satır 61–88) |
| 3 | Donan değerler kalemde düz; siparişte yalnız kupon kodu, kargo ücreti, toplamlar; ara katman yok | ✓ | `FrozenValuesTests.TheFrozenValues_LiveFlatInTheItem_AndTheOrderCarriesOnlyTheCartLevelValues_KK3` |
| 4 | Adres, e-posta, firma kimliği, sürümler, kutular, IBAN donar; düzeltilen adres/e-posta belgelerde onay anındaki değerle | ✓ | `FrozenValuesTests...KK4` ve KK2 testinin kutu bölümü; `OrderFreezeTests` KK4 (5 test) |
| 5 | Donma anı siparişin oluştuğu an; kargoya verme sözü o anda donar | ✓ | `FrozenValuesTests.TheMomentOfFreezing_IsTheOrders_AndADiscountEndingLater_DoesNotReachTheItem_KK5`; `OrderFreezeTests.TheShippingPromise_IsWrittenOnceAtConfirmation_KK5` |
| 6 | Marka ayarları siparişe donmaz | ✓ | `FrozenValuesTests.TheOrderAndItsItems_HaveNoBrandColumn_SoAPastOrderShowsTheCurrentBrand_KK6` |

### Öz denetim (bitiş kapısı 2)

> Devam turu bütün maddeleri (önceki işçinin yaptıkları dahil) yeniden denetledi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1 | ✓ | `OrderFreeze.FreezeItem`; `OrderItemSourceReader.ListAsync`; KK1 testleri |
| Kriter 2 | ✓ | `OrderFreeze.FreezeItem` (tip bağlı alanlar); KK2 testi (değerler + cayma ve indirme düğmeleri) |
| Kriter 3 | ✓ | KK3 testi (model üzerinden: düz alanlar, tek FK, ara varlık yok) |
| Kriter 4 | ✓ | `OrderFreeze.FreezeLegalIdentity`, `PartiesAtConfirmation`; `OrderingModule.GetConfirmationPartiesAsync`; KK4 testleri |
| Kriter 5 | ✓ | KK5 testleri (`CreatedAt` = onay anı, indirim bitince kalem değişmez, `ShippingPeriod` sabit) |
| Kriter 6 | ✓ | KK6 testi |
| KK1: ürün sonradan değişince kalem eski değerleri korur | ✓ | `FrozenValuesTests...KK1` |
| KK2: tip, istisna, ifa süresi, hak donar; kalem ve düğmeler eskiyle çalışır | ✓ | `FrozenValuesTests...KK2` |
| KK3: kalem alanları düz, ara katman tablosu yok | ✓ | `FrozenValuesTests...KK3` |
| KK4: adres, kimlik, sürüm, kutu, IBAN donar; düzeltmede belgeler onay anını gösterir | ✓ | `FrozenValuesTests...KK4`, KK2 kutu bölümü, `OrderFreezeTests` KK4 |
| KK5: donma anı doğru, söz onayda sabit | ✓ | `FrozenValuesTests...KK5`, `OrderFreezeTests...KK5` |
| KK6: marka değişikliği eski siparişte görünür | ✓ | `FrozenValuesTests...KK6` |
| `09 §25` — modül sınırı | ✓ | Ordering yalnız `Catalog.Contracts`'a başvurur; arayüz yöntemleri `SaveChanges` çağırmaz, işlem açmaz; Unit `ModuleBoundaryTests` yeşil (5126/5126) |
| `09 §25` — zaman yalnız `TimeProvider` | ✓ | `OrderItemSourceReader` "şimdi"yi `TimeProvider`'dan okur; `OrderFreeze` saat okumaz (`now` parametre) |
| `09 §25` — tek işlev | ✓ | Referans fiyat `PriceHistoryReads.ReferencesAsync` + `DiscountRules.LineOf` (etiketle aynı); tutar `ItemPricing.Price` (çağıran); görsel `ProductGallery.GetVariantImageAsync` çağrılır; o anki dosya `DigitalFileReader.CurrentFileAsync` çağrılır (`FindAsync` de onu kullanır — ikinci kopya yok) |
| `09 §25` — para hesabı | ✓ | Tutar üretilmez; `FreezeItem` yalnız gelen `ItemAmounts`'ın uygulanan fiyatla tutarlılığını denetler (bkz. ⚠ c) |
| `09 §25` — yeni ayar/sabit, göç, sayfa, iş, paket | uygulanamaz | Diff göç, ayar, sayfa, arka plan işi ya da paket eklemiyor |
| `09 §25` — modül kurucusuna yeni bağımlılık | ✓ | `CatalogModule` kurucusu DI'dan kurulur (elle kuran test yok: `grep "new CatalogModule("` boş); Integration tam koşu (aşağıda) |
| Backlog: hedefi T355 olan kalemler | ✓ | D-476 kapandı, D-437 → T348, D-508 hedefinden çıktı; açık hedefi T355 olan kalem kalmadı |
| Güvenlik taraması | ✓ | Sır, kimlik doğrulama, giriş doğrulama etkisi yok; yeni uç yok; okuma parametreli EF sorgusu; kişisel veri kayda yazılmıyor |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build tests/Shopfolio.IntegrationTests -c Release -warnaserror` ve `tests/Shopfolio.UnitTests` | 0 uyarı, 0 hata (main birleştirmesinden sonra) |
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release` | 5126/5126 |
| Integration | ✓ | `--filter-namespace ...Ordering` · `...Catalog` · geri kalan (`--filter-not-namespace` iki ad alanı) | 353/353 · 72/72 · 2543/2545; düşen 2 test `LivenessEndpointTests` (tek başına 14/14, D-391 yük altında kararsız, T355'e dokunmuyor). Tek işlev düzeltmesinden (`CurrentFileAsync`, görsel) sonra dokunulan alan yeniden: `Files` + `Catalog` 174/174, `FrozenValuesTests` 6/6 |
| Format | ✓ | `dotnet format --verify-no-changes --include <değişen dosyalar>` | çıktısız |
| Contract | uygulanamaz | — | Uç değişmedi |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-09, head `e958219b`; kayıt commit'inden önce `origin/main` birleştirildi: zaten güncel, çakışma yok). Kriter 1–6, KK1–KK6, `09 §25` ve güvenlik taraması ✓ |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

**Doğrulayıcı kanıtları (tur 1):**

| # | Kriter / KK | Sonuç | Doğrulayıcı kanıtı |
|---|---|---|---|
| 1 / KK1 | Ödenen tutarı üreten değerler ve ürünün o anki tanımı donar | ✓ | `OrderFreeze.FreezeItem` `OrderItem`'ın bütün donan sütunlarını yazar (birim fiyat, KDV oranı, `amounts.Vat`, `CouponShare`, indirim oranı, indirimli fiyat, referans fiyat, ad, iki seçenek, görsel); okuma `OrderItemSourceReader` referansı `PriceLabelReader` ile aynı yoldan alır (`PriceHistoryReads.ReferencesAsync` + `DiscountRules.LineOf`); Integration `AnItem_KeepsItsPriceDiscountCouponShareVatAndDescription_WhenTheProductChangesLater_KK1`: ürün adı, KDV, fiyat, görsel, seçenek değeri ve indirim değişince canlı okuma yeniyi, kalem eskiyi verir |
| 2 / KK2 | Tip, istisna, ifa süresi, indirme hakkı donar; düğmeler donmuş değeri okur | ✓ | `FreezeItem` tipe bağlı alanları yalnız kendi tipinde yazar; Ordering ve AfterSales'te ürün özniteliği okuyan yer yok (`grep "_db.(Products\|Variants\|DigitalFiles)\|_catalog."` yalnız sepet ve stok sayaçları); `OrderOperations`, `OrderPeriods` (Z-39), `DownloadCounters`, `OrderTransitions.ItemRecords` kalemin sütunlarını okur; Integration KK2: istisna kaldırılıp ifa süresi ve P-8 değiştikten sonra ilk sipariş eskiyi, ikinci yeniyi taşır; ilk siparişin cayma düğmesi kapalı (`ReadAsync` + `CheckAsync` → `Closed`), ikincininki açık; dosya istisnası `DigitalFileReader.CurrentFileAsync` (tek ev, `FindAsync` de kullanır) |
| 3 / KK3 | Kalem alanları düz; siparişte yalnız kupon kodu, kargo ücreti, toplamlar; ara katman yok | ✓ | Integration KK3 EF modelinden: 15 donan alan `OrderItem`'da, kalemin siparişe tek FK'si `OrderId`, sipariş ile kalem arasında varlık yok; `Order`'da ürün düzlemli alan yok; `06 §4.17` sipariş sütunları ile birebir |
| 4 / KK4 | Adres, e-posta, kimlik, sürümler, kutular, IBAN donar; düzeltmede belgeler onay anını gösterir | ✓ | `FreezeLegalIdentity` `06 §4.17`'nin 11 donmuş firma sütununu (tip, unvan, VKN / MERSİS + sicil, il, ilçe, adres, telefon, e-posta, KEP) ve üç sürümü yazar, IBAN yalnız havalede; `PartiesAtConfirmation` geçmişin ilk satırının (`CreatedAt`, `Id`) eski değerini alır; Integration KK4: iki adres düzeltmesi + e-posta düzeltmesinden sonra taraflar onay anındaki değerler; Unit KK4 (Theory üç tip, IBAN, eksik alan, imha edilmiş sipariş); kutu kaydı KK2 testinde metin ve kalemle okunur; adres donması `Order.FreezeAddresses` (T344) |
| 5 / KK5 | Donma anı doğru; söz onayda sabit | ✓ | `FreezeItem(now)` → kalemin `CreatedAt`'i siparişinki; Integration KK5: indirim bittikten sonra canlı okuma indirimsiz, kalem 800 ve sipariş toplamı 800 kalır, `ShippingPeriod` 2; Unit `TheShippingPromise_IsWrittenOnceAtConfirmation_KK5` (ikinci yazım hata) |
| 6 / KK6 | Marka ayarları siparişe donmaz | ✓ | Integration KK6: `orders` ve `order_items` sütunlarında brand/logo/color/favicon yok |
| `09 §25` | ilgili satırlar | ✓ | Ordering yalnız `Catalog.Contracts`, `Settings.Contracts`, `Payment.Contracts`'a başvurur (Unit `Architecture` yeşil); arayüz yöntemleri `SaveChanges` çağırmaz; zaman `TimeProvider`'dan (`OrderFreeze` saatsiz); para üretilmez (`FreezeItem` gelen `ItemAmounts`'ı denetler); tek işlev: dosya kuralı `CurrentFileAsync`'e taşındı, görsel `ProductGallery`, referans `PriceHistoryReads`; göç, paket, ayar, uç, iş yok |
| Güvenlik | taraması | ✓ | Sır yok; yeni uç ya da POST yok (yetki etkisi yok); okumalar parametreli EF sorgusu; kişisel veri kayda yazılmıyor; `GetConfirmationPartiesAsync` çağıranı yok (T356); yeni bağımlılık yok |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana kopyanın bekçisi, worktree); ana dal son 3 tamamlanmış run 42532103711, 42532103776 (`d0c97cf0` T327), 42530025706 (`02d83f79` T360) — hepsi success.

**Testler (doğrulayıcı, yerel, `e958219b`):** `dotnet restore` temiz; `dotnet build Shopfolio.slnx --no-restore -warnaserror` 0 uyarı / 0 hata; `dotnet format --verify-no-changes` üç proje çıkış 0; Unit (Architecture + Consistency dahil) 5134/5134; betik testleri 4/4; Integration (PostgreSQL 18 konteyner): `FrozenValuesTests` 6/6, `DigitalDeliveryTests` 21/21 (`DigitalFileReader` yeniden düzenlemesi); dal CI run 42533334898 (head `e958219b`) dokuz işin hepsi yeşil (Integration tam koşu dahil).

**`12` satırları:** KP-17'nin T355'e düşen satırları `VAL-D010`, `D011`, `D012`, `D013`, `D014`, `D015`, `D016` sipariş sayfası / uç yanıtı ve verilmiş sipariş (T348 onayı) ister; T355 uç ve onay akışı eklemez (D-705 → T348, T356). Servis düzeyi kanıt yukarıdaki tablodadır; satırların `Durum`'u `BEKLEMEDE` kaldı (T354, T346, T318, T338 emsali).

**Yapım raporu karşılaştırması (adım 13):** tam uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Sayı farkı: Unit 5126 (yapım) ↔ 5134 (doğrulayıcı) — doğrulayıcı main birleştirmesinden sonraki uçta koştu; hüküm etkisi yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok — kalem ve sipariş sütunları T338/T344/T349'da vardı
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T355-donan-degerler`
- **Commit:** son commit PR'da
- **PR:** #484
- **CI:** dal run 42533334898 (head `e958219b`) success
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

Dış varsayım: yok (kütüphane, platform, plan ya da dış API sözleşmesi eklenmedi).

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Donma birimi siparişi oluşturan akışa (T348 onay işlemi) ve T356 form gösterimine bağlanmadı | D-705 | Sipariş onayı T348'in işi; T348 T355'e bağımlı |
| 2 | Adres çözümü ve `FreezeAddresses` çağrısı | D-437 | Aynı nedenle T348 |

## ⚠ Kolay kararlar (kurala en uygun seçenekle verildi)

- **a)** Firma kimliğinin tipe göre alanları (esnafta vergi no, tacirde MERSİS + sicil) `06 §4.17` "esnafta dolu / tacirde dolu" cümlesinden Ordering'de yazıldı; Settings'in `CompanyIdentityRules` tipi iç tip (modül sınırı), bu yüzden çağrılamadı.
- **b)** Onay kutusu kaydının oluşturulması T356'nın işi; T355 yalnız kaydın donmuş kaldığını KK2 testinin kutu bölümünde sınar (KK4).
- **c)** `FreezeItem` tutarı yeniden hesaplamaz; çağıranın `ItemPricing.Price` sonucunu uygulanan fiyat × adetle karşılaştırıp uyuşmazlıkta programlama hatası verir (koruma denetimi, para üretimi değil).
- **d)** KK2'nin cayma düğmesi denetimi koşulsuz istisna (`MadeToOrder`) ile yapıldı; hijyen ve mühürlü ortam koşullu istisna olduğu için düğmeyi kapatmaz (`WithdrawalExemptions.IsConditional`).
- **e)** Integration'da 2 `LivenessEndpointTests` vakası tam koşuda düştü, sınıf tek başına 14/14 geçti (D-391'in bilinen kararsızlığı).

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** önceki işçi 42524245424, 42524245383, 42522612870 — hepsi success; devam turunda main ilerledi, `02d83f79` CI 42530025674 ve Publish 42530025706 success; `origin/main` dala birleştirildi (çakışmasız)
- **hooksPath:** `scripts/git-hooks`
- **Repo memory (bitiş kapısı 9):** talimat gereği MEMORY'ye dokunulmadı (parça dosyaları düzeni)
- **Diğer:** İş iki işçide yapıldı (ilki bağlam sınırında bölündü); devam turu bütün maddeleri yeniden denetledi.
