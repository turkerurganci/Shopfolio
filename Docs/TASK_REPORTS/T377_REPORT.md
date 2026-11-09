# T377 — Limit ilkeleri: iki eksen, ödenmemiş sipariş sayımı, yönetici muafiyeti yok, tanınan tarayıcı işareti

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-09

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Mekanizma T342/T376'dan hazırdı (`PasswordRegime` iki eksen ve tanınan tarayıcı sayacı, `SessionStore` işaret kayıtları, `AttemptCounter`, `Coupons.ApplyAsync` sepet ekseni, `OpenUnpaidOrderLimit`, yönetici aynı koddan). T377 eksikleri kapattı:
  - **L-7 sayımı** `UnpaidCancellationLimit`: `TransitionRecords` S3 + sistem aktörü + sebep yalnız `PaymentWindowExpired` ve `CardPaymentFailed` (`CountedReasons`); kayan pencere (P-37 saat); IP ve e-posta ekseni ayrı.
  - **Ortak eksen süzgeci** `OrderAxes` (IP /64 ve IPv4-mapped; e-posta yalnız üye siparişi); `OpenUnpaidOrderLimit` da bunu kullanır.
  - **Tek kapı** `OrderPlacementLimits.FindExceededAsync`: L-7 her ödeme yöntemini, L-8 yalnız havaleyi kapatır; hesap türü almaz (muafiyet yok). `OrderConfirmation` kapıyı önceki aynı-sepet siparişi iptal edildikten ve ödeme yöntemi denetiminden sonra çağırır; yeni `OrderConfirmationRefusal.AttemptLimitExceeded`, `ExceededLimit`. `OrderConfirmationRequest.IpAddress` null olamaz (D-783).
  - **Tanınan tarayıcı çerezi** `BrowserCookies.WriteKnownDevice` / `ReadKnownDevice` (`__Host-known-device`).
