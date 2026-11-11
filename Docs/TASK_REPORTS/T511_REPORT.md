# T511 — Sipariş takibi girişi, sipariş sayfası ve indirme bağlantısı (E-15, E-16)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **E-15 sipariş takibi girişi** (`Pages/Orders/Tracking.cshtml(.cs)`, `TrackingModel`): `GET /siparis-takibi` iki alanlı formu ve öteki iki yolun (e-postadaki bağlantı, üyenin geçmişi) hatırlatmasını verir; başlık `RecordPages.ClosedScreen` (noindex). `FindOrder` L-5'i iki eksende (yazılan e-posta, istemci adresi) ayrı sayar; eşik aşılırsa 429 + nötr limit mesajı (`Retry-After` yok); eşleşme denemeyi unutur ve 303 → `/siparis/erisim/{anahtar}`; eşleşmeme alanı ve siparişin varlığını söylemeyen nötr cümleyle 422, deneme kaydı kalır ve `OrderLookupFailed` red satırı yazılır.
- **Eşleşme** (`OrderingModule.FindOrderByNumberAndEmailAsync`): numara harf büyüklüğüne duyarsız, e-posta normalleştirilmiş; silinmiş hesabın ve e-postası değişmiş üyenin siparişi sipariş üzerindeki donmuş e-postayla eşleşir; kişisel verisi imha edilmiş sipariş eşleşmez.
- **E-16 sipariş sayfası, iki kök** (`MemberOrder.cshtml`, `AccessOrder.cshtml`, ortak `_OrderPage.cshtml`, `OrderPageModels.cs`): üye kökü `GET /siparis/{numara}` (oturumsuz 302 girişe `returnUrl` ile, yabancı/bağlanmamış sipariş 404, yalnız yönetici oturumu 404); erişim anahtarı kökü `GET /siparis/erisim/{anahtar}` (üyeye de açık; geçersiz, yenilenmiş, imha edilmiş 404; `KeyedLinkPage` → `Referrer-Policy: no-referrer`). Sayfa iki ekseni, beklenen işlemi, kalemleri (kalem kayıtları satırında), dökümü, gerçekleşmiş geri ödemeleri, teslimatı, adresleri ve onaylanan belge sürümlerini (`_OrderDocumentText.cshtml`) gösterir; bekleyen havale bloğu IBAN'ı ve son ödeme gününü taşır.
- **İşlemler sunucudan** (`OrderOperations`, `OrderPageReader`, `OrderPageContracts`): düğmeler işlem kararından çizilir; açık olmayan işlem hiç sunulmaz; sonucu gelmemiş kart ödemesinde "ödeme bekleniyor" metni, kendiliğinden yenileme yok.
- **İndirme** (`OrderPageModel.OnPostDownloadDigitalFileAsync`, `DownloadRefusal`): bir saatlik imzalı bağlantı, 303 → `/indir/{değer}`; hak tek koşullu güncellemeyle düşer, her basış bir haktır; üç 422 sebebi (hak dolmuş, havalede ödeme işaretsiz, dosya yok) düğmenin yerinde mesajla; başka siparişin ya da fiziksel kalemin kimliği 404; red sonrası yeniden okuma 404'e düşerse 404.
- **Modül arayüzleri:** `IAfterSalesModule.ListItemRecordsAsync` (`ItemRecordFacts`, `ItemRecordFactKind`), `IPaymentModule` ödeme olguları (`PaymentFacts`).
- **Çerçeve süzgeci sırası** `StorefrontFrameFilter` 100 → -150 (T508 dalıyla ortak sabit ve yorum): doğrulama 422'si çerçeveli çizilir.
- **09 §15.7 madde 1:** "kalem indirilemez" kapsamı yazıldı (D-218): dijital değil ya da ödeme onaylı değil (`DownloadRules.PaymentConfirmed`); koşul genişletilmedi.
- **Birleştirmeler:** main T504, T543 ve T502 birleştirildi (rebase yok); T504 sonrası her yanıtta yeni nonce olduğundan "aynı 404" karşılaştırmaları `<main>` içerik bölgesini karşılaştırır (`OrderPageTests.ContentOfAsync`, `SessionAccessTests`, `NotFoundPageTests` ×2 uyarlandı).
- **Backlog:** D-122, D-218, D-508, D-565, D-566, D-668 kapandı; D-693, D-780, D-801'in T511 payı notlandı (hedeften T511 çıktı); yeni kalemler D-1194…D-1197 (`Docs/DEFERRED_BACKLOG.d/T511.md`).

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Pages/Orders/*` (Tracking, MemberOrder, AccessOrder, `_OrderPage`, `_OrderDocumentText`, `OrderPageModels.cs`), `Modules/Ordering/{OrderingModule,OrderOperations,OrderPageReader}.cs`, `Modules/Ordering/Contracts/{OrderPageContracts,DownloadRefusal}.cs`, `Modules/AfterSales/{AfterSalesModule.ItemRecordList.cs,Contracts/*}`, `Modules/Payment/{PaymentModule.cs,Contracts/*}`, `Infrastructure/Routing/OrderRoutes.cs`, `Pages/Frame/StorefrontFrameFilter.cs`; testler `tests/Shopfolio.IntegrationTests/{Ordering/OrderTrackingPageTests,Files/OrderPageTests,Web/SessionAccessTests,Web/NotFoundPageTests,Pages/AccessSamples}.cs`, `tests/Shopfolio.UnitTests/Pages/OrderPageLinksTests.cs`, `Support/ApplicationEnums.cs`; `Docs/09_CODING_GUIDELINES.md` §15.7 m.1; kayıt dosyaları.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /siparis-takibi` form + iki yolun hatırlatması; noindex | ✓ | `OrderTrackingPageTests.TheForm_HasTheTwoFieldsAndTheReminder_AndIsClosedToSearchEngines_KK1` |
| 2 | Numara (büyük/küçük harf) + normalleştirilmiş e-posta → 303 erişim köküne; silinmiş hesabın/değişmiş e-postanın siparişi donmuş e-postayla | ✓ | `AMatch_InAnyLetterCase_Is303ToTheAccessKeyRoot_AndForgetsTheAttempt_KK2`, `TheOrderOfADeletedAccount_MatchesItsFrozenEmail_KK2` |
| 3 | 422 boş/biçim; nötr eşleşmeme; imha edilmiş eşleşmez; 429 iki eksen ayrı; eşleşmeyen sorgu kaydedilir | ✓ | `AnEmptyOrMalformedField_Is422_AndCountsNothing_KK3`, `AMiss_IsTheSameNeutralSentence_WhicheverPartWasWrong_AndADisposedOrderNeverMatches_KK3` (deneme kaydı iki eksende 6 satır), `TheEmailAxis_IsCountedApart_…_KK3`, `TheAddressAxis_IsCountedApart_…_KK3` (429, nötr mesaj, `Retry-After` yok) |
| 4 | Üye kökü: oturumsuz 302, yabancı/bağlanmamış 404 | ✓ | `OrderPageLinksTests.MemberPath_…_KK4`; `SessionAccessTests` üye kökü 302/404 testleri |
| 5 | Erişim anahtarı kökü üyeye açık; geçersiz/yenilenmiş/imha 404; yönetici oturumu müşteri değil | ✓ | `OrderPageTests.ARenewedOrDisposedKey_IsTheSame404_KK5`; `OrderPageLinksTests.AccessPath…`, `Buttons_KeepTheAccessRoot`; `SessionAccessTests` yönetici oturumu |
| 6 | İki eksen, beklenen işlem, kalemler, döküm, geri ödemeler, teslimat, adresler, onaylı belge sürümleri | ✓ | `OrderPageTests.ThePage_ShowsTheRegionsAndTheFrozenDocuments…_KK6`, `AnItemRecord_StandsOnTheItemsRow_UnderItsName_KK6`, `AnUnpaidTransferOrder…` |
| 7 | İşlemleri sunucu belirler; açık olmayan işlem sunulmaz | ✓ | `OrderPageLinksTests.NoOpenOperation…`, `OperationsWithoutAScreen…`, `AnOperationOpenForSeveralItems…`, `Buttons_FollowTheOrder…` |
| 8 | Kart sonucu gelmediyse bekleme metni, kendiliğinden yenileme yok | ✓ | `OrderPageTests.ACardOrderWhoseResultHasNotCome_SaysSo_AndHasNoRefresh_KK8` |
| 9 | Bir saatlik imzalı bağlantı, 303 → `/indir/{değer}`; hak tek koşullu güncellemeyle, her basış bir hak | ✓ | `OrderPageTests.TheDownload_Is303ToTheSignedAddress_ThatOpensForOneHour_AndEveryPressSpendsOneRight_KK9` |
| 10 | 422: hak dolmuş, havale işaretsiz, dosya yok; 404: bu siparişin dijital kalemi değil | ✓ | `OrderPageTests.AUsedUpRight…`, `AnUnconfirmedTransferPayment…`, `AMissingFile…`, `AnItemOfAnotherOrderOrAPhysicalItem_Is404…` |

### Öz denetim (bitiş kapısı 2)

Önceki altı işçinin işi dahil dalın bütün diff'i (`origin/main...HEAD`) madde madde denetlendi.

| Madde (kabul kriteri / KK / 12 / sık FAIL) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 form, hatırlatma, noindex | ✓ | `Tracking.cshtml` (`ClosedScreen`); `…_KK1` |
| KK2 eşleşme, büyük harf, donmuş e-posta | ✓ | `FindOrderByNumberAndEmailAsync`; iki `…_KK2` testi |
| KK3 nötr mesaj, L-5, imha | ✓ | `TrackingModel.NotMatchedText` tek sabit; dört `…_KK3` testi; `RejectionLog.Rejected(OrderLookupFailed)` |
| KK4 üç yetki sonucu | ✓ | `MemberOrderModel`; `SessionAccessTests`, `OrderPageLinksTests.MemberPath_…_KK4` |
| KK5 anahtar durumları ve yönetici | ✓ | `AccessOrderModel` (`[KeyedLinkPage]`); `ARenewedOrDisposedKey_IsTheSame404_KK5` |
| KK6 bölümler ve belge sürümü | ✓ | `_OrderPage.cshtml`, `_OrderDocumentText.cshtml`; `…_KK6` |
| KK7 kapalı işlem sunulmaz | ✓ | `OrderPageLinksTests` dört test |
| KK8 bekleme metni, yenileme yok | ✓ | `…_KK8` (`http-equiv="refresh"` yok) |
| KK9 bir saatlik bağlantı, hak düşer | ✓ | `…_KK9` |
| KK10 üç 422 sebebi ve 404 | ✓ | `OrderPageTests` dört test; `DownloadRefusal` |
| 12 VAL-A054 (numara tek başına açmaz), VAL-B015, VAL-B016 | ✓ | `MemberPath_…_KK4` (302/404), `…_KK2`, `…_KK5` |
| 12 VAL-A488 `OrderLookupFailed` nötr 422 | ✓ | `AMiss_IsTheSameNeutralSentence_…_KK3` |
| 12 VAL-A491 `DownloadNotAvailable` 422, düğmenin yerinde mesaj | ✓ | KK10 testleri |
| 12 VAL-A636 her basış bir hak | ✓ | `…_KK9` (iki basış, iki hak) |
| 12 VAL-A981 anahtarlı sayfada `no-referrer` | ✓ | `[KeyedLinkPage]` → `SecurityHeadersMiddleware`; `SecurityHeadersTests` yeşil |
| 12 VAL-A353 (L-5 429 uç yanıtı, D-801 payı) | ✓ | `TheEmailAxis_…_KK3`, `TheAddressAxis_…_KK3` |
| 12 tarayıcı satırları (VAL-B014, A055, A057, A062) | ⚠ | tarayıcı gözlemi doğrulayıcının adımında; tarayıcısız PASS'a çekilmedi |
| 09 §25 ilgili satırlar (geniş catch yok, satır içi stil/olay özniteliği yok, modül sınırı arayüzden, tek kural evi) | ✓ | e-posta denetimi `[InvalidEmail]`, zorunlu alan `[FieldRequired]` (tek kural evi); sayfa modüllere arayüzle erişir (mimari testler Unit'te yeşil); görünüm taramaları (`FrameSourceScanTests`, `FormSourceScanTests`) yeşil |
| Güvenlik taraması | ✓ | anahtar metin olarak görünmez (yalnız 303 hedefi); nötr mesaj sipariş varlığını sızdırmaz; `returnUrl` `ReturnPath` ile site içi; yeni dış bağımlılık yok; sır yok |
| Sık FAIL 1 — bekçi/tarama istisnası geniş | ✓ (yok) | yeni tarama istisnası eklenmedi |
| Sık FAIL 2 — kaynak taraması çağrı biçimi kaçırıyor | ✓ (yok) | yeni tarama kuralı yok; yeni görünümler mevcut taramalara girdi |
| Sık FAIL 3 — 06 Varsayılan / FK dizini | ✓ (yok) | şema/göç değişikliği yok |
| Sık FAIL 4 — kural değişince öteki yerler | ✓ | çerçeve süzgeci sırası T508 dalında aynı sabit/yorum; nonce sonrası "aynı 404" karşılaştıran bütün testler (`SessionAccessTests`, `NotFoundPageTests`, `OrderPageTests`) içerik bölgesine geçti |
| Sık FAIL 5 — 09 metni karar kaydısız | ⚠ | 09 §15.7 m.1 yalnız kapsamı yazar (D-218: durumlar sayıldı, koşul genişlemedi); kapanmış dijital kalem sorusu D-1197'de açık |
| Sık FAIL — Integration TAM / birleşik ağaç | ⚠ | main (T502) birleşik ağaçta Unit TAM ve ilgili Integration sınıfları koşuldu; tam Integration yerelde süre sınırına takıldı (aşağıda), CI tam koşar |
| Backlog: bu task'a devredilen kalemler | ✓ | D-122, D-218, D-508, D-565, D-566, D-668 kapandı; D-693, D-780, D-801 T511 payı notlandı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` (main T502 birleştirmesi sonrası) | 7139/7139 |
| Integration | ✓ (parçalı) | Postgres 18 `track_commit_timestamp=on`, `SHOPFOLIO_TEST_DATABASE`; `--filter-namespace` / `--filter-class` | Birleşik ağaçta (T502 sonrası): Web ad alanı 519/519 (10 dk zaman aşımında kesildi, kırmızı yok); ayrıca `StorefrontHomeAndCategoryTests`, `SessionAccessTests`, `NotFoundPageTests`, `StorefrontFrameTests`, `UnexpectedErrorLayerTests`, `SitemapEndpointTests` 134/134; `SearchEngineDataTests`, `AddressRoutingTests`, `SecurityHeadersTests`, `FormComponentTests`, `ForgeryProtectionTests`, `AttemptLimitPatternTests`, `OrderTrackingPageTests`, `OrderPageTests` 124/124. Önceki turlarda: Web 651/651 (T543 öncesi), `Ordering` ad alanı 193 geçti (zaman aşımı, kırmızı yok) |
| Derleme / biçim | ✓ | `dotnet build` (Integration, Unit); `dotnet format <proje> --verify-no-changes --include <değişen dosyalar>` | 0 uyarı, 0 hata; biçim temiz |

Yerelde koşulmayan ad alanları (Model, Database, Catalog, Commands, Concurrency, Disposal, Logging, Jobs, Payment, Outages, Security, Administration, CorporateContent, FileSecrets, SignedEndpoints, Pages ve Ordering'in kalanı): makine yavaş (sınıf başına 1–2 dk), CI tam koşar.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✓ PASS — tur 1 (2026-11-11, head `20c8bbcc` = dal `1064e43f` + `origin/main` `25f1f1ee` (T508 #544) birleştirmesi; çakışma yalnız `DEFERRED_BACKLOG.md` kayıt satırlarında, kurala göre çözüldü). Kriter 1–10 ve KK1–KK10 ✓ (kanıtlar: `OrderTrackingPageTests` 7, `OrderPageTests`, `SessionAccessTests`, `NotFoundPageTests`, `OrderPageLinksTests`; doğrulayıcı sondaları — eşleşen sorgunun 303'ü çerez ve deneme kaydı bırakmaz; ret satırları `OrderLookupFailed`/`OrderLookupLimitExceeded`/`DownloadNotAvailable` uygulama kaydında, e-posta ve anahtar kayıtta yok; numara tek başına 302; havale IBAN'ı tam, kargo adı + takip no + "Takip et", kendi aracı beyanı, gönderilmiş geri ödeme tarih/tutar/yol ile, gönderilmemiş ve geçersizleşmiş yok — sondalar silindi). Para/yetki/IDOR: indirme kalemi önce çözülen kökün siparişinde ve dijital olmalı (değilse 404, hak düşmez), kural kapıda kilit altında yeniden okunur, hak tek koşullu güncellemeyle düşer; tutarlar siparişin donmuş sütunlarından, geri ödemeler yalnız gönderilmiş ve geçersizleşmemiş kayıtlardan; üye kökü sahiplikle, yönetici oturumu 404. `09 §25` ilgili satırlar ✓ (POST `[GatedOperation]` + sahtecilik, `[NoStore]`, `[KeyedLinkPage]`, betik/meta yenileme yok, ret satırı `07` koduyla, yeni bağımlılık/şema/sır yok; GET indirme düğmesi ayrı koşuldan — K2). Yerel: derleme `-warnaserror` 0 uyarı, `dotnet format` üç proje temiz, Unit 7150/7150, betik testleri 4/4, Integration TAM ad alanı parçalarıyla (`track_commit_timestamp=on`): T511 sınıfları 67, Ordering 645, Model 1407, Web 755, Administration+Catalog+Commands+Concurrency+CorporateContent 436, Database+Disposal+FileSecrets 121, Files+Jobs+Logging+Outages 280, Payment+Security+SignedEndpoints+Pages 104 — hepsi 0 kırmızı (Web'in önceki iki koşusunda ve bir parça koşusunda ~17 sn'lik farklı kırmızılar yeniden koşularda yeşil: yük kararsızlığı, T511 kodu dışında). Dal CI run 43069617924 success (9 iş). `12` satırları PASS: `VAL-B015`, `VAL-A605`, `VAL-A488`, `VAL-A489` (KK2, KK3), `VAL-A054`, `VAL-A977` (KK4), `VAL-B016`, `VAL-A609` (KK5), `VAL-A056`, `VAL-A061`, `VAL-A063` (KK6), `VAL-B018`, `VAL-A065`, `VAL-A636` (KK9), `VAL-A064`, `VAL-A491` (KK10); tarayıcı gözlemi isteyen satırlar (`VAL-A058`, `VAL-A391`, `VAL-A981`, `VAL-B014`, `VAL-A055`, `VAL-A057`, `VAL-A062`, `VAL-A883`) uç kısmı karşılansa da BEKLEMEDE; öteki task'larla paylaşılan satırlar (`VAL-A353`, `VAL-A604`, `VAL-A773`, `VAL-A864`, `VAL-A868`, `VAL-B011`, `VAL-B017`, `VAL-A059`, `VAL-A060`, `VAL-A066`, `VAL-D013`, `VAL-D014`) BEKLEMEDE. |
| Bulgu sayısı (tur 1) | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (tur 1) | 3 — **K1** (D-1258'e forward): `04` 5.16.14 "IBAN'ı kopyalamak" E-16'nın havale bloğunda yok. **K2** (D-1259'a forward): indirme düğmesinin GET'teki açıklığı `OrderAccess.GetSummaryAsync`'in kendi koşulundan, POST işlem satırının kuralından; bugün eşit, D-1197 satırı değiştirirse ayrışır. **K3** (bu PR'da kapatıldı, kod okuması): KK2 "değişmiş e-posta" ayrı testle sınanmıyor; sorgu yalnız siparişin donmuş `NormalizedContactEmail`'ini okur ve onu yalnız `Order.SetContactEmail` (oluşturma, imha, misafir e-posta düzeltmesi) yazar — hesap e-postasının değişmesi siparişe dokunmaz. |
| Yapım raporu karşılaştırması (tur 1) | Kabul kriterlerinde tam uyum (1–10 ✓); öz denetim tablosu var, maddeler kanıtlı. Uyuşmazlık 2: (1) öz denetim `VAL-A981`'i ✓ sayar; satırın kanıt türü tarayıcı gözlemi de istediği için doğrulamada BEKLEMEDE kaldı (uç kısmı `SecurityHeadersTests` ile karşılanıyor). (2) Integration yapımda TAM koşulmamıştı; doğrulamada TAM koşuldu, kırmızı yok. |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T511-siparis-takibi-sayfasi`
- **Commit:** PR başlığına ve dal geçmişine bakınız
- **PR:** #545
- **CI:** dal run 43069617924 (head `20c8bbcc`) success — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E (advisory); Guard atlandı
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sipariş sayfasında IBAN alanı | D-1194 (T512) | `SaveRefundIban` T512'nin |
| 2 | Kalem kaydının ayrıntıları satırda | D-1195 (T513) | cayma/ayıp ekranları T513'te |
| 3 | İşlem düğmelerinin hedef ekranları | D-1196 (T512, T513) | ekranlar o task'larda |
| 4 | Kapanmış dijital kalemin indirme düğmesi | D-1197 (T512) | `03 §1.11.14` okuması kalem iptalini yazan task'ta |

## Notlar

- **Working tree (Adım -1):** temiz
- **Repo memory:** MEMORY'ye, STATUS "Son güncelleme" satırına ve changelog ana dosyasına dokunulmadı (parça kuralı); olay `Docs/STATUS_CHANGELOG.d/T511.md`, backlog `Docs/DEFERRED_BACKLOG.d/T511.md`.
- **⚠ (öneriyle karar verildi):**
  1. Çerçeve süzgeci sırası -150 T508 ile ortak sabit; değiştirilmedi.
  2. 09 §15.7 m.1 kapsam metni karar kaydısız (koşul genişlemedi, D-218'in istediği sayım).
  3. D-1197: kapanmış dijital kalemde indirme düğmesi — `03 §1.11.14` okuması T512'ye.
  4. D-566 karar kaydısız kapandı: havale bloğunun görünürlüğü `04` 5.16.4'te zaten yazılı.
  5. T504 testlerinin T511'e uyarlanması (`NotFoundPageTests` ×2, `SessionAccessTests`, `OrderPageTests` nonce): "aynı 404" karşılaştırması `<main>` içerik bölgesinde.
  6. Integration tam koşu yerelde süreye sığmadı; ilgili sınıflar birleşik ağaçta yeşil, CI tam koşar.
  7. Tarayıcı isteyen `12` satırları doğrulayıcının tarayıcı adımına bırakıldı.
