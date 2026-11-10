# T353 — Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Ödeme yöntemleri bölümü (kriter 1, 3):** `IPaymentMethodSettings` (`GetAsync`/`SaveAsync`) — havale anahtarı ve IBAN öteki ayar bölümleri gibi kaydedilir: satır kilidi, `PaymentMethodsVersion` koşullu güncellemesi, eski/yeni değerli iz satırı. Açıkken IBAN zorunlu (`FieldRequired`), yazılan IBAN projenin tek kuralıyla denetlenir (`InvalidIbanAttribute.Allows` → `InvalidIban`), `RequestFormat.CompactIban` biçiminde saklanır. IBAN ilk kez girilince ya da değişince aynı işlemde bütün yöneticilere F-5 (`IAdminNotices.QueueIbanChangedAsync` → `IbanChangedNotice`, e-postada yalnız son dört hane); temizlenen IBAN kimseye bildirilmez.
- **Satış kapısı havale hattı (D-198 T353 payı):** `BankTransferCondition` (havale açık ve IBAN dolu) Settings modülünün kaydıyla bağlandı; `UnboundBankTransferCondition` silindi. Aynı okuma ödeme adımının yöntem listesini (`PaymentMethods.ListOpenAsync`) ve kapıyı besler.
- **L-8 havale kapanışı (kriter 2):** havale seçilen onayda açık ödenmemiş sipariş tavanı doluysa onay reddedilir; kart açık kalır. Düzeltme 1'den sonra ret main'in tek yolundan gelir (T377: `OrderPlacementLimits.FindExceededAsync` → `OrderConfirmationRefusal.AttemptLimitExceeded`, `ExceededLimit = L8`). Aynı sepetin onayda iptal edilen (henüz kaydedilmemiş) önceki siparişi sayılmaz: `OpenUnpaidOrderLimit.Open()` bu iş biriminin Pending'den çıkardığı siparişleri sayımdan düşer.
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
| KK2: tavan dolunca havale kapanır, kart açık | ✓ | `OrderPlacementLimits.FindExceededAsync` (L-8 yalnız havalede); `OpenUnpaidOrderLimit.Open()` (aynı sepetin kaydedilmemiş iptali düşülür); `…_T353_KK2` (`AttemptLimitExceeded` + `L8`) — düzeltme 1'de birleşik ağaçta yeniden koştu |
| KK3: IBAN siparişte sabit, değişiklik e-postası | ✓ | `AdminNotices.cs`; `…_T353_KK3` (üç test) |
| KK4: ödendi işareti ödemeyi onaylar | ✓ | `PaymentTransitionsTests.MarkTransferPaid_Confirmed_…_KK3`; B-2 `…_T353_KK4_D770` |
| KK5: hatırlatma süre başına bir kez, doğru gün | ✓ | `PaymentWindowTests.Reminder_GoesOnceAtItsMoment_…_KK7`; `TransferScheduleTests` |
| KK6: eksik ödeme durumu ve gelen tutar alanı yok | ✓ | `BankTransferAmountTests` |
| KK7: iptalli siparişte işaret reddi, yönetici aşamaz; ertelenen aralıkta kabul | ✓ | `…_T353_KK7` (iki test) |
| `09 §25`: tek kural evi | ✓ | IBAN kuralı `InvalidIbanAttribute.Allows`; biçim `RequestFormat.CompactIban`; L-8 sayımı `OpenUnpaidOrderLimit`, ret `OrderPlacementLimits` (düzeltme 1: T353'ün ikinci L-8 yolu kaldırıldı) |
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
| Doğrulama durumu | ✓ PASS (tur 2) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (D-852, D-853; tur 2'de yeni K yok) |

### Doğrulama tur 2 — Doğrulama Sonucu — T353 Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş — tur 2
**Tarih:** 2026-11-09   **Branch:** `task/T353-havale-eft`   **Commit:** `3189dd97` + birleştirme `d45313e5` (`origin/main` `d5676e75`, T372 #492)

Kapsam TAM (kriter 1–7, KK1–KK7, `09 §25`, güvenlik taraması baştan; tur 1 raporuna dayanılmadı). Verdict: **✓ PASS**.

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş; `core.hooksPath` = ana depo `scripts/git-hooks` |
| Ana dal CI (son 3) | ✓ | 42627783852 success, 42627783617 success, 42619534143 success |
| Dal CI (8a) | ✓ | PR #500 head `3189dd97`: run 42625668294 success (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate pass) |

#### Kabul Kriterleri (doğrulayıcı kanıtı)
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `PaymentMethodSettings.SaveAsync`: IBAN boş + açık → `FieldRequired`, bozuk → `InvalidIban`, hiçbir şey yazılmaz; `BankTransferCondition.IsOpenAsync` (açık ∧ IBAN dolu) kapıyı ve `PaymentMethods.ListOpenAsync`'i besler, onay kapalı yöntemi `PaymentMethodUnavailable` ile reddeder; `FirmSettingsTests` `_T353_KK1` ×2, `SalesGateTests` geçti. UC-167'nin 422 kodu (`BankTransferIbanRequired`) uçla birlikte T536 k3'tedir |
| 2 | ✓ | `OrderPlacementLimits.FindExceededAsync`: L-8 yalnız `BankTransfer`'de, kart sorulmaz; `OpenUnpaidOrderLimit.Open()` aynı sepetin onayda iptal edilen siparişini düşer; `OrderConfirmationTests…_T353_KK2` geçti |
| 3 | ✓ | `OrderConfirmation` IBAN'ı onay anında ayardan okur, `OrderFreeze.FreezeLegalIdentity` siparişe yazar; `AdminNotices.QueueIbanChangedAsync` bütün yönetici adreslerine F-5 (son dört hane), boşaltılan IBAN bildirim üretmez; `…AChangedIban_ReachesOnlyTheNewOrders…_T353_KK3`, `FirmSettingsTests` `_T353_KK3` ×2 geçti |
| 4 | ✓ | `OrderAccess` havale siparişinde donmuş IBAN ve numarayı verir; B-2 aynı işlemde (`…_T353_KK4_D770`); `MarkTransferPaidAsync` Ö1 + S1 (`PaymentTransitionsTests.MarkTransferPaid_Confirmed_…_KK3`) geçti |
| 5 | ✓ | `TransferSchedule.ReminderAt` son iş gününün 08:00'i (`SchedulerTimings.TransferReminderHour`); `TransferPaymentReminder` anı boşaltır (bir kez); düzeltme yeni an yazar; `PaymentWindowTests.Reminder_…_KK7`, `StatusCorrectionTests…_KK3_KK8`, Unit `TransferScheduleTests`, `SchedulerTimingsConsistencyTests` geçti |
| 6 | ✓ | Unit `BankTransferAmountTests` ×3 (`PaymentStatus`'ta eksik ödeme yok, `TransferPaidMark`'ta ve kayıtlarda tutar alanı yok) |
| 7 | ✓ | `CancellationIsCorrectableAsync`: sistemin S3'ü firma iptali değildir → `NotCorrectable`; iptalli siparişte Ö1 reddi; `TransferWindowDeferral` ertelenen aralıkta işarete izin verir; `PaymentWindowTests…_KK6`, `…_T353_KK7`, `AutomaticMomentsTests…_T353_KK7` geçti |

#### Task Satırları (`12 §7.5`)
KP-15 `VAL-B009`, KP-46 `VAL-C039`, KP-59 `VAL-A288`, `A289`, `D032`, `A290`: kanıt türü uç yanıtı / tarayıcı gözlemidir (sipariş sayfası T511, ayar ekranı T536); T353 çekirdek kanıtıyla tamamlanmaz (T350/T352 doğrulamalarındaki uygulama). `Durum` değişmedi (BEKLEMEDE). `12_matris_denetle.py --kapsam 1-77` Hata 0.

#### Doğrulama Kontrol Listesi
KK1–KK7 ✓ (üstteki tablo). `09 §25`: modül sınırı (Settings → `Administration.Contracts`; Architecture testleri yeşil), tek kural evi (`InvalidIbanAttribute`, `CompactIban`, L-8 tek yolu), eşzamanlılık (satır kilidi + `PaymentMethodsVersion` koşullu güncellemesi), iz aynı işlemde (`UpdateSettings`, eski/yeni değer — `02 §9.3.3`), zaman `TimeProvider`'dan, yeni paket / göç / ayar yok ✓.

#### Test Sonuçları (birleşik ağaç)
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore / Build | ✓ | `dotnet restore Shopfolio.slnx`; `dotnet build … --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 | değişiklik yok |
| Unit (+Architecture, Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 5559/5559 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (dokunulan sınıflar) | ✓ | `FirmSettingsTests`, `SalesGateTests`, `AutomaticMomentsTests`, `OrderConfirmationTests`, `PaymentWindowTests` | 99/99 |
| Integration (komşu + T372) | ✓ | `OrderingTestServices` kullanan Ordering sınıfları, `PaymentTransitionsTests`, `StatusCorrectionTests`, `WithdrawalDeclarationTests`, `ItemRecordsTests`, `SchedulerHeartbeatTests`, `CustomerNoticeQueueTests` | 257/257 |
| Sonrası | ✓ | `git status` temiz; `dotnet restore --locked-mode` | çıkış 0 |

postgres:18, `-c track_commit_timestamp=on`, port 55640.

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (yalnız testlerde örnek IBAN'lar)
- Auth etkisi: Temiz (uç eklenmedi; kayıt çağıranın işleminde, iz için oturumdaki yönetici gerekir)
- Input validation: Temiz (IBAN projenin tek kuralıyla; her boşluk karakteri atılır — D-151)
- Yeni bağımlılık: Yok

#### Bulgular
Yok.

#### Bloke-etmeyen Notlar
Yeni K yok; tur 1'in D-852 (T530) ve D-853 (T509) kayıtları yerinde.

#### Birleştirme notu
`origin/main` (`d5676e75`, T372 #492) birleştirildi; kod çakışması yok. Yalnız `Docs/DEFERRED_BACKLOG.d/T327.md` çakıştı: main D-667'yi (T372 payı kapandı), dal D-668'i (T353 payı → T511) değiştirmişti; iki tarafın değişikliği satır satır korundu.

#### Yapım Raporu Karşılaştırması
Tam uyumlu; öz denetim tablosu var ve her madde kanıtlı. Farklar: Integration'ı dokunulan ve komşu sınıflarla koştum (yapım TAM); Unit sayısı birleşik ağaçta 5559.

### Doğrulama tur 1 — Doğrulama Sonucu — T353 Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş — tur 1
**Tarih:** 2026-11-09   **Branch:** task/T353-havale-eft   **Commit:** `9204f00d` (doğrulanan dal ucu), PR #500

**Verdict: ✓ PASS**

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook | ✓ | `core.hooksPath` ana çalışma kopyasının `scripts/git-hooks`'u |
| Ana dal CI (son 3) | ✓ | 42613944083 success, 42613944248 success, 42611407627 success |

#### Kabul Kriterleri (doğrulayıcı kanıtı)
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `PaymentMethodSettings.SaveAsync`: açıkken IBAN yoksa `FieldRequired`, geçersizse `InvalidIban`, hiçbir şey yazılmaz; `BankTransferCondition` (açık + IBAN dolu) `TryAddScoped` ile tek kayıt, kapı (`SalesGates`) ve yöntem listesi (`PaymentMethods.ListOpenAsync`) aynı arayüzü okur; kapalı yöntemi onay `PaymentMethodUnavailable` ile reddeder. Testler `FirmSettingsTests` iki `_T353_KK1`, `SalesGateTests.UnboundConditions_…`, `PaymentMethodTests.OpenMethods_…`, `OrderConfirmationTests` (satır 590) |
| 2 | ✓ | `OrderConfirmation.IsTransferCeilingReachedAsync` (IP ekseni herkes, e-posta ekseni yalnız üye — `04` 5.13.5: üyenin iletişim e-postası hesabınki), yalnız havalede; aynı sepetin onayda iptal edilen siparişi `excluding` ile düşülür. `…AFullOpenUnpaidOrderCeiling_ClosesTheTransfer_LeavesTheCardOpen_…_T353_KK2` |
| 3 | ✓ | IBAN onayda `GetBankTransferIbanAsync` ile siparişe yazılır (`OrderFreeze`); `…AChangedIban_ReachesOnlyTheNewOrders_…_T353_KK3`. F-5: `AdminNotices` bütün yönetici adreslerine (yapan dahil, firma adresine değil), `IbanChangedNotice` yalnız son dört hane + yapan + an (`02 §9.3.3`); temizlenen ya da aynı kalan IBAN bildirim üretmez; iz satırı eski/yeni değerli. `FirmSettingsTests` `_T353_KK3` iki test + sürüm çakışması testi |
| 4 | ✓ | B-2 havale onayında aynı işlemde (`TransferChosenNotice`; şablon IBAN bloğu + numara + son gün); sipariş sayfası özeti donan IBAN'ı verir (`OrderAccess`); "ödendi" işareti Ö1 (`MarkTransferPaidAsync`, `PaymentTransitionsTests`, `AxesTieTests`). `…TransferConfirmation_QueuesB2WithTheWindowEnd_…_T353_KK4_D770` |
| 5 | ✓ | Mevcut kod: `TransferSchedule.ReminderAt` = son günün 08:00'i (`SchedulerTimings.TransferReminderHour`, `05 5.3.1`); `TransferPaymentReminder` gönderip anı boşaltır (bir kez); düzeltme yeni an yazar. `PaymentWindowTests.Reminder_GoesOnce…_KK7`, `MissedReminder_…`, `StatusCorrectionTests.TheTransferMarksCorrection_…_TheReminderGoesASecondTime_…`, `TransferScheduleTests` |
| 6 | ✓ | `BankTransferAmountTests` (ödeme ekseni beş durum, işaret tutar taşımaz, `Order`/`PaymentRecord`'da gelen tutar alanı yok); elle: diff'te tutar alanı yok |
| 7 | ✓ | `CancelledTransferOrder_CannotBeMarkedPaidAfterwards_KK6`, `TheExpiryCancellation_IsNotCorrectedByTheAdministrator_…_T353_KK7` (red yolu kodda `CancellationIsCorrectableAsync` → `NotCorrectable`; test yalnız `IsRejected` der → K1), `InTheDeferredGap_TheFirmCanStillMarkThePayment_…_T353_KK7` |

#### Task Satırları (`12 §7.5`)
KP-15 (10), KP-46 (`VAL-C039` ilgili), KP-59 (`VAL-A288`, `A289`, `D032`, `A290` ilgili); KD-3'ün satırı yok. İlgili satırların hepsi "Uç yanıtı" ya da "Tarayıcı gözlemi" ister — ayar ekranı T536, ödeme adımı/teyit T509/T510, sipariş sayfası T511, panel işareti T527; çekirdek kanıtı satırı tamamlamaz. `Durum` değişmedi (VAL-C001 PASS kalır, ötekiler BEKLEMEDE). `12_matris_denetle.py --kapsam 1-77` Hata 0.

#### Test Sonuçları
| Tür | Sonuç | Komut / kapsam |
|---|---|---|
| Build | ✓ 0 uyarı | `dotnet build Shopfolio.slnx --no-restore -warnaserror` |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 |
| Unit | ✓ 5526/5526 | TAM koşu (Consistency, Architecture dahil) |
| Betik | ✓ | `tests/scripts/run-script-tests.sh` 4 dosya |
| Integration | ✓ 1194/1194 | `FirmSettingsTests`+`SalesGateTests`+`LegalTextTests` 67, Ordering ad alanı 449, Payment+Web+Commands 678 |
| Dal CI | ✓ | run 42616258907 (head `9204f00d`) success — Integration dahil |
| Kilit | ✓ | koşu sonrası `git status` temiz |

#### Güvenlik Kontrolü
Secret sızıntısı temiz · auth etkisi yok (yeni uç yok; ayar yazımı iz satırı için oturumdaki yöneticiyi ister) · girdi doğrulaması: IBAN tek kural evinden (`InvalidIbanAttribute.Allows`), sıkıştırma `RequestFormat.CompactIban`; F-5 e-postası tam IBAN taşımaz · yeni bağımlılık yok, göç yok.

#### Bulgular
Yok.

#### Bloke-etmeyen Notlar
| # | Not | Karar |
|---|---|---|
| K1 | KK7'nin düzeltme testi reddin sebebini (`CorrectionNotPossible`/`NotCorrectable`) sınamıyor | D-852 (`DEFERRED_BACKLOG.d/T353.md`), hedef T530 |
| K2 | Ödeme adımının yöntem listesi L-8 cevabını onayın kullandığı tek işlevden okumalı | D-853, hedef T509 |

#### Birleştirme notu
`origin/main` (`9d0d611c`, T377 #497 ve T329 #499) ile birleştirme kodda çakıştı: `src/Shopfolio.Web/Modules/Ordering/OpenUnpaidOrderLimit.cs`, `src/Shopfolio.Web/Modules/Ordering/OrderConfirmation.cs`. T377 aynı L-8 havale reddini `OrderPlacementLimits.FindExceededAsync` → `OrderConfirmationRefusal.AttemptLimitExceeded` ile yazdı (L-7 de); T353'ün `TransferCeilingReached` reddi ve `excluding` sayımı onunla örtüşür. Birleştirme geri alındı; doğrulama dalın ucunda yapıldı. Çözümde tek L-8 sorgusu kalmalı ve T353'ün KK2 testi (özellikle aynı sepetin yeni onayının sayılmaması) kalan yolla geçmeli.

#### Düzeltme 1 (main birleştirmesi)
`origin/main` (`9d0d611c`) birleştirildi (rebase yok). L-8'in tek evi main'dir: T353'ün `OrderConfirmationRefusal.TransferCeilingReached`, `OrderConfirmation.IsTransferCeilingReachedAsync` ve `OpenUnpaidOrderLimit`'in `excluding` aşırı yüklemeleri kaldırıldı; onay main'deki gibi `OrderPlacementLimits.FindExceededAsync`'i sorar. T353'e özgü davranış (aynı sepetin onayda iptal edilen önceki siparişi sayılmaz) main'de yoktu — main'in yorumu sayılmadığını söylüyordu ama iptal henüz kaydedilmediği için veritabanı onu Pending tutuyordu; KK2 main yoluyla kırmızıydı. En küçük ekleme: `OpenUnpaidOrderLimit.Open()` bağlamın değiştirdiği ve ödemesi artık Pending olmayan `Order` girdilerini sayımdan düşer (imza değişmedi; `AfterSalesModule`'deki kaydedilmemiş satır okumasıyla aynı kalıp). Main'in testleri değişmedi. KK2'nin ret beklentisi `AttemptLimitExceeded` + `ExceededLimit = L8` oldu. Kabul kriterleri 1–7 birleşik ağaçta yeniden denetlendi (testler yerinde, aşağıdaki sınama). D-853 metni kalan tek işleve (`OrderPlacementLimits.FindExceededAsync`) güncellendi.

Sınama (birleşik ağaç): derleme `-warnaserror` 0/0; `dotnet format --verify-no-changes` üç proje temiz; Unit 5550/5550 (Architecture + Consistency 1458 dahil); Integration Ordering 468/468, Payment 14/14, T377 limit sınıfları (`MembershipSignInAxesTests`, `AttemptCounterTests`) 16/16, `FirmSettingsTests` + `SalesGateTests` 51/51 (postgres:18, `track_commit_timestamp=on`).

#### Yapım Raporu Karşılaştırması
Tam uyumlu; öz denetim tablosu var ve her madde kanıtlı. Farklar: K1 ve K2 yapım raporunda yok; Integration'ı ben dokunulan sınıf ve ad alanlarıyla koştum (yapım TAM 3211). ⚠ kararları (D-566 ve D-668 → T511, D-833 → T536) yerinde: sipariş sayfası ve ayar ekranı o task'ların kapsamında.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (havale sütunları ve `PaymentMethodsVersion` önceden vardı)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T353-havale-eft`
- **Commit:** dal head'i PR'da
- **PR:** #500
- **CI:** run 42616258907 (head `9204f00d`) success
- **Post-merge run'lar:** squash `c5cd790` (#500) — CI 42640152318 success, Publish 42640152244 success (F3 gate'te dolduruldu)

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
