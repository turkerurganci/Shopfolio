# T352 — Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Kart hattı bağı (D-81, D-198):** `CardPaymentCondition` (Payment modülü) açılışta hesaplanan `IReadOnlyList<ConditionalLine>` tekilindeki `CardPayment` hattını okur; kendi denetimini yazmaz. Payment modülünün kaydı `ICardPaymentCondition`'ı Settings'in `TryAdd` varsayılanının yerine bağlar. `AddConditionalLines` `TryAddSingleton` oldu ve Payment kaydı da onu çağırır: önce kaydeden (web ve komut kipi, seçenekler bağlı) kalır.
- **İki yöntem (02 §3.21.2, §3.21.4):** `IPaymentMethods.ListOpenAsync` (Payment sözleşmesi, ayrı kapsamlı hizmet; modül kurucusu değişmedi) açık yöntemleri sabit sırayla verir: kart hattı açıksa kart, havale hattı açıksa havale. Kural `PaymentMethodRules.Open`'da; kapı ile aynı iki koşulu okur.
- **Seçilemeyen yöntemin reddi (07 §4.13.6):** `OrderConfirmation.ConfirmAsync` sepetin kilidinden sonra, değerlendirmeden önce yöntemi açık yöntemlerle karşılaştırır; açık değilse `OrderConfirmationRefusal.PaymentMethodUnavailable` döner, hiçbir şey doğmaz, önceki siparişin iptali geri alınır, sepet durur.
- **Taramalar (KK1, KK3, KK5, KK7):** uygulamanın bütün tip, özellik, alan, yöntem, parametre (kayıt yöntemleri dahil) ve enum değeri adları kart verisi, taksit, 3D'yi atlayan anahtar/eşik ve ters ibraz adları için taranır; sabit metinler `ters-ibraz`/`chargeback` rotası için taranır. Her taramanın ihlal örneği (`ViolationSample`) aynı işlevle bulunur.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Payment/PaymentMethods.cs` (yeni), `Modules/Payment/Contracts/IPaymentMethods.cs` (yeni), `Modules/Payment/PaymentModule.cs` (kayıt)
- `src/Shopfolio.Web/Modules/Ordering/OrderConfirmation.cs` (red + bağımlılık)
- `src/Shopfolio.Web/Infrastructure/InstallationSettings/InstallationSettingsRegistration.cs` (`TryAddSingleton`)
- `src/Shopfolio.Web/Modules/Settings/SettingsModule.cs`, `UnboundSalesGateConditions.cs`, `Contracts/ISalesGateConditions.cs` (yorumlar)
- Testler: `tests/Shopfolio.UnitTests/Payment/PaymentMethodTests.cs` (yeni), `tests/Shopfolio.IntegrationTests/Ordering/OrderConfirmationTests.cs`, `OrderingTestServices.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Satıcı firmadır; kart sağlayıcının sayfasında girilir; ürün kart verisini görmez, saklamaz, loglamaz | ✓ | `PaymentMethodTests.Application_NamesNoCardData_InAnyRecordColumnOrLogParameter_KK1`, `CardDataScan_FindsEachKindOfName_InTheSample_KK1`, `PaymentStartRequest_CarriesNoCardData_TheCardIsEnteredOnTheProvidersForm_KK1` |
| 2 | Yalnız kart ve havale; kapıda ödeme yok | ✓ | `PaymentMethod_HasExactlyTheCardAndTheBankTransfer_NoCashOnDelivery_KK2`, `OpenMethods_AreTheCardWhenItsLineIsOpen_AndTheTransferWhenItIsOpen_KK2_KK4` |
| 3 | Her kart ödemesi 3D'den geçer; başarısızsa ödenmiş sayılmaz; eşik/takdir yok | ✓ | `Application_HasNoSwitchThatSkips3DSecure_AndNoAmountThreshold_KK3` + örnek; var olan `PaymentNotificationRuleTests.Failure_IsProcessed_WithoutAnAmountCheck_Row5`; `UnpaidCardOrderCancelledBySystem_..._KK6_KK8(CardPaymentFailed)` (Ö2: İptal + Başarısız) |
| 4 | Kartın panel anahtarı yok; anahtarsız kurulumda görünmez, havale açıksa satış yalnız havaleyle; hiçbiri açık değilse kapı T331'de | ✓ | `CardCondition_IsOpen_OnlyWhenTheCardLineIsOpen_KK4`, `PaymentMethods_WithoutTheProviderKeys_OfferOnlyTheTransfer_KK4`, `KeylessInstallation_OffersOnlyTheTransfer_RefusesTheCard_AndOpensTheGateWithTheTransfer_T352_KK2_KK4`, `NoOpenMethod_ClosesTheGate_AndASwitchedOffTransferIsRefusedWhileTheCardIsTaken_T352_KK2_KK4` |
| 5 | Ürün taksit bilmez; sipariş tek tutar | ✓ | `Application_NamesNoInstallment_InTheModelOrTheScreensData_KK5` + örnek, `Order_CarriesOneAmount_NoInstallmentPlan_KK5`; sağlayıcı ekranının taksiti açık kalır → D-786 (T401: `no_installment`/`max_installment` = 0) |
| 6 | Hazırlığa ancak ödeme onayıyla | ✓ | `Preparing_StandsWithAnUnapprovedPayment_Never_KK6`; integration'da ödemesiz sipariş Alındı + Bekliyor kalır (`UnpaidCardOrderCancelledBySystem_...`) |
| 7 | Ters ibraz dışarıda; bilgi alınmaz, eksen değişmez | ✓ | `Application_HasNoChargebackEndpointStateOrField_KK7` + örnek, `PaymentAxis_HasOnlyItsFiveStates_NoneForAChargeback_KK7` |
| 8 | Yarım ödemede ve sağlayıcı kesintisinde sepet durur | ✓ | `UnpaidCardOrderCancelledBySystem_LeavesTheCartAsItIs_AndOnlyTheApprovalEmptiesIt_T352_KK6_KK8` (CardPaymentFailed, PaymentPageNotOpened, PaymentWindowExpired); reddedilen onayda sepet satırı durur (`KeylessInstallation_...`) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 kart verisi kayıtta, veritabanında, günlükte yok | ✓ | Ad taraması varlık özelliklerini (sütunlar), kayıt yöntemlerinin parametrelerini, yük ve sözleşme tiplerini kapsar; `PaymentMethodTests` KK1 üç testi |
| KK2 yalnız iki yöntem | ✓ | `PaymentMethod` iki değer; `PaymentMethodRules.Open`; onay kapalı yöntemi reddeder |
| KK3 3D başarısızsa onay yok, eşik yok | ✓ | 3D/eşik ad taraması + örnek; başarısız bildirim tutar denetimsiz işlenir ve Ö2'ye gider (T311/T317 testleri) |
| KK4 anahtarsız kurulumda kart görünmez; havale açıksa yalnız havale | ✓ | `CardPaymentCondition` + `IPaymentMethods`; kapının `PaymentMethod` koşulu gerçek bağla integration'da |
| KK5 vitrin/ürün/sepet/özet ve modelde taksit yok; sağlayıcı ekranındaki taksit kapatılmaz | ✓ | Taksit ad taraması (bütün uygulama) + `Order` tek tutar; sağlayıcı payı D-786 → T401 |
| KK6 onaysız hazırlığa geçiş reddedilir | ✓ | `TransitionTable.IsPairAllowed(Preparing, Pending/Failed)` yanlış; DB `status_pair_allowed` aynı (T317) |
| KK7 ters ibraz için uç, durum, alan yok | ✓ | Ad + rota sabiti taraması, `PaymentStatus` beş değer |
| KK8 yarım ödemede ve sağlayıcı kesintisinde sepet durur | ✓ | Üç sistem iptali sebebiyle integration testi; sepet yalnız Ö1'de boşalır (`EmptyCartOfOrderedItemsAsync`) |
| `09 §25` tek kural evi | ✓ | Kart hattı `ConditionalLines`'tan okunur (yeni denetim yok); yöntem kuralı tek işlevde (`PaymentMethodRules.Open`), kapı ve onay aynı iki koşulu okur |
| `09 §25` modül sınırı / kurucu | ✓ | Ordering yalnız `Payment.Contracts.IPaymentMethods`'u kullanır; `PaymentModule` kurucusu değişmedi; `ModuleBoundaryTests` yeşil |
| `09 §19.2 m.3` taramanın ihlal örneği | ✓ | Her tarama için `ViolationSample` testi |
| Devredilen backlog (D-81, D-198) | ✓ | İkisinin T352 payı yerinde kapatıldı |
| Sık FAIL (1) geniş istisna | ✓ | Tarama istisnası yok; yalnız kısa adlar tam eşleşmeyle (`pan`, `expiry`…) — `ExpiryDated` (cayma istisnası) yanlış pozitifti, tam eşleşmeye alındı |
| Sık FAIL (3) DB default / FK | ✓ | Şema değişikliği yok |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | `ISalesGateConditions`, `UnboundSalesGateConditions`, `SettingsModule` yorumları güncellendi |
| Sık FAIL (5) karar kaydısız belge değişikliği | ✓ | Belge kuralı değişmedi |
| Güvenlik taraması | ✓ | Sır/kart verisi yazılmıyor; yeni uç yok; yeni dış bağımlılık yok; yöntem denetimi sunucuda |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 5439/5439 (yeni `PaymentMethodTests` 22) |
| Integration | ✓ (kısmi koşu ⚠) | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release --filter-namespace/--filter-class …` | Ordering 386, Model 1393, Commands 70, Payment 14, Catalog+CorporateContent+Concurrency+Disposal 323, Web (açılış/kayıt/durum sınıfları) 49 — hepsi geçti; Database, Files, FileSecrets, Jobs, Logging, Outages, Pages, Security, SignedEndpoints ve Web'in kalanı süre bütçesi yüzünden yerelde koşulmadı (CI tam koşar) |
| Contract | — | NOT_APPLICABLE | — |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --include <değişen dosyalar>` | üç proje temiz |
| Build | ✓ | `dotnet build -c Release -warnaserror` (Web, Unit, Integration) | 0 uyarı |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (D-795) |

### Doğrulama tur 1 — Doğrulama Sonucu — T352 Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz — tur 1
**Tarih:** 2026-11-09   **Branch:** task/T352-odeme   **Commit:** `10da8ed7` (doğrulanan dal ucu; `origin/main` `5f9b4c43` ile aynı taban, birleştirme gerekmedi), PR #491

**Verdict: ✓ PASS**

#### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook | ✓ | `core.hooksPath` ana çalışma kopyasının `scripts/git-hooks`'u |
| Ana dal CI (son 3) | ✓ | 42578567856 success, 42578567859 success, 42570905276 success |

#### Kabul Kriterleri (doğrulayıcı kanıtı)
| # | Sonuç | Kanıt |
|---|---|---|
| 1 | ✓ | `PaymentMethodTests` KK1 üç testi (ad taraması + ihlal örneği + `PaymentStartRequest` alan listesi); elle: `PaymentNotice`, `InboundNotificationRecord` kart alanı taşımaz; `src/`'de `masked_pan`/`cvv`/`card_number` yok; Payment modülünün tek kayıt satırı `{Event} {Reason} {RecordId}`; göçlerde kart sütunu yok |
| 2 | ✓ | `PaymentMethod_HasExactlyTheCardAndTheBankTransfer_NoCashOnDelivery_KK2`, `OpenMethods_..._KK2_KK4`; onay kapalı yöntemi `PaymentMethodUnavailable` ile reddeder (07 §6 kodu var) |
| 3 | ✓ | 3D/eşik ad taraması + örnek; başarısız bildirim onaylamaz (`PaymentNotificationRuleTests.Failure_IsProcessed_WithoutAnAmountCheck_Row5`, `PaymentNotificationTests.Failure_CancelsTheUnpaidOrderInTheJob_...`); `CardPaymentFailed` → İptal + Başarısız. Sağlayıcıda kapatılamazlık kurulum şartı (`08 §2.1.3`, T406) |
| 4 | ✓ | `CardCondition_*_KK4` (iki), `PaymentMethods_WithoutTheProviderKeys_OfferOnlyTheTransfer_KK4`; tümleşik `KeylessInstallation_..._T352_KK2_KK4`, `NoOpenMethod_ClosesTheGate_..._T352_KK2_KK4`; kart açık/kapalı ayar alanı yok |
| 5 | ✓ | Taksit ad taraması + örnek, `Order_CarriesOneAmount_NoInstallmentPlan_KK5`; ekran dosyalarında "taksit" yok; sağlayıcı payı D-786 (T401) |
| 6 | ✓ | `Preparing_StandsWithAnUnapprovedPayment_Never_KK6`; `OrderTransitionsTests.IsPairAllowed_EqualsTheCombinationRuleOfTheDatabase_ForAllPairs`, `CartAndOrderCoreTablesTests.PendingPaymentWithAPreparingOrder_IsRefused` |
| 7 | ✓ | `Application_HasNoChargebackEndpointStateOrField_KK7` + örnek, `PaymentAxis_HasOnlyItsFiveStates_NoneForAChargeback_KK7`; `src/`'de `chargeback`/`ibraz` yok |
| 8 | ✓ | `UnpaidCardOrderCancelledBySystem_..._T352_KK6_KK8` (üç sistem iptali sebebi; sepet adedi durur, ayrılan 0); reddedilen onayda sepet satırı durur |

#### Task Satırları (`12 §7.5`)
KP-15 → 10 satır (`VAL-B008`…`B012`, `C001`, `C002`, `A053`, `D003`, `D004`). Hiçbiri T352 kanıtıyla tamamlanmaz: uç yanıtı ya da tarayıcı gözlemi ister (T509/T510, sağlayıcı katmanı T401) ya da başka task'ın kaynağıdır (B009 T353; C002/A053 geç ödeme). `Durum` değişmedi (C001 PASS kalır, ötekiler BEKLEMEDE). `12_matris_denetle.py --kapsam 1-77` Hata 0.

#### Test Sonuçları
| Tür | Sonuç | Komut / kapsam |
|---|---|---|
| Build | ✓ 0 uyarı | `dotnet build Shopfolio.slnx --no-restore -warnaserror` |
| Biçim | ✓ | `dotnet format <proje> --verify-no-changes --no-restore` ×3 |
| Unit | ✓ 5439/5439 | TAM koşu |
| Betik | ✓ | `tests/scripts/run-script-tests.sh` 4 dosya |
| Integration | ✓ 754/754 | Ordering 395, Payment 14, `SalesGateTests`+`AccountAndStatusCommandTests`+`StartupSettingsCheckTests` 83, Catalog+CorporateContent+Concurrency 262 |
| Dal CI | ✓ | run 42582347076 (head `10da8ed7`) success — Integration tam koşu dahil |

#### Güvenlik Kontrolü
Secret sızıntısı temiz · auth etkisi yok (yeni uç yok) · girdi doğrulaması: seçilen yöntem sunucuda açık yöntemlerle karşılaştırılır · yeni bağımlılık yok.

#### Bulgular
Yok.

#### Bloke-etmeyen Notlar
| # | Not | Karar |
|---|---|---|
| K1 | `UnpaidCardOrderCancelledBySystem_..._AndOnlyTheApprovalEmptiesIt_...` adı onayın sepeti boşalttığını söyler, gövde bunu sınamaz | D-795 (`DEFERRED_BACKLOG.d/T352.md`), hedef T353 |

#### Yapım Raporu Karşılaştırması
Tam uyumlu; öz denetim tablosu var ve kanıtlı. Farklar: K1 yapım raporunda yok; Ordering koşu sayısı 386 ↔ 395 (hepsi geçti). ⚠ (1) yerinde: havale hattı T353'e dek bağsız, üretimde havale onayı da reddedilir; kapı kapalı ve onayı çağıran uç yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (`AddConditionalLines` kayıt biçimi `TryAddSingleton`)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T352-odeme`
- **Commit:** `1f8220c4` — T352: kod ve testler
- **PR:** #491
- **CI:** run 42582347076 (head `10da8ed7`) success
- **Post-merge run'lar:** squash `30d43ba` (#491) — CI 42586725547 success, Publish 42586725445 success (F3 gate'te dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok. Sağlayıcıya çağrı bu task'ta yok; 3D'nin sağlayıcıda kapatılamazlığı kurulum şartıdır (`08 §2.1.3`, T406) | `08 §2.1.3` madde 1 | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ödeme adımı sayfası açık yöntemleri göstermeli, UC-27 422 eşlemesi | D-785 | Sayfa T509'un |
| 2 | Jeton isteğinde `no_installment`/`max_installment` = 0 | D-786 | Sağlayıcı katmanı T401'in |
| 3 | Sipariş testlerinin havale sahtesi T353 bağıyla değişmeli | D-787 | `IBankTransferCondition` T353'te bağlanır |

## Notlar

- **Working tree:** temiz. **Hook:** `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI (Adım 0):** 42578567856 success, 42578567859 success, 42570905276 success.
- ⚠ **Kolay kararlar:** (1) Havale hattı yöntem listesine `IBankTransferCondition`'dan girer; T353'e dek bağsız olduğu için üretimde havale onayı da reddedilir — kapı zaten kapalı olduğundan satış etkilenmez. (2) Yöntem denetimi onayın sırasında sepetin kilidinden ve "girebilecek kalem yok" denetiminden sonra, özet değerlendirmesinden önce (07 §4.13.6: kapılar değerlendirmeden önce). (3) Integration yerelde kısmi koşuldu (yukarıda); değişikliğin dokunduğu bütün koleksiyonlar (`AddPaymentModule` kullananlar, Ordering, Model, Commands, web açılışı) koşuldu.
