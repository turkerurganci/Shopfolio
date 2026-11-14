# T525 — Ürün listesi ve ürün formu, görsel yükleme (E-32, E-33)

**Faz:** F5 | **Durum:** ✓ Düzeltme 1 tamamlandı (doğrulama tur 2 bekliyor) | **Tarih:** 2026-11-14

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **E-32 ürün listesi** (`Pages/Panel/Products.cshtml(.cs)`, `Catalog/PanelProductListReader.cs`): en yeni önce sayfalı liste (`CreatedAt`, ardından iç anahtar), ad araması taslak ve arşivi de bulur, yayın durumu süzgeci; fiyat aralığı ve stok durumu (`PanelStockKind`) sunucunun hesabıdır; geçersiz süzgeç değeri süzgeç uygulamaz, son sayfadan öte istek son sayfadır; sayfa başına komut sayısı varyant sayısından bağımsızdır.
- **E-33 ürün formu** (`Pages/Panel/ProductForm.cshtml(.cs)`, `_ProductFormBody.cshtml`, `NewProduct.cshtml`, `Catalog/PanelProductFormReader.cs`): yeni ve var olan form; `Version`, gösterilen stok/kontenjan (`Original` gizli alan), ayrılmış adet ve onu tutan siparişler (bağlantılı), yayın kapısının eksik koşulları; ürün alanları (ad, tip, açıklama, KDV, üretim yeri, ölçü birimi + U7 hatırlatması, kargo/ifa süresi, siparişte en çok adet, cayma istisnası), kategoriler (ana kategori), seçenekler/değerler, galeri (sıra, ana görsel, alternatif metin), varyant tablosu, dijital dosya kancası (T548), indirim.
- **UC-114 `SaveProduct`** (`Catalog/ProductSaves.cs`): ürün, varyantlar, galeri, dosya ve indirim tek işlemde; yeni ürün ve varyant Taslak doğar; açıklama kapalı biçim setiyle süzülür; ürün satırı kilidi (`Catalog/ProductLocks.cs`, `LockKind.Product`, işlem başında); stok/kontenjan gösterilen değere karşı koşullu yazılır; fiyat değişikliği fiyat geçmişine; P-6, P-9, P-44 değişikliği eski/yeni değerle ize (`UpdateSettings`, konu `Product`; K-1615). Ret sırası 404 → alan 422 → 409 → ayrılmış 422 (K-800). Modül ret sebepleri sayfada `ProductSaveRefusals` ile K-799 metnine çevrilir.
- **UC-115/UC-116** (`ProductTransitions`, `ProductRemoval`): mutlak hedef durum, ilk yayın adresi sabitler, vitrinden kalkan kalem sepetten çıkar, iz; yayındaki üründe dosyasız ya da fiyatı sıfır varyant yayına alınmaz; silmede onaysız hâl açık sipariş sayısını söyler.
- **UC-117 `UploadImage`**: kaydı yazmadan formu yeni görselle 200 döner; P-22…P-24 aşımı ve dolu tek görsel alanı 422; görsel gövde tavanı 413 (`RequestBodyLimits`).
- **Kalemin dosya bağı (D-527):** `IOrderingModule.AdvanceItemFilesAsync` + `Catalog/OrderItemFiles.cs`.
- **Bu tur (devam 14):** `origin/main` (3fe7d829, T517) birleştirildi (yalnız `DEFERRED_BACKLOG.md` çakışması: D-487/D-488 T525 hâli, D-428/D-429 T517 hâli korundu); öz denetimde eksik bulunan **U7 ölçü birimi hatırlatması** (`04` 9.5.6; K-798 (7), K-573; VAL-A170) eklendi (`ProductFormModel.MeasurementUnitReminderText`, iki form testinde iddia).

### Düzeltme 1 (2026-11-14; doğrulama tur 1 FAIL'i)

Dal `origin/main` (8a51cdcc) ile güncel; birleştirme gerekmedi.

