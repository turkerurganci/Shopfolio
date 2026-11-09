# T375 — Yöneticinin sipariş düzeltmeleri: ileri yönlü kayıt, tutar değişmez, firmanın teyidiyle e-posta ve adres düzeltmesi, cayma beyanı geri alınmaz

**Faz:** F3 | **Durum:** ✓ Yapım tamam, doğrulama bekliyor | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Adres düzeltmesi (müdahale 1; k3, k4, k5):** `IOrderCorrections.CorrectAddressAsync` (`Ordering/OrderCorrections.cs`). Sipariş satır kilidi, ekranın sürümü, `OrderOperations.CheckAsync(CorrectAddress)` kararı (fiziksel kalemde `Placed`/`Preparing`; fiziksel kalemsizde `Delivered` hariç), firmanın teyidi (`AddressRequestConfirmation`: sipariş e-postasından geldi / firma e-postayla / telefonla teyit etti; `None` reddedilir), alan kuralı `IMembershipModule.ResolveOrderAddressesAsync`'in tek evi (kargo: fiziksel + siparişin mevcut faturası; fatura: dijital), `OrderAddressHistory` satırı (eski ve yeni değer, yönetici, an), sürüm bir kez, B-10 siparişin e-postasına, iz satırı (`CorrectAddress`, değer yok).
- **Misafir e-postası düzeltmesi (müdahale 9; k6, k7, k8):** `CorrectGuestEmailAsync`. Yalnız misafir ve imha edilmemiş sipariş (03 1.11.35); teyit ad + kalemler + tutar (+ fiziksel kalemde teslimat telefonu), numara tek başına teyit değil; `OrderEmailHistory` satırı; `Order.RenewAccessKey` (eski anahtar ve eski adresle numara açmaz); hesap bağı eski bağı keser, yeni adresin doğrulanmış hesabı varsa bağlar, `IsAccountDeleted` ise bağlamaz; gönderilmemiş bildirimler yeni adrese yönlenir (`IOrderContactNotices.ReroutePendingCustomerNoticesAsync`); B-1 yeni adrese (zincirin kökü ilk B-1), B-15 eski adrese; iz satırı (`CorrectContactEmail`).
- **Dokuz müdahale (k2):** `OrderInterventions.All` — kapalı liste, her biri işlem iz işlemine eşli; elle sipariş yok.
- **D-66:** `OrderAddressHistory` / `OrderEmailHistory` ayarlayıcıları `private set`; T110 testleri `Set(...)` yardımcısına geçti.
- **D-567:** `OrderAccessKeySourceScan` — anahtar yazımı yalnız `Order.cs`, yenileme çağrısı yalnız `OrderCorrections.cs` (ihlal örneğiyle).
- **Tarama uyumları:** `AddressRequestConfirmation` kendi enum dosyasında, 06'da saklanmayan enumlar listesinde (`ApplicationEnums.NotStored`); yeniden yönlendirme yöntemi adı `Reroute…` (RedirectSourceScan); `IrreversibleConfirmationTests` "Correct" sayımı `IOrderCorrections`'ı ayırır.

## Etkilenen Modüller / Dosyalar

