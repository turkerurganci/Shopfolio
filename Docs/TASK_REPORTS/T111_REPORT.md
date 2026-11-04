# T111 — Kalem kayıtları

**Faz:** F1 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/AfterSales/` altında sekiz entity ve yapılandırması (`11` aday tablosu: "İptal, cayma ve iade"; `05 §3.3.5`): `Cancellation` (`06 §4.22`), `ItemRemoval` (§4.23), `LateDeliveryTermination` (§4.24), `Withdrawal` (§4.25), `Return` (§4.26), `NotReturnedClosure` (§4.28), `DefectClaim` (§4.30), `RefundIban` (§4.31). Özellik adları alan tablolarıyla birebir. Ortak tutar dörtlüsü ve iade adresi `ItemRecordRules.cs`'te (`ItemAmounts`, `ReturnAddress` ve kısıt yardımcıları).
- Veritabanı varsayılanları `06` "Varsayılan" sütunuyla: dört tutarlı kayıtta `coupon_share` `0`; `defect_claim_status` `'Open'`; `status_history` `'[]'::jsonb`. Öteki sütunlarda varsayılan yok.
- Kontrol kısıtları: dört tutarlı kayıtta `quantity_above_zero`, `product_amount_above_zero`, `coupon_share_within_product` (0 ≤ pay ≤ ürün tutarı), `vat_amount_not_negative`; teslim alma, kapanış ve ayıp talebinde `quantity_above_zero`. Yapanı olan üç kayıtta `ActorKind` yalnız `Buyer`/`Admin` (izinli değer kısıtı) ve `buyer_without_admin`; iptalde `reason_by_admin` (sebep tam yöneticide dolu), `reason_note_only_on_other`, `refund_iban_only_by_buyer`. `returns.at_most_one_link` (cayma, fesih, ayıp talebinden en çok biri). Cayma ve ayıp talebinde iade adresinin dört alanı birlikte (`return_address_fields_together`), telefon yalnız adresle (`return_phone_only_with_address`) ve on rakam (`return_phone_ten_digits`). IBAN `^[A-Z0-9]+$`.
- Dizinler `06` "İndeksler" satırlarıyla: her yabancı anahtarın dizini; `DefectClaim.OrderItemId` tekil; `DefectClaimStatus` `Open` satırlarda filtreli; `RefundIban.ScheduledDeletionAt` dolu satırlarda filtreli.
- Bağlar: kaleme, geçiş kaydına, cayma beyanına, fesihe, siparişe ve ile giden bağlar kısıtlayıcı (`NoAction`); yöneticiye, IBAN'a ve (teslim almada) ayıp talebine giden bağlar boşaltılır (`SetNull`). Modüller arası bağlar `const string` + `HasOne` kalıbıyla (`09 §10.1` m.1).
- `AfterSalesRecordsGuard`: altı kalıcı kaydın (`Cancellation`, `ItemRemoval`, `LateDeliveryTermination`, `Withdrawal`, `Return`, `NotReturnedClosure`) `SaveChanges` ile silinmesini SQL gitmeden reddeder. `DefectClaim` (süreyle) ve `RefundIban` (olayla) silindiği için kapsam dışı; kayıtların yerinde güncellenmesi serbest.
- Tek göç `20261104064259_CreateItemRecordTables` (`Down` yok; BOM'lu LF): sekiz tablo. `AppDbContext`'e sekiz `DbSet` ve bekçi.
- `EnumDatabaseTests` enum sütun sayısı 34 → 39 (üç `actor_kind`, `cancellation_reason`, `defect_claim_status`).
- T110 raporunda "Post-merge run'lar" dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/AfterSales/{Cancellation,ItemRemoval,LateDeliveryTermination,Withdrawal,Return,NotReturnedClosure,DefectClaim,RefundIban,ItemRecordRules,AfterSalesRecordsGuard}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{ItemRecordTablesTests,EnumDatabaseTests}.cs`.
- `Docs/DEFERRED_BACKLOG.md` (D-67, D-51 eki), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T110_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

