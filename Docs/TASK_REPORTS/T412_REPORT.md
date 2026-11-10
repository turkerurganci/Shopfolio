# T412 — F3 kapanış düzeltmeleri: sipariş bekçisi, satış sonrası kapanış ve IBAN kullanımı

**Faz:** F4 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-10

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
- **Backlog:** D-80, D-95, D-309, D-379, D-808, D-847, D-896, D-903, D-905, D-910, D-915 yerinde ✅. Yeni kalem yok.

## Etkilenen Modüller / Dosyalar

- Ordering: `OrderVersionGuard.cs`, `OrderAxes.cs`, `OpenUnpaidOrderLimit.cs`, `UnpaidCancellationLimit.cs`, `OrderPlacementLimits.cs`, `OrderConfirmation.cs`, `OrderTransitions.Shipment.cs`, `OrderTransitions.RefundLine.cs`, `RefundIbanDeletionDay.cs`, `OrderExport.cs`, `OrderOperationFacts.cs`, `SalesPeriodReader.cs`, `Contracts/PaymentApproval.cs` (yeni), `Contracts/ShipmentTransitionRequests.cs`
- AfterSales: `AfterSalesModule.RefundIbanUses.cs` (yeni), `AfterSalesModule.RefundIbans.cs`, `AfterSalesModule.RefundLine.cs`, `AfterSalesModule.ItemRecords.cs`, `AfterSalesModule.PendingWork.cs`, `Contracts/RefundIbanUse.cs`
- Payment: `PaymentModule.cs`, `Contracts/IPaymentModule.cs`; Administration: `SalesSummaryRules.cs`; Infrastructure: `Validation/FieldRules.cs`
- Betikler: `scripts/git-hooks/pre-commit`, `scripts/git-hooks/test-pre-commit.sh`
- Testler: Integration `Concurrency/ConcurrencyTests.cs`, `Ordering/AutomaticMomentsTests.cs`, `AxesTieTests.cs`, `ItemRecordsTests*.cs`, `OpenUnpaidOrderLimitTests.cs`, `OrderCorrectionTests.Interventions.cs`, `OrderPlacementLimitTests.cs`, `RefundAmountTests.cs`, `ShipmentTransitionsTests.cs`, `Model/MembershipTests.cs`; Unit `Ordering/PaymentApprovalTests.cs` (yeni), `Validation/FieldRuleTests.cs`
- Belgeler: `02 §7.1.6`, `04` 7.3.2.1, `06 §4.17` (üç satır + İndeksler), `06 §4.26`, `09 §3.5`, §10.2 m.3, §15.4 m.5; `DEFERRED_BACKLOG.md` (11 satır); `STATUS_CHANGELOG.d/T412.md`; `IMPLEMENTATION_STATUS.md` (T412 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kilitsiz siparişe yazım her durumda kilit iletisi; kökü bilinmeyen kayıt her durumda ret; kökünü yüklemeyen işlem kökü yükler; `09 §10.2` m.3 | ✓ | `OrderVersionGuard.Check`; `ConcurrencyTests.ARecordWhoseRootIsNotLoaded_IsRefused_EvenWithOneLockedOrder_AndItsOwnRaisedOrder_T412_KK1`, `AWriteToAnUnlockedOrder_GetsTheLockMessage_EvenWhenItsVersionIsRaised_T412_KK1`; kök yükleme `ReportTransferFailedAsync`, `RefundIbanDeletionDay.ProcessAsync`; tam Integration yeşil (kökü yüklemeyen başka yol patlamadı); `09` satır 913 |
| 2 | E-posta ekseni `CustomerId`; `09 §15.4` m.5 | ✓ | `OrderAxes.OfMember`; `OpenUnpaidOrderLimitTests.TheEmailAxis_IsTheAccount_NotTheContactEmailTheOrderCarries_T412_KK2`, `OrderPlacementLimitTests` (müşteri satırlı tohum, imza `Guid?`); `09 §15.4` m.5, `06 §4.17` |
| 3 | `FindRefundIbanUseAsync` kargodan önceki caymayı, hizmet caymasını, istek bağlantılı geri ödemeyi sayar; küme sipariş başına bir kez | ✓ | `RefundIbanUses.SubjectsOf`, `LoadRefundIbanUsesAsync`; `AutomaticMomentsTests.DeletionDay_KeepsTheIban_ForAWithdrawalBeforeTheDelivery_AServiceWithdrawal_AndARequestsRefund_WhileTheirRefundsArePending_T412_KK3`, `DeletionDay_DeletesTheIban_OnceTheRefundsOfThoseThreeSubjectsWereSentOrVoided_T412_KK3`; `DeleteUnusedRefundIbansAsync` döngüsünde sorgu yok |
| 4 | Tam dönülmüş kalem S8'den ve teslim işaretinden çıkar; S9 kapanışı | ✓ | `OpenItemsAsync` → `UnavailableQuantitiesAsync`; `ItemRecordsTests.AReturnedShipmentWhoseUnitsAreAllRescinded_IsNotResent_AndItsS9IsTheClosure_NotACancellation_T412_KK4`; `02 §7.1.6` |
| 5 | Ret de "mal dönmedi" kapanışını açar | ✓ | `AddReturnRejectionAsync` (`includeClosed: true`, `ReopenClosuresBeyondAwaitingAsync`); `ItemRecordsTests.Rejection_AfterTheClosure_RejectsTheClosedUnits_AndReopensTheClosure_T412_KK5`; `06 §4.26` |
| 6 | `TrackingNumber` ≤ 64, `OtherCarrierName` ≤ 100; `09 §3.5` sabitleri; aşım `InvalidInput` | ✓ | `ShipmentLimits`; `ShipmentTransitionsTests.S5_TakesTheTrackingNumberAndTheCompanyName_AtTheirCaps_AndRefusesOneCharacterMore_T412_KK6`, `OrderCorrectionTests.TheCarrierCorrection_TakesTheCaps_AndRefusesOneCharacterMore_T412_KK6` (sınırın iki yanı, beklenen değer sabitten); `09 §3.5`, `06 §4.17` |
| 7 | Yerel kısım noktayla başlamaz, bitmez, ardışık nokta yok; `04` 7.3.2.1 | ✓ | `InvalidEmailAttribute`; `FieldRuleTests` (`.ad@`, `ad.@`, `a..b@`), `MembershipTests.Register_WithAWrongAddress_IsRefusedAndNamed` (üç biçim); `04` 7.3.2.1 |
| 8 | Kanca ve paket betiğinin taraması `-e "` önekli satırı atlamaz | ✓ | `pre-commit` önek soyma; `test-pre-commit.sh` `sh -e quoted/bare`, `sh --env quoted`, `sh --env= form` (yakalanır), `sh -e placeholder`, `sh -e _FILE short path` (geçer); `package-build.sh` 1/7 deseni önekli satırı zaten görür |
| 9 | Ödeme kümesi tek yerde; okuyan her yer onu kullanır | ✓ | `PaymentApproval`; `PaymentApprovalTests` (küme sabit + kaynak taraması ikinci tanımı yakalar, tek istisna `TransitionTable.cs` izinli çift satırı) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: bekçinin iki yolu ve kök yükleme testli; `09 §10.2` m.3 kodla eşit | ✓ | İki `ConcurrencyTests.*_T412_KK1` testi (kök yüklenmemiş → ret; kilitsiz sipariş artırılmış/artırılmamış → kilit iletisi); kök yükleyen yollar tam Integration'da (Ordering 595, Concurrency) yeşil; paragraf kodla aynı sırayı anlatır (ret → yeni sipariş → kilit → artış) |
| KK2: e-posta ekseni `CustomerId` | ✓ | `OrderAxes.OfMember`; iletişim e-postası hesabınkinden farklı sipariş testi (`..._T412_KK2`) |
| KK3: üç konu sayılır, sipariş başına tek sayım; Integration testi | ✓ | İki `AutomaticMomentsTests.DeletionDay_*_T412_KK3` (üç konu bekliyor → IBAN kalır, günü boşalır; gönderilmiş/geçersiz → silinir, üç imha kaydı); tek yükleme `LoadRefundIbanUsesAsync` |
| KK4: tam dönülmüş kalem S8'e girmez; S9 açılır | ✓ | `..._T412_KK4` (eski `..._KK8_B1` beklentisi tersine döndü) |
| KK5: ret sonrası kapanış | ✓ | `..._T412_KK5` |
| KK6: iki sınırın iki yanı testli; `06` sütun notu | ✓ | İki `..._T412_KK6` testi (64/65, 100/101); `06 §4.17` `OtherCarrierName`, `TrackingNumber` |
| KK7: üç geçersiz biçim reddedilir | ✓ | `FieldRuleTests` ve `MembershipTests` ret teorisi |
| KK8: kanca testinde önekli sır yakalanır | ✓ | `test-pre-commit.sh` dört `check 1` satırı |
| KK9: kümenin tek tanımı | ✓ | `PaymentApproval`; kaynak taraması `PaymentApprovalTests` |
| `09 §25` ilgili satırlar | ✓ | Yeni tavan §3.5 tablosunda (`ShipmentLimits`); aynı işi yapan tek işlev (`PaymentApproval`, `InvalidEmailAttribute`, `RefundIbanUses`) çağrılıyor; modül sınırı korunuyor (`ShipmentLimits`, `PaymentApproval` `Contracts`'ta; mimari testler yeşil); eşzamanlılık: bekçi sırası ve kök yükleme kilit altında; göç yok, yeni paket yok, yeni ayar yok |
| Güvenlik taraması | ✓ | Sır yok (yeni test satırlarındaki değerler uydurma, betik testinin kendi örnekleri); yeni dış bağımlılık yok; girdi doğrulaması sıkılaştı (kargo tavanı, e-posta nokta kuralı); sır bekçisi kaçağı kapandı; yetki etkisi yok |
| `12 §6.1` | ✓ | KK'lerin hepsi ✓, kısmi yok; `12`'de T412'ye bağlı satır yok (`grep T412` boş); mini güvenlik kontrolü üstte |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | Önek soyma yalnız satır başındaki `-e`/`--env` ve ardından boşluk ya da `=`; değer denetimi değişmedi; `PaymentApprovalTests` taramasının istisnası tek dosya ve dar desen |
| Sık FAIL (2) tarama çağrı biçimini kaçırıyor | ✓ | Önek dört biçimde (`-e "`, `-e `, `--env "`, `--env=`) sınandı; küme taraması her sıra ve biçimi arar |
| Sık FAIL (3) DB default / FK dizini | ✓ | Göç yok; `CustomerId` dizini zaten var, `06` İndeksler satırı yeni kullanımı anar |
| Sık FAIL (4) aynı kuralı anan öteki yerler | ✓ | E-posta kuralı: `04` 7.3.2.1 + kayıt testi; eksen: `09 §15.4`, `06 §4.17` iki yer; tavan: `09 §3.5`, `06 §4.17`; `IsRefundIbanInUseAsync` ve `OfMemberEmail` adı `src`, `tests` ve L1–L3 belgelerinde kalmadı (`grep`; tarihçe kayıtları hariç) |
| Sık FAIL (5) belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09 §10.2` m.3 → K-1581; §3.5 → K-1586; §15.4 m.5, `02 §7.1.6`, `04` 7.3.2.1, `06 §4.26` → F3 kapısı kararları (D-808 (a), D-905 (a), D-379 (a), D-915 (a); `GATE_CHECK_F3`) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 6018/6018 | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | `origin/main` `0b0d1828` birleştirilmiş ağaçta, 31 sn |
| Integration | ✓ 3502 (parçalı, tamamı) | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-class …` (Model `--max-threads 3`) | PostgreSQL 18 `track_commit_timestamp=on`. Bu tur, birleşik ağaçta: Web ikinci yarısı 376 (+ T407'nin değiştirdiği `EmailCaptureRegistrationTests`, `SecretFilesTests`), Model 1407. Önceki turlarda: SignedEndpoints+Payment+FileSecrets+Concurrency+Logging+Security+Database+Outages 230; Disposal+Administration+Commands+Jobs 277; Catalog+Files+CorporateContent 369; Ordering 595; Web ilk yarısı 248 |
| Betik | ✓ (parçalı) | `scripts/git-hooks/test-pre-commit.sh` | Yeni altı satır yerelde koşuldu; tam betik Windows'ta > 400 sn, CI'da tam koşar |
| Contract | — | — | Bu task'ta yok |

`dotnet format --verify-no-changes --include` (değişen src ve test dosyaları): temiz. Derleme 0 uyarı.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T412-f3-kapanis-duzeltmeleri`
- **Commit:** son commit — T412: F3 kapanış düzeltmeleri: sipariş bekçisi, satış sonrası kapanış ve IBAN kullanımı
- **PR:** *(PR açılınca)*
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

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
- **⚠ Kolay kararlar (kurala en uygun seçenekle):**
  - (a) Firma kalem iptalinin kendiliğinden kapanışı (`FirmCancellation.cs`, "açık adet kalmadı") ve `EvaluateClosureAsync` kapanan adedi (`ClosedQuantitiesAsync`) okumayı sürdürür — `02 §7.1.6` dönülen adedin kalemi kapatmadığını söyler (K-1077; `ItemRecordsTests.TheFirmsItemCancellation_OfTheUnitsLeftBesideARescindedOne_DoesNotCloseTheOrder_KK8_B1` korunur); KK4 yalnız S5/S8, teslim işareti ve S9'un açık kalem okumasını değiştirir.
  - (b) Z-38'de "teslimden önce" ayrımı kalemin teslim tarihi ve `IsDeliveredBeforeDeclaration` ile yapılır (`AfterSalesPeriodRules.IsAfterDelivery`, beyanın silme günü koyan kuralıyla aynı). Kalem isteğinin kayıtları için geri ödeme hattındaki "IBAN geldikten sonra kapanan kalemin geri ödemesi" inceliği konu listesinde yok: her kaydın geri ödemesi tamamlanmalı (daha korumacı — IBAN gereğinden erken silinmez).
  - (c) `FindRefundIbanUseAsync` kilit öncesi okumada da tam yükleme yapar (kilitten sonra bir kez daha); küme sipariş başına sınırlı.
  - (d) Bekçi kilitli sipariş hiç yokken sessiz kalır (kayıt kendi koşullu güncellemesi, test tohumu); K-1581 bu yolu değiştirmedi, sınıf yorumu ve `09 §10.2` m.3 böyle yazar.
  - (e) `MembershipTests.Register_TakesTheAddressTheProjectsOneEmailRuleAccepts` `a..b@example.com` ile eski kuralı sınıyordu; adres `a.b+etiket@example.com` oldu, eski adres ret teorisine geçti.
  - (f) `04` 7.2.7.2 "Kısa metin" hücresine tavan yazılmadı; K-1586 tavanın evini `09 §3.5` ve `06` sütun notu olarak koyar.
