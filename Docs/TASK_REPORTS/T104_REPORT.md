# T104 — Ürün çekirdeği ve görsel

**Faz:** F1 | **Durum:** ✗ FAIL (doğrulama tur 1) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Modules/Catalog/` altında sekiz entity ve yapılandırması: `Product`, `Variant`, `Option`, `OptionValue`, `Category`, `ProductCategory`, `Image`, `ProductImage` (`06 §4.5`…`§4.10`, `§4.69`, `§4.71`, `§4.72`).
- Tek göç `CreateCatalogCoreTables` (`Down` yok; LF; anlık görüntüde `SingleRow` satırı yok).
- `Infrastructure/ProductConstants` (P-20, P-21, P-27), `ParameterFences` (+P-9, P-44 alt çit), `AppDbContext`'e sekiz `DbSet`.
- `TextNormalizer.Simplify` (`09 §9.4` madde 2): `Product.NameSearchForm` bu işlevle üretilir; plan başka bir task'a yazmadığı için bu task'a girdi (⚠ 1).
- D-50 (a): `ModelRulesConvention` tek satır işaretini kullandıktan sonra kaldırır; (b) `LegalTextVersion_NumberWithoutAPublishedMoment_IsRefused`; (c) `ModelRuleTests` sıradan tablonun her yabancı anahtarının dizinle başladığını sınar.
- Test altyapısı: `SampleDatabaseFixture` şemayı sıfırlarken `pg_trgm` eklentisini yeniden kurar (katalog trigram dizini); `EnumDatabaseTests` sayacı 8 → 14; örnek model basamak testi yalnız örnek varlıklara bakar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/*`, `Infrastructure/ProductConstants.cs`, `ParameterFences.cs`, `TextNormalizer.cs`, `AppDbContext.cs`, `Infrastructure/Migrations/*`, `DataModel/ModelRulesConvention.cs`.
- `tests/Shopfolio.IntegrationTests/Model/{CatalogCoreTablesTests,ModelRuleTests,SettingsTablesTests,EnumDatabaseTests,SampleDatabaseFixture}.cs`, `tests/Shopfolio.UnitTests/Infrastructure/DataModelFoundationTests.cs`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz entity tek göçle | ✓ | `Migration_MakesTheEightTables`; göç betiği yerelde PostgreSQL 18'e uygulandı |
| 2 | VatRate, süre çitleri, trigram dizin | ✓ | `CatalogCoreTablesTests` KDV/süre/dizin testleri |
| 3 | Varyant tekillikleri ve kısıtları | ✓ | stok kodu, `NULLS NOT DISTINCT`, fiyat/stok/ayrılmış adet testleri |
| 4 | Option sırası | ✓ | üçüncü seçenek ve sıra çiti testleri |
| 5 | Category düzeyi | ✓ | düzey/üst kategori testleri |
| 6 | Ana kategori/görsel, Slug | ✓ | filtreli tekil dizin ve Slug testleri |
| 7 | Image sınırları, basamak/kısıtlayıcı silme | ✓ | görsel ölçü ve silme testleri |
| 8 | Göç tek konu, `Down` yok | ✓ | `MigrationSourceScan` (Unit) |

*Validator kanıtı (tur 1, `b4c73c8`):* sekiz kriterin her biri PostgreSQL 18'e uygulanan göç üzerinde ham SQL, `pg_constraint`/`pg_indexes` ve sorgu planıyla yeniden sınandı — ✓ (ayrıntı: "Doğrulama tur 1"); verdict `09 §25` bulguları yüzünden FAIL.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 225/225 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests` (gerçek PostgreSQL 18) | 183/183 (iki parça) |
| Göç kuru koşusu | ✓ | `scripts/ci/migration-dry-run.sh` | boş veritabanına uygulandı; yayın etiketi yok, adım 3-4 atlandı |
| Görüntü derlemesi | ✓ | `docker build -f deploy/images/app/Dockerfile .` | başarılı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-04, head `b4c73c8`) |
| Bulgu sayısı | 3 (S1 DÜŞÜK ×3) |
| Düzeltme gerekli mi | Evet |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-51, K2 → D-45) |

### Doğrulama tur 1 — ✗ FAIL (2026-11-04)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` mutlak yol (`C:\projects\Shopfolio\scripts\git-hooks`, hook dosyaları daldakiyle birebir); ana dal son 3 run `41728889871`, `41728889974`, `41715894986` success; `MEMORY.md`'de T104 satırı var.

