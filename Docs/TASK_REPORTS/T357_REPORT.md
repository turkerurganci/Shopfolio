# T357 — Fatura verisi ve dışa aktarmada donmuş kimlik

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; merge yardımcıda) | **Tarih:** 2026-11-09

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
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-09, head `5c08e626`; kayıt commit'inden önce `origin/main` (`e958c598`, T359 #479 + T388 #483) birleştirildi, çakışma yok). Kriter 1–4, KK1–KK4, `09 §25` ve güvenlik taraması ✓ |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-736, T384) |

**Doğrulayıcı kanıtları (tur 1):**

| # | Kriter / KK | Sonuç | Doğrulayıcı kanıtı |
|---|---|---|---|
| 1 / KK1 | Fatura kesilmez; e-Arşiv / e-Fatura entegrasyonu yok | ✓ | `grep -rniE "invoice\|fatura\|e-?ar[sş]iv\|efatura" src/` (göçler hariç) yalnız T357'nin üç dosyasını ve dört yorum / dizgi değerini verir (`OrderNumber.cs`, `OrderAccess.cs` yorumu; `AuditEntryRules` kişisel veri anahtarı `invoiceaddress`; bildirim şablonunda adres tipi metni "fatura"); `Pages/`, `Endpoints/`, `wwwroot/` içinde eşleşme yok; paket eklenmedi. Unit `InvoiceSurfaceTests` (bulgu 0, kapsam testi > 5000 ad, 14 ihlal + 5 serbest örnek) yeşil |
| 2 / KK2 | Fatura verisinin bütün alanları siparişin donmuş değerlerinden; kargo KDV'si oranlara bölünmüş | ✓ | `InvoiceData` / `InvoiceItem` alanları `02 §3.25.1` listesiyle birebir (kalem dökümü, KDV oranı ve tutarı, indirim, kupon payı ve kodu, kargo ücreti, `ShippingFeeVatBreakdown`, fatura adresi, donmuş kimlik, ödeme yöntemi, onay anı, kalem tipine göre kargoya verme anı, kargo şirketi, alan adı). Kimlik, tutar ve bölüşüm `Order`/`OrderItem` sütunlarından; anlar `06` §4.17'ye uygun olarak son düzeltilmemiş Ö1 ve S5/S8 geçiş kaydından ("okunur, saklanmaz" — `06`'nın `02 §2.2` m6 ve `02 §3.20.7` eşleme satırları). Integration KK2: ürünün KDV oranı ve fiyatı sonradan değişir, veri değişmez; bölüşüm iki oranlı; düzeltilmiş S8 sayılmaz. `OrderTransitions.Correction.cs` yeniden düzenlemesi davranışı değiştirmez: `correction_fields_only_on_correction` kısıtı `corrected_transition_record_id`'yi yalnız `Correction` satırında izin verir, eklenen kod koşulu eşdeğerdir |
| 3 / KK3 | Fatura dosyası için alan, uç, ekran yok | ✓ | Diff göç, varlık özelliği, sayfa, uç eklemez (`git diff --stat origin/main...HEAD`); `Order`/`OrderItem`'da fatura dosyası alanı yok; KK1 taraması ve `InvoiceSurfaceTests` |
| 4 / KK4 | Firma kimliği değişince eski siparişin fatura verisi eski kimliği verir | ✓ | `Compose` kimliği yalnız `Order.Company*`'dan okur; `Company*` alanlarını yazan tek yer `OrderFreeze.FreezeLegalIdentity` (`grep "CompanyLegalName ="`); Integration KK4 (`IOrderingModule` üzerinden, şirket satırı değiştikten sonra eski sipariş "Eski Unvan", yeni sipariş ve panel "Yeni Unvan") yeşil |
| `09 §25` | ilgili satırlar | ✓ | Ordering yalnız `Catalog`/`Payment`/`Settings.Contracts` ve `Infrastructure`'a başvurur (Unit `Architecture` 1095/1095); arayüz yöntemi `SaveChanges` çağırmaz, kilit almaz, `AsNoTracking`; saat okunmaz; para üretilmez; tek işlev: `ShippingVatPart`, `CarrierCatalog.Describe`, `OrderPeriods` tanımları; yeni bağımlılık `IOptions<SiteOptions>` web ve komut düzeneğinde çözülür (Integration `Commands` 59/59); göç, ayar, paket yok. Ad: "fatura verisi"nin sözlükte kod adı yok, alan adları `06 §4.17`/§4.18 ile birebir — yapımın ⚠ a notu kabul |
| Güvenlik | taraması | ✓ | Sır yok; yeni uç ya da POST yok (yetki etkisi yok); okuma parametreli EF sorgusu; kişisel veri kayda yazılmıyor; yeni dış bağımlılık yok |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana kopyanın bekçisi, worktree); ana dal son 3 tamamlanmış run 42542560429, 42542560342 (`e958c598` T388), 42540074716 (`241f8669` T359) — hepsi success. Dal CI: run 42542237326 (`5c08e626`) success — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate geçti.

**Testler (doğrulayıcı, yerel):** `5c08e626`'da `dotnet restore` temiz; `dotnet build Shopfolio.slnx --no-restore -warnaserror` 0 uyarı / 0 hata; `dotnet format --verify-no-changes` üç proje çıkış 0; Unit Architecture 1095/1095, Consistency 283/283, kalan 3800/3800 (T357 sınıfları 27/27); betik testleri 4/4; Integration (PostgreSQL 18 konteyner) `Shopfolio.IntegrationTests.Ordering` 360/360 (`InvoiceDataTests` 3/3 dahil). Birleştirme sonrası: build 0 uyarı / 0 hata, Unit 5220/5220, Integration `InvoiceDataTests` 3/3, `Commands` 59/59 (`track_commit_timestamp = on` ile; T388'in `RestoreCommandsTests`'i bu ayarı ister).

**`12` satırları:** KP-52'nin satırları `VAL-A266`…`VAL-A271`, `VAL-D029` dışa aktarma dosyası, ucu ve işlem izi ister (T384); T357 dosya, uç ve iz eklemez (D-724 → T384). KD-1'in kendi satırı yok. Servis düzeyi kanıt yukarıdaki tablodadır; satırların `Durum`'u `BEKLEMEDE` kaldı (T355 emsali). `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** K1 — fiziksel kalemin kargoya verme anı siparişin gönderimidir; gönderimden önce iptal edilen ya da çıkarılan fiziksel kalem de bu anı alır ve fatura verisi kalem işlemlerini (iptal, iade adedi) taşımaz. `02 §3.25.1`/§10.7.1 bunu düzenlemez; dosyanın satırında nasıl yazılacağı T384'ün kararıdır → D-736.

**Yapım raporu karşılaştırması (adım 13):** tam uyumlu; öz denetim tablosu her kriter, KK ve `09 §25` maddesi için kanıtlı. ⚠ a–d notlarında doğrulayıcı aynı sonuca vardı. Sayı farkı: Unit 5178 (yapım) ↔ 5220 (doğrulayıcı, birleştirme sonrası) — main'den gelen testler; hüküm etkisi yok.

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
- **Post-merge run'lar:** squash `0203297` (#485) — CI 42548005029 success, Publish 42548005043 success (F3 gate'te dolduruldu)

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
