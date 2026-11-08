# T350 — Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-08

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
| Doğrulama durumu | ✓ PASS (tur 1) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — Doğrulama Sonucu — T350 Teslim — tur 1
**Tarih:** 2026-11-08   **Branch:** task/T350-teslim   **Commit:** `bfdc36c` (doğrulanan head; `9c97e24` + `origin/main` `637885c` birleştirmesi), PR #460

#### Verdict: ✓ PASS

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42464077677, 42464077701 (T335), 42463181128 (T318) — üçü success |
| Memory drift | — | Adım 0b kaldırıldı (2026-11-08); aranmadı |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Teslimat illeri seçilir (varsayılan Türkiye); kapalı ilde fiziksel kalemli sipariş sebebiyle reddedilir; dijital/hizmet girmez; il düzeyi | ✓ | `DeliveryProvinces` (kilit → koşullu sürüm → izlenen satırlar → plaka kodlu iz satırı); varsayılan 81 il `InsertStartupData` göçünde; `AddressBooks.ResolveAsync` yalnız teslimat adresi (fiziksel sepet) için `DeliveryProvinceNotServed` + il adı (`Detail`); metin `04 5.13.23` ile aynı ("Bu firma Ankara iline teslimat yapmıyor."). Testler: `AddressTests …_T350KK1`, `FirmSettingsTests` 4 yeni test, `DeliveryAreaTests` 7 test — yeşil |
| 2 | Defterdeki kapalı il adresi ödemede seçilemez | ✓ | `AddressBookEntry.IsDeliverable`; defter kaydı seçimi `AddressBookId` alanıyla reddedilir; fatura ili kısıtlanmaz. `AddressTests …_T350KK2` yeşil |
| 3 | Tek teslimat yolu kargo; seçenek/mağaza teslimi/adlandırılmış seçenek yok | ✓ | `SingleDeliveryWayTests`: uygulama derlemesindeki tüm ad ve EF tablo/sütun taraması + yakalama/yanlış pozitif örnekleri — yeşil |
| 4 | Tek parça, tek takip numarası; kalemler en uzun süreyi bekler | ✓ | `SingleDeliveryWayTests` (`orders.tracking_number` tek; taşıyıcı sütunları 4 tane); `ShippingPromise.OfOrder` en uzun (`ShippingPromiseTests`) |
| 5 | Söz = kargoya verme süresi, iş günü, yalnız fiziksel; vitrin metni; çit (P-5/P-6) ve çit üstü sebepli ret | ✓ | `ShippingPromise.Effective` dijital/hizmette null; `Text` "N iş günü içinde kargoya verilir" (`02 §3.20.4`, `04 5.4.5`); `PromiseEnds` → `PeriodCounting` iş günü; ürün süresi `ProductDefinitionRules.CheckShippingPeriod` (P-6 = P-5 çiti 0–10) + kayıt bekçisi + DB check (`CatalogCoreTablesTests`); firma varsayılanı çiti `FirmSettingsTests` (11 → AboveUpper). Ürün formu ekranı reddi D-496 (T525) — ekran bu task'ın değil |
| 6 | Ürün varsayılanı ezer, varyant devralır; sözü en uzun süre, onayda donar | ✓ | `ShippingPeriodReader` (varyant → ürünün süresi/tipi; `DiscountAndReferenceTests …_T350KK6`); `Effective` (ürün ?? firma, 0 geçerli); `Order.FreezeShippingPromise` private set + bir kez (`ShipmentTransitionsTests …_T350KK6`). Onay anındaki çağrı siparişin doğduğu T348'de: D-494 |
| 7 | Süre ödeme onayı anında (Ö1) başlar | ✓ | `ShippingViewReader` son düzeltilmemiş Ö1'den sayar; `ShipmentTransitionsTests …_T350KK7` (2 test), `ShippingPromiseTests` (hafta sonu onayı, onay günü sayılmaz). Kart/havale yolu kriter metnince T325: D-495 |
| 8 | Bilgisiz kargoya verildi işareti konmaz; müşteri sipariş sayfasında görür | ✓ | `S5_WithInvalidCarrierData_IsRefused_AndWritesNothing_KK7` (7 bilgisiz/yanlış bileşim, T316) yeşil; müşteri görünümü `GetShippingViewAsync` → `…_T350KK8KK9` yeşil |
| 9 | Liste, "Diğer" adla, "Takip et" yalnız listedekinde | ✓ | `CarrierCatalog` (T310) üzerinden: listedeki şirkette bağlantı, "Diğer"de ad+numara, kendi aracında yalnız beyan — `…_T350KK8KK9` |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-A046 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (uç yanıtı ödeme adımında — T509) | — |
| VAL-A060 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (uç ürün sayfasında — T511) | — |
| VAL-A061 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (uç müşteri sipariş sayfasında) | — |
| VAL-A062 | DÜŞÜK | yok | BEKLEMEDE | BEKLEMEDE (tarayıcı gözlemi; sayfa sonra) | — |
| VAL-A063 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (uç müşteri sipariş sayfasında) | — |
| VAL-C040 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (uç panel sipariş ekranında — T527) | — |
| VAL-C041 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (aynı — T527) | — |
| VAL-A293 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (uç panel ayar ekranında — T536) | — |
| VAL-A294 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (uç panel ayar ekranında — T536) | — |

