# T107 — Müşteri hesabı

**Faz:** F1 | **Durum:** ✓ PASS (doğrulama tur 1) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Modules/Membership/` altında üç entity ve yapılandırması: `Customer` (`06 §4.36`), `PendingEmailChange` (§4.40), `AddressBook` (§4.45). `Customer` ve `PendingEmailChange` e-postayı ve normalleştirilmiş sütununu birlikte yazan `SetEmail` / `SetNewEmail` ile kurar (`TextNormalizer.NormalizeIdentity`); T106'daki `Admin` kalıbıyla aynı.
- `Customer`: `NormalizedEmail` tekil dizin; `ck_customers_unverified_has_password_and_no_google` (`email_verified_at IS NOT NULL OR (password_hash IS NOT NULL AND google_linked_at IS NULL)`). Sürüm yok (`06 §2.6.5`). Alan tablosunda varsayılan "—" olduğundan hiçbir sütunda veritabanı varsayılanı yok.
- `PendingEmailChange`: `CustomerId` ve `AdminId` boş olabilir; `ck_pending_email_changes_exactly_one_account` (`num_nonnulls(customer_id, admin_id) = 1`); iki bağ ayrı ayrı filtreli tekil dizin (`... IS NOT NULL`); `NormalizedNewEmail` tekil değil. İki bağ da `Cascade`.
- `AddressBook`: `CustomerId` `Cascade`, `ProvinceId` `NoAction` (kısıtlayıcı); `Phone` `varchar(10)` ve `ck_address_books_phone_ten_digits` (`Company.Phone` kalıbı); telefon isteğe bağlı. İki bağın dizini (`06 §7.5`).
- Tek göç `CreateCustomerAddressBookAndPendingEmailChangeTables` (`Down` yok; LF, BOM yok): üç tablo.
- **Modül sınırı:** `PendingEmailChange.AdminId` (`Administration.Admin`) ve `AddressBook.ProvinceId` (`Settings.Province`) uygulamadaki ilk modüller arası yabancı anahtarlardır. `09 §6` madde 2 başka modülün iç türüne başvuruyu yasaklar (mimari test kırmızıya döndü); yapılandırma asıl türü `HasOne("<tam ad>", navigationName: null)` ile **adıyla** bağlar, tür başvurusu yoktur. Model ve göç aynıdır (`has-pending-model-changes`: değişiklik yok). ⚠ karar — PR açıklaması 1, D-61.
- `ModelRuleTests.RegisteredCascades` listesine üç basamaklı bağ eklendi (`06 §2.2.5`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/{Customer,PendingEmailChange,AddressBook}.cs`, `Infrastructure/AppDbContext.cs` (üç `DbSet`), `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{CustomerAccountTablesTests,ModelRuleTests}.cs`.
- `Docs/DEFERRED_BACKLOG.md` (D-61), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç entity `06 §4.36`, §4.40, §4.45 alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheThreeTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `ThreeTables_HaveNoColumnDefault_…` (`column_default` yok), `Indexes_AreTheDocumentedOnes`, `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable`, `CheckConstraints_AreTheDocumentedOnes` |
| 2 | `Customer` normalleştirilmiş e-postada tekil; doğrulanmamış hesapta şifre özeti dolu, Google bağı boş | ✓ | `SecondCustomerWithTheSameEmail_InAnotherCase_IsRefused`, `…_WithTurkishDottedAndDotlessLetters_IsRefused`, `UnverifiedPendingRow_HoldsTheAddress_AgainstAVerifiedRow`, `UnverifiedCustomer_*` (3 ret, 1 kabul), `VerifiedCustomer_IsAcceptedWithOrWithoutPasswordAndGoogleLink` |
| 3 | `PendingEmailChange` tam bir hesaba bağlı; her bağ ayrı tekil; yeni e-posta tekil değil | ✓ | `PendingChange_LinkedToNeitherAccount_IsRefused`, `…_LinkedToBothAccounts_IsRefused`, `SecondPendingChange_OfTheSameCustomer_IsRefused`, `SecondPendingChange_OfTheSameAdmin_IsRefused`, `TwoAccounts_MayWaitForTheSameNewEmail_InAnyCase`, `CustomerAndAdmin_MayEachHaveOnePendingChange` |
| 4 | `AddressBook.Phone` on rakam | ✓ | `AddressWithAPhoneOtherThanTenDigits_IsRefused` (5 örnek), `AddressWithATenDigitPhone_IsAccepted`, `AddressWithoutAPhone_IsAccepted_…` |
| 5 | Hesapla adres defteri ve bekleyen değişiklik basamaklanır (`06 §2.2.5`) | ✓ | `DeletingTheCustomer_TakesTheAddressBookAndThePendingChangeWithIt`, `DeletingTheAdmin_TakesThePendingChangeWithIt_…`, `Province_UsedByAnAddress_CannotBeDeleted`, `ModelRuleTests` basamak listesi |
| 6 | Göç tek konu, `Down` yok | ✓ | `Migration_HasNoDownMethod`, `MigrationSourceScan` (Unit) |