> **Validator kanıtı (tur 1, head `6cefd1c`):** 8 kriterin ve KK1–KK8'in hepsi ✓ — tek göç `20261104064259_CreateItemRecordTables` sekiz tabloyu kurar, `Down` yok, `GRANT`/`REVOKE`/`Sql(` yok. Para sütunları `numeric(12,2)`, adet `integer`, tarih `date`, an `timestamptz`, telefon `varchar(10)` + `^[0-9]{10}$`, IBAN `text` + `^[A-Z0-9]+$` (`06 §2.5`); varsayılanlar `coupon_share` 0, `defect_claim_status` `Open`, `status_history` `'[]'`. Tutar kısıtları (`quantity>0`, `product_amount>0`, `0<=coupon_share<=product_amount`, `vat>=0`) dört tutarlı tabloda; teslim alma, kapanış ve ayıp talebinde `quantity>0`. İptalin dört kuralı, yapanlı üç kayıtta `Buyer`/`Admin` alt kümesi, `returns.at_most_one_link`, `DefectClaim.order_item_id` tekil, iade adresi dört alan birlikte ve telefon yalnız adresle. FK silme davranışları `06 §7.2` ile, dizinler `06 §7.5` ile birebir (yönetici/IBAN/ayıp talebi `SetNull`, ötekiler `NO ACTION`). `AfterSalesRecordsGuard` altı kalıcı kaydın silinmesini reddeder; `DefectClaim` ve `RefundIban` silinebilir. Yeni sekiz tablo `09 §10.8` madde 3'ün istisna listesinde değil, varsayılan yetkiyle doğru: göçte yetki deyimi yok, `PrivilegeMatrix_OnTheTablesOfTheMigratedDatabase_HasNoDeviation` ve `DatabaseUsersTests` yeşil (59/59).

