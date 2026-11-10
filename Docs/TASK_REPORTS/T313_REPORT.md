# T313 — Site haritası ve robots.txt

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-07

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Adlar:** `AddressRules.SitemapFile` ve `RobotsFile` sabitleri; `ProtocolFiles` bunlardan türer. Dosya adı yalnız burada geçer (`SearchEngineSourceScan` kuralı korundu, mesajı güncellendi).
- **İçerik:** `Infrastructure/SearchEngine/SiteMapFiles` — `FixedListPaths` (`/`, `/hizmetler`, `/referanslar`, `/sss`, `/iletisim`, `/islem-rehberi`), `Compose` (modüllerin yolları + sabit liste; kapalı yol düşer, tek küçük harfli biçim, tekil, sıralı), `Xml` (UTF-8, BOM yok, mutlak adres `SiteAddress` ile = kurulumun alan adı), `Robots` (tek `Sitemap:` satırı, `Disallow` yok). Tablo okumaz, modül türü anmaz.
- **Modül verisi:** `ICatalogModule.ListSitemapPathsAsync` (yayındaki ürün + tüm kategori; `CatalogSitemapReader`) ve `ICorporateContentModule.ListSitemapPathsAsync` (yayındaki hizmet, referans, genel sayfa; yayındaysa `/hakkimizda`; `CorporateContentSitemapReader`). Arşiv ve taslak ürün okunmaz.
- **Önbellek:** `Settings.Contracts.ISitemapSource` + `SitemapView` + `Settings/SitemapSource` — iki modülün yollarını birleştirir, `AppCache` `CacheArea.Sitemap` yuvasında tutar; ilgili kayıt `CacheInvalidations.Add(CacheArea.Sitemap)` ile düşürür.
- **Uç:** `Endpoints/SitemapEndpoint` (`MapSitemapEndpoints`, `Program.cs`): yalnız GET; `application/xml` ve `text/plain`; çerez, oturum, `X-Robots-Tag` yok (`MachineEndpointMetadata`).
- **Test kaldırıldı:** T219'un `SitemapAndRobotsTxt_HaveNoEndpointInThisTask` testi (uç artık var; yerine `SitemapEndpointTests`).
- **Belgeler:** `09 §9.4` madde 6 (site haritası cümlesi T313 ile güncellendi; belge kuralı değişmedi, `05 §3.7.3-4` ve `07 UC-79` ile uyumlu → karar kaydı gerekmedi), `.claude/CONTEXT.md` (arama motoru satırı, `Endpoints/` satırı), `IMPLEMENTATION_STATUS` T313 ⏳, `DEFERRED_BACKLOG` D-163, `MEMORY.md` Güncel Durum.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Endpoints/SitemapEndpoint.cs`, `Infrastructure/Routing/AddressRules.cs`, `Infrastructure/SearchEngine/{SiteMapFiles,SearchEngineClosure}.cs`, `Program.cs`
- `Modules/Catalog/{CatalogSitemapReader,CatalogModule}.cs`, `Contracts/ICatalogModule.cs`; `Modules/CorporateContent/{CorporateContentSitemapReader,CorporateContentModule}.cs`, `Contracts/ICorporateContentModule.cs`; `Modules/Settings/{SitemapSource,SettingsModule}.cs`, `Contracts/ISitemapSource.cs`
- `tests/Shopfolio.UnitTests/Infrastructure/{SiteMapFiles,SitemapSource}Tests.cs`, `Architecture/{ModuleBoundaryTests,SearchEngineSourceScan,SearchEngineSourceScanTests}.cs`; `tests/Shopfolio.IntegrationTests/Web/{SitemapEndpointTests,SearchEngineDataTests}.cs`
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`
- Varlık, göç ve paket yok.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki adres istek anında üretilir, 200 | ✓ | `TheTwoAddresses_Answer200_WithTheirTypes_NoByteOrderMark_AndAreNotClosedToTheSearchEngines` (TestServer + Kestrel), `TheTwoAddresses_AreGetsOnly` |
| 2 | Harita yayındaki ürün, kategori, içerik sayfaları ve sabit liste sayfalarını taşır | ✓ | `TheMap_HasTheFourLists_...`; `Compose_PutsTheFixedPagesFirst_ThenTheRecordsInPathOrder_EachOnce`; `CatalogSitemapReader`, `CorporateContentSitemapReader` |
| 3 | Arşiv, taslak ve kapalı sayfa girmez | ✓ | `TheMap_HasTheFourLists_...` (arşiv, taslak, kapalı sayfa yok); `Compose_LeavesOutAClosedPath`; `AClosedPathFromAModule_NeverReachesTheMap` |
| 4 | Adresler küçük harfli tek biçim | ✓ | `EveryAddress_IsLowerCase_AndCarriesTheInstallationDomainAndNotTheRequestHost`; `Compose_MakesEveryAddressOneLowerCaseForm_AndDropsWhatIsNotAPagePath` |
| 5 | `/robots.txt` yalnız harita adresi, kurulum alan adıyla; kapalı sayfa engellenmez | ✓ | `Robots_IsOneLineWithTheMapAddress_BlocksNothing_AndNotTheArchivedProduct`; `Robots_IsOneLineWithTheSiteMapAddressOfTheInstallation_AndHasNoRuleThatBlocksAPage` |
| 6 | Kapalı sayfa HTML'de robots meta (T219 kalıbı) | ✓ | T219 `ClosedPages_CarryTheRobotsMetaTagInTheHtml_AndTheOpenOnesDoNot` (`SearchEngineDataTests`) yeşil; T313 uçları kalıba girmez (`X-Robots-Tag` yok) |
| 7 | HTML olmayan yanıt `X-Robots-Tag: noindex` (T219 kalıbı) | ✓ | T219 `ClosedAnswersThatAreNotHtml_CarryOneXRobotsTagNoindex_AndTheOpenOnesCarryNone`; iki T313 ucu açık, başlık taşımaz (kriter 1 testi) |
| 8 | Haritada olmayan arşiv ürün adresi `/robots.txt`'te engellenmez | ✓ | `Robots_IsOneLineWithTheMapAddress_BlocksNothing_AndNotTheArchivedProduct` (sayfa testi T549) |
| 9 | Bellek önbelleği; kayıt kaydedilince geçersiz | ✓ | `TheMap_ComesFromTheCache_ASavedProductThatNamesTheAreaDropsIt_AndARolledBackSaveDoesNot`; `SitemapSourceTests` (`TheSecondCall_ComesFromTheCache_...`, `AnInvalidationOfTheSiteMapArea_...`, `AnInvalidationOfAnotherArea_...`) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1–k9 | ✓ | Yukarıdaki tablo; bütün maddeler bu devamda diff'te yeniden denetlendi |
| KK1: iki adres 200 | ✓ | `TheTwoAddresses_Answer200_...` |
| KK2: dört liste haritada | ✓ | `TheMap_HasTheFourLists_...` (ürün, kategori, içerik, sabit liste) |
| KK3: arşiv, taslak, kapalı sayfa yok | ✓ | `TheMap_HasTheFourLists_...`, `Compose_LeavesOutAClosedPath` |
| KK4: büyük harf yok | ✓ | `EveryAddress_IsLowerCase_...` |
| KK5: robots tek satır, `Disallow` yok | ✓ | `Robots_IsOneLineWithTheMapAddress_BlocksNothing_...` |
| KK6: hesap ve panel sayfasında meta | ✓ | T219 `ClosedPages_CarryTheRobotsMetaTag...` (hesap, panel dahil) |
| KK7: indirme yanıtında başlık | ✓ | T219 `ClosedAnswersThatAreNotHtml_CarryOneXRobotsTag...` |
| KK8: robots arşiv ürünü engellemez | ✓ | `Robots_..._AndNotTheArchivedProduct` |
| KK9: ürün kaydı önbelleği düşürür | ✓ | `TheMap_ComesFromTheCache_ASavedProductThatNamesTheAreaDropsIt_...` |
| `09 §25` — belge uyumu | ✓ | `05 §3.7.3-4`, `07 UC-79`, `02 §3.30.5-6` okundu; `09 §9.4` m.6 güncel; ad yalnız `AddressRules`'ta |
| `09 §25` — mimari ve kod | ✓ | Modül sınırı: haritayı Settings birleştirir, modüller yalnız kendi yollarını verir (`ModuleBoundaryTests.SiteMap_...`); yeni paket yok; `SiteMapFiles` tablo okumaz |
| `09 §25` — veri/göç | uygulanamaz | Varlık, göç, DB varsayılanı, FK dizini yok |
| Sık FAIL (1) bekçi/tarama istisnası | ✓ | Yeni istisna yok; `SearchEngineSourceScan` ad kuralı aynı, ihlal örneği `SearchEngineSourceScanTests`'te |
| Sık FAIL (2) çağrı biçimi | ✓ | Dosya adı yalnız `AddressRules` sabitlerinde; tarama literali her biçimde yakalar (mevcut kural, mesajı güncellendi) |
| Sık FAIL (3) 06 default / FK | uygulanamaz | Tablo yok |
| Sık FAIL (4) sayıyı anan yerler | ✓ | T219 testi silindi, tarama mesajı, `SearchEngineClosure` özeti, `CONTEXT.md` (iki satır), `09 §9.4`, status, backlog, memory |
| Sık FAIL (5) karar kaydısız belge kuralı | ✓ | 09 metni belge kuralını değiştirmez; yalnız T313'ün yerini yazar |
| Güvenlik taraması | ✓ | Sır yok; girdi alan yok (salt GET, sorgu/başlık okunmaz); adresler kurulumun alan adından, istek ana bilgisayarından değil (açık yönlendirme/başlık enjeksiyonu yok); XML `XmlWriter` ile kaçışlı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit Architecture | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --filter-namespace Shopfolio.UnitTests.Architecture` | 764/764 |
| Unit yeni | ✓ | `--filter-class *SiteMapFilesTests --filter-class *SitemapSourceTests` | 24/24 |
| Unit diğer | ✓ | önceki devam: bütün Unit (Consistency hariç) parçalı | 2087 geçti |
| Consistency | ✓ | `--filter-trait "Consistency=true"` | 207/207 |
| Integration | ✓ | PostgreSQL 18, `shopfolio-t313-pg`; `SitemapEndpointTests` 10/10; AddressRouting+SearchEngineData+AppCache 72; SecurityHeaders+Factory+Liveness+CookiePolicy 39; RequestLimits+UnexpectedError+Forgery+SessionAccess 112 | hepsi geçti |
| Biçim ve derleme | ✓ | `dotnet build -c Release -warnaserror` (Web, UnitTests, IntegrationTests); `dotnet format Shopfolio.slnx --no-restore --verify-no-changes --include <değişenler>` | 0 uyarı, 0 hata, format çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `336f075`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (K1–K3 bu PR'da kapatıldı — kayıt bu bölüm; açık uçlar zaten D-163) |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u (`C:\projects\Shopfolio\scripts\git-hooks`); main son üç tamamlanmış run 42335449709, 42335449717, 42306192052 `success`; hafızada T313 satırı var. Dal CI 42339118325 `success` (head `336f075`).

**Kriterler 1–9 ✓, KK1–KK9 ✓, `09 §25` ✓.** Kanıtlar (doğrulayıcı):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `SitemapEndpoint` iki `MapGet` (`Program.cs` `MapSitemapEndpoints`); istek anında `ISitemapSource` + `SiteMapFiles`; `TheTwoAddresses_Answer200_…` (TestServer + Kestrel) 200, `application/xml`/`text/plain` UTF-8, BOM yok; POST 405/400 |
| Kriter 2 / KK2 | ✓ | `CatalogSitemapReader` (yayındaki ürün `PublishStatus.Published`, tüm kategoriler — kategoride yayın durumu yok, boş kategori 200'dür `07 §4.2.1`), `CorporateContentSitemapReader` (yayındaki hizmet, referans, genel sayfa; yayındaysa `/hakkimizda` — taslak Hakkımızda 404, `07 §4.8.1`); sabit liste `/`, `/hizmetler`, `/referanslar`, `/sss`, `/iletisim` (+`/islem-rehberi`) hep 200 (`07 §4.1`, §4.7, §4.9, §4.10, §4.11). `TheMap_HasTheFourLists_…` gerçek PostgreSQL'de |
| Kriter 3 / KK3 | ✓ | Okuyucular yalnız `Published` okur; `Compose` `SearchEngineClosure.IsClosedPath` ile kapalıyı düşürür; test arşiv/taslak (ürün, hizmet, referans, sayfa) ve on kapalı kökü dışarıda bulur; `Compose_LeavesOutAClosedPath` (13 durum), `AClosedPathFromAModule_NeverReachesTheMap` |
| Kriter 4 / KK4 | ✓ | `OneForm` küçük harf, sondaki `/` yok, sorgu/parça/çift eğik çizgi düşer (`07 §2.1.7`); `EveryAddress_IsLowerCase_…` — sahte `Host` başlığı yansımaz, adres kurulumun alan adı |
| Kriter 5 / KK5 | ✓ | `SiteMapFiles.Robots` = `Sitemap: https://<alan adı>/sitemap.xml\n`; `Robots_IsOneLine…` tek satır, `Disallow` yok |
| Kriter 6 / KK6 | ✓ | T219 kalıbı değişmedi: panel HTML'i robots meta (`ClosedPages_CarryTheRobotsMetaTagInTheHtml_…`); hesap kökü adresle kapalı (`IsClosedPath_DependsOnTheFirstSegmentOnly` `/hesap/adresler`, `/giris`). Hesabın HTML ekranları henüz yok (oturumsuz istek 302 + `X-Robots-Tag`) — K1 |
| Kriter 7 / KK7 | ✓ | T219 ara katmanı adrese bakar: CSV dışa aktarma ve panel 404'ü tek `X-Robots-Tag: noindex`; `/indir` kökü kapalı (`IsClosedPath` `/indir/abc`). İndirme ucunun kendisi T312'nin — K3. İki T313 ucu başlık taşımaz |
| Kriter 8 / KK8 | ✓ | Robots yalnız harita satırı; test arşiv ürün adresini, `/panel`, `/hesap`'ı içermediğini doğrular |
| Kriter 9 / KK9 | ✓ | `SitemapSource` `AppCache.GetOrLoadAsync(CacheArea.Sitemap)`; değer değişmez `SitemapView` (`09 §10.4` m.1). `TheMap_ComesFromTheCache_…`: işlem dışı yazım önbelleği düşürmez, geri alınan kayıt düşürmez, `CacheInvalidations.Add(CacheArea.Sitemap)` adlı kesinleşen ürün kaydı kuşağı +1 yapar ve yeni ürün haritada. Üretimde kayıt yazıcı henüz yok; yazıcı task'larının yükümlülüğü D-163 (b) |
| `09 §6` modül sınırı | ✓ | Uç yalnız `Settings.Contracts.ISitemapSource`; önbellek Settings modülünde; modüller yolları `Contracts` ile verir (`ModuleBoundaryTests.SiteMap_…`) |
| `09 §9.4` m.6, tarama | ✓ | Dosya adları yalnız `AddressRules.SitemapFile`/`RobotsFile`; `SearchEngineSourceScan` literali yakalar (ihlal örneği testte) |
| Hız sınırı (`07 §4.32.1` "sayfa") | ✓ | `RateLimitScope.IsMachineEndpoint` yalnız `/sistem`, `/indir`, parçalı yükleme — iki adres sayfa sınırında |
| Güvenlik | ✓ | Sır yok; yeni paket yok (kilit dosyası değişmedi); girdi yok (parametresiz GET); adres istek ana bilgisayarından değil; XML `XmlWriter` ile kaçışlı; uçlar spec gereği açık |

