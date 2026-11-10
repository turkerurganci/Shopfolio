# T384 — Dışa aktarma: sipariş CSV (fatura verisi), üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-09

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/CsvExportWriter` (formül etkisizleştirme, noktalı virgül, UTF-8 BOM, kalıcı sütun türleri) ve `Infrastructure/ExportTempDirectory` (açılışta süpürme, istekte eski dosya süpürme, yazılamayan dizinde açılış reddi, `DeleteOnClose` akışı) 09 §10.10 madde 1 yerinde.
- `OrderExport`: oluşma Türkiye gününe göre aralık, ödemesi tamamlanmış (Paid, PartiallyRefunded, Refunded) siparişler, kalem başına satır, donmuş fatura alanları, ödeme, kargo, iletişim ve geç ödeme sütunları, imha edilmişte ad, e-posta, adres boş; sayfalı okuma (`ExportLimits.PageSize`, eşit oluşma anında kayıpsız).
- `CustomerExport` (Membership) ve `ContactRequestExport` (CorporateContent): aralıksız, tüm hesaplar ve tüm saklanan talepler.
- `IDataExport`/`DataExport` (Administration): üç dışa aktarma, iz satırı dosyadan sonra ve kendi işleminde; boş küme, yanlış aralık ve boyut/süre tavanı aşımı iz yazmaz, dosya kalmaz.
- `deploy/caddy/Caddyfile` `/panel/disa-aktarma*` için `response_header_timeout 150s`; `.env.example` başlığı daraltıldı ve `Export__TempDirectory` satırı eklendi.
- Bu turda: Integration testi (31 test) derlendi ve koşuldu; `OrderExport` ilk halinde `IAfterSalesModule` ile bağımlılık döngüsü kuruyordu, AfterSales ve Settings çağrıları gecikmeli çözülür; `ExportDirectoryStartup` Administration kaydına taşındı (yeniden-yazma açılışından sonra başlar); `ExportOutcome` ve `ExportRangeError` kendi dosyalarına ayrıldı ve "saklanmaz" listesine alındı; `ExportTempDirectory` geniş catch'ten tek tek istisnaya geçti; `CsvEnumLabels` sözlük yerine switch; Disposal, Address, Membership düzeneklerine `OrderExport` kaydı; Membership "dışa aktarma yüzeyi yok" testi T384'ün üye listesi yöntemi için daraltıldı.

## Etkilenen Modüller / Dosyalar

Ordering (`OrderExport.cs`, `OrderingModule.cs`), Payment (`ListLatePaymentsAsync`), Membership (`CustomerExport`), CorporateContent (`ContactRequestExport`), Administration (`DataExport`, `IDataExport`, `ExportOutcome`, `ExportRangeError`), Infrastructure (`CsvExportWriter`, `ExportTempDirectory`), `deploy/`.

## Kabul Kriterleri Kontrolü

### Öz denetim (bitiş kapısı 2)

| # | Madde | Durum | Kanıt |
|---|---|---|---|
| 1 | Aralık oluşma günü; ödenmişler; Başarısız ve bekleyen girmez | ✓ | `DataExportTests.TheOrderFile_CarriesTheOrdersCreatedInTheRange_..._KK1`, `TheRange_IsTheCreationDayInTurkey_..._KK1`; `OrderExportTests` |
| 2 | Satır bir kalem, sipariş alanları yinelenir | ✓ | `ALine_IsAnOrderItem_AndTheOrdersValuesRepeatOnEachLine_KK2` |
| 3 | Fatura verisi donmuş değerden | ✓ | `TheInvoiceColumns_AreTheFrozenValues_..._KK3` (ürün, fiyat, firma kimliği sonradan değişir, bayt eşit) |
| 4 | Ödeme, kargo, iletişim alanları | ✓ (düzeltme 1: B1 giderildi) | `ThePaymentShippingAndContactColumns_..._KK4`, `TheBankTransferOrder_..._KK4`; kargoya verme tarihi kuralı: `AnItemWhoseWholeQuantityWasCancelledBeforeTheShipment_...`, `..._InTwoRecords_...`, `AnItemCancelledInPartBeforeTheShipment_KeepsTheShippingDay_KK4`, `..._AndWithdrawnAfterIt_...`, `AnItemWithdrawnWholeAfterTheShipment_...` (36/36 geçti) |
| 5 | Geç ödeme: iki tarih ve tutar | ✓ | `ACancelledOrder_ThatALateCardPaymentCameTo_..._KK5`, `ACancelledOrderWithoutALatePayment_..._KK5` |
| 6 | İmha edilmişte ad, e-posta, adres boş | ✓ | `ADisposedOrder_StaysInTheFile_..._KK6` |
| 7 | Tüm üyeler ve talepler, aralıksız | ✓ | `TheMemberList_HoldsEveryAccount_..._KK7`, `TheContactRequestList_HoldsEveryKeptRequest_..._KK7`, sayfa testleri |
| 8 | Her dışa aktarma izde | ✓ | `EveryDownload_IsInTheTrail_..._KK8`, `TheTrailRow_IsWrittenInItsOwnTransaction_...`, `AnEmptySet_AWrongRange_...`, boyut/süre/iptal testleri |
| 9 | Başka dışa aktarma yolu yok | ✓ | `ExportSurfaceTests` (KK9 taraması, ihlal örnekleriyle), `MembershipAccountDeletionTests.ThePersonalDataRequest_HasNoTable...` |
| KK1–KK9 | Doğrulama kontrol listesi | ✓ | yukarıdaki satırlar; KK9 birim testi |
| 09 §10.10 | madde 1 yol, 2 biçim, 3 akış, 5 sütunlar, 6 dizin, 7 ömür | ✓ (madde 2 Excel denemesi ve gerçek Kestrel: D-850) | `CsvExportWriterTests`, `ExportSurfaceTests`, `DataExportTests` dizin testleri |
| 09 §25 | ilgili satırlar | ✓ | `ExportSurfaceTests`, `JobSourceScanTests`, `CacheSourceScanTests`, `EnumSourceScanTests`, `EnumLockTests` geçer |
| Güvenlik | yeni uç yok; dosya adı Guid, dizin yalnız sürecin; formül etkisizleştirme | ✓ | `CsvExportWriterTests` formül örnekleri |

## Test Sonuçları

- UnitTests: 5696/5696 geçti (düzeltme 1, main birleştirmesinden sonra; Architecture ve Consistency dahil).
- IntegrationTests (düzeltme 1): `DataExportTests` 36/36; Ordering ad alanı 502/502 (`LegalInterestWarningsAsync` testleri dahil, `ClosingRecordMoment` değişikliği).
- Derleme `-warnaserror` temiz; `dotnet format --verify-no-changes` `src/Shopfolio.Web` ve `tests/Shopfolio.IntegrationTests` için temiz.

### Düzeltme 1 (tur 1 B1)

`OrderExport.FullyClosedBeforeShipmentAsync` artık gönderimden önce kapanan adetlerin toplamına bakar (`ClosingRecordMoment` kayda kapanan adedi taşır: iptal ve gecikme feshi, anı gönderimden önce olanlar); toplam kalemin adedine ulaşınca "Kargoya verme tarihi" boşalır. Kısmi kapanma ya da gönderim sonrası kapanma (cayma) tarihi korur (`09 §10.10` madde 5). Yeni testler: `AnItemWhoseWholeQuantityWasCancelledBeforeTheShipment_HasNoShippingDay_KK4`, `AnItemWhoseQuantityWasCancelledBeforeTheShipmentInTwoRecords_HasNoShippingDay_KK4`, `AnItemCancelledInPartBeforeTheShipment_KeepsTheShippingDay_KK4`, `AnItemCancelledInPartBeforeTheShipment_AndWithdrawnAfterIt_KeepsTheShippingDay_KK4`, `AnItemWithdrawnWholeAfterTheShipment_KeepsTheShippingDay_KK4`. D-736'nın "testle" şartı bu testlerle karşılandı (kapanış satırında adlar var).

## Doğrulama

**Tur 2 — ✓ PASS (2026-11-09; head `2640fb6a`, PR #502; `origin/main` `8cd6652e` dalda, birleştirme "Already up to date").** Bulgu: 0. Düzeltme gerekli: hayır. Kapsam tam (dokuz kriter yeniden koşuldu).

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `scripts/git-hooks`; ana dal son üç run `success` (42643385147, 42643385305, 42640152318). Dal CI 42651565853 doğrulama anında koşuyordu (Lint, Build, Unit, Contract, Migration dry-run, E2E pass; Integration ve Paketleme bekliyor); merge öncesi yeşil olması yardımcının kapısıdır.

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Aralık oluşma Türkiye günü; yalnız ödemesi tamamlanmış | ✓ | `OrderExport.ReadAsync` (`TurkeyTime.StartOf`/`EndOfRange`, `IsPaymentCompleted` ya da geç ödeme); KK1 testleri geçti |
| 2 | Kalem başına satır | ✓ | `ALine_IsAnOrderItem_…_KK2` geçti |
| 3 | Fatura verisi donmuş değerden | ✓ | `InvoiceDataReader.ComposeAsync`; sütunlar `09 §10.10` madde 5 tablosuyla eşleşti (başlık önekleri D-851); `…_KK3` geçti |
| 4 | Ödeme, kargo, iletişim alanları | ✓ | B1 giderildi: `FullyClosedBeforeShipmentAsync` yalnız anı gönderimden önce olan iptal ve fesih kayıtlarının adetlerini topluyor, toplam kalemin adedine ulaşınca tarihi boşaltıyor; cayma artık sayılmıyor. Beş yeni test (tam iptal, iki kayıtla tam iptal, kısmi iptal, kısmi iptal + sonradan cayma — tur 1 sondajının senaryosu —, tam cayma) geçti |
| 5 | Geç ödemede iki tarih ve tutar | ✓ | `ListLatePaymentsAsync`; `…_KK5` testleri geçti |
| 6 | İmha edilmişte ad, e-posta, adres boş | ✓ | `…_KK6` geçti |
| 7 | Bütün üyeler ve saklanan talepler, aralıksız | ✓ | `CustomerExport`, `ContactRequestExport`; KK7 ve sayfa testleri geçti |
| 8 | Her dışa aktarma izde | ✓ | `DataExport.ProduceAsync`; `EveryDownload_IsInTheTrail_…_KK8` geçti |
| 9 | İkinci dışa aktarma yolu yok | ✓ | `ExportSurfaceTests` geçti |

**Task satırları:** değişiklik yok — `VAL-D029` PASS; `VAL-A266`…`A271` uç ister (T524), `BEKLEMEDE`. `12_matris_denetle.py --kapsam 1-77` hata 0.

**Testler (validator):** restore; `build -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` üç projede 0; Unit 5696/5696; betik testleri 4/4; Integration `DataExportTests`, Disposal (3 sınıf), `AddressTests`, `MembershipTests`, `MembershipAccountDeletionTests`, `DeliveryPeriodTests`, `LateDeliveryAndFirmCancellationTests` (`ClosingRecordMoment`'in öteki tüketicisi `OrderPeriods`), `MainFlowTests` 279/279 (`postgres:18`, `track_commit_timestamp=on`); `restore --locked-mode` 0.

**Güvenlik:** düzeltme yalnız okuma mantığı ve bir sözleşme kaydına alan ekliyor; sır, uç, girdi, paket değişikliği yok.

**K-notları:** K1 → D-848'e not (`Docs/DEFERRED_BACKLOG.d/T384.md`): kaydın gerekçesi "`ItemRemoval` anı tutulmuyor" doğru değil (`ItemRemoval.CreatedAt` var); asıl boşluk `ClosingRecordMoment`'in çıkarma kaydını taşımaması. Çıkarma kargodan önce olur (`02 §7.2.2`) ve yazanı T386'dır; bugün üretilemez. T386 gelince gönderimden önce iptal + çıkarma ile tam kapanan kalemde tarih dolu kalır.

**Yapım raporu karşılaştırması:** tam uyumlu. Öz denetim tablosu kanıtlı; kriter 4 düzeltmesi ve test adları validator bulgusuyla örtüşüyor. Küçük: rapor `dotnet format`'ı iki projede yazıyor, validator üçünde koştu (temiz).

**Tur 1 — ✗ FAIL (2026-11-09; head `f2ad23c3`, PR #502; `origin/main` `d5676e75` ile güncel, birleştirme gerekmedi).** Bulgu: 1 (ORTA, S1). Düzeltme gerekli: evet.

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; ana dal son üç run `success` (42627783852, 42627783617, 42619534143); dal CI 42634024845 yeşil (Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, CI Gate).

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Aralık oluşma Türkiye günü; yalnız ödemesi tamamlanmış | ✓ | `OrderExport.ReadAsync` (`CreatedAt` + `TurkeyTime.StartOf`/`EndOfRange`, `IsPaymentCompleted` ya da geç ödeme); `DataExportTests` KK1 iki test geçti |
| 2 | Kalem başına satır | ✓ | `ALine_IsAnOrderItem_…_KK2` geçti |
| 3 | Fatura verisi donmuş değerden | ✓ | `InvoiceDataReader.ComposeAsync`; 37 sütun `09 §10.10` madde 5 tablosuyla eşleşti (başlık önekleri D-851); `…_KK3` geçti |
| 4 | Ödeme, kargo, iletişim alanları | ~ | Genel yol doğru (`…_KK4` iki test geçti), ama `FullyClosedBeforeShipmentAsync` gönderilmiş adedi olan kalemde kargoya verme tarihini boş yazıyor — B1. D-736'nın istediği test yok |
| 5 | Geç ödemede iki tarih ve tutar | ✓ | `ListLatePaymentsAsync`; `…_KK5` iki test geçti |
| 6 | İmha edilmişte ad, e-posta, adres boş | ✓ | `Order.ErasePersonalData` alanları boşaltır; `…_KK6` geçti |
| 7 | Bütün üyeler ve saklanan talepler, aralıksız | ✓ | `CustomerExport`, `ContactRequestExport`; KK7 testleri ve sayfa testleri geçti |
| 8 | Her dışa aktarma izde | ✓ | `DataExport.ProduceAsync` (iz dosyadan sonra, kendi işleminde); `EveryDownload_IsInTheTrail_…_KK8` yönetici kimliği ve adıyla |
| 9 | İkinci dışa aktarma yolu yok | ✓ | `ExportSurfaceTests`; `CsvFileResult` kullanılmıyor; içe aktarma/toplu güncelleme adı yok |

**Bulgu B1 (ORTA, S1, `VAL-A267` komşusu):** `OrderExport.FullyClosedBeforeShipmentAsync` kapanan adedi `ClosedQuantitiesAsync`'ten (iptal, çıkarma, fesih ve cayma) alıyor; ama kalemi "gönderimden önce kapanmış" saymak için iptal ya da fesih anlarından herhangi birinin gönderimden önce olması yetiyor. Örnek: 2 adetlik kalemin 1'i gönderimden önce iptal ediliyor, kalan 1 gönderiliyor ve teslimden sonra cayılıyor. Bu kalemin "Kargoya verme tarihi" hücresi boş geliyor; `09 §10.10` madde 5 ve kodun kendi yorumuna göre ("An item closed only in part, or after the shipment, keeps the date") dolu olmalı. Validator sondajı (`DataExportTests`'e geçici test; beklenen `2026-11-07`, gelen `""`) bunu doğruladı; sondaj geri alındı. D-736 "testle" diyordu, ama bu kural için test yok. Düzeltme: gönderimden önce kapanan adet toplamı kalemin adedine ulaşınca tarih boşaltılır; testte tam kapanma, kısmi kapanma ve karışık durum sınanır.

**Task satırları:** `VAL-D029` → PASS (servis düzeyinde iz satırı yönetici ve anla birlikte yazılıyor). `VAL-A266`…`A271` "Uç yanıtı" istiyor, uç T524'te → `BEKLEMEDE` (T318/T338 emsali). `12_matris_denetle.py --kapsam 1-77` hata 0.

**Testler (validator):** restore, `build -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç projede 0; Unit 5666/5666; betik testleri 4/4; Integration `DataExportTests`, Disposal (3 sınıf), `AddressTests`, `MembershipTests`, `MainFlowTests`, `StorefrontReadersTests` 263/263 (`postgres:18`, `track_commit_timestamp=on`).

