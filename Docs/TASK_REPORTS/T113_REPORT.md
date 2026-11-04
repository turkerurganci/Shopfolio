# T113 — İade reddi ve IBAN isteği

**Faz:** F1 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/AfterSales/` altında iki entity ve yapılandırması (`05 §3.3.5` "İptal, cayma ve iade" — iade reddi ve geri ödeme IBAN'ı bu modülün; `RefundIbanRequestReason` T111'den beri `AfterSales/Contracts`'ta): `ReturnRejection` (`06 §4.27`), `RefundIbanRequest` (§4.29). Özellik adları alan tablolarıyla birebir.
- Veritabanı varsayılanı yok — iki alan tablosunun "Varsayılan" sütunu boş.
- Kontrol kısıtları (`06 §7.3`):
  - `return_rejections.quantity_above_zero` (`ItemRecordRules.HasQuantityRule`).
  - `refund_iban_requests`: `fulfilled_or_closed` (karşılanma ve kapanış anından en çok biri), `iban_only_when_fulfilled` (IBAN bağı yalnız karşılanma anı doluyken), `refund_by_reason` (geri ödeme bağı dolu ⇔ sebep `CardRefundFailed` ya da kalemsiz istekte `TransferFailed`), `itemless_on_refund_failure` (kalem bağı boşsa sebep bu ikisinden biri ve geri ödeme bağı dolu); `reason` izinli değer kısıtı (altı değer, `06 §5.6.4`).
- Dizinler (`06 §7.1`, §7.2): her yabancı anahtarın dizini (7/7); filtreli tekil dizinler — `refund_iban_requests.order_item_id` (`fulfilled_at IS NULL AND closed_at IS NULL`), `refund_iban_requests.refund_id` (`order_item_id IS NULL AND fulfilled_at IS NULL AND closed_at IS NULL`); iki sütunun düz dizinleri ayrıca. Filtreler `06 §7.1`'in "Filtre" sütunuyla birebir.
- Bağlar (`06 §7.2`): iki tabloda yönetici bağı ve isteğin IBAN bağı boşaltılır (`SetNull`); kalem, cayma beyanı ve geri ödeme bağları kısıtlayıcı (`NoAction`). Modüller arası bağlar (`OrderItem`, `Admin`, `Refund`) `const string` + `HasOne` kalıbıyla (`09 §10.1` m.1).
- `AfterSalesRecordsGuard` kapsamına `ReturnRejection` eklendi (ticari kayıt, silinmez); `RefundIbanRequest` imhanın açık silmesiyle gittiği için kapsam dışı (`06 §4.29`, §2.2.5). Yerinde güncelleme serbest.
- Tek göç `20261104091422_CreateReturnRejectionAndIbanRequestTables` (`Down` yok; BOM'lu LF): iki tablo. `AppDbContext`'e iki `DbSet`.
- `EnumDatabaseTests` enum sütun sayısı 44 → 45 (`refund_iban_requests.reason`).
- T112 raporunda "Post-merge run'lar" dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/AfterSales/{ReturnRejection,RefundIbanRequest,AfterSalesRecordsGuard}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{ReturnRejectionAndIbanRequestTablesTests,EnumDatabaseTests}.cs`.
- `Docs/DEFERRED_BACKLOG.md` (D-55 eki), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T112_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

