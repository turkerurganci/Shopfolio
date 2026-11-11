# T541 — Mesaj, onay, boş ve hata hâlleri ve satış kapalı hâli (OB-03, OB-04, OB-07, OB-08)

**Faz:** F5 | **Durum:** ✓ Tamamlandı — doğrulama tur 1 ✓ PASS | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Mesaj bileşeni:** `MessageModels.cs` (`MessageView`, `Messages`, `MessageSymbols`, `FormErrorSummary`), `_Message`, `_FormErrorSummary`; dört anlam, dört yer (alanda, yerinde, bir kez söylenen, kısa süreli); simge + gizli sözcük + metin; anlam renkleri `base.css`'te marka renginden ayrı.
- **Onay bileşeni:** `ConfirmModels.cs` (`Confirmations`), `_Confirm`, `_ConfirmForm`; yönetici penceresi (`<dialog open>`, odak "Vazgeç"te, dar sınıfta tam ekran), müşteri adımı, betiksiz yolda aynı işaretleme, `data-js="submit-once"`.
- **Boş, yükleniyor, hata ve satış kapalı:** `StateModels.cs` (`EmptyStates`, `LoadingViews`, `SalesClosedMessages`, `PanelSalesStatus.From`), `_EmptyRow`, `_Loading`, `_UnexpectedError`, `_PanelSalesStatus`.
- **D-135 (500 sayfası):** `IErrorPageRenderer` + `FramedErrorPageRenderer`; sayfa ve işlem düzeyi 500 vitrin, panel ve çerçevesiz panel çerçevesinde yazılır; çerçeveli sayfa düşerse yalın HTML.
- **Devam turunda bulunan ve giderilen hatalar:** (1) `_Confirm`, `_FormErrorSummary`, `_PanelSalesStatus`, `_UnexpectedError` `@addTagHelper` taşımıyordu; `<partial>` ham etiket olarak yazılıyordu (hata sayfasında mesaj görünmüyordu) — düzeltildi ve `FrameSourceScanTests` taraması eklendi. (2) `ModelStateDictionary` alfabetik gezilir; "ilk hatalı alan" form sırasına göre seçilsin diye `FormErrorSummary.From(state, formOrder)` ve `common.js` belge sırasını kullanır; işleyici parametre adı küçük harfliyse (`input.Name`) form yazımı esas alınır. (3) Eylem düzeyi mesajlar (nötr limit cümlesi) özette tekrar çizilmiyor; düğmenin yanında ekranın yeridir.
- **Kapsam dışı bırakılan:** 429 ve 413 sayfalarının çerçeveye alınması D-1146'ya taşındı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Components/{MessageModels,ConfirmModels,StateModels}.cs`, `Pages/Shared/_{Message,FormErrorSummary,Confirm,ConfirmForm,EmptyRow,Loading,UnexpectedError,PanelSalesStatus}.cshtml`, `Pages/Frame/FramedErrorPageRenderer.cs`, `Infrastructure/Errors/{IErrorPageRenderer,UnexpectedErrorLayer}.cs`
- `wwwroot/css/{base,components}.css`, `wwwroot/js/common.js`
- `tests/Shopfolio.UnitTests/Pages/MessageStateModelTests.cs`, `Architecture/{MessageSourceScanTests,FrameSourceScanTests}.cs`
- `tests/Shopfolio.IntegrationTests/Web/MessageComponentTests.cs`, `Pages/MessageSamples.cs`, `Pages/Storefront/MessageSample.cshtml`, `Pages/Panel/MessageSample.cshtml`
- `Docs/DEFERRED_BACKLOG.md` (D-135 kısmen ✅, D-138 ve D-141 yeniden hedef), `Docs/DEFERRED_BACKLOG.d/{T540,T541}.md` (D-1138 ✅; D-1145–D-1148), `Docs/STATUS_CHANGELOG.d/T541.md`, `Docs/IMPLEMENTATION_STATUS.md` (T541 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Simge + metin; renkle tek başına değil; marka renginden bağımsız | ✓ | `MessageStateModelTests.EveryMeaning_*_KK1`, `MessageComponentTests.EveryMessage_CarriesASymbol*_KK1`, `MessageSourceScanTests.TheFourMeanings_*_KK1` |
| 2 | Hatalı alanlar birlikte, sayı, ilk alana odak, değerler korunur | ✓ | `MessageComponentTests.ASubmittedFormWithTwoWrongFields_*_KK2`, `AFormWithOneWrongField_*`, `MessageStateModelTests.TheSummary_*`, `TheFormOrder_*` |
| 3 | Panel engel mesajı yerinde kalıcı; sebep + sayı + çıkış yolu; pencere yok | ✓ | `ThePanelBlockMessage_*_KK3`, `MessageStateModelTests.ThePanelBlock*` |
| 4 | Kısa süreli bildirim yalnız başarı | ✓ | `Messages.ShortLivedSuccess` tek kurucu; `TheShortLivedNotice_IsASuccess_*_KK4`, `OnlyTheSuccessIsShortLived_*_KK4`, `MessageSourceScanTests.TheShortLivedNotice_*` |
| 5 | Nötr limit düğmenin yanında; sayı/süre/eksen yok | ✓ | `TheNeutralLimitMessage_IsBesideTheBlockedButton_*_KK5`, `MessageStateModelTests.TheNeutralLimitMessage_*` |
| 6 | Onay anatomisi; pencere + odak Vazgeç'te + dar sınıf tam ekran; müşteri adımı; betiksiz yol | ✓ | `TheCustomersConfirmation_*_KK6`, `TheAdministratorsConfirmation_*_KK6`, `MessageSourceScanTests.TheConfirmationWindow_*`, `Confirmations` birim testleri |
| 7 | İkinci basış engeli; sonucu belirsiz onay sonuca götürür | ✓ | `TheConfirmationsForm_CanNotBeSentTwice_*_KK7`, `MessageSourceScanTests.TheSubmitOnceScript_*`, `AnOutcomeUncertainConfirmation_*_KK7` (+ `UnexpectedErrorLayerTests` 303) |
| 8 | İki boş hâl; yükleniyor çerçeveyi bırakır ve metin taşır | ✓ | `TheTwoEmptyStates_*_KK8`, `TheLoadingState_KeepsTheFrame_*_KK8`, `MessageSourceScanTests.TheEmptyAndLoadingViews_*` |
| 9 | Üç hata hâli; kod yok | ✓ (tarayıcı kanıtı gerektirmez) | `ThePageLevelErrorPage_IsDrawnInsideTheFrameOfItsSurface_*_KK9` (vitrin, panel), `UnexpectedErrorLayerTests` (işlem, sayfa, belirsiz) |
| 10 | Satış kapalı: vitrin, panel dört yer, veri kapısı ayrı | ✓ | `WhileSalesAreClosed_*_KK10`, `TheDataEntryGate_*_KK10`, `ThePanelSalesState_*_KK10` (üç test), `MessageStateModelTests.ThePanelStatus_*` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1–KK10 (yukarıdaki tablo) | ✓ | Önceki işçinin yazdıkları dahil bütün maddeler diff'te yeniden denetlendi; iki gerçek hata (tag helper bildirimi, alan sırası) bulunup giderildi |
| `sik-fail-nedenleri` (1) tarama istisnası geniş | ✓ | Tek istisna `(prefers-reduced-motion: reduce)` tam eşitlik; yakın ıskalamalar (`no-preference`, genişlikle birleşik) için ayrı ihlal örneği |
| (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ | Tag helper taraması `<partial` ve `asp-*` taşıyan her etiketi kapsar; üç ihlal ve üç yakın ıskalama örneği |
| (3) `06` Varsayılan / FK dizini | ✓ (uygulanmaz) | Şema değişikliği yok |
| (4) kural değişince öteki yerler | ✓ | `UnexpectedErrorAnswer.PageMessage` değişimi `UnexpectedErrorLayerTests` ve entegrasyon testinde aynı; D-135, D-138, D-141, D-1138 satırları güncellendi |
| (5) belge kuralını değiştiren 09 metni | ✓ | `09` metni değişmedi; belgede olmayan iki cümle ve süre D-1145 ve D-1147'ye `doc-drift` olarak yazıldı |
| `12` tarayıcı kanıtı isteyen satırlar | ~ BEKLEMEDE | Odak davranışı, dar sınıf tam ekran ve CSS animasyonu işaretleme ve kaynak düzeyinde sınandı; gerçek tarayıcı kanıtı F5 tarayıcı turuna kalır, PASS sayılmadı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ (tam) | `dotnet test --project tests/Shopfolio.UnitTests` | 6848 test, 0 kırmızı (devam turunda yeniden koşuldu; önceki tek kırmızı `SearchEngineSourceScanTests` `Canonical` adı giderilmişti) |
| Integration | ✓ (tam, ad alanı parçalarıyla) | `--filter-namespace Shopfolio.IntegrationTests.<Alan>` (Postgres 18, `track_commit_timestamp=on`) | Web 643, Model 1407, Ordering 637, CorporateContent 185, Files 102, Jobs 85, Catalog 82, Commands 70, Administration 69, Disposal 61, Payment 59, Outages 53, Database 36, Security 34, Logging 30, Concurrency 30, FileSecrets 24, SignedEndpoints 11; 0 kırmızı. `UnexpectedErrorLayerTests` 25/25 ve `MessageComponentTests` 20/20 (tag helper düzeltmesinden sonra) |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1, yeniden koşu (2026-11-11, kod head'i `8e7e4782`; önceki doğrulayıcının `a08349f5` kayıt commit'i kanıt sayılmadı, her madde baştan doğrulandı ve kayıtları bu bulgulara göre korundu). Kriter 1–10 ve KK1–KK10 ✓ (validator kanıtı: kod okuması + `MessageComponentTests` 20, `MessageStateModelTests`, `MessageSourceScanTests`, `FrameSourceScanTests`, `UnexpectedErrorLayerTests` yeşil). Yerel: build `-warnaserror` 0 uyarı, format üç projede temiz, Unit 6848/6848 (Consistency ve Architecture dahil), betik testleri 4/4, Integration Web 643, Logging 30, SignedEndpoints 11, Security 34, Jobs 85 (PostgreSQL 18, `track_commit_timestamp=on`), `restore --locked-mode` 0. Ana dal son 3 run yeşil (42976026259, 42976026333, 42964801854). Dal CI: head `8e7e4782` run 42975292161 success (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate); PR #537 `main` ile çakıştığı için PR denetimi raporlanmıyor. `12`: KP-38 (VAL-A136…A145) ve KP-69 (VAL-A337…A339, VAL-B030…) satırları BEKLEMEDE kaldı — uçtan uca satış kapısı ve tarayıcı kanıtı bu task'ın kapsamında değil. Birleştirme notu: `origin/main` `db7984ac` (T542) `components.css` ve `IMPLEMENTATION_STATUS.md`'de çakıştı; kod çakışması olduğu için birleştirme geri alındı, kayıt birleştirmesiz. |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — **K1** (D-1173): `Confirmations`/`_ConfirmForm` onay işaretini (`Confirmed`, `07 §2.2.4`) kendisi yazmıyor, çağırana bırakıyor; panel örneği işaretsiz. **K2** (D-1174): işlem düzeyi yedek sayfa çerçeveye geçince "Yeniden dene" ve "Sayfa açılamadı" başlığı taşıyor (`04` 2.7.3.1); geçici kapatma + eksik koşul uyarısı "Geçici kapatma … tamamlanmadı" diyor (`04` 2.8.2); yeniden koşu ekledi: "Yeniden dene" adresi işleyicinin sorgusunu (`?handler=…`) da taşır ve E-44 koşul satırları "Satışı geçici kapatma anahtarı kapalı: Eksik" ve "Yasal metinler: Aydınlatma metni: Eksik" diye okunur. **K3** (bu PR'da kapatıldı): T541'e hedefli D-1134 ve D-1143 alınmamış, hedefi T541'de kalmıştı; dalın T542'ye yeniden hedeflediği D-138, D-141, D-1145, D-1146, D-1147 T542 merge edildiği için hedefsiz kaldı — hepsi F5 gate'e yeniden hedeflendi. |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T541-mesaj-onay-bos-hata-halleri`
- **Commit:** bkz. PR
- **PR:** #537
- **CI:** `54a45d98` run 42971044507 success; head `8e7e4782` run 42975292161 success
- **Post-merge run'lar:** main `8ac3b828`: CI 42991019267 success, Publish 42991019324 success

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | 429 ve 413 sayfaları çerçeveli değil | D-1146 | Süre bütçesi; kriter 9 500 sayfasını (sayfa, işlem, belirsiz) ister |
| 2 | Kısa süreli bildirim süresi (6 sn) belgede yok | D-1145 | `04`'e yazılmalı |
| 3 | Vitrin ve veri kapısı cümleleri `04`'te yok | D-1147 | `04`'e yazılmalı |
| 4 | Alan parçası (`_Field`) yok | D-1148 | İlk form ekranı task'ı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yönetici hazırlığı kapsamında
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum"a dokunulmadı (parça dosyası kuralı)
- **Diğer:** devam turu; ilk işçi bağlam sınırında bölünmüştü
