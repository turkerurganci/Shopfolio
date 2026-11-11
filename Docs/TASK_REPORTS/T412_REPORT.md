# T412 — F3 kapanış düzeltmeleri: sipariş bekçisi, satış sonrası kapanış ve IBAN kullanımı

**Faz:** F4 | **Durum:** ✓ Tamamlandı — doğrulama tur 3 ✓ PASS (tur 1 ve tur 2 ✗ FAIL; tur 2 sonrası eskalasyonla düzeltme 2) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **KK1 — sipariş sürüm bekçisi (D-95; K-1581):** `OrderVersionGuard` kökü izlenen kayıtlardan çıkmayan kaydı kilitli sipariş sayısından bağımsız reddeder ("cannot be told"; tek kilitli siparişin sayılmaz). Siparişi bilinen kayıtta önce kilide bakar — kilitsiz siparişe yazım sürümü artmış olsa da "written without that order's row lock" iletisini alır —, sonra artışa. Yeni (Added) sipariş atlanır. Kökü yüklemeyen işlemler kökü yükler: `OrderTransitions.ReportTransferFailedAsync` kalemi izlenen olarak okur, `RefundIbanDeletionDay` kilitten sonra siparişi ve kalemlerini yükler; test düzenekleri (`ItemRecordsTests`, `RefundAmountTests`, `AxesTieTests`, `ConcurrencyTests`) kalemi yükler. `09 §10.2` m.3 paragrafı yeniden yazıldı.
- **KK2 — e-posta ekseni (D-808):** `OrderAxes.OfMember(orders, customerId)` (`!IsGuestOrder && CustomerId == id`); `OpenUnpaidOrderLimit` ve `UnpaidCancellationLimit` müşteri kimliğiyle sayar, `OrderPlacementLimits.FindExceededAsync` `Guid? memberId` alır, `OrderConfirmation` `request.Owner.CustomerId` geçirir. `09 §15.4` m.5, `06 §4.17` `NormalizedContactEmail` notu ve `CustomerId` dizini eşitlendi.
- **KK3 — IBAN kullanımı (D-903, D-910):** yeni `AfterSalesModule.RefundIbanUses.cs`: `LoadRefundIbanUsesAsync` siparişin IBAN kullanıcılarını sabit sayıda sorguyla bir kez yükler (kayıtlar, istek kalemlerinin IBAN'sız kayıtları, geri ödemeler, kapanan cayma adetleri, teslim almalar, bekleyen havale geri ödemelerinin IBAN'ı). `RefundIbanUses.IsInUse` eski `IsRefundIbanInUseAsync`'in (kaldırıldı) kuralını bellekte uygular; `DeleteUnusedRefundIbansAsync` döngüde sorgu yapmaz. `RefundIbanUses.SubjectsOf` Z-38 konularını verir: bağlı iptal/fesih; cayma → teslimden sonraysa teslim almalar, teslimden önce ve hizmette cayma kaydının kendisi; istek `RefundId` → o geri ödeme; kalem isteği → kalemin IBAN'sız kayıtları; bekleyen havale geri ödemeleri. `PaymentModule.FindSettledRefundSubjectsAsync` `Id`, `ItemRemovalId` ve `WithdrawalId` (yalnız `ReturnId == null`) eşleşmelerini de okur.
- **KK4 — tam dönülmüş kalem (D-905):** `OrderTransitions.OpenItemsAsync` kapanan ve dönülen adedi (`UnavailableQuantitiesAsync`) düşer: bütün adetleri dönülmüş kalem S5/S8'e ve teslim işaretine girmez; Teslim edilemedi'de yalnız böyle kalemleri kalan siparişte S9 kapanıştır. `02 §7.1.6` cümlesi.
- **KK5 — ret ve kapanış (D-915):** `AddReturnRejectionAsync` `includeClosed: true` ile kapanış adetlerini de reddeder ve teslim almayla aynı `ReopenClosuresBeyondAwaitingAsync` ile kapanışı açar; `06 §4.26` İlişkiler satırına "ret de" cümlesi.
- **KK6 — kargo bilgisi tavanı (D-309; K-1586):** `Ordering/Contracts/ShipmentLimits` (`TrackingNumberMaxLength` 64, `OtherCarrierNameMaxLength` 100); `TryNormalizeShipment` kırpılmış değere uygular (S5, S8, kargo bilgisi düzeltmesi), aşım `InvalidInput`. `09 §3.5` tablosuna satır, `06 §4.17` iki sütun notu.
- **KK7 — e-posta yerel kısmı (D-379):** `InvalidEmailAttribute` yerel kısmı noktayla başlayan, biten ya da ardışık nokta içeren adresi reddeder; `04` 7.3.2.1 hücresi eşitlendi. Eski davranışı sınayan kayıt testi (`MembershipTests.Register_TakesTheAddressTheProjectsOneEmailRuleAccepts`, `a..b@`) yeni kurala göre değişti; ret teorisine üç biçim eklendi.
- **KK8 — sır bekçisi (D-80):** `scripts/git-hooks/pre-commit` satırın başındaki `-e`/`--env` önekini (`-e "K=v"`, `-e K=v`, `--env "K=v"`, `--env=K=v`) soyar, anahtar biçimi denetimi anahtarı görür; `test-pre-commit.sh` dört yakalanan, iki geçen satır. Paket betiğinin 1/7 taraması (`package-build.sh`, `(^|[^A-Za-z0-9_])NAME=`) `-e "` önekli satırı zaten atlamıyor — öndeki `"` karakter sınıfına uyar; kod değişmedi.
- **KK9 — ödeme kümesi (D-847, D-896):** `Ordering/Contracts/PaymentApproval` (`IsApproved`, sorgu dizisi `Statuses`) tek tanım; `SalesPeriodReader`, `SalesSummaryRules`, `OrderExport.IsPaymentCompleted` (devreder), `OrderOperationFacts.PaymentApproved`, `AfterSalesModule.PendingWork` onu çağırır.
- **Düzeltme 1 (tur 1 B1, KK9):** kümeyi tümleyeniyle ("Pending ve Failed değil") yazan iki yer de tek eve bağlandı: `DeliveryMarkRules.IsPaymentOpenToMark` → `PaymentApproval.IsApproved`; `PendingWorkReader` `ServiceUnmarked` sorgusu → `PaymentApproval.Statuses` (sorgu dizisi, `Contains` ile çevrilir). `OrderTransitions` S9 teslim kapanışındaki yasak çift denetimi (`03 §1.4.1`, Teslim edildi + Pending/Failed) kendi kopyası yerine çift tablosunu okur: `TransitionTable.IsPairAllowed(OrderStatus.Delivered, …)` — istisnaya gerek kalmadı. `PaymentApprovalTests` taraması tümleyeni beş biçimde, her sırada, boşlukta ve satır kırılımında yakalar: `… or …` deseni (`is not (…)`, `is …`, switch kolu), `not … and not …`, `!= … && != …` zinciri, `== … || == …` zinciri, `[Pending, Failed]` listesi; tek istisna yine `TransitionTable.cs` izinli çift satırı. Davranış aynı (`DeliveryMarkRulesTests` durum başına beklenen değerler değişmeden yeşil).
- **Düzeltme 2 (tur 2 B1, KK1):** kod değişmedi; belge koda hizalandı. `09 §10.2` m.3 paragrafı bekçinin yeni sipariş muafiyetini yazar: aynı `SaveChanges`'te eklenen (`Added`) siparişin kayıtları kilit ve artış denetiminin dışındadır (satırı işlem kesinleşmeden başka işlemce görülmez; sipariş onayı önceki siparişi onun kilidiyle iptal edip yeni siparişi aynı işlemde yazar — `05 §4.2.2` (3)); muafiyet yalnız eklenen siparişindir, var olan siparişte sıra kilit → artış. Karar kaydı K-1605; `05 §7.3.2` ve `06 §2.6.4`'ün "her işlem kilidi alır" cümlesine yeni sipariş istisnası. İki yeni test: `ConcurrencyTests.AnOrderAddedInTheTransaction_IsWrittenWithoutItsLockOrRaise_WhileAnotherOrderIsLocked_T412_KK1` (başka sipariş kilitliyken yeni sipariş ve kalemi kilitsiz, artışsız yazılır, sürüm 1) ve `AnOrderAddedInTheTransaction_DoesNotExemptAnExistingUnlockedOrder_ItGetsTheLockMessage_T412_KK1` (aynı kayıtta var olan kilitsiz siparişe yazım, sürümü artırılmış olsa da, kilit iletisiyle reddedilir, yeni sipariş de yazılmaz); düzenek `BuildOrder` (kaydetmeden sipariş grafiği) `SeedOrderAsync`'ten ayrıldı.
- **Backlog:** D-80, D-95, D-309, D-379, D-808, D-847, D-896, D-903, D-905, D-910, D-915 yerinde ✅. Yeni kalem yok.

