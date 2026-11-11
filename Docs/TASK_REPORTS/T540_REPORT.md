# T540 — Tasarım tabanı, vitrin ve panel çerçevesi, yönetici şeridi ve Taslak bandı (OB-01, OB-02, OB-09)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-11

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
| 1 | Üç genişlik sınıfı; geniş sınıfta sınırlı genişlik; tarayıcı tabanı | ✓ | `base.css` `--bp-medium: 768px`, `--bp-wide: 1024px`, `.container` sınırlı genişlik; `FrameSourceScanTests.TheBaseFile_DefinesTheTwoLimitsOnce_AndEveryWidthConditionOfEveryFileIsOneOfThem_KK1` (+ ihlal ve yakın örnekler) |
| 2 | Vitrin dar, panel geniş sınıftan; hiçbir içerik gizlenmez; metin kesilmez (üç istisna dışında) | ✓ | Vitrin `storefront.css` dar tabandan, panel `admin.css` geniş sınıftan; `…TheCutTextLine_IsUsedByTheTwoNamedExceptionsOnly_AndNoStyleSheetCutsTextOtherwise_KK2`; menü, arama ve sosyal bağlantılar dar sınıfta açılır panelde, belgede durur (`TheMenuAndTheAccountAreaAndTheSearchBox_AreInsideTheFoldOutPanel_OfTheHeader_T540_KK5`) |
| 3 | Marka rengi vitrinde yalnız dolu zemin | ✓ | `FrameSourceScanTests.TheBrandColour_IsAFilledBackgroundOnTheStorefrontOnly_…_KK3`; `TheBrandVariables_AreTwoColours_…_KK3` |
| 4 | Klavye, odak, %200; ölçüm, dış betik, gömme yok | ✓ (tarayıcı sınaması `12`'de) | `TheScripts_ImportNothing_…_KK4`, `TheFrameViews_HaveNoEventAttribute…_KK4`; entegrasyon `TheLayouts_HaveOneNoncedStyleBlock_…_KK3_KK4`; atlama bağlantısı ve odak halkası `base.css` |
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
| Doğrulama durumu | ⏳ bekliyor |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T540-tasarim-tabani-cerceve`
- **PR:** *(PR açılınca yazıldı)*
- **CI:** *(yönetici izler)*
- **Post-merge run'lar:** *(validator doldurur)*

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
| 4 | Hover kategori ağacı dokunmatikte | D-1137 | İçerik gizlenmiyor; bağlantı çalışıyor |
| 5 | Panel menü işareti metni "Bekleyen iş" ⚠ | D-1138 | `04` metni yazmıyor |

## Notlar

- **⚠ kolay kararlar:** global `_ViewStart` yok (her sayfa düzenini adıyla seçer); `Assets` yerine `StaticAsset` yardımcısı (Razor Pages'te derlenmedi); Web'de `_ViewImports` yok (test projesindekiyle tür çakışıyordu); panel başlığı "Shopfolio Panel"; Taslak bandı metni "Bu kayıt yayında değil, yalnız siz görüyorsunuz."; panel menü işareti "Bekleyen iş"; filtreler `RelativePath` yerine `ViewEnginePath` kullanır (T537 kodu panel sayfalarını ayıramıyordu); `wa.me/<rakam>` sohbet bağlantısı paylaşım taramasından ayrıldı (sohbet bağlantısı, paylaşım değil: `04` 2.2.3); çıkış formu modeli `Action` yerine `Path` taşır, `?handler=SignOut` görünümde yazılı (06/07 işleyici kuralı taraması); birleşme kutusunun kancası `data-js="merge-notice"` (bildirim parametresi adıyla çakışıyordu); `SessionAccessTests` kapalı listesine test projesinin çerçevesiz örnek sayfası `/panel/davet/{key}` eklendi (üründe davet sayfası aynı adreste gelecek).
- **12 Durum hücreleri:** VAL-A125, A127, A128, A129, A133, A671 PASS'a çekildi (kanıt yukarıdaki testler; doğrulayıcı onaylar). VAL-A126, A130, A131, A132, A134, A135 BEKLEMEDE bırakıldı (kanıtı bu task'ta yok: işletme adı ya da marka kimlik sayfasında, rehber sayfasının kendisi T507, sosyal bağlantı ve WhatsApp düğmesi birim düzeyinde sınandı ama tarayıcı gözlemi yok, Paylaş düğmesi ürün ve içerik sayfalarında).
- **Repo memory:** dokunulmadı.
- Önceki iki işçi bağlam sınırında bölündü; bu tur: mimari taramalar, KK2 testi, belgeler, kayıtlar, tam Unit ve parçalı Integration.
