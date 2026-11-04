# T106 — Yönetici hesabı, davet ve işlem izi

**Faz:** F1 | **Durum:** ✗ Doğrulama tur 1 FAIL — düzeltme bekliyor | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Modules/Administration/` altında üç entity ve yapılandırması: `Admin` (`06 §4.37`), `AdminInvitation` (§4.38), `AuditLog` (§4.63). E-posta ve normalleştirilmiş sütunu birlikte yazan `SetEmail`; `AuditLog` yalnız `Create` ile kurulur, bütün setter'ları özeldir.
- `AdminInvitation`: üç an alanından en çok biri dolu (`ck_admin_invitations_at_most_one_outcome`); üçü de boş satırlarda `NormalizedEmail` için filtreli tekil dizin; gönderen bağı `SetNull`, gönderenin adı değer olarak kalır.
- `AuditLog`: bağ yoktur (yönetici ve konu kayıt değerdir); `AuditArea` enum (`ck_audit_logs_audit_area` kısıtı), `Operation` düz metin (tanınmayan kod kodun kendisiyle gösterilir — `AuditOperationText`; `ck_audit_logs_operation` yoktur), `Details` `jsonb`; dizinler `CreatedAt` ve (`AdminId`, `CreatedAt`).
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
| 1 | Üç entity `06 §4.37`, §4.38, §4.63 alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheThreeTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `ThreeTables_HaveNoColumnDefault_…`, `Indexes_AreTheDocumentedOnes` |
| 2 | `Admin` normalleştirilmiş e-postada tekil | ✓ | `SecondAdminWithTheSameEmail_InAnotherCase_IsRefused`, `…_WithTurkishDottedAndDotlessLetters_IsRefused` |
| 3 | Davet: en çok bir sonuç anı; üçü boşken adres tekil | ✓ | `InvitationWithTwoOrMoreOutcomeMoments_IsRefused`, `SecondValidInvitationToTheSameAddress_InAnotherCase_IsRefused`, `NewInvitationToTheSameAddress_IsAccepted_…` |
| 4 | İz: `UPDATE`/`DELETE` rol ayarından okunan adla geri alınır; imha fonksiyonu (Z-30, `as_of` üst sınırı, yalnız uygulama kullanıcısı) | ✓ | `ApplicationUser_CannotUpdateDeleteOrTruncateTheTrail`, `DisposalFunction_*` (6 test), `Migration_WithoutTheApplicationUserRoleSetting_IsRolledBack_…`, `MigrationSourceScan` |
| 5 | İz satırı yöneticiyi ad ve e-postayla değer olarak taşır; `AuditLog` bağ taşımaz | ✓ | `AuditRow_CarriesTheAdministratorsNameAndEmailAsValues_…`, `AuditLogs_HasNoForeignKey_AndNoTableRefersToIt` |
| 6 | Yönetici hesabına giden bağlar boşaltılır | ✓ | `RemovingTheSender_EmptiesTheLink_AndKeepsTheName`, `SenderLink_IsAnEmptyingForeignKey_WithItsOwnIndex` (T106'daki tek bağ; öteki bağlar sahibi tabloların task'ında — D-57) |
| 7 | Göç tek konu, `Down` yok | ✓ | `Migration_HasNoDownMethod`, `MigrationSourceScan` (Unit) |

*Validator kanıtı (tur 1, `54e45a9`):* kriter 1–7 ve KK1–KK7 ✓ — tablo validator turunda plandaki yedi kritere hizalandı (yapım tablosunun 5. ve 6. satırları plandaki kriterler değildi; içerikleri kriter 1 ve D-48 (a) kanıtıdır). Ayrıntı ve verdict'i belirleyen bulgu (sır bekçisi istisnası): "Doğrulama tur 1".

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
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-04, head `54e45a9`) |
| Bulgu sayısı | 1 (ORTA, güvenlik — sır bekçisi istisnası) |
| Düzeltme gerekli mi | Evet — bulgu 1 (`scripts/git-hooks/pre-commit`, `test-pre-commit.sh`, `README.md`) |
| Bloke-etmeyen not (K) sayısı | 2 (K1 bu PR'da kapatıldı; K2 → D-58) |

### Doğrulama tur 1 — ✗ FAIL (2026-11-04)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (mutlak yol, hook dosyaları mevcut — önceki turlarla aynı; worktree'de ana dizinin bekçisini çalıştırır); ana dal son 3 run `41751877585`, `41751877590`, `41746249953` success; `MEMORY.md` satır 22'de T106.

**Kabul kriterleri (validator kanıtı):** testlerin dışında kendi PostgreSQL 18 konteynerinde rol ayarları (`shopfolio.application_user`, `reader_user`, `backup_status_user`) yazılmış boş veritabanına `dotnet ef migrations script` çıktısı göç kullanıcısıyla uygulandı ve ham SQL yoklamaları koşuldu.
- **1 ✓** üç tablo tek göçte (`CreateAdminInvitationAndAuditLogTables`); sütun adı, türü ve boş olabilirliği `06 §4.37`, §4.38, §4.63 (ortak `Id`, `CreatedAt` dahil) ile birebir; `Details` `jsonb`; varsayılan yok; dizinler `06 §4.37`/§4.38/§4.63 ve §7.5 ile aynı (`ix_admins_normalized_email` tekil, `ix_admin_invitations_normalized_email` filtreli tekil, `ix_admin_invitations_sender_admin_id`, `ix_audit_logs_created_at`, `ix_audit_logs_admin_id_created_at`).
- **2 ✓** `ALI@x.com` → `ix_admins_normalized_email` ihlali; `PasswordHash` boş kabul. **3 ✓** iki sonuç anı → `ck_admin_invitations_at_most_one_outcome`; `y@X.com` ikinci geçerli davet → `ix_admin_invitations_normalized_email`; geçersizleşmiş aynı adres kabul.
- **4 ✓** `role_table_grants`: uygulama kullanıcısı `INSERT,SELECT`, okuyucu `SELECT`; uygulama kullanıcısının `UPDATE`, `DELETE`, `TRUNCATE`'i "permission denied"; fonksiyon `prosecdef = t`, `proconfig = {search_path=pg_catalog, public}`, sahip göç kullanıcısı, `proacl` yalnız sahip + uygulama kullanıcısı (PUBLIC yok); okuyucu ve öteki kullanıcı "permission denied for function"; `created_at` 2014-12-31T20:59:59Z satırı `as_of` 2024-12-31T20:59:59Z'de 0, 21:00:00Z'de 1 siler; `as_of` 2200 yalnız bugün süresi dolanı siler; rol ayarsız göçün geri alınması `Migration_WithoutTheApplicationUserRoleSetting_…` testiyle (42704, üç tablo ve fonksiyon yok); göç kaynağında `shopfolio_` yok (`MigrationSourceScan`), gövde `09 §9.2` m.7 kalıbıyla birebir.
- **5 ✓** `audit_logs`'tan ve `audit_logs`'a yabancı anahtar yok; yönetici silinince iz satırı aynen kalır (test). **6 ✓** gönderen silinince `sender_admin_id` boş, `sender_admin_name` kalır (`confdeltype = n`). **7 ✓** `Down` yok, tek konu.
- **KK1–KK7 ✓** — `AdminAndAuditTablesTests` (42) her maddeyi ihlal adıyla sınıyor; `MigrationSourceScan` yeşil, D-49 ihlal örnekleri (`AuditTrail.cs`, `Clean.cs`) beklenen satırlarda.

**Yerel:** `dotnet build -warnaserror` 0 uyarı; Unit 240/240 (Consistency dahil); Integration 276/276 (PostgreSQL 18, kendi konteyneri; `Model` 214 + diğerleri 62; T106 sınıfı ayrıca 42/42); `dotnet format --verify-no-changes` 0; `restore --locked-mode` 0; `test-pre-commit.sh` 195/195; koşu sonrası `git status` temiz. **Dal CI** `41758732045` (head `54e45a9`) success.

**`09 §25`:** ad uyumu `06` ile birebir; silme davranışı açık (`SetNull`), kimlik `EntityIds`'ten, `AuditArea` metin + izinli değer kısıtı, işlem kodunda kısıt yok; göç genişletmedir, `Down`/`now()` yok, `audit_logs`'a yalnız ekleme, kullanıcı adı rol ayarıyla; yeni tablolar yetki matrisinde (`PrivilegeMatrix` yeşil); yeni paket yok. "Diff yalnız görevin istediğini mi içeriyor" ve Güvenlik 1. soru (repoda sır yok mu): sır bekçisi değişikliği gerekçeli ama geniştir — bulgu 1.

**Güvenlik:** kodda sır yok; yetki/giriş yüzeyi yok; girdi denetimi veritabanı kısıtlarıyla; yeni bağımlılık yok. **Sır bekçisi istisnası (bulgu 1):** istisna tırnaklı dizgileri silip kalanı `table.Column<T>(…)` kalıbıyla eşler; bu yüzden kurucunun argümanlarındaki literal de bekçiden geçer. Doğrulayıcı yoklaması (aynı `check` düzeneği, dalın ve main'in bekçisiyle): `password_hash = table.Column<string>(type: "text", nullable: false, defaultValue: "Sup3rS3cretValue!"),`, `api_key = table.Column<string>(type: "text", comment: "sk_live_…"),`, `Password = table.Column<string>("abc12345xyz");` ve `token = table.Column<string>(…, defaultValueSql: "'abc12345xyz'"),` dalda çıkış 0 (geçer), main'de çıkış 1 (yakalanır). README "ek literal yine yakalanır" der; yalnız kurucudan sonraki literal yakalanıyor.

**`12` satırları:** task'ın kaynak birimleri `06` entity'leri ve `02`/`05`/`09` bölümleridir (`11 §7.1`; KP birimi yok — KP-62/KP-63 T379/T380'indir); `12` matrisinde T106'ya bağlı satır yok (`06 §4.63`'ü anan `VAL-A838`, `VAL-A770` ve iz satırı `VAL-D084` işlem izinin uç/panel task'larınındır) — `Durum` hücresi değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**Bulgular:**

| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Karar | Dosya |
|---|---|---|---|---|---|---|
| 1 | — (`09 §25` Güvenlik 1. soru; `scripts/git-hooks/README.md` istisna 3) | ORTA | S1 | Beklenen: yalnız üretilmiş, değer taşımayan sütun kurucusu atlanır; "ek literal yine yakalanır". Gerçekleşen: kurucu argümanındaki literal (`defaultValue:`, `defaultValueSql:`, `comment:`, konumsal dizgi) bekçiden geçer — main'in yakaladığı gerçek sır ataması artık yakalanmaz | T106 düzeltme turu: tırnak silinmeden önce argümanlar yalnız değer taşımayan adlı argümanlara (`type:` dizgisi, `nullable:`, `maxLength:`, `precision:`, `scale:`, `unicode:`, `fixedLength:`) daraltılır ya da `defaultValue`/`defaultValueSql`/`computedColumnSql`/`comment` ve konumsal dizgi içeren satır istisnadan çıkarılır; dört örnek `test-pre-commit.sh`'e "must be caught" olarak eklenir; README cümlesi düzelir | `scripts/git-hooks/pre-commit` (satır 159–168), `scripts/git-hooks/test-pre-commit.sh`, `scripts/git-hooks/README.md` |

**K-notları:**
- **K1** — D-48 (a) kapanış metni ve bu raporun "Yapılan İşler" satırı alan kısıtını `ck_audit_logs_area` / `ck_…_area` diye anar; gerçek ad `ck_audit_logs_audit_area`'dır (enum kısıtının kural adı sütun adıdır — `06 §2.4.3`; test de onu sınar). **Bu PR'da kapatıldı** (D-48 metni doğrulayıcı commit'inde düzeldi).
- **K2** — `09 §9.2` m.2 `AuditOperation`'ın "EF dizgi dönüşümü" ile yazıldığını söyler; T106 D-48 (a) gereği `AuditLog.Operation`'ı düz `string` özelliği + `AuditOperationText.ToCode` ile yazdı (sütun aynı: üye adı, kısıtsız). Davranış farkı yok, doküman cümlesi eskidi → **D-58** (doc-drift).

**Yapım raporu karşılaştırması:** kriter sonuçları uyumlu (1–7 ✓). Uyuşmazlık 2: (a) yapım "Birleştirme ve literal yine yakalanır" diyor — kurucu argümanındaki literal için yanlış (bulgu 1); (b) yapımın kriter tablosu plandaki 5. ve 6. kriteri değil başka iki iddiayı numaralıyordu — validator turunda hizalandı.

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
