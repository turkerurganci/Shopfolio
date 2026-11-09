# T356 — Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 2 PASS) | **Tarih:** 2026-11-09

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

### Düzeltme 1 (tur 1 bulgusu B1)

- **B1 düzeltildi:** `PlaceholderFiller.Inert` artık `Regex.Replace(value, @"\{(?=\{)", "{ ")` ile her ardışık açılış ayracını ayırır; `{{{Ad}}` → `{ { {Ad}}`, dört ayraç da sonuçta `{{` bırakmaz. Adım görünümü (`ConfirmationSummaryText.Render`) ve sipariş metni (`LegalTextGenerator` değerleri, ikinci doldurma) aynı tek işlevi çağırır. Yeni testler: `Inert_LeavesNoDoubleOpeningBrace_ForARunOfThreeOrFourBraces_...` (3 örnek, ikinci doldurma dahil), `Render_MakesAProductNameWithThreeOrFourBracesInert_...`. D-268 (4) kanıtı backlog satırında güncellendi.
- **Öz denetim (düzeltilmiş diff'te yeniden):** kriter 1–9 ve KK1–KK9 yukarıdaki tabloyla aynı kanıtla ✓ (6 ve 9 modül düzeyinde, D-729/D-730); diff yalnız `PlaceholderFiller`, `ConfirmationStepTests`, backlog satırı ve kayıt dosyalarına dokunur, öteki kriterlerin kodu değişmedi. Sık FAIL satırı "etkisizleştirme tek geçişli" ✓ (üç/dört ayraç örnekleri). Çözülmemiş çakışma yok, `git status` temiz.
- **Sınama (yerel, `postgres:18 -c track_commit_timestamp=on`):** restore 0; build `-warnaserror` 0 uyarı; format Web/UnitTests/IntegrationTests temiz; Unit 5306/5306; Integration `Ordering` 369/369; `*LegalText*` 16/16.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 2) — tur 1 ✗ FAIL |
| Bulgu sayısı | 0 (tur 1 B1 tur 2'de kapandı) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 yeni (tur 1 K1 → D-743 açık; K2 kayıt gerektirmez, D-729/D-730 var) |

### Doğrulama Sonucu — tur 2 (validator)

**Head:** `166e750b` (origin/main `02032971` ile güncel; birleştirme gerekmedi) · **PR:** #486 · **Verdict:** ✓ PASS

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = depo `scripts/git-hooks`; ana dal CI son 3: 42548005029, 42548005043, 42546628955 hepsi `success`.
- **Dal CI:** 42554853076 (`166e750b`) ✓ — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E (advisory), CI Gate `pass`.
- **B1 kapandı:** `PlaceholderFiller.Inert` `\{(?=\{)` → `"{ "` ile her ardışık açılış ayracını ayırır; `{{{X}}` → `{ { {X}}`, `{{{{X}}` → `{ { { {X}}`; sonuçta `{{` kalmaz, `TokenNames` boş. Çağıranlar: `LegalTextGenerator` firma değerleri ve iade adresi (sürüm anı), `ConfirmationSummaryText.Render` ürün adı + seçenekler (adım ve donmuş kopya aynı işlev). Şablonlarda yan yana iki belirteç yok (`}}{{` taraması boş), değer sınırından belirteç kurulamaz. Testler: `Inert_LeavesNoDoubleOpeningBrace_ForARunOfThreeOrFourBraces_...` (3 örnek, ikinci doldurma dahil), `Render_MakesAProductNameWithThreeOrFourBracesInert_...`. D-268 (4) ✅ artık doğru.
- **Kabul kriterleri (bütün task yeniden; kod + test okundu, testler koşuldu):** 1 ✓ · 2 ✓ · 3 ✓ (form = `Render` + doldurulan sürüm metni; dört ek bilgi, meslek odası, sabit iade/taşıyıcı/14 gün, istisna ve koşulu, ifa süresi, uyuşmazlık, "sipariş nasıl kurulur", ayrı cayma özeti) · 4 ✓ · 5 ✓ · 6 ✓ (modül payı; ekran D-729) · 7 ✓ · 8 ✓ · 9 ✓ (modül payı; sayfa ve altbilgi D-730 — Pages'te düzen/altbilgi bileşeni yok). KK1–KK9 ✓.
- **`09 §25`:** Ordering, Settings'e yalnız `Contracts` üzerinden; arayüz yöntemleri işlem açmaz; şema/göç yok; yeni paket yok; zaman okuması yok; belge klasörü kaçış taramasında (D-167), ihlalli örnek kırmızı; doküman çelişkisi yok.
- **Task satırları:** `VAL-A020`, `A026`, `A027`, `A047`–`A052`, `A986`, `A129`, `A130` tarayıcı gözlemi / uç yanıtı / verilmiş sipariş ister; T356 ekran, uç ve sipariş doğuşu eklemez (D-729, D-730, D-732). `Durum` hücreleri `BEKLEMEDE` kaldı (tur 1 ve T355, T357 emsali); matrise dokunulmadı.
- **Testler (yerel, `postgres:18 -c track_commit_timestamp=on`):** restore 0; build `-warnaserror` 0 uyarı; format Web/UnitTests/IntegrationTests temiz; Unit 5306/5306; betik 4/4; Integration `Ordering` 369/369; `Model.LegalTextTests` + `CompanyIdentityTests` + `SalesGateTests` + `SettingsTablesTests` 90/90.
- **Güvenlik:** sır yok (diff'teki `Html.Raw`/`MarkupString`/`HttpContext` yalnız ihlalli örnekte); yetki etkisi yok (uç yok); girdi: belge metni ham, kaçış gösterim kanalında, değerler etkisiz; yeni bağımlılık yok.
- **Yapım raporu karşılaştırması:** tam uyumlu (düzeltme 1 bölümü B1'i doğru anlatır; kriter 6 ve 9 "~" yerine validator modül payını ✓ sayar — tur 1 K2 ile aynı). Öz denetim tablosu var ve kanıtlı.

### Doğrulama Sonucu — tur 1 (validator)

**Head:** `0663fc3d` (origin/main `02032971` ile güncel; birleştirme gerekmedi) · **PR:** #486 · **Verdict:** ✗ FAIL

- **Başlangıç kapıları:** working tree temiz; `core.hooksPath` = depo `scripts/git-hooks`; ana dal CI son 3: 42548005029, 42548005043, 42546628955 hepsi `success`.
- **Dal CI:** 42550296580 (`0663fc3d`) ✓ — Lint, Build, Unit, Integration, Contract, Migration dry-run, Paketleme, E2E (advisory), CI Gate `pass`.
- **Kabul kriterleri (kod + test okundu, testler koşuldu):** 1 ✓ (iki sabit kutu her sepette, `MissingBoxes` eksik kutuyu sayar, rıza kutusu yok, düğme metni ve yanındaki ibare iki yöntemde aynı) · 2 ✓ (dijital ve hizmet kutusu sepete göre; işaretsizse `IsReady` false) · 3 ✓ (form = `ConfirmationSummaryText.Render` + doldurulan sürüm metni; adım ve donmuş kopya aynı işlevden; dört ek bilgi, meslek odası, sabit iade, istisna ve koşulu, ifa süresi, "sipariş nasıl kurulur" ve kısa cayma özeti var; B1 bu kriterin değil `09 §9.3` madde 3'ün sapmasıdır) · 4 ✓ (sürüm kimliği güncel değilse kutulara bakılmadan durur) · 5 ✓ · 6 ✓ (modül düzeyi: e-posta satırı görünümde, düzeltilmiş özetle yeniden üretilir; ekran bağlantısı T509, D-729) · 7 ✓ (kutu, gösterilen metin, kapsanan kalemler; gerçek veritabanına yazılır) · 8 ✓ (şema ve kayıtlar not alanı taşımaz, uç yok) · 9 ✓ (modül düzeyi: sabit metin, sayfa parçası `AddressRules`'ta, bayraklar; altbilgi bileşeni ve sayfa yok — T540/T507, D-730; `11 §7.1` `§3.24.8`'i T507 ile paylaştırır).
- **KK1–KK9:** ✓ (kriterlerle aynı kanıt). **`09 §25`:** Ordering, Settings'e yalnız `Contracts` üzerinden başvurur; arayüz yöntemleri işlem açmaz; şema/göç yok; yeni paket yok; zaman okuması yok; belge klasörü kaçış taramasında (D-167). "Kod hiçbir doküman cümlesiyle çelişmiyor mu" sorusu B1 yüzünden **hayır**.
- **Task satırları:** T356'nın KP satırları (`VAL-A020`, `A026`, `A027`, `A047`–`A052`, `A986`, `A129`, `A130`) tarayıcı gözlemi / uç yanıtı / verilmiş sipariş ister; T356 ekran, uç ve sipariş doğuşu eklemez (D-729, D-730, D-732). `Durum` hücreleri `BEKLEMEDE` kaldı (T355, T357 emsali), matrise dokunulmadı.
- **Testler (yerel, `postgres:18 -c track_commit_timestamp=on`):** restore 0; build `-warnaserror` 0 uyarı; format Web/UnitTests/IntegrationTests temiz; Unit 5302/5302; betik testleri 4/4; Integration `Ordering` 369/369; `Model` 1392/1393 — tek hata `CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled_…` okuma zaman aşımı (yük altında), sınıf tek başına 60/60; T356 göç ve katalog kodu değiştirmez, CI'da tam Integration yeşil.
- **B1 — DÜŞÜK, S1 Sapma (`09 §9.3` madde 3 "değer yeniden taranmaz"; D-268 (4)):** `PlaceholderFiller.Inert` `value.Replace("{{", "{ {")` ile tek geçişte çalışır; tek sayıda ardışık açılış ayracı bir `{{` bırakır: `"{{{ContactEmail}}"` → `"{ {{ContactEmail}}"` (.NET `String.Replace` ile denendi). Ürün adı ya da firma kimlik değeri (unvan, adres …) `{{{ShippingPhone}}` gibi bir dize taşırsa ikinci doldurmada (adımda `GetViewAsync`, siparişte `GetTextAsync`) müşterinin kişisel değeri metne açılır ve `Render`'ın "the order stage cannot expand it" ile `Inert`'in "a double opening brace gets a space" sözü tutmaz; D-268 (4) ✅ erken kapatıldı. Testler (`Inert_*`, `Render_MakesAProductName…`) yalnız iki ayraçlı dizeyi sınıyor. Düzeltme: etkisizleştirme sonuçta hiç `{{` bırakmaz (ör. her `{` ardından gelen `{` öncesine boşluk — `Regex.Replace(value, @"\{(?=\{)", "{ ")` ya da sonuç `{{` içermeyene kadar), testlere `{{{Ad}}` ve `{{{{Ad}}}}` örnekleri eklenir.
- **K1 → D-743:** kısa cayma özeti fiziksel kalemi olmayan sepette de fiziksel ürün ve iade kargo cümlelerini yazıyor; düzeltme turunda kapatılabilir.
- **K2:** kriter 6 ve 9 modül düzeyinde kanıtlı, ekran ve altbilgi parçası D-729 (T509) ve D-730 (T540 · T507) kayıtlarında; yapım raporu bunları "~" diye işaretledi, validator `11 §7.1` paylaşımına dayanarak modül payını ✓ saydı. Yeni kayıt gerekmez.
- **Güvenlik:** sır yok; yetki etkisi yok (uç yok); girdi: B1 dışında belge metni kaçışsız ham metin, kaçış gösterim kanalında (`09 §9.3` madde 6), belge klasörü taramada; yeni bağımlılık yok.
- **Yapım raporu karşılaştırması:** 2 uyuşmazlık — (a) rapor D-268 (1–4)'ü kapanmış sayar, (4) tam değil (B1); (b) rapor kriter 6 ve 9'u "~" yazar, validator modül payını ✓ saydı (K2). Öz denetim tablosu var ve kanıtlı.

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
