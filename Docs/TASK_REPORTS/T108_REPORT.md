# T108 — Oturum, bağlantı, deneme ve giriş kayıtları

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Modules/Membership/` altında beş entity ve yapılandırması: `Session` (`06 §4.39`), `RecognizedBrowserMark` (§4.41), `LinkRecord` (§4.42), `AttemptRecord` (§4.43), `LoginRecord` (§4.44). Özellik adları `06` alan tablolarıyla birebir (`ReauthenticatedOperation`, `ReauthenticationTokenHash`, `AdminInvitationId` ...). Hiçbir alanın varsayılanı yok (alan tablolarında "—"); `column_default` testi boş döner.
- `Session`: `TokenHash` tekil; `ck_sessions_exactly_one_account`, `ck_sessions_reauthentication_fields_together`, `ck_sessions_admin_without_account_deletion`; `LastActivityAt` dizini; iki bağ `Cascade`. `GrantReauthentication` iznin iki alanını birlikte yazar.
- `RecognizedBrowserMark`: `MarkHash` tekil; `ck_..._exactly_one_account`; `ExpiresAt` dizini; iki bağ `Cascade`.
- `LinkRecord`: `TokenHash` tekil; (`LinkType`, `ExpiresAt`) dizini ve `PreviousEmail` dolu satırlarda filtreli `ExpiresAt` dizini; kontrol kısıtları: üç sahipten tam biri, davet bağı yalnız `AdminInvitation` türünde, `EmailVerification` ve `GoogleLinkConfirmation` türlerinde müşteri bağı, `PreviousEmail` yalnız `EmailChangeRevert` türünde, `UsedAt`/`SupersededAt` en çok biri; üç bağ `Cascade` (davetin bağlantı kaydı davetle gider).
- `AttemptRecord`: bağ yok; `AttemptLimit` izinli değerleri L1–L6 ve L9 (L7/L8 reddedilir, `06 §2.9.2`); `ck_attempt_records_axis_is_the_limits_axis` (L4 yalnız IP, L6 yalnız sepet, tanınan tarayıcı yalnız L1, L1/L2/L3/L5/L9 e-posta ya da IP); pencere sayımı bileşik dizini ve `CreatedAt` dizini.
- `LoginRecord`: bağ yok; `IpAddress` `inet` (`IPAddress`); tüm ayarlayıcılar özel, tek yol `Create`; `LoginRecordsGuard` (SaveChanges kesicisi) değişmiş satırı SQL gitmeden reddeder (`PriceHistory` / `CatalogRecordsGuard` kalıbı); `CreatedAt`, `NormalizedEmail`, `IpAddress` dizinleri.
- Tek göç `CreateSessionLinkAttemptAndLoginTables` (`Down` yok; LF, BOM yok): beş tablo.
- **D-61 kapandı:** dört modüller arası bağ (`Session.AdminId`, `RecognizedBrowserMark.AdminId`, `LinkRecord.AdminId`, `LinkRecord.AdminInvitationId`) `HasOne("<tam ad>", navigationName: null)` ile adla kuruldu (ad `const string`'te). Kalıp cümlesi `09 §10.1` madde 1'e yazıldı; `DataModelSourceScan`'a "modül tür adı dizgisi `const string` dışında yazılmaz, sabit yalnız `HasOne`'da kullanılır" kuralı ve iki kasıtlı ihlal örneği eklendi (`ViolationSamples/DataModel/ModuleTypeName*.cs`).
- `ModelRuleTests.RegisteredCascades` (yedi yeni basamaklı bağ) ve `EnumDatabaseTests` enum sütun sayısı (15 → 21) güncellendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/{Session,RecognizedBrowserMark,LinkRecord,AttemptRecord,LoginRecord}.cs`, `Infrastructure/AppDbContext.cs` (beş `DbSet`, `LoginRecordsGuard`), `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{SessionLinkAttemptLoginTablesTests,ModelRuleTests,EnumDatabaseTests}.cs`; `tests/Shopfolio.UnitTests/Architecture/{DataModelSourceScan,DataModelSourceScanTests}.cs` ve iki ihlal örneği.
- `Docs/09_CODING_GUIDELINES.md` (§10.1 m.1), `Docs/DEFERRED_BACKLOG.md` (D-61), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Beş entity alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheFiveTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `FiveTables_HaveNoColumnDefault_…`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_…`, `CheckConstraints_AreTheDocumentedOnes` |
| 2 | Oturum: tam bir hesap; tekil özet; izin çifti; hesap silme izni yok | ✓ | `Session_LinkedToNeitherAccount_…`, `…_LinkedToBothAccounts_…`, `Session_WithOnlyThe*OfThePermission_…`, `AdminSession_WithTheAccountDeletionPermission_…`, `SecondSession_WithTheSameTokenDigest_…` |
| 3 | İşaret özeti tekil, hesap bağı tam bir | ✓ | `SecondMark_WithTheSameDigest_…`, `Mark_LinkedToBothAccounts_…`, `Mark_LinkedToNeitherAccount_…` |
| 4 | Bağlantı kaydı: sahip, davet türü, tür–hesap, eski adres, kullanım–yerine geçme, tekil özet | ✓ | `InvitationLink_WithoutAnInvitation_…`, `NonInvitationLink_WithAnInvitation_…`, `CustomerOnlyLinkTypes_WithAnAdminLink_…`, `Link_WithNoOwner/TwoOwners_…`, `Link_UsedAndSupersededTogether_…`, `PreviousEmail_On*`, `SecondLink_WithTheSameTokenDigest_…` |
| 5 | Deneme kaydı ekseni limitin eksenine uyar | ✓ | `Attempt_OnTheLimitsAxis_IsAccepted` (13), `Attempt_OffTheLimitsAxis_IsRefused` (14), `Attempt_OfALimitCountedFromTheOrders_IsRefused` |
| 6 | Giriş kaydı yalnız eklenir | ✓ | `LoginRecords_AreAppended_…`, `LoginRecord_HasNoPublicSetter`, `UpdatingALoginRecordThroughTheContext_IsRefused_…` |
| 7 | Sahibiyle basamaklanma; deneme ve giriş kaydı kalır | ✓ | `DeletingTheCustomer_…`, `DeletingTheAdmin_…`, `DeletingTheInvitation_TakesItsLinks_…`, `ModelRuleTests` basamak listesi |
| 8 | Göç tek konu, `Down` yok | ✓ | `Migration_HasNoDownMethod`, `MigrationSourceScan` (Unit) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 244/244 (kaynak taraması kuralı ve örnekleri dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests` (PostgreSQL 18, kendi konteyneri; ad alanı parçaları) | Model 323, Database 29, Web 17, FileSecrets 16 = 385; T108 sınıfı 70/70 |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — `sessions`, `recognized_browser_marks`, `link_records`, `attempt_records`, `login_records`.
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T108-oturum-baglanti-deneme-giris`
- **PR:** #365
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Yönetici hesabına giden öteki bağlar (`TransitionRecord.AdminId` ...) | D-57 (T106) | Sahibi tabloların task'ı; T108'in dört bağı kuruldu |

## Notlar

- **Working tree (Adım -1):** temiz (yeni worktree); `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** yardımcı düzeninde yardımcının işi; işçi `gh run` yoklamaz.
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** Sır bekçisi `…hash = Digest(...)` atamasını yakaladığı için izin çifti `Session.GrantReauthentication` ile yazılır; indeks tanımı testinde `ad: tanım` yerine `ad | tanım` ayracı kullanıldı. `AttemptRecord` satırı sepet birleşmesinde güncellenebildiğinden değiştirilebilirdir (yalnız `LoginRecord` ekleme-yalnız).
