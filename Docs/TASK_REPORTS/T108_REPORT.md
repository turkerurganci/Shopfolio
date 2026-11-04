# T108 — Oturum, bağlantı, deneme ve giriş kayıtları

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Infrastructure/Security/` altında (düzeltme 1; ilk yapımda `Modules/Membership/`) beş entity ve yapılandırması: `Session` (`06 §4.39`), `RecognizedBrowserMark` (§4.41), `LinkRecord` (§4.42), `AttemptRecord` (§4.43), `LoginRecord` (§4.44). Özellik adları `06` alan tablolarıyla birebir (`ReauthenticatedOperation`, `ReauthenticationTokenHash`, `AdminInvitationId` ...). Hiçbir alanın varsayılanı yok (alan tablolarında "—"); `column_default` testi boş döner.
- `Session`: `TokenHash` tekil; `ck_sessions_exactly_one_account`, `ck_sessions_reauthentication_fields_together`, `ck_sessions_admin_without_account_deletion`; `LastActivityAt` dizini; iki bağ `Cascade`. `GrantReauthentication` iznin iki alanını birlikte yazar.
- `RecognizedBrowserMark`: `MarkHash` tekil; `ck_..._exactly_one_account`; `ExpiresAt` dizini; iki bağ `Cascade`.
- `LinkRecord`: `TokenHash` tekil; (`LinkType`, `ExpiresAt`) dizini ve `PreviousEmail` dolu satırlarda filtreli `ExpiresAt` dizini; kontrol kısıtları: üç sahipten tam biri, davet bağı yalnız `AdminInvitation` türünde, `EmailVerification` ve `GoogleLinkConfirmation` türlerinde müşteri bağı, `PreviousEmail` yalnız `EmailChangeRevert` türünde, `UsedAt`/`SupersededAt` en çok biri; üç bağ `Cascade` (davetin bağlantı kaydı davetle gider).
- `AttemptRecord`: bağ yok; `AttemptLimit` izinli değerleri L1–L6 ve L9 (L7/L8 reddedilir, `06 §2.9.2`); `ck_attempt_records_axis_is_the_limits_axis` (L4 yalnız IP, L6 yalnız sepet, tanınan tarayıcı yalnız L1, L1/L2/L3/L5/L9 e-posta ya da IP); pencere sayımı bileşik dizini ve `CreatedAt` dizini.
- `LoginRecord`: bağ yok; `IpAddress` `inet` (`IPAddress`); tüm ayarlayıcılar özel, tek yol `Create`; `LoginRecordsGuard` (SaveChanges kesicisi) değişmiş satırı SQL gitmeden reddeder (`PriceHistory` / `CatalogRecordsGuard` kalıbı); `CreatedAt`, `NormalizedEmail`, `IpAddress` dizinleri.
- Tek göç `CreateSessionLinkAttemptAndLoginTables` (`Down` yok; LF, BOM yok): beş tablo.
- **D-61 kapandı:** dört modüller arası bağ (`Session.AdminId`, `RecognizedBrowserMark.AdminId`, `LinkRecord.AdminId`, `LinkRecord.AdminInvitationId`) `HasOne("<tam ad>", navigationName: null)` ile adla kuruldu (ad `const string`'te). Kalıp cümlesi `09 §10.1` madde 1'e yazıldı; `DataModelSourceScan`'a "modül tür adı dizgisi `const string` dışında yazılmaz, sabit yalnız `HasOne`'da kullanılır" kuralı ve iki kasıtlı ihlal örneği eklendi (`ViolationSamples/DataModel/ModuleTypeName*.cs`).
- `ModelRuleTests.RegisteredCascades` (yedi yeni basamaklı bağ) ve `EnumDatabaseTests` enum sütun sayısı (15 → 21) güncellendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/{Session,RecognizedBrowserMark,LinkRecord,AttemptRecord,LoginRecord}.cs`, `Infrastructure/AppDbContext.cs` (beş `DbSet`, `LoginRecordsGuard`), `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
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
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-04, head `e046beb`, PR #365) |
| Bulgu sayısı | 1 (S1, ORTA — oturum kayıtlarının yeri) |
| Düzeltme gerekli mi | Evet — beş varlık `Modules/Membership/`'ten `Infrastructure/`'a taşınır (doğrulama raporu Bulgu 1) |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-62, K2 → D-63) |

### Doğrulama tur 1 — ✗ FAIL (2026-11-04)

**Kapılar:** working tree temiz; `core.hooksPath` mutlak yolla `scripts/git-hooks` (eşdeğer); ana dal son 3 run `41770864743`, `41770864703`, `41767748868` success; `MEMORY.md` satır 22'de T108.

**Kabul kriterleri ve testler:** 8 kriterin hepsi validator kanıtıyla ✓ (göç tek, beş tablo, kısıtlar/dizinler/FK'lar `06` ile birebir, `LoginRecord` yalnız eklenir, `Down` yok). Yerel: build `-warnaserror` 0 uyarı, `dotnet format` temiz, Unit 244/244, Integration 385/385 (T108 sınıfı 70/70, PostgreSQL 18), `has-pending-model-changes` "No changes", kilitli restore temiz. Dal CI `41772781004` success (head `e046beb`).

**Bulgu 1 (S1, ORTA):** beş varlık ve yapılandırmaları `src/Shopfolio.Web/Modules/Membership/` altında (`Shopfolio.Modules.Membership`); `05 §3.3.11`, `05 §1.2.18`, `11` aday tablosu (T108 Modül = Ortak altyapı), `09 §4.1` m.2 ve `09 §15.4` m.5 bu kayıtları ortak altyapıya (`Infrastructure/`) verir. `09 §6` m.2 gereği ortak altyapı (`AttemptCounter`) modül türüne başvuramaz. Düzeltme: varlıklar + yapılandırma + `LoginRecordsGuard` `Infrastructure/` altına, modül türlerine bağlar adla; göç tasarım dosyası ve anlık görüntü yeniden üretilir (şema aynı); `09 §10.1` m.1 cümlesi "altyapıdan modüle" bağı da kapsar. `09 §25`'te iki soru "hayır" (doküman çelişkisi, `§4.1` düzeni).

**K-notları:** K1 → D-62 (`DataModelSourceScan` `HasOne(` satırında sabitsiz düz dizgiyi yakalamıyor); K2 → D-63 (`06 §7.3` `LinkRecord` satırında `GoogleLinkConfirmation` eksik). `12` matrisinde T108'e bağlı satırların `Durum`'u değişmedi (sonraki task'lara bağlı, BEKLEMEDE).

## Düzeltme 1 (2026-11-04, PR #365)

| Bulgu | Değişiklik | Test |
|---|---|---|
| Bulgu 1 (S1, ORTA): beş varlık modül klasöründe | `Session`, `RecognizedBrowserMark`, `LinkRecord`, `AttemptRecord`, `LoginRecord` ve yapılandırmaları + `LoginRecordsGuard` `src/Shopfolio.Web/Infrastructure/Security/`'e, ad alanı `Shopfolio.Infrastructure.Security`'ye taşındı (`05 §3.3.11`, `09 §4.1` m.2, `09 §15.4` m.5). Ortak altyapı modül türüne başvurmadığı için (`09 §6` m.2) `Customer`, `Admin`, `AdminInvitation` bağları `HasOne("<tam ad>", navigationName: null)` + yapılandırmadaki `const string` kalıbına çevrildi; `LoginRecord`'un kullandığı `AccountType` (`Membership.Contracts`'taydı) `Infrastructure/Values/`'a taşındı (kilit dosyası adı da değişti, değerler aynı). `09 §10.1` m.1 "ortak altyapıdan modüle" bağını da kapsar | Release build `-warnaserror` temiz; `dotnet format --verify-no-changes` çıkış 0; Unit 245/245; `ModuleBoundaryTests` yeşil |
| Göç tasarım dosyası ve anlık görüntü | Yerinde yeniden üretildi (aynı göç kimliği `20261104040730_…`, `Up` gövdesi aynı, `Down` yok). Anlık görüntü yalnız tür adları ve varlık sırası bakımından değişti (satır kümesi, ad değişimi hariç, birebir aynı); tablo, sütun, kısıt ve dizin adları aynı | `dotnet ef migrations has-pending-model-changes` → "No changes"; Integration T108 sınıfı 70/70, Model ad alanı 323/323, öteki 62/62 (PostgreSQL 18, `shopfolio-t108-fix-pg`, silindi) |
| K1 → D-62 | `DataModelSourceScan` `.HasOne(` satırında da düz dizgiyi yakalar (yalnız sabit adı serbest); kasıtlı ihlal örneği `ViolationSamples/DataModel/ModuleTypeNameInHasOne.cs`; D-62 "✅ Çözüldü → T108". D-63 (doc-drift) açık kalır | `Scan_OnViolationSample_ReportsTheRuleOnItsLine` yeni örnekle (Unit 245/245) |

`dotnet restore Shopfolio.slnx --locked-mode` çıkış 0. Doğrulama tur 2 bekliyor.

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
