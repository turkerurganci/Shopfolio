# T534 — Kurumsal içerik ve marka yönetimi (E-41, E-43)

**Faz:** F5 | **Durum:** ✓ Tamamlandı — doğrulama tur 1 PASS | **Tarih:** 2026-11-14

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Modül yüzeyi** (`Modules/CorporateContent/PanelContentEditing.cs`, `Contracts/PanelContentContracts.cs`, `ICorporateContentModule` yedi yöntem): liste (yedi grup, elle sıra, sayfasız), form, kayıt (kayıt + galeri/şube görseli + bağlı ürünler tek işlemde), mutlak yayın geçişi, silme, sıra değişimi. Ret sonuçları değerdir; işlemi açan sayfa/yürütücüdür.
- **Sayfalar** (`Pages/Panel`): `ContentList` (UC-96, UC-161), `ContentForm` + `NewContent` + `_ContentFormBody` (UC-96, UC-158…UC-160, UC-117, `SearchProducts`), `Brand` (UC-98, UC-163, UC-117), `ContentRoutes` (kapalı `{tip}` listesi). `admin.css` yeni sınıfları (`.content-*`, `.brand-form__*`), satır içi stil yok.
- **Backlog:** D-673 (`ContentPublishing.DeleteAsync` kaydı yapmaz), D-720 (`ProductLocks.ShareKeysAsync`, `FOR KEY SHARE`; `ShowcaseReader.FindProductIdsAsync` işlem içindeyse onu çağırır), D-1177 (`ListCatalog.IsPaged`, `PageSizeOf(PanelContent)` istisna) kapandı. D-442 F5 kapısına ileri atıldı (gerekçe: gönderen markayı tek okumadan alıyor; logo mutlak adresi `VAL-A159` e-posta bileşiminin işi, kriterlerde yok).
- **Bekçi uyumları:** yedi `PanelContent*` enum'u kendi dosyasında ve `ApplicationEnums.NotStored`'da; `ConcurrencySourceScan` `ProductLocks.cs`'te yalnız `FOR KEY SHARE`'e izin verir (+ ihlal örneği testi, `09 §10.2` m.5 metni); `ReorderContentInput` ve satır `Remove` alanları `SpecAssignedName`; `!` işleçleri gerekçeli.

## Öz denetim (kabul kriterleri ve kontrol listesi — bütün maddeler)

