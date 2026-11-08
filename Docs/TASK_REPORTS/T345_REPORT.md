# T345 — Hesabın silinmesi ve kişisel veri başvurusu

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama bekliyor) | **Tarih:** 2026-11-08

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
| 5 | Silinmiş hesabın siparişi bağlanmaz | ✓ | `ANewAccountOfTheSameAddress_DoesNotTakeTheDeletedAccountsOrder_KK5` |
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

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests -c Release` | 4934 geçti, 0 kaldı |
| Integration | ✓ | `MembershipTests` sınıfı + `--filter-not-class "*MembershipTests"` (docker `t345`, postgres:18) | Membership 108/108; öteki sınıflarda yüke bağlı zaman aşımı (5 migrasyon/başlangıç/çerez testi) tek tek koşulunca yeşil (CatalogCore 60, Scheduler 55, StartupDatabaseCheck 18, CookiePolicy 11, AdminAndAudit 1) |
| Contract | — | — | Bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | doğrulayıcı doldurur |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

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
- **⚠ Kolay kararlar:** yönetici silmesi doğrulanmamış kaydı bulamaz (NotFound); sepet danışma kilidi alınmaz; yeniden doğrulama değeri geri alma bağlantısı reddinde harcanmaz; eşzamanlı onaylanan sipariş + silme yarışı için test yazılmadı; Account kilidini istemeyen `TryRedeemReauthenticationHeldAsync` yalnız hesap satırını yazıyla tutan silme akışı içindir.
- **Hook:** kurulu
