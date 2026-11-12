# T532 — Talepler ve iletişim talebi ayrıntısı (E-38, E-39)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (PR açıldı, doğrulama bekliyor) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /panel/talepler` (E-38): iletişim ve ayıp talepleri tek listede; `requestType` ve `requestStatus` çok değerli, arama ve konu süzgeci yok; sıra açılış tarihine göre en yeni önce (ayıp talebinde son açılış); geçersiz değer süzgeç uygulamaz; boş hâl satırı; sayfalama.
- `GET /panel/talepler/{iç anahtar}` (E-39) ve `CloseContactRequest` / `ReopenContactRequest` işleyicileri (UC-94, UC-155, UC-156); "Siparişlerde ara" ve "Üye kaydında ara" e-postayla.
- Saklama kuralı tek yerde (`ContactRequestRetention`): gece imhası, liste, sayaç, ayrıntı, kapatma ve yeniden açma aynı koşulu kullanır.
- Ayıp talebi okuması: `AfterSalesModule.PanelList.cs` (`SELECT d.*` + son açılış tarihi; durum süzgeci, sıra, sınır LINQ ile), kalem etiketi `OrderingModule.FindItemLabelsAsync`, "e-posta ulaşmadı" işareti `NotificationsModule.FindClaimsWithUndeliveredMarkAsync` (B-9 dağıtım kaydı).
- `Talep kapatıldı` ve `Talep yeniden açıldı` bildirim metinleri `NoticeTexts`'te (D-1232 kapandı); `_PanelTable` rozet eki; `admin.css` süzgeç ve ayrıntı stilleri.
- Önceki iki işçi bağlam ve süre sınırında bölündü; bu tur ayıp satırı sayfa testleri (4 Integration), `dotnet format`, kayıtlar ve rapor.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/Requests.cshtml(.cs)`, `RequestDetail.cshtml(.cs)`, `Pages/Components/RequestTexts.cs`, `Pages/Lists/ListModels.cs`, `Pages/Shared/_PanelTable.cshtml`, `wwwroot/css/admin.css`
- `Modules/Administration` (`PanelRequestListService`, `PanelRequestContracts`, `PanelRequestKind/Status`), `Modules/AfterSales` (`AfterSalesModule.PanelList.cs`, `PanelDefectClaims`), `Modules/CorporateContent` (`ContactRequests`, `ContactRequestDisposalPart`, sözleşmeler), `Modules/Notifications` (`DispatchMarks`), `Modules/Ordering` (`FindItemLabelsAsync`), `Infrastructure/Notices/NoticeTexts.cs`
- Testler: `tests/Shopfolio.UnitTests/Pages/RequestScreensModelTests.cs`, `LegalTextsModelTests.cs`, `Support/ApplicationEnums.cs`; `tests/Shopfolio.IntegrationTests/Web/RequestsPageTests.cs`, `RequestsPageTests.DefectRows.cs`, `RequestsDefectClaimTests.cs`
- Kayıt: `Docs/DEFERRED_BACKLOG.d/T532.md` (D-1302…D-1306), `Docs/DEFERRED_BACKLOG.d/T507.md` (D-1232 ✅), `Docs/STATUS_CHANGELOG.d/T532.md`, `Docs/IMPLEMENTATION_STATUS.md` T532 satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek liste, çok değerli tür ve durum süzgeci, arama ve konu süzgeci yok | ✓ | `RequestsPageTests.TheList_ShowsTheContactRequestsNewestFirst_WithBothFilters_AndNoSearch_KK1_KK2`, `TheStatusFilter_TakesSeveralValues_…_KK1_KK3`, `TheTwoKinds_StandInOneListByTheLastOpening_…_KK1_KK2_KK3` |
| 2 | Sıra açılışa göre; yeniden açılan ayıp son açılışla | ✓ | `TheTwoKinds_…_KK2`, `RequestsDefectClaimTests.TheList_OrdersByTheLastOpening_…_KK2` |
| 3 | "Açık" satır sayısı = ana sayfa sayacı; süresi dolan yok | ✓ | `TheStatusFilter_…_AndTheOpenRowsEqualTheHomeCounter_KK1_KK3`, `TheTwoKinds_…` (iki tür sayaç eşitliği), `ARequestPastItsRetention_…_KK3`, `RequestsDefectClaimTests.TheCount_FollowsTheSameStatusFilter_KK3` |
| 4 | İletişim → UC-94, ayıp → sipariş bölümü, ikisi `returnUrl`; ulaşmamış işaret | ✓ | `ARowOpensTheRequest_WithTheListAddressInReturnUrl_…_KK4`, `ADefectClaimRow_NamesTheItemWithItsOptions_LeadsToTheOrderSection_…_KK4` |
| 5 | Geçersiz süzgeç uygulanmaz; boş hâl | ✓ | `AnInvalidFilterValue_AppliesNoFilter_AndAnEmptyResultIsTheEmptyStateRow_KK5`, `AnEmptyList_SaysThereIsNoRequestYet_KK5` |
| 6 | Ayrıntı: konu, durum, gönderen, mesaj, ulaşma anı, son kapatılış, süre tarihi | ✓ | `TheDetail_ShowsTopicStatusSenderMessageAndTheDates_KK6`, `TheDetail_OfAClosedRequest_…_KK6_KK10` |
| 7 | Tek birincil düğme; iki arama e-postayla; adres maskeli | ✓ | `TheDetail_HasOnePrimaryButtonByStatus_AndTwoSearchesWithTheEmailInTheQuery_…_KK7` |
| 8 | Yok ya da imha edilmiş: üç işleyicide 404 | ✓ | `AMissingOrDestroyedRequest_Answers404_OnTheGet_AndOnBothPosts_KK8` |
| 9 | Kapatma: Kapatıldı, `LastClosedAt`, sayaçtan düşer, bildirim yok; ikinci gönderim 303 | ✓ | `Close_SetsClosedAndTheClosingMoment_SendsNoNotice_AndASecondPostIs303WithoutANotice_KK9` |
| 10 | Yeniden açma: Açık, `LastClosedAt` değişmez, süre durmaz; ikinci gönderim 303 | ✓ | `Reopen_SetsOpenAndKeepsTheLastClosing_…_KK10` |

> `? Doğrulanamadı` **FAIL değildir** — kanıt eksikliğidir. FAIL'den ayrı tutulur.

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil kriter 1–10 ve KK1–KK10 bütünüyle diff'te denetlendi; `sik-fail-nedenleri.md` beş ana tür ayrıca.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: iki tür, üç durum; arama yok | ✓ | `Requests.cshtml.cs` (`RequestQuery` yalnız `RequestType`, `RequestStatus`); `TheList_…_KK1_KK2`, `TheTwoKinds_…` (Açık, Çözüldü, Kapatıldı süzgeçleri) |
| KK2: iki sıra durumu | ✓ | `TheTwoKinds_…_KK2` (yeniden açılan ayıp "son açılış"), `RequestsDefectClaimTests` |
| KK3: sayaç eşitliği; süresi dolan yok | ✓ | `ContactRequestRetention.Kept` ortak koşul; iki sayaç testi |
| KK4: iki bağlantı; işaret | ✓ | `ADefectClaimRow_…_KK4` (işaretli ve işaretsiz satır), `…ReturnUrl_…_KK4` |
| KK5: geçersiz değer; boş hâl | ✓ | KK5 iki test |
| KK6: tarihler; KVKK ulaşma anı | ✓ | `…_KK6`, `…_KK6_KK10`; `RequestScreensModelTests.TheDetailScreen_KeepsTheRetentionLinesApart` |
| KK7: iki düğme; iki arama; maske | ✓ | `…_KK7`; adres kayıtlarda maskeli: bütün sorgu değerleri kayıt yolunda `PathMasker` ile `*` olur (mevcut kural, `LoggingConsistencyTests`); bağlantıda `Uri.EscapeDataString` |
| KK8: üç işleyicide 404 | ✓ | `…_KK8` |
| KK9: kapatma; ikinci gönderim | ✓ | `…_KK9` |
| KK10: süre durmaz; ikinci gönderim | ✓ | `…_KK10` |
| `09 §25`: sayfalı listeye sınır testi (aynı an, son sayfa, taşan sayfa sayısı) | ✓ | `APageBoundaryInsideOneMoment_LosesAndRepeatsNoRow_TheLastPageAndAHugePageNumberAreTheLastPage_KK1` |
| `09 §20`: N+1 yok | ✓ | `TwoPages_SendTheSameNumberOfCommands_WhateverTheNumberOfDefectClaimsOnThem_KK1` |
| `09 §9`: modül arayüz yöntemi kendi işlemini açmıyor | ✓ | Kapatma ve yeniden açma `Transactions.RunAsync` ile sayfa katmanında sarılır (`RequestDetail.cshtml.cs`) |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | Yeni istisna eklenmedi; `ApplicationEnums.NotStored` yalnız üç adlı enum |
| Sık FAIL 2: kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama kuralı eklenmedi; mevcut `ListSourceScan`, `FrameSourceScan` ve mimari taramalar yeni kodu da görüyor, Unit yeşil |
| Sık FAIL 3: "Varsayılan" DB default'u / FK dizini | ✓ | Şema değişikliği yok |
| Sık FAIL 4: kural değişince öteki yerler | ✓ | Saklama koşulu tek yerde (`ContactRequestRetention`), imha da onu kullanır; bildirim metin tablosu ve `NoticeCodeConsistencyTests` güncel; üç test sahtesine yeni `ICorporateContentModule` üyeleri eklendi |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09` değişmedi |
| Modül kurucusuna yeni bağımlılık, elle kuran test düzenekleri | ✓ | `PanelRequestListService` DI ile kurulur; elle kuran düzenekler (`PanelHomeTests`, `SalesSummaryTests`, `SitemapSourceTests`) güncellendi; Integration TAM aşağıda |
| Güvenlik taraması | ✓ | İki POST işleyicisi çerçevenin sahte istek korumasından geçer (`TheScreens_RefuseAGuest_KK1`, form `ForgeryTokenHandler`); yönetici oturumu şart; iç anahtar `Guid` bağlanır; ham SQL parametresiz sabit metin, kullanıcı girdisi yalnız LINQ parametresi; e-posta bağlantıda kaçışlı, kayıt yolunda `PathMasker` ile maskeli, işlem izine yazılmaz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 7233/7233 (önceki işçi, bu turda Unit kodu değişmedi) |
| Integration (T532 sınıfları) | ✓ | `--filter-class "*RequestsPageTests"`, `"*RequestsDefectClaimTests"` | 17/17 ve 2/2 |
| Integration TAM | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-namespace "Shopfolio.IntegrationTests.<Ad>"` (ad alanı parçaları) | 3836 test, 0 kırmızı: Administration 69, CorporateContent 185, Catalog 82, Commands 70, Concurrency 30, Database 36, Disposal 61, FileSecrets 24, Files 112, Jobs 85, Logging 30, Outages 53, Payment 59, Security 34, SignedEndpoints 11, Ordering 645, Model 1410, Web 840 (Pages ve kök ad alanında test yok, örnek sayfalar) |
| `dotnet format` | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (Web, Unit, Integration) | değişiklik yok (bir boş satır düzeltildi) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T532-talepler`
- **Commit:** son baş SHA PR açıklamasında
- **PR:** #552
- **CI:** yoklanmadı (yapım işçisi CI izlemez)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Üç kaynak (belge, gerçek PostgreSQL 18, ASP.NET Core test sunucusu) kendi depodan; ağ çağrısı yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ayıp satırı parça tanımlayıcısı sipariş sayfasında | D-1302 | E-37 ayıp bölümü T528'in |
| 2 | Kalem adı biçimi belgede yazılı değil (⚠, kurala uygun seçimle karar verildi) | D-1303 | Belge kararı |
| 3 | Kapatma/yeniden açma sonrası `returnUrl` taşınmaz (UC-155/156 yalnız ayrıntı adresi der) | D-1304 | Belge UC-155/156 |
| 4 | Eşit anda sıra, rozet eki | D-1305 | Belge UC-93 |
| 5 | `search` parametresi sipariş ve üye listelerinde; ham SQL eşlemesi | D-1306 | T527, T533 |

D-1232 (iki bildirim metni) bu task'ta kapandı.

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yönetici düzeninde yardımcı izler
- **Repo memory (bitiş kapısı 9):** `MEMORY.md` "Güncel Durum" ve backlog sayacı parça kuralıyla dokunulmadı
- **VAL satırları:** `VAL-A256`, `C063`, `C064`, `A556`, `A735`, `A736`, `B048` doğrulayıcının PASS'ı ile işaretlenir (yapım PASS yazmaz)
- **Integration TAM:** ad alanı parçalarıyla ön planda koşuldu, hepsi yeşil; DB silme zaman aşımı olmadı. Postgres 18 konteyneri `track_commit_timestamp=on` ile (CI gibi)
