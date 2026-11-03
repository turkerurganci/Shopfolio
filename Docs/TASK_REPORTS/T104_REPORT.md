# T104 — Ürün çekirdeği ve görsel

**Faz:** F1 | **Durum:** ✗ FAIL (doğrulama tur 2) | **Tarih:** 2026-11-04

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
| 1 | Sekiz entity tek göçle | ✗ (tur 2) | `Migration_MakesTheEightTables`; göç betiği yerelde PostgreSQL 18'e uygulandı — *validator tur 2:* `06` alan tablolarının "Varsayılan" sütunu (veritabanının varsayılanı) altı sütunda karşılanmıyor (bulgu 1) |
| 2 | VatRate, süre çitleri, trigram dizin | ✓ | `CatalogCoreTablesTests` KDV/süre/dizin testleri |
| 3 | Varyant tekillikleri ve kısıtları | ✓ | stok kodu, `NULLS NOT DISTINCT`, fiyat/stok/ayrılmış adet testleri |
| 4 | Option sırası | ✓ | üçüncü seçenek ve sıra çiti testleri |
| 5 | Category düzeyi | ✓ | düzey/üst kategori testleri |
| 6 | Ana kategori/görsel, Slug | ✓ | filtreli tekil dizin ve Slug testleri |
| 7 | Image sınırları, basamak/kısıtlayıcı silme | ✓ | görsel ölçü ve silme testleri |
| 8 | Göç tek konu, `Down` yok | ✓ | `MigrationSourceScan` (Unit) |

*Validator kanıtı (tur 1, `b4c73c8`):* sekiz kriterin her biri PostgreSQL 18'e uygulanan göç üzerinde ham SQL, `pg_constraint`/`pg_indexes` ve sorgu planıyla yeniden sınandı — ✓ (ayrıntı: "Doğrulama tur 1"); verdict `09 §25` bulguları yüzünden FAIL.

