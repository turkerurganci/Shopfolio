# T345 — Hesabın silinmesi ve kişisel veri başvurusu

**Faz:** F3 | **Durum:** Düzeltme 1 tamam (doğrulama tur 2 bekliyor) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `IMembershipModule`: `DeleteAccountAsync` (üyenin kendi silmesi, yeniden doğrulama değeriyle), `DeleteAccountOnRequestAsync` (yönetici, talep üzerine; iz satırı yalnız işlemi ve tarihi adlandırır), `FindMemberRecordAsync` (düzenlemesiz üye kaydı görünümü), `GetDeletionBlockedUntilAsync` (açık geri alma bağlantısı bitişi).
- `AccountDeletions`: hesap satırı yazıyla tutulur, geri alma bağlantısı kapısı, yeniden doğrulama değeri tüketimi, siparişlerin işaretlenmesi, tek silme e-postası, silmeyle düşen satırların imha kaydı (tür ve sayı).
- `OrderingModule.MarkAccountDeletedAsync`: siparişler kilitlenip izlenen satırlar olarak işaretlenir (`IsAccountDeleted`, `CustomerId` boş; sürüm oynamaz); toplu yazma yok, tarama istisnası açılmadı.
- `SessionStore/ISessionService.TryRedeemReauthenticationHeldAsync`: hesap satırını kendi yazısıyla tutan çağıran için Account kilidi ailesini istemeyen alma (Account kilidi tek başına alınır, `05 §7.3.5`; sipariş kilitleri ondan sonra gelemez, ters sıra da yasak). Kilit sırası kuralı gevşetilmedi.
- `MemberRecord`: enum yerine `Exists` + `IsAccount` (enum 06'da ve kilit dosyasında tanımlı olmadığı için tablo/kilit tutarlılık testleri düşüyordu).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/` (AccountDeletions, MemberRecords, MembershipModule, SessionService, Contracts), `Modules/Ordering/OrderingModule.cs`, `Infrastructure/Security/SessionStore.cs`.
- Testler: `MembershipAccountDeletionTests.cs`, `MembershipTests.cs`, `AddressTests.cs` düzenekleri.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ad, giriş bilgisi, oturum, defter, sepet silinir; işaret, bekleyen değişiklik düşer; imha kaydı | ✓ | `DeleteOwn_RemovesTheAccountAndWhatHangsOnIt_AndTheDisposalRecordCountsThem_KK1` |
| 2 | Sipariş kişisel verisi saklama süresine kadar durur | ✓ | `DeleteOwn_LeavesTheOrdersWithTheirOwnRecord_MarkedDeleted_WithoutRaisingTheVersion_KK2_KK4`; süre sonu imhası mevcut `OrderDisposalPart` |
| 3 | Silme e-postası bir kez | ✓ | `DeleteOwn_QueuesTheNoticeOnce_ToTheAccountsAddress_KK3` |
| 4 | Yürüyen sipariş silmeyi engellemez | ✓ | KK2_KK4 testi |
| 5 | Silinmiş hesabın siparişi bağlanmaz | ✓ | `ANewAccountOfTheSameAddress_DoesNotTakeTheDeletedAccountsOrder_KK5`; eşzamanlı: `AnOrderInsertedWhileTheAccountIsBeingDeleted_IsWaitedFor_AndEndsMarked_KK5`, `AnOrderInsertedAfterTheDeletionHoldsTheAccount_WaitsAndFails_SoNoUnmarkedOrderExists_KK5` (iki oturum, gerçek PostgreSQL; düzeltme 1) |
| 6 | KVKK talebi ayrı modül/durum makinesi/sayaç yok | ✓ | `ThePersonalDataRequest_HasNoTableStateMachineOrExportOfItsOwn_KK6_KK7` |
| 7 | Veri indirme yok; üye kaydı düzenlemesiz | ✓ | `TheMemberRecordView_ReadsTheAccount_AndWritesNothing_KK7` |
| 8 | Yönetici silmesi aynı kural, iz satırı hesabı adlandırmaz | ✓ | `DeleteOnRequest_GivesTheSameResult_AndWritesATrailRowThatNamesNothingOfTheAccount_KK8`; `DeleteOnRequest_WithoutAnActor_Throws_...` |
| 9 | Açık geri alma bağlantısında silme reddedilir, bitiş tarihi söylenir | ✓ | `AnOpenRevertLink_ClosesBothDeletions_NothingIsDeleted_AndTheReauthenticationValueIsNotSpent_KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1–KK9 (yukarıdaki tablo) | ✓ | MembershipTests sınıfı 108 test yeşil |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | İstisna açılmadı; `VersionedSideBulkWriteScanTests` değişmedi, yeşil |
| Sık FAIL 2: tarama çağrı biçimi kaçırma | ✓ | Yeni toplu yazma yok; tarama kümesi aynı |
| Sık FAIL 3: DB default / FK dizini | ✓ | Şema/göç değişikliği yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | Enum kaldırıldı (`EnumSourceScanTests`, `EnumTableConsistencyTests`, `EnumLockTests` yeşil); `ISessionService`'e yöntem eklendi, tüm elle kurulan düzenekler derlendi |
| Sık FAIL 5: belge kuralı değiştiren 09 metni karar kaydısız | ✓ | 09/05/02 metni değişmedi; kilit sırası kuralı olduğu gibi |
| Kaynak tutarlılığı: modül işlemi açmaz, SaveChanges çağırmaz | ✓ | `AccountDeletions.RequireTransaction`, `MarkAccountDeletedAsync` işlem kontrolü |
| Elle kuran test düzenekleri | ✓ | Integration TAM (parçalar halinde) yeşil |
| `dotnet format` | ✓ | Web, UnitTests, IntegrationTests `--verify-no-changes` temiz |

### Düzeltme 1 (doğrulama tur 1 FAIL; B1) — öz denetim

Düzeltme: `AccountDeletions.HoldAndCheckAsync` hesabın satırını `AccountLocks.LockAsync` ile `SELECT … FOR UPDATE` kilitler (`09 §15.1` m.3; `05 §6.1.2` K-1084); kuru `UPDATE` (`FOR NO KEY UPDATE`) kalktı. `LockSequence`: `LockKind.Account` artık sepet danışma kilidi gibi sıranın başıdır — hesap, sonra sipariş satırları kimliğe göre, varyant, kupon; hesap kilidi başka kilidin ardından ve sepet/ayrı ailelerle birlikte istenemez, giriş ve şifre akışları yine yalnız hesap kilidini alır. Belgeyle çelişki yok: `09 §15.1` m.3 hesabı silen işlemi `AccountLocks` kullanıcısı sayar, `05 §7.3.5` hesap kilidini anmaz (metin eksiği D-664). `main` birleşti: `MembershipModule.cs` çakışmasında iki taraf (`AccountDeletions`, `MemberRecords` + `IAdminPendingEmailChanges`) korundu; test düzeneğinde main'in `SwitchableAuditActor`'ı tek kaynak (T345'in `AuditActorHolder` düzeneği kaldırıldı).

| Madde | Sonuç | Kanıt |
|---|---|---|
| K1 silmeden sonra hesap ve ona bağlı satırlar yok, imha kaydı | ✓ | KK1 testi yeşil (Membership 142/142 birleşik ağaçta) |
| K2 sipariş kişisel verisi durur | ✓ | KK2_KK4 testi |
| K3 e-posta bir kez | ✓ | KK3 testi |
| K4 yürüyen sipariş engellemez | ✓ | KK2_KK4 testi |
| K5 silinmiş hesabın siparişi bağlanmaz (eşzamanlı dahil) | ✓ | sıralı KK5 + iki yeni iki-oturumlu yarış testi: sipariş ya işaretli ya hiç oluşmamış |
| K6, K7 | ✓ | KK6_KK7, KK7 testleri |
| K8 yönetici silmesi, iz satırı | ✓ | KK8 testleri (main'in aktör düzeneğiyle) |
| K9 geri alma bağlantısı kapısı | ✓ | KK9 testi |
| `09 §25`: kod hiçbir doküman cümlesiyle çelişmiyor; eşzamanlılık doğru | ✓ | `09 §15.1` m.3 uygulanıyor; kilit sırası birim testleri (Account → Order/Variant/Coupon izinli; Order/Cart/ayrı aileler → Account ve Account → Cart/CompanySettings yasak); `ConcurrencySourceScan` yeşil |
| Sık FAIL: eşzamanlılık belge kilidinden zayıf, yarış testi yok (T345 r1) | ✓ | `FOR UPDATE` + iki yarış testi |
| Sık FAIL: kilit sırası tutarsız (ters sıralı iki akış) | ✓ | sıra tek: hesap → sipariş; olası onay↔silme ölü kilidi D-665'e yazıldı |
| Sık FAIL: kod birleştirmesinde iki yol / main'in yeni zorunlu alanı / elle kurulan düzenekler | ✓ | birleşik ağaçta Unit 5008, Integration parçaları yeşil |
| Sık FAIL: format test dosyasında koşulmadan "temiz" | ✓ | Web, UnitTests, IntegrationTests `--verify-no-changes` çıkış 0 |
| Çakışma işareti / durum | ✓ | `git diff --name-only --diff-filter=U` boş, `git grep` işaret boş, `git status` temiz |

## Test Sonuçları (düzeltme 1)

Birleşik ağaçta: build `-warnaserror` 0 uyarı; Unit 5008/5008; Integration `MembershipTests` + `AddressTests` 142/142; `ConcurrencyTests` + `OperationPatternTests` + `OrderTransitionsTests` 70/70 (postgres:18, `shopfolio-t345-fix-pg`).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release` | 4934 geçti, 0 kaldı |
| Integration | ✓ | `MembershipTests` sınıfı + `--filter-not-class "*MembershipTests"` (docker `t345`, postgres:18) | Membership 108/108; öteki sınıflarda yüke bağlı zaman aşımı (5 migrasyon/başlangıç/çerez testi) tek tek koşulunca yeşil (CatalogCore 60, Scheduler 55, StartupDatabaseCheck 18, CookiePolicy 11, AdminAndAudit 1) |
| Contract | — | — | Bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-08, head `7f842037`) |
| Bulgu sayısı | 1 (B1 ORTA S1) |
| Düzeltme gerekli mi | Evet — B1 bu dalda düzeltilir, yeni doğrulama turu |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-652) |