- `09 §15.4` madde 5 güncellendi (L-7/L-8 sınıfları, tek kapı, muafiyet yok). Belge kuralı değişmedi, karar kaydı yok.
- Main birleştirmesi: `origin/main` (T367 dahil) dala birleştirildi, çakışma yok.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/` (`OrderAxes`, `UnpaidCancellationLimit`, `OrderPlacementLimits`, `OrderConfirmation`, `OpenUnpaidOrderLimit`, `OrderingModule`), `Infrastructure/Security/BrowserCookies.cs`
- Testler: `Ordering/OrderPlacementLimitTests`, `Ordering/OrderConfirmationTests`, `Ordering/CartTests`, `Model/MembershipSignInAxesTests`, `Security/AttemptCounterTests`, `Infrastructure/KnownDeviceCookieTests`, `Architecture/SignInLimitSourceScanTests`, `Architecture/AttemptNumberScanTests`, `Consistency/AttemptLimitInventoryTests.Surfaces`
- Belgeler: `09 §15.4` m.5, `DEFERRED_BACKLOG.d/T376.md` (D-781/782/783 ✅), `DEFERRED_BACKLOG.d/T377.md` (D-805–809), `STATUS_CHANGELOG.d/T377.md`, `IMPLEMENTATION_STATUS.md` satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki eksen ayrı sayılır | ✓ | `MembershipSignInAxesTests` `TheIpAxisStopsAlone...`, `TheEmailAxisStopsAlone...` (L-1); `OrderPlacementLimitTests.TheTwoAxesAreSeparate...` (L-7/L-8) |
| 2 | Kupon denemesi sepet ekseninde | ✓ | `CartTests.TheCouponCounter_IsTheCartsOwn...T377_KK2` |
| 3 | L-5 e-posta ekseni sorgudaki adresi sayar | ✓ | `AttemptCounterTests.The_guest_lookups_email_axis_counts_the_typed_address...T377_KK3` |
| 4 | L-7 yalnız süresi dolan ve kart başarısız; dört istisna sayılmaz | ✓ | `OrderPlacementLimitTests.OnlyTheWindowThatRanOut...KK4`, `AnyNumberOfTheLeftOutCancellations...KK4` (teori: dışarıda kalan her sebep); `OrderConfirmationTests.ARepeatedConfirmationFromTheSameCart...KK4` |
| 5 | L-8 yalnız havale; e-posta ekseni yalnız üye | ✓ | `OrderPlacementLimitTests.AFullOpenCeiling_ClosesTheBankTransferOnly...KK5`, `L7_/L8_EmailAxisCountsOnlyAMembersOrders...KK5`; `OrderConfirmationTests.AFullOpenCeiling_StopsTheNextBankTransfer...KK5` |
| 6 | Yönetici muafiyeti yok | ✓ | `MembershipSignInAxesTests.TheAdministrator_IsBlocked...KK6`; `OrderPlacementLimitTests.TheGateAsksForNoAccountType...KK6` |
| 7 | Sepet kalemi ve stok yoklama için limit yok | ✓ | `AttemptLimitInventoryTests.Surfaces` üç test (`NoRowOfTheDocumentsTable...`, `NoLimitOfTheInventory...`, `NoAttemptConstant...`) |
| 8 | Çerez: hesap için zorunlu, birinci taraf, kişisel veri yok | ✓ | `KnownDeviceCookieTests` (dört test) |
| 9 | L-1 e-posta ekseni yalnız işaretsizi sayar; işaretli kendi sayacında, IP aynen | ✓ | `ARecognizedBrowser_IsCountedOnItsMark...` (T342), `MembershipSignInAxesTests.AForgedMark...`, `AMarkedBrowser_HasItsOwnCount...KK9` |
| 10 | Sıfırlama işaret verir; Google L-1'e girmez; ömür 90 gün | ✓ | `TheReset_TakesTheLinkOnce...` (T342), `SignInLimitSourceScanTests`, `MembershipSignInAxesTests.TheMark_IsRecognisedUntilNinetyDays...KK10`, `SignIn_OpensTheLongSession...` (MarkLifetime) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–10 (yukarıdaki tablo) | ✓ | yukarıdaki tablo |
| KK1 iki eksen bağımsız | ✓ | `MembershipSignInAxesTests`, `OrderPlacementLimitTests.TheTwoAxesAreSeparate...` |
| KK2 kupon sayacı sepette | ✓ | `CartTests...T377_KK2` |
| KK3 sorgudaki e-posta sayılır | ✓ | `AttemptCounterTests...T377_KK3` |
| KK4 dört istisna L-7'ye girmez | ✓ | müşteri iptali, firma iptali, aynı sepetten yeni sipariş, sağlayıcıya bağlanılamayan: `CountedReasons` tam iki sebep (`OrderPlacementLimitTests`), `AnyNumberOfTheLeftOutCancellations...` her dışarıdaki sebep için; aynı sepet: `OrderConfirmationTests.ARepeatedConfirmation...` (Max+1 tekrar, ret yok, sayaç 0) |
| KK5 L-8 yalnız havale, misafir e-postada sayılmaz | ✓ | `OrderPlacementLimitTests`, `OrderConfirmationTests.AFullOpenCeiling...` |
| KK6 yöneticide muafiyet yok | ✓ | `MembershipSignInAxesTests...KK6`; kapı hesap türü almaz |
| KK7 sepet ve stok için limit yok | ✓ | `AttemptLimitInventoryTests.Surfaces` |
| KK8 işaret çerezinde kişisel veri yok | ✓ | `KnownDeviceCookieTests` |
| KK9 işaretsiz, uydurma, başka hesabın işareti; işaretli ayrı | ✓ | `MembershipSignInAxesTests.AForgedMark...`, `AMarkedBrowser...` |
| KK10 sıfırlama, Google, ömür | ✓ | yukarıda |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | sayı taraması (`AttemptNumberScanTests` yeni sayaç dosyalarını kapsar); `SignInLimitSourceScanTests` (ihlal örnekli: boşluklu, çok satırlı, `using static`, takma ad, `Enum.Parse`); sayaç dosyalarında eşik/pencere sabiti yok |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `SignInLimitSourceScanTests` istisnası dosya listesi (6 sahip dosya) ve yalnız L-1; sahip dosyaların varlığı sınanır |
| Sık FAIL 2: tarama bir çağrı biçimini kaçırıyor | ✓ | yedi ihlal biçimi örnekli (nitelikli, boşluklu, `\n` ve `\r\n` bölünmüş, `using static`, takma ad, `Enum.Parse`) |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ n/a | şema değişikliği yok |
| Sık FAIL 4: kural değişince aynı sayıyı anan yerler | ✓ | `grep L-7` 09 ve README: tek yer `09 §15.4` m.5 güncellendi; sayılar `ProductConstants`'ta, kopya yok |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09` metni yalnız sınıf adlarını ekler; 02 kuralı değişmedi |
| Kayıt: D-781/782/783 (hedef T377) kapatıldı; devredilen kalem taraması | ✓ | `DEFERRED_BACKLOG.d/T376.md` ✅; hedefi T377 olan D-572 (sayfa düzeyi engel testi) sayfalar olmadığı için T515, T516'ya yeniden hedeflendi |
| Modül kurucusuna yeni bağımlılık, elle kuran düzenekler | ✓ | Integration TAM koşuldu (aşağıda), düzenekler yeşil |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build tests/... --no-restore -warnaserror` (iki test projesi) | 0 uyarı, 0 hata |
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 5515/5515 |
| Integration (Ordering) | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Ordering` | 446/446 |
| Integration (Ordering dışı) | ✓ | `--filter-not-namespace ...Ordering` (yeni KK3 testinden önce) | 2732/2732; ardından `AttemptCounterTests` 16/16 |
| Format | ✓ | `dotnet format --verify-no-changes --include <değişenler>` (üç çağrı) | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T377-limit-ilkeleri`
- **PR:** #497
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Adressiz isteğin ödeme adımı sayfasında ele alınması | D-805 | Sayfa görevi (T509) |
| 2 | Çerezin giriş, sıfırlama, değiştirme sayfalarında yazılıp okunması | D-806 | Sayfa görevleri (T515, T516, T517, T520) |
| 3 | L-7/L-8 reddinin nötr mesajlı uç yanıtı; `12` KP-72 `A354`–`A356`, `A358`–`A361` uç yanıtı kanıtı | D-807 | Sayfa görevi (T509); bu satırların Durum hücresi BEKLEMEDE kaldı |
| 4 | Üye e-posta ekseni iletişim e-postasıyla | D-808 | ⚠ karar, F3 gate |
| 5 | L-7 `>=` eşiği | D-809 | ⚠ karar, F3 gate |

## Notlar

- **⚠ kararlar (kurala en uygun seçenekle verildi):** (1) L-7 dolu = `>= Max` (altıncı onay reddedilir), L-8 ile aynı okuma; (2) L-8 sayımı kart ve havale Bekliyor siparişini sayar, yalnız havaleyi kapatır (D-781); (3) üyenin e-posta ekseni siparişin iletişim e-postasıyla sayılır; (4) `IpAddress` null olamaz.
- **Working tree (Adım -1):** temiz; devam işçisi `995ff4b2`'den başladı.
- **`12` Durum hücreleri:** PASS vermek doğrulayıcının işi; yapım BEKLEMEDE bıraktı. Modül düzeyi kanıtı yukarıdaki testlerde.
