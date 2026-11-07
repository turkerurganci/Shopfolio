# T304 — Alıcı başına bildirim gönderim sınırı

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-07

---

## Yapılan İşler

- `Notifications/RecipientSendLimit.cs`: saf kural — sınıra giren satırlar (B-1…B-16 + sipariş bağı), önceki satır sayımı (24 saat kayan pencere, `CreatedAt` sonra `Id` sırası, alıcı `TextNormalizer.NormalizeIdentity` ile harf ayrımsız), eşik 20, aşım yazımı (`Refuse`), muafiyet (`OriginalDispatchId`). Sabitler (20, 24 saat, 60 sn) yalnız bu dosyada.
- `Notifications/RecipientLimitCheck.cs`: ilk denemede bir kez sorulan sınır (`AskAsync`). Gönderen (T305/T385/T407) gövdeyi üretmeden önce sorar; `Refused` ise servise çağrı yapmaz. Aşan satır kendi işleminde `Undelivered` + `RecipientError` olur; kanal hâli, sipariş ve başka satıra dokunulmaz.
- `Notifications/RecipientLimitLog.cs`: seyreltilmiş, adressiz güvenlik satırı (`RecipientLimitExceededSampled`; ilk aşım bir satır, 60 sn pencerede sonrakiler sayılır, sayı pencereden sonraki ilk satırda `suppressedCount`).
- `Notifications/DispatchMarks.cs`: siparişin "e-posta ulaşmadı" işareti kayıttan okunur (grubun en son satırı `Undelivered`); aşan satır işareti düşürür.
- `LogEvents.RecipientLimitExceededSampled`; `09 §18.2` m.3 olay listesi ve `09 §3.5` süre/tavan tablosu satırı (08 §2.2.4'ün aynısı, yeni kural değil); DI kaydı `NotificationsModule.cs`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Notifications/{RecipientSendLimit,RecipientLimitCheck,RecipientLimitLog,DispatchMarks,NotificationsModule}.cs`
- `src/Shopfolio.Web/Infrastructure/Logging/LogEvents.cs`
- `Docs/09_CODING_GUIDELINES.md` (§3.5 satırı, §18.2 m.3), `Docs/DEFERRED_BACKLOG.md` (D-166)
- Testler: `tests/Shopfolio.UnitTests/Notifications/*`, `tests/Shopfolio.UnitTests/Consistency/RecipientSendLimitConsistencyTests.cs`, `tests/Shopfolio.IntegrationTests/Outages/RecipientSendLimitTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Yalnız B-1…B-16 sayılır ve sınırlanır | ✓ | `RecipientSendLimit.TakesPart`/`IsCustomerNotice`; `OnlyTheCustomerNoticesB1ToB16_TakePartInTheLimit`; `FirmAndAccountEmails_AreNotLimited_AndNotCounted` |
| 2 | Sayaç saklanmaz, kayıttan okunur | ✓ | `RecipientLimitCheck.AskAsync` (aday satırlar `dispatch_records`'tan); `TheCounter_IsReadFromTheRecord_…` (şema denetimi dahil) |
| 3 | Harf ayrımsız, yalnız siparişe bağlı | ✓ | `CountEarlier` + `OrderId != null` sorgusu; `AddressesThatDifferInCaseSpaceOrTheTurkishI_…`, `AddressesThatDifferInCase_AreOneRecipient_EvenOnTwoOrders` |
| 4 | Eşik 20; satır kendisi ve sonrakiler sayılmaz; `CreatedAt`, eşitlikte `Id` | ✓ | `TheTwentiethRow_Passes_AndTheTwentyFirst_IsOver`, `TheRowItself_AndTheRowsMadeAfterIt_AreNotCounted`, `RowsMadeAtTheSameMoment_AreOrderedById_…`, `OfTwentyOneRowsMadeAtTheSameMoment_…` |
| 5 | Aşım satırı sayılır; pencere kaydıkça düşer | ✓ | `ARefusedRow_IsCounted_AndTheCountFallsWhenTheWindowSlides`, `ARowOlderThanTheWindow_FallsOut_…` |
| 6 | Aşan satır ilk denemede `Undelivered`+`RecipientError`; çağrı yok; kanal hâli aynı | ✓ | `RecipientSendLimit.Refuse`; `AFreshRowOverTheLimit_IsUndelivered_…` (sahte servis `Calls` boş, kanal hâli ve sipariş JSON'u aynı) |
| 7 | Sipariş/ödeme/geçiş etkilenmez; "ulaşmadı" işareti düşer | ✓ | aynı test (sipariş satırı aynı; `DispatchMarks.HasMarkAsync` true/false); `DispatchMarksTests` |
| 8 | Yeniden gönderim ve düzeltme sonrası B-1 muaf ama sayılır | ✓ | `IsExempt`; `AResending_IsExempt_GoesToTheService_AndIsCountedByTheLaterRows`, `TheResendingCounts_…` |
| 9 | İlk denemede bir kez sorulur; seyreltilmiş adressiz kayıt | ✓ | `AskAsync` ilk kapı (`AttemptCount == 0`, `LastOutcome` boş); `ARetry_DoesNotAskTheLimitAgain`; `TheSecurityLine_IsWrittenOnTheFirstRefusal_ThenCounted_AndCarriesNoAddress`; `RecipientLimitLogTests` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: F ve hesap e-postası sayaca girmez | ✓ | `FirmAndAccountEmails_AreNotLimited_AndNotCounted` (19 B + F1 + PasswordResetLink + 5 F5 → 20. B geçer) |
| KK2: sayaç tabloda saklı değil | ✓ | `TheCounter_IsReadFromTheRecord_…` (satır silinince cevap değişir; `limit`/`counter` adlı tablo/sütun yok) |
| KK3: büyük/küçük harf aynı | ✓ | `AddressesThatDifferInCase_AreOneRecipient_EvenOnTwoOrders` (`ALİ@EXAMPLE.COM` ile 20 satır → aşım) |
| KK4: 21. aşar, 20. geçer; eşit anda `Id` | ✓ | `OfTwentyOneRowsMadeAtTheSameMoment_…` (sorulma sırasından bağımsız), `TheIdOrder_IsTheDatabasesByteOrder_…` |
| KK5: aşım satırı sayıya girer; pencere kayınca düşer | ✓ | `ARefusedRow_IsCounted_…`, `WhenTheWindowSlides_…` |
| KK6: ilk denemede Undelivered+RecipientError; çağrı yok; kanal hâli aynı | ✓ | `AFreshRowOverTheLimit_IsUndelivered_…`, `ARowUnderTheLimit_GoesToTheService_AfterTheAsk` (karşı örnek) |
| KK7: sipariş durumu değişmez, işaret düşer | ✓ | `AFreshRowOverTheLimit_IsUndelivered_…`, `DispatchMarksTests` |
| KK8: yeniden gönderim geçer ve sayılır | ✓ | `AResending_IsExempt_…` (Sent olur, sonraki aşar) |
| KK9: yeniden denemede sorulmaz; kayıtta adres yok, sonraki aşımlar sayı | ✓ | `ARetry_DoesNotAskTheLimitAgain`, `TheSecurityLine_…` (`example.com` satırda yok, `suppressedCount` 2) |
| `09 §25` ilgili satırlar | ✓ | sabitler tek yerde ve 08/09 ile eşit (`RecipientSendLimitConsistencyTests`); olay adı kapalı listede (`LoggingConsistencyTests`); `internal` türler (`ModuleBoundaryTests`); olay adı `LogEvents` sabitiyle (kaynak taraması); zaman `TimeProvider`; işlem `ITransactionRunner` içinde, dış çağrı yok |
| Sık FAIL (1) istisna geniş | ✓ | istisna tek koşul: `OriginalDispatchId != null`; başka muafiyet yok |
| Sık FAIL (2) çağrı biçimi | ✓ | yeni kaynak taraması yok; olay adı çağrısı `LogEvents.RecipientLimitExceededSampled` |
| Sık FAIL (3) DB default / FK dizini | ✓ | şema değişmedi; mevcut `(order_id, created_at)` ve `(created_at)` dizinleri sorguyu karşılıyor |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | 20 / 24 saat / 60 sn yalnız `RecipientSendLimit`'te; 08 §2.2.4 ve 09 §3.5 satırı testle eşit; 09 §18.2 listesi ve `LogEvents` eşit |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ | 09'a eklenenler 08 §2.2.4'ün aktarımıdır; 60 sn pencere `K-1570` (tek tür pencere) ve `SignatureFailureLog` kalıbı — ⚠ A1 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency hariç) | ✓ 2759 | `dotnet test --project tests/Shopfolio.UnitTests -c Release --filter-not-trait "Consistency=true" --max-threads 3` | 0 hata (T304: 37 yeni birim testi) |
| Consistency | ✓ 209 | `… --filter-trait "Consistency=true"` | 0 hata (2 yeni) |
| Integration (yeni) | ✓ 11 | `--filter-class Shopfolio.IntegrationTests.Outages.RecipientSendLimitTests` | gerçek PostgreSQL 18 |
| Integration (komşu) | ✓ 19 + 30 | `Outages` ad alanı; `DispatchAndChannelStateTablesTests` | 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — (doğrulama ayrı) |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T304-bildirim-siniri`
- **Commit:** `365a29d` — T304: Alıcı başına bildirim gönderim sınırı
- **PR:** #421
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Tetikleyici tarama (plan tier, kota, rate limit, paket sürümü…) 11'deki T304 bloğunda "rate limit" sözcüğünü yalnız ürünün kendi kuralı olarak geçirir; dış servise çağrı, yeni paket ya da ortam gerekmez. Yerel PostgreSQL 18 (`postgres:18`, CI ile aynı imaj) `shopfolio-t304-pg` konteynerinde, bitince silinir.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Yok | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gönderen (kuyruktaki satırı alan, gövdeyi üreten, servisi çağıran) bu task'ta yok; `RecipientLimitCheck.AskAsync` sahibinin çağrısı bekliyor | D-166 | Gönderen T305/T385/T407'dir (D-97); T304'ün bağımlılığı yok |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 42344353073 success, 42344353123 success, 42343289321 success.
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ A1 (kolay, kurala en uygun seçenekle karar verildi):** (a) seyreltilmiş satır penceresi 08'de sayı vermez; `SignatureFailureLog` kalıbı ve K-1570 ("tek tür pencere") ile 60 sn alındı, adres tutulmadığı için pencere ürün geneli (adres başına değil). (b) Aşan satırın `AttemptCount`'u 1 yazılır ("ilk denemede"; her deneme sayacı artırır, 06 §4.58); servis çağrısı yoktur. (c) Pencerenin alt sınırı dahildir (`CreatedAt >= satır − 24 saat`). (d) Kayıtta alıcı normalleştirilmiş sütun olmadığı için (06 §4.58) 24 saatlik pencerenin sipariş bağlı satırları okunur, alıcı bellekte `NormalizeIdentity` ile karşılaştırılır; `(created_at)` dizini kullanılır — hacim büyürse normalleştirilmiş sütun bir 06 değişikliği ister.
- **Id sırası:** PostgreSQL `uuid` bayt sırasıyla (büyük uçlu) karşılaştırılır; `UuidOrder` bunu bellekte aynen yapar ve testle sabitlenmiştir.