**Güvenlik:** sır yok; yeni uç yok (yetki T524); aralık girdisi denetleniyor; yeni paket yok; dosya adı Guid, Unix kipi yalnız sahip; formül etkisizleştirme bütün metin hücrelerinde.

**K-notları:** K1 → D-867 (ödenmiş siparişe gelen ikinci kart ödemesinde geç ödeme sütunları dolu; `09 §10.10` tablosu yalnız iptal edilmiş siparişi anıyor, `02 §6.2.16` K-1211 ikisini aynı yola koyuyor — belge kayması).

**Yapım raporu karşılaştırması:** 1 uyuşmazlık. Öz denetim KK4'ü ✓ yazıyor; B1 yüzünden ~. D-736 "çözüldü" diye kapatılmış, ama istenen test yok. "Kısıtlar" bölümüne atıf var, bölüm raporda yok (`dotnet format` sonucu validator koşusunda temiz).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** `Export__TempDirectory` (isteğe bağlı, yorum satırı)
- **Paketleme / dağıtım:** `Caddyfile` yeni `handle /panel/disa-aktarma*` bloğu
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T384-disa-aktarma`
- **PR:** #502
- **CI:** *(yönetici izler)*
- **Post-merge run'lar:** squash `5352fad` (#502) — CI 42659142242 success, Publish 42659142166 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `caddy validate` yerelde `server block without any key` hatası verdi; main'deki dosya da aynı hatayı verdi (T384'ten bağımsız) | `caddy:2.11.7-alpine`, `SHOPFOLIO_DOMAIN=example.com` | ⚠ ortam, CI'da bakılır |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ödeme tamamlanmış kümesi tek yerde (T383 ile) | D-847 | T383 ya da F3 kapısı |
| 2 | Kargo öncesi tam kapanmış kalemde kargoya verme tarihi | D-848 | Sözleşmede kapanış anı yok |
| 3 | Sayfa ucu ve `CsvFileResult` akışa geçişi | D-849 | Sayfa görevi (T524) |
| 4 | Excel'de açılış, gerçek Kestrel doğrulaması | D-850 | Uç yok, T524 |
| 5 | Aynı adlı grup sütunları belgede | D-851 | Belge düzeltmesi |

### ⚠ Kararlar (kolay, kurala en uygun seçenekle)

- Geç ödeme sütunları hem iptal edilmiş siparişe gelen hem ödenmiş siparişe gelen ikinci ödeme için dolu (09 §10.10 tablosu yalnız iptal edileni söylüyor; `02 §6.2.16` ikisini sayar).
- İade tarihi gönderilmediyse boş, tutar dolu. "Ödeme tamamlanmış" = Paid, PartiallyRefunded, Refunded.
- `PageSize` üç dosyada ortak (`ExportLimits.PageSize`); `CustomerExport` `new` ile, öteki ikisi DI ile kurulur (kabul).
- `OrderExport` AfterSales ve Settings modüllerini `IServiceProvider` ile gecikmeli çözer (döngüyü ve harness kırılmasını önlemek için).