**Task satırları (`12 §7.5`):** `KP-37` (`11 §7.1`: T219, T313, T363). T313'ün uç yanıtıyla karşılanan dört satır `PASS`: `VAL-A378` (dört liste), `VAL-A379` (arşiv ve taslak yok), `VAL-A381` (XML, kurulumun alan adı), `VAL-A398` (robots engellemez, harita adresi). `VAL-A380` (kayıt değişince güncelleme — panel yazıcıları yok), `VAL-A395`/`VAL-A396` (sayfanın 404'ü / arşiv sayfasının açılışı — T549), `VAL-A397` (indirme ucu T312) ve öteki KP-37 satırları `BEKLEMEDE`. Matris betiği (`--kapsam 1-77`) hata 0.

**Testler (doğrulayıcı, yerel):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı/0 hata · `dotnet format --verify-no-changes` Web ve IntegrationTests projesi çıkış 0, UnitTests değişen beş dosya çıkış 0 (proje bütünü 150 sn'ye sığmadı) · Unit Architecture 764 + öteki 2087 = 2851 · betik testleri 4 dosya · Integration gerçek PostgreSQL 18 (`shopfolio-t313-val-pg`, port 55640): MigrationBundle 2, Sitemap+SearchEngineData 28, Web A–E 80, F 67, J+L 82, M–R 161, S–W 150, Pages+Security+Logging 62, Concurrency+Disposal+FileSecrets+Files 101, Jobs+Outages+SignedEndpoints 96, Database 36, Model (3 parça, `--max-threads 3`) 367+309+217 — hepsi geçti · `git status` temiz, `dotnet restore --locked-mode` 0.

**Bloke-etmeyen notlar:**

| # | Not | Karar |
|---|---|---|
| K1 | Öz denetimde KK6 kanıtı "hesap dahil" deniyor; T219 testi yalnız panel, süzgeç ve arama sayfasını açar. Hesap kökünün kapalılığı adres kuralının birim testinden gelir; hesabın HTML ekranları kendi task'larında | bu PR'da kapatıldı (kanıt bu bölümde düzeltildi) |
| K2 | Harita `/` ve `/islem-rehberi`'yi de taşır; `02 §3.30.2`'nin beş liste sayfasından geniş, ama ikisi de hep 200 ve kapalı değil — `07 §4.32.1` yasaklamaz | bu PR'da kapatıldı (değişiklik gerekmez) |
| K3 | KK7'nin "indirme yanıtı" bugün yalnız adres kuralıyla sınanır; `/indir` ucu T312'nin, başlık oradaki uç yanıtında görünür | bu PR'da kapatıldı (T312'nin kendi kriteri; `VAL-A397` `BEKLEMEDE`) |

