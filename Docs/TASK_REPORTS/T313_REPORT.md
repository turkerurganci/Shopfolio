# T313 — Site haritası ve robots.txt

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-07

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Adlar:** `AddressRules.SitemapFile` ve `RobotsFile` sabitleri; `ProtocolFiles` bunlardan türer. Dosya adı yalnız burada geçer (`SearchEngineSourceScan` kuralı korundu, mesajı güncellendi).
- **İçerik:** `Infrastructure/SearchEngine/SiteMapFiles` — `FixedListPaths` (`/`, `/hizmetler`, `/referanslar`, `/sss`, `/iletisim`, `/islem-rehberi`), `Compose` (modüllerin yolları + sabit liste; kapalı yol düşer, tek küçük harfli biçim, tekil, sıralı), `Xml` (UTF-8, BOM yok, mutlak adres `SiteAddress` ile = kurulumun alan adı), `Robots` (tek `Sitemap:` satırı, `Disallow` yok). Tablo okumaz, modül türü anmaz.
- **Modül verisi:** `ICatalogModule.ListSitemapPathsAsync` (yayındaki ürün + tüm kategori; `CatalogSitemapReader`) ve `ICorporateContentModule.ListSitemapPathsAsync` (yayındaki hizmet, referans, genel sayfa; yayındaysa `/hakkimizda`; `CorporateContentSitemapReader`). Arşiv ve taslak ürün okunmaz.
- **Önbellek:** `Settings.Contracts.ISitemapSource` + `SitemapView` + `Settings/SitemapSource` — iki modülün yollarını birleştirir, `AppCache` `CacheArea.Sitemap` yuvasında tutar; ilgili kayıt `CacheInvalidations.Add(CacheArea.Sitemap)` ile düşürür.
- **Uç:** `Endpoints/SitemapEndpoint` (`MapSitemapEndpoints`, `Program.cs`): yalnız GET; `application/xml` ve `text/plain`; çerez, oturum, `X-Robots-Tag` yok (`MachineEndpointMetadata`).
- **Test kaldırıldı:** T219'un `SitemapAndRobotsTxt_HaveNoEndpointInThisTask` testi (uç artık var; yerine `SitemapEndpointTests`).
- **Belgeler:** `09 §9.4` madde 6 (site haritası cümlesi T313 ile güncellendi; belge kuralı değişmedi, `05 §3.7.3-4` ve `07 UC-79` ile uyumlu → karar kaydı gerekmedi), `.claude/CONTEXT.md` (arama motoru satırı, `Endpoints/` satırı), `IMPLEMENTATION_STATUS` T313 ⏳, `DEFERRED_BACKLOG` D-161, `MEMORY.md` Güncel Durum.

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

- **Branch:** `task/T313-site-haritasi`
- **Commit:** son commit PR'da
- **PR:** PR açıklamasında
- **CI:** yapım chat'i izlemez
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; ayrı `chore:` PR'ında veya sonraki task dalında)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok (paket, plan/tier, dış servis yok) | `git diff origin/main --stat` paket dosyası içermez | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Aydınlatma metni ve çerez politikası haritada yok (yayına alınmadıkça 404) | D-161 (a) | Yayın bilgisi ilgili sayfa task'ında |
| 2 | Kaydı yazan task'lar `CacheInvalidations.Add(CacheArea.Sitemap)` çağırmalı | D-161 (b) | T313 yazıcı eklemez; kalıp testte gösterildi |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** CI 42335449717 success, Publish 42335449709 success (F2 gate PR #415, `6efa235`).
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ kararlar (kurala en uygun seçenekle verildi):** (a) "sabit liste sayfaları" `05`/`07`'de sayılmamış: `/`, hizmetler, referanslar, sss, iletisim, islem-rehberi seçildi; `/hakkimizda` yayındaysa girer (D-161 a); (b) 50.000 URL sınırı uygulanmadı (`05`'te sınır yok, küçük mağaza); (c) birleştirme katmanı Settings'te (iki modülün Contracts'ını birleştirir); (d) kategori haritaya her zaman girer (kategori yayın durumu yok).
- **Önceki task kaydı (skill adım 15):** T229 post-merge satırı ve status Commit hücresi dolu; ek işlem yok.
- **Önceki işçi:** işin büyük kısmı önceki yapım işçisince yapıldı (bağlam sınırında bölündü); bu devam kalanı (biçim denetimi, belgeler, rapor, öz denetim) tamamladı.
