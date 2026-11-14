# T534 — Kurumsal içerik ve marka yönetimi (E-41, E-43)

**Faz:** F5 | **Durum:** ✓ Yapım tamamlandı (doğrulama bekliyor) | **Tarih:** 2026-11-14

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
