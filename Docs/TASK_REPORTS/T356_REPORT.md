# T356 — Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-09

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Onay adımı kuralları:** `Ordering/ConfirmationStepRules.cs` (hangi kutular gerekli, eksik kutular, düğme metni), `ConfirmationStep.cs` (`IConfirmationStep`: görünüm ve denetim), `ConfirmationAcceptances.cs` (kutu kayıtları: işaretlenen kutu, gösterilen metin, kapsadığı kalemler). Sözleşmeler `Ordering/Contracts/ConfirmationStepContracts.cs`; `ConfirmationBox` enum değil, kapalı sınıf (dört kutu; enum kilit/tutarlılık testleri 06'ya bağlı olduğundan). `ConfirmationStepCheck(LegalTextVersionRaised, MissingBoxes)` sürüm artışını ve eksik kutuyu ayrı taşır.
- **Belge üretimi:** `Ordering/Documents/ConfirmationSummaryText.cs` (kalem satırları, tutar tablosu, cayma özeti, dijital ve hizmet kutusu metinleri) ve `OrderDocuments.cs` (`IOrderDocuments`: `Produce`, `GetTextAsync`; donmuş değerler ve kişisel değerler iki aşamalı doldurma). `PlaceholderFiller.Inert` çift ayracı etkisizleştirir.
- **Form içeriği (`02 §3.24.3`):** `PreInformationForm.tr.txt` il kısıtı, kargoya verme süresi (ödeme onayından), cayma koşulları ve süresi, firma kimliği, meslek odası, sabit iade ve istisna içeriği, hizmette ifa süresi, "sipariş nasıl kurulur" metni; geri ödeme başlangıcı teslimden önce ve hizmette beyandan. `LegalTextGenerator` meslek odası ve `HowTheOrderIsPlaced` değerlerini sürüm anında doldurur, değerleri etkisizleştirir (D-268 1–4).
- **İşlem rehberi:** `Settings/Contracts/ProcessGuide.cs` (dört bölüm, sabit metin, sayfa parçası `islem-rehberi`, altbilgi ve satış kapalıyken gösterim bayrakları); onay adımı aynı metni gösterir, form kendi parçası olarak taşır.
- **Mimari tarama:** `EmailRenderingSourceScan` belge klasörünü ve ad alanını da kapsar; ihlalli örnek `ViolationSamples/EmailRendering/DocumentsMisuse.cs` (D-167).

## Etkilenen Modüller / Dosyalar

`Ordering` (kurallar, adım, kayıtlar, belgeler, DI), `Settings` (şablonlar, üretici, ProcessGuide), `Infrastructure/PlaceholderFiller`, testler (`Ordering/ConfirmationStepTests` Unit ve Integration, `Architecture/EmailRenderingSourceScan*`).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki kutu, biri eksikse olmaz; rıza kutusu yok; düğmede ödeme yükümlülüğü (havalede de) | ✓ | Unit `MissingBoxes_OneBoxAloneIsNotEnough_...`, `TheButton_SaysThePaymentObligation_ForTheCardAndTheBankTransferAlike`, `TheBoxSet_IsClosedToTheFourContractBoxes_...`; Integration `TheCheck_StopsAnOrderWithOnlyOneOfTheTwoBoxes_..._KK1_KK2`, `TheView_CarriesTheButtonWithThePaymentObligation_ForBothMethods_..._KK1_KK5_...` |
| 2 | Dijital ve hizmet için ayrı kutu; gereken işaretsizse olmaz | ✓ | Unit `NeededBoxes_AreTheFormAndTheContractAlways_...`, `MissingBoxes_TheDigitalAndTheServiceBoxAreRequired_...`; Integration `TheCheck_Stops..._KK1_KK2` |
| 3 | Form özetle birebir; dört ek bilgi, sabit iade, istisna, ifa, kurulma metni, cayma özeti | ✓ | Unit `TheForm_AddsTheFourItemsTheFixedReturnTerms...`, `Render_OfTheSourceLines_EqualsRender_OfTheFrozenItems`, `TheWithdrawalSummary_*`; Integration `TheFormInTheView_IsTheSummaryFollowedByTheGeneratedText_AndEqualsTheOrdersFrozenCopyOnceTheOrderIsBorn_KK3` |
| 4 | Sürüm artışı siparişi durdurur | ✓ | Integration `ARisenFormVersion_StopsTheOrderBeforeTheBoxesAreLookedAt_..._KK4` |
| 5 | Yaş beyanı, doğum tarihi, üyelik sözleşmesi yok | ✓ | Unit `TheBoxSet_IsClosedToTheFourContractBoxes_AndNoStepRecordHasAConsentAgeBirthOrNoteField`; Integration `TheOrderTheCartAndTheCustomer_HaveNoAgeBirthDateNoteInstructionOrConsentColumn_..._KK5_KK8` |
| 6 | Misafir e-postası özette görünür, düzeltilebilir | ~ ⚠ | Görünür: Unit `TheEmailLine_ShowsTheGuestsAddress`, Integration `TheView_..._GuestsEmail_...`, `TheShownText_ReadsTheFirstOldValueOfACorrectedAddressAndEmail_...`; düzeltme bağlantısı ekran işi (T509); bkz. ⚠ a |
| 7 | Kutu kaydı kutuyu, metni, kalemleri taşır | ✓ | Unit `TheRecords_CarryTheDigitalAndTheServiceBox...`; Integration `TheBoxRecords_ProducedByTheStep_AreWrittenWithTheirTextAndItems_AndKeepTheirText..._KK7` |
| 8 | Not ve teslimat talimatı alanı yok | ✓ | Integration `TheOrderTheCartAndTheCustomer_HaveNoAgeBirthDateNoteInstructionOrConsentColumn_..._KK5_KK8` (şema) |
| 9 | İşlem rehberi altbilgide bağlantılı, sabit metin | ~ ⚠ | Unit `TheProcessGuide_HasTheFourInformationsOfTheSpec_...`, `TheProcessGuidePage_IsLinkedInTheFooterOfEveryPage_...`, `TheStepsFixedText_IsTheGuideText_...`; altbilgi bileşeni Pages'te, T540/T507 işi; bkz. ⚠ b |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–5, 7, 8 | ✓ | yukarıdaki tablo; diff `Ordering/ConfirmationStep*.cs` |
| Kriter 6, 9 (ekran parçası) | ✓ ⚠ | modül düzeyi kanıtlı; ekran yüzü sonraki task, D-729 / D-730 |
| KK1 | ✓ | Unit `TheButton_...`, Integration `TheView_CarriesTheButton...` |
| KK2 | ✓ | Integration `TheCheck_Stops...` |
| KK3 | ✓ | `TheForm_AddsTheFourItems...`, `Render_*`, `TheFormInTheView_...` |
| KK4 | ✓ | `ARisenFormVersion_...` |
| KK5 | ✓ | `TheBoxSet_IsClosed...`, şema testi |
| KK6 | ✓ ⚠ | e-posta özette (test); düzeltilebilirlik ekranda (D-729) |
| KK7 | ✓ | `TheRecords_*`, `TheBoxRecords_*` |
| KK8 | ✓ | şema testi |
| KK9 | ✓ ⚠ | sabit ve bayrak testleri; ana sayfa altbilgi bileşeni D-730 |
| `09 §25` | ✓ | Ordering yalnız sözleşmeler üzerinden Settings'e başvurur; mimari testler yeşil (Unit tam koşu) |
| Sık FAIL 1 (tarama istisnası geniş) | ✓ | belge taraması tam klasör ve ad alanı; ihlalli örnek `DocumentsMisuse.cs` |
| Sık FAIL 2 (çağrı biçimi kaçırma) | ✓ | tarama bütün biçimleri aynı kurala bağlıyor (`EmailRenderingSourceScanTests` yeni örnek) |
| Sık FAIL 3 (Varsayılan, FK dizini) | ✓ uygulanamaz | şema/göç yok |
| Sık FAIL 4 (aynı sayıyı anan yerler) | ✓ | enum yerine sınıf: enum kilit/tutarlılık/kaynak taraması testleri yeşil; belge ad alanı kapsamı iki yerde de güncel |
| Sık FAIL 5 (belge kuralı değişince karar kaydı) | ⚠ | 09 §9.3 madde 2 kümesi genişledi (meslek odası, rehber metni); metin 09'a yazılmadı, karar bekliyor: D-240 + D-731 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build` Web, UnitTests, IntegrationTests `-warnaserror` | 0 uyarı, 0 hata (son `origin/main` birleştirmesinden sonra) |
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 5302 testten 1 hata (kalem kimliği sırası: testin kendi hatası, kimlikler rastgele); düzeltildi, `*ConfirmationStepTests` 3 tekrar yeşil; öncesi tam koşu 5275/5275 |
| Integration | ✓ | `--filter-namespace "*Ordering*"` (369) · `--filter-class "*LegalText*"` (16) · `*ConfirmationStepIntegrationTests` (9) | 369/369 (birleştirme sonrası), 16/16, 9/9 |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (Web, UnitTests, IntegrationTests) | çıkış 0 |
| Contract | uygulanamaz | — | Uç yok |

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

- **Branch:** `task/T356-onay-adimi`
- **Commit:** son head PR'da
- **PR:** açıldı (bu PR)
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Task dış servis, plan, kota ya da paket kullanmaz.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Onay adımı ekranı: e-posta düzeltme bağlantısı, kutular, düğme | D-729 | Ekran T509 / T507'nin işi, Pages boş |
| 2 | Altbilgide "İşlem rehberi" bağlantısı ve sayfası | D-730 | Altbilgi bileşeni ve sayfa T540 / T507 |
| 3 | `09 §9.3` madde 2 belirteç kümesi (meslek odası, rehber metni, iade adresi) karar kaydı | D-731 (D-240 ile) | Belge kuralı değişikliği karar kaydı ister |
| 4 | T348 bağlama: `OrderDocuments.Produce` + `ConfirmationAcceptances.Records` çağrısı sipariş doğuşunda | D-732 | Sipariş doğuşu T348 |
| 5 | `ConfirmationBox` kapalı sınıf kararı (enum değil) | D-733 | Enum testleri 06'ya bağlı; karar kaydı |

## Notlar

- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** dokunulmadı (faz kapısı günceller)
- **⚠ a — kriter 6:** misafir e-postası görünümde ve düzeltme sonrası ilk eski değer okunuyor; "onaydan önce düzeltilebilir" bağlantısı ekranda (T509). Plan düzeltmesi gerekirse karar.
- **⚠ b — kriter 9:** modül düzeyi sabit ve bayrak; gerçek altbilgi bileşeni Pages'te olmadığı için ana sayfada görünme yalnız bayrak/sabit düzeyinde kanıtlı.
- **⚠ c:** `ConfirmationBox` enum yerine kapalı sınıf; belge kuralı kümeleri D-240 / D-731'de karar bekliyor. Karar verilmedi, kurala en uygun seçenekle ilerlendi.
- Ara hata: Unit testindeki kimlik sırası beklentisi rastgele Guid'e bağlıydı; iki tarafı da sıralayarak düzeltildi.
