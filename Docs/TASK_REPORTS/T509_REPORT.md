# T509 — Ödeme adımı (E-13)

**Faz:** F5 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /odeme` (`Pages/Storefront/Checkout.cshtml(.cs)`, `[NoStore]`, noindex): tek sayfa, tek form; bölümler `03 §2.4` sırasıyla (iletişim, fiziksel kalemde teslimat adresi, fatura adresi, yöntem, özet, onay). Her gösterimde yeni `ConfirmationKey`, özet parmak izi ve iki metnin sürüm kimlikleri; misafir e-postası onay özetinde, e-posta alanına düzeltme bağlantısıyla (D-729).
- Beş işleyici (`ApplyCoupon`, `RemoveCoupon`, `Recalculate`, `Leave`, `PlaceOrder`) aynı formda `formaction` ile. Geçiş kapalıysa (T508 k3) GET ve üç ara gönderim 303 → `/sepet`.
- Çerez (`CheckoutForms.cs`, `CheckoutCookieValues`) yalnız `05 §6.7.9` alanlarını taşır, yalnız ayrılışta yazılır, sipariş oluşunca silinir; kutular, anahtar, parmak izi, sürüm yazılmaz.
- Onay `05 §4.2.2`'nin tek işleminde (`ITransactionRunner.RunAsync(work, PlaceOrderRequest.RetryOnUniqueIndexes, ct)`; D-563 tek ev); sonuç eşlemesi: teyit 303, 422 (eksik alan/kutu, il, özet farkı, kapanan satış, yöntem), 429 (L-7, L-8; adressiz istek de nötr 429 — D-805), 404 (yabancı defter girdisi), 303'ler (kalem yok / asgari tutar → `/sepet`, oluşmuş sipariş → teyit, belirsiz sonuç → `IOutcomeTarget` sonuç ekranı `[OutcomeUncertain("UC-27")]`, kapanan oturum → giriş).
- Modül yüzü: `ICheckout` (okuma, onay, `MarkPaymentUnavailableAsync`), `OrderConfirmation.ReadStepAsync` (kilitli okuma, kupon kararı, önceki ödenmemiş sipariş), `SummaryChanges` (değişiklik satırları), `Carts.MarkPaymentUnavailableAsync` (UC-26).
- Bu tur: cayma özetinde iki fiziksel cümle yalnız fiziksel kalemde (D-743); hesabı silinen oturumun siparişi misafir yoluna düşer ve doğrulanmış hesaba bağlanır (D-385); hedefi T509 olan backlog kalemleri kapatıldı ya da gerekçeyle ileri atıldı.
- Tarama kuralları gevşetilmeden kod kurala uyduruldu: enum başına dosya (`PlaceOrderOutcome.cs`, `SummaryChangeKind.cs` + `SummaryChange.cs`), iki enum saklanmayan değer listesinde (`ApplicationEnums.NotStored`; sütun değil, sonuç kodu ve okuma biçimi), form `asp-antiforgery` + sayfa yoluyla `action`, gerekçesiz `!` gerekçe aldı, `07 §5.2` kalın alan adları ve dört kutu alanı `[SpecAssignedName]`, erişim anahtarı parametresi varsayılansız (ölü `OrderConfirmationResult.Placed` silindi), KK2 testi iddiayı testin kendisinde yapar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Storefront/Checkout.cshtml(.cs)`, `CheckoutForms.cs`, `PlaceOrderInput.cs`
- `src/Shopfolio.Web/Modules/Ordering/` (`Checkout.cs`, `Contracts/CheckoutContracts.cs`, `Contracts/PlaceOrderOutcome.cs`, `Contracts/SummaryChangeKind.cs`, `Contracts/SummaryChange.cs`, `OrderConfirmation.cs`, `OrderIdentifiers.cs`, `Carts.cs`, `ConfirmationStepRules.cs`, `OrderingModule.cs`; `SummaryComparison.cs` kaldırıldı), `Modules/Membership/` (`IMembershipModule`, `MembershipModule`)
- Testler: `tests/Shopfolio.IntegrationTests/Web/CheckoutPageTests.cs` + `.Helpers.cs`, `Web/StorefrontFrameTests.cs`, `Model/MembershipTests.cs`, `Ordering/MainFlowTests.cs`, `Ordering/OrderConfirmationTests.cs`, `Ordering/OpenSalesGate.cs`, `Ordering/OrderingTestServices.cs`, `Administration/PanelHomeTests.cs`, `OutcomeUncertainConsistencyTests.cs`; `tests/Shopfolio.UnitTests/Pages/CheckoutFormsTests.cs`, `Ordering/ConfirmationStepTests.cs`, `Support/ApplicationEnums.cs`
- Kayıt: `Docs/DEFERRED_BACKLOG.md` (yerinde kapatma/ileri atma), `Docs/DEFERRED_BACKLOG.d/T508.md`, `T537.md`, `T544.md`, `T509.md` (D-1333, D-1334), `Docs/STATUS_CHANGELOG.d/T509.md`, `Docs/IMPLEMENTATION_STATUS.md` T509 satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Bölümler tek sayfada; yeni anahtar, parmak izi, iki sürüm; misafir e-postası özette, kodsuz, düzeltilebilir | ✓ | `CheckoutPageTests.TheGet_DrawsTheSectionsOfTheCart_WithAFreshKey_TheFingerprintAndTheVersions_T509_KK1` |
| 2 | Geçiş kapalıysa UC-22…UC-25 303 → `/sepet` | ✓ | `TheStepThatCannotOpen_SendsTheGetAndTheThreeSubmissionsToTheCart_T509_KK2` |
| 3 | Çerezden dolar; yalnız ayrılışta, yalnız `05 §6.7.9` alanları | ✓ | `TheLeaving_WritesTheCookieWithoutValidating_…_T509_KK3_KK6`; Unit `CheckoutFormsTests.TheCookie_KeepsOnlyTheDocumentedFields_…` |
| 4 | `ApplyCoupon` 200 yazmaz; 422 iki biçim; 429 L-6; hak ayrılmaz | ✓ | `TheCoupon_IsAppliedWithoutWriting_RefusedWith422_AndRemoved_T509_KK4_KK5`, `TheCouponAttempts_AreClosedByL6With429_T509_KK4` |
| 5 | `RemoveCoupon`; `Recalculate` zorunluluk denetlemez; hatırlatma L-3'e sayılmaz | ✓ | `…_KK4_KK5`, `TheRecalculation_ChecksNoRequiredField_TellsAnUnservedProvince_AndTheRegisteredEmailReminder_T509_KK5` |
| 6 | `Leave` doğrulamasız çerez; üç hedef; yöntem yoksa işaret | ✓ | `…_KK3_KK6`, `TheReturnToTheCart_WithNoUsableMethod_MarksTheCart_T509_KK6` |
| 7 | Tek işlem, 303 teyit, çerez silinir | ✓ | `TheConfirmation_PlacesTheOrder_GoesToItsConfirmation_DeletesTheCookie_AndTheRepeatOpensNoSecondOrder_T509_KK7_KK10` |
| 8 | 422 beş sebep; 429 L-7/L-8; 404 yabancı defter | ✓ | `…RefusesMissingFieldsAndBoxesTogether_With422_…_KK8`, `…AnUnservedProvince_AChangedSummary_AndAnUnusableMethod_With422_T509_KK8`, `…TheClosedSale_With422_AndAForeignBookEntry_With404_T509_KK8`, `AFullOpenUnpaidCeiling_RefusesTheNextConfirmationWith429_T509_KK8`, `AConfirmationWithoutAClientAddress_IsRefusedWithTheNeutral429_AndNoOrder_T509_KK8` |
| 9 | 303'ler: kalem yok / asgari tutar → sepet; oluşmuş → teyit; belirsiz → sonuç ekranı; kapanan oturum → giriş | ✓ | `…SendsTheCartsLostLinesToTheCart_AndAClosedMemberSessionToTheSignIn_T509_KK9`, `…UnderTheMinimumAmount_GoesToTheCart_T509_KK9`, `AnUnexpectedError_…_T509_KK9`, `…_KK7_KK10` (aynı anahtar → teyit) |
| 10 | Aynı anahtar ikinci sipariş açmaz; iki sekme kilitle sıraya girer | ✓ | `…_KK7_KK10`, `TwoTabs_WithTwoKeys_AreQueuedByTheCartsLock_AndTheSecondCancelsTheFirstsUnpaidOrder_T509_KK10` |

