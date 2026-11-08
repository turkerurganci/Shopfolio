# T311 — Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS; merge yardımcıda) | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Uç:** `POST /sistem/odeme-bildirimi` imzalı makine ucu olarak eşlendi (`PaymentModuleRegistration.MapPaymentNotificationEndpoint`, `Program.cs`). Yol `AntiforgeryExemptions.PaymentNotificationPath`'ten gelir; gövde tavanı, 400, işlem, işlemden sonra 200 `OK` ve 500 T203'ün imzalı uç hattındadır (`SignedEndpointHandler`).
- **Bağlantı katmanı (KK1):** `IPaymentGateway`'e iki yerel işlev eklendi: `ReadNotificationResult(eventType, outcome)` → `NotificationResult {PaymentSucceeded, PaymentFailed, Unrecognized}` ve `CheckNotificationAmount(notice, orderTotal)` → `NotificationAmountCheck {Holds, Mismatch}`. Tutarın "tuttu" biçimi sağlayıcınındır, katmanda kalır (`08 §2.1.5`, `05 §3.6.1`). `PaymentNotice` alan eşlemesinin okuduğu alanları taşır: işlem kimliği (olduğu gibi), geri eşlenen sipariş numarası, olay türü, sonuç, tahsil edilen ve başlatılan tutar (okunamazsa boş), kart / lira / test işaretleri.
- **Sınıflama:** `PaymentNotificationRule` `08 §2.1.5` tablosunun 3–7. satırlarını sırayla uygular (1–2: hat ve tekil dizin). Satır 4'e `05 §4.5.5`'in "gövdesi çözümlenemeyen" kolu (tutar okunamadı) ve kart dışı yöntemli sipariş eklendi.
- **Kayıt:** `PaymentNotificationEndpoint.RecordAsync` kaydı `InboundNotificationInsert.TryInsertAsync` ile `INSERT … ON CONFLICT DO NOTHING` olarak yazar (tekrar — yarışan iki kopya dahil — hiçbir şey yazmaz, işlem bozulmaz). Normal bildirime aynı işlemde iş satırı (`JobType.InboundNotificationProcessing`) açılır; işaretli bildirim `ProcessedAt` + `UnprocessedReason` ile yazılır, iş satırı açılmaz, `InboundNotificationUnprocessed` satırı yazılır (sebep + kaydın iç kimliği; referans ve tutar yok).
- **İşleme:** `InboundNotificationJobHandler` (`ITransactionalJobHandler`) başarıyı `IOrderTransitions.ApplyCardPaymentAsync`'e (son sorgunun da çağırdığı tek yol), başarısızlığı `CancelOrderAsync(BySystem(CardPaymentFailed))`'e verir; `ProcessedAt`'i yazar; geri gelen iş satırı yazılmış anı bulur ve bir şey yapmaz.
- **Ordering sözleşmesi:** `IOrderingModule.FindOrderForPaymentAsync(orderNumber)` → `OrderForPayment(OrderId, Total, PaymentMethod)` (izlemesiz, projeksiyonlu okuma).
- **Kayıt olayı:** `LogEvents.InboundNotificationUnprocessed` ve `09 §18.2` m.3 kapalı listesi (kural "yeni olay adı bu listeye eklenir" der; kural değişmedi).
- **Tarama:** kayıt deyimi tek deyimlik ayrı dosyada; `VersionedSideBulkWriteScan.AllowedPlaces`'e tek dosya olarak girdi (`InboundNotificationInsertPlace`). Test dosyanın tek `Execute*` çağrısı ve tek yazma deyimi taşıdığını, deyimin yalnız `inbound_notification_records`'a `ON CONFLICT DO NOTHING` olduğunu, uç / iş / kural dosyalarının yer olmadığını ve bulgu vermediğini sınar.
- **Devam işçisi:** önceki işçinin derlenmeyen testleri düzeltildi (eksik using'ler, geçiş kayıtlarının sıra karşılaştırması). Uygulamanın ucu artık aynı yola eşlendiği için `RequestLimitsTests`'in kendi test ucu kaldırıldı; iki test uygulamanın ucunu sahte bağlantı katmanıyla sınar. `origin/main` (T336) birleştirildi, çakışma yok.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/IPaymentGateway.cs`, `Infrastructure/Logging/LogEvents.cs`
- `src/Shopfolio.Web/Modules/Ordering/Contracts/IOrderingModule.cs`, `Modules/Ordering/OrderingModule.cs`
- `src/Shopfolio.Web/Modules/Payment/`: `PaymentNotificationEndpoint.cs`, `PaymentNotificationRule.cs`, `InboundNotificationInsert.cs`, `InboundNotificationJobHandler.cs`, `PaymentModule.cs`
- `src/Shopfolio.Web/Program.cs`
- `Docs/09_CODING_GUIDELINES.md` §18.2 m.3 (olay adı)
- Testler: `tests/Shopfolio.UnitTests/Payment/PaymentNotificationRuleTests.cs`, `Fakes/FakePaymentGateway.cs`, `Architecture/PaymentGatewayImplementationTests.cs`, `Architecture/VersionedSideBulkWriteScan.cs` + `…Tests.cs`; `tests/Shopfolio.IntegrationTests/Payment/PaymentNotificationTests.cs`, `Web/RequestLimitsTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Uç `POST /sistem/odeme-bildirimi`; imza ve tutar bağlantı katmanında | ✓ | `PaymentModule.cs:221` (`MapSignedEndpoint` + `AntiforgeryExemptions.PaymentNotificationPath`), `Program.cs:191`; `PaymentNotificationEndpoint.Verify` → `IPaymentGateway.VerifyNotification`; `PaymentNotificationRule.Classify` → `CheckNotificationAmount`; `PaymentNotificationRuleTests.Verify_AsksTheLinkLayer_…_KK1`, `SuccessWhoseAmountDoesNotHold_IsMarkedAmountMismatch_Row6_KK5` |
| 2 | Bozuk imza 400, kayıt yok, işleme yok, seyreltilmiş güvenlik olayı | ✓ | `PaymentNotificationTests.SignatureThatDoesNotHold_Gets400WithAnEmptyBody_NoRecordNoJob_AndAThinnedSecurityLine_KK1_KK2`; hız sınırı T215: `RequestLimitsTests.ABadSignature_IsAnsweredByTheEndpointEvenWhenTheLimitIsReached`, `ANotificationWithARightSignature_IsOutsideTheLimit_…` |
| 3 | Doğru imzalı bildirim kayıt diske kesinleştikten sonra 200 `OK` | ✓ | `Success_IsRecordedWithItsJobRow_ReceivedAfterTheCommit_AndTheOrderChangesOnlyInTheJob_KK3_KK6_KK7` (commit anında dışarıdan 0 satır; yanıt `OK`); yanıt sırası `SignedEndpointHandler` (T203) |
| 4 | Tekrar kaydedilmez, işlenmez, yine 200 | ✓ | `SameNotificationAgain_IsReceivedWithTheSameAnswer_AndWritesNoSecondRecordOrJobRow_KK4`, `SameTransactionWithAnotherOutcome_IsAnotherNotification_KK4`, `JobRowThatComesBack_FindsTheRecordProcessed_AndAppliesNothingAgain_KK4_KK7`; `InboundNotificationInsert.cs` `ON CONFLICT DO NOTHING` |
| 5 | Tutarı / siparişi tutmayan, türü tanınmayan, gövdesi çözümlenemeyen: işaretli, işlenmez, 200 | ✓ | `AmountMismatch_IsRecordedMarked_WithoutAJobRow_AndReceived_KK5`, `ReferenceOfNoOrder_IsRecordedMarkedOrderNotFound_Unlinked_AndReceived_KK5`, `UnrecognizedOutcomeOrUnreadableBody_IsRecordedMarkedUnrecognized_AndReceived_KK5`; birim: `PaymentNotificationRuleTests` satır 3–7 testleri |
| 6 | Kayıt ve iş satırı aynı işlemde; işleme kuyruktan | ✓ | `PaymentNotificationEndpoint.cs:87-99` (aynı `DbContext`, hattın işlemi); `RecordThatCannotBeCommitted_Gets500WithAnEmptyBody_AndNeitherTheRecordNorTheJobRowStays_KK6_KK9`; `Success_…_KK3_KK6_KK7` (iş satırı 1, sipariş değişmedi) |
| 7 | Tek yoldan aktarım; uç siparişi yazmaz | ✓ | `InboundNotificationJobHandler.cs:60,64` (`ApplyCardPaymentAsync`, `CancelOrderAsync`); uçta `IOrderTransitions` yok; `Success_…_KK7` (yanıttan sonra sipariş Placed/Pending, iş koşunca Preparing/Paid); `TheInboundNotificationInsertPlace_…_AndTheEndpointHoldsNone` |
| 8 | Gövde tavanı 64 KB (413); sahtecilik korumasının dışında | ✓ | `BodyAboveTheCeiling_Gets413_WithoutAskingTheLinkLayer_KK8`, `TokenlessRequest_IsNotRefusedByTheForgeryCheck_KK8`; `ForgeryProtectionTests` (koruma dışı liste genişlemedi); Kestrel tavanı T203'ün `SignedEndpointHandler.cs:77-102`'si |
| 9 | Kaydedilemeyen bildirim 500, hata gövdeleri boş | ✓ | `RecordThatCannotBeCommitted_Gets500WithAnEmptyBody_…_KK6_KK9`; 400 gövdesi boş: `SignatureThatDoesNotHold_…_KK1_KK2` |
| 10 | İptal edilmiş siparişe ödeme aynı tek yoldan; geri ödeme ve F-4 T317'de | ✓ | `PaymentToACancelledOrder_GoesTheOnePath_IsRecorded_AndIsPaidBack_KK10` (ödeme kaydı, `LatePayment` geri ödemesi, `CardRefund` iş satırı) |

### Öz denetim (bitiş kapısı 2)

Bütün maddeler — önceki işçinin yaptıkları dahil — bu diff'te yeniden denetlendi (`11` satır 1952–1977).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: imza ve tutar katmanda; bozuk imza düşer, tutar uyuşmazlığı sınıf olarak döner | ✓ | `IPaymentGateway.CheckNotificationAmount` → `NotificationAmountCheck`; `PaymentNotificationRuleTests.SuccessWhoseAmountDoesNotHold_…`, `SuccessWithTheInstallmentDifference_Holds_KK4OfTheLayer`, `Verify_WithoutAPaymentGateway_RefusesEveryRequest` |
| KK2: 400'de satır yok, olay seyreltilmiş | ✓ | `SignatureThatDoesNotHold_…_KK1_KK2` (kayıt 0, iş 0, `SignatureFailureSampled` satırı) |
| KK3: yanıt kayıt diske inmeden dönmez | ✓ | `Success_…_KK3_KK6_KK7` (`CommitStep.BeforeCommit` anında dış bağlantı 0 satır görür) |
| KK4: ikinci gönderimde tek satır, yanıt aynı | ✓ | `SameNotificationAgain_…_KK4` (1 kayıt, 1 iş, iki yanıt aynı `OK`) |
| KK5: tutar uyuşmazlığı işaretli kayıt ve 200 | ✓ | `AmountMismatch_IsRecordedMarked_WithoutAJobRow_AndReceived_KK5` (kayıt satırında referans yok) |
| KK6: kayıt ve iş satırı birlikte geri alınır | ✓ | `RecordThatCannotBeCommitted_…_KK6_KK9` (commit hatası → kayıt 0, iş 0) |
| KK7: sipariş yalnız kuyruk işinde değişir | ✓ | `Success_…_KK7`, `Failure_CancelsTheUnpaidOrderInTheJob_S3WithO2AndTheCardFailureReason`, `FailureOnAnOrderThatIsPaidAlready_ChangesNothing_AndTheRecordIsProcessed` |
| KK8: 64 KB üstü 413; sahtecilik denetimi yok | ✓ | `BodyAboveTheCeiling_…_KK8`, `TokenlessRequest_…_KK8` |
| KK9: veritabanı hatasında 500, gövde boş | ✓ | `RecordThatCannotBeCommitted_…_KK6_KK9` |
| KK10: iptal edilmiş siparişin ödeme bildirimi kuyruk işinde T317'nin geri ödeme yoluna ulaşır | ✓ | `PaymentToACancelledOrder_…_KK10` |
| `09 §25` — doküman uyumu | ✓ | Yeni adlar `06 §4.35`, §5.6.6 ve `07 UC-80`'den; `06 §4.35` `ProcessedAt` açıklamasıyla dar sapma ⚠ 1 / D-468 |
| `09 §25` — modül sınırı | ✓ | Payment yalnız `Ordering.Contracts`'a başvurur; arayüz metodunda `SaveChanges` yok; mimari testler yeşil |
| `09 §25` — dış servis `Integrations` arayüzünde, imza ve tutar işleyiciden önce | ✓ | `Verify` hattın kayıt adımından önce; tutar `RecordAsync`'te katmanın işleviyle |
| `09 §25` — durum değişikliği yalnız geçiş birimiyle | ✓ | İş `IOrderTransitions` çağırır; uç yazmaz |
| `09 §25` — iş kalıbı (iş türü kayıtlı, işleyici sahip task'ta) | ✓ | `PaymentModule.cs:207`; `JobQueueRegistrationTests` yeşil |
| `09 §25` — eşzamanlılık | ✓ | Tekillik dizini + `ON CONFLICT`; siparişin kilidi geçiş biriminde |
| `09 §25` — zaman `TimeProvider`'dan | ✓ | Uç ve iş `TimeProvider` alır |
| `09 §25` — kayıtta kişisel veri yok | ✓ | `AmountMismatch_…_KK5` log satırında referans yok; kayıt kişisel alan taşımaz |
| `09 §25` — sahtecilik koruması dışı liste genişlemedi | ✓ | `AntiforgeryExemptions.Paths` değişmedi; `ForgeryProtectionSourceScan` yeşil |
| `09 §25` — okuma izlemesiz ve projeksiyonlu | ✓ | `OrderingModule.FindOrderForPaymentAsync` (`AsNoTracking` + `Select`) |
| `09 §25` — test katmanı, sahteler `Fakes/`'ten | ✓ | Gerçek PostgreSQL 18; `FakePaymentGateway` |
| `09 §25` — yeni mimari kural / istisna ihlal örneğiyle | ✓ | Yeni kural yok; istisna tek dosya, `TheInboundNotificationInsertPlace_HoldsExactlyOneRawStatement_…` |
| Güvenlik taraması | ✓ | Sır yok; yetki imzayla; girdi yalnız katmanın doğruladığı alanlardan; yeni bağımlılık yok |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | İstisna yalnız `InboundNotificationInsert.cs`; test tek `Execute*` çağrısını, tek yazma deyimini ve tabloyu sınar |
| Sık FAIL 2: kaynak taraması çağrı biçimini kaçırıyor | ✓ | Yeni tarama yok; var olan taramalar (`VersionedSideBulkWriteScan`, `ForgeryProtectionSourceScan`, `IntegrationSourceScan`, `LogTemplateSourceScan`, `JobSourceScan`) yeşil |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ | Şema değişmedi, göç yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `LogEvents` ↔ `09 §18.2` m.3 eşit (`LoggingConsistencyTests`); `AllowedPlaces` özeti güncellendi; backlog sayacı yardımcının (⚠ 6) |
| Sık FAIL 5: belge kuralını değiştiren 09 metni | ✓ | `09 §18.2` m.3'e yalnız olay adı eklendi (kuralın kendi talimatı) |
| Sık FAIL: modül metodu SaveChanges çağırıyor | ✓ | Uç, iş ve `FindOrderForPaymentAsync` `SaveChanges` çağırmaz; kaydı hat / çalıştırıcı yapar |
| Sık FAIL: main ilerledi | ✓ | `origin/main` (T336) birleştirildi; birleşik ağaçta Unit 4393, dokunulan Integration sınıfları yeşil |
| `12` satırları: VAL-A545, A586, A595, A706, A707, A822, A824, D082 | ✓ | Yukarıdaki Integration testleri; D082 için `UnknownOutcomeOrEventType_IsMarkedUnrecognized_Row4` ("refund") |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Lint | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | temiz |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --no-build` (birleşik ağaç) | 4393/4393 (Consistency 242 dahil) |
| Betik | ✓ | `sh tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration | ✓ | parça parça (`--filter-namespace` / `--filter-class`) | `PaymentNotificationTests` 14/14; Payment + Ordering + Jobs 249; Web (31 sınıf) 605; Disposal, Concurrency, SignedEndpoints, Outages, Logging, Security, Catalog 217; Files, CorporateContent, FileSecrets, Database 205; Model 1191 — hepsi yeşil (birleştirmeden önce); birleştirmeden sonra Payment + Ordering + Jobs 249, RequestLimits + ForgeryProtection + SignedEndpointDatabase + Liveness 81 yeşil |
| Contract | — | `NOT_APPLICABLE` | — |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-08, head `9bb8e06` + `origin/main` ea62688c birleştirmesi, çakışmasız) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-480; K2 D-97'de kapandı) |

Doğrulamadan sonra `origin/main` (T324 zaman aşımı #454, T344 adres #451) birleştirildi: `OrderingModule.cs` ve `PaymentModule.cs`'te iki tarafın eklemeleri yan yana korundu (yalnız ekleme), backlog D-97'ye T324 notu eklendi ve tablo sonu numara sırasıyla dizildi; öz denetim tablosundaki `PaymentModule.cs:217` atfı `:221` oldu.

### Doğrulayıcı kanıtları (tur 1)

#### Kabul Kriterleri

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Uç `POST /sistem/odeme-bildirimi`; imza ve tutar bağlantı katmanında | ✓ | `PaymentModuleRegistration.MapPaymentNotificationEndpoint` → `MapSignedEndpoint` (`Program.cs`); `Verify` → `IPaymentGateway.VerifyNotification`; tutar `IPaymentGateway.CheckNotificationAmount` (`PaymentNotificationRule.Classify`); `PaymentNotificationRuleTests` (Row6/Row7, Verify_…_KK1) yeşil |
| 2 | Bozuk imza 400, kaydedilmez, işlenmez, seyreltilmiş olay; hız sınırı T215 | ✓ | `SignatureThatDoesNotHold_…_KK1_KK2` (400, gövde boş, kayıt 0, iş 0, `SignatureFailureSampled`); `RequestLimitsTests.ANotificationWithARightSignature_IsOutsideTheLimit_…` yeşil |
| 3 | Kayıt diske kesinleştikten sonra 200 `OK` | ✓ | `Success_…_KK3_KK6_KK7` (commit anında dış bağlantı 0 satır; yanıt `OK`); `SignedEndpointHandler` sırası |
| 4 | Tekrar kaydedilmez, işlenmez, 200 | ✓ | `SameNotificationAgain_…_KK4` (1 kayıt, 1 iş, iki `OK`); `InboundNotificationInsert` `ON CONFLICT DO NOTHING` + filtreli tekil dizin (`UniqueNotificationWithoutId`) |
| 5 | Tutarı/siparişi tutmayan, türü tanınmayan, gövdesi çözümlenemeyen: işaretli, işlenmez, 200 | ✓ | `AmountMismatch_…_KK5`, `ReferenceOfNoOrder_…_KK5`, `UnrecognizedOutcomeOrUnreadableBody_…_KK5`; sınıflama sırası `08 §2.1.5` tablosu 3–7 ile birebir |
| 6 | Kayıt ve iş satırı aynı işlemde; işleme kuyruktan | ✓ | `RecordAsync` hattın işleminde yazar; `RecordThatCannotBeCommitted_…_KK6_KK9` (ikisi de 0) |
| 7 | Tek yoldan aktarım; uç siparişi yazmaz | ✓ | `InboundNotificationJobHandler` → `ApplyCardPaymentAsync` / `CancelOrderAsync(BySystem(CardPaymentFailed))`; uçta `IOrderTransitions` yok; `Success_…_KK7`, `Failure_CancelsTheUnpaidOrderInTheJob_…`, `FailureOnAnOrderThatIsPaidAlready_…` (sistem iptali yalnız ödenmemiş Placed'de — `CancellationConditionAsync`) |
| 8 | 64 KB tavan 413; sahtecilik korumasının dışında | ✓ | `BodyAboveTheCeiling_…_KK8`, `TokenlessRequest_…_KK8` (`RequiresValidation` false) |
| 9 | Kaydedilemeyen 500, hata gövdeleri boş | ✓ | `RecordThatCannotBeCommitted_…_KK6_KK9` (500, gövde boş) |
| 10 | İptal edilmiş siparişe ödeme aynı tek yoldan; geri ödeme T317 | ✓ | `PaymentToACancelledOrder_…_KK10` (ödeme kaydı, `LatePayment` geri ödeme, `CardRefund` iş satırı) |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-A545 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-A586 | ORTA | mock | BEKLEMEDE | PASS | — |
| VAL-A595 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-A706 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-A707 | KRİTİK | mock | BEKLEMEDE | PASS | — |
| VAL-A821 | KRİTİK | mock | BEKLEMEDE | PASS (kayıt uç işleminde kesinleşir; işlemli işleyici hatası geri alınır ve varsayılan kuralla yeniden denenir — `JobRunnerTests.RunOne_ATransactionalHandlerThatFailsAfterItWrote_…`; yeniden gelen satır bir şey uygulamaz — `JobRowThatComesBack_…`) | — |
| VAL-A824 | ORTA | mock | BEKLEMEDE | PASS | — |
| VAL-D082 | KRİTİK | mock | BEKLEMEDE | PASS (başarısız + başarılı iki kayıt — `SameTransactionWithAnotherOutcome_…`; iade türü işaretli/işlenmez — `UnknownOutcomeOrEventType_…_Row4`) | — |
| VAL-A822 | KRİTİK | mock | BEKLEMEDE | BEKLEMEDE — ön koşul (bildirimle eşzamanlı son sorgu) T403'te; `12 §4.2` kural 9 | — |

KP-15'in öteki satırları (B008–B012, C001, C002, A053, D003, D004) ve D081, A584 sayfa/PayTR/son sorgu/kart iadesi işleyicisi ister; T311'in kriterleriyle eşlenmez, `BEKLEMEDE` kalır. `12_matris_denetle.py --kapsam 1-77` hata 0 (1503 satır).

#### Test Sonuçları

| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Lint | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 | temiz |
| Unit (+Consistency, Architecture) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 4393/4393 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya |
| Integration | ✓ | `--filter-class` PaymentNotificationTests, RequestLimitsTests, JobRunnerTests | 72/72 |
| Integration | ✓ | `--filter-namespace` SignedEndpoints, Ordering, Web, Security, Model | 2005/2005 |
| Dal CI | ✓ | `gh pr checks 456` (run 42450573628, head 9bb8e06) | Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate pass |

#### Güvenlik Kontrolü

- Secret sızıntısı: Temiz
- Auth etkisi: Temiz (imza tek güvence; koruma dışı liste `AntiforgeryExemptions` değişmedi; bozuk imza form sınırına sayılır)
- Input validation: Temiz (ham SQL parametreli `ExecuteSqlAsync`; alanlar katmanın doğruladıklarıyla sınırlı; uygulama kaydına yalnız sebep + iç kimlik)
- Yeni bağımlılık: Yok

#### Bloke-etmeyen Notlar (K-notları)

| # | Not | Karar |
|---|---|---|
| K1 | `IPaymentGateway`'e eklenen `ReadNotificationResult`/`NotificationResult` ve `CheckNotificationAmount`/`NotificationAmountCheck` `08 §3.1` tablosunda ve `09 §11.1` madde 1–2'de yok ("sınıf eklemek önce 08'e girer"; enum, kapalı kayıt ailesi değil). Davranış KK1 ve `08 §2.1.5` ile uyumlu | DEFERRED_BACKLOG'a forward: D-480 |
| K2 | D-97 T311'in `InboundNotificationProcessing` işleyicisini sayıyordu; işleyici kayıtlı | bu PR'da kapatıldı (D-97'de T311 ✅) |

#### Yapım Raporu Karşılaştırması

- Uyum: 2 uyuşmazlık, verdict'i etkilemez.
- Yapım raporu VAL-A822'yi ✓ sayar; doğrulayıcı `BEKLEMEDE` bırakır (son sorgu yolu T403'te, satırın ön koşulu kurulamaz). VAL-A821'i rapor anmaz; doğrulayıcı PASS verdi.
- Öz denetim tablosu var, maddeler kanıtlı. K1 raporda geçmiyor.
- Dalın `STATUS_CHANGELOG.md` girişi ve MEMORY "Güncel Durum" satırı chore #450 kuralına aykırıydı; birleştirmede main sürümü alındı, giriş parça dosyasına taşındı (`Docs/STATUS_CHANGELOG.d/T311.md`).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T311-odeme-bildirim-ucu`
- **Commit:** dalın son commit'i (PR head)
- **PR:** #456
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Gerçek sağlayıcı bağlantısı bu task'ta yok | `11` T401/T402 PayTR katmanı; `Program.cs`'te `IPaymentGateway` kaydı yok | Uç gateway kayıtsızken her isteğe 400 verir (⚠ 4) |
| — | Plan / kota / API anahtarı varsayımı | Tetikleyici tarama: kabul kriterlerinde yok | Dış varsayım: yok |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `06 §4.35` `ProcessedAt` metni işaretli bildirimin kayıt anında yazılmasını anmaz | D-468 | Doküman geçişi; kod `07`/`05`/`08` ile uyumlu |
| 2 | Ödeme başarısının sepetten kalem çıkarması (`07 UC-80` yan etkiler) | — | `11` T347 kriter 6'nın işi (`02 §3.16.12`); backlog gerekmez |
| 3 | `failed_reason_code` seyreltilmiş log | — | `11` T402 KK6 |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; dal `origin/task/T311-odeme-bildirim-ucu`).
- **Ana dal CI startup (Adım 0):** 42446700473 success, 42446700476 success, 42445081110 success (önceki işçi: 42443632733, 42443632759, 42436765868 success).
- **Repo memory (bitiş kapısı 9):** `.claude/memory/MEMORY.md` Güncel Durum'a T311 satırı eklendi.
- **⚠ kararları (kurala en uygun seçenekle):**
  1. İşaretli bildirim (`OrderNotFound` / `UnrecognizedNotification` / `AmountMismatch`) kayıt anında `ProcessedAt` + `UnprocessedReason` ile yazılır, iş satırı açılmaz: işaretin dayandığı imzasız alanlar (`payment_amount`, `payment_type`, `currency`, `test_mode`) kayda girmez (`08 §2.1.5`), iş satırı onları göremez. `06 §4.35` `ProcessedAt` metni "iş satırının yazdığı an" der — D-468.
  2. "Canlı kurulum" = `IHostEnvironment.IsProduction()` (`08 §2.1.10`; kesin ayar T406'da).
  3. Kart dışı yöntemli siparişe gelen bildirim `UnrecognizedNotification` (siparişe bağlanır).
  4. Gateway kayıtsızken uç her isteğe 400 verir (gerçek uygulama T401/T402); iş işleyicisi gateway'siz satırda istisna atar (satır yalnız doğrulanmış bildirimle doğar).
  5. İşaretli satırın uygulama kaydı satırı commit'ten önce yazılır (hat commit sonrası kanca vermiyor); commit düşerse satır yazılmış ama kayıt yok kalabilir — yanıt 500'dür, sağlayıcı yeniden gönderir.
  6. Backlog "Aktif kalem" sayacına ve `IMPLEMENTATION_STATUS` "Son güncelleme" satırına dokunulmadı (yardımcı kuralı); D-468 eklendi.
  7. `RequestLimitsTests` kendi test ucunu bildirim yoluna eşliyordu; uygulamanın ucu eşlenince yol çift oldu (500). Test ucu kaldırıldı, iki test uygulamanın ucunu `FakePaymentGateway` ile sınar (davranış beklentisi değişmedi).
  8. Kayıt deyimi `VersionedSideBulkWriteScan` kapalı yer listesine tek deyimlik dosya olarak girdi (`GalleryMainMark` örneği); gelen bildirim kaydı sipariş tarafı satırı değildir.
