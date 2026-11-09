# T374 — Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- Kanal, Z-18 süresi, açılış anı, durumlar, yeniden açma, bildirimler (B-9, F-3) ve dijital dosya güncellemesi bağımlılıklardan hazırdı (T320 yazıcıları, T325 `AfterSalesPeriodRules`, T340 dosya değiştirme, T351 iletişim formu); bu task onları kanıtladı ve eksik olan **para çözümlerini** kurdu.
- **Sözleşmeden dönme** (`IOrderTransitions.RescindDefectClaimAsync`): açık talepte, adet ≤ talebin dönülmemiş ayıplı adedi ve ≤ açık adet; tutar `ItemShares.SetAside` ile (son açık adetler kalanı alır); `DefectClaimRescission` iadesi siparişin ödeme yoluyla başlar (`IPaymentModule.StartDefectRemedyRefundAsync`), adetler, kupon payı ve KDV satırda.
- **Bedel indirimi** (`ReduceDefectClaimPriceAsync`): kalem dönmeden `AmountBased` iade; tutar > 0, kuruşa yuvarlı, tavan = sipariş toplamı − iptal edilmemiş iadeler (`CommittedRefundTotalAsync`).
- **Açık adet**: dönülen adet kalemi kapatmaz ama işleme açık adetten düşer (K-1077): `IAfterSalesModule.UnavailableQuantitiesAsync`; gecikme feshi, `OrderPeriods`, firma iptali ve işlem ekranı (`OrderOperations`, `ItemPeriodState.RescindedUnits`) aynı okumaya geçti.
- **Havale**: dosyada IBAN ve kalemde açık istek yoksa yöneticinin IBAN isteği (`AdminRequest`, iadeye bağsız — 06 §4.29 kısıtı) yazılır ve B-14 gider; ikinci çözüm yeni istek açmaz.
- **Z-18 bitişi**: `AfterSalesPeriodRules.IsDefectClaimChannelExpired` → `ItemPeriodState.DefectChannelExpired` (sipariş sayfası iletişim formuna yönlendirecek; ekran D-880).
- Devamda düzeltilen: IBAN isteği iadeye bağlanıyordu (`ck_refund_iban_requests_refund_by_reason` ihlali) → bağ kaldırıldı; testlerin tohumunda kupon payı yoktu (20 KDV oranıydı) → kalemin kupon payı 20 yapıldı.

## Etkilenen Modüller / Dosyalar

