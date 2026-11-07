# T332 — Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Ürün tanımı kuralı:** `ProductDefinitionRules` (saf) ve `ProductDefinitionGuard` (SaveChanges kesicisi, `AppDbContext.Configure`'a kayıtlı): cayma istisnası sebebi yalnız fiziksel üründe; yayındaki hizmet ürününde ifa süresi (gün) zorunlu ve en az 1; istisna yayın kapısına girmez; tip değişiminde dolu istisna sebebi bekçiye takılır. Ürün tipi üç değerli kapalı alan, varyantta tip, istisna ve ifa süresi alanı yok (şema değişmedi).
- **Seçenek yapısı:** `OptionStructureRules` (saf: `NextOptionOrder`, `CheckBinding`, `BuildMatrix`); boyut tavanı `ProductConstants.OptionDimensionMax`'tan okunur, kuralda ve testte literal yok. `ProductStructureReader`: `CheckVariantAsync` (varyantın değerleri ürünün kendisinin, olan her seçenek için zorunlu, birleşim tekil) ve `GetMatrixAsync` (birleşim matrisi tam olmak zorunda değil; açılmayan hücre `VariantId = null`).
- **Stok kodu:** `StockCodes.CheckAsync` (boş = eksik; kimlik biçiminde — harf ayrımsız, I/İ/ı/i tek harf — başka varyantta varsa alınmış; düzenlenen varyant hariç) ve `SuggestAsync` (var olan ürünün adresinden, kaydedilmemiş üründe o anki addan; ad boşsa öneri yok; var olan kodlar değişmez, çakışmada sıra artar). `Slug.cs`: `StockCodeSuggestion.Prefix` eklendi. Kod yazıldığı gibi saklanır (kayıt öncesi normalleştirme yok).
- **Modül yüzü:** `ICatalogModule` + `CatalogModule`: `SuggestStockCodeAsync`, `CheckStockCodeAsync`, `CheckVariantStructureAsync`, `GetOptionMatrixAsync`. Tür adları `Contracts.Variants` alt ad alanında (üç tür, her enum kendi dosyasında: `StockCodeCheck`, `VariantStructureCheck`, `OptionMatrixCell`); Contracts'taki her enum saklanan sayıldığı için alt ad alanı (T331 ile aynı kalıp).
- **Yasak ad taraması:** `SalesModelShape` (yansıma tabanlı: her tür, üye, enum değeri, parametre adı sözcüklerine bölünür; teklif, "fiyat sorunuz", randevu/rezervasyon, asgari adet, yorum/puan/yıldız/öneri kalıpları) ve DB tarafında tablo/sütun adı taraması. Tarama bulgusu olarak `ImageDisposalPart.Quote` (SQL tanımlayıcı kaçışı) `SqlIdentifier` olarak yeniden adlandırıldı.
- Önceki task'ların merge kaydı: T308, T312, T331 status Commit hücreleri ve raporlarının "Post-merge run'lar" satırı.
- Backlog: D-246…D-249.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `ProductDefinitionRules.cs`, `ProductDefinitionGuard.cs`, `OptionStructureRules.cs`, `ProductStructureReader.cs`, `StockCodes.cs`, `CatalogModule.cs`, `ImageDisposalParts.cs` (yeniden adlandırma); `Contracts/ICatalogModule.cs`, `Contracts/Variants/{StockCodeCheck,VariantStructureCheck,OptionMatrixCell}.cs`
- `src/Shopfolio.Web/Infrastructure/`: `AppDbContext.cs` (bekçi kaydı), `Slug.cs`
- `tests/Shopfolio.UnitTests/`: `Catalog/ProductDefinitionRulesTests.cs`, `Catalog/OptionStructureRulesTests.cs`, `Architecture/SalesModelShape.cs`, `Architecture/SalesModelShapeTests.cs`, `Infrastructure/SitemapSourceTests.cs` (sahte modül yeni yöntemleri taşır)
- `tests/Shopfolio.IntegrationTests/Model/SalesModelTests.cs`
- `Docs/DEFERRED_BACKLOG.md` (D-246…D-249), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, T308/T312/T331 raporları

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Satış yalnız doğrudan; teklif, "fiyat sorunuz", talep hattı, randevu, rezervasyon yok | ✓ | Unit `SalesModelShapeTests.NoTypeMemberEnumValueOrParameterOfTheApplication_…` + `TheScan_FindsEachForbiddenFormInItsSpelling` (ihlal örnekleri); Integration `NoTableOrColumn_CarriesAQuoteAnAppointment…` |
| 2 | Üç ürün tipi, ürün düzleminde kapalı alan; varyant devralır | ✓ | Integration `ProductType_IsTheThreeValuesOfTheProduct_AndTheVariantHasNoTypeColumn`, `AFourthProductType_IsRefusedByTheDatabase_…`; Unit `TheVariant_HasNoTypeNoExemptionAndNoFulfilmentPeriod_TheProductHasThem` |
| 3 | Satılan birim varyant; sepete varyant girer | ✓ | Integration `AVariantWithAnyUnitCount_IsSoldAsOne_AndTheCartTakesTheVariantNotTheProduct`; Unit `TheCartItem_ReferencesTheVariant_AndNotTheProduct` |
| 4 | En çok iki boyut; sayı ürün sabitinden | ✓ | Integration `AThirdOptionDimension_IsRefused_ByTheRuleAndByTheDatabase`; Unit `TheNextOptionOrder_IsTheLowestFreeOrderUpToTheConstantLimit_AndNullAtTheLimit`, `TheOptionDimensionLimit_ComesFromTheProductConstant_NotFromALiteralInTheRule` |
| 5 | Matris tam olmak zorunda değil; olmayan birleşim görünür ama seçilemez | ✓ (modül yüzü; vitrin gösterimi D-247) | Integration `TheMatrixNeedNotBeFull_AMissingCombinationIsACellNoVariantOpens`, `TheMatrixOfAProductWithoutOptions_…`; Unit `TheMatrixOfTwoDimensions_NeedNotBeFull_…` |
| 6 | Zorunlu ve tekil stok kodu; harf ayrımsız; yazıldığı gibi; adresten öneri | ✓ | Integration `AStockCode_IsMissingWhenBlank_TakenInItsIdentityForm_AndFreeOtherwise`, `ASecondCodeInAnotherLetterCase_IsRefused_AndTheFirstStaysAsItWasTyped`, `TheSuggestion_IsTheAddressInCapitalsAndTheOrder_RaisedUntilTheCodeIsFree`, `TheSuggestion_ForAProductNotSavedYet_ComesFromTheNameNow_AndIsNullWhenThereIsNone`, `ARenamedProduct_KeepsItsCodes_…` |
| 7 | Asgari sipariş adedi alanı yok | ✓ | Unit `TheProduct_HasNoMinimumOrderQuantityAndTheVariantNoneEither_OnlyTheMaximumPerOrderExists`; Integration `…NoTableOrColumn_…` (min/quantity kalıbı) |
| 8 | Yorum, puan, öneri bloğu yok | ✓ | `SalesModelShape` yasak listesi (review, rating, star, recommendation, related/similar products) hem kodda hem DB adlarında |
| 9 | Cayma istisnası ürün düzleminde; varyanta inmez; yayın kapısına girmez | ✓ | Integration `TheExemption_IsTheReasonFieldBeingFilled_WithNoFlagAndNothingOnTheVariant`, `APhysicalProduct_CarriesAnyExemptionReason_AndIsPublishedWithOrWithoutIt`, `AnExemptionReasonOnANonPhysicalProduct_IsRefusedBeforeAnySql`; Unit `TheExemption_NeverEntersTheGate_…` |
| 10 | Hizmette ifa süresi zorunlu, gün, varsayılan yok, alt sınır 1; girilmeden yayına alınamaz | ✓ | Integration `AServiceWithoutAFulfilmentPeriod_CannotBePublished_ButCanBeADraft`, `AServiceWithAFulfilmentPeriodOfOneDayOrMore_IsPublished_AndZeroDaysIsRefused`; Unit `TheFulfilmentPeriod_IsEnteredFromTheLowerFenceUp_AndHasNoDefault` |

