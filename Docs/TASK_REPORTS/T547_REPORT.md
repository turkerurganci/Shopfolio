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
- **Belge:** `04 §7`'nin 10 kural hücresi kimlikle gösterir; karar kaydı K-1618 (⚠). Düzeltme 1: `04 §7.2.5.1` ve `§7.2.6.2` kural hücreleri kimlik aldı; K-1619 (⚠).

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/` — `StateRole*.cs` (4), `FormInventoryCheck.cs`, `FormFieldCheck.cs`, `FormFieldInventory.cs`, `FormFieldEnforcement.cs`, `FormFieldEnforcementTable.cs`, `FormFieldEnforcementTests.cs`, `FormInventoryConsistencyTests.cs`.
- `tests/Shopfolio.IntegrationTests/Web/StateRolePageTests.cs`.
- `Docs/04_UI_SPECS.md` (§7 kural hücreleri, sürüm satırı), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1618, sürüm satırı), `Docs/STATUS_CHANGELOG.d/T547.md`, `Docs/IMPLEMENTATION_STATUS.md` (satır 306). `src` değişmedi.

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
| B2 (KK7, K-1619) | Kaynak arandı (`02` P-28 kapsamı, `06 §4.9`, `04` 9.6.8, T526 raporu): kategori adı tavanı hiçbirinde yok. 7.2.5.1 hücresi eski "Kısa metin"e döndü; koddaki tavan kaldırıldı (`Categories.cshtml.cs` iki `TextTooLong`, `Categories.cshtml` üç `maxlength`); 7.2.5.1 satırının kural kimliği boş. K-1619 buna göre yeniden yazıldı | `Docs/04_UI_SPECS.md`, `Docs/PRODUCT_DISCOVERY_STATUS.md` K-1619; `FieldInventory_EqualsTheDocumentsAndTheCode` |
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

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --filter-trait "Consistency=true"` | 639 geçti (düzeltme 2) |
| Unit (diğer) | ✓ | `--filter-not-trait "Consistency=true"` | 7420 geçti |
| Integration | ✓ (ilgili sınıf) — TAM: CI | `dotnet test tests/Shopfolio.IntegrationTests --filter-class "*StateRolePageTests"` | düzeltme 2: `StateRolePageTests` 2/2 geçti (postgres:18, `track_commit_timestamp=on`, konteyner `shopfolio-t547-pg`); TAM: CI |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <değişen .cs>` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, 2026-11-15; tur 1 de FAIL) |
| Bulgu sayısı | Tur 2: 2 (C1 ORTA S1, C2 ORTA S2); tur 1'in B1–B5'i kapandı |
| Düzeltme gerekli mi | Evet — C1, C2 (ikinci iterasyon FAIL: eskalasyon, `INSTRUCTIONS.md` §3.3) |
| Bloke-etmeyen not (K) sayısı | Tur 1: 2 (D-1656, D-1657); tur 2: 1 (D-1661) |

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