| Madde | Durum | Kanıt |
|---|---|---|
| K1 / KK1 yedi grup, elle sıra, rozet ve duyuru görünürlüğü, sayfasız | ✓ | `PanelContentEditingTests.TheList_HasTheSevenGroupsInOrder_…`; `ContentPanelPageTests.TheList_ShowsTheSevenGroupsInOrder_WithoutAPager_…`; `ListComponentModelTests…D1177`; duyurunun görünürlüğü `PanelContentRow.IsShownNow` (`ContentVisibility`) |
| K2 / KK2 sürüm, 404, tekil kayıt 303 | ✓ | `…TheForm_OfARecordThatIsNotThere_OrIsAnotherTypes_IsNull…`; `…AFormOutsideTheClosedTypeList_OrOfAMissingRecord_Is404`; 303 kodu `ContentFormModel.OnGetAsync` (`FindSinglePanelContentAsync`, `ASecondAnnouncement_IsRefused_…`) |
| K3 / KK3 kayıt Taslak, grup sonu, işaret, galeri, bağlı ürün | ✓ | `…ASave_OfANewOffering_CreatesADraftAtTheEndOfItsGroup_WithItsGalleryAndLinkedProducts`; sayfa: `…TheNewForm_Opens_AndASave_MakesADraft_…` |
| K4 / KK4 422: alan, adsız taslak, yayındaki kapı, galeri tavanı, silinmiş görsel; 409: sürüm, tekil | ✓ | `…ASave_WithAnEmptyRequiredField_IsRejected…`, `…EditingAPublishedRecord_ToEmptyAGateField_IsRefused…`, `…ASave_WithMoreImagesThanTheCeiling_OrADeletedImage_…`, `…ASave_WithAStaleVersion_IsAConflict…`, `…ASecondAnnouncement_…` |
| K5 / KK5 `SearchProducts` yazmaz, seçilen sona eklenir | ✓ | `ContentPanelPageTests.TheProductSearch_WritesNothing_AndTheTickedResult_JoinsTheLinkedProducts_OfTheSameForm` |
| K6 / KK6 mutlak durum, eksik kapı 422, adres sabit, iz, sürüm | ✓ | `…TheStatusMove_IsAbsolute_ChecksTheGate_WritesTheTrail_…`; `…ThePageAddress_OfAPublishedOffering_StaysFixed_…`; sayfa `…APublishMove_OfAnIncompleteRecord_Is422_…` (açık formun 409'u: sürüm `ChangeStatus`'ta artar, K4'ün sürüm testi) |
| K7 / KK7 silme: onaysız 200, onaylı siler, Hakkımızda 422, tekrar 404 | ✓ | `…ADeletion_IsSavedByTheTransactionsOwner_AndTheAboutPageIsRefused_D673`; sayfa `…ADeletion_WithoutTheMark_ShowsTheConfirmation_…` |
| K8 / KK8 sıra: yer değiştirir, 409 yazmaz, Hakkımızda/duyuru/sınır 422 | ✓ | `…AMove_OneOrderUp_SwapsTheNeighbours_AStaleScreenIsAConflict_AndTheEdgesAndSinglesAreRefused`; sayfa `…AMove_WithTheShownOrders_…` |
| K9 / KK9 marka geri düşüşü, kayıt | ✓ | `…TheBrandScreen_Opens_…AGoodSave_RedirectsAndShowsTheNewName`; geri düşüş `BrandIdentity` (`Brand.cshtml.cs` `Draw`) |
| K10 / KK10 422 boşaltılan ad / bozuk renk / silinmiş görsel; 409; iz | ✓ (görsel kısmı kodda) | `…AFilledBrandName_ThatIsEmptied_Is422_…`, `…AnInvalidColor_Is422_AStaleVersion_Is409_…`; silinmiş görsel `FieldCodes.InvalidImage` → `ExpiredImageText` (`Brand.cshtml.cs`); marka siparişe donmaz (kayıt yalnız ayar satırını yazar, sipariş okumaz); iz modülün yazıcısında (`BrandingSettings`) |
| `09 §25` / `§16.2` satırları | ✓ | tek işlem sahibi (modül `SaveChanges` çağırmaz), kilit tek yerde (`ConcurrencySourceScan`), enum dosya kuralı, `!` gerekçesi, satır içi stil yok (`…ShowsTheSevenGroups…` `style=` yok), sıra/sınır testleri |
| Güvenlik taraması | ✓ | yönetici oturumu çerezi gerekli (`ForgeryTokenHandler` + `AdminSession`); `{tip}` kapalı liste (404); formda sahte anahtar 404; sürüm/sıra yarışı 409; görsel yalnız yüklenen kimlikle |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | `ProductLocks.cs`'te yalnız `FOR KEY SHARE`; düz `FOR SHARE` ihlal örneği testinde hâlâ bulunur |
| Sık FAIL (2) taramalar bir biçimi kaçırmıyor | ✓ | `KeySharedLock` örneği (satır 9) hem ProductLocks'ta serbest hem başka yerde yakalanır (mevcut test) |
| Sık FAIL (3) 06 "Varsayılan"/FK dizini | yok | şema değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | 09 ürün satırı kilidi cümlesi + tarama belge yorumu + `07` alan adı eşitlendi; ModelBinding/Enum kilit testleri koştu |
| Sık FAIL (5) belge kuralı değişimi karar kaydısız | ✓ | `09 §10.2` m.5 eki D-720 (hedefi T534 olan kalem) ile gerekçeli; `07` alan adı sapması D-1537 |
| Hedefi T534 olan kalemler | ✓ | D-673 ✓ kapandı, D-720 ✓ kapandı, D-1177 ✓ kapandı, D-442 gerekçeli ileri (yukarıda) |

## Sınama

- Build `-warnaserror` (tüm çözüm) 0 uyarı/0 hata; `dotnet format` değişen 34 `.cs` dosyasında temiz.
- Unit TAM 7643/7643 (ek: `ContentRoutesTests`, `…D1177`, tarama örnek testi).
- Integration (hedefli, `main` birleşmesi sonrası): `ContentPanelPageTests`, `PanelContentEditingTests`, `ContentAuthoringTests`, `ContentStatusTests`, `ContentRelationsTests`, `StorefrontReadersTests`, `ProductFormPageTests` 178/178 — **TAM: CI**.

## ⚠ Kolay kararlar (kurala en uygun seçenek)

1. `SaveContentInput.RelatedProducts` → `LinkedProducts` (`SalesModelShape` yasağı), `07 §5.2.2` eşitlendi — D-1537.
2. Ana sayfada işaretli sayısı taslağı da sayar (K-755 belirsiz) — D-1535.
3. Sıra 409/422 ileti metni kısa cümle (04 2.10.2 belirsiz) — D-1536.
4. Marka logo/simge kaldırma ve ürün satırı "Kaldır" işareti `07`'de yok — D-1538.
5. Şube görselinde sürüm iki kez artabilir — D-1539.
6. `09 §10.2` m.5'e `FOR KEY SHARE` eki (D-720 gereği).

## Bilinen Eksikler / Takip

- Tarayıcı satırları VAL-B050, VAL-B085 (320/768 genişlik) doğrulayıcının işi; yapımda tarayıcı gözlemi yok.

## Doğrulama

**Tur 1 — 2026-11-14 — ✓ PASS** (bağımsız doğrulayıcı; dal `task/T534-kurumsal-icerik-marka`, bakılan uç `0c928897`, main `6e6d4a41` — dal main'in güncel ucunu içeriyor, birleştirme gerekmedi). Bulgu 0; K-notu 2 (backlog'a); düzeltme gerekmedi.

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `scripts/git-hooks` (ana çalışma alanının mutlak yolu); main CI son 3 tamamlanmış run `success` (43575320382, 43575320271, 43570435627).

**Kabul kriterleri (doğrulayıcı kanıtı):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `PanelContentEditing.ListAsync` yedi grup sabit sırada, `SortOrder`/`CreatedAt`/`Id`; duyuru `ContentVisibility.Decide` ile okunan hâl; sayfalama yok (`ListCatalog.IsPaged`). Testler: `TheList_HasTheSevenGroupsInOrder_…`, `TheList_ShowsTheSevenGroupsInOrder_WithoutAPager_…`, `…D1177` |
| 2 | ✓ | Form `VersionFields.Version` taşır (geçici sınama: kaydın formunda `Input.Version` = 1); `{tip}` dışı ve yabancı anahtar 404 (`AFormOutsideTheClosedTypeList_…`); Hakkımızda ve duyuru varken `/yeni` GET → 303 var olan kaydın adresine (geçici sınama, iki tip) |
| 3 | ✓ | Yeni kayıt Taslak, grup sonu (`SortOrder` 2), galeri + bağlı ürün sırası (`ASave_OfANewOffering_…`, `TheNewForm_Opens_…`); "ana sayfada göster" yazılır (geçici sınama: `is_shown_on_homepage` = true); yedi tipin hepsi sayfadan Taslak doğar (geçici sınama); görsel yüklemesi `ICatalogModule.UploadImageAsync` (T525'in yolu) |
| 4 | ✓ | 422: alan kuralı (geçici sınama: 400 karakterlik ad, bozuk harita bağlantısı, 121 karakterlik duyuru — hiçbiri yazılmaz), adsız taslak, yayındaki kaydın kapısı, galeri tavanı, silinmiş görsel (`ASave_WithAnEmptyRequiredField_…`, `EditingAPublishedRecord_…`, `ASave_WithMoreImagesThanTheCeiling_OrADeletedImage_…`; şube görseli geçici sınamayla); 409: sürüm (`ASave_WithAStaleVersion_…`), ikinci duyuru — sayfada 409, gönderilen metin korunur, form var olan kaydın adresine bağlanır, tabloda tek satır (geçici sınama) |
| 5 | ✓ | `OnPostSearchProductsAsync` yalnız okur (işlem açmaz); `TheProductSearch_WritesNothing_AndTheTickedResult_JoinsTheLinkedProducts_…` |
| 6 | ✓ | Mutlak hedef, eksik kapı 422, iz ve sürüm yalnız değişimde (`TheStatusMove_IsAbsolute_…`, `APublishMove_…`); geçici sınama: yayın 303, ikinci yayın 303 ve sürüm/iz değişmez, `slug_fixed_at` dolu, ad değişse adres aynı, açılış sürümüyle form kaydı 409 |
| 7 | ✓ | Onaysız 200 ve yazmaz, onaylı 303 → liste, tekrar 404 (sayfa testi + geçici sınama); Hakkımızda sayfada 422, kayıt durur (geçici sınama); D-673: silme işlemin sahibinde kaydedilir |
| 8 | ✓ | Yer değiştirme, eski sıra 409 ve yazmaz (`AMove_…` iki test); grup başında yukarı 422 (geçici sınama), duyuru 422 (modül testi) |
| 9 | ✓ | Boş markada `NameFallbackPrefix` ve `LogoFallbackText` sayfada (geçici sınama); kayıt 303 ve yeni ad (`TheBrandScreen_Opens_…`), renk büyük harfle saklanır (geçici sınama) |
| 10 | ✓ | Boşaltılan ad 422 (`AFilledBrandName_…`), bozuk renk 422, sürüm 409 (`TheBrandScreen_…`), silinmiş logo 422 (geçici sınama), iz satırı yazılır (geçici sınama + `BrandingTests`); siparişe donma yok — yazıcı yalnız `brandings` satırını yazar (`ASave_ChangesNoOtherRow_…`) |

**`12` satırları:** PASS — VAL-A146, C032 (Taslak kaydın vitrin adresi ziyaretçiye 404, geçici sınama), C033, C034 (kısa tanıtımsız Hakkımızda yayına alınmaz 422, yazılınca 303), A151, C035 (silme 422, kayıt durur; taslağa alma 303 ve `/hakkimizda` ziyaretçiye 404), A161 (303, tek satır, 120 tavanı). BEKLEMEDE kalır (vitrin, tarayıcı ya da e-posta payı bu doğrulamada kanıtlanmadı): A147, A148, A149, A150, A157, A158, A159 (D-442), A160, C036, A162; KP-69 tarayıcı satırları B050, B085 tarayıcı turunda.

**Kontrol listesi:** KK1–KK10 ✓ (yukarıdaki kanıtlar). `09 §25`: modül `SaveChanges` çağırmaz (yeni kaydın satırı aynı işlemde yazılır, işlemi sayfa açar); sürüm gizli alanda `VersionFields`; satır içi stil, `Html.Raw` yok; panel klasörü `NoStore` kuralı; yeni paket ve göç yok. Kilit: `FOR KEY SHARE` yalnız `ProductLocks.cs`'te, kimlik sırasıyla; ürün silmesi tek `DELETE` (bağlar veritabanında basamaklanır) — içerik kaydıyla döngü kuran bekleme bulunmadı; metin eksikleri D-1546.

**Testler (yerel, bu uçta):** restore ✓; build `-warnaserror` 0/0; `dotnet format --include` (34 değişen `.cs`) çıkış 0; Unit 7643/7643 (ilk koşu 1 hata, iki tekrar temiz — D-1547); betik testleri 4/4; Integration hedefli 400/400 — `PanelContentEditingTests`, `ContentPanelPageTests`, `PanelHomeTests`, `SalesSummaryTests`, `ContentStatusTests`, `ContentRelationsTests`, `ContentAuthoringTests`, `AnnouncementAndPreviewTests`, `StorefrontReadersTests`, `BrandingTests`, `BranchContentLinkAndContactRequestTablesTests`, `ProductPublishGateTests`, `ContentPagesTests`, `ForgeryProtectionTests`, `PageFormPatternTests`, `SecurityHeadersTests`, `SessionAccessTests`, `WritingEndpointAudit*` — TAM: CI. Geçici sınamalar 5/5 (koşudan sonra silindi). PR #574 CI run 43577902722: Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate ✓.

**Güvenlik:** sır yok; bütün uçlar panel oturumu ister, POST'lar sahtecilik korumalı; `{tip}` kapalı liste, yabancı anahtar 404; görsel yalnız T525 yükleme yolundan ve kimliği satırla doğrulanarak bağlanır; GET'ler yazmaz (`ListAsync`, `GetFormAsync`, `ReadSectionAsync` izlemesiz); `SearchProducts` yazmaz; yeni bağımlılık yok.

**K-notları:** K1 → D-1546 (`FOR KEY SHARE` istisnasının `09 §10.2` m.4–m.6 ve tarama iletisinde yazılı olmaması); K2 → D-1547 (geçici sınamayla doğrulanan davranışların kalıcı testi yok; Unit'te bir kez görülen kararsız test).

**Yapım raporu karşılaştırması:** sonuçlar uyumlu. Öz denetim kanıtı iki maddede yalnız koda dayanıyor: KK2'nin 303'ü ("kod `OnGetAsync`") ve KK10'un silinmiş görseli ("kodda") — doğrulayıcı ikisini geçici sınamayla kanıtladı, kalıcı test D-1547. KK6'nın "açık formun 409'u" öz denetimde dolaylı (sürüm testi) — doğrulayıcı doğrudan sınadı. ⚠ kararlar: D-1537 (`07 §5.2.2` alan adı eşitlemesi, yapısal değişiklik değil, karar satırı backlog'da) ve `09 §10.2` m.5 eki D-720'nin gereği olarak kabul edildi; D-1535, D-1536, D-1538, D-1539 bloke etmeyen, F5 kapısında; D-442'nin ileri atılması gerekçeli (logo mutlak adresi T534 kriterlerinde yok, VAL-A159 BEKLEMEDE).
