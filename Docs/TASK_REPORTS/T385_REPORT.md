# T385 — Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti: üç deneme, işaretin düşüşü, panelden yeniden gönderim, işaretsiz bildirimler

**Faz:** F3 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-10

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Gönderici** (`Notifications/DispatchSender`): kuyruktaki ("queued") satırlardan zamanı gelenleri seçer (ilk deneme ya da `NextAttemptAt`); B-1…B-16, F-1…F-6 ve hesap e-postalarını gönderir. Alıcı başına sınır (`RecipientLimitCheck`) gövdeden ve servisten önce sorulur; `Refused` çağrısız ve yeniden denemesiz "ulaşmadı" olur. Gövde yapılamazsa (sipariş yok, değer okunamıyor, bağlam kurulamıyor) çağrısız alıcı hatası yazılır ve satır yeniden deneme yoluna girer. Bağlantısı geçersizleşmiş hesap e-postası çağrısız kapanır (D-233).
- **Zamanlayıcı kaynağı** (`DispatchRetrySource`): satırın kendi zamanı; takvim ve iş satırı yok. Kayıtlı ağ geçidi yoksa satırlar beklemede kalır (ağ geçidini yalnız `DispatchAttempts` adlandırır; `DispatchAttempts.FromServices`).
- **Yeniden deneme kuralı** (`DispatchRetrySchedule`, T204'ten): ilk denemeden 5 dk, 30 dk, 2 saat; geç gelen deneme sonrakileri kaydırmaz; üçüncüden sonra "ulaşmadı".
- **Gövde bağlamı** (`OrderNoticeContexts`, `OrderNoticeFactsReader`, `IOrderNoticeFacts`; D-232, D-293): sipariş numarası, erişim anahtarı, misafir/silinmiş hesap, donan IBAN, dijital kalem, B-1'in iki donan metni, B-9/B-10 il adları, firma e-postası ve telefonu, yeniden gönderim cümlesinin tarihi.
- **İşaret** (`DispatchMarks`): grubun (ilk satır + `OriginalDispatchId` taşıyanlar) en son satırından okunur; saklanmaz, işlem izine yazılmaz; sınır reddinde ve servisin sonradan bildirdiği kalıcı geri dönmede de düşer; ayıp/cayma/fesih talebinin satırında B-9 işareti görünür; işaretli siparişler sayfa sayfa listelenir (`FindMarkedOrderIdsAsync`); çağrısız kapanan satır işaret düşürmez.
- **Yeniden gönderim** (`DispatchResending`, `CustomerNoticeQueue.QueueResending`): işaretli grubun son satırı için gruba yeni "queued" satır; alıcı siparişin güncel iletişim e-postası, B-15 düzeltmeden önceki adres; ilk e-postanın anı olay değerlerinde; izine `ResendEmail` (sipariş numarasıyla, kişisel veri yok); olağan kuyruktan ve üç denemeyle gider, başarıda işaret kalkar. `NoticeResendResult` Contracts'ta enum değil değer türüdür (enum, saklanan değer sayılır).
- **Panel işlemi**: `OrderOperations.EmailNotDelivered` artık `OrderHasUndeliveredMarkAsync`'ten okunur; `ResendEmail` işaret varken açık, yokken kapalı.
- Bu turda (devam 2): test sınıfındaki derleme hatası ve eksik firma satırı giderildi; mimari/tarama testlerinin dört bulgusu (enum taraması, bildirim parametresi taraması, zamanlayıcı kaynağında yeniden deneme anma, ağ geçidi ve satır yazımının kuyruk dışında olması) kaynak düzeltmesiyle kapandı; üç test düzeneğine `DispatchResending` kaydı eklendi; kalan birim ve tümleşik testler yazıldı; kayıtlar (backlog, STATUS, parça dosyaları) tamamlandı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Notifications/{DispatchSender,DispatchRetrySource,DispatchAttempts,DispatchMarks,DispatchResending,OrderNoticeContexts,CustomerNoticeQueue,CustomerNoticeValues,NotificationsModule}.cs`, `Notifications/Contracts/{DispatchResending,INotificationsModule}.cs`
- `src/Shopfolio.Web/Modules/Ordering/{OrderNoticeFactsReader,OrderOperations,OrderingModule}.cs`, `Ordering/Contracts/IOrderNoticeFacts.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Outages/DispatchRetryAndMarkTests.cs` (yeni, 31 test), `Ordering/OrderOperationsTests.cs`, `Ordering/PaymentNotificationHost.cs`, `Ordering/OrderingTestServices.cs`, `Model/FirmSettingsTests.cs`, dört sahte `INotificationsModule`; `tests/Shopfolio.UnitTests/Notifications/DispatchMarksTests.cs`, `Outages/DispatchRetryScheduleTests.cs`
- `Docs/IMPLEMENTATION_STATUS.md` (T385 satırı), `Docs/STATUS_CHANGELOG.d/T385.md`, `Docs/DEFERRED_BACKLOG.d/T385.md`, `Docs/DEFERRED_BACKLOG.md` (D-97, D-99, D-110, D-166, D-232, D-233, D-293 yerinde), `Docs/DEFERRED_BACKLOG.d/T320.md` (D-526 yerinde)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç yeniden deneme 5 dk / 30 dk / 2 saat, birikmez | ✓ | `AFailingRow_IsTriedAtTheFirstAttempt_ThenFiveMinutes…`, `ALateRetry_DoesNotMoveTheNextOnes…`; birim: `DispatchRetryScheduleTests`; sabitler `OutageConsistencyTests.DispatchRetryConstants_AreTheP38Row` |
| 2 | Üçüncü hata işareti düşürür; B-9'da talep satırında da | ✓ | `TheMark_FallsWithTheThirdRetrysFailure_ButNotBefore…`, `AClaimsB9_ShowsTheMarkOnTheClaimToo…` |
| 3 | İşaret izde yok, akışı etkilemez | ✓ | aynı test: iz satırı sayısı ve sipariş parmak izi değişmez |
| 4 | Sınır reddinde ve kalıcı geri dönmede işaret | ✓ | `ARowRefusedByTheRecipientLimit_IsMarkedWithoutAnyRetry…`, `ASentRow_TheServiceLaterReportsAsAPermanentBounce…` |
| 5 | Hesap e-postalarından yalnız davet; firma bildirimi işaret düşürmez; işaretli sipariş satırı süzülmüş listeye götürür | ✓ | `AFirmNoticeAndAnAccountEmailThatIsNotTheInvitation_PutNoMark…`, `AFirmNotice_IsSentByTheSameSender_AndItsFailureNeverMarksTheOrder`, `TheMarkedOrdersList_HoldsOnlyTheMarkedOrders_InPagesThatContinue`; firma uyarısı T382 `PanelWarningReader` (satır tarafı D-968) |
| 6 | Firma işaretli siparişten yeniden gönderir; B-1…B-16 | ✓ | `EveryCustomerNotice_IsMadeFromTheOrder_SentAgainThroughTheQueue…` (16 kuram), `OrderOperationsTests.TheResendEmailOperation_IsOpenOnlyWhile…`; düğme ve eşzamanlılık T531 (D-967) |
| 7 | Güncel e-posta; B-15 eski adres | ✓ | aynı kuram: `expectedRecipient` B15 için eski adres |
| 8 | Yeniden gönderim izde | ✓ | aynı kuram: `ResendEmail`/`Order`, ayrıntıda `@` yok |
| 9 | Olağan kuyruk ve yeniden deneme; işaret son satırdan, başarıda kalkar | ✓ | aynı kuram; `AResendingThatFailsThroughItsRetries_MarksAgain…`; `DispatchMarksTests` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 üç an ilk denemeden, sahte saatle | ✓ | `DispatchRetryAndMarkTests.AFailingRow_IsTriedAtTheFirstAttempt_…` |
| KK2 işaret üçüncü hatayla; B-9'da talep satırında | ✓ | `TheMark_FallsWithTheThirdRetrysFailure_…`, `AClaimsB9_ShowsTheMarkOnTheClaimToo_…` |
| KK3 izde işaret yok, akış sürer | ✓ | `TheMark_FallsWithTheThirdRetrysFailure_…` (iz sayısı ve `OrderFingerprintAsync`) |
| KK4 sınır ve kalıcı geri dönme | ✓ | `ARowRefusedByTheRecipientLimit_…`, `ASentRow_TheServiceLaterReportsAsAPermanentBounce_…` |
| KK5 firma bildirimi işaret düşürmez; işaretli sipariş listesi | ✓ | `AFirmNoticeAndAnAccountEmail…`, `TheMarkedOrdersList_…` |
| KK6 B-1…B-16 yeniden gönderilir | ✓ | `EveryCustomerNotice_…` (Theory B1…B16, 16 satır) |
| KK7 B-15 eski adrese | ✓ | aynı Theory, B15 |
| KK8 yeniden gönderim izde | ✓ | aynı Theory, iz satırı |
| KK9 kuyruktan geçer, başarıda işaret kalkar | ✓ | aynı Theory (`SendAsync` sonrası `HasMarkAsync` yanlış) |
| `09 §25` ilgili satırlar | ✓ | tarama/mimari testleri: `NotificationChannelSourceScanTests`, `ExpiryTimerScanTests`, `NoticeParameterSourceScanTests`, `EnumSourceScanTests`, `EnumLockTests`, `EnumTableConsistencyTests`, `ModuleBoundary*` yeşil (Unit 5773/5773) |
| Güvenlik taraması | ✓ | iz satırı kişisel veri taşımaz (`ResendEmail` ayrıntısında yalnız sipariş numarası); olay değerlerinde erişim anahtarı yok; gövde gönderimde kurulur, saklanmaz; yeni uç nokta yok |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | yeni istisna yok; `QueueFiles`/`GatewayFiles` listeleri değişmedi, kaynak listelere uydu |
| Sık FAIL (2) tarama çağrı biçimi kaçırıyor | ✓ | yeni tarama yazılmadı |
| Sık FAIL (3) DB default'u, FK dizini | ✓ (yok) | şema/göç değişmedi |
| Sık FAIL (4) kural değişince aynı sayıyı anan yerler | ✓ | `NoticeResendResult` enum olmaktan çıktı, `06`'da yeni enum yok; backlog'da `D-97/D-99/D-110/D-166/D-232/D-233/D-293/D-526` yerinde güncellendi |
| Sık FAIL (5) belge kuralını değiştiren 09 metni karar kaydısız | ✓ (yok) | 09 ve karar kaydı değişmedi; `LinkNotValid` durum notu D-969 olarak faz kapısına yazıldı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build` | 5773 / 5773 |
| Integration | ✓ | ad alanı başına `--filter-namespace` (Ordering 600; Outages+Model 1453; Jobs+CorporateContent+Administration+Concurrency+Disposal 418; Payment+Security+Catalog+Pages+Web+Commands 794; Database+FileSecrets+Files+Logging+SignedEndpoints 203) | hepsi yeşil; sonradan eklenen testler dahil `DispatchRetryAndMarkTests` 31/31, `OrderOperationsTests` 9/9 |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes` (Web, Unit, Integration) | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-10, head `3de1e988`) |
| Bulgu sayısı | 1 (B1 DÜŞÜK S1) |
| Düzeltme gerekli mi | Evet — B1 |
| Bloke-etmeyen not (K) sayısı | 3 (K1 → D-974; K2, K3 → D-975) |

**Validator kanıtları (tur 1):** kriter 1 ✓ (`AFailingRow_…`, `ALateRetry_…`; 5/30/120 dk ilk denemeden, sahte saat), 3 ✓ (iz sayısı ve sipariş parmak izi değişmez), 4 ✓ (sınır reddi `AttemptCount = 1`, `RecipientError` yazar → işaret; geri dönme `08 §2.2.3`'ün sonucu ile sınandı — sorgu işi ayrı task'ta), 5 ✓ (F satırları `OrderId` taşımaz; yalnız davet `AccountEmailHasMarkAsync`), 6–9 ✓ (16 türlü kuram: gruba yeni `Queued` satır, B-15 eski adres, öteki güncel adres, iz `ResendEmail`/`Order` yalnız sipariş numarasıyla, olağan gönderici ve başarıda işaret yok; sınır muafiyeti `OriginalDispatchId`). **Kriter 2 ~ Kısmi (B1):** `DispatchMarks.ClaimHasMarkAsync` ve `INotificationsModule.ClaimHasUndeliveredMarkAsync` B-9 işaretini `Withdrawal` ve `LateDeliveryTermination` satırında da verir; `02 §9.1.6`, `03 §1.7.3.1`, `06 §4.58` ("Ayıp talebinin satırı": `DefectClaim`) ve `04` 2.5.3.1 yalnız ayıp talebini sayar (`09 §25`: ek davranış yok, belge cümlesiyle çelişki yok). `AClaimsB9_ShowsTheMarkOnTheClaimToo_…` cayma ile kurulu; ayıp talebinin talep satırı sınanmıyor. Düzeltme: tür denetimi yalnız `DefectClaim`, testte ayıp talebi ✓ ve cayma/fesih ✗. **Uyuşmazlık:** yapım raporu "ayıp/cayma/fesih talebinin satırında" diyerek genişlemeyi yazıyor ve KK2'yi ✓ sayıyor. Yerel: build 0 uyarı, format temiz, Unit 5789/5789, betik 4/4, Integration 663 + 1589 yeşil.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T385-gonderim-yeniden-deneme`
- **PR:** yapım sonrası açıldı (no dönüş mesajında)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok. Gerçek e-posta servisi ağ geçidi T407'dir; gönderici kayıtlı ağ geçidi yokken satırı beklemede tutar | `DispatchRetrySource`, `DispatchAttempts.FromServices` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | E-posta başlığında logo yolu boş | D-966 | marka görselinin e-postaya bağlanması D-442'dedir |
| 2 | Panelden yeniden gönder düğmesi, koşul ve eşzamanlı istek | D-967 | T531 (`05 §4.4.3`; K-1084) |
| 3 | Ana sayfa "işaretli siparişler" satırı ve süzülmüş liste ekranı | D-968 | panel sayfası task'ları; veri hazır |
| 4 | `LinkNotValid` kapanışının `06 §4.58` notu | D-969 | faz kapısı; karar kaydı gerekebilir ⚠ |

## Notlar

- **⚠ Firma iletişimi:** B-6, B-7, B-10, B-14, B-15 gövdelerindeki firma e-postası ve telefonu ayarların güncel hukuki kimliğinden okunur. `02`, `03`, `06` ve `08` siparişte donan firma e-postası ile telefonunun bildirim metnine yazılacağını söylemez (`08 §2.2.6` m.6: `Company.Email`, `Company.Phone`; alt bilgi ve yanıt adresi de güncel ayardır); `Order.CompanyEmail` yalnız donan belge verisidir. Kolay karar: güncel ayar.
- **⚠ Çift yönlü Contracts bağımlılığı:** Ordering → Notifications.Contracts (`OrderOperations`) ve Notifications → Ordering.Contracts (`IOrderNoticeFacts`, `IOrderDocuments`). `09 §6` yalnız "Contracts'tan başvur" der, döngü yasağı yoktur; mimari testler yeşil; DI döngüsü yok.
- **⚠ Yeniden gönderim gövdesi** gönderim anındaki siparişten kurulur (B-2'nin IBAN'ı donan sipariş değerinden; B-1'in metinleri donan sürümlerden).
- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** yönetici işi (devam talimatı).
