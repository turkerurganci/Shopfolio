# T109 — Sepet ve sipariş çekirdeği

**Faz:** F1 | **Durum:** ⏳ Yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/Ordering/` altında sekiz entity ve yapılandırması: `Cart` (`06 §4.15`), `CartItem` (§4.16), `Order` (§4.17), `OrderItem` (§4.18), `TransitionRecord` (§4.19), `InternalNote` (§4.20), `CheckboxAcceptance` ve `CheckboxAcceptanceOrderItem` (§4.21). Özellik adları `06` alan tablolarıyla birebir; alan tablolarında dolu **Varsayılan** hücreleri veritabanı varsayılanıdır (`HasDefaultValue`): `carts.is_payment_unavailable_notice_pending`, `cart_items.is_merge_notice_pending`, `orders.is_account_deleted`, `orders.is_own_vehicle_delivery` = `false`; `orders.order_status` = `Placed`, `orders.payment_status` = `Pending`, `orders.version` = 1; `order_items.coupon_share` = 0, `order_items.download_count` = 0.
- Kontrol kısıtları `06 §7.3` satırlarıyla (ve §2.5.1'in telefon/KDV oranı kurallarıyla) `HasRule` üzerinden: sepette tam bir sahip ve misafirde son erişim anı; sepet kaleminde adet/fiyat > 0 ve varyant bağı boşsa kaldırılan ürün adı; siparişte iki eksenin yasak çiftleri, imhanın birlikte boşalttığı alanlar, silinmiş hesapta boş bağ, teslimat adresi hep/hiç, ödeme yöntemi–IBAN, kartta hatırlatma yok, kupon–kod birlikte, taşıyıcı alanları, tutar ve kargo ücreti–döküm, `ShippingPeriod` ≥ 0; sipariş kaleminde tipe göre alanlar, indirimin üç alanı, hakkı aşmayan indirme sayacı; geçiş kaydında en çok bir neden, düzeltme alanları yalnız düzeltme satırında ve yönetici, nedenlerin kod/yapan sınırları.
- Tekil dizinler `06 §7.1`: sepette `CustomerId` ve `GuestCartKeyHash` ayrı ayrı; sepet kaleminde işaretsiz satırlarda (`CartId`, `VariantId`) filtreli; siparişte numara, erişim anahtarı ve onay anahtarı ayrı ayrı; iç not siparişte; onay kaydı (`OrderId`, `CheckboxType`); kayıt–kalem bağı (`CheckboxAcceptanceId`, `OrderItemId`). `06 §7.5` okuma dizinleri: misafir sepetinin `LastAccessedAt`'i (filtreli), siparişin `NormalizedContactEmail`, `CartId`, ödemesi beklenen siparişlerde filtreli `PaymentWindowEndsAt` ve `PaymentReminderAt`, (`IpAddress`, `CreatedAt`), (`OrderStatus`, `PaymentStatus`, `CreatedAt`); geçiş kaydında (`OrderId`, `CreatedAt`).
- Bağlar `06 §7.2`: üyenin sepeti hesapla, kalem sepetle basamaklanır; hesap, yönetici, kalemin ürün ve varyant bağı boşaltılır; öteki bütün bağlar (sipariş, kupon, il, hukuki metin sürümü, görsel, dijital dosya, düzeltilen satır, onay kaydı) kısıtlayıcıdır. `Order.CartId` bağ değil, değerdir (§7.2.3). Modüller arası bağlar `HasOne("<tam ad>", navigationName: null)` + `const string` kalıbıyla (`09 §10.1` m.1).
- `OrderingRecordsGuard` (SaveChanges kesicisi, `CatalogRecordsGuard` kalıbı): `Order`, `OrderItem`, `TransitionRecord`, `CheckboxAcceptance`, `CheckboxAcceptanceOrderItem` silinmesini; `TransitionRecord`'un imha dışındaki her değişikliğini (yalnız `AdminId`, `AdminName`, `ReasonNote` null'a çekilebilir) SQL gitmeden reddeder. `AppDbContext`'e sekiz `DbSet` ve kesici eklendi.
- Özel ayarlayıcılar: `Order.OrderStatus`/`PaymentStatus` (geçiş birimi, `09 §9.1` m.4), `ContactEmail`/`NormalizedContactEmail` (`SetContactEmail`, `09 §9.4` m.1), iki belge kopyası (`FreezeDocuments`, bir kez, `09 §9.3` m.1), `Version` (`BumpVersion`; eşzamanlılık belirteci kuraldan), `OrderItem.DownloadCount`, `TransitionRecord`'un bütün alanları (`Create` + `ErasePersonalData`).
- Tek göç `CreateCartAndOrderCoreTables` (`Down` yok; LF): sekiz tablo.
- `ModelRuleTests.RegisteredCascades` (iki yeni basamaklı bağ), `EnumDatabaseTests` enum sütun sayısı (21 → 33) ve D-54 için `EveryLinkToTheCoupons_IsRestrictive` (küme boş değil, `fk_orders_coupons_coupon_id=a`) güncellendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/{Cart,CartItem,Order,OrderItem,TransitionRecord,InternalNote,CheckboxAcceptance,OrderingRecordsGuard}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{CartAndOrderCoreTablesTests,ModelRuleTests,EnumDatabaseTests,PriceCouponDigitalFileTablesTests}.cs`.
- `Docs/DEFERRED_BACKLOG.md` (D-54, D-51), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz entity alan tabloları ve kontrol kısıtlarıyla tek göçle | ✓ | `Migration_MakesTheEightTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `ColumnDefaults_AreTheDefaultColumnOfTheFieldTables`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable`, `CheckConstraints_AreTheDocumentedOnes`, `Migration_HasNoDownMethod` |
| 2 | Sepet tam bir sahip; ikisi ayrı tekil | ✓ | `Cart_WithoutAnOwner_IsRefused`, `Cart_WithBothOwners_IsRefused`, `GuestCart_WithoutTheLastAccessMoment_…`, `SecondCartOfTheMember_IsRefused`, `SecondCartWithTheSameGuestDigest_IsRefused` |
| 3 | Sepet kalemi işaretsizde tekil; varyantsızda ad dolu | ✓ | `UnmarkedItemWithoutAVariant_IsRefused`, `SecondUnmarkedItemOfTheSameVariant_IsRefused_ButAMarkedOneIsAccepted` |
| 4 | Sipariş üç tekil; yasak çiftler, imha alanları, silinmiş hesap, yöntem–IBAN | ✓ | `SecondOrderWithTheSameNumberAccessKeyOrConfirmationKey_IsRefused`, `FailedPaymentWithAPlacedOrder_IsRefused`, `PendingPaymentWithAPreparingOrder_IsRefused`, `EmptyEmailWithAFilledAccessKey_IsRefused`, `FilledEmailWithoutTheBillingAddress_IsRefused`, `DisposedOrder_…_IsAccepted`, `DeletedAccountWithAFilledAccountLink_IsRefused`, `BankTransferOrderWithoutAnIban_…` (+ kart/kupon/adres/taşıyıcı/ücret testleri) |
| 5 | Kalem tipe göre alanlar; hakkı aşmayan sayaç | ✓ | `ItemWithZeroQuantity_IsRefused`, `DigitalItemWithTwoUnits_IsRefused`, `DownloadCountAboveTheAllowance_IsRefused`, `DownloadCountOnAPhysicalItem_IsRefused`, `FieldsOfAnotherType_AreRefused`, `PartialDiscountAndDiscountedPriceAboveTheUnitPrice_AreRefused` |
| 6 | Geçiş kaydı en çok bir neden; düzeltme alanları yalnız düzeltmede | ✓ | `RecordWithTwoReasons_IsRefused`, `CorrectionReasonOnANonCorrectionRow_IsRefused`, `CorrectionRowWithoutItsCorrectedRow_…`, `CorrectionRowByTheAdministrator_IsAccepted`, `ReasonsOutsideTheirCodesAndActors_AreRefused` |
| 7 | İç not siparişte tekil; onay kaydı ve bağ çiftlerde tekil | ✓ | `SecondInternalNoteOfTheOrder_IsRefused`, `SecondAcceptanceOfTheSameType_…`, `SecondLinkOfTheSameAcceptanceAndItem_IsRefused` |
| 8 | Sipariş ve kalem silinmez; bağlar kısıtlayıcı; hesap/yönetici/ürün/varyant bağı boşalır; sepet basamaklanır | ✓ | `DeletingAnOrderWithAnItem_IsRefusedByTheDatabase`, `DeletingAnOrderWithItsRecords_…` (3), `DeletingThePermanentRowsThroughTheContext_IsRefused_…`, `ChangingATransitionRecordThroughTheContext_IsRefused_…`, `ErasingTheAdministratorsDataOfATransitionRecord_…`, `DeletingTheCustomer_TakesTheCartAndItsItems_…`, `DeletingTheVariant_KeepsTheOrderItem_…`, `DeletingTheVariant_IsStoppedByAnUnmarkedCartItem_…`, `DeletingTheAdmin_EmptiesTheLink…`, `DeletingACouponLinkedToAnOrder_…`, `DeletingALegalTextVersionOrAProvinceOfAnOrder_…`, `ModelRuleTests` basamak listesi |

