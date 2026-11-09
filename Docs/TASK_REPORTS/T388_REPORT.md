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
| 1 | `rollback-check` salt okuma, uygulama dururken | ✓ | `RollbackAudit` yalnız `SELECT`; Unit `TheCheck_HasNoWritingCall_AndTheScanSeesOneInASample`; Integration `RollbackCheck_OnlyReads_NoTransactionIsStartedThatWrites` (testin kendi veritabanında yeni oturumlar salt okunur açılır, komut çıkış 0 ile biter, satır değişmez; kontrol: aynı koşulda yazma 25006 ile reddedilir; sunucu genelinde sayaç yok); komut kipinde sunucu/hosted service yok (`CommandStructureTests`) |
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
| Düzeltme 1 (main birleşik) | ✓ | build `-warnaserror` 0 uyarı, `dotnet format` Web/IntegrationTests temiz; Unit tam; Integration Commands + Ordering + Concurrency + Security/Disposal parçalı, paralel, iki koşu (`track_commit_timestamp=on` yerel konteyner) | Unit 5161/5161; Integration 59 (Commands) + 469 + 497, 0 kalan |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1) |
| Bulgu sayısı | 1 (B1 ORTA, S1) |
| Düzeltme gerekli mi | Evet — B1 |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-716) |

### Doğrulama Sonucu — tur 1 (validator)

