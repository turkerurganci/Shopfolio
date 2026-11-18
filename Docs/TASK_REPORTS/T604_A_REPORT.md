# T604 (A) — F5 kapısından kalan test aracı kalemleri

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (dilim A1 bitti; A2…A4 payları sonraki dilimlerde) | **Tarih:** 2026-11-18

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

## ⚠ Maddeler

- D-1283 (2): `06 §4.31` IBAN'ı beyan (istek) başına tek satır sayar ("aynı beyanla girilen IBAN'ı birden çok kalem kullanır"); kod kalem kaydı başına yazar. `06` izlenir (kurala uygun seçenek); hizalama `src` (para) değiştirir → T604 U.
- D-1599: üst belge (`02 §10.4.10`, `03 §8.4.8`, `12 VAL-A238`) denetimden geçmeyen IBAN'da IBAN'sız kayıt der; `07` UC-130, `04` 7.2.10.4 ve T528 kodu 422 der. Öneri: `07`/`04` `02`'ye hizalanır, kod değişir; karar kaydı, belge hizası, kod ve `VAL-A238` koşumu T604 U'da (K-1623 numarası kullanılmadı).
- `VAL-A371` tarayıcı gözlemi yapılmadı (D-1753); uç yanıtı ve veri kaydı sınandı.
- D-981 C080/C002, D-1424 testleri var olan davranışın ilk testleridir; değişinimle kırmızı gösterilmedi.
- Üye kökü oturumsuz yönlendirme: backlog "302" diyordu; gerçekte GET 302, POST 303; testler gerçeği sabitler.
- C077 "yönetici önce ödendi" çiftinde müşterinin eski tam-iptal formu `OperationClosed` ile reddedilir (ödenmiş siparişin iptali kalem iptalidir); `02 §3.31.2`'nin "ya uygulanır ya reddedilir ve sebebi söylenir" cümlesine uyar.

## Commit & PR

PR: #594
