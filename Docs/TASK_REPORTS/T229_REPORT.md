# T229 — Uygulama belleğinde önbellek

**Faz:** F2 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-07

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
| Doğrulama durumu | *(doğrulama chat'i doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

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
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

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
