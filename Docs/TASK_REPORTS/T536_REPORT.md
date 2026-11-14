# T536 — Ödeme yöntemleri, kargo, süreler ve varsayılanlar (E-45, E-46)

**Faz:** F5 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-14

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **E-45 ödeme yöntemleri** (`Pages/Panel/PaymentMethods.cshtml(.cs)`, UC-100/UC-167): kart satırı kurulumdan (`ICardPaymentCondition`), havale anahtarı, IBAN dörderli; dört hâl. Havalenin açılması ve ilk IBAN onaysız; dolu IBAN değişimi, havale kapatma ve havale kapalıyken dolu IBAN'ın boşaltılması onay hâli (200, hiçbir şey yazılmaz) — cümle ödemesi beklenen havale siparişi sayısını `IOrderingModule.CountBankTransferOrdersAwaitingPaymentAsync`'ten okur (D-833). 422: IBAN boşken havale açma ve açıkken IBAN boşaltma (`BankTransferIbanRequired`, K-817), bozuk IBAN; 409 sürüm. `[OutcomeUncertain("UC-167")]`.
- **E-46 kargo, süreler ve varsayılanlar** (`Pages/Panel/ShippingAndDefaults.cshtml(.cs)`, UC-101, UC-168…UC-172): beş bölüm ayrı form ve ayrı sürüm (07 adları `ShippingFeeVersion`…), bölüm başına 409 yalnız kendi bölümünde, K-817 çit metinleri (`FenceText`, ret kaydı `ValueOutOfBounds`); iller tek tek kutu (betiksiz yolda "Tümünü seç" yok; betikli düğmeler D-1443); iade adresi değişikliği onay hâli, sayı `IAfterSalesModule.CountItemsAwaitingReturnGoodsAsync` — yalnız "iade malı bekleniyor" işaretli kalemler (`ItemMarksAsync` ile 500'lük sayfalarla daraltılır, KK10). `[OutcomeUncertain("UC-172")]`.
- **Metin sürümü:** `LegalTextGenerator.RegenerateAsync(feedingSettingChanged, ct)` — ücret, il, kargoya verme süresi ve iade adresi gerçekten değişince metin aynı kalsa da Ön Bilgilendirme Formu ile sözleşmenin yeni sürümü yazılır (D-681, D-493 sürüm payı); `SettingsModule.RegenerateWhenFedAsync` sürüm iz satırlarını (`PublishLegalText`) ayarın iz satırıyla aynı işlemde yazar (D-220 iade payı). Eşik, havale süresi ve varsayılanlar sürüm doğurmaz.
- **İz:** `FirmSettings.SaveReturnAddressAsync` iade adresi iz satırı (il plaka koduyla; D-165 (a), D-256, D-1377); `CompanySettingsWriter.SaveReturnAddressAsync`'e `readUnderLock` geri çağrısı (çift kilit `LockSequence`'i kırıyordu). İz yazarı `RequestAuditActorSource` yöneticiyi `SessionSchemes.AccountId()` ile okur (D-176).
- Tutar kuralları sunucudaki tek evden (`FirmSettings` / ayar girdi doğrulayıcıları); sayfa katmanı tutar hesaplamaz, yalnız iletiyi seçer.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/{PaymentMethods,ShippingAndDefaults}.cshtml(.cs)`
- `src/Shopfolio.Web/Modules/Settings/{LegalTextGenerator,SettingsModule,FirmSettings,CompanySettingsWriter,DeliveryProvinces}.cs`, `Modules/Ordering/OrderingModule.cs` (+ arayüz), `Modules/AfterSales/AfterSalesModule.PendingWork.cs` (+ arayüz), `Modules/Administration/AuditActorSource.cs` (`RequestAuditActorSource`)
- Düzeltme 2: `Modules/Settings/{CompanySettingsWriter,SettingsModule}.cs`, `Contracts/ISettingsModule.cs` (`CheckReturnAddressAsync`), `Pages/Panel/ShippingAndDefaults.cshtml.cs`; test sahteleri `Model/AddressTests.cs`, `Model/MembershipTests.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Web/PaymentAndShippingPagesTests.cs` (yeni, 44; düzeltme 2 ile iki test, dört durum daha), `Model/FirmSettingsTests.cs`, `Model/SalesGateTests.cs` (iz satırı kurulumu); `tests/Shopfolio.UnitTests/Consistency/OutcomeUncertainConsistencyTests.cs` (UC-167, UC-172 listeden çıktı)
- Kayıt: `Docs/STATUS_CHANGELOG.d/T536.md`, `Docs/DEFERRED_BACKLOG.d/T536.md` (D-1443, D-1444), `IMPLEMENTATION_STATUS.md` tablo satırı, `DEFERRED_BACKLOG.md` (D-833, D-165, D-220, D-256, D-681, D-176 kapandı; D-493 sürüm payı), `DEFERRED_BACKLOG.d/T524.md` (D-1377 kapandı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kart satırı kurulumdan; havale anahtarı; IBAN dörderli | ✓ | `ThePaymentScreen_ReadsTheCardLineFromTheInstallation_AndShowsTheIbanInGroupsOfFour_KK1`, `ThePaymentScreen_ShowsEachOfTheFourStates_KK1`, `ThePaymentScreen_WithBothMethodsClosed_SaysTheGateConditionIsNotMet_KK1` |
| 2 | Havale açma + ilk IBAN onaysız; IBAN değişimi ve kapatma onay hâli 200 | ✓ | `OpeningTheTransferWithTheFirstIban_IsSavedWithoutAConfirmation_…_KK2_KK4`, `ChangingAFilledIban_AnswersTheConfirmationWithTheWaitingOrders_WritesNothing_…_KK2_KK4`, `ClosingTheTransfer_AnswersTheConfirmationWithTheWaitingOrders_…_KK2`, `EmptyingAFilledIbanWhileTheTransferIsOff_AsksTheConfirmation_…_KK2` |
| 3 | 422 üç hâl; 409 sürüm; yeni IBAN yalnız yeni siparişe | ✓ | `ThePaymentSave_RefusesTransferWithoutIban_AnEmptiedIbanAndABadIban_AndAStaleVersionIs409_KK3`, `BothIbanRefusals_LogBankTransferIbanRequired_KK3`; bekleyen siparişin IBAN'ı donmuş kalır: `ChangingAFilledIban_…_KK2_KK4` (`orders.bank_transfer_iban` eski) |
| 4 | IBAN girişi ve değişimi bütün yöneticilere bildirim; iz eski/yeni | ✓ | `OpeningTheTransfer…_KK2_KK4` (iki yönetici, iki `dispatch_records`, iz `BankTransferIban`), `ChangingAFilledIban…_KK2_KK4` (iz eski ve yeni IBAN) |
| 5 | Beş bölüm ayrı sürüm; boş hâller; 409 + iz; besleyen değişiklikte iki metin sürümü | ✓ | `TheShippingScreen_CarriesFiveVersions_AndSaysWhenNoProvinceAndNoThreshold_KK5`, `EachSection_WithAStaleVersion_Is409_WritesNothing_AndShowsTheConflictOnItsOwnSectionOnly_KK5`, `ASavedSection_WritesItsTrailRow_AndOnlyAFeedingValueRaisesTheTwoTexts_KK5` (Theory: ücret, il, süre, iade adresi +2; varsayılanlar 0), `OnlyTheThresholdAndTheWindowChanged_RaiseNoTextVersion_KK5` |
| 6 | Dört tutar 422'si | ✓ | `TheFeeSave_RefusesANegativeAThreeDecimalAnOverLimitAndAnEmptyFee_KK6` |
| 7 | İl satırları tek işlem; liste dışı il 422; seçimsiz kayıt; betiksiz yolda "Tümünü seç" yok | ✓ | `TheProvincesSave_RefusesAProvinceOffTheList_SavesAnEmptySelection_AndHasNoSelectAllWithoutScript_KK7` |
| 8 | P-5 üst, P-7 alt/üst çit 422 | ✓ | `ThePeriodsSave_RefusesAValueOutsideItsFence_WithK817sText_KK8` |
| 9 | Çit dışı 422; var olan ürünün oranı değişmez | ✓ | `TheDefaultsSave_RefusesAValueOutsideTheFence_KK9`, `TheDefaultsSave_WritesTheDefault_AndAnExistingProductKeepsItsRate_KK9` |
| 10 | İlk giriş onaysız, değişiklik onay hâli; 422; beyan adresi sabit; onayda beklenen iade sayısı | ✓ | `TheReturnAddress_FirstEntryIsSavedWithoutAConfirmation_AChangeAsksItWithTheAwaitedItems_AndTheConfirmedSendSaves_KK10`, `TheReturnAddressSave_RefusesAnEmptyRequiredPartAProvinceOffTheListAndABadPhone_KK10`, `TheReturnAddressConfirmation_CountsTheItemsMarkedAwaitingTheReturnGoods_NotTheOtherAwaitedOnes_KK10` (n=2); girilmiş adresin geçersiz değişikliği onaysız ilk gönderimde 422 + alan mesajı, onay hâli yok, geçerli değişiklik onay hâli (düzeltme 2): `AnInvalidChangeOfTheEnteredReturnAddress_Is422WithTheFieldMessageOnItsFirstSend_NotTheConfirmation_AndAValidChangeIsAsked_KK10` (boş alıcı, liste dışı il, telefon "12"); beyan adresi: `CompanyIdentityTests.Withdrawal_KeepsTheReturnAddressItCopied_WhenTheSettingChangesLater` |

### Öz denetim (bitiş kapısı 2)

Önceki dört yapım turunun yaptıkları dahil bütün maddeler diff'te yeniden denetlendi. Düzeltme 1'de (main `49ba60f0` birleşik ağaç) tablonun bütün maddeleri yeniden denetlendi: kanıt testlerinin hepsi dalda duruyor ve yeşil (PaymentAndShippingPagesTests 44/44, CompanyIdentityTests ve FirmSettingsTests yeşil); hedefi T536 olan açık backlog kalemi yok (D-165, D-176, D-220, D-256, D-681, D-833, D-1377 kapalı; D-493 sürüm payı). Düzeltme 2'de (main `49ba60f0`, dal main'i içeriyor) kabul kriterleri 1–10 ve KK1–KK10'un bütün maddeleri yeniden denetlendi: değişiklik yalnız iade adresi işleyicisinin onaydan önceki yoluna ve yazıcının denetiminin okuma yoluna dokunur; KK1–KK9 kanıtları değişmedi ve yeşil (PaymentAndShippingPagesTests, CompanyIdentityTests, FirmSettingsTests 101/101); KK10'un 422 payı artık girilmiş adresin değişikliğinde de kanıtlı (B1); hedefi T536 olan D-1480 bu turda kalıcı testle kapandı.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 dört hâl; dörderli IBAN | ✓ | yukarıda KK1 |
| KK2 onaysız, onaylı; beklenen havale sayısı | ✓ | KK2 testleri; sayı `CountBankTransferOrdersAwaitingPaymentAsync` (D-833) |
| KK3 üç 422; 409 | ✓ | KK3 testleri |
| KK4 bildirim; iz | ✓ | `…_KK2_KK4` |
| KK5 beş sürüm, 409, iz; boş hâller; metin sürümü | ✓ | KK5 testleri |
| KK6 dört tutar 422'si | ✓ | KK6 Theory |
| KK7 il 422; seçimsiz kayıt | ✓ | KK7 testi |
| KK8 iki çit | ✓ | KK8 Theory |
| KK9 çit; oran sabit | ✓ | KK9 testleri |
| KK10 422; beyan adresi sabit; beklenen iade sayısı | ✓ | KK10 testleri (n>0 hâli `…_KK10` sayım testi); 422 hem ilk girişte (`TheReturnAddressSave_Refuses…_KK10`) hem girilmiş adresin değişikliğinin onaysız ilk gönderiminde (`AnInvalidChangeOfTheEnteredReturnAddress_…_KK10`, düzeltme 2) |
| VAL-A597 ilk gönderim alanları doğrular (onaydan önce) | ✓ | `AnInvalidChangeOfTheEnteredReturnAddress_…_KK10` (422 + alan mesajı, onay cümlesi yok, yazma yok); ödeme yöntemlerinde bozuk IBAN onaydan önce 422 (`ThePaymentSave_Refuses…_KK3`) |
| VAL-A752 IBAN değişikliği + havale kapatma birlikte | ✓ | `ChangingTheIbanAndClosingTheTransferTogether_AsksBothSentencesWithTheWaitingOrder_AndTheConfirmedSendClosesTheSale_A752` (D-1480; kart kapalı, bir bekleyen havale: iki cümle, ikisinde sayı 1, "satış da kapanacak"; yazma yok; onaylı kayıt havaleyi kapatır, satış kapısında `PaymentMethod` karşılanmamış, sipariş eski IBAN'lı) |
| VAL-A288 havale aç/kapat | ✓ (panel payı) | `OpeningTheTransfer…`, `ClosingTheTransfer…`; vitrin ödeme seçeneği T509'un |
| VAL-A289 IBAN girişi | ✓ | `OpeningTheTransfer…` (`company_settings.bank_transfer_iban`) |
| VAL-D032 IBAN siparişe donar | ✓ | `ChangingAFilledIban…`, `EmptyingAFilledIban…` (`orders.bank_transfer_iban` eski kalır) |
| VAL-A290 bekleyen havale sayısı | ✓ | `ChangingAFilledIban…`, `ClosingTheTransfer…` (cümlede N) |
| VAL-A291/A292 ücret, eşik, asgari tutar | ✓ (panel payı) | `TheFeeSave…_KK6`, `ASavedSection…_KK5`; sepete yansıma T509 / sepet task'larının |
| VAL-A293 teslimat illeri | ✓ (panel payı) | `TheProvincesSave…_KK7`; adres kısıtı `AddressTests` (önceki task) |
| VAL-A294 kargoya verme ve havale süresi | ✓ (panel payı) | `ThePeriodsSave…_KK8`, `ASavedSection…_KK5` |
| VAL-A295 iade adresi | ✓ | `TheReturnAddress_FirstEntry…_KK10` |
| VAL-D033 beyan adresi sabit | ✓ | `CompanyIdentityTests.Withdrawal_KeepsTheReturnAddressItCopied_WhenTheSettingChangesLater` |
| VAL-A296 kargo KDV'si ayar değil | ✓ | E-46'da kargo KDV alanı yok, yalnız açıklama satırı `FeeVatNote` (`ShippingAndDefaults.cshtml`) |
| VAL-A312 on firma ayarı panelde | ✓ (bu task payı) | E-46 bölümleri: ücret, eşik, asgari tutar, iller, iki süre, indirme hakkı ve KDV varsayılanı (KK5–KK9 testleri); ürün formundaki alanlar kendi task'ının |
| VAL-A539 `BankTransferIbanRequired` | ✓ | `BothIbanRefusals_LogBankTransferIbanRequired_KK3` |
| VAL-A750 onay yalnız riskli değişiklikte | ✓ | KK2 testleri |
| VAL-A755 iade adresi onayı + iade malı sayısı | ✓ | `TheReturnAddress_FirstEntry…_KK10`, `TheReturnAddressConfirmation_Counts…_KK10` |
| VAL-B087/B088/B103 (tarayıcı) | BEKLEMEDE | tarayıcı turu; betikli "Tümünü seç" D-1443 |
| `09 §25` ilgili satırlar; güvenlik | ✓ | panel sayfaları yönetici oturumu + antiforgery (Unit rota/form taramaları yeşil); girdi modeli taramaları; sır yok; `[OutcomeUncertain]` UC-167/UC-172 |

### Sık FAIL nedenleri

- (1) Bekçi/tarama istisnası eklenmedi. (2) Yeni tarama yazılmadı; var olan taramalar (`[SpecAssignedName]`, D-82 lambda, kart verisi adı) düzeltilerek geçirildi. (3) Şema değişmedi. (4) KK10 sayısı ile ana sayfa sayacı ayrıştı; sayacın kendisi değişmedi, ayrışma D-1444'e yazıldı. (5) 09 metni değişmedi. (6) Düzeltme 1: test sırasına bağımlılık (koleksiyon içi yalıtım) giderildi — düzenek bilinen durumu kendisi kurar. (7) Düzeltme 2: tek kural evi — sayfa iade adresi kuralını yeniden yazmaz, yazıcının denetimi (`CompanySettingsWriter.CheckReturnAddressAsync`) kayıtta ve onaydan önce aynı yöntemdir; modül arayüzüne eklenen yöntemi uygulayan iki el yazımı test sahtesi (`AddressTests`, `MembershipTests`) güncellendi.

## Test Sonuçları

| Tür | Sonuç | Komut | Not |
|---|---|---|---|
| Build | ✓ | `dotnet build -warnaserror` | düzeltme 2: 0 uyarı 0 hata; düzeltme 1: main `49ba60f0` birleşik ağaçta 0 uyarı 0 hata (önce `edf4eb2a`) |
| Unit | ✓ TAM | `dotnet test --project tests/Shopfolio.UnitTests` | düzeltme 2: 7557/7557; düzeltme 1: Consistency 295/295 + kalan 7262/7262, betik testleri 4/4 (önce 7546/7546) |
| Integration | ✓ hedefli — TAM: CI | `--filter-class <ad>` | düzeltme 2: PaymentAndShippingPagesTests + CompanyIdentityTests + FirmSettingsTests 101/101 (iki yeni test dahil); main `3fe7d829` (T517) birleştirmesinden sonra PaymentAndShippingPagesTests 48/48 (44 + dört yeni durum), Unit 7557/7557; TAM: CI; düzeltme 1: FirmSettingsTests + PaymentAndShippingPagesTests + SalesGateTests + LegalTextTests 116/116, CompanyIdentityTests + FirmSettingsTests 53/53; kırmızı test tek başına da yeşil (düzeltmeden önce tek başına 0 ≠ 2 ile kırmızıydı). İlk tur: PaymentAndShippingPagesTests 44/44 (birleşik ağaç); önceki turlarda Model 161 (FirmSettings, SalesGate, Address, SettingsTables, LegalText, CompanyIdentity, AuditTrail) + AuditTrail 22, Web/Admin 97 (AppCache, StartupDatabaseCheck, HomeAndFirmPage, PanelHomeAndVerificationPages, SecurityHeaders, Administration.PanelHomeTests) |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` | Web, UnitTests, IntegrationTests temiz; birleştirmede değişen `OutcomeUncertainConsistencyTests.cs` ayrıca temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL (2026-11-14; dal ucu `b7bc9496`, origin/main `49ba60f0` — dal main'i içeriyor, birleştirme gerekmedi) — build `-warnaserror` 0 uyarı, format ×3 temiz, Unit 7557/7557, betik testleri 4/4, hedefli Integration 342/342 (PaymentAndShippingPages, FirmSettings, SalesGate, LegalText, HomepageAndIdentityTrail, FirmSettingDefaults, CompanyIdentity, AuditTrail, PanelHome, OrderConfirmation, WithdrawalDeclaration, ItemRecords, HomeAndFirmPage; TAM: CI), PR #568 CI yeşil (run 43546205079) |
| `12` satırları (tur 1) | PASS: `VAL-D032`, A290, D033, A539, A750, A751, A752 (doğrulayıcının geçici sınamasıyla — kalıcı test D-1480), A753, A754, A755, A756, A869; BEKLEMEDE: A288, A289, A291–A296, A312, A757, A871 (vitrin/sepet/sipariş sayfası ya da genel altyapı payı başka task'ta), B087, B088, B103 (tarayıcı turu; betikli il düğmeleri D-1443) |
| Bulgu sayısı (tur 1) | 1 (ORTA, S1) |
| Düzeltme gerekli mi | Evet — **B1 (KK10; `07 §2.2.4`, UC-172; komşu `VAL-A597`):** `ShippingAndDefaultsModel.OnPostSaveReturnAddressAsync` girilmiş adresin değişikliğinde onay hâlini alanları doğrulamadan verir; zorunlu alanın boşaltılması, liste dışı il ve bozuk telefon onay işaretsiz ilk gönderimde 422 yerine 200 onay hâli ("İade adresini değiştir" ve K-814 cümlesi) alır, 422 yalnız onaylı gönderimde gelir. `07 §2.2.4`: "formun ilk gönderimi alanları doğrular ve hiçbir şey yazmadan … onay hâliyle yeniden gösterir"; UC-172 422: "dört zorunlu alandan biri boş ya da girilmiş adres boşaltılıyor; il kapalı listenin dışında; telefon biçimi". Doğrulayıcının geçici sınaması: adres girilmiş, sürüm 2, (boş alıcı / liste dışı il / telefon "12") onaysız → üçünde de 200. KK10'un 422 testleri yalnız ilk girişi (adres boşken, onay yolu yok) sınar. Düzeltme: onay hâlinden önce aynı kuralla alan denetimi (kural tek evde kalsın — yazıcının denetimi okuma yolu olarak açılır, sayfa yeniden yazmaz); KK10 testi girilmiş adresin üç geçersiz değişikliğini onaysız gönderimle sınar (422, onay hâli yok, yazma yok). Ödeme yöntemlerinde aynı açık yok: bozuk IBAN `ValidationPageFilter`'da, havale açıkken boşaltma onay koşulunun dışında 422 alır. |
| Bloke-etmeyen not (tur 1) | 3 — **K1** (forward D-1479): havale kapalıyken dolu IBAN'ı boşaltmanın onay cümlesi ve düğmesi K-814'te yok, kod K-814 kalıbından türetti. **K2** (forward D-1480): IBAN değişikliği + havale kapatma birlikte (`VAL-A752`) davranışı doğru (geçici sınama: 200, üç cümle sayı 1 ile, onaylı 303, havale kapalı, bekleyen sipariş eski IBAN'lı; aynı değerle kayıt yazmaz), kalıcı testi yok. **K3** (bu PR'da kapatıldı): D-681 ve D-220 kapanış cümleleri iade adresinin de `RegenerateWhenFedAsync`/"metin aynı kalsa da" yolundan geçtiğini söylüyordu; iade adresi `SaveReturnAddressAsync` + metin karşılaştırmasıyla sürüm doğurur (adresin bütün parçaları metinde olduğundan sonuç aynı) — iki satırın cümlesi düzeltildi. |
| Doğrulanan iddialar | CI düzeltmesi `b7bc9496` yalnız `FirmSettingsTests.InitializeAsync`'e iki metnin yayımlanmış sürümünü kuran düzeneği ekler; beklenen değer (2 sürüm iz satırı) gevşetilmedi. Kargo ücreti ve ödeme yöntemi kuralları tek evde (`FirmSettings`, `PaymentMethodSettings`); sayfa tutar hesaplamaz. İade adresi iz satırı değişen parçaları eski ve yeni değerle (il plaka koduyla) yazar; `SalesOpeningRules` yalnız doluluk okuduğu için plaka kodu geri sarmayı bozmaz. Kapatılan kalemler (D-833, D-165, D-176, D-220, D-256, D-681, D-1377; D-493 sürüm payı) testleriyle karşılandı. D-1444 ⚠: onay sayısı K-814'ün "işaretli kalem" kuralına uygun; ana sayfa sayacı değişmedi, ayrışma F5 kapısına yazıldı — uygun. Aynı değerli ücret ve iade adresi kaydı sürüm ve iz yazmaz, gerçek ücret değişikliği iki sürüm yazar (geçici sınama). |
| Yapım raporu karşılaştırması (tur 1) | 1 uyuşmazlık: öz denetim kriter 10 ve KK10'u "422" için ✓ verir, kanıt yalnız ilk giriş yolunu sınar — girilmiş adresin geçersiz değişikliği onaysız ilk gönderimde 422 almıyor (B1). Öteki iddialar uyumlu (KK1–KK9 testleri, kapatılan backlog kalemleri, `[OutcomeUncertain]` UC-167/UC-172, D-1443/D-1444 forward'ları). |
| Doğrulama durumu (tur 2) | ✓ PASS (2026-11-14; tam yeniden doğrulama, dal ucu `c39d6399`, main `3fe7d829` birleşik; kayıt öncesi main `ce300c0e` (T510) çakışmasız birleştirildi — birleşik ağaçta build 0 uyarı, format ×3 temiz, Unit 7567/7567, Integration PaymentAndShippingPages + CheckoutPage + OrderConfirmation 103/103, `--locked-mode` 0). Build `-warnaserror` 0 uyarı 0 hata, format ×3 temiz, Unit 7557/7557 (`AddressTests`/`MembershipTests` sahteleri derlendi; Consistency, Architecture dahil), betik testleri 4/4, hedefli Integration 263/263 (PaymentAndShippingPages, FirmSettings, SalesGate, Address, Membership) + 245/245 (LegalText, HomepageAndIdentityTrail, FirmSettingDefaults, CompanyIdentity, AuditTrail, PanelHome, OrderConfirmation, WithdrawalDeclaration, ItemRecords, HomeAndFirmPage) — TAM: CI; CI run 43555636021 (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate) yeşil. Kabul kriterleri 1–10, KK1–KK10, `09 §25` ilgili satırlar ve güvenlik taraması ✓. |
| Tur 1 bulgusunun kapanışı (tur 2) | **B1 kapandı:** `ShippingAndDefaultsModel.OnPostSaveReturnAddressAsync` girilmiş adresin değişikliğinde onay hâlinden önce `ISettingsModule.CheckReturnAddressAsync` → `CompanySettingsWriter.CheckReturnAddressAsync` çağırır; kural tek evde (kayıt da aynı yöntemi kilit altında koşar, sayfada kural kopyası yok). `AnInvalidChangeOfTheEnteredReturnAddress_Is422WithTheFieldMessageOnItsFirstSend_…_KK10` üç hâlde (boşaltılan alıcı → `FilledFieldCleared`, liste dışı il → `InvalidProvince`, telefon "12" → `InvalidPhone`) 422 + alan mesajı, onay cümlesi yok, sürüm ve adres değişmez; tersi (geçerli değişiklik) 200 onay hâli. D-1480 kapandı (`ChangingTheIbanAndClosingTheTransferTogether_…_A752` kalıcı test, yeşil). D-1479 F5 kapısında doğru: boşaltmanın cümlesi/düğmesi belge kararı (K-814 / `04 §9.17` / `07 §9.17.2`) — sahip kararı, kod davranışı belgeyle çelişmez. main birleştirmesi: `OutcomeUncertainConsistencyTests.NotMarkedYet` UC-167/UC-172 çıktı (`[OutcomeUncertain]` iki işleyicide), D-681/D-683 satırlarında çakışma işareti yok, D-683 main sürümüyle aynı. |
| `12` satırları (tur 2) | Değişiklik yok: tur 1'de PASS yazılan satırlar (`VAL-D032`, A290, D033, A539, A750–A756, A869) tur 2 kanıtıyla yeniden doğrulandı — A752 artık kalıcı testle; A288, A289, A291–A296, A312, A757, A871 ve tarayıcı satırları B087, B088, B103 BEKLEMEDE kalır (vitrin/sepet/sipariş sayfası, genel altyapı ya da tarayıcı payı). `12_matris_denetle.py --kapsam 1-77`: Hata 0. |
| Bulgu sayısı (tur 2) | 0 · K-notu 0 |
| Yapım raporu karşılaştırması (tur 2) | Tam uyumlu: öz denetim tablosu her kabul kriteri ve KK için kanıtlı; KK10 422 kanıtı artık girilmiş adresin değişikliğini de sınıyor. |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T536-odeme-kargo-varsayilan`
- **PR:** PR açıklamasında
- **CI:** *(PR'da)*
- **Post-merge run'lar:** main `8a51cdcc`: CI 43560690926 success, Publish 43560690928 success

## ⚠ Maddeler

- İki onaylı değişiklik (IBAN değişimi + havale kapatma) birlikte gelince iki cümle gösterilir, düğme "Havaleyi kapat".
- n=0'da kapatma cümlesinin ikinci cümlesi düşer; iade adresi onayında n=0'da cümle aynen kalır.
- P-8 çit metni "İndirme hakkı N'den küçük olamaz." K-817'de yok, `04` 9.18.12'den alındı.
- Havale kapalıyken dolu IBAN'ın boşaltılması onay ister (07 9.17 "dolu IBAN'ın değişmesi onay ister", K-1194 (2)); cümle K-814 kalıbından türetildi ("IBAN silinecek; ödemesi beklenen N havale siparişi eski IBAN'ı göstermeye devam edecek."), düğme "IBAN'ı değiştir", bildirim yok (silinen IBAN F-5 değil).
- Havale açıkken IBAN boşaltma `FilledFieldCleared` değil `BankTransferIbanRequired` kaydı yazar (VAL-A539, 07 6.2.8); metin katalogdan "IBAN boşaltılamaz."
- KK1 dört hâl testi kurulumdan bağımsız (sabit kart koşulu, `ConfigureTestServices`).
- KK10 sayısı yalnız "iade malı bekleniyor" işaretli kalemler; ana sayfa sayacı (T382) daha geniş kümeyi sayar, değişmedi — D-1444 (F5 kapısı).
- `CardNotDefinedText` → `CardUndefinedText` (kart verisi adı taraması).
- Betikli "Tümünü seç / Seçimi kaldır" yazılmadı (JS yok) — D-1443 → T539.
- D-493 yalnız sürüm payı karşılandı; şablona il listesi belirteci eklenmesi sahip kararı olarak açık kalır.

## Known Limitations / Follow-up

- D-1443 (betikli il düğmeleri → T539), D-1444 (ana sayfa sayacı işaretten geniş → F5 kapısı), D-493 kalan payı (sahip kararı).

## Notlar

- **Düzeltme 1 (CI Integration kırmızısı, run 43513685960):** `FirmSettingsTests.SaveShippingFee_WritesTheSection_StepsOnlyItsVersion_AndWritesOneTrailRowWithTheChangedFields` 2 metin sürümü iz satırı beklerken 0 aldı. Kök neden test düzeneği: sınıf iki üretilen metnin yayımlanmış sürümünü kurmuyordu; test, sınıfın öteki testlerinin (ör. süre kaydı) bıraktığı sürümlere dayanıyordu. Tek başına ya da önce koşunca ücret kaydının doğurduğu sürüm türün ilk sürümü olur — `02 §3.33.3` "yeni şablonun ilk kullanıldığı anda" doğan sürüm sistem olayıdır, ize yazılmaz (`LegalTextGenerator`: `last is null` → `IsTemplateChange`); kod belgeye uygun, değişmedi. Düzeltme: `InitializeAsync` uygulama açılışının bıraktığı durumu kurar (`SynchronizeGeneratedLegalTextsAsync`); iddia gevşetilmedi. Yeni test yok. main `49ba60f0` (T518) çakışmasız birleştirildi.
- **Düzeltme 2 (doğrulama tur 1 B1, ORTA S1; KK10, UC-172, `07 §2.2.4`, VAL-A597):** iade adresi değişikliğinin ilk gönderiminde alan denetimi. `ShippingAndDefaultsModel.OnPostSaveReturnAddressAsync` girilmiş adresin değişikliğinde onay hâlini alanları doğrulamadan veriyordu; boş zorunlu alan, liste dışı il ve bozuk telefon onaysız ilk gönderimde 200 onay hâli alıyordu. Şimdi onay hâlinden önce yazıcının aynı denetimi çağrılır (`ISettingsModule.CheckReturnAddressAsync` → `CompanySettingsWriter.CheckReturnAddressAsync`; kayıt yolu da aynı yöntemi kilit altında çağırır, ikinci kopya yok) ve ret, kaydın 422'siyle aynı yoldan (`AnswerAsync`, alan mesajı) döner; onay hâli yalnız alanlar geçerliyse gösterilir. Yeni testler: `AnInvalidChangeOfTheEnteredReturnAddress_Is422WithTheFieldMessageOnItsFirstSend_NotTheConfirmation_AndAValidChangeIsAsked_KK10` (üç geçersiz değişiklik 422 + alan mesajı, onay cümlesi yok, sürüm sabit; ters test: geçerli değişiklik onay hâli) ve K2/D-1480 için `ChangingTheIbanAndClosingTheTransferTogether_AsksBothSentencesWithTheWaitingOrder_AndTheConfirmedSendClosesTheSale_A752`. K1 (D-1479) belge/sahip kararı, F5 kapısında kalır; K3 tur 1'de kapatıldı.
- **Yapım beş turda bölündü (süre).** Son tur: main `edf4eb2a` birleştirildi (tek çakışma `OutcomeUncertainConsistencyTests.NotMarkedYet` — main UC-27'yi, dal UC-167/UC-172'yi çıkarmıştı; ikisi de çıktı), derleme, PaymentAndShippingPagesTests, Unit TAM, format, kayıtlar ve PR.
