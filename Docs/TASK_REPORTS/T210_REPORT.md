# T210 — Liste kalıbı

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Sayfalama:** `Infrastructure/{PageNumber,PageSizes,PagedList,Paging}.cs`. `PageNumber.Parse` metni okur (ASCII rakam dışı, sıfır, negatif: 1; `int` aralığını aşan: `int.MaxValue`); `Paging.ToPageAsync(IOrderedQueryable<T>, page, pageSize, ct)` sayar, sayfayı son sayfaya kırpar, `Skip/Take` ile okur; `PageSizes` yalnız `04` 2.13.3'ün dört sayısını taşır (24/24/25/25).
- **Arama sözcüğü:** `SearchTerm.ToLikePattern` (kırp, sadeleştir, 150'ye kes, `\ % _` kaçışı, başa-sona `%`); boş sözcük `null`.
- **Sorgu modeli ve parametre adları:** `Lists/PagedQuery` (`Page` metin, `[FromQuery(Name = "page")]`), `Lists/QueryParameterNames` (alan adının camelCase'i). Sayfa boyu parametre değildir; sıralama parametresi yoktur.
- **Süzgeç okuma:** `Lists/ListFilters` (`ReadPrice`, `ReadDates`, `ReadInStock`, `ReadCode(s)`); geçersiz değer süzgeç uygulamaz ve hata değildir. Ters fiyat/tarih aralığı `PriceRangeResults` / `DateRangeResults` ile `IValidatableObject` yolundan alan mesajı olur (`InvalidPriceRange` alt değer alanına, `InvalidDateRange` bitiş tarihi alanına). `RequestFormat` (tr-TR sayı, `yyyy-AA-GG` tarih) T216'nın genişleteceği ilk hâliyle kuruldu.
- **Liste adresi ve dönüş:** `Lists/ListAddress.Of` (kök + dolu alanlar camelCase + `page` yalnız birinci sayfa değilse), `Lists/ListReturn` (`LinkTo`, `Target`), `Security/ReturnPath.ResolveOr` (`Resolve` onu kullanır; tek yerel yol denetimi).
- **Kapalı işareti:** `Lists/ListIndexing` + `ListIndexingFilter` + `ListSetup.AddListPattern` (`Program.cs`): `page` dışında parametre taşıyan adres ve arama sayfası `ClosedToSearchEngines`. `noindex` etiketi T219'un.
- **Testler:** `UnitTests/Lists/*` (160 test), `UnitTests/Consistency/PageSizesConsistencyTests`, `IntegrationTests/Web/{ListPagingTests,ListPatternPageTests}` ve test ana bilgisayarındaki örnek sayfalar (`Pages/ListSamples.cs`, `Pages/Storefront/*ListSample.cshtml`, `Pages/Panel/{ProductListSample,ProductRecordSample,DateRangeListSample}.cshtml`, `Web/ListSeed.cs`).
- **Dokümanlar:** `09 §10.5` madde 3 (`FromQuery` eki, ⚠ 1), D-127'nin T210 payı kapandı, `CONTEXT.md`, durum, hafıza; önceki task kaydı: T209 Commit hücresi ve "Post-merge run'lar" satırı dolduruldu (skill adım 15), eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/{PageNumber,PageSizes,PagedList,Paging,SearchTerm,RequestFormat}.cs`, `Infrastructure/Lists/*`, `Infrastructure/Security/ReturnPath.cs`, `Program.cs`.
- Testler: yukarıdaki listeye göre; `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T209_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Liste GET; parametre adları sorgu modeli alanlarının camelCase'i | ✓ | `QueryParameterNames.Of`; `ListAddressTests` (adres camelCase üretir); `ListPatternPageTests.TheParametersAreTheCamelCaseNamesOfTheModelFields_AndEachOneFilters` (`minPrice`, `maxPrice`, `inStock`, `search`, `page` süzer) |
| 2 | `page` birden başlar, birinci sayfa parametresiz, sayfa boyu parametre değil | ✓ | `ListAddressTests` (birinci sayfa `page` yazmaz); `PageOne_AndNoPage_ShowTheSamePage_…`; `PageSizesConsistencyTests` (sayılar `04` ile aynı); `PagedQuery`'de boy alanı yok |
| 3 | Sayı olmayan / birden küçük: birinci sayfa; aşan: son sayfa | ✓ | `PageNumberTests` (tablo); `ListPatternPageTests.TheShownPage_ForTheValueAsked` (`abc`, `0`, `-1`, boş, `3`, `4`, `99999999999`, `2147483648`); `ListPagingTests.APagePastTheLast_IsTheLastPage_AndAValuePastTheIntRange_IsTheLastPage` |
| 4 | Sıralama parametresi yok; eşit anahtarda `Id` ile kesinleşir, yön 09 §10.5 m.1 | ✓ | `ListPagingTests.EqualKeys_OnPageBoundaries_AreNeitherRepeatedNorLost_Descending/Ascending` (sayfa boyu 1–25, gerçek PostgreSQL); `ListPatternPageTests.TheStorefrontList_IsNewestFirst_WithNoSortParameter` (`?sort=` yok sayılır) |
| 5 | Geçersiz değer süzgeç uygulamaz, hata vermez; bilinmeyen parametre yok sayılır; tarih aralığı alan mesajı | ✓ | `ListFiltersTests`; `InvalidFilterValues_ApplyNoFilter_AndGiveNoError_AndAnUnknownParameterIsIgnored`; `ADateRangeThatBreaksTheRule_GivesTheFieldMessage_OfTheEndDate` ("Bitiş tarihi başlangıç tarihinden önce olamaz."); `AValidOrInvalidDate_GivesNoRangeMessage` |
| 6 | Alt fiyat üstten büyükse süzgeç yok, alan mesajı | ✓ | `AnInvertedPriceRange_HasTheMessage_AndNoFilter` ("En düşük fiyat üst değerden büyük olamaz."), `EqualPriceBounds_AreNoInversion`; `ListFiltersTests` (`PriceRangeResults`) |
| 7 | Boş / yalnız boşluk arama sözcüğü sonuç listesi üretmez, arama alanını gösterir | ✓ | `ABlankSearchWord_ShowsTheSearchField_AndMakesNoResultList` (boş, `search=`, boşluk, sekme); `AWordWithNoMatch_IsAnEmptyResultList_…`; `SearchTermTests`; `ListPagingTests.APercentSign_IsNotAWildcard_…` |
| 8 | `page` dışında parametre taşıyan vitrin listesi ve arama sayfası kapalı işaretli; etiket T219'un | ✓ | `ListPatternPageTests.TheMarkOfTheListPage` (11 adres: yalnız `page` ve parametresiz `open`; `?minPrice=1`, geçersiz, bilinmeyen, `page`+süzgeç, `/arama` `closed`); `ListIndexing` yalnız işareti koyar, etiket basılmaz |
| 9 | Panel listesinden kayda `returnUrl` listenin adresi; "listeye dön" ve kayıt sonrası yönlendirme onu kullanır; her uç yerel yol denetiminden geçirir; geçmeyen / olmayan değer: listenin kökü | ✓ | `TheLinkToARecord_CarriesTheAddressOfTheListWithItsPageAndFilter`; `ALocalReturnUrl_IsTheWayBack_AndTheRedirectAfterTheSave`; `ANotLocalOrEmptyReturnUrl_ReturnsToTheRootOfTheList` (dış adres, `//`, ters eğik çizgi, `javascript:`, göreli, boş, boşluk; hem sayfa hem POST 303); `ARecordOpenedWithNoReturnUrl_…`; `ListReturnTests` |

### Öz denetim (bitiş kapısı 2)

`git diff origin/main...HEAD` üzerinde kriter 1–9 ve KK1–KK9 madde madde, bütün maddeler yeniden denetlendi (yalnız kalanlar değil); testler bu turda koşuldu. Önceki işçinin tablosu devralındı.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (test adı) |
|---|---|---|
| KK1 sorgu modeli alan adı parametre adıyla bağlanır | ✓ | `…TheParametersAreTheCamelCaseNamesOfTheModelFields_AndEachOneFilters`; `page` için `FromQuery` olmadan bağlanmadığı koşuda görüldü (yol değeri `page`, ⚠ 1) |
| KK2 `page=1` ile parametresiz aynı sayfa | ✓ | `PageOne_AndNoPage_ShowTheSamePage_AndPageTwoShowsTheNextOne` |
| KK3 `abc`, `0`, aşırı büyük değer | ✓ | `TheShownPage_ForTheValueAsked` (8 satır); `PageNumberTests` |
| KK4 eşit anahtar iç kimlikle, yönüyle sıralı | ✓ | `ListPagingTests…Descending/Ascending` (12 kayıt aynı `CreatedAt`, sayfa boyu 1–25) |
| KK5 geçersiz süzgeç hata vermez; tarih aralığında alan mesajı | ✓ | `InvalidFilterValues_…`; `ADateRangeThatBreaksTheRule_…` |
| KK6 ters fiyat: mesaj var, süzgeç yok | ✓ | `AnInvertedPriceRange_HasTheMessage_AndNoFilter` (toplam 60 = süzgeçsiz) |
| KK7 boşluk arama sözcüğü boş arama ekranı | ✓ | `ABlankSearchWord_ShowsTheSearchField_AndMakesNoResultList` |
| KK8 `?minPrice=1` kapalı; yalnız `page` açık | ✓ | `TheMarkOfTheListPage` |
| KK9 dış / ağ yolu / boş `returnUrl` listenin köküne; geçerli değer sayfa + süzgeç | ✓ | `ANotLocalOrEmptyReturnUrl_…`, `ALocalReturnUrl_…` |
| `09 §25`: ilgili satırlar (yeni sayfalı liste: `Id` ile biten sıra, `Paging.ToPageAsync`/`PageSizes`, arama tek işlevden, sınır testi) | ✓ | `ListPagingTests` sınır testi; `SearchTerm.ToLikePattern` tek işlev; sayı yalnız `PageSizes`'ta (`PageSizesConsistencyTests`) |
| `09 §25`: `CONTEXT.md` aynı PR'da | ✓ | `.claude/CONTEXT.md` yeni `Infrastructure/Lists/` satırı |
| Güvenlik taraması | ✓ | `returnUrl` her okuyan uçta `ReturnPath` ile; açık yönlendirme yok (`ListReturnTests`, sayfa testleri); `LIKE` deseni parametreli ve kaçışlı; süzgeç değerleri metinden güvenli ayrıştırılır; yeni dış bağımlılık yok |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | Yeni tarama / istisna eklenmedi (⚠ 5) |
| Sık FAIL 2: tarama çağrı biçimini kaçırıyor | ✓ | Yeni kaynak taraması yok; `returnUrl` davranış testleriyle sınandı |
| Sık FAIL 3: aynı sayıyı/adı anan yerler | ✓ | `09 §10.5` m.3 `Page` tanımı `FromQuery` ile hizalandı; `09 §10.5` m.4 sayıları `PageSizesConsistencyTests` ile; `CONTEXT.md`, durum, hafıza, D-127 |
| Sık FAIL 4: belgede olmayan davranış karar kaydısız | ✓ | Ek davranışlar ⚠ 1–5'te gerekçeli (PR açıklaması) |
| Sık FAIL 5: kabul kriteri kısmen karşılandı | ✓ | Dokuz kriterin dokuzu tam |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (+Consistency, Architecture) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 2197 geçti, 0 kaldı |
| Integration (liste) | ✓ | `--filter-class "*ListPagingTests"`, `"*ListPatternPageTests"` (PostgreSQL 18, `t210-pg2`) | 15 + 49 geçti |
| Integration (Web ad alanı, 64 liste testi dahil) | ✓ | `--filter-namespace "Shopfolio.IntegrationTests.Web"` (PostgreSQL 18, `t210-pg2`) | 338 geçti; kalan ad alanları dal CI'ında |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-06, head `5476b9f`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — K1 → D-134 (T333), K2 ve K3 bu PR'da kapandı (değerlendirme, değişiklik yok) |

**Validator kanıtı (tur 1, 2026-11-06, `5476b9f`; dal CI'ı run 42165996784 success):** kapılar: working tree temiz, `core.hooksPath` `scripts/git-hooks`; ana dal son üç run (42162567568, 42162567538, 42153788582) success; hafızada T210 satırı var. Kriter 1–9 ve KK1–KK9 ✓ — kod okundu, testler doğrulama anında yeniden koşuldu: `dotnet build -warnaserror` 0/0, `dotnet format --verify-no-changes` temiz, Unit 2197/2197, betik testleri 4/4, Integration (PostgreSQL 18, ad alanlarına bölünerek) Web 338, Security 18, Concurrency 26, Jobs 77, Database 35, Model 482 + 411, kalan 123 — toplam 1510, kırmızı yok; `dotnet restore --locked-mode` çıkış 0. Kriter kanıtları yapım tablosundaki testlerle aynıdır; doğrulayıcı ayrıca `PageNumber.Parse` taşma yolunu, `Paging`'in `checked` çarpımını, `ListIndexingFilter`'ın yalnız işaret yazdığını, örnek kaydın GET ve POST'unun `ListReturn.Target`'ı ayrı ayrı çağırdığını ve `ReturnPath.ResolveOr`'un geri dönüş değerini de denetlediğini kodda okudu. Güvenlik temiz (sır yok, yeni uç ve bağımlılık yok, `returnUrl` tek yerel yol denetiminden, `LIKE` deseni parametreli ve kaçışlı).

**Task satırları (`12 §7.5`):** `KP-3` grep'i `VAL-A006`, `VAL-A007`, `VAL-A008`'i getirir (`11 §7.1`: KP-3 → T210, T333, T503). Beklenen sonuçları gerçek vitrin listesinin uç yanıtıdır; T210 yalnız kalıbı ve test ana bilgisayarındaki örnek sayfaları kurar → üçü `BEKLEMEDE` kalır (T333/T503). `KD-32` satırı yok.

| Satır | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` |
|---|---|---|---|---|
| VAL-A006 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (T333/T503) |
| VAL-A007 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (T333/T503) |
| VAL-A008 | DÜŞÜK | yok | BEKLEMEDE | BEKLEMEDE (T333/T503) |

**K-notları:** K1 → D-134: örnek vitrin listesinin fiyat süzgeci alt ve üst sınırı iki ayrı `Variants.Any` ile uygular (varyantları 5 ve 100 olan ürün 50–60 aralığına girer), stok süzgeci sınırsız stoğu saymaz, liste projeksiyonsuz okur (`09 §20` m.3); kalıbı etkilemez, gerçek listeyi kuran T333 kopyalamaz. K2: `09 §10.5` m.3 eki yalnız bağlama kaynağını yazar, `07`/`05`/`02`/`04` davranışını değiştirmez — karar kaydı gerekmez; ⚠ 2 ve ⚠ 3 belgeyle uyumlu. K3: D-127'nin T210 payının kapanışı doğrulandı.

**Yapım raporu karşılaştırması:** tam uyumlu; fark 1: yapım raporu `12` satırlarını anmıyor (doğrulayıcı üçünü BEKLEMEDE bıraktı); fark 2: yapım Integration'da yalnız Web ad alanını yerelde koştu, doğrulayıcı bütün ad alanlarını koştu. Öz denetim tablosu var, her maddesi kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T210-liste-kalibi`
- **Commit:** son commit PR'ın başıdır
- **PR:** #406
- **CI:** dal run 42165996784 success (validator, 2026-11-06)
- **Post-merge run'lar:** squash `539d9cd` (#406) — CI 42169337584 success, Publish 42169337501 success (kimlikler F2 gate'te düzeltildi) (T212 dalında dolduruldu, skill adım 15)

## Dış Varsayımlar

Dış varsayım: yok. Yeni dış servis, ağ çağrısı, kimlik bilgisi, sabit kodlu adres yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Gelecek tarih kuralı ve `InvalidDate` kodu; sayı/tarih gösterimi | D-127 (T216 payı) | Tarih alanının bağlayıcısı T216'nındır; `RequestFormat` onun genişleteceği ilk hâlde |
| 2 | `noindex` etiketinin HTML'de basılması | — (T219 KK8, kriter metninde) | `11` T210 madde 8: etiketi basan kalıp T219'un |
| 3 | Gerçek vitrin / panel liste sayfaları | — (kendi task'ları) | T210 kalıbı kurar; örnek sayfalar test ana bilgisayarındadır |

## Notlar

- **Working tree (Adım -1):** temiz. **Ana dal CI (Adım 0):** `gh run list --branch main`: T209 squash `088168e` CI 42162567538 success, Publish 42162567568 success.
- **Hedefli D kaydı:** `grep -n T210 Docs/DEFERRED_BACKLOG.md` yalnız D-127 (✅ T210 payı kapandı); yeni D açılmadı.
- **Test ortamı:** Docker Desktop'ta `localhost` / `127.0.0.1` üzerinden yayımlanan portlar `wslrelay` tarafından kesildi; test veritabanına makinenin ağ adresiyle bağlanıldı (ortam notu, kodu etkilemez).
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Branch izolasyonu:** dal yalnız T210 commit'lerini taşır.
- **⚠ 1 — `Page` alanı `[FromQuery(Name = "page")]`:** Razor Pages sayfanın kendi yolunu `page` adlı yol değeri olarak taşır ve yol değeri sorgu dizgesinden önce okunur; niteliksiz `Page` alanı her istekte sayfanın yolunu görür, sayı olmadığı için hep birinci sayfa olur (`?page=2` çalışmaz; ilk koşuda görüldü). `09 §10.5` madde 3 buna göre güncellendi (davranış `07 §2.6.2` ile aynı; yalnız bağlama kaynağı).
- **⚠ 2 — Ters aralıkta ikisi de uygulanmaz:** fiyat için `07 §2.6.3` açık; tarih için belge sessiz, aynı kural alındı (`ListFilters.ReadDates`: bitiş başlangıçtan önceyse ikisi de uygulanmaz, mesaj bitiş tarihi alanında).
- **⚠ 3 — Kapalı işareti:** `page` dışındaki her parametre (bilinmeyen ve geçersiz dahil) adresi kapalı yapar; çünkü sınırsız üretilebilen adres dizine girmemelidir (`ListIndexing`).
- **⚠ 4 — Gelecek tarih kuralı ve `InvalidDate` T216'nın:** T210 yalnız aralık kuralını (`InvalidDateRange`) kurar.
- **⚠ 5 — `returnUrl` için kaynak taraması eklenmedi:** davranış testleri (dış adres, `//`, ters eğik çizgi, `javascript:`, boş) iki uçta da koşar; örnek sayfalar test ana bilgisayarındadır.