Öz denetim (PR öncesi): her kriter ve kontrol listesi maddesi diff'te göç satırıyla ve testle tek tek karşılaştırıldı; `06 §4.22`…§4.26, §4.28, §4.30, §4.31'in alan, Varsayılan, Kısıtlar, İndeksler ve Silme davranışı satırları göçe karşı okundu. 26 yabancı anahtarın 26'sı dizinli; varsayılanı olan dört alan (`CouponShare` ×4, `DefectClaimStatus`, `StatusHistory`) veritabanında varsayılanlı.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz entity alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheEightTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `ColumnDefaults_AreTheDefaultColumnOfTheFieldTables`, `ARowWrittenWithoutTheDefaultedColumns_GetsTheDefaults`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable`, `CheckConstraints_AreTheDocumentedOnes` |
| 2 | Tutarlı dört kayıtta adet > 0, ürün tutarı > 0, 0 ≤ pay ≤ ürün tutarı, KDV ≥ 0; teslim alma, kapanış, ayıp talebinde adet > 0 | ✓ | `AmountRecord_WithAQuantityOfZero_IsRefused`, `…WithAProductAmountOfZero…`, `…WithACouponShareAboveTheProductAmountOrBelowZero…`, `…WithANegativeVat…`, `AmountRecord_AtTheLimitsOfTheRules_IsAccepted_AndKeepsItsKurus`, `AmountRecord_AboveTheMoneyType_IsRefused` (dört tablo için teori), `ReceiptClosureOrClaimWithAQuantityOfZero_IsRefused` |
| 3 | Yapanlı kayıtlarda yönetici alanları tür kuralına uyar; iptalde sebep yöneticide dolu, müşteride boş; açıklama yalnız "diğer"de; IBAN yalnız müşteri iptalinde | ✓ | `CustomerCancellation_WithAnAdministratorField_IsRefused`, `CancellationReason_IsFilledOnTheAdministratorsRecord_AndOnlyThere`, `ReasonNote_IsFilledOnlyOnTheOtherReason`, `RefundIban_IsLinkedOnlyToTheCustomersCancellation`, `CustomerTerminationOrWithdrawal_WithAnAdministratorField_IsRefused`, `RecordsOfBothActors_AreAccepted_AndAnEmptiedAdministratorLinkIsAccepted`, `SystemActor_IsRefusedOnTheThreeRecordsWithAnActor` |
| 4 | `Return` en çok bir bağ | ✓ | `ReceiptWithTwoOrThreeLinks_IsRefused_AndOneWithOneOrNoneIsAccepted` (`ck_returns_at_most_one_link`) |
| 5 | `DefectClaim` kalem başına tekil; iade adresi dört alan birlikte, telefon yalnız adresle ve on rakam | ✓ | `SecondClaimOfTheSameItem_IsRefused_ButAClaimOfAnotherItemIsAccepted`, `ReturnAddress_WithOneOfTheFourFieldsMissing_IsRefused`, `ReturnPhone_WithoutTheAddress_IsRefused`, `ReturnPhone_ThatIsNotTenDigits_IsRefused`, `ReturnAddress_FilledWithOrWithoutAPhone_OrEmpty_IsAccepted` (cayma ve ayıp talebi için) |
| 6 | `RefundIban` biçim kısıtı; olayla silinir | ✓ | `RefundIban_NotInCapitalsAndDigits_IsRefused`, `RefundIban_InCapitalsAndDigits_IsAccepted_AndItsCorrectionIsWrittenInPlace`, `DeletingTheIban_EmptiesTheLinksOfTheThreeRecords` |
| 7 | Kalem kayıtları silinmez; yönetici, IBAN, ayıp talebi bağı boşaltılır, öteki bağlar kısıtlayıcı; `DefectClaim` süreyle silinir | ✓ | `DeletingAPermanentRecordThroughTheContext_IsRefused_AndTheRowStays`, `ChangingARecordInPlace_IsAccepted`, `DeletingAnItemWithARecord_IsRefusedByTheDatabase`, `DeletingAnOrderWithAnIban_IsRefusedByTheDatabase`, `DeletingAWithdrawalOrATerminationOrATransitionWithARecord_IsRefusedByTheDatabase`, `DeletingAProvinceOfAReturnAddress_IsRefusedByTheDatabase`, `DeletingTheAdmin_KeepsTheRecords_EmptiesTheirLinkAndKeepsTheName`, `DeletingTheClaim_KeepsTheReceipt_AndEmptiesItsLink` |
| 8 | Göç tek konu; `Down` yok | ✓ | `Migration_HasNoDownMethod`; `has-pending-model-changes` → "No changes"; göçte `GRANT`/`REVOKE`/`now()`/`Sql(` yok |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 göç boş veritabanına, sekiz tablo | ✓ | `Migration_MakesTheEightTables` (test veritabanı bütün göçlerle kurulur) |
| KK2 payı ürün tutarını aşan kayıt, sıfır adetli teslim alma | ✓ | `AmountRecord_WithACouponShareAboveTheProductAmountOrBelowZero_IsRefused`, `ReceiptClosureOrClaimWithAQuantityOfZero_IsRefused` |
| KK3 müşteri kaydında yönetici alanı | ✓ | `CustomerCancellation_WithAnAdministratorField_IsRefused`, `CustomerTerminationOrWithdrawal_WithAnAdministratorField_IsRefused` |
| KK4 iki bağlı `Return` | ✓ | `ReceiptWithTwoOrThreeLinks_IsRefused_AndOneWithOneOrNoneIsAccepted` |
| KK5 ikinci ayıp talebi; eksik adres; adressiz telefon | ✓ | `SecondClaimOfTheSameItem_…`, `ReturnAddress_WithOneOfTheFourFieldsMissing_IsRefused`, `ReturnPhone_WithoutTheAddress_IsRefused` |
| KK6 küçük harfli IBAN | ✓ | `RefundIban_NotInCapitalsAndDigits_IsRefused` |
| KK7 kayıtlı kalemin silinmesi | ✓ | `DeletingAnItemWithARecord_IsRefusedByTheDatabase` |
| KK8 kaynak taraması | ✓ | Unit'teki mimari/kaynak taramaları yeşil (245/245); diff'te `Guid.NewGuid`, `CreateVersion7`, `Restrict`/`Client…`, `HasConversion<int>`, `DateTime.Now` yok |
| `09 §25` | ✓ | Enum metin + izinli değer kısıtı (`§10.1`); her FK'nin silme davranışı açık; göçte `Down`, `now()`, `suppressTransaction` yok (`§10.6`); yeni tablolar varsayılan yetkide, `DatabaseUsersTests` yeşil (`§10.8`); yeni enum yok (kilit dosyası değişmez); yeni paket yok |
| Güvenlik taraması | ✓ | Sır yok; yetki/kimlik doğrulama etkisi yok; giriş noktası yok; yeni dış bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18, `shopfolio-t111-pg`), ad alanıyla bölündü | 546: Model 484/484, öteki 62/62; T111 sınıfı 68/68 |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

