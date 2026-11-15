# T547 — Durum × rol matrisi ve form envanteri eşitlik denetimleri

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-15

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Durum × rol matrisi (kriter 1, 3, 4, 5, 8):** `StateRoleDocument`, `StateRoleInventory`, `StateRoleCheck`, `StateRoleConsistencyTests` — 53 ekranın blokları, iki kapı, durum adları, Yönetici satırı sınırı, panel tek rol ve `/panel` kökü.
- **Form envanteri (kriter 6, 7):** `FormInventoryCheck` (form blokları, satır numaraları, ekran tarafı, kural kimlikleri, değer yasağı, ürün sabiti okuma) ve `FormFieldInventory` + `FormFieldCheck` (her alan satırı = kodda bir yer, gereklilik sözcüğü, kural ve sınır kimlikleri). Bu turda: çapa kanıtı tam tanımlayıcı olarak aranır (`FormFieldCheck.Holds`).
- **Sayfa testi (kriter 2, KK2):** `StateRolePageTests` — 6.2.16.1–10 ve 6.3.9.1–10 çiftleri üç tohumla (kayıtsız · kayıtlı · açık IBAN isteği); müşteri sayfası ve panel ayrıntısı `IOrderOperations` ile birebir.
- **Belge:** `04 §7`'nin 10 kural hücresi kimlikle gösterir; karar kaydı K-1618 (⚠).

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/` — `StateRole*.cs` (4), `FormInventoryCheck.cs`, `FormFieldCheck.cs`, `FormFieldInventory.cs`, `FormInventoryConsistencyTests.cs`.
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
| 6 | Form envanteri: alan, zorunluluk, kural kodla eşit | ✓ | `FieldInventory_EqualsTheDocumentsAndTheCode`, `Field_DeletedFromTheCode_IsReported`, `Requirement_*`, `Rule_*`, `FieldMarkedRequiredInTheCode_ThatTheRowCallsOptional_IsReported`, `FieldWithAnAttribute_WhoseRuleTheRowDoesNotName_IsReported`; satır-satır deney aşağıda |
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
| KK6 alan silinince ya da kural değişince kırılır | ✓ | `Field_DeletedFromTheCode_IsReported`, `Field_RenamedWhileALongerNameHoldsItsName_IsReported`, `Rule_ChangedInTheDocument_IsReported`; gerçek `src` deneyi: 21/21 çapa, 194/194 özellik yeri `FieldNotInCode` verdi |
| KK7 kimlik yerine sayı / `ProductConstants` yerine sayı kırar | ✓ | `Rule_WritingAValue_IsReported`, `ProductConstant_NoValidationReads_IsReported` |
| KK8 `/panel` dışı panel formu ya da işlemi kırar | ✓ | `PanelForm_PostingOutsidePanel_IsReported` |
| KK9 testler `UnitTests/Consistency/` altında | ✓ | dosya yolları |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | `src` değişmedi; sır bekçisi yanlış pozitifleri için `Concealed` ve `cancel =>` adları kullanıldı |

### Sık FAIL nedenleri (öz denetim)

| # | Neden | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Bekçi/tarama istisnası geniş | ✓ | Muaf tutulan tek şey E-06/E-07'nin taslak okuması ve bu, kodda belirteç yokluğuyla denetlenir (`DraftReadDiffers`); alan çapasında istisna yok |
| 2 | Kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Çapa kanıtı bu turda zayıf bulundu (alt dize): `Holds` tam tanımlayıcı ister, `Field_RenamedWhileALongerNameHoldsItsName_IsReported`; kapı belirteçleri 4+3 biçim |
| 3 | 06 varsayılanı / FK dizini | ✓ (kapsam dışı) | göç ve şema değişmedi |
| 4 | Kural değişince aynı sayıyı anan öteki yerler | ✓ | `04` sürüm satırı (satır 5307) ve `PRODUCT_DISCOVERY_STATUS.md` satır 48 K-1618 ile güncellendi; `04` v0.55 kalır |
| 5 | Belge kuralını değiştiren metin karar kaydısız | ✓ | K-1618 (10 hücre; anlam değişmedi) |

### T546 emsali: her satır için ayırt edici kanıt ve gerçek `src` deneyi

| Bölüm | Satır | Kanıt | Ayırt edici mi | Gerçek `src` değişimi test kırdı mı |
|---|---|---|---|---|
| `04 §6.2` / `§6.3` blokları | 53 ekran + panel blokları | ekranın rotası sayfa dosyasına bağlı (`StateRoleInventory`, rota başına) | evet — rota kod dosyasında aranır, iki yönlü | `Screen_WhosePageLeavesTheCode_IsReported` (belgesel) ; sayfa testi her çift için |
| `04 §7.1` ve `§7.2`, çapa yeri (21 satır: 7.1.1.1–3, 7.1.12.1–4, 7.1.13.1–2, 7.1.14.1, 7.1.15.4, 7.2.2.1, 7.2.9.2, 7.2.10.2, 7.2.10.4, 7.2.11.1–2, 7.2.12.2, 7.2.13.1, 7.2.24.4, 7.2.25.1) | çapa satırı başına | `dosya#çapa`; çapa özellik adı ya da çağrı biçimi (`TryReadUnits(input.Items`, `Fields.Email("Input.Email"`) | evet — kelime sınırlı | evet: `t547-mutate.py anchors` her çapayı gerçek dosyada yeniden adlandırdı; 21/21 satır `FieldNotInCode` (deney betiği geçici klasörde, geri alındı) |
| `04 §7.1` ve `§7.2`, özellik yeri (194 yer, 151 satır) | satır başına | `Ns.Tür.Özellik` yansıma | evet — tür ve özellik adı | evet: her özellik `public` → `internal` yapıldı (derlenir, yansıma `Public` bulamaz); 194/194 yer `FieldNotInCode`; ad çakışması (`Panel.ChangeNameInput.Name`, `Account` ile aynı ad) tek tek sınandı |
| gereklilik sözcüğü, kural ve sınır kimlikleri | her satır | `RequirementDiffers`, `RuleDiffers`, `LimitDiffers` (satırın hücresi ↔ envanter) | evet | belge tarafı deneyleri: `Requirement_ChangedInTheDocument_IsReported`, `Rule_ChangedInTheDocument_IsReported`, `Limit_ChangedInTheDocument_IsReported`; kodda `FieldRequired`/`Invalid*` niteliği satırla çelişirse `RequirementContradictsCode`, `RuleMissesAttribute` |

> Not: kuralı kodda değiştirmek (ör. bir `Invalid*` niteliğini kaldırmak) satırı kırmaz; denetim yönü nitelik → satırdır (kod belgede olmayan kural taşırsa kırılır). Niteliğin eksilmesi doğrulama testlerinin işidir (T-doğrulama sınıfları).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --filter-trait "Consistency=true"` | 622 geçti |
| Unit (diğer) | ✓ | `--filter-not-trait "Consistency=true"` | 7420 geçti |
| Integration | ✓ (ilgili sınıf) — TAM: CI | `dotnet test tests/Shopfolio.IntegrationTests --filter-class "*StateRolePageTests"` | 2/2 geçti (postgres:18, `track_commit_timestamp=on`) |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <değişen .cs>` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

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
