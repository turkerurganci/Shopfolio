# T211 — Model bağlama kalıbı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-06

> Taslağı yapım işçisi doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Binding/` (yeni klasör): `IFormInput`, `IQueryModel`, `IMachineRequest`, `IMachineResponse` işaretleri (üç model türü), `ModelNaming` (sonek ve işleyici adı kalıbı), `SpecAssignedNameAttribute` (`06 §4`'te karşılığı olmayan, `07`'nin bir kez verdiği alan adı), `ItemRows.RejectRepeated` (`Items` kalem başına tek satır).
- Mimari kural motoru `UnitTests/Architecture/ModelBindingRules` (yansıma): ad–tür, işleyici adı, entity bağlanmaz, alan adı. Kuralın ihlal örneği ayrı derlemede (`ModelBindingViolationSample.dll`; `ViolationSamples/ModelBinding/Models.cs`), her kuralın kendi ihlali ve sessiz komşuları var.
- `Infrastructure/Validation/` (yeni klasör; `09 §3.4` m.1–7, yönetici kararıyla T211'de): `FieldCodes` (`07 §6.2.2`'nin 21 kodu), `FieldError` (kodu iletiye yazma/okuma, `AddFieldError`), `FieldRuleAttribute` + `FieldRules` (12 nitelik sınıfı; sınır değeri parametre sabitinden), `FieldMessageCatalog` (kod → Türkçe kalıp), `TrimmingBinder` + `NotTrimmedAttribute` + `PasswordFieldAttribute` (şifre ve işaretli alan kırpılmaz), `ValidationPageFilter` (işleyiciden önce 422 + `ValidationRejected` satırı; işleyicinin eklediği kural aynı çeviriden geçer), `FieldTargets` (alan etiketi ve dönüşüm hatasının kodu), `FieldValidationSetup.AddFieldValidation` (örtük zorunluluk ve `data-val-*` kapalı, bağlayıcı ve süzgeç kayıtlı). `Program.cs`'e tek satır.
- Mimari kural: `ModelBindingRules` `BuiltInRule` (model özelliği yerleşik doğrulama niteliği taşımaz; ihlal örneği `Models.cs`), `FieldRuleSourceScan` (nitelikte sayı sınırı ve istemci doğrulama çıktısı yok; ihlal örneği `ViolationSamples/Validation/NumberOnRule.cs`).
- Test amaçlı örnek sayfa `IntegrationTests/Pages/Panel/BindingForm` (yalnız test sunucusunda): iki işleyici aynı girdiyi bağlar, modelde olmayan alan, sunucunun fiyatı, `Items`, HTML'i değiştirilmiş istek.
- Önceki task kaydı: T206'nın status Commit hücresi (`494a34c`) ve rapor "Post-merge run'lar" satırı dolduruldu (skill adım 15); eski status girişi `STATUS_CHANGELOG.md`'ye taşındı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Binding/{ModelKinds,ModelNaming,SpecAssignedNameAttribute,ItemRows}.cs` (yeni); `src/Shopfolio.Web/Infrastructure/Validation/*.cs` (yeni); `Program.cs` (tek satır `.AddFieldValidation()`), `Forms/HandlerRequiredFilter.IsWritingMethod` (internal), `Binding/ItemRows` (`AddFieldError`). Test örnek sayfası `Pages/Panel/PatternForm` (T206): `Add` girdisi işleyici parametresi oldu (aşağıda Notlar).
- `tests/Shopfolio.UnitTests/Architecture/{ModelBindingRules,ModelBindingRuleTests}.cs`, `Architecture/ViolationSamples/ModelBinding/Models.cs`, `Infrastructure/ItemRowsTests.cs`, `Shopfolio.UnitTests.csproj` (ihlal örneği derleme hedefi).
- `tests/Shopfolio.IntegrationTests/Pages/Panel/BindingForm.cshtml(.cs)`, `Web/ModelBindingTests.cs`.
- `.claude/CONTEXT.md` (Binding satırı), `.claude/memory/MEMORY.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/DEFERRED_BACKLOG.md` (D-119).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç model türü; ad kalıbı; bildirimin gövdesi `08`'indir, öteki makine uçları gövde almaz | ✓ | `ModelKinds.cs` (üç tür + `IMachineResponse`), `ModelNaming.MarkerOfName`; `ModelBindingRuleTests.ModelNaming_ReadsTheKindFromTheSuffix`, `Sample_ReportsEveryRuleOnItsOwnViolation` (`KindName`, `MissingMarker`). Bildirimin gövdesi için işaret yoktur (`ModelKinds.cs` başlık notu); başka makine ucu gövde türü tanımlanmadı |
| 2 | Model adı formun asıl işleyicisinin adı ve türü; aynı formu gönderen işleyiciler aynı modeli bağlar | ✓ | `ModelNaming.IsInputNameOf`; `InputName` kuralı; `ModelNaming_FormInputIsNamedAfterTheHandler`; `ModelBindingTests.SaveNoteAndSearchNotes_BindTheSameInputType`, `BothHandlersOfTheForm_ReadTheSameFields` |
| 3 | Bağlama yalnız modelin alanlarınadır; fazladan alan yok sayılır; `Items` kalem başına tek satır, tekrar 422, toplama yok, satır işlenmez | ✓ | `ModelBindingTests.Post_WithFieldsTheModelDoesNotHave_IgnoresThem_…`, `Model_HasOnlyItsOwnFields_…`, `Post_WithTwoRowsOfTheSameItem_Is422_AddsNothingUp_AndProcessesNoRow` (1+2 ve 3+3), `…ApartAndAnotherBetween…`, `…OneRowPerItem_Is303…`; `ItemRowsTests` (4) |
| 4 | `06` varlıkları doğrudan bağlanmaz | ✓ | `ModelBindingRules` `EntityBound`, `EntityInModel`, `EntityIsModel`; uygulama assembly'sinde ihlal yok: `ApplicationAssembly_BreaksNoModelBindingRule`; ihlal örnekleri `BindsEntityPage`, `WrapsEntityInput`, `ExposedEntityInput` |
| 5 | Sunucunun türettiği değer istekten bağlanmaz; geri gelen gizli alanlar modelin alanı | ✓ (payı kriter metninde T207, T208, T209'a paylaştırılmış) | `Post_WithFieldsTheModelDoesNotHave_…` (`Price`, `Total`, `Id`, `Status` ve `input.` önekli biçim yok sayılır, fiyat `ServerPrice` kalır). Gizli alanların sahiplik/sürüm/tüketim doğrulaması T207, T208, T209'un işidir (kriterin kendi atfı); bu task'ta alan adı kuralı gizli alanlara da işler (`SpecAssignedName` ya da 06 adı) |
| 6 | Alan adı `06 §4`'te karşılığı varsa onun adı, yoksa `07`'nin bir kez verdiği ad; kapalı liste değeri `06 §5` kod adı | ✓ (biçim denetimi) | `FieldName` ve `SpecNameShadows06` kuralları (`ModelBindingRules.WalkModel`), ihlal örneği `WrongFieldNamesInput`; uygulamada ihlal yok. `07 §5.2`/§5.3 tablolarıyla tam eşitlik T538'in işidir (`11` K-1416 (2)); kapalı liste değerinin türü uygulamada `06 §5` enum'larıdır (enum dosya kuralı `EnumSourceScan`, T103) |
| 7 | Doğrulama sunucuda yetkilidir; HTML nitelikleri iyileştirmedir | ✓ | `ModelBindingTests.TheFormCarriesTheHtmlAttributes_AsAnImprovementOnly` + `Post_ChangedAgainstItsHtml_IsRejectedByTheServer_…` (boş, boşluk, 31 karakter, listede olmayan ve boş `Reason`) |
| 8 | Doğrulama hatası 422 ve alan mesajıyla gösterilir | ✓ | `ValidationPageFilter` + `FieldMessageCatalog` + nitelik sınıfları (`09 §3.4`, D-119 T211'de karşılandı); `FieldValidationTests.Post_WithSeveralWrongFields_Is422_WithEveryFieldsCatalogMessageTogether_AndNoCodeOnThePage`, `Post_Rejection_WritesOneLineWithTheFieldsAndCodes_…`; `Post_ChangedAgainstItsHtml_…` 422, Location yok, mesaj, kayıt yazılmadı |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: örnek üç model türünün ad kalıbı doğru | ✓ | `ModelBindingRuleTests.Sample_QuietNeighbours_ProduceNothing` (`AddNoteInput`, `AdminAddNoteInput`, `NoteQuery`, `StartUploadRequest`, `UploadStatusResponse`), `ModelNaming_ReadsTheKindFromTheSuffix` |
| KK2: iki işleyici aynı girdi türünü kullanır | ✓ | `ModelBindingTests.SaveNoteAndSearchNotes_BindTheSameInputType`; kural örneği `CleanNotesPage` (`OnPostAddNoteAsync` ve `OnPostSearchNotesAsync` → `AddNoteInput`) sessiz |
| KK3: fazla alan modele girmez; aynı kalemi iki satırda taşıyan `Items` 422, hiçbir satır işlenmez | ✓ | `ModelBindingTests` yukarıdaki satırlar; `Store.Returns()` boş |
| KK4: işleyici imzasında entity türü yok | ✓ | `ApplicationAssembly_BreaksNoModelBindingRule`, `Sample_ReportsEveryRuleOnItsOwnViolation` (`EntityBound:` üç ihlal: bağlı özellik, parametre, koleksiyon parametresi) |
| KK5: fiyat gibi türetilmiş değer formdan okunmaz | ✓ | `Post_WithFieldsTheModelDoesNotHave_…` ve `…UnderItsParameterPrefix…`: `NoteRecord("masa", "Damaged", ServerPrice)` |
| KK6: alan adları `06 §4` ile, karşılığı olmayanlar `07`'nin verdiği adla eşit | ✓ (biçim) | `FieldName`, `SpecNameShadows06` ihlal örnekleri; tam envanter eşitliği T538 |
| KK7: HTML'i değiştirilmiş istek sunucuda reddedilir | ✓ | `Post_ChangedAgainstItsHtml_IsRejectedByTheServer_With422AndTheFieldMessage` (5 satır) |
| KK8: hata 422 ve alan mesajı | ✓ | `FieldValidationTests.Post_WithSeveralWrongFields_…`, `Post_ChangedAgainstItsHtml_…`; `Post_WithoutTheFieldsAtAll_Is422WithBothFieldMessages_…` |
| `09 §25` ilgili satırlar | ✓ | ad sözlükten/belgeden (`InvalidListValue` `07 §6.2.2`'de); yeni dosya `CONTEXT.md`'de; diff yalnız göreve ait; ayar/sabit eklenmedi; zaman ve para yok |
| Güvenlik taraması | ✓ | aşağıda "Mini güvenlik kontrolü" |

### `09 §3.4` maddeleri (yönetici kararıyla T211'de; D-119)

| Madde | Sonuç | Kanıt |
|---|---|---|
| m.1 Nitelikler: her kod bir nitelik sınıfı, kod iletide; yerleşik nitelik yok (mimari test); örtük zorunluluk kapalı; sınır sayı değil sabit | ✓ | `FieldRules.cs` (12 sınıf), `FieldRuleTests`; `ModelBindingRules` `BuiltInRule` + ihlal örneği `Models.cs`, `ModelBindingRuleTests`; `FieldValidationSetup` (`SuppressImplicitRequiredAttributeForNonNullableReferenceTypes`), `FieldValidationTests.FrameworkOptions_AreSet_…`, `Post_WithoutANonNullableTextThatHasNoRule_IsNotRefused_…`; `FieldRuleSourceScan` + `NumberOnRule.cs`, `FieldRuleSourceScanTests` (tur 1 düzeltmesi: listede ikinci nitelik, `Attribute` sonekli ad ve adlı bağımsız değişken de yakalanır, her biçim için ihlal ve sessiz komşu satırı); şifre işareti artık `ValidationAttribute` olmayan `[PasswordField]`, `SignInInput` sessiz komşusu ve `DataTypePasswordInput` ihlali (`ModelBindingRuleTests`) |
| m.2 Alana özgü kural (`IValidatableObject`) ve işleyicinin eklediği kural aynı yoldan | ✓ | `FieldError.AddFieldError`; `FieldValidationTests.Post_HandlerOwnedRule_GoesThroughTheSameTranslation_AfterTheHandlerReturns`; `ItemRows` `AddFieldError` kullanır |
| m.3 Süzgeç: kayıt (alan adı + kod), katalog mesajı, 422 işleyiciye bırakmadan; bütün hatalar birlikte; kod sayfaya yazılmaz | ✓ | `ValidationPageFilter`; `Post_WithSeveralWrongFields_…_AndNoCodeOnThePage`, `Post_Rejection_WritesOneLineWithTheFieldsAndCodes_…`, `Get_WithARejectedQuery_ShowsTheMessageWithStatus200_…` |
| m.4 Mesaj kataloğu: her kodun kalıbı, her kalıbın kodu `07 §6.2.2`'den | ✓ | `FieldMessageCatalog`; `FieldMessageCatalogTests` (kodlar `07`'den okunur) |
| m.5 Boşluk kırpma; yalnız boşluk boş dizge; şifre ve işaretli alan kırpılmaz | ✓ | `TrimmingBinder`, `NotTrimmedAttribute`, `PasswordFieldAttribute` (şifre `[PasswordField]` ile tanınır, `[DataType(DataType.Password)]` ile değil — o `BuiltInRule` ihlali); `Post_TrimsTexts_MakesWhiteSpaceOnlyTheEmptyString_AndKeepsAPasswordAndAMarkedFieldAsTyped`, `Post_WithWhiteSpaceOnlyInARequiredField_…` |
| m.6 İstemci: HTML nitelikleri parametre sabitinden ve iyileştirme; `data-val-*` kapalı | ✓ | `BindingForm.cshtml` (`BindingLimits` sabiti), `TheFormCarriesTheHtmlAttributes_AsAnImprovementOnly`; `ClientValidationEnabled=false`; `Get_RendersNoDataValAttribute_…`, `Get_WithTheSwitchTurnedOnAgain_RendersDataVal_…`; `FieldRuleSourceScan` istemci doğrulama çıktısı taraması |
| m.7 Test: nitelik başına birim testi; süzgeç testi; kırpma testi | ✓ | `FieldRuleTests` (nitelik başına geçerli/geçersiz + kod), `FieldValidationTests` (süzgeç, kırpma) |

`12`: VAL-A578, A579, A581 satırlarının `Durum`'u BEKLEMEDE kalır (satırlar akış ucunda — UC-40, UC-43, UC-129, UC-130 — sınanır; bu task kalıbı sınar), değişmedi.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 1757 geçti | `dotnet test --project tests/Shopfolio.UnitTests` | main (T207 sonrası) 1628; +129 (ModelBindingRuleTests, ItemRowsTests, FieldRuleTests, FieldMessageCatalogTests, FieldRuleSourceScanTests) |
| Integration | ✓ 1392 geçti (Web ad alanı 220 + öteki ad alanları 1172) | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-namespace Shopfolio.IntegrationTests.Web` ve `--filter-not-namespace` aynı (PostgreSQL 18, 127.0.0.1) | Web: main (T207 sonrası) 188;  +ModelBindingTests 16, FieldValidationTests 16; `PageFormPatternTests` 27 (hepsi geçti) |
| Derleme/biçim | ✓ | `dotnet build -warnaserror`; `dotnet format --verify-no-changes` | 0 uyarı, 0 hata, biçim temiz |
| Contract | — | — | Bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-06, head `8e99ab6`) |
| Bulgu sayısı | 2 — B1 ORTA S1: şifrenin kırpılmama işareti `[DataType(DataType.Password)]` `ModelBindingRules`'un `BuiltInRule` ihlalidir (`DataTypeAttribute` bir `ValidationAttribute`; doğrulayıcı sondası `BuiltInRule:…Password:DataTypeAttribute`), `09 §3.4` m.1 ile m.5 gerçek bir şifre modelinde birlikte sağlanamaz — örnek sayfa test derlemesinde olduğu için kural görmüyor. B2 DÜŞÜK S1: `FieldRuleSourceScan` sayı taraması `[FieldRequired, TextTooLong(150)]`, `[TextTooLongAttribute(150)]`, `[TextTooLong(max: 150)]` yazımlarını kaçırır |
| Düzeltme gerekli mi | Evet — B1 (dar istisna: tam türü `DataTypeAttribute` ve `DataType.Password`; ya da projenin şifre işareti; ihlal örneğine sessiz komşu + kırmızı kalan `EmailAddress`) ve B2 (desen genişler, `NumberOnRule.cs`'e üç satır) bu PR'da, tur 2 |
| Bloke-etmeyen not (K) sayısı | 3 — K1 → D-123 (örnek sayfada `Max = 1000`; `data-val` deseni `data-value`'yu yakalar), K2 → D-124 (tutar/yüzde mesajı sınırı göstermiyor; T216), K3 → D-125 (`IsSimple` `DateTimeOffset`'i geçirir, `IFormFile` reddedilir; T216, T312) |

Doğrulayıcı kanıtları (tur 1): build `-warnaserror` 0/0, format temiz, Unit 1757/1757, betik testleri 4 dosya, Integration 1392/1392 (PostgreSQL 18, filtreli parçalar; `--list-tests` 1392), `restore --locked-mode` 0, dal CI 42139962380 success. Kriter 1–8 ve KK1–KK8 ✓; `09 §3.4` m.1 ve m.5 ~ (B1). Ayrıca "Dış Varsayımlar" #3 `[AllowedValues]`/`[Required]` der; kod projenin niteliklerini kullanır — düzeltme turunda güncellenmeli.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (UnitTests csproj'a ihlal örneği derleme hedefi eklendi)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T211-model-baglama-kalibi`
- **Commit:** son commit PR gövdesinde ve dönüşte
- **main ile birleştirme (T207 #403 sonrası):** `origin/main` dala merge edildi. Çakışmalar: `Program.cs` (`AddSessionAccess` + `AddFieldValidation` ve `AddSessionAuthentication` birlikte), `DEFERRED_BACKLOG.md` (D-118 ✅ ve D-120–D-122 main'den, D-119 ✅ daldan; aktif kalem 65), `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `MEMORY.md`, `T206_REPORT.md`. T207 panel klasörünü varsayılan kapattığı için `ModelBindingTests` ve `FieldValidationTests` yönetici oturumu çerezi (`TestSessions.AdminAsync`) taşır; erişim kuralı gevşetilmedi.
- **PR:** #402
- **CI:** yapım işçisi izlemez (yönetici yardımcı düzeni)
- **Post-merge run'lar:** *(validator doldurur)*

### Doğrulama tur 1 düzeltmeleri

- **B1 (ORTA):** Şifre işareti için `BuiltInRule` istisnası yerine projenin kendi işareti seçildi: `PasswordFieldAttribute` (`ValidationAttribute` değil). Gerekçe: `09 §3.4` m.1 "alan yalnız projenin kural niteliklerini taşır" der; istisna açmak yasağı delerdi (`DataType` türevleri `EmailAddress` gibi sızabilirdi), ayrı işaret ise kuralı olduğu gibi bırakır ve en dar çözümdür. `TrimmingBinder` artık şifreyi `[PasswordField]` ya da `[NotTrimmed]` ile tanır; `DataTypeName` okuması kalktı. Örnek sayfa `[PasswordField]` kullanır. Testler: sessiz komşu `SignInInput` (`[PasswordField]` + `[NotTrimmed]`, `Sample_QuietNeighbours_ProduceNothing`) ve ihlal `DataTypePasswordInput` (`BuiltInRule:…:DataTypeAttribute`, `Sample_ReportsEveryRuleOnItsOwnViolation`); kural uygulama derlemesinde de koşar (`ApplicationAssembly_BreaksNoModelBindingRule`), dolayısıyla gerçek bir `[PasswordField]` modeli çakışmaz.
- **B2 (DÜŞÜK):** Desen `\b(TextTooLong|InvalidQuantity)(Attribute)?\s*\(\s*(\w+\s*:\s*)?\d` ve `\bInvalidAmount(Attribute)?\s*\([^)\]]*Max\s*=\s*\d` oldu. `NumberOnRule.cs`'e 5 ihlal (`[FieldRequired, TextTooLong(150)]`, `[TextTooLongAttribute(150)]`, `[TextTooLong(max: 150)]`, `[InvalidAmountAttribute(AllowZero = true, Max = 5)]`, `[FieldRequired, InvalidQuantity(max: 5)]`) ve 3 sessiz komşu (sabitten sınır, aynı biçimlerde) eklendi; uygulama kaynağı taraması temiz (yanlış pozitif yok).
- K1–K3 backlog'da (D-123–D-125); bu turda yeniden yapılmadı.
- Kanıt: build `-warnaserror` 0/0, format temiz, Unit 1757/1757 (sayı değişmedi: yeni satırlar mevcut testlerin içinde), Integration `FieldValidationTests` 16/16 ve `ModelBindingTests` 16/16 (PostgreSQL 18, `shopfolio-t211-pg`, iş bitince silindi).

## Dış Varsayımlar

Dış varsayım yok (plan düzeyi tetikleyici terimler — plan tier, kota, API anahtarı, paket sürümü, sandbox — kabul kriterlerinde geçmiyor). Çatı davranışı (bilgim tarihli) yerelde sınandı:

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Razor Pages işleyici parametresi önek bulunmazsa öneksiz alanlardan bağlanır; modelde olmayan alan yok sayılır | `ModelBindingTests.Post_WithFieldsTheModelDoesNotHave_…` | Doğrulandı |
| 2 | Parametre adıyla öneklenen alan (`input.Name`) varsa önekli okunur, önekli ile öneksiz karışmaz (yalnız önekli gönderen kendini 422'ye düşürür) | Geliştirme sırasında gözlendi; `…UnderItsParameterPrefix…` | Doğrulandı |
| 3 | Çatı doğrulaması boş ve listede olmayan değeri `ModelState`'e yazar (kod projenin `FieldRequired`/`InvalidListValue` niteliklerini kullanır) | `Post_ChangedAgainstItsHtml_…` | Doğrulandı |
| 4 | Yerel PostgreSQL 18 konteyneri | `shopfolio-t211-pg`, 127.0.0.1:54311 | Doğrulandı, iş bitince silinir |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `07 §5.2`/§5.3 tablolarıyla model alanlarının tam eşitliği | — (T538'in kapsamı, `11` K-1416 (2)) | T538 |

Hedefi T211 olan backlog kaydı yoktu (`grep -n "T211" Docs/DEFERRED_BACKLOG.md` boş). Açılan ve aynı task'ta kapanan: D-119 (✅, yönetici kararı: `09 §3.4` T211'de yazıldı).

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks` (depo kökü).
- **Ana dal CI startup (Adım 0):** ana dalın son üç tamamlanmış run'ı success: Publish 42128519903, CI 42128519929 (T206 #401) ve CI 42113408503 (T224 #400).
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Mini güvenlik:** sır yok; yetki etkisi yok (yeni uç yok); girdi doğrulaması etkisi: yalnız kural ve örnek; yeni dış bağımlılık yok.
- **⚠ kararlar (kolay, öneriyle):** (a) Aynı kalemin tekrar satırının hata kodu `InvalidListValue` (`07 §6.2.2`'de "kalem seçimi" tek bir kodla anılmıyor; `FieldRequired` kalem seçimi boşluğu, kapalı küme dışı değer `InvalidListValue`); (b) Kind işaretleri arayüz (kural motoru yansımayla bulur; `CA1040` gerekçeyle bastırıldı); (c) `Original…`, `Version` gibi ortak gizli alanların adı için ayrı liste tutulmadı: `06`'da yoksa `SpecAssignedName` ister; (d) sayısal dönüşüm hatası alanın `OwnsConversionFailure` kuralının koduna, böyle kural yoksa `InvalidListValue`'ya çevrilir; (e) katalog metinleri ortak kalıptır, sahip task'ların (K-772, K-799, K-817) metinleri o task'larda ince ayarlanır.
- **PageFormPatternTests 3 kırığının nedeni (T206 örnek sayfası):** sayfanın girdisi `[BindProperty]` idi; süzgeç artık işleyiciden önce bağlı modelin hatasını 422'ye çevirdiğinden, form göndermeyen `Refuse` ve `Export` işleyicileri de `Name` zorunluluğuna takıldı (üçüncüsü `?handler=Refuse` ile gönderen test). Süzgeç doğru davranıyor (bağlanan model doğrulanır); örnek sayfa düzeltildi: `Add` girdisini işleyici parametresi alır, sayfa gönderilen değeri `ModelState`'ten gösterir. Testler gevşetilmedi. Kalıp notu: birden çok işleyicili sayfada girdi bağlı özellik değil işleyici parametresidir (`CONTEXT.md` Validation satırı).