> Kriter 5 ve 6'nın ekran yüzleri (vitrinde seçilemez gösterim, formda öneri) modül yüzü olarak teslim edildi; ekran bağları D-247, D-249.

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1–k10 | ✓ | yukarıdaki tablo |
| KK1 teklif ve "fiyat sorunuz" hattı yok | ✓ | `SalesModelShapeTests`, `NoTableOrColumn_…` |
| KK2 tipin dördüncü değeri kaydedilmez; varyantta tip alanı yok | ✓ | `AFourthProductType_IsRefusedByTheDatabase_…`, `ProductType_IsTheThreeValues…` |
| KK3 sepete varyant girer | ✓ | `AVariantWithAnyUnitCount_IsSoldAsOne_AndTheCartTakesTheVariantNotTheProduct` |
| KK4 üçüncü boyut reddedilir | ✓ | `AThirdOptionDimension_IsRefused_ByTheRuleAndByTheDatabase` |
| KK5 eksik birleşim seçilemez | ✓ (modül yüzü) | `TheMatrixNeedNotBeFull_…` |
| KK6 harfi farklı kod reddedilir, ilk kod yazıldığı gibi; öneri adresten, ad boşsa yok | ✓ | `ASecondCodeInAnotherLetterCase_IsRefused_AndTheFirstStaysAsItWasTyped`, `TheSuggestion_ForAProductNotSavedYet_…IsNullWhenThereIsNone` |
| KK7 asgari adet alanı yok; tek adet sepete eklenir | ✓ | `TheProduct_HasNoMinimumOrderQuantity…`, `AVariantWithAnyUnitCount_IsSoldAsOne_…` |
| KK8 yorum formu, puan, öneri bloğu yok | ✓ | `SalesModelShape` taraması |
| KK9 istisna işaretinin varyantta alanı yok; işaretsiz ürün yayına alınır | ✓ | `TheExemption_IsTheReasonFieldBeingFilled_…`, `APhysicalProduct_CarriesAnyExemptionReason_AndIsPublishedWithOrWithoutIt` |
| KK10 ifa süresiz hizmet yayına alınamaz; 0 gün reddedilir | ✓ | `AServiceWithoutAFulfilmentPeriod_…`, `…ZeroDaysIsRefused` |
| `09 §25` / `09 §10` | ✓ | bekçi tek yerde (kural saf sınıfta, kesici yalnız çağırır); iş yazan yeni yöntem yok, okumalar salt okunur ve çağıranın işlemine uyar |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | tarama istisnasız; tek bulgu (`Quote`) yeniden adlandırılarak giderildi, listeye istisna eklenmedi |
| Sık FAIL (2) tarama çağrı biçimini kaçırıyor | ✓ | tür, üye, parametre, enum değeri, iç içe tür, alan; camelCase/snake_case/PascalCase yazımlar (`TheScan_FindsEachForbiddenFormInItsSpelling`) ve yanlış pozitif sınaması (`…DoesNotMistakeAnOrdinaryName…`); DB tarafında `start_date` gibi adlar yalnız tam sözcükle (`_star_`) eşleşir |
| Sık FAIL (3) 06 "Varsayılan" DB default'u / FK dizini | ✓ (uygulanmaz) | şema değişmedi; yeni sütun, FK ya da göç yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | boyut sayısı yalnız `ProductConstants.OptionDimensionMax`; yeni enum'lar `Contracts.Variants` altında (`EnumLockTests`, `EnumTableConsistency`, `EnumSourceScanTests` yeşil) |
| Sık FAIL (5) belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09` değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | 3528 geçti (düzeltmeden sonra tam koşu: yeniden adlandırma ve enum dosyası ayrımı sonrası ilgili sınıflar 104 + 38 + 68 yeşil) |
| Integration | ✓ (parçalı) | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-namespace …` | `SalesModelTests` 28; Model ad alanı 953 (süre sınırında kesilen üç göç testi `SchedulerBackupDisposalAndKeyTablesTests`, `CatalogCoreTablesTests` ayrı koşuda 115/115 geçti); Concurrency+Files+Disposal+Database 161; Web+Pages+Jobs 408; Logging+Outages+Security+SignedEndpoints+FileSecrets 116 |
| Format / derleme | ✓ | `dotnet format <proje> --verify-no-changes`; `dotnet build -warnaserror` | üç proje temiz |
| Consistency | ✓ | `--filter-trait "Consistency=true"` (docs commit'inden sonra) | 209 geçti |

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

- **Branch:** `task/T332-satis-modeli`
- **Commit:** PR açıklamasında head SHA
- **PR:** dönüş mesajında
- **CI:** *(yapım işçisi izlemez)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; ayrı `chore:` PR'ında veya sonraki task dalında)*

## Dış Varsayımlar

Dış varsayım: yok. Yapı yalnız veritabanı, `ProductConstants` ve var olan varlıklara dayanır.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ürün formunun tip, ifa süresi, istisna alanları | D-246 | Ekran T525'in |
| 2 | Vitrinde matris okuması ve "seçilemez" gösterimi | D-247 | Sayfa T503'ün |
| 3 | Yayın kapısının geri kalanı | D-248 | T335'in kapsamı; T332 yalnız ifa süresi koşulunu taşır |
| 4 | Seçenek/varyant ekleme yazma yolu, önerinin forma bağı | D-249 | Ürün formu T525'in |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** main `ad8c48d`: Publish 42368786653, CI 42368786665, Publish 42366197659 (`1112ca6`) — success.
- **Repo memory (bitiş kapısı 9):** güncellendi (`MEMORY.md` T332 satırı).
- **Kararlar (⚠ kolay):** (a) modül yüzü + okuma servisleri, yazma yolu ve ekran sonraki task'larda; (b) tam yayın kapısı T335'te; (c) enum'lar `Contracts.Variants` altında; (d) yasak ad taraması yansıma tabanlı, istisnasız.
- **Önceki task kaydı:** T308, T312, T331 status Commit hücreleri ve raporları dolduruldu.
