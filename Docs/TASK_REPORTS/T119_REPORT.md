# T119 — Başlangıç verisi göçü ve seed olmayanların sınırı

**Faz:** F1 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **D-52 genişletme göçü (önce, ayrı):** `20261104134540_SetSettingsColumnDefaults` — `companies.version`, `company_settings`'in yedi bölüm sürümü ve `legal_text_versions.version` için veritabanı varsayılanı `1`; `is_bank_transfer_enabled` ve `is_sales_temporarily_closed` için `false` (`06 §4` girişi "Varsayılan veritabanının varsayılanıdır"; §4.1, §4.2, §4.56). Model `HasDefaultValue` ile hizalı; `Down` yok; satır değiştirmez.
- **Başlangıç verisi göçü:** `20261104134754_InsertStartupData` — yalnız `migrationBuilder.Sql` ile, `public.` nitelikli `INSERT`'ler:
  - 81 il (kod = plaka, 01 Adana … 81 Düzce) ve ülke listesi (ISO 3166-1 alfa-2'nin resmî 249 kodu, Türkçe adlarla);
  - altı tek satır (`companies`, `company_settings`, `brandings`, `channel_state_records`, `heartbeat_records`, `backup_status_records`);
  - `company_settings`: `02 §11.1` varsayılanları (P-1 0,00 · P-3 boş · P-4 boş · P-5 2 · P-7 2 · P-8 5 · P-10 20,00), `homepage_layout = ContentFirst`, ürünle gelen menü adları (`04` 9.14.4); anahtarlar ve bölüm sürümleri D-52 varsayılanından (kapalı, 1);
  - `company_delivery_provinces`: 81 il — ayar satırıyla aynı deyimde (veri değiştiren CTE) ve yalnız ayar satırı yazıldığında;
  - iki taslak (`PrivacyNotice`, `CookiePolicy`): `published_at`, `version_number`, `hosting_location`, `template_version` boş; metin yer tutuculu.
- **Sabit deyimler:** kimlikler göçün kendi anından ve sayaçtan kurulan v7 UUID'leri, `created_at` aynı sabit an (`now()` yok — `09 §7.1`, §10.6 m.11); `UPDATE`/`DELETE` yok.
- **İkinci uygulama değiştirmez (`06 §8.1.2`):** liste satırı kodla (`ON CONFLICT (code) DO NOTHING`), tek satır tek-satır diziniyle (`ON CONFLICT DO NOTHING`), taslak türüyle (`WHERE NOT EXISTS`) eşlenir; teslimat illeri yalnız ayar satırıyla doğar — firmanın çıkardığı il geri gelmez.
- **Seed olmayanlar listesi** göç sınıfının XML özetinde açıkça yazılı (`06 §8.3`, §8.4): kod (ürün sabitleri, enum'lar, iş günü takvimi, yaygın şifreler, kargo şirketleri, takip kalıpları, yerli üretim logosu, iki metnin şablonu) · kurulum (ilk yönetici, kurulum ayarları) · ilk açılış (üretilen iki metnin ilk sürümü, ize girmez — T366) · çatı (`DataProtectionKey`).
- **Testler:** entegrasyon `StartupDataTests` + `StartupDataReapplyTests` (26), birim mimari `StartupDataSourceScanTests` (+ `ViolationSamples/StartupData`), eşitlik `SettingDefaultsConsistencyTests` (`[Trait("Consistency","true")]`).
- **Backlog:** D-52 ✅ Çözüldü → T119; D-39 (a) satırına T119 notu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Migrations/20261104134540_SetSettingsColumnDefaults.cs` (+ `.Designer.cs`)
- `src/Shopfolio.Web/Infrastructure/Migrations/20261104134754_InsertStartupData.cs` (+ `.Designer.cs`), `AppDbContextModelSnapshot.cs`
- `src/Shopfolio.Web/Modules/Settings/Company.cs`, `CompanySettings.cs`, `LegalTextVersion.cs` (yalnız `HasDefaultValue`)
- `tests/Shopfolio.IntegrationTests/Model/StartupDataTests.cs`
- `tests/Shopfolio.UnitTests/Architecture/StartupDataSourceScan.cs`, `StartupDataSourceScanTests.cs`, `ViolationSamples/StartupData/{CodeViolations,MigrationViolations,Clean}.cs`
- `tests/Shopfolio.UnitTests/Consistency/SettingDefaultsConsistencyTests.cs`
- `Docs/DEFERRED_BACKLOG.md` (D-52, D-39), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Seed göçün veri adımında; kodda "yoksa oluştur" dalı yok | ✓ | `InsertStartupData.Up` yalnız `Sql` · `StartupDataSourceScanTests.ApplicationCode_HasNoPathThatCreatesAStartupRow` (fabrika çağrısı, kurucu, `Add`, elle `INSERT` — Migrations dışı `src`) |
| 2 | 81 il ve ülke listesi; il kodu plaka | ✓ | `EmptyInstallation_HasThe81ProvincesUnderTheirPlateCodes` (01…81 ardışık, 06/33/34/35/46/63/81 adları) · `EmptyInstallation_HasTheCountryListWithTurkishNames` (249, TR Türkiye, CY Kıbrıs) |
| 3 | Altı tek satır başlangıç değerleriyle | ✓ | `EmptyInstallation_HasEachSingleRowOnce`, `Company_StartsEmptyAtVersion1`, `Branding_ChannelState_Heartbeat_BackupStatus_StartEmpty`, `SeedRows_CarryTheMigrationsMomentAndVersion7Ids` |
| 4 | `CompanySettings` varsayılanları `02 §11.1`; havale, geçici kapatma kapalı; bölüm sürümleri 1; `ContentFirst` | ✓ | `CompanySettings_TakesTheDefaultsOf02_11_1`, `CompanySettings_SwitchesOff_SectionsAtVersion1_LayoutContentFirst_MenuNamesOfTheProduct` · `SettingDefaultsConsistencyTests` |
| 5 | 81 teslimat ili, varsayılan tüm Türkiye | ✓ | `EmptyInstallation_DeliversToAll81Provinces` |
| 6 | İki taslak, yayın anı ve sürüm numarası boş | ✓ | `LegalTexts_AreTheTwoUnpublishedDraftsOfTheFirmEditedTexts`, `LegalTextDrafts_UseOnlyTheClosedTextFormatSet` |
| 7 | Liste değişikliği yeni göçle; ayar varsayılanı kurulmuş satırı değiştirmez; `UPDATE`/`DELETE` izinli tablolar dışında yok | ✓ | `StartupDataReapplyTests.SecondApplication_ChangesNoRowCountAndNoValueTheCompanyChanged` · `MigrationSourceScanTests.Migrations_OfTheApplication_BreakNoRule` · göç `Down`'suz (`Migration_HasNoDownMethod`) · sınıf özeti "this file is never edited" |
| 8 | Seed olmayanlar kodda/kurulumda; üretilen iki metin ve `DataProtectionKey` göçte yok | ✓ | `InsertStartupData` XML özeti (liste) · `StartupDataSourceScanTests.Migrations_WriteNoKeyRingRowAndNoGeneratedTextVersion` · `EmptyInstallation_HasNoKeyRingRowAndNoGeneratedTextVersion` |

### Öz denetim — doğrulama kontrol listesi

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 — boş veritabanında 81 il (kodu plaka) ve 81 teslimat ili | ✓ | `StartupDataTests.EmptyInstallation_HasThe81ProvincesUnderTheirPlateCodes`, `EmptyInstallation_DeliversToAll81Provinces` (koleksiyon veritabanı gerçek göç demetiyle kurulan şablonun kopyası) |
| KK2 — altı tek satır başlangıç değerlerinde | ✓ | `EmptyInstallation_HasEachSingleRowOnce`, `Company_StartsEmptyAtVersion1`, `CompanySettings_*` (2), `Branding_ChannelState_Heartbeat_BackupStatus_StartEmpty` |
| KK3 — iki taslak yayımlanmamış | ✓ | `LegalTexts_AreTheTwoUnpublishedDraftsOfTheFirmEditedTexts` |
| KK4 — ikinci uygulama satır sayısını ve firmanın değiştirdiği değeri değiştirmez | ✓ | `StartupDataReapplyTests` — ayar, kimlik, marka, ülke adı, taslak metni değiştirilir, bir teslimat ili silinir; göçün geçmiş satırı silinip göç kullanıcısıyla yeniden uygulanır; 11 tablonun sayısı ve iki satırın tam JSON'u aynı |
| KK5 — `UPDATE`/`DELETE` izinli tablo dışında yok | ✓ | `MigrationSourceScan` (T103/D-49; izinli: `companies`, `company_settings`, `provinces`, `countries`) yeni iki göçü de tarar; iki göçte `UPDATE`/`DELETE` hiç yok |
| KK6 — kodda varsayılan kaydı oluşturan yol yok | ✓ | `StartupDataSourceScan.ScanCode` (+ ihlal örneği 13 satır, temiz örnek); `src`'de seed tiplerinin fabrika çağrısı yalnız testlerde |
| KK7 — ayar varsayılanı tabloyla eşit | ✓ | `SettingDefaultsConsistencyTests.StartupData_TakesEveryFirmSettingDefaultOf02_11_1` (02 §11.1'in "Firma ayarı" satırlarının kalın varsayılanı = `InsertStartupData.SettingDefaults`) + entegrasyonda veritabanı = tablo |
| KK8 — seed olmayanlar listede; göçte üretilen metin ve `DataProtectionKey` yok | ✓ | göç sınıfı özeti · `StartupDataSourceScan.ScanMigration` (+ ihlal örneği) · `EmptyInstallation_HasNoKeyRingRowAndNoGeneratedTextVersion` |
| `09 §25` ilgili satırlar | ✓ | PR açıklamasında "Veri ve göç" soruları cevaplı |
| Güvenlik taraması | ✓ | sır yok; yetki değişikliği yok (yalnız `INSERT`, göç kullanıcısı); yeni paket yok; dış girdi yok — taslak metin sabit, kapalı biçim setinde (`LegalTextDrafts_UseOnlyTheClosedTextFormatSet`) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --filter-not-trait "Consistency=true"` | 223/223 |
| Consistency | ✓ | `... --filter-trait "Consistency=true"` | 44/44 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/...` (PostgreSQL 18, yerel konteyner) | 952/952 (T118'de 926; +26) |
| Contract | — | NOT_APPLICABLE | — |
| Biçim / derleme | ✓ | `dotnet format --verify-no-changes`; `dotnet build -c Release -warnaserror`; `dotnet ef migrations has-pending-model-changes` | temiz; "No changes have been made to the model since the last migration." |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulama chat'i doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `SetSettingsColumnDefaults` (genişletme: `SET DEFAULT`, 11 sütun) ve `InsertStartupData` (yalnız veri, `INSERT`). Sürüm yoksa (`git tag` boş) göç kuru çalıştırması yalnız boş veritabanına uygular; ikisi de önceki imajın koduyla uyumlu (varsayılan eklemek ve satır eklemek eski kodu bozmaz).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T119-baslangic-verisi-gocu`
- **Commit:** dal başı PR'da
- **PR:** #377
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `uuid_extract_version` PostgreSQL 18'de var (test v7 kimliği denetler) | `StartupDataTests.SeedRows_CarryTheMigrationsMomentAndVersion7Ids` postgres:18'de geçti | ✓ |
| 2 | `ON CONFLICT DO NOTHING` hedefsiz yazıldığında ifade dizinli tek-satır benzersizliğini de kapsar | `StartupDataReapplyTests` ikinci uygulamada hata vermeden geçti | ✓ |
| 3 | Ülke listesi ISO 3166-1'in resmî 249 alfa-2 kodudur | Türkçe adlar CLDR `tr` temelli; test sayıyı ve biçimi denetler — ⚠ 2 | ✓ (⚠) |

Dış servis, plan, kota ya da paket varsayımı yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Taslak metinlerin yer tutucularının (`{{Alan}}`) gösterimde doldurulması | — (T366'nın işi) | Gösterim T366'nındır; bu task yalnız satırı yazar |
| 2 | `06 §8.2.1` açılışta tek satırların varlık denetimi (fail-fast) | — | T119'un kriterlerinde yok; açılış denetiminin sahibi task'tır |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** önceki işçi geçti — T117 41830794202/41830794252 success, T116 41825046782/41825046994 success, T115 41820820561 success.
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Okunan bölümler:** `11` T119; `06 §8` (8.1–8.5), §4 girişi, §4.1–§4.4, §4.54, §4.56; `02 §11.1`, §3.33.2, §12.2.5; `09 §7.1`, §10.6, §10.8, §12, §25; `scripts/ci/migration-dry-run.sh`.
- **Göç kuru çalıştırması:** `scripts/ci/migration-dry-run.sh` incelendi — sürüm etiketi yok, adım 3–4 (önceki imaj yeni şemaya karşı) atlanır; adım 2 boş veritabanına demeti uygular, entegrasyon şablonu aynı demeti uygular (952 test). `psql` yerelde yok; adım CI'da koşar.
- **⚠ kararlar:** PR açıklamasında numaralı.
