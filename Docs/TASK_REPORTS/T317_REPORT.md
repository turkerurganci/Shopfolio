# T317 — Ödeme durumu geçişleri: ödeme ekseni

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 3 ✓ PASS; merge yardımcıda) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Geçiş biriminin ödeme ekseni işlemleri (`OrderTransitions.Payment.cs`, `OrderTransitions`'ın parçası; her durum yazımı çekirdeğin `ApplyAsync`'inden geçer, yan etkiler aynı işlemde):
  - `ApplyCardPaymentAsync` — kartta sağlayıcının başarı bildirimi ve son sorgunun olumlu cevabı aynı yoldan: Ö1 sistem, açık fiziksel kalem varsa S1 aynı istekte; ödeme kaydı (Ö1 geçiş kaydına bağlı) ve B-4. Aynı işlem kimliği ikinci kez gelirse hiçbir şey yazılmaz (`IsAlreadyApplied`).
  - Ödenmesi kalmamış siparişe gelen kart ödemesi (iptal edilmiş ya da yeni işlem kimliğiyle ödenmiş): ödeme kaydında Ö1'siz durur, sistem `Refund.ForLatePayment` + `CardRefund` iş satırıyla geri öder; eksenler kıpırdamaz, sürüm bir kez artar.
  - `MarkTransferPaidAsync` — havalede yöneticinin "ödendi" işareti: geri alınamaz onay ve ekrandaki sürüm zorunlu; Ö1 yönetici, S1 sistem (iki istek, aynı kilit, sürüm bir kez artar — D-291 (a)); ödeme kaydı ve B-4.
  - ~~`CancelUnpaidOrderAsync`~~ — düzeltme turu 3'te kaldırıldı (B1): ödenmemiş siparişin iptalinin tek yolu T316'nın `CancelOrderAsync`'idir (Alındı + Bekliyor'da S3 + Ö2 tek istekte, kim yaparsa yapsın; yöneticide geri alınamaz onay, sürüm ve "Diğer ⇔ not" denetimi; B-7). Ö2'nin ödeme ekseninde ayrı yöntemi yoktur.
  - `ApplyCardRefundOutcomeAsync` — kart iadesinin sonucu: gerçekleşince gönderilen toplam ödenen tutara ulaştıysa Ödendi'den Ö4 / Kısmen geri ödendi'den Ö5, altındaysa Ödendi'den Ö3; B-8 her gerçekleşen geri ödemede, F-4 yalnız sistemin kendiliğinden kart iadesinde (firmanın yeniden denemesi değilse). Başarısız ya da bilinmeyen sonuçta eksen aynı, bildirim yok, panel işareti iade kaydından düşer.
  - `ProcessTransferRefundAsync` — yöneticinin havale geri ödemesini işlemesi (onay + sürüm): Ö3/Ö4/Ö5 aynı kuralla, B-8 (F-4 yok).