- **B1 — yükleme testlerinin dosya deposu:** `ProductFormPageTests.Open()` uygulamayı testin kendi geçici dizininde (`%TEMP%/shopfolio-t525-<guid>/images|digital`) bir `FileStoreDirectories` ile açar (`RemoveAll` + `AddSingleton`, `FileEndpointsHost`/`AccountAndStatusCommandTests` kalıbı); `DisposeAsync` kendi dizinini siler. Görsel artık `/var/lib/shopfolio/images`'a (Windows'ta `C:\var\lib\...`) yazılmaz; koşudan sonra o dizinde yeni dosya yok.
- **B2 — ürün kilidi kipi:** `ProductLocks` `FOR NO KEY UPDATE` alır. Sipariş onayı önce varyantı ayırır, sonra `order_items` ekler; kalemin ürüne yabancı anahtarı `FOR KEY SHARE` ister ve düz `FOR UPDATE` bunu bekletip ürünü tutan panel yazımıyla döngü kuruyordu. `NO KEY UPDATE` anahtar paylaşımıyla çakışmaz, panel yazıcılarını yine sıralar. Belge: `09 §10.2` m.5 (kip ve gerekçe), m.6 (tarama cümlesi + eşanlı test), K-1615 (2) metni. Tarama: `ConcurrencySourceScan` `ProductLocks.cs`'te yalnız `NO KEY` biçimine izin verir, düz `FOR UPDATE` orada bulgudur. Eşanlı test `ProductSavesTests.TheFormsSave_AndAnOrderConfirmationOnTheSameVariant_BothComplete_WithoutADeadlock_KK4` (onay varyantı ayırır → form ürünü kilitleyip varyantı bekler → onay kalemini ekleyip kesinleşir → form kesinleşir); eski kiple `40P01 deadlock detected` ile kırmızı olduğu yerelde görüldü. Kilit bekleyen testlerin `pg_stat_activity` yoklaması `%FOR NO KEY UPDATE%` metnine çevrildi (`ProductSavesTests`, `ProductTransitionsTests`).
- **K1 — kategori bağı reddi:** `ProductSaves.SaveAsync` `_links.ApplyAsync`'in reddini atmaz: denetimden sonra silinen kategori `Categories` alanında `InvalidListValue` (422; ana kategori kuralı `FieldRequired`), hiçbir şey yazılmaz. Test `ACategoryDeletedAfterTheCheck_IsRefusedByTheLinksWrite_OnTheCategoryField_AndNothingIsWritten_KK5` (komut yakalayıcı kategoriyi bağ yazımının sayımından hemen önce siler; iki sayım iddia edilir).
- **K2 — D-488:** form fiyat değişikliğinden sonra var olan indirimi `Discounts.SaveAsync` ile yeniden kaydettiği için `02 §3.9.5` denetimi fiyat artışında da koşar. Test `ProductFormPageTests.SaveProduct_APriceRiseUnderADiscount_ThatLiftsTheDiscountedPriceToTheReference_Is422AtThePrice_AndWritesNothing_D488` (satışta olan varyant 100 TL, %10 indirim, fiyat 120 → 108 ≥ 100 → 422, fiyat/geçmiş/indirim/sürüm değişmez). Fiyatı değiştiren tek uygulama yolu form olduğundan D-488 kapandı (✅, `DEFERRED_BACKLOG.md`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/{Products,ProductForm,NewProduct}.cshtml(.cs)`, `_ProductFormBody.cshtml`, `ProductSaveRefusals.cs`, `SaveProductInput.cs`; `Pages/Components/{UploadComponentModels,ConfirmModels}.cs`, `Pages/Lists/ListModels.cs`, `Pages/Shared/_PanelTable.cshtml`, `wwwroot/css/admin.css`
- `src/Shopfolio.Web/Modules/Catalog/{ProductSaves,ProductLocks,PanelProductListReader,PanelProductFormReader,OrderItemFiles,ProductTransitions,ProductRemoval,PublishGate,StockCounters,ProductGallery,ProductCategoryLinks,PriceLabelRules,Discounts,CatalogModule}.cs`, `Contracts/{ICatalogModule,PanelProductFormContracts,PanelProductListContracts,PanelStockKind,ProductSaveContracts,PublishChangeResult}.cs`
- `Modules/Ordering/{OrderingModule,Contracts/IOrderingModule,Documents/ConfirmationSummaryText}.cs`, `Modules/AfterSales/Contracts/WithdrawalExemptionReasonNames.cs`, `Modules/Settings/{CountryList,Contracts/ICountryList}.cs`, `Modules/Administration/AuditEntryRules.cs`
- `Infrastructure/{LockKind,LockSequence,Money}.cs`, `Security/RequestBodyLimits.cs`, `Validation/{FieldMessageCatalog,FieldRuleAttribute,InvalidNetQuantityAttribute,ValidationPageFilter}.cs`
- Testler: Integration `Web/ProductFormPageTests.cs`, `Web/ProductListPageTests.cs`, `Catalog/ProductSavesTests.cs`, `Catalog/ProductTransitionsTests.cs`, `Catalog/ProductPublishGateTests.cs`, `Files/DigitalDeliveryTests.cs`; Unit `Architecture/{ConcurrencySourceScan(Tests),ControlSourceScanTests,ModelBindingRules}.cs`, `Catalog/PublishGateTests.cs`, `Concurrency/LockSequenceTests.cs`, `Validation/FieldRuleTests.cs`, `Administration/AuditEntryRulesTests.cs`, `Infrastructure/SitemapSourceTests.cs`
- Belge: `09` v0.35 (§9.2 m.3 tablo + m.4; §10.2 m.5–m.6 `ProductLocks`; K-1615), `PRODUCT_DISCOVERY_STATUS.md` K-1615
- Kayıt: `Docs/STATUS_CHANGELOG.d/T525.md`, `Docs/DEFERRED_BACKLOG.d/T525.md` (D-1396–D-1400), `IMPLEMENTATION_STATUS.md` tablo satırı, `DEFERRED_BACKLOG.md` (yerinde kapanan/ileri atılan satırlar)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Liste en yeni önce; arama taslak/arşivi bulur; fiyat aralığı ve stok sunucunun | ✓ | `ProductListPageTests`: `TheList_ShowsEveryStatusNewestFirst_WithTheSearchAndTheStatusFilter_KK1`, `TheSearch_FindsADraftAndAnArchivedProduct_ByTheSearchedFormOfTheName_KK1`, `TheStatusFilter_KeepsOneStatus_…_KK1`, `ThePriceAndTheStock_AreTheServersFigures_ByType_KK1`, `AnInvalidFilterValue_AppliesNoFilter_AndAPagePastTheLastIsTheLast_KK1`, `ThePageBorder_WithTheSameCreationMoment_NeitherRepeatsNorLosesARow_KK1`, `TwoPages_SendTheSameNumberOfCommands_…_KK1` |
| 2 | Yeni ve var olan form; `Version`, gösterilen stok, ayrılmış adet, kapı eksikleri | ✓ | `ProductFormPageTests`: `TheNewForm_IsEmpty_AndAnUnknownKeyIs404_KK2`, `TheExistingForm_ShowsTheStockTheReservedUnitsAndTheGatesUnmetConditions_KK2`, `TheReservedUnits_NameTheOrdersThatHoldThem_EachALinkToItsOrder_KK2`, `TheForms_CarryTheEntries_TheVersionAndTheShownStock_…_KK2`, `TheForm_ShowsEveryPartItWrites_AsAVisibleField_…_KK2`, `ADigitalProductsForm_CarriesTheFileFields_HookedForTheChunkedUpload_KK2` |
| 3 | Tek işlem; Taslak doğum; açıklama kapalı setle süzülür | ✓ | `ProductSavesTests`: `ANewProduct_AndItsVariant_AreBornDraft_InOneSave_WithTheFirstPriceInTheHistory_KK3`, `TheDescription_IsCleanedByTheClosedFormatSet_KK3`, `ANewVariantWithABlankStockCode_TakesTheSuggestedCode_KK3`; sayfa `SaveProduct_Saved_Is303…_KK3`, `SaveProduct_ThroughTheVisibleFields_…_KK3`, `SaveProduct_OfTheFormAsItWasShown_…_KK2_KK3`, `TheBlankNewEntries_AreNotRead_…_KK3`, `SaveProduct_ABodyAboveTheDefaultCeilingAndBelowTheLongTextOne_IsNot413_KK3` |
| 4 | Koşullu stok/kontenjan; fiyat geçmişi; ikisi ize girmez; P-6/P-9/P-44 eski-yeni ize | ✓ | `AnExistingProduct_WritesItsStockAgainstTheShownValue_AndItsNewPriceToTheHistory_AndRaisesTheVersionOnce_KK4`, `AnExistingProductsShippingPeriod_EntersTheTrailWithItsOldAndNewValue_AndAnUnchangedSettingDoesNot_KK4`, `AServicesFulfilmentPeriodAndMostPerOrder_EnterTheTrailFromEmptyToTheirValue_KK4`, `ANewProductsBirth_WithItsSettingsGiven_WritesNoTrailRow_KK4`; sayfa `SaveProduct_AChangedShownStock_Is409BeforeTheReservedQuantitys422_…_KK4_KK6`; eşanlılık (düzeltme 1) `TheFormsSave_AndAnOrderConfirmationOnTheSameVariant_BothComplete_WithoutADeadlock_KK4`, `TheFormsSave_AndAStockWriteOfTheSameProduct_TakeTheProductsRowInTurn_D362` |
| 5 | 422 sebepleri (alan, kapı, indirim, ayrılmış, stok kodu, silinmiş görsel/dosya/kategori, yarım yükleme) | ✓ | Her sebep ayrı test (`ProductSavesTests`): `APriceThatIsNoAmount_…_KK5`, `ABlankNameAndABlankStockCode_…_KK5`, `APublishedProductsSave_ThatEmptiesItsName_BreaksItsGate_KK5`, `ADiscountWhoseRoundedPriceReachesZero_IsRefusedAtTheVariant_KK5`, `AStockBelowTheReservedQuantity_…_KK5`, `AStockCodeAnotherVariantHolds_IsRefused_KK5`, `AGalleryImageWhoseRowIsGone_IsTheUploadsField_KK5`, `ACategoryDeletedMeanwhile_IsTheCategoryField_KK5`, `ACategoryDeletedAfterTheCheck_IsRefusedByTheLinksWrite_OnTheCategoryField_AndNothingIsWritten_KK5` (düzeltme 1, K1), `AFileWhoseUploadHasNotCompleted_IsTheFilesField_KK5`, `RemovingTheFileOfAPublishedDigitalVariant_IsRefused_KK5`, `TheNetQuantity_TakesThreeDecimals_FromItsOneRule_KK5`; sayfa metni `TheModulesRefusals_TakeK799sTexts_…_KK5`, `SaveProduct_TheModulesRefusals_AreTheirFieldsMessages_…_KK5`; indirim sınırı fiyat artışında `SaveProduct_APriceRiseUnderADiscount_…_Is422AtThePrice_AndWritesNothing_D488` (düzeltme 1, K2); çitler `…_D496`, `…_D246`, `…_D279` |
| 6 | 409 sürüm/gösterilen stok; ret yazmaz; sıra 404, 422, 409, ayrılmış 422 | ✓ | `SaveProduct_OnAGoneRecord_Is404_BeforeAFieldsRefusal_…_KK6`, `SaveProduct_AFieldsRefusal_ComesBeforeAChangedVersion_AndWritesNothing_KK6`, `SaveProduct_AtAChangedVersion_Is409_WritesNothing_AndYineDeKaydetSavesWithTheCurrentVersion_KK6`, `SaveProduct_AChangedShownStock_Is409BeforeTheReservedQuantitys422_…_KK4_KK6`, `SaveProduct_AnUnreadableShownStock_IsNoMatch_Is409_AndWritesNothing_KK6`; modül `AStaleVersion_IsTheConflict_AndNothingIsWritten_KK6` |
| 7 | Mutlak hedef; ilk yayın adresi; sepetten çıkarma; iz | ✓ | Sayfa `APublishMove_IsAbsolute_AndARepeatWritesNothing_KK7`, `AVariantOfAnotherProduct_IsNotFound_…_KK7_KK9`; modül (`ProductTransitionsTests`) `TheFirstPublication_FixesTheAddress_…_OnceOnly`, `AProductTakenOffTheShowcase_LeavesTheCarts_AndIsToldOnce_KK7`, `AVariantTakenOffTheShowcase_LeavesTheCarts_…_KK7`, `AMove_WritesOneTrailRowOfThePublishStatus_…_KK2`, `AMoveWhoseTrailRowCannotBeWritten_IsRolledBackWithItsCartEffect_KK2_KK7` |
| 8 | 422: kapı eksik, son Yayında varyant, dosyasız / sıfır fiyatlı varyant | ✓ | `APublishMoveRefusedByTheGate_Is422_AndNamesTheUnmetConditions_KK8`, `TheLastPublishedVariant_OfAPublishedProduct_IsKeptByTheMoveAndTheDeletion_KK8_KK9`; `ProductPublishGateTests.ADigitalVariantWithoutAFile_CannotGoPublished_UnderAPublishedProduct_…_KK4`; Unit `PublishGateTests.AVariantGoesPublished_UnderAPublishedProduct_OnlyWithAPriceAboveZero_T525_KK8` |
| 9 | Silme: onaysızda açık sipariş sayısıyla 200; onaylıda siler; son varyant 422 | ✓ | `TheDeletionWithoutTheMark_IsTheConfirmationState_AndWithItTheProductGoesAndTheListOpens_KK9`, `TheDeletionSentence_NamesTheOpenOrders_OnlyWhenThereIsOne_KK9`, `…_KK8_KK9` |
| 10 | `UploadImage` kaydetmez; P-22…P-24, dolu tek alan 422; 413 | ✓ | `UploadImage_ToTheGallery_Answers200WithTheSentEntriesAndTheNewImage_AndWritesNothingOfTheRecord_KK10`, `UploadImage_ToAVariantRow_…_KK10`, `UploadImage_OfAContentThatIsNoImage_Is422AtTheField_…_KK10`, `UploadImage_ToAFullGallery_Is422_…_KK10`, `UploadImage_ToAFilledVariantImageField_Is422_…_KK10`, `UploadImage_OnTheNewForm_Answers200_AndOnAGoneRecord_Is404_KK10`, `UploadImage_AboveTheImageBodyCeiling_Is413_KK10`; Unit `ControlSourceScanTests` `…_KK10` (yükleme işareti yalnız panelde). Düzeltme 1 (B1): testler kendi geçici dosya deposunda koşar (`ProductFormPageTests.Open()`), üretim dizinine yazmaz — Linux CI'daki 500'ün sebebi kalktı; CI sonucu PR'da |

### Öz denetim (bitiş kapısı 2)

> **Düzeltme 1:** tablonun bütün maddeleri (on kabul kriteri, KK1–KK10, `09 §25`, güvenlik, `12` satırları, backlog) düzeltilmiş ağaçta yeniden geçildi. Değişen satırlar: KK4 (eşanlılık testi), KK5 (kategori bağı reddi, D-488 testi), KK10 (testlerin dosya deposu), `09 §25` kilit satırı (`NO KEY UPDATE`), D-488 (✅). Öteki satırların kanıtı değişmedi; hedefli Integration sınıflarında hepsi yeşil.
>
> Devam 14'te on kabul kriteri, KK1–KK10, `09 §25`, güvenlik, `12`'nin ilgili satırları ve hedefi T525 olan backlog kalemleri önceki işçilerin yaptıkları dahil birleşik ağaçta yeniden geçildi. Bulunan tek eksik: `04` 9.5.6'nın ölçü birimi alanı yanındaki U7 hatırlatması (VAL-A170, K-798 (7)) — eklendi ve iki form testinde iddia edildi.

| Madde | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 arama, süzgeç, fiyat aralığı | ✓ | `ProductListPageTests` sekiz `_KK1` testi (sayfa sınırı ve komut sayısı dahil) |
| KK2 yeni ve var olan form | ✓ | `ProductFormPageTests` altı `_KK2` testi |
| KK3 tek işlem; Taslak doğum | ✓ | `ProductSavesTests` üç `_KK3`, sayfa beş `_KK3` |
| KK4 koşullu stok; fiyat geçmişi | ✓ | `AnExistingProduct_WritesItsStockAgainstTheShownValue_…_KK4`; iz üç `_KK4`; kilit `TheFormsSave_AndAStockWriteOfTheSameProduct_TakeTheProductsRowInTurn_D362`; sipariş onayıyla eşanlı `TheFormsSave_AndAnOrderConfirmationOnTheSameVariant_BothComplete_WithoutADeadlock_KK4` (düzeltme 1) |
| KK5 422 sebepleri ayrı testte | ✓ | Modülde on iki ayrı `_KK5` (düzeltme 1: `ACategoryDeletedAfterTheCheck_…_KK5`); sayfada iki `_KK5` + `_D496`, `_D246`, `_D488` |
| KK6 409 ve ret sırası | ✓ | Sayfa beş `_KK6`, modül `AStaleVersion_…_KK6` |
| KK7 mutlak, adres, sepet, iz | ✓ | Sayfa `…_KK7`; `ProductTransitionsTests` adres, sepet ve iz testleri |
| KK8 üç 422 sebebi | ✓ | Kapı `…_KK8`; son varyant `…_KK8_KK9`; dosyasız `ProductPublishGateTests …_KK4`; sıfır fiyat Unit `…_T525_KK8` |
| KK9 onay hâli; son varyant | ✓ | `TheDeletionWithoutTheMark_…_KK9`, `TheDeletionSentence_…_KK9`, `…_KK8_KK9` |
| KK10 kaydetmez; dört ret; 413 | ✓ | Yedi `UploadImage_…_KK10`; düzeltme 1: testler geçici dosya deposunda (`Open()` `FileStoreDirectories`'i ezer), yerelde üretim dizinine dosya düşmedi; Linux sonucu PR CI'ında |
| `09 §25` tek kural evi | ✓ | Net miktar `InvalidNetQuantityAttribute` (ölçek `PriceLabelRules`'tan), tutar `InvalidAmount`, kapı `PublishGate`, çitler `FirmSettingFences.Check`, ret metinleri `FieldMessageCatalog`/K-799 (`ProductSaveRefusals` yalnız eşler) |
| `09 §25` kilit tek yerde / sıra | ✓ | `ProductLocks.cs` (`FOR NO KEY UPDATE` — düzeltme 1: sipariş kaleminin `FOR KEY SHARE`'iyle çakışmaz; `LockSequence`'e bildirilir, işlem başında); `ConcurrencySourceScanTests.Scan_InTheProductLocksFile_AllowsOnlyTheNoKeyRowLockText_T525`, eşanlı `…_WithoutADeadlock_KK4`, `Scan_AFileOfTheSameNameAsProductLocksElsewhere_IsNotAnAllowedPlace_T525`; `LockSequenceTests.Check_TheProductsRow_HeadsItsTransaction_ThenVariantsByIdOrOneFile_T525`; yarış `…_D362`, `…_D401` |
| `09 §25` sayfalı liste sınırı | ✓ | `ThePageBorder_WithTheSameCreationMoment_…_KK1`, `AnInvalidFilterValue_…_AndAPagePastTheLastIsTheLast_KK1` |
| `09 §25` döngüde sorgu yok | ✓ | `TwoPages_SendTheSameNumberOfCommands_WhateverTheNumberOfVariantsOnThem_KK1` |
| `09 §8.2` m.3 ret kaydı; `09 §3.4` alan kalıbı | ✓ | Sayfanın 422/409 dallarında `RejectionLog`; alan hataları `ModelState` + `FieldCodes`; ret metinleri K-799 birebir (`…_KK5` sayfa testleri) |
| `09 §9.2` iz (K-1615) | ✓ | `09` v0.35 m.3 tablosu `Settings` satırı ve m.4; `AuditEntryRulesTests` (+); `…_KK4` iz testleri |
| Güvenlik | ✓ | Sayfalar `/Panel` yönetici politikasında (`AGuest_IsNotShownTheList`); POST'lar antiforgery'li; yabancı/bozuk anahtar 404 (`AVariantOfAnotherProduct_IsNotFound_…`, `TheNewForm_…_AnUnknownKeyIs404_KK2`); görsel türü içerikten okunur (`UploadImage_OfAContentThatIsNoImage_…`); gövde tavanı (`…_Is413_KK10`, `…_IsNot413_KK3`); açıklama kapalı biçim setiyle süzülür (`TheDescription_IsCleanedByTheClosedFormatSet_KK3`); yükleme işareti müşteri sayfasında yasak (`ControlSourceScanTests …_KK10`); ham SQL yalnız `ProductLocks` (parametreli); model bağlama kuralı (`ModelBindingRules`) |
| `12` VAL-A163, A511 kapı | ✓ | `APublishMoveRefusedByTheGate_Is422_AndNamesTheUnmetConditions_KK8`; `ProductTransitionsTests.AMoveToPublished_AsksTheGate_…_KK5` |
| `12` VAL-A164 varyant ve stok kodu | ✓ (panel payı) | `SaveProduct_ThroughTheVisibleFields_…_KK3`, `AStockCodeAnotherVariantHolds_IsRefused_KK5`; vitrinde seçenek gösterimi T50x |
| `12` VAL-A165 fiyat > 0 | ✓ | Alan kuralı `InvalidAmount` (`APriceThatIsNoAmount_…_KK5`); veritabanı `ck_variants_price_above_zero`; yayın `PublishGate.HasSalePrice` |
| `12` VAL-A166 KDV ürün düzleminde | ✓ | Form `Input.VatRate`; yeni form P-10 varsayılanı `TheForms_CarryTheEntries_…_AndTheNewFormStartsFromTheDefaultVatRate_KK2` |
| `12` VAL-A167, A168 stok, üretim yeri, birim, net miktar | ✓ | `TheForm_ShowsEveryPartItWrites_…_KK2` (`OriginCountryId`, `MeasurementUnit`, "Net miktar (gram)"); `TheNetQuantity_TakesThreeDecimals_…_KK5`; `APriceThatIsNoAmount…` |
| `12` VAL-A169 ölçü alanı yayın koşulu değil | ✓ | `PublishGate` koşul listesi ölçü alanını içermez (`PublishGateTests`); birim fiyatın gösterilmemesi vitrinde (T50x) |
| `12` VAL-A170 U7 hatırlatması | ✓ (bu tur) | `MeasurementUnitReminderText`; `TheNewForm_IsEmpty_…_KK2`, `TheForm_ShowsEveryPartItWrites_…_KK2` iddiası. Tarayıcı satırları VAL-B157/B158 doğrulamada |
| `12` VAL-A171 hizmetin ifa süresi, kontenjan | ✓ | `AServicesFulfilmentPeriodAndMostPerOrder_…_KK4`; kapı ifa süresini ister (`PublishGate`) |
| `12` VAL-A172, A173, A832 ayrılmış adet tabanı, koşullu yazım | ✓ | `AStockBelowTheReservedQuantity_IsRefusedWithTheReservedQuantity_KK5`, `SaveProduct_AChangedShownStock_Is409BeforeTheReservedQuantitys422_…_KK4_KK6`, `…_IsNoMatch_Is409_KK6` |
| `12` VAL-A174 ayrılmış adet ve siparişler | ✓ | `TheReservedUnits_NameTheOrdersThatHoldThem_EachALinkToItsOrder_KK2` |
| `12` VAL-A175, A176 tek tek ekleme; arama ve süzgeç | ✓ | `…_KK3` kayıt testleri; `…_KK1` liste testleri (toplu içe aktarma düğmesi yok) |
| `12` VAL-A177, A178 Taslak önizleme | — | Vitrin önizlemesi T50x'in; formun adres satırı yalnız bağlantı verir |
| `12` VAL-A179, A180 silme, açık sipariş sayısı | ✓ | `TheDeletionWithoutTheMark_…_KK9`, `TheDeletionSentence_NamesTheOpenOrders_OnlyWhenThereIsOne_KK9` |
| `12` VAL-A183 çok kategori, tek ana kategori | ✓ (⚠ D-1400) | `SaveProduct_ThroughTheVisibleFields_WritesTheCategory…_KK3`, `SaveProduct_OfTheFormAsItWasShown_KeepsTheCategories…_KK2_KK3`; birden çok işaretlenirse ilki ana (D-1400) |
| `12` VAL-A181, A182, A184, A185 kategori ağacı | — | T526/T333 (bu task'ın ekranı değil) |
| `12` VAL-A186–A189 görsel, sıra, ana görsel, alternatif metin | ✓ (panel payı) | `UploadImage_…_KK10`; `AMainImageMovedByTheGallerysBulkMark_RaisesTheVersion_…_D361`; form `Images[].SortOrder/IsMain/AltText`; boş alternatif metnin düşüşü vitrinde (T50x) |
| `12` VAL-A190 kapalı biçim seti | ✓ | `TheDescription_IsCleanedByTheClosedFormatSet_KK3` |
| `12` VAL-A191–A193, A514 indirim, %100, sıfır, referans | ✓ | Form indirim bölümü (`TheForm_ShowsEveryPartItWrites_…AndTheDiscountsPricesAsTheServerComputedThem_KK2`); `ADiscountWhoseRoundedPriceReachesZero_…_KK5`; `TheModulesRefusals_TakeK799sTexts_…_KK5` (`InvalidPercentage`, `DiscountedPriceNotBelowReference`, D-487); fiyat artışında referans `SaveProduct_APriceRiseUnderADiscount_…_D488` (düzeltme 1) |
| `12` VAL-A194, A195 referans fiyat hesabı | — | T337 modül kuralı; form yalnız sunucunun hesabını gösterir |
| `12` VAL-A196–A198 kupon | — | T526 |
| `12` VAL-A199, A200 dijital dosya bağlama/güncelleme | ✓ (panel payı, ⚠ D-1399) | `ADigitalProductsForm_CarriesTheFileFields_HookedForTheChunkedUpload_KK2`; `DigitalDeliveryTests.AFileTheFormBinds_…_D527` (iki); `FileUpdateText`; parçalı yükleme betiği T548 |
| `12` VAL-A201 indirme hakkı yenileme | — | Sipariş ekranı (T527) |
| `12` VAL-A202 cayma istisnası kapalı liste | ✓ | Form `Input.WithdrawalExemptionReason` (`WithdrawalExemptionReasonNames`); `SaveProduct_ThroughTheVisibleFields_…AndTheExemption_…_KK3`; `…_D246` |
| `12` VAL-A203–A206 istisnanın vitrin/sipariş etkisi | — | Vitrin ve sipariş ekranları (öteki task'lar) |
| `12` VAL-A465, A466 `InvalidImage`, `ImageLimitExceeded` | ✓ | `UploadImage_OfAContentThatIsNoImage_…_KK10`, `UploadImage_ToAFullGallery_…_KK10`, `UploadImage_ToAFilledVariantImageField_…_KK10` (piksel tavanı ve çözülemeyen görsel modülün var olan testlerinde) |
| `12` VAL-A508 `RecordVersionConflict` | ✓ | `SaveProduct_AtAChangedVersion_Is409_WritesNothing_AndYineDeKaydetSavesWithTheCurrentVersion_KK6` |
| `12` VAL-A513 `LastPublishedVariant` | ✓ | `TheLastPublishedVariant_OfAPublishedProduct_IsKeptByTheMoveAndTheDeletion_KK8_KK9` |
| `12` VAL-A554 "Kaydedildi" | ✓ (ürün payı) | `SaveProduct_Saved_Is303ToTheProductsForm_AndWritesTheEntries_KK3` (303 `notice=Saved`) |
| `12` VAL-A592, A701 silinmiş kategori / yükleme | ✓ | `ACategoryDeletedMeanwhile_IsTheCategoryField_KK5`, `ACategoryDeletedAfterTheCheck_…_KK5` (düzeltme 1), `AGalleryImageWhoseRowIsGone_IsTheUploadsField_KK5`, `AFileWhoseUploadHasNotCompleted_IsTheFilesField_KK5` |
| `12` VAL-A717 geçiş/silme sürümsüz, o anki hâlle | ✓ | `APublishMove_IsAbsolute_AndARepeatWritesNothing_KK7`, `…_KK8_KK9`; `AFileRemovalAndAPublishOfTheSameProduct_…_D401` |
| `12` VAL-A723 stok kodu önerisi / zorunluluk | ✓ (⚠ D-1398) | `ANewVariantWithABlankStockCode_TakesTheSuggestedCode_KK3`, `ABlankNameAndABlankStockCode_BothTakeTheRequiredMessage_KK5`; öneri kayıtta yazılır, formda alan içinde görünmez (D-1398) |
| `12` VAL-A724 betiksiz yolda boş girdiler | ✓ | `TheBlankNewEntries_AreNotRead_AndAnEmptyDiscountIsNone_KK3`, `SaveProduct_ThroughTheVisibleFields_…AndTheBlankNewEntriesAreNotRead_KK3` |
| `12` VAL-B043, B044, B102 [tarayıcı] | — | Tarayıcı satırları; yapımda tarayıcı koşulmadı, stiller `admin.css`'te (doğrulamada tarayıcıyla) |

### Hedefi T525 olan backlog kalemleri

| Kalem | Sonuç | Kanıt / gerekçe |
|---|---|---|
| D-125, D-164, D-249, D-335, D-344, D-346, D-358, D-475, D-689 | ✅ yerinde kapandı | `DEFERRED_BACKLOG.md` satırlarında T525 kanıtı (form alanları, iz K-1615, liste/form okuyucuları) |
| D-246, D-279, D-496 | ✅ | `…_D246`, `…_D279`, `…_D279_D496`, `SaveProduct_AShippingPeriodAboveItsFence_…_D496` |
| D-361 | ✅ | `AMainImageMovedByTheGallerysBulkMark_RaisesTheVersion_…_D361` |
| D-362, D-381, D-401 | ✅ | `ProductLocks` (`FOR NO KEY UPDATE`, düzeltme 1); `…_D362`, `…_D401`, `…_WithoutADeadlock_KK4`; `LockSequenceTests …_T525` |
| D-487 | ✅ | `ProductSaveRefusals.TextOf`; `TheModulesRefusals_TakeK799sTexts_…_KK5` |
| D-527 | ✅ | `DigitalDeliveryTests.AFileTheFormBinds_ThenTheProductDeleted_…_D527`, `…_ThenRemovedWithoutANewOne_…_D527` |
| D-488 | ✅ yerinde kapandı (düzeltme 1) | Form fiyat değişikliğinden sonra indirimi yeniden kaydeder, `02 §3.9.5` denetimi fiyat artışında da koşar; fiyatı değiştiren tek uygulama yolu form. `SaveProduct_APriceRiseUnderADiscount_…_D488`; ileri kaydı kaldırıldı |
| D-283 | ⚠ F5 kapısına yeniden ileri | Form seçenek silmez; silme kuralı ayrı karar |
| D-334 | — (hedef T335) | T525 payı formun kategori alanı — karşılandı; tek seçim D-1400 |
| D-981 | — (hedef F5 gate) | T525 anılıyor ama sipariş ekranı payı; ürün formunda karşılığı yok |
| D-148 | — | Zaten ✅ |
| D-1396–D-1400 | ⚪ yeni | `DEFERRED_BACKLOG.d/T525.md` |

### Sık FAIL nedenleri

| # | Neden | Sonuç |
|---|---|---|
| 1 | Bekçi/tarama istisnası geniş | ✓ `FOR NO KEY UPDATE` izni yalnız `Catalog/ProductLocks.cs` tam yoluna ve yalnız o biçime (düz `FOR UPDATE` orada bulgu); aynı adlı başka yer ayrı ihlal testinde (iki biçim de bulgu) |
| 6 | Test düzeneği üretim dosya deposunu ezmiyor (T525 r1) | ✓ düzeltme 1: `ProductFormPageTests` kendi geçici deposu; yükleme yapan öteki T525 test sınıfı yok (`ProductSavesTests`/`DigitalDeliveryTests` dosya yazmıyor — koşudan sonra üretim dizininde yeni dosya yok) |
| 7 | Yeni satır kilidi düz FOR UPDATE, FK ile ölü kilit (T525 r1) | ✓ düzeltme 1: `NO KEY UPDATE`; eski kiple kırmızı olan eşanlı test |
| 8 | Hedefi bu task olan backlog kaydı ne kapandı ne ileri atıldı | ✓ D-488 kapandı; D-283 F5 kapısına ileri (gerekçeli) |
| 2 | Kaynak taraması çağrı biçimi kaçırıyor | ✓ Yükleme işareti taraması (`ControlSourceScanTests`) işaretin bütün biçimlerini ve yakın-ıskalamayı ayrı örnekle sınar |
| 3 | 06 "Varsayılan" / FK dizini | ✓ Şema değişmedi |
| 4 | Aynı sayıyı anan öteki yerler | ✓ `09 §9.2` m.3 toplamı (43) değişmedi (yeni kod yok, konu eklendi); `§10.2` m.5–m.6 `ProductLocks`'u sayar |
| 5 | 09 metni karar kaydısız | ✓ `09` v0.35 değişikliği K-1615'e dayanır (numara main'de çakışmıyor) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build (düzeltme 1) | ✓ | `dotnet restore Shopfolio.slnx`; `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Format (düzeltme 1) | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 | Web, UnitTests, IntegrationTests rc=0 |
| Unit TAM (düzeltme 1) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 7609/7609 |
| Integration (düzeltme 1) | ✓ hedefli — **TAM: CI** | `--filter-class` ProductSavesTests, ProductFormPageTests, ProductTransitionsTests, DigitalDeliveryTests, ProductPublishGateTests, ProductListPageTests (166/166); ConcurrencyTests, OrderConfirmationTests, StockAndCapacityTests, DiscountAndReferenceTests, PriceAndVatTests, CategoryTreeAndListsTests, ContentRelationsTests (197/197) | 13 sınıf 363/363 (Postgres 18, `track_commit_timestamp=on`). Eşanlı test eski `FOR UPDATE` kipiyle `40P01` ile kırmızı (regresyon kanıtı), düzeltmeyle yeşil |
| Unit TAM | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 7599/7599 (devam 13, birleştirmeden önce); birleşik ağaçta (3fe7d829 sonrası) Architecture + Consistency 2065/2065 |
| Integration | ✓ hedefli — TAM: CI | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build -- --filter-class <ad>` | Birleşik ağaçta ProductSavesTests 24/24, ProductFormPageTests 33/33 (U7 eklendikten sonra); önceki turlarda ProductListPageTests + StockAndCapacityTests 37, ProductTransitionsTests, ConcurrencyTests, DigitalDeliveryTests, ProductPublishGateTests yeşil. Tam koşuyu CI'ın Integration işi yapar |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` | Web, UnitTests, IntegrationTests temiz (devam 13); bu turun iki dosyası `--include` ile temiz |

## Doğrulama

**Tur 1 — 2026-11-14 — ✗ FAIL** (head `e85731b0` + `origin/main` `8a51cdcc` birleştirmesi; yalnız `DEFERRED_BACKLOG.md` kayıt çakışması: D-256 main, D-279 dal hâli)

| Alan | Değer |
|---|---|
| Durum | ✗ FAIL — 2 bulgu (ORTA ×2), 2 K-notu; düzeltme gerekli |
| Başlangıç kapıları | Working tree temiz; `core.hooksPath` = `scripts/git-hooks`; main son 3 run `success` (43557742117, 43557742023, 43552777848) |
| Yerel sınama | Build `-warnaserror` 0/0; Unit 7609/7609; `dotnet format` üç proje temiz; betik testleri 4/4; Integration hedefli 16 sınıf 382/382 (ProductSaves, ProductFormPage, ProductListPage, ProductTransitions, ProductPublishGate, DigitalDelivery, ProductGallery, DiscountAndReference, PriceAndVat, PageAddress, StockAndCapacity, Concurrency, AuditTrail, ContentRelations, CategoryTreeAndLists, ConfirmationStep) — TAM: CI; `12_matris_denetle.py --kapsam 1-77` hata 0 |
| PR CI (#570, run 43558444807) | ✗ **4. Integration test kırmızı** (3 test) → CI Gate kırmızı; öteki işler yeşil |

**Bulgular**

| # | Madde | Severity | Sınıf | Beklenen ↔ gerçekleşen | Karar | Dosya |
|---|---|---|---|---|---|---|
| B1 | KK10 (`VAL-A186`) · CI | ORTA | S1 | Görsel yükleme testleri her ortamda 200 vermeli ↔ CI'da (Linux) `UploadImage_ToTheGallery_…_KK10`, `UploadImage_ToAVariantRow_…_KK10`, `UploadImage_OnTheNewForm_…_KK10` 500 alıyor. Sebep: `ProductFormPageTests.Open()` dosya deposunun dizinini ezmiyor, görsel `FileStoreDirectories.Default` (`/var/lib/shopfolio/images`) altına yazılmaya çalışılıyor; Linux çalıştırıcısında yazma izni yok (Windows'ta `C:\var\lib\shopfolio\images` açılıp geçiyor — yerelde yeşil, makineyi de kirletiyor). | Düzeltme turu: `Open()` `FileStoreDirectories`'i geçici dizine çevirsin (`AccountAndStatusCommandTests`'in kalıbı); CI yeşil | `tests/Shopfolio.IntegrationTests/Web/ProductFormPageTests.cs` |
| B2 | KK3/KK4 · D-401, D-362, D-381 · `05 §7.3.5`, `09 §10.2` m.5 | ORTA | S2 | Kilit sırası bütün işlemlerde aynı, ölü kilit yok ↔ `ProductLocks` düz `FOR UPDATE` alıyor; sipariş onayı önce varyantı ayırır (`StockCounters` `ExecuteUpdate`), sonra işlem sonunda `order_items` ekler — `fk_order_items_products_product_id` ürün satırına `FOR KEY SHARE` ister ve düz `FOR UPDATE` bunu bekletir. Ürün formu kaydı (ürün kilidi → stok koşullu yazımı), varyant yayın geçişi ve varyant silme aynı varyantta onayla çakışınca döngü kurulur. Postgres 18'de aynı sıra ile yeniden üretildi: `ERROR: deadlock detected … while updating tuple in relation "variants"`; `FOR NO KEY UPDATE` ile iki işlem de kesinleşti. Sonuç: müşterinin sipariş onayı ya da panel kaydı beklenmedik hatayla düşer (veri bozulmaz). T525 öncesi ürün satırında `FOR UPDATE` yoktu (regresyon, T348 onay yolu). | Düzeltme turu: kilit `FOR NO KEY UPDATE` (ya da eşdeğer çözüm) + `09 §10.2` m.5–m.6 ve madde 6 tarama metni + K-1615 (2) güncellemesi (karar satırı) + onay ile form kaydının eşanlı testi | `src/Shopfolio.Web/Modules/Catalog/ProductLocks.cs`, `Docs/09_CODING_GUIDELINES.md` |

