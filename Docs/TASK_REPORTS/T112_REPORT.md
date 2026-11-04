# T112 — Ödeme ve geri ödeme kayıtları

**Faz:** F1 | **Durum:** ⏳ Yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/Payment/` altında dört entity ve yapılandırması (`11` aday tablosu: "Ödeme"; `05 §3.3.4`): `PaymentRecord` (`06 §4.32`), `Refund` (§4.33), `RefundRecord` (§4.34), `InboundNotificationRecord` (§4.35). Özellik adları alan tablolarıyla birebir; hiçbiri kendi sürümünü taşımaz.
- Veritabanı varsayılanları `06` "Varsayılan" sütunuyla: `refunds.is_shipping_fee_included` `false`; `refund_records.state` `'Sending'`, `refund_records.is_marked_failed_by_admin` `false`. Öteki sütunlarda varsayılan yok.
- Tutar sütunları `numeric(12,2)` (`AsMoney()`; `06 §2.5.1`): `refunds.amount`, `coupon_share`, `vat_amount`, `inbound_notification_records.amount`.
- Kontrol kısıtları (`06 §7.3`):
  - `payment_records.transaction_or_transition` — işlem kimliği ile geçiş kaydından en az biri.
  - `refunds`: `links_by_type` (türe göre bağlar — `06 §4.33` tablosu; `DefectClaimId` ve `ReturnId`'nin boşluğuna izin verir, türün saymadığı bağlar boş), `not_returned_closure_only_amount_based`, `not_returned_closure_with_item`, `shipping_fee_only_on_item_records` (iptal, çıkarma, fesih, cayma), `amount_above_zero`, `quantity_above_zero`, `coupon_share_not_negative`, `vat_amount_not_negative`, `system_without_admin`, `sent_or_voided`; `actor_kind` yalnız `System`/`Admin` (izinli değer alt kümesi).
  - `refund_records`: `outcome_at_with_outcome` (sonuç anı başarılı/gerçekleşmedi iken ve yalnız o zaman), `admin_mark_is_failed`, `late_success_only_when_marked`.
  - `inbound_notification_records`: `notification_or_transaction`, `amount_not_negative`, `reason_only_when_processed`.
- Dizinler (`06 §7.1`, §7.2, §7.5): her yabancı anahtarın dizini (16/16); filtreli tekil dizinler — `payment_records.provider_transaction_id` (dolu satırlar), `refunds.payment_record_id` (dolu satırlar), `refunds.order_id` (kargo ücretli ve geçersizleşmemiş satırlar), `refund_records.refund_id` (açık istek: `Sending`/`OutcomePending`; başarılı istek: `Succeeded`), `inbound_notification_records.provider_notification_id` (dolu satırlar) ve (`provider_transaction_id`, `event_type`, `outcome`) (bildirim kimliği boş satırlar); `refunds.order_id` ve `refund_records.refund_id` düz dizinleri ayrıca; `inbound_notification_records.created_at`.
- Bağlar (`06 §7.2`): geri ödemenin ayıp talebi ve yönetici bağı boşaltılır (`SetNull`); öteki bütün bağlar kısıtlayıcı (`NoAction`). Modüller arası bağlar `const string` + `HasOne` kalıbıyla (`09 §10.1` m.1).
- `PaymentRecordsGuard`: üç kalıcı kaydın (`PaymentRecord`, `Refund`, `RefundRecord`) `SaveChanges` ile silinmesini SQL gitmeden reddeder; `InboundNotificationRecord` süreyle (imhanın açık silmesiyle) silindiği için kapsam dışı. Yerinde güncelleme serbest.
- Tek göç `20261104082807_CreatePaymentAndRefundTables` (`Down` yok; BOM'lu LF): dört tablo. `AppDbContext`'e dört `DbSet` ve bekçi.
- `EnumDatabaseTests` enum sütun sayısı 39 → 44 (`refunds.refund_type`, `payment_method`, `actor_kind`; `refund_records.state`; `inbound_notification_records.unprocessed_reason`).
- T111 raporunda "Post-merge run'lar" dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Payment/{PaymentRecord,Refund,RefundRecord,InboundNotificationRecord,PaymentRecordsGuard}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{PaymentRecordTablesTests,EnumDatabaseTests}.cs`.
- `Docs/DEFERRED_BACKLOG.md` (D-51 eki, D-55 eki), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T111_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

