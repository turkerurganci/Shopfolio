# T103 — Başlangıç listeleri, firma ve yasal metin sürümü

**Faz:** F1 | **Durum:** ✗ Doğrulama tur 1 FAIL (düzeltme bekliyor) | **Tarih:** 2026-11-04

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
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-04, head `6e88812`) |
| Bulgu sayısı | 1 (DÜŞÜK, S1) |
| Düzeltme gerekli mi | Evet — bulgu 1 |
| Bloke-etmeyen not (K) sayısı | 3 (K1, K3 bu PR'ın düzeltme turunda kapatılır; K2 → D-49) |

### Doğrulama tur 1 — ✗ FAIL (2026-11-04)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` mutlak yol (`C:\projects\Shopfolio\scripts\git-hooks`, hook içeriği main ile aynı); ana dal son 3 run `41715894986`, `41715895065`, `41710384453` success; `MEMORY.md`'de T103 satırı var.

**Kabul kriterleri (validator kanıtı; PostgreSQL 18 konteyneri, göç T10 kurucu SQL'iyle kurulan veritabanına göç kullanıcısıyla uygulandı — "Applying migration 20261103205423_CreateSettingsTables. Done."; KK2–KK6 ayrıca uygulama kullanıcısıyla ham SQL yoklamasıyla sınandı):** 1–7 ✓, KK1–KK7 ✓. KK1 altı tablo; KK2 ikinci `companies` ve `company_settings` satırı `ix_*_single_row` (`((true))`) ile reddedildi; KK3 IBAN'sız açık havale, yarım iade adresi, eksi eşik ve asgari tutar, süre/pencere/indirme/KDV çitleri, dokuz rakamlı firma telefonu, iade telefonu ve WhatsApp numarası, harfli telefon, geçersiz ana sayfa düzeni reddedildi (her biri adıyla `ck_` kısıtı), on rakamlı telefon kabul; KK4 `123`/`6` il kodu, `tr`/`TUR` ülke kodu, tekrarlanan kod ve aynı ilin ikinci teslimat satırı reddedildi; KK5 aynı tür+numara, numarasız yayın anı ve tersi, Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi taslağı, ikinci aydınlatma metni taslağı, **şablon sürümlü aydınlatma metni** (yayımlanmış ve taslak — `ck_legal_text_versions_template_version_only_generated`), çerez politikasında barındırma konumu ve barındırma konumsuz yayımlanmış aydınlatma metni reddedildi; üretilen metinde şablon sürümü kabul; KK6 bağlı il silme `fk_companies_provinces_province_id` ile reddedildi, teslimat ili satırı silindi, bağlar `NO ACTION` (`confdeltype=a`); KK7 `__EFMigrationsHistory`'de uygulama ve salt okuma kullanıcısının SELECT/INSERT/UPDATE/DELETE yetkisi yok, göç kullanıcısınınki var; altı yeni tabloda uygulama kullanıcısı okur-yazar. Yerel: `restore --locked-mode` 0; Release `-warnaserror` 0 uyarı; Unit 217/217; Integration 119/119; `has-pending-model-changes` temiz. Dal CI `41721390301` (`6e88812`) success, PR #360 dokuz denetim yeşil.

**Bulgu 1 (DÜŞÜK, S1 Sapma; `09 §25` "Kod hiçbir doküman cümlesiyle çelişmiyor mu" → hayır):** göç tek satırlık iki tabloya yabancı anahtar dizini kurar — `ix_companies_province_id`, `ix_company_settings_return_province_id`. `06 §7.5.2`: "Tek satırlık kayıtlar yalnız tek satırın tekil dizinini taşır"; `06 §4.1` ve §4.2 "İndeksler" yalnız tek satırın tekil dizinini sayar (§4.1: "yabancı anahtarın dizini gerekmez"); `06 §7.2` tablosu iki bağ için "Dizin: gerekmez — tek satır". PR ⚠ karar 2 "kaldırma etkisiz kaldı; zararsız" diyerek bıraktı, BLOCKED bildirmedi. Validator yoklaması: `ConfigureConventions`'a `Conventions.Remove<ForeignKeyIndexConvention>()` eklenince `dotnet ef migrations has-pending-model-changes` "Changes have been made to the model" der — dizinler kaldırılabilir (deneme geri alındı). Düzeltme: iki dizin modelden kalkar (kural genel kaldırılırsa `06 §7.2`'nin "Dizin" sütunu dizin isteyen bağlara açık `HasIndex` yazılır; ya da yalnız bu iki dizin hedefli kaldırılır), göç yeniden üretilir (göç henüz ana dalda değil), tek satırlık tabloların dizin kümesi testle kilitlenir; rapor ve PR'daki karar 2 ile "Diğer" notu düzeltilir.

**K-notları:** K1 (bu PR'ın düzeltme turunda) KK5'in "şablon sürümlü aydınlatma metni" maddesini entegrasyon testi çerez politikasıyla sınar (`SettingsTablesTests.LegalTextVersion_TemplateVersionOnAFirmEditedText_IsRefused`); davranış doğru (validator ham SQL'le aydınlatma metninde gösterdi), test aydınlatma metni durumunu da sınamalı. K2 (→ D-49) `MigrationSourceScan` `09 §10.6` m.11'in "`UPDATE` ve `DELETE FROM` yalnız izinli tablolardadır" ve `audit_logs` denetimlerini içermez; `PublicPrivilege` kuralı `09 §10.8` m.7'nin imha fonksiyonu için istediği `REVOKE … FROM PUBLIC`'i de reddeder. T103 göçünde etkisi yok. K3 (bu PR'ın düzeltme turunda) `EnumLockTests.StoredEnums_AreTheUnionOf…` totolojiktir — `StoredEnums()` tanımı zaten `Persistent() ∪ ModelEnums()`'tır, test bir süzgecin geri gelmesini yakalamaz; D-48 (b)'nin üretim değişikliği (süzgecin kalkması) doğrudur, kilidi anlamlı bir örnekle (ad alanı dışındaki model enumu) sınanmalı.

**Yapım raporu karşılaştırması:** kriter 1–7 ve test sayıları uyumlu; uyuşmazlık 1 — yapım FK dizinlerini "zararsız" sayıp bıraktı (⚠ karar 2, Notlar "Diğer"), validator `06 §7.5.2` ile çelişki sayar ve kaldırmanın mümkün olduğunu gösterdi. CI düzeltmeleri (`a69f574` imaja `.editorconfig`; `1b317d0` dry-run kurucu SQL ile rol ayarlı göç kullanıcısı) `09 §10.6` m.9 ve §10.8 m.5 ile uyumlu; iki `PB_ALLOW_DIRECT_PUSH` `[ci-failure]` atlaması `BYPASS_LOG`'da (`b185bf6`, `6e88812`), `PB_ALLOW_SECRET` yok. D-37 (a), D-46 (a)(b), D-48 (b) kapanışları doğrulandı.

## Düzeltme turu 1 (2026-11-04)

- **Bulgu 1 (S1, DÜŞÜK) — kapandı:** `AppDbContext.ConfigureConventions` çerçevenin `ForeignKeyIndexConvention`'ını kaldırır; yerine `ModelRulesConvention` her yabancı anahtara aynı dizini (başında anahtar sütunları olan bir dizin ya da anahtar yoksa) kendisi ekler, ama `AsSingleRow()` ile işaretli tabloya eklemez. `Company` ve `CompanySettings` işaretlidir; böylece yalnız tek satırın tekil dizinini taşır (`06 §7.5.2`, §7.2). Diğer tablolarda davranış değişmedi (açık `HasIndex`'ler korunur). Göç `CreateSettingsTables`, tasarım dosyası ve anlık görüntüden iki dizin çıkarıldı (göç ana dalda değil; model ile uyumlu, `has-pending-model-changes` temiz). PR ⚠ karar 2 düzeltildi.
- **Test:** `SettingsTablesTests.SingleRowTables_CarryOnlyTheSingleRowIndex` (veritabanında yalnız `pk_*` ve `ix_*_single_row`) ve `ModelOfTheSingleRowTables_HasNoForeignKeyIndex` (modelde dizin yok).
- **K1 — kapandı:** şablon sürümü testi artık aydınlatma metnini sınar: yayımlanmış ve taslak aydınlatma metninde `TemplateVersion` reddedilir (`ck_…_template_version_only_generated`); çerez politikası testi ayrı kaldı.
- **K3 — kapandı:** totolojik enum kilit testi (`StoredEnums_AreTheUnionOf…`) yerine `EveryEnumOfTheModel_HasALockFileOnDisk_WhateverItsNamespace`: modelin her enum özelliği için kilit dosyası diskte bağımsızca aranır (`StoredEnums()` kullanılmaz).
- **K2:** D-49'a (T106) ileri alındı, bu turda değişmedi.
- **Yerel sınama:** `build -c Release -warnaserror` 0 uyarı 0 hata; Unit 217/217; Integration 123/123 (PostgreSQL 18); `scripts/ci/migration-dry-run.sh` (`MIGRATION_DRYRUN_CMD`, Docker `shopfolio-t103d1-pg`, iş bitince silindi) çıkış 0, "Done".

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
- **CI düzeltmesi (6. Migration dry-run):** `scripts/ci/migration-dry-run.sh` artık göçü T10 kurucu SQL dosyalarıyla (`deploy/images/database/bootstrap`) kurulan veritabanına göç kullanıcısıyla uygular; üç rol ayarı hazırdır. Ayar yokken göç 42704 ile düşmeye devam eder. Yerelde PostgreSQL 18 ile koşuldu: geçti.
- **Diğer:** Tek satırlık iki tabloda FK dizini yok (düzeltme turu 1: `06 §7.5.2`); ilk sürümdeki "zararsız" notu geçersizdir.
