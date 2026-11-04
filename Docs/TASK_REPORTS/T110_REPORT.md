# T110 — Ayırma ve düzeltme geçmişleri

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/Ordering/` altında üç entity ve yapılandırması: `StockReservation` (`06 §4.14`), `OrderEmailHistory` (§4.61), `OrderAddressHistory` (§4.62). Özellik adları alan tablolarıyla birebir; üçünde de "Varsayılan" hücresi dolu değildir, veritabanı varsayılanı yoktur. Modüller arası bağlar (`Variant`, `Coupon`, `Admin`, `Province`) `HasOne("<tam ad>", navigationName: null)` + `const string` kalıbıyla (`09 §10.1` m.1).
- Kontrol kısıtları `06 §7.3` satırlarıyla: ayırmada `VariantId` ile `CouponId`'den tam biri dolu (`exactly_one_of_variant_and_coupon`) ve adet sıfırdan büyük, kupon satırında 1 (`quantity_above_zero_one_if_coupon`); adres geçmişinde teslimat adresinde iki telefon dolu, fatura adresinde ikisi boş (`phones_by_address_type`) ve iki telefon on rakam (`old_phone_ten_digits`, `new_phone_ten_digits`; `06 §2.5.1`); `AddressType` sütununun izinli değer kısıtı çatıdan gelir.
- Dizinler `06 §7.1`, `§7.5`: ayırmada iki filtreli tekil dizin — (`OrderId`, `VariantId`) `variant_id IS NOT NULL` ve `OrderId` `coupon_id IS NOT NULL` (ikisi de `OrderId` ile başlar, ayrı `OrderId` dizini yok) — ile `VariantId` ve `CouponId` dizinleri; e-posta geçmişinde (`OrderId`, `CreatedAt`) ve `AdminId`; adres geçmişinde (`OrderId`, `AddressType`, `CreatedAt`), `OldProvinceId`, `NewProvinceId`, `AdminId`.
- Bağlar `06 §7.2`: ayırmanın varyant bağı basamaklı; sipariş ve kupon bağı kısıtlayıcı. İki geçmişte sipariş bağı, adres geçmişinde ayrıca iki il bağı kısıtlayıcı; iki geçmişte yönetici bağı boşaltılır (`AdminName` değer olarak kalır).
- `OrderingRecordsGuard` genişletildi: `OrderEmailHistory` ve `OrderAddressHistory` satırının `SaveChanges` ile değiştirilmesi SQL gitmeden reddedilir; tek izinli değişiklik `AdminId`'nin null'a çekilmesidir (veritabanının yönetici kaldırılınca yaptığı boşaltmanın uygulama karşılığı). Silme açıktır: ayırma olayla, geçmişler imha işinin açık silmesiyle silinir (`06 §4.14`, §4.61, §4.62), bu yüzden bekçi silmeyi engellemez.
- Tek göç `CreateStockReservationAndHistoryTables` (`Down` yok; EF'in ürettiği `Down` silindi, dosyalar BOM'lu LF): üç tablo. `AppDbContext`'e üç `DbSet`.
- `ModelRuleTests.RegisteredCascades` (`stock_reservations -> variants`), `EnumDatabaseTests` enum sütun sayısı (33 → 34) güncellendi.
- **Backlog D-54 (T110 payı):** `StockReservation.CouponId` bağı eklendi; bağlı kuponun ham `DELETE`'i `fk_stock_reservations_coupons_coupon_id` ile reddedilir (`DeletingACouponWithAReservation_IsRefusedByTheDatabase`); `EveryLinkToTheCoupons_IsRestrictive` kümeye `fk_stock_reservations_coupons_coupon_id=a`'yı da ekledi. D-54 ✅ (T109 payı daha önce kapanmıştı).
- Önceki task raporlarının merge kaydı dolduruldu: T108 ve T109 raporlarında "Post-merge run'lar".

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/{StockReservation,OrderEmailHistory,OrderAddressHistory,OrderingRecordsGuard}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{StockReservationAndHistoryTablesTests,ModelRuleTests,EnumDatabaseTests,PriceCouponDigitalFileTablesTests}.cs`.
- `Docs/DEFERRED_BACKLOG.md` (D-54), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/{T108,T109}_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç entity alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheThreeTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `ColumnDefaults_AreTheDefaultColumnOfTheFieldTables`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable`, `CheckConstraints_AreTheDocumentedOnes` |
| 2 | Ayırma varyant ile kuponun tam birine bağlı; adet > 0, kuponda 1 | ✓ | `ReservationWithNeitherAVariantNorACoupon_IsRefused_AndOneWithBoth_IsRefused`, `VariantReservationWithoutAPositiveQuantity_IsRefused` (0, -1), `CouponReservationOfTwo_IsRefused_AndVariantReservationOfTwo_IsAccepted` |
| 3 | (`OrderId`, `VariantId`) varyant satırlarında, `OrderId` kupon satırlarında tekil | ✓ | `SecondReservationOfTheSameVariantInTheOrder_IsRefused_ButAnotherOrderOrVariantIsAccepted`, `SecondCouponReservationInTheOrder_IsRefused_ButAnotherOrderIsAccepted`, `OrderWithAVariantRowAndACouponRow_IsAccepted` |
| 4 | Ayırma varyantla basamaklı; sipariş ve kupon bağı kısıtlayıcı; geçmişlerde sipariş (ve adreste iki il) kısıtlayıcı, yönetici boşaltılır | ✓ | `DeletingTheVariant_TakesItsReservations_AndKeepsTheOthers`, `DeletingACouponWithAReservation_IsRefusedByTheDatabase`, `DeletingAnOrderWithARecord_IsRefusedByTheDatabase`, `DeletingTheAdmin_KeepsTheHistoryRows_EmptiesTheirLinkAndKeepsTheName`, `DeletingAProvinceOfTheAddressHistory_IsRefusedByTheDatabase_ForBothLinks`, `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable` |
| 5 | Adres geçmişi: teslimatta iki telefon dolu, faturada boş; telefonlar on rakam | ✓ | `ShippingAddressRow_NeedsBothPhones_AndBillingAddressRow_NeedsNone`, `ShippingAddressRow_WithAPhoneThatIsNotTenDigits_IsRefused` (9 hane, harfli, boşluklu), `AddressTypeOutsideTheEnum_IsRefused` |
| 6 | İki geçmiş uygulama düzeyinde yalnız eklenir | ✓ | `ChangingAHistoryRowThroughTheContext_IsRefused_AndTheRowStays`, `EmptyingTheAdminLinkOfAHistoryRowThroughTheContext_IsAccepted`, `TheDisposalsExplicitDeleteOfTheHistoryAndReservationRows_IsAccepted`, `SeveralCorrectionsOfTheSameOrderAndType_AreAccepted` |
| 7 | Göç tek konu; `Down` yok | ✓ | `Migration_HasNoDownMethod`; `dotnet ef migrations has-pending-model-changes` → "No changes"; göç listesinde tek yeni göç |

Doğrulama kontrol listesi: KK1 kriter 1; KK2 kriter 2; KK3 kriter 3; KK4 kriter 4 (varyant silmede ayırma gider; yönetici silmede geçmiş satırı kalır, yönetici bağı boş); KK5 kriter 5; KK6 kriter 6 (bekçi testleri; ayrıca `src`'de iki geçmiş için başka güncelleme yolu yoktur — `Create` dışında ayarlayıcıyı çağıran kod yok); KK7 kaynak taraması Unit'teki mimari/kaynak taramalarıyla (yeşil).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18, `shopfolio-t110-pg`) | 478/478; T110 sınıfı 28/28 |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `stock_reservations`, `order_email_histories`, `order_address_histories` (genişletme göçü).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T110-ayirma-duzeltme-gecmisleri`
- **PR:** #367
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. `OrderingRecordsGuard` iki geçmiş satırının değişikliğine yalnız `AdminId`'nin null'a çekilmesinde izin verir (veritabanının boşaltmasının eşdeğeri); `AdminName` dahil başka her değişiklik reddedilir. Geçmişlerin `AdminName`'i imhada boşaltılmaz (`06 §4.61`, §4.62: yalnız bağ boşaltılır; satırlar imhada silinir).
2. Filtreli tekil dizinler `OrderId` ile başladığı için ayrı bir `OrderId` dizini yoktur (`06 §7.2` "iki filtreli tekil dizinin ilk sütunu"); kontrol kısıtı adları `exactly_one_of_variant_and_coupon`, `quantity_above_zero_one_if_coupon`, `phones_by_address_type` olarak seçildi (`06 §7.3` ad vermiyor).
3. Üç entity `Ordering` modülündedir (sipariş kaydının parçaları); `Variant` ve `Coupon` ile `Admin` ve `Province` bağları ad ile kurulur.
4. `OrderEmailHistory`/`OrderAddressHistory` için `Create` alan başına parametre alır; adres geçmişinde eski ve yeni değer beşer alanlı ayrı demetlerdir (yazımı yapan task'ın çağrısı sade kalsın diye).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Geçmişlerin yazım yolları (sipariş e-posta/adres düzeltmesi, onay ve serbest bırakma) | — | İlgili akış task'larının işi; bu task yalnız tabloları kurar |

## Notlar

- **Working tree (Adım -1):** temiz (yeni worktree); `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** son üç run success — `41782286498` (Publish), `41782286493` (CI) ve `41777013421` (Publish).
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Diğer:** Port 55410 Windows'ta ayrılmış (dışlanmış port aralığı), yerel konteyner 35432'ye bağlandı. EF göç dosyalarını CRLF üretti, BOM'lu LF'ye çevrildi ve `Down` silindi.
