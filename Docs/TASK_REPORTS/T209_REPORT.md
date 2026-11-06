# T209 — Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları

**Faz:** F2 | **Durum:** ✓ Tamamlandı (yapım; doğrulama bekliyor) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Sahteciliğe karşı koruma:** `ForgerySetup.AddFormSecurity` iki genel süzgeç kaydeder: `ForgeryProtectionFilter` (sayfa süzgeci; model bağlandıktan sonra, alan doğrulamasından önce `IAntiforgery` ile doğrular; sıra `Filters.Add<T>(order)` ile verilir) ve `ForgeryProtectionPolicy` (işaret, `Order = 1001`; Razor Pages'in varsayılan süzgecinin duruşunu geçersiz kılar). Koruma sayfada işaretsiz ve kapalıya düşer: `IsInForce` yalnız işaretin varlığına bakar, `IgnoreAntiforgeryToken` niteliği onu kapatamaz. Doğrulama başarısızsa işleyici çalışmaz, aynı sayfa 400 ile, yazılanlar korunarak ve yeni değerle gösterilir (`FormResults` genişletmesi).
- **Kapalı liste:** `AntiforgeryExemptions` yalnız imzalı ödeme bildirimi yolunu taşır; `MapSignedEndpoint` `DisableAntiforgery()` yazar (tek yer).
- **Güvenlik başlıkları:** `SecurityHeadersMiddleware` + `SecurityHeaders` + `ContentSecurityPolicy`: her yanıtta (makine ucu, hata, statik dosya dahil) dört başlık, istek başına yeni nonce, nonce yalnız `style-src`'de; HSTS ve `X-Frame-Options` yok (HSTS'i ters vekil yazar, T05). Anahtarlı bağlantı sayfaları `no-referrer`.
- **Önbellek:** `NoStoreAttribute`; panel ve hesap klasörlerine klasör kuralıyla, sepet, ödeme adımı, erişim anahtarlı sipariş ve anahtarlı bağlantı sayfalarına nitelikle. Ek: 404 yanıtı da `no-store` (⚠ 1).
- **Tek kullanımlık değerler:** `ReauthenticationValues` (verilir, form gizli alanında taşınır, gönderimde `TryRedeemAsync` ile bir kez tüketilir; yalnız özet saklanır; yanlış değer ya da işlem hiçbir şey tüketmez; alan hatası değeri tüketmez); `Forms/ConfirmationKeys` (sipariş onay anahtarı: her gösterilen form yeni anahtar; aynı anahtarla ikinci gönderim ikinci sipariş açmaz, teyit ekranına gider).
- **Taramalar:** `ForgeryProtectionSourceScan` (koruma kapatma adları, HSTS, listedeki olmayan imzalı uç, başka yazan uç, kendi adresli ve başka siteye POST formlar, başlık yazımı, zayıflatılmış politika, satır içi betik / stil niteliği / nonce'suz stil bloğu) ayrı ihlal örnekli; `WritingEndpointAudit` gerçek uç veri kaynağını denetler.
- **Test altyapısı:** `Web/ForgeryTokenHandler` (test istemcileri jetonu otomatik taşır; atlama başlığı `X-Test-No-Forgery-Token`), `Pages/ForgeryTokenSample`, `Pages/FormSecuritySamples` (onay, yeniden doğrulama, sepet, anahtarlı bağlantı örnekleri); eski örnek sayfalardan `[IgnoreAntiforgeryToken]` kalktı.
- **Dokümanlar:** `09 §15.2` madde 1, 2, 5 metni gerçek mekanizmaya hizalandı (⚠ 2); `09 §25` yeni satır; `CONTEXT.md`; D-131; durum, hafıza.
- Önceki task kayıtları: T208 ve T211 Commit hücresi ve "Post-merge run'lar" satırı dolduruldu (skill adım 15).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/{AntiforgeryExemptions,ContentSecurityPolicy,ForgeryProtectionFilter,ForgerySetup,NoStoreAttribute,ReauthenticationValues,SecurityHeaders,SecurityHeadersMiddleware,CookieSetup}.cs`; `Infrastructure/Forms/{ConfirmationKeys,FormResults}.cs`; `Infrastructure/SignedEndpoints/SignedEndpointRegistration.cs`; `Program.cs`.
- Testler: `UnitTests/Architecture/ForgeryProtectionSourceScan(.cs,Tests.cs)`, `UnitTests/Infrastructure/FormSecurityTests.cs`; `IntegrationTests/Web/{ForgeryProtectionTests,WritingEndpointAudit,SecurityHeadersTests,OneTimeValueTests,ForgeryTokenHandler}.cs`, `Pages/{ForgeryTokenSample,FormSecuritySamples,AccessSamples}.cs`, örnek `.cshtml` sayfaları; mevcut altı test sınıfı istemcisi `ForgeryTokenHandler`'a geçti.
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/{T208,T211}_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tekrar gönderim genel kuralı güncel hâl; ayrı istek kimliği yok; listelenen işlemler ikinci kez işlenir | ✓ | `ForgeryProtectionTests.TheSameFormSentTwice_WithTheListedOperationsOfCriterion1_WritesTwice_KK1` (sepete ekleme örneği iki satır); güncel hâlle karşılaştırılan yarı T208 `VersionedFormPageTests` (409). Listelenen işlemlerin kendi uçları kendi task'larında |
| 2 | Sipariş onayı tek istisna: aynı anahtar ikinci sipariş açmaz, teyit ekranına gider | ✓ | `OneTimeValueTests.TheSameConfirmationKeySentTwice_OpensOneOrder_AndBothAnswersGoToItsConfirmationScreen_KK2`, `TwoShownForms_HaveTwoKeys_AndTwoKeysOpenTwoOrders_KK2`; `FormSecurityTests.ConfirmationKey_IsFreshEachTime_…` |
| 3 | Yeniden doğrulama değeri formun gizli alanı; gönderim tüketir | ✓ | `OneTimeValueTests.TheFormCarriesTheValueInAHiddenField_AndTheValidSendConsumesIt_SoTheSecondSendAsksAgain_KK3`, `AGrantedValue_IsConsumedOnce_…`, `OnlyTheDigestIsStored_…`, `AValidationErrorOnTheForm_DoesNotConsumeTheValue_…`; `FormSecurityTests.ReauthenticationField_IsAHiddenFieldOfThatName_…` |
| 4 | Bütün tarayıcı POST'ları korumadan geçer; parçalı yüklemede değer `RequestVerificationToken` başlığında | ✓ (başlık yarısı sayfa işleyicilerinde; parçalı yükleme ucunun kendisi T312) | `Post_WithoutAToken_…KK4`, `…WithAWrongToken…`, `…ButNoCookieHalf…`, `…TokenOfOnePairAndTheCookieOfAnother…`, `…MadeForAnotherUser…`, `Post_OfAHandlerThatAnswers200_WithoutAToken_RunsNoHandler_KK4`, `Post_WithTheTokenInTheHeader_Passes_…KK4`. Parçalı yükleme ucu henüz yok (T312, `11` T312 madde 8); başlık adı `CookieSetup.AntiforgeryHeaderName`, ucun doğrulaması D-131 |
| 5 | Doğrulama başarısızsa yazılmaz; 400, yazılanlar ve yeni değerle | ✓ | `Post_WithoutAToken_Is400_WritesNothing_ShowsTheSentValueAMessageAndANewToken_KK4_KK5`, `…ThatAlsoBreaksAFieldRule_ShowsNoRuleCode_…KK5`; `FormSecurityTests.ForgeryRefused_ShowsThePageAgainWithStatus400` |
| 6 | Koruma dışı uçlar kapalı liste; yalnız imzalı bildirim | ✓ | `WritingEndpointAudit` / `EveryPageEndpointOfTheApplication_CarriesTheProtection_AndNoWritingEndpointIsOutsideTheClosedList_KK6` (gerçek uygulama uç veri kaynağı), dört ayrı kırmızı örnek (`TheAudit_IsRedOn…`, `AListedPathThatIsMappedWithTheCheckOn_IsRedToo_KK6`); `FormSecurityTests.TheClosedList_HoldsOnlyTheSignedPaymentNotification_AndTheReturnIsNotOnIt` |
| 7 | Başlıklar makine ucu dahil her yanıtta; CSP dış betik ve gömmeyi yükletmez | ✓ | `SecurityHeadersTests.EveryAnswer_MachineEndpointPageErrorAndStaticFile_CarriesTheFourHeaders_…KK7_KK9`, `ThePolicyAsSent_BlocksAnOutsideScript…AndLetsTheNonceBlockAndGoogleThrough_KK7`, `TheNonce_IsNewInEveryResponse_…KK7`, `APageOfAKeyedLink_SendsNoReferrer_…`; `FormSecurityTests.Policy_HoldsEveryDirectiveOfTheRule_…`. Ödeme çerçevesi T510'da (`AllowPaymentFrame` ona bırakıldı) |
| 8 | Kişisel veri/oturum/sepet/erişim anahtarı yanıtı `no-store` | ✓ | `SecurityHeadersTests.PanelAccountGuestCartAccessKeyOrderAndKeyedLink_AreNotStored_AndTheProductPageAndTheStaticFileAre_KK8`, `EveryPageOfThePanelAndTheAccountFolder_CarriesTheNoStoreAttribute_ByTheFolderConvention_KK8`, `ADeniedOrMissingAnswer_IsAlsoNotStored_…` |
| 9 | Yalnız HTTPS; HSTS'i ters vekil yazar; uygulama `UseHsts` çağırmaz | ✓ | `SecurityHeadersTests…KK7_KK9` (HSTS yok), `TheSameHolds_WhenTheRequestComesThroughTheReverseProxyAsHttps_KK9`; `ForgeryProtectionSourceScanTests.Scan_OfHstsInTheApplication_IsAFinding`; ters vekilde sınama T05 / `VAL-A982` |

### Öz denetim (bitiş kapısı 2)

`git diff origin/main...HEAD` üzerinde kriter 1–9 ve KK1–KK9 madde madde denetlendi (yalnız kalanlar değil); testler bu turda yeniden koşuldu.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (test adı) |
|---|---|---|
| KK1 listelenen işlemler dışında formun etkisi tektir | ✓ | `TheSameFormSentTwice_…KK1`; T208 `VersionedFormPageTests` (409) |
| KK2 aynı onay anahtarı ikinci sipariş açmaz | ✓ | `OneTimeValueTests…KK2` (iki test) |
| KK3 tüketilen değerde ikinci gönderim yeniden doğrulama ister | ✓ | `OneTimeValueTests…KK3` (altı test) |
| KK4 değersiz POST reddedilir | ✓ | `ForgeryProtectionTests…KK4` |
| KK5 reddedilen formda yazılanlar korunur | ✓ | `…KK4_KK5`, `…ThatAlsoBreaksAFieldRule_…KK5` |
| KK6 imzalı uç dışında korumasız POST ucu yok | ✓ | `WritingEndpointAudit` + `ForgeryProtectionSourceScanTests` (uç ve form kuralları) |
| KK7 makine ucunda başlıklar; dış betik engelli; nonce'lu stil bloğu ve Google'a gönderim geçer | ✓ | `SecurityHeadersTests…KK7`; `CspEvaluator` tarayıcı gibi okur (`TheEvaluator_ReadsDefaultSrc…`) |
| KK8 panel, hesap, misafir sepeti, anahtarlı sipariş sayfasında `no-store` | ✓ | `SecurityHeadersTests…KK8` (üç test) |
| KK9 uygulama HSTS yazmaz; değeri T05'te sınanır | ✓ | `…KK7_KK9`, `…ReverseProxy…KK9`, `VAL-A982` |
| `09 §25`: ilgili satırlar | ✓ | Mevcut satır "Yeni POST sahtecilik korumasından geçiyor mu" (§15.2); başlık, `no-store`, satır içi betik/stil için yeni satır eklendi |
| `09 §25`: mimari testler yeşil, ayrı ihlal örneği | ✓ | `ForgeryProtectionSourceScanTests` her kural için kuram + ihlal; uç denetimi için `WritingEndpointAudit` kırmızı örnekleri |
| `09 §25`: `CONTEXT.md` aynı PR'da | ✓ | `.claude/CONTEXT.md` yeni Security satırı |
| `12` ilgili kontrol listesi | ✓ | `12`'de T209 kontrolü `VAL-A982` (KK9) ve `07 §2.5` satırları; ekranlı kayıtlar sahip task'larda. Hedefli D kaydı yok (aşağıda) |
| Güvenlik taraması | ✓ | Jeton eşleşmesi çerez yarısı + kullanıcıya bağlı; yeniden doğrulama değeri yalnız özet olarak saklanır; CSP'de `unsafe-inline` yok; nonce 16 rastgele bayt; `IgnoreAntiforgery*` uygulama kaynağında taranır; yeni dış bağımlılık yok |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | Muaf uç listesi yalnız belgedeki (imzalı bildirim); listedeki yol işaretsiz ya da korumasız eşlenirse kırmızı (`AListedPathThatIsMappedWithTheCheckOn_IsRedToo_KK6`); CSP'de istisna yok, zayıflatma başlık dosyalarında bile bulgu (`Scan_OfAWeakenedPolicy_IsAFinding_EvenInTheHeaderFiles`); başlık yazımı yalnız kendi dosyalarında serbest |
| Sık FAIL 2: tarama çağrı biçimini kaçırıyor | ✓ | Kapatma adları (`IgnoreAntiforgeryToken`, `IIgnoreAntiforgeryToken`, `RequireAntiforgeryToken(false)`, `asp-antiforgery="false"`, `Attribute` sonekli yazım) `Scan_OfAnyNameThatSwitchesTheProtectionOff_IsAFinding` kuramında; kendi adresli form (tek/çok satırlı, başka siteye) `Scan_OfAPostForm…`; her kural için ayrı ihlal örneği |
| Sık FAIL 3: aynı sayı/etiketi anan yerler | ✓ | `09 §15.2` madde 1, 2, 5, `§25`, `CONTEXT.md`, `SignedEndpointRegistration` açıklaması (eski `IAntiforgeryMetadata` anlatımı düzeltildi), durum, hafıza, T208/T211 kayıtları, backlog sayacı (73 → 74) |
| Sık FAIL 4: belgede olmayan davranış / karar kaydısız 09 metni | ✓ | Davranış belgedekiyle aynı; tek ek 404 `no-store` (⚠ 1, gerekçeli); 09 metin değişikliği mekanizma adlarını gerçeğe getirir (⚠ 2) |
| Sık FAIL 5: iki kural birbirini kilitliyor | ✓ | `ApplicationSource_HasNoWayOutOfTheProtection_…` gerçek uygulama kaynağında ve görünümlerinde koşar; `EveryPageEndpointOfTheApplication_…` gerçek uygulama derlemesinde; örnek sayfalar `PageSourceScan` ve öteki taramalardan geçer |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (+Consistency) | ✓ | `dotnet test --no-build` (UnitTests) | 2035 geçti, 0 kaldı |
| Integration | ✓ | `dotnet test --no-build --filter-namespace …` parça parça (PostgreSQL 18, `shopfolio-t209-pg`) | Web 274, Database 35, Model 893, Concurrency+Disposal+FileSecrets+Files+Logging+Outages+Security+SignedEndpoints 167, Jobs+Pages.Panel+AutoGenerated 77 = 1446 geçti, 0 kaldı |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | temiz (bir içe aktarma sırası düzeltildi) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | (doğrulayıcı doldurur) |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T209-tek-kullanimlik-sahtecilik-basliklar`
- **Commit:** son commit PR'ın başıdır
- **PR:** (PR gövdesinde)
- **CI:** yapım işçisi izlemez (yönetici yardımcı düzeni)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

Dış varsayım: yok. Tetikleyici tarama (yeni dış servis, ağ çağrısı, kimlik bilgisi, sabit kodlu adres): değişikliklerde yok; CSP'deki Google adresi yalnız politikada izinli hedef olarak yazılı (`02 §8.3`), çağrı yapılmaz.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Parçalı yükleme ucunun (UC-83) başlık jetonu doğrulaması ve taramanın o uç için gevşetilmesi | D-131 | Uç T312'de kurulur |
| 2 | Ödeme çerçevesi izni (`AllowPaymentFrame`) ve ödeme adımı / anahtarlı sayfalara `[NoStore]` / `[KeyedLinkPage]` | — (T510 ve ilgili sayfa task'ları kriter metninde) | `11` T209 madde 7 "ödeme çerçevesi T510'dadır"; sayfalar kendi task'ında kurulur. Hedefli D kaydı yok |
| 3 | Ters vekilde HSTS sınaması | — (`VAL-A982`) | T05 / ürünle gelen ters vekil yapılandırması |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: CI 42153788573 (aeab023) success, Publish 42153788582 success, CI 42149223864 (da20c4e) success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Branch izolasyonu:** dal yalnız T209 commit'lerini taşır.
- **⚠ 1 — Her 404'e `no-store` eki:** `11` T209 madde 8 yalnız kişisel veri/oturum/sepet/anahtar taşıyan yanıtı sayar; ama T207'nin "aynı 404" kuralı (`07 §2.3.3`) gereği panel ve hesap kapılarının reddi ile gerçek olmayan yol aynı yanıtı vermelidir ve panel/hesap klasörleri 404'te de `no-store` yazar. Ötekiler yazmazsa başlık kümesi hangi kapıya gelindiğini söylerdi (`SessionAccessTests` bunu ister). Bu yüzden `SecurityHeadersMiddleware` başlık yoksa her 404'e yazar; maliyeti yoktur (404 önbelleğe alınmaz). `09 §15.2` madde 5'e "Tek ek" cümlesi olarak yazıldı: "404 yanıtı hangi kapıdan geldiyse `no-store` taşır … 'aynı 404' kuralı başlıklar için de geçerlidir". Test: `ADeniedOrMissingAnswer_IsAlsoNotStored_…`.
- **⚠ 2 — `09 §15.2` madde 1, 2, 5 metin güncellemesi davranışı değiştirmez:** madde 1 ve 2 davranışı aynı bıraktı (her POST korumalı, koruma dışı liste kapalı ve tek öğeli, makine ucu `DisableAntiforgery`); yalnız mekanizma anlatımı gerçeğe getirildi: `AutoValidateAntiforgeryToken` süzgeci yerine `ForgeryProtectionFilter` + işaret, sayfa uçlarında `IAntiforgeryMetadata` bulunmadığı için ("uç veri kaynağı metadata testi" yerine) `WritingEndpointAudit` + kaynak taraması. Sayfa için çıkış yolu belgede zaten yoktu; `IgnoreAntiforgery*` kaynakta yasaklandı. Madde 5 yukarıdaki 404 eki. Üç madde de aynı task'ın kendi mekanizmasıdır; belge kuralı (05/07) değişmedi.
- **Sapma 3 — kapalıya düşen koruma:** `Ignore` niteliği işareti geçersiz kılamaz (`Protection_StaysInForceOnAPageThatCarriesTheIgnoreAttribute_FailClosed`); `Filters.Add<T>()` sırayı 0 yaptığından süzgeç `Add<T>(order)` ile kaydedilir.
- **Sapma 4 — ad:** `ReauthenticationValues.TryConsumeAsync` → `TryRedeemAsync` (`LinkKeySourceScan` "TryConsumeAsync outside OnPost" kuralıyla ad çakışması).
- **Hedefli D kaydı:** `grep -n T209 Docs/DEFERRED_BACKLOG.md` önce yoktu; bu task'ta yalnız D-131 açıldı (hedef T312).
