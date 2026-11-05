# T222 — Anahtarlar ve tek kullanımlık bağlantılar

**Faz:** F2 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Anahtar tek işlevden:** `Infrastructure/Security/SecretKey` — `Create()` `RandomNumberGenerator.GetBytes(32)` değerini Base64Url (dolgusuz) yapar, `Hash(key)` UTF-8 baytlarının SHA-256 özetini küçük harf onaltılık verir (`09 §15.1` m.1).
- **Bağlantı anahtarı türetme:** `Infrastructure/Security/LinkKeys` — `Derive(Guid)` = HMAC-SHA256(sır, UTF-8(kimlik, `N` biçimi)) Base64Url; `DigestFor(Guid)` = `SecretKey.Hash(Derive(id))` (anahtarı dışarı vermez). Sır `LinkKeyOptions.DerivationSecret`, ilk kullanımda okunur. `AddLinkRecords()` (`Program.cs`) kaydeder.
- **Bağlantı kaydı:** `LinkRecord.ForCustomer/ForAdmin/ForInvitation(LinkKeys, …)` aşırı yüklemeleri kimliği önce üretip özeti `DigestFor` ile yazar.
- **Hâl, okuma, tüketim:** `Infrastructure/Security/LinkRecords` — `StateOf` (kullanılmış > yenisi geçmiş > ömrü dolmuş > geçerli; `ExpiresAt` ≤ şimdi ömrü dolmuş), `ReadAsync` (izlemesiz, yazmaz; gelen anahtarın özetini arar, sırrı okumaz), `TryConsumeAsync` (tek koşullu `ExecuteUpdateAsync`, etkilenen satır 1 ise başarılı), `SupersedeEarlierAsync` (aynı tür ve sahibin önceki geçerli bağlantıları "yerine yenisi geçmiş").
- **Açılışta özet yeniden yazımı (kriter 8):** `Modules/Notifications/RevertLinkDigestRewrite` (tek işlemde, `ITransactionRunner`; "gönderilecek" `EmailChangeNotice` kaydının `EventValues`'ındaki `LinkRecordId` ile adlandırılan `EmailChangeRevert` bağlantısının özetini `DigestFor` ile yazar; aynıysa satıra yazmaz), `RevertLinkDigestRewriteStartup` (`IHostedService`; `NotificationsModule`'de `StartupDatabaseCheck`'ten sonra kayıtlı; DB'ye ulaşılamazsa atlar, `HasRun` false), `LinkEventValues` (EventValues girdisinin tek yazıcısı/okuyucusu).
- **Kaynak taraması:** `Architecture/LinkKeySourceScan` (üreteç/özet yalnız `Infrastructure/Security/` ve `Modules/Membership/`; zayıf kaynak yok; `Derive` yalnız `Modules/Notifications/` ve `LinkKeys.cs`; tüketim yalnız `OnPost…`) + ihlal örneği `ViolationSamples/LinkKeys/KeyMisuse.cs`.
- **Dokümanlar:** `09` §15.1 m.4 (açılış adımı, `DigestFor`, hâl/tüketim, `EventValues` girdisi) ve Test paragrafı; `.claude/CONTEXT.md` (IntegrationTests `Security/`, `LinkKeySourceScan`); status/memory; önceki task kayıtları (skill adım 15).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/`: `SecretKey.cs`, `LinkKeys.cs`, `LinkRecords.cs`, `LinkRecord.cs` (aşırı yüklemeler); `Program.cs`
- `src/Shopfolio.Web/Modules/Notifications/`: `RevertLinkDigestRewrite.cs`, `NotificationsModule.cs`
- Testler: `UnitTests/Infrastructure/` (`SecretKeyTests`, `LinkKeysTests`, `LinkRecordStateTests`), `UnitTests/Architecture/` (`LinkKeySourceScan(.cs, Tests.cs)`, `ViolationSamples/LinkKeys/KeyMisuse.cs`), `UnitTests/Consistency/SystemLogsDeployConsistencyTests.cs` (KK7 testi); `IntegrationTests/Security/` (`LinkRecordsTests`, `RevertLinkDigestRewriteTests`), `IntegrationTests/Web/RevertLinkDigestRewriteStartupTests.cs`
- Doküman: `Docs/09_CODING_GUIDELINES.md` §15.1, `.claude/CONTEXT.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, T225/T228 raporlarının "Post-merge run'lar" satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Anahtarlar OS kriptografik üreteciyle | ✓ | `SecretKey.cs` `Create`; `SecretKeyTests.Create_GivesAtLeast128Bits_…`; `LinkKeySourceScanTests.ApplicationSource_KeepsTheGeneratorInItsFolders_…` |
| 2 | Hesap bağlantısı anahtarı kimlikten HMAC ile türetilir | ✓ | `LinkKeys.Derive`; `LinkKeysTests.Derive_IsTheBase64UrlOfTheHmacOfTheIdWithoutHyphens`, `Derive_SameIdAndSecret_…`, `Derive_AnotherSecret_…` |
| 3 | Veritabanında yalnız özet; sipariş erişim anahtarı siparişte | ✓ | `LinkRecordsTests.Record_HoldsTheDigestOfTheDerivedKey_AndNoColumnHoldsTheKey`, `…_ForTheThreeOwners_EachHoldsItsOwnDigest`; `Order.OrderAccessKey` sütunu önceki task'ın şemasında (açık durur, `06 §2.3.3`) — bu task'ta değişmedi |
| 4 | Beş hâl bağlantı kaydından okunur | ✓ | `LinkRecordsTests.Read_GivesFiveDifferentResults_…`; `LinkRecordStateTests.StateOf_*` |
| 5 | GET kaydı değiştirmez, işlem POST'ta | ✓ | `LinkRecordsTests.Read_OfEveryState_LeavesTheLink…Unchanged_AndOnlyTheConsumingWritesTheLink` (xmin dahil görüntü, `ChangeTracker` boş); `LinkKeySourceScan` `ConsumeOutsidePost` kuralı. Sayfa yok (T342) — bkz. ⚠ 6 |
| 6 | Tüketim tek koşullu güncelleme | ✓ | `LinkRecords.TryConsumeAsync`; `TryConsume_OfAValidLink_TakesItOnce_…`, `…_OfAnExpiredAUsedAndASupersededLink…_ChangesNoRow`, `…_TwoRequestsAtOnce_OneSucceedsAndOneDoesNot` |
| 7 | Anahtarlı yol kayıtta maskeli | ✓ | `SystemLogsDeployConsistencyTests.KeyedPaths_CarryingRealKeys_AreMaskedTheSameByTheProxyAndTheApplication_AndNoLineHoldsTheKey` |
| 8 | Açılışta, kuyruk/e-posta başlamadan geri alma bildirimi özeti yeni sırla | ✓ | `RevertLinkDigestRewriteStartupTests.Start_WithANewSecret_WritesTheQueuedNoticesDigestBeforeAnyLaterServiceStarts_AndLeavesTheSentOnes`, `…_RegistersTheRewriteAfterTheTwoChecksAndBeforeEveryOtherServiceOfTheApplication` |

