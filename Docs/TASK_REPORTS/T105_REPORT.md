# T105 — Fiyat, kupon ve dijital dosya

**Faz:** F1 | **Durum:** ✓ PASS (doğrulama tur 1) | **Tarih:** 2026-11-04

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
| 5 | Kupon silinmez, gelen bağlar kısıtlayıcı | ✓ (tur 1) | `DeletingACouponThroughTheContext_IsRefused_ButItsFormCanBeChanged`, `EveryLinkToTheCoupons_IsRestrictive`; bağlar T109/T110'da → D-54 — *validator tur 1:* silme bağlı/bağsız her durumda `CatalogRecordsGuard`'la reddedilir; kupona gelen bağ T105'te yoktur (`pg_constraint` 0 satır) ve bağın kendisi `06 §7.2`'de sahibi tablonun (§4.14, §4.17) satırıdır — kriter T105'in kapsamında karşılanır, DB testi D-54'le T109/T110'da |
| 6 | Dijital dosya kısıtları ve dizinleri | ✓ | tamamlanmamış yüklemede bağ, iki bağ, `DetachedAt` / `AttachedAt`, `SizeBytes`, depo anahtarı, düzlemde ikinci dosya, ürün/varyant silmede bağın boşalması testleri; `Indexes_AreTheDocumentedOnes` |
| 7 | Göç tek konu, `Down` yok | ✓ | `MigrationSourceScan` (Unit) |

*Validator kanıtı (tur 1, `67e983a`):* kriter 1–7 ve KK1–KK7 ✓ — boş PostgreSQL 18 veritabanına `dotnet ef database update` ile üç göç uygulandı; `information_schema.columns`, `pg_constraint` (`confdeltype` dahil), `pg_indexes` okundu ve 42 ham SQL yoklaması koşuldu (ayrıntı: "Doğrulama tur 1").

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
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `67e983a`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-56) |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (mutlak yol, hook dosyaları mevcut — T104 turlarıyla aynı); ana dal son 3 run `41746249953`, `41746250013`, `41728889871` success; `MEMORY.md` satır 22'de T105.

