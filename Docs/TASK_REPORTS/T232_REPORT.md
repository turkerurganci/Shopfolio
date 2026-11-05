# T232 — Veri koruma halkası

**Faz:** F2 | **Durum:** ✗ Doğrulama tur 1 FAIL (düzeltme bekliyor) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Halka kurulumu:** `Infrastructure/Security/DataProtectionSetup.Add(IServiceCollection)` — `AddDataProtection().SetApplicationName("Shopfolio").PersistKeysToDbContext<AppDbContext>()` ve `ProtectedValues` tekil kaydı. Anahtar şifreleyicisi ve anahtar ömrü çağrısı yok (çatının varsayılanı); `Program.cs` yalnız `DataProtectionSetup.Add(builder.Services)` çağırır (`09 §15.5` madde 1).
- **Amaçlar:** `DataProtectionPurposes` — altı `const string` (`Shopfolio.<Kullanım>.v1`) ve kapalı liste `All` (`09 §15.5` madde 2).
- **Okuma/yazma:** `ProtectedValues.Protect(purpose, payload)` ve `Unprotect(purpose, value)` → `ProtectedValueResult` (`Valid(payload)` / `Invalid`). Altı koruyucu yapıcıda altı açık `CreateProtector(DataProtectionPurposes.X)` çağrısıyla kurulur (başka `CreateProtector` yok). `CryptographicException` yakalanır → `Invalid`; boş/null değer `Invalid`; listede olmayan amaç programlama hatasıdır (`ArgumentException`).
- **Kaynak taraması:** `DataProtectionSourceScan` — `ProtectKeysWith*`, `SetDefaultKeyLifetime`, `src/`'de `DataProtectionSetup.cs` dışında `AddDataProtection(`, argüman listesi tek `DataProtectionPurposes.<Ad>` olmayan `CreateProtector(` (alt amaç argümanı dahil), her `GetDataProtector(` çağrısı, `DataProtectionPurposes.cs` dışında `"Shopfolio.<X>.v<n>"` literali. İstisna iki dosya, her biri tek kural için; test projeleri taranmaz. Kasıtlı ihlal örneği: `ViolationSamples/DataProtection/RingMisuse.cs`.
- **Eşitlik denetimi:** `DataProtectionConsistencyTests` — `09 §15.5` madde 2 tablosundaki amaç dizgeleri ↔ `DataProtectionPurposes.All` (iki yönlü); yedek yapılandırmasında (`pgbackrest.conf`, giriş ve iş betikleri) veritabanı/tablo süzgeci yok (tam küme yedeği); anahtar tablosu uygulama veritabanının göçünde.
- **Açılış günlüğü (⚠ aşağıda):** halka açılışta anahtarları EF ile okuyunca, erişilemeyen veritabanında EF'in iki hata satırı (`Database.Connection`, `Query`) sunucu adresini, veritabanı adını ve istisna metnini ileti içinde yazar; biçimleyici ileti metnini ayıklayamaz. **Düzeltme 1 sonrası:** kategoriler uyarıda kalır; `LoggingSetup.Configure` yalnız iki olayı kategori + EF olay kimliğiyle süzer (`SuppressedDataAccessFailures`; hata çağıranda istisna olarak kalır, maskeli yazılır). `09 §18.1` madde 4 buna göre yazıldı (karar T232-K1, "Kararlar" bölümü).
- **Paketleme:** anahtarlar veritabanındadır, dosya sistemi yolu yok; `scripts/ci/package-build.sh` adım 5'te konteynerde yazılabilir anahtar dizini gerekmez, imaj/Compose değişmedi. Paket (`Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`) ve `data_protection_keys` tablosu daha önce vardı; yeni paket/göç yok.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/DataProtectionSetup.cs`, `DataProtectionPurposes.cs`, `ProtectedValues.cs`, `ProtectedValueResult.cs` (yeni); `Program.cs`; `Infrastructure/Logging/LoggingSetup.cs`
- `tests/Shopfolio.UnitTests/Infrastructure/ProtectedValuesTests.cs`, `DataProtectionSetupTests.cs`; `Architecture/DataProtectionSourceScan(+Tests).cs`, `ViolationSamples/DataProtection/RingMisuse.cs`; `Consistency/DataProtectionConsistencyTests.cs`
- `tests/Shopfolio.IntegrationTests/Web/DataProtectionRingTests.cs`
- `Docs/09_CODING_GUIDELINES.md` (§18.1 madde 4), `Docs/IMPLEMENTATION_STATUS.md` (T232 satırı), `.claude/memory/MEMORY.md`, bu rapor

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Halka kurulumu tek: `DataProtectionSetup.Add`; sabit uygulama adı; anahtarlar veritabanında şifresiz; şifreleyici yok; ömür varsayılan | ✓ | `DataProtectionSetup.cs`; `Add_FixesTheApplicationName`, `Add_KeepsTheKeysInTheDatabaseInClearWithTheDefaultLifetime` (depo türü, `XmlEncryptor` null, `NewKeyLifetime` = çatı varsayılanı); `Start_WritesTheKeyAsARowOfTheKeyTable_InClear` (satırda `encryptedSecret` yok); `ApplicationSource_SetsUpTheRingInOneFileAndWritesNoPurposeStringElsewhere` |
| 2 | Amaç dizgeleri tek yerde; altı amaç kapalı liste | ✓ | `DataProtectionPurposes.cs`; `Purposes_AreTheClosedList_EachShapedShopfolioUseV1AndDistinct` (sabitler yansımayla okunur, `All` ile eşit); `PurposeTableOf09_EqualsTheClosedListOfTheCode_InBothDirections` |
| 3 | `CreateProtector` yalnız bu sabitlerle; dizge başka yerde yazılmaz | ✓ | `ProtectedValues.cs` altı çağrı; `ApplicationSource_...`; `Scan_OnViolationSample_ReportsEachRule...` (literal satır 6 ve 13, değişken satır 7, sarılı çağrı satır 8, alt amaç argümanı satır 12, `GetDataProtector` satır 15) |
| 4 | Çözme istisna fırlatmaz: `Valid` / `Invalid` | ✓ | `Unprotect_OnEmptyOrDamagedText_IsInvalidAndDoesNotThrow`, `Unprotect_OnTruncatedAndTamperedValue_IsInvalid` (altı amaç) |
| 5 | Boş, bozuk, başka amaçlı, anahtarı halkada olmayan → `Invalid` | ✓ | `Unprotect_OnEmptyOrDamagedText_...` (null, "", " ", bozuk, tek harf), `Unprotect_OfAValueWhoseKeyIsNotInTheRing_IsInvalid` (ayrı halka), entegrasyon `Value_WhoseKeyIsNoLongerInTheRing_IsInvalid` (anahtar satırları silindi, yeni örnek); başka amaç: madde 6 |
| 6 | Bir amaçla korunan değer öteki beş amaçta çözülmez | ✓ | `Unprotect_UnderAnotherPurpose_IsInvalid` (6×5 çift `All`'dan kurulur) ve `CrossPairs_CoverEveryOrderedPairOfDifferentPurposes` (sayı `n·(n−1)`) |
| 7 | Biçim değişirse sonek artar; eski değer çözülemez | ✓ | `ChangedSuffix_RefusesTheOldValue_AndTheOldPurposeRefusesTheNewValue` (v2 koruyucu v1 değerini `CryptographicException` ile reddeder; v1 okuyucu v2 değerini `Invalid` sayar) |
| 8 | Anahtar halkası yedeğin kapsamındadır (`05 §7.6.3`) | ✓ | `DatabaseBackup_IsOfTheWholeClusterWithNoTableOrDatabaseFilter` (pgBackRest tam küme, süzgeç yok), `KeyRingTable_IsInTheMigrationOfTheApplicationDatabase`; halka dosya değil tabloda (`Start_WritesTheKey...`) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1–k8 | ✓ | Yukarıdaki tablo |
| KK1: halka tek noktada; anahtar `DataProtectionKey` tablosunda; kaynakta `ProtectKeysWith`/`SetDefaultKeyLifetime` yok; ikinci örnek birincinin değerini çözer | ✓ | `ApplicationSource_...` (iki kural, `Scan_OnViolationSample_...` satır 5'te kırmızı); `Start_WritesTheKeyAsARowOfTheKeyTable_InClear`; `SecondInstance_OnTheSameDatabase_ReadsWhatTheFirstProtected` (altı amaç, ayrı konak/ayrı fabrika) ve `TwoInstancesRunningTogether_ShareOneRing` |
| KK2: altı sabit, biçim `Shopfolio.<Kullanım>.v1` | ✓ | `Purposes_AreTheClosedList_...` (regex), tablo eşitliği |
| KK3: kaynak taraması başka yerde amaç dizgesi bulmaz; ek argüman ve `GetDataProtector` bulgudur | ✓ (düzeltme 1) | `ApplicationSource_...` (`src/`'de `CreateProtector(` yalnız `ProtectedValues.cs` 23–28, hepsi tek sabit; `GetDataProtector` 0); `Scan_OnViolationSample_...` satır 12 ve 15 kırmızı; `Scan_InThePurposesFile_...`; ihlal örneği ayrı dosyada |
| KK4: bozuk değer istisna atmaz | ✓ | `Unprotect_OnEmptyOrDamagedText_...`, `..._OnTruncatedAndTamperedValue_...` |
| KK5: boş, bozuk, yanlış amaçlı, anahtarı olmayan | ✓ | madde 5'teki testler |
| KK6: 6×5 çapraz çift reddedilir | ✓ | `Unprotect_UnderAnotherPurpose_IsInvalid` (30 kayıt) |
| KK7: `v2` çözme eski değeri reddeder | ✓ | `ChangedSuffix_...` |
| KK8: yedek kapsamı anahtar tablosunu içerir | ✓ | `DataProtectionConsistencyTests` (Lint adımında koşar) |
| `09 §25` — doküman uyumu | ✓ | Okunan: `09 §15.5`, `05 §7.6.3`, `06 §4.68`, `09 §18.1` madde 4, `§18.2` madde 2; sınıf/yöntem adları `§15.5`'ten birebir (`DataProtectionSetup.Add`, `DataProtectionPurposes`, `ProtectedValues.Unprotect`, `ProtectedValueResult`); `09 §15.5` madde 4–5'in sınıfları (`DownloadLinks`, bekleyen kayıt/davet) T232 dışıdır, eklenmedi |
| `09 §25` — diff kapsamı | ✓ | Halka, amaçlar, okuma/yazma, tarama, eşitlik denetimi; ek olarak günlük kategorisi düzeltmesi (⚠ a) |
| `09 §25` — mimari ve kod | ✓ | Yeni dosyalar `Infrastructure/Security/` (modül ilişkisi yok); yeni paket yok; zaman/`catch`: yalnız `CryptographicException`; `Program.cs` tek satır |
| `09 §25` — test | ✓ | Her tarama kuralı için ihlal örneği (`RingMisuse.cs`, beş kural); beklenen değerler `DataProtectionPurposes.All`/doküman tablosundan türer (elle sayı yok; 6×5 `n·(n−1)`); birim testleri dosya/ağ/DB'ye yazmaz (çatının bellek sağlayıcısı `EphemeralDataProtectionProvider`; tarama ve tablo testleri salt okunur); entegrasyon her test için şablondan kopya veritabanı |
| `09 §25` — veri/göç | uygulanamaz | Tablo ve göç T1xx'te vardı; bu task göç/varlık/DB varsayılanı/FK eklemez |
| Sık FAIL (1) beklenen değer sabitten türer | ✓ | yukarıda |
| Sık FAIL (2) kural değişince öteki yerler | ✓ (düzeltme 1) | `09 §18.1` madde 4 ↔ `LoggingSetup.SuppressedDataAccessFailures`; `09 §15.5` madde 2 ↔ tarama kuralları; `IMPLEMENTATION_STATUS`, hafıza (`MEMORY.md`) ve bu rapor güncel; `09`'da "hiç yazılmaz"/`Fatal` kalıntısı yok (`grep -n`); amaç listesi ↔ `09 §15.5` tablosu eşitlik testli; "altı amaç" sayısı kodda `All` ile, testlerde `All.Count`'tan |
| Sık FAIL (3) bekçi/tarama istisnası geniş | ✓ | `SetupFile` ve `PurposesFile` tam yol eşleşmesi, her biri tek kural; `Scan_InTheSetupFile_...`, `Scan_InThePurposesFile_...` kalan kuralların hâlâ kırmızı olduğunu gösterir; sır bekçisine dokunulmadı |
| Sık FAIL (4) ihlal örneği / dosya sistemi | ✓ | `RingMisuse.cs`; birim testi dosya yazmaz (`grep -n "WriteAll\|CreateTemp\|File.Delete\|Directory.Create"` yeni birim testlerinde 0) |
| Sık FAIL (5) ikinci maskesiz çıktı | ✓ | Açılış günlüğü: erişilemeyen veritabanında dosyada sunucu/veritabanı adı yok: `Start_WithADatabaseThatCannotBeReached_Opens_AndTheFileCarriesNeitherTheServerNorTheDatabaseName`; yeni kayıt satırı eklenmedi; DB varsayılanı/FK dizini uygulanamaz |
| Sık FAIL (6) kayıt kategorisi bütünüyle kapatılmadı | ✓ | `LoggingSetup.cs` yalnız `MinimumLevel.Warning()` + uygulama ad alanı geçersiz kılması + `Filter.ByExcluding` (`grep -n "Fatal"` `Infrastructure/Logging/`'te 0); test uyarının yazıldığını gösterir |
| Sık FAIL (7) tarama çağrı biçimini kaçırmıyor | ✓ | `CreateProtector` ek argümanlı, `GetDataProtector(`/`<`: `RingMisuse.cs` satır 12 ve 15 |
| Sık FAIL (8) test beklenen değeri sabitten türetir | ✓ | yeni kayıt testi çiftleri `LoggingSetup.SuppressedDataAccessFailures`'tan, tarama testi sabit adlarından (`ExtensionProtector` vb.) okur; EF olay kimlikleri paketten (`RelationalEventId`, `CoreEventId`) |
| Güvenlik taraması | ✓ | Anahtar değeri/sır depoda, kayıtta ya da hata iletisinde yok (testler yalnız `encryptedSecret` yokluğuna ve şekle bakar, anahtar içeriğini yazmaz); `ProtectedValues` hata iletisine değer koymaz (`ArgumentException` yalnız sabit metin); dosya/ortam sırrı eklenmedi; yeni dış bağımlılık yok; girdi doğrulama: bozuk değer `Invalid` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 730/730 (önceki 648; Consistency etiketli alt küme 117/117) |
| Integration | ✓ | PostgreSQL 18, konteyner `shopfolio-t232-pg` (127.0.0.1:55632; 55370–55569 Windows'ta ayrılmış); `Web` 64/64 (yeni 10), `Logging`+`FileSecrets`+`Database` 72/72, `Model` 893/893 | toplam 1029/1029 |
| Biçim ve derleme | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes`; her iki test projesi derlemesi (analizör hataları dahil) | çıkış 0, 0 uyarı |
| Contract | — | yok | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-05, bağımsız doğrulayıcı; dal head `5a8e97e`) |
| Bulgu sayısı | 2 — B1 ORTA S1, B2 DÜŞÜK S1 |
| Düzeltme gerekli mi | Evet (B1, B2) |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Halka tek kurulum; sabit ad; şifresiz; şifreleyici yok; varsayılan ömür | ✓ | `src/`'de `AddDataProtection` yalnız `DataProtectionSetup.cs`, `ProtectKeysWith`/`SetDefaultKeyLifetime`/`GetDataProtector` yok (validator grep); `Add_FixesTheApplicationName` `ApplicationDiscriminator` = `Shopfolio` (yoldan bağımsız); `XmlEncryptor` null, `NewKeyLifetime` çatı varsayılanı; entegrasyon: anahtar `data_protection_keys` satırı, `<masterKey>` var, `encryptedSecret` yok; ikinci fabrika (ayrı DI konteyneri, aynı veritabanı) altı amacın değerini çözer, tek anahtar satırı |
| 2 | Amaçlar tek yerde; altı amaç kapalı liste | ✓ | `DataProtectionPurposes` altı `const`, biçim `Shopfolio.<Kullanım>.v1`; yansımayla sabitler = `All`; `09 §15.5` tablosu ↔ kod iki yönlü eşit |
| 3 | `CreateProtector` yalnız sabitlerle; dizge başka yerde yok | ~ | Bugünkü kod uyar (altı çağrı, hepsi tek sabit). Tarama kuralı tam değil: B2 |
| 4 | Çözme istisna fırlatmaz | ✓ | `catch (CryptographicException)` → `Invalid`; null/boş/bozuk/kırpılmış/değiştirilmiş testleri; listede olmayan amaç `ArgumentException` (programlama hatası, değer hatası değil) |
| 5 | Boş, bozuk, başka amaçlı, anahtarı olmayan → `Invalid` | ✓ | Birim (ayrı halka) + entegrasyon (anahtar satırları silinip yeni örnek açılınca eski değer `Invalid`, yeni değer `Valid`) |
| 6 | 6×5 çapraz red | ✓ | `CrossPairs` `All`'dan kurulur, 30 kayıt; `n·(n−1)` denetimi |
| 7 | Sonek artınca eski değer reddedilir | ✓ | v2 koruyucu v1 değerini reddeder; v1 okuyucu v2 değerini `Invalid` sayar |
| 8 | Halka yedek kapsamında | ✓ | Tablo uygulama veritabanının göçünde; pgBackRest yapılandırması ve betiklerde veritabanı/tablo süzgeci yok (tam küme) |

**Task satırları (`12 §7.5`):** task'ın kaynakları `05 §7.6` ve `09 §15`'tir (KP değil); KP grep'i satır getirmez. İlgili satır `VAL-A589` (çapraz amaç; KK6 ile örtüşür) çağıranın davranışını da ister ("çağıran değeri hiç gelmemiş sayar") — çağıranlar sonraki task'ların; `Durum` hücresine dokunulmadı (`BEKLEMEDE`).

**B1 (ORTA, S1):** `LoggingSetup` iki veri erişim kategorisini (`Microsoft.EntityFrameworkCore.Database.Connection`, `Microsoft.EntityFrameworkCore.Query`) `Fatal`'a çeker ve bu dal `09 §18.1` madde 4'e "iki kategori hiç yazılmaz" cümlesini ekler. (a) **Çelişki:** `05 §9.1.6` "çatının ve veri erişim katmanının kendi kayıt kategorileri uyarı düzeyindedir" der; `09`'un yeni cümlesi buna aykırıdır ve karar kaydı yoktur (`09 §24` madde 2, 8: çelişkiyi ajan seçerek çözmez). (b) **Kapsam dar değil:** gerekçe yalnız iki hata satırıdır (bağlantı hatası, sorgu yineleme hatası), ama `Query` kategorisi EF'in sorgu uyarılarını da taşır (ör. çoklu koleksiyon `Include`, `OrderBy`'sız satır sınırı) — bunlar da artık hiç yazılmaz. Hatanın kendisi gizlenmiyor: istisna çağırana gider, `KeyRingProvider` satırı maskeli yazılır (`Start_WithADatabaseThatCannotBeReached_...` yeşil). (c) `09` cümlesindeki "biçimleyici ileti metnini ayıklayamaz" gerekçesi kısmen eksik: yapım raporu ⚠ (a) sunucu/veritabanının ayrı özellik olarak da geldiğini yazar. Düzeltme yönü: yalnız iki hata olayını olay kimliğiyle kapatan dar süzgeç (kategori uyarıda kalır) ya da biçimleyici yolu; hangisi seçilirse `05 §9.1.6` ile ilişkisi karar kaydına bağlanır ve `09` cümlesi buna göre yazılır; dar süzgeç için test (hata satırı yazılmaz, aynı kategorinin uyarısı yazılır).

**B2 (DÜŞÜK, S1, güvenlik):** `09 §15.5` madde 2 "`CreateProtector` yalnız `DataProtectionPurposes`'ın sabitleriyle çağrılır (kaynak taraması: dizge sabiti başka yerde yazılmaz)". `DataProtectionSourceScan` ilk argüman sabitse gerisine bakmaz: `CreateProtector(DataProtectionPurposes.CheckoutCookie, "extra")` bulgu değildir ve ihlal örneğinin 12. satırı bunu **kabul edilen** biçim olarak sabitler (`Scan_OnViolationSample_...` beklenen listesinde satır 12 yok). Ayrıca çatının `GetDataProtector(IServiceProvider, string)` uzantısı taranmaz — listesiz amaçla koruyucu kurmanın ikinci yolu. Bugünkü kodda ikisi de yok. Düzeltme yönü: tarama tek argümanlı sabit çağrıyı kabul etsin (alt amaç dizgesi bulgu), `GetDataProtector(` bulgu olsun; ihlal örneğinde ikisi kırmızı.

Testler (validator, yerel): build `-warnaserror` 0 uyarı · `dotnet format --verify-no-changes` 0 · Unit 730/730 (T232 sınıfları 82/82) · betik testleri 4/4 · Integration 1029/1029 (Web 64, Database+Logging+FileSecrets 72, Model 893; PostgreSQL 18 `shopfolio-t232-val-pg`, silindi) · `dotnet restore --locked-mode` 0, `git status` temiz · dal CI 41949769170 `success` (head `5a8e97e`) · ana dal son 3 run success (41946974227, 41946974061, 41928364198).

**Yapım raporu karşılaştırması:** kriter ve öz denetim tabloları kanıtlı. İki uyuşmazlık: KK3 ve "Sık FAIL (3)" satırları ✓ der, taramanın alt amaç dizgesini kabul ettiğini görmez (B2); "`09 §25` — diff kapsamı" ve ⚠ (a) kategori kapatmasını kurala uygun sayar, `05 §9.1.6` ile çelişkiyi görmez (B1).

## Düzeltme 1 (tur 1 FAIL: B1, B2)

Dal `origin/main` ile birleştirildi (çakışma: hafıza ve `IMPLEMENTATION_STATUS`; iki tarafın satırları korundu — T231 ✓ ve T232 ⏳ #388).

- **B1 kapandı:** `LoggingSetup`'ta `Database.Connection` ve `Query` kategorilerinin `Fatal` geçersiz kılmaları kaldırıldı; kategoriler uyarıda. `.Filter.ByExcluding(IsSuppressedDataAccessFailure)` yalnız `SuppressedDataAccessFailures` listesindeki iki (kategori, EF olay kimliği) çiftini dışlar: `RelationalEventId.ConnectionError.Id` ve `CoreEventId.QueryIterationFailed.Id` (kimlikler EF paketinden okunur, elle sayı yok). `09 §18.1` madde 4 buna göre yeniden yazıldı. Test: `NormalWriter_DoesNotWriteTheTwoDataAccessFailureEvents_ButWritesAWarningOfTheSameCategory` (iki hata olayı yazılmaz, aynı kategorinin uyarısı yazılır; çiftler listeden türer). Erişilemeyen veritabanı testleri (`Start_WithADatabaseThatCannotBeReached_...`, `Get_WhenTheServerIsUnreachable_...`) yeşil kaldı.
- **B2 kapandı:** `ProtectorWithOtherArgument` artık argüman listesinin tek `DataProtectionPurposes.<Ad>` ile `)` ile bitmesini ister; yeni `GetDataProtectorCall` kuralı (`ExtensionProtector`) her `GetDataProtector(`/`GetDataProtector<` çağrısını bulgu sayar. `RingMisuse.cs` satır 12 (`CreateProtector(..., "extra")`) ve yeni satır 15 (`GetDataProtector`) kırmızı; `Scan_OnViolationSample_...` beklenen listesinde. `09 §15.5` madde 2 cümlesi eşitlendi.
- **Sınama:** build `-warnaserror` 0/0; `dotnet format --verify-no-changes` temiz; Unit 811/811; Integration (PostgreSQL 18, `shopfolio-t232-fix-pg`, silindi) Logging 14/14, Web 64/64, Database 35/35, FileSecrets 24/24 (Model 893 bu değişiklikten etkilenmez, koşulmadı).

## Kararlar

| No | Karar | Gerekçe |
|---|---|---|
| T232-K1 | Veri erişim kategorileri (`Database.Connection`, `Query`) uyarıda kalır; yalnız iki hata olayı (bağlantı hatası, sorgu yineleme hatası) EF olay kimliğiyle süzülür. Karar yardımcıdan (sahip yerine, en kurala uygun seçenek). | `05 §9.1.6` veri erişim kategorilerini uyarı düzeyinde tutar; bütünüyle kapatmak EF sorgu uyarılarını susturur. `09 §18.1` madde 4 buna göre yazıldı (çelişki yok). Doküman dönemi karar kaydı (`PRODUCT_DISCOVERY_STATUS.md`) K-1574'te arşivlendi ve yeni karar kabul etmez; bu yüzden karar bu raporda tutulur (`00 §D`–`§E`). |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (anahtar dizini yok; imaj ve Compose değişmedi)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T232-veri-koruma-halkasi`
- **Commit:** *(PR açıklamasında head SHA)*
- **PR:** #388
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(merge sonrası doğrulayıcı doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Çatı varsayılanları: uygulama adı ayarlanabilir, anahtar ömrü 90 gün, şifreleyici yoksa anahtar açık XML | `KeyManagementOptions` testleri; satırda `<masterKey>` var, `encryptedSecret` yok | ✓ |
| 2 | `Protect(string)` URL güvenli Base64 verir; bozuk metin `CryptographicException` | `RoundTrip_ForEachPurpose_...` (`^[A-Za-z0-9_-]+$`); bozuk/kırpılmış testleri | ✓ |
| 3 | Çatı açılışta anahtar halkasını okur ve ilk anahtarı kendisi yazar | `Start_WritesTheKeyAsARowOfTheKeyTable_InClear` (istek öncesi bir satır); erişilemeyen veritabanında uygulama yine açılır | ✓ |
| 4 | Plan katmanı, kota, anahtar, bölge: yok | 4a tarama terimleri task metninde yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `DownloadLinks` ve bekleyen kayıt/davet değer sınıfları (`09 §15.5` madde 4–5) | — | T232 kabul kriterlerinde yok; ilgili uç task'larına ait |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 41946974227, 41946974061, 41928364198 — üçü success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Port:** 55460–55469 Windows'ta ayrılmış aralıkta (55370–55569); 55632 kullanıldı.
- **⚠ Kararlar (sahibe sorulmadan, kurala en uygun seçenekle):**
  - (a) Halka açılışta anahtarları EF ile okuyunca erişilemeyen veritabanında mevcut iki açılış testi (`Start_WithADatabaseThatCannotBeReached_...`, `Get_WhenTheServerIsUnreachable_...`) kırıldı: EF'in `Database.Connection` ve `Query` hata satırları sunucu adresini/veritabanı adını/istisna metnini ileti içinde yazar. Seçenek: iki kategoriyi `LoggingSetup`'ta `Fatal`'a çekmek (hata çağıranda istisna, maskeli yazılır) + `09 §18.1` madde 4'e cümle. Alternatif (biçimleyiciye `Server`/`Database`/`Error` özellik adlarını yasaklamak) `09 §18.2` madde 2a'nın kişisel veri listesini değiştirirdi. Sahip isterse kategori yerine biçimleyici yolu seçilebilir.
  - (b) `ProtectedValues` örnek (sınıf değil) ve `IDataProtectionProvider` alır, `DataProtectionSetup.Add` tekil olarak kaydeder; `09 §15.5` madde 3 imzası `Unprotect(string purpose, string? value)` korundu, `Protect(purpose, payload)` amaçlı eklendi (amaç parametresi madde 3'ün "değer yazan taraf `Protect(string)`" cümlesinin kapalı liste ile uyumlu biçimi).
  - (c) Listede olmayan amaç `Invalid` değil `ArgumentException` (programlama hatası; değer hatası değil).
  - (d) KK8 yedek kapsamı birim düzeyinde (yapılandırma ve göç okuması) sınandı; gerçek pgBackRest geri yükleme provası T07'nin işidir.
  - (e) Önceki task'ların (T205, T230) merge kaydı (skill adım 15) bu dalda doldurulmadı: paralel işçilerle ortak belge çakışması; doğrulayıcı/yardımcı doldurur.
