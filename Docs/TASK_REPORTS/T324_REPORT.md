# T324 — Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri

**Faz:** F3 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Ödeme penceresinin kuralları tek evde:** `PaymentWindowRules` (`Awaiting`, `CardWindowEnded`, `TransferWindowEnded`, `TransferReminderDue`, `IsPaymentOutcomeUnknown`) — kaynakların sorgusu ve kilitli ikinci bakışı aynı ifadeyi okur; "ödeme sonucu alınamadı" işareti saklanmaz, siparişin kendi alanları üzerindeki bu kuraldır (`06 §2.7.3`).
- **Pencere süresi:** `PaymentWindows.StartAsync` — kart `ProductConstants.CardPaymentMinutes` (P-18), havale firma ayarı `BankTransferPaymentWindow` (P-7, `ISettingsModule.GetPeriodsAsync`) iş günüyle (`PeriodCounting.EndOf`), hatırlatma anı `TransferSchedule.ReminderAt`. Çağıranlar: sipariş onayı (T348) ve "ödendi" düzeltmesi (T319).
- **Zamanı gelen kayıt kaynakları** (`AddDueRecordSource<T>`, `OrderingModule` kaydı): `TransferPaymentExpiry` (Z-8; `WaitsForFreshHeartbeat = true`; sistem iptali S3 + Ö2 + B-7 + ayırmanın serbest kalması), `TransferPaymentReminder` (Z-9; B-3, sonra `PaymentReminderAt` boşalır; pencere bitmişse gitmez — D-100), `CardPaymentExpiry` (Z-7; sipariş değişmez, sürüm artmaz, `FinalPaymentQuery` iş satırı yazılır; sipariş beklerken kaynak hep zamanı gelmiş sayar, çalıştırıcının vazgeçtiği satırın yerine sonraki uyanışta yenisini yazar).
- **Son sorgu işleyicisi:** `FinalPaymentQueryJobHandler` — hedefi okur (`IOrderingModule.GetCardPaymentQueryTargetAsync`), sağlayıcıyı kilitsiz sorar, `FinalPaymentQueryRule.Decide`: ödeme → Ö1 (`ApplyCardPaymentAsync`, sağlayıcı işlem kimliğiyle), ödeme yok + pencere bitmiş → sistem iptali, yanıtsız → `RescheduleAfter(FinalPaymentQueryInterval)` (deneme sayısı artmaz, üst sınır yok). Yönetici kararı: DI kaydı sağlayıcı uygulamasıyla T403'te (D-449); işleyici testte sahte sağlayıcıyla kurulur.
- `PaymentQueryResult.Paid` ilk alanı `ProviderTransactionId` (gerçek eşleme T403 — D-448).
- **Ayırmanın serbest kalması** (`06 §4.14` yol 3): geçiş biriminin `CancelOrderAsync`'i ödenmemiş siparişin iptalinde `ReleaseReservationAsync` çağırır (`ICatalogModule.ReleaseAsync` + `StockReservations` satırları silinir); tek kural evi geçiş birimidir.
- `ScheduledOrderTransition` sürümü `OrderTransitions.RaiseVersionOnce(db, order)` ile bir kez artırır (geçiş biriminin işlemi zaten artırdıysa ikinci artış yok).
- **Z-1 gecelik silme** (D-253): `PendingRecordExpiry.EndedRegistration` / `EndedEmailChange` + `PendingRegistrationDisposalPart`, `PendingEmailChangeDisposalPart` (`MembershipModule` kaydı, sayı imha kaydına). `RegisterAsync`'in tembel silmesi aynı kuralı okur.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/` (`PaymentWindowRules.cs`, `PaymentWindowSources.cs`, `OrderTransitions.cs`, `OrderTransitions.Shipment.cs`, `ScheduledOrderTransition.cs`, `OrderingModule.cs`, `Contracts/IOrderingModule.cs`)
- `src/Shopfolio.Web/Modules/Payment/` (`FinalPaymentQueryJobHandler.cs`, `FinalPaymentQueryRule.cs` özeti, `PaymentModule.cs` kayıt yorumu)
- `src/Shopfolio.Web/Modules/Membership/` (`PendingRecordExpiry.cs`, `MembershipModule.cs`)
- `src/Shopfolio.Web/Infrastructure/Integrations/PaymentQueryResult.cs`
- Testler: `IntegrationTests/Ordering/PaymentWindowTests.cs`, `IntegrationTests/Disposal/PendingRecordDisposalTests.cs`; `UnitTests/Ordering/PaymentWindowRulesTests.cs`, `UnitTests/Infrastructure/LinkLifetimeTests.cs`, `UnitTests/Architecture/CartTimerScanTests.cs`, `UnitTests/Outages/OutageRulesTests.cs` (Paid imzası)
- Belgeler: `DEFERRED_BACKLOG.md` (D-97 notu, D-99 T324 payı ✅, D-100 ✅, D-253 ✅, D-448, D-449), `IMPLEMENTATION_STATUS.md` (T324 satırı), `STATUS_CHANGELOG.d/T324.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Z-1/Z-2/Z-3 süre dolunca, yenisi istenince geçersiz; Z-1'de doğrulanmamış kayıt ve bekleyen e-posta değişikliği silinir, imha kaydında | ✓ | Unit `LinkLifetimeTests` (P-12/P-13/P-14 ömrü, `SupersededAt`, `EndedEmailChange`); Integration `PendingRecordDisposalTests` (3); `MembershipTests` (tembel silme) |
| 2 | Z-4/Z-5 son işlemden sayılır; üyenin sepeti hesabında kalır | ✓ | `SessionAccessTests.AShortSession_LivesTwoHoursFromItsLastActivity…` (546), `ALongSession_LivesThirtyDaysFromItsLastActivity` (560); sepet oturuma değil hesaba bağlı (`Cart` → `Customer` FK), `SessionDisposalPart` sepete dokunmaz |
| 3 | Sepet kendiliğinden boşalmaz; Z-6 zamanlayıcısı yok | ✓ | Unit `CartTimerScanTests` (iş türü, kaynak, kesinti etkisi, gecelik adım, iş işleyicisi yansımayla bulunur; kasıtlı örnek kırmızı) |
| 4 | Z-7'de son sorgu; ödeme varsa Ö1, yoksa iptal + serbest | ✓ | `PaymentWindowTests.CardWindowEnd_WritesOneFinalQueryRow…`, `FinalQuery_ThatFindsThePayment_AppliesO1…_KK4`, `FinalQuery_ThatFindsNoPayment_CancelsTheOrder_AndReleasesItsReservation_KK4` |
| 5 | Yanıtsız sorguda iptal yok, işaret var, yanıta dek yinelenir | ✓ | `UnansweredFinalQuery_…_KK5` (TimedOut/Unreachable/Unrecognized), `CardWindowEnd_WritesTheQueryAgain_WhenTheRunnerGaveUpOnTheRow_KK5`; Unit `UnknownMark_…_KK5` |
| 6 | Z-8'de havale iptali; iptal sonrası "ödendi" yapılamaz | ✓ | `TransferWindowEnd_CancelsTheOrder_WithS3AndO2AndB7_AndReleasesItsReservation_KK6`, `CancelledTransferOrder_CannotBeMarkedPaidAfterwards_KK6` |
| 7 | B-3 bir kez, düzeltmede bir kez daha; kesinti dönüşünde süre dolmuşsa gitmez | ✓ | `Reminder_GoesOnceAtItsMoment_AndOnceMoreWhenACorrectionWritesANewMoment_KK7`, `MissedReminder_…_KK7`, `WakeUpBeforeTheReturnsFirstHeartbeat_…_D100`; Unit `Reminder_…_KK7` |
| 8 | Havale süresi firma ayarından; öteki süreler `ProductConstants`'tan; sayı başka yerde yok | ✓ | `TransferWindow_FollowsTheFirmSetting_AndTheCardWindow_TheProductConstant_KK8`; diff taraması (aşağıda) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt (dosya:satır, test adı, grep) |
|---|---|---|
| KK1 bağlantı süresi/yenisi; Z-1 silmesi imha kaydında | ✓ | `PendingRecordExpiry.cs:23,38`; `PendingRecordDisposalTests` 61, 89, 114; `LinkLifetimeTests` |
| KK2 oturum son işlemden | ✓ | `SessionAccessTests` 546, 560, 572 (T2xx kodu; T324 değiştirmedi) |
| KK3 sepet için süre işi yok | ✓ | `CartTimerScanTests`; `grep -rn "Carts\|CartItems" src` → yalnız `AppDbContext`, `GuestCartDisposalPart` (06 §9.4.3 saklama, zamanlayıcı değil), `OrderingModule` |
| KK4 Ö1 / iptal | ✓ | `FinalPaymentQueryJobHandler.cs:57-75`; `PaymentWindowTests` 224, 241 |
| KK5 yanıtsızda iptal yok, işaret, vazgeçmeden | ✓ | `JobOutcome.RescheduleAfter` deneme sayısını artırmaz; `PaymentWindowSources.cs:88-122` (sipariş beklerken hep zamanı gelmiş); `PaymentWindowTests` 191, 208, 264 |
| KK6 iptal edilmiş havale "ödendi" olmaz | ✓ | `PaymentWindowTests` 98 (`MarkTransferPaidAsync` → `IsRejected`) |
| KK7 B-3 bir kez / düzeltmede / dönüşte | ✓ | `PaymentWindowRules.cs:39-41` (`PaymentWindowEndsAt > now`); B-3 tekilliği pencere sonu başına `CustomerNoticeQueue.ReminderIsDueAsync` (T305) |
| KK8 firma ayarı / ürün sabiti | ✓ | `PaymentWindowRules.cs:73-76`; `git diff origin/main -- src \| grep -E "From(Minutes\|Hours\|Days)\([0-9]\|\b(30\|24\|60\|72)\b"` → eşleşme yok |
| `09 §25` tek işlev: ayırma serbest bırakma tek yol | ✓ | `OrderTransitions.cs` `ReleaseReservationAsync` → `ICatalogModule.ReleaseAsync` (T334 `StockCounters`); ödenmemiş iptalin tek yolu `CancelOrderAsync` (T317 r2 notu) |
| `09 §25` modül sınırı; arayüz yöntemi `SaveChanges` yok | ✓ | `GetCardPaymentQueryTargetAsync` yalnız okur (`AsNoTracking`); Unit mimari taramaları yeşil (4273) |
| `09 §25` eşzamanlılık / satır kilidi | ✓ | Kaynaklar `ScheduledOrderTransition` tabanında sipariş kilidi + kilitli yeniden sınama; işleyicinin iptali ve Ö1'i geçiş biriminin kilidi altında; değişken ve kupon kilidi siparişten sonra (`05 §7.3.5`, `StockCounters`) |
| `09 §25` arka plan işi kalıbı | ✓ | İşleyici işlem dışında çağrılır, sağlayıcı çağrısı kilitsiz, sonuç kendi işleminde; kaynaklar `AddDueRecordSource`; `FinalPaymentQuery` kaydı D-449 (yönetici kararı) |
| `09 §25` zaman `TimeProvider`'dan | ✓ | diff'te `DateTime.Now/UtcNow` yok |
| `09 §25` göç / yeni ayar | ✓ | göç yok; yeni süre yok (P-7, P-12…P-14, P-18 mevcut sabit ve ayar) |
| `09 §25` mimari kural için kasıtlı ihlal örneği | ✓ | `CartTimerScanTests.CartTablePattern_SeesTheCartsSetsAndTypes_AndNotACartIdInAComment` |
| `12` VAL-E056 (yanıtsız son sorgu iptal etmez, beş dakikada bir) | ✓ | `UnansweredFinalQuery_…_KK5` (`JobTimings.FinalPaymentQueryInterval`) |
| `12` VAL-A826 (nabız eskiyken yalnız havale iptali bekler) | ✓ | `TransferPaymentExpiry.WaitsForFreshHeartbeat = true`, ötekiler false (Unit `OnlyTheTransferCancellation_WaitsForAFreshHeartbeat`); `WakeUpBeforeTheReturnsFirstHeartbeat_…_D100` |
| `12` VAL-D002 (sepet kendiliğinden boşalmaz) | ✓ | `CartTimerScanTests` |
| `12` VAL-A976 (onay ile iptal aynı kilit sırası) | ⚠ | İptal tarafı sipariş → değişken/kupon sırasıyla; onay tarafı T348'de — yarış testi T348'in |
| Sık FAIL: istisnası geniş bekçi / kaçan çağrı biçimi | ✓ | Tarama tip listesi yerine yansıma; desen `Carts`, `CartItems`, `CartItem`, `<Cart>`, `Cart x =` biçimlerini görür, örnekle sınandı |
| Sık FAIL: aynı sayıyı anan öteki yerler | ✓ | `FinalPaymentQueryRule` özeti, D-97, D-99 ("kart ödemesinin süresi (T403)" payı), D-100, D-253, `PaymentModule` yorumu güncel |
| Sık FAIL: main'in yeni alanları | ✓ | `origin/main` (T302, #446) birleştirildi; birleşik ağaçta testler yeşil |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/...` | 4273 geçti, 0 hata (19'u yeni) |
| Integration | ✓ | `--filter-class` / `--filter-namespace` parçaları (yerel PostgreSQL 18, `t324`) | PaymentWindowTests 15, PendingRecordDisposalTests 3; Payment/Shipment/OrderTransitions 158; Membership + Scheduler + Disposal + Outage 203; Catalog/Concurrency/CorporateContent/Database/FileSecrets/Files/Jobs/Logging/Security/SignedEndpoints/Ordering 575; Model 1190; Web parçaları 61 + 114 + 104 — hepsi geçti; istisna aşağıda |
| Format / derleme | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror`, `dotnet format --verify-no-changes` | 0 uyarı, 0 hata |
| Contract | — | — | bu task'ta yok |

Yerel istisnalar (T324'ten bağımsız, `origin/main`'de de aynı): `VersionedFormPageTests.OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8` 23505 (D-378/D-383/D-392; `origin/main` `bcb856d`'de aynı test aynı hatayla düştü); `LivenessEndpointTests` süreci bitirmiyor (D-391) — bu yüzden Web'in ilk yarısı parçası zaman sınırında kesildi (181 geçti, 0 hata).

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T324-zaman-asimi`
- **Commit:** PR başlığında; head SHA PR'da
- **PR:** PR açıklamasında
- **CI:** doğrulayıcı izler

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `Paid.ProviderTransactionId` ve tutar/tip/para birimi denetimi gerçek yanıttan | D-448 | sağlayıcı katmanı T403 |
| 2 | `FinalPaymentQueryJobHandler` DI kaydı | D-449 | `IPaymentGateway` uygulaması T403'te (yönetici kararı) |

⚠ listesi (kurala en uygun seçenekle karar verildi):
- (a) Z-3'ün öteki geçersizleşme koşulları (davet geri alınınca vb.) T379 k8'dedir; T324 süre ve "yenisi istenince"yi sınar.
- (b) "Ödendi" düzeltmesinde yeni pencere sonu ve hatırlatma anının yazılması T319 k8'dedir (`PaymentWindows.StartAsync` hazır); T324 testi bu yazımı SQL ile canlandırır.
- (c) Sipariş onayı pencereyi henüz yazmaz (T348); `PaymentWindows` yalnız DI'da ve testte çağrılır.
- (d) Son sorgunun başarı yanıtı tutar/`KART`/`TL` denetimi olmadan uygulanır; denetim T403 k5 (D-448).
- (e) Kriter 2'nin kanıtı önceki task'ların kodudur (`SessionAccessTests`, `SessionDisposalPart`); T324 oturum kodunu değiştirmedi.
- (f) Bekleyen e-posta değişikliğinin ömrü kaydın `CreatedAt`'inden + P-12 sayılır (değişiklik ilk bağlantısıyla yazılır; yeniden gönderim ömrü uzatmaz, `02 §3.13.5`).

## Notlar

- Önceki iki yapım işçisi bağlam sınırında bölündü; kod onların, test derlemesi/düzeltmeleri, unit testler, belge ve öz denetim bu turun.
