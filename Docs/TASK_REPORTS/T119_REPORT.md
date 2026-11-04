# T119 — Başlangıç verisi göçü ve seed olmayanların sınırı

**Faz:** F1 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-04

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
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `7bf60e6`, PR #377) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-69; K2 bu PR'da kapandı) |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Kapılar:** working tree temiz; `core.hooksPath` mutlak yolla `scripts/git-hooks` (ana checkout'un hook'ları; eşdeğer); ana dal son 3 run `41839444317`, `41839444366` (T118 squash) ve `41830794202` (T117) success; `MEMORY.md`'de T119 satırı var (satır 22).

**Kabul kriterleri:** kapsam tam (8 kriter, KK1–KK8, `09 §25`, güvenlik); hepsi ✓. Validator göçleri `dotnet ef migrations script` ile SQL'e çevirip deyimleri `06 §8.1`–§8.4, §4 girişi, §4.1–§4.4, §4.54, §4.56, §4.60, §4.65, §4.67, `02 §11.1`, §3.33.2, `04` 9.14.4 ve `09 §10.1`, §10.6, §25 satırlarına karşı elle okudu:
- Kriter 1: `InsertStartupData.Up` yalnız `migrationBuilder.Sql`; `src`'de (Migrations dışı) başlangıç kayıtlarına hiçbir okuma ya da yazma yolu yok (`DbSet` tanımları dışında başvuru yok — grep); fabrikalar (`Company.Create` vb.) yalnız testlerden çağrılıyor; `StartupDataSourceScan` fabrika çağrısı, kurucu, `Add` ve elle `INSERT`'i yakalıyor, ihlal örneği 13 satırda kırmızı.
- Kriter 2: 81 il `01 Adana` … `81 Düzce`, 81 plakanın tamamı resmî sırayla elle karşılaştırıldı (33 Mersin, 46 Kahramanmaraş, 67–81 sonradan kurulan iller dahil); ülke 249 ISO 3166-1 alfa-2 kodu (`AD` … `ZW`, `TR` Türkiye); `ON CONFLICT (code) DO NOTHING` `Code`'un tekil dizinine dayanıyor.
- Kriter 3: altı tek satır `INSERT (id, created_at) … ON CONFLICT DO NOTHING`; öteki sütunlar veritabanı varsayılanından — `brandings.is_platform_signature_enabled true`, `version 1`; `channel_state_records.consecutive_channel_error_count 0`; anlar boş (`06 §4.54`, §4.60, §4.65, §4.67).
- Kriter 4 (para hassasiyeti): üretilen SQL `0.00, NULL, NULL, 2, 2, 5, 20.00` → P-1 **0 TL**, P-3 **Boş**, P-4 **Boş**, P-5 **2 iş günü**, P-7 **2 iş günü**, P-8 **5**, P-10 **%20** (`numeric(5,2)` yüzde, `06 §4.2`: 0 ≤ oran < 100) — `02 §11.1` ile birebir; tutarlar `numeric(12,2)`, çitler (P-5 ≤ 10, P-7 1–3, P-8 ≥ 1) içinde. Havale ve geçici kapatma `false`, yedi bölüm sürümü `1` (D-52 göçünün varsayılanı), `ContentFirst`, menü adları `04` 9.14.4 ile aynı. **Mutasyon:** P-10 18,00 yapıldı → `SettingDefaultsConsistencyTests` kırmızı ("Expected 20, Actual 18,00"); geri alındı, yeşil.
- Kriter 5: 81 teslimat ili ayar satırıyla aynı deyimde (`WITH settings AS (INSERT … RETURNING id) … WHERE EXISTS`) — firmanın çıkardığı il ikinci uygulamada geri gelmiyor.
- Kriter 6: iki taslak `version_number`, `published_at`, `hosting_location`, `template_version` boş; `WHERE NOT EXISTS` türe göre; metin kapalı biçim setinde (`h3`, `p`, `ul`, `li`, `strong` — `09 §15.3` m.2). İçeriğin hukuki denetimi T366'nın kriteri 2'sidir (`11`).
- Kriter 7: iki göçte `UPDATE`/`DELETE` yok (üretilen SQL grep); `MigrationSourceScan` izinli tablo listesiyle yeni göçleri de tarıyor; ikinci uygulama testi göçün geçmiş satırını silip göç kullanıcısıyla gerçekten yeniden çalıştırıyor — 11 tablonun sayısı ve iki satırın JSON'u aynı; `Down` yok.
- Kriter 8: seed olmayanlar göç sınıfının özetinde `06 §8.3.1`, §8.4.1–§8.4.3 ile madde madde aynı; göçlerde `data_protection_keys` ve üretilen iki metnin satırı yok (tarama + boş kurulumda sayım 0).
- **D-52 kapsamı:** backlog satırının sekiz sütununa ek `is_bank_transfer_enabled`, `is_sales_temporarily_closed` (`false`) ve `legal_text_versions.version` (`1`) — üçü de `06 §4.2`, §4.56'da Varsayılan'ı dolu ve seed onları veritabanından alıyor; T103 tablolarında Varsayılan'ı dolu başka sütun kalmadı. Kapsam genişlemesi gerekçeli, task dışı davranış yok.