- AfterSales: `AfterSalesModule.DefectRemedies.cs` (yeni), `Contracts/DefectRemedies.cs` (yeni), `IAfterSalesModule.cs`, `ItemPeriodStates.cs`, `AfterSalesPeriodRules.cs`, `AfterSalesModule.OperationFacts.cs`
- Ordering: `OrderTransitions.DefectRemedies.cs` (yeni), `IOrderTransitions.cs`, `ItemRecordRequests.cs`, `OrderOperations.cs`, `OrderPeriods.cs`, `OrderTransitions.FirmCancellation.cs`, `OrderTransitions.LateDeliveryTermination.cs`
- Payment: `IPaymentModule.cs`, `PaymentFacts.cs`, `PaymentModule.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Ordering/ItemRecordsTests.DefectRemedies.cs` (yeni), `tests/Shopfolio.UnitTests/AfterSales/DefectClaimRulesTests.cs` (yeni)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sebepli; kanal iki yıl açık; süre sabit, ayar değil | ✓ | `AfterSalesPeriodRules.DefectClaimYears` sabiti; Unit `DefectClaimRulesTests.ThePeriod_IsTheLegalConstantOfTwoYears_AndNoSettingCarriesIt_KK1`, `AfterSalesPeriodRulesTests.DefectClaimChannel_ClosesTwoYearsAfterTheDeliveryDate_KK8`, `…_FromALeapDay_…`, `…_ClosesAtTurkeyMidnight_…` |
| 2 | Süre teslim işaretinden: fiziksel teslim tarihi, hizmet "tamamlandı", dijital ödeme onayı | ✓ | `AfterSalesPeriodRulesTests.DefectClaimChannel_OfAServiceItem_OpensWithTheCompletedMark_AndOfADigitalItem_WithTheApproval`; `DeliveryMarksTests` (T320) |
| 3 | "Sorun bildir": kalem, ayıplı adet, açıklama; dosya yok | ✓ | Unit `DefectClaimRulesTests.TheClaim_CarriesNoFile_OnlyTheUnitsAndTheDescription_KK3`; `OpenDefectClaimRequest(OrderId, ItemId, Quantity, Description)` |
| 4 | Fizikselde "Kargoya verildi"den, öteki ikisinde süreyle aynı işaretten açılır | ✓ | `AfterSalesPeriodRulesTests.DefectClaimChannel_OfAShippedPhysicalItemWithoutADeliveryDate_IsOpen_AndDoesNotClose_KK4`, `…_IsOpenInDeliveryFailed_AndClosedInCancelled`; Model `DefectClaim_PhysicalItemOnlyOnceShipped_OneClaimPerItem` |
| 5 | Tarihsiz kalemde süre işlemez | ✓ | `DefectClaimRulesTests.AnItemWithoutTheDeliveryDate_NeverExpires_TheRiskIsTheFirms_KK5_KK6`; `…_KK4` (yukarıda) |
| 6 | Süre dolunca kanal kapanır, iletişim formuna yönlenir; formda ayıp konusu yok | ✓ (kural) | `IsDefectClaimChannelExpired` → `ItemPeriodState.DefectChannelExpired`; `DefectClaimRulesTests.TheChannel_Expires_AtTheEndOfZ18_…_KK6`, `TheContactForm_HasNoDefectClaimTopic_KK6`; `ContactFormTests.TheTopics_…_AndTheDefectClaimIsNotOne_KK3`. Ekran D-880 |
| 7 | Açık/Çözüldü; firma çözer, firma ya da müşteri yeniden açar; kayıt tek; sistem çözümü yürütmez | ✓ | `ItemRecordsTests.SecondClaim_IsNotOpened_AndTheResolvedClaimIsReopened_OnTheSameRow`, `Resolve_IsTheAdministratorsOnly`, `Reopening_OnTheLastDay_IsAllowed_AndTheDayAfter_IsRefused`; çözüm yalnız firmanın kayıt adımı (`Remedies_WithoutMoneyToPayBack_OrByANonAdministrator_…_KK8`) |
| 8 | Dönmede iade hattı adetlerle, bedel indiriminde tutar bazlı; dönülen adet açık adetten düşer; havalede IBAN yoksa istek + B-14 | ✓ | `ItemRecordsTests.Rescission_StartsTheRescissionRefund_WithTheUnitsShares_AndTheUnitsLeaveTheOpenQuantity_WithoutClosingTheItem_KK8`, `Rescission_TakesAtMostTheClaimsUnitsNotRescindedYet_AndOnlyOnAnOpenClaim_KK8`, `Rescission_OfTheLastOpenUnits_TakesWhatTheEarlierRecordsLeft_KK8`, `PriceReduction_StartsAnAmountBasedRefundOfTheItem_AndTakesNoUnit_KK8`, `PriceReduction_IsCappedAtThePaidMoneyNotRefunded_TheStartedRefundsIncluded_KK8`, `Remedies_OnTheTransferLineWithoutAnIban_RequestItOnce_AndSendB14Once_KK8`, `Remedies_OnTheTransferLineWithAnIbanOnFile_AskNothing_KK8`, `OperationDecision_ReadsTheRescindedUnitsOutOfTheOpenQuantity_LikeTheWriters_D842` |
| 9 | Firma dosyayı yeniden yükler, eski alıcıya ulaşır; garanti süresi alanı yok | ✓ | `DigitalDeliveryTests.ReplacedFile_IsWhatTheEarlierBuyerGets_…_KK7_KK8` (T340); `DefectClaimRulesTests.NoWarrantyPeriodField_IsOnTheProductOrItsFile_KK9` |
| 10 | Doğuşta ve müşteri yeniden açınca B-9 + F-3; yönetici yeniden açması ve çözüldü bildirimsiz | ✓ | `ItemRecordsTests.ClaimNotices_GoToTheRightRecipients_OnlyOnTheBirthAndTheCustomersReopening`; `F3_RefusesAKindItDoesNotKnow_AndAReopeningOfAnythingButADefectClaim` |

