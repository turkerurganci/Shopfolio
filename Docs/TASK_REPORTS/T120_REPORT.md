# T120 — Model eşitlik denetimleri

**Faz:** F1 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-04

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `UnitTests/Consistency/` altında, hepsi `[Trait("Consistency","true")]` ve veritabanına/ağa dokunmayan altı eşitlik denetimi ailesi: ilişki (`RelationshipConsistencyTests`), dizin (`IndexConsistencyTests`), kontrol kısıtı (`CheckConstraintConsistencyTests`), saklanan türetilmiş değer (`StoredDerivedValueConsistencyTests`). Okuyucular: `DataModelDocument` (06'yı bölüm/tablo/Mermaid olarak okur), `DocumentedRelationships`, `DocumentedIndexes`, `DocumentedChecks`, `FilterAtoms` (filtre düzyazısı → SQL atomları), `SetComparison` (iki yönlü karşılaştırma), `ModelFacts` (bitmiş modelden yabancı anahtar, dizin, kısıt, özellik; yer tutucu bağlantı dizesiyle, bağlantı açılmaz), `CheckConstraintTable` (kod listesi).
- Her denetim iki yönlüdür (belge fazlası ve model fazlası) ve her biri için ayrı ihlal örneği vardır: belge ya da model kopyası değiştirilir, kaynak değiştirilmez (09 §19.2 m.3).
- `UnitTests/Architecture/TestIsolationScan` + `TestIsolationScanTests`: KK7 ve KK8 için kaynak taraması, yansıma denetimi ve CI komutu denetimi; ihlal örnekleriyle.
- Belge–model uyuşmazlığı **bulunmadı**: 130 çizgi/130 FK, 7.1 tekil dizinler (filtreler dahil), 7.5.1 sorgu dizinleri, 7.2.1 dizin vaatleri, 7.3 kısıt tabloları, telefon, sayfa adresi, enum değer kümeleri, 2.7 envanteri modelle eşit çıktı.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/*` (yeni: `ModelFacts`, `DataModelDocument`, `SetComparison`, `FilterAtoms`, `Documented*`, `CheckConstraintTable`, dört `*ConsistencyTests`)
- `tests/Shopfolio.UnitTests/Architecture/TestIsolationScan.cs`, `TestIsolationScanTests.cs`
- Üretim koduna ve göçe dokunulmadı.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Diyagramın 130 çizgisi = modelin yabancı anahtarları | ✓ | `RelationshipConsistencyTests.Diagrams_AndTheForeignKeysOfTheModel_AreTheSameSet`, `Diagrams_LineCountsAreTheStatedOnes_AndEqualTheModelsForeignKeyCount` |
| 2 | Tekil dizin listesi ve filtreleri modelle eşit | ✓ | `IndexConsistencyTests.UniqueIndexTable_AndTheUniqueIndexesOfTheModel_AreTheSameSet` (filtre atomları dahil) |
| 3 | FK ve silme davranışı listesi modelle eşit; `RESTRICT` yok | ✓ | `ForeignKeyTable_AndTheForeignKeysOfTheModel_AreTheSameSet`, `ForeignKeyTable_BehaviorCounts_...`, `Model_UsesTheThreeBehaviorsOnly_AndNoRestrict` |
| 4 | Kontrol kısıtı listesi modelle eşit | ✓ | `CheckConstraintConsistencyTests` (tablo kümesi, kod listesi + madde sayısı, slug, telefon, enum) |
| 5 | Sorgu dizinleri ve bağların kendi dizinleri modelle eşit | ✓ | `IndexConsistencyTests.QueryIndexTable_AndTheOtherIndexesOfTheModel_AreTheSameSet`, `RelationshipConsistencyTests.ForeignKeyIndexes_ThatTheTablePromises_ExistInTheModel` |
| 6 | Saklanan türetilmiş değerler envanterin dört türüyle sınırlı | ✓ | `StoredDerivedValueConsistencyTests.StoredDerivedValues_OfTheModel_AreExactlyTheInventory`, `PanelMarks_OfTheDocument_AreNotStoredByTheModel` |
| 7 | Göçe dönüşmemiş değişiklik denetimi birim testi değil | ✓ | `TestIsolationScanTests.UnitTests_Sources_ApplyNoMigrationAndLookForNoPendingModelChanges` |
| 8 | `Consistency` özellikli, Lint'te koşar, Unit'te dışarıda, DB/ağ yok | ✓ | `ConsistencyChecks_Types_CarryTheConsistencyTrait_AndNoOtherTypeDoes`, `Pipeline_RunsTheConsistencyChecksInLintOnly`, `ConsistencyChecks_Sources_ReadNoConnectionStringOrEnvironmentAndOpenNoConnection` |

## Öz Denetim (PR öncesi; her madde kendi diff'inde denetlendi)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1: 130 çizgi, 130 FK | ✓ | `Diagrams_LineCountsAreTheStatedOnes_AndEqualTheModelsForeignKeyCount` (çizgi = satır = bağ = model FK sayısı) |
| KK2: tekil dizin satırı silinince kırılır | ✓ | `UniqueIndexTable_ADeletedRow_IsReported`, `UniqueIndexTable_AnExtraRowAndAChangedFilter_AreReported`, `UniqueIndexTable_AFilterChangedInTheModel_IsReported`, `UniqueIndexTable_ASingleRowIndexOrAUniqueIndexMissingInTheModel_IsReported` |
| KK3: bağın davranışı değişince kırılır | ✓ | `ForeignKeyTable_ABehaviorChangedInTheModel_IsReported`, `..._InTheDocument_IsReported`, `ForeignKeyTable_ADeletedRow_IsReported`, `Diagrams_AChangedMarkOrBehavior_IsReported`, `Behaviors_ARestrictiveForeignKeyOrMigrationText_IsReported` |
| KK4: kontrol kısıtı eksilince kırılır | ✓ | `CodeList_AConstraintMissingFromTheModel_IsReported`, `CodeList_AConstraintTheListDoesNotKnow_IsReported`, `Tables_ADocumentRowDeleted_OrAModelTableThatLostItsConstraints_IsReported`, `Items_AnItemAddedToOrRemovedFromARow_IsReported`, `Phones_...`, `Slugs_...`, `Enums_...` |
| KK5: sorgu dizini / bağın kendi dizini eksilince kırılır | ✓ | `QueryIndexTable_ADeletedRow_IsReported`, `QueryIndexTable_AnIndexMissingFromTheModelOrWithAChangedFilter_IsReported`, `ForeignKeyIndexes_ADeletedOwnIndexOrComposite_IsReported` |
| KK6: envanterde olmayan saklanan alan kırar | ✓ | `Model_AStoredValueTheInventoryDoesNotName_IsReported` (`Variant.StockCache`), `Model_AnInventoryValueTheModelLost_IsReported`, `Document_AFifthKindInTheInventoryTable_IsReported`, `Marks_APanelMarkStoredAsAProperty_IsReported` |
| KK7: UnitTests altında göç uygulayan / bekleyen model değişikliği arayan test yok | ✓ | `UnitTests_Sources_ApplyNoMigrationAndLookForNoPendingModelChanges` (tüm `tests/Shopfolio.UnitTests/**/*.cs` taranır, 0 bulgu); ihlal örnekleri: `Scan_AUnitTestThatLooksForModelChangesOrPendingMigrations_IsReported` (4), `Scan_AUnitTestThatAppliesMigrations_IsReported` (2); denetim T04'ün dry-run adımındadır |
| KK8: `Consistency` özelliği, yalnız Lint, bağlantı dizesi okumaz | ✓ | yansıma: `ConsistencyChecks_Types_CarryTheConsistencyTrait_AndNoOtherTypeDoes` (+ `TraitProblems_...AreReported` ihlali); CI: `Pipeline_RunsTheConsistencyChecksInLintOnly` (`.github/ci-commands.env` `LINT_CMD` `--filter-trait`, `UNIT_TEST_CMD` `--filter-not-trait`; ihlal `Pipeline_AUnitStepWithoutTheExclusion_...`); kaynak: `ConsistencyChecks_Sources_Read...` (+ 4 ihlal örneği); `ModelFacts` yalnız yer tutucu dize taşır, bağlantı açılmaz |
| 09 §25 — doküman uyumu ve kapsam | ✓ | 06 §2.7, §6, §7 okundu; kodda yeni alan adı yok; diff yalnız test; çelişki bulunmadı |
| 09 §25 — test (§19.2 m.3, §21.4 m.1) | ✓ | her denetimin kasıtlı ihlalli örneği var; eşitlik denetimleri `UnitTests/Consistency/` + `Consistency` özelliği; kaynak taraması `UnitTests/Architecture/` + ihlal örneği; testler dosya yazmaz, DB/ağ/sır yok |
| 09 §25 — mimari ve kod, veri ve göç, performans | ✓ | üretim kodu, göç ve ayar değişmedi; Release derlemesi `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` temiz |
| Güvenlik taraması | ✓ | diff'te sır, parola, token, gerçek bağlantı dizesi yok (yer tutucu `Host=localhost;Database=shopfolio_consistency_model` hiçbir testte açılmaz); dosya yazma/ağ yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-trait "Consistency=true"` | 100 geçti, 0 kaldı |
| Unit (Unit adımı) | ✓ | `... --filter-not-trait "Consistency=true"` | 241 geçti, 0 kaldı |
| Integration | — | koşulmadı (üretim koduna dokunulmadı) | |
| Contract | — | yok | |

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
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T120-model-esitlik-denetimleri`
- **PR:** #378 (başlık "T120: Model eşitlik denetimleri")
- **CI:** izlenmedi (yapım işçisi CI beklemez)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Denetimler yalnız depodaki `Docs/06_DATA_MODEL.md`, göç dosyaları, `.github/ci-commands.env` ve derlenmiş modelden okur.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Yok | — | — |

## Notlar

- **⚠ Kararlar:** (1) Tek satır dizinleri (`((true))`) ve `ContactRequest` ifade dizini EF modelinde yoktur, göçlerde ham SQL'dir; denetim modelin yanında göç dosyalarındaki `CREATE INDEX` metnini de kaynak okur. (2) Diyagramdaki sağ işaret "bir" (`o|`) EF'in `IsUnique` değerinden değil, FK sütununda (tam ya da `col IS NOT NULL` filtreli) tekil dizinin varlığından türetilir; EF bu bağlarda FK'yi tekil saymaz.
- **Kısıt tablosu:** `CheckConstraintTable` kod listesidir (50 tablo, 166 kısıt); 06 §7.3.2'nin madde sayısıyla ve modelin kısıt adlarıyla iki yönlü karşılaştırılır — yeni kısıt eklenince liste de güncellenir (test kırılır).
- **KK6 istisna listesi dardır:** `DownloadCount`, `ConsecutiveChannelErrorCount`, iki `AttemptCount`, `DisposalRecord.Count`, `Order.Total` ve iki sepet bildirim işareti — her biri belgede kayıt verisi olarak yazılıdır; gerekçesi testin yanındadır.
- **Repo memory (bitiş kapısı 8):** güncellendi
