# T104 — Ürün çekirdeği ve görsel

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

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
| Doğrulama durumu | — |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `CreateCatalogCoreTables` (sekiz tablo, trigram GIN dizini; `pg_trgm` eklentisi kurulum SQL'inde zaten var).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T104-urun-cekirdegi`
- **PR:** aşağıda PR açıklamasında
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
