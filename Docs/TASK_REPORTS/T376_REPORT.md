# T376 — Deneme limitleri: dokuz limit, kayan pencere, hesap kilitlenmez, nötr mesaj

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- Mekanizma T215/T326/T342'den hazırdı (`AttemptCounter`, `AttemptLimits` L-1…L-6 ve L-9, `NeutralMessages`, yeniden doğrulama sayımı). T376 eksikleri kapattı:
- `AttemptLimitInventory`: `02 §8.2` tablosunun dokuz satırı (ad, eksen, sayım türü: başarısızlıkta / her istekte / siparişlerden). `AttemptLimits.OrderCountedOf`: L-7 ve L-8 sabitlerden (L-8 pencere taşımaz).
- `OpenUnpaidOrderLimit` (Ordering): L-8 sayımı; ödemesi Bekliyor olan açık sipariş, pencere yok, e-posta ekseni yalnız üye siparişi, IP ekseni /64 (IPv4-mapped IPv4 sayılır), tavan `ProductConstants.OpenUnpaidOrdersMax`.
- `Carts.MergeAsync`: girişte birleşen sepette L-6 sayacı iki sayacın büyüğünü üye sepete taşır (D-561 ✅).
- `FormResults.AttemptLimitExceeded(page, code)`: red satırını limit koduyla yazar; kod sekiz limit kodundan biri olmalı (D-145 ✅; `09 §8.2` m.3 çağıran listesi güncellendi).
- Kaynak taraması `AttemptNumberScan`: sayaç, tablo, envanter, kayıt ve L-8 sayımı dosyalarında 0 ve 1 dışında sayı yazılamaz (onaltılık, ikili, alt çizgili ve sonekli biçimler dahil).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/{AttemptLimitInventory,AttemptLimits,ClientAddress}.cs`, `Infrastructure/Forms/FormResults.cs`
- `src/Shopfolio.Web/Modules/Ordering/{OpenUnpaidOrderLimit,Carts,OrderingModule}.cs`
- Testler: Unit `Architecture/AttemptNumberScan(+Tests)`, `Consistency/AttemptLimitInventoryTests`, `RateLimiting/{NeutralMessageTests,AttemptLimitResultTests}`; Integration `Ordering/{OpenUnpaidOrderLimitTests,CartTests}`
- Docs: `09 §8.2` m.3, backlog (D-145, D-561 kapandı; D-108 yeniden hedef; D-780–D-783 yeni)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dokuz limit | ✓ | `AttemptLimitInventoryTests.TheTableHasNineRows_..._KK1_KK9` |
| 2 | Değerler `ProductConstants`'tan | ✓ | `AttemptNumberScanTests` + `AttemptLimitInventoryTests.EveryLimitsParameterRows_..._KK2_KK9` |
| 3 | Kayan pencere | ✓ | `AttemptCounterTests.The_window_slides_with_every_attempt_..._T326_KK8`, `A_row_exactly_one_window_old_...` |
| 4 | Hesap kilitlenmez | ✓ | `MembershipSessionTests.SignIn_IsBlockedAfterFiveFailures_EvenWithTheRightPassword_AndTheAccountIsNotLocked_KK9` |
| 5 | Nötr mesaj | ✓ | `NeutralMessageTests` (üç test), `AttemptLimitResultTests`, `AttemptLimitPatternTests` |
| 6 | L-1 yeniden doğrulamayı sayar | ✓ | `MembershipSessionTests.AWrongPasswordAtTheReauthentication_CountsToL1_AndTheSixthIsBlocked_KK6` |
| 7 | L-6 sepet başına, birleşmede büyük sayaç | ✓ | `CartTests.TheSignInMerge_CarriesTheLargerInvalidCouponCountUnderTheMembersCart_NotTheSum_T376_KK7`, `AttemptCounterTests.Merging_carts_*` |
| 8 | L-8 pencere yok, boşalır | ✓ | `OpenUnpaidOrderLimitTests` (`ThereIsNoWindow_...`, `AnOpenOrderThatIsPaidOrCancelled_LeavesTheCount_..._KK8`), `AttemptLimitInventoryTests.L8HasNoWindow_...` |
| 9 | Envanter tabloyla eşit | ✓ | `AttemptLimitInventoryTests` (KK1_KK9 testleri) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 dokuz limit adı ve kapsamı | ✓ | `AttemptLimitInventoryTests.EveryLimitsAxes_...`, `TheTableHasNineRows_...` |
| KK2 sayı yalnız `ProductConstants`'ta | ✓ | `AttemptNumberScanTests` (ihlal örnekleri: tamsayı, `FromMinutes(15)`, adlı argüman, ondalık, 0x/0b/alt çizgi/L soneki) |
| KK3 sahte saatle kayan pencere | ✓ | `AttemptCounterTests` (FakeTimeProvider) |
| KK4 engel yalnız işleme | ✓ | `MembershipSessionTests` KK9 testi |
| KK5 mesajda sayı, süre, eksen yok | ✓ | `NeutralMessageTests` |
| KK6 yeniden doğrulama sayılır | ✓ | `MembershipSessionTests` KK6 testi |
| KK7 birleşen sepette büyük sayaç | ✓ | `CartTests` T376_KK7 (Theory, 3 satır) |
| KK8 L-8 pencere yok, boşalır | ✓ | `OpenUnpaidOrderLimitTests` 7 test |
| KK9 envanter tabloyla eşit | ✓ | `AttemptLimitInventoryTests` |
| Sık FAIL 1: tarama istisnası geniş | ✓ | Tarama yalnız 0 ve 1'e izin verir; yorum ve metin dışı hiçbir biçim geçmez |
| Sık FAIL 2: çağrı biçimi kaçırma | ✓ | Sayı biçimleri (onaltılık, ikili, alt çizgi, sonek) ihlal örnekli |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ | Şema değişmedi |
| Sık FAIL 4: aynı sayıyı anan yerler | ✓ | "beş çağıran" → "altı çağıran" (`09 §8.2` m.3); başka yer taranıp bulunmadı |
| Sık FAIL 5: belge kuralı değişikliği karar kaydısız | ✓ | Yalnız D-145'in öngördüğü çağıran eklendi; kural değişmedi |
| Devredilen backlog (D-145, D-561, D-108, D-565) | ✓ | D-145 ve D-561 ✅; D-108 F3 gate'e yeniden hedeflendi (⚠); D-565 L-5 kısmı T511'de |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | 5446/5446 (tarama örnekleri sonrası `AttemptNumberScanTests` 14/14) |
| Integration | ✓ | `--filter-class` CartTests/OpenUnpaidOrderLimitTests/AttemptLimitPatternTests; `--filter-namespace` Ordering+Security; Model+Web+Pages | 35/35; 433/433; 1987/1987 |
| Lint | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <değişenler>` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T376-deneme-limitleri`
- **PR:** #493
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | L-4 ve L-5 için tekrar kullanılır çok eksenli kapı; çağıranlar sayfalar | D-780 | Sayfalar T365/T506/T511'de |
| 2 | L-8 kart ve havale Bekliyor'u sayar; T377 yalnız havaleyi kapatır | D-781 | T377 kriter 5 |
| 3 | L-7 sayımı (kendiliğinden iptal) | D-782 | `11` kapsamı T377'ye bırakır |
| 4 | Sipariş onayında null adres ile imha kısıtı | D-783 | T348 alanı; T376 yolu denemedi |

## Notlar

- **⚠ D-108 (bellek ekseni tavanı):** T376'nın sayaçları veritabanındadır, bellekte sayaç kurmadı; `SignatureFailureLog` tavanı karar kaydı ister (sahip kararı). Kayıt F3 gate'e yeniden hedeflendi.
- **Repo memory (bitiş kapısı 9):** parça dosyaları kuralı gereği MEMORY'ye dokunulmadı.
