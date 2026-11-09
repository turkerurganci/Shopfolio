# T353 — Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Ödeme yöntemleri bölümü (kriter 1, 3):** `IPaymentMethodSettings` (`GetAsync`/`SaveAsync`) — havale anahtarı ve IBAN öteki ayar bölümleri gibi kaydedilir: satır kilidi, `PaymentMethodsVersion` koşullu güncellemesi, eski/yeni değerli iz satırı. Açıkken IBAN zorunlu (`FieldRequired`), yazılan IBAN projenin tek kuralıyla denetlenir (`InvalidIbanAttribute.Allows` → `InvalidIban`), `RequestFormat.CompactIban` biçiminde saklanır. IBAN ilk kez girilince ya da değişince aynı işlemde bütün yöneticilere F-5 (`IAdminNotices.QueueIbanChangedAsync` → `IbanChangedNotice`, e-postada yalnız son dört hane); temizlenen IBAN kimseye bildirilmez.
- **Satış kapısı havale hattı (D-198 T353 payı):** `BankTransferCondition` (havale açık ve IBAN dolu) Settings modülünün kaydıyla bağlandı; `UnboundBankTransferCondition` silindi. Aynı okuma ödeme adımının yöntem listesini (`PaymentMethods.ListOpenAsync`) ve kapıyı besler.
- **L-8 havale kapanışı (kriter 2):** havale seçilen onayda açık ödenmemiş sipariş tavanı doluysa yeni ret `OrderConfirmationRefusal.TransferCeilingReached`; kart açık kalır. Aynı sepetin onayda iptal edilen önceki siparişi sayımdan düşülür (`OpenUnpaidOrderLimit` `excluding` aşırı yüklemesi). `IsTransferCeilingReachedAsync` ödeme adımı için açık.
- **B-2 (D-770):** havale onayı `TransferChosenNotice`'i ödeme süresinin sonuyla aynı işlemde kuyruğa koyar.
- **D-151:** `CompactIban`/`CompactPhone` `char.IsWhiteSpace` olan her karakteri atar.
- **Devam turu:** `PaymentMethodSettings` IBAN kuralını `ValidationAttribute.IsValid(object)` ile çağırıyordu (bağlamsız `ValidationContext` → `ArgumentNullException`); `FieldRuleAttribute.Allows` ile düzeltildi. Ertelenen aralık testi gerçek yönetici satırıyla kuruldu (FK). `origin/main` (T365, T368) birleştirildi; birleşik ağaçta Integration ve Unit TAM koştu.

## Etkilenen Modüller / Dosyalar