*Validator kanıtı (tur 1, `e765c68`):* kriter 1–6 ve KK1–KK6 ✓ — boş PostgreSQL 18.6 veritabanına `dotnet ef database update` ile beş göç uygulandı; `pg_constraint` (`confdeltype` dahil) okundu ve ham SQL yoklamaları koşuldu (ayrıntı: "Doğrulama tur 1").

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 240/240 (modül sınırı testi dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests` (PostgreSQL 18, kendi konteyneri) | T107 sınıfı 39/39; `ModelRuleTests` 14/14; tam koşu 315 (ilk koşuda yalnız cascade listesi eksikti, düzeltildi) — *validator tur 1:* düzeltmeden sonra tam koşu 315/315 (`--filter-namespace` ile dört parça: Model 253, Database 29, FileSecrets 16, Web 17) |
| Betik testleri | ✓ | `sh tests/scripts/run-script-tests.sh` | 4 dosya geçti (validator tur 1) |
| Göç kuru koşusu | ✓ (adım 1) | `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration" (validator tur 1) |
| Biçim / kilit | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes`; `dotnet restore --locked-mode` | çıkış 0 / 0 |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `e765c68`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (mutlak yol, hook dosyaları mevcut — önceki turlarla aynı); ana dal son 3 tamamlanmış run `41767748868` (CI), `41767748965` (Publish, deneme 2), `41751877585` (CI) success — yapım notundaki Publish kırmızısı yeniden denemede yeşile döndü; `MEMORY.md` satır 22'de T107.

**Kabul kriterleri (validator kanıtı):** kendi PostgreSQL 18.6 konteynerinde `pg_trgm`'li boş veritabanına `dotnet ef database update` ile beş göç uygulandı (rol ayarları `shopfolio.application_user`, `shopfolio.reader_user` önceden yazıldı); `pg_constraint` okundu ve ham SQL yoklamaları koşuldu.
- **1 ✓** — üç tablo (`customers`, `pending_email_changes`, `address_books`) tek göçte (`CreateCustomerAddressBookAndPendingEmailChangeTables`); sütun adları, türleri (`text`, `timestamptz`, `uuid`, telefon `varchar(10)` — `Company.Phone` kalıbı) ve boş olabilirlik `06 §4.36`, §4.40, §4.45 ile (ortak `Id`, `CreatedAt` dahil) birebir; varsayılan yok (alan tablolarında "—"); dizinler `06 §7.1` (`ix_customers_normalized_email`, iki filtreli tekil bağ) ve §7.2 "kendi dizini" (`ix_address_books_customer_id`, `ix_address_books_province_id`) ile eşit, `NormalizedNewEmail`'de dizin yok; `has-pending-model-changes` temiz.
- **2 ✓** — `ALI@EXAMPLE.COM` / `ISIK`–`ısık` → `ix_customers_normalized_email`; `SetEmail` görünen ve normalleştirilmiş değeri birlikte yazar (`09 §9.4` m.1, ayarlayıcılar özel). Yoklama: doğrulanmamış + şifresiz satır ve doğrulanmış satırı `email_verified_at = NULL, google_linked_at = now()` ile güncelleme → `ck_customers_unverified_has_password_and_no_google`; kısıt tanımı `06 §4.36`/§7.3 cümlesiyle birebir.
- **3 ✓** — iki bağ boş ve ikisi dolu → `ck_pending_email_changes_exactly_one_account` (`num_nonnulls(...) = 1`); aynı müşteri ve aynı yöneticiye ikinci satır → `ix_pending_email_changes_customer_id` / `admin_id` (`WHERE … IS NOT NULL`); üç hesap aynı yeni adresi bekleyebilir.
- **4 ✓** — dokuz hane, `+90…`, harf, boşluk → `ck_address_books_phone_ten_digits`; on bir hane sütuna sığmaz; yoklama: sonu satır sonlu `532123456\n` → aynı kısıt; on hane ve boş telefon kabul.
- **5 ✓** — yoklama: müşteri silinince adres defteri ve bekleyen değişiklik 0'a iner; `confdeltype` müşteri ve yönetici bağlarında `c`, il bağında `a` (`NO ACTION`, `RESTRICT` değil — `06 §7.2`); kullanılan il silinemez. `RegisteredCascades` `06 §2.2.5`'in hesap satırıyla hizalandı.
- **6 ✓** — tek göç, `Down` yok; göç dosyasında `suppressTransaction`, `CONCURRENTLY`, `VACUUM`, `ALTER SYSTEM`, `CREATE DATABASE`, `now()`, `CREATE EXTENSION`, `UPDATE`, `DELETE FROM` yok.
- **KK1–KK6 ✓** — `CustomerAccountTablesTests` (39) her maddeyi ihlal adıyla sınıyor; `Modules/Membership`'te `Guid.NewGuid`, `HasColumnName`, `ToTable`, `Restrict`/`Client…`, `DateTime.Now`, `ToLower(` yok.

**Modüller arası bağ (D-61):** `PendingEmailChange.AdminId` ve `AddressBook.ProvinceId` başka modülün varlığını `HasOne("<tam ad>", navigationName: null)` ile adla bağlar. `09 §6` m.2 bir modülün öteki modülün iç **türüne** başvurusunu ve bağlam üzerinden veriye dokunmayı yasaklar; burada tür başvurusu, sorgu ya da yazım yoktur, yalnız `06 §7.2`'nin istediği veritabanı bağı kurulur — madde aynı ad başvurusunu göç ve anlık görüntü için zaten kabul eder; `05 §3.4.1`'in yasağı tabloya doğrudan okuma/yazmadır. Kod hiçbir doküman cümlesiyle çelişmez; kalıbın `09`'da adlandırılmaması bir doküman boşluğudur ve D-61 (`k-note`, T108 ya da `09`'a dokunan ilk PR) onu doğru yere forward eder. Ad yanlış olsaydı EF gölge varlık açardı — `ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable` (`fk_…_admins_…`, `fk_…_provinces_…`) ve bekleyen model değişikliği denetimi bunu yakalar.

**Yerel:** `dotnet build -warnaserror --no-incremental` 0 uyarı; Unit 240/240 (Consistency dahil); betik testleri 4 dosya; Integration 315/315 (PostgreSQL 18, kendi konteyneri; dört parça); `dotnet format --verify-no-changes` 0; `restore --locked-mode` 0; koşu sonrası `git status` temiz. **Dal CI** `41769181335` (head `e765c68`) dokuz iş success (E2E advisory success).

**`09 §25`:** ad uyumu (`Name`, `Email`, `NormalizedEmail`, `PasswordHash`, `EmailVerifiedAt`, `GoogleLinkedAt`, `CustomerId`, `AdminId`, `NewEmail`, `NormalizedNewEmail`, `RecipientName`, `ProvinceId`, `District`, `AddressLine`, `Phone`) `06` ile birebir; kapsam yalnız üç entity + göç + testler; silme davranışı her ilişkide açık (`Cascade` ×3, `NoAction` ×1); kimlik `EntityIds`'ten; göç genişletmedir; yeni tablolar varsayılan yetki matrisinde (`PrivilegeMatrix_OnTheTablesOfTheDatabase_HasNoDeviation` yeşil); enum, kilit dosyası, yük biçimi yok (`09 §12` uygulanamaz); sürüm yok (`06 §2.6.5`); yeni paket yok.

**Güvenlik:** sır yok (testlerde yer tutucu özet dizgisi); yetki/giriş yüzeyi yok; girdi denetimi veritabanı kısıtlarıyla (uygulama yazımı sonraki task'larda); yeni bağımlılık yok.

**`12` satırları:** task'ın kaynak birimleri (`11 §7.1`: `06:§4.36`, §4.40, §4.45, `02:§1.3`; KP birimi yok) kaynak hücresinde yalnız `VAL-D121`'i getirir (PBKDF2 şifre özeti, `06 §4.36`, §4.37) — şifre özetleyicisi sonraki task'ındır (T106'daki kararla aynı), T107'nin kriterlerine bağlı değil → `BEKLEMEDE` kaldı; `VAL-D079` (`06 §4.42`) bağlantı kaydının task'ındır. `Durum` hücresi değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**Bulgular:** yok.

**K-notları:** yok.

**Yapım raporu karşılaştırması:** kriter sonuçları uyumlu (1–6 ✓). Uyuşmazlık 1: yapım raporu tam Integration koşusunu cascade düzeltmesinden önce yaptı (durum satırı "T107 sınıfı 39/39"); doğrulayıcı düzeltilmiş dalda 315/315 koştu — sonuç aynı.

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `customers`, `address_books`, `pending_email_changes`.
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T107-musteri-hesabi`
- **PR:** #364
- **CI:** dal CI `41769181335` (head `e765c68`) yeşil — doğrulayıcı tur 1
- **Post-merge run'lar:** squash `d374bc1` (#364) — CI `41770864703` ve Publish `41770864743` success (F1 gate'inde doğrulandı, 2026-11-04)

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Modüller arası yabancı anahtar kalıbı (ad ile bağlama) mimari testte tanımlı değil | D-61 | `09 §6` kararı gerektirir; ilk örnek T107 |
| 2 | Yönetici hesabına giden öteki bağlar (`RecognizedBrowserMark`, `Session`, `LinkRecord` ...) | D-57 (T106) | Sahibi tabloların task'ı (T108...); `PendingEmailChange.AdminId` bu task'ta kuruldu |

## Notlar

- **Working tree (Adım -1):** temiz (yeni worktree).
- **Ana dal CI startup (Adım 0):** `41767748868` CI success; `41767748965` Publish **failure** (T106 birleşimi; kayıt itme adımında `unknown blob` — görev kodu dışı, ortam/kayıt kaynaklı görünüyor); `41751877585` CI success. Publish kırmızısı yapım işçisinin kapsamı dışında, yardımcıya bildirildi.
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** Kimlik tablosu `Id`/`CreatedAt` ortak alanları T106 kalıbıyla. Telefon "00000..." gibi sıfır dolu on rakamı veritabanı kabul eder; geçerlilik uygulama kuralıdır.