*Validator kanıtı (tur 2, `1b9132d`):* kriter 2–8 ve KK1–KK8 ✓; kriter 1 ✗ — sütunlar, türler, zorunluluk, dizinler ve silme eylemleri `06` ile birebir, ama Varsayılan'ı dolu altı sütunun veritabanı varsayılanı yok (ayrıntı: "Doğrulama tur 2").

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
| Doğrulama durumu | ✗ FAIL — tur 2 (2026-11-04, head `1b9132d`); tur 1 ✗ FAIL (`b4c73c8`) |
| Bulgu sayısı | Tur 2: 1 (S1 DÜŞÜK); tur 1: 3 (S1 DÜŞÜK ×3, düzeltme 1'de kapandı) |
| Düzeltme gerekli mi | Evet — ikinci iterasyon FAIL: `12 §8.5` sınırı, eskalasyon (`INSTRUCTIONS.md` §3.3) |
| Bloke-etmeyen not (K) sayısı | Tur 2: 1 (K1 → D-52); tur 1: 2 (K1 → D-51, K2 → D-45) |

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

## Düzeltme 1 (tur 1 FAIL sonrası, 2026-11-04)

| # | Bulgu | Değişiklik | Test |
|---|---|---|---|
| 1 | `pg_trgm` denetimi ve `09 §10.6` m.11 testleri yok | `CreateCatalogCoreTables.Up` ilk deyimi: `DO $$ … RAISE EXCEPTION 'pg_trgm extension is not installed; the database creator user installs it (DEPLOY_RUNBOOK)'` (m.8 kalıbı; göç henüz main'de değildi, yerinde düzeltildi, `Down` yok) | `Migration_WhenPgTrgmIsNotInstalled_StopsWithAClearMessageAndKeepsTheEarlierMigrations` (koleksiyon veritabanının kopyasında eklenti ve sekiz tablo silinir, göç satırı geri alınır, paket uygulanır → açık ileti; geçmişte yalnız `CreateSettingsTables`; `products` yok); `ProductNameSearch_UsesTheTrigramIndex` (`enable_seqscan = off`, `LIKE '%celik%'` planı `ix_products_name_search_form`) |
| 2 | `Variant.Sku`/`NormalizedSku` ↔ `06 §4.6` `SKU`/`NormalizedSKU` | Özellik adları `SKU`, `NormalizedSKU`; `SetSku` yöntemi ve parametre adı aynı kaldı (dokümanda yöntem adı yok). Sütun (`sku`, `normalized_sku`) ve dizin adları değişmedi; Designer ve anlık görüntü güncellendi, `has-pending-model-changes` temiz | Mevcut `Variant` testleri (`ix_variants_normalized_sku` ihlali, `sku` sütunu) yeşil |
| 3 | `Simplify` Latin dışı işaretleri atıyor | FormD sonrası işaret yalnız Latin taban harfin (Temel Latin, Latin-1, Latin Genişletilmiş-A/B, Ek Latin) ardından atılır; ötekiler kalır | `Simplify_LeavesTheNonLatinLettersAlone` (`й`, `ά`, `कु`, karma satırlar) |

Test sonuçları (düzeltme sonrası): Unit 230/230, Integration 185/185 (gerçek PostgreSQL 18), Release `-warnaserror` 0 uyarı, `restore --locked-mode` 0.

### Doğrulama tur 2 — ✗ FAIL (2026-11-04)

Yeniden doğrulama, kapsam tam (bütün kriterler, KK'lar, `09 §25`, güvenlik).

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (mutlak yol, hook dosyaları mevcut — tur 1 ile aynı); ana dal son 3 run `41728889871`, `41728889974`, `41715894986` success; `MEMORY.md` satır 22'de T104.

**Kabul kriterleri (validator kanıtı):** bütün göçlerin betiği (`dotnet ef migrations script`) `pg_trgm`'li boş PostgreSQL 18 veritabanına uygulandı; `pg_constraint`, `pg_indexes`, `information_schema.columns` okundu.
- **1 ✗** — sekiz tablo, sütun adları/türleri (`varchar(150)`, `numeric(5,2)`, `numeric(12,2)`, `numeric(12,3)`), `NOT NULL`'lar, dizinler ve silme eylemleri `06 §4.5`…§4.10, §4.69, §4.71 ile birebir; `has-pending-model-changes` temiz. Ancak `06 §4`'ün sütun okunuşu "**Varsayılan** veritabanının varsayılanıdır" der ve Varsayılan'ı dolu altı sütunun `column_default`'u boştur (bulgu 1).
- **2 ✓** `ck_products_vat_rate_range` (`>= 0 AND < 100`), `shipping_period` 0–10 (P-5 üst çiti), `fulfilment_period >= 1` (P-44), `max_quantity_per_order >= 1` (P-9); `ix_products_name_search_form` `USING gin (name_search_form gin_trgm_ops)`. **3 ✓** `ix_variants_normalized_sku` tekil; kombinasyon dizini `NULLS NOT DISTINCT`; fiyat, net miktar, ayrılmış adet, stok kısıtları; `second_value_needs_first`. **4 ✓** `ix_options_product_id_sort_order` tekil, `sort_order` 1–2. **5 ✓** `level` 1–3, `(parent_category_id IS NULL) = (level = 1)`. **6 ✓** filtreli tekil `ix_product_categories_product_id` (`WHERE is_primary_category`), `ix_product_images_product_id` (`WHERE is_main`); filtresiz `ix_products_slug`, `ix_categories_slug`; Slug deseni. **7 ✓** `ck_images_dimensions_within_limits`; `confdeltype` — products'a bağlı dört bağ ve `option_values → options` `c`, öteki yedi bağ `a`. **8 ✓** tek göç, `Down` ve yasak deyim yok; ilk deyim `pg_trgm` denetimi.
- **KK1–KK8 ✓** — `CatalogCoreTablesTests` her maddeyi ihlal adıyla sınıyor (SQLSTATE + kısıt adı); tur 1'in üç bulgusu kapandı (`pg_trgm` ilk deyimi ve iki test; `SKU`/`NormalizedSKU`; `Simplify` yalnız Latin taban harfin işaretini atar). D-50 (a)(b)(c) kapanışı doğru.

**Yerel:** `dotnet build -warnaserror` 0 uyarı; Unit 230/230 (Consistency dahil); Integration 185/185 (PostgreSQL 18, kendi konteyneri); `dotnet format --verify-no-changes` 0; `restore --locked-mode` 0. **Dal CI** `41739943430` (head `1b9132d`) dokuz iş success (E2E advisory success); önceki kırmızı `41739162198` Lint WHITESPACE → `2dda9cd`, `BYPASS_LOG`'da.

**Güvenlik:** sır yok; yetki/giriş yüzeyi yok; yeni bağımlılık yok; girdi denetimi veritabanı kısıtlarıyla (uygulama yazımı sonraki task'larda).

**`12` satırları:** task'ın kaynak birimleri `06` entity'leridir; `12` matrisinde bu birimlere bağlı satır yok — `Durum` hücresi değişmedi.

| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Etkilenen komşu | Dosya |
|---|---|---|---|---|---|---|
| 1 | Kriter 1; `06 §4` girişi ("Varsayılan veritabanının varsayılanıdır"); §4.5, §4.6, §4.10, §4.71; `09 §25` "doküman cümlesiyle çelişmiyor mu" | DÜŞÜK | S1 | `products.publish_status` DEFAULT `'Draft'`, `products.version` DEFAULT `1`, `variants.publish_status` DEFAULT `'Draft'`, `variants.reserved_quantity` DEFAULT `0`, `product_categories.is_primary_category` DEFAULT `false`, `product_images.is_main` DEFAULT `false` ↔ altısında da `column_default` boş; varsayılan yalnız C# başlatıcısında. Yoklama: bu sütunları yazmayan ham `INSERT INTO products` → `23502 null value in column "publish_status"`. Düzeltme: modelde `HasDefaultValue` (göç aynı dosyada, main'de değil), göç/anlık görüntü yeniden üretilir, varsayılanları sınayan test. | T103 tabloları (D-52); T119 başlangıç verisi göçü | `Modules/Catalog/{Product,Variant,ProductCategory,ProductImage}.cs`, `Migrations/20261103222027_CreateCatalogCoreTables*.cs`, `AppDbContextModelSnapshot.cs` |

**K-notları:** K1 (→ D-52) aynı boşluk T103 tablolarında: `companies.version` ve `company_settings`'in yedi bölüm sürümü veritabanında varsayılansız; T103 göçü main'de olduğu için ayrı genişletme göçü.

**Yapım raporu karşılaştırması:** kriter 2–8, KK'lar, test sayıları (Unit 230, Integration 185) ve düzeltme 1 tablosu uyumlu; uyuşmazlık 1 — rapor kriter 1'i ✓ der, Varsayılan sütunu gözden kaçmış (tur 1 doğrulaması da görmedi).

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

## Düzeltme 2 (tur 2 FAIL sonrası, 2026-11-04)

Bulgu 1 (S1 DÜŞÜK, kriter 1): `06 §4` "Varsayılan veritabanının varsayılanıdır" der; T104'ün sekiz entity'sinin (`06 §4.5`–§4.10, §4.69, §4.71) alan tablolarında Varsayılan sütunu dolu her alan tarandı — altı alan çıktı (başka yok), altısına da modelde `HasDefaultValue` verildi (enum alanlarda değer dönüşümüyle saklanan metin değeri).

| Alan | Varsayılan | Model |
|---|---|---|
| `products.publish_status` | `'Draft'` | `HasDefaultValue(PublishStatus.Draft)` |
| `products.version` | `1` | `HasDefaultValue(1)` |
| `variants.publish_status` | `'Draft'` | `HasDefaultValue(PublishStatus.Draft)` |
| `variants.reserved_quantity` | `0` | `HasDefaultValue(0)` |
| `product_categories.is_primary_category` | `false` | `HasDefaultValue(false)` |
| `product_images.is_main` | `false` | `HasDefaultValue(false)` |

`CreateCatalogCoreTables` göçü yerinde güncellendi (main'de değil; Designer ve anlık görüntü yeniden üretildi; ilk deyim `pg_trgm` denetimi, `Down` yok); `has-pending-model-changes` temiz. T103 tablolarındaki aynı boşluk D-52'de — dokunulmadı.

Testler (`CatalogCoreTablesTests`): `ColumnsWithADefault_AreExactlyTheSixOfTheFieldTables_WithTheirDocumentedValues` (`information_schema.columns.column_default`, sekiz tabloda tam altı) ve `RawInsertsWithoutTheDefaultedColumns_GetTheDocumentedDefaults` (ham `INSERT` bu sütunlar yazılmadan satır ekler, varsayılanlar gelir).

Test sonuçları (düzeltme 2): Unit 230/230, Integration 187/187 (gerçek PostgreSQL 18), `-warnaserror` 0 uyarı, `dotnet format --verify-no-changes` 0, `restore --locked-mode` 0.

## Notlar

- **Ön koşul:** örnek şema sıfırlaması `pg_trgm`'yi düşürüyordu; `SampleDatabaseFixture` düzeltildi (main'de katalog dizini olmadığı için görünmüyordu).
- **Repo memory (bitiş kapısı 8):** güncellendi