- Settings: `PaymentMethodSettings.cs`, `Contracts/IPaymentMethodSettings.cs`, `Contracts/FirmSettingRecords.cs`, `CompanySettingsWriter.cs`, `SettingsModule.cs`, `UnboundSalesGateConditions.cs`, `Contracts/ISalesGateConditions.cs`
- Administration: `Contracts/IAdminNotices.cs`, `AdminNotices.cs`, `AdministrationModule.cs`
- Ordering: `OrderConfirmation.cs`, `OpenUnpaidOrderLimit.cs`; Payment: `PaymentMethods.cs`; Infrastructure: `RequestFormat.cs`
- Testler: `FirmSettingsTests`, `SalesGateTests`, `OrderConfirmationTests`, `OrderingTestServices`, `PaymentWindowTests`, `AutomaticMomentsTests`; Unit `RequestFormatTests`, `SalesGateRulesTests`, `BankTransferAmountTests`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Havale açılır/kapatılır; açıkken IBAN zorunlu; kapalıyken yöntem yok; açık havale kapı koşulunu karşılar | ✓ | `FirmSettingsTests.SwitchingTheTransferOnWithoutAnIban_IsRefused_AndNothingIsStoredNorTold_T353_KK1`, `…TheTransferWithItsIban_OpensTheLine_AndSwitchedOffClosesIt_KeepingTheIban_T353_KK1`; `SalesGateTests.UnboundConditions_…` (havale kapalı → kapı kapalı); `PaymentMethods.ListOpenAsync` aynı `IBankTransferCondition`'ı okur |
| 2 | L-8 dolunca havale seçilemez, kart açık | ✓ | `OrderConfirmationTests.AFullOpenUnpaidOrderCeiling_ClosesTheTransfer_LeavesTheCardOpen_AndTheSameCartsNewConfirmationIsNotCountedTwice_T353_KK2` |
| 3 | IBAN siparişe donar; girilme/değişme bütün yöneticilere e-posta | ✓ | `OrderConfirmationTests.AChangedIban_ReachesOnlyTheNewOrders_TheOpenOneKeepsTheIbanItFroze_T353_KK3`; `FirmSettingsTests.TheIbanEnteredAndChanged_TellsEveryAdministrator_WithTheLastFourDigitsOnly_T353_KK3`, `…ASaveThatLeavesTheIbanAsItIs_TellsNoOne_T353_KK3` |
| 4 | Havale onayında donan IBAN ve sipariş no görünür; ödendi işareti ödemeyi onaylar | ✓ | `OrderConfirmationTests.TransferConfirmation_QueuesB2WithTheWindowEnd_AndACardConfirmationDoesNot_T353_KK4_D770` (B-2; IBAN ve numara `OrderPageSummary`'de, T354); `PaymentTransitionsTests.MarkTransferPaid_Confirmed_IsO1ByTheAdministratorAndS1ByTheSystem_OneVersionRaise_TheRecordAndB4_KK3` |
| 5 | Süre başına bir hatırlatma, son iş gününün ilk gönderim çalışmasında | ✓ | Mevcut kod (T324/T327): `TransferPaymentReminder`; `PaymentWindowTests.Reminder_GoesOnceAtItsMoment_AndOnceMoreWhenACorrectionWritesANewMoment_KK7`, `TransferScheduleTests` |
| 6 | Gelen tutar alanı, karşılaştırma, "eksik ödeme" yok | ✓ | Unit `BankTransferAmountTests` (üç test, `_T353_KK6`) |
| 7 | İptalli siparişe işaret yok, yönetici de aşamaz; ertelenen aralıkta işaret kabul | ✓ | `PaymentWindowTests.TheExpiryCancellation_IsNotCorrectedByTheAdministrator_SoTheMarkStaysClosed_T353_KK7`; `AutomaticMomentsTests.InTheDeferredGap_TheFirmCanStillMarkThePayment_AndTheDeferredMomentThenCancelsNothing_T353_KK7` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: IBAN'sız havale açılmaz, kapalıyken yöntem yok | ✓ | `PaymentMethodSettings.SaveAsync` (FieldRequired); `BankTransferCondition.IsOpenAsync`; `FirmSettingsTests` iki `_T353_KK1` testi |
| KK2: tavan dolunca havale kapanır, kart açık | ✓ | `OrderConfirmation.cs` (L-8 sayımı, `TransferCeilingReached`); `…_T353_KK2` |
| KK3: IBAN siparişte sabit, değişiklik e-postası | ✓ | `AdminNotices.cs`; `…_T353_KK3` (üç test) |
| KK4: ödendi işareti ödemeyi onaylar | ✓ | `PaymentTransitionsTests.MarkTransferPaid_Confirmed_…_KK3`; B-2 `…_T353_KK4_D770` |
| KK5: hatırlatma süre başına bir kez, doğru gün | ✓ | `PaymentWindowTests.Reminder_GoesOnceAtItsMoment_…_KK7`; `TransferScheduleTests` |
| KK6: eksik ödeme durumu ve gelen tutar alanı yok | ✓ | `BankTransferAmountTests` |
| KK7: iptalli siparişte işaret reddi, yönetici aşamaz; ertelenen aralıkta kabul | ✓ | `…_T353_KK7` (iki test) |
| `09 §25`: tek kural evi | ✓ | IBAN kuralı `InvalidIbanAttribute.Allows`; biçim `RequestFormat.CompactIban`; L-8 sayımı `OpenUnpaidOrderLimit` (ikinci sorgu yok) |
| `09 §25`: modül arayüzü işlem açmaz / SaveChanges çağırmaz | ✓ | `PaymentMethodSettings` ve `AdminNotices` çağıranın işleminde yazar (testler `ITransactionRunner` içinde) |
| `09 §25`: eşzamanlılık | ✓ | Satır kilidi + `PaymentMethodsVersion` koşullu güncelleme; `FirmSettingsTests.APaymentMethodsSaveOnAStaleVersion_IsAConflict_AndTellsNoOne` |
| `09 §25`: sayım veritabanında | ✓ | `OpenUnpaidOrderLimit` sayımı `CountAsync` |
| `09 §25`: modül kurucusuna yeni bağımlılık → elle kuran düzenekler | ✓ | `OrderConfirmation` ve `PaymentMethodSettings` bağımlılıkları; Integration TAM birleşik ağaçta 3211/3211 |
| `09 §25`: backlog'da bu task'a devredilenler | ✓ | D-151, D-770, D-787, D-795, D-198 (T353 payı) kapandı; D-668 → T511, D-566 → T511 (T353 payı ⚠ karar) |
| Güvenlik taraması | ✓ | Sır yok; yetki: ayar yazımı yalnız panel/yönetici (sayfa T536); IBAN girdi doğrulaması tek kural evi; e-postada yalnız son dört hane, iz satırı eski/yeni değeri tutar (`02 §9.3.3`); yeni dış bağımlılık yok; göç yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/...` (birleşik ağaç) | 5526/5526 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-namespace` üç parça (birleşik ağaç; postgres:18, `track_commit_timestamp=on`) | 1219 + 1398 + 594 = 3211/3211 |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <T353 .cs dosyaları>` | değişiklik yok |
| Derleme | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (havale sütunları ve `PaymentMethodsVersion` önceden vardı)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T353-havale-eft`
- **Commit:** dal head'i PR'da
- **PR:** #500
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok (çekirdek; banka entegrasyonu yok, havale elle işaretlenir) | 11 T353 kriterleri | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | IBAN değişikliği/kapatma onayında ödemesi beklenen havale siparişi sayısı | D-833 | Onay ekranı E-45 (T536); sayım işlevi onunla |
| 2 | Sipariş sayfasının ertelenen son günü | D-668 → T511 | Razor sayfası T511 |
| 3 | IBAN'ın ödendi sonrası görünümü | D-566 → T511 | ⚠ yapım kararı: çekirdek her durumda verir |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi, `origin/task/T353-havale-eft` `b9fe0993`'ten)
- **Ana dal CI startup (Adım 0):** 42600847174 success · 42600846924 success · 42596386708 success; hook yolu `scripts/git-hooks`
- **Repo memory (bitiş kapısı 9):** dokunulmadı
- **⚠ kararlar:** (1) D-566 T353 payı — IBAN ödendi sonrası da gösterilir (belge gizlemeyi yazmaz); (2) D-668 sayfa işi T511'e; (3) D-833 hedef T536 (E-45 ödeme yöntemleri ekranı).
- **Diğer:** `LivenessEndpointTests.Start_WithoutConnectionString_…` birleşme öncesi Web parçasında bir kez `ObjectDisposedException` ile düştü, tek başına ve birleşik ağaçtaki Web parçasında geçti (kararsız, T353 ile ilgisiz).