**Doğrulama kontrol listesi (öz denetim):**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 süre sabit, ayar yok | ✓ | K1 satırı; `DefectClaimYears` `const`, ayar tablosunda yok (`…_AndNoSettingCarriesIt_KK1`) |
| KK2 başlangıç üç tipte | ✓ | K2 satırı |
| KK3 dosya alanı yok, adet seçimi | ✓ | K3 satırı |
| KK4 açılış anı tipe göre | ✓ | K4 satırı |
| KK5 tarihsiz kalemde süre işlemez | ✓ | K5 satırı |
| KK6 kapanışta forma yönlendirme | ✓ (kural) | K6 satırı; ekran D-880 |
| KK7 durumlar ve yeniden açma | ✓ | K7 satırı |
| KK8 iki çözüm yolu doğru hatta, tutar sınırı; IBAN isteği ve B-14 | ✓ | K8 satırı; beklenen paylar `ItemShares.SetAside`'dan türetilir (elle sayı yok) |
| KK9 dosya güncellemesi eski alıcıya | ✓ | K9 satırı |
| KK10 B-9/F-3 yalnız doğuşta ve müşteri yeniden açmasında | ✓ | K10 satırı |
| `09 §25` doküman uyumu: adlar 06'dan | ✓ | `RefundType.DefectClaimRescission`/`AmountBased`, 06 §4.33 `DefectClaimId`, `Quantity`, `CouponShare`, `VatAmount`; `RefundIbanRequestReason.AdminRequest` (06 §4.29); yeni enum/kapalı değer yok |
| `09 §25` modül sınırı, arayüzde `SaveChanges` yok | ✓ | Yeni Payment/AfterSales yöntemleri çağıranın izleyicisine yazar, kaydetmez; işlem dışı çağrı `InvalidOperationException`; `ModuleBoundaryTests` yeşil |
| `09 §25` tek işlev | ✓ | Pay yalnız `ItemShares.SetAside` + `SetAsideAsync` (dönme payları dahil); açık adet tek okuma (`UnavailableQuantitiesAsync` / `OpenQuantityAsync` aynı toplam) |
| `09 §25` para hesabı yalnız `Money` | ✓ | Bedel indirimi kuruş denetimi `Money.Round`; paylar `ItemShares` |
| `09 §25` zaman `TimeProvider` | ✓ | `_time.GetUtcNow()` (Ordering, Payment); sınırlar `AfterSalesPeriodRules` |
| `09 §25` göç / yeni tablo / yeni ayar | ✓ | Yok (var olan `refunds`, `refund_iban_requests` sütun ve kısıtları) |
| 06 kısıtları (sık FAIL 3) | ✓ | IBAN isteği `ck_refund_iban_requests_refund_by_reason`'a uyar (devamda düzeltildi, test `…_RequestItOnce_AndSendB14Once_KK8`); iade tutarı > 0 (sıfır dönme reddedilir, D-881) |
| Aynı kuralı anan öteki yerler (sık FAIL 4) | ✓ | Açık adeti okuyan bütün yerler (fesih, `OrderPeriods`, firma iptali, `OrderOperations`, AfterSales yazıcıları) aynı toplama geçti; `ItemRecordTable`/`03 §1.7.1` değişmedi — dönme kalem kaydı değil iade satırıdır, kalemi kapatmaz (1.7.1.10 "hattı etkilemez" doğru) |
| Kurucuya yeni bağımlılık → DI düzenekleri | ✓ | Kurucu değişmedi (AfterSales zaten `IPaymentModule` alıyordu); arayüze üye eklendi, test sahteleri derlendi |
| Devredilen backlog kalemleri (sık FAIL) | ✓ | D-524, D-820, D-842 yerinde ✅; D-513 T374 payı kapandı; ekran payı D-880'e |
| Belge kuralı değişikliği (sık FAIL 5) | ✓ | 09/06/02 metni değişmedi; karar kaydı gerekmez |
| Güvenlik taraması | ✓ | Yeni uç/sayfa yok; çözümler yalnız yönetici aktörüyle (`TryAdminActor`, test `…_OrByANonAdministrator_…`); satır kilidi + ekran sürümü (`LockItemAsync`); tutar girdisi sınırlı (> 0, kuruş, tavan); sır, ham SQL, yeni paket yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (yeni + Z-18) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --filter-class "*DefectClaimRulesTests" --filter-class "*AfterSalesPeriodRulesTests"` | 25/25 |
| Unit (Consistency) | ✓ | `--filter-trait "Consistency=true"` | 291/291 |
| Integration | ✓ | `--filter-namespace` Ordering 511/511; Payment+Concurrency+Disposal+Jobs 178/178; Model (`--max-threads 3`) 1403/1404 — `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled_…` yük altında düştü, tek başına 1/1 | yeşil |
| Lint | ✓ | `dotnet format` (Web değişen klasörler; Unit, Integration `--verify-no-changes`) | temiz |
| Build | ✓ | `dotnet build` Unit + Integration | 0 hata, 0 uyarı |

Koşulmayanlar (süre bütçesi): Unit tam koşu, Integration'ın değişiklikle ilgisiz ad alanları (Catalog, Pages, Web, Security vb.) — CI koşar.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-09, head `bbe9c7de`; `origin/main` `5352fadd` T384 #502 birleştirildi, çakışma yok, birleşik ağaç `b989df76`). Kriter 1–7, 9, 10 ve KK1–KK7, KK9, KK10 ✓ (modül düzeyi; ekran payı D-880). Kriter 8 / KK8 ~ Kısmi → FAIL: dönme ve bedel indirimi doğru hatta başlar, tutar sınırı ve IBAN isteği + B-14 doğru; ama dönülen adet sipariş bütününün firma iptalinin açık adet okumasından düşmüyor (B1). Ana dal CI son 3 run yeşil (42659142166, 42643385305, 42643385147). Dal CI 42659236737 (head `bbe9c7de`) yeşil, 9/9 iş. Yerelde (birleşik ağaç): derleme 0 uyarı / 0 hata (`-warnaserror`), `dotnet format --verify-no-changes` üç proje temiz, Unit 5412/5412 + Consistency 291/291, betik testleri 4/4, Integration Ordering 511/511, Files + Administration 138/138, `PaymentRecordTablesTests` + `ItemRecordTablesTests` + `RecordDisposalTests` 166/166; `dotnet restore --locked-mode` 0. |
| Bulgu sayısı | 1 — **B1 ORTA S1** (`02 §7.1.6`, §7.5.3; K-1077; komşu satır VAL-A909; D-820'nin "firma iptali aynı hesabı okur" kapanışı): sipariş bütününün firma iptali (`OrderTransitions.FirmCancellation.cs` `CancelOpenItemsByFirmAsync`, satır 112) açık adedi hâlâ `ClosedQuantitiesAsync` ile okur — dal yalnız kalem iptalinin yolunu (satır 66) `UnavailableQuantitiesAsync`'e geçirdi. Teslim edilemedi'deki tarihsiz fiziksel kalemde (kanal açık, D-532) talep açılıp bir adetten dönülünce firmanın S9 iptali kalemin bütün adedini iptal kaydına yazmak ister, yazıcı (`OpenQuantityAsync` dönülen adedi düşer) reddeder ve `InvalidOperationException("The whole-order cancellation's item record was refused under the order's lock.")` fırlar → 500, işlem geri alınır, sipariş firma iptaliyle kapanamaz. Geçici sınama (doğrulayıcı, silindi): üç adetli Teslim edilemedi siparişi, iki adetlik talep, bir adetlik dönme (sürüm 3), `CancelOrderRequest.ByAdministrator(..., 3, true, CannotShipToAddress)` → istisna. Yapılacak: `CancelOpenItemsByFirmAsync` (ve aynı kararı veren `CancellationConditionAsync` / `OpenItemsAsync` okuması gerekiyorsa) işleme açık adedi `UnavailableQuantitiesAsync` ile okur; açık adedi dönme yüzünden sıfır olan kalem atlanır; Integration sınaması eklenir. |
| Bloke-etmeyen not (K) sayısı | 2 — K1: dönme, bedel indirimi ve çözümün IBAN isteği işlem izine yazılmıyor (`03 §8.3.3.6` "İstek ize yazılır", §8.3.3.7 "İze yazılır"; D-646, D-774 emsali) → D-892 (T528). K2 ⚠: bedel indirimiyle ödenmiş tutarın tamamı geri ödendikten sonra dönme yine başlar ve geri ödemelerin toplamı ödenmiş tutarı aşar (geçici sınama: 280 TL'lik siparişte 280 TL bedel indirimi + iki adet dönme → toplam 466,67 TL); `02 §5.5` tavanı yalnız tutar bazlı geri ödemeye koyar, dönmenin önceki bedel indirimini düşüp düşmediğini belgeler yazmaz (`03 §8.3.3.7`'nin "mal dönmedi" kalıbı benzer durumu düşerek çözer) → D-893 (karar → T370). |
| Task satırları (`12 §7.5`) | KP-23 → VAL-A082, C020, C021, C022, C023, C024, A083, A084, C025; KP-49 → VAL-C061, C062 (öteki KP-49 satırları iletişim talebinindir). Hepsi uç yanıtı ister; T374 uç kurmaz (müşteri ve panel ekranları T513, T528; D-880) → modül düzeyi kanıt bu raporda, `Durum` `BEKLEMEDE` kalır (T320, T372 emsali); `12_matris_denetle.py --kapsam 1-77` Hata 0 (hücre değişmedi). VAL-A084 (geri gönderim masrafı) T373'ündür. |
| Yapım raporu karşılaştırması | 1 uyuşmazlık: öz denetimin "Aynı kuralı anan öteki yerler" satırı "açık adeti okuyan bütün yerler (… firma iptali …) aynı toplama geçti" diyor; sipariş bütününün firma iptali (`CancelOpenItemsByFirmAsync`) geçmedi (B1). Öz denetim tablosu kabul kriteri ve KK başına kanıtlı. |
| Düzeltme gerekli mi | Evet — B1 (yeni yapım turu; düzeltmeden sonra tur 2 doğrulaması) |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T374-ayip-talebi`
- **Commit:** `cd044e6d` (ara kayıt), `43ac6994` (devam), PR no commit'i
- **PR:** #504
- **CI:**
- **Post-merge run'lar:**

