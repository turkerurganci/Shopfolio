# T538 — Uç ve model eşitlik denetimleri: uç envanteri, form girdi ve sorgu modelleri, makine ucu gövdeleri

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-15

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `ApiInventoryDocument`: `07 §5.2`, `§5.3.2`, `§5.4.2` model tablolarını ve `§3.2` uç envanterini okur (belge metnini parametre alır; yanlış metinle sınanabilir).
- `ModelInventoryCheck`: tablo ile koddaki model sınıflarını ad ve alan düzeyinde iki yönlü karşılaştırır (form: ayarlanabilir alanlar ve işleyicinin doldurduğu listeler, iç içe liste alanları dahil; sorgu: camelCase parametre adları; makine gövdesi: JSON adları).
- `EndpointBindingCheck`: envanterdeki uçları sayfaların `@page` şablonlarına ve işleyicilerine çözer; işleyicinin bağladığı modeli "Uçlar" sütunuyla, `§5.2.4` modelsiz listesiyle ve `06` varlıklarıyla karşılaştırır.
- `ModelInventoryConsistencyTests`: eşitlik testleri (form, sorgu, makine gövdesi, işleyici bağlama) + her denetim için kırıldığını gösteren ihlal örnekleri + dosya/ağ/veritabanına dokunmama denetimi. Bilinen fark listesi yoktur.
- Kodda hizalamalar: `SignOutInput` (UC-77), `ContentListQuery` → `ContentQuery` (UC-07, UC-08), `CouponListQuery` → `CouponQuery` (UC-90).
- **Düzeltme 1 (2026-11-15, yönetici kararı, K-1616):** ilk teslimdeki açık farklar ertelenmişti (`KnownModelDifferences`); bu task'ın amacı eşitlik olduğundan hepsi kapatıldı. (a) `07 §5.2` koda hizalandı: geri ödeme işlemleri iki ortak model (`ProcessRefundInput`, `RetryCardRefundInput`), ödeme formunun dört kutusu, görsel dosyasının model alanı olmaması (`ImageTarget` tek alan), `Version`, `Original`, `AddProducts`, `Remove`, `RemoveLogo`, `RemoveFavicon`, `AddressType`, `ArrivalRecordId`; UC-131, UC-132, UC-146, UC-149, UC-162, UC-117 metinleri aynı adlara çekildi. (b) Sorgu modelleri kodda kuruldu: `RegisterQuery`, `SignInQuery`, `ReauthenticateQuery`, `AccountQuery`, `AdminAccountQuery`, `AdminSignInQuery` ve Google dönüşü için `GoogleReturnQuery`; sekiz sayfa bunları bağlar (sorgu dizesi adları ve davranış aynı). `CategoryQuery` (UC-02) `07 §5.3.2`'ye satır oldu, `ProductQuery` yalnız UC-03. (c) `KnownModelDifferences` ve iki testi kaldırıldı; denetim farkı artık kırar. (d) `07` v0.43, K-1616.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/ApiInventoryDocument.cs`, `ModelInventoryCheck.cs`, `EndpointBindingCheck.cs`, `ModelInventoryConsistencyTests.cs` (`KnownModelDifferences.cs` kaldırıldı)
- `src/Shopfolio.Web/Pages/Storefront/SignOut.cshtml.cs`, `Pages/Content/ContentListPages.cs`, `Pages/Panel/Coupons.cshtml.cs`
- Düzeltme 1: `Pages/Storefront/AccessQueries.cs` (yeni: `RegisterQuery`, `SignInQuery`, `AccountQuery`, `GoogleReturnQuery`), `Register.cshtml.cs`, `SignIn.cshtml.cs`, `GoogleReturn.cshtml.cs`, `Pages/Account/Reauthentication.cshtml.cs`, `Pages/Account/Profile.cshtml.cs`, `Pages/Panel/Reauth.cshtml.cs` (`ReauthenticateQuery`), `Pages/Panel/MyAccount.cshtml.cs` (`AdminAccountQuery`), `Pages/Panel/SignIn.cshtml.cs` (`AdminSignInQuery`)
- `Docs/07_API_DESIGN.md` (v0.43), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1616, `07` sürüm satırı)
- `Docs/DEFERRED_BACKLOG.d/T538.md`, `T509.md`, `T529.md`, `T530.md`, `T534.md` (kapanan kalemler), `Docs/STATUS_CHANGELOG.d/T538.md`, `Docs/IMPLEMENTATION_STATUS.md` (T538 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Form girdi modelleri envanteri koddaki modellerle eşit | ✓ | `FormInputs_AreTheRowsOf07_5_2_ByNameAndField` — bilinen fark listesi yok, sonuç boş; fark `07`'yi koda hizalamakla kapandı (K-1616) |
| 2 | Sorgu modelleri koddakilerle eşit (ad, camelCase parametre, uç) | ✓ | `QueryModels_AreTheRowsOf07_5_3_2_ByNameAndParameter`; altı sorgu modeli ve `GoogleReturnQuery` kodda, `CategoryQuery` `07`'de (K-1616) |
| 3 | Makine ucu gövdeleri alan adlarıyla eşit | ✓ | `MachineBodies_AreTheRowsOf07_5_4_2_ByNameAndField` |
| 4 | "Uçlar" sütunundaki her uç modeli bağlar; modelsiz bağlayan uç ve uçsuz model yok; §5.2.4 modelsiz | ✓ | `Handlers_BindTheModelsOfTheirRows_AndNoEntity` — bilinen fark listesi yok; UC-47, 51, 54, 60, 63, 84, 104, 108 artık model bağlar |
| 5 | Hiçbir işleyici varlık bağlamaz | ✓ | `Handlers_…`; `HandlerCheck_AHandlerThatBindsAnEntity_IsNamed` |
| 6 | `Consistency` özellikli birim testi | ✓ | sınıf `[Trait("Consistency","true")]`; `--filter-trait "Consistency=true"` 310 test yeşil, `--filter-not-trait` 7420 test yeşil ve bu sınıfı içermez |
| 7 | Veritabanına ve ağa dokunmaz, dosya yazmaz | ✓ | `Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile` |

### Öz denetim (bitiş kapısı 2; düzeltme 1'de yeniden)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: envanterden satır silinince ya da koda model eklenince kırılır | ✓ | `FormCheck_ADocumentRowDeleted_NamesTheModelOnlyInTheCode`, `FormCheck_AModelAddedToTheCode_NamesTheModelOnlyInTheCode`, `HandlerCheck_AModelNoHandlerBinds_IsNamed`; envanter koda eşit (bilinen fark yok) |
| KK2: parametre adı değişince kırılır | ✓ | `QueryCheck_AParameterRenamedInTheDocument_NamesBothSides` (iki yönlü) |
| KK3: gövde alanı değişince kırılır | ✓ | `MachineCheck_AFieldRenamedInTheDocument_NamesBothSides`; form ve iç içe liste: `FormCheck_AFieldRenamedInTheDocument_NamesBothSides`, `FormCheck_AListFieldRenamedInTheDocument_NamesTheNestedField` |
| KK4: girdi okuyan modelsiz uç kırar; §5.2.4 işleyicileri kırmaz | ✓ | `HandlerCheck_AHandlerThatReadsInputWithoutAModel_IsNamed`, `HandlerCheck_AModellessHandlerThatBindsAModel_IsNamed`; gerçek uygulamada `Handlers_…` boş; `Request.Query` okuyan geriye kalan yerler `07`'nin saydığı ortak `returnUrl` ve `notice` ile UC-88, UC-92, UC-94'ün kayıt sayfalarıdır (`07 §5.3.2` son paragraf) |
| KK5: varlık bağlayan işleyici kırar | ✓ | `HandlerCheck_AHandlerThatBindsAnEntity_IsNamed` |
| KK6: Lint koşar, Unit koşmaz | ✓ | iki süzgeç komutu ayrı koşuldu (310 ve 7420) |
| KK7: testler bağlantı dizesi okumaz | ✓ | `Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile`; Consistency koşusu veritabanı değişkeni olmadan yeşil |
| Kapsam içi fark ertelenmez (yönetici kararı, düzeltme 1) | ✓ | `KnownModelDifferences` kaldırıldı; D-1606–D-1609, D-1333, D-1527, D-1538, D-1551 ve D-1552 ✅ (kanıt: K-1616 + test adları) |
| Aynı sayıyı/adı anan öteki yerler (sık FAIL) | ✓ | `07`'de eski adlar (`AcceptedCheckboxes`, `OriginalIsSalesTemporarilyClosed`, üç eski geri ödeme modeli, `UC-117'nin iki alanı`, `HomepageVersion` form alanı) tarandı: yalnız tarihçe satırları (v0.17 deep review, K-1196, 06'nın veri alanı) kaldı; `07` başlık, dipnot ve `PRODUCT_DISCOVERY_STATUS` tablosu v0.43; `09` 1785 `ImageFile` dosya parçası adı olarak geçerli |
| Yeni belge metni karar kaydısız (sık FAIL) | ✓ | `07`'deki bütün değişiklikler K-1616 ile kayıtlı |
| Hedefi T538 olan backlog kalemi | ✓ | `grep T538` backlog: hedefi T538 olan kalem yok; açık kalan D-1205 (iyileştirme) aşağıda |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | yeni dış bağımlılık yok; sorgu modelleri metin alır, geçersiz değer bağlama hatası değil sayfanın kendi cevabıdır; MyAccount'ta aynı parametrenin iki kez gönderilmesi eskisi gibi değer sayılmaz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-trait "Consistency=true"` | 310 / 310 |
| Unit (Consistency dışı) | ✓ | `… --filter-not-trait "Consistency=true"` | 7420 / 7420 (mimari model bağlama kuralı dahil) |
| Integration | ✓ (ilgili sınıflar) — TAM: CI | `RegistrationPageTests`, `CustomerSignInPageTests`, `AccountProfilePageTests`, `CustomerPasswordResetAndReauthenticationPageTests`, `MyAccountPageTests`, `PanelAccessPagesTests`; `PanelHomeAndVerificationPagesTests`, `OneTimeValueTests`, `SessionAccessTests`, `CookiePolicyTests`, `StorefrontFrameTests`, `FrameLayoutTests` | 158 / 158 ve 102 / 102 |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <değişen .cs>` | değişiklik yok |
| Contract | — | — | NOT_APPLICABLE |

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

- **Branch:** `task/T538-uc-model-esitlik`
- **Commit:** `cccb597b` — T538: Uç ve model eşitlik denetimleri: form girdi, sorgu, makine ucu gövdeleri ve uç bağlama (devam); düzeltme 1 commit'i PR'ın son commit'idir
- **PR:** #579
- **CI:** yardımcı yürütür
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Bu task'ta dış varsayım yok (depo dosyalarını ve uygulama derlemesini okuyan birim testleri) | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `ProductQuery` ve `CategoryQuery` (ve kırıntı modelleri) tek tipe indirilmedi | D-1205 (açık) | Eşitlik `07 §5.3.2`'ye `CategoryQuery` satırı eklenerek sağlandı; tekilleştirme saf iyileştirmedir, eşitlik için gerekmez |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; WIP commit'i `96a4c23c` üzerinden sürdürüldü, derlenmemiş yarım iş yeniden yazılıp tamamlandı)
- **Ana dal CI startup (Adım 0):** `43666144462` success, `43666144436` success, `43618088406` success
- **Repo memory (bitiş kapısı 9):** dokunulmadı
- **Düzeltme 1 (2026-11-15):** ilk teslimdeki ⚠ (a) ve (b) — kabul kriterleri 1, 2 ve 4'ün `~` kalması — yönetici kararıyla (yardımcı 70, sahip yerine) kapatıldı: `07` koda hizalandı, sorgu modelleri kodda kuruldu, bilinen fark listesi kaldırıldı (K-1616). Kriterlerin hepsi ✓.
