# T381 — Ayar katmanları ve parametre envanteri: firma ayarı, kurulum ayarı, ürün sabiti, varsayılan ve çit, değişikliğin ize yazılması

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama tur 1 ✗ FAIL — düzeltme bekliyor) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Envanter** (`Infrastructure/ParameterInventory.cs`): `ParameterLayer` (firma ayarı, kurulum ayarı, ürün sabiti), `ParameterHarm`, `ParameterLayers.Assign` (atama ölçütü), `ParameterEntry` (P-1…P-49; P-2 ve P-11 `Removed`), `ParameterUnit`, `InstallationSettings` (5 kalem: alan adı, gönderen adresi, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları, Google kimlik bilgileri).
- **Çitler** (`Infrastructure/FirmSettingFences.cs`): P-5, P-6, P-7, P-8, P-9, P-44 için `BoundsOf`/`Check`; `PeriodFences`'e `CheckDownloadAllowance` ve `CheckMaxQuantityPerOrder`.
- **Ürün sabitleri** (`ProductConstants`): P-12, 13, 14, 17, 18, 19, 22, 24, 25, 29, 30, 39, 40 (2), 41, 42, 43, 47, 48 eklendi; `ContentRules` P-27/29/30'u sabitten alır.
- **Firma ayarı yazma/okuma** (`Modules/Settings/FirmSettings.cs`, `CompanySettingsWriter`, `ISettingsModule` altı yeni üye, `FirmSettingRecords`): kargo ücreti, süreler ve ürün varsayılanları; tek yazıcı kuralı; değişen her alan eski/yeni değerle `UpdateSettings` iz satırı, değişmeyen kayıt iz yazmaz. Çit dışı değer `InvalidQuantity` + `Side` + `Fence` ile reddedilir.
- **Tarama** (`SettingLayerScan.ScanPanelFormSource`): panel formunda ürün sabiti adlı alan yok (bağlı sayfa modeli üyesi ve işaretleme alanı: `name`, `id`, `asp-for`, `@Model.` önekli, birleşik nitelik listesi, bir önceki satırdaki `[BindProperty]`); iki ihlal ve bir temiz örnek.
- **Belgeler:** `09 §3.5` m.1'e envanter ve çit evi cümlesi; backlog D-164, D-176 yeniden ileri atıldı, D-256 yeni; status, memory; T308/T312/T331 merge kayıtları dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/`: `ParameterInventory.cs`, `FirmSettingFences.cs` (yeni), `PeriodFences.cs`, `ProductConstants.cs`
- `src/Shopfolio.Web/Modules/Settings/`: `FirmSettings.cs` (yeni), `CompanySettingsWriter.cs`, `SettingsModule.cs`, `Contracts/ISettingsModule.cs`, `Contracts/FirmSettingRecords.cs` (yeni), `Contracts/CompanyIdentityRecords.cs`
- `src/Shopfolio.Web/Modules/CorporateContent/ContentRules.cs`
- `tests/Shopfolio.UnitTests/`: `Consistency/ParameterInventoryConsistencyTests.cs`, `InstallationSettings/ParameterLayersTests.cs`, `Architecture/SettingLayerScan(.cs, Tests.cs)`, `Architecture/ViolationSamples/SettingLayers/*`
- `tests/Shopfolio.IntegrationTests/Model/FirmSettingsTests.cs`, `FirmSettingDefaultsTests.cs`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T308|T312|T331_REPORT.md` (merge kaydı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç katman ve değişebilirlik | ✓ | `ParameterLayer`, `ParameterInventory`; `ParameterLayersTests` |
| 2 | Atama ölçütü | ✓ | `ParameterLayers.Assign`; `ParameterLayersTests` (Theory) |
| 3 | Kurulum ayarları beş kalem, panelde yok | ✓ | `InstallationSettings`; `ParameterLayersTests.TheFirmSettingInputsAndSections…`; `ISettingsModule` yüzeyi |
| 4 | On firma ayarı (P-1, P-3…P-10, P-44), panelden değişir | ✓ | `ParameterInventoryConsistencyTests`; `FirmSettingsTests` (kaydet/oku) |
| 5 | Varsayılanla dolu kurulum; tek istisna P-44 | ✓ | `FirmSettingDefaultsTests` (4); `SettingDefaultsConsistencyTests` |
| 6 | Çitler uygulanır, çit dışı reddedilir | ✓ | `FirmSettingFences`/`PeriodFences`; `FirmSettingsTests` çit Theory'leri |
| 7 | Ürün sabiti için form alanı yok; her değer tek yerde | ✓ | `SettingLayerScanTests.PanelForms_HaveNoFieldForAProductConstant`, `PanelFormScan_OnViolationSample…`, `PanelFormScan_SeesEachWayAFieldIsWritten`; `ParameterInventoryConsistencyTests` |
| 8 | P-2 ve P-11 kaldırılmış | ✓ | `ParameterEntry.Removed`; `ParameterLayersTests`, `ParameterInventoryConsistencyTests` |
| 9 | Her ayar değişikliği eski/yeni değerle izde | ✓ | `FirmSettingsTests` (iz satırı eski/yeni, değişmeyen kayıt iz yazmaz, geri alma) |
| 10 | Envanter `02 §11` ile eşit, `Consistency` testi | ✓ | `ParameterInventoryConsistencyTests` (02 §11 ↔ envanter, sabit değeri ve birimi) |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçinin kodu dahil bütün maddeleri diff'te yeniden denetledi; `sik-fail-nedenleri.md` okundu.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 üç katman ve değişebilirlik | ✓ | `ParameterInventory.cs`; `ParameterLayersTests` |
| KK2 ölçütle katman | ✓ | `ParameterLayers.Assign`; `ParameterLayersTests` Theory |
| KK3 kurulum ayarı panelde yok | ✓ | `InstallationSettings` listesi; form taraması |
| KK4 on ayar varsayılanlarla | ✓ | `FirmSettingDefaultsTests`, `SettingDefaultsConsistencyTests` |
| KK5 boş kurulumda uçtan uca | ✓ | `FirmSettingDefaultsTests` (boş kurulumda okuma) |
| KK6 çit dışı reddedilir | ✓ | `FirmSettingsTests` çit Theory'leri |
| KK7 ürün sabiti için alan yok | ✓ | `SettingLayerScanTests` (gerçek Pages taraması, ihlal örnekleri, çağrı biçimleri) |
| KK8 P-2, P-11 envanterde yok | ✓ | `Removed` bayrağı; Consistency |
| KK9 ayar değişikliği izde | ✓ | `FirmSettingsTests` iz testleri |
| KK10 envanter eşitlik testi | ✓ | `ParameterInventoryConsistencyTests` |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | gizli bilgi yok, yeni bağımlılık yok, girdi çiti sunucuda (`FirmSettingFences`) |
| Sık FAIL 1: tarama istisnası geniş | ✓ | yeni istisna yok; form taraması yalnız ürün sabiti adına bakar |
| Sık FAIL 2: tarama bir çağrı biçimini kaçırıyor | ✓ | `PanelFormScan_SeesEachWayAFieldIsWritten` (`@Model.`, tek tırnak `id`, birleşik nitelik listesi, önceki satır niteliği) |
| Sık FAIL 3: 06 "Varsayılan" DB default'u / FK dizini | ✓ | T381 şema değiştirmez; varsayılan `FirmSettingDefaultsTests` + var olan `SettingDefaultsConsistencyTests` |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | sabit sayıları testte sabitten/02 §11'den türetilir; backlog aktif 104 (status/memory aynı) |
| Sık FAIL 5: belge kuralını değiştiren 09 metni | ✓ | 09 §3.5 m.1 yalnız kodun evini yazar, kural değişmedi; karar kaydı gerekmez |
| Backlog: hedefi T381 olan kayıtlar | ✓ | D-164 → T525, D-176 → T536 (gerekçeli), yeni D-256 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency hariç) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build --filter-not-trait "Consistency=true"` | 3318/3318 (Architecture dahil) |
| Consistency | ✓ | `... --filter-trait "Consistency=true"` | 215/215 (docs commit inden sonra) |
| Integration | ✓ | `... IntegrationTests --filter-class "*FirmSetting*"`; `*SalesGateTests`, `*CompanyIdentityTests` | 24/24; 46/46 |
| Derleme/format | ✓ | `dotnet build Shopfolio.slnx -warnaserror`; `dotnet format <proje> --verify-no-changes` (üç proje) | 0 uyarı, 0 hata; değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-07, head `98f5550`) |
| Bulgu sayısı | 2 (B1 ORTA S1, B2 DÜŞÜK S1) |
| Düzeltme gerekli mi | Evet — B1 ve B2 |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-279; K2 bu PR'da kapatıldı) |

