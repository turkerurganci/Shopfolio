# T501 — Yerli üretim logosu varlığı ve gösterimi

**Faz:** F5 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- Bakanlığın "Renkli Vektörel Görsel" dosyası (JPG 900x387, SHA-256 `5316c50050b9be32d6ed8ed1c4547f1d1bec42e1adf00e7c0292e484273955b6`) derlemeye gömülü kaynak oldu; ayarda ve panelde yeri yok.
- `DomesticProductionLogo`: karar (`Shows`: fiziksel ürün + "TR"), sunum adresi (`/statik/yerli-uretim-logosu.{SHA-512 ilk 10}.jpg`) ve içerik; `OriginLabel.Of`: hizmet ve boş üretim yerinde satır yok, ülke adı logolu da yazılır.
- Sunum ucu `DomesticProductionLogoEndpoint` (Program.cs'te tek satır); tek bileşen `_OriginLabel.cshtml`; stil `components.css` + `--origin-logo-width` (`base.css`), hiçbir genişlik sınıfında gizleme yok.
- `08` DV-34 satırına kaynak (duyuru ve dosya adresi), tarih, dosya özeti ve kullanım koşulu notu yazıldı.
- Backlog: D-1160 (JPG ve oran), D-1161 (açık lisans metni yok); D-344 notu; D-1136 F5 gate'e yeniden ileri atıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/Assets/DomesticProductionLogo.jpg`, `Modules/Catalog/Contracts/Pricing/DomesticProductionLogo.cs`, `Endpoints/DomesticProductionLogoEndpoint.cs`, `Pages/Shared/_OriginLabel.cshtml`, `Program.cs`, `Shopfolio.Web.csproj`, `wwwroot/css/base.css`, `wwwroot/css/components.css`
- Testler: `tests/Shopfolio.UnitTests/Catalog/DomesticProductionLogoTests.cs`, `Architecture/DomesticProductionLogoSourceScanTests.cs`; `tests/Shopfolio.IntegrationTests/Web/DomesticProductionLogoTests.cs`, `Pages/OriginLabelSamples.cs`, `Pages/Storefront/OriginLabelSample.cshtml`
- Docs: `08_INTEGRATION_SPEC.md` (DV-34), `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.d/T501.md`, `DEFERRED_BACKLOG.d/T501.md`, `DEFERRED_BACKLOG.md` (D-344 notu), `DEFERRED_BACKLOG.d/T540.md` (D-1136 hedefi)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Logo kodla gelir; firma/ayar değiştiremez, kapatamaz | ✓ | `TheFile_IsEmbeddedInTheAssembly_…_KK1_KK6`, `TheEmbeddedFile_IsTheSourceFileOfTheRepositoryByteForByte_KK1`, `OnlyTheContractTheEndpointTheComponentAndTheWiring_NameTheLogo_…_KK1_KK5` |
| 2 | Yalnız Türkiye üretimi fiziksel üründe, fiyatın yanında | ✓ | `TheLogo_ShowsForAPhysicalProductMadeInTurkey_AndForNothingElse_KK2`; Integration `APhysicalProductMadeInTurkey_ShowsTheLogoBesideTheOriginLine_T501_KK2`, `ADigitalProductAndAService_NeverShowTheLogo_T501_KK2` (fiyatın yanındaki yer T503'te) |
| 3 | Başka ülke/boş: logo yok; logolu etikette ülke adı yazılır | ✓ | `TheOriginLine_KeepsTheCountrysName_…_KK3`, `TheOriginLine_IsAbsent_ForAServiceAndForAnEmptyPlaceOfOrigin_KK3`; Integration `AnotherCountryAndAnEmptyPlaceOfOrigin_ShowNoLogo_…_KK3` |
| 4 | Hiçbir genişlik sınıfında gizlenmez | ✓ (tarayıcı: `12`) | `NoStyleRuleOfAnyWidthClass_HidesEmptiesOrClipsTheLogoComponent_KK4`, `AStyleRuleThatHidesTheLogo_IsFound_KK4`, `ANearMissOfTheHidingRule_IsLeftAlone_KK4`, `TheComponent_HasNoHiddenAttribute…_KK4`; tarayıcı sınaması `12` VAL-A016/VAL-B034 |
| 5 | Bileşen tek; çağrı T503'te | ✓ | `_OriginLabel.cshtml` tek dosya; sahip taraması `…_KK1_KK5`; D-344 notu T503'e yönlendirir |
| 6 | Güncel dosya Bakanlık duyurusundan alındı, koşul okundu, DV-34'e tarihle yazıldı | ✓ | `08` DV-34 satırı; `TheFile_IsEmbeddedInTheAssembly_IsAJpegAndIsTheOneDv34Records_KK1_KK6` (özet DV-34'le eşit) |
| 7 | Yerli sayılma firmanın beyanıdır; ürün denetlemez | ✓ | `TheDecision_TakesTheProductTypeAndTheCountryCodeOnly_KK7`; kodda ek denetim yok |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 dosya gömülü kaynak; ayarda ve panelde yeri yok | ✓ | `Shopfolio.Web.csproj` EmbeddedResource; `…_KK1` testleri (gömülü, bayt eşit, adres özeti, sahip taraması) |
| KK2 üç ürün tipi ve ülke birleşimi | ✓ | `TheLogo_ShowsForAPhysicalProductMadeInTurkey_AndForNothingElse_KK2` (ProductType x ülke birleşimi); Integration KK2 |
| KK3 başka ülke ve boş üretim yeri | ✓ | KK3 Unit ve Integration testleri |
| KK4 gizleyen stil kuralı yok | ✓ | `…_KK4` dört test (selector listesi, @media, `img`, `*`, !important, scale(0) örnekli ihlal ve yakın-kaçış örnekleri) |
| KK5 bileşen tek | ✓ | tek `_OriginLabel.cshtml`; `…_KK1_KK5` |
| KK6 DV-34 tarih ve kaynak | ✓ | `08` DV-34 |
| KK7 yalnız tip ve üretim yeri | ✓ | `…_KK7` |
| `09 §25` satırları, güvenlik taraması | ✓ | `ETag`/`If-None-Match` kullanılmadı (FormVersionSourceScan); SHA-512 (LinkKeySourceScan'a takılmaz); Integration WritingEndpointAudit, SecurityHeaders, AddressRouting yeşil; uç yalnız GET, kişisel veri yok |
| sık FAIL (1) tarama istisnası geniş | ✓ | sahip listesi sabit, yorum soyulur; istisna yok; her tarama ihlal ve yakın-kaçış örnekli |
| sık FAIL (2) çağrı biçimi kaçıyor | ✓ | gizleme taraması çok biçimli (bkz. KK4 satırı) |
| sık FAIL (3) DB default / FK dizini | ✓ yok | veritabanı değişikliği yok |
| sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | sayı/etiket değişmedi; DV tablosu sayımı (43 satır) aynı |
| sık FAIL (5) belge kuralı karar kaydısız | ✓ | belge kuralı değişmedi; `08 §4.3` madde 3 aşama notu olarak kaldı, yalnız DV-34 satırı güncellendi |
| sık FAIL: hedefi bu task olan D kaydı | ✓ | D-1136 F5 gate'e ileri atıldı (`/gorsel` ucu T501 kapsamında değil); D-344 notu T503'e |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | yeşil | `dotnet test tests/Shopfolio.UnitTests -c Release --no-build` | 6831/6831 (Consistency dahil, docs değişikliklerinden sonra) |
| Integration | yeşil | `--filter-class` DomesticProductionLogoTests, PageFormPatternTests, FrameLayoutTests, StorefrontFrameTests | 56/56 (önceki işçi: WritingEndpointAudit, SecurityHeaders, AddressRouting dahil 76/76) |
| Contract | — | — | bu task'ta sözleşme testi yok |

Derleme: Release `-warnaserror` 0 uyarı, 0 hata; `dotnet format --verify-no-changes` üç projede temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-11, head `131fe46b`; `origin/main` `06057bf9` dal tabanında, birleştirme gerekmedi). Kriter 1–7 ve KK1–KK7 ✓. Gömülü dosyanın SHA-256'sı Bakanlığın DV-34'teki dosya adresinden doğrulayıcının bugün indirdiği dosyayla aynı (`5316c500…55b6`, 39882 bayt, 900x387 JPEG); duyuru sayfası dosyayı "Renkli Vektörel Görseli" olarak bağlıyor. Yerel: `-warnaserror` derleme 0 uyarı/0 hata; `dotnet format --verify-no-changes` üç projede temiz; Unit 6831/6831; Integration `Shopfolio.IntegrationTests.Web` ad alanı 629/629 (DomesticProductionLogo, FrameLayout, PageFormPattern, RequestLimits, SecurityHeaders dahil); betik testleri 4/4. Dal CI run 42968341155 (head `131fe46b`) yeşil. Ana dal son 3 run yeşil. |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — **K1** (D-1165'e forward): `08 §4.3` madde 3 "bu aşamada logonun dosyası indirilmedi" demeye devam ediyor; T501 sonrası okuyanı yanıltır. **K2** (D-1166'ya forward): `DomesticProductionLogo.Address` `/statik` önekini elle yazar, tek tanım `AddressRules.StaticArea`'dır. **K3** (bu PR'da kapatıldı): `12` VAL-E035 (DV-34, dış sistem kaydı) yapımda BEKLEMEDE bırakılmıştı; dosya güncel ve koşul satıra yazılı olduğu için doğrulama PASS yazdı (VAL-E034/DV-33 emsali). |
| `12` satırları | VAL-E035 → PASS. VAL-A015, VAL-A016, VAL-B034 BEKLEMEDE kalır: beklenen sonuç ürün sayfası uç yanıtı ya da tarayıcı gözlemidir, çağrı T503'tedir (kriter 4–5'in ileri işareti). `12_matris_denetle.py --kapsam 1-77` hata 0. |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Var — JPG gömülü kaynak (csproj)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T501-yerli-uretim-logosu`
- **Commit:** `bc3ae96e` — T501: Yerli üretim logosu: rapor, DV-34 kaydı ve backlog (PR açıldığındaki head)
- **PR:** #535
- **CI:** run 42968341155 (head `131fe46b`) — 1–8 ve CI Gate yeşil
- **Post-merge run'lar:** main `e7e5d3e1`: CI 42988154308 success, Publish 42988154253 success

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| a | Bakanlık duyurusu ve dosya erişilebilir | WebFetch + curl, 2026-11-11; dosya 39882 bayt JPG | ✓ |
| b | Kullanım koşulu metni | sayfada açık lisans metni yok; yalnız "okunur, orijinal renkte, kılavuz oranına uyarak" şartı | ~ (D-1161) |
| c | Mevzuat atfı: Fiyat Etiketi Yönetmeliği m.5 | arama sonucu "5/2-e" numarasını doğrulamadı; `02`/`08` metni zaten m.5/2-e der | ~ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Dosya JPG raster, oranı kılavuzdan farklı | D-1160 | Bakanlık vektör vermiyor; dosya değiştirilmeden kullanıldı |
| 2 | Kullanım koşulu açık metin değil | D-1161 | Sahip bilgisi / gerekirse Bakanlığa sorgu |
| 3 | Ürün sayfasında çağrı | D-344 | T503 işi |
| 4 | `/gorsel` ucu | D-1136 | T501 kapsamında değil; F5 gate'e ileri atıldı |

## Notlar

- **Working tree (Adım -1):** temiz; hooksPath `scripts/git-hooks`
- **Ana dal CI startup (Adım 0):** main son 3 run 42964801854 (success), 42964801839 (success), 42951686078 (success)
- **Repo memory (bitiş kapısı 9):** MEMORY'ye dokunulmadı (parça dosyası kuralı)
- **Diğer:** ⚠ dosya oranı (2,33) kılavuz oranından (2,54) farklı, yeniden oranlanmadı; `12` VAL-E035 `Durum`'una dokunulmadı.