**K-notları**

| # | Not | Karar |
|---|---|---|
| K1 | `ProductSaves.SaveAsync` `_links.ApplyAsync` sonucunu atıyor (`(ProductCategoriesResult _, bool _)`); `CheckCategoriesAsync` ile uygulama arasında (aynı işlem, READ COMMITTED) seçilen boş kategori silinirse bağ yazılmaz ama kayıt başarılı döner — `07` UC-114 bunu kategori alanında 422 ister. | Bu PR'da kapatılır (düzeltme turu: ret sonucu `Categories` alan hatası) |
| K2 | D-488'in ileri kaydı "T525 yeni ürün kuralı yazmaz" diyor; oysa form her kayıtta indirimi `Discounts.SaveAsync` ile yeniden kaydettiği için `02 §3.9.5` referans denetimi fiyat artışında da koşuyor (indirim varken indirimli fiyatı referansa ya da üstüne çıkaran fiyat reddedilir). Davranış belgeye uygun; kayıt ve test eksik. | Bu PR'da kapatılır (düzeltme turu: D-488 metni gerçek davranışla güncellenir, form testi eklenir) |

**Kabul kriterleri (doğrulayıcı kanıtı):** KK1 ✓ (`ProductListPageTests` 10 test; `PanelProductListReader` fiyat aralığı ve stok hâli sunucuda) · KK2 ✓ (`TheNewForm_…_KK2`, `TheExistingForm_…_KK2`, `TheReservedUnits_…_KK2`) · KK3 ✓ (`ANewProduct_…_KK3`, `TheDescription_…_KK3`, `SaveProduct_Saved_…_KK3`) · KK4 ✓ işlevsel, ~ eşanlılık (B2) · KK5 ✓ (`ProductSavesTests` `_KK5` on bir test, `SaveProduct_TheModulesRefusals_…_KK5`) · KK6 ✓ (`SaveProduct_OnAGoneRecord_…`, `…AFieldsRefusal_ComesBeforeAChangedVersion…`, `…AtAChangedVersion_Is409…`, `…AChangedShownStock_Is409BeforeTheReservedQuantitys422…`) · KK7 ✓ (`APublishMove_IsAbsolute_…_KK7`; `ProductTransitionsTests`) · KK8 ✓ (`APublishMoveRefusedByTheGate_…_KK8`, `TheLastPublishedVariant_…_KK8_KK9`, `PublishGateTests …_KK8`) · KK9 ✓ (`TheDeletionWithoutTheMark_…_KK9`) · KK10 ✗ CI (B1).

