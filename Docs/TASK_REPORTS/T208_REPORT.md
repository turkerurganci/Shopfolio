# T208 — Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım tamamlandı, doğrulama bekliyor) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Sürüm arayüzü:** `IVersionedRecord` (`Version` + `BumpVersion()`); sürüm taşıyan 13 varlık (Coupon, Product, sekiz kurumsal içerik varlığı, Order, Company, LegalTextVersion) uygular.
- **Alanlar:** `VersionFields` — gizli `Version` (bölüm için `...Version`), `Original...` alanı (`OpenQuantity` dahil); sayfa modelin gönderilen değerini değil işleyicinin seçtiği değeri basar, çakışma sonrası form güncel sürümü taşır.
- **Kayıt varyantı:** `RecordSave.SaveAsync` — izlenen kayıt, formun sürümüyle erken karşılaştırma, `BumpVersion`, tek koşullu güncelleme; eşzamanlı ikinci kayıt `DbUpdateConcurrencyException` → `VersionConflict` (tek yer). İzlenmeyen kayıt `InvalidOperationException`.
- **Sipariş varyantı:** `ShownVersion.Matches`; `ConflictControls.OrderNotice` (K-799), üzerine yazma düğmesi yok.
- **Gösterilen değer:** `ShownValue` (sayı/metin/anahtar karşılaştırması; eksik, boş, sayı olmayan = eşleşme değil).
- **Çakışma sonucu:** `FormResults.Conflict` (409); `ConflictControls.RecordSaveControl` (K-800 mesajı, "Güncel hâli aç" yeni sekme, tek "Yine de kaydet").
- **Sürümsüz kayıt:** `CurrentStateWrite.ApplyAsync` (o anki hâle karşı tek koşullu güncelleme).
- **Taramalar:** `FormVersionSourceScan` (adres, başlık, GET formu, model durumundan sürüm alanı) ve `VersionedSideBulkWriteScan` (D-95 (c): sipariş tarafı ve ürün formu tablolarına toplu yazım).
- **Dokümanlar:** `09 §10.2` madde 2, 3 (D-95 (a) gerekçesi), 4; `09 §25` yeni satır; `CONTEXT.md` Forms satırı; D-95 (c) kapandı.
- Önceki task kayıtları: T207 Commit hücresi ve "Post-merge run'lar" satırı dolduruldu (skill adım 15).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/DataModel/IVersionedRecord.cs`; `Infrastructure/Forms/{VersionFields,ShownValue,RecordSave,ConflictControls,CurrentStateWrite,FormResults}.cs`; 13 varlık dosyası (`Modules/{Catalog,CorporateContent,Ordering,Settings}`).
- Testler: `UnitTests/Infrastructure/{VersionedFormFieldTests,FormResultsTests}.cs`; `UnitTests/Architecture/{FormVersionSourceScan,VersionedSideBulkWriteScan}(.cs,Tests.cs)` + `ViolationSamples/{Forms/VersionCarriage.cs,Forms/VersionFields.cshtml,BulkWrites/VersionedSides.cs}`; `IntegrationTests/Web/VersionedFormPageTests.cs` + `Pages/Panel/Versioned{Product,Stock,Order}.cshtml(.cs)` (yalnız test sunucusunda).
- `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T207_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kayıt formu sürümü gizli alanda `06 §4` sütun adıyla taşır | ✓ | `VersionedFormFieldTests.Version_IsAHiddenFieldNamedVersion_WithTheValue`, `SectionVersion_IsNamedByTheSectionsColumn_...`; `VersionedFormPageTests.RecordForm_CarriesTheVersionTheRecordHas_InAHiddenField_AndInNoAddress_KK1`; `TheRecordsOfTheEditFormsAndTheOrder_AreVersionedRecords` (13 varlık) |
| 2 | Yönetici sipariş işlemi sürüm taşır, müşteri taşımaz | ✓ | `OrderPage_TheAdministratorsFormCarriesTheVersion_TheCustomersCarriesOnlyTheShownOpenQuantity_KK2`; `TheCustomerItemFieldIsTheOpenQuantity_AndNoVersionColumnIsInIt` |
| 3 | `Original` alanı, kaydın sürümüne girmez; onaysız gönderimde de karşılaştırılır; son adım ilk formun değerini taşır (stok/kontenjan T208; öteki değerlerin yanıtı T212 madde 4) | ✓ (stok/kontenjan) | `ShownValue_FirstSend_WithAChangedValue_IsRefused_BeforeTheConfirmation_AndWritesNothing_KK3`, `ShownValue_TheLastStep_CarriesTheFirstFormsValue_AndAChangeInBetweenIsNotOverwritten_KK3`, `ShownValue_MissingEmptyOrNotANumber_IsNotAMatch_...`; `ShownValue.Matches` birim testleri. Öteki değerlerin yanıtı (409 gövdesi, kayıt alanı) T212 madde 4'tür; kriter metni payı ayırır |
| 4 | Karşılaştırma tek koşullu güncelleme | ✓ | `RecordUpdate_WithAStaleVersion_AffectsNoRow_KK4`, `TwoSavesOfTheSameVersion_ThatArriveTogether_OneSavesAndTheOtherIsRefused_KK4`; stok yazımı `ICatalogModule.SaveStockQuantityAsync` (T228) tek koşullu güncelleme; sürümsüz kayıt `CurrentStateWrite` |
| 5 | Sürüm değişmişse yazma yok; 409 + yazılanlar; güncel sürüm; tek düğme "Yine de kaydet"; araya giren değişiklikte uyarı yeniden | ✓ | `RecordPost_WithAnOldVersion_Is409_...OneButton_KK4`, `SaveAnyway_SendsTheFormWithTheCurrentVersion_AndSaves_KK5`, `SaveAnyway_WhenOneMoreChangeWasMadeInBetween_ShowsTheWarningAgain_KK5` |
| 6 | Sipariş varyantında işlem uygulanmaz; güncel hâl 409; üzerine yazma yok | ✓ | `OrderPost_ByTheAdministrator_WithAnOldVersion_IsNotApplied_Is409_ShowsTheCurrentOrder_AndHasNoWayToOverwrite_KK6`; `OrderNotice_IsAMessageOnly_ThereIsNoWayToOverwrite` |
| 7 | Sürüm ve gösterilen değer adreste/başlıkta taşınmaz | ✓ | `NoVersionAndNoShownValue_IsInAnAddressOrAHeader_AndTheRequestsNeedNoConditionalHeader_KK7` (ETag/Last-Modified/sürüm başlığı yok, action'da Version/Original yok, `If-Match`siz çalışır); `FormVersionSourceScanTests.ApplicationSource_CarriesNoVersionNorShownValueInAnAddressOrAHeader` + ihlal örneği `VersionCarriage.cs`/`VersionFields.cshtml` |
| 8 | Çift tıklamayla ikinci yönetici gönderimi çakışır; müşteri kalem iptalinde ikincisi işlenmez | ✓ | `OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8`; `OrderPost_ByTheCustomer_WithAChangedOpenQuantity_IsNotProcessed_AndWithTheShownOneIsProcessed_KK2`. Cayma (`Withdrawal`) işleminin kendisi sonraki task'tadır; `Original...OpenQuantity` alan kalıbı ve karşılaştırma burada |
| 9 | Sürüm taşımayan kayıtta durum değiştiren işlem o anki hâlle denetlenir | ✓ | `RecordsWithoutAVersion_HaveNoVersionColumn_AndTheirOperationWritesOnlyFromTheStateItNeeds_KK9` |

### Öz denetim (bitiş kapısı 2)

`git diff origin/main...HEAD` üzerinde madde madde denetlendi (origin/main `3226fcd`, çakışma yok); testler bu turda koşuldu.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| KK1 form çıktısında sürüm gizli alanı | ✓ | `VersionedFormPageTests.RecordForm_CarriesTheVersion...KK1` |
| KK2 sipariş formunda sürüm var, müşteri formunda yok | ✓ | `OrderPage_TheAdministratorsFormCarriesTheVersion...KK2` |
| KK3 `Original…` ilk formunkini taşır; tutmayan değer yazmaz | ✓ | `ShownValue_TheLastStep_...KK3`, `..._FirstSend_...KK3`, `..._MissingEmptyOrNotANumber...KK3` |
| KK4 eski sürümle güncelleme sıfır satır | ✓ | `RecordUpdate_WithAStaleVersion_AffectsNoRow_KK4` |
| KK5 409 + "Yine de kaydet"; araya giren değişiklikte uyarı yeniden | ✓ | `SaveAnyway_...KK5` (iki test) |
| KK6 sipariş çakışmasında yazma yok, güncel hâl var | ✓ | `OrderPost_ByTheAdministrator_WithAnOldVersion...KK6` |
| KK7 adreste ve başlıkta sürüm yok | ✓ | `NoVersionAndNoShownValue...KK7`; `FormVersionSourceScan` |
| KK8 yönetici çift gönderiminde ikincisi çakışır | ✓ | `OrderPost_ByTheAdministrator_SentTwice...KK8` |
| KK9 kategori güncellemesi sürümsüz çalışır | ✓ | `RecordsWithoutAVersion...KK9` (iki bağlamdan iki ardışık kategori adı yazımı da geçer) |
| `09 §25`: ilgili satırlar | ✓ | Yeni satır §25 (eşzamanlılık satırının altı); `§10.2` madde 2–4 güncel |
| `09 §25`: mimari testler yeşil, ayrı ihlal örneği | ✓ | `VersionCarriage.cs`, `VersionFields.cshtml`, `VersionedSides.cs` (ayrı örnek dosyalar; dosya satırları teste yazılı) |
| `09 §25`: `CONTEXT.md` aynı PR'da | ✓ | `.claude/CONTEXT.md` Forms satırı |
| `12` ilgili kontrol listesi (VAL-A610, VAL-A611, VAL-A744) | ✓ (kısmen sonraki task'lar) | A610/A611 → KK4/KK5 (örnek sayfada); ekranlı VAL kaydı ürün/sipariş ekranlarında (T348 ve sonrası). `12`'de T208 satırı yok |
| Güvenlik taraması | ✓ | Sürüm/değer yalnız gövde alanı (adres, başlık yok); "Güncel hâli aç" bağlantısı `FormResults.IsLocalPath` ile yalnız site yolu (`RecordSaveControl_ToAnythingButASitePath_Throws`); alan değerleri `TagBuilder` ile kodlanır (`Original_EncodesTheShownText`); yeni dış bağımlılık yok |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `VersionedSideBulkWriteScan` istisnası tam yolla dört dosya (`StockCounters`, `GuestCartDisposalPart`, `ImageDerivativeJobHandler`, `ImageDisposalParts`); `AllowedPlaces_AreFilesThatExist`, `Scan_InAnAllowedPlace_IsQuiet_AndAFileOfTheSameNameElsewhereIsNotAnAllowedPlace`. `FormVersionSourceScan`'in istisnası yok |
| Sık FAIL 2: tarama çağrı biçimini kaçırıyor | ✓ | Toplu yazım: `ExecuteUpdate/Delete(Async)`, `CurrentStateWrite.ApplyAsync`, değişkenden geçen çağrı (yan modülde), ham `UPDATE/DELETE FROM/INSERT INTO` (tırnaklı, şemalı, `\"` kaçışlı, çok satırlı, `CommandText`/değişken dahil); sürüm taraması: `asp-route-*`, sorgu çifti, yol parametresi, çok satırlı çağrı, `HeaderNames`, `EntityTagHeaderValue`, `Headers[...]` — her biçim ayrı örnek satırda (`Scan_Of...IsAFinding(string)` kuramları ve örnek dosyalar). Bu turda iki boşluk kapatıldı: kaçışlı tırnaklı tablo adı ve komut nesnesi metni |
| Sık FAIL 3: aynı sayı/etiketi anan yerler | ✓ | Sürüm taşıyan 13 varlık: `TheRecordsOfTheEditFormsAndTheOrder_AreVersionedRecords` modeldeki `Version` özelliğiyle eşler; tarama kümeleri `ScansSetsAndTables_AreTheOnesTheTwoVersionGuardsCover` ile bekçilerin kümesine bağlı; `CONTEXT.md`, `09 §10.2`, §25, status, memory, T207 kaydı güncellendi |
| Sık FAIL 4: hedefi bu task olan backlog kaydı | ✓ | `grep -n T208 Docs/DEFERRED_BACKLOG.md`: yalnız D-95; (c) kapandı (satırda işaretli, hedef sütunu "~~T208 (c)~~ kapandı; T348 (a, b)"), (a)(b) T348'e kalır |
| Sık FAIL 5: iki kural birbirini kilitliyor | ✓ | `TheSamplePagesOfTheVersionedForms_PassTheFormVersionScanAndThePageScan`: gerçek sayfa biçiminde (post form + işleyici adı + `VersionFields`) örnek sayfalar hem `PageSourceScan` hem `FormVersionSourceScan`'dan geçer; `VersionFields.cs` kendisi taranır ve temizdir. `asp-for` yasağı ile "model durumu gönderileni geri basar" çelişmez: yardımcı değeri açıkça yazar |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 1801 / 1801 | `dotnet test tests/Shopfolio.UnitTests` | önce 1628 (T207); +173 (VersionFields, ShownValue, ConflictControls, FormVersionSourceScan, VersionedSideBulkWriteScan, FormResults.Conflict) |
| Integration | ✓ 1381 / 1381 | `dotnet test tests/Shopfolio.IntegrationTests --no-build --filter-namespace …` (parça parça: Web 209, Model 893, Concurrency+Security+Files+Jobs 154, öteki 125) | önce 1360; +21 (`VersionedFormPageTests`), gerçek PostgreSQL 18 |
| Contract | — | — | Bu task'ta sözleşme testi yok |
| Derleme / biçim | ✓ | `dotnet build -warnaserror --no-incremental`; `dotnet format --verify-no-changes` | 0 uyarı, 0 hata; biçim çıkışı 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (sürüm sütunları mevcut; arayüz yalnız koddur)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T208-surum-gosterilen-deger`
- **Commit:** son commit PR'ın başıdır (aşağıdaki PR numarasıyla birlikte)
- **PR:** #NN (PR açılınca doldurulur)
- **CI:** yapım işçisi izlemez; dal CI'ı yardımcı izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | EF Core koşullu güncelleme ve eşzamanlılık belirteci davranışı gerçek PostgreSQL 18'de sınandı (`TwoSavesOfTheSameVersion...`, `RecordUpdate_WithAStaleVersion...`) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Hata kodları `RecordVersionConflict`, `DisplayedValueConflict`, `OrderVersionConflict` (07 §6.2) yalnız belge adı; kayıt T213'te (`ErrorCode`) | T213 | Hata kodu kaydı T213'ün işi; burada ret dönüş değeri (`RecordSaveResult.VersionConflict`, `StockQuantityWriteResult.ShownValueChanged`, `SampleOutcome.Refused`) |
| 2 | Kabul kriteri 3'ün öteki değerler payı (metin, anahtar, elle sıra: 409 gövdesi ve yanıtı) | T212 madde 4 | Kriter metni payı T212'ye verir; burada `ShownValue.Matches` ve `Original` alan kalıbı hazır |
| 3 | D-95 (a) kilit denetiminin kendisi ve (b) alt kaydın kökünün kilit altında yüklenmesi | D-95 → T348 | Sipariş tarafına ilk gerçek yazımı kuran task; (a)'nın sıra gerekçesi `09 §10.2` madde 3'te yazıldı |
| 4 | Gerçek ürün, kupon, sipariş vb. ekranlarının sürüm alanlarını kullanması | ilgili ekran task'ları (T348 ve sonrası) | Bu task altyapı ve test sunucusunda örnek sayfalar verir; gerçek ekran yok |