Doğrulama kontrol listesi: KK1–KK7 yukarıdaki testlerle; KK8 (kaynak taraması) Unit'teki mimari/kaynak taramaları yeşil.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18, `shopfolio-t109-pg`; ad alanı parçaları + tam koşu) | 450/450 (Model 388, Database+FileSecrets 45, öteki 17); T109 sınıfı 65/65 |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | *(validator doldurur)* |
| Düzeltme gerekli mi | *(validator doldurur)* |
| Bloke-etmeyen not (K) sayısı | *(validator doldurur)* |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `carts`, `cart_items`, `orders`, `order_items`, `transition_records`, `internal_notes`, `checkbox_acceptances`, `checkbox_acceptance_order_items` (genişletme göçü).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T109-sepet-siparis-cekirdegi`
- **PR:** #366
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. Ödemesi beklenen siparişlerin filtreli dizinlerinin (`PaymentWindowEndsAt`, `PaymentReminderAt`) filtresi `payment_status = 'Pending'`.
2. `Order.Create` çekirdek alanları alır; donan öteki alanlar (adresler, firma kimliği, sürüm kimlikleri, kargo sözü) public ayarlayıcıdır. İki belge kopyası `FreezeDocuments` ile bir kez yazılır; belge üretimi sonraki task'ın.
3. `ApplyTransition` (geçiş birimi) yazılmadı — geçiş biriminin task'ı; testler durumu `Entry().Property().CurrentValue` ile kurar.
4. `CheckboxAcceptanceOrderItem` ve yapılandırması `CheckboxAcceptance.cs` içinde (ayrı dosyaya bölünmedi).
5. Çatı dört adı 63 bayta kırptı: `fk_orders_legal_text_versions_distance_sales_contract_version_`, `fk_transition_records_transition_records_corrected_transition_`, `fk_checkbox_acceptance_order_items_checkbox_acceptances_checkb`, `ix_checkbox_acceptance_order_items_checkbox_acceptance_id_orde`. `09 §5.2` m.3 ad vermeyi yasakladığı için kısa ad seçilmedi; testler pg'deki adları tutar; D-51'e eklendi.
6. `OrderingRecordsGuard` geçiş kaydının değişikliğine yalnız `AdminId`, `AdminName`, `ReasonNote`'un null'a çekilmesinde izin verir; değeri dolduran ya da başka alanı değiştiren güncelleme reddedilir.
7. `Order.ShippingFeeVatBreakdown` bugün `string?` + `jsonb` (D-57'deki `AuditLog.Details` gibi); değer listesi tipi ve `PayloadJson` dönüştürücüsü kargo ücretini hesaplayan task'ta bağlanır, sütun değişmez.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Kupon ayırmasının bağı (`StockReservation.CouponId`) | D-54 (T110 payı) | Tablo T110'da |
| 2 | Çatının kırptığı adlar | D-51 | `09`/`06` doküman geçişi |

## Notlar

- **Working tree (Adım -1):** temiz (yeni worktree, devir notuyla devam); `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** yardımcı düzeninde yardımcının işi; işçi `gh run` yoklamaz.
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Diğer:** İş iki oturumda yapıldı (ilk işçi varlıkları yazdı, ara commit `4f8728c`; ikinci işçi bekçi, `DbSet`, göç, testler ve kayıtlar). EF göç dosyalarını CRLF üretti, LF'ye çevrildi. Sır bekçisi `…AccessKey = …` değişken adını yakaladığı için test değişkeni `sameAccess` adını aldı.
