# T317 — Ödeme durumu geçişleri: ödeme ekseni

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-07

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Geçiş biriminin ödeme ekseni işlemleri (`OrderTransitions.Payment.cs`, `OrderTransitions`'ın parçası; her durum yazımı çekirdeğin `ApplyAsync`'inden geçer, yan etkiler aynı işlemde):
  - `ApplyCardPaymentAsync` — kartta sağlayıcının başarı bildirimi ve son sorgunun olumlu cevabı aynı yoldan: Ö1 sistem, açık fiziksel kalem varsa S1 aynı istekte; ödeme kaydı (Ö1 geçiş kaydına bağlı) ve B-4. Aynı işlem kimliği ikinci kez gelirse hiçbir şey yazılmaz (`IsAlreadyApplied`).
  - Ödenmesi kalmamış siparişe gelen kart ödemesi (iptal edilmiş ya da yeni işlem kimliğiyle ödenmiş): ödeme kaydında Ö1'siz durur, sistem `Refund.ForLatePayment` + `CardRefund` iş satırıyla geri öder; eksenler kıpırdamaz, sürüm bir kez artar.
  - `MarkTransferPaidAsync` — havalede yöneticinin "ödendi" işareti: geri alınamaz onay ve ekrandaki sürüm zorunlu; Ö1 yönetici, S1 sistem (iki istek, aynı kilit, sürüm bir kez artar — D-291 (a)); ödeme kaydı ve B-4.
  - `CancelUnpaidOrderAsync` — ödenmemiş siparişin iptali kim yaparsa yapsın S3 + Ö2 tek istekte; yönetici firma gerekçesi ve sürümle, sistem otomatik gerekçeyle (kapanış hariç); B-7. Ödenmiş sipariş reddedilir (Ö2 yalnız Bekliyor'dan).
  - `ApplyCardRefundOutcomeAsync` — kart iadesinin sonucu: gerçekleşince gönderilen toplam ödenen tutara ulaştıysa Ödendi'den Ö4 / Kısmen geri ödendi'den Ö5, altındaysa Ödendi'den Ö3; B-8 her gerçekleşen geri ödemede, F-4 yalnız sistemin kendiliğinden kart iadesinde (firmanın yeniden denemesi değilse). Başarısız ya da bilinmeyen sonuçta eksen aynı, bildirim yok, panel işareti iade kaydından düşer.
  - `ProcessTransferRefundAsync` — yöneticinin havale geri ödemesini işlemesi (onay + sürüm): Ö3/Ö4/Ö5 aynı kuralla, B-8 (F-4 yok).
- Ödeme modülü yüzü (`IPaymentModule`/`PaymentModule`): `IsPaymentRecorded`, `RecordCardPayment`, `RecordTransferMark`, `StartLatePaymentRefund`, `WriteCardRefundOutcome`, `MarkTransferRefundSent`, `SentRefundTotal`, `ReadRefundMarks`; çağıranın işlemine yazar, `SaveChanges` çağırmaz. Kayıtlar `RefundFacts`, `RefundMarks`.
- `TransitionTable`: `FindPayment(from, to)` (Ö3/Ö4/Ö5 ikinci bir listeden değil tablodan okunur), `IsTerminal(PaymentStatus)`.
- `TransitionRequest` yorumu D-291 (a)'ya göre düzeltildi; `PaymentRecord` yorumları `06 §4.32` ile eşitlendi (geçiş kaydı Ö1'dir).
- Sözleşme kayıtları (`Ordering/Contracts/PaymentTransitions.cs`): `CardPaymentSucceeded`, `TransferPaidMark`, `UnpaidOrderCancellation`, `CardRefundOutcome`, `TransferRefundProcessing`, `PaymentTransitionResult` (enum eklenmedi; ret bir değerdir, işlem koşucusu geri alır).
- Bu turda (3. işçi): test derlemesi (eksik `using`), dijital kalemin adet kuralına uyan test tohumu, `09 §3.1` madde 5'e göre kaynak koddaki null-forgiving `!` kullanımlarının kaldırılması (`Refused(TransitionOutcome)`, `[NotNullWhen(true)]`, `OfType<RefundRecord>()`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/`: `OrderTransitions.Payment.cs` (yeni), `OrderTransitions.cs`, `TransitionTable.cs`, `TransitionRequests.cs`, `Contracts/IOrderTransitions.cs`, `Contracts/PaymentTransitions.cs` (yeni)
- `src/Shopfolio.Web/Modules/Payment/`: `PaymentModule.cs`, `PaymentRecord.cs`, `Contracts/IPaymentModule.cs`, `Contracts/PaymentFacts.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Ordering/PaymentTransitionsTests.cs` (yeni, 29), `Ordering/OrderingTestServices.cs` (yeni, ortak DI), `Ordering/OrderTransitionsTests.cs` (ortak DI'a bağlandı); `tests/Shopfolio.UnitTests/Ordering/TransitionTableTests.cs` (+2)
- Kayıt: bu rapor, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/DEFERRED_BACKLOG.md`, `.claude/memory/MEMORY.md`; T307, T315, T332, T366, T381 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ö1…Ö5 beyaz liste; Başarısız ve Geri ödendi terminal | ✓ | `TransitionTable.Payment` + `IsTerminal`; `ThePaymentAxisTerminalStates_AreExactlyTheStatesNoPaymentRowLeaves_T317KK1`, `MarkTransferPaid_OnAnOrderThatIsNotPending_IsRefused_AndWritesNothing_KK1`, `RefundSent_OnARefundedOrder_DoesNotLeaveTheTerminalState_KK1` |
| 2 | Ö1 kartta başarı bildirimi ya da son sorgunun olumlu cevabıyla | ✓ | `ApplyCardPaymentAsync`; `CardPayment_FromTheNotificationAndFromTheFinalQuery_AppliesO1AndS1ByTheSystem_TheRecordAndB4_KK2`, `CardPayment_ThatCameAlready_DoesNothing_KK2`, `CardPayment_OfAnOrderWithoutAPhysicalItem_AppliesO1Only_AndATransferOrderIsRefused_KK2` |
| 3 | Ö1 havalede yöneticinin işareti, geri alınamaz onay, B-4 | ✓ | `MarkTransferPaidAsync`; `MarkTransferPaid_WithoutTheConfirmation_IsRefused_AndWritesNothing_KK3`, `MarkTransferPaid_Confirmed_IsO1ByTheAdministratorAndS1ByTheSystem_OneVersionRaise_TheRecordAndB4_KK3`, `MarkTransferPaid_OnACardOrder_IsRefused_KK3` |
| 4 | Ö2 S3'le aynı anda, her iptalde, kim yaparsa yapsın | ✓ | `CancelUnpaidOrderAsync`; `CancelUnpaid_ByAnyone_IsS3AndO2InOneOperation_WithTheReason_AndB7_KK4` (Buyer/Admin/System), `CancelUnpaid_OfAPaidOrder_DoesNotApplyO2_AndWritesNothing_KK4` |
| 5 | Ö3/Ö4/Ö5 kart iadesi gerçekleşince ya da yöneticinin geri ödemeyi işlemesiyle | ✓ | `ApplyRefundSentAsync`; `CardRefundSucceeded_OnAPaidOrder_IsO3ForAPartAndO4ForTheWhole_KK5`, `CardRefundSucceeded_OnAPartlyRefundedOrder_IsO5ForTheRest_AndNoTransitionBelowIt_KK5`, `ProcessTransferRefund_IsTheAdministratorsO3_WithTheConfirmationAndTheVersion_AndB8WithoutF4_KK5` |
| 6 | B-8 müşteriye; sistemin kendiliğinden kart iadesinde F-4 firmaya | ✓ | `SystemCardRefundSucceeded_WritesB8ToTheCustomer_AndF4ToTheFirm_KK6`, `FirmRetryOrAdministratorsCardRefundSucceeded_WritesB8Only_KK6` |
| 7 | Başarısız/bilinmeyen iadede eksen aynı, işaret düşer, bildirim yok | ✓ | `CardRefundNotSentOrUnknown_LeavesTheAxis_SendsNoNotice_AndTheMarkFalls_KK7` (Failed, OutcomePending), `CardRefundOutcome_OfARecordThatIsNotOpen_IsRefused` |
| 8 | İptal edilmiş siparişe gelen kart ödemesi kayıtta durur, sistem geri öder | ✓ | `CardPayment_ToACancelledOrder_OrASecondOneToAPaidOrder_IsRecordedAndRefunded_TheAxesStay_KK8`, `LatePaymentRefundSucceeded_MovesNoAxis_AndWritesB8AndF4_KK8` |

### Öz denetim (bitiş kapısı 2)

> Bütün maddeler bu turda yeniden denetlendi (önceki işçilerin yazdıkları dahil).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–8 | ✓ | Üstteki tablo |
| KK1 tablo dışı geçiş reddedilir, terminal durum kalkmaz | ✓ | `MarkTransferPaid_OnAnOrderThatIsNotPending_IsRefused_AndWritesNothing_KK1` (Theory), `RefundSent_OnARefundedOrder_DoesNotLeaveTheTerminalState_KK1`; birim `ThePaymentAxisTerminalStates_…` beklenen kümeyi tablodan iki yönlü türetir |
| KK2 başarı bildirimi ve son sorgu Ö1'i işler | ✓ | `CardPayment_FromTheNotificationAndFromTheFinalQuery_…_KK2` |
| KK3 onaysız "ödendi" reddedilir | ✓ | `MarkTransferPaid_WithoutTheConfirmation_IsRefused_AndWritesNothing_KK3` |
| KK4 ödenmemişte S3+Ö2 aynı işlemde; ödenmişte Ö2 yok | ✓ | `CancelUnpaid_ByAnyone_…_KK4`, `CancelUnpaid_OfAPaidOrder_…_KK4`, `CancelUnpaid_ByTheAdministratorWithoutTheVersion_OrByTheSystemAsAClosure_IsRefused_KK4` |
| KK5 kısmi Ö3, tam Ö4, kalan Ö5 | ✓ | `CardRefundSucceeded_OnAPaidOrder_…_KK5`, `CardRefundSucceeded_OnAPartlyRefundedOrder_…_KK5`, `ProcessTransferRefund_…_KK5`; hedef kod `TransitionTable.FindPayment`'tan (beklenen değer tablodan türetilir) |
| KK6 B-8 ve F-4 doğru alıcıya | ✓ | `SystemCardRefundSucceeded_WritesB8ToTheCustomer_AndF4ToTheFirm_KK6`, `FirmRetryOrAdministratorsCardRefundSucceeded_WritesB8Only_KK6` |
| KK7 yanıtsız iadede eksen aynı, işaret düşer | ✓ | `CardRefundNotSentOrUnknown_LeavesTheAxis_SendsNoNotice_AndTheMarkFalls_KK7` (`ReadRefundMarksAsync`) |
| KK8 iptal edilmiş siparişin ödemesi iade edilir | ✓ | `CardPayment_ToACancelledOrder_…_KK8`, `LatePaymentRefundSucceeded_…_KK8` |
| `09 §25` doküman uyumu: okunan bölümler | ✓ | 11 T317 bloğu; `03 §1.3`, §1.2 (S1), §2.5; `02 §5.5`, §5.6.3, §6.1.2, §6.2.16, §7.2.8; `05 §4.5.3`, §4.6, §7.3.2; `06 §4.32`; `09 §9`, §3.1, §25 |
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
| Güvenlik taraması | ✓ | Sır yok; yeni uç/paket yok; yönetici kimliği ve sürüm çağırandan, yetki sayfa kapısında; girdi `ArgumentNullException`/`ThrowIfNullOrWhiteSpace` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build --no-restore Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Lint | ✓ | `dotnet format … --verify-no-changes --include` (değişen klasörler) | değişiklik yok |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/… --no-build` | 3965 / 3965 (TransitionTableTests 26) |
| Integration | ✓ (1 ilgisiz) | `dotnet test --project tests/Shopfolio.IntegrationTests/… --no-build --filter-namespace …` (postgres:18, `shopfolio-t317-pg`) | Ordering 60 (PaymentTransitionsTests 29), Jobs + Concurrency 103, Model 1076, Database/Files/Security/… 251, Web 513'te 512 — düşen `VersionedFormPageTests.OrderPost_ByTheAdministrator_WithAnOldVersion_…_KK6` T317'ye bağlı değil, sıra bağımlı tohum çakışması (D-310) |
| Contract | — | NOT_APPLICABLE | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07; head `0926ba7`, `origin/main` `c4c872c` güncel — birleştirmede değişiklik yok) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-322) |

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
- **PR:** #441
- **CI:** 42396093293 success (head `0926ba7`; validator)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında)*

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
8. Sürümsüz yönetici iptali `IsVersionConflict` döner (D-291 (b)).

## Notlar

- **Working tree (Adım -1):** temiz (3. işçi worktree'si, `origin/task/T317-odeme-ekseni` `8207bc0` + `origin/main` `c4c872c` birleştirmesi)
- **Hook kurulumu:** `core.hooksPath` = `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** main CI 42393301185 success, 42391220711 success, 42389626405 success (Publish 42393301195, 42391220720, 42389626428 success)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** yapım üç işçiye bölündü (bağlam sınırı); devir notları `helper-state/shopfolio/impl/T317/`.