Satırların kanıt türü uç yanıtı / tarayıcı gözlemidir; T350 uç ve ekran yazmaz (T344 doğrulamasındaki uygulama). KP-13/KP-46/KP-59'un öteki satırları T349, T316/T318/T319, T331/T353'ündür. `12` değişmedi; `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

#### Doğrulama Kontrol Listesi
- [x] KK1–KK9 (yukarıdaki kanıtlarla)
- [x] `09 §25` ilgili satırlar: modül sınırı (yalnız `Contracts`), arayüz yönteminde `SaveChanges` yok, bölüm sürümü + satır kilidi + koşullu güncelleme, iz satırı aynı işlemde ve kişisel veri yok (plaka kodu), zaman `TimeProvider`'dan, yeni paket/göç/değer yok (`DeliveryProvinceNotServed` mevcut kod)
- [x] Güvenlik taraması

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` / `--locked-mode` | çıkış 0 |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 | üçü çıkış 0 |
| Unit (+Consistency, Architecture) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 4649/4649 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (dokunulan sınıflar) | ✓ | Address, FirmSettings, CatalogCoreTables, DiscountAndReference, Membership, ShipmentTransitions | 244/244 |
| Integration (komşu, gerçek modül kaydı) | ✓ | ProductPublishGate, ProductTransitions, LegalText, SalesGate, CompanyIdentity, FirmSettingDefaults, Branding, PriceAndVat, StockAndCapacity | 213/213 |
| Dal CI | ✓ (not) | `gh pr checks 460`; `gh run list --branch task/T350-teslim` | `9c97e24`: 42461958983 success. `bfdc36c` (yalnız main birleştirmesi): 42464103500 failure — yalnız "7. Paketleme", restic indirmesinde `HTTP 504 Gateway Time-out` (dış ağ, geçici); Lint/Build/Unit/Integration/Contract/Migration yeşil. Bu turun kayıt push'u CI'ı yeniden koşturur; merge öncesi yeşil görülmeli |

Konteyner `shopfolio-t350-val-pg` (port 55611) koşu sonunda `docker rm -f -v` ile silindi; `git status` temiz.

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz (yeni uç yok; modül yöntemleri çağıranın yetkili işleminde)
- Input validation: Temiz (il kimliği 81'lik kapalı listeye karşı; ürün süresi çitle, bekçi + DB check)
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
Yok.

#### Bloke-etmeyen Notlar (K-notları)
Yok. (Yapımın açtığı D-493–D-496 ile D-221/D-309 güncellemeleri yerinde ve hedefli; D-436 kapanışı kodla eşleşiyor.)

#### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu. Öz denetim tablosu var ve her maddesi kanıtlı. Sayı farkı (Unit 4562 → 4649) main birleştirmesinden.

#### Merge ve Post-merge
- Merge yapılmadı (yardımcının işi, 17–18. adımlar).
