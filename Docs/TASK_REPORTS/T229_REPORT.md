# T229 — Uygulama belleğinde önbellek

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Önbellek çekirdeği** (`09 §10.4` madde 1–3): `Infrastructure/CacheArea` (beş alan: `Menu`, `CategoryTree`, `Settings`, `HomepageBlocks`, `Sitemap`), `Infrastructure/AppCache` (tekil; alan başına yuva: değer + kuşak sayacı + `Lock`; `GetOrLoadAsync`, `Invalidate`, `GenerationOf`; kilit içinde `await` yok, süre sonu ve ısıtma yok), `Infrastructure/CacheInvalidations` (kapsamlı; `Add` açık işlem yoksa reddeder; `TakePendingAreas`). Kayıt `AddAppCache()` (`Program.cs`).
- **İşlem sonrası geçersiz kılma:** `TransactionRunner` işlemin başında kümeyi temizler, kesinleşmeden **sonra** her alan için `AppCache.Invalidate` çağırır; ret ve iş istisnasında kümeyi unutur. Kesinleştirme çağrısının kendisi hata verirse (sonuç bilinmez) alanlar yine atılır — atmak zararsızdır (⚠ K1, `09 §10.4` madde 3'e bir cümle).
- **Kalıp örneği gerçek ad alanlarında** (`tests/Shopfolio.UnitTests/Architecture/CachePattern.cs`; tümleşik projeye bağlı dosya): Settings modülünün iki okuma yolu (gösterim → önbellek; sipariş onayı → işlem içinde veritabanı), CorporateContent duyurusu (önbellekte kayıt, görünürlük her istekte `TimeProvider`'dan), Catalog fiyatı (önbelleksiz), `Shopfolio.Pages.Storefront.CachePatternPage` (yalnız `Contracts` arayüzleri). Kaydetme alanı işlem içinde adlandırır; ad, deyimden önce verilir (işlem dışında hiçbir şey yazılmadan reddedilir).
- **Taramalar ve mimari testler:** `CacheSourceScan` (önbellek türleri yalnız kendi dosyalarında; `Invalidate`/`TakePendingAreas` yalnız koşucuda; başka bellek içi önbellek — çatının önbellekleri, `ConcurrentDictionary`, `Lazy<>`, `LazyInitializer`, `<cache>` etiket yardımcıları — yok; sayfa çıktısı önbelleği yok; bağımlılık ve dağıtım dosyalarında ayrı önbellek sunucusu/istemcisi yok), derlenen kural (çatının önbellek kitaplıklarına ve çıktı önbelleğine bağımlılık yok), statik değiştirilebilir koleksiyon alanı kuralı (yansıma; ⚠ K3), `ModuleBoundaryTests`'e iki test. Her kuralın kasıtlı ihlal örneği var (`ViolationSamples/Cache/*`, derlenen `ViolationSamples/CacheCode/CacheDependencies.cs`).
- **Tümleşik testler** (`Web/AppCacheTests`, gerçek PostgreSQL; sayfa testi TestServer ve gerçek Kestrel): KK2–KK7 ve gerçek eşzamanlı işlemlerle kuşak yarışı.
- **Belgeler:** `09 §10.4` madde 3'e kesinleştirme hatası cümlesi; `CONTEXT.md` Infrastructure satırı; T219 raporunun "Post-merge run'lar" satırı ve status Commit hücresi (`4ce191c`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/`: `AppCache.cs`, `CacheArea.cs`, `CacheInvalidations.cs` (yeni); `TransactionRunner.cs`, `ITransactionRunner.cs`; `Program.cs`
- `tests/Shopfolio.UnitTests/`: `Infrastructure/AppCacheTests.cs`, `Architecture/CachePattern.cs`, `Architecture/CacheSourceScan.cs`, `Architecture/CacheSourceScanTests.cs`, `Architecture/ModuleBoundaryTests.cs`, `Architecture/ViolationSamples/Cache/*`, `Architecture/ViolationSamples/CacheCode/CacheDependencies.cs`, csproj (`BuildCacheViolationSample`)
- `tests/Shopfolio.IntegrationTests/`: `Web/AppCacheTests.cs`, `Pages/Storefront/CachePatternSample.cshtml`, csproj (bağlı `CachePattern.cs`); dokuz test donanımına `AddAppCache()` ve beş yerde koşucunun yeni yapıcısı
- `Docs/09_CODING_GUIDELINES.md` (§10.4 m.3), `.claude/CONTEXT.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T219_REPORT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Önbellek uygulamanın belleğinde; ayrı sunucu yok; geçersiz kılma tek örnekte | ✓ | `AppCache` tekil (`AppCacheRegistration`); `DependenciesAndDeploymentFiles_NameNoSeparateCacheServerOrClient`, `ApplicationTypes_ReachNoCacheLibraryAndCacheNoPageOutput` |
| 2 | Menü, kategori ağacı, ayarlar, ana sayfa blokları, site haritası | ✓ | `CacheArea.cs`; `TheAreas_AreTheFiveOf05_MenuCategoryTreeSettingsHomepageBlocksAndSitemap`, `EachArea_HasItsOwnSlot_AndItsSecondReadDoesNotLoad` |
| 3 | Kayıt sonrası, kesinleşmeden sonra hemen geçersiz kılma | ✓ | `TransactionRunner.cs:101–110`; `ASavedSetting_IsTheNextRead_AfterTheCommit_AndTheGenerationRisesByOne`, `TheRunner_IsWhereTheAreasAreDropped_AfterTheCommit` |
| 4 | Kuşak sayacı eski okumanın yazılmasını önler | ✓ | `AppCache.GetOrLoadAsync`; birim `AnInvalidationDuringTheLoad_…`, `ParallelReadersAndSaves_…`; tümleşik `AReadThatLoadedTheOldRowBeforeTheCommit_…`, `AReadBetweenTheWriteAndTheCommit_…`, `ParallelReadersAndSavesThroughTheRunner_…` |
| 5 | Zamana bağlı görünürlük her istekte hesaplanır | ✓ | `PatternAnnouncementSource.GetVisibleAsync`; `ADatedAnnouncementInTheCache_ShowsWhenItsRangeStarts_AndHidesWhenItEnds_WithoutAnInvalidationOrASecondLoad` |
| 6 | Fiyat/stok/sepet veritabanından; kapı her istekte; onay işlemi ayarı DB'den okur | ✓ | `PatternPriceSource` (önbelleksiz), `ReadForConfirmationAsync` (işlem içinde DB); `ThePriceShowsAtOnce_WhileTheCachedSettingIsOld_TheConfirmationReadsTheDatabase_AndThePageIsRenderedEveryTime` (TestServer + Kestrel) |
| 7 | Sayfanın tamamı önbelleğe alınmaz | ✓ | Kaynak + derlenen kural (`PageOutputText`, `ForbiddenOutputCachedPage`); sayfa testi: iki GET farklı `render`, `IOutputCacheStore` kayıtlı değil, `Age` yok |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi bütün maddeleri yeniden denetledi (önceki işçinin kodu dahil, `grep -n` + satır aralığı).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1 — bellekte, sunucu yok, tek örnek | ✓ | `AppCache.cs` (yerleşik `Lock`, kitaplık yok); `CacheSourceScanTests.DependenciesAndDeploymentFiles_…` (Directory.Packages.props, csproj, packages.lock.json, compose, Dockerfile, Caddyfile), `DependencyScan_OnAPackageLineOfEachRefusedName_Reports` (19 ad) |
| Kriter 2 — beş öğe | ✓ | `CacheArea.cs:9–16`; `AppCacheTests.TheAreas_AreTheFiveOf05_…` |
| Kriter 3 — kesinleşmeden sonra | ✓ | `TransactionRunner.cs:101–110` (commit → Invalidate), `:127–137` (ret/istisna unutur; kesinleştirme hatası atar); tümleşik `ASavedSetting_…`, `ARefusedSave_…`, `ASaveThatThrows_…`, `ACommitThatThrows_…`, `NamingAnAreaOutsideATransaction_IsRefused` |
| Kriter 4 — kuşak sayacı | ✓ | `AppCache.cs` (kuşak yükten önce, karşılaştırma+yazma kilit altında); tümleşik gerçek eşzamanlılık: bekletilen okuma yazamaz; yazım–kesinleşme arası okuma eski satırı doldurur, kesinleşme atar; yanlış sıra (`TheWrongOrder_…`) bayat bırakır; 4 okuyucu + 20 kayıt stres `ParallelReadersAndSavesThroughTheRunner_…`; geri alınan işlemde yarış `AReadDuringASaveThatIsThenRefused_…` |
| Kriter 5 — görünürlük her istekte | ✓ | `CachePattern.cs` `PatternAnnouncementSource`; `ADatedAnnouncementInTheCache_…` (`ManualTimeProvider`, sınır anları ±1 sn, tek yükleme, kuşak 0) |
| Kriter 6 — fiyat/kapı/onay | ✓ | `ThePriceShowsAtOnce_…` (SQL ile fiyat 120 → sayfada anında; ücret 80 + kapı kapalı SQL ile → önbellek 50 gösterir, onay işlemi 80 ve `IsSalesTemporarilyClosed=true` okur; işlem dışı onay okuması reddedilir); `ModuleBoundaryTests.CachePattern_…` (fiyat yolu `AppCache`/`CacheArea`'ya bağlı değil) |
| Kriter 7 — sayfa çıktısı önbelleksiz | ✓ | `CacheSourceScan.PageOutput()`; `Scan_OnTheRazorSample_…`, `CompiledRule_OnTheViolationSample_…`; sayfa testi iki farklı `render` |
| KK1 — ayrı önbellek istemcisi yok | ✓ | `DependenciesAndDeploymentFiles_NameNoSeparateCacheServerOrClient`, `DependencyScan_OnTheFrameworksCachePackagesAndOnComposeImages_…`, `ApplicationTypes_ReachNoCacheLibraryAndCacheNoPageOutput` |
| KK2 — beş anahtar; ikinci okuma DB'ye gitmez | ✓ | `TheSecondReadOfTheSettingsArea_DoesNotGoToTheDatabase_ButThePriceIsReadEveryTime` (komut yakalayıcı: ayar 1 sorgu, fiyat 2 sorgu); birim `EachArea_HasItsOwnSlot_…`; okuyan sayfalar T313 m.9, T361, T537'de |
| KK3 — kayıt sonrası yeni değer | ✓ | `ASavedSetting_IsTheNextRead_AfterTheCommit_AndTheGenerationRisesByOne` |
| KK4 — eski okuma yarışında bayat yazılmaz | ✓ | Kriter 4 satırındaki beş tümleşik + iki birim testi |
| KK5 — aralığı biten duyuru gizlenir | ✓ | `ADatedAnnouncementInTheCache_…` |
| KK6 — fiyat anında; onay DB değeri | ✓ | `ThePriceShowsAtOnce_…` (iki sunucuda) |
| KK7 — sayfa çıktısı önbelleğe alınmaz | ✓ | `ThePriceShowsAtOnce_…` (render farkı, `IOutputCacheStore` null, `ResponseCachingOptions` null, `Age` yok, `Cache-Control: public` yok) |
| `09 §25` — önbellek yalnız `AppCache`'te; geçersiz kılma kesinleşmeden sonra (§10.4) | ✓ | `ApplicationSource_KeepsTheCacheInItsPlaces_…`, `TheCacheTypes_AreDeclaredOnceEach_InTheInfrastructureNamespace`, `TheRunner_IsWhereTheAreasAreDropped_AfterTheCommit` |
| `09 §25` — önbellek yalnız §10.4'te (§20 m.9) | ✓ | `OtherCache()` taraması + `ApplicationTypes_HaveNoStaticFieldOfAMutableCollection_ButTheNamedLookupTables` + ihlal örneği `StaticFieldRule_OnTheViolationSample_…` |
| `09 §25` — modül sınırları (§6) | ✓ | `ModuleBoundaryTests.CachePattern_UnderTheModuleRules_IsClean_AndOnlyTheModuleReachesTheCache` (sayfa gerçek `Shopfolio.Pages.Storefront`'ta), `TheCache_InApplicationAssembly_IsCommonInfrastructureThatReachesNoModuleType` |
| `09 §25` — yeni kural için tarama + kasıtlı ihlal örneği (§19.2 m.3) | ✓ | Her kural için ayrı örnek satırı; `Scan_OnTheCodeSample_ReportsEachRuleAndEveryCallForm_…` (çağrı, koşullu çağrı, yöntem grubu, `nameof`, yansıma dizgesi, tam nitelikli ad, takma ad), yerlerin tam yol olduğu `Scan_AFileOfTheSameNameAsAPlaceElsewhere_IsNotAnAllowedPlace` |
| `09 §25` — test katmanı; gerçek saat yok (§19.1, §19.2) | ✓ | Tümleşik testler gerçek PostgreSQL; zaman `ManualTimeProvider`; eşzamanlılık `TaskCompletionSource` kapılarıyla, bekleme yok; birim testleri dosya yazmaz |
| `09 §25` — işlemde ağ çağrısı yok; okuma `AsNoTracking` + projeksiyon (§20 m.3, m.6) | ✓ | `CachePattern.cs` yüklemeleri `AsNoTracking().Select(...)`; değer `record` |
| `09 §3.5` — belgede olmayan süre/tavan sabiti | ✓ | `src/`'ye süre/tavan sabiti girmedi (önbellekte süre sonu yok) |
| Üst belgeyle çelişki (05 §7.10.1, §4.1.5, §3.1.2) | ✓ | 09 §10.4 m.3'e eklenen cümle yalnız sonucu bilinmeyen kesinleştirmede atmayı söyler; "kesinleştikten sonra geçersiz kılma" (§7.10.1) ve onayın DB'den okuması (§4.1.5) değişmedi; tek örnek (§3.1.2) korunur |
| Aynı adı/sayıyı anan öteki yerler | ✓ | `CONTEXT.md` Infrastructure satırı; `ITransactionRunner` ve `TransactionRunner` özetleri güncel |
| Güvenlik taraması | ✓ | Sır yok (diff taraması); yetki/oturum etkisi yok; yeni dış bağımlılık yok; örnek sayfa yalnız test projesinde |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release` (+ `run-script-tests.sh`) | 2826 / 2826 geçti; betik testleri 4 dosya geçti |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... -c Release`, on parçada (Web 3, Security, Concurrency+diğerleri, Jobs, Database, Model 3) | 1729 / 1729 geçti (`--list-tests` 1729) |
| Contract | — | `NOT_APPLICABLE` | — |

Derleme: `dotnet build Shopfolio.slnx -c Release -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç projede temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `6d25c8d`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-158) |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; main son üç tamamlanmış run 42294762521, 42294762382, 42276704951 `success`; hafızada T229 satırı var.

**Kriterler 1–7 ✓, KK1–KK7 ✓, `09 §25` ✓.** Kanıtlar (doğrulayıcı):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `AppCache` tekil (`AddSingleton`), yerleşik `Lock`, kitaplık yok; `Directory.Packages.props`, Web csproj, kilit dosyası, compose, Dockerfile'lar ve Caddyfile'da önbellek sunucusu/istemcisi yok (grep + `DependenciesAndDeploymentFiles_…`); Caddy yalnız `/gorsel/*` görsellerine tarayıcı başlığı yazar, ara önbellek tutmaz. Uygulama derlemesi çatının önbellek ad alanlarına bağlı değil (derlenen kural; `src`'de `IMemoryCache`/`OutputCache`/`ResponseCach` yok) |
| Kriter 2 / KK2 | ✓ | `CacheArea` = `Menu`, `CategoryTree`, `Settings`, `HomepageBlocks`, `Sitemap` (`09 §10.4` m.1 sırası); parametreli anahtar yok. İkinci okuma veritabanına gitmez: komut yakalayıcıyla ayar 1 sorgu, fiyat iki okumada 2 sorgu (`TheSecondReadOfTheSettingsArea_…`). `src`'de alanı okuyan ya da besleyen kayıt yazan kod henüz yok (grep: `AppCache`/`CacheInvalidations` yalnız `Infrastructure` + `Program.cs`); okuyucular T313 m.9, T361, T537'de |
| Kriter 3 / KK3 | ✓ | `TransactionRunner`: işlem başında küme boşaltılır; `CommitAsync` → sonra `Invalidate`; ret/iş istisnasında küme unutulur; `CacheInvalidations.Add` açık işlem yoksa reddeder. Testler: kayıt sonrası yeni değer ve kuşak +1; ret ve istisna kuşağı oynatmaz, kapsamın sonraki işi eski alanı atmaz; yazım–kesinleşme arası okuma eski satırı doldurur, kesinleşme atar (`AReadBetweenTheWriteAndTheCommit_…`); yanlış sıra bayat bırakır (`TheWrongOrder_…`); kaynak sırası `TheRunner_IsWhereTheAreasAreDropped_AfterTheCommit` |
| Kriter 4 / KK4 | ✓ | Kuşak yükten önce, karşılaştırma + yazma yuva kilidi altında, kilitte `await` yok. Gerçek eşzamanlılık: birimde ≥4 okuyucu × 3000 kayıt; tümleşikte gerçek PostgreSQL işlemleriyle 4 okuyucu + 20 kayıt (`ParallelReadersAndSavesThroughTheRunner_…`, okuyucunun gördüğü değer son kesinleşenin altına inmez) ve bekletilen eski okuma (`AReadThatLoadedTheOldRowBeforeTheCommit_…`). **Mutasyon denemesi:** kuşak karşılaştırması kaldırıldığında birim sınıfı kırmızı — belirlenimci test 3/3, paralel test 2/3 koşuda (değişiklik geri alındı, tekrar 15/15 yeşil) |
| Kriter 5 / KK5 | ✓ | Önbellek duyuruyu aralığıyla taşır; görünürlük her çağrıda `TimeProvider`'dan; `ADatedAnnouncementInTheCache_…` Türkiye saatiyle sınırın ±1 sn'sinde görünür/gizlenir, tek yükleme, kuşak 0 |
| Kriter 6 / KK6 (para) | ✓ | Kendim sınadım: ücret 80 ve kapı kapalı doğrudan SQL ile yazılınca gösterim önbelleği 50'yi tutmaya devam eder (testte aynı kapsamda `GetForDisplayAsync` 50), onayın işlemi aynı anda 80 ve `IsSalesTemporarilyClosed=true` okur; işlem dışındaki onay okuması reddedilir; fiyat (120) sayfada anında — TestServer ve gerçek Kestrel. Tutarı etkileyen başka okuma yolu kaynakta yok: `src`'de `AppCache`'i kullanan kod yok, `CompanySettings` yalnız `AppDbContext`'te tanımlı (gerçek onay işlemi ileriki task'ların, onlar `09 §10` → §10.4 m.5'i okur); fiyat yolu `AppCache`/`CacheArea`'ya bağlı değil (`ModuleBoundaryTests.CachePattern_…`) |
| Kriter 7 / KK7 | ✓ | Sayfa iki istekte farklı `render` değeri; `IOutputCacheStore` ve `ResponseCachingOptions` kayıtlı değil, `Age` yok, `Cache-Control: public` yok; kaynak ve derlenen kural ihlal örnekleriyle kırmızı |
| Sonucu bilinmeyen kesinleştirme (⚠ K1) | ✓ | `commitSent && !committed` hâlinde alanlar `finally`'de geri almadan önce atılır; atma hep kesinleştirme denemesinden sonradır, kesinleşmeden önce atma yolu yok. `09 §10.4` m.3'e eklenen cümle `05 §7.10.1`'den ayrılmaz: 05 "kesinleştikten sonra hemen geçersiz kılınır" der ve fazladan atmayı yasaklamaz; sunucu kesinleştirip bağlantı düştüğünde unutmak 05'i bozardı (değer süre sonu olmadan bayat kalırdı). 05'in bıraktığı kalıp `09`'undur (§7.10.1 son cümle) — karar kaydı gerekmez. Test `ACommitThatThrows_…` yalnız o bağlantıyı `pg_terminate_backend` ile keser |
| `09 §6` modül sınırı | ✓ | `CachePattern.cs` gerçek ad alanlarında (`Shopfolio.Modules.Settings`/`.Contracts`, `CorporateContent`, `Catalog`, `Shopfolio.Pages.Storefront`); `ModuleBoundaryTests` sayfa, Contracts, internal ve başvuru kurallarını bu türlerle koşar; sayfa yalnız `Contracts` arayüzlerine bağlı |
| Taramalar ve ihlal örnekleri | ✓ | Metin taraması (tür yerleri, `Invalidate`/`TakePendingAreas` yalnız koşucuda — çağrı, koşullu çağrı, yöntem grubu, `nameof`, yansıma dizgesi —, başka önbellek, sayfa çıktısı), bağımlılık taraması (19 ad), derlenen kural (takma ad dahil) ve statik koleksiyon kuralı (yedi biçim) ayrı ihlal örnekleriyle kırmızı; yerler tam yol. Darlık ve belge dayanağı K1'de |
| `09 §3.5`, bellek | ✓ | `src`'ye süre, tavan ya da ayar sabiti girmedi (süre sonu yok, ısıtma yok — §10.4 m.2). Bellek beş sabit yuvayla sınırlı; parametreli anahtar olmadığından büyümez, değerin boyu kaydın kendisidir (`09 §20` m.1 kodda hacim tavanı istemez) |
| Güvenlik | ✓ | sır yok; yeni paket yok (kilit dosyası değişmedi); yetki ve girdi yüzeyi değişmedi; örnek sayfa yalnız test projesinde |

**Testler (doğrulayıcı, yerel):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` üç projede temiz; Unit 2826/2826 (Consistency 207 + diğer 2619); betik testleri 4/4; Integration 1729/1729 dokuz parçada (Concurrency+Database+Disposal 79, FileSecrets+Files+Jobs 134, Logging+Outages+SignedEndpoints+Security 81, Web üç parça 229+161+152, Model üç parça `--max-threads 3` 324+307+262; PostgreSQL 18 konteyneri, her parça ≤ 147 sn). Dal CI: run 42300942345 (head `6d25c8d`) bütün işler yeşil. Koşu sonrası `git status` temiz, `dotnet restore --locked-mode` çıkış 0.

**Task satırları (`12 §7.5`):** `11 §7.1`'de T229'un kaynak birimleri `05` MK-34, §7.10 ve `09` §10, §20'dir (`10 KP` birimi yok); bu birimlerin grep'i `12`'de satır getirmez. Konuyla ilgili tek satır `VAL-A835`'tir (kaynağı K-980 (5) · 05 §7.3): ön koşulu "tarihi geçen indirim", kanıt türü "Uç yanıtı" — indirim ve sayfa task'larının ucu; T229 yalnız kalıbı ve kuşak/kesinleşme testlerini kurar → `BEKLEMEDE` kalır (T223 emsali). Matris betiği hata 0.

**K1 → D-158:** `09 §20` m.9'un yorumu (`Lazy<BusinessCalendar>` istisnası, kuraldan önceki sekiz statik tablo, `ConcurrentDictionary` ve `Lazy<`'in her konumda yasağı) yalnız test kodunda yazılı; tarama koleksiyon olmayan statik değer tutmayı (çalışırken atanan statik kayıt, yeniden atanan değişmez sözlük) yakalamaz.

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Tek fark: ⚠ K3'ün "hepsi başlatıcıda sabitlerden bir kez doldurulur" cümlesi tam değil — `FieldMessageCatalog` bir satırı statik yapıcıda ekler, `ProductConstantNameGuard` adları yansımayla okur (ikisi de bir kez ve kayıttan bağımsız; D-158'de).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (yerleşik `System.Threading.Lock`)

## Commit & PR

- **Branch:** `task/T229-uygulama-bellegi-onbellek`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #414
- **CI:** yapım chat'i beklemedi (yönetici düzeni); yönetici izler
- **Post-merge run'lar:** squash `f4da03c` (#414) — CI 42306192016 success, Publish 42306192052 success (F2 gate'te dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok — kütüphane eklenmedi; kilit ve sayaç yerleşik araçlardır (`09 §10.4` giriş, §3.3).

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## ⚠ Kararlar (öneriyle verildi)

| # | Karar | Gerekçe |
|---|---|---|
| K1 | Kesinleştirme çağrısı hata verirse alanlar atılır (`TransactionRunner` finally; `09 §10.4` m.3'e bir cümle) | Sonuç bilinmez; süre sonu olmadığından tutulan değer yeniden başlatmaya kadar bayat kalabilirdi; atmak yalnız yeniden yükletir. 05 §7.10.1'in "kesinleştikten sonra" kuralını değiştirmez (kesinleşmeden önce atma yok). Test: `ACommitThatThrows_…` (yalnız o bağlantı `pg_terminate_backend(pid)` ile kesilir) |
| K2 | `Lazy<BusinessCalendar>` (`Infrastructure/BusinessCalendar.cs`) taramanın tek istisnası; tam yol + tam tür adı | Uygulamayla gelen gömülü tatil dosyasını tutar, kayıt değildir; geçersiz kılınacak bir şeyi yoktur. Aynı dosyada başka `Lazy<>` ve başka dosyada `Lazy<BusinessCalendar>` kırmızı (`Scan_InTheCalendarFile_SparesOnlyTheCalendarsOwnLazy`, `Scan_AFileOfTheSameNameAsAPlaceElsewhere_…`) |
| K3 | "Statik sözlük" (§20 m.9) için yansımayla kural: statik değiştirilebilir koleksiyon alanı yok; kuraldan önce var olan sekiz arama tablosu tam adla listelenir, liste alanla birebir eşitlenir | Hepsi başlatıcıda sabitlerden bir kez doldurulur ya da (`LockSequence.States`) canlı bağlamın kilit durumunu tutar — veritabanından okunan değer değil. Yeni tablo salt okunur türle (`IReadOnly*`, `Frozen*`) yazılır. İhlal örneği yedi biçim (`StaticFieldRule_OnTheViolationSample_…`) |
| K4 | Kalıp örneğinde alan, yazan deyimden önce adlandırılır | İşlem dışı çağrı hiçbir şey yazılmadan reddedilir; yazmayan deyim yalnız zararsız bir atma getirir (ret sonucu kümeyi unutturur) |

## Known Limitations / Follow-up

> Ertelenen her kalem `Docs/DEFERRED_BACKLOG.md`'ye **yazılır**. Yalnız burada kalan iş buharlaşır.

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Hedefi T229 olan backlog kaydı yoktu; yeni kayıt açılmadı. Alanları okuyan sayfalar ve alanı besleyen kayıtların yazıcıları planda kendi task'larındadır (T313 m.9, T361, T537; `09 §10.4` m.3 "hangi yazımın hangi alanı attığı task'ın tasarımında") | — | — |

## Notlar

- Working tree: temiz (devralınan dal `045237a`). hooksPath `scripts/git-hooks`.
- Ana dal CI (başlangıç kapısı, ilk işçi): son üç tamamlanmış run success — 42276704951, 42276704953, 42230581953. Bu işçide main: CI 42294762382, Publish 42294762521 (T219 merge `4ce191c`) yeşil.
- Bağımlılık T219 ✓ (status).
- Diğer: T219 raporunun "Post-merge run'lar" satırı ve status Commit hücresi (`4ce191c`) bu dalda dolduruldu.