- Ödeme modülü yüzü (`IPaymentModule`/`PaymentModule`): `IsPaymentRecorded`, `RecordCardPayment`, `RecordTransferMark`, `StartLatePaymentRefund`, `WriteCardRefundOutcome`, `MarkTransferRefundSent`, `SentRefundTotal`, `ReadRefundMarks`; çağıranın işlemine yazar, `SaveChanges` çağırmaz. Kayıtlar `RefundFacts`, `RefundMarks`.
- `TransitionTable`: `FindPayment(from, to)` (Ö3/Ö4/Ö5 ikinci bir listeden değil tablodan okunur), `IsTerminal(PaymentStatus)`.
- `TransitionRequest` yorumu D-291 (a)'ya göre düzeltildi; `PaymentRecord` yorumları `06 §4.32` ile eşitlendi (geçiş kaydı Ö1'dir).
- Sözleşme kayıtları (`Ordering/Contracts/PaymentTransitions.cs`): `CardPaymentSucceeded`, `TransferPaidMark`, `CardRefundOutcome`, `TransferRefundProcessing`, `PaymentTransitionResult` (enum eklenmedi; ret bir değerdir, işlem koşucusu geri alır).
- Bu turda (3. işçi): test derlemesi (eksik `using`), dijital kalemin adet kuralına uyan test tohumu, `09 §3.1` madde 5'e göre kaynak koddaki null-forgiving `!` kullanımlarının kaldırılması (`Refused(TransitionOutcome)`, `[NotNullWhen(true)]`, `OfType<RefundRecord>()`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/`: `OrderTransitions.Payment.cs` (yeni), `OrderTransitions.cs`, `OrderTransitions.Shipment.cs` (düzeltme turu 3: `StartPreparationAsync` yalnız S1), `TransitionTable.cs`, `TransitionRequests.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/PaymentTransitions.cs` (yeni)
- Düzeltme turu 3 testleri: `tests/Shopfolio.IntegrationTests/Ordering/PaymentTransitionsTests.cs` (KK4 `CancelOrderAsync` üzerinden, 33), `Ordering/ShipmentTransitionsTests.cs` (S1 testleri yeni imzaya; karışık sipariş testi kart ödemesi yoluna)
- `src/Shopfolio.Web/Modules/Payment/`: `PaymentModule.cs`, `PaymentRecord.cs`, `Contracts/IPaymentModule.cs`, `Contracts/PaymentFacts.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Ordering/PaymentTransitionsTests.cs` (yeni, 29), `Ordering/OrderingTestServices.cs` (yeni, ortak DI), `Ordering/OrderTransitionsTests.cs` (ortak DI'a bağlandı); `tests/Shopfolio.UnitTests/Ordering/TransitionTableTests.cs` (+2)
- Kayıt: bu rapor, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/DEFERRED_BACKLOG.md`, `.claude/memory/MEMORY.md`; T307, T315, T332, T366, T381 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ö1…Ö5 beyaz liste; Başarısız ve Geri ödendi terminal | ✓ | `TransitionTable.Payment` + `IsTerminal`; `ThePaymentAxisTerminalStates_AreExactlyTheStatesNoPaymentRowLeaves_T317KK1`, `MarkTransferPaid_OnAnOrderThatIsNotPending_IsRefused_AndWritesNothing_KK1`, `RefundSent_OnARefundedOrder_DoesNotLeaveTheTerminalState_KK1` |
| 2 | Ö1 kartta başarı bildirimi ya da son sorgunun olumlu cevabıyla | ✓ | `ApplyCardPaymentAsync`; `CardPayment_FromTheNotificationAndFromTheFinalQuery_AppliesO1AndS1ByTheSystem_TheRecordAndB4_KK2`, `CardPayment_ThatCameAlready_DoesNothing_KK2`, `CardPayment_OfAnOrderWithoutAPhysicalItem_AppliesO1Only_AndATransferOrderIsRefused_KK2` |
| 3 | Ö1 havalede yöneticinin işareti, geri alınamaz onay, B-4 | ✓ | `MarkTransferPaidAsync`; `MarkTransferPaid_WithoutTheConfirmation_IsRefused_AndWritesNothing_KK3`, `MarkTransferPaid_Confirmed_IsO1ByTheAdministratorAndS1ByTheSystem_OneVersionRaise_TheRecordAndB4_KK3`, `MarkTransferPaid_OnACardOrder_IsRefused_KK3` |
| 4 | Ö2 S3'le aynı anda, her iptalde, kim yaparsa yapsın | ✓ | Tek yol `CancelOrderAsync` (`OrderTransitions.Shipment.cs:35`; `ChooseCancellation` :219 Alındı + Bekliyor → S3 + Ö2); `CancelOrder_OfAnUnpaidOrder_ByAnyone_IsS3AndO2InOneOperation_WithTheReason_AndB7_KK4` (Buyer/Admin/System), `CancelOrder_OfAPaidOrder_DoesNotApplyO2_KK4` (Buyer/Admin/System) |
| 5 | Ö3/Ö4/Ö5 kart iadesi gerçekleşince ya da yöneticinin geri ödemeyi işlemesiyle | ✓ | `ApplyRefundSentAsync`; `CardRefundSucceeded_OnAPaidOrder_IsO3ForAPartAndO4ForTheWhole_KK5`, `CardRefundSucceeded_OnAPartlyRefundedOrder_IsO5ForTheRest_AndNoTransitionBelowIt_KK5`, `ProcessTransferRefund_IsTheAdministratorsO3_WithTheConfirmationAndTheVersion_AndB8WithoutF4_KK5` |
| 6 | B-8 müşteriye; sistemin kendiliğinden kart iadesinde F-4 firmaya | ✓ | `SystemCardRefundSucceeded_WritesB8ToTheCustomer_AndF4ToTheFirm_KK6`, `FirmRetryOrAdministratorsCardRefundSucceeded_WritesB8Only_KK6` |
| 7 | Başarısız/bilinmeyen iadede eksen aynı, işaret düşer, bildirim yok | ✓ | `CardRefundNotSentOrUnknown_LeavesTheAxis_SendsNoNotice_AndTheMarkFalls_KK7` (Failed, OutcomePending), `CardRefundOutcome_OfARecordThatIsNotOpen_IsRefused` |
| 8 | İptal edilmiş siparişe gelen kart ödemesi kayıtta durur, sistem geri öder | ✓ | `CardPayment_ToACancelledOrder_OrASecondOneToAPaidOrder_IsRecordedAndRefunded_TheAxesStay_KK8`, `LatePaymentRefundSucceeded_MovesNoAxis_AndWritesB8AndF4_KK8` |

### Öz denetim (bitiş kapısı 2)

> Bütün maddeler bu turda yeniden denetlendi (önceki işçilerin yazdıkları dahil). Düzeltme turu 3'te (2026-11-08) 11 satır 2112–2135'teki sekiz kriter, KK1–KK8, `09 §25` ve güvenlik taraması birleşik ağaçta (`origin/main` + dal) yeniden `grep -n` ile denetlendi; değişen satırlar KK4, B1 ve K1/D-389'dur, ötekiler aynı kanıtla geçerli ve testleri yeniden koşuldu.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–8 | ✓ | Üstteki tablo |
| KK1 tablo dışı geçiş reddedilir, terminal durum kalkmaz | ✓ | `MarkTransferPaid_OnAnOrderThatIsNotPending_IsRefused_AndWritesNothing_KK1` (Theory), `RefundSent_OnARefundedOrder_DoesNotLeaveTheTerminalState_KK1`; birim `ThePaymentAxisTerminalStates_…` beklenen kümeyi tablodan iki yönlü türetir |
| KK2 başarı bildirimi ve son sorgu Ö1'i işler | ✓ | `CardPayment_FromTheNotificationAndFromTheFinalQuery_…_KK2` |
| KK3 onaysız "ödendi" reddedilir | ✓ | `MarkTransferPaid_WithoutTheConfirmation_IsRefused_AndWritesNothing_KK3` |
| KK4 ödenmemişte S3+Ö2 aynı işlemde; ödenmişte Ö2 yok | ✓ | `PaymentTransitionsTests.cs:209` `CancelOrder_OfAnUnpaidOrder_ByAnyone_…_KK4` (iki kayıt, tek sürüm artışı, gerekçe, B-7), :232 `CancelOrder_OfAPaidOrder_DoesNotApplyO2_KK4` (alıcı/sistem `ConditionNotMet`; firma S3 tek başına, ödeme Ödendi'de kalır), :249 onaysız yönetici → `ConfirmationRequired`, :264 "Diğer" dışı notlu / "Diğer" notsuz (boş dahil) → `InvalidInput`, :276 sürümsüz → `InvalidInput`, eski sürüm → `VersionConflict`, sistem kapanış gerekçesi → `ReasonNotAllowed`; retlerde hiçbir şey yazılmaz |
| B1: ödenmemiş iptalin tek yolu (`09 §24` m.4, `09 §25` "tek işlev") | ✓ | `CancelUnpaidOrderAsync`, `UnpaidOrderCancellation`, `TryReadCanceller` silindi (`grep -rn CancelUnpaidOrder src` → yalnız `FinalPaymentQueryStep.CancelUnpaidOrder` adımı); S3+Ö2 yalnız `ChooseCancellation`'da (`grep -n "TransitionCode.O2" src` → Shipment :219, tablo ve okumalar) |
| K1/D-389: Ö1'in tek yolu | ✓ | `StartPreparationAsync(Guid, CancellationToken)` yalnız S1 (`OrderTransitions.Shipment.cs:21`); Ö1 yazan yalnız `ApplyCardPaymentAsync` :20 ve `MarkTransferPaidAsync` :64 (`grep -n "TransitionCode.O1" src` → Payment :44, :90, :96 ve tablo); `ShipmentTransitionsTests` `S1_OnAMixedOrderWithAPhysicalItem_ComesWithTheCardApprovalO1_…_KK3` ödeme kaydını da sınar |
| KK5 kısmi Ö3, tam Ö4, kalan Ö5 | ✓ | `CardRefundSucceeded_OnAPaidOrder_…_KK5`, `CardRefundSucceeded_OnAPartlyRefundedOrder_…_KK5`, `ProcessTransferRefund_…_KK5`; hedef kod `TransitionTable.FindPayment`'tan (beklenen değer tablodan türetilir) |
| KK6 B-8 ve F-4 doğru alıcıya | ✓ | `SystemCardRefundSucceeded_WritesB8ToTheCustomer_AndF4ToTheFirm_KK6`, `FirmRetryOrAdministratorsCardRefundSucceeded_WritesB8Only_KK6` |
| KK7 yanıtsız iadede eksen aynı, işaret düşer | ✓ | `CardRefundNotSentOrUnknown_LeavesTheAxis_SendsNoNotice_AndTheMarkFalls_KK7` (`ReadRefundMarksAsync`) |
| KK8 iptal edilmiş siparişin ödemesi iade edilir | ✓ | `CardPayment_ToACancelledOrder_…_KK8`, `LatePaymentRefundSucceeded_…_KK8` |
| `09 §25` doküman uyumu: okunan bölümler | ✓ | 11 T317 bloğu; `03 §1.3`, §1.2 (S1), §2.5; `02 §5.5`, §5.6.3, §5.9 (firma iptali S3 geri alınamaz onay ister — düzeltme turu 3), §6.1.2, §6.2.16, §7.2.8; `05 §4.5.3`, §4.6, §7.3.2; `06 §4.19` ("Diğer" not taşır), §4.32; `09 §9`, §3.1, §24 m.4 (tek işlev tablosu), §25 |
| `09 §25` yeni adlar belgeden, çelişki yok | ✓ | Ö1…Ö5, B-4/B-7/B-8/F-4, `PaymentRecord`, `Refund`, `RefundRecord` adları 06/02 ile aynı; yeni enum yok |
| `09 §25` diff yalnız görev | ✓ | Kapsam dışı (stok/kupon serbest bırakma, sepet temizliği, sayfa ve iş işleyicisi) yazılmadı — ⚠ listesi |
| `09 §25` modül sınırı (yalnız Contracts, arayüzde SaveChanges yok) | ✓ | `IPaymentModule`, `INotificationsModule`, `IAfterSalesModule` üzerinden; Architecture testleri yeşil (Unit 3965) |
| `09 §25` `!` / `async void` / `dynamic` yok | ✓ | Kaynak koddaki null-forgiving `!` bu turda kaldırıldı (`OrderTransitions.Payment.cs`, `PaymentModule.cs`) |
| `09 §25` zaman yalnız `TimeProvider` | ✓ | `PaymentModule` ve `OrderTransitions` `_time.GetUtcNow()` |
| `09 §25` durum değişikliği yalnız geçiş birimiyle, tablo `02 §5.5` ile eşit | ✓ | Yazım yalnız `ApplyAsync`; `TransitionUnitSourceScan`, `TransitionTableConsistencyTests` yeşil |
| `09 §25` işlem sırası; dış yan etki aynı işlemde `Job` satırı; ret geri alınır | ✓ | `StartLatePaymentRefundAsync` → `IJobQueue.EnqueueAsync(CardRefund)`; ret `PaymentTransitionResult.IsRejected` (koşucu geri alır) |
| `09 §25` işlem izi (`AuditOperation`) | uygulanamaz | Yönetici işleminin izi sayfa kapısında (T527 kriter 10); birim iz yazmaz (T315 ile aynı) |
| `09 §25` eşzamanlılık: kilit, sürüm | ✓ | `LockAndLoadAsync` (`OrderLocks`); yönetici yöntemleri sürüm karşılaştırır (D-291 (b)); sürüm bir kez artar (`…_OneVersionRaise_…_KK3`) |
| `09 §25` para hesabı | ✓ | Yalnız toplama/karşılaştırma (`SentRefundTotalAsync`); yeni tutar üretilmez — geç ödemenin iadesi siparişin toplamıdır (kriter 8) |
| `09 §25` veri/göç, değer ekleme | uygulanamaz | Göç, enum, iş türü eklenmedi (`CardRefundResult` taslağı enum kilidi yüzünden silindi, mevcut `RefundRecordState` kullanıldı) |
| `09 §25` test katmanı; gerçek saat yok | ✓ | Davranış gerçek PostgreSQL'de (Integration), sahte saat (`Now` sabiti) |
| Sık FAIL (1) tarama istisnası geniş | uygulanamaz | Yeni tarama/istisna yok |
| Sık FAIL (2) eşitlik tek yönlü | ✓ | Terminal küme testi iki yönlü (tablodan "çıkışı olmayan" = `IsTerminal`) |
| Sık FAIL (3) DB varsayılanı / FK dizini | uygulanamaz | Şema değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | Backlog `Aktif kalem` 125 → 126 (sayım 126); status satırı; D-291 hedef hücresi |
| Sık FAIL (5) beklenen değer sabitten | ✓ | Testler hedef durumu `TransitionTable.FindPayment/FindShipment(...).To`'dan okur |
| Sık FAIL — tek kural evi yerine kendi denetimi (T341 r2, T317 r2) | ✓ | Düzeltme turu 3: ödenmemiş iptalin onay, sürüm ve "Diğer ⇔ not" denetimi yalnız `TryCancellationActor`'da (`OrderTransitions.Shipment.cs:314`, :323); T317'nin kendi denetimi (`TryReadCanceller`) silindi |
| Sık FAIL — modül metodu SaveChanges / işlem açar (T223, T321) | ✓ | `grep -n "SaveChanges\|BeginTransaction" src/Shopfolio.Web/Modules/Ordering src/Shopfolio.Web/Modules/Payment` → yalnız kayıt bekçileri (interceptor); yöntemler çağıranın işlemine yazar |
| Sık FAIL — kural değişince öteki yerler (#381) | ✓ | `IOrderTransitions` özeti ve `StartPreparationAsync` yorumu, `OrderTransitions.Payment.cs` sınıf özeti, D-307 ve D-389, bu raporun "Yapılan İşler", "Sözleşme kayıtları" ve ⚠ 8. maddesi güncellendi; `grep -rn "approvesPayment\|CancelUnpaidOrderAsync\|UnpaidOrderCancellation" src tests` → 0 |
| Güvenlik taraması | ✓ | Sır yok; yeni uç/paket yok; yönetici kimliği ve sürüm çağırandan, yetki sayfa kapısında; girdi `ArgumentNullException`/`ThrowIfNullOrWhiteSpace` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build --no-restore Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Lint | ✓ | `dotnet format … --verify-no-changes --include` (değişen klasörler) | değişiklik yok |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/… --no-build` | 3965 / 3965 (TransitionTableTests 26); düzeltme turu 3 birleşik ağaçta 4116 / 4116 |
| Integration | ✓ (1 ilgisiz) | `dotnet test --project tests/Shopfolio.IntegrationTests/… --no-build --filter-namespace …` (postgres:18, `shopfolio-t317-pg`) | Ordering 60 (PaymentTransitionsTests 29), Jobs + Concurrency 103, Model 1076, Database/Files/Security/… 251, Web 513'te 512 — düşen `VersionedFormPageTests.OrderPost_ByTheAdministrator_WithAnOldVersion_…_KK6` T317'ye bağlı değil, sıra bağımlı tohum çakışması (D-310). Düzeltme turu 3 (birleşik ağaç, `shopfolio-t317-fix3-pg`): Payment 33, Shipment + OrderTransitions 125, Disposal + Concurrency 71 |
| Contract | — | NOT_APPLICABLE | |

## Düzeltme turu 3 (2026-11-08)

Doğrulama tur 2 B1 ve K1 (D-389) için. Dal `origin/task/T317-odeme-ekseni` (`545a12b`) + `origin/main` `a89710b` (T321, #439) birleştirmesi; kayıt çakışmaları (backlog, status) çözüldü, kod çakışması yok.

**Ne değişti:**

- **B1 — ödenmemiş iptalin tek yolu.** `IOrderTransitions.CancelUnpaidOrderAsync`, `UnpaidOrderCancellation` kaydı ve `TryReadCanceller` kaldırıldı. Seçilen yol "kaldır, T316 yolundan sına": `09 §24` madde 4 aynı işin tek işlevini ister ve T316'nın `CancelOrderAsync`'i Alındı + Bekliyor'da S3 + Ö2'yi zaten tek istekte yazıyor (`ChooseCancellation`). Onay, sürüm ve "Diğer ⇔ not" denetimi tek yerde kalır (`TryCancellationActor`); ikinci bir sarmalayıcı ikinci bir giriş noktası olurdu. Müşteri yolu ve S3 + Ö2 tek işlem davranışı değişmedi. `IOrderTransitions` ve `OrderTransitions.Payment.cs` özetleri Ö2'nin ayrı yöntemi olmadığını yazar.
- **K1 / D-389 — Ö1'in tek yolu.** `StartPreparationAsync`'ten `approvesPayment` parametresi ve Ö1 + B-4 dalı kaldırıldı; yöntem yalnız S1 yazar (ödeme Ödendi'yken; Bekliyor'da çift kuralı reddeder). Ö1'i yalnız ödeme kaydı yazan `ApplyCardPaymentAsync` (aynı işlem kimliğini ayıklar) ve `MarkTransferPaidAsync` yazar. Kaldırılan dalın üretim çağıranı yoktu; para davranışı değişmedi. D-389 kapandı, D-307 T318 payına daraldı.

**Testler:**

- `PaymentTransitionsTests` KK4 (11 durum, 5 yöntem) artık `CancelOrderAsync` üzerinden: ödenmemiş siparişte alıcı/yönetici/sistem S3 + Ö2 tek işlemde (iki kayıt, gerekçe, tek sürüm artışı, B-7); ödenmiş siparişte Ö2 yok (alıcı ve sistem `ConditionNotMet`, firma S3 tek başına ve ödeme Ödendi'de); **onaysız yönetici iptali `ConfirmationRequired`**; **"Diğer" dışı gerekçeyle notlu, "Diğer" ile notsuz ya da boş notlu yönetici iptali `InvalidInput`**; sürümsüz `InvalidInput`, eski sürüm `VersionConflict`, sistemin kapanış gerekçesi `ReasonNotAllowed`. Her retten sonra sipariş, kayıt ve bildirim satırı değişmemiştir.
- `ShipmentTransitionsTests`: S1 testleri yeni imzaya geçti (fiziksel kalemsiz sipariş ödeme Ödendi'yken tohumlanır, koşul reddeder); karışık sipariş testi kart ödemesi yoluna taşındı (`S1_OnAMixedOrderWithAPhysicalItem_ComesWithTheCardApprovalO1_OneVersionRaise_AndOneB4Row_KK3`) ve ödeme kaydının Ö1'e bağlandığını da sınar.
- Birleşik ağaçta (yerel): restore 0; build `-warnaserror` 0 uyarı / 0 hata; `dotnet format --verify-no-changes` (Ordering kaynak + test klasörleri) temiz; Unit 4116 / 4116; Integration (postgres:18, `shopfolio-t317-fix3-pg`, 127.0.0.1:55437) PaymentTransitionsTests 33, ShipmentTransitionsTests + OrderTransitionsTests 125, Disposal + Concurrency 71 — hepsi yeşil.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 3, 2026-11-08; head `24be2b5` + `origin/main` `9bb46eb` (T339, #445) birleştirmesi). Önceki: ✗ FAIL (tur 2, head `66bcf16`), ✓ PASS (tur 1, head `0926ba7`) |
| Bulgu sayısı | tur 3: 0 · tur 2: 1 (B1 — ORTA, S1; tur 3'te giderildi doğrulandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | tur 3: 1 (K1 → bu PR'da kapatıldı, D-291 metni); tur 2: 1 (K1 → D-389, ✅); tur 1: 1 (K1 → D-322) |

### Tur 3 (2026-11-08) — ✓ PASS

Kapsam tam (kriter 1–8, KK1–KK8, `09 §25`, güvenlik); önceki tur raporu açılmadan. Başlangıç kapıları: working tree temiz; `core.hooksPath` = ana çalışma kopyasının `scripts/git-hooks`'u (mutlak yol); main CI 42419095630, 42419095662, 42411903829 success; MEMORY'de T317 satırı var.

| # | Kriter / KK | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 / KK1 | Ö1…Ö5 beyaz liste; Başarısız ve Geri ödendi terminal | ✓ | `TransitionTable.Payment` beş satır, `IsTerminal(PaymentStatus)` = {Failed, Refunded}; `ApplyRefundSentAsync` terminalde eksene dokunmaz; `MarkTransferPaid_OnAnOrderThatIsNotPending_…_KK1` (Cancelled/Failed, Preparing/Paid → `IsNotOpen`, kayıt yok), `RefundSent_OnARefundedOrder_…_KK1`, birim `ThePaymentAxisTerminalStates_…_T317KK1` yeşil |
| 2 / KK2 | Ö1 kartta başarı bildirimi ya da son sorgu | ✓ | `ApplyCardPaymentAsync` tek yol: kilit → aynı işlem kimliği `IsAlreadyApplied` → Bekliyor'da Ö1 (+açık fiziksel kalemde S1) sistemle, ödeme kaydı Ö1'e bağlı, B-4; `CardPayment_…_KK2` (3 test) yeşil |
| 3 / KK3 | Ö1 havalede yönetici işareti, geri alınamaz onay, B-4 | ✓ | `MarkTransferPaidAsync`: onaysız `IsConfirmationRequired`, sürüm farkı `IsVersionConflict`, kart siparişi `IsNotOpen`; Ö1 yönetici + S1 sistem, sürüm bir kez; ödeme kaydı işlem kimliksiz, Ö1'e bağlı; `MarkTransferPaid_…_KK3` (3 test) yeşil. Yoklama: dijital havale siparişinde yalnız Ö1 + B-4 |
| 4 / KK4 | Ö2 S3'le aynı anda, kim iptal ederse | ✓ | Tek yol `CancelOrderAsync` → `ChooseCancellation` (Alındı + Bekliyor → [S3, Ö2], tek istek, tek sürüm artışı); `CancelUnpaidOrderAsync` yok (`grep` src 0); `ReasonsAllowed` Ö2'de sistem gerekçesine izin verir; `CancelOrder_OfAnUnpaidOrder_ByAnyone_…_KK4` (Buyer/Admin/System), `CancelOrder_OfAPaidOrder_DoesNotApplyO2_KK4`, onaysız / "Diğer ⇔ not" / sürümsüz yönetici iptali reddi yeşil |
| 5 / KK5 | Ö3/Ö4/Ö5 | ✓ | `SentRefundTotalAsync` (geç ödeme iadesi ve geçersizler hariç, açık işlemin değişiklikleri dahil) ≥ `Order.Total` → Ödendi'den Ö4 / Kısmen'den Ö5, altında Ödendi'den Ö3; `CardRefundSucceeded_…_KK5` (4 durum), `ProcessTransferRefund_…_KK5` yeşil |
| 6 / KK6 | B-8 müşteriye, F-4 firmaya | ✓ | B-8 sipariş e-postasına her gerçekleşen iadede; F-4 firmanın e-postasına yalnız sistem + kart + firmanın yeniden denemesi değil; `SystemCardRefundSucceeded_…_KK6`, `FirmRetryOrAdministratorsCardRefundSucceeded_…_KK6` yeşil |
| 7 / KK7 | Başarısız/bilinmeyen iade: eksen aynı, işaret, bildirim yok | ✓ | `!IsSent` dalı yalnız sürümü artırır; `ReadRefundMarksAsync` işareti kayıttan okur; `CardRefundNotSentOrUnknown_…_KK7` (Failed, OutcomePending) yeşil. Yoklama: başka siparişin iade kaydı `IsNotOpen`, hiçbir şey yazılmaz |
| 8 / KK8 | İptal edilmiş siparişin ödemesi iade edilir | ✓ | Ö1'siz ödeme kaydı + `Refund.ForLatePayment` (tutar `Order.Total`) + `CardRefund` iş satırı aynı işlemde; eksenler kıpırdamaz; iadesi gerçekleşince eksen yine aynı, B-8 + F-4; `…_KK8` (2 test) yeşil. Yoklama: Kısmen geri ödendi siparişe ikinci ödeme de geç iadeye gider (K-1211, `02 §6.2.16`) |
| KK | `09 §25` ilgili satırlar | ✓ | Tek işlev: Ö1 yalnız `ApplyCardPaymentAsync`/`MarkTransferPaidAsync` (`StartPreparationAsync` yalnız S1, Bekliyor'da çift kuralı reddeder), Ö2 yalnız `ChooseCancellation`; durum yalnız çekirdekten; iç yan etki aynı işlemde, dış yan etki `Job` satırı; ret `IsRejected` ile geri alınır; zaman `TimeProvider`; yeni paket/göç/enum yok; işlem izi sayfa kapısında (T527; T316 ile aynı) — uygulanamaz |
| KK | Güvenlik taraması | ✓ | Sır yok; yeni uç/paket yok; yönetici kimliği ve sürüm çağırandan, yetki sayfa kapısında; iade kaydı siparişe bağlılıkla denetlenir (başka siparişin kaydı reddedilir); girdi `ArgumentNullException`/`ThrowIfNullOrWhiteSpace` |

**Birleştirmede değişen ödeme kodu (özel sınama):** B-4/B-8 `QueueNoticeAsync` ile sipariş e-postasına (yoksa yazılmaz), F-4 `QueueCompanyNoticeAsync` ile; S1 kararı `OpenItemsAsync(physicalOnly: true)`; `PaymentRefused` çekirdeğin reddini `IsOrderNotFound`/`IsVersionConflict`/`IsNotOpen`'a eşler ve ret işlem koşucusunda geri alınır. Dört geçici yoklama testi (başka siparişin iade kaydı, dijital havale işareti, Kısmen geri ödendi siparişe ikinci ödeme, kart siparişine havale işareti iade kaydını değiştirmez) yeşil; yoklama dosyadan geri alındı.

**K1 (→ bu PR'da kapatıldı):** D-291'in ✅ metni düzeltme turu 3'te kaldırılan `CancelUnpaidOrderAsync`'i ve artık olmayan `CancelUnpaid_ByTheAdministratorWithoutTheVersion_OrByTheSystemAsAClosure_IsRefused_KK4` testini anıyordu; kayda güncel yol ve test adı eklendi.

Yerel (tur 3, birleştirmeden önce head `24be2b5`): restore 0; build `-warnaserror` 0 uyarı / 0 hata; `dotnet format --verify-no-changes` 3 proje temiz; Unit Architecture 933 + Consistency 241 + öteki 2942 = 4116 yeşil; betik testleri 4 dosya; Integration (postgres:18, `shopfolio-t317-val3-pg`, 127.0.0.1:55640) PaymentTransitionsTests 33, OrderTransitionsTests 31, ShipmentTransitionsTests 94, Disposal 45 yeşil; sonrasında `git status` temiz, `restore --locked-mode` 0. Dal CI 42421489591 (head `24be2b5`) success — 8 adım + CI Gate.

**`12` satırları (tur 3):** KP-18 satırları (`VAL-B014`–`B017`, `VAL-A054`–`A059`, `VAL-A977`) sipariş sayfasının tarayıcı ve uç kanıtını ister; T317 sayfa ya da uç kurmaz, kriterlerle eşlenmez → `BEKLEMEDE` kalır. KD-62 satırı yok. `12_matris_denetle.py --kapsam 1-77` hata 0.

**Yapım raporu karşılaştırması (tur 3):** tam uyumlu. Öz denetim tablosu var, her madde kanıtlı; validator'ın test sayıları (Unit 4116, PaymentTransitionsTests 33) raporla aynı. Tek ek: rapordaki "kural değişince öteki yerler" maddesinin `grep`'i `src`/`tests` ile sınırlı kaldığı için D-291'in eski adları gözden kaçtı (K1, kapatıldı).

### Tur 2 (2026-11-08) — ✗ FAIL

Kapsam tam (kriter 1–8, KK1–KK8, `09 §25`, güvenlik); birleştirmede değişen ödeme kodu (`QueueNoticeAsync`/`OpenItemsAsync` ortak yardımcıları, `PaymentRefused`) ayrıca okundu ve sınandı.

| # | Kriter / KK | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 / KK1 | Ö1…Ö5 beyaz liste; Başarısız ve Geri ödendi terminal | ✓ | `TransitionTable.IsTerminal(PaymentStatus)` = {Failed, Refunded}; `ApplyRefundSentAsync` terminalden çıkmaz; `MarkTransferPaid_OnAnOrderThatIsNotPending_…_KK1`, `RefundSent_OnARefundedOrder_…_KK1`, birim `TransitionTableTests` yeşil |
| 2 / KK2 | Ö1 kartta başarı bildirimi ya da son sorgu | ✓ | `ApplyCardPaymentAsync` tek yol; Bekliyor'da Ö1 (+açık fiziksel kalemde S1) sistemle, ödeme kaydı Ö1'e bağlı, B-4 `QueueNoticeAsync` ile; aynı işlem kimliği `IsAlreadyApplied`; `CardPayment_…_KK2` (3 test) yeşil |
| 3 / KK3 | Ö1 havalede yönetici işareti, geri alınamaz onay, B-4 | ✓ | `MarkTransferPaidAsync`: onaysız `IsConfirmationRequired`, sürüm farkı `IsVersionConflict`; Ö1 yönetici + S1 sistem, sürüm bir kez; `MarkTransferPaid_…_KK3` (3 test) yeşil |
| 4 / KK4 | Ö2 S3'le aynı anda, kim iptal ederse | ✓ | `CancelUnpaidOrderAsync` S3+Ö2 tek istekte, ödenmişte çekirdek reddeder; `CancelUnpaid_…_KK4` yeşil. Yöneticinin yolundaki onay eksiği B1'de |
| 5 / KK5 | Ö3/Ö4/Ö5 | ✓ | `SentRefundTotalAsync` (`LatePayment` ve geçersizler hariç) ≥ `Order.Total` → Ö4/Ö5, altında Ö3; `CardRefundSucceeded_…_KK5`, `ProcessTransferRefund_…_KK5` yeşil |
| 6 / KK6 | B-8 müşteriye, F-4 firmaya | ✓ | `ApplyRefundSentAsync`: B-8 her gerçekleşen iadede; F-4 yalnız sistem + kart + yeniden deneme değil; `…_KK6` (2 test) yeşil |
| 7 / KK7 | Başarısız/bilinmeyen iade: eksen aynı, işaret, bildirim yok | ✓ | `ApplyCardRefundOutcomeAsync` `!IsSent` dalı yalnız sürümü artırır; `CardRefundNotSentOrUnknown_…_KK7` yeşil |
| 8 / KK8 | İptal edilmiş siparişin ödemesi iade edilir | ✓ | Ö1'siz ödeme kaydı + `LatePayment` iadesi + `CardRefund` iş satırı aynı işlemde; `…_KK8` (2 test) yeşil |
| KK | `09 §25` ilgili satırlar | ✗ | B1: "Kod hiçbir doküman cümlesiyle çelişmiyor mu?" ve "Aynı işi yapan tek işlev varsa o çağrılmış mı?" → hayır |
| KK | Güvenlik taraması | ✓ | Sır yok; yeni uç/paket yok; yetki sayfa kapısında (T527); girdi denetimi `ArgumentNullException`/`ThrowIfNullOrWhiteSpace` |

**Bulgu B1 (ORTA, S1 — `09 §25`; `02 §5.9`, `03 §1.6.1.3`, `09 §24` madde 4):** birleştirmeden sonra `IOrderTransitions` ödenmemiş siparişin iptalini iki açık yöntemle sunar: T316'nın `CancelOrderAsync`'i (Alındı + Bekliyor'da S3+Ö2) ve T317'nin `CancelUnpaidOrderAsync`'i. İkincisinin yönetici yolu firma iptalini (S3) **geri alınamaz onay olmadan** yazar: `UnpaidOrderCancellation`'da onay alanı yoktur, `TryReadCanceller` yalnız yönetici kimliği, adı ve firma gerekçesini ister. `02 §5.9` "firma kaynaklı iptal (S3, S4, S9)" için onayı zorunlu tutar. Aynı yol "Diğer ⇔ not" kuralını da (`06 §4.19`) denetlemez. T316'nın yolu ikisini de denetler (`Confirmed` → `ConfirmationRequired`, `Other`/not → `InvalidInput`). Kanıt: `CancelUnpaid_ByAnyone_IsS3AndO2InOneOperation_WithTheReason_AndB7_KK4(Admin)` onaysız istekle S3+Ö2 yazıp geçer (yerel ve CI yeşil). Düzeltme: ödenmemiş iptalin tek yolu kalır. Ya `CancelUnpaidOrderAsync` kaldırılır ve KK4 testleri `CancelOrderAsync` üzerinden sınanır, ya da `CancelUnpaidOrderAsync` T316'nın yoluna bağlanır ve yöneticinin onaysız ya da "Diğer"siz notlu iptalinin reddi test edilir.

**K1 (→ D-389):** Ödeme onayının iki yolu var. T316'nın `StartPreparationAsync(orderId, approvesPayment: true)` yöntemi Ö1+S1 ve B-4 yazar, ama ödeme kaydı yazmaz ve tekrar gelen ödemeyi ayıklamaz. T317'nin `ApplyCardPaymentAsync` ve `MarkTransferPaidAsync` yöntemleri ise Ö1'i kendileri yazar. D-307 hâlâ "T317 ve T318 bu çağrı biçimini kullanır" der. Bugün bu yöntemleri çağıran üretim kodu yok. Yapılacak: Ö1'in tek yolu seçilir (T317 yöntemleri), `approvesPayment: true` dalı kaldırılır ya da ödeme kaydı yazmayan bir Ö1 yolu kalmayacak biçimde daraltılır, D-307 buna göre güncellenir.

Yerel (tur 2): restore 0; build `-warnaserror` 0 uyarı / 0 hata; `dotnet format --verify-no-changes` 3 proje temiz; Unit 3862 (Consistency hariç; Architecture dahil) + Consistency 241 yeşil; betik testleri 4 dosya; Integration (postgres:18, `shopfolio-t317-val2-pg`, 127.0.0.1:55930) PaymentTransitionsTests + OrderTransitionsTests 60, ShipmentTransitionsTests 94, DisposalTests + OrderDisposalTests 45 — hepsi yeşil; `--locked-mode` restore 0; `12_matris_denetle.py --kapsam 1-77` hata 0. Dal CI 42414305022 success (head `66bcf16`). Ana dal CI son 3: 42411903829, 42411903876, 42408991705 success. Memory drift: `T317` satırı var.

**`12` satırları (tur 2):** KP-18 satırları (`VAL-B014`–`B017`, `VAL-A054`–`A059`, `VAL-A977`) sipariş sayfasının tarayıcı ve uç kanıtını ister. T317'nin kabul kriterleriyle eşlenmez → `BEKLEMEDE` kalır, `Durum` değişmedi.

**Yapım raporu karşılaştırması (tur 2):** öz denetim tablosu var ve her maddesi kanıtlı. 1 uyuşmazlık var: "`09 §25` yeni adlar belgeden, çelişki yok ✓" maddesi `02 §5.9`'u okumamış (okunan bölümler listesinde §5.9 yok). Birleştirmenin bıraktığı çift yol da raporda yok.

### Tur 1 (2026-11-07) — ✓ PASS

Validator kanıtları (bağımsız): sekiz kriter ve KK1–KK8 kodda ve testte ✓. Ö1…Ö5 yalnız çekirdeğin `ApplyAsync`'inden yazılır; `TransitionTable.IsTerminal(PaymentStatus)` = {Başarısız, Geri ödendi} ve tabloda bu iki durumdan çıkan satır yok (`03 §1.3`; `02 §5.5`). Kart ödemesi tek yöntemdir (`05 §4.5.3`): Bekliyor'da Ö1 (+ açık fiziksel kalemde S1) sistemle, ödeme kaydı Ö1 satırına bağlı (`06 §4.32` "Ödemeyi onaylayan Ö1"), B-4; aynı işlem kimliği hiçbir şey yazmaz (K-970); İptal edildi'de ve Ödendi'de yeni işlem kimliğinde ödeme Ö1'siz kaydedilir, `LatePayment` geri ödemesi siparişin toplamıyla ve `CardRefund` iş satırıyla aynı işlemde doğar, eksenler durur (`02 §6.2.16`; K-1211). Havale işareti onaysız `IsConfirmationRequired`, sürüm farkında `IsVersionConflict`, kart siparişinde ve Bekliyor dışında red; Ö1 yönetici + S1 sistem, sürüm bir kez artar. Ödenmemiş iptal S3+Ö2 tek istekte (alıcı/yönetici/sistem), ödenmişte çekirdek Ö2'yi reddeder ve hiçbir şey yazılmaz. Gerçekleşen geri ödeme gönderilen toplam (`LatePayment` ve geçersizler hariç) ≥ `Order.Total` ise Ö4/Ö5, altında Ödendi'den Ö3; B-8 iletişim e-postasına, F-4 yalnız sistemin kendi kart iadesinde (firmanın yeniden denemesi hariç, `03 §7.1.18`) firma adresine; başarısız/bilinmeyen sonuçta eksen ve bildirim yok, işaret iade kaydından okunur. Yerel: build `-warnaserror` 0/0, format 3 proje temiz, Unit 3965 (Architecture 906, Consistency 230, öteki 2829), betik testleri 4 dosya, Integration (postgres:18, `shopfolio-t317-val-pg`) Ordering 60 (PaymentTransitionsTests 29), Concurrency 26, PaymentRecordTables + JobCleanupAndRefund 92, web açılışı (ShopfolioWebApplicationFactory, JobQueueRegistration, OperationPattern) 18 — hepsi yeşil; `--locked-mode` restore 0. Dal CI 42396093293 success (head `0926ba7`).

K1 (→ D-322): `PaymentModule.ReadRefundMarksAsync` "geri ödeme gerçekleşmedi" işaretinde geri ödemenin hâlâ kart yolunda olmasını aramaz (`06 §4.34`: işaret havaleye dönünce kalkar). Bugün yolu havaleye çeviren kod yok (T370), bu yüzden davranış etkilenmiyor; T370 yolu çevirirken işaret okumasına `PaymentMethod == Card` koşulu eklenir.

**`12` satırları (`12 §7.5`):** `11 §7.1` T317 kaynakları `10` KP-18 ve KD-62. KP-18 grep'i `VAL-B014`–`B017`, `VAL-A054`–`A059`, `VAL-A977`'yi getirir; hepsi sipariş sayfasının ve girişinin tarayıcı gözlemi ya da uç yanıtıdır, T317 sayfa ya da uç kurmaz (KP-18 → T317, T354, T511) → `BEKLEMEDE` kalır, `Durum` değişmedi. KD-62 matriste yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T317-odeme-ekseni`
- **Commit:** `4362c3d` — T317: Ödeme ekseni: testler derlendi ve geçti, null-forgiving kaldırıldı, rapor ve kayıtlar (dalın son kod commit'i; PR numarası commit'i bunu izler)
- **Düzeltme turu 3:** `T317: düzeltme turu 3 — ödenmemiş iptalin tek yolu CancelOrderAsync (B1), Ö1'in tek yolu ödeme ekseni (D-389)` + `origin/main` birleştirmesi (dalın başı)
- **PR:** #441
- **CI:** 42421489591 success (head `24be2b5`; validator tur 3) · 42414305022 success (head `66bcf16`; validator tur 2) · 42396093293 success (head `0926ba7`; validator tur 1)
- **Post-merge run'lar:** squash `7097c01` (#441) — CI 42430286061 success, Publish 42430286045 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok — task yalnız uygulama içi geçiş kuralıdır; sağlayıcı çağrısı (son sorgu, kart iadesi isteği) T403/T405'tedir.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `VersionedFormPageTests` tohumunun yasal metin sürüm numarası açılışta üretilen sürümle çakışıyor (yerelde düşüyor, main CI'da sıra farklı) | D-310 | T317 dosyasına dokunmaz (bundled PR yasağı) |
| 2 | D-291 (b)'nin T316 payı | D-291 | T316'nın işi |

### ⚠ Kurala en uygun seçenekle verilen kararlar

1. Geç ya da ikinci ödemede B-8/F-4 kuyruğa yazımda değil, iade gerçekleşince gider (`05 §4.5.3` aynı cümlede sayar; `02 §7.2.8` ile tutarlı).
2. Ödenmemiş iptalde stok/kupon serbest bırakma ve iptal kaydı bu task'ta yok — T318 kriter 8 (an tablosu).
3. Ödeme başarısında sepet kalemlerinin silinmesi (`05 §4.5.3`) bu task'ta yok — kartta T348, havalede T527 (`07 UC-123`).
4. Firma işaretinden sonra gelen geç iade başarısı işlenmez: kayıt açık değilse `IsNotOpen` (T405 kriter 8 ile aynı).
5. `ProcessTransferRefundAsync` yalnız havale yolu; kart yolundaki firma geri ödemesi iş + sonuç (`ApplyCardRefundOutcomeAsync`) ile, aktör geri ödemenin yöneticisi.
6. S1 koşulu: açık adedi olan fiziksel kalem (`03 §1.2` "en az bir fiziksel kalem"); dijital/hizmet siparişinde yalnız Ö1.
7. Geç ödemenin iade tutarı siparişin toplamı (`06 §4.32`: ödeme kaydında tutar saklanmaz, siparişin toplamıdır).
8. ~~Sürümsüz yönetici iptali `IsVersionConflict` döner (D-291 (b)).~~ Düzeltme turu 3: ödenmemiş iptal T316'nın `CancelOrderAsync`'inden geçer; sürümsüz yönetici iptali orada `InvalidInput`, eski sürüm `VersionConflict` döner (D-291 (b)).

## Notlar

- **Working tree (Adım -1):** temiz (3. işçi worktree'si, `origin/task/T317-odeme-ekseni` `8207bc0` + `origin/main` `c4c872c` birleştirmesi)
- **Hook kurulumu:** `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** main CI 42393301185 success, 42391220711 success, 42389626405 success (Publish 42393301195, 42391220720, 42389626428 success)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** yapım üç işçiye bölündü (bağlam sınırı); devir notları `helper-state/shopfolio/impl/T317/`.
