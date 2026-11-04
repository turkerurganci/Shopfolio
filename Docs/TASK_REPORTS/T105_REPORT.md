# T105 — Fiyat, kupon ve dijital dosya

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Modules/Catalog/` altında dört entity ve yapılandırması: `PriceHistory` (`06 §4.11`), `Discount` (§4.12), `Coupon` (§4.13), `DigitalFile` (§4.70).
- `CatalogRecordsGuard` (kaydetme yakalayıcısı): değişmiş `PriceHistory` satırını ve silinen `Coupon`'u SQL gönderilmeden reddeder (`06 §2.2.3`, §4.11, §4.13). `PriceHistory`'nin bütün setter'ları özeldir; varyantla basamaklanma izlenen satırda `Deleted`'dır, `Modified` değil.
- `Coupon`: `SetCode` kodu ve normalleştirilmiş biçimini birlikte yazar; `UsedCount`, `ReservedCount`, `Version` özel setter'lı ve veritabanı varsayılanlı (0, 0, 1 — `06` "Varsayılan" sütunu).
- Tek göç `CreatePriceCouponAndDigitalFileTables` (`Down` yok; LF); `AppDbContext`'e dört `DbSet` ve yakalayıcı.
- Testler: `PriceCouponDigitalFileTablesTests` (47 test, KK1–KK6); `ModelRuleTests.RegisteredCascades`'e iki basamaklı bağ. T104'ün `pg_trgm` göç testi geçmiş satırlarını katalog göçünden itibaren siler (yeni göç geçmişte kalınca `Single` düşüyordu).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/{PriceHistory,Discount,Coupon,DigitalFile,CatalogRecordsGuard}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*`.
- `tests/Shopfolio.IntegrationTests/Model/{PriceCouponDigitalFileTablesTests,ModelRuleTests,CatalogCoreTablesTests}.cs`.
- `Docs/DEFERRED_BACKLOG.md` (D-54, D-55).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dört entity tek göçle, alan tabloları | ✓ | `Migration_MakesTheFourTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `ColumnsWithADefault_...` (yalnız kuponun üç varsayılanı) |
| 2 | Fiyat > 0, varyantla basamak, yalnız eklenir | ✓ | `PriceHistoryWithAPriceNotAboveZero_IsRefused`, `PriceHistory_HasNoPublicSetter`, `UpdatingAPriceHistoryRowThroughTheContext_IsRefused_AndTheRowStays`, basamak testleri |
| 3 | İndirim ürün başına tekil, yüzde ve tarih kısıtları, ürünle basamak | ✓ | ikinci indirim, %0/%100, bitiş < başlangıç ve ürün silme testleri |
| 4 | Kupon kısıtları | ✓ | normalleştirilmiş kod tekilliği, tam bir değer, yüzde aralığı, tutar > 0, asgari sepet (boş / eşit / eksi), tarih ve eksi sayaç testleri |
| 5 | Kupon silinmez, gelen bağlar kısıtlayıcı | ✓ (kısmi — ⚠ 2) | `DeletingACouponThroughTheContext_IsRefused_ButItsFormCanBeChanged`, `EveryLinkToTheCoupons_IsRestrictive`; bağlar T109/T110'da → D-54 |
| 6 | Dijital dosya kısıtları ve dizinleri | ✓ | tamamlanmamış yüklemede bağ, iki bağ, `DetachedAt` / `AttachedAt`, `SizeBytes`, depo anahtarı, düzlemde ikinci dosya, ürün/varyant silmede bağın boşalması testleri; `Indexes_AreTheDocumentedOnes` |
| 7 | Göç tek konu, `Down` yok | ✓ | `MigrationSourceScan` (Unit) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 230/230 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests` (gerçek PostgreSQL 18; `--filter-class` ile üç parça) | 234/234 (61 + 111 + 62) |
| Göç kuru koşusu | ✓ (adım 1) | `scripts/ci/migration-dry-run.sh` | adım 1 "No changes have been made to the model since the last migration"; adım 2 yerelde `psql` olmadığı için koşmadı — göç paketi integration testlerinde (`MigrationBundleTests`) boş veritabanına uygulandı |
| Biçim / kilit | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes`; `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 / 0 |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `CreatePriceCouponAndDigitalFileTables` (dört tablo).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T105-fiyat-kupon-dijital`
- **PR:** (açılınca yazılır)
- **CI:** doğrulayıcı izler

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Bağlı kuponun veritabanında `fk_*` ile silinemediği | D-54 | Kupona gelen bağlar T109 ve T110'un tablolarıyla gelir |
| 2 | Aynı sütunda iki dizinin çatının tekilleştirdiği adı (`…_id1`) | D-55 | Doküman bu durumu anmıyor; ad kurala göre türedi |

## Notlar

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `scripts/git-hooks`; ana dal run'ları `41728889871`, `41728889974`, `41715894986` success (T104'ün iki run'ı o an sürüyordu, sayılmadı). `grep T105 DEFERRED_BACKLOG` boştu.
- **Yapım iki işçide yapıldı:** ilk işçi bağlam sınırında bölündü (kod, göç, testler yazıldı, koşulmadı); ikinci işçi testleri koşup iki düzeltme yaptı (`confdeltype::text`, T104'ün `pg_trgm` testi).
- **Repo memory (bitiş kapısı 8):** güncellendi
