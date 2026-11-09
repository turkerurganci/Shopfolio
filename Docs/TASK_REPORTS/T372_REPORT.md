# T372 — Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 ✓ PASS; tur 1 ✗ FAIL: havale hattında aynı kalemin ikinci IBAN'sız firma kaydı 500 — düzeltme 1 ile kapandı) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Geçiş birimi (D-522):** `IOrderTransitions.DeclareWithdrawalAsync` (müşteri, sipariş sayfası) ve `RecordWithdrawalAsync` (yönetici, başka kanal bildirimi) — `OrderTransitions.Withdrawal.cs`. Sıra `09 §9.1` m.4: onay → kilit (+ panelde sürüm) → kilitli siparişte işlem kararı (`OrderOperations.CheckWithFactsAsync`, 1.11.5/1.11.6/1.11.34; düğmeyle aynı kural, D-153 kalıbı) → AfterSales yazıcısı → 1.4.4.6 sayaçları (`OrderCounters.ReturnAsync`) → kapanış kuralı (`EvaluateClosureAsync`; tek yol, D-306) → sürüm bir kez → B-9 (iki yol), F-3 (yalnız müşteri), B-14 (yönetici kaydı havalede IBAN'sız).
- **Başka kanal uyarıları:** `Ordering/WithdrawalNoticeRules` (saf) — pencere dışı, bir yıl (`ExtendedYears`, Yönetmelik m.10/1; yasal süre, ürün sabiti değil), mutlak istisna, teslim günü sorusu (K-608). Uyarı varsa `ConfirmationRequired` + `Warnings`; ileri tarih `InvalidInput`.
- **Yazıcı (AfterSales):** `WithdrawalDeclaration` artık donmuş değerleri (`ItemFrozenAmounts`) ve `WithdrawalIban`'ı alır; tutarı `ItemAmountSplit.Of` böler (`02 §7.1.7`: ürün tutarı yuvarlanmaz, kupon payı ve KDV `Money.ShareOfUnits` ile kuruşa aşağı, son açık adetler kalanı alır; önceki kayıtlar `TakenSharesAsync`). Havale IBAN'ı girilen / dosyadaki / istek (`RefundIbanRequest.ForItem(NoticeWithoutIban)`); teslim sonrası beyanda Z-38 silme günü (`AfterSalesPeriodRules.ReturnIbanDeletionAt`, D-667). Fiziksel kalemde iade adresi değer olarak donar ve sonuçla döner (B-9 içeriği).
- **Beyan ekranı okuması:** `IOrderOperations.ReadWithdrawalScreenAsync` → `WithdrawalScreen` (iade adresi, 14 gün, kargodaysa taşıyıcı notu, koşullu istisnada ambalaj koşulu, havalede IBAN alanı, açık adet); kargoya verilmemiş fiziksel kalemde `null`. Notlar `IAfterSalesModule.WithdrawalNotesAsync`'ten.
- **`Money.ShareOfUnits` (D-74):** `09 §14.1` m.1'in dördüncü işlevi `Money`'ye yazıldı; `ItemAmountSplit` kendi yuvarlamasını bıraktı (`TimeAndMoneyRulesTests` yuvarlamayı yalnız `Money`'de bulur).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/OrderTransitions.Withdrawal.cs` (yeni), `WithdrawalNoticeRules.cs` (yeni), `Contracts/WithdrawalRequests.cs` (yeni), `Contracts/IOrderTransitions.cs`, `Contracts/IOrderOperations.cs`, `OrderOperations.cs`, `OrderTransitions.cs`
- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.ItemRecords.cs`, `ItemAmountSplit.cs` (yeni), `AfterSalesPeriodRules.cs`, `Contracts/IAfterSalesModule.cs`, `Contracts/ItemRecordWrites.cs`, `Contracts/ItemRecordRefusal.cs`
- `src/Shopfolio.Web/Infrastructure/Money.cs` (`ShareOfUnits`)
- Testler: `tests/Shopfolio.IntegrationTests/Ordering/WithdrawalDeclarationTests.cs` (yeni, 21 — düzeltme 1 ile +3), `tests/Shopfolio.UnitTests/AfterSales/WithdrawalDeclarationRulesTests.cs` (yeni, 10), `MoneyTests` (+10), `ShipmentTransitionContractTests`, `AutomaticMomentsTests`, `ItemRecordsTests` (imza uyarlaması)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kalem ve adet seçilir, tarih damgası, form/noter yok, ardışık beyan | ✓ | Integration `TheCustomersDeclarations_AreStampedRecordsOfTheirOwn_…_KK1_KK2_KK5` (iki beyan, iki satır, damga = gönderim anı); `TheCustomersDeclaration_AsksOnlyTheItemTheUnitsTheConfirmationAndTheTransferIban_KK1` (istek alanları); firma yolunda ardışık kayıt (düzeltme 1): `TwoFirmRecordsWithoutAnIban_OnTheSameTransferItem_AreBothWritten_AndUseTheOneOpenRequest_KK1_KK6_KK7` (havale kaleminde iki IBAN'sız kayıt, iki satır) |
| 2 | Fizikselde güncel iade adresi, 14 gün, kargodaysa dönüş notu; hizmette yok; adres donar | ✓ | `TheScreen_ShowsTheAddressAndPeriodOnlyOnAPhysicalItem_…_KK2_KK3_KK8`; donma: `…_KK1_KK2_KK5` (ayar değişince ilk satır eski adresi korur) |
| 3 | Koşullu istisnada ambalaj koşulu | ✓ | `TheScreen_…_KK2_KK3_KK8` (`HygieneSensitive` → koşul; teslimli koşulsuz → `null`); tek kural `WithdrawalExemptions.IsConditional` |
| 4 | Eksen kendi başına değişmez; işaretsiz kalemde adetler kapanır; son açık kalemde kapanış kuralı | ✓ | `…_KK1_KK2_KK5` (Delivered/Paid sabit, geçiş kaydı yok); `APhysicalItemWithdrawnInTransit_ClosesItsUnits_ButTheShippedOrderWaitsForTheGoods_KK4`; `TheLastOpenServiceItemWithNothingDelivered_ClosesTheOrderToCancelled_ByTheClosureRule_KK4` (S3/S4, ödeme Paid, iptal kaydı ve B-7 yok); `TheLastOpenServiceItemBesideADeliveredItem_ClosesTheOrderToDelivered_KK4` (S2) |
| 5 | B-9 iki yolda, F-3 yalnız müşteri beyanında | ✓ | `…_KK1_KK2_KK5` (B-9 + F-3); `TheFirmsRecord_TakesTheNoticesDate_PastButNeverFuture_AndOnlyB9Goes_KK5_KK6_KK7` (yalnız B-9) |
| 6 | Kalem/adetle kayıt; tarih ulaşma tarihi, geçmiş olur, ileri olmaz; pencere dışı da kaydedilir | ✓ | `TheFirmsRecord_…_KK5_KK6_KK7` (ileri tarih `InvalidInput`, iki gün sonra girilen geçmiş tarih); `ANoticeOutsideTheWindow_AsksTheFirmToConfirm_AndIsRecordedThen_KK6`; `ANoticeBeyondTheWindowsYear_AndOneOnAnAbsolutelyExemptItem_AreWarnedOf_KK6`; `ANoticeOnTheDeliveryDay_…_KK6`; Unit sınır günleri `TheWindowsLastDay_IsInside_…`, `AYearAfterTheWindowsEnd_…`, `AServiceItemsCompletedMark_…`, `AnAbsoluteExemption_…`, `ANoticeOnThePhysicalItemsDeliveryDay_…`; aynı kalemin başka adedi için ikinci bildirim (düzeltme 1): `TwoFirmRecordsWithoutAnIban_…_KK1_KK6_KK7` |
| 7 | Firma teyit eder, ürün denetlemez; teyit tarihi değiştirmez; IBAN yalnız siparişin e-postasından; yoksa alan açılır, B-14 | ✓ | `TheFirmsRecord_HasNoFieldForTheProductToCheckTheSender_KK7`; `TheFirmsRecord_…_KK5_KK6_KK7` (kayıt anı değil bildirim tarihi); `OnTheTransferLine_TheNoticesIbanIsCarriedOverOnlyFromTheOrdersEmail_OtherwiseARequestOpensAndB14Goes_KK7` (e-postadan aktarılır + Z-38 günü; değilse istek + B-14; kartta hiçbiri); `TheCustomerOnTheTransferLine_EntersTheIban_OrUsesTheOneOnFile_KK7`. Düzeltme 1: `TwoFirmRecordsWithoutAnIban_…_KK1_KK6_KK7` (ikinci IBAN'sız kayıt açık isteği kullanır — tek açık istek, 23505 yok; B-14 her IBAN'sız kayıtta); `AnIbanFromTheOrdersEmail_ThatFailsTheCheck_IsNotCarried_…_KK7_D798` (e-postadan gelen, sağlaması tutmayan IBAN → IBAN'sız kayıt + istek + B-14); `TheCustomersDeclarationWithAnIban_AfterTheFirmsRecordWithoutOne_FulfilsTheItemsOpenRequest_KK7_D797` (IBAN getiren beyan açık isteği karşılar; sonraki IBAN'sız kayıt yeni istek açar) |
| 8 | Kargoya verilmemiş fizikselde düğme ve kayıt yok; bildirim "müşteriyle anlaşıldı" iptaliyle | ✓ | `APhysicalItemNotShippedYet_TakesNoWithdrawal_AndTheNoticeIsTheFirmsAgreedCancellation_KK8` (iki yol `ConditionNotMet`, satır yok, sürüm sabit; `CancelOrderAsync(AgreedWithBuyer)`); ekran `null` (`TheScreen_…_KK8`). Kalem düzeyindeki firma iptali T368'in yazıcısı (D-773, K-notu) |
| 9 | Geri alma ve düzeltme yok; firmanın kaydı da onay ister | ✓ | `NeitherWayWritesWithoutTheConfirmation_KK9`; `NoModuleSurface_UndoesOrCorrectsAWithdrawal_KK9`; `AfterSalesRecordsGuard` `Withdrawal`'ı salt eklenir tutar (T320) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1: damga, ardışık beyan, ek kanal şartı yok | ✓ | kriter 1 testleri |
| KK2: adres yalnız fizikselde, donar | ✓ | `TheScreen_…`, `…_KK1_KK2_KK5` |
| KK3: koşul metni yalnız koşullu üründe | ✓ | `TheScreen_…` (koşullu / koşulsuz / hizmet) |
| KK4: teslimlide eksen sabit; işaretsiz son açık kalemde kapanış | ✓ | dört KK4 testi |
| KK5: B-9 ve F-3 | ✓ | kriter 5 testleri |
| KK6: ileri tarih; pencere dışı onay, bir yıl | ✓ | dört Integration + beş Unit test |
| KK7: teyit denetimi yok, tarih sabit; B-14 yalnız havalede | ✓ | kriter 7 testleri; düzeltme 1'de her yazma yolunun ardışık çağrısı tekil dizinlere karşı tarandı: `refund_iban_requests` (`OneOpenIbanRequestPerItem`) artık açık isteği yeniden kullanır ya da karşılar; `withdrawals`, `refund_ibans`, `dispatch_records`'ta tekil dizin yok; sayaç dönüşü satır eklemez (`OrderCounters.ReturnAsync` → katalog sayacı) |
| KK8: kargo öncesi iptal yolu, cayma kaydı yok | ✓ | `…_KK8` |
| KK9: onaysız kayıt, geri alma, düzeltme yok | ✓ | iki KK9 testi + salt eklenir bekçisi |
| Devredilen backlog | ✓ | D-522 ✅, D-74 ✅ (`ShareOfUnits`); D-667, D-513, D-453 (1.4.4.6 hizmet kontenjanı: `AServiceWithdrawal_GivesTheWithdrawnCapacityBack_…_D453`), D-306 T372 payları kapandı (yerinde); düzeltme 1: D-797 ✅, D-798 ✅ |
| `09 §25` ilgili satırlar | ✓ | modül sınırı yalnız `Contracts` (mimari testler yeşil); yuvarlama yalnız `Money` (`TimeAndMoneyRulesTests`); kilit + sürüm + tek kayıt sahibi (yazıcı `SaveChanges` çağırmaz, işlem `ITransactionRunner`'da); zaman `TimeProvider`; göç ve yeni ayar yok; sözleşme biçimi (`ShipmentTransitionContractTests`) |
| Güvenlik taraması | ✓ | yeni uç/sır/bağımlılık yok; IBAN `InvalidIbanAttribute` (tek kural evi) ile denetlenir ve sıkıştırılır, geçersiz IBAN e-postadan da aktarılmaz; kartta IBAN girişi reddedilir; yönetici yolu `TryAdminActor` |
| Sık FAIL nedenleri | ✓ | (1) tarama istisnası: yeni tarama yok; KK9 adı deseni yalnız açık yüzlere bakar, satır bekçisi asıl garanti; (2) çağrı biçimi: yuvarlama taraması `ItemAmountSplit`'teki `Math.Round`'u yakaladı → `Money`'ye taşındı; (3) DB default/FK: göç yok; (4) sayı anan yerler: `Money` sınıf yorumu (T369 → T372), D-74 kapandı, bir yıl tek sabitte ve yasal süre testinde; (5) belge kuralı değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Düzeltme 1 (birleşik ağaç, `origin/main` `30d43bae` T352 dahil) | ✓ | restore `--locked-mode`; `dotnet build Shopfolio.slnx --no-restore -warnaserror`; `dotnet format <proje> --verify-no-changes` (3 proje); Unit parçalı; Integration `--filter-namespace Shopfolio.IntegrationTests.Ordering` ve AfterSales tabloları (`ReturnRejectionAndIbanRequestTablesTests`, `*AfterSales*`, `*Withdrawal*`) | restore exit 0; 0 uyarı 0 hata; format temiz; Unit 5177/5177 + Consistency 283/283; Ordering 416/416 (`WithdrawalDeclarationTests` 21/21); AfterSales/IBAN tabloları 72/72 |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 5430/5430; Consistency 283/283 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/… --filter-namespace` (Postgres 18, `track_commit_timestamp=on`) | TAM koşu üç parçada: Ordering 392, Model (`--max-threads 3`) 1393, öteki 1333 — 3118/3118; sonra eklenen D-453 testiyle `WithdrawalDeclarationTests` 18/18; `origin/main` (T348) birleştirmesinden sonra Ordering 408/408, Unit 5438/5438 |
| Contract | — | — | uç yok |
| Lint | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (src, Unit, Integration) | değişiklik yok |
| Build | ✓ | `dotnet build` (src, Unit, Integration) | 0 uyarı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS — tur 2 (2026-11-09, head `7901c14e`; `origin/main` `2e55a220` T376 birleştirildi, çakışma yok). Kriter 1–9 ve KK1–KK9 ✓: B1 kapandı — iki ardışık IBAN'sız firma kaydı yazılır, tek açık istek kullanılır, 23505 yok (`TwoFirmRecordsWithoutAnIban_…_KK1_KK6_KK7`); IBAN getiren beyan açık isteği karşılar (`…_KK7_D797`); e-postadan gelen sağlaması tutmayan IBAN aktarılmaz (`…_KK7_D798`). Ana dal CI son 3 run yeşil (42593651762, 42586725445, 42586725547). Yerelde restore (kilitli kip dahil) ✓, derleme `-warnaserror` 0/0, `dotnet format --verify-no-changes` 3 proje temiz, Unit 5177/5177 + Consistency 283/283, betik testleri 4/4, Integration dalın dokunduğu ve yazıcıyı kullanan 11 sınıf 206/206 (Postgres 18, `track_commit_timestamp=on`); main birleştirmesinden sonra derleme 0/0, Unit 5493/5493, Integration 4 sınıf 108/108. Dal CI: son run 42589139528 success (head `62454adc`); `7901c14e` için run tetiklenmemişti — kayıt commit'inin push'u CI'ı tetikler, merge'den önce yeşil görülür. Güvenlik temiz. Task satırları değişmedi (BEKLEMEDE; uç/sayfa yok, D-776); `12_matris_denetle.py --kapsam 1-77` Hata 0 |
| Bulgu sayısı (tur 2) | 0 |
| Bloke-etmeyen not (K) sayısı (tur 2) | 0 |
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-09, head `79ce2083`; `origin/main` `5f9b4c43` ile güncel, birleştirme gerekmedi). Kriter 2–5, 8, 9 ✓; kriter 1 ~ ve kriter 6 ~ (firma yolunda ardışık kayıt, B1); kriter 7 / KK7 ✗ (B1); güvenlik taraması temiz; dal CI 42583106374 yeşil. Yerelde restore (kilitli kip dahil) ✓, derleme `-warnaserror` 0/0, `dotnet format --verify-no-changes` 3 proje temiz, Unit 5155/5155 + Consistency 283/283, betik testleri 4/4, Integration dalın dokunduğu 6 sınıf 86/86 |
| Bulgu sayısı | 1 — **B1 KRİTİK S1** (`02 §10.4.10`, §7.3.5, §7.1.6; VAL-A238, VAL-A231, VAL-A071): havale hattında kalemde açık IBAN isteği varken ikinci IBAN'sız kayıt `AddWithdrawalAsync`'in `iban.Request` dalında ikinci `RefundIbanRequest.ForItem(NoticeWithoutIban)` ekler; `OneOpenIbanRequestPerItem` tekil dizini (`ix_refund_iban_requests_order_item_id1`) 23505 → `DbUpdateException` (500), kayıt yazılmaz. Geçici sınama: iki adetli havale kalemi, `RecordWithdrawalAsync` ×2 (`CameFromOrderEmail=false`) |
| Düzeltme gerekli mi | Evet — yazıcı kalemde açık istek varsa yenisini eklemez (alan zaten açık); B-14'ün ikinci kayıtta gidip gitmeyeceği `02 §10.4.10` / `03 §1.11.34` ile kararlaştırılır; iki ardışık IBAN'sız kayıt Integration testiyle sınanır |
| Bloke-etmeyen not (K) sayısı | 2 — K1: firmanın IBAN'sız kaydından sonra müşteri aynı kalemden IBAN girerek beyanda bulunursa yeni `RefundIban` yazılır ama açık IBAN isteği (`FulfilledAt` null) açık kalır; isteğin karşılanması üretimde hiç yazılmıyor → D-797 (T370; düzeltme turu B1 ile kapatabilir). K2: VAL-A238'in üçüncü hâli (sipariş e-postasından gelen, sağlaması tutmayan IBAN → IBAN'sız kayıt + istek + B-14) testte yok → D-798 (T372 düzeltme turu) |

**Task satırları (`12 §7.5`):** KP-22 ve KP-47 grep'i 56 satır getirir; T372'nin konusu VAL-C011, VAL-A071, A072, A077, A080, A081, A231–A238, VAL-C054 (KRİTİK) ve VAL-A234 (ORTA). Hepsi uç yanıtı ya da tarayıcı gözlemi ister; uç ve sayfa yok (D-776) → **BEKLEMEDE** (T371 emsali). Öteki KP-22/KP-47 satırları T369/T370/T373/T368'in. `12_matris_denetle.py --kapsam 1-77` Hata 0 (hücre değişmedi).

**Doğrulayıcı uyuşmazlık notları, tur 2 (adım 13):** tam uyumlu — kriter tablosu, öz denetim (KK başına kanıtlı) ve düzeltme 1 notu doğrulayıcının bağımsız bulgusuyla eşleşir; B-14'ün her IBAN'sız kayıtta gitmesi `02 §10.4.10` ("kayıtla birlikte ... B-14 gider") ve `03 §1.11.34` ("IBAN'sızsa B-14") ile tutarlı, ikinci olayın açık isteği kullanması `06 §4.29` ile birebir.

**Doğrulayıcı uyuşmazlık notları, tur 1 (adım 13):** (1) Rapor kriter 7'yi ve öz denetim KK7'yi ✓ veriyor; havale hattında aynı kalemin ikinci IBAN'sız firma kaydı sınanmamış ve 500 ile düşüyor (B1). (2) Kriter 1 ve 6'nın "ardışık beyan / kayıt" sözü yalnız müşteri yolunda sınanmış. (3) Test sayıları yapımın kendi ağacına ait (Unit 5430); doğrulama ağacında Unit 5155 + Consistency 283 = 5438, birleşik ağaç sayısıyla eşit.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T372-cayma-beyani`
- **PR:** #492
- **CI:** 42583106374 success (head `79ce2083`); 42589139528 success (head `62454adc`); doğrulama tur 2 kayıt commit'i yeni run'ı tetikler

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | Kabul kriterlerinde plan tier, kota, anahtar, paket sürümü yok; yeni paket, servis, ortam yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Kalem düzeyinde "müşteriyle anlaşıldı" firma iptali | D-773 (K-notu) | T368'in firma iptali yazıcısı; T372 sipariş düzeyindeki yolu sınar |
| 2 | Başka kanal kaydının işlem izi satırı | D-774 | Ordering hiçbir geçişte iz yazmaz (D-646 emsali); panel işlemi (T527) |
| 3 | Öteki kalem kayıtlarının aynı bölme işlevini kullanması | D-775 | İptal/çıkarma/fesih üretim yazıcıları T367/T368/T369'da |
| 4 | Beyan ve kayıt ekranları | D-776 (K-notu) | Sayfa katmanı yok; T513, T527 |

## Notlar

- **`12` satırları:** `VAL-C011`, `VAL-A071`, `A072`, `A077`, `A080`, `A081` uç yanıtı / panel kanıtı ister; `Durum` hücrelerini validator yazar (T371 emsali `BEKLEMEDE`).
- **⚠ Kolay kararlar:** (a) "Sipariş sayfası" = `IOrderOperations.ReadWithdrawalScreenAsync` + geçiş birimi; ekran yok (D-776). (b) Adet payı bölmesi T369'un kuralı olsa da cayma yazıcısı ilk hesaplayan olduğu için burada kuruldu; `Money.ShareOfUnits` `09 §14.1`'in yazdığı imzayla (D-74, D-775). (c) Bir yıllık uzama yasal süre olarak kuralın yanında sabit (`WithdrawalNoticeRules.ExtendedYears`; `09 §3.5` m.1 "envantere girmeyenler kendi evlerinde"), yasal süreler testine eklendi. (d) İşlem izi satırı yazılmıyor (D-774). (e) Müşteri yolunda sürüm denetimi yok (müşteri iptaliyle aynı; panel yolunda var).
- **Düzeltme 1 (2026-11-09; doğrulama tur 1 B1, K1, K2):** (B1) `AfterSalesModule.AddWithdrawalAsync` kalemin açık IBAN isteğini okur; havale hattında IBAN'sız kayıtta açık istek varsa yenisini eklemez, var olanı kullanır (`06 §4.29`: "açık istek varken doğan ikinci olay onu kullanır") — ikinci kayıt yazılır, `OneOpenIbanRequestPerItem` 23505'i yok. B-14 tekrar kuralı `02 §9.2` B-14 ve `03 §1.11.34`'ten: tetik "firma IBAN taşımayan bildirimi havale hattında kaydetti"dir, "bir kez" kuralı yok → her IBAN'sız kayıtta B-14 gider (⚠ kolay karar). (K1) IBAN getiren beyan ya da kayıt (girilen, e-postadan aktarılan, dosyadaki) kalemin açık isteğini karşılar: `FulfilledAt` + `RefundIbanId` (`06 §4.29` "müşteri IBAN'ı girerek karşılar"; `02 §7.4.5` IBAN cayma beyanı sırasında girilir); T370'in IBAN ömrü kuralı değişmez (D-797 ✅). (K2) e-postadan gelen, sağlaması tutmayan IBAN testi eklendi (D-798 ✅). Bütün kabul kriterleri ve KK1–KK9 düzeltilmiş diff'te yeniden denetlendi. `origin/main` (`30d43bae` T352) birleştirildi, çakışma yok.
- **Yapım akışı:** ilk işçi kod iskeletini, ikinci işçi testleri yazdı; bu tur testleri koştu, iki kırmızıyı (`TimeAndMoneyRulesTests` yuvarlama, sözleşme dönüş tipi) düzeltti, D-453 testini ekledi, kayıtları yazdı.
