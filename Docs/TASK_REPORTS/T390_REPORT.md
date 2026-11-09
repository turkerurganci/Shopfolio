# T390 — Uçtan uca dal akış testleri (servis düzeyi): karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `03 §2.10.2`…§2.10.5'in dört dal anlatısı dört ayrı Integration testinde (IBAN'sız iade iki kollu `Theory`: firmanın "mal dönmedi" kapanışı / Z-38 silmesi), gerçek PostgreSQL ve elle ilerletilen saatle (`ManualTimeProvider`), servis katmanından: kartla ödeme `FakePaymentGateway` + uygulamanın ödeme bildirimi ucu + iş koşucusu.
- Siparişler anlatının başladığı durumda tohumlanır (iki eksen SQL ile; çift kuralı veritabanında yine geçerli); sonraki her adım uygulamanın kendi işlemleriyle (`IOrderTransitions`, `IOrderCorrections`, `IAfterSalesModule`, `RefundIbanDeletionDay`) yürür; her adımdan sonra iki eksen, iz, bildirim satırları ve kayıtlar ayrı bağlantıdan okunur (KK10).
- **D-828 kapandı:** ödeme bildirimi ucunu barındıran test sunucusu tek kurucuda (`PaymentNotificationHost.StartAsync`); `PaymentNotificationTests`, `MainFlowTests` ve `BranchFlowTests` onu kullanır (üç kopya → bir).
- Üretim kodu değişmedi.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.IntegrationTests/Ordering/BranchFlowTests.cs` (yeni; düzenek, koleksiyon `BranchFlows`, yardımcılar)
- `tests/Shopfolio.IntegrationTests/Ordering/BranchFlowTests.MixedOrder.cs`, `BranchFlowTests.Withdrawals.cs`, `BranchFlowTests.EmailCorrection.cs` (yeni; anlatılar)
- `tests/Shopfolio.IntegrationTests/Ordering/PaymentNotificationHost.cs` (yeni; D-828)
- `tests/Shopfolio.IntegrationTests/Ordering/MainFlowTests.cs`, `tests/Shopfolio.IntegrationTests/Payment/PaymentNotificationTests.cs` (ortak kurucuya geçti)
- Migration yok.

## Kabul Kriterleri Kontrolü ve Öz Denetim

Bütün maddeler (önceki işçinin yaptıkları dahil) diff'te yeniden denetlendi.

| # | Kriter / KK | Sonuç | Kanıt |
|---|---|---|---|
| 1 / KK1 | Dört anlatı sahte ödemeyle ayrı testlerde | ✓ | `TheMixedOrder_…_KK1_KK2_KK3_KK4_KK10`, `TheParcelWithdrawnInTransit_…_KK1_KK5_KK6_KK7_KK10`, `TheReturnGoods_ArrivingAfterTheIbanWasDeleted_…_KK1_KK8_KK10` (true/false), `TheGuestsEmailCorrection_…_KK1_KK9_KK10`; kart ödemesi `PayByCardAsync` (fake bildirim → uç → iş koşucusu) |
| 2 / KK2 | Ö1+S1 birlikte, dijital kalem o anda teslim, B-4; dijitalin iptal/cayma yolu kapalı | ✓ | MixedOrder adım 2: iz `O1,S1`, rehberin `DeliveryMarkedAt` = onay anı, B-4 = 1; sayfada `CancelDigitalItem`/`WithdrawDigitalItem` kapalı ve işlemin kendisi reddeder (`ConditionNotMet`, cayma reddi, kayıt yok) |
| 3 / KK3 | Kargodan önce lamba iptali: B-7, kart iadesi kendiliğinden, kargo ücreti dahil, Ö3 + B-8, kupon dönmez | ✓ | MixedOrder adım 3: B-7 = 1; sistemin kart iadesi (`ActorKind.System`, `IsShippingFeeIncluded`, tutar = lamba − kupon payı + kargo); `RunCardRefundAsync` → `O3`, Kısmen geri ödendi, B-8 = 1, F-4 = 1 (firma adresine, iade kaydına bağlı); `used_count` 1 kalır |
| 4 / KK4 | Hizmet tamamlanınca Teslim edildi; teslim tarihi yok, hak işarete kadar açık | ✓ | MixedOrder adım 4: 30 gün sonra `WithdrawServiceItem` açık; `MarkServiceCompletedAsync` → S10, Teslim edildi + Kısmen geri ödendi; işaretten sonra kapalı; lambada `DeliveryDate` yok |
| 5 / KK5 | Kargoda cayma: IBAN + iade adresi yazılır, kalem kapanır, B-9, F-3; durum Kargoya verildi, süre beyandan | ✓ | Withdrawals adım 1: `RefundIban.Iban`, `ReturnAddressLine`/`ReturnProvinceId`/`DeclarationDate`; B-9 = 1, F-3 = 1 (cayma kaydına bağlı); `ClosedUnitsAsync` = 1; eksenler Shipped + Paid; `WithdrawalRefundStartDay` = beyan günü |
| 6 / KK6 | "Teslim edilemedi" B-6; kapanışta iptal kaydı yok, ikinci geri ödeme yok, B-7 yok; teslim alma alt sınırsız, süreyi başlatmaz | ✓ | Withdrawals adım 2–3: B-6 = 1; S9, `cancellations` 0, `refunds` 0, B-7 0; beyandan 30 gün önceki ulaşma tarihi kabul; başlangıç günü yine beyan günü; stoğa ekleme +1 |
| 7 / KK7 | IBAN'a kargo ücretiyle geri ödeme; Ö4, B-8 | ✓ | Withdrawals adım 4: tutar = fiyat + kargo = sipariş toplamı, `O4`, Geri ödendi, B-8 = 1, IBAN silindi; iz `S5,S7,S9,O4` |
| 8 / KK8 | IBAN'sız iade: sistem kapatmaz; IBAN Z-42 sonrası "mal dönmedi" ya da Z-38 ile gider; ulaşan mal yeniden açar, B-14, süre durmaz | ✓ | Withdrawals Theory: firmanın kapanışı `ReturnShipmentPeriodDays + 1` gün sonra / `RefundIbanDeletionDay` `ScheduledDeletionAt` sonrası; iki kolda IBAN 0, bildirim yok, eksenler sabit; teslim alma kapanışı açar (`ReopenedAt`), `RefundIbanRequest` (`ReturnAfterIbanDeleted`), B-14 = 1; 5 gün sonra kalan gün 5 azalır; IBAN girilince geri ödeme `O4` |
| 9 / KK9 | E-posta düzeltmesi: yeni anahtar, yeni adrese B-1, eskiye B-15; durum/süre/kalem/IBAN sabit | ✓ | EmailCorrection: anahtar değişti, eski anahtar ve eski adresle bulunmaz; B-1 yeni, B-15 eski adrese tek; eksenler, ödeme penceresi, söz süresi, kalemler, IBAN sayısı, iz önceki gibi |
| 10 / KK10 | Her adımda iki eksen | ✓ | Her adım sonunda `AssertAxesAsync` (sipariş satırından); ana akış T329'da (`MainFlowTests`) |
| `09 §25` ilgili satırlar | Test yalıtımı, saat, tek kural evi | ✓ | Kendi koleksiyonu ve şablondan kopya veritabanı; gerçek saat yok (`ManualTimeProvider`); tutarlar sabitlerden ve kural evlerinden (`IAfterSalesModule.RefundAmountAsync`, `IsShippingFeeIncludedAsync`, `AfterSalesPeriodRules`) türetilir; sunucu kurulumu tek yerde (D-828) |
| Güvenlik taraması | — | ✓ | Üretim kodu değişmedi; ham SQL yalnız test tohumlamasında, sabit değerlerle; yeni uç yok |
| Devredilen backlog | D-828 (hedef T390) | ✓ | `PaymentNotificationHost` + üç sınıf; `DEFERRED_BACKLOG.d/T329.md`'de yerinde ✅ |
| sik-fail-nedenleri.md | okundu | ✓ | Integration TAM koşuldu (aşağıda); `dotnet format` test projesinde koşuldu; kopya kural/kurulum yok; main ilerlemedi (dal tabanı `29d3ce6f` = `origin/main`) |

## Test Sonuçları

| Koşu | Sonuç |
|---|---|
| `BranchFlowTests` | 5 / 5 ✓ |
| `BranchFlowTests` + `MainFlowTests` + `PaymentNotificationTests` (ortak kurucu sonrası) | 20 / 20 ✓ |
| Integration `Ordering` ad alanı | 573 / 573 ✓ |
| Integration `Model` (`--max-threads 3`) | 1404 / 1404 ✓ |
| Integration geri kalan (Model ve Ordering dışı) | 1409 / 1409 ✓ |
| Integration toplam (yerel, Postgres 18 `track_commit_timestamp=on`; ortak kurucu öncesi, kurucuyu kullanan üç sınıf sonra yeniden koşuldu) | 3386 / 3386 ✓ |
| Unit (Consistency dahil) | 5739 / 5739 ✓ |
| `dotnet build -warnaserror` (Integration projesi) | 0 uyarı / 0 hata |
| `dotnet format` (değişen test dosyaları) | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-10, head `d5838e2b`; `origin/main` `29d3ce6f` dal tabanı, birleştirme gerekmedi). Kriter 1–8 ve 10, KK1–KK8 ve KK10 ✓; kriter 9 / KK9 `~ Kısmi` (B1) → FAIL. Ana dal CI son 3 run yeşil (42712574041, 42712573916, 42691939512). Dal CI 42719494443 (head `d5838e2b`) koşuyordu (Lint, Build, Contract, E2E pass; Unit, Integration, Migration, Paketleme bekliyor). Yerelde: derleme 0 uyarı / 0 hata (`-warnaserror`), `dotnet format --verify-no-changes` üç proje temiz, Unit 5739/5739 (Consistency ve Architecture dahil), betik testleri 4/4, Integration `BranchFlowTests` + `MainFlowTests` + `PaymentNotificationTests` 20/20 (Postgres 18, `track_commit_timestamp=on`). `09 §25` test ve güvenlik satırları temiz; üretim kodu değişmedi. |
| Bulgu sayısı | 1 — **B1 DÜŞÜK S1** (`03 §2.10.5` adım 4; `02 §10.4.11` "Ne değişmez: müşterinin girdiği IBAN"; kriter 9, KK9): e-posta düzeltmesi testinde "IBAN değişmez" kanıtı boş. Sipariş misafirin havale siparişidir, düzeltme anında iptal/cayma yoktur, `refund_ibans` düzenekte boşaltılır; `ibansBefore` 0'dır ve `Assert.Equal(ibansBefore, …)` 0 = 0 karşılaştırır. Düzeltme müşterinin IBAN'ını silse ya da değiştirse test yine geçer. `OrderCorrectionTests` (T375) de IBAN'lı siparişte düzeltmeyi sınamıyor; kriter başka yerde de kilitli değil. Karşılaştırılan `BankTransferIban` firmanın havale IBAN'ıdır, müşterinin girdiği değil. Öneri: düzeltmeden önce müşterinin IBAN'lı bir iptali (kalem adedi 2, biri iptal) ya da beyanı yazılsın; düzeltmeden sonra IBAN satırı (kimlik, metin, silinme anı) aynı kalsın. |
| Bloke-etmeyen not (K) sayısı | 0 |
| Task satırları (`12 §7.5`) | Yok — kaynak birim `03 §2.10.2`–§2.10.5, KP yok; `12`'de task satırı yok (T329 emsali); `12_matris_denetle.py --kapsam 1-77` Hata 0. |
| Yapım raporu karşılaştırması | 1 uyuşmazlık: yapım raporu KK9'u "IBAN sayısı önceki gibi" kanıtıyla ✓ sayıyor; sayı 0 olduğundan kanıt boş (B1). Öz denetim tablosu kabul kriteri ve KK başına kanıtlı. Not: yapım raporu doğrulayıcının diff okumasında (Faz 1) dal diff'inin içinde göründü; karar kendi kanıtlarıyla verildi. |
| Düzeltme gerekli mi | Evet — B1 (yeni yapım turu; düzeltmeden sonra tur 2 doğrulaması) |

## Altyapı Değişiklikleri

- Yok (migration, paket, CI değişikliği yok).

## Commit & PR

- Dal `task/T390-uctan-uca-akislar`; PR base `main`. PR: #508

## Dış Varsayımlar

- Yok.

## Known Limitations / Follow-up

- Kart iadesi işleyicisi T405'in; testte işleyicinin yaptığı (`CardRefundJobRule.Decide` → `RefundRecord.Sending` → sahte `RequestRefundAsync` → `ApplyCardRefundOutcomeAsync`) adım adım taklit edilir.
- Havale cayma geri ödemesini yazan panel adımı sonraki task'ın; test satırı kural evlerinin verdiği tutarla yazar, işlemeyi `ProcessTransferRefundAsync` yapar.

## Notlar

⚠ Kolay konularda verilen kararlar:
1. F-3 ve F-4 bildirim satırı sipariş kimliği taşımaz (firma bildirimi; `CompanyNoticeQueue` `order_id` boş yazar, kayıt kimliğine bağlar) — test bu satırları ilgili kayıt kimliği ve firma adresiyle sayar.
2. Havale cayma iadesi satırı ve kart iadesi işleyicisi testte taklit edildi (yukarıda); üretimde karşılığı olan task'larda kendiliğinden yerini alır.
3. D-828 bu task'ta kapatıldı (hedefi T390'dı); ortak kurucu `Ordering` test ad alanında, `OrderingTestServices` yanında.