**Validator kabul kriterleri tablosu:** K1 ✓ · K2 ✓ · K3 ✓ · K4 ✓ · K5 ✗ (B1: eşzamanlı onaylanan sipariş işaretin dışında kalır) · K6 ✓ · K7 ✓ · K8 ✓ · K9 ✓.

- **B1 (ORTA, S1) — silmenin hesap satırı kilidi eşzamanlı onaylanan siparişi işarete katmıyor.** `AccountDeletions.HoldAndCheckAsync` hesabın satırını `ExecuteUpdateAsync(set => set.SetProperty(c => c.Name, c => c.Name))` ile tutar; anahtar sütunu değiştirmeyen `UPDATE` PostgreSQL'de `FOR NO KEY UPDATE` kilidi alır ve bu, `orders.customer_id` yabancı anahtarının ekleme denetimindeki `FOR KEY SHARE` kilidiyle çakışmaz. Silme sürerken aynı hesaba eklenen ve `MarkAccountDeletedAsync`'in aday sorgusunda henüz kesinleşmemiş sipariş işaretlenmez; hesabın silinmesi `ON DELETE SET NULL` ile `customer_id`'yi boşaltır, `is_account_deleted` `false` kalır ve sipariş `LinkGuestOrdersAsync`'in koşuluna (`CustomerId == null && !IsAccountDeleted`) girer — aynı adresle açılan yeni hesaba bağlanır. Beklenen: `05 §6.1.2` (K-1084) "hesabı silen işlem önce hesabın satırını güncelleme kilidiyle okur … eşzamanlı onaylanan sipariş işaretin dışında kalmaz"; `09 §15.1` madde 3: hesabı silen işlem `AccountLocks` ile `FOR UPDATE`; `02 §3.15.3`. Kanıt: PostgreSQL 18'de iki oturum (A: kuru `UPDATE` → 3 sn → aday sayımı → işaret → `DELETE`; B: 1. sn'de `INSERT` siparişi, 4. sn'de `COMMIT`) → A'nın aday sayımı 0, sonuç `customer_id` boş, `is_account_deleted = f`; aynı akış `SELECT … FOR UPDATE` ile koşulunca B'nin eklemesi bekler ve yabancı anahtar hatasıyla düşer, kaçan sipariş yok. Yapım raporu bu yarış için test yazılmadığını not etmiş. Kodda `LockSequence` `LockKind.Account`'u tek başına alınan kilit sayar ve `09 §15.1` madde 3 ile çelişir; çelişki `09 §25` gereği bildirilmeden daha zayıf bir kilitle çözülmüş. Düzeltme yönü: silmede `FOR KEY SHARE` ile çakışan kilit (`AccountLocks` `FOR UPDATE`; `LockSequence`'in hesap → sipariş sırasına izni ya da belge kararıyla eşdeğeri) ve eşzamanlı onay + silme Integration testi.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T345-hesap-silme-kvkk`
- **PR:** açılışta eklenir
- **CI:** yoklanmadı (yönetici izler)
- **Post-merge run'lar:** doğrulayıcı doldurur

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Panel üye kaydı görünümünde bağlı sipariş listesi | D-605 | Sayfa task'ı (UC-95) |
| 2 | Silme sayfaları, sebep ve bitiş tarihi, geri alınamaz onay, sonuç çerezi | D-606 | Sayfa task'ları (UC-68/69/157) |
| 3 | 07 UC-68/157/95 sözleşme metnine yeni modül yüzeyi | D-607 | Belge işi |
| 4 | Customer dışı satırların imha kaydı türleri 05 §7.4.2/06 §4.64'te sayılmıyor | D-608 | Belge kararı gerekir |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 42506023683, 42506023800, 42500144509 — hepsi success
- **Repo memory (bitiş kapısı 9):** güncellenmedi (parça dosyası kuralı; MEMORY "Güncel Durum" dokunulmaz)
- **⚠ Kolay kararlar:** yönetici silmesi doğrulanmamış kaydı bulamaz (NotFound); sepet danışma kilidi alınmaz; yeniden doğrulama değeri geri alma bağlantısı reddinde harcanmaz; eşzamanlı onaylanan sipariş + silme yarışı (düzeltme 1'de iki oturumlu test eklendi); `TryRedeemReauthenticationHeldAsync` yalnız silme akışı içindir (hesap kilidi bu akışta zaten tutulur). Düzeltme 1: hesap kilidi sıranın başıdır (D-664 belge, D-665 olası ölü kilit).
- **Hook:** kurulu
