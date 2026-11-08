# T363 — Sayfa adresleri, arama motoru bilgileri, kapalı sayfalar ve paylaşım

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Adres sabitleme:** `IAddressedContentRecord` (`Slug`, `SlugFixedAt`); hizmet, referans ve genel sayfa bunu uygular. `ContentPublishing` ilk yayında `SlugFixedAt`'i aynı kayıtta yazar (`TimeProvider` ile). `ContentAuthoring` üç kayıt türünde ilk yayına kadar adresi addan yeniden üretir (`Slug.ForSaveAsync`, kendi adresi dolu sayılmaz), sabitlenince dokunmaz.
- **Ürün adresi:** `ProductAddresses.ForSaveAsync` (taslak, arşiv dolu sayılır; kalıcı silinenin satırı gitmiştir) ve `Product.SetAddress` (sabit adres başka değere yazılamaz). Adres kategori ağacından değil addan üretilir.
- **Arama motoru verisi:** `Infrastructure/SearchEngine/RecordPages.cs` — ürün, kategori, içerik kaydı ve kapalı ekran için başlık, açıklama, yol, kapalı bayrağı; taslak ve arşiv `noindex`.
- **Paylaş:** `wwwroot/js/common.js` (cihaz menüsü, yoksa panoya kopya, dış adres yok) ve `Pages/Shared/_ShareButton.cshtml` (tek gizli düğme).
- **Taramalar:** `AddressSourceScan` — yönlendirme/eski adres yok, platform paylaşım kodu yok, `Slug` ataması tek yerde; form modellerinde `Slug` üyesi yok.
- **Devam işçisi:** test projeleri derlendi; iki test düzeltildi (tarama kalıbı postal adres geçmişi kayıtlarını (`AddressHistory`, `OldAddressLine`) yanlış yakalıyordu, daraltıldı; başlık HTML kodlaması), tohum ürün türü `Digital` yapıldı (hizmet ürünü yayında süre ister).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `Product.cs`, `ProductAddresses.cs` (yeni)
- `src/Shopfolio.Web/Modules/CorporateContent/`: `ContentRecords.cs`, `ContentAuthoring.cs`, `ContentPublishing.cs`, `Offering.cs`, `PortfolioItem.cs`, `CustomPage.cs`
- `src/Shopfolio.Web/Infrastructure/SearchEngine/RecordPages.cs` (yeni); `wwwroot/js/common.js`; `Pages/Shared/_ShareButton.cshtml` (yeni)
- `tests/Shopfolio.UnitTests/Architecture/AddressSourceScan(.cs, Tests.cs)`, `Infrastructure/RecordPagesTests.cs`; `tests/Shopfolio.IntegrationTests/Catalog/PageAddressTests.cs`, `CorporateContent/ContentAuthoringTests.cs`, `ContentStatusTests.cs`
- `Docs/DEFERRED_BACKLOG.md` (D-315, D-357 yerinde), `Docs/DEFERRED_BACKLOG.d/T363.md`, `Docs/STATUS_CHANGELOG.d/T363.md`, `Docs/IMPLEMENTATION_STATUS.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün ve kategori adresi addan, ağaçtan bağımsız | ✓ | `PageAddressTests.AProductsAddress_AndTheCategoriesAddresses_StayWhenTheCategoryIsRenamedOrMoved_KK1` |
| 2 | İlk yayına kadar adla değişir, sonra sabit; sadeleşme, çakışma, boş ad kökü; liste adresi sabit; elle düzenleme yok | ✓ | `PageAddressTests` KK2 testleri (7), `ContentAuthoringTests` T363 (2+), `AddressSourceScanTests.NoInputOrFormModel…_KK2` |
| 3 | Yönlendirme sistemi yok | ✓ | `AddressSourceScanTests.ApplicationSource_HasNoRedirect…`, ihlal örnekleri |
| 4 | Olmayan, silinmiş, yeni kayda verilmemiş/taslak adres "bulunamadı" | ✓ | `PageAddressTests.AnUnknownAddress_ADeletedProductsAddress_AndADraftSeenByAVisitor_AreTheSamePageNotFound_KK4` (ürün okuyucusu); içerik sayfası okuyucusu D-690 |
| 5 | Arama bilgileri otomatik: başlık, açıklama, harita, yapılandırılmış veri, önizleme | ✓ | `RecordPagesTests` KK5 (başlık, açıklama, kısa alan tavanı, logo); harita T313 |
| 6 | Kapalı sayfalar `noindex`, haritada yok | ✓ | `RecordPagesTests` KK6 (ekran, taslak, arşiv); harita `SiteMapFiles` (T313) |
| 7 | Tek Paylaş düğmesi, platform kodu yok | ✓ | `AddressSourceScanTests` paylaş testleri (düğme tek ve gizli, dış adres yok), `OperationSourceScan`/`ThirdPartyContentScan` yeşil |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçinin yaptıkları dahil bütün maddeleri diff'te yeniden denetledi; `sik-fail-nedenleri.md` okundu.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 kategori taşınınca ürün adresi değişmez | ✓ | `PageAddressTests` KK1 |
| KK2 sadeleşme, çakışma (taslak/arşiv dolu, silinen serbest), boş ad kökü | ✓ | `PageAddressTests` (`…TurkishLettersAreSimplified…`, `…CountAsUsed…`, `…GivesTheRootOfTheType…`) |
| KK2 kategori adresi oluşturmada sabit | ✓ | `PageAddressTests.ACategorysAddress_IsFixedWhenItIsCreated…` |
| KK2 adres düzenleme alanı ve ucu yok | ✓ | `AddressSourceScanTests.NoInputOrFormModelOfTheApplication_CarriesAnAddress…_KK2` (Input/Form/Model türlerinde `Slug` üyesi yok); ürün formu henüz yok → D-689 |
| KK3 yönlendirme ucu yok | ✓ | `AddressSourceScanTests` (gerçek `src` taraması + ihlal ve yakın-ıska örnekleri) |
| KK4 üç hâl aynı sayfa | ✓ | `PageAddressTests` KK4 (iki test; kalıcı silinen adresin yeni kayda geçmesi bilinen risk K-568) |
| KK5 beş öğe kuralla, görselsiz önizleme logoyla | ✓ | `RecordPagesTests` KK5 (`…PreviewsWithTheLogo_KK5`) |
| KK6 kapalı sayfalar işaretli, haritada yok | ✓ | `RecordPagesTests` KK6 |
| KK7 paylaşım tek düğme, harici kod yok | ✓ | `AddressSourceScanTests.…IsOneHiddenButton…`, `…UsesTheDeviceMenuThenTheClipboard…` |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | yeni bağımlılık yok, gizli bilgi yok, adres sunucuda üretilir (istemci değeri alınmaz), paylaşımda yalnız sayfanın kendi yolu |
| Sık FAIL 1: tarama istisnası geniş | ✓ | yeni istisna yok; `Slug` atama taramasının tek izinli dosyası `ContentAuthoring.cs` |
| Sık FAIL 2: tarama bir çağrı biçimini kaçırıyor | ✓ | `AddressSourceScanTests` çağrı biçimi Theory'leri (alan, dizin erişimi, başlatıcı, karşılaştırma) |
| Sık FAIL 3: 06 "Varsayılan"/FK dizini | ✓ | yeni sütun, tablo, göç yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `ContentPublishing` kurucusuna `TimeProvider` eklendi, bütün kurucu çağrıları güncellendi (Unit 4822, Integration ilgili ad alanları 172 yeşil) |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | 09 metni değişmedi |
| Sık FAIL: tarama istisnası geniş / backlog hedefi bu task olan kayıt | ✓ | D-357 (hedef T363) T549'a ileri atıldı; D-315 notu yerinde |
| `dotnet format` test dosyalarında | ✓ | üç projede `--verify-no-changes` temiz (Unit, Integration, Web) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --filter-not-trait "Consistency=true"` | 4822/4822 (main birleşmesi sonrası; `AddressSourceScanTests` 38/38); Consistency 283/283 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-class …` / `--filter-namespace …` | `PageAddressTests` 10/10; `ContentAuthoringTests`+`ContentStatusTests`+`CategoryTreeAndListsTests` 90/90; `CorporateContent`+`Storefront`+`Catalog` ad alanları 172/172 (PostgreSQL 18.6) |
| Contract | — | — | Bu task'ta uç yok |

> Integration TAM küme CI'da koşar; yerelde ilgili ad alanları koşuldu.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T363-sayfa-adresleri-seo`
- **Commit:** PR başlığındaki son commit
- **PR:** açıldığında yönetici kaydeder
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ürün formu adres işlevlerini çağırır | D-689 | Panel ürün formu T525'te |
| 2 | İçerik sayfası okuyucusu ve 404 | D-690 | Vitrin sayfaları T549 |
| 3 | Sayfalarda başlık, açıklama, Paylaş kullanımı | D-691 | Sayfalar henüz yok |
| 4 | Önizleme görsel yolu | D-692 | Okuyucularla birlikte |
| 5 | Kapalı ekranların `ClosedScreen`'i çağırması | D-693 | Sayfalar sonraki task'larda |
| 6 | `SearchEngineData` ham işaretlemesi (D-357) | D-357 → T549 | Başlığı kullanan örnek sayfa ve testler çok |
| 7 | İçerik yayın izi (D-315 kalanı) | D-315 (T360) | Denetim izi T360'ta |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yönetici izler
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum" parça kuralı gereği dokunulmadı
- **Diğer:** önceki işçi bağlam sınırında bölündü; bu rapor devam işçisinin tam öz denetimini taşır. `main` dala birleştirildi (rebase yok).