### Düzeltme turu 1 (2026-11-07)

- **B1:** `FirmSettings.CheckFenced` çit dışı değeri `07 §6.2.6`'daki `ValueOutOfBounds` (422) koduyla reddeder (`Side` + `Fence` ile); `FirmSettingsTests` çit testleri bu kodu bekler. Kriter 6 ve KK6 artık ✓.
- **B2:** `PeriodsInput.ShippingPeriod` ve `PeriodsSection.ShippingPeriod` adı `DefaultShippingPeriod` oldu (`07 UC-170`, `06 §4.2`); reddin `Field` adı da aynıdır. Aynı adı anan öteki yerler (`FirmSettings`, `FirmSettingDefaultsTests`, `FirmSettingsTests`) güncellendi; `Product`/`Order.ShippingPeriod` ve `CountedPeriod.ShippingPeriod` başka kavramlardır, `06`'da da bu adladır.
- **`origin/main` birleştirmesi:** T309 aynı ömür sabitlerini (P-12, P-13, P-14, P-47) `ProductConstants`'a eklemişti; bu daldaki yinelenenler silindi, `P-47` envanter satırı T309'un `EmailChangeRevertLinkDays` adını anar. Kayıt belgeleri çakışması çözüldü.

### Doğrulama tur 1 — validator kanıtları

