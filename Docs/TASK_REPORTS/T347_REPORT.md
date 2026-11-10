# T347 — Sepet adet kuralları: stok ve sipariş başına sınır, boşalma, istek listesi yokluğu

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder. Yapım iki işçide yürüdü: ilk işçi kodu ve testleri yazıp bağlam sınırında durdu, devam işçisi tam test paketlerini, biçimi, kayıtları ve raporu bitirdi.

---

## Yapılan İşler

- Katalog: `CartQuantityRules` (saf kural), `CartQuantityReader` (stok, ayrılmış adet, ürün ve "bir siparişte en fazla" okuması), `ICatalogModule.JudgeCartQuantitiesAsync` / `JudgeCartAddAsync`, `CartQuantityProblem` (`None`, `NoStock`, `OverOrderLimit`; saklanmayan tür).
- Sepet: `Carts.AddAsync` her eklemeyi aynı kuraldan geçirir (`NoStock`, `OverOrderLimit`; sınır stoktan önce söylenir; ilk eklemede reddedilen ekleme boş sepet bırakmaz; `int` taşması `NoStock`); `Carts.OpenAsync` her kalemi (birleşenler dahil) aynı kuraldan geçirir, adedi hiç düşürmez; `CartLine.QuantityProblem` / `OrderLimit`, `CartRules.EntersOrder` sorunlu kalemi siparişe sokmaz.
- D-589 kod düzeltmesi: ekleme kilidi aldıktan sonra misafir sepeti birleşmeyle silinmişse tarayıcının yeni misafir sepetini açar.
- Ödeme başarısı: `OrderTransitions.CompleteApprovalAsync` -> `CartStatements.DeleteOrderedItemsAsync` (tek koşullu `DELETE`, kilitsiz): siparişin varyantları sepetten bütün adetleriyle çıkar, başka ürün kalır; silinmiş (birleşmiş) misafir sepeti hesabın sepetine dokunmaz.
- İstek listesi, favori ve sepet hatırlatma yokluğu kaynak taramasıyla sabitlendi (`WishlistAbsenceScanTests`).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Catalog/`: `CartQuantityRules.cs`, `CartQuantityReader.cs`, `CatalogModule.cs`; `Contracts/`: `CartQuantityContracts.cs`, `CartQuantityProblem.cs`, `ICatalogModule.cs`.
- `src/Shopfolio.Web/Modules/Ordering/`: `Carts.cs`, `CartRules.cs`, `CartStatements.cs`, `OrderTransitions.cs`, `OrderTransitions.Payment.cs`; `Contracts/`: `CartContracts.cs`, `CartAddOutcome.cs`, `IOrderingModule.cs`.
- Testler: `tests/Shopfolio.UnitTests/Catalog/CartQuantityRulesTests.cs`, `Ordering/CartRulesTests.cs`, `Architecture/WishlistAbsenceScanTests.cs`, `Architecture/VersionedSideBulkWriteScan(.Tests).cs`, `Support/ApplicationEnums.cs`, `Infrastructure/SitemapSourceTests.cs`; `tests/Shopfolio.IntegrationTests/Ordering/CartQuantityTests.cs`.
- Kayıt: `Docs/STATUS_CHANGELOG.d/T347.md`, `Docs/DEFERRED_BACKLOG.d/T347.md` (D-619–D-621), `Docs/DEFERRED_BACKLOG.d/T346.md` (D-560, D-589, D-590 yerinde kapandı), `Docs/DEFERRED_BACKLOG.md` (D-150 hedefi), `Docs/IMPLEMENTATION_STATUS.md` (tablo satırı).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Stoktan fazla adet eklenmez; mesaj adet söylemez; ayrılmış adetler düşülür | ✓ | `CartQuantityTests.AQuantityAboveTheStock_IsRefused_*_KK1`, `TheReservedUnits_AreTakenOffTheStock_*_KK1`; `CartQuantityRulesTests.ExceedsStock_*_KK1`, `TheVerdictsAndTheLines_CarryNoStockNumber_KK1` |
| 2 | Artırma ve birleşme aynı kural; stok altına düşen kalem kalır, girmez, adet indirilmez; kontenjan aynı | ✓ | `CartQuantityTests.TheMerge_KeepsALineAboveTheStock_*_KK2`, `AQuantityIncrease_GoesThroughTheSameRule_*_KK2`, `TheServiceCapacity_FollowsTheSameRule_KK2`, `AServiceWithoutCapacity_IsNeverHeldBack_KK2` |
| 3 | Sınır varyantların toplamına; aşan eklenmez, sayıyla söylenir; ikisi aşılınca yalnız sınır; alan boşsa stok | ✓ | `CartQuantityTests.TheLimit_CoversTheSumOfAllVariants_AndIsToldAsANumber_KK3`, `WhenTheLimitAndTheStockAreBothExceeded_OnlyTheLimitIsTold_KK3`, `WithAnEmptyLimit_*_KK3`, `TheSignInMerge_IsJudgedByTheSameLimit_*_KK3` |
| 4 | Sonradan aşılan sınırda kalemler sepette kalır, girmez, toplama sayılmaz | ✓ | `CartQuantityTests.ALoweredLimit_KeepsTheLinesInTheCart_OutOfTheOrder_AndOutOfTheAmounts_KK4`; `CartQuantityRulesTests.Judge_ALimitLoweredLater_*_KK4` |
| 5 | Kalem sayısına tavan yok; elli kalem performans kabulü, `12`'de (ileri işaret: 12) | ✓ | `CartQuantityTests.TheFiftyFirstLine_IsAdded_TheCartHasNoCeiling_KK5`; performans satırı `VAL-F013` `12`'de zaten var (yeni kalem gerekmedi) |
| 6 | Ödemede yalnız giren kalemler çıkar; ödeme sürerken eklenen adet çıkar; başka ürün kalır; birleşen misafir sepetinin ödemesi hesabın sepetine dokunmaz | ✓ | `CartQuantityTests.APaidCardOrder_TakesTheOrdersVariantOutWhole_*_KK6`, `APaidOrder_OfAGuestCartThatMergedAtSignIn_TakesNothingOutOfTheAccountsCart_KK6`, `APaymentThatIsAlreadyApplied_TakesNothingOut_KK6`; `CartStatements.DeleteOrderedItemsAsync` |
| 7 | İstek listesi, favori düğmesi, sepet hatırlatma e-postası yok | ✓ | `WishlistAbsenceScanTests.NoSourceFile_NamesAWishList_AFavouriteOrACartReminder_KK7`, `NoNoticeOfTheNotificationsModule_IsAboutTheCart_KK7` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil bütün kriterler diff'te yeniden denetlendi (kural, okuyucu, `Carts`, ödeme çıkışı, testler satır satır okundu).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–7 | ✓ | yukarıdaki tablo |
| KK1 fazla adet reddi, mesajda adet yok, ayrılmış adet düşülmüş | ✓ | `CartQuantityTests` KK1 iki test; sonuç türleri stok sayısı taşımaz |
| KK2 birleşme ve artış aynı kural, adet indirilmez, kontenjanda da | ✓ | `CartQuantityTests` KK2 dört test |
| KK3 iki varyant toplamı; stok da aşılınca yalnız sınır | ✓ | `CartQuantityTests` KK3 dört test; `CartQuantityRulesTests` KK3 beş test (taşma dahil) |
| KK4 düşürülen sınırda kalem kalır, toplama girmez | ✓ | `CartQuantityTests` KK4; `CartQuantityRulesTests` KK4 üç test |
| KK5 elli birinci kalem eklenir | ✓ | `CartQuantityTests.TheFiftyFirstLine_IsAdded_*_KK5` |
| KK6 giren varyant çıkar, başka ürün kalır, birleşen misafir sepeti hesaba dokunmaz | ✓ | `CartQuantityTests` KK6 üç test |
| KK7 favori ucu ve hatırlatma e-postası yok | ✓ | `WishlistAbsenceScanTests` (örnek tarama deseninin ikili sınaması dahil: `ThePattern_SeesEachFormOfTheAbsentFeatures_KK7`, `ThePattern_DoesNotSee_*_KK7`) |
| `09 §25` ilgili satırlar | ✓ | aşağıdaki tablo |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | yeni istisna yok; `VersionedSideBulkWriteScanTests` `CartStatements.cs` yerini 5 deyim ve iki tabloya bağlar (yeni `DELETE` deyimi sayıya girdi, başka yolda aynı metin sessiz kalmaz) |
| Sık FAIL (2) tarama çağrı biçimini kaçırıyor | ✓ | yeni ham SQL yalnız `CartStatements.cs`'te; `OrderTransitions`'ta `ExecuteSql*` yok; istek listesi taraması her yazım biçimini (tekil, çoğul, Türkçe, e-posta sınıfı) örnekle sınar |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `CartStatements` deyim sayısı (4 -> 5) hem testte hem sınıf özetinde; `ApplicationEnums.NotStored`'a `CartQuantityProblem` eklendi; `FakeCatalog` (`SitemapSourceTests`) iki yeni arayüz yöntemini aldı |
| Sık FAIL (5) belge kuralını değiştiren yeni 09 metni | ✓ | 09, 05, 06, 02 metni değişmedi; karar kaydı gerekmiyor |
| Modül kurucusuna yeni bağımlılık / elle kurulan düzenekler | ✓ | `OrderTransitions` kurucusuna `CartStatements` eklendi (`AddOrderingModule` kaydı var); Integration TAM koşuldu (Disposal, Address, Membership düzenekleri dahil) yeşil |
| Modül yöntemi kendi işlemini açmıyor / kaydetmiyor | ✓ | `Carts` ve okuyucular çağıranın işlemine katılır, kaydetmez; ödeme çıkışı ödemenin işleminde |
| Eşzamanlılık / kilit sırası | ✓ | ekleme sepet kilidini alır ve kuralı kilit altında okur; ödeme çıkışı kilitsiz tek deyim (ödeme işlemi sipariş kilidini tutar, sepet kilidi işlemin ilk deyimi olmak zorunda: `09 §10.2` m.5); D-589 kodu eklemeyi yeni sepete yönlendirir |
| Testte gerçek saate bağlılık yok | ✓ | yeni testlerde Stopwatch, bekleme ya da gerçek saate bağlı sınır yok (grep 0) |

`09 §25` ilgili satırlar:

| Satır | Sonuç | Kanıt |
|---|---|---|
| Veri: göç, FK, kilit dosyası | ✓ | şema değişikliği yok; yeni enum saklanmıyor (`NotStored`) |
| Eşzamanlılık | ✓ | yukarıda |
| Modül sınırı | ✓ | Ordering Catalog'a yalnız `ICatalogModule` yöntemleriyle bakar; Catalog tablolarını Ordering'den sorgulamaz |
| Kayıt / gizli veri | ✓ | yeni kayıt satırı yok |
| Tek işlev | ✓ | stok ve sınır kuralı yalnız `CartQuantityRules`'ta; okuma ve ekleme aynı kuraldan geçer |

Güvenlik taraması: kullanıcı girdisi ham SQL'e yalnız parametre olarak girer (`ExecuteSqlAsync` interpolasyonu; `variant_id = ANY(...)` dizisi parametre); adet toplamı `long` ile hesaplanır ve `int` taşması stok reddidir; mesajlar stok adedini sızdırmaz; yeni bağımlılık yok.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` (`origin/main` birleştirmesinden sonra) | 4951 geçti, 0 kaldı |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests` TAM (PostgreSQL 18, docker; `origin/main` birleştirmesinden sonra) | 2848 geçti, 0 kaldı (birleştirmeden önce 2837); `Ordering` ad alanı tek başına 297 geçti |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | `CartQuantityTests.cs` using sırası düzeltildi; sonra değişiklik yok (birleştirmeden sonra da) |
| Contract | — | — | bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | **tur 1: ✓ PASS** (2026-11-08; head `a8bfecd1`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1 → D-637, K2 → D-638) |

### Doğrulama tur 1 (2026-11-08)

**Verdict: ✓ PASS** (kapsam tam: kabul kriterleri 1–7, KK1–KK7, `09 §25`, güvenlik taraması).

Başlangıç: working tree temiz; `core.hooksPath` depo bekçisi; ana dal son 3 tamamlanmış run (42500144509, 42500141831, 42493884474) success. Dal CI'ı run 42502184068 (head `a8bfecd1`) bütün işler yeşil.

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Stoktan fazla adet eklenmez, adet söylenmez, ayrılmış adet düşülür | ✓ | `CartQuantityRules.ExceedsStock` (`stock - reserved`); `CartAddResult`/`CartQuantityVerdict` stok sayısı taşımaz, `OrderLimit` yalnız `OverOrderLimit`'te dolu; ilk eklemede red sepet bırakmaz; `CartQuantityTests` KK1 iki test yeşil |
| 2 | Artırma ve birleşme aynı kural, adet indirilmez, kalem siparişe girmez; kontenjan aynı | ✓ | Ekleme kuralı sepet kilidi altında güncel kalemlerden okur (`Carts.AddAsync` → `RefusalAsync`); birleşme adedi değiştirmez (`GREATEST`), okuma her kalemi `JudgeCartQuantitiesAsync`'ten geçirir, adet yazan yer yok; kontenjan aynı `stock_quantity` alanında; KK2 dört test |
| 3 | Sınır bütün varyantların toplamına, sayıyla söylenir; ikisi aşılınca yalnız sınır; boşsa stok | ✓ | `CartQuantityRules.JudgeAdd` (sınır önce, aday stoğu aşsa da toplama girer); KK3 dört entegrasyon + beş birim testi (taşma dahil) |
| 4 | Sonradan aşılan sınırda kalem kalır, siparişe ve toplama girmez | ✓ | `Judge` toplamı yalnız `EntersBeforeLimit` kalemlerden; `CartRules.EntersOrder(..., quantityProblem)`; `OrderableSubtotal` 100 TL (KK4 testi) |
| 5 | Kalem sayısına tavan yok | ✓ | Eklemede kalem sayısı okunmaz; `TheFiftyFirstLine_IsAdded_*_KK5` yeşil; performans satırı `VAL-F013` |
| 6 | Ödemede yalnız giren kalemler çıkar; birleşen misafir sepeti hesaba dokunmaz | ✓ | `CompleteApprovalAsync` (kart ve havale ortak yolu) → `DeleteOrderedItemsAsync` (siparişin `cart_id`'si ve varyantları); birleşmede misafir sepeti silinir, kimlik yeniden kullanılmaz; KK6 üç test |
| 7 | İstek listesi, favori, sepet hatırlatma yok | ✓ | `WishlistAbsenceScanTests` (kaynak + bildirim sözleşmeleri); validator `git grep` `src` bütün dosya türlerinde 0 eşleşme |

Testler (validator): build `-warnaserror` 0/0; format üç proje çıkış 0; Unit 4951/4951; betik testleri 4/4; Integration (PG18, `shopfolio-t347-val-pg`) Ordering 301, Catalog + Files + Web + Concurrency 794 — hepsi yeşil. `origin/main` (T311) çakışmasız birleşti; birleşik ağaçta build 0/0, Unit 4974/4974, Integration Ordering + Payment 315 yeşil; `--locked-mode` çıkış 0.

Güvenlik: ham SQL tek kapalı yerde ve parametreli (`variant_id = ANY({variantIds})`), tarama testi beş deyime bağlar; silme siparişin kendi `cart_id`'siyle sınırlı; adet toplamı `long`, `int` taşması `NoStock`; yanıtlar stok adedini taşımaz; yetki yüzeyi değişmedi; yeni bağımlılık yok.

`12` satırları: `VAL-A030`, `A031`, `A033`, `A979` (KP-9), `A978`, `A989` (02 §3.16.8–§3.16.10) ve `D072` (02 §3.16.12) uç yanıtı kanıtı ister; T347 modül düzeyidir (uç T508) → `BEKLEMEDE` kalır (T346/T318/T338 emsali). `12_matris_denetle.py --kapsam 1-77` hata 0.

K-notları: K1 → D-637 (D-560 "dijital ürün adedi" payı kapanmadı: `AddToCartAsync` dijital varyantı ikinci kez ya da 1'den büyük adetle kabul eder; kural `CheckDigitalAddAsync`'te var ama ekleme yolu çağırmıyor — T347 kriteri değil, `02 §3.12.4`). K2 → D-638 (yeni sonuç adları `NoStock`/`OverOrderLimit` `07`'nin hata kodlarıyla — `StockInsufficient`, `MaxQuantityPerOrderExceeded` — birebir değil; uçta eşleme yazılmalı).

Yapım raporu karşılaştırması: davranışta tam uyum; öz denetim tablosu kriter ve KK başına kanıtlı. Uyuşmazlık: "Kapanan: D-560 (bu task)" — D-560'ın dijital adet payı yapılmadı (K1).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T347-sepet-adet`
- **Commit:** `bffe1c80` — T347 rapor ve kayıtlar (yapım kodu `451b6a9b`)
- **PR:** #475
- **CI:** işçi yoklamaz; yöneticinin izlemesindedir
- **Post-merge run'lar:** squash `8789b22` (#475) — CI 42510559202 success, Publish 42510559169 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok. Yeni dış bağımlılık, hizmet ya da harici sistem yok; PostgreSQL 18 mevcut test düzeneğidir.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sepet sayfası adet mesajları ve adet alanı | D-620, D-150 (hedef T508) | sayfa task'ı; T347 ekran kurmaz |
| 2 | Onayda sepetin yeniden değerlendirilmesi ve stok ayırma | D-621 (T348) | sipariş onayı task'ı |
| 3 | Misafir ekleme ile birleşme yarışının deterministik testi | D-619 | kilit sırasını sabitleyen kanca gerekir; kod düzeltildi (D-589 kapandı) |
| 4 | `D-198` "kapıyı okuyan yüz" listesinde T347 anılıyor | — (hedef bozulmadı) | T347 satış kapısını okumaz; sepet okuması T346'dadır; not düşüldü |
| 5 | Elli kalemin performans kabulü | — (`VAL-F013` `12`'de var) | kriter 5 ileri işareti `12`'ye; yeni kalem gerekmedi |

Kapanan: D-560 (bu task), D-589 (kod), D-590 (test `TheMerge_CarriesTheTakenOffMark_*_D590`).

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 42500144509, 42500141831, 42493884421 hepsi success
- **Repo memory (bitiş kapısı 9):** güncelleme gerekmedi (proje hafızası değişmedi; "Güncel Durum" parça düzeni gereği dokunulmadı)
- **Diğer:** `origin/main` (T350) çakışmasız birleştirildi, birleşik ağaçta Unit ve Integration tam yeniden koşuldu.
