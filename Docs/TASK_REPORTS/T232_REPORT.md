# T232 — Veri koruma halkası

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-05

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Halka kurulumu:** `Infrastructure/Security/DataProtectionSetup.Add(IServiceCollection)` — `AddDataProtection().SetApplicationName("Shopfolio").PersistKeysToDbContext<AppDbContext>()` ve `ProtectedValues` tekil kaydı. Anahtar şifreleyicisi ve anahtar ömrü çağrısı yok (çatının varsayılanı); `Program.cs` yalnız `DataProtectionSetup.Add(builder.Services)` çağırır (`09 §15.5` madde 1).
- **Amaçlar:** `DataProtectionPurposes` — altı `const string` (`Shopfolio.<Kullanım>.v1`) ve kapalı liste `All` (`09 §15.5` madde 2).
- **Okuma/yazma:** `ProtectedValues.Protect(purpose, payload)` ve `Unprotect(purpose, value)` → `ProtectedValueResult` (`Valid(payload)` / `Invalid`). Altı koruyucu yapıcıda altı açık `CreateProtector(DataProtectionPurposes.X)` çağrısıyla kurulur (başka `CreateProtector` yok). `CryptographicException` yakalanır → `Invalid`; boş/null değer `Invalid`; listede olmayan amaç programlama hatasıdır (`ArgumentException`).
- **Kaynak taraması:** `DataProtectionSourceScan` — `ProtectKeysWith*`, `SetDefaultKeyLifetime`, `src/`'de `DataProtectionSetup.cs` dışında `AddDataProtection(`, argümanı `DataProtectionPurposes.<Ad>` olmayan `CreateProtector(`, `DataProtectionPurposes.cs` dışında `"Shopfolio.<X>.v<n>"` literali. İstisna iki dosya, her biri tek kural için; test projeleri taranmaz. Kasıtlı ihlal örneği: `ViolationSamples/DataProtection/RingMisuse.cs`.
- **Eşitlik denetimi:** `DataProtectionConsistencyTests` — `09 §15.5` madde 2 tablosundaki amaç dizgeleri ↔ `DataProtectionPurposes.All` (iki yönlü); yedek yapılandırmasında (`pgbackrest.conf`, giriş ve iş betikleri) veritabanı/tablo süzgeci yok (tam küme yedeği); anahtar tablosu uygulama veritabanının göçünde.
- **Açılış günlüğü (⚠ aşağıda):** halka açılışta anahtarları EF ile okuyunca, erişilemeyen veritabanında EF'in iki hata satırı (`Database.Connection`, `Query`) sunucu adresini, veritabanı adını ve istisna metnini ileti içinde yazar; biçimleyici ileti metnini ayıklayamaz. `LoggingSetup.Configure` bu iki kategoriyi `Fatal`'a çeker (hata çağıranda istisna olarak kalır, maskeli yazılır). `09 §18.1` madde 4'e cümle eklendi.
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
| 3 | `CreateProtector` yalnız bu sabitlerle; dizge başka yerde yazılmaz | ✓ | `ProtectedValues.cs` altı çağrı; `ApplicationSource_...`; `Scan_OnViolationSample_ReportsEachRule...` (literal satır 6 ve 13, değişken satır 7, sarılı çağrı satır 8) |
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
| KK3: kaynak taraması başka yerde amaç dizgesi bulmaz | ✓ | `ApplicationSource_...`; `Scan_InThePurposesFile_AllowsThePurposeStringsAndNothingElse`; ihlal örneği ayrı dosyada |
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
| Sık FAIL (2) kural değişince öteki yerler | ✓ | `09 §18.1` madde 4 yeni cümlesi ↔ `LoggingSetup` sabitleri; amaç listesi ↔ `09 §15.5` tablosu eşitlik testli; "altı amaç" sayısı kodda `All` ile, testlerde `All.Count`'tan |
| Sık FAIL (3) bekçi/tarama istisnası geniş | ✓ | `SetupFile` ve `PurposesFile` tam yol eşleşmesi, her biri tek kural; `Scan_InTheSetupFile_...`, `Scan_InThePurposesFile_...` kalan kuralların hâlâ kırmızı olduğunu gösterir; sır bekçisine dokunulmadı |
| Sık FAIL (4) ihlal örneği / dosya sistemi | ✓ | `RingMisuse.cs`; birim testi dosya yazmaz (`grep -n "WriteAll\|CreateTemp\|File.Delete\|Directory.Create"` yeni birim testlerinde 0) |
| Sık FAIL (5) ikinci maskesiz çıktı | ✓ | Açılış günlüğü: erişilemeyen veritabanında dosyada sunucu/veritabanı adı yok: `Start_WithADatabaseThatCannotBeReached_Opens_AndTheFileCarriesNeitherTheServerNorTheDatabaseName`; yeni kayıt satırı eklenmedi; DB varsayılanı/FK dizini uygulanamaz |
| Güvenlik taraması | ✓ | Anahtar değeri/sır depoda, kayıtta ya da hata iletisinde yok (testler yalnız `encryptedSecret` yokluğuna ve şekle bakar, anahtar içeriğini yazmaz); `ProtectedValues` hata iletisine değer koymaz (`ArgumentException` yalnız sabit metin); dosya/ortam sırrı eklenmedi; yeni dış bağımlılık yok; girdi doğrulama: bozuk değer `Invalid` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 730/730 (önceki 648; Consistency etiketli alt küme 117/117) |
| Integration | ✓ | PostgreSQL 18, konteyner `shopfolio-t232-pg` (127.0.0.1:55632; 55370–55569 Windows'ta ayrılmış); `Web` 64/64 (yeni 10), `Logging`+`FileSecrets`+`Database` 72/72, `Model` 893/893 | toplam 1029/1029 |
| Biçim ve derleme | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes`; her iki test projesi derlemesi (analizör hataları dahil) | çıkış 0, 0 uyarı |
| Contract | — | yok | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (anahtar dizini yok; imaj ve Compose değişmedi)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T232-veri-koruma-halkasi`
- **Commit:** *(PR açıklamasında head SHA)*
- **PR:** *(aşağıda)*
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