**Kabul kriterleri (validator kanıtı; PostgreSQL 18 konteyneri, `CreateCatalogCoreTables` boş veritabanına `dotnet ef database update` ile uygulandı):** 1–8 ✓, KK1–KK8 ✓. KK1 sekiz tablo, `has-pending-model-changes` temiz; KK2 `ck_products_vat_rate_range`, `shipping_period BETWEEN 0 AND 10` (P-5), `>= 1` alt çitleri (P-44, P-9), `ix_products_name_search_form` GIN `gin_trgm_ops` — `enable_seqscan = off` ile `LIKE '%celik%'` planı Bitmap Index Scan; KK3 `ix_variants_normalized_sku` tekil, kombinasyon dizini `NULLS NOT DISTINCT`, fiyat/net/ayrılmış/stok kısıtları; KK4 `ix_options_product_id_sort_order` tekil, `sort_order BETWEEN 1 AND 2`; KK5 `level BETWEEN 1 AND 3`, `(parent_category_id IS NULL) = (level = 1)`; KK6 filtreli tekil `ix_product_categories_product_id`, `ix_product_images_product_id`, filtresiz `ix_products_slug`, `ix_categories_slug`, Slug deseni `^[a-z0-9]+(-[a-z0-9]+)*$`; KK7 `pg_constraint.confdeltype` — products'a bağlı beş bağ basamaklı (`c`), diğer yedi bağ `NO ACTION` (`a`); KK8 göçte `Down` ve yasak deyim yok. Yerel: `restore --locked-mode` 0; Release `-warnaserror` 0 uyarı; Unit 225/225; betik testleri geçti; Integration 183/183. Dal CI `41735064804` (`b4c73c8`) dokuz iş success.

| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Dosya |
|---|---|---|---|---|---|
| 1 | `09 §10.6` m.8, m.11; `09 §25` "Test maddesi karşılanmış mı" | DÜŞÜK | S1 | Trigram dizinini kuran göçün ilk deyimi `pg_trgm`'nin kurulu olduğunu sınamalı ve açık iletiyle düşmeli (m.8 `DO $$ … RAISE EXCEPTION` kalıbı); m.11'in iki testi (eklentisiz veritabanında göç açık iletiyle düşer, öncekiler kalır; tarama kapalıyken arama dizini kullanır) ↔ denetim de testler de yok. Yoklama: eklentisiz PG18'de göç `42704: operator class "gin_trgm_ops" does not exist for access method "gin"` ile düştü. | `Migrations/20261103222027_CreateCatalogCoreTables.cs`, `CatalogCoreTablesTests.cs` |
| 2 | `06 §4.6`, `06 §4` girişi ("`Variant.SKU` … `NormalizedSKU`"), `09 §5.1` m.3, §9.4 m.1; `09 §25` "ad birebir mi" | DÜŞÜK | S1 | Özellik adları `SKU`, `NormalizedSKU` ↔ kodda `Sku`, `NormalizedSku` (`SetSku`). Sütun adları değişmez, göç etkilenmez. | `Modules/Catalog/Variant.cs`, testler |
| 3 | `09 §9.4` m.2 ("kalan aksanlı Latin harfler … Başka karaktere dokunulmaz"); `09 §25` "doküman cümlesiyle çelişmiyor mu" | DÜŞÜK | S1 | `Simplify` FormD sonrası her `NonSpacingMark`'ı atar ↔ Latin olmayan harfler de değişiyor (yoklama: `й`→`и`, `ά`→`α`, `कु`→`क`). İşaret yalnız Latin taban harfin ardından atılmalı; test eklenmeli. | `Infrastructure/TextNormalizer.cs`, `DataModelFoundationTests.cs` |

**K-notları:** K1 (→ D-51) EF, 67 baytlık türeyen dizin adını kendiliğinden 62 bayta kırpıyor (`ix_variants_product_id_first_option_value_id_second_option_val`); `09 §5.2` m.5 çatının kırpmasını anmıyor. K2 (→ D-45) `09 §10.6` m.8'in iki "bilgim tarihli" olgusu sınandı (`HasMethod`/`HasOperators` → Npgsql ek açıklamaları; plan dizini kullanır).

**Yapım raporu karşılaştırması:** kriter 1–8 ve test sayıları uyumlu; uyuşmazlık 3 — rapor ve PR `09 §25` sorularının her birini uyumlu der, bulgular 1–3 yapımda görülmedi. D-50 (a)(b)(c) kapanışı doğrulandı (anlık görüntüde `SingleRow` yok, iki yön testi ve FK dizin kilidi testi var).

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `CreateCatalogCoreTables` (sekiz tablo, trigram GIN dizini; `pg_trgm` eklentisi kurulum SQL'inde zaten var).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T104-urun-cekirdegi`
- **PR:** #361
- **CI:** doğrulayıcı izler

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `Simplify` bu task'ta üretildi; D-45'in notu bu dosyada bulunamadı | — | Plan başka task'a yazmıyor; ⚠ olarak PR'da |

## Notlar

- **Ön koşul:** örnek şema sıfırlaması `pg_trgm`'yi düşürüyordu; `SampleDatabaseFixture` düzeltildi (main'de katalog dizini olmadığı için görünmüyordu).
- **Repo memory (bitiş kapısı 8):** güncellendi