**Kabul kriterleri (validator kanıtı):** kendi PostgreSQL 18.6 konteynerinde `pg_trgm`'li boş veritabanına `dotnet ef database update` ile üç göç uygulandı (rol ayarları `shopfolio.application_user`, `shopfolio.reader_user` önceden yazıldı); `information_schema.columns`, `pg_constraint`, `pg_indexes` okundu ve 42 ham SQL yoklaması koşuldu.
- **1 ✓** — dört tablo (`price_histories`, `discounts`, `coupons`, `digital_files`) tek göçte (`CreatePriceCouponAndDigitalFileTables`); 34 sütunun adı, türü (`numeric(12,2)`, `numeric(5,2)`, `date`, `bigint`, `timestamptz`, `text`) ve boş olabilirliği `06 §4.11`…§4.13, §4.70 ile (ortak `Id`, `CreatedAt` dahil — §4'ün girişi) birebir; varsayılan yalnız `coupons.used_count` 0, `reserved_count` 0, `version` 1 (`06` Varsayılan sütunu); `has-pending-model-changes` temiz.
- **2 ✓** fiyat 0 → `ck_price_histories_price_above_zero`, 0,01 kabul; aynı varyant ve anda ikinci satır kabul (yalnız eklenir, tekillik yok); varyant (ürünle) silinince fiyat geçmişi 0'a iner (`c`); kodda güncelleme yolu yok — bütün setter'lar özel, `CatalogRecordsGuard` `Modified` satırı SQL'den önce reddeder, `src`'de `PriceHistories` üzerinde `ExecuteUpdate`/`ExecuteDelete` yok. **3 ✓** ikinci indirim → `ix_discounts_product_id`; %100 ve %0 → `ck_discounts_percentage_range`; bitiş < başlangıç → `ck_discounts_end_not_before_start`; %99,99 tek gün kabul; ürün silinince indirim gider (`c`). **4 ✓** `Yaz`/`yaz` → `ix_coupons_normalized_code`; yüzde+tutar ve ikisi boş → `ck_coupons_exactly_one_value`; %100 → `ck_coupons_percentage_range`; bitiş < başlangıç → `ck_coupons_end_not_before_start`; asgari tutar boş ve tutara eşit → `ck_coupons_amount_needs_larger_minimum_basket`; tutar 0 → `ck_coupons_amount_above_zero`; eksi toplam adet, eksi kullanılmış ve eksi ayrılmış hak → `ck_coupons_usage_counts_not_negative`; eksi asgari tutar → `ck_coupons_minimum_basket_amount_not_negative`; 50/50,01 sabit ve %0,01/asgari 0 yüzdesel kabul. `SetCode` kodu ve `NormalizeIdentity` biçimini birlikte yazar (`09 §9.4` m.1). **5 ✓** yukarıdaki kriter satırı. **6 ✓** tamamlanmamış yüklemede ürün bağı, varyant bağı ve `AttachedAt` → `ck_digital_files_incomplete_upload_unattached`; iki bağ → `ck_digital_files_at_most_one_owner`; `AttachedAt`'siz `DetachedAt` → `ck_digital_files_detached_needs_attached`; boyut 0 → `ck_digital_files_size_above_zero`, 5.000.000.000 kabul (`bigint`); aynı depo anahtarı → `ix_digital_files_storage_key`; aynı ürüne ve varyanta ikinci kaldırılmamış dosya → `ix_digital_files_product_id1` / `variant_id1` (`WHERE detached_at IS NULL`), kaldırılmış olan kabul; aynı işlemde eskiyi kaldırıp yenisini bağlama kabul; ürün ve varyant silinince satırlar kalır, bağları boş (`n`). **7 ✓** tek göç, `Down` yok; göç dosyasında `suppressTransaction`, `CONCURRENTLY`, `VACUUM`, `ALTER SYSTEM`, `CREATE DATABASE`, `now()`, `CREATE EXTENSION`, `UPDATE`, `DELETE FROM` yok.
- **KK1–KK7 ✓** — `PriceCouponDigitalFileTablesTests` (47) her maddeyi ihlal adıyla sınıyor; `MigrationSourceScan` ve `DataModelSourceScan` yeşil; `Modules/Catalog`'da `Guid.NewGuid`, `HasColumnName`, `ToTable`, `HasDatabaseName`, `Restrict`/`Client…`, `DateTime.Now` yok.

**Yerel:** `dotnet build -warnaserror` 0 uyarı; Unit 230/230 (Consistency dahil); betik testleri 4 dosya geçti; Integration 234/234 (PostgreSQL 18, kendi konteyneri; 47 + 125 + 62 üç parça); `dotnet format --verify-no-changes` 0; `restore --locked-mode` 0; koşu sonrası `git status` temiz. **Dal CI** `41749429553` (head `67e983a`) dokuz iş success (E2E advisory success).

**`09 §25`:** ad uyumu (`Price`, `StartedAt`, `Percentage`, `NormalizedCode`, `MinimumBasketAmount`, `TotalUsageLimit`, `UsedCount`, `ReservedCount`, `StorageKey`, `SizeBytes`, `UploadCompletedAt`, `AttachedAt`, `DetachedAt`) `06` ile birebir; kapsam yalnız dört entity + yakalayıcı + iki test uyarlaması; silme davranışı her ilişkide açık (`Cascade` ×2, `SetNull` ×2), `RegisteredCascades` `06 §2.2.5` ile hizalandı; kimlik `EntityIds`'ten; `Coupon.Version` özel setter + `BumpVersion` + sözleşmeyle eşzamanlılık belirteci, sayaçların setter'ı özel (`09 §10.2` m.1, m.4); her yabancı anahtar sütunu filtresiz dizinin ilk sütunu; göç genişletmedir; yeni tablolar varsayılan yetki matrisinde (`PrivilegeMatrix` testi yeşil); enum, kilit dosyası, yük biçimi yok (`09 §12` uygulanamaz); yeni paket yok.

**Güvenlik:** sır yok; yetki/giriş yüzeyi yok; girdi denetimi veritabanı kısıtlarıyla (uygulama yazımı sonraki task'larda); yeni bağımlılık yok.

**`12` satırları:** task'ın kaynak birimleri `06` entity'leridir (`11 §7.1`); `12` matrisinde bu birimlere bağlı satır yok (`06 §4.70`'i anan tek satır `VAL-A694`, kaynağı K-1171 (1) / `07 §4.31.2` — parçalı yükleme ucunun task'ı) — `Durum` hücresi değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**Bulgular:** yok.

**K-notları:** K1 (→ D-56) `06 §7.3`'ün `Coupon` satırı 0 < `Percentage` < 100 kısıtını saymıyor; §4.13, §2.5.1 ve `11` T105 k4 sayıyor (kod ve göç kısıtı taşıyor).

**Yapım raporu karşılaştırması:** 1 uyuşmazlık — yapım raporu kriter 5'i "✓ (kısmi)" yazdı; doğrulayıcı ✓ verdi: kriterin T105'e düşen parçası (silmenin reddi) tam, bağların kısıtlayıcılığı bağ sahibi tabloların task'ındadır (D-54). Öteki kriterler ve test sayıları (Unit 230, Integration 234) doğrulayıcının bulgusuyla aynı.

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `CreatePriceCouponAndDigitalFileTables` (dört tablo).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T105-fiyat-kupon-dijital`
- **PR:** #362
- **CI:** dal CI `41749429553` (head `67e983a`) yeşil — doğrulayıcı tur 1
- **Post-merge run'lar:** squash `7b75bb5` (#362) — CI `41751877585` ve Publish `41751877590` success (F1 gate'inde doğrulandı, 2026-11-04)

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