Not: Model ad alanının ilk koşusunda iki eski test (`AdminAndAuditTablesTests.Migration_WithoutTheApplicationUserRoleSetting_…`, `SettingsTablesTests.PermanentRows_AreNeverDeletedThroughTheContext`) yük altında ~15 sn'de düştü; iki sınıf tek başına 70/70, Model ad alanı ikinci koşuda 484/484 geçti — zamanlama kaynaklı, T111 değişikliğiyle ilgisi yok.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `6cefd1c`, PR #369) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-67, K2 → D-51; ikisi dalda forward edildi) |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Kapılar:** working tree temiz; `core.hooksPath` mutlak yolla `scripts/git-hooks` (eşdeğer, hook dosyaları daldakiyle birebir); ana dal son 3 run `41794927593`, `41794927676`, `41786270854` success; `MEMORY.md`'de T111 satırı var.

**Kabul kriterleri ve testler:** kapsam tam (8 kriter, KK1–KK8, `09 §25`, güvenlik); hepsi ✓. Yerel: build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 0; Unit 245/245 (Consistency dahil); Integration 546 (T111 sınıfı 68/68, Model 484/484, öteki 62/62; PostgreSQL 18); yetki: `DatabaseUsersTests` + `AdminAndAuditTablesTests` 59/59 (göçlü veritabanında yetki matrisi sapmasız); `has-pending-model-changes` "No changes"; koşu sonrası `git status` temiz, `restore --locked-mode` 0. Dal CI `41796325905` (head `6cefd1c`) success — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E, Gate.

**`09 §25`:** göç genişletmedir (sekiz yeni tablo); `Down`, `suppressTransaction`, `CONCURRENTLY`, `now()` yok; her FK'nin silme davranışı açık; enumlar metin ve izinli değer kısıtlı; yeni kapalı değer yok. §10.8: sekiz tablo istisna listesinde değil, göçte `GRANT`/`REVOKE` yok, uygulama kullanıcısına yeni yetki eklenmedi; kalıcı kayıtların silinmezliği uygulama bekçisinde (`06` veritabanı düzeyinde yetki ayrımı istemiyor).

