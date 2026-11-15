# T547 — Durum × rol matrisi ve form envanteri eşitlik denetimleri

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-15

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Durum × rol matrisi (kriter 1, 3, 4, 5, 8):** `StateRoleDocument`, `StateRoleInventory`, `StateRoleCheck`, `StateRoleConsistencyTests` — 53 ekranın blokları, iki kapı, durum adları, Yönetici satırı sınırı, panel tek rol ve `/panel` kökü.
- **Form envanteri, iki yönlü kural ve zorunluluk (kriter 6, düzeltme 1):** `FormFieldEnforcement` (eşleme tabloları, karşılaştırma), `FormFieldEnforcementTable` (işlem satırları), `FormFieldEnforcementTests`.
- **Form envanteri (kriter 6, 7):** `FormInventoryCheck` (form blokları, satır numaraları, ekran tarafı, kural kimlikleri, değer yasağı, ürün sabiti okuma) ve `FormFieldInventory` + `FormFieldCheck` (her alan satırı = kodda bir yer, gereklilik sözcüğü, kural ve sınır kimlikleri). Bu turda: çapa kanıtı tam tanımlayıcı olarak aranır (`FormFieldCheck.Holds`).
- **Sayfa testi (kriter 2, KK2):** `StateRolePageTests` — 6.2.16.1–10 ve 6.3.9.1–10 çiftleri üç tohumla (kayıtsız · kayıtlı · açık IBAN isteği); müşteri sayfası ve panel ayrıntısı `IOrderOperations` ile birebir.
- **Düzeltme 3 (doğrulama tur 2):** iki yerli satırlarda nitelikler yer başına denetlenir (`FormFieldPlaceMarks`, `PlaceMarksDiffer`); kategori adı tavanı P-27 olarak geri geldi (K-1619).
- **Belge:** `04 §7`'nin 10 kural hücresi kimlikle gösterir; karar kaydı K-1618 (⚠). Düzeltme 1: `04 §7.2.5.1` ve `§7.2.6.2` kural hücreleri kimlik aldı; K-1619 (⚠).

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/` — `StateRole*.cs` (4), `FormInventoryCheck.cs`, `FormFieldCheck.cs`, `FormFieldInventory.cs`, `FormFieldEnforcement.cs`, `FormFieldEnforcementTable.cs`, `FormFieldEnforcementTests.cs`, `FormInventoryConsistencyTests.cs`.
- `tests/Shopfolio.IntegrationTests/Web/StateRolePageTests.cs`.
- `Docs/04_UI_SPECS.md` (§7 kural hücreleri, sürüm satırı), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1618, sürüm satırı), `Docs/STATUS_CHANGELOG.d/T547.md`, `Docs/IMPLEMENTATION_STATUS.md` (satır 306). Düzeltme 3: `src/Shopfolio.Web/Pages/Panel/Categories.cshtml(.cs)` ve `ProductConstants.cs` (özet yorumu) — kategori adı tavanı P-27; `FormFieldPlaceMarks.cs`, `FormFieldPlaceMarksTests.cs` (yeni), `tests/Shopfolio.IntegrationTests/Web/CategoriesPageTests.cs`; `Docs/02` P-27, `04` 7.2.5.1 / 9.6.8 / 7.3.2.14, `06 §4.9`, `07` 9.6.2–9.6.3, `09` P-27 yorumu.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 53 ekranın bloğu | ✓ | `StateRoleMatrix_EqualsTheCode_InBothDirections`; `Block_DeletedFromTheDocument_IsReported`, `PanelBlock_DeletedFromTheDocument_IsReported`, `Document_HasBothFamiliesOfBlocks_AndTheInventoryHasThem` |
| 2 | Her çiftte gösterilen ve açık işlemler sayfa testiyle; aralık dışında düğme hiç yok | ✓ | `StateRolePageTests` iki test × (kayıtsız, kayıtlı, IBAN istekli); müşteri bağlantıları + IBAN formu + indirme düğmesi; panel 27 işlem; pasif denetim yok (`Disabled()`). Deney: `_OrderDetailRefunds.cshtml` `amountFormOpen = true` → 59 satırda "ProcessRefund is outside its range"; `_OrderDetailItemSteps.cshtml` talep süzgeci `false` → 26 satırda ResolveOrReopenDefectClaim; `_OrderIbanForm.cshtml` kart alanı gizlenince → 23 satırda IBAN formu (geri alındı) |
| 3 | Satış kapısı yalnız E-04, E-12, E-13; veri kapısı yalnız E-10, E-20, E-22 | ✓ | `SalesGate_ChangingAnotherScreen_IsReported`, `SalesGate_ReadByAnotherScreensPage_IsReported`, `DataGate_*`, `GateSentence_ChangedInTheDocument_IsReported`; belirteçler `StateRoleInventory.SalesGateTokens/DataGateTokens` (satış kapısı 4, veri kapısı 3 çağrı biçimi) |
| 4 | Durum adları `02 §5`'inkidir, eş anlamlı yok | ✓ | `StatusBadgeText_OfTheCode_EqualsTheNameOf02`, `SynonymOfAStatusName_IsReported` |
| 5 | Yönetici satırı yalnız taslak ekranlarda | ✓ | `AdministratorRow_*`, `DraftRead_*`; E-06/E-07 "sonuç" ekranları muaf değil, olumsuz kanıtlı: kodda taslak belirteci taşımamaları `DraftReadDiffers` ile denetlenir (kümeler eşit olmalı) |
| 6 | Form envanteri: alan, zorunluluk, kural kodla eşit | ✓ | `FieldInventory_EqualsTheDocumentsAndTheCode`, `Field_DeletedFromTheCode_IsReported`, `Requirement_*`, `Rule_*`, `FieldMarkedRequiredInTheCode_ThatTheRowCallsOptional_IsReported`, `FieldWithAnAttribute_WhoseRuleTheRowDoesNotName_IsReported`; `FormFieldEnforcementTests` (iki yön: satır → kod ve kod → satır; kural, sınır, zorunluluk; eşlenmeyen tür kırar); satır-satır deney aşağıda |
| 7 | Kural değeri yazmaz, kimlikle işaret eder; ürün sabiti `ProductConstants`'tan | ✓ | `Rule_WritingAValue_IsReported`, `Values_AreTheNumbersThatAreNotIds`, `ProductConstant_NoValidationReads_IsReported`, `LengthAttribute_WritingTheNumber_IsReported`; 10 hücre K-1618 ile kimliğe çevrildi |
| 8 | Panel tek rol; `/panel` altında | ✓ | `PanelRow_OfAnotherRole_IsReported`, `PanelPage_OutsideTheRoot_IsReported`, `PagePanelRoute_OutsideThePanelFolder_IsReported`, `PanelFolder_NotClosedByThePolicy_IsReported`, `PanelForm_PostingOutsidePanel_IsReported`, `CustomerForm_PostingToThePanel_IsReported` |
| 9 | 2 dışındakiler `Consistency` Unit; 2 Integration | ✓ | `tests/Shopfolio.UnitTests/Consistency/`; `[Trait("Consistency","true")]` ile 622 test; `StateRolePageTests` Integration |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 ekran bloğu eksilince kırılır | ✓ | `Block_DeletedFromTheDocument_IsReported` |
| KK2 işlem düğmesi aralık dışında bulununca kırılır | ✓ | `StateRolePageTests` — yukarıdaki üç deney; kayıt gerektiren işlemler (iade, IBAN, ayıp talebi, e-posta, indirme) tohumla eşlendi, D-1648'e atılmadı |
| KK3 iki kapı yalnız altı vitrin ekranını değiştirir | ✓ | `SalesGate_*`, `DataGate_*` |
| KK4 eş anlamlı durum adı kırar | ✓ | `SynonymOfAStatusName_IsReported` |
| KK5 Yönetici satırı sınırı | ✓ | `AdministratorRow_InAScreenOutsideTheList_IsReported`, `…OfAListedScreenDeleted…`, `…OfE05_ThatShowsSomethingElse…` |
| KK6 alan silinince ya da kural değişince kırılır | ✓ | `Field_DeletedFromTheCode_IsReported`, `Field_RenamedWhileALongerNameHoldsItsName_IsReported`, `Rule_ChangedInTheDocument_IsReported`, `Attribute_RemovedFromTheProperty_IsReported`, `Requirement_RemovedFromTheCode_IsReported`, `Requirement_AddedToTheCode_IsReported`, `EveryEnforcementLine_RemovedFromTheCode_IsReported`; gerçek `src` deneyi: 21/21 çapa, 194/194 özellik yeri `FieldNotInCode`; kural ve zorunluluk: 182/182 işlem satırı silinince 127/127 satır, 75 nitelik silinince 40/40 satır, nitelik türü değişince raporlandı |
| KK7 kimlik yerine sayı / `ProductConstants` yerine sayı kırar | ✓ | `Rule_WritingAValue_IsReported`, `ProductConstant_NoValidationReads_IsReported` |
| KK8 `/panel` dışı panel formu ya da işlemi kırar | ✓ | `PanelForm_PostingOutsidePanel_IsReported` |
| KK9 testler `UnitTests/Consistency/` altında | ✓ | dosya yolları |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | `src` değişmedi; sır bekçisi yanlış pozitifleri için `Concealed` ve `cancel =>` adları kullanıldı |

Düzeltme 1 öz denetimi (bütün maddeler yeniden): kabul kriterleri 1–9 ve KK1–KK9 testleri yeşil (Consistency 634/634, Unit diğer 7420/7420); değişen yalnız 6 ve KK6 (iki yön, kural ve zorunluluk, açık eşleme tabloları, eşlenmeyen tür kırar) ile `04 §7.2.5.1`, `§7.2.6.2` hücreleri (K-1619). 7 ve KK7 (kimlik, ürün sabiti okuma) değişmedi, testleri geçti. Quantity kuralını taşıyan zorunlu alanda boş değer "1 den küçük" olarak reddedildiğinden adet kuralı zorunluluğun kanıtı sayılır (`FormFieldEnforcement.Problems`; ⚠ kolay karar).

### Düzeltme 2 (doğrulama tur 1: B1–B5)

| Bulgu | Düzeltme | Kanıt |
|---|---|---|
| B1 (KK6) | `@@@` ve 12 satırlık pencere kaldırıldı: satırın kanıtı tek parça koddur (boşluklar tek boşluk; çok satırlı deyim tek parça), dosyada tam bir kez bulunur (`EnforcementAmbiguous`) ve her satırın başka formun satırının kullanmadığı kendi parçası vardır (`EnforcementNotDistinctive`). 64 satır yeniden yazıldı; sınır satırlarında formun kendi çağrısı (kayıt `L-3`, kayıt bağlantısı yeniden isteği Register/SignIn sayfası `L-9`, giriş `CheckPasswordAsync`, sıfırlama `RequestResetAsync`, yeniden doğrulama `ReauthenticateAsync`, e-posta değişikliği `EmailChanges`/`AdminEmailChanges`) ve ortak sayım satırı (`EmailChangeRules`, `PasswordRegime`) birlikte; "Beni hatırla" `SignInAsync(..., input.IsLongLived, ...)`; adres kuralları `CheckAddress(` çağrısının kendi öneki; zorunlu liste `CommonFields =` öneki | `EveryEnforcementLine_RemovedFromTheCode_ReportsExactlyTheRowsThatHoldThatCode` (her parça gerçek kaynaktan çıkarılır; raporlanan satırlar tam o koda sahip satırlardır), `ChangeOfALimit_OfAFlowsQuantityCheck_AndOfARequiredList_ReportsTheRowsOfThatFlow`, `EnforcementLine_HeldTwiceInItsFile_IsReported`, `Row_WithNoLineOfItsOwn_IsReported`, `EveryRowOfTheTable_HasALineOfItsOwn`. Gerçek `src` deneyleri (betikle, geri alındı): (a) `EmailChangeRules` L-9 → L-3: 7.1.15.2, 7.1.15.4, 7.2.24.2, 7.2.24.4 kırıldı; (b) `ArriveAsync` adet denetimi (371) kaldırıldı: yalnız 7.2.12.2 kırıldı, 7.2.11.2 kırılmadı; (c) 7.2.5.1 artık tavansız (B2) — tavanı geri koymak `RuleMissesAttribute` (`Check_TheRowDoesNotName_IsReported`, `LengthAttribute_WritingTheNumber_IsReported`); (d) `CommonFields` listesinden `ProvinceId`, `Phone`, `Email` çıkarıldı: 7.2.19.3, .4, .5, .6 kırıldı |
| B2 (KK7, K-1619) | **(Düzeltme 3'te geri alındı: C2 — kategori adı tavanı P-27, kaynak `09` 9.4 madde 3.)** Kaynak arandı (`02` P-28 kapsamı, `06 §4.9`, `04` 9.6.8, T526 raporu): kategori adı tavanı hiçbirinde yok. 7.2.5.1 hücresi eski "Kısa metin"e döndü; koddaki tavan kaldırıldı (`Categories.cshtml.cs` iki `TextTooLong`, `Categories.cshtml` üç `maxlength`); 7.2.5.1 satırının kural kimliği boş. K-1619 buna göre yeniden yazıldı | `Docs/04_UI_SPECS.md`, `Docs/PRODUCT_DISCOVERY_STATUS.md` K-1619; `FieldInventory_EqualsTheDocumentsAndTheCode` |
| B3 (K-1618) | 7.2.4.13 hücresi "ondalık ölçeği K-1013'tedir (`06` `NetQuantity`), ayırıcı biçimi 8.2.3'tedir"; denetimde `K-1013` → `NetQuantity`, `8.2.3` → hiçbir denetim türü (yalnız biçim) | `NetQuantityScale_IsTheIdOfK1013_AndTheSeparatorFormatNamesNoCheck` |
| B4 | 7.3.2 "Kullanıldığı": 7.3.2.10 ve 7.3.2.16 için `7.2.6` eklendi; 7.3.2.14 (7.2.5.1 geri alındığı için) değişmedi | `Docs/04_UI_SPECS.md` |
| B5 (KK2) | `Always` kümesi kalktı: 27 işlemin hepsi iki yönlü (açık ⇒ çizildi, aralık dışı ⇒ çizilmedi). Yeni tohumlar `Records.Returns` (teslim edilmiş kalem, koşullu muafiyet, 35 gün önceki cayma, kargo geçiş kaydı) ve `Records.Aged` (120 günlük sipariş: gecikme feshi). Hiç açılmayan ya da açıkken hiç çizilmeyen işlem testi "hiç açılmadı" diye kırar. Deney: `_OrderDetailCorrections.cshtml` `email = false` → `CorrectGuestEmail is open but the page draws none` (120 durum), geri alındı | `StateRolePageTests` 2/2 |

Düzeltme 2 öz denetimi (bütün maddeler yeniden): 1–9 ve KK1–KK9 testleri yeşil (Consistency 639/639; Unit diğer 7420/7420; `StateRolePageTests` 2/2). Kural ve zorunluluk satırlarının kanıtı şimdi ayırt edici: her satırın deneyi `EveryEnforcementLine_RemovedFromTheCode_ReportsExactlyTheRowsThatHoldThatCode` ile bütün işlem satırlarında (126 satır) gerçek kaynak metninden parça çıkarılarak yapılır ve yalnız o koda sahip satırlar raporlanır; nitelik satırları `EveryRuleAttribute_TakenOffTheProperty_IsNoLongerEvidence` ile. Hedefi T547 olan backlog kalemi yok (D-1656, D-1657 doğrulama notlarıdır, ileri).

| Bölüm | Satır grubu | Kanıt | Ayırt edici mi | Gerçek `src` değişimi test kırdı mı |
|---|---|---|---|---|
| `04 §7.1` işlem satırları (iletişim, ödeme, sipariş, adres) | 7.1.1.3, 7.1.2.x, 7.1.3.x, 7.1.5.x, 7.1.6.x, 7.1.7–7.1.10, 7.1.16.x | alanın kendi çağrısı (örn. `CheckAddress( ... ShippingPrefix, request.ShippingRecipientName,`) ve sınırın `RecordAsync(AttemptLimit.Lx, ...)` | evet — dosyada tek, başka formun satırı değil | evet: parça çıkarma deneyi satır başına; (a) `EmailChangeRules` |
| `04 §7.1` giriş, kayıt, sıfırlama, hesap | 7.1.11.x–7.1.15.x | formun kendi çağrısı + ortak sayım satırı | evet — kendi satırı var | evet: (a) |
| `04 §7.2` panel giriş ve hesap | 7.2.1.x, 7.2.24.x, 7.2.26.1 | `AdministrationModule` kendi çağrısı + `EmailChangeRules`/`PasswordRegime` | evet | evet: (a) |
| `04 §7.2` ürün ve içerik | 7.2.3.x–7.2.6.x, 7.2.16.x–7.2.18.x | alan ve hata kodu aynı parçada; yükleme `FormatMessage`/`SizeMessage` formun kendi çağrısı | evet | evet: parça çıkarma deneyi |
| `04 §7.2` sipariş ayrıntısı | 7.2.7.x–7.2.14.x | işleyici imzasından denetime kadar tek parça (`OnPostRecordWithdrawalAsync(` ... `QuantityInvalidText`); `ArriveAsync` ile `OnPostCloseNotReturnedAsync` ayrı | evet | evet: (b) yalnız 7.2.12.2 |
| `04 §7.2` firma kimliği ve ayarlar | 7.2.17.x, 7.2.19.x, 7.2.21.x | `CommonFields =` öneki, `TradesmanFields`, altı menü adı tek parça | evet | evet: (d) |

### Sık FAIL nedenleri (öz denetim)

| # | Neden | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Bekçi/tarama istisnası geniş | ✓ | Muaf tutulan tek şey E-06/E-07'nin taslak okuması ve bu, kodda belirteç yokluğuyla denetlenir (`DraftReadDiffers`); alan çapasında istisna yok |
| 2 | Kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Çapa kanıtı bu turda zayıf bulundu (alt dize): `Holds` tam tanımlayıcı ister, `Field_RenamedWhileALongerNameHoldsItsName_IsReported`; kapı belirteçleri 4+3 biçim |
| 3 | 06 varsayılanı / FK dizini | ✓ (kapsam dışı) | göç ve şema değişmedi |
| 4 | Kural değişince aynı sayıyı anan öteki yerler | ✓ | `04` sürüm satırı (satır 5307) ve `PRODUCT_DISCOVERY_STATUS.md` satır 48 K-1618 ile güncellendi; `04` v0.55 kalır; düzeltme 1: K-1619 (`04` sürüm satırı ve `PRODUCT_DISCOVERY_STATUS.md` satır 48 güncellendi) |
| 5 | Belge kuralını değiştiren metin karar kaydısız | ✓ | K-1618 (10 hücre; anlam değişmedi); düzeltme 1: K-1619 (7.2.5.1 ve 7.2.6.2 kural hücreleri kimlik aldı) |

### T546 emsali: her satır için ayırt edici kanıt ve gerçek `src` deneyi

| Bölüm | Satır | Kanıt | Ayırt edici mi | Gerçek `src` değişimi test kırdı mı | Kural ve zorunluluk, iki yön (satır → kod, kod → satır) |
|---|---|---|---|---|---|
| `04 §6.2` / `§6.3` blokları | 53 ekran + panel blokları | ekranın rotası sayfa dosyasına bağlı (`StateRoleInventory`, rota başına) | evet — rota kod dosyasında aranır, iki yönlü | `Screen_WhosePageLeavesTheCode_IsReported` (belgesel) ; sayfa testi her çift için | blok düzeyi; alan kuralı yok |
| `04 §7.1` ve `§7.2`, çapa yeri (21 satır: 7.1.1.1–3, 7.1.12.1–4, 7.1.13.1–2, 7.1.14.1, 7.1.15.4, 7.2.2.1, 7.2.9.2, 7.2.10.2, 7.2.10.4, 7.2.11.1–2, 7.2.12.2, 7.2.13.1, 7.2.24.4, 7.2.25.1) | çapa satırı başına | `dosya#çapa`; çapa özellik adı ya da çağrı biçimi (`TryReadUnits(input.Items`, `Fields.Email("Input.Email"`) | evet — kelime sınırlı | evet: `t547-mutate.py anchors` her çapayı gerçek dosyada yeniden adlandırdı; 21/21 satır `FieldNotInCode` (deney betiği geçici klasörde, geri alındı) | çapa satırının kural ve zorunluluğu `FormFieldEnforcementTable` satırlarıyla ayrıca eşlenir (aşağıdaki son satır) |
| `04 §7.1` ve `§7.2`, özellik yeri (194 yer, 151 satır) | satır başına | `Ns.Tür.Özellik` yansıma | evet — tür ve özellik adı | evet: her özellik `public` → `internal` yapıldı (derlenir, yansıma `Public` bulamaz); 194/194 yer `FieldNotInCode`; ad çakışması (`Panel.ChangeNameInput.Name`, `Account` ile aynı ad) tek tek sınandı | nitelik → satır: `RuleMissesAttribute`, `RequirementContradictsCode`; satır → nitelik: `RuleWithoutEnforcement`, `RequirementWithoutEnforcement`; gerçek `src`: 75 nitelik satırı silindi → yalnız niteliğe dayanan 40/40 satır raporlandı; `[InvalidEmail]` → `[InvalidPhone]` → `RuleWithoutEnforcement` + `RuleMissesAttribute` |
| gereklilik sözcüğü, kural ve sınır kimlikleri | her satır | `RequirementDiffers`, `RuleDiffers`, `LimitDiffers` (satırın hücresi ↔ envanter) | evet | belge tarafı deneyleri: `Requirement_ChangedInTheDocument_IsReported`, `Rule_ChangedInTheDocument_IsReported`, `Limit_ChangedInTheDocument_IsReported`; kodda `FieldRequired`/`Invalid*` niteliği satırla çelişirse `RequirementContradictsCode`, `RuleMissesAttribute` | belge tarafı; kod tarafı için son satır |
| kural, sınır ve zorunluluğun kodda karşılığı (127 satır; 182 işlem satırı, 75 nitelik) | satır başına | nitelik (yansıma) ya da işlem satırı (`dosya#parça` ya da `alan@@denetim`: alanı adlayan parça ve hata kodu aynı satırda ya da 12 satır içinde); tür, kural ve nitelik eşleme tabloları açık | evet — alan parçası ve denetim simgesi birlikte; paylaşılan tek ifade yok (sınır satırı formun bekçisidir) | evet: 182 işlem satırının hepsi `src`'den silindi → 127/127 satır `EnforcementNotInCode`; nitelik silindi → 40/40; tür değişti (`InvalidEmail` → `InvalidPhone`) → raporlandı; testler: `EveryEnforcementLine_RemovedFromTheCode_IsReported`, `EveryRuleAttribute_TakenOffTheProperty_IsNoLongerEvidence`, `Attribute_RemovedFromTheProperty_IsReported`, `Requirement_RemovedFromTheCode_IsReported`, `KindOrRule_NotInTheTables_IsReported` | tablo kapalı: `EveryRuleAttributeOfTheCode_IsInTheKindTable`, `EveryFieldType_OfTheDocument_IsInTheRuleTable`; eşlenmeyen tür `RuleKindUnmapped` |

### Düzeltme 3 (doğrulama tur 2: C1, C2)

| Bulgu | Düzeltme | Kanıt |
|---|---|---|
| C1 (KK6) | Birden çok özellik yeri olan satırda her yerin nitelikleri ayrı okunur: `FormFieldPlaceMarks` (satır\|yer → nitelik türleri; tabloda olmayan yerin niteliği yoktur) ve `FormFieldCheck.PlaceMarkProblems` — bir yer niteliğini kaybederse ya da kazanırsa `PlaceMarksDiffer:<satır>: the place '<yer>' ...`; tablodaki yer envanterde çok yerli satırın özellik yeri değilse `PlaceMarksNotInInventory`. Satır düzeyi birleşim denetimi (zorunluluk, kural kapsamı, `RuleMissesAttribute`) aynen durur. Çok yerli satır: 31 (75 özellik yeri); nitelik taşıyan 8 yer pinli, öteki 67 yerin niteliği yok (denetimi işleyici satırı; kod → satır yönü D-1656'dadır) | `FormFieldPlaceMarksTests` (5 test): `EveryAttribute_TakenOffOnePlaceOfARowWithSeveralPlaces_IsReportedForThatPlace` (her pinli nitelik tek tek, gerçek envanter), `RequiredMark_OfTheCreateInput_AndDateMark_OfTheEndDate_AreReported_WhileTheOtherPlaceStillCarriesThem` (raporun iki deneyi), `AttributeGained_ByAPlace_AndAStrayPin_AreReported`, `EveryPlace_OfARowWithSeveralPlaces_CarriesItsPinnedMarks`, `RowWithOnePropertyPlace_NeedsNoPin` |
| C2 (K-1619) | Kategori adı tavanı P-27 (`ProductConstants.ProductNameMaxLength`) geri kondu: `CreateCategoryInput.Name`, `RenameCategoryInput.Name` `[TextTooLong(...)]`, `Categories.cshtml` üç `maxlength`. Kaynak `09` 9.4 madde 3. `04` 7.2.5.1 "Kısa metin — ad P-27 (7.3.2.14)", 9.6.8, 7.3.2.14 tanımı ve "Kullanıldığı" (7.2.5); `02` P-27 kapsamı "kategori adının" ile genişledi (§3.4.1, K-1619); `06 §4.9` `Category.Name` (sütun `text`, tavan uygulamada); `07` 9.6.2 ve 9.6.3 422 listesi; `09` P-27 yorumu; `ProductConstants` özet yorumu. K-1619 baştan yazıldı. Denetim tarafı: `FormFieldInventory` 7.2.5.1 kuralı `7.3.2.14 P-27`; `FormFieldEnforcementTable` iki P-27 satırı (iki girdinin nitelik parçası) | `CategoriesPageTests.ANameOverTheCeiling_IsA422WithTheFieldMessage_OnCreateAndRename_AndWritesNothing_T547_C2` (6000 karakter → 422 + "en çok 150 karakter olabilir.", yazı yok; tam tavandaki ad 303); `LengthAttribute_WritingTheNumber_IsReported` yeni çapaya uyarlandı |

Gerçek `src` deneyleri (betikle, hepsi geri alındı; her biri tüm Consistency koşusu):

| Deney | Sonuç |
|---|---|
| Raporun (1): `CreateCategoryInput.Name`'den `[FieldRequired]` | KIRILDI — `PlaceMarksDiffer:7.2.5.1 ... 'Panel.CreateCategoryInput.Name' carries [TextTooLongAttribute], the table pins [FieldRequiredAttribute, TextTooLongAttribute]` (Rename hâlâ taşırken) |
| Raporun (2): `SaveProductDiscountRow.EndDate`'ten `[InvalidDate]` | KIRILDI — `PlaceMarksDiffer:7.2.4.18 ... 'Panel.SaveProductDiscountRow.EndDate' carries []` (StartDate hâlâ taşırken) |
| Toplu, her pinli yerin ilk niteliği birlikte (8 yer, 5 satır): 7.2.4.13 `NetQuantity`, 7.2.4.18 `Percentage`, `StartDate`, `EndDate`, 7.2.5.1 Create ve Rename `Name` `FieldRequired`, 7.2.16.8 `StartDate`, `EndDate` (7.2.16.8 dahil) | 8/8 yer kendi satırıyla KIRILDI (her yer için ayrı `PlaceMarksDiffer` satırı) |
| 7.2.5.1 Create ve Rename `Name`'den `TextTooLong` (ikinci nitelik) | 2/2 yer KIRILDI (`PlaceMarksDiffer` + `RuleWithoutEnforcement` + `EnforcementNotInCode`) |

Çok yerli 31 satırda 8 yerin tamamı nitelik taşıyan yerdir ve hepsi yukarıda tek tek sınandı; öteki 67 yer nitelik taşımaz (silinecek nitelik yok), onların kural ve zorunluluğu işleyici satırıdır ve satır düzeyindedir: yeri adlayan parça ile denetlenir (kod → satır yönü D-1656'dır, dokunulmadı). Satırlar: 7.2.3.1, .2, .3, .5; 7.2.4.3, .5, .6, .9, .10, .13, .18; 7.2.5.1; 7.2.7.1; 7.2.8.1–.3; 7.2.10.1–.4; 7.2.13.2; 7.2.14.1–.3; 7.2.16.1, .2, .3, .7, .8, .9, .10.

Düzeltme 3 öz denetimi (bütün maddeler yeniden; `Docs/11` T547 kabul kriterleri ve `12`'nin ilgili kontrol listesi):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 ekran bloğu | ✓ | `StateRoleConsistencyTests` yeşil (değişmedi) |
| Kriter 2 / KK2 sayfa testi | ✓ | `StateRolePageTests` koşulmadı (kodu değişmedi); doğrulama tur 2'de 2/2; `Categories.cshtml` değişti → `CategoriesPageTests`, `CategoryTreeAndListsTests` 42/42 |
| Kriter 3 / KK3 iki kapı | ✓ | `SalesGate_*`, `DataGate_*` yeşil |
| Kriter 4 / KK4 durum adları | ✓ | `SynonymOfAStatusName_IsReported` yeşil |
| Kriter 5 / KK5 Yönetici satırı | ✓ | `AdministratorRow_*`, `DraftRead_*` yeşil |
| Kriter 6 / KK6 alan, zorunluluk, kural | ✓ | C1: yer başına nitelik (`FormFieldPlaceMarksTests`; gerçek `src` deneyleri yukarıda); satır düzeyi: `FormFieldEnforcementTests` yeşil |
| Kriter 7 / KK7 kimlik, ürün sabiti | ✓ | `Rule_WritingAValue_IsReported`, `ProductConstant_NoValidationReads_IsReported`; 7.2.5.1 kuralı kimlikle (P-27), değer `ProductConstants`'tan; `LengthAttribute_WritingTheNumber_IsReported` yeni çapada kırar |
| Kriter 8 / KK8 panel | ✓ | `Panel*_IsReported` yeşil |
| Kriter 9 / KK9 yer | ✓ | `UnitTests/Consistency/` (644 test, yeni 5 dosya içi test); Integration `CategoriesPageTests` |
| `09 §25` / güvenlik taraması | ✓ | `src` değişti: kategori adı tavanı; uzun ad artık 422 (kayıt tavanı), 500 yok; yeni bağımlılık, sır, yetki etkisi yok; sır bekçisi adları (`Concealed`, `cancel =>`) kullanıldı |
| Sık FAIL (1) istisna geniş | ✓ | yeni istisna yok; tablodaki olmayan yer "nitelik yok" sayılır ve kod gerçekte öyleyse eşleşir, fazla nitelik de `PlaceMarksDiffer` verir |
| Sık FAIL (2) çağrı biçimi kaçıyor | ✓ | yer başına nitelik yansımayla okunur (ek argüman, adlı argüman, Attribute soneki fark etmez: tür adı) |
| Sık FAIL (3) 06 varsayılanı / FK | ✓ (kapsam dışı) | şema değişmedi; `Category.Name` `text` kaldı |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | P-27 anan yerler arandı: `02` P-27, `04` 7.2.5.1, 9.6.8, 7.3.2.14 + Kullanıldığı, `06 §4.9`, `07` 9.6.2–9.6.3, `09` P-27 yorumu, `ProductConstants`; `04` sürüm dipnotu ve `PRODUCT_DISCOVERY_STATUS.md` satır 48, K-1619 |
| Sık FAIL (5) karar kaydısız belge kuralı | ✓ | K-1619 (kapsam genişletmesi P-27 → kategori adı) baştan yazıldı |
| T538 / T546 emsali: sessiz atlama yok, ortak ifade kanıt değil | ✓ | iki yerli satırda kanıt artık yer başına; tabloda olmayan ya da kaybolan yer sessiz kalmaz |
| Hedefi T547 olan backlog kalemi | ✓ | `grep T547`: yalnız D-1656, D-1657 (doğrulama tur 1 notları) ve D-1661 (tur 2 K1); üçü ileri, dokunulmadı |

### Düzeltme 4 (doğrulama tur 3: E1)

| Bulgu | Düzeltme | Kanıt |
|---|---|---|
| E1 (KK6; C1'in devamı) | Çok yerli satırda nitelik taşımayan her yerin denetimi artık o yerin kendi satırlarıyla kanıtlanır: yeni `FormFieldPlaceEnforcement` (anahtar `satır\|yer` → `Tür\|dosya#parça` satırları; boş giriş "bu yerde satırın adlandırdığı türden denetim yok" beyanı). İki işleyicide denetlenen alan iki ayrı satırdır (ör. 7.2.16.1 başlık: referans + genel sayfa). `FormFieldEnforcementTable.Lines` artık satırın kendi tablosu ile yer girişlerinin birleşimidir (satır düzeyi karşılaştırma değişmedi; tür bazında birleşim yalnız satırın "bu tür kodda var" sorusu içindir). `FormFieldCheck.PlaceLineProblems`: pinlenmeyen her özellik yerinin girişi yoksa `PlaceEnforcementMissing`; girişin her satırı dosyasında tam bir kez tutulmazsa `PlaceLineNotInCode:<satır>\|<yer>` (yer adıyla); envanterde olmayan yerin girişi `PlaceEnforcementNotInInventory`. Gerekli üç yeni "Zorunlu" belirteci (`carrier is null`, `tracking is null`, `Reason is not`). Tabloda üç satır başka forma ortaktı → ayırt edici olmaktan çıkardı (7.1.5.2, 7.1.5.3, 7.2.12.1 `EnforcementNotDistinctive`): 7.2.14.1 yalnız kendi `OrderCorrections.cs` parçalarını kullanır, 7.2.8.3 teslim almada iki ifadeli kendi parçasını | `FormFieldPlaceEnforcementTests` (11 test): `EveryPlace_OfARowWithSeveralPlaces_HasItsEntry_AndItsLinesAreInTheCode` (67 yer), `PlaceWithoutAnEntry_AndAnEntryOfNoPlace_AreReported` (67 yerin her girişi tek tek silinir), `EveryLineOfEveryPlace_RemovedFromTheCode_IsReportedForThePlacesThatHoldIt` (her parça gerçek kaynaktan çıkarılır → o parçayı tutan her yer adıyla raporlanır), `ChangeOfOneHandlersCheck_OnTheRealCode_ReportsThatPlaceAndNoOtherPlace` (raporun 7 deneyi, yalnız beklenen yer raporlanır), `TwoPlacesOfOneRow_WithTheSameKindOfCheck_ReadItInTheirOwnHandlers` |

Gerçek `src` deneyleri (geçici betiklerle, depoya girmez; her deneyde `src` dosyasında parçanın tür belirteci değiştirilir, `FormInventoryConsistencyTests` koşulur, dosya geri alınır; `git status src` temiz). Raporun 7 deneyi (`ContentInputs.cs` `PortfolioItemInput.Title`, `CustomPageInput.Title`, `BranchInput.Name` tavanı `int.MaxValue`; `BranchInput.WorkingHours`, `PortfolioItemInput.ShortDescription` tavanı; `PanelItemSteps.cs` cayma bildirimi IBAN denetimi `false`; `PanelOrderSteps.cs` teslim kaydı sıra sorusu `false`): 7/7 KIRILDI, her biri yalnız kendi yerini raporladı (`PlaceLineNotInCode:7.2.16.1|…Title`, `…|…Name`, `7.2.16.2|…WorkingHours`, `…ShortDescription`, `7.2.10.4|…RecordWithdrawalInput.Iban`, `7.2.8.2|…MarkDeliveredInput.IsDeliveredBeforeDeclaration`).

Satır başına deney: çok yerli 31 satırın 75 özellik yerinden niteliği olan 8'i C1'de (düzeltme 3) sınandı; nitelik taşımayan 67 yerin 43'ü kendi satırıyla denetlenir (61 (yer, tür) çifti, 87 (yer, tür, parça) deneyi): her parça gerçek `src`'de değiştirildi, test KIRILDI ve ilgili yer adıyla raporladı (87/87; "hayır" kalan çift yok, betik hata 0). Kalan 24 yer "yok" beyanlıdır (aşağıda); onların satırın adlandırdığı türden denetimi kodda yoktur, dolayısıyla silinecek denetim yoktur.

| Satır | Yer | (yer, tür) → parça dosyaları | gerçek `src` deneyi |
|---|---|---|---|
| 7.2.3.1 | `AdminProductQuery.Search` | SearchTrim → `PanelProductListReader.cs`, `SearchTerm.cs` | kırıldı |
| 7.2.3.5 | `AuditLogQuery.StartDate`, `EndDate` | DateRange → `AuditTrail.cshtml.cs` (tek `CheckRange` çağrısı iki alanın) | kırıldı (iki yer) |
| 7.2.4.3 | `SaveProductInput.Categories`; `SaveProductCategoryRow.IsPrimaryCategory` | Required → `ProductSaves.cs` (yayın kapısı `MainCategory`); Required → `ProductSaves.cs` (ana kategori seçili kategoriler arasında) | kırıldı (2/2) |
| 7.2.4.5 | `SaveProductInput.Images` | Image, P-22, P-23, P-24 → `ProductForm.cshtml.cs`, `UploadComponentModels.cs`, `ImageUploads.cs`, `ProductSaves.cs` | kırıldı (4 tür) |
| 7.2.4.6 | `SaveProductInput.Options` | P-21 → `OptionStructureRules.cs` | kırıldı |
| 7.2.4.9 | `SaveProductVariantRow.StockQuantity` | Required → `ProductSaves.cs` | kırıldı |
| 7.2.4.10 | `SaveProductVariantRow.ImageId`; `SaveProductVariantRow.DigitalFileId`, `SaveProductInput.DigitalFileId` | Image → `ProductForm.cshtml.cs`; Image, P-26 → `UploadComponentModels.cs`, `DigitalFileDelivery.cs` (tek yükleme bileşeni iki yerin) | kırıldı (3 yer) |
| 7.2.7.1 | `MarkShippedInput.Carrier`, `OtherCarrierName` | Required → `OrderDetail.cshtml.cs`, `OrderTransitions.Shipment.cs` (şirket ya da ad: tek ifade) | kırıldı (2 yer) |
| 7.2.8.1 | `MarkDeliveredInput.DeliveryDate`; `CorrectDeliveryDateInput.DeliveryDate` | Required, Date → `OrderDetail.cshtml.cs`; Required, Date → `OrderDetail.Corrections.cs` | kırıldı (4 çift) |
| 7.2.8.2 | `MarkDeliveredInput.…IsDeliveredBeforeDeclaration`; `CorrectDeliveryDateInput.…` | Required → `PanelOrderSteps.cs`; Required → `OrderCorrections.Interventions.cs` | kırıldı (2/2; raporun 7. deneyi) |
| 7.2.8.3 | `ReceiveReturnInput.ArrivalDate`; `CorrectArrivalDateInput.ArrivalDate` | Required → `OrderDetail.ItemSteps.cs`; Required → `OrderDetail.Corrections.cs` | kırıldı (2/2) |
| 7.2.10.1, 7.2.10.3 | `RecordWithdrawalInput.DeclarationDate`; `RecordLateDeliveryTerminationInput.DeclarationDate` | Required → `OrderDetail.ItemSteps.cs` (cayma ve gecikme feshi işleyicisi ayrı parça) | kırıldı (4 çift) |
| 7.2.10.2 | `RecordWithdrawalInput.Items`; `RecordWithdrawalInputLine.Quantity` | Required → `PanelItemSteps.cs`; Quantity → `OrderDetail.ItemSteps.cs` | kırıldı (2/2) |
| 7.2.10.4 | `RecordWithdrawalInput.Iban`; `RecordLateDeliveryTerminationInput.Iban` | Iban → `PanelItemSteps.cs` (cayma, 173) + `OrderDetail.ItemSteps.cs` (engel eşlemesi); Iban → `OrderDetail.ItemSteps.cs` (238) + `PanelItemSteps.cs` (270) + engel eşlemesi | kırıldı (raporun 6. deneyi dahil, 2/2) |
| 7.2.14.1 | `CorrectAddressInput.ShippingRecipientName`; `ShippingPhone` | Required → `OrderCorrections.cs`; Required, Phone → `OrderCorrections.cs` | kırıldı (3 çift) |
| 7.2.14.2 | `CorrectCarrierInput.Carrier`, `TrackingNumber` | Required → `PanelOrderCorrections.cs`, `OrderTransitions.Shipment.cs` | kırıldı (2 yer) |
| 7.2.14.3 | `CorrectTransitionInput.CorrectionReason`, `ReasonNote` | Required → `OrderDetail.Corrections.cs`, `PanelOrderCorrections.cs` | kırıldı (2 yer) |
| 7.2.16.1 | `SaveContentInput.Name`; `Title`; `Question` | Required, TextCap, P-27 (Name: teklif + şube; Title: referans + genel sayfa) / P-28 (Question) → `ContentInputs.cs`; Required → `ContentRecords.cs` kapısı | kırıldı (9 çift; raporun 1–3. deneyleri) |
| 7.2.16.2 | `ShortDescription`; `ShortIntroduction`; `WorkingHours` | P-28 (teklif + referans) / Required (kapı) + P-29 / P-28 → `ContentInputs.cs`, `ContentRecords.cs` | kırıldı (4 çift; raporun 4–5. deneyleri) |
| 7.2.16.3 | `SaveContentInput.Answer` | Required → `ContentRecords.cs` | kırıldı |
| 7.2.16.7 | `SaveContentInput.LinkUrl`; `MapUrl` | Url → `ContentInputs.cs` (duyuru, şube) | kırıldı (2/2) |
| 7.2.16.9 | `SaveContentInput.ImageId` | Image, P-22, P-23, P-25 → `ContentForm.cshtml.cs`, `UploadComponentModels.cs`, `ImageUploads.cs` | kırıldı (4 tür) |

"Yok" beyanlı 24 yer (satırın adlandırdığı türden kodda denetim yok; süzgeç, bayrak, isteğe bağlı metin ya da eşzamanlılık değeri): 7.2.3.1 `PublishStatus`; 7.2.3.2 `Search`, `OrderStatus`, `PaymentStatus`, `PendingWork`; 7.2.3.3 `RequestType`, `RequestStatus`; 7.2.3.5 `AdminId`; 7.2.4.5 `SaveProductImageRow.AltText`; 7.2.4.6 `SaveProductOptionRow.Name`, `SaveProductOptionValueRow.Value`; 7.2.4.9 `OriginalStockQuantity`; 7.2.4.13 `MeasurementUnit`; 7.2.10.4 iki `IsFromContactEmail`; 7.2.13.2 `OrderItemId`, `NotReturnedClosureId`; 7.2.16.3 `LongText`, `Text`; 7.2.16.9 `ImageAltText`, `SaveContentImageRow.AltText`; 7.2.16.10 `LinkedProducts`, `IsShownOnHomepage`, `IsShownInMenu`. Boş girişi silmek `PlaceEnforcementMissing` verir (67/67 yer sınandı).

Düzeltme 4 öz denetimi (bütün maddeler yeniden; `Docs/11` T547 kabul kriterleri ve `12`'nin ilgili kontrol listesi):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 ekran bloğu | ✓ | `StateRoleConsistencyTests` yeşil (değişmedi) |
| Kriter 2 / KK2 sayfa testi | ✓ | `StateRolePageTests` kodu değişmedi; `src` bu turda değişmedi |
| Kriter 3 / KK3 iki kapı | ✓ | `SalesGate_*`, `DataGate_*` yeşil |
| Kriter 4 / KK4 durum adları | ✓ | `SynonymOfAStatusName_IsReported` yeşil |
| Kriter 5 / KK5 Yönetici satırı | ✓ | `AdministratorRow_*`, `DraftRead_*` yeşil |
| Kriter 6 / KK6 alan, zorunluluk, kural | ✓ | E1: yer başına satır (`FormFieldPlaceEnforcementTests` 11/11, `FormFieldPlaceMarksTests` 5/5); raporun 7 deneyi ve 87 (yer, tür, parça) deneyi gerçek `src`'de KIRILDI (yukarıdaki tablo); C1'in nitelik yüzü durur |
| Kriter 7 / KK7 kimlik, ürün sabiti | ✓ | `Rule_WritingAValue_IsReported`, `ProductConstant_NoValidationReads_IsReported`, `LengthAttribute_WritingTheNumber_IsReported` yeşil (değişmedi) |
| Kriter 8 / KK8 panel | ✓ | `Panel*_IsReported` yeşil |
| Kriter 9 / KK9 yer | ✓ | yeni testler `UnitTests/Consistency/` altında, `[Trait("Consistency","true")]`; DB/ağ yok |
| `09 §25` / güvenlik taraması | ✓ | yalnız test kodu ve belge satırı; `src` yok, yeni bağımlılık, sır, yetki etkisi yok; sır bekçisi adları kullanıldı |
| Sık FAIL (1) istisna geniş | ✓ | yeni istisna yok; boş giriş yer başına ve tek tek beyan, eksik giriş kırar |
| Sık FAIL (2) çağrı biçimi kaçıyor | ✓ | her yerin parçası kod metninde tam bir kez tutulur (ek argüman, adlı argüman değişince kırar); iki işleyicili alanda iki ayrı parça |
| Sık FAIL (3) 06 varsayılanı / FK | ✓ (kapsam dışı) | şema değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | 67 yer / 31 satır / 8 niteliği olan yer: `PlaceMarks` özeti ve düzeltme 3 satırı aynı; yeni sayı yalnız bu bölümde ve `FormFieldPlaceEnforcementTests`'te (67); `04` sürüm satırı güncellendi |
| Sık FAIL (5) karar kaydısız belge kuralı | ✓ | belge kuralı değişmedi (`04` yalnız sürüm satırı); karar kaydı gerekmez |
| T538 / T546 emsali: sessiz atlama yok, ortak ifade kanıt değil | ✓ | her (yer, tür) kendi parçasıyla; iki yerin tek ifade olduğu yerler (denetim tek çağrıda iki alanı: 7.2.3.5 tarih aralığı, 7.2.7.1 / 7.2.14.2 şirket-ad-takip ifadesi, 7.2.4.10 dijital dosya yükleme bileşeni) tabloda açıkça iki yerin ortak parçasıdır; öteki parçalar yerine özeldir |
| Hedefi T547 olan backlog kalemi | ✓ | `grep T547`: D-1656, D-1657, D-1661, D-1666; dördü ileri, dokunulmadı (D-1656 kod → satır yönü; bu düzeltme satır → kod yönündedir) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build --filter-trait "Consistency=true"` | 655 geçti (düzeltme 4; 644 + `FormFieldPlaceEnforcementTests` 11) |
| Unit (tamamı) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | düzeltme 4: 8075/8075 |
| Integration | ✓ (`src` değişmedi, koşulmadı) — TAM: CI | — | düzeltme 4 yalnız test kodu ve belge değiştirdi; Integration'a dokunan kod yok; TAM: CI |
| Format (düzeltme 4) | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <6 .cs>` | değişiklik yok |
| Unit (Consistency, düzeltme 3) | ✓ | `dotnet test tests/Shopfolio.UnitTests --filter-trait "Consistency=true"` | 644 geçti (düzeltme 3; 639 + `FormFieldPlaceMarksTests` 5) |
| Unit (diğer, düzeltme 3) | ✓ | `--filter-not-trait "Consistency=true"` | 7420 geçti |
| Integration (düzeltme 3) | ✓ (ilgili sınıf) — TAM: CI | `dotnet test tests/Shopfolio.IntegrationTests --filter-class "*CategoriesPageTests" "*CategoryTreeAndListsTests"` | düzeltme 3: 42/42 geçti (postgres:18, `track_commit_timestamp=on`, konteyner `t547-fix3-pg`, port 56547); TAM: CI |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <değişen .cs>` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 3, 2026-11-16; tur 1 ve tur 2 de FAIL) |
| Bulgu sayısı | Tur 3: 1 (E1 ORTA S1 — C1'in işleyici kodundaki yerleri); C2 kapandı, C1'in nitelik yüzü kapandı; tur 2: 2 (C1, C2); tur 1'in B1–B5'i kapandı |
| Düzeltme gerekli mi | Evet — E1 (ikinci iterasyonun düzeltmesi de FAIL: eskalasyon, `INSTRUCTIONS.md` §3.3; `12 §8.5`) |
| Bloke-etmeyen not (K) sayısı | Tur 1: 2 (D-1656, D-1657); tur 2: 1 (D-1661); tur 3: 1 (D-1666) |

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 53 ekranın bloğu | ✓ | `StateRoleConsistencyTests` yeşil; `Block_DeletedFromTheDocument_IsReported` |
| 2 | Her çiftte gösterilen ve açık işlemler sayfa testiyle | ~ | `StateRolePageTests` 2/2 yeşil (yerel, postgres:18). B5: `Always` dışındaki 13 işlem (ör. `RemoveOrReduceItem`, `MarkServiceCompleted`, `RetryOrRedirectCardRefund`, `CorrectGuestEmail`) yalnız "aralık dışında çizilmez" yönünde denetleniyor; 12'si `Always`'e eklenince test yine geçti — "açık ama çizilmemiş" yönü gereksiz yere kapalı |
| 3 | İki kapı yalnız altı vitrin ekranı | ✓ | Gerçek `src` deneyi: `Pages/Product.cshtml.cs`'te satış kapısı belirteçleri kaldırıldı → `GateReadDiffers` |
| 4 | Durum adları `02 §5`'inki | ✓ | `SynonymOfAStatusName_IsReported`, `StatusBadgeText_OfTheCode_EqualsTheNameOf02` |
| 5 | Yönetici satırı sınırı | ✓ | `AdministratorRow_*`, `DraftRead_*`; E-06/E-07 belgede listeli, kodda taslak okuması `DraftReadDiffers` ile denetli |
| 6 | Alan, zorunluluk, kural kodla eşit | ✗ | B1: 11 gerçek `src` deneyinden 6'sı sessiz (8 satır) |
| 7 | Kural kimlikle işaret eder, değer `02 §11`'den | ✗ | B2: 7.2.5.1'in P-28'i `02 §11`'de bu alanı kapsamıyor; B3: 7.2.4.13'ün 8.2.3'ü ölçeği tutmuyor |
| 8 | Panel tek rol, `/panel` altında | ✓ | `Panel*_IsReported` testleri |
| 9 | `Consistency` Unit; 2 Integration | ✓ | 634 Consistency, `StateRolePageTests` Integration |

Gerçek `src` deneyleri (her biri geri alındı): kıran — `OrderDetail.ItemSteps.cs` 163 (7.2.9.2), 262 (7.2.11.1), `FirmSettings.cs` 110 (7.2.17.2), `CompanyIdentityRules.cs` `ChamberName` (7.2.19.6), `Product.cshtml.cs` kapı belirteçleri (KK3); sessiz — `EmailChangeRules.cs` L-9 → L-3, `OrderDetail.ItemSteps.cs` 371 (`ArriveAsync` adet denetimi), `Categories.cshtml.cs` 25 (`ShortTextMaxLength` → `ProductNameMaxLength`), `CompanyIdentityRules.cs` ortak zorunlu listeden `ProvinceId`, `Phone`, `Email`.

Yapım raporuyla uyuşmazlık: öz denetimdeki "182 işlem satırı silindi → 127/127 satır" deneyi satırın kendi metnini sildiği için kanıtın o alana özgü olup olmadığını sınamıyor; B1 bu boşluktan doğdu.

### Doğrulama tur 2 — validator kanıtları (baş `962a08d9`)

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 53 ekranın bloğu | ✓ | Consistency 639/639 yeşil; `Block_DeletedFromTheDocument_IsReported` |
| 2 | Her çiftte gösterilen ve açık işlemler sayfa testiyle | ✓ | `StateRolePageTests` 2/2 (yerel postgres:18, `track_commit_timestamp=on`); B5 kapandı: gerçek `src` deneyi `_OrderDetailItemSteps.cshtml` `close-not-returned-form-` kimliği değiştirildi → "CloseAsNotReturned is open but the page draws none" (çok satır); hiç açılmayan işlem de raporlanır |
| 3 | İki kapı yalnız altı vitrin ekranı | ✓ | `SalesGate_*`, `DataGate_*` yeşil |
| 4 | Durum adları `02 §5`'inki | ✓ | `SynonymOfAStatusName_IsReported`, `StatusBadgeText_OfTheCode_EqualsTheNameOf02` |
| 5 | Yönetici satırı sınırı | ✓ | `AdministratorRow_*`, `DraftRead_*` |
| 6 | Alan, zorunluluk, kural kodla eşit | ✗ | C1: birden çok özellik yerini taşıyan satırda nitelikler birleşimle okunuyor; bir yerin niteliği kalkınca test sessiz (7.2.5.1, 7.2.4.18). B1 deneyleri (a)–(d) ve 9 ek deney kırdı |
| 7 | Kural kimlikle işaret eder, değer `02 §11`'den | ✓ | B2 (7.2.5.1 "Kısa metin", tavan kimliği yok), B3 (7.2.4.13 → K-1013), B4 (7.3.2 "Kullanıldığı") kapandı; satır → "Kullanıldığı" yönünde açık kalmadı (betikle tarandı) |
| 8 | Panel tek rol, `/panel` altında | ✓ | `Panel*_IsReported` testleri |
| 9 | `Consistency` Unit; 2 Integration | ✓ | 639 Consistency; `StateRolePageTests` Integration |

Gerçek `src` deneyleri (hepsi geri alındı). Kıran (13): `EmailChangeRules.cs` 38 L-9 → L-3 (7.1.15.2, 7.1.15.4, 7.2.24.2, 7.2.24.4); `OrderDetail.ItemSteps.cs` 371 `TryReadQuantity` → `int.TryParse` (7.2.12.2); `CompanyIdentityRules.cs` ortak listeden `Email` (7.2.19.5, 7.2.19.6; 7.2.19.4 doğru olarak sessiz), 81 `InvalidUrl` → `InvalidEmail` (7.2.19.7); `Coupons.cshtml.cs` 96 (7.2.6.4); `FirmSettings.cs` 119 (7.2.17.4); `ContentInputs.cs` 120 P-30 → P-28 simgesi (7.2.16.4); `CompanySettingsWriter.cs` 235 (7.2.21.7); `AfterSalesScreenModels.cs` 598 (7.1.10.2); `Modules/Catalog/Coupons.cs` 202 L-6 → L-4 (7.1.5.7); `OrderDetail.ItemSteps.cs` 238 IBAN kuralı (7.2.10.4); `Checkout.cshtml.cs` 281 (7.1.5.8); `RenameCategoryInput.Name`'e `TextTooLong` geri eklendi → `RuleMissesAttribute:7.2.5.1` (B1 (c)). Sessiz (3): `CreateCategoryInput.Name`'den `[FieldRequired]` kaldırıldı (7.2.5.1; C1); `SaveProductDiscountRow.EndDate`'ten `[InvalidDate]` kaldırıldı (7.2.4.18; C1); `EmailChangeRules.cs` 40 `IsExceededAsync(L9, byEmail)` → L3 (parça yalnız sayma satırını tutar; metin tabanlı denetimin doğal sınırı, bulgu sayılmadı).

C2: kategori adı tavanının kaldırılması (`Categories.cshtml(.cs)`, K-1619) 6000 karakterlik adla `POST /panel/kategoriler?handler=CreateCategory`'yi 500'e düşürüyor (geçici sınama, `CategoriesPageTests` içinde; geri alındı) — main'de 200 karakter tavanı 422 alan mesajı veriyordu. `09` 9.4 madde 3 "Kırpma yoktur (ad tavanı P-27)" diyerek kategori dahil beş kayıt türünün adının tavanlı olduğunu varsayıyor; K-1619'un "tavanın kaynağı hiçbir belgede yok" tespiti bu satırı görmemiş. Çözüm belge kararı ister (kategori adına tavan — ör. P-27 kapsamının karar kaydıyla genişletilmesi ve `04` 7.2.5.1, 9.6.8, `06 §4.9` hizası — ya da adresin kırpılması ve `09` 9.4 madde 3'ün düzeltilmesi).

Yapım raporuyla uyuşmazlık: öz denetimde "`09 §25`; güvenlik taraması — `src` değişmedi" yazıyor, düzeltme 2 `src`'yi değiştirdi (C2); KK6 özellik yeri deneyi (194/194, özellik `internal`) yerin tamamını kaldırıyor, iki yerli satırda tek yerin niteliğinin kalkmasını sınamıyor (C1).

### Doğrulama tur 3 — validator kanıtları (baş `e7a1b07e`)

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | 53 ekranın bloğu | ✓ | Consistency 644/644; gerçek belge deneyi `04` "6.2.5 E-05" başlığı bozuldu → `StateRoleMatrix_EqualsTheCode_InBothDirections` dahil 3 test kırıldı |
| 2 | Her çiftte gösterilen ve açık işlemler sayfa testiyle | ✓ | `StateRolePageTests` + `CategoriesPageTests` 11/11 (yerel postgres:18, `track_commit_timestamp=on`); dosya tur 2'den beri değişmedi, "never open/never drawn" denetimi yerinde |
| 3 | İki kapı yalnız altı vitrin ekranı | ✓ | `SalesGate_*`, `DataGate_*` yeşil; kod tur 2'den beri değişmedi |
| 4 | Durum adları `02 §5`'inki | ✓ | `SynonymOfAStatusName_IsReported`, `StatusBadgeText_OfTheCode_EqualsTheNameOf02` |
| 5 | Yönetici satırı sınırı | ✓ | `AdministratorRow_*`, `DraftRead_*` |
| 6 | Alan, zorunluluk, kural kodla eşit | ✗ | E1: çok yerli satırda nitelik taşımayan, kuralı işleyici kodunda duran yerlerin kuralı satır düzeyinde birleşimle okunuyor; 8 gerçek `src` deneyinden 7'si sessiz (aşağıda). C1'in nitelik yüzü kapandı (`PlaceMarksDiffer`) |
| 7 | Kural kimlikle işaret eder, değer `02 §11`'den | ✓ | 7.2.5.1 "Kısa metin — ad P-27 (7.3.2.14)"; belge deneyi kimlik yerine "en çok 150 karakter" → 2 test kırıldı; `LengthAttribute_WritingTheNumber_IsReported` |
| 8 | Panel tek rol, `/panel` altında | ✓ | `Panel*_IsReported` testleri |
| 9 | `Consistency` Unit; 2 Integration | ✓ | 644 Consistency `UnitTests/Consistency/` altında; `StateRolePageTests` Integration |

Gerçek `src` deneyleri (hepsi geri alındı). Sessiz (Consistency 644/644 yeşil): `ContentInputs.cs` 73 `PortfolioItemInput.Title` tavanı `int.MaxValue` (7.2.16.1 başlık P-27); 92 `CustomPageInput.Title` (7.2.16.1 başlık P-27); `BranchInput.Name` tavanı (7.2.16.1 ad P-27, şube); 106 `BranchInput.WorkingHours` (7.2.16.2 çalışma saatleri P-28); 74 `PortfolioItemInput.ShortDescription` (7.2.16.2 P-28); `Modules/Ordering/PanelItemSteps.cs` 173 cayma bildiriminin IBAN denetimi (7.2.10.4, `RecordWithdrawalInput.Iban`); `Modules/Ordering/PanelOrderSteps.cs` 94 teslim kaydında sıra sorusu zorunluluğu (7.2.8.2, `MarkDeliveredInput`). İki başlık tavanı ve çalışma saatleri birlikte kaldırılınca bütün Unit 8064/8064 yeşil. Kıran: `OrderDetail.ItemSteps.cs` 365 ulaşma tarihi zorunluluğu (7.2.12.1); `BranchInput.Phone` kuralı (7.2.16.6); `CreateCategoryInput.Name` `[FieldRequired]` → `PlaceMarksDiffer:7.2.5.1`; `SaveProductDiscountRow.EndDate` `[InvalidDate]` → `PlaceMarksDiffer:7.2.4.18`; `AddressBooks.cs` 177 sipariş telefonu (7.1.5.3); `PasswordRegime.cs` 64 L-1 → L-2 (6 satır).

C2 kapandı: `Categories.cshtml(.cs)` `TextTooLong(ProductConstants.ProductNameMaxLength)` ve üç `maxlength`; `ANameOverTheCeiling_IsA422WithTheFieldMessage_OnCreateAndRename_AndWritesNothing_T547_C2` yeşil (6000 karakter ekleme ve yeniden adlandırmada 422 + alan mesajı, tam tavanda 303). K-1619, `02` P-27, `04` 7.2.5.1, 7.3.2.14, 9.6.8, `06 §4.9`, `07` 9.6.2–9.6.3, `09` §3.5 örneği tutarlı ve `09` 9.4 madde 3'e dayanıyor; kalan tek yer `07 §6.2.2` `TextTooLong` satırının UC sütunu (D-1666).

Yapım raporuyla uyuşmazlık: düzeltme 3 bölümü nitelik taşımayan 67 yerin "kural ve zorunluluğu satır düzeyinde, yeri adlayan parça ile denetlenir" diyor; deneyler satırın kuralını başka yerin işlem satırının kanıtladığını gösterdi (E1). D-1656 kod → satır yönüdür, bu boşluk satır → kod yönüdür.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T547-durum-rol-form-esitlik`
- **Commit:** son baş SHA PR'da
- **PR:** #NN (açılışta)
- **CI:** —
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Hedefi T547 olan backlog kalemi yoktu (`grep T547`); yeni kalem açılmadı | — | — |

## Notlar

- **⚠ K-1618:** `04 §7`'nin 10 kural hücresi (7.1.3.4, 7.1.5.9, 7.2.4.11, 7.2.4.13, 7.2.4.18, 7.2.5.2, 7.2.6.3, 7.2.6.6, 7.2.17.1, 7.2.21.2) sayı sözcüğü yerine kimlik gösterir; anlam değişmedi, kural eklenmedi.
- **Çapa zayıflığı:** deney, çapanın alt dize olarak aranmasının yeniden adlanmış alanı gizleyebildiğini gösterdi; `Holds` ile giderildi.
- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** `MEMORY.md` ve backlog sayacına dokunulmadı (parça dosyası düzeni)