Öz denetim (PR öncesi, sahip 2026-11-04): her kriter ve kontrol listesi maddesi diff'te göç satırıyla ve testle tek tek karşılaştırıldı; `06 §4.32`…§4.35'in alan, Varsayılan, Kısıtlar, İndeksler ve Silme davranışı satırları ile §7.1, §7.2, §7.3, §7.5'in dört tabloya ait satırları göçe karşı okundu. 16 yabancı anahtarın 16'sı dizinli; varsayılanı olan üç alan veritabanında varsayılanlı; varlıklar `Payment` modülünde (`05 §3.3.4`, `11` aday tablosu "Ödeme"); bekçi yalnız üç kalıcı kaydın silinmesini reddeder.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dört entity alan tabloları ve kontrol kısıtlarıyla tek göçle | ✓ | `Migration_MakesTheFourTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `ColumnDefaults_AreTheDefaultColumnOfTheFieldTables`, `ARowWrittenWithoutTheDefaultedColumns_GetsTheDefaults`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable`, `CheckConstraints_AreTheDocumentedOnes`, `EnumColumns_AllowTheDocumentedValues` |
| 2 | `PaymentRecord` işlem kimliği dolu satırlarda tekil; işlem kimliği ile geçiş kaydından en az biri | ✓ | `SecondPaymentWithTheSameTransactionId_IsRefused`, `PaymentsWithoutATransactionId_AreNotUniqueByIt`, `PaymentWithoutATransactionIdAndATransition_IsRefused`, `CardPaymentWithItsTransition_IsAccepted` |
| 3 | `Refund` türe göre bağlar; kapanış bağı yalnız tutar temelli; kargo ücreti yalnız dört türde ve siparişte geçersizleşmemiş en çok bir satırda; ödeme kaydı bağı tekil; gönderim ile geçersizleşmeden en çok biri | ✓ | `RefundOfEachType_WithTheLinksOfItsType_IsAccepted` (7 tür), `RefundWithTheOptionalLinksEmpty_IsAccepted`, `RefundWithoutTheRecordLinkOfItsType_IsRefused` (7), `RefundOfAnItemRecordType_WithoutItsItemOrUnits_IsRefused` (5), `RefundWithALinkItsTypeDoesNotName_IsRefused`, `ClosureLinkOnATypeOtherThanAmountBased_IsRefused` (5), `ClosureLinkOnTheLatePayment_IsRefused`, `AmountBasedRefundOfAClosure_WithoutTheItem_IsRefused`, `ShippingFee_OnACancellationRemovalTerminationOrWithdrawal_IsAccepted` (4), `ShippingFee_OnARescissionAmountBasedOrLatePayment_IsRefused` (3), `SecondShippingFeeRefundOfTheOrder_IsRefused`, `ShippingFeeRefund_AfterTheFirstIsVoided_OrOnAnotherOrder_IsAccepted`, `SecondRefundOfTheSamePaymentRecord_IsRefused`, `RefundBothSentAndVoided_IsRefused`, `RefundAmounts_OutsideTheirRules_AreRefused`, `SystemRefund_WithAnAdministratorField_IsRefused`, `RefundByACustomer_IsRefused` |
| 4 | `RefundRecord` sonuç anı başarılı/gerçekleşmedi iken ve yalnız o zaman; firma işareti → gerçekleşmedi; en çok bir açık ve bir başarılı istek | ✓ | `OpenRefundRecord_WithAnOutcomeMoment_IsRefused` (2), `RefundRecordWithAnOutcome_WithoutTheOutcomeMoment_IsRefused` (2), `AdminMark_OnARecordThatIsNotFailed_IsRefused`, `LateSuccess_OnARecordWithoutTheAdminMark_IsRefused`, `MarkedRecordWithALateSuccess_IsAccepted`, `SecondOpenRequestOfTheRefund_IsRefused` (2), `SecondSuccessfulRequestOfTheRefund_IsRefused`, `FailedRequests_AndOneOpenAndOneSuccessfulPerRefund_AreAccepted` |
| 5 | `InboundNotificationRecord` bildirim ya da işlem kimliğinden en az biri; iki filtreli tekil dizin tekrarı engeller | ✓ | `NotificationWithoutAnyId_IsRefused`, `SameNotificationId_IsRefused`, `SameNotificationWithoutAnId_IsRefused`, `OtherEventsOfTheSameTransaction_AndNotificationsWithIds_AreAccepted`, `NotificationAmount_BelowZero_IsRefused_AndZeroOrEmptyIsAccepted`, `UnprocessedReason_WithoutTheProcessingMoment_IsRefused`, `ProcessedNotification_LinkedToItsOrderAndRefundRecord_IsAccepted` |
| 6 | Tutar `numeric(12,2)`; bildirim süreyle silinir, ötekiler silinmez; ayıp talebi ve yönetici bağı boşaltılır, öteki bağlar kısıtlayıcı | ✓ | `MoneyColumns_AreNumeric12And2`, `RefundAmounts_AtTheLimitsOfTheRules_AreAccepted_AndKeepTheirKurus`, `RefundAmount_AboveTheMoneyType_IsRefused`, `DeletingAPermanentRecordThroughTheContext_IsRefused_AndTheRowStays` (3), `ChangingARecordInPlace_AndDeletingANotification_AreAccepted`, `DeletingTheDefectClaim_KeepsTheRefund_AndEmptiesItsLink`, `DeletingTheAdmin_KeepsTheRefund_EmptiesItsLinkAndKeepsTheName`, `DeletingALinkedPrincipal_IsRefusedByTheDatabase` (11 bağ), `DeletingThePaymentRecordsPrincipals_IsRefusedByTheDatabase`, `DeletingTheWithdrawalOfARefund_IsRefusedByTheDatabase` |
| 7 | Göç tek konu; `Down` yok | ✓ | `Migration_HasNoDownMethod`; `has-pending-model-changes` → "No changes"; göçte `GRANT`/`REVOKE`/`now()`/`Sql(` yok |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 göç boş veritabanına, dört tablo | ✓ | `Migration_MakesTheFourTables` (test veritabanı bütün göçlerle kurulur) |
| KK2 aynı işlem kimlikli ikinci ödeme kaydı | ✓ | `SecondPaymentWithTheSameTransactionId_IsRefused` (`ix_payment_records_provider_transaction_id`) |
| KK3 tutar temelsiz türde kapanış bağı; ikinci kargo ücretli geri ödeme; kargo ücretli tutar temelli; aynı ödeme kaydına ikinci geri ödeme; gönderim + geçersizleşme | ✓ | `ClosureLinkOnATypeOtherThanAmountBased_IsRefused`, `SecondShippingFeeRefundOfTheOrder_IsRefused` (`ix_refunds_order_id1`), `ShippingFee_OnARescissionAmountBasedOrLatePayment_IsRefused`, `SecondRefundOfTheSamePaymentRecord_IsRefused`, `RefundBothSentAndVoided_IsRefused` |
| KK4 bekleyen hâlde sonuç anı dolu; başarılı hâlde boş; ikinci açık istek | ✓ | `OpenRefundRecord_WithAnOutcomeMoment_IsRefused`, `RefundRecordWithAnOutcome_WithoutTheOutcomeMoment_IsRefused`, `SecondOpenRequestOfTheRefund_IsRefused` (`ix_refund_records_refund_id1`) |
| KK5 kimliksiz bildirim; aynı bildirim ikinci kez | ✓ | `NotificationWithoutAnyId_IsRefused`, `SameNotificationId_IsRefused`, `SameNotificationWithoutAnId_IsRefused` |
| KK6 tutar türü tabloyla eşit; ayıp talebi silinince geri ödemenin bağı boşalır | ✓ | `MoneyColumns_AreNumeric12And2`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `DeletingTheDefectClaim_KeepsTheRefund_AndEmptiesItsLink` |
| KK7 kaynak taraması | ✓ | Unit'teki mimari/kaynak taramaları yeşil (245/245); diff'te `Guid.NewGuid`, `CreateVersion7`, `Restrict`/`Client…`/`Cascade`, `HasConversion<int>`, `DateTime.Now`, `UtcNow` yok |
| `09 §25` | ✓ | Enum metin + izinli değer kısıtı (`§10.1`); her FK'nin silme davranışı açık; göçte `Down`, `now()`, `suppressTransaction` yok (`§10.6`); yeni tablolar varsayılan yetkide, `DatabaseUsersTests` yeşil (`§10.8`); yeni enum yok (kilit dosyası değişmez); yeni paket yok |
| Güvenlik taraması | ✓ | Sır yok; yetki/kimlik doğrulama etkisi yok; giriş noktası yok; bildirim kaydı kişisel alan taşımaz (`06 §4.35`); yeni dış bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18, `shopfolio-t112-pg`), sınıf/ad alanıyla dört koşuya bölündü | 631: Database+FileSecrets+Web 62/62; Model 569/569 (146 + 270 + 153); T112 sınıfı 85/85 |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — dört tablo (genişletme göçü).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T112-odeme-geri-odeme-kayitlari`
- **PR:** #370
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; sağlayıcının değerleri — olay türü, sonuç, kimlik biçimi — Entegrasyon Spesifikasyonu'nundur ve bu task metin olarak saklar, izinli değer kısıtı kurmaz; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. Türe göre bağlar tek kısıtta (`ck_refunds_links_by_type`, `CASE refund_type`); kapanış bağının iki kuralı (`not_returned_closure_only_amount_based`, `not_returned_closure_with_item`) ayrı adlı kısıt — kriter 3'ün "kapanış bağı yalnız tutar temelli türde" cümlesi kendi adıyla reddedilsin diye. `links_by_type` kapanış bağına bakmaz; böylece bir ihlal tek kısıta düşer. `Quantity`, `CouponShare`, `VatAmount` `06 §4.33` tablosunun "Dolu bağlar" sütununda sayıldığı için türe göre kuralın içinde: tutar temelli ve geç ödeme türünde boş.
2. `ActorKind` geri ödemede izinli değer alt kümesiyle (`System`, `Admin`) sınırlı; `Buyer` reddedilir (`06 §4.33` "sistem ya da yöneticidir"). Yöneticinin geri ödemesinde `AdminId` zorunlu değil — yönetici kaldırılınca ve imhada boşalır.
3. Aynı sütunda iki/üç dizin: `refunds.order_id` (düz + kargo ücreti), `refund_records.refund_id` (düz + açık + başarılı). Veritabanı adları çatının numaralı adları (`ix_refunds_order_id1`, `ix_refund_records_refund_id1`, `…_id2`); model adları `OneShippingFeePerOrder`, `OneOpenRefundRequest`, `OneSuccessfulRefundRequest` → **D-55 eki**. `refunds.payment_record_id` için `06 §7.2` yalnız filtreli tekil dizini sayar; düz dizin eklenmedi (FK dizini kuralı onu kapsanmış sayar).
4. EF'in 63 bayt kırpması iki adda: `fk_refunds_late_delivery_terminations_late_delivery_terminatio` ve `ix_inbound_notification_records_provider_transaction_id_event_`; elle kısaltılmadı, test kırpılmış adları tutar → **D-51 eki**. Bir kontrol kısıtı adı ilk üretimde kırpıldığı için (`…_unprocessed_reason_only_when_p~`) kural adı `reason_only_when_processed` seçildi.
5. Kontrol kısıtı adları `06` ad vermediği için seçildi.
6. Bekçi yalnız silmeyi engeller: geri ödeme gönderilir/geçersizleşir/yolu döner, kaydın hâli ve sonucu yerinde yazılır (`06 §4.33`, §4.34). Bildirim kaydı bekçi dışında — imhanın açık silmesi (`06 §4.35`, §9.3).
7. Fabrika yöntemleri asgari: `PaymentRecord.ForCardPayment`/`ForBankTransferMark`, `Refund.BySystem`/`ByAdmin`/`ForLatePayment` (türün bağlarını çağıran yazar), `RefundRecord.Sending`, `InboundNotificationRecord.Create`. Ayarlayıcılar public (yerinde güncellenen kayıtlar; T111 ile aynı).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ödeme, geri ödeme, kart iadesi isteği ve gelen bildirimin yazım yolları ve "Uygulamada" kuralları (tutar hesabı, ödenen toplamı aşmama, satır kilidi, yolun dönüşü, imha) | — | İlgili akış task'larının işi; bu task yalnız tabloları kurar |
| 2 | Kırpılmış FK ve dizin adı | D-51 | Doküman geçişi |
| 3 | Aynı sütunda numaralı dizin adları | D-55 | Doküman geçişi |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks`.
- **Ana dal CI startup (Adım 0):** açılışta T111 merge run'ları sürüyordu; son üç tamamlanmış run success — `41794927676` (CI), `41794927593` (Publish), `41786270854`; PR öncesi T111 merge run'ları da success — `41798998317` (CI), `41798998313` (Publish).
- **Dış varsayım:** yok (yukarıda).
- **Repo memory (bitiş kapısı 8):** güncellendi.