Başlangıç kapıları: working tree temiz, `core.hooksPath` = `scripts/git-hooks`; ana dal son 3 run success (42368786653, 42368786665, 42366197659); memory'de T381 satırı var. Dal CI 42375448881 success (head `98f5550`).

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Üç katman | ✓ | `ParameterLayer` üç değer; `ParameterInventory.Entries` 49 satır; `ParameterLayersTests.TheInventory_HasTheThreeLayers…` |
| 2 | Atama ölçütü | ✓ | `ParameterLayers.Assign` (`02 §10.1.3` ile birebir); Theory 8 durum + var olan satırlar ölçütle eşleşir |
| 3 | Kurulum ayarları | ✓ | `InstallationSettings` 5 kalem, 3 zorunlu (`09 §3.5` m.2); `company_settings`'te karşılığı yok (`FirmSettingDefaultsTests`) |
| 4 | On firma ayarı | ✓ (servis düzeyi) | Consistency: 10 firma ayarı = `02 §11.1`; yedi mağaza ayarının üç kaydı `FirmSettingsTests`'te; panel ekranı T536, ürün formu T525 |
| 5 | Varsayılanla dolu kurulum | ✓ | `FirmSettingDefaultsTests` (4) gerçek göçlerle; P-44 sütunu yok |
| 6 | Çitler, çit dışı ret | ~ | Değer reddedilir ve saklanmaz, iz yazılmaz (`FirmSettingsTests` Theory 4 + P-8); ama ret kodu `InvalidQuantity`, `07 §6.2` `ValueOutOfBounds` der (B1). P-6/P-9/P-44 yazım yolu yok (K1) |
| 7 | Ürün sabiti alanı yok, tek yer | ✓ | `SettingLayerScanTests.PanelForms_HaveNoFieldForAProductConstant` + iki ihlal örneği kırmızı; Consistency sabit ↔ satır birebir |
| 8 | P-2, P-11 kaldırılmış | ✓ | `Removed` satırları, Consistency `["P-11","P-2"]` |
| 9 | Ayar değişikliği izde | ✓ (üç bölüm) | `FirmSettingsTests`: `UpdateSettings` satırı, yönetici, görünen ad, eski/yeni değer; değişmeyen kayıt iz yazmaz; geri alma; ürün formu D-164, öteki kayıtlar D-256 |
| 10 | Envanter eşitliği Consistency | ✓ | `ParameterInventoryConsistencyTests` 6 test; Consistency 215/215 |

