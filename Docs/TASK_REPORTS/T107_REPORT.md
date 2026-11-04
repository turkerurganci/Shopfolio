# T107 — Müşteri hesabı

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-04

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

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 240/240 (modül sınırı testi dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests` (PostgreSQL 18, kendi konteyneri) | T107 sınıfı 39/39; `ModelRuleTests` 14/14; tam koşu 315 (ilk koşuda yalnız cascade listesi eksikti, düzeltildi) |
| Biçim / kilit | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes`; `dotnet restore --locked-mode` | çıkış 0 / 0 |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — (bekliyor) |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `customers`, `address_books`, `pending_email_changes`.
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T107-musteri-hesabi`
- **PR:** PR açıklamasında.
- **CI:** yapım işçisi izlemez.
- **Post-merge run'lar:** *(validator doldurur)*

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
