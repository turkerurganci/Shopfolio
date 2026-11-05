# T222 — Anahtarlar ve tek kullanımlık bağlantılar

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-05

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
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T222-anahtarlar-baglantilar`
- **PR:** #392
- **Commit:** son commit PR başlığında (`T222: Anahtarlar ve tek kullanımlık bağlantılar`) ve dal başında
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur)*

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