## Etkilenen Modüller / Dosyalar

- Düzeltme 2: Integration `Concurrency/ConcurrencyTests.cs`; belgeler `09 §10.2` m.3, `05 §7.3.2`, `06 §2.6.4`, `PRODUCT_DISCOVERY_STATUS.md` (K-1605)
- Ordering (düzeltme 1): `DeliveryMarkRules.cs`, `PendingWorkReader.cs`, `OrderTransitions.cs`, `Contracts/PaymentApproval.cs` (yorum); Unit `Ordering/PaymentApprovalTests.cs`
- Ordering: `OrderVersionGuard.cs`, `OrderAxes.cs`, `OpenUnpaidOrderLimit.cs`, `UnpaidCancellationLimit.cs`, `OrderPlacementLimits.cs`, `OrderConfirmation.cs`, `OrderTransitions.Shipment.cs`, `OrderTransitions.RefundLine.cs`, `RefundIbanDeletionDay.cs`, `OrderExport.cs`, `OrderOperationFacts.cs`, `SalesPeriodReader.cs`, `Contracts/PaymentApproval.cs` (yeni), `Contracts/ShipmentTransitionRequests.cs`
- AfterSales: `AfterSalesModule.RefundIbanUses.cs` (yeni), `AfterSalesModule.RefundIbans.cs`, `AfterSalesModule.RefundLine.cs`, `AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.PendingWork.cs`, `Contracts/RefundIbanUse.cs`
- Payment: `PaymentModule.cs`, `Contracts/IPaymentModule.cs`; Administration: `SalesSummaryRules.cs`; Infrastructure: `Validation/FieldRules.cs`
- Betikler: `scripts/git-hooks/pre-commit`, `scripts/git-hooks/test-pre-commit.sh`
- Testler: Integration `Concurrency/ConcurrencyTests.cs`, `Ordering/AutomaticMomentsTests.cs`, `AxesTieTests.cs`, `ItemRecordsTests*.cs`, `OpenUnpaidOrderLimitTests.cs`, `OrderCorrectionTests.Interventions.cs`, `OrderPlacementLimitTests.cs`, `RefundAmountTests.cs`, `ShipmentTransitionsTests.cs`, `Model/MembershipTests.cs`; Unit `Ordering/PaymentApprovalTests.cs` (yeni), `Validation/FieldRuleTests.cs`
- Belgeler: `02 §7.1.6`, `04` 7.3.2.1, `06 §4.17` (üç satır + İndeksler), `06 §4.26`, `09 §3.5`, §10.2 m.3, §15.4 m.5; `DEFERRED_BACKLOG.md` (11 satır); `STATUS_CHANGELOG.d/T412.md`; `IMPLEMENTATION_STATUS.md` (T412 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kilitsiz siparişe yazım her durumda kilit iletisi; kökü bilinmeyen kayıt her durumda ret; kökünü yüklemeyen işlem kökü yükler; `09 §10.2` m.3 | ✓ (düzeltme 2; tur 2 ~ B1) | `OrderVersionGuard.Check`; `ConcurrencyTests.ARecordWhoseRootIsNotLoaded_IsRefused_EvenWithOneLockedOrder_AndItsOwnRaisedOrder_T412_KK1`, `AWriteToAnUnlockedOrder_GetsTheLockMessage_EvenWhenItsVersionIsRaised_T412_KK1`; yeni sipariş muafiyeti `AnOrderAddedInTheTransaction_IsWrittenWithoutItsLockOrRaise_WhileAnotherOrderIsLocked_T412_KK1`, `AnOrderAddedInTheTransaction_DoesNotExemptAnExistingUnlockedOrder_ItGetsTheLockMessage_T412_KK1`; kök yükleme `ReportTransferFailedAsync`, `RefundIbanDeletionDay.ProcessAsync`; tam Integration yeşil (kökü yüklemeyen başka yol patlamadı); `09` satır 913 kodun dört adımını kodla aynı sırada yazar (kökü bilinmeyen → ret; aynı `SaveChanges`'te eklenen sipariş → muaf; var olan siparişte kilit → artış), K-1605 |
| 2 | E-posta ekseni `CustomerId`; `09 §15.4` m.5 | ✓ | `OrderAxes.OfMember`; `OpenUnpaidOrderLimitTests.TheEmailAxis_IsTheAccount_NotTheContactEmailTheOrderCarries_T412_KK2`, `OrderPlacementLimitTests` (müşteri satırlı tohum, imza `Guid?`); `09 §15.4` m.5, `06 §4.17` |
| 3 | `FindRefundIbanUseAsync` kargodan önceki caymayı, hizmet caymasını, istek bağlantılı geri ödemeyi sayar; küme sipariş başına bir kez | ✓ | `RefundIbanUses.SubjectsOf`, `LoadRefundIbanUsesAsync`; `AutomaticMomentsTests.DeletionDay_KeepsTheIban_ForAWithdrawalBeforeTheDelivery_AServiceWithdrawal_AndARequestsRefund_WhileTheirRefundsArePending_T412_KK3`, `DeletionDay_DeletesTheIban_OnceTheRefundsOfThoseThreeSubjectsWereSentOrVoided_T412_KK3`; `DeleteUnusedRefundIbansAsync` döngüsünde sorgu yok |
| 4 | Tam dönülmüş kalem S8'den ve teslim işaretinden çıkar; S9 kapanışı | ✓ | `OpenItemsAsync` → `UnavailableQuantitiesAsync`; `ItemRecordsTests.AReturnedShipmentWhoseUnitsAreAllRescinded_IsNotResent_AndItsS9IsTheClosure_NotACancellation_T412_KK4`; `02 §7.1.6` |
| 5 | Ret de "mal dönmedi" kapanışını açar | ✓ | `AddReturnRejectionAsync` (`includeClosed: true`, `ReopenClosuresBeyondAwaitingAsync`); `ItemRecordsTests.Rejection_AfterTheClosure_RejectsTheClosedUnits_AndReopensTheClosure_T412_KK5`; `06 §4.26` |
| 6 | `TrackingNumber` ≤ 64, `OtherCarrierName` ≤ 100; `09 §3.5` sabitleri; aşım `InvalidInput` | ✓ | `ShipmentLimits`; `ShipmentTransitionsTests.S5_TakesTheTrackingNumberAndTheCompanyName_AtTheirCaps_AndRefusesOneCharacterMore_T412_KK6`, `OrderCorrectionTests.TheCarrierCorrection_TakesTheCaps_AndRefusesOneCharacterMore_T412_KK6` (sınırın iki yanı, beklenen değer sabitten); `09 §3.5`, `06 §4.17` |
| 7 | Yerel kısım noktayla başlamaz, bitmez, ardışık nokta yok; `04` 7.3.2.1 | ✓ | `InvalidEmailAttribute`; `FieldRuleTests` (`.ad@`, `ad.@`, `a..b@`), `MembershipTests.Register_WithAWrongAddress_IsRefusedAndNamed` (üç biçim); `04` 7.3.2.1 |
| 8 | Kanca ve paket betiğinin taraması `-e "` önekli satırı atlamaz | ✓ | `pre-commit` önek soyma; `test-pre-commit.sh` `sh -e quoted/bare`, `sh --env quoted`, `sh --env= form` (yakalanır), `sh -e placeholder`, `sh -e _FILE short path` (geçer); `package-build.sh` 1/7 deseni önekli satırı zaten görür |
| 9 | Ödeme kümesi tek yerde; okuyan her yer onu kullanır | ✓ (düzeltme 1; tur 1 ~ B1) | `PaymentApproval`; yedi tüketici çağırır (`SalesPeriodReader`, `SalesSummaryRules`, `OrderExport`, `OrderOperationFacts`, `AfterSalesModule.PendingWork`, `DeliveryMarkRules.IsPaymentOpenToMark`, `PendingWorkReader` `ServiceUnmarked`). `PaymentApprovalTests.NoSourceFile_WritesTheSetAgain_OutsideItsOneHome_KK9` üç durumlu kopyayı ve tümleyeni arar; eski üç satır geçici kopya olarak `src` altına konunca tarama üçünü de buldu (`DeliveryMarkRules.cs:17`, `PendingWorkReader.cs:48`, `OrderTransitions.cs:361`), kopya silindi. `git grep -nE "PaymentStatus\.(Pending\|Failed)" src/`: kalanlar tek durum okumaları (ödeme penceresi, Ö2, iptal sınırı), çift tablosu ve DB çift kuralı — küme değil |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: bekçinin iki yolu ve kök yükleme testli; `09 §10.2` m.3 kodla eşit | ✓ (düzeltme 2) | Dört `ConcurrencyTests.*_T412_KK1` testi (kök yüklenmemiş → ret; kilitsiz sipariş artırılmış/artırılmamış → kilit iletisi; başka sipariş kilitliyken eklenen sipariş kilitsiz yazılır; eklenen sipariş var olan kilitsiz siparişi muaf kılmaz); kök yükleyen yollar Integration'da (Ordering 611, Concurrency 30) yeşil. `09 §10.2` m.3 satır satır `OrderVersionGuard.Check` ile karşılaştırıldı: kilitli sipariş yoksa sessiz ✓, kökü bilinmeyen → ret ✓, `IsNew` (aynı `SaveChanges`'te `Added` sipariş) → muaf, gerekçesiyle ✓ (düzeltme 2), var olan siparişte kilit → sonra artış ✓. Kuralı anan öteki yerler: `05 §7.3.2`, `06 §2.6.4` ("her işlem kilidi alır") yeni sipariş istisnasıyla; `07`'de bekçiyi ya da yeni siparişin kilidini anan yer yok (`grep -n "satır kilidi"`) |
| KK2: e-posta ekseni `CustomerId` | ✓ | `OrderAxes.OfMember`; iletişim e-postası hesabınkinden farklı sipariş testi (`..._T412_KK2`) |
| KK3: üç konu sayılır, sipariş başına tek sayım; Integration testi | ✓ | İki `AutomaticMomentsTests.DeletionDay_*_T412_KK3` (üç konu bekliyor → IBAN kalır, günü boşalır; gönderilmiş/geçersiz → silinir, üç imha kaydı); tek yükleme `LoadRefundIbanUsesAsync` |
| KK4: tam dönülmüş kalem S8'e girmez; S9 açılır | ✓ | `..._T412_KK4` (eski `..._KK8_B1` beklentisi tersine döndü) |
| KK5: ret sonrası kapanış | ✓ | `..._T412_KK5` |
| KK6: iki sınırın iki yanı testli; `06` sütun notu | ✓ | İki `..._T412_KK6` testi (64/65, 100/101); `06 §4.17` `OtherCarrierName`, `TrackingNumber` |
| KK7: üç geçersiz biçim reddedilir | ✓ | `FieldRuleTests` ve `MembershipTests` ret teorisi |
| KK8: kanca testinde önekli sır yakalanır | ✓ | `test-pre-commit.sh` dört `check 1` satırı |
| KK9: kümenin tek tanımı | ✓ (düzeltme 1) | `PaymentApproval`; tümleyen yazan iki yer ona bağlandı, yasak çift denetimi `TransitionTable.IsPairAllowed`; tarama ve beş yeni ihlal örneği teorisi: `TheScan_CatchesTheComplementAsAnOrPattern_KK9` (5), `…AsAnAndNotPattern_KK9` (2), `…AsANotEqualChain_KK9` (5, çok satırlı dahil), `…AsAnEqualChain_KK9` (3), `…AsAPairList_KK9` (2); dar kalma: `TheScan_LeavesASingleStatusAndOtherRules_KK9` (7: tek durum, Ö2 tablo satırı, `IsTerminal`, deyim/blok sınırı, `\|\|` ile bağlı `!=`) |
| `09 §25` ilgili satırlar | ✓ | Yeni tavan §3.5 tablosunda (`ShipmentLimits`); aynı işi yapan tek işlev (`PaymentApproval`, `InvalidEmailAttribute`, `RefundIbanUses`) çağrılıyor; modül sınırı korunuyor (`ShipmentLimits`, `PaymentApproval` `Contracts`'ta; mimari testler yeşil); eşzamanlılık: bekçi sırası ve kök yükleme kilit altında; göç yok, yeni paket yok, yeni ayar yok |
| Güvenlik taraması | ✓ | Sır yok (yeni test satırlarındaki değerler uydurma, betik testinin kendi örnekleri); yeni dış bağımlılık yok; girdi doğrulaması sıkılaştı (kargo tavanı, e-posta nokta kuralı); sır bekçisi kaçağı kapandı; yetki etkisi yok |
| `12 §6.1` | ✓ | KK'lerin hepsi ✓, kısmi yok; `12`'de T412'ye bağlı satır yok (`grep T412` boş); mini güvenlik kontrolü üstte |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | Önek soyma yalnız satır başındaki `-e`/`--env` ve ardından boşluk ya da `=`; değer denetimi değişmedi; `PaymentApprovalTests` taramasının istisnası tek dosya ve dar desen |
| Sık FAIL (2) tarama çağrı biçimini kaçırıyor | ✓ | Önek dört biçimde (`-e "`, `-e `, `--env "`, `--env=`) sınandı; küme taraması her sıra ve biçimi arar; düzeltme 1: tümleyen biçimi (beş kalıp, sıra/boşluk/satır kırılımı) her kalıp için ayrı ihlal örneğiyle |
| Sık FAIL (tek kural evi) kümeyi kendi yazan yer | ✓ | Düzeltme 1: `git grep -nE "PaymentStatus\.(Pending\|Failed)" src/` tek tek okundu; küme ya da tümleyeni kalan yer yok (tarama yeşil) |
| Sık FAIL (3) DB default / FK dizini | ✓ | Göç yok; `CustomerId` dizini zaten var, `06` İndeksler satırı yeni kullanımı anar |
| Sık FAIL (4) aynı kuralı anan öteki yerler | ✓ | E-posta kuralı: `04` 7.3.2.1 + kayıt testi; eksen: `09 §15.4`, `06 §4.17` iki yer; tavan: `09 §3.5`, `06 §4.17`; `IsRefundIbanInUseAsync` ve `OfMemberEmail` adı `src`, `tests` ve L1–L3 belgelerinde kalmadı (`grep`; tarihçe kayıtları hariç) |
| Sık FAIL (5) belge kuralını değiştiren 09 metni karar kaydısız | ✓ | Düzeltme 2: `09 §10.2` m.3 yeni sipariş muafiyeti → K-1605 (`05 §7.3.2`, `06 §2.6.4` aynı PR'da hizalı); `09 §10.2` m.3 → K-1581; §3.5 → K-1586; §15.4 m.5, `02 §7.1.6`, `04` 7.3.2.1, `06 §4.26` → F3 kapısı kararları (D-808 (a), D-905 (a), D-379 (a), D-915 (a); `GATE_CHECK_F3`) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 6018/6018 | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | `origin/main` `0b0d1828` birleştirilmiş ağaçta, 31 sn |
| Integration | ✓ 3502 (parçalı, tamamı) | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-class …` (Model `--max-threads 3`) | PostgreSQL 18 `track_commit_timestamp=on`. Bu tur, birleşik ağaçta: Web ikinci yarısı 376 (+ T407'nin değiştirdiği `EmailCaptureRegistrationTests`, `SecretFilesTests`), Model 1407. Önceki turlarda: SignedEndpoints+Payment+FileSecrets+Concurrency+Logging+Security+Database+Outages 230; Disposal+Administration+Commands+Jobs 277; Catalog+Files+CorporateContent 369; Ordering 595; Web ilk yarısı 248 |
| Betik | ✓ (parçalı) | `scripts/git-hooks/test-pre-commit.sh` | Yeni altı satır yerelde koşuldu; tam betik Windows'ta > 400 sn sürer. CI bu betiği koşmaz (`.github/` içinde çağıran adım yok; tur 1 notuyla düzeltildi); validator tur 1'de tam betiği yerelde koştu: 216/0 |
| Unit (düzeltme 1) | ✓ 6142/6142 | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | `origin/main` `06eb0362` birleşik ağaçta (Architecture, Consistency, Ordering dahil) |
| Integration (düzeltme 1) | ✓ 620 + 69 | `--filter-namespace` Ordering; Administration (PanelHome dahil) | PostgreSQL 18 `track_commit_timestamp=on`, konteyner `shopfolio-t412-fix1-pg` |
| Unit (düzeltme 2) | ✓ 5850 + 292 | `--filter-not-trait "Consistency=true"`; `--filter-trait "Consistency=true"` | `origin/main` `06eb0362` dal tabanında (birleştirme gerekmedi); Architecture dahil |
| Integration (düzeltme 2) | ✓ 30 + 611 | `--filter-class *ConcurrencyTests`; Ordering'in 29 sınıfı üç parçada (130, 251, 230) | PostgreSQL 18 `track_commit_timestamp=on`, konteyner `shopfolio-t412-fix2-pg`; host kaydı değişmedi (yalnız test ve belge), tam Integration gerekmedi |
| Contract | — | — | Bu task'ta yok |

`dotnet format --verify-no-changes --include` (değişen src ve test dosyaları): temiz. Derleme 0 uyarı.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-10, head `5b76129a`; `origin/main` `0b0d1828` dal tabanında, birleştirme gerekmedi). Kriter 1–8 ve KK1–KK8 ✓; **kriter 9 / KK9 ~ Kısmi**. Dal CI'ı yeşil (run 42808755298), ana dal son üç run yeşil. Yerelde: derleme 0 uyarı, `dotnet format` üç proje temiz, Unit 6018/6018, betik testleri 4/4, `test-pre-commit.sh` tam 216/0, Integration dokunulan ve komşu sınıflar 303 + 341 (dokuz `*_T412_*` testi dahil) yeşil; `--locked-mode` restore 0. Güvenlik temiz. |
| Bulgu sayısı | 1 — **B1** (S1, DÜŞÜK; KK9): "ödeme onaylanmış" kümesini okuyan iki yer `PaymentApproval`'ı kullanmıyor, kümeyi tümleyeniyle yeniden yazıyor: `DeliveryMarkRules.IsPaymentOpenToMark` (`payment is not (PaymentStatus.Pending or PaymentStatus.Failed)`; kuralı `02 §3.20.9` "İşaret ancak ödeme onaylanmışken konur") ve `PendingWorkReader` `ServiceUnmarked` sorgusu (`o.PaymentStatus != PaymentStatus.Pending && o.PaymentStatus != PaymentStatus.Failed`). Değerler bugün eşit; yeni bir `PaymentStatus` değeri eklenince bu iki yer kümeden ayrışır — kriter 9'un önlemek istediği kayma. `PaymentApprovalTests` taraması yalnız üç durumu aynı satırda yazan kopyayı arar, tümleyen biçimini görmez. |
| Düzeltme gerekli mi | Evet — iki yer `PaymentApproval.IsApproved` / `PaymentApproval.Statuses` çağırır; tarama tümleyen biçimini (`is not (Pending or Failed)`, `!= Pending && != Failed`) de yakalar (izinli çift tablosu ve `03 §1.4.1` yasak çift denetimi gibi başka kuralın satırları gerekçeyle ayrılır). |
| Bloke-etmeyen not (K) sayısı | 1 — **K1** (D-1036'ya forward): sır bekçisi `-e`/`--env` önekini yalnız satır başında soyar; aynı satırdaki ikinci önekli atama ve tek satırlık `docker run … -e "API_KEY=…"` yakalanmaz (validator sondası). D-80'in tarif ettiği devam satırı biçimi kapandı. |
| Doğrulama durumu (tur 2) | ✗ FAIL — tur 2 (2026-11-10, head `6e805767`; `origin/main` `06eb0362` dal tabanında, birleştirme gerekmedi). Kriter 2–9 ve KK2–KK9 ✓ (tur 1 B1 kapandı: `DeliveryMarkRules`, `PendingWorkReader` `PaymentApproval`'ı çağırır, tarama tümleyeni yakalar); **kriter 1 / KK1 ~ Kısmi**. Dal CI'ı yeşil (run 42821586998, dokuz iş, tam Integration dahil), ana dal son üç run yeşil (42817913057, 42817913065, 42804053557). Yerelde: derleme `-warnaserror` 0 uyarı, `dotnet format` üç proje temiz, Unit 5850 + Consistency 292 yeşil, betik testleri 4/4, `test-pre-commit.sh` tam 216/0, Integration dokunulan sınıflar 316 + 123 ve komşu sınıflar 225 yeşil; `--locked-mode` restore 0; `12_matris_denetle.py` hata 0 (`12`'de T412 satırı yok). Güvenlik temiz. |
| Bulgu sayısı (tur 2) | 1 — **B1** (S1, DÜŞÜK; KK1 "`09 §10.2` m.3 kodla eşit"): `OrderVersionGuard` bu kayıtta eklenen (Added) siparişin kayıtlarını kilit ve artış denetiminden muaf tutar (`IsNew` → `continue`, kilit denetiminden önce); `09 §10.2` m.3'ün yeni paragrafı bu muafiyeti yazmaz, tersine "kilidi tutulmayan siparişe yazım, sürümü artırılmış olsa da … reddedilir" der. Muafiyet yük taşır: aynı sepetten yeni sipariş onayı önceki ödenmemiş siparişi kilidiyle iptal eder (`OrderConfirmation` → `CancelEarlierUnpaidOrdersAsync`) ve yeni siparişi aynı işlemde, o kilit tutulurken yazar (`LockSequence.LockedOrders` işlem boyunca dolu) — muafiyet olmasa yeni siparişin yazımı kilit iletisiyle reddedilirdi. T412 öncesi paragraf "önce sürüm artışına bakar" diyordu ve `VersionScope.IsRaised` eklenen kökü artmış sayıyordu (kapsıyordu); sıra değişince muafiyet ayrı bir dal oldu ama belgeye girmedi. `06 §2.6.4` ve `05`'te de yazılı değil. Yapım raporunun öz denetim satırı "paragraf kodla aynı sırayı anlatır (ret → yeni sipariş → kilit → artış)" der — paragrafta "yeni sipariş" adımı yok. |
| Düzeltme gerekli mi (tur 2) | Evet — `09 §10.2` m.3 paragrafına muafiyet cümlesi: bu kayıtta eklenen siparişin kayıtları kilit ve artış denetiminin dışındadır (satırı kesinleşmeden başka işlem görmez; ör. aynı sepetten yeni siparişin onayı önceki siparişin kilidi altında yazılır); kilit denetimi siparişi bilinen ve var olan kayıtta işler. Kod değişikliği gerekmez. Tur 2, iki iterasyon sınırının sonudur (`12 §8.5`): eskalasyon (`INSTRUCTIONS.md` §3.3). |
| Bloke-etmeyen not (K) sayısı (tur 2) | 0 |
| Doğrulama durumu (tur 3) | ✓ PASS — tur 3 (2026-11-10, head `08878fb6`; `origin/main` `398deeaa` dalda, ek birleştirme gerekmedi). Kriter 1–9 ve KK1–KK9 ✓, `09 §25` ilgili satırlar ✓, güvenlik temiz. Tur 2 B1 kapandı: `09 §10.2` m.3 yeni sipariş muafiyetini yazar (K-1605), `OrderVersionGuard.Check` ile satır satır eşit. Dal CI'ı yeşil (run 42836235245, dokuz iş, tam Integration dahil), ana dal son üç run yeşil (42817913057, 42817913065, 42804053557). Yerelde: derleme `-warnaserror` 0 uyarı, `dotnet format` üç proje temiz, Unit 6154/6154 (Consistency ve Architecture dahil), betik testleri 4/4, `test-pre-commit.sh` 216/0, Integration dokunulan ve komşu 32 sınıf 791 + 167 yeşil (PostgreSQL 18, `track_commit_timestamp=on`); paket betiğinin 1/7 deseni dört önekli satırı yakaladı (sonda); `--locked-mode` restore 0. `12`'de T412'ye eşlenen satır yok (Dokümanlar satırında KP-n yok). |
| Bulgu sayısı (tur 3) | 0 |
| Düzeltme gerekli mi (tur 3) | Hayır |
| Bloke-etmeyen not (K) sayısı (tur 3) | 0 (tur 1 K1 D-1036'da açık) |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T412-f3-kapanis-duzeltmeleri`
- **Commit:** squash `553222f` (#517) (F4 gate'te dolduruldu)
- **PR:** #517
- **CI:** yönetici izler
- **Post-merge run'lar:** squash `553222f` (#517) — CI 42845641513 success, Publish 42845641664 success (F4 gate'te dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok (plan, kota, paket sürümü, API sözleşmesi yok) | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Yok | — | — |

## Notlar

- **Working tree:** temiz; `core.hooksPath` = `scripts/git-hooks`. Dal üç işçi turunda yazıldı (bağlam/süre bölünmesi); `origin/main` iki kez birleştirildi (`8e1e6215`, `0b0d1828`), çakışma yok.
- **Repo memory:** MEMORY.md'ye dokunulmadı.
- **Düzeltme 1 (2026-11-10):** tur 1 B1 kapandı (yukarıda "Düzeltme 1"). Kriter 1–9 ve KK1–KK9 yeniden denetlendi: KK1–KK8'in kodu bu turda değişmedi, testleri birleşik ağaçta yeşil (Integration Ordering 620, Administration 69; Unit 6142); KK9 ✓. `origin/main` (`06eb0362`, T401) dalda, çakışma yok. Derleme `-warnaserror` 0 uyarı, `dotnet format --verify-no-changes` üç proje temiz. Yeni backlog kalemi yok.
  - ⚠ (g) Talimat `OrderTransitions` yasak çift denetimini taramanın gerekçeli istisnası olarak bırakmaya izin veriyordu; istisna yerine denetim çift tablosunun kendisini okur (`TransitionTable.IsPairAllowed(OrderStatus.Delivered, …)`; Teslim edildi satırı Paid, PartiallyRefunded, Refunded → reddedilen yine Pending ve Failed). Davranış aynı, tarama istisnası `TransitionTable.cs` izinli çift satırında kaldı.
- **Düzeltme 2 (2026-11-10):** tur 2 B1 kapandı (yukarıda "Düzeltme 2"). Kriter 1–9 ve KK1–KK9 yeniden denetlendi: `src` bu turda değişmedi (yalnız `ConcurrencyTests.cs` ve dört belge); KK2–KK9'un testleri yeşil (Ordering 611, Unit 5850 + Consistency 292); KK1 ✓. `origin/main` `06eb0362` dal tabanında, birleştirme gerekmedi. Derleme `-warnaserror` 0 uyarı, `dotnet format --verify-no-changes` üç proje temiz. Yeni backlog kalemi yok.
  - ⚠ (h) Talimat 05/07'de aynı kuralı anan yeri tutarlı tutmayı istiyordu; `05 §7.3.2` ve `06 §2.6.4` "her işlem siparişin satır kilidini alır" der — yeni sipariş istisnası iki cümleye de kısa bir ekle yazıldı (doğrulayıcının etkilenen komşu madde olarak andığı `06 §2.6.4` dahil); `07`'de karşılığı yok. Belge sürüm başlıkları değiştirilmedi (T412'nin önceki belge düzeltmeleri gibi).
- **⚠ Kolay kararlar (kurala en uygun seçenekle):**
  - (a) Firma kalem iptalinin kendiliğinden kapanışı (`FirmCancellation.cs`, "açık adet kalmadı") ve `EvaluateClosureAsync` kapanan adedi (`ClosedQuantitiesAsync`) okumayı sürdürür — `02 §7.1.6` dönülen adedin kalemi kapatmadığını söyler (K-1077; `ItemRecordsTests.TheFirmsItemCancellation_OfTheUnitsLeftBesideARescindedOne_DoesNotCloseTheOrder_KK8_B1` korunur); KK4 yalnız S5/S8, teslim işareti ve S9'un açık kalem okumasını değiştirir.
  - (b) Z-38'de "teslimden önce" ayrımı kalemin teslim tarihi ve `IsDeliveredBeforeDeclaration` ile yapılır (`AfterSalesPeriodRules.IsAfterDelivery`, beyanın silme günü koyan kuralıyla aynı). Kalem isteğinin kayıtları için geri ödeme hattındaki "IBAN geldikten sonra kapanan kalemin geri ödemesi" inceliği konu listesinde yok: her kaydın geri ödemesi tamamlanmalı (daha korumacı — IBAN gereğinden erken silinmez).
  - (c) `FindRefundIbanUseAsync` kilit öncesi okumada da tam yükleme yapar (kilitten sonra bir kez daha); küme sipariş başına sınırlı.
  - (d) Bekçi kilitli sipariş hiç yokken sessiz kalır (kayıt kendi koşullu güncellemesi, test tohumu); K-1581 bu yolu değiştirmedi, sınıf yorumu ve `09 §10.2` m.3 böyle yazar.
  - (e) `MembershipTests.Register_TakesTheAddressTheProjectsOneEmailRuleAccepts` `a..b@example.com` ile eski kuralı sınıyordu; adres `a.b+etiket@example.com` oldu, eski adres ret teorisine geçti.
  - (f) `04` 7.2.7.2 "Kısa metin" hücresine tavan yazılmadı; K-1586 tavanın evini `09 §3.5` ve `06` sütun notu olarak koyar.
