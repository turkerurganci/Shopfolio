# T358 — Kurumsal içerik tipleri: Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube, genel sayfa, sosyal bağlantılar

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-08

> Taslağı yapım işçisi doldurur; doğrulama finalize eder.

---

## Yapılan İşler

- Kurumsal içerik **giriş birimi** `ContentAuthoring`: Hakkımızda (tek kayıt; ikincisi `AlreadyExists`), hizmet tanıtımı, referans iş, SSS, şube ve genel sayfa için oluştur/kaydet. Yeni kayıt taslak doğar; adres sunucuda `Slug.From` ile üretilir (çakışmada `-2`, `-3`). `SaveChanges` çağırılmaz; düzenleme `ContentPublishing.SaveEditAsync` üzerinden gider (sürüm denetimi, yayındaki kaydın kapısını bozma kuralı oradadır).
- `ContentInputRules`: alan kuralları `Infrastructure/Validation` niteliklerinden (`InvalidUrl`, `InvalidPhone`, `TextTooLong`); düz metinler (kısa tanıtım, soru) olduğu gibi saklanır, uzun metinler kayıtta kapalı set temizleyicisinden geçer.
- `SocialLinks`: altı platform (`SocialPlatform`), boş alan satırı siler, bilinmeyen ad ya da hatalı bağlantı bütün kaydı reddeder; WhatsApp kuralı `CheckWhatsAppNumber` (yazım T361 `SaveHomepage`'de).
- `ContentTypes` / `ContentPresentation`: kapalı tip listesi (`ContentRecordKind` ile iki yönlü eşit), "Bize ulaşın" (`/iletisim`), "Haritada aç", iletişim sayfası parçaları.
- `ContentReaders`: SSS tek düz liste (yalnız yayındakiler, elle sıra), iletişim sayfası bileşkesi (kimlik + yayındaki şubeler + form), `MapLinkOf`.
- `Settings/Contracts/IProvinceList` + `ProvinceList` (il kapalı listesi: `ExistsAsync`, `GetNamesAsync`); `SettingsModule` kaydı.
- **D-360 kapandı:** beş varlığın uzun metin ayarlayıcıları `private` + `SetLongText`/`SetText`/`SetAnswer` (`ContentRules.CleanLongText`).
- Telefonun saklama biçimi tek yere alındı: `RequestFormat.StoredPhone`; şirket kimliği ve şube aynı işlevi çağırır (iki kopya kaldırıldı).
- Derleme düzeltmesi: `FieldRejection` ad çakışması (`Settings.Contracts` ve `Infrastructure.Validation`) için `ContentAuthoring` ve `ContentReaders`'ta takma ad.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/` (`ContentAuthoring`, `ContentInputs`, `SocialLinks`, `ContentTypes`, `ContentReaders`, beş varlıkta ayarlayıcılar, `CorporateContentModule` kaydı)
- `src/Shopfolio.Web/Modules/Settings/` (`ProvinceList`, `Contracts/IProvinceList`, `SettingsModule`, `CompanyIdentityRules.Phone`)
- `src/Shopfolio.Web/Infrastructure/RequestFormat.cs` (`StoredPhone`), `Infrastructure/Routing/AddressRules.cs` (`ContactPage` sabiti)
- Testler: `UnitTests/CorporateContent/CorporateContentTypesTests.cs`, `IntegrationTests/CorporateContent/ContentAuthoringTests.cs`; mevcut testlerde `SetXxx` uyarlaması
- Belgeler: `IMPLEMENTATION_STATUS.md`, `STATUS_CHANGELOG.md`, `DEFERRED_BACKLOG.md` (D-360), `.claude/memory/MEMORY.md`, `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Hazır tipler ve sabit düzenli genel sayfa; sayfa kurucu yok | ✓ | `ContentAuthoring.cs` tip başına altı oluştur/kaydet çifti; `CorporateContentTypesTests.NoTypeOrMemberOfTheModule_IsAPageBuilder_KK1`, `TheEntryForms_HaveNoBlockSectionOrLayoutField_KK1` |
| 2 | Beş hazır tip + genel sayfa + duyuru; galeri tipi yok | ✓ | `ContentTypes.cs:14,18`; `TheTypeList_IsTheContentRecordKindEnum_InBothDirections_KK2`, `TheRecordKinds_HaveNoGalleryOrContactPageKind_KK2` |
| 3 | Hakkımızda tek kayıt; kısa tanıtım düz metin ve tavanlı; ikincisi açılmaz, silinmez | ✓ | `ContentAuthoring.cs:86-89` (`AlreadyExists`); `ContentAuthoringTests.TheAboutPage_FirstSaveCreatesTheOneRow_AndASecondIsRefused_KK3`; tavan `ContentInputs.cs` (`ShortIntroductionMaxLength`) + `TheAboutPage_IsOneRecord_AndTheShortIntroductionHasTheProductCeiling_KK3`; silme reddi T322 (`ContentStatusTests` about page silme) |
| 4 | Hizmet fiyatsız, satın alınmaz; "Bize ulaşın" forma götürür | ✓ | `ContentTypes.cs` (`OfferingContactButtonText`, `OfferingContactButtonPath`); `TheServiceIntroduction_HasNoPriceCartOrStockMember_KK4`, `TheServiceButton_SaysBizeUlasinAndLeadsToTheContactPage_KK4` |
| 5 | Referans iş başlık, kısa açıklama, metin, görsel; müşteri/yıl/kategori yok | ✓ | `PortfolioItemInput` dört alan; `TheReferenceWork_HasExactlyTheFourFormFields_AndNoCustomerYearOrCategory_KK5` |
| 6 | SSS soru-cevap; soru düz metin ve tavanlı; tek liste | ✓ | `ContentInputs.cs` (`FaqItemInput` kuralı `ShortDescriptionMaxLength`); `ContentReaders.ListFaqAsync`; `TheQuestionCeiling_IsTheShortTextConstant_AndTheQuestionIsKeptLiterally_KK6`, `TheFaq_IsOneFlatListOfPublishedQuestions_InTheFirmsOrder_KK6` |
| 7 | Şube: ad ve adres zorunlu (kapıda), il kapalı listeden, gömülü harita yok, "Haritada aç" | ✓ | `ContentAuthoring.cs:230-239` (`CheckBranchAsync`, `InvalidProvince`); adres kapıda `ContentGate` (T322, `ContentGateTests`); `ContentReaders.MapLinkOf`; `ABranch_WithAProvinceNotOnTheList_IsRefused_...KK7`, `TheMapLink_IsKeptOnlyWhenItIsAnHttpOrHttpsAddress_KK7`, `TheBranch_HasNoEmbeddedMapOrCoordinateMember_KK7` |
| 8 | İletişim sayfası ayrı tip değil; kimlik + şubeler + form | ✓ | `ContentReaders.ReadContactPageAsync`; `TheContactPage_IsBuiltFromThreeParts_AndHasNoRecordKindOfItsOwn_KK8`, `TheContactPage_ListsOnlyPublishedBranches_..._KK8` |
| 9 | Genel sayfa adet sınırsız; yasal metin genel sayfa olarak girilmez | ✓ | `TheGeneralPages_HaveNoCountLimit_AndEachGetsItsOwnAddress_KK9` (12 kayıt); yasal metin ayrı birim (T366), `ContentRecordKind`'de yasal tip yok: `TheGeneralPage_HasNoCountLimit_AndNoLegalTextType_KK9` |
| 10 | Altı platform + isteğe bağlı WhatsApp; siparişe donmaz | ✓ | `SocialLinks.cs` (`Platforms`, `TryParsePlatform`, `CheckWhatsAppNumber`); `ASocialSaveWithAnUnknownPlatformOrABadLink_IsRefusedWhole_...KK10`, `NoOrderTable_HasASocialLinkOrWhatsAppColumn_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 sayfa kurucu ucu yok | ✓ | `NoTypeOrMemberOfTheModule_IsAPageBuilder_KK1` (giriş tarafı türleri ve her üye adı; `HomepageLayout` T361'in kapalı iki değeri, kapsam dışı bırakıldı ve testte gerekçesi yazılı) |
| KK2 tip listesi kapalı, galeri tipi yok | ✓ | `ContentTypes.cs:14-21`; `ANameOutsideTheClosedList_IsNotAType_KK2` (Gallery, Contact, "0", boş, null) |
| KK3 ikinci Hakkımızda ve silme reddedilir; kısa tanıtım biçimsiz saklanır | ✓ | `ContentAuthoring.cs:86-89`; `TheAboutPage_ShortIntroduction_IsStoredAsTyped_AndTheLongTextIsCleaned_KK3` (`<b>x</b> & y` harfiyen; uzun metinde script yok); silme `ContentStatusTests` |
| KK4 hizmette sepet yok, form bağlantısı var | ✓ | `TheServiceIntroduction_HasNoPriceCartOrStockMember_KK4`, `TheServiceButton_...KK4` (`/iletisim` = `AddressRules.ContactPage`) |
| KK5 referans işte ek alan yok | ✓ | alan kümesi birebir: `["IsShownOnHomepage","ShortDescription","Text","Title"]` |
| KK6 SSS tek liste; soru biçimsiz | ✓ | `ListFaqAsync` dönüşü düz liste, parametresiz; `TheQuestion_HasNoHeadingOrCategoryField_AndTheListIsOne_KK6`; `AQuestion_IsStoredAsTyped_...KK6` |
| KK7 şubede zorunlu alan ve harita bağlantısı doğru | ✓ | ad zorunlu, il listede; harita http/https dışı → reddedilir ve okumada `null` (8 örnek: javascript:, ftp:, iframe metni, boşluklu); "Haritada aç" sabiti |
| KK8 iletişim sayfasının kendi kaydı yok | ✓ | `ContentRecordKind` adlarında "Contact" yok; `ContactPageContent` üç parça |
| KK9 genel sayfada sınır yok | ✓ | 12 oluşturma, 12 ayrı adres; sınır sabiti taraması |
| KK10 platform listesi kapalı, donma yok | ✓ | `APlatformName_IsOnTheListOnlyWhenItIsExactlyAValue_KK10` ("1", "0", "instagram", " X"); sipariş tablolarında Social/WhatsApp sütunu yok; `social_links` yabancı anahtarsız; platform başına tekil dizin veritabanında sınandı |
| 09 §25 Doküman uyumu: yeni sabit/tavan §3.5'te | ✓ | yeni sayı yok: tavanlar `ContentRules` → `ProductConstants` (P-27/P-28/P-29); WhatsApp on hane mevcut `InvalidPhone` kuralı |
| 09 §25 Modül sınırı, arayüzde `SaveChanges` yok | ✓ | `grep SaveChanges` `ContentAuthoring`, `SocialLinks`, `ContentReaders`, `ProvinceList`: 0; mimari testler yeşil (Unit 4286); `CorporateContent` yalnız `Settings.Contracts.IProvinceList`'e başvurur |
| 09 §25 Aynı işi yapan tek işlev | ✓ | telefon saklama biçimi `RequestFormat.StoredPhone`'a taşındı, iki kopya (`CompanyIdentityRules.Phone`, `ContentInputRules.Phone`) buna devreder; bağlantı/telefon/tavan denetimi `Infrastructure/Validation` nitelikleri (tek kural evi); temizleme `RichTextSanitizer` |
| 09 §25 Zaman yalnız `TimeProvider` | ✓ | `ContentAuthoring`, `SocialLinks` kurucudan `TimeProvider`; `grep DateTime.UtcNow\|DateTimeOffset.Now` 0 |
| 09 §25 Sayfa adresi `Slug.From`, çakışma | ✓ | `ContentAuthoring.NewSlugAsync` → `Slug.From` + `Slug.FirstFree`; `TheAddressOfANewRecord_...NumberedOneOnAClash`; eşzamanlı çakışma: `ThreeRecordsOfTheSameName...WhenTheCallerRetriesOnTheSlugIndex` (yeniden deneme işlem sahibinin, ⚠ 1) |
| 09 §25 DB: yabancı anahtar, varsayılan, dizin | ✓ (uygulanamaz) | şema/göç değişikliği yok; `git diff --stat` göç dosyası içermez; `branches.province_id` FK ve dizin T104'ten, FK davranışı `BranchContentLinkAndContactRequestTablesTests` |
| 09 §25 Kullanıcı HTML'i temizleyiciden geçer | ✓ | beş ayarlayıcı `ContentRules.CleanLongText`; `grep "Answer =\|LongText =\|Text ="` modülde atama yalnız `SetXxx` içinde; `RichTextSourceScanTests` yeşil; entegrasyon: `<script>` kayıtta düşer |
| 09 §25 Güvenlik: sır/kişisel veri | ✓ | yeni sır, kayıt satırı ya da kişisel veri yok; şube telefonu firma verisi |
| 09 §25 Güvenlik: yetkilendirme, POST | ✓ (uygulanamaz) | uç, sayfa ya da POST eklenmedi; modül metotları panel tarafından (T534) kapıdan çağrılır |
| 09 §25 Test: yeni kural testi, mimari ihlal örneği | ✓ | yeni mimari kural/tarama eklenmedi (mevcut taramalar yeşil); `ModuleBoundaryTests`, `RichTextSourceScanTests`, `SalesModelShape` koşuda |
| 09 §25 Eşitlik denetimleri | ✓ | `ContentTypes` ↔ `ContentRecordKind` iki yönlü test; Consistency testleri yeşil (D-360 satırı kapandı, aktif kalem 152) |
| Güvenlik taraması: bağlantı şeması | ✓ | `javascript:` ve `ftp:` hem yazımda hem okumada düşer; şube `MapUrl`/sosyal bağlantı yalnız `InvalidUrl` |
| Güvenlik taraması: gömülü harita | ✓ | `iframe`/gömülü harita girdisi bağlantı kuralında reddedilir; `BranchView` yalnız `MapLinkView(Url, Text)` |
| Güvenlik taraması: SQL/metin | ✓ | tüm sorgular LINQ; slug `[a-z0-9-]` (LIKE meta yok) |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | yeni istisna yok; KK1 taraması kapsamı dar ve gerekçeli |
| Sık FAIL (2) tarama çağrı biçimi | ✓ | KK1 taraması tür adı + her üye adı + form alanı; KK10 EF modelinden sütun taraması |
| Sık FAIL (3) DB varsayılanı / FK dizini | ✓ | göç yok |
| Sık FAIL (4) kural değişince öteki yerler | ✓ | telefon biçimi tek işlev; `CONTEXT.md` satırı, MEMORY, backlog sayacı güncel |
| Sık FAIL (5) modül metodu `SaveChanges` / tek kural evi | ✓ | yukarıda |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 4286 / 4286 (Consistency ve Architecture dahil) | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release --no-build` | yeni `CorporateContentTypesTests` 58 örnek |
| Integration | ✓ CorporateContent 64 / 64 | `... IntegrationTests ... --filter-namespace "*CorporateContent*"` | yeni `ContentAuthoringTests` 19; `Model` ad alanı 664 geçti (süre sınırında kesildi, 0 hata) |
| Biçim / derleme | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes`; `dotnet build -c Release --no-restore -warnaserror` | temiz |

Not: `StoredPhone` taşıması son testlerden sonra yapıldı (davranış aynı, derleme temiz); dal CI'ı bütün takımı koşar.

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

- **Branch:** `task/T358-kurumsal-icerik`
- **Commit:** (PR açılışında head SHA: yardımcıya dönüş mesajında)
- **PR:** #449
- **CI:** yardımcı yürütür (işçi CI yoklamaz)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. (Plan tier, kota, paket sürümü, dış servis yok; yeni paket yok; PostgreSQL 18.6 yerel konteyner CI ile aynı sürüm.)

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Eşzamanlı adres çakışmasında yeniden deneme `ITransactionRunner.RunAsync(work, ["ix_offerings_slug", "ix_portfolio_items_slug", "ix_custom_pages_slug"], ct)` çağıranındır (T534 paneli); giriş yöntemi her denemede adresleri yeniden okur (testli) | — (planlı: T534) | Giriş birimi işlemi açmaz (`09 §9.1` m.3); çağıran henüz yok |
| 2 | WhatsApp numarasının yazımı `SaveHomepage`'de (kural `SocialLinks.CheckWhatsAppNumber` burada) | — (planlı: T361 k2) | Numara ana sayfa bölümünün ayarı (`06 §4.2`) |
| 3 | İlk yayında adres sabitleme, ad değişince yeniden üretim | — (planlı: T363) | Plan T363'ün |
| 4 | Panel ekranları ve `ICorporateContentModule` sözleşme üyeleri | — (planlı: T534) | `ContentAuthoring` T322 gibi `internal` |
| 5 | Görsel ekleme / alternatif metin içerik kayıtlarında | — (planlı: T359) | T359 alanı |

Yeni backlog kaydı açılmadı (D-414…D-418 kullanılmadı): beş kalem de planda sahibi olan task'larda.

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** yönetici yardımcı düzeninde CI yoklaması işçiye yasak; yardımcı yürütür.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ Kararlar (kurala en uygun seçenek):** (a) `IProvinceList` `Settings.Contracts`'a konuldu (modül sınırı: CorporateContent Settings'in iç tablosuna dokunamaz); (b) telefon saklama biçimi iki kopya yerine `RequestFormat.StoredPhone` (tek işlev, `09 §25`); (c) KK1 taraması `HomepageLayout`/`HomepageBlocks` adlarını kapsam dışı bıraktı (T361'in kapalı iki değeri ve önbellek alanı); (d) şubenin adres alanları taslakta zorunlu değil, yayın kapısı ister (`02 §3.27.24` + T322 kapısı).
- Önceki işçi bölündü; devir notu okundu, kod derlenmeden gelmişti: tek derleme hatası `FieldRejection` ad çakışması, test derlemesinde `FaqItem.Answer` ataması.
