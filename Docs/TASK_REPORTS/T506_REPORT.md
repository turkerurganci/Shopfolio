# T506 — Sık sorulan sorular ve iletişim sayfası ve formu (E-09, E-10)

**Faz:** F5 | **Durum:** ✓ Doğrulama tur 2 PASS (merge yardımcıda; main çakışması önce çözülür) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **SSS sayfası (E-09):** `Pages/Storefront/Faq.cshtml(.cs)`, `FaqPage.cs`; yayındaki sorular elle sırayla tek listede, sayfalama ve arama yok, soru yoksa boş hâl satırı ve ana sayfaya yol; sayfa adı menüdeki SSS öğesinin adıdır; panel oturumunda taslak sorular yerinde "Taslak" etiketiyle görünür ve sayfa arama motorlarına kapalıdır; SSS yapılandırılmış verisi ilk yanıtın `<head>`'indedir ve yalnız yayındaki soruları taşır (`StorefrontHeads.cs`).
- **İletişim sayfası ve formu (E-10):** `Contact.cshtml(.cs)`, `ContactTopicLabels.cs`; kimlik bloğu, form ve şubeler (firmanın sırasıyla, harita bağlantılı); üyede ad ve e-posta ön dolu, düzenlenebilir; kapı kapalıyken form kapalı hâli (düğme yok, kimlik bloğu ve şubeler kalır); `?notice=ContactRequestSent` ile gönderim sonrası hâli, yeni form bağlantısı kodsuz `/iletisim`.
- **`POST ?handler=SendContactRequest`:** model adı `SendContactRequestInput` (D-824); talep Açık yazılır, 303 → kodlu `/iletisim`, gönderene alındı e-postası gitmez; 422 (boş zorunlu alan, geçersiz e-posta/telefon, beş değer dışı konu; hatalar birlikte); kapı kapalıysa kapalı hâliyle 422; 429 (L-4) nötr mesaj düğmenin yanında, yazılanlar korunur, her deneme kaydedilir; `Website` tuzağı doluysa kayıt, deneme kaydı ve L-4 sayımı yok, yanıt başarıyla aynı; ikinci gönderim ikinci talep açar, L-4'e sayılır, firmaya F-2 gider.
- **Kimlik bloğu:** `_IdentityBlock.cshtml` altbilgi ile iletişim sayfasında ortak partial; altbilgi buna taşındı (`_StorefrontFooter.cshtml`).
- **Modül yüzü:** `ICorporateContentModule` / `CorporatePageViews` (SSS ve iletişim okuyucuları), `IMembershipModule` üyenin ad ve e-postasını verir; sabit `ContactPageContent.HasContactForm` kaldırıldı, form açıklığı `GetContactFormAsync` sonucundan okunur (D-812).
- **Stil:** `storefront.css` (SSS, iletişim, kimlik bloğu, şube listesi); dar sınıfta tek sütun, ≥ 1024'te iki sütun.
- **Devam turlarında:** T507 ve sonra T503, T520 `origin/main` birleştirmesi (çakışma yalnız `storefront.css`'te; iki taraf da dosya sonuna ekliyordu, ikisi de korundu); main'in yeni zorunlu alanlarına karşı Integration yeniden koşuldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/{ContentReaders,CorporateContentModule}.cs`, `Contracts/{CorporatePageViews,ICorporateContentModule}.cs`; `Modules/Membership/{MembershipModule}.cs`, `Contracts/{IMembershipModule,MembershipRecords}.cs`
- `src/Shopfolio.Web/Pages/Storefront/{Contact,Faq}.cshtml(.cs)`, `{ContactTopicLabels,FaqPage,StorefrontHeads}.cs`, `Pages/Shared/{_IdentityBlock,_StorefrontFooter}.cshtml`, `Pages/Frame/FramePaths.cs`, `wwwroot/css/storefront.css`
- `tests/Shopfolio.IntegrationTests/Web/{ContactPageTests,FaqPageTests,FrameLayoutTests}.cs`, `tests/Shopfolio.UnitTests/Pages/FaqAndContactPageModelTests.cs`; mevcut testlerde küçük uyarlamalar (`PanelHomeTests`, `SalesSummaryTests`, `ContentAuthoringTests`, `CorporateContentTypesTests`, `SitemapSourceTests`, örnek sayfalar)
- `Docs/DEFERRED_BACKLOG.md` (D-811, D-812, D-824 yerinde ✅), `Docs/DEFERRED_BACKLOG.d/T506.md` (D-1227, D-1228), `Docs/STATUS_CHANGELOG.d/T506.md`, `Docs/IMPLEMENTATION_STATUS.md` (T506 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `/sss` yayındaki soruları elle sırayla tek listede verir; sayfalama ve arama yok; boş hâl satırı | ✓ | `FaqPageTests.ThePublishedQuestions_AreInOneListInTheManualOrder_*_KK1`, `WhileNoQuestionIsPublished_TheEmptyStateLineShows_*_KK1`, `ThePageName_IsTheMenuNameOfTheFaqItem_KK1`; `FaqAndContactPageModelTests.TheList_KeepsTheModulesOrder_*_KK1`, `WithoutAQuestion_*_KK1`, `ThePagesName_IsTheMenusName_*_KK1` |
| 2 | Panel oturumunda taslak soru "Taslak" etiketiyle; yapılandırılmış veri ilk yanıtta | ✓ | `FaqPageTests.APanelSession_SeesTheDraftQuestionInItsPlace_*_KK2`, `TheStructuredData_IsInTheHeadOfTheFirstResponse_*_KK2`; `FaqAndContactPageModelTests.AListWithADraftQuestion_IsAPreview_KK2` |
| 3 | `/iletisim` kimlik bloğu, form, harita bağlantılı şubeler; üyede ad ve e-posta ön dolu | ✓ | `ContactPageTests.ThePage_ShowsTheIdentityBlock_TheOpenForm_AndTheBranches*_KK3`, `AMembersNameAndEmail_StartTheForm_AndAGuestsFormIsEmpty_KK3`, `APanelSession_SeesTheDraftBranchMarked_*_KK3` |
| 4 | Kapı kapalıyken form kapalı, düğme yok; kimlik bloğu ve şubeler kalır | ✓ | `ContactPageTests.WhileTheGateOfTheDataCollectingEntriesIsClosed_TheFormIsClosed_NoButton_*_KK4` |
| 5 | `ContactRequestSent` kodunda gönderim sonrası hâli; yeni form bağlantısı kodsuz | ✓ | `ContactPageTests.AnAddressWithTheContactRequestSentCode_ShowsTheStateAfterTheSend_*_KK5` |
| 6 | POST Açık talep yazar, 303 → kodlu `/iletisim`, alındı e-postası yok | ✓ | `ContactPageTests.ASend_Is303ToTheContactPageWithTheCode_WritesTheRequestOpen_*_KK6` |
| 7 | 422: boş zorunlu alan, geçersiz e-posta/telefon, beş değer dışı konu; birlikte işaretlenir; kapalı kapıda kapalı hâl | ✓ | `ContactPageTests.EmptyRequiredFields_AnInvalidEmailAndPhone_*_KK7`, `WhileTheGateIsClosed_ASendIs422_*_KK7`, `EachRefusalOfTheSend_WritesItsOneRejectionLine_*_KK7` (kapalı kapı `DataCollectionClosed`, alan redi alan satırı, 429 `ContactRequestLimitExceeded`); `FaqAndContactPageModelTests` (`TheTopicOptions_*_KK7`, `ATopicsText_*_KK7`, `ATopicOutsideTheEnum_*_KK7`, `ThePagesInput_*_KK7`, `TheSummarysOrder_*_KK7`) |
| 8 | 429 (L-4): nötr mesaj düğmenin yanında, yazılanlar korunur, her deneme kaydedilir | ✓ | `ContactPageTests.TheSendOverTheLimitL4_Is429_WithTheNeutralMessageBesideTheButton_*_KK8` |
| 9 | `Website` tuzağı doluysa kayıt, deneme kaydı ve L-4 sayımı yok; yanıt başarıyla aynı | ✓ | `ContactPageTests.ASendWithTheTrapFieldFilled_AnswersAsASuccess_LeavesNoRequestNoNoticeNoAttemptRow_*_KK9` |
| 10 | Tekrar gönderim ikinci talep açar, L-4'e sayılır; firmaya F-2 tuzak boşken gider | ✓ | `ContactPageTests.ASecondSend_OpensASecondRequest_CountsForL4_AndTheFirmGetsTheNoticeF2ForEach_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1–KK10 (yukarıdaki tablo) | ✓ | Önceki işçilerin yazdıkları dahil bütün maddeler diff'te yeniden denetlendi; her kriterin testi var ve adı `_KKn` ile biter |
| `09 §25` / güvenlik taraması: satır içi betik, `Html.Raw`, `style=`, `onclick` | ✓ | `Contact.cshtml`, `Faq.cshtml`, `_IdentityBlock`, `_StorefrontFooter` taraması boş; form `asp-antiforgery="true"` ve `asp-page-handler="SendContactRequest"` taşır; tuzak alanı `tabindex="-1"` `autocomplete="off"` |
| `sik-fail-nedenleri` (1) tarama istisnası geniş | ✓ (uygulanmaz) | Yeni tarama kuralı eklenmedi; mevcut `PageSourceScanTests` ve mimari testler yeşil |
| (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ (uygulanmaz) | Yeni kaynak taraması yok |
| (3) `06` Varsayılan / FK dizini | ✓ (uygulanmaz) | Şema, göç ve yeni tablo yok |
| (4) kural değişince aynı sayıyı anan öteki yerler | ✓ | `HasContactForm` kaldırıldı, bütün okuyucular ve testler `GetContactFormAsync`'e taşındı; derleme 0 uyarı; altbilgi kimlik bloğu ortak partial; `FrameLayoutTests` güncellendi; D-811/D-812/D-824 yerinde kapandı |
| (5) belge kuralını değiştiren yeni `09` metni karar kaydısız | ✓ | `09` ve öteki belgeler değişmedi; belgede olmayan alan uzunluk sınırı D-1227'ye `doc-drift` yazıldı |
| main'in yeni zorunlu alanı / kurucu bağımlılığı (sık FAIL "Integration TAM") | ✓ | T507, T503, T520 birleştirmesi sonrası Unit tam ve değişen ad alanları yeniden koşuldu (aşağıda) |
| `12` tarayıcı kanıtı isteyen satırlar (`VAL-B037`, `VAL-B067`, `VAL-A853`) | ~ BEKLEMEDE | İşaretleme ve CSS kaynağı düzeyinde sınandı, PASS sayılmadı; F5 tarayıcı turuna D-1228 ile kaldı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ (tam) | `dotnet test --project tests/Shopfolio.UnitTests` | 7226 test, 0 kırmızı (T503/T520 birleşiminden sonra) |
| Integration | ✓ (tam, `d3a4e73b` + `205b371e` üzerinde ad alanı parçalarıyla) | `--filter-namespace` / `--filter-class` parçaları, Postgres 18 `track_commit_timestamp=on`, Model `--max-threads 3` | Administration 69, Catalog 82, Commands 70, Concurrency 30, CorporateContent 185, Database 36, Disposal 61, FileSecrets 24, Files 112, Jobs 85, Logging 30, Outages 53, Payment 59, Security 34, SignedEndpoints 11, Ordering 645, Web 791, Model 1410 (A 93, B-C 422, D-L 231, M-N 156, P-R 214, S 294); 0 kırmızı |
| Integration (main T503 + T520 birleşimi sonrası, değişen ad alanları) | ✓ | aynı komutlar, birleşik ağaç | Web 837 (A-J 219, L-O 258, P-W 360), Catalog + Administration + Security + Commands + CorporateContent 440, Ordering + Model.Settings 673; 0 kırmızı |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, UnitTests, IntegrationTests temiz (birleştirmeden önceki ağaçta; birleştirme yalnız CSS dosya sonu çakışmasını çözdü) |
| Derleme | ✓ | `dotnet build Shopfolio.slnx` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL (2026-11-12; dal ucu `a53c6c33`, origin/main `9f5b8ac8` içinde) |
| Bulgu sayısı (tur 1) | 1 (ORTA, S1) + 2 K-notu |
| Düzeltme gerekli mi | Evet — B1: kapı kapalıyken `SendContactRequest` 422'si `DataCollectionClosed` ret satırını yazmıyor (`Contact.cshtml.cs` `default` dalı yalnız `FormResults.Redisplay`; `RejectionLog.Rejected(result.Code)` yok; `09 §8.2` m.3, K-1198; `VAL-A123` kanıtı "Uygulama kaydı"). Düzeltme: dal ret satırını `result.Code` ile yazar; test kapalı kapı POST'unda kayıt satırını (`code=DataCollectionClosed`) doğrular |
| Yapım raporu karşılaştırması (tur 1) | KK1–KK10 kanıtları uyumlu; uyuşmazlıklar: (1) öz denetimin `09 §25` satırı "red uygulama kaydında mı" sorusunu kapsamıyor (B1 kaçtı); (2) rapor sonuç kodunu `?code=ContactRequestSent` yazar, adres `?notice=ContactRequestSent`'tir; (3) `12` Durum hücreleri yazılmamış, `VAL-A120`…`A122` (tarayıcı) D-1228'de yok (D-1313) |
| Doğrulayıcı kanıtları (tur 1) | Derleme `-warnaserror` 0/0; `dotnet format --verify-no-changes` üç proje temiz; Unit 7226/7226; betik 4/4; Integration TAM 3833/3833 (Web 837; Administration+Catalog+Commands+Concurrency+CorporateContent+Security+SignedEndpoints+FileSecrets+Logging 535; Database+Disposal+Files+Jobs+Outages+Payment 406; Ordering 645; Model 1410); CI run 43115380835 yeşil; `12` Durum: A123 FAIL, A452/A558/A580/A634 PASS |
| Doğrulama durumu (tur 2) | ✓ PASS (2026-11-12; dal ucu `fd92dbcd`) |
| Bulgu sayısı (tur 2) | 0 bulgu, 0 K-notu; birleştirme notu 1 |
| Doğrulayıcı kanıtları (tur 2) | Ana dal CI son 3 run yeşil (43128171608, 43128171584, 43125378261); derleme `-warnaserror` 0/0; `dotnet format --verify-no-changes` üç proje temiz; Unit 7226/7226; betik 4/4; Integration TAM 3834/3834 (Web 838, Model 1410, Ordering 645, öteki ad alanları 941); `restore --locked-mode` 0; KK1–KK10 ✓; B1 kapandı: kapalı kapı POST'u 422 + tek `DataCollectionClosed` ret satırı (`ContactPageTests.EachRefusalOfTheSend_*_KK7`); `12` Durum: A123 FAIL → PASS, A487 BEKLEMEDE → PASS |
| Birleştirme notu (tur 2) | `git merge origin/main` (`33a9d21a`; T505 #543, T549 #550) kod çakışması verdi: `src/Shopfolio.Web/Modules/CorporateContent/CorporateContentModule.cs` (kurucu: dalın `ContentReaders readers` alanı ↔ main'in `StorefrontContentPages contentPages` alanı; ikisi de korunmalı) ve kayıt belgesi `Docs/IMPLEMENTATION_STATUS.md`. Birleştirme geri alındı; doğrulama dalın kendi ucunda yapıldı. PR #551 `CONFLICTING` olduğu için `fd92dbcd` üzerinde CI koşmadı (son dal run'ı 43122758392 `12ab2810` yeşil); çakışma çözülüp CI yeşil olmadan merge edilmez |

### Düzeltme 1 (tur 1 FAIL sonrası)

- **B1 (ORTA, S1; VAL-A123, KK7, `09 §25`):** `ContactModel.OnPostSendContactRequestAsync` `default:` (kapalı kapı) dalı artık `RejectionLog.Rejected(result.Code)` yazar (`DataCollectionClosed`). Aynı sayfadaki öteki ret dalları tarandı: alan redi `ValidationPageFilter`'in işleyici sonrası çevirisiyle alan satırını, 429 `FormResults.AttemptLimitExceeded` ile `ContactRequestLimitExceeded` satırını zaten yazıyor; yalnız kapı dalı eksikti. Yeni test: `ContactPageTests.EachRefusalOfTheSend_WritesItsOneRejectionLine_TheClosedGateWithDataCollectionClosed_TheFieldsWithTheirNames_TheLimitWithItsCode_KK7` (üç ret türünün her biri tek satır).
- **K2:** rapordaki sonuç kodu `?notice=ContactRequestSent` olarak düzeltildi. **K1:** D-1313 ileri atıldı (doğrulayıcı kaydı), bu turda iş yok.
- **Öz denetim:** KK1–KK10 ve plandaki 10 kabul kriteri yeniden denetlendi; tablo ✓. `09 §25` "red uygulama kaydında mı" satırı artık üç ret türü için ✓. Çakışma işareti taraması boş.
- **Sınama:** restore, build `-warnaserror` 0/0, format üç proje temiz, Unit 7226/7226, Integration Web 838/838 (ContactPageTests 12/12). Integration'ın öteki ad alanı parçaları süre bütçesi nedeniyle bu turda koşulmadı (değişiklik yalnız `Contact.cshtml.cs` ret satırı); doğrulayıcı TAM koşar.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T506-sss-iletisim`
- **Commit:** `50b7486e` (yapım ucu; birleştirme `e07bdf75`)
- **PR:** #551
- **CI:** dal ucu `fd92dbcd`'de run yok (PR çakışmalı); önceki uç `12ab2810` run 43122758392 yeşil — çakışma çözümünden sonra yardımcı denetler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İletişim formu alan uzunluk sınırı belgede yok; form `maxlength` yazmıyor | D-1227 | `04` 5.10 / `09 §3.5` karar task'ı |
| 2 | E-09 ve E-10 tarayıcı kanıtı (`VAL-B037`, `VAL-B067`, `VAL-A853`) | D-1228 | F5 tarayıcı turu |

## Notlar

- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum"a ve backlog sayacına dokunulmadı (parça dosyası kuralı)
- **Integration yükü:** makinede başka koşular vardı; kırmızı çıkmadı, yeniden koşu gerekmedi
- **Diğer:** devam turları (7 işçi); ilk işçiler bağlam/süre sınırında bölündü