**`12` satırları:** FAIL turunda `Durum` hücreleri değişmedi (KP-39, 40, 42–45 satırları `BEKLEMEDE`; A196–A198 T526'dan `PASS`). Sonraki turda yapım raporundaki ek satırlar (A183, A465, A466, A723, B157, B158) de eşlenir.

**Backlog:** hedefi T525 olan 20 kalemden 18'i kapatılmış (kanıtlar test adlarıyla eşleşiyor; D-401/D-362/D-381'in kapanışı B2 düzeltmesiyle birlikte geçerli), D-283 ve D-488 F5 kapısına ileri (K-1615 ile çelişki yok; D-488 için K2). Yeni kalem açılmadı.

**Yapım raporu karşılaştırması:** 2 uyuşmazlık — öz denetim KK10'u "yedi test ✓" sayıyor ama PR CI'ında üçü kırmızı (B1); `09 §25` "kilit sırası ✓" satırı sipariş onayının yabancı anahtar kilidini hesaba katmıyor (B2).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T525-urun-listesi-formu`
- **PR:** PR açıklamasında
- **CI:** *(PR CI'ı)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

## ⚠ Maddeler

- K-1615 (sahip yerine): P-6/P-9/P-44 değişikliği `UpdateSettings` + konu `Product` ile ize; yeni ürün doğumunda iz yok; ürün satırı kilidi `ProductLocks` (`09` v0.35 §9.2, §10.2).
- D-283 F5 kapısına ileri (ürün kuralı kararı ister). D-488 düzeltme 1'de kapandı (form davranışı + test).
- Düzeltme 1: K-1615 (2) metni yerinde güncellendi (kip `FOR NO KEY UPDATE` ve gerekçesi); yeni karar numarası açılmadı. `CouponLocks` da düz `FOR UPDATE` alır (T526); kupon satırına yabancı anahtar veren sipariş yolu varsa aynı döngü orada da kurulabilir — bu task'ın kapsamı dışında, incelenmedi.
- Kilit yarış testleri `pg_stat_activity` yoklamasıyla bekler (25 ms × 400; emsal `MembershipSignInLockTests`).
- Önceki bir turda kırılan KK5 testi devam 12'de düzeltildi; devam 13'te `OrderItemFiles.FilePlanes` `internal`e çekildi (modül sınırı) ve iki testte açık iddia eklendi.
- Yayındaki üründe sıfır fiyatlı varyantın kapı koşulu `05`/`07` listesinde adsız; ret `PublishGateIncomplete` (D-1396).
- Net miktar kuralı `InvalidNetQuantityAttribute` belgede yok (D-1397).
- Önerilen stok kodu formda alanın içinde görünmez, kayıtta yazılır (D-1398).
- Dijital dosyanın parçalı yükleme betiği T548'de; form yalnız kancayı taşır (D-1399).
- "Ana kategori" tek seçimli değil; birden çoğu işaretlenirse ilki ana kategori (D-1400). Kategori ağacı formda tek liste.
- Okunamayan gösterilen stok değeri eşleşmez sayılır → 409.
- `ProductSaveRefusals`'ta K-799 kataloğunda birebir olmayan metinler (görsel/piksel reddi, net miktar) alan kuralının metnini taşır.
- K-799 422 siparişlerinde ayrılmış adet "Ayrılmış" hücresinde gösterilir.
- Bu tur: öz denetimde eksik bulunan U7 ölçü birimi hatırlatması eklendi (VAL-A170; B157/B158 tarayıcı satırları doğrulamada).
- `origin/main` birleştirmesinde `DEFERRED_BACKLOG.md` çakışması elle çözüldü (D-487/D-488 T525, D-428/D-429 T517 hâli).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sıfır fiyat kapı koşulunun adı | D-1396 | Kapalı koşul listesi belge turunda |
| 2 | Net miktar kuralı belgeye | D-1397 | Karar kaydıyla belge turunda |
| 3 | Stok kodu önerisi alanın içinde | D-1398 | Betiksiz yolda gidiş-dönüş gerekir |
| 4 | Parçalı yükleme betiği | D-1399 | T548 |
| 5 | Ana kategori tek seçim | D-1400 | Girdinin şekli değişmeden kurulamaz |
| 6 | Seçenek silme | D-283 | F5 kapısı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Yapım on dört turda bölündü (süre);** devam 14 main birleştirmesi, birleşik ağaçta derleme ve hedefli testler, U7 eksiği, rapor ve PR.
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum"a dokunulmadı (faz kapısı kayıt PR'ı).
