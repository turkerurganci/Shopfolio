# T324 — Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; merge yardımcıda) | **Tarih:** 2026-11-08

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
| Sık FAIL: main'in yeni alanları | ✓ | `origin/main` (T302, #446; sonra #452) birleştirildi; birleşik ağaçta derleme `-warnaserror` 0/0, Unit 4273, `PaymentWindowTests` 15 + `PendingRecordDisposalTests` 3 yeniden koşuldu, yeşil |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/...` | 4273 geçti, 0 hata (19'u yeni) |
| Integration | ✓ | `--filter-class` / `--filter-namespace` parçaları (yerel PostgreSQL 18, `t324`) | PaymentWindowTests 15, PendingRecordDisposalTests 3; Payment/Shipment/OrderTransitions 158; Membership + Scheduler + Disposal + Outage 203; Catalog/Concurrency/CorporateContent/Database/FileSecrets/Files/Jobs/Logging/Security/SignedEndpoints/Ordering 575; Model 1190; Web parçaları 61 + 114 + 104 — hepsi geçti; istisna aşağıda |
| Format / derleme | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror`, `dotnet format --verify-no-changes` | 0 uyarı, 0 hata |
| Contract | — | — | bu task'ta yok |

Yerel istisnalar (T324'ten bağımsız, `origin/main`'de de aynı): `VersionedFormPageTests.OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8` 23505 (D-378/D-383/D-392; `origin/main` `bcb856d`'de aynı test aynı hatayla düştü); `LivenessEndpointTests` süreci bitirmiyor (D-391) — bu yüzden Web'in ilk yarısı parçası zaman sınırında kesildi (181 geçti, 0 hata).

Main sonrası düzeltme (2026-11-08): T336'nın üretim yeri kuralıyla `PaymentWindowTests.SeedReservationAsync` fiziksel ürün tohumu `.WithSeededOrigin()` alır (yalnız test tohumu, davranış değişmedi); derleme `-warnaserror` 0 uyarı, format temiz, `PaymentWindowTests` + `PendingRecordDisposalTests` 18/18 geçti (PostgreSQL 18).

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-08, head `2b5e108` = `6a8d862` + `origin/main` `b345583` (T358) birleştirmesi). Kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓ |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-464 (Z-3 davetin "öteki koşullar" kolu T379 k8 / T520 k9'da sınanır; işlemleri T324'te yok), K2 → D-465 (`PaymentWindows.StartAsync`'i üretimde T348 sipariş onayı ve T319 k8 düzeltmesi çağırır) |

**Doğrulayıcı kanıtları (tur 1):**

| # | Kriter / KK | Sonuç | Doğrulayıcı kanıtı |
|---|---|---|---|
| 1 / KK1 | Z-1/Z-2/Z-3 süre dolunca ve yenisi istenince geçersiz; Z-1 silmesi imha kaydında | ✓ | Bağlantı kuralı tek yerde: `LinkRecords.StateOf`, `TryConsumeAsync` (`ExpiresAt > now`, `SupersededAt == null`), `SupersedeEarlierAsync`; `LinkLifetimeTests` dört bağlantı türü için ömrü `ProductConstants`'tan okur (P-12/P-13/P-14). `PendingRecordExpiry.EndedRegistration` (ilk bağlantı ölçüdür, yeniden istek ömrü uzatmaz) tembel silmede (`RegisterAsync`) ve gece parçasında aynı ifade; `EndedEmailChange` = `CreatedAt` + P-12. Integration `PendingRecordDisposalTests` 3/3 (sayı imha kaydında, genç kayıt kalır, boşta kayıt yazılmaz); `MembershipTests` (yeniden istek öncekileri geçersiz kılar ve ilk bağlantının sonunda biter; dolmuş bağlantı geçersiz; dolmuş kayıtta yeniden kayıt adresi boşaltır ve sayar) yeşil. Z-3'ün "öteki koşullar"ını doğuran işlemler T379/T520'nin (K1 → D-464) |
| 2 / KK2 | Z-4/Z-5 son işlemden; üyenin sepeti hesapta | ✓ | `SessionAuthenticationHandler` `now - LastActivityAt > ömür` (P-15/P-16 sabitten); `SessionAccessTests` (`AShortSession_LivesTwoHoursFromItsLastActivity…`, `ALongSession_LivesThirtyDaysFromItsLastActivity`, `TheLastActivity_IsWrittenAtMostOnceAMinute`) yeşil; `Cart.CustomerId` → `Customer` FK, oturumla bağı yok |
| 3 / KK3 | Sepet kendiliğinden boşalmaz; Z-6 zamanlayıcısı yok | ✓ | `grep -rnE "\bCarts\b\|\bCartItems\b" src` → yalnız `AppDbContext`, `GuestCartDisposalPart` (400 gün misafir sepeti, `06 §9.4.3` saklama kuralı) ve `OrderingModule` (ödeme onayında sepetten çıkarma); `CartTimerScanTests` (yansımayla bulunan kaynak/kesinti etkisi/gecelik adım/işleyici sepete dokunmaz; desen örnekle sınanır) yeşil |
| 4 / KK4 | Z-7'de son sorgu; ödeme varsa Ö1, yoksa iptal ve serbest | ✓ | `CardPaymentExpiry` pencere bitince `FinalPaymentQuery` satırını yazar (bekleyen tekil satır); `FinalPaymentQueryJobHandler` sağlayıcıyı kilitsiz sorar, ödeme → `ApplyCardPaymentAsync`, ödeme yok + pencere bitmiş → sistem iptali (S3+Ö2, B-7, `ReleaseReservationAsync`); `PaymentWindowTests` KK4 testleri yeşil. Gerçek sağlayıcı ve DI kaydı T403 (D-97, D-448, D-449) |
| 5 / KK5 | Yanıtsızda iptal yok, işaret var, vazgeçmeden yinelenir | ✓ | `AskAgain` → `RescheduleAfter(JobTimings.FinalPaymentQueryInterval)` (deneme sayısı artmaz); işaret saklanmaz, `PaymentWindowRules.IsPaymentOutcomeUnknown` (`06` K-675 türetilmiş değer); çalıştırıcı satırdan vazgeçse de sipariş zamanı gelmiş kalır, sonraki uyanış yeni satır yazar; `UnansweredFinalQuery_…_KK5` (3 hata türü × 3 tur), `CardWindowEnd_WritesTheQueryAgain_WhenTheRunnerGaveUpOnTheRow_KK5` yeşil |
| 6 / KK6 | Z-8'de havale iptali; iptal edilmiş sipariş "ödendi" olmaz | ✓ | `TransferPaymentExpiry` (`WaitsForFreshHeartbeat = true`), kilitli ikinci bakış aynı ifade; sürüm bir kez artar (`RaiseVersionOnce`); `TransferWindowEnd_…_KK6` (S3+Ö2, B-7, ayırma ve kupon hakkı serbest, stok kıpırdamaz), `CancelledTransferOrder_CannotBeMarkedPaidAfterwards_KK6` yeşil |
| 7 / KK7 | B-3 bir kez; düzeltmede bir kez daha; dönüşte süre dolmuşsa gitmez | ✓ | `TransferReminderDue` = an gelmiş ∧ `PaymentWindowEndsAt > now`; gönderimden sonra `PaymentReminderAt` boşalır; `Reminder_…_KK7`, `MissedReminder_…_KK7`, `WakeUpBeforeTheReturnsFirstHeartbeat_…_D100` yeşil |
| 8 / KK8 | Havale süresi firma ayarından; öteki süreler `ProductConstants`'tan; sayı başka yerde yok | ✓ | `PaymentWindows.StartAsync` havalede `ISettingsModule.GetPeriodsAsync().BankTransferPaymentWindow` (iş günü), kartta `ProductConstants.CardPaymentMinutes`; `TransferWindow_FollowsTheFirmSetting_…_KK8` (1 ve 3 iş günü farklı son) yeşil; diff'in kod satırlarında sayı sabiti taraması boş. Üretimde çağıranlar T348/T319 (K2 → D-465) |
| `09 §25` | ilgili satırlar | ✓ | zaman `TimeProvider`; modül sınırı (`IOrderingModule.GetCardPaymentQueryTargetAsync` salt okuma, `AsNoTracking`); iç yan etki (ayırmanın serbest kalması) iptalle aynı işlemde, kilit sırası sipariş → değişken/kupon; işleyici işlem dışında, sonuç kendi işleminde; iş türünün işleyicisi sahip task'ta (T403) kayıtlı; göç, paket, uç yok |
| Güvenlik | taraması | ✓ | sır yok (diff taraması); yeni uç/POST yok, yetki etkisi yok; dış girdi yok (sağlayıcı yanıtı T403); yeni bağımlılık yok (csproj/kilit dosyası değişmedi) |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana çalışma ağacının `scripts/git-hooks`'u); ana dal son 3 tamamlanmış run 42443632733 (success), 42443632759 (success) — `7ddf334` (#452), 42436765868 (success) — `bcb856d` (T302); memory kapısı: 2026-11-08 kayıt kuralı (chore #450) gereği task PR'ı MEMORY "Güncel Durum"a dokunmaz, Adım 0b uygulanmadı.

**Testler (doğrulayıcı, yerel, `2b5e108`):** `dotnet restore` + `dotnet build -warnaserror` 0 uyarı/0 hata; `dotnet format --verify-no-changes` üç projede çıkış 0; Unit Architecture 961 + Consistency 242 + öteki 3128 (hepsi geçti); betik testleri 4/4; Integration (PostgreSQL 18, ayrı konteyner) `PaymentWindowTests` + `PendingRecordDisposalTests` 18, `ShipmentTransitionsTests` + `PaymentTransitionsTests` + `StockAndCapacityTests` 154, `MembershipTests` + `SchedulerRegistrationTests` + `SchedulerScanTests` + `AuditTrailTests` 68, `DisposalTests` + `SessionAccessTests` + `OrderTransitionsTests` + `AppCacheTests` 99 — hepsi geçti; koşu sonrası `git status` temiz, `dotnet restore --locked-mode` çıkış 0. Dal CI 42445269145 (`2b5e108`) bütün işler yeşil.

**`12` satırları:** KP-74 satırları (`VAL-A368`…`A370`, `D046`…`D051`). `D046`, `D047`, `D051` zaten PASS; `D049`'un üç işleminden ikisi (bekleyen kaydın ve bekleyen e-posta değişikliğinin silinmesi imha kaydında) bu task'ta kanıtlandı, IBAN'ın silinmesi T370'tedir — satır `BEKLEMEDE` kaldı; `A368`…`A370`, `D048`, `D050` ekran ya da öteki task işidir, `BEKLEMEDE`. `Durum` hücresi değişmedi.

**Yapım raporu karşılaştırması (adım 13):** hüküm uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Uyuşmazlıklar: (1) yapım raporunun andığı `VAL-E056`, `VAL-A826`, `VAL-D002`, `VAL-A976` T324'ün kaynak birimi (KP-74) dışındaki satırlardır (08 §3.1, K-898, KP-10, K-890); `Durum`'ları değiştirilmedi. (2) ⚠ (a) ve (c) maddeleri backlog'a yazılmamıştı; K1 → D-464, K2 → D-465 ile forward edildi. Not: doğrulayıcı adım 4'te `PaymentWindows`'u ararken grep çıktısında bu raporun iki satırını erken gördü (T348/T319 sahipliği); sahiplik `11` planından bağımsız doğrulandı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T324-zaman-asimi`
- **Commit:** PR başlığında; head SHA PR'da
- **PR:** #454
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** squash `e7f6ba0` (#454) — CI 42452259999 success, Publish 42452259880 success (F3 gate'te dolduruldu)

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