Öz denetim (PR öncesi): her kriter ve kontrol listesi maddesi diff'te göç satırıyla ve testle tek tek karşılaştırıldı; `06 §4.27` ve §4.29'un alan, Varsayılan, Kısıtlar, İndeksler ve Silme davranışı satırları ile §7.1 (satır 5476–5477), §7.2 (5597–5606), §7.3 (5708–5709) göçe karşı okundu. 7 yabancı anahtarın 7'si dizinli; varsayılanlı alan yok (veritabanında da yok — `Columns_HaveNoDefault`); varlıklar `AfterSales` modülünde (`05 §3.3.5`); ON DELETE davranışları `06 §7.2` ile birebir; iki filtreli tekil dizinin filtresi `06 §7.1` ile birebir; bekçi yalnız iade reddinin silinmesini ekledi, IBAN isteği açık.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki entity alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheTwoTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `Columns_HaveNoDefault`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable`, `CheckConstraints_AreTheDocumentedOnes` (tanımlarıyla) |
| 2 | `ReturnRejection` adedi > 0; kalem ve cayma beyanı bağları kısıtlayıcı | ✓ | `RejectionWithoutUnits_IsRefused` (0, -1), `RejectionsOfTheSameDeclaration_AreAccepted_AndKeepTheirArrivalDate`, `DeletingALinkedPrincipal_IsRefusedByTheDatabase` (`fk_return_rejections_order_items_order_item_id`, `fk_return_rejections_withdrawals_withdrawal_id`) |
| 3 | Karşılanma ve kapanıştan en çok biri; kalem başına ve kalemsiz istekte geri ödeme başına en çok bir açık istek | ✓ | `RequestBothFulfilledAndClosed_IsRefused`, `SecondOpenRequestOfTheItem_IsRefused` (`ix_refund_iban_requests_order_item_id1`), `SecondOpenItemlessRequestOfTheRefund_IsRefused` (`ix_refund_iban_requests_refund_id1`), `FulfilledAndClosedRequests_DoNotHoldTheItem_AndAnotherItemHasItsOwnOpenRequest`, `OpenItemRequestsOnTheSameRefund_AreNotUniqueByTheRefund`, `NewItemlessRequestOfTheRefund_AfterTheFirstIsFulfilled_IsAccepted` (K-1131) |
| 4 | IBAN bağı yalnız karşılanma anı doluyken | ✓ | `OpenRequestWithAnIban_IsRefused`, `ClosedRequestWithAnIban_IsRefused`, `FulfilledRequestWithItsIban_AndWithItsIbanDeleted_AreAccepted` |
| 5 | Geri ödeme bağı sebebe göre; kalemsiz istekte sebep iki başarısızlıktan biri ve bağ dolu | ✓ | `ItemRequestOfEachReason_WithTheRefundLinkOfItsReason_IsAccepted` (6), `ItemRequestOfAnotherReason_WithARefundLink_IsRefused` (5), `ItemRequestOfAFailedCardRefund_WithoutTheRefundLink_IsRefused`, `ItemlessRequestOfAFailedRefund_WithTheRefundLink_IsAccepted` (2), `ItemlessRequestOfAFailedRefund_WithoutTheRefundLink_IsRefused` (2), `ItemlessRequestOfAnotherReason_IsRefused_WithOrWithoutTheRefundLink` (4), `UnknownReason_IsRefused` |
| 6 | `ReturnRejection` silinmez; `RefundIbanRequest` açık silmeyle gider; yönetici ve IBAN bağı boşalır; isteğin kalem ve geri ödeme bağı kısıtlayıcı | ✓ | `DeletingARejectionThroughTheContext_IsRefused_AndTheRowStays`, `FulfillingARequestInPlace_AndDeletingItThroughTheContext_AreAccepted`, `DeletingTheIban_KeepsTheRequest_AndEmptiesItsLink`, `DeletingTheAdmin_KeepsTheRejectionAndTheRequest_EmptiesTheirLinksAndKeepsTheNames`, `DeletingALinkedPrincipal_IsRefusedByTheDatabase` (`fk_refund_iban_requests_order_items_order_item_id`, `fk_refund_iban_requests_refunds_refund_id`) |
| 7 | Göç tek konu; `Down` yok | ✓ | `Migration_HasNoDownMethod`; `has-pending-model-changes` → "No changes"; göçte `GRANT`/`REVOKE`/`now()`/`Sql(`/`suppressTransaction` yok |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 göç boş veritabanına, iki tablo | ✓ | `Migration_MakesTheTwoTables` (test veritabanı bütün göçlerle kurulur) |
| KK2 sıfır adetli ret | ✓ | `RejectionWithoutUnits_IsRefused` (`ck_return_rejections_quantity_above_zero`) |
| KK3 hem karşılanmış hem kapanmış; aynı kalemde ikinci açık; kalemsiz istekte aynı geri ödemede ikinci açık | ✓ | `RequestBothFulfilledAndClosed_IsRefused`, `SecondOpenRequestOfTheItem_IsRefused`, `SecondOpenItemlessRequestOfTheRefund_IsRefused` |
| KK4 karşılanmamış istekte IBAN bağı | ✓ | `OpenRequestWithAnIban_IsRefused`, `ClosedRequestWithAnIban_IsRefused` (`ck_refund_iban_requests_iban_only_when_fulfilled`) |
| KK5 sebebe aykırı geri ödeme bağı; öteki sebeple kalemsiz istek | ✓ | `ItemRequestOfAnotherReason_WithARefundLink_IsRefused`, `ItemRequestOfAFailedCardRefund_WithoutTheRefundLink_IsRefused` (`ck_refund_iban_requests_refund_by_reason`), `ItemlessRequestOfAnotherReason_IsRefused_WithOrWithoutTheRefundLink` (`ck_refund_iban_requests_itemless_on_refund_failure`) |
| KK6 IBAN silinince istek kalır, bağ boşalır | ✓ | `DeletingTheIban_KeepsTheRequest_AndEmptiesItsLink` |
| KK7 kaynak taraması | ✓ | Unit'teki mimari/kaynak taramaları yeşil (245/245); diff'te `Guid.NewGuid`, `CreateVersion7`, `Restrict`/`Client…`/`Cascade`, `HasConversion<int>`, `DateTime.Now`, `UtcNow` yok |
| `09 §25` | ✓ | Enum metin + izinli değer kısıtı (`§10.1`); her FK'nin silme davranışı açık; anahtar `EntityIds.New()`; göç genişletme, `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (`§10.6`); yeni tablolar varsayılan yetkide, `DatabaseUsersTests` yeşil (`§10.8`); yeni kapalı değer yok (kilit dosyası değişmez); yeni paket yok; modül sınırı — başka modülün varlığına yalnız tür adıyla FK, kod başvurusu yok |
| Güvenlik taraması | ✓ | Sır yok (IBAN örneği testte sıfırlardan yer tutucu); yetki/kimlik doğrulama etkisi yok; giriş noktası yok; IBAN değeri bu tablolarda tutulmaz (bağ `refund_ibans`'a); yeni dış bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18, `shopfolio-t113-pg`), ad alanı ve sınıfla dört koşuya bölündü | 676: Database+FileSecrets+Web 62/62; Model 614/614 (216 + 200 + 198); T113 sınıfı 45/45 |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `429ce56`, PR #371) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-68) |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Kapılar:** working tree temiz; `core.hooksPath` mutlak yolla `scripts/git-hooks` (eşdeğer; altı hook dosyası daldakiyle özetçe birebir); ana dal son 3 run `41805273609`, `41805273814`, `41798998313` success; `MEMORY.md`'de T113 satırı var.

**Kabul kriterleri:** kapsam tam (7 kriter, KK1–KK7, `09 §25`, güvenlik); hepsi ✓. Validator göçün SQL'ini `06 §4.27`, §4.29, §5.6.4, §7.1 (5476–5477), §7.2 (5597–5606), §7.3 (5708–5709) satırlarına karşı elle okudu:
- Kriter 1, 7: iki tablo tek göçte (`20261104091422_CreateReturnRejectionAndIbanRequestTables`), `Down` yok; sütun türleri ve boşluk `06 §2.5.1` ve alan tablolarıyla birebir (`AdminName` istekte `NOT NULL`, retde boşalabilir); varsayılan yok; göçte `Down`/`suppressTransaction`/`CONCURRENTLY`/`VACUUM`/`ALTER SYSTEM`/`CREATE DATABASE`/`now()`/`CREATE EXTENSION`/`UPDATE`/`DELETE` yok (grep çıkış 1); `has-pending-model-changes` → "No changes".
- Kriter 2: `ck_return_rejections_quantity_above_zero` (`quantity > 0`); kalem ve beyan FK'leri `NO ACTION`.
- Kriter 3: `num_nonnulls(fulfilled_at, closed_at) <= 1`; filtreli tekil dizinler `06 §7.1`'in "Filtre" sütunuyla birebir — kalem: `fulfilled_at IS NULL AND closed_at IS NULL`; geri ödeme: `order_item_id IS NULL AND fulfilled_at IS NULL AND closed_at IS NULL` (kalemli isteklerin aynı geri ödemede birden çok açık isteği serbest — `OpenItemRequestsOnTheSameRefund_AreNotUniqueByTheRefund`).
- Kriter 4: `refund_iban_id IS NULL OR fulfilled_at IS NOT NULL`.
- Kriter 5: `refund_by_reason` iki yönlü eşitlik `(refund_id IS NOT NULL) = (reason = 'CardRefundFailed' OR (reason = 'TransferFailed' AND order_item_id IS NULL))` — `06 §4.29` Kısıtlar ve §7.3 cümlesinin birebir okunuşu (kalemli `TransferFailed` "öteki sebepler"dedir; yapım ⚠ kararı 1 ile aynı okuma); `itemless_on_refund_failure` kalemsiz satırda sebebi iki başarısızlıkla sınırlar ve bağı ister; `reason` NOT NULL olduğundan iki ifade `NULL` üretmez. Enum kısıtının altı değeri `06 §5.6.4` ile birebir.
- Kriter 6: FK silme davranışları `06 §7.2` ile birebir (yönetici ve IBAN bağı `SET NULL`; kalem, beyan, geri ödeme `NO ACTION`); `AfterSalesRecordsGuard` `ReturnRejection`'ı silinmez kayıtlara ekledi, `RefundIbanRequest` kapsam dışı (imhanın açık silmesi); modül yeri `05 §3.3.5`/§4.1 tablosu (`AfterSales`) ile uyumlu.

**`09 §25`:** göç genişletmedir; enum metin + izinli değer kısıtı; her FK'nin silme davranışı açık; anahtar `EntityIds.New()`; yeni kapalı değer ve paket yok; yeni tablolar varsayılan yetkide (`Database` ad alanı testleri yeşil); numaralı dizin adları D-55 ekinde.

**Testler (validator, yerel PostgreSQL 18 `shopfolio-t113-val-pg`):** build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` çıkış 0; Unit 245/245 (Consistency dahil); Integration 676 — T113 sınıfı 45/45, Model 614/614, Database+FileSecrets+Web 62/62; `dotnet restore --locked-mode` çıkış 0; koşu sonrası `git status` temiz. Dal CI `41807318144` (head `429ce56`) yeşil, 9 check pass + guard skip.

**Güvenlik:** sır yok (IBAN testte sıfırlardan yer tutucu); yalnız şema, auth etkisi yok; girdi doğrulama uygulanamaz (uç yok); yeni bağımlılık yok.

**`12` satırları:** kaynak birimleri `06` entity'leri (KP yok); `06 §4.27`/§4.29'u anan matris satırı yok (`VAL-A537` `ReturnRejectionNotAllowed` uç task'ının — BEKLEMEDE kalır); `Durum` değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** K1 — Model ad alanının yerel koşusunda üç denemenin ikisinde `SampleDatabaseFixture.DisposeAsync` (`EnumDatabaseTests` sınıf temizliği) veritabanını düşürürken Npgsql okuma zaman aşımına uğradı (testlerin kendisi 614/614 geçti; tek başına ve CI'da temiz). T113'ün dokunmadığı test altyapısı; → D-68.

**Yapım raporu karşılaştırması:** kriter tablosu, KK listesi, test sayıları ve ⚠ kararlar (özellikle kalemli `TransferFailed`'de bağın boş olması) validator'ın bağımsız okumasıyla uyumlu; uyuşmazlık yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Var — iki tablo (genişletme göçü).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T113-iade-reddi-iban-istegi`
- **PR:** #371
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. **Kalemli `TransferFailed` isteğinde geri ödeme bağı boş.** `06 §4.29` ve §7.3: bağ "sebep `CardRefundFailed` iken ve kalemsiz istekte `TransferFailed` iken dolu, öteki sebeplerde boş". Kalemli `TransferFailed` cümlede ayrıca anılmıyor; kabul kriteri 5'in "sebebe göre dolu ya da boş" çerçevesi ve alanın açıklaması (bağ havale yolunun açıldığı ya da kalemsiz istekte gerçekleşmeyen havalenin geri ödemesi) üzerine kural iki yönlü eşitlik olarak yazıldı: `(refund_id IS NOT NULL) = (reason = 'CardRefundFailed' OR (reason = 'TransferFailed' AND order_item_id IS NULL))`. Kalemli `TransferFailed` bağla gelirse reddedilir (`ItemRequestOfAnotherReason_WithARefundLink_IsRefused`). Kısıt sonraki bir göçle gevşetilebilir.
2. Kalemsiz isteğin kuralı ayrı adlı kısıt (`itemless_on_refund_failure`) — KK5'in "öteki sebeplerden biriyle kalemsiz istek" maddesi kendi adıyla reddedilsin diye; `06 §7.3`'ün cümlesindeki "ve `RefundId` dolu" kısmı bu kısıtta da yazılı (`refund_by_reason` ile örtüşür). İki kısıtın birlikte düştüğü satırda PostgreSQL kısıtları ad sırasıyla denediği için `itemless_on_refund_failure` adlanır; testler bunu tutar.
3. `refund_iban_requests.order_item_id` filtreli tekil dizininin filtresi `06 §7.1` ile birebir (`fulfilled_at IS NULL AND closed_at IS NULL`); kalemsiz satırlar PostgreSQL'de `NULL` olduğu için birbirini engellemez — `order_item_id IS NOT NULL` eklenmedi.
4. Aynı sütunda iki dizin: `order_item_id` (düz + açık istek), `refund_id` (düz + kalemsiz açık istek). Veritabanı adları çatının numaralı adları (`ix_refund_iban_requests_order_item_id1`, `ix_refund_iban_requests_refund_id1`); model adları `OneOpenIbanRequestPerItem`, `OneOpenIbanRequestPerRefund` → **D-55 eki**. Kırpılan ad yok (en uzun FK adı 51 bayt).
5. Kontrol kısıtı adları `06` ad vermediği için seçildi.
6. Fabrika yöntemleri asgari: `ReturnRejection.Create`, `RefundIbanRequest.ForItem` (kalem; geri ödeme yalnız havale yolu açılırken) / `ForRefund` (kalemsiz istek). Her istek bir yönetici adımıyla doğduğu için fabrikalar yöneticiyi ister (`03 §1.7.1.9`). Ayarlayıcılar public (T111, T112 ile aynı; isteğin karşılanması ve kapanması yerinde yazılır).
7. `DeletingALinkedPrincipal_IsRefusedByTheDatabase` testinde iade reddinin kalemi, cayma beyanının kalemi değil — her bağı yalnız kendi satırına tutturmak için; kalem–beyan eşleşmesi yazarın kuralıdır (`06 §4.27` "Uygulamada").

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İade reddinin ve IBAN isteğinin yazım yolları ve "Uygulamada" kuralları (koşullu istisna sebebi, ulaşmamış adet sınırı, teslim almanın tarih sınırları, açık istek varken ikinci olayın onu kullanması, yalnız havale hattında doğma, `AdminName`'in yazımda dolu olması, kalemsiz istekte geri ödeme türünün `LatePayment` olması, imha) | — | İlgili akış task'larının işi; bu task yalnız tabloları kurar |
| 2 | Aynı sütunda numaralı dizin adları | D-55 | Doküman geçişi |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks`.
- **Ana dal CI startup (Adım 0):** açılışta T112 merge run'ları sürüyordu; son üç tamamlanmış run success — `41798998313` (Publish), `41798998317` (CI), `41794927676` (CI); PR öncesi T112 merge run'ları da success — `41805273814` (CI), `41805273609` (Publish).
- **Dış varsayım:** yok (yukarıda).
- **Bağlı backlog:** `grep -n "T113" Docs/DEFERRED_BACKLOG.md` boş — kapanan kalem yok.
- **Repo memory (bitiş kapısı 8):** güncellendi.
