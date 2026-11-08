# T388 — Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **`rollback-check --restore-point`** (`Commands/RollbackCheckCommand.cs`, `Infrastructure/Backup/RollbackAudit.cs`): önce `track_commit_timestamp` açık mı bakar (kapalıysa sayım yapılamaz, 3); sonra `02 §3.34.8`'in sipariş tarafı tablolarını ve gelen bildirim kaydını (toplam 24 tablo) `pg_xact_commit_timestamp(xmin) > nokta` ile sayar, tablo başına satır sayısını yazar; toplam sıfırsa 0, değilse 3. Yalnız `SELECT` çalıştırır; işlem yazmaz.
- **`reapply-deletions --restore-point`** (`Commands/ReapplyDeletionsCommand.cs`, `Infrastructure/Logging/InstantDeletionLog.cs`): uygulama kaydındaki (`audit-yyyyMMdd.jsonl`, Türkiye günü) `InstantDeletion` satırlarını dönüş anından bugüne okur; her satırı ayrı işlemde modülün yeniden uygulama yöntemiyle uygular (`IMembershipModule.ReapplyAccountDeletionAsync` → `AccountDeletions.ReapplyAsync`; `IAdministrationModule.ReapplyAdminRemovalAsync` → `AdminAccounts.ReapplyRemovalAsync`, satır işi `DeleteAccountRowAsync` ortak yardımcıya çıkarıldı, `RemoveAsync` da onu kullanır). Sonra tek işlemde `SessionStore.EndEverySessionAsync` ve `LinkRecords.EndUnusedAsync(nokta)` (e-posta değişikliğini geri alma bağlantısı, kullanılmış ve zaten bitmiş bağlantılar hariç). Okunamayan satır ya da bilinmeyen tür sayılır, çıkış 3.
- **Komut kipi servis listesi** (`Commands/CommandHost.cs`): `RollbackAudit`, `InstantDeletionLog`; Membership/Administration modüllerinin kurucularının istediği `ImageProcessor`, `SiteAddress` (`AddSearchEngineData`), kurulum ayarı satırları (`AddInstallationSettings`), `DiskUsageGuard` + `IDiskUsageReader`. `RemoveAll<IHostedService>` sonrası da hizmet çalışmaz.
- **Denetim izi tarama testi** (`AuditTrailSourceScan`): `audit_logs` tablo adı kuralına yalnız `RollbackAudit.cs` içindeki `("audit_logs", string.Empty)` liste girişi için dar istisna; aynı dosyada ifade biçimi ve başka dosyada aynı giriş hâlâ bulgu.
- **Oturum yazım taraması**: `AdminAccounts` içindeki demet öğesi adı `Sessions` taramanın `Sessions` kümesi kuralına takıldığı için küçük harfli adlara çevrildi (davranış aynı).
- **CI** (`.github/workflows/ci.yml` 4. iş): hizmet konteyneri `track_commit_timestamp = on` ile yeniden başlatılır ve ayar doğrulanır; `Docs/CI_CD_SETUP.md` bir cümle.
- Komutlar `AdminCommandCatalog`'a kayıtlı (`09 §13.2` tablosu iki komutu zaten içeriyordu).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Commands/` (iki yeni komut, `CommandHost`, `AdminCommandCatalog`), `Infrastructure/Backup/RollbackAudit.cs`, `Infrastructure/Logging/InstantDeletionLog.cs`, `Infrastructure/Operations/DeletionReapplyResult.cs`, `Infrastructure/Security/{LinkRecords,SessionStore}.cs`
- `Modules/Membership/{AccountDeletions,MembershipModule,Contracts/IMembershipModule}.cs`, `Modules/Administration/{AdminAccounts,AdministrationModule,Contracts/IAdministrationModule}.cs`
- `tests/Shopfolio.UnitTests/Commands/RestoreCommandsTests.cs`, `tests/Shopfolio.UnitTests/Architecture/AuditTrailSourceScan{,Tests}.cs`, `tests/Shopfolio.IntegrationTests/Commands/RestoreCommandsTests.cs`
- `.github/workflows/ci.yml`, `Docs/CI_CD_SETUP.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.d/T388.md`, `Docs/DEFERRED_BACKLOG.d/T388.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `rollback-check` salt okuma, uygulama dururken | ✓ | `RollbackAudit` yalnız `SELECT`; Unit `TheCheck_HasNoWritingCall_AndTheScanSeesOneInASample`; Integration `RollbackCheck_OnlyReads_NoTransactionIsStartedThatWrites` (komut öncesi ve sonrası `pg_snapshot_xmax` eşit); komut kipinde sunucu/hosted service yok (`CommandStructureTests`) |
| 2 | `--restore-point` ofsetli; ofsetsiz/yalnız tarih 2 | ✓ | Unit `RestorePoint_WithoutAnOffset_OrADateAlone_IsAUsageError_AndNothingIsOpened` (6 satır, iki komut), `RestorePoint_IsRequired` |
| 3 | Her tabloda kesinleşme anına göre sayım | ✓ | Unit `TheCheck_CountsTheTablesOf02_3_34_8_AndTheInboundNotificationRecord`, `EveryTableOfTheModel_IsEitherCountedByTheCheck_OrListedAsNotTheOrderSide`; Integration `RollbackCheck_CountsARowAddedAfterThePoint_WhateverItsOwnTimeSays...` (satırın kendi zamanı 3 yıl önce), `..._CountsATransactionThatStartedBeforeThePointAndCommittedAfterIt` |
| 4 | Silme = süreye bağlı + anlık; boşaltma/bağ güncellemesi sayılır | ✓ | Sayım güncellemeyi de yakalar: Integration `RollbackCheck_CountsARowChangedAfterThePoint_TheEmptyingOfAnOrderRowIsAChange`; sipariş tarafı tablolar (orders, order_items, refund_ibans dışı sipariş kayıtları, `order_address_histories`...) listede. Süreye bağlı silmeyi (imha, IBAN) zamanlayıcı yeniden çalıştırır, komut gerektirmez; anlık silme `reapply-deletions` ile (kriter 6) |
| 5 | Sayı sıfır değilse dönülmez | ✓ | Unit `ARollbackIsAllowedOnlyWhenCommitMomentsAreKeptAndTheCountIsZero` (3 durum); Integration `RollbackCheck_CountsARowAddedAfter...` çıkış 3 + "dönülmez", `RollbackCheck_WithNoWriteAfterThePoint_ExitsWithZero` |
| 6 | `reapply-deletions` anlık silme satırlarını dönüş anından bugüne okur, aynı yolla uygular | ✓ (okuma ve uygulama); yazan çağıran D-700 | Unit `LogReader_KeepsTheDeletionLinesFromTheRestoreMomentOn_InTheOrderTheyWereWritten`, `LogReader_ReadsTheTurkeyDayOfTheLine_NotTheUtcDay`; Integration `Reapply_DeletesTheAccountsOfTheLinesFromTheRestoreMoment_AndLeavesTheOnesBefore` |
| 7 | Kapılar uygulanmaz; kaydı olmayan satır atlanır | ✓ | Integration `Reapply_AppliesNoGate_TheLastAdministratorIsRemovedAndAMissingRecordIsSkipped`, `Reapply_WritesNoTrailSentOrJobRow_AndTheSecondRunIsHarmless` (denetim izi, gönderim ve iş tabloları boş; ikinci koşu atlar) |
| 8 | Bütün oturumlar silinir; kullanılmamış bağlantıların ömrü dönüş anına; geri alma bağlantısı hariç | ✓ | Integration `Reapply_DeletesEverySession_AndEndsEveryUnusedLinkAtTheRestoreMoment_ExceptTheRevertLink` (kullanılmış ve süresi bitmiş bağlantı da dokunulmaz) |
| 9 | Göç ve imaj sınaması bu task'ta yok | ✓ | Diff'te göç/imaj değişikliği yok (T602 KK7) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 komut yalnız okur | ✓ | kriter 1 satırı |
| KK2 ofsetsiz an 2 | ✓ | kriter 2 satırı |
| KK3 sayım kesinleşme anına göre | ✓ | kriter 3 satırı (başlayıp sonra kesinleşen işlem dahil) |
| KK4 boşaltma güncellemesi sayılır | ✓ | kriter 4 satırı |
| KK5 sıfırdan büyük sayı dönüşü reddeder | ✓ | kriter 5 satırı |
| KK6 silmeler dönüş anından sonrası için uygulanır | ✓ | kriter 6 satırı (dönüş anından önceki satır uygulanmaz) |
| KK7 kapılar atlanır, eksik kayıt atlanır | ✓ | kriter 7 satırı |
| KK8 oturumlar silinir, geri alma bağlantısı sürer | ✓ | kriter 8 satırı |
| KK9 göç ve imaj sınaması yok | ✓ | kriter 9 satırı |
| `09 §25` / güvenlik taraması | ✓ | tam Unit (mimari taramalar dahil) ve tam Integration yeşil; komut satırına değer yazılmaz (`AuditLine.AdminCommand` yalnız ad ve boş kimlik); SQL tablo adları sabit listeden, an parametre |
| Sık FAIL (1) tarama istisnası geniş | ✓ | `audit_logs` istisnası tek giriş biçimi + tek dosya; ayrı testle ihlal örnekleri (`TheTableRule_AllowsOnlyTheRollbackCountsListEntry_InThatFile`) |
| Sık FAIL (2) tarama çağrı biçimi | ✓ | yeni tarama eklenmedi; salt-okuma taraması örnekle (`TheCheck_HasNoWritingCall_AndTheScanSeesOneInASample`) |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ (yok) | şema/göç değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | `09 §13.2` komut tablosu iki komutu zaten içeriyor; `AdminCommandCatalog` yorumu "T387" ile uyumlu; komut sayısını sayan başka yer yok (grep `create-admin`) |
| Sık FAIL (5) karar kaydısız 09 metni | ✓ (yok) | belge kuralı değişmedi; sapmalar backlog D-701–D-703'te |
| Modül yöntemi kendi işlemini açmıyor, SaveChanges çağırmıyor | ✓ | `ReapplyAsync`/`ReapplyRemovalAsync` `RequireTransaction`; işlemi komut açar (`ITransactionRunner`) |
| Modül kurucusu yeni bağımlılık | ✓ | kurucu değişmedi; Integration TAM koşuldu (2935) |
| `dotnet format` | ✓ | Web, UnitTests, IntegrationTests `--verify-no-changes` temiz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build --max-threads 3` | 5080 geçti, 0 kalan (dönüş komutları sınıfı 21) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests --no-build --max-threads 3` (yerel `postgres:18.6 -c track_commit_timestamp=on`) | 2935 geçti, 0 kalan (`RestoreCommandsTests` 12) |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (yerel/CI veritabanı `track_commit_timestamp = on` ister; üretim imajında zaten açık, `05 §7.6.1`)
- **Paketleme / dağıtım:** CI 4. iş ayar adımı (ci.yml)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T388-donus-denetimi`
- **Commit:** son head PR açıklamasında
- **PR:** PR açıklamasında
- **CI:** yoklanmadı (yönetici izler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `pg_xact_commit_timestamp(xmin)` satırın son yazım işleminin kesinleşme anını verir | Integration testleri (ekleme, güncelleme, başlayıp sonra kesinleşen işlem) | ✓ |
| 2 | Hizmet konteyneri yeniden başlatılınca `ALTER SYSTEM` ayarı kalır | ci.yml adımı `SHOW track_commit_timestamp = on` ile doğrular (CI'da ilk koşuda görülecek) | ? CI'da doğrulanır |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Anlık silme satırını yazan çağıran yok (sayfalar yok) | D-700 | Sayfalar T517/T522'de; satır işlem sonrası çağıranda yazılmalı |
| 2 | Hesapsız komutta kayıt kimliği `Guid.Empty` | D-701 | `09 §13.2` m.5 hesapsız komutu anmıyor, belge kararı |
| 3 | Yeniden uygulamada e-posta kuyruğa girmez | D-702 | `05 §8.5.10` bildirimi anmıyor, belge kararı |
| 4 | Sayılan tablo listesi `02 §3.34.8`'in ötesinde | D-703 | Belge doğrulaması |
| 5 | Kesinleşme anı bilinmeyen eski satırlar sayılmaz | D-704 | Yalnız elle kurulan ortam; bilgi |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** main son 3 run success (42524245424, 42524245383, 42522612870; önceki işçi).
- **Repo memory (bitiş kapısı 9):** `.claude/memory/MEMORY.md` "Güncel Durum" bu parça kuralı gereği dokunulmadı (faz kapısı aktarır).
- **⚠ kolay kararlar:** (1) anlık silme satırını üretici kod yok → D-700; (2) hesapsız komutta `Guid.Empty` → D-701; (3) yeniden uygulamada e-posta yok → D-702; (4) fazladan sayılan tablolar → D-703; (5) kesinleşme anı NULL satırlar → D-704; (6) çıkış kodları: ayar kapalı ya da sayı > 0 → 3, okunamayan satır / bilinmeyen tür → 3; (7) CI hizmet konteyneri ALTER SYSTEM + restart ile ayarlanıyor (konteynere komut argümanı verilemediği için).
- **Komut kipi servis listesi:** önceki işçinin yazdığı komutlar modülleri çözerken `ImageProcessor`, `SiteAddress`, `ConditionalLine` listesi ve `DiskUsageGuard` eksik çıktı (Integration testi bunu yakaladı); `CommandHost_ResolvesWhatTheReplayUses_...` testi artık sınıyor.
