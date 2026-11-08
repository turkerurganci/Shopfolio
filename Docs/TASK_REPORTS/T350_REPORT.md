# T350 — Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi

**Faz:** F3 | **Durum:** ⏳ Yapım tamam, doğrulama bekliyor | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Başlangıç CI

`main` üzerinde son tamamlanmış başarılı koşular: 42460749345, 42457733416, 42457733364.

## Dış varsayım

Yok.

## Yapılan İşler

- **Settings:** `DeliveryProvinces` (firmanın teslimat illeri; varsayılan 81 il), `ISettingsModule.GetDeliveryAreaAsync`, `SaveDeliveryProvincesAsync`. Kayıt izlenen `RemoveRange/Add` ile yazılır (`ExecuteDelete` tarama istisnasını genişletmek gerektirdiğinden bırakıldı).
- **Membership:** `AddressBooks.ResolveAsync` il kontrolü (elle yazılan adres için `ShippingProvinceId`, defter kaydı için `AddressBookId` alanı; `Detail` = il adı), `AddressBookEntry.IsDeliverable`, `MembershipFieldError.Detail`; fatura ili kısıtlanmaz.
- **Catalog:** `ICatalogModule.GetShippingPeriodsAsync`, `VariantShippingPeriod`, `ShippingPeriodReader` (ürün varyanta, firma varsayılanı ürüne göre); `ProductDefinitionRules.CheckShippingPeriod` ve `ProductDefinitionGuard` çit (P-6, 0–10 iş günü) denetimi.
- **Ordering:** `ShippingPromise` (Effective, OfOrder, Text, PromiseEnds), `Order.FreezeShippingPromise` (bir kez), `OrderShippingView` ve `IOrderingModule.GetShippingViewAsync` (müşteri sipariş sayfası görünümü).
- **Tek yol:** teslimat seçeneği, mağazadan teslim, sevkiyat tablosu, ikinci takip numarası yok; mimari testle korunur.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Settings/`, `Membership/`, `Catalog/`, `Ordering/` (ve `Contracts/`)
- `tests/Shopfolio.UnitTests/` (`ShippingPromiseTests`, `DeliveryAreaTests`, `Architecture/SingleDeliveryWayTests`)
- `tests/Shopfolio.IntegrationTests/` (`AddressTests`, `FirmSettingsTests`, `ShipmentTransitionsTests`, `DiscountAndReferenceTests`, `CatalogCoreTablesTests`)
- `Docs/IMPLEMENTATION_STATUS.md` (yalnız tablo satırı), `Docs/STATUS_CHANGELOG.d/T350.md`, `Docs/DEFERRED_BACKLOG.d/T350.md` (D-493–D-496), `Docs/DEFERRED_BACKLOG.md` (D-436 kapandı, D-221 ve D-309 güncellendi)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Teslimat illeri seçilir; kapalı ilde fiziksel sipariş sebebiyle reddedilir; dijital/hizmet girmez; il düzeyi | ✓ | `AddressTests.AProvinceTheCompanyDoesNotDeliverTo_..._T350KK1`; `FirmSettingsTests` (`SaveDeliveryProvinces_*`, `TheDeliveryProvinces_StartAsAllEightyOne_*`); `DeliveryAreaTests` |
| 2 | Defterdeki kapalı il adresi seçilemez | ✓ | `AddressTests.ABillingProvince_IsNeverRestricted_AndABookEntry..._T350KK2` (`IsDeliverable`, `AddressBookId` hatası) |
| 3 | Tek teslimat yolu kargo | ✓ | `SingleDeliveryWayTests` (adlar + EF tablo/sütun taraması, ihlal örnekleri) |
| 4 | Tek parça, tek takip numarası, en uzun süre | ✓ | `SingleDeliveryWayTests.TheOrder_CarriesExactlyOneTrackingNumberColumn_*`, `TheOrdersCarrierColumns_*`; `ShippingPromiseTests.Effective` |
| 5 | Söz = kargoya verme süresi, iş günü, yalnız fiziksel; vitrin metni; P-5 çiti, çit üstü sebepli ret | ✓ | `ShippingPromiseTests` (Effective null, Text, CheckShippingPeriod 0..10); `CatalogCoreTablesTests.ShippingPeriodOutsideTheFence_IsRefused` (bekçi + DB check); `FirmSettingsTests` P-5 çiti. Panel ekranı reddi D-496 |
| 6 | Ürün firmayı ezer, varyant devralır; sözü en uzun süre, onayda donar | ✓ | `DiscountAndReferenceTests.AVariant_TakesItsProductsShippingPeriod..._T350KK6`; `ShipmentTransitionsTests.TheShippingPeriodOfAnOrder_IsWrittenOnce_*`; onay anında çağrı D-494 (T348) |
| 7 | Süre ödeme onayı anından (Ö1) başlar | ✓ | `ShippingPromiseTests.ThePromiseEnd_*`, `AWeekendApproval_*`; `ShipmentTransitionsTests ..._T350KK7`. Kart ve havale yolu D-495 (T325) |
| 8 | Bilgisiz kargoya verildi işareti reddedilir; müşteri görür | ✓ | `ShipmentTransitionsTests.S5_WithInvalidCarrierData_IsRefused_KK7`, `S5_WithoutTheConfirmation` (T316); `TheShippedMark_*` (T350 müşteri görünümü) |
| 9 | Liste, "Diğer" adla, bağlantı yalnız listede | ✓ | `CarrierCatalogTests` (T310); `ShipmentTransitionsTests.TheShippedMark_ShowsAListedCompany...` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1–KK9 | ✓ | yukarıdaki tablodaki testler (KK numarası = kriter numarası) |
| `09 §25` modül sınırı, arayüz yönteminde `SaveChanges` yok | ✓ | mimari testler Unit 4562/4562 içinde; yeni yöntemler okuma ya da izlenen değişiklik yapar |
| Sık FAIL (1) tarama/bekçi istisnası geniş | ✓ | tarama istisnası genişletilmedi; `ExecuteDelete` yerine izlenen `Remove/Add` |
| Sık FAIL (2) tarama çağrı biçimi kaçırır | ✓ | `SingleDeliveryWayTests` ad taramasının yanında EF model (tablo/sütun) taraması ve ihlal örnekleri var |
| Sık FAIL (3) DB default / FK dizini | ✓ | yeni tablo yok (teslimat illeri mevcut `provinces` sütunu ile); `DeliveryProvinces` şeması T107'den |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | sayı/etiket değişmedi; durum satırı + changelog parçası + backlog parçası yazıldı |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | `09` değişmedi; şablon belirteç kümesi dokunulmadı (D-493 sahip kararı) |
| Hedefi T350 olan backlog (D-436) | ✓ | D-436 kapandı; D-221, D-309 yeniden ileri atıldı |
| Güvenlik taraması | ✓ | gizli bilgi yok, yeni bağımlılık yok, SQL parametreli (EF), yeni uç yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --no-build -c Release` (UnitTests) | 4562/4562 |
| Integration | ✓ | `dotnet test --no-build -c Release` (IntegrationTests, postgres, `shopfolio-t350-pg`) | 2552/2552 |
| Biçim | ✓ | `dotnet format --verify-no-changes` | temiz |

## ⚠ Kolay kararlar

- Şablon değişmedi (belirteç kümesi `09 §9.3` m.2 kapalı); il listesi değişince sürüm artmaz: D-493 (UC-169 "sürümü artar", sahip kararı).
- Kısıt satırında eşitlikte "yapılan iller" listelenir (K-773 belirsiz); boş seçim kabul (K-817/K-828).
- Defter kaydı hatası alanı `AddressBookId`, elle yazılan adres `ShippingProvinceId`.
- `Order.ShippingPeriod` private set; `FreezeShippingPromise` bir kez (null = fiziksel kalem yok; ikinci çağrı hata).
- `DeliveryProvinces` kayıtları izlenen Remove/Add ile yazılır (ConcurrencySourceScan istisnası genişletilmedi).
- `ProductDefinitionGuard` kargoya verme süresini de denetler; eski `CatalogCoreTablesTests` testi güncellendi.
- `GetShippingViewAsync` `ShippingViewReader`'ı `OrderingModule` içinde `new` ile kurar (DI kaydı yok).
- D-309: T350 sınır koymadı, hedef T386; D-221: hedef T349.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |
