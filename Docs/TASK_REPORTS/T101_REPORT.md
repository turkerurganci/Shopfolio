# T101 — Veri modeli temeli

**Faz:** F1 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `src/Shopfolio.Web/Infrastructure/`: `EntityIds` (UUID v7), `TextNormalizer.NormalizeIdentity`, `EmptyKeyGuard` (boş anahtar kesicisi), `EmptyTextGuard` (boş metin → `NULL`), `DataModel/ModelRulesConvention` (silme davranışı yazılmamışsa `NoAction`, Guid anahtar `ValueGeneratedNever`, `Version` int → concurrency token, `HasRule` → `ck_<tablo>_<kural>`, tipsiz `decimal` model kurmaz), `ModelBuilderExtensions` (`HasRule`, `AsMoney`, `AsPercent`, `AsNetQuantity`); `AppDbContext` kesicileri ve kuralları bağlar.
- `Microsoft.EntityFrameworkCore.Relational` 10.0.14 sabitlendi (`Directory.Packages.props`, `09` Kütüphaneler satırı, üç kilit dosyası).
- Testler: Unit (örnek model, kaynak taraması + ihlal örnekleri, temel testler), Consistency (`06 §2.5.1` tür tablosu eşitliği), Integration (gerçek PostgreSQL: UUID v7, tür/ad, sürüm, boş metin, `ck_` adı, `pg_constraint`'te `r` yok).
- `Docs/DEFERRED_BACKLOG.md`: D-37 (a) "T103'e" notu, yeni D-45.
- `origin/main` (EF Core 10.0.14, #357) dala birleştirildi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/**`, `Directory.Packages.props`, `packages.lock.json` (3), `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`
- `tests/Shopfolio.UnitTests/{Samples,Architecture,Infrastructure,Consistency}`, `tests/Shopfolio.IntegrationTests/Model`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek bağlam, `Infrastructure/` altında | ✓ | kaynak taraması testi (KK1) |
| 2 | Anahtar `id`, UUID v7, uygulama üretir | ✓ | `SampleDatabaseTests` (`uuid_extract_version` = 7) (KK2) |
| 3 | Çoğul tablo, küçük harf sütun, `pk_/fk_/ix_/ck_` türer | ✓ | `SampleDatabaseTests` ad denetimi (KK3) |
| 4 | Türler `06 §2.5.1` tablosuna uyar | ✓ | `ColumnTypeTable` eşitlik testi (KK4) |
| 5 | `version` int, koşullu güncelleme | ✓ | eski sürümle güncelleme 0 satır (KK5) |
| 6 | Tek normalleştirme işlevi | ✓ | `I İ ı i` aynı biçim, boşluk atılır (KK6) |
| 7 | `created_at` ve boş değer yalnız `NULL` | ✓ | model testi + boş metin → `NULL` (KK7) |
| 8 | Silme davranışı açık, `RESTRICT` yok | ✓ | model testi + `pg_constraint` `r` yok (KK8) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit + Consistency | ✓ | `dotnet test Shopfolio.slnx -c Release --no-restore` | UnitTests geçti |
| Integration | ✓ | aynı; `SHOPFOLIO_TEST_DATABASE` yerel PostgreSQL 18 | IntegrationTests geçti |
| Toplam | ✓ | aynı | 209 / 209, 0 başarısız |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (göç geçmişi tablosunu kuran ilk göç T103; migration-dry-run adım 1 yerelde geçti, adım 2+ CI'da)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Var — `Microsoft.EntityFrameworkCore.Relational` 10.0.14 (MIT, EF Core ile aynı depo; sağlayıcı paketinin geçişli bağımlılığı, MSB3277 çakışmasını kapatmak için sabitlendi; `09` tablosunda satır)

## Commit & PR

- **Branch:** `task/T101-veri-modeli-temeli`
- **Commit:** `b7a332a`, `1098437`, birleştirme `e1287ee` — T101: Veri modeli temeli
- **PR:** #358
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `Microsoft.EntityFrameworkCore.Relational` 10.0.14 nuget'te var | `dotnet restore --force-evaluate` ve `--locked-mode` çıkış 0 | ✓ |
| 2 | PostgreSQL 18 (docker) ve `uuid_extract_version` (PG18) | Integration testleri yerel PG 18'de geçti | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | T101 sonrası açık kalem | D-45 | bkz. backlog |
| 2 | Yetki matrisi testinin göç geçmişi beklentisi | D-37 (a) | göç geçmişini T103 kurar |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 41702892272, 41702891253, 41696545899 — hepsi success
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** ⚠ kararlar PR #358 açıklamasında numaralı.
