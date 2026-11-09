# T357 — Fatura verisi ve dışa aktarmada donmuş kimlik

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Fatura verisi okuması:** `IOrderingModule.ListInvoiceDataAsync(orderIds)` → `InvoiceData` (`Ordering/Contracts/InvoiceData.cs`). Siparişin kendi değerlerinden verir: sipariş numarası, oluşma anı, toplam, kupon kodu, kargo ücreti ve donmuş KDV bölüşümü (`ShippingVatPart`, Katalog sözleşmesinin aynı kaydı), fatura adresi, siparişin donmuş firma kimliği (`FrozenCompanyIdentity`), ödeme yöntemi, ödeme onayı anı, kargo şirketinin adı, kendi aracıyla teslim, alan adı (`SiteOptions.Domain`) ve kalemler (`InvoiceItem`: ad, seçenekler, tip, adet, birim fiyat, indirim oranı ve indirimli fiyat, kupon payı, KDV oranı ve tutarı, kargoya verme anı).
- **Saf birleştirme:** `InvoiceDataReader.Compose` — kalemin kargoya verme anı tipine göre: fizikselde siparişin gönderimi, hizmette "tamamlandı" işareti (`DeliveryMarkedAt`), dijitalde ödeme onayı (`02 §3.25.1`). Kargo şirketinin adı gönderimden sonra `CarrierCatalog.Describe` ile; "Diğer" yazılan adla, kendi aracında ad yok. İmha edilmiş siparişte fatura adresi boş.
- **Tek tanım:** ödeme onayı ve gönderim `OrderPeriods.PaymentApprovals` / `OrderPeriods.Shipments` (düzeltilmemiş Ö1, düzeltilmemiş S5/S8). `PaymentApprovedAtAsync` ve düzeltme koşulunun "gönderildi mi" sorusu (`OrderTransitions.Correction.cs`) bu tanımlara bağlandı. Okuma toplu: sipariş listesi için dört sorgu (T384'ün dosyası için N+1 yok).
- **Fatura yüzeyi yok:** `InvoiceSurfaceTests` uygulama derlemesinin bütün adlarını (tip, üye, enum değeri, parametre, Razor sayfası, varlık özelliği) sözcüklere bölüp tarar; fatura kesen, yükleyen, ileten, arşivleyen ya da e-Arşiv/e-Fatura bağlayan ad bulmaz. Tek istisna okumanın beş adı (birebir). Tarama örnek ihlallerle sınandı (`09 §19.2 m.3`).
- **Testler:** Unit `InvoiceDataTests` (6), `InvoiceSurfaceTests` (2 + 19 örnek); Integration `InvoiceDataTests` (3: KK2 iki test, KK4).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/`: `Contracts/InvoiceData.cs` (yeni), `InvoiceDataReader.cs` (yeni), `Contracts/IOrderingModule.cs`, `OrderingModule.cs` (kurucuya `IOptions<SiteOptions>`), `OrderPeriods.cs` (iki tanım), `OrderTransitions.Correction.cs` (tanıma bağlandı)
- `tests/Shopfolio.UnitTests/`: `Ordering/InvoiceDataTests.cs`, `Architecture/InvoiceSurfaceTests.cs` (yeni)
- `tests/Shopfolio.IntegrationTests/Ordering/InvoiceDataTests.cs` (yeni)
- `Docs/`: `IMPLEMENTATION_STATUS.md` (satır), `STATUS_CHANGELOG.d/T357.md`, `DEFERRED_BACKLOG.d/T357.md`, bu rapor

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ürün fatura kesmez; e-Arşiv ve e-Fatura entegrasyonu yok | ✓ | `InvoiceSurfaceTests.NoNameOfTheApplication_IssuesStoresOrIntegratesAnInvoice_BesideTheOneInvoiceDataRead_KK1_KK3`; örnekler `TheScan_FindsEachFormOfAnInvoiceSurface` (`EArchiveClient`, `EFaturaSender`, `EInvoiceProvider`, `IssueInvoiceAsync`…) |
| 2 | Sistem fatura verisini taşır: kalem dökümü, KDV, indirim ve kupon payı, kargo ücreti ve bölünmüş KDV'si, fatura adresi, donmuş kimlik, ödeme yöntemi ve onay tarihi, kargoya verme tarihi (hizmette tamamlama, dijitalde ödeme onayı), kargo şirketi, alan adı; dışa aktarmayla (T384) verilir | ✓ | `InvoiceData` alanları; Unit `TheInvoiceData_GivesEveryFieldFromTheOrdersFrozenValues_AndTheFeesVatSplitOverTheRates_KK2`, `TheHandOverMoment_…_KK2`, `TheCarrierName_…_KK2`; Integration `TheInvoiceData_GivesEveryFieldFromTheFrozenValues_AfterTheProductChanged_WithTheFeesVatSplit_KK2`. T384 bağlantısı D-724 |
| 3 | Fatura saklanmaz, gösterilmez; sipariş sayfasında yükleme, iletme, arşiv yok | ✓ | Aynı tarama: `InvoiceFileId`, `UploadInvoice`, `SendInvoiceToCustomer`, `InvoiceArchive`, `Pages_Admin_Orders_Invoice` örnekleri yakalanır, uygulamada bulgu 0; diff göç, sütun, sayfa eklemiyor |
| 4 | Fatura verisinde siparişin donmuş firma kimliği, panelin o anki kimliği değil | ✓ | `Compose` kimliği `Order.Company*` sütunlarından okur; Integration `WhenTheFirmChangesItsIdentity_AnEarlierOrdersInvoiceData_GivesTheOldIdentity_KK4` (şirket satırı değişir, panel yeni kimliği, eski sipariş eskiyi verir); Unit `TheIdentity_IsTheOrdersFrozenCopyByItsType_KK4` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1 | ✓ | `InvoiceSurfaceTests` (bulgu 0 + 14 ihlal örneği + 5 serbest örnek) |
| Kriter 2 | ✓ | `InvoiceData.cs` (alan listesi `02 §3.25.1` ile birebir); Unit KK2 testleri (4); Integration KK2 testleri (2) |
| Kriter 3 | ✓ | `InvoiceSurfaceTests`; `git diff --stat` göç ve sayfa yok |
| Kriter 4 | ✓ | Integration KK4; Unit KK4 |
| KK1: fatura kesen ya da entegre eden uç yok | ✓ | `InvoiceSurfaceTests` — tarama tip, üye, parametre, Razor sayfası adlarını kapsar (`SalesModelShape.NamesOf`); kapsam testi `TheScanReadsTheWholeApplication_AndSeesTheInvoiceDataRead` |
| KK2: bütün alanlar siparişin donmuş değerlerinden, kargo KDV'si oranlara bölünmüş | ✓ | Integration KK2: ürünün KDV'si ve fiyatı sonradan değişir, veri değişmez; bölüşüm `ItemPricing.SplitShippingFee`'den türetilen beklenenle eşit, iki oran; düzeltilmiş S8 gönderim sayılmaz (gönderim anı S5'in) |
| KK3: fatura dosyası için alan, uç ve ekran yok | ✓ | `InvoiceSurfaceTests` (varlık özellikleri ve sayfa tipleri derlemenin adlarıdır) |
| KK4: kimlik değişince eski siparişin verisi eski kimliği verir | ✓ | Integration KK4 (`IOrderingModule` üzerinden, gerçek `ISettingsModule.GetCompanyIdentityAsync`) |
| `09 §25` — modül sınırı | ✓ | Ordering yalnız `Catalog.Contracts`, `Payment.Contracts`, `Settings.Contracts` ve `Infrastructure`'a başvurur; arayüz yöntemi `SaveChanges` çağırmaz, işlem açmaz; Unit tam koşu (`ModuleBoundaryTests` dahil) 5178/5178 |
| `09 §25` — tek işlev | ✓ | Kargo KDV'si kaydı Katalog'un `ShippingVatPart`'ı (yeni kopya yok); kargo adı `CarrierCatalog.Describe`; ödeme onayı ve gönderim `OrderPeriods` tanımları, `PaymentApprovedAtAsync` ve düzeltme koşulu da onları çağırır; kalan iki okuma D-725 |
| `09 §25` — zaman yalnız `TimeProvider` | ✓ | Okuma saat okumaz; anlar geçiş kayıtlarından |
| `09 §25` — para hesabı | ✓ | Tutar üretilmez; donmuş değerler olduğu gibi verilir |
| `09 §25` — sayfalı liste | uygulanamaz | Sayfalı liste yok; okuma verilen kimliklerle sınırlı (T384 aralığı kendisi seçer) |
| `09 §25` — yeni ayar/sabit, göç, sayfa, iş, paket | uygulanamaz | Yeni ayar yok (var olan `Site:Domain` okunur); göç, sayfa, iş, paket yok |
| `09 §25` — modül kurucusuna yeni bağımlılık (sık FAIL 51) | ✓ | `OrderingModule` kurucusuna `IOptions<SiteOptions>`; elle kuran test yok (`grep "new OrderingModule("` boş); DI ile kuran düzenekler (Disposal, Address, Membership, OrderingTestServices) seçenek altyapısını kaydeder; Integration tam koşu yeşil |
| `09 §25` — adlar | ✓ ⚠ | Alan adları `06 §4.17`/`4.18` sütunlarıyla birebir (`ShippingFeeVatBreakdown`, `CompanyLegalName`…); "fatura verisi" sözlükte kod adı taşımıyor → `InvoiceData` (⚠ a) |
| Backlog: hedefi T357 olan kalem | ✓ | `DEFERRED_BACKLOG.md` ve `.d/` içinde hedefi T357 olan kalem yok |
| Sık FAIL nedenleri (talimat 1–5) | ✓ | (1) tarama istisnası beş adın birebir eşleşmesi, önek/sonek serbest değil (`InvoiceDataWriter` örneği yakalanır); (2) tarama bütün ad biçimlerini sözcüklere böler (snake, camel, `e`+`archive` dizisi); (3) göç yok; (4) sayı/şablon değişmedi; (5) 09 metni değişmedi |
| Güvenlik taraması | ✓ | Yeni uç yok; sır yok; okuma parametreli EF sorgusu (`Contains` dizi parametresi); kişisel veri kayda yazılmıyor; yetki etkisi yok (okuma modül içi, çağıran T384 yönetici işlemi) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build src/Shopfolio.Web`, `tests/Shopfolio.UnitTests`, `tests/Shopfolio.IntegrationTests` (`-c Release -warnaserror`) | 0 uyarı, 0 hata |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 5178/5178 |
| Integration | ✓ | ad alanı grupları: Ordering · Disposal · Model · Catalog+Commands+Concurrency+CorporateContent+Payment · Database+FileSecrets+Files+Jobs+Logging+Outages · Pages+Security+SignedEndpoints · Web | 360 · 61 · 1031 · 270 · 288 · 44 · 257 = 2311/2311 (yeni `InvoiceDataTests` 3/3) |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <değişen dosyalar>` | çıkış 0 |
| Contract | uygulanamaz | — | Uç değişmedi |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — (bekliyor) |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (var olan `Site:Domain` okunur)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T357-fatura-verisi`
- **Commit:** `ff95888c` — T357: Fatura verisi ve dışa aktarmada donmuş kimlik
- **PR:** #485
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Tetikleyici tarama (plan tier, kota, API key, paket sürümü, sandbox…) kabul kriterlerinde ve plan metninde eşleşme vermedi; task dış servis çağırmaz (e-Arşiv/e-Fatura entegrasyonu kapsam dışıdır), paket eklemez.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Fatura verisinin CSV'ye yazılması (il adı çözümü dahil) | D-724 | T384'ün işi (`02 §3.25.3` "dosyaya yazılışı T384'tedir") |
| 2 | `ShippingViewReader` ve teslim tarihi denetimi düzeltilmiş gönderimi dışlamıyor | D-725 | Kapsam dışı düzeltme (`09 §23`); tek tanım bu task'ta kuruldu |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana deponun `scripts/git-hooks`'u)
- **Ana dal CI startup (Adım 0):** 42537382825 success · 42537382843 success · 42535966562 success · 42535966531 success
- **Repo memory (bitiş kapısı 9):** dokunulmadı (faz kapısı kayıt PR'ı günceller)
- **⚠ a — ad:** "fatura verisi" sözlükte (`02 §1.2`) ve `06`'da kod adı taşımıyor; `InvoiceData` / `InvoiceItem` / `FrozenCompanyIdentity` seçildi. Alan adları `06` sütunlarıyla birebir.
- **⚠ b — fatura adresi:** firma adresi düzelttiyse fatura verisi siparişin güncel (düzeltilmiş) fatura adresini verir — `02 §3.25.1`/§10.7.1 "donmuş fatura adresi" siparişin fatura adresi alanıdır; belgelerin onay anı tarafları (`GetConfirmationPartiesAsync`, K-1027) ayrı bir okumadır.
- **⚠ c — kargoya verme anı:** fiziksel kalemde siparişin son düzeltilmemiş S5/S8 anı (`06` §4.17 "son S5 ya da S8"; düzeltilen gönderim bir şey göndermemiştir, `03 §1.6.2.3`); hizmette kalemin `DeliveryMarkedAt`'i; dijitalde ödeme onayı (son düzeltilmemiş Ö1). Kargo şirketinin adı yalnız gönderimden sonra verilir.
- **⚠ d — alan adı:** `06` "alan adı kurulumdan" der; veri `SiteOptions.Domain`'i taşır, siparişe donmaz.