## Dış Varsayımlar

Dış varsayım: yok (tetikleyici tarama: kabul kriterleri ve plan metninde plan tier, kota, API key, paket sürümü, sandbox vb. terim geçmiyor; iş modül içi kayıt ve hesap).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Çözüm iadesi başlatılır, gönderilmez (kart işi, havale "gönderildi") | D-878 | Geri ödeme hattı T370, kart iadesi T405 |
| 2 | Fizikselde dönülen malın teslim alınması | D-879 | Teslim alma adımı T373 |
| 3 | Çözüm düğmeleri ve süre bitişinde forma yönlendirme ekranı | D-880 | Ekranlar T513, T528 |
| 4 | Tutarı sıfır dönme reddediliyor ⚠ | D-881 | Kayıt biçimi karar ister |

## Notlar

- **Working tree:** temiz (devam işçisi; dal `origin/task/T374-ayip-talebi` `cd044e6d`'dan alındı; `origin/main` dalın tabanı `8cd6652e`, ileri gitmemişti).
- **⚠ kararlar (kurala en uygun seçenekle):**
  1. Çözüm yalnız **Açık** talepte; çözülmüş talep önce yeniden açılır (`03 §1.9.1`).
  2. IBAN isteğinin sebebi `AdminRequest` ve iadeye bağsızdır — 06 §4.29 iade bağını yalnız başarısız kart iadesine ve kalemsiz isteğe verir; girilen IBAN siparişin dosyasına iner, bekleyen iadeleri öder.
  3. İade satırı dönme anında başlar; dönme kaydı iadenin kendisidir (K-1077, 06 §4.33).
  4. Bedel indirimi tavanı siparişin toplamı − iptal edilmemiş iadeler (gecikmiş ödeme iadesi hariç; `03 §1.11.31`).
  5. Tutarı sıfır dönme reddedilir (06 §4.33 tutar > 0) → D-881.
- **Repo memory:** dokunulmadı.