> `? Doğrulanamadı` **FAIL değildir** — kanıt eksikliğidir. FAIL'den ayrı tutulur.

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil kriter 1–10 ve KK1–KK10 diff'te denetlendi; `sik-fail-nedenleri.md` ana türleri ve hedefi T509 olan backlog kalemleri ayrıca tarandı.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1: sepete göre bölüm, taze anahtar; misafir e-postası özette | ✓ | KK1 testi (dijital sepette teslimat bölümü yok; iki GET farklı anahtar; e-posta + düzeltme bağlantısı) |
| KK2: üç sebep, dört uçta | ✓ | KK2 testi (GET, ApplyCoupon, RemoveCoupon, Recalculate; satış kapalı, kapı, özet yok) |
| KK3: çerez alan listesi; kutu yazılmaz | ✓ | Unit `CheckoutFormsTests` (alan listesi = `05 §6.7.9`), KK3_KK6 testi |
| KK4: üç sonuç, L-6; 200 yazmaz, hak ayırmaz | ✓ | KK4 testleri (`reserved_count` değişmez) |
| KK5: kaldırma, ara gönderim, hatırlatma | ✓ | KK4_KK5, KK5 testleri |
| KK6: üç hedef, boş formla da; işaret | ✓ | KK3_KK6, KK6 testleri |
| KK7: tek işlem, çerez silinir | ✓ | KK7_KK10 testi; işlem `Transactions.RunAsync` sayfada (modül işlem açmaz, `09 §9.1` m.3) |
| KK8: beş 422 sebebi, 429, 404 | ✓ | KK8 testleri (5) |
| KK9: beş 303 sebebi | ✓ | KK9 testleri (3) + KK7_KK10 |
| KK10: iki sekme sıraya girer | ✓ | `TwoTabs_…_KK10` |
| `09 §8.1` m.4/m.6: geniş `catch` yok, tek hata katmanı | ✓ | `[OutcomeUncertain("UC-27")]`; `OutcomeUncertainConsistencyTests` (UC-27 `NotMarkedYet`'ten çıkarıldı) |
| `09 §15`: istemci adresi tek kaynaktan | ✓ | `ClientAddress.Of` (`RequestOriginSourceScan` kuralı) |
| `09 §9.4` m.3: tekil dizin yeniden denemesi işlemin sahibinde, liste tek ev | ✓ | `PlaceOrderRequest.RetryOnUniqueIndexes`; `OrderIdentifiers.UniqueIndexes` ona bağlı; `OrderAccessTests` |
| `09 §3.4` tek kural evi | ✓ | e-posta `EmailRule.Allows`, kutular `ConsentBoxes.Missing`, il `AddressForms.CheckProvince`, yöntemler `IPaymentMethods`, L-8 `OrderPlacementLimits.FindExceededAsync` (onayda) |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | Yeni istisna yok; `OutcomeUncertainConsistencyTests` listesinden bir öğe çıktı (daraldı) |
| Sık FAIL 2: kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama kuralı yok; mevcut taramalar yeni sayfayı da görür (Integration TAM) |
| Sık FAIL 3: "Varsayılan" DB default'u / FK dizini | ✓ | Şema değişikliği yok |
| Sık FAIL 4: kural değişince aynı sayıyı anan öteki yerler | ✓ | UC-27 işareti tek listede güncellendi; tekil dizin listesi tek evde |
| Sık FAIL 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09` değişmedi |
| Hedefi T509 olan backlog kalemleri | ✓ | Kapandı: D-385, D-563, D-626, D-729, D-743, D-785, D-805, D-807; T509 payı kapandı: D-254, D-437, D-801, D-1077, D-1225 (D-1080: `VAL-E061` doğrulamanın); ileri atıldı (gerekçeli): D-435, D-650, D-769, D-853, D-1189, D-1190, D-1245, D-1246 → F5 gate; D-1127 → T510 |
| Modül kurucusuna yeni bağımlılık, elle kuran test düzenekleri | ✓ | `Checkout` kurucusuna `Carts`; `OrderingTestServices` kullanan bütün sınıflar koşuldu (Test Sonuçları); Integration TAM doğrulayıcıda |
| Sık FAIL (modül/akış değişince elle kuran test düzenekleri) | ✓ | Satış kapısı onaya girince `OrderingTestServices`/`MainFlowTests` düzeneği güncellendi (`OpenSalesGate`); gerçek kapıyı okuyan testler gerçek `SettingsModule`'ü kurar (`OrderConfirmationTests` iki satır, `PanelHomeTests` kurulum listesi KK8/KK9) |
| `09 §25` doküman uyumu: yeni adlar `07`/`06`'dan; diff yalnız görev; yeni ayar/sabit yok | ✓ | Model alanları `07 §5.2` (sapma ⚠ (a), D-1333); `appsettings`/`.env.example` değişmedi; diff `Pages/Storefront`, `Modules/Ordering`, `Modules/Membership` ve testlerle sınırlı |
| `09 §25` mimari: modül sınırı, `SaveChanges` yok, tek işlev, `!`, `TimeProvider` | ✓ | Sayfa yalnız `Contracts`'a başvurur; modül diff'inde `SaveChanges`/işlem açma yok (işlem sayfada `RunAsync`); `OrderConfirmation` zamanı `TimeProvider`'dan; gerekçesiz `!` taraması yeşil (Unit TAM) |
| `09 §25` hata katmanı ve ret kaydı | ✓ | Adlı 422 retleri `RejectionLog.Rejected(code)` (`RefuseAsync`, `ApplyCoupon`); belirsiz sonuç `[OutcomeUncertain("UC-27")]`; geniş `catch` yok |
| `09 §25` `[GatedOperation]` | — | Uygulanmaz: ödeme adımı `OrderOperationCatalog` işlemi değil (kapı panel sipariş işlemlerinin); geçiş kapısı `ConfirmationStepRules` (KK2) |
| `09 §25` eşzamanlılık, para, veri/göç, sayfalı liste | ✓ | Yeni kilit yok: onay var olan sepet kilidini `Carts.OpenAsync` ile alır (KK10 testi); yeni para hesabı yok, tutarlar var olan sepet/sipariş hesabından; göç, yeni tablo, yeni sayfalı liste, yeni enum değeri yok (iki yeni enum saklanmaz, `ApplicationEnums.NotStored`) |
| `09 §25` güvenlik: `NoStore`, sahtecilik, hız sınırı, anahtar `SecretKey`'den | ✓ | `[NoStore]` + noindex; form `asp-antiforgery` (`Forgery` taraması yeşil); L-6/L-7/L-8; `ConfirmationKeys.NewKey()` → `SecretKey.Create()` |
| Güvenlik taraması | ✓ | Beş POST sahtecilik süzgecinden geçer; yabancı defter girdisi 404 (sahiplik); çerez yalnız belgeli alanlar, parmak izi e-posta/adres taşımaz ve sunucu özeti yeniden hesaplar; L-6/L-7/L-8 429 nötr mesaj; sayfa `[NoStore]` + noindex |

### ⚠ Kararlar (kurala en uygun seçenekle verildi)

- (a) `07 §5.2` `AcceptedCheckboxes` yerine T544 bileşeninin dört kutu adı (`AcceptPreInformation`, `AcceptDistanceSales`, `AcceptDigital`, `AcceptService`) kullanıldı → D-1333. Model bağlama adı taraması için bu dört alan ve `07 §5.2`'de kalın yazılan `SaveToAddressBook`, `IsBillingSameAsShipping`, `SummaryFingerprint`, `LeaveTarget` `[SpecAssignedName]` ile işaretlendi; kutu adları `07`'nin adı değil, bileşenin adıdır — D-1333 kapanınca işaret de kalkar ya da `07` adına döner (`PlaceOrderInput` sınıf özetinde not).
- (b) Çereze `AddressBookId` yazılıyor (teslimat adresi alanı sayıldı).
- (c) `SavedAddressName` `07`'de yok → boş verilir, defter adı alıcı adı olur (D-435 F5 gate'e).
- (d) Oturum kapandı tespiti: formda `ContactEmail` anahtarı yok ve üye yok.
- (e) Parmak izi istemciden gelir, güvenilmez ama zararsızdır: sunucu özeti yeniden hesaplar.
- (f) Değişiklik satırı metinleri `04`'te yalnız örnekle var; örneğe göre yazıldı.
- (g) Test derlemesindeki örnek sayfa da `/odeme` rotasını taşır; testlerde rota sırası (`AttributeRouteModel.Order`) ile ayrıldı.
- (h) İstemci adresi bulunamayan onay, limit dolmuş gibi nötr 429 alır; sipariş oluşmaz (D-805).
- (i) Girişten dönen müşterinin kuponu çerezden gelir ve GET okumasında yeniden değerlendirilir; bileşenin `CouponFields.AfterSignIn` yardımcısı kullanılmadı → D-1334.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Integration (T509 sınıfları) | ✓ | `--filter-class "Shopfolio.IntegrationTests.Web.CheckoutPageTests"`, `"*OrderAccessTests"`, `"*OutcomeUncertainConsistencyTests"`, `"*MembershipTests"` | 17/17; üçü birlikte 35/35; Membership 137/137; tarama düzeltmelerinden ve `main` birleştirmesinden sonra `CheckoutPageTests` yeniden 17/17 |
| Unit TAM | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` (`main` `a56a1735` birleştirmesinden sonra) | 7435/7435; `CheckoutFormsTests` dahil |
| Integration (değişen/ilgili parçalar) | ✓ | `--filter-class` parçaları (`main` `a56a1735` birleştirmesinden sonra) | Önceki turlarda ad alanı parçaları 350 + 209; birleştirmeden sonra `CheckoutPageTests` 17/17, `StorefrontFrameTests` yeşil, `OrderConfirmationTests` + `MainFlowTests` 29/29; `Ordering` ad alanının öteki 28 sınıfı 264/264 + 367/367; `OrderingTestServices` kullanan öteki sınıflar (`DataExportTests`, `PanelHomeTests`, `SalesSummaryTests`, `CardRefundJobTests`, `RequestsPageTests`) ve `main`'in yeni sayfa testleri (`OrderOperationScreenTests`, `PanelHomeAndVerificationPagesTests`, `SessionAccessTests`) 171/171 (`PanelHomeTests` düzeltmesinden sonra 25/25) |
| Integration TAM | ⏳ | — | Yönetici kararı (makine yükü, "Yük altında Integration TAM"): yapımda koşulmadı; **TAM doğrulayıcıda koşulacak** |
| `dotnet format` | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` (Web, Unit, Integration; son turda değişen 5 test dosyası `--include` ile) | Temiz (çıkış 0) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL (2026-11-12; dal ucu `42e1e6a7`, origin/main `4b37cf0c` birleşik) — build `-warnaserror`, format, Unit 7441/7441, betik 4/4, Integration parçalı 215/215 + 57/57, dal CI (run 43262776784) yeşil |
| Bulgu sayısı (tur 1) | 1 (1 ORTA, S1); K-notu yok |
| Düzeltme gerekli mi | Evet — B1 (ORTA, VAL-E061, D-1077, D-1080; kriter 1/UC-22): sağlayıcıya iki dakika erişilemeyen kart listeden çıkıyor; `04` 5.13.8 / `07 4.13.1` listede kalmasını, seçilemez olmasını ve yanında "şu an kullanılamıyor" yazmasını ister |
| Yapım raporu karşılaştırması (tur 1) | 1 uyuşmazlık: kriter 1 ✓ ve D-1077 "T509 payı kapandı" sayılmış; kartın iki dakikalık hâli sayfada çizilmiyor, testi yok |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T509-odeme-adimi`
- **Commit:** son baş SHA PR açıklamasında
- **PR:** #559
- **CI:** tur 1: run 43262776784 yeşil

## Dış Varsayımlar

Dış varsayım: yok. Kaynaklar (belge, gerçek PostgreSQL 18, ASP.NET Core test sunucusu) kendi depodan; ağ çağrısı yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok | — | — |
