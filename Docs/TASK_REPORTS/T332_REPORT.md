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
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07; head `48db1e8` + `origin/main` (T309, `987e438`) birleştirmesi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (D-283) |

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = ana checkout'un `scripts/git-hooks`'u; ana dal CI son üç tamamlanmış run success — 42368786653, 42368786665 (`ad8c48d`), 42366197659 (`1112ca6`); `MEMORY.md` T332 satırı var.

**Kabul kriterleri (validator kanıtı):**

| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `SalesModelShape` uygulama derlemesinin bütün tür, üye, enum değeri ve parametre adlarını sözcüklere bölüp tarar (quote, price on request, appointment, booking, …); `NoTableOrColumn_…` DB adlarını tarar; `src/Shopfolio.Web` arayüz dosyalarında (cshtml/razor/js/json/resx) "fiyat sorunuz", "teklif iste", "randevu" grep'i boş |
| 2 | ✓ | `ProductType` üç üye; `products.product_type` kısıtı dördüncü değeri `CheckViolation` ile reddeder; `variants`'ta tip/istisna/ifa süresi sütunu yok (Integration + Unit `TheVariant_HasNoType…`) |
| 3 | ✓ | `CartItem` yalnız `VariantId` taşır; `cart_items`'ta `product_id` yok |
| 4 | ✓ | `OptionStructureRules.NextOptionOrder` sınırı `ProductConstants.OptionDimensionMax`'tan okur; `Option` kısıtı üçüncü sırayı `CheckViolation` ile reddeder; kuralda literal yok (kaynak taraması testi) |
| 5 | ✓ | `GetOptionMatrixAsync` kartezyen hücreleri verir, açılmamış hücre `VariantId = null` (`IsOpened = false`); vitrin gösterimi T503 (D-247) |
| 6 | ✓ | `CheckStockCodeAsync` boşu `Missing`, `ab-ı1`/`Ab-I1`/` ab-i1 ` için `Taken`; benzersiz dizin harfi farklı ikinci kodu `UniqueViolation` ile reddeder, ilk kod `Ahşap-I01` olarak kalır; öneri adresten (`AHSAP-MASA-01`, ilk yirmi karakter, kesimde kalan tire düşer), kaydedilmemiş üründe o anki addan, ad boşsa `null`; ad değişince var olan kod değişmez |
| 7 | ✓ | `Product`/`Variant`'ta min/minimum adlı özellik yok, yalnız `MaxQuantityPerOrder`; DB'de `min_…quantity` sütunu yok; tek adetli `CartItem` kaydedilir |
| 8 | ✓ | review/rating/stars/recommendation/related/similar products adları kodda ve DB'de yok; arayüz dosyalarında "yorum", "puan", "yıldız", "benzer ürün" grep'i boş |
| 9 | ✓ | `withdrawal_exemption_reason` yalnız `products`'ta; bayrak sütunu yok; işaretli ve işaretsiz fiziksel ürün yayına alınır; dijital/hizmette sebep `ProductDefinitionGuard` ile SQL'den önce reddedilir; tip değişiminde de |
| 10 | ✓ | Yayındaki hizmet ifa süresiz `InvalidOperationException` (taslak kaydedilir); 1 ve 30 gün yayınlanır; 0 gün `CheckViolation`; varsayılan yok (`ProductDefinitionRules.IsFulfilmentPeriodEntered`) |

**Task satırları (`12 §7.5`):** `11 §7.1` T332 kaynakları KP-4, KP-5, KP-6, KP-39, KD-6, KD-28, KD-35, KD-37. KP grep'i `VAL-A009`…`VAL-A020`, `VAL-B001`, `VAL-A163`…`VAL-A176`'yı getirir; hepsinin beklenen sonucu vitrin, ürün sayfası, sepet ya da panel ekranıdır (tarayıcı gözlemi, uç yanıtı) ve T332 uç ya da sayfa kurmaz (KP-4 → T332, T334, T503; KP-5 → … T503; KP-6 → … T351, T356, T503; KP-39 → T334, T335, T336, T525) → hepsi `BEKLEMEDE` kalır, `Durum` değişmedi. KD satırları matriste yok.

**Doğrulama kontrol listesi:** KK1–KK10 ✓ (yukarıdaki kanıtlar); `09 §25` ✓ — göç yok, yeni kalıcı enum yok (`StockCodeCheck`, `VariantStructureCheck` saklanmaz), yeni paket yok, modül sınırı korunur (yalnız `Contracts` yüzü), okumalar `AsNoTracking`; `ImageDisposalPart.Quote` → `SqlIdentifier` adlandırması tarama bulgusunun gereğidir, davranış değişmez (ImageDisposal Integration yeşil). Güvenlik: sır yok; yetki etkisi yok (yeni uç yok); girdi — stok kodu sorgusu EF parametresiyle, `StartsWith` öneki yalnız `[a-z0-9-]` adresten; yayın bekçisi bütün `SaveChanges` yollarında, `Products` üzerinde toplu `ExecuteUpdate`/`UPDATE products` yok (grep boş); stok kodu tekliği normalleştirilmiş sütunun benzersiz dizininde.

**Testler (validator):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç proje çıkış 0; Unit 3528/3528 (birleştirmeden önce), 3585/3585 (birleştirmeden sonra); betik testleri 4 dosya geçti; Integration (yerel `postgres:18`) SalesModelTests 28/28, ImageDisposal + Sitemap 10/10, birleştirme sonrası SalesModel + AccountEmailQueue 42/42; dal CI 42376259158 (head `48db1e8`) success — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate.

**Bloke-etmeyen notlar:** K1 — seçenek silinince yalnız sırası 2 olan seçenek kalırsa `CheckVariantStructureAsync(ürün, null, değer)` `Valid` der ama `second_value_needs_first` kuralı reddeder; T332'de seçenek silen yol yok → D-283 (T525). Boş stok kodunun veri düzleminde kısıtı yoktur; yazma yolunun `CheckStockCodeAsync` çağırması D-249'da kayıtlı (ek kayıt gerekmedi).

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Uyuşmazlık: raporda `12` task satırları bölümü yoktu (bu doğrulamada yazıldı).

**Merge notu:** dal `origin/main` (T309, #429) ile birleştirildi; çakışma yalnız kayıt belgelerinde (STATUS, STATUS_CHANGELOG, MEMORY, backlog, T308/T331 raporları), main kayıtları korunarak çözüldü ("Aktif kalem" 109 + D-283 = 110; T309 PASS ve T332 yapım girişleri `STATUS_CHANGELOG.md`'ye taşındı); T312 raporundaki yinelenen post-merge notu main'deki satır olduğu için düşürüldü; T309'un Commit hücresi ve post-merge satırı dolduruldu; kodda çakışma yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T332-satis-modeli`
- **Commit:** PR açıklamasında head SHA
- **PR:** #432
- **CI:** dal CI 42376259158 (head `48db1e8`) success
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