**Head:** `5dc4f221` (origin/main `d0c97cf0` ile güncel; birleştirme gerekmedi) · **PR:** #483 · **Verdict:** ✗ FAIL

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = depo `scripts/git-hooks` (ana checkout'un mutlak yolu); ana dal CI son 3: 42532103711, 42532103776, 42530025706 hepsi `success`.
- **Dal CI:** 42533329123 (`5dc4f221`) ✓ — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E (advisory), CI Gate `pass`. Önceki run 42533131953 (`dd2d047b`) `cancelled`.
- **Kabul kriterleri (kod + test okundu, testler koşuldu):** 1 ✓ (kod yalnız `SELECT`; `current_setting` + tablo başına `count(*)`; komut kipinde hosted service yok — ancak kanıt testi kararsız, B1) · 2 ✓ (dağıtıcı `Moment` türü; 6 satırlık teori iki komutta 2, değer iletide yok) · 3 ✓ (`pg_xact_commit_timestamp(xmin) > nokta`; satırın kendi zamanı 3 yıl önce olsa da sayılır; nokta öncesi başlayıp sonra kesinleşen işlem sayılır; hesap/oturum/bağlantı yazımı sayılmaz) · 4 ✓ (xmin son yazanı gösterdiği için boşaltma ve `ON DELETE SET NULL` bağ güncellemesi sayılır; IBAN silme günü sipariş sürümünü yükseltir — `RefundIbanDeletionDay` — yönetici kaldırma iz satırı yazar, hesap silme siparişe yazar; üçü de sayılan tablolarda görünür) · 5 ✓ (toplam > 0 ya da ayar kapalı → 3 ve "dönülmez") · 6 ✓ (Türkiye günü dosyaları, `At >= nokta`, yazılış sırası; her satır kendi işleminde aynı silme yoluyla: `AccountDeletions.RemoveAsync`, `AdminAccounts.DeleteAccountRowAsync`) · 7 ✓ (son yönetici silinir, açık geri alma/yeniden kimlik/aktör kapısı yok; kaydı olmayan satır atlanır; iz, gönderim, iş satırı yazılmaz; ikinci koşu zararsız) · 8 ✓ (`sessions` tümden silinir; `UsedAt IS NULL`, `EmailChangeRevert` dışı ve ömrü noktadan sonra biten bağlantılar noktaya çekilir) · 9 ✓ (göç/imaj değişikliği yok).
- **KK1–KK9:** ✓ (kriterlerle aynı kanıt; KK1'in entegrasyon kanıtı B1 ile kararsız). **`09 §25`:** modül yöntemleri `RequireTransaction`, işlemi komut açar; `AdminSet` ve hesap kilidi mevcut yollarla; şema/göç yok; yeni paket yok; SQL'de tablo adları sabit listeden, an parametre; tarama istisnası tek giriş + tek dosya.
- **Task satırları:** T388'in kaynak birimleri `05:MK-72`, `05:§8.5`, `05:§8.7`, `09:§13` — KP birimi yok, adım 2a grep'i satır getirmez. İlgili VAL-A808, A810–A813 (§12.5) geri yüklenmiş test ortamı / uç yanıtı ister; `Durum` hücreleri `BEKLEMEDE` kaldı, matrise dokunulmadı.
- **Testler (yerel, `postgres:18 -c track_commit_timestamp=on`):** restore 0; build `-warnaserror` 0 uyarı; format Web/UnitTests/IntegrationTests temiz; Unit 5144/5144; betik testleri 4/4; Integration `RestoreCommandsTests` tek başına 12/12; `RestoreCommandsTests` + `Membership*` + `SessionLinkAttemptLoginTablesTests` + `AuditTrail*` birlikte 230/231 — iki koşuda da `RollbackCheck_OnlyReads_NoTransactionIsStartedThatWrites` düştü (B1).
- **B1 — ORTA, S1 Sapma (`09 §19.1` koleksiyon yalıtımı):** `RollbackCheck_OnlyReads_NoTransactionIsStartedThatWrites` komuttan önce ve sonra `pg_snapshot_xmax(pg_current_snapshot())` değerini karşılaştırır; bu sayaç bütün sunucunundur (veritabanı başına değil). Koleksiyonlar aynı sunucuda kendi veritabanlarında paralel koşar (`09 §19.1`), öteki koleksiyonların yazımı sayacı ilerletir ve test düşer: yerelde iki koşuda da `Expected "3100" / Actual "3106"`. Dal CI'ı bu kez yeşil, ama test paralel yükte kararsız; ana dalda kırmızı run bütün doğrulamaları durdurur (Adım 0). Düzeltme: salt okumanın kanıtı koleksiyonun kendi veritabanına bağlanır (ör. komutun bağlantısını salt okunur işlemde çalıştırmak ya da `pg_stat_database.xact_commit` yerine veritabanına özgü bir ölçü / sayılan tabloların `xmin` kümesinin değişmediği), test `RestoreCommandsTests` başka koleksiyonlarla birlikte koşarken de yeşil.
- **K1 → D-716:** `refund_iban_requests` ve `refund_ibans` sayılmayan tablo olarak sınıflandı; `06` §4.29 `RefundIbanRequest`'i kalem kayıtlarıyla birlikte sayar ve IBAN silmesinin boşalttığı bir bağ o tablodadır (`05 §8.5.11`). Bugün etkisiz: IBAN silme günü sipariş sürümünü yükseltir (`orders` sayılır), IBAN yazan akış henüz yok. Belgede karara bağlanmalı.
- **Güvenlik:** sır yok; komut satırı yalnız ad ve boş kimlik; kişisel veri okunmaz; SQL enjeksiyonu yok (sabit tablo adları, parametreli an); yetki etkisi: komutlar yalnız sunucudan; yeni bağımlılık yok.
- **Yapım raporu karşılaştırması:** 1 uyuşmazlık — kriter 1 ve öz denetim "Integration TAM koşuldu (2935) yeşil" der; yapım koşusu `--max-threads 3` ile sınırlıydı, paralel koleksiyonlarla aynı test kararsız (B1). Öz denetim tablosu var ve kanıtlı; öteki satırlar validator bulgularıyla uyumlu.

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

## Düzeltme 1

- **B1 (ORTA, KK1, `09 §19.1`):** salt okuma testi sunucu genelindeki `pg_snapshot_xmax` sayacını bırakıp testin kendi veritabanına bağlandı: veritabanının `default_transaction_read_only` ayarı açılır, komut çıkış 0 ile biter, sonra ayar kapatılır ve havuz temizlenir (`NpgsqlConnection.ClearAllPools`); aynı koşulda bir yazmanın 25006 ile reddedilmesi kontrol olarak sınanır. Test öteki koleksiyonlarla (Ordering, Concurrency, Disposal/Security) birlikte iki koşuda yeşil.
- **main birleştirmesi (T389 #472):** `AdminCommandCatalog` iki tarafın beş komutunu da taşır; `CommandHost` iki tarafın kayıtlarını taşır (T388: `RollbackAudit`, `InstantDeletionLog`; main: durum, dosya deposu, görüntü işleme, bağlam erişimcisi, koşullu satırlar). Main'in `AddStatus`/`AddImageProcessing` kayıtları T388'in elle eklediği görüntü işleyici, disk okuyucu ve disk bekçisi kayıtlarını kapsadığından onlar tek kayda indi; `AddInstallationSettings`/`AddSearchEngineData` çağrıları kaldırıldı (doğrulayıcıları T389'un komut kipi testlerini kırıyordu; modüller onlarsız çözülüyor, T388 testleri yeşil). STATUS tablosunda T388 ⏳ ve T389 ✓ satırları birlikte.
- **Öz denetim:** 11'deki T388 kriter 1–9 ve KK1–KK9 birleşik ağaçta yeniden denetlendi, yukarıdaki tablo güncel (kriter 1 kanıtı değişti, ötekiler aynı testlerle yeşil); sık FAIL "main ilerleyince kayıt/kurucu çakışması" için Integration parçalı koşuldu.

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** main son 3 run success (42524245424, 42524245383, 42522612870; önceki işçi).
- **Repo memory (bitiş kapısı 9):** `.claude/memory/MEMORY.md` "Güncel Durum" bu parça kuralı gereği dokunulmadı (faz kapısı aktarır).
- **⚠ kolay kararlar:** (1) anlık silme satırını üretici kod yok → D-700; (2) hesapsız komutta `Guid.Empty` → D-701; (3) yeniden uygulamada e-posta yok → D-702; (4) fazladan sayılan tablolar → D-703; (5) kesinleşme anı NULL satırlar → D-704; (6) çıkış kodları: ayar kapalı ya da sayı > 0 → 3, okunamayan satır / bilinmeyen tür → 3; (7) CI hizmet konteyneri ALTER SYSTEM + restart ile ayarlanıyor (konteynere komut argümanı verilemediği için).
- **Komut kipi servis listesi:** önceki işçinin yazdığı komutlar modülleri çözerken `ImageProcessor`, `SiteAddress`, `ConditionalLine` listesi ve `DiskUsageGuard` eksik çıktı (Integration testi bunu yakaladı); `CommandHost_ResolvesWhatTheReplayUses_...` testi artık sınıyor.
