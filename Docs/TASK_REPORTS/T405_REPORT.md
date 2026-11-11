# T405 — PayTR: kart iadesi

**Faz:** F4 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-11

- **Dal:** `task/T405-paytr-kart-iadesi` (taban `main` `5a29775e`)
- **Bağımlılık:** T404 ✓
- **Yapım:** üç işçi (ilki istek ve sorgu katmanında, ikincisi iş işleyicileri ve testlerden sonra bağlam sınırında bölündü — ara commit'ler `d57bf815`, `de4437a3`, `e1c3f1c7`; üçüncüsü Integration'ın kalan ad alanlarını ve Unit'i tam koştu, VAL-E093'ün tarihli öğe durumunu ekledi, belgeleri, backlog'u, kayıtları ve raporu yazdı)

---

## Yapılan İşler

- **İstek ve sorgu (`PayTrRefund`, `PayTrPaymentGateway`):** `POST /odeme/iade` beş alanla (form kodlu); `paytr_token` `merchant_id` + `merchant_oid` + `return_amount` + `merchant_salt` üzerinden, `reference_no` özete girmez; `return_amount` `PaymentAmounts.ToPlainText` (iki ondalık, nokta, kültürden bağımsız) ve özete giren dizge gönderilendir; `reference_no` iade kaydının tiresiz kimliği. Yanıt: `success` → `Accepted`, `failed`/`error` → `NotCompleted` (`err_no` kayda, mesaj alan değerlerinden arındırılmış), 2xx dışı / tablosuz / yanıtsız → `Unknown`. Sorgu son sorgunun isteğidir; `returns` içinde kimliği ve tutarı (kuruşla) eşit öğe `Completed`, `004`, öğe yokluğu ve farklı tutar `NoResult`. Anahtar yoksa çağrı yapılmaz (`keys-missing`). Süre iş çağrısının 30 sn'si (`ExternalCallTimeouts.Job`).
- **Kayıt istekten önce (`PaymentModule.WriteCardRefundSendingAsync`, `IOrderTransitions.BeginCardRefundRequestAsync`):** "gönderiliyor" kaydı siparişin kilidi altında kendi işleminde yazılıp kesinleşir, sonra istek bir kez gider; geri ödeme kart yolunda değilse, gönderilmiş ya da geçersizleşmişse (D-518, D-908) ya da açık/başarılı kaydı veya bu satırın yazdığı kayıt varsa kayıt yazılmaz. Kaydın yazımı siparişin sürümünü bir artırır.
- **İş işleyicileri:** `CardRefundJobHandler` (`CardRefundJobRule.ReadLatestRecordAsync`/`Decide`/`OutcomeOf`; "gönderiliyor"da kalmış kayıt yeniden gönderilmez, "sonucu bekleniyor"a döner) ve `RefundOutcomeQueryJobHandler` (`RefundOutcomeQueryRule.NextDelay`: zaman aşımından 1 dk sonra, sonra 1, 5, 15, 60 dk, sonra saatte bir, üst sınırsız); iki sonuç yolu aynı işlevle uygulanır (`ApplyCardRefundOutcomeAsync`); bilinen yanıt `CancellationToken.None` ile uygulanır. Ağ geçidi isteğe bağlı kurucu parametresi; ağ geçidisiz host kurulur, satır kod hatası kuralıyla düşer. Kayıt `PaymentModuleRegistration`, `AutomaticMoments` 4.2.7.
- **Firma işareti ve geç başarı:** işaretten sonra sorgu satırı yazılmaz, işaretli kayıtta sorgu yapılmaz; işaret anında yanıtı beklenen sorgunun "gerçekleşti"si siparişe işlenmez, kayıtta `LateSuccessAt` bir kez yazılır (`RecordLateCardRefundSuccessAsync`), sistem geçişi tek ret satırıyla reddedilir.
- **Ayıp çözümü (D-878 kart payı):** `StartDefectRemedyRefundAsync` kart yolunda `CardRefund` iş satırını aynı işlemde yazar.
- **Belgeler:** `09 §3.5` sorgu takvimi satırı (`JobTimingsConsistencyTests.RefundOutcomeQueryRow_*`); `08 §4.1` DV-6 ve DV-10 son hücreleri; `PAYTR.md` kapsam notu, §6.1 DV-6/DV-10 satırları, §7 iade satırları.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Integrations/PayTrRefund.cs` (yeni), `PayTrPaymentGateway.cs`
- `src/Shopfolio.Web/Infrastructure/Jobs/JobTimings.cs`, `AutomaticMoments.cs`
- `src/Shopfolio.Web/Modules/Payment/` — `CardRefundJobHandler.cs`, `RefundOutcomeQueryJobHandler.cs`, `RefundOutcomeQueryRule.cs` (yeni), `CardRefundJobRule.cs`, `PaymentModule.cs`, `Contracts/IPaymentModule.cs`, `Contracts/PaymentFacts.cs`
- `src/Shopfolio.Web/Modules/Ordering/` — `OrderTransitions.Payment.cs`, `OrderingModule.cs`, `Contracts/IOrderTransitions.cs`, `IOrderingModule.cs`, `PaymentTransitions.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Payment/CardRefundJobTests.cs` (yeni); `tests/Shopfolio.UnitTests/Integrations/PayTrPaymentGatewayTests.Refund.cs`, `Jobs/RefundOutcomeQueryRuleTests.cs` (yeni), `Consistency/JobTimingsConsistencyTests.cs`, `Fakes/FakePaymentGateway.cs`, `Ordering/ShipmentTransitionContractTests.cs`
- Belgeler: `Docs/09_CODING_GUIDELINES.md` §3.5, `Docs/08_INTEGRATION_SPEC.md` §4.1, `Docs/INTEGRATION_RUNBOOKS/PAYTR.md`, `Docs/DEFERRED_BACKLOG.md` (D-97, D-518, D-878, D-908 yerinde), `Docs/DEFERRED_BACKLOG.d/T405.md`, `Docs/STATUS_CHANGELOG.d/T405.md`, `Docs/IMPLEMENTATION_STATUS.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `POST /odeme/iade`; token dört girdili, `reference_no` özete girmez; 30 sn | ✓ | Unit `Refund_PostsOnceToTheRefundAddress_FormEncoded_WithFiveFields_AndTheKnownFourInputToken_KK1_KK3` (openssl ile hesaplanmış bilinen token), `Refund_ThatHangs_EndsAtTheThirtySecondJobLimit_AsUnknown_WithOneRequest_KK1_KK5` (29 sn açık, 30 sn `Unknown`) |
| 2 | `return_amount` iki ondalık, noktalı, kültürden bağımsız; özete giren gönderilen | ✓ | `Refund_Amount_IsTwoDecimalsWithAPoint_WhateverTheCulture_AndTheTokenIsOverTheSentText_KK2` (`tr-TR`, `de-DE`; 10 → `10.00`) |
| 3 | `reference_no` tiresiz kayıt kimliği; tekrar koruması değil | ✓ | `…_KK1_KK3` (`ToString("N")`); tekrar koruması kayıttır (kriter 4) |
| 4 | Kayıt kesinleşince istek; yeniden gönderilmez; en çok bir açık + bir başarılı | ✓ | Integration `Request_GoesAfterTheSendingRecordIsCommitted_AndAcceptedIsSucceeded_O3_B8_F4_KK4_KK5` (istek anında kayıt ayrı bağlantıdan görünür), `RowSeenAgain_AfterItsRequest_SendsNoSecondRequest_KK4`, `RowSeenAgain_WithItsSendingRecord_DoesNotSend_TheOutcomeIsPending_AndTheQueryRowIsWritten_KK4`, `FirmRetry_IsANewRowAndANewRecord_AfterTheFailedOne_KK4` |
| 5 | `success` → `Accepted`; `failed`/`error` → `NotCompleted`; yanıtsızlık → `Unknown` | ✓ | Unit `Refund_FailedOrError_IsNotCompleted_WithTheCodeWritten_AndTheMessageMasked_KK5`, `Refund_AnswerOutsideTheTable_IsUnknown_NotARefusal_KK5`, `Refund_ConnectionError_IsUnknown_AndIsNotSentAgain_KK5`, `RefundOutcomeQueryRuleTests.RequestAnswer_MapsToTheRecordsOutcome_KK5`; Integration `Answer_RefusedOrUnknown_LeavesTheAxis_FailedHasNoQuery_UnknownWritesTheQueryRow_KK5_KK9` |
| 6 | Eşit öğe `Completed`; `004`, öğe yok dahil başkası `NoResult` | ✓ | `RefundQuery_ItemWithTheReferenceAndTheAmount_IsCompleted_AndAsksTheFinalQuerysAddress_KK6`, `RefundQuery_MatchingItem_IsCompleted_WithOrWithoutItsCompletionDate_KK6`, `RefundQuery_004_IsNoResult_KK6`, `RefundQuery_WithoutTheItem_OrWithAnItemOfAnotherAmount_IsNoResult_KK6`, `RefundQuery_AnswerOutsideTheTable_IsNoResult_AndWritesItsReason` |
| 7 | 1 dk, sonra 1/5/15/60 dk, sonra saatlik; üst sınır yok | ✓ | `RefundOutcomeQueryRuleTests.Schedule_IsTheFirstDelay_ThenTheFourIntervals_ThenHourly_KK7`, `Schedule_ThatRunsAFewSecondsLate_KeepsTheSameIntervals_KK7`, `Query_AfterZ17sFourteenDays_StillRepeatsHourly_WithNoUpperLimit_KK7`; Integration `Query_NoResult_AsksAgainByTheSchedule_AndPastZ17StillHourly_KK7`; sabitler `JobTimings.RefundOutcomeQuery*` = `09 §3.5` (`JobTimingsConsistencyTests.RefundOutcomeQueryRow_*`) |
| 8 | İki sonuç yolu aynı işlev; işaretten sonra sorgu durur, geç iade işlenmez | ✓ | `Query_Completed_IsAppliedByTheSameFunction_AndASecondApplicationChangesNothing_KK6_KK8`, `AfterTheFirmsMark_NoQueryIsAsked_AndNoQueryRowIsWritten_KK8`, `MarkWhileTheQueryWaits_ItsCompletedIsNotApplied_AndStaysOnTheMarkedRecord_OnlyOnce_KK8` |
| 9 | Anahtarsız istek yok, "gerçekleşmedi" | ✓ | Unit `Refund_AndItsQuery_WithoutTheKeys_SendNothing_AndTheRefundDidNotHappen_KK9`; Integration `Answer_RefusedOrUnknown_*_KK5_KK9` (keys-missing kolu) |
| 10 | DV-6 ve DV-10 gözlenir; DV-10'un `02` dönüşü T410 | ✓ (gözlem OWNER_INPUT) | `08 §4.1` DV-6, DV-10 ve `PAYTR.md` §6.1 tarihli "gözlenmedi" satırları; kod payı sabit (adlar `PayTrRefund.ReferenceField`/`AmountField`); gözlem D-1082/D-1083; uydurulmadı |

> Kriter 10'un gerçek gözlem payı T401–T404'teki gibi OWNER_INPUT'tur (PayTR test mağazası yok). Kod payı tam; ad uymazsa sorgu "sonuç yok" kalır, para yanlış "gerçekleşti" sayılmaz.

### Öz denetim (bitiş kapısı 2)

| Madde (11 KK / 12 / 09 §25) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 istek biçimi ve özet | ✓ | `Refund_PostsOnceToTheRefundAddress_*_KK1_KK3`, `Refund_ThatHangs_*_KK1_KK5` |
| KK2 10 → `10.00`, `tr-TR`'de de | ✓ | `Refund_Amount_*_KK2` |
| KK3 tiresiz kimlik | ✓ | `…_KK1_KK3` |
| KK4 kayıt istekten önce | ✓ | `Request_GoesAfterTheSendingRecordIsCommitted_*_KK4_KK5` (sahte sağlayıcının `BeforeRefund` kancası kaydı ayrı bağlantıdan okur), `RowSeenAgain_*` (iki), `FirmRetry_*` |
| KK5 üç yanıt sınıfı | ✓ | Unit KK5 dört test; Integration `Answer_RefusedOrUnknown_*` |
| KK6 dört sorgu durumu | ✓ | Unit KK6 dört test + tablosuz yanıt testi |
| KK7 beş aralık; Z-17 sonrası saatlik | ✓ | `RefundOutcomeQueryRuleTests` üç test (sahte saat), Integration `Query_NoResult_*_KK7` |
| KK8 ikinci uygulama; işaretten sonra satır yok; işaret anındaki sorgu işlenmez | ✓ | `Query_Completed_*_KK6_KK8`, `AfterTheFirmsMark_*_KK8`, `MarkWhileTheQueryWaits_*_KK8` |
| KK9 anahtarsız istek yok | ✓ | `Refund_AndItsQuery_WithoutTheKeys_*_KK9` |
| KK10 DV-6 ve DV-10 satırları | ✓ (OWNER_INPUT) | `08 §4.1`, `PAYTR.md` §6.1; D-1082, D-1083 |
| `12` VAL-E093 (altı sorgu yanıtı) | ✓ (kod) | Tarihli/tarihsiz eşleşen öğe `RefundQuery_MatchingItem_*`; farklı tutar, başka kimlik, boş `returns` `RefundQuery_WithoutTheItem_*`; `004` `RefundQuery_004_*`; eksen değişmez, sonraki sorgu kurulur `Query_NoResult_*`. `Durum` hücresi doğrulayıcının |
| `12` VAL-A990 (takvim) | ✓ (kod) | İlk sorgu satırının `NextAttemptAt`'i zaman aşımı + 1 dk (`Answer_RefusedOrUnknown_*` "unknown" kolu); aralıklar `RefundOutcomeQueryRuleTests` (1, 2, 7, 22, 82 dk, sonra saatlik); `Query_NoResult_*_KK7` (15 gün sonra saatlik); `AfterTheFirmsMark_*` |
| `12` VAL-E007, E008, E009, E011 | not | Gerçek kayıt ister (DV-6/7/8/10); `BEKLEMEDE` kalır (D-1082, D-1083; DV-7/DV-8 PayTR sözleşmesi) |
| 09 §3.5 yeni süre sabiti tabloda | ✓ | `09 §3.5` satırı + `JobTimingsConsistencyTests.RefundOutcomeQueryRow_*`; testler sabitten türetir |
| 09 §3.5 yeni metin karar kaydı | ✓ | Satır `08 §2.1.9`'un ve K-1238 (3)'ün var olan takvimini kayda alır; belge kuralı değişmez (karar kaydı gerekmez) |
| 09 §6 modül sınırı; arayüzde `SaveChanges` yok | ✓ | `WriteCardRefundSendingAsync`, `RecordLateCardRefundSuccessAsync` çağıranın izleyicisine yazar, kaydetmez; işlemi `ITransactionRunner` açar; mimari testler (Unit tam) yeşil |
| 09 §7 zaman `TimeProvider`'dan | ✓ | İşleyici ve modül `TimeProvider`; testler sahte saat |
| 09 §9.1 geçiş birimi; dış yan etki iş satırı | ✓ | Sonuç `ApplyCardRefundOutcomeAsync`'te; sorgu satırı kayıtla aynı işlemde (`_jobs.EnqueueAsync`); çözüm iadesinin satırı aynı işlemde |
| 09 §10.2 eşzamanlılık | ✓ | Kayıt siparişin kilidi altında (IBAN girişi ve düzeltmeyle aynı kilit; D-908 testi); kayıt yazımı sürümü artırır |
| 09 §11 dış çağrı arayüzde, sonuç sınıfıyla; yineleme yok | ✓ | `IPaymentGateway.RequestRefundAsync`/`QueryRefundAsync`; `ExternalCallRunner` tek çağrı; `Refund_ConnectionError_*` istek 1 |
| 09 §11.1 m.6 red nedeni kayda, alan değeri değil | ✓ | `WithoutFieldValues` ile maskeli mesaj (`Refund_FailedOrError_*_MessageMasked`) |
| 09 §13 iş türü kayıtlı, işleyici sahip task'ta | ✓ | `PaymentModuleRegistration`; `WithoutTheGatewayInTheHost_TheRowFailsByTheCodeErrorRule_BeforeAnyRecord` (ağ geçidisiz host kurulur) |
| 09 §14 para yalnız `PaymentAmounts` | ✓ | `ToPlainText`, `TryParseProviderText`, `ToKurus` |
| 09 §25 tek işlev | ✓ | Sorgu isteği `PayTrPaymentQuery.Build`'i, hata kodu `PayTrPaymentQuery.ErrorReason`'ı, sonuç `ApplyCardRefundOutcomeAsync`'i yeniden kullanır |
| Modül kurucusuna yeni bağımlılık → Integration TAM | ✓ | Integration tam: `Ordering`+`Payment`+`Jobs`+`Commands` 839 (işçi 2, `e1c3f1c7`) + öteki bütün ad alanları 2724 (işçi 3, aynı kod) = 3563 yeşil; listelenen 3557 testin hepsi kapsandı |
| Hedefi T405 olan backlog kalemleri | ✓ | D-97 ✅, D-518 ✅, D-908 ✅ (yerinde); D-878 kart payı ✅, havale payı T529'a; `DEFERRED_BACKLOG.md` + `.d/` grep'inde başka kalem yok |
| Göç / şema / yeni paket / ayar | uygulanamaz | Göç yok, paket yok, `.env.example` değişmez (kod sabiti) |
| Bekçi/tarama istisnası | uygulanamaz | Yeni istisna yok |
| `dotnet format` test dosyaları dahil | ✓ | Web, UnitTests, IntegrationTests `--verify-no-changes` temiz (UnitTests son değişiklikten sonra yeniden) |
| Güvenlik taraması | ✓ | Anahtar, token ve alan değeri kayda yazılmaz; yeni uç, yetki ya da girdi yüzeyi yok; para gönderen istek kayıt kesinleşmeden gitmez ve yinelenmez; yeni bağımlılık yok |

## Test Sonuçları

- **Unit:** 6482 yeşil (tam, Consistency dahil); son eklenen testten sonra `PayTrPaymentGatewayTests` 155 yeşil.
- **Integration:** 3563 yeşil (Postgres 18, `track_commit_timestamp=on`), iki parçada (yukarıda); son test değişikliğinden sonra `Payment` ad alanı 47 yeniden yeşil.
- **Biçim:** `dotnet format --verify-no-changes` üç projede temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-11; dal ucu `be7accaf`; `origin/main` `5a29775e`, birleştirme gerekmedi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 — K1 → D-1087 (`RefundOutcomeQueryRule.NextDelay` bekleyişi geçen süreden seçer; çalıştırıcı uzun süre geç kalırsa sonraki bekleyiş daha uzun aralıktan seçilir; zamanında koşuda 1, 2, 7, 22, 82 dk tutar) |

**Tur 1 kanıtları (doğrulayıcı):** kriter 1–10 ve KK1–KK10 ✓ (kriter 10'un test mağazası gözlem payı OWNER_INPUT, D-1082/D-1083). Kod okundu: `PayTrRefund`, `PayTrPaymentGateway` (`RequestRefundAsync`, `QueryRefundAsync`, `PostRefund*`), `CardRefundJobHandler`, `CardRefundJobRule`, `RefundOutcomeQueryJobHandler`, `RefundOutcomeQueryRule` (takvim elle hesaplandı: 1, 2, 7, 22, 82 dk, sonra 60 dk), `PaymentModule` (`WriteCardRefundSendingAsync`, `RecordLateCardRefundSuccessAsync`, bekleyen sonuçta sorgu satırı, ayıp çözümünün kart satırı), `OrderTransitions.BeginCardRefundRequestAsync` ve geç başarı kolu, `ExternalCallRunner` (30 sn iş süresi). Bilinen iade özeti `printf '%s' '12345620267K4M9P10.00salt-sample' | openssl dgst -sha256 -hmac key-sample -binary | base64` = `f02lP+RU…ameFg=` testteki değere eşit. CI run 42906065591 `success` (9 iş). Yerel: `dotnet build -warnaserror` 0 uyarı, `dotnet format --verify-no-changes` üç projede temiz, Unit 6483 yeşil (Consistency, Architecture dahil), betik testleri 4 dosya yeşil, Integration (Postgres 18, `track_commit_timestamp=on`) 14 sınıf 517 yeşil. `12`: `VAL-E093`, `VAL-A990` → `PASS`; `12_matris_denetle.py --kapsam 1-77` Hata 0.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (sorgu takvimi kod sabiti, `09 §3.5`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T405-paytr-kart-iadesi`
- **Commit:** squash `cda5032` (#529) (F4 gate'te dolduruldu)
- **PR:** #529
- **CI:** run 42906065591 `success` (dal ucu `be7accaf`)
- **Post-merge run'lar:** squash `cda5032` (#529) — CI 42912360534 success, Publish 42912360420 success (F4 gate'te dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | PayTR test mağazası ve anahtarları (DV-6 alan adları, DV-10 taksitli tam iade) | Depoda ve ortamda test mağazası yok; T401–T404 aynı durumu kaydetti | Kırık — OWNER_INPUT (D-1082, D-1083) |
| 2 | Sorgu öğesinin adları pazaryeri örneğinin adlarıdır | `08 §2.1.9` (DV-6) | Gözlenmedi; uymazsa "sonuç yok" (güvenli yön) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `returns` öğesinin alan adları gözlenmedi | D-1082 | Test mağazası yok (OWNER_INPUT) |
| 2 | Taksitli tam iade gözlenmedi; `02` dönüşü | D-1083 | Test mağazası ve yazılı teyit yok; dönüş T410 |
| 3 | `MarkCardRefundFailed` komutu ve `LateSuccessAt` panel uyarısı | D-1084 | E-37 / UC-137 T529'da |
| 4 | Ayıp çözümü iadesinin havale payının sayfadan sınanması | D-878 (açık kalan pay) | Sayfa T529'da |

## Notlar

- **Working tree:** temiz; işçi 3 kendi worktree'sinde `origin/task/T405-paytr-kart-iadesi`'den (`e1c3f1c7`) devam etti; `origin/main` ilerlemedi (`5a29775e`), birleştirme gerekmedi.
- **Repo memory:** dokunulmadı.
- **⚠ kararlar (kurala en uygun seçenekle):**
  - (a) `BeginCardRefundRequestAsync` siparişin sürümünü bir artırır: `06 §4.17`/§4.34 "kaydın yazımı siparişin sürümünü artırır"; açık bir formun eski sürümü çakışma alır.
  - (b) Geç başarıda (işaretten sonra) sistem geçişi tek `AutomaticTransitionRefused` satırıyla reddedilir ve `LateSuccessAt` yazımı sürümü bir artırır; eksen ve bildirim değişmez.
  - (c) Bilinen yanıt (`Accepted`, `NotCompleted`, sorgunun `Completed`'ı) `CancellationToken.None` ile uygulanır: host dururken kaybolan ret "sonucu bekleniyor"da kalırdı ve sorgu reddi göstermez.
  - (d) DV-6/DV-10 OWNER_INPUT: uydurulmadı; kod güvenli yönde (eşleşmeyen ad = sonuç yok).
  - (e) Sorgu takvimi satırın oluşturulma anından sayılır (sayaçsız); geç alınan satır aynı aralıkları korur (`Schedule_ThatRunsAFewSecondsLate_*`).
  - (f) D-878'in havale payı kapatılmadı, T529'a yeniden hedeflendi: genel havale işlemi (`ProcessTransferRefundAsync`) çözüm iadesini de işler ama sayfadan sınanması E-37'dedir.
  - (g) VAL-E093'ün "tamamlanma tarihi dolu öğe" durumu için Unit testi eklendi (`RefundQuery_MatchingItem_*`); kod tarihe bakmaz, öğenin varlığı yeter. VAL-A990'ın "ilk sorgu anından önce gitmez" payı için ilk sorgu satırının zamanı Integration testinde iddia edildi.
