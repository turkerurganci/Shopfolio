# T371 — Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Ön tespit:** T371'in davranışının kuralları bağımlılıklarda kurulmuştu — pencere `AfterSalesPeriodRules.IsWithdrawalWindowOpen` / `ServiceWithdrawalStartDay` (T325), düğme `OrderOperationCatalog.WithdrawPhysical` / `WithdrawService` / `WithdrawDigitalItem` ve `RecordWithdrawal` (T323), işaretin ürün düzeyi ve donması `ProductDefinitionRules`, `OrderItemSourceReader`, `OrderFreeze` (T332, T339), teslim tarihi (T351), dijital ve hizmet onay kutuları (T356). Pencere kuralını düğmeye `AfterSalesModule.ItemPeriodStatesAsync` bağlar (D-513'ün "T371/T372 cayma düğmesinde `IsWithdrawalWindowOpen`" kolu düğme tarafında karşılanmış durumda; beyan tarafı T372'nindir).
- **Tek kural evi (KK8):** koşullu istisna kuralı üç yerde yazılıydı (`AfterSales.WithdrawalExemptions`, `ConfirmationSummaryText.IsConditional`, Notifications'ta iki desen). Kural `AfterSales/Contracts/WithdrawalExemptions` olarak genel ve saf hâle getirildi (`Ordering.Contracts.ShippingPromise` kalıbı); onay özeti ve B-16 şablonu/değer okuyucusu bu işlevi çağırır, ikinci kopya kaldırıldı.
- **Testler:** kabul kriterlerinin her biri gerçek işlem kararı (Integration) ya da kural/yapı (Unit) üzerinden sınandı — ayrıntı öz denetim tablosunda.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/AfterSales/Contracts/WithdrawalExemptions.cs` (eski `AfterSales/WithdrawalExemptions.cs`'ten taşındı; `internal` → `public`)
- `src/Shopfolio.Web/Modules/Ordering/Documents/ConfirmationSummaryText.cs` (`IsConditional` kopyası kaldırıldı)
- `src/Shopfolio.Web/Modules/Notifications/CustomerNoticeTemplates.AfterOrder.cs`, `CustomerNoticeValues.AfterOrder.cs` (desen yerine tek işlev)
- `tests/Shopfolio.UnitTests/AfterSales/WithdrawalRightTests.cs` (yeni, 6 test)
- `tests/Shopfolio.IntegrationTests/Ordering/WithdrawalRightTests.cs` (yeni, 6 test)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 14 gün, sabit, ayar değil; m.10 pencere dışı kayıtla | ✓ | Unit `TheWindow_IsTheFourteenDayLegalPeriodInCalendarDays_AndNoSettingConstantOrInstallationKeyCarriesIt_KK1`; Integration `AClosedWindow_LeavesTheEarlierDeclarationStanding_AndTheFirmStillRecordsAnotherChannelsNotice_KK1_KK4` |
| 2 | Üç başlangıç; tarihsiz ya da kapanmış fiziksel kalemde başlamaz, çekilmez | ✓ | Integration `APhysicalItem_IsWithdrawableFromTheShipmentOn_…_KK2_KK3`, `AServiceItem_RunsFromTheOrderDate_…_KK2_KK6`, `AServiceItemOnAMixedOrder_…_KK2_KK6`; Unit `AServiceOnAnOrderThatCarriedAPhysicalItem_…_KK2_KK6` |
| 3 | Kargodaki kalemde cayma | ✓ | Integration `APhysicalItem_IsWithdrawableFromTheShipmentOn_…_KK2_KK3` (Kargoya verildi, 60 gün, tarihsiz → açık) |
| 4 | Kapanınca düğme kapanır, beyan geçerli, yeniden başlatma yok | ✓ | Integration `AClosedWindow_…_KK1_KK4`; Unit `ALateDeliveryMark_DoesNotRestartThePhysicalWindow_…_KK2_KK4` |
| 5 | Dijitalde hak yok, ödemede düşer, ayrı kutu | ✓ | Integration `ADigitalItem_HasNoWithdrawal_BeforeOrAfterThePayment_KK5`; kutu: T356 `ConfirmationStepTests` (üçüncü kutu) |
| 6 | Hizmette önce gelen sınır; başlamamışsa işaret; ifa başlangıcı kaydedilmez | ✓ | Unit `AServiceWindow_EndsAtWhicheverComesFirst_…_KK6`, `NoRecordKeepsTheStartOfAServicesFulfilmentOrAPerformedShare_…_KK6_KK7`; Integration `AServiceItemOnAMixedOrder_…` |
| 7 | Kısmi ifada tam cayma, kesinti yok | ✓ | Unit `NoRecordKeepsTheStartOfAServicesFulfilmentOrAPerformedShare_…_KK6_KK7` (ifa/kesinti alanı yok; kayıt yalnız üç tutarı ayırır). Tutarın hesabı T369'undur (bağımlılık değil) |
| 8 | İşaret isteğe bağlı; mutlakta düğme yok, koşulluda açık, koşul yazılır | ✓ | Integration `TheFrozenExemptionReason_HidesTheButtonForAnAbsoluteReason_AndKeepsItForAConditionalOne_KK8` (beş sebep + sebepsiz); tek kural `WithdrawalExemptions.IsConditional`; koşul metni `ConfirmationSummaryText.ExemptionNote` (T356 `ConfirmationStepTests`); işaret isteğe bağlı: `ProductDefinitionRulesTests` (T332) |
| 9 | İşaret ve sebep kaleme donar | ✓ | Integration `FrozenValuesTests.TheDeliveryLineAndWithdrawalAttributes_Freeze_AndOnlyANewOrderTakesTheirChange_KK2` (T339); Unit `OrderFreezeTests` |
| 10 | Tek tüketici rejimi, ticari bilgi yok | ✓ | Unit `TheBuyersSide_CarriesNoCommercialField_AndTheOrdersCompanyFieldsAreOnlyTheSellersFrozenIdentity_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: ayar yok, süre sabit | ✓ | `AfterSalesPeriodRules.WithdrawalWindowDays` tek sabit; `CompanySettings`, `ProductConstants`, `ParameterInventory.Entries`, `appsettings.json` taraması — Unit `…_KK1` |
| KK2: başlangıç üç durumda; başlamayan pencere | ✓ | fiziksel teslim tarihi, yalnız hizmet sipariş tarihi, karışık siparişte siparişin teslim tarihi; fiziksel kalem iptal edilmiş siparişte 90 gün sonra açık — Integration üç test |
| KK3: kargodaki kalemde düğme açık | ✓ | Integration `…_KK2_KK3` |
| KK4: kapanış ve eski beyanın korunması | ✓ | Integration `…_KK1_KK4` (beyan satırı aynı, `ReceiveReturnedGoods` açık, düğme kapalı) |
| KK5: dijital düğme yok, hak ödemede düşer | ✓ | Integration `…_KK5` (Pending ve Paid; başka kanal kaydı da yok) |
| KK6: önce gelen sınır; başlamamış pencerede işarete kadar açık | ✓ | Unit `…_KK6`, `…_KK2_KK6`; Integration `AServiceItem…`, `AServiceItemOnAMixedOrder…` |
| KK7: kısmi ifada tam geri ödeme | ✓ | Unit `…_KK6_KK7` (alan yok → kesinti yolu yok) |
| KK8: mutlak ve koşullu sebep düğmesi | ✓ | Integration `…_KK8`; `WithdrawalExemptions` tek ev, kopyalar kaldırıldı (`grep -rn "HygieneSensitive or" src` → yalnız kural evi) |
| KK9: donma sonradan değişmez | ✓ | `FrozenValuesTests` (T339) — bu task kod değiştirmedi |
| KK10: ticari bilgi alanı yok | ✓ | Unit `…_KK10` (satıcının donmuş kimliği dışında `Order`, `Customer`, `AddressBook`, `PartyAddress`, `ConfirmationParties`, `OrderAddressHistory`, `Cart`) |
| `09 §25` ilgili satırlar | ✓ | modül sınırı: yalnız `Contracts`'a başvuru (mimari testler yeşil); tek işlev (§24 m.4) uygulandı; yeni ayar/sabit yok; göç yok; zaman yalnız `TimeProvider` (testte `ManualTimeProvider`) |
| Güvenlik taraması | ✓ | yeni uç, girdi, sır, bağımlılık yok; değişiklik saf kural ve test |
| Sık FAIL nedenleri | ✓ | (1) tarama istisnası: KK10 istisnası yalnız `FrozenCompanyIdentity`'nin özellik adları, `Order.Company*` alt kümesi ayrıca doğrulanır; (2) çağrı biçimi: desen kopyaları `grep` ile tarandı; (3) DB default/FK: göç yok; (4) sayı anan yerler: 14 tek sabitte; (5) belge kuralı değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 5312/5312 geçti (yeni 6); Consistency 283/283 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/… --filter-class` (Postgres 18, `track_commit_timestamp=on`) | `WithdrawalRightTests` 6, `ItemRecordsTests` + `CustomerNoticeAfterOrderQueueTests` 43, `OrderOperationsTests` + `FrozenValuesTests` + `ConfirmationStepTests` 23 — hepsi geçti (tam Integration koşusu CI'da) |
| Contract | — | — | uç değişmedi |
| Lint | ✓ | `dotnet format <proje> --verify-no-changes --include <değişen dosyalar>` | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (D-767) |

