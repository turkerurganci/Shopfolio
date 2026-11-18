# T604 (A) — F5 kapısından kalan test aracı kalemleri

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (dilimler A1 ve A2 bitti; A3…A7 payları sonraki dilimlerde) | **Tarih:** 2026-11-18

---

## Dilim A1 — iade, ödeme, sipariş testleri

Yalnız test ve belge dosyaları değişti; `src` değişikliği yok (`src` isteyen iki kalem U parçasına daraltıldı: IBAN alanı ve iade ekranı kaynağı U'nun).

### Yapılan işler

İlk işçi (A1 ilk yarı):

- D-1676: `PayTrPaymentGatewayTests.Query.cs` yasak değer ve yanıttaki net tutar "9,7631".
- D-1725: `ScriptlessFlowTests.FillCheckoutAsync` yanıltıcı fatura değerlerini betikli yolda da gönderir.
- D-1368: `OrderOperationScreenTests` üye kökü: başka hesap 404, oturumsuz 302/303 giriş, kapalı işlem 303.
- D-1284 ve D-1283 (1): `CustomerOperationsTests` itemless alan süzgeci ve CardRefundNotRealized olgusu.
- D-1424: `DataExportPageTests` erişim, `SalesSummaryPageTests` sayfa düzeyi ciro/sayım.
- D-697: `SchedulerScanTests` VAL-C109 ve VAL-A827 payları.
- D-981 (C090, C091, C080, C002): `CardRefundJobTests`, `PaymentNotificationTests`.

Bu devam:

- D-981 `VAL-C088`: `PaymentWindowTests.UnansweredFinalQuery_KeepsTheReservations_ShowsTheMarkOnThePanelRows_AndTheMarkGoesWithEitherAnswer_VALC088_D981` — yanıtsız sorguda Alındı + Bekliyor, ayırma sürer, iki panel liste satırında işaret; başarı yanıtında Ö1, `004` yanıtında S3 + Ö2, iki satırda işaret kalkar. D-981 kapandı.
- D-1167 (KP-76 beş KRİTİK satır): `VAL-A371`, `A372` → `ProductFormPageTests.TwoAdministratorsOnTheSameProduct_...` (E-33) ve `ContentPanelPageTests.TwoAdministratorsOnTheSameContentRecord_...` (E-41); `VAL-C076`, `C077`, `C078` → yeni `Files/ConcurrentOrderScreenTests` (E-37; 6 test: iki yönetici; yönetici önce kargo/müşteri iptali geçersiz; müşteri önce iptal/yönetici işareti geçersiz; yönetici önce ödendi/müşterinin eski iptal formu sebepli reddedilir; sistem önce süre iptali/yönetici işareti 409; yönetici önce ödendi/sistem süre iptali uygulanmaz). Geçersizleştirmeyen sistem çifti `PaymentNotificationTests...LateSuccess_OnACancelledOrder_..._VALC002`. D-1167 kapandı; `VAL-A371`'in tarayıcı gözlemi payı D-1753 (yeni).
- D-1494 (`VAL-B009`…`B011`): `ScriptlessFlowTests.ThePaymentScreens_ShowTheIban_TheReceivedSentenceAndTheAwaitedCard_InBothWays_VALB009_VALB010_VALB011` (`BrowserSeed.NewOrderAsync` kart seçeneği aldı). Üç motor, iki yol yeşil. D-1494 kapandı.
- D-1725 KK3: `TheSameAsDeliveryBox_DecidesTheBillingAddress_InBothWays_VALB107` firefox ve webkit'te de iki yolda yeşil (chromium önceki dilimde).
- D-1283 (2) ve D-1599: kod değiştirdikleri için T604 U'ya daraltıldı (aşağıda ⚠).

### Notlar

- Working tree: temiz. `core.hooksPath` = `scripts/git-hooks`.
- Dış varsayım: yok (yalnız test kodu; PostgreSQL 18, `track_commit_timestamp=on`).
- Ana dal CI başlangıç denetimi yardımcının işidir (yönetici düzeni); işçi dal CI'ını yoklamaz.
- Integration: **TAM: CI**. Koşulan sınıflar: `ProductFormPageTests`, `ContentPanelPageTests`, `PaymentWindowTests`, `ConcurrentOrderScreenTests` (tek çağrı 92/92); önceki dilimden OrderOperationScreenTests, CustomerOperationsTests, DataExportPageTests, SalesSummaryPageTests, SchedulerScanTests, CardRefundJobTests, PaymentNotificationTests (184/184). Tarayıcı: `VALB009` ve `VALB107` chromium, firefox, webkit (motor başına ayrı çağrı).
- Unit TAM: 8088/8088. `dotnet format` değişen `.cs` dosyalarında temiz.

## Dilim A2 — kimlik, oturum, davet testleri

Yalnız test ve belge dosyaları değişti; `src` değişikliği yok (değişinim denemeleri için geçici bozuldu, `git checkout -- src` ile geri alındı). Kalemler: D-464, D-572, D-801, D-1289, D-1301, D-1374, D-1389, D-1394, D-1405, D-1723.

### Yapılan işler (A2)

- D-1394 ve D-464 (`AdminAccountsPageTests`): davet sayfasında (`/panel/davet/{anahtar}`) dört geçersizleşme anı — geri çekilmiş, aynı adrese yenisiyle geçersizleşmiş, göndereni kaldırılmış, adresi yönetici olmuş (K-1085); her birinde sayfa geçersiz cümlesini formsuz söyler, kabul POST'u 200 geçersiz sayfa verir ve hesap açmaz. KK8 satır denetimi: kaldırılan yöneticinin bağlantı satırları silinir, yalnız onun daveti geçersizleşir, başkasının daveti açık kalır.
- D-572, D-801, D-1405 (`CustomerSignInPageTests`, `RegistrationPageTests`): L-1 429'da `Retry-After` yok; tanınan tarayıcı e-posta ekseni engelliyken girer, işaretsiz tarayıcı engelli kalır; deneme satırları pencere dışına alınınca doğru şifre 303 alır ve `customers` tablosunda kilit/engel sütunu yoktur; kayıtta aynı pencere dolma sınaması; gerçek misafir sepetiyle `/giris` birleşmesi (çerez silinir, eklenen satır adı bir kez söylenir); `/kayit` Google yüzü (tanımsızken düğme yok ve 422, tanımlı ve kapı açıkken düğme ve 303 → Google, kapı kapalıyken düğme yok) ve `SocialLoginEmailUnverified` cümlesi.
- D-1289 (`PanelAccessPagesTests`): yaygın şifre sıfırlamada ve davet kabulünde 422 (bağlantı tüketilmez); sıfırlama tanınan tarayıcı çerezini yazar ve oturum açmaz; L-1'de engellenen deneme giriş kaydına da yazılır, hesap kilitlenmez, pencere dolunca girer; yönetici çıkışı vitrin oturumuna ve çerezine dokunmaz (K-899); `VAL-A306` (müşteri şifresi panele girmez, vitrin oturumu paneli açmaz) ve `VAL-A308` (müşteri adresine giden davet ayrı yönetici hesabı açar, müşteri satırı değişmez).
- D-1301 (`PanelHomeAndVerificationPagesTests`): iki eşzamanlı tüketim — iki basış hesap satır kilidinde birlikte bekletilip sırayla bırakılır; yeni adres doğrulaması ve geri alma için ikişer test; bir 303, bir geçersiz sayfa, tek tüketim, tek iz satırı.
- D-1723 (`Browser/ScriptlessFlowTests.TheLinkScreens_SendTheirButtons_…_VALB110_D1723`): E-21 yeni e-posta ve Google teyidi, E-28 geri alma, E-54 doğrulama ve geri alma düğmeleri gönderilir; yeniden açılışta düğme yok. chromium, firefox, webkit ayrı çağrılarla, her biri betikli ve betiksiz: 2/2 yeşil (üç çağrı). `BrowserSeed.LinkIdOfAsync` eklendi.
- D-1374, D-1389: `src` gerektirdiği için T604 U'ya daraltıldı (karar önerisi satırda; ⚠ aşağıda).

### Notlar (A2)

- Integration: **TAM: CI**. Koşulan sınıflar: `AdminAccountsPageTests`, `CustomerSignInPageTests`, `RegistrationPageTests`, `PanelAccessPagesTests`, `PanelHomeAndVerificationPagesTests` (iki çağrıda 90/90 ve 50/50); tarayıcı `…_VALB110_D1723` üç motorda 2/2.
- Unit TAM: 8088/8088. `dotnet format` değişen yedi `.cs` dosyasında temiz.
- Docker: Postgres 18 `track_commit_timestamp=on`, konteyner `shopfolio-t604a-pg`, port 56604 (iş bitince silindi).

## Para ve güvenlik kalemleri (KK2)

Bu dilimin kalemleri yalnız test ekler; ürün kodu değişmediği için "değişiklikten önce kırmızı" yerine her testin **ayırt ediciliği** değişinimle gösterilir (src geçici bozuldu, test kırıldı, geri alındı; `git checkout src`) ve var olan testler değişmeden geçer. Çıkış yolu ve geri alma: test dosyalarının PR'ı geri alınır.

| Kalem | Test | Değişinim → sonuç |
|---|---|---|
| D-1167 `VAL-A371/A372` (E-33) | `ProductFormPageTests.TwoAdministratorsOn...` | `RecordSave` sürüm karşılaştırması kapalı → KIRMIZI |
| D-1167 `VAL-A371/A372` (E-41) | `ContentPanelPageTests.TwoAdministratorsOn...` | `ContentPublishing.SaveEditAsync` ve `RecordSave` karşılaştırması kapalı → KIRMIZI |
| D-1167 `VAL-C076/C077/C078` | `ConcurrentOrderScreenTests` | `ShownVersion.Matches` hep doğru → üç test KIRMIZI (C076, C077 müşteri önce, C078 sistem önce) |
| D-981 `VAL-C088` | `...VALC088_D981` | `IsPaymentOutcomeUnknown` hep yanlış → KIRMIZI |
| D-981 `VAL-C090/C091` | `CardRefundJobTests...O4/O5` | `sent >= order.Total` → `>` → iki test KIRMIZI |
| D-697 | `SchedulerScanTests` | `OutageRecovery.IsFresh` hep yanlış → VALC109 ve VALA827 satırları KIRMIZI |
| D-1725 | `...VALB107` | önceki dilim: kutu işaretliyken fatura adı değişinimi → iki yol KIRMIZI |
| D-1368 | `...UnderTheMembersRoot_...` | önceki dilim: ilk koşuda beklenen 303 gerçek davranışla (GET 302, POST 303) uyuşmadı |
| D-1284 | `...TheItemlessIbanFields_...` | önceki dilim: CHECK kısıtına takıldı, tohum düzeltildi |
| D-981 `C080`, `C002`, D-1424, D-1494 | ilgili testler | değişinim yapılmadı (⚠ aşağıda) |
| A2: D-801/D-572 (L-1 429 `Retry-After` yok) | `CustomerSignInPageTests.TheLimit429OfTheSignIn_HasNoRetryAfter_…` | `FormResults.AttemptLimitExceeded` 429'a `Retry-After` yazar → KIRMIZI (kayıt için `PastTheLimitL3_…NoRetryAfter` da) |
| A2: D-572/D-1405 pencere dolunca engel kalkar | `…TheBlock_LiftsWhenTheAttemptsLeaveTheWindow_…`, `RegistrationPageTests.TheBlockOfTheRegistration_…`, `PanelAccessPagesTests.TheBlockedAttempt_…` | `AttemptCounter.IsExceededAsync` pencereyi yok sayar → üçü KIRMIZI |
| A2: D-1405 tanınan tarayıcı | `ARecognizedBrowser_SignsIn_WhileTheEmailAxisIsBlocked_…` | `PasswordRegime` işareti hiç sunmaz → KIRMIZI |
| A2: D-1405 misafir sepeti birleşmesi | `ASignIn_WithARealGuestCart_MergesItIntoTheMembersCart_…` | `GuestCartMerge` birleştirmeyi atlar → KIRMIZI |
| A2: D-1405 `/kayit` Google yüzü | `TheRegistrationPage_HasNoGoogleButtonWhileGoogleIsUndefined_…` | `GoogleOpen` Google tanımını yok sayar → KIRMIZI |
| A2: D-1394/D-464 davet sayfası | `AdminAccountsPageTests.…_D1394_D464` dört test, `Removing_Deletes…_KK8` | `AdminAccounts` davet okuması bayrakları ve adres çakışmasını yok sayar, bağlantı sonlandırma ve geçersizleştirme etkisiz → altısı KIRMIZI |
| A2: D-1289 yaygın şifre | `ACommonPassword_Is422_…` | `PasswordPolicy` yaygın listeyi atlar → KIRMIZI |
| A2: D-1289 tanınan tarayıcı çerezi | `TheSetting_WritesTheKnownBrowserMarkCookie_…` | `Reset` çerezi yazmaz → KIRMIZI |
| A2: D-1289 K-899 çıkış | `TheAdministratorsSignOut_EndsOnlyThePanelSession_…` | çıkış vitrin çerezini de siler → KIRMIZI |
| A2: D-1301 eşzamanlı tüketim | `TwoVerificationPressesAtOnce_…`, `TwoRevertPressesAtOnce_…` | hesap kilidi atlanır ve `TryConsumeAsync` kullanılmışlık koşulunu bırakır → iki test (ve KK8 iki var olan test) KIRMIZI |
| A2: D-1723 | `…_VALB110_D1723` | `TryConsumeAsync` kullanıldı işaretini yazmaz → iki yol KIRMIZI (chromium) |
| A2: D-1289 `VAL-A306`, `VAL-A308` | ilgili testler | değişinim yapılmadı (⚠ aşağıda) |

## Öz denetim tablosu (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kabul 1: hedefi T604 olan kalemler kapanır ya da gerekçeyle daraltılır (A1 payı) | ✓ | Kapandı: D-697, D-981, D-1167, D-1284, D-1368, D-1424, D-1494, D-1676, D-1725. Daraltıldı: D-1283 (2) ve D-1599 → T604 U (kod gerekçesi satırda); yeni D-1753 hedef T604. Kalan T604 kalemleri A2…A4 dilimlerinin (bu dilimin kalemi değil) |
| Kabul 2 / KK2: para-güvenlik kalemleri ayrı başlıkta, kırmızı–yeşil | ~ | Yukarıdaki tablo; C080, C002, D-1424 değişinimsiz ⚠ |
| Kabul 3 / KK3 (D-1725, D-1494): üç motor, iki yol | ✓ | `VALB107` ve `VALB009` chromium, firefox, webkit × betikli/betiksiz yeşil |
| Kabul 4 / KK4 (D-1678) | — | Bu dilimin değil |
| Kabul 5 / KK5: kapanan satır `✅ Çözüldü → T604` | ✓ | `Docs/DEFERRED_BACKLOG.md` dokuz satır; daraltılan iki satır yeni hedefi (T604 U) ve gerekçeyi taşır |
| KK1: hedefi T604 olan kalem envanteri (`grep T604`) | ✓ | Her A1 kalemi kapalı ya da gerekçeli; satır listesi yukarıda |
| D-697 | ✓ | `SchedulerScanTests...VALC109`, `...VALA827` |
| D-981 | ✓ | C090, C091, C080, C002, C088 testli |
| D-1167 | ✓ | `ProductFormPageTests`, `ContentPanelPageTests`, `ConcurrentOrderScreenTests`; A371 tarayıcı payı D-1753 |
| D-1283 | ~ | (1) testli; (2) U'ya (`06 §4.31`: IBAN beyan başına, kod kayıt başına) |
| D-1284, D-1368, D-1424, D-1676, D-1725 | ✓ | önceki dilim; D-1725 üç motorda yeniden koşuldu |
| D-1494 | ✓ | `VALB009` testi, üç motor |
| D-1599 | ~ | U'ya (belge çatışması kodu da değiştirir) |
| `sik-fail-nedenleri`: tarama istisnası geniş / çağrı biçimi kaçırma | ✓ | Tarama ya da bekçi değişmedi |
| Hedefi bu task olan kayıt ne kapandı ne ileri atıldı | ✓ | Dokuz kapandı, iki yeniden ileri (T604 U), bir yeni |
| Kısmen karşılanan kabul (~) | ⚠ | A1 dilimi bütün T604 değildir; kabul 1'in kalan payı A2…A4 ve U parçasında, PR bu dilimin PR'ıdır |
| 06 default/FK, kural sayısı, 09 karar kaydı | ✓ | Şema, kural sayısı ve 09 metni değişmedi; karar kaydı yok (D-1599 kararı U'da) |
| Gerçek saate bağlı test yok | ✓ | Yeni testler test saati ve sürümle çalışır; bekleme yok |
| **A2** Kabul 1: A2 kalemleri kapanır ya da gerekçeyle daraltılır | ✓ | Kapandı: D-464, D-572, D-801, D-1289, D-1301, D-1394, D-1405, D-1723. Daraltıldı → T604 U: D-1374, D-1389 (satırlarda gerekçe). `grep -nE "T604" Docs/DEFERRED_BACKLOG.md` ile on kalem de tarandı |
| A2 D-464, D-1394 | ✓ | `AdminAccountsPageTests` beş yeni test (dört geçersizleşme anı sayfada, KK8 satırı); değişinimle altısı kırmızı |
| A2 D-572, D-801, D-1405 | ✓ | `CustomerSignInPageTests` sekiz yeni test, `RegistrationPageTests` bir; üç parçanın (1) `/kayit` yüzü, (2) 429/tanınan tarayıcı/pencere, (3) misafir sepeti hepsi testli |
| A2 D-1289 | ✓ | `PanelAccessPagesTests` altı yeni test: dört davranış + `VAL-A306` + `VAL-A308` |
| A2 D-1301 | ✓ | `PanelHomeAndVerificationPagesTests` iki yeni test, kilit altında deterministik eşzamanlılık |
| A2 D-1723 | ✓ | `…_VALB110_D1723` üç motor × iki yol yeşil; gözlem kaydında e-posta maskeli (`***@***`), anahtar yok; kayıt satırı yalnız boolean sütunlar |
| A2 D-1374, D-1389 | ~ | U'ya daraltıldı: `src` gerektiriyor (⚠ aşağıda); karar kaydı K-1625 U'da |
| A2 KK2 (para/güvenlik) | ✓ | Yukarıdaki KK2 tablosuna A2 satırları eklendi; çıkış yolu ve geri alma aynı: test dosyalarının PR'ı geri alınır |
| A2 KK3 (D-1723) | ✓ | chromium, firefox, webkit ayrı çağrılar, betikli ve betiksiz |
| A2 `sik-fail-nedenleri` (1) bekçi/tarama istisnası | ✓ | Tarama ya da bekçi değişmedi |
| A2 `sik-fail-nedenleri` (2) çağrı biçimi | ✓ | Yeni tarama yok |
| A2 `sik-fail-nedenleri` (3) 06 default/FK | ✓ | Şema değişmedi |
| A2 `sik-fail-nedenleri` (4) aynı sayıyı anan yerler | ✓ | Kural, sayaç ve şablon değişmedi; backlog sayacına dokunulmadı |
| A2 `sik-fail-nedenleri` (5) belge kuralı / karar kaydı | ✓ | 05/07/11/09 metni değişmedi; yeni karar yok (D-1374/D-1389 kararları U'da, kayıtla) |
| A2 12 satırı durumları | ⚠ | `VAL-A351`, `VAL-C075`, `VAL-A306`, `VAL-A308` BEKLEMEDE bırakıldı: kanıt testleri var, PASS yazımı ve uygulama kaydı payı faz kapısı/`12` sahibinde |

## ⚠ Maddeler

- D-1283 (2): `06 §4.31` IBAN'ı beyan (istek) başına tek satır sayar ("aynı beyanla girilen IBAN'ı birden çok kalem kullanır"); kod kalem kaydı başına yazar. `06` izlenir (kurala uygun seçenek); hizalama `src` (para) değiştirir → T604 U.
- D-1599: üst belge (`02 §10.4.10`, `03 §8.4.8`, `12 VAL-A238`) denetimden geçmeyen IBAN'da IBAN'sız kayıt der; `07` UC-130, `04` 7.2.10.4 ve T528 kodu 422 der. Öneri: `07`/`04` `02`'ye hizalanır, kod değişir; karar kaydı, belge hizası, kod ve `VAL-A238` koşumu T604 U'da (K-1623 numarası kullanılmadı).
- `VAL-A371` tarayıcı gözlemi yapılmadı (D-1753); uç yanıtı ve veri kaydı sınandı.
- D-981 C080/C002, D-1424 testleri var olan davranışın ilk testleridir; değişinimle kırmızı gösterilmedi.
- Üye kökü oturumsuz yönlendirme: backlog "302" diyordu; gerçekte GET 302, POST 303; testler gerçeği sabitler.
- C077 "yönetici önce ödendi" çiftinde müşterinin eski tam-iptal formu `OperationClosed` ile reddedilir (ödenmiş siparişin iptali kalem iptalidir); `02 §3.31.2`'nin "ya uygulanır ya reddedilir ve sebebi söylenir" cümlesine uyar.

- A2 D-1374: karar (kurala uygun seçenek): belge yönü korunur (`04` 6.3.12.2, K-811 gerekçesi), UC-157 doğrulanmamış kaydı da siler; `AccountDeletions.DeleteByAdministratorAsync` ve T533 sayfası değişir → T604 U, karar kaydı K-1625 U'da.
- A2 D-1389: karar: izin düşürme tek panel ara katmanına alınır (D-1454 ile aynı iş), kayıt kapatılmaz → T604 U.
- A2 `VAL-A351`, `VAL-C075`, `VAL-A306`, `VAL-A308` 12 satırları BEKLEMEDE bırakıldı (PASS yazımı ve uygulama kaydı payı faz kapısında); kanıt testleri backlog satırlarında.
- A2 D-1289 `VAL-A306`/`VAL-A308` testleri ve K-899 testi var olan davranışın ilk testleridir; `VAL-A306`/`VAL-A308` için değişinim yapılmadı (ikisi seed ve ret durumlarını doğrudan sınar), K-899 ve öteki testler değişinimle kırmızı gösterildi.
- A2 `/kayit` testleri `CustomerSignInPageTests` içinde (Google taklidi ve `Open(google, gateOpen)` orada); backlog "`CustomerSignInPageTests` ve `RegistrationPageTests`" dediği için yeri iki dosyaya bölündü.
- A2 D-1301 testleri bekletmeyi `pg_stat_activity` ile bekler (var olan KK8 deseni); gerçek saate bağlı bekleme yok.

## Commit & PR

PR: #594
