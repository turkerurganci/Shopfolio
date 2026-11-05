# T211 — Model bağlama kalıbı

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-06

> Taslağı yapım işçisi doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Infrastructure/Binding/` (yeni klasör): `IFormInput`, `IQueryModel`, `IMachineRequest`, `IMachineResponse` işaretleri (üç model türü), `ModelNaming` (sonek ve işleyici adı kalıbı), `SpecAssignedNameAttribute` (`06 §4`'te karşılığı olmayan, `07`'nin bir kez verdiği alan adı), `ItemRows.RejectRepeated` (`Items` kalem başına tek satır).
- Mimari kural motoru `UnitTests/Architecture/ModelBindingRules` (yansıma): ad–tür, işleyici adı, entity bağlanmaz, alan adı. Kuralın ihlal örneği ayrı derlemede (`ModelBindingViolationSample.dll`; `ViolationSamples/ModelBinding/Models.cs`), her kuralın kendi ihlali ve sessiz komşuları var.
- Test amaçlı örnek sayfa `IntegrationTests/Pages/Panel/BindingForm` (yalnız test sunucusunda): iki işleyici aynı girdiyi bağlar, modelde olmayan alan, sunucunun fiyatı, `Items`, HTML'i değiştirilmiş istek.
- Önceki task kaydı: T206'nın status Commit hücresi (`494a34c`) ve rapor "Post-merge run'lar" satırı dolduruldu (skill adım 15); eski status girişi `STATUS_CHANGELOG.md`'ye taşındı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Binding/{ModelKinds,ModelNaming,SpecAssignedNameAttribute,ItemRows}.cs` (yeni); `Program.cs` ve ortak dosyalara dokunulmadı.
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
| 5 | Sunucunun türettiği değer istekten bağlanmaz; geri gelen gizli alanlar modelin alanı | ✓ (kısmen başka task'ta) | `Post_WithFieldsTheModelDoesNotHave_…` (`Price`, `Total`, `Id`, `Status` ve `input.` önekli biçim yok sayılır, fiyat `ServerPrice` kalır). Gizli alanların sahiplik/sürüm/tüketim doğrulaması T207, T208, T209'un işidir (kriterin kendi atfı); bu task'ta alan adı kuralı gizli alanlara da işler (`SpecAssignedName` ya da 06 adı) |
| 6 | Alan adı `06 §4`'te karşılığı varsa onun adı, yoksa `07`'nin bir kez verdiği ad; kapalı liste değeri `06 §5` kod adı | ✓ (biçim denetimi) | `FieldName` ve `SpecNameShadows06` kuralları (`ModelBindingRules.WalkModel`), ihlal örneği `WrongFieldNamesInput`; uygulamada ihlal yok. `07 §5.2`/§5.3 tablolarıyla tam eşitlik T538'in işidir (`11` K-1416 (2)); kapalı liste değerinin türü uygulamada `06 §5` enum'larıdır (enum dosya kuralı `EnumSourceScan`, T103) |
| 7 | Doğrulama sunucuda yetkilidir; HTML nitelikleri iyileştirmedir | ✓ | `ModelBindingTests.TheFormCarriesTheHtmlAttributes_AsAnImprovementOnly` + `Post_ChangedAgainstItsHtml_IsRejectedByTheServer_…` (boş, boşluk, 31 karakter, listede olmayan ve boş `Reason`) |
| 8 | Doğrulama hatası 422 ve alan mesajıyla gösterilir | ✓ (kalıp düzeyi) | `FormResults.Redisplay` (T206) + `ModelState`; `Post_ChangedAgainstItsHtml_…` 422, Location yok, `data-field="…"` mesajı, kayıt yazılmadı; `Post_WithoutTheFieldsAtAll_…` iki alan mesajı. Mesaj kataloğu ve nitelik sınıfları T211'in değildir: bkz. D-119 |

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
| KK8: hata 422 ve alan mesajı | ✓ | aynı test; `Post_WithoutTheFieldsAtAll_Is422WithBothFieldMessages_…` |
| `09 §25` ilgili satırlar | ✓ | ad sözlükten/belgeden (`InvalidListValue` `07 §6.2.2`'de); yeni dosya `CONTEXT.md`'de; diff yalnız göreve ait; ayar/sabit eklenmedi; zaman ve para yok |
| Güvenlik taraması | ✓ | aşağıda "Mini güvenlik kontrolü" |

`12`: VAL-A578, A579, A581 satırlarının `Durum`'u BEKLEMEDE kalır (satırlar akış ucunda — UC-40, UC-43, UC-129, UC-130 — sınanır; bu task kalıbı sınar), değişmedi.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 1546 geçti | `dotnet test tests/Shopfolio.UnitTests` | önce 1515; +31 (ModelBindingRuleTests, ItemRowsTests) |
| Integration | ✓ Web ad alanı 169 geçti (153 + ModelBindingTests 16) | `dotnet test tests/Shopfolio.IntegrationTests --filter-namespace Shopfolio.IntegrationTests.Web` (PostgreSQL 18, 127.0.0.1) | Yalnız test sayfası ve test eklendi; öteki ad alanları değişmedi |
| Contract | — | — | Bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (UnitTests csproj'a ihlal örneği derleme hedefi eklendi)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T211-model-baglama-kalibi`
- **Commit:** son commit PR gövdesinde ve dönüşte
- **PR:** #PR_NO
- **CI:** yapım işçisi izlemez (yönetici yardımcı düzeni)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım yok (plan düzeyi tetikleyici terimler — plan tier, kota, API anahtarı, paket sürümü, sandbox — kabul kriterlerinde geçmiyor). Çatı davranışı (bilgim tarihli) yerelde sınandı:

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Razor Pages işleyici parametresi önek bulunmazsa öneksiz alanlardan bağlanır; modelde olmayan alan yok sayılır | `ModelBindingTests.Post_WithFieldsTheModelDoesNotHave_…` | Doğrulandı |
| 2 | Parametre adıyla öneklenen alan (`input.Name`) varsa önekli okunur, önekli ile öneksiz karışmaz (yalnız önekli gönderen kendini 422'ye düşürür) | Geliştirme sırasında gözlendi; `…UnderItsParameterPrefix…` | Doğrulandı |
| 3 | `[AllowedValues]` ve `[Required]` boş ve listede olmayan değeri `ModelState`'e yazar | `Post_ChangedAgainstItsHtml_…` | Doğrulandı |
| 4 | Yerel PostgreSQL 18 konteyneri | `shopfolio-t211-pg`, 127.0.0.1:54311 | Doğrulandı, iş bitince silinir |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `09 §3.4` girdi doğrulama kalıbının (nitelik sınıfları, sayfa süzgeci, mesaj kataloğu, kırpma bağlayıcısı) planda sahibi yok | D-119 | Plan düzeltmesi gerekir; belgede olmayan davranışı koda sokmamak için T211'de yazılmadı |
| 2 | `07 §5.2`/§5.3 tablolarıyla model alanlarının tam eşitliği | — (T538'in kapsamı, `11` K-1416 (2)) | T538 |

Hedefi T211 olan backlog kaydı yoktu (`grep -n "T211" Docs/DEFERRED_BACKLOG.md` boş). Açılan: D-119. Kapanan: yok.

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks` (depo kökü).
- **Ana dal CI startup (Adım 0):** ana dalın son üç tamamlanmış run'ı success: Publish 42128519903, CI 42128519929 (T206 #401) ve CI 42113408503 (T224 #400).
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Mini güvenlik:** sır yok; yetki etkisi yok (yeni uç yok); girdi doğrulaması etkisi: yalnız kural ve örnek; yeni dış bağımlılık yok.
- **⚠ kararlar (kolay, öneriyle):** (a) Aynı kalemin tekrar satırının hata kodu `InvalidListValue` (`07 §6.2.2`'de "kalem seçimi" tek bir kodla anılmıyor; `FieldRequired` kalem seçimi boşluğu, kapalı küme dışı değer `InvalidListValue`); (b) Kind işaretleri arayüz (kural motoru yansımayla bulur; `CA1040` gerekçeyle bastırıldı); (c) `Original…`, `Version` gibi ortak gizli alanların adı için ayrı liste tutulmadı: `06`'da yoksa `SpecAssignedName` ister.