### Doğrulama tur 1 — Doğrulama Sonucu — T371 Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim — tur 1
**Tarih:** 2026-11-09   **Branch:** task/T371-cayma-hakki   **Commit:** `d1a42218` (doğrulanan dal ucu); kayıt commit'i `origin/main` `b90fa93a` (T387) birleştirmesi üstünde, PR #488

**Verdict: ✓ PASS**

- **Başlangıç kapıları:** working tree temiz; hook yolu ana depodaki `scripts/git-hooks` (mutlak yol); ana dal CI son 3: 42558302378, 42558302344, 42548005029 success.
- **Kabul kriterleri 1–10 ✓** (validator kanıtı): (1) `AfterSalesPeriodRules.WithdrawalWindowDays` = 14 tek sabit, takvim günü; `CompanySettings`, `ProductConstants`, `ParameterInventory`, `appsettings.json`'da cayma yok; m.10 kaydı `OrderOperationCatalog.RecordWithdrawal` pencereyi koşul tutmaz (kapanmış pencerede ve mutlak istisnada açık, Integration `…_KK1_KK4`). (2) `IsWithdrawalWindowOpen` fiziksel kalemde teslim tarihinden, `ServiceWithdrawalStartDay` yalnız hizmette sipariş gününden, fiziksel kalem taşıyan siparişte siparişin teslim tarihinden; `OrderOperations.LoadAsync` `hasPhysical`'ı silinmeyen kalem satırlarından okur (iptal/çıkarma/fesih kayıttır), tarih yoksa pencere başlamaz ve sipariş gününe çekilmez. (3) `WithdrawPhysical` `HasShipped` (Kargoya verildi, Teslim edilemedi, Teslim edildi) ile açılır, tarihsiz kalemde kapanmaz. (4) Pencere sonunda düğme kapanır; beyan kaydına dokunulmaz, `ReceiveReturnedGoods` açık kalır; pencere işaret anından değil girilen tarihten işler (Unit `…_KK2_KK4`). (5) `WithdrawDigitalItem` her zaman kapalı; üçüncü kutu `ConfirmationStepRules` `DigitalWithdrawalWaiver` (T356). (6) `WithdrawService` ödeme onayı ister, pencere ya da `DeliveryMarkedAt` — önce gelen; başlangıçsız pencerede yalnız işaret; `ServiceFulfilmentConsent` kutusu (T356); ifa başlangıcı alanı yok. (7) `OrderItem`, `Withdrawal`, `WithdrawalDeclaration`, `ItemAmounts`'ta ifa/kesinti alanı yok, beyan üç tutarı çağırandan alır (tutar hesabı T369; `VAL-A076`). (8) `Product.WithdrawalExemptionReason` boşalabilir, varyantta yok, fiziksel dışında `ProductDefinitionRules` reddeder; mutlakta düğme yok, koşulluda açık (Integration `…_KK8`, beş sebep + sebepsiz); koşul metni `ConfirmationSummaryText.ExemptionNote`; kural tek evde `AfterSales.Contracts.WithdrawalExemptions`. (9) `OrderFreeze` sebebi kaleme yazar, sonraki ürün değişikliği verilmiş kaleme ulaşmaz (`FrozenValuesTests …_KK2`, `OrderFreezeTests`). (10) Alıcı tarafında (`Order` satıcı kimliği dışında, `Customer`, `AddressBook`, `PartyAddress`, `ConfirmationParties`, `OrderAddressHistory`, `Cart`) ticari alan yok; `src`'de vergi dairesi/fatura tipi/alıcı tipi alanı yok.
- **KK1–KK10, `09 §25`, güvenlik ✓.** Modül sınırı: Ordering ve Notifications yalnız `AfterSales.Contracts`'a başvurur (mimari testler yeşil); tek işlev (§24 m.4); yeni ayar, sabit, göç, uç yok. Secret yok (test verisi yer tutucu); auth/girdi etkisi yok; yeni bağımlılık yok.
- **Task satırları (`12 §7.5`):** KP-22 → `VAL-C012`, `C013`, `C015` (kriter 1, 2, 4, 6), `C016`, `C018` (8), `C017` (5), `A076` (7); KP-45 → `VAL-A202`…`A206` (8, 9). Hepsi uç yanıtı / tarayıcı gözlemi / mock geri ödeme ister; beyan ucu T372, panel T525, tutar T369 — `BEKLEMEDE` kaldı (T351, T356 emsali). KP-22'nin öteki satırları (`C011`, `C014`, `C019`, `A071`…`A075`, `A077`…`A081`) beyan, iptal, geri ödeme ve iade task'larınındır. `12_matris_denetle.py --kapsam 1-77` hata 0.
- **Testler:** build `-warnaserror` 0 uyarı; format ×3 temiz; Unit 5312/5312 (dal ucu), 5409/5409 (birleşik ağaç); betik testleri 4/4; Integration parçalı (`WithdrawalRightTests`, `OrderOperationsTests`, `ItemRecordsTests`, `FrozenValuesTests`, `ConfirmationStepTests`, `CustomerNoticeAfterOrderQueueTests`, `NotificationMapQueueTests`, `OperationPatternTests`) 79/79, birleşik ağaçta `WithdrawalRightTests` + `OrderOperationsTests` 14/14; `--locked-mode` çıkış 0. Dal CI run 42562463736 tüm işler pass.
- **Bulgu:** 0.
- **K-notları:** K1 `VAL-A206` beklenen sonucu "üç mutlak, dört koşullu sebep" der; `02 §7.3.4` ve `06 §5.3.7` iki koşullu sebep tanır ((ç) `HygieneSensitive`, (e) `SealedMedia`) → D-767 (`12` güncelleme turu; satır `BEKLEMEDE`).
- **Yapım raporu karşılaştırması:** kriter ve KK sonuçları uyumlu; öz denetim tablosu kanıtlı. Tek uyuşmazlık: yapım `12` satırlarını `C011`…`C015`, `A076`, `A202`…`A206` sayar; validator `C011` (beyan, T372) ve `C014` (ödeme bekleyen iptal) satırlarını dışarıda, `C016`…`C018` (istisna ve dijital) satırlarını içeride sayar. D-513'ün T371 düğme kolu `ItemPeriodStatesAsync` → `OrderOperationCatalog` ile karşılanmış; kalem öteki task'lar için açık kalır.
- **Birleştirme notu:** `origin/main` (`b90fa93a`: T387) çakışmasız birleşti; birleşik ağaçta build ve tam Unit 5409/5409.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T371-cayma-hakki`
- **Commit:** `d070a34c` — T371: Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim
- **PR:** #488
- **CI:** run 42562463736 (`d1a42218`) tüm işler pass
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | Tetikleyici terim taraması (plan tier, kota, API key, paket sürümü…) kabul kriterlerinde ve plan metninde boş; yeni paket, servis, ortam yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sipariş sayfasındaki cayma düğmesi ve koşul metni ekranı | — | Ekran henüz yok; düğme kararı `OrderOperationCatalog`'dan, koşul metni `ConfirmationSummaryText.ExemptionNote`'tan okunacak. Yeni kalem açılmadı: ekranlar kendi task'larının kapsamı |
| 2 | Kısmi caymada tutarın hesabı | — | T369'un kapsamı (`02 §7.1.7`); T371 kesinti alanının yokluğunu sınar |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 42558302378 success (T356), 42548005029 success, 42548005043 success (T357); 42558302344 sürüyordu (sayılmadı).
- **Repo memory (bitiş kapısı 9):** dokunulmadı (faz kapısı kayıt PR'ı günceller).
- **`12` satırları:** ilgili `VAL-C011`…`C015`, `A076`, `A202`…`A206` uç yanıtı kanıtı ister; `Durum` hücrelerini validator yazar.
- **⚠ Kolay karar:** `WithdrawalExemptions` modülün `Contracts`'ına genel saf kural olarak taşındı (`06 §5.1.3` "değerin davranışı sahibi kuralda": sahip AfterSales kalır; `ShippingPromise` emsali). Notifications ve Ordering yalnız `Contracts`'a başvurur.
- **Diğer:** D-513 (T325 parçası) birden çok task'ı hedefler; T371'in düğme kolu T323'te bağlı, kalem kapatılmadı.
