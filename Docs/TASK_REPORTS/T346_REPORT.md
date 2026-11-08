# T346 — Sepet: sahiplik, giriş anında birleşme, canlı kalem, siparişe girmeyen kalemler

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Sepet işlemleri `IOrderingModule`'e eklendi: `OpenCartAsync` (sayfanın canlı okuması), `AddToCartAsync`, `MergeGuestCartAsync`. Üçü de çağıranın işlemine katılır, sepet kilidini en başta alır ve kaydetmez (`09 §10.2` m.5, `09 §6` m.3).
- `CartOwner` (üye: hesap kimliği; misafir: tarayıcı çerezindeki ham anahtar, yalnız özeti saklanır, `ToString` anahtarı yazmaz), `CartLine`, `CartView`, `CartAddResult`, `CartMergeResult`, `CartAddOutcome`, `CartPaymentBlock` sözleşmeleri.
- `CartRules` (saf kurallar): `PriceChanged` (yönsüz tek işaret), `EntersOrder`, `PaymentBlockOf`, `OrderableSubtotal`, `MergeShowsNew`.
- `Carts` (işlemler) ve `CartStatements` (eşzamanlı yazılan satırlar için tek `ON CONFLICT` deyimleri): sepet bir kez yapılır, aynı varyant ikinci kez eklenince yalnız adet artar (ilk fiyat ve an kalır), birleşmede büyük adet kalır, önce eklenen satır fiyatı ve anı belirler.
- Katalog: `ICatalogModule.ListCartVariantFactsAsync` + `CartVariantReader` (güncel fiyat, indirim dahil; vitrinde mi; Tükendi mi; stok sayısı taşımaz).
- Görülmeyen ekleme `IsMergeNoticePending` ile işaretlenir, `OpenCartAsync`'te bir kez söylenir. Vitrinden kalkan kalem `OpenCartAsync`'te bir kez söylenip silinir (`TakeRemovedNamesAsync`, T321'in `TakeRemovedItemNoticesAsync`'i buna devredildi).
- Bu turda: test projeleri derlendi, 16 Unit + 19 Integration test yazıldı; `CartAddOutcome` ve `CartPaymentBlock` kendi dosyalarına alındı ve `ApplicationEnums.NotStored`'a eklendi (saklanmayan sonuç türü); ham SQL `CartStatements.cs`'e toplandı ve `VersionedSideBulkWriteScan` kapalı yerler listesine (test dosyayı dört deyim, yalnız `carts`/`cart_items` ve `UPDATE`/`DELETE` yok olarak bağlar) alındı; belgeler.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/`: `Carts.cs`, `CartStatements.cs`, `CartRules.cs`, `OrderingModule.cs`; `Contracts/`: `CartContracts.cs`, `CartAddOutcome.cs`, `CartPaymentBlock.cs`, `IOrderingModule.cs`.
- `src/Shopfolio.Web/Modules/Catalog/`: `CartVariantReader.cs`, `CatalogModule.cs`; `Contracts/`: `CartVariantFact.cs`, `ICatalogModule.cs`.
- `tests/Shopfolio.UnitTests/`: `Ordering/CartRulesTests.cs`, `Architecture/VersionedSideBulkWriteScan(.Tests).cs`, `Support/ApplicationEnums.cs`, `Infrastructure/SitemapSourceTests.cs`.
- `tests/Shopfolio.IntegrationTests/Ordering/CartTests.cs`.
- Kayıt: `Docs/STATUS_CHANGELOG.d/T346.md`, `Docs/DEFERRED_BACKLOG.d/T346.md` (D-558–D-562), `Docs/DEFERRED_BACKLOG.md` (D-303 yerinde kapandı), `Docs/IMPLEMENTATION_STATUS.md` (tablo satırı).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üyenin sepeti hesapta, her cihazda aynı; misafirin sepeti tarayıcıya bağlı, üye çıkınca o tarayıcıda boş | ✓ | `CartTests.AMembersCart_IsTheSameInEveryBrowser_AndTheGuestKeyIsEmptyAfterTheSignIn_KK1`, `TwoGuestBrowsers_HaveTheirOwnCarts_AndAMemberAddsToTheAccountCart_KK1`; `Carts.FindCartIdAsync` |
| 2 | Sepet kendiliğinden boşalmaz, süre parametresi yok; yarım ödeme, kesinti, kapalı satışta durur | ✓ | `CartTests.ACartThatWasLeftForTwoYears_AndAnOpenOrderOfTheCart_AreNotEmptied_KK2`, `ASoldOutLine_AndALineOfAProductTakenOffTheShowcase_DoNotEmptyTheCartOnTheirOwn_KK2`; `CartTimerScanTests` (mevcut, zamanlayıcı yok); `Carts` hiçbir yerde sepeti boşaltmaz |
| 3 | Girişte iki sepet birleşir; aynı varyantta büyük adet, farklı varyant ayrı kalem; giriş yolundan bağımsız | ✓ | `CartTests.TheMerge_KeepsTheLargerQuantity_*_KK3`, `TheMerge_IsOneMethod_ForEveryWayIn_*_KK3`, `TwoMergesAtOnce_*_KK3`, `TwoAddsAtOnce_*_KK3`; tek yöntem `IOrderingModule.MergeGuestCartAsync` |
| 4 | Görülmeyen ürün/adet bir kez ve adıyla söylenir; eklenmediyse mesaj yok | ✓ | `CartTests.TheMerge_TellsOnceWhatTheGuestCartDidNotShow_ByName_KK4`, `TheMerge_TellsNothing_WhenTheMemberCartAddedNothingTheGuestDidNotSee_KK4`; `CartRulesTests.MergeShowsNew_*_KK4` |
| 5 | Güncel fiyat ve alınabilirlik; ilk fiyattan farklıysa tek metin, yönsüz; adet artışı referansı değiştirmez; birleşmede önce eklenen belirler | ✓ | `CartTests.ThePriceMark_IsOneFlag_WithoutDirection_AndGoesWhenThePriceReturns_KK5`, `AQuantityIncrease_DoesNotMoveTheReferencePrice_KK5`, `TheMerge_TakesTheReferencePriceOfTheLineAddedFirst_KK5` (iki yön); `CartRulesTests.PriceChanged_*` |
| 6 | Tükendi kalem sepette kalır, siparişe girmez; stok dönünce alınır; özet gösterilmeden düşen kalem özete girmez | ✓ | `CartTests.ASoldOutLine_StaysInTheCart_DoesNotEnterTheOrder_AndEntersAgainWhenTheStockReturns_KK6`, `ASoldOutVariant_CannotBeAdded_*_KK6`; `CartRulesTests.EntersOrder_*_KK6` |
| 7 | Vitrinden kalkan kalem çıkar ve bir kez söylenir; siparişe girecek kalem yoksa ödeme açılmaz, sebep sepette; (onayda boşalma T348); girmeyen kalem tutara girmez | ✓ | `CartTests.AnItemTakenOffTheShowcase_LeavesTheCart_AndIsToldOnce_KK7`, `WithoutAnOrderableLine_ThePaymentStepDoesNotOpen_AndTheReasonIsInTheCart_KK7`; `CartRulesTests.PaymentBlock_*_KK7`, `OrderableSubtotal_*_KK7`. Onayda boşalma T348 KK2; kupon asgarisi, kargo ve eşik T349 (D-562) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–7 | ✓ | yukarıdaki tablo; önceki işçinin kodu (`Carts.cs`, `CartRules.cs`, `CartVariantReader.cs`) diff'te satır satır okundu, testlerle çalıştırıldı |
| KK1 üye iki tarayıcı / çıkışta boş | ✓ | `CartTests` KK1 üç test (üçüncüsü: kaydedilmeyen işlemde birleşme geri alınır) |
| KK2 süre, yarım ödeme, sağlayıcı kesintisi, kapalı satış | ✓ | `CartTests` KK2 iki test: iki yıllık misafir sepeti ve sepete bağlı açık sipariş durur. Sağlayıcı kesintisi ve satış kapısı sepet modülünün okumadığı durumlardır (hiçbir sepet yöntemi ödeme ya da ayar okumaz: `Carts.cs` yalnız `ICatalogModule`'e bakar); sayfadaki gösterimi D-559 (T508) |
| KK3 büyük adet, giriş yolu fark etmez | ✓ | `CartTests` KK3 dört test (iki eşzamanlı birleşme ve iki eşzamanlı ekleme dahil) |
| KK4 bir kez, eklenmediyse yok | ✓ | `CartTests` KK4 iki test |
| KK5 yönsüz tek işaret, referans, önce eklenen | ✓ | `CartTests` KK5 dört test |
| KK6 Tükendi siparişe girmez, stok dönünce girer | ✓ | `CartTests` KK6 iki test |
| KK7 kalkan kalem çıkar, boş sepette ödeme açılmaz; Tükendi tutara girmez | ✓ | `CartTests` KK7 iki test; `CartRulesTests` |
| `09 §25` ilgili satırlar | ✓ | aşağıdaki tablo |
| Sık FAIL (1) bekçi istisnası geniş | ✓ | kapalı yer yalnız `CartStatements.cs` tam yolu; `TheCartStatementsPlace_*` dört deyim, iki tablo, `UPDATE`/`DELETE`/`FROM` yok olarak sabitler ve aynı metnin başka yolda sessiz kaldığını sınar |
| Sık FAIL (2) tarama çağrı biçimini kaçırıyor | ✓ | tüm ham deyimler tek dosyada (`CartStatements.cs`); `Carts.cs` ve `CartVariantReader.cs`'te `ExecuteSql*`/`ExecuteUpdate` yok; Unit'te bütün taramalar yeşil |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok; `carts`/`cart_items` mevcut (T-önceki), `IsMergeNoticePending` mevcut DB default'lu |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `ApplicationEnums.NotStored`, `VersionedSideBulkWriteScan.AllowedPlaces` + kapsamı sabitleyen yeni test; `AllowedPlaces` sayısını anan belge yok (grep) |
| Sık FAIL (5) belge kuralını değiştiren yeni 09 metni | ✓ | 09/05/06/02 metni değişmedi |
| Eşzamanlılık: kilit sırası, deadlock | ✓ | birleşme yalnız misafir sepet kilidini alır (tek kilit, `05 §7.3.5`); üye tarafı koşullu deyimlerle yazılır; `TwoMergesAtOnce_*`, `TwoAddsAtOnce_*` istisnasız |
| Modül yöntemi kendi işlemini açmıyor / kaydetmiyor | ✓ | `Carts` yalnız çağıranın işlemine katılır; `TheCartMethods_DoNotSaveOnTheirOwn_*` |
| Testte gerçek saate bağlılık yok | ✓ | testler bekleme/Stopwatch kullanmaz; sınırlar veri ile kurulur (iki yıllık erişim anı) |

`09 §25` ilgili satırlar:

| Satır | Sonuç | Kanıt |
|---|---|---|
| Veri: göç, FK, kilit dosyası | ✓ | şema değişikliği yok; yeni enum saklanmıyor (`NotStored`, kilit dosyası yok) |
| Eşzamanlılık | ✓ | yukarıda |
| Modül sınırı | ✓ | Ordering Catalog'a yalnız `ICatalogModule.ListCartVariantFactsAsync` ile bakar; Catalog tablosuna Ordering'den sorgu yok |
| Kayıt / gizli veri | ✓ | `CartOwner.ToString` anahtarı yazmaz; veritabanında yalnız `SecretKey.Hash` özeti (`CartOwner_DoesNotPrintTheGuestKey_KK1`) |
| Tek işlev | ✓ | sepet işlemleri `Carts`'ta; birleşme tek yöntem |

Güvenlik taraması: kullanıcı girdisi ham SQL'e yalnız parametre olarak (`ExecuteSqlAsync` interpolasyonu) girer; misafir anahtarı ham saklanmaz; başka müşterinin sepetine erişim yolu yoktur (sahip her çağrıda parametredir; sayfa katmanı sahibi oturumdan kurar, D-559).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 4716 geçti, 0 kaldı (16 yeni `CartRulesTests`, 1 yeni tarama testi) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests/... --filter-class *Ordering.CartTests --filter-class *ProductTransitionsTests` (PostgreSQL 18, docker) | 60 geçti (19 yeni `CartTests` + 41 `ProductTransitionsTests`) |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` (Web, UnitTests, IntegrationTests) | değişiklik yok |
| Contract | — | — | bu task'ta sözleşme testi yok |

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

- **Branch:** `task/T346-sepet-sahiplik`
- **PR:** başlıkta ve STATUS satırında
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Giriş yolları birleştirmeyi ayrı işlemde çağırır | D-558 | giriş sayfası task'ı (T515) |
| 2 | Sepet sayfası `OpenCartAsync` ve satış kapısı okuması | D-559 | T508 |
| 3 | Stok, sipariş başına sınır, dijital adet, `int` taşması koruması | D-560 | T347 |
| 4 | L-6 sayaç taşıması birleşmede | D-561 | T347 |
| 5 | `OrderableSubtotal` kupon asgarisi, kargo ve eşik okuması | D-562 | T349 |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** yönetici izler
- **Devir:** ilk işçi disk dolduğu için testlere geçemeden durdu (`e5176a9a`); devam işçisi `task/T346-sepet-sahiplik-devam` yerel dalından `task/T346-sepet-sahiplik`'e push etti.
- **Diğer:** ⚠ yok; kolay kararlar: saklanmayan enum'lar `NotStored`'a, ham SQL tek kapalı yere (D-361 / `GalleryMainMark` kalıbı).