### Öz denetim (bitiş kapısı 2)

Devam işçisi önceki işçinin yaptıklarını dahil bütün maddeleri diff'te yeniden denetledi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–8 | ✓ | yukarıdaki tablo |
| KK1 üreteç kodda `RandomNumberGenerator` | ✓ | `SecretKey.cs` (`Create`); `LinkKeySourceScanTests.Scan_OnViolationSampleOutsideTheSecurityFolders_…` (ayrı ihlal örneği: `Random`/`Guid` → `Scan_OnViolationSampleInAGeneratorFolder_…`) |
| KK2 aynı kimlik+sır aynı anahtar; sır değişince değişir | ✓ | `LinkKeysTests.Derive_SameIdAndSecret_GiveTheSameKey_EvenFromAnotherInstance`, `Derive_AnotherSecret_GivesAnotherKey`, `Derive_AnotherId_GivesAnotherKey` |
| KK3 tabloda ham anahtar yok | ✓ | `LinkRecordsTests.Record_HoldsTheDigestOfTheDerivedKey_AndNoColumnHoldsTheKey` (satırın tamamı `to_jsonb` + bağlantı/gönderim/hesap/oturum tablolarının metni) |
| KK4 beş hâl ayrı sonuç | ✓ | `Read_GivesFiveDifferentResults_ForNoRecordExpiredUsedSupersededAndValid` (beş sonuç ayrı, `Distinct` sayısı eşit); `StateOf_TheFiveOutcomesWithTheMissingReading_AreAllDifferent` |
| KK5 GET sonrası bağlantı/hesap/sipariş değişmez; yalnız POST | ✓ | `Read_OfEveryState_…` (link_records, customers, admins, sessions, orders, dispatch_records satır değerleri + `xmin`); sayfa T342'de (⚠ 6) |
| KK6 ikinci tüketim, ömrü dolmuş, yenisi geçmiş sıfır satır | ✓ | `TryConsume_OfAValidLink_TakesItOnce_AndASecondConsumingChangesNoRow`, `TryConsume_OfAnExpiredAUsedAndASupersededLinkAndOfNoRecord_ChangesNoRow`, `TryConsume_OfALinkWhoseLifetimeIsDrawnToNow_IsRefused`, `SupersedeEarlier_…` |
| KK7 kayıtta anahtarlı yol maskeli | ✓ | `KeyedPaths_CarryingRealKeys_…` (gerçek türetilmiş ve üretilmiş anahtarlar, `-`/`_` içerenler, sorgu ve çift eğik çizgi karışık örnekler; Caddy süzgeci ile `PathMasker` çıktısı aynı örnek kümesinde eşit) |
| KK8 açılışta kuyruk işlenmeden özet yeni sırla; gönderilmişin kaydı değişmez | ✓ | `RevertLinkDigestRewriteStartupTests.Start_WithANewSecret_…` (kuyruğu temsil eden en sona kayıtlı `IHostedService` ilk çağrıda yeni özeti görür, gönderilmişin özeti eski); `RevertLinkDigestRewriteTests.Run_LeavesTheLinkOfASentNoticeAsItIs_RowVersionIncluded`, `…_LeavesAQueuedNoticeOfAnotherType…`, `…_WithTheSameSecret_RewritesNothing_AndNoRowVersionMoves`, `…_ForAnAdminAccount_…`, `…_WithManyPendingNotices_IsOneTransaction_…` |
| `09 §25` Doküman uyumu | ✓ | adlar `06 §4.42/4.58`'den (`LinkRecord`, `DispatchRecord.EventValues`); diff yalnız kriterlerin istediği; yeni ayar/sabit/süre yok (`LinkKeyOptions` önceden vardı, `.env.example` değişmedi) |
| `09 §25` Mimari ve kod | ✓ | yeni dosyalar `§4.1` düzeninde; yeni klasör yalnız testte (`IntegrationTests/Security/`) → `CONTEXT.md` güncel; modül sınırı: bağlantı kaydı altyapıda, bildirim modülü yalnız `LinkKeys`/`LinkRecord` kullanır; işlem işi lambda (`RewriteAsync`; `DatabaseSessionSourceScanTests.ApplicationSource_UsesNoOutsideServiceInsideATransactionBody` yeşil) |
| `09 §25` Durum/olay/işlem izi, Eşzamanlılık | ✓ | tüketim ve öncekini geçersiz kılma koşullu güncelleme (`§10.2`); durum geçişi/denetim kaydı yok (kullanım hâli bağlantı kaydının kendi alanı) |
| `09 §25` Veri ve göç | uygulanamaz | şema değişmedi; model anlık görüntüsü aynı (06 "Varsayılan" DB default'u etkilenmedi) |
| `09 §25` Güvenlik — anahtar `SecretKey`'den | ✓ | `Random`/`Guid.NewGuid`/`CreateVersion7` ile anahtar yok (`LinkKeySourceScan.WeakKeySource` + ihlal örneği); `Derive` yalnız Notifications (`DeriveOutsideNotifications` + ihlal örneği) |
| `09 §25` Güvenlik — sır ve kişisel veri kayda girmez | ✓ | yeni log olayı yok; açılış adımı hata metnini yazmaz; `KK7` ve `KK3` testleri |
| `09 §25` Test | ✓ | `09 §15.1` Test paragrafı karşılandı; her yeni tarama kuralının ayrı ihlal örneği var (`KeyMisuse.cs`: üreteç, zayıf kaynak, `Derive`, tüketim) |
| Güvenlik taraması | ✓ | sır sızıntısı yok (test sırları çalışma anında üretilir; pre-commit bekçisi geçti); auth/yetki yüzeyi yeni yok (sayfa yok); girdi doğrulama: `ReadAsync` boş anahtarı kayıt yok sayar, `SecretKey.Hash` boş anahtarı reddeder, `LinkEventValues` bozuk girdiyi yok sayar; SQL parametreli (EF); yeni dış bağımlılık yok (`System.Buffers.Text.Base64Url` çatıyla) |
| Aynı şeyi anan öteki yerler | ✓ | `09 §15.1` m.4 + Test; runbook §B satır 15 ve K-1081 satırı "açılışta yeniden yazar" zaten bu davranışı söyler (kontrol edildi, değişiklik gerekmedi); `CONTEXT.md`; status/memory; `README` bu konuyu anmaz |
| Bekçi/tarama istisnası dar | ✓ | `GeneratorExceptions` boş; `LinkKeys.cs` yalnız `Derive` kuralından, `LinkRecords.cs` yalnız tüketim kuralından muaf (`Scan_InTheTwoNamedFiles_AllowsTheirOwnRuleOnly`); çok satırlı çağrı biçimi bulunur |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Script | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 dosya |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release --filter-not-trait "Consistency=true"` ve `--filter-trait "Consistency=true"` | 794/794 + 138/138 = 932 |
| Integration | ✓ (⚠ 7) | `dotnet test --project tests/Shopfolio.IntegrationTests/... -c Release` (ad alanı filtreleriyle bölümlü) | Security+Concurrency 44/44 (yeni 18), Database+FileSecrets+Logging+Model 982/982, Web (Liveness hariç) 51/51, Web `LivenessEndpointTests` 15/16 (1 yerel ortam düşüşü, ⚠ 7); toplam 1093, yeni test 21 (LinkRecordsTests 11, RevertLinkDigestRewriteTests 7, RevertLinkDigestRewriteStartupTests 3) |
| Contract | uygulanamaz | — | `CONTRACT_TEST_CMD=NOT_APPLICABLE` |
| Build / Lint | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata, biçim temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-05, bağımsız doğrulayıcı; dal head `fcd3009`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1, K2 → `DEFERRED_BACKLOG` D-96 |

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Bütün anahtarlar OS kriptografik üreteciyle (KK1) | ✓ | `SecretKey.Create` = `Base64Url(RandomNumberGenerator.GetBytes(32))` (256 bit); türetme sırrı kurulumda üreteçle (`DEPLOY_RUNBOOK §B` satır 15, T08). `LinkKeySourceScan`: `RandomNumberGenerator`/`SHA256`/`HMACSHA256` yalnız `Infrastructure/Security/` ve `Modules/Membership/`'te; bu iki klasörde `new Random(`, `Random.Shared`, `Guid.NewGuid(`, `Guid.CreateVersion7(` bulgu; istisna listesi boş; ihlal örneği `KeyMisuse.cs` her kuralı ayrı satırla tetikler |
| 2 | Bağlantı anahtarı kimlikten HMAC ile türetilir (KK2) | ✓ | `LinkKeys.Derive` = `Base64Url(HMACSHA256.HashData(secret, UTF8(id.ToString("N"))))` (`09 §15.1` m.4 ile birebir); sır ilk kullanımda okunur (açılış denetiminden önce atmaz). `LinkKeysTests`: aynı kimlik+sır başka örnekten aynı anahtar, başka sır/başka kimlik başka anahtar, değer elle hesaplanan HMAC'e eşit |
| 3 | Veritabanında yalnız özet, bağlantı kaydında; sipariş erişim anahtarı siparişte (KK3) | ✓ | `LinkRecord.For…(LinkKeys, …)` kimliği önce üretir, `TokenHash = DigestFor(Id)`; `DigestFor` anahtarı döndürmez. `Record_HoldsTheDigestOfTheDerivedKey_AndNoColumnHoldsTheKey` satırın `to_jsonb`'si ve bütün veritabanı metninde ham anahtar yok; üç sahip için ayrı özet. Sipariş erişim anahtarı şeması önceki task'ta, değişmedi |
| 4 | Hâl bağlantı kaydından: beş sonuç (KK4) | ✓ | `LinkReading.Missing` + `LinkState` 4 değeri; `StateOf` öncelik kullanılmış > yenisi geçmiş > ömrü dolmuş (`ExpiresAt` ≤ şimdi) > geçerli. `Read_GivesFiveDifferentResults_…` (gerçek PG), `StateOf_TheFiveOutcomes…_AreAllDifferent`, `StateOf_TheLifetimeEndsAtItsMoment` |
| 5 | GET hiçbir kaydı değiştirmez; işlem POST'ta (KK5) | ✓ | `ReadAsync` `AsNoTracking().SingleOrDefaultAsync`, yazma yok, sırrı okumaz. `Read_OfEveryState_…` her hâlde okumadan sonra `ChangeTracker` boş ve `link_records`, `customers`, `admins`, `sessions`, `orders`, `dispatch_records` satırları `xmin` dahil aynı; ardından tüketim görüntüyü değiştirir. `LinkKeySourceScan` `ConsumeOutsidePost`: `TryConsumeAsync` yalnız `OnPost…` gövdesinde (yerel işlev ve çok satırlı çağrı dahil). Sayfa yok — sayfa düzeyi (`VAL-A616`, `A677`) T342 ve hesap task'larında |
| 6 | Tüketim tek koşullu güncelleme; satır yoksa işlem yok (KK6) | ✓ | `TryConsumeAsync` tek `ExecuteUpdateAsync`, koşul `UsedAt == null && SupersededAt == null && ExpiresAt > now`, sonuç `changed == 1`; çağıranın işlemine katılır (`…_InsideTheTransactionRunner_JoinsTheCallersTransaction_…`). İkinci tüketim, ömrü dolmuş, kullanılmış, yenisi geçmiş, ömrü şimdiye çekilmiş ve kaydı olmayan bağlantı 0 satır (görüntü aynı); eşzamanlı iki istek 15 turda her seferinde tam biri başarılı (`Barrier`, gerçek PG 18) |
| 7 | Anahtarlı yol sistem kayıtlarında maskeli (KK7) | ✓ | `KeyedPaths_CarryingRealKeys_…`: `PathMasker.KeyedPaths` her yolu için gerçek türetilmiş ve üretilmiş anahtarlar (`-`/`_` içerenler dahil) yol, sorgu, çift eğik çizgi ve büyük harf biçimlerinde; Caddyfile süzgeçleri ile `PathMasker.Mask` aynı çıktıyı verir ve hiçbirinde anahtar kalmaz. Maskeleme katmanı T225/T231'de (`VAL-A991` PASS) |
| 8 | Açılışta kuyruk ve e-postadan önce "gönderilecek" geri alma bildiriminin özeti o anki sırla (KK8) | ✓ | `RevertLinkDigestRewrite` `Queued` `EmailChangeNotice` satırlarının `EventValues`'ındaki `LinkRecordId`'leri okur, yalnız `EmailChangeRevert` bağlantısını `DigestFor` ile yazar, aynıysa yazmaz; tek `ITransactionRunner` işlemi, iş lambda (`09 §10.7`). `RevertLinkDigestRewriteStartup` `NotificationsModule`'de, `Program.cs`'te iki açılış denetiminden sonra kayıtlı; uygulamanın öteki bütün barındırılan hizmetleri ondan sonra (`Start_RegistersTheRewrite…`). `Start_WithANewSecret_…`: yeni sırla açılan uygulamada en sona kayıtlı sonda hizmeti ilk çağrısında yeni özeti görür, gönderilmiş bildirimin özeti eski; `RevertLinkDigestRewriteTests` gönderilmiş, öteki tür, öteki bağlantı türü, bozuk girdi ve aynı sırda satır sürümü oynamaz; yönetici hesabı da yazılır |

**Güvenlik sınaması (talimatın ayrıca istediği):** (a) üreteç: bütün anahtar yolları `RandomNumberGenerator`'dan; zayıf kaynak taramayla kapalı. (b) ham anahtar sızıntısı: yeni kodda kayıt satırı yok; `SecretKey.Hash` boş anahtarda yalnız parametre adıyla atar; `LinkKeys` sır çözme hatası değeri taşımaz; açılış adımı Npgsql istisnasını yazmaz; veritabanında yalnız özet (KK3 testi bütün veritabanı metnini tarar). (c) HMAC: girdi kimliğin `N` biçimi UTF-8, anahtar sırrın Base64 çözümü; testteki elle hesap eşit. (d) yarış: tek koşullu güncelleme, eşzamanlı testte tek başarı. (e) GET yan etkisizliği: salt okuma + `xmin` görüntüsü. (f) açılış sırası: kayıt sırası testi + sonda hizmeti. Açık yol bulunmadı; iki kenar K1–K2'dir.

**`09` değişikliği (`§15.1` m.4 ve Test paragrafı):** kodla birebir ve `05 §6.4`, §6.x.20 ile çelişmiyor — `DigestFor`, `ReadAsync`/`TryConsumeAsync`/`SupersedeEarlierAsync`, `EventValues` girdisi ve açılış adımının kaydı ekleniyor; önceki kurallar daraltılmadı.

**Task satırları (`12 §7.5`):** task'ın kaynakları `05 §6.4`, §6.x ve `09 §15`'tir (KP değil); KP grep'i satır getirmez. İlgili satırlar `VAL-A616` (POST'ta tek koşullu tüketim) ve `VAL-A677` (tutmayan tüketim hâli gösterir) uç düzeyindedir ve sayfaları ister (T342, hesap task'ları) — `Durum` `BEKLEMEDE` kaldı; `VAL-A991` T225'te PASS, değişmedi. `12_matris_denetle.py --kapsam 1-77`: Hata 0.

**K-notları:** (K1) Açılışta veritabanına ulaşılamazsa adım atlanır ve `HasRun` yanlış kalır; kuyruğun bu bekçiye bakması yalnız `09 §15.1` m.4'te yazılı, T220 bloğu `09 §15`'i anmıyor → D-96. (K2) `SupersedeEarlierAsync` iki sahip boşken (davet türü) bütün öteki davet bağlantılarını geçersiz kılar; `05 §6.4.6`'da davette yenileme ömrü çekmektir → D-96. İkisinin de bugün çağıranı yok.

Testler (validator, yerel, head `fcd3009`): build `-warnaserror` 0/0 · `dotnet format --verify-no-changes` 0 · Unit 932/932 (Consistency dahil) · betik testleri 4/4 · Integration 1093/1093 (Security 18, Web 67 — `LivenessEndpointTests` dahil yeşil, Database+Concurrency+FileSecrets+Logging+kök 115, Model 482 + 411; PG 18 konteyner `shopfolio-t222-val-pg`) · koşu sonrası `git status` temiz, `dotnet restore --locked-mode` 0 · dal CI 41977809448 success (head `fcd3009`) · ana dal son 3 run 41973187361, 41973187267, 41969409700 success.

**Yapım raporu karşılaştırması:** kriter ve öz denetim tabloları her kriter ve KK için kanıtlı; verdict uyumlu. Uyuşmazlık yok. ⚠ 7'deki yerel `Liveness…` düşüşü validator koşusunda görülmedi (Web 67/67). ⚠ 3 (`HasRun`) K1 olarak backlog'a bağlandı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T222-anahtarlar-baglantilar`
- **PR:** #392
- **Commit:** son commit PR başlığında (`T222: Anahtarlar ve tek kullanımlık bağlantılar`) ve dal başında
- **CI:** dal CI 41977809448 success (head `fcd3009`)
- **Post-merge run'lar:** squash `11fcfb3` (#392) — CI 41981392945 success, Publish 41981392743 success (T206 dalında dolduruldu, skill adım 15)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Konak hosted service'leri kayıt sırasıyla başlatır (rewrite sırası buna dayanır) | `RevertLinkDigestRewriteStartupTests` — en sona kayıtlı sonda hizmeti yeni özeti görür; PG18 yerel konteyner | ✓ |
| 2 | `jsonb` sütunu geçerli JSON ister (bozuk `EventValues` testleri geçerli ama yapısı yanlış JSON kullanır) | `RevertLinkDigestRewriteTests` | ✓ |

Başka dış varsayım yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Anahtarlı sayfaların GET/POST davranışı (sayfalar) | — (T342 kapsamı) | Sayfa yok; bu task katmanı ve kaynak taramasını kurar |

Bu task'ın hedefi olan backlog kaydı yok (`grep -n T222 Docs/DEFERRED_BACKLOG.md` boş); yeni D kaydı açılmadı.

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; dal `origin/task/T222-anahtarlar-baglantilar` head `9cd5bdd`'den sürdürüldü)
- **Ana dal CI startup (Adım 0):** main `36b586f` — CI 41973187267 success, Publish 41973187361 success; önceki run'lar 41969409791, 41969409700 success
- **Repo memory (bitiş kapısı 9):** güncellendi (Güncel Durum'da T222 satırı)
- **Önceki task kaydı (skill adım 15):** T228 status Commit hücresi `36b586f` yapıldı; T225 ve T228 raporlarının "Post-merge run'lar" satırı dolduruldu
- **Yerel entegrasyon notu:** `LivenessEndpointTests.Get_WhenDatabaseFailsBeforeTheLimit_Returns503AndWritesNoTimeoutLine` bu makinede yerel Docker PG'ye karşı hem bu dalda hem açılış adımı devre dışı bırakılarak da `DbTimeout` uyarısı görüp düşüyor (bu task'ın değişikliğinden bağımsız, ortam gecikmesi); CI'da sınanır (⚠ 7)
- **⚠ Kararlar (kurala en uygun seçenek):**
  1. ⚠ Hâl önceliği kullanılmış > yenisi geçmiş > ömrü dolmuş > geçerli; ömür sonunun anında bağlantı ömrü dolmuştur (`ExpiresAt` ≤ şimdi) — "ömrü şimdiye çekilen" bağlantı (`05 §6.x.20`) hemen geçersiz olur.
  2. ⚠ `EventValues` biçimi `[{"Name":"LinkRecordId","Value":"<kimlik, D biçimi>"}]` (yük biçimi henüz yok; yazıcı ve okuyucu tek yerde `LinkEventValues`). E-posta değişikliğini yazan ileri task bunu kullanmalıdır.
  3. ⚠ Açılış adımı veritabanına ulaşılamazsa atlanır ve `HasRun` false kalır (mevcut liveness 503 davranışı bozulmasın); kuyruğu ya da e-postayı başlatan ileri task `HasRun`'a bakmalıdır. Not: ulaşılamayan sunucuda bağlantı denemesi `StartupDatabaseCheck`'inkine eklenir.
  4. ⚠ Yeni log olayı eklenmedi (`05 §9.1` listesi kapalı).
  5. ⚠ `LinkKeys.Derive` yalnız Notifications'tan çağrılır; kayıt oluşturma `DigestFor` kullanır (`09 §15.1` m.4 ile uyumlu, belgeye yazıldı).
  6. ⚠ Sayfa olmadığı için GET/POST kanıtı katman düzeyindedir: `ReadAsync` salt okuma testi ve `TryConsumeAsync`'in yalnız `OnPost…` işleyicisinden çağrılmasını zorlayan kaynak taraması; sayfa testi T342'de.
  7. ⚠ Yukarıdaki `Liveness…Returns503AndWritesNoTimeoutLine` yerel düşüşü ortam kaynaklıdır (task kodundan bağımsız kanıtlandı); CI sonucu yardımcıda izlenir.
  8. ⚠ Açılış adımı işi `RunAsync` adlı bir yöntemle değil `RewriteAsync` adıyla sunar: `TransactionBodySourceScan` her `.RunAsync(` çağrısının ilk argümanının lambda olmasını ister (D-82); iş de lambda olarak çağrı yerinde yazıldı.
