# T506 — Sık sorulan sorular ve iletişim sayfası ve formu (E-09, E-10)

**Faz:** F5 | **Durum:** ⏳ Yapım tamam, doğrulama bekliyor | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **SSS sayfası (E-09):** `Pages/Storefront/Faq.cshtml(.cs)`, `FaqPage.cs`; yayındaki sorular elle sırayla tek listede, sayfalama ve arama yok, soru yoksa boş hâl satırı ve ana sayfaya yol; sayfa adı menüdeki SSS öğesinin adıdır; panel oturumunda taslak sorular yerinde "Taslak" etiketiyle görünür ve sayfa arama motorlarına kapalıdır; SSS yapılandırılmış verisi ilk yanıtın `<head>`'indedir ve yalnız yayındaki soruları taşır (`StorefrontHeads.cs`).
- **İletişim sayfası ve formu (E-10):** `Contact.cshtml(.cs)`, `ContactTopicLabels.cs`; kimlik bloğu, form ve şubeler (firmanın sırasıyla, harita bağlantılı); üyede ad ve e-posta ön dolu, düzenlenebilir; kapı kapalıyken form kapalı hâli (düğme yok, kimlik bloğu ve şubeler kalır); `?code=ContactRequestSent` ile gönderim sonrası hâli, yeni form bağlantısı kodsuz `/iletisim`.
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
| 7 | 422: boş zorunlu alan, geçersiz e-posta/telefon, beş değer dışı konu; birlikte işaretlenir; kapalı kapıda kapalı hâl | ✓ | `ContactPageTests.EmptyRequiredFields_AnInvalidEmailAndPhone_*_KK7`, `WhileTheGateIsClosed_ASendIs422_*_KK7`; `FaqAndContactPageModelTests` (`TheTopicOptions_*_KK7`, `ATopicsText_*_KK7`, `ATopicOutsideTheEnum_*_KK7`, `ThePagesInput_*_KK7`, `TheSummarysOrder_*_KK7`) |
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
| Doğrulama durumu (tur 1) | *(validator doldurur)* |
| Bulgu sayısı (tur 1) | — |
| Düzeltme gerekli mi | — |
| Yapım raporu karşılaştırması (tur 1) | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T506-sss-iletisim`
- **Commit:** birleştirme `e07bdf75` (yapım ucu bu raporun commit'inden önce)
- **PR:** *(PR açılınca yazılır)*
- **CI:** *(yardımcı denetler)*
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
