# T403 — PayTR: son sorgu ve tek tahsilat

- **Dal:** `task/T403-paytr-son-sorgu` (taban `main`, `origin/main` `44f651aa` ile birleştirildi)
- **Bağımlılık:** T402 ✓
- **Tarih:** 2026-11-10
- **Yapım:** iki işçi (ilki bağlam sınırında bölündü, ara commit `b157ce6b`; ikincisi testleri, belgeleri ve kayıtları tamamladı)

## Yapılan İşler

- **Son sorgu isteği** (`PayTrPaymentQuery`, saf): `POST https://www.paytr.com/odeme/durum-sorgu`, form kodlu; alanlar `merchant_id`, `merchant_oid` (numaranın ayraçsız hâli), `paytr_token` = HMAC-SHA256(`merchant_id` + `merchant_oid` + `merchant_salt`, `merchant_key`) Base64. Süre iş sınırı 30 sn (`ExternalCallTimeouts.Job`), katman yinelemez.
- **Yanıt tablosu:** `success` → `Paid` (sorulan `merchant_oid` ödeme kimliği; tutarlar `TryParseProviderText`; `KART`, `TL`/`TRY`, `test_mode` işaretleri) · `error` + `004` → `NoPayment` · `001`–`003` → `NoResponse(CodeError: true)` · 2xx dışı, JSON olmayan gövde, bilinmeyen `status`/`err_no`, zaman aşımı, bağlantı hatası → `NoResponse` (beş dakika). Tanınmayan tutar metni → `NoResponse("amount-text", CodeError: true)`. `err_msg` ve kart/kişisel alanlar okunmaz.
- **İki aralık ayrı sınıfta:** `FinalPaymentQueryRule.Decide` — `AskAgain` (5 dk, `JobTimings.FinalPaymentQueryInterval`) ve yeni `RetryByCodeErrorRule` (işleyici atar, çalıştırıcı varsayılan kuralı uygular: 1, 5, 15, 60, 240 dk).
- **Beş koşul** çekirdekte: `FinalPaymentQueryRule.NotAppliedReason` — `amount` (`PaymentAmounts.QueryMatches`: `payment_amount` eşit, `payment_total` ≥, kuruşta), `payment-type`, `currency`, `test-mode` (yalnız canlı kurulumda). Uygulanmayan başarı `LogEvents.FinalPaymentQueryNotApplied` uyarı satırı (`reason`, `orderId`) + kod hatası.
- **İşleyici kaydı** (D-449): `PaymentModule` → `AddJobHandler<FinalPaymentQueryJobHandler>(JobType.FinalPaymentQuery)`; ağ geçidi ve ortam isteğe bağlı kurucu parametresi — ağ geçidisiz host (komut kipi, öteki modüllerin test host'ları) kurulur, satır orada kod hatası kuralıyla düşer.
- **`PaymentAmounts`:** `TryParseProviderText` (DV-12 kuralı) ve `QueryMatches` (D-741 kapandı). `09 §14.2` m.4'ün mimari testi yazıldı: bağlantı katmanında `decimal.Parse`/`TryParse`, `Convert.ToDecimal`, JSON okuyucusunun ondalık alıcıları yalnız `PaymentAmounts`'ta (derlenmiş koddan; ihlal örneği her biçimi yakalar).
- **Tek tahsilat (KK8):** aynı `merchant_oid`'li ikinci başarılı bildirim (başka toplamla) uygulanmış siparişte tekrar sayılır — ikinci kayıt ve iş satırı yok, tek ödeme kaydı.
- **Belgeler:** `09 §18.2` m.3 olay listesine `FinalPaymentQueryNotApplied`; `09 §14.2` m.4'e testin okuma biçimi; `08 §4.1` DV-12 hücresi; `INTEGRATION_RUNBOOKS/PAYTR.md` kapsam, test komutları, §6.1 DV-12 satırı, sorun giderme ve kesinti.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/`: `PayTrPaymentQuery.cs` (yeni), `PayTrPaymentGateway.cs`, `PaymentAmounts.cs`, `PaymentQueryResult.cs`, `ExternalCallRunner.cs`
- `src/Shopfolio.Web/Infrastructure/Logging/LogEvents.cs`
- `src/Shopfolio.Web/Modules/Payment/`: `FinalPaymentQueryJobHandler.cs`, `FinalPaymentQueryRule.cs`, `PaymentModule.cs`
- `src/Shopfolio.Web/Modules/Ordering/`: `Contracts/IOrderingModule.cs` (`CardPaymentQueryTarget.Total`), `OrderingModule.cs`
- Testler: Unit `Integrations/PayTrPaymentGatewayTests.Query.cs`, `Integrations/PaymentAmountsQueryTests.cs`, `Payment/FinalPaymentQueryRuleTests.cs`, `Architecture/TimeAndMoneyRulesTests.ProviderAmounts.cs` + `ViolationSamples/TimeAndMoney/ProviderAmountViolations.cs`; Integration `Ordering/PaymentWindowTests.PayTr.cs` (yeni), `Ordering/PaymentWindowTests.cs`, `Payment/PaymentNotificationTests.PayTr.cs`
- Belgeler: `09`, `08`, `INTEGRATION_RUNBOOKS/PAYTR.md`, `DEFERRED_BACKLOG.md` (yerinde kapanış), `DEFERRED_BACKLOG.d/T403.md`, `IMPLEMENTATION_STATUS.md` (T403 satırı), `STATUS_CHANGELOG.d/T403.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `POST /odeme/durum-sorgu`, form kodlu, `paytr_token` üç girdili, süre 30 sn | ✓ | `PayTrPaymentGatewayTests.Query_PostsOnceToTheQueryAddress_FormEncoded_WithThreeFields_AndTheKnownToken_KK1` (token openssl ile .NET dışında hesaplandı), `Query_ThatHangs_EndsAtTheThirtySecondJobLimit_AsNoResponse_WithOneRequest_KK1_KK3` |
| 2 | Eşleşen başarı `Paid`; `004` → `NoPayment` | ✓ | `Query_004_IsNoPayment_ASeparateClass_KK2`, `Query_Success_IsPaid_…_KK5_KK7`; Integration `PaymentWindowTests.PayTr_Query004_IsNoPayment_AndCancelsTheOrderAtTheWindowsEnd_KK2` |
| 3 | `001`–`003`, zaman aşımı, bağlantı hatası, tablosuz yanıt `NoResponse`; sipariş iptal edilmez; olay kayda | ✓ | Integration `PayTr_QueryRequestError_…_KK3_KK4` (001/002/003), `PayTr_QueryWithoutAnAnswer_…_KK3_KK4` (Hang/Refuse/Unlisted/503): sipariş Placed/Pending/sürüm 1, ödeme kaydı yok, `ExternalCallFailed` satırı; Unit `Query_RequestErrors_…`, `Query_ConnectionError_…`, `Query_AnAnswerOutsideTheTable_…` |
| 4 | 5 dk yalnız kesinti ve tablosuz yanıt; `001`–`003` kod hatası kuralı | ✓ | `FinalPaymentQueryRuleTests.AnUnansweredCallThatWaitingMayChange_…_KK4`, `AnUnansweredCallThatWaitingDoesNotChange_…_KK4`; Integration: request error → `InvalidOperationException` (çalıştırıcı varsayılan kural), kesinti → `RescheduleAfter(FinalPaymentQueryInterval)` |
| 5 | Beş koşul; canlıda `test_mode` 1 uygulanmaz; uygulanmayan başarı uygulama kaydına | ✓ | `FinalPaymentQueryRuleTests.ASuccessWithOneBrokenCondition_…_KK5`, `ATestTransaction_IsAppliedOutsideTheLiveInstallation_KK5`, `PaymentAmountsQueryTests.QueryMatches_…_KK5`; Integration `PayTr_SuccessWhoseConditionDoesNotHold_…_KK5` (5 satır: iki tutar, tip, para birimi, canlıda test) — `FinalPaymentQueryNotApplied <reason> <orderId>` uyarı satırı, referanssız; `PayTr_SuccessWithTheInstalmentDifference_IsApplied_…_KK5_KK7` |
| 6 | Tutar metni ayraçsız ya da tek ayraçlı en çok iki ondalık; başkası `NoResponse` | ✓ | `PaymentAmountsQueryTests.TryParseProviderText_OneSeparatorOrNone_…_KK6`, `_ATextOutsideTheRule_…_KK6`, `_IsTheSameUnderEitherServerCulture_KK6`; `Query_SuccessWithAnAmountTextOutsideTheRule_…_KK6`; Integration `PayTr_SuccessWithAnUnrecognizedAmountText_…_KK6` |
| 7 | Kart ve kişisel alanlar okunmaz, saklanmaz | ✓ | `Query_TheCardAndPersonalFieldsOfTheAnswer_AreInNeitherTheResultNorALogLine_KK7`; Integration `PayTr_SuccessWithTheInstalmentDifference_…_KK7` (maskeli kart, marka, auth kodu, kesinti ne kayıtta ne `payment_records`/`orders` satırında) |
| 8 | Aynı `merchant_oid`'li ikinci başarılı bildirim tekrardır, yazılmaz | ✓ | Integration `PaymentNotificationTests.PayTr_SecondSuccessOfTheSameMerchantOid_WithAnotherTotal_IsARepeat_NoRecordNoJobRow_AndOnePaymentRecord_T403_KK8`; kalan risk DV-11 (`08 §2.1.7`) |
| 9 | DV-12'nin ayracı ve para birimi yazımı gözlenip `08 §2.1.6`'ya eklenir | ✓ (gözlem OWNER_INPUT) | `08 §4.1` DV-12 hücresi ve `PAYTR.md` §6.1 DV-12 satırı tarihli "Bekliyor — OWNER_INPUT"; gerçek gözlem payı D-1065 (test mağazası yok, uydurulmadı) |

### Öz denetim (bitiş kapısı 2)

| Madde (11 KK / 12 / 09 §25) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 istek biçimi ve süre | ✓ | `Query_PostsOnceToTheQueryAddress_…_KK1`, `Query_ThatHangs_…_KK1_KK3`; Integration Hang satırı sahte saatle 30 sn |
| KK2 `004` ayrı sonuç | ✓ | `Query_004_…_KK2`, `FinalPaymentQueryRuleTests.NoPayment_IsItsOwnStep_…_KK2`, Integration `PayTr_Query004_…_KK2` |
| KK3 altı hata girdisi `NoResponse`, sipariş sabit | ✓ | Integration 7 satır (001, 002, 003, zaman aşımı, bağlantı reddi, tablosuz gövde, 503); her biri sipariş sürümü 1 ve ödeme kaydı yok |
| KK4 iki aralık ayrı sınıfta | ✓ | `NoResponse.CodeError` + `FinalPaymentQueryStep.RetryByCodeErrorRule` / `AskAgain`; iki Unit + iki Integration theory |
| KK5 beş koşuldan biri bozulunca uygulanmaz ve uygulama kaydına yazılır | ✓ | Integration theory 5 satır, satırın biçimi ve referanssızlığı denetlenir |
| KK6 `10`, `10,8`, `9.76`, `10,123`, `1.234,56` tablosu | ✓ | `PaymentAmountsQueryTests` (`CultureTexts` iki kültürde), Integration iki tanınmayan metin |
| KK7 okunmayan alan sonuçta yok | ✓ | Unit `…_KK7`, Integration dört değer kayıtta ve satırlarda yok |
| KK8 ikinci bildirimde satır yok | ✓ | `PayTr_SecondSuccessOfTheSameMerchantOid_…_T403_KK8` |
| KK9 DV-12 satırı | ✓ (OWNER_INPUT) | `08 §4.1`, `PAYTR.md` §6.1; D-1065 |
| 09 §25: tek işlev | ✓ | Tutar dönüşümü ve karşılaştırması `PaymentAmounts`; imza `PayTrSignature`; numara `OrderNumber.WithoutSeparator`; ödemeyi uygulayan tek işlev `IOrderTransitions.ApplyCardPaymentAsync` |
| 09 §14.2 m.4: kaynak taraması | ✓ | `TimeAndMoneyRulesTests.ProviderAmountParsing_*` — derlenmiş koddan; ihlal örneği 8 biçim (span, yöntem grubu, lambda, JSON alıcısı dahil), istisna yalnız sınıf adıyla, katman dışı kapsam dışı |
| 09 §18.2 m.3: kapalı olay listesi | ✓ | `FinalPaymentQueryNotApplied` `LogEvents` ve 09 metninde aynı PR'da; `LoggingConsistencyTests.LogEvents_EqualTheClosedListOfTheGuidelines` |
| 09 §3.5 sabitler | ✓ | Yeni süre/tavan sabiti yok (30 sn ve 5 dk var olan `ExternalCallTimeouts.Job`, `JobTimings.FinalPaymentQueryInterval`) |
| 09 §19: sahte saat | ✓ | Integration zaman aşımı `ManualTimeProvider` ile; gerçek bekleme yok |
| Modül kurucusuna bağımlılık → Integration TAM | ✓ | Integration 3546 test (parçalı, `origin/main` birleştirilmiş ağaçta) |
| `dotnet format` test dosyaları dahil | ✓ | Web, UnitTests, IntegrationTests ayrı ayrı `--verify-no-changes` temiz |
| Backlog: hedefi T403 olan kalemler | ✓ | D-448 ✅, D-449 ✅, D-741 ✅ (yerinde); D-97'nin `FinalPaymentQuery` payı ✅; yeni D-1065 |
| Güvenlik taraması | ✓ | Anahtar ve tuz istekte ve kayıtta yok (Unit KK1); `err_msg` kayda yazılmaz (Integration `MESSAGE-WITH` yok); kod hatası iletisi yalnız neden kodu; uyarı satırı referans ve tutar taşımaz; test anahtarları parçalı yazıldı (sır bekçisi) |

## Test Sonuçları

- Unit: 6113/6113 (Consistency hariç), Consistency 292/292, `TimeAndMoneyRulesTests` 44/44.
- Integration: 3546 test parçalı (973 + 1928 + 645) yeşil. İki koşuda birer test ortam kaynaklı düştü ve yeniden koşuda geçti: `SitemapEndpointTests` (paylaşılan geçici dizinin probe dosyasına Windows erişim reddi) ve ikinci grupta adı kaydedilmemiş bir test; ikisi de T403 koduna dokunmuyor.
- `dotnet format --verify-no-changes`: üç proje temiz.

## ⚠ Kararlar

- Uygulanmayan başarının kararı çekirdekte (`NotAppliedReason`), tutar karşılaştırması `PaymentAmounts.QueryMatches`; bildirimdeki gibi ayrı ağ geçidi yöntemi açılmadı.
- JSON sayı biçimindeki alanlar ham metinleriyle okunur; sayısal `err_no` `4` tabloda değildir (5 dk).
- Bilinmeyen `err_no` yalnız en çok dört haneli rakamsa kayda yazılır, değilse `body`.
- `09 §14.2` m.4 testi JSON okuyucusunun ondalık alıcısını da dönüşüm sayar (aynı kuralın başka çağrı biçimi; kapsam bağlantı katmanı).
- İşleyicinin ağ geçidi isteğe bağlı: ağ geçidisiz host'ta satır `no-gateway` kod hatasıyla düşer (D-449'un ValidateOnBuild sorunu böyle çözüldü).

## Açık kalanlar

- D-1065 (OWNER_INPUT): DV-12'nin test mağazasında gözlemi.
