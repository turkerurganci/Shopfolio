# T354 — Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama tur 2 ⛔ BLOCKED — main ile kod çakışması, birleştirme bekliyor) | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Sipariş numarası — `Infrastructure/Security/OrderNumber`:** `Create(year)` yıl, tire ve P-19 (`ProductConstants.OrderNumberRandomBlockLength` = 6) kadar karakter verir; alfabe 32 karakterdir ve sabittir (`23456789ABCDEFGHJKLMNPQRSTUVWXYZ`: `0`, `1`, `I`, `O`, `l` yok), her karakter `RandomNumberGenerator.GetInt32(32)` ile seçilir (`09 §15.1` madde 1, `05 §6.4.2`).
- **Numaranın ve anahtarın verilmesi — `Ordering/OrderIdentifiers`:** `IssueAsync(now, ct)` Türkiye yılıyla numara üretir; var olan siparişin ya da çağıranın henüz kaydetmediği siparişin numarasını atıp yenisini üretir (sipariş oluşmadan), yeni erişim anahtarını `SecretKey.Create()` ile verir. Sipariş silinmediği ve iptal edilen sipariş yerinde kaldığı için iptalin numarası da yeniden verilmez. `UniqueIndexes` (`ix_orders_order_number`, `ix_orders_order_access_key`) eşzamanlı çakışmada `ITransactionRunner` yeniden denemesine verilecek adlardır (`09 §9.4` madde 3); tavan koyulmadı (belgede yok, `09 §3.5`).
- **Üç giriş yolu:** üye geçmişi (`FindMemberOrderAsync`, var olan), misafir numara + e-posta (`FindOrderByNumberAndEmailAsync`, yeni; numara harf büyüklüğüne duyarsız, e-posta normalleştirilmiş, siparişe donmuş e-postayla eşleşir; imha edilmiş sipariş eşleşmez; her yanlış parça aynı `null`), e-posta bağlantısı (`FindOrderByAccessKeyAsync`, var olan; anahtar harf büyüklüğüne duyarlı). Numara tek başına hiçbir yolda sipariş açmaz.
- **Erişim anahtarı:** `Order.RenewAccessKey()` yeni anahtarı üretir, eskisini aynı anda geçersiz kılar, imha edilmiş siparişte reddeder; anahtarın saat okuyan hiçbir yolu yok (ömrü yok), imhada `ErasePersonalData` boşaltır (var olan).
- **Sipariş sayfasının dört bilgisi — `GetOrderPageSummaryAsync`:** iki durum ekseni, teslimat alanı ve "Takip et" bağlantısı (`CarrierCatalog`), ödemesi onaylı dijital kalemlerin indirme düğmeleri (hak kalmadıysa kapalı), havale siparişinin donmuş IBAN'ı; yalnız siparişten okunur, e-posta, bildirim ya da kuyruk gerekmez.
- **Backlog:** D-563 (T348 çağıranı), D-564 (T375 yenileme), D-565 (T511 sayfaları, T376 limit kaydı), D-566 (IBAN görünümü belge boşluğu), D-567 (anahtar tek yazıcı taraması).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Security/OrderNumber.cs` (yeni)
- `src/Shopfolio.Web/Modules/Ordering/OrderIdentifiers.cs`, `OrderAccess.cs` (yeni); `Order.cs` (`RenewAccessKey`); `OrderingModule.cs` (kayıt ve iki yöntem); `Contracts/IOrderingModule.cs`, `Contracts/OrderAccessContracts.cs` (yeni)
- Testler: `tests/Shopfolio.UnitTests/Security/OrderNumberTests.cs`; `tests/Shopfolio.IntegrationTests/Ordering/OrderAccessTests.cs`
- Belgeler: `Docs/DEFERRED_BACKLOG.d/T354.md`, `Docs/STATUS_CHANGELOG.d/T354.md`, `Docs/IMPLEMENTATION_STATUS.md` (T354 satırı), bu rapor

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Numara tahmin edilemez kod: yıl + rastgele blok (üretimi `09 §15.1`), karışabilen karakter yok, blok uzunluğu §11 değeri | ✓ | `OrderNumber.Create`; Unit `Alphabet_HasThirtyTwoDistinctCharacters_AndNoneThatLooksAlike`, `Create_GivesTheYear_AHyphen_AndABlockOfTheProductLength` (uzunluk ve desen `ProductConstants`'tan), `Create_UsesOnlyTheAlphabet_AndEveryCharacterOfItShowsUp`, `Create_TwoCalls_GiveTwoNumbers`, `Create_RefusesAYearThatIsNotFourDigits`; Integration `Issue_GivesANumberOfTheTurkeyYear_AndAnAccessKey_KK1` |
| 2 | Numara oluşurken verilir, iptalde yeniden kullanılmaz, tekildir; çakışmada sipariş oluşmadan yenisi; fatura numarası değildir | ✓ | Integration `Issue_WhenTheNumberIsTakenByAnOrder_MakesAnotherBeforeAnyOrderExists_KK2` (sipariş sayısı değişmez), `Issue_AfterTheOrderIsCancelled_StillSkipsItsNumber_KK2`, `Issue_SkipsTheNumberOfAnOrderTheCallerAddedAndHasNotSavedYet_KK2`, `TheUniqueIndexes_AreTheOnesTheRetryNames_AndTheDatabaseBreaksThemOnAClash_KK2` (veritabanı iki dizin adını verir, liste ile eşit); numara hiçbir fatura alanına bağlı değildir (`src/`'te fatura numarası alanı yok) |
| 3 | Üç yoldan giriş: üye geçmişi, misafir numara + e-posta, e-posta bağlantısı; L-5 limiti T376'da | ✓ | Integration `AMemberOpensTheOrderFromTheHistory_...KK3`, `AGuestOpensTheOrderWithTheNumberAndTheEmail_WhateverTheLetterCase_KK3`, `TheGuestsLookup_MatchesTheFrozenEmail_OfADeletedAccountAndOfAnAccountWhoseAddressChanged_KK3`, `TheAccessKeyOpensTheOrderOfAGuestAndOfAMember_AndNothingElse_KK3_KK4`, `TheThreeWays_OpenTheSameOrder_KK3`; sayfa ve limit kaydı D-565 |
| 4 | Misafir ve silinmiş hesap bağlantısı siparişe özgü, tahmin edilemez anahtar taşır, e-posta sormaz; ömrü yok, yalnız e-posta düzeltmesinde yenilenir, eskisi geçersiz | ✓ | `SecretKey.Create` (256 bit); Integration `TheAccessKeyOpensWithoutAskingForTheEmail_AndHasNoLifeOfItsOwn_KK4` (on yıllık sipariş), `TheAccessKeyIsCaseSensitive_AndTwoOrdersHaveTwoKeys_KK4`, `WhenTheKeyIsRenewed_TheOldOneStopsOpening_AndTheNewOneOpens_KK4`, `WhenThePersonalDataIsDisposed_TheKeyIsEmpty_AndNoWayOpensTheOrder_KK4`; Unit `RenewAccessKey_*`; düzeltme işlemi T375 (D-564) |
| 5 | Üyenin bağlantısı hesaptaki sayfanın adresi, girişsiz açılmaz; numara tek başına açmaz | ✓ | Integration `AMemberOpensTheOrderFromTheHistory_AnotherCustomerAndAGuestOrderDoNot_KK3` (başka müşteri ve misafir siparişi `null`), `TheGuestsLookup_AnswersNullForEveryWrongPart_AndTheNumberAloneOpensNothing_KK3_KK5` (boş e-posta, yanlış e-posta, yalnız numara); adres kökü `OrderRoutes.MemberRoot` T215'ten, girişsiz 302 sayfası D-565 |
| 6 | Sayfa durum, takip bağlantısı, indirme düğmesi, donmuş IBAN taşır; hiçbir akış e-postaya bağlı değil | ✓ | Integration `ThePageSummary_CarriesTheStatus_TheTrackingLink_TheDownloadButtons_AndTheFrozenIban_KK6` (aynı testte `dispatch_records` ve `jobs` boş), `ThePageSummary_OfAnUnpaidCardOrder_HasNoDownloadButton_NoIban_AndNoLink_KK6`, `ThePageSummary_OfAMissingOrder_IsNull_KK6`; IBAN görünüm kuralı D-566 |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–6 (yukarıdaki tablo) | ✓ | tablodaki test adları; diff'te tek tek denetlendi |
| KK1 numara biçimi ve karakter kümesi; blok `09 §15.1` üreticisinden | ✓ | `OrderNumber.cs`; Unit `OrderNumberTests` (alfabe, biçim, yıl, tüm karakterler görünür); Integration `Issue_GivesANumberOfTheTurkeyYear_...`: `OrderIdentifiers` numarayı yalnız `OrderNumber.Create`'ten alır (başka üretici yok) |
| KK2 çakışma ve iptalde yeniden kullanım yok | ✓ | üç `Issue_*_KK2` testi ve dizin adı testi |
| KK3 üç giriş yolu çalışır | ✓ | beş KK3 testi; üç yol aynı siparişi açar |
| KK4 anahtarlı bağlantı e-posta sormaz, tahmin edilemez, ömrü yok (imhada boşalır: T301 k4), yenilenince eskisi açmaz | ✓ | dört KK4 testi; imha testi `ErasePersonalData` ile |
| KK5 üye bağlantısı girişsiz açılmaz, yalnız numara reddedilir | ✓ | `..._AnotherCustomerAndAGuestOrderDoNot_KK3`, `..._TheNumberAloneOpensNothing_KK3_KK5` |
| KK6 sayfa dört bilgiyi taşır, e-postasız da erişilir | ✓ | `ThePageSummary_Carries...`, e-posta kaydı yok |
| `09 §25` ilgili satırlar: modül yöntemi kendi işlemini açmıyor, `SaveChanges` yok | ✓ | `OrderAccess` ve `OrderIdentifiers` salt okuma; `Order.RenewAccessKey` alan yöntemi, kaydı çağıran yapar |
| `09 §25`: `!` yok, zaman `TimeProvider`/parametre, yeni sabit yok | ✓ | `OrderAccess` desen eşlemesi kullanır; `IssueAsync(now)` saati çağırandan alır; belgede olmayan tavan ya da süre sabiti eklenmedi (denemeye tavan yok) |
| `09 §15.1` madde 1: `RandomNumberGenerator` yalnız `Infrastructure/Security/`'de | ✓ | `OrderNumber.cs` o klasörde; `Modules/Ordering` rastgele üretmez, `SecretKey.Create()` ve `OrderNumber.Create()` çağırır |
| Güvenlik taraması: sır sızıntısı, yetki, girdi doğrulama, yeni bağımlılık | ✓ | anahtar ve numara kayda yazılmaz; tüm yanlış parçalar tek `null` (varlık sızmaz); yeni paket yok |
| Sık FAIL: bekçi/tarama istisnası geniş | ✓ | yeni tarama ve istisna eklenmedi; tek yazıcı taraması D-567 olarak T375'e bağlandı |
| Sık FAIL: kaynak taraması çağrı biçimi | ✓ | yeni tarama yok (yukarısı) |
| Sık FAIL: DB default / FK dizini | ✓ | şema değişmedi (migration yok); `ix_orders_order_number` ve `ix_orders_order_access_key` önceden vardı, testle adları doğrulandı |
| Sık FAIL: aynı sayıyı anan öteki yerler | ✓ | Düzeltme 1: önceki iddia yanlıştı. `git grep -nE "OrderingModule>|OrderingModule\(|IOrderingModule"` ile `tests/` tarandı: elle kuran üç düzenek (`DisposalHarness`, `AddressTests`, `MembershipTests`) vardı, üçüne `OrderAccess` kaydı eklendi; ötekiler `AddOrderingModule()` çağırıyor; `SchedulerHarness` `IOrderingModule` kaydetmiyor. Integration TAM küme yeşil (aşağıda) |
| Sık FAIL: karar kaydısız 09 metni | ✓ | `09`, `02`, `05` değişmedi; belge boşluğu olan yorum D-566'da |
| Sık FAIL: test beklenen değeri sabitten türetmiyor | ✓ | blok uzunluğu ve desen `ProductConstants`'tan; alfabe literali `05 §6.4.2`'nin kuralının kendisidir (bilerek ikinci kopya) |
| Sık FAIL: sayfalı liste / eşzamanlılık / önbellek | ✓ | yok: liste, önbellek ya da yazma yok; çakışma yarışı dizin + yeniden deneme adlarıyla (D-563) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release --no-build` | 4748 geçti, 0 kalan (14 yeni: `OrderNumberTests`; Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj -c Release --no-build --filter-namespace ...Ordering` | `OrderAccessTests` 18 yeni; Ordering ad alanı 244; `SessionAccessTests` 36 geçti. Düzeltme 1 sonrası (main birleştirilmiş ağaç) TAM Integration klasör parçalarıyla koşuldu: Disposal 61, Model 1354, Ordering 244, Catalog 72, Concurrency 26, Jobs 77, Security 32, geri kalanı 916; toplam 2782, 0 kalan (Model'in ilk koşusunda 1 test bir kez düştü, aynı komutla iki yeniden koşuda 1354/1354 geçti) |
| Build / format | ✓ | `dotnet build ... -c Release -warnaserror` (Web, Unit, Integration); `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; format temiz |
| Contract | — | `NOT_APPLICABLE` | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⛔ BLOCKED (tur 2, 2026-11-08 — main ile kod çakışması); önceki: ✗ FAIL (tur 1) |
| Bulgu sayısı | 0 içerik bulgusu (tur 1: B1 ORTA S2, giderildi) |
| Düzeltme gerekli mi | Evet — `origin/main` birleştirmesi ve `OrderingModule.cs` çakışmasının çözümü |
| Bloke-etmeyen not (K) sayısı | 1 (rapor içi eskimiş cümle) |

### Doğrulama Sonucu — tur 1 (validator)

**Head:** `53a71cd0` + `origin/main` `f1603761` birleştirmesi · **PR:** #470 · **Verdict:** ✗ FAIL

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = depo `scripts/git-hooks`; ana dal CI son 3: 42483789437, 42483789393, 42480365166 hepsi `success`.
- **Kabul kriterleri (modül düzeyi):** 1–6 ✓ ve KK1–KK6 ✓ — `OrderNumber` alfabesi ve uzunluğu `09 §15.1` madde 1 ile birebir (P-19 = 6); `OrderIdentifiers` çakışmada ve iptalde numarayı atlar, dizin adları veritabanıyla eşleşir; üç yol (`FindMemberOrderAsync`, `FindOrderByNumberAndEmailAsync`, `FindOrderByAccessKeyAsync`) ve `RenewAccessKey`/imha davranışı testlerle kanıtlı; sayfa özeti dört bilgiyi siparişten okur. Sayfa uçları T511 (D-565), onay çağıranı T348 (D-563), yenileme çağıranı T375 (D-564) — plan katmanlamasıyla uyumlu.
- **Task satırları:** KP-17/18'in T354'e düşen satırları (VAL-D009, B014, B015, B016, A054, A977, A055, A057, A058, B017) uç yanıtı / tarayıcı / sipariş onayı kanıtı ister; uçlar ve onay sonraki task'larda. `Durum` hücreleri `BEKLEMEDE` kaldı.
- **Testler (yerel):** build `-warnaserror` 0 uyarı; format üç proje temiz; Unit 4770/4770; betik testleri 4 dosya; Integration Ordering ad alanı 244/244. `Model.AddressTests` ve `Model.MembershipTests` yerelde düşüyor (aşağıda B1).
- **Dal CI:** 42485052233 ✗ — 4. Integration test 110/2729 başarısız; öteki adımlar yeşil.
- **B1 — ORTA, S2 Kırılma:** `OrderingModule` kurucusuna `OrderAccess` eklendi; `OrderingModule`'ü elle kaydeden üç düzenek (`tests/Shopfolio.IntegrationTests/Disposal/DisposalHarness.cs:69`, `Model/AddressTests.cs:111`, `Model/MembershipTests.cs:121`) `OrderAccess`'i kaydetmediği için `IOrderingModule` çözülemiyor: `Unable to resolve service for type 'Shopfolio.Modules.Ordering.OrderAccess'` (CI'da 109 test; Disposal*, `AddressTests`, `MembershipTests`). Düzeltme: üç düzeneğe `services.AddScoped<OrderAccess>();` eklenir; tam Integration yerelde ya da CI'da yeşil görülür.
- **Güvenlik:** sır yok; numara tek başına hiçbir yolu açmıyor, yanlış parçaların hepsi aynı `null`; anahtar `SecretKey.Create` (256 bit), imhada boş; yeni bağımlılık yok.
- **Yapım raporu karşılaştırması:** 1 uyuşmazlık — öz denetimdeki "`OrderingModule`'ü elle kuran yer yok; bütün test sağlayıcıları `AddOrderingModule`'ü çağırır" iddiası yanlış (`grep "new OrderingModule("` arama biçimi `AddScoped<IOrderingModule, OrderingModule>` kaydını yakalamadı); tam Integration koşulmadığı için kırılma görülmedi.

### Düzeltme 1

B1 giderildi: `DisposalHarness`, `Model/AddressTests`, `Model/MembershipTests` düzeneklerine `services.AddScoped<OrderAccess>();` eklendi (başka elle kuran düzenek yok). Kabul kriterleri 1–6 ve KK1–KK6 ile kontrol listesinin bütün maddeleri düzeltilmiş diff'te yeniden denetlendi: ✓ (üretim kodu değişmedi). Build `-warnaserror` 0 uyarı, format üç proje temiz, Integration tam küme yeşil.

### Doğrulama Sonucu — tur 2 (validator)

**Head:** `12de6aad` · **PR:** #470 · **Verdict:** ⛔ BLOCKED

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = ana depo `scripts/git-hooks`; ana dal CI son 3: 42492032672, 42490732319, 42490732324 hepsi `success`.
- **Kabul kriterleri:** 1–6 ve KK1–KK6 ✓ (modül düzeyi; tur 1 ile aynı kanıtlar yeniden üretildi). `09 §25` ilgili satırlar ve güvenlik taraması temiz; yeni bağımlılık yok.
- **Task satırları:** VAL-D009, B014, B015, B016, A054, A977, A055, A057, A058, B017 `BEKLEMEDE` kaldı (uç/tarayıcı/onay kanıtı T511 ve T348'de).
- **Testler (yerel):** build `-warnaserror` 0 uyarı; format üç proje temiz; Unit 4822/4822; betik 4 dosya; Integration `OrderAccessTests` 18/18, Ordering 244/244, Disposal 61/61, Model 1353/1354 (düşen `SchedulerBackupDisposalAndKeyTablesTests.Migration_WithoutARoleSettingItReads_…` T354 dışı, tek başına 55/55 geçti — yük altında kararsız).
- **Dal CI:** 42492570840 ✓ (bütün adımlar).
- **BLOCKED nedeni:** `git merge origin/main` kod dosyasında çakıştı: `src/Shopfolio.Web/Modules/Ordering/OrderingModule.cs` — T346 (#467) kurucuya `Carts` ve kayda `CartStatements`/`Carts` ekledi, T354 `OrderAccess` ve `OrderIdentifiers`/`OrderAccess` ekliyor. Talimat gereği merge iptal edildi. Çözüm: kurucu iki parametreyi de alır, dört kayıt birlikte kalır; otomatik birleşen üç düzenekte (`DisposalHarness`, `Model/AddressTests`, `Model/MembershipTests`) iki kaydın da bulunduğu denetlenir ve Disposal/Model/Ordering Integration koşulur; sonra birleşik uç için yeni doğrulama turu.
- **İçerik bulgusu:** yok. **K-notu:** "Notlar > Diğer" satırındaki "Tam Integration paketi yerelde koşulmadı" cümlesi Düzeltme 1 ile eskimiş; birleşme düzeltmesinde güncellenir.
- **Yapım raporu karşılaştırması:** tam uyumlu; öz denetim tablosu kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T354-siparis-no-takip`
- **Commit:** (PR'da)
- **PR:** #470
- **CI:** (yönetici yardımcı izler)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Bu task'ta plan tier, kota, sağlayıcı, paket sürümü ya da onay süreci varsayımı yok: yalnız modül kodu, mevcut tekil dizinler ve `SecretKey` | 4a tarama terimleri kabul kriterlerinde geçmiyor; yeni paket yok; şema değişmedi | ✓ |
| 2 | Yerel PostgreSQL 18, CI hizmetiyle aynı imaj | `docker run postgres:18` (`.github/workflows/ci.yml` `postgres:18`) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Numara ve anahtarı sipariş onayında alan çağıran | D-563 | Sipariş onayı T348 |
| 2 | Anahtar yenilemeyi çağıran e-posta düzeltmesi | D-564 | T375 |
| 3 | Üç yolun sayfaları, 303/302/404 yanıtları, L-5 sayaç kaydı | D-565 | T511, T376 |
| 4 | IBAN'ın ödeme sonrası görünümü (belge boşluğu) | D-566 | T353, T511 |
| 5 | Anahtarın tek yazıcısını kilitleyen tarama | D-567 | T375 yazıcıyı eklerken |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks` (depo kökü).
- **Ana dal CI startup (Adım 0):** 42480365166 success, 42480365007 success, 42479203249 success.
- **Repo memory (bitiş kapısı 9):** `MEMORY.md`'ye dokunulmadı; olay `Docs/STATUS_CHANGELOG.d/T354.md`'de.
- **⚠ Kolay kararlar:** (1) numara üretiminde deneme tavanı yok — belgede tavan yazılı değil, döngü ilk boş numarada biter; (2) misafir sorgusu üyenin siparişini de donmuş e-postayla eşleştirir (`04` 5.15.6: e-postası değişmiş hesabın siparişi); (3) sayfa özeti IBAN'ı duruma bakmadan verir (D-566).
- **Diğer:** Tam Integration paketi Düzeltme 1'de klasör parçalarıyla yerelde koşuldu (2782, 0 kalan). `origin/main` ikinci birleştirmesinden (`OrderingModule.cs` kurucusunda `Carts` + `OrderAccess` birlikte) sonra TAM küme yeniden koşuldu: Ordering 299, Model 1355, Catalog/Commands/Concurrency/CorporateContent/Database 234, Disposal/FileSecrets/Files/Jobs/Logging/Outages 313, Pages/Security/SignedEndpoints/Web 638; toplam 2839, 0 kalan (Model'in ilk koşusunda yük altında zaman aşımıyla düşen `SchedulerBackupDisposalAndKeyTablesTests` tek başına 55/55 geçti). Unit 4878 yeşil.
