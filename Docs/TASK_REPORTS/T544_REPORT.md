# T544 — Tarih alanı, kupon alanı, onay kutuları, görsel ve dosya yükleme arayüzü (OB-16…OB-19)

**Faz:** F5 | **Durum:** ✗ Doğrulama tur 1 FAIL — düzeltme bekliyor | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Tarih alanı:** `DateModels.cs` (`DateFields`): olay tarihi (`CheckEvent`: geçmiş serbest, ileri tarih ve alt sınırın altı sebebiyle reddedilir; sınırın kendi günü kabul), sıra sorusu (`SequenceQuestion`, `CheckSequence`: cayma beyanı günüyle çakışınca formun zorunlu kapalı listesi, pencere değil), tarih aralığı (`CheckRange`, `DateRangePolicy.FutureAllowed`/`NoFuture`; uçlar `TurkeyTime` ile T378 k7: başlangıç günü 00:00, bitiş gününü izleyen gün 00:00; duyuruda iki tarih isteğe bağlı).
- **Kupon alanı:** `CouponModels.cs` (`CouponFields`, `CouponFieldView`): kapalı satır "Kupon kodum var", açık alan, uygulanmış hâl (kod adı, indirdiği tutar `-50 TL`, "Değiştir" ve "Kaldır"; tek kod), limitle kapanan alan (nötr mesaj), girişten dönüşte kodun yeniden uygulanması (`AfterSignIn`); kod karşılaştırması `TextNormalizer.NormalizeIdentity` ile (ikinci büyük-küçük harf kuralı yok).
- **Onay kutuları:** `ConsentModels.cs` (`ConsentBoxes`): iki sabit kutu, sepete göre dijital ve hizmet kutusu; hiçbiri işaretli gelmez; eksik kutu `CheckboxNotAccepted` ile kutunun yanında; metin sürümü değişince hepsi, özet parmak izi (T509 k1) değişince iki yasal kutu boşalır; KVKK/yaş/pazarlama kutusu yok; yerleşim: yasal metinler, cayma özeti, kutular, onay düğmesi.
- **Yükleme:** `UploadComponentModels.cs`: galeri (sıra elle, ana görsel sıradan bağımsız, boş alternatif metni sistem üretir), tek dosyalı alanlar (logo, site simgesi, dijital dosya), sınır aşımı alan mesajı (sınırlar `ProductConstants`'tan), ilerleme göstergesi ve yükleme sürerken kapalı kayıt düğmesi, müşteri yüklemesi yok (`UploadChecks.RequirePanel`).
- **Görünümler/stil:** `_ConsentBoxes`, `_CouponField`, `_GalleryUpload`, `_SingleUpload`, `_UploadStatus`, `_Field` (tarih `min`/`max`), `components.css`.
- **Devam turu (bu işçi):** önceki işçinin yazdıkları birleşik ağaçta (origin/main dahil) yeniden denetlendi; `dotnet format` Unit ve Integration test projeleri temiz doğrulandı; kayıtlar (rapor, değişiklik günlüğü, backlog parçası, durum satırı) yazıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Components/{DateModels,CouponModels,ConsentModels,UploadComponentModels}.cs`, `FieldModels.cs` (tarih alanı), `Pages/Shared/_{ConsentBoxes,CouponField,GalleryUpload,SingleUpload,UploadStatus,Field}.cshtml`, `wwwroot/css/components.css`
- `tests/Shopfolio.UnitTests/Pages/ControlComponentModelTests.cs`, `Architecture/ControlSourceScanTests.cs`
- `tests/Shopfolio.IntegrationTests/Web/ControlComponentTests.cs`, `Pages/ControlSamples.cs`, `Pages/Storefront/ControlSample.cshtml`
- `Docs/DEFERRED_BACKLOG.d/T544.md` (D-1222–D-1226), `Docs/STATUS_CHANGELOG.d/T544.md`, `Docs/IMPLEMENTATION_STATUS.md` (T544 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Olay tarihi geçmişe dönük; ileri tarih ve alt sınır altı sebebiyle reddedilir | ✓ | `ControlComponentModelTests.AnEventDateInThePast_OrToday_IsAccepted_KK1`, `AnEventDateAfterToday_IsRefusedForBeingInTheFuture_KK1` (+ alt sınır testleri); `ControlSourceScanTests.TheDateComponent_ReadsTurkeyTimeFromTurkeyTimeOnly_KK1` |
| 2 | Sıra sorusu formun alanı, pencere değil | ✓ | `TheSequenceQuestion_IsAskedOnlyWhenTheDateIsADeclarationDay_AsARequiredClosedList_KK2`, `TheSequenceAnswer_IsRequiredAndOneOfTwo_KK2` |
| 3 | Aralıkta ileri tarih serbest; uçlar T378 k7; duyuruda iki tarih isteğe bağlı | ✓ | `ADiscountRange_TakesAFutureDate_KK3`, `AReportFilterRange_RefusesAFutureDate_KK3` (7.3.2.12 ayrı politika), uç testleri `TurkeyTime` ile; `ControlSourceScanTests.ADateComponentWithItsOwnClock_IsFound_KK3` (ihlal) ve `...AsksTurkeyTime_IsQuiet_KK3` |
| 4 | Kupon alanı toplamın üstünde, kapalı satır; kod harf ayrımsız | ✓ | `ControlComponentTests.TheCouponField_IsAClosedRowAboveTheTotal_AndOpensWithoutAScript_KK4`; `TwoCodes_AreTheSameIgnoringTheCase_KK4`; `ControlSourceScanTests.TheCouponComponent_HasNoCaseRuleOfItsOwn_AndUsesTheIdentityNormaliser_KK4` (+ ihlal ve sessiz örnek) |
| 5 | Uygulanan kod adı ve tutarla, "Değiştir"/"Kaldır"; en fazla bir kod | ✓ | `ControlComponentTests.AnAppliedCoupon_ShowsItsNameAndAmount_WithChangeAndRemove_AndOffersNoSecondCodeField_KK5`; `TheAcceptedAnswer_IsTheAppliedField_AndANewCodeReplacesTheOldOne_KK5` |
| 6 | Geçersiz kod ve asgari tutar ayrı mesaj; limit alanı kapatır; girişten dönüşte yeniden uygulama | ✓ | `ControlComponentTests.ARefusedCode_IsToldByTwoDifferentMessages_AndTheLimitClosesTheField_KK6`; `AnInvalidCodeAndACodeUnderTheMinimum_AreToldWithTwoSeparateMessages_KK6`; `CouponFields.AfterSignIn` modelde sınandı |
| 7 | Dört kutu (iki sabit + sepete göre), hiçbiri işaretli gelmez, yasal metinlerin altında, düğmenin üstünde | ✓ | `ControlComponentTests.TheConfirmationBoxes_AreTheTwoFixedAndTheDigitalOne_NoneTicked_...DirectlyAboveTheButton_KK7`; `NoBoxComesTicked_WhenTheStepOpens_KK7`; `ControlSourceScanTests.TheConsentView_TicksNoBoxOnItsOwn_KK7` (+ ihlal/sessiz örnek) |
| 8 | İşaretsiz kutu sipariş yaratmaz, alan mesajı; sürüm/özet değişince boşalır; KVKK/yaş/pazarlama kutusu yok | ✓ | `ControlComponentTests.AFormWithAnUntickedBox_MakesNoOrder_...KK8`, `AFormWithEveryRequiredBoxTicked_MakesTheOrder_KK8`; `ControlComponentModelTests.AMissingBox_..._KK8` ve sıfırlama testleri; `ControlSourceScanTests.TheConsentBoxes_HaveNoKvkkNoAgeAndNoMarketingBox_KK8` (+ ihlal örneği) |
| 9 | Galeri çoklu yükleme, elle sıra, ana görsel sıradan bağımsız, boş alternatif metni sistem üretir | ✓ | `ControlComponentTests.TheGallery_ShowsItsRowsInTheFirmsOrder_WithOneMainChoice_...KK9`; `TheGalleryRows_FollowTheOrderTheFirmGave_AndTiesKeepTheirPlace_KK9`; `TheMainMark_IsIndependentOfTheOrder_KK9` |
| 10 | Sınır aşımı alan mesajı; ilerleme görünür, kayıt kapalı; logo/site simgesi/dijital dosya tek dosya; müşteri yükleme yapmaz | ✓ | `ControlComponentTests.WhileAnUploadGoesOn_TheProgressShows_AndTheSaveButtonIsClosed_KK10`, `TheSingleFileField_TakesOneFile_...TheCustomersFormHasNoFileInput_KK10`; `ControlComponentModelTests.AFileThatIsNotAnImage_IsRefusedForItsFormat_ReadFromItsBytes_KK10`; `ControlSourceScanTests.NoPageOutsideTheUploadComponent_NamesAFileInputOrAMultipartForm_KK10`, `TheControlComponents_TypeNoLimit_KK10` (+ ihlal ve sessiz örnekler) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–10 / KK1–KK10 (önceki işçilerin yazdıkları dahil, bütün maddeler diff'te) | ✓ | Yukarıdaki tablo; her kriter için model testi, entegrasyon testi ve gerekirse kaynak taraması |
| `sik-fail-nedenleri` (1) tarama istisnası geniş | ✓ | Taramalar belirli nitelik/çağrı biçimini arar; her tarama için ayrı ihlal örneği ve ayrı sessiz örnek var (KK3, KK4, KK7, KK8, KK10); geniş istisna yok. Bilinen sınır: `max`/`min` sayı nitelikleri taranmaz (D-1226) |
| (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ | Dosya girişi taraması çok satırlı ve adlı biçimi, sınır taraması sabit yerine yazılan sayıyı, kupon taraması `ToLower`/`ToUpper`/`OrdinalIgnoreCase` biçimlerini kapsar (ihlal örnekleri `[Theory]`); bilinen boşluk D-1226'da |
| (3) `06` Varsayılan / FK dizini | ✓ (uygulanmaz) | Şema/migration değişikliği yok |
| (4) kural değişince aynı sayıyı anan öteki yerler | ✓ | Yeni sabit/sayı yok; sınırlar `ProductConstants`'tan, kod karşılaştırması tek normalleştirici, tarih uçları tek `TurkeyTime`; main birleştirmesinde (T508/T502) çakışma ve tutarsızlık yok |
| (5) belge kuralını değiştiren yeni 09 metni | ✓ | `09` değişmedi; belgede olmayan metinler ve iki politika `doc-drift` olarak D-1222–D-1224'e yazıldı |
| D-483 (kuponun ödeme adımına bağlanması) | ✓ ileri atıldı | Bileşen `CouponApplication` sonucundan alanı kurar; işleyiciye bağlama E-13 (T509) işi, D-1225'e yazıldı |
| `12` tarayıcı kanıtı isteyen satırlar | ~ BEKLEMEDE | Gerçek zamanlı ilerleme ve tarayıcı görünümü kaynak/HTML düzeyinde sınandı; tarayıcı kanıtı F5 tarayıcı turuna kalır, PASS sayılmadı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ (tam) | `dotnet test --project tests/Shopfolio.UnitTests` (birleşik ağaç, origin/main dahil) | 7265 test, 0 kırmızı (`ControlComponentModelTests` 78, `ControlSourceScanTests` 48) |
| Integration | ✓ (TAM, ad alanı parçaları; `track_commit_timestamp=on`) | `dotnet test tests/Shopfolio.IntegrationTests --no-build -- --filter-namespace X` | Administration 69, Catalog 82, Commands 70, Concurrency 30, CorporateContent 185, Database 36, Disposal 61, FileSecrets 24, Files 102, Jobs 85, Logging 30, Model 1407, Ordering 638, Outages 53, Payment 59, Security 34, SignedEndpoints 11, Web 764 (`ControlComponentTests` 9); 0 kırmızı |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Tur 1 (2026-11-11): ✗ FAIL — kriter 10 ~ Kısmi (KK10 "reddedilen dosya") |
| Bulgu sayısı | 2 (ORTA 1, DÜŞÜK 1) + 1 K-notu |
| Düzeltme gerekli mi | Evet — yeni yapım turu |

**Validator kanıtları (tur 1):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1–9 | ✓ | Kod okuması (`DateModels`, `CouponModels`, `ConsentModels`, `UploadComponentModels`, görünümler); Unit 7265/7265, `ControlComponentTests` 9/9 yerel (postgres:18, `track_commit_timestamp=on`); CI run 43072832924 yeşil |
| 10 | ~ | Kurallar ve mesaj metinleri var (`UploadChecks.CheckImage/CheckDigitalFile/CheckCount`), ilerleme ve kapalı kayıt düğmesi ✓, tek dosya ✓, müşteride yükleme yok ✓; ama `_GalleryUpload` ve `_SingleUpload` alanın mesajını çizmez (görünüm modelinde `Error` yok), `AddRefusal` yalnız model durumuna yazar; galerinin dosya girişi `id="gallery-files"`, özetin bağlantısı `FieldIdOf("Input.Gallery.Files")` = `Input_Gallery_Files` — eşleşmez. Sebep alan mesajı olarak görünmez (`04` 2.12.8.3, 2.3.2 "her alan kendi mesajını taşır"; 2.12.8.7 `UploadMissing` aynı boşluk) |

**Bulgular:** (1) ORTA S1 — yükleme bileşenlerinde alan mesajı yok (yukarıda). (2) DÜŞÜK S1 — `CouponFields.BlankField` kupon kodunu `FieldRequirement.Required` ile kurar, etiket "zorunlu" der; `04` 7.1.5.7 kupon kodu "İsteğe bağlı". **K1:** `CouponFieldView.TakesACode` yorumu "yalnız açık hâl" der, kod `Mode != LimitClosed` döner — düzeltme turunda bu PR'da kapatılır.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T544-tarih-kupon-onay-yukleme`
- **PR:** #546
- **CI:** yardımcı denetler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Onay kutusu metinleri `04`'te cümle olarak yok | D-1222 | `04`'e yazılmalı |
| 2 | Tarih aralığında iki politika (2.12.5.3 / 7.3.2.12) | D-1223 | `04` birlikte söylemeli |
| 3 | Sürüm değişimi hepsi, özet farkı iki yasal kutu | D-1224 | `04` açık yazmalı |
| 4 | Gerçek zamanlı ilerleme betiği, kupon yerleşimi ve işleyici bağlama | D-1225 | Ekran task'ları (T509 ve sonrası) |
| 5 | `max`/`min` taranmaz; sıra sorusu yalnız teslim tarihi için | D-1226 | Yapısal sayılar; ekran task'ı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum"a dokunulmadı (parça dosyası kuralı)
- **Diğer:** üçüncü işçi bitirdi; önceki ikisi bağlam/süre sınırında bölündü
