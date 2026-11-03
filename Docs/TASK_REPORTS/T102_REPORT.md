# T102 — Enum tanımları ve değer dönüşümü

**Faz:** F1 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- 42 enum (`06 §5.2`–§5.6 + `AuditOperation` 43 kod): sahip modülün `Modules/<Modül>/Contracts/` ad alanında; modül dışı kalıcı altyapı enumları `Infrastructure/Values/`; `LinkState` (sütun değil) `Infrastructure/`. Çıplak kod adı; sayı/yorum kuralı yok.
- `Infrastructure/DataModel/EnumText.cs` (`EnumText<T>`, `EnumTextConverter<T>`, katı okuma), `ModelBuilderExtensions.HasAllowedValues`, `ModelRulesConvention.ApplyEnumRules` (metin dönüşümü zorunlu, `ck_<tablo>_<sütun>` izinli değer kısıtı; `AuditOperation` adla istisna), `AppDbContext.ConfigureConventions` (tüm enumlara dönüşüm; `AuditOperation` hariç).
- `Modules/Administration/Contracts/AuditOperationText.cs`: `ToCode`/`TryParse`/`Display` (tanınmayan kodu aynen gösterir), `AreaOf` (43 kod → alan).
- Testler: `Locks` (41 kilit dosyası + `EnumLockTests`), `Consistency/EnumTable`, `EnumSourceScan` (+ ihlal örnekleri), `DataModelSourceScan`'a `HasConversion<int>` kuralı (D-46 (c)), `EnumConversionTests`, örnekler, `IntegrationTests/Model/EnumDatabaseTests`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/*/Contracts/**`, `src/Shopfolio.Web/Infrastructure/{Values,DataModel}/**`, `AppDbContext`
- `tests/Shopfolio.UnitTests/{Locks,Consistency,Architecture,Infrastructure,Samples}`, `tests/Shopfolio.IntegrationTests/Model`
- `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

Kriterler `Docs/11_IMPLEMENTATION_PLAN.md` `#### T102` bloğundadır; yapım yerel kanıtı aşağıdadır, bağımsız doğrulama sonucu validator tarafından yazılır.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Enumlar `06 §5` listesine uyar, kilit testi var | ✓ | `EnumLockTests`, `Consistency/EnumTable` |
| 2 | Kalıcı enum metin olarak yazılır/okunur, tanınmayan kod hata verir | ✓ | `EnumConversionTests`, `EnumDatabaseTests` |
| 3 | İzinli değer kısıtı (`ck_`) veritabanında | ✓ | `EnumDatabaseTests` (`pg_constraint`) |
| 4 | `HasConversion<int>` / dönüşümsüz enum kaynak taramasında kırmızı | ✓ | `DataModelSourceScan`, `EnumSourceScan` + ihlal örnekleri |
| 5 | `AuditOperation` kodları ve alan eşlemesi | ✓ | `AuditOperationText` testleri |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit + Consistency | ✓ | `dotnet test Shopfolio.slnx -c Release --no-restore` | 199 / 199 |
| Integration | ✓ | aynı; `SHOPFOLIO_TEST_DATABASE` yerel PostgreSQL 18 | 94 / 94 |
| Biçim / restore | ✓ | `dotnet format --verify-no-changes`; `dotnet restore --locked-mode` | temiz, çıkış 0 |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (enum sütunları ilk varlıkla T103+; kısıt mekanizması hazır)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T102-enum-tanimlari`
- **Commit:** `84f729c` — T102: Enum tanımları, metin dönüşümü, izinli değer kısıtı, kilit dosyaları ve testler
- **PR:** başlık "T102: Enum tanımları ve değer dönüşümü" (no `IMPLEMENTATION_STATUS.md`'de)
- **CI:** *(yardımcı izler)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | EF Core API'leri: `ConfigureConventions` + `HaveConversion`, finalizing kuralında `GetColumnName(StoreObjectIdentifier.Table)`, `AddCheckConstraint` sütun kısıtı, `ValueConverterType` ek açıklaması | Integration testleri yerel PG 18'de geçti | ✓ (D-45 notu) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `AuditOperation` etiket testi; `ErrorCode`/`NoticeCode` kilit testine eklenmesi | D-47 | mesaj kataloğu ve kodlar sonraki task'larda |
| 2 | D-46 (a), (b) | D-46 | T103 (ilk varlık); (c) bu task'ta ✅ |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 41710384453, 41710384654, 41706376352 — hepsi success
- **Hook:** `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (kurulu)
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** ⚠ kararlar PR açıklamasında numaralı.