**Bulgular:**
- **B1 (ORTA, S1):** `FirmSettings.CheckFenced` çit dışı değeri `FieldCodes.InvalidQuantity` ile reddeder. `07 §6.2.6` `ValueOutOfBounds` (422) kodunu tam bu iş için tanımlar: "Parametrenin çitinin dışında — kargoya verme süresi P-5, ifa süresi P-44, havale ödeme süresi P-7, indirme hakkı P-8 … UC-114, UC-170, UC-171". `InvalidQuantity` ise "adet birden küçük ya da tam sayı değil" kodudur (mesajı "1 ile {1} arasında bir tam sayı"). Testler (`FirmSettingsTests` çit Theory'si, P-8 testi) yanlış kodu kilitliyor. Yapım raporundaki "07 §6.2'de çite özel hata kodu yok" ifadesi yanlış. Düzeltme: ret `ValueOutOfBounds` ile (gerekiyorsa `FieldRejection.Code` sözleşmesi bu alan dışı kodu da kabul edecek biçimde), testler aynı PR'da.
- **B2 (DÜŞÜK, S1):** `PeriodsInput.ShippingPeriod` ve `PeriodsSection.ShippingPeriod` — `07 UC-170` isteği ve `06 §4.2` sütunu `DefaultShippingPeriod` der (`09 §25`: yeni ad `06`/`07`'den birebir). Reddin `Field` adı da bu yüzden formun alanıyla eşleşmez. Düzeltme: ad `DefaultShippingPeriod`.

**K-notları:** K1 — ürün formundaki P-6/P-9/P-44 çitinin yazım yolu yok → D-279 (T525). K2 — yapım T308'in PASS girişini status'tan kaldırmış ama `STATUS_CHANGELOG.md`'ye taşımamıştı → bu PR'da taşındı.

**Testler (validator, yerel):** `dotnet build Shopfolio.slnx -c Release -warnaserror` 0 uyarı; `dotnet format` üç proje değişiklik yok; Consistency 215/215; Unit Architecture 838 + öteki 2480 = 3318/3318; betik testleri 4 dosya geçti; Integration `FirmSettingsTests` + `FirmSettingDefaultsTests` 24/24, `CompanyIdentityTests` + `SalesGateTests` 46/46 (PostgreSQL 18, ayrı konteyner, sonra silindi).

**Yapım raporu karşılaştırması:** kriter 6 ve KK6 raporda ✓, validator ~ (B1); "kolay karar (1)" belgeyle çelişiyor. Öz denetim tablosu var ve kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T381-ayar-katmanlari`
- **PR:** #430
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır sonraki task dalında güncellenir)*

## Dış Varsayımlar

Dış varsayım: yok. (Kanıt: yeni paket, dış servis ya da araç sürümü kullanılmadı; tek dış nokta yerel Postgres 18 konteyneri, T106'dan beri kullanılan test düzeneği.)

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ürün formundaki P-6/P-9/P-44 iz satırı (`09 §9.2` m.3 konu tablosu) | D-164 → T525 | Ürün formunu T525 yazar; T381'de çağıran üretim kodu yok |
| 2 | Oturumlu yönetici kaynağının sınanması | D-176 → T536 | İlk istek-çağıran panel task'ı |
| 3 | Kimlik, iade adresi, ödeme yöntemi, ana sayfa bölümü kayıtları iz yazmıyor | D-256 | Kendi panel task'ları yazar; T381 üç sayısal bölümü kapsar |

## Notlar

- **Working tree (Adım -1):** temiz; hooksPath repo betiğinde.
- **Ana dal CI startup (Adım 0):** main `ad8c48d`: CI 42368786665 success, Publish 42368786653 success; önceki `1112ca6`: CI 42366197638 success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ Kolay kararlar:** (1) Çit dışı değer `07 §6.2.6`'daki `ValueOutOfBounds` (422) koduyla reddedilir (`Side` + `Fence` ile); mesajı T536 kurar (tur 1 B1 düzeltmesi; ilk yapım yanlış olarak `InvalidQuantity` kullanmış, `07`'de çite özel kod yok demişti). (2) `ProductConstants`'a henüz kullanıcısı olmayan 19 sabit eklendi (eşitlik denetimi gereği); kullanıcıları ilgili task'larda bağlanır. (3) P-22 sabiti dize (`ImageFormats`).
- **Devir:** önceki işçi bağlam sınırında bölündü; bu işçi testleri koştu, formatı doğruladı, taramayı sertleştirdi, belge ve kayıt işlerini bitirdi.
