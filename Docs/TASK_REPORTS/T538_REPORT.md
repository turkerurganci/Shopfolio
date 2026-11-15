# T538 — Uç ve model eşitlik denetimleri: uç envanteri, form girdi ve sorgu modelleri, makine ucu gövdeleri

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-15

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `ApiInventoryDocument`: `07 §5.2`, `§5.3.2`, `§5.4.2` model tablolarını ve `§3.2` uç envanterini okur (belge metnini parametre alır; yanlış metinle sınanabilir).
- `ModelInventoryCheck`: tablo ile koddaki model sınıflarını ad ve alan düzeyinde iki yönlü karşılaştırır (form: ayarlanabilir alanlar ve işleyicinin doldurduğu listeler, iç içe liste alanları dahil; sorgu: camelCase parametre adları; makine gövdesi: JSON adları).
- `EndpointBindingCheck`: envanterdeki uçları sayfaların `@page` şablonlarına ve işleyicilerine çözer; işleyicinin bağladığı modeli "Uçlar" sütunuyla, `§5.2.4` modelsiz listesiyle ve `06` varlıklarıyla karşılaştırır.
- `ModelInventoryConsistencyTests`: 4 eşitlik testi + her denetim için kırıldığını gösteren ihlal örnekleri + dosya/ağ/veritabanına dokunmama denetimi.
- `KnownModelDifferences`: kodla `07` arasında açık kalan farklar satır satır ve backlog kalemiyle; kapanan fark satırı silinmezse ve backlog kalemi yoksa test kırılır.
- Kodda iki hizalama: `SignOutInput` (UC-77), `ContentListQuery` → `ContentQuery` (UC-07, UC-08), `CouponListQuery` → `CouponQuery` (UC-90).

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/ApiInventoryDocument.cs`, `ModelInventoryCheck.cs`, `EndpointBindingCheck.cs`, `KnownModelDifferences.cs`, `ModelInventoryConsistencyTests.cs`
- `src/Shopfolio.Web/Pages/Storefront/SignOut.cshtml.cs`, `Pages/Content/ContentListPages.cs`, `Pages/Panel/Coupons.cshtml.cs`
- `Docs/DEFERRED_BACKLOG.d/T538.md`, `Docs/STATUS_CHANGELOG.d/T538.md`, `Docs/IMPLEMENTATION_STATUS.md` (T538 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Form girdi modelleri envanteri koddaki modellerle eşit | ~ | `FormInputs_AreTheRowsOf07_5_2_ByNameAndField`; açık farklar D-1606, D-1333, D-1607, D-1527, D-1551, D-1538, D-1608 (⚠ a) |
| 2 | Sorgu modelleri koddakilerle eşit | ~ | `QueryModels_AreTheRowsOf07_5_3_2_ByNameAndParameter`; `ContentQuery`, `CouponQuery` hizalandı; açık: D-1609, D-1205 (⚠ b) |
| 3 | Makine ucu gövdeleri alan adlarıyla eşit | ✓ | `MachineBodies_AreTheRowsOf07_5_4_2_ByNameAndField` (bilinen fark yok) |
| 4 | "Uçlar" sütunundaki her uç modeli bağlar; modelsiz bağlayan uç ve uçsuz model yok; §5.2.4 modelsiz | ~ | `Handlers_BindTheModelsOfTheirRows_AndNoEntity`; `SignOutInput` kodda bağlandı; açık: D-1606, D-1609, D-1205 |
| 5 | Hiçbir işleyici varlık bağlamaz | ✓ | `Handlers_…` (`EntityBound` satırı yok); `HandlerCheck_AHandlerThatBindsAnEntity_IsNamed` |
| 6 | `Consistency` özellikli birim testi | ✓ | sınıf `[Trait("Consistency","true")]`; `--filter-trait "Consistency=true"` 312 test yeşil, `--filter-not-trait` 7420 test yeşil ve bu sınıfı içermez |
| 7 | Veritabanına ve ağa dokunmaz, dosya yazmaz | ✓ | `Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: envanterden satır silinince ya da koda model eklenince kırılır | ✓ | `FormCheck_ADocumentRowDeleted_NamesTheModelOnlyInTheCode`, `FormCheck_AModelAddedToTheCode_NamesTheModelOnlyInTheCode`, `HandlerCheck_AModelNoHandlerBinds_IsNamed` |
| KK2: parametre adı değişince kırılır | ✓ | `QueryCheck_AParameterRenamedInTheDocument_NamesBothSides` (iki yönlü) |
| KK3: gövde alanı değişince kırılır | ✓ | `MachineCheck_AFieldRenamedInTheDocument_NamesBothSides`; form ve iç içe liste: `FormCheck_AFieldRenamedInTheDocument_NamesBothSides`, `FormCheck_AListFieldRenamedInTheDocument_NamesTheNestedField` |
| KK4: girdi okuyan modelsiz uç kırar; §5.2.4 işleyicileri kırmaz | ✓ | `HandlerCheck_AHandlerThatReadsInputWithoutAModel_IsNamed`, `HandlerCheck_AModellessHandlerThatBindsAModel_IsNamed`; gerçek uygulamada §5.2.4 işleyicileri `Handlers_…` testinde sorunsuz (UC-29, 53, 67, 76, 109 …) |
| KK5: varlık bağlayan işleyici kırar | ✓ | `HandlerCheck_AHandlerThatBindsAnEntity_IsNamed` |
| KK6: Lint koşar, Unit koşmaz | ✓ | iki süzgeç komutu ayrı koşuldu (yukarıda 6) |
| KK7: testler bağlantı dizesi okumaz | ✓ | `Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile`; Consistency koşusu veritabanı değişkeni olmadan yeşil |
| Bilinen fark listesi sessiz büyümez (sık FAIL 1) | ✓ | satır başına tam eşleşme (örüntü yok), eski satır `Stale`, backlog kalemi yoksa `NoBacklogItem`: `KnownDifferences_*` testleri |
| Çağrı biçimi kaçağı (sık FAIL 2) | ✓ | işleyiciler `OnGet/OnPost` + `Async` biçimiyle, taban sınıflar dahil aranır; ek argüman ve adlı parametre biçimi parametre listesinden okunur (çağrı değil bildirim taranır) |
| Kural sayısı anan öteki yerler (sık FAIL 4) | ✓ | `09 §21.4` madde 1 zaten T538'i sayar; sayaç değişmedi |
| Hedefi T538 olan backlog kalemi (sık FAIL, yardımcı 66) | ✓ | `grep T538` backlog: hedefi T538 olan kalem yok; kapanan kalem yok |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | yeni dış bağımlılık yok; çıkış işleyicisi aynı davranır (`SignOutInput`) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-trait "Consistency=true"` | 312 / 312 |
| Unit (Consistency dışı) | ✓ | `… --filter-not-trait "Consistency=true"` | 7420 / 7420 |
| Integration | ✓ (ilgili sınıflar) — TAM: CI | `CustomerSignOutPageTests`, `CouponsPageTests`, `ContentPagesTests`, `StorefrontFrameTests` | 81 / 81 |
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
- **Commit:** `cccb597b` — T538: Uç ve model eşitlik denetimleri: form girdi, sorgu, makine ucu gövdeleri ve uç bağlama (devam)
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
| 1 | UC-132, UC-136, UC-137 `07`'nin ayrı model satırlarını kullanmıyor (ortak sınıf, ek alanlar) | D-1606 | `07` ya da kod kararı gerekir; sipariş ayrıntısı geri ödeme akışı |
| 2 | Görsel yükleme dosyası model alanı değil | D-1607 | `07 §2.8` gövde sınıfı kuralıyla birlikte karar |
| 3 | Ekran-içi alan adları (`Version`, `Original`, `AddProducts`) | D-1608 | alan adı form testlerinde sabit; belge mi kod mu kararı |
| 4 | Beş sorgu modeli düz değer; UC-51 Google dönüşü `07`'de yok | D-1609 | 7 sayfanın imzası ve istek testleri; bu makinede tam koşu CI'da |
| 5 | Var olan: D-1205 (CategoryQuery), D-1333, D-1527, D-1538, D-1551 | aynı | önceki task'larda kayıtlı; satırlar `KnownModelDifferences`'ta |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; WIP commit'i `96a4c23c` üzerinden sürdürüldü, derlenmemiş yarım iş yeniden yazılıp tamamlandı)
- **Ana dal CI startup (Adım 0):** `43666144462` success, `43666144436` success, `43618088406` success
- **Repo memory (bitiş kapısı 9):** dokunulmadı
- ⚠ (a) ve (b): kabul kriterleri 1, 2 ve 4 açık farklar nedeniyle tam değil (`~`); farklar `KnownModelDifferences`'ta tek tek ve backlog kalemiyle duruyor; belge ya da kod kararı gerektirdiğinden ve 07'yi karar kaydısız değiştirmemek için ertelendi.
