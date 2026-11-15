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
- **Düzeltme 2 (2026-11-15, doğrulama tur 1 FAIL):** bulgu 1 (ORTA): iki adlı model satırında kodda olmayan adı sessizce atlayan istisna kaldırıldı — `ModelInventoryCheck` satırın her adını koddaki sınıfla iki yönlü karşılaştırır, `EndpointBindingCheck` her adın satırın bir ucunda bağlandığını model başına denetler (`ModelWithoutEndpoint:<ad>`); `07 §5.2.2` koda hizalandı (UC-125 `MarkShippedInput`'u, UC-140'ın reddi `ReceiveReturnInput`'u bağlar; `MarkReshippedInput` ve `RejectReturnInput` satırdan çıktı, `§5.1.5` örnekleri, `07` v0.44, K-1616 ek madde); üç kırılma örneği testi eklendi. Bulgu 2 (DÜŞÜK): dört uç (UC-60, UC-63, UC-104, UC-108) tek yardımcıdan (`QueryValues.SentOnce`) geçer — iki kez gönderilen parametre eskisi gibi değer sayılmaz; her biri için istek testi.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/ApiInventoryDocument.cs`, `ModelInventoryCheck.cs`, `EndpointBindingCheck.cs`, `ModelInventoryConsistencyTests.cs` (`KnownModelDifferences.cs` kaldırıldı)
- `src/Shopfolio.Web/Pages/Storefront/SignOut.cshtml.cs`, `Pages/Content/ContentListPages.cs`, `Pages/Panel/Coupons.cshtml.cs`
- Düzeltme 1: `Pages/Storefront/AccessQueries.cs` (yeni: `RegisterQuery`, `SignInQuery`, `AccountQuery`, `GoogleReturnQuery`), `Register.cshtml.cs`, `SignIn.cshtml.cs`, `GoogleReturn.cshtml.cs`, `Pages/Account/Reauthentication.cshtml.cs`, `Pages/Account/Profile.cshtml.cs`, `Pages/Panel/Reauth.cshtml.cs` (`ReauthenticateQuery`), `Pages/Panel/MyAccount.cshtml.cs` (`AdminAccountQuery`), `Pages/Panel/SignIn.cshtml.cs` (`AdminSignInQuery`)
- Düzeltme 2: `src/Shopfolio.Web/Infrastructure/Binding/QueryValues.cs` (yeni), `Reauthentication.cshtml.cs`, `Profile.cshtml.cs`, `Reauth.cshtml.cs`, `MyAccount.cshtml.cs`, `OrderDetail.cshtml.cs` ve `OrderDetail.ItemSteps.cs` (yalnız özet yorumu); testler: `ModelInventoryCheck.cs`, `EndpointBindingCheck.cs`, `ModelInventoryConsistencyTests.cs`, `AccountProfilePageTests.cs`, `CustomerPasswordResetAndReauthenticationPageTests.cs`, `MyAccountPageTests.cs`, `PanelHomeAndVerificationPagesTests.cs`
- `Docs/07_API_DESIGN.md` (v0.44), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1616 ek madde, `07` sürüm satırı)
- `Docs/DEFERRED_BACKLOG.d/T538.md`, `T509.md`, `T529.md`, `T530.md`, `T534.md` (kapanan kalemler), `Docs/STATUS_CHANGELOG.d/T538.md`, `Docs/IMPLEMENTATION_STATUS.md` (T538 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Form girdi modelleri envanteri koddaki modellerle eşit | ✓ | `FormInputs_AreTheRowsOf07_5_2_ByNameAndField` — bilinen fark listesi yok, sonuç boş; fark `07`'yi koda hizalamakla kapandı (K-1616); düzeltme 2: iki adlı satırda her ad kodda sınıf (`FormCheck_ARowOfOneNameGivenASecondNameThatTheCodeLacks_NamesTheInventedModel`, `FormCheck_ARowOfTwoNamesLosingOne_…`, `FormCheck_TheTwoModelsOfOldRowsPutBack_…`) |
| 2 | Sorgu modelleri koddakilerle eşit (ad, camelCase parametre, uç) | ✓ | `QueryModels_AreTheRowsOf07_5_3_2_ByNameAndParameter`; altı sorgu modeli ve `GoogleReturnQuery` kodda, `CategoryQuery` `07`'de (K-1616) |
| 3 | Makine ucu gövdeleri alan adlarıyla eşit | ✓ | `MachineBodies_AreTheRowsOf07_5_4_2_ByNameAndField` |
| 4 | "Uçlar" sütunundaki her uç modeli bağlar; modelsiz bağlayan uç ve uçsuz model yok; §5.2.4 modelsiz | ✓ | `Handlers_BindTheModelsOfTheirRows_AndNoEntity` — bilinen fark listesi yok; UC-47, 51, 54, 60, 63, 84, 104, 108 artık model bağlar; düzeltme 2: uç ↔ model eşlemesi model başına (`ModelWithoutEndpoint:<ad>`), UC-125 `MarkShippedInput`'u, UC-140 reddi `ReceiveReturnInput`'u bağlar |
| 5 | Hiçbir işleyici varlık bağlamaz | ✓ | `Handlers_…`; `HandlerCheck_AHandlerThatBindsAnEntity_IsNamed` |
| 6 | `Consistency` özellikli birim testi | ✓ | sınıf `[Trait("Consistency","true")]`; `--filter-trait "Consistency=true"` 313 test yeşil (düzeltme 2), `--filter-not-trait` 7420 test yeşil ve bu sınıfı içermez |
| 7 | Veritabanına ve ağa dokunmaz, dosya yazmaz | ✓ | `Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile` |

### Öz denetim (bitiş kapısı 2; düzeltme 1 ve düzeltme 2 (yedi kriter ve bütün KK yeniden) )

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: envanterden satır silinince, koda model eklenince, satıra kodda olmayan ad eklenince ya da iki adlı satırdan ad silinince kırılır (düzeltme 2) | ✓ | `FormCheck_ARowOfOneNameGivenASecondNameThatTheCodeLacks_NamesTheInventedModel`, `FormCheck_ARowOfTwoNamesLosingOne_NamesTheModelOnlyInTheCode`, `FormCheck_ADocumentRowDeleted_NamesTheModelOnlyInTheCode`, `FormCheck_AModelAddedToTheCode_NamesTheModelOnlyInTheCode`, `HandlerCheck_AModelNoHandlerBinds_IsNamed`; envanter koda eşit (bilinen fark yok) |
| KK2: parametre adı değişince kırılır | ✓ | `QueryCheck_AParameterRenamedInTheDocument_NamesBothSides` (iki yönlü) |
| KK3: gövde alanı değişince kırılır | ✓ | `MachineCheck_AFieldRenamedInTheDocument_NamesBothSides`; form ve iç içe liste: `FormCheck_AFieldRenamedInTheDocument_NamesBothSides`, `FormCheck_AListFieldRenamedInTheDocument_NamesTheNestedField` |
| KK4: girdi okuyan modelsiz uç kırar; §5.2.4 işleyicileri kırmaz | ✓ | `HandlerCheck_AHandlerThatReadsInputWithoutAModel_IsNamed`, `HandlerCheck_AModellessHandlerThatBindsAModel_IsNamed`; gerçek uygulamada `Handlers_…` boş; `Request.Query` okuyan geriye kalan yerler `07`'nin saydığı ortak `returnUrl` ve `notice` ile UC-88, UC-92, UC-94'ün kayıt sayfalarıdır (`07 §5.3.2` son paragraf) |
| KK5: varlık bağlayan işleyici kırar | ✓ | `HandlerCheck_AHandlerThatBindsAnEntity_IsNamed` |
| KK6: Lint koşar, Unit koşmaz | ✓ | iki süzgeç komutu ayrı koşuldu (310 ve 7420) |
| KK7: testler bağlantı dizesi okumaz | ✓ | `Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile`; Consistency koşusu veritabanı değişkeni olmadan yeşil |
| Kapsam içi fark ertelenmez (yönetici kararı, düzeltme 1) | ✓ | `KnownModelDifferences` kaldırıldı; D-1606–D-1609, D-1333, D-1527, D-1538, D-1551 ve D-1552 ✅ (kanıt: K-1616 + test adları) |
| Denetim kodunda başka sessiz atlama/istisna kalmadı (düzeltme 2) | ✓ | `grep -n "continue|Skip|Except|ignore"` Consistency dosyalarında: kalanlar yalnız ayrıştırıcı (`ApiInventoryDocument`, eşleşmeyen parça ve çözümlenmiş alan), yol parçası eşlemesi (`EndpointBindingCheck`) ve iki yönlü `Except` karşılaştırmaları; model adı ya da uç atlayan yok |
| Dört uç aynı yoldan (sık FAIL: yeniden düzenlemede kenar girdi) | ✓ | UC-60, UC-63, UC-104, UC-108 `QueryValues.SentOnce`; testler: `AnOperationParameterSentTwice_IsNoOperation_A303ToTheAccountPage_T538`, `AParameterSentTwice_IsNoValue_TheFormStaysClosed_T538`, `AParameterSentTwice_IsNoValue_KeepsTheFormsClosed_T538` (iki satır), `TheReauthenticationPage_Redirects_…` yeni satır |
| Aynı sayıyı/adı anan öteki yerler (sık FAIL) | ✓ | `07`'de eski adlar (`AcceptedCheckboxes`, `OriginalIsSalesTemporarilyClosed`, üç eski geri ödeme modeli, `UC-117'nin iki alanı`, `HomepageVersion` form alanı) tarandı: yalnız tarihçe satırları (v0.17 deep review, K-1196, 06'nın veri alanı) kaldı; `07` başlık, dipnot ve `PRODUCT_DISCOVERY_STATUS` tablosu v0.43; `09` 1785 `ImageFile` dosya parçası adı olarak geçerli |
| Yeni belge metni karar kaydısız (sık FAIL) | ✓ | `07`'deki bütün değişiklikler K-1616 ile kayıtlı |
| Hedefi T538 olan backlog kalemi | ✓ | `grep T538` backlog: hedefi T538 olan kalem yok; açık kalan D-1205 (iyileştirme) aşağıda |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | yeni dış bağımlılık yok; sorgu modelleri metin alır, geçersiz değer bağlama hatası değil sayfanın kendi cevabıdır; MyAccount, Reauthentication, Profile ve Panel/Reauth aynı yardımcıdan (`QueryValues.SentOnce`) geçer: iki kez gönderilen parametre eskisi gibi değer sayılmaz (düzeltme 2; istek testleri) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-trait "Consistency=true"` | 313 / 313 |
| Unit (Consistency dışı) | ✓ | `… --filter-not-trait "Consistency=true"` | 7420 / 7420 (mimari model bağlama kuralı dahil) |
| Integration | ✓ (ilgili sınıflar; düzeltme 2) — TAM: CI | `AccountProfilePageTests`, `CustomerPasswordResetAndReauthenticationPageTests`, `MyAccountPageTests`, `PanelHomeAndVerificationPagesTests`, `CustomerSignInPageTests`, `GoogleRegistrationTests` · `CustomerSignOutPageTests`, `RegistrationPageTests`, `PanelAccessPagesTests`, `CouponsPageTests`, `ContentPagesTests`, `OneTimeValueTests`, `AdminAccountsPageTests` (postgres:18, `track_commit_timestamp=on`) | 135 / 135 ve 117 / 117 |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <değişen .cs>` | değişiklik yok |
| Contract | — | — | NOT_APPLICABLE |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-15, head `ba579a9b`) |
| Bulgu sayısı | 2 — 1 ORTA S1 (kriter 1 / KK1: `07 §5.2.2`'nin `MarkReshippedInput` ve `RejectReturnInput` adları kodda yok, iki adlı satır istisnası gizliyor) · 1 DÜŞÜK S1 (UC-60, UC-63, UC-108'de yinelenen parametrenin okunuşu değişti; K-1616 (2)) |
| Düzeltme gerekli mi | Evet — iki bulgu bu dalda |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-1614) |

Tur 1'in tam sonucu raporun sonundaki "Doğrulama Sonucu — T538 … — tur 1" bölümündedir.

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

## Doğrulama tur 1

### Doğrulama Sonucu — T538 Uç ve model eşitlik denetimleri: uç envanteri, form girdi ve sorgu modelleri, makine ucu gövdeleri — tur 1
**Tarih:** 2026-11-15   **Branch:** task/T538-uc-model-esitlik   **Commit:** `ba579a9b` (doğrulanan head; doğrulama kaydı bunun üstüne commit'lendi — kayıt commit'i aşağıda)

#### Verdict: ✗ FAIL

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (worktree, `T538-val` = `origin/task/T538-uc-model-esitlik`) |
| Hook kurulumu | ✓ | `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana çalışma kopyasının bekçisi; worktree'ler ortak config) |
| Ana dal CI (son 3) | ✓ | `43666144462` success · `43666144436` success · `43618088406` success |
| Memory drift | — | Adım 0b kaldırıldı (2026-11-08) |
| main birleştirmesi | ✓ | `git merge origin/main` → "Already up to date"; çakışma yok |

#### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Form girdi envanteri (`07 §5.2`) koddaki modellerle eşit: her satırın adı ve alanları var, envanterde olmayan model yok | ✗ | `07 §5.2.2` satırları `MarkShippedInput · MarkReshippedInput` (UC-124 · UC-125) ve `ReceiveReturnInput · RejectReturnInput` (UC-140) dört model adı sayar; kodda `MarkReshippedInput` ve `RejectReturnInput` sınıfı yok (`grep -rlE "class MarkReshippedInput\b\|class RejectReturnInput\b" src` → 0); `OnPostMarkReshippedAsync(Guid, MarkShippedInput, …)` (`Panel/OrderDetail.cshtml.cs:265`), `OnPostRejectReturnAsync(Guid, ReceiveReturnInput, …)` (`Panel/OrderDetail.ItemSteps.cs:282`). Denetim bunu görmez: `ModelInventoryCheck.Problems` satır adlarından yalnız kodda bulunanları "belgelenmiş" sayar (`ModelInventoryCheck.cs:49`), `EndpointBindingCheck.CheckRows` satırın adlarından birinin bağlanmasını yeterli sayar (`EndpointBindingCheck.cs:118`, `:125`). Geçici sınama (dalda tutulmadı): `| \`CancelOrderInput\` |` → `| \`CancelOrderInput\` · \`FakeCancelInput\` |` yapınca form ve uç denetimi **0 sorun**; `RejectReturnInput` satırdan silinince de 0 sorun. Bu "iki adlı satır tek sınıfla taşınır" kuralının `07`'de dayanağı yok (`grep "tek sınıf\|aynı sınıf\|iki model" Docs/07_API_DESIGN.md` boş; §5.1.5 "Model adı formun asıl işleyicisinin adıdır") ve K-1616'nın "kapsam içi fark bırakmaz" kararına aykırı — gizli istisna |
| 2 | Sorgu modelleri (`07 §5.3.2`) koddakilerle eşit: ad, camelCase parametre, uçlar | ✓ | `QueryModels_AreTheRowsOf07_5_3_2_ByNameAndParameter` yeşil; 20 satır; geçici sınama: `RegisterQuery` satırı silinince "Model: only in the model: RegisterQuery", `pendingRegistration`→`pendingSignup` iki yönlü, `error`→`err` (`GoogleReturnQuery`) iki yönlü, `RegisterQuery` UC-47→UC-48 `ModelNotBound` + `UseCaseNotListed`. (Not: sorgu satırlarında iki adlı tek satır `AccountQuery · AdminAccountQuery`; ikisi de kodda var, kriter 1'deki istisna burada fiilen fark gizlemiyor) |
| 3 | Makine ucu gövdeleri (`07 §5.4.2`) alan adlarıyla eşit | ✓ | `MachineBodies_AreTheRowsOf07_5_4_2_ByNameAndField` yeşil; geçici sınama: `fileName`→`name` iki yönlü, `UploadRejectedResponse` satırı silinince "only in the model" |
| 4 | "Uçlar" sütunundaki her uç o modeli bağlar; modelsiz bağlayan uç ve ucu olmayan model yok; §5.2.4 işleyicileri modelsiz | ~ | `Handlers_BindTheModelsOfTheirRows_AndNoEntity` yeşil; geçici sınama: §5.2.4'ten UC-29 çıkarılınca `NoModelRow:UC-29`, `CancelOrderInput` UC-39→UC-40 üç sorun. Eksik: çok adlı satırda uç ↔ model eşlemesi satır düzeyinde (kriter 1'deki istisna) — UC-125'in `07`'deki modeli `MarkReshippedInput`, kod `MarkShippedInput` bağlar ve denetim geçer |
| 5 | Hiçbir işleyici varlık bağlamaz | ✓ | `EndpointBindingCheck` `EntityBound` (işleyici parametresi) + T211 `ModelBindingRules` (bağlı özellik, model alanı); `HandlerCheck_AHandlerThatBindsAnEntity_IsNamed` |
| 6 | `UnitTests/Consistency/`, `Consistency` özelliği; Lint koşar, Unit dışarıda | ✓ | `[Trait("Consistency", "true")]` (`ModelInventoryConsistencyTests.cs:18`); `.github/ci-commands.env:27` LINT `--filter-trait "Consistency=true"`, `:36` UNIT `--filter-not-trait "Consistency=true"`; yerel 310 / 7420 ayrı koşu |
| 7 | DB ve ağa dokunmaz; dosya yazmaz | ✓ | Üç denetim dosyası yalnız `File.ReadAllText` (07) ve yansıma kullanır; `Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile`; Consistency koşusu `SHOPFOLIO_TEST_DATABASE` olmadan yeşil |

#### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| — | — | — | — | — | — |

`grep -nE "07 §5\.2([^0-9]|$)|07 §5\.3|07 §5\.4|T538" Docs/12_VALIDATION_PROTOCOL.md` → yalnız `VAL-A580` (UC-15 tuzak alanı, iletişim formunun task'ının satırı); T538'in matris satırı yok, `Durum` hücresi yazılmadı, matris betiği gerekmedi.

#### Doğrulama Kontrol Listesi
- [ ] KK1: envanterden satır silinince ya da koda model eklenince test kırılır — satır silme ve kod modeli ekleme kırıyor, ama satıra kodda olmayan ikinci model adı eklemek ya da iki adlı satırdan adı silmek kırmıyor (kriter 1)
- [x] KK2: parametre adı değişince test kırılır
- [x] KK3: gövde alanı değişince test kırılır
- [x] KK4: girdi okuyan modelsiz uç testi kırar; §5.2.4'ün işleyicileri kırmaz (işleyici parametresi düzeyinde; doğrudan `Request.Form/Query` okuması kapsam dışı → K1)
- [x] KK5: varlık bağlayan işleyici testi kırar
- [x] KK6: Lint koşar, Unit koşmaz
- [x] KK7: testler bağlantı dizesi okumaz
- [x] `09 §25` / güvenlik taraması — aşağıda; davranış farkı bulgu 2

#### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Restore | ✓ | `dotnet restore Shopfolio.slnx` | tamam |
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format (değişen 16 .cs) | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes --no-restore --include <16 dosya>` | çıkış 0 |
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build --filter-trait "Consistency=true"` | 310 / 310 |
| Unit (Consistency dışı) | ✓ | `... --filter-not-trait "Consistency=true"` | 7420 / 7420 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Integration (ilgili sınıflar) — TAM: CI | ✓ | `AccountProfilePageTests`, `CustomerPasswordResetAndReauthenticationPageTests`, `MyAccountPageTests`, `CustomerSignInPageTests`, `CustomerSignOutPageTests`, `GoogleRegistrationTests`, `RegistrationPageTests`, `PanelAccessPagesTests` · `CouponsPageTests`, `ContentPagesTests`, `OneTimeValueTests`, `PanelHomeAndVerificationPagesTests`, `AdminAccountsPageTests` (postgres:18, `track_commit_timestamp=on`, 127.0.0.1:56738) | 175 / 175 · 72 / 72 |
| CI (PR #579, run `43684165804`, head `ba579a9b`) | koşuyor | `gh pr checks 579` | Lint, Build, Unit, Contract, Migration, Paketleme, E2E pass; **Integration pending (CI koşuyor)** |

#### Güvenlik Kontrolü
- Secret sızıntısı: Temiz (yeni sır, bağlantı dizesi yok)
- Auth etkisi: Temiz — yeniden doğrulama izni hâlâ oturumda `HoldsReauthenticationAsync` ile denetleniyor; bulgu 2 yalnız yinelenen parametrenin okunuşu
- Input validation: Bulgu 2 (DÜŞÜK) — sorgu modelleri metin alır (geçersiz değer bağlama hatası değil, `07 §2.6.6`), ama yinelenen tek değerli parametre davranışı değişti
- Yeni bağımlılık: Yok

#### Bulgular (`12 §8.2` defect kaydı)
| # | Madde (`VAL-…`) | Severity | Hata sınıfı (S1/S2/S3) | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| 1 | Kriter 1, KK1 (kriter 4'ü de etkiler); matris satırı yok | ORTA | S1 | Beklenen: `07 §5.2`'nin her model adı kodda bir sınıf, denetim adı yalnız belgede kalan modeli söyler (K-1616: istisna/bilinen fark listesi yok). Gerçekleşen: `MarkReshippedInput` ve `RejectReturnInput` yalnız `07`'de; denetim iki adlı satırda kodda bulunmayan adı sessizce atlıyor (`ModelInventoryCheck.cs:47-50`, `EndpointBindingCheck.cs:118,124-128`), uydurma ad ekleme sınaması 0 sorun. Düzeltme: (a) iki adlı satırdaki her ad koddaki bir sınıf olmalı (istisna kaldırılır) ve uç ↔ model eşlemesi model başına denetlenmeli; (b) fark ya `07`'yi koda hizalayarak (UC-125 `MarkShippedInput`'u, UC-140'ın reddi `ReceiveReturnInput`'u bağlar — §5.1.5'in "aynı formu gönderen işleyiciler aynı modeli bağlar" cümlesine örnek, karar kaydı) ya da iki sınıf açılarak kapanır; (c) satıra kodda olmayan ad ekleyen kırılma örneği testi | Kriter 4 (UC-125/UC-140 eşlemesi); `09 §21.4` m.1 | `T538a` düzeltme turu (bu dal) | `tests/Shopfolio.UnitTests/Consistency/ModelInventoryCheck.cs`, `EndpointBindingCheck.cs`, `ModelInventoryConsistencyTests.cs`, `Docs/07_API_DESIGN.md` §5.2.2 (ya da `Panel/OrderDetail*.cs`) |
| 2 | K-1616 (2) "sorgu dizesi adları ve davranış değişmez"; matris satırı yok | DÜŞÜK | S1 | Beklenen: sorgu modeline geçiş davranışı değiştirmez. Gerçekleşen: eskiden `Request.Query[...]` yinelenen parametreyi virgülle birleştirip ("PasswordChange,EmailChange") tanımadığı için UC-60 (`Account/Reauthentication.cshtml.cs:90`) ve UC-108 (`Panel/Reauth.cshtml.cs:74`) 303/"işlem değil" dönüyordu, UC-63 (`Account/Profile.cshtml.cs:232-233`) formu açmıyordu; şimdi model bağlayıcı ilk değeri alıyor ve ekran/form açılıyor. Panel UC-104 (`MyAccount.cshtml.cs:123`) `Count == 1` denetimini bilerek korudu, öteki üçü korumadı — tutarsız. İzin hâlâ oturumda doğrulandığından güvenlik etkisi yok. Düzeltme: üç uçta `Count == 1` (ya da ortak yardımcı) ile eski davranış, istek testiyle | UC-60, UC-63, UC-108; `07 §2.6.6` | `T538a` düzeltme turu (bu dal) | `src/Shopfolio.Web/Pages/Account/Reauthentication.cshtml.cs`, `Account/Profile.cshtml.cs`, `Panel/Reauth.cshtml.cs` |

#### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Uç bağlama denetimi yalnız işleyici parametrelerine bakar; sayfa modelinin doğrudan `Request.Form[…]`/`Request.Query[…]` okuması (bugün: yeniden çizim — `Register`, `Cart`, `Coupons` — ve `07 §5.3.1` ortak parametreleri) KK4'ün kapsamına girmez; `07` ile çelişen okuma görülmedi | DEFERRED_BACKLOG'a forward → D-1614 (`Docs/DEFERRED_BACKLOG.d/T538.md`) |

#### Backlog kapanışları (odak 4)
D-1606, D-1607, D-1608, D-1609 (`DEFERRED_BACKLOG.d/T538.md`), D-1333 (`T509.md`), D-1527, D-1551 (`T530.md`), D-1538 (`T534.md`), D-1552 (`T529.md`) ✅ — her biri K-1616 ve yeşil eşitlik testleriyle kanıtlı; `07` satırları (§5.2.1 `PlaceOrderInput` dört kutu, §5.2.2 `ProcessRefundInput` `RefundId`/`ShownIban`, `RetryCardRefundInput`, `SaveHomepageInput` `Version`, `SuspendSalesInput`/`ResumeSalesInput` `Original`, `SaveContentInput` `Remove`, `SaveBrandingInput` `RemoveLogo`/`RemoveFavicon`, `CorrectAddressInput` `AddressType`, `CorrectArrivalDateInput` `ArrivalRecordId`, §5.2.3 `ImageFile`, §5.3.2 `CategoryQuery`, `GoogleReturnQuery`) koda birebir (alan testleri iki yönlü yeşil). D-1205 açık (`T503.md`) ✓. K-1616 `PRODUCT_DISCOVERY_STATUS.md:1692` gerekçeli. Kriter 1'deki iki ad K-1616'nın hizalamasında atlanmış.

#### Yapım Raporu Karşılaştırması
- Uyum: 2 uyuşmazlık
- Öz denetim KK1 "envanter koda eşit (bilinen fark yok)" ✓ diyor; `07`'nin iki model adı kodda yok ve denetimin iki adlı satır kuralı bunu gizliyor (bulgu 1).
- Güvenlik satırı yalnız MyAccount'un yinelenen parametreyi eskisi gibi saymadığını yazıyor; UC-60, UC-63, UC-108'deki davranış farkı anılmıyor, değişiklik kaydı "davranış aynı" diyor (bulgu 2).
- Öteki satırlar (KK2–KK7, test sayıları 310 / 7420) bağımsız sonuçla uyumlu.

#### Merge ve Post-merge
- FAIL — merge yok; kayıt dalda kalır.