- Ordering: `Contracts/IOrderCorrections.cs`, `Contracts/AddressRequestConfirmation.cs`, `OrderCorrections.cs`, `OrderInterventions.cs`, `OrderingModule.cs`, `OrderAddressHistory.cs`, `OrderEmailHistory.cs`
- Notifications: `Contracts/IOrderContactNotices.cs`, `OrderContactNotices.cs`, `NotificationsModule.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Ordering/OrderCorrectionTests.cs` (26), `tests/Shopfolio.UnitTests/Architecture/OrderAccessKeySourceScan*.cs` + `ViolationSamples/OrderAccessKey/KeyWriters.cs` (5), `Model/StockReservationAndHistoryTablesTests.cs` (uyarlandı), `Support/ApplicationEnums.cs`, `Ordering/IrreversibleConfirmationTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İleri yönlü kayıt, tutar değişmez, kalem eklenmez, fiyat değişmez | ✓ | `OrderCorrectionTests.BothCorrections_LeaveTheAmountTheItemsAndTheAxesAlone_…_KK1` (toplam, kargo, kalem adet/fiyat/KDV/kupon, eksenler, sürüm +2; sözleşme yalnız iki yöntem, istekte tutar/adet/fiyat/kalem alanı yok) |
| 2 | Kapalı dokuz müdahale, her biri iz; elle sipariş yok | ✓ (iz: 1 ve 9; kalanı ⚠ D-884) | `TheInterventions_AreNine_…_KK2`, `EachCorrection_WritesItsTrailRow_…_KK2` |
| 3 | Adres sınırı iki durumda | ✓ | `AnOrderWithAPhysicalItem_TakesBothAddressesUntilShipped_KK3` (Placed/Preparing evet; Shipped/Delivered hayır), `AnOrderWithNoPhysicalItem_TakesTheBillingAddressUntilDelivered_…_KK3` |
| 4 | Müşteri talebi; sipariş e-postasından değilse firma teyidi | ✓ | `AnAddressRequestThatTheFirmDidNotConfirm_ChangesNothing_…_KK4`, `ARefusedCorrection_WritesNothing_…_KK4` |
| 5 | B-10 gider; eski ve yeni adres geçmişte | ✓ | `AnAddressCorrection_SendsB10ToTheOrdersEmail_AndKeepsTheOldAndTheNewValueOnTheOrder_KK5` |
| 6 | E-posta yalnız firma teyidiyle; müşteri alanı yok; numara teyit değil | ✓ | `TheCustomersSurfaces_HaveNoWayToChangeTheEmailOrTheAddressOfAnOrder_KK6`, `TheEmailOfAnAccountsOrder_IsNotCorrected_…_KK6`, `TheEmailCorrection_NeedsTheNameThePhoneTheItemsAndTheAmount_…_KK7` (4 durum + teyitsiz) |
| 7 | Teyit ad/telefon/kalem/tutar; eski anahtar geçersiz; yeni adrese B-1, eskiye B-15 | ✓ | `TheEmailCorrection_KillsTheOldKey_OpensTheOrderByTheNewAddress_SendsB1ToTheNewAndB15ToTheOld_…_KK7`; `AnOrderWithNoPhysicalItem_NeedsNoShippingPhone_…_KK7` |
| 8 | Eski/yeni e-posta geçmişte; doğrulanmış hesaba düşer; silinmiş hesaba düşmez | ✓ | `AVerifiedAccountOfTheNewAddress_TakesTheOrder_…_KK8`, `AnAddressWithNoVerifiedAccount_LeavesAGuestOrder_…_KK8`, `AnOrderWhoseAccountWasDeleted_IsNotLinkedTo…_KK8`; geçmiş satırı `…KillsTheOldKey…_KK7` |
| 9 | Cayma beyanı geri alınmaz | ✓ | `NoModuleSurface_TakesBackOrCorrectsAWithdrawalDeclaration_KK9` (+ T372'nin `NoModuleSurface_UndoesOrCorrectsAWithdrawal_KK9`, `IrreversibleConfirmationTests`) |

### Öz denetim (bitiş kapısı 2)

Devam işçisi bütün maddeleri (önceki işçinin yaptıkları dahil) diff'te yeniden denetledi; `sik-fail-nedenleri.md` okundu.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 tutar sabit, kalem ekleme yok | ✓ | yukarıdaki 1 |
| KK2 dokuz müdahale, her biri izde | ✓ (⚠ iz satırları 2-4, 6-8) | `OrderInterventions.All`; D-884 |
| KK3 adres sınırı iki durumda | ✓ | iki Theory |
| KK4 teyitsiz adres düzeltmesi yok | ✓ | `…KK4` iki test |
| KK5 B-10 gider, geçmiş durur | ✓ | `…KK5` |
| KK6 müşteri e-posta alanı yok | ✓ | `…KK6` iki test |
| KK7 eski anahtar geçersiz, B-1 yeni, B-15 eski | ✓ | `…KK7` |
| KK8 hesap bağı doğru; silinmiş hesap düşmez | ✓ | `…KK8` üç test |
| KK9 beyanı geri alma yolu yok | ✓ | `…KK9` |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | D-567 taraması istisnası tam yol (`OrderFile`, `CorrectionsFile`); aynı adlı başka dosya istisna değil (`Scan_AFileOfTheSameNameElsewhere_…`) |
| Sık FAIL 2: tarama bir çağrı biçimini kaçırıyor | ✓ | atama, `SetProperty` toplu güncelleme, ham sütun adı ve `RenewAccessKey(` ayrı kurallar; örnek dosyada hepsi + yorum/karşılaştırma/okuma temiz |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ (yok) | yeni tablo, sütun, göç yok; geçmiş tabloları T110'dan |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | `IrreversibleConfirmationTests` "Correct" sayımı, `ApplicationEnums.NotStored`, `EnumLockTests`/`EnumTableConsistencyTests` yeşil; Unit tam koşu |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ (yok) | 09/02/03/06 metni değişmedi |
| Devredilen backlog | ✓ | D-66, D-564, D-567 yerinde kapatıldı; D-647 → D-883 (T527) |
| `09 §25` modül sınırı, arayüzde `SaveChanges` yok | ✓ | `OrderCorrections` çağıranın işleminde çalışır; `ModuleBoundaryTests` yeşil |
| `09 §25` zaman `TimeProvider` | ✓ | `_time.GetUtcNow()` |
| `09 §25` göç / yeni tablo / yeni ayar | ✓ | Yok |
| Güvenlik taraması | ✓ | Yalnız yönetici çağırır (`TransitionAdmin` zorunlu, yoksa `ActorNotAllowed`); teyitsiz yazım yok; iz satırı değer taşımaz (test); yeni paket/uç yok |

## ⚠ Kolay kararlar (kurala en uygun seçenekle verildi)

- Değişmeyen değerle düzeltme `InvalidInput`; aynı e-posta (büyük/küçük harf farkıyla) da `InvalidInput`.
- Fiziksel kalemsiz siparişte kargo adresi düzeltmesi ve telefonla teyit `ConditionNotMet` (kargo adresi ve teslimat telefonu yok).
- Kargo adresi teslimat alanı denetimine tabidir (`ResolveOrderAddressesAsync`); alan kuralı yeni evde değil, mevcut tek evde.
- İstek doğrulaması ürünce denetlenmez; yalnız beyan istenir.
- Müdahale 2-4, 6-8'in iz satırları Ordering'de henüz yazılmıyor (D-884, panel T527); teslim tarihi düzeltmesi ve cayma penceresi yeniden hesabı D-883 (T527).
- Integration testinde `IMembershipModule` yalnız iki yöntemi gerçek (adres kuralları = gerçek `AddressBooks`, hesap araması = modülün sorgusu) bir vekille kurulur; Membership'in tam DI zinciri bu test için gerekmedi.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 5587/5587 |
| Integration | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Ordering` + `StockReservationAndHistoryTablesTests` | Ordering 528/528 (OrderCorrectionTests 26), Model sınıfı 28/28 |
| Lint | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (3 proje) | temiz |
| Build | ✓ | `dotnet build <proje> -warnaserror --no-restore` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