**Güvenlik:** sır yok (test IBAN'ı bilinen örnek); yalnız şema, auth etkisi yok; girdi doğrulama uygulanamaz (uç yok); yeni bağımlılık yok.

**`12` satırları:** kaynak birimleri `06` entity'leri (KP yok). `06 §4.22`…§4.31'i anan `VAL-D110`, `VAL-D111` ile ilgili `VAL-C107`, `VAL-D095` akış ve imha task'larınındır — BEKLEMEDE kalır, `Durum` değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** K1 → D-67 (`DefectClaim.StatusHistory` düz `string` + `jsonb`, `09 §12` m.5 dönüştürücüsü yok; D-57/D-65 kalıbı, hedef T320). K2 → D-51 eki (FK adı `fk_returns_late_delivery_terminations_late_delivery_terminatio` çatıca kırpıldı).

**Yapım raporu karşılaştırması:** kriter tablosu, KK listesi, test sayıları ve ⚠ kararlar uyumlu; uyuşmazlık yok. İki K-notu yapım raporunun ⚠ 1–2'siyle aynıdır.

## Altyapı Değişiklikleri

- **Şema/migration:** Var — sekiz tablo (genişletme göçü).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T111-kalem-kayitlari`
- **PR:** #369
- **CI:** yardımcı izler
- **Post-merge run'lar:** squash `f90d538` — CI `41798998317`, Publish `41798998313` (success)

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. `DefectClaim.StatusHistory` düz `string` + `jsonb` (varsayılan `'[]'::jsonb`); `PayloadJson` dönüştürücüsü yok — D-57/D-65 kalıbı → **D-67** (hedef T320).
2. EF'in 63 bayt kırpması: `fk_returns_late_delivery_terminations_late_delivery_terminatio`; elle kısaltılmadı, test kırpılmış adı tutar → **D-51 eki**.
3. Kontrol kısıtı adları `06` ad vermediği için seçildi; tutar kuralı `coupon_share_within_product` (en uzun tabloda 57 bayt, sınırın altında).
4. `ActorKind` yapanlı üç kayıtta izinli değer alt kümesiyle (`Buyer`, `Admin`) sınırlı; `System` reddedilir (`06 §4.22` "müşteri ya da yönetici").
5. İptalde müşteri notu: `reason_note_only_on_other` ile sebep yalnız yöneticide dolu olduğundan müşteri iptali açıklama taşıyamaz — `06 §4.22`'nin "müşteride `ReasonNote` boştur" cümlesi bu iki kuraldan türer, ayrı kısıt yazılmadı.
6. Bekçi yalnız silmeyi engeller: kalem kayıtları yerinde güncellenir (ulaşma tarihi düzeltmesi, stoğa ekleme, kapanışın yeniden açılması, imhada yönetici/ad/not boşaltması — `06 §4.26`, §4.28, §4.22).
7. Varlıklar `AfterSales` modülünde (`05 §3.3.5` ve `11` aday tablosu); `ItemRemoval` da aynı modülde (`11` T111 satırı). Kaleme, geçiş kaydına, siparişe, yöneticiye ve ile bağlar adla (`const string` + `HasOne`).
8. Ayarlayıcılar public (yerinde güncellenen kayıtlar; `Order`/`OrderItem` ile aynı); yazım fabrika yöntemleriyle (`ByBuyer`, `ByAdmin` …).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Kalem kayıtlarının yazım yolları (iptal, cayma, fesih, teslim alma, kapanış, ayıp talebi, IBAN) ve uygulama kuralları (açık adet, aralıklar, "Uygulamada" maddeleri) | — | İlgili akış task'larının işi (T320, T367–T372 …); bu task yalnız tabloları kurar |
| 2 | `DefectClaim.StatusHistory` tipli değer listesi | D-67 | `PayloadJson` yok; ayıp talebi durumunu ilk yazan task (T320) |
| 3 | Kırpılmış FK adı | D-51 | Doküman geçişi |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks`.
- **Ana dal CI startup (Adım 0):** ilk yapım ajanında son üç run success — `41782286498`, `41782286493`, `41777013421`; devam ajanında `41794927676` (CI), `41794927593` (Publish), `41786270867` (CI) success.
- **Devir:** ilk yapım ajanı bağlam sınırında, sır bekçisinin büyük commit yavaşlığı yüzünden commit atamadan durdu; devam ajanı stage'li 16 dosyayı yeni worktree'ye kopyalayıp (main `78075dd`, PR #368 dahil) commit'ledi. Designer ve anlık görüntü CRLF'den LF'ye çevrildi.
- **Repo memory (bitiş kapısı 8):** güncellendi.
