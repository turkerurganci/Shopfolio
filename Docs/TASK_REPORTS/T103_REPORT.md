# T103 — Başlangıç listeleri, firma ve yasal metin sürümü

**Faz:** F1 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/Settings/`: `Province`, `Country`, `Company`, `CompanySettings`, `CompanyDeliveryProvince`, `LegalTextVersion` (varlık + yapılandırma); `AppDbContext`'e altı `DbSet`.
- Tek göç `CreateSettingsTables` (`Down` yok): altı tablo, `ck_` kısıtları, tekil dizinler, `ix_companies_single_row` ve `ix_company_settings_single_row` (sabit ifadenin tekil dizini; elle yazılmış SQL), `__EFMigrationsHistory` üzerinde uygulama ve salt okuma kullanıcısının bütün yetkisinin rol ayarından okunan adlarla alınması.
- `ProtectedRecordsGuard` (kaydetme kesicisi): il, ülke, firma, ayar ve yasal metin satırı silinmez; yayımlanmış yasal metin sürümü güncellenmez. Teslimat ili satırı serbest.
- `Infrastructure/ParameterFences` (P-5, P-7, P-8 çitleri); `.editorconfig` göç klasörünü üretilmiş kod sayar.
- Testler: `SettingsTablesTests` (KK1–KK7, 23 test), `MigrationSourceScan` + ihlal örnekleri (`ViolationSamples/Migrations`), `ModelRules.Check` DbSet-adı denetimi (D-46 b), `PrivilegeMatrix` göç geçmişi beklentisi (D-37 a), `EnumLockTests`/`EnumSourceScan` (D-48 b), `DataModelSourceScan` göç klasörü muafiyeti, `SampleDatabaseFixture` şema sıfırlama.
- Göç dosyalarındaki CRLF temizlendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Settings/*`, `src/Shopfolio.Web/Infrastructure/{AppDbContext,ParameterFences,EmptyTextGuard}.cs`, `src/Shopfolio.Web/Infrastructure/Migrations/*`, `.editorconfig`
- `tests/Shopfolio.IntegrationTests/{Model,Database}/*`, `tests/Shopfolio.UnitTests/{Architecture,Locks}/*`
- `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Altı entity tek göçle kurulur | ✓ | `Migration_MakesTheSixTables` |
| 2 | `Company`, `CompanySettings` tek satır (sabit ifade dizini) | ✓ | `SecondCompanyRow_…`, `SecondCompanySettingsRow_…` |
| 3 | Kontrol kısıtları, bölüm sürümleri | ✓ | `OpenBankTransferWithoutAnIban_…`, `HalfFilledReturnAddress_…`, `NegativeAmount_…`, `PhoneWithNineDigits_…`, `CompanySettings_SectionVersionsAreNotConcurrencyTokens` |
| 4 | İl/ülke kodu tekil ve biçimli; teslimat ili tekil | ✓ | `ProvinceCodeWithThreeDigits_…`, `CountryCodeInLowerCase_…`, `SameProvinceTwiceInTheDeliveryList_…` |
| 5 | `LegalTextVersion` tekillik, birlikte doluluk, taslak, tür koşulları | ✓ | `LegalTextVersion_*` (7 test) |
| 6 | Silme ve güncelleme kuralları | ✓ | `ProvinceInUse_…`, `PermanentRows_…`, `PublishedLegalTextVersion_…`, `DeliveryProvinceRow_CanBeDeleted` |
| 7 | Göç tek konu, `Down` yok, geçmiş tablosu kapalı | ✓ | `MigrationSourceScanTests`, `MigrationHistoryTable_IsClosedTo…` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency dahil) | ✓ 217/217 | `dotnet test tests/Shopfolio.UnitTests --no-build` | geçti |
| Integration | ✓ 119/119 (PostgreSQL 18) | `dotnet test tests/Shopfolio.IntegrationTests --no-build` (iki parça) | geçti |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | (doğrulayıcı doldurur) |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `CreateSettingsTables`
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T103-baslangic-listeleri`
- **PR:** dönüşte yazılı
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Test sunucusu PostgreSQL 18 | `shopfolio-t103-pg` (`postgres:18`) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Tohum veri (81 il, ülke listesi, tek satırlar, 2 taslak) | T119 | Task yalnız yapı kurar |
| 2 | Toplu `ExecuteDelete/Update` bekçi kapsamı dışı | — | Uygulama kodu bunları bu tablolarda kullanmaz; ilgili task'lar kendi testini yazar |
| 3 | `audit_logs.operation` eşlemesi | D-48 (a) | T106 |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yapım işçisi izlemez
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** FK indeksleri (`ix_companies_province_id`, `ix_company_settings_return_province_id`) EF konvansiyonuyla oluşuyor, zararsız.
