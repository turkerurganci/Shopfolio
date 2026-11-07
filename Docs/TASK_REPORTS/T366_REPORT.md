# T366 — Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin

**Faz:** F3 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İki üretilen metin:** Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi ürünle gelen sabit metin dosyalarıdır (`Settings/Templates/*.tr.txt`, derlemeye kültürsüz gömülür); sürüm kimliği, satır sonları normalleştirilmiş içeriğin SHA-256 özetinin ilk on iki hanesidir (`LegalTextTemplates`, elle artan sürüm sabiti yok).
- **Üretici:** `LegalTextGenerator.RegenerateAsync` ayar satırı kilidi altında metni üretir, son yayımlanan satırla metin ve şablon kimliği bakımından karşılaştırır, yalnız fark varsa yeni sürüm ekler (değişmeyen değer sürüm yazmaz). Kimlik ve iade adresi kaydı aynı işlemde çağırır; açılışta `GeneratedLegalTextsStartup` ilk sürümü ve şablon değişimini yazar (sistem olayı, iz satırı yok, günlük satırı yok).
- **Doldurma:** `PlaceholderFiller` tek işlevdir (belirteç değeri içindeki `{{ }}` genişletilmez; sipariş belirteçleri yerinde kalır; kaçış gösterim kanalındadır).
- **İki firma metni:** aydınlatma ve çerez politikası taslak satırıdır; `GetLegalTextDraftAsync`, `SaveLegalTextDraftAsync` (boşaltma ve dolu barındırma konumunu boşaltma reddedilir, kayıt varyantı), `PublishLegalTextAsync` (kimlik yer tutucuları HTML kodlanarak donar, aydınlatma barındırma konumu olmadan yayımlanmaz, tek iz satırı).
- **Sürüm okuma:** `GetCurrentLegalTextVersionsAsync`, `AreShownLegalTextVersionsCurrentAsync` (onay adımı), `GetLegalTextVersionAsync` (eski sürüm okunur), `GetLatestLegalTextVersionAsync`; panel listesi `GetLegalTextsAsync` eksik parçayı adıyla verir (`LegalTextMissingPart`).
- **Satış kapısı bağı:** `PrivacyNoticeCondition` bağlandı, varsayılan kayıt kaldırıldı (D-198 payı).
- **Bağlantılar:** `LegalTextLinks` aydınlatma bağlantısı ekranlarının listesini, çerez bağlantısını (altbilgi) ve hatırlatma satırını taşır; onay kutusu, "gördü" kaydı ve ek yoktur.
- **Mimari tarama:** `LegalTextSourceScan` (rıza kaydı, ek, yasal metin satırı oluşturma yalnız sahibinde) ve ihlalli örnek; `LinkKeySourceScan` istisna listesine tek dosya (`LegalTextTemplates.cs`, `09 §11.4` madde 5 içerik özeti).
- Önceki task'ların merge kaydı: T331 ve T312 raporlarının "Post-merge run'lar" satırı, status Commit hücreleri.
- Yapım sırasında bulunup düzeltilenler: `.tr.txt` uzantısı MSBuild'de kültür eki sayılıp uydu derlemeye gidiyordu (`WithCulture="false"`); üretici kendi `SaveChanges`'ini çağırıyordu (ayar satırının tek yazıcısı kuralı, `09 §10.3` madde 4 — artık çağıranın birimine ekler); açılış senkronu ikinci bir `StartupDatabaseChecked` uyarısı yazıyordu (kaldırıldı, veritabanı denetimi zaten uyarır); mevcut testlerin elle yazdığı form/sözleşme sürüm numaraları üretilen ilk sürümle çakışıyordu.
- Backlog: D-219, D-220, D-221, D-222 yeni; D-165 (b) kapandı; D-167 ileri atıldı (T356); D-198 T366 payı kapandı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Settings/`: `LegalTextGenerator.cs`, `LegalTextTemplates.cs`, `LegalTexts.cs`, `GeneratedLegalTextsStartup.cs`, `SettingsModule.cs`, `UnboundSalesGateConditions.cs`, `Contracts/` (`LegalTextRecords.cs`, `LegalTextMissingPart.cs`, `LegalLinkScreen.cs`, `ISettingsModule.cs`, `ISalesGateConditions.cs`), `Templates/*.tr.txt`
- `src/Shopfolio.Web/Infrastructure/PlaceholderFiller.cs`, `src/Shopfolio.Web/Shopfolio.Web.csproj`
- `tests/Shopfolio.UnitTests/`: `LegalTextTests/*`, `Architecture/LegalTextSourceScan*.cs`, `Architecture/ViolationSamples/LegalText/*`, `Architecture/LinkKeySourceScan*.cs`, `Architecture/SalesGateSourceScan.cs`, `InstallationSettings/SalesGateRulesTests.cs`
- `tests/Shopfolio.IntegrationTests/Model/LegalTextTests.cs` (yeni), `Model/SalesGateTests.cs`, `Model/CompanyIdentityTests.cs`, `Web/OperationPatternTests.cs`
- `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, T331 ve T312 raporları

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dört metin sürümlü, ilk ikisi üretilir, son ikisi taslak | ✓ | `LegalTextTests.TheFourTexts_AreFourSeparateRecords_...`; `LegalTextTemplateTests` (`_AreExactlyTheFormAndTheContract_AndOnlyTheyHaveATemplate`) |
| 2 | Son iki metin boşaltılamaz; aydınlatma kapsamı; barındırma dolmadan tamamlanmaz | ✓ | `LegalTextTests.AnEmptyText_IsRefused...`, `ThePrivacyNotice_IsNotPublishedWithoutItsHostingLocation_...`, `TheList_NamesWhatIsMissing_ByPart`; `LegalTextTemplateTests` (`_WritesTheFiveRecipientGroups_TheLegalBases_AndTheApplicationRoutes`: aydınlatma içeriği; taslakta üç içerik `StartupDataTests.LegalTexts_Are...`) |
| 3 | Sürüm kendiliğinden artar; ayar ya da şablon değişince; şablonunki izde yok | ✓ | `ASaveThatChangesNoValue_WritesNoVersion_...`, `ATemplateChange_WritesANewVersionAsASystemEvent_WithNoTrailRow`, `TheFirstVersionsOfTheGeneratedTexts_WriteNoTrailRow`, `ASecondSynchronization_WritesNothing`, `ConcurrentIdentityAndReturnAddressSaves_...` |
| 4 | Onay adımında sürüm artarsa sipariş oluşmaz; yeniden onay ve bildirim yok | ✓ (modül düzeyi; adım T356) | `ANewVersion_MakesTheShownIdsStale_ButTheOldVersionStaysReadable_AndNobodyIsNotified` (`AreShownLegalTextVersionsCurrentAsync` false, `dispatch_records` boş) |
| 5 | Aydınlatma bağlantısı yeni veri alanı açan her ekranda, kutu/gördü yok; çerez altbilgide | ✓ (liste modül düzeyinde; ekranlar D-222) | `PlaceholderFillerAndLinksTests` (`_IsOnEveryScreenThatOpensAPersonalDataField_AndOnTheFooter`, `_IsOnTheFooterOnly`); `LegalTextTests.NoTableOrColumn_RecordsAConsentOrASeenMark` |
| 6 | Onay e-postasında donan sürümle tam metin, ek yok | ~ modül yüzü ✓, gövde T306 / yeniden gönderim T385 (plan bölümü gereği, D-222) | `GetLegalTextVersionAsync` eski sürümü verir; `LegalTextTemplateTests` (`_CarriesTheReturnTermsOfTheForm_AndNamesNoFile`) |
| 7 | Uyuşmazlık başvuru bilgisi var, parasal sınır rakamla yok | ✓ | `LegalTextTemplateTests` (`_WritesTheDisputeBodies_AndNoMoneyLimit`) |
| 8 | Çelişen serbest metin tespit edilmez; hatırlatma satırı | ✓ | `PlaceholderFillerAndLinksTests` (`_NamesWhoIsResponsible_AndWhichTextBinds_AndTheLinksHoldNoDetection`), `LegalTextLinks.Reminder` |
| 9 | Dört metin tamamlanmadan satış açılmaz; eksik adıyla | ✓ | `LegalTextTests.TheGate_HasThePrivacyNoticeOpen_OnlyAfter...`, `TheList_NamesWhatIsMissing_ByPart`; `SalesGateTests` (22) |
| 10 | Açık rıza yönetimi yok | ✓ | `NoTableOrColumn_RecordsAConsentOrASeenMark`; `LegalTextSourceScanTests` (`_HasNoConsentRecordNoAttachment_AndMakesLegalTextRowsOnlyInTheOwners`) |

> Madde 4, 5 ve 6'nın ekran ve e-posta yüzleri planda başka task'lara (T356, T306, T385, T540, T507) aittir; T366'nın payı modül kuralı ve verisidir. Kısmen kalan kalem D-222'de.

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–10 | ✓ | yukarıdaki tablo (6 ve ekran yüzleri D-222) |
| KK1: dört metin ayrı kayıt | ✓ | `LegalTextTests.TheFourTexts_AreFourSeparateRecords_...` |
| KK2: boşaltma reddedilir; bölüm eksikse tamam değil; taslakta üç içerik var | ✓ | `AnEmptyText_IsRefused...`, `TheList_NamesWhatIsMissing_ByPart`; `LegalTextTemplateTests` aydınlatma içeriği testi (alıcı grupları, hukuki sebep, başvuru yolu) |
| KK3: değişmeyen ayar sürüm yazmaz; şablonunki, ilk sürüm dahil, izde yok | ✓ | `ASaveThatChangesNoValue_...`, `ATemplateChange_...WithNoTrailRow`, `TheFirstVersionsOfTheGeneratedTexts_WriteNoTrailRow` |
| KK4: sürüm artışı siparişi durdurur; yeniden onay ve bildirim yok | ✓ (modül düzeyi) | `ANewVersion_MakesTheShownIdsStale_...AndNobodyIsNotified` |
| KK5: bağlantı ekranlarda ve altbilgide; kutu ve gördü kaydı yok | ✓ (liste) | `PlaceholderFillerAndLinksTests`, `NoTableOrColumn_RecordsAConsentOrASeenMark` |
| KK6: dosya ve PDF yok; gövde T306 KK3'te | ✓ | `LegalTextTemplateTests` (`_NamesNoFile`); `LegalTextSourceScanTests` ek taraması |
| KK7: mercii bilgisi var, rakam yok | ✓ | `LegalTextTemplateTests` (`_WritesTheDisputeBodies_AndNoMoneyLimit`) |
| KK8: çelişki denetimi yok; hatırlatma satırı var | ✓ | `PlaceholderFillerAndLinksTests` (`_NamesWhoIsResponsible_...`) |
| KK9: eksik metin satışı kapatır, adı görünür | ✓ | `TheGate_HasThePrivacyNoticeOpen_OnlyAfter...`; yayın yokken `UnmetConditions` PrivacyNotice adını taşır |
| KK10: rıza ekranı ve kaydı yok | ✓ | `NoTableOrColumn_RecordsAConsentOrASeenMark` |
| `09 §25` ilgili satırlar: tarama için ihlalli örnek, tek kural kopyası, karar kaydı | ✓ | `ViolationSamples/LegalText/ConsentAttachmentAndRowCreation.cs` (satırlar testte sabit); `LinkKeySourceScanTests` istisna listesi iki dosya; belge kuralı değişmedi (karar kaydı gerekmedi) |
| Sık FAIL: istisna geniş | ✓ | `LinkKeySourceScan` istisnası tek dosya ve tek neden; taramanın örnek testi komşu dosyanın bulunduğunu doğrular |
| Sık FAIL: tarama çağrı biçimini kaçırır | ✓ | `LegalTextSourceScanTests` (`_IsNotThisRulesBusiness_AndASplitCallIsFound`: bölünmüş çağrı bulunur) |
| Sık FAIL: 06 "Varsayılan" / FK dizini | ✓ yok | şema değişmedi (migration yok) |
| Sık FAIL: aynı sayıyı anan öteki yerler | ✓ | `LinkKeySourceScanTests` istisna sayısı, `Aktif kalem` 102 → 106, status satırı, T331/T312 Post-merge |
| Sık FAIL: belge kuralını değiştiren 09 metni karar kaydısız | ✓ yok | 09 değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build` (Architecture ayrı) | Architecture 846, öteki 2597 geçti |
| Integration (yeni) | ✓ | `--filter-class "*LegalTextTests"` | 13/13 |
| Integration (dokunulan) | ✓ | `SalesGateTests` 22, `CompanyIdentityTests` 24, `SettingsTablesTests` 28, `StartupDatabaseCheckTests` 18, `OperationPatternTests` 13 | hepsi geçti |
| Integration (ad alanı) | ✓ | `--filter-namespace` Model 974, Web 492 (ilk koşuda 7 hata düzeltildi), Files 81, Logging 30 | hepsi geçti |
| Consistency | Aşağıda (docs commit'inden sonra) | `--filter-trait "Consistency=true"` | bkz. Notlar |
| Lint | ✓ | `dotnet format ... --verify-no-changes` (Web, UnitTests, IntegrationTests); derleme `-warnaserror` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-07; head `24163d5` + `origin/main` birleştirmesi) |
| Bulgu sayısı | 1 (S1, KRİTİK) |
| Düzeltme gerekli mi | Evet — düzeltme turu |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-240) |

### Doğrulama tur 1 — validator kanıtları

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `scripts/git-hooks`; ana dal CI son 3 run 42368786653, 42368786665, 42366197659 `success`; memory'de T366 satırı var. Dal CI 42368746924 (`24163d5`) `success`.

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Dört metin sürümlü, ayrı kayıt; ilk ikisi üretilir, son ikisi taslak | ✓ | `LegalTextTemplates.GeneratedTypes`, `LegalTexts.RequireFirmEdited`; `LegalTextTests.TheFourTexts_...` geçti |
| 2 | Son iki metin boşaltılamaz; aydınlatma taslağı kapsamı; barındırma dolmadan tamamlanmaz | ✗ | Boşaltma reddi ve taslak içeriği ✓; ama `LegalTexts.PublishAsync` firma kimliği eksikken aydınlatmayı yayımlar (yalnız metin ve barındırma denetlenir), kimlik yer tutucuları `PlaceholderFiller.Fill(..., requireAll: false)` ile boş değerle donar. Geçici sonda testi (koşudan sonra geri alındı): kimliksiz yayın → `"<p>Sorumlu: [] KEP: []</p>"`; kimlik kaydedilince `GetLegalTextsAsync` aydınlatmayı `IsComplete` der, kapının `PrivacyNotice` koşulu açılır, son yayımlanan metin hâlâ boş unvan/KEP taşır. `06 §4.56` ve `04` 9.19.7 "aydınlatma metni tamamlanmadan yayına alınmaz", `04` 9.19.11 "kimlik değişirse yer tutucu yeni değeri gösterir", `02 §3.1.5` koşul 4 |
| 3 | Sürüm kendiliğinden artar; şablonunki ize girmez | ✓ | `LegalTextGenerator` kilit altında üretir, metin + şablon kimliği karşılaştırması; ilgili beş test geçti; ayar sürümünün iz satırı D-220 |
| 4 | Sürüm artarsa sipariş oluşmaz; yeniden onay ve bildirim yok | ✓ (modül düzeyi; onay adımı T356) | `AreShownLegalTextVersionsCurrentAsync`; `ANewVersion_MakesTheShownIdsStale_...` geçti |
| 5 | Aydınlatma bağlantısı ekranları, çerez altbilgide; kutu/gördü kaydı yok | ✓ (liste; ekranlar D-222) | `LegalTextLinks`, `LegalLinkScreen.All` `02 §3.33.5` ile birebir; şema taraması ve `LegalTextSourceScan` |
| 6 | Onay e-postasında tam metin, ek yok | ✓ (modül yüzü; gövde T306, yeniden gönderim T385) | şablon "tam metin" yazar, ek/PDF taraması |
| 7 | Uyuşmazlık bilgisi var, rakam yok | ✓ | iki şablonun 6./9. maddesi; `_WritesTheDisputeBodies_AndNoMoneyLimit` |
| 8 | Çelişki denetimi yok; hatırlatma satırı | ✓ | `LegalTextLinks.Reminder` |
| 9 | Dört metin tamamlanmadan satış açılmaz; eksik adıyla | ✓ (kriter 2'nin bulgusu kapının "tamam" tanımını da etkiler) | `PrivacyNoticeCondition` bağlı; `LegalTextMissingPart` |
| 10 | Açık rıza yönetimi yok | ✓ | şema ve kaynak taraması |

**Testler (yerel):** build `-warnaserror` temiz; `dotnet format --verify-no-changes` üç projede 0; Consistency 209/209; Unit 3234/3234; betik testleri 4 dosya; Integration parçalı (`postgres:18`, konteyner `shopfolio-t366-val-pg`): LegalText + SalesGate + CompanyIdentity 59/59, SettingsTables + StartupDatabaseCheck + OperationPattern 59/59; `dotnet restore --locked-mode` 0.

**Bulgu 1 (S1, KRİTİK, kriter 2):** aydınlatma metni firma kimliği eksikken yayımlanabiliyor, kimlik yer tutucuları boş donuyor; kimlik dolunca liste ve satış kapısı metni "tamam" sayıyor, vitrindeki ve siparişe donan aydınlatma veri sorumlusunun unvanı/iletişimi boş. Kimlik sonradan değişince yer tutucu yeni değeri göstermiyor (yapımın ⚠ (b) kararı `04` 9.19.11 ile çelişir). Düzeltme önerisi: (1) `PublishAsync` kimliğin eksik alanı varken `CompanyIdentity` reddiyle yayımlamaz; (2) yer tutucunun güncel kimliği göstermesi — ya gösterimde doldurma (yayımlanan metin belirteci taşır, okuma `PlaceholderFiller` + HTML kodlamayla doldurur) ya da kimlik kaydında aydınlatmanın yeni sürümü; seçim belge kuralıyla çelişirse karar kaydı. Test: kimliksiz yayın reddi, kimlik değişince görünen değerin değişmesi.

**K1 → D-240:** `{{ReturnAddress}}` `09 §9.3` madde 2'nin kapalı kümesinin (`Order`/`OrderItem` alanları) dışında; davranış doğru, kural metni saymıyor.

**`12` satırları:** KP-14, KP-25, KP-35, KP-61, KP-73 satırları (`VAL-A047`…`VAL-A052`, `VAL-A986`, `VAL-B020`, `VAL-A090`, `VAL-C026`, `VAL-C027`, `VAL-A125`…`VAL-A130`, `VAL-A299`, `VAL-A300`, `VAL-D034`, `VAL-A363`…`VAL-A367`) tarayıcı/uç kanıtı ister; `Durum` `BEKLEMEDE` kaldı.

**Yapım raporu karşılaştırması:** kriter 2 yapımda ✓, validator ✗ (kimliksiz yayın senaryosu öz denetimde yok); öteki kriterler uyumlu. Öz denetim tablosu var ve maddeleri kanıtlı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (tablo ve sütunlar T1xx'te vardı)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Var — iki gömülü şablon kaynağı (`Shopfolio.Web.csproj`, `WithCulture="false"`)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T366-yasal-metinler`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #428
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (şablon metinleri yalnız ürünün kendi dosyaları; yeni paket yok) | `Shopfolio.Web.csproj` farkı yalnız `EmbeddedResource` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Taslak metninin temizlenmesi panel sayfasının çağrıdan önceki adımı | D-219 | Panel sayfası T507 |
| 2 | Ayar kaydının doğurduğu sürümün iz satırı | D-220 | İz yazıcısı T380; kayıt bağı D-165 (a) |
| 3 | Kargo ücreti ve süresi bölümlerinin üreticiyi çağırması | D-221 | Ayar bölümleri T350 |
| 4 | Kalem tablosu, e-posta gövdesi, yeniden gönderim, bağlantı ekranları | D-222 | T356, T306, T385, T540, T507 |
| 5 | Belge ad alanının kaçış taramasına girmesi | D-167 (T356'ya ileri) | `IOrderDocuments` yazılırken |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; dal önceki işçinin `927dda1` commit'inden devam etti, `origin/main` ile birleştirildi, çakışma yok)
- **Ana dal CI startup (Adım 0):** son run'lar: CI 42366197638, Publish 42366197659 (T312 #426) success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Ürün sahibine:** (a) `ReturnAddress` belirteci `09 §9.3` kapalı kümesine (Order/OrderItem alanları) bir ektir: iade adresi sipariş alanı olmadığı için sürüm anında doldurulan şirket değeridir; (b) kimlik yer tutucuları yayında doldurulur (donmuş anlık görüntü), kimlik sonradan değişirse firma yeniden yayımlar; (c) iki şablonun hukuki metni ürün sahibinin (ve hukuk danışmanının) gözden geçirmesini bekler; (d) kargo ücreti ve süresi sipariş belirteci olarak kaldı (D-221); (e) iki üretilen metnin ilk sürümü açılış senkronuna bağlıdır, satış kapısı yalnız aydınlatma ve çerez koşullarını adıyla taşır; (f) `LegalTextMissingPart` ve `LegalLinkScreen` enum değil kapalı değer sınıfıdır (modül sözleşme enum'ları `06` veri modelinde tanımlı olmak zorunda; bunlar saklanan değer değil).