**Yapım raporu karşılaştırması:** kriter ve KK sonuçları uyumlu; tek uyuşmazlık K1 (KK6 kanıt cümlesi). Öz denetim tablosu var ve her maddesi kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T313-site-haritasi`
- **Commit:** son commit PR'da
- **PR:** #417
- **CI:** yapım chat'i izlemez
- **Post-merge run'lar:** squash `f94b4d8` (#417) — CI 42346300012 success, Publish 42346300006 success (T305 yapımında dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok (paket, plan/tier, dış servis yok) | `git diff origin/main --stat` paket dosyası içermez | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Aydınlatma metni ve çerez politikası haritada yok (yayına alınmadıkça 404) | D-163 (a) | Yayın bilgisi ilgili sayfa task'ında |
| 2 | Kaydı yazan task'lar `CacheInvalidations.Add(CacheArea.Sitemap)` çağırmalı | D-163 (b) | T313 yazıcı eklemez; kalıp testte gösterildi |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** CI 42335449717 success, Publish 42335449709 success (F2 gate PR #415, `6efa235`).
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ kararlar (kurala en uygun seçenekle verildi):** (a) "sabit liste sayfaları" `05`/`07`'de sayılmamış: `/`, hizmetler, referanslar, sss, iletisim, islem-rehberi seçildi; `/hakkimizda` yayındaysa girer (D-163 a); (b) 50.000 URL sınırı uygulanmadı (`05`'te sınır yok, küçük mağaza); (c) birleştirme katmanı Settings'te (iki modülün Contracts'ını birleştirir); (d) kategori haritaya her zaman girer (kategori yayın durumu yok).
- **Önceki task kaydı (skill adım 15):** T229 post-merge satırı ve status Commit hücresi dolu; ek işlem yok.
- **Önceki işçi:** işin büyük kısmı önceki yapım işçisince yapıldı (bağlam sınırında bölündü); bu devam kalanı (biçim denetimi, belgeler, rapor, öz denetim) tamamladı.
