# T604 (A) — F5 kapısından kalan test aracı kalemleri

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (dilim A1 yapıldı, kalan A1 payları sonraki dilimde) | **Tarih:** 2026-11-18

---

## Dilim A1 — iade, ödeme, sipariş testleri

Yalnız test ve belge dosyaları değişti; `src` değişikliği yok (D-1283/D-1284/D-1368 ↔ U parçası UE1 çakışması: IBAN alanı ve iade ekranı kaynağına dokunulmadı).

### Yapılan işler

- D-1676: `PayTrPaymentGatewayTests.Query.cs` yasak değer ve yanıttaki net tutar "9,7631" (virgül JSON zaman damgasında bulunamaz).
- D-1725: `ScriptlessFlowTests.FillCheckoutAsync` yanıltıcı fatura değerlerini betikli yolda da gönderir.
- D-1368: `OrderOperationScreenTests` üye kökü: başka hesap 404, oturumsuz 302/303 giriş, kapalı işlem 303.
- D-1284 ve D-1283 (1): `CustomerOperationsTests` itemless alan süzgeci ve CardRefundNotRealized olgusu.
- D-1424: `DataExportTests` erişim, `SalesSummaryTests` sayfa düzeyi ciro/sayım.
- D-697: `SchedulerScanTests` VAL-C109 ve VAL-A827 payları.
- D-981: `CardRefundJobTests` (VAL-C090, C091), `PaymentNotificationTests` (VAL-C080, C002).
- Backlog: altı satır `✅ Çözüldü → T604`, iki satır daraltıldı, üç satır BÖLÜNDÜ (süre) notuyla işaretlendi.

### Notlar

- Working tree: temiz. `core.hooksPath` = `scripts/git-hooks`.
- Dış varsayım: yok (yalnız test kodu; PostgreSQL 18 CI ile aynı imaj, `track_commit_timestamp=on`).
- Ana dal CI başlangıç denetimi yardımcının işidir (yönetici düzeni); işçi dal CI'ını yoklamaz.
- Integration: TAM: CI. Koşulan sınıflar (tek çağrı, 184/184): OrderOperationScreenTests, CustomerOperationsTests, DataExportTests, SalesSummaryTests, SchedulerScanTests, CardRefundJobTests, PaymentNotificationTests. Tarayıcı: `TheSameAsDeliveryBox_DecidesTheBillingAddress` chromium iki yol 2/2 (firefox/webkit ve D-1725 üç motor kanıtı CI E2E ve A4 diliminde).
- Unit TAM: 8088/8088. `dotnet format` değişen 9 `.cs` dosyasında temiz.

## Para ve güvenlik kalemleri (KK2)

Bu dilimin kalemleri yalnız test ekler; ürün kodu değişmediği için "değişiklikten önce kırmızı" yerine her yeni testin **ayırt ediciliği** gösterilir ve var olan testler değişmeden geçer. Çıkış yolu ve geri alma: test dosyalarının PR'ı geri alınır (ürün davranışı etkilenmez).

| Kalem | Test | Kırmızı kanıtı |
|---|---|---|
| D-1725 | `ScriptlessFlowTests...VALB107` (chromium, iki yol) | `AddressBooks.cs` kutu işaretliyken `BillingRecipientName` boşsa teslimatı kullan değişinimi: iki yol KIRMIZI (2/2 fail); eski test betikli yolda yeşil kalıyordu; değişinim geri alınınca 2/2 yeşil |
| D-1368 | `...UnderTheMembersRoot_...` | İlk koşuda beklenen durum kodları (303) gerçek davranışla (GET 302, POST 303) uyuşmadı, kırmızı → gerçek davranış sabitlendi (backlog metni 302 diyordu) |
| D-1284 | `...TheItemlessIbanFields_KeepOnly...` | İlk koşu CHECK kısıtına (`itemless_on_refund_failure`) takıldı, tohum düzeltildi; süzgeç testi sipariş başına küme ve komut sayısını sınar |
| D-981 C080/C002/C090/C091, D-697, D-1424 | ilgili testler | Mevcut davranışın ilk kalıcı testleri; yeşil. Ürün kodu değişmediği için mutasyonla kırmızı gösterilmedi (⚠ aşağıda) |

## Öz denetim tablosu (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kabul 1: hedefi T604 olan kalemler kapanır ya da gerekçeyle daraltılır (A1 payı) | ~ | D-697, D-1284, D-1368, D-1424, D-1676, D-1725 kapandı; D-981 (C088 kaldı), D-1283 ((2) kaldı) daraltıldı; D-1167, D-1494, D-1599 BÖLÜNDÜ (süre) → sonraki A1 dilimi (tüm A1 bitmedi) |
| Kabul 2 / KK2: para-güvenlik kalemleri ayrı başlıkta, kırmızı–yeşil | ~ | Yukarıdaki tablo; yeni davranış kodu yok |
| Kabul 3 / KK3 (D-1725): üç motor, iki yol | ~ | chromium iki yol yerelde yeşil (mutasyonla kırmızı); firefox/webkit CI E2E'ye bırakıldı ⚠ |
| Kabul 4 / KK4 (D-1678) | — | Bu dilimin değil (A4) |
| Kabul 5 / KK5: kapanan satır `✅ Çözüldü → T604` | ✓ | `Docs/DEFERRED_BACKLOG.md` D-697, D-1284, D-1368, D-1424, D-1676, D-1725 |
| D-697 | ✓ | `SchedulerScanTests.Scan_AHeartbeatOneMinuteOld_...VALC109`, `...VALA827` (3 veri satırı) |
| D-981 | ~ | VAL-C090, C091, C080, C002 testli; C088 kaldı |
| D-1167 | ✗ | BÖLÜNDÜ |
| D-1283 | ~ | (1) testli; (2) kaldı |
| D-1284 | ✓ | test adı yukarıda |
| D-1368 | ✓ | iki test |
| D-1424 | ✓ | (a) ve (b) testleri |
| D-1494 | ✗ | BÖLÜNDÜ |
| D-1599 | ✗ | BÖLÜNDÜ |
| D-1676 | ✓ | Unit TAM 8088/8088 |
| D-1725 | ✓ | değişinim denemesi |
| `sik-fail-nedenleri`: tarama istisnası geniş / çağrı biçimi kaçırma | ✓ | Tarama ya da bekçi değişmedi |
| 06 default/FK, kural sayısı, 09 karar kaydı | ✓ | Şema, kural ve 09 metni değişmedi |
| Gerçek saate bağlı test yok | ✓ | Yeni zamanlayıcı testleri test saatiyle (`Now`) çalışır |

## ⚠ Maddeler

- Süre bütçesi (45 dk) nedeniyle D-1167, D-1494, D-1599 yapılmadı; D-981 C088 ve D-1283 (2) kaldı. Yönetici A1'in ikinci dilimini açar.
- D-981/D-1424/D-697 testleri var olan davranışın ilk kalıcı testleri olduğundan ürün kodu mutasyonuyla kırmızı gösterilmedi.
- Üye kökü oturumsuz yönlendirme: backlog "302" diyordu; gerçekte GET 302, POST 303 (ikisi de `/giris`); testler gerçeği sabitler.

## Commit & PR

PR: (açılınca yazılır)
