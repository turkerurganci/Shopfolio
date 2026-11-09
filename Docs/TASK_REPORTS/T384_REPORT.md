# T384 — Dışa aktarma: sipariş CSV (fatura verisi), üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları

**Faz:** F3 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-09

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
| 4 | Ödeme, kargo, iletişim alanları | ✓ | `ThePaymentShippingAndContactColumns_..._KK4`, `TheBankTransferOrder_..._KK4` |
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

- UnitTests: 5633/5633 geçti (main birleştirmesinden önceki koşu).
- IntegrationTests: `DataExportTests` 31/31; Catalog, Disposal, Administration, Model, Ordering, CorporateContent, Web ve kalan sınıflar parça parça koşuldu, hepsi geçti (düzeltmelerden sonra).
- `dotnet format --verify-no-changes`: bkz. Kısıtlar.

## Doğrulama

*(validator doldurur)*

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** `Export__TempDirectory` (isteğe bağlı, yorum satırı)
- **Paketleme / dağıtım:** `Caddyfile` yeni `handle /panel/disa-aktarma*` bloğu
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T384-disa-aktarma`
- **PR:** *(PR açılınca)*
- **CI:** *(yönetici izler)*

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