**Testler (validator, yerel PostgreSQL 18 `shopfolio-t119-val-pg`):** build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` çıkış 0; Unit 267/267 (Consistency dahil; yeni 22 test ayrıca 22/22); Integration 952 — T119 sınıfları 26/26, Model iki grupta 482/482 ve 408/408, Model dışı 62/62; yerel zaman aşımı yok; `dotnet restore --locked-mode` çıkış 0; koşu sonrası `git status` temiz, konteyner silindi. Dal CI `41843874734` (head `7bf60e6`) success — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E (advisory) yeşil (önceki `41843801361` head `4daacc5` iptal — yeni push). Göç kuru çalıştırmasında sürüm etiketi olmadığından önceki imaj adımı atlanıyor (ilk etiket D-40).

**Güvenlik:** sır yok; auth etkisi yok — yetki deyimi yok, göç yalnız `INSERT` ve `SET DEFAULT`; girdi doğrulama uygulanamaz (uç yok; taslak metin sabit, kapalı biçim setinde); yeni bağımlılık yok.

**`12` satırları:** task'ın kaynak birimleri `06 §8.1`, §8.4 ve `09 §10` (alt bölüm birimleri; KP yok). `12`'de bu birimleri, K-1031'i ya da `09 §10.6`'yı anan satır yok (grep 0); `Durum` değişikliği yok; `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** K1 — göçün sabit v7 kimlikleri (`InsertStartupData.Id`) `09 §10.1` m.2'nin "üretim tek yerdedir" kuralında anılmıyor; ihlal değil (yasaklı çağrı yok), belgede kalıp yazılı değil → **D-69**. K2 — rapordaki follow-up 2'nin cümlesi eksikti ("sahibi task'tır"); sahibi T205 (`11 §7.1`) — **bu PR'da düzeltildi**.

**Yapım raporu karşılaştırması:** kriter tablosu, KK listesi, ⚠ kararlar (D-52 kapsamı, ülke listesi, taslaklar, ikinci uygulama) ve dış varsayımlar validator'ın bağımsız okumasıyla uyumlu; test sayıları birebir (Unit 223 + Consistency 44 = 267; Integration 952). Tek fark K2 (eksik cümle).

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
- **Post-merge run'lar:** squash `07e6121` (#377) — CI `41848143907` ve Publish `41848143892` success (F1 gate'inde doğrulandı, 2026-11-04)

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
| 2 | `06 §8.2.1` açılışta tek satırların varlık denetimi (fail-fast) | — (T205'in işi) | T119'un kriterlerinde yok; açılış denetiminin sahibi T205'tir (`11 §7.1`: `06:§8.2` → T205) |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** önceki işçi geçti — T117 41830794202/41830794252 success, T116 41825046782/41825046994 success, T115 41820820561 success.
- **Repo memory (bitiş kapısı 8):** güncellendi.
- **Okunan bölümler:** `11` T119; `06 §8` (8.1–8.5), §4 girişi, §4.1–§4.4, §4.54, §4.56; `02 §11.1`, §3.33.2, §12.2.5; `09 §7.1`, §10.6, §10.8, §12, §25; `scripts/ci/migration-dry-run.sh`.
- **Göç kuru çalıştırması:** `scripts/ci/migration-dry-run.sh` incelendi — sürüm etiketi yok, adım 3–4 (önceki imaj yeni şemaya karşı) atlanır; adım 2 boş veritabanına demeti uygular, entegrasyon şablonu aynı demeti uygular (952 test). `psql` yerelde yok; adım CI'da koşar.
- **⚠ kararlar:** PR açıklamasında numaralı.
