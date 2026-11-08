# T360 — Duyuru şeridi, boş kurulum, içerik yayın durumu, önizleme ve silme

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Duyuru yazımı** (`ContentAuthoring`): `CreateAnnouncementAsync` (tek satır; ikincisi `AlreadyExists`, tek satır dizini yarışa karşı güvence; silindikten sonra yenisi açılır) ve `SaveAnnouncementAsync` (sürüm denetimli). Kurallar `ContentInputRules.Check(AnnouncementInput)`: metin P-30 tavanlı (saklanan kırpılmış biçimin karakter sayısı), bağlantı `InvalidUrlAttribute` ile, bitiş günü başlangıçtan önce olamaz; boş duyuru taslak olarak kaydedilir.
- **Duyuru okuyucusu** (`AnnouncementReader`, `ICorporateContentModule.GetAnnouncementAsync`): görünürlük `ContentVisibility.Decide` ile (tek kural evi), bugün `TurkeyTime.DateOf(TimeProvider)`; önbellek yok, yazma yok; metin işaretlemesiz döner (sayfa kaçışlı basar). Yönetici taslağı bantlı (`IsDraft`) görür, yayındaki ama aralık dışı duyuruyu kimse görmez.
- **Taslak önizleme** (`ContentPageReader`, `ContentReaders`): taslak sayfa ziyaretçiye null (sayfa bulunamadı) döner, yöneticiye `IsDraft`; SSS sorusu ve şube yerinde işaretli listelenir (`VisibleStatuses`, `FaqView.IsDraft`, `BranchView.IsDraft`).
- **Yayın durumu izi** (`ContentPublishing`): yayına alma ve yayından kaldırma aynı işlemde `ChangePublishStatus` satırı yazar (`IAuditTrail`; kim, eski ve yeni durum); metin düzenlemesi, tekrarlı ya da reddedilen yayın ve silme iz yazmaz; iz yazılamazsa işlem geri alınır.
- **Boş kurulum:** `EmptyInstallationTests` şablon veritabanında içerik, katalog, görsel ve duyuru tablolarının boş olduğunu ve modülün her kitleye boş döndüğünü sınar.
- Silme (kalıcı silme, menü yeri, ana sayfa işareti, ürün bağı) T322'de yazılmıştı; burada testle kilitlendi (KK7).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/`: `AnnouncementReader.cs`, `ContentPageReader.cs` (yeni); `ContentAuthoring.cs`, `ContentInputs.cs`, `ContentPublishing.cs`, `ContentReaders.cs`, `ContentVisibility.cs`, `CorporateContentModule.cs`, `Contracts/AnnouncementView.cs` (yeni), `Contracts/ICorporateContentModule.cs`
- `tests/Shopfolio.UnitTests/CorporateContent/AnnouncementAndPublishRulesTests.cs` (yeni)
- `tests/Shopfolio.IntegrationTests/CorporateContent/`: `AnnouncementAndPreviewTests.cs`, `EmptyInstallationTests.cs` (yeni); `ContentAuthoringTests.cs`, `ContentStatusTests.cs` (kurucu ve iz düzeneği)
- `tests/Shopfolio.IntegrationTests/Infrastructure/SitemapSourceTests.cs` (ayar satırı kurulumu)
- Belgeler: bu rapor, `IMPLEMENTATION_STATUS.md` satırı, `STATUS_CHANGELOG.d/T360.md`, `DEFERRED_BACKLOG.d/T360.md`, D-571/D-315/D-317 satırları

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek duyuru, P-30 tavanlı düz metin + isteğe bağlı bağlantı, aralıkta görünür | ✓ | `AnnouncementAndPublishRulesTests.TheAnnouncementCeiling_IsTheProductConstantP30…_KK1`; `AnnouncementAndPreviewTests.TheStrip_ShowsOnlyInsideTheDateRange_BothEndDaysInclusive_OnTheTurkeyDay_KK1`, `TheAnnouncementIsOne_…_KK1` |
| 2 | Site boş kurulur | ✓ | `EmptyInstallationTests.ANewInstallation_CarriesNoContentCatalogImageOrAnnouncementRow_KK2` |
| 3 | Yayından kaldırılan taslağa döner; ikinci onay yok | ✓ | `AnyOneAdministrator_Publishes_AndTheWayBackIsToDraft_WithoutAnArchive_KK3`; `ThePublishOperations_AreTwoAndNeitherAsksForASecondApproval_KK3` |
| 4 | Taslak kaydı, boş Hakkımızda/duyuru taslağı, yayın zorunlu alan ister | ✓ | `AnEmptyAnnouncementAndAnEmptyAboutPage_AreSavedAsDrafts_…_KK4`; `ATitledReferenceDraft_AndASlugOnlyTitleDraft_…_KK4` |
| 5 | Taslak ziyaretçiye 404, yöneticiye bantlı; SSS, şube, duyuru yerinde | ✓ | `ADraftServiceReferenceAndGeneralPage_…_KK5`; `ADraftAboutPage_…_KK5`; `ADraftQuestionAndADraftBranch_…_KK5`; `AnAnnouncement_IsBornADraft_…_KK1_KK5` |
| 6 | Sürüm geçmişi yok; durum geçişi izde, metin düzenlemesi izde değil | ✓ | `APublicationAndTheWayBack_WriteOneTrailRowEach_…_KK6`; `EveryKindOfRecord_WritesItsStatusChangeIntoTheTrail_…_KK6`; `ATextEdit_…_WritesNoTrailRow_…_KK6` |
| 7 | Kalıcı silme menü yeri, ana sayfa işareti, ürün bağı | ✓ | `DeletingTheLastPublishedServiceIntroduction_…_KK7`; `DeletingAReferenceWork_…_KK7`; `DeletingAGeneralPage_…_KK7`; `AWarningThatIsNotConfirmed_DeletesNothing_…_KK7` |
| 8 | İleri tarihli yayın yok; yalnız duyuru aralığı | ✓ | `NoRecordButTheAnnouncement_HasADateThatPublishesItLater_KK8`; `ThePublishOperations_TakeNoMoment_ThePublicationIsAlwaysNow_KK8` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil, bütün maddeler diff'te yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–8 | ✓ | Yukarıdaki tablo; her test bu dalda yeşil |
| KK1: aralık dışı görünmez; P-30 aşan kaydedilmez; metin biçimlendirmesiz | ✓ | `TheStrip_ShowsOnlyInside…_KK1`; `ATextOverP30_OrALinkOfTheWrongForm_IsNotSaved_…_KK1`; `ATextOfExactlyP30Characters_IsSaved_…_KK1`; `TheText_IsKeptAsTyped_…_KK1` |
| KK2: temiz kurulumda örnek kayıt yok | ✓ | `EmptyInstallationTests` (şablon DB, TRUNCATE yok) |
| KK3: kaldırılan Taslak; onaysız yayın | ✓ | `ContentStatusTests`, `AnyOneAdministrator_Publishes_…_KK3` |
| KK4: eksik alanlı yayın reddedilir; başlıklı referans ve boş Hakkımızda taslağı kaydedilir | ✓ | `…_KK4` testleri |
| KK5: bant yalnız yöneticide; SSS, şube, duyuru yerinde işaretli | ✓ | `…_KK5` testleri (`FaqView.IsDraft`, `BranchView.IsDraft`, `AnnouncementView.IsDraft`) |
| KK6: eski sürüme dönüş yok, izde kim yazılı; metin iz doğurmaz | ✓ | `…_KK6` testleri; geri alma yöntemi yok (iki durum, arşiv yok: `ContentPublishing`) |
| KK7: silinen kaydın menü yeri, işareti, ürün bağı | ✓ | `…_KK7` testleri |
| KK8: ileri tarih yalnız duyuruda | ✓ | `…_KK8` testleri |
| `09 §25` / bekçi-tarama istisnası geniş (sık FAIL 1) | ✓ | Yeni tarama ya da istisna eklenmedi; mimari ve tarama testleri tam koşuda yeşil |
| Kaynak taraması çağrı biçimi (sık FAIL 2) | ✓ | Yeni tarama kuralı yok; duyuru tavanı `ContentRules.AnnouncementTextMaxLength` sabitinden, testte sabit okunur |
| 06 "Varsayılan" / FK dizini (sık FAIL 3) | ✓ | Göç ve şema değişikliği yok; tek satır dizini mevcut göçte (`CreateContentRecordAndBrandingTables`), yarış testi `TwoFirstSavesOfTheAnnouncementAtTheSameTime_…` |
| Aynı sayıyı anan öteki yerler (sık FAIL 4) | ✓ | Sayı/etiket değişikliği yok; D-315/D-317/D-571 satırları güncellendi; `OperationTableConsistencyTests` dahil Consistency 283 yeşil |
| Belge kuralını değiştiren yeni 09 metni (sık FAIL 5) | ✓ | 09 değişmedi; davranış `02 §3.27.21–28` ile birebir |
| Modül kurucusuna yeni bağımlılık, elle kuran düzenekler (sık FAIL) | ✓ | `ContentPublishing` (IAuditTrail) ve `CorporateContentModule` (AnnouncementReader) kurucu değişti; elle kuran `ContentAuthoringTests`, `ContentStatusTests` güncellendi; Integration TAM koşuldu |
| Önbellek geçersiz kılma (sık FAIL) | ✓ | Duyuru okuyucusu önbelleksiz (D-678); durum geçişi `DropStaleAreas` ile alanları düşürmeye devam ediyor |
| `dotnet format` (sık FAIL) | ✓ | src, UnitTests, IntegrationTests `--verify-no-changes` temiz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 5055 / 5055 |
| Unit (Consistency) | ✓ | `… --filter-trait "Consistency=true"` | 283 / 283 |
| Integration | ✓ | `--filter-namespace` parçalarıyla (Postgres 18, docker `t360`) | CorporateContent 111; Model 1379 (1 yük zaman aşımı, tek başına `AdminAndAuditTablesTests` 42/42); Ordering+Catalog+Payment+Outages 432; Web 594; öteki 414 |
| Biçim | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (3 proje) | değişiklik yok |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata (main birleştirmesi sonrası) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T360-duyuru-icerik-yayin`
- **Commit:** dalın başı (PR açıklamasında)
- **PR:** PR açıklamasında
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Yok (dış servis, kütüphane ya da ortam varsayımı yapılmadı) | Şema, paket ve yapılandırma değişikliği yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Taslak" bandı biçimi ve panel/sayfa katmanı | D-676 | Sayfa ve panel task'ları (`04`) |
| 2 | Sayfa okuyucusu ve yönetici önizleme okumaları modül arayüzünde değil | D-677 | Çağıranı olmayan yöntem eklenmedi |
| 3 | Duyuru okuyucusu önbelleksiz | D-678 | Ölçüm gerekmedikçe gerek yok |
| 4 | Silme iz yazmaz (belgeyle uyumlu) | D-679 | Belge değişirse |
| 5 | İlk yayında adres sabitleme; `PublishStatus` ayarlayıcısı public | D-315 (kalan), D-317 | T363 |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: 42524245424 Publish success; 42524245383 CI devam ediyor (T349 #476); 42522612870 Publish success.
- **Kapanan kayıtlar:** D-571 ✅ (duyuru okuyucusu); D-315 iz kısmı kapandı, kalan T363'e; D-317 T363'e ileri atıldı.
- **⚠ kararlar (kolay, kurala en uygun seçenekle):** duyuru okuyucusu önbelleksiz; silme iz yazmaz; "Taslak" bandı UI katmanı (burada yalnız `IsDraft`); sayfa okuyucusu modül arayüzüne eklenmedi (çağıranı yok).
- **Test ortamı notu:** Model testlerinde tek bir yük zaman aşımı (`Migration_WithoutTheApplicationUserRoleSetting_…`) görüldü, tek başına ve parçalı koşuda yeşil.
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum" bu task'ta değiştirilmez (parça kuralı).
