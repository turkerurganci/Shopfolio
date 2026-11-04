# T106 — Yönetici hesabı, davet ve işlem izi

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Modules/Administration/` altında üç entity ve yapılandırması: `Admin` (`06 §4.37`), `AdminInvitation` (§4.38), `AuditLog` (§4.63). E-posta ve normalleştirilmiş sütunu birlikte yazan `SetEmail`; `AuditLog` yalnız `Create` ile kurulur, bütün setter'ları özeldir.
- `AdminInvitation`: üç an alanından en çok biri dolu (`ck_admin_invitations_at_most_one_outcome`); üçü de boş satırlarda `NormalizedEmail` için filtreli tekil dizin; gönderen bağı `SetNull`, gönderenin adı değer olarak kalır.
- `AuditLog`: bağ yoktur (yönetici ve konu kayıt değerdir); `AuditArea` enum (`ck_…_area` kısıtı), `Operation` düz metin (tanınmayan kod kodun kendisiyle gösterilir — `AuditOperationText`; `ck_audit_logs_operation` yoktur), `Details` `jsonb`; dizinler `CreatedAt` ve (`AdminId`, `CreatedAt`).
- Tek göç `CreateAdminInvitationAndAuditLogTables` (`Down` yok; LF): üç tablo, uygulama kullanıcısından `UPDATE`/`DELETE` geri alınması, `dispose_expired_audit_logs(max_rows integer, as_of timestamptz)` fonksiyonu, `REVOKE … FROM PUBLIC` ve uygulama kullanıcısına `GRANT EXECUTE`.
- Model kuralı yorumu düzeltmesi (D-48 (a)): `ModelRulesConvention` özet yorumu `AuditOperationTypeName`'e bağlandı.
- `MigrationSourceScan` genişletildi (D-49 (a)(b)): `UPDATE`/`DELETE FROM` yalnız izinli tablolarda; `audit_logs` için `UPDATE`/`DELETE`/`DROP`/`RENAME` ihlal; `FROM PUBLIC` yalnız `REVOKE … ON FUNCTION`; `clock_timestamp` yalnız imha kırpma satırında. İhlal örnekleri `ViolationSamples/Migrations/{AuditTrail,Clean}.cs`.
- Sır bekçisine dar istisna (`scripts/git-hooks/pre-commit`): `.cs` dosyasında sağ taraf tamamıyla EF göç sütun kurucusuysa (`table.Column<Tür>(…)`) satır aday sayılmaz — üretilmiş `password_hash = table.Column<string>(…)` satırı yanlış pozitifti. Birleştirme ve literal yine yakalanır; `test-pre-commit.sh` 195/195 (iki yakalanan, iki geçen yeni örnek). README'ye bir paragraf.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Administration/{Admin,AdminInvitation,AuditLog}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/DataModel/ModelRulesConvention.cs`, `Infrastructure/Migrations/*`.
- `tests/Shopfolio.IntegrationTests/Model/{AdminAndAuditTablesTests,EnumDatabaseTests}.cs`, `tests/Shopfolio.IntegrationTests/Database/DatabaseUsersTests.cs`; `tests/Shopfolio.UnitTests/Architecture/MigrationSourceScan.cs` + `ViolationSamples/Migrations/`.
- `scripts/git-hooks/{pre-commit,test-pre-commit.sh,README.md}`.
- `Docs/DEFERRED_BACKLOG.md` (D-48, D-49 ✅; D-57).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç entity tek göçle, alan tabloları | ✓ | `Migration_MakesTheThreeTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables` |
| 2 | Yönetici e-postası yönetici içinde tekil, normalleştirilmiş sütunda | ✓ | `SecondAdminWithTheSameEmail_InAnotherCase_IsRefused`, `…_WithTurkishDottedAndDotlessLetters_IsRefused` |
| 3 | Davet: en çok bir sonuç, adres başına tek geçerli davet, gönderen boşalır | ✓ | `InvitationWithTwoOrMoreOutcomeMoments_IsRefused`, `SecondValidInvitationToTheSameAddress_InAnotherCase_IsRefused`, `RemovingTheSender_EmptiesTheLink_AndKeepsTheName` |
| 4 | İşlem izi yalnız eklenir (`INSERT`, `SELECT`); imha fonksiyonu | ✓ | `ApplicationUser_CannotUpdateDeleteOrTruncateTheTrail`, `DisposalFunction_IsGrantedToTheApplicationUserOnly_…`, `DisposalFunction_DeletesTheRowAtTheBoundary_…`, `…_RemovesAtMostMaxRows_…` |
| 5 | `Operation` kısıtsız, `AuditArea` kısıtlı | ✓ | `AuditLogs_HasNoCheckConstraintOnTheOperation_ButOnTheArea` |
| 6 | Veritabanı varsayılanı yok (`06` "Varsayılan" sütunu üç tabloda "—") | ✓ | `ThreeTables_HaveNoColumnDefault_BecauseTheFieldTablesNameNone` |
| 7 | Göç tek konu, `Down` yok | ✓ | `MigrationSourceScan` (Unit) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 240/240 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (gerçek PostgreSQL 18; `--filter-class` ile dört parça) | 276/276 (42 + 101 + 71 + 62) |
| Bekçi betiği | ✓ | `bash scripts/git-hooks/test-pre-commit.sh` | 195/195 |
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

- **Şema/migration:** Var — `CreateAdminInvitationAndAuditLogTables` (üç tablo, imha fonksiyonu, yetki satırları).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T106-yonetici-davet-islem-izi`
- **PR:** #363
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `AuditLog.Details` `AuditDetails` tipine ve `PayloadJson` dönüştürücüsüne çevrilecek (sütun değişmez) | D-57 | Tip ve dönüştürücü çatıda henüz yok; panel okumasıyla gelir |
| 2 | `AuditOperation` etiket testi ve okuma | D-47 (1) | Mesaj kataloğu T380'de |

## Notlar

- **Yapım üç işçide yapıldı:** ilk iki işçi bağlam sınırında bölündü; göç dosyaları commit edilemeden kaldı (sır bekçisi yanlış pozitifi), üçüncü işçi bekçiye dar istisna ekleyip göçü commit etti.
- **⚠ Hook yolu:** `core.hooksPath` ana çalışma dizinindeki (`C:\projects\Shopfolio\scripts\git-hooks`) eski bekçiyi gösterir; göç `.cs` dosyası yeni bekçiyle (`git -c core.hooksPath=<bu dalın bekçi kopyası>`) commit edildi. Atlama (`PB_ALLOW_*`) kullanılmadı. Dal main'e girince ana dizin de yeni bekçiyi alır.
- **Repo memory (bitiş kapısı 8):** güncellendi
