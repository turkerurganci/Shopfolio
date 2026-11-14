# T536 — Ödeme yöntemleri, kargo, süreler ve varsayılanlar (E-45, E-46)

**Faz:** F5 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-14

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
- Testler: `tests/Shopfolio.IntegrationTests/Web/PaymentAndShippingPagesTests.cs` (yeni, 44), `Model/FirmSettingsTests.cs`, `Model/SalesGateTests.cs` (iz satırı kurulumu); `tests/Shopfolio.UnitTests/Consistency/OutcomeUncertainConsistencyTests.cs` (UC-167, UC-172 listeden çıktı)
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
| 10 | İlk giriş onaysız, değişiklik onay hâli; 422; beyan adresi sabit; onayda beklenen iade sayısı | ✓ | `TheReturnAddress_FirstEntryIsSavedWithoutAConfirmation_AChangeAsksItWithTheAwaitedItems_AndTheConfirmedSendSaves_KK10`, `TheReturnAddressSave_RefusesAnEmptyRequiredPartAProvinceOffTheListAndABadPhone_KK10`, `TheReturnAddressConfirmation_CountsTheItemsMarkedAwaitingTheReturnGoods_NotTheOtherAwaitedOnes_KK10` (n=2); beyan adresi: `CompanyIdentityTests.Withdrawal_KeepsTheReturnAddressItCopied_WhenTheSettingChangesLater` |

### Öz denetim (bitiş kapısı 2)

Önceki dört yapım turunun yaptıkları dahil bütün maddeler diff'te yeniden denetlendi. Düzeltme 1'de (main `49ba60f0` birleşik ağaç) tablonun bütün maddeleri yeniden denetlendi: kanıt testlerinin hepsi dalda duruyor ve yeşil (PaymentAndShippingPagesTests 44/44, CompanyIdentityTests ve FirmSettingsTests yeşil); hedefi T536 olan açık backlog kalemi yok (D-165, D-176, D-220, D-256, D-681, D-833, D-1377 kapalı; D-493 sürüm payı).

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
| KK10 422; beyan adresi sabit; beklenen iade sayısı | ✓ | KK10 testleri (n>0 hâli `…_KK10` sayım testi) |
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

- (1) Bekçi/tarama istisnası eklenmedi. (2) Yeni tarama yazılmadı; var olan taramalar (`[SpecAssignedName]`, D-82 lambda, kart verisi adı) düzeltilerek geçirildi. (3) Şema değişmedi. (4) KK10 sayısı ile ana sayfa sayacı ayrıştı; sayacın kendisi değişmedi, ayrışma D-1444'e yazıldı. (5) 09 metni değişmedi. (6) Düzeltme 1: test sırasına bağımlılık (koleksiyon içi yalıtım) giderildi — düzenek bilinen durumu kendisi kurar.

## Test Sonuçları

| Tür | Sonuç | Komut | Not |
|---|---|---|---|
| Build | ✓ | `dotnet build -warnaserror` | düzeltme 1: main `49ba60f0` birleşik ağaçta 0 uyarı 0 hata (önce `edf4eb2a`) |
| Unit | ✓ TAM | `dotnet test --project tests/Shopfolio.UnitTests` | düzeltme 1: Consistency 295/295 + kalan 7262/7262, betik testleri 4/4 (önce 7546/7546) |
| Integration | ✓ hedefli — TAM: CI | `--filter-class <ad>` | düzeltme 1: FirmSettingsTests + PaymentAndShippingPagesTests + SalesGateTests + LegalTextTests 116/116, CompanyIdentityTests + FirmSettingsTests 53/53; kırmızı test tek başına da yeşil (düzeltmeden önce tek başına 0 ≠ 2 ile kırmızıydı). İlk tur: PaymentAndShippingPagesTests 44/44 (birleşik ağaç); önceki turlarda Model 161 (FirmSettings, SalesGate, Address, SettingsTables, LegalText, CompanyIdentity, AuditTrail) + AuditTrail 22, Web/Admin 97 (AppCache, StartupDatabaseCheck, HomeAndFirmPage, PanelHomeAndVerificationPages, SecurityHeaders, Administration.PanelHomeTests) |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` | Web, UnitTests, IntegrationTests temiz; birleştirmede değişen `OutcomeUncertainConsistencyTests.cs` ayrıca temiz |

## Doğrulama

*(validator doldurur)*

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T536-odeme-kargo-varsayilan`
- **PR:** PR açıklamasında
- **CI:** *(PR'da)*
- **Post-merge run'lar:** *(validator doldurur)*

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
- **Yapım beş turda bölündü (süre).** Son tur: main `edf4eb2a` birleştirildi (tek çakışma `OutcomeUncertainConsistencyTests.NotMarkedYet` — main UC-27'yi, dal UC-167/UC-172'yi çıkarmıştı; ikisi de çıktı), derleme, PaymentAndShippingPagesTests, Unit TAM, format, kayıtlar ve PR.
