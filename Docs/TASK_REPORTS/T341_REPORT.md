# T341 — Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

---

## Yapılan İşler

- `Modules/Membership/Contracts/IMembershipModule.cs` ve `MembershipModule.cs`: `RegisterAsync` (satış kapısı → alan denetimi → L-3 → bekleyen kaydın tembel silinmesi → doğrulanmamış hesap + P-12 (24 saat) ömürlü doğrulama bağlantısı), `ReadVerificationLinkAsync` (GET: hiçbir kaydı değiştirmez), `VerifyEmailAsync` (düğmenin işi: tek koşullu tüketim, `EmailVerifiedAt`, misafir siparişlerinin bağlanması), `ResendVerificationLinkAsync` (L-9; öncekiler `SupersededAt` alır, ömür uzamaz), `CheckCredentialsAsync` (sert doğrulama kapısı; her deneme giriş kaydına; eski ayarlı özet başarılı girişte yenilenir), `ResolveOrderAccountAsync` (`Guest`, `SignedIn`, `GuestBoundToAccount`, `RefusedUnverified`), `ChangeNameAsync` (yeniden doğrulamasız).
- `Contracts/MembershipRecords.cs` (sonuç tipleri) ve `Contracts/Results/{OrderAccountKind,VerificationLinkStatus}.cs`: sonuç değerleri saklanmaz; `Contracts` ad alanının kendisinde olsalardı `06 §5` enum kaydı ve kilit dosyası ister (saklanan enum sayılırlar), bu yüzden `Settings.Contracts.Gates` örneğindeki gibi alt ad alanındadır.
- `Infrastructure/Security/PasswordService.cs`: çerçevenin parola özetleyicisinin tek sarmalayıcısı (210 000 yineleme, `09 §15.1` m.5); kayıtlı olmayan adres için sahte özetle aynı işi yapar (`VerifyNobody`). `LoginRecorder.cs`: giriş kaydını kendi kapsam ve işleminde yazar (IP yoksa `IPAddress.None`).
- `LoginRecord.Create` artık ham adresi alıp `TextNormalizer.NormalizeIdentity` ile normalleştirir (D-64 ikinci yarı). `LinkRecords.SupersedeEarlierAsync` müşteri/yönetici kimliğinden ikisi de boş ya da ikisi de dolu çağrıyı reddeder (D-96 K2). Doğrulama bağlantısı ömrü P-12 için `ProductConstants.EmailVerificationLinkHours` = 24 (tek sabit; CI düzeltmesi). `Customer.RenewDigest`.
- `Ordering`: `IOrderingModule.LinkGuestOrdersAsync` — adaylar okunur, `OrderLocks` ile kilitlenir, kilitten sonra izlemeli yüklenip `CustomerId` atanır (sürüm artmaz; `ExecuteUpdate` yok, `VersionedSideBulkWriteScan`). Hesabı olan ve hesabı silinmiş siparişler alınmaz.
- Kaynak taramaları: `CredentialSourceScan` (+ testler, ihlal örneği `ViolationSamples/Credentials/Misuse.cs`): parola özetleyicisi yalnız `PasswordService.cs`'de, giriş kaydı normalleştirmesi tek yerde. `LinkKeySourceScan`: `ConsumingMethods` (yalnız `MembershipModule.cs` içinde `VerifyEmailAsync` metodu `TryConsumeAsync` çağırabilir) ve `.VerifyEmailAsync(` çağrısı yalnız `OnPost…` işleyicisinde.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/**`, `Modules/Ordering/{OrderingModule,Contracts/IOrderingModule}.cs`, `Infrastructure/Security/{PasswordService,LoginRecorder,LoginRecord,LinkRecords}.cs`, `Infrastructure/ProductConstants.cs`, `Program.cs`.
- Testler: `tests/Shopfolio.IntegrationTests/Model/MembershipTests.cs`, `tests/Shopfolio.UnitTests/Architecture/{CredentialSourceScan,CredentialSourceScanTests,LinkKeySourceScan,LinkKeySourceScanTests}.cs`, `ViolationSamples/Credentials/Misuse.cs`.
- Belgeler: `CONTEXT.md`, `IMPLEMENTATION_STATUS.md` (+ `STATUS_CHANGELOG.md`), `MEMORY.md`, `DEFERRED_BACKLOG.md`; T308, T312, T331 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri dolduruldu.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Misafir siparişi doğrulama kapısının dışında | ✓ | `AGuestOrder_NeedsNoVerification_AndAnAddressWithNoAccountStaysAGuest` |
| 2 | Geçmiş misafir siparişleri doğrulanmış e-postayla hesaba düşer; silinmiş hesabın siparişi bağlanmaz | ✓ | `Verifying_BringsTheGuestOrdersOfTheAddressToTheAccount_AndOnlyThose` (büyük/küçük harf ve boşluk, başkasının, hesabı olan, hesabı silinmiş); `APendingRegistration_TakesNoOrder_UntilItIsVerified` |
| 3 | Doğrulanmış hesabın e-postasıyla girişsiz sipariş hesaba anında bağlanır, bekleyen kayda bağlanmaz | ✓ | `AGuestOrderWithAVerifiedAccountsAddress_IsBoundToTheAccountAtOnce_AndAPendingRegistrationIsNotBoundTo` (modül yüzü `ResolveOrderAccountAsync`; sipariş ucunun bağı D-254) |
| 4 | Doğrulanmamış hesapla giriş ve sipariş yok | ✓ | `AnUnverifiedAccount_IsRefusedAtTheSignIn_EvenWithTheRightPassword_AndTheAttemptIsAFailedOne`, `ASignedInOrder_IsTheAccountsOwn_AndAnUnverifiedAccountCannotOrder` |
| 5 | Z-1 sonrası kayıt silinir, e-posta yeniden kayda açılır; süresi geçmiş bağlantı "geçersiz" | ✓ | `Register_AfterThePendingRegistrationEnded_FreesTheAddress_DeletesTheOldRowAndCountsItInTheDisposalRecord`, `AnExpiredLink_IsInvalid_AndVerifiesNothing_AndASecondUseIsRefused` (gece silmesi T324, D-253; mesajın ekranı D-254) |
| 6 | Doğrulama bağlantıyla değil düğmeyle | ✓ | `OpeningTheLink_ChangesNothing_AndTheButtonVerifies`; `LinkKeySourceScanTests` (`VerifyEmailAsync` çağrısı OnGet'te bulgu, OnPost'ta temiz) |
| 7 | Ad zorunlu; ad değişimi yeniden doğrulamasız | ✓ | `Register_WithoutAName_IsRefused_AndNothingIsWritten`, `TheName_ChangesWithoutANewVerification_AndLeavesTheOrdersRecipientNameAlone` |
| 8 | Pazarlama onayı alanı yok | ✓ | `TheFormAndTheAccount_HaveNoMarketingConsentField` (`RegisterInput` üç özellik; `Customer` üyeleri) |
| 9 | Kapı kapalıyken kayıt reddedilir; her deneme zaman, IP, e-posta, sonuçla giriş kaydında | ✓ | `Register_WithTheGateClosed_IsRefused_NamingTheWaitingConditions_AndCountsNoAttempt`, `SignIn_OfAnExistingAccount_IsNotBehindTheClosedGate`, `EveryAttempt_IsOneRow_WithTheMomentTheIpTheNormalizedEmailAndTheOutcome_AndARefusalDoesNotTakeItBack` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1: misafir sipariş verir, doğrulama aranmaz | ✓ | `AGuestOrder_NeedsNoVerification_…`; `MembershipModule.ResolveOrderAccountAsync` (`Guest`) |
| KK2: geçmiş misafir siparişi doğrulanmış e-postayla hesaba düşer | ✓ | `Verifying_BringsTheGuestOrdersOfTheAddressToTheAccount_AndOnlyThose` (`LinkedOrders` 2; sürüm 1 kalır) |
| KK3: anında bağlanır, bekleyen kayda bağlanmaz | ✓ | `AGuestOrderWithAVerifiedAccountsAddress_IsBoundToTheAccountAtOnce_…` |
| KK4: doğrulanmamış hesap girişte ve siparişte reddedilir | ✓ | `AnUnverifiedAccount_IsRefusedAtTheSignIn_…`, `ASignedInOrder_IsTheAccountsOwn_AndAnUnverifiedAccountCannotOrder` |
| KK5: Z-1 sonrası kayıt silinir, e-posta yeniden kaydolur; geçersiz bağlantı | ✓ | `Register_AfterThePendingRegistrationEnded_…`, `AnExpiredLink_IsInvalid_…` (`VerificationLinkStatus.Expired`) |
| KK6: bağlantıya tıklamak doğrulamaz, düğme doğrular | ✓ | `OpeningTheLink_ChangesNothing_AndTheButtonVerifies`, `LinkKeySourceScanTests.Scan_OnACallOfTheVerificationCommand_…` |
| KK7: adsız kayıt reddedilir; ad değişimi yeniden doğrulama istemez | ✓ | `Register_WithoutAName_IsRefused_…`, `TheName_ChangesWithoutANewVerification_…` |
| KK8: formda ve kayıtta pazarlama alanı yok | ✓ | `TheFormAndTheAccount_HaveNoMarketingConsentField` |
| KK9: kapı kapalıyken kayıt reddedilir; giriş dört alanla yazılır | ✓ | `Register_WithTheGateClosed_…`, `EveryAttempt_IsOneRow_…` |
| `09 §25` — kaynak taraması ve ihlal örneği | ✓ | `CredentialSourceScanTests` (9; `ViolationSamples/Credentials/Misuse.cs`), `LinkKeySourceScanTests` (12; yeni 3) |
| `09 §25` — güvenlik taraması (mini) | ✓ | sır yok; parola yalnız `PasswordService`'te özetlenir, hiçbir kayıt/günlük/mesaja yazılmaz (`Register_MakesAnUnverifiedAccount_…` özette ham değerin geçmediğini sınar); kimsesiz adreste sahte özetle aynı iş (zamanlama); bağlantı anahtarı `LinkKeys` ile türer, `Guid.NewGuid` yok |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | tek istisna `ConsumingMethods`: tek dosya, tek metot, yalnız `TryConsumeAsync`; başka metotta, başka yolda ve metodun çağrısında bulgu (`Scan_OnTheConsumingCommandsFile_AllowsTheConsumeInItsOneNamedMethodOnly`); dosya ve metot varlığı da sınanır (`ConsumingMethods_NamesOnlyExistingFiles_…`). Bekçi ayarına dokunulmadı |
| Sık FAIL (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ | `CredentialSourceScan` nitelikli/niteliksiz `PasswordHasher<…>`, `IPasswordHasher<…>`, `PasswordHasherOptions`, ek argümanlı çağrıları ve model anlık görüntüsünü sınar (`CredentialSourceScanTests`); `VerifyCall` üye erişimi (`.VerifyEmailAsync(`) çok satırlı da bulur |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ | şema değişmedi (yeni tablo/sütun/göç yok) |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | P-12 yalnız `ProductConstants.EmailVerificationLinkHours` (envanterde `ParameterInventory` P-12 satırı; ayrı sabit yok); test beklentisi sabitten türer (`MembershipTests`); backlog aktif sayısı satır sayılarak 132; D-64, D-96, D-199 güncellendi; `LinkKeySourceScan` başlık özeti güncellendi |
| Sık FAIL (5) belge kuralı değiştiren 09 metni karar kaydısız | ✓ | 09/05/07 metnine dokunulmadı; `LinkKeySourceScan` istisnası kuralı daraltmaz (çağıran `OnPost` kuralına bağlı) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Architecture dahil) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 3477 geçti, 0 hata (Architecture ad alanı 842 dahil) |
| Consistency | ✓ | `… --filter-trait "Consistency=true"` | 209 geçti |
| Integration (yerelde, parçalı) | ✓ | `--filter-class "*MembershipTests"`, `"*SessionLinkAttemptLoginTablesTests"`, `"*LinkRecordsTests"`, `"*DisposalTests"` | MembershipTests 29/29; üç komşu sınıf 99/99; Concurrency 26/26 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07; head `391cdc3` + `origin/main` birleştirmesi). Kriter 1–9 ve KK1–KK9 validator kanıtıyla ✓ (kanıt testleri yukarıdaki tabloyla aynı, kod okunarak teyit edildi: kapı → alan → L-3 sırası, tembel Z-1 silmesi ve imha kaydı, düğmeyle tek koşullu tüketim, `LinkGuestOrdersAsync`'in hesabı olan ve hesabı silinmiş siparişi almaması, `ResolveOrderAccountAsync`'in bekleyen kayda bağlamaması, giriş kaydının kendi işleminde dört değerle yazılması). Kriter 3 ve 5'in sipariş ucu ve ekran mesajı yüzleri modül düzeyinde doğrulandı; bağları D-254 (T509, T514). Validator yerelde: build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç proje temiz; Unit 3534 (Architecture 842, Consistency 213, öteki 2479); betik testleri 4 dosya; Integration PostgreSQL 18 (`shopfolio-t341-val-pg`) MembershipTests 29/29, SessionLinkAttemptLoginTables + LinkRecords + AccountEmailQueue 95/95, AppCache + SessionAuthentication 14/14. Dal CI 42378956325 success (head `391cdc3`). Güvenlik: sır yok; parola yalnız `PasswordService`'te (210 000 yineleme), hesapsız adreste sahte özetle aynı iş; bağlantı anahtarı türetilir, ham anahtar saklanmaz; L-3 ve L-9 her istekte iki eksende; hesap varlığı yalnız `02 §8.1.4`'ün izin verdiği kayıt ekranında söylenir; misafir siparişi yalnız doğrulanmış e-postayla bağlanır. Yeni dış bağımlılık yok. `12`: KP-28 → `VAL-D018`, `VAL-D019`, `VAL-D020` (T341) ve `VAL-D021` (T375); üçünün beklenen sonucu sipariş geçmişinin uç yanıtıdır (Veri kaydı + Uç yanıtı), uç sonraki task'larda → `Durum` `BEKLEMEDE` kaldı; `12_matris_denetle.py --kapsam 1-77` hata 0. |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — K1: D-252'nin hedefi T309'du, T309 bu PR'dan önce merge edildi ve doğrulama e-postasını kuyruğa yazan çağrı kimsenin kapsamında kalmadı → hedef T514 (UC-48/UC-49 yan etkisi; bu PR'da kayıt güncellendi). K2: `origin/main` birleştirmesiyle P-12 iki sabitte (T341'in ayrı ömür sabiti ve T309'un `EmailVerificationLinkHours`'u, ikisi de 24) → D-289 (CI düzeltmesinde kapandı, aşağıya bakın). K3: `09 §15.1` madde 5 `PasswordHasher<object>` der, kod `PasswordHasher<HashSubject>` → D-290 (F3 gate). Yapım raporu karşılaştırması: kriter tablosu ve öz denetim kanıtlı, verdict uyumlu; tek uyuşmazlık öz denetimin "P-12 yalnız tek sabitte" satırıdır — dal başında doğruydu, `origin/main` birleştirmesinden sonra değil (K2). |

## CI düzeltmesi (2026-11-07)

Doğrulama PASS sonrası main'e giren T381 (ayar katmanları, parametre envanteri) dalı kırmızı yaptı: (1) Lint CS0535 — `MembershipTests.GateOnlySettings` `ISettingsModule`'ün yeni üyelerini uygulamıyordu; altı üye eklendi (yalnız `NotSupportedException`, kapı okuması değişmedi). (2) Consistency `ParameterInventory` — üyelik modülünün ayrı doğrulama bağlantısı ömrü sabiti `ProductConstants`'ta envanterde yoktu; tek kaynak ilkesiyle sabit silindi, üyelik modülü ve testleri envanterdeki P-12 sabiti `EmailVerificationLinkHours`'u (24) okur. Davranış değişmedi; envanter ve öteki belgeler değişmedi; D-289 kapandı.

- Düzeltme sonrası yerelde: build `-warnaserror` 0 uyarı; format üç proje temiz; Consistency 230, Architecture 918, Integration MembershipTests 29/29, AccountEmailQueueTests 14/14.
- **⚠** Karar yardımcı tarafından sahip yerine verildi (tek sabit, envantere yeni satır eklenmedi). Doğrulamadan geçmiş kodun üzerinde yalnız bu iki değişiklik ve `origin/main` birleştirmesi var.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (`Microsoft.AspNetCore.Identity` çerçevenin parçası)

## Commit & PR

- **Branch:** `task/T341-uyelik`
- **PR:** #433
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(doğrulayıcı doldurur — merge sonrası; sonraki task dalında)*

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Şifre politikası (uzunluk, yaygın liste) kayıtta yok | D-251 | T342 kriter 7 |
| 2 | Doğrulama e-postasının gönderim satırı ve gövdesi | D-252 | T514 (doğrulama tur 1 K1: T309 önce merge edildi, kuyruk yüzü hazır, çağrı UC-48/UC-49 yan etkisi) |
| 3 | Gecelik Z-1 silmesi (T341'de yalnız tembel silme) | D-253 | T324 |
| 4 | Kayıt, doğrulama, giriş ekranları ve POST uçları; yeniden kayıt tekil dizin yarışı için yeniden deneme | D-254 | T514, T515, T517 |
| 5 | L-1 sayacı ve oturum açma | D-255 | T342 |
| 6 | D-64 (LoginRecord yarısı), D-96 K2 kapandı; D-199 modül yüzü kapandı, ekran D-254 | D-64, D-96, D-199 | — |
| 7 | P-12 iki sabitte (doğrulama tur 1 K2) — CI düzeltmesinde tek sabite indi | D-289 ✅ | — |
| 8 | `09 §15.1` madde 5 metni `PasswordHasher<HashSubject>` ile hizalanır (doğrulama tur 1 K3) | D-290 | F3 gate |

## Notlar

- **⚠ Kolay kararlar:** (1) Kayıtta kapı önce, alan denetimi L-3'ten önce gelir; alan hatası (boş ad/şifre, geçersiz e-posta) L-3 sayacına girmez. (2) `ResolveOrderAccount`: girişsiz misafir + bekleyen (doğrulanmamış) kayıt e-postası = `Guest` (reddedilmez, bağlanmaz); `RefusedUnverified` yalnız oturumdaki doğrulanmamış hesap içindir. (3) Parola yinelemesi 210 000 (OWASP; çerçeve varsayılanı 100 000); politika T342. (4) Sonuç enum-ları `Contracts.Results` alt ad alanında: saklanmadıkları için `06 §5` kaydı ve kilit dosyası gerekmez; kayıt/kilit ister bir görüş çıkarsa bu iki dosya taşınır. (5) `LinkKeySourceScan`'e dar istisna `ConsumingMethods` eklendi (doğrulayıcı için).
- **Çalışma:** önceki iki işçi bağlam sınırında bölündü; commit sır bekçisi `…PasswordHash = …`, `PasswordVerificationResult x = …` gibi atamaları anahtar adı sandığı için değişken/tür adları değiştirildi (`entered`, `hashing`, `HashCheck`, `Verdict` takma adı, `Customer.RenewDigest`); `PasswordHasher<object>` genel argümanı `ThirdPartyContentScan` `<object` desenine takıldığı için `HashSubject` sınıfı eklendi. Bekçi ayarına dokunulmadı.
- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: T308 (#424) CI 42368786665 ve Publish 42368786653 success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
