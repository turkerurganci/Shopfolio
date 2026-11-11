# T540 — Tasarım tabanı, vitrin ve panel çerçevesi, yönetici şeridi ve Taslak bandı (OB-01, OB-02, OB-09)

**Faz:** F5 | **Durum:** ✓ Tamamlandı — doğrulama tur 2 ✓ PASS | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- Dört stil dosyası: `base.css` (tasarım tabanı değişkenleri, sıfırlama, yerleşim, üç genişlik sınıfı; sınırlar tek tanım), `components.css`, `storefront.css`, `admin.css`; `common.js` (menü açma, birleşme kutusunu kapatma; içe aktarma yok).
- `_StorefrontLayout` (+ `_StorefrontHeader`, `_StorefrontFooter`, `_MenuTree`, `_AdminStrip`, `_DraftBand`): yönetici şeridi ya da Taslak bandı, duyuru şeridi, üst bölüm, menü, içerik, eksiksiz altbilgi; ödeme adımında yalnız marka ve "Sepete dön"; birleşme mesajı içerik alanının başında kapatılabilir kutuda; marka rengi tek nonce'lu stil bloğunda.
- `_PanelLayout` (üst satır, sekiz girişli menü, "Bekleyen iş" işareti, çıkış formu) ve `_PanelBareLayout` (E-29, E-30, E-54 iniş adımları, çerçevesiz); `PanelFrame`, `PanelFrameBuilder`, `PanelFrameFilter`.
- `Pages/Frame/StaticAssetPaths.cs`: `Context.StaticAsset("css/base.css")` parmak izli `/statik/...` adresini uç noktalardan her çağrıda okur (önbellek ve alan yok).
- `FrameStyle`: marka değişkenleri, görsel adresi, WhatsApp sohbet bağlantısı, sosyal ağ adı.
- Bu tur (devam 2): Unit mimari taramaları yeşile döndü; `09 §16.1` statik adres örneği hizalandı; KK2 metin kesme denetimi eklendi; kayıtlar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/wwwroot/css/*`, `wwwroot/js/common.js`, `Pages/Shared/_*Layout.cshtml`, `_StorefrontHeader/_StorefrontFooter/_MenuTree/_AdminStrip/_DraftBand.cshtml`, `Pages/Frame/*` (yeni: panel modeli, süzgeç, `StaticAssetPaths`, `FrameStyle`; çıkış formu modeli `Action` yerine `Path`).
- Testler: `tests/Shopfolio.IntegrationTests/Web/FrameLayoutTests.cs` (StorefrontFrameTests'in parçası) ve örnek sayfalar `Pages/FrameLayoutSamples.cs`, `Pages/Storefront/*Sample.cshtml`, `Pages/Panel/Panel*Sample.cshtml`; `tests/Shopfolio.UnitTests/Pages/PanelFrameModelTests.cs`, `Architecture/FrameSourceScanTests.cs`; taramalar: `AddressSourceScan` (wa.me sohbet bağlantısı), `ForgeryProtectionSourceScan` (vitrin düzeninin nonce bloğu); `SessionAccessTests` kapalı liste beklentisi.
- Belgeler: `09 §16.1` madde 2, 3, 9; `12` VAL-A125, A127, A128, A129, A133, A671 Durum hücreleri.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç genişlik sınıfı; geniş sınıfta sınırlı genişlik; tarayıcı tabanı | ✓ | `base.css` `--bp-medium: 768px`, `--bp-wide: 1024px`, `.container` sınırlı genişlik; `FrameSourceScanTests.TheBaseFile_DefinesTheTwoLimitsOnce_AndEveryWidthConditionOfEveryFileIsOneOfThem_KK1` (+ ihlal ve yakın örnekler); panel: `admin.css` geniş sınıfta `.panel-content` `max-width: var(--content-max-width)` + `margin-inline: auto` (tek değişken `base.css`'te); `FrameSourceScanTests.TheVariableOfTheContentWidth_IsDefinedOnce_AndTheStorefrontAndThePanelColumnsBothLimitAndCentreTheirContent_KK1` (+ `AColumnWithoutALimitedAndCentredWidth_IsFound_KK1` ihlal örnekleri, `AColumnWithALimitedAndCentredWidth_IsLeftAlone_KK1`) — düzeltme 1 (B2) |
| 2 | Vitrin dar, panel geniş sınıftan; hiçbir içerik gizlenmez; metin kesilmez (üç istisna dışında) | ✓ | Vitrin `storefront.css` dar tabandan, panel `admin.css` geniş sınıftan; `…TheCutTextLine_IsUsedByTheTwoNamedExceptionsOnly_AndNoStyleSheetCutsTextOtherwise_KK2`; menü, arama ve sosyal bağlantılar dar sınıfta açılır panelde, belgede durur (`TheMenuAndTheAccountAreaAndTheSearchBox_AreInsideTheFoldOutPanel_OfTheHeader_T540_KK5`); kategori ağacı her sınıfta "Ürünler" düğmesiyle açılır, betiksiz yolda açık (`TheProductsGroupOfTheMenu_IsAButtonWithAriaState_ThatControlsTheCategoryTree_T540_KK2_KK4`) — düzeltme 1 (B1) |
| 3 | Marka rengi vitrinde yalnız dolu zemin | ✓ | `FrameSourceScanTests.TheBrandColour_IsAFilledBackgroundOnTheStorefrontOnly_…_KK3`; `TheBrandVariables_AreTwoColours_…_KK3` |
| 4 | Klavye, odak, %200; ölçüm, dış betik, gömme yok | ✓ (tarayıcı sınaması `12`'de) | `TheScripts_ImportNothing_…_KK4`, `TheFrameViews_HaveNoEventAttribute…_KK4`; entegrasyon `TheLayouts_HaveOneNoncedStyleBlock_…_KK3_KK4`; atlama bağlantısı ve odak halkası `base.css`; menü ağacı: `FrameSourceScanTests.TheMenuGroupTitle_HasAFocusableTrigger_ThatTheScriptOpensByClickAndClosesOnEscape_KK4` (+ `AGroupTitleWithoutAFocusableTrigger_IsFound_KK4`, `ATriggerThatTheScriptNeverBinds_IsFound_KK4`, `AGroupTitleWithItsTrigger_IsLeftAlone_KK4`); `common.js` `bindMenuTreeGroup` (tıklama/Enter/Space, Esc odağı düğmeye döndürür) — düzeltme 1 (B1) |
| 5 | Vitrin çerçevesi beş bölge; dar sınıfta menü, arama, sosyal açılır menüde | ✓ | `TheStorefrontLayout_DrawsTheFiveRegionsInOrder_…_KK5_KK9`; `…AreInsideTheFoldOutPanel…_KK5` |
| 6 | Altbilgi eksiksiz, katlanmaz; "İletişim" E-10'a; yalnız yayındaki yasal metne bağlantı | ✓ | `TheFooter_IsWholeOnEveryScreen_TheContactHeadingLeadsToE10_AndOnlyPublishedLegalTextsAreLinked_T540_KK6` |
| 7 | Ödeme adımı üst bölümü yalnız logo/marka adı + "Sepete dön" | ✓ | `ThePaymentStepHeader_CarriesTheBrandAndBackToTheCartOnly_T540_KK7` |
| 8 | Panel çerçevesi üst satır + sekiz giriş; `PanelFrame` veri ve bekleyen iş işareti; E-29, E-30, E-54 çerçevesiz | ✓ | `ThePanelLayout_DrawsTheTopRowAndTheEightEntries_…_T540_KK8`, `ThePanelMenu_MarksOrdersAndRequestsWhileWorkIsPending_…_KK8`, `TheScreensBeforeTheSession_HaveNoPanelFrame_…_KK8`; `PanelFrameModelTests` (sekiz giriş, etkin giriş, bekleyen iş, çerçevesiz yollar) |
| 9 | Yönetici şeridi yalnız panel oturumunda, "Panele dön"; Taslak bandı şeridin yerini alır, "Düzenlemeye dön"; müşteri görmez | ✓ | `TheDraftBand_TakesTheStripsPlace_LeadsBackToTheForm_AndTheCustomerNeverSeesIt_T540_KK9`; `PanelFrameModelTests.TheDraftBand_*_KK9` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil kabul kriterlerinin ve `12` kontrol listesinin tamamı diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 üç sınıf; P-42; tarayıcı tabanı | ✓ | yukarıda 1; `EveryClassOfTheFrame_IsABemName_…_KK1` |
| KK2 gizlenen yok; metin kesilmez | ✓ | `…TheCutTextLine_…_KK2` (yalnız `_PanelLayout` yönetici adı ve `_StorefrontHeader` marka adı `u-single-line` kullanır; `text-overflow` yalnız o seçicilerde) |
| KK3 marka rengi yalnız zemin | ✓ | yukarıda 3 |
| KK4 ölçüm, betik, gömme yok; klavye, odak, %200 `12`'de | ✓ | yukarıda 4 |
| KK5 beş bölge; açılır menü | ✓ | yukarıda 5 |
| KK6 altbilgi; yayınsız metne bağlantı yok | ✓ | yukarıda 6 (gizlilik bildirimi yayında değil: bağlantı yok) |
| KK7 sade üst bölüm | ✓ | yukarıda 7 |
| KK8 üst satır; menü; çerçevesiz; panel modeli | ✓ | yukarıda 8 |
| KK9 şerit, bant; müşteride yok | ✓ | yukarıda 9 |
| `09 §25` ilgili satırlar | ✓ | `ForgeryProtectionSourceScanTests` (vitrin düzeninin nonce'lu tek stil bloğu; panel düzeni stil bloğu yazamaz — ihlal örneği var), `RichTextSourceScan` (`Html.Raw` yok), `NoticeParameterSourceScan`, `SearchEngineSourceScan` (robots yalnız `SearchEngineData.Head` kalıbında), `PageSourceScan` (her post formu adlandırılmış işleyici ve `asp-antiforgery`) hepsi yeşil |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `wa.me` istisnası yalnız `wa.me/` ardından rakam; `wa.me/` ve `wa.me/?text=` hâlâ bulgu (`Scan_OfAPlatformShareAddressOrWidget_IsAFinding`, yeni `…TheWhatsAppChatLinkOfTheFirmsOwnNumber_IsQuiet`); stil bloğu istisnası tam yol sonu `_StorefrontLayout.cshtml` + nonce zorunlu, başka düzen ve nonce'suz blok bulgu (yeni test) |
| Sık FAIL 2: kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | yeni istisnalar iki yönlü örnekle sınandı (yukarıdaki iki test); `StaticAssetPaths` için ayrı tarama gerekmedi: `CacheSourceScan` alan ve Lazy yok |
| Sık FAIL 3: 06 Varsayılan / FK dizini | ✓ yok | veri şeması ve göç değişikliği yok |
| Sık FAIL 4: kural değişince aynı sayıyı anan yerler | ✓ | `09 §16.1` m.2, m.3 ve m.9 birlikte güncellendi; `asp-append-version` başka yerde geçmiyor (grep: Docs, src, tests, .claude boş); çıkış formu modeli `Action`→`Path`: T537 testi ve iki entegrasyon beklentisi güncellendi |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09 §16.1` değişikliği `07 §2.1.2` ile hizalama (kaynak doküman kazanır, `09 §5.3`); yeni kural yok, D-116'nın kendi kararı |
| Devredilen backlog | ✓ | aşağıda |
| `dotnet format --verify-no-changes` | ✓ | Web, UnitTests, IntegrationTests temiz |
| Güvenlik taraması | ✓ | aşağıda |

**Güvenlik taraması.** Secret yok. Yetki: panel düzenleri yalnız panel oturumunun sayfalarında çizilir; çerçevesiz düzen yalnız `SessionSchemes` açık listesindeki sayfalarda. Girdi: marka rengi altı haneli renk dışında varsayılana düşer, stil bloğu yalnız iki renk taşır (`@` ile kaçışlanır, `Html.Raw` yok); görsel ve WhatsApp bağlantıları yalnız kimlik ve on rakamdan kurulur. Çıkış formları POST ve `asp-antiforgery="true"`; token güvenli şema ister (testler `X-Forwarded-Proto: https` gönderir, 05 6.3.3). Yeni bağımlılık yok. Dış betik, gömme içerik ve satır içi betik yok.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (tam, Mimari ve Consistency dahil) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 6764 geçti, 0 kaldı |
| Integration (parçalı) | ✓ | `--filter-class` parçaları, PostgreSQL 18 `track_commit_timestamp=on` | StorefrontFrameTests 22/22; PageFormPattern, SearchEngineData, SecurityHeaders, ForgeryProtection 74/74; SessionAccess 36/36; Notice, Operation, ListPattern, Format, VersionedForm, ModelBinding, AnnouncementAndPreview, StorefrontReaders, UnexpectedErrorLayer, AddressRouting, AttemptLimit parçası 340/340 (SessionAccess'te tek kırmızı kapalı liste beklentisiydi, düzeltildi) |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | 3 proje temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 2) | ✓ PASS — tur 2 (2026-11-11, head `0fddab5e`; `origin/main` `de34daf4` dal tabanında, birleştirme boş geçti). Kriter 1–9 ve KK1–KK9 ✓; B1 ("Ürünler" `<button type="button">`, `aria-expanded`/`aria-controls`, tıklama/Enter/Space ve Esc `common.js` `bindMenuTreeGroup`'ta, hover kuralı yok, betiksiz yolda ağaç açık) ve B2 (`admin.css` geniş sınıfta `.panel-content` `max-width: var(--content-max-width)` + `margin-inline: auto`) giderildi. Dal CI'ı yeşil (run 42960747674, dokuz iş), ana dal son üç run yeşil (42951686078, 42951686087, 42945971198). Yerelde: restore, derleme 0 uyarı (`-warnaserror`), format üç proje temiz, Unit 6777/6777, betik testleri 4/4, Integration TAM 3598/3598 (Web 623, Model 1407 — taze konteynerde ilk koşu yeşil, öteki ad alanları 1568; PostgreSQL 18 `track_commit_timestamp=on`). `12`: tarayıcı gözlemi isteyen KP-69…KP-71 satırları (VAL-A337…A350, B028…B092) BEKLEMEDE; VAL-A671 PASS (uç yanıtı ve veri kaydı: `TheMergeMessage_IsDrawnWithTheProductNames_ThenGone_T540_D1132` + T537'nin yönlendirme testi). |
| Bulgu sayısı (tur 2) | 0 |
| Bloke-etmeyen not (tur 2) | 2 — **K1** (D-1143'e forward): orta ve geniş sınıfta ETBİS bandı kimlik bloğunun altında; `04` 2.2.5 "hemen yanında — dar sınıfta hemen altında" der. **K2** (bu PR'da kapatıldı): raporun Notlar bölümü VAL-A125, A127, A128, A129, A133'ü PASS diye anıyordu; tur 1 bunları BEKLEMEDE'ye döndürmüştü, satır düzeltildi. |
| Yapım raporu karşılaştırması (tur 2) | Kabul kriterlerinde tam uyum (1–9 ✓); öz denetim tablosu var, maddeler kanıtlı. 1 uyuşmazlık: K2 (Notlar'daki eski `12` satırı). |
| Doğrulama durumu (tur 1) | ✗ FAIL — tur 1 (2026-11-11, head `f39d4137`; `origin/main` `de34daf4` dal tabanında, birleştirme gerekmedi). Kriter 3, 5–9 ve KK3, KK5–KK9 ✓; **kriter 1 / KK1 ~ Kısmi** (B2), **kriter 2 / KK2 ~ Kısmi** ve **kriter 4 / KK4 ✗** (B1). Dal CI'ı yeşil (run 42955145005), ana dal son üç run yeşil (42951686078, 42951686087, 42945971198). Yerelde: restore, derleme 0 uyarı (`-warnaserror`), format üç proje temiz, Unit 6764/6764, betik testleri 4/4, Integration TAM 3597/3597 (Web 622, Ordering 637, Model 1407, kalan 15 ad alanı 931; Model'in ilk koşusunda 1 kararsız kırmızı, iki yeniden koşuda yeşil — K2). `12_matris_denetle.py --kapsam 1-77` hata 0. |
| Bulgu sayısı | 2 — **B1** (S1, ORTA; kriter 4 ve 2, `04` 2.1.4 kural (3), 2.1.2; komşu satırlar VAL-A001, VAL-A345, VAL-B054 — satırsız bulgu, satırlar tarayıcı kanıtı beklediği için BEKLEMEDE): orta ve geniş sınıfta vitrin menüsünün kategori ağacı klavyeyle ulaşılamaz. `storefront.css` `@media (min-width: 768px)` içinde `.site-menu__item--group > .site-menu__tree { display: none; }` ağacı yalnız `:hover` ya da `:focus-within` ile açar; grubun başlığı "Ürünler" odaklanamayan bir `<span class="site-menu__group-title">`'dır (`_StorefrontHeader.cshtml`) ve `display: none` altındaki bağlantılar sekme sırasından çıkar — grubun içinde odaklanacak öğe kalmaz, `:focus-within` hiç tetiklenmez. Sonuç: klavyeyle (ve fare üstüne gelmesi olmayan dokunmatik orta/geniş ekranda) birinci, ikinci ve üçüncü seviye kategorilerin hiçbiri menüden açılamaz (`04` 3.1.11); CSS yorumu ("the same links stay reachable by keyboard") ve D-1137'nin "içerik gizlenmez, bağlantı çalışır" cümlesi bu yüzden yanlış. **B2** (S1, DÜŞÜK; kriter 1, `04` 2.1.1, `05 §3.8.2`; VAL-B062 → FAIL): panel çerçevesinde geniş sınıfta içerik sınırlı genişlikte durmaz — `.panel-content` (`admin.css`) yalnız `flex: 1 1 auto` taşır, `max-width` yok; 1280 ile daha geniş pencere arasında panel içeriği ekranla yayılır. Vitrin `.container` (`--content-max-width: 72rem`) kuralı karşılar. |
| Düzeltme gerekli mi | Evet — **B1:** ağaç orta/geniş sınıfta klavyeyle ve dokunmayla açılmalı (ör. "Ürünler" başlığı `aria-expanded` taşıyan bir düğme olur, betiksiz yolda ağaç açık kalır ya da `:focus-within` için odaklanabilir bir öğe; seçim yapımın), CSS yorumu ve D-1137 buna göre güncellenir, kaynak taraması ya da entegrasyon testi gizli ağacın odaklanabilir bir tetikleyicisi olduğunu sınar. **B2:** `.panel-content` (ya da içindeki sarmalayıcı) geniş sınıfta sınırlı genişlik alır (değişken `base.css`'te); vitrin kuralıyla aynı ilke. |
| Bloke-etmeyen not (K) sayısı | 4 — **K1** (D-1139'a forward): dar sınıfta açılır menüdeki kategori ağacı "katlanarak açılır" değil, hep açık (`04` 2.2.4); kabul kriteri 5 bunu saymaz. **K2** (D-1140'a forward): Integration `Model` ad alanının ilk koşusunda 1/1407 kararsız kırmızı (taze konteynerde ilk koşu), iki yeniden koşuda ve dal CI'ında yeşil; test adı ilk koşunun özetinde alınamadı. **K3** (bu PR'da kapatıldı): yapım commit'i `12`'de VAL-A125, A127, A128, A129, A133'ü PASS'a çekmişti; `Durum` doğrulayıcının hücresidir (`12 §7.5`) ve bu satırların kanıt türü "Tarayıcı gözlemi" (ana dalda yalnız tarayıcı gözlemi isteyen hiçbir satır PASS değil) — tarayıcı gözlemi yapılmadığı için BEKLEMEDE'ye döndü; VAL-A671 (Uç yanıtı + Veri kaydı) entegrasyon testiyle kanıtlı, PASS kaldı. **K4** (düzeltme turunda bu PR'da kapatılır): `.pending-mark` kuralı hem `components.css` hem `admin.css`'te tanımlı (ortak bileşen tek yerde durmalı, `09 §16.1` m.1). |
| Yapım raporu karşılaştırması | 2 uyuşmazlık — kriter 1 ve 4 yapımda ✓; doğrulamada ~ ve ✗ (B2, B1). Known Limitations 4 (D-1137) "içerik gizlenmiyor" diyor; klavye yolu da kapalı. Öz denetim tablosu var ve maddeler kanıtlı. |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T540-tasarim-tabani-cerceve`
- **PR:** #534
- **CI:** yeşil — run 42960747674 (head `0fddab5e`, doğrulama tur 2)
- **Post-merge run'lar:** main `06057bf9`: CI 42964801839 success, Publish 42964801854 success

## Dış Varsayımlar

Dış varsayım: yok. Task yalnız uygulama içi düzen, stil ve model kurar; plan katmanı, kota, dış servis, paket sürümü ya da ortam sırrı gerekmez. Test Postgres'i CI ile aynı (`postgres:18`, `track_commit_timestamp=on`).

## Devredilen backlog kalemleri

| Kalem | Karar |
|---|---|
| D-116 | Kapandı: `09 §16.1` hizalandı, `StaticAsset` yardımcısı, gerçek uygulamada parmak izli adres sınandı; önbellek başlığı ölçümü D-1135'e. |
| D-222, D-730 | T540 payı kapandı (altbilgi bağlantıları); kalan sayfalar T507 ve ilgili task'larda, satırlar yerinde güncellendi. |
| D-1125, D-1126, D-1128, D-1132 | Kapandı (şerit yalnız "Panele dön"; WhatsApp bağlantısı; `PanelFrame`; birleşme kutusu). |
| D-135, D-138 | Hata sayfası çerçeveye dönüşümü ve çerez süzgeci hata yolu T541'e alındı (üst sınır F5 gate). |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Diğer" taşma menüsü | D-1134 | Kategori sayısı küçükken sorun çıkmaz; `04` 2.2.3 |
| 2 | Favicon yedeği; statik dosya önbellek başlığı ölçümü | D-1135 | Belge istemiyor; ölçüm |
| 3 | Görsel adresi `/gorsel/{id}` varsayımı ⚠ | D-1136 | Görsel sunan uç T501'de |
| 4 | ~~Hover kategori ağacı dokunmatikte~~ | D-1137 ✅ | Düzeltme 1'de kapandı: düğmeyle açılır (tıklama, Enter, Space, Esc); D-1139 (dar sınıfta katlanma) da kapandı |
| 5 | Panel menü işareti metni "Bekleyen iş" ⚠ | D-1138 | `04` metni yazmıyor |

## Düzeltme 1 (doğrulama tur 1 FAIL sonrası, 2026-11-11)

- **B1:** "Ürünler" gerçek bir `<button type="button" aria-expanded aria-controls="site-menu-tree">` oldu (`_StorefrontHeader.cshtml`; betiksiz yolda düğme gizli, başlık metin, ağaç açık). `common.js` `bindMenuTreeGroup`: tıklama/Enter/Space açar ve kapatır, Esc kapatıp odağı düğmeye döndürür; kapalıyken ağaç `display: none` (bağlantılar sekme sırasında değil), açıkken sekme sırasında. Hover açılışı kaldırıldı (tek yol olamaz; dokunmatik ve klavye aynı yol). Dar sınıfta da aynı düğme ağacı katlar (`04` 2.2.4): D-1139 ve D-1137 ✅. CSS yorumu düzeltildi; D-1137'nin yanlış cümlesi çözüm kaydıyla değişti.
- **B2:** `.panel-content` geniş sınıfta `max-width: var(--content-max-width)` ve `margin-inline: auto` (vitrinle aynı tek değişken; `04` belge panel için ayrı değer yazmıyor).
- **K4:** `.pending-mark` yinelenen kuralı `admin.css`'ten kaldırıldı (tek ev `components.css`).
- **12:** VAL-B062 FAIL'den BEKLEMEDE'ye döndü (kaynak taraması + Integration kanıtı var; tarayıcı gözlemi bekliyor, PASS'a çekilmedi); tarayıcı isteyen öteki satırlar BEKLEMEDE'de kaldı.
- **Öz denetim (düzeltme 1):** T540 kabul kriterleri 1–9 ve KK1–KK9 yeniden denetlendi — 1, 2, 4 yukarıdaki yeni kanıtla ✓; 3, 5–9 değişmedi (StorefrontFrameTests 23/23, Unit 6777/6777 yeşil). `sik-fail-nedenleri.md` okundu: menü klavye erişimi ve iki çerçevede sınırlı genişlik satırları kapatıldı, `12` satırı PASS'a çekilmedi.

## Notlar

- **⚠ kolay kararlar:** global `_ViewStart` yok (her sayfa düzenini adıyla seçer); `Assets` yerine `StaticAsset` yardımcısı (Razor Pages'te derlenmedi); Web'de `_ViewImports` yok (test projesindekiyle tür çakışıyordu); panel başlığı "Shopfolio Panel"; Taslak bandı metni "Bu kayıt yayında değil, yalnız siz görüyorsunuz."; panel menü işareti "Bekleyen iş"; filtreler `RelativePath` yerine `ViewEnginePath` kullanır (T537 kodu panel sayfalarını ayıramıyordu); `wa.me/<rakam>` sohbet bağlantısı paylaşım taramasından ayrıldı (sohbet bağlantısı, paylaşım değil: `04` 2.2.3); çıkış formu modeli `Action` yerine `Path` taşır, `?handler=SignOut` görünümde yazılı (06/07 işleyici kuralı taraması); birleşme kutusunun kancası `data-js="merge-notice"` (bildirim parametresi adıyla çakışıyordu); `SessionAccessTests` kapalı listesine test projesinin çerçevesiz örnek sayfası `/panel/davet/{key}` eklendi (üründe davet sayfası aynı adreste gelecek).
- **12 Durum hücreleri:** VAL-A671 PASS (uç yanıtı ve veri kaydı kanıtı; doğrulama tur 2 onayladı). VAL-A125, A127, A128, A129, A133'ü yapım PASS'a çekmişti; doğrulama tur 1 tarayıcı gözlemi olmadığı için BEKLEMEDE'ye döndürdü (tur 2 K2). VAL-A126, A130, A131, A132, A134, A135 BEKLEMEDE bırakıldı (kanıtı bu task'ta yok: işletme adı ya da marka kimlik sayfasında, rehber sayfasının kendisi T507, sosyal bağlantı ve WhatsApp düğmesi birim düzeyinde sınandı ama tarayıcı gözlemi yok, Paylaş düğmesi ürün ve içerik sayfalarında).
- **Repo memory:** dokunulmadı.
- Önceki iki işçi bağlam sınırında bölündü; bu tur: mimari taramalar, KK2 testi, belgeler, kayıtlar, tam Unit ve parçalı Integration.
