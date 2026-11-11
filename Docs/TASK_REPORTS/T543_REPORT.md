# T543 — Form alanı ve düzeni, adres formu, IBAN alanı, sebep seçimi ve ortak alan kuralları (OB-12…OB-15)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Alan parçası:** `FieldModels.cs` (`FieldView`, `Fields`, `ClosedLists`, `FormSectionView`, `FieldRequirements`), `_Field`, `_FormSection`; görünür etiket + "(zorunlu)" / "(isteğe bağlı)" metni, mesaj alanın altında ve `aria-describedby` ile bağlı, kapalı liste `<select>` (metin kutusu ve `datalist` yok), `maxlength` çağıranın ürün sabitinden.
- **Adres formu:** `AddressModels.cs` (`AddressForms`, `AddressVariant`, `ProvinceVerdict`, `AddressMessages`), `_AddressForm`; teslimat, fatura, defter, panel varyantları; posta kodu ve T.C. kimlik no alanı yok; teslimat varyantında kapalı il "Bu firma Ankara iline teslimat yapmıyor." ile reddedilir, defter varyantı tutar ve ödemede seçilemez (`AddressBookChoice`).
- **IBAN alanı:** `IbanModels.cs` (`IbanFields`), `_IbanField`; yalnız havale hattı ve iki istisna (kart iadesi gerçekleşmedi, geç gelen ödeme); istisnada sipariş sayfasında açılır ve isteğe bağlı; işlenince salt okunur.
- **Sebep seçimi:** `ReasonModels.cs` (`Reasons`, `ReasonCheck`), `_ReasonSelect`; sebep kapalı listeden, sebepsiz onay açılmaz, "diğer" açıklamayı zorunlu yapar.
- **Stil:** `components.css` sonu; form tek sütun, `.form-fields--paired` yalnız `@media (min-width: 768px)`.
- **Devam turunda:** dört kırmızı birim test düzeltildi (`ColumnViolations` taraması basitleştirildi ve arkaya dönük eşleşme hatası giderildi; `_Field`'de `required` değişken adı tarama yanlış alarmı verdi, `ariaRequired` yapıldı; `ThrowsAny`); `FormSourceScanTests` örnekleri `ForbiddenTestPatternScanTests` kuralına takılmasın diye bölündü; `_FormSection` işleyiciyi `<form asp-page-handler>` ile söyler (`PageSourceScanTests` 07 2.2.1: her POST formu işleyicisini adlandırır; düğmedeki `name="handler"` kaldırıldı).
- **Kapsam dışı bırakılan:** "teslimat adresimle aynı" açıp kapama betiği (D-1179), firma iptali onay cümlesi ve yasal uyarı çizimi (D-1182).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Components/{FieldModels,AddressModels,IbanModels,ReasonModels}.cs`, `Pages/Shared/_{Field,FormSection,AddressForm,IbanField,ReasonSelect}.cshtml`, `wwwroot/css/components.css`
- `tests/Shopfolio.UnitTests/Pages/FormComponentModelTests.cs`, `Architecture/FormSourceScanTests.cs`
- `tests/Shopfolio.IntegrationTests/Web/FormComponentTests.cs`, `Pages/FormSamples.cs`, `Pages/Storefront/FormSample.cshtml`
- `Docs/DEFERRED_BACKLOG.d/{T541,T543}.md` (D-1148 ✅; D-1179–D-1183), `Docs/STATUS_CHANGELOG.d/T543.md`, `Docs/IMPLEMENTATION_STATUS.md` (T543 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Etiket, zorunluluk metni, mesaj alanın altında ve etikete bağlı | ✓ | `FormComponentTests.EveryField_HasAVisibleLabel*_KK1`, `TheMessageOfAWrongField_*_KK1`; `FormSourceScanTests.TheFieldView_ReadsTheFieldsRequirementInWords_*_KK1` |
| 2 | Tek sütun; yazılanlar silinmez; bölüm başına kendi düğmesi | ✓ | `FormSourceScanTests.TheForm_IsOneColumnByDefault_*_KK2` (+ 4 ihlal, 1 sessiz örnek), `FormComponentTests.ASectionedForm_*_KK2`, `TheDeliveryAddress_Refuses*_KK6` (değerler kalır) |
| 3 | Kapalı liste serbest metin kabul etmez | ✓ | `FormComponentTests.AClosedList_IsASelect*_KK3`, `FormSourceScanTests.TheProvinceAndTheReason_*_KK3` (+ ihlal ve sessiz örnekler), `FormComponentModelTests.AReasonTypedAsText_*_KK3` |
| 4 | Baştaki ve sondaki boşluk yok sayılır; yalnız boşluk boştur | ✓ | `FormComponentTests.ASpaceAroundAValue_*_KK4`, `FormComponentModelTests.AReasonOfWhiteSpaceOnly_*_KK4` |
| 5 | Adres alanları; posta kodu ve T.C. kimlik no yok | ✓ | `FormComponentTests.TheAddressForm_Has*_KK5`, `FormSourceScanTests.TheAddressForm_HasNoPostalCode*_KK5` (+ ihlal örnekleri) |
| 6 | Teslimatta kapalı il reddedilir (sebep yerinde), defterde tutulur, ödemede seçilemez | ✓ | `FormComponentTests.TheDeliveryAddress_Refuses*_KK6`; `FormComponentModelTests` (`CheckProvince` varyantları, `AddressBookChoice`) |
| 7 | IBAN yalnız havale ve iki istisna; istisnada isteğe bağlı; işlenene kadar düzeltilir | ✓ | `FormComponentTests.TheIbanField_IsOnlyOnTheTransferLine_*_KK7` (7 durum), `TheIbanFieldOfAnException_IsOptional_*_KK7` |
| 8 | Sebep kapalı listeden; sebepsiz onay açılmaz; "diğer" açıklamayı zorunlu yapar | ✓ | `FormComponentTests.AFirmCancellationWithoutAReason_*_KK8`, `FormComponentModelTests` (`Reasons.Check`) |
| 9 | Limit yalnız o işlemi engeller; dil nötr | ✓ | `FormComponentTests.TheLimit_BlocksOnlyItsOwnAction_*_KK9` |
| 10 | Sınır değeri mesajda ve `maxlength`'te ürün sabitinden; bileşen yazmaz | ✓ | `FormComponentTests.TheCharacterLimit_IsTheProductConstant_*_KK10`, `FormSourceScanTests.TheFormComponents_TypeNoLimit_KK10` (+ ihlal ve sessiz örnekler) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1–KK10 (yukarıdaki tablo) | ✓ | Önceki işçinin yazdıkları dahil bütün maddeler diff'te yeniden denetlendi; tarama ve işleyici adı tutarsızlıkları (yukarıda) bulunup giderildi |
| `sik-fail-nedenleri` (1) tarama istisnası geniş | ✓ | `ColumnViolations` yalnız `@media (min-width…)` bloklarını çıkarır; `max-width` bloğu, bloksuz çok sütun ve yalnız `display: grid` için ayrı ihlal örneği |
| (2) kaynak taraması çağrı biçimini kaçırıyor | ✓ | Kapalı liste taraması çok satırlı ve adlı argümanlı çağrıyı kapsar (ihlal örneği); `required` taraması öznitelik biçimini arar |
| (3) `06` Varsayılan / FK dizini | ✓ (uygulanmaz) | Şema değişikliği yok |
| (4) kural değişince öteki yerler | ✓ | `_FormSection` işleyici değişimi `FieldModels.cs` belge yorumu, parça yorumu ve entegrasyon testinde aynı; D-1148 kapandı |
| (5) belge kuralını değiştiren 09 metni | ✓ | `09` değişmedi; belgede olmayan metinler D-1180, `required` kararı D-1183'e `doc-drift` yazıldı |
| `12` tarayıcı kanıtı isteyen satırlar | ~ BEKLEMEDE | Dar sınıf tek sütun CSS kaynağı düzeyinde sınandı; gerçek tarayıcı kanıtı F5 tarayıcı turuna kalır, PASS sayılmadı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ (tam) | `dotnet test --project tests/Shopfolio.UnitTests` | 7016 test, 0 kırmızı |
| Integration | ✓ (Web ad alanı) | `--filter-namespace Shopfolio.IntegrationTests.Web` (Postgres 18, `track_commit_timestamp=on`) | Web 680, 0 kırmızı (`FormComponentTests` 18); öteki ad alanlarına dokunulmadı (yalnız yeni dosyalar ve CSS) |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, UnitTests (using sırası düzeltildi), IntegrationTests temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✓ PASS — tur 1 (2026-11-11, head `01411663`; `origin/main` `f40f3a9d` (T545 #538) birleşik ağaçta, birleştirme çakışmasız). Kriter 1–10 ve KK1–KK10 ✓; `09 §25` ilgili satırlar (adlar `06`/`03` listeleriyle birebir — `CancellationReason`, `CorrectionReason` metinleri `03 §8.3.1.1`, §8.4.5 ile aynı; `§16` betiksiz form kalıbı, formlarda antiforgery, `Html.Raw`/satır içi betik/stil niteliği yok, yeni bağımlılık yok, yeni kural için kaynak taraması ihlal örnekli) ✓. Yerel: derleme `-warnaserror` 0 uyarı, `dotnet format` üç proje temiz, Unit 7096/7096, betik testleri 4/4, Integration TAM (Web 706, Model 1407, Ordering 637, kalan ad alanları 931; hepsi 0 kırmızı; `track_commit_timestamp=on`), `restore --locked-mode` 0. Dal CI'ı (run 42999862098) bakışta Integration hariç sekiz iş yeşil, Integration koşuyordu. `12` task satırları (KP-12 VAL-A039…A042, KP-71 VAL-A340…A350, VAL-A785) tarayıcı gözlemi ya da ödeme ekranı uç kanıtı ister → BEKLEMEDE kaldı; `12_matris_denetle.py --kapsam 1-77` Hata 0. |
| Bulgu sayısı (tur 1) | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (tur 1) | 2 — **K1** (D-1214'e forward): KK9 örnek sayfası limit reddini elle kurar (`ModelState.AddModelError` + 422); gerçek yol `FormResults.AttemptLimitExceeded` (429, red kaydı) bu testte sınanmıyor — o yolun kendi testi (`AttemptLimitPatternTests`) var. **K2** (bu PR'da kapatıldı): ilk tam Web koşusunda 3 kırmızı (makinede eşzamanlı dört Integration koşusu vardı), aynı ağaçta yeniden koşu 706/706 ve `FormComponentTests` tek başına 18/18; kod kaynaklı değil. |
| Yapım raporu karşılaştırması (tur 1) | Kabul kriterlerinde tam uyum (1–10 ✓); öz denetim tablosu var, maddeler kanıtlı. Uyuşmazlık yok; yapımcı Integration'ı yalnız Web ad alanında koşmuştu, doğrulamada tam koşu temiz. Not: doğrulayıcı bu raporu adım 4'te dal diff'ini okurken (`git diff … -- Docs/`) erken gördü; kriter kanıtları rapordan değil kendi koşu ve okumalarından üretildi. |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T543-form-alani-adres-iban`
- **Commit:** `01411663` (yapım ucu); doğrulama kaydı bu dalda
- **PR:** #539
- **CI:** dal run 42999862098 — doğrulama bakışında Lint, Build, Unit, Contract, Migration, Paketleme, E2E yeşil; Integration koşuyordu (merge öncesi yardımcı denetler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Teslimat adresimle aynı" açıp kapama betiği | D-1179 | Ekran task'ı |
| 2 | İşlenmiş IBAN ipucu ve "Seçiniz" `04`'te yok | D-1180 | `04`'e yazılmalı |
| 3 | Panel adres düzeltmesinde il kısıtı | D-1181 | `04` susuyor; E-37'de karar |
| 4 | Firma iptali onay cümlesi ve yasal uyarı çizimi | D-1182 | Onay penceresini kuran ekran işi |
| 5 | `required` yazılmıyor, `aria-required` var | D-1183 | Toplu hata işareti; sahip isterse tek yerden |

## Notlar

- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum"a dokunulmadı (parça dosyası kuralı)
- **Diğer:** devam turu; ilk işçi bağlam sınırında bölünmüştü
